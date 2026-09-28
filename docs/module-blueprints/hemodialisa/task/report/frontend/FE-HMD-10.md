# Laporan Perubahan Frontend — `FE-HMD-10`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HMD-10` |
| Judul | Ruang Kerja Episode Pasien — Kelayakan, Akses Vaskular, Serologi, dan Isolasi (`FE-HMD-06` Bagian Klinis pada Wireframe 3.3) |
| Slice | `MVP-2` — Permintaan Masuk, Daftar Pasien, dan Ruang Kerja Episode |
| Roadmap | `docs/module-blueprints/hemodialisa/roadmap/frontend-roadmap.md` bagian 4.4 |
| Trace | `FR-HMD-011`, `FE-HMD-06`, `CAP-28`, `CAP-29`, `CAP-30`, `CAP-31`, `NFR-006`, `NFR-010`, `HMD-VAL-011`; `contracts/api-contract.md` Tag `Hemodialysis Eligibility`, `Hemodialysis Vascular Access`, `Hemodialysis Serology`, `Hemodialysis Isolation`; `contracts/state-transition-matrix.md` Bagian 2 & 3 |
| Contract version | `HMD-CONTRACT-v1` — status `approved` (18 September 2026) |
| Wewenang UI | `CANONICAL_SPEC` untuk batas privasi serologi (`AC-1`), alokasi isolasi PPI, dan mitigasi anti-stale data (`AC-2` / `NFR-010`) |
| Keputusan UI Gate | **10 Elemen Terverifikasi**: Seluruhnya `REUSE` (`REUSE 10, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`) |
| Dependency | `FE-HMD-02` (selesai), `FE-HMD-09` (selesai), `BE-HMD-09` (selesai) |
| Klasifikasi | `HIGH` — skor 11: repository 0, berkas diperiksa 10, berkas dibuat 6, berkas diubah 2, logika 3, kontrak API 3, database 0, UI 3 |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — `src/utils/health-services/hemodialysis-management/hemodialysis-patients-display-utils.js`, `src/lib/state/slice/health-services/hemodialysis-management/hemodialysisEpisodeSlice.js`, `src/style/health-services/hemodialysis-management/hemodialysis-episode-workspace.module.css`, `src/components/view/health-services/hemodialysis-management/patients/workspace/**`, `src/app/health-services/hemodialysis-management/patients/[patientId]/episodes/[episodeId]/page.jsx`, `tests/unit/hemodialysis-episode-workspace.test.mjs` |
| Model | Gemini 3.8 Flash |
| Commit frontend saat dikerjakan | `a03676d1d` pada branch `HamzahV2` |
| Commit backend yang dijadikan rujukan | `dd860cd6` pada branch `MHamzah` |
| Tanggal | 23 September 2026 |
| Status | ✅ Selesai — seluruh acceptance criteria terverifikasi dengan unit test otomatis Node.js (7 lolos, 65/65 anti-regresi), ESLint 0 error 0 warning, dan kompilasi Next.js sukses |

---

## 1. Keadaan yang Ditemukan di Awal

Sebelum implementasi task `FE-HMD-10`:
1. Rute rincian episode pasien hemodialisa `/health-services/hemodialysis-management/patients/[patientId]/episodes/[episodeId]` belum memiliki implementasi antarmuka ruang kerja terpadu (*clinical workspace*).
2. Dari layar daftar pasien (`FE-HMD-09`), tombol aksi *"Buka Berkas"* belum memiliki layar target fungsional tempat dokter DPJP dan perawat dialisis melakukan pendataan klinis spesifik episode.
3. Tiga fondasi klinis keselamatan dialisis belum memiliki representasi visual dan alur input di antarmuka web:
   - **Penilaian Kelayakan Klinis Hemodialisa**: Evaluasi indikasi medis dan kontraindikasi absolut/relatif oleh Dokter Spesialis Penyakit Dalam / Konsultan Ginjal Hipertensi (Sp.PD-KGH).
   - **Inventarisasi Akses Vaskular**: Pencatatan riwayat AV Fistula (Cimino), AV Graft, dan kateter vena sentral double lumen (CDL tunneled/non-tunneled), penentuan akses primer sesi, serta pemantauan tanda fisik bruit dan thrill.
   - **Tinjauan Serologi & Protokol Isolasi PPI**: Pemantauan hasil lab virus darah (HBsAg, Anti-HCV, Anti-HIV) serta penetapan protokol pencegahan infeksi (PPI) dan alokasi mesin dialisis terdedikasi.
