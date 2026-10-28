# Laporan Perubahan Frontend — `FE-FIN-053`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-053` |
| Judul | Petugas Finance mencatat penerimaan barang per batch lengkap dengan kedaluwarsa, melihat sisa yang dapat diterima, dan membuka kembali rincian batch |
| Slice | `REV-22` (Finance AP mengikuti BLM, sisi frontend) — paket `FE-AP-02` pada dokumen konsolidasi audit |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md`, bagian "AMENDMENT ROADMAP REVISI 22", tabel Task REV-22 Frontend |
| Trace | `AP-UAT-06` (beberapa batch dan kedaluwarsa: detail tidak hilang); `FIN-VAL-104` (akumulasi kuantitas); `AUD-AP` Bagian IX butir "Batch/expired GR" |
| Contract version | `FIN-API-1.1` §B.2 ditambah `batchNumber` dan `expiredDate` pada baris GR (aditif, delta kontrak `BE-FIN-125`) |
| Wewenang UI | `DEV_DISCRETION` untuk tata letak modal; mengikat untuk aturan sisa kuantitas, label "Sisa", "No. Batch", "Kedaluwarsa", dan pengiriman satu baris PO sebagai beberapa item |
| Dependency | `BE-FIN-125` 🟡 [BE] — source ada, migration `20261026120000_AddPurchasingApBlmComponents` **belum diterapkan** |
| Klasifikasi | `MEDIUM` — 1 hook, 2 view, 1 hook daftar, 1 utilitas murni baru, 1 berkas unit test |
| Task mode | `FRONTEND MODE` — target tulis `QuilvianSystemFrontendDev`; backend dibaca read-only |
| Target tulis | `QuilvianSystemFrontendDev`; `NewQuilvianSystemBackend/docs/module-blueprints/finance-management/` (laporan dan roadmap saja) |
| Model | Claude Sonnet 5.5 |
| Commit frontend saat dikerjakan | `7257982bb` (branch `yasmina`, perubahan belum di-commit) |
| Commit backend yang dijadikan rujukan | `cfe22fe8` (branch `Yasmina`) |
| Tanggal | 26 Oktober 2026 |
| Status | 🟡 **SEBAGIAN** — source selesai, lint dan unit test lulus; build dan uji manual di browser belum dijalankan |

---

## 1. Keadaan yang ditemukan di awal

1. Backend (`BE-FIN-125`) menyimpan `BatchNumber` dan `ExpiredDate` per baris GR, dan sejak awal menghitung akumulasi kuantitas per baris PO
   dari seluruh GR yang belum dibatalkan (`FIN-VAL-104`). Satu baris PO yang dikirim sebagai beberapa item (beberapa batch) diterima.
2. Modal **Catat Tanda Terima Barang** (di detail Purchase Order) hanya punya satu baris per item PO dan kolom Qty Diterima serta Catatan.
   Tidak ada isian batch maupun kedaluwarsa, dan tidak ada cara memecah satu item ke beberapa batch.
3. Batas isian Qty Diterima adalah **kuantitas PO awal**, bukan sisa. Setelah penerimaan sebagian, layar tetap membiarkan pengguna mengetik
   angka yang pasti ditolak backend.
4. Riwayat GR pada detail PO hanya menampilkan nomor, tanggal, dan status. Batch dan kedaluwarsa yang sudah tersimpan **tidak dapat dilihat di layar mana pun**.
5. Daftar **Penerimaan Barang** tidak punya jalan menuju dokumen yang ditelusuri.

---

## 2. Proses bisnis dari sisi pengguna

Pengguna: staf Finance / penerima barang (hak `FinanceGoodsReceipt : Create` dan `Read`).

1. Staf membuka **Transaksi A/P → Pengadaan & Penerimaan → Pesanan Pembelian (PO)**, memilih PO berstatus Disetujui / Diterima Sebagian, lalu **Catat Penerimaan Barang**.
2. Modal terbuka dan menghitung **Sisa** tiap item = kuantitas PO − seluruh GR aktif (GR yang dibatalkan tidak dihitung). Selama dihitung, tombol simpan terkunci.
3. Staf mengisi **Qty Diterima**, **No. Batch** (opsional, maksimal 100 karakter), **Kedaluwarsa** (opsional), dan **Catatan**.
4. Bila satu item datang dalam dua batch, staf menekan **Batch** pada baris itu. Muncul baris "Batch tambahan" tepat di bawahnya. Contoh: PO 100 unit Paracetamol —
   baris asal 60 unit batch B-001 kedaluwarsa 30 Juni 2027; baris tambahan 40 unit batch B-002 kedaluwarsa 31 Desember 2027. Keduanya dikirim sebagai dua item dalam satu GR.
5. Staf menekan **Simpan Tanda Terima Barang**. Berhasil → notifikasi dengan nomor GR, modal tertutup, riwayat diperbarui.
6. Untuk melihat hasilnya, staf menekan **Lihat Rincian** pada GR di riwayat: tabel baris item beserta qty, no. batch, kedaluwarsa, dan catatan terbuka.
7. Dari daftar **Penerimaan Barang**, klik dua kali sebuah baris membuka PO induknya (tempat rincian batch dilihat).
8. **Jalur tidak normal:**
   - Total batch satu item melebihi sisa → "Kuantitas diterima untuk {produk} melebihi sisa yang dapat diterima ({sisa})." Tidak ada permintaan dikirim.
   - Item sudah diterima penuh (sisa 0) → baris dinonaktifkan, tombol Batch dinonaktifkan.
   - Riwayat GR gagal dimuat → peringatan kuning; batas sementara = kuantitas PO; backend tetap menjaga akumulasi saat simpan.
   - Nomor batch lebih dari 100 karakter → pesan di layar; tidak dikirim.
   - Backend menolak (mis. melebihi pesanan `422`) → pesan backend tampil di modal.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`; `rules/frontend/*`; `use-purchase-order-detail.jsx`, `purchase-order-detail-view.jsx`, `use-goods-receipt-list.jsx`, `goods-receipt-view.jsx`, `goods-receipt-constants.jsx`;
