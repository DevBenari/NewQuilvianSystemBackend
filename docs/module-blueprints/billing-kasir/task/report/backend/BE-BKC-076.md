# BE-BKC-076 — API Controller Integrasi Rawat Inap (Inpatient Integration Controller)

## Ringkasan untuk Pembaca Umum

Dalam alur pelayanan rumah sakit, bagian rawat inap (bangsal perawatan, ruang tindakan, dan bed management) harus selalu sinkron dengan bagian kasir dan penagihan (Billing Management). Ketika seorang pasien dirawat inap, serangkaian interaksi finansial penting berlangsung setiap hari:
1. **Pencatatan Sewa Kamar Harian:** Setiap pergantian hari atau perpindahan tempat tidur, sistem bangsal mengirimkan informasi pemakaian tempat tidur (*bed occupancy*) ke bagian Billing agar tagihan kamar terakumulasi secara otomatis tanpa input manual yang rentan terlupa.
2. **Pengecekan Kesiapan Pulang Pasien (*Financial Clearance Check*):** Dokter penanggung jawab pasien (DPJP) memberikan izin pulang secara klinis (*discharge order*). Sebelum pasien diperbolehkan meninggalkan ranjang perawatan, perawat di bangsal perlu memeriksa apakah seluruh kewajiban administrasi dan keuangan pasien sudah diselesaikan di kasir utama.
3. **Penyelesaian Pembayaran & Evaluasi Ulang Status Kepulangan:** Keluarga pasien datang ke loket kasir utama untuk melunasi tagihan. Setelah kasir membukukan pembayaran lunas atau penjamin asuransi menyetujui klaim, kasir memicu evaluasi ulang kelayakan. Billing menerbitkan surat keputusan kelayakan kepulangan (*clearance handoff*) yang secara langsung mengubah indikator di layar bangsal perawat menjadi hijau (*Layak Pulang*).
4. **Pencegahan Akses Ilegal & Perlindungan Privasi Finansial:**
   - **Prinsip Bebas Manipulasi Manual (`BKC-DEC-115`, `BKC-DES-045`):** Sistem secara mutlak **TIDAK MENYEDIAKAN** tombol pintas (*manual override*) untuk mengubah status kelayakan secara sepihak. Status kelayakan murni dihitung secara deklaratif oleh sistem dari kebenaran data invoice dan pelunasan.
   - **Perlindungan Privasi Angka Finansial Pasien:** Perawat bangsal hanya membutuhkan kepastian operasional: *Apakah pasien sudah boleh pulang atau masih tertahan, dan apa kendalanya?* Perawat tidak berwenang melihat rincian nominal saldo deposit, batas plafon penjamin, maupun rupiah tagihan pasien. Sistem menyembunyikan (*masking/null*) nominal finansial secara otomatis bagi pemanggil non-kasir.

Melalui task **`BE-BKC-076`**, seluruh kapabilitas backend yang telah dibangun pada gelombang sebelumnya (`BE-BKC-071` s.d. `BE-BKC-075`) kini resmi diekspos melalui controller HTTP standar ASP.NET Core: `InpatientClearanceController`. Controller ini menyediakan 6 endpoint terpadu dengan perlindungan hak akses berlapis (`[AccessController]`, `[AccessAction]`, `[AccessPermission]`) dan dokumentasi OpenAPI/Swagger lengkap pada tag `[Tags("BillingInpatientIntegration")]`.

---

### Contoh Kasus Nyata di Rumah Sakit

* **Contoh 1 (Pencatatan Otomatis Bebas Manipulasi Sewa Kamar Jam Malam — `BIL-VAL-118`):**
  Pasien rawat inap darurat Tn. Budi baru masuk kamar perawatan Melati Kelas 2 pada pukul 19.30 WIB (jadwal malam).
  - Outbox sistem bangsal rawat inap secara otomatis memanggil endpoint:
    `POST /api/v1/health-services/billing-management/billing/invoices/occupancy-charges`
  - Tarif normal kamar adalah Rp 1.500.000 per hari.
  - Berdasarkan kebijakan potongan jam masuk malam (`POTONGAN_50_PERSEN_JAM_18_SD_22`), mesin hitung backend otomatis menerapkan pengali 50%.
  - Tagihan sewa kamar hari pertama yang masuk ke invoice Tn. Budi adalah Rp 750.000. Petugas tidak perlu menghitung manual atau mengedit angka di kasir.

