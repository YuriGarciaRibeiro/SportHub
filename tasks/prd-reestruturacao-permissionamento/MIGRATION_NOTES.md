# Migration Notes - Permission System Refactoring

## Overview
This document contains important notes about the permission system refactoring migration.

## Database Migration

### EF Core Migration
- **Migration Name**: `RefactorUserRoleEnum`
- **Created**: Task 1.0
- **Status**: ✅ Created, pending application
- **Location**: `src/SportHub.Infrastructure/Persistence/Migrations/`

### Data Migration
- **Script**: `MigrateUserRolesData.sql`
- **Purpose**: Update existing user roles from old enum values to new values
- **Status**: ✅ Created, pending execution

## Enum Changes

### Old UserRole Enum
```csharp
public enum UserRole
{
    User = 0,
    EstablishmentMember = 1,
    Admin = 2,
    SuperAdmin = 99
}
```

### New UserRole Enum
```csharp
public enum UserRole
{
    Customer = 0,      // Was: User
    Staff = 1,         // Was: EstablishmentMember
    Manager = 2,       // NEW
    Owner = 3,         // Was: Admin (value changed from 2 to 3)
    SuperAdmin = 99    // Unchanged
}
```

### EstablishmentRole Enum
- **Status**: ✅ REMOVED
- **Replaced by**: UserRole (unified enum)

## Migration Steps

### 1. Apply EF Core Migration
```bash
cd src/SportHub.Api
dotnet ef database update --context ApplicationDbContext
```

### 2. Run Data Migration Script
```bash
psql -h localhost -U postgres -d sporthub -f src/SportHub.Infrastructure/Persistence/Migrations/MigrateUserRolesData.sql
```

### 3. Verify Migration
```sql
-- Check role distribution across all tenant schemas
SELECT nspname as schema, "Role", COUNT(*) as total
FROM pg_namespace 
CROSS JOIN LATERAL (
    SELECT "Role" FROM <schema>."Users" WHERE "IsDeleted" = false
) u
WHERE nspname NOT IN ('pg_catalog', 'information_schema', 'public')
AND nspname NOT LIKE 'pg_%'
GROUP BY nspname, "Role"
ORDER BY nspname, "Role";
```

## Breaking Changes

### Frontend Impact
The frontend will need to update role handling:
- `User` → `Customer`
- `Admin` → `Owner`
- New roles: `Staff`, `Manager`

### JWT Tokens
- **Impact**: Existing JWT tokens will be invalidated
- **Action**: Users will need to re-login
- **Reason**: Role claim values have changed

### API Responses
- `AuthResponse` now includes `Role` field
- Role values in responses: `"Customer"`, `"Staff"`, `"Manager"`, `"Owner"`, `"SuperAdmin"`

## New Features

### Member Management Endpoints
- `GET /api/members` - List operational members (Staff+)
- `PUT /api/members/{id}/role` - Update member role

### Authorization Policies
- `IsStaff` - Requires Staff or above
- `IsManager` - Requires Manager or above
- `IsOwner` - Requires Owner
- `IsSuperAdmin` - Requires SuperAdmin

### Endpoint Permissions Matrix

| Endpoint | Method | Policy |
|---|---|---|
| `/api/sports` | GET | Anonymous |
| `/api/sports` | POST/PUT/DELETE | IsManager |
| `/api/courts` | GET | Anonymous |
| `/api/courts` | POST/PUT/DELETE | IsManager |
| `/api/courts/{id}/reservations` | GET | IsStaff |
| `/api/reservations/me` | GET | RequireAuth |
| `/api/members` | GET/PUT | IsOwner |
| `/api/settings` | PUT | IsOwner |
| `/admin/stats` | GET | IsManager |

## Rollback Plan

If migration fails:
1. Restore database backup
2. Revert code changes: `git revert <commit-hash>`
3. Redeploy previous version

## Testing Checklist

- [x] Unit tests pass
- [ ] Integration tests pass
- [ ] Manual testing of all endpoints
- [ ] Verify role-based access control
- [ ] Test member management endpoints
- [ ] Verify JWT tokens contain correct role
- [ ] Test frontend integration

## Deployment Notes

1. **Backup database** before applying migration
2. Apply migration during maintenance window
3. Monitor error logs after deployment
4. Verify all users can login successfully
5. Test critical user flows

## Support

For issues or questions, contact the development team.
