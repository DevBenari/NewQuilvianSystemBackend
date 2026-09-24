# Laporan Perubahan Frontend — `FE-BD-008`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BD-008` |
| Judul | Koreksi dua langkah dan daftar tunggakan bukti darurat |
| Slice | Slice 3 — Kantong darah, pemberian, dan penyelesaiannya |
| Roadmap | `docs/module-blueprints/bank-darah/roadmap/frontend-roadmap.md` §3 kartu `FE-BD-008` |
| Trace | `DEC-BD-023`, `DEC-BD-030`, `DEC-BD-034`, `DEC-BD-041`, `DEC-BD-051`..`054` · `INV-BD-033` · `03-frontend-architecture.md` §3 `FE-BD-04` (saringan tunggakan bukti darurat) dan `FE-BD-05` (baris "Tombol Ajukan koreksi pencatatan" dan "Daftar koreksi + tombol Setujui/Tolak") · kewajiban layar `FE-BD-004`, `FE-BD-016`, `FE-BD-017` · `AC-BD-047/048/049/050/086/087/088` (dibuktikan backend `BE-BD-010`) |
| Contract version | Kontrak berlaku `v5` (disetujui `Sukmagp` 19 September 2026). Keempat endpoint koreksi tidak berubah sejak `v4`; dibaca langsung dari source backend `74f5f9e5` |
| Wewenang UI | Layar `FE-BD-05` (detail kantong) sebatas tombol **Ajukan Koreksi**, daftar koreksi beserta tombol **Setujui/Tolak** per koreksi, dan ketiga dialognya. Layar `FE-BD-04` (daftar kantong) sebatas preset **Tunggakan Bukti Darurat**. Rupa layar `DEV_DISCRETION` mengikuti modul Kantong Darah |
| Dependency | `BE-BD-010` ✅; `FE-BD-004` ✅ (layar daftar dan detail kantong); `FE-BD-006` ✅ (`useEffectivePermissions`) |
| Klasifikasi | `MEDIUM` — tiga tindakan tulis, dua butir hak akses, satu perbandingan pelaku, satu preset daftar |
| Task mode | `FRONTEND` |
| Target tulis | `V2QuilvianSystemFrontendDev` cabang `sukmagpV2`; laporan dan tautan bukti pada `docs/module-blueprints/bank-darah/` di repository backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `de955fcd1eaccadff041b19419a3105d58f0ff72` (perubahan task ini belum di-commit) |
| Commit backend yang dijadikan rujukan | `74f5f9e5e41ff745217ea3f9d603aacf11e4dcb6` cabang `sukmagp` |
| Tanggal | 24 September 2026 |
| Status | 🟡 **Sebagian.** Source lengkap. `lint:errors` `PASS`; `test:unit` 1619 test (1612 lulus, 8 test baru lulus, 7 kegagalan lama sama dengan baseline); `build` `PASS` (371 halaman). Runtime: **6 dari 6 skenario akun A `PASS`** (`R0`, `R1`, `R2`, `R3`, `R5`, `R9`) terhadap backend sungguhan. **Belum dijalankan:** 4 skenario tahap kedua (`R4`, `R6`, `R7`, `R8`), karena akun B belum diberikan. Keputusan pemilik `E6` menuntut dua akun berbeda |

---

## 1. Keadaan yang ditemukan di awal

- Layar daftar dan detail kantong sudah ada sejak `FE-BD-004`. Nama aksi `RequestIssuanceCorrection`
  yang ditawarkan backend pada kantong `Issued` **sengaja diabaikan** layar sampai task ini.
- Backend sudah lengkap sejak `BE-BD-010`:
  - `GET /{id}/corrections` untuk membaca daftar koreksi.
  - `POST /{id}/corrections` untuk mengajukan, dijaga `BloodUnit : Correct` (`403 VAL-BD-024`).
  - `POST …/approve` dan `…/reject` untuk memutuskan, dijaga `BloodUnit : ApproveCorrection` (`403 VAL-BD-074`).
  - Pengaju yang memutuskan koreksinya sendiri ditolak `422 VAL-BD-073`.
  - Koreksi yang sudah diputuskan ditolak `422 VAL-BD-075`.
  - Penolakan tanpa alasan ditolak `422 VAL-BD-077`.
- `IssuanceCorrectionDto` hanya memulangkan `RequestedByUserId`/`DecidedByUserId` **tanpa nama**.
- Penyaring `emergencyPendingEvidence=true` sudah ada di `GET /blood-units`. Namun bukti kecocokan hanya
  dapat dicatat pada kantong `Allocated`, sedangkan kantong di daftar itu sudah `Issued`. Akibatnya
  **tidak ada jalan menyusulkan bukti** sesudah pemberian darurat.
