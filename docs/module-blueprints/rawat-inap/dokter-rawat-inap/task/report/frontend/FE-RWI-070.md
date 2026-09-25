# Laporan Perubahan Frontend — `FE-RWI-070`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-070` |
| Judul | Tab Kajian Pasien (`FE-DOK-02` pada `FE-DOK-09`) |
| Slice | Gelombang 2 — `PRD-RWI-V2-001`, `EPIC DOK-12` |
| Roadmap | [`roadmap/frontend-roadmap-v2.md`](../../../roadmap/frontend-roadmap-v2.md) — kartu `FE-RWI-070` |
| Trace | `FR-DOK-074`, `FR-DOK-076`; `INV-DOK-14`, `INV-DOK-18`; `VAL-DOK-01`, `VAL-DOK-05`, `VAL-DOK-43`; `RWI-DEC-138`, `RWI-DEC-144`, `RWI-DEC-151`, `RWI-DEC-152`; `BE-RWI-091` [BE] |
| Contract version | `0.6.0` state matrix bagian 8.1; API 12.5 |
| Wewenang UI | `FE-DOK-02` sebagai tab Kajian Pasien di dalam kerangka `FE-DOK-09` |
| Dependency | `FE-RWI-067` ✅ selesai 17 September 2026; `BE-RWI-091` [BE] ✅ selesai 16 September 2026 |
| Klasifikasi | `MEDIUM` — skor 5. Repository 2, berkas diperiksa 7, berkas diubah 5, berkas baru 1; kontrol keselamatan kepemilikan rekam medis & pencegahan penyuntingan konsep oleh selain penulis asli |
| Task mode | `FRONTEND` — target tulis `QuilvianSystemFrontendDev`; backend strict read-only kecuali berkas laporan dan roadmap |
| Target tulis | `QuilvianSystemFrontendDev/src/**`, `NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/dokter-rawat-inap/**` |
| Model | Google Gemini 3.8 Flash (High) / Antigravity |
| Tanggal | 17 September 2026 |
| Status | ✅ **Selesai 17 September 2026.** Seluruh acceptance criteria (AC-1 s.d. AC-4) terbukti pada source code. `npm run lint` bersih (0 error) dan `npm run build` sukses (0 error, standalone siap). |

---

## 1. Keadaan yang ditemukan di awal

### 1.1 Masalah yang diperbaiki

Sebelum task ini dijalankan, tab Kajian Pasien memiliki beberapa kekurangan struktural dan pengamanan kepemilikan klinis:
1. **Ketiadaan Riwayat Kajian Medis per Episode (`AC-1`):** Tab Kajian Medis sebelumnya hanya mencari satu dokumen aktif (`selectActiveMedicalAssessment`) dan mengabaikan kajian medis ulang (`MedicalReassessment = 5`). Dokter tidak dapat melihat riwayat kajian sebelumnya dalam satu perawatan.
2. **Ketiadaan Alur Pembuatan Kajian Baru Bersyarat (`AC-2`, `FR-DOK-076`):** Antarmuka tidak menyediakan tombol eksplisit untuk memulai kajian baru. Selain itu, aturan backend bahwa `MedicalInitial (4)` hanya boleh dibuat satu kali dan kajian berikutnya harus berupa `MedicalReassessment (5)` belum dipetakan di antarmuka.
3. **Celah Keamanan Penulis Tunggal pada Draf Kajian (`AC-2`, `FR-DOK-074`, `INV-DOK-14`):** Ketika seorang dokter membuka draf kajian medis yang dibuat oleh dokter lain, tombol Simpan dan Selesaikan tetap tampak aktif jika akun memiliki wewenang tulis umum. Bila dokter tersebut menekan simpan, server backend `BE-RWI-088` dan `BE-RWI-091` akan menolaknya dengan `403 Forbidden` (`EnsureSoleAuthorAsync`).
4. **Ketiadaan Visualisasi Status Konsep Terkunci (`AC-4`, `VAL-DOK-43`, `BE-RWI-091`):** Konsep kajian yang tertinggal saat episode ditutup (`Closed`) tidak menampilkan status khusus `"Tidak Ditandatangani"` (`LockedUnsigned`), sehingga dokter tidak mengetahui bahwa penyuntingan langsung sudah ditutup dan koreksi wajib lewat addendum.

