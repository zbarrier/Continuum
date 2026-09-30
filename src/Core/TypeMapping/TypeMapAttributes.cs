// Portions of this file are adapted from Eventuous (https://github.com/Eventuous/eventuous).
// Copyright (C) Eventuous HQ OÜ.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.

using System.ComponentModel;

namespace Continuum.TypeMapping;

// Max value is 31
/// <summary>
/// Categories of mapped types. Values are flags so type mappers can be created for combinations of kinds.
/// </summary>
[Flags]
public enum TypeMapKinds
{
    /// <summary>A command.</summary>
    Command = 1,
    /// <summary>A domain event persisted to an event stream.</summary>
    DomainEvent = 2,
    /// <summary>An integration event published to other services.</summary>
    IntegrationEvent = 4,
    /// <summary>A persisted snapshot of event-sourced grain state.</summary>
    Snapshot = 8,
    /// <summary>Metadata persisted alongside commands, events, or snapshots.</summary>
    Metadata = 16,
}

/// <summary>
/// Base class for attributes that map a class to a stable type identifier of a single <see cref="TypeMapKinds"/>.
/// Decorated types are registered at compile time by the Continuum.Core source generator.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public abstract class TypeMapAttribute : Attribute
{
    /// <summary>Initializes a new instance.</summary>
    /// <param name="typeIdentifier">The stable identifier the type is persisted under. Must not change once data exists.</param>
    /// <param name="typeKinds">The single kind of the decorated type.</param>
    /// <exception cref="ArgumentException"><paramref name="typeIdentifier"/> is blank, or <paramref name="typeKinds"/> is a combination of kinds.</exception>
    public TypeMapAttribute(string typeIdentifier, TypeMapKinds typeKinds)
    {
        if (string.IsNullOrWhiteSpace(typeIdentifier))
        {
            throw new ArgumentException($"'{nameof(typeIdentifier)}' cannot be null or whitespace.", nameof(typeIdentifier));
        }

        int typeKindsValue = (int)typeKinds;
        bool isCombinationOfKinds = (typeKindsValue & (typeKindsValue - 1)) != 0;
        if (isCombinationOfKinds)
        {
            throw new ArgumentException($"'{nameof(typeKinds)}' cannot be a combination of kinds.", nameof(typeKinds));
        }

        TypeIdentifier = typeIdentifier;
        TypeKind = typeKinds;
    }

    /// <summary>Gets the stable identifier the type is persisted under.</summary>
    public string TypeIdentifier { get; }
    /// <summary>Gets the kind of the decorated type.</summary>
    public TypeMapKinds TypeKind { get; }
}

/// <summary>Maps a command class to a stable type identifier.</summary>
/// <param name="typeIdentifier">The stable identifier the type is persisted under.</param>
[AttributeUsage(AttributeTargets.Class)]
public sealed class CommandTypeAttribute(string typeIdentifier) : TypeMapAttribute(typeIdentifier, TypeMapKinds.Command) { }

/// <summary>Maps a domain event class to a stable type identifier.</summary>
/// <param name="typeIdentifier">The stable identifier the type is persisted under.</param>
[AttributeUsage(AttributeTargets.Class)]
public sealed class DomainEventTypeAttribute(string typeIdentifier) : TypeMapAttribute(typeIdentifier, TypeMapKinds.DomainEvent) { }

/// <summary>Maps an integration event class to a stable type identifier.</summary>
/// <param name="typeIdentifier">The stable identifier the type is persisted under.</param>
[AttributeUsage(AttributeTargets.Class)]
public sealed class IntegrationEventTypeAttribute(string typeIdentifier) : TypeMapAttribute(typeIdentifier, TypeMapKinds.IntegrationEvent) { }

/// <summary>
/// Maps an event-sourced grain state class to a stable type identifier. The state normally lives in memory;
/// when persisted it is a snapshot of the state at that point in time.
/// </summary>
/// <param name="typeIdentifier">The stable identifier the type is persisted under.</param>
[AttributeUsage(AttributeTargets.Class)]
public sealed class SnapshotTypeAttribute(string typeIdentifier) : TypeMapAttribute(typeIdentifier, TypeMapKinds.Snapshot) { }

/// <summary>Maps a metadata class, persisted alongside commands, events, or snapshots, to a stable type identifier.</summary>
/// <param name="typeIdentifier">The stable identifier the type is persisted under.</param>
[AttributeUsage(AttributeTargets.Class)]
public sealed class MetadataTypeAttribute(string typeIdentifier) : TypeMapAttribute(typeIdentifier, TypeMapKinds.Metadata) { }

/// <summary>
/// Helpers for <see cref="TypeMapKinds"/>.
/// </summary>
public static class TypeMapKindsExtensions
{
    private const int MinTypeMapKindsValue = 1;
    private const int MaxTypeMapKindsValue = 31;

    /// <summary>Builds the keyed-service key for the type mapper of <paramref name="value"/> on <paramref name="connectionName"/>.</summary>
    /// <param name="value">The kind or combination of kinds.</param>
    /// <param name="connectionName">The connection name.</param>
    /// <returns>The service key.</returns>
    public static string ToTypeMapperKey(this TypeMapKinds value, string connectionName) => ToTypeMapperKey((int)value, connectionName);

    /// <inheritdoc cref="ToTypeMapperKey(TypeMapKinds, string)"/>
    public static string ToTypeMapperKey(this int value, string connectionName) => $"{connectionName}-{value}";

    /// <summary>Converts an integer to <see cref="TypeMapKinds"/>, validating that it is a non-empty combination of defined kinds.</summary>
    /// <param name="value">The integer value.</param>
    /// <returns>The kinds.</returns>
    /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> is outside the valid range.</exception>
    public static TypeMapKinds ToTypeMapKinds(this int value)
    {
        if (IsValidTypeMapKinds(value))
        {
            return (TypeMapKinds)value;
        }
        throw new InvalidEnumArgumentException(nameof(value), value, typeof(TypeMapKinds));
    }

    private static bool IsValidTypeMapKinds(int value)
    {
        return value >= MinTypeMapKindsValue && value <= MaxTypeMapKindsValue;
    }
}