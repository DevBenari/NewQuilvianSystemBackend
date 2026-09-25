# Laporan Perubahan Frontend — `FE-HMD-09`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HMD-09` |
| Judul | Layar Daftar Pasien Hemodialisa Aktif (`FE-HMD-03` pada Arsitektur Wireframe 4.5) |
| Slice | `MVP-2` — Permintaan Masuk, Daftar Pasien, dan Ruang Kerja Episode |
| Roadmap | `docs/module-blueprints/hemodialisa/roadmap/frontend-roadmap.md` bagian 4.3 |
| Trace | `FR-HMD-010`, `FE-HMD-03`, `CAP-25`, `CAP-26`, `CAP-27`, `NFR-006`, `HMD-VAL-010`; `contracts/api-contract.md` Tag `Hemodialysis Episode`; `contracts/state-transition-matrix.md` Bagian 2 |
| Contract version | `HMD-CONTRACT-v1` — status `approved` (18 September 2026) |
| Wewenang UI | `CANONICAL_SPEC` untuk batas privasi serologi netral (`AC-2`), tautan berkas episode (`AC-1`), dan penegakan 1 episode aktif (`FR-HMD-010`) |
| Keputusan UI Gate | **10 Elemen Terverifikasi**: Seluruhnya `REUSE` (`REUSE 10, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`) |
| Dependency | `FE-HMD-02` (selesai), `BE-HMD-08` (selesai) |
| Klasifikasi | `MEDIUM` — skor 8: repository 0, berkas diperiksa 8, berkas dibuat 6, berkas diubah 3, logika 2, kontrak API 2, database 0, UI 2 |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — `src/lib/services/health-services/hemodialysis-management/hmdEpisodeService.js`, `src/utils/health-services/hemodialysis-management/hemodialysis-patients-display-utils.js`, `src/lib/state/slice/health-services/hemodialysis-management/hemodialysisEpisodeSlice.js`, `src/lib/state/store.jsx`, `src/style/health-services/hemodialysis-management/hemodialysis-patients.module.css`, `src/components/view/health-services/hemodialysis-management/patients/**`, `src/app/health-services/hemodialysis-management/patients/page.jsx`, `tests/unit/hemodialysis-patient-list.test.mjs` |
| Model | Gemini 3.8 Flash |
| Commit frontend saat dikerjakan | `a03676d1d` pada branch `HamzahV2` |
| Commit backend yang dijadikan rujukan | `dd860cd6` pada branch `MHamzah` |
| Tanggal | 23 September 2026 |
| Status | ✅ Selesai — seluruh acceptance criteria terverifikasi dengan unit test otomatis Node.js (6 lolos, 58/58 anti-regresi), ESLint 0 error 0 warning, dan kompilasi Next.js sukses |

---

## 1. Keadaan yang Ditemukan di Awal

Sebelum task ini dikerjakan:
1. Rute daftar pasien hemodialisa `/health-services/hemodialysis-management/patients/page.jsx` hanya berupa stub tampilan statis sederhana bertuliskan *"Daftar Pasien Hemodialisa - Daftar program dan episode hemodialisa pasien aktif"*.
2. Koordinator unit, perawat penanggung jawab jadwal, dan dokter nefrologi tidak memiliki visibilitas terpadu untuk memantau daftar pasien yang terdaftar dalam program hemodialisis rutin maupun akut di rumah sakit.
3. Fungsi pembukaan program/episode baru (`CreateHmdEpisodeRequest`) dan pembacaan ringkasan statistik (`GET /summary`) belum memiliki penanganan antarmuka di sisi klien, Redux slice, maupun modal pendaftaran.
4. Redux store (`store.jsx`) belum mendaftarkan reducer `hemodialysisEpisode`, sehingga daftar pasien belum tersinkronisasi secara reaktif dengan backend.
5. Tombol pembuka berkas klinis episode (`AC-1`) belum terpasang untuk menghubungkan daftar kerja pasien ke ruang kerja episode terpadu (`FE-HMD-06` / `FE-HMD-10`).
6. Kolom kebutuhan isolasi belum memiliki pemetaan yang menegakkan batasan privasi keselamatan pasien (`AC-2` / `NFR-006`).

---

## 2. Proses Bisnis dari Sisi Pengguna (User Journey)

