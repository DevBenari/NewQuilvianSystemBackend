# Laporan Perubahan Frontend — `FE-BD-007`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BD-007` |
| Judul | Penyelesaian `PendingReview`, tiga tombol tiga penjaga |
| Slice | Slice 3 — Kantong darah, pemberian, dan penyelesaiannya |
| Roadmap | `docs/module-blueprints/bank-darah/roadmap/frontend-roadmap.md` §3 kartu `FE-BD-007` |
| Trace | `DEC-BD-019`, `DEC-BD-028`, `DEC-BD-043`, `DEC-BD-045` · `INV-BD-013`, `INV-BD-034` · `03-frontend-architecture.md` §3 `FE-BD-05` baris "Tombol Penyelesaian (`PendingReview`)" · kewajiban layar `FE-BD-020` · `AC-BD-024`, `AC-BD-025`, `AC-BD-071`, `AC-BD-092`, `AC-BD-093`, `AC-BD-094` (dibuktikan backend `BE-BD-009`) |
| Contract version | Kartu tidak menyebut versi; kontrak berlaku `v5` (disetujui `Sukmagp` 19 September 2026). Ketiga endpoint penyelesaian tidak berubah sejak `v4`; dibaca langsung dari source backend `cf58ca8c` |
| Wewenang UI | Layar `FE-BD-05` (detail kantong) sebatas tiga tombol penyelesaian `PendingReview`, dialognya, dan penanda penyebab menunggu keputusan. Rupa layar `DEV_DISCRETION` mengikuti modul Kantong Darah |
| Dependency | `BE-BD-009` ✅; `FE-BD-004` ✅ (layar detail dan preset **Menunggu Keputusan**); `FE-BD-006` ✅ (`useEffectivePermissions`) |
| Klasifikasi | `MEDIUM` — tiga tindakan tulis, tiga butir hak akses terpisah, satu dialog berantai (order → baris), dua tindakan permanen |
| Task mode | `FRONTEND` |
| Target tulis | `V2QuilvianSystemFrontendDev` cabang `sukmagpV2`; laporan dan tautan bukti pada `docs/module-blueprints/bank-darah/` di repository backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `1d2cf3c9d52540f4921d925d6c5fdb07e251e046` (perubahan task ini belum di-commit) |
| Commit backend yang dijadikan rujukan | `cf58ca8cc8a83fbc333a9ba85bee9e49c5839ce4` cabang `sukmagp` |
| Tanggal | 24 September 2026 |
| Status | ✅ **Selesai.** Acceptance `FE-BD-020` terbukti runtime (`R2a`–`R2g`). `lint:errors` `PASS`, `test:unit` 1611 test (1604 lulus; 6 test baru lulus; 7 kegagalan lama sama dengan baseline), `build` `PASS` (371 halaman), runtime **13 dari 13 skenario `PASS`** terhadap backend sungguhan. Kartu tidak punya baris DoD (`NOT APPLICABLE`) |

---

## 1. Keadaan yang ditemukan di awal

