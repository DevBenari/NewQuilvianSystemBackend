# QuilvianBackend — Instruksi Agent

API ASP.NET Core (`QuilvianSystemBackend.csproj`), branch kerja **`MHamzah`**.

## ⛔ Aturan keras — baca dulu

> **Claude mengerjakan sampai `git commit`, lalu berhenti. `git push` dijalankan user.**

Berlaku untuk semua bentuk push, termasuk yang tujuannya benar. Ditegakkan oleh
`.claude/hooks/guard-no-auto-push.mjs`, jadi pelanggaran gagal sebelum dieksekusi.
Detail: `.claude/rules/no-auto-push.md`.

Selain push, backend **boleh diubah bebas** — file, endpoint, DTO, model, migration,
`dotnet ef`, `dotnet build`. Yang perlu izin user dulu:

- `appsettings*.json` berisi connection string / kredensial
- `dotnet ef database update` ke database selain lokal
- Menulis ulang migration yang sudah ada di `origin/MHamzah`

## Setiap perubahan wajib punya halaman laporan

`docs/hamzah/report/<nama-topik>.md`, ditulis **sebelum** commit dan ikut di-stage.
Folder `docs/` tidak di-gitignore, jadi laporan ini ikut ter-push — inilah jejak yang
dibaca tim backend.

Kalau pekerjaannya berasal dari register di `docs/hamzah/task/`, dokumen register itu
**wajib ikut diperbarui pada commit yang sama**.

## Alur Master Data

Master data punya kontrak baku sendiri:

```
/master-data-audit   Cari kekurangan backend terhadap kebutuhan frontend
                     → register di docs/hamzah/task/<topik>.md
                     Tidak mengubah kode.

/master-data-set     Kerjakan tugas dari register itu
                     → kode + laporan di docs/hamzah/report/<topik>.md
                     Build, commit, berhenti sebelum push.
```

Kontraknya di `.claude/rules/master-data-contract.md` — wajib dibaca sebelum menyentuh
kode master data.

## Build

```bash
dotnet build QuilvianSystemBackend.csproj --no-incremental
```

`--no-incremental` penting: build polos melewati kompilasi kalau tidak ada file yang
berubah, lalu melaporkan `0 Warning` yang menyesatkan. Baseline rebuild penuh repo ini:
**125 warning bawaan, 0 error** (per 2026-08-11).

SDK yang diminta `global.json`: **9.0.316** (feature band `9.0.3xx`, `rollForward:
latestPatch` — SDK band lain tidak dipakai).

## Struktur

```
Areas/          Administrator/  Corporate/  HealthServices/  SelfServices/
Controllers/    AuthController.cs, VersionController.cs
DTOs/  Models/  Enums/  Responses/  Services/  Repositories/  Hubs/  Migrations/
docs/hamzah/    task/    ← register kebutuhan (hasil audit)
                report/  ← laporan perubahan (wajib per pekerjaan)
```

> ⚠️ Empat controller EmployeeRelation ada di `Repositories/Configurations/`, bukan
> `Areas/`. Rutenya tetap jalan karena ASP.NET memindai atribut `[Route]`. Jangan terlewat
> saat menyisir controller master data.

@.claude/rules/no-auto-push.md

@.claude/rules/master-data-contract.md
