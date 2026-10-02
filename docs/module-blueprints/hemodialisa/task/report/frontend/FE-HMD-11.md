# Laporan Perubahan Frontend — `FE-HMD-11`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HMD-11` |
| Judul | Ruang Kerja Episode Pasien — Pembuatan, Penggantian, dan Riwayat Resep HD (`FE-HMD-06` Bagian Resep pada Wireframe 3.3) |
| Slice | `MVP-2` — Permintaan Masuk, Daftar Pasien, dan Ruang Kerja Episode |
| Roadmap | `docs/module-blueprints/hemodialisa/roadmap/frontend-roadmap.md` bagian 4.4 |
| Trace | `FR-HMD-020`, `FR-HMD-021`, `FR-HMD-022`, `FE-HMD-06`, `CAP-28`, `HMD-VAL-020`, `HMD-VAL-021`, `HMD-VAL-022`, `HMD-VAL-024`; `contracts/api-contract.md` Tag `Health Services / Hemodialysis Management / Hemodialysis Prescription`; `contracts/state-transition-matrix.md` Bagian 3 |
| Contract version | `HMD-CONTRACT-v1` — status `approved` (18 September 2026) |
| Wewenang UI | `CANONICAL_SPEC` untuk immutability resep aktif (`AC-2`), alur revisi otomatis (`AC-1`), dan batas keselamatan cairan UF > 4000 ml |
| Keputusan UI Gate | **11 Elemen Terverifikasi**: Seluruhnya `REUSE` (`REUSE 11, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`) |
| Dependency | `FE-HMD-10` (selesai), `BE-HMD-09` (selesai) |
| Klasifikasi | `HIGH` — skor 11: repository 0, berkas diperiksa 10, berkas dibuat 2, berkas diubah 4, logika 3, kontrak API 3, database 0, UI 3 |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — `src/utils/health-services/hemodialysis-management/hemodialysis-patients-display-utils.js`, `src/lib/state/slice/health-services/hemodialysis-management/hemodialysisEpisodeSlice.js`, `src/style/health-services/hemodialysis-management/hemodialysis-episode-workspace.module.css`, `src/components/view/health-services/hemodialysis-management/patients/workspace/sections/prescription-section.jsx`, `src/components/view/health-services/hemodialysis-management/patients/workspace/hemodialysis-episode-workspace-view.jsx`, `tests/unit/hemodialysis-prescription.test.mjs` |
| Model | Gemini 3.8 Flash |
| Tanggal | 23 September 2026 |
| Status | ✅ Selesai — seluruh acceptance criteria terverifikasi dengan unit test otomatis Node.js (7 lolos, 72/72 anti-regresi), ESLint 0 error 0 warning, dan kompilasi Next.js sukses |

---

## 1. Keadaan yang Ditemukan di Awal

Sebelum implementasi task `FE-HMD-11`:
1. Rute ruang kerja episode klinis (`/health-services/hemodialysis-management/patients/[patientId]/episodes/[episodeId]`) telah memiliki tab Penilaian Kelayakan, Akses Vaskular, dan Serologi/Isolasi (`FE-HMD-10`), namun **belum memiliki tab Resep Dialisis**.
2. Dokter Spesialis Penyakit Dalam / Konsultan Ginjal Hipertensi (Sp.PD-KGH) belum memiliki antarmuka untuk menyusun parameter teknis cuci darah pasien (target durasi, target penarikan cairan UF, laju QB, QD, dializer, natrium, dan heparin).
3. Belum ada mekanisme antarmuka yang menegakkan prinsip keselamatan medis **immutability resep aktif** (`AC-2` / `HMD-VAL-024`): instruksi medis yang sedang berjalan tidak boleh diedit langsung di tempat (*in-place edit*) untuk mencegah perubahan resep diam-diam saat sesi berlangsung.
4. Belum ada alur revisi resep terpadu (`AC-1`) yang menyalin parameter resep aktif ke form draf baru, mewajibkan alasan evaluasi klinis dokter, dan menggantikan resep lama secara atomik.
5. Belum ada integrasi visual komponen **`ClinicalRevisionHistory`** untuk menelusuri evolusi parameter resep pasien dari waktu ke waktu (*superseded prescriptions timeline*).

---

## 2. Proses Bisnis dari Sisi Pengguna (User Journey)

