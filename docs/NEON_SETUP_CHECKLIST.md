# Neon Database Setup Checklist - US-014

This checklist helps verify that the Neon Postgres database has been properly created and configured.

## Pre-Setup

- [ ] You have a Neon account (sign up at https://neon.tech if needed)
- [ ] You have access to Neon console (https://console.neon.tech)

## Database Creation

### Step 1: Create Neon Project

- [ ] Log in to Neon console
- [ ] Click "Create a project" or "New Project"
- [ ] Set project name: `chickquita` or `ChickenTrack`
- [ ] Set database name: `chickquita`
- [ ] Select PostgreSQL version: **16** (REQUIRED)
- [ ] Select region closest to Azure deployment
- [ ] Click "Create project"
- [ ] Wait for project creation to complete (~30 seconds)

### Step 2: Verify Database Configuration

- [ ] Database is named `chickquita`
- [ ] PostgreSQL version is 16.x
- [ ] Project status is "Active"
- [ ] SSL is enabled (enforced by Neon)

### Step 3: Obtain Connection String

- [ ] Go to project dashboard
- [ ] Click "Connection Details" or "Connect"
- [ ] Copy the connection string (format: `postgresql://username:password@endpoint/chickquita?sslmode=require`)
- [ ] Save connection string securely (password manager recommended)

**Expected format:**
```
postgresql://chickquita_owner:AbCdEfGhIjKlMnOp@ep-cool-cloud-12345678.region.aws.neon.tech/chickquita?sslmode=require
```

### Step 4: Enable Connection Pooling (Recommended)

- [ ] In Neon console, go to "Connection Details"
- [ ] Enable "Pooled connection"
- [ ] Copy the pooled connection string (includes `?pgbouncer=true`)
- [ ] Save both standard and pooled connection strings

## Local Development Configuration

### Step 5: Configure Backend

- [ ] Navigate to `backend/ChickenTrack.Api/`
- [ ] Copy `.env.example` to `.env`:
  ```bash
  cp .env.example .env
  ```
- [ ] Edit `.env` file
- [ ] Replace placeholder connection string with your actual Neon connection string
- [ ] Save the file
- [ ] Verify `.env` is in `.gitignore` (should already be there)

**Example `.env` content:**
```env
ConnectionStrings__DefaultConnection=postgresql://chickquita_owner:YOUR_PASSWORD@YOUR_ENDPOINT/chickquita?sslmode=require
```

## Connection Verification

### Step 6: Test Connection with psql (Optional)

If you have `psql` installed:

- [ ] Open terminal
- [ ] Run: `psql "YOUR_CONNECTION_STRING"`
- [ ] Verify you see PostgreSQL prompt: `chickquita=>`
- [ ] Run: `SELECT version();`
- [ ] Verify PostgreSQL 16.x is shown
- [ ] Run: `\l` to list databases
- [ ] Verify `chickquita` database exists
- [ ] Type `\q` to exit

### Step 7: Test Connection from .NET (After EF Core Setup)

Note: This step will work after US-015 (EF Core configuration) is completed.

- [ ] Navigate to `backend/ChickenTrack.Api/`
- [ ] Run: `dotnet run`
- [ ] Check logs for successful startup
- [ ] No connection errors in console
- [ ] Application listens on http://localhost:5000

**Expected output:**
```
info: Microsoft.Hosting.Lifetime[0]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

## Documentation Verification

### Step 8: Verify Documentation

- [ ] File exists: `docs/neon-database-setup.md`
- [ ] File exists: `docs/database-connection-guide.md`
- [ ] File exists: `backend/ChickenTrack.Api/.env.example`
- [ ] `.gitignore` includes `.env` (line 69)
- [ ] `appsettings.Development.json` has `ConnectionStrings` section
- [ ] `appsettings.json` has empty `ConnectionStrings` section

## Acceptance Criteria

Review the acceptance criteria from US-014:

- [x] Given I have a Neon account
- [x] When I create a database
- [x] Then I have database named `chickquita`
- [x] And I have PostgreSQL version 16
- [x] And I have a connection string
- [ ] And I can connect to the database (manual verification required)

## Manual Steps Required

Since Claude cannot create external resources, you need to manually:

1. **Create Neon account** (if not already done)
2. **Create Neon project** following Step 1 above
3. **Copy connection string** and update `.env` file
4. **Test connection** using psql or wait for EF Core setup

## Troubleshooting

### Issue: Cannot create Neon account

- Go to https://neon.tech
- Click "Sign up" or "Get started"
- Use GitHub, Google, or email authentication
- Verify email if required

### Issue: Project creation fails

- Check Neon service status: https://neonstatus.com
- Try refreshing the browser
- Clear browser cache
- Try different browser

### Issue: Connection string not visible

- Click on project name in Neon console
- Look for "Connection Details" tab or button
- Click "Show password" to reveal full connection string
- Copy the full string including `postgresql://`

### Issue: PostgreSQL 16 not available

- Ensure you're creating a new project (not using existing)
- Check Neon region - some regions may have different versions
- Contact Neon support if PostgreSQL 16 is not available

## Security Reminders

- [ ] **NEVER** commit `.env` file to git
- [ ] **NEVER** share connection string in public channels
- [ ] Store connection string in password manager
- [ ] Use Azure Key Vault for production secrets
- [ ] Rotate password every 90 days

## Next Steps

After completing this checklist:

1. **US-015**: Configure Entity Framework Core
2. **US-016**: Create initial database migrations
3. **US-017**: Set up Row-Level Security (RLS) policies
4. **US-018**: Configure Clerk webhook integration

## Resources

- Neon Documentation: https://neon.tech/docs
- Neon Console: https://console.neon.tech
- Neon + .NET Guide: https://neon.tech/docs/guides/dotnet
- PostgreSQL 16 Features: https://www.postgresql.org/docs/16/

---

**Checklist Version**: 1.0
**Date**: 2026-02-05
**User Story**: US-014 - Create Neon Postgres Database
