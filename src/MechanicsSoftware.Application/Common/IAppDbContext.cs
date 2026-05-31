using Microsoft.EntityFrameworkCore;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.ValueObjects;
using MechanicsSoftware.Domain.Enums;
using MechanicsSoftware.Domain.Exceptions;

namespace MechanicsSoftware.Application.Common;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<Customer> Customers { get; }
    DbSet<Vehicle> Vehicles { get; }
    DbSet<Part> Parts { get; }
    DbSet<Service> Services { get; }
    DbSet<ServiceOrder> ServiceOrders { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
