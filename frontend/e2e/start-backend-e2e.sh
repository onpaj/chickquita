#!/bin/bash

# Script to start the backend API in E2ETests mode
# This script is used by E2E tests to ensure the correct database connection is used

echo "Starting Chickquita API in E2ETests mode..."
cd "$(dirname "$0")/../../backend/src/Chickquita.Api"

# Set the environment to E2ETests
export ASPNETCORE_ENVIRONMENT=E2ETests

# Run the API
dotnet run --no-launch-profile
