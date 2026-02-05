# Database Connection Configuration Guide

This guide explains how to configure the Neon Postgres database connection for local development and production deployment.

## Quick Start (Local Development)

### Step 1: Obtain Neon Connection String

Follow the instructions in [neon-database-setup.md](./neon-database-setup.md) to create your Neon database and obtain the connection string.

### Step 2: Configure Local Environment

Create a `.env` file in `backend/ChickenTrack.Api/`:

```bash
cd backend/ChickenTrack.Api
cp .env.example .env
```

Edit `.env` and add your actual Neon connection string:

```env
ConnectionStrings__DefaultConnection=postgresql://your_username:your_password@your_endpoint/chickquita?sslmode=require
```

### Step 3: Verify Configuration

The application will read the connection string in this priority order:

1. **Environment Variable** (highest priority): `ConnectionStrings__DefaultConnection`
2. **appsettings.Development.json**: `ConnectionStrings:DefaultConnection`
3. **appsettings.json**: `ConnectionStrings:DefaultConnection`

For local development, using `.env` file is recommended because:
- It's not committed to git (secure)
- Easy to switch between different databases
- Follows .NET Core conventions

## Configuration Files

### appsettings.json (Production)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  }
}
```

This file is committed to git but should NOT contain actual credentials. Production connection string comes from Azure Key Vault.

### appsettings.Development.json (Local Development)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "postgresql://username:password@endpoint/chickquita?sslmode=require"
  }
}
```

This file contains a placeholder connection string for reference. Actual credentials should be in `.env` file.

### .env File (Local Development - Not Committed)

```env
ConnectionStrings__DefaultConnection=postgresql://actual_username:actual_password@actual_endpoint/chickquita?sslmode=require
```

This file is in `.gitignore` and should contain your real Neon credentials.

## Production Deployment (Azure)

### Azure Key Vault Setup

1. Create Azure Key Vault resource:
```bash
az keyvault create \
  --name chickquita-kv \
  --resource-group chickquita-rg \
  --location westeurope
```

2. Add Neon connection string as secret:
```bash
az keyvault secret set \
  --vault-name chickquita-kv \
  --name NeonConnectionString \
  --value "postgresql://username:password@endpoint/chickquita?sslmode=require"
```

### Azure Container Apps Configuration

Configure the Container App to read from Key Vault:

```bash
az containerapp secret set \
  --name chickquita-app \
  --resource-group chickquita-rg \
  --secrets neon-connection-string=keyvaultref:https://chickquita-kv.vault.azure.net/secrets/NeonConnectionString,identityref:/subscriptions/.../managedIdentities/chickquita-identity
```

Then set environment variable:

```bash
az containerapp update \
  --name chickquita-app \
  --resource-group chickquita-rg \
  --set-env-vars "ConnectionStrings__DefaultConnection=secretref:neon-connection-string"
```

## Connection String Format

### Standard Format

```
postgresql://username:password@endpoint/database?sslmode=require
```

### With Connection Pooling (Recommended for Production)

```
postgresql://username:password@endpoint/database?sslmode=require&pgbouncer=true
```

Connection pooling is recommended for serverless deployments (Azure Container Apps) to improve performance and reduce connection overhead.

## Testing Database Connection

### Using psql Command-Line Tool

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

### Using .NET Application

```bash
cd backend/ChickenTrack.Api
dotnet run
```

If connection is successful, the application should start without errors. You can verify in logs:

```
info: Microsoft.Hosting.Lifetime[0]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

## Troubleshooting

### Issue: Connection Timeout

**Symptom**: Application hangs when starting or throws timeout exception.

**Solutions**:
1. Check Neon project status (may be paused on free tier)
2. Verify connection string is correct
3. Check network connectivity to Neon endpoint
4. Ensure firewall allows outbound connections to Neon

### Issue: SSL Certificate Error

**Symptom**: "The certificate chain was issued by an authority that is not trusted"

**Solutions**:
1. Ensure `sslmode=require` is in connection string
2. Update system CA certificates
3. Try `sslmode=verify-full` for stricter validation

### Issue: Authentication Failed

**Symptom**: "password authentication failed for user"

**Solutions**:
1. Verify username and password are correct
2. Check for special characters in password (may need URL encoding)
3. Regenerate password in Neon console if needed

### Issue: Database Not Found

**Symptom**: "database 'chickquita' does not exist"

**Solutions**:
1. Verify database name in Neon console
2. Ensure connection string uses correct database name
3. Check if database was created during Neon project setup

## Security Best Practices

1. **Never commit `.env` file** - Always in `.gitignore`
2. **Use Azure Key Vault** for production secrets
3. **Rotate passwords regularly** - Change Neon password every 90 days
4. **Use connection pooling** - Reduces connection overhead
5. **Monitor connections** - Use Neon dashboard to track connection usage
6. **Limit IP access** - Configure IP allowlist in Neon (optional)

## Next Steps

After configuring the database connection:

1. Install Entity Framework Core packages
2. Create DbContext class
3. Configure EF Core in `Program.cs`
4. Create initial migrations
5. Apply migrations to create tables

See related documentation:
- [neon-database-setup.md](./neon-database-setup.md) - Database creation guide
- Entity Framework Core setup (to be created in US-015)

---

**Document Version**: 1.0
**Date**: 2026-02-05
**Status**: Ready for use
