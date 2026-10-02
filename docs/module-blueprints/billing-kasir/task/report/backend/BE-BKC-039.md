# Laporan Perubahan Backend — `BE-BKC-039`

## Metadata

| Field | Nilai |
| --- | --- |
| **Task ID** | `BE-BKC-039` |
| **Judul** | Kebijakan minimum deposit per penjamin dan kelas perawatan |
| **Slice / Milestone** | `BKC-PH-020` (Deposit rawat inap terikat episode — integrasi dengan alur admisi) |
| **Roadmap** | `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` baris 1547 |
| **Trace** | `RWI-DEC-094`, `RWI-OQ-053`; `FR-RI-164`, `FR-RI-175` pada `04-prd-to-mvp.md` `0.6.1` Rawat Inap; `api-contract.md` `0.6.1` Rawat Inap bagian Deposit Rawat Inap |
| **Contract Version** | `BIL-API-0.4` ditambah satu operasi baca; `BIL-VALIDATION-0.4` dan `BIL-PERMISSION-0.4` tetap |
| **Dependency** | `BE-BKC-001` (Fondasi Billing); `BE-BKC-009` (Deposit ledger yang sudah ada). Tidak bergantung pada task Rawat Inap mana pun |
| **Klasifikasi** | `MEDIUM` (1 model master baru, 1 konfigurasi EF, 1 migrasi database, 1 DTO, 1 method service, 1 endpoint controller) |
| **Task Mode** | `TASK MODE: BACKEND` |
| **Target Tulis** | `NewQuilvianSystemBackend`, branch `Yasmina` |
| **Model** | Gemini 3.8 Flash |
| **Tanggal** | 9 September 2026 |
| **Status** | `Selesai` (Implementasi selesai; verifikasi manual build/test dilakukan oleh pengguna) |

### Backend Governance Preflight

| Parameter | Nilai |
| --- | --- |
| **Area** | `HealthServices` |
| **Module / Submodule** | `BillingManagement / Billing` dan `BillingManagement / MasterData` |
| **Owner / Prefix Registry** | Prefix `Mst` (`Administrator / HealthServices`, Category: `BUSINESS DOMAIN / MASTER / REFERENCE`, Status: `ACTIVE`); Prefix `Bil` (`HealthServices / BillingManagement / Billing`, Category: `BUSINESS DOMAIN / MODULE`, Status: `ACTIVE`) |
| **Keberlakuan** | `NEW CODE` — Master data kebijakan deposit baru (`MstDepositPolicy`), konfigurasi EF Core, DTO, method service, dan endpoint controller |
| **QBE ID yang Berlaku** | `QBE-MOD-002`, `QBE-MOD-003`, `QBE-API-001`, `QBE-DTO-001`, `QBE-DB-001` |
| **Pengecualian / Temuan** | Sesuai instruksi pemilik, proses kompilasi (`dotnet build`) dan eksekusi pengujian otomatis terminal ditiadakan untuk diverifikasi secara manual oleh pengguna |

---

## 1. Masalah yang Diselesaikan

Sebelumnya, pada alur admisi rawat inap (pendaftaran pasien masuk rawat inap), nominal deposit sering kali di-hardcode di antarmuka (misalnya tertulis Rp100.000.000 dengan kalimat wajib diisi dan tidak boleh Rp0). Padahal, kebijakan rumah sakit menentukan bahwa:
1. Pasien dengan penjamin asuransi atau perusahaan yang menanggung penuh tidak boleh dimintai uang muka deposit.
2. Setiap penjamin dan kelas perawatan memiliki ambang batas minimum deposit yang berbeda-beda sesuai kebijakan keuangan rumah sakit.
3. Bila kombinasi penjamin dan kelas belum memiliki aturan khusus, langkah pengisian deposit harus dapat dilewati dengan wajar tanpa menimbulkan pesan galat (*error 404*).

