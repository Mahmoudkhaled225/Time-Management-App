namespace src.A_GenericRepository;

public interface IGenericRepository<T> where T : BaseEntity
{
    Task<T?> Get(int? id);
    Task<IEnumerable<T>> GetAll();
    Task Add(T entity);
    
    void Delete(T entity);
    void Update(T entity);
    // T Update(T entity);

    
}
