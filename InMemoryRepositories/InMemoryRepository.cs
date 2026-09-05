using Entities;

namespace InMemoryRepositories;

public class InMemoryRepository<T> where T : IEntity
{
    private List<T> _entities;

    public InMemoryRepository(IEnumerable<T>? initialEntities = null)
    {
        _entities = initialEntities?.ToList() ?? new List<T>();
    }

    public Task<T> AddAsync(T entity)
    {
        entity.Id = _entities.Any()
            ? _entities.Max(e => e.Id) + 1
            : 1;

        _entities.Add(entity);
        return Task.FromResult(entity);
    }

    public Task UpdateAsync(T entity)
    {
        T? existingEntity = _entities.SingleOrDefault(e => e.Id == entity.Id);
        if (existingEntity is null)
        {
            throw new InvalidOperationException(
                $"Entity with Id {entity.Id} not found.");
        }

        _entities.Remove(existingEntity);
        _entities.Add(entity);

        return Task.CompletedTask;
    }

    public Task DeleteAsync(int entityId)
    {
        T? entityToDelete = _entities.SingleOrDefault(e => e.Id == entityId);
        if (entityToDelete is null)
        {
            throw new InvalidOperationException(
                $"Entity with Id {entityId} not found.");
        }

        _entities.Remove(entityToDelete);
        return Task.CompletedTask;
    }

    public Task<T> GetByIdAsync(int entityId)
    {
        T? entity = _entities.SingleOrDefault(e => e.Id == entityId);
        if (entity is null)
        {
            throw new InvalidOperationException(
                $"Entity with Id {entityId} not found.");
        }

        return Task.FromResult(entity);
    }

    public IQueryable<T> GetManyAsync()
    {
        return _entities.AsQueryable();
    }
}