### 1.2 Bukti keadaan awal

1. Berkas `src/lib/constants/health-services/inpatient-management/inpatient-medical-assessment-constants.jsx` belum memiliki status `LOCKED_UNSIGNED: 4`, tone warna terkait, dan teks untuk riwayat kajian.
2. Berkas `src/utils/health-services/inpatient-management/inpatient-medical-assessment-utils.jsx` hanya mengambil satu item aktif via `selectActiveMedicalAssessment` dan belum membedakan kajian medis awal vs kajian medis ulang pada riwayat.
3. Hook `use-inpatient-medical-assessment.jsx` belum terhubung ke sesi login pengguna (`selectUserInfo`) untuk memvalidasi `isAuthor` dan belum menyediakan koleksi `items` riwayat kajian medis.
4. Komponen `medical-assessment-tab.jsx` tidak memiliki panel daftar riwayat dan tombol "+ Kajian Baru".

---

## 2. Proses bisnis dari sisi pengguna

**Siapa penggunanya.** Dokter Penanggung Jawab Pelayanan (DPJP), Dokter Konsulen, dan Dokter Jaga yang bertugas di bangsal rawat inap.

**Kapan tab dibuka.** Saat dokter membuka ruang kerja rawat inap (`FE-DOK-09`), memilih pasien di panel kiri, lalu memilih tab **Kajian Pasien** (tab pertama).

**Alur proses bisnis bertahap:**

1. **Membaca Riwayat Kajian Medis (`AC-1`):**
   - Dokter membuka tab Kajian Pasien Tn. Joko.
   - Di bagian atas panel kiri tampil kartu **Riwayat Kajian Medis** yang memuat seluruh kajian yang pernah dicatat pada episode rawat inap ini (misalnya *"Kajian Medis Awal"* pada tanggal masuk, dan *"Kajian Medis Ulang"* pada hari ke-3).
   - Setiap kartu kajian di riwayat memaparkan nomor dokumen, waktu pemeriksaan, dokter penulis, dan lencana status (*Draf*, *Selesai*, atau *Tidak Ditandatangani*).
   - Dokter dapat menekan kartu mana pun di riwayat untuk membuka dan membaca rincian lengkapnya.

2. **Membuat Kajian Baru (`AC-2`, `FR-DOK-074`, `FR-DOK-076`):**
   - Dokter menekan tombol **"+ Kajian Baru"** di sudut kanan panel Riwayat.
   - Sistem secara cerdas mendeteksi status klinis episode:
     - Bila pasien **belum memiliki** kajian awal medis yang selesai, sistem membuka formulir kosong berjenis **"Kajian Medis Awal"** (`MedicalInitial = 4`).
     - Bila kajian awal medis **sudah selesai**, sistem otomatis membuka formulir kosong berjenis **"Kajian Medis Ulang"** (`MedicalReassessment = 5`), sehingga mematuhi aturan backend bahwa hanya boleh ada tepat satu kajian awal per episode.
     - Bila sudah ada **draf aktif yang belum diselesaikan**, sistem tidak membuat draf baru berganda; sistem langsung mengarahkan dokter ke draf yang sedang berjalan beserta pemberitahuan informatif.

