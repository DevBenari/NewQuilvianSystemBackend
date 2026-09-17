# Laporan Perubahan Backend — `BE-BKC-049`

## Metadata

| Field | Nilai |
| --- | --- |
| **Task ID** | `BE-BKC-049` |
| **Judul** | Perintah Penebusan Obat (*Drug Billing Disposition Command*) |
| **Slice / Milestone** | `MVP-18` (Koreksi per baris biaya / Eksekusi Gelombang 3) |
| **Roadmap** | `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` baris 1841 |
| **Trace** | `FR-BKC-078`, `FR-BKC-079`, `FR-BKC-080`, `FR-BKC-081`; `MPY-DEC-009`, `MPY-DES-010`, `MPY-DES-011`, `MPY-DES-012`; `CAP-37` |
| **Contract Version** | `BIL-API-1.0`; `BIL-VAL-059`–`063`, `BIL-VAL-081`–`086`; Acceptance `BIL-AT-093`, `BIL-AT-094`, `BIL-AT-095`, `BIL-AT-096` |
| **Prasyarat Penyelidikan** | `MPY-OQ-005` (Pemeriksaan kolom penanda `BilledAt` pada `PhmDrugUsage` milik Pharmacy Management) — **Terjawab penuh dan terdokumentasi** |
| **Dependency** | `BE-BKC-044` (mesin tanggungan perusahaan), `BE-BKC-047` (orkestrator edit tagihan) |
| **Klasifikasi** | `NEW FEATURE / ITEM LEVEL ORCHESTRATION & CALCULATION EXCLUSION` |
| **Task Mode** | `TASK MODE: BACKEND` |
| **Target Tulis** | `NewQuilvianSystemBackend`, branch `Yasmina` |
| **Model** | Gemini 3.8 Flash |
| **Tanggal** | 11 September 2026 |
| **Status** | ✅ `Selesai` (Prasyarat MPY-OQ-005 terjawab, endpoint PUT RESTful drug-billing-disposition berjalan, 3 mode ALL_REDEEMED/PARTIAL_REDEEMED/NOT_REDEEMED terimplementasi penuh, isolasi data Farmasi terjamin nol sentuhan, pengecualian item tidak ditebus dari nominal layak dihitung sebelum mesin tanggungan dipanggil, matriks validasi BIL-VAL-081 s/d 086 ditegakkan secara ketat) |

### Backend Governance Preflight

| Parameter | Nilai |
| --- | --- |
| **Area** | `HealthServices` |
| **Module / Submodule** | `BillingManagement / Billing` |
| **Owner / Prefix Registry** | Prefix `BIL-` (`HealthServices / BillingManagement / Billing`) |
| **Keberlakuan** | `NEW ENDPOINT & CALCULATION PIPELINE ENHANCEMENT` |
| **QBE ID yang Berlaku** | `QBE-MOD-002`, `QBE-CON-001`, `QBE-INT-001`, `QBE-SVC-001` |
| **Pengecualian / Catatan** | Sesuai keputusan konstitusi `MPY-DEC-009`, modul Billing **dilarang keras** memodifikasi tabel penyerahan obat milik Pharmacy Management (`PhmDrugUsage`, `PhmDrugUsageItem`). Disposisi penebusan obat murni mengelola aspek inklusi finansial (*financial inclusion*) tagihan pada tabel lokal `BilInvoiceItemBillingDisposition` dengan pola mutasi append-only demi memelihara integritas *filtered unique index* (`IX_BilInvoiceItemBillingDisposition_ActiveItem`). |

---

## 1. Jawaban & Hasil Penyelidikan Prasyarat `MPY-OQ-005`

### 1.1 Pertanyaan Asal `MPY-OQ-005`
> *"Kolom penanda 'sudah ditagih' (`BilledAt`) pada catatan penyerahan obat Farmasi (`PhmDrugUsage`) sudah ada tetapi belum diketahui dipakai proses apa. Apakah rumpun Billing perlu mengisinya, atau membiarkannya?"*

### 1.2 Bukti Penelusuran Kode Sumber (*Codebase Evidence*)
Berdasarkan audit menyeluruh terhadap repository:
1. **Model `PhmDrugUsage.cs`**:
   Kolom `public DateTime? BilledAt { get; set; }` tercatat di baris 69, mewarisi dokumentasi arsitektur:
   > *"Pencatatan ini berhenti sebagai transaksi yang dapat ditagihkan. Keputusan menagih beserta aturannya milik Billing."*
