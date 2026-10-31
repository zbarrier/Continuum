; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
CTM001 | Continuum.Core | Warning | Type map attribute on a non-public type in a library
CTM002 | Continuum.Core | Warning | Type-mapped type is not listed on a JsonSerializerContext
CTM003 | Continuum.Core | Error | Type map attribute on a type the generated registration cannot access
CTM004 | Continuum.Core | Warning | Type map attribute on a generic or static class
CTM005 | Continuum.Core | Error | Type map identifier is used by more than one type
