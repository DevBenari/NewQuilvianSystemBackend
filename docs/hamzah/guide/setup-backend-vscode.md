# Menjalankan Backend Quilvian di Visual Studio Code

Panduan setup dan referensi perintah untuk `QuilvianSystemBackend` (ASP.NET Core 9) di VS Code.
Ditujukan untuk developer yang **tidak memakai Visual Studio full** — semua langkah di sini
memakai `dotnet` CLI + ekstensi VS Code.

| | |
|---|---|
| Tanggal | 2026-08-11 |
| Target framework | `net9.0` |
| SDK yang dipatok `global.json` | **9.0.316** (`rollForward: latestPatch`) |
| Database | PostgreSQL (Npgsql 9.0.4) |
| Cache | Redis (StackExchange) |
| ORM | Entity Framework Core 9.0.18 |
| Solution | `QuilvianSystemBackend.sln` |

---

# BAGIAN 1 — INSTALASI

## 1.1 .NET SDK — baca ini dulu, ini jebakan paling sering

`global.json` di root repo mematok SDK **9.0.316**:

```json
{
  "sdk": {
    "version": "9.0.316",
    "rollForward": "latestPatch",
    "allowPrerelease": false
  }
}
```

`rollForward: latestPatch` **tidak melompati feature band**. Artinya SDK 9.0.1xx atau 9.0.2xx
**tidak diterima** walaupun sama-sama .NET 9. Hanya 9.0.3xx dengan patch ≥ 316 yang dipakai.

### Cara mengecek

```bash
cd QuilvianBackend
dotnet --version
```

Kalau muncul error seperti ini, SDK Anda salah band:

```
A compatible .NET SDK was not found.

Requested SDK version: 9.0.316
global.json file: ...\QuilvianBackend\global.json

Installed SDKs:
9.0.101 [C:\Program Files\dotnet\sdk]
```

> Ini kondisi nyata di mesin developer saat dokumen ini ditulis: terpasang **9.0.101**,
> sementara yang diminta **9.0.316**. Akibatnya `dotnet build` dan seluruh perintah
> `dotnet ef` **tidak bisa dijalankan sama sekali** dari folder backend.

### Solusinya

**Pilihan A — pasang SDK yang benar (disarankan).**
Unduh **.NET SDK 9.0.3xx** dari <https://dotnet.microsoft.com/download/dotnet/9.0>
(pilih installer x64 untuk Windows). Beberapa SDK boleh terpasang berdampingan — yang lama
tidak perlu dihapus, `global.json` yang menentukan mana yang dipakai.

Verifikasi setelah install:

```bash
dotnet --list-sdks          # harus memuat 9.0.3xx
cd QuilvianBackend
dotnet --version            # harus mencetak 9.0.3xx, bukan error
```

**Pilihan B — turunkan patokan di `global.json`.**
Hanya kalau tim sepakat. Mengubah `global.json` berarti mengubah versi build untuk semua
orang dan untuk CI, jadi **jangan** dilakukan sendirian demi mesin sendiri.

**Pilihan C — jangan pakai `--global-json`-bypass.**
Menghapus `global.json` sementara lalu lupa mengembalikannya adalah penyebab commit
"kenapa CI-nya beda" yang paling sering. Kalau terpaksa, jangan sampai ikut ter-commit.

## 1.2 Ekstensi VS Code

| Ekstensi | ID | Wajib | Kegunaan |
|---|---|---|---|
| C# Dev Kit | `ms-dotnettools.csdevkit` | ✅ | Solution Explorer, debug, test runner |
| C# | `ms-dotnettools.csharp` | ✅ | Bahasa server (ikut terpasang bersama Dev Kit) |
| .NET Install Tool | `ms-dotnettools.vscode-dotnet-runtime` | ✅ | Dependency Dev Kit |
| Docker | `ms-azuretools.vscode-docker` | ○ | Kalau menjalankan lewat `Dockerfile` |
| REST Client | `humao.rest-client` | ○ | Uji endpoint tanpa Postman |
| PostgreSQL | `ms-ossdata.vscode-pgsql` | ○ | Lihat isi tabel dari dalam editor |

Pasang lewat Command Palette → *Extensions: Install Extensions*, atau:

```bash
code --install-extension ms-dotnettools.csdevkit
code --install-extension ms-azuretools.vscode-docker
```

## 1.3 Buka folder yang benar

