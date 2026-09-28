# Laporan Perubahan Frontend — `FE-BD-009`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BD-009` |
| Judul | Penyelesaian konflik di dalam layar pemeriksaan |
| Slice | Slice 3 — Kantong darah, pemberian, dan penyelesaiannya |
| Roadmap | `docs/module-blueprints/bank-darah/roadmap/frontend-roadmap.md` §3 kartu `FE-BD-009` |
| Trace | `DEC-BD-026`, `DEC-BD-031`, `DEC-BD-033`, `DEC-BD-039` · `03-frontend-architecture.md` §1, §3 `FE-BD-06`, §5, §6 · kewajiban layar `FE-BD-007`, `FE-BD-009`, `FE-BD-019` · `AC-BD-036`, `AC-BD-051`, `AC-BD-053`, `AC-BD-054`, `AC-BD-079`, `AC-BD-080` (dibuktikan backend `BE-BD-011`) |
| Contract version | Kartu merujuk api-contract `v4`; kontrak berlaku `v5` (disetujui `Sukmagp` 19 September 2026). Endpoint `blood-group-exams` tidak berubah di antara keduanya — dibaca langsung dari source backend `1f6f8222` |
| Wewenang UI | Kerangka minimal layar `FE-BD-06`: butir menu, route, daftar, dan detail. Tindakan hanya **Validasi hasil** dan **Penyelesaian konflik**. Pencatatan sampel dan hasil **tidak** termasuk. Rupa layar `DEV_DISCRETION` mengikuti modul Kantong Darah |
| Dependency | `BE-BD-011` ✅ (endpoint `POST /conflict-resolution`), `BE-BD-005` ✅ (pemeriksaan golongan darah), `FE-BD-006` ✅ (pembaca hak akses `useEffectivePermissions`) |
| Klasifikasi | `MEDIUM` — satu layar baru (daftar + detail), dua tindakan tulis, dua butir hak akses terpisah, satu sumber tampilan lintas modul (nama pasien) |
| Task mode | `FRONTEND` |
| Target tulis | `V2QuilvianSystemFrontendDev` cabang `sukmagpV2`; laporan dan tautan bukti pada `docs/module-blueprints/bank-darah/` di repository backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `88bd2b1c7a8ce8e03889c0352f66d048c82cc1f0` (perubahan task ini belum di-commit) |
| Commit backend yang dijadikan rujukan | `1f6f8222995413e13dd64b4021e304ac6604aabd` cabang `sukmagp` |
| Tanggal | 24 September 2026 |
| Status | ✅ **Selesai.** Kedua acceptance kartu dan DoD terbukti. `lint:errors` `PASS`, `test:unit` 1605 test (1598 lulus; 10 test baru lulus; 7 kegagalan lama sama dengan baseline), `build` `PASS` (371 halaman), runtime **11 dari 11 `PASS`** di Chromium terhadap backend sungguhan |

---

## 1. Keadaan yang ditemukan di awal

- **Layar `FE-BD-06` belum ada sama sekali.** Nol route, nol view, nol hook, dan nol butir menu
  "Pemeriksaan Golongan Darah". `FE-BD-005` sengaja tidak membangunnya atas keputusan pemilik
  24 September 2026. Satu-satunya pemakaian `blood-group-exams` di frontend adalah pembacaan
  golongan darah sah pada detail order darah (`use-blood-order-detail.jsx`).
- **Backend sudah lengkap.** `BbkBloodGroupExamController` memuat sembilan endpoint, termasuk
  `POST /{id}/validate` (butir `BloodGroupExam : Validate`) dan `POST /conflict-resolution` (butir
  `BloodGroupExam : ResolveConflict`). Detail pemeriksaan membawa `AvailableActions`: `RecordResult`
  pada `SampleTaken`, `Validate` pada `ResultRecorded`, dan `ResolveConflict` pada pemeriksaan yang
  `IsConflictHeld`.
- **Tiga celah kontrak ditemukan saat audit**, lalu diputuskan pemilik (bagian 1.1):
  1. Belum ada kategori alasan untuk penyelesaian konflik. Kesepuluh kategori yang ada menyangkut
     order dan kantong. Backend menerima alasan aktif apa pun.
  2. Backend tidak menyediakan daftar kandidat pemeriksaan ulang.
  3. DTO pemeriksaan hanya membawa `PatientId`, tanpa nama pasien. Resource `patients` yang ada
     (`GET /patients/options`) tidak dapat dicari berdasarkan ID, dan endpoint pasien dijaga policy
     `KioskRead` yang hanya meloloskan role SuperAdmin, Administrator, dan Kiosk.
