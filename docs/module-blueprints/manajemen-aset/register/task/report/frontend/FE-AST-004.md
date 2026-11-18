# Laporan Perubahan Frontend — `FE-AST-004`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-AST-004` |
| Judul | Daftar inventaris, data perolehan, detail aset |
| Slice | `REG-FE-S1` — Fondasi dan pencatatan |
| Roadmap | `register/roadmap/frontend-roadmap.md` — kartu `FE-AST-004` |
| Trace | `CAP-03.2`, `03.6`; `AST-DEC-025`, `AST-DES-016`, `AST-FE-004`; `SCR-AST-REG-11`, `15`, `17`; `03-frontend-architecture.md` §3.1, §3.4 |
| Contract version | `AST-REG-1.0.0` `approved` 2026-11-12; layar mengikuti **source** backend `BE-AST-007` (`55610f4a`, `f70fd551`) ditambah tambalan daftar 2026-11-17 (bagian 1) |
| Wewenang UI | Rupa `DEV_DISCRETION` (`AST-FE-005`) dalam design token dan base component Quilvian |
| Dependency | `FE-AST-003` ✅, `BE-AST-007` ✅ |
| Klasifikasi | `HEAVY` — 3 layar (Daftar Inventaris, Data Perolehan, Detail Aset) + route pindai nomor, 12 berkas baru, 10 berkas diubah |
| Task mode | `FRONTEND`, diminta Rizki 2026-11-17; tambalan backend sempit atas izin Rizki pada hari yang sama |
| Target tulis | `QuilvianSystemFrontendDev` (`src/`); backend hanya tambalan daftar (bagian 1); laporan ini dan baris status roadmap/traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit frontend saat dikerjakan | `6db0342dc` (branch `RizkiV2`) |
| Commit backend yang dijadikan rujukan | `f70fd551` (branch `rizkiG`) + tambalan belum di-commit |
| Tanggal | 17 November 2026 |
| Status | ✅ SELESAI 17 November 2026 — keempat kriteria terpetakan ke source, uji layar 22 langkah lulus dengan tiga aktor, ESLint `0 errors, 0 warnings`, build pemilik (frontend dan backend) berhasil |

---

## 1. Keadaan yang ditemukan di awal

- `GET /register/assets` (`BE-AST-007`) sudah membawa unit, ruangan, penanggung jawab, kondisi, jenis dan tanggal
  perolehan, tetapi **tidak** membawa tanggal siap pakai, pemasok, nomor GR, dan nomor BA hibah. Layar Data
  Perolehan (`SCR-AST-REG-11`) butuh keempatnya. Atas izin Rizki, backend ditambal: `AssetListItemResponse` +
  `ReadyForUseDate`, `SupplierName`, `GoodsReceiptNumber`, `DonationReference` (+8 baris di `AssetRecordDtos.cs`
  dan `AssetRecordService.Query.cs`, nol komentar, kolom bukan rupiah sehingga `AST-DES-016` tetap terjaga).
  Dicatat sebagai delta no. 11 di laporan `BE-AST-007`.
- `GET /register/assets/by-number/{nomor}` belum ada (`BE-AST-008`). URL pindai label dilayani lewat pencarian
  daftar dengan nomor yang sama persis (bagian 2), lalu dialihkan ke detail.
- Pembuka rincian bertoken sebelumnya hanya ada di hook daftar draft; dipindah ke hook bersama
  `use-asset-record-route` supaya semua daftar register membuka detail dengan cara yang sama.

---

## 2. Proses bisnis dari sisi pengguna