C# Dev Kit mencari file `.sln`/`.csproj` di folder yang dibuka. Kalau Anda membuka folder
root workspace (`QuilvianFinal/`), IntelliSense C# **tidak akan menyala** karena `.sln`
ada satu tingkat di dalam.

Dua cara yang benar:

- **Buka langsung folder backend:** `code "QuilvianFinal/QuilvianBackend"`
- **Atau pakai multi-root workspace** kalau ingin frontend + backend sekaligus:
  *File → Add Folder to Workspace…* lalu tambahkan `QuilvianBackend`.

Setelah terbuka, cek status bar bawah — harus muncul nama project
`QuilvianSystemBackend`. Kalau tidak muncul, jalankan Command Palette →
*.NET: Restart Language Server*.

## 1.4 Tooling EF Core (`dotnet-ef`)

Paket `Microsoft.EntityFrameworkCore.Tools` di `.csproj` hanya melayani Package Manager
Console di Visual Studio. Untuk CLI, **tool global-nya harus dipasang terpisah**:

```bash
dotnet tool install --global dotnet-ef --version 9.*
```

Sudah pernah pasang tapi versinya lama:

```bash
dotnet tool update --global dotnet-ef --version 9.*
```

Verifikasi:

```bash
dotnet ef --version        # harus 9.x
dotnet tool list -g        # dotnet-ef harus terdaftar
```

> `dotnet ef` ikut tunduk pada `global.json`. Kalau langkah 1.1 belum beres, perintah ini
> gagal juga — bukan karena tool-nya salah pasang.

## 1.5 `appsettings.Development.json` — tidak ada di repo

File ini **di-gitignore** (`.gitignore:366`). Fresh clone **tidak** akan memilikinya, dan
aplikasi tidak bisa start tanpa itu karena connection string tinggal di sana.

**Minta file ini ke rekan tim atau ke lead backend. Jangan mengarang isinya, dan jangan
pernah meng-commit-nya.**

Struktur key yang harus ada (nilai sengaja tidak ditulis di sini):

```jsonc
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=…;Port=…;Username=…;Password=…;Database=…",
    "Redis": "localhost:6379"
  },
  "Jwt":            { "Issuer": "…", "Audience": "…", "…": "…" },
  "AuthCookie":     { "Secure": false, "SameSite": "…", "Domain": "…" },
  "Cors":           { "AllowedOrigins": [ "http://localhost:3000", "…" ] },
  "SeedSuperAdmin": { "Enabled": false, "…": "…" },
  "SeedDefaultData":{ "Enabled": false },
  "AiSettings":     { "…": "…" },
  "LoginGeofence":  { "Enabled": false, "…": "…" },
  "Logging":        { "…": "…" },
  "AllowedHosts":   "*"
}
```

> ⚠️ **`DefaultConnection` di file dev tim menunjuk ke server PostgreSQL remote, bukan
> `localhost`.** Artinya begitu aplikasi start, Anda menulis ke database yang dipakai
> bersama. Baca §2.3 sebelum menjalankan perintah migration apa pun.

`appsettings.json` (yang ter-commit) **tidak memuat** `ConnectionStrings` sama sekali — jadi
kalau file Development hilang, gejalanya adalah error koneksi saat start, bukan error
konfigurasi.