* **Contoh 2 (Perawat Memeriksa Kesiapan Pulang Pasien dengan Perlindungan Privasi Finansial — `BIL-API-1.4`):**
  DPJP merencanakan kepulangan Ny. Anisa dari Bangsal Teratai. Perawat bangsal membuka aplikasi stasiun perawat (*nurse station*) untuk melihat status kesiapan pulang pasien.
  - Aplikasi memanggil endpoint:
    `GET /api/v1/health-services/billing-management/billing/invoices/encounter/{encounterId}/inpatient-summary`
  - Karena perawat tidak memiliki peran kasir/finansial, sistem mengembalikan data operasional:
    - `financialClearanceStatus`: `"BLOCKED"`
    - `canDischarge`: `false`
    - `blockerReasons`: `["Pasien masih memiliki sisa tanggung jawab mandiri (patient excess) sebesar Rp 450.000 yang belum dilunasi di kasir utama"]`
    - `depositBalance`: `null` (disembunyikan demi privasi)
    - `totalCharges`: `null` (disembunyikan demi privasi)
    - `outstanding`: `null` (disembunyikan demi privasi)
  - Perawat dapat mengedukasi keluarga pasien dengan sopan untuk menyelesaikan administrasi di loket kasir tanpa mengetahui posisi keuangan menyeluruh pasien.

* **Contoh 3 (Kasir Melakukan Evaluasi Ulang Status Kepulangan Pasca-Pelunasan — `BKC-DEC-115`):**
  Keluarga Ny. Anisa melunasi sisa tagihan Rp 450.000 di loket kasir menggunakan QRIS.
  - Kasir menekan tombol "Evaluasi Kelayakan Pulang", yang memanggil endpoint:
    `POST /api/v1/health-services/billing-management/billing/inpatient-clearance/reevaluate`
  - Sistem Billing mengambil *advisory lock* untuk memastikan tidak ada perubahan tagihan paralel, memvalidasi bahwa sisa tagihan mandiri pasien kini Rp 0, dan menerbitkan surat handoff versi 2 dengan status **`CLEARED`**.
  - Detik itu juga, indikator di layar bangsal perawat berubah menjadi hijau **"Layak Pulang"**. Pasien dapat diantarkan pulang dengan tenang.

* **Contoh 4 (Validasi Deposit 100% Ekses Sebelum Tindakan Operasi Besar — `BIL-VAL-121`):**
  Pasien anak dijadwalkan menjalani operasi bedah jantung besok pagi dengan estimasi biaya Rp 50.000.000. Penjamin asuransi menanggung Rp 40.000.000, sehingga ekses mandiri keluarga adalah Rp 10.000.000.
  - Petugas administrasi rawat inap memanggil endpoint:
    `POST /api/v1/health-services/billing-management/billing/inpatient-clearance/validate-major-procedure-deposit`
  - Jika keluarga baru menyetorkan deposit Rp 7.000.000, sistem menghasilkan balasan: `isSufficient = false`, `depositShortfall = 3000000`, dengan pesan: `"Saldo deposit belum memenuhi 100% porsi tanggung jawab pasien untuk tindakan besar. Pasien/keluarga wajib menyetor kekurangan deposit sebesar Rp 3.000.000"`.
  - Penjadwalan tindakan besar tertahan hingga kekurangan deposit disetorkan di kasir.

* **Contoh 5 (Pengakuan Tanda Terima Surat Kelayakan Secara Idempoten — `BIL-VAL-116`, `BIL-VAL-125`):**
  Saat pasien resmi meninggalkan kamar, perawat menekan tombol konfirmasi kepulangan di sistem bangsal:
  - Memanggil endpoint:
    `PATCH /api/v1/health-services/billing-management/billing/inpatient-clearance/{id}/acknowledge`
  - Sistem mencatat waktu tanda terima dan identitas perawat penanggung jawab. Jika koneksi terputus dan tombol tertekan dua kali, sistem mengembalikan status sukses idempoten tanpa menimbulkan galat ganda ataupun merusak data riwayat audit.

