#nullable enable
using System.Text.Json;

namespace Continuum.CSharpFunctionalExtensions.Json.Serialization;

internal interface IErrorJsonConverter
{
    string TypeDiscriminator { get; }
    Type ErrorType { get; }
    Error Read(ref Utf8JsonReader reader, JsonSerializerOptions options);
    void WriteProperties(Utf8JsonWriter writer, Error error, JsonSerializerOptions options);
}

/// <summary>
///     Reads and writes one <see cref="Error"/> type as JSON. Register instances with <see cref="ErrorJsonTypeRegistry"/>.
/// </summary>
/// <remarks>
///     <para>
///         Errors are written as a JSON object with a string type discriminator, followed by the shared properties
///         (<c>Code</c>, <c>HttpStatusCode</c>, <c>GrpcStatusCode</c>), the properties written by
///         <see cref="WriteProperties"/>, and finally an output-only <c>Message</c> rendered for
///         <see cref="System.Globalization.CultureInfo.CurrentUICulture"/>.
///     </para>
///     <para>
///         Converters use <see cref="Utf8JsonReader"/>/<see cref="Utf8JsonWriter"/> directly so they are trimming and
///         Native AOT safe. Use the helpers on <see cref="ErrorJson"/> for property names and arguments.
///     </para>
/// </remarks>
/// <typeparam name="TError">The error type handled by this converter.</typeparam>
public abstract class ErrorJsonConverter<TError> : IErrorJsonConverter where TError : Error
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ErrorJsonConverter{TError}"/> class.
    /// </summary>
    /// <param name="typeDiscriminator">The unique string written to identify <typeparamref name="TError"/>.</param>
    /// <exception cref="ArgumentException"><paramref name="typeDiscriminator"/> is null, empty, or whitespace.</exception>
    protected ErrorJsonConverter(string typeDiscriminator)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(typeDiscriminator);
        TypeDiscriminator = typeDiscriminator;
    }

    /// <summary>Gets the unique string written to identify <typeparamref name="TError"/>.</summary>
    public string TypeDiscriminator { get; }

    Type IErrorJsonConverter.ErrorType => typeof(TError);

    /// <summary>
    ///     Reads an error. The reader is positioned on the <see cref="JsonTokenType.StartObject"/> token and must be left on the
    ///     matching <see cref="JsonTokenType.EndObject"/> token.
    /// </summary>
    /// <remarks>Unknown properties, including the type discriminator and <c>Message</c>, must be skipped.</remarks>
    /// <param name="reader">The reader.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The error.</returns>
    public abstract TError Read(ref Utf8JsonReader reader, JsonSerializerOptions options);

    /// <summary>
    ///     Writes the properties specific to <typeparamref name="TError"/>. The object start/end, type discriminator, shared
    ///     properties and <c>Message</c> are written by the caller.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="error">The error.</param>
    /// <param name="options">The serializer options.</param>
    public abstract void WriteProperties(Utf8JsonWriter writer, TError error, JsonSerializerOptions options);

    Error IErrorJsonConverter.Read(ref Utf8JsonReader reader, JsonSerializerOptions options) => Read(ref reader, options);

    void IErrorJsonConverter.WriteProperties(Utf8JsonWriter writer, Error error, JsonSerializerOptions options) =>
        WriteProperties(writer, (TError)error, options);
}
