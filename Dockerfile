FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first for efficient caching
COPY CrmErp.sln ./

# Copy BuildingBlocks project files
COPY src/BuildingBlocks/Application/*.csproj src/BuildingBlocks/Application/
COPY src/BuildingBlocks/Common/*.csproj src/BuildingBlocks/Common/
COPY src/BuildingBlocks/Domain/*.csproj src/BuildingBlocks/Domain/
COPY src/BuildingBlocks/Infrastructure/*.csproj src/BuildingBlocks/Infrastructure/
COPY src/BuildingBlocks/SharedKernel/*.csproj src/BuildingBlocks/SharedKernel/

# Copy Host project files
COPY src/Host/CrmErp.Host/*.csproj src/Host/CrmErp.Host/

# Copy Modules project files
COPY src/Modules/CRM/API/*.csproj src/Modules/CRM/API/
COPY src/Modules/CRM/Application/*.csproj src/Modules/CRM/Application/
COPY src/Modules/CRM/Domain/*.csproj src/Modules/CRM/Domain/
COPY src/Modules/CRM/Infrastructure/*.csproj src/Modules/CRM/Infrastructure/

COPY src/Modules/Customer/API/*.csproj src/Modules/Customer/API/
COPY src/Modules/Customer/Application/*.csproj src/Modules/Customer/Application/
COPY src/Modules/Customer/Domain/*.csproj src/Modules/Customer/Domain/
COPY src/Modules/Customer/Infrastructure/*.csproj src/Modules/Customer/Infrastructure/

COPY src/Modules/Finance/API/*.csproj src/Modules/Finance/API/
COPY src/Modules/Finance/Application/*.csproj src/Modules/Finance/Application/
COPY src/Modules/Finance/Domain/*.csproj src/Modules/Finance/Domain/
COPY src/Modules/Finance/Infrastructure/*.csproj src/Modules/Finance/Infrastructure/

COPY src/Modules/Identity/API/*.csproj src/Modules/Identity/API/
COPY src/Modules/Identity/Application/*.csproj src/Modules/Identity/Application/
COPY src/Modules/Identity/Domain/*.csproj src/Modules/Identity/Domain/
COPY src/Modules/Identity/Infrastructure/*.csproj src/Modules/Identity/Infrastructure/

COPY src/Modules/Platform/API/*.csproj src/Modules/Platform/API/
COPY src/Modules/Platform/Application/*.csproj src/Modules/Platform/Application/
COPY src/Modules/Platform/Domain/*.csproj src/Modules/Platform/Domain/
COPY src/Modules/Platform/Infrastructure/*.csproj src/Modules/Platform/Infrastructure/

COPY src/Modules/Project/API/*.csproj src/Modules/Project/API/
COPY src/Modules/Project/Application/*.csproj src/Modules/Project/Application/
COPY src/Modules/Project/Domain/*.csproj src/Modules/Project/Domain/
COPY src/Modules/Project/Infrastructure/*.csproj src/Modules/Project/Infrastructure/

COPY src/Modules/Reporting/API/*.csproj src/Modules/Reporting/API/
COPY src/Modules/Reporting/Application/*.csproj src/Modules/Reporting/Application/
COPY src/Modules/Reporting/Domain/*.csproj src/Modules/Reporting/Domain/
COPY src/Modules/Reporting/Infrastructure/*.csproj src/Modules/Reporting/Infrastructure/

COPY src/Modules/Sales/API/*.csproj src/Modules/Sales/API/
COPY src/Modules/Sales/Application/*.csproj src/Modules/Sales/Application/
COPY src/Modules/Sales/Domain/*.csproj src/Modules/Sales/Domain/
COPY src/Modules/Sales/Infrastructure/*.csproj src/Modules/Sales/Infrastructure/

COPY src/Modules/Support/API/*.csproj src/Modules/Support/API/
COPY src/Modules/Support/Application/*.csproj src/Modules/Support/Application/
COPY src/Modules/Support/Domain/*.csproj src/Modules/Support/Domain/
COPY src/Modules/Support/Infrastructure/*.csproj src/Modules/Support/Infrastructure/

# Copy Tools project files
COPY src/Tools/Depix.Cli/*.csproj src/Tools/Depix.Cli/

# Copy tests project files
COPY tests/CrmErp.ArchitectureTests/*.csproj tests/CrmErp.ArchitectureTests/
COPY tests/Depix.Cli.Tests/*.csproj tests/Depix.Cli.Tests/
COPY tests/IntegrationTests/*.csproj tests/IntegrationTests/
COPY tests/Modules.Identity.Tests/*.csproj tests/Modules.Identity.Tests/
COPY tests/UnitTests/*.csproj tests/UnitTests/

# Restore dependencies
RUN dotnet restore CrmErp.sln

# Copy full source
COPY . .

# Build and publish
RUN dotnet publish src/Host/CrmErp.Host/CrmErp.Host.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
WORKDIR /app
RUN mkdir -p /app/logs
EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:5000

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CrmErp.Host.dll"]
