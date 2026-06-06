using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MechanicsSoftware.API.Transport.Inventory;
using MechanicsSoftware.Application.UseCases.Inventory.Handlers;
using MechanicsSoftware.Application.UseCases.Inventory.Queries;
using MechanicsSoftware.IntegrationTests.Base;
using MechanicsSoftware.IntegrationTests.Helpers;
using MechanicsSoftware.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MechanicsSoftware.IntegrationTests.Features.Inventory;

public class InventoryIntegrationTests : IntegrationTestBase
{
    public override async Task InitializeAsync()
    {
        await Factory.ResetDatabaseAsync();
        await AuthenticateAsync();
    }

    [Fact]
    public async Task CreatePart_WithValidData_Returns201Created()
    {
        // Arrange
        var request = new CreatePartRequest(
            Code: "OIL-002",
            Name: "Synthetic Engine Oil",
            Description: "High-quality synthetic oil",
            UnitPriceInCents: 7500,
            InitialStock: 5
        );

        // Act
        var response = await Client.PostAsJsonAsync("/api/parts", request);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var partId = doc.RootElement.GetProperty("id").GetGuid();
        var code = doc.RootElement.GetProperty("code").GetString();
        var name = doc.RootElement.GetProperty("name").GetString();
        var stockQuantity = doc.RootElement.GetProperty("stockQuantity").GetInt32();

        partId.Should().NotBe(Guid.Empty);
        code.Should().Be("OIL-002");
        name.Should().Be("Synthetic Engine Oil");
        stockQuantity.Should().Be(5);
    }

    [Fact]
    public async Task ReplenishStock_WithValidQuantity_Returns200Ok()
    {
        // Arrange
        await using var scope = Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var partId = await TestDataSeeder.SeedTestPartAsync(context, initialStock: 10);

        var replenishRequest = new UpdateStockRequest(Quantity: 15);

        // Act
        var response = await Client.PatchAsJsonAsync($"/api/parts/{partId}/stock", replenishRequest);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var updatedStock = doc.RootElement.GetProperty("stockQuantity").GetInt32();

        updatedStock.Should().Be(25); // 10 + 15
    }

    [Fact]
    public async Task ReplenishStock_CreatesStockMovementRecord()
    {
        // Arrange
        await using var scope = Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var partId = await TestDataSeeder.SeedTestPartAsync(context, initialStock: 10);

        var replenishRequest = new UpdateStockRequest(Quantity: 20);

        // Act
        var response = await Client.PatchAsJsonAsync($"/api/parts/{partId}/stock", replenishRequest);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        // Verify that StockMovement record was created in the database
        await using var verifyScope = Factory.Services.CreateAsyncScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var part = await verifyContext.Parts
            .Include(p => p.Movements)
            .FirstAsync(p => p.Id == partId);

        part.Movements.Should().HaveCountGreaterThanOrEqualTo(2); // Initial + replenish
        part.Movements.Where(m => m.Type == MechanicsSoftware.Domain.Enums.StockMovementType.Inbound)
            .Last().Quantity.Should().Be(20);
    }

    [Fact]
    public async Task ReplenishStock_WithNonExistentPart_Returns404NotFound()
    {
        // Arrange
        var nonExistentPartId = Guid.NewGuid();
        var replenishRequest = new UpdateStockRequest(Quantity: 10);

        // Act
        var response = await Client.PatchAsJsonAsync($"/api/parts/{nonExistentPartId}/stock", replenishRequest);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ReplenishStock_WithZeroQuantity_ReturnsUnprocessableEntity()
    {
        // Arrange
        await using var scope = Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var partId = await TestDataSeeder.SeedTestPartAsync(context);

        var replenishRequest = new UpdateStockRequest(Quantity: 0);

        // Act
        var response = await Client.PatchAsJsonAsync($"/api/parts/{partId}/stock", replenishRequest);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task ReplenishStock_WithNegativeQuantity_ReturnsUnprocessableEntity()
    {
        // Arrange
        await using var scope = Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var partId = await TestDataSeeder.SeedTestPartAsync(context);

        var replenishRequest = new UpdateStockRequest(Quantity: -5);

        // Act
        var response = await Client.PatchAsJsonAsync($"/api/parts/{partId}/stock", replenishRequest);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreatePart_WithDuplicateCode_ReturnsBadRequest()
    {
        // Arrange
        await using var scope = Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await TestDataSeeder.SeedTestPartAsync(context, code: "SPARK-001");

        var request = new CreatePartRequest(
            Code: "SPARK-001",
            Name: "Different Spark Plug",
            Description: "Test",
            UnitPriceInCents: 1000,
            InitialStock: 5
        );

        // Act
        var response = await Client.PostAsJsonAsync("/api/parts", request);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CreatePart_WithZeroInitialStock_Returns201Created()
    {
        // Arrange
        var request = new CreatePartRequest(
            Code: "EMPTY-001",
            Name: "Empty Part",
            Description: "Part with zero initial stock",
            UnitPriceInCents: 5000,
            InitialStock: 0
        );

        // Act
        var response = await Client.PostAsJsonAsync("/api/parts", request);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var stockQuantity = doc.RootElement.GetProperty("stockQuantity").GetInt32();

        stockQuantity.Should().Be(0);
    }

    [Fact]
    public async Task MultipleReplenishments_AreAllTracked()
    {
        // Arrange
        await using var scope = Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var partId = await TestDataSeeder.SeedTestPartAsync(context, initialStock: 10);

        // Act - First replenishment
        var response1 = await Client.PatchAsJsonAsync($"/api/parts/{partId}/stock", new UpdateStockRequest(5));
        response1.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        // Act - Second replenishment
        var response2 = await Client.PatchAsJsonAsync($"/api/parts/{partId}/stock", new UpdateStockRequest(3));
        response2.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        // Verify database records
        await using var verifyScope = Factory.Services.CreateAsyncScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var part = await verifyContext.Parts
            .Include(p => p.Movements)
            .FirstAsync(p => p.Id == partId);

        part.Should().NotBeNull();
        part!.StockQuantity.Should().Be(18); // 10 + 5 + 3
        part.Movements.Should().HaveCountGreaterThanOrEqualTo(3); // Initial + 2 replenishments
    }
}
