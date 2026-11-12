# Laporan Perubahan Backend — `BE-HRD-027`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-027` |
| Judul | Layanan dan Controller Penempatan Awal Batch 55 Pegawai Baseline |
| Slice | `S-A1` (Workforce Core & Employee 360) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang 10 |
| Trace | `FR-HRD-017`, `HRD-DEC-050` |
| Dependency | `BE-HRD-023` |
| Klasifikasi | `MEDIUM` |
| Task mode | `BACKEND` |
| Target tulis | DTOs, Service, Controller, Program.cs, Laporan Task |
| Tanggal | 02 November 2026 |
| Status | Selesai (`PASS`) |

---

## 1. Masalah yang Diselesaikan

Audit kapabilitas sistem menemukan bahwa 55 profil pegawai baseline telah memiliki penempatan organisasi di database, tetapi 0 memiliki penempatan jabatan (`WfpPositionAssignment`), 0 memiliki penempatan atasan (`WfpManagerAssignment`), dan 0 memiliki penetapan gaji (`WfpSalaryAssignment`). Menginput satu per satu 55 SK mutasi untuk data awal rumah sakit tidak efisien dan rentan kesalahan. Sesuai keputusan `HRD-DEC-050`, diperlukan sarana batching penempatan awal (Initial Placement Batch) yang memvalidasi dan mengeksekusi penempatan awal dalam satu transaksi database atomik.

---

## 2. Proses Bisnis Rumah Sakit

1. **Pengambilan Template Pra-Isi (`GET /template`)**:
   - Sistem membaca 55 pegawai baseline yang telah memiliki penempatan organisasi aktif namun belum memiliki jabatan aktif.
   - Menghasilkan daftar baris siap isi dengan nama, kode pegawai, dan unit organisasi asal.
2. **Validasi Integritas Data (`POST /validate`)**:
   - Memastikan jabatan yang dipilih ada dan aktif.
   - Memastikan atasan yang dipilih aktif dan bukan diri pegawai itu sendiri.
   - Memastikan tanggal mulai berlaku valid.
3. **Penyimpanan Draft Batch (`POST /`)**:
   - Menyimpan batch dengan penomoran unik `BATCH-INIT-yyyyMMdd-0001`.
4. **Eksekusi Atomik Massal (`POST /{id}/execute`)**:
   - Dalam satu transaksi database atomik (`BeginTransactionAsync`):
     - Membentuk `WfpPositionAssignment` utama baru untuk setiap pegawai.
     - Membentuk `WfpManagerAssignment` utama baru untuk setiap pegawai (jika atasan ditentukan).
     - Membentuk `WfpSalaryAssignment` utama baru untuk setiap pegawai (jika nominal gaji ditentukan).
     - Menandai seluruh baris batch dan status batch sebagai `Executed`.

---

## 3. Rincian Perubahan yang Dikerjakan

1. `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/WorkforceCore/DTOs/InitialPlacementBatchDtos.cs` — DTO template, validasi, pembuatan, dan eksekusi batch.
2. `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/WorkforceCore/Services/IHrdInitialPlacementBatchService.cs` — Antarmuka service batch penempatan awal.
3. `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/WorkforceCore/Services/HrdInitialPlacementBatchService.cs` — Logika pra-isi template, validasi integritas baris, dan eksekusi transaksi massal.
4. `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/WorkforceCore/Controllers/HrdInitialPlacementBatchController.cs` — Controller Web API RESTful dengan tag Swagger dan otorisasi terpusat.
5. `NewQuilvianSystemBackend/Program.cs` — Pendaftaran service batch penempatan awal ke DI container.

---

## 4. Verifikasi Integritas Transaksi

* Operasi eksekusi batch dijamin sepenuhnya atomik: jika terjadi kegagalan pada salah satu baris pegawai, seluruh perubahan dibatalkan (`RollbackAsync`) sehingga tidak meninggalkan database dalam kondisi setengah terisi.
