# Register Aset — Arsitektur Frontend

| Field | Value |
|---|---|
| Sub-modul | `register` — contract set `AST-REG-1.0.0` `draft` |
| Repository | `QuilvianSystemFrontendDev` (Next.js App Router, JavaScript/JSX, Redux, Axios) |
| Route dasar | `src/app/corporate/asset-management/` |
| Komponen dasar dipakai ulang | `src/components/features/base-features/` — `data-table`, `data-filter`, `pagination`, `status-badge`, `summary-grid`, `row-action-menu`, `confirm-modal`, `access-denied-gate`, `base-detail-view`, `base-editor-form`, `base-letterhead`, `resource-filter-select` (`TS-23`) |

Dokumen ini mengunci **keterjangkauan, isi, dan sumber data**. Rupa (warna, jarak, ikon, tab/modal/drawer)
adalah `DEV_DISCRETION` dalam design token Quilvian (`AST-FE-005`).

## 1. Kebutuhan layar

| ID | Layar | Menu | Endpoint utama |
|---|---|---|---|
| `SCR-AST-REG-01` | Dashboard Aset | 1 | `GET /register/dashboard/summary`, `/by-unit`; kartu 1.4 `pergerakan`, 1.5 `inventarisasi` |
| `SCR-AST-REG-02` | Kategori Aset | 2.1 | `/master-data/categories` |
| `SCR-AST-REG-03` | Kelompok Aset | 2.2 | `/master-data/groups` |
| `SCR-AST-REG-04` | Kode Inventaris | 2.3 | `GET /master-data/groups/inventory-codes` |
| `SCR-AST-REG-05` | Jenis Kepemilikan | 2.4 | `/master-data/ownership-types` |
| `SCR-AST-REG-06` | Lokasi | 2.5 | `/master-data/locations` |
| `SCR-AST-REG-07` | Klasifikasi Medis & Non-Medis | 2.6 | `/master-data/classifications` |
| `SCR-AST-REG-08` | Jenis Dokumen | tambahan | `/master-data/document-types` |
| `SCR-AST-REG-09` | Pengaturan Aset | tambahan | `/master-data/settings` |
| `SCR-AST-REG-10` | Pencatatan Aset Baru (draft saya + formulir) | 3.1 | `POST /register/assets/drafts`, `PUT /{id}`, `POST /{id}/submit` |
| `SCR-AST-REG-11` | Data Perolehan | 3.2 | `GET /register/assets` (kolom perolehan) + `/amounts` |
| `SCR-AST-REG-12` | Nomor Inventaris (persetujuan pencatatan) | 3.3 | `GET /register/assets?recordStatus=Diajukan`, `/approve`, `/approve-batch`, `/reject` |
| `SCR-AST-REG-13` | Label Aset | 3.4 | `POST /register/assets/labels` |
| `SCR-AST-REG-14` | Dokumen Kepemilikan | 3.5 | `/register/documents` |
| `SCR-AST-REG-15` | Daftar Inventaris | 3.6 | `GET /register/assets` |
| `SCR-AST-REG-16` | Impor Data Awal | tambahan | `/register/imports` |
| `SCR-AST-REG-17` | Detail Aset (layar anak) | — | `GET /register/assets/{id}`, `/amounts`, `/history`, dokumen |
| `SCR-AST-REG-18` | Penempatan Per Unit/Ruangan | 4.1 | `GET /register/placements` |
| `SCR-AST-REG-19` | Penanggung Jawab Aset | 4.2 | `GET /register/placements?custodianEmployeeId` |
| `SCR-AST-REG-20` | Berita Acara Penyerahan | 4.3 | `/register/handovers` |
| `SCR-AST-REG-21` | Riwayat Lokasi & Penggunaan | 4.4 | `GET /register/placements/history` |
| `SCR-AST-REG-22` | Pemeriksaan Kondisi | 6.1 | `/register/condition-reports` |
| `SCR-AST-REG-23..26` | Aset Aktif / Rusak Ringan-Berat / Tidak Digunakan / Hilang | 6.2–6.5 | `GET /register/assets?condition=…` (satu bentuk, saringan tetap) |
| `SCR-AST-REG-27` | Riwayat Perubahan Status | 6.6 | `GET /register/reports/history?eventType=KondisiBerubah` |
| `SCR-AST-REG-28` | Pengajuan Pemanfaatan | 8.1 | `/register/utilizations` |
| `SCR-AST-REG-29` | Kelengkapan Dokumen Kepemilikan | 8.2 | `GET /register/documents/completeness` |
| `SCR-AST-REG-30` | Pengamanan Administrasi | 8.3 | `GET /register/documents/expiring` |
| `SCR-AST-REG-31` | Pemantauan Aset Menganggur | 8.4 | `GET /register/assets/idle` |
| `SCR-AST-REG-32` | Pemanfaatan Sesuai Kewenangan (antrean persetujuan) | 8.5 | `GET /register/utilizations?status=Diajukan,MenungguDirektur` |
| `SCR-AST-REG-33` | Laporan Daftar Inventaris Per-unit | 11.1 | `/register/reports/inventory-by-unit` |
| `SCR-AST-REG-34` | Laporan Kondisi Aset | 11.3 | `/register/reports/condition` |
| `SCR-AST-REG-35` | History & Audit | 11.7 | `/register/reports/history` |