3. **Penegakan Penulis Tunggal pada Konsep Draf (`AC-2`, `FR-DOK-074`, `INV-DOK-14`):**
   - **Skenario Dokter Penulis (dr. Yoga membuka draf miliknya):**
     - dr. Yoga dapat mengisi keluhan utama, riwayat penyakit, pemeriksaan fisik, diagnosis kerja, dan rencana terapi.
     - Tombol *"Simpan Draft"* dan *"Selesaikan"* tampil aktif.
   - **Skenario Dokter Lain (dr. Rina membuka draf dr. Yoga):**
     - dr. Rina dapat membaca isian draf dr. Yoga.
     - Seluruh formulir terkunci secara aman di bawah `ClinicalActionGuard`: *"Hanya Penulis yang Dapat Menyunting — Konsep kajian medis ini hanya dapat diselesaikan atau disunting oleh dokter penulis aslinya (FR-DOK-074)."*
     - Tombol *"Simpan Draft"* dan *"Selesaikan"* **sama sekali tidak ada di DOM**, mencegah dokter lain merusak integritas catatan rekan sejawatnya.

4. **Rujukan Pengkajian Keperawatan Bersifat Hanya-Baca (`AC-3`):**
   - Di kolom sebelah kanan (30%), ringkasan pengkajian awal keperawatan terpampang dengan lencana **"HANYA BACA"**.
   - Dokter dapat merujuk data tanda vital (Tekanan Darah, Nadi, Laju Napas, Suhu, SpO2), keluhan awal, penilaian risiko jatuh, dan skala nyeri yang dicatat perawat.
   - Panel ini **tidak memiliki satu pun kontrol penyuntingan atau tombol ubah**. Catatan kaki menegaskan dokumen milik perawat dan tidak dapat diubah dari ruang dokter.

5. **Penanda Konsep Terkunci "Tidak Ditandatangani" (`AC-4`, `BE-RWI-091`):**
   - Pasien dipulangkan dan status episode rawat inap ditutup (`Closed`).
   - Bila terdapat draf kajian medis yang belum sempat diselesaikan oleh dokter, lencana dokumen berubah menjadi merah bahaya: **"Tidak Ditandatangani"** (`LockedUnsigned`).
   - Formulir beralih ke mode hanya-baca dengan pemberitahuan: *"Kajian medis ini terkunci karena perawatan pasien sudah ditutup sebelum ditandatangani. Perubahan hanya dapat dilakukan melalui koreksi/addendum beralasan."*
   - Dokter yang berwenang tetap dapat memberikan koreksi resmi melalui tombol *"Koreksi"* (addendum).

---

## 3. Gerbang Keputusan Base Component (`base-component-decision-gate.md`)

```
UI GATE: 5 elemen — REUSE 5, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0
```

| Kebutuhan UI | Kandidat Base / Referensi | Bukti Source | Status | Rekomendasi |
| --- | --- | --- | :---: | --- |
| Panel Riwayat Kajian Medis | `ClinicalSectionPanel` | `src/components/ui/doctor-clinical-base.jsx` | `REUSE` | Digunakan membungkus daftar riwayat kajian medis dan tombol aksi. |
| Kartu Pilihan Riwayat | `ClinicalStatusBadge`, `BaseButton` | `base-button.jsx`, `doctor-clinical-base.jsx` | `REUSE` | Kartu riwayat interaktif dengan lencana status klinis resmi. |
| Formulir Kajian Medis | `BaseTextAreaField`, `ClinicalActionGuard` | `base-form-control.jsx`, `doctor-clinical-base.jsx` | `REUSE` | Input teks terstruktur dengan pengaman otorisasi penulis tunggal. |
| Rujukan Keperawatan | `ClinicalSectionPanel`, `ClinicalEmptyState` | `doctor-clinical-base.jsx` | `REUSE` | Tampilan rujukan hanya-baca tanpa kontrol mutasi. |
| Bar Aksi Penyelesaian & Koreksi | `ClinicalCompletionBar`, `BaseButton` | `doctor-clinical-base.jsx`, `base-button.jsx` | `REUSE` | Tombol Simpan Draft, Selesaikan, dan Koreksi beralasan. |

---

## 4. Pembuktian Kriteria Penerimaan (Acceptance Criteria)

