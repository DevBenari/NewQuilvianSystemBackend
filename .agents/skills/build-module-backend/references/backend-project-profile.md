# Profil Implementasi Backend Quilvian

## Project

- Root: `NewQuilvianSystemBackend/`
- Stack utama: ASP.NET Core, Entity Framework Core, controller/service/DTO, SQL Server.
- Project file: `QuilvianSystemBackend.csproj`.
- Blueprint canonical: `docs/module-blueprints/<module>/`.
- Laporan perubahan: `docs/hamzah/report/<topic>.md`.

Selalu verifikasi struktur aktual sebelum memilih path implementasi. Folder area/modul adalah sumber bukti; template ini tidak mengizinkan pembuatan struktur paralel yang tidak perlu.

## Perintah verifikasi umum

Pilih yang relevan dan sesuai instruksi repository:

```powershell
dotnet build QuilvianSystemBackend.csproj --no-incremental
dotnet test
```

Jangan jalankan build atau seluruh test suite bila pengguna melarangnya atau task hanya mengubah konfigurasi agent/dokumentasi.

## Isi laporan minimum

- tujuan dan task ID;
- branch dan scope;
- daftar file/material change;
- perubahan contract/database/runtime;
- test/build yang dijalankan beserta hasil;
- hal yang tidak diverifikasi;
- risiko/blocker dan rekomendasi;
- status Git tanpa melakukan add/commit/push.

## Guardrail repository

- Git backend bersifat read-only bagi agent: jangan `git add`, `commit`, `push`, `pull`, `merge`, atau rewrite history.
- Gunakan migration baru untuk schema baru; jangan rewrite migration remote tanpa instruksi eksplisit.
- Jangan simpan secret pada source atau laporan.
- Jangan mengubah database bersama/non-lokal tanpa otorisasi.

