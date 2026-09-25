# Laporan Perubahan Frontend — `FE-BD-012`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BD-012` |
| Judul | Penyimpanan dan perpindahan lokasi kantong |
| Slice | Slice 3 — kantong darah, pemberian, dan penyelesaiannya |
| Roadmap | [roadmap/frontend-roadmap.md](../../../roadmap/frontend-roadmap.md), kartu `FE-BD-012` |
| Trace | `DEC-BD-036`, `DEC-BD-037`; `INV-BD-026`, `INV-BD-027`, `INV-BD-028`; `VAL-BD-060`, `VAL-BD-061`, `VAL-BD-062`; kewajiban layar `FE-BD-010` dan `FE-BD-011` (`03-frontend-architecture.md` §`FE-BD-04`/`FE-BD-05`); keputusan pemilik `D1`–`D4` (bagian 1.1) |
| Contract version | api-contract `v5` `approved`, termasuk Amendment `D6` (`inactiveLocation`, `BE-BD-020`). Kartu roadmap masih menulis `v4` — lihat bagian 8 |
| Wewenang UI | Layar `FE-BD-04` (dua preset saringan) dan `FE-BD-05` sebatas penetapan lokasi, perpindahan lokasi, dan riwayat penempatan. Rupa layar `DEV_DISCRETION` mengikuti `FE-BD-004`. **Tidak** mencakup bukti kecocokan dan pemberian (`FE-BD-005`), penyelesaian `PendingReview` (`FE-BD-007`), maupun koreksi (`FE-BD-008`) |
| Dependency | `BE-BD-015` ✅, `BE-BD-020` ✅ (commit backend `8bc7b512`), `BE-BD-014` ✅, `BE-BD-016` ✅, `FE-BD-004` ✅ — nol penahan |
| Klasifikasi | `MEDIUM` — skor 7: satu repository tulis (0), 9–20 berkas diperiksa (1), 11 berkas diubah termasuk test (2), logika sedang (1), memakai kontrak yang ada (1), database tidak disentuh langsung (0), keamanan berkaitan — gerbang dua hak akses (1), satu workflow terbatas (1) |
| Task mode | `FRONTEND` — backend strict read-only; wewenang lintas repository hanya untuk laporan ini dan register roadmap |
| Target tulis | `V2QuilvianSystemFrontendDev` branch `sukmagpV2` (upstream `origin/sukmagpV2`) |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `99cabdbe8` — working tree bersih sebelum task |
| Commit backend yang dijadikan rujukan | `8bc7b512` (`BE-BD-020`) |
| Tanggal | 24 September 2026 |
| Status | ✅ **Selesai** — `lint:errors` `PASS`, `test:unit` `PASS` untuk cakupan task (7 test baru lulus, 7 kegagalan lama tetap sama), `build` `PASS`, dan pada **validasi ulang 24 September 2026 (bagian 6.2) 13 dari 13 skenario runtime di browser sungguhan PASS terhadap backend sungguhan** (A9), ditambah regresi `FE-BD-004` **16 dari 16 PASS**. **Riwayat:** validasi pertama 9 dari 9 (bagian 6.1) |

---

## 1. Keadaan yang ditemukan di awal

`FE-BD-004` sudah membangun layar kantong minimal — daftar, detail, alokasi, pembatalan alokasi,
kolom **Lokasi Penyimpanan**, dan penanda **Lokasi nonaktif** (kewajiban `FE-BD-011`). Yang belum ada:

| Kebutuhan | Keadaan sebelum task |
| --- | --- |
| Saringan kantong yang belum disimpan | Hanya bisa lewat dropdown Status → "Diterima"; tidak ada preset |
| Saringan kantong tertahan di lokasi nonaktif | **Tidak ada.** Backend baru menerimanya sejak `BE-BD-020` |
| Menetapkan lokasi pertama kantong `Received` | **Tidak ada.** Kantong baru dari PMI tidak pernah bisa disimpan dari layar, sehingga tidak pernah bisa dialokasikan (`VAL-BD-063`) |
| Memindahkan kantong keluar dari kulkas nonaktif | **Tidak ada.** Peringatan `VAL-BD-068` menyebut jumlah kantong tertahan, tetapi petugas tidak punya jalan keluar dari layar (`DEC-BD-037`) |
| Riwayat penempatan | **Tidak ada** |

Temuan audit backend yang membentuk keputusan:

| Temuan | Bukti |
| --- | --- |
| `BloodUnitPlacementDto` hanya membawa `PlacedByUserId` (GUID) tanpa nama | `DTOs/BloodUnitStorageDtos.cs`; spesifikasi layar meminta "oleh siapa" |
| Kelayakan penetapan/perpindahan sudah dikirim backend | `AvailableActions` memuat `AssignStorageLocation` (kantong `Received` tanpa lokasi) dan `MoveStorageLocation` (punya lokasi, masih di stok) |
| Pilihan lokasi aktif hanya tersedia sebagai thunk Redux milik layar master data | `master-data-blood-storage-locations-slice.jsx` |
| Backend **mengizinkan** pindah ke lokasi yang sama | `MoveStorageLocationAsync` tidak membandingkan tujuan dengan lokasi saat ini; satu baris riwayat baru tercipta |

### 1.1 Keputusan pemilik, 24 September 2026

| Kode | Keputusan |
| --- | --- |
| `D1` | Riwayat penempatan **tanpa kolom pelaku**. Celah `placedByName` dicatat di laporan ini (bagian 8) |
| `D2` | Dua **preset eksklusif**: **Belum Disimpan** (`unitStatus=0`) dan **Lokasi Nonaktif** (`inactiveLocation=true`) |
| `D3` | `getBloodStorageLocationOptions()` di `blood-unit.service.js`, **tanpa Redux** |
| `D4` | Acceptance `A1`–`A9` (bagian 7) |

---

## 2. Proses bisnis dari sisi pengguna

### 2.1 Menyimpan kantong yang baru diterima

1. Petugas BDRS membuka **Kantong Darah** lalu menekan **Belum Disimpan**. Daftar menampilkan kantong
   berstatus Diterima — misalnya 7 kantong pada data pengembangan.
2. Petugas membuka satu kantong. Tombol **Tetapkan Lokasi** muncul karena backend menyatakan kantong itu
   layak ditetapkan lokasinya **dan** petugas memegang `BloodUnit : Store` serta
   `BloodStorageLocation : Read`.
3. Dialog menampilkan pilihan **lokasi aktif saja**, diambil dari backend. Petugas memilih, misalnya
   `TBD006-LOC1 — Kulkas Satu`, mengisi keterangan bila perlu, lalu menekan **Tetapkan Lokasi**.
4. Backend menyimpan penempatan dan menentukan status akhir. Pada uji runtime, kantong
   `TEST-BD006-20260914094559-01` bergerak **Diterima → Tersimpan → Tersedia**; kantong berlebih atau
   yang permintaan asalnya sudah ditutup akan berakhir **Menunggu keputusan** — layar hanya membaca.
5. Tombol berganti menjadi **Pindahkan Lokasi**, dan **Riwayat Penempatan** menampilkan satu baris
   "Penempatan pertama" bertanda **Berlaku**.

### 2.2 Mengeluarkan kantong dari kulkas yang dinonaktifkan

1. Pengelola menonaktifkan sebuah kulkas; backend memperingatkan berapa kantong tertahan.
2. Petugas menekan **Lokasi Nonaktif** pada daftar kantong dan melihat kantong-kantong itu, masing-masing
   bertanda "Lokasi nonaktif".
3. Petugas membuka satu kantong. Tombol **Pindahkan Lokasi** **tetap muncul** walau lokasinya nonaktif —
   inilah jalan keluarnya. Dialog menampilkan lokasi saat ini dengan peringatan "lokasi ini nonaktif".
4. Petugas memilih lokasi tujuan yang aktif lalu menekan **Pindahkan**. Status kantong **tidak berubah**;
   riwayat lama tetap tersimpan dan baris lama kini bertanda "Lokasi nonaktif".
5. Kantong itu keluar dari saringan **Lokasi Nonaktif**, dan gerbang alokasinya terbuka kembali.

### 2.3 Jalur tidak normal

