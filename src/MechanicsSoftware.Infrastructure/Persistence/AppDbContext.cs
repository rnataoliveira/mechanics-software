using System.Reflection;
using Microsoft.EntityFrameworkCore;
using MechanicsSoftware.Application.Common;
using MechanicsSoftware.Domain.Auth;
using MechanicsSoftware.Domain.Customers;
using MechanicsSoftware.Domain.Inventory;
using MechanicsSoftware.Domain.ServiceOrders;
using MechanicsSoftware.Domain.Services;
using MechanicsSoftware.Domain.Vehicles;

namespace MechanicsSoftware.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<Vehicle> Vehicles { get; set; } = null!;
    public DbSet<Part> Parts { get; set; } = null!;
    public DbSet<Service> Services { get; set; } = null!;
    public DbSet<ServiceOrder> ServiceOrders { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        PromoteNewOwnedItemsToAdded();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        PromoteNewOwnedItemsToAdded();
        return base.SaveChanges();
    }

    // Workaround: EF Core may track freshly-added owned entities as Modified instead of
    // Added, producing UPDATE statements that affect 0 rows and throw
    // DbUpdateConcurrencyException. For each Modified owned entity, check the DB: if no row
    // exists for that key, flip the state to Added.
    private void PromoteNewOwnedItemsToAdded()
    {
        ChangeTracker.DetectChanges();
        foreach (var entry in ChangeTracker.Entries().ToList())
        {
            if (entry.State != EntityState.Modified) continue;
            if (entry.Metadata.FindOwnership() is null) continue;

            if (entry.GetDatabaseValues() is null)
                entry.State = EntityState.Added;
        }
    }
}
