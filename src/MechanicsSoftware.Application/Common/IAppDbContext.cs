using Microsoft.EntityFrameworkCore;
using MechanicsSoftware.Domain.Customers;
using MechanicsSoftware.Domain.Inventory;
using MechanicsSoftware.Domain.Vehicles;

namespace MechanicsSoftware.Application.Common;

public interface IAppDbContext
{
    DbSet<Customer> Customers { get; }
    DbSet<Vehicle> Vehicles { get; }
    DbSet<Part> Parts { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
