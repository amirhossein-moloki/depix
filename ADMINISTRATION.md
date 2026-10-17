# Administration, Registration and Company Visits

## Bootstrap the first administrator

Run this only after PostgreSQL is available and the backend image contains this change. The command applies pending EF Core migrations, creates a new account with the Super Admin role, and ensures a least-privilege Sales Representative role and CRM permissions exist. It refuses to elevate an existing email address.

```bash
read -r -p "Administrator email: " ADMIN_EMAIL
read -r -s -p "Administrator password (12+ characters): " ADMIN_PASSWORD
echo
export ADMIN_EMAIL ADMIN_PASSWORD
docker compose run --rm -e ADMIN_EMAIL -e ADMIN_PASSWORD backend create-admin
unset ADMIN_EMAIL ADMIN_PASSWORD
```

Do not put the password directly in shell command arguments or commit it to a file. Use a unique, strong password.

## Public registration

POST /api/auth/register

```json
{
  "firstName": "Jane",
  "lastName": "Doe",
  "email": "jane@example.com",
  "password": "a-unique-password-with-12-chars",
  "phone": "+1 555 0100"
}
```

The endpoint creates an inactive user with a pending approval record. It does not issue tokens. Passwords are hashed with the application's existing ASP.NET Core PasswordHasher service.

## Review registrations

All endpoints below require a JWT whose role is Super Admin.

- GET /api/admin/users/roles — available non-administrator roles.
- GET /api/admin/users/pending — registrations waiting for review.
- POST /api/admin/users/{userId}/approve with body {"roleName":"Sales Representative"} — approve and assign an existing non-admin role.
- POST /api/admin/users/{userId}/reject — reject and keep the account inactive.

Approval is enforced by both the login and refresh-token paths. Existing users that predate the approval table remain usable; new public registrations require an approved record.

## Business locations and visits

Company creation/update accepts optional latitude and longitude together. Coordinates are range-validated and nullable for existing records.

- GET /api/crm/company-visits/map-points — map-ready companies that have coordinates.
- POST /api/crm/company-visits — create a visit report. The seller ID is taken from the authenticated token, not from request input.
- GET /api/crm/company-visits/company/{companyId} — visit history for a company.

Example visit body:

```json
{
  "companyId": "00000000-0000-0000-0000-000000000000",
  "visitedAt": "2026-10-17T10:00:00Z",
  "notes": "Discussed current needs and requested a follow-up next week.",
  "outcome": "FOLLOW_UP",
  "followUpAt": "2026-10-24T10:00:00Z",
  "latitude": 40.4093,
  "longitude": 49.8671
}
```

The backend provides map points and visit APIs. Rendering an interactive map and registration/approval screens requires the CRM frontend application to consume these endpoints.
