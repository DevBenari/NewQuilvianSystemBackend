# Laporan Penyelesaian Backend Task: BE-HRD-196

## 1. Identitas Task
* **ID Task**: `BE-HRD-196`
* **Nama Task**: Berkas Lampiran Tiket & Utas Klarifikasi Dua Arah (`HrServiceTicketAttachment` & Comments)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 13 — HR Service Management & Request
* **Slice / Milestone**: `Slice S-D4` · `Fase MVP-39: Katalog, Template Token Dinamis & Agregat Tiket Helpdesk`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-09
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Kelengkapan dokumen fisik merupakan syarat mutlak dalam proses administrasi kepegawaian rumah sakit (seperti surat disposisi pimpinan untuk izin darurat, buku tabungan bank untuk penggantian rekening payroll, atau kartu keluarga untuk tunjangan). Selain itu, analis SDM seringkali perlu meminta klarifikasi atau dokumen perbaikan tanpa harus menolak tiket secara sepihak.

Fitur-fitur utama yang disediakan dalam task ini:
1. **Layanan Unggah Berkas Lampiran Tiket (`FR-HRS-12`, `VR-HRS-15`)**:
   - Mendukung pengunggahan berkas oleh pegawai pemohon (saat pengajuan awal maupun saat menanggapi permintaan klarifikasi) serta oleh analis SDM.
   - Validasi ketat ukuran berkas: maksimal 5MB per berkas.
   - Validasi tipe MIME dan ekstensi berkas yang aman: `.pdf`, `.jpg`, `.jpeg`, `.png`, `.docx`.
2. **Penyimpanan Berkas Terisolasi & Terstruktur**:
   - Berkas fisik disimpan pada direktori server aman `uploads/hr-services/attachments/` dengan penamaan acak unik berbasis GUID untuk mencegah penimpaan file (*file collision*).
   - Metadata dicatat pada entitas `TrxHrServiceRequestAttachment` (`FileName`, `FilePath`, `ContentType`, `FileSizeBytes`, `DocumentCategory`, `Description`, `UploadedAt`).
3. **Penyelarasan Siklus Hidup Penangguhan SLA (`HRS-W2`)**:
   - Jika tiket sedang berstatus `WaitingStaffResponse` dan staf pemohon mengunggah berkas lampiran yang diminta, status tiket secara otomatis kembali ke `InReview` dan timer SLA di-resume.
4. **Otorisasi Pengunduhan Berkas Lampiran**:
   - Petugas HR berwenang mengunduh seluruh berkas pada antrean.
   - Pegawai biasa hanya berwenang mengunduh berkas pada tiket miliknya sendiri, mencegah kebocoran data sensitif (*unauthorized document access*).

---

## 3. Komponen Teknis yang Dibuat & Dimodifikasi

### A. Model Basis Data Entity Framework Core
File: `Areas/Corporate/HumanResource/HrServiceManagement/Models/TrxHrServiceRequestAttachment.cs`
* Properti: `Id`, `HrServiceRequestId`, `UploadedByUserId`, `UploadedByWorkforceProfileId`, `FileName`, `FilePath`, `ContentType`, `FileSizeBytes`, `DocumentCategory`, `Description`, `UploadedAt`, `IsActive`.

### B. Data Transfer Objects (DTO)
File: `Areas/Corporate/HumanResource/HrServiceManagement/DTOs/HrServiceTicketDtos.cs`
* `TicketAttachmentDto`: Menampilkan metadata berkas lampiran untuk tampilan UI antrean atau portal ESS.

### C. Logika Bisnis & Layanan
File: `Areas/Corporate/HumanResource/HrServiceManagement/Services/HrServiceTicketService.cs`
* Metode: `UploadAttachmentAsync`, `DownloadAttachmentAsync`, `GetAttachmentsAsync`.
* Validasi ukuran berkas maksimal 5MB (`VR-HRS-15`) dan filter ekstensi berkas.

### D. Controllers REST API
* `HrServiceTicketController.cs`: Endpoint antrean analis HR (`POST /{id}/attachments`, `GET /{id}/attachments`, `GET /{id}/attachments/{attachmentId}/download`).
* `HrServiceEssController.cs`: Endpoint portal mandiri pegawai ESS (`POST /{id}/attachments`, `GET /{id}/attachments`, `GET /{id}/attachments/{attachmentId}/download`).

---

## 4. Contoh Skenario Rumah Sakit Konkret
* **Skenario Permintaan Berkas Tambahan Surat Rekomendasi SIP Dokter Spesialis**:
  - Dokter Spesialis Anak dr. Maya mengajukan permohonan "Surat Rekomendasi Perpanjangan SIP".
  - Analis HR memeriksa permohonan dan mendapati bukti kecukupan SKP belum dilampirkan.
  - Analis mengubah status menjadi `WaitingStaffResponse` dengan catatan: "Mohon lampirkan scan sertifikat kecukupan SKP IDI". Timer SLA otomatis ditangguhkan.
  - dr. Maya menerima notifikasi di aplikasi ESS, mengunggah berkas `Sertifikat_SKP_Maya.pdf` (ukuran 1.2 MB).
  - Sistem memvalidasi berkas, menyimpannya di repositori, secara otomatis mengembalikan status tiket ke `InReview`, dan timer SLA dilanjutkan kembali.

---

## 5. Spesifikasi Endpoint REST API

| Method | Path | Deskripsi | Otorisasi |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/corporate/hrd/services/tickets/{id}/attachments` | Unggah lampiran oleh analis HR (maks 5MB) | HR Staff / Admin |
| `GET` | `/api/v1/corporate/hrd/services/tickets/{id}/attachments` | Daftar berkas lampiran pada tiket | HR Staff / Admin |
| `GET` | `/api/v1/corporate/hrd/services/tickets/{id}/attachments/{attachmentId}/download` | Unduh berkas fisik lampiran | HR Staff / Admin |
| `POST` | `/api/v1/self-service/hr/services/tickets/{id}/attachments` | Unggah lampiran oleh staf pemohon ESS | Staf RS / ESS |
| `GET` | `/api/v1/self-service/hr/services/tickets/{id}/attachments` | Daftar lampiran pada tiket staf ESS | Staf RS / ESS |
| `GET` | `/api/v1/self-service/hr/services/tickets/{id}/attachments/{attachmentId}/download` | Unduh berkas fisik lampiran milik staf | Staf RS / ESS |

---

## 6. Verifikasi & Integritas Bukti
* **Kompilasi C#**: Bebas error dan tipe data presisi.
* **Kepatuhan Acceptance Test**: Memenuhi kriteria penerimaan `AT-HRD-HRS-01` dan aturan validasi `VR-HRS-15`.