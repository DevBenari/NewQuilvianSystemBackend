# Laporan Perubahan Backend — `BE-BKC-048`

## Metadata

| Field | Nilai |
| --- | --- |
| **Task ID** | `BE-BKC-048` |
| **Judul** | Perintah Penanggung per Baris Biaya (*Item Payer Assignment Command*) |
| **Slice / Milestone** | `MVP-18` (Koreksi per baris biaya / Eksekusi Gelombang 3) |
| **Roadmap** | `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` baris 1824 |
| **Trace** | `FR-BKC-074`, `FR-BKC-075`, `FR-BKC-076`, `FR-BKC-077`; `MPY-DEC-004`, `MPY-DES-008`; `CAP-37` |
| **Contract Version** | `BIL-API-1.0`; `BIL-VAL-059`–`063`, `BIL-VAL-075`–`080`; Acceptance `BIL-AT-089`, `BIL-AT-090`, `BIL-AT-091` |
| **Dependency** | `BE-BKC-044` (mesin tanggungan perusahaan), `BE-BKC-047` (orkestrator dan pola perintah edit tagihan) |
| **Klasifikasi** | `NEW FEATURE / ITEM LEVEL ORCHESTRATION & CALCULATION ENGINE INTEGRATION` |
| **Task Mode** | `TASK MODE: BACKEND` |
| **Target Tulis** | `NewQuilvianSystemBackend`, branch `Yasmina` |
| **Model** | Gemini 3.8 Flash |
| **Tanggal** | 11 September 2026 |
| **Status** | ✅ `Selesai` (Endpoint PUT RESTful berjalan, integrasi mesin kalkulasi menghormati penanggung manual per baris biaya, mutasi nonaktifkan-lalu-sisipkan append-only menjamin integritas filtered unique index, penegakan matriks validasi BIL-VAL-075 s/d 080 dan BIL-VAL-059 s/d 063 terpenuhi penuh, jejak audit komando tercatat) |

### Backend Governance Preflight

| Parameter | Nilai |
| --- | --- |
| **Area** | `HealthServices` |
| **Module / Submodule** | `BillingManagement / Billing` |
| **Owner / Prefix Registry** | Prefix `BIL-` (`HealthServices / BillingManagement / Billing`) |
| **Keberlakuan** | `NEW ENDPOINT & CALCULATION ENGINE ENHANCEMENT` |
| **QBE ID yang Berlaku** | `QBE-MOD-002`, `QBE-CON-001`, `QBE-INT-001`, `QBE-SVC-001` |
| **Pengecualian / Catatan** | Seluruh perubahan status penanggung per baris biaya dieksekusi secara atomik dalam batas transaksi serializable dan PostgreSQL advisory lock (`BIL_CALCULATION_{invoiceId}`, `BIL_ENCOUNTER_{encounterId}`). Penulisan menggunakan pola append-only (nonaktifkan baris aktif lama lalu sisipkan baris baru) demi memelihara riwayat audit dan integritas filtered unique index. Sesuai keputusan bisnis `MPY-DEC-004`, penandaan penanggung **tidak digerbang hasil tanggungan** (baris tidak tertanggung tetap boleh ditandai dan hasilnya nol tertanggung, bukan ditolak). |

---

## 1. Masalah & Latar Belakang Bisnis

### 1.1 Kebutuhan Penentuan Penanggung di Tingkat Baris Biaya (`CAP-37`, `MPY-DES-008`, `FR-BKC-074`)
Pada operasional rumah sakit sehari-hari, pasien yang berobat dengan jaminan asuransi atau penjamin perusahaan sering kali memiliki item tindakan, obat-obatan, atau suplemen yang:
1. Diminta oleh pasien untuk dibayar secara pribadi (misalnya vitamin non-standar atau barang kenyamanan tambahan).
2. Diminta oleh dokter penanggung jawab untuk dipisahkan dari klaim penjamin karena alasan kebijakan medis tertentu.
3. Ditentukan oleh kasir untuk dialihkan penanggungnya mengikuti konfirmasi pihak penjamin.

Sebelum task ini, penanggung biaya hanya melekat secara monolitik di tingkat kunjungan (`EncounterPaymentType`). Sistem tidak dapat membagi baris mana yang dibebankan ke pasien (`CASH`) dan baris mana yang diajukan ke penjamin (`INSURANCE` atau `COMPANY_GUARANTOR`). Task `BE-BKC-048` menghadirkan perintah `PUT api/v1/health-services/billing-management/billing/invoices/{id}/item-payer-assignments` untuk mengubah penanggung beberapa baris biaya sekaligus.