backend `FinanceGoodsReceiptDtos.cs`, `FinanceGoodsReceiptService.cs` (`CreateAsync`: akumulasi dan validasi batch), `FinancePurchaseOrderDtos.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/finance/purchasing/goods-receipt-utils.jsx` (**baru**) | Fungsi murni: `sumReceivedByPurchaseOrderItem` (GR `CANCELLED` tidak dihitung), `calculateRemainingQuantity`, `buildGoodsReceiptRows`, `addBatchRowAfter`, `removeBatchRow`, `validateGoodsReceiptRows`, `buildGoodsReceiptItemsPayload` |
| `src/lib/hooks/finance/purchasing/use-purchase-order-detail.jsx` | Modal memuat sisa dari rincian GR aktif; baris batch tambahan; validasi dan payload memakai utilitas; `batchNumber` dan `expiredDate` ikut terkirim; `loadGoodsReceiptDetail` dan `toggleGoodsReceiptDetail` untuk rincian riwayat (disimpan sehingga tidak diambil dua kali, dibuang saat GR dibatalkan); endpoint GR dipakai dari `goods-receipt-constants.jsx` (sebelumnya didefinisikan ulang lokal) |
| `src/components/view/finance/purchasing/purchase-order/detail/purchase-order-detail-view.jsx` | Modal `modal-xl` dengan kolom Sisa, No. Batch, Kedaluwarsa, dan tombol **Batch** / hapus batch tambahan (`BaseButton`); peringatan dan penanda "Menghitung sisa…"; tombol **Lihat Rincian** dan tabel rincian pada riwayat GR (`data-flat-table="true"`) |
| `src/lib/hooks/finance/purchasing/use-goods-receipt-list.jsx` dan `src/components/view/finance/purchasing/goods-receipt/goods-receipt-view.jsx` | Klik dua kali baris membuka PO induk; deskripsi filter dan tooltip baris diperbarui |
| `tests/unit/finance-purchasing-ap-blm.test.mjs` (**baru**) | 8 test untuk task ini (sisa dari GR aktif, sisa tidak diketahui, sisip/hapus batch, dua batch jadi dua item, melebihi sisa, payload tanpa batch/kedaluwarsa, batas sementara, batas 100 karakter) |

### 3.3 Kepatuhan arsitektur frontend

**UI GATE: 4 elemen — REUSE 4, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0** (tombol **Batch**/hapus: `BaseButton`; sel isian: `input.form-control-sm` pola modal yang sama; tabel rincian riwayat: tabel Bootstrap pola modul ini dengan `data-flat-table="true"`; peringatan: `alert alert-warning` pola modal yang sama).

