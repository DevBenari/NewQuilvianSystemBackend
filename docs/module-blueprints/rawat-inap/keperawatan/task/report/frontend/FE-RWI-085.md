# Laporan Perubahan Frontend — `FE-RWI-085`

## Metadata

| Field | Nilai |
| --- | --- |
| **Task ID** | `FE-RWI-085` |
| **Judul** | Evaluasi Awal Manajer Pelayanan Pasien (MPP) 8 Bagian Checklist |
| **Slice** | Gelombang 2/3 — `FE-KEP-11` Evaluasi Awal (Layar baru terpadu) |
| **Roadmap** | [`../../../roadmap/frontend-roadmap-v2.md`](../../../roadmap/frontend-roadmap-v2.md), Bagian 2 & Kartu `FE-RWI-085` |
| **Traceability** | `FR-KEP-053` (dokumen Evaluasi Awal 8 bagian checklist), `FR-KEP-054` (penjaga hak akses MPP & penempatan unit), `FR-KEP-055` (satu dokumen hidup per episode, addendum menunggu `INT-KEP-12`); `RWI-DEC-115`, `RWI-DEC-140`, `RWI-DEC-150` (`G-09`); `VAL-KEP-23a`–`c`; Kamus data 11.7; State matrix 5.3; API-contract 7.3 |
| **Contract Version** | `0.5.0` API 7.3 (`CaseManagementEvaluationResponse`, `CreateCaseManagementEvaluationRequest`, `UpdateCaseManagementEvaluationRequest`, `ResolvedInstrumentResponse`) |
| **Dependency** | `FE-RWI-081` ✅ (Workspace V2 Navigation), `BE-RWI-113` ✅ (Case Management Evaluation Backend Service & Migration K3), `BE-RWI-107` ✅ (Clinical Instrument Versioning) |
| **Klasifikasi** | `HIGH` — Instrumen kerja Manajer Pelayanan Pasien (MPP), penegakan batas wewenang klinis-administratif berbasis peran dan penempatan unit kerja rawat inap (`VAL-KEP-23a`), siklus hidup dokumen tunggal per episode (`AC-3`), penanganan galat transparan (`AC-4`), serta peniadaan addendum sebelum jenis dokumen 14 resmi tersedia (`INT-KEP-12`, `AC-5`) |
| **Task Mode** | `CROSS-REPO` sempit — Kode implementasi di `QuilvianSystemFrontendDev`; laporan tracked dan pembaruan roadmap di `NewQuilvianSystemBackend` |
| **Tanggal** | 18 September 2026 |
| **Status** | ✅ **SELESAI.** Seluruh 5 Acceptance Criteria (AC-1 s.d. AC-5) terbukti penuh. Pengujian unit otomatis lulus 6 dari 6 test (6/6 passing). Seluruh suite keperawatan lulus 100% (97/97 passing). ESLint 0 error 0 warning. Next.js build berhasil. |

---

## 1. Masalah yang Diselesaikan & Rasional Bisnis

### 1.1 Masalah Sebelumnya
1. **Pencampuran Dokumen Medis dan Manajemen Pelayanan (*Document Domain Mixing*):**
   Pada sistem sebelumnya (Quilvian V1), telaah kasus oleh Manajer Pelayanan Pasien (MPP) sering tercampur atau disisipkan secara tidak terstruktur di dalam pengkajian keperawatan umum atau catatan CPPT bebas. Akibatnya, rekomendasi pembiayaan, advokasi asuransi, dan hambatan sosial keluarga pasien sulit ditelusuri kembali oleh tim manajemen rumah sakit.
2. **Ketiadaan Pembatasan Wewenang Penulisan Berbasis Unit (`FR-KEP-054`):**
   Pada antarmuka lama, tidak ada filter yang membedakan apakah pengguna yang sedang membuka layar adalah MPP yang bertugas di ruang rawat tersebut atau staf lain. Sering kali perawat ruangan atau dokter secara tidak sengaja mengubah telaah MPP, atau sebaliknya MPP dari bangsal lain mengedit data pasien yang bukan menjadi tanggung jawab unitnya.
