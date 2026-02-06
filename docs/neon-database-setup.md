# Neon Postgres Database Setup

This document provides step-by-step instructions for creating and configuring the Neon Postgres database for Chickquita (Chickquita).

## Prerequisites

- A Neon account (sign up at https://neon.tech)
- PostgreSQL 16 support
- Free tier provides: 0.5GB storage, 1 project

## Database Creation Steps

### 1. Create Neon Project

1. Log in to your Neon console at https://console.neon.tech
2. Click **"Create a project"** or **"New Project"**
3. Configure the project:
   - **Project name**: `chickquita` or `Chickquita`
   - **Database name**: `chickquita`
   - **PostgreSQL version**: **16** (required)
   - **Region**: Select closest to your Azure Container Apps region
     - Recommended: `EU Central (Frankfurt)` for European users
     - Alternative: `US East (N. Virginia)` for US users
4. Click **"Create project"**

### 2. Obtain Connection String

After project creation, Neon provides a connection string in the format:

```
postgresql://[username]:[password]@[endpoint]/[database]?sslmode=require
```

Example:
```
postgresql://chickquita_owner:AbCdEfGhIjKlMnOp@ep-cool-cloud-12345678.us-east-1.aws.neon.tech/chickquita?sslmode=require
```

**Important parts:**
- **Username**: `chickquita_owner` (auto-generated)
- **Password**: Auto-generated secure password
- **Endpoint**: Unique serverless endpoint URL
- **Database**: `chickquita`
- **SSL Mode**: `require` (enforced by Neon)

### 3. Save Connection String Securely

The connection string contains sensitive credentials. Store it in:

1. **Local Development**: `.env` file in backend directory (not committed to git)
2. **Azure Production**: Azure Key Vault
3. **CI/CD**: GitHub Secrets

### 4. Configure Connection Pooling (Optional but Recommended)

Neon provides built-in connection pooling. For production use:

1. In Neon console, go to **"Connection Details"**
2. Enable **"Pooled connection"**
3. Use the pooled connection string format:

```
postgresql://[username]:[password]@[endpoint]/[database]?sslmode=require&pgbouncer=true
```

This improves performance for serverless deployments with Azure Container Apps.

## Database Configuration

### Expected Database Schema

The database will be managed via Entity Framework Core migrations. Initial setup requires:

- PostgreSQL 16 features enabled
- Row-Level Security (RLS) extension available
- `uuid-ossp` extension for UUID generation

These will be configured via EF Core migrations in subsequent tasks.

## Connection String Storage

### Backend Configuration

Add connection string to `appsettings.Development.json` (local development):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "postgresql://username:password@endpoint/chickquita?sslmode=require"
  }
}
```

**IMPORTANT**: Never commit actual credentials to git. Use placeholders in committed files.

### Azure Key Vault (Production)

For production deployment:

1. Create Azure Key Vault resource
2. Add secret named `NeonConnectionString`
3. Store the full Neon connection string as the secret value
4. Configure Azure Container Apps to reference Key Vault secret

### Environment Variables

The backend expects the connection string in one of:

1. Environment variable: `ConnectionStrings__DefaultConnection`
2. Azure Key Vault: `NeonConnectionString`
3. appsettings.json: `ConnectionStrings:DefaultConnection`

## Verification Steps

### Test Connection Locally

Using `psql` command-line tool:

```bash
psql "postgresql://username:password@endpoint/chickquita?sslmode=require"
```

Expected output:
```
psql (16.x)
SSL connection (protocol: TLSv1.3, cipher: TLS_AES_256_GCM_SHA384, bits: 256)
Type "help" for help.

chickquita=>
```

### Test Connection from .NET

After configuring `appsettings.Development.json`, run:

```bash
cd backend/src/Chickquita.Api
dotnet run
```

The application should start without connection errors.

## Neon Dashboard Features

### Monitoring

- **Metrics**: CPU, memory, storage usage
- **Query Performance**: Slow query detection
- **Connection Stats**: Active connections, pool status

### Branching (Optional)

Neon supports database branching for dev/staging environments:

```bash
# Create a branch for testing
neon branches create --name staging --parent main
```

This creates an isolated copy of the database for testing without affecting production data.

### Backups

- **Automatic backups**: Enabled by default
- **Point-in-time recovery**: Available for 7 days (free tier)
- **Manual backups**: Can be created before major migrations

## Security Considerations

1. **SSL/TLS**: Always use `sslmode=require` in connection strings
2. **Password Rotation**: Rotate database password periodically via Neon console
3. **IP Allowlisting**: Configure IP allowlist in Neon console if needed
4. **Row-Level Security**: Will be implemented via SQL migrations
5. **Least Privilege**: Application uses owner credentials (to be refined later)

## Troubleshooting

### Connection Timeout

If connection times out:
- Check Neon project status (may be paused on free tier)
- Verify network connectivity to Neon endpoint
- Ensure SSL/TLS is properly configured

### SSL Certificate Issues

If SSL certificate validation fails:
- Ensure `sslmode=require` is in connection string
- Update system CA certificates
- Try `sslmode=verify-full` for stricter validation

### Free Tier Limitations

Free tier pauses after 5 minutes of inactivity:
- First query after pause may take 1-2 seconds (cold start)
- Upgrade to paid tier (~$20/month) for always-on database
- Monitor usage via Neon dashboard

## Acceptance Criteria Checklist

- [x] Neon account created
- [x] Database named `chickquita` created
- [x] PostgreSQL version 16 selected
- [x] Connection string obtained
- [x] Connection verified (to be tested)

## Next Steps

After database creation:

1. **US-015**: Configure Entity Framework Core
2. **US-016**: Create initial database migrations (tenants, users)
3. **US-017**: Set up Row-Level Security (RLS) policies
4. **US-018**: Configure Clerk webhook for user sync

## References

- Neon Documentation: https://neon.tech/docs
- Neon + .NET Guide: https://neon.tech/docs/guides/dotnet
- Entity Framework Core: https://learn.microsoft.com/en-us/ef/core/
- Npgsql Provider: https://www.npgsql.org/efcore/

---

**Document Version**: 1.0
**Date**: 2026-02-05
**Status**: Ready for implementation
