# syntax=docker/dockerfile:1.7

# ============================================================
# Runtime base
# Stable runtime dependencies. These layers should be reusable
# by GitHub BuildKit once a successful build exports the cache.
# ============================================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:80
ENV LD_LIBRARY_PATH=/opt/piper:${LD_LIBRARY_PATH}

EXPOSE 80

RUN set -eux; \
    apt-get update \
      -o Acquire::Retries=5 \
      -o Acquire::http::Timeout=30 \
      -o Acquire::https::Timeout=30; \
    apt-get install -y --no-install-recommends \
      -o Acquire::Retries=5 \
      ca-certificates \
      ffmpeg \
      espeak-ng \
      libespeak-ng1; \
    mkdir -p \
      /opt/piper \
      /app/Storage/PiperVoices/id_ID \
      /app/Storage/QueueVoiceCache; \
    chmod -R 755 /opt/piper; \
    chmod -R 775 /app/Storage; \
    rm -rf /var/lib/apt/lists/*


# ============================================================
# Restore
#
# Deliberately use a normal Docker layer instead of a BuildKit
# cache mount. The previous Dockerfile stalled on a RUN using:
#   --mount=type=cache,...,sharing=locked
#
# With the project file copied first, Docker/GHA layer caching
# can still reuse this restore layer while keeping the publish
# step independent from a locked NuGet cache mount.
# ============================================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS restore
WORKDIR /src

ENV DOTNET_CLI_TELEMETRY_OPTOUT=1
ENV DOTNET_NOLOGO=1

COPY ["QuilvianSystemBackend.csproj", "./"]

RUN dotnet restore "QuilvianSystemBackend.csproj" \
    --verbosity minimal


# ============================================================
# Build
#
# Build and publish are intentionally separate so GitHub Actions
# logs show whether a future slowdown is in compilation or in the
# publish packaging phase.
# ============================================================
FROM restore AS build

COPY . .

RUN dotnet build "QuilvianSystemBackend.csproj" \
    -c Release \
    --no-restore \
    /p:ExcludeEfMigrationMetadata=true \
    /p:UseAppHost=false \
    /p:DebugSymbols=false \
    /p:DebugType=None \
    /p:RunAnalyzers=false \
    /p:ContinuousIntegrationBuild=true \
    /p:UseSharedCompilation=false \
    --verbosity minimal

RUN dotnet publish "QuilvianSystemBackend.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    --no-build \
    /p:ExcludeEfMigrationMetadata=true \
    /p:UseAppHost=false \
    /p:DebugSymbols=false \
    /p:DebugType=None \
    /p:RunAnalyzers=false \
    /p:ContinuousIntegrationBuild=true \
    --verbosity minimal


# ============================================================
# Final image
# Dynamic build metadata stays in the final stage so it does not
# invalidate restore/build/runtime dependency layers.
# ============================================================
FROM base AS final
WORKDIR /app

ARG APP_BUILD_VERSION
ARG APP_BUILD_NUMBER=0
ARG APP_COMMIT_SHA=unknown
ARG APP_BRANCH
ARG APP_BUILD_DATE

ENV APP_BUILD_VERSION=${APP_BUILD_VERSION}
ENV APP_BUILD_NUMBER=${APP_BUILD_NUMBER}
ENV APP_COMMIT_SHA=${APP_COMMIT_SHA}
ENV APP_BRANCH=${APP_BRANCH}
ENV APP_BUILD_DATE=${APP_BUILD_DATE}

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "QuilvianSystemBackend.dll"]
