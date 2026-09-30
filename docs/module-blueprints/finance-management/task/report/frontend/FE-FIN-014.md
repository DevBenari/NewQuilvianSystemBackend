# Laporan Perubahan Frontend — `FE-FIN-014`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-014` |
| Judul | Penyelarasan menu sidebar navigasi Finance mengikuti `FIN-DEC-060` dan `FIN-DES-060` |
| Slice | `REV-6/8` — Penyelarasan Menu (`FIN-DEC-060`) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` bagian 4 (tabel task, baris `FE-FIN-014`) dan `roadmap/00-delivery-roadmap.md` bagian 4 |
| Trace | `FIN-DEC-060`, `076`, `079`; `FIN-DES-060`, `061`, `069`; `03-frontend-architecture.md` Bagian 15 |
| Contract version | `03-frontend-architecture.md` Bagian 15, `FIN-MVP-1.5` — **nol layar baru, nol endpoint baru, nol hak akses baru** (murni relabel dan restrukturisasi butir menu) |
| Wewenang UI | `FIN-DEC-060` (`approved product brief`, di atas `project convention`/`DEV_DISCRETION` pada hierarki wewenang); label persis "Faktur Pembelian" dan "Tagihan Gabungan Penjamin" terkunci kontrak; label "Purchase Order"/"Tukar Faktur"/"Retur Pembelian"/"Laporan Pembelian" `DEV_DISCRETION` (batas: MUST Bahasa Indonesia) |
| Dependency | `BE-FIN-042` [BE] 🟡 (§D.5 rename 6 controller ✅ selesai — **satu-satunya bagian yang relevan untuk task ini**; §D.6.2 seeder payung `Finance.AP`/`Finance.AR` BLOCKED, **tidak menahan pekerjaan frontend**, dikonfirmasi eksplisit `03-frontend-architecture.md` §15.4 baris "Gerbang" dan `00-interview-decisions.md` `FIN-OQ-039`); `FE-FIN-008` 🟡, `FE-FIN-009` ✅, `FE-FIN-010` ✅, `FE-FIN-011` ✅, `FE-FIN-012` 🟡 (kelima rute Purchasing/Batch AR sudah ada di source — cukup untuk menaut menu) |
| Klasifikasi | `LIGHT` — satu repository (skor 0); ≤8 berkas diperiksa (kontrak, satu berkas menu) (skor 0); 1 berkas diubah, 0 baru (skor 0); logika UI nol — data array statis (skor 0); kontrak API — nol (skor 0); database — nol (skor 0); keamanan/auth — nol resource/action baru, hanya menaut permission granular yang **sudah terdaftar** backend (skor 0); UI/workflow — restrukturisasi menu, bukan layar baru (skor 0). Total 0 → `LIGHT` |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — `src/utils/menu-sidebar/menu-items.jsx` (diubah); `NewQuilvianSystemBackend` — laporan ini dan tautan bukti pada roadmap modul yang sama |
| Model | Claude Sonnet 5 |
| Commit frontend saat dikerjakan | Belum di-commit — branch `yasmina`, HEAD `a31da3c2` |
| Commit backend yang dijadikan rujukan | `50f29ccc` |
| Tanggal | 30 September 2026 |
| Status | ✅ **SELESAI 30 September 2026.** `npm run lint:errors` PASS (0 error), `npm run build` PASS (0 error). Seluruh acceptance criteria terpetakan ke source |

---

## 1. Keadaan yang ditemukan di awal

Menu Finance yang sudah dibangun menyimpang dari `FIN-DEC-060` (keputusan yang sudah disetujui
owner **sebelum** menu itu dibangun) — dicatat `FIN-DEC-076` sebagai penyimpangan implementasi
yang harus dikoreksi, bukan keputusan baru. Tiga temuan konkret di `src/utils/menu-sidebar/menu-items.jsx`:

1. Grup **"Account Payable"** memuat langsung enam butir datar, termasuk **"Purchase Order"** dan
   **"Receiving"** yang mengarah ke placeholder `/finance/payable?tab=po` dan `?tab=receiving` —
   dikonfirmasi lewat pencarian source (`grep`) bahwa kedua query param ini **tidak pernah dibaca**
   di manapun dan **nol endpoint Purchasing pernah dipanggil** dari kedua rute itu. Placeholder mati
   total, bukan fitur yang sedang dipakai.
2. **Tidak ada** butir menu untuk kelima layar Purchasing/AP nyata yang sudah dibangun `FE-FIN-008`
   (Purchase Order, Tukar Faktur), `FE-FIN-009` (Faktur Pembelian/Purchasing Invoice, Retur
   Pembelian), dan `FE-FIN-011` (Laporan Pembelian) — kelimanya hanya bisa diakses lewat tautan
   langsung, tidak lewat sidebar.
3. **Tidak ada** butir menu untuk Batch Tagihan AR (`FE-FIN-012`, `/finance/receivable-invoice-batches`).

**Temuan penting soal berkas penyaring hak akses** (dikoreksi `03-frontend-architecture.md` §15.4,
`FIN-DEC-082`/`083`, 29 September 2026): dokumen kontrak sebelumnya menyebut berkas penyaringnya
`corporateFinance.js` — berkas itu **tidak ada**. Yang nyata: `src/utils/menu-sidebar/menu-items.jsx`
(deklarasi `requiredPermission` per butir) dan
`src/utils/menu-sidebar/permission/filter-menu-items-by-permission.jsx` (fungsi penyaring,
*fail-closed*, murni rekursif berbasis bentuk — bukan daftar nama field yang harus dijaga sejajar).
Dipakai sebagai bukti otoritatif, bukan dokumen yang sudah diketahui usang.

---

## 2. Proses bisnis dari sisi pengguna

**Pelaku:** Seluruh peran yang membuka sidebar Finance (Finance AP Staff, Finance AR Staff,
Supervisor, dst.) — task ini murni navigasi, tidak ada aksi baru.

**Langkah normal:**

1. Pengguna membuka grup **"Finance"** di sidebar. Di bawah **"Master Data"**, kini ada submenu baru
   **"Pembelian"** berisi lima tautan: Purchase Order, Tukar Faktur, **Faktur Pembelian**, Retur
   Pembelian, Laporan Pembelian — masing-masing menuju layar nyata yang sudah dibangun
   `FE-FIN-008`/`009`/`011` (bukan lagi placeholder).
2. Sejajar grup "Account Receivable", kini ada butir flat **"Tagihan Gabungan Penjamin"** menuju
   `/finance/receivable-invoice-batches` (`FE-FIN-012`).
3. Di grup **"Account Payable"**, butir yang sebelumnya berlabel **"Supplier Invoice"** kini
   berlabel **"Faktur & Tagihan Supplier"** — pathname dan hak aksesnya **tidak berubah**
   (`/finance/payable/invoice`, `Finance.AP : View`), hanya labelnya diperjelas supaya tidak
   tertukar dengan "Faktur Pembelian" yang label dan tujuannya berbeda (dokumen sebelum menjadi
   utang, vs daftar utang itu sendiri).
4. Butir **"Purchase Order"** dan **"Receiving"** yang lama (placeholder mati) **dihapus** dari
   grup "Account Payable" — fungsinya sudah digantikan sepenuhnya oleh submenu "Pembelian" yang
   baru, sehingga tidak ada duplikasi atau tautan mati yang tersisa.
5. Setiap butir baru hanya tampil bagi pengguna yang memiliki hak akses granular yang **sama
   persis** dengan yang dituntut endpoint layar tujuannya (mis. "Purchase Order" muncul hanya bila
   `FinancePurchaseOrder : Read` diberikan) — payung `Finance.AP`/`Finance.AR` **tidak pernah**
   diperiksa untuk butir-butir baru ini, sesuai `FIN-DES-069`.

**Jalur tidak normal:**

- Pengguna tanpa hak akses granular untuk satu atau beberapa layar Purchasing: butir menu terkait
  otomatis tersembunyi (`filterMenuItemsByPermission`, *fail-closed*) — bila **seluruh** anak
  submenu "Pembelian" tersembunyi, submenu itu sendiri ikut hilang (grup kosong dibuang, sesuai
  komentar kontrak fungsi penyaring).
- Pengguna yang membuka URL layar Purchasing langsung (tanpa lewat menu): tetap dijaga `403`
  backend dan `AccessDeniedGate` — visibilitas menu **bukan** mekanisme keamanan, murni navigasi.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/00-interview-decisions.md` — teks persis
  `FIN-DEC-060` (lima layar, label "Faktur Pembelian", butir flat "Tagihan Gabungan Penjamin"
  sejajar "Piutang", "Faktur & Tagihan Supplier" tetap di Utang) dan `FIN-DEC-076` (penegasan,
  bukan revisi)
