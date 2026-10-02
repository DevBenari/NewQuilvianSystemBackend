# Matriks Wewenang & Audit (Permission & Audit Matrix) — Integrasi Rawat Inap ↔ Billing

| Field | Nilai |
|---|---|
| Blueprint ID | `RWI-BP-001-INT-BIL` |
| Sub-modul | `integrasi-billing` (Slice `INP-S22`) |
| `contract_version` | **`1.0.0`** |
| Status | **`draft`** |

---

## 1. Daftar String Hak Akses Baku (Permission Strings)

Sistem menggunakan atribut `[AccessPermission("Resource", "Action")]` terstandarisasi:

| String Permission Lengkap | Resource | Action | Deskripsi Wewenang |
|---|---|---|---|
| `[AccessPermission("InpatientEpisode", "Read")]` | `InpatientEpisode` | `Read` | Membaca data umum episode dan sensus kamar bangsal. |
| `[AccessPermission("InpatientNurse", "Read")]` | `InpatientNurse` | `Read` | Membaca status operasional tagihan kasir tanpa angka rupiah. |
| `[AccessPermission("InpatientNurse", "Write")]` | `InpatientNurse` | `Write` | Mengonfirmasi penempatan tempat tidur fisik dan pelepasan fisik pasien pulang. |
| `[AccessPermission("InpatientSupervisor", "Override")]` | `InpatientSupervisor` | `Override` | Mengeksekusi mutasi kamar beralasan dan supervisor override pemulangan darurat klinis. |
| `[AccessPermission("InpatientBilling", "View")]` | `InpatientBilling` | `View` | Membaca rincian nominal rupiah akumulasi biaya tagihan rawat inap. |
| `[AccessPermission("BillingStaff", "Write")]` | `BillingStaff` | `Write` | Mengelola pembayaran kasir, menyetujui clearance, atau mencabut clearance. |

---

## 2. Pemetaan Hak Akses Berdasarkan Peran Rumah Sakit

| Peran Rumah Sakit | `InpatientEpisode:Read` | `InpatientNurse:Read` | `InpatientNurse:Write` | `InpatientSupervisor:Override` | `InpatientBilling:View` | `BillingStaff:Write` |
|---|:---:|:---:|:---:|:---:|:---:|:---:|
| **Petugas Admisi Rawat Inap** | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| **Perawat Pelaksana Bangsal** | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ |
| **Kepala Ruangan Bangsal** | ✅ | ✅ | ✅ | ❌ | ❌ | ❌ |
| **Supervisor Rawat Inap / Duty Manager** | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| **Petugas Kasir / Billing Rawat Inap** | ❌ | ❌ | ❌ | ❌ | ✅ | ✅ |
| **Manajer Keuangan / Finance Auditor** | ✅ | ✅ | ❌ | ❌ | ✅ | ❌ |

> **Catatan Privasi Klinis:** Perawat Pelaksana dan Kepala Ruangan secara tegas **TIDAK MEMILIKI** hak akses `InpatientBilling:View`. Hal ini menjamin perawat di ruangan pasien tidak dibebani sengketa nominal uang pasien ([`RWI-DEC-160`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/00-interview-decisions.md)).

---

## 3. Matriks Jejak Rekam Audit (Audit Trail Matrix)

Seluruh tindakan yang mempengaruhi status keuangan, durasi sewa kamar, atau pemulangan wajib dicatat ke dalam log audit *immutable* (tidak dapat diedit atau dihapus).

| Kejadian yang Diaudit (Action Type) | Data Sebelum (Old Value) | Data Sesudah (New Value) | Kolom Wajib Tersimpan | Tingkat Kritis Audit |
|---|---|---|---|:---:|
| **Koreksi / Mutasi Kamar Pasien** | Nomor kamar lama, kelas lama, jam mulai lama | Nomor kamar baru, kelas baru, jam mulai baru | `UserId`, `TimestampUtc`, `IpAddress`, `OldValueJson`, `NewValueJson`, `Reason` | **Tinggi (Finansial)** |
| **Persetujuan Clearance Kasir** | `ClearanceStatus = Pending` | `ClearanceStatus = Cleared` | `UserId`, `TimestampUtc`, `ReceiptNumber`, `ClearedAmount` | **Tinggi (Finansial)** |
| **Pencabutan Clearance Kasir (*Revoke*)** | `ClearanceStatus = Cleared` | `ClearanceStatus = Revoked` | `UserId`, `TimestampUtc`, `RevokeReason`, `LateChargeDetails` | **Kritis (Audit RS)** |
| **Eksekusi *Supervisor Override*** | `ClearanceStatus = Revoked/Pending` | `ClearanceStatus = Overridden` | `SupervisorUserId`, `TimestampUtc`, `IpAddress`, `EmergencyReason`, `PinVerified = true` | **Sangat Kritis (Hukum & Medis)** |
| **Konfirmasi Kepulangan Fisik Pasien** | `BedOccupied = true`, `PhysicallyLeftAt = null` | `BedOccupied = false`, `PhysicallyLeftAt = DateTime` | `NurseUserId`, `TimestampUtc`, `PhysicallyLeftAt`, `BedCode` | **Tinggi (Operasional)** |

