using System.Reflection;
using Microsoft.EntityFrameworkCore;
using src.Entities;

namespace src.Context;

public class ApplicationDbContext : DbContext
{
    
    public ApplicationDbContext() { }
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly()); 
    

    
    // public DbSet<WeatherForecast> WeatherForecasts { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    
    public DbSet<ToDo> ToDos { get; set; } = null!;

}