---
name: sporthub-debug
description: >
  Diagnoses and fixes common SportHub runtime failures: API errors, migration problems, tenant resolution failures, Redis issues, Docker problems, and auth failures.
  Use this skill whenever something is broken or not working in SportHub — even if the user just says "tá quebrando", "deu erro", "não tá funcionando", "500 na API", "migration falhou", "tenant não resolve", "redis tá estranho", "docker não sobe". Trigger any time there's an error, unexpected behavior, or the user asks why something isn't working.
---

# SportHub Debug

Structured diagnostic playbook for SportHub failures. Follow the section that matches the symptom.

## Quick environment check

Run these first for any problem — they tell you the state of the stack:

```bash
# Are all containers up?
docker compose ps

# Recent API logs (last 50 lines)
docker compose logs api --tail=50

# Check DB connectivity
docker compose exec db psql -U postgres -d SportHubDb -c "SELECT 1"

# Check Redis
docker compose exec redis redis-cli ping
```

---

## 1. API returns 500

**Step 1:** Get the full error from logs:
```bash
docker compose logs api --tail=100 | grep -A 10 "ERROR\|Exception\|fail"
```

**Step 2:** Check common causes:

| Symptom in logs | Cause | Fix |
|---|---|---|
| `NpgsqlException` / `connection refused` | DB container not healthy | `docker compose restart db` then `docker compose restart api` |
| `RedisConnectionException` | Redis not reachable | `docker compose restart redis` then `docker compose restart api` |
| `InvalidOperationException: No service for type` | Missing DI registration | Add `builder.Services.AddScoped<I{X}, {X}>()` in `ServiceExtensions.cs` |
| `KeyNotFoundException` in middleware | Tenant resolution failed | See section 3 |
| `FluentValidation` errors in 422 | Validation failure, not a bug | Check request body against validator rules |

---

## 2. Migration failures

**"Unable to create an object of type 'ApplicationDbContext'"**
```bash
# Fix: build the project first
dotnet build
# If build fails, fix compile errors before retrying the migration
```

**"The entity type 'X' requires a primary key"**
- Open `src/SportHub.Infrastructure/Persistence/Configurations/{Entity}Configuration.cs`
- Add: `builder.HasKey(e => e.Id);`

**"Table already exists" / out-of-sync DB**
```bash
# Check which migrations are applied
dotnet ef migrations list --project src/SportHub.Infrastructure --startup-project src/SportHub.Api

# Check what's in the DB
docker compose exec db psql -U postgres -d SportHubDb -c "SELECT * FROM \"__EFMigrationsHistory\" ORDER BY \"MigrationId\" DESC LIMIT 10"
```

**Migration creates unexpected columns**
- Check `AppDbContext.cs` for stray `DbSet<>` entries
- Check that `{Entity}Configuration.cs` only maps what the entity actually has

**Pre-flight before any migration:**
1. `AppDbContext.cs` has `DbSet<{Entity}>`
2. `{Entity}Configuration.cs` exists in Configurations/
3. `dotnet build` passes
4. Run: `dotnet ef migrations add {Name} --project src/SportHub.Infrastructure --startup-project src/SportHub.Api`

---

## 3. Tenant not resolving (401 / tenant context null)

The `TenantResolutionMiddleware` resolves tenant from subdomain first, then from `X-Tenant-Slug` header.

**Check 1:** Is the header being sent?
```bash
# Test with curl
curl -v http://localhost:5001/api/courts -H "X-Tenant-Slug: your-slug"
```

**Check 2:** Does the tenant exist in DB?
```bash
docker compose exec db psql -U postgres -d SportHubDb -c "SELECT \"Id\", \"Slug\", \"Status\" FROM \"Tenants\""
```

**Check 3:** Is the tenant cached correctly in Redis?
```bash
docker compose exec redis redis-cli keys "*tenant*"
docker compose exec redis redis-cli get "<key>"
```