Task `BE-BKC-039` menyediakan master data `MstDepositPolicy` beserta endpoint pembacaan `GET /patient-funds/deposit-policies` untuk memberikan angka minimum deposit dan interval tindak lanjut yang akurat dan dinamis kepada sistem admisi dan kasir.

---

## 2. Proses Bisnis

```text
Petugas Admisi memilih Penjamin & Kelas Perawatan Pasien
                       │
                       ▼
Frontend Admisi memanggil GET /patient-funds/deposit-policies?guarantorId={id}&patientClassId={id}
                       │
                       ▼
Sistem mencari kebijakan aktif di MstDepositPolicy (pencocokan spesifik hingga default global)
                       │
         ┌─────────────┴─────────────┐
         ▼                           ▼
[Kebijakan Ditemukan]      [Tidak Ada Kebijakan / Kosong]
         │                           │
  Mengembalikan:              Mengembalikan HTTP 200 OK:
  - isRequired = true/false   - isRequired = false
  - minimumAmount = Rp X      - minimumAmount = Rp 0
  - followUpIntervalDays = N  - followUpIntervalDays = 0
         │                           │
         └─────────────┬─────────────┘
                       ▼
Layar Admisi menampilkan nominal minimum yang disyaratkan atau langsung melewati langkah deposit
```

### Contoh Skenario Rumah Sakit:
- **Skenario A (Pasien Umum Kelas VIP):** Pasien memilih kelas VIP dengan pembayaran tunai/mandiri. Kebijakan rumah sakit mensyaratkan uang muka minimum Rp10.000.000 dengan evaluasi kekurangan tiap 3 hari. Sistem mengembalikan `isRequired = true`, `minimumAmount = 10000000`, `followUpIntervalDays = 3`.
- **Skenario B (Pasien Asuransi Perusahaan Kerjasama Penuh):** Pasien memiliki penjamin PT Telkom yang menanggung penuh rawat inap. Kebijakan tercatat dengan `isRequired = false` dan `minimumAmount = 0`. Sistem mengembalikan `isRequired = false`, sehingga petugas admisi tidak meminta deposit sama sekali.
- **Skenario C (Kombinasi Belum Diatur):** Rumah sakit baru membuka kelas perawatan baru dan belum mengatur kebijakan depositnya. Sistem mengembalikan `isRequired = false`, `minimumAmount = 0` dengan kode status `200 OK` (bukan `404 Not Found`), memastikan proses admisi pasien tidak macet.

---

## 3. Spesifikasi Endpoint (Gaya Swagger)

### `[Tags("Health Services / Billing Management / Billing / Patient Funds")]`

