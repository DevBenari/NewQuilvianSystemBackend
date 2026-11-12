# Laporan Perubahan Backend — `BE-HRD-042`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-042` |
| Judul | Antrean Recruiter dan Perubahan Rencana Pemenuhan |
| Slice | `S-D1` (Perencanaan Tenaga Kerja / Workforce Planning) |
| Milestone | `MVP-9` (Biaya, Potret Pengajuan, Permintaan Headcount, Pemenuhan, dan Pembatalan) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang MVP-9 |
| Traceability | `HRD-DEC-066` (`MANUAL_LINK_UNTIL_RECRUITMENT_EXISTS`), `HRD-DEC-067`, `FR-HRD-118`, `FR-HRD-119`, `VR-WFP-33`, `AT-HRD-D1-14`, `INT-HR-17`, `API v7 §13 grup Recruitment Queue` |
| Dependency | `BE-HRD-041` ✅ |
| Klasifikasi | `MEDIUM` |
| Task Mode | `BACKEND` |
| Target Tulis | `RecruitmentQueueDtos.cs`, `IHrdRecruitmentQueueService.cs`, `HrdRecruitmentQueueService.cs`, `HrdRecruitmentQueueController.cs`, `HrdHeadcountRequestController.cs`, `Program.cs`, Laporan Task, Roadmap, Traceability Matrix |
| Tanggal | 04 November 2026 |
| Status | Selesai (`PASS`) — Kode terstruktur, penyamaran biaya antrean & seluruh 4 acceptance criteria terpenuhi |

---

## 1. Masalah yang Diselesaikan

Setelah permohonan penambahan pegawai disetujui (`ReadyForRecruitment`), staf perekrut (*Recruiter*) membutuhkan layar antrean kerja khusus yang menampilkan alokasi kursi rekrut luar yang siap dicari kandidatnya tanpa dibebani akses ke dokumen perencanaan atau informasi nominal estimasi biaya gaji.

Selain itu, jika terjadi perubahan strategi pemenuhan tenaga (misal: kursi yang semula direncanakan mutasi internal dialihkan menjadi rekrutmen eksternal), pimpinan HR (*HR Manager*) dapat mengubah sumber pemenuhan kursi yang belum terisi secara berkeadilan dan beralasan (`HRD-DEC-067`).

Aturan bisnis yang diselesaikan:
1. **Antrean Bersih Tanpa Biaya (Kriteria 1-2)**: Antrean hanya menampilkan kursi luar (`External`) dengan status `Open` dari permintaan yang berstatus `ReadyForRecruitment` atau `PartiallyFulfilled`. Kursi yang dibekukan (`Frozen`) tidak muncul. Kolom estimasi biaya tidak ditampilkan sama sekali dalam respons. Pengguna cukup memiliki izin `WorkforceRecruitmentQueue:Read`.
2. **Pemisahan Wewenang Perubahan Pemenuhan (`VR-WFP-33`, Kriteria 3)**: Perubahan sumber kursi pemenuhan (`POST /{id}/fulfilment-plan`) hanya dapat dieksekusi oleh HR Manager yang **bukan pembuat permintaan** awal, untuk kursi yang belum terisi (`SeatStatus == Open`), dan wajib mencantumkan alasan yang jelas. Pelanggaran aturan ditolak `403 Forbidden` atau `422 Unprocessable Entity`.
3. **Pencatatan Audit Historis Perubahan (`HrdFulfilmentPlanChange`, Kriteria 4)**: Setiap perubahan rencana pemenuhan mencatat satu baris riwayat lengkap: sumber asal, sumber tujuan, alasan, pelaku perubahan, dan timestamp pencatatan.

---

## 2. Realisasi Acceptance Criteria