## 2. Peta butir menu

Peta seluruh modul ada di [`../02-module-map.md`](../02-module-map.md) bagian 3.1. Butir milik
sub-modul ini: Dashboard Aset, seluruh grup Master Klasifikasi Aset, Registrasi & Inventaris, Penempatan &
Tanggung Jawab, Kondisi & Status, Pemanfaatan & Pengamanan, dan tiga butir laporan (11.1, 11.3, 11.7).

Layar anak: **Detail Aset `SCR-AST-REG-17`** dibuka dari baris tabel pada `SCR-AST-REG-11`, `12`, `15`,
`18`, `19`, `23..26`, `31`, `33`, `34`, dan dari hasil pindai QR. Pendaftaran grup menu menjadi
acceptance criteria task layar dashboard (task frontend pertama sub-modul ini).

## 3. Skema fitur per layar

### 3.1 Bentuk daftar bersama (dipakai `SCR-AST-REG-02..09`, `11`, `14`, `15`, `18..27`, `29..35`)

```text
+- <Judul layar> ---------------------------------------------- SCR-AST-REG-xx -+
| [cari nomor / nama]   [Unit v] [Kelompok v] [Kondisi v] [Status v]            |
|                                                         [Ekspor] [+ Tambah]  |
+-------------------------------------------------------------------------------+
| No. Inventaris | Nama | Kelompok | Unit | Ruangan | Penanggung Jawab | Kondisi |
| -------------- | ---- | -------- | ---- | ------- | ---------------- | chip    | [⋮]
+-------------------------------------------------------------------------------+
| memuat -> kerangka baris                                                      |
| kosong -> "Belum ada aset pada saringan ini."              [Atur ulang]       |
| gagal  -> "Data gagal dimuat."                             [Coba lagi]        |
+- Halaman 1 dari n --------------------------- [< Sebelumnya] [Berikutnya >] --+
```

| Wilayah | Isi | Sumber data | Hak akses | Kosong / gagal |
|---|---|---|---|---|
| Saringan | Unit (hanya unit sendiri bila tanpa `ReadAllUnits`), kelompok, kondisi, status | Pilihan dari endpoint master `/options` | `: Read` resource terkait | Pilihan kosong → saringan disembunyikan |
| Tabel | Kolom sesuai layar; **tanpa rupiah** | Endpoint daftar layar | `: Read` | Kalimat kosong per layar, misalnya "Belum ada aset hilang." |
| Tambah | Formulir master / draft | — | `: Create` | Tombol disembunyikan bila tidak berhak |
| Ekspor | Excel | `/register/reports/{kode}/export` | `AssetReport : Read` | — |
| Aksi baris | Lihat detail, ubah, nonaktifkan | — | `: Read`/`: Update` | — |

### 3.2 Pencatatan Aset Baru — `SCR-AST-REG-10`