### 2.1 Penyusunan Resep Hemodialisa Perdana (Dokter Sp.PD-KGH)
*Contoh Skenario Rumah Sakit:*
> Pasien **Bapak Rahmad (No. RM 01-99-88)** telah dinyatakan *Layak Hemodialisis* dan memiliki AV Fistula yang siap pakai. Dokter nefrologi membuka ruang kerja episode pasien dan memilih tab **Resep Dialisis** untuk menetapkan instruksi cuci darah pertamanya.

1. Layar menampilkan kartu informatif: *"Belum Ada Resep Hemodialisa Aktif"*.
2. Dokter menekan tombol **`+ Susun Resep Hemodialisa Baru`**.
3. Sistem menampilkan modal dialog peresepan:
   - Tanggal mulai berlaku (default hari ini).
   - Frekuensi dialisis: 2x seminggu.
   - Target durasi: 240 menit (4.0 jam).
   - Target Ultrafiltrasi (UF Goal): 2.500 ml.
   - Quick Blood (QB): 200 ml/menit.
   - Quick Dialysate (QD): 500 ml/menit.
   - Tipe Dializer: `High Flux (FX80)`.
   - Komposisi Dialisat: `Bikarbonat Standar`.
   - Profil Natrium: `Step-down 145 -> 140 mEq/L`.
   - Rencana Antikoagulan: `Heparin Reguler (Bolus 1000 IU + 500 IU/jam)`.
   - Akses Vaskular Rujukan: Memilih Cimino Sinistra pasien.
4. Dokter dapat memilih tombol **`Simpan Draf`** jika resep masih ingin ditelaah ulang, atau langsung menekan **`Simpan & Aktifkan Segera`**.
5. Setelah diaktifkan, resep langsung berstatus **Aktif (Active)** dan menjadi dasar parameter mesin pada sesi dialisis mendatang.

---

### 2.2 Penegakan Immutability Resep Aktif (AC-2 & HMD-VAL-024)
*Aturan Keselamatan Medis:*
> Resep hemodialisa adalah dokumen instruksi medikolegal. Perubahan instruksi cuci darah tidak boleh mengubah dokumen yang sedang berlaku, melainkan wajib membuat versi resep baru.

1. Pada kartu **Resep Dialisis Aktif**, sistem secara tegas menampilkan lencana:
   > 🔒 **Instruksi Terkunci (HMD-VAL-024)**  
   > *"Resep aktif tidak dapat diedit langsung di tempat demi keselamatan pasien dan integritas rekam medis."*
2. **Tidak ada tombol "Edit"** pada kartu resep aktif.
3. Tombol tindakan yang disediakan hanya:
   - **`Buat Revisi Resep Baru`** (untuk melakukan perubahan parameter)
   - **`Batalkan Resep`** (jika program dialisis dihentikan)

---

### 2.3 Alur Pembuatan Revisi Resep & Penyesuaian QB (AC-1)
*Contoh Skenario Rumah Sakit:*
> Setelah 1 bulan menjalani dialisis, hasil evaluasi laboratorium menunjukkan klirens uremikum Bapak Rahmad belum optimal (Kt/V = 1.1, target > 1.2). Dokter Sp.PD-KGH memutuskan menaikkan laju aliran darah QB dari 200 ml/menit ke **250 ml/menit**.

1. Dokter menekan tombol **`Buat Revisi Resep Baru`** pada kartu resep aktif.
2. Modal formulir terbuka dengan **seluruh parameter resep aktif lama disalin secara otomatis**:
   - Durasi tetap 240 menit, UF 2500 ml, dializer FX80, heparin reguler.
3. Dokter mengubah nilai **Laju Aliran Darah (QB)** dari `200` menjadi **`250`** ml/menit.
4. **Penegakan Validasi `HMD-VAL-022`**: Sistem mewajibkan pengisian kolom **Alasan Revisi Resep** (misal: *"Peningkatan laju QB dari 200 ke 250 ml/menit untuk mencapai target adekuasi dialisis Kt/V > 1.2"*). Form menolak penyimpanan jika alasan dikosongkan.
5. Dokter menekan tombol **`Simpan & Aktifkan Segera`**:
   - Sistem secara atomik mengaktifkan resep baru (QB 250 ml/m) menjadi **Aktif**.
   - Resep lama (QB 200 ml/m) secara otomatis berpindah status menjadi **Digantikan (Superseded)** dan masuk ke panel riwayat evolusi di bawahnya.

