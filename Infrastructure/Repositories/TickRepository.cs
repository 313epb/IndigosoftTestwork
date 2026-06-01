using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Entities;

namespace Infrastructure.Repositories;

public class TickRepository(AppDbContext dbContext,IMapper mapper): ITickRepository
{
    public async Task SaveBatchAsync(
        IReadOnlyCollection<Tick> ticks,
        CancellationToken cancellationToken)
    {
        if (ticks.Count == 0)
        {
            return;
        }
        
        var entities = mapper.Map<List<TickEntity>>(ticks);

        await dbContext.Ticks.AddRangeAsync(entities, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}