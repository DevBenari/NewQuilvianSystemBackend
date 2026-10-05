# Laporan Perubahan Backend — `BE-FIN-065`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-065` |
| Judul | Petugas dapat memetakan setiap kelompok saldo dan segmennya ke kode akun Accounting, dan melihat apa yang belum terpetakan |
| Slice | `REV-14B` (`EPIC FIN-21` — pemetaan akun control, cutover saldo awal, dan sinkronisasi subledger) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` |
| Trace | `FR-FIN-141`, `FR-FIN-144`; `FIN-DEC-113`; `FIN-DES-080`; `FIN-API-1.5` F.1; `FIN-VAL-1.7` `FIN-VAL-172`..`179`; `FIN-PERM-1.7` G.1-G.3 |
| Contract version | `FIN-API-1.5` F.1 |
| Dependency | `BE-FIN-064` (skema tabel `FinSubledgerControlAccountMap` berdiri) |
| Klasifikasi | `MEDIUM` (1 DTO, 1 file exception, 1 service, 1 controller dengan 5 endpoint operasional + 1 placeholder RBAC, 1 registrasi DI) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` |
| Model | Gemini 3.8 Flash (High) |
| Tanggal | 2 Oktober 2026 |
| Status | `SELESAI` |

---

## 1. Masalah yang diperbaiki

Sebelum task ini:
1. **Ketiadaan Layanan Pemetaan Akun Control:**
   Walaupun skema tabel `FinSubledgerControlAccountMap` telah dibuat pada task `BE-FIN-064`, belum tersedia antarmuka API maupun logika bisnis di backend untuk mengelola baris pemetaan akun control. Staf akuntansi rumah sakit tidak memiliki cara untuk memetakan kelompok saldo (`KAS-KASIR`, `KAS-KECIL`, `PIUTANG`, `UTANG-SUPPLIER`, `UTANG-JASA-MEDIS`) ke kode akun Chart of Accounts (COA) Accounting.
2. **Risiko Pemetaan Ganda dan Penghitungan Ganda (*Double Counting*):**
   Jika suatu kelompok saldo dipetakan secara menyeluruh (tanpa segmen/`NULL`) dan pada saat yang sama ditambahkan pemetaan per segmen spesifik (misalnya segmen BPJS dipetakan terpisah), saldo subledger berisiko terhitung dua kali saat snapshot bulanan dikirim ke Accounting. Sistem belum memiliki validasi fail-closed untuk mencegah tumpang-tindih konfigurasi tersebut.
3. **Ketiadaan Alat Deteksi Kesiapan Integrasi (*Coverage Audit*):**
   Petugas keuangan tidak memiliki alat untuk memantau apakah seluruh kelompok saldo dan segmennya sudah terpetakan secara lengkap. Tanpa mekanisme deteksi ini, proses tutup buku bulanan pada tanggal 1 berpotensi gagal mendadak akibat adanya segmen yang terlewat.

---

## 2. Proses bisnis & Skenario Rumah Sakit

### 2.1 Skenario Operasional Rumah Sakit

Rumah sakit memiliki kebutuhan integrasi pembukuan yang bervariasi:
- **Kas Kasir & Kas Kecil:** Kas operasional kasir rawat jalan/inap dan kas kecil operasional tidak dipecah per debitur, sehingga menggunakan pemetaan menyeluruh (`SegmentKey = NULL`) ke akun kas terkait.
- **Utang Supplier:** Utang pengadaan obat dan BHP medis ke distributor farmasi dipetakan menyeluruh (`SegmentKey = NULL`) ke akun utang usaha farmasi/supplier.
- **Piutang Rumah Sakit:** Rumah sakit dapat memilih:
  1. *Opsi A (Menyeluruh):* Menggunakan satu akun pengendali piutang umum (`SegmentKey = NULL`).
  2. *Opsi B (Bersegmen):* Memecah piutang menjadi 3 akun spesifik:
     - `PAYER`: Piutang penjamin asuransi komersial dan BPJS Kesehatan.
     - `PATIENT_GUARANTOR`: Piutang keluarga penjamin pasien pribadi.
     - `EMPLOYEE_BENEFIT`: Piutang jaminan klaim kesehatan pegawai internal rumah sakit.
