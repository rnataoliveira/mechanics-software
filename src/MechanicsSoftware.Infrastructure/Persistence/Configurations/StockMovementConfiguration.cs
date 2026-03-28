using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MechanicsSoftware.Domain.Inventory;

namespace MechanicsSoftware.Infrastructure.Persistence.Configurations;

internal sealed class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("stock_movements");

        builder.HasKey(sm => sm.Id);

        builder.Property(sm => sm.Id)
            .HasColumnName("id");

        builder.Property(sm => sm.PartId)
            .HasColumnName("part_id")
            .IsRequired();

        builder.Property(sm => sm.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(sm => sm.Quantity)
            .HasColumnName("quantity")
            .IsRequired();

        builder.Property(sm => sm.Reference)
            .HasColumnName("reference");

        builder.Property(sm => sm.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
    }
}
