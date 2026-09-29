using System.Text.Json;
using System.Text.Json.Serialization;

namespace Continuum.CSharpFunctionalExtensions.Json.Serialization
{
    /// <summary>
    ///     Extension methods for configuring <see cref="JsonSerializerOptions"/>.
    /// </summary>
    public static class JsonSerializerOptionsExtensionMethods
    {
        /// <summary>
        ///     Registers the JSON converters for <see cref="Result"/> and <see cref="Result{T}"/>.
        /// </summary>
        /// <param name="options">The options to configure.</param>
        /// <returns>The same <paramref name="options"/> instance, for chaining.</returns>
        /// <remarks>
        ///     The <see cref="Result{T}"/> converter factory uses <c>MakeGenericType</c>. For Native AOT, use
        ///     <see cref="AddResultConverter(JsonSerializerOptions)"/> and <see cref="AddResultConverter{T}(JsonSerializerOptions)"/> instead.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.RequiresDynamicCode("Uses MakeGenericType for Result<T>. For Native AOT, use AddResultConverter<T>.")]
        public static JsonSerializerOptions AddCSharpFunctionalExtensionsConverters(this JsonSerializerOptions options)
        {
            options.Converters.Add(new ResultJsonConverter());
            options.Converters.Add(new ResultOfTJsonConverterFactory());

            return options;
        }

        /// <summary>
        ///     Registers the Native AOT safe JSON converter for <see cref="Result"/>.
        /// </summary>
        /// <param name="options">The options to configure.</param>
        /// <returns>The same <paramref name="options"/> instance, for chaining.</returns>
        public static JsonSerializerOptions AddResultConverter(this JsonSerializerOptions options)
        {
            options.Converters.Add(new ResultJsonConverter());
            return options;
        }

        /// <summary>
        ///     Registers the Native AOT safe JSON converter for <see cref="Result{T}"/>.
        /// </summary>
        /// <remarks>Metadata for <typeparamref name="T"/> must be available from the options' type info resolver.</remarks>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <param name="options">The options to configure.</param>
        /// <returns>The same <paramref name="options"/> instance, for chaining.</returns>
        public static JsonSerializerOptions AddResultConverter<T>(this JsonSerializerOptions options)
        {
            options.Converters.Add(new ResultJsonConverter<T>());
            return options;
        }
    }
}