4. Tidak ada mekanisme otomatis untuk mencegah kebocoran data antar pasien (*stale state data leak* / `NFR-010`) saat pengguna berpindah halaman dari Pasien A ke Pasien B pada browser.
5. Belum ada gerbang otorisasi klinis (`AC-1`) untuk memblokir staf yang tidak berwenang dari data hasil uji serologi yang bersifat sangat rahasia.

---

## 2. Proses Bisnis dari Sisi Pengguna (User Journey)

### 2.1 Penilaian Kelayakan Medis Hemodialisa (DPJP Nefrologi) & Validasi HMD-VAL-011
*Contoh Skenario Rumah Sakit:*
> Pasien **Bapak Rahmad (No. RM 01-99-88)** dirujuk dari rawat inap bangsal penyakit dalam dengan diagnosis Gagal Ginjal Kronik (GGK) stadium 5, kadar ureum 210 mg/dL, kreatinin 12.4 mg/dL, serta terdapat edema paru akut. Dokter Sp.PD-KGH melakukan asesmen kelayakan untuk menentukan apakah pasien dapat segera menjalani hemodialisis reguler atau membutuhkan stabilisasi terlebih dahulu.

1. Dokter DPJP membuka ruang kerja episode Bapak Rahmad dan memilih tab navigasi **Penilaian Kelayakan**.
2. Layar menyajikan riwayat asesmen sebelumnya dalam bentuk tabel interaktif lengkap dengan lencana status (*Layak Hemodialisis*, *Layak Bersyarat*, *Tidak Layak*, *Kontraindikasi Sementara*).
3. Dokter menekan tombol **`+ Nilai Kelayakan Baru`**:
   - Sistem menampilkan modal formulir asesmen kelayakan klinis.
   - Dokter mengisi nama DPJP, tanggal asesmen, ringkasan indikasi klinis (*"GGK Stage 5 dengan ensefalopati uremikum dan fluid overload"*), serta memilih keputusan kelayakan.
   - **Penegakan Aturan Integritas HMD-VAL-011**: Jika dokter memilih hasil *Tidak Layak* atau *Kontraindikasi Sementara*, sistem mewajibkan pengisian alasan klinis / kontraindikasi (misal: *"Hipotensi refrakter dengan MAP < 50 mmHg memerlukan inotropik di ICU sebelum HD"*). Form akan menolak penyimpanan jika kontraindikasi dikosongkan.
4. Setelah disimpan, data riwayat asesmen langsung diperbarui secara reaktif, dan kartu metrik ringkasan kelayakan terkini langsung berganti warna sesuai hasil telaah dokter.

---

### 2.2 Inventarisasi Akses Vaskular & Pemantauan Bruit / Thrill
*Contoh Skenario Rumah Sakit:*
> Bapak Rahmad telah menjalani operasi pembuatan Cimino (AV Fistula) radiocephalic sinistra 2 bulan yang lalu oleh dokter bedah vaskular, dan saat ini juga memiliki kateter CDL di vena jugularis dextra yang masih aktif.

1. Perawat dialisis atau dokter beralih ke tab **Akses Vaskular**.
2. Layar menyajikan kartu ringkasan:
   - **Akses Primer Sesi Saat Ini**: Menampilkan jenis akses utama yang akan dikanulasi/dihubungkan ke bloodline mesin HD.
   - **Status Kelaikan**: Lencana *Laik Pakai* (hijau), *Maturasi* (biru), *Bermasalah* (kuning), atau *Non-Fungsional* (merah).
   - **Total Akses Terdaftar**: Menghitung seluruh akses vaskular yang pernah dibuat.
3. Perawat dapat menekan **`+ Tambah Akses Vaskular`** untuk mendaftarkan akses baru:
   - Memilih tipe akses (`AV Fistula / Cimino`, `AV Graft`, `CDL Tunneled / Permacath`, `CDL Non-Tunneled / Sementara`).
   - Memilih sisi tubuh (`Sinistra / Kiri` atau `Dextra / Kanan`) dan lokasi anatomis spesifik.
   - Mengisi tanggal pemasangan/operasi dan operator bedah.
   - Memasukkan hasil pemeriksaan fisik: tanda centang **Bruit terdengar jelas (auskultasi)** dan **Thrill teraba kuat (palpasi)**.
   - Menandai opsi *"Jadikan sebagai Akses Vaskular Primer"* jika akses tersebut siap digunakan untuk penarikan darah dialisis.