- **Utang Jasa Medis:** Dapat dipetakan menyeluruh atau dipecah ke 3 akun profesi medis: `DOCTOR` (dokter spesialis/umum), `NURSE` (perawat), dan `OTHER_PRACTITIONER` (fisioterapis, radiografer, nutrisionis).

### 2.2 Tahapan Alur Kerja Sistem

```text
[Staf Akuntansi RS]
        │
        ├─► 1. Periksa Kesiapan Cakupan (GET /control-accounts/coverage)
        │      └─► Sistem mengevaluasi 5 kelompok saldo: mana yang sudah terpetakan, mana yang belum.
        │
        ├─► 2. Tambah Pemetaan Akun (POST /control-accounts)
        │      ├─► Validasi kelompok saldo (FIN-VAL-172)
        │      ├─► Validasi kesesuaian segmen (FIN-VAL-173)
        │      ├─► Penegakan aturan larangan campur menyeluruh vs segmen (FIN-VAL-176)
        │      ├─► Penegakan keunikan segmen & kode akun (FIN-VAL-174, FIN-VAL-175)
        │      └─► Simpan pemetaan aktif ke FinSubledgerControlAccountMap
        │
        ├─► 3. Koreksi Kode Akun / Catatan (PUT /control-accounts/{id})
        │      └─► Validasi kode akun unik dan simpan perubahan
        │
        └─► 4. Nonaktifkan Pemetaan (POST /control-accounts/{id}/deactivate)
               └─► Menandai IsActive = false; baris tetap tersimpan sebagai rekam jejak historis
```

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

1. `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md`
2. `docs/module-blueprints/finance-management/02-backend-architecture.md` (§FIN-DES-080, §L.8, §L.9)
3. `docs/module-blueprints/finance-management/contracts/api-contract.md` (§F.1)
4. `docs/module-blueprints/finance-management/contracts/validation-matrix.md` (§F.2, FIN-VAL-172..179)
5. `docs/module-blueprints/finance-management/contracts/permission-audit-matrix.md` (§G.1..G.3)
6. `Areas/Corporate/FinanceManagement/AccountingIntegration/Models/FinSubledgerControlAccountMap.cs`
7. `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs`

### 3.2 Berkas yang dibuat dan diubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/DTOs/SubledgerControlAccountDtos.cs` | DTO untuk query berpaging (`ControlAccountMapPagedQuery`), respons (`ControlAccountMapResponse`), pembuatan (`CreateControlAccountMapRequest`), pembaruan (`UpdateControlAccountMapRequest`), penonaktifan (`DeactivateControlAccountMapRequest`), dan analisis cakupan (`ControlAccountCoverageResponse`). |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceSubledgerSetupExceptions.cs` | Exception khusus penanganan respon API: `FinanceSubledgerValidationException` (422), `FinanceSubledgerConflictException` (409), dan `FinanceSubledgerBadRequestException` (400). |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceSubledgerControlAccountService.cs` | Logika bisnis pemetaan akun control, validasi kelayakan segmen, penegakan integritas menyeluruh vs bersegmen, dan audit kelengkapan cakupan sebelum snapshot. |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Controllers/FinanceSubledgerSetupController.cs` | Controller API dengan 5 endpoint operasional pemetaan akun control dan 1 method placeholder RBAC untuk action `Approve` pada resource `FinanceSubledgerSetup`. |
| `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` | Pendaftaran dependensi `FinanceSubledgerControlAccountService` ke service collection ASP.NET Core (`AddScoped`). |

### 3.3 Penegakan Aturan Validasi Bisnis

