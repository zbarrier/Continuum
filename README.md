# Continuum

An open-source framework based on .NET, Microsoft Orleans and KurrentDB.

## Packages

| Package | Description |
|---|---|
| [Continuum.CSharpFunctionalExtensions](src/CSharpFunctionalExtensions/README.md) | `Result`, `Maybe`, `ValueObject`, and a composable, localizable validation and error model. |
| [Continuum.CSharpFunctionalExtensions.Validators.US](src/CSharpFunctionalExtensions.Validators.US/README.md) | United States specific validators (phone number, zip code, state abbreviation, tax ID). |
| [Continuum.NewId](src/NewId/README.md) | Sequential, time-ordered, globally unique identifiers that interoperate with `Guid`. |
| [Continuum.NewId.Orleans](src/NewId.Orleans/README.md) | Microsoft Orleans serialization surrogate for `NewId`. |

## Building

```shell
dotnet build Continuum.slnx
dotnet test Continuum.slnx
```

## Versioning

All packages in this repository are versioned in lockstep. The version is defined once in
[`Directory.Build.props`](Directory.Build.props) (`VersionPrefix`) and can be overridden at pack time:

```shell
dotnet pack Continuum.slnx -c Release -o artifacts /p:Version=1.2.3
```

## License

Licensed under the [Apache License 2.0](LICENSE).

## Acknowledgements

Continuum builds on the work of other open-source projects:

- [CSharpFunctionalExtensions](https://github.com/vkhorikov/CSharpFunctionalExtensions) by Vladimir Khorikov (MIT) - the basis for `Result`, `Maybe`, and `ValueObject`.
- [Eventuous](https://github.com/Eventuous/eventuous) by Eventuous HQ OÜ (Apache 2.0) - the basis for the type mapping design.
- [FluentValidation](https://github.com/FluentValidation/FluentValidation) by the .NET Foundation and contributors (Apache 2.0) - the basis for the validators and their localized messages.
- [NewId](https://github.com/phatboyg/NewId) by Chris Patterson (Apache 2.0) - the basis for `NewId`, the sequential, time-ordered identifier.
- [Orleans.EventStore](https://github.com/hongliyu2002/Orleans.EventStore) by Leo Hong (MIT) - the basis for the KurrentDB (formerly EventStoreDB) log-consistency and grain storage providers.

See [NOTICE]