| Keadaan | Yang terjadi di layar |
| --- | --- |
| Kantong diubah petugas lain saat dialog terbuka | Backend menolak `409`; dialog ditutup, pesan backend tampil, detail dimuat ulang dan menampilkan lokasi terbaru |
| Lokasi tujuan dinonaktifkan saat dialog terbuka | Backend menolak `422` dengan kalimat `VAL-BD-060` persis, tampil di dalam dialog; detail dimuat ulang |
| Belum ada lokasi aktif sama sekali | Dialog menampilkan "Belum ada lokasi penyimpanan darah yang aktif." dan tombol konfirmasi tertahan |
| Tanpa `BloodUnit : Store` atau tanpa `BloodStorageLocation : Read` | Tombol Tetapkan/Pindahkan **disembunyikan**, bukan ditampilkan lalu ditolak `403` |
| Pindah ke lokasi yang sama | Tidak dicegah layar — opsinya hanya diberi keterangan "(lokasi saat ini)". Penilaiannya wewenang backend (bagian 8) |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Blueprint: kartu `FE-BD-012`, `03-frontend-architecture.md` §`FE-BD-04`/`FE-BD-05`, `api-contract.md` grup Blood Unit dan Blood Storage Location, laporan `FE-BD-004`, `FE-BD-002` §8.1, `BE-BD-020`
- Backend (read-only): `BbkBloodUnitController.cs`, `BbkBloodUnitService.cs` (`AssignStorageLocationAsync`, `MoveStorageLocationAsync`, `CheckDestinationAsync`, `GetPlacementsAsync`, `AvailableActionsFor`), `BloodUnitStorageDtos.cs`, `BloodUnitDtos.cs`, `BloodStorageLocationController.cs` (`GET /options`)
- Frontend: seluruh berkas `FE-BD-004`, `data-filter.jsx`, `base-text-field.jsx`, `base-detail-section.jsx`, `blood-order-form-view.jsx`, `blood-order.module.css`, slice master data lokasi, `InstanceAxios`
- Governance: `AGENTS.md` frontend, `rules/frontend/frontend-architecture.md`, `design-tokens.md`, `ui-consistency-checklist.md`, `REPORT_TEMPLATE.md`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/blood-bank-management/blood-unit-constants.jsx` | Preset `NOT_STORED` dan `INACTIVE_LOCATION`, `inactiveLocation` pada filter bawaan, dua nama aksi baru, endpoint opsi lokasi, batas keterangan 500, salinan teks |
| `src/lib/services/health-services/blood-bank-management/blood-unit.service.js` | `getBloodUnitPlacements`, `getBloodStorageLocationOptions` (`D3`), `assignBloodUnitStorageLocation`, `moveBloodUnitStorageLocation`. Isi permintaan **tepat** `storageLocationId`, `note`, `version` |
| `src/utils/health-services/blood-bank-management/blood-unit-utils.js` | Fungsi murni `buildStorageLocationOptions`, `normalizePlacements`, `buildStorageResult` |
| `src/lib/hooks/health-services/blood-bank-management/use-blood-unit-storage.jsx` | **Baru.** Gerbang dua hak akses + `AvailableActions`, dialog tetapkan/pindahkan, penanganan `409`/`422`, pemuatan riwayat penempatan |
| `src/lib/hooks/health-services/blood-bank-management/use-blood-unit-detail.jsx` | Merangkai `useBloodUnitStorage`; Muat ulang ikut membersihkan galat penyimpanan |
| `src/lib/hooks/health-services/blood-bank-management/use-blood-unit-list.jsx` | Filter `inactiveLocation`, preset eksklusif `showNotStored`, `toggleInactiveLocation` |
| `src/components/view/health-services/blood-bank-management/blood-units/blood-unit-list-view.jsx` | Tombol preset **Belum Disimpan** dan **Lokasi Nonaktif** |
| `src/components/view/health-services/blood-bank-management/blood-units/detail/blood-unit-detail-view.jsx` | Tombol **Tetapkan Lokasi** dan **Pindahkan Lokasi**, dialog penyimpanan, bagian **Riwayat Penempatan** |
| `src/style/health-services/blood-bank-management/blood-units/blood-unit.module.css` | Style `textarea` keterangan dan penghitung karakter, seluruhnya token |
| `tests/unit/blood-unit-storage.test.mjs` | **Baru.** Tujuh test penjaga `A5`/`A8` |
| `tests/e2e/blood-unit-storage-screen.spec.mjs` | **Baru.** Sembilan skenario runtime terhadap backend sungguhan. Alamat backend dan cookie sesi dibaca dari environment, tidak ditulis di source; tanpa keduanya spec dilewati |

**Nol route baru, nol butir menu baru, nol Redux slice baru, nol dependency ditambahkan, nol berkas backend
tersentuh.**

### 3.3 Kepatuhan arsitektur frontend

Alur `src/app` → `components/view` → `lib/hooks` → `lib/services` → `InstanceAxios` dipertahankan.
Hook penyimpanan dipisah dari hook detail (531 baris) supaya logika tidak menumpuk dalam satu berkas,
lalu dirangkai oleh hook detail — view tetap memakai satu hook.

**Tidak ada aturan bisnis di layar.** Kelayakan tombol dari `AvailableActions`; status sesudah
penyimpanan dari detail yang dipulangkan backend; keaktifan lokasi tujuan dari `GET /options`; lokasi
saat ini tidak dibuang dari pilihan. Satu-satunya batas di layar adalah `maxLength` 500 pada isian
keterangan, cermin `[MaxLength(500)]` request backend.

**Gerbang keputusan base component.**

`UI GATE: 9 elemen — REUSE 7, EXTEND 0, COMPOSE 2, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Dua tombol preset | `BaseButton` lewat prop `actions` (array) `DataFilter` | preset Menunggu Keputusan `FE-BD-004` | REUSE | Array ber-key supaya tiap tombol mendapat slot `filterActionItem` sendiri |
| Tombol Tetapkan / Pindahkan | `BaseButton` | `FE-BD-004` | REUSE | — |
| Riwayat penempatan | `BaseDetailSection` + `DataTable` | Riwayat Alokasi `FE-BD-004` | REUSE | `sortLatestFirst={false}` — urutan backend |
| Penanda Berlaku / Lokasi nonaktif | `StatusBadge` | `FE-BD-004` | REUSE | — |
| Pesan galat dan hasil | `InformationAlert`, `ToastStack` | `FE-BD-004` | REUSE | — |
| Pemilih lokasi | `FilterSelect` | dialog alokasi `FE-BD-004` | REUSE | — |
| Tanpa hak akses | tombol disembunyikan; `AccessDeniedGate` | `FE-BD-004` | REUSE | — |
| Dialog tetapkan / pindahkan | `ConfirmModal` + `FilterSelect` + `InformationAlert` | pola dialog `FE-BD-004`/`FE-BD-010` | COMPOSE | Satu dialog, dua mode |
| Isian keterangan | `<textarea>` di dalam `.formField` | dialog alasan order ganda `FE-BD-002` | COMPOSE | Lihat pilihan di bawah |

