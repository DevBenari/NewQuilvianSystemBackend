# Laporan Perubahan Backend — `BE-HRD-385`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-385` |
| Judul | Registri dokumen administrasi lintas jenis di server |
| Slice | `S-FIN-ADM` / Dokumen Administrasi (alias `DA-IMP`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.1, kartu `BE-HRD-385` |
| Trace | Baseline `BE-HRD-025`, `BE-HRD-205`; `04_IMPLEMENTATION_TASKS.md` Task 5 |
| Contract version | Kontrak Administrasi Kepegawaian (`docs/update/administrasi-kepegawaian`); endpoint baru dicatat sebagai delta |
| Dependency | `BE-HRD-381` ✅, `BE-HRD-383` ✅ |
| Klasifikasi | `MEDIUM` |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | `AdministrativeDocumentRegistryService`, `AdministrativeDocumentRegistryController`, DTO; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `a7c37c56` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 3/3 acceptance criteria terpetakan ke source; build, QBE, registri lulus; bagian Surat Tugas bergantung migration `20261120093000` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `HrServiceManagement` |
| Keberlakuan | `NEW CODE` (Surat Tugas, registri) + `TOUCHED LEGACY` (Surat Keterangan) |
| QBE yang berlaku | QBE-TXN-001, QBE-VAL-001, QBE-CODE-003/004/006 (bila menyangkut nomor) |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Registri dirakit di klien dari tiga daftar terpisah; tidak ada filter server, kerahasiaan, maupun status TIDAK BERLAKU yang seragam.

## 2. Proses bisnis

1. `GET /api/v1/corporate/human-resource/administrative-documents` menggabungkan: SK Kepegawaian (Terjadwal/Diterapkan/Dibatalkan), Surat Tugas terbit, Surat Keterangan terbit/dicabut, dan dokumen personal (tanpa salinan arsip Surat Keterangan agar tidak ganda).
2. Filter di server: sumber, jenis, pegawai, unit penempatan, rentang tanggal, keberlakuan (`BERLAKU`/`TERJADWAL`/`TIDAK_BERLAKU`/`KEDALUWARSA`), kerahasiaan, kata kunci; paging dan urutan tanggal terbaru.
3. Dokumen rahasia (surat finansial, dokumen personal rahasia) hanya tampil bagi pemegang `WorkforceDocument : ViewConfidential`.
4. Setiap baris memuat nomor, versi, pemilik, status sumber, label keberlakuan, level akses, riwayat status (diajukan/disetujui/diterapkan/terbit/dicabut + BAP/dibatalkan), dan `AccessPath` ke endpoint terotorisasi sumbernya.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`HrdPersonnelActionOrder`, `TrxEmployeeDocumentIssuance`, `WfpDocument`, `HrdAssignmentLetter`, frontend `administrative-documents-view.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/HrServiceManagement/Services/AdministrativeDocumentRegistryService.cs` | Registri lintas sumber |
| `Areas/Corporate/HumanResource/HrServiceManagement/Controllers/AdministrativeDocumentRegistryController.cs` | Endpoint baru |
| `Areas/Corporate/HumanResource/HrServiceManagement/DTOs/AdministrativeDocumentRegistryDtos.cs` | DTO + konstanta sumber/keberlakuan |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint baru `GET …/administrative-documents` |
| Database | Tanpa migration sendiri (membaca tabel Surat Tugas baru) |
| Keamanan/Auth | Aksi baru `AdministrativeDocumentRegistry : Read`; kerahasiaan via `WorkforceDocument : ViewConfidential` |

## 4. Dokumentasi endpoint

#### HR Services - Administrative Document Registry

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/human-resource/administrative-documents` | Registri berpaging + filter server | `AdministrativeDocumentRegistry : Read` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratchpad>` | Berhasil, 0 error; himpunan warning (tanpa nomor baris) sama dengan sebelum perubahan — tambahan 6 instans berasal dari commit pengguna `a7c37c56` pada `HrdInitialPlacementBatchService.cs`, bukan dari perubahan ini | `PASS` | Log build 20 November 2026 |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` (18 berkas slice Administrasi Kepegawaian) | 18 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran: pengguna tanpa `ViewConfidential` | Dokumen rahasia tidak tampil | `PASS` | filter `IsFinancialSensitive`/`IsConfidential` |
| Penelusuran: Surat Keterangan dicabut | `TIDAK BERLAKU` + riwayat BAP | `PASS` | `History` |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Filter jenis, pegawai, unit, sumber, tanggal, status, kerahasiaan di server | Terpenuhi | `AdministrativeDocumentQuery` |
| (2) Dokumen rahasia hanya bagi peran berhak; unduhan terotorisasi | Terpenuhi | penanda `ViewConfidential`; `AccessPath` endpoint berizin |
| (3) Dokumen dicabut tampil TIDAK BERLAKU beserta riwayat | Terpenuhi | `Validity` + `StatusHistory` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Batas teknis | Paging gabungan mengambil maksimal `halaman × ukuran` baris per sumber lalu digabung; cocok untuk penelusuran berhalaman |
| Dampak frontend | `administrative-documents-view.jsx` dapat memakai satu endpoint ini |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit; tidak ada stage/commit/push |