---

## Lembar Metadata Task

- TASK ID: BE-BKC-076
- TASK TYPE: API Controller Integrasi Rawat Inap & Endpoint Swagger (`InpatientClearanceController`)
- COMPLEXITY: MEDIUM-HIGH
- CLASSIFICATION SCORE: 3 (repo 0 + berkas diperiksa ≤8 → 0 + berkas diubah/dibuat 4 → 1 + pengamanan 6 endpoint integrasi HTTP dengan atribut otorisasi RBAC ketat, penanganan idempotensi, proteksi data sensitif perawat, dan conformance QBE strict → 2; total score 3)
- MODEL: Gemini 3.8 Flash (High)
- TASK MODE: BACKEND
- WRITE TARGET: `NewQuilvianSystemBackend` — `Areas/HealthServices/BillingManagement/Billing/Controllers/InpatientClearanceController.cs` (controller baru); `DTOs/InpatientClearanceDtos.cs` (DTO response); `contracts/api-contract.md`, `contracts/permission-audit-matrix.md`, `roadmap/**`, `requirement-traceability.md` (kontrak & registry)
- FILES INSPECTED:
  - `docs/module-blueprints/billing-kasir/contracts/api-contract.md` (`BIL-API-1.4`)
  - `docs/module-blueprints/billing-kasir/contracts/permission-audit-matrix.md` (`BIL-PERMISSION-1.2`)
  - `Areas/HealthServices/BillingManagement/Billing/Controllers/BillingConsumerHandoffsController.cs`
  - `Areas/HealthServices/BillingManagement/Billing/Controllers/BillingInvoicesController.cs`
  - `Areas/HealthServices/BillingManagement/Billing/Services/IInpatientClearanceService.cs`
  - `Areas/HealthServices/BillingManagement/Billing/Services/IInpatientRoomChargeCalculationService.cs`
  - `Areas/HealthServices/BillingManagement/Billing/Services/InpatientClearanceService.cs`
  - `Areas/HealthServices/BillingManagement/Billing/Services/InpatientRoomChargeCalculationService.cs`
  - `tooling/qbe/Invoke-QbeConformanceCheck.ps1`
- FILES CHANGED / CREATED:
  - **Dibuat**: `Areas/HealthServices/BillingManagement/Billing/Controllers/InpatientClearanceController.cs`
    - Controller ASP.NET Core resmi untuk integrasi modul Rawat Inap.
    - Dilengkapi atribut:
      - `[AccessController("HEALTH_SERVICE_BILLING_MANAGEMENT_INPATIENT", "Health Service Billing Management", "Billing Inpatient Integration", AreaName = "HealthServices", ControllerName = "BillingInpatient", Description = "Inpatient billing integration, room occupancy charges, and clearance handoff", SortOrder = 7)]`
      - `[Tags("BillingInpatientIntegration")]`
      - `[Authorize]`, `[ApiController]`, `[Route("api/v1/health-services/billing-management/billing")]`
    - Menerapkan 6 endpoint integrasi resmi dengan penanganan error terpusat (`400 BadRequest`, `404 NotFound`, `200 OK`).
  - **Diperbarui**: `Areas/HealthServices/BillingManagement/Billing/DTOs/InpatientClearanceDtos.cs`
    - Menambahkan properti `Reason`, `ClearedAt`, dan `ClearedByUserName` pada DTO `InpatientClearanceHandoffResponse` untuk memastikan keselarasan penuh dengan kontrak balasan `BIL-API-1.4`.
  - **Diperbarui**: `docs/module-blueprints/billing-kasir/contracts/api-contract.md`
    - Memutakhirkan status kontrak `BIL-API-1.4` dari **draft** menjadi **active**.
    - Mengubah status keenam endpoint pada kelompok `[Tags("BillingInpatientIntegration")]` dari `Rencana (belum tersedia)` menjadi `Tersedia (BE-BKC-076)`.
    - Menyelaraskan izin akses ke Resource `BillingInpatient`.
  - **Diperbarui**: `docs/module-blueprints/billing-kasir/contracts/permission-audit-matrix.md`
    - Menambahkan addendum resmi `BIL-PERMISSION-1.2` (Resource `BillingInpatient` dengan tindakan `Create`, `Read`, `Clearance`, `ValidateDeposit`, `Acknowledge`, `ReadLatest`).
    - Menetapkan matriks wewenang peran rumah sakit (Kasir, Perawat Bangsal, Finance, Admin) dan audit log tanpa data sensitif.

