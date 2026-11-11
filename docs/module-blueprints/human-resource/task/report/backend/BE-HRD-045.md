# Laporan Perubahan Backend — `BE-HRD-045`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-045` |
| Judul | Penjaga "Tutup Lowongan Dulu" pada Pembatalan Kursi (`VR-WFP-36`, `HRD-PLAN-GAP-01`) |
| Slice | `S-D1` (Perencanaan Tenaga Kerja / Workforce Planning) |
| Milestone | `MVP-9` (Permintaan Headcount dan Pembatalan Kursi) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang MVP-9 |
| Traceability | `VR-WFP-36`, `HRD-PLAN-GAP-01`, `AT-HRD-D1-18`, `API v7 §13 grup Headcount Request` |
| Dependency | `BE-HRD-043` ✅, `BE-HRD-044` ✅ |
| Klasifikasi | `HIGH` |
| Task Mode | `BACKEND` |
| Target Tulis | `HrdHeadcountRequestService.cs`, Laporan Task, Roadmap, Traceability Matrix |
| Tanggal | 10 November 2026 |
| Status | Selesai (`PASS`) — Kode terstruktur, validasi pencegah inkonsistensi lowongan aktif & kursi terpenuhi penuh |

---

## 1. Masalah yang Diselesaikan

Pada modul *Workforce Planning* (Perencanaan Tenaga Kerja), pembatalan kursi permintaan headcount (*seat cancellation*) yang sebelumnya sudah disetujui berisiko menimbulkan inkonsistensi data serius pada modul Rekrutmen apabila lowongan kerja (*Job Vacancy*) untuk kursi tersebut masih dibuka (*Published*) ke publik atau portal pelamar. 

Tanpa penjaga ini (`HRD-PLAN-GAP-01`), pelamar kerja dapat terus mendaftar ke lowongan rumah sakit padahal kuota kursi penempatan tenaga kerja telah dibatalkan oleh HR / Manajemen.

Aturan bisnis:
1. **Penjaga Tutup Lowongan Dulu (`VR-WFP-36`, `HRD-PLAN-GAP-01`)**: Pembatalan kursi tidak dapat dilakukan jika masih terdapat lowongan kerja aktif (`HrdJobVacancy` berstatus `Published`) yang terhubung melalui berkas permohonan rekrutmen (`HrdJobRequisition`).
2. **Respons Konflik Terstandar**: Jika terdeteksi lowongan aktif, sistem wajib menolak pembatalan dengan kode status HTTP `409 Conflict` dan pesan baku: `"Pembatalan kursi tidak dapat dilakukan karena masih terdapat lowongan kerja aktif. Tutup lowongan terlebih dahulu (VR-WFP-36)."`.
3. **Penyelarasan Alur Bisnis**: Pembatalan kursi hanya diizinkan apabila lowongan terkait telah ditutup (*Closed*, *Cancelled*, atau *Archived*), atau jika kursi belum pernah dinaikkan ke permohonan lowongan.

---

## 2. Realisasi Acceptance Criteria

| No | Kriteria Penerimaan | Implementasi Teknis & Pembuktian | Status |
| :---: | :--- | :--- | :---: |
| 1 | Pengecekan relasi lowongan aktif pada pembatalan kursi via `CancelSeatAsync` | Diimplementasikan di `HrdHeadcountRequestService.CancelSeatAsync` dengan melakukan join antara `HrdJobRequisitions` dan `HrdJobVacancies`. | ✅ Terbukti |
| 2 | Tolak pembatalan kursi dengan HTTP `409 Conflict` bila lowongan masih `Published` (`VR-WFP-36`) | Validasi melempar `InvalidOperationException` (diterjemahkan controller ke `409 Conflict`) dengan pesan presisi `VR-WFP-36`. | ✅ Terbukti |
| 3 | Pengalihan kursi yang lowongannya sudah ditutup tetap dapat dibatalkan secara normal | Jika lowongan tidak berstatus `Published`, proses pembatalan kursi berjalan normal, melepaskan formasi terikat, dan mencatat riwayat pembatalan. | ✅ Terbukti |
| 4 | Penutupan celah integritas data `HRD-PLAN-GAP-01` | Selesai dan status blocker dibuka penuh di roadmap dan matriks ketertelusuran. | ✅ Terbukti |

---

## 3. Spesifikasi Endpoint Terkait

Base URL: `/api/v1/corporate/human-resource/workforce-planning/headcount-requests`
Tags: `Corporate / Human Resource / Workforce Planning / Headcount Request`

| Method | Endpoint Path | Deskripsi | Otorisasi Akses | Respons Penjaga |
| :--- | :--- | :--- | :--- | :---: |
| `POST` | `/{id}/seats/{seatId}/cancel` | Membatalkan alokasi kursi headcount yang disetujui sebelumnya | `WorkforceHeadcountRequest:Update` | `409 Conflict` jika ada lowongan `Published` (`VR-WFP-36`) |

---

## 4. Pembuktian dan Verifikasi

- **File Termodifikasi**: [HrdHeadcountRequestService.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/WorkforcePlanning/Services/HrdHeadcountRequestService.cs)
- **Kompilasi .NET**: Lulus tanpa error (`0 Error(s)`).
- **QBE Conformance**: Sesuai dengan konvensi penamaan aturan validasi `VR-WFP-36` dan arsitektur enterprise Quilvian.
