using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MechanicsSoftware.Application.Features.Customers;
using MechanicsSoftware.IntegrationTests.Base;
using MechanicsSoftware.IntegrationTests.Helpers;
using MechanicsSoftware.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace MechanicsSoftware.IntegrationTests.Features.Customers;

public class CustomersIntegrationTests : IntegrationTestBase
{
    public override async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        await AuthenticateAsync();
    }

    [Fact]
    public async Task CreateCustomer_WithValidData_Returns201Created()
    {
        // Arrange
        var request = new CreateCustomerRequest(
            Name: "John Doe",
            DocumentValue: "11222333000181",
            PersonType: "COMPANY",
            Email: "john@example.com",
            Phone: "11999999999"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/customers", request);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var customerId = doc.RootElement.GetProperty("id").GetGuid();
        var name = doc.RootElement.GetProperty("name").GetString();
        var email = doc.RootElement.GetProperty("email").GetString();

        customerId.Should().NotBe(Guid.Empty);
        name.Should().Be("John Doe");
        email.Should().Be("john@example.com");
    }

    [Fact]
    public async Task GetCustomerById_WithValidId_Returns200Ok()
    {
        // Arrange
        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var customerId = await TestDataSeeder.SeedTestCustomerAsync(context);

        // Act
        var response = await _client.GetAsync($"/api/customers/{customerId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var returnedId = doc.RootElement.GetProperty("id").GetGuid();
        var name = doc.RootElement.GetProperty("name").GetString();

        returnedId.Should().Be(customerId);
        name.Should().Be("Test Customer");
    }

    [Fact]
    public async Task GetCustomerById_WithNonExistentId_Returns404NotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/customers/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCustomer_WithValidData_Returns200Ok()
    {
        // Arrange
        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var customerId = await TestDataSeeder.SeedTestCustomerAsync(context);

        var updateRequest = new UpdateCustomerRequest(
            Name: "Jane Doe Updated",
            Email: "jane@example.com",
            Phone: "11988888888"
        );

        // Act
        var response = await _client.PutAsJsonAsync($"/api/customers/{customerId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var name = doc.RootElement.GetProperty("name").GetString();
        var email = doc.RootElement.GetProperty("email").GetString();
        var phone = doc.RootElement.GetProperty("phone").GetString();

        name.Should().Be("Jane Doe Updated");
        email.Should().Be("jane@example.com");
        phone.Should().Be("11988888888");
    }

    [Fact]
    public async Task UpdateCustomer_WithNonExistentId_Returns404NotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateRequest = new UpdateCustomerRequest("Updated Name", "updated@example.com", "11988888888");

        // Act
        var response = await _client.PutAsJsonAsync($"/api/customers/{nonExistentId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCustomer_WithValidId_Returns204NoContent()
    {
        // Arrange
        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var customerId = await TestDataSeeder.SeedTestCustomerAsync(context);

        // Act
        var response = await _client.DeleteAsync($"/api/customers/{customerId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);

        // Verify customer is deleted
        var getResponse = await _client.GetAsync($"/api/customers/{customerId}");
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCustomer_WithNonExistentId_Returns404NotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/customers/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateCustomer_WithDuplicateDocument_ReturnsBadRequest()
    {
        // Arrange
        await using var scope = _factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await TestDataSeeder.SeedTestCustomerAsync(context, documentValue: "11222333000181");

        var request = new CreateCustomerRequest(
            Name: "Another Customer",
            DocumentValue: "11222333000181",
            PersonType: "COMPANY",
            Email: "another@example.com",
            Phone: "11988888888"
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/customers", request);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.UnprocessableEntity);
    }
}
