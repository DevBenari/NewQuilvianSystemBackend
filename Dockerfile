# Runtime image: hanya untuk menjalankan aplikasi
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

# Untuk ASP.NET Core 6, port default umum adalah 80
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

# Build image: hanya dipakai saat proses build di CI/CD
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copy file project terlebih dahulu agar layer restore bisa tercache
COPY ["QuilvianSystemBackend.csproj", "./"]

# Restore dependency
RUN dotnet restore "QuilvianSystemBackend.csproj"

# Copy semua source code setelah restore
COPY . .

# Publish langsung ke folder output final
# Tidak perlu dotnet build terpisah karena dotnet publish sudah melakukan build
RUN dotnet publish "QuilvianSystemBackend.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# Final image: hanya berisi hasil publish, bukan SDK
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "QuilvianSystemBackend.dll"]