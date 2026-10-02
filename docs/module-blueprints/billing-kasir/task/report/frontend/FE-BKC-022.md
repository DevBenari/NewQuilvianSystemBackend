# Laporan Perubahan Frontend — `FE-BKC-022`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BKC-022` |
| Judul | Struk Pasien: breakdown, Penjamin, tanda tangan |
| Slice | Susulan gelombang Dokumen Kasir (`FE-BKC-011`/`017`/`018`) — lihat `roadmap/frontend-roadmap.md` § "Amendment 8 September 2026 — `FE-BKC-022` dibuka kembali sebagai `READY_FOR_TASK_APPROVAL`" |
| Roadmap | `docs/module-blueprints/billing-kasir/roadmap/frontend-roadmap.md` § 3 pada amendment yang sama |
| Trace | `BKC-DEC-093` (breakdown), `BKC-DEC-094` (field Penjamin, dengan koreksi bukti — lihat § 1 Keadaan yang ditemukan di awal), `BKC-DEC-096` (tanda tangan); `BKC-DEC-095` (QR) sengaja TIDAK ikut task ini |
| Contract version | Tidak ada kontrak baru. `GET /billing/invoices/{id}/calculation-preview` (dipakai apa adanya, sudah dikonsumsi Menu Pembayaran) dan `InvoiceDetailResponse.Patient.GuarantorName`/`PaymentType` (sudah ada pada `GET /billing/invoices/{id}`) |
| Wewenang UI | `DEV_DISCRETION` untuk layout blok tanda tangan (garis kosong + label, mengikuti pola `kwitansi-document.jsx`) dan lokasi/nama file utilitas breakdown yang diekstrak |
| Dependency | Tidak ada — kedua kontrak backend yang dipakai sudah live saat ini |
| Klasifikasi | `MEDIUM` — menyentuh 1 hook, 2 komponen, plus ekstraksi 1 fungsi murni bersama; tanpa endpoint/tabel/migration baru |
| Task mode | `FRONTEND` (backend strict read-only, dipakai sebagai bukti kontrak) |
| Target tulis | `QuilvianSystemFrontendDev` (source); laporan ini di `NewQuilvianSystemBackend` sesuai aturan lokasi laporan |
| Model | Claude Sonnet 5 |
| Commit frontend saat dikerjakan | `52d7de26e` (branch `yasmina`), working tree belum di-commit |
| Commit backend yang dijadikan rujukan | `df48d7c` (branch `Yasmina`) |
| Tanggal | 2026-09-08 |
| Status | Source selesai ditulis, lint dan build lulus bersih, unit test baru lulus. Verifikasi manual ter-autentikasi belum dijalankan (lihat § 6) |

---

## 1. Keadaan yang ditemukan di awal

Sebelum task ini, `struk-pasien-document.jsx` hanya menampilkan header invoice, tabel item
(obat/tindakan/racikan/biaya admin), dan footer Total — nol field payer, nol breakdown
finansial, nol blok tanda tangan.

Saat memverifikasi kontrak sebelum menulis kode, ditemukan **satu premis keputusan yang perlu
dikoreksi**: `BKC-DEC-094` disetujui dengan kalimat "field Penjamin ditampilkan berdampingan
dengan field Asuransi yang sudah ada". Dua bagian premis itu diperiksa langsung ke source dan
tidak akurat:

1. Tidak ada field "Asuransi" pada Struk Pasien sebelum task ini — keduanya (Asuransi maupun
   Penjamin) sama-sama baru.
2. Backend hanya melacak **satu** payer aktif per kunjungan. Model
   `TrxPatientEncounterGuarantor` (`NewQuilvianSystemBackend/Areas/HealthServices/RegistrationManagement/Models/TrxPatientEncounterGuarantor.cs`)
   punya komentar eksplisit "Sumber pembayaran satu-ke-satu milik encounter", dengan satu
   `PaymentType` (`Cash` XOR `Insurance` XOR `CompanyGuarantor`) dan satu
   `PaymentSourceNameSnapshot`, diekspos sebagai satu field
   `InvoicePatientSummaryResponse.GuarantorName`
   (`Areas/HealthServices/BillingManagement/Billing/Dtos/BillingInvoiceDtos.cs:185-200`,
   `Areas/HealthServices/BillingManagement/Billing/Services/BillingInvoiceService.cs:712-732`).
   Satu kunjungan tidak pernah punya provider asuransi DAN perusahaan penjamin aktif sekaligus.