| Langkah | Pelaku | Layar | Yang terjadi |
|---|---|---|---|
| 1 | Petugas / Kepala Unit | Daftar Inventaris | Melihat aset `Terdaftar` beserta unit, ruangan, penanggung jawab, kondisi. Saring per unit, ruangan, kelompok, kondisi, penempatan, status; cari nomor/nama. Tanpa `ReadAllUnits`, hanya aset unitnya yang tampil (dijaga backend) |
| 2 | Petugas Aset | Data Perolehan | Melihat asal aset: jenis (Pembelian / Hibah Masuk), tanggal perolehan, tanggal siap pakai, pemasok, nomor GR atau BA hibah. Saring kategori, kelompok, jenis perolehan, status |
| 3 | Siapa pun berhak baca | Detail Aset | Dibuka dengan klik dua kali baris atau menu **Lihat detail** dari Daftar Inventaris, Data Perolehan, Pencatatan Aset Baru, dan Nomor Inventaris |
| 4 | Siapa pun berhak baca | Detail Aset | Bagian **Ringkasan** (nomor, status, kondisi, penempatan, kelompok, atribut medis, perolehan, pengaju/penyetuju), **Linimasa** (kejadian terbaru di atas, berhalaman) |
| 5 | Pemegang `ViewAmount` | Detail Aset | Bagian **Nilai**: harga perolehan saat pencatatan, dikapitalisasi/disusutkan, biaya, residu, akumulasi penyusutan, nilai buku |
| 6 | Petugas lapangan | Pindai label | QR label membuka `/register/assets/nomor/<nomor>` → dialihkan ke detail aset; nomor tak dikenal → "Nomor inventaris tidak ditemukan, atau aset ini bukan milik unit Anda." |
| 7 | Pengguna unit lain | Detail Aset | 403 → layar Akses Ditolak dengan pesan "Aset ini bukan milik unit Anda." |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` frontend; `rules/frontend/frontend-architecture.md`; layar `FE-AST-003` (hook, kolom, service,
route token); base `base-detail-view`, `base-detail-section`, `base-detail-card`, `access-denied-gate`,
`data-table`, `data-filter`, `row-action-menu`, `status-badge`; `use-permission`, `permission-slice`;
`private-route-token-utils`; registry pilihan aset; kontrak 2.8, `03-frontend-architecture.md` §3.1, §3.4;
DTO dan service `BE-AST-007`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/corporate/asset-management/register/use-asset-record-route.jsx` | Baru — token rute bersama: `goDetail`, `goDraftUpdate`, `goBack` |
| `src/lib/hooks/corporate/asset-management/register/use-asset-inventory-list.jsx` | Baru — satu hook untuk dua varian daftar (inventaris, perolehan) |
| `src/lib/hooks/corporate/asset-management/register/use-asset-detail.jsx` | Baru — rincian, `/amounts` (hanya bila `ViewAmount`), `/history` berhalaman, ubah draft |
| `src/lib/hooks/corporate/asset-management/register/use-asset-number-lookup.jsx` | Baru — cari nomor persis lalu alihkan ke detail |
| `src/components/view/corporate/asset-management/register/asset-inventory-list-view.jsx` | Baru — `SCR-AST-REG-11` dan `15` (dibedakan oleh `variant`) |
| `src/components/view/corporate/asset-management/register/asset-detail-view.jsx` | Baru — `SCR-AST-REG-17` |
| `src/components/view/corporate/asset-management/register/asset-detail-rows.jsx` | Baru — baris Ringkasan dan Nilai |
| `src/components/view/corporate/asset-management/register/asset-number-lookup-view.jsx` | Baru — layar sementara saat mencari nomor pindai |
| `src/app/corporate/asset-management/register/assets/page.jsx`, `assets/[slug]/page.jsx`, `assets/nomor/[number]/page.jsx`, `acquisitions/page.jsx` | Baru — route tipis |
| `src/components/view/corporate/asset-management/register/asset-record-table-columns.jsx` | Diubah — kolom inventaris, perolehan, linimasa; aksi "Lihat detail" pada daftar draft dan antrean |
| `src/components/view/corporate/asset-management/register/asset-draft-list-view.jsx`, `asset-approval-list-view.jsx` | Diubah — klik dua kali / Lihat detail |
| `src/lib/hooks/corporate/asset-management/register/use-asset-draft-list.jsx`, `use-asset-approval-list.jsx` | Diubah — memakai `use-asset-record-route` |
| `src/lib/constants/corporate/asset-management/register/asset-record-constants.jsx` | Diubah — route, varian daftar (judul, saringan, default), ukuran halaman linimasa |
| `src/utils/corporate/asset-management/register/asset-record-utils.js` | Diubah — parameter daftar umum, `isAccessDeniedStatus`, `findExactInventoryNumber` |
| `src/lib/services/corporate/asset-management/asset-record-service.js` | Diubah — `fetchAssetHistory` |
| `src/lib/hooks/select/corporate/asset-management-select-resources.js` | Diubah — pilihan `assetLocations` |
| `src/utils/menu-sidebar/menu-items.jsx` | Diubah — butir "Data Perolehan" dan "Daftar Inventaris" (`AssetRegister : Read`) |