```text
+- Pencatatan Aset Baru ---------------------------------------- SCR-AST-REG-10 -+
| Draft saya  [3 draft]  [2 ditolak - perlu diperbaiki]                         |
+-------------------------------------------------------------------------------+
| Kelompok* [v]  Jenis kepemilikan* [v]  Jumlah unit* [ 1 ]                     |
| Nama aset* [            ]  Jenis perolehan* (Pembelian | Hibah masuk)         |
| Tgl perolehan* [  ]  Tgl siap pakai* [  ]  Harga perolehan per unit* [Rp   ]  |
| Tautkan GR [cari GR...]   atau   No. BA hibah [        ]                      |
| -- Atribut alat medis (muncul bila kelompok medis) --                         |
| Merek* Tipe* Nomor seri* Produsen* No. izin edar* Thn buat Distributor        |
|                                       [Simpan draft]  [Simpan & ajukan]       |
+-------------------------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Hak akses | Kosong / gagal |
|---|---|---|---|---|
| Daftar draft | Draft dan draft yang dikembalikan beserta alasan tolak | `GET /register/assets?recordStatus=Draft&createdByMe=true` | `AssetRegister : Read` | "Belum ada draft." |
| Formulir | Isian `AssetDraftCreateRequest`; atribut medis tampil bila klasifikasi kelompok medis | `/master-data/groups/options` (membawa `IsMedical`) | `AssetRegister : Create` / `Update` | Galat per isian dari `AST-VAL-REG-001..009` |
| Simpan & ajukan | Menyimpan lalu `POST /{id}/submit` | — | `AssetRegister : Submit` | Bila jumlah > 1 dan alat medis, tombol ajukan nonaktif sampai tiap draft punya nomor seri |
| Harga | Isian rupiah | — | `AssetRegister : Create` | Peran tanpa `ViewAmount` tidak melihat layar ini |

### 3.3 Nomor Inventaris (persetujuan) — `SCR-AST-REG-12`

Bentuk daftar bersama dengan kotak centang per baris dan tombol **[Setujui terpilih]** `AssetRegister : Approve`,
**[Tolak]** (wajib alasan). Hasil persetujuan massal ditampilkan per aset: "38 disetujui, 2 gagal" beserta
sebabnya. Baris yang diajukan pengguna sendiri tidak dapat dicentang.

### 3.4 Detail Aset — `SCR-AST-REG-17`

```text
+- AST-ALKMED-000123 - Monitor Pasien ---------------------------- SCR-AST-REG-17 -+
| Terdaftar  Aktif  IGD / Resusitasi  PJ: Kepala Ruang IGD   [proses: -]          |
| [Lapor kondisi] [Serahkan / pindah ruangan] [Cetak label] [Ajukan mutasi*]      |
+---------------+-----------------------------------------------------------------+
| - Ringkasan   | data perolehan, atribut medis                                   |
| - Nilai       | (hanya ViewAmount) harga saat pencatatan, nilai buku            |
| - Dokumen     | dokumen kepemilikan                                             |
| - Linimasa    | AstAssetHistory terbaru di atas                                 |
+---------------+-----------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Hak akses | Kosong / gagal |
|---|---|---|---|---|
| Kepala | Nomor, nama, status, kondisi, lokasi, penanggung jawab, proses aktif | `GET /register/assets/{id}` | `AssetRegister : Read` | Gagal → seluruh layar diganti pesan + coba lagi; 403 → "Aset ini bukan milik unit Anda." |
| Tombol | Lapor kondisi; serahkan; cetak label; tautan ke pengajuan mutasi/pinjaman/penghapusan milik sub-modul lain | — | `AssetCondition : Report`, `AssetPlacement : Create`, `AssetRegister : Read`, `AssetTransfer : Create`* | Tombol disembunyikan bila tidak berhak atau aset terkunci proses lain |
| Nilai | Rupiah | `GET /{id}/amounts` | `AssetRegister : ViewAmount` | Bagian tidak dirender untuk peran tanpa hak |
| Linimasa | Kejadian lintas sub-modul | `GET /{id}/history` | `AssetRegister : Read` | "Belum ada kejadian." |

\* Tautan saja; aksinya milik `pergerakan`/`penghapusan` (satu kemampuan, satu tempat).

### 3.5 Dashboard Aset — `SCR-AST-REG-01`

```text
+- Dashboard Aset ---------------------------------------------- SCR-AST-REG-01 -+
| [Total aset] [Terdaftar] [Belum ditempatkan] [Mutasi berjalan*] [Opname tertunda**] |
| Kondisi:  Aktif | Rusak Ringan | Rusak Berat | Tidak Digunakan | Hilang        |
| Aset per unit (tabel/grafik)          | Aset rusak terbaru (daftar)           |
+-------------------------------------------------------------------------------+
```

| Wilayah | Sumber data | Hak akses | Kosong / gagal |
|---|---|---|---|
| Kartu 1.1, 1.3, 1.6 | `GET /register/dashboard/summary` | `AssetDashboard : Read` | Kartu menampilkan "–" dan tombol coba lagi; kartu lain tetap tampil |
| Aset per unit 1.2 | `GET /register/dashboard/by-unit` | `AssetDashboard : Read` | "Belum ada aset ditempatkan." |
| \*Mutasi berjalan 1.4 | `GET /movement/dashboard/summary` (`pergerakan`) | `AssetTransfer : Read` | Kartu disembunyikan bila tidak berhak |
| \*\*Opname tertunda 1.5 | `GET /physical-inventory/dashboard/summary` (`inventarisasi`) | `AssetStockTake : Read` | Kartu disembunyikan bila tidak berhak |

