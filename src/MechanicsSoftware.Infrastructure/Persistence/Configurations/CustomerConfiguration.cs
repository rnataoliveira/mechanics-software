using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MechanicsSoftware.Domain.Customers;

namespace MechanicsSoftware.Infrastructure.Persistence.Configurations;

internal sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id");

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Phone)
            .HasColumnName("phone")
            .HasMaxLength(20)
            .IsRequired();

        builder.OwnsOne(c => c.Document, doc =>
        {
            doc.Property(d => d.Value)
                .HasColumnName("document_value")
                .HasMaxLength(14)
                .IsRequired();

            doc.Property(d => d.PersonType)
                .HasColumnName("document_person_type")
                .HasConversion<string>()
                .HasMaxLength(10)
                .IsRequired();
        });

        builder.HasIndex("Document_Value").IsUnique();

        builder.OwnsOne(c => c.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("email")
                .HasMaxLength(256)
                .IsRequired();
        });
    }
}