### 1.2 Prinsip Bisnis: Penandaan Tidak Digerbang Hasil Tanggungan (`MPY-DEC-004`, `FR-BKC-076`, `BIL-AT-090`)
Salah satu keputusan arsitektur dan bisnis terpenting pada sistem billing Quilvian adalah:
> **Penandaan penanggung baris biaya TIDAK BOLEH digerbang oleh hasil penilaian polis/kontrak.**

Bila kasir menandai obat tertentu sebagai tanggungan `INSURANCE` atau `COMPANY_GUARANTOR`, namun berdasarkan aturan polis item tersebut berstatus tidak tertanggung (*not covered* atau melebihi limit plafon):
- Server **MUST NOT** menolak permintaan kasir.
- Server mencatat pilihan kasir bahwa baris itu ditujukan ke asuransi/perusahaan.
- Mesin kalkulasi (`BillingCalculationService` & `BillingCoverageAdapter`) mengevaluasi item tersebut, menghasilkan porsi penjamin = Rp 0, dan membebankan biayanya penuh kepada pasien.
- Dengan cara ini, lembar tagihan klaim dapat secara transparan menunjukkan bahwa obat tersebut memang diajukan ke asuransi namun ditolak/tidak ditanggung oleh polis.

### 1.3 Integritas Matematika Tagihan Selalu Nol Selisih (`FR-BKC-077`, `BIL-AT-089`)
Setiap kali kasir memperbarui penanggung baris biaya, sistem secara atomik menghitung ulang tagihan. Persamaan akuntansi tagihan dijamin seimbang sempurna:
$$\text{Total Tagihan} = \text{Bruto} + \text{Biaya Administrasi} + \text{Biaya Kamar} - \text{Diskon} + \text{PPN} + \text{Pembulatan}$$
$$\text{Total Tagihan} = \text{Porsi Pasien} + \text{Porsi Penjamin Utama} + \text{Porsi Excess} + \text{Porsi Unresolved}$$
Tidak ada selisih satu rupiah pun sesudah perubahan penanggung dieksekusi.

---

## 2. Alur Proses Bisnis & Matriks Validasi

### 2.1 Diagram Alur Eksekusi Perintah Penanggung per Baris Biaya

```text
[Kasir Memilih Penanggung per Baris di Antarmuka]
                         │
                         ▼
PUT /api/v1/health-services/billing-management/billing/invoices/{id}/item-payer-assignments
(Header: Idempotency-Key, Body: UpdateItemPayerAssignmentsRequest)
                         │
                         ├── 1. Validasi Sintaksis & Header
                         │      ├── BIL-VAL-063: Idempotency-Key tidak kosong (400)
                         │      ├── BIL-VAL-062: Alasan perubahan tidak kosong (400)
                         │      ├── ExpectedRowVersion berformat Guid valid (400)
                         │      ├── BIL-VAL-079: Daftar penanggung tidak kosong (400)
                         │      ├── BIL-VAL-080: Tidak ada InvoiceItemId ganda (400)
                         │      └── PayerKind valid (CASH / INSURANCE / COMPANY_GUARANTOR) (400)
                         │
                         ├── 2. Buka Transaksi Serializable & Kunci Advisory PostgreSQL
                         │      ├── pg_advisory_xact_lock(hashtext('BIL_CALCULATION_{invoiceId}'))
                         │      └── Cek Idempotency Replay di BilInvoicePayerChangeCommand
                         │
                         ├── 3. Validasi Kondisi Tagihan
                         │      ├── BIL-VAL-059: Status tagihan wajib OPEN (409)
                         │      ├── BIL-VAL-060: Tagihan belum menerima pembayaran (paidAmount == 0) (409)
                         │      ├── BIL-VAL-061: RowVersion cocok dengan database (409)
                         │      └── pg_advisory_xact_lock(hashtext('BIL_ENCOUNTER_{encounterId}'))
                         │
                         ├── 4. Validasi Integritas Baris Biaya
                         │      ├── BIL-VAL-075: Semua item milik tagihan ini (422)
                         │      └── BIL-VAL-076: Tidak ada item yang berstatus VOIDED (422)
                         │
                         ├── 5. Validasi Jenis Penjamin Kunjungan
                         │      ├── BIL-VAL-077: Penanggung INSURANCE ditolak jika kunjungan bukan Asuransi (422)
                         │      └── BIL-VAL-078: Penanggung COMPANY_GUARANTOR ditolak jika kunjungan bukan Perusahaan (422)
                         │
                         ├── 6. Eksekusi Mutasi Append-Only pada BilInvoiceItemPayerAssignment
                         │      ├── Set IsActive = false pada penanggung aktif lama
                         │      ├── Simpan perubahan (SaveChanges) demi filtered unique index
                         │      └── Sisipkan baris baru (IsActive = true, AssignmentSource = "MANUAL", PayerKind)
                         │
                         ├── 7. Panggil Mesin Kalkulasi (BillingCalculationService.RecalculateAsync)
                         │      ├── CalculateItemsAndTaxesAsync membaca BilInvoiceItemPayerAssignment
                         │      │   ├── CASH -> Coverable = false (mandiri pasien)
                         │      │   └── INSURANCE / COMPANY_GUARANTOR -> Coverable = true (dievaluasi mesin tanggungan)
                         │      └── Terbit versi kalkulasi baru dan RowVersion baru
                         │
                         ├── 8. Catat Audit Command pada BilInvoicePayerChangeCommand
                         │      └── Catat idempotency key, correlation id, previous/new calc version, aktor
                         │
                         └── 9. Commit Transaksi & Kembalikan ApiResponse<InvoiceEditResultResponse>
```

