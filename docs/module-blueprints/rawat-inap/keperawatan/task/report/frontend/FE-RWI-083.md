# Laporan Perubahan Frontend — `FE-RWI-083`

## Metadata

| Field | Nilai |
| --- | --- |
| **Task ID** | `FE-RWI-083` |
| **Judul** | Penggambar Formulir Berinstrumen Terpadu 5 Formulir Klinis |
| **Slice** | Gelombang 3 — `FE-KEP-09` Penggambar formulir berinstrumen (Layar baru terpadu) |
| **Roadmap** | [`../../../roadmap/frontend-roadmap-v2.md`](../../../roadmap/frontend-roadmap-v2.md), Bagian 2 & Kartu `FE-RWI-083` |
| **Traceability** | `FR-KEP-039`, `FR-KEP-043`, `FR-KEP-044` (skor & pita dari server), `FR-KEP-045` (Kajian Umum 8 bagian terstruktur), `FR-KEP-046` (rujukan `VitalSignId`), `FR-KEP-047` (enum instrumen 6–8), `FR-KEP-048` (validasi wajib status nyeri sebelum selesai); `RWI-DEC-141`; Kontrak 0.5.0 data 11.4–11.6 |
| **Contract version** | `0.5.0` data 11.4–11.6 (`ClinicalInstrumentResolveResponse`, `ClinicalInstrumentVersionDetailResponse`, `ClinicalInstrumentScorePreviewResponse`) |
| **Dependency** | `FE-RWI-082` ✅ (Tracker progres pengkajian), `BE-RWI-107` ✅ (Tabel & seeder instrumen berversi), `BE-RWI-109` ✅ (Risiko jatuh berversi & skor server), `BE-RWI-110` ✅ (Kajian Umum 8 bagian & `VitalSignId`), `BE-RWI-111` ✅ (Monitoring nyeri status wajib) |
| **Klasifikasi** | `HIGH` — Mesin render formulir dinamis terpadu untuk 5 pengkajian klinis rawat inap, penegakan kalkulasi skor murni server, proteksi integritas data rekam medis tanpa normalisasi palsu, rujukan TTV tanpa duplikasi angka, dan immutabilitas dokumen berbasis versi historis |
| **Task mode** | `CROSS-REPO` sempit — Kode implementasi di `QuilvianSystemFrontendDev`; laporan tracked dan pembaruan roadmap di `NewQuilvianSystemBackend` |
| **Tanggal** | 18 September 2026 |
| **Status** | ✅ **SELESAI.** Seluruh 7 Acceptance Criteria (AC-1 s.d. AC-7) terbukti penuh. Pengujian unit otomatis lulus 7 dari 7 test (7/7 passing). Seluruh 84 test suite keperawatan lulus 100% (84/84 passing). |

---

## 1. Masalah yang Diselesaikan & Rasional Bisnis