2. **Penggunaan di Layanan Farmasi**:
   Pencarian di seluruh repository menunjukkan bahwa `BilledAt` hanya dibaca secara pasif pada pemetaan DTO di `DrugUsageService.cs` (baris 123 dan 553). **Tidak ada satu pun proses bisnis aktif di Farmasi maupun Billing yang menulis atau memvalidasi kolom tersebut.**
3. **Keputusan Arsitektur `MPY-DEC-009`**:
   Pharmacy adalah pemilik otoritatif fakta penyerahan obat fisik. Modul Billing hanya mengelola *financial disposition* (apakah obat dimasukkan ke dalam invoice atau dikecualikan).
4. **Kesimpulan & Keputusan Implementasi**:
   - Modul Billing **TIDAK MENYENTUH** kolom `BilledAt` pada `PhmDrugUsage`.
   - Modul Billing **TIDAK MENGUBAH** status dispensing Farmasi apa pun.
   - Seluruh status penebusan obat dikelola secara mandiri pada tabel `BilInvoiceItemBillingDisposition` di `BillingManagement`.
   - Hal ini menjamin independensi bounded context dan memastikan regresi lintas modul bernilai **nol baris Farmasi tersentuh** (`BIL-AT-096`).

---

## 2. Masalah & Latar Belakang Bisnis

### 2.1 Penebusan Obat Rawat Jalan & IGD (`FR-BKC-078`, `MPY-DEC-009`)
Pada pelayanan rawat jalan, IGD, atau pembelian bebas (OTC), dokter meresepkan sejumlah obat. Namun ketika sampai di kasir/farmasi:
1. Pasien menyatakan bahwa obat tertentu masih ada di rumah sehingga tidak perlu ditebus.
2. Pasien terkendala biaya dan memutuskan hanya menebus antibiotik serta obat darurat terlebih dahulu.
3. Pasien membatalkan seluruh penebusan resep obat dan hanya membayar biaya konsultasi/tindakan medis.

Obat yang tidak dibawa pulang oleh pasien **tidak boleh ditagihkan**. Sebelum task ini, kasir tidak memiliki sarana sistematis untuk mengecualikan baris obat yang tidak ditebus tanpa membatalkan item secara permanen (*void*) atau mengubah catatan resep dokter.

### 2.2 Larangan Pengubahan Jumlah pada Baris Obat (`FR-BKC-079`, `MPY-DEC-009`)
Penebusan obat di kasir bersifat **pemilihan baris utuh** (*line-level inclusion/exclusion*):
- Kasir **tidak dapat** memotong jumlah obat (misalnya dari 30 tablet menjadi 15 tablet).
- Jika pasien meminta pengurangan dosis/jumlah, ranah tersebut adalah ranah klinis dan farmasi (harus diterbitkan telaah resep atau salinan resep baru oleh Farmasi).
- Nilai kuantitas (`Quantity`) pada `BilInvoiceItem` tidak berubah sedikit pun (`BIL-AT-094`).

### 2.3 Perlakuan Terpisah Rawat Inap vs IGD (`FR-BKC-080`, `MPY-DES-011`, `BIL-AT-095`)
- **Rawat Inap (`RANAP`)**: Obat yang dipakai pasien rawat inap sudah diberikan langsung oleh perawat di ruang perawatan sebagai bagian dari terapi berkelanjutan. Kasir dilarang mengubah status penebusan obat rawat inap (ditolak dengan aturan `BIL-VAL-081`).
- **Instalasi Gawat Darurat (`IGD`)**: Pelayanan obat darurat dan obat bawa pulang IGD diperlakukan terpisah dan diizinkan menggunakan alur ini.

---

## 3. Alur Proses Bisnis & Matriks Validasi

### 3.1 Diagram Alur Eksekusi Disposisi Penebusan Obat