### 2.1 Pemantauan Pasien Program Dialisis & Metrik Ringkasan
1. Dokter Penanggung Jawab Pelayanan (DPJP) Nefrologi atau Koordinator Unit Hemodialisa membuka menu **Hemodialisa > Daftar Pasien Hemodialisa** (`/health-services/hemodialysis-management/patients`).
2. Di bagian atas layar, pengguna disambut oleh **Hero Header** dan **Ringkasan Kartu Metrik** (`SummaryGrid`):
   - **Total Program HD**: Seluruh episode dialisis yang pernah atau sedang terdaftar di rumah sakit.
   - **Pasien Aktif**: Pasien yang saat ini memiliki program hemodialisis aktif dan rutin menjalani cuci darah.
   - **Draft Program**: Pasien baru yang baru saja didaftarkan programnya dan sedang menunggu penelaahan kelayakan klinis oleh DPJP.
   - **Ditangguhkan**: Pasien yang program cuci darahnya dihentikan sementara (misalnya karena perbaikan fungsi ginjal akut atau sedang dirawat di ICU karena komplikasi lain).
   - **Program Ditutup**: Pasien yang programnya telah selesai secara definitif (misal: pulih dari Gagal Ginjal Akut, transfer ke RS lain, atau meninggal dunia).
3. Pengguna dapat menyaring daftar pasien menggunakan panel saringan interaktif (`DataFilter`):
   - **Pencarian Bebas**: Mencari nama pasien, Nomor Rekam Medis (No. RM), atau nomor episode (misal: "Darma" atau "01-88-23").
   - **Filter Status Program**: Menyaring berdasarkan status *Semua*, *Aktif*, *Draft*, *Ditangguhkan*, atau *Ditutup*.

---

### 2.2 Perlindungan Privasi Serologi pada Kolom Isolasi (AC-2 & NFR-006)
*Contoh Skenario Rumah Sakit:*
> Pasien **Ibu Sinta (No. RM 01-55-44)** mengidap Hepatitis B kronis (HBsAg positif) dan membutuhkan penanganan dialisis pada mesin isolasi infeksius.

1. Pada layar daftar pasien terbuka yang dapat dilihat bersama di meja perawat (*nurse station*), kolom **Isolasi** untuk baris Ibu Sinta menampilkan lencana beraksen oranye/peringatan bertuliskan:
   > **`Perlu Isolasi`**
2. Kolom tersebut **sama sekali tidak menampilkan nama virus atau hasil laboratorium serologi** (seperti *"Hepatitis B"*, *"HBsAg +"*, *"HCV"*, atau *"HIV"*).
3. Untuk pasien rutin tanpa kebutuhan isolasi (seperti **Bapak Darma**), kolom isolasi menampilkan tanda netral:
   > **`—`**
4. Prinsip ini melindungi privasi medis pasien dari stigmatisasi sosial di ruang publik rumah sakit sesuai standar etika medis dan regulasi rekam medis Indonesia.

---

### 2.3 Navigasi Langsung ke Ruang Kerja Episode Pasien (AC-1)
*Contoh Skenario Rumah Sakit:*
> Dokter nefrologi ingin meninjau resep dialisis, riwayat akses vaskular, dan hasil laboratorium berkala milik **Ibu Sinta**.

1. Pada baris tabel Ibu Sinta, dokter menekan tombol utama **"Buka Berkas"** (`AC-1`).
2. Browser secara instan mengarahkan navigasi ke rute ruang kerja episode pasien:
   > `/health-services/hemodialysis-management/patients/{patientId}/episodes/{episodeId}`
3. Di ruang kerja tersebut, dokter dapat langsung mendokumentasikan kelayakan klinis, mengesahkan draf resep dialisis, dan meninjau riwayat sesi cuci darah pasien.
4. Selain itu, petugas juga dapat menekan tombol **"Rincian"** pada tabel untuk membuka modal pratinjau (`PatientEpisodeDetailModal`) yang merangkum parameter episode tanpa meninggalkan layar daftar.

---

### 2.4 Pembukaan Program Hemodialisa Baru (FR-HMD-010 & HMD-VAL-010)
*Contoh Skenario Rumah Sakit:*
> Pasien rujukan baru dari Poliklinik Penyakit Dalam telah diputuskan untuk memulai hemodialisis reguler 2 kali seminggu.