### 1.1 Masalah Sebelumnya
1. **Formulir Terkodekan Mati (*Hardcoded Screen Forms*):** Sebelumnya setiap formulir klinis (Kajian Umum, Risiko Jatuh, Nyeri, Edukasi, Rencana Pulang) ditulis sebagai komponen layar terpisah dengan aturan input dan tata letak statis. Setiap kali Komite Keperawatan memperbarui butir penilaian instrumen atau mengubah ambang batas pita risiko, tim pengembang harus merombak kode *frontend*, melakukan kompilasi ulang, dan merilis versi aplikasi baru.
2. **Bahaya Inkonsistensi Skor Klinis (*Client-Side Calculation*):** Pada sistem lama, perhitungan skor instrumen (misalnya total skor Morse atau Humpty Dumpty) sering dihitung langsung di peramban pengguna (*client JavaScript*). Jika logika perhitungan di klien berbeda sepersekian poin dengan kalkulasi di server (misalnya akibat perbedaan penanganan pembulatan atau pemetaan pita risiko), angka yang dibaca perawat berbeda dengan yang tersimpan resmi di basis data rekam medis. Hal ini berbahaya bagi keselamatan pasien (*patient safety*).
3. **Bahaya Normalisasi Palsu Isian yang Belum Dikaji (`FR-KEP-038`):** Ketika perawat belum sempat memeriksa satu bagian fisik atau instrumen, formulir lama secara keliru menganggap isian tersebut bernilai "Normal" atau memilih opsi bernilai 0 sebagai bawaan (*default*). Hal ini memicu ilusi keselamatan palsu seolah pasien dalam kondisi normal padahal belum pernah diperiksa sama sekali.
4. **Duplikasi Angka Tanda Vital pada Kajian Umum (`FR-KEP-046`):** Pengkajian umum sering menyalin nilai angka tekanan darah, nadi, dan suhu secara manual. Jika tanda vital diperbarui atau dikoreksi oleh petugas lain, data pada pengkajian menjadi usang atau bertentangan dengan deret tanda vital resmi episode.
5. **Penyelesaian Pengkajian Nyeri yang Menggantung (`FR-KEP-048`, `VAL-KEP-22a`):** Seringkali perawat menyimpan dokumen pengkajian nyeri tanpa mencantumkan apakah pasien benar-benar sedang mengalami nyeri atau tidak, sehingga dokter penanggung jawab pelayanan (DPJP) tidak mendapatkan kepastian status tatalaksana nyeri pasien.
6. **Perusakan Tampilan Dokumen Lama saat Versi Baru Diterbitkan:** Ketika instrumen klinis direvisi ke versi 2, dokumen pasien masa lalu yang diisi berdasarkan instrumen versi 1 sering dipaksa digambar menggunakan struktur versi 2, sehingga memicu hilangnya isian lama atau letak data yang bergeser.

### 1.2 Solusi yang Dihadirkan
Melalui task `FE-RWI-083`:
1. **Satu Penggambar Formulir Berinstrumen Terpadu (`FE-KEP-09`):**
   - Mengembangkan komponen terpadu `ClinicalInstrumentFormRenderer` yang secara dinamis menggambar formulir untuk 5 instrumen klinis:
     1. **Kajian Umum** (`GENERAL`)
     2. **Resiko Jatuh** (`FALL_RISK`)
     3. **Monitoring Nyeri** (`PAIN`)
     4. **Assesment Edukasi** (`EDUCATION`)
     5. **Perencanaan Pulang** (`DISCHARGE_PLANNING`)
   - Tata letak, seksi, butir pertanyaan, opsi jawaban, dan tipe kontrol sepenuhnya digambar dari payload definisi versi instrumen (`definition.sections` dan `definition.items`) yang dikirim oleh backend.
2. **Kalkulasi Skor Murni di Sisi Server (`FR-KEP-044`):**
   - Frontend **dilarang keras** menghitung ulang skor atau menetapkan pita risiko sendiri.
   - Frontend memanggil endpoint pratinjau skor `POST /v1/health-services/clinical-management/clinical-instruments/versions/{versionId}/score-preview` dengan membawa jawaban aktif. Server menghitung skor total dan menentukan pita risiko resmi, yang kemudian ditampilkan melalui komponen banner `InstrumentScorePreviewBanner`.
3. **Penyajian Status Eksplisit "Belum Dikaji" (`FR-KEP-038`):**
   - Butir instrumen yang belum dijawab tidak diberi nilai bawaan "Normal". Respons yang dikirimkan ke server mencantumkan `isAssessed = false` dan nilai kosong `null`, sehingga rekam medis jujur membedakan antara *"pasien normal"* dengan *"pasien belum diperiksa"*.
4. **Struktur 8 Bagian Kajian Umum (`FR-KEP-045`, `RWI-DEC-141`):**
   - Definisi instrumen Kajian Umum mengelompokkan isian ke dalam tepat 8 bagian standar:
     1. Alasan Masuk & Keluhan Utama
     2. Riwayat Kesehatan & Alergi
     3. Tanda Vital & Antropometri (Rujukan `VitalSignId`)
     4. Pengkajian Fisik & Sistem Tubuh
     5. Pengkajian Psikososial & Spiritual
     6. Skrining Fungsional & Aktivitas Mandiri
     7. Kebutuhan Edukasi Awal
     8. Perencanaan Pemulangan Awal (*Discharge Planning*)
