using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Domain.Tasks;
using TaskManagementSystem.Infrastructure.Persistence;

namespace TaskManagementSystem.Tests.Tasks.Builders;

public abstract class TaskHandlerBuilderBase : IDisposable
{
    protected readonly AppDbContext Db;

    protected TaskHandlerBuilderBase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Db = new AppDbContext(options);
    }

    public AppDbContext GetDbContext() => Db;

    public void Dispose() => Db.Dispose();
}
