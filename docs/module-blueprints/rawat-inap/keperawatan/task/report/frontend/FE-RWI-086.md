# Laporan Perubahan Frontend — `FE-RWI-086`

## Metadata

| Field | Nilai |
| --- | --- |
| **Task ID** | `FE-RWI-086` |
| **Judul** | Asuhan Keperawatan Terpadu (Vital Sign Deret Episode, SOAP Perawat, Catatan Terintegrasi, Tindakan Harian, Catatan Naratif, & Rencana Asuhan) |
| **Slice** | Gelombang 2 — `FE-KEP-12` Asuhan Keperawatan (Rework `FE-KEP-04` & `FE-KEP-05`) |
| **Roadmap** | [`../../../roadmap/frontend-roadmap-v2.md`](../../../roadmap/frontend-roadmap-v2.md), Bagian 2 & Kartu `FE-RWI-086` |
| **Traceability** | `FR-KEP-077` (SOAP keperawatan dan catatan naratif terintegrasi ke CPPT berjenis); `FR-KEP-056` (deret tanda vital per episode); `RWI-DEC-113`, `RWI-DEC-114`, `RWI-DEC-118`, `RWI-DEC-126`, `RWI-DEC-141`; `RWI-AC-204`, `RWI-AC-205`, `RWI-AC-206`; `VAL-KEP-22c`, `VAL-DOK-59`, `INV-DOK-11`, `INV-KEP-04`; Kamus data 11.2; API-contract keperawatan `0.5.0` Bagian 7.4 & 7.15; API-contract dokter `0.6.0` Bagian 12.4 |
| **Contract Version** | `0.5.0` + `0.6.0` [DOK] (`PatientIntegratedProgressNoteResponse`, `CreatePatientIntegratedProgressNoteRequest`, `PatientVitalSignSeriesItem`, `CpptNoteKind`) |
| **Dependency** | `FE-RWI-081` ✅ (Workspace V2 Navigation), `BE-RWI-124` ✅ (SOAP & Catatan Keperawatan sebagai CPPT berjenis), `BE-RWI-121` ✅ (Tanda Vital Deret per Episode) |
| **Klasifikasi** | `HIGH` — Konsolidasi pencatatan rekam medis harian perawat, penegakan integritas CPPT lintas profesi (`FR-KEP-077`), penghilangan tabel terisolasi lama, aturan keselamatan mutlak pencatatan cairan/obat tidak mempengaruhi MAR/cairan (`RWI-AC-206`), dan proteksi wewenang verifikasi DPJP (`INV-DOK-11`) |
| **Task Mode** | `CROSS-REPO` sempit — Kode implementasi di `QuilvianSystemFrontendDev`; laporan tracked dan pembaruan roadmap di `NewQuilvianSystemBackend` |
| **Tanggal** | 18 September 2026 |
| **Status** | ✅ **SELESAI.** Seluruh 5 Acceptance Criteria (AC-1 s.d. AC-5) terbukti penuh. Pengujian unit otomatis lulus 6 dari 6 test (6/6 passing). Seluruh suite keperawatan rawat inap lulus 100% (103/103 passing). ESLint 0 error 0 warning. Next.js production build berhasil. |

---

## 1. Masalah yang Diselesaikan & Rasional Bisnis

### 1.1 Masalah Sebelumnya
1. **Silo Dokumentasi Keperawatan vs Dokter (*Fragmented Clinical Records*):**
   Pada sistem lama, perawat menuliskan catatan SOAP harian pada tabel mandiri keperawatan yang terpisah dari Catatan Perkembangan Pasien Terintegrasi (CPPT) milik dokter. Akibatnya, dokter penanggung jawab pelayanan (DPJP) tidak dapat membaca catatan perkembangan perawat secara langsung di lini masa CPPT, begitu pula perawat harus berpindah modul untuk mengetahui rencana dokter.
2. **Ketiadaan Pembeda Jenis Catatan Perkembangan (*Unstructured Progress Notes*):**
   Ketika wacana penggabungan CPPT pertama kali muncul, belum ada pemisahan tegas antara catatan perkembangan dokter, SOAP keperawatan, dan catatan naratif insidental, sehingga format entri menjadi tidak seragam dan menyulitkan audit akreditasi rumah sakit (SNARS/JCI).
