using src.A_GenericRepository;
using src.Entities;

namespace src.Interfaces;


//good for uof pattern
public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> FindUserByEmailAsync(string email);

    Task<User?> GetWithToDos(int? userId);

}