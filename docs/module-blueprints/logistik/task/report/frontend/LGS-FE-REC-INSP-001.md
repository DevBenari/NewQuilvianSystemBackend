# Laporan Perubahan Frontend — `LGS-FE-REC-INSP-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `LGS-FE-REC-INSP-001` (task ad-hoc, 27 November 2026) |
| Judul | Pisahkan halaman Logistik "Pemeriksaan Hasil Pengadaan" dari Penerimaan Barang Finance |
| Slice | Pemisahan halaman Logistik dari Finance (pasangan backend: `LGS-BE-REC-INSP-001`) |
| Roadmap | Tidak ada (blueprint Logistik belum punya roadmap); tidak ada tanda status yang dipasang |
| Trace | `LGS-DEC-001`, `FIN-DEC-261`, `FIN-DEC-268`, `LGS-OQ-015`, `LGS-OQ-017`, `LGS-OQ-018` |
| Contract version | Kontrak baca baru dari `LGS-BE-REC-INSP-001`; belum bertversi/disetujui sebagai kontrak target |
| Wewenang UI | Instruksi tertulis pemilik: route `/logistics/receiving/procurement-inspections`, judul, deskripsi, ringkasan, filter, kolom tabel, dan isi detail. Susunan visual mengikuti pola halaman list dan `BaseDetailView` yang sudah ada |
| Dependency | `LGS-BE-REC-INSP-001` — source selesai, **belum dibuild**. Halaman memanggil endpoint yang baru ada setelah backend dibuild dan dijalankan |
| Klasifikasi | `MEDIUM` — skor 8 (repository 0, diperiksa 1, diubah 2, logika 1, API 1, database 0, keamanan 1, UI 2) |
| Task mode | `CROSS-REPO` terbatas: source frontend + laporan di repository backend |
| Target tulis | `QuilvianSystemFrontendDev` (source) dan laporan ini |
| Model | Claude Sonnet 5.5 |
| Commit frontend saat dikerjakan | `533b21dc6a6f6c630a79071fc75dbf2b825371eb` (branch `yasmina`), ditambah perubahan yang belum di-commit dari task `FIN-FE-LGS-LINK-001` |
| Commit backend yang dijadikan rujukan | `1cd7b477dc48799acc68792aea534d348b902975` (branch `Yasmina`) |
| Tanggal | 27 November 2026 |
| Status | 🟡 **Source selesai; tampilan belum dilihat.** `VISUAL CHECK: NOT FEASIBLE`, `MANUAL TEST: NOT FEASIBLE`, `npm run build` `NOT RUN` |

---

## 1. Keadaan yang ditemukan di awal

| Temuan | Bukti |
| --- | --- |
| Butir menu Logistik "Pemeriksaan Hasil Pengadaan" (key `healthServicesLogisticsReceivingProcurementInspection`) menunjuk `/finance/purchasing/goods-receipts` dan dijaga `FinanceGoodsReceipt : Read` | `src/utils/menu-sidebar/menu-items.jsx` baris 4215–4223 sebelum perubahan |
| Direktori `src/app/logistics` belum ada | Pencarian berkas sebelum implementasi |
| Halaman Finance memang dibuat untuk memantau dokumen GR (kolom Nomor GR, Nomor PO, Tanggal, Status) | `goods-receipt-view.jsx`, `goods-receipt-table-columns.jsx` |
| Butir menu Finance `financePurchasingGoodsReceipt` terpisah dan mengarah ke halaman Finance | `menu-items.jsx` baris 3776–3783 |

---

## 2. Proses bisnis dari sisi pengguna

