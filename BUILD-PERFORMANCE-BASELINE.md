# Build Performance Baseline

## Overview

Dokumen ini mencatat baseline performa build Backend Quilvian
dan perubahan optimasi yang sudah diterapkan.

Dokumen ini menjadi referensi sebelum melakukan perubahan terhadap:

- QuilvianSystemBackend.csproj
- Struktur project
- Entity Framework Migration
- Test project structure
- CI/CD build process


---

# Environment

## Application

Project:

Quilvian System Backend


Framework:

.NET 9


Target:

net9.0


Build Configuration:

Release


## Test Machine

Hardware:

- CPU : Intel Core i5
- RAM : 20 GB


SDK:

.NET SDK 9.0.316


---

# Before Optimization


## Condition

Sebelum optimasi, proses build mengalami perlambatan signifikan.


Command:

```powershell
dotnet clean

Remove-Item -Recurse -Force bin

Remove-Item -Recurse -Force obj

dotnet build QuilvianSystemBackend.sln -c Release