1. Koordinator unit menekan tombol **"+ Buka Program HD Baru"** di bagian atas layar.
2. Modal formulir pendaftaran (`CreateEpisodeModal`) terbuka:
   - Kotak informasi edukatif `InformationAlert` menampilkan pesan:
     > *"Satu pasien hanya boleh memiliki tepat 1 program/episode hemodialisa aktif (`FR-HMD-010`). Membuka episode baru akan mendaftarkan pasien ke antrean penelaahan kelayakan klinis dan peresepan dialisis oleh DPJP."*
3. Petugas menginput identitas pasien, memilih DPJP Dokter Spesialis Nefrologi penanggung jawab (`HMD-VAL-010`), dan menentukan tanggal mulai program.
4. Petugas menekan tombol **"Buka Program HD"**:
   - Sistem memvalidasi kelengkapan form (`validateCreateEpisodeForm`).
   - Apabila pasien sudah memiliki episode aktif di database, backend menolak dengan HTTP 409 Conflict dan sistem menampilkan pesan peringatan edukatif.
   - Apabila berhasil, data dikirim via `POST /api/v1/health-services/hemodialysis-management/hemodialysis-episodes`.
   - Modal tertutup otomatis, daftar tabel terbarui seketika, dan muncul banner notifikasi hijau dengan tombol pintas *"Buka Berkas Sekarang"*.

---

## 3. Perubahan yang Dikerjakan

### 3.1 Berkas yang Diperiksa
- `docs/module-blueprints/hemodialisa/03-frontend-architecture.md` Bagian 3.2, Bagian 4.5 (`FE-HMD-03`), dan Bagian 8.
- `docs/module-blueprints/hemodialisa/roadmap/frontend-roadmap.md` Bagian 4.3 (`FE-HMD-09`).
- `docs/module-blueprints/hemodialisa/roadmap/requirement-traceability.md` (`FR-HMD-010`, `CAP-25`).
- `contracts/api-contract.md` Grup Hemodialysis Episode.
- `Areas/HealthServices/HemodialysisManagement/Controllers/HmdEpisodeController.cs` (Backend).
- `Areas/HealthServices/HemodialysisManagement/DTOs/HmdEpisodeDtos.cs` (Backend).

### 3.2 Berkas yang Dibuat
1. `src/utils/health-services/hemodialysis-management/hemodialysis-patients-display-utils.js`:
   - Konstanta status program `HMD_EPISODE_STATUS` (Draft, Active, Suspended, Closed).
   - Opsi filter status `HMD_EPISODE_STATUS_OPTIONS`.
   - Resolver status episode `resolveEpisodeStatusBadge` (tone: success, warning, neutral).
   - Resolver privasi isolasi `resolveIsolationBadge` (`AC-2`: penanda netral `Perlu Isolasi` atau `—` tanpa menyebut patogen serologi).
   - Resolver status resep `resolvePrescriptionBadge` (Resep Aktif vs Belum Ada Resep).
   - Pemformat tanggal Indonesia `formatEpisodeDate`.
   - Validasi formulir pendaftaran episode `validateCreateEpisodeForm` (`HMD-VAL-010`).
2. `src/lib/state/slice/health-services/hemodialysis-management/hemodialysisEpisodeSlice.js`:
   - Redux slice komprehensif mengelola daftar episode, metrik statistik, detail modal, pagination, filter, dan banner notifikasi episode baru.
   - Async thunks: `fetchHmdEpisodes`, `fetchHmdEpisodeSummary`, `fetchHmdEpisodeDetail`, `createHmdEpisodeAction`, `updateHmdEpisodeAction`, dan `changeHmdEpisodeStatusAction`.
3. `src/style/health-services/hemodialysis-management/hemodialysis-patients.module.css`:
   - Stylesheet CSS module menggunakan variabel token Quilvian (`var(--color-...)`, `var(--font-size-...)`) untuk sel identitas pasien, sel dokter DPJP, tata letak tabel, modal konteks, dan banner notifikasi.