Backend (tambalan yang disetujui; rincian di laporan `BE-AST-007` delta no. 11):
`Register/DTOs/AssetRecordDtos.cs`, `Register/Services/AssetRecordService.Query.cs`.

### 3.3 Kepatuhan arsitektur frontend

- Alur URL → `app` → `view` → `hook` → `service` → `InstanceAxios`. View tidak memanggil Axios. Tidak ada slice
  atau perubahan store.
- Pola "respons berkunci" yang sama dengan `FE-AST-003`.
- Nol baris komentar, nol inline style, tidak ada CSS baru, tidak ada komponen base baru.

`UI GATE: 10 elemen — REUSE 9, EXTEND 0, COMPOSE 1, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Status |
|---|---|---|
| Kepala layar daftar dan layar pindai | `Hero`, `FilterSelect`, `BaseButton` | REUSE |
| Kepala halaman, ringkasan, tombol kembali/ubah | `BaseDetailView` (`heroActions`) | REUSE |
| Bagian Nilai, Linimasa | `BaseDetailSection`, `BaseDetailCard` | REUSE |
| 403 unit lain | `AccessDeniedGate` (`subtitle`) | REUSE |
| Daftar + saringan + paginasi | `DataTable`, `DataFilter`, `Pagination` | REUSE |
| Kondisi, penempatan, status | `StatusBadge` + `asset-status-labels.js` | REUSE |
| Aksi baris | `RowActionMenu` | REUSE |
| Saringan unit/ruangan/kelompok | pilihan remote registry | REUSE |
| Pesan nomor tidak ditemukan | `InformationAlert` | REUSE |
| Dua layar daftar dari satu view | `variant` atas konstanta `ASSET_LIST_VARIANTS` | COMPOSE |

---

## 4. State yang ditangani di layar

| Keadaan | Perilaku |
|---|---|
| Memuat | `DataTable` memuat; detail "Mengambil rincian aset..."; linimasa memuat sendiri; nomor pindai "Mencari aset dengan nomor ini..." |
| Kosong | "Belum ada aset pada saringan ini." / "Belum ada data perolehan pada saringan ini." / "Belum ada kejadian." |
| Gagal | Pesan galat + coba lagi dari `BaseDetailView` / daftar |
| 403 detail | `AccessDeniedGate` → "Aset ini bukan milik unit Anda." |
| Tanpa `ViewAmount` | Bagian Nilai tidak dirender |
| Token rute rusak | "Tautan aset tidak dapat dibuka. Buka ulang dari daftar inventaris." (UUID mentah tetap diterima) |
| Draft | Tombol **Ubah Draft** hanya untuk draft dan pemegang `Update` |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Asset Management / Register / Asset (`api/v1/corporate/asset-management/register/assets`)

| Layar | Endpoint |
|---|---|
| Daftar Inventaris | `GET /?recordStatus=&organizationUnitId=&locationId=&groupId=&condition=&placementState=&search=&sortBy=inventoryNumber` |
| Data Perolehan | `GET /?recordStatus=&categoryId=&groupId=&acquisitionType=&search=&sortBy=acquisitionDate&sortDirection=desc` |
| Detail Aset | `GET /{id}`, `GET /{id}/amounts` (hanya bila `ViewAmount`), `GET /{id}/history?pageNumber=&pageSize=10` |
| Pindai nomor | `GET /?search=<nomor>&pageSize=10` → cocok persis → detail |
| Pilihan | `organizationUnits`, `/master-data/locations/options`, `/master-data/categories/options`, `/master-data/groups` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint --max-warnings=0` atas 22 berkas | `0 errors, 0 warnings` | `PASS` | Dijalankan ulang 2026-11-17 sesudah build |
| Grep komentar pada diff dan berkas baru (FE dan BE) | Kosong | `PASS` | — |
| QBE Conformance (tambalan backend) | `PASS` | `PASS` | Sesi pengerjaan kode |
| `dotnet build` (tambalan backend) | Dijalankan Rizki dan berhasil. DLL 2026-11-17 09:29:11, sesudah suntingan 09:17:45. Respons daftar memuat keempat kolom perolehan | `PASS` | `bin/Debug/net9.0`; kolom terisi di layar Data Perolehan |
| `npm run build` | Dijalankan Rizki dan berhasil. `BUILD_ID` `2tQwhyy0sHcse4G6-rV7q` (2026-11-17 09:32:22), sesudah semua suntingan; manifest memuat `register/assets`, `assets/[slug]`, `assets/nomor/[number]`, `acquisitions` | `PASS` | `.next/BUILD_ID`, `app-path-routes-manifest.json` |
| Uji layar Playwright (`test-with-agy/asset-management/fe-ast-004-20261117/uji-inventaris.mjs`) | **22 langkah, semua sesuai harapan**; 0 galat halaman; 0 respons 5xx | `PASS` | `hasil-uji.jsonl`, 8 tangkapan layar |
| Uji API langsung cakupan unit (skrip scratchpad) | Tanpa `ReadAllUnits`: detail 403 `AST-VAL-REG-016`, `/history` 403 `016`, `/amounts` 403. Dengan `ReadAllUnits` tanpa `ViewAmount`: detail 200, `/history` 200, `/amounts` 403 | `PASS` | Keluaran konsol sesi |