---

## 4. Kebijakan Retensi Log Audit
- Log audit mutasi kamar dan supervisor override disimpan secara permanen (**retensi minimum 10 tahun** sesuai regulasi rekam medis dan keuangan rumah sakit Indonesia).
- Log outbox yang telah berstatus `Published` disimpan selama **30 hari** sebelum dipindahkan ke tabel arsip historis.

---

## 5. Perubahan pada `contract_version` `1.1.0` — Finishing Rawat Inap ★ 1 Oktober 2026

| Field | Nilai |
|---|---|
| `last_changed_in` | `1.1.0` |
| Status | **`draft`** |
| Traceability | `FR-RWF-001`, `005`, `017`, `019`, `022`, `024`; `RWI-DEC-170`, `187`, `192` (f); `PR-RWF-07` |

**Bagian 1 s.d. 3 di atas tidak berlaku lagi.** Keenam string pada bagian 1 (`InpatientNurse:Read`, `InpatientNurse:Write`, `InpatientSupervisor:Override`, `InpatientBilling:View`, `BillingStaff:Write`) tidak pernah ada di source dan tidak dipakai. Pemetaan endpoint ke hak akses hanya hidup di kolom `Hak akses` pada `api-contract.md` bagian 3; berkas ini tidak mengulangnya.

### 5.1 Cara kerja hak akses di repository ini

| Unsur | Rujukan source | Arti bagi desain |
|---|---|---|
| `[AccessPermission("Resource", "Action")]` pada setiap endpoint | `Attributes/AccessPermissionAttribute.cs`, ditegakkan `Filters/AccessPermissionFilter.cs` lewat `AccessPermissionService.HasAccessAsync` | Satu-satunya penjaga; nama peran tidak pernah dibaca |
| Registry permission | `Services/Security/PermissionRegistryDescriptor.cs` | Baris registry **hanya** lahir dari atribut endpoint. Karena itu hak lihat rupiah dibuat sebagai endpoint sendiri (`…/amounts`), bukan pemeriksaan di dalam service |
| `[AccessAction]` | `Attributes/AccessActionAttribute.cs` | Metadata tampilan pada layar Akses Role |
| Endpoint tanpa penjaga | `KnownUnenforcedBusinessEndpoints` kosong | Webhook `[AllowAnonymous]` yang dihapus adalah satu-satunya penyimpangan di area ini |

### 5.2 Permission baru, diubah, dan dicabut

| Resource : Action | Status | Dipakai untuk |
|---|---|---|
| `PatientBillingSummary : ViewAmount` | **Baru** | Endpoint `…/amounts` ringkasan dan rincian tagihan bangsal |
| `PatientBillingSummary : Read` | Sudah ada | Ringkasan dan rincian tanpa rupiah |
| `InpatientBedOccupancy : Correct` | **Baru** | Koreksi salah catat penempatan |
| `InpatientIntegrationOutbox : Read` | **Baru** | Pemantauan outbox |
| `InpatientIntegrationOutbox : Replay` | **Baru** | Putar ulang saat rilis |
| `InpatientDischarge : RecordDeparture`, `: Close`, `: CloseOverride`, `: Read`, `: ReadFinancialClearance` | Sudah ada | Perilaku endpoint berubah, string tetap |
| `InpatientBillingOperational : Read` | Sudah ada | `billing-status` |
| `InpatientMonitoring : Read` | Sudah ada | Daftar "pulang sebelum izin kasir" |
| `BillingInvoice : Read`, `: Update` | Sudah ada | Antrean "perlu diperiksa" dan penyelesaiannya |
| `InpatientDischargeClearance : SupervisorOverride`, `: ConfirmPhysicalDischarge` | **Dicabut** | Endpoint-nya dihapus. Baris registry lama dinonaktifkan oleh seeder |
| `InpatientBillingOperational : ViewBillingDetails` | **Dicabut** | Endpoint `billing-details` dihapus |
| `InpatientDischarge : MarkFinancialClearance` | **Dicabut** | Endpoint tulis tanda manual dihapus |
| `BillingInpatient : Create` | **Dicabut** untuk `occupancy-charges` | Endpoint dihapus. Bila Resource/Action ini tidak dipakai endpoint lain, barisnya ikut nonaktif |

