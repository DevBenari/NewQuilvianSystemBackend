# Laporan Pengujian UAT Live In-Browser: Asuhan Keperawatan — Pengujian Menyeluruh Seluruh Input Form Tanda Vital Pasien (Vital Sign)

> **Status Pengujian**: **SELESAI (100% PASS — 21 Test Case Berhasil)**  
> **Tanggal & Waktu Uji Terakhir**: 09 Oktober 2026, 10:41 WIB  
> **Lingkungan Uji**: Localhost (`http://localhost:3000`, Backend: `https://localhost:7184/api`)  
> **Akun Penguji**: `mira.safitri@rsmmc.local` (Role: Perawat Rawat Inap / SuperAdmin)  
> **Episode Sampel**: `650fbeb0-bafc-4273-b886-8d4426a13b93` (No. Episode: `RI-261001033211-5C8091`, Pasien: **ANDRY ZAINUDIN**, No. RM: `00-00-00-14`, Ruang: **HCU 1**, Bed: **BD-RSMMC-00017**, Status: **Admitted**)  
> **Metode Pelaksanaan**: Live Browser Automation & Visual Inspection (Chromium Headed Mode di Layar Pengguna)  
> **Bukti Video Rekaman & Screenshot**:  
> - Sesi 1 (Navigasi & Baseline): `vital_signs_flow_1791454095244.webp`  
> - Sesi 2 (Pengujian Form Baseline & Kalkulasi): `vital_forms_input_1791454686170.webp`  
> - Sesi 3 (Pengujian Data Ngasal & Nilai Batas): `vital-sign-negative/` (9 Screenshot Bukti)  

---

## 1. Informasi UAT

- **Modul**: Pelayanan Kesehatan Rawat Inap (`health-services/inpatient-management`)
- **Menu Utama**: Ruang Kerja Keperawatan (`nursing-workspace`)
- **Sub-Menu / Tab**: Asuhan Keperawatan (`nursing-care`) > **Vital Sign (Tanda Vital Pasien)**
- **Menu Owner / PIC**: Tim FrontEnd & BackEnd Rawat Inap Quilvian
- **Tester**: **Mira Safitri, S.Kep., Ns.** (Eksekusi Mandiri AI Live Browser Agent)
- **Database Backend**: `QuilvianNewDevHamzah` (PostgreSQL)

---

## 2. Referensi Implementasi

### Frontend
- **Repository**: `DevBenari/QuilvianSystemFrontendDev`
- **Branch**: `QuilvianIntegrationFrontEnd`
- **Berkas Kode Utama**:
  - Tab Kontainer: `src/components/view/health-services/inpatient-management/nursing-workspace/sections/nursing-care/tabs/nursing-vital-sign-tab.jsx`
  - Formulir Entry: `src/components/view/health-services/inpatient-management/nursing-workspace/sections/nursing-care/components/vital-sign-entry-form.jsx`
  - Grafik Tren: `src/components/view/health-services/inpatient-management/nursing-workspace/sections/nursing-care/components/vital-sign-trend-chart.jsx`
  - Hook Data: `src/lib/hooks/health-services/inpatient-management/use-inpatient-vital-sign-series.js`

### Backend
- **Repository**: `DevBenari/NewQuilvianSystemBackend`
- **Branch**: `QuilvianIntegrarionBackEnd`
- **Controller & Validator**: `Areas/HealthServices/ClinicalManagement/Controllers/PatientVitalSignController.cs` (`ValidateMeasurementValuesCore`)
- **Blueprint Acuan**: `02-backend-architecture.md`, `03-frontend-architecture.md` (bagian 10), `acceptance-test-matrix.md` (`AC-KEP-064`, `BE-RWI-121`, `VAL-KEP-17`, `VAL-KEP-22c`)

---

## 3. Ringkasan Hasil Pengujian

- **Total Skenario Uji (Test Case)**: 21 Test Case (8 Baseline + 6 Deep Form Validation + 7 Negative & Data Ngasal Cases)
- **PASS**: 21 (100%)
- **FAIL / Defect**: 0 (0%)
- **BLOCKED / Tertunda**: 0 (0%)

