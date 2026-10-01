# Laporan Perubahan Frontend — `FE-FIN-016`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-016` |
| Judul | Menu Keuangan berbentuk dua grup datar "Transaksi A/R" dan "Transaksi A/P" seperti sistem produksi V1 |
| Slice | `REV-13A` |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` bagian "AMENDMENT ROADMAP REVISI 13" |
| Trace | `FIN-DEC-094` (**supersedes `FIN-DEC-060`**); `03-frontend-architecture.md` §17.2 |
| Contract version | Tidak ada kontrak API baru dikonsumsi — task ini murni restrukturisasi butir menu atas kontrak endpoint yang sudah ada |
| Wewenang UI | `FIN-DEC-094` adalah **approved product brief** — mengunci layar mana yang berdiri sendiri dan butir menu mana yang ada. Tata letak, warna, ikon, urutan visual tetap `DEV_DISCRETION` (tidak disentuh task ini — hanya struktur data menu) |
| Dependency | Tidak ada — berdiri sendiri, menjadi prasyarat `FE-FIN-017`..`023` |
| Klasifikasi | `LIGHT` — satu repository (skor 0); 1 berkas diubah (skor 0); nol component/hook/Redux baru (skor 0); nol endpoint baru dikonsumsi (skor 0); database — tidak relevan (skor 0); keamanan/auth — nol resource/action baru, hanya menata ulang `requiredPermission` existing (skor 0); UI/workflow — murni data konfigurasi menu, nol markup baru (skor 0). Total 0 → `LIGHT` |
| Task mode | `FRONTEND MODE` — target tulis `QuilvianSystemFrontendDev` saja; backend `NewQuilvianSystemBackend` dibaca read-only untuk memverifikasi string `[AccessPermission]` |
| Target tulis | `QuilvianSystemFrontendDev` — `src/utils/menu-sidebar/menu-items.jsx` dan laporan ini |
| Model | Claude Sonnet 5 |
| Commit frontend saat dikerjakan | `d2e8a3538` — branch `yasmina` |
| Commit backend yang dijadikan rujukan | `99df41e1` — branch `Yasmina` (read-only, memverifikasi resource `[AccessPermission]`) |
| Tanggal | 1 Oktober 2026 |
| Status | 🟡 **Sebagian.** Source lengkap sesuai kontrak. `npm run lint:errors` **PASS**. `npm run test:unit` dan `npm run build` **NOT RUN** — menunggu konfirmasi eksplisit pengguna, mengikuti konvensi sesi saat ini |

---

## 1. Keadaan yang ditemukan di awal

Sebelum task ini, menu "Finance" di sidebar tersusun dari pengelompokan ad-hoc yang dibangun
bertahap (`FE-FIN-002`/`004`/`008`/`009`/`010`/`012`/`014`): submenu "Pembelian" (proses
pembelian), grup "Account Receivable", butir flat "Tagihan Gabungan Penjamin", dan grup "Account
Payable". Pengelompokan ini **berjalan dan tidak rusak**, tetapi tidak mengikuti bentuk yang sudah
diuji dan disetujui pengguna lewat UAT pada sistem produksi V1 — pengguna melaporkan "ada beberapa
menu yang belum tampil khususnya di bagian keuangan" karena sejumlah butir menu V1 (Ayat Silang,
Canceled Invoice, Manajemen Klaim, dan lainnya) memang belum ada jalan masuknya sama sekali di
sini.

`FIN-DEC-094` membalik arah itu: setiap kemampuan yang di V1 berdiri sebagai halaman sendiri
**MUST** berdiri sebagai halaman sendiri di sini juga, dikelompokkan sebagai dua grup datar
"Transaksi A/R" dan "Transaksi A/P" — bukan submenu bertingkat seperti struktur lama.

Celah yang menghalangi outcome task ini sebelum dikerjakan: struktur menu lama tidak memiliki
tempat untuk 9 layar baru yang akan dibangun task `FE-FIN-017`..`023`, dan tiga butir sudah ada
(Ayat Silang, Settlement AR, Purchasing Payment) ternyata menunjuk ke pathname yang salah secara
konseptual dibanding bentuk V1 (lihat §3.2).

---

## 2. Proses bisnis dari sisi pengguna

**Pelaku:** Seluruh pengguna yang memegang hak akses modul Finance (Petugas AR, Petugas AP,
Supervisor/Manajer Finance).

**Pemicu:** Pengguna membuka sidebar navigasi dan mengeklik grup "Finance".

**Langkah normal:**

1. Pengguna melihat grup "Finance" berisi, berurutan: Master Data (tidak berubah), **Transaksi
   A/R** (baru, datar), **Transaksi A/P** (baru, datar), Petty Cash (tidak berubah), Cash
   Monitoring (tidak berubah).
2. Pengguna mengeklik "Transaksi A/R" dan melihat 7 butir langsung (tanpa submenu bertingkat
   lagi): Tagihan/Billing, Receivable AR/Invoice, Ayat Silang, Settlement AR, Report AR, Laporan
   Aging AR, Piutang Tagihan — masing-masing menuju layar yang sudah berfungsi hari ini.
3. Pengguna mengeklik "Transaksi A/P" dan melihat 9 butir langsung: Pembelian Pesanan, Tukar
   Faktur, Purchasing Invoice, Laporan Aging AP, Retur Pembelian Supplier, Purchasing Payment,
   Laporan Pembayaran AP, Penerimaan Invoice, Utang Usaha (A/P Aging).
4. Setiap butir hanya tampil bila pengguna memegang `requiredPermission` yang menjaganya —
   perilaku penyaringan ini tidak berubah sama sekali, murni memakai `filterMenuItemsByPermission`
   yang sudah ada.

**Jalur tidak normal:** Pengguna tanpa hak akses pada seluruh butir di satu grup tidak akan
melihat grup itu sama sekali (grup ikut tersembunyi, perilaku bawaan `filterMenuItemsByPermission`
— lihat §3.3). Ini bukan perilaku baru, melainkan properti fungsi penyaring yang sudah ada dan
sengaja tidak disentuh task ini.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/03-frontend-architecture.md` §17 (17.1, 17.2) —
  peta butir menu lengkap beserta pathname, butir hak akses, dan status layar (`Sudah ada`/
  `Baru`/`TERTAHAN`) per butir
