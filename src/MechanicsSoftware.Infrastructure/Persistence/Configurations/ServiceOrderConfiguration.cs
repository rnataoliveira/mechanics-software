using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MechanicsSoftware.Domain.Customers;
using MechanicsSoftware.Domain.ServiceOrders;
using MechanicsSoftware.Domain.Shared;
using MechanicsSoftware.Domain.Vehicles;

namespace MechanicsSoftware.Infrastructure.Persistence.Configurations;

public sealed class ServiceOrderConfiguration : IEntityTypeConfiguration<ServiceOrder>
{
    public void Configure(EntityTypeBuilder<ServiceOrder> builder)
    {
        builder.ToTable("service_orders");

        builder.HasKey(so => so.Id);
        builder.Property(so => so.Id).HasColumnName("id");

        builder.Property(so => so.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(so => so.VehicleId)
            .HasColumnName("vehicle_id")
            .IsRequired();

        builder.Property(so => so.Status)
            .HasConversion(
                v => v.Value.ToString(),
                v => new ServiceOrderStatus(Enum.Parse<ServiceOrderStatus.Status>(v)))
            .HasColumnName("status")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(so => so.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(so => so.DeliveredAt)
            .HasColumnName("delivered_at");

        builder.OwnsMany(so => so.ServiceItems, si =>
        {
            si.ToTable("service_items");
            si.HasKey(s => s.Id);
            si.Property(s => s.Id).HasColumnName("id");
            si.WithOwner().HasForeignKey(s => s.ServiceOrderId);
            si.Property(s => s.ServiceOrderId).HasColumnName("service_order_id");
            si.Property(s => s.ServiceId).HasColumnName("service_id").IsRequired();
            si.Property(s => s.ServiceName)
                .HasColumnName("service_name")
                .HasMaxLength(100)
                .IsRequired();
            si.Property(s => s.UnitPrice)
                .HasConversion(m => m.Cents, m => new Money(m))
                .HasColumnName("unit_price")
                .IsRequired();
            si.Property(s => s.Quantity).HasColumnName("quantity").IsRequired();
            si.Ignore(s => s.Total);
        });

        builder.OwnsMany(so => so.PartItems, pi =>
        {
            pi.ToTable("part_items");
            pi.HasKey(p => p.Id);
            pi.Property(p => p.Id).HasColumnName("id");
            pi.WithOwner().HasForeignKey(p => p.ServiceOrderId);
            pi.Property(p => p.ServiceOrderId).HasColumnName("service_order_id");
            pi.Property(p => p.PartId).HasColumnName("part_id").IsRequired();
            pi.Property(p => p.PartName)
                .HasColumnName("part_name")
                .HasMaxLength(100)
                .IsRequired();
            pi.Property(p => p.UnitPrice)
                .HasConversion(m => m.Cents, m => new Money(m))
                .HasColumnName("unit_price")
                .IsRequired();
            pi.Property(p => p.Quantity).HasColumnName("quantity").IsRequired();
            pi.Property(p => p.Availability)
                .HasConversion<string>()
                .HasColumnName("availability")
                .IsRequired();
            pi.Ignore(p => p.Total);
        });

        builder.OwnsOne(so => so.Budget, budget =>
        {
            budget.ToTable("budgets");
            budget.HasKey(b => b.Id);
            budget.Property(b => b.Id).HasColumnName("id");
            budget.WithOwner().HasForeignKey(b => b.ServiceOrderId);
            budget.Property(b => b.ServiceOrderId).HasColumnName("service_order_id");
            budget.Property(b => b.Total)
                .HasConversion(m => m.Cents, m => new Money(m))
                .HasColumnName("total")
                .IsRequired();
            budget.Property(b => b.Status)
                .HasConversion(
                    v => v.Value.ToString(),
                    v => new BudgetStatus(Enum.Parse<BudgetStatus.Status>(v)))
                .HasColumnName("status")
                .HasMaxLength(20)
                .IsRequired();
            budget.Property(b => b.CreatedAt).HasColumnName("created_at").IsRequired();
        });

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(so => so.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Vehicle>()
            .WithMany()
            .HasForeignKey(so => so.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
