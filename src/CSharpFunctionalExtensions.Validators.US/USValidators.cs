using System.Runtime.CompilerServices;

namespace Continuum.CSharpFunctionalExtensions;

/// <summary>
///     Entry points for the US validators package.
/// </summary>
public static class USValidators
{
    /// <summary>
    ///     Ensures the US error codes are registered for localization.
    /// </summary>
    /// <remarks>
    ///     Registration happens automatically when this package is first used, so calling this is only needed when
    ///     an application creates or reads <c>us_*</c> errors without having called any US validator first.
    ///     Calling it more than once has no effect.
    /// </remarks>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void EnsureRegistered()
    {
        // Intentionally empty: calling into this assembly runs its module initializer.
    }
}
