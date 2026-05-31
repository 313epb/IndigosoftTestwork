using Infrastructure.Entities;

namespace Infrastructure.Repositories;

public class TickRepository
{
    private readonly AppDbContext _db;

    public async Task SaveAsync(TickEntity tick, CancellationToken ct)
    {
        _db.Ticks.Add(tick);

        await _db.SaveChangesAsync();
    }
}