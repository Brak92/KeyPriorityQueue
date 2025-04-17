This project was pulled off from another project I had where I had to send messages to different Teams channels and each channel url would have the same delay but not shared with other channels. I would separate each channel (Key) and Queue them based on the priority that I set up in different part of the original sender.\
\
The usage is very simple with this project:\
\
1- Create an instance of KeyPriorityQueue<TKey, TItem> and pass the interval delay in the constructor;\
2- Enqueue a task using TKey, TItem and the priority of this task, as parameters, using KeyPriorityQueue.Enqueue;\
3- If the return value of Enqueue is true, it means it's a new Queue, so you can use ProcessQueue with the parameters being TKey, Func<QueueItem<TItem>, TKey, Task>. Where Func will receive the Queued item and the Key as values to be used in the callback and it will return a Task;\
\
Usage example:
```csharp
public static class Communicator
{
  private static readonly KeyPriorityQueue<string, string> priorityQueues = new();

      public static Task AddMessageToQueue(string message, string group, PriorityType priority)
      {
        bool isNew = priorityQueues.Enqueue(group, message, priority);

        if (isNew)
            Task.Run(() => priorityQueues.ProcessQueue(group, SendMessageAsync));

        return Task.CompletedTask;
      }

    private static async Task SendMessageAsync(QueueItem<string> message, string group)
    {
        if (string.IsNullOrWhiteSpace(message.Item))
        {
            GenerateConsoleMessage(204, "Empty Message");
            return;
        }

        try
        {
            StringContent content = new(message.Item, Encoding.UTF8, "application/json");

            string url = "Your URL here";
            HttpResponseMessage response = await _httpClient.PostAsync(url, content);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                await Task.Delay(_delay);
                await SendMessageAsync(message, group);
            }

            GenerateConsoleMessage(response.StatusCode, (response.IsSuccessStatusCode ? $"Ok - {await response.Content.ReadAsStringAsync()}"
                                                                                      : await response.Content.ReadAsStringAsync()));

        }
        catch (Exception ex)
        {
            GenerateConsoleMessage(500, ex.Message);
        }
    }

    private static string GenerateConsoleMessage(HttpStatusCode statusCode, string message)
        => GenerateConsoleMessage((int)statusCode, message);

    private static string GenerateConsoleMessage(int statusCode, string message)
        => $"{statusCode} - {DateTime.Now:yyyy-MM-dd HH:mm:ss.ffff} - {message}";

}
```