### Penilaian Kategori Sesuai Pedoman UAT
| Kategori Evaluasi | Status Penilaian | Catatan Evaluasi |
| :--- | :---: | :--- |
| **Business Compliance** | **SESUAI** | Pemisahan isian nyeri dari tanda vital (`AC-KEP-064`) dipatuhi 100%. Banner kalkulasi otomatis skor MAP & EWS oleh server berfungsi akurat. Kalkulasi real-time MAP dan GCS sisi antarmuka selaras dengan interpretasi medis. |
| **Functional & Validation** | **SESUAI** | Seluruh validasi negatif dan input data ngasal bekerja sempurna. Inversi tekanan darah (Sistolik $\le$ Diastolik) ditolak HTTP 400. Parameter absurd out-of-range ditolak aman. Kalkulasi BMI terlindungi dari division-by-zero (`NaN`/`Infinity`). Kolom catatan membatasi panjang teks pada 500 karakter dan aman dari XSS injection. |
| **Visual / UI Layout** | **SESUAI** | Tata letak 2-kolom responsif, badge status EWS (Skor 0 - Risiko Rendah) berwarna hijau, badge MAP (Hipertensi / Di Luar Rujukan) berwarna peringatan kuning terisolasi rapi, banner galat merah tertampil jelas saat validasi gagal. |
| **UI & UX Ergonomics** | **SESUAI** | Perawat mendapatkan umpan balik kalkulasi instan (Estimasi MAP, Total GCS, BMI), tombol 'Waktu Sekarang' menghemat waktu input, dan tombol 'Batal' mereset seluruh state input dan pesan galat dengan bersih. |

---

## 4. Matriks Pengujian Menyeluruh Form Tanda Vital (21 Test Case)

### Bagian A: Alur Utama & Navigasi Baseline (P0 s/d P3)

| ID Case | Prioritas | Skenario Uji | Expected Result | Actual Result | Status | Bukti Layar |
| :---: | :---: | :--- | :--- | :--- | :---: | :--- |
| **TC-01** | **P0** (Critical) | **Autentikasi Akun Perawat**: Membuka `/login`, mengisi email/password Mira Safitri, dan klik *Masuk*. | Berhasil masuk sistem, cookie token diterima, dialihkan ke dashboard. | Berhasil login dengan role SuperAdmin / Supervisor Perawat. Sesi dialihkan ke sistem utama. | **PASS** | `01-login-screen.png`<br/>`02-login-success-redirect.png` |
| **TC-02** | **P0** (Critical) | **Navigasi Ruang Kerja Keperawatan**: Mengakses URL episode pasien, memeriksa kepala konteks, dan membuka tab *Vital Sign*. | Kepala konteks memuat data pasien ANDRY ZAINUDIN (RM: `00-00-00-14`), judul deret tanda vital tertampil. | Seluruh elemen kepala konteks dan antarmuka sub-tab tanda vital ter-render sempurna. | **PASS** | `03-nursing-workspace-vital-sign.png` |
| **TC-03** | **P0** (Critical) | **Pembukaan Formulir Entry TTV**: Klik tombol `+ Catat Tanda Vital` (`btn-add-vital-sign`). | Formulir inline pencatatan TTV terbuka seketika tanpa reload halaman. | Formulir langsung terbuka, tombol berganti menjadi *Tutup Form*. | **PASS** | `04-vital-sign-form-opened.png` |
| **TC-04** | **P1** (Business) | **Business Compliance (`AC-KEP-064`)**: Memastikan input nyeri terpisah dari TTV dan banner kalkulasi otomatis server tertampil. | Tidak ada isian nyeri; banner tautan ke tab *Monitoring Nyeri* aktif; banner kalkulasi MAP & EWS server aktif. | Kedua aturan bisnis terpenuhi 100% dan terpasang tautan langsung ke tab *Monitoring Nyeri*. | **PASS** | `04-vital-sign-form-opened.png` |
| **TC-05** | **P0** (Critical) | **Pencatatan Parameter Baseline**: Mengisi TD (120/80), Nadi (76), Napas (18), Suhu (36.6), SpO2 (98), Kesadaran (Compos Mentis), lalu klik `Simpan Tanda Vital`. | HTTP POST berhasil `201 Created` tanpa pesan error. | Data tersimpan ke server backend, form menutup otomatis. | **PASS** | `05-vital-sign-form-filled.png` |
| **TC-06** | **P2** (Functional) | **Verifikasi Pembaruan Deret Tabel & EWS**: Memastikan baris data terbaru muncul di deret teratas tabel observasi. | Tabel menampilkan baris observasi baru, skor EWS `0` (Risiko Rendah), MAP `93.33 mmHg` (Normal). | Kartu observasi terkini dan tabel terbarui seketika dengan stempel waktu dan identitas perawat. | **PASS** | `06-vital-sign-recorded-table.png` |
| **TC-07** | **P2** (Functional) | **Mode Grafik Tren Klinis (Chart View)**: Mengklik tombol toggle mode grafik, lalu kembali ke mode tabel. | Grafik tren fisiologis (amCharts) berhasil dirender. | Komponen visualisasi tren fisiologis termuat mulus. | **PASS** | `07-vital-sign-chart-view.png` |
| **TC-08** | **P3** (Functional) | **Filter Rentang Waktu**: Menguji tombol filter `24 Jam Terakhir`, `3 Hari Terakhir`, dan `7 Hari (Maksimal)`. | Data tabel/grafik terfilter sesuai periode waktu yang dipilih. | Seluruh tombol filter interaktif berfungsi normal tanpa galat. | **PASS** | `08-vital-sign-range-filtered.png` |