- **Judul permintaan berbeda dari roadmap.** Permintaan berjudul "Penyelesaian Kantong Darah",
  sedangkan kartu `FE-BD-009` adalah penyelesaian **konflik golongan darah**. Penyelesaian kantong
  `PendingReview` adalah `FE-BD-007`. Pemilik mengonfirmasi cakupan kartu (keputusan `B0`).

### 1.1 Keputusan pemilik `Sukmagp`, 24 September 2026

| No | Keputusan |
| --- | --- |
| `B0` | `FE-BD-009` = penyelesaian konflik golongan darah pada `FE-BD-06` |
| `B1` | `FE-BD-009` membangun kerangka minimal `FE-BD-06`: menu, route, daftar, dan detail |
| `B2` | Cakupan: validasi hasil dan penyelesaian konflik. Pencatatan sampel dan hasil **tidak** termasuk. Memakai API backend yang sudah ada |
| `B3` | Pakai seluruh alasan aktif `BloodBankReason`. Kategori alasan konflik dicatat sebagai backlog; tidak membuka task backend |
| `B4` | Frontend tidak menentukan pemeriksaan ulang. Kelayakan dibaca dari `AvailableActions`, `IsConflictHeld`, dan golongan darah sah milik backend |
| `B5` | Nama pasien hanya untuk tampilan, tanpa mengubah DTO backend. **Lanjutan B5:** karena resource `patients` tidak dapat mencari berdasarkan ID, pemilik memilih `GET /patients/{id}` — endpoint master pasien yang sudah ada — sekali per pasien, dengan tanda hubung bila ditolak |
| Runtime | Pasien uji **IKBAL YULIYANTO** (`00-00-00-15`, `6c84fab5-c1d0-4714-a126-45aee8351369`) boleh ditulisi data uji permanen, karena tidak ada pasien berlabel uji dan pasien ini tidak punya order darah |

---

## 2. Proses bisnis dari sisi pengguna

**Contoh yang dipakai sepanjang bagian ini.** Pasien IKBAL punya hasil sah **O Positif**. Sampel
kedua diperiksa dan hasilnya **A Positif**, lalu divalidasi. Sejak saat itu kedua hasil ditahan dan
pasien tidak punya golongan darah sah.

### 2.1 Alur normal

| No | Langkah | Pelaku | Yang terlihat di layar |
| ---: | --- | --- | --- |
| 1 | Buka **Bank Darah → Pemeriksaan Golongan Darah** | Petugas BDRS atau validator klinis | Kepala halaman, lima angka ringkasan (termasuk **Konflik Tertahan** dan **Pasien Tanpa Golongan Sah**), penyaring, dan tabel pemeriksaan |
| 2 | Tekan **Konflik Tertahan** | Validator klinis | Tabel hanya memuat pemeriksaan yang sedang menahan perbedaan. Menekannya lagi mematikan saringan |
| 3 | Klik dua kali baris A Positif | Validator klinis | Detail pemeriksaan. Di bawah kepala halaman ada peringatan **Golongan darah pasien bertentangan** beserta pesan backend. Bagian **Riwayat Hasil Pasien** memuat seluruh pemeriksaan pasien, dan kedua hasil yang bentrok bertanda **Tertahan konflik** |
| 4 | Petugas mencatat hasil pemeriksaan ulang B Negatif (di luar layar ini), lalu membuka detail pemeriksaan ulang itu dan menekan **Validasi** | Petugas berwenang validasi | Dialog konfirmasi. Sesudah divalidasi, pesan backend tampil: "Hasil pemeriksaan ulang berhasil divalidasi. Perbedaan hasil … masih tertahan sampai validator klinis menyatakan hasil yang berlaku." |
| 5 | Kembali ke pemeriksaan yang tertahan, tekan **Selesaikan Konflik** | Validator klinis | Dialog berisi pemilih **Pemeriksaan Ulang** (seluruh pemeriksaan pasien, masing-masing dengan hasil, status, sampel, dan penanda tertahan) dan pemilih **Alasan Penyelesaian** (seluruh alasan aktif) |
| 6 | Pilih pemeriksaan B Negatif dan satu alasan, tekan **Selesaikan Konflik** | Validator klinis | Dialog tertutup, muncul **Konflik diselesaikan** dengan pesan backend, dan kepala halaman berubah menjadi **Golongan darah sah: B Negatif**. O Positif dan A Positif tetap terbaca di riwayat |

Sistem tidak pernah memilih hasil sendiri. Validator yang menunjuk pemeriksaan ulang, dan backend
yang menilai apakah pilihannya sah.

### 2.2 Jalur tidak normal

