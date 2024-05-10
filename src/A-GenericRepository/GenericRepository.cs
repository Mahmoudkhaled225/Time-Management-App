using Microsoft.EntityFrameworkCore;
using src.Context;

namespace src.A_GenericRepository;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    private readonly ApplicationDbContext _context;

    public GenericRepository(ApplicationDbContext context) =>
        _context = context; 
    
    public async Task<T?> Get(int? id) => 
        await _context.Set<T>().FindAsync(id);
    public async Task<IEnumerable<T>> GetAll() =>
        await _context.Set<T>().ToListAsync();
    public async Task Add(T entity) =>
        await _context.Set<T>().AddAsync(entity);
        
    public void Delete(T entity) =>
        _context.Set<T>().Remove(entity);
    public void Update(T entity) =>
        _context.Set<T>().Update(entity);
}