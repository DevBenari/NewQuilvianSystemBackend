# Kamus Data (Data Dictionary) — Integrasi Rawat Inap ↔ Kasir / Billing (`INP-S22`)

| Field | Nilai |
|---|---|
| Blueprint ID | `RWI-BP-001-INT-BIL` |
| Sub-modul | `integrasi-billing` (Slice `INP-S22`) |
| Versi Dokumen | `1.0.0` (Draft) — 17 September 2026 |
| Target Database | PostgreSQL / Microsoft SQL Server |
| ORM Framework | Entity Framework Core 8 |

---

## 1. Konvensi Kolom Standar Entity Framework (Base Entity)

Seluruh entitas yang diturunkan dari `IdentityModel` atau `BaseAuditableEntity` pada sistem Quilvian mewarisi 10 kolom standar berikut. Kolom-kolom ini **tidak diulang** di setiap tabel di bawah ini:

| Nama Kolom | Tipe Data | Nullable | Sensitif | Keterangan |
|---|---|:---:|:---:|---|
| `Id` | `uuid` | Tidak | Tidak | Primary Key unik entitas (Clustered Index). |
| `CreatedBy` | `varchar(100)` | Tidak | Tidak | Nama akun pengguna yang membuat data pertama kali. |
| `CreatedAt` | `timestamp with time zone` | Tidak | Tidak | Waktu pembuatan data pertama kali (UTC). |
| `LastModifiedBy` | `varchar(100)` | Ya | Tidak | Nama akun pengguna yang terakhir mengubah data. |
| `LastModifiedAt` | `timestamp with time zone` | Ya | Tidak | Waktu perubahan terakhir data (UTC). |
| `IsDeleted` | `boolean` | Tidak | Tidak | Penanda soft-delete (default: `false`). |
| `DeletedBy` | `varchar(100)` | Ya | Tidak | Nama akun pengguna yang melakukan soft-delete. |
| `DeletedAt` | `timestamp with time zone` | Ya | Tidak | Waktu soft-delete data (UTC). |
| `RowVersion` | `bytea` / `rowversion` | Ya | Tidak | Penanda kontrol konkurensi optimistik (*Optimistic Concurrency*). |

> **Penanda Kolom Sensitif:** Kolom yang ditandai **Sensitif = Ya** tidak boleh dicatat dalam plain-text log, custom audit logger, atau dijadikan payload contoh publik untuk mencegah kebocoran data privasi medis atau kredensial.

---

## 2. Tabel Baru: `InpIntegrationOutboxes`

- **Status:** `Baru`
- **Modul Pemilik:** `InPatientManagement` (Rawat Inap)
- **Tujuan Bisnis:** Menampung seluruh pesan integrasi asinkronus ke modul Billing secara transaksional sebelum dikirimkan oleh background worker, menjamin prinsip *at-least-once delivery* dan *idempotency*.

| Nama Kolom | Tipe Data | Nullable | Sensitif | Index & Constraint | Deskripsi & Aturan Bisnis |
|---|---|:---:|:---:|---|---|
| `IdempotencyKey` | `varchar(255)` | Tidak | Tidak | **Unique Index:** `UQ_InpIntegrationOutbox_IdempotencyKey` | Kunci keunikan gabungan pesan. Format baku: `SourceDomain:SourceType:SourceDetailId:Version`. Mencegah duplikasi charge di kasir. |
| `SourceDomain` | `varchar(50)` | Tidak | Tidak | Index biasa | Domain pembuat event, e.g. `'INPATIENT'`. |
| `SourceType` | `varchar(50)` | Tidak | Tidak | Index biasa | Tipe entitas pelayanan: `'ADMISSION'`, `'ROOM_STAY'`, `'DISCHARGE'`. |
| `SourceDetailId` | `varchar(100)` | Tidak | Tidak | Index biasa | ID unik baris entitas pelayanan terkait (e.g. `AdmissionId` atau `PlacementId`). |
| `EventType` | `varchar(100)` | Tidak | Tidak | Index biasa | Jenis event bisnis: `ADMISSION_CONFIRMED`, `BED_OCCUPIED`, `OCCUPANCY_CORRECTED`, `BED_RELEASED`. |
| `PayloadJson` | `text` / `jsonb` | Tidak | **Ya** | — | Konten data payload event terstruktur dalam format JSON. |
| `Status` | `integer` | Tidak | Tidak | **Filtered Index:** `IX_InpOutbox_Status_Pending` (`WHERE Status IN (0, 3)`) | Nilai enum: `0` = Pending, `1` = Processing, `2` = Published, `3` = Failed, `4` = DeadLetter. |
| `RetryCount` | `integer` | Tidak | Tidak | — | Jumlah percobaan pengiriman yang sudah dilakukan (default: `0`). |
| `NextRetryAtUtc` | `timestamp with time zone` | Ya | Tidak | Index biasa | Jadwal percobaan pengiriman berikutnya dihitung via formula exponential backoff. |
| `PublishedAtUtc` | `timestamp with time zone` | Ya | Tidak | — | Stempel waktu saat pesan berhasil dikonfirmasi (*ACK*) oleh penerima. |
| `LastError` | `text` | Ya | Tidak | — | Pesan galat teknis terakhir bila pengiriman gagal. |
| `CreatedAtUtc` | `timestamp with time zone` | Tidak | Tidak | Index biasa | Waktu pencatatan pesan ke antrean outbox (UTC). |

