using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MechanicsSoftware.Domain.Inventory;
using MechanicsSoftware.Domain.Shared;

namespace MechanicsSoftware.Infrastructure.Persistence.Configurations;

public sealed class PartConfiguration : IEntityTypeConfiguration<Part>
{
    public void Configure(EntityTypeBuilder<Part> builder)
    {
        builder.ToTable("parts");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");

        builder.Property(p => p.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(p => p.UnitPrice)
            .HasConversion(
                m => m.Cents,
                m => new Money(m))
            .HasColumnName("unit_price")
            .IsRequired();

        builder.Property(p => p.StockQuantity)
            .HasColumnName("stock_quantity")
            .IsRequired();

        builder.Property(p => p.ReservedQuantity)
            .HasColumnName("reserved_quantity")
            .IsRequired();

        builder.Ignore(p => p.AvailableQuantity);

        builder.HasIndex(p => p.Code)
            .IsUnique()
            .HasDatabaseName("ix_parts_code");

        builder.Navigation(p => p.Movements)
            .HasField("_movements")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(p => p.Movements, sm =>
        {
            sm.ToTable("stock_movements");
            sm.HasKey(s => s.Id);
            sm.Property(s => s.Id).HasColumnName("id");
            sm.WithOwner().HasForeignKey(s => s.PartId);
            sm.Property(s => s.PartId).HasColumnName("part_id");
            sm.HasIndex(s => s.PartId).HasDatabaseName("ix_stock_movements_part_id");
            sm.Property(s => s.Type)
                .HasColumnName("type")
                .HasConversion<string>()
                .IsRequired();
            sm.Property(s => s.Quantity).HasColumnName("quantity").IsRequired();
            sm.Property(s => s.Reference).HasColumnName("reference");
            sm.Property(s => s.CreatedAt).HasColumnName("created_at").IsRequired();
        });
    }
}