- `BloodUnitListDto` tidak membawa penanda darurat, dan ringkasan daftar tidak punya hitungan tunggakan.

### 1.1 Keputusan pemilik `Sukmagp`, 24 September 2026

| No | Keputusan |
| --- | --- |
| `E1` | Bangun preset `emergencyPendingEvidence=true`. Catat **backlog**: belum ada endpoint penyusulan bukti sesudah pemberian darurat. Tanpa jalan pintas frontend |
| `E2` | Preset menjadi satu-satunya sumber penanda tunggakan; **tidak** mengambil detail per baris. Penanda khusus pada tabel dan hitungan ringkasan menjadi **backlog** |
| `E3` | Tampilkan status, waktu pengajuan, waktu keputusan, dan label **Anda** bila `RequestedByUserId` sama dengan pengguna yang login. GUID tidak ditampilkan |
| `E6` | Runtime memakai dua akun: A (`BloodUnit : Correct`) mengajukan, B (`BloodUnit : ApproveCorrection`) menyetujui/menolak. Buktikan `FE-BD-017` |
| Cakupan | Detail kantong: ajukan, daftar, setujui, tolak. Daftar kantong: preset tunggakan. Tanpa endpoint baru, tanpa hitungan pemenuhan di frontend, tanpa sentuhan biaya |

---

## 2. Proses bisnis dari sisi pengguna

**Contoh.** Order meminta 1 kantong dan kantong `TEST-BD013-20260917193327-01` sudah diberikan.
Petugas BDRS menyadari pencatatan pemberiannya keliru.

### 2.1 Alur normal

| No | Langkah | Pelaku | Yang terlihat di layar |
| ---: | --- | --- | --- |
| 1 | Buka **Kantong Darah**, lalu klik dua kali kantong berstatus Diberikan | Petugas BDRS | Detail kantong dengan bagian **Koreksi Pencatatan Pemberian**, yang menjelaskan bahwa koreksi berlaku hanya sesudah disetujui dan biaya tindakan tidak ikut dibatalkan. Tombol **Ajukan Koreksi** ada di kepala halaman |
| 2 | Tekan **Ajukan Koreksi** | Petugas BDRS | Dialog berisi isian **Apa yang Keliru Dicatat**, **Seharusnya Tercatat**, **Alasan Koreksi** (dari daftar alasan terkendali), dan **Bukti Pendukung**. Tombol konfirmasi nonaktif sampai keempatnya diisi |
| 3 | Tekan **Ajukan Koreksi** di dialog | Petugas BDRS | Pemberitahuan **Koreksi diajukan** berisi pesan backend. Baris baru tampil dengan status **Menunggu persetujuan** dan penanda **Anda**. Di bawah kepala halaman muncul peringatan **Koreksi menunggu persetujuan**: koreksi belum berlaku, dan angka pemenuhan order tidak berubah. Tidak ada tombol Setujui/Tolak pada baris milik sendiri |
| 4 | Buka kantong yang sama | Dokter Bank Darah | Baris koreksi tanpa penanda **Anda**, dengan tombol **Setujui** dan **Tolak** |
| 5a | Tekan **Tolak**, isi **Alasan Penolakan**, lalu tekan **Ya, Tolak** | Dokter Bank Darah | Status menjadi **Ditolak**, beserta waktu dan keterangan keputusan. Rekam pemberian tidak berubah |
| 5b | Tekan **Setujui**, lalu **Ya, Setujui** (keterangan opsional) | Dokter Bank Darah | Status menjadi **Disetujui**. Angka pemenuhan order dihitung ulang oleh backend. Pemberian asal tetap tersimpan |

### 2.2 Daftar tunggakan bukti darurat

| No | Langkah | Yang terlihat di layar |
| ---: | --- | --- |
| 1 | Pada **Kantong Darah**, tekan **Tunggakan Bukti Darurat** | Tombol aktif; daftar hanya memuat kantong yang menurut backend diberikan lewat jalur darurat dan buktinya belum ada. Muncul keterangan bahwa pencatatan bukti susulan belum tersedia di sistem, sehingga daftar ini untuk dipantau |
| 2 | Tekan lagi tombol yang sama, atau pilih preset lain | Saringan tunggakan mati. Keempat preset saling eksklusif |

### 2.3 Jalur tidak normal