Kepala Unit melihat angka unitnya sendiri. Grafik memakai pustaka yang sudah ada (`TS-24`); pilihannya
`DEV_DISCRETION`.

### 3.6 Penyerahan — `SCR-AST-REG-20`, Kondisi — `SCR-AST-REG-22`, Pemanfaatan — `SCR-AST-REG-28`/`32`, Impor — `SCR-AST-REG-16`

Keempatnya memakai bentuk daftar bersama ditambah panel/formulir dokumen dengan tombol langkah sesuai
`contracts/state-transition-matrix.md`:

| Layar | Tombol langkah | Hak akses | Catatan |
|---|---|---|---|
| `SCR-AST-REG-20` | Serahkan, Konfirmasi, Tolak terima, Batalkan, Cetak berita acara | `AssetPlacement : Create/Confirm/Cancel/Read` | Kepala Unit hanya melihat penyerahan ke unitnya, dengan penanda "menunggu konfirmasi Anda" |
| `SCR-AST-REG-22` | Lapor, Tetapkan, Tolak, Setujui Hilang, Cetak berita acara kehilangan | `AssetCondition : Report/Decide/ApproveLoss/Read` | Unggah foto memakai `UploadPhotoField.jsx` yang sudah ada (kamera perangkat) |
| `SCR-AST-REG-28`, `32` | Simpan, Ajukan, Setujui, Teruskan ke Direktur, Setujui (Direktur), Tolak, Akhiri | `AssetUtilization : Create/Submit/Approve/ApproveDirector/End` | Kolom nilai perjanjian hanya untuk peran yang punya `AssetUtilization : Read` |
| `SCR-AST-REG-16` | Unduh templat, Unggah, Setujui data aset, Setujui angka, Tolak, Batalkan | `AssetImport : Read/Upload/ApproveAsset/ApproveValue` | Tabel galat per baris dengan nomor baris Excel; ringkasan total untuk Kepala Akuntansi |

### 3.7 Label Aset — `SCR-AST-REG-13`

Pilih aset (saringan unit/kelompok/tanggal disetujui) → **[Cetak label]** → PDF dari backend dibuka di
tab baru. Ukuran label menunggu `AST-OQ-016`; sementara memakai ukuran stiker umum.

## 4. Aksi per peran

Diturunkan dari [`contracts/permission-audit-matrix.md`](contracts/permission-audit-matrix.md) bagian 2,
tidak dikarang ulang. Ringkasnya: Petugas Aset mencatat, menyerahkan, menetapkan kondisi, mengimpor;
Kepala Bagian Aset menyetujui; Kepala Unit dan penanggung jawab melapor kondisi dan mengonfirmasi
penerimaan; Kepala Akuntansi menyetujui angka impor; Direktur menyetujui pemanfaatan besar.

## 5. Data dan status yang dikonsumsi

Status ditampilkan dengan `status-badge.jsx` memakai label Bahasa Indonesia dari enum
(`02-backend-architecture.md` 5.4). Pemetaan nilai enum → label disimpan satu kali di
`src/utils/asset-management/asset-status-labels.js` (Baru), bukan diulang per layar.

## 6. Penanganan keadaan

| Keadaan | Perilaku |
|---|---|
| Memuat | Kerangka baris/kartu |
| Kosong | Kalimat per layar + tombol atur ulang saringan |
| Gagal | "Data gagal dimuat." + coba lagi; 403 dengan kalimat `AST-VAL-REG-016` |
| Data basi (`409` `AST-VAL-REG-015`) | Pesan "Data ini baru saja diubah orang lain" + muat ulang otomatis rincian |
| Kirim ganda | Tombol simpan/setujui nonaktif selama permintaan berjalan; persetujuan massal menampilkan hasil per aset |

## 7. Kewenangan UI

| Keputusan | Owner | Status |
|---|---|---|
| Letak grup menu di sidebar | Rizki | `open` (`AST-OQ-015`) |
| Nama dan urutan butir | Rizki | `approved` (`AST-FE-002`) |
| Rupiah tidak dikirim ke peran tanpa hak | Rizki | `approved` (`AST-FE-004`) |
| Tab/panel/modal, warna, grafik, tata letak dashboard | — | `DEV_DISCRETION` (`AST-FE-005`) |
| Ukuran label QR | Bagian Aset | `open` (`AST-OQ-016`) |