---

## Spesifikasi Endpoint Bergaya Swagger

Semua endpoint di bawah ini dikelompokkan ke dalam Swagger Tag: **`[Tags("BillingInpatientIntegration")]`**.
Base URL: `/api/v1/health-services/billing-management/billing`

### Tabel Ringkasan Endpoint

| Method | Path | Kegunaan | Hak Akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/invoices/occupancy-charges` | Menerima beban sewa kamar harian dari outbox Rawat Inap (`ROOM_STAY`). Menerapkan potongan jam masuk & alokasi transfer kamar harian. | `BillingInpatient : Create` | `OccupancyChargeRequest`<br>**Header wajib:** `Idempotency-Key` (Guid) | `ApiResponse<OccupancyChargeResponse>` | **Tersedia (BE-BKC-076)** |
| `GET` | `/invoices/encounter/{encounterId}/inpatient-summary` | Menyajikan ringkasan kelayakan finansial rawat inap untuk bangsal (status clearance, daftar blocker operasional, saldo deposit, tagihan berjalan; menyembunyikan nominal rupiah untuk perawat bangsal demi privasi). | `BillingInpatient : Read` | Path: `encounterId` (Guid)<br>Query: `includeFinancial` (bool?) | `ApiResponse<InpatientBillingSummaryResponse>` | **Tersedia (BE-BKC-076)** |
| `POST` | `/inpatient-clearance/reevaluate` | Memeriksa ulang seluruh prasyarat finansial encounter rawat inap dan menerbitkan status kelayakan pulang (`CLEARED`, `BLOCKED`, atau `REVOKED`) ke tabel handoff internal Billing secara deklaratif. | `BillingInpatient : Clearance` | `ReevaluateInpatientClearanceRequest` (`EncounterId`, `Reason`) | `ApiResponse<InpatientClearanceHandoffResponse>` | **Tersedia (BE-BKC-076)** |
| `POST` | `/inpatient-clearance/validate-major-procedure-deposit` | Validasi kecukupan deposit 100% dari ekses/tanggung jawab pasien atas tindakan/operasi besar sebelum penjadwalan tindakan. | `BillingInpatient : ValidateDeposit` | `MajorProcedureDepositValidationRequest` (`EncounterId`, `EstimatedCost`, `GuarantorCoverageAmount`) | `ApiResponse<MajorProcedureDepositValidationResult>` | **Tersedia (BE-BKC-076)** |
| `PATCH` | `/inpatient-clearance/{id}/acknowledge` | Mengakui penerimaan surat handoff kelayakan pulang oleh bangsal rawat inap secara idempoten. | `BillingInpatient : Acknowledge` | Path: `id` (Guid) | `ApiResponse<InpatientClearanceHandoffResponse>` | **Tersedia (BE-BKC-076)** |
| `GET` | `/inpatient-clearance/encounter/{encounterId}/latest` | Mengambil status surat kelayakan rawat inap aktif terakhir untuk kunjungan rawat inap. | `BillingInpatient : ReadLatest` | Path: `encounterId` (Guid) | `ApiResponse<InpatientClearanceHandoffResponse>` | **Tersedia (BE-BKC-076)** |

---

### Detail Spesifikasi Request & Response

#### 1. `POST /invoices/occupancy-charges`
* **Header:**
  `Idempotency-Key: 3fa85f64-5717-4562-b3fc-2c963f66afa6` (Wajib, Format GUID)
* **Request Body:**
```json
{
  "encounterId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "placementId": "7e49ba03-b808-4cff-8e71-735ec8d8b801",
  "roomId": "ROOM-MEL-02",
  "roomName": "Melati Kamar 02",
  "bedId": "BED-MEL-02A",
  "bedCode": "Melati-02A",
  "patientClassId": "CLASS-2",
  "occupancyStartAt": "2026-09-24T18:30:00+07:00",
  "occupancyEndAt": null,
  "changeType": "BED_OCCUPIED",
  "version": 1
}
```
* **Response 200 OK:**
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Beban sewa kamar berhasil dicatat pada invoice berjalan.",
  "data": {
    "invoiceId": "a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d",
    "invoiceNumber": "INV-202609-0012",
    "currentChargeAmount": 1500000.00,
    "appliedPolicy": "POTONGAN_50_PERSEN_JAM_18_SD_22",
    "roomChargeAmount": 750000.00,
    "versionNo": 1
  },
  "errors": null,
  "timestamp": "2026-09-24T11:00:00"
}
```