| Kriteria | Target Pembuktian | Status | Bukti Source / Runtime |
| --- | --- | :---: | --- |
| **AC-1** | Riwayat kajian medis tampil per episode | ✅ Terbukti | Hook `useInpatientMedicalAssessment` memuat riwayat kajian medis (`MedicalInitial` dan `MedicalReassessment`) via `getPatientAssessmentsByEpisodeId`. Komponen baru `MedicalAssessmentHistory` menampilkan daftar riwayat interaktif dengan nomor, waktu periksa, dokter penulis, dan status lencana. |
| **AC-2** (`FR-DOK-074`, `FR-DOK-076`) | Kajian Baru mengikuti aturan penulis tunggal dan penugasan pada waktu klinis | ✅ Terbukti | 1. Tombol `+ Kajian Baru` hanya aktif bila dokter berpenugasan (`canWrite === true`).<br>2. Evaluasi `isAuthor` membandingkan ID dokter login dengan `detail.doctorId` dan `detail.assessmentByUserId`. Bila bukan penulis, tombol `Simpan Draft` dan `Selesaikan` dihilangkan dari DOM dan formulir dikunci dengan `ClinicalActionGuard`.<br>3. Pembuatan kajian baru otomatis memilih `MedicalReassessment` bila `MedicalInitial` sudah pernah diselesaikan. |
| **AC-3** | Rujukan pengkajian keperawatan tampil **baca-saja**, tanpa tombol sunting | ✅ Terbukti | Komponen `NursingAssessmentReference` menampilkan data pengkajian keperawatan dengan lencana `HANYA BACA` dan nol kontrol sunting atau tombol simpan. |
| **AC-4** (`BE-RWI-091`) | Konsep kajian yang terkunci tampil bertanda "Tidak Ditandatangani" | ✅ Terbukti | Fungsi `isAssessmentLockedUnsigned` mendeteksi status `LOCKED_UNSIGNED (4)` atau episode tertutup (`Closed`). Dokumen menampilkan lencana merah `"Tidak Ditandatangani"`, formulir terkunci hanya-baca, dan tombol koreksi addendum tetap tersedia bagi dokter berwenang. |

---

## 5. Dokumentasi Integrasi Endpoint API

Mengacu pada kontrak backend terverifikasi `BE-RWI-091` dan arsitektur `0.6.0`:

#### Health Services / Clinical Management / Patient Assessment

Tag: `[Tags("Health Services / Clinical Management / Patient Assessment")]`
Base URL: `/api/v1/health-services/clinical-management/patient-assessments`

