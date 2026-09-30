// Portions of this file are adapted from Eventuous (https://github.com/Eventuous/eventuous).
// Copyright (C) Eventuous HQ OÜ.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.

namespace Continuum.TypeMapping;

/// <summary>
/// Thrown when an <see cref="ITypeMapper"/> has no mapping for a requested type or type name.
/// </summary>
public sealed class TypeMappingNotFoundException : Exception
{
    const string MessageFormat = "A type mapping for the type '{0}' was not found.";

    /// <summary>Initializes a new instance for an unmapped CLR type.</summary>
    /// <param name="type">The unmapped type.</param>
    public TypeMappingNotFoundException(Type type) : base(string.Format(MessageFormat, type.Name)) { }

    /// <summary>Initializes a new instance for an unmapped type name.</summary>
    /// <param name="typeName">The unmapped type name.</param>
    public TypeMappingNotFoundException(string typeName) : base(string.Format(MessageFormat, typeName)) { }
}