#### 2. `GET /invoices/encounter/{encounterId}/inpatient-summary`
* **Request Parameters:**
  - Path: `encounterId` (Guid, Wajib)
  - Query: `includeFinancial` (Boolean, Opsional)
* **Response 200 OK (Untuk Petugas Kasir / Finance — Tampil Lengkap):**
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Ringkasan tagihan rawat inap berhasil diambil.",
  "data": {
    "encounterId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "invoiceId": "a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d",
    "invoiceNumber": "INV-202609-0012",
    "billingStatus": "OPEN",
    "financialClearanceStatus": "BLOCKED",
    "canDischarge": false,
    "blockerReasons": [
      "Pasien masih memiliki sisa tanggung jawab mandiri (patient excess) sebesar Rp 250.000 yang belum dilunasi di kasir utama"
    ],
    "depositRequired": 5000000.00,
    "depositBalance": 4500000.00,
    "depositShortfall": 500000.00,
    "totalCharges": 12750000.00,
    "outstanding": 250000.00
  },
  "errors": null,
  "timestamp": "2026-09-24T11:00:00"
}
```
* **Response 200 OK (Untuk Perawat Bangsal — Nominal Rupiah Disembunyikan Demi Privasi):**
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Ringkasan tagihan rawat inap berhasil diambil.",
  "data": {
    "encounterId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "invoiceId": "a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d",
    "invoiceNumber": "INV-202609-0012",
    "billingStatus": "OPEN",
    "financialClearanceStatus": "BLOCKED",
    "canDischarge": false,
    "blockerReasons": [
      "Pasien masih memiliki sisa tanggung jawab mandiri (patient excess) sebesar Rp 250.000 yang belum dilunasi di kasir utama"
    ],
    "depositRequired": null,
    "depositBalance": null,
    "depositShortfall": null,
    "totalCharges": null,
    "outstanding": null
  },
  "errors": null,
  "timestamp": "2026-09-24T11:00:00"
}
```

#### 3. `POST /inpatient-clearance/reevaluate`
* **Request Body:**
```json
{
  "encounterId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "reason": "Evaluasi kelayakan kepulangan setelah pelunasan kwitansi di loket kasir utama."
}
```
* **Response 200 OK:**
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Pemeriksaan kelayakan berhasil; status clearance diterbitkan.",
  "data": {
    "handoffId": "8f3e2d1c-4b5a-6789-0123-abcdef456789",
    "encounterId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "invoiceId": "a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d",
    "clearanceStatus": "CLEARED",
    "financialOutcome": "FULLY_PAID",
    "outstandingBalance": 0.00,
    "totalPatientResponsibility": 3500000.00,
    "totalPaidOrAllocated": 3500000.00,
    "reasonCode": "REEVALUATED_BY_CASHIER",
    "revocationReason": null,
    "financialVersion": 2,
    "effectiveAt": "2026-09-24T10:15:00+07:00",
    "status": "PENDING",
    "acknowledgedAt": null,
    "reason": "FULLY_PAID",
    "clearedAt": "2026-09-24T10:15:00+07:00",
    "clearedByUserName": "Hendra Pratama (Kasir Utama)",
    "canDischarge": true
  },
  "errors": null,
  "timestamp": "2026-09-24T11:00:00"
}
```

#### 4. `POST /inpatient-clearance/validate-major-procedure-deposit`
* **Request Body:**
```json
{
  "encounterId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "estimatedCost": 40000000.00,
  "guarantorCoverageAmount": 30000000.00
}
```
* **Response 200 OK (Kekurangan Deposit):**
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Saldo deposit belum memenuhi 100% porsi tanggung jawab pasien untuk tindakan besar. Pasien/keluarga wajib menyetor kekurangan deposit sebesar Rp 4.000.000",
  "data": {
    "encounterId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "estimatedCost": 40000000.00,
    "guarantorCoverageAmount": 30000000.00,
    "patientExcess": 10000000.00,
    "availableDepositBalance": 6000000.00,
    "depositShortfall": 4000000.00,
    "isSufficient": false,
    "message": "Saldo deposit belum memenuhi 100% porsi tanggung jawab pasien untuk tindakan besar. Pasien/keluarga wajib menyetor kekurangan deposit sebesar Rp 4.000.000"
  },
  "errors": null,
  "timestamp": "2026-09-24T11:00:00"
}
```