1. Petugas Logistik membuka **Logistik & Manajemen Persediaan → Penerimaan Barang → Pemeriksaan Hasil Pengadaan**. Butir ini hanya tampil bila ia memiliki `LogisticsProcurementInspection : Read`.
2. Halaman memuat tujuh kartu ringkasan (hitungan dokumen/item) dan tabel baris pemeriksaan. Satu baris = satu barang pada satu dokumen penerimaan.
3. Petugas menyaring menurut periode terima, supplier, kondisi barang, sumber dokumen, status dokumen, atau mengetik nama barang/nomor GR/nomor PO pada kotak cari.
4. Petugas menekan **Lihat Pemeriksaan** (atau klik dua kali baris) untuk membuka detail dokumen: identitas GR/PO/supplier, status penghubung persediaan, dan rincian per barang.
5. Petugas kembali ke daftar dengan **Kembali**.

**Jalur tidak normal.**

| Kejadian | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tabel menampilkan kerangka pemuatan "Mengambil data pemeriksaan hasil pengadaan..."; kartu menampilkan skeleton |
| Kosong | "Data pemeriksaan hasil pengadaan tidak ditemukan." + "Coba gunakan saringan lain." |
| Gagal memuat daftar | Peringatan merah berisi pesan backend, atau pesan bahasa Indonesia sesuai kode HTTP; tombol **Muat Ulang** di header |
| Ringkasan gagal | Peringatan kuning; seluruh kartu tampil "—" (bukan 0). Tabel tetap tampil |
| Tanpa hak akses (`401`/`403`) | Halaman diganti `AccessDeniedGate` ("Ups! Akses Ditolak"). Berlaku juga saat membuka URL langsung |
| Tanpa izin `ReadDetail` | Tombol **Lihat Pemeriksaan** nonaktif; klik dua kali baris tidak membuka apa pun |
| Alamat detail bukan GUID | Rute menampilkan halaman tidak ditemukan; tidak ada permintaan ke backend |
| Dokumen tidak ditemukan (`404`) | Pesan "Dokumen penerimaan tidak ditemukan." |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`menu-items.jsx`, `filter-menu-items-by-permission.jsx`, `use-permission.jsx`, `access-denied-gate.jsx`, `access-denied-utils.jsx`, halaman/hook/kolom/konstanta Finance Goods Receipt, `goods-receipt-logistics-utils.js`, `base-detail-view.jsx`, `data-table.jsx`, `filter-date-picker.jsx`, `resource-filter-select.jsx`, `use-select-resource.jsx`, `select-resource-registry.js`, `finance-select-resources.js`, katalog base component, pola komposisi halaman, dan checklist UI.

### 3.2 Berkas yang berubah

**Baru**

| Berkas | Isi |
| --- | --- |
| `src/app/logistics/receiving/procurement-inspections/page.jsx` | Route tipis daftar |
| `src/app/logistics/receiving/procurement-inspections/[goodsReceiptId]/page.jsx` | Route tipis detail; menolak ID yang bukan GUID dengan `notFound()` |
| `src/components/view/logistics/receiving/procurement-inspection-view.jsx` | Halaman daftar |
| `src/components/view/logistics/receiving/procurement-inspection-detail-view.jsx` | Halaman detail |
| `src/components/view/logistics/receiving/procurement-inspection-table-columns.jsx` | Kolom daftar dan kolom rincian barang |
| `src/lib/hooks/logistics/receiving/use-procurement-inspection-list.jsx` | Controller daftar + ringkasan (dua `GET`, `AbortController`) |
| `src/lib/hooks/logistics/receiving/use-procurement-inspection-detail.jsx` | Controller detail |
| `src/lib/hooks/select/logistics/logistics-select-resources.js` | Sumber pilihan supplier dari endpoint Logistik |
| `src/lib/hooks/select/logistics/use-logistics-select.js` | Hook pilihan |
| `src/lib/constants/logistics/receiving/procurement-inspection-constants.js` | Endpoint, route, izin, label, opsi, salinan teks |
| `src/utils/logistics/receiving/procurement-inspection-utils.js` | Fungsi murni: parameter, format, deskripsi kondisi, ringkasan, pesan galat |
| `tests/unit/logistics-procurement-inspection.test.mjs` | 16 tes statis |

**Diubah**

