// Portions of this file are adapted from Eventuous (https://github.com/Eventuous/eventuous).
// Copyright (C) Eventuous HQ OÜ.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.

namespace Continuum.TypeMapping;

/// <summary>
/// <see cref="TypeMapper"/> populated from types decorated with type map attributes, as recorded in <see cref="TypeMapRegistry"/>.
/// </summary>
public sealed class DefaultTypeMapAttributeMapper : TypeMapper
{
    /// <summary>Adds every registered attribute-decorated type whose kind is included in <paramref name="typeMapKinds"/>.</summary>
    /// <param name="typeMapKinds">The kinds to add. May be a combination of flags.</param>
    public void RegisterKnownTypes(TypeMapKinds typeMapKinds)
        => RegisterKnownAttributeTypes(typeMapKinds);
}