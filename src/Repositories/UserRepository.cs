using Microsoft.EntityFrameworkCore;
using src.A_GenericRepository;
using src.Context;
using src.Entities;
using src.Interfaces;

namespace src.Repositories;


//if you will make specific repo to add extra methods, you can inherit from GenericRepository
//and implement the interface and use CONTEXT.ENTITY not unit of work
public class UserRepository : GenericRepository<User> , IUserRepository
{
    private readonly ApplicationDbContext _context;
    

    public UserRepository(ApplicationDbContext context) : base(context) =>
        this._context = context;
    
    public async Task<User?> FindUserByEmailAsync(string email) => 
        await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    
    public async Task<User?> GetWithToDos(int? userId)
    {
        return await _context.Users
            .Include(user => user.ToDos)
            .FirstOrDefaultAsync(user => user.Id == userId);
    }

    
}