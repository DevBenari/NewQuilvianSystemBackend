# Laporan Perubahan Backend — `BE-HRD-039`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-039` |
| Judul | Potret Gap Pengajuan dan API Baca Potret |
| Slice | `S-D1` (Perencanaan Tenaga Kerja / Workforce Planning) |
| Milestone | `MVP-9` (Biaya, Potret Pengajuan, Permintaan Headcount, Pemenuhan, dan Pembatalan) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang MVP-9 |
| Traceability | `HRD-DEC-069` (`SNAPSHOT_IMMUTABLE`, `SUBMITTED_VS_CURRENT`), `FR-HRD-105`, `VR-WFP-39`, `AT-HRD-D1-16`, `State Transition v7 §11.7`, `API v7 §13 grup Staffing Gap` |
| Dependency | `BE-HRD-037` ✅ |
| Klasifikasi | `HIGH` |
| Task Mode | `BACKEND` |
| Target Tulis | `StaffingGapSnapshotDtos.cs`, `IHrdStaffingGapSnapshotService.cs`, `HrdStaffingGapSnapshotService.cs`, `HrdStaffingGapController.cs`, `Program.cs`, Laporan Task, Roadmap, Traceability Matrix |
| Tanggal | 04 November 2026 |
| Status | Selesai (`PASS`) — Kode terstruktur, potret gap immutable & seluruh 4 acceptance criteria terpenuhi |

---

## 1. Masalah yang Diselesaikan

Ketika sebuah permohonan penambahan pegawai (*Headcount Request*) atau rencana tahunan (*Manpower Plan*) diajukan ke jajaran pimpinan untuk persetujuan, angka kesenjangan tenaga kerja pada saat pengajuan tersebut harus dibekukan (*snapshot*). Hal ini penting agar para penyetuju (*approver*) dapat membandingkan kondisi riil saat diajukan dengan kondisi berjalan tanpa terpengaruh oleh mutasi pegawai yang terjadi setelah tanggal pengajuan (`HRD-DEC-069`).

Aturan bisnis:
1. **Perekaman Potret Pengajuan (`Submission Snapshot`)**: Dilakukan otomatis saat pengajuan dokumen di dalam transaksi yang sama. Seluruh baris unit dan jabatan beserta nilai kesenjangannya disimpan secara utuh ke `HrdStaffingGapSnapshot` dan `HrdStaffingGapSnapshotLine`. Nilai `null` pada data sumber tetap dipertahankan `null` sesuai prinsip `UNKNOWN_IS_NOT_ZERO`.
2. **Kekekalan Potret Gap (`VR-WFP-39`, Kriteria 3)**: Potret gap bersifat *immutable*. Sistem tidak menyediakan endpoint untuk mengubah (`PUT`) atau menghapus (`DELETE`) potret gap. Setiap perubahan penempatan pegawai di kemudian hari tidak akan pernah mengubah data potret yang telah diambil (Kriteria 4).
3. **Endpoint Penelusuran Historis (Kriteria 2)**:
   - `GET /api/v1/corporate/human-resource/workforce-planning/staffing-gaps/snapshots`: Menampilkan daftar potret berhalaman dengan filter jenis potret, tanggal acuan, dan dokumen sumber.
   - `GET /api/v1/corporate/human-resource/workforce-planning/staffing-gaps/snapshots/{id}`: Menampilkan detail lengkap potret beserta seluruh baris rincian unit dan jabatan.

---

## 2. Realisasi Acceptance Criteria

| No | Kriteria Penerimaan | Implementasi Teknis & Pembuktian | Status |
| :---: | :--- | :--- | :---: |
| 1 | Service menulis potret `Submission` beserta baris dan penanda kelengkapannya di dalam transaksi pemanggil; nilai `null` tetap `null`. | Diimplementasikan di `HrdStaffingGapSnapshotService.CaptureSubmissionSnapshotAsync`. Memetakan hasil kalkulasi gap ke baris snapshot dengan mempertahankan nilai `null`. Tidak membuka transaksi baru agar ikut transaksi pemanggil. | ✅ Terbukti |
| 2 | `GET /snapshots` dan `GET /snapshots/{id}` mengembalikan potret beserta jenisnya. | Diimplementasikan di `HrdStaffingGapController.GetSnapshots` dan `GetSnapshotById`. Mendukung filter dan paginasi. | ✅ Terbukti |
| 3 | Tidak ada endpoint ubah maupun hapus potret; method tulis selain pembuatan dokumen pemanggil tidak disediakan (`VR-WFP-39`). | Tidak ada method `PUT`, `PATCH`, atau `DELETE` pada controller. Potret bersifat read-only. | ✅ Terbukti |
| 4 | Data penempatan yang berubah sesudah potret diambil tidak mengubah isi potret. | Data disimpan sebagai entitas fisik terpisah `HrdStaffingGapSnapshotLine`, terisolasi dari mutasi tabel operasional penempatan pegawai. | ✅ Terbukti |

---

## 3. Spesifikasi Endpoint Controller

Base URL: `/api/v1/corporate/human-resource/workforce-planning/staffing-gaps`
Tags: `Corporate / Human Resource / Workforce Planning / Staffing Gap`

| Method | Endpoint Path | Deskripsi | Otorisasi Akses | Respons Sukses |
| :--- | :--- | :--- | :--- | :---: |
| `GET` | `/snapshots` | Ambil daftar potret gap berhalaman (filter jenis, dokumen sumber, periode) | `WorkforceStaffingGap:Read` | `200 OK` (Paged) |
| `GET` | `/snapshots/{id}` | Ambil detail satu potret gap beserta seluruh baris rincian unit dan jabatan | `WorkforceStaffingGap:Read` | `200 OK` |

---

## 4. Daftar Berkas Baru & Diubah

### Berkas Baru:
1. `Areas/Corporate/HumanResource/WorkforcePlanning/DTOs/StaffingGapSnapshotDtos.cs`: DTO potret gap, baris detail, dan query berhalaman.
2. `Areas/Corporate/HumanResource/WorkforcePlanning/Services/IHrdStaffingGapSnapshotService.cs`: Antarmuka layanan perekaman potret pengajuan dan pembacaan.
3. `Areas/Corporate/HumanResource/WorkforcePlanning/Services/HrdStaffingGapSnapshotService.cs`: Implementasi perekaman potret pengajuan dan query pembacaan.

### Berkas Diubah:
1. `Areas/Corporate/HumanResource/WorkforcePlanning/Controllers/HrdStaffingGapController.cs`: Menambahkan endpoint `GET /snapshots` dan `GET /snapshots/{id}`.
2. `Program.cs`: Mendaftarkan scoped service `IHrdStaffingGapSnapshotService` dan `HrdStaffingGapSnapshotService`.
