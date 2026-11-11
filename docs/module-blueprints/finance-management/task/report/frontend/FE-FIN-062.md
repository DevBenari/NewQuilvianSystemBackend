# Laporan Perubahan Frontend — `FE-FIN-062`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-062` |
| Judul | Petugas melihat kemajuan penerimaan PO BLM dalam satuan dasar (dipesan, diterima sah, menunggu periksa, rusak, sisa) dan tanda bila konversi belum pasti; PO lama tampil seperti sekarang |
| Slice | `REV-24` (Penampung Data PO/GR dari BLM) — paket rilis `MVP-19` |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md`, bagian REV-24 Frontend, tabel Task |
| Trace | `FR-FIN-284`; `FIN-DEC-268`, `FIN-DEC-276`; `FIN-DES-139`; `03-frontend-architecture.md` §27.1–27.2 |
| Contract version | `FIN-API-1.13` §W.3 (`PurchasingTrailItemProgress`) — approved 9 November 2026 (Revisi 23 putaran 2) |
| Wewenang UI | Satu komponen yang sudah ada (panel jejak dokumen, bagian kemajuan item) mendapat kolom tambahan. Tidak ada layar, butir menu, tombol, atau hak akses baru. Warna, ikon, dan susunan visual `DEV_DISCRETION` (§27.2) |
| Dependency | `BE-FIN-147` ✅ — kontrak W.3 terkunci dan respons dijalankan pada data uji BLM milik pemilik (11 November 2026) |
| Klasifikasi | `LIGHT` — satu panel yang sudah ada, satu fungsi format murni, dua blok konstanta; tanpa route, Redux, atau panggilan API baru |
| Task mode | `FRONTEND MODE` |
| Target tulis | `QuilvianSystemFrontendDev` (branch `yasmina`); di repository backend hanya laporan ini dan tanda status roadmap |
| Model | Claude Sonnet 5.5 |
| Commit frontend saat dikerjakan | `ef8be65849c2e95869801f69cfafaa530ca74542` (perubahan task ini belum di-commit) |
| Commit backend yang dijadikan rujukan | `98a9140ca81799d747821a4ee3695846b46f96e3` (perubahan `BE-FIN-142`..`147` masih di working tree backend) |
| Tanggal | 10 November 2026 |
| Status | ✅ **Selesai dan terverifikasi di browser oleh pemilik pada data uji BLM (10–11 November 2026).** 14 unit test baru PASS, ESLint PASS, render statis PO lama identik dengan `HEAD`. Panel `UJI-BLM-W07` teramati lewat tangkapan layar pemilik (10 November); PO uji lain (`W00`, `W05`, `W06`, `W08`–`W12`) dan `UJI-LAMA-B` dinyatakan cocok oleh pemilik (pernyataan pemilik di server lokal, 11 November 2026, tanpa tangkapan layar). Data berasal dari skrip uji pemilik, bukan kiriman BLM. `npm run dev`/`build` oleh agen tidak dijalankan |

---

## 1. Keadaan yang Ditemukan di Awal

1. Panel jejak dokumen (`purchasing-document-trail-panel.jsx`) menampilkan tabel "Kemajuan Penerimaan per Baris" dengan lima kolom lama: Produk, Satuan, Dipesan, Sudah Diterima, Sisa. Seluruhnya dalam **satuan PO** dan diambil dari `orderedQuantity`, `receivedQuantity`, `remainingQuantity`.
2. Untuk PO bersumber BLM satu satuan PO bisa berbeda dari satuan dasar produk (mis. PO dalam BOX, stok dalam PCS), dan sebagian penerimaan bisa menunggu periksa atau rusak. Kolom lama tidak dapat menyampaikan itu.
3. `BE-FIN-147` sudah menambahkan field satuan dasar pada `itemProgress[]` (`sourceSystem`, `baseUomCode`, `orderedBaseQuantity`, `physicalBaseQuantity`, `acceptedBaseQuantity`, `pendingInspectionBaseQuantity`, `damagedBaseQuantity`, `remainingBaseQuantity`, `isConversionResolved`). Panel belum membacanya.
4. Pemakai panel ini: halaman detail PO dan halaman detail Tukar Faktur. Tabel kemajuan hanya tampil bila pemanggil memberi `showItemProgress`.

Temuan penting dari membaca penghitung backend (`PurchasingReceiptQuantityCalculator.CalculateItem`), karena memengaruhi tampilan:

| Keadaan di backend | Isi respons | Dampak pada tampilan |
| --- | --- | --- |
| Item PO sendiri tidak punya jumlah dipesan, isi per satuan, atau satuan dasar yang sah | `orderedBaseQuantity = null`, `remainingBaseQuantity = null`, `isConversionResolved = false` | Dipesan dan Sisa tampil "—" |
| Ada baris GR yang konversinya tidak pasti | Baris itu **tidak dijumlahkan**; kolom penerimaan berisi jumlah dari baris lain saja (bukan `null`), `isConversionResolved = false` | Angka itu bukan total penerimaan sebenarnya, jadi tidak boleh tampil seolah pasti. Kolom penerimaan ditampilkan "—" sesuai skema §27.2 |

---

## 2. Proses Bisnis dari Sisi Pengguna

Pengguna: petugas Finance/Purchasing yang membuka detail PO (atau detail Tukar Faktur) dan melihat kartu **Jejak Dokumen**.

1. Petugas membuka detail PO. Kartu Jejak Dokumen memuat rantai dokumen seperti biasa.
2. **PO biasa (QUILVIAN):** bagian "Kemajuan Penerimaan per Baris" tampil persis seperti sebelumnya — Produk, Satuan, Dipesan, Sudah Diterima, Sisa, dengan kalimat "Sisa belum diterima: … dari …".
3. **PO dari BLM:** bagian yang sama berganti menjadi enam kolom: Produk | Dipesan | Diterima sah | Menunggu periksa | Rusak | Sisa. Setiap angka memuat satuan dasar, mis. `12 PCS`.
4. **Bila konversi satuan sebuah item belum pasti:** di samping nama produk muncul lencana kuning "Konversi belum pasti", kolom Diterima sah/Menunggu periksa/Rusak/Sisa menampilkan "—", dan di bawah tabel muncul peringatan "Konversi satuan belum pasti. Penerimaan item ini belum dihitung."
5. **PO BLM yang belum punya GR:** tabel tetap tampil (Dipesan terisi, kolom lain `0 …`), dengan kalimat "Belum ada penerimaan barang untuk PO ini." di bawahnya.

Jalur tidak normal:

| Keadaan | Yang dilihat petugas |
| --- | --- |
| Akun tidak berhak membaca bagian Penerimaan Barang (`restrictedSections` memuat `GOODS_RECEIPTS`) | Kalimat "Belum ada penerimaan barang…" **tidak** ditampilkan, karena daftar GR yang kosong di sini bukan bukti tidak ada penerimaan; peringatan "Sebagian dokumen tidak ditampilkan…" yang sudah ada tetap muncul |
| Respons jejak gagal dimuat | Pesan galat dan tombol "Muat Ulang Jejak" yang sudah ada (tidak diubah) |
| Respons dari backend yang belum membawa `sourceSystem` | Diperlakukan sebagai PO lama; tampilan lama |
| Baris BLM dengan `isConversionResolved = null` | Tidak ditandai belum pasti; nilai `null` tampil "—" (tidak ditebak menjadi 0) |

---

## 3. Perubahan yang Dikerjakan

### 3.1 Berkas yang diperiksa

- Blueprint: `03-frontend-architecture.md` §27.1–27.3; `contracts/api-contract.md` §W.3; roadmap `02-frontend-roadmap.md` (kartu `FE-FIN-062`).
- Backend (baca-saja, bukti bentuk respons): `Dtos/FinancePurchasingDocumentTrailDtos.cs`, `Services/FinancePurchasingDocumentTrailService.cs`, `Services/PurchasingReceiptQuantityCalculator.cs`.
- Frontend: `purchasing-document-trail-panel.jsx`, `purchasing-trail-utils.jsx`, `purchasing-trail-constants.jsx`, `purchasing-display-utils.jsx`, `status-badge.jsx` + `status-badge.css`, `information-alert.jsx`, `globals.css` (kontrak `data-flat-table`), `purchase-order-table-columns.jsx` (pemakaian `StatusBadge`), `tests/unit/finance-purchasing-ap-flow.test.mjs`, `tests/helpers/alias-resolver.mjs`.
- Panduan: `build-module-frontend/references/` — gerbang keputusan base component, katalog, checklist konsistensi UI, kebijakan test.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/finance/purchasing/shared/purchasing-document-trail-panel.jsx` | Komponen privat `BlmItemProgressTable` (tabel enam kolom, lencana, peringatan, kalimat belum ada penerimaan). Blok lama diberi syarat `!isBlmProgress`; isi blok lama **tidak diubah** satu karakter pun |
| `src/utils/finance/purchasing/purchasing-trail-utils.jsx` | Fungsi murni baru: `isBlmItemProgress`, `hasBlmItemProgress`, `formatBaseQuantity`, `buildBlmItemProgressRows`, `hasUnresolvedBlmConversion`, `hasNoVisibleGoodsReceipts`. Tidak ada penjumlahan, pembagian, atau konversi |
| `src/lib/constants/finance/purchasing/purchasing-trail-constants.jsx` | `PURCHASING_SOURCE_SYSTEMS` dan `PURCHASING_BLM_ITEM_PROGRESS_TEXT` (salinan teks baku) |
| `tests/unit/finance-purchasing-blm-item-progress.test.mjs` | **Baru** — 14 unit test |