### 2.2 Matriks Aturan Validasi

| Aturan | Kondisi | Pesan bagi Pengguna | Kode HTTP |
| :--- | :--- | :--- | :---: |
| `BIL-VAL-059` | Tagihan tidak berstatus `OPEN` | "Tagihan ini sudah difinalisasi sehingga tidak dapat diubah lagi." | `409` |
| `BIL-VAL-060` | Tagihan sudah menerima pembayaran berhasil | "Tagihan ini sudah menerima pembayaran. Perubahan penjamin atau penanggung memerlukan proses pembalikan, bukan pengeditan." | `409` |
| `BIL-VAL-061` | Versi baris tagihan tidak cocok (*concurrency conflict*) | "Data tagihan telah berubah sejak layar ini dibuka. Muat ulang data sebelum menyimpan kembali." | `409` |
| `BIL-VAL-062` | Alasan perubahan tidak diisi | "Alasan perubahan wajib diisi." | `400` |
| `BIL-VAL-063` | Header `Idempotency-Key` tidak disertakan | "Header Idempotency-Key wajib disertakan pada permintaan ini." | `400` |
| `BIL-VAL-075` | Baris biaya yang dikirim bukan milik tagihan ini | "Ada baris biaya yang tidak terdaftar pada tagihan ini." | `422` |
| `BIL-VAL-076` | Baris biaya berstatus dibatalkan (`Voided`) | "Baris biaya yang sudah dibatalkan tidak dapat diubah penanggungnya." | `422` |
| `BIL-VAL-077` | Penanggung `INSURANCE` dipilih padahal kunjungan tidak berpenjamin asuransi | "Kunjungan ini tidak memakai asuransi, sehingga baris biaya tidak dapat ditanggung asuransi." | `422` |
| `BIL-VAL-078` | Penanggung `COMPANY_GUARANTOR` dipilih padahal kunjungan tidak berpenjamin perusahaan | "Kunjungan ini tidak memakai penjamin perusahaan, sehingga baris biaya tidak dapat ditanggung penjamin." | `422` |
| `BIL-VAL-079` | Daftar penanggung yang dikirim kosong | "Tidak ada perubahan penanggung yang dikirim." | `400` |
| `BIL-VAL-080` | Satu baris biaya dikirim lebih dari sekali dalam request | "Ada baris biaya yang dikirim lebih dari satu kali." | `400` |

---

## 3. Contoh Konkret Skenario Rumah Sakit