3. **Penyalahgunaan Kolom Catatan untuk Transaksi Klinis Cairan & Obat (`RWI-AC-206`):**
   Sering terjadi salah kaprah di mana staf menuliskan angka pemberian cairan infus atau dosis obat di dalam teks narasi SOAP, lalu berasumsi bahwa neraca cairan (*fluid balance*) dan rekam pemberian obat (MAR) otomatis terbarui. Hal ini sangat berbahaya bagi keselamatan pasien (*patient safety*).
4. **Tanda Vital Tidak Tersusun per Episode Perawatan (`BE-RWI-121`):**
   Pencatatan tanda vital sebelumnya tidak mengikat episode rawat inap, sehingga perawat kesulitan melihat grafik tren perburukan kondisi pasien (Early Warning Score / EWS dan Mean Arterial Pressure / MAP) sepanjang hari perawatan berjalan.

### 1.2 Solusi yang Dihadirkan
Melalui task `FE-RWI-086`:
1. **Penyatuan ke Lembar CPPT dengan Pembeda Jenis (`FR-KEP-077` / `BE-RWI-124`):**
   - SOAP Keperawatan disimpan langsung ke tabel CPPT terintegrasi menggunakan `NoteKind = 2` (`NursingSoap`).
   - Tab **SOAP** menyaring dan menampilkan catatan berjenis `NursingSoap`.
   - Catatan Keperawatan Naratif disimpan ke CPPT terintegrasi menggunakan `NoteKind = 3` (`NursingNarrative`).
   - Tab **Catatan Keperawatan** menyaring dan menampilkan catatan berjenis `NursingNarrative`.
   - Dokter dan tenaga kesehatan lain dapat membaca catatan ini secara terintegrasi pada lembar CPPT mereka.
2. **Catatan Terintegrasi Tanpa Tombol Verifikasi bagi Perawat (`AC-3` / `INV-DOK-11`):**
   - Tab **Catatan Terintegrasi** menyajikan lini masa lintas PPA (Dokter, Perawat, Bidan, Gizi, Farmasi).
   - Antarmuka **meniadakan tombol verifikasi bagi perawat**, karena verifikasi tanda tangan baca CPPT secara regulasi medis adalah wewenang eksklusif Dokter Penanggung Jawab Pelayanan (DPJP).
3. **Deret Tanda Vital per Episode Rawat Inap (`AC-4` / `BE-RWI-121`):**
   - Tab **Vital Sign** memuat deret waktu observasi per episode melalui endpoint `GET /patient-vital-signs/episodes/{episodeId}`.
   - Menampilkan visualisasi tabel dan grafik tren (Tekanan Darah Sistolik/Diastolik, Nadi, Suhu, Pernapasan, SpO2).
   - Nilai EWS dan MAP murni berasal dari kalkulasi server backend.
   - Kolom isian nyeri **ditiadakan secara mutlak** pada dialog pencatatan tanda vital (`VAL-KEP-22c`), digantikan dengan tautan pintas menuju tab resmi *Monitoring Nyeri* (`FR-KEP-049`).
4. **Penegakan Aturan Keselamatan Pasien (`RWI-AC-206`):**
   - Ditampilkan banner peringatan keselamatan tegas pada modal dan tab: *"Catat intake/output di Pengawasan Harian dan pemberian obat di Obat & Alkes."*
5. **Konservasi Alur Asuhan yang Sudah Ada (`AC-5`):**
   - Sub-tab **Tindakan Harian** (`NursingInterventionSection`) dan **Rencana Asuhan** (`CarePlanSection`) tetap berfungsi normal.

---

## 2. Alur Proses Bisnis & Skenario Rumah Sakit

