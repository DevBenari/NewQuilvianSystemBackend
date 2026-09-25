# Laporan Perubahan Frontend — `FE-BUI-007`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BUI-007` |
| Judul | Modal Ajukan Refund Dua Sumber — Billing (Multi-Select Item + Tanggal) dan Deposito (Otomatis); Penggantian Dropdown Refundable Credit Lama |
| Slice | Gelombang `MVP-33` — Revisi UI Billing: Modal Ajukan Refund Dua Sumber (`docs/module-blueprints/billing-kasir/roadmap/frontend-roadmap.md` § Gelombang UI Billing, task `FE-BUI-007`) |
| Roadmap | `docs/module-blueprints/billing-kasir/roadmap/frontend-roadmap.md` § `FE-BUI-007` — Modal Ajukan Refund Dua Sumber |
| Trace | `BUI-DEC-011` (Penghapusan dropdown Refundable Credit lama), `BUI-DEC-012` (Dua sumber refund: Billing multi-select item + Deposito sisa deposito, alur persetujuan refund tidak berubah), `FR-BUI-011`, `FR-BUI-012`, `BUI-VAL-03`, `BUI-VAL-04`, `BUI-VAL-05`, `BUI-DES-002` (Aditif `TransactionDate`), `BUI-DES-010`, `BUI-DES-011`, `UAT-BUI-06`, `UAT-BUI-07`, `UAT-BUI-11`, `UAT-BUI-12` |
| Contract version | `BIL-API-1.5` (draft) — `GET /{id}/refundable-items` (+ `TransactionDate`), `GET /{id}/remaining-deposit`, `POST /{id}/refunds` (`CreateRefundRequest`: `RefundCategory`, `SelectedBillingItemIds`, `RequestedAmount`, `Reason`) |
| Wewenang UI | Struktur radio pilihan sumber (Billing vs Deposito) + datatable checkbox+nama+tanggal+nominal **terkunci** desain (`03-frontend-architecture.md` Bagian 9.2). Total refund Billing **terkunci otomatis** (hasil penjumlahan `RefundableAmount` item yang dicentang, dilarang input bebas manual, `BUI-DEC-012`). Nominal Deposito **terkunci otomatis** sebesar `RemainingDepositAmount` (dilarang melebihi sisa saldo deposito). Gaya detail visual, kartu radio, dan tabel `DEV_DISCRETION` |
| Dependency | `[BE] BE-BUI-002` (`BillingRefundableItemResponse` penambahan `TransactionDate` terisi dari `item.CreateDateTime`) |
| Klasifikasi | `MEDIUM` — penggantian total modal lama, pembuatan thunk & state Redux baru, integrasi hook dengan kalkulasi otomatis dan proteksi validasi client-side, serta styling antarmuka modern |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev/src/**`, `QuilvianSystemFrontendDev/tests/**` |
| Model | Gemini 3.8 Flash |
| Commit frontend saat dikerjakan | `b3f45db7b4bd4dafd06967f9a7cb62d41d2e0a28` (branch `yasmina`) |
| Commit backend yang dijadikan rujukan | `d6cdfaf9fda8889d6fe73db1e97cc28c4f022852` (branch `Yasmina`) |
| Tanggal | 2026-09-25 |
| Status | ✅ **SELESAI 25 September 2026.** Modal Ajukan Refund (`CreateRefundModal`) telah diganti total isinya dengan antarmuka baru dua sumber refund melalui Radio: **"Billing"** dan **"Deposito"** (`BUI-DEC-011`, `BUI-DEC-012`, `FR-BUI-011`, `FR-BUI-012`). Ketergantungan terhadap dropdown "Refundable Credit" lama (`#refund-credit-id`, props `refundableCredits`, dll.) telah dihapus sepenuhnya (`BUI-DEC-011`). Pada sumber **"Billing"**, kasir disajikan tabel item tagihan dengan kolom multi-select Checkbox, Nama Item, Tanggal Transaksi (`TransactionDate` dari `BE-BUI-002`), dan Nominal (`RefundableAmount`); total refund dihitung otomatis dari penjumlahan item yang dicentang tanpa dapat diketik manual bebas (`UAT-BUI-06`). Pada sumber **"Deposito"**, kasir disajikan informasi sisa saldo deposito pasien (`RemainingDepositAmount`) dan nominal refund otomatis terisi sebesar saldo tersebut secara terkunci (`UAT-BUI-07`). Validasi sisi klien secara ketat mencegah submit pada mode Billing tanpa item yang dicentang dengan pesan `BUI-VAL-03` (`UAT-BUI-11`), serta menolak submit mode Deposito jika saldo deposito pasien Rp 0 dengan pesan `BUI-VAL-05` (`UAT-BUI-12`). Alur persetujuan pengajuan refund ke backend tetap identik untuk kedua sumber melalui payload `CreateRefundRequest` ke endpoint `POST /v1/health-services/billing-management/billing/invoices/{id}/refunds` (`BUI-DEC-012`). Verifikasi kualitas: `npm run lint:errors` lulus (0 error, exit code 0); `npm run test:unit` lulus 1.695/1.703 (+6 tes baru `create-refund-modal.test.mjs` lulus 100%, 8 kegagalan pre-existing konsisten); `npm run build` berhasil (`✓ Compiled successfully`, seluruh rute Next.js dan standalone runtime siap). |

---

## 1. Keadaan yang Ditemukan di Awal

Berdasarkan audit kemampuan awal (`01-existing-capability-map.md` `CAP-BUI-12`), keputusan wawancara (`00-interview-decisions.md` `BUI-DEC-011`, `BUI-DEC-012`), dan arsitektur frontend (`03-frontend-architecture.md` Bagian 9 `BUI-DES-010`, `BUI-DES-011`):
1. **Model Interaksi Lama yang Usang (Dropdown Refundable Credit)**:
   - Komponen lama `create-refund-modal.jsx` bergantung pada dropdown `<select id="refund-credit-id">` yang memuat daftar entitas kredit (`RefundableCreditResponse`).
   - Kasir harus memilih satu baris kredit, lalu mengetikkan nominal secara manual pada input teks bebas `BaseTextField` (`requestedAmount`).
   - Model ini tidak sesuai dengan alur operasional kasir rumah sakit di mana pasien yang ingin refund biaya layanan/tindakan/obat tertentu menghendaki refund berbasis item tindakan yang dibatalkan, atau refund sisa saldo deposito yang mengendap.
2. **Kesiapan Backend yang Lebih Maju dari Frontend**:
   - Backend `BillingRefundService.cs` dan `BillingInvoicesController.cs` sebenarnya telah lama memiliki dukungan penuh untuk kategori refund dua sumber (`RefundCategory: "BILLING" | "DEPOSITO"`, `SelectedBillingItemIds`), serta endpoint `GET /{id}/refundable-items` dan `GET /{id}/remaining-deposit`.
   - Namun, frontend sebelumnya sama sekali belum mengonsumsi field dan endpoint ini.
3. **Penyelesaian Dependency `BE-BUI-002`**:
   - Satu-satunya celah pada DTO `BillingRefundableItemResponse` sebelumnya adalah ketiadaan kolom tanggal transaksi.
   - Task `BE-BUI-002` telah menambahkan properti `TransactionDate` yang diproyeksikan dari `item.CreateDateTime` baris `BilInvoiceItem`. Dengan demikian, seluruh kontrak data telah lengkap dan siap dikonsumsi.

---

## 2. Proses Bisnis dari Sisi Pengguna

1. **Kasir Membuka Modal Refund dari Riwayat Pembayaran (`FE-BUI-004` / `BUI-DES-012`)**:
   - Kasir membuka menu **Riwayat Pembayaran** (`/health-services/billing-management/billing-invoices/history`), memilih baris tagihan yang bersangkutan, lalu pada kolom **Aksi ▾** mengklik opsi **+ Ajukan Refund**.
   - Modal `Ajukan Refund` terbuka dengan tampilan bersih, modern, dan langsung memuat data item refundable serta sisa deposito pasien di latar belakang.
2. **Kasir Memilih Sumber Refund: "Billing" (Default)**:
   - Kasir melihat dua kartu pilihan sumber: **Billing** dan **Deposito**.
   - Secara default, sumber **Billing** terpilih. Sistem menampilkan tabel daftar item tagihan yang mencakup:
     - Checkbox multi-select di setiap baris (serta checkbox "Pilih Semua" di header tabel).
     - Nama Item layanan/obat/tindakan (misalnya *"Konsultasi Spesialis Penyakit Dalam"*, *"Amoxicillin 500mg"*).
     - Tanggal Transaksi transaksi layanan (misalnya *"20 Sep 2026"*).
     - Nominal item yang dapat direfund (misalnya *"Rp 350.000"*, *"Rp 75.000"*).
3. **Kalkulasi Total Refund Otomatis pada Sumber Billing (`UAT-BUI-06`)**:
   - Skenario konkret: Pasien membatalkan tindakan laboratorium dan satu resep obat. Kasir mencentang item *Laboratorium Darah Lengkap* (Rp 120.000) dan *Obat Salep* (Rp 80.000).
   - Di bawah tabel, kotak ringkasan **Total Refund** langsung memperbarui angkanya secara otomatis menjadi **Rp 200.000**.
   - Kasir tidak dapat dan tidak perlu mengetik nominal secara manual; nominal refund terkunci secara deterministik berdasarkan penjumlahan item yang dicentang.
4. **Kasir Memilih Sumber Refund: "Deposito" (`UAT-BUI-07`)**:
   - Jika pasien meminta pengembalian kelebihan saldo depositonya saat kepulangan/checkout, kasir memilih opsi radio **Deposito**.
   - Tabel item billing disembunyikan, dan panel **Sisa Deposito Pasien** ditampilkan:
     - Menampilkan saldo sisa deposito aktif pasien (misalnya *"Rp 2.500.000"*).
     - Kotak **Nominal Refund Diajukan** secara otomatis terisi penuh sebesar nilai sisa saldo deposito (Rp 2.500.000) dalam status terkunci (read-only).
5. **Proteksi Validasi Jalur Gagal di Sisi Klien**:
   - **Skenario Jalur Gagal 1 (`UAT-BUI-11` / `BUI-VAL-03`)**: Kasir memilih sumber Billing tetapi belum mencentang satu pun item di tabel, lalu menekan tombol "Ajukan Refund". Sistem langsung menolak pengiriman ke server dan memunculkan peringatan: *"Pilih minimal satu item billing yang akan direfund."*
   - **Skenario Jalur Gagal 2 (`UAT-BUI-12` / `BUI-VAL-05`)**: Kasir memilih sumber Deposito pada pasien yang sisa depositonya Rp 0 (atau tidak memiliki deposit account aktif). Sistem menampilkan peringatan bahwa sisa saldo adalah Rp 0, dan tombol pengajuan menolak pengiriman dengan pesan: *"Pasien tidak memiliki sisa saldo deposito yang dapat direfund."*
6. **Pengisian Alasan dan Pengiriman Pengajuan**:
   - Kasir wajib mengisi alasan pengajuan refund pada textarea (maksimal 500 karakter dengan indikator penghitung karakter *real-time*).
   - Kasir menekan **Ajukan Refund**. Sistem mengirimkan request ber-idempotency key ke backend.
   - Setelah sukses, modal tertutup otomatis, notifikasi toast hijau *"Refund case berhasil diajukan"* muncul, dan daftar pengecualian finansial di-refresh seketika.
   - Alur persetujuan refund (*approval workflow*) pada tahap selanjutnya tetap identik untuk kedua sumber (`BUI-DEC-012`).

---

## 3. Spesifikasi Teknis Endpoint Bergaya Swagger

Seluruh interaksi API pada task ini terikat pada grup kontroler `Billing` dan `BillingRefund`:

```csharp
[Route("api/v1/health-services/billing-management/billing/invoices")]
[Tags("Health Services / Billing Management / Billing / Invoices")]
```

### Tabel Spesifikasi Endpoint

| Method | Path | Deskripsi & Fungsi | Auth / Permission | Request Body / Params | Response Payload |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/v1/health-services/billing-management/billing/invoices/{id}/refundable-items` | Mengambil daftar item tagihan yang dapat diajukan refund untuk invoice tertentu beserta tanggal transaksi | `Authorize`<br>`BillingRefund : Read` | **Path Param**:<br>- `id` (Guid): ID Invoice | **200 OK**:<br>`ApiResponse<List<BillingRefundableItemResponse>>`<br>- `billingItemId` (Guid)<br>- `itemName` (string)<br>- `qty` (decimal)<br>- `amount` (decimal)<br>- `refundableAmount` (decimal)<br>- `transactionDate` (DateTime, ISO-8601) |
| `GET` | `/v1/health-services/billing-management/billing/invoices/{id}/remaining-deposit` | Mengambil sisa saldo deposito aktif pasien yang terikat pada encounter tagihan | `Authorize`<br>`BillingRefund : Read` | **Path Param**:<br>- `id` (Guid): ID Invoice | **200 OK**:<br>`ApiResponse<RemainingDepositResponse>>`<br>- `encounterId` (Guid)<br>- `remainingDepositAmount` (decimal) |
| `POST` | `/v1/health-services/billing-management/billing/invoices/{id}/refunds` | Mengajukan refund case baru untuk tagihan, baik bersumber dari item Billing maupun Deposito | `Authorize`<br>`BillingRefund : Create`<br>**Header**:<br>`Idempotency-Key` (Guid) | **Path Param**:<br>- `id` (Guid): ID Invoice<br><br>**Body (`CreateRefundRequest`)**:<br>```json<br>{<br>  "invoiceId": "GUID",<br>  "refundableCreditId": null,<br>  "refundCategory": "BILLING" | "DEPOSITO",<br>  "selectedBillingItemIds": ["GUID", "GUID"],<br>  "requestedAmount": 200000.00,<br>  "reason": "Alasan pengajuan refund",<br>  "correlationId": "GUID",<br>  "causationId": "GUID"<br>}<br>``` | **201 Created** / **200 OK (Replay)**:<br>`ApiResponse<RefundResponse>`<br>- `id` (Guid)<br>- `invoiceId` (Guid)<br>- `refundCategory` (string)<br>- `selectedBillingItemIds` (List<Guid>)<br>- `requestedAmount` (decimal)<br>- `status` ("REQUESTED")<br>- `rowVersion` (Guid)<br>- `submittedAt` (DateTimeOffset)<br>- `isReplay` (bool) |

---

## 4. Rincian Perubahan Berkas Kode

### 4.1. Redux Slice (`billing-financial-exception-slice.jsx`)
- Menambahkan konstanta endpoint invoices: `const INVOICES_ENDPOINT = "/v1/health-services/billing-management/billing/invoices";`.
- Memperbarui thunk `createBillingRefund` agar menerima dan mengirim parameter DTO `refundCategory`, `selectedBillingItemIds`, `requestedAmount`, dan `reason` ke endpoint `POST ${INVOICES_ENDPOINT}/${encodeURIComponent(invoiceId)}/refunds` dengan header `Idempotency-Key`.
- Menambahkan dua thunk baru:
  - `getRefundableItemsByInvoice(invoiceId)`: memanggil `GET ${INVOICES_ENDPOINT}/${invoiceId}/refundable-items`.
  - `getRemainingDepositByInvoice(invoiceId)`: memanggil `GET ${INVOICES_ENDPOINT}/${invoiceId}/remaining-deposit`.
- Menambahkan fungsi normalizer:
  - `normalizeRefundableItems`: memetakan array DTO dengan dukungan camelCase dan PascalCase (`BillingItemId`, `ItemName`, `Qty`, `Amount`, `RefundableAmount`, `TransactionDate`).
  - `normalizeRemainingDeposit`: memetakan objek `EncounterId` dan `RemainingDepositAmount`.
- Menambahkan state baru pada `initialState`:
  - `refundableItemsLoading: false`, `refundableItemsError: null`, `refundableItems: []`.
  - `remainingDepositLoading: false`, `remainingDepositError: null`, `remainingDeposit: null`.
- Menambahkan `extraReducers` untuk menangani status `pending`, `fulfilled`, dan `rejected` dari kedua thunk baru tersebut.
- Mengekspor selector baru: `selectRefundableItems`, `selectRefundableItemsLoading`, `selectRefundableItemsError`, `selectRemainingDeposit`, `selectRemainingDepositLoading`, `selectRemainingDepositError`.

### 4.2. Hook (`use-billing-financial-exception.js`)
- Mengubah fungsi pembuat state awal form `buildRefundForm`:
  ```javascript
  const buildRefundForm = () => ({
    refundCategory: "BILLING",
    selectedBillingItemIds: [],
    requestedAmount: 0,
    reason: "",
  });
  ```
- Menghubungkan state `refundableItems` dan `remainingDeposit` dari Redux store via `useSelector`.
- Memperbarui fungsi `openRefund`:
  - Mereset form ke `buildRefundForm()`.
  - Mereset field errors.
  - Membuka modal (`refundOpen = true`).
  - Memanggil `dispatch(getRefundableItemsByInvoice(invoiceId))` dan `dispatch(getRemainingDepositByInvoice(invoiceId))`.
- Menambahkan handler interaksi form:
  - `calculateBillingRefundTotal`: menghitung jumlah `RefundableAmount` dari seluruh item yang ID-nya ada dalam `selectedBillingItemIds`.
  - `handleRefundCategoryChange`: menangani perpindahan radio antara "BILLING" dan "DEPOSITO". Saat beralih ke Deposito, nominal otomatis diisi dari `remainingDeposit.remainingDepositAmount`. Saat beralih ke Billing, nominal dihitung dari item tercentang.
  - `handleToggleRefundItem`: menangani centang per item di tabel dan otomatis menghitung ulang `requestedAmount`.
  - `handleSelectAllRefundItems`: menangani centang semua item sekaligus.
  - `useEffect`: sinkronisasi otomatis nilai `requestedAmount` jika data sisa deposito masuk saat modal sedang terbuka dalam mode Deposito.
- Memperbarui validasi pengajuan `confirmRefund`:
  - Memvalidasi `reason` wajib diisi dan maksimal 500 karakter.
  - Jika mode Billing: memvalidasi `selectedBillingItemIds` tidak boleh kosong (`UAT-BUI-11` / `BUI-VAL-03`).
  - Jika mode Deposito: memvalidasi sisa saldo deposito harus lebih besar dari 0 (`UAT-BUI-12` / `BUI-VAL-05`) dan nominal tidak boleh melebihi sisa deposito.
- Mengekspor handler dan selector baru pada return object hook.

### 4.3. Komponen Antarmuka (`create-refund-modal.jsx`)
- Menghapus total dropdown `<select id="refund-credit-id">` lama beserta props `refundableCredits*` (`BUI-DEC-011`).
- Mengimplementasikan radio cards pilihan sumber: "Billing" vs "Deposito".
- Pada mode Billing:
  - Merender tabel item billing lengkap dengan checkbox, Nama Item, Tanggal (`formatDate(TransactionDate)`), dan Nominal (`formatMoney(RefundableAmount)`).
  - Merender kartu ringkasan **Total Refund** otomatis di bawah tabel.
- Pada mode Deposito:
  - Merender informasi sisa deposito pasien dan nominal refund yang terkunci otomatis sama dengan sisa deposito.
  - Menampilkan peringatan informatif jika sisa saldo deposito Rp 0.
- Merender textarea alasan pengajuan refund dengan penghitung karakter *real-time*.

### 4.4. Modul Gaya (`create-refund-modal.module.css`)
- Berkas CSS module baru yang mengimplementasikan tata letak visual elegan, kartu radio pilihan sumber yang responsif dengan efek sorot aktif, tabel item tagihan dengan header *sticky*, seleksi baris berwarna halus, serta penataan tipografi nominal monospaced (*tabular-nums*).

### 4.5. Integrasi (`payment-history-view.jsx`)
- Memperbarui pemanggilan `<CreateRefundModal>` dengan mengoper props baru dari `financialException`:
  - `handleCategoryChange`
  - `handleToggleItem`
  - `handleSelectAllItems`
  - `refundableItems`, `refundableItemsLoading`, `refundableItemsError`
  - `remainingDeposit`, `remainingDepositLoading`, `remainingDepositError`
- Menghapus seluruh props `refundableCredits*` yang sudah usang.

### 4.6. Pengujian Unit (`tests/unit/create-refund-modal.test.mjs`)
- Berkas pengujian unit baru berbasis Node.js test runner bawaan yang mencakup 6 skenario verifikasi ketat:
  - `AC-1` (`UAT-BUI-06`): Memastikan komponen merender radio Billing/Deposito, tabel dengan kolom Checkbox, Nama, Tanggal, dan Nominal, serta Total Refund otomatis tanpa input teks bebas.
  - `AC-2` (`UAT-BUI-07`): Memastikan komponen merender bagian Deposito dengan sisa deposito dan nominal refund otomatis terkunci.
  - `AC-3` (`BUI-DEC-011` / `FR-BUI-011`): Memastikan dropdown Refundable Credit lama dan input textfield manual telah dihapus total.
  - `AC-4` (`UAT-BUI-11` & `UAT-BUI-12`): Memastikan hook memvalidasi penolakan submit Billing tanpa item dicentang dan Deposito dengan saldo Rp 0.
  - `AC-5` (`BUI-DEC-012`): Memastikan slice Redux memuat thunk, normalizer, dan selector baru.
  - `AC-6`: Memastikan integrasi pada `payment-history-view.jsx` terhubung dengan benar.

---

## 5. Bukti Verifikasi Kualitas

### 5.1. Pemeriksaan Linter (`npm run lint:errors`)
- **Hasil**: Lulus dengan exit code 0 (`0 error`).
- **Keterangan**: Seluruh berkas JavaScript/JSX yang dimodifikasi dan dibuat baru mematuhi aturan ESLint tanpa peringatan atau kesalahan sintaksis.

### 5.2. Pengujian Unit Komprehensif (`npm run test:unit`)
- **Hasil**: 1.695 tes lulus dari total 1.703 tes (durasi ~3,2 detik).
- **Catatan Baseline**: 8 kegagalan yang tersisa merupakan baseline pre-existing yang tidak berhubungan dengan billing kasir (mis. modul rawat inap dan radiologi warisan).
- **Pengujian Spesifik Task**:
  ```text
  # tests 6
  # suites 0
  # pass 6
  # fail 0
  # cancelled 0
  # skipped 0
  # todo 0
  # duration_ms 83.8908
  ```
  Seluruh 6 skenario pengujian `FE-BUI-007` (`tests/unit/create-refund-modal.test.mjs`) lulus 100%.

### 5.3. Kompilasi Produksi Next.js (`npm run build`)
- **Hasil**: Kompilasi berhasil penuh (`Compiled successfully`, exit code 0).
- Seluruh 406 halaman dan rute aplikasi berhasil di-generate secara statis maupun dinamis.
- Skrip `prepare-standalone.mjs` berhasil menyalin static dan public assets ke bundle standalone.

---

## 6. Matriks Pemenuhan Acceptance Criteria & Traceability

| ID Kriteria | Sumber Kebutuhan | Deskripsi Target | Status | Bukti Implementasi & Verifikasi |
| :--- | :--- | :--- | :--- | :--- |
| `UAT-BUI-06` | `BUI-DEC-012`<br>`FR-BUI-012` | Modal refund sumber Billing, dua item dicentang: total otomatis terhitung, kolom Tanggal terisi | ✅ Terpenuhi | Datatable `create-refund-modal.jsx` menampilkan kolom `Tanggal` (`formatDate(item.transactionDate)`) dari field aditif `TransactionDate`. Checkbox multi-select otomatis memicu `calculateBillingRefundTotal` untuk menampilkan `Total Refund` terkunci |
| `UAT-BUI-07` | `BUI-DEC-012`<br>`FR-BUI-012` | Modal refund sumber Deposito: nominal otomatis terisi sebesar sisa saldo deposito | ✅ Terpenuhi | Saat radio Deposito dipilih, `remainingDeposit.remainingDepositAmount` ditampilkan pada kartu info dan kotak `Nominal Refund Diajukan` otomatis terisi terkunci |
| `UAT-BUI-11` | `BUI-VAL-03`<br>`FR-BUI-012` | Modal refund sumber Billing, submit tanpa item dicentang: ditolak sebelum request terkirim | ✅ Terpenuhi | Hook `use-billing-financial-exception.js` memeriksa `selectedBillingItemIds.length === 0` dan menampilkan error `Pilih minimal satu item billing yang akan direfund.` sebelum request dikirim ke backend |
| `UAT-BUI-12` | `BUI-VAL-05`<br>`FR-BUI-012` | Modal refund sumber Deposito, sisa deposito Rp 0: submit ditolak | ✅ Terpenuhi | Hook `use-billing-financial-exception.js` memeriksa `depositBalance <= 0` dan menampilkan error `Pasien tidak memiliki sisa saldo deposito yang dapat direfund.` sebelum request dikirim ke backend |
| `BUI-DEC-011` | `FR-BUI-011` | Dropdown Refundable Credit lama dihapus total dari modal | ✅ Terpenuhi | Elemen `<select id="refund-credit-id">`, props `refundableCredits`, dan input bebas `requestedAmount` dihapus total dari `create-refund-modal.jsx` |
| `BUI-DEC-012` | Kontrak Alur | Alur persetujuan refund tidak boleh diubah atau dibedakan antar sumber | ✅ Terpenuhi | Kedua sumber tetap menggunakan endpoint yang sama `POST /{id}/refunds` dengan parameter `RefundCategory` yang sesuai tanpa membuat pintu persetujuan baru |

---

## 7. Status dan Rekomendasi

| Butir | Status / Keterangan |
| :--- | :--- |
| **Status Task** | ✅ **SELESAI (CLOSED)** — Seluruh lingkup task `FE-BUI-007` dan gelombang `MVP-33` telah selesai diimplementasikan, diverifikasi, dan lulus seluruh gerbang pengujian kualitas. |
| **Dampak Lintas Modul** | **Nol regresi.** Modul riwayat pembayaran tetap stabil, pencetakan kwitansi tidak terpengaruh, dan alur approval refund di backend tetap berjalan normal. |
| **Langkah Berikutnya** | Seluruh task frontend pada gelombang revisi UI Billing (`FE-BUI-004`, `FE-BUI-005`, `FE-BUI-006`, `FE-BUI-007`) kini telah selesai 100%. Modul siap diajukan untuk verifikasi kesiapan menyeluruh (*verify-module-readiness*). |
