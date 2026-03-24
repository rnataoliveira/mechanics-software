using Microsoft.EntityFrameworkCore;
using MechanicsSoftware.Domain.Customers;
using MechanicsSoftware.Domain.Inventory;
using MechanicsSoftware.Domain.ServiceOrders;
using MechanicsSoftware.Domain.Services;
using MechanicsSoftware.Domain.Vehicles;

namespace MechanicsSoftware.Application.Common;

public interface IAppDbContext
{
    DbSet<Customer> Customers { get; }
    DbSet<Vehicle> Vehicles { get; }
    DbSet<Part> Parts { get; }
    DbSet<Service> Services { get; }
    DbSet<ServiceOrder> ServiceOrders { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