Rincian uji layar (superadmin uji; penyetuju = akun "Uji Penyetuju Aset" dengan izin dicabut sementara; tanpa hak = LOKET):

| Langkah | Hasil |
|---|---|
| Daftar Inventaris | 200; 11 baris `Terdaftar`; kolom No, No. Inventaris, Nama Aset, Kelompok, Unit, Ruangan, Penanggung Jawab, Kondisi, Aksi; **tanpa rupiah**; menu Data Perolehan + Daftar Inventaris tampil |
| Saringan | Kondisi Aktif → `condition=Aktif`; kelompok Mebel → 7 baris, semuanya Mebel; atur ulang → parameter kembali ke default |
| Data Perolehan | 200; kolom Jenis, Tgl Perolehan, Tgl Siap Pakai, Pemasok, No. GR / BA Hibah; saring Hibah Masuk → 1 baris dengan `BA-HIBAH…` |
| **AC4** dari baris | Klik dua kali `AST-ALKMED-000001` → detail bertoken; bagian Ringkasan, Nilai Buku, Linimasa ("Didaftarkan"); tombol Ubah Draft tidak tampil (aset sudah terdaftar) |
| **AC4** dari URL pindai | `/assets/nomor/AST-ALKMED-000001` → dialihkan ke detail yang sama |
| Nomor tak dikenal | `/assets/nomor/AST-XXX-999999` → "Nomor inventaris tidak ditemukan…" |
| Lihat detail dari daftar draft | Menu baris memuat "Lihat detail" |
| **AC3** lewat pindai | Penyetuju tanpa `ReadAllUnits` → pesan gabungan "…tidak ditemukan, atau aset ini bukan milik unit Anda." (daftar tersaring unit, sehingga tidak dapat dibedakan; lihat bagian 8) |
| **AC3** lewat detail | `GET /{id}` 403 `AST-VAL-REG-016` → layar Akses Ditolak "Aset ini bukan milik unit Anda." |
| **AC2** | Penyetuju dengan `ReadAllUnits` tanpa `ViewAmount` → detail 200, ringkasan tampil, bagian Nilai dan "Harga Perolehan saat Pencatatan" **tidak dirender** |
| Pemulihan izin | Policy penyetuju dikembalikan ke Read/Approve/ViewAmount/ReadAllUnits (200) |
| LOKET | Layar Akses Ditolak; butir menu Daftar Inventaris tidak tampil |

Tiga putaran dijalankan. Perbedaan antarputaran semuanya berasal dari skrip, dan layar tidak diubah:

1. Putaran 1 mencatat status 200 untuk "detail lewat URL" meskipun layar menampilkan Akses Ditolak. Penyebabnya,
   pola tunggu `/register/assets/{id}` ikut cocok dengan dokumen halaman Next.js. Uji API langsung memastikan
   backend menjawab 403 (tidak ada kebocoran). Pola tunggu lalu dibatasi ke `/api/v1/`.