**Pilihan untuk isian keterangan** (bukan `REUSE`):

1. **`<textarea>` bergaya token di `.formField` — direkomendasikan.** Sama persis dengan pola modul
   yang sama (`FE-BD-002`). Konsistensi visual terjaga lewat token; nol risiko regresi pada base
   component; biaya satu blok CSS kecil.
2. `BaseTextField multiline`. Base component yang tepat secara nama, tetapi **wajib** berada di dalam
   `FormProvider` react-hook-form (melempar galat bila tidak). Membungkus satu isian opsional dalam form
   provider menambah state form paralel dengan state hook — biaya dan risiko lebih besar tanpa manfaat.

**Grep anti-regresi** (checklist, pada baris yang ditambahkan):

| No | Pemeriksaan | Hasil |
| ---: | --- | --- |
| 1 | Warna literal di CSS | Bersih |
| 2 | Typography di CSS | 3 temuan dipertahankan — `font-size`/`line-height` pada `textarea` dan penghitung karakter di dialog domain, **seluruhnya token** (`--font-size-control`, `--line-height-body`, `--font-size-small`); bukan override komponen shared |
| 3 | Tombol non-base | Bersih |
| 4 | `<table>` mentah | Bersih |
| 5 | Utility typography Bootstrap | Bersih |
| 6 | `!important` | Bersih |
| — | Inline style | Bersih |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | "Menyiapkan..." pada tombol Tetapkan/Pindahkan saat opsi dimuat; "Memuat lokasi..." pada pemilih; "Memuat riwayat penempatan..." pada tabel riwayat |
| Kosong | Riwayat: "Belum pernah disimpan" — "Kantong belum pernah disimpan."; opsi: "Belum ada lokasi penyimpanan darah yang aktif."; daftar tersaring kosong memakai "Belum ada kantong" milik `FE-BD-004` |
| Gagal | Pesan backend apa adanya; cadangan "Pilihan lokasi penyimpanan gagal dimuat.", "Riwayat penempatan kantong gagal dimuat.", "Lokasi penyimpanan kantong gagal ditetapkan.", "Kantong gagal dipindahkan."; pemulihan lewat Muat ulang |
| Tanpa hak akses | Tombol Tetapkan/Pindahkan disembunyikan tanpa `BloodUnit : Store` **atau** tanpa `BloodStorageLocation : Read`. Tanpa `BloodUnit : Read` tetap `AccessDeniedGate` milik `FE-BD-004` |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Blood Bank Management / Blood Unit