### 3.3 Kepatuhan arsitektur frontend

- Alur dependensi: komponen view → util murni → konstanta. Util tidak mengimpor React; teks baku ada di konstanta, bukan di JSX.
- Penempatan: util di `src/utils/finance/purchasing/`, konstanta di `src/lib/constants/finance/purchasing/`, test di `tests/unit/` — sesuai pola yang ada.
- Tidak ada pola baru, tidak ada stylesheet baru, tidak ada nilai visual literal, tidak ada pemanggilan API baru.

**UI GATE** (gerbang keputusan base component):

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Tabel kemajuan item BLM | `DataTable`; tabel Bootstrap yang sama dengan blok lama di panel | Panel tidak memakai `DataTable` (seluruhnya markup Bootstrap ringkas); `DataTable` membawa pagination dan urutan "terbaru di atas" sehingga mengubah tampilan PO lama (melanggar kriteria 3) | `WRAP` (di dalam panel, tabel inline khusus domain, `data-flat-table="true"`) | Opsi A (dipilih): tabel inline di panel. Opsi B: pindah ke `DataTable` — ditolak, mengubah PO lama dan menambah pagination pada ringkasan kecil. Opsi C: komponen tabel baru — tidak perlu |
| Lencana "Konversi belum pasti" | `StatusBadge` | Dipakai `purchase-order-table-columns.jsx` | `REUSE` (`status="warning"`, `variant="warning"`, `showIcon={false}`) | — |
| Peringatan konversi | `InformationAlert` | Dipakai `finance-cash-management-view.jsx` | `REUSE` (`variant="warning"`, `closable={false}`) | — |
| Kalimat kosong | Pola teks `small text-muted` yang sama dengan "Belum ada dokumen terkait." di panel | `purchasing-document-trail-panel.jsx` | Mengikuti pola setempat | — |

