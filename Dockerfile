FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY MechanicsSoftware.sln .
COPY src/MechanicsSoftware.Domain/MechanicsSoftware.Domain.csproj src/MechanicsSoftware.Domain/
COPY src/MechanicsSoftware.Application/MechanicsSoftware.Application.csproj src/MechanicsSoftware.Application/
COPY src/MechanicsSoftware.Infrastructure/MechanicsSoftware.Infrastructure.csproj src/MechanicsSoftware.Infrastructure/
COPY src/MechanicsSoftware.API/MechanicsSoftware.API.csproj src/MechanicsSoftware.API/

RUN dotnet restore src/MechanicsSoftware.API/MechanicsSoftware.API.csproj

COPY src/ src/

RUN dotnet publish src/MechanicsSoftware.API/MechanicsSoftware.API.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MechanicsSoftware.API.dll"]
