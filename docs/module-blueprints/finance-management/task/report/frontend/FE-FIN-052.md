# Laporan Perubahan Frontend — `FE-FIN-052`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-052` |
| Judul | Petugas Finance mencatat dan melihat bea materai serta ongkos kirim pada Invoice Supplier, dan total yang dikirim layar sama dengan hitungan backend |
| Slice | `REV-22` (Finance AP mengikuti BLM, sisi frontend) — paket `FE-AP-01` pada dokumen konsolidasi audit |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md`, bagian "AMENDMENT ROADMAP REVISI 22", tabel Task REV-22 Frontend |
| Trace | `AP-UAT-10`; `AP-DEC-04` (diterima 26 Oktober 2026); `AUD-AP` Bagian IX butir "Biaya invoice"; `FIN-VAL-107` diperluas `BE-FIN-123` |
| Contract version | `FIN-API-1.1` §B.4 ditambah dua field aditif `stampDutyAmount` dan `shippingCostAmount` (delta kontrak `BE-FIN-123`, belum diterbitkan sebagai revisi kontrak) |
| Wewenang UI | `DEV_DISCRETION` untuk tata letak isian; mengikat untuk rumus total, penolakan nilai negatif, dan label "Bea Materai" serta "Ongkos Kirim" |
| Dependency | `BE-FIN-123` 🟡 [BE] — source ada, migration `20261026120000_AddPurchasingApBlmComponents` **belum diterapkan** ke database |
| Klasifikasi | `LIGHT` — 1 hook editor, 1 view form, 1 view detail, 1 berkas konstanta, 1 utilitas murni baru, 1 berkas unit test |
| Task mode | `FRONTEND MODE` — target tulis `QuilvianSystemFrontendDev`; backend dibaca read-only; laporan dan penandaan roadmap ditulis pada `NewQuilvianSystemBackend` |
| Target tulis | `QuilvianSystemFrontendDev` (source dan test); `NewQuilvianSystemBackend/docs/module-blueprints/finance-management/` (laporan dan roadmap saja) |
| Model | Claude Sonnet 5.5 |
| Commit frontend saat dikerjakan | `7257982bb` (branch `yasmina`, perubahan belum di-commit) |
| Commit backend yang dijadikan rujukan | `cfe22fe8` (branch `Yasmina`; perubahan `BE-FIN-127` belum di-commit) |
| Tanggal | 26 Oktober 2026 |
| Status | 🟡 **SEBAGIAN** — source selesai, lint dan unit test lulus; build dan uji manual di browser belum dijalankan |

---

## 1. Keadaan yang ditemukan di awal

1. Backend sudah menerima dan mengembalikan `stampDutyAmount` dan `shippingCostAmount` (`BE-FIN-123`) dan menghitung total dengan rumus
   `Subtotal − Diskon + PPN + Materai + Ongkir − Uang Muka − Potongan Lain`.
2. `use-purchasing-invoice-editor.jsx` tidak punya state, tidak memuat, tidak menghitung, dan tidak mengirim kedua field itu. Rumus total di layar
   masih rumus lama tanpa materai dan ongkir.
3. **Risiko kehilangan data.** Backend mengganti seluruh nilai pada `PUT` (field yang tidak dikirim menjadi 0). Karena itu, membuka invoice
   yang sudah berbiaya lalu menyimpannya lewat editor lama akan menyimpan materai/ongkir 0 dan **menurunkan nilai invoice tanpa pesan galat**
   (total yang dikirim layar juga tanpa biaya, sehingga lolos pemeriksaan keseimbangan backend).
4. `purchasing-invoice-detail-view.jsx` hanya menampilkan Subtotal, Diskon, PPN, dan Uang Muka. **Potongan Lain tidak pernah ditampilkan**,
   padahal ikut membentuk total.

---

## 2. Proses bisnis dari sisi pengguna

Pengguna: staf Finance yang menyusun Purchasing Invoice dari Tukar Faktur (hak `FinancePurchasingInvoice : Create` / `Update`).

1. Staf membuka **Transaksi A/P → Invoice & Pengakuan Utang → Invoice Supplier**, memilih **+ Susun Purchasing Invoice** (atau **Perbarui** pada invoice berstatus Draf).
2. Staf mengisi rincian item, lalu isian **Diskon, PPN, Bea Materai, Ongkos Kirim, Uang Muka, Potongan Lain**.
3. Kotak **Total Purchasing Invoice** dihitung otomatis dan tidak dapat diketik. Contoh: Subtotal Rp 900.000, PPN Rp 99.000, materai Rp 10.000,
   ongkir Rp 15.000, tanpa diskon/uang muka/potongan → total Rp 1.024.000.
4. Staf menekan **Simpan** / **Perbarui**. Layar mengirim seluruh komponen bersama total.
5. **Jalur tidak normal:**
   - Materai atau ongkir bernilai negatif → pesan "Bea materai tidak boleh negatif." / "Ongkos kirim tidak boleh negatif." tampil di layar, tidak ada permintaan dikirim.
   - Rincian lama gagal dimuat saat **Perbarui** → pesan "Rincian Purchasing Invoice belum termuat. Muat ulang halaman sebelum menyimpan." Simpan ditolak di layar, sehingga biaya tersimpan tidak tertimpa 0.
   - Backend menolak (mis. total tidak seimbang `422`) → pesan backend tampil di kotak galat form.
6. Pada halaman **detail**, staf melihat Subtotal, Diskon, PPN, Bea Materai, Ongkos Kirim, Uang Muka, Potongan Lain, dan Total.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` frontend; `rules/frontend/*` (arsitektur, katalog komponen, gerbang keputusan komponen, pola halaman, token desain, kebijakan test, checklist konsistensi UI);
`use-purchasing-invoice-editor.jsx`, `purchasing-invoice-form-view.jsx`, `purchasing-invoice-detail-view.jsx`, `purchasing-invoice-constants.jsx`;
backend `FinancePurchasingInvoicesController.cs`, `FinancePurchasingInvoiceDtos.cs`, `FinancePurchasingInvoiceService.cs` (`CreateAsync`, `UpdateAsync`, `ValidateCharges`, `ValidateBalance`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/finance/purchasing/purchasing-invoice-utils.jsx` (**baru**) | Fungsi murni: `calculatePurchasingInvoiceSubtotal`, `calculatePurchasingInvoiceTotal` (rumus BE-FIN-123, dibulatkan 2 desimal seperti pembanding backend), `validatePurchasingInvoiceCharges`, `toInvoiceChargeFormValues` (field biaya yang tidak dikirim dibaca 0) |
| `src/lib/hooks/finance/purchasing/use-purchasing-invoice-editor.jsx` | State `stampDutyAmount` dan `shippingCostAmount`; dimuat pada mode update; ikut rumus total; ikut payload create dan update; validasi non-negatif; penolakan simpan bila `expectedRowVersion` belum ada pada mode update; komentar kontrak diperbarui |
| `src/components/view/finance/purchasing/purchasing-invoice/purchasing-invoice-form-view.jsx` | Isian **Bea Materai** dan **Ongkos Kirim** (tata letak 3 kolom x 2 baris mengikuti urutan rumus); label rumus total dan deskripsi halaman diperbarui |
| `src/components/view/finance/purchasing/purchasing-invoice/detail/purchasing-invoice-detail-view.jsx` | Menampilkan Bea Materai, Ongkos Kirim, dan Potongan Lain (yang sebelumnya tidak tampil) |
| `src/lib/constants/finance/purchasing/purchasing-invoice-constants.jsx` | Komentar kontrak diperbarui (tidak lagi menyatakan backend tanpa daftar/cancel) |
| `tests/unit/finance-purchasing-ap-blm.test.mjs` (**baru**) | 7 test untuk task ini (rumus total, pembulatan, nilai string/kosong, subtotal, validasi negatif, prefill biaya); sisanya milik `FE-FIN-053` dan `FE-FIN-055` |

### 3.3 Kepatuhan arsitektur frontend

**UI GATE: 2 elemen — REUSE 2, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0** (isian angka: markup `input.form-control` + `form-label small fw-semibold` yang sama dengan Diskon/PPN pada berkas yang sama; kotak total: markup kotak Subtotal/Total yang sudah ada).

Alur `view → hook → util` dipertahankan; perhitungan dipindahkan dari hook ke utilitas murni di `src/utils/finance/purchasing/` (aturan "fungsi transformasi domain tidak di komponen").
Isian mengikuti markup yang sudah ada pada baris yang sama (`input.form-control` dengan label `form-label small fw-semibold`) supaya tampilan tidak berbeda dari Diskon/PPN di sebelahnya;
migrasi seluruh form ke `BaseTextField` berada di luar cakupan. Tidak ada CSS, token, atau komponen baru.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Spinner "Memuat Purchasing Invoice..." (mode Perbarui) |
| Kosong | Nilai biaya yang tidak dikirim backend tampil `0` |
| Gagal | Kotak galat form berisi pesan backend, atau pesan validasi layar (negatif / rincian belum termuat) |
| Tanpa hak akses | `AccessDeniedGate` menggantikan halaman bila backend menjawab akses ditolak |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Finance Management / Purchasing / Purchasing Invoice

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/finance-management/purchasing/purchasing-invoices/{id}` | Memuat rincian dan biaya tersimpan (prefill Perbarui, halaman detail) | `FinancePurchasingInvoice : Read` |
| `POST` | `/v1/corporate/finance-management/purchasing/purchasing-invoices` | Menyusun invoice dengan `stampDutyAmount` dan `shippingCostAmount` | `FinancePurchasingInvoice : Create` |
| `PUT` | `/v1/corporate/finance-management/purchasing/purchasing-invoices/{id}` | Memperbarui invoice (Draf) dengan seluruh komponen biaya dan `expectedRowVersion` | `FinancePurchasingInvoice : Update` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` atas 28 berkas yang diubah/ditambah (gabungan `REV-22` frontend) | 0 error; 8 warning (`react-hooks/set-state-in-effect` x7, `react-hooks/refs` x1) | `PASS` untuk error; warning `EXISTING WARNING` | Aturan dan pola yang sama muncul pada hook sejenis yang **tidak diubah** (`use-invoice-exchange-list.jsx`, `use-purchase-order-number-resolver.jsx`, `use-supplier-return-deposit-list.jsx`); lima warning berada pada baris `useEffect(() => fetch…)` lama |
| `node --import ./tests/helpers/register.mjs --test tests/unit/finance-purchasing-ap-blm.test.mjs` | 16 test, 16 lulus (7 milik task ini) | `PASS` | Keluaran perintah |
| `npm run test:unit` | Skrip npm gagal: "Could not find `…\tests\unit\*.test.mjs`" (Node 20.20.2 tidak meng-expand glob di Windows) | `EXISTING / ENVIRONMENT ISSUE` | Keluaran perintah |
| Seluruh suite dengan glob yang di-expand shell | 3851 test: 3809 lulus, 42 gagal pada 28 berkas test CRM, bangsal/pra-operasi, hemodialisa, rawat inap, IGD, dan menu | `UNRELATED EXISTING ISSUE` (tanpa pembanding baseline) | Tidak satu pun dari 28 berkas itu memuat rujukan ke berkas purchasing yang diubah. Baseline bersih **tidak dijalankan ulang** karena `git stash`/`checkout` dilarang `AGENTS.md` |
| `npm run build` | Tidak dijalankan | `NOT RUN` | Instruksi pengguna: build dilakukan sendiri; juga menimpa `.next` yang dipakai server lokal |
| Uji manual di browser (isian materai/ongkir, prefill Perbarui, total, simpan) | Tidak dijalankan | `NOT FEASIBLE` | Dev server dan skrip browser tidak dinyalakan tanpa diminta; kolom `StampDutyAmount`/`ShippingCostAmount` juga belum ada di database sampai migration `BE-FIN-123` diterapkan |

Uji manual: `NOT FEASIBLE`.

**Tidak dijalankan:** `npm run build`, `npm run dev`, Playwright, `npm run test:e2e` — alasan pada tabel di atas.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Form Susun dan Perbarui menampilkan Bea Materai dan Ongkos Kirim; total dihitung otomatis sesuai rumus backend | Terpenuhi (source) | `purchasing-invoice-form-view.jsx`; `calculatePurchasingInvoiceTotal`; test "total menambahkan bea materai dan ongkos kirim" |
| 2. Membuka Perbarui pada invoice berbiaya memuat nilai tersimpan dan mengirimnya kembali, total tidak turun | Terpenuhi (source) | `use-purchasing-invoice-editor.jsx` (`toInvoiceChargeFormValues`, payload); test "edit invoice lama memuat biaya tersimpan…" |
| 3. Materai/ongkir negatif dicegah di layar | Terpenuhi (source) | `validatePurchasingInvoiceCharges`; test "materai dan ongkir negatif ditolak" |
| 4. Detail menampilkan Bea Materai, Ongkos Kirim, dan Potongan Lain | Terpenuhi (source) | `purchasing-invoice-detail-view.jsx` |
| 5. Simpan pada mode Perbarui ditolak bila rincian lama belum termuat | Terpenuhi (source) | `handleSubmit` memeriksa `expectedRowVersion` |
| DoD: lint PASS, laporan tracked | Terpenuhi untuk error lint (0); 8 warning pola lama | Bagian 6 |
| DoD: build PASS, uji manual runtime | **Belum terpenuhi** | `NOT RUN` / `NOT FEASIBLE` |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Delta kontrak `FIN-VAL-107` (rumus total) dan dua field aditif belum diterbitkan sebagai revisi kontrak; frontend mengikuti source backend |
| Masalah yang diketahui | Form ini masih memakai `input.form-control` dan tabel Bootstrap pola lama modul Purchasing; tidak dimigrasikan ke `BaseTextField`/`DataTable` karena di luar cakupan |
| Dependency backend | `BE-FIN-123` 🟡: sampai migration diterapkan dan backend di-build, `POST`/`PUT` dengan kolom baru dapat gagal di lingkungan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat `FE-FIN-054` (satu `git status --short` untuk seluruh `REV-22` frontend); tidak ada stage, commit, atau push |
| Langkah berikutnya | Terapkan migration `BE-FIN-123`, jalankan `npm run build`, lalu uji manual: susun invoice dengan materai dan ongkir, buka Perbarui, simpan, dan bandingkan total pada detail |