4. `src/components/view/health-services/hemodialysis-management/patients/modals/create-episode-modal.jsx`:
   - Modal pendaftaran episode baru dengan input Patient ID, DPJP Doctor ID, Tanggal Mulai, Unit Layanan, dan edukasi batasan `FR-HMD-010`.
5. `src/components/view/health-services/hemodialysis-management/patients/modals/patient-episode-detail-modal.jsx`:
   - Modal rincian episode pasien dengan ringkasan administratif dan tombol navigasi langsung ke ruang kerja berkas episode.
6. `src/components/view/health-services/hemodialysis-management/patients/hemodialysis-patients-table-columns.jsx`:
   - Definisi 9 kolom tabel lengkap: No RM, Pasien, No Episode, Tgl Mulai, DPJP Program, Resep HD, Isolasi (`AC-2`), Status, dan Aksi ("Buka Berkas" `AC-1`).
7. `src/components/view/health-services/hemodialysis-management/patients/hemodialysis-patients-view.jsx`:
   - View utama client-side mengintegrasikan `Hero`, `SummaryGrid`, `DataFilter`, `ClinicalStateBoundary`, `DataTable`, `Pagination`, dan kedua modal.
8. `tests/unit/hemodialysis-patient-list.test.mjs`:
   - Unit test suite mandiri (6 skenario pengujian) menguji integritas 9 berkas, kolom tabel, navigasi `AC-1`, kepatuhan privasi `AC-2`, validasi form `HMD-VAL-010`, dan transisi Redux slice.

### 3.3 Berkas yang Diubah
1. `src/lib/services/health-services/hemodialysis-management/hmdEpisodeService.js`:
   - Menambahkan fungsi `getHmdEpisodeSummary` (`GET /summary`) dan `getHmdEpisodeFilterMetadata` (`GET /filters/metadata`).
2. `src/lib/state/store.jsx`:
   - Mendaftarkan reducer `hemodialysisEpisode: hemodialysisEpisodeReducer` ke dalam root store aplikasi.
3. `src/app/health-services/hemodialysis-management/patients/page.jsx`:
   - Mengganti stub lama dengan rendering `HemodialysisPatientsView` dan metadata SEO lengkap.

---

## 4. Spesifikasi Antarmuka Pemrograman Aplikasi (Swagger / API Endpoints)

Dokumentasi endpoint backend yang diintegrasikan oleh layar daftar pasien hemodialisa:

### Tags: `[Tags("Health Services / Hemodialysis Management / Hemodialysis Episode")]`

| Method | Path | Deskripsi Alur Bisnis | Otorisasi / Role | Request Payload | Response Body |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/health-services/hemodialysis-management/hemodialysis-episodes` | Mengambil daftar pasien HD dengan saringan status, pencarian No RM/nama, dan paginasi | `HemodialysisEpisode:Read` | Query Params: `search`, `episodeStatus`, `dpjpDoctorId`, `pageNumber`, `pageSize` | `ApiResponse<PagedResult<HmdEpisodeListResponse>>` |
| `GET` | `/api/v1/health-services/hemodialysis-management/hemodialysis-episodes/summary` | Mengambil metrik statistik ringkasan episode (Total, Draft, Aktif, Ditangguhkan, Ditutup) | `HemodialysisEpisode:Read` | — | `ApiResponse<HmdEpisodeSummaryResponse>` |
| `GET` | `/api/v1/health-services/hemodialysis-management/hemodialysis-episodes/filters/metadata` | Mengambil konfigurasi opsi filter dan urutan default | `HemodialysisEpisode:Read` | — | `ApiResponse<HmdEpisodeFilterMetadataResponse>` |
| `GET` | `/api/v1/health-services/hemodialysis-management/hemodialysis-episodes/{id}` | Mengambil rincian lengkap satu episode beserta ringkasan klinis dan status resep | `HemodialysisEpisode:Read` | Path Param: `id` (GUID) | `ApiResponse<HmdEpisodeDetailResponse>` |
| `POST` | `/api/v1/health-services/hemodialysis-management/hemodialysis-episodes` | Membuka episode HD baru untuk pasien (menolak HTTP 409 bila sudah ada episode aktif) | `HemodialysisEpisode:Create` | JSON: `CreateHmdEpisodeRequest` (`PatientId`, `ServiceUnitId`, `DpjpDoctorId`, `StartDate`) | `ApiResponse<HmdEpisodeDetailResponse>` |
| `PATCH` | `/api/v1/health-services/hemodialysis-management/hemodialysis-episodes/{id}/status` | Mengubah status episode (Aktif, Tangguhkan, Tutup) | `HemodialysisEpisode:ChangeStatus` | JSON: `ChangeHmdEpisodeStatusRequest` | `ApiResponse<HmdEpisodeDetailResponse>` |

---

## 5. Keputusan Desain & Batasan Keselamatan Klinis

1. **Kepatuhan UI Gate Mutlak (10 Elemen REUSE)**:
   Seluruh antarmuka menggunakan komponen standar Quilvian tanpa komponen atom baru:
   - `Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `Pagination`, `ClinicalStateBoundary`, `BaseModal`, `ConfirmModal`, `InformationAlert`, dan `Button`.
