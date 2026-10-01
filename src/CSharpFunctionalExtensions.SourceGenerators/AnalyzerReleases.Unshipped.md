; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
CFE001 | Continuum.CSharpFunctionalExtensions | Error | EnumValueObject types must be partial
CFE002 | Continuum.CSharpFunctionalExtensions | Warning | Unusual HTTP/gRPC status code pair
CFE003 | Continuum.CSharpFunctionalExtensions | Warning | RetryAfter set for a non-retryable status
CFE004 | Continuum.CSharpFunctionalExtensions | Warning | Result converters are not registered
CFE005 | Continuum.CSharpFunctionalExtensions | Error | Invalid error format string
CFE006 | Continuum.CSharpFunctionalExtensions | Warning | Error format string is built per call