Base path: `/v1/health-services/blood-bank-management/blood-units`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/?unitStatus=0` | Preset **Belum Disimpan** | `BloodUnit : Read` |
| `GET` | `/?inactiveLocation=true` | Preset **Lokasi Nonaktif** (`D6`) | `BloodUnit : Read` |
| `GET` | `/{id}` | `AvailableActions`, `Version`, lokasi saat ini | `BloodUnit : Read` |
| `GET` | `/{id}/placements` | Riwayat Penempatan | `BloodUnit : Read` |
| `POST` | `/{id}/storage-location` | Tetapkan lokasi — isi `{ storageLocationId, note, version }` | `BloodUnit : Store` |
| `PUT` | `/{id}/storage-location` | Pindahkan lokasi — isi sama | `BloodUnit : Store` |

#### Health Services / Master Data / Blood Storage Location

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/master-data/blood-storage-locations/options` | Pilihan lokasi **aktif** | `BloodStorageLocation : Read` |

**Delta kontrak, bukan penahan.** Dokumen menulis `status=Received`; source backend menerima
`unitStatus=0`. Layar mengikuti source (sama dengan `FE-BD-004`).

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Kode keluar `0`, keluaran kosong | `PASS` | Log perintah. Spec e2e yang ditulis sesudahnya di-lint terpisah: `npx eslint --quiet tests/e2e/blood-unit-storage-screen.spec.mjs` kode keluar `0` |
| `npm run test:unit` | 1583 test — **1576 lulus, 7 gagal**. Ketujuh test baru lulus | `PASS` untuk cakupan task | Ketujuh kegagalan identik dengan baseline `FE-BD-004` (1576 test, 7 gagal): `accounting-reconciliation` 1, `inpatient-physician-entry` 4, `inpatient-physician-workspace` 1, `menu-permission-filter` M0 1 — nol di Bank Darah Kantong |
| `npm run build` | Kode keluar `0`, 370 halaman, `postbuild` standalone berhasil, `Compiled successfully in 48s` | `PASS` | `/blood-units` dan `/blood-units/[slug]` terdaftar |
| `npx playwright test tests/e2e/blood-unit-storage-screen.spec.mjs --workers=1` | **9 passed (36,3 detik)** dalam satu run dari keadaan awal | `PASS` | Bagian 6.1 |
| `npx playwright test tests/e2e/blood-unit-allocation-screen.spec.mjs --workers=1` | **16 passed (40,8 detik)** | `PASS` | Regresi `FE-BD-004`, termasuk R7 — tombol penyimpanan tetap tersembunyi karena spec itu tidak memberi `BloodStorageLocation : Read` |

`AUTOMATED TEST: npm run test:unit — PASS` (7 test baru lulus; 7 kegagalan lain sudah ada sebelumnya).

`MANUAL TEST: PASS` — 9 skenario di Chromium sungguhan terhadap backend sungguhan (bagian 6.1).

### 6.1 Validasi runtime `A9` — 24 September 2026

**Cara pembuktiannya.** Hasil `next build` standalone dijalankan pada `http://127.0.0.1:3710` dan diuji
lewat Playwright Chromium. **Berbeda dari `FE-BD-004`, jawaban API tidak dikarang**: setiap request
`/v1/**` dari layar diteruskan (`route.fetch`) ke **backend terbaru** — hasil build `BE-BD-020` yang
dijalankan terpisah di `http://localhost:5217`, database `QuilvianNewDevSukma`, sesi `superadmin`.
Backend milik pemilik di port `5107` (dinyalakan sebelum `BE-BD-020`) tidak disentuh. Satu-satunya
jawaban yang dipasang adalah daftar kewenangan pada skenario `A3a`–`A3c`, supaya ketiadaan satu butir
dapat dibuktikan tanpa membuat akun baru. Angka pembanding diambil langsung dari backend di dalam test.