| Keadaan | Yang terjadi |
| --- | --- |
| Validator menunjuk salah satu hasil yang sedang bentrok, misalnya O Positif | Backend menolak `422`. Pesannya tampil apa adanya **di dalam dialog**: "Perbedaan hasil hanya dapat diselesaikan setelah ada pemeriksaan ulang yang tervalidasi." Dialog tetap terbuka supaya validator dapat menunjuk pemeriksaan lain; bacaan dimuat ulang. Tidak ada data yang berubah |
| Pasien sudah tidak menahan konflik, misalnya diselesaikan validator lain | Backend menolak `422` "Pasien ini sedang tidak menahan perbedaan hasil golongan darah." dan bacaan dimuat ulang |
| Validasi yang justru melahirkan konflik | Tetap berhasil. Pemberitahuan berwarna peringatan **Hasil divalidasi, konflik tertahan** dengan pesan backend. Warna dipilih dari penanda `IsConflictHeld` pada jawaban backend |
| Pemeriksaan baru sampel, hasil belum dicatat | Keterangan **Hasil belum dicatat — Pencatatan hasil pemeriksaan belum tersedia di layar ini.** Nol tombol tindakan (keputusan `B2`) |
| Petugas hanya berwenang validasi | Tombol **Validasi** tampil; **Selesaikan Konflik** tidak (`FE-BD-019`) |
| Validator klinis tanpa hak validasi | **Selesaikan Konflik** tampil; **Validasi** tidak |
| Validator tanpa `BloodBankReason : Read` | **Selesaikan Konflik** disembunyikan, karena pemilih alasannya tidak akan pernah dapat diisi |
| Detail pasien ditolak (`403`), misalnya akun non-admin | Nama pasien tampil sebagai tanda hubung; seluruh layar lain tetap berjalan |
| Golongan darah sah gagal dibaca | "Hasil pemeriksaan tidak dapat ditampilkan." (`03-frontend-architecture.md` §2) |
| Menekan tombol tindakan dua kali | Tombol nonaktif selama proses, dan kunci pengiriman ganda menolak kiriman kedua |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Blueprint: kartu `FE-BD-009` dan `FE-BD-005`, register dan slice pada roadmap frontend,
  `03-frontend-architecture.md` §1/§2/§3 `FE-BD-06`/§5/§6, laporan `BE-BD-011`, `BE-BD-005`,
  `FE-BD-005`, `requirement-traceability.md`.
- Backend (baca saja): `BbkBloodGroupExamController.cs`, `BloodGroupExamDtos.cs`,
  `BbkBloodGroupExamService.cs` (`GetPagedAsync`, `GetValidBloodGroupAsync`, `ValidateAsync`,
  `ResolveConflictAsync`, `ApplyValidationOutcome`, `AvailableActionsOf`, `BuildFilterMetadata`),
  `BbkBloodGroupExamStatus.cs`, `BloodType.cs`, `BloodBankReasonController.cs` +
  `BloodBankReasonService.GetOptionsAsync`, `MstBloodBankReason.cs` (`BloodBankReasonCategories`),
  `PatientController.cs` (`options`, `{id}`, `ApplyStandardFilter`), `Program.cs` (policy `KioskRead`).
- Frontend: modul referensi **Kantong Darah** — `blood-units` route, `blood-unit-list-view.jsx`,
  `blood-unit-table-columns.jsx`, `blood-unit-detail-view.jsx`, `blood-unit-issuance-dialogs.jsx`,
  `use-blood-unit-list.jsx`, `use-blood-unit-detail.jsx`, `blood-unit.service.js`,
  `blood-unit-constants.jsx`, `blood-unit-utils.js`, `blood-order-utils.js`, `blood-order.service.js`;
  base component `confirm-modal`, `filter-select`, `information-alert`, `status-badge`,
  `base-detail-view`; `use-permission.jsx`; `use-select-resource.jsx` beserta registry
  `patients`; `menu-items.jsx`; spec `blood-unit-issuance-screen.spec.mjs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/health-services/blood-bank-management/blood-group-exams/page.jsx` | **Baru.** Route tipis daftar, metadata "Pemeriksaan Golongan Darah" |
