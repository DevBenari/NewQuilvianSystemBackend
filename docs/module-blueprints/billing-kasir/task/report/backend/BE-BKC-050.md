# Laporan Perubahan Backend — `BE-BKC-050`

## Metadata

| Field | Nilai |
| --- | --- |
| **Task ID** | `BE-BKC-050` |
| **Judul** | Lembar Tagihan Penjamin Perusahaan (*Company Guarantor Invoice Document*) |
| **Slice / Milestone** | `MVP-19` (Dokumen Penjamin Perusahaan & Hardening / Eksekusi Gelombang 3) |
| **Roadmap** | `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` baris 1858 |
| **Trace** | `FR-BKC-082`, `FR-BKC-083`, `FR-BKC-084`; `MPY-DEC-006`, `MPY-DES-013`, `MPY-DES-014`; `CAP-38`, `CAP-41` |
| **Contract Version** | `BIL-API-1.0`; UAT `UAT-53` |
| **Dependency** | `BE-BKC-044` (mesin tanggungan perusahaan terbukti menghitung porsi penjamin secara akurat) |
| **Klasifikasi** | `NEW FEATURE / READ-ONLY DOCUMENT SERVICE & ENDPOINT` |
| **Task Mode** | `TASK MODE: BACKEND` |
| **Target Tulis** | `NewQuilvianSystemBackend`, branch `Yasmina` |
| **Model** | Gemini 3.8 Flash |
| **Tanggal** | 11 September 2026 |
| **Status** | ✅ `Selesai` (DTO CompanyGuarantorInvoiceDocumentResponse terpisah, service BillingCompanyGuarantorInvoiceDocumentService baca-saja dari kalkulasi terkunci/segar, endpoint RESTful GET /{id}/company-guarantor-invoice-document berjalan dengan hak akses BillingInvoice:Read, rute reimbursement tampil sebagai keterangan metadata tanpa menggeser debitur, dan validasi jenis penjamin kunjungan terbukti) |

### Backend Governance Preflight

| Parameter | Nilai |
| --- | --- |
| **Area** | `HealthServices` |
| **Module / Submodule** | `BillingManagement / Billing` |
| **Owner / Prefix Registry** | Prefix `BIL-` (`HealthServices / BillingManagement / Billing`) |
| **Keberlakuan** | `NEW ENDPOINT & READ-ONLY SERVICE (STRUCTURED DUPLICATION)` |
| **QBE ID yang Berlaku** | `QBE-MOD-002`, `QBE-CON-001`, `QBE-SVC-001`, `QBE-INT-001` |
| **Pengecualian / Catatan** | Sesuai keputusan konstitusi `MPY-DEC-006` dan `MPY-DES-013`, lembar dokumen penjamin perusahaan dirancang sebagai DTO dan Service terpisah yang meniru pola `InsuranceInvoiceDocument` (*structured duplication*, bukan menambah percabangan ke service asuransi yang sudah ada demi memisahkan domain debitur). Hak akses `BillingInvoice : Read` dipakai ulang (`MPY-DEC-006`). Nomor dokumen memakai nomor tagihan tanpa seri nomor baru (`CAP-41`). |

---

## 1. Masalah & Latar Belakang Bisnis

### 1.1 Kebutuhan Penagihan Terpisah kepada Perusahaan Mitra (`FR-BKC-082`, `MPY-DEC-006`)
Ketika karyawan dari perusahaan yang bermitra dengan rumah sakit (misalnya PT Petrokimia Nusantara atau PT Sejahtera Abadi) berobat, rumah sakit tidak menagihkan biaya pengobatan langsung kepada pasien untuk bagian yang ditanggung perusahaan. Sebagai gantinya, bagian Keuangan/Finance rumah sakit menerbitkan lembar tagihan resmi yang ditujukan kepada manajemen perusahaan penjamin.

Sebelum adanya task ini:
1. Lembar penjamin yang tersedia hanya lembar *Invoice Asuransi* yang dikhususkan bagi perusahaan asuransi (`MstInsuranceProvider`).
2. Kunjungan berpenjamin perusahaan jika dicetak menggunakan template asuransi akan ditolak sistem dengan peringatan: *"Penjamin kunjungan ini adalah perusahaan tempat kerja, bukan perusahaan asuransi."*
3. Staf kasir dan piutang rumah sakit terpaksa membuat rekapitulasi penagihan perusahaan secara manual di luar sistem.