- `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` baris `FE-FIN-016`
  — cakupan task persis ("daftarkan hanya butir yang rutenya sudah ada hari ini (13 butir)")
- `src/utils/menu-sidebar/menu-items.jsx` — struktur menu "Finance" existing secara penuh
  (submenu "Pembelian", grup "Account Receivable", butir "Tagihan Gabungan Penjamin", grup
  "Account Payable", grup "Petty Cash", grup "Cash Monitoring")
- `src/utils/menu-sidebar/permission/filter-menu-items-by-permission.jsx` — dibaca penuh untuk
  memastikan perilaku penyaringan (termasuk pembuangan grup kosong) tidak terpengaruh perubahan
  struktur data menu
- Seluruh `page.jsx` di bawah `src/app/finance/` (`find ... -iname page.jsx`) — memverifikasi
  mana pathname pada peta menu §17.2 yang benar-benar punya route hari ini, bukan sekadar
  percaya pada tabel status "Sudah ada" di dokumen
- `src/app/finance/receivable/invoice/page.jsx`, `.../receivable/payment/page.jsx`,
  `.../payable/aging/page.jsx`, `.../payable/report/page.jsx`, `.../payable/payment/page.jsx`,
  `.../ap-report/page.jsx` — dibaca untuk membuktikan lima di antaranya adalah **alias tipis**
  yang mengimpor client component dari pathname kanonikal V1 (lihat §3.2 dan §3.3)
