using System.Text.Json;
using System.Text.Json.Serialization;

namespace Continuum.CSharpFunctionalExtensions.Json.Serialization
{
    public static class JsonSerializerOptionsExtensionMethods
    {
        public static JsonSerializerOptions AddCSharpFunctionalExtensionsConverters(this JsonSerializerOptions options)
        {
            options.Converters.Add(new ResultJsonConverter());
            options.Converters.Add(new ResultOfTJsonConverterFactory());

            return options;
        }
    }
}