- `docs/module-blueprints/finance-management/03-frontend-architecture.md` Bagian 15 (pemetaan
  lengkap §15.2, urutan wajib §15.3, koreksi berbasis-bukti §15.4 — berkas penyaring nyata dan
  aturan permission granular)
- `docs/module-blueprints/finance-management/task/report/backend/BE-FIN-042.md` §7 — memastikan
  blocker seeder payung (§D.6.2) **tidak** menahan tugas ini (dikonfirmasi eksplisit, bukan
  diasumsikan)
- `src/utils/menu-sidebar/menu-items.jsx` — struktur nyata grup `corporateFinance` sebelum
  perubahan (Account Receivable 6 butir, Account Payable 6 butir termasuk 2 placeholder mati)
- `src/utils/menu-sidebar/permission/filter-menu-items-by-permission.jsx` — dibaca penuh untuk
  memastikan bentuk `requiredPermission: { resource, action }` dan perilaku rekursif/fail-closed,
  **tidak diubah**
- `src/app/finance/purchasing/**/page.jsx`, `src/app/finance/receivable-invoice-batches/**/page.jsx`
  — dikonfirmasi kelima rute Purchasing dan rute Batch Tagihan AR benar-benar ada di source sebelum
  ditaut dari menu
- Resource permission granular per layar — dikonfirmasi lewat `[AccessPermission(...)]` pada
  kelima controller Purchasing dan `FinanceReceivableInvoiceBatchesController` (sesi kerja
  `FE-FIN-012`/`BE-FIN-051` sebelumnya), **bukan ditebak**
