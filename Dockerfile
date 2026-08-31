# syntax=docker/dockerfile:1.7

# ============================================================
# Runtime base image
# Keep this stage stable so OS/package layers can be reused
# across deployments even when application build metadata changes.
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
# Build stage
# Restore once, then publish without triggering another restore.
# ============================================================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["QuilvianSystemBackend.csproj", "./"]

RUN --mount=type=cache,id=nuget-v2,target=/root/.nuget/packages,sharing=locked \
    dotnet restore "QuilvianSystemBackend.csproj"

COPY . .

RUN --mount=type=cache,id=nuget-v2,target=/root/.nuget/packages,sharing=locked \
    dotnet publish "QuilvianSystemBackend.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false \
    /p:DebugSymbols=false \
    /p:DebugType=None \
    /p:RunAnalyzers=false \
    /p:ContinuousIntegrationBuild=true \
    --verbosity minimal


# ============================================================
# Final image
# Dynamic build metadata belongs here so it does not invalidate
# the expensive runtime dependency layer above.
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
