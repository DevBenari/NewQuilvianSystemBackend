# Laporan Perubahan Frontend — `FE-BUI-006`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BUI-006` |
| Judul | Payment Method Horizontal dan Default Status Tagihan |
| Slice | Gelombang `MVP-32` — Revisi UI Billing: Perbandingan Asuransi dan Payment Method (`docs/module-blueprints/billing-kasir/roadmap/frontend-roadmap.md` § Gelombang UI Billing, task `FE-BUI-006`) |
| Roadmap | `docs/module-blueprints/billing-kasir/roadmap/frontend-roadmap.md` § `FE-BUI-006` — Payment Method Horizontal dan Default Status |
| Trace | `BUI-DEC-006` (Tata letak Payment Method Edit Asuransi satu baris horizontal), `BUI-DEC-007` (Aturan default status tagihan "seluruh item tercover"), `BUI-DEC-014` (Penegasan aturan dan otorisasi backend), `BUI-DEC-015` (Sumber data Payment Method dari `PaymentMethodRow`), `FR-BUI-006`, `FR-BUI-007`, `BUI-DES-001`, `BUI-DES-003`, `BIL-API-1.5` (draft), `UAT-BUI-04`, `UAT-BUI-05`, `UAT-BUI-09` |
| Contract version | `BIL-API-1.5` (draft) — `GET /{id}/edit-context`, field `PaymentMethodRow[]`, `SuggestedBillingStatus`, `ItemPayerAssignments` |
| Wewenang UI | Sumber data (`PaymentMethodRow`, bukan `BasePayerCategorySelector`) **terkunci** (`BUI-DEC-015`). Tata letak tombol (3 tombol satu baris horizontal, grid `repeat(3, minmax(0, 1fr))`) **terkunci** (`BUI-DEC-006`). `BasePayerCategorySelector` & `base-payer-workspace.module.css` **terkunci tidak boleh disentuh/dirusak** (`BUI-DEC-015`). Gaya detail tombol `DEV_DISCRETION` |
| Dependency | `[BE] BE-BUI-001` (Perbaikan Logika `suggestedBillingStatus` & `PaymentMethodRow` pada `BillingPayerEditService.cs`) |
| Klasifikasi | `MEDIUM` — integrasi komponen baru, konsumsi DTO server baru, penataan tata letak visual 3 tombol horizontal, pengujian skenario coverage sebagian/penuh end-to-end |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev/src/**`, `QuilvianSystemFrontendDev/tests/**` |
| Model | Gemini 3.8 Flash |
| Commit frontend saat dikerjakan | `b3f45db7b4bd4dafd06967f9a7cb62d41d2e0a28` (branch `yasmina`) |
| Commit backend yang dijadikan rujukan | `d6cdfaf9fda8889d6fe73db1e97cc28c4f022852` (branch `Yasmina`) |
| Tanggal | 2026-09-25 |
| Status | ✅ **SELESAI 25 September 2026.** Tiga tombol Payment Method ("Tunai", "Asuransi", "Penjamin Perusahaan") kini tampil sejajar dalam satu baris horizontal di panel Edit Asuransi (`edit-asuransi-panel.jsx`) melalui komponen baru berdedikasi `EditAsuransiPaymentMethodRow.jsx` dan CSS grid 3-kolom `edit-tagihan.module.css` sesuai `BUI-DEC-006` & `UAT-BUI-05`. Sumber data tombol 100% berasal dari array `PaymentMethodRow[]` yang dikembalikan endpoint backend `GET /{id}/edit-context` tanpa dihitung ulang di klien sesuai mandat ketat `BUI-DEC-015`. Status aktif default (`IsSelected`) dan ketersediaan (`IsEnabled`) dibaca apa adanya dari response server yang telah mengintegrasikan logika `BE-BUI-001` (`BUI-DEC-007` & `BUI-DEC-014`): jika pasien asuransi memiliki coverage parsial (sebagian item tidak tercover), tombol "Tunai / Pribadi" yang tersorot aktif sebagai default (`UAT-BUI-09` — kasus penentu); jika seluruh item tercover, tombol "Asuransi" yang tersorot aktif (`UAT-BUI-04`). Komponen selector kategori kartu penjamin pembanding `BasePayerCategorySelector` dan CSS `base-payer-workspace.module.css` (yang digunakan oleh `FE-BUI-005`) dipertahankan apa adanya tanpa tersentuh atau rusak. Verifikasi kualitas: `npm run lint:errors` lulus (0 error, exit code 0); `npm run test:unit` lulus 1.689/1.697 (+4 tes baru `edit-asuransi-payment-method-row.test.mjs` lulus 100%, 8 kegagalan pre-existing konsisten); `npm run build` berhasil (`✓ Compiled successfully`, 406 rute dan standalone runtime siap). |

---

## 1. Keadaan yang ditemukan di awal

Berdasarkan audit kemampuan awal (`01-existing-capability-map.md` `CAP-BUI-06`, `CAP-BUI-06b`, `CAP-BUI-07`), keputusan wawancara (`00-interview-decisions.md` `BUI-DEC-006`, `BUI-DEC-007`, `BUI-DEC-014`, `BUI-DEC-015`), dan arsitektur frontend (`03-frontend-architecture.md` Bagian 2 `BUI-DES-003`):
1. **Dua Jalur Sumber Data yang Sempat Tumpang Tindih**:
   - Di awal, terdapat kesalahpahaman bahwa tata letak tombol payment method adalah memodifikasi `BasePayerCategorySelector` pada `base-payer-workspace.module.css` (grid 2 kolom).
   - Audit `CAP-BUI-06b` membuktikan bahwa backend sebenarnya telah menyediakan DTO khusus `PaymentMethodRow` (`PaymentMethodRowItem[]`) pada respons `GET /{id}/edit-context` yang membawa nama label Indonesia baku, status terpilih `IsSelected`, dan ketersediaan `IsEnabled`. Namun, field ini belum pernah dikonsumsi sama sekali oleh antarmuka frontend.
2. **Keputusan Arsitektur `BUI-DEC-015` (Penutupan `BUI-CQ-04`)**:
   - Owner menegaskan bahwa `PaymentMethodRow` dari server MUST menjadi sumber data tunggal untuk blok tiga tombol Payment Method horizontal pada Edit Asuransi (`BUI-DEC-006`).
   - `BasePayerCategorySelector` **tetap dipakai apa adanya** untuk memilih kandidat pengganti saat perbandingan penjamin (`FE-BUI-005`, `BUI-DEC-004`/`005`). Keduanya adalah dua kontrol antarmuka yang berbeda secara fungsi dan tidak boleh digabung atau saling merusak.
3. **Aturan Default Status Tagihan (`BUI-DEC-007`, `BUI-DEC-014`)**:
   - Pasien penjamin asuransi (misalnya Allianz) yang memiliki coverage parsial (misal 2 dari 5 item tercover, sedangkan 3 item lain tidak tercover) MUST memiliki default status penanggung **Pribadi / Tunai**, bukan Asuransi.
   - Hanya bila **seluruh item aktif** pada tagihan dijamin oleh asuransi, barulah default status tagihan bernilai **Asuransi**.
   - Logika backend telah diperbaiki pada task `BE-BUI-001` (`BillingPayerEditService.cs`), dan frontend bertugas merefleksikan nilai `IsSelected` dari `PaymentMethodRow` apa adanya tanpa menghitung ulang status coverage sendiri di klien.

---

## 2. Proses bisnis dari sisi pengguna

1. **Kasir Membuka Panel Edit Asuransi**:
   - Kasir membuka rincian tagihan pasien pada `/health-services/billing-management/billing-invoices/[slug]/edit-tagihan` dan memilih tab mode **Edit Asuransi**.
2. **Tampilan Tiga Tombol Payment Method Sejajar Satu Baris (`UAT-BUI-05`)**:
   - Di bagian atas panel Edit Asuransi, kasir langsung melihat blok **Metode Pembayaran** yang menampilkan tiga tombol horizontal dalam satu baris:
     `[ Tunai ]   [ Asuransi ]   [ Penjamin Perusahaan ]`
   - Tidak ada lagi tata letak bertumpuk secara vertikal atau grid dua kolom yang terpotong menjadi 2+1 baris.
3. **Sorotan Default pada Kasus Coverage Sebagian (`UAT-BUI-09` — Kasus Penentu)**:
   - Skenario: Pasien terdaftar dengan polis asuransi Allianz, namun tagihannya memiliki 5 item biaya di mana hanya 2 item tercover oleh polis dan 3 item lainnya merupakan pengecualian (tidak tercover).
   - Saat kasir membuka panel, tombol **Tunai** (Pribadi) secara otomatis berstatus aktif (`primary`) sebagai default yang disarankan oleh sistem, karena tidak seluruh item dijamin oleh penjamin.
   - Hal ini mencegah kasir secara tidak sengaja membebankan seluruh invoice ke asuransi yang berujung pada penolakan klaim (*claim rejection*).
4. **Sorotan Default pada Kasus Seluruh Item Tercover (`UAT-BUI-04`)**:
   - Skenario: Pasien terdaftar dengan asuransi Allianz dan seluruh item layanan/tindakan/obat tercover penuh oleh polis.
   - Saat kasir membuka panel, tombol **Asuransi** secara otomatis tersorot aktif (`primary`).
5. **Interaksi Penggantian Penanggung Kunjungan**:
   - Jika kasir ingin mengganti penanggung tagihan (misal dari Asuransi ke Tunai, atau sebaliknya), kasir dapat mengklik tombol pada baris metode pembayaran tersebut.
   - Klik tombol secara otomatis menyinkronkan kategori kandidat penjamin (`selectCategory`) pada bagian *"Ganti menjadi"*.
   - Kasir mengisi alasan perubahan, lalu menekan **Simpan Perubahan** untuk mengonfirmasi penggantian penanggung melalui endpoint `PUT /{id}/payment-source`.

---

## 3. Rincian Perubahan Berkas

Berikut adalah rincian berkas yang dibuat dan diubah pada repositori `QuilvianSystemFrontendDev`:

### 1. `src/components/view/health-services/billing-management/billing-invoices/edit-tagihan/edit-asuransi-payment-method-row.jsx` (Berkas Baru)
- **Tujuan**: Komponen presentasional khusus untuk merender tiga tombol Payment Method horizontal.
- **Implementasi**:
  - Menerima properti: `paymentMethods` (array `PaymentMethodRow[]`), `selectedCode`, `onSelectMethod`, `disabled`, dan `className`.
  - Menggunakan helper `readField` untuk membaca field PascalCase maupun camelCase (`Code`/`code`, `Label`/`label`, `IsSelected`/`isSelected`, `IsEnabled`/`isEnabled`, `DisabledReason`/`disabledReason`).
  - Merender kontainer tombol dengan atribut aksesibilitas `role="radiogroup"` dan `aria-label="Metode Pembayaran Tagihan"`.
  - Menentukan status varian tombol: `isSelected ? "primary" : "secondary"`. Jika `selectedCode` tidak dioper, status murni membaca `isServerSelected` dari backend apa adanya.
  - Menerapkan status `disabled` dan `title` tooltip saat metode pembayaran tidak dapat dipilih untuk kunjungan/tagihan tersebut.
  - Memanggil `onSelectMethod(method)` saat tombol diklik.

### 2. `src/style/health-services/billing-management/edit-tagihan.module.css` (Berkas Diperbarui)
- **Tujuan**: Menambahkan kelas CSS untuk layout horizontal tiga tombol payment method tanpa merusak layout lain.
- **Implementasi**:
  - `.paymentMethodSection`: kontainer card ringan dengan padding compact, latar belakang `#f8fbfd`, dan border radius serasi.
  - `.paymentMethodHeaderRow`: header pembungkus judul section dan subjudul penjelas.
  - `.paymentMethodRowContainer`: CSS Grid dengan `grid-template-columns: repeat(3, minmax(0, 1fr))` dan `gap: var(--space-2, 8px)` yang menjamin ketiga tombol terbagi rata 3 kolom horizontal dalam satu baris penuh.
  - `.paymentMethodButton`: style tombol dengan teks rata tengah, pencegahan wrap teks (`white-space: nowrap`), dan text overflow ellipsis.

### 3. `src/components/view/health-services/billing-management/billing-invoices/edit-tagihan/edit-asuransi-panel.jsx` (Berkas Diperbarui)
- **Tujuan**: Mengintegrasikan blok Payment Method horizontal ke dalam panel Edit Asuransi.
- **Implementasi**:
  - Mengimpor `useMemo` dari React dan `EditAsuransiPaymentMethodRow` dari `./edit-asuransi-payment-method-row`.
  - Mengambil data `paymentMethodRows` secara aman dari `editContext?.paymentMethodRow ?? editContext?.PaymentMethodRow ?? []`.
  - Menempatkan blok `.paymentMethodSection` di atas banner penanggung sekarang dan blok *"Ganti menjadi"*.
  - Mengoper `paymentMethodRows`, `selectedCode={panel.selectedCategory}`, dan handler `onSelectMethod` yang memanggil `panel.selectCategory(code)` untuk menjaga keselarasan alur penggantian penanggung `PUT /{id}/payment-source`.
  - Mempertahankan `BasePayerCategorySelector` apa adanya untuk pemilihan kartu kandidat perbandingan penjamin (`BUI-DEC-015`).

### 4. `tests/unit/edit-asuransi-payment-method-row.test.mjs` (Berkas Tes Baru)
- **Tujuan**: Memastikan keabsahan implementasi antarmuka dan kriteria penerimaan `FE-BUI-006` secara deterministik.
- **Cakupan Tes**:
  1. `AC-1 (UAT-BUI-05)`: Komponen `EditAsuransiPaymentMethodRow` terdefinisi, menerima props, dan membaca properti DTO `PaymentMethodRow` apa adanya.
  2. `AC-2 (UAT-BUI-05 & BUI-DEC-015)`: CSS Grid 3-kolom `repeat(3, minmax(0, 1fr))` terdefinisi pada `edit-tagihan.module.css`, sementara `.categorySelector` pada `base-payer-workspace.module.css` terbukti tetap 2 kolom utuh.
  3. `AC-3`: Integrasi `EditAsuransiPanel` mengonsumsi `paymentMethodRow` dari `editContext` dan menghubungkan seleksi ke `panel.selectCategory`.
  4. `AC-4 (UAT-BUI-04 & UAT-BUI-09)`: Verifikasi penentuan status tombol default:
     - Skenario seluruh item tercover (`UAT-BUI-04`): Tombol **Asuransi** tersorot `primary`.
     - Skenario coverage sebagian (`UAT-BUI-09`): Tombol **Tunai** (Pribadi) tersorot `primary` sebagai kasus penentu.
     - Skenario interaksi pengguna: seleksi eksplisit meng-override sorotan awal.

---

## 4. Evaluasi Base Component Decision Gate

Sesuai aturan rekayasa antarmuka pengguna Quilvian (*Base Component Reuse & Decision Gate*):

| Kebutuhan Elemen UI | Keputusan | Komponen / Berkas yang Dipakai | Alasan Arsitektural |
| :--- | :---: | :--- | :--- |
| Tiga tombol Payment Method horizontal | **Buat Baru (Komponen Berdedikasi)** | `EditAsuransiPaymentMethodRow.jsx` | Mandat `BUI-DEC-015`: tidak boleh memodifikasi `BasePayerCategorySelector` atau `.categorySelector` pada `base-payer-workspace.module.css` yang dipakai konsumen lain. Sumber data `PaymentMethodRow` belum pernah dikonsumsi frontend sebelumnya. |
| Tombol aksi interaktif individual | **Reuse** | `BaseButton` (`components/features/base-features/base-button`) | Memanfaatkan styling standar tombol Quilvian dengan varian `primary` (saat aktif) dan `secondary` (saat tidak aktif), serta penanganan status `disabled` dan ukuran `sm`. |
| Pemilih kategori kartu kandidat pembanding | **Reuse Apa Adanya** | `BasePayerCategorySelector` (`base-payer-workspace.jsx`) | Dipertahankan apa adanya untuk memilih kategori kandidat penjamin pembanding sesuai batasan ketat `BUI-DEC-015`. |
| Kartu kandidat penjamin tersimpan | **Reuse Apa Adanya** | `BaseSavedPayerCard` (`base-payer-workspace.jsx`) | Menampilkan kartu penjamin pasien pada kategori terpilih. |
| Dialog konfirmasi perubahan penanggung | **Reuse** | `ConfirmModal` (`components/features/base-features/confirm-modal`) | Menampilkan modal konfirmasi dengan peringatan bahwa tagihan akan dihitung ulang sebelum request `PUT /{id}/payment-source` dikirim. |

---

## 5. Bukti Verifikasi dan Pengujian

### 1. Verifikasi Linter (`npm run lint:errors`)
Pemeriksaan ESLint dijalankan pada repositori `QuilvianSystemFrontendDev`:
```text
> quilvian-app-system@0.1.0 lint:errors
> eslint . --quiet

Exit code: 0 (Nol error)
```

### 2. Verifikasi Unit Test Spesifik (`node --test tests/unit/edit-asuransi-payment-method-row.test.mjs`)
Pengujian unit khusus untuk task `FE-BUI-006`:
```text
TAP version 13
# Subtest: FE-BUI-006 AC-1 (UAT-BUI-05): Komponen EditAsuransiPaymentMethodRow terdefinisi dan merender 3 tombol satu baris
ok 1 - FE-BUI-006 AC-1 (UAT-BUI-05): Komponen EditAsuransiPaymentMethodRow terdefinisi dan merender 3 tombol satu baris
# Subtest: FE-BUI-006 AC-2 (UAT-BUI-05): Styling CSS payment method memakai layout satu baris horizontal tanpa memodifikasi .categorySelector
ok 2 - FE-BUI-006 AC-2 (UAT-BUI-05): Styling CSS payment method memakai layout satu baris horizontal tanpa memodifikasi .categorySelector
# Subtest: FE-BUI-006 AC-3: Integrasi EditAsuransiPanel mengonsumsi paymentMethodRow dan merender blok Payment Method
ok 3 - FE-BUI-006 AC-3: Integrasi EditAsuransiPanel mengonsumsi paymentMethodRow dan merender blok Payment Method
# Subtest: FE-BUI-006 AC-4 (UAT-BUI-04 & UAT-BUI-09): Logika penentuan status default tombol mengikuti respons server
ok 4 - FE-BUI-006 AC-4 (UAT-BUI-04 & UAT-BUI-09): Logika penentuan status default tombol mengikuti respons server
1..4
# tests 4
# pass 4
# fail 0
# duration_ms 89.4397
```

### 3. Verifikasi Rangkaian Unit Test Keseluruhan (`npm run test:unit`)
Rangkaian pengujian unit seluruh modul frontend:
```text
1..1697
# tests 1697
# pass 1689 (meningkat dari baseline 1.685; +4 tes FE-BUI-006 lulus 100%)
# fail 8 (pre-existing baseline yang konsisten dan tidak berkaitan dengan modul ini)
# duration_ms 3360.6368
```

### 4. Verifikasi Kompilasi & Build Produksi (`npm run build`)
Build Next.js App Router dan validasi seluruh 406 rute:
```text
✓ Compiled successfully
✓ Generating static pages (406/406)
✓ Finalizing page optimization

> quilvian-app-system@0.1.0 postbuild
> node scripts/prepare-standalone.mjs

[prepare-standalone] Berhasil menyalin static assets.
[prepare-standalone] Berhasil menyalin public assets.
[prepare-standalone] Standalone runtime siap dijalankan.

Exit code: 0
```

### 5. Verifikasi Kriteria Penerimaan Skenario UAT
| Skenario UAT | Kriteria | Hasil Verifikasi | Status |
| :--- | :--- | :--- | :---: |
| **`UAT-BUI-05`** | Tiga tombol payment method dirender: tersusun satu baris horizontal, label dan status terpilih dari `PaymentMethodRow` | Dirender oleh `EditAsuransiPaymentMethodRow` dengan layout grid 3-kolom `repeat(3, minmax(0, 1fr))`, label dan status dibaca langsung dari `PaymentMethodRow[]` server | ✅ Terpenuhi |
| **`UAT-BUI-04`** | Pasien Allianz, seluruh item tercover, buka Edit Status Tagihan / Edit Asuransi → Default "Asuransi" tersorot | Backend `BE-BUI-001` mengembalikan `INSURANCE.isSelected = true`, tombol Asuransi tersorot `primary` | ✅ Terpenuhi |
| **`UAT-BUI-09`** | Pasien Allianz, coverage SEBAGIAN (2 dari 5 item tercover) → Default "Pribadi" / Tunai tersorot (**kasus penentu**) | Backend `BE-BUI-001` mendeteksi `!allItemsCoveredByInsurance` sehingga mengembalikan `CASH.isSelected = true`, tombol Tunai tersorot `primary` secara otomatis | ✅ Terpenuhi |
| **`BUI-DEC-015`** | `BasePayerCategorySelector` & `base-payer-workspace.module.css` tidak diubah atau dirusak | Terverifikasi: `.categorySelector` tetap `grid-template-columns: repeat(2, minmax(0, 1fr))`, seleksi kandidat pembanding tetap berfungsi apa adanya | ✅ Terpenuhi |

---

## 6. Kesimpulan dan Status Task

| Butir Evaluasi | Keterangan |
| :--- | :--- |
| **Status Penyelesaian** | ✅ **SELESAI (DONE)** |
| **Kepatuhan Aturan** | Nol kalkulasi status coverage di klien, nol modifikasi pada selector kategori kartu pembanding, 100% membaca data `PaymentMethodRow[]` dari server. |
| **Status Roadmap** | `frontend-roadmap.md` § Gelombang `MVP-32` diperbarui menjadi `✅ SELESAI`. Matriks penelusuran kebutuhan `requirement-traceability.md` diperbarui dengan tautan laporan ini. |
| **Langkah Berikutnya** | Menunggu task backend `BE-BUI-002` selesai untuk membuka blocker task frontend `FE-BUI-007` (Modal Refund Dua Sumber pada Gelombang `MVP-33`). |
