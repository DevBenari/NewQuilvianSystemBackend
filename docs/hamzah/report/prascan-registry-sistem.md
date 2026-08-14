# Laporan Perubahan — Scan Sistem Wajib Sebelum Wawancara Modul

| Field | Nilai |
| --- | --- |
| Tanggal | 2026-08-14 |
| Jenis perubahan | Konfigurasi AI agent dan dokumentasi |
| Source aplikasi | Tidak diubah |
| Migration / database | Tidak ada |
| Dokumen canonical | [docs/agency/update-skills/04-prascan-registry-sistem.md](../../agency/update-skills/04-prascan-registry-sistem.md) |
| Commit yang diaudit | backend `dd09806` cabang `MHamzah`, frontend `08c84d371` cabang `HamzahV2` |

## Ringkasan

Menambahkan satu tahap wajib di paling depan workflow pengembangan modul: pemindaian seluruh
sistem menjadi registry keadaan nyata, dijalankan sebelum wawancara bisnis dimulai.

Sebelum perubahan ini, `/grill-me` memulai wawancara tanpa membaca kode sama sekali, sedangkan
audit kode baru berjalan setelahnya dan hanya mencakup satu modul. Keputusan bisnis diambil
tanpa peta sistem, sehingga modul baru berisiko membangun ulang kemampuan yang sudah ada,
memakai nama yang sudah dipakai modul lain, atau mengambil alih data milik modul lain.

Angka yang mendasari: backend memuat 445 `DbSet`, 452 file EF configuration, 246 controller,
dan 81 migration.

## Berkas baru

| Berkas | Isi |
| --- | --- |
| `.claude/skills/scan-system-registry/SKILL.md` | Skill pemindaian dengan tiga mode `full`, `refresh`, `focus`; batas kewenangan read-only |
| `.claude/rules/rule-prascan/README.md` | Ringkasan aturan dan alur setelah perubahan |
| `.claude/rules/rule-prascan/aturan-prascan-modul.md` | Gerbang wajib, Kartu Konteks Pra-Wawancara, status kesegaran, kewajiban skill lain |
| `.claude/rules/rule-prascan/format-registry-sistem.md` | Format baku tujuh berkas registry, legenda tingkat kesiapan `L0`–`L4`, larangan isi |
| `.claude/commands/qv-scan.md` | Slash command backend beserta pengamannya |
| `docs/agency/update-skills/04-prascan-registry-sistem.md` | Dokumen rekomendasi lengkap beserta tiga keputusan yang diminta |
| `QuilvianSystemFrontendDev/.claude/commands/qv-scan.md` | Penunjuk dari sesi frontend ke sesi backend |

## Berkas yang disunting

| Berkas | Perubahan |
| --- | --- |
| `.claude/skills/grill-me/SKILL.md` | Bagian baru "Gerbang wajib: registry sistem harus segar"; penguncian scope kini diperiksa terhadap Kartu Konteks |
| `.claude/skills/trace-existing-capabilities/SKILL.md` | Bagian baru "Batas dengan `/scan-system-registry`"; wajib memeriksa kesegaran registry dan memulai dari registry |
| `.claude/PANDUAN-PENGGUNAAN-SKILLS.md` | Skill backend 6 menjadi 7; tahap 0 ditambahkan; tabel command, tabel effort, artefak canonical, dan tabel loop perbaikan diperbarui |
| `docs/agency/update-skills/README.md` | Indeks dokumen 04 |

## Verifikasi

| Yang diperiksa | Hasil |
| --- | --- |
| Seluruh berkas baru tercipta di lokasi yang benar | Lulus — 6 berkas backend, 1 berkas frontend |
| Skill terbaca Claude Code | Lulus — `scan-system-registry` muncul pada daftar skill backend |
| Tautan relatif antar berkas aturan, skill, command, dan dokumen | Lulus — seluruh tautan menunjuk berkas yang ada |
| Konsistensi jumlah skill pada panduan | Lulus — 7 backend + 1 frontend, konsisten di seluruh bagian |
| Angka yang dipakai sebagai dasar rekomendasi | Lulus — dihitung langsung dari source pada commit `dd09806` |

**Tidak dijalankan:** `dotnet build` tidak dijalankan karena tidak ada berkas `.cs`, `.csproj`,
atau migration yang berubah. Seluruh perubahan berupa berkas Markdown pada `.claude/` dan
`docs/`. Bila tetap ingin diverifikasi, jalankan `dotnet build QuilvianSystemBackend.sln`.

**Belum dikerjakan:** registry itu sendiri belum dibuat. Berkas di `docs/system-registry/` baru
akan ada setelah `/qv-scan full` dijalankan pada sesi tersendiri.

## Keputusan yang masih menunggu owner

| No | Keputusan | Owner |
| ---: | --- | --- |
| 1 | Menyetujui `/qv-scan` sebagai gerbang wajib sebelum `/grill-me` | Pemilik suite skill |
| 2 | Menetapkan siapa yang berwenang mengisi kolom pemilik data | Pemilik arsitektur backend |
| 3 | Menetapkan masa berlaku pemindaian penuh, usulan 30 hari | Pemilik suite skill |

Rinciannya ada pada bagian 9 dokumen canonical.

## Risiko tersisa

Registry mencakup seluruh sistem sehingga berpotensi cepat basi. Penanganannya sudah terpasang
berupa mode `refresh` yang hanya menyisir berkas pada `git diff --name-only <sha>..HEAD`, dan
pemeriksaan kesegaran berbasis commit SHA kedua repository sebelum skill lain boleh berjalan.
