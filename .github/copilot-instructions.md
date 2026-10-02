# Copilot Instructions

## Project Guidelines
- Do not commit or push without explicit approval; leave changes uncommitted for review.
- Follow the Continuum release strategy: no package releases until all Continuum framework projects are added; then a single coordinated 1.0.0 release of all packages.
- All Continuum framework library projects must be Native AOT compatible (set IsAotCompatible=true and keep builds free of AOT/trim warnings). Orleans-dependent runtime features (e.g., Orleans DeepCopier/AddSerializer used by grains and sessions) are not required to run under Native AOT since Orleans does not support AOT yet; libraries must still be IsAotCompatible with no AOT/trim warnings from Continuum's own code.
- Continuum projects reference each other with ProjectReference. Only the Core test project needs an analyzer reference to Continuum.Core.SourceGenerators.

## Framework Usage
- Continuum type map attributes are used almost only in application code that consumes the framework via NuGet, not in other Continuum projects.