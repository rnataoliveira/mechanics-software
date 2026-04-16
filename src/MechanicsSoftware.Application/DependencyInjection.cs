using System.Diagnostics.CodeAnalysis;
using MechanicsSoftware.Application.Features.Auth;
using MechanicsSoftware.Application.Features.Customers;
using MechanicsSoftware.Application.Features.Inventory;
using MechanicsSoftware.Application.Features.ServiceOrders;
using MechanicsSoftware.Application.Features.Vehicles;
using Microsoft.Extensions.DependencyInjection;

namespace MechanicsSoftware.Application;

[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Auth
        services.AddScoped<LoginUseCase>();

        // Customers
        services.AddScoped<CreateCustomerUseCase>();
        services.AddScoped<GetCustomerUseCase>();
        services.AddScoped<ListCustomersUseCase>();
        services.AddScoped<UpdateCustomerUseCase>();
        services.AddScoped<DeleteCustomerUseCase>();

        // Vehicles
        services.AddScoped<CreateVehicleUseCase>();
        services.AddScoped<GetVehicleUseCase>();
        services.AddScoped<ListVehiclesUseCase>();
        services.AddScoped<UpdateVehicleUseCase>();
        services.AddScoped<DeleteVehicleUseCase>();

        // Inventory
        services.AddScoped<CreatePartUseCase>();
        services.AddScoped<GetPartUseCase>();
        services.AddScoped<ListPartsUseCase>();
        services.AddScoped<UpdatePartUseCase>();
        services.AddScoped<DeletePartUseCase>();
        services.AddScoped<UpdateStockUseCase>();

        // Service Orders
        services.AddScoped<CreateServiceOrderUseCase>();
        services.AddScoped<StartDiagnosisUseCase>();
        services.AddScoped<AddServiceItemUseCase>();
        services.AddScoped<AddPartItemUseCase>();
        services.AddScoped<GenerateBudgetUseCase>();
        services.AddScoped<SendBudgetUseCase>();
        services.AddScoped<ApproveServiceOrderUseCase>();
        services.AddScoped<RejectServiceOrderUseCase>();
        services.AddScoped<StartExecutionUseCase>();
        services.AddScoped<CompleteServiceOrderUseCase>();
        services.AddScoped<DeliverServiceOrderUseCase>();
        services.AddScoped<GetServiceOrderStatusUseCase>();
        services.AddScoped<GetServiceOrderUseCase>();
        services.AddScoped<ListServiceOrdersUseCase>();
        services.AddScoped<GetAverageExecutionTimeUseCase>();

        return services;
    }
}
