# Multi-stage Dockerfile for ChickenTrack
# Builds frontend (React/Vite) and backend (.NET 8 API) into a single container

# Stage 1: Build frontend
FROM node:20-alpine AS frontend-build

WORKDIR /app/frontend

# Copy package files and install dependencies
COPY frontend/package*.json ./
RUN npm ci --prefer-offline --no-audit

# Copy frontend source code
COPY frontend/ ./

# Build frontend for production
RUN npm run build

# Stage 2: Build backend
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build

WORKDIR /app/backend

# Copy solution and project files
COPY backend/*.sln* ./
COPY backend/ChickenTrack.Api/ChickenTrack.Api.csproj ./ChickenTrack.Api/
COPY backend/ChickenTrack.Application/ChickenTrack.Application.csproj ./ChickenTrack.Application/
COPY backend/ChickenTrack.Domain/ChickenTrack.Domain.csproj ./ChickenTrack.Domain/
COPY backend/ChickenTrack.Infrastructure/ChickenTrack.Infrastructure.csproj ./ChickenTrack.Infrastructure/

# Restore dependencies
RUN dotnet restore ChickenTrack.Api/ChickenTrack.Api.csproj

# Copy backend source code
COPY backend/ ./

# Copy frontend build output to backend wwwroot
COPY --from=frontend-build /app/frontend/dist ./ChickenTrack.Api/wwwroot/

# Build backend
RUN dotnet publish ChickenTrack.Api/ChickenTrack.Api.csproj \
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
ENTRYPOINT ["dotnet", "ChickenTrack.Api.dll"]