Tidak ada status `NEW` dan tidak ada `EXTEND` yang mengubah perilaku base component, jadi tidak ada keputusan yang menunggu pemilik.

---

## 4. State yang Ditangani di Layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Memuat jejak dokumen..." (yang sudah ada, tidak diubah) |
| Kosong | PO BLM tanpa GR: "Belum ada penerimaan barang untuk PO ini." (hanya bila bagian GR terbaca). Tanpa dokumen sama sekali: "Belum ada dokumen terkait." (yang sudah ada) |
| Gagal | Pesan galat jejak dokumen dan tombol "Muat Ulang Jejak" (yang sudah ada, tidak diubah) |
| Tanpa hak akses | Peringatan "Sebagian dokumen tidak ditampilkan karena akun Anda tidak memiliki hak baca: …" (yang sudah ada); kalimat belum ada penerimaan disembunyikan bila yang dibatasi adalah Penerimaan Barang |
| Konversi belum pasti | Lencana per item, "—" pada kolom penerimaan, dan peringatan di bawah tabel |

---

## 5. Endpoint yang Dikonsumsi

Tidak ada endpoint baru. Panel membaca `itemProgress[]` dari respons jejak dokumen yang sudah dikonsumsi.

#### Corporate / Finance Management / Purchasing / Purchase Order

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/finance-management/purchasing/purchase-orders/{id}/document-trail` (padanan untuk dokumen jangkar lain dibangun oleh `buildDocumentTrailEndpoint`) | Sumber `itemProgress[]`, termasuk field satuan dasar W.3 | `FinancePurchaseOrder : Read` (tidak berubah dari sebelum task ini) |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --no-warnings --import ./tests/helpers/register.mjs --test tests/unit/finance-purchasing-blm-item-progress.test.mjs tests/unit/finance-purchasing-ap-flow.test.mjs` | 30 test, 30 lulus (14 baru + 16 yang sudah ada, termasuk `summarizeItemProgress`) | `PASS` | Keluaran perintah |
| Semua test yang mengimpor berkas yang diubah (3 berkas) | 46 test, 46 lulus | `PASS` | Keluaran perintah |
| `npx eslint --max-warnings 0` pada 4 berkas yang diubah/ditambah | exit 0, tanpa temuan | `PASS` | Keluaran perintah |
| Seluruh `tests/unit/` | 4637 test, 4595 lulus, 42 gagal. Tidak satu pun yang gagal mengimpor atau membaca berkas yang diubah. Dibandingkan dengan salinan `HEAD` yang diekstrak ke scratchpad (repo backend dihubungkan lewat symlink): `HEAD` 4623 test, 45 gagal; tidak ada nama test gagal yang hanya muncul pada working tree. Tiga test yang gagal di salinan tetapi lulus di working tree (`M4 version.json`, `CSP origin API`, `next.config.js`) bergantung pada berkas akar yang tidak ikut disalin | `EXISTING / ENVIRONMENT ISSUE` (42 kegagalan sudah ada sebelum task ini; 0 baru) | Keluaran perintah; daftar nama dibandingkan dengan `diff` |
| Render statis panel dengan `react-dom/server` (pemuat sekali pakai di scratchpad, TypeScript untuk JSX; **bukan** bagian repo) — PO QUILVIAN, dengan dan tanpa `sourceSystem`, `showItemProgress` hidup/mati | Keluaran HTML **identik byte demi byte** dengan panel `HEAD` pada 4 kombinasi | `PASS` | `git show HEAD:…` dibandingkan dengan `cmp` |
| Render statis PO BLM: dua item (satu pasti, satu belum pasti) | Enam kolom; `12 PCS / 10 PCS / 2 PCS / 0 PCS / 2 PCS`; item kedua `24 PCS` lalu empat "—", lencana "Konversi belum pasti", peringatan tampil | `PASS` | Keluaran render |
| Render statis PO BLM tanpa GR; dan dengan `GOODS_RECEIPTS` dibatasi | Kalimat "Belum ada penerimaan barang…" tampil pada kasus pertama dan **tidak** tampil pada kasus kedua | `PASS` | Keluaran render |
| `npm run dev` / `npm run build` | Tidak dijalankan | `NOT RUN` | Instruksi pemilik: tanpa build dan dev server |
| Panel pada respons backend yang sebenarnya (W.3; data uji BLM milik pemilik: 9 PO BLM dan 2 PO lama) | `UJI-BLM-W07`: Dipesan 12 PCS, Diterima sah 10 PCS, Menunggu periksa 2 PCS, Rusak 0 PCS, Sisa 2 PCS (tangkapan layar, 10 November). PO uji lain, termasuk `W00` (kalimat "Belum ada penerimaan barang…"), `W10` (lencana "Konversi belum pasti", empat sel "—", peringatan), dan `UJI-LAMA-B` (lima kolom lama): cocok dengan masukan skrip uji menurut pemilik (11 November) | `PASS` (pengamatan pemilik) | Tangkapan layar `W07`; pernyataan pemilik untuk sisanya |