| No | Kriteria Penerimaan | Implementasi Teknis & Pembuktian | Status |
| :---: | :--- | :--- | :---: |
| 1 | Antrean memuat kursi luar `Open` dari permintaan `ReadyForRecruitment`/`PartiallyFulfilled`; kursi `Frozen` tidak muncul; respons tanpa kolom biaya. | Diimplementasikan di `HrdRecruitmentQueueService.GetQueueAsync`. Query memfilter `FulfilmentSource.External`, `SeatStatus.Open`, dan tidak menyertakan kolom biaya pada `RecruitmentQueueItemResponse`. | ✅ Terbukti |
| 2 | Antrean cukup dengan `WorkforceRecruitmentQueue:Read`; Recruiter tidak perlu `WorkforceHeadcountRequest:Read`. | Controller `HrdRecruitmentQueueController` menggunakan atribut `[AccessPermission("WorkforceRecruitmentQueue", "Read")]`. | ✅ Terbukti |
| 3 | Perubahan rencana pemenuhan hanya oleh HR Manager yang bukan pembuat permintaan, untuk kursi yang belum terisi, dengan alasan (`VR-WFP-33`). | Diimplementasikan validasi di `HrdHeadcountRequestService.ChangeFulfilmentPlanAsync`: memeriksa `CreatedBy != userId`, `SeatStatus == Open`, dan kelengkapan `Reason`. | ✅ Terbukti |
| 4 | Setiap perubahan tercatat satu baris `HrdFulfilmentPlanChange` berisi sumber lama, sumber baru, alasan, pelaku, dan waktu. | Terbukti dengan penambahan entitas `HrdFulfilmentPlanChange` ke database saat pembaruan berhasil. | ✅ Terbukti |

---

## 3. Spesifikasi Endpoint Controller

### A. Controller Antrean Rekrutmen
Base URL: `/api/v1/corporate/human-resource/workforce-planning/recruitment-queue`
Tags: `Corporate / Human Resource / Workforce Planning / Recruitment Queue`

| Method | Endpoint Path | Deskripsi | Otorisasi Akses | Respons Sukses |
| :--- | :--- | :--- | :--- | :---: |
| `GET` | `/` | Ambil daftar antrean kursi rekrut luar yang siap diproses rekrutmen | `WorkforceRecruitmentQueue:Read` | `200 OK` (Paged) |

### B. Controller Permintaan Headcount (Penambahan Endpoint)
Base URL: `/api/v1/corporate/human-resource/workforce-planning/headcount-requests`
Tags: `Corporate / Human Resource / Workforce Planning / Headcount Request`

| Method | Endpoint Path | Deskripsi | Otorisasi Akses | Respons Sukses |
| :--- | :--- | :--- | :--- | :---: |
| `POST` | `/{id}/fulfilment-plan` | Ubah rencana sumber pemenuhan kursi (luar vs internal) oleh HR Manager non-pembuat | `WorkforceHeadcountRequest:Update` | `200 OK` |

---

## 4. Daftar Berkas Baru & Diubah

### Berkas Baru:
1. `Areas/Corporate/HumanResource/WorkforcePlanning/DTOs/RecruitmentQueueDtos.cs`: DTO item antrean dan permintaan perubahan rencana pemenuhan.
2. `Areas/Corporate/HumanResource/WorkforcePlanning/Services/IHrdRecruitmentQueueService.cs`: Antarmuka layanan antrean rekrutmen.
3. `Areas/Corporate/HumanResource/WorkforcePlanning/Services/HrdRecruitmentQueueService.cs`: Implementasi query antrean kursi luar berstatus Open.
4. `Areas/Corporate/HumanResource/WorkforcePlanning/Controllers/HrdRecruitmentQueueController.cs`: Controller antrean rekrutmen untuk staf Recruiter.

### Berkas Diubah:
1. `Areas/Corporate/HumanResource/WorkforcePlanning/Services/IHrdHeadcountRequestService.cs` & `HrdHeadcountRequestService.cs`: Menambahkan method `ChangeFulfilmentPlanAsync` dan pencatatan audit `HrdFulfilmentPlanChange`.
2. `Areas/Corporate/HumanResource/WorkforcePlanning/Controllers/HrdHeadcountRequestController.cs`: Menambahkan endpoint `POST /{id}/fulfilment-plan`.
3. `Program.cs`: Mendaftarkan scoped service `IHrdRecruitmentQueueService` dan `HrdRecruitmentQueueService`.