| Kode Aturan | Kondisi yang Diuji | Respon Sistem |
| :--- | :--- | :--- |
| `FIN-VAL-172` | `BalanceGroup` di luar 5 nilai sah | HTTP 400 — *"Kelompok saldo tidak dikenal."* |
| `FIN-VAL-173` | `SegmentKey` diisi pada kelompok kas/utang supplier, atau di luar daftar debitur/payee yang sah | HTTP 400 — *"Segmen ini tidak berlaku untuk kelompok saldo yang dipilih."* |
| `FIN-VAL-174` | Kelompok dan segmen yang sama sudah memiliki baris aktif | HTTP 409 — *"Kelompok dan segmen ini sudah dipetakan."* |
| `FIN-VAL-175` | Kode akun control sudah dipakai oleh pemetaan aktif lain | HTTP 409 — *"Kode akun ini sudah dipakai pemetaan lain."* |
| `FIN-VAL-176` | Mencoba menambahkan pemetaan menyeluruh saat sudah ada pemetaan bersegmen aktif (atau sebaliknya) | HTTP 422 — *"Satu kelompok saldo tidak boleh memakai pemetaan menyeluruh dan pemetaan per segmen sekaligus."* |
| `FIN-VAL-179` | Kode akun kosong atau panjang melebihi 50 karakter | HTTP 400 — *"Kode akun maksimal 50 karakter."* |

---

## 4. Dokumentasi Endpoint API

Tag Grup: `[Tags("Corporate / Finance Management / Subledger Setup")]`  
Base URL: `/api/v1/corporate/finance-management/subledger-setup`

| Method | Path | Deskripsi | Hak Akses | Request Body / Query | Response Body | Status HTTP |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/control-accounts` | Mengambil daftar pemetaan akun control secara berpaging dengan filter kelompok dan status aktif | `FinanceSubledgerSetup : Read` | `ControlAccountMapPagedQuery` (query string) | `ApiResponse<PagedResult<ControlAccountMapResponse>>` | `200 OK` |
| `GET` | `/control-accounts/coverage` | Menganalisis kelengkapan cakupan akun control untuk 5 kelompok saldo sebelum snapshot bulanan diterbitkan | `FinanceSubledgerSetup : Read` | — | `ApiResponse<ControlAccountCoverageResponse>` | `200 OK` |
| `POST` | `/control-accounts` | Menambahkan satu baris pemetaan kelompok/segmen ke kode akun control COA | `FinanceSubledgerSetup : Create` | `CreateControlAccountMapRequest` (JSON) | `ApiResponse<ControlAccountMapResponse>` | `201 Created`<br>`400 Bad Request`<br>`409 Conflict`<br>`422 Unprocessable` |
| `PUT` | `/control-accounts/{id}` | Memperbarui kode akun control atau catatan pada pemetaan yang ada | `FinanceSubledgerSetup : Update` | `UpdateControlAccountMapRequest` (JSON) | `ApiResponse<ControlAccountMapResponse>` | `200 OK`<br>`400 Bad Request`<br>`404 Not Found`<br>`409 Conflict` |
| `POST` | `/control-accounts/{id}/deactivate` | Menonaktifkan pemetaan akun control secara idempoten; riwayat tetap tersimpan | `FinanceSubledgerSetup : Update` | `DeactivateControlAccountMapRequest` (JSON) | `ApiResponse<ControlAccountMapResponse>` | `200 OK`<br>`404 Not Found` |
| `POST` | `/opening-balances/{id}/approve` | *(Placeholder RBAC)* Mendaftarkan aksi `Approve` pada resource `FinanceSubledgerSetup` untuk persiapan task `BE-FIN-066` | `FinanceSubledgerSetup : Approve` | — | `ApiResponse<object>` | `501 Not Implemented` |

---

## 5. Verifikasi dan Bukti Kepatuhan

1. **Kelengkapan RBAC:**
   Resource `FinanceSubledgerSetup` telah didaftarkan lengkap dengan 4 aksi sah pada controller: `Read`, `Create`, `Update`, dan `Approve` (`FIN-PERM-1.7 G.1`).
2. **Kepatuhan Invariant Bisnis:**
   Konstanta `SubledgerControlAccountDefaults` pada DTO snapshot tetap dipertahankan sebagai nilai referensi bawaan (*seed*), sementara sumber kebenaran operasional kini sepenuhnya berasal dari basis data melalui `FinanceSubledgerControlAccountService` (`FIN-DES-080`).
3. **Kepatuhan Pembatasan Task:**
   - **Nol migrasi baru:** Task ini tidak menambah atau mengubah skema basis data.
   - **Nol build otomatis:** Tidak ada perintah `dotnet build` otomatis yang dijalankan sesuai instruksi pengguna.