| `src/app/health-services/blood-bank-management/blood-group-exams/[slug]/page.jsx` | **Baru.** Route tipis detail |
| `src/app/health-services/blood-bank-management/blood-group-exams/[slug]/route-token.js` | **Baru.** Resolusi token route privat, pola `blood-units` |
| `src/lib/constants/health-services/blood-bank-management/blood-group-exam-constants.jsx` | **Baru.** Endpoint, route, status, saringan, badge, nama aksi `AvailableActions`, dan salinan teks |
| `src/lib/services/health-services/blood-bank-management/blood-group-exam.service.js` | **Baru.** Daftar, ringkasan, metadata, detail, pemeriksaan per pasien, validasi, penyelesaian konflik (tepat tiga isian), seluruh alasan aktif, dan detail pasien untuk tampilan |
| `src/utils/health-services/blood-bank-management/blood-group-exam-utils.js` | **Baru.** Normalisasi opsi status, golongan darah sah, pilihan pemeriksaan ulang (tanpa saringan), nama tampilan pasien, dan hasil tindakan |
| `src/lib/hooks/health-services/blood-bank-management/use-blood-group-exam-list.jsx` | **Baru.** Daftar, saringan, preset Konflik Tertahan, dan navigasi detail |
| `src/lib/hooks/health-services/blood-bank-management/use-blood-group-exam-detail.jsx` | **Baru.** Detail, golongan darah sah, riwayat pasien, dua gerbang hak akses, dialog validasi dan penyelesaian konflik |
| `src/lib/hooks/health-services/blood-bank-management/use-blood-group-exam-patient-names.jsx` | **Baru.** Nama tampilan pasien sekali per `PatientId`, tanda hubung bila ditolak |
| `src/components/view/health-services/blood-bank-management/blood-group-exams/blood-group-exam-list-view.jsx` | **Baru.** Layar daftar |
| `src/components/view/health-services/blood-bank-management/blood-group-exams/blood-group-exam-table-columns.jsx` | **Baru.** Kolom daftar, badge status, dan penanda keadaan hasil |
| `src/components/view/health-services/blood-bank-management/blood-group-exams/detail/blood-group-exam-detail-view.jsx` | **Baru.** Layar detail |
| `src/components/view/health-services/blood-bank-management/blood-group-exams/detail/blood-group-exam-dialogs.jsx` | **Baru.** Dialog validasi dan penyelesaian konflik |
| `src/style/health-services/blood-bank-management/blood-group-exams/blood-group-exam.module.css` | **Baru.** Isi dialog saja, seluruhnya token |
| `src/utils/menu-sidebar/menu-items.jsx` | **Diubah.** Butir **Pemeriksaan Golongan Darah** di bawah **Kantong Darah**, berpenjaga `BloodGroupExam : Read` (urutan `03-frontend-architecture.md` §2) |
| `tests/unit/blood-group-exam-conflict.test.mjs` | **Baru.** 10 test utility murni |
| `tests/e2e/blood-group-exam-conflict-screen.spec.mjs` | **Baru.** 11 skenario runtime terhadap backend sungguhan |

### 3.3 Kepatuhan arsitektur frontend

- Alur dependensi `app → view → hook → service → InstanceAxios` dipatuhi. View tidak memanggil
  Axios; route hanya entry point dan metadata.
- Pola service tanpa Redux, sama dengan seluruh layar Bank Darah (`blood-unit.service.js`). Tidak
  ada slice baru.
- Dipakai ulang tanpa diubah: `getValidBloodGroup` (`blood-order.service.js`), `buildReasonOptions`
  (`blood-unit-utils.js`), `normalizeApiData`, `normalizePagedResult`, `readApiFailure`, `safeText`,
  `formatDateTimeId` (`blood-order-utils.js`), `useEffectivePermissions`, dan utilitas token route
  privat. `getBloodBankReasonOptions` **tidak** diubah: penjaga "tanpa kategori = kosong" miliknya
  sengaja dipertahankan untuk pemakai lain. Alasan tanpa kategori memakai fungsi baru di service
  task ini.
- **Nol logika bisnis frontend.** Kelayakan dari `AvailableActions`, izin dari `decide()`, penanda
  konflik dan hasil sah dari backend, penolakan dari backend. Pilihan pemeriksaan ulang **tidak**
  disaring (keputusan `B4`); test unit menjaga agar pihak konflik dan sampel tanpa hasil tetap
  ditawarkan.
- Pemilih di dalam dialog **tidak** dibungkus `<label>` (perbaikan temuan `FE-BD-005`). Runtime
  membuktikan daftar tertutup sendiri sesudah dipilih.

### 3.4 Gerbang keputusan base component

