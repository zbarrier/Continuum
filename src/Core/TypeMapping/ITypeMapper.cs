// Portions of this file are adapted from Eventuous (https://github.com/Eventuous/eventuous).
// Copyright (C) Eventuous HQ OÜ.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.

using System.Diagnostics.CodeAnalysis;

namespace Continuum.TypeMapping;

/// <summary>
/// Maps CLR types to stable type names (and back) so persisted or transmitted payloads do not depend on CLR type names.
/// </summary>
public interface ITypeMapper
{
    /// <summary>Gets the mappings from CLR type to type name.</summary>
    IReadOnlyDictionary<Type, string> TypeMap { get; }

    /// <summary>Gets the mappings from type name to CLR type.</summary>
    IReadOnlyDictionary<string, Type> TypeNameMap { get; }

    /// <summary>Gets a value indicating whether <see cref="Freeze"/> has been called.</summary>
    bool IsFrozen { get; }

    /// <summary>
    /// Marks registration as complete. Lookups switch to read-optimized frozen dictionaries, and subsequent
    /// calls to <see cref="AddType"/> or <see cref="RemoveType{T}"/> throw <see cref="InvalidOperationException"/>.
    /// Calling it more than once is a no-op.
    /// </summary>
    void Freeze();

    /// <summary>Gets the type name mapped to <typeparamref name="T"/>.</summary>
    /// <typeparam name="T">The mapped type.</typeparam>
    /// <returns>The mapped type name.</returns>
    /// <exception cref="TypeMappingNotFoundException"><typeparamref name="T"/> is not mapped.</exception>
    string GetTypeName<T>();

    /// <summary>Gets the type name mapped to <paramref name="type"/>.</summary>
    /// <param name="type">The mapped type.</param>
    /// <returns>The mapped type name.</returns>
    /// <exception cref="TypeMappingNotFoundException"><paramref name="type"/> is not mapped.</exception>
    string GetTypeName(Type type);

    /// <summary>Tries to get the type name mapped to <paramref name="type"/>.</summary>
    /// <param name="type">The mapped type.</param>
    /// <param name="typeName">When this method returns <see langword="true"/>, the mapped type name.</param>
    /// <returns><see langword="true"/> if <paramref name="type"/> is mapped; otherwise <see langword="false"/>.</returns>
    bool TryGetTypeName(Type type, [NotNullWhen(true)] out string? typeName);

    /// <summary>Gets the CLR type mapped to <paramref name="typeName"/>.</summary>
    /// <param name="typeName">The mapped type name.</param>
    /// <returns>The mapped CLR type.</returns>
    /// <exception cref="TypeMappingNotFoundException"><paramref name="typeName"/> is not mapped.</exception>
    Type GetType(string typeName);

    /// <summary>Tries to get the CLR type mapped to <paramref name="typeName"/>.</summary>
    /// <param name="typeName">The mapped type name.</param>
    /// <param name="type">When this method returns <see langword="true"/>, the mapped CLR type.</param>
    /// <returns><see langword="true"/> if <paramref name="typeName"/> is mapped; otherwise <see langword="false"/>.</returns>
    bool TryGetType(string typeName, [NotNullWhen(true)] out Type? type);

    /// <summary>
    /// Adds a mapping. Adding an existing mapping again is a no-op. Intended to be called during startup.
    /// </summary>
    /// <param name="type">The CLR type.</param>
    /// <param name="typeName">The stable type name.</param>
    /// <exception cref="ArgumentException"><paramref name="type"/> is already mapped to a different name, or <paramref name="typeName"/> is already mapped to a different type.</exception>
    /// <exception cref="InvalidOperationException">The mapper is frozen.</exception>
    void AddType(Type type, string typeName);

    /// <summary>Removes the mapping for <typeparamref name="T"/>.</summary>
    /// <typeparam name="T">The mapped type.</typeparam>
    /// <exception cref="TypeMappingNotFoundException"><typeparamref name="T"/> is not mapped.</exception>
    /// <exception cref="InvalidOperationException">The mapper is frozen.</exception>
    void RemoveType<T>();

    /// <summary>Determines whether <typeparamref name="T"/> is mapped.</summary>
    /// <typeparam name="T">The type to check.</typeparam>
    /// <returns><see langword="true"/> if <typeparamref name="T"/> is mapped; otherwise <see langword="false"/>.</returns>
    bool IsTypeRegistered<T>();

    /// <summary>Verifies that every type in <paramref name="types"/> is mapped.</summary>
    /// <param name="types">The types to check.</param>
    /// <exception cref="TypeMappingNotFoundException">A type is not mapped.</exception>
    void EnsureTypesRegistered(IEnumerable<Type> types);

    /// <summary>
    /// Adds every type recorded in <see cref="TypeMapRegistry"/> whose kind is included in <paramref name="typeMapKinds"/>.
    /// </summary>
    /// <param name="typeMapKinds">The kinds to add. May be a combination of flags.</param>
    void RegisterKnownAttributeTypes(TypeMapKinds typeMapKinds);
}
