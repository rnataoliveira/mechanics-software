using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MechanicsSoftware.Domain.Vehicles;

namespace MechanicsSoftware.Infrastructure.Persistence.Configurations;

internal sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("vehicles");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .HasColumnName("id");

        builder.Property(v => v.Make)
            .HasColumnName("make")
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(v => v.Model)
            .HasColumnName("model")
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(v => v.Year)
            .HasColumnName("year")
            .IsRequired();

        builder.Property(v => v.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.OwnsOne(v => v.LicensePlate, lp =>
        {
            lp.Property(l => l.Value)
                .HasColumnName("license_plate")
                .HasMaxLength(7)
                .IsRequired();
        });

        builder.HasIndex("LicensePlate_Value").IsUnique();
    }
}