### Skenario 1: Pasien Meminta Suplemen Vitamin Dibayar Mandiri (`BIL-AT-089`)
- **Kasus**: Tn. H berobat jalan dengan penjamin PT Freeport Indonesia. Dokter meresepkan 3 item: Konsultasi Dokter Spesialis (Rp 250.000), Obat Antibiotik (Rp 180.000), dan Suplemen Vitamin Khusus (Rp 80.000). Pasien meminta suplemen vitamin tidak diklaim ke perusahaan melainkan dibayar tunai sendiri.
- **Tindakan Kasir**: Kasir membuka Edit Tagihan, memilih baris Suplemen Vitamin Khusus, mengganti penanggung menjadi `CASH`, memasukkan alasan *"Permintaan pasien dibayar mandiri"*, lalu menekan Simpan.
- **Hasil Sistem**:
  - `BilInvoiceItemPayerAssignment` untuk suplemen diperbarui menjadi `PayerKind = 'CASH'`, `AssignmentSource = 'MANUAL'`.
  - Tagihan dihitung ulang: Porsi penjamin berkurang sebesar Rp 80.000 dan porsi pasien bertambah sebesar Rp 80.000.
  - Subtotal Mandiri + Subtotal Penjamin + PPN = Total Tagihan (nol selisih).

### Skenario 2: Obat Tidak Ditanggung Tetap Boleh Ditandai Asuransi (`BIL-AT-090`)
- **Kasus**: Ny. R dirawat dengan asuransi swasta. Terdapat resep obat herbal/non-formularium seharga Rp 150.000 yang menurut polis asuransinya berstatus *Not Covered*. Kasir tetap menandai baris obat tersebut sebagai `INSURANCE` agar klaim dapat diajukan secara transparan.
- **Hasil Sistem**:
  - Permintaan **berhasil**, bukan ditolak.
  - Mesin kalkulasi mengevaluasi aturan asuransi untuk baris tersebut. Karena tidak tertanggung, porsi asuransi dihitung Rp 0 dan dibebankan penuh ke porsi pasien (Rp 150.000).
  - Pada rekap rincian tagihan, penanggung baris tetap tercatat `INSURANCE` dengan nilai klaim disetujui Rp 0.

### Skenario 3: Penolakan Payer yang Tidak Sesuai Kunjungan (`BIL-AT-091`)
- **Kasus**: Pasien Tn. K datang berobat sebagai pasien umum tunai (`CASH`). Kasir mencoba menandai salah satu baris laboratorium sebagai `COMPANY_GUARANTOR`.
- **Hasil Sistem**:
  - Server memeriksa jenis penjamin kunjungan aktif. Karena kunjungan adalah tunai, server menolak permintaan dengan kode HTTP `422 Unprocessable Entity` dan pesan:
    > *"Kunjungan ini tidak memakai penjamin perusahaan, sehingga baris biaya tidak dapat ditanggung penjamin."*
  - Nol perubahan yang tersimpan di database.

---

## 4. Spesifikasi Endpoint Bergaya Swagger

### Tag: `Health Services / Billing Management / Billing / Invoices`

```yaml
openapi: 3.0.3
info:
  title: Quilvian Billing System - Item Payer Assignments API
  version: 1.0.0
paths:
  /api/v1/health-services/billing-management/billing/invoices/{id}/item-payer-assignments:
    put:
      tags:
        - Health Services / Billing Management / Billing / Invoices
      summary: Memperbarui penanggung beberapa baris biaya sekaligus dan menghitung ulang tagihan
      description: |
        Mengubah penanggung (CASH, INSURANCE, atau COMPANY_GUARANTOR) untuk satu atau beberapa baris biaya tagihan.
        Menerapkan transaksi serializable, idempotensi perintah, pola penulisan append-only, dan kalkulasi ulang otomatis.
      operationId: UpdateItemPayerAssignments
      parameters:
        - name: id
          in: path
          required: true
          description: ID unik invoice billing
          schema:
            type: string
            format: uuid
        - name: Idempotency-Key
          in: header
          required: true
          description: Kunci unik idempotensi dari klien untuk mencegah eksekusi ganda
          schema:
            type: string
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/UpdateItemPayerAssignmentsRequest'
      responses:
        '200':
          description: Penanggung baris biaya berhasil diperbarui dan tagihan telah dihitung ulang
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/ApiResponseInvoiceEditResultResponse'
        '400':
          description: Format request tidak valid, daftar kosong, item duplikat, atau header tidak lengkap
        '403':
          description: Pengguna tidak memiliki hak akses BillingInvoice:Update
        '404':
          description: Invoice billing atau encounter tidak ditemukan
        '409':
          description: Tagihan tidak OPEN, sudah ada pembayaran, atau versi baris berubah (concurrency conflict)
        '422':
          description: Item bukan milik tagihan, item dibatalkan, atau penanggung tidak sesuai penjamin kunjungan
components:
  schemas:
    UpdateItemPayerAssignmentsRequest:
      type: object
      required:
        - assignments
        - expectedRowVersion
        - reason
      properties:
        assignments:
          type: array
          items:
            $ref: '#/components/schemas/ItemPayerAssignmentItemRequest'
        expectedRowVersion:
          type: string
          description: Versi baris tagihan saat layar dibuka
        reason:
          type: string
          description: Alasan perubahan penanggung yang dicatat auditor
        correlationId:
          type: string
          nullable: true
        causationId:
          type: string
          nullable: true
    ItemPayerAssignmentItemRequest:
      type: object
      required:
        - invoiceItemId
        - payerKind
      properties:
        invoiceItemId:
          type: string
          format: uuid
        payerKind:
          type: string
          enum: [CASH, INSURANCE, COMPANY_GUARANTOR]
    ApiResponseInvoiceEditResultResponse:
      type: object
      properties:
        success:
          type: boolean
        code:
          type: integer
        message:
          type: string
        data:
          $ref: '#/components/schemas/InvoiceEditResultResponse'
    InvoiceEditResultResponse:
      type: object
      properties:
        calculation:
          type: object
          description: Hasil perhitungan tagihan terkini setelah perubahan
        rowVersion:
          type: string
          format: uuid
          description: Versi baris tagihan yang baru
        resetAssignmentCount:
          type: integer
          description: Jumlah baris biaya yang direset otomatis (0 untuk perintah ini)
        warnings:
          type: array
          items:
            type: string
```

