# Identity Module Documentation

## Overview
The Identity Module provides production-ready authentication and permission-based authorization for the CRM/ERP Modular Monolith system.

## Features
1. **User & Role Management**: Domain model with `User`, `Role`, `Permission`, `RolePermission`, and `RefreshToken`.
2. **Password Hashing**: Uses ASP.NET Identity `PasswordHasher<User>` to hash passwords securely before storage.
3. **JWT Access Tokens**: Issues short-lived JWT tokens signed with SHA-256 containing user claims, roles, and permissions.
4. **Refresh Token Rotation**: Secure refresh tokens stored as SHA-256 hashes in PostgreSQL. Automatically revoked and rotated upon renewal.
5. **Permission-Based Authorization**: Custom ASP.NET Core authorization handler (`PermissionAuthorizationHandler`) and `[Permission("...")]` attribute evaluating permissions dynamically.

## Endpoints
- `POST /api/auth/login`: Authenticate email and password, returning AccessToken and RefreshToken.
- `POST /api/auth/refresh`: Validate and rotate RefreshToken, returning a new AccessToken and RefreshToken.
- `POST /api/auth/logout`: Revoke active RefreshToken.
- `GET /api/auth/me`: Retrieve profile, roles, and permissions of the currently authenticated user.

## Configuration (`appsettings.json`)
```json
"Jwt": {
  "Secret": "Development_Placeholder_Jwt_Secret_Key_Change_In_Production_32_Bytes!",
  "Issuer": "CrmErpApi",
  "Audience": "CrmErpClients",
  "AccessTokenExpirationMinutes": 60,
  "RefreshTokenExpirationDays": 7
}
```

## Adding New Permissions
1. Define new permission codes in domain or database (e.g. `CRM.Lead.Create`, `Sales.Proposal.Create`).
2. Assign permissions to roles via `RolePermission` entities.
3. Protect API endpoints using `[Permission("CRM.Lead.Create")]`.
