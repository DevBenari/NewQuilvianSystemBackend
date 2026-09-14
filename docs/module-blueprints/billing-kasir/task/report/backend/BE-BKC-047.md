# Laporan Perubahan Backend — `BE-BKC-047`

## Metadata

| Field | Nilai |
| --- | --- |
| **Task ID** | `BE-BKC-047` |
| **Judul** | Konteks Layar Edit, Pratinjau Perbandingan, dan Perintah Ganti Payer (*Edit Screen Context, Comparison Preview, and Switch Payment Source Command*) |
| **Slice / Milestone** | `MVP-17` (Penggantian penanggung kunjungan sebelum pembayaran / Ganti Payer) |
| **Roadmap** | `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` baris 1806 |
| **Trace** | `FR-BKC-068`, `FR-BKC-069`, `FR-BKC-070`, `FR-BKC-071`, `FR-BKC-072`, `FR-BKC-073`, `FR-BKC-085`, `FR-BKC-086`; `MPY-DES-001`, `MPY-DES-003`, `MPY-DES-009`, `MPY-DES-015`, `MPY-DES-016` |
| **Contract Version** | `BIL-API-1.0`; `BIL-VAL-059`–`BIL-VAL-074` |
| **Dependency** | `BE-BKC-044` (mesin tanggungan perusahaan), `BE-BKC-045` (kontrak serah terima diserahkan), `BE-BKC-046` (konteks payer eksplisit untuk pratinjau) |
| **Gerbang Eksternal** | `EncounterPaymentSourceService` di `RegistrationManagement` (diimplementasikan penuh memenuhi `MPY-ENC-PAYER-001` tanpa migrasi pendaftaran baru) |
| **Klasifikasi** | `NEW FEATURE / ORCHESTRATION & API` |
| **Task Mode** | `TASK MODE: BACKEND` |
| **Target Tulis** | `NewQuilvianSystemBackend`, branch `Yasmina` |
| **Model** | Gemini 3.8 Flash |
| **Tanggal** | 11 September 2026 |
| **Status** | ✅ `Selesai` (Tiga endpoint RESTful berjalan, orkestrator transaksi serializable atomik memegang seluruh aturan `BIL-VAL-059`–`074`, reset otomatis baris biaya terhitung akurat, dan jejak audit tersimpan tak terhapus) |

### Backend Governance Preflight

| Parameter | Nilai |
| --- | --- |
| **Area** | `HealthServices` |
| **Module / Submodule** | `BillingManagement / Billing` & `RegistrationManagement` |
| **Owner / Prefix Registry** | Prefix `BIL-` (`HealthServices / BillingManagement / Billing`) & `REG-` (`HealthServices / RegistrationManagement`) |
| **Keberlakuan** | `NEW ORCHESTRATOR & NEW ENDPOINTS` |
| **QBE ID yang Berlaku** | `QBE-MOD-002`, `QBE-CON-001`, `QBE-INT-001`, `QBE-SVC-001` |
| **Pengecualian / Catatan** | Seluruh mutasi ganti payer dikunci dalam batas transaksi serializable pada `BillingPayerEditService` (`MPY-DES-016`). Controller tipis dan tidak mengakses `ApplicationDbContext` langsung. Layanan eksternal `EncounterPaymentSourceService` berpartisipasi 100% pada transaksi pemanggil tanpa membuka transaksi mandiri. |

---

## 1. Masalah & Latar Belakang Bisnis

### 1.1 Kebutuhan Satu Panggilan untuk Layar Edit Tagihan (`MPY-DES-015`)
Sebelumnya, ketika kasir membuka layar Edit Tagihan di antarmuka web, kasir harus memanggil berbagai endpoint secara terpisah untuk mengetahui:
1. Informasi umum tagihan dan data pasien.
2. Hasil perhitungan tagihan terkini (porsi penjamin vs porsi pasien).
3. Payer kunjungan yang sedang aktif saat ini.
4. Kartu asuransi dan kartu penjamin perusahaan milik pasien yang terdaftar di RS.
5. Penanggung per baris biaya tagihan.
6. Disposisi penebusan obat (apakah masuk tagihan atau tidak).
7. Hak dan kapabilitas kasir saat ini (apakah tagihan masih boleh diedit, atau sudah difinalisasi/dibayar).

Memanggil 6–7 endpoint terpisah berisiko menampilkan data inkonsisten (*race condition*) bila tagihan sedang berjalan. Melalui pola *read model gabungan* `GET /{id}/edit-context` (`MPY-DES-015`), seluruh bahan layar disajikan dalam satu panggilan konsisten.