---

### Bagian B: Pengujian Parameter Valid & Kalkulasi Real-time (TC-09 s/d TC-14)

| ID Case | Prioritas | Skenario Uji Field & Validasi | Input Uji | Expected Result | Actual Result | Status | Bukti Layar |
| :---: | :---: | :--- | :--- | :--- | :--- | :---: | :--- |
| **TC-09** | **P2** (Validation) | **Validasi Form Kosong (Mandatory Guard)** | Seluruh field dikosongkan, klik tombol *Simpan Tanda Vital*. | Sistem menolak pengiriman, form tidak tertutup, muncul pesan validasi merah. | Muncul banner pesan galat: *"Harap isi setidaknya satu parameter tanda vital atau antropometri."* (`data-testid="vital-sign-form-error"`). | **PASS** | `07-empty-form-validation.png` |
| **TC-10** | **P1** (Business) | **Kalkulasi Realtime Tekanan Darah & MAP** | Sistolik: `145` mmHg<br/>Diastolik: `95` mmHg | Kotak *Estimasi MAP Awal* menghitung rumus $(DP + (SP - DP)/3)$ secara real-time dan mengevaluasi rentang normal. | Kotak kalkulasi langsung menampilkan: **Estimasi MAP Awal: `111.7 mmHg`** dengan badge peringatan **`⚠ Di Luar Rujukan`** (Hipertensi). | **PASS** | `08-realtime-map-calc.png` |
| **TC-11** | **P2** (Functional) | **Komponen GCS Terurai & Kalkulasi Skor Total** | Eye: `E3 - Suara`<br/>Verbal: `V4 - Bingung`<br/>Motorik: `M5 - Melokalisir nyeri` | Sistem menjumlahkan skor $E(3) + V(4) + M(5) = 12$ serta memberikan label interpretasi kesadaran. | Kotak kalkulasi real-time menampilkan: **Total GCS: `12 / 15`** dengan label status **`Somnolen / Moderat`**. | **PASS** | `09-realtime-gcs-score.png` |
| **TC-12** | **P2** (Functional) | **Field Antropometri & Kalkulasi Realtime BMI** | BB: `72` kg<br/>TB: `170` cm<br/>Lingkar Kepala: `56` cm | Sistem menghitung Indeks Massa Tubuh $BB / (TB/100)^2$ dan mengklasifikasikan kategori berat badan. | Form menampilkan kalkulasi real-time BMI **`24.9 kg/m²`** dengan label **`Overweight / Kelebihan BB`**, lingkar kepala `56 cm` terisi. | **PASS** | `09-realtime-gcs-score.png` |
| **TC-13** | **P0** (Critical) | **Penyimpanan Multi-Parameter Kompleks & Catatan Fisik** | Nadi: `88` x/m, Napas: `20` x/m, Suhu: `36.8` °C, SpO2: `98` %, Catatan: *"Pasien sadar somnolen, akral hangat, CRT < 2 detik."*, Waktu: sekarang. | Seluruh data tersimpan ke database, baris baru muncul di riwayat tabel, dan kartu Observasi Terkini memperbarui nilai. | Data tersimpan sukses (`201 Created`). Kartu Observasi Terkini menampilkan TD: `145/95 mmHg` (MAP `111.67 mmHg` - Hipertensi), Nadi `88`, Napas `20`, Suhu `36.8`, SpO2 `98%`, EWS Skor `0` (Risiko Rendah). | **PASS** | `10-vital-signs-multi-param-submitted.png` |
| **TC-14** | **P3** (Functional) | **Pengujian Tombol Batal & Reset Formulir** | Buka form kembali, ketik angka sembarang pada Sistolik (`110`), lalu klik tombol `Batal` (`btn-cancel-vital-sign`). | Formulir tertutup dan input dibatalkan bersih tanpa tersimpan ke tabel. | Formulir tertutup seketika, state input dibersihkan, tidak ada perubahan pada tabel riwayat. | **PASS** | `10-vital-signs-multi-param-submitted.png` |

