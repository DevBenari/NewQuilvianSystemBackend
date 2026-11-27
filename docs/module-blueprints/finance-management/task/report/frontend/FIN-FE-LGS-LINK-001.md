# Laporan Perubahan Frontend — `FIN-FE-LGS-LINK-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FIN-FE-LGS-LINK-001` |
| Judul | Status penghubung Penerimaan Barang → Logistik pada daftar Penerimaan Barang, jejak dokumen, dan Monitoring Purchasing |
| Slice | Pekerjaan ad-hoc di luar penomoran roadmap (27 Nov 2026). Tidak ada baris roadmap, sehingga **tidak ada tanda status yang dipasang pada roadmap** |
| Trace | `FIN-DEC-268`, `LGS-CAP-07`, `LGS-OQ-017`, `LGS-OQ-018`, `LGS-OQ-015` (terbuka) |
| Contract version | Mengonsumsi kontrak aditif `FIN-BE-LGS-LINK-001` (`logisticsLink` nullable pada GR/jejak dokumen; `GET goods-receipts/logistics-summary`) |
| Dependency | `FIN-BE-LGS-LINK-001` (source selesai, belum dibuild). Aman terhadap backend lama: status tampil "Tidak dapat ditentukan" dan kartu tampil "—" |
| Task mode | `FRONTEND`. Repository backend hanya diberi laporan ini |
| Branch / SHA saat dikerjakan | `yasmina` @ `533b21dc6a6f6c630a79071fc75dbf2b825371eb` (working tree bersih sebelum task) |
| Model | Claude Sonnet 5.5 |
| Tanggal | 2026-11-27 |
| Status | 🟡 **SEBAGIAN** — source dan tes statis selesai; layar **belum dibuka**, `npm run build`/dev server tidak dijalankan (dilarang pada task ini) |

---

## 1. Masalah yang diperbaiki

Petugas Finance tidak dapat melihat apakah satu Penerimaan Barang sudah punya hubungan dengan persediaan Logistik. Akibatnya "GR tercatat" mudah terbaca sebagai "barang sudah masuk gudang", padahal GR Finance belum menambah stok. Monitoring Purchasing juga tidak punya indikator penghubung, dan tidak ada tempat untuk menyatakan bahwa sebagian kategori **tidak dapat ditentukan**.

## 2. Proses bisnis dari sisi pengguna

1. **Daftar Penerimaan Barang** (`/finance/purchasing/goods-receipts`) menambah kolom **Sumber** (BLM / Quilvian) dan **Penghubung Logistik** (badge). Arahkan kursor ke badge untuk melihat alasan dari backend.
2. **Jejak dokumen** (detail PO, dan panel jejak lain) menampilkan badge "Logistik: …" di bawah status setiap Penerimaan Barang.
3. **Monitoring Purchasing** (`/finance/purchasing/monitoring`) menambah bagian "Penghubung Penerimaan ke Logistik": empat kartu hitungan (Belum Terhubung, Menunggu Pemeriksaan, Perlu Rekonsiliasi, Terhubung ke Persediaan) dan tiga kartu **tanpa angka** (Menunggu Pemetaan Barang, Menunggu Konfirmasi Logistik, Gagal Sinkronisasi) yang selalu "—" beserta alasannya.
4. Status tidak dapat dibaca → badge "Tidak dapat ditentukan"; ringkasan gagal dimuat → seluruh kartu "—" dan peringatan kuning. **Tidak pernah 0 atau "terhubung" karena data tidak ada.**
5. Tidak ada tombol yang mengubah stok, GR, invoice, atau utang. Keterangan sumber data BLM ("Integrasi otomatis BLM ke Quilvian belum tersedia") **tidak diubah**.