Maksud `BKC-DEC-094` — menampilkan nama pihak penjamin pada Struk Pasien — tetap valid dan
tidak dipertanyakan ulang oleh task ini. Yang diimplementasikan adalah bentuk yang benar secara
kontrak: **satu baris berlabel dinamis** ("Asuransi" bila `paymentType === "Insurance"`,
"Penjamin" bila `"CompanyGuarantor"`, baris tidak muncul sama sekali untuk `"Cash"`), bukan dua
baris tetap berdampingan. Koreksi ini sudah dicatat lebih dulu di
`roadmap/frontend-roadmap.md` § 1 amendment yang sama sebelum implementasi dimulai.

Temuan kedua: rumus Subtotal Mandiri/Asuransi dan Pajak Mandiri/Asuransi di
`menu-pembayaran-view.jsx` sudah tiga kali diperbaiki (komentar `FE-BKC-016`/`019`/`020` pada
file itu) karena versi yang ditulis terpisah menyimpang dari angka sebenarnya. Menulis ulang
rumus itu di `struk-pasien-document.jsx` akan mengulang pola bug yang sama, sehingga rumusnya
diekstrak menjadi satu fungsi murni yang dipakai KEDUA tempat — lihat § 3.3.

---

## 2. Proses bisnis dari sisi pengguna

**Penggunanya**: kasir yang sedang membuka halaman Dokumen Kasir (`FE-BKC-017`, route
terpisah dari Menu Pembayaran) untuk invoice tertentu.

1. Kasir menekan tombol "Dokumen Kasir" pada Menu Pembayaran, atau membuka tautan langsung
   `?tab=STRUK_PASIEN`.
2. Halaman Dokumen Kasir terbuka pada tab Struk Pasien (tab bawaan bila tidak ada tab lain yang
   diminta).
3. Selama tab ini aktif, halaman memuat data tambahan yang sebelumnya belum diambil di halaman
   ini: `GET /billing/invoices/{id}/calculation-preview` (endpoint yang sama yang sudah dipakai
   Ringkasan Pembayaran di Menu Pembayaran). Selama data ini belum sampai, bagian breakdown
   menampilkan kalimat "Memuat rincian pembayaran..." — tabel item dan header invoice TETAP
   langsung terlihat karena keduanya berasal dari data invoice yang sudah dimuat lebih dulu.
4. Setelah data sampai, Struk Pasien menampilkan:
   - Baris "Asuransi: <nama provider>" atau "Penjamin: <nama perusahaan>" di bawah Nama Pasien
     — hanya bila kunjungan memang punya penjamin tercatat; tidak ada baris ini untuk pasien
     tunai.
   - Tabel item (tidak berubah dari sebelumnya).
   - Tabel breakdown baru: Subtotal Mandiri, Subtotal Asuransi, Pajak Mandiri, Pajak Asuransi
     (disembunyikan untuk pasien tunai murni, sama seperti Ringkasan Pembayaran), dan Harus
     Dibayar (baris tebal, angka final).
   - Blok tanda tangan dua kolom: "Kasir" (dengan nama kasir yang sedang login di bawah garis
     kosong) dan "Penerima" (kosong, untuk ditandatangani manual di atas kertas).
5. Kasir menekan "Cetak Struk Pasien" untuk mengunduh PDF — perilaku unduh PDF ini TIDAK
   berubah dari `FE-BKC-011`.

