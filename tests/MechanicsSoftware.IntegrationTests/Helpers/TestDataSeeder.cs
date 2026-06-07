using Microsoft.EntityFrameworkCore;
using MechanicsSoftware.Infrastructure.Persistence;
using MechanicsSoftware.Infrastructure.Security;
using MechanicsSoftware.Domain.Entities;
using MechanicsSoftware.Domain.ValueObjects;
using MechanicsSoftware.Domain.Enums;

namespace MechanicsSoftware.IntegrationTests.Helpers;

public static class TestDataSeeder
{
    private static readonly BCryptPasswordHasher _passwordHasher = new();

    public static async Task<(string Email, string Password)> SeedTestUserAsync(AppDbContext context, string email = "test@example.com", string password = "Password123!")
    {
        var normalizedEmail = email.ToLowerInvariant();

        var exists = await context.Users
            .AnyAsync(u => u.Email == new Email(normalizedEmail));

        if (exists) return (normalizedEmail, password);

        var user = User.Create(
            id: Guid.NewGuid(),
            name: "Test User",
            email: normalizedEmail,
            passwordHash: _passwordHasher.Hash(password),
            role: User.Roles.Attendant
        );

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return (normalizedEmail, password);
    }

    public static async Task<Guid> SeedTestCustomerAsync(
        AppDbContext context,
        string name = "Test Customer",
        string documentValue = "11222333000181",
        string email = "customer@example.com",
        string phone = "11999999999")
    {
        var customer = Customer.Create(
            id: Guid.NewGuid(),
            name: name,
            taxId: documentValue,
            personType: PersonType.COMPANY,
            email: email.ToLowerInvariant(),
            phone: phone
        );

        context.Customers.Add(customer);
        await context.SaveChangesAsync();
        return customer.Id;
    }

    public static async Task<Guid> SeedTestPartAsync(
        AppDbContext context,
        string code = "OIL-001",
        string name = "Engine Oil",
        int priceInCents = 5000,
        int initialStock = 10)
    {
        var part = Part.Create(
            id: Guid.NewGuid(),
            code: code,
            name: name,
            description: $"Test part - {name}",
            unitPrice: new Money(priceInCents),
            initialStock: initialStock
        );

        context.Parts.Add(part);
        await context.SaveChangesAsync();
        return part.Id;
    }

    public static string HashPassword(string password) => _passwordHasher.Hash(password);
}