Uji manual oleh agen: `NOT FEASIBLE` — tidak ada dev server/browser pada sesi agen. Uji manual oleh pemilik: `PASS` — dilakukan di browser pada server lokal (lihat baris di atas).

AUTOMATED TEST: `node --no-warnings --import ./tests/helpers/register.mjs --test tests/unit/` — 14 test baru PASS; 42 kegagalan lama di modul lain tidak berubah.

**Grep anti-regresi** (checklist UI §G, pada baris yang ditambahkan):

| Pemeriksaan | Hasil |
| --- | --- |
| Warna literal, `font-size`/`font-weight`/`line-height`, `!important` di stylesheet baru | Tidak ada stylesheet baru |
| `<button`/`.btn` non-base | Kosong |
| `<table` tanpa `data-flat-table="true"` | Tabel baru membawa atributnya. Tabel lama (tidak disentuh) tidak memilikinya; dibiarkan agar tampilan PO lama tetap identik (kriteria 3) |
| Utility tipografi Bootstrap (`fw-*`, `fs-*`) | 1 temuan dipertahankan: judul bagian "Kemajuan Penerimaan per Baris" memakai kelas yang sama dengan judul di blok lama, supaya kedua cabang tampak sama. Letaknya di luar tabel |
| Hex/rgb/inline style baru | Kosong |

