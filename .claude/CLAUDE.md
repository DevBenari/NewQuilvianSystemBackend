# QuilvianBackend — Instruksi Agent

API ASP.NET Core (`QuilvianSystemBackend.csproj`), branch kerja **`MHamzah`**.

## ⛔ Aturan keras — baca dulu

> **Git di backend hanya boleh DIBACA.** Claude menulis file dan menjalankan build.
> Seluruh perintah git yang mengubah sesuatu — `add`, `commit`, `push`, `pull`, `fetch`,
> `checkout`, `branch`, `reset`, `restore`, `stash` — dijalankan user sendiri.

Yang boleh dijalankan Claude hanya perintah yang menampilkan keadaan: `status`, `log`,
`diff`, `show`, `blame`, `rev-parse`. Ditegakkan oleh
`.claude/hooks/guard-git-read-only.mjs` memakai allowlist, jadi verb yang tidak terdaftar
ikut tertahan. Detail: `.claude/rules/git-read-only.md`.

Selain git, backend **boleh diubah bebas** — file, endpoint, DTO, model, migration,
`dotnet ef`, `dotnet build`. Yang perlu izin user dulu:

- `appsettings*.json` berisi connection string / kredensial
- `dotnet ef database update` ke database selain lokal
- Menulis ulang migration yang sudah ada di `origin/MHamzah`

## Setiap perubahan wajib punya halaman laporan

`docs/hamzah/report/<nama-topik>.md`, ditulis **sebelum** pekerjaan diserahkan ke user.
Folder `docs/` tidak di-gitignore, jadi laporan ini ikut ter-push — inilah jejak yang
dibaca tim backend.

Kalau pekerjaannya berasal dari register di `docs/hamzah/task/`, dokumen register itu
**wajib ikut diperbarui**, dan disebutkan agar ikut di-stage user pada commit yang sama.

## Cara mengakhiri pekerjaan

Setelah file ditulis dan build lolos, sajikan serah terima: daftar file yang berubah,
hasil verifikasi, lalu blok perintah `git add` + `commit` + `push` yang tinggal disalin
user. Jangan menawarkan untuk menjalankannya.

## Alur Master Data

Master data punya kontrak baku sendiri:

```
/master-data-audit   Cari kekurangan backend terhadap kebutuhan frontend
                     → register di docs/hamzah/task/<topik>.md
                     Tidak mengubah kode.

/master-data-set     Kerjakan tugas dari register itu
                     → kode + laporan di docs/hamzah/report/<topik>.md
                     Build, lalu serahkan perintah git ke user.
```

Keduanya punya **dua pintu masuk** ke prosedur yang sama:

| Pintu masuk | File | Kapan dipakai |
|---|---|---|
| Slash command | `.claude/commands/<nama>.md` | User mengetik `/master-data-audit <cakupan>` |
| Skill | `.claude/skills/<nama>/SKILL.md` | Claude memilih sendiri saat permintaannya cocok |

File `commands/` sengaja dibuat **tipis** — isinya hanya mengarahkan ke `SKILL.md` yang
bersangkutan. Prosedur lengkapnya hidup di satu tempat saja, di `SKILL.md`. Kalau ada
langkah yang perlu diubah, ubah di sana; jangan menyalin ulang ke file command.

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

@.claude/rules/git-read-only.md

@.claude/rules/master-data-contract.md
