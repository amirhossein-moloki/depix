FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first for efficient caching
COPY CrmErp.slnx ./
COPY src/BuildingBlocks/*/*.csproj ./
COPY src/Modules/*/*/*.csproj ./
COPY src/Host/*/*.csproj ./
COPY tests/*/*.csproj ./

# Restore dependencies
RUN dotnet restore CrmErp.slnx

# Copy full source
COPY . .

# Build and publish
RUN dotnet publish src/Host/CrmErp.Host/CrmErp.Host.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CrmErp.Host.dll"]