`UI GATE: 14 elemen — REUSE 9, EXTEND 0, COMPOSE 5, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Kepala halaman daftar | `Hero` | `base-features/hero.jsx`, dipakai `blood-unit-list-view` | REUSE | Tanpa aksi hero |
| Ringkasan | `SummaryGrid` | dipakai `blood-unit-list-view` | REUSE | Angka dari `GET /summary` apa adanya |
| Penyaring, pencarian, preset | `DataFilter`, `FilterSelect`, `BaseButton` | pola `blood-unit-list-view` | REUSE | Preset **Konflik Tertahan** sebagai `actions` |
| Tabel daftar | `DataTable` | kolom di `*-table-columns.jsx` | REUSE | — |
| Badge status dan keadaan hasil | `StatusBadge` | varian `info`/`pending`/`active`/`warning` tersedia | REUSE | — |
| Galat dan akses ditolak | `InformationAlert`, `AccessDeniedGate` | pola modul referensi | REUSE | — |
| Kerangka detail | `BaseDetailView` | dipakai `blood-unit-detail-view` | REUSE | `detailRows` + `afterHero` |
| Tombol tindakan | `BaseButton` | varian `primary` dan `warning` tersedia | REUSE | Selesaikan Konflik bervarian peringatan |
| Pemberitahuan | `ToastStack` lewat `BaseDetailView.toasts` | pola modul referensi | REUSE | — |
| Penanda golongan darah sah/konflik | `InformationAlert` | varian `success`/`warning`/`info`/`danger` | COMPOSE | Dirangkai di view dari data `/valid` |
| Bagian Sampel | `BaseDetailSection` + `DataTable` | pola `FE-BD-004`/`005` | COMPOSE | Sama dengan bagian riwayat kantong |
| Bagian Riwayat Hasil Pasien | `BaseDetailSection` + `DataTable` | pola `FE-BD-004`/`005` | COMPOSE | — |
| Dialog Validasi | `ConfirmModal` | pola dialog Berikan `FE-BD-005` | COMPOSE | — |
| Dialog Selesaikan Konflik | `ConfirmModal` + `FilterSelect` + `InformationAlert` | pola dialog pembatalan alokasi `FE-BD-004` | COMPOSE | Varian `warning` |

Kelima baris `COMPOSE` merujuk keputusan komposisi yang sama pada `FE-BD-004` dan `FE-BD-005` di
modul yang sama, sehingga tidak disajikan ulang sebagai pilihan baru. Nol elemen `NEW` atau
`EXTEND`; nol base component diubah.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Kerangka tabel "Memuat pemeriksaan golongan darah...", kerangka ringkasan, "Memuat golongan darah sah pasien...", nama pasien "Memuat pasien..." |
| Kosong | "Belum ada pemeriksaan" — "Belum ada pemeriksaan golongan darah yang sesuai dengan filter ini." beserta tombol atur ulang filter. Riwayat pasien kosong: "Belum ada riwayat" |
| Gagal | "Daftar pemeriksaan golongan darah gagal dimuat." atau pesan backend; detail: pesan backend atau "Detail pemeriksaan golongan darah gagal dimuat." dengan tombol **Muat ulang**; golongan darah sah: "Hasil pemeriksaan tidak dapat ditampilkan." |
| Tanpa hak akses | `AccessDeniedGate` menampilkan akses ditolak bila backend menolak `403`; butir menu tersembunyi tanpa `BloodGroupExam : Read`; tombol tindakan tersembunyi tanpa butirnya masing-masing |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Blood Bank Management / Blood Group Exam

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/blood-bank-management/blood-group-exams/filters/metadata` | Opsi saringan status | `BloodGroupExam : Read` |
| `GET` | `/v1/health-services/blood-bank-management/blood-group-exams/summary` | Lima angka ringkasan | `BloodGroupExam : Read` |
| `GET` | `/v1/health-services/blood-bank-management/blood-group-exams` | Daftar (`search`, `examStatus`, `isConflictHeld`, halaman) dan riwayat per pasien (`patientId`, `pageSize=100`) | `BloodGroupExam : Read` |
| `GET` | `/v1/health-services/blood-bank-management/blood-group-exams/{id}` | Detail beserta `AvailableActions` | `BloodGroupExam : Read` |
| `GET` | `/v1/health-services/blood-bank-management/blood-group-exams/patient/{patientId}/valid` | Golongan darah sah dan penanda konflik | `BloodGroupExam : Read` |
| `POST` | `/v1/health-services/blood-bank-management/blood-group-exams/{id}/validate` | Validasi hasil rutin, tanpa isi | `BloodGroupExam : Validate` |
| `POST` | `/v1/health-services/blood-bank-management/blood-group-exams/conflict-resolution` | Penyelesaian konflik; isi tepat `patientId`, `resolvingExamId`, `reasonCode` | `BloodGroupExam : ResolveConflict` |