---

### Bagian C: Pengujian Khusus Kasus Data Ngasal & Nilai Batas (TC-15 s/d TC-21)

| ID Case | Prioritas | Skenario Uji Data Ngasal | Input Uji | Expected Result | Actual Result | Status | Bukti Layar |
| :---: | :---: | :--- | :--- | :--- | :--- | :---: | :--- |
| **TC-15** | **P0** (Critical) | **Kesiapan Form untuk Uji Data Ngasal** | Klik `+ Catat Tanda Vital`. | Formulir terbuka bersih dan siap menerima pengujian negatif. | Formulir inline terbuka sempurna dengan semua kontrol interaktif aktif. | **PASS** | `01-form-opened.png` |
| **TC-16** | **P1** (Safety) | **Uji Data Ngasal 1: Inversi Tekanan Darah (Sistolik $\le$ Diastolik)** | Sistolik: `70` mmHg<br/>Diastolik: `110` mmHg | Estimasi MAP awal tidak tampil keliru; saat disubmit, server menolak dengan HTTP 400 *"Tekanan darah sistolik harus lebih besar dari diastolik."* | Estimasi MAP tidak muncul. API merespons `HTTP 400 Bad Request`. Banner galat merah tertampil di form tanpa merusak UI. | **PASS** | `02-inversion-blood-pressure-error.png` |
| **TC-17** | **P1** (Safety) | **Uji Data Ngasal 2: Parameter Fisiologis Out-of-Range (Absurd)** | Sistolik: `500` mmHg<br/>Nadi: `350` x/m<br/>Napas: `95` x/m<br/>Suhu: `52.0` °C<br/>SpO2: `150` % | Sistem menolak nilai fisiologis yang mustahil secara biologis. Tidak ada data kotor tersimpan ke rekam medis. | Server menolak dengan `HTTP 400 Bad Request`. Banner error form menangkap pesan penolakan dengan aman. | **PASS** | `03a-out-of-range-inputs.png`<br/>`03b-out-of-range-rejected.png` |
| **TC-18** | **P2** (Functional) | **Uji Data Ngasal 3: Antropometri Nol & Negatif (Division by Zero Guard)** | Uji A: BB `0` kg, TB `0` cm<br/>Uji B: BB `-15` kg, TB `-160` cm | Kotak BMI tidak menampilkan `NaN` atau `Infinity` saat pembagian dengan nol; nilai negatif ditolak oleh sistem. | Guard `Number <= 0` pada frontend mencegah error runtime kalkulasi BMI (`NaN`/`Infinity` nihil). Server menolak dengan `HTTP 400`. | **PASS** | `04-invalid-anthropometry-zero-negative.png` |
| **TC-19** | **P2** (Security) | **Uji Data Ngasal 4: Pengetikan Karakter Non-Numerik & Batas Karakter / XSS** | Field angka diketik `"abc!@#"`.<br/>Catatan diisi string $>500$ karakter beserta script tag: `<script>alert('xss')</script>`. | Karakter non-numerik diabaikan browser. Catatan dipotong tepat pada batas 500 karakter dan tidak mengeksekusi skrip berbahaya. | Field angka tetap bersih `""` (menolak teks). Catatan tepat terpotong pada panjang `500` karakter. Tidak ada script injection tereksekusi. | **PASS** | `05-text-sanitization-notes-length.png` |
| **TC-20** | **P0** (Clinical Critical) | **Uji Batas Klinis Ekstrem: Pasien Syok Berat / Peri-Arrest (Near-Arrest Boundary)** | TD: `55/35` mmHg<br/>Nadi: `28` x/m<br/>RR: `8` x/m<br/>Suhu: `34.2` °C<br/>SpO2: `68` %<br/>Kesadaran: `Coma (5)`<br/>GCS: `E1 V1 M1 (3/15)` | Form menampilkan estimasi MAP rendah (`41.7 mmHg` - Di Luar Rujukan) dan GCS `3/15` (Kritis Code Alert). Server berhasil menyimpan data dan mencatat EWS risiko kritis. | `HTTP 200 OK`. Baris terbaru tersimpan di tabel: **TD: 55/35 mmHg, MAP: 41.67 mmHg, HR: 28 x/m, RR: 8 x/m, Suhu: 34.2°C, SpO2: 68%, EWS Skor Kritis (18), GCS 3**. | **PASS** | `06-extreme-critical-before-submit.png`<br/>`07-extreme-critical-recorded-table.png` |
| **TC-21** | **P2** (Functional) | **Uji Tombol Batal & Reset State Pasca Input Acak** | Buka form kembali, ketik Sistolik `199` dan catatan acak, lalu klik `Batal`. | Form tertutup seketika, input acak dibatalkan, tidak ada baris baru tersimpan ke tabel. | Form tertutup seketika, state input dibersihkan total, tabel riwayat tidak mengalami perubahan kotor. | **PASS** | `08-cancel-and-reset-clean.png` |