**Tidak dijalankan:** `npm run build`, `npm run dev`, e2e (`test:e2e`), UAT — di luar instruksi.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Bila `sourceSystem = "BLM"`: lima kolom satuan dasar sesuai skema §27.2 | Terpenuhi (source, unit test, render statis, dan browser pada data uji) | `BlmItemProgressTable`; test "baris BLM terselesaikan memuat lima kolom…"; render PO BLM |
| (2) Item `isConversionResolved = false` bertanda dan menampilkan "—" | Terpenuhi (source, unit test, render statis, dan browser pada `UJI-BLM-W10`) | Lencana + empat sel "—" + peringatan; tiga test konversi belum pasti. Jumlah parsial dari backend tidak ditampilkan |
| (3) Bila `QUILVIAN`: kolom dan nilai lama tidak berubah | Terpenuhi (render identik dengan `HEAD` dan browser pada `UJI-LAMA-B`) | Render HTML identik dengan `HEAD` pada 4 kombinasi; `summarizeItemProgress` tidak diubah dan test lamanya lulus |
| (4) Angka tidak dihitung ulang di klien, hanya diformat | Terpenuhi | Tidak ada operasi aritmetika pada angka satuan dasar; test "angka tidak dihitung ulang — sisa tetap nilai backend walau tidak sama dengan dipesan − diterima" |
| DoD: kriteria 1–4 terbukti | Terpenuhi | Terbukti pada source, unit test, render statis, dan browser pada respons backend sebenarnya dengan data uji (10–11 November 2026) |
| DoD: laporan `task/report/frontend/FE-FIN-062.md` | Terpenuhi | Berkas ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Pesan "Belum ada penerimaan barang…" dalam §27.2 hanya disebut sebagai keadaan kosong tabel tanpa syarat pemicu. Karena jejak PO tidak membawa `sourceSystem` di tingkat PO (hanya per baris `itemProgress`), pemicunya ditafsirkan: PO BLM, daftar GR kosong, dan bagian GR tidak dibatasi hak akses. **Dikonfirmasi pemilik** pada 10 November 2026 (jawaban "boleh" atas tafsiran ini); blueprint §27.2 tidak diubah |
| Masalah yang diketahui | (a) Kalimat ringkasan "Sisa belum diterima: X dari Y" pada blok lama **tidak** ditampilkan untuk cabang BLM, karena akan menjumlahkan angka satuan PO yang dibulatkan dan tidak ada di skema §27.2. (b) Komentar DTO backend menyebut besaran yang tidak pasti `null`, padahal penghitung mengisi jumlah parsial untuk kolom penerimaan; FE menyembunyikannya sesuai §27.2, tetapi sebaiknya kontrak W.3 menyatakan perilaku ini secara eksplisit. Tidak diubah di task ini |
| Dependency backend | `BE-FIN-147` ✅ — respons W.3 terbukti pada data uji; nama dan bentuk field cocok |
| Perubahan sampingan | `NONE` di repository. Berkas sementara hanya di scratchpad sesi (pemuat render dan salinan `HEAD`); tidak ada yang ditambahkan ke repo selain empat berkas di §3.2 |
| Interupsi | `NONE` |
| Status Git | FE: `M purchasing-document-trail-panel.jsx`, `M purchasing-trail-constants.jsx`, `M purchasing-trail-utils.jsx`, `?? tests/unit/finance-purchasing-blm-item-progress.test.mjs`. Tidak ada stage/commit |
| Langkah berikutnya | Tidak ada untuk task ini |