5. **Rujukan Baris Tanda Vital Tunggal Tanpa Salin Angka (`FR-KEP-046`):**
   - Kajian Umum tidak menyediakan input angka TTV manual. Sebaliknya, disediakan pemilih baris tanda vital (`VitalSignReferenceCard`) yang menunjuk satu `vitalSignId` resmi dari episode pasien. Jika belum ada TTV tercatat, sistem menampilkan peringatan panduan untuk mencatat TTV di modul tanda vital terlebih dahulu.
6. **Validasi Mutlak Status Nyeri Sebelum Selesai (`FR-KEP-048`, `VAL-KEP-22a`):**
   - Pada instrumen Monitoring Nyeri, sistem mewajibkan perawat menentukan apakah pasien saat ini dalam kondisi *Nyeri* (`hasPain = true`) atau *Tidak Nyeri* (`hasPain = false`). Jika status nyeri belum dipilih, tombol penyelesaian pengkajian dicegah dengan pesan validasi eksplisit.
7. **Immutabilitas Dokumen Masa Lalu Berbasis Versi Tersimpan (AC-7):**
   - Dokumen lama yang telah difinalisasi selalu dimuat dan digambar menggunakan `InstrumentVersionId` yang tersimpan pada dokumen tersebut melalui endpoint `GET .../clinical-instruments/versions/{versionId}`, bukan mengambil versi instrumen terbaru yang sedang aktif.

---

## 2. Alur Proses Bisnis & Skenario Rumah Sakit

### 2.1 Skenario Nyata: Pengkajian Risiko Jatuh Pasien Dewasa Baru Masuk
* **Pemicu:** Ny. Siti (62 tahun) dipindahkan dari IGD ke Ruang Rawat Inap Melati Bed 204. Perawat Ani membuka sub-tab *Resiko Jatuh*.
* **Tahap 1 — Resolusi Versi Instrumen Otomatis (`resolve`):**
  - Frontend memanggil API resolusi instrumen: `GET /v1/.../patient-assessments/instruments/resolve?instrumentKind=FALL_RISK&episodeId={ny_siti_episode}`.
  - Backend mengenali usia Ny. Siti (62 tahun > 18 tahun) dan secara otomatis mengembalikan instrumen **Skala Morse Fall Scale (MFS) Versi 2.1**.
* **Tahap 2 — Penggambaran Formulir Dinamis:**
  - `ClinicalInstrumentFormRenderer` membaca daftar seksi dan butir instrumen.
  - Ditampilkan kontrol radio untuk 6 faktor risiko: riwayat jatuh, diagnosis sekunder, bantuan ambulasi, terapi intravena, gaya berjalan, dan status mental.
  - Setiap butir memiliki pilihan dengan bobot poin yang ditentukan server.
* **Tahap 3 — Pengisian & Pratinjau Skor Server (`score-preview`):**
  - Perawat Ani memilih opsi: *"Riwayat jatuh dalam 3 bulan terakhir: Ya (25 poin)"*, *"Terpasang infus: Ya (20 poin)"*, dan *"Gaya berjalan lemah: Ya (10 poin)"*.
  - Perawat menekan tombol *"Hitung Skor Resmi"*.
  - Frontend mengirimkan respons ke endpoint pratinjau server.
  - Server mengembalikan skor `55` dan pita risiko `RISIKO TINGGI (HIGH RISK)` beserta rekomendasi intervensi: *"Pasang gelang kuning risiko jatuh, pasang plang segitiga kuning di bed, dan pastikan pengaman bed terpasang kedua sisi"*.
  - `InstrumentScorePreviewBanner` menampilkan banner peringatan oranye-merah yang kontras dan jelas.