| Kode | Skenario | Hasil |
| --- | --- | --- |
| `A1` | **Belum Disimpan** → request `unitStatus=0` **tanpa** `inactiveLocation`; tombol aktif tertahan; jumlah baris = `totalData` backend; `TEST-BD006-…-01` tampil | `PASS` |
| `A2` | Setelah Belum Disimpan, **Lokasi Nonaktif** → request `inactiveLocation=true` **tanpa** `unitStatus` (eksklusif); `aria-pressed=true`; jumlah baris = backend; `TEST-BD009-…-08` tampil dengan penanda "Lokasi nonaktif"; ditekan lagi → saringan mati pada daftar yang sama, `aria-pressed=false` | `PASS` |
| `A3` | Kantong `Received`: **Tetapkan Lokasi** ada, **Pindahkan Lokasi** tidak; riwayat "Kantong belum pernah disimpan."; dialog memanggil `/blood-storage-locations/options`; jumlah opsi = jumlah lokasi aktif backend; isi permintaan **tepat** `note`, `storageLocationId`, `version` dengan `version` = versi backend; `200`; label status sesudahnya = label backend (**Tersedia**, lewat Tersimpan); tombol berganti Pindahkan; riwayat "Penempatan pertama" + **Berlaku** + keterangan | `PASS` |
| `A4` | Kantong `PendingReview` di lokasi nonaktif `TBD009-LOCB`: penanda nonaktif tampil; **Pindahkan Lokasi** tetap ada; dialog memperingatkan "lokasi ini nonaktif"; pindah ke `TBD009-LOCA` → `200`; status **tetap** Menunggu keputusan menurut backend; riwayat 2 baris — lama bertanda "Lokasi nonaktif", baru **Berlaku**; kantong keluar dari `inactiveLocation=true` backend | `PASS` |
| `A7-409` | Dialog terbuka, "petugas lain" memindahkan kantong yang sama lewat API sungguhan → konfirmasi layar mendapat **`409` dari backend**; dialog tertutup; detail dimuat ulang dan menampilkan lokasi baru `TBD007-LOCY` | `PASS` |
| `A7-422` | Dialog terbuka, lokasi tujuan `TBD006-LOC1` dinonaktifkan lewat API sungguhan → **`422`** dan kalimat `VAL-BD-060` persis tampil di dialog; lokasi diaktifkan kembali di blok `finally` | `PASS` |
| `A3a` | Hak akses `BloodUnit : Read` + `BloodUnit : Store` + `BloodStorageLocation : Read` → **Pindahkan Lokasi** muncul (kontrol positif) | `PASS` |
| `A3b` | Tanpa `BloodUnit : Store` → nol kemunculan | `PASS` |
| `A3c` | Tanpa `BloodStorageLocation : Read` → nol kemunculan | `PASS` |

**Riwayat percobaan.** Run pertama: 5 lulus, `A7-422` gagal, 3 tidak berjalan. Sebabnya **cacat spec,
bukan produk**: request detail yang masih berjalan terputus saat test berakhir, dan Playwright menganggap
galat callback route itu kegagalan. Spec diperbaiki (galat `route.fetch` sesudah halaman ditutup ditelan,
`unrouteAll` pada `afterEach`); keempat sisanya lalu lulus; fixture dipulihkan; dan spec lengkap dijalankan
ulang dari keadaan awal: **9 dari 9 lulus dalam satu run**.

**Keadaan database.** Snapshot kantong `TEST-BD006-20260914094559-01` dan `TEST-BD009-20260917100009-08`,
penempatannya, lima lokasi, jumlah penempatan (41), dan riwayat transisi (213, terakhir 23 September)
diambil sebelum pengujian. Sesudah setiap run, baris yang dibuat uji dihapus dan field yang berubah
dikembalikan lewat SQL; hasil akhirnya **identik** dengan snapshot. Catatan log aplikasi backend
(`LoggerService`) dari aksi uji tidak dihapus.

### 6.2 Validasi ulang atas permintaan pemilik — 24 September 2026

Pemilik meminta validasi runtime `A1`–`A9` dijalankan ulang dengan pola `FE-BD-004`, dengan butir
pemeriksaan yang lebih rinci. **Nol source fitur diubah**; hanya spec e2e yang diperkuat supaya setiap
butir permintaan punya assertion eksplisit:

| Butir permintaan | Assertion yang ditambahkan |
| --- | --- |
| `A5` hanya lokasi aktif dari `/options`, frontend tidak membuat daftar sendiri | Kode lokasi di pemilih **sama persis** dengan kode dari `GET /options` backend, dan ketiga lokasi nonaktif di database (`TBD007-LOCX`, `TBD008-LOCB`, `TBD009-LOCB`) **tidak** ada |
| `A6` lokasi sebelumnya tampil, urutan mengikuti backend, tanpa pelaku | Baris perpindahan menampilkan `TBD009-LOCB` sebagai lokasi sebelumnya; baris pertama "Penempatan pertama"; urutan kode lokasi di tabel = urutan `GET /placements`; kepala tabel tanpa kata oleh/petugas/pelaku |
| `A7` 422 — detail mengikuti backend | Sesudah `422`, `GET /blood-units/{id}` dipanggil ulang, dan lokasi kantong menurut backend tetap `TBD007-LOCY` |
| `A3` tombol Tetapkan hanya bila `AvailableActions` + dua hak akses | `A3d`–`A3f` pada kantong `Received` lain (hanya dibaca, nol request tulis); `A3g`: kantong yang sudah punya lokasi tidak ditawari Tetapkan Lokasi walau hak akses penuh — sisi `AvailableActions` |