| Berkas | Perubahan |
| --- | --- |
| `src/utils/menu-sidebar/menu-items.jsx` | Hanya butir `healthServicesLogisticsReceivingProcurementInspection`: `pathname` → `/logistics/receiving/procurement-inspections`, `requiredPermission` → `LogisticsProcurementInspection : Read`. **Key tidak diubah** |
| `src/lib/hooks/select/select-resource-registry.js` | Mendaftarkan `LOGISTICS_SELECT_RESOURCES` (impor, penggabungan `SELECT_RESOURCES`, dan pencarian di `getSelectResource`) |

**Tidak diubah (dan tidak perlu diubah):** halaman, hook, kolom, dan konstanta Finance Goods Receipt; menu Finance `financePurchasingGoodsReceipt`; kunci menu Logistik lain. Perubahan Finance yang tampak di `git status` berasal dari task terpisah `FIN-FE-LGS-LINK-001`.

### 3.3 Kepatuhan arsitektur frontend

Alur `app → view → hook → InstanceAxios` diikuti; route tipis; view tidak memanggil Axios; endpoint dan label ada di constants; fungsi transformasi murni di utils; semua request meneruskan `signal` dan dibatalkan saat unmount; tidak ada Redux slice baru (state lokal, mengikuti pola Finance Goods Receipt); tidak ada Axios instance baru; alamat memakai `@/`. Komponen Finance tidak diimpor; satu-satunya impor lintas domain adalah fungsi murni `describeGoodsReceiptLogisticsLink` (status penghubung persediaan) agar kosakata status tidak digandakan.

### 3.4 Gerbang tampilan

**Modul referensi visual:** `src/components/view/finance/purchasing/goods-receipt/goods-receipt-view.jsx` (list) dan `src/components/view/health-services/pharmacy-management/stock-request-view/` (modul Logistik terdekat).

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Header | `Hero` (`eyebrow` teks `Logistik › Penerimaan Barang › …`) | `REUSE` | Gaya bawaan |
| Peringatan batas halaman | `InformationAlert` | `REUSE` | — |
| Kartu ringkasan | `SummaryGrid` | `REUSE` | Nilai `—` bila tidak diketahui |
| Filter | `DataFilter`, `FilterDatePicker` (prop `min`/`max` opt-in yang sudah ada), `FilterSelect` | `REUSE` | — |
| Pilihan supplier | `ResourceFilterSelect` + registry select | `EXTEND` (data, bukan komponen) | Menambah satu entri `procurementInspectionSuppliers` pada registry; perilaku komponen tidak berubah, pemakai lain tidak terdampak |
| Nomor GR/PO dan nama barang | Kotak cari `DataFilter` | `REUSE` | Tidak ada komponen filter teks tambahan di `base-features`; tiga saringan teks digabung menjadi satu kotak cari |
| Tabel | `DataTable`, `Pagination`, kelas sel dari `administrator-region-table.module.css` | `REUSE` | Kolom di berkas `…-table-columns.jsx` |
| Badge kondisi/status | `StatusBadge` dengan kunci `status` (`active`, `danger`, `warning`, `info`, `inactive`) | `REUSE` | Tanpa prop `variant` |
| Tombol aksi baris | `BaseButton variant="secondary" size="sm"`, kolom `align: "center"` | `REUSE` | Sejajar judul kolom Aksi |
| Detail | `BaseDetailView` (+ `afterDetail` berisi `InformationAlert` dan `DataTable`) | `COMPOSE` | Hanya tombol Kembali |
| Penjaga akses | `AccessDeniedGate`, `usePermission` | `REUSE` | — |
| CSS | Tanpa CSS Module baru; hanya `base-data-components`, `administrator-region-table`, `administrator-region-tokens` | `REUSE` | Sumber gaya urutan 1–2 |

`UI GATE: PASS — 12 elemen: REUSE 10, EXTEND 1 (data registry), COMPOSE 1, WRAP 0, NEW 0`

Hasil pemeriksa dan grep anti-regresi, ditempel apa adanya:

```text
$ node <rules>/frontend/tools/check-css-usage.mjs <5 berkas jsx logistik>
OK — 5 berkas diperiksa, 0 temuan.
$ grep -nE "<button|className=\"btn|<table|<input|<select|<textarea|UUID|GUID|fixed inset-0|!important" src/components/view/logistics/receiving/*.jsx
(kosong)
$ grep -nE "<pola Tailwind #7>" src/components/view/logistics/receiving/*.jsx
(kosong)
$ grep -nE "fw-(bold|semibold|light)|fs-[0-9]" src/components/view/logistics/receiving/*.jsx
(kosong)
```

| Layar | Screenshot | Catatan |
| --- | --- | --- |
| `/logistics/receiving/procurement-inspections` | — | `VISUAL CHECK: NOT FEASIBLE` — `npm run dev`/dev server dilarang pada sesi ini |
| `/logistics/receiving/procurement-inspections/{id}` | — | `VISUAL CHECK: NOT FEASIBLE` — alasan sama; juga membutuhkan backend baru yang sudah dibuild |

Tabel 16 kolom (daftar) lebih lebar dari layar; `DataTable` menggulir horizontal. Responsivitas ponsel **belum dilihat**.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kerangka tabel dan skeleton kartu |
| Kosong | "Data pemeriksaan hasil pengadaan tidak ditemukan." |
| Gagal | Pesan backend atau pesan Indonesia per kode HTTP (`error.message` Axios tidak pernah ditampilkan); tombol Muat Ulang |
| Tanpa hak akses | `AccessDeniedGate` |
| Nilai tidak ada | "-" atau "Belum Tersedia"; tidak pernah 0 yang dikarang |
| Kondisi tidak ada | Lencana abu-abu "Belum Ada Data Pemeriksaan", **bukan** "Baik" |
| Dokumen dibatalkan | Lencana "Dokumen Dibatalkan"; kuantitas "-" |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Logistics Management / Receiving / Procurement Inspection

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/logistics-management/receiving/procurement-inspections` | Tabel daftar | `LogisticsProcurementInspection : Read` |
| `GET` | `…/summary` | Tujuh kartu ringkasan | `LogisticsProcurementInspection : Read` |
| `GET` | `…/supplier-options` | Pilihan supplier pada saringan | `LogisticsProcurementInspection : Read` |
| `GET` | `…/{goodsReceiptId}` | Halaman detail | `LogisticsProcurementInspection : ReadDetail` |

Halaman **tidak** memanggil endpoint Finance mana pun.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `eslint` pada seluruh berkas baru/diubah (view, app, hooks, select, constants, utils, menu, tes) | 0 error, 0 warning (satu peringatan `set-state-in-effect` pada percobaan pertama diperbaiki) | `PASS` | Keluaran perintah |
| `check-css-usage.mjs` pada 5 berkas JSX | `OK — 5 berkas diperiksa, 0 temuan` | `PASS` | Bagian 3.4 |
| `node --import ./tests/helpers/register.mjs --test tests/unit/logistics-procurement-inspection.test.mjs` | 16 lulus, 0 gagal | `PASS` | Bukti teks/fungsi murni, **bukan UAT runtime** |
| Tujuh berkas tes terkait (logistik baru, link Finance–Logistik, pemisahan modul Purchasing, GR satuan dasar, AP–BLM, AP flow, progres item BLM) | 105 lulus, 0 gagal | `PASS` | Keluaran perintah |
| Tes lama menu/sidebar (`crm-membership-menu`, `hemodialysis-sidebar-navigation`, `hr-master-data-sidebar-grouping`) | 6 gagal | `EXISTING / ENVIRONMENT ISSUE` | Gagal sama persis (6 dari 6) ketika `menu-items.jsx` dikembalikan sementara ke versi `HEAD`, lalu versi saya dipulihkan. Tidak disebabkan task ini |
| Rute dan izin FE ↔ `[Route]`/`ControllerName` controller backend | Cocok | `PASS` (statis) | Tes kontrak membaca source backend |
| `npm run build` | Tidak dijalankan | `NOT RUN` | Dilarang pada sesi ini |
| Layar dibuka di peramban, filter/paginasi/detail dicoba | Tidak dijalankan | `NOT RUN` / `NOT FEASIBLE` | Dev server dilarang; backend belum dibuild |

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test … — PASS (16 tes baru; 105 pada tujuh berkas terkait)`