- Detail kantong `FE-BD-05` sudah ada sejak `FE-BD-004`, termasuk preset **Menunggu Keputusan**
  (daftar kerja #2) pada daftar kantong. Kantong `PendingReview` hanya menampilkan keterangan
  "Penyelesaian kantong ini … belum tersedia di layar ini."
- Backend sudah lengkap sejak `BE-BD-009`: `POST /{id}/reallocate`, `/return-to-provider`, dan
  `/mark-not-usable`, dijaga tiga butir hak akses berbeda (`ResolveReallocate`, `ResolveReturn`,
  `ResolveNotUsable`) dengan kode penolakan `VAL-BD-080/081/082`. `AvailableActions` pada kantong
  `PendingReview` menawarkan ketiga nama aksi itu.
- Setiap jalur menuntut **kategori alasan berbeda**: `PendingReviewResolution`, `Return`, dan
  `NotUsable`. Kategori lain ditolak `422`.
- **Tidak ada field penyebab `PendingReview`.** Yang tersedia hanya `IsExcess` dan riwayat
  `Transitions` (terurut dari terlama).
- **Gap kontrak terbuka sejak `BE-BD-009` §10.2:** status `Reallocated` tidak punya perpindahan
  sesudahnya. Backend hanya menawarkan pindah lokasi, sehingga kantong yang dialihkan belum dapat
  dicatat bukti kecocokannya maupun diberikan.
- Register keputusan masih menandai `DEC-BD-043`/`DEC-BD-045` sebagai `draft`, padahal kontrak `v4`
  yang memuatnya sudah disetujui dan diimplementasikan. Ketidaksesuaian dokumen, tidak menahan task.

### 1.1 Keputusan pemilik `Sukmagp`, 24 September 2026

| No | Keputusan |
| --- | --- |
| `G1` | Opsi A: tombol **Alihkan** dibangun sesuai kontrak backend. Status `Reallocated` yang belum punya jalan keluar ke pemberian dicatat sebagai **backlog kontrak/backend**. Tanpa jalan pintas frontend, tanpa transisi baru |
| `G2` | Opsi A: tampilkan `IsExcess` dan `ReasonNote` transisi **terakhir** yang `ToStatus`-nya `PendingReview`, tanpa aturan bisnis baru |
| `G5` | **Kembalikan ke PMI** dan **Tidak Layak** memakai `ConfirmModal` bervarian bahaya, dengan teks yang menjelaskan tindakan permanen |
| Cakupan | Tiga tombol saja. Perbaikan kontrak `Reallocated` dan endpoint baru **tidak** termasuk |

---

## 2. Proses bisnis dari sisi pengguna

**Contoh.** Kantong `TEST-BD006-20260914094559-07` menunggu keputusan karena berlebih dari
kebutuhan order asalnya.

### 2.1 Alur normal

| No | Langkah | Pelaku | Yang terlihat di layar |
| ---: | --- | --- | --- |
| 1 | Buka **Kantong Darah**, tekan **Menunggu Keputusan**, lalu klik dua kali kantongnya | Petugas BDRS | Detail kantong. Di bawah kepala halaman ada peringatan **Kantong menunggu keputusan** berisi penyebab dari backend, misalnya "Kantong ini berlebih dari kebutuhan order asalnya." dan/atau "Keterangan: Order asal dibatalkan (tanggal)." |
| 2a | Tekan **Alihkan** | Pemegang kewenangan klinis BDRS | Dialog: pilih **Order Darah**, lalu **Baris Kebutuhan**, lalu **Alasan Pengalihan**. Ringkasan order (pasien, nomor rekam medis, status order) tampil dari backend. Teks dialog menyebut bahwa bukti kecocokan lama gugur |
| 3a | Tekan **Alihkan** di dialog | Pemegang kewenangan klinis BDRS | Pemberitahuan **Kantong dialihkan** dengan pesan backend. Status menjadi **Dialihkan**; ketiga tombol hilang |
| 2b | Tekan **Kembalikan ke PMI** | Pemegang kewenangan operasional BDRS | Dialog bahaya: "Kantong … akan dikembalikan kepada PMI dan keluar dari peredaran. **Tindakan ini permanen** …" dan pemilih **Alasan Pengembalian** |
| 3b | Tekan **Ya, Kembalikan ke PMI** | Pemegang kewenangan operasional BDRS | **Kantong dikembalikan ke PMI**; status akhir **Dikembalikan ke PMI**; nol tombol tindakan |
| 2c | Tekan **Tidak Layak** | Pemegang kewenangan penetapan kelayakan (`DEC-BD-045`) | Dialog bahaya dengan kalimat permanen yang sama dan pemilih **Alasan Tidak Layak** |
| 3c | Tekan **Ya, Nyatakan Tidak Layak** | Pemegang kewenangan penetapan kelayakan | **Kantong dinyatakan tidak layak**; status akhir **Tidak layak** |

Setiap pemilih alasan hanya menawarkan alasan aktif dari kategori jalurnya. Siapa pasien tujuan,
apakah order masih berjalan, apakah lokasi aktif, dan apa status akhirnya, seluruhnya ditentukan
backend.

### 2.2 Jalur tidak normal

| Keadaan | Yang terjadi |
| --- | --- |
| Petugas hanya memegang satu butir, misalnya `ResolveReturn` | Hanya tombol **Kembalikan ke PMI** yang tampil (`FE-BD-020`) |
| Tanpa `BloodBankReason : Read` | Ketiga tombol tersembunyi, karena alasan wajib dipilih dari daftar |
| Tanpa `BloodOrder : Read` | Tombol **Alihkan** tersembunyi, karena order tujuan tidak dapat dipilih; dua tombol lain tetap tampil |
| Kantong tidak menunggu keputusan (misalnya Tersedia) | Nol tombol penyelesaian, karena `AvailableActions` tidak menawarkannya |
| Alihkan kantong yang lokasinya nonaktif | Backend menolak `422 VAL-BD-064`. Pesannya tampil apa adanya di dialog, dialog tetap terbuka, dan kantong tetap Menunggu keputusan. Tombol **Pindahkan Lokasi** tersedia di layar yang sama |
| Order tujuan sudah berakhir, atau kategori alasan tidak sesuai | Backend menolak `422`; pesan tampil di dialog dan detail dimuat ulang |
| Kantong baru saja diubah petugas lain | Backend menolak `409`; dialog ditutup, pesan tampil, dan detail dimuat ulang |
| Belum ada alasan aktif untuk kategori itu | Kalimat "Belum ada alasan aktif untuk …" dan tombol konfirmasi nonaktif |
| Kantong sudah **Dialihkan** | Hanya **Pindahkan Lokasi** yang tersedia. Pencatatan bukti dan **Berikan** tidak tampil karena backend tidak menawarkannya (gap `G1`) |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Blueprint: kartu `FE-BD-007`, `03-frontend-architecture.md` §3 `FE-BD-05` dan kewajiban
  `FE-BD-020`, register `00-interview-decisions.md` (`DEC-BD-043`, `DEC-BD-045`, `OQ-BD-017`,
  matriks status `PENDING_REVIEW`), laporan `BE-BD-009` dan `BE-BD-010`, `state-transition-matrix.md`,
  `requirement-traceability.md`.
- Backend (baca saja): `BbkBloodUnitController.cs` (ketiga endpoint, `MapFailure`),
  `BloodUnitResolutionDtos.cs`, `BbkBloodUnitService.cs` (`ReallocateAsync`, `ReturnToProviderAsync`,
  `MarkNotUsableAsync`, `ResolveOutOfCirculationAsync`, `BeginResolutionAsync`,
  `ResolveControlledReasonAsync`, `AvailableActionsFor`, `ReadTransitionsAsync`, pesan penolakan),
  `BloodUnitDtos.cs`, `BloodBankCommonDtos.cs`, `BbkBloodUnitStatus.cs`, `MstBloodBankReason.cs`,
  `BloodBankReasonSeeder.cs`.
- Frontend: `blood-unit-detail-view.jsx`, `blood-unit-issuance-dialogs.jsx`,
  `use-blood-unit-detail.jsx`, `use-blood-unit-issuance.jsx`, `use-blood-unit-storage.jsx`,
  `blood-unit.service.js`, `blood-order.service.js`, `blood-unit-constants.jsx`, `blood-unit-utils.js`,
  `use-permission.jsx`, `confirm-modal.jsx`, spec `blood-unit-issuance-screen.spec.mjs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/blood-bank-management/blood-unit-constants.jsx` | **Diubah.** Tiga nama aksi `ResolveReallocate`/`ResolveReturn`/`ResolveNotUsable` dan `PENDING_REVIEW_REASON_CATEGORY` per jalur |
| `src/lib/services/health-services/blood-bank-management/blood-unit.service.js` | **Diubah.** `reallocateBloodUnit` (tepat `bloodOrderLineId`, `reasonCode`, `version`), `returnBloodUnitToProvider` dan `markBloodUnitNotUsable` (tepat `reasonCode`, `version`) |
| `src/utils/health-services/blood-bank-management/blood-unit-utils.js` | **Diubah.** `readPendingReviewCause` (keputusan `G2`) dan `buildResolutionResult` (pesan backend apa adanya) |
| `src/lib/hooks/health-services/blood-bank-management/use-blood-unit-resolution.jsx` | **Baru.** Sub-hook penyelesaian dengan pola `useBloodUnitIssuance`: tiga gerbang hak akses terpisah, pemilih order → baris, alasan per kategori, kirim, dan penanganan `409`/`422` |
| `src/lib/hooks/health-services/blood-bank-management/use-blood-unit-detail.jsx` | **Diubah.** Merangkai `useBloodUnitResolution`, mengekspos `pendingReviewCause`, dan membersihkan galat penyelesaian saat muat ulang |
| `src/components/view/health-services/blood-bank-management/blood-units/detail/blood-unit-resolution-dialogs.jsx` | **Baru.** Tiga `ConfirmModal`: Alihkan (primary), Kembalikan ke PMI dan Tidak Layak (danger, kalimat permanen) |
| `src/components/view/health-services/blood-bank-management/blood-units/detail/blood-unit-detail-view.jsx` | **Diubah.** Tiga tombol, penanda penyebab menggantikan keterangan "belum tersedia", galat penyelesaian, dan tanda sibuk gabungan |
| `tests/unit/blood-unit-resolution.test.mjs` | **Baru.** 6 test utility murni dan konstanta kontrak |
| `tests/e2e/blood-unit-resolution-screen.spec.mjs` | **Baru.** 13 skenario runtime terhadap backend sungguhan |

### 3.3 Kepatuhan arsitektur frontend

- Alur `view → hook → service → InstanceAxios` dipatuhi. Sub-hook baru mengikuti pola
  `useBloodUnitIssuance`/`useBloodUnitStorage`, dan kodenya disambungkan lewat
  `applyDetail`/`requestReload`/`onResult` yang sudah ada.
- Dipakai ulang tanpa diubah: `getBloodOrders`, `getBloodOrderDetail`, `getBloodBankReasonOptions`,
  `buildOrderOptionsForAllocation`, `buildOrderLineOptions`, `summarizeOrderForAllocation`,
  `buildReasonOptions`, `isPendingReviewStatus`, dan `useEffectivePermissions`. Nol CSS baru —
  `blood-unit.module.css` dipakai apa adanya.
- **Nol logika bisnis frontend.** Kelayakan dari `AvailableActions`, izin dari `decide()`, penyebab
  dari `IsExcess` dan `ReasonNote`, status akhir dan penolakan dari backend. Pemetaan tombol →
  kategori alasan adalah kontrak endpoint (sama dengan `AllocationCancellation` pada `FE-BD-004`), dan
  backend tetap menolak kategori yang tidak sesuai. Tidak ada jalan pintas untuk status `Reallocated`.
- Pemilih di dalam dialog **tidak** dibungkus `<label>` (perbaikan temuan `FE-BD-005`). Runtime
  membuktikan daftar tertutup sendiri sesudah dipilih.
- Logika pemilih order → baris ditulis ulang di sub-hook baru, tidak diekstrak dari dialog alokasi
  `FE-BD-004`, supaya task ini tidak me-refactor pekerjaan task lain.

### 3.4 Gerbang keputusan base component

`UI GATE: 6 elemen — REUSE 2, EXTEND 0, COMPOSE 4, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Tiga tombol penyelesaian | `BaseButton` | varian `primary`/`danger` dipakai `blood-unit-detail-view` | REUSE | Alihkan `primary`; Kembalikan ke PMI dan Tidak Layak `danger` |
| Pemberitahuan hasil | `resultNotice` + `ToastStack` lewat `BaseDetailView` | pola `FE-BD-004`/`005` | REUSE | Pesan backend apa adanya |
| Penanda penyebab menunggu keputusan | `InformationAlert` | menggantikan alert `FE-BD-004` di posisi yang sama | COMPOSE | Kalimat disusun dari data backend saja |
| Dialog Alihkan | `ConfirmModal` + `FilterSelect` + `InformationAlert` | pola dialog alokasi `FE-BD-004` | COMPOSE | Pemilih memakai pola `htmlFor` `FE-BD-005` |
| Dialog Kembalikan ke PMI | `ConfirmModal` `danger` + `FilterSelect` | pola dialog pembatalan alokasi `FE-BD-004` | COMPOSE | Keputusan pemilik `G5` |
| Dialog Tidak Layak | `ConfirmModal` `danger` + `FilterSelect` | idem | COMPOSE | Keputusan pemilik `G5` |

Keempat baris `COMPOSE` merujuk keputusan komposisi yang sama pada `FE-BD-004`/`FE-BD-005` di modul
yang sama. Nol base component diubah.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tombol **Menyiapkan...** saat alasan dimuat; pemilih "Memuat order darah...", "Memuat baris kebutuhan...", "Memuat alasan..."; tombol konfirmasi berputar selama kiriman |
| Kosong | "Tidak ada order darah yang dapat dipilih saat ini." · "Belum ada alasan aktif untuk …" · baris kebutuhan "Pilih order darah lebih dulu"; tombol konfirmasi nonaktif |
| Gagal | Pesan backend apa adanya di dialog (`400`/`422`); di kepala halaman bila dialog sudah tertutup (`409`); gagal memuat pilihan: "Daftar order darah gagal dimuat." / "Pilihan alasan … gagal dimuat." |
| Tanpa hak akses | Tombol yang tak berhak tidak ditampilkan (`FE-BD-020`); `403` backend tetap ditampilkan lewat `AccessDeniedGate` |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Blood Bank Management / Blood Unit

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/blood-bank-management/blood-units/{id}` | Detail, `AvailableActions`, `IsExcess`, `Transitions`, `Version` | `BloodUnit : Read` |
| `POST` | `/v1/health-services/blood-bank-management/blood-units/{id}/reallocate` | Alihkan; isi tepat `bloodOrderLineId`, `reasonCode`, `version` | `BloodUnit : ResolveReallocate` (`403 VAL-BD-080`) |
| `POST` | `/v1/health-services/blood-bank-management/blood-units/{id}/return-to-provider` | Kembalikan ke PMI; isi tepat `reasonCode`, `version` | `BloodUnit : ResolveReturn` (`403 VAL-BD-081`) |
| `POST` | `/v1/health-services/blood-bank-management/blood-units/{id}/mark-not-usable` | Nyatakan tidak layak; isi tepat `reasonCode`, `version` | `BloodUnit : ResolveNotUsable` (`403 VAL-BD-082`) |

#### Health Services / Blood Bank Management / Blood Order

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/blood-bank-management/blood-orders` | Pilihan order tujuan pengalihan | `BloodOrder : Read` |
| `GET` | `/v1/health-services/blood-bank-management/blood-orders/{id}` | Baris kebutuhan order tujuan | `BloodOrder : Read` |

#### Health Services / Master Data / Blood Bank Reason

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/master-data/blood-bank-reasons/options?category=` | `PendingReviewResolution` / `Return` / `NotUsable` per tombol | `BloodBankReason : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Kode keluar `0`, nol error | `PASS` | Keluaran perintah |
| `npx eslint` pada berkas task | 0 error, 3 warning | `EXISTING WARNING` | Ketiganya sudah ada sebelum task: dua `set-state-in-effect` di `use-blood-unit-detail.jsx` dan satu `exhaustive-deps` pada `state.detail \|\| {}` di view. Berkas baru task ini nol warning |
| `npm run test:unit` | 1611 test — **1604 lulus, 7 gagal**; ke-6 test baru lulus | `PASS` untuk cakupan task | Ketujuh kegagalan sama persis dengan baseline `FE-BD-009`: `route, menu, dan store terdaftar`, empat `FE-RWI-042`, `FE-RWI-043`, `M0` menu Setup Bank Darah |
| `npm run build` | Kode keluar `0`, `Compiled successfully in 38.5s`, **371 halaman**, standalone siap | `PASS` | `/blood-units` dan `/blood-units/[slug]` terdaftar |
| Grep anti-regresi UI | Nol tombol mentah, nol `<table>`, nol utility typography Bootstrap, nol inline style, nol pemanggilan Axios di view, nol CSS baru; `<label>` hanya berbentuk `htmlFor` | `PASS` | Keluaran grep pada berkas diubah |
| Runtime `R0`–`R6` | **13 dari 13 skenario `PASS`** (run 1: 11 lulus, 1 gagal karena cacat spec, 1 tidak berjalan; run 2: 3 dari 3 lulus) | `PASS` | Bagian 6.1–6.2 |

`AUTOMATED TEST: npm run test:unit — PASS` (6 test baru lulus; 7 kegagalan lain sudah ada sebelumnya).

Uji manual: `PASS` — tombol, ketiga dialog, pemilih berantai order → baris, pemilih alasan,
konfirmasi, dan penolakan backend dijalankan di peramban sungguhan lewat Playwright (bagian 6.2).

### 6.1 Skenario runtime

Spec `tests/e2e/blood-unit-resolution-screen.spec.mjs` mengikuti pola `FE-BD-005`/`FE-BD-009`. Hasil
build standalone dijalankan pada `http://127.0.0.1:3710` dan diuji lewat Playwright Chromium. Setiap
request `/v1/**` diteruskan ke **backend sungguhan**: `QuilvianSystemBackend` Debug di
`https://localhost:7184`, database `QuilvianNewDevSukma`, sesi `superadmin`. **Jawaban bisnis backend
tidak pernah dikarang.** Satu-satunya jawaban yang dipasang adalah daftar kewenangan pada `R2a`–`R2g`
(`isSuperAdmin: false`), supaya ketiadaan satu butir dapat dibuktikan tanpa membuat akun baru.

**Fixture (`R0`, ditemukan dari backend).** Kantong `TEST-` berstatus Menunggu keputusan dari fixture
`BE-BD-006`/`BE-BD-009`; order tujuan `ORD-00000087` (Aktif) baris 1; alasan `TBD009-ALIH`
(`PendingReviewResolution`), `TBD009-KEMBALI` (`Return`), `TBD009-TIDAKLAYAK` (`NotUsable`).

### 6.2 Hasil validasi runtime — 24 September 2026

| Kode | Skenario | Hasil |
| --- | --- | --- |
| `R0` | Fixture: kantong Menunggu keputusan di lokasi aktif dan nonaktif, satu kantong Tersedia, alasan per kategori, baris order tujuan | `PASS` |
| `R1` | Kantong Menunggu keputusan, hak penuh: **Kantong menunggu keputusan** dengan penyebab dari backend; **Alihkan**, **Kembalikan ke PMI**, **Tidak Layak** tampil; keterangan "belum tersedia di layar ini" hilang | `PASS` |
| `R2a` | `FE-BD-020`: ketiga butir → 3 tombol | `PASS` |
| `R2b` | Hanya `ResolveReallocate` → hanya **Alihkan** | `PASS` |
| `R2c` | Hanya `ResolveReturn` → hanya **Kembalikan ke PMI** | `PASS` |
| `R2d` | Hanya `ResolveNotUsable` → hanya **Tidak Layak** | `PASS` |
| `R2e` | Ketiga butir tanpa `BloodBankReason : Read` → nol tombol | `PASS` |
| `R2f` | Ketiga butir tanpa `BloodOrder : Read` → **Alihkan** tersembunyi, dua lainnya tampil | `PASS` |
| `R2g` | Kantong Tersedia dengan hak penuh → nol tombol (sisi `AvailableActions`) | `PASS` |
| `R3` | Alihkan kantong `TEST-BD009-20260917100009-08` di lokasi nonaktif → **`422`, `errors.code` `VAL-BD-064`**, pesan tampil di dialog; kantong tetap Menunggu keputusan dan `version` tidak bergeser | `PASS` |
| `R4` | Kembalikan ke PMI `TEST-BD006-20260914094559-B1`: dialog bertuliskan **Tindakan ini permanen**, tombol konfirmasi berkelas `danger`, alasan diminta dengan `category=Return`; `200` dengan isi **tepat** `reasonCode` + `version` (versi backend); status akhir **Dikembalikan ke PMI**, nol tombol | `PASS` |
| `R5` | Tidak Layak `TEST-BD009-20260917100009-03`: dialog bahaya bertuliskan permanen, `category=NotUsable`; `200` dengan isi tepat `reasonCode` + `version`; status akhir **Tidak layak** | `PASS` (run 2) |
| `R6` | Alihkan `TEST-BD006-20260914094559-07` ke `ORD-00000087` baris 1: `category=PendingReviewResolution`; `200` dengan isi **tepat** `bloodOrderLineId`, `reasonCode`, `version`; **Kantong dialihkan** dengan pesan backend; status **Dialihkan**; `AvailableActions` backend tanpa `Issue` dan tombol **Berikan** tidak tampil (gap `G1`, tanpa jalan pintas) | `PASS` (run 2) |

**Riwayat percobaan, apa adanya.**

1. **Run 1** — 11 lulus (`R0`–`R4`); `R5` gagal, `R6` tidak berjalan. Sebabnya **cacat spec**: spec
   membaca catatan permintaan alasan sebelum proxy sempat mencatat jawabannya (balapan waktu).
   Kegagalan terjadi **sebelum** tombol konfirmasi ditekan, jadi nol kiriman tulis;
   `TEST-BD009-20260917100009-07` tetap Menunggu keputusan. Spec diperbaiki dengan menunggu
   (`expect.poll`) catatan permintaan alasan pada ketiga dialog. Source produk tidak berubah.
2. **Run 2** — `R0`, `R5`, `R6`: **3 dari 3 lulus**. `R0` menemukan ulang fixture; karena
   `TEST-BD006-…-B1` sudah dikembalikan di run 1, kantong Tidak Layak menjadi
   `TEST-BD009-20260917100009-03`. `R1`–`R4` tidak diulang karena source tidak berubah di antara run.

**Keadaan database sesudah run** (diperiksa langsung ke backend):

| Kantong | Keadaan akhir |
| --- | --- |
| `TEST-BD006-20260914094559-07` | **Dialihkan** (`version` 2), alokasi aktif ke `ORD-00000087`, transisi `PendingReview → Reallocated` `TBD009-ALIH`; `AvailableActions` hanya `MoveStorageLocation` |
| `TEST-BD006-20260914094559-B1` | **Dikembalikan ke PMI** (akhir), transisi `PendingReview → ReturnedToProvider` `TBD009-KEMBALI` |
| `TEST-BD009-20260917100009-03` | **Tidak layak** (akhir), transisi `PendingReview → NotUsable` `TBD009-TIDAKLAYAK` |
| `TEST-BD009-20260917100009-07` | Menunggu keputusan, tidak berubah |
| `TEST-BD009-20260917100009-08` | Menunggu keputusan, tidak berubah (pengalihan ditolak `VAL-BD-064`) |

Data ini dibiarkan, sama dengan perlakuan fixture `TEST-` pada `FE-BD-005`.

**Kebersihan sesudah run:** server standalone uji dihentikan; berkas cookie dan berkas bantu di
scratchpad dihapus; nol kemunculan token di log run, `test-results/`, dan `playwright-report/`; dua
berkas tracked di `test-results/` yang diubah Playwright dipulihkan dengan `git restore` pada kedua
path itu saja.

**Tidak dijalankan:** `test:uat` (tidak diminta). Skenario `409` tidak dijalankan di layar; penanganan
`409` memakai pola yang sama dengan `FE-BD-004`/`005`, dan konkurensinya sudah dibuktikan backend
`BE-BD-009` §6. Penolakan `403` **oleh backend** (`VAL-BD-080/081/082`) tidak diulang di sini, karena
sesi uji SuperAdmin; buktinya ada pada `BE-BD-009` §6 dengan dua akun non-SuperAdmin.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `FE-BD-020` — ketiga tombol punya penjaga hak akses **terpisah**, sesuai `DEC-BD-043` dan `DEC-BD-045` | **Terpenuhi** | Tiga pemeriksaan `decide()` terpisah di `use-blood-unit-resolution.jsx`; `R2a`–`R2g` membuktikan setiap butir membuka tombolnya sendiri saja |
| Catatan hak akses kartu: `AvailableActions` menyatakan kelayakan, bukan izin; pakai ulang pembaca hak akses `FE-BD-006` | **Terpenuhi** | Tombol menuntut keduanya: izin (`useEffectivePermissions`) dan kelayakan (`AvailableActions`) — `R2a` vs `R2g` |
| `03-frontend-architecture.md` §3: alasan wajib pada ketiga tindakan | **Terpenuhi** | Tombol konfirmasi nonaktif tanpa alasan; kiriman selalu membawa `reasonCode` (`R4`–`R6`) |
| Keputusan `G1`, `G2`, `G5` | **Terpenuhi** | `R6` (tanpa jalan pintas `Reallocated`), `R1` (penyebab), `R4`/`R5` (dialog bahaya permanen) |
| Definition of Done | `NOT APPLICABLE` | Kartu tidak memuat baris DoD |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Token sesi `superadmin` backend lokal yang diberikan pemilik untuk runtime `FE-BD-009` dipakai ulang untuk run ini selama masih berlaku (kedaluwarsa 24 September 2026 10:49 UTC), atas permintaan validasi runtime pada task ini. Token disimpan sementara di scratchpad, tidak pernah dicetak ke log, dan berkasnya sudah dihapus. Disarankan logout atau rotasi sesi `superadmin` lokal |
| Masalah yang diketahui | **`G1` — backlog kontrak/backend:** kantong **Dialihkan** tidak punya jalan keluar ke pemberian (`BE-BD-009` §10.2, `BE-BD-010`). Terbukti ulang di `R6`: sesudah dialihkan, backend hanya menawarkan `MoveStorageLocation`. Layar mengikuti apa adanya, tanpa jalan pintas. **Temuan baru — riwayat kantong berlebih:** `TEST-BD009-20260917100009-07` berstatus Menunggu keputusan, tetapi transisi terakhirnya `Received → Stored`, sehingga tidak ada `ReasonNote` ke `PendingReview` untuk dibaca. Layar hanya menampilkan baris "berlebih" (dari `IsExcess`) dan tidak menebak. Diusulkan sebagai backlog backend: transisi penyimpanan kantong berlebih mencatat status tujuan sebenarnya. **Dokumen:** register menandai `DEC-BD-043`/`045` `draft` walaupun kontraknya disetujui |
| Dependency backend | Nihil yang menahan task. Dua backlog di atas tidak dibuka sebagai task |
| Perubahan sampingan | Dua berkas tracked `test-results/` yang diubah Playwright dipulihkan dengan `git restore` pada kedua path itu saja |
| Interupsi | `NONE` di luar kegagalan spec run 1 (bagian 6.2) |
| Status Git | Frontend: ` M blood-unit-detail-view.jsx`, ` M blood-unit-constants.jsx`, ` M use-blood-unit-detail.jsx`, ` M blood-unit.service.js`, ` M blood-unit-utils.js`; baru `blood-unit-resolution-dialogs.jsx`, `use-blood-unit-resolution.jsx`, `tests/e2e/blood-unit-resolution-screen.spec.mjs`, `tests/unit/blood-unit-resolution.test.mjs`. Belum di-stage atau di-commit. Backend: hanya laporan ini dan tautan bukti pada roadmap serta `requirement-traceability.md` |
| Langkah berikutnya | Task frontend terakhir: `FE-BD-008` (koreksi dua langkah dan tunggakan bukti darurat). Pemilik kontrak perlu memutuskan jalan keluar status `Reallocated` sebelum tombol **Alihkan** dipakai di produksi |