#### Health Services / Master Data / Blood Bank Reason

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/master-data/blood-bank-reasons/options` | Seluruh alasan aktif, **tanpa** `category` (keputusan `B3`) | `BloodBankReason : Read` |

#### Patient Management / Master Data / Patient

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/patient-management/master-data/patients/{id}` | Nama dan nomor rekam medis, **hanya tampilan** (keputusan `B5`) | Policy `KioskRead` — SuperAdmin, Administrator, Kiosk |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Kode keluar `0`, nol error | `PASS` | Keluaran perintah |
| `npx eslint` pada berkas task | 0 error, 4 warning `react-hooks/set-state-in-effect` pada `setState(loading)` di awal efek muat | `EXISTING WARNING` (pola) | Pola identik pada hook referensi `use-blood-unit-list.jsx`/`use-blood-unit-detail.jsx` (3 warning yang sama). Satu warning `exhaustive-deps` di view detail diperbaiki dengan `useMemo` |
| `npm run test:unit` | 1605 test — **1598 lulus, 7 gagal**; ke-10 test baru lulus | `PASS` untuk cakupan task | Ketujuh kegagalan sama persis dengan baseline `FE-BD-005`: `route, menu, dan store terdaftar`, empat `FE-RWI-042`, `FE-RWI-043`, `M0` menu Setup Bank Darah. Nol di berkas task ini |
| `npm run build` | Kode keluar `0`, `Compiled successfully in 38.3s`, **371 halaman** (sebelumnya 370), standalone siap | `PASS` | `/blood-group-exams` (○) dan `/blood-group-exams/[slug]` (ƒ) terdaftar |
| Grep anti-regresi UI | Nol warna literal, nol `<button>` mentah, nol `<table>`, nol utility typography Bootstrap, nol `!important`, nol inline style | `PASS` | Grep 2 menemukan `line-height`/`font-size`/`font-weight` pada `.modalContent p` dan `.formField > label`: **dipertahankan**, karena menyasar paragraf dan label lokal dialog (bukan komponen shared) dan seluruhnya memakai token — pola yang sama dengan `blood-unit.module.css` |
| Runtime `R0`–`R8` | **11 dari 11 `PASS`** pada run pertama, 50,7 detik | `PASS` | Bagian 6.1–6.2 |

`AUTOMATED TEST: npm run test:unit — PASS` (10 test baru lulus; 7 kegagalan lain sudah ada sebelumnya).

Uji manual: `PASS` — seluruh kontrol interaktif dijalankan di peramban sungguhan lewat Playwright
(bagian 6.2): preset, saringan status, atur ulang, klik dua kali, kedua dialog, kedua pemilih
dialog, konfirmasi, dan penolakan backend.

### 6.1 Skenario runtime

Spec `tests/e2e/blood-group-exam-conflict-screen.spec.mjs` mengikuti pola `FE-BD-005`. Hasil build
standalone dijalankan pada `http://127.0.0.1:3710` dan diuji lewat Playwright Chromium. Setiap
request `/v1/**` diteruskan ke **backend sungguhan** milik pemilik: `QuilvianSystemBackend` Debug
di `https://localhost:7184`, database `QuilvianNewDevSukma`, sesi `superadmin`. **Jawaban bisnis
backend tidak pernah dikarang.** Hanya dua jawaban yang dipasang: daftar kewenangan pada `R1` dan
`R4` (`isSuperAdmin: false`), supaya ketiadaan satu butir dapat dibuktikan tanpa membuat akun
baru, dan satu `403` pada detail pasien di `R8`.

**Fixture (`R0`).** Dibuat lewat API sungguhan pada pasien IKBAL YULIYANTO:
`TEST-FE009-20260924095226-1` O Positif divalidasi (`5a55ac28-3bb8-4098-bdc8-81e7884403fc`),
`-2` A Positif divalidasi sehingga konflik lahir (`3ef1e884-57ad-40a5-8f78-30f79f6d8e69`),
`-3` B Negatif dicatat tanpa divalidasi (`138ec89f-0e95-49ce-8fe3-3954bf8613ab`), dan `-4` sampel
tanpa hasil (`a76dc73f-f5fc-400e-9d16-b47858c0180c`). Alasan: `TBD006-BATALALOK`, alasan aktif
pertama dari daftar. Kategorinya pembatalan alokasi — contoh nyata backlog kategori alasan `B3`.

### 6.2 Hasil validasi runtime — 24 September 2026

