// Polyfill so records compile when targeting netstandard2.0 (required for Roslyn components).
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit
    {
    }
}