| Keadaan | Yang terjadi |
| --- | --- |
| Pengguna memegang `Correct` **dan** `ApproveCorrection` | Tombol keputusan tetap tersembunyi pada koreksinya sendiri (`FE-BD-017`). Bila dipaksa lewat API, backend menolak `422 VAL-BD-073` |
| Tanpa `BloodUnit : Correct`, atau tanpa `BloodBankReason : Read` | Tombol **Ajukan Koreksi** tidak tampil, karena alasan wajib dipilih dari daftar |
| Tanpa `BloodUnit : ApproveCorrection` | Tombol **Setujui/Tolak** tidak tampil |
| Identitas pengguna belum terbaca | Tombol keputusan tidak tampil (keputusan ketat) |
| Kantong bukan Diberikan | Tidak ada tombol **Ajukan Koreksi**, karena backend tidak menawarkannya. Bagian koreksi hanya tampil bila kantong sudah punya koreksi |
| Koreksi sudah diputuskan petugas lain | Backend menolak `422 VAL-BD-075`. Pesan tampil di dialog, lalu daftar dimuat ulang |
| Kantong baru saja diubah petugas lain saat pengajuan | Backend menolak `409`. Dialog ditutup dan detail dimuat ulang |
| Belum ada alasan aktif berkategori `IssuanceCorrection` | Kalimat "Belum ada alasan aktif untuk koreksi pencatatan pemberian." dan tombol konfirmasi nonaktif |
| Kantong di daftar tunggakan | Tidak ada tombol untuk mencatat bukti susulan (backlog `E1`) |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Blueprint: kartu `FE-BD-008`, `03-frontend-architecture.md` §3 `FE-BD-04`/`FE-BD-05` dan kewajiban
  `FE-BD-004`/`016`/`017`, `00-interview-decisions.md` (`DEC-BD-023`), `api-contract.md` baris 242 dan
  252–255, `validation-matrix.md` (`VAL-BD-016/024/025/049/073..077`), laporan `BE-BD-010`,
  `requirement-traceability.md`.
- Backend (baca saja): `BbkBloodUnitController.cs` (keempat endpoint, `GetAll`, `GetCurrentUserId`),
  `IssuanceCorrectionDtos.cs`, `BbkCorrectionStatus.cs`, `BbkBloodUnitService.cs`
  (`RequestIssuanceCorrectionAsync`, `DecideIssuanceCorrectionAsync`, `ReadIssuanceCorrectionsAsync`,
  `AvailableActionsFor`, `RecordCompatibilityEvidenceAsync`, penyaring `emergencyPendingEvidence`),
  `BloodUnitDtos.cs`, `BloodOrderDtos.cs` (`FulfillmentSummaryDto`), `AuthController.cs` (klaim
  `NameIdentifier` dan `UserId` login sama-sama `user.Id`), `MstBloodBankReason.cs`.
- Frontend: `blood-unit-detail-view.jsx`, `blood-unit-resolution-dialogs.jsx`,
  `blood-unit-issuance-dialogs.jsx`, `blood-unit-list-view.jsx`, `use-blood-unit-detail.jsx`,
  `use-blood-unit-resolution.jsx`, `use-blood-unit-storage.jsx`, `use-blood-unit-list.jsx`,
  `blood-unit.service.js`, `blood-unit-constants.jsx`, `blood-unit-utils.js`, `use-permission.jsx`,
  `permission-slice.jsx`, `login-slice.jsx` (`selectUserInfo`, cookie `userId`),
  `use-inpatient-correction.jsx` (preseden `selectUserInfo`), `base-button.jsx`, spec
  `blood-unit-resolution-screen.spec.mjs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/blood-bank-management/blood-unit-constants.jsx` | **Diubah.** Aksi `RequestIssuanceCorrection`, `ISSUANCE_CORRECTION_REASON_CATEGORY`, batas panjang isian (500/1000, sama dengan service backend), `BLOOD_UNIT_CORRECTION_STATUS` beserta badge-nya, `BLOOD_UNIT_EMERGENCY_PENDING_EVIDENCE_FILTER`, filter bawaan `emergencyPendingEvidence`, dan salinan deskripsi daftar |
