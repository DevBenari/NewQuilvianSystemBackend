# Laporan Penyelesaian Backend Task: BE-HRD-195

## 1. Identitas Task
* **ID Task**: `BE-HRD-195`
* **Nama Task**: Agregat Tiket Helpdesk SDM & Pengajuan Mandiri ESS (`HrServiceTicketService`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 13 — HR Service Management & Request
* **Slice / Milestone**: `Slice S-D4` · `Fase MVP-39: Katalog, Template Token Dinamis & Agregat Tiket Helpdesk`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-09
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Pegawai rumah sakit, dokter, bidan, perawat, maupun staf administrasi membutuhkan satu pintu terpadu untuk menyampaikan kendala dan kebutuhan kepegawaian tanpa harus mendatangi kantor HRD secara fisik. Agregat tiket helpdesk SDM menyediakan portal mandiri pegawai (*Employee Self-Service / ESS*) dan antrean kerja analis (*Workplace Helpdesk Queue*).

Fitur-fitur utama yang dibangun dalam task ini:
1. **Pengajuan Tiket Mandiri Pegawai (Portal ESS, `FR-HRS-06`)**:
   - Pegawai dapat memilih kategori dan jenis layanan dari katalog resmi, mengisi subjek, deskripsi kebutuhan, serta memilih tier prioritas (`Standar`, `Khusus`, `Cito`).
   - Sistem menerbitkan nomor tiket unik otomatis berformat `TKT-YYYYMM-XXXXX`.
2. **Validasi Ketat Prioritas Cito (`HRS-B1`, `VR-HRS-02`)**:
   - Pengajuan prioritas **Cito** (SLA kilat 4 jam) **wajib** menyertakan nomor referensi surat disposisi pimpinan medis/keperawatan (`EmergencyDispositionReference`).
   - Pengajuan Cito tanpa lampiran/nomor disposisi sah ditolak oleh sistem dengan kode error `VR-HRS-02`.
3. **Kalkulasi Tenggat Waktu Awal SLA (`SlaDeadlineUtc`, `SlaTargetHours`)**:
   - Menghitung batas waktu deadline secara presisi: Tier Cito = 4 jam kalender, Standar = 24 jam kerja, Khusus = 72 jam kerja.
4. **Meja Kerja Antrean Helpdesk Analis SDM (`HRS-W1`)**:
   - Menampilkan seluruh tiket dengan filter status, tier prioritas, dan penugasan analis.
   - Fitur *Claim / Re-assign* tiket ke petugas HR tertentu (`AssignTicket`), mencatat waktu respons pertama (`FirstResponseAt`).
5. **Manajemen Status Tiket & Resolusi**:
   - Transisi status: `Submitted` $\rightarrow$ `Assigned` $\rightarrow$ `InReview` $\rightarrow$ `WaitingStaffResponse` $\rightarrow$ `InProgress` $\rightarrow$ `Resolved` $\rightarrow$ `Closed` / `Cancelled`.
   - Pencatatan ringkasan resolusi (*resolution summary*) dan alasan pembatalan.
6. **Utas Komentar Klarifikasi Berkas & Catatan Internal HR (`HRS-W2`)**:
   - Komunikasi dua arah antara analis HR dan pegawai pemohon.
   - Catatan internal HR (`IsInternalNote = true`) disembunyikan secara otomatis dari portal ESS pegawai.
   - Mengirim balasan dari pegawai otomatis meresume status dari `WaitingStaffResponse` kembali ke `InReview`.

---

## 3. Komponen Teknis yang Dibuat & Dimodifikasi

### A. DTOs
File: `Areas/Corporate/HumanResource/HrServiceManagement/DTOs/HrServiceTicketDtos.cs`
* `HrServiceTicketDto`, `CreateHrServiceTicketRequestDto`, `AssignTicketRequestDto`, `UpdateTicketStatusRequestDto`, `AddTicketCommentRequestDto`, `TicketCommentDto`.

### B. Antarmuka & Layanan Bisnis
File: `Areas/Corporate/HumanResource/HrServiceManagement/Services/IHrServiceTicketService.cs` & `HrServiceTicketService.cs`
* Logika submit tiket ESS, validasi Cito `VR-HRS-02`, penugasan antrean analis, penanganan status, dan utas komentar.

### C. Controllers REST API
1. `Areas/Corporate/HumanResource/HrServiceManagement/Controllers/HrServiceTicketController.cs`
   - Tag Swagger: `[Tags("HR Services - Tickets & Queue")]` (Meja Kerja Analis HR).
2. `Areas/Corporate/HumanResource/HrServiceManagement/Controllers/HrServiceEssController.cs`
   - Tag Swagger: `[Tags("HR Services - ESS Requests & My Documents")]` (Portal Mandiri Pegawai ESS).

---

## 4. Contoh Skenario Rumah Sakit Konkret
* **Skenario Pengajuan Surat Izin Tugas Luar Kota Cito Dokter Bedah**:
  - Dokter Spesialis Bedah dr. Ardi mendapat panggilan darurat untuk melakukan operasi transplantasi ginjal di RSUP rujukan dalam waktu 6 jam.
  - dr. Ardi mengajukan tiket di portal ESS:
    * `Category`: `TRAVEL-REQ`
    * `ServiceType`: `SRV-CITO` (Surat Tugas Darurat Medis)
    * `PriorityTier`: `Cito`
    * `EmergencyDispositionReference`: `DISP-DIRMED-20261109-08` (Surat Disposisi Direktur Medis)
  - Sistem memvalidasi kelengkapan disposisi, menerbitkan nomor tiket `TKT-202611-00015`, dan menetapkan SLA 4 jam (`SlaDeadlineUtc` = 15:00 WIB).
  - Analis HR langsung menerima tiket di urutan teratas antrean (*Cito highlight*), melakukan *claim*, dan menerbitkan surat dalam 45 menit.

---

## 5. Spesifikasi Endpoint REST API

### A. Antrean Helpdesk Analis SDM: `[Tags("HR Services - Tickets & Queue")]`
| Method | Path | Deskripsi | Otorisasi |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/corporate/hrd/services/tickets/queue` | Antrean seluruh tiket permohonan | HR Staff / Admin |
| `GET` | `/api/v1/corporate/hrd/services/tickets/{id}` | Detail tiket helpdesk | HR Staff / Admin |
| `POST` | `/api/v1/corporate/hrd/services/tickets/{id}/assign` | Menugaskan tiket ke analis spesifik | HR Staff / Admin |
| `POST` | `/api/v1/corporate/hrd/services/tickets/{id}/status` | Mengubah status tiket (InReview, Resolved, dll) | HR Staff / Admin |
| `GET` | `/api/v1/corporate/hrd/services/tickets/{id}/comments` | Daftar seluruh komentar & internal notes | HR Staff / Admin |
| `POST` | `/api/v1/corporate/hrd/services/tickets/{id}/comments` | Kirim komentar / catatan internal HR | HR Staff / Admin |

### B. Portal Mandiri Pegawai ESS: `[Tags("HR Services - ESS Requests & My Documents")]`
| Method | Path | Deskripsi | Otorisasi |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/self-service/hr/services/tickets` | Mengajukan tiket permohonan mandiri ESS | Staf RS / ESS |
| `GET` | `/api/v1/self-service/hr/services/tickets/my-tickets` | Daftar tiket permohonan milik pegawai | Staf RS / ESS |
| `GET` | `/api/v1/self-service/hr/services/tickets/{id}` | Detail tiket permohonan milik pegawai | Staf RS / ESS |
| `GET` | `/api/v1/self-service/hr/services/tickets/{id}/comments` | Percakapan klarifikasi (tanpa catatan internal) | Staf RS / ESS |
| `POST` | `/api/v1/self-service/hr/services/tickets/{id}/comments` | Kirim balasan klarifikasi pegawai | Staf RS / ESS |

---

## 6. Verifikasi & Integritas Bukti
* **Kepatuhan Hak Akses**: Catatan internal HR disaring secara ketat sehingga tidak pernah bocor ke antarmuka pegawai umum.
* **Kepatuhan Acceptance Test**: Memenuhi kriteria penerimaan `AT-HRD-HRS-01` dan `AT-HRD-HRS-02`.