---

## 3. Tabel Diperbarui: `InpBedPlacements`

- **Status:** `Diperbarui` (penambahan kolom penunjang standardisasi occupancy)
- **Modul Pemilik:** `InPatientManagement` (Rawat Inap)
- **Tujuan Bisnis:** Mencatat riwayat hunian tempat tidur pasien secara presisi, membedakan jam penempatan awal dan jam kepergian fisik.

| Nama Kolom | Tipe Data | Nullable | Sensitif | Index & Constraint | Deskripsi & Aturan Bisnis |
|---|---|:---:|:---:|---|---|
| `PhysicallyLeftAt` | `timestamp with time zone` | Ya | Tidak | — | Waktu pasien meninggalkan ruangan kamar tidur secara fisik nyata. Menjadi dasar pematokan `OccupancyEndAt`. |
| `Version` | `integer` | Tidak | Tidak | — | Nomor versi hunian tempat tidur (default: `1`). Naik setiap terjadi koreksi data kamar. |
| `ChangeReason` | `text` | Ya | Tidak | — | Alasan mutasi kamar atau koreksi admisi. Wajib diisi jika terjadi koreksi data. |
| `IsSuperseded` | `boolean` | Tidak | Tidak | Index biasa | Bernilai `true` jika baris ini telah digantikan oleh versi koreksi yang lebih baru (tidak pernah dihapus fisik). |
| `SupersededAtUtc` | `timestamp with time zone` | Ya | Tidak | — | Stempel waktu saat baris data ini digantikan oleh versi baru. |

---

## 4. Tabel Diperbarui: `InpDischargeRequests`

- **Status:** `Diperbarui` (penambahan kolom pelacakan clearance & override)
- **Modul Pemilik:** `InPatientManagement` (Rawat Inap)
- **Tujuan Bisnis:** Mengelola status izin pulang medis dokter DPJP, persetujuan kasir, dan wewenang pelepasan fisik pasien.

| Nama Kolom | Tipe Data | Nullable | Sensitif | Index & Constraint | Deskripsi & Aturan Bisnis |
|---|---|:---:|:---:|---|---|
| `ClearanceStatus` | `integer` | Tidak | Tidak | Index biasa | Nilai enum: `0` = None, `1` = Pending, `2` = Cleared, `3` = Revoked, `4` = Overridden. |
| `ClearanceRevokedReason` | `text` | Ya | Tidak | — | Alasan kasir mencabut persetujuan clearance (misal tagihan susulan). |
| `IsSupervisorOverridden` | `boolean` | Tidak | Tidak | Index biasa | Bernilai `true` jika pemulangan pasien disahkan melalui jalur *Supervisor Override*. |
| `SupervisorOverrideReason` | `text` | Ya | Tidak | — | Alasan wajib kedaruratan klinis / evakuasi ambulans rujukan saat melakukan override. |
| `SupervisorOverriddenByUserId`| `uuid` | Ya | Tidak | FK ke User | ID akun supervisor bangsal yang mengeksekusi override darurat. |
| `SupervisorOverriddenAtUtc` | `timestamp with time zone` | Ya | Tidak | — | Stempel waktu presisi pelaksanaan supervisor override (UTC). |

---

## 5. Tabel Referensi: Modul `BillingManagement` (Hanya Dibaca)

- **Status:** `Sudah ada` (milik modul tetangga)
- **Modul Pemilik:** `BillingManagement`
- **Catatan:** Modul Rawat Inap **TIDAK MEMILIKI** hak tulis atau wewenang skema atas tabel ini. Hanya dibaca melalui kontrak DTO REST.

| Nama Tabel / Entitas | Modul Asal | Kolom Kunci yang Dirujuk | Keterangan Pemakaian |
|---|---|---|---|
| `BillingFolio` / `BilInvoice` | `BillingManagement` | `FolioId`, `EncounterId`, `FolioStatus` (`OPEN`, `CLOSED`) | Memvalidasi apakah tagihan pasien masih berstatus `OPEN` sebelum mengizinkan mutasi kamar. |
| `BilFinancialClearance` | `BillingManagement` | `ClearanceId`, `EncounterId`, `Status`, `BlockerReasons` | Membaca status persetujuan kasir dan daftar kendala tanpa membaca nominal uang. |