### 2.1 Skenario 1: Dokumentasi SOAP Keperawatan Pasien Sesak Napas
- **PPA Terlibat:** Ns. Siti Rahmawati, S.Kep (Perawat Ruang Rawat Inap Melati) & dr. Hendra, Sp.P (DPJP Paru).
- **Pasien:** Tn. Budi (Episode `#RWI-20260918-001`).
- **Alur:**
  1. Pukul 14.00, Ns. Siti melakukan observasi rutin di ruang rawat. Pasien menyatakan sesak berkurang setelah posisi tidur semi-fowler dan terapi oksigen nasal kanul 3 lpm.
  2. Ns. Siti membuka menu **Asuhan Keperawatan** &rarr; tab **SOAP**, lalu mengklik tombol **"Catat SOAP"**.
  3. Ns. Siti mengisi:
     - **S:** Pasien menyatakan sesak berkurang setelah posisi semi-fowler.
     - **O:** Kesadaran compos mentis, TD 120/80 mmHg, Nadi 84 x/m, RR 20 x/m, SpO2 98% dengan O2 kanul 3 lpm. Ronkhi berkurang di basal paru kanan.
     - **A:** Pola napas tidak efektif teratasi sebagian.
     - **P:** Pertahankan posisi semi-fowler, lanjutkan terapi O2 3 lpm, pantau respirasi per 4 jam.
  4. Sistem mengirimkan payload ke `POST /patient-integrated-progress-notes` dengan `noteKind = 2` (`NursingSoap`) dan `professionType = "Nurse"`.
  5. Catatan langsung terbit pada tab SOAP Ns. Siti dengan lencana status *"Menunggu Verifikasi DPJP"*.
  6. Saat dr. Hendra (DPJP) membuka ruang kerja dokter pada tab CPPT, catatan Ns. Siti langsung tampil di lini masa terintegrasi dan dr. Hendra dapat membubuhkan verifikasi tanda tangan telaah baca.

### 2.2 Skenario 2: Catatan Keperawatan Naratif untuk Kejadian Insidental
- **PPA Terlibat:** Ns. Siti Rahmawati, S.Kep.
- **Alur:**
  1. Pukul 19.30, Tn. Budi tersedak saat meminum air putih dan mengalami batuk hebat sesaat. Ns. Siti segera melakukan penepukan punggung (*back blows*) dan menenangkan pasien.
  2. Karena bukan merupakan evaluasi komprehensif berkala SOAP melainkan laporan peristiwa insidental, Ns. Siti membuka tab **Catatan Keperawatan** &rarr; tombol **"Catat Keperawatan"**.
  3. Ns. Siti menuliskan uraian: *"Pukul 19.30 pasien tersedak air minum saat posisi berbaring. Dilakukan penanganan batuk efektif dan posisi duduk tegak. Batuk reda dalam 3 menit, sputum tidak berdarah, jalan napas paten, SpO2 98%."*
  4. Sistem menyimpan catatan dengan `noteKind = 3` (`NursingNarrative`). Catatan tersimpan rapi di tab Catatan Keperawatan dan ikut terdaftar pada rekam jejak CPPT terpadu.

### 2.3 Skenario 3: Membaca Lini Masa Lintas Profesi di Catatan Terintegrasi
- **PPA Terlibat:** Ns. Siti Rahmawati, S.Kep.
- **Alur:**
  1. Sebelum operan shift malam, Ns. Siti ingin mengetahui arahan gizi dan instruksi DPJP terkini.
  2. Ns. Siti membuka sub-tab **Catatan Terintegrasi**.
  3. Antarmuka menampilkan kronologis terpadu dari:
     - Catatan Visite DPJP dr. Hendra (dengan lencana biru *Dokter*).
     - Catatan Asesmen Gizi dari Dietisien (dengan lencana abu-abu *Profesi Lain*).
     - Catatan SOAP Ns. Siti (dengan lencana hijau *Perawat*).
  4. Ns. Siti melihat status bahwa catatan dokter telah memverifikasi catatan paginya.
  5. Pada antarmuka Ns. Siti, **sama sekali tidak ada tombol verifikasi**, menegakkan integritas bahwa hanya DPJP yang menandatangani verifikasi telaah rekam medis.

### 2.4 Skenario 4: Pemantauan Deret Tanda Vital & Deteksi Perburukan
- **Alur:**
  1. Ns. Siti membuka tab **Vital Sign**.
  2. Sistem memuat seluruh riwayat tanda vital pasien selama 24 jam terakhir dari endpoint `GET /patient-vital-signs/episodes/{episodeId}` (`BE-RWI-121`).
  3. Ns. Siti beralih ke mode grafik (**Grafik Tren**) dan melihat pergerakan tekanan darah sistolik/diastolik serta frekuensi nadi yang stabil.
  4. Pada baris tabel, skor MAP (misal: 93 mmHg) dan skor EWS (misal: 1 / Risiko Rendah) tampil otomatis berdasarkan perhitungan server.
  5. Ketika Ns. Siti ingin mencatat TTV baru, modal pencatatan tegas tidak menyediakan kolom isian skala nyeri. Tersedia tautan pintas menuju tab **Monitoring Nyeri** untuk memastikan pengkajian nyeri dilakukan menggunakan instrumen klinis yang sah.