4. Jika di kemudian hari akses mengalami komplikasi (misalnya timbul hematom atau aliran darah melemah), perawat/dokter menekan tombol **`Ubah Status`** pada baris akses untuk mengganti status kelaikan menjadi *Bermasalah* atau *Maturasi* beserta alasan klinisnya.

---

### 2.3 Perlindungan Privasi Serologi Virus Darah (AC-1)
*Aturan Keselamatan Pasien & Kerahasiaan Medis:*
> Hasil pemeriksaan laboratorium untuk virus yang ditularkan lewat darah (Hepatitis B, Hepatitis C, HIV) merupakan data medis dengan klasifikasi kerahasiaan tertinggi. Hanya Dokter Nefrologi dan Tim Klinis Dialisis Berwenang yang memiliki wewenang `HemodialysisSerology:Read` yang diizinkan melihat lembar hasil dan titer antibodi/antigen.

1. **Skenario Staf Non-Klinis / Administrasi Umum**:
   - Jika pengguna yang sedang login tidak memiliki hak akses `HemodialysisSerology:Read` membuka tab *Serologi & Isolasi PPI*, sistem **memblokir seluruh konten data**.
   - Sistem menampilkan kartu peringatan khusus dengan latar belakang kuning hangat dan ikon gembok:
     > **Akses Dibatasi — Privasi Medis Serologi**
     > *"Anda tidak memiliki wewenang untuk melihat data serologi pasien ini. Data pengujian virus darah (HBsAg, Anti-HCV, Anti-HIV) dilindungi secara ketat dan hanya dapat diakses oleh Dokter Nefrologi dan Tim Klinis Berwenang."*
   - Tidak ada baris tabel, nilai kuantitatif, nama laboratorium, ataupun tombol form yang dimuat atau dibocorkan.
2. **Skenario Dokter DPJP / Tim Klinis Berwenang**:
   - Dokter yang memiliki wewenang `HemodialysisSerology:Read` dapat melihat daftar lengkap riwayat pengujian HBsAg, Anti-HCV, dan Anti-HIV lengkap dengan tanggal tes, hasil kualitatif (Reaktif / Non-Reaktif), nilai rasio titer S/CO, dan laboratorium penguji.
   - Dokter dapat menekan **`Rujukan Lab Baru`** untuk mencatat hasil uji serologi berkala (skrining rutin tiap 3-6 bulan sesuai standar PERNEFRI).

---

### 2.4 Keputusan Protokol Isolasi PPI & Alokasi Mesin Terdedikasi
*Contoh Skenario Rumah Sakit:*
> Hasil pemeriksaan serologi pasien Ibu Sinta menunjukkan HBsAg Reaktif. Berdasarkan pedoman Pencegahan dan Pengendalian Infeksi (PPI) Rumah Sakit dan PERNEFRI, pasien tidak boleh menggunakan mesin hemodialisis reguler dan harus menggunakan mesin yang terisolasi khusus Hepatitis B.

1. Dokter Sp.PD-KGH yang memiliki wewenang `HemodialysisIsolation:Decide` menekan tombol **`Tetapkan Isolasi PPI`**.
2. Modal formulir keputusan isolasi ditampilkan:
   - Dokter memilih kebutuhan protokol isolasi:
     - `Standar / Tidak Perlu Isolasi` (untuk pasien non-reaktif)
     - `Isolasi Hepatitis B (Wajib Mesin Khusus Hep B)`
     - `Isolasi Hepatitis C (Mesin Didedikasikan)`
     - `Isolasi HIV (Kewaspadaan Standar / Terdedikasi)`
     - `Isolasi Transmisi Airborne (Kasus TB Aktif / Tekanan Negatif)`
     - `Isolasi Kontak (MRSA / VRE)`
   - Sistem mewajibkan pengisian **Alokasi Mesin Khusus** (misal: *"Mesin HD-04 Ruang Isolasi B"*) dan **Justifikasi Klinis PPI** jika pasien membutuhkan isolasi.
3. Keputusan isolasi ini langsung tersimpan ke rekam medis episode dan menjadi dasar sistem validasi jadwal untuk melarang penempatan pasien di stasiun dialisis reguler.

---

### 2.5 Mitigasi Kebocoran Data Basi antar Pasien (AC-2 / NFR-010)
*Aturan Keselamatan Medis Anti-Stale Data:*
> Saat pengguna berpindah membuka berkas dari Pasien A ke Pasien B pada URL (misal mengeklik rujukan pasien lain di bilah samping atau memasukkan URL langsung), data Pasien A tidak boleh tertinggal sekejap pun pada layar Pasien B.

1. Komponen `HemodialysisEpisodeWorkspaceView` mengimplementasikan pembersihan state reaktif (`resetEpisodeWorkspaceState()`) di awal efek pemanggilan serta pada fungsi *cleanup*:
   ```javascript
   useEffect(() => {
     dispatch(resetEpisodeWorkspaceState());
     if (patientId && episodeId) {
       dispatch(fetchEpisodeWorkspaceBundle({ patientId, episodeId }));
     }
     return () => {
       dispatch(resetEpisodeWorkspaceState());
     };
   }, [dispatch, patientId, episodeId]);
   ```
2. Seluruh data asesmen kelayakan, akses vaskular, serologi, dan isolasi langsung kembali ke array kosong dan menampilkan kerangka skeleton bersih sebelum data Pasien B tiba dari jaringan. Tidak ada risiko dokter keliru membaca data serologi Pasien A sebagai kondisi klinis Pasien B.

---

## 3. Komponen Antarmuka & Keputusan UI Gate

Implementasi 100% menggunakan kembali base component Quilvian yang sudah terbukti stabil (**REUSE 10, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0**):

| No | Elemen Antarmuka | Pustaka Asal | Status UI Gate | Peran dalam Layar |
| :---: | :--- | :--- | :---: | :--- |
| 1 | `ClinicalWorkspaceShell` | `@/components/ui/clinical-workspace` | `REUSE` | Wadah layout standar 4 wilayah Shared Clinical Workspace |
| 2 | `PatientContextHeader` | `@/components/ui/clinical-workspace` | `REUSE` | Bilah identitas pasien sticky di puncak layar dengan No. RM, DPJP, dan status |
| 3 | `ClinicalSectionNav` | `@/components/ui/clinical-workspace` | `REUSE` | Navigasi vertikal 3 sub-seksi klinis (Kelayakan, Akses, Serologi/Isolasi) |
| 4 | `ClinicalContentPanel` | `@/components/ui/clinical-workspace` | `REUSE` | Panel wadah konten dokumentasi klinis dengan judul dinamis |
| 5 | `ClinicalStateBoundary`| `@/components/ui/clinical-workspace` | `REUSE` | Penanganan 4 kondisi deterministik (*loading, error, empty, content*) |
| 6 | `DataTable` | `@/components/features/base-features/data-table` | `REUSE` | Tabel data riwayat asesmen, inventaris akses vaskular, serologi, dan isolasi |
| 7 | `BaseButton` | `@/components/features/base-features/base-button` | `REUSE` | Tombol aksi primer, sekunder, ikon, dan status submitting |
| 8 | `InformationAlert` | `@/components/features/base-features/information-alert` | `REUSE` | Kotak notifikasi kesalahan submit pada modal formulir |
| 9 | `Modal` | `react-bootstrap` | `REUSE` | Dialog formulir tambah kelayakan, tambah akses, ubah status, rujukan serologi, dan penetapan isolasi |
| 10 | `StatusBadge` | `@/components/features/base-features/status-badge` | `REUSE` | Penanda visual status kelaikan akses dan kelayakan klinis |

---

## 4. Arsitektur State Management & Redux Slice

State ruang kerja episode dikelola secara terpusat pada `hemodialysisEpisodeSlice.js`:

```javascript
// State Ruang Kerja Episode (FE-HMD-10)
workspaceEpisode: null,
eligibilityAssessments: [],
vascularAccesses: [],
serologyReviews: [],
isolationDecisions: [],
activeSection: "eligibility",
workspaceLoading: false,
workspaceSubmitting: false,
workspaceError: null,
serologyPermissionDenied: false,
```

### Thunk Async & Reducers
1. `resetEpisodeWorkspaceState`: Mengembalikan seluruh state ruang kerja ke kondisi awal (null / array kosong) untuk menegakkan `AC-2 / NFR-010`.
2. `setActiveWorkspaceSection`: Mengubah sub-seksi aktif antara `eligibility`, `vascular`, dan `serology_isolation`.
3. `fetchEpisodeWorkspaceBundle`: Mengambil data episode induk, riwayat kelayakan, dan akses vaskular secara paralel. Jika panggilan serologi/isolasi mengembalikan HTTP 403 Forbidden, thunk secara cerdas menandai `serologyPermissionDenied: true` tanpa menggagalkan pemuatan data kelayakan dan akses vaskular.
4. `createEligibilityAssessmentAction`: Menyimpan evaluasi kelayakan baru oleh DPJP.
5. `createVascularAccessAction`: Mendaftarkan akses vaskular baru ke inventaris pasien.
6. `changeVascularAccessStatusAction`: Memperbarui status kelaikan akses vaskular.
7. `createSerologyReviewAction`: Mencatat hasil uji laboratorium virus darah.
8. `createIsolationDecisionAction`: Menyimpan penetapan protokol isolasi PPI dan alokasi mesin.

---

## 5. Endpoint API Swagger-Style

Semua kontrak integrasi API telah terkunci pada `HMD-CONTRACT-v1`:

| Method | Path | Tag Swagger | Deskripsi | Otorisasi Wajib | Payload / Respon |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/hemodialysis/patients/{patientId}/episodes/{episodeId}/eligibility` | `[Tags("Hemodialysis Eligibility")]` | Mengambil daftar riwayat asesmen kelayakan pasien | `HemodialysisEpisode:Read` | Array of `HmdEligibilityAssessmentDto` |
| `POST` | `/api/v1/hemodialysis/patients/{patientId}/episodes/{episodeId}/eligibility` | `[Tags("Hemodialysis Eligibility")]` | Mencatat penilaian kelayakan klinis oleh DPJP | `HemodialysisEpisode:Write` | `CreateHmdEligibilityRequest` -> `Guid` |
| `GET` | `/api/v1/hemodialysis/patients/{patientId}/episodes/{episodeId}/vascular-access` | `[Tags("Hemodialysis Vascular Access")]` | Mengambil inventaris akses vaskular pasien | `HemodialysisEpisode:Read` | Array of `HmdVascularAccessDto` |
| `POST` | `/api/v1/hemodialysis/patients/{patientId}/episodes/{episodeId}/vascular-access` | `[Tags("Hemodialysis Vascular Access")]` | Mendaftarkan akses vaskular baru | `HemodialysisEpisode:Write` | `CreateHmdVascularAccessRequest` -> `Guid` |
| `PATCH` | `/api/v1/hemodialysis/patients/{patientId}/episodes/{episodeId}/vascular-access/{accessId}/status` | `[Tags("Hemodialysis Vascular Access")]` | Mengubah status kelaikan akses vaskular | `HemodialysisEpisode:Write` | `ChangeVascularAccessStatusRequest` |
| `GET` | `/api/v1/hemodialysis/patients/{patientId}/episodes/{episodeId}/serology` | `[Tags("Hemodialysis Serology")]` | Mengambil riwayat pengujian serologi virus darah | `HemodialysisSerology:Read` (Ketat) | Array of `HmdSerologyReviewDto` |
| `POST` | `/api/v1/hemodialysis/patients/{patientId}/episodes/{episodeId}/serology` | `[Tags("Hemodialysis Serology")]` | Mencatat rujukan hasil uji laboratorium | `HemodialysisSerology:Write` | `CreateHmdSerologyReviewRequest` -> `Guid` |
| `GET` | `/api/v1/hemodialysis/patients/{patientId}/episodes/{episodeId}/isolation` | `[Tags("Hemodialysis Isolation")]` | Mengambil riwayat keputusan isolasi PPI | `HemodialysisEpisode:Read` | Array of `HmdIsolationDecisionDto` |
| `POST` | `/api/v1/hemodialysis/patients/{patientId}/episodes/{episodeId}/isolation` | `[Tags("Hemodialysis Isolation")]` | Menetapkan protokol isolasi PPI & mesin khusus | `HemodialysisIsolation:Decide` | `CreateHmdIsolationDecisionRequest` -> `Guid` |

---

## 6. Hasil Pengujian & Bukti Validasi

### 6.1 Unit Test Spesifik Task `FE-HMD-10`
Dijalankan via Node.js Test Runner:
```bash
node --import ./tests/helpers/register.mjs --test tests/unit/hemodialysis-episode-workspace.test.mjs
```
Hasil: **7 dari 7 pengujian lolos (100%)**:
- ✔ `FE-HMD-10 File Integrity`: Seluruh berkas ruang kerja klinis episode, sub-seksi, stylesheet, dan App Router tersedia
- ✔ `FE-HMD-10 AC-1 Perlindungan Privasi Serologi`: Pengecekan otorisasi `HemodialysisSerology:Read` dan blokir akses staf tanpa izin
- ✔ `FE-HMD-10 AC-2 Mitigasi Data Basi (NFR-010)`: `resetEpisodeWorkspaceState` membersihkan state saat berganti pasien
- ✔ `FE-HMD-10 Resolver & Validasi Kelayakan`: Menangani outcome badge dan validasi `HMD-VAL-011`
- ✔ `FE-HMD-10 Resolver & Validasi Akses Vaskular`: Tipe akses, status kelaikan, dan form validation
- ✔ `FE-HMD-10 Resolver & Validasi Serologi dan Isolasi PPI`: Resolver status serologi & form isolasi
- ✔ `FE-HMD-10 Redux Reducer`: Navigasi sub-seksi aktif via `setActiveWorkspaceSection`

### 6.2 Uji Anti-Regresi Seluruh Modul Hemodialisa
Dijalankan terhadap seluruh suite pengujian hemodialisa:
```bash
node --import ./tests/helpers/register.mjs --test tests/unit/hemodialysis-*.test.mjs
```
Hasil: **65 dari 65 pengujian lolos (100% PASS)** tanpa ada regresi pada task sebelumnya (`FE-HMD-01` s.d. `FE-HMD-09`).

### 6.3 Pemeriksaan Linter (ESLint)
Dijalankan terhadap seluruh berkas yang disentuh:
```bash
node ./node_modules/eslint/bin/eslint.js "src/utils/health-services/hemodialysis-management/hemodialysis-patients-display-utils.js" "src/lib/state/slice/health-services/hemodialysis-management/hemodialysisEpisodeSlice.js" "src/components/view/health-services/hemodialysis-management/patients/workspace/**/*.jsx" "src/app/health-services/hemodialysis-management/patients/[patientId]/episodes/[episodeId]/page.jsx"
```
Hasil: **0 error, 0 warning** (bersih sempurna).

---

## 7. Status Traceability Matrix

| Requirement ID | Deskripsi Kebutuhan | Komponen Frontend | Pengujian Otomatis | Status |
| :--- | :--- | :--- | :--- | :---: |
| `FR-HMD-011` | Evaluasi kelayakan klinis hemodialisa oleh DPJP | `eligibility-section.jsx` | `hemodialysis-episode-workspace.test.mjs` | ✅ COMPLETED |
| `FE-HMD-06` | Ruang kerja episode klinis wireframe 3.3 | `hemodialysis-episode-workspace-view.jsx` | `hemodialysis-episode-workspace.test.mjs` | ✅ COMPLETED |
| `CAP-28` | Pencatatan kelayakan klinis & kontraindikasi | `eligibility-section.jsx` | `hemodialysis-episode-workspace.test.mjs` | ✅ COMPLETED |
| `CAP-29` | Inventarisasi akses vaskular & kelaikan | `vascular-access-section.jsx` | `hemodialysis-episode-workspace.test.mjs` | ✅ COMPLETED |
| `CAP-30` | Pemantauan serologi virus darah | `serology-isolation-section.jsx` | `hemodialysis-episode-workspace.test.mjs` | ✅ COMPLETED |
| `CAP-31` | Penetapan isolasi PPI & alokasi mesin | `serology-isolation-section.jsx` | `hemodialysis-episode-workspace.test.mjs` | ✅ COMPLETED |
| `NFR-006` / `AC-1` | Batasan privasi serologi ketat | `serology-isolation-section.jsx` | `hemodialysis-episode-workspace.test.mjs` | ✅ COMPLETED |
| `NFR-010` / `AC-2` | Mitigasi stale state antar pasien | `hemodialysisEpisodeSlice.js` | `hemodialysis-episode-workspace.test.mjs` | ✅ COMPLETED |
| `HMD-VAL-011` | Kontraindikasi wajib jika ineligble | `hemodialysis-patients-display-utils.js` | `hemodialysis-episode-workspace.test.mjs` | ✅ COMPLETED |
