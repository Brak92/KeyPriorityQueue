using System.Collections.Concurrent;

namespace KeyQueue;

public class KeyPriorityQueue<TKey, TItem>(long delay = 1) where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, PriorityQueue<QueueItem<TItem>, int>> _queues = [];
    private readonly ConcurrentDictionary<TKey, CancellationTokenSource> _cancellationTokens = [];
    private readonly ConcurrentDictionary<TKey, SemaphoreSlim> _semaphores = [];
    private readonly TimeSpan _delay = TimeSpan.FromSeconds(delay);

    public bool Enqueue(TKey key, TItem item, PriorityType priority)
    {
        bool isNew = false;

        _queues.GetOrAdd(key, _ =>
        {
            isNew = true;
            _cancellationTokens[key] = new CancellationTokenSource();
            _semaphores[key] = new SemaphoreSlim(1, 1);
            return new PriorityQueue<QueueItem<TItem>, int>();
        });

        lock (_queues[key])
        {
            PriorityQueue<QueueItem<TItem>, int> queue = _queues[key];
            (QueueItem<TItem> Element, int Priority) = queue.UnorderedItems.FirstOrDefault(q => q.Element.Equals(item));

            if (Element is not null)
                Element.Count++;
            else
                queue.Enqueue(new QueueItem<TItem>(item), -(int)priority);
        }

        return isNew;
    }

    public async Task ProcessQueue(TKey key, Func<QueueItem<TItem>, TKey, Task> callback)
    {
        while (!_cancellationTokens[key].IsCancellationRequested)
        {
            QueueItem<TItem>? dqMessage = default;

            lock (_queues[key])
                if (_queues[key].Count > 0)
                    dqMessage = Dequeue(key);

            if (dqMessage is not null)
            {
                await _semaphores[key].WaitAsync(_cancellationTokens[key].Token);

                try
                {
                    await callback(dqMessage, key);
                }
                finally
                {
                    _semaphores[key].Release();
                }

                await Task.Delay(_delay, _cancellationTokens[key].Token);
            }
            else
                await Task.Delay(100, _cancellationTokens[key].Token);

        }
    }

    public void Stop(TKey key) 
        => _cancellationTokens[key].Cancel();

    public QueueItem<TItem>? Dequeue(TKey key) 
        => GetValue(key).Dequeue();

    public QueueItem<TItem>? Peek(TKey key)
    {
        lock (_queues[key])
            return GetValue(key).Peek();
    }

    public bool HasQueue(TKey key) 
        => _queues.ContainsKey(key);

    public int GetCount(TKey key) 
        => _queues.TryGetValue(key, out var queue) ? queue.Count 
                                                   : 0;

    private PriorityQueue<QueueItem<TItem>, int> GetValue(TKey key)
    {
        if (_queues.TryGetValue(key, out var queue) && queue.Count > 0)
            return queue;

        throw new InvalidOperationException($"Key '{key}' for the queue is empty or doesn't exist.");
    }
}