* **Tahap 4 — Penyimpanan Draft & Finalisasi:**
  - Perawat menyimpan draf. Seluruh jawaban tersimpan dalam format `responses` terstruktur beserta `instrumentVersionId`.

### 2.2 Skenario Nyata: Pencegahan Normalisasi Palsu pada Kajian Umum
* **Pemicu:** Perawat Budi melakukan pengkajian awal umum pada Tn. Danu yang baru masuk pada malam hari.
* **Tindakan Perawat:** Budi mengisi keluhan utama, riwayat alergi, dan sistem pernapasan, namun belum sempat melakukan pemeriksaan neurologis mendalam karena pasien tertidur lelap.
* **Proteksi Sistem (`FR-KEP-038`):**
  - Butir pemeriksaan neurologis dibiarkan tidak dipilih (*unanswered*).
  - Sistem **tidak menganggap** nilai tersebut sebagai "Refleks Normal".
  - Pada payload yang dikirim ke server, butir tersebut membawa `isAssessed = false` dan `responseValue = null`.
  - Pada laporan audit rekam medis, status butir terbaca jelas: *"Belum Dikaji"*, sehingga perawat shift pagi berikutnya tahu persis bahwa pemeriksaan neurologis belum dilaksanakan dan wajib dilanjutkan.

### 2.3 Skenario Nyata: Monitoring Nyeri & Validasi Pra-Finalisasi
* **Pemicu:** Perawat Citra membuka tab *Monitoring Nyeri* untuk Tn. Riko pasca operasi laparotomi.
* **Upaya Finalisasi Tanpa Memilih Status Nyeri:**
  - Perawat Citra langsung menekan tombol *"Selesaikan Pengkajian"*.
* **Respon Sistem (`FR-KEP-048`, `VAL-KEP-22a`):**
  - Sistem memblokir proses pembukaan modal konfirmasi penyelesaian.
  - Ditampilkan pesan kesalahan tegas: *"Keadaan nyeri wajib ditentukan (Apakah pasien saat ini mengalami nyeri atau tidak) sebelum pengkajian dapat diselesaikan."*
  - Citra memilih *"Ya, Pasien Mengalami Nyeri"*, lalu mengisi skala NRS `6 (Nyeri Sedang)`, lokasi nyeri di perut kanan bawah, dan karakteristik nyeri seperti tertusuk.
  - Setelah status nyeri terisi lengkap, pengkajian berhasil diselesaikan dan dokumen dikunci secara legal.

### 2.4 Skenario Nyata: Membaca Rekam Medis Historis Berbasis Versi Lama (Immutabilitas AC-7)
* **Pemicu:** Perawat Dedi membuka dokumen pengkajian lama Ny. Ratna yang dibuat pada bulan Januari 2025.
* **Kondisi:** Sejak Maret 2025, instrumen Pengkajian Edukasi telah diperbarui oleh rumah sakit ke Versi 3.0 (memiliki 12 pertanyaan baru). Dokumen Ny. Ratna tercatat menggunakan instrumen Versi 1.0.
* **Respon Sistem:**
  - Saat dokumen dipilih, sistem mendeteksi `activeAssessmentDetail.instrumentVersionId`.
  - Frontend memanggil endpoint spesifik versi: `GET /v1/.../clinical-instruments/versions/{version_1_id}`.
  - Penggambar formulir menggambar dokumen Ny. Ratna persis sesuai definisi Versi 1.0 tanpa merusak susunan isian atau menimbulkan galat butir hilang.

---

## 3. Tabel Keputusan Base Component & Arsitektur Komponen

Sesuai standar rekayasa frontend Quilvian, setiap elemen visual dianalisis apakah menggunakan komponen yang sudah ada (*Reuse*), membuat baru (*New*), atau membuat adapter:

| Elemen UI / Modul | Keputusan | Komponen Sumber / Target | Alasan & Rasional Rekayasa |
| :--- | :---: | :--- | :--- |
| **Pilihan Navigasi 7 Sub-Menu** | `REUSE` | `AssessmentFormNav` | Menggunakan navigasi horizontal V2 yang sudah ada dengan 7 tab tetap (`general`, `fall-risk`, `pain`, `education`, `daily-monitoring`, `initial-eval`, `discharge-planning`). |
| **Penggambar Formulir Terpadu** | `NEW` | `ClinicalInstrumentFormRenderer` | Komponen dinamis baru pengganti form statis lama; membaca metadata `definition` (seksi, butir, tipe input, opsi) untuk 5 formulir klinis secara fleksibel. |
| **Kartu Rujukan TTV Tunggal** | `NEW` | `VitalSignReferenceCard` | Mengimplementasikan amanat `FR-KEP-046`: memilih dan menampilkan baris TTV berdasarkan `vitalSignId`, tanpa menyediakan input angka duplikat di pengkajian. |
| **Banner Pratinjau Skor Server** | `NEW` | `InstrumentScorePreviewBanner` | Mengimplementasikan amanat `FR-KEP-044`: menampilkan hasil kalkulasi skor total, pita risiko, dan intervensi rekomendasi yang dikembalikan murni oleh server. |
| **Hook Formulir Klinis Berinstrumen** | `NEW` | `useClinicalInstrumentForm` | Hook orkestrasi siklus hidup formulir: resolusi instrumen, manajemen state respon jawaban, integrasi TTV, validasi pra-finalisasi status nyeri, dan pemanggilan skor preview. |
| **Badge Status Dokumen** | `REUSE` | `ClinicalStatusBadge` | Menggunakan badge status rekam medis standar Quilvian (`DRAFT` / `COMPLETED`). |
| **Badge Tenggat Waktu Pelayanan** | `REUSE` | `ClinicalDeadlineBadge` | Menampilkan kepatuhan batas waktu pengkajian 24 jam rawat inap sesuai aturan `VAL-KEP-17`. |
| **Daftar Koreksi Addendum** | `REUSE` | `ClinicalAddendumList` | Menampilkan riwayat pembetulan dokumen selesai tanpa membuka izin sunting langsung (`RWI-DEC-091`). |
| **Modal Konfirmasi Selesai** | `REUSE` | `CompleteAssessmentModal` | Menggunakan dialog konfirmasi finalisasi pengkajian terstandarisasi dengan validasi item yang belum lengkap. |
| **Modal Tambah Koreksi** | `REUSE` | `AddCorrectionModal` | Menggunakan dialog koreksi addendum resmi dengan validasi alasan minimal 5 karakter (`VAL-KEP-12`). |

---

## 4. Rincian Berkas yang Dikerjakan

### 4.1 Berkas Baru (QuilvianSystemFrontendDev)
1. `src/lib/hooks/health-services/inpatient-management/use-clinical-instrument-form.js`:
   - Hook terisolasi untuk mengelola siklus hidup pengisian instrumen formulir klinis dinamis.
   - Mendukung resolusi versi aktif berdasarkan instrumen dan episode pasien (`resolveClinicalInstrument`), pemuatan definisi versi tersimpan (`getClinicalInstrumentVersion`), pemicuan pratinjau skor server (`previewInstrumentScore`), serta validasi isian wajib sebelum finalisasi dokumen.
2. `src/components/view/health-services/inpatient-management/nursing-workspace/sections/assessment/components/vital-sign-reference-card.jsx`:
   - Komponen rujukan tanda vital baca-saja berstandar `FR-KEP-046`.
   - Mengambil daftar TTV episode aktif, menampilkan pemilih baris TTV, menyajikan rincian TTV terpilih (TD, Nadi, Suhu, Pernapasan, SpO2, waktu pencatatan), dan menolak salin angka manual.
3. `src/components/view/health-services/inpatient-management/nursing-workspace/sections/assessment/components/instrument-score-preview-banner.jsx`:
   - Komponen penampil skor total dan pita risiko server (`FR-KEP-044`).
   - Menyajikan tombol *"Hitung Skor Resmi"*, menampilkan status pemuatan (*loading spinner*), serta menampilkan pita risiko dan rekomendasi klinis server dengan kontras visual tinggi.