| Kondisi | Tampilan |
| --- | --- |
| Backend lama / `logisticsLink` kosong | Badge abu-abu "Tidak dapat ditentukan" |
| Status tidak dikenal (mis. `SUCCESS`) | "Tidak dapat ditentukan" — tidak pernah dibaca sebagai terhubung |
| `POSTED_TO_STOCK` | Hijau "Terhubung ke Persediaan" — satu-satunya status hijau |
| `HAS_DISCREPANCY` | Merah "Perlu Rekonsiliasi" |
| `AWAITING_INSPECTION` | Kuning "Menunggu Pemeriksaan" |
| `NOT_LINKED` | Abu-abu "Belum Terhubung" |
| Ringkasan: backend mengirim `0` | Kartu "0" (angka nyata) |
| Ringkasan: field tidak dikirim / permintaan gagal | Kartu "—" |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`goods-receipt-view.jsx`, `goods-receipt-table-columns.jsx`, `use-goods-receipt-list.jsx`, `goods-receipt-constants.jsx`, `purchasing-monitoring-constants.jsx`, `use-purchasing-monitoring.jsx`, `purchasing-monitoring-view.jsx`, `purchasing-monitoring-utils.js`, `purchasing-document-trail-panel.jsx`, `purchasing-trail-utils.jsx`, `status-badge.jsx`, serta tes `purchasing-module-separation`, `finance-purchasing-gr-base-quantity-display`, `finance-purchasing-ap-blm`, `finance-purchasing-ap-flow`, `finance-purchasing-blm-item-progress`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/finance/purchasing/goods-receipt-logistics-constants.js` (baru) | Kode status (sama dengan backend), label, kunci status badge, label "Tidak dapat ditentukan", kode kategori tak tertentu, endpoint ringkasan, teks layar |
| `src/utils/finance/purchasing/goods-receipt-logistics-utils.js` (baru) | `describeGoodsReceiptLogisticsLink`, `formatGoodsReceiptSourceLabel`, `buildGoodsReceiptLogisticsCards` — fungsi murni; layar tidak menurunkan status sendiri |
| `.../goods-receipt/goods-receipt-table-columns.jsx` | Kolom "Sumber" dan "Penghubung Logistik" |
| `.../shared/purchasing-document-trail-panel.jsx` | `GoodsReceiptLogisticsBadge` (komponen lokal kecil) dipasang lewat `trailing` pada `TrailRow` GR |
| `src/lib/hooks/finance/purchasing/use-purchasing-monitoring.jsx` | Permintaan keempat `GET goods-receipts/logistics-summary` dalam `Promise.allSettled`; gagal → `null` + pesan |
| `.../monitoring/purchasing-monitoring-view.jsx` | Bagian "Penghubung Penerimaan ke Logistik" memakai `SummaryGrid` dan `InformationAlert` |
| `tests/unit/finance-purchasing-logistics-link.test.mjs` (baru) | 14 tes: util, kontrak FE↔BE (route, izin, kode status), larangan tulis/posting stok |

Tidak ada route, menu, atau halaman Logistik yang diubah; tidak ada route fiktif; tidak ada penelusuran ke dokumen Logistik karena dokumennya belum ada.

### 3.3 Aturan tampilan

- Badge memakai `StatusBadge` dengan kunci status bawaan (`active`/`inactive`/`warning`/`danger`), bukan `variant` Bootstrap. Label selalu berupa teks, sehingga status tidak hanya dibedakan oleh warna.
- Kartu kategori tak tertentu tidak pernah menampilkan angka, dan tidak menaut ke daftar terfilter karena daftar GR belum punya filter status penghubung.

### 3.4 Kepatuhan arsitektur frontend

Konstanta → `src/lib/constants/finance/purchasing/`; fungsi murni → `src/utils/finance/purchasing/`; hook membaca API; view hanya memanggil fungsi. Tanpa nilai visual literal dan tanpa CSS baru.

**UI GATE: 3 elemen — REUSE 2, EXTEND 0, COMPOSE 1, WRAP 0, NEW 0**

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Kolom "Sumber" dan "Penghubung Logistik" | `DataTable` + `StatusBadge` | Pola kolom badge sama dengan kolom Status pada berkas yang sama | REUSE | Tidak ada komponen baru |
| Kartu ringkasan penghubung | `SummaryGrid`, `InformationAlert` | Dipakai bagian PO/GR pada halaman yang sama | REUSE | Tidak ada komponen baru |
| Badge status pada baris jejak dokumen | `TrailRow` (`trailing`) + `StatusBadge` | `TrailRow` sudah menyediakan slot `trailing` | COMPOSE | Fungsi lokal 10 baris di berkas yang sama; tidak dijadikan komponen shared |

**Pemeriksa `check-css-usage.mjs`** (3 berkas JSX yang diubah): **5 temuan, seluruhnya kode lama yang tidak saya ubah** — `goods-receipt-table-columns.jsx:60` (badge Status GR memakai `variant`), `purchasing-document-trail-panel.jsx:76` (dua temuan pada `TrailRow` memakai `bg-${tone}`) dan `:123` (badge konversi memakai `variant`), `purchasing-monitoring-view.jsx:76` (kelas halaman `finance-purchasing-monitoring-page`). **Temuan baru dari perubahan ini: 0** (dua temuan awal dari badge Logistik sudah diperbaiki ke kunci `status`). Target "0 temuan" untuk berkas-berkas ini belum tercapai karena utang lama tersebut; tidak diperbaiki agar tampilan Purchasing yang sudah dipakai tidak berubah di luar lingkup.

---

## 4. State yang ditangani di layar

| State | Perilaku |
| --- | --- |
| Loading | Kartu memakai `loading` `SummaryGrid`; daftar memakai loading `DataTable` yang sudah ada |
| Ringkasan gagal / 403 | Peringatan kuning "Ringkasan penghubung Logistik gagal dimuat"; semua kartu "—"; bagian PO dan GR lain tidak terganggu (`allSettled`) |
| Backend lama | Kolom dan badge "Tidak dapat ditentukan"; kartu "—" |
| Tidak ada GR | Daftar kosong seperti sebelumnya |

## 5. Endpoint yang dikonsumsi

| Endpoint | Izin | Catatan |
| --- | --- | --- |
| `GET /v1/corporate/finance-management/purchasing/goods-receipts` | `FinanceGoodsReceipt:Read` | Respons kini memuat `logisticsLink` per baris (aditif) |
| `GET .../goods-receipts/logistics-summary` | `FinanceGoodsReceipt:Read` | Baru; hanya baca |
| `GET .../purchase-orders/{id}/document-trail` (dan trail GR) | izin yang sudah ada | Bagian GR memuat `logisticsLink` |

## 6. Verifikasi

| Pemeriksaan | Hasil |
| --- | --- |
| `node --import ./tests/helpers/register.mjs --test` (tes baru + 5 berkas Purchasing terkait, 6 berkas) | **89 lulus, 0 gagal, 0 dilewati**; 14 di antaranya tes baru (`AUTOMATED TEST: … — PASS`) |
| ESLint pada 7 berkas yang diubah/dibuat | exit 0, tanpa keluaran |
| `check-css-usage.mjs` | 5 temuan lama, 0 baru (lihat 3.4) |
| `npm run build`, `npm run dev`, dev server | **TIDAK DIJALANKAN** (dilarang pada task ini) |
| `VISUAL CHECK` | **NOT FEASIBLE** — dev server tidak boleh dijalankan; tidak ada screenshot. Tampilan ketiga layar belum pernah dilihat |
| `MANUAL TEST` | **NOT FEASIBLE** — alasan sama; interaksi badge/tooltip dan kartu belum diverifikasi di peramban |
| Catatan tes statis | Tes yang membaca source BE hanya membuktikan teks source, bukan perilaku runtime |

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status |
| --- | --- |
| Kolom penghubung pada daftar GR; status tak diketahui bukan 0/berhasil | Source + tes statis ✅; tampilan **belum dilihat** |
| Badge pada jejak dokumen GR | Source + tes statis ✅; tampilan belum dilihat |
| Monitoring: kartu penghubung + kategori "tidak dapat ditentukan" | Source + tes statis ✅; tampilan belum dilihat |
| Keterangan "integrasi BLM belum tersedia" tidak diubah menjadi aktif | ✅ (dikunci tes) |
| Tanpa route/menu baru, tanpa perubahan Logistik | ✅ |
| `VISUAL CHECK` + `MANUAL TEST` | ⛔ NOT FEASIBLE → status paling tinggi 🟡 |

## 8. Catatan penutup

- Penelusuran AP → Purchasing Invoice → Tukar Faktur → GR → PO di `/finance/payable/invoice` **tidak diubah**; jejak dokumen yang sudah ada tetap jalur penelusuran, dan kini membawa badge Logistik pada baris GR.
- Kolom pemasok pada daftar GR tidak ditambahkan karena respons GR hanya memuat id PO; resolver nomor PO yang sudah ada tetap dipakai.
- Sebelum dipakai: build FE + BE bersamaan, lalu buka tiga layar di atas dengan data uji (GR lokal, GR BLM dengan baris `PENDING_INSPECTION` dan rusak) untuk memastikan badge dan kartu tampil sesuai tabel di bagian 2.
