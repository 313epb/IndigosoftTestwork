namespace Infrastructure.Entities;

public class TickEntity
{
    public long Id { get; set; }

    public string Source { get; set; } = default!;

    public string Symbol { get; set; } = default!;

    public decimal Price { get; set; }

    public decimal Volume { get; set; }

    public DateTime Timestamp { get; set; }
}