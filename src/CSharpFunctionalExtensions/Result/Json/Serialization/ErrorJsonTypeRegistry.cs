#nullable enable
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Continuum.CSharpFunctionalExtensions.Json.Serialization;

/// <summary>
///     Maps <see cref="Error"/> types to the string type discriminators and converters used for JSON serialization.
/// </summary>
/// <remarks>
///     <see cref="RequestError"/> (<c>"RequestError"</c>) and <see cref="ValidationError"/> (<c>"ValidationError"</c>) are
///     registered by default. Register custom error types once at startup, before serializing.
/// </remarks>
public static class ErrorJsonTypeRegistry
{
    private static readonly ConcurrentDictionary<string, IErrorJsonConverter> ByDiscriminator = new(StringComparer.Ordinal);
    private static readonly ConcurrentDictionary<Type, IErrorJsonConverter> ByType = new();
    private static readonly object Gate = new();

    static ErrorJsonTypeRegistry()
    {
        Register(new RequestErrorJsonConverter());
        Register(new ValidationErrorJsonConverter());
    }

    /// <summary>
    ///     Registers a converter for <typeparamref name="TError"/>.
    /// </summary>
    /// <remarks>Registering the same error type with the same discriminator again replaces the converter.</remarks>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="converter">The converter.</param>
    /// <exception cref="ArgumentNullException"><paramref name="converter"/> is null.</exception>
    /// <exception cref="InvalidOperationException">
    ///     The discriminator is already used by another error type, or <typeparamref name="TError"/> is already registered with
    ///     another discriminator.
    /// </exception>
    public static void Register<TError>(ErrorJsonConverter<TError> converter) where TError : Error
    {
        ArgumentNullException.ThrowIfNull(converter);

        lock (Gate)
        {
            if (ByDiscriminator.TryGetValue(converter.TypeDiscriminator, out var existing) && existing.ErrorType != typeof(TError))
            {
                throw new InvalidOperationException(
                    $"The discriminator '{converter.TypeDiscriminator}' is already registered for '{existing.ErrorType}'.");
            }
            if (ByType.TryGetValue(typeof(TError), out existing) && existing.TypeDiscriminator != converter.TypeDiscriminator)
            {
                throw new InvalidOperationException(
                    $"The error type '{typeof(TError)}' is already registered with discriminator '{existing.TypeDiscriminator}'.");
            }

            ByDiscriminator[converter.TypeDiscriminator] = converter;
            ByType[typeof(TError)] = converter;
        }
    }

    /// <summary>Gets whether <paramref name="errorType"/> is registered.</summary>
    /// <param name="errorType">The error type.</param>
    /// <returns><see langword="true"/> when a converter is registered for the type.</returns>
    public static bool IsRegistered(Type errorType) => ByType.ContainsKey(errorType);

    internal static bool TryGetConverter(Type errorType, [NotNullWhen(true)] out IErrorJsonConverter? converter) =>
        ByType.TryGetValue(errorType, out converter);

    internal static bool TryGetConverter(string discriminator, [NotNullWhen(true)] out IErrorJsonConverter? converter) =>
        ByDiscriminator.TryGetValue(discriminator, out converter);
}