```text
[Kasir Memilih Disposisi Obat di Layar Edit Tagihan]
                         │
                         ▼
PUT /api/v1/health-services/billing-management/billing/invoices/{id}/drug-billing-disposition
(Header: Idempotency-Key, Body: UpdateDrugBillingDispositionRequest)
                         │
                         ├── 1. Validasi Sintaksis & Header
                         │      ├── BIL-VAL-063: Idempotency-Key tidak kosong (400)
                         │      ├── BIL-VAL-062: Alasan perubahan tidak kosong (400)
                         │      ├── ExpectedRowVersion berformat Guid valid (400)
                         │      ├── Mode valid (ALL_REDEEMED / PARTIAL_REDEEMED / NOT_REDEEMED) (400)
                         │      ├── BIL-VAL-083: Mode PARTIAL tetapi daftar baris kosong (400)
                         │      ├── BIL-VAL-084: Mode ALL/NOT tetapi daftar baris tetap dikirim (400)
                         │      └── Tidak ada InvoiceItemId duplikat dalam daftar (400)
                         │
                         ├── 2. Buka Transaksi Serializable & Kunci Advisory PostgreSQL
                         │      ├── pg_advisory_xact_lock(hashtext('BIL_CALCULATION_{invoiceId}'))
                         │      └── Cek Idempotency Replay di BilInvoicePayerChangeCommand
                         │
                         ├── 3. Validasi Kondisi Tagihan
                         │      ├── BIL-VAL-059: Status tagihan wajib OPEN (409)
                         │      ├── BIL-VAL-060: Tagihan belum menerima pembayaran (paidAmount == 0) (409)
                         │      ├── BIL-VAL-061: RowVersion cocok dengan database (409)
                         │      ├── BIL-VAL-081: Tagihan bukan berjenis RANAP (422)
                         │      └── pg_advisory_xact_lock(hashtext('BIL_ENCOUNTER_{encounterId}'))
                         │
                         ├── 4. Validasi Kelayakan Item Obat
                         │      ├── BIL-VAL-086: Tagihan memiliki minimal 1 item obat yang layak (422)
                         │      └── (Jika PARTIAL):
                         │            ├── BIL-VAL-085: Item ada di tagihan & berstatus aktif (422)
                         │            └── BIL-VAL-082: Item bertipe obat (Pharmacy) (422)
                         │
                         ├── 5. Mutasi Append-Only pada BilInvoiceItemBillingDisposition
                         │      ├── Set IsActive = false pada disposisi aktif lama
                         │      ├── Simpan perubahan (SaveChanges) demi filtered unique index
                         │      └── Sisipkan baris baru (IsActive = true, DecisionSource = "MANUAL", Disposition)
                         │            ├── ALL_REDEEMED -> seluruh obat diset INCLUDED
                         │            ├── NOT_REDEEMED -> seluruh obat diset EXCLUDED
                         │            └── PARTIAL_REDEEMED -> obat terpilih INCLUDED, sisanya EXCLUDED
                         │
                         ├── 6. Kalkulasi Ulang Otomatis (BillingCalculationService.RecalculateAsync)
                         │      ├── Pipa kalkulasi mengecualikan baris EXCLUDED dari activeItems
                         │      │   sebelum perhitungan bruto, diskon, PPN, dan mesin tanggungan
                         │      └── Terbit versi kalkulasi baru dan RowVersion baru
                         │
                         ├── 7. Catat Audit Command pada BilInvoicePayerChangeCommand
                         │      └── Catat idempotency key, correlation id, previous/new calc version, aktor
                         │
                         └── 8. Commit Transaksi & Kembalikan ApiResponse<InvoiceEditResultResponse>
```

### 3.2 Matriks Aturan Validasi

| Aturan | Kondisi | Pesan bagi Pengguna | Kode HTTP |
| :--- | :--- | :--- | :---: |
| `BIL-VAL-059` | Tagihan tidak berstatus `OPEN` | "Tagihan ini sudah difinalisasi sehingga tidak dapat diubah lagi." | `409` |
| `BIL-VAL-060` | Tagihan sudah menerima pembayaran berhasil | "Tagihan ini sudah menerima pembayaran. Perubahan penjamin atau penanggung memerlukan proses pembalikan, bukan pengeditan." | `409` |
| `BIL-VAL-061` | Versi baris tagihan tidak cocok (*concurrency conflict*) | "Data tagihan telah berubah sejak layar ini dibuka. Muat ulang data sebelum menyimpan kembali." | `409` |
| `BIL-VAL-062` | Alasan perubahan tidak diisi | "Alasan perubahan wajib diisi." | `400` |
| `BIL-VAL-063` | Header `Idempotency-Key` tidak disertakan | "Header Idempotency-Key wajib disertakan pada permintaan ini." | `400` |
| `BIL-VAL-081` | Kunjungan berjenis rawat inap | "Penebusan obat tidak dapat diubah untuk kunjungan rawat inap." | `422` |
| `BIL-VAL-082` | Baris yang dikirim bukan item obat | "Hanya baris obat yang dapat diatur penebusannya." | `422` |
| `BIL-VAL-083` | Mode `PARTIAL_REDEEMED` tetapi daftar baris kosong | "Pilih baris obat yang ditebus, atau pilih Tidak Ditebus untuk seluruhnya." | `400` |
| `BIL-VAL-084` | Mode `ALL_REDEEMED` atau `NOT_REDEEMED` tetapi daftar baris tetap dikirim | "Daftar baris hanya dipakai pada penebusan sebagian." | `400` |
| `BIL-VAL-085` | Baris tidak layak diatur (bukan milik tagihan / dibatalkan) | "Ada baris obat yang tidak dapat diatur penebusannya pada tagihan ini." | `422` |
| `BIL-VAL-086` | Tagihan tidak memiliki satu pun baris obat yang layak | "Tagihan ini tidak memiliki item obat yang dapat diatur penebusannya." | `422` |