Aturan hitung dikeluarkan dari hook ke `src/utils/finance/purchasing/goods-receipt-utils.jsx` (murni, tanpa React/Axios). Endpoint tidak lagi ganda (diambil dari konstanta domain).
Seluruh tombol baru memakai `BaseButton`; tidak ada `<button>` mentah, CSS, token, atau komponen baru. Sel input tetap `input.form-control-sm` karena komentar existing pada berkas yang sama
menyatakan `BaseTextField` memaksa label per instance dan tidak cocok untuk sel tabel padat.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Menghitung sisa kuantitas yang dapat diterima..." di modal (simpan terkunci); tombol **Lihat Rincian** menampilkan "Memuat..." |
| Kosong | Item tanpa penerimaan sebelumnya: Sisa = kuantitas PO; riwayat GR kosong tidak ditampilkan (perilaku lama) |
| Gagal | Peringatan kuning di modal bila riwayat gagal dimuat; kotak galat merah bila simpan ditolak; notifikasi galat bila rincian GR gagal dibuka |
| Tanpa hak akses | Aksi ditolak backend `403`; pesan backend tampil di modal / notifikasi |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Finance Management / Purchasing / Goods Receipt

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/finance-management/purchasing/goods-receipts/{id}` | Rincian GR (baris item, batch, kedaluwarsa) untuk menghitung sisa dan tabel rincian riwayat | `FinanceGoodsReceipt : Read` |
| `POST` | `/v1/corporate/finance-management/purchasing/goods-receipts` | Mencatat GR; item membawa `batchNumber` dan `expiredDate` bila diisi | `FinanceGoodsReceipt : Create` |
| `POST` | `/v1/corporate/finance-management/purchasing/goods-receipts/{id}/cancel` | Membatalkan GR (perilaku lama, tidak diubah) | `FinanceGoodsReceipt : Cancel` |
| `GET` | `/v1/corporate/finance-management/purchasing/goods-receipts` | Daftar Penerimaan Barang (perilaku lama; kini baris dapat dibuka ke PO) | `FinanceGoodsReceipt : Read` |

#### Corporate / Finance Management / Purchasing / Purchase Order

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/finance-management/purchasing/purchase-orders/{id}` | Item PO dan ringkasan riwayat GR | `FinancePurchaseOrder : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` atas 28 berkas yang diubah/ditambah (gabungan `REV-22` frontend) | 0 error; 8 warning pola `useEffect` lama yang sama dengan hook yang tidak diubah | `PASS` / `EXISTING WARNING` | Rincian pada laporan `FE-FIN-052` bagian 6 |
| `node --import ./tests/helpers/register.mjs --test tests/unit/finance-purchasing-ap-blm.test.mjs` | 16 test, 16 lulus (8 milik task ini) | `PASS` | Keluaran perintah |
| `npm run test:unit` dan suite penuh | Lihat `FE-FIN-052` bagian 6: glob npm gagal pada Node 20/Windows; suite penuh 42 gagal pada 28 berkas yang tidak terkait | `EXISTING / ENVIRONMENT ISSUE`; `UNRELATED EXISTING ISSUE` | Sama |
| Grep anti-regresi pada JSX baru | Tidak ada `<button>`, `btn`, `<table>` mentah, `fw-*`/`fs-*`, atau inline style pada berkas baru. Pada baris yang ditambahkan ke berkas lama: label `fw-semibold` mengikuti label tetangga, `style={{ width: "140px" }}` adalah baris lama yang dipertahankan | `PASS` dengan pengecualian beralasan | Bagian 7 `FE-FIN-054` |
| `npm run build` | Tidak dijalankan | `NOT RUN` | Instruksi pengguna |
| Uji manual di browser (modal, tambah batch, sisa, rincian riwayat) | Tidak dijalankan | `NOT FEASIBLE` | Dev server/browser tidak dinyalakan tanpa diminta; kolom `BatchNumber`/`ExpiredDate` belum ada di database sampai migration diterapkan |

Uji manual: `NOT FEASIBLE`.

**Tidak dijalankan:** `npm run build`, `npm run dev`, Playwright.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Modal memuat Sisa, No. Batch, dan Kedaluwarsa per baris | Terpenuhi (source) | `purchase-order-detail-view.jsx` |
| 2. Satu baris PO dapat diterima dalam beberapa batch dan dikirim sebagai beberapa item | Terpenuhi (source) | `addBatchRowAfter`, `buildGoodsReceiptItemsPayload`; test "dua batch satu baris PO dikirim sebagai dua item…" |
| 3. Sisa = kuantitas PO − GR aktif; jumlah batch melebihi sisa dicegah di layar; sisa 0 menonaktifkan baris | Terpenuhi (source) | `sumReceivedByPurchaseOrderItem`, `validateGoodsReceiptRows`; test "sisa dihitung dari GR aktif saja…", "jumlah seluruh batch… tidak boleh melebihi sisa" |
| 4. Riwayat GR dapat dibuka dan batch serta kedaluwarsa terbaca | Terpenuhi (source) | `toggleGoodsReceiptDetail`, tabel rincian pada riwayat |
| 5. Riwayat gagal dimuat → peringatan, batas sementara kuantitas PO | Terpenuhi (source) | `openGoodsReceiptModal` (cabang `catch`); test "sisa tidak diketahui memakai kuantitas PO…" |
| 6. Daftar Penerimaan Barang dapat membuka PO induk | Terpenuhi (source) | `openPurchaseOrder` |
| DoD: lint PASS, laporan tracked | Terpenuhi untuk error lint (0) | Bagian 6 |
| DoD: build PASS, uji manual runtime | **Belum terpenuhi** | `NOT RUN` / `NOT FEASIBLE` |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Sisa dihitung dari rincian GR yang diambil satu per satu (`GET /goods-receipts/{id}`) — jumlah permintaan sebanyak GR aktif pada PO itu; wajar untuk PO biasa, bisa banyak pada PO dengan puluhan GR |
| Masalah yang diketahui | Tidak ada aturan menolak barang kedaluwarsa atau tanggal lampau (backend hanya mencatat; aturan belum diputuskan), sehingga layar tidak menambahkannya. UOM terima dan kondisi barang belum didukung kontrak backend (`AP-DEC-09`) |
| Dependency backend | `BE-FIN-125` 🟡: sampai migration diterapkan, simpan GR dengan batch dapat gagal di lingkungan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat `FE-FIN-054` bagian 8 |
| Langkah berikutnya | Terapkan migration, `npm run build`, lalu uji manual: PO 100 unit → GR batch A 60 + batch B 40 → buka Lihat Rincian → coba GR ketiga (sisa 0) |
