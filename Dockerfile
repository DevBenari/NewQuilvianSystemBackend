# syntax=docker/dockerfile:1.7

# Runtime image: hanya untuk menjalankan aplikasi
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80


# Build image: hanya dipakai saat proses build di CI/CD
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy file project terlebih dahulu agar restore bisa tercache
COPY ["QuilvianSystemBackend.csproj", "./"]

# Restore dependency dengan cache NuGet BuildKit
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore "QuilvianSystemBackend.csproj" \
    --disable-parallel false

# Copy semua source code setelah restore
COPY . .

# Publish aplikasi
# Optimasi:
# --no-restore             : tidak restore ulang
# UseAppHost=false         : output lebih kecil
# DebugSymbols=false       : tidak generate symbol debug
# DebugType=None           : tidak generate debug info
# RunAnalyzers=false       : mempercepat build kalau analyzer aktif
# clp:ErrorsOnly           : log lebih bersih
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet publish "QuilvianSystemBackend.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false \
    /p:DebugSymbols=false \
    /p:DebugType=None \
    /p:RunAnalyzers=false \
    /p:ContinuousIntegrationBuild=true \
    /clp:ErrorsOnly


# Final image: hanya berisi hasil publish, bukan SDK
FROM base AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "QuilvianSystemBackend.dll"]