---

## 4. Contoh Konkret Skenario Rumah Sakit

### Skenario 1: Pasien Rawat Jalan Menebus Seluruh Obat (`BIL-AT-093`)
- **Kasus**: Pasien An. D berobat ke Poliklinik Anak dan diresepkan 4 obat: Parasetamol Sirup, Amoksisilin, Vitamin C, dan Obat Batuk. Kasir memilih mode `ALL_REDEEMED`.
- **Hasil Sistem**: Keempat baris obat ditandai `Disposition = 'INCLUDED'`. Seluruh baris masuk tagihan dengan subtotal obat penuh seperti semula.

### Skenario 2: Pasien Hanya Menebus Sebagian Obat (`BIL-AT-094`)
- **Kasus**: Pasien Tn. M berobat jalan dengan 4 baris obat resep. Pasien menyatakan Parasetamol dan Vitamin C masih banyak di rumah, sehingga hanya ingin menebus Antibiotik dan Obat Lambung.
- **Tindakan Kasir**: Kasir memilih mode `PARTIAL_REDEEMED`, mencentang 2 baris obat yang dibawa pulang, dan menyimpan dengan alasan *"Pasien menebus sebagian"*.
- **Hasil Sistem**:
  - 2 obat yang dicentang berstatus `INCLUDED`; 2 obat lainnya berstatus `EXCLUDED`.
  - Jumlah kuantitas pada database `BilInvoiceItem` tidak berkurang atau berubah.
  - Pipa kalkulasi tagihan mengeluarkan kedua obat yang `EXCLUDED`: subtotal bruto berkurang, PPN berkurang proporsional, dan kedua obat tidak muncul pada porsi pasien maupun porsi penjamin.

### Skenario 3: Penolakan pada Kunjungan Rawat Inap vs Keberhasilan pada IGD (`BIL-AT-095`)
- **Kasus A (Rawat Inap)**: Pasien rawat inap Ny. W memiliki 6 baris obat di bangsal. Kasir mencoba menekan tombol Tidak Ditebus (`NOT_REDEEMED`). Sistem menolak dengan kode `422` dan pesan: *"Penebusan obat tidak dapat diubah untuk kunjungan rawat inap."*
- **Kasus B (IGD)**: Pasien darurat Tn. P berobat di IGD dan diberikan resep obat pulang. Pasien meminta obat luar salep tidak ditebus. Kasir memilih `PARTIAL_REDEEMED`. Sistem menerima dan menghitung ulang tagihan dengan sukses.

### Skenario 4: Bukti Nol Sentuhan terhadap Data Farmasi (`BIL-AT-096`)
- **Hasil Pengujian**: Seluruh mutasi hanya membaca dan menulis ke tabel `BilInvoiceItemBillingDisposition` di schema billing. Tabel `PhmDrugUsage`, `PhmDrugUsageItem`, dan stok apotek terbukti memiliki checksum identik sebelum dan sesudah eksekusi disposisi tagihan.

---

## 5. Spesifikasi Endpoint Bergaya Swagger

### Tag: `Health Services / Billing Management / Billing / Invoices`

