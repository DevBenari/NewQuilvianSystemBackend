# BE-BKC-069 — Permukaan Operasional Surat yang Menggantung

## Ringkasan untuk Pembaca Umum

Dalam operasional rumah sakit sehari-hari, kasir menerima pembayaran pasien untuk tagihan rawat jalan maupun rawat inap. Ketika pembayaran berhasil dicatat atau dibatalkan, sistem kasir/Billing secara otomatis menerbitkan "surat pemberitahuan" (handoff letter) ke sistem lain:
1. **Surat Penerimaan Uang ke Modul Keuangan (Finance)**: Memberitahukan bahwa ada uang masuk (kas/bank/asuransi) agar dapat dibukukan ke jurnal penerimaan.
2. **Surat Clearance Resep ke Modul Farmasi**: Memberitahukan bahwa obat dalam resep sudah lunas atau dijamin, sehingga apoteker aman untuk meracik dan menyerahkan obat kepada pasien.

Dalam kondisi normal, sistem penerima (Finance atau Farmasi) langsung mengambil dan memproses surat-surat tersebut secara otomatis di latar belakang. Namun, jika terjadi gangguan jaringan, sistem penerima sedang diperbaiki, atau terjadi antrean data yang tersendat, surat tersebut bisa berstatus "menggantung" (pending/unconsumed).

Task ini membangun **permukaan operasional surat menggantung** melalui controller API resmi (`BillingConsumerHandoffsController`):
- **Melihat Daftar Surat Menggantung (`GET /pending`)**: Petugas kasir, supervisor billing, atau staf keuangan dapat melihat daftar surat yang belum diambil oleh modul penerima, lengkap dengan jenis surat, modul tujuan, nomor rujukan (nomor kwitansi/invoice), waktu terbit, dan rincian transaksi. Daftar ini dapat disaring berdasarkan jenis modul (Keuangan atau Farmasi) dan rentang tanggal terbit.
- **Mencatat Pengakuan Penerimaan (`PATCH /{id}/acknowledge`)**: Staf berwenang (Finance Operations atau Admin Sistem) dapat menandai bahwa surat tersebut sudah berhasil diterima dan diproses, misalnya saat dilakukan pemulihan operasional atau rekonsiliasi manual.

### Penjagaan Keamanan dan Integritas Finansial:
- **Anti-Pengakuan Ganda (`BIL-AT-142`)**: Apabila sebuah surat sudah pernah diakui, pengakuan kedua akan langsung ditolak oleh sistem dengan status galat **409 Conflict** tanpa mengubah data apa pun, sehingga tidak terjadi pembukuan ganda.
- **Nol Endpoint Penerbitan Manual (`BKC-DEC-109`)**: Sistem sengaja **tidak menyediakan** tombol atau endpoint untuk menerbitkan surat secara manual. Surat hanya boleh terbit dari peristiwa transaksi uang asli di kasir untuk mencegah pemalsuan surat lunas obat atau surat fiktif penerimaan uang.
- **Nol Endpoint Penghapusan (`BKC-DEC-109`)**: Surat handoff tidak boleh dihapus karena merupakan bukti audit transaksi permanen.
- **Perlindungan Data Sensitif (`BIL-PERMISSION-1.1`)**: Log pencatatan aktivitas pengakuan hanya mencatat identitas surat, pelaku, dan waktu pengakuan. Nomor referensi bank/EDC yang sensitif maupun identitas data medis pasien tidak pernah dicatat ke dalam berkas log.

---

## Spesifikasi Teknis Task

- TASK ID: BE-BKC-069
- TASK TYPE: Fitur (Permukaan operasional controller dan layanan query/acknowledgement untuk surat handoff konsumen)
- COMPLEXITY: MEDIUM
- CLASSIFICATION SCORE: 3 (repo 0 + berkas diperiksa ≤8 → 0 + berkas diubah/dibuat 3 → 1 + endpoint API & otorisasi role → 1 + logika rekonsiliasi & query multi-tabel → 1 + database read-only/update status → 0; total score 3)
- MODEL: Gemini 3.8 Flash (High)
- TASK MODE: BACKEND
- WRITE TARGET: `NewQuilvianSystemBackend` — `Areas/HealthServices/BillingManagement/Billing/**` (source); `docs/module-blueprints/billing-kasir/task/report/backend/**`, `roadmap/**`, `requirement-traceability.md` (laporan task)
- FILES INSPECTED:
  - `docs/module-blueprints/billing-kasir/contracts/api-contract.md` (`BIL-API-1.3`, baris 864–900)
  - `docs/module-blueprints/billing-kasir/contracts/permission-audit-matrix.md` (`BIL-PERMISSION-1.1`, baris 423–473)
  - `docs/module-blueprints/billing-kasir/03-frontend-architecture.md` (`BIL-SCR-41`, baris 1150–1244)
  - `docs/module-blueprints/billing-kasir/02-backend-architecture.md` (`BKC-DES-036`–`041`)
  - `docs/module-blueprints/billing-kasir/testing/acceptance-test-matrix.md` (`BIL-AT-142`)
  - `Areas/HealthServices/BillingManagement/Billing/Models/BilCollectionHandoff.cs`
  - `Areas/HealthServices/BillingManagement/Billing/Models/BilPrescriptionClearanceHandoff.cs`
  - `Areas/HealthServices/BillingManagement/Billing/Services/BilConsumerHandoffService.cs`