2. Putaran 2 mencatat pesan "nomor tidak ada" = 0 dan login LOKET tertahan di `/login`. Penyebabnya jeda tetap
   4 detik, padahal API pencarian terukur 0,4–2,2 detik ditambah pemuatan izin. Jeda diganti dengan menunggu
   kondisi.
3. Putaran 3 lulus seluruhnya.

Satu pengamatan dari putaran 1: `usePermission` sengaja mengembalikan `allowed = true` selama daftar izin belum
termuat (jaring pengaman bawaan Quilvian). Akibatnya, bila detail dibuka langsung tepat sesudah login, `/amounts`
dapat terkirim sekali, dijawab 403, lalu bagian Nilai tetap tidak dirender. Bunyi kriteria 2 ("dirender hanya
bila `/amounts` dapat dibaca") terpenuhi; tidak ada rupiah yang sampai ke layar.

`AUTOMATED TEST: SKIPPED (opsional) — kebijakan tanpa unit test`.

**Data yang ditulis ke `QuilvianNewDevRizki`:** tidak ada data aset. Hanya policy pasangan Logistik × Staff
Logistik yang diubah sementara lewat `POST /administrator/setting/role-access/policies`, lalu dipulihkan pada
setiap putaran.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Skema §3.1 dan §3.4 | Terpenuhi | Daftar bersama dengan saringan unit/ruangan/kelompok/kondisi/status, tabel tanpa rupiah, kalimat kosong per layar; detail dengan Ringkasan, Nilai, Linimasa. Delta: bagian Dokumen dan tombol Lapor kondisi / Serahkan / Cetak label / Ajukan mutasi tidak dibuat di sini, karena milik `FE-AST-008`, `007`, `006`, `005`, dan `pergerakan`; Ekspor milik `FE-AST-010` |
| 2. Bagian Nilai dirender hanya bila `/amounts` dapat dibaca | Terpenuhi | `showAmounts` = `ViewAmount` ∧ `/amounts` sukses; uji AC2 |
| 3. 403 menampilkan "Aset ini bukan milik unit Anda." | Terpenuhi | `AccessDeniedGate` meneruskan pesan backend `AST-VAL-REG-016`; uji AC3 |
| 4. Detail dapat dibuka dari baris tabel dan dari URL pindai | Terpenuhi | Klik dua kali / Lihat detail di 4 daftar; route `assets/nomor/[number]`; uji AC4 |
| `DoD-FE` | Terpenuhi | Lint, build pemilik, uji layar, laporan, roadmap, traceability |

Rekap: **4 dari 4 kriteria terpenuhi** → `✅`.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Masalah yang diketahui | (1) URL pindai memakai pencarian daftar sampai `BE-AST-008` menyediakan `/by-number`. Karena daftar tersaring unit, aset unit lain tampil sebagai pesan gabungan "tidak ditemukan, atau bukan milik unit Anda", bukan 403 tersendiri. Cukup ganti satu panggilan di `use-asset-number-lookup` saat `BE-AST-008` selesai. (2) `SCR-AST-REG-11` di peta layar menyebut `/amounts` untuk Data Perolehan. Daftar sengaja tanpa rupiah (§3.1, `AST-DES-016`), dan memanggil `/amounts` per baris berarti N permintaan, jadi harga ditampilkan di detail aset. (3) `AT-REG-12`/`46` (Kepala Unit dengan aset yang sudah ditempatkan) belum dapat diuji di layar sampai penempatan `BE-AST-009`/`FE-AST-006` |
| Risiko | Tidak ada data uji baru |
| Perubahan sampingan | Pembuka rute draft dipindah ke `use-asset-record-route`; perilaku Ubah Draft `FE-AST-003` tetap. Dua berkas lain yang berubah di working tree frontend (layar dokter IGD dan rawat inap) bukan milik task ini dan tidak di-commit |
| Status Git | Frontend `RizkiV2`: 10 berkas diubah, 12 berkas baru. Backend `rizkiG`: 2 berkas tambalan + dokumen |
| Langkah berikutnya | `BE-AST-008` (label, cari nomor) → `FE-AST-005`; atau `BE-AST-009` → `FE-AST-006` |