Alternatif yang lebih aman untuk kredensial, karena project sudah punya `UserSecretsId`:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Username=postgres;Password=…;Database=quilvian"
dotnet user-secrets list
```

User secrets disimpan di luar folder repo, jadi mustahil ikut ter-commit.

## 1.6 PostgreSQL & Redis

| Layanan | Dipakai untuk | Kalau tidak ada |
|---|---|---|
| PostgreSQL | Seluruh data (`AddDbContext` + `UseNpgsql`, `Program.cs:137-140`) | Aplikasi gagal saat request pertama yang menyentuh DB |
| Redis | Distributed cache (`AddStackExchangeRedisCache`, `Program.cs:167-170`) | Koneksi bersifat lazy — error baru muncul saat cache dipakai |

Cara tercepat menyiapkan keduanya secara lokal:

```bash
docker run -d --name quilvian-postgres -e POSTGRES_PASSWORD=postgres -p 5432:5432 postgres:16
docker run -d --name quilvian-redis -p 6379:6379 redis:7
```

Kalau memakai Postgres lokal, arahkan `DefaultConnection` ke `Host=localhost;Port=5432`
lalu jalankan `dotnet ef database update` untuk membentuk skemanya (§2.3).

## 1.7 File `.vscode/` — belum ada di repo, buat sendiri

Repo ini **tidak** menyertakan folder `.vscode/`. `Properties/launchSettings.json` sudah ada
dan sudah cukup untuk `dotnet run`, tapi untuk tombol **F5** (debug) VS Code butuh
`launch.json` sendiri.

`.vscode/` **tidak** masuk `.gitignore`, jadi kalau dibuat lalu di-commit, isinya berlaku
untuk semua orang. Sepakati dulu dengan tim sebelum meng-commit.

`.vscode/launch.json`:

```jsonc
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET Core Launch (web)",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/bin/Debug/net9.0/QuilvianSystemBackend.dll",
      "args": [],
      "cwd": "${workspaceFolder}",
      "stopAtEntry": false,
      "serverReadyAction": {
        "action": "openExternally",
        "pattern": "\\bNow listening on:\\s+(https?://\\S+)",
        "uriFormat": "%s/swagger"
      },
      "env": { "ASPNETCORE_ENVIRONMENT": "Development" },
      "sourceFileMap": { "/Views": "${workspaceFolder}/Views" }
    },
    {
      "name": ".NET Core Attach",
      "type": "coreclr",
      "request": "attach"
    }
  ]
}
```

`.vscode/tasks.json`:

```jsonc
{
  "version": "2.0.0",
  "tasks": [
    {
      "label": "build",
      "command": "dotnet",
      "type": "process",
      "args": ["build", "${workspaceFolder}/QuilvianSystemBackend.sln", "/property:GenerateFullPaths=true", "/consoleloggerparameters:NoSummary"],
      "problemMatcher": "$msCompile",
      "group": { "kind": "build", "isDefault": true }
    },
    {
      "label": "watch",
      "command": "dotnet",
      "type": "process",
      "args": ["watch", "run", "--project", "${workspaceFolder}/QuilvianSystemBackend.csproj"],
      "problemMatcher": "$msCompile"
    }
  ]
}
```

## 1.8 Verifikasi instalasi

Jalankan berurutan dari folder `QuilvianBackend`:

```bash
dotnet --version                 # 1. harus 9.0.3xx, tidak error
dotnet ef --version              # 2. harus 9.x
dotnet restore                   # 3. harus selesai tanpa error
dotnet build                     # 4. harus "Build succeeded"
dotnet run                       # 5. buka https://localhost:7184/swagger
```

Kalau langkah 5 berhasil, Swagger menampilkan **5 grup dokumen**: Authentication,
Administrator, Corporate, Health Services, Self Services.

> Swagger **hanya aktif di Development** (`Program.cs:814`). Kalau `ASPNETCORE_ENVIRONMENT`
> bukan `Development`, `/swagger` menghasilkan 404 — itu bukan bug.

---

# BAGIAN 2 — REFERENSI PERINTAH

Semua perintah dijalankan dari folder `QuilvianBackend/`.
Kalau terminal Anda ada di root workspace, tambahkan `--project QuilvianBackend` atau
`cd QuilvianBackend` dulu.

## 2.1 Build & jalankan

| Perintah | Kegunaan |
|---|---|
| `dotnet restore` | Unduh NuGet package. Otomatis dipanggil `build`/`run`, perlu manual hanya setelah ubah `.csproj` |
| `dotnet build` | Kompilasi. **Ini yang wajib sukses sebelum commit** |
| `dotnet build --configuration Release` | Sama seperti yang dijalankan CI |
| `dotnet build --no-restore` | Lebih cepat kalau restore sudah pernah jalan |
| `dotnet run` | Jalankan API. Baca profil dari `Properties/launchSettings.json` |
| `dotnet run --launch-profile QuilvianSystemBackend` | Pilih profil eksplisit |
| `dotnet watch run` | Hot reload — kode disimpan, aplikasi menyesuaikan tanpa restart manual |
| `dotnet clean` | Hapus `bin/` + `obj/`. Obat pertama untuk error build yang tidak masuk akal |
| `dotnet publish -c Release -o ./publish` | Hasil siap deploy |

URL default dari `launchSettings.json`:

| | |
|---|---|
| HTTPS | `https://localhost:7184` |
| HTTP | `http://localhost:5107` |
| Swagger | `https://localhost:7184/swagger` |
| SignalR hub antrean | `/hubs/queues` |