| `src/lib/services/health-services/blood-bank-management/blood-unit.service.js` | **Diubah.** `getBloodUnitCorrections`, `requestBloodUnitCorrection` (tepat `whatWasWrong`, `whatIsCorrect`, `reasonCode`, `supportingEvidenceNote`, `version`; kedua isian penjaga backend tidak dikirim), `approveBloodUnitCorrection` dan `rejectBloodUnitCorrection` (tepat `decisionNote`) |
| `src/utils/health-services/blood-bank-management/blood-unit-utils.js` | **Diubah.** `normalizeCorrectionStatusName`, `isCorrectionAwaitingDecision`, `normalizeIssuanceCorrections`, `isOwnCorrection`, `canDecideCorrection`, `buildCorrectionResult` |
| `src/lib/hooks/health-services/blood-bank-management/use-blood-unit-correction.jsx` | **Baru.** Sub-hook berpola `useBloodUnitResolution`. Gerbang hak akses, daftar koreksi dari `GET /{id}/corrections` yang dimuat ulang setiap detail berganti, perbandingan pelaku dengan `selectUserInfo`, dialog, kirim, dan penanganan `409`/`422` |
| `src/lib/hooks/health-services/blood-bank-management/use-blood-unit-detail.jsx` | **Diubah.** Merangkai `useBloodUnitCorrection` dan membersihkan galat koreksi saat muat ulang |
| `src/components/view/health-services/blood-bank-management/blood-units/detail/blood-unit-correction-dialogs.jsx` | **Baru.** Tiga `ConfirmModal`: Ajukan (primary), Setujui (primary), Tolak (danger) |
| `src/components/view/health-services/blood-bank-management/blood-units/detail/blood-unit-detail-view.jsx` | **Diubah.** Tombol **Ajukan Koreksi**, peringatan koreksi menunggu (`FE-BD-016`), bagian **Koreksi Pencatatan Pemberian** dengan kolom status/waktu/isi/penanda **Anda** dan tombol keputusan per baris |
| `src/lib/hooks/health-services/blood-bank-management/use-blood-unit-list.jsx` | **Diubah.** Isian `emergencyPendingEvidence`, preset keempat yang saling eksklusif, dan `toggleEmergencyPendingEvidence` |
| `src/components/view/health-services/blood-bank-management/blood-units/blood-unit-list-view.jsx` | **Diubah.** Tombol **Tunggakan Bukti Darurat** dan keterangan backlog `E1` saat preset aktif |
| `tests/unit/blood-unit-correction.test.mjs` | **Baru.** 8 test utility murni dan konstanta kontrak, termasuk `FE-BD-017` |
| `tests/e2e/blood-unit-correction-screen.spec.mjs` | **Baru.** 10 skenario runtime terhadap backend sungguhan. Akun B opsional; tanpa akun B, skenario tahap kedua dilewati |

### 3.3 Kepatuhan arsitektur frontend

- Alur `view → hook → service → InstanceAxios` dipatuhi. Sub-hook baru disambungkan lewat
  `requestReload`/`onResult` yang sudah ada.
- **Nol logika bisnis frontend.**
  - Kelayakan pengajuan dibaca dari `AvailableActions`, dan izin dari `decide()`.
  - Status "menunggu keputusan" dibaca dari `CorrectionStatus` backend.
  - Angka pemenuhan tidak dihitung maupun ditampilkan ulang oleh layar ini.
  - Biaya tidak disentuh.
  - Kesiapan tombol konfirmasi hanya memeriksa isian bertanda bintang yang masih kosong. Isinya tetap dinilai backend (`VAL-BD-076`/`077`).
- **Perbandingan pelaku** (`FE-BD-017`) memakai `state.auth.userInfo.userId`, yang dibaca dari cookie
  `userId`. Login sungguhan mengisinya dari `UserId` jawaban backend, sedangkan backend mengisi
  `RequestedByUserId` dari klaim `NameIdentifier`. Kedua nilai itu sama-sama `user.Id`
  (`AuthController.cs`). Perbandingannya tidak peka huruf besar-kecil. Identitas yang tidak terbaca
  berarti tombol keputusan disembunyikan.
- Daftar tunggakan tidak mengambil detail per baris (keputusan `E2`).
- Nol CSS baru. `blood-unit.module.css` dipakai apa adanya. Pemilih alasan memakai pola `htmlFor`
  (tidak dibungkus `<label>`, temuan `FE-BD-005`).

### 3.4 Gerbang keputusan base component

