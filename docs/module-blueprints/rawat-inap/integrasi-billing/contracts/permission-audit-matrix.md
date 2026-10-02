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