---

## 3. Komponen Antarmuka & Keputusan Desain

Sesuai aturan *Base Component Decision Gate*, seluruh elemen UI menggunakan komponen terstandar:

| Kebutuhan Antarmuka | Kandidat Komponen | Bukti Sumber | Status | Rekomendasi / Keputusan |
| :--- | :--- | :--- | :--- | :--- |
| **Wadah Panel Layanan & Tab** | `ClinicalContentPanel` | `src/components/ui/clinical-workspace` | `REUSE` | Pembungkus utama seluruh sub-tab Asuhan Keperawatan |
| **Manajemen Status (Loading, Error, Empty)** | `ClinicalStateBoundary` | `src/components/ui/clinical-workspace` | `REUSE` | Penanganan status data kosong, kegagalan jaringan, dan pemuatan |
| **Lini Masa Kartu Catatan (SOAP/Naratif/CPPT)** | `ClinicalTimelineItem` | Pola terpadu kartu klinis | `REUSE` | Kartu entri catatan berstruktur S-O-A-P dan naratif |
| **Tombol Interaktif** | `BaseButton` | `src/components/features/base-features/base-button` | `REUSE` | Tombol primer, sekunder, dan outline dengan ikon |
| **Modal Dialog Formulir** | `BaseModal` | `src/components/ui/form-pemeriksaan-ui` | `REUSE` | Modal dialog input SOAP dan catatan naratif |
| **Lencana Identitas Profesi & Verifikasi** | Status badge berdesain token | `nursing-workspace.module.css` | `COMPOSE` | Lencana Dokter, Perawat, Bidan, serta lencana Terverifikasi DPJP |
| **Visualisasi Grafik Tren TTV** | Pure SVG Chart | Sama dengan `DailyMonitoringSection` | `REUSE` | Grafik tren SVG tanpa dependensi library eksternal baru |
| **Sub-Tab Rencana Asuhan** | `CarePlanSection` | `sections/care-plan/care-plan-section.jsx` | `REUSE` | Dipertahankan apa adanya (`AC-5`) |
| **Sub-Tab Tindakan Harian** | `NursingInterventionSection` | `sections/intervention/nursing-intervention-section.jsx` | `REUSE` | Dipertahankan apa adanya (`AC-5`) |

---

## 4. Dokumentasi Endpoint API Bergaya Swagger

Seluruh interaksi jaringan Asuhan Keperawatan terhubung ke pengontrol klinis backend:

### `[Tags("HealthServices - ClinicalManagement - PatientIntegratedProgressNote")]`

#### 1. Menyimpan Catatan Perkembangan Terintegrasi (SOAP / Naratif)
- **Method & Path:** `POST /v1/health-services/clinical-management/patient-integrated-progress-notes`
- **Deskripsi:** Membuat entri CPPT baru. Jenis catatan (`NoteKind`) divalidasi terhadap profesi pengguna yang sedang masuk. Untuk perawat: `NursingSoap` (`2`) atau `NursingNarrative` (`3`).
- **Otorisasi:** `PatientIntegratedProgressNote : Create`
- **Request Body:**
  ```json
  {
    "patientId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "encounterId": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
    "noteDateTime": "2026-09-18T14:00:00Z",
    "noteKind": 2,
    "professionType": "Nurse",
    "subjectiveSummary": "Pasien menyatakan sesak berkurang...",
    "objectiveSummary": "TD 120/80 mmHg, Nadi 84x/m, RR 20x/m, SpO2 98%...",
    "assessmentSummary": "Pola napas tidak efektif teratasi sebagian...",
    "planSummary": "Pertahankan posisi semi-fowler, lanjutkan terapi O2 3 lpm...",
    "instruction": "Observasi per 4 jam",
    "evaluation": "Kondisi stabil",
    "isActive": true
  }
  ```
