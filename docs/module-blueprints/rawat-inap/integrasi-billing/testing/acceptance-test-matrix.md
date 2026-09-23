# Matriks Pengujian Penerimaan (Acceptance Test Matrix) — Integrasi Rawat Inap ↔ Billing

| Field | Nilai |
|---|---|
| Blueprint ID | `RWI-BP-001-INT-BIL` |
| Sub-modul | `integrasi-billing` (Slice `INP-S22`) |
| `contract_version` | **`1.0.0`** |
| Status | **`draft`** |

---

## 1. Pemetaan Kriteria Penerimaan Kanonik (`RWI-AC-236` s.d. `241`)

| ID Kriteria | Deskripsi Kriteria Penerimaan | Jenis Pengujian | Target Class / File Test | Status Rencana |
|---|---|:---:|---|:---:|
| **`RWI-AC-236`** | Sinkronisasi admisi `Admitted` memicu pembuatan folio billing `OPEN`; room charge baru aktif saat `Bed Occupied` fisik. | Integration Test | `InpatientAdmissionBillingIntegrationTests.cs` | Rencana |
| **`RWI-AC-237`** | Koreksi kamar hanya saat billing `OPEN`, dilakukan oleh Supervisor, beralasan wajib, immutable versioning, picu `OCCUPANCY_CORRECTED`. | Unit & Integration Test | `InpatientBedTransferValidationTests.cs` | Rencana |
| **`RWI-AC-238`** | Clearance kasir mengontrol pelepasan fisik; penolakan pelepasan saat pending; *Auto-Reblock* saat revoked; *Supervisor Override* beralasan wajib. | Integration Test | `InpatientDischargeClearanceGateTests.cs` | Rencana |
| **`RWI-AC-239`** | `OccupancyEndAt` identik dengan `PhysicallyLeftAt`; finalisasi tagihan kamar dipatok dari jam kepergian fisik. | Unit Test | `InpatientPhysicalDischargeTimeTests.cs` | Rencana |
| **`RWI-AC-240`** | Antarmuka bangsal bebas nominal rupiah; hanya status operasional dan blocker string; `InpatientBilling:View` diperlukan untuk rupiah. | UI Component & API Test | `InpatientBillingPrivacyAuthorizationTests.cs` | Rencana |
| **`RWI-AC-241`** | Outbox transaksional `InpIntegrationOutbox`, compound key `IdempotencyKey` unik, retry exponential backoff. | Worker & DB Test | `InpatientIntegrationOutboxResilienceTests.cs` | Rencana |

---

## 2. Rincian Kasus Uji Otomatis (Automated Test Cases)

### 2.1 Pengujian Ketahanan Outbox & Idempotensi (`TC-OUTBOX-01` s.d. `03`)
- **`TC-OUTBOX-01` (Transactional Consistency):**
  - *Skenario:* Menyimpan penempatan tempat tidur baru.
  - *Verifikasi:* Verifikasi bahwa baris `InpBedPlacement` dan pesan `InpIntegrationOutbox` tersimpan di database dalam transaksi yang sama. Bila `SaveChanges` digagalkan sengaja, kedua baris tidak tersimpan (rollback).
- **`TC-OUTBOX-02` (Duplicate Key Rejection):**
  - *Skenario:* Mencoba memasukkan dua pesan outbox dengan `IdempotencyKey` yang sama persis (`INPATIENT:ROOM_STAY:OCC-08891:1`).
  - *Verifikasi:* Basis data melempar `DbUpdateException` karena pelanggaran unique constraint `UQ_InpIntegrationOutbox_IdempotencyKey`.
- **`TC-OUTBOX-03` (Worker Exponential Backoff):**
  - *Skenario:* Mock endpoint Billing merespons `503 Service Unavailable`.
  - *Verifikasi:* Pesan outbox ditandai `Failed`, `RetryCount` bertambah menjadi 1, dan `NextRetryAtUtc` dijadwalkan 10 detik ke depan.

### 2.2 Pengujian Gerbang Pemulangan & Auto-Reblock (`TC-GATE-01` s.d. `04`)
- **`TC-GATE-01` (Discharge Gate Rejection saat Pending):**
  - *Skenario:* Perawat menekan tombol `Konfirmasi Pasien Pulang Fisik` saat clearance masih `Pending`.
  - *Verifikasi:* Endpoint merespons `422 Unprocessable Entity` dengan pesan kesalahan `VAL-INT-001`.
- **`TC-GATE-02` (Auto-Reblock saat Webhook Revoked Diterima):**
  - *Skenario:* Pasien berstatus `Cleared`. Webhook kasir `CLEARANCE_REVOKED` diterima.
  - *Verifikasi:* Status clearance seketika berubah menjadi `Revoked`, tombol pemulangan fisik menjadi `disabled`.
- **`TC-GATE-03` (Supervisor Override Success):**
  - *Skenario:* Pasien berstatus `Revoked`. Supervisor memasukkan alasan darurat (>= 20 karakter) dan PIN yang valid.
  - *Verifikasi:* Respons `200 OK`, status berubah menjadi `Overridden`, tombol pemulangan fisik aktif kembali, dan audit log darurat tersimpan.
- **`TC-GATE-04` (Supervisor Override Rejection):**
  - *Skenario:* Supervisor memasukkan alasan kurang dari 20 karakter (misal "Darurat").
  - *Verifikasi:* Endpoint merespons `400 Bad Request` dengan pesan kesalahan `VAL-INT-004`.

### 2.3 Pengujian Privasi Tampilan Bangsal (`TC-PRIVACY-01` s.d. `02`)
- **`TC-PRIVACY-01` (Perawat Non-Finansial):**
  - *Skenario:* Pengguna dengan token perawat memanggil `GET /episodes/{id}/billing-status`.
  - *Verifikasi:* Respons memuat `operationalStatusText` dan `blockerReasons`, namun field `totalCharges`, `depositBalance`, dan `outstanding` bernilai `null` / tidak disertakan dalam JSON.
- **`TC-PRIVACY-02` (Akses Rincian Rupiah Ditolak):**
  - *Skenario:* Pengguna dengan token perawat mencoba memanggil `GET /episodes/{id}/billing-details`.
  - *Verifikasi:* Endpoint merespons `403 Forbidden` (`VAL-INT-006`).

---

## 3. Matriks Kriteria Kelulusan Pengujian (Exit Criteria)

- [ ] Seluruh Unit Test (cakupan > 85% untuk logika outbox, validasi mutasi, dan clearance gate) lulus 100%.
- [ ] Seluruh Integration Test database (EF Core InMemory / SQLite / Testcontainers) lulus 100%.
- [ ] Pengujian UAT end-to-end (`UAT-INT-001` s.d. `UAT-INT-013`) dieksekusi bersama tim modul Billing dengan hasil lolos tanpa deviasi.