**Perintah dan hasil**, berurutan dari keadaan awal yang sudah diverifikasi identik dengan snapshot:

| Perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| `npm run lint:errors` (mencakup spec yang diperkuat) | Kode keluar `0` | `PASS` |
| `npm run build` | Kode keluar `0`, `Compiled successfully in 40s`, 370 halaman, standalone siap | `PASS` |
| `npx playwright test tests/e2e/blood-unit-storage-screen.spec.mjs --workers=1` | **13 passed (1,1 menit)** dalam satu run | `PASS` |
| `npx playwright test tests/e2e/blood-unit-allocation-screen.spec.mjs --workers=1` | **16 passed (34,9 detik)** — regresi `FE-BD-004` | `PASS` |
| `npm run test:unit` | 1583 test — 1576 lulus, 7 gagal lama (sama dengan baseline) | `PASS` untuk cakupan task |

**Hasil per acceptance — run ulang:**

| Acceptance | Skenario | Hasil |
| --- | --- | --- |
| `A1` | `A1` — request `unitStatus=0`, jumlah baris = `totalData` backend | `PASS` |
| `A2` | `A2` — request `inactiveLocation=true` tanpa `unitStatus`; eksklusif; penanda "Lokasi nonaktif"; dapat dimatikan | `PASS` |
| `A3` | `A3`, `A3a`–`A3g` — gerbang `AvailableActions` + `BloodUnit : Store` + `BloodStorageLocation : Read` untuk kedua tombol; isi tepat `note`/`storageLocationId`/`version`; status sesudah simpan = backend (Diterima → Tersimpan → **Tersedia**, tercatat pada `BbkTransitionHistory`) | `PASS` |
| `A4` | `A4` — tombol tetap ada di lokasi nonaktif; pindah `200`; status tetap Menunggu keputusan; riwayat bertambah menjadi 2 baris | `PASS` |
| `A5` | Di dalam `A3` — daftar kode sama persis dengan `/options`, nol lokasi nonaktif | `PASS` |
| `A6` | Di dalam `A3` dan `A4` — lokasi, lokasi sebelumnya, Berlaku/nonaktif, urutan = backend, tanpa pelaku | `PASS` |
| `A7` | `A7-409` dan `A7-422` — kode dan kalimat asli backend, dialog/detail sesuai | `PASS` |
| `A8` | Lihat pemeriksaan statik di bawah, ditambah `A3`/`A4` (status dibaca dari backend) dan unit test `buildStorageResult` | `PASS` |
| `A9` | Seluruh run di atas: standalone + Chromium + backend `BE-BD-020` | `PASS` |

**Pemeriksaan statik `A8`.** Grep baris tambahan pada seluruh source yang diubah terhadap
perbandingan status (`unitStatus ===`, konstanta status), penilaian keaktifan (`isActive ===`), dan
penyaringan opsi berdasarkan status atau keaktifan: **nol temuan logika**. Dua kecocokan hanya
pengecekan panjang array untuk state UI (`placements.length === 0` untuk kerangka memuat,
`locationOptions.length === 0` untuk menahan tombol konfirmasi). `use-blood-unit-storage.jsx` tidak
memuat satu pun rujukan status kantong maupun `isActive`. Penanda "Lokasi nonaktif" hanya
**menampilkan** flag `isStorageLocationActive`/`isCurrentStorageLocationActive` dari backend.

