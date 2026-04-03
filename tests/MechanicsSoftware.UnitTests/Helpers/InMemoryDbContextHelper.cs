using Microsoft.EntityFrameworkCore;
using MechanicsSoftware.Infrastructure.Persistence;

namespace MechanicsSoftware.UnitTests.Helpers;

public static class InMemoryDbContextHelper
{
    public static AppDbContext Create(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
