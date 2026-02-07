# ── Build stage ──────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first (layer caching for restore)
COPY TaskManagementSystem.sln .
COPY TaskManagementSystem.Domain/TaskManagementSystem.Domain.csproj TaskManagementSystem.Domain/
COPY TaskManagementSystem.Infrastructure/TaskManagementSystem.Infrastructure.csproj TaskManagementSystem.Infrastructure/
COPY TaskManagementSystem.Features/TaskManagementSystem.Features.csproj TaskManagementSystem.Features/
COPY TaskManagementSystem.Api/TaskManagementSystem.Api.csproj TaskManagementSystem.Api/
COPY TaskManagementSystem.Tests/TaskManagementSystem.Tests.csproj TaskManagementSystem.Tests/

RUN dotnet restore

# Copy everything and build
COPY . .
RUN dotnet build -c Release --no-restore

# ── Test stage ───────────────────────────────────────────────
FROM build AS test
RUN dotnet test -c Release --no-build --verbosity normal

# ── Publish stage ────────────────────────────────────────────
FROM build AS publish
RUN dotnet publish TaskManagementSystem.Api/TaskManagementSystem.Api.csproj \
    -c Release --no-build -o /app/publish

# ── Runtime stage ────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user
RUN addgroup --system --gid 1001 appgroup && \
    adduser --system --uid 1001 --ingroup appgroup appuser

# Create directory for SQLite database with correct ownership
RUN mkdir -p /app/data && chown appuser:appgroup /app/data

COPY --from=publish /app/publish .

# Set ownership of app files
RUN chown -R appuser:appgroup /app

USER appuser

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/taskmanagement.db"

ENTRYPOINT ["dotnet", "TaskManagementSystem.Api.dll"]
