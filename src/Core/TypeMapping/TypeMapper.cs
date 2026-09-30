// Portions of this file are adapted from Eventuous (https://github.com/Eventuous/eventuous).
// Copyright (C) Eventuous HQ OÜ.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.

using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace Continuum.TypeMapping;

/// <summary>
/// Base <see cref="ITypeMapper"/> implementation. Writes are synchronized; reads are lock-free and assume
/// mappings are added during startup, before concurrent lookups begin. Call <see cref="Freeze"/> once startup
/// registration is complete to switch lookups to frozen dictionaries and reject further changes.
/// </summary>
public abstract class TypeMapper : ITypeMapper
{
    private readonly object _lock = new();

    private readonly Dictionary<Type, string> _typeMap = new();
    private readonly Dictionary<string, Type> _typeNameMap = new();

    private FrozenDictionary<Type, string>? _frozenTypeMap;
    private FrozenDictionary<string, Type>? _frozenTypeNameMap;

    /// <inheritdoc/>
    public IReadOnlyDictionary<Type, string> TypeMap => (IReadOnlyDictionary<Type, string>?)_frozenTypeMap ?? _typeMap;
    /// <inheritdoc/>
    public IReadOnlyDictionary<string, Type> TypeNameMap => (IReadOnlyDictionary<string, Type>?)_frozenTypeNameMap ?? _typeNameMap;

    /// <inheritdoc/>
    public bool IsFrozen => Volatile.Read(ref _frozenTypeMap) is not null;

    /// <inheritdoc/>
    public string GetTypeName<T>() => GetTypeName(typeof(T));
    /// <inheritdoc/>
    public string GetTypeName(Type type)
    {
        if (TryGetTypeName(type, out var typeName))
        {
            return typeName;
        }
        throw new TypeMappingNotFoundException(type);
    }
    /// <inheritdoc/>
    public bool TryGetTypeName(Type type, [NotNullWhen(true)] out string? typeName)
    {
        var frozen = Volatile.Read(ref _frozenTypeMap);
        return frozen is not null ? frozen.TryGetValue(type, out typeName) : _typeMap.TryGetValue(type, out typeName);
    }

    /// <inheritdoc/>
    public Type GetType(string typeName)
    {
        if (TryGetType(typeName, out var type))
        {
            return type;
        }
        throw new TypeMappingNotFoundException(typeName);
    }
    /// <inheritdoc/>
    public bool TryGetType(string typeName, [NotNullWhen(true)] out Type? type)
    {
        var frozen = Volatile.Read(ref _frozenTypeNameMap);
        return frozen is not null ? frozen.TryGetValue(typeName, out type) : _typeNameMap.TryGetValue(typeName, out type);
    }

    /// <inheritdoc/>
    public void AddType(Type type, string typeName)
    {
        lock (_lock)
        {
            ThrowIfFrozen();
            if (_typeMap.TryGetValue(type, out var existingTypeName))
            {
                if (existingTypeName != typeName)
                {
                    throw new ArgumentException($"Type '{type.FullName}' has already been added with a different name '{existingTypeName}'.", nameof(typeName));
                }
                return; // Already added and has same typeName, just return, no harm, no foul.
            }
            if (_typeNameMap.TryGetValue(typeName, out var existingType))
            {
                throw new ArgumentException($"Type name '{typeName}' is already mapped to type '{existingType.FullName}'; cannot also map it to '{type.FullName}'.", nameof(typeName));
            }
            _typeMap.Add(type, typeName);
            _typeNameMap.Add(typeName, type);
        }
    }

    /// <inheritdoc/>
    public void RemoveType<T>()
    {
        lock (_lock)
        {
            ThrowIfFrozen();
            var name = GetTypeName<T>();
            _typeMap.Remove(typeof(T));
            _typeNameMap.Remove(name);
        }
    }

    /// <inheritdoc/>
    public void Freeze()
    {
        lock (_lock)
        {
            if (_frozenTypeMap is not null)
            {
                return;
            }
            Volatile.Write(ref _frozenTypeNameMap, _typeNameMap.ToFrozenDictionary());
            Volatile.Write(ref _frozenTypeMap, _typeMap.ToFrozenDictionary());
        }
    }

    /// <inheritdoc/>
    public bool IsTypeRegistered<T>() => TryGetTypeName(typeof(T), out _);

    /// <inheritdoc/>
    public void EnsureTypesRegistered(IEnumerable<Type> types)
    {
        foreach (var type in types)
        {
            _ = GetTypeName(type);
        }
    }

    /// <inheritdoc/>
    public void RegisterKnownAttributeTypes(TypeMapKinds typeMapKinds)
    {
        foreach (var registration in TypeMapRegistry.GetRegistrations(typeMapKinds))
        {
            AddType(registration.Type, registration.TypeIdentifier);
        }
    }

    private void ThrowIfFrozen()
    {
        if (_frozenTypeMap is not null)
        {
            throw new InvalidOperationException("The type mapper is frozen and can no longer be modified.");
        }
    }
}
