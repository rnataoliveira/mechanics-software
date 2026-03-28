using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MechanicsSoftware.Domain.Customers;
using MechanicsSoftware.Domain.Vehicles;

namespace MechanicsSoftware.Infrastructure.Persistence.Configurations;

public sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("vehicles");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).HasColumnName("id");

        builder.Property(v => v.LicensePlate)
            .HasConversion(
                v => v.Value,
                v => new LicensePlate(v))
            .HasColumnName("license_plate")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(v => v.Make)
            .HasColumnName("make")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(v => v.Model)
            .HasColumnName("model")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(v => v.Year)
            .HasColumnName("year")
            .IsRequired();

        builder.Property(v => v.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.HasIndex(v => v.LicensePlate)
            .IsUnique()
            .HasDatabaseName("ix_vehicles_license_plate");

        builder.HasIndex(v => v.CustomerId)
            .HasDatabaseName("ix_vehicles_customer_id");

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(v => v.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