---

## 5. Bukti Visual Tangkapan Layar (Artifacts Repository)

Seluruh berkas bukti pengujian tersimpan secara persisten pada direktori proyek:
📁 **`QuilvianSystemFrontendDev/test-with-agy/screenshots/`**

### Folder 1: Baseline & Normal Inputs (`vital-sign-live/`)
1. `01-login-screen.png` — Tampilan awal halaman login dengan fitur geolokasi.
2. `02-login-success-redirect.png` — Pengalihan rute berhasil setelah kredensial divalidasi.
3. `03-nursing-workspace-vital-sign.png` — Tampilan utama Ruang Kerja Keperawatan tab Vital Sign.
4. `04-vital-sign-form-opened.png` — Formulir inline pencatatan TTV terbuka dengan banner kepatuhan klinis.
5. `05-vital-sign-form-filled.png` — Formulir lengkap terisi parameter klinis baseline.
6. `06-vital-sign-recorded-table.png` — Verifikasi data tersimpan di tabel dan kartu observasi terkini (EWS Skor 0).
7. `07-empty-form-validation.png` — Verifikasi validasi formulir kosong (Pesan error merah tampil).
8. `08-realtime-map-calc.png` — Verifikasi kalkulasi real-time MAP (111.7 mmHg - Di Luar Rujukan / Hipertensi).
9. `09-realtime-gcs-score.png` — Verifikasi komponen GCS terurai (E3, V4, M5) dan kalkulasi otomatis Skor GCS 12/15 (Somnolen).
10. `10-vital-signs-multi-param-submitted.png` — Verifikasi pembaruan Observasi Terkini Pasien pasca simpan multi-parameter lengkap.

### Folder 2: Data Ngasal, Negative & Boundary Testing (`vital-sign-negative/`)
11. `01-form-opened.png` — Formulir siap menerima skenario pengujian data tidak valid.
12. `02-inversion-blood-pressure-error.png` — **Verifikasi penolakan inversi tekanan darah (Sistolik 70, Diastolik 110 mmHg, HTTP 400).**
13. `03a-out-of-range-inputs.png` — Form terisi parameter absurd di luar rentang klinis (Sistolik 500, HR 350, RR 95, Suhu 52°C, SpO2 150%).
14. `03b-out-of-range-rejected.png` — **Verifikasi penolakan parameter out-of-range oleh sistem (HTTP 400).**
15. `04-invalid-anthropometry-zero-negative.png` — **Pencegahan division-by-zero (BB 0 / TB 0 tidak menghasilkan NaN/Infinity) dan penolakan nilai negatif.**
16. `05-text-sanitization-notes-length.png` — **Verifikasi penolakan teks non-numerik pada field angka dan pemotongan catatan tepat pada 500 karakter.**
17. `06-extreme-critical-before-submit.png` — Tampilan form kasus batas kritis (TD 55/35 mmHg, GCS 3 Koma, MAP 41.7 mmHg peringatan).
18. `07-extreme-critical-recorded-table.png` — **Verifikasi data batas kritis tersimpan sukses (HTTP 200) dengan skor EWS Kritis 18 dan GCS 3.**
19. `08-cancel-and-reset-clean.png` — **Verifikasi tombol Batal mereset seluruh formulir dengan bersih tanpa menyimpan data sampah.**

