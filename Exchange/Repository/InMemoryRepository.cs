namespace Exchange.Repository;

public class InMemoryRepository<T> : IRepository<T> where T : class, IEntity
{

    private readonly Dictionary<int, T> _entities = new();
    
    public void AddNew(T value)
    {
        if (!_entities.TryAdd(value.Id, value))
        {
            throw new ArgumentException($"Entity with id {value.Id} already exists");
        }
    }

    public T? GetById(int id)
    {
        return _entities.GetValueOrDefault(id);
    }
    public IReadOnlyCollection<T> GetAll()
    {
        return [.. _entities.Values];
    }

    public bool Remove(int id)
    {
        return _entities.Remove(id);
    }

    public override string ToString()
    {
        string result = "";

        foreach (var entity in _entities)
        {
            result += entity.Key.ToString() + entity.Value + Environment.NewLine;
        }

        return result;
    }
}
