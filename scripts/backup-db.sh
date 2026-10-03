#!/usr/bin/env bash
set -e

BACKUP_DIR="${BACKUP_DIR:-./backups}"
CONTAINER_NAME="crmerp-prod-postgres"
DB_USER="${POSTGRES_USER:-postgres}"
DB_NAME="${POSTGRES_DB:-crm_erp_db}"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
BACKUP_FILE="${BACKUP_DIR}/crmerp_backup_${TIMESTAMP}.sql.gz"

mkdir -p "$BACKUP_DIR"

echo "=========================================="
echo "Creating PostgreSQL Database Backup"
echo "=========================================="
echo "Container: ${CONTAINER_NAME}"
echo "Database:  ${DB_NAME}"
echo "Target:    ${BACKUP_FILE}"

if ! docker ps -q -f name="${CONTAINER_NAME}" | grep -q .; then
  echo "ERROR: Container ${CONTAINER_NAME} is not running!"
  exit 1
fi

docker exec -t "${CONTAINER_NAME}" pg_dump -U "${DB_USER}" -d "${DB_NAME}" -F c | gzip > "${BACKUP_FILE}"

if [ -s "${BACKUP_FILE}" ]; then
  echo "--> Backup completed successfully! File size: $(du -h "${BACKUP_FILE}" | cut -f1)"
else
  echo "ERROR: Backup file is empty or was not created successfully."
  rm -f "${BACKUP_FILE}"
  exit 1
fi

echo "=========================================="
