# syntax=docker/dockerfile:1.7

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80


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