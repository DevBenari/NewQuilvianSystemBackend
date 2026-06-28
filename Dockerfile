# syntax=docker/dockerfile:1.7

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:80
ENV LD_LIBRARY_PATH=/opt/piper:${LD_LIBRARY_PATH}

EXPOSE 80

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        ca-certificates \
        ffmpeg \
        espeak-ng \
        libespeak-ng1 \
    && mkdir -p \
        /opt/piper \
        /app/Storage/PiperVoices/id_ID \
        /app/Storage/QueueVoiceCache \
    && chmod -R 755 /opt/piper \
    && chmod -R 775 /app/Storage \
    && rm -rf /var/lib/apt/lists/*


FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY ["QuilvianSystemBackend.csproj", "./"]

RUN --mount=type=cache,id=nuget-v2,target=/root/.nuget/packages,sharing=locked \
    dotnet restore "QuilvianSystemBackend.csproj"

COPY . .

RUN --mount=type=cache,id=nuget-v2,target=/root/.nuget/packages,sharing=locked \
    dotnet publish "QuilvianSystemBackend.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false \
    /p:DebugSymbols=false \
    /p:DebugType=None \
    /p:RunAnalyzers=false \
    /p:ContinuousIntegrationBuild=true \
    /clp:ErrorsOnly


FROM base AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "QuilvianSystemBackend.dll"]
