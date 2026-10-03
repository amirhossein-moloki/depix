#!/usr/bin/env bash
set -e

COMPOSE_FILE="docker-compose.prod.yml"

echo "=========================================="
echo "Depix CRM/ERP Production Deployment"
echo "=========================================="

if [ -f .env ]; then
  echo "--> Loading environment configuration from .env file..."
  export $(grep -v '^#' .env | xargs)
else
  echo "--> Note: No .env file found, using active environment variables or .env.production.example template."
fi

echo "--> Building production containers..."
docker compose -f "$COMPOSE_FILE" build

echo "--> Starting PostgreSQL container..."
docker compose -f "$COMPOSE_FILE" up -d postgres

echo "--> Waiting for PostgreSQL to be healthy..."
RETRIES=30
until [ $RETRIES -le 0 ]; do
  HEALTH=$(docker inspect --format='{{json .State.Health.Status}}' crmerp-prod-postgres 2>/dev/null || echo "\"unhealthy\"")
  if [ "$HEALTH" = "\"healthy\"" ]; then
    echo "--> PostgreSQL is healthy!"
    break
  fi
  echo "    Waiting for PostgreSQL ($RETRIES retries left)..."
  sleep 2
  RETRIES=$((RETRIES - 1))
done

if [ $RETRIES -le 0 ]; then
  echo "ERROR: PostgreSQL failed to become healthy in time."
  exit 1
fi

echo "--> Running EF Core database migrations safely..."
if command -v dotnet-ef &> /dev/null; then
  dotnet ef database update --project src/Host/CrmErp.Host/CrmErp.Host.csproj
else
  echo "--> dotnet-ef CLI not found on host, discovering Docker network name dynamically..."
  NETWORK_NAME=$(docker inspect crmerp-prod-postgres --format='{{range $k, $v := .NetworkSettings.Networks}}{{$k}}{{end}}' 2>/dev/null || echo "crmerp-prod-network")

  echo "--> Running migration inside temporary SDK container connected to network '${NETWORK_NAME}'..."
  docker run --rm \
    --network "${NETWORK_NAME}" \
    -v "$(pwd)":/src \
    -w /src \
    mcr.microsoft.com/dotnet/sdk:8.0 \
    sh -c "dotnet tool install --global dotnet-ef --version 8.0.0 && export PATH=\"\$PATH:/root/.dotnet/tools\" && dotnet ef database update --project src/Host/CrmErp.Host/CrmErp.Host.csproj --connection \"Host=postgres;Port=5432;Database=${POSTGRES_DB:-crm_erp_db};Username=${POSTGRES_USER:-postgres};Password=${POSTGRES_PASSWORD:-postgrespassword}\""
fi

echo "--> Starting remaining production application containers..."
docker compose -f "$COMPOSE_FILE" up -d

echo "--> Verifying system health..."
sleep 5
HEALTH_RESPONSE=$(curl -s http://localhost/health || true)
if [ "$HEALTH_RESPONSE" = "Healthy" ]; then
  echo "--> System is HEALTHY and successfully deployed!"
else
  echo "--> Checking container status..."
  docker compose -f "$COMPOSE_FILE" ps
  echo "--> Deployment health check returned: $HEALTH_RESPONSE"
fi

echo "=========================================="
echo "Deployment Process Complete"
echo "=========================================="
