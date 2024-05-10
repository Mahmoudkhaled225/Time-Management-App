using src.A_GenericRepository;
using src.Entities;
using src.Interfaces;

namespace src.B_UnitOfWork;

public interface IUnitOfWork : IAsyncDisposable
{
    IUserRepository UserRepository { get; set; }
    IToDoRepository ToDoRepository { get; set; }
    
    Task<int> Save();
}