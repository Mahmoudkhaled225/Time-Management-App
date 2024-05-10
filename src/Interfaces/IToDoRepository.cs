using src.A_GenericRepository;
using src.Entities;

namespace src.Interfaces;

public interface IToDoRepository : IGenericRepository<ToDo>
{ 
    Task<IEnumerable<ToDo>> GetAllByUserId(int? userId);
    
}