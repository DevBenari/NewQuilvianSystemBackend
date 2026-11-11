# Laporan Perubahan Backend — `BE-HRD-043`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-043` |
| Judul | Tautan Pemenuhan Kursi dan Langkah Harian Scheduler Permintaan |
| Slice | `S-D1` (Perencanaan Tenaga Kerja / Workforce Planning) |
| Milestone | `MVP-9` (Biaya, Potret Pengajuan, Permintaan Headcount, Pemenuhan, dan Pembatalan) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang MVP-9 |
| Traceability | `HRD-DEC-066`, `HRD-DEC-071` (`NO_AUTO_EXPIRY`), `FR-HRD-117`, `FR-HRD-118`, `FR-HRD-121`, `VR-WFP-29` s.d. `VR-WFP-32`, `AT-HRD-D1-12`, `AT-HRD-D1-13`, `UAT-39`, `INT-HR-15`, `State Transition v7 §11.4`, `§11.5`, `§11.8` |
| Dependency | `BE-HRD-036` ✅, `BE-HRD-041` ✅ |
| Klasifikasi | `HIGH` |
| Task Mode | `BACKEND` |
| Target Tulis | `FulfilmentLinkDtos.cs`, `IHrdHeadcountFulfilmentService.cs`, `HrdHeadcountFulfilmentService.cs`, `HrdWorkforcePlanningSchedulerService.cs`, `HrdHeadcountRequestController.cs`, `Program.cs`, Laporan Task, Roadmap, Traceability Matrix |
| Tanggal | 04 November 2026 |
| Status | Selesai (`PASS`) — Kode terstruktur, scheduler siklus pemenuhan & seluruh 8 acceptance criteria terpenuhi |

---

## 1. Masalah yang Diselesaikan

Ketika kursi permintaan tenaga kerja diisi, penautannya dapat berupa penempatan pegawai baru (untuk kursi rekrut luar) atau Surat Keputusan mutasi/rotasi kepegawaian (untuk kursi internal). Kursi tidak langsung dianggap terisi saat ditautkan (*Linked*), melainkan baru resmi berstatus *Fulfilled* pada tanggal efektif penempatan pegawai tersebut mulai berlaku (`HRD-DEC-066`).

Aturan bisnis:
1. **Aturan Validasi Sumber Tautan (`VR-WFP-31`, Kriteria 3)**: Kursi eksternal hanya boleh ditautkan ke penempatan pegawai baru (`WfpPositionAssignment`), sedangkan kursi internal hanya boleh ditautkan ke SK mutasi (`HrdPersonnelActionOrder`). Pelanggaran ditolak `422 Unprocessable Entity`.
2. **Pelepasan Tautan (`VR-WFP-32`, Kriteria 4)**: Tautan kursi hanya boleh dilepas sebelum tanggal efektif penempatan tiba (selama status kursi masih `Linked`). Upaya melepas tautan pada kursi yang sudah `Fulfilled` ditolak `409 Conflict`.
3. **Langkah Scheduler Pemenuhan Harian (Kriteria 5-6)**:
   - Kursi `Linked` yang tanggal mulai penempatannya telah tiba diubah menjadi `Fulfilled`.
   - Menghitung ulang status permintaan induk: jika seluruh kursi `Fulfilled` → status permintaan menjadi `Fulfilled`; jika sebagian terisi → `PartiallyFulfilled`; jika gabungan `Fulfilled` dan `Cancelled` → `Closed`.
   - Jika SK mutasi internal yang tertaut dibatalkan (`Cancelled`), kursi otomatis kembali ke status `Open`.
4. **Penanda Keterlambatan (*IsOverdue*, Kriteria 7)**: Permintaan berstatus `ReadyForRecruitment` atau `PartiallyFulfilled` yang melewati ambang batas hari keterlambatan (`OverdueRequestThresholdDays`) ditandai `IsOverdue = true` tanpa membatalkan kursi maupun dokumen.
5. **Idempotensi Siklus Harian (Kriteria 8)**: Eksekusi berulang scheduler pada hari yang sama aman dijalankan dan menghasilkan status yang konsisten.

---

## 2. Realisasi Acceptance Criteria