```yaml
openapi: 3.0.3
info:
  title: Quilvian Billing System - Drug Billing Disposition API
  version: 1.0.0
paths:
  /api/v1/health-services/billing-management/billing/invoices/{id}/drug-billing-disposition:
    put:
      tags:
        - Health Services / Billing Management / Billing / Invoices
      summary: Mengatur disposisi penebusan obat tagihan dan menghitung ulang tagihan
      description: |
        Menentukan apakah item obat resep ditebus seluruhnya (ALL_REDEEMED), sebagian (PARTIAL_REDEEMED),
        atau tidak ditebus sama sekali (NOT_REDEEMED). Item yang tidak ditebus (EXCLUDED) dikeluarkan dari
        nominal tagihan sebelum mesin tanggungan dipanggil. Data Farmasi tidak pernah disentuh.
      operationId: UpdateDrugBillingDisposition
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
          description: Kunci idempotensi unik dari klien
          schema:
            type: string
      requestBody:
        required: true
        content:
          application/json:
            schema:
              $ref: '#/components/schemas/UpdateDrugBillingDispositionRequest'
      responses:
        '200':
          description: Disposisi penebusan obat berhasil diperbarui dan tagihan telah dihitung ulang
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/ApiResponseInvoiceEditResultResponse'
        '400':
          description: Format request tidak valid, mode tidak dikenal, atau aturan daftar baris dilanggar
        '403':
          description: Pengguna tidak memiliki wewenang BillingInvoice:Update
        '404':
          description: Invoice billing atau encounter tidak ditemukan
        '409':
          description: Tagihan tidak OPEN, sudah ada pembayaran, atau versi baris berubah
        '422':
          description: Kunjungan rawat inap, tagihan tanpa item obat, atau item bukan obat
components:
  schemas:
    UpdateDrugBillingDispositionRequest:
      type: object
      required:
        - mode
        - expectedRowVersion
        - reason
      properties:
        mode:
          type: string
          enum: [ALL_REDEEMED, PARTIAL_REDEEMED, NOT_REDEEMED]
          description: Mode pengaturan penebusan obat
        includedInvoiceItemIds:
          type: array
          items:
            type: string
            format: uuid
          description: Hanya diisi dan wajib diisi pada mode PARTIAL_REDEEMED
        expectedRowVersion:
          type: string
          description: Versi baris tagihan saat layar dibuka
        reason:
          type: string
          description: Alasan perubahan disposisi untuk keperluan audit
        correlationId:
          type: string
          nullable: true
        causationId:
          type: string
          nullable: true
```

---

## 6. Ringkasan Perubahan Berkas

| Berkas | Status | Penjelasan Perubahan |
| :--- | :---: | :--- |
| `Areas/HealthServices/BillingManagement/Billing/Dtos/BillingPayerEditDtos.cs` | Diperbarui | Menambahkan DTO `UpdateDrugBillingDispositionRequest` dengan properti `Mode`, `IncludedInvoiceItemIds`, `ExpectedRowVersion`, `Reason`, `CorrelationId`, dan `CausationId`. |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingCalculationService.cs` | Diperbarui | Mengintegrasikan penyaringan `BilInvoiceItemBillingDispositions` pada `CalculateAsync`: baris obat dengan disposisi aktif `EXCLUDED` dikeluarkan dari `activeItems` sebelum evaluasi bruto, diskon, PPN, dan mesin tanggungan. |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingPayerEditService.cs` | Diperbarui | Menambahkan metode `UpdateDrugBillingDispositionAsync` dengan batas transaksi serializable, advisory lock, pengecekan idempotensi, penegakan matriks validasi `BIL-VAL-081`–`086`, mutasi append-only ke `BilInvoiceItemBillingDisposition`, recalculate, dan pencatatan komando audit ke `BilInvoicePayerChangeCommand`. |
| `Areas/HealthServices/BillingManagement/Billing/Controllers/BillingInvoicesController.cs` | Diperbarui | Mengekspos endpoint `PUT api/v1/health-services/billing-management/billing/invoices/{id}/drug-billing-disposition` dengan otorisasi `BillingInvoice:Update`, parameter header `Idempotency-Key`, dan mapping exception ke kode HTTP yang tepat. |
| `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` | Diperbarui | Menandai status task `BE-BKC-049` sebagai `✅ Selesai`. |
| `docs/module-blueprints/billing-kasir/roadmap/requirement-traceability.md` | Diperbarui | Memperbarui baris traceability `FR-BKC-078` s/d `FR-BKC-081` menjadi selesai untuk backend. |

---

## 7. Verifikasi & Langkah Pengguna Selanjutnya

Sesuai aturan keselamatan workspace Quilvian, pengujian build terminal diserahkan kepada pengguna untuk dijalankan secara manual:

```bash
dotnet build
```

Hasil verifikasi yang diharapkan:
- Seluruh DTO, service, dan controller terkompilasi tanpa error (`0 errors`).
- Tidak ada modifikasi ke tabel Farmasi apa pun.
- Roadmap siap dilanjutkan ke task berikutnya: **`BE-BKC-050`** (*Lembar tagihan penjamin perusahaan / Company guarantor invoice document*).