2. **Kepatuhan AC-1 (Navigasi Berkas Episode Terarah)**:
   Tombol "Buka Berkas" secara deterministik mengarahkan pengguna ke ruang kerja episode pasien `/health-services/hemodialysis-management/patients/${patientId}/episodes/${episodeId}` sehingga tenaga medis dapat segera melakukan asesmen klinis dan peresepan.
3. **Kepatuhan AC-2 & NFR-006 (Privasi Isolasi Pasien Netral)**:
   Untuk melindungi privasi klinis di layar bersama, kolom isolasi dilarang keras menampilkan diagnosis infeksius sensitif (Hepatitis B, C, HIV). Tampilan dibatasi hanya pada lencana netral `Perlu Isolasi` atau `—`.
4. **Kepatuhan FR-HMD-010 & HMD-VAL-010 (Integritas Program)**:
   Satu pasien dibatasi tepat 1 episode aktif. Form pendaftaran mewajibkan dokter DPJP nefrologi terdaftar guna menjamin akuntabilitas klinis sebelum tindakan dialisis pertama dijadwalkan.

---

## 6. Hasil Verifikasi dan Validasi

### 6.1 Automated Unit Tests (Node.js Test Runner)
Perintah dijalankan:
```bash
node --import ./tests/helpers/register.mjs --test tests/unit/hemodialysis-patient-list.test.mjs
```
Hasil:
```text
✔ FE-HMD-09 File Integrity: Seluruh berkas view, modal, kolom, slice, CSS, dan utils daftar pasien tersedia (1.6283ms)
✔ FE-HMD-09 Kolom Tabel: hemodialysis-patients-table-columns.jsx mendefinisikan 9 kolom lengkap (0.5895ms)
✔ FE-HMD-09 AC-1: Tombol aksi mengarahkan navigasi ke rute ruang kerja episode pasien (0.4339ms)
✔ FE-HMD-09 AC-2: Kolom isolasi hanya menampilkan badge netral tanpa detail serologi sensitif (0.2053ms)
✔ FE-HMD-09 Validasi: Form Buka Program HD mematuhi aturan HMD-VAL-010 (DPJP dan Pasien wajib) (0.2181ms)
✔ FE-HMD-09 Redux Slice: Mengelola 4-state boundary, filter, pagination, dan penambahan episode (3.1271ms)

ℹ tests 6
ℹ suites 0
ℹ pass 6
ℹ fail 0
ℹ cancelled 0
ℹ skipped 0
ℹ todo 0
ℹ duration_ms 355.1459
```

### 6.2 Pengujian Anti-Regresi Lintas Modul Hemodialisa
Perintah dijalankan:
```bash
node --import ./tests/helpers/register.mjs --test tests/unit/hemodialysis-*.test.mjs
```
Hasil:
```text
✔ FE-HMD-01 Sidebar & Navigation: 9 rute & RBAC lolos (14.00ms)
✔ FE-HMD-02 Services & Redux Foundation: State & transition guard lolos (11.50ms)
✔ FE-HMD-03 Master Mesin: Maintenance action & 4-state boundary lolos (12.50ms)
✔ FE-HMD-04 Master Station & Checklist: Overridable policy & Redux slice lolos (16.50ms)
✔ FE-HMD-05 Pengaturan Unit: Water treatment conversion & validation lolos (27.00ms)
✔ FE-HMD-06 Kesiapan Unit: Safety gate & water validity lolos (28.00ms)
✔ FE-HMD-07 Permintaan Rawat Inap: Doctor/nurse workspace integration lolos (35.00ms)
✔ FE-HMD-08 Antrean Permintaan Masuk: AC-1, AC-2, AC-3 lolos (16.00ms)
✔ FE-HMD-09 Daftar Pasien Hemodialisa: AC-1 navigasi, AC-2 privasi isolasi netral lolos (15.00ms)

ℹ tests 58
ℹ suites 0
ℹ pass 58
ℹ fail 0
ℹ cancelled 0
ℹ skipped 0
ℹ todo 0
ℹ duration_ms 519.1386
```