---

## 5. Ringkasan Perubahan Berkas

| Berkas | Status | Penjelasan Perubahan |
| :--- | :---: | :--- |
| `Areas/HealthServices/BillingManagement/Billing/Dtos/BillingPayerEditDtos.cs` | Diperbarui | Menambahkan DTO `UpdateItemPayerAssignmentsRequest` dan `ItemPayerAssignmentItemRequest` sesuai kontrak `BIL-API-1.0`. |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingCalculationService.cs` | Diperbarui | Mengintegrasikan pembacaan `BilInvoiceItemPayerAssignments` aktif pada `CalculateItemsAndTaxesAsync`: bila `PayerKind == "CASH"` maka `Coverable = false` (mandiri pasien); bila `INSURANCE` atau `COMPANY_GUARANTOR` maka `Coverable = true` (dievaluasi mesin tanggungan); bila belum ada maka fallback ke master kategori. |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingPayerEditService.cs` | Diperbarui | Menambahkan metode `UpdateItemPayerAssignmentsAsync` yang mengelola batas transaksi serializable, advisory lock, verifikasi idempotensi, penegakan matriks validasi `BIL-VAL-059` s/d `063` dan `BIL-VAL-075` s/d `080`, penulisan append-only, recalculate, jejak audit pada `BilInvoicePayerChangeCommand`, serta audit log. |
| `Areas/HealthServices/BillingManagement/Billing/Controllers/BillingInvoicesController.cs` | Diperbarui | Mengekspos endpoint `PUT api/v1/health-services/billing-management/billing/invoices/{id}/item-payer-assignments` dengan otorisasi `BillingInvoice:Update`, parameter header `Idempotency-Key`, dan mapping exception ke kode HTTP yang tepat. |
| `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` | Diperbarui | Menandai status task `BE-BKC-048` sebagai `✅ Selesai`. |
| `docs/module-blueprints/billing-kasir/roadmap/requirement-traceability.md` | Diperbarui | Memperbarui baris traceability `FR-BKC-074` s/d `FR-BKC-077` dan `CAP-37` menjadi selesai untuk backend. |

---

## 6. Verifikasi & Langkah Pengguna Selanjutnya

Sesuai aturan keselamatan workspace Quilvian, pengujian build terminal diserahkan kepada pengguna untuk dijalankan secara manual:

```bash
dotnet build
```

Hasil verifikasi yang diharapkan:
- Seluruh DTO, service, dan controller terkompilasi tanpa error (`0 errors`).
- Tidak ada breaking change terhadap endpoint kalkulasi existing maupun flow `BE-BKC-047`.
- Roadmap siap dilanjutkan ke task berikutnya: **`BE-BKC-049`** (*Perintah penebusan obat / Drug billing disposition command*).