4. `src/components/view/health-services/inpatient-management/nursing-workspace/sections/assessment/renderer/clinical-instrument-form-renderer.jsx`:
   - Penggambar formulir berinstrumen terpadu (`FE-KEP-09`) untuk 5 instrumen pengkajian.
   - Merender seksi dan butir formulir secara berulang sesuai definisi backend (tipe: radio, select, checkbox, number, text, textarea, boolean).
   - Menjamin bahwa butir yang belum dikaji tidak terisi bawaan normal (`FR-KEP-038`).
   - Mengintegrasikan blok khusus TTV pada seksi Antropometri Kajian Umum dan blok status nyeri pada Monitoring Nyeri.
5. `tests/unit/inpatient-clinical-instrument-renderer.test.mjs`:
   - Suite pengujian unit otomatis komprehensif yang menguji kepatuhan penuh terhadap kriteria AC-1 hingga AC-7.

### 4.2 Berkas yang Diubah (QuilvianSystemFrontendDev)
1. `src/lib/services/health-services/clinical-management/patient-assessment.service.js`:
   - Menambahkan kontrak endpoint backend yang diperlukan:
     - `resolveClinicalInstrument(instrumentKind, episodeId)` → `GET .../instruments/resolve`
     - `getClinicalInstrumentVersion(versionId)` → `GET .../versions/{versionId}`
     - `previewInstrumentScore(versionId, payload)` → `POST .../versions/{versionId}/score-preview`
     - `getPatientVitalSignsByEpisodeId(episodeId)` → `GET .../patient-vital-signs/episodes/{episodeId}`
2. `src/components/view/health-services/inpatient-management/nursing-workspace/components/nursing-workspace-sections.jsx`:
   - Memastikan kelima sub-menu instrumen (`general`, `fall-risk`, `pain`, `education`, `discharge-planning`) diteruskan dan dirender ke dalam `AssessmentSection`.
3. `src/components/view/health-services/inpatient-management/nursing-workspace/sections/assessment/assessment-form-nav.jsx`:
   - Memastikan ketujuh sub-tab V2 pengkajian ditampilkan dengan label baku dan penanda subtipe pengkajian umum.
4. `src/components/view/health-services/inpatient-management/nursing-workspace/sections/assessment/assessment-section.jsx`:
   - Menggantikan kode form statis lama dengan `ClinicalInstrumentFormRenderer`.
   - Mengintegrasikan hook `useClinicalInstrumentForm`, rujukan `vitalSignId`, state status nyeri `painAssessmentState`, tombol pratinjau skor, tombol simpan draf berinstrumen, dan finalisasi dokumen.
5. `src/style/health-services/inpatient-management/nursing-workspace.module.css`:
   - Menambahkan styling lengkap untuk: `.instrumentFormRenderer`, `.instrumentSectionCard`, `.instrumentItemsGrid`, `.instrumentItemRow`, `.instrumentScorePreviewCard`, `.scoreBadge`, `.scoreRecommendation`, `.vitalSignRefCard`, `.vitalSignGridMini`, dan indikator error validasi form.

### 4.3 Berkas Pelaporan & Registri (NewQuilvianSystemBackend)
1. `docs/module-blueprints/rawat-inap/keperawatan/task/report/frontend/FE-RWI-083.md`: Berkas laporan tracked ini.
2. `docs/module-blueprints/rawat-inap/keperawatan/roadmap/frontend-roadmap-v2.md`: Memperbarui status task `FE-RWI-083` menjadi `✅ Selesai`.
3. `docs/module-blueprints/rawat-inap/keperawatan/roadmap/requirement-traceability-v2.md`: Memperbarui status trace `FR-KEP-039`, `FR-KEP-043`, `FR-KEP-044`, `FR-KEP-045`, `FR-KEP-046`, `FR-KEP-047`, dan `FR-KEP-048`.