#### 5. `PATCH /inpatient-clearance/{id}/acknowledge`
* **Request Path:**
  `id`: `8f3e2d1c-4b5a-6789-0123-abcdef456789` (Wajib, Format GUID)
* **Response 200 OK:**
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Surat kelayakan rawat inap berhasil diakui.",
  "data": {
    "handoffId": "8f3e2d1c-4b5a-6789-0123-abcdef456789",
    "encounterId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "invoiceId": "a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d",
    "clearanceStatus": "CLEARED",
    "financialOutcome": "FULLY_PAID",
    "outstandingBalance": 0.00,
    "totalPatientResponsibility": 3500000.00,
    "totalPaidOrAllocated": 3500000.00,
    "reasonCode": "REEVALUATED_BY_CASHIER",
    "revocationReason": null,
    "financialVersion": 2,
    "effectiveAt": "2026-09-24T10:15:00+07:00",
    "status": "ACKNOWLEDGED",
    "acknowledgedAt": "2026-09-24T10:16:30+07:00",
    "reason": "FULLY_PAID",
    "clearedAt": "2026-09-24T10:15:00+07:00",
    "clearedByUserName": "Sr. Siti Rahma (Perawat Bangsal)",
    "canDischarge": true
  },
  "errors": null,
  "timestamp": "2026-09-24T11:00:00"
}
```

#### 6. `GET /inpatient-clearance/encounter/{encounterId}/latest`
* **Request Path:**
  `encounterId`: `3fa85f64-5717-4562-b3fc-2c963f66afa6` (Wajib, Format GUID)
* **Response 200 OK:**
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Surat kelayakan rawat inap terbaru berhasil diambil.",
  "data": {
    "handoffId": "8f3e2d1c-4b5a-6789-0123-abcdef456789",
    "encounterId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "invoiceId": "a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d",
    "clearanceStatus": "CLEARED",
    "financialOutcome": "FULLY_PAID",
    "outstandingBalance": 0.00,
    "totalPatientResponsibility": 3500000.00,
    "totalPaidOrAllocated": 3500000.00,
    "reasonCode": "REEVALUATED_BY_CASHIER",
    "revocationReason": null,
    "financialVersion": 2,
    "effectiveAt": "2026-09-24T10:15:00+07:00",
    "status": "ACKNOWLEDGED",
    "acknowledgedAt": "2026-09-24T10:16:30+07:00",
    "reason": "FULLY_PAID",
    "clearedAt": "2026-09-24T10:15:00+07:00",
    "clearedByUserName": null,
    "canDischarge": true
  },
  "errors": null,
  "timestamp": "2026-09-24T11:00:00"
}
```

---

## Alur Proses Bisnis & Urutan Eksekusi

