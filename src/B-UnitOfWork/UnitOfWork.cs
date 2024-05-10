using src.A_GenericRepository;
using src.Context;
using src.Entities;
using src.Interfaces;

namespace src.B_UnitOfWork;
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context; 
    public IUserRepository UserRepository { get; set; }
    public IToDoRepository ToDoRepository { get; set; }


    public UnitOfWork(ApplicationDbContext context, IUserRepository userRepository, IToDoRepository toDoRepository)
    {
        _context = context;
        UserRepository = userRepository;
        ToDoRepository = toDoRepository;
    }

    public async Task<int> Save() => 
        await _context.SaveChangesAsync();
    
    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
        // for better performance, 
        // use it when your class might be inherited by other classes that could introduce a finalizer
        // This will prevent the garbage collector from calling the finalizer of the UnitOfWork object, if one exists
        GC.SuppressFinalize(this);

    }
}



