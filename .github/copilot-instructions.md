# Copilot Instructions

## Project Guidelines
- Do not commit or push without explicit approval; leave changes uncommitted for review. Approval to commit/push applies only to the changes that exist when approval is given. Any work done after that (even within the same request, e.g., "commit and push, then write X") must be left uncommitted for review until the user explicitly approves committing it.
- Follow the Continuum release strategy: no package releases until all Continuum framework projects are added; then a single coordinated 1.0.0 release of all packages.
- All Continuum framework library projects must be Native AOT compatible (set IsAotCompatible=true and keep builds free of AOT/trim warnings). Orleans-dependent runtime features (e.g., Orleans DeepCopier/AddSerializer used by grains and sessions) are not required to run under Native AOT since Orleans does not support AOT yet; libraries must still be IsAotCompatible with no AOT/trim warnings from Continuum's own code.
- Continuum projects reference each other with ProjectReference. Any Continuum project that defines type-mapped types (e.g., [DomainEventType]) must reference Continuum.Core.SourceGenerators as an analyzer (OutputItemType="Analyzer" ReferenceOutputAssembly="false"), not only the Core test project. Only the Core test project needs an analyzer reference to Continuum.Core.SourceGenerators.
- Continuum has not shipped to production; no backward compatibility with previously stored data or prior behavior is needed — don't add legacy fallbacks.

## Framework Usage
- Continuum type map attributes are used almost only in application code that consumes the framework via NuGet, not in other Continuum projects.
- Use the Orleans [Alias] naming convention in Continuum: "Continuum.<Area>.<TypeName>.V<n>" (e.g., Continuum.KurrentDBStorageException.V1). Omit namespace segments already implied by the type name or by Orleans itself. Abstract types do not get an [Alias].
- Orleans [Alias] names in Continuum should not include an "Orleans" segment, since being for Orleans is implied (e.g., "Continuum.KurrentDBStorageException.V1", not "Continuum.Orleans.KurrentDBStorageException.V1").
- Add a test project only when the library has behavior that genuinely needs testing; Continuum libraries do not each need their own test project.