`UI GATE: 7 elemen — REUSE 3, EXTEND 0, COMPOSE 4, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Tombol **Ajukan Koreksi** dan preset **Tunggakan Bukti Darurat** | `BaseButton` | Pola tombol kepala `FE-BD-007` dan preset `FE-BD-012` | REUSE | `secondary`; preset memakai `aria-pressed` seperti **Lokasi Nonaktif** |
| Pemberitahuan hasil | `resultNotice` + `ToastStack` lewat `BaseDetailView` | Pola `FE-BD-004`/`005`/`007` | REUSE | Pesan backend apa adanya |
| Status koreksi dan penanda **Anda** | `StatusBadge` | Pola badge "Berlaku"/"Lokasi nonaktif" di riwayat penempatan | REUSE | Label status dari `CorrectionStatusLabel` backend |
| Bagian daftar koreksi + tombol per baris | `BaseDetailSection` + `DataTable` + `BaseButton size="sm"` | Pola bagian riwayat detail kantong; `size="sm"` di `provider-request-detail-view` | COMPOSE | Kolom disusun di view seperti kolom riwayat lain |
| Dialog Ajukan | `ConfirmModal` + `FilterSelect` + `textarea` di `.formField` + `InformationAlert` | Pola dialog jalur darurat `FE-BD-005` | COMPOSE | — |
| Dialog Setujui / Tolak | `ConfirmModal` `primary`/`danger` + `InformationAlert` ringkasan | Pola dialog `FE-BD-007` | COMPOSE | Tolak memakai varian bahaya karena keputusannya final |
| Peringatan koreksi menunggu dan keterangan tunggakan | `InformationAlert` | Pola penanda `PendingReview` `FE-BD-007` | COMPOSE | Kalimat disusun dari jumlah status backend saja |

Nol base component diubah; tidak ada elemen berstatus `NEW` atau `EXTEND`.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tombol **Menyiapkan...** saat alasan dimuat; tabel "Memuat koreksi pencatatan..."; pemilih "Memuat alasan..."; tombol konfirmasi berputar selama kiriman; seluruh tombol aksi nonaktif selama satu aksi berjalan |
| Kosong | "Belum ada koreksi" pada kantong Diberikan tanpa koreksi; "Belum ada alasan aktif untuk koreksi pencatatan pemberian."; daftar tunggakan kosong memakai kalimat kosong daftar kantong |
| Gagal | "Daftar koreksi pencatatan gagal dimuat." di bagian koreksi; pesan backend apa adanya di dialog (`400`/`422`) atau di kepala halaman (`409`); "Pilihan alasan koreksi gagal dimuat." |
| Tanpa hak akses | Tombol yang tak berhak tidak ditampilkan; `403` backend tetap ditampilkan lewat `AccessDeniedGate` |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Blood Bank Management / Blood Unit

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/blood-bank-management/blood-units` | Preset tunggakan dengan `emergencyPendingEvidence=true` | `BloodUnit : Read` |
| `GET` | `/v1/health-services/blood-bank-management/blood-units/{id}` | Detail, `AvailableActions`, `UnitStatus`, `Version` | `BloodUnit : Read` |
| `GET` | `/v1/health-services/blood-bank-management/blood-units/{id}/corrections` | Daftar koreksi beserta status | `BloodUnit : Read` |
| `POST` | `/v1/health-services/blood-bank-management/blood-units/{id}/corrections` | Ajukan; isi tepat `whatWasWrong`, `whatIsCorrect`, `reasonCode`, `supportingEvidenceNote`, `version` | `BloodUnit : Correct` (`403 VAL-BD-024`) |
| `POST` | `/v1/health-services/blood-bank-management/blood-units/{id}/corrections/{correctionId}/approve` | Setujui; isi tepat `decisionNote` (opsional) | `BloodUnit : ApproveCorrection` (`403 VAL-BD-074`) |
| `POST` | `/v1/health-services/blood-bank-management/blood-units/{id}/corrections/{correctionId}/reject` | Tolak; isi tepat `decisionNote` (wajib menurut backend) | `BloodUnit : ApproveCorrection` (`403 VAL-BD-074`) |