`MANUAL TEST: NOT FEASIBLE` — dev server dan build dilarang pada sesi ini; backend baru belum dibuild.

---

## 7. Acceptance criteria dan Definition of Done

`PASS (statis)` = terbukti dari source/tes teks. Tidak satu pun butir di bawah dijalankan di peramban, API, atau database.

| AC | Kriteria | Status | Bukti |
| --- | --- | --- | --- |
| 01 | Menu Finance Penerimaan Barang tetap membuka halaman Finance | PASS (statis) | Tes menu; `financePurchasingGoodsReceipt` tidak berubah |
| 02 | Menu Logistik membuka halaman Logistik baru | PASS (statis) | `menu-items.jsx`, tes menu |
| 03 | Breadcrumb berbeda | PASS (statis) | `Finance › Purchasing › Penerimaan Barang` vs `Logistik › Penerimaan Barang › Pemeriksaan Hasil Pengadaan` |
| 04 | Tujuan dan kolom berbeda | PASS (statis) | 16 kolom pemeriksaan vs 7 kolom dokumen Finance |
| 05 | Izin Logistik tersendiri | PASS (statis) | `LogisticsProcurementInspection` |
| 06 | Pengguna Logistik tidak wajib `FinanceGoodsReceipt:Read` | PASS (statis) | Tes: view/hook Logistik dan controller tidak memuat `FinanceGoodsReceipt`; supplier dari endpoint Logistik |
| 07 | Tanpa izin Logistik tidak bisa membuka halaman | PASS (statis) / NOT RUN | Sidebar menyembunyikan; `AccessDeniedGate` pada 401/403; belum dijalankan |
| 08 | URL langsung tetap diperiksa izinnya | PASS (statis) / NOT RUN | Backend `[AccessPermission]` + `AccessDeniedGate` |
| 09 | Daftar menampilkan data nyata dari backend | NOT RUN | Butuh backend dibuild |
| 10 | Tanpa data dummy/hardcode | PASS (statis) | Seluruh isi tabel dari respons |
| 11–14 | Filter tanggal, PO/GR (kotak cari), supplier, kondisi bekerja | NOT RUN | Parameter terkirim dan nama cocok BE (tes statis) |
| 15 | Paginasi | NOT RUN | `DataTable` + `Pagination`; `totalPage` dari backend |
| 16 | Detail memakai ID GR dan item yang benar | PASS (statis) | Rute `{goodsReceiptId:guid}`; item dicocokkan `PurchaseOrderItemId` |
| 17–19 | GOOD/DAMAGED/PENDING_INSPECTION tampil sesuai sumber | PASS (statis) | `describeInspectionCondition`; badge `active`/`danger`/`warning` |
| 20 | Kondisi kosong tidak dianggap GOOD | PASS (statis) | Tes "kondisi kosong … TIDAK pernah dibaca Baik" |
| 21–22 | Rusak dan menunggu periksa tidak masuk jumlah sah | PASS (statis) | Kalkulator Finance di backend; FE tidak menghitung |
| 23 | Konversi mengikuti aturan backend | PASS (statis) | `PurchasingReceiptQuantityCalculator`; FE tidak membuat formula |
| 24–25 | Batch dan kedaluwarsa tampil sesuai data | PASS (statis) | Kolom Batch/Lot dan Tanggal Kedaluwarsa; kosong "-" |
| 26–27 | Beberapa batch / penerimaan sebagian per item PO | PASS (statis) / NOT RUN | Satu baris per baris GR; kolom kumulatif PO pada detail (BLM) |
| 28 | GR `CANCELLED` dibedakan | PASS (statis) | Lencana "Dokumen Dibatalkan", kuantitas "-", filter status dokumen |
| 29 | Tidak ada dokumen duplikat | PASS (statis) | Tidak ada aksi tulis |
| 30 | Tidak ada penambahan saldo stok | PASS (statis) | Tidak ada `ReceiveBatchAsync`/`DrugStockService`; hanya `GET` |
| 31 | Tidak ada perubahan transaksi Finance | PASS (statis) | Berkas Finance tidak disentuh task ini |
| 32 | Halaman responsif | NOT RUN | `VISUAL CHECK: NOT FEASIBLE` |
| 33 | Tombol aksi sejajar kolom AKSI | PASS (statis) / NOT RUN | `align: "center"` pada kolom `actions`; belum dilihat |
| 34 | Loading dan empty state benar | PASS (statis) / NOT RUN | `DataTable` + teks baku |
| 35 | Backend tidak mengirim harga | PASS (statis) | Tes: DTO dan service tidak memuat `UnitPrice`/`LineTotal`/`TotalAmount` |
| 36 | Integrasi BLM mengikuti kepemilikan sumber | PASS (statis) | Tidak ada tulis ke GR; kondisi dibaca apa adanya |
| 37 | Tidak ada perubahan status akibat GET | PASS (statis) | Service `AsNoTracking`, tanpa `SaveChanges` |
| 38 | Tidak ada migration/perubahan database | PASS | Tidak ada berkas migration; tidak ada DDL/DML |