3. **Duplikasi Dokumen Evaluasi Per Episode:**
   Sering terjadi pembuatan dokumen evaluasi baru setiap kali pergantian hari atau pergantian shift, padahal Evaluasi Awal MPP menurut standar akreditasi rumah sakit (KARS dan JCI) adalah **satu dokumen berkelanjutan hidup per episode rawat inap** yang dipelihara sejak awal masuk hingga kepulangan pasien.
4. **Kegagalan Menambah Addendum yang Membingungkan Pengguna (`INT-KEP-12`):**
   Jika antarmuka menampilkan tombol *"Tambah Addendum"* pada dokumen yang telah selesai, namun mesin rekam medis backend menolak dengan kode `501 Not Implemented` (karena jenis dokumen `14` masih menunggu regulasi approval), pengguna merasa sistem mengalami kerusakan (*bug*). Sesuai prinsip *usability*, kontrol addendum wajib disembunyikan sampai fiturnya resmi didukung penuh.

### 1.2 Solusi yang Dihadirkan
Melalui task `FE-RWI-085`:
1. **Layar Mandiri Evaluasi Awal MPP (`FE-KEP-11` / `InitialEvaluationSection`):**
   - Menghadirkan konsol kerja terpadu untuk MPP pada sub-tab *Evaluasi Awal* di bawah kelompok menu *Pengkajian Pasien*.
   - Menyajikan metadata nomor dokumen unik bernomor seri resmi (`#MPP-YYYYMMDD-XXXX`), status dokumen (*Konsep*, *Selesai*, atau *Dibatalkan*), nama MPP penulis, dan waktu klinis.
2. **Penggambaran 8 Bagian Checklist Terstruktur Berversi (`FR-KEP-053`, PRD Bagian 33):**
   - Mengambil definisi checklist dari backend (`resolveCaseManagementChecklist`) yang terdiri dari 8 seksi standar:
     1. `MPP_SCREENING` — Identifikasi / Skrining Pasien
     2. `MPP_PROBLEM` — Identifikasi Masalah Pasien & Keluarga
     3. `MPP_GOAL` — Harapan / Sasaran Asuhan Manajer Pelayanan
     4. `MPP_PLAN` — Perencanaan Pelayanan & Kolaborasi Klinis
     5. `MPP_SUPPORT` — Dukungan Sosial & Sistem Keluarga
     6. `MPP_FINANCIAL` — Aspek Finansial & Jaminan Pembiayaan
     7. `MPP_LEGAL` — Aspek Legal & Etika Pelayanan
     8. `MPP_DISCHARGE` — Perencanaan Pemulangan (*Discharge Planning*)
3. **Penegakan Hak Tulis Eksklusif & Penempatan Unit (`FR-KEP-054`, `VAL-KEP-23a`):**
   - Hanya staf dengan hak akses `CaseManagementEvaluation:Create` atau `:Update` yang ditempatkan di unit pasien yang dapat mengedit dokumen.
   - Perawat pelaksana dan staf non-MPP disajikan tampilan baca-saja (*read-only*).
   - Bila dokumen belum pernah dibuat dan staf non-MPP membuka layar, ditampilkan *Pemberitahuan Khusus Wewenang MPP* tanpa tombol pembuatan konsep.
4. **Satu Dokumen Hidup Per Episode (`AC-3`):**
   - Jika dokumen telah dibuat (berstatus Draft maupun Selesai), antarmuka **tidak menyediakan tombol buat baru**, mencegah duplikasi data rekam medis.
5. **Transparansi Galat Wewenang 403 (`AC-4`):**
   - Penolakan akses dari backend (misalnya: *"Evaluasi Awal hanya ditulis MPP yang ditempatkan di unit pasien ini"* atau *"Konsep ini ditulis MPP lain"*) disajikan transparan melalui banner peringatan keselamatan pasien.
6. **Peniadaan Tombol Addendum Sebelum Didukung (`AC-5`, `INT-KEP-12`):**
   - Dokumen berstatus selesai (*Completed*) dikunci permanen. Jalur tombol addendum sengaja tidak ditampilkan, dilengkapi catatan informatif bahwa koreksi addendum menunggu persetujuan integrasi rekam medis.