Mengganti port tanpa menyentuh `launchSettings.json`:

```bash
dotnet run --urls "http://localhost:5000"
```

## 2.2 Paket NuGet

```bash
dotnet add package <NamaPaket>                    # tambah paket versi terbaru
dotnet add package <NamaPaket> --version 9.0.18   # patok versi
dotnet remove package <NamaPaket>
dotnet list package                               # daftar paket terpasang
dotnet list package --outdated                    # cek yang tertinggal
dotnet list package --vulnerable                  # cek yang punya CVE
```

Aturan repo: paket runtime ASP.NET/EF Core di sini seragam di **9.0.18**. Kalau menambah
paket dari keluarga `Microsoft.*`, samakan versinya supaya tidak terjadi konflik binding.

## 2.3 Entity Framework Core — migration

> ⛔ **Baca dulu.** `appsettings.Development.json` milik tim menunjuk ke **PostgreSQL
> remote yang dipakai bersama**, bukan localhost. `dotnet ef database update` di kondisi itu
> akan **mengubah skema database tim**, bukan database Anda sendiri.
>
> Aturan workspace: `dotnet ef database update` ke database selain lokal **butuh izin
> eksplisit dari pemilik repo** — lihat `.claude/rules/backend-workflow.md` RULE 1.
>
> Cara aman: arahkan `DefaultConnection` ke Postgres lokal (§1.6), atau pakai
> `--connection` untuk menimpa sekali jalan:
>
> ```bash
> dotnet ef database update --connection "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=quilvian_local"
> ```

### Perintah harian

| Perintah | Kegunaan |
|---|---|
| `dotnet ef migrations list` | Daftar migration + mana yang sudah diterapkan |
| `dotnet ef migrations add <Nama>` | Buat migration baru dari perubahan model |
| `dotnet ef migrations remove` | Batalkan migration **terakhir** yang belum diterapkan |
| `dotnet ef database update` | Terapkan seluruh migration yang tertunda |
| `dotnet ef database update <NamaMigration>` | Maju/mundur ke migration tertentu |
| `dotnet ef migrations script` | Hasilkan SQL, tidak menyentuh database |
| `dotnet ef migrations script <Dari> <Ke> -o migrasi.sql` | SQL untuk rentang tertentu — ini yang dikirim ke DBA |
| `dotnet ef dbcontext info` | Cek provider + connection string mana yang sedang terbaca |

### Konvensi penamaan migration di repo ini

Dibaca dari `Migrations/`, polanya kata kerja + cakupan, camelCase atau PascalCase:

```
initializeTrxWorkflowApproverAssignments
addOvertimeRequestAndPlanningFoundation4A3
changeColumnWfpDisciplinaryAction
FinalApplicationVersioningV2
```

Contoh yang benar untuk fitur baru:

```bash
dotnet ef migrations add addEmployeeCategoryMasterData
```

### Alur aman menambah migration

```
1. Ubah model / EF configuration
2. dotnet build                                    ← harus sukses dulu
3. dotnet ef migrations add <Nama>
4. Baca file migration yang dihasilkan             ← WAJIB, jangan asal percaya
5. dotnet ef migrations script                     ← lihat SQL-nya
6. dotnet ef database update                       ← hanya ke DB lokal
```

⛔ **Jangan** menghapus atau menulis ulang migration yang sudah ada di `origin/MHamzah` —
migration yang sudah dipakai orang lain hanya boleh diperbaiki lewat migration baru.

## 2.4 Format & kualitas kode

```bash
dotnet format                          # rapikan whitespace + using + style
dotnet format --verify-no-changes      # cek saja, tidak mengubah — cocok untuk pre-commit
dotnet format --include Areas/Corporate/HumanResource/**   # batasi cakupan
```

Repo ini **belum punya `.editorconfig`**, jadi `dotnet format` memakai default .NET.
Untuk perubahan kecil, batasi cakupannya dengan `--include` supaya diff tidak melebar ke
file yang tidak Anda sentuh.

## 2.5 Docker

`Dockerfile` sudah ada di root backend, berbasis `mcr.microsoft.com/dotnet/aspnet:9.0`,
mengekspos port 80 dan memuat dependency tambahan (piper/ffmpeg untuk QueueVoice).

```bash
docker build -t quilvian-backend:dev .
docker run --rm -p 8080:80 --name quilvian-backend quilvian-backend:dev
```