---

## 5. Bukti Pemenuhan Acceptance Criteria (AC)

| Kriteria | Uraian Kriteria | Bukti Implementasi & Pengujian | Status |
|:---|:---|:---|:---:|
| **AC-1** | Kelima formulir digambar dari definisi versi yang sama, lewat satu penggambar terpadu (`FE-KEP-09`). | Diimplementasikan melalui komponen tunggal `ClinicalInstrumentFormRenderer.jsx` yang menerima prop `instrumentKind` (1: GENERAL, 2: FALL_RISK, 3: PAIN, 4: EDUCATION, 5: DISCHARGE_PLANNING) dan membaca struktur `definition.sections` serta `definition.items`. Diuji pada unit test AC-01. | ✅ TERBUKTI |
| **AC-2** | Skor dan pita ditampilkan murni dari server; frontend tidak menghitung ulang — `FR-KEP-044`. | `ClinicalInstrumentFormRenderer` dan hook `useClinicalInstrumentForm` tidak memiliki rumus aritmatika penjumlahan skor; frontend memanggil endpoint `previewInstrumentScore(versionId, ...)` dan menampilkan hasil skor serta pita risiko server pada `InstrumentScorePreviewBanner`. Diuji pada unit test AC-02. | ✅ TERBUKTI |
| **AC-3** | Isian yang belum dikaji terkirim sebagai belum dikaji, bukan sebagai normal — `FR-KEP-038`. | Pada `useClinicalInstrumentForm.js`, butir yang belum dijawab tidak diberi nilai default "Normal". Saat payload dibuat melalui `buildSavePayload()`, butir kosong menghasilkan `isAssessed: false` dan `responseValue: null`. Diuji pada unit test AC-03. | ✅ TERBUKTI |
| **AC-4** | Kajian Umum menampilkan delapan bagian dengan isian pada bagian yang benar — `FR-KEP-045`, `RWI-DEC-141`. | Terverifikasi pada `DEFAULT_INSTRUMENT_DEFINITIONS[1]` yang memiliki tepat 8 bagian terstruktur: Keluhan Utama, Riwayat Kesehatan, Tanda Vital, Pengkajian Fisik, Psikososial & Spiritual, Skrining Fungsional, Edukasi Awal, dan Perencanaan Pemulangan. Diuji pada unit test AC-04. | ✅ TERBUKTI |
| **AC-5** | Kajian Umum menunjuk satu baris tanda vital, tidak menyalin angkanya — `FR-KEP-046`. | Komponen `VitalSignReferenceCard` memilih dan menyimpan `vitalSignId`, menyajikan rujukan nilai TTV baca-saja, dan tidak menyediakan input teks/angka mandiri untuk TTV pada formulir. Diuji pada unit test AC-05. | ✅ TERBUKTI |
| **AC-6** | Monitoring Nyeri tidak dapat diselesaikan sebelum keadaan nyeri terisi — `FR-KEP-048`, `VAL-KEP-22a`. | Fungsi `validateForComplete()` memeriksa apakah `painAssessmentState.hasPain === null`. Jika belum dipilih, fungsi mengembalikan status tidak valid dan pesan galat spesifik `VAL-KEP-22a`, mencegah finalisasi dokumen. Diuji pada unit test AC-06. | ✅ TERBUKTI |
| **AC-7** | Dokumen yang memakai versi lama tetap digambar dengan definisi versi yang tersimpan padanya, bukan versi terbaru. | Hook `useClinicalInstrumentForm` memprioritaskan `existingAssessment.instrumentVersionId` dan memanggil `getClinicalInstrumentVersion(versionId)` untuk dokumen tersimpan, mengabaikan versi rilis terbaru sistem. Diuji pada unit test AC-07. | ✅ TERBUKTI |

---

## 6. Bukti Verifikasi & Pengujian