---

## 2. Alur Proses Bisnis & Skenario Rumah Sakit

### 2.1 Skenario 1: Skrining Pasien Kompleks oleh MPP Bangsal Melati
* **Konteks:** Tn. Arifin (68 tahun) didiagnosis stroke iskemik dengan hemiparesis dextra dan afasia motorik. Pasien tinggal berdua dengan istrinya yang juga lansia, dan menggunakan jaminan BPJS Kesehatan kelas 3.
* **Alur MPP (Ns. Dewi, S.Kep., Ners — MPP Unit Melati):**
  1. Ns. Dewi membuka ruang kerja keperawatan Tn. Arifin, lalu mengklik sub-tab **Evaluasi Awal**.
  2. Karena belum ada dokumen evaluasi, Dewi melihat nomor konsep otomatis dan 8 seksi checklist terstruktur.
  3. Dewi mengisi hasil skrining pada 8 bagian:
     - *Identifikasi/Skrining:* Pasien risiko tinggi komplikasi dekubitus dan pemulangan terhambat (*delayed discharge*).
     - *Masalah:* Keterbatasan fisik total, pengasuh utama (*caregiver*) tidak memadai di rumah.
     - *Sasaran:* Mencegah dekubitus, melatih keluarga teknik alih baring dan pemberian makan via NGT.
     - *Perencanaan:* Koordinasi dengan fisioterapi, konseling keluarga, dan permohonan rujukan *home care*.
     - *Finansial:* Kepesertaan BPJS aktif, membutuhkan koordinasi perpanjangan surat eligibilitas peserta (SEP).
     - *Discharge Planning:* Membutuhkan kasur dekubitus dan tabung oksigen sewa saat pulang.
  4. Dewi menekan tombol *"Simpan Konsep"*.
  5. Dokumen tersimpan dengan nomor seri resmi `#MPP-20260918-0004` (Status: *Konsep*).

### 2.2 Skenario 2: Akses Baca-Saja oleh Perawat Pelaksana Ruangan (`FR-KEP-054`)
* **Konteks:** Ns. Siti (perawat pelaksana shift sore) bertugas merawat Tn. Arifin dan ingin mengetahui apakah keluarga pasien menyetujui pemakaian kasur dekubitus.
* **Alur Perawat Pelaksana:**
  1. Ns. Siti membuka sub-tab **Evaluasi Awal**.
  2. Antarmuka mendeteksi peran Siti sebagai perawat pelaksana (bukan MPP penanggung jawab).
  3. Layar menampilkan lencana *"Mode Baca-Saja"* di sudut kanan atas dan menonaktifkan seluruh kolom isian teks.
  4. Siti dapat membaca seluruh 8 bagian telaah yang ditulis Ns. Dewi dengan jelas, namun tidak memiliki tombol *"Simpan Konsep"*, *"Selesaikan"*, maupun *"Batalkan"*.
  5. Integritas telaah MPP terlindungi dari suntingan pihak yang tidak berwenang.

### 2.3 Skenario 3: Penolakan 403 Saat Pasien Dipindahkan Antar-Ruangan (`AC-4`)
* **Konteks:** Tn. Arifin mengalami perburukan kondisi dan dipindahkan dari Ruang Melati ke ICU pukul 14:00.
* **Insiden:** Ns. Dewi (MPP Melati) yang masih membuka halaman konsep Tn. Arifin mencoba menekan tombol simpan pada pukul 14:15.
* **Respon Sistem:**
  1. Backend mendeteksi unit aktif pasien kini adalah ICU, sedangkan penempatan Ns. Dewi adalah di Melati.
  2. Backend mengembalikan status `403 Forbidden` dengan pesan: *"Evaluasi Awal hanya ditulis MPP yang ditempatkan di unit pasien ini."*
  3. Antarmuka frontend menangkap galat tersebut dan menampilkan banner peringatan oranye-merah:
     > *"Penolakan Wewenang: Evaluasi Awal hanya ditulis MPP yang ditempatkan di unit pasien ini."*
  4. Data tidak tertimpa secara ilegal dan Ns. Dewi diinstruksikan untuk melakukan serah terima kasus kepada MPP ICU.