Build butuh argumen versi kalau ingin metadata versinya benar:

```bash
docker build \
  --build-arg APP_BUILD_VERSION=1.0.0-dev.1 \
  --build-arg APP_COMMIT_SHA=$(git rev-parse HEAD) \
  --build-arg APP_BRANCH=MHamzah \
  -t quilvian-backend:dev .
```

## 2.6 Yang dijalankan CI

Dari `.github/workflows/validate-agent-codex-backend.yml` — kalau dua perintah ini lolos
di mesin Anda, CI juga akan lolos:

```bash
dotnet restore ./QuilvianSystemBackend.sln
dotnet build ./QuilvianSystemBackend.sln --configuration Release --no-restore
```

CI memakai `dotnet-version: 9.0.x` lewat `actions/setup-dotnet`, dan tetap tunduk pada
`global.json`.

## 2.7 Menghubungkan dengan frontend

Backend sudah mengizinkan origin `http://localhost:3000` di `Cors:AllowedOrigins`, jadi
Next.js dev server bisa langsung memanggilnya.

Di sisi frontend, base URL dibaca dari environment variable
**`NEXT_PUBLIC_API_QUILVIAN`** (`src/lib/axiosInstance/InstanceAxios.jsx:222`). Isi `.env`
frontend dengan URL backend lokal Anda:

```
NEXT_PUBLIC_API_QUILVIAN=https://localhost:7184
```

Kalau browser menolak sertifikat dev HTTPS:

```bash
dotnet dev-certs https --trust
```

atau pakai profil HTTP saja (`http://localhost:5107`).

---

# BAGIAN 3 — TROUBLESHOOTING

| Gejala | Penyebab | Solusi |
|---|---|---|
| `A compatible .NET SDK was not found. Requested SDK version: 9.0.316` | SDK beda feature band | §1.1 — pasang SDK 9.0.3xx |
| `dotnet ef` → `command not found` / `is not a dotnet command` | Tool global belum dipasang | `dotnet tool install --global dotnet-ef --version 9.*` |
| IntelliSense C# mati, tidak ada Solution Explorer | Folder yang dibuka salah | §1.3 — buka folder `QuilvianBackend`, lalu *.NET: Restart Language Server* |
| Start gagal dengan error koneksi Npgsql | `appsettings.Development.json` tidak ada / connection string salah | §1.5 |
| `/swagger` menghasilkan 404 | `ASPNETCORE_ENVIRONMENT` bukan `Development` | Set env var, atau jalankan lewat profil `QuilvianSystemBackend` |
| Error Redis saat memakai fitur tertentu | Redis lokal tidak jalan | `docker run -d -p 6379:6379 redis:7` |
| Build error aneh setelah pindah branch | Artefak `bin/`/`obj/` basi | `dotnet clean` lalu `dotnet build` |
| `dotnet ef migrations add` gagal padahal kode benar | `dotnet build` belum sukses | EF butuh assembly hasil build — perbaiki build dulu |
| Browser menolak sertifikat HTTPS localhost | Dev cert belum dipercaya | `dotnet dev-certs https --trust` |
| Perubahan tidak terlihat walau sudah save | Aplikasi tidak restart | Pakai `dotnet watch run` |

---

# Lampiran — Ringkasan perintah

```bash
# Setup sekali di awal
dotnet tool install --global dotnet-ef --version 9.*
dotnet dev-certs https --trust
dotnet restore

# Harian
dotnet build                     # sebelum commit — wajib
dotnet watch run                 # ngoding dengan hot reload
dotnet run                       # jalankan biasa
dotnet format --verify-no-changes

# Migration (hati-hati: cek dulu DB tujuan)
dotnet ef migrations list
dotnet ef migrations add <Nama>
dotnet ef migrations script
dotnet ef database update

# Sama seperti CI
dotnet restore ./QuilvianSystemBackend.sln
dotnet build ./QuilvianSystemBackend.sln --configuration Release --no-restore
```

## Catatan aturan repo

- Commit backend hanya boleh di-push ke **`origin MHamzah`**, ditulis eksplisit.
- Setiap perubahan kode backend wajib punya halaman laporan di `docs/report/`.
- `appsettings*` berisi kredensial dan `dotnet ef database update` ke DB non-lokal
  **butuh izin pemilik repo** lebih dulu.

Selengkapnya di `.claude/rules/backend-workflow.md` (repo frontend).