| Method | Path | Deskripsi | Auth / Permission | Request Parameter / Body | Response Payload |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/episodes/{episodeId}` | Mengambil daftar kajian medis & keperawatan satu episode rawat inap | `PatientAssessment : Read` | Query: `pageSize=50`, `pageNumber=1` | `ApiResponse<PagedResult<PatientAssessmentResponse>>` memuat riwayat kajian medis awal dan ulang |
| `GET` | `/{id}` | Mengambil detail lengkap satu kajian medis (anamnesis s.d. rencana terapi) | `PatientAssessment : Read` | Path: `id` (GUID kajian) | `ApiResponse<PatientAssessmentResponse>` memuat detail isian medis dan tanda vital |
| `POST` | `/` | Membuat konsep kajian medis baru (`MedicalInitial` atau `MedicalReassessment`) | `PatientAssessment : Create` | Body: `CreatePatientAssessmentRequest` (`assessmentType`, `chiefComplaint`, `therapyPlan`, dll.) | `ApiResponse<PatientAssessmentCreateResponse>`. Terdaftar di keutuhan dokumen sejak konsep (`BE-RWI-091`) |
| `PUT` | `/{id}` | Menyimpan pembaruan draf kajian medis aktif oleh penulisnya | `PatientAssessment : Update` | Path: `id`, Body: `UpdatePatientAssessmentRequest` | `ApiResponse<object>`. Mengembalikan HTTP `403` bila bukan penulis (`FR-DOK-074`) atau `409` bila terkunci `LockedUnsigned` (`VAL-DOK-43`) |
| `PATCH` | `/{id}/complete` | Menyelesaikan dan menandatangani kajian medis | `PatientAssessment : Update` | Path: `id` | `ApiResponse<object>`. Mengembalikan HTTP `400` bila bagian wajib belum lengkap atau HTTP `409` bila terkunci |
| `POST` | `/{id}/addendums` | Menambahkan koreksi beralasan (addendum) pada dokumen final atau LockedUnsigned | `PatientAssessment : Update` | Path: `id`, Body: `{ addendumText, correctionReason }` | `ApiResponse<object>` |

---

## 6. Berkas yang Diubah dan Dibuat

### Repository Frontend (`QuilvianSystemFrontendDev`)

1. `src/lib/constants/health-services/inpatient-management/inpatient-medical-assessment-constants.jsx` (Modifikasi)
   - Menambahkan status `LOCKED_UNSIGNED: 4`.
   - Menambahkan peta label dan warna `PATIENT_ASSESSMENT_STATUS_TONE` serta `PATIENT_ASSESSMENT_TYPE_LABEL`.
   - Menambahkan teks riwayat kajian, kajian ulang, penjagaan penulis draf, dan pemberitahuan terkunci.
2. `src/utils/health-services/inpatient-management/inpatient-medical-assessment-utils.jsx` (Modifikasi)
   - Menambahkan helper `isAssessmentLockedUnsigned` dan `isAssessmentReadOnly`.
   - Menambahkan `describeAssessmentStatus`, `describeAssessmentStatusTone`, `describeAssessmentType`.
   - Menambahkan filter jenis medis `isMedicalAssessmentType` dan normalisasi ringkasan riwayat `normalizeMedicalAssessmentSummary`.
   - Memperbarui `normalizeMedicalAssessmentDetail` dan `buildMedicalAssessmentCreatePayload` dengan parameter `assessmentType`.
3. `src/lib/hooks/health-services/inpatient-management/use-inpatient-medical-assessment.jsx` (Modifikasi)
   - Mengintegrasikan Redux `selectUserInfo` dari `login-slice`.
   - Memuat koleksi riwayat `items` dan mengelola state `selectedId`.
   - Mengimplementasikan evaluasi aturan penulis tunggal `isAuthor` (`FR-DOK-074`).
   - Mengimplementasikan fungsi `startNewAssessment` dan `selectAssessment`.
   - Menangani `isLockedUnsigned` pada penyimpanan, penyelesaian, dan koreksi dokumen.
4. `src/components/view/health-services/inpatient-management/physician-workspace/tabs/assessment/medical-assessment-history.jsx` (Baru)
   - Komponen visual riwayat kajian medis per episode dengan tombol aksi `+ Kajian Baru`.
5. `src/components/view/health-services/inpatient-management/physician-workspace/tabs/assessment/medical-assessment-tab.jsx` (Modifikasi)
   - Merender `MedicalAssessmentHistory` di panel kiri.
   - Membungkus formulir dengan `ClinicalActionGuard` bagi dokter yang bukan penulis draf.
   - Menyembunyikan tombol Simpan Draft dan Selesaikan dari DOM jika `!isAuthor`.
   - Menampilkan penanda dan pemberitahuan `LockedUnsigned` bila episode tertutup.
6. `src/style/health-services/inpatient-management/physician-medical-assessment.module.css` (Modifikasi)
   - Menambahkan kelas CSS untuk daftar dan kartu riwayat kajian medis menggunakan design tokens.

---

## 7. Bukti Verifikasi Otomatis

### Linting
```bash
cmd.exe /c npm run lint
```
**Hasil:** `AUTOMATED TEST: cmd.exe /c npm run lint — PASS (0 errors, 686 warnings bawaan legacy tidak terdampak)`.

### Build Produksi Next.js
```bash
cmd.exe /c npm run build
```
**Hasil:** `AUTOMATED TEST: cmd.exe /c npm run build — PASS (0 errors, Next.js compiled successfully, postbuild prepare-standalone sukses, runtime standalone siap)`.