**Belum terpenuhi / ditahan:** build FE dan BE, pemeriksaan visual, uji manual, UAT, dan seluruh aksi pencatatan hasil pemeriksaan (lihat bagian 8).

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tampilan belum pernah dilihat. Tabel 16 kolom perlu dinilai di layar desktop dan ponsel |
| Masalah yang diketahui | Filter Nomor PO, Nomor GR, dan nama barang digabung menjadi satu kotak cari karena tidak ada komponen filter teks tambahan; Nomor PO/GR tidak lagi punya kotak sendiri |
| Dependency backend | `LGS-BE-REC-INSP-001` harus dibuild dan dijalankan, dan admin mencentang `Read` + `ReadDetail` pada Akses Role. Tanpa itu halaman menampilkan Akses Ditolak atau galat |
| **BLOCKED / NEEDS CONFIRMATION — aksi pencatatan hasil pemeriksaan** | Tidak ada tombol Periksa/Setujui/Tolak/Karantina. Menunggu `LGS-OQ-015` (pemilik dan penyetuju), kepemilikan hasil pemeriksaan (Quilvian atau BLM), aturan koreksi/pembatalan, dan skema penyimpanan baru. Status pemeriksaan (Menunggu → Dalam Pemeriksaan → Selesai; keputusan Diterima / Diterima Sebagian / Ditahan / Ditolak) hanya usulan |
| **BLOCKED — integrasi stok** | Halaman tidak menambah saldo; menunggu `LGS-OQ-017` dan `LGS-OQ-018` |
| Rute detail | Memakai GUID di URL, sama seperti detail Purchase Order Finance; tidak memakai token rute privat karena halaman ini bukan master data |
| Perubahan sampingan | `NONE`. Berkas `menu-items.jsx` sempat dikembalikan sementara ke `HEAD` untuk membandingkan 6 tes lama yang gagal, lalu versi saya dipulihkan (selisih akhir: 2 baris tambah, 2 baris hapus) |
| Interupsi | `NONE` |
| Status Git | Berkas baru di `src/app/logistics/`, `src/components/view/logistics/`, `src/lib/**/logistics/`, `src/utils/logistics/`, tes baru; `menu-items.jsx` dan `select-resource-registry.js` diubah; tidak ada stage/commit/push |
| Langkah berikutnya | (1) Build BE lalu FE; (2) centang izin Logistik di Akses Role; (3) buka kedua layar dengan data uji (GR lokal, GR BLM menunggu periksa, GR BLM rusak, GR dibatalkan) dan simpan screenshot; (4) keputusan pemilik atas `LGS-OQ-015/017/018` sebelum merancang aksi tulis |
