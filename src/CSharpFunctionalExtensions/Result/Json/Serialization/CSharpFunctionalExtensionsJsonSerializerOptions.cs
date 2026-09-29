using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Continuum.CSharpFunctionalExtensions.Json.Serialization
{
    /// <summary>
    ///     Provides shared <see cref="JsonSerializerOptions"/> configured for result serialization.
    /// </summary>
    public static class CSharpFunctionalExtensionsJsonSerializerOptions
    {
        private static JsonSerializerOptions? _options;

        /// <summary>
        ///     Gets lazily created web-default options with the result converters registered.
        /// </summary>
        /// <remarks>
        ///     Uses the reflection-based <c>Result&lt;T&gt;</c> converter factory. For Native AOT, build options with
        ///     <see cref="JsonSerializerOptionsExtensionMethods.AddResultConverter(JsonSerializerOptions)"/> and a source-generated context.
        /// </remarks>
        public static JsonSerializerOptions Options
        {
            [RequiresDynamicCode("Uses MakeGenericType for Result<T>. For Native AOT, use AddResultConverter<T>.")]
            get => LazyInitializer.EnsureInitialized(ref _options, static () =>
            {
                var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
                options.AddCSharpFunctionalExtensionsConverters();
                return options;
            });
        }
    }
}
