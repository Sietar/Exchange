namespace Exchange.Repository;

public interface IRepository<T> where T : class, IEntity
{
    public void AddNew(T value);
    public T? GetById(int id);
    public IReadOnlyCollection<T> GetAll();
    public bool Remove(int id);
}