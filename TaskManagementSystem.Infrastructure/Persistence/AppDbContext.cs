using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Domain.Tasks;
using TaskManagementSystem.Domain.Users;

namespace TaskManagementSystem.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<User> Users => Set<User>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