### 1.2 Informasi Identitas Karyawan & Perusahaan Penjamin (`FR-BKC-084`, `MPY-DES-013`)
Lembar tagihan perusahaan wajib memuat informasi komprehensif yang dibutuhkan bagian HR/Finance perusahaan untuk memverifikasi keabsahan klaim pengobatan karyawannya:
- **Identitas Perusahaan**: Nama perusahaan penjamin, kode perusahaan, nomor kontrak kerja sama RS-perusahaan, dan alamat kantor perusahaan.
- **Identitas Karyawan**: Nomor Induk Karyawan (NIK/Employee Number), nama lengkap karyawan, nama paket manfaat (*Benefit Plan*), dan kelas perawatan yang menjadi haknya.
- **Rincian Finansial**: Hanya baris biaya yang ditanggung perusahaan (`CoveredAmount > 0`) yang dimunculkan, lengkap dengan porsi netto tertanggung, PPN tertanggung, serta sisa tanggungan pasien (*excess/co-payment* bila ada).

### 1.3 Rute Reimbursement Sebagai Metadata Murni (`FR-BKC-083`, `MPY-DES-014`)
Beberapa perusahaan menanggung biaya kesehatan karyawannya secara mandiri (*Self-Insured*), sedangkan perusahaan lain memiliki skema kerja sama dengan asuransi mitra (*Third Party Administrator / TPA* atau *Reimbursement Route*).
- **Aturan Tegas `MPY-DES-014`**: Keterangan asuransi mitra pada rute reimbursement **hanya bersifat catatan/metadata informatif** pada lembar tagihan.
- **Debitur Tetap Perusahaan**: Rumah sakit memiliki ikatan perjanjian dengan perusahaan penjamin, sehingga pihak yang ditagih dan dicatat sebagai debitur piutang (*Accounts Receivable*) tetaplah perusahaan penjamin, bukan asuransi mitra.

---

## 2. Arsitektur Solusi & Keputusan Desain

### 2.1 Pola Duplikasi Terstruktur vs Anti-Pattern Multi-Branching (`CAP-38`, `MPY-DES-013`)
Sesuai audit kapabilitas `CAP-38` dan keputusan `MPY-DES-013`:
- **Menolak Penambahan Percabangan**: Menambahkan kondisi `if (payer == CompanyGuarantor)` ke dalam `BillingInsuranceInvoiceDocumentService` akan merusak prinsip *Single Responsibility*. Dokumen asuransi dan dokumen perusahaan melayani pihak eksternal yang berbeda dengan format dan kebutuhan metadata yang berlainan.
- **Duplikasi Terstruktur yang Terisolasi**: Membuat DTO mandiri (`BillingCompanyGuarantorInvoiceDtos.cs`) dan Service mandiri (`BillingCompanyGuarantorInvoiceDocumentService.cs`) yang mengadopsi pola baca-saja dari kalkulasi terkunci/segar. Perubahan tata letak lembar penjamin perusahaan di masa depan tidak akan pernah memicu regresi pada cetakan invoice asuransi.

### 2.2 Penomoran Dokumen (`CAP-41`)
Mengikuti preseden Invoice Asuransi:
- Dokumen tagihan penjamin perusahaan **bukan surat klaim formal baru** dengan seri penomoran tersendiri.
- Kolom `DocumentNumber` diisi sama dengan `InvoiceNumber` tagihan billing. Hal ini memudahkan rekonsiliasi antara pembayaran kasir dan penagihan piutang korporat.

### 2.3 Hak Akses Tanpa Izin Baru (`MPY-DEC-006`)
- Endpoint dokumen penjamin perusahaan memakai ulang hak akses yang sudah ada: `BillingInvoice : Read`.
- Tidak ada penambahan permission baru pada sistem wewenang. Siapa pun petugas administrasi atau kasir yang berwenang membuka tagihan billing otomatis berwenang mencetak dokumen tagihan ini.

---

## 3. Alur Proses Bisnis & Logika Pembentukan Dokumen

### 3.1 Diagram Alur Eksekusi Dokumen Tagihan Perusahaan