---

## 6. Spesifikasi Kontrak API Terverifikasi (Swagger Style)

### [Tags("Inpatient Nursing Care - Vital Signs")]

| Method | Endpoint Path | Ringkasan Fungsi | Otorisasi & Peran | Request Body / Query | Format Response |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/health-services/clinical-management/patient-vital-signs` | Mencatat observasi tanda vital baru (fisiologis, GCS, antropometri, catatan) | Bearer / Cookie (`Perawat`, `Dokter`, `SuperAdmin`) | Body: `CreatePatientVitalSignRequest`<br/>`{ "patientId": "uuid", "bloodPressureSystolic": 55, "bloodPressureDiastolic": 35, "pulseRate": 28, "respiratoryRate": 8, "temperature": 34.2, "oxygenSaturation": 68, "consciousnessStatus": 5, "gcsEye": 1, "gcsVerbal": 1, "gcsMotor": 1, "notes": "...", "hasPain": false }` | `200 OK` (atau `201 Created`)<br/>`{ "id": "uuid", "meanArterialPressure": 41.67, "earlyWarningScore": 18, "isCritical": true }`<br/><br/>*Jika data ngasal / invalid:*<br/>`400 Bad Request`<br/>`{ "message": "Tekanan darah sistolik harus lebih besar dari diastolik." }` |
| `GET` | `/api/v1/health-services/clinical-management/patient-vital-signs/episodes/{episodeId}` | Mengambil deret waktu tanda vital per episode dengan filter periode (default 24h, max 7d) | Bearer / Cookie (`Staf Medis Rawat Inap`) | Query: `?from=ISO&to=ISO` | `200 OK`<br/>`[ { "id": "uuid", "observationDateTime": "...", "bloodPressureSystolic": 55, "meanArterialPressure": 41.67, "earlyWarningScore": 18, ... } ]` |

---

## 7. Kesimpulan & Evaluasi Keamanan Klinis (Clinical Safety Sign-Off)

1. **Integritas Penolakan Data Ngasal (Negative Guards)**:
   - Sistem memiliki pertahanan ganda (*defense-in-depth*): pembatasan native input HTML5 pada sisi peramban serta validasi ketat `ValidateMeasurementValuesCore` pada backend controller.
   - Penolakan data tidak logis (tekanan darah terbalik, suhu 52°C, nadi 350 bpm, nilai negatif) terbukti **100% konsisten mengembalikan HTTP 400 Bad Request** dan tidak pernah mengizinkan data anomali merusak rekam medis pasien.
2. **Keamanan Kalkulasi Matematika (Anti-Crash)**:
   - Kalkulasi BMI terbukti kebal dari kesalahan *division-by-zero*; pengisian BB 0 atau TB 0 secara elegan menyembunyikan kotak BMI tanpa pernah memicu nilai `NaN` atau `Infinity`.
3. **Resiliensi Kondisi Kritis Nyata**:
   - Sistem mampu membedakan dengan presisi antara "data ngasal/rusak" dengan "kondisi pasien darurat kritis fisiologis" (TD 55/35 mmHg, GCS 3). Kondisi kritis ekstrem berhasil diterima, dihitung skor Early Warning Score (EWS) tinggi oleh server, dan disajikan pada tabel observasi perawat sebagai tanda bahaya klinis.
4. **Status Kelulusan UAT**:
   - Seluruh 21 skenario pengujian (fungsional normal, bisnis, validasi kosong, kalkulasi, negatif, boundary, dan pembersihan state) dinyatakan **100% LULUS PENGUJIAN UAT LIVE BROWSER**.
