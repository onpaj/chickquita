# Multi-stage Dockerfile for Chickquita
# Builds frontend (React/Vite) and backend (.NET 8 API) into a single container

# Stage 1: Build frontend
FROM node:20-alpine AS frontend-build

WORKDIR /app/frontend

# Build args for Vite environment variables (injected at build time)
ARG VITE_API_URL=http://localhost:8080
ARG VITE_CLERK_PUBLISHABLE_KEY=

ENV VITE_API_URL=$VITE_API_URL
ENV VITE_CLERK_PUBLISHABLE_KEY=$VITE_CLERK_PUBLISHABLE_KEY

# Copy package files and install dependencies
COPY frontend/package*.json ./
RUN npm ci --prefer-offline --no-audit

# Copy frontend source code
COPY frontend/ ./

# Build frontend for production (lint and type-check run in CI before this)
RUN npm run build

# Stage 2: Build backend
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build

WORKDIR /app/backend

# Copy project files for layer caching
COPY backend/src/Chickquita.Api/Chickquita.Api.csproj ./src/Chickquita.Api/
COPY backend/src/Chickquita.Application/Chickquita.Application.csproj ./src/Chickquita.Application/
COPY backend/src/Chickquita.Domain/Chickquita.Domain.csproj ./src/Chickquita.Domain/
COPY backend/src/Chickquita.Infrastructure/Chickquita.Infrastructure.csproj ./src/Chickquita.Infrastructure/

# Restore dependencies
RUN dotnet restore src/Chickquita.Api/Chickquita.Api.csproj

# Copy backend source code
COPY backend/src/ ./src/

# Copy frontend build output to backend wwwroot
COPY --from=frontend-build /app/frontend/dist ./src/Chickquita.Api/wwwroot/

# Build backend
RUN dotnet publish src/Chickquita.Api/Chickquita.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Stage 3: Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime

WORKDIR /app

# Copy published backend with embedded frontend
COPY --from=backend-build /app/publish ./

# Create a non-root user
RUN addgroup -g 1000 appgroup && \
    adduser -u 1000 -G appgroup -s /bin/sh -D appuser && \
    chown -R appuser:appgroup /app

USER appuser

# Expose port 8080 (non-privileged port)
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

# Start the application
ENTRYPOINT ["dotnet", "Chickquita.Api.dll"]