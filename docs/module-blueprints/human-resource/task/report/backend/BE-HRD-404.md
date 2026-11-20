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
| Dependency | `{MIG-M05-JD}` — izin menulis migration diberikan 20 November 2026; eksekusi belum |
| Klasifikasi | `MEDIUM` — migration manual |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | `Migrations/20261120091000_AddMstJobDescription.cs`; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d17aa187` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | 🟡 Sebagian — migration ditulis dan build/QBE lulus; kriteria (3) "tabel tersedia di DB dev" menunggu eksekusi migration oleh pemilik |

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
| `Migrations/20261120091000_AddMstJobDescription.cs` | Migration manual idempoten pembuat dua tabel, FK, index, dan penjaga; belum dijalankan |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` |
| Database | **Migration baru, belum dijalankan.** Wajib dijalankan sebelum endpoint Uraian Tugas dipakai |
| Keamanan/Auth | `NOT APPLICABLE` |

## 4. Dokumentasi endpoint

Tidak ada endpoint — skema basis data.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratchpad>` | Berhasil, 0 error, 608 warning (himpunan sama dengan sebelum perubahan) | `PASS` | Log build 20 November 2026 |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` (SK service, batch service, controller atasan) | berkas yang disentuh: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran: kolom migration vs model/konfigurasi EF | Sesuai (nama, tipe, panjang, default, FK, index) | `PASS` | perbandingan manual |
| `dotnet ef database update` / cek `information_schema` | Belum dijalankan | `NOT RUN` | Eksekusi migration butuh wewenang terpisah |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Satu jabatan maksimal satu versi aktif pada satu tanggal | Terpenuhi (source + migration) | aktivasi transaksional + index unik parsial `UX_MstJobDescription_OneActivePerPosition` |
| (2) Histori tidak ditimpa | Terpenuhi | versi lama diarsipkan, bukan diubah isinya |
| (3) Tabel tersedia di DB dev setelah migration disetujui | Belum terpenuhi | migration ditulis, belum dijalankan |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Langkah berikutnya | Pemilik menjalankan `dotnet ef database update` (migration `20261120091000_AddMstJobDescription`), lalu tanda ini dapat dinaikkan ke ✅ |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit; tidak ada stage/commit/push |