- `NewQuilvianSystemBackend/Areas/.../FinancePaymentsController.cs`,
  `.../FinanceReceiptsController.cs` (read-only) — memverifikasi string resource
  `[AccessPermission("FinancePayment", ...)]`/`[AccessPermission("FinanceReceipt", ...)]` persis
  sebelum dipakai sebagai `requiredPermission` baru

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/menu-sidebar/menu-items.jsx` | Submenu "Pembelian", grup "Account Receivable", butir flat "Tagihan Gabungan Penjamin", dan grup "Account Payable" **dicabut**, digantikan dua grup datar baru "Transaksi A/R" (7 butir) dan "Transaksi A/P" (9 butir) persis sesuai `03-frontend-architecture.md` §17.2. Import `RiShoppingCart2Line` dicabut karena jadi tidak terpakai. Grup "Master Data", "Petty Cash", dan "Cash Monitoring" **tidak disentuh** |

**Rincian 16 butir yang didaftarkan** (7 A/R + 9 A/P), beserta pathname dan hak akses — seluruhnya
diverifikasi punya `page.jsx` yang benar-benar ada hari ini:

| Grup | Butir | `pathname` | `requiredPermission` | Catatan |
| --- | --- | --- | --- | --- |
| Transaksi A/R | Tagihan/Billing | `/finance/receivable-invoice-batches` | `FinanceReceivableInvoiceBatch : Read` | Dipindah dari butir flat "Tagihan Gabungan Penjamin" — pathname dan permission sama persis, hanya label dan posisi berubah |
| Transaksi A/R | Receivable AR/Invoice | `/finance/receivable` | `FinanceReceivable : Read` | Dipindah dari "Receivable AR" (Account Receivable) — pathname sama, **permission diperbarui** dari `Finance.AR : View` (V2 payung) ke `FinanceReceivable : Read` (granular) sesuai §17.2 |
| Transaksi A/R | Ayat Silang | `/finance/receipts?debtorType=PAYER` | `FinanceReceipt : Read` | **Baru didaftarkan** — pandangan tersaring atas layar `/finance/receipts` yang sudah ada (`FIN-DEC-096`), bukan layar baru |
| Transaksi A/R | Settlement AR | `/finance/receipts` | `FinanceReceipt : Read` | **Diperbaiki.** Pathname lama `/finance/receivable?status=SETTLED` adalah saringan status di atas daftar piutang — bukan layar penerimaan/alokasi yang dimaksud V1. Dibuktikan dari §17.2: Settlement AR memakai `FinanceReceipt : Read`, bukan `Finance.AR`, dan layar `/finance/receipts` memang layar penerimaan/alokasi pembayaran |
| Transaksi A/R | Report AR | `/finance/ar-report` | `Finance.AR : View` | Dipindah dari "Report AR" (pathname lama `/finance/receivable/report`). Dibuktikan lewat pembacaan source: `/finance/receivable/report/page.jsx` hanya **meng-import ulang** `ArReportClient` dari `/finance/ar-report/ar-report-client` — keduanya merender komponen yang sama persis, `/finance/ar-report` adalah rute kanonikalnya |
| Transaksi A/R | Laporan Aging AR | `/finance/ar-aging` | `Finance.AR : View` | Sama pola dengan Report AR — `/finance/receivable/aging/page.jsx` adalah alias tipis dari `/finance/ar-aging/ar-aging-client` |
| Transaksi A/R | Piutang Tagihan | `/finance/receivable?view=billed` | `FinanceReceivable : Read` | **Baru didaftarkan** — pandangan tersaring atas `/finance/receivable` yang sudah ada |
| Transaksi A/P | Pembelian Pesanan | `/finance/purchasing/purchase-orders` | `FinancePurchaseOrder : Read` | Dipindah dari "Purchase Order" (submenu Pembelian) — pathname dan permission sama persis, label direlabel mengikuti V1 |
| Transaksi A/P | Tukar Faktur | `/finance/purchasing/invoice-exchanges` | `FinanceInvoiceExchange : Read` | Dipindah — sama persis |
| Transaksi A/P | Purchasing Invoice | `/finance/purchasing/purchasing-invoices` | `FinancePurchasingInvoice : Read` | Dipindah dari "Faktur Pembelian" — pathname dan permission sama, direlabel mengikuti V1 |
| Transaksi A/P | Laporan Aging AP | `/finance/ap-aging` | `Finance.AP : View` | **Baru didaftarkan.** Butir lama "Aging AP" menunjuk `/finance/payable/aging`, terbukti alias tipis dari `/finance/ap-aging/ap-aging-client` — rute kanonikalnya dipakai di sini |
| Transaksi A/P | Retur Pembelian Supplier | `/finance/purchasing/supplier-returns` | `FinanceSupplierReturn : Read` | Dipindah dari "Retur Pembelian" — sama persis, direlabel |
| Transaksi A/P | Purchasing Payment | `/finance/payment-ap` | `FinancePayment : Read` | **Diperbaiki.** Butir lama "Supplier Payment" menunjuk `/finance/payable/payment`, terbukti alias tipis dari `/finance/payment-ap/payment-ap-client` — rute kanonikal dipakai, permission disamakan dengan `[AccessPermission("FinancePayment","Read")]` pada `FinancePaymentsController.cs` (dikonfirmasi read-only) |
| Transaksi A/P | Laporan Pembayaran AP | `/finance/payable/report` | `Finance.AP : View` | Dipindah dari "Report AP" — pathname dan permission sama persis, direlabel. **Catatan:** berbeda dari Report AR/Laporan Aging AP, di sini rute kanonikalnya justru `/finance/ap-report` (`/finance/ap-report/page.jsx` mendefinisikan `ApReportClient` lokal sendiri di `./ap-report-client`, sedangkan `/finance/payable/report/page.jsx` meng-impornya lewat `@/app/finance/ap-report/ap-report-client`). §17.2 memilih `/finance/payable/report` (alias) sebagai pathname-nya — diikuti apa adanya karena pathname menu adalah wewenang UI terkunci `FIN-DEC-094`, bukan keputusan teknis task ini. Kedua rute tetap merender komponen yang identik sehingga tidak ada risiko fungsional |
| Transaksi A/P | Penerimaan Invoice | `/finance/purchasing/purchasing-invoices?stage=intake` | `FinancePurchasingInvoice : Read` | **Baru didaftarkan** — pandangan tersaring atas `/finance/purchasing/purchasing-invoices` yang sudah ada |
| Transaksi A/P | Utang Usaha (A/P Aging) | `/finance/payable/invoice` | `Finance.AP : View` | Dipindah dari "Faktur & Tagihan Supplier" — pathname dan permission sama persis, direlabel mengikuti V1 |

**Butir yang hilang dari sidebar akibat restrukturisasi ini** (route-nya **tidak dihapus**, hanya
tidak lagi punya jalan masuk menu): "Billing / Invoice" (`/finance/receivable/invoice`) dan
"Payment AR" (`/finance/receivable/payment`). Keduanya dibuktikan sebagai alias tipis: yang
pertama meng-import ulang komponen `FinanceReceivablePage` yang sama dengan "Receivable
AR/Invoice", yang kedua meng-import `PaymentArClient` dari `/finance/payment-ar` yang juga **tidak
pernah tertaut di menu sebelum maupun sesudah task ini**. Fungsinya tetap terjangkau lewat
"Receivable AR/Invoice" dan "Settlement AR" (`/finance/receipts`, layar penerimaan/alokasi yang
sesungguhnya menangani pencatatan pembayaran piutang di V1). Tidak ada kapabilitas yang hilang,
hanya jalan masuk duplikat yang dirapikan — ini bukan permintaan eksplisit `FIN-DEC-094`, melainkan
konsekuensi tak terhindarkan dari mengunci menu persis bentuk V1; dicatat di sini apa adanya untuk
ditinjau Product Owner bila dianggap perlu jalan masuk terpisah.

### 3.3 Kepatuhan arsitektur frontend

Task ini **hanya** mengubah satu berkas konfigurasi data statis (`src/utils/menu-sidebar/
menu-items.jsx`), tidak menyentuh `src/app`, `components`, `hooks`, Redux, atau service — sehingga
sebagian besar `rules/frontend/frontend-architecture.md` tidak relevan untuk task ini. Yang
relevan:

- Nol duplikasi helper/komponen/endpoint — butir baru memakai ulang route dan permission yang
  sudah ada, nol route baru dibuat.
- `requiredPermission` tetap bentuk `{ resource, action }`, konsisten dengan seluruh butir
  existing dan dengan kontrak `filterMenuItemsByPermission`.
- `key` tiap butir diverifikasi unik dalam file (diperiksa manual seluruh blok "Finance" yang
  diubah); `key` murni dipakai sebagai React list key dan penanda struktural oleh
  `filterMenuItemsByPermission` (`isMenuNode` memeriksa `typeof value.key === "string"`) — tidak
  direferensikan di tempat lain, sehingga aman diganti nama tanpa efek samping, dibuktikan lewat
  `grep -rn "menu-items" src` yang hanya menemukan tiga pemakai (`left-sidebar-items-virtualized.jsx`,
  file ini sendiri, dan filter-nya).
- Grup "Petty Cash" dan "Cash Monitoring" (dipetakan dari "Manajemen Kas"/"Pemantauan" pada
  §17.2, ditandai "tidak berubah") **tidak disentuh sama sekali** — dikonfirmasi lewat `git diff`
  yang hanya menunjukkan perubahan pada blok antara "Master Data" dan "Petty Cash".

---

## 4. State yang ditangani di layar

`NOT APPLICABLE` — task ini tidak membuat atau mengubah layar. Keempat state (memuat, kosong,
gagal, tanpa hak akses) pada layar tujuan setiap butir menu adalah tanggung jawab masing-masing
layar tersebut, bukan task restrukturisasi menu ini. Perilaku "tanpa hak akses" pada level menu
itu sendiri (butir/grup tersembunyi) tidak berubah — murni memakai `filterMenuItemsByPermission`
yang sudah ada dan tidak disentuh.

---

## 5. Endpoint yang dikonsumsi

`NOT APPLICABLE` — task ini tidak memanggil API apa pun. Seluruh 16 butir menu menunjuk ke layar
yang sudah mengonsumsi endpointnya masing-masing sejak sebelum task ini.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Berhasil tanpa error (`eslint . --quiet`, exit code 0) | `PASS` | Keluaran perintah |
| `npm run test:unit` | — | `NOT RUN` | Menunggu konfirmasi eksplisit pengguna |
| `npm run build` | — | `NOT RUN` | Menunggu konfirmasi eksplisit pengguna |
| Review diff/scope | 1 berkas berubah, persis sesuai `03-frontend-architecture.md` §17.2 | `PASS` | `git status --short` |
| Review 16 pathname terhadap route yang benar-benar ada | Seluruhnya dikonfirmasi punya `page.jsx` lewat `find src/app/finance -iname page.jsx` | `PASS` | §3.1, §3.2 |
| Review "13 butir" pada roadmap vs 16 butir yang didaftarkan | **Bukan kontradiksi** — "13 butir" pada roadmap menghitung 13 rute dasar unik yang sudah ada; 16 butir menu didaftarkan karena 2 pasang butir berbagi rute dasar yang sama dengan parameter query berbeda (`/finance/receivable` dipakai "Receivable AR/Invoice" dan "Piutang Tagihan"; `/finance/purchasing/purchasing-invoices` dipakai "Purchasing Invoice" dan "Penerimaan Invoice") | `PASS` — didokumentasikan, bukan error | Perhitungan manual, lihat juga §8 |
| Review `key` unik dalam blok yang diubah | Tidak ada duplikat | `PASS` | Pembacaan manual seluruh blok "Finance" |

Uji manual: `NOT FEASIBLE` — `npm run build`/`npm run dev` belum dijalankan pada task ini (server
tidak aktif), sehingga sidebar tidak dapat diverifikasi secara visual di browser. Verifikasi
dilakukan lewat pembacaan source dan `find` atas `page.jsx` yang benar-benar ada, bukan tangkapan
layar.

**Tidak dijalankan:** `npm run test:unit`, `npm run build` (keduanya menunggu konfirmasi eksplisit
pengguna); `npm run test:e2e`/`npm run test:uat` (tidak diminta, dan environment server tidak
aktif); verifikasi manual visual di browser (server tidak dijalankan).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria (dari `02-frontend-roadmap.md` baris `FE-FIN-016`) | Status | Bukti |
| --- | --- | --- |
| Kedua grup tampil dengan label dan urutan V1 | Terpenuhi (source) | §3.2 — urutan butir dalam tiap grup mengikuti urutan persis `03-frontend-architecture.md` §17.2, dikurangi butir yang belum punya layar |
| **Nol** butir mengarah ke rute yang belum ada | Terpenuhi | §3.1, §6 — seluruh 16 `pathname` diverifikasi punya `page.jsx` |
| Butir yang dijaga `Finance.AP`/`Finance.AR` V2 tidak berubah hak aksesnya | Terpenuhi | Kelima butir V2 payung (Report AR, Laporan Aging AR, Laporan Aging AP, Laporan Pembayaran AP, Utang Usaha (A/P Aging)) tetap memakai `Finance.AR : View`/`Finance.AP : View` persis seperti sebelumnya — nol perubahan permission pada kelimanya |
| Lint PASS | Terpenuhi | `npm run lint:errors` PASS |
| Build PASS | **Belum terpenuhi** | `npm run build` `NOT RUN`, menunggu konfirmasi pengguna |
| Laporan task tracked ada | Terpenuhi | Laporan ini |
| Roadmap ditandai | Terpenuhi setelah laporan ini — lihat pembaruan `02-frontend-roadmap.md` | — |

Task ini **belum** dapat ditandai ✅ selama `npm run build` belum dikonfirmasi pengguna —
konsisten dengan kriteria "Build PASS" yang eksplisit pada roadmap.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `npm run build` belum dijalankan sama sekali untuk task ini — bukan "dijalankan lalu gagal", melainkan sengaja tidak dijalankan mengikuti konvensi sesi saat ini. Perubahan hanya berupa data konfigurasi menu (bukan JSX/komponen), dan `npm run lint:errors` sudah PASS, sehingga risiko kegagalan build murni dari perubahan ini dinilai rendah — tetap **tidak** dianggap terbukti sampai dijalankan |
| Masalah yang diketahui | Dua butir ("Billing / Invoice", "Payment AR") kehilangan jalan masuk menu akibat restrukturisasi — dibuktikan sebagai alias/duplikat tak-tertaut, bukan kapabilitas yang hilang (lihat §3.2). Permission "Receivable AR/Invoice" dan "Piutang Tagihan" berubah dari `Finance.AR : View` (V2 payung) ke `FinanceReceivable : Read` (granular) — pengguna yang hanya memegang hak V2 payung tanpa hak granular `FinanceReceivable : Read` akan kehilangan akses ke dua butir ini sampai admin menyesuaikan peran lewat layar Manajemen Role, **ini konsekuensi `FIN-DES-069` yang sudah berlaku, bukan temuan baru dari task ini** |
| Dependency backend | Tidak ada — seluruh 16 endpoint yang dirujuk sudah ada dan tidak disentuh task ini |
| Perubahan sampingan | Import `RiShoppingCart2Line` dicabut karena menjadi tidak terpakai setelah submenu "Pembelian" dihapus — bagian langsung dari task ini, bukan cleanup terpisah |
| Interupsi | `NONE` |
| Status Git | `git status --short` menunjukkan tepat satu berkas berubah: `src/utils/menu-sidebar/menu-items.jsx` |
| Langkah berikutnya | (1) Pengguna menjalankan `npm run build` (dan `npm run test:unit` bila diperlukan) serta mengonfirmasi hasilnya — baru setelah itu status task ini dapat naik ke ✅; (2) `FE-FIN-017`/`018` (`REV-13A`, berdiri sendiri, nol pekerjaan backend) sebagai lanjutan tercepat |
