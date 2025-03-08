using System.Collections.Generic;

public static class EnumerableExtensions
{
    /// <summary>
    /// Returns an enumerable collection of indexed elements, similar to Rust's enumerate().
    /// </summary>
    /// <typeparam name="T">The type of elements in the source sequence</typeparam>
    /// <param name="source">The source enumerable</param>
    /// <returns>An enumerable of indexed items containing the original element and its zero-based index</returns>
    public static IEnumerable<IndexedItem<T>> Enumerate<T>(this IEnumerable<T> source)
    {
        return EnumerateIterator(source);
    }

    private static IEnumerable<IndexedItem<T>> EnumerateIterator<T>(IEnumerable<T> source)
    {
        int index = 0;
        foreach (var item in source)
        {
            yield return new IndexedItem<T>(index, item);
            index++;
        }
    }
}

/// <summary>
/// Represents an item with its index in a sequence
/// </summary>
/// <typeparam name="T">Type of the item</typeparam>
public readonly struct IndexedItem<T>
{
    public int Index { get; }

    public T Value { get; }

    public IndexedItem(int index, T value)
    {
        Index = index;
        Value = value;
    }

    // Deconstruction support for modern C# to allow: var (index, value) = item;
    public void Deconstruct(out int index, out T value)
    {
        index = Index;
        value = Value;
    }
}
