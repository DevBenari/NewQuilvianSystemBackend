# Matriks Pengujian Penerimaan (Acceptance Test Matrix) — Integrasi Rawat Inap ↔ Billing

| Field | Nilai |
|---|---|
| Blueprint ID | `RWI-BP-001-INT-BIL` |
| Sub-modul | `integrasi-billing` (Slice `INP-S22`) |
| `contract_version` | **`1.0.0`** |
| Status | **`verified`** (Pengujian Otomatis Terpadu 100% Sukses) |
| Laporan Hasil Uji | [Laporan Testing Integrasi Billing](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/integrasi-billing/testing/test-by-agy/laporan-testing-integrasi-billing.md) |

---

## 1. Pemetaan Kriteria Penerimaan Kanonik (`RWI-AC-236` s.d. `241`)

| ID Kriteria | Deskripsi Kriteria Penerimaan | Jenis Pengujian | Target Class / File Test | Status Hasil Uji |
|---|---|:---:|---|:---:|
| **`RWI-AC-236`** | Sinkronisasi admisi `Admitted` memicu pembuatan folio billing `OPEN`; room charge baru aktif saat `Bed Occupied` fisik. | Integration Test | `InpBedPlacement`, `BilFolio`, `InpIntegrationOutboxes` | **LULUS (100%)** |
| **`RWI-AC-237`** | Koreksi kamar hanya saat billing `OPEN`, dilakukan oleh Supervisor, beralasan wajib, immutable versioning, picu `OCCUPANCY_CORRECTED`. | Unit & Integration Test | `InpBedOccupancyService.TransferAsync` | **LULUS (100%)** |
| **`RWI-AC-238`** | Clearance kasir mengontrol pelepasan fisik; penolakan pelepasan saat pending; *Auto-Reblock* saat revoked; *Supervisor Override* beralasan wajib. | Integration Test | `InpatientClearanceGateService.cs` | **LULUS (100%)** |
| **`RWI-AC-239`** | `OccupancyEndAt` identik dengan `PhysicallyLeftAt`; finalisasi tagihan kamar dipatok dari jam kepergian fisik. | Unit & Integration Test | `InpatientClearanceGateService.cs` | **LULUS (100%)** |
| **`RWI-AC-240`** | Antarmuka bangsal bebas nominal rupiah; hanya status operasional dan blocker string; `InpatientBilling:View` diperlukan untuk rupiah. | UI Component & API Test | `InpatientBillingOperationalController.cs` | **LULUS (100%)** |
| **`RWI-AC-241`** | Outbox transaksional `InpIntegrationOutbox`, compound key `IdempotencyKey` unik, retry exponential backoff. | Worker & DB Test | `InpatientIntegrationOutboxWorker.cs` | **LULUS (100%)** |

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

- [x] Seluruh Unit & Integration Test (cakupan untuk logika outbox, validasi mutasi, clearance gate, auto-reblock, supervisor override, dan privasi tampilan bangsal) **LULUS 100% (14/14 kasus uji)**.
- [x] Seluruh Integration Test basis data (PostgreSQL live query, atomisitas transaksi, dan unique constraint `UQ_InpIntegrationOutbox_IdempotencyKey`) **LULUS 100%**.
- [x] Pengujian otomatis terpadu dieksekusi melalui skrip `test-billing-integration.mjs` dengan hasil lolos tanpa deviasi (*zero defect*).
- [x] Laporan hasil pengujian terpadu telah disusun lengkap dan terdokumentasi di [`testing/test-by-agy/laporan-testing-integrasi-billing.md`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/integrasi-billing/testing/test-by-agy/laporan-testing-integrasi-billing.md).
