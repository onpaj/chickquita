# GitHub Actions CI/CD Pipeline

This directory contains the GitHub Actions workflows for the ChickenTrack (Chickquita) project.

## Workflows

### CI/CD Pipeline (`ci-cd.yml`)

Main continuous integration and deployment pipeline that runs on pushes and pull requests to `main` and `develop` branches.

#### Pipeline Stages

1. **Backend Tests** (`backend-tests`)
   - Runs .NET backend tests
   - Uses .NET 8.0
   - Executes all test projects in the solution
   - Uploads test results as artifacts

2. **Frontend Tests** (`frontend-tests`)
   - Runs frontend linting
   - Performs TypeScript type checking
   - Builds the frontend application
   - Uploads build artifacts

3. **Docker Build** (`docker-build`)
   - Builds the multi-stage Docker image
   - Runs only after both test jobs pass
   - Uses Docker BuildKit with caching
   - Does not push the image

4. **Docker Push** (`docker-push`) - **Main branch only**
   - Pushes Docker image to GitHub Container Registry (ghcr.io)
   - Only runs on `main` branch pushes
   - Tags images with:
     - Branch name
     - Git SHA
     - `latest` tag for default branch
   - Generates SLSA attestation for supply chain security

#### Environment Variables

- `DOTNET_VERSION`: .NET SDK version (8.0.x)
- `NODE_VERSION`: Node.js version (20.x)
- `DOCKER_REGISTRY`: Container registry URL (ghcr.io)
- `IMAGE_NAME`: Full image name (repository name)

#### Permissions

The `docker-push` job requires:
- `contents: read` - To checkout code
- `packages: write` - To push to GitHub Container Registry

#### Secrets

The workflow uses the built-in `GITHUB_TOKEN` secret for:
- Authenticating to GitHub Container Registry
- Uploading artifacts
- Creating attestations

No additional secrets need to be configured.

#### Artifacts

The workflow produces the following artifacts:
- **backend-test-results**: Test result files (.trx format)
- **frontend-build**: Compiled frontend application

#### Running Locally

To test the individual steps locally:

```bash
# Backend tests
cd backend
dotnet restore Chickquita.slnx
dotnet build Chickquita.slnx --configuration Release
dotnet test Chickquita.slnx --configuration Release

# Frontend tests
cd frontend
npm ci
npm run lint
npx tsc --noEmit
npm run build

# Docker build
docker build -t chickquita:local .
```

#### Caching

- **Node.js**: Uses npm cache for faster dependency installation
- **Docker**: Uses GitHub Actions cache for Docker layer caching

#### Future Enhancements

Potential improvements for the pipeline:
- [ ] Add code coverage reporting
- [ ] Add security scanning (Snyk, Trivy)
- [ ] Add E2E tests with Playwright
- [ ] Add deployment to Azure Container Apps
- [ ] Add staging environment deployment
- [ ] Add performance testing