| Kode | Skenario | Hasil |
| --- | --- | --- |
| `R0` | Fixture konflik dibuat lewat backend; `/valid` menyatakan `isConflictHeld` = `true` dan `isUsableForClinicalDecision` = `false` | `PASS` |
| `R1` | Butir menu **Pemeriksaan Golongan Darah** terdaftar dan menuju `/blood-group-exams`; dengan kewenangan tanpa `BloodGroupExam : Read`, butir itu tidak ada di sidebar | `PASS` |
| `R2` | **Konflik Tertahan** mengirim `isConflictHeld=true`, dan hanya pemeriksaan bentrok yang tampil; ditekan lagi, parameter hilang. Saringan status "Hasil tercatat" mengirim `examStatus=1`; **atur ulang** menghapusnya. Klik dua kali membuka detail | `PASS` |
| `R3` | Detail pihak konflik: **Golongan darah pasien bertentangan** dengan pesan backend apa adanya; **Selesaikan Konflik** tampil, **Validasi** tidak; `GET /patients/{id}` `200`; riwayat memuat kedua hasil bertanda **Tertahan konflik** | `PASS` |
| `R4a` | `FE-BD-019`: `Read` + `Validate` + `BloodBankReason : Read` → pemeriksaan tertahan tanpa **Selesaikan Konflik**; pemeriksaan hasil tercatat menampilkan **Validasi** | `PASS` |
| `R4b` | `Read` + `ResolveConflict` + `BloodBankReason : Read` → **Selesaikan Konflik** tampil; **Validasi** tidak tampil | `PASS` |
| `R4c` | `Read` + `ResolveConflict` + `Validate` tanpa `BloodBankReason : Read` → **Selesaikan Konflik** tersembunyi; **Validasi** tampil | `PASS` |
| `R5` | **Validasi** pada B Negatif → `POST /validate` `200` **tanpa isi**; pesan backend "…masih tertahan sampai validator klinis menyatakan hasil yang berlaku." tampil; tombol Validasi hilang; backend tetap `isConflictHeld` = `true` | `PASS` |
| `R6` | Dialog: `GET /blood-bank-reasons/options` **tanpa** `category`; pemilih Pemeriksaan Ulang memuat opsi **sebanyak `totalData` backend** untuk pasien itu (tanpa saringan). Menunjuk O Positif → **`422`**, pesan tampil di dialog, dialog tetap terbuka, backend tetap berkonflik. Menunjuk B Negatif → **`200`**; isi permintaan **tepat** `patientId`, `reasonCode`, `resolvingExamId` dengan nilai yang dipilih. Dialog tertutup, **Konflik diselesaikan**, kepala **Golongan darah sah: B Negatif**, Selesaikan Konflik hilang. Backend: `sourceExamId` = pemeriksaan B Negatif, `isUsableForClinicalDecision` = `true`. O Positif dan A Positif tetap terbaca | `PASS` |
| `R7` | Sampel tanpa hasil: **Hasil belum dicatat** tampil; nol tombol Validasi/Selesaikan Konflik; nol `POST` | `PASS` |
| `R8` | Detail pasien dijawab `403`: halaman tetap berjalan, golongan darah sah tetap tampil, nama pasien tidak macet di "Memuat pasien..." | `PASS` |

**Keadaan database sesudah run** (diperiksa langsung ke backend):

| Pemeriksaan | Hasil | Keadaan akhir |
| --- | --- | --- |
| `TEST-FE009-20260924095226-1` | O Positif | Tervalidasi, bukan hasil sah, tidak tertahan |
| `TEST-FE009-20260924095226-2` | A Positif | Tervalidasi, bukan hasil sah, tidak tertahan |
| `TEST-FE009-20260924095226-3` | B Negatif | Tervalidasi, **hasil sah** pasien |
| `TEST-FE009-20260924095226-4` | — | Sampel diambil |

Satu catatan penyelesaian konflik tersimpan (append-only). Data ini **dibiarkan** sesuai keputusan
pemilik. **Satu panggilan tambahan sesudah run:** saat memeriksa keadaan akhir, agent mengirim satu
`POST /conflict-resolution` langsung ke backend. Backend menolaknya `422` "Pasien ini sedang tidak
menahan perbedaan hasil golongan darah." — **nol data berubah**.

**Percobaan sebelum run:** token sesi pertama yang diberikan sudah kedaluwarsa (`401 invalid_token`),
dan token kedua ternyata token yang sama. Nol request lolos dan nol data tertulis sampai token baru
diberikan.

**Kebersihan sesudah run:** server standalone uji dihentikan; berkas cookie dan berkas bantu di
scratchpad dihapus; nol kemunculan token di log run, `test-results/`, dan `playwright-report/`; dua
berkas tracked di `test-results/` yang diubah Playwright (`.last-run.json` dan satu
`error-context.md` milik spec rawat inap) dipulihkan dengan `git restore` pada kedua path itu saja.

