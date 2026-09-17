using System.Text.Json;
using Entities;

namespace FileRepositories;

public class FileRepository<T> where T : IEntity
{
    private readonly string _filePath;

    public FileRepository(string dataDirectory)
    {
        _filePath = Path.Combine(dataDirectory, $"{typeof(T).Name}.json");
    }

    // Handles these cases:
    // 1. The file does not exist yet.
    // 2. The file exists but has no content or only whitespace.
    // 3. The file contains the valid JSON value `null`.
    private async Task<List<T>> LoadAsync()
    {
        if (!File.Exists(_filePath))
        {
            // [] is a collection expression; here it creates an empty List<T>.
            return [];
        }

        string dataAsJson = await File.ReadAllTextAsync(_filePath);

        if (string.IsNullOrWhiteSpace(dataAsJson))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<T>>(dataAsJson) ?? [];
    }

    private async Task SaveAsync(List<T> entities)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);

        string dataAsJson = JsonSerializer.Serialize(entities);
        await File.WriteAllTextAsync(_filePath, dataAsJson);
    }

    public async Task<T> AddAsync(T entity)
    {
        List<T> entities = await LoadAsync();

        entity.Id = entities.Any()
            ? entities.Max(e => e.Id) + 1
            : 1;

        entities.Add(entity);
        await SaveAsync(entities);

        return entity;
    }

    public async Task UpdateAsync(T entity)
    {
        List<T> entities = await LoadAsync();

        T? existingEntity = entities.SingleOrDefault(e => e.Id == entity.Id);
        if (existingEntity is null)
        {
            throw new InvalidOperationException(
                $"Entity with Id {entity.Id} not found.");
        }

        entities.Remove(existingEntity);
        entities.Add(entity);
        await SaveAsync(entities);
    }

    public async Task DeleteAsync(int entityId)
    {
        List<T> entities = await LoadAsync();

        T? entityToDelete = entities.SingleOrDefault(e => e.Id == entityId);
        if (entityToDelete is null)
        {
            throw new InvalidOperationException(
                $"Entity with Id {entityId} not found.");
        }

        entities.Remove(entityToDelete);
        await SaveAsync(entities);
    }

    public async Task<T> GetByIdAsync(int entityId)
    {
        List<T> entities = await LoadAsync();

        T? entity = entities.SingleOrDefault(e => e.Id == entityId);
        if (entity is null)
        {
            throw new InvalidOperationException(
                $"Entity with Id {entityId} not found.");
        }

        return entity;
    }

    public IQueryable<T> GetManyAsync()
    {
        List<T> entities = LoadAsync().GetAwaiter().GetResult();

        return entities.AsQueryable();
    }
}