### 2.4 Skenario 4: Penyelesaian Dokumen & Peniadaan Tombol Addendum (`AC-5`, `INT-KEP-12`)
* **Konteks:** Ns. Dewi telah melengkapi seluruh 8 bagian dan melakukan telaah akhir.
* **Alur Finalisasi:**
  1. Dewi menekan tombol *"Selesaikan & Kunci Evaluasi"*.
  2. Modal konfirmasi terbuka mengingatkan bahwa dokumen akan dikunci permanen.
  3. Dewi mengonfirmasi penyelesaian. Status berubah menjadi *Selesai* (`Completed`), lencana berubah hijau, dan waktu penyelesaian tercatat resmi.
  4. Seluruh form terkunci permanen.
  5. Sesuai kriteria keselamatan `AC-5`, sistem **tidak menampilkan tombol addendum**. Di bagian bawah disajikan catatan informatif:
     > *"Dokumen Telah Selesai & Terkunci Permanen. Koreksi lewat addendum belum tersedia karena jenis dokumen Evaluasi Awal pada mesin keutuhan rekam medis masih menunggu persetujuan pemilik (INT-KEP-12)."*

---

## 3. Keputusan Penggunaan Base Component & Modul Desain

| Elemen Antarmuka | Keputusan | Komponen / Sumber | Rationale & Bukti |
| :--- | :--- | :--- | :--- |
| **Modal Dialog (Selesai & Batal)** | `REUSE` | `BaseModal` (`@/components/ui/form-pemeriksaan-ui`) | Menggunakan modal baku form pemeriksaan yang memiliki ARIA compliance, backdrop trap, dan transisi halus. Digunakan pada `CompleteEvaluationModal` dan `CancelEvaluationModal`. |
| **Tombol Aksi Utama & Sekunder** | `REUSE` | `BaseButton` (`@/components/features/base-features/base-button`) | Menjamin konsistensi visual variant `primary`, `outline`, `danger`, dan status `disabled` saat submit. |
| **Navigasi Sub-Tab Pengkajian** | `REUSE` | `AssessmentFormNav` (`../assessment/assessment-form-nav`) | Navigasi horizontal 7 sub-tab pengkajian keperawatan dengan penanda aktif pada `initial-eval`. |
| **Layar Terpadu Evaluasi Awal** | `NEW` | `InitialEvaluationSection` | Komponen baru yang mengoordinasikan metadata dokumen, wewenang peran, banner galat 403, 8 seksi checklist, dan proteksi satu dokumen per episode. |
| **Modal Konfirmasi Finalisasi** | `NEW` | `CompleteEvaluationModal` | Dialog peringatan penguncian permanen dokumen sebelum status diubah ke *Completed*. |
| **Modal Pembatalan Konsep** | `NEW` | `CancelEvaluationModal` | Dialog pembatalan konsep dengan penegakan validasi alasan minimal 5 karakter. |
| **Styling & Token Desain** | `EXTENSION` | `nursing-workspace.module.css` | Menambahkan aturan styling untuk `evaluationHeaderCard`, `evalSectionsAccordion`, badge status MPP, dan empty state non-MPP menggunakan CSS variables terstandar. |

---

## 4. Spesifikasi Endpoint API Bergaya Swagger

Seluruh interaksi data Evaluasi Awal dikonsolidasikan melalui service `case-management-evaluation.service.js` sesuai kontrak backend API 7.3 (`BE-RWI-113`):