**Jalur tidak normal**: invoice tanpa item aktif tetap menampilkan "Belum ada item pada invoice
ini." (tidak berubah). Bila `calculation-preview` gagal dimuat, breakdown tetap menampilkan
kalimat memuat tanpa pernah menampilkan angka yang salah (fungsi breakdown mengembalikan nol
untuk input kosong, diuji eksplisit — lihat § 6) — tidak ada error yang melempar/merusak
render dokumen.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `Areas/HealthServices/BillingManagement/Billing/Controllers/BillingInvoicesController.cs` (backend, read-only) — konfirmasi endpoint `calculation-preview` sudah ada.
- `Areas/HealthServices/BillingManagement/Billing/Dtos/BillingInvoiceDtos.cs` (backend, read-only) — bentuk `InvoicePatientSummaryResponse.GuarantorName`/`PaymentType`.
- `Areas/HealthServices/BillingManagement/Billing/Services/BillingInvoiceService.cs` (backend, read-only) — cara `GuarantorName` diisi dari `PaymentSourceNameSnapshot`.
- `Areas/HealthServices/RegistrationManagement/Models/TrxPatientEncounterGuarantor.cs` (backend, read-only) — konfirmasi relasi satu-ke-satu payer per kunjungan.
- `src/components/view/health-services/billing-management/billing-invoices/menu-pembayaran/menu-pembayaran-view.jsx` — sumber rumus breakdown yang diekstrak.
- `src/components/view/health-services/billing-management/billing-invoices/menu-pembayaran/struk-pasien-document.jsx`, `kwitansi-document.jsx` — pola dokumen printable existing.
- `src/lib/hooks/health-services/billing-management/billing-invoices/use-dokumen-kasir-page.js`, `use-menu-pembayaran.js` — pola fetch dan wiring hook.
- `src/lib/state/slice/health-services/billing-management/billing-invoice-slice.jsx` — thunk/selector `calculation-preview` yang sudah ada.
- `src/lib/state/slice/auth/login-slice.jsx` — selector `selectUserInfo` untuk nama kasir yang sedang login.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/health-services/billing-management/billing-invoices/billing-invoice-calculation-breakdown.js` (baru) | Fungsi murni `computeBillingCalculationBreakdown(calculationPreview)` — ekstraksi rumus Subtotal Mandiri/Asuransi, Pajak Mandiri/Asuransi, Total Tagihan, dan Harus Dibayar dari `menu-pembayaran-view.jsx`, tanpa perubahan hasil aritmetika |
| `src/components/view/health-services/billing-management/billing-invoices/menu-pembayaran/menu-pembayaran-view.jsx` | Rumus breakdown yang sebelumnya inline (±45 baris) diganti menjadi satu pemanggilan `computeBillingCalculationBreakdown` via `useMemo`; `EMPTY_OBJECT` dan variabel antara yang jadi tidak terpakai dihapus. Perilaku dan angka yang ditampilkan **tidak berubah** (dibuktikan lewat unit test § 6, angka identik dengan rumus lama) |
| `src/lib/hooks/health-services/billing-management/billing-invoices/use-dokumen-kasir-page.js` | Menambah fetch `previewBillingInvoiceCalculation` saat tab Struk Pasien aktif (pola sama dengan fetch Invoice Asuransi yang sudah ada); menambah `guarantorName`, `paymentType`, `cashierName` (dari `selectUserInfo`), `calculationLoading`, dan lima field breakdown ke `strukDocumentProps` |
| `src/components/view/health-services/billing-management/billing-invoices/menu-pembayaran/struk-pasien-document.jsx` | Menambah baris payer dinamis (Asuransi/Penjamin), tabel breakdown baru (Subtotal Mandiri/Asuransi, Pajak Mandiri/Asuransi, Harus Dibayar), dan blok tanda tangan dua kolom (Kasir/Penerima). Tabel item dan footer Total-nya **tidak disentuh** |
| `tests/unit/billing-invoice-calculation-breakdown.test.mjs` (baru) | 5 test memverifikasi rumus breakdown untuk kasus tunai murni, tertanggung penuh asuransi, coverage sebagian dengan residual non-billable, input kosong, dan bentuk PascalCase |

### 3.3 Kepatuhan arsitektur frontend

Fungsi breakdown ditempatkan di `src/lib/hooks/.../billing-invoices/` (bersebelahan dengan hook
pemakainya), mengikuti pola colocation yang sudah ada di folder yang sama
(`billing-invoice-constants.js`, dst.) — bukan folder `src/utils/` generik, karena fungsi ini
spesifik pada satu domain fitur, bukan utility lintas modul. Ini BUKAN abstraksi baru yang
dilarang `AGENTS.md` ("factory/generator/hook generik") — murni ekstraksi satu rumus yang
sudah ada ke satu tempat, dipanggil oleh dua consumer yang sudah ada, tanpa parameter
konfigurasi generik apa pun.

Thunk (`previewBillingInvoiceCalculation`) dan selector (`selectBillingCalculationPreview`,
`selectBillingCalculationPreviewLoading`) dipakai apa adanya dari
`billing-invoice-slice.jsx` yang sudah ada — tidak ada slice atau thunk baru. Pola fetch
di `use-dokumen-kasir-page.js` mengikuti persis pola `useEffect` + `useRef` penanda
invoiceId-yang-sudah-di-fetch yang sudah dipakai untuk Invoice Asuransi pada file yang sama.

Dokumen printable (`struk-pasien-document.jsx`) tetap memakai pola `WRAP` yang sudah dikunci
sejak `FE-BKC-011` (bukan base component baru) — style inline dan warna hex literal yang
ditambahkan (`#667085`, `#d0d5dd`) MENGULANG warna yang SUDAH ADA di file yang sama, bukan
warna baru.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tabel breakdown menampilkan "Memuat rincian pembayaran..." selama `calculation-preview` belum sampai; tabel item dan header tetap langsung terlihat (data lain sudah tersedia) |
| Kosong | Invoice tanpa item aktif tetap menampilkan "Belum ada item pada invoice ini." (tidak berubah dari sebelumnya) |
| Gagal | Bila `calculation-preview` gagal dimuat, breakdown tetap pada nilai nol (fungsi murni tidak melempar error untuk input kosong/null — diuji eksplisit), tidak merusak render dokumen. Tidak ada toast tambahan untuk kegagalan ini pada slice ini — konsisten dengan sifat dokumen ini yang tidak mengunci proses lain |
| Tanpa hak akses | `NOT APPLICABLE` — halaman Dokumen Kasir sudah memiliki gerbang akses sendiri dari `FE-BKC-017`, tidak disentuh task ini |

