using Microsoft.EntityFrameworkCore;
using src.A_GenericRepository;
using src.Context;
using src.Entities;
using src.Interfaces;

namespace src.Repositories;

public class ToDoRepository : GenericRepository<ToDo>, IToDoRepository
{
    private readonly ApplicationDbContext _context;


    public ToDoRepository(ApplicationDbContext context) : base(context) =>
        this._context = context;
    
    public async Task<IEnumerable<ToDo>> GetAllByUserId(int? userId)
    {
        return await _context.ToDos
            .Where(todo => todo.UserId == userId).ToListAsync();
    }
}