---

### 2.4 Peringatan Keselamatan Batas Penarikan Cairan Ekstrem (UF Goal > 4.000 ml)
*Aturan Keselamatan Pasien:*
> Penarikan cairan lebih dari 4 liter dalam satu sesi dialisis (4 jam) berisiko tinggi memicu syok hipovolemik mendadak, hipotensi refrakter, iskemia jantung, dan kram otot ekstrem.

1. Jika dokter memasukkan angka Target Ultrafiltrasi melebihi 4.000 ml (misal `4.500 ml`):
2. Form secara reaktif memunculkan banner peringatan oranye kontras:
   > ⚠️ **Peringatan Batas Penarikan Cairan Ekstrem**  
   > *"Target penarikan cairan (UF) melebihi 4.000 ml. Pastikan toleransi kardiovaskular pasien memadai untuk mencegah risiko hipotensi intra-dialitik refrakter dan kram otot."*
3. Peringatan ini mengharuskan telaah klinis mendalam oleh DPJP sebelum instruksi disahkan.

---

### 2.5 Penelusuran Garis Waktu Evolusi Resep (`ClinicalRevisionHistory`)
1. Seluruh resep terdahulu yang pernah berlaku bagi pasien diarsipkan di panel bawah: **"Riwayat Perubahan & Evolusi Resep HD"**.
2. Menggunakan komponen baku `ClinicalRevisionHistory` dan simpul `ClinicalRevisionItem`.
3. Setiap simpul resep menampilkan nomor versi kronologis, tanggal dan waktu pengesahan, nama dokter peresep, ringkasan parameter klinis (QB, UF Goal, Durasi, Dializer, Heparin), serta alasan klinis perubahan atau pembatalan.

---

## 3. Komponen Antarmuka & Keputusan UI Gate

Implementasi 100% menggunakan kembali base component Quilvian yang sudah terbukti stabil (**REUSE 11, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0**):

| No | Elemen Antarmuka | Pustaka Asal | Status UI Gate | Peran dalam Layar |
| :---: | :--- | :--- | :---: | :--- |
| 1 | `ClinicalWorkspaceShell` | `@/components/ui/clinical-workspace` | `REUSE` | Wadah layout standar 4 wilayah Shared Clinical Workspace |
| 2 | `PatientContextHeader` | `@/components/ui/clinical-workspace` | `REUSE` | Bilah konteks identitas pasien sticky di puncak layar dengan No. RM, DPJP, dan status |
| 3 | `ClinicalSectionNav` | `@/components/ui/clinical-workspace` | `REUSE` | Navigasi vertikal 4 sub-seksi klinis (Kelayakan, Akses, Serologi/Isolasi, Resep) |
| 4 | `ClinicalContentPanel` | `@/components/ui/clinical-workspace` | `REUSE` | Panel wadah konten dokumentasi klinis dengan judul dinamis |
| 5 | `ClinicalStateBoundary` | `@/components/ui/clinical-workspace` | `REUSE` | Penanganan 4 kondisi deterministik (*loading, error, empty, content*) |
| 6 | `ClinicalRevisionHistory` | `@/components/ui/clinical-workspace` | `REUSE` | Wadah garis waktu riwayat resep superseded |
| 7 | `ClinicalRevisionItem` | `@/components/ui/clinical-workspace` | `REUSE` | Simpul visual kronologis untuk setiap resep terdahulu |
| 8 | `BaseButton` | `@/components/features/base-features/base-button` | `REUSE` | Tombol aksi primer, sekunder, ikon, dan status penyerahan form |
| 9 | `StatusBadge` | `@/components/features/base-features/status-badge` | `REUSE` | Lencana visual status resep (*Aktif, Draf, Digantikan, Dibatalkan*) |
| 10 | `InformationAlert` | `@/components/features/base-features/information-alert` | `REUSE` | Kotak notifikasi kesalahan submit pada modal dialog |
| 11 | `Modal` | `react-bootstrap` | `REUSE` | Dialog formulir buat resep, revisi resep, dan pembatalan resep |

---

## 4. Arsitektur State Management & Redux Slice