### 1.2 Kebutuhan Pratinjau Perbandingan Payer Tanpa Efek Samping (`FR-BKC-070`, `MPY-DES-005`)
Kasir dan pasien memerlukan simulasi perbandingan sebelum memutuskan mengganti penanggung. Pasien sering bertanya: *"Berapa yang dicover jika saya ganti dari Tunai ke Asuransi Prudential dibandingkan jika memakai penjamin kantor PT Telkom?"*.
Sistem menyediakan `POST /{id}/payer-comparison-preview` yang mengevaluasi mesin tanggungan dengan `CandidatePayerContext` tanpa menulis apa pun ke database (*zero writes, zero side-effects*).

### 1.3 Kebutuhan Eksekusi Ganti Payer yang Bersifat Sekaligus atau Tidak Sama Sekali (`FR-BKC-086`, `MPY-DES-016`)
Saat kasir menekan tombol "Simpan Perubahan Penjamin":
- Sumber pembayaran kunjungan pendaftaran (`RegPatientEncounterGuarantor`) harus diperbarui secara in-place.
- Baris biaya yang sebelumnya ditandai dengan penanggung yang sudah tidak lagi sah (misalnya baris berstatus `INSURANCE` padahal kunjungan diganti menjadi `CASH`) harus **direset otomatis** menjadi tanggungan pasien (`CASH`, `AUTO`) beserta alasannya (`MPY-DES-009`, `FR-BKC-072`).
- Tagihan harus dihitung ulang secara menyeluruh (`BillingCalculationService.RecalculateAsync`) melahirkan versi kalkulasi baru dan *row version* baru.
- Jejak perintah harus dicatat pada tabel audit permanen `BilInvoicePayerChangeCommand` (`MPY-DES-003`, `FR-BKC-073`).
- Jika salah satu tahap gagal (misal: versi baris sudah basi karena ada kasir lain yang mengubah tagihan, atau kartu asuransi kedaluwarsa), **seluruh langkah harus dibatalkan (rollback)**.

---

## 2. Alur Proses Bisnis & Matriks Validasi

### 2.1 Alur Penggantian Penanggung Tagihan

```text
[Kasir Membuka Layar Edit Tagihan]
        │
        ▼
   GET /{id}/edit-context
        │  (Mengembalikan Invoice, Calculation, CurrentPayer, AvailablePayerOptions, Capabilities)
        ▼
[Kasir Memilih Payer Kandidat & Menekan 'Bandingkan']
        │
        ▼
   POST /{id}/payer-comparison-preview
        │  (Evaluasi 100% read-only tanpa menyimpan data; menampilkan tabel perbandingan berdampingan)
        ▼
[Kasir Memasukkan Alasan & Menekan 'Simpan Perubahan']
        │
        ▼
   PUT /{id}/payment-source (Header: Idempotency-Key)
        │
        ├── 1. Kunci Transaksi Serializable & pg_advisory_xact_lock (BIL_CALCULATION, BIL_ENCOUNTER)
        ├── 2. Cek Idempotency Replay (BilInvoicePayerChangeCommand)
        ├── 3. Validasi Gerbang Tagihan (BIL-VAL-059: OPEN, BIL-VAL-060: Nol bayar, BIL-VAL-061: RowVersion)
        ├── 4. Validasi Kelayakan Payer & Kartu (BIL-VAL-064 s/d 074)
        ├── 5. Perbarui RegPatientEncounterGuarantor in-place via EncounterPaymentSourceService
        ├── 6. Reset otomatis BilInvoiceItemPayerAssignment yang obsolete (menjadi CASH/AUTO)
        ├── 7. Panggil BillingCalculationService.RecalculateAsync (versi baru lahir, row version berubah)
        ├── 8. Simpan entri jejak BilInvoicePayerChangeCommand
        └── 9. Commit transaksi dan kembalikan InvoiceEditResultResponse (Calculation, RowVersion, Warnings)
```

### 2.2 Matriks Aturan Bisnis (`BIL-VAL-059`–`BIL-VAL-074`)