---

## 5. Endpoint yang dikonsumsi

Tidak ada endpoint baru. Satu endpoint yang SUDAH ADA kini juga dipanggil dari halaman Dokumen
Kasir (sebelumnya hanya dipanggil dari Menu Pembayaran):

#### Corporate / Billing Management / Billing / Invoices

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/billing/invoices/{id}/calculation-preview` | Mengisi breakdown Subtotal Mandiri/Asuransi dan Pajak Mandiri/Asuransi pada tab Struk Pasien | Sama seperti pemakaian existing di Menu Pembayaran — `BillingInvoice : Read` (tidak berubah) |

`InvoicePatientSummaryResponse.GuarantorName`/`PaymentType` dibaca dari respons `GET
/billing/invoices/{id}` yang sudah dipanggil `useBillingInvoiceDetail` sebelumnya — tidak ada
field baru diminta ke backend.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint --quiet <9 file berubah/baru>` | Selesai tanpa output | `PASS` | Exit code 0 |
| `npx eslint <9 file berubah/baru>` (full severity) | 0 error, 0 warning | `PASS` | Exit code 0 |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 445/445 lulus (440 existing + 5 baru), 0 gagal | `PASS` | Output `node:test`, `# pass 445 / # fail 0` |
| `npm run test:unit` (script `package.json` apa adanya) | Gagal dengan pesan `Could not find '...\\tests\\unit\\**\\*.test.mjs'` | `EXISTING / ENVIRONMENT ISSUE` | Direproduksi identik di Git Bash maupun PowerShell dengan command persis dari `package.json`; glob `**` pada `--test` tidak ter-resolve di environment ini. Pre-existing, tidak disentuh task ini — dilaporkan, bukan diperbaiki (di luar cakupan task) |
| `npm run build` | `next build` exit 0; kedua route terdampak (`.../pembayaran` dan `.../pembayaran/dokumen-kasir`) terkonfirmasi ada di output build; `postbuild` selesai normal | `PASS` | Output build lengkap tersimpan pada sesi ini |
| Grep anti-regresi (checklist G #3, #4, #5) | #3 (tombol non-base) dan #5 (utility typography Bootstrap) nihil hasil pada `struk-pasien-document.jsx`. #4 — dua `<table>` ditemukan, keduanya sudah punya `data-flat-table="true"` | `PASS` | Hasil grep tersimpan pada sesi ini |
| Unit test rumus breakdown (5 skenario: tunai murni, tertanggung penuh, coverage sebagian, input kosong, bentuk PascalCase) | Seluruhnya lulus, angka dicocokkan manual terhadap rumus asal `menu-pembayaran-view.jsx` | `PASS` | `tests/unit/billing-invoice-calculation-breakdown.test.mjs` |

Uji manual: **`NOT FEASIBLE`** — tidak ada kredensial login yang tersedia untuk builder pada
sesi ini, dan sengaja tidak diminta lewat chat untuk alasan keamanan (pola yang sama seperti
`FE-BKC-011`). Klik-coba nyata (buka invoice dengan tender asuransi/penjamin/tunai, buka tab
Struk Pasien, bandingkan angka breakdown dengan Ringkasan Pembayaran Menu Pembayaran untuk
invoice yang sama, cetak PDF) **belum dijalankan**.

**Tidak dijalankan**: `npm run test:e2e` dan `npm run test:uat` — tidak diminta task, dan
environment tidak dikonfirmasi mendukungnya.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria (persis seperti `frontend-roadmap.md`) | Status | Bukti |
| --- | --- | --- |
| 1. Breakdown (Subtotal Mandiri/Asuransi, Pajak Mandiri/Asuransi, Harus Dibayar) identik dengan Ringkasan Pembayaran untuk invoice yang sama | **Terpenuhi secara rumus** (satu fungsi yang sama dipanggil kedua tempat, dibuktikan unit test); **belum diverifikasi visual** dengan invoice nyata (lihat uji manual) | `billing-invoice-calculation-breakdown.js` + unit test |
| 2. Baris payer: "Asuransi"+nama untuk `Insurance`, "Penjamin"+nama untuk `CompanyGuarantor`, tidak muncul untuk `Cash` | Terpenuhi | `struk-pasien-document.jsx` `resolveGuarantorLabel`/`showGuarantorRow` |
| 3. Blok tanda tangan menampilkan nama kasir yang login di kolom Kasir, kolom Penerima kosong | Terpenuhi | `struk-pasien-document.jsx` blok tanda tangan; `cashierName` dari `selectUserInfo` |
| 4. Tidak ada elemen QR | Terpenuhi (tidak ada task/kode untuk elemen ini) | Tidak ada perubahan terkait QR |
| 5. Item baris (obat/tindakan/racikan/biaya admin) sejak `BKC-DEC-058` tidak berubah | Terpenuhi | Tabel item dan footer Total pada `struk-pasien-document.jsx` tidak disentuh — diff hanya menambah bagian baru di bawahnya |

**Definition of Done**:

| Butir | Status |
| --- | --- |
| Ketiga jenis payer (Tunai/Asuransi/Penjamin) diverifikasi manual menghasilkan PDF dengan angka yang cocok Ringkasan Pembayaran | **Belum terpenuhi** — lihat uji manual `NOT FEASIBLE` |
| Unit test fungsi breakdown lulus | Terpenuhi — 5/5 lulus |
| Lint/build lulus | Terpenuhi |
| `BKC-DEC-093`/`094`/`096` masing-masing punya baris acceptance criteria yang terverifikasi eksplisit | Terpenuhi secara kode; verifikasi visual/manual masih tertunda |

Task ini **belum bisa ditandai selesai sepenuhnya** — source, lint, build, dan unit test lulus
bersih, tetapi klik-coba nyata dengan invoice dan tender sungguhan (ketiga jenis payer) belum
dijalankan.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `npm run test:unit` (script `package.json`) gagal karena glob `**` tidak ter-resolve di environment ini — pre-existing, dilaporkan di § 6, tidak diperbaiki (di luar cakupan task ini) |
| Masalah yang diketahui | Breakdown pada Struk Pasien memerlukan satu fetch tambahan (`calculation-preview`) yang sebelumnya tidak dimuat halaman Dokumen Kasir — menambah satu request jaringan saat tab Struk Pasien dibuka pertama kali, sama seperti pola Invoice Asuransi yang sudah ada |
| Dependency backend | Tidak ada — kedua kontrak yang dipakai sudah live |
| Perubahan sampingan | `EMPTY_OBJECT` di `menu-pembayaran-view.jsx` dihapus karena menjadi tidak terpakai akibat ekstraksi rumus (bukan cleanup tidak terkait — konsekuensi langsung dari perubahan task ini, dikonfirmasi lint quiet lulus setelahnya) |
| Interupsi | `NONE` |
| Status Git | `git status --short` (frontend) menunjukkan 4 berkas termodifikasi (`menu-pembayaran-view.jsx`, `struk-pasien-document.jsx`, `use-dokumen-kasir-page.js`, plus `store.jsx` yang **BUKAN** perubahan task ini — sudah dimodifikasi sebelum task ini mulai, milik pekerjaan Petty Cash sesi lain) dan sejumlah file/folder untracked terkait Petty Cash yang juga **BUKAN** bagian task ini. Empat berkas milik task ini: 3 dimodifikasi (disebut di atas) + `billing-invoice-calculation-breakdown.js` (baru) + `tests/unit/billing-invoice-calculation-breakdown.test.mjs` (baru). Belum di-commit |
| Langkah berikutnya | Login dengan peran yang punya akses Dokumen Kasir, buka tiga invoice berbeda jenis payer (Tunai, Asuransi, Penjamin Perusahaan), bandingkan angka breakdown Struk Pasien dengan Ringkasan Pembayaran Menu Pembayaran untuk invoice yang sama, lalu cetak PDF dan periksa blok tanda tangan serta baris payer tampil benar |