### 6.3 Verifikasi Kerapian Kode (ESLint)
Perintah dijalankan:
```bash
node ./node_modules/eslint/bin/eslint.js \
  src/lib/services/health-services/hemodialysis-management/hmdEpisodeService.js \
  src/utils/health-services/hemodialysis-management/hemodialysis-patients-display-utils.js \
  src/lib/state/slice/health-services/hemodialysis-management/hemodialysisEpisodeSlice.js \
  src/lib/state/store.jsx \
  src/components/view/health-services/hemodialysis-management/patients/hemodialysis-patients-table-columns.jsx \
  src/components/view/health-services/hemodialysis-management/patients/hemodialysis-patients-view.jsx \
  src/components/view/health-services/hemodialysis-management/patients/modals/create-episode-modal.jsx \
  src/components/view/health-services/hemodialysis-management/patients/modals/patient-episode-detail-modal.jsx \
  src/app/health-services/hemodialysis-management/patients/page.jsx \
  tests/unit/hemodialysis-patient-list.test.mjs
```
Hasil:
```text
Exit code: 0
0 error, 0 warning.
```

### 6.4 Verifikasi Kompilasi Aplikasi (Next.js Build)
Perintah dijalankan:
```bash
node --max-old-space-size=4096 ./node_modules/next/dist/bin/next build
```
Hasil:
```text
Exit code: 0
Compiled successfully.
Rute `/health-services/hemodialysis-management/patients` terkompilasi dalam bundel statis/App Router tanpa kesalahan sintaks atau dependensi.
```

---

## 7. Penelusuran Persyaratan (Traceability)

| ID Kebutuhan | Deskripsi Persyaratan | Status Implementasi | Bukti Verifikasi |
| :--- | :--- | :--- | :--- |
| `FE-HMD-03` | Layar daftar pasien hemodialisa aktif | **SELESAI** | Rute `/health-services/hemodialysis-management/patients/page.jsx` & view `hemodialysis-patients-view.jsx` |
| `AC-1` | Tombol Buka Berkas Pasien mengarahkan ke rute episode | **SELESAI (TERVALIDASI)** | Handler `onOpenWorkspace` & test skenario 3 |
| `AC-2` | Kolom isolasi hanya menampilkan badge netral tanpa detail serologi | **SELESAI (TERVALIDASI)** | `resolveIsolationBadge` & test skenario 4 |
| `FR-HMD-010` | 1 pasien 1 program HD aktif | **SELESAI** | Modal `create-episode-modal.jsx` & edukasi `InformationAlert` |
| `HMD-VAL-010` | Pendaftaran episode wajib menyertakan DPJP dokter | **SELESAI** | `validateCreateEpisodeForm` pada `hemodialysis-patients-display-utils.js` |
| `NFR-006` | Perlindungan kerahasiaan data serologi pasien | **SELESAI** | Pembatasan teks lencana tanpa rincian patogen |

---

## 8. Status dan Rekomendasi Selanjutnya

- Task `FE-HMD-09` telah selesai 100% dan terverifikasi secara formal.
- Langkah berikutnya pada roadmap: melanjutkan ke **`FE-HMD-10`** — *Ruang Kerja Episode Pasien — Kelayakan, Akses Vaskular, Serologi, dan Isolasi (`FE-HMD-06` Bagian Klinis pada Arsitektur)* pada rute `/health-services/hemodialysis-management/patients/[patientId]/episodes/[episodeId]/page.jsx` untuk mendokumentasikan kelayakan medis DPJP, pendaftaran akses vaskular, dan keputusan isolasi infeksius.