**Check 4:** Is the route bypassing tenant resolution?
Routes that bypass: `/api/tenants/**`, `/auth/register`, `/auth/me`, `/health`, `/scalar/**`, `/openapi/**`, `/hubs/**`
Routes with optional tenant: `/auth/login`, `/auth/refresh`, `/api/branding`

**Fix for stale tenant cache:**
```bash
docker compose exec redis redis-cli flushdb
# or selectively:
docker compose exec redis redis-cli del "<specific-key>"
```

---

## 4. Auth / JWT failures

**401 on protected endpoint:**
1. Check that `Authorization: Bearer <token>` header is present
2. Check token hasn't expired: decode at jwt.io
3. Check the endpoint has `.RequireAuthorization(...)` wired correctly
4. Check the correct policy name is used (`IsStaff`, `IsManager`, `IsOwner`, `IsSuperAdmin`)

**403 Forbidden:**
- User's role is too low for the policy
- Check user role in DB:
```bash
docker compose exec db psql -U postgres -d SportHubDb -c "SELECT \"Email\", \"Role\" FROM \"Users\" WHERE \"Email\" = 'user@example.com'"
```

**"No tenant found" on login:**
- `/auth/login` has optional tenant — it won't block, but the returned token won't have `EstablishmentRole` if no tenant was resolved
- Ensure `X-Tenant-Slug` is sent on login requests when you need tenant-specific role claims

---

## 5. Docker / container issues

**API container keeps restarting:**
```bash
docker compose logs api --tail=50
# Look for startup exception — usually a missing env var or DB not ready
```

**DB not healthy:**
```bash
docker compose logs db --tail=30
# Common fix:
docker compose down -v  # WARNING: destroys DB data
docker compose up -d
```

**Port already in use (5001, 5432, etc.):**
```bash
lsof -i :5001
# kill the PID shown, then restart
```

**Rebuild API after code changes:**
```bash
docker compose up -d --build api
```

---

## 6. Frontend not reaching API

**CORS / network errors in browser:**
- Confirm `NEXT_PUBLIC_API_URL` in `apps/storefront/.env.local` points to `http://localhost:5001`
- Confirm API is running: `curl http://localhost:5001/health`

**`X-Tenant-Slug` not sent:**
- The axios interceptor in `api.ts` auto-resolves slug from: setTenantSlug() → subdomain → first URL path segment
- If all fail, slug is `null` and header is omitted — protected tenant routes will 404/401

**401 loop (refresh failing):**
- Check `/api/auth/refresh` Next.js route in `apps/storefront/src/app/api/auth/`
- Clear cookies and localStorage, then re-login

---

## 7. Redis cache issues

**Stale data being returned:**
```bash
# Clear all SportHub keys
docker compose exec redis redis-cli keys "SportHub*" | xargs docker compose exec -T redis redis-cli del
```

**Cache not being populated:**
- Confirm the Query handler injects `ICacheService` and calls `SetAsync` after the DB fetch
- Confirm the `CacheKeyPrefix` entry exists in the enum

**Cache not being invalidated after mutation:**
- The Command handler must call `_cache.RemoveAsync(key, ct)` after `SaveChangesAsync`
- Confirm the key uses the same `CacheKeyPrefix` and same args as the Query handler that populated it

---

## 8. Useful one-liners

```bash
# Tail all logs live
docker compose logs -f

# Check migrations status
dotnet ef migrations list --project src/SportHub.Infrastructure --startup-project src/SportHub.Api

# Rebuild and restart only the API (fastest iteration cycle)
docker compose up -d --build api && docker compose logs api -f

# Reset Redis completely
docker compose exec redis redis-cli flushall

# List all tenants
docker compose exec db psql -U postgres -d SportHubDb -c "SELECT \"Slug\", \"Status\", \"Name\" FROM \"Tenants\""

# Check pending migrations
docker compose exec db psql -U postgres -d SportHubDb -c "SELECT \"MigrationId\" FROM \"__EFMigrationsHistory\" ORDER BY \"MigrationId\""
```