**Penyelarasan tiga nama hak lihat rupiah (`RWI-DEC-170` butir 6).** `PatientBillingSummary : Read` (`RWI-DEC-154`) tetap menjadi hak baca rincian. `InpatientBilling:View` (`RWI-DEC-160`) dan `InpatientBillingOperational : ViewBillingDetails` (PRD) **digantikan** `PatientBillingSummary : ViewAmount`.

### 5.3 Peta peran ke butir hak akses

Pemberian nyata dilakukan Admin Akses Role (`FIN-UNK-05`). Tabel ini adalah peta yang dianjurkan desain, bukan data seed.

| Peran rumah sakit | `PatientBillingSummary : Read` | `PatientBillingSummary : ViewAmount` | `InpatientDischarge : RecordDeparture` | `InpatientDischarge : Close` | `InpatientDischarge : CloseOverride` | `InpatientBedOccupancy : Correct` | `InpatientMonitoring : Read` | `InpatientIntegrationOutbox : Read/Replay` | `BillingInpatient : Read` |
|---|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| Perawat pelaksana | ✅ | — | ✅ | — | — | — | ✅ | — | — |
| Kepala ruangan | ✅ | — | ✅ | — | — | ✅ | ✅ | — | — |
| Petugas admisi | ✅ | ✅ | ✅ | ✅ | — | ✅ | ✅ | — | — |
| Supervisor rawat inap | ✅ | ✅ | ✅ | ✅ | ✅ | — | ✅ | — | — |
| Kasir | — | — | — | — | — | — | ✅ | — | ✅ |
| Tim TI pelaksana rilis | — | — | — | — | — | — | — | ✅ (`Replay` hanya dengan wewenang tertulis) | — |

### 5.4 Kewenangan yang tidak dapat dijaga mesin hak akses

| Kewenangan | Penjaga | Yang **tidak** dijaganya | Risiko |
|---|---|---|---|
| Pencatat keluar ruangan sudah melihat peringatan kasir | Kode `409 INP-DEP-001` memaksa pengakuan eksplisit | Bahwa keluarga benar-benar sudah ke kasir | Piutang; dimitigasi daftar "pulang sebelum izin kasir" (`RSK-RWF-07`) |
| Override penutupan oleh orang yang benar | Permission dan akun login | Komputer supervisor yang ditinggal login | Diterima pemilik (`RWI-DEC-187`); mitigasi kebijakan kunci layar dan laporan harian |
| Koreksi penempatan hanya untuk salah catat, bukan pengganti transfer | Alasan wajib dan versi lama tersimpan | Niat pengguna | Laporan transfer menandai koreksi berbeda dari transfer |

### 5.5 Audit

| Kejadian | Lapisan | Jejak tahan lama |
|---|---|---|
| Keluar ruangan | `LoggerService` (bukan `GET`) | `InpEpisode.PhysicallyLeftAt`, `PhysicallyLeftByUserId`, tiga kolom pengamatan kasir |
| Penutupan normal dan override | `LoggerService` | `InpStatusHistory` (sudah ada), `ClosedWithoutClearanceReason`, `ClosureClearanceObserved` |
| Koreksi penempatan | `LoggerService` | Baris lama dan baru beserta `ChangeReason` |
| Putar ulang, termasuk `DryRun` | `LoggerService` — **pengecualian bernama**: dicatat walaupun hanya simulasi | `ReplayBatchId` pada setiap baris outbox |
| Penerimaan event Billing | Log aplikasi | `BilInpatientEventReceipt` |
| Penyelesaian pemeriksaan invoice | `LoggerService` | Kolom `ReviewResolved*` pada `BilInvoice` |

Payload log hanya `EntityId`, controller, action, dan status. Payload log **tidak** memuat alasan override, rupiah, atau data klinis.

### 5.6 Kolom sensitif

Kolom bertanda sensitif pada `data/data-dictionary.md` bagian 6: `InpEpisode.ClosedWithoutClearanceReason`, `InpEpisode.Notes`, `InpEpisode.IsolationNote`, `InpBedPlacement.ChangeReason`, `BilInvoice.ReviewResolutionNote`. Kolom itu tidak masuk custom logger dan tidak dipakai sebagai contoh data asli.