#### Health Services / Master Data / Blood Bank Reason

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/master-data/blood-bank-reasons/options?category=IssuanceCorrection` | Pilihan alasan koreksi | `BloodBankReason : Read` |

#### Health Services / Blood Bank Management / Blood Order

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/blood-bank-management/blood-orders/{id}/fulfillment` | **Hanya bukti runtime** (`R3`), dibaca spec langsung dari backend. Layar task ini tidak memanggilnya | `BloodOrder : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Kode keluar `0`, nol error | `PASS` | Keluaran perintah |
| `npx eslint` pada berkas task | 0 error, 5 warning | 4 `EXISTING WARNING` + 1 warning baru berpola lama | Empat sudah ada sebelum task: tiga `set-state-in-effect` di `use-blood-unit-detail.jsx`/`use-blood-unit-list.jsx`, dan satu `exhaustive-deps` pada `state.detail \|\| {}` di view. Satu baru: `set-state-in-effect` di `use-blood-unit-correction.jsx`, berpola sama persis dengan pemuatan riwayat penempatan di `use-blood-unit-storage.jsx` |
| `npm run test:unit` | 1619 test — **1612 lulus, 7 gagal**; ke-8 test baru lulus | `PASS` untuk cakupan task | Ketujuh kegagalan sama persis dengan baseline `FE-BD-007`: `route, menu, dan store terdaftar`, empat `FE-RWI-042`, `FE-RWI-043`, `M0` menu Setup Bank Darah |
| `npm run build` | Kode keluar `0`, `Compiled successfully in 110s`, **371 halaman**, standalone siap | `PASS` | `/blood-units` dan `/blood-units/[slug]` terdaftar |
| Grep anti-regresi UI | Nol tombol mentah, nol `<table>`, nol utility typography Bootstrap, nol inline style, nol pemanggilan Axios di view, nol CSS baru | `PASS` | Keluaran grep pada berkas diubah |
| Runtime akun A (`R0`–`R3`, `R5`, `R9`) | **6 dari 6 `PASS`** (dari 3 run; dua kegagalan di tengah adalah cacat spec) | `PASS` | Bagian 6.1–6.2 |
| Runtime tahap kedua (`R4`, `R6`, `R7`, `R8`) | Tidak dijalankan (`SKIPPED`) | `NOT RUN` | Akun B belum diberikan (`E6`) |

`AUTOMATED TEST: npm run test:unit — PASS` (8 test baru lulus; 7 kegagalan lain sudah ada sebelumnya).

Uji manual: **sebagian.** Tombol **Ajukan Koreksi**, dialog pengajuan, pemilih alasan, konfirmasi,
peringatan menunggu, penanda **Anda**, dan preset tunggakan (nyala, ganti preset, matikan) dijalankan di
peramban sungguhan lewat Playwright. Dialog **Setujui/Tolak** belum dijalankan di layar
(`MANUAL TEST: NOT FEASIBLE` untuk akun B — identitas kedua belum tersedia).

### 6.1 Cara uji

- Hasil build standalone dijalankan pada `http://127.0.0.1:3710` dan diuji lewat Playwright Chromium.
- Setiap request `/v1/**` diteruskan ke **backend sungguhan**: `QuilvianSystemBackend` di
  `https://localhost:7184`, dijalankan pemilik dari `sukmagp` terbaru, database `QuilvianNewDevSukma`.
- **Akun A** adalah sesi `superadmin` yang diberikan pemilik. Identitas ini sungguhan, dan ia memegang
  **seluruh** butir hak akses, termasuk `Correct` sekaligus `ApproveCorrection`.
- Satu-satunya jawaban yang dipasang adalah daftar kewenangan pada `R5`. Di sana akun A tetap dirinya
  sendiri, hanya daftar kewenangannya dibatasi ke `Read`, `Correct`, `ApproveCorrection`, dan
  `BloodBankReason : Read`.
- **Jawaban bisnis backend tidak pernah dikarang.**

**Fixture (`R0`, ditemukan dari backend).**

| Unsur | Nilai |
| --- | --- |
| Kantong | `TEST-BD013-20260917193327-01` (`0991186d-f51d-4133-a665-8b29bcc37048`), berstatus Diberikan tanpa koreksi yang masih menunggu |
| Order alokasi aktif | `4fb9cf3b-df46-4319-bc33-73266c0ddc39` |
| Alasan | `TBD010-KOREKSI` (kategori `IssuanceCorrection`) |
| Angka pemenuhan awal | Diberikan **1** |

### 6.2 Hasil validasi runtime — 24 September 2026