**Kebersihan sesudah run:** database kembali **identik** dengan snapshot (2 riwayat transisi dan
3 penempatan uji dihapus, field dikembalikan); kedua server uji dihentikan; berkas cookie dihapus;
nol kemunculan token di `test-results/` dan `playwright-report/`; dua berkas tracked di `test-results/`
yang berubah karena Playwright dipulihkan lagi dengan `git restore` pada kedua path itu saja.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria (`D4`) | Status | Bukti |
| --- | --- | --- |
| `A1` saringan Belum Disimpan | Terpenuhi | `A1` |
| `A2` saringan Lokasi Nonaktif, bukan daftar kerja baru | Terpenuhi | `A2` — saringan pada daftar yang sama, dapat dimatikan |
| `A3` Tetapkan Lokasi hanya dari `AvailableActions` + `BloodUnit : Store` + `BloodStorageLocation : Read`, status dari backend | Terpenuhi | `A3`, `A3a`–`A3c`; `use-blood-unit-storage.jsx` |
| `A4` Pindahkan Lokasi idem, tetap muncul saat lokasi asal nonaktif | Terpenuhi | `A4`, `A3a`–`A3c` |
| `A5` pilihan lokasi hanya dari `/options` | Terpenuhi | `A3` (jumlah opsi = backend); unit test "lokasi saat ini tetap ditawarkan" |
| `A6` riwayat penempatan urut + penanda berlaku/nonaktif | Terpenuhi | `A3`, `A4`; `sortLatestFirst={false}` |
| `A7` `409`/`422` tampil apa adanya lalu muat ulang | Terpenuhi | `A7-409`, `A7-422` — keduanya dari backend sungguhan |
| `A8` nol validasi status di frontend | Terpenuhi | Status sesudah penetapan dibaca dari backend (`A3`, `A4`); unit test `buildStorageResult`; nol pembandingan status di hook/view |
| `A9` validasi runtime di browser terhadap backend terbaru | Terpenuhi | Bagian 6.1 |
| DoD kartu: **bukan** daftar kerja keempat | Terpenuhi | Preset menempel pada daftar `FE-BD-04` yang sudah ada |
| Kewajiban `FE-BD-010`: saringan `Received` dan lokasi nonaktif | Terpenuhi | `A1`, `A2` |
| Kewajiban `FE-BD-011`: kolom lokasi beserta penandanya | Terpenuhi | Sudah dibangun `FE-BD-004`; terbukti ulang di `A2` dan `A4` |
| Nol route/menu baru | Terpenuhi | Bagian 3.2 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Run pertama e2e mencetak header request — termasuk cookie JWT sesi `superadmin` **backend lokal** — ke keluaran terminal agent melalui pesan galat Playwright. Token itu **tidak** tersimpan di berkas mana pun (artefak `test-results/` diperiksa: nol kemunculan), berkas cookie di scratchpad sudah dihapus, dan spec kini menelan galat tersebut tanpa meneruskan header. Token berlaku sampai kedaluwarsa bawaannya; bila dianggap perlu, logout/rotasi sesi `superadmin` lokal. |
| Masalah yang diketahui | **GAP BACKEND `placedByName` (`D1`).** `BloodUnitPlacementDto` hanya membawa `PlacedByUserId`; spesifikasi `FE-BD-05` meminta "oleh siapa". Kolom pelaku tidak ditampilkan. Yang dibutuhkan: satu isian `placedByName` dengan pola `GetActorName` yang sudah baku — aditif, nol klien rusak. Belum dibuka sebagai task. **Temuan backend:** pindah ke lokasi yang sama diterima dan menambah baris riwayat; layar tidak mencegahnya karena penilaian tujuan adalah wewenang backend. **Dokumen:** kartu roadmap `FE-BD-012` masih menulis kontrak `v4` dan dependency hanya `BE-BD-015`, padahal saringan `inactiveLocation` bergantung pada `BE-BD-020` (`v5` `D6`) — di luar wewenang tulis roadmap task ini |
| Dependency backend | Nihil yang tertunda. Runtime pemilik di port `5107` perlu dijalankan ulang dari commit `8bc7b512` supaya saringan **Lokasi Nonaktif** berfungsi di lingkungan itu — tanpa itu backend lama mengabaikan `inactiveLocation` dan daftar tampil tanpa tersaring |
| Perubahan sampingan | Dua berkas tracked di `test-results/` berubah karena run Playwright (`.last-run.json` dimodifikasi; `…inpatient-admiss-…/error-context.md` terhapus). Keduanya tidak ada dalam keadaan berubah saat task dimulai (working tree bersih) dan dipulihkan dengan `git restore` pada kedua path itu saja |
| Interupsi | `NONE` |
| Status Git | `M` 8 berkas source, `??` `use-blood-unit-storage.jsx`, `tests/unit/blood-unit-storage.test.mjs`, `tests/e2e/blood-unit-storage-screen.spec.mjs` — seluruhnya milik task ini. Nol stage, nol commit |
| Langkah berikutnya | Commit source frontend bila disetujui pemilik. Task frontend berikutnya: **`FE-BD-005`** (golongan darah, bukti kecocokan, pemberian, jalur darurat) — kantong Tersedia kini dapat dihasilkan dari layar tanpa bantuan API. Opsional: buka task backend kecil `placedByName` |