**Tidak dijalankan:** `test:uat` (tidak diminta). Pembuktian penolakan `403` **oleh backend** untuk
akun non-SuperAdmin (`AC-BD-037`/`AC-BD-078`) tetap di luar task ini: `R4` membuktikan gerbang
**layar**, bukan penegakan backend, karena sesi uji adalah SuperAdmin.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Outcome: validator klinis menyelesaikan konflik golongan darah **di dalam layar pemeriksaan**, bukan lewat daftar kerja tersendiri | **Terpenuhi** | Tindakan Selesaikan Konflik hanya ada di detail `FE-BD-06` (`R3`, `R6`). Preset **Konflik Tertahan** adalah saringan `isConflictHeld` pada daftar layar yang sama — bukan route, menu, atau daftar kerja terpisah |
| Penyelesaian konflik hidup di layar pemeriksaan, bukan daftar kerja keempat (kewajiban layar `FE-BD-009`) | **Terpenuhi** | Sama dengan di atas |
| `FE-BD-019`: tombol Validasi (`BloodGroupExam : Validate`) dan tindakan Penyelesaian konflik (`BloodGroupExam : ResolveConflict`) tampil **terpisah** menurut hak akses; petugas berwenang validasi melihat Validasi tetapi tidak melihat Penyelesaian konflik | **Terpenuhi** | Dua pemeriksaan `decide()` terpisah di `use-blood-group-exam-detail.jsx`; `R4a`, `R4b`, `R4c` |
| Aturan pemilik: `FE-BD-019` dipegang task terakhir di antara `FE-BD-005` dan task ini yang menyentuh `FE-BD-06` | **Terpenuhi** | `FE-BD-005` tidak menyentuh `FE-BD-06`; task ini yang membangunnya dan memegang kewajibannya. Tidak ada perpindahan kewajiban ke `FE-BD-005` |
| Catatan hak akses: pakai ulang pembaca hak akses `FE-BD-006` | **Terpenuhi** | `useEffectivePermissions` — keputusan ketat, hanya `allowed` yang membuka tombol |
| DoD: **bukan** daftar kerja keempat | **Terpenuhi** | Sama dengan baris pertama |

Kewajiban layar `FE-BD-007` (penanda konflik terlihat) **terpenuhi pada layar ini** (`R3`). Bagian
"menahan tombol pemberian/alokasi" tetap milik layar kantong `FE-BD-005`, dan backlog gerbang
golongan darah `VAL-BD-034` pada kantong tidak berubah oleh task ini.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Token sesi `superadmin` backend lokal ditempel pemilik di percakapan agent. Token itu disimpan sementara di scratchpad, tidak pernah dicetak ke log, dan berkasnya sudah dihapus. Token tetap berlaku sampai kedaluwarsa, jadi disarankan logout atau rotasi sesi `superadmin` lokal |
| Masalah yang diketahui | **Nama pasien terbatas role.** `GET /patients/{id}` dijaga `KioskRead`, sehingga petugas BDRS non-admin melihat tanda hubung (`R8`). Layar tetap berfungsi. **Nama pelaku** pengambil sampel dan validator hanya GUID dari backend, jadi tidak ditampilkan (pola `D1` `FE-BD-012`). **Catatan penyelesaian konflik** (validator, alasan, waktu) tidak punya endpoint baca, sehingga tidak ditampilkan; riwayat hasil tetap terbaca |
| Dependency backend | Nihil yang menahan task. **BACKLOG (keputusan pemilik `B3`, tidak dibuka sebagai task):** kategori alasan khusus penyelesaian konflik golongan darah — contoh nyata di runtime: alasan pertama yang ditawarkan berkategori pembatalan alokasi. **Backlog usulan:** `PatientName`/`MedicalRecordNumber` pada DTO pemeriksaan (menghapus ketergantungan `KioskRead`), endpoint baca catatan penyelesaian konflik, dan aturan "pemeriksaan ulang harus lebih baru dari konflik" (backend kini menerima pemeriksaan tervalidasi lama yang bukan pihak konflik) |
| Perubahan sampingan | Dua berkas tracked `test-results/` yang diubah Playwright dipulihkan dengan `git restore` pada kedua path itu saja |
| Interupsi | Token sesi kedaluwarsa dua kali sebelum run; dipulihkan dengan token baru dari pemilik. Nol data tertulis selama interupsi |
| Status Git | Frontend: ` M src/utils/menu-sidebar/menu-items.jsx`, ditambah berkas baru `src/app/health-services/blood-bank-management/blood-group-exams/`, `src/components/view/health-services/blood-bank-management/blood-group-exams/`, `blood-group-exam-constants.jsx`, `use-blood-group-exam-detail.jsx`, `use-blood-group-exam-list.jsx`, `use-blood-group-exam-patient-names.jsx`, `blood-group-exam.service.js`, `src/style/health-services/blood-bank-management/blood-group-exams/`, `blood-group-exam-utils.js`, `tests/e2e/blood-group-exam-conflict-screen.spec.mjs`, `tests/unit/blood-group-exam-conflict.test.mjs`. Belum di-stage atau di-commit. Backend: hanya laporan ini dan tautan bukti pada roadmap serta `requirement-traceability.md` |
| Langkah berikutnya | Task frontend berikutnya: `FE-BD-007` (penyelesaian kantong `PendingReview`, tiga tombol tiga penjaga) |