- **Response Success (200 OK):**
  ```json
  {
    "success": true,
    "statusCode": 200,
    "message": "CPPT berhasil dibuat.",
    "data": {
      "id": "7fa85f64-5717-4562-b3fc-2c963f66afa9",
      "progressNoteNumber": "CPPT-20260918-0042",
      "noteKind": 2,
      "noteKindName": "Catatan SOAP Keperawatan",
      "professionType": "Nurse",
      "isVerified": false,
      "createDateTime": "2026-09-18T14:00:05Z"
    }
  }
  ```
- **Response Error (400 Bad Request):** Jenis catatan tidak sesuai dengan profesi penulis.
- **Response Error (403 Forbidden):** Pengguna tidak bertugas di unit perawatan pasien.

#### 2. Membaca Lini Masa Catatan Terintegrasi Per Episode
- **Method & Path:** `GET /v1/health-services/clinical-management/patient-integrated-progress-notes/episodes/{episodeId}`
- **Deskripsi:** Mengambil daftar entri CPPT untuk satu episode. Mendukung parameter kueri `noteKind` untuk menyaring jenis spesifik (misal: `noteKind=2` untuk tab SOAP, `noteKind=3` untuk tab Catatan Keperawatan, atau tanpa filter untuk tab Catatan Terintegrasi).
- **Otorisasi:** `PatientIntegratedProgressNote : Read`
- **Query Parameters:**
  - `noteKind` (integer, opsional): `2` (SOAP), `3` (Naratif), `1` (Dokter).
  - `pageNumber` (integer, default `1`).
  - `pageSize` (integer, default `100`).

---

### `[Tags("HealthServices - ClinicalManagement - PatientVitalSign")]`

#### 3. Membaca Deret Tanda Vital Per Episode (`BE-RWI-121`)
- **Method & Path:** `GET /v1/health-services/clinical-management/patient-vital-signs/episodes/{episodeId}`
- **Deskripsi:** Membaca deret waktu tanda vital pasien sepanjang perawatan untuk tabel dan grafik. Menghasilkan skor MAP dan EWS murni dari server.
- **Otorisasi:** `PatientVitalSign : Read`
- **Query Parameters:**
  - `from` (string ISO, opsional): Batas awal waktu observasi (bawaan: 24 jam yang lalu).
  - `to` (string ISO, opsional): Batas akhir waktu observasi (bawaan: saat ini, maksimal rentang 7 hari).
- **Response Success (200 OK):**
  ```json
  {
    "success": true,
    "statusCode": 200,
    "data": [
      {
        "id": "8fa85f64-5717-4562-b3fc-2c963f66afb1",
        "observationDateTime": "2026-09-18T08:00:00Z",
        "bloodPressureSystolic": 120,
        "bloodPressureDiastolic": 80,
        "meanArterialPressure": 93.3,
        "pulseRate": 82,
        "respiratoryRate": 18,
        "temperature": 36.6,
        "oxygenSaturation": 98.0,
        "earlyWarningScore": 0,
        "ewsRiskLevel": "Low",
        "consciousnessStatus": "Compos Mentis"
      }
    ]
  }
  ```

#### 4. Mencatat Tanda Vital Pasien
- **Method & Path:** `POST /v1/health-services/clinical-management/patient-vital-signs`
- **Deskripsi:** Mencatat tanda vital perawat. Kolom isian nyeri ditolak server bila dikirim (`VAL-KEP-22c`).
- **Otorisasi:** `PatientVitalSign : Create`

---

## 5. Bukti Verifikasi Pengujian Otomatis

### 5.1 Unit Test Spesifik `FE-RWI-086`
```bash
cmd /c npx node --test tests/unit/inpatient-nursing-care.test.mjs
```
```text
✔ FE-RWI-086 AC-1: SOAP perawat tersimpan sebagai CPPT berjenis NursingSoap dan menunya menyaring jenis itu (FR-KEP-077) (4.2ms)
✔ FE-RWI-086 AC-2: Catatan Keperawatan tersimpan sebagai NursingNarrative dan menunya menyaring jenis itu (1.7ms)
✔ FE-RWI-086 AC-3: Catatan Terintegrasi menampilkan lini masa lintas profesi, tanpa tombol verifikasi bagi perawat (8.6ms)
✔ FE-RWI-086 AC-4: Vital Sign menampilkan deret per episode dari BE-RWI-121 (1.0ms)
✔ FE-RWI-086 AC-5: Rencana Asuhan dan Tindakan Harian tetap bekerja seperti sebelumnya (1.0ms)
✔ FE-RWI-086 Safety & Compliance: Aturan keselamatan RWI-AC-206 tampil pada SOAP dan Catatan Keperawatan (0.6ms)
ℹ tests 6 | pass 6 | fail 0 | cancelled 0 | skipped 0 | todo 0 | duration_ms 103.66ms
```

