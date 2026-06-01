using Domain.Entities;

namespace Domain.Interfaces;

public interface ITickRepository
{
    Task SaveBatchAsync(IReadOnlyCollection<Tick> ticks, CancellationToken cancellationToken);
}