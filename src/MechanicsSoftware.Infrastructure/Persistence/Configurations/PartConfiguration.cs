using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MechanicsSoftware.Domain.Inventory;

namespace MechanicsSoftware.Infrastructure.Persistence.Configurations;

internal sealed class PartConfiguration : IEntityTypeConfiguration<Part>
{
    public void Configure(EntityTypeBuilder<Part> builder)
    {
        builder.ToTable("parts");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id");

        builder.Property(p => p.Code)
            .HasColumnName("code")
            .HasMaxLength(30)
            .IsRequired();

        builder.HasIndex(p => p.Code).IsUnique();

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(p => p.StockQuantity)
            .HasColumnName("stock_quantity")
            .IsRequired();

        builder.Property(p => p.ReservedQuantity)
            .HasColumnName("reserved_quantity")
            .IsRequired();

        builder.OwnsOne(p => p.UnitPrice, money =>
        {
            money.Property(m => m.Cents)
                .HasColumnName("unit_price_cents")
                .IsRequired();
        });

        builder.HasMany(p => p.Movements)
            .WithOne()
            .HasForeignKey(sm => sm.PartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Movements)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
