# Laporan Perubahan Backend — `BE-HRD-404`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-404` |
| Judul | Persistence Uraian Tugas berversi per jabatan |
| Slice | `S-FIN-M05` / Uraian Tugas (alias `FIN-HRD-05-BE-002`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.3, kartu `BE-HRD-404` |
| Trace | Gerbang `{MIG-M05-JD}`; instruksi Menu 05 `FIN-HRD-05-BE-002` |
| Contract version | Kontrak existing Menu 05/07; tidak ada endpoint baru |
| Dependency | `{MIG-M05-JD}` — izin menulis migration diberikan 20 November 2026; migration diterapkan pemilik ke DB dev, terkonfirmasi 21 November 2026 |
| Klasifikasi | `MEDIUM` — migration manual |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | `Migrations/20261120091000_AddMstJobDescription.cs`; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d17aa187` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026; status dinaikkan 21 November 2026 |
| Status | ✅ Selesai (21 November 2026; sebelumnya 🟡 sebagian 20 November 2026) — migration ditulis, build/QBE lulus, dan migration sudah diterapkan di DB dev `QuilvianNewDevHamzah`; uji API di DB dev `NOT RUN` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `MasterData/Organization` (prefix `Mst` terdaftar) |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | QBE-TXN-001, QBE-VAL-001, QBE-CODE-003/004/006 (bila menyangkut nomor) |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Model `MstJobDescription`/`MstJobDescriptionItem`, konfigurasi EF, dan `DbSet` sudah ada, tetapi tidak ada migration maupun skrip SQL yang membuat tabelnya. Akibatnya endpoint Uraian Tugas Menu 05 dan Uraian Tugas Pegawai Menu 07 gagal `42P01` di database mana pun.

## 2. Proses bisnis

1. Migration membuat `MstJobDescription` (jabatan, kode, nomor versi, periode efektif, status `Draft`/`Active`/`Archived`, tujuan jabatan) dan `MstJobDescriptionItem` (seksi, urutan, isi), sesuai konfigurasi EF.
2. Index dari konfigurasi: unik `(PositionId, VersionNumber)`, unik `JobDescriptionCode`, `(PositionId, Status)`, `(JobDescriptionId, SectionType, Sequence)`.
3. Tambahan penjaga di database: satu versi `Active` per jabatan dan satu `Draft` per jabatan (index unik parsial), `CHECK` status dan rentang tanggal.
4. Histori tidak ditimpa: aktivasi versi baru mengarsipkan versi lama dan menutup tanggal berakhirnya ke H-1 (`JobDescriptionService`).

**Contoh.** Jabatan Perawat Pelaksana punya V1 aktif sejak 1 Januari 2026. V2 diaktifkan 1 Desember 2026 → V1 `Archived` berakhir 30 November 2026, V2 `Active`; dua aktivasi bersamaan tidak dapat menghasilkan dua versi aktif karena index unik parsial.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Model dan konfigurasi `MstJobDescription*`, `JobDescriptionService` (draft, aktivasi), pola migration `20261118100000_AddHrdOvertimeSetting`, `IdentityModel`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Migrations/20261120091000_AddMstJobDescription.cs` | Migration manual idempoten pembuat dua tabel, FK, index, dan penjaga; sudah diterapkan di DB dev (21 November 2026) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` |
| Database | Migration baru, **sudah diterapkan** di DB dev `QuilvianNewDevHamzah` (terkonfirmasi 21 November 2026). Database lain (staging/production) tetap wajib menjalankannya sebelum endpoint Uraian Tugas dipakai |
| Keamanan/Auth | `NOT APPLICABLE` |

## 4. Dokumentasi endpoint

Tidak ada endpoint — skema basis data.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratchpad>` | Berhasil, 0 error, 608 warning (himpunan sama dengan sebelum perubahan) | `PASS` | Log build 20 November 2026 |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` (SK service, batch service, controller atasan) | berkas yang disentuh: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran: kolom migration vs model/konfigurasi EF | Sesuai (nama, tipe, panjang, default, FK, index) | `PASS` | perbandingan manual |
| `dotnet ef database update --no-build` (dijalankan pemilik, 21 November 2026) | "No migrations were applied. The database is already up to date." | `PASS` | Keluaran terminal pemilik |
| `dotnet ef migrations list --no-build` terhadap `QuilvianNewDevHamzah` (21 November 2026) | 431 migration, tidak ada yang `Pending`; `20261120091000_AddMstJobDescription` tercatat sudah diterapkan. Assembly dibangun 21 November 2026 17:02, sesudah berkas migration ditulis | `PASS` | Keluaran perintah |
| Cek `information_schema` langsung | Tidak dijalankan (kueri database langsung tidak diminta) | `NOT RUN` | Bukti penerapan dari riwayat migration EF |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Satu jabatan maksimal satu versi aktif pada satu tanggal | Terpenuhi (source + migration) | aktivasi transaksional + index unik parsial `UX_MstJobDescription_OneActivePerPosition` |
| (2) Histori tidak ditimpa | Terpenuhi | versi lama diarsipkan, bukan diubah isinya |
| (3) Tabel tersedia di DB dev setelah migration disetujui | Terpenuhi (21 November 2026) | migration tercatat sudah diterapkan di `QuilvianNewDevHamzah` (`dotnet ef migrations list`) |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Langkah berikutnya | Migration sudah diterapkan dan tanda dinaikkan ke ✅ (21 November 2026). Sisa: uji API Uraian Tugas di DB dev (`BE-HRD-405`, `413`) bila wewenang uji diberikan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit; tidak ada stage/commit/push |
