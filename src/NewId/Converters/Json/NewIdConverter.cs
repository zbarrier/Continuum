// Portions of this file are adapted from NewId (https://github.com/phatboyg/NewId).
// Copyright 2007-2019 Chris Patterson.
// Licensed under the Apache License, Version 2.0. See LICENSE and NOTICE for details.
//
// Modified: adapted for Continuum (namespace, .NET 10, nullable annotations, XML documentation).

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Continuum.Converters.Json;

/// <summary>System.Text.Json converter that reads and writes a <see cref="NewId"/> as a sequential <see cref="Guid"/> string.</summary>
public sealed class NewIdConverter : JsonConverter<NewId>
{
    /// <inheritdoc />
    public override NewId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => NewId.FromSequentialGuid(Guid.Parse(reader.GetString()!));

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, NewId value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToSequentialGuid().ToString());
}
