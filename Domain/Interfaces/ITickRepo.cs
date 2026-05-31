using Domain.Entities;

namespace Domain.Interfaces;

public interface ITickRepo
{
    Task SaveBatchAsync(IReadOnlyCollection<Tick> ticks, CancellationToken cancellationToken);
}