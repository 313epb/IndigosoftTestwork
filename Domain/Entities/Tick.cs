namespace Domain.Entities;

public class Tick
{
    public string Source { get; set; } = null!;

    public string Symbol { get; set; } = null!;

    public decimal Price { get; set; }

    public decimal Volume { get; set; }

    public DateTime Timestamp { get; set; }

    public string UniqueKey =>
        $"{Source}:{Symbol}:{Price}:{Volume}:{Timestamp:O}";
}