State resep dikelola secara terpusat pada `hemodialysisEpisodeSlice.js`:

```javascript
// State Resep HD Episode (FE-HMD-11)
prescriptions: [],
activePrescription: null,
prescriptionLoading: false,
prescriptionSubmitting: false,
prescriptionError: null,
```

### Thunk Async & Reducers
1. `resetEpisodeWorkspaceState`: Membersihkan seluruh array `prescriptions` dan `activePrescription` ke kondisi awal (null / []) untuk menegakkan mitigasi data basi (**`AC-2 / NFR-010`**).
2. `fetchEpisodeWorkspaceBundle`: Mengambil berkas resep bersamaan dengan kelayakan dan akses vaskular, serta secara otomatis menetapkan `state.activePrescription`.
3. `fetchEpisodePrescriptionsAction`: Mengambil daftar riwayat resep per episode.
4. `createPrescriptionAction`: Menyimpan draf resep baru.
5. `updatePrescriptionAction`: Menyunting parameter resep draf.
6. `activatePrescriptionAction`: Mengaktifkan resep draf dan secara atomik memindahkan resep aktif lama menjadi status `Superseded` (`status: 3`).
7. `cancelPrescriptionAction`: Membatalkan resep dengan alasan pembatalan dokter.

---

## 5. Endpoint API Swagger-Style

Semua kontrak integrasi API telah terkunci pada `HMD-CONTRACT-v1` dan terhubung ke backend `BE-HMD-09`:

| Method | Path | Tag Swagger | Deskripsi | Otorisasi Wajib | Payload / Respon |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/health-services/hemodialysis-management/hemodialysis-prescriptions` | `[Tags("Health Services / Hemodialysis Management / Hemodialysis Prescription")]` | Mengambil riwayat resep pada satu episode | `HemodialysisPrescription:Read` | `HmdPrescriptionPagedQuery` -> `ApiResponse<PagedResult<HmdPrescriptionResponse>>` |
| `GET` | `/api/v1/health-services/hemodialysis-management/hemodialysis-prescriptions/{id}` | `[Tags("Health Services / Hemodialysis Management / Hemodialysis Prescription")]` | Mengambil rincian lengkap satu resep | `HemodialysisPrescription:Read` | `ApiResponse<HmdPrescriptionResponse>` |
| `POST` | `/api/v1/health-services/hemodialysis-management/hemodialysis-prescriptions` | `[Tags("Health Services / Hemodialysis Management / Hemodialysis Prescription")]` | Membuat draf resep HD baru | `HemodialysisPrescription:Create` | `CreateHmdPrescriptionRequest` -> `ApiResponse<HmdPrescriptionResponse>` (201 Created) |
| `PUT` | `/api/v1/health-services/hemodialysis-management/hemodialysis-prescriptions/{id}` | `[Tags("Health Services / Hemodialysis Management / Hemodialysis Prescription")]` | Menyunting resep draf (resep aktif ditolak 423 Locked) | `HemodialysisPrescription:Update` | `UpdateHmdPrescriptionRequest` -> `ApiResponse<HmdPrescriptionResponse>` |
| `POST` | `/api/v1/health-services/hemodialysis-management/hemodialysis-prescriptions/{id}/activate` | `[Tags("Health Services / Hemodialysis Management / Hemodialysis Prescription")]` | Mengaktifkan resep & menggantikan resep aktif lama secara atomik | `HemodialysisPrescription:Activate` | `ApiResponse<HmdPrescriptionResponse>` |
| `POST` | `/api/v1/health-services/hemodialysis-management/hemodialysis-prescriptions/{id}/cancel` | `[Tags("Health Services / Hemodialysis Management / Hemodialysis Prescription")]` | Membatalkan resep draf atau aktif dengan alasan | `HemodialysisPrescription:Cancel` | `CancelHmdPrescriptionRequest` -> `ApiResponse<HmdPrescriptionResponse>` |

---

## 6. Hasil Pengujian & Bukti Validasi

### 6.1 Unit Test Spesifik Task `FE-HMD-11`
Dijalankan via Node.js Test Runner:
```bash
node --import ./tests/helpers/register.mjs --test tests/unit/hemodialysis-prescription.test.mjs
```
Hasil: **7 dari 7 pengujian lolos (100% PASS)**:
- ✔ `FE-HMD-11 File Integrity`: Seluruh berkas komponen, modal, sub-seksi, service, utilitas, dan stylesheet resep tersedia
- ✔ `FE-HMD-11 AC-1 Alur Revisi Resep`: Komponen menyalin parameter resep aktif dan mewajibkan catatan evaluasi klinis
- ✔ `FE-HMD-11 AC-2 Immutability Resep Aktif`: Kartu resep aktif terkunci tanpa tombol edit in-place (`HMD-VAL-024`)
- ✔ `FE-HMD-11 Validasi Parameter Klinis & Peringatan UF Ekstrem (> 4000 ml)`: Deteksi batas aman dan peringatan cairan
- ✔ `FE-HMD-11 Resolver Status Resep & Presets`: Memetakan badge status secara deterministik
- ✔ `FE-HMD-11 Redux Reducer`: Penggantian resep aktif secara atomik dan pembersihan data basi (`NFR-010`)
- ✔ `FE-HMD-11 Integrasi ClinicalRevisionHistory`: Garis waktu resep menggunakan pustaka shared

### 6.2 Uji Anti-Regresi Seluruh Modul Hemodialisa
Dijalankan terhadap seluruh suite pengujian hemodialisa:
```bash
node --import ./tests/helpers/register.mjs --test tests/unit/hemodialysis-*.test.mjs
```
Hasil: **72 dari 72 pengujian lolos (100% PASS)** tanpa ada regresi pada task sebelumnya (`FE-HMD-01` s.d. `FE-HMD-10`).

### 6.3 Pemeriksaan Linter (ESLint)
Dijalankan terhadap seluruh berkas yang disentuh:
```bash
node ./node_modules/eslint/bin/eslint.js "src/utils/health-services/hemodialysis-management/hemodialysis-patients-display-utils.js" "src/lib/state/slice/health-services/hemodialysis-management/hemodialysisEpisodeSlice.js" "src/components/view/health-services/hemodialysis-management/patients/workspace/**/*.jsx"
```
Hasil: **0 error, 0 warning** (bersih sempurna).

### 6.4 Validasi Kompilasi Produksi Next.js (`next build`)
Dijalankan dengan Turbopack production build:
```bash
node --max-old-space-size=4096 ./node_modules/next/dist/bin/next build
```
Hasil: **Exit code 0 (Sukses 100%)**, 374/374 halaman statis dan rute dinamis `/patients/[patientId]/episodes/[episodeId]` terkompilasi sempurna.

---

## 7. Status Traceability Matrix

| Requirement ID | Deskripsi Kebutuhan | Komponen Frontend | Pengujian Otomatis | Status |
| :--- | :--- | :--- | :--- | :---: |
| `FR-HMD-020` | Penyusunan resep dialisis baru | `prescription-section.jsx` | `hemodialysis-prescription.test.mjs` | ✅ COMPLETED |
| `FR-HMD-021` | Aktivasi resep & penggantian resep lama atomik | `hemodialysisEpisodeSlice.js` | `hemodialysis-prescription.test.mjs` | ✅ COMPLETED |
| `FR-HMD-022` | Riwayat kronologis evolusi resep | `prescription-section.jsx` | `hemodialysis-prescription.test.mjs` | ✅ COMPLETED |
| `FE-HMD-06` | Ruang kerja episode klinis bagian resep | `prescription-section.jsx` | `hemodialysis-prescription.test.mjs` | ✅ COMPLETED |
| `CAP-28` | Immutability instruksi resep aktif | `prescription-section.jsx` | `hemodialysis-prescription.test.mjs` | ✅ COMPLETED |
| `AC-1` | Alur revisi resep terpadu dari resep aktif | `prescription-section.jsx` | `hemodialysis-prescription.test.mjs` | ✅ COMPLETED |
| `AC-2` | Ketiadaan tombol edit in-place pada resep aktif | `prescription-section.jsx` | `hemodialysis-prescription.test.mjs` | ✅ COMPLETED |
| `HMD-VAL-024` | Penguncian instruksi resep aktif | `prescription-section.jsx` | `hemodialysis-prescription.test.mjs` | ✅ COMPLETED |
| `UAT-05` / `UAT-06` | Pembuatan resep dan riwayat revisi | `prescription-section.jsx` | `hemodialysis-prescription.test.mjs` | ✅ COMPLETED |