```text
[Pengguna Membuka Lembar Tagihan Perusahaan di Layar Billing/Kasir]
                              │
                              ▼
GET /api/v1/health-services/billing-management/billing/invoices/{id}/company-guarantor-invoice-document
(Wewenang: BillingInvoice : Read)
                              │
                              ├── 1. Validasi Keberadaan Invoice
                              │      └── Cari di BilInvoices (AsNoTracking). Jika tidak ada -> 404 Not Found
                              │
                              ├── 2. Penentuan Sumber Kalkulasi (Baca-Saja)
                              │      ├── Jika Invoice Status == OPEN:
                              │      │     Hitung angka segar via BillingCalculationService.PreviewCalculationAsync
                              │      └── Jika Invoice Status != OPEN (FINAL / CLOSED / SETTLED):
                              │            Ambil snapshot terkunci dari BilCalculationVersions (Versi == CurrentCalculationVersion)
                              │
                              ├── 3. Pemeriksaan Jenis Penjamin Kunjungan (RegPatientEncounterGuarantors)
                              │      ├── Jenis CASH:
                              │      │     IsPrintable = false
                              │      │     Warnings = ["Kunjungan ini dibayar mandiri, sehingga tidak ada Lembar Tagihan Perusahaan yang dapat diterbitkan."]
                              │      ├── Jenis INSURANCE:
                              │      │     IsPrintable = false
                              │      │     Warnings = ["Penjamin kunjungan ini adalah perusahaan asuransi, bukan perusahaan tempat kerja. Gunakan dokumen Invoice Asuransi."]
                              │      ├── Jenis UNKNOWN:
                              │      │     IsPrintable = false
                              │      │     Warnings = ["Sumber pembayaran kunjungan ini belum tercatat. Lengkapi data penjamin di Registrasi terlebih dahulu."]
                              │      └── Jenis COMPANY_GUARANTOR:
                              │            ├── Muat Data Pasien (MstPatients, MstRooms, MstServiceUnits)
                              │            ├── Muat Data Perusahaan (MstCompanyGuarantors & snapshot identitas karyawan)
                              │            └── Muat Metadata Rute Reimbursement (MstCompanyGuarantorReimbursementRoutes)
                              │
                              ├── 4. Penyaringan Baris Tertanggung (Server-Side Filtering)
                              │      ├── Saring item dengan (CoveredNetAmount + CoveredTaxAmount) > 0
                              │      ├── Sertakan Biaya Administrasi jika PrimaryAmount > 0
                              │      ├── Sertakan Biaya Kamar jika PrimaryAmount > 0
                              │      └── Jika tidak ada item tertanggung -> Warnings += "Tidak ada item yang ditanggung penjamin perusahaan pada tagihan ini."
                              │
                              ├── 5. Agregasi Total Finansial
                              │      ├── EligibleAmount, TotalCoveredAmount (PrimaryAmount), ExcessAmount, PatientAmount
                              │      └── IsPrintable = (PayerKind == COMPANY_GUARANTOR && Payer != null && Items.Count > 0)
                              │
                              └── 6. Kembalikan Response HTTP 200 OK
                                     └── ApiResponse<CompanyGuarantorInvoiceDocumentResponse>
```

---

## 4. Contoh Konkret Skenario Rumah Sakit

### Skenario 1: Kunjungan Karyawan PT Sejahtera dengan Rute Asuransi Mitra (`UAT-53`)
- **Konteks**: Tn. Budi, staf dari PT Sejahtera Abadi (perusahaan penjamin korporat), menjalani perawatan rawat jalan di Poliklinik Penyakit Dalam. Total tagihan rumah sakit adalah Rp 1.250.000 (Konsultasi Spesialis Rp 250.000, Laboratorium Darah Lengkap Rp 400.000, Obat Resep Rp 600.000). Sesuai aturan tanggungan perusahaan, 100% biaya konsultasi dan laboratorium ditanggung perusahaan, sedangkan obat ditanggung Rp 500.000 (pasien membayar selisih Rp 100.000 di kasir). PT Sejahtera memiliki rute reimbursement lewat *Asuransi Mitra Sehat*.
- **Tindakan Staf Kasir/Finance**: Membuka dokumen tagihan penjamin perusahaan pada tagihan Tn. Budi.
- **Hasil Sistem**:
  - `PayerKind`: `"COMPANY_GUARANTOR"`.
  - `Payer`: Menampilkan Nama Perusahaan: `"PT Sejahtera Abadi"`, Kode: `"CORP-SEJ-001"`, Nomor Kontrak: `"KTR-2026-088"`, NIK Karyawan: `"EMP-99214"`, Nama Karyawan: `"Budi Santoso"`, Benefit Plan: `"Gold Corporate Plan"`.
  - `ReimbursementRoute`: `RouteType = "INSURANCE_PROVIDER"`, `InsuranceProviderName = "Asuransi Mitra Sehat"`, `Description = "Penggantian biaya lewat asuransi mitra: Asuransi Mitra Sehat"`.
  - `Debitur`: Lembar tagihan secara tegas berkepala PT Sejahtera Abadi (bukan Asuransi Mitra Sehat).
  - `Items`: Hanya menampilkan 3 baris item yang memiliki nilai pertanggungan.
  - `Totals`: `TotalCoveredAmount = Rp 1.150.000`, `PatientAmount = Rp 100.000`.
  - `IsPrintable = true`, `Warnings = []`.

### Skenario 2: Kunjungan Karyawan PT Mandiri Prima Menanggung Sendiri (*Self-Insured*)
- **Konteks**: Ny. Siti berobat dengan penjamin PT Mandiri Prima. PT Mandiri Prima tidak bekerja sama dengan asuransi mitra mana pun (`RouteType = "SELF"`).
- **Hasil Sistem**:
  - `ReimbursementRoute`: `RouteType = "SELF"`, `Description = "Menanggung sendiri (Tanpa asuransi mitra)"`.
  - Lembar tagihan terbit dan dapat dicetak dengan sukses.

### Skenario 3: Penolakan Penerbitan untuk Pasien Tunai / Asuransi Pribadi (`FR-BKC-084`)
- **Kasus A (Pasien Tunai)**: Kasir membuka tagihan pasien umum mandiri (Cash). Sistem mengembalikan HTTP 200 dengan `IsPrintable = false` dan pesan peringatan: *"Kunjungan ini dibayar mandiri, sehingga tidak ada Lembar Tagihan Perusahaan yang dapat diterbitkan."* Tombol cetak pada antarmuka kasir dinonaktifkan.
- **Kasus B (Pasien Asuransi Pribadi)**: Kasir membuka tagihan pasien pemegang asuransi Prudential. Sistem mengembalikan HTTP 200 dengan `IsPrintable = false` dan pesan peringatan: *"Penjamin kunjungan ini adalah perusahaan asuransi, bukan perusahaan tempat kerja. Gunakan dokumen Invoice Asuransi."*

---

## 5. Spesifikasi Teknis & Berkas yang Dibuat / Dimodifikasi

### 5.1 Berkas yang Dibuat (*New Files*)

1. **`Areas/HealthServices/BillingManagement/Billing/Dtos/BillingCompanyGuarantorInvoiceDtos.cs`**
   - Mendefinisikan DTO respon dokumen:
     - `CompanyGuarantorInvoiceDocumentResponse`
     - `CompanyGuarantorInvoicePatientResponse`
     - `CompanyGuarantorInvoicePayerResponse`
     - `CompanyGuarantorInvoiceReimbursementRouteResponse`
     - `CompanyGuarantorInvoiceItemResponse`
     - `CompanyGuarantorInvoiceTotalResponse`
     - Konstanta `CompanyGuarantorInvoicePayerKinds` dan `CompanyGuarantorInvoiceItemKinds`.

2. **`Areas/HealthServices/BillingManagement/Billing/Services/BillingCompanyGuarantorInvoiceDocumentService.cs`**
   - Layanan dokumen murni baca (`AsNoTracking`).
   - Mengambil kalkulasi mutakhir (segar untuk tagihan `OPEN`, snapshot tersimpan untuk tagihan non-`OPEN`).
   - Menyaring item tertanggung penjamin perusahaan secara *server-side* (`CoveredAmount > 0`).
   - Mengisi identitas perusahaan dan karyawan dari master dan snapshot registrasi.
   - Membaca konfigurasi rute reimbursement dari `MstCompanyGuarantorReimbursementRoute`.

### 5.2 Berkas yang Dimodifikasi (*Modified Files*)

1. **`Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs`**
   - Mendaftarkan dependensi layanan:
     ```csharp
     services.AddScoped<BillingCompanyGuarantorInvoiceDocumentService>();
     ```

2. **`Areas/HealthServices/BillingManagement/Billing/Controllers/BillingInvoicesController.cs`**
   - Menyuntikkan `BillingCompanyGuarantorInvoiceDocumentService` ke constructor controller.
   - Menambahkan endpoint RESTful:
     `GET api/v1/health-services/billing-management/billing/invoices/{id}/company-guarantor-invoice-document`
     dengan hak akses `[AccessPermission("BillingInvoice", "Read")]` dan `SortOrder = 23`.

---

## 6. Spesifikasi Endpoint Bergaya Swagger (OpenAPI 3.0)

### Tag: `Health Services / Billing Management / Billing / Invoices`

```yaml
openapi: 3.0.3
info:
  title: Quilvian Billing System - Company Guarantor Invoice Document API
  version: 1.0.0
paths:
  /api/v1/health-services/billing-management/billing/invoices/{id}/company-guarantor-invoice-document:
    get:
      tags:
        - Health Services / Billing Management / Billing / Invoices
      summary: Menyusun lembar tagihan resmi yang ditujukan kepada perusahaan penjamin (Company Guarantor)
      description: |
        Membaca data kalkulasi tagihan terkunci atau kalkulasi aktif untuk menyusun lembar penagihan ke perusahaan
        penjamin tempat pasien bekerja. Memuat identitas perusahaan, identitas karyawan, rute reimbursement
        (sebagai metadata informatif), dan daftar item yang ditanggung perusahaan.
        
        Kunjungan tunai atau asuransi pribadi akan mengembalikan status 200 dengan isPrintable = false beserta
        penjelasan pada daftar warnings.
      operationId: GetCompanyGuarantorInvoiceDocument
      parameters:
        - name: id
          in: path
          required: true
          description: ID unik invoice billing
          schema:
            type: string
            format: uuid
      security:
        - BearerAuth: []
      responses:
        '200':
          description: Dokumen tagihan penjamin perusahaan berhasil disusun.
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/CompanyGuarantorInvoiceDocumentApiResponse'
        '404':
          description: Invoice Billing atau versi kalkulasi tidak ditemukan.
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/ErrorApiResponse'
        '422':
          description: Terjadi kesalahan validasi kalkulasi tagihan.
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/ErrorApiResponse'

components:
  schemas:
    CompanyGuarantorInvoiceDocumentApiResponse:
      type: object
      properties:
        success:
          type: boolean
          example: true
        message:
          type: string
          example: Dokumen Tagihan Penjamin Perusahaan berhasil disusun.
        statusCode:
          type: integer
          example: 200
        data:
          $ref: '#/components/schemas/CompanyGuarantorInvoiceDocumentResponse'
    CompanyGuarantorInvoiceDocumentResponse:
      type: object
      properties:
        invoiceId:
          type: string
          format: uuid
        documentNumber:
          type: string
          example: INV-20260911-0001
        invoiceNumber:
          type: string
          example: INV-20260911-0001
        invoiceStatus:
          type: string
          example: OPEN
        serviceType:
          type: string
          example: RAJAL
        invoiceDate:
          type: string
          format: date-time
        payerKind:
          type: string
          enum: [COMPANY_GUARANTOR, INSURANCE, CASH, UNKNOWN]
          example: COMPANY_GUARANTOR
        isPrintable:
          type: boolean
          example: true
        isFromLockedSnapshot:
          type: boolean
          example: false
        isPerItemBreakdownAvailable:
          type: boolean
          example: true
        calculationVersionNo:
          type: integer
          example: 3
        patient:
          type: object
          properties:
            medicalRecordNumber:
              type: string
              example: RM-001234
            fullName:
              type: string
              example: Budi Santoso
            encounterNumber:
              type: string
              example: ENC-20260911-0042
        payer:
          type: object
          properties:
            companyGuarantorId:
              type: string
              format: uuid
            companyGuarantorCode:
              type: string
              example: CORP-SEJ-001
            companyGuarantorName:
              type: string
              example: PT Sejahtera Abadi
            contractNumber:
              type: string
              example: KTR-2026-088
            employeeNumber:
              type: string
              example: EMP-99214
            employeeName:
              type: string
              example: Budi Santoso
            planName:
              type: string
              example: Gold Corporate Plan
        reimbursementRoute:
          type: object
          properties:
            routeType:
              type: string
              example: INSURANCE_PROVIDER
            insuranceProviderName:
              type: string
              example: Asuransi Mitra Sehat
            description:
              type: string
              example: Penggantian biaya lewat asuransi mitra: Asuransi Mitra Sehat
        items:
          type: array
          items:
            type: object
            properties:
              description:
                type: string
                example: Konsultasi Dokter Spesialis
              quantity:
                type: number
                example: 1
              unitPrice:
                type: number
                example: 250000
              coveredAmount:
                type: number
                example: 250000
        totals:
          type: object
          properties:
            totalCoveredAmount:
              type: number
              example: 1150000
            patientAmount:
              type: number
              example: 100000
        warnings:
          type: array
          items:
            type: string
```

---

## 7. Kesimpulan & Langkah Selanjutnya

- Seluruh kriteria penerimaan untuk `BE-BKC-050` (`FR-BKC-082`, `FR-BKC-083`, `FR-BKC-084`, `UAT-53`, `CAP-38`, `CAP-41`) telah terpenuhi dengan baik.
- Penjamin perusahaan kini memiliki lembar tagihan resmi terpisah yang rapi, profesional, dan siap dicetak/diunduh oleh bagian Keuangan Rumah Sakit.
- Siap melanjutkan ke task berikutnya: **`BE-BKC-051`** (*Hak akses, privasi, dan hardening lintas-slice* / Gelombang 4 eksekusi).