| Kode | Skenario | Hasil |
| --- | --- | --- |
| `R0` | Fixture: kantong Diberikan yang menawarkan `RequestIssuanceCorrection`, order alokasinya, alasan `IssuanceCorrection`, angka pemenuhan awal | `PASS` |
| `R1` | Akun A membuka kantong: **Ajukan Koreksi** tampil; bagian koreksi tampil; nol tombol Setujui/Tolak; nol GUID di bagian koreksi | `PASS` |
| `R2` | Akun A mengajukan. Pemilih alasan meminta `category=IssuanceCorrection`, dan tombol konfirmasi nonaktif sebelum isian lengkap. Hasil kiriman: `200` dengan isi **tepat** 5 kunci, `correctionStatus` `0`, `requestedByUserId` = akun A. Pemberitahuan **Koreksi diajukan** tampil. Baris berstatus **Menunggu persetujuan** dengan penanda **Anda**, dan peringatan **Koreksi menunggu persetujuan** tampil. **Nol tombol keputusan pada baris sendiri, walaupun akun A sungguh memegang `ApproveCorrection`** (`FE-BD-017`). Koreksi `3d57f1f0-d2b2-47b5-875c-63a5e7a562b1` | `PASS` (run 2) |
| `R3` | `FE-BD-016`: `TotalIssuedQuantity` backend tetap **1** sesudah pengajuan | `PASS` (run 2) |
| `R4` | Akun B melihat **Setujui/Tolak** pada koreksi akun A, tanpa **Ajukan Koreksi** dan tanpa penanda **Anda** | `NOT RUN` — akun B belum ada |
| `R5` | `FE-BD-017` dengan daftar kewenangan minimal (`Correct` + `ApproveCorrection`): baris sendiri berpenanda **Anda** dan tanpa tombol keputusan; **Ajukan Koreksi** tetap tampil | `PASS` (run 2) |
| `R6` | Akun B menolak: alasan wajib, isi tepat `decisionNote`, status **Ditolak**, pemenuhan tetap | `NOT RUN` — akun B belum ada |
| `R7` | Akun A mengajukan koreksi kedua, akun B menyetujui: status **Disetujui**, peringatan menunggu hilang, pemenuhan dihitung ulang backend | `NOT RUN` — akun B belum ada |
| `R8` | Daftar koreksi backend: status dan pelaku sesuai `R6`/`R7` | `NOT RUN` — akun B belum ada |
| `R9` | Preset **Tunggakan Bukti Darurat**. Query membawa `emergencyPendingEvidence=true` tanpa `unitStatus`/`inactiveLocation`. Tombol `aria-pressed=true` dan keterangan backlog tampil. `totalData` layar sama dengan backend, **5 kantong**. Beralih ke **Menunggu Keputusan** mengosongkan saringan tunggakan, dan menekan tombolnya dua kali mematikannya | `PASS` (run 3) |

**Bukti tambahan lewat API, tanpa mengubah data.** `POST …/3d57f1f0…/approve` oleh akun A sendiri
menghasilkan **`422`, `errors.code` `VAL-BD-073`**, dengan pesan "Koreksi tidak dapat disetujui oleh
orang yang mengajukannya. Mintakan keputusan kepada Dokter Bank Darah lain." Koreksi tetap
**Menunggu persetujuan**. Tombol yang disembunyikan layar selaras dengan aturan backend.

**Riwayat percobaan, apa adanya.**

1. **Run 1** — `R0`, `R1` lulus. `R2` gagal karena **cacat spec**: pemilih alasan dicari lewat teks
   placeholder, padahal nama aksesibelnya adalah label "Alasan Koreksi *". Kegagalan terjadi **sebelum**
   tombol konfirmasi ditekan, jadi nol kiriman tulis. 7 skenario tidak berjalan.
2. **Run 2** — spec dicari lewat label, seperti spec `FE-BD-007`. `R0`, `R1`, `R2`, `R3`, `R5` lulus,
   dan 4 skenario akun B dilewati. `R9` gagal karena **cacat spec**: halaman daftar dibuka dengan
   sesi akun B yang kosong, sehingga layar terlempar ke halaman login. Diagnosa dengan spec sementara
   membuktikan halaman daftar normal dengan sesi akun A; spec sementara itu sudah dihapus.
3. **Run 3** — `R9` memakai akun B bila ada, akun A bila tidak. `R0` dan `R9` lulus.

Source produk tidak berubah di antara run.

**Keadaan database sesudah run.**

| Data | Keadaan |
| --- | --- |
| Koreksi `3d57f1f0-d2b2-47b5-875c-63a5e7a562b1` pada `TEST-BD013-20260917193327-01` | **Menunggu persetujuan**, diajukan akun A (`superadmin`). Dibiarkan; koreksi bersifat append-only |
| Kantong `TEST-BD013-20260917193327-01` | Tetap Diberikan, pemenuhan order tetap 1 |

**Kebersihan sesudah run:**
- Server standalone uji dihentikan.
- Berkas sesi di scratchpad dihapus.
- Nol kemunculan token di log run dan `test-results/`.
- Dua berkas tracked di `test-results/` yang diubah Playwright dipulihkan dengan `git restore` pada
  kedua path itu saja.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Koreksi menuntut **dua langkah** | **Sebagian** | Tahap 1 (ajukan → Menunggu persetujuan, belum berlaku) terbukti runtime (`R2`). Tahap 2 (Setujui/Tolak oleh orang lain) sudah ada di source dan tercakup test unit, tetapi belum dijalankan di layar (`R4`, `R6`, `R7`, `R8` menunggu akun B) |