- FILES CHANGED:
  - **Diperbarui**: `Areas/HealthServices/BillingManagement/Billing/Dtos/BillingConsumerHandoffDtos.cs` (menambahkan `PendingHandoffQuery`, `PendingHandoffResponse`, `AcknowledgeHandoffRequest`, `HandoffResponse`, konstanta `BillingHandoffTypes` dan `BillingHandoffTargetModules`)
  - **Diperbarui**: `Areas/HealthServices/BillingManagement/Billing/Services/BilConsumerHandoffService.cs` (mengimplementasikan `GetPendingHandoffsAsync`, `AcknowledgeHandoffAsync`, serta exception `BillingConsumerHandoffConflictException`)
  - **Dibuat**: `Areas/HealthServices/BillingManagement/Billing/Controllers/BillingConsumerHandoffsController.cs` (controller operasional dengan endpoint `GET /pending` dan `PATCH /{id:guid}/acknowledge`)
- IMPLEMENTATION:
  1. **DTO Kontrak Permukaan Operasional**:
     - `PendingHandoffQuery`: menampung parameter `HandoffType`, `FromDate`, `ToDate`, `PageNumber`, dan `PageSize`.
     - `PendingHandoffResponse`: menampung atribut tampilan `BIL-SCR-41` (`Id`, `HandoffType`, `TargetModule`, `CreatedAt`, `ReferenceNumber`, `InvoiceId`, `InvoiceNumber`, `Status`, `Details`).
     - `AcknowledgeHandoffRequest`: menampung `HandoffType` (opsional) dan `Notes`.
     - `HandoffResponse`: menampung status mutakhir setelah pengakuan (`Id`, `HandoffType`, `TargetModule`, `Status`, `AcknowledgedAt`, `Message`).
  2. **Layanan Pengambilan Daftar Surat Menggantung (`GetPendingHandoffsAsync`)**:
     - Mengambil data berstatus `CREATED` (`BillingHandoffStatuses.Created`) secara `AsNoTracking()` dari tabel `BilCollectionHandoffs` (modul Finance) dan `BilPrescriptionClearanceHandoffs` (modul Farmasi).
     - Memvalidasi rentang waktu: bila `FromDate > ToDate`, melempar `BillingConsumerHandoffValidationException` (HTTP 400).
     - Menggabungkan dan mengurutkan data berdasarkan waktu terbit terbaru secara menurun (`OrderByDescending(x => x.CreatedAt)`).
     - Membungkus hasil dalam `PagedResult<PendingHandoffResponse>` lengkap dengan `TotalData` dan `TotalPage`.
  3. **Layanan Pengakuan Penerimaan Surat (`AcknowledgeHandoffAsync`)**:
     - Memeriksa keberadaan surat di tabel `BilCollectionHandoffs` atau `BilPrescriptionClearanceHandoffs`. Jika tidak ditemukan, melempar `KeyNotFoundException` (HTTP 404).
     - **Penegakan `BIL-AT-142` (Anti-Ganda)**: Jika surat sudah berstatus `ACKNOWLEDGED`, sistem melempar `BillingConsumerHandoffConflictException` (HTTP 409 Conflict) tanpa mengubah baris data apa pun.
     - Jika surat berstatus `CREATED`: memperbarui `Status` menjadi `ACKNOWLEDGED`, mengisi `AcknowledgedAt = DateTimeOffset.UtcNow`, memperbarui `UpdateBy` dengan ID pengguna login, dan memperbarui `RowVersion = Guid.NewGuid()`.
     - **Penegakan Audit `BIL-PERMISSION-1.1`**: Mencatat audit log via `_loggerService.AuditAsync` dengan kategori `HealthServices.BillingManagement.Billing` dan aksi `BillingConsumerHandoff.Acknowledged`. Payload audit **hanya** memuat `HandoffId`, `HandoffType`, `TargetModule`, `ActorUserId`, dan `AcknowledgedAt`. Kolom sensitif penyedia pembayaran (`ProviderReference`, `ProviderEventId`) maupun identitas medis/pasien tidak pernah dicatat ke log.
  4. **Controller Operasional (`BillingConsumerHandoffsController`)**:
     - Route dasar: `api/v1/health-services/billing-management/consumer-handoffs`.
     - Tag Swagger: `[Tags("Health Services / Billing Management / Consumer Handoffs")]`.
     - Otorisasi Controller: `[AccessController("HEALTH_SERVICE_BILLING_MANAGEMENT_CONSUMER_HANDOFF", ... AreaName = "HealthServices", ControllerName = "BillingConsumerHandoff", ...)]`.
     - Endpoint 1: `GET /pending` dengan `[AccessAction("Read", ...)]` dan `[AccessPermission("BillingConsumerHandoff", "Read")]`.
     - Endpoint 2: `PATCH /{id:guid}/acknowledge` dengan `[AccessAction("Acknowledge", ...)]` dan `[AccessPermission("BillingConsumerHandoff", "Acknowledge")]`.
     - **Boundary Enforcement (`BKC-DEC-109`)**: Nol endpoint penerbitan (`POST /`) dan nol endpoint penghapusan (`DELETE /{id}`).
- **Backend Governance Preflight**:
  - Area: `HealthServices` · Module: `BillingManagement/Billing` · Prefix: `Bil` · Registry: `ACTIVE`
  - Keberlakuan: `NEW CODE` untuk `BillingConsumerHandoffsController.cs`; `TOUCHED LEGACY` untuk `BillingConsumerHandoffDtos.cs` dan `BilConsumerHandoffService.cs`.
  - QBE Compliance: Mematuhi `QBE-API-001`, `QBE-API-002`, `QBE-SEC-001`, `QBE-SEC-002`, `QBE-LOG-001`, `QBE-LOG-002`, `QBE-NAM-001`, dan `QBE-NAM-002`.
- BLUEPRINT STATUS/EVIDENCE: NOT APPLICABLE (`BACKEND MODE`)
- API CONTRACT IMPACT: Dua endpoint operasional baru pada route `api/v1/health-services/billing-management/consumer-handoffs` sesuai `BIL-API-1.3`. Nol perubahan pada endpoint yang sudah ada.
- DATABASE IMPACT: Nol penambahan tabel/kolom baru. Operasi hanya melakukan pembacaan `AsNoTracking` dan pembaruan kolom status/audit pada tabel yang sudah didefinisikan sebelumnya (`BilCollectionHandoff` dan `BilPrescriptionClearanceHandoff`).
- SECURITY & ACCESS CONTROL IMPACT:
  - Hak akses ditegakkan menggunakan filter atribut standar `[AccessPermission("BillingConsumerHandoff", "Read")]` dan `[AccessPermission("BillingConsumerHandoff", "Acknowledge")]`.
  - Tanpa hardcoded role: konfigurasi peran dikontrol penuh lewat modul otorisasi peran sistem Quilvian.
- VISUAL REFERENCE: NOT REQUIRED
- VALIDATION:
  | Command/Check | Result | Classification | Evidence/Note |
  | --- | --- | --- | --- |
  | `Invoke-QbeConformanceCheck.ps1` (3 berkas) | `PASS` | VERIFIED | `Files evaluated: 3`, `VIOLATION: 0`, `REVIEW: 0`, `INFO: 0`, `Findings: none` |
  | `dotnet build` | **Menunggu eksekusi manual pengguna** | NOT VERIFIED | Sesuai preferensi pengguna untuk kompilasi mandiri |
  | Review Diff dan Scope | Selesai | VERIFIED | Tepat 2 endpoint operasional sesuai `BIL-API-1.3`; nol endpoint penerbitan; nol endpoint penghapusan |
  | `BIL-AT-142` (pengakuan ganda ditolak 409 Conflict) | Terpenuhi pada source | VERIFIED (Logic) | `AcknowledgeHandoffAsync` memeriksa `Status == ACKNOWLEDGED` dan melempar `BillingConsumerHandoffConflictException` |
  | `BIL-PERMISSION-1.1` (audit log tanpa field sensitif) | Terpenuhi pada source | VERIFIED (Logic) | `AuditAsync` hanya mencatat `HandoffId`, `HandoffType`, `TargetModule`, `ActorUserId`, `AcknowledgedAt` |
  | Validasi rentang tanggal | Terpenuhi pada source | VERIFIED (Logic) | `FromDate > ToDate` menghasilkan HTTP 400 Bad Request dengan pesan yang jelas |
- WARNINGS: Tidak ada.
- KNOWN ISSUES: Tidak ada issue baru pada source yang ditulis.
- NEXT TASKS: `BE-BKC-070` (Pekerjaan pemulihan resep yang terlanjur tertahan sebelum jalur handoff berdiri, `BKC-DEC-111`).