| Aturan | Kondisi | Pesan bagi Pengguna | HTTP Code | Status Implementasi |
| :--- | :--- | :--- | :---: | :---: |
| `BIL-VAL-059` | Tagihan tidak berstatus `OPEN` | "Tagihan ini sudah difinalisasi sehingga tidak dapat diubah lagi." | `409 Conflict` | ✅ Terpenuhi |
| `BIL-VAL-060` | Sudah ada pembayaran berhasil pada tagihan ini | "Tagihan ini sudah menerima pembayaran. Perubahan penjamin atau penanggung memerlukan proses pembalikan, bukan pengeditan." | `409 Conflict` | ✅ Terpenuhi |
| `BIL-VAL-061` | Versi baris tagihan (`ExpectedRowVersion`) berbeda dari tersimpan | "Data tagihan telah berubah sejak layar ini dibuka. Muat ulang data sebelum menyimpan kembali." | `409 Conflict` | ✅ Terpenuhi |
| `BIL-VAL-062` | Alasan kosong | "Alasan perubahan wajib diisi." | `400 Bad Request` | ✅ Terpenuhi |
| `BIL-VAL-063` | `Idempotency-Key` header tidak dikirim | "Permintaan tidak lengkap. Muat ulang halaman lalu coba lagi." | `400 Bad Request` | ✅ Terpenuhi |
| `BIL-VAL-064` | `paymentType` di luar `CASH`/`INSURANCE`/`COMPANY_GUARANTOR` | "Jenis pembayaran tidak dikenali." | `400 Bad Request` | ✅ Terpenuhi |
| `BIL-VAL-065` | `paymentType = CASH` tetapi menyertakan id kartu penjamin | "Pembayaran tunai tidak boleh disertai kartu penjamin." | `400 Bad Request` | ✅ Terpenuhi |
| `BIL-VAL-066` | `paymentType = INSURANCE` tetapi id kartu asuransi tidak dikirim | "Pilih kartu asuransi yang akan dipakai." | `400 Bad Request` | ✅ Terpenuhi |
| `BIL-VAL-067` | `paymentType = COMPANY_GUARANTOR` tetapi id kartu penjamin perusahaan tidak dikirim | "Pilih kartu penjamin perusahaan yang akan dipakai." | `400 Bad Request` | ✅ Terpenuhi |
| `BIL-VAL-068` | Kartu yang dipilih bukan milik pasien kunjungan ini | "Kartu penjamin yang dipilih bukan milik pasien ini." | `422 Unprocessable` | ✅ Terpenuhi |
| `BIL-VAL-069` | Kartu yang dipilih sudah tidak aktif atau terhapus | "Kartu penjamin yang dipilih sudah tidak berlaku. Perbarui data penjamin pasien di Registrasi." | `422 Unprocessable` | ✅ Terpenuhi |
| `BIL-VAL-070` | Tanggal layanan berada di luar masa berlaku kartu | "Kartu penjamin ini tidak berlaku pada tanggal pelayanan. Pilih kartu lain atau perbarui masa berlakunya di Registrasi." | `422 Unprocessable` | ✅ Terpenuhi |
| `BIL-VAL-071` | Kartu belum dinyatakan layak dipakai (`IsEligible = false`) | "Kartu penjamin ini belum dinyatakan layak. Periksa kelayakannya di Registrasi sebelum dipakai." | `422 Unprocessable` | ✅ Terpenuhi |
| `BIL-VAL-072` | Provider asuransi atau penjamin perusahaan tidak aktif / kontrak berakhir | "Kerja sama dengan perusahaan asuransi ini sudah berakhir pada tanggal pelayanan." | `422 Unprocessable` | ✅ Terpenuhi |
| `BIL-VAL-073` | Payer kandidat sama persis dengan payer yang sedang berlaku | "Penjamin yang dipilih sama dengan yang sedang dipakai. Tidak ada yang perlu diubah." | `422 Unprocessable` | ✅ Terpenuhi |
| `BIL-VAL-074` | Kunjungan tidak memiliki baris sumber pembayaran sama sekali | "Data penjamin kunjungan ini belum lengkap. Hubungi Registrasi sebelum mengubah tagihan." | `422 Unprocessable` | ✅ Terpenuhi |

### 2.3 Skenario Nyata Rumah Sakit (Data Samaran)

1. **Skenario Pasien Ny. S (Ganti Tunai ke Asuransi Prudential):**
   - Pasien didaftarkan Tunai dengan tagihan rawat jalan Rp 750.000 (konsultasi spesialis Rp 250.000, lab darah Rp 300.000, obat Rp 200.000).
   - Kasir membuka `GET /{id}/edit-context`. Kasir melihat `capabilities.canEditPaymentSource = true` dan kartu Prudential Ny. S aktif di `availablePayerOptions`.
   - Kasir menekan Bandingkan (`POST /{id}/payer-comparison-preview`). Sistem menampilkan pratinjau: Porsi Prudential menanggung Rp 650.000 (konsultasi & lab tertanggung 100%, obat tertanggung 50%), porsi mandiri pasien sisa Rp 100.000.
   - Kasir memasukkan alasan: *"Pasien menyerahkan kartu asuransi sebelum pembayaran"* dan menekan Simpan.
   - Sistem memanggil `PUT /{id}/payment-source`. Database memperbarui penjamin encounter menjadi Prudential, menghitung ulang invoice melahirkan versi kalkulasi 2, dan tagihan yang harus dibayar pasien berkurang menjadi Rp 100.000.
2. **Skenario Pasien Tn. B (Ganti Penjamin Perusahaan ke Tunai dengan Reset Baris):**
   - Pasien berpenjamin PT Maju Mundur. Kasir sebelumnya menandai 2 item vitamin khusus sebagai tanggungan penjamin perusahaan (`BilInvoiceItemPayerAssignment` `PayerKind = COMPANY_GUARANTOR`).
   - Karena persetujuan jaminan dibatalkan oleh HRD perusahaan, pasien bersedia membayar pribadi secara Tunai.
   - Kasir mengeksekusi ganti payer ke `CASH`.
   - Di dalam transaksi atomik, `BillingPayerEditService` mendeteksi 2 baris item yang menunjuk `COMPANY_GUARANTOR`. Karena payer kunjungan kini Tunai, penanggung kedua baris tersebut di-nonaktifkan dan diganti dengan baris baru `PayerKind = CASH`, `AssignmentSource = AUTO`, `Reason = "Direset otomatis menjadi Pribadi..."`.
   - Response mengembalikan `resetAssignmentCount = 2` dan `warnings = ["2 baris biaya dikembalikan menjadi tanggungan pasien karena penanggung sebelumnya tidak lagi berlaku."]`.

---

## 3. Spesifikasi Endpoint Bergaya Swagger

Tag Grup: `[Tags("Health Services / Billing Management / Billing / Invoices")]`  
Base URL: `api/v1/health-services/billing-management/billing/invoices`

| Method | Path | Deskripsi | Hak Akses | Request Body | Response Body |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/{id}/edit-context` | Memuat seluruh bahan layar Edit Tagihan dalam satu panggilan gabungan: header, kalkulasi terkini, payer aktif, pilihan kartu terdaftar, penanggung per baris, disposisi obat, dan kapabilitas edit | `BillingInvoice:Read` | — | `ApiResponse<InvoiceEditContextResponse>` |
| `POST` | `/{id}/payer-comparison-preview` | Menghitung pratinjau perbandingan perhitungan antara payer aktif dengan payer kandidat tanpa efek samping apa pun (*zero writes*) | `BillingInvoice:Read` | `PayerComparisonPreviewRequest` | `ApiResponse<PayerComparisonPreviewResponse>` |
| `PUT` | `/{id}/payment-source` | Mengganti penanggung kunjungan yang berlaku secara atomik di dalam transaksi serializable, mereset penanggung baris obsolete, dan menghitung ulang tagihan | `BillingInvoice:Update` | `SwitchPaymentSourceRequest` *(Header wajib: `Idempotency-Key`)* | `ApiResponse<InvoiceEditResultResponse>` |

---

## 4. Rincian Komponen & Struktur Kode

### 4.1 Layanan Domain Pendaftaran: `EncounterPaymentSourceService.cs`
- **Lokasi:** `Areas/HealthServices/RegistrationManagement/Services/EncounterPaymentSourceService.cs`
- **Peran:** Menyelesaikan prasyarat gerbang eksternal `MPY-ENC-PAYER-001`. Melakukan pembaruan in-place pada `RegPatientEncounterGuarantor` tanpa menambah baris baru (sehingga tidak melanggar filtered unique index), menyinkronkan ringkasan pada `RegPatientEncounter`, dan 100% berpartisipasi pada transaksi pemanggil tanpa membuka/menutup transaksi sendiri.

### 4.2 DTO Baru: `BillingPayerEditDtos.cs`
- **Lokasi:** `Areas/HealthServices/BillingManagement/Billing/Dtos/BillingPayerEditDtos.cs`
- **Isi DTO:**
  - `PayerComparisonPreviewRequest`
  - `SwitchPaymentSourceRequest`
  - `InvoiceEditContextResponse` (Header, Calculation, CurrentPayer, AvailablePayerOptions, ItemPayerAssignments, DrugBillingDisposition, EligibleDrugInvoiceItemIds, Capabilities)
  - `InvoiceEditHeaderResponse`
  - `CurrentPayerResponse`
  - `AvailablePayerOptionResponse`
  - `ItemPayerAssignmentResponse`
  - `DrugBillingDispositionItemResponse`
  - `InvoiceEditCapabilitiesResponse`
  - `PayerComparisonPreviewResponse`
  - `PayerComparisonSummaryResponse`
  - `PayerComparisonItemResponse`
  - `InvoiceEditResultResponse`

### 4.3 Layanan Orkestrator: `BillingPayerEditService.cs`
- **Lokasi:** `Areas/HealthServices/BillingManagement/Billing/Services/BillingPayerEditService.cs`
- **Peran:** Orkestrator transaksi finansial serializable (`MPY-DES-016`).
  - Mengimplementasikan `GetEditContextAsync`: pemanggilan read-only gabungan.
  - Mengimplementasikan `PreviewPayerComparisonAsync`: evaluasi murni membaca data tanpa efek samping.
  - Mengimplementasikan `SwitchPaymentSourceAsync`: orkestrasi atomik yang mengunci `pg_advisory_xact_lock`, memvalidasi `BIL-VAL-059`–`074`, memanggil `EncounterPaymentSourceService`, mereset penanggung per baris biaya yang obsolete, memicu `BillingCalculationService.RecalculateAsync`, dan mencatat jejak tak terhapus ke `BilInvoicePayerChangeCommand`.

### 4.4 Controller: `BillingInvoicesController.cs`
- **Lokasi:** `Areas/HealthServices/BillingManagement/Billing/Controllers/BillingInvoicesController.cs`
- **Peran:** Menghubungkan HTTP request ke `BillingPayerEditService`, memetakan kode status HTTP (`200`, `400`, `404`, `409`, `422`), dan mengekstrak `CurrentUserId()` serta header `Idempotency-Key`.

### 4.5 Registrasi Dependency Injection
- `Program.cs`: Mendaftarkan `EncounterPaymentSourceService` pada container DI:
  `builder.Services.AddScoped<EncounterPaymentSourceService>();`
- `BillingManagementServiceCollectionExtensions.cs`: Mendaftarkan `BillingPayerEditService`:
  `services.AddScoped<BillingPayerEditService>();`

---

## 5. Ringkasan Status Git (`git status --short`)

```text
 M Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs
 M Areas/HealthServices/BillingManagement/Billing/Controllers/BillingInvoicesController.cs
 M Areas/HealthServices/BillingManagement/Billing/Dtos/BillingInvoiceDtos.cs
 M Areas/HealthServices/BillingManagement/Billing/Services/BillingCalculationService.cs
 M Areas/HealthServices/BillingManagement/Billing/Services/BillingCoverageAdapter.cs
 M Areas/HealthServices/ClinicalManagement/Services/EncounterInsuranceService.cs
 M Areas/HealthServices/ClinicalManagement/Services/InsuranceCoverageService.cs
 M Program.cs
 M docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md
 M docs/module-blueprints/billing-kasir/roadmap/requirement-traceability.md
 M docs/module-blueprints/billing-kasir/task/report/backend/BE-BKC-041.md
?? Areas/Administrator/MasterData/Controllers/CompanyGuarantorReimbursementRouteController.cs
?? Areas/Administrator/MasterData/DTOs/CompanyGuarantorReimbursementRouteDtos.cs
?? Areas/Administrator/MasterData/Services/
?? Areas/HealthServices/BillingManagement/Billing/Dtos/BillingPayerEditDtos.cs
?? Areas/HealthServices/BillingManagement/Billing/Services/BillingPayerEditService.cs
?? Areas/HealthServices/ClinicalManagement/Services/CompanyGuarantorCoverageService.cs
?? Areas/HealthServices/MasterData/Controllers/CompanyGuarantorCoverageRuleController.cs
?? Areas/HealthServices/MasterData/DTOs/CompanyGuarantorCoverageRuleDtos.cs
?? Areas/HealthServices/MasterData/Services/CompanyGuarantorCoverageRuleService.cs
?? Areas/HealthServices/RegistrationManagement/Services/EncounterPaymentSourceService.cs
?? docs/module-blueprints/billing-kasir/task/report/backend/BE-BKC-042.md
?? docs/module-blueprints/billing-kasir/task/report/backend/BE-BKC-043.md
?? docs/module-blueprints/billing-kasir/task/report/backend/BE-BKC-044.md
?? docs/module-blueprints/billing-kasir/task/report/backend/BE-BKC-045.md
?? docs/module-blueprints/billing-kasir/task/report/backend/BE-BKC-046.md
?? docs/module-blueprints/billing-kasir/task/report/backend/BE-BKC-047.md
```
