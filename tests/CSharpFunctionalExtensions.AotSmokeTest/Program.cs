using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

using Continuum.CSharpFunctionalExtensions;
using Continuum.CSharpFunctionalExtensions.Json.Serialization;

// Exercises the library under Native AOT. Publish with:
//   dotnet publish tests/CSharpFunctionalExtensions.AotSmokeTest -c Release
// then run the produced executable. A non-zero exit code means a check failed.

var failures = 0;

void Check(string name, bool condition)
{
    Console.WriteLine($"{(condition ? "PASS" : "FAIL")}  {name}");
    if (!condition) failures++;
}

// Result / Maybe basics
var ok = Result.Success(5).Map(x => x * 2).Ensure(x => x > 5, RequestErrors.NewInvalidArg("too small"));
Check("Result map/ensure", ok.IsSuccess && ok.Value == 10);

var maybe = Maybe.From("value").Map(v => v.ToUpperInvariant());
Check("Maybe map", maybe.HasValue && maybe.Value == "VALUE");

// Core validators + combine
var combined = Result.Combine(
    Result.NotNullOrEmpty("", "Name"),
    Result.IsInt32("abc", "Age"));
Check("Validation combine", combined.IsFailure && combined.Error is ValidationError { Entries.Length: 2 });

// Localization of a core code
var localizer = new ErrorMessageLocalizer();
localizer.AddTranslation("fr", ValidationErrorCodes.NotEmpty, "'{0}' est requis.");
var notEmpty = (ValidationError)Result.NotNullOrEmpty("", "Name").Error;
Check("Core localization", notEmpty.Entries[0].GetFormattedMessage(CultureInfo.GetCultureInfo("fr"), localizer) == "'Name' est requis.");

// US package: validators, module initializer registration, localization
Check("US zip valid", Result.IsUSZipCode("12345", "Zip").IsSuccess);
Check("US tax id invalid", Result.IsUSTaxId("12-345678a", "Tin").IsFailure);
USValidators.EnsureRegistered();
localizer.AddTranslation("fr", USValidationErrorCodes.ZipCode, "'{0}' invalide.");
var zip = (ValidationError)Result.IsUSZipCode("1", "Zip").Error;
Check("US localization", zip.Entries[0].GetFormattedMessage(CultureInfo.GetCultureInfo("fr"), localizer) == "'Zip' invalide.");

// EnumValueObject reflection over static fields
Check("EnumValueObject", Color.All.Count == 2 && Color.FromId("red").Value == Color.Red);

// JSON: generated AddResultConverters with a source-generated context
var jsonOptions = SmokeJsonContext.AddResultConverters(
    new JsonSerializerOptions(JsonSerializerDefaults.Web) { TypeInfoResolver = SmokeJsonContext.Default });
var resultInfo = (JsonTypeInfo<Result>)jsonOptions.GetTypeInfo(typeof(Result));
var personResultInfo = (JsonTypeInfo<Result<Person>>)jsonOptions.GetTypeInfo(typeof(Result<Person>));

var personJson = JsonSerializer.Serialize(Result.Success(new Person("Ada", 36)), personResultInfo);
var personBack = JsonSerializer.Deserialize(personJson, personResultInfo);
Check("JSON Result<T> success", personBack.IsSuccess && personBack.Value == new Person("Ada", 36));

var failedJson = JsonSerializer.Serialize(Result.Failure<Person>(RequestErrors.NewInvalidArg("too small")), personResultInfo);
var failedBack = JsonSerializer.Deserialize(failedJson, personResultInfo);
Check("JSON Result<T> request error", failedBack.IsFailure && failedBack.Error is RequestError);

var validationJson = JsonSerializer.Serialize(Result.Combine(Result.NotNullOrEmpty("", "Name"), Result.IsInt32("abc", "Age")), resultInfo);
var validationBack = JsonSerializer.Deserialize(validationJson, resultInfo);
Check("JSON Result validation error",
    validationBack.IsFailure && validationBack.Error is ValidationError { Entries.Length: 2 } v && v.Entries[0].Target == "Name");

using (var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
{
    Content = new StringContent(personJson, Encoding.UTF8, "application/json")
})
{
    var read = await response.ReadResultAsync(personResultInfo);
    Check("HTTP ReadResultAsync<T>", read.IsSuccess && read.Value.Name == "Ada");
}

Console.WriteLine(failures == 0 ? "All checks passed." : $"{failures} check(s) failed.");
return failures == 0 ? 0 : 1;

sealed partial class Color : EnumValueObject<Color>
{
    public static readonly Color Red = new("red");
    public static readonly Color Blue = new("blue");

    private Color(string id) : base(id) { }
}

sealed record Person(string Name, int Age);

[JsonSerializable(typeof(Result))]
[JsonSerializable(typeof(Result<Person>))]
[JsonSerializable(typeof(Person))]
partial class SmokeJsonContext : JsonSerializerContext;