| Method | Path | Deskripsi | Otorisasi & Permission | Request Parameter | Response Body |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/health-services/billing-management/billing/patient-funds/deposit-policies` | Mengambil kebijakan minimum deposit berdasarkan penjamin dan kelas perawatan | `[Authorize]`<br>`[AccessAction("Read", "Read Deposit Policy", AccessType = AccessTypes.Read)]`<br>`[AccessPermission("BillingDeposit", "Read")]` | Query String:<br>• `guarantorId` (Guid?, opsional)<br>• `patientClassId` (Guid?, opsional) | `ApiResponse<DepositPolicyResponse>` |

#### Contoh Respons Sukses (Kebijakan Aktif Ditemukan):
```json
{
  "statusCode": 200,
  "message": "Kebijakan deposit berhasil diambil.",
  "data": {
    "isRequired": true,
    "minimumAmount": 10000000.00,
    "followUpIntervalDays": 3,
    "guarantorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "patientClassId": "7d2e5b88-1234-4567-89ab-cdef01234567",
    "policyId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "policyCode": "POL-DEP-VIP-UMUM",
    "policyName": "Deposit Rawat Inap Pasien Umum Kelas VIP",
    "description": "Wajib deposit minimum 10 juta untuk kelas VIP umum",
    "effectiveFrom": "2026-01-01T00:00:00+07:00",
    "effectiveTo": null
  },
  "errors": null
}
```

#### Contoh Respons (Tanpa Kebijakan / Melewati Deposit):
```json
{
  "statusCode": 200,
  "message": "Kebijakan deposit berhasil diambil.",
  "data": {
    "isRequired": false,
    "minimumAmount": 0.00,
    "followUpIntervalDays": 0,
    "guarantorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "patientClassId": "7d2e5b88-1234-4567-89ab-cdef01234567",
    "policyId": null,
    "policyCode": null,
    "policyName": null,
    "description": "Tidak ada kebijakan deposit yang dikonfigurasi untuk penjamin dan kelas ini.",
    "effectiveFrom": null,
    "effectiveTo": null
  },
  "errors": null
}
```

---

## 4. Berkas yang Dibuat dan Diubah

1. **[NEW]** [`Areas/HealthServices/BillingManagement/MasterData/Models/MstDepositPolicy.cs`](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/MasterData/Models/MstDepositPolicy.cs): Entitas master kebijakan minimum deposit dengan kolom audit lengkap (`IdentityModel`).
2. **[NEW]** [`Repositories/Configurations/HealthServices/BillingManagement/MasterData/MstDepositPolicyConfiguration.cs`](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/NewQuilvianSystemBackend/Repositories/Configurations/HealthServices/BillingManagement/MasterData/MstDepositPolicyConfiguration.cs): Konfigurasi EF Core, check constraints (`MinimumAmount >= 0`, `EffectivePeriod`, `FollowUpIntervalDays >= 0`), dan indeks unik/pencarian.
3. **[MODIFY]** [`Repositories/ApplicationDbContext.cs`](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/NewQuilvianSystemBackend/Repositories/ApplicationDbContext.cs): Pendaftaran `DbSet<MstDepositPolicy> MstDepositPolicies`.
4. **[NEW]** [`Areas/HealthServices/BillingManagement/Billing/Dtos/DepositPolicyDtos.cs`](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Billing/Dtos/DepositPolicyDtos.cs): Kontrak transport `DepositPolicyResponse`.
5. **[MODIFY]** [`Areas/HealthServices/BillingManagement/Billing/Services/BillingDepositService.cs`](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Billing/Services/BillingDepositService.cs): Penambahan method `GetDepositPolicyAsync` dengan resolusi hierarkis.
6. **[MODIFY]** [`Areas/HealthServices/BillingManagement/Billing/Controllers/BillingPatientFundsController.cs`](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Billing/Controllers/BillingPatientFundsController.cs): Penambahan action endpoint `GET deposit-policies` dengan `AccessPermission("BillingDeposit", "Read")`.
7. **[NEW]** [`Migrations/20260909050000_AddTableMstDepositPolicy.cs`](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/NewQuilvianSystemBackend/Migrations/20260909050000_AddTableMstDepositPolicy.cs): Berkas migrasi database untuk tabel `MstDepositPolicy`.
8. **[MODIFY]** [`Tests/QuilvianSystemBackend.UnitTests.InMemory/BillingManagement/BillingDepositServiceTests.cs`](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/NewQuilvianSystemBackend/Tests/QuilvianSystemBackend.UnitTests.InMemory/BillingManagement/BillingDepositServiceTests.cs): Penambahan verifikasi kontrak route/permission serta 4 unit test skenario kebijakan deposit.

---

## 5. Status Verifikasi & Catatan Deployment

- **Verifikasi Mandiri:** Pengguna memilih untuk menjalankan proses kompilasi (`dotnet build`) dan pengujian test in-memory secara manual pada lingkungan lokal.
- **Database / Data Awal:** Tabel `MstDepositPolicy` disiapkan kosong tanpa data awal (sesuai *Acceptance Criteria* dan *Definition of Done*), karena penentuan besaran angka deposit merupakan wewenang manajemen keuangan rumah sakit (*Billing/Finance Owner*). Sampai kebijakan diisi, sistem beroperasi dalam mode aman (`isRequired = false`).
