; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
CKDB001 | Continuum.EventSourcing.KurrentDB | Error | Journaled grain type contains the KurrentDB category separator
CKDB002 | Continuum.EventSourcing.KurrentDB | Warning | Grain type contains the reserved stream namespace separator