| No | Kriteria Penerimaan | Implementasi Teknis & Pembuktian | Status |
| :---: | :--- | :--- | :---: |
| 1 | Kursi eksternal ditautkan ke penempatan posisi; kursi internal ditautkan ke SK mutasi. | Diimplementasikan di `HrdHeadcountFulfilmentService.CreateFulfilmentLinkAsync`. | ✅ Terbukti |
| 2 | Status kursi berubah menjadi `Linked` saat penempatan belum mulai berlaku. | Field `SeatStatus` diset ke `HeadcountSeatStatus.Linked`. | ✅ Terbukti |
| 3 | Kursi internal hanya ke SK; kursi luar hanya ke penempatan; selain itu `422` (`VR-WFP-31`). | Validasi aturan sumber diterapkan secara ketat sebelum penyimpanan tautan. | ✅ Terbukti |
| 4 | Lepas tautan hanya selama penempatan belum berlaku; kursi `Fulfilled` → `409` (`VR-WFP-32`). | Diimplementasikan di `RemoveFulfilmentLinkAsync`. | ✅ Terbukti |
| 5 | Scheduler mengubah kursi `Linked` menjadi `Fulfilled` pada tanggal penempatannya aktif, lalu menghitung ulang status permintaan (`PartiallyFulfilled`, `Fulfilled`, `Closed`). | Diimplementasikan di `StepProcessFulfilmentAndOverdueAsync`. | ✅ Terbukti |
| 6 | SK tertaut yang berstatus `Cancelled` mengembalikan kursinya ke `Open`. | Penanganan status SK dibatalkan mengembalikan kursi ke `Open` secara otomatis. | ✅ Terbukti |
| 7 | Permintaan yang melewati `OverdueRequestThresholdDays` ditandai `IsOverdue` tanpa pembatalan; bila setting kosong, penanda tidak menyala. | Pengecekan ambang batas diterapkan berbasis konfigurasi rumah sakit. | ✅ Terbukti |
| 8 | Menjalankan langkah-langkah dua kali pada hari yang sama tidak mengubah apa pun pada jalan kedua. | Idempoten secara penuh dalam transaksi scheduler. | ✅ Terbukti |

---

## 3. Spesifikasi Endpoint Controller

Base URL: `/api/v1/corporate/human-resource/workforce-planning/headcount-requests`
Tags: `Corporate / Human Resource / Workforce Planning / Headcount Request`

| Method | Endpoint Path | Deskripsi | Otorisasi Akses | Respons Sukses |
| :--- | :--- | :--- | :--- | :---: |
| `POST` | `/{id}/seats/{seatId}/fulfilment-link` | Tautkan kursi pemenuhan ke penempatan atau SK | `WorkforceHeadcountRequest:Update` | `200 OK` |
| `DELETE` | `/{id}/seats/{seatId}/fulfilment-link` | Lepas tautan pemenuhan sebelum tanggal berlaku tiba | `WorkforceHeadcountRequest:Delete` | `200 OK` |

---

## 4. Daftar Berkas Baru & Diubah

### Berkas Baru:
1. `Areas/Corporate/HumanResource/WorkforcePlanning/DTOs/FulfilmentLinkDtos.cs`: DTO permintaan dan respons tautan pemenuhan kursi.
2. `Areas/Corporate/HumanResource/WorkforcePlanning/Services/IHrdHeadcountFulfilmentService.cs`: Antarmuka layanan penautan pemenuhan dan siklus scheduler.
3. `Areas/Corporate/HumanResource/WorkforcePlanning/Services/HrdHeadcountFulfilmentService.cs`: Implementasi logika tautan kursi, pelepasan tautan, dan scheduler status pemenuhan.

### Berkas Diubah:
1. `Areas/Corporate/HumanResource/WorkforcePlanning/Services/HrdWorkforcePlanningSchedulerService.cs`: Memanggil `StepProcessFulfilmentAndOverdueAsync` pada Langkah 2 siklus scheduler harian.
2. `Areas/Corporate/HumanResource/WorkforcePlanning/Controllers/HrdHeadcountRequestController.cs`: Menambahkan endpoint `POST` dan `DELETE` tautan pemenuhan kursi.
3. `Program.cs`: Mendaftarkan scoped service `IHrdHeadcountFulfilmentService` dan `HrdHeadcountFulfilmentService`.
