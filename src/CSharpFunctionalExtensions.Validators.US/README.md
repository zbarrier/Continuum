# Continuum.CSharpFunctionalExtensions.Validators.US

United States specific validators for [Continuum.CSharpFunctionalExtensions](https://www.nuget.org/packages/Continuum.CSharpFunctionalExtensions).

## Install

```
dotnet add package Continuum.CSharpFunctionalExtensions.Validators.US
```

## Validators

| Validator | Error codes |
|---|---|
| `IsUSPhoneNumber` | `us_phone_number` |
| `IsUSPhoneNumberExtension` | `us_phone_number_extension` |
| `IsUSZipCode` | `us_zip_code` |
| `IsUSPostalStateAbbreviation` | `us_state_abbreviation` |
| `IsUSTaxId` | `us_tax_id`, `us_ein_format`, `us_ssn_format`, `us_itin_format` |
| `IsUSEinTaxId` | `us_ein_format` |
| `IsUSSsnTaxId` | `us_ssn`, `us_ssn_format` |
| `IsUSItinTaxId` | `us_itin`, `us_itin_format` |

Each validator is available as a static `Result.IsUSX(value, propertyName)` call and as a chained `result.IsUSX(propertyName)` call.

```csharp
var result = Result.IsUSZipCode(input.Zip, nameof(input.Zip))
	.Bind(zip => Result.IsUSPostalStateAbbreviation(input.State, nameof(input.State)).Map(_ => zip));
```

## Localization

Translations can be registered with `ErrorMessageLocalizer.AddTranslation(culture, code, template)`.

a custom message passed to `ValidationError` is always preserved.

If your application creates or reads `us_*` errors (for example, deserializing stored errors) without calling any US validator first, call `USValidators.EnsureRegistered()` once at startup.

## License

MIT. See the repository `LICENSE` and `NOTICE` files.