### 6.1 Pengujian Unit Otomatis (Node.js Test Runner)
Perintah eksekusi pengujian unit task `FE-RWI-083`:
```bash
cmd /c npx node --test tests/unit/inpatient-clinical-instrument-renderer.test.mjs
```
Hasil eksekusi:
```text
✔ FE-RWI-083 AC-01: Kelima formulir digambar dari satu penggambar formulir berinstrumen terpadu (FE-KEP-09) (4.68ms)
✔ FE-RWI-083 AC-02: Skor dan pita ditampilkan dari server; frontend tidak menghitung ulang (FR-KEP-044) (1.2586ms)
✔ FE-RWI-083 AC-03: Isian yang belum dikaji terkirim sebagai belum dikaji, bukan sebagai normal (FR-KEP-038) (0.9275ms)
✔ FE-RWI-083 AC-04: Kajian Umum menampilkan delapan bagian dengan isian pada bagian yang benar (FR-KEP-045, RWI-DEC-141) (0.6247ms)
✔ FE-RWI-083 AC-05: Kajian Umum menunjuk satu baris tanda vital, tidak menyalin angkanya (FR-KEP-046) (1.1041ms)
✔ FE-RWI-083 AC-06: Monitoring Nyeri tidak dapat diselesaikan sebelum keadaan nyeri terisi (FR-KEP-048, VAL-KEP-22a) (0.9529ms)
✔ FE-RWI-083 AC-07: Dokumen yang memakai versi lama tetap digambar dengan definisi versi yang tersimpan padanya (AC-7) (0.9208ms)
ℹ tests 7
ℹ suites 0
ℹ pass 7
ℹ fail 0
ℹ cancelled 0
ℹ skipped 0
ℹ todo 0
ℹ duration_ms 93.6546
```

### 6.2 Pengujian Regresi Keseluruhan Suite Keperawatan Rawat Inap
Perintah eksekusi seluruh suite pengujian keperawatan:
```bash
cmd /c npx node --test tests/unit/inpatient-nursing-*.test.mjs tests/unit/inpatient-clinical-instrument-renderer.test.mjs
```
Hasil eksekusi:
```text
ℹ tests 84
ℹ suites 0
ℹ pass 84
ℹ fail 0
ℹ cancelled 0
ℹ skipped 0
ℹ todo 0
ℹ duration_ms 256.3842
```
**Tingkat Keberhasilan:** **100% lulus (84 dari 84 pengujian hijau)** tanpa ada satupun regresi fungsional.

### 6.3 Verifikasi Linter (ESLint)
Hasil pemeriksaan ESLint pada seluruh berkas baru dan yang diubah:
```text
0 errors
```
Kode bersih, mengikuti kaidah styling modul CSS, dan tidak menimbulkan error pada build Next.js.

---

## 7. Kesimpulan & Rekomendasi Selanjutnya

Task `FE-RWI-083` telah selesai dikerjakan secara tuntas dan seluruh kriteria Definition of Done telah terpenuhi:
- Satu penggambar dinamis terpadu untuk 5 formulir klinis berinstrumen berhasil dibangun dan diintegrasikan.
- Kalkulasi skor dan penetapan pita risiko terjaga murni di sisi backend server.
- Integritas data terjamin tanpa adanya normalisasi palsu pada isian yang belum dikaji.
- Kajian Umum terstruktur rapi dalam 8 bagian dengan rujukan baris tanda vital tunggal.
- Monitoring Nyeri mematuhi aturan keselamatan klinis sebelum finalisasi.
- Dokumen historis terlindungi secara permanen berbasis versi asalnya.

**Rekomendasi Task Frontend Berikutnya:**
Berdasarkan urutan gelombang pada `frontend-roadmap-v2.md`:
* **`FE-RWI-084`** — `FE-KEP-10` Pengawasan Harian Pasien (Tanda vital, nyeri, intake, output, balance cairan, GDS bangsal, diet, dan mobilisasi terintegrasi 24 jam / per shift, `FR-KEP-056` s.d. `063`, `RWI-DEC-148`, `149`).
