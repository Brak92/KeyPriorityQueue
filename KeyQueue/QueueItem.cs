namespace KeyQueue;
public class QueueItem<T>(T item, int count = 1)
{

    public T Item { get; set; } = item;
    public int Count { get; set; } = count;

    public override bool Equals(object? obj) 
        => obj is QueueItem<T> other && EqualityComparer<T>.Default.Equals(Item, other.Item);

    public override int GetHashCode() 
        => Item?.GetHashCode() ?? 0;
}

