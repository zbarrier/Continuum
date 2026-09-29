namespace Continuum.CSharpFunctionalExtensions;

internal static class HashCodeExtensions
{
    public static void AddSequence<T>(this ref HashCode hash, IReadOnlyList<T>? items)
    {
        if (items is null)
        {
            hash.Add(0);
            return;
        }

        hash.Add(items.Count);
        for (int i = 0; i < items.Count; i++)
        {
            hash.Add(items[i]);
        }
    }
}