### `[Tags("Case Management Evaluation")]` — Evaluasi Awal Manajer Pelayanan Pasien
| Method | Path | Deskripsi | Auth | Request Body / Params | Response Body |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/v1/health-services/clinical-management/case-management-evaluations/episodes/{episodeId}` | Memuat dokumen Evaluasi Awal hidup satu episode (data `null` jika belum diisi). | Bearer Token (`CaseManagementEvaluation:Read`) | `episodeId` (Route, GUID) | `ApiResponse<CaseManagementEvaluationResponse?>` (200 OK) |
| `GET` | `/v1/health-services/clinical-management/case-management-evaluations/checklist/resolve` | Mengambil checklist Evaluasi Awal yang berlaku bagi pasien episode (8 bagian PRD 33). | Bearer Token (`CaseManagementEvaluation:Read`) | `episodeId` (Query, GUID) | `ApiResponse<ResolvedInstrumentResponse>` (200 OK) |
| `GET` | `/v1/health-services/clinical-management/case-management-evaluations/{id}` | Mengambil detail dokumen Evaluasi Awal beserta jawaban checklist dan riwayat versinya. | Bearer Token (`CaseManagementEvaluation:Read`) | `id` (Route, GUID) | `ApiResponse<CaseManagementEvaluationResponse>` (200 OK) |
| `POST` | `/v1/health-services/clinical-management/case-management-evaluations` | Membuat konsep Evaluasi Awal baru oleh MPP (403 jika bukan MPP di unit pasien, 409 jika sudah ada). | Bearer Token (`CaseManagementEvaluation:Create`) | `CreateCaseManagementEvaluationRequest` | `ApiResponse<CaseManagementEvaluationResponse>` (201 Created) |
| `PUT` | `/v1/health-services/clinical-management/case-management-evaluations/{id}` | Menyimpan ulang konsep Evaluasi Awal aktif (wajib membawa `expectedUpdateDate`). | Bearer Token (`CaseManagementEvaluation:Update`) | `UpdateCaseManagementEvaluationRequest` | `ApiResponse<CaseManagementEvaluationResponse>` (200 OK) |
| `PATCH` | `/v1/health-services/clinical-management/case-management-evaluations/{id}/complete` | Menyelesaikan dan mengunci permanen dokumen Evaluasi Awal (Draft -> Completed). | Bearer Token (`CaseManagementEvaluation:Update`) | - | `ApiResponse<CaseManagementEvaluationResponse>` (200 OK) |
| `PATCH` | `/v1/health-services/clinical-management/case-management-evaluations/{id}/cancel` | Membatalkan konsep Evaluasi Awal (wajib menyertakan alasan minimal 5 karakter). | Bearer Token (`CaseManagementEvaluation:Update`) | `CancelCaseManagementEvaluationRequest` | `ApiResponse<CaseManagementEvaluationResponse>` (200 OK) |
| `POST` | `/v1/health-services/clinical-management/case-management-evaluations/{id}/addendums` | Menambah addendum (menjawab 501 Not Implemented sampai jenis dokumen 14 disahkan). | Bearer Token (`CaseManagementEvaluation:Amend`) | `CaseManagementEvaluationAddendumRequest` | `ApiResponse<object>` (501 Not Implemented) |

---

## 5. Bukti Verifikasi Kriteria Keberhasilan (AC-1 s.d. AC-5)

| Kriteria Keberhasilan (AC) | Status | Bukti Implementasi & Verifikasi Otomatis |
| :--- | :---: | :--- |
| **AC-1:** Delapan bagian checklist tampil dari definisi versi, sama seperti formulir lain (`FR-KEP-053`). | ✅ **LULUS** | Komponen `InitialEvaluationSection` menggambar 8 bagian terstruktur: `MPP_SCREENING`, `MPP_PROBLEM`, `MPP_GOAL`, `MPP_PLAN`, `MPP_SUPPORT`, `MPP_FINANCIAL`, `MPP_LEGAL`, dan `MPP_DISCHARGE` dari definisi instrumen backend `resolveCaseManagementChecklist`. Teruji pada unit test `inpatient-case-management-evaluation.test.mjs` (Test 1). |
| **AC-2:** Hanya pemegang hak MPP yang ditempatkan di unit episode melihat kontrol tulis; perawat lain melihat baca-saja (`FR-KEP-054`). | ✅ **LULUS** | Hook `useCaseManagementEvaluation` memvalidasi izin `CaseManagementEvaluation:Create` / `:Update` dan memeriksa `availableActions` dari server. Bila pengguna non-MPP membuka layar saat dokumen belum dibuat, ditampilkan banner informatif `non-mpp-empty-notice` tanpa tombol buat. Bila dokumen sudah ada, staf non-MPP disajikan dalam mode baca-saja (`eval-readonly-badge`). Teruji pada unit test (Test 2). |
| **AC-3:** Layar menampilkan **satu** Evaluasi Awal per episode; tidak ada tombol "buat baru" bila sudah ada. | ✅ **LULUS** | Service memuat dokumen tunggal per episode via `/episodes/{episodeId}`. Pada layar antarmuka, ketika dokumen sudah ada (berstatus Draft maupun Selesai), tidak ada tombol pembuatan konsep baru, menjamin kepatuhan dokumen tunggal per perawatan. Teruji pada unit test (Test 3). |
| **AC-4:** Penolakan `403` dari server ditampilkan apa adanya. | ✅ **LULUS** | Hook menangkap pesan galat spesifik dari backend (seperti penolakan penempatan unit 403 atau konsep milik MPP lain) dan menyajikannya secara transparan pada antarmuka via banner galat `eval-error-banner`. Teruji pada unit test (Test 4). |
| **AC-5:** Selama jenis dokumen `14` belum tersedia, jalur addendum **tidak ditampilkan** — bukan ditampilkan lalu gagal (`INT-KEP-12`). | ✅ **LULUS** | Antarmuka dokumen selesai (`isCompleted === true`) tidak menampilkan tombol addendum (`btn-add-addendum`), melainkan menyajikan catatan kaki informatif `eval-locked-notice` yang menerangkan pembatasan `INT-KEP-12`. Teruji pada unit test (Test 5). |
| **Safety & Routing:** Integrasi rute sub-tab `initial-eval` pada `NursingWorkspaceSections`. | ✅ **LULUS** | `NursingWorkspaceSections` mengimpor `InitialEvaluationSection` dan merendernya saat `activeSection === "assessment"` dan `activeTab === "initial-eval"`. Teruji pada unit test (Test 6). |

---

## 6. Ringkasan Eksekusi Pengujian Otomatis

```bash
# 1. Eksekusi Unit Test FE-RWI-085:
npx node --test tests/unit/inpatient-case-management-evaluation.test.mjs
✔ FE-RWI-085 AC-1: Delapan bagian checklist tampil dari definisi versi (FR-KEP-053) (3.3ms)
✔ FE-RWI-085 AC-2: Kontrol tulis hanya bagi pemegang hak MPP di unit pasien; staf lain baca-saja (FR-KEP-054) (1.4ms)
✔ FE-RWI-085 AC-3: Tepat satu Evaluasi Awal per episode; tidak ada tombol buat baru bila sudah ada (0.6ms)
✔ FE-RWI-085 AC-4: Penolakan 403 dari server ditampilkan apa adanya (0.6ms)
✔ FE-RWI-085 AC-5: Jalur addendum TIDAK DITAMPILKAN sebelum jenis dokumen 14 tersedia (INT-KEP-12) (0.5ms)
✔ FE-RWI-085 Safety & Routing: Integrasi rute sub-tab initial-eval pada NursingWorkspaceSections (0.5ms)
tests 6 | pass 6 | fail 0 | duration_ms 87ms

# 2. Eksekusi Seluruh Suite Keperawatan Rawat Inap (97 tests):
npx node --test tests/unit/inpatient-nursing-*.test.mjs tests/unit/inpatient-clinical-instrument-renderer.test.mjs tests/unit/inpatient-daily-monitoring.test.mjs tests/unit/inpatient-case-management-evaluation.test.mjs
tests 97 | pass 97 | fail 0 | duration_ms 257ms

# 3. Validasi Kode & Linting:
npx eslint src/lib/services/health-services/clinical-management/case-management-evaluation.service.js \
           src/lib/hooks/health-services/inpatient-management/use-case-management-evaluation.js \
           src/components/view/health-services/inpatient-management/nursing-workspace/sections/initial-evaluation/ \
           src/components/view/health-services/inpatient-management/nursing-workspace/components/nursing-workspace-sections.jsx \
           tests/unit/inpatient-case-management-evaluation.test.mjs
# Exit Code: 0 (0 errors, 0 warnings)
```