### 5.2 Regresi Seluruh Suite Keperawatan Rawat Inap (103 Tests Passing)
```bash
cmd /c npx node --test tests/unit/inpatient-nursing-*.test.mjs tests/unit/inpatient-clinical-instrument-renderer.test.mjs tests/unit/inpatient-daily-monitoring.test.mjs tests/unit/inpatient-case-management-evaluation.test.mjs tests/unit/inpatient-nursing-care.test.mjs
```
```text
ℹ tests 103 | pass 103 | fail 0 | cancelled 0 | skipped 0 | todo 0 | duration_ms 281.48ms
```

### 5.3 Validasi Linting ESLint
```bash
cmd /c npx eslint src/lib/hooks/health-services/inpatient-management/use-inpatient-nursing-care-notes.js \
                 src/lib/hooks/health-services/inpatient-management/use-inpatient-vital-sign-series.js \
                 src/components/view/health-services/inpatient-management/nursing-workspace/sections/nursing-care/ \
                 tests/unit/inpatient-nursing-care.test.mjs
```
*Hasil:* **Exit Code: 0 (0 error, 0 warning)**

### 5.4 Kompilasi Produksi Next.js
```bash
cmd /c npm run build
```
*Hasil:* **Exit Code: 0 (320/320 static pages generated, standalone runtime siap)**

---

## 6. Acceptance Criteria & Definition of Done

| Acceptance Criteria | Status | Bukti Implementasi & Hasil Pengujian |
| :--- | :---: | :--- |
| **AC-1** | ✅ Terpenuhi | SOAP perawat tersimpan sebagai CPPT berjenis `NursingSoap` (`noteKind = 2`), dan tab SOAP menyaring khusus data berjenis ini (`FR-KEP-077`). *(Test 1 Lulus)* |
| **AC-2** | ✅ Terpenuhi | Catatan Keperawatan tersimpan sebagai CPPT berjenis `NursingNarrative` (`noteKind = 3`), dan tab Catatan Keperawatan menyaring khusus jenis ini. *(Test 2 Lulus)* |
| **AC-3** | ✅ Terpenuhi | Catatan Terintegrasi menampilkan lini masa CPPT lintas profesi lengkap dengan status verifikasi DPJP, **tanpa tombol verifikasi bagi perawat** (`INV-DOK-11`). *(Test 3 Lulus)* |
| **AC-4** | ✅ Terpenuhi | Vital Sign memuat deret waktu observasi per episode (`BE-RWI-121`), menampilkan kalkulasi server MAP & EWS, tanpa isian nyeri, dan menyediakan toggle tabel/grafik tren. *(Test 4 Lulus)* |
| **AC-5** | ✅ Terpenuhi | Sub-tab Tindakan Harian (`NursingInterventionSection`) dan Rencana Asuhan (`CarePlanSection`) tetap berfungsi normal seperti sebelumnya. *(Test 5 Lulus)* |
| **Keselamatan Pasien** | ✅ Terpenuhi | Pengingat keselamatan `RWI-AC-206` tampil pada antarmuka SOAP dan Catatan Keperawatan: angka cairan dan obat di catatan narasi tidak merubah neraca cairan maupun MAR. *(Test 6 Lulus)* |

---

## 7. Catatan Penutup & Langkah Selanjutnya

1. Seluruh kode front-end untuk task `FE-RWI-086` telah terintegrasi dengan mulus pada ruang kerja keperawatan rawat inap (`nursing-workspace-sections.jsx`).
2. Backward-compatibility untuk tautan dan rute legacy `care-plan` dan `intervention` tetap terlindungi.
3. Task berikutnya pada roadmap keperawatan rawat inap adalah:
   - **`FE-RWI-087` — `FE-KEP-13` Obat & Alkes** (Pemberian Obat / MAR, Sliding Scale, Obat Bawaan, Resep Aktif, dan Pemakaian Alkes).
