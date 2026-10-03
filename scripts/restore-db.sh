#!/usr/bin/env bash
set -e

CONTAINER_NAME="crmerp-prod-postgres"
DB_USER="${POSTGRES_USER:-postgres}"
DB_NAME="${POSTGRES_DB:-crm_erp_db}"

if [ -z "$1" ]; then
  echo "Usage: $0 <path-to-backup-file.sql.gz>"
  exit 1
fi

BACKUP_FILE="$1"

if [ ! -f "$BACKUP_FILE" ]; then
  echo "ERROR: Backup file '$BACKUP_FILE' does not exist."
  exit 1
fi

echo "=========================================="
echo "Restoring PostgreSQL Database Backup"
echo "=========================================="
echo "Container:   ${CONTAINER_NAME}"
echo "Database:    ${DB_NAME}"
echo "Backup File: ${BACKUP_FILE}"

if ! docker ps -q -f name="${CONTAINER_NAME}" | grep -q .; then
  echo "ERROR: Container ${CONTAINER_NAME} is not running!"
  exit 1
fi

echo "--> Restoring database schema and data..."
gunzip -c "${BACKUP_FILE}" | docker exec -i "${CONTAINER_NAME}" pg_restore -U "${DB_USER}" -d "${DB_NAME}" --clean --if-exists --no-owner --role="${DB_USER}" || true

echo "--> Database restore complete!"
echo "=========================================="