| Daftar tunggakan bukti darurat tersedia (worklist #3, `FE-BD-004`) | **Terpenuhi** | `R9`; keputusan `E1`/`E2` |
| `FE-BD-016` — koreksi menunggu tampil sebagai **menunggu**; angka pemenuhan **tidak berubah** sampai keputusan turun | **Terpenuhi** | `R2` (status dan peringatan), `R3` (pemenuhan backend tetap 1). Bagian "berubah sesudah disetujui" menunggu `R7` |
| `FE-BD-017` — Setujui/Tolak **tersembunyi** pada koreksi pengguna sendiri, ditentukan perbandingan pelaku | **Sebagian** | Sisi "tersembunyi" terbukti: `R2` dengan identitas yang sungguh memegang `ApproveCorrection`, `R5`, `422 VAL-BD-073` lewat API, dan 4 test unit. Sisi "tampil bagi pemutus lain" belum terbukti runtime (`R4`) |
| Catatan biaya — layar tidak menjanjikan pembatalan tagihan | **Terpenuhi** | Deskripsi bagian koreksi (terlihat di `R1`/`R2`) dan ketiga dialog menyatakan biaya tindakan tidak ikut dibatalkan; nol kode biaya |
| Keputusan pemilik `E1`, `E2`, `E3` | **Terpenuhi** | `R9`; tidak ada pengambilan detail per baris; `R1`/`R2` (status, waktu, **Anda**, nol GUID) |
| Keputusan pemilik `E6` — dua akun | **Belum terpenuhi** | Akun B belum diberikan |
| Definition of Done | `NOT APPLICABLE` | Kartu tidak memuat baris DoD |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Token sesi `superadmin` backend lokal diberikan pemilik **di chat** untuk runtime task ini. Token disimpan sementara di scratchpad, tidak pernah dicetak ke log atau laporan, dan berkasnya sudah dihapus. Karena sudah tertulis di riwayat percakapan, **disarankan logout atau rotasi sesi `superadmin` lokal** |
| Masalah yang diketahui | **Backlog `E1` (backend):** belum ada endpoint untuk mencatat bukti kecocokan susulan pada kantong yang sudah Diberikan lewat jalur darurat. `RecordCompatibilityEvidenceAsync` hanya menerima kantong `Allocated`, sehingga daftar tunggakan tidak pernah berkurang. **Backlog `E2` (backend):** `BloodUnitListDto` tanpa penanda darurat, dan ringkasan daftar tanpa hitungan tunggakan. **Backlog `E3` (backend):** `IssuanceCorrectionDto` tanpa `requestedByName`/`decidedByName`. **Catatan kontrak:** backend mengizinkan lebih dari satu koreksi Menunggu persetujuan pada satu kantong; layar tidak membatasinya |
| Dependency backend | Nihil yang menahan source. Yang menahan status ✅ adalah akun B untuk runtime `E6` |
| Perubahan sampingan | Dua berkas tracked `test-results/` yang diubah Playwright dipulihkan dengan `git restore` pada kedua path itu saja. Spec diagnosa sementara dibuat lalu dihapus |
| Interupsi | Dua kegagalan cacat spec (bagian 6.2), keduanya diperbaiki tanpa mengubah source produk |
| Status Git | Frontend: ` M blood-unit-list-view.jsx`, ` M blood-unit-detail-view.jsx`, ` M blood-unit-constants.jsx`, ` M use-blood-unit-detail.jsx`, ` M use-blood-unit-list.jsx`, ` M blood-unit.service.js`, ` M blood-unit-utils.js`; baru `blood-unit-correction-dialogs.jsx`, `use-blood-unit-correction.jsx`, `tests/e2e/blood-unit-correction-screen.spec.mjs`, `tests/unit/blood-unit-correction.test.mjs`. Belum di-stage atau di-commit. Backend: hanya laporan ini dan tautan bukti pada roadmap serta `requirement-traceability.md` |
| Langkah berikutnya | Berikan sesi akun B (`BloodUnit : Read, ApproveCorrection` + `BloodOrder : Read`) beserta ID penggunanya, lalu jalankan `R4`, `R6`, `R7`, `R8` dengan spec yang sama. Akun A boleh tetap `superadmin`; spec membuat koreksinya sendiri. Bila keempatnya `PASS`, task ini naik ke ✅. Koreksi `3d57f1f0…` yang masih menunggu dapat diputuskan akun B secara manual |
