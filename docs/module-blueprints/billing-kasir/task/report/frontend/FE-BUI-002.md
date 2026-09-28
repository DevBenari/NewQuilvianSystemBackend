# Laporan Perubahan Frontend — `FE-BUI-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BUI-002` |
| Judul | Penggantian Label Drug → Obat / Medicine |
| Slice | Gelombang `MVP-31` — Revisi UI Billing: Perbaikan Tampilan Kasir (`docs/module-blueprints/billing-kasir/roadmap/frontend-roadmap.md` § Gelombang UI Billing, task `FE-BUI-002`) |
| Roadmap | `docs/module-blueprints/billing-kasir/roadmap/frontend-roadmap.md` § `FE-BUI-002` — Penggantian Label Drug → Obat / Medicine |
| Trace | `BUI-DEC-003` (Penggantian Label "Drug" Menjadi "Obat / Medicine"), `FR-BUI-003`, `BUI-AC-03`, `BUI-OQ-01` (diasumsikan dan dikonfirmasi: label UI saja, master data identifier tidak diubah) |
| Contract version | Tidak berlaku — murni string UI, nol endpoint baru |
| Wewenang UI | Murni translasi / penyelarasan terminologi UI ke standar rumah sakit Indonesia. Seluruh elemen `REUSE`; tidak ada komponen baru |
| Dependency | Tidak ada — task ini independen dengan nol dependency antar sesama maupun ke gelombang lain |
| Klasifikasi | `LIGHT` — satu repository (skor 0); berkas diperiksa 12–25 (skor 1); berkas diubah 11 (skor 1); logika sederhana — normalisasi teks label dan display mapper (skor 0); kontrak API tidak berlaku / tidak berubah (skor 0); database `NOT APPLICABLE` (skor 0); keamanan/auth `NOT APPLICABLE` (skor 0); UI/workflow murni penggantian teks label statis (skor 0). Total skor 2 |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev/src/**` |
| Model | Gemini 3.8 Flash |
| Commit frontend saat dikerjakan | `b3f45db7b4bd4dafd06967f9a7cb62d41d2e0a28` (branch `yasmina`) |
| Commit backend yang dijadikan rujukan | `d6cdfaf9fda8889d6fe73db1e97cc28c4f022852` (branch `Yasmina`) |
| Tanggal | 2026-09-25 |
| Status | ✅ **SELESAI 25 September 2026.** Seluruh label UI bertuliskan "Drug" telah diganti menjadi "Obat / Medicine", termasuk normalisasi nama kategori pada tabel tagihan pasien, master data tarif, dan aturan tanggungan penjamin. Nilai data master obat (`DrugName`/`MedicineName`/`Obat`) tidak diubah dan tetap dipetakan apa adanya. Pencarian menyeluruh kata utuh `\bDrug\b` membuktikan nol label tampilan "Drug" tersisa. `npm run lint:errors` `0 error`; `npm run test:unit` 1685/1693 lulus (8 kegagalan pre-existing sama persis); `npm run build` berhasil `✓ Compiled successfully`, 406 halaman statis dan standalone runtime siap |

---

## 1. Keadaan yang ditemukan di awal

Berdasarkan audit kemampuan awal (`01-existing-capability-map.md` `CAP-BUI-03`), keputusan owner `BUI-DEC-003` mengamanatkan agar seluruh label tampilan bertuliskan "Drug" diganti menjadi "Obat / Medicine" agar selaras dengan bahasa yang dipahami pengguna rumah sakit Indonesia.

Pada pemeriksaan awal:
1. **Layar Billing dan Turunannya**:
   - Komponen tabel tagihan (`BillingInvoiceItemsTable`) merender sub-kelompok item biaya berdasarkan kategori (`groupItemsByCategory`). Jika backend atau data master mengirimkan nama kategori sebagai "Drug" (misalnya item dari instalasi farmasi), teks header kelompok tersebut sebelumnya tampil mentah sebagai "Drug".
   - Tiga layar utama (`edit-tagihan-view.jsx`, `menu-pembayaran-view.jsx`, dan `detail-invoice-billing-view.jsx`) mengelompokkan baris invoice memakai logika serupa tanpa penyeragaman label "Drug" ke "Obat / Medicine".
   - Hook `use-billing-invoice-edit-tagihan.js` dan `use-edit-billing-panel.js` hanya memeriksa flag `canEditDrugBilling` dan `drugBillingDisposition`, sedangkan backend (`BillingPayerEditDtos.cs` & `BillingPayerEditService.cs`) sudah menyediakan alias ramah `canEditMedicineBilling` dan `medicineBillingDisposition`.
2. **Master Data & Utilitas Terkait**:
   - Di Master Data Tarif (`tariff-constants.jsx` dan `tariff-utils.jsx`), dropdown dan baris detail rincian tarif menampilkan label `"Drug"` alih-alih `"Obat / Medicine"`.
   - Di Aturan Tanggungan Asuransi (`insurance-coverage-rule-constants.jsx` dan `insurance-coverage-rule-display-utils.jsx`), pilihan tipe item dan mapper tampilannya menggunakan label `"Drug"` dan `"Drug Category"`.
   - Di Aturan Tanggungan Penjamin Perusahaan (`company-guarantor-coverage-rule-utils.jsx`), mapper tipe item menampilkan `"Drug"` dan `"Drug Category"`.
   - Di halaman demo select (`select-demo-client.jsx`), heading dan label field bertuliskan `"Drug"`.

---

## 2. Proses bisnis dari sisi pengguna

1. **Kasir / Staf Billing Membuka Edit Tagihan / Menu Pembayaran / Detail Invoice**:
   - Staf kasir membuka halaman tagihan pasien.
   - Pada tabel rincian biaya yang memiliki kelompok obat dari farmasi, judul kelompok biaya kini terbaca jelas sebagai **"Obat / Medicine"** (bukan terminologi teknis "Drug").
   - Nama spesifik obat (seperti "Paracetamol 500mg", "Amoxicillin Syr") tetap tampil apa adanya dari data resep/master obat tanpa perubahan huruf atau format.
2. **Staf Administrasi Mengatur Tarif atau Aturan Tanggungan Asuransi / Penjamin**:
   - Staf membuka menu Master Data Tarif atau Master Data Aturan Tanggungan Penjamin.
   - Pada dropdown pemilihan tipe item atau informasi detail, label yang terlihat adalah **"Obat / Medicine"** (dan **"Kategori Obat / Medicine"** untuk rumpun kategori), memudahkan staf administrasi yang terbiasa dengan terminologi bilingual rumah sakit Indonesia.
3. **Jalur Tidak Normal**:
   - Apabila item tidak memiliki kategori (`categoryName` kosong/null), tabel tetap menampilkan fallback aman `"Tanpa Kategori"` (`UNCATEGORIZED_LABEL`).
   - Apabila ada data master dengan nama obat tidak lazim, fungsi pembacaan data tetap memetakan nilai dari `DrugName`, `MedicineName`, maupun `Obat` secara transparan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `src/components/view/health-services/billing-management/billing-invoices/edit-tagihan/edit-tagihan-view.jsx`
- `src/components/view/health-services/billing-management/billing-invoices/edit-tagihan/edit-billing-panel.jsx`
- `src/components/view/health-services/billing-management/billing-invoices/menu-pembayaran/menu-pembayaran-view.jsx`
- `src/components/view/health-services/billing-management/billing-invoices/detail-billing/detail-invoice-billing-view.jsx`
- `src/components/view/health-services/billing-management/billing-invoices/billing-invoice-items-table.jsx`
- `src/lib/hooks/health-services/billing-management/billing-invoices/billing-invoice-constants.js`
- `src/lib/hooks/health-services/billing-management/billing-invoices/use-billing-invoice-edit-tagihan.js`
- `src/lib/hooks/health-services/billing-management/billing-invoices/use-edit-billing-panel.js`
- `src/lib/constants/health-services/master-data/tariff-constants.jsx`
- `src/utils/health-services/master-data/tariff-utils.jsx`
- `src/lib/constants/health-services/master-data/insurance-coverage-rule-constants.jsx`
- `src/utils/health-services/master-data/insurance-coverage-rule/insurance-coverage-rule-display-utils.jsx`
- `src/utils/health-services/master-data/company-guarantor-coverage-rule/company-guarantor-coverage-rule-utils.jsx`
- `src/app/health-services/select-demo/select-demo-client.jsx`
- Backend (read-only): `Areas/HealthServices/BillingManagement/Billing/Dtos/BillingPayerEditDtos.cs` (`DrugBillingDispositionItemResponse`, `InvoiceEditCapabilitiesResponse`), `Areas/HealthServices/BillingManagement/Billing/Services/BillingPayerEditService.cs`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/health-services/billing-management/billing-invoices/edit-tagihan/edit-tagihan-view.jsx` | Pada `groupItemsByCategory`, tambahkan normalisasi: jika nama kategori adalah `"Drug"` / `"drug"`, tampilkan sebagai `"Obat / Medicine"` |
| `src/components/view/health-services/billing-management/billing-invoices/menu-pembayaran/menu-pembayaran-view.jsx` | Pada `groupItemsByCategory`, tambahkan normalisasi: jika nama kategori adalah `"Drug"` / `"drug"`, tampilkan sebagai `"Obat / Medicine"` |
| `src/components/view/health-services/billing-management/billing-invoices/detail-billing/detail-invoice-billing-view.jsx` | Pada `groupItemsByCategory`, tambahkan normalisasi: jika nama kategori adalah `"Drug"` / `"drug"`, tampilkan sebagai `"Obat / Medicine"` |
| `src/lib/hooks/health-services/billing-management/billing-invoices/use-billing-invoice-edit-tagihan.js` | Pada evaluasi `capabilities` mode Edit Billing, dukung `canEditMedicineBilling` / `medicineBillingBlockReason` di samping `canEditDrugBilling` / `drugBillingBlockReason` |
| `src/lib/hooks/health-services/billing-management/billing-invoices/use-edit-billing-panel.js` | Pada pembacaan `dispositions`, dukung `medicineBillingDisposition` / `MedicineBillingDisposition` di samping `drugBillingDisposition` / `DrugBillingDisposition` |
| `src/lib/constants/health-services/master-data/tariff-constants.jsx` | Ubah label kontrol formulir `drugId` dari `"Drug"` menjadi `"Obat / Medicine"`, dan placeholder menjadi `"Pilih obat / medicine"` |
| `src/utils/health-services/master-data/tariff-utils.jsx` | Ubah label baris detail tarif untuk kunci `"drug"` dari `"Drug"` menjadi `"Obat / Medicine"` |
| `src/lib/constants/health-services/master-data/insurance-coverage-rule-constants.jsx` | Ubah label opsi `INSURANCE_COVERAGE_ITEM_TYPE_OPTIONS` dari `"Drug"` menjadi `"Obat / Medicine"` dan `"Drug Category"` menjadi `"Kategori Obat / Medicine"`; perbarui placeholder |
| `src/utils/health-services/master-data/insurance-coverage-rule/insurance-coverage-rule-display-utils.jsx` | Pada `getItemTypeLabel`, petakan kunci `Drug` ke `"Obat / Medicine"` dan `DrugCategory` ke `"Kategori Obat / Medicine"` |
| `src/utils/health-services/master-data/company-guarantor-coverage-rule/company-guarantor-coverage-rule-utils.jsx` | Pada `getItemTypeLabel`, petakan kunci `Drug` ke `"Obat / Medicine"` dan `DrugCategory` ke `"Kategori Obat / Medicine"` |
| `src/app/health-services/select-demo/select-demo-client.jsx` | Ubah judul bagian dari `"Server-side Search: Drug"` menjadi `"Server-side Search: Obat / Medicine"`, dan label field dari `"Drug"` menjadi `"Obat / Medicine"` |

### 3.3 Kepatuhan arsitektur frontend

- **Penyelarasan Teks Tanpa Perubahan Kontrak**: Nilai enum backend (`value: "Drug"`, `value: "DrugCategory"`, nama properti DTO `DrugName`, `MedicineName`, `Obat`) **tidak diubah sama sekali**. Perubahan murni menyasar label tampilan (`label`, placeholder, display mapper).
- **Semua Elemen `REUSE`**: Tidak ada penambahan komponen base baru, tidak ada custom style baru, dan tidak ada dependensi eksternal baru. Seluruh perubahan mematuhi `rules/frontend/frontend-architecture.md` dan `rules/frontend/ui-consistency-checklist.md`.
- **Tidak Ada UI Gate Blocker**: Perubahan merupakan string translation yang dikunci langsung oleh keputusan owner (`BUI-DEC-003`).

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Komponen tabel dan dropdown menampilkan loading state bawaan masing-masing tanpa gangguan |
| Kosong | Jika tidak ada baris dalam kategori obat, pesan empty state existing tetap berlaku |
| Gagal | Alert kesalahan API / validasi tetap menampilkan pesan standar |
| Tanpa hak akses | Dibungkus `AccessDeniedGate` existing tanpa modifikasi |

---

## 5. Endpoint yang dikonsumsi

Task ini murni menyelaraskan string dan label tampilan di sisi frontend; tidak ada pemanggilan endpoint baru atau pengubahan request/response payload API backend.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Berhasil tanpa error (`0 error`) | `PASS` | Task background linting exit code 0 |
| `npm run test:unit` | 1685 lulus, 8 gagal (pre-existing, tidak terkait task ini) | `PASS` | 1693 total unit test berjalan, hasil konsisten dengan baseline |
| `npm run build` | Kompilasi Next.js berhasil penuh, 406 halaman statis tergenerasi, postbuild sukses | `PASS` | Task background build exit code 0, standalone bundle siap |
| Grep anti-regresi `git grep -n -w "Drug" src/` | Nol kemunculan teks label "Drug" tersisa sebagai tampilan UI | `PASS` | Hanya tersisa komentar kode dan pemetaan properti DTO internal |

Uji manual: `NOT APPLICABLE` — Task ini adalah penggantian label teks statis dan display mapping yang telah divalidasi penuh lewat penelusuran AST/regex kata utuh serta build & lint compilation.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Seluruh terminologi "Drug" pada tampilan Billing terbaca "Obat / Medicine" (`BUI-DEC-003`, `FR-BUI-003`) | Terpenuhi | Normalisasi pada `groupItemsByCategory` di `edit-tagihan-view.jsx`, `menu-pembayaran-view.jsx`, dan `detail-invoice-billing-view.jsx` |
| Seluruh label "Drug" berbentuk teks tampilan di luar tampilan billing (tarif, coverage rule) terganti | Terpenuhi | Pembaruan pada `tariff-constants.jsx`, `tariff-utils.jsx`, `insurance-coverage-rule-constants.jsx`, `insurance-coverage-rule-display-utils.jsx`, `company-guarantor-coverage-rule-utils.jsx`, `select-demo-client.jsx` |
| Nilai data master obat (`DrugName`/`MedicineName`/`Obat`) tidak berubah dan tetap dipetakan apa adanya | Terpenuhi | Nilai `value` enum API tidak disentuh; properti backend di-preserve di `use-edit-billing-panel.js` |
| Nol kemunculan "Drug" sebagai label tersisa di seluruh `src/` | Terpenuhi | Verifikasi `git grep -n -w "Drug" src/` membuktikan nol label tersisa |
| `npm run build` lulus | Terpenuhi | `npm run build` lulus dengan exit code 0 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada peringatan baru yang tergenerasi |
| Masalah yang diketahui | 8 unit test gagal adalah pre-existing issue di modul lain (sama persis dengan commit dasar `b3f45db7b`) |
| Dependency backend | Tidak ada dependency backend; backend sudah memiliki DTO yang kompatibel |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | 11 berkas diperbarui untuk `FE-BUI-002` (ditambah 3 berkas dari `FE-BUI-001` yang sedang berjalan di branch lokal yang sama) |
| Langkah berikutnya | Eksekusi task roadmap berikutnya di Gelombang `MVP-31` (`FE-BUI-003` atau `FE-BUI-004`) |
