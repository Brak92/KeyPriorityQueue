namespace KeyQueue;

public enum PriorityType
{
    Lowest = 0,
    Low = 1,
    Medium = 2,
    High = 3,
    Highest = 4
}

public static class PriorityTypeExtension
{
    public static void Increase(ref this PriorityType priority)
        => priority = priority switch
        {
            PriorityType.Highest => PriorityType.Highest,
            _ => (priority + 1)
        };

    public static void Decrease(ref this PriorityType priority)
        => priority = priority switch
        {
            PriorityType.Lowest => PriorityType.Lowest,
            _ => (priority - 1)
        };
}