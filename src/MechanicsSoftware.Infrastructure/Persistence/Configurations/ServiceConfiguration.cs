using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MechanicsSoftware.Domain.Services;
using MechanicsSoftware.Domain.Shared;

namespace MechanicsSoftware.Infrastructure.Persistence.Configurations;

public sealed class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("services");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");

        builder.Property(s => s.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(s => s.BasePrice)
            .HasConversion(
                m => m.Cents,
                m => new Money(m))
            .HasColumnName("base_price")
            .IsRequired();

        builder.Property(s => s.EstimatedMinutes)
            .HasColumnName("estimated_minutes")
            .IsRequired();

        builder.HasIndex(s => s.Name)
            .IsUnique()
            .HasDatabaseName("ix_services_name");
    }
}