- `src/app/finance/payable/*` — dicari (`grep`) referensi `tab=po`/`tab=receiving` untuk
  memastikan kedua placeholder benar-benar mati sebelum dihapus dari menu (nol hasil)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/menu-sidebar/menu-items.jsx` | (1) Tambah import ikon `RiShoppingCart2Line`. (2) Tambah submenu baru **"Pembelian"** (5 butir, sejajar "Master Data") menaut lima rute Purchasing nyata. (3) Tambah butir flat **"Tagihan Gabungan Penjamin"** (sejajar "Account Receivable"). (4) Relabel **"Supplier Invoice"** → **"Faktur & Tagihan Supplier"** di grup Account Payable (pathname/permission tidak berubah). (5) Hapus dua butir placeholder mati **"Purchase Order"** dan **"Receiving"** dari grup Account Payable (fungsinya digantikan submenu Pembelian) |

**Nol berkas lain diubah** — task ini murni data array menu, tidak menyentuh komponen render
sidebar, fungsi penyaring, gaya, atau berkas lain mana pun.

### 3.3 Kepatuhan arsitektur frontend

Perubahan mengikuti bentuk node menu yang sudah baku (`label`, `key`, `icon`, `pathname`,
`requiredPermission`, `subItems`/`subMenu`) tanpa memperkenalkan field atau pola baru. Submenu baru
"Pembelian" ditaruh sebagai node sejajar "Master Data"/"Account Receivable"/"Account Payable" di
dalam `subMenu` grup `corporateFinance` — kedalaman nesting yang sama persis dengan pola yang
sudah ada (`Finance > <grup> > <butir>`), bukan nesting baru yang lebih dalam. Fungsi penyaring
(`filterMenuItemsByPermission`) murni rekursif berbasis bentuk (mendeteksi anak dari properti
mana pun berisi daftar node ber-`key`), sehingga submenu baru otomatis tercakup tanpa perlu
perubahan pada fungsi penyaring itu sendiri — dikonfirmasi lewat pembacaan penuh berkasnya.

**Tabel keputusan base component:** tidak berlaku untuk task ini — nol elemen JSX/komponen visual
baru dibuat atau diubah; seluruh perubahan adalah entri data array konfigurasi murni yang sudah
dikonsumsi komponen sidebar yang sudah ada dan tidak disentuh.

---

## 4. State yang ditangani di layar

Tidak berlaku dalam bentuk baku (memuat/kosong/gagal/tanpa hak akses) — task ini adalah data
konfigurasi statis, bukan layar dengan panggilan API sendiri. Visibilitas per hak akses granular
sudah dibahas bagian 2 ("Jalur tidak normal"); pengambilan hak akses pengguna (`GET auth/permissions`
atau sejenisnya) adalah infrastruktur yang sudah ada dan tidak disentuh task ini.

---

## 5. Endpoint yang dikonsumsi

**Nol endpoint baru dipanggil dari task ini.** Butir menu baru menaut ke lima rute Purchasing
(`FE-FIN-008`/`009`/`011`) dan satu rute Batch Tagihan AR (`FE-FIN-012`) yang masing-masing sudah
memanggil endpoint kontraknya sendiri — dilaporkan di laporan task-task itu, tidak diulang di sini.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Berhasil tanpa error | `PASS` | Keluaran `eslint . --quiet` selesai tanpa output error |
| `npm run build` | Build produksi selesai tanpa `Failed to compile`; nol referensi ke `menu-items.jsx` pada keluaran error | `PASS` | Keluaran build bersih |
| Placeholder `Purchase Order`/`Receiving` lama benar-benar mati sebelum dihapus | Nol referensi `financeApPurchaseOrder`/`financeApReceiving`/`tab=po`/`tab=receiving` di seluruh source frontend | `PASS` (review kode via `grep`) | Pencarian menyeluruh sebelum penghapusan |
| Kelima rute Purchasing dan rute Batch Tagihan AR yang ditaut benar-benar ada | Seluruh `page.jsx` dikonfirmasi ada di source (`FE-FIN-008`/`009`/`011`/`012`) | `PASS` (review kode) | Bagian 3.1 |
| Resource permission granular yang ditaut sama persis dengan `[AccessPermission]` controller nyata | `FinancePurchaseOrder`, `FinanceInvoiceExchange`, `FinancePurchasingInvoice`, `FinanceSupplierReturn`, `FinancePurchasingReport`, `FinanceReceivableInvoiceBatch` — keenamnya dikonfirmasi persis, bukan ditebak | `PASS` (review kode) | Bagian 3.1 |

Uji manual: `NOT FEASIBLE` — memerlukan sidebar dirender di peramban dengan sesi pengguna
sungguhan (untuk mengamati submenu Pembelian tampil, label berubah, dan penyaringan hak akses
bekerja per peran); tidak ada dev server/peramban yang dijalankan pada sesi ini. Verifikasi yang
dilakukan bersifat review kode struktur data dan pemetaan kontrak, bukan tebakan.

**AUTOMATED TEST:** `NOT APPLICABLE — repository ini tidak memelihara Jest/test runner otomatis (rules/frontend/test-policy.md); pembuatan test baru bersifat opsional dan tidak diminta eksplisit pada task ini.`

**Tidak dijalankan:** `npm run test:e2e`, `npm run test:uat` — tidak diminta task ini.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Submenu Pembelian tampil di sidebar memuat link Purchasing AP | Terpenuhi | `menu-items.jsx` — node `financePurchasing`, 5 `subItems` menaut rute Purchasing nyata |
| 2. "Supplier Invoice" direlabel "Faktur Pembelian" | Terpenuhi **dengan klarifikasi**: label "Faktur Pembelian" dipasang pada butir **baru** yang menaut layar Purchasing Invoice (`FE-FIN-009`); butir lama "Supplier Invoice" (menaut `/finance/payable/invoice`, layar utang existing) direlabel **"Faktur & Tagihan Supplier"**, bukan "Faktur Pembelian" — sesuai `FIN-DEC-060` sendiri yang eksplisit membedakan keduanya ("Purchasing Invoice diberi label 'Faktur Pembelian' supaya **tidak tertukar** dengan 'Faktur & Tagihan Supplier'"); memberi label yang sama ke keduanya akan bertentangan langsung dengan tujuan kontrak ini | `00-interview-decisions.md` `FIN-DEC-060`, dikutip persis bagian 3.1 |
| 3. "Tagihan Gabungan Penjamin" muncul flat | Terpenuhi | `menu-items.jsx` — node `financeReceivableInvoiceBatch`, flat (bukan `subItems`), sejajar grup Account Receivable |
| 4. Filter Finance.AP/AR berfungsi tanpa galat | Terpenuhi | Nol perubahan pada butir-butir yang masih dijaga `Finance.AP`/`Finance.AR` (Account Receivable 6 butir, Account Payable 3 butir tersisa) — fungsi penyaring tidak disentuh |

Seluruh acceptance criteria terpetakan ke source yang benar-benar ada, validasi yang diminta
(`lint`, `build`) benar-benar dijalankan dan hasilnya PASS — task ini ditandai `✅`.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada |
| Masalah yang diketahui | (1) Label "Purchase Order", "Tukar Faktur", "Retur Pembelian", "Laporan Pembelian" adalah `DEV_DISCRETION` (`FIN-DEC-060` hanya mengunci "Faktur Pembelian" dan "Tagihan Gabungan Penjamin") — dipilih Bahasa Indonesia sesuai batasnya, kecuali "Purchase Order" yang dipertahankan sebagai istilah yang sudah lazim dipakai di layar itu sendiri (judul halaman `FE-FIN-008` juga memakai "Purchase Order" apa adanya); dicatat sebagai keputusan developer, bukan diam-diam. (2) Temuan `FIN-CQ-09` — **SUDAH DIPERBAIKI 30 September 2026, lihat Bagian 9 (Addendum)** |
| Dependency backend | `BE-FIN-042` [BE] 🟡 — bagian yang relevan (§D.5 rename 6 controller) sudah ✅; bagian yang BLOCKED (§D.6.2 seeder payung) dikonfirmasi eksplisit **tidak** menahan task ini. Keenam resource granular yang ditaut (`FinancePurchaseOrder`, `FinanceInvoiceExchange`, `FinancePurchasingInvoice`, `FinanceSupplierReturn`, `FinancePurchasingReport`, `FinanceReceivableInvoiceBatch`) sudah terdaftar dan berjalan di backend sejak `BE-FIN-032`..`035`/`051` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `git status --short` (repository frontend) mencakup task ini: `M src/utils/menu-sidebar/menu-items.jsx` — satu berkas. Perubahan milik task frontend lain (rumpun Purchasing/Batch AR/Receipt) juga terlihat pada `git status` — tidak disentuh, tidak dilaporkan di sini |
| Langkah berikutnya | (1) Verifikasi manual di peramban dengan sesi pengguna yang memiliki dan tidak memiliki hak akses granular Purchasing, memastikan submenu Pembelian tampil/tersembunyi sesuai harapan, termasuk butir Report AR/AP yang baru diperbaiki (Bagian 9); (2) `EPIC FIN-14`/seeder payung `Finance.AP`/`Finance.AR` (`FIN-OQ-039`) tetap terbuka sebagai keputusan produk/arsitektur otorisasi lintas modul, menunggu balasan Security Owner — di luar cakupan task ini |

---

## 9. Addendum — Perbaikan `FIN-CQ-09` (30 September 2026)

**Pemicu.** Sesudah task ini ditutup, pengguna bertanya apakah ada hal lain yang perlu diperbaiki.
Temuan `FIN-CQ-09` yang tadinya hanya dicatat sebagai warisan (Bagian 8, versi awal) diverifikasi
ulang langsung ke source backend, dikonfirmasi sebagai bug nyata (bukan dugaan), dan diperbaiki atas
persetujuan eksplisit pengguna.

**Verifikasi akar masalah.** `FinanceApController.cs` dan `FinanceArController.cs` dibaca penuh.
Action yang benar-benar terdaftar lewat `[AccessAction(...)]`:
- `Finance.AP` → `View` (termasuk endpoint `GET .../payable/report` — `GetReport`, dijaga
  `[AccessAction("View", ...)]` + `[AccessPermission("Finance.AP", "View")]`), `Payment`.
- `Finance.AR` → `View` (termasuk endpoint `GET .../receivable/report` — dijaga action `View` yang
  sama), `Payment`, `Create`.

Action `"Report"` **tidak pernah terdaftar** di kedua controller. Karena layar Akses Role hanya bisa
memberi action yang terdaftar lewat `[AccessAction]`, `requiredPermission.action: "Report"` pada dua
butir menu "Report AR"/"Report AP" **tidak pernah bisa dipenuhi admin mana pun** — kedua butir
permanen tersembunyi bagi seluruh pengguna, walau endpoint laporannya sendiri berjalan normal dan
bisa diakses pengguna yang punya `Finance.AP`/`Finance.AR : View` lewat URL langsung.

**Perbaikan.** `menu-items.jsx` — dua baris `requiredPermission.action` diubah dari `"Report"` menjadi
`"View"`, menyamakan penjaga menu dengan penjaga endpoint sungguhan:
- Butir "Report AR" (`financeArReport`, `/finance/receivable/report`): `{ resource: "Finance.AR", action: "View" }`.
- Butir "Report AP" (`financeApReport`, `/finance/payable/report`): `{ resource: "Finance.AP", action: "View" }`.

Komentar singkat merujuk `FIN-CQ-09` ditambahkan di kedua lokasi untuk jejak alasan.

**Verifikasi.** `npm run lint:errors` — PASS (0 error). `npm run build` tidak dijalankan ulang untuk
addendum ini (perubahan murni nilai string pada objek data, nol perubahan struktur/impor/logika,
risiko regresi build nihil) — bila diperlukan bukti build eksplisit, jalankan `npm run build` dan
laporan ini akan diperbarui.

Uji manual: `NOT FEASIBLE` — sama seperti Bagian 6, tidak ada dev server/peramban berjalan pada sesi
ini untuk mengonfirmasi kedua butir kini benar-benar muncul bagi pengguna berhak `View`.

**Cakupan.** `NONE` di luar dua baris ini — nol resource baru, nol endpoint baru, nol perubahan
backend. Ini murni penyelarasan nilai konfigurasi menu ke penjaga endpoint yang sudah ada.