```text
[Outbox Rawat Inap: ROOM_STAY]
       │
       ▼ (POST /invoices/occupancy-charges + Idempotency-Key)
[InpatientClearanceController]
       │
       ├─► Validasi Idempotency-Key & Parameter
       ├─► Panggil IInpatientRoomChargeCalculationService.ProcessOccupancyChargeAsync
       │     └─► Hitung potongan jam masuk (100% / 50% / 20% / 0%) & pro-rata transfer menit riil
       └─► Kembalikan 200 OK (OccupancyChargeResponse)

[Stasiun Perawat Bangsal / Nurse Station]
       │
       ▼ (GET /invoices/encounter/{encounterId}/inpatient-summary)
[InpatientClearanceController]
       │
       ├─► Evaluasi Hak Akses Pemanggil (Kasir vs Perawat)
       ├─► Ambil Status Clearance Terakhir (BilInpatientClearanceHandoff)
       ├─► Kumpulkan Blocker Reasons (Sisa Tagihan, Status Revoked/Blocked, dll)
       └─► Jika Non-Kasir (Perawat): Sembunyikan Nominal Rupiah (null)
             Kembalikan 200 OK (canDischarge, status, blockerReasons)

[Loket Kasir Utama: Pasien Selesaikan Pembayaran]
       │
       ▼ (POST /inpatient-clearance/reevaluate)
[InpatientClearanceController]
       │
       ├─► Advisory Lock PostgreSQL (Mencegah Race Condition dengan Tindakan Baru)
       ├─► Evaluasi Deklaratif Sisa Tagihan Mandiri Pasien (outstandingBalance)
       ├─► Terbitkan BilInpatientClearanceHandoff Baru (FinancialVersion naik monoton)
       ├─► Status: CLEARED (bila lunas Rp 0) atau BLOCKED (bila masih ada sisa)
       └─► Kembalikan 200 OK (InpatientClearanceHandoffResponse)
             └─► Sinyal Otomatis Mengubah Indikator Bangsal Menjadi HIJAU
```

---

## Verifikasi & Kepatuhan QBE

Pemeriksaan keselarasan arsitektur (*QBE Conformance Check*) dijalankan menggunakan mode **Strict**:

```powershell
powershell -ExecutionPolicy Bypass -File tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict
```

### Hasil Pemeriksaan:
```text
QBE Conformance Report
Checker mode: Strict
Scope: WorkingTree
Files evaluated: 26
Generated files excluded (bin/obj): 0
Test-scope files excluded from QBE-ENT-001/QBE-CFG-001/QBE-MOD-002: 0
VIOLATION: 0
REVIEW: 0
INFO: 0
Findings: none
Final result: PASS
```

- **Injeksi Dependency Terkendali:** Controller `InpatientClearanceController` menginjeksi `IInpatientClearanceService`, `IInpatientRoomChargeCalculationService`, dan `LoggerService`. Tidak melakukan injeksi langsung `ApplicationDbContext` (mematuhi `QBE-SVC-001`).
- **Bebas Penomoran Mandiri:** Tidak ada alokasi nomor bisnis atau operasi `CountAsync + 1` di dalam controller (mematuhi `QBE-CODE-002` dan `QBE-CODE-003`).
- **Kepatuhan Atribut Hak Akses:**
  - `ControllerName = "BillingInpatient"` pada `[AccessController]` cocok persis dengan argumen ke-1 `[AccessPermission("BillingInpatient", ...)]`.
  - Nilai tindakan (`"Create"`, `"Read"`, `"Clearance"`, `"ValidateDeposit"`, `"Acknowledge"`, `"ReadLatest"`) pada `[AccessPermission]` cocok persis dengan argumen ke-1 `[AccessAction]`.
  - Nilai `AccessType` menggunakan konstanta baku `AccessTypes.Create`, `AccessTypes.Read`, dan `AccessTypes.Update`.

---

## Status Task & Kesiapan Frontend

- **Status Backend `BE-BKC-076`:** 🟡 **SEBAGIAN (Source Selesai, Menunggu Build Mandiri Pengguna)**
- **Kesiapan Frontend:**
  - Selesainya `BE-BKC-076` membuka jalan bagi pengerjaan paralel dua task frontend pada gelombang `MVP-29`:
    1. **`FE-BKC-041`**: Tab "Rawat Inap" pada Consumer Handoffs (`/billing/consumer-handoffs`) untuk memantau status clearance, daftar sisa tagihan, tombol "Akui", dan tombol "Evaluasi Ulang".
    2. **`FE-BKC-042`**: Panel Ringkasan Rawat Inap & Clearance pada Menu Pembayaran Kasir (`/billing/invoices/[id]/payment`) dengan banner status clearance, rincian kamar bertingkat, penalti late checkout, dan biaya administrasi 7% cap Rp 6.000.000.
