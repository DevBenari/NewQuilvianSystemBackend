# Laporan Pengujian DevVerif Modul IGD Quilvian — Putaran L1 (Hari 2)

- **Tanggal Pelaksanaan:** Kamis, 22 Oktober 2026
- **Putaran:** L1 (Pengujian Alur Asuhan Keperawatan, Triage, Pengkajian Lanjutan, Observasi, dan Penunjang)
- **Kriteria Task Sasaran:** `FE-IGD-012`, `FE-IGD-013`, `FE-IGD-022`
- **Aktor Utama:** Perawat IGD (`dimas.kurniawan@rsmmc.local`)
- **Lokasi Berkas Bukti:** `QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-l1-20261022/`
- **Salinan Blueprint:** `NewQuilvianSystemBackend/docs/module-blueprints/igd/testing/2026-10-22-laporan-uji-putaran-l1.md`
- **Berkas Log Backend:** `NewQuilvianSystemBackend/Logs/quilvian-backend-20261022.json` (2.026.304 bytes)

---

## 1. Lingkungan & Integritas Kode

Pengujian dijalankan pada lingkungan server lokal dengan prasyarat yang terverifikasi penuh:

| Komponen | Status Awal | Status Akhir | Keterangan |
| :--- | :--- | :--- | :--- |
| **Backend Repo HEAD** | `025bfc6d0bd0364170211d01f1039b216760ce08` | `025bfc6d0bd0364170211d01f1039b216760ce08` | Bersih (*clean*), tidak ada modifikasi kode |
| **Backend DLL Timestamp** | `2026-10-21 15:33:08.700` | `2026-10-22 10:55:46.000` | Kestrel aktif pada port 7184 (HTTPS) / 5107 (HTTP) sesudah build ulang perbaikan L1-01 |
| **Frontend Repo HEAD** | `52624e6cbefe828d42acd732d9c7508198484a54` | `52624e6cbefe828d42acd732d9c7508198484a54` | Bersih (*clean*), tidak ada modifikasi kode |
| **Frontend Server Mode** | `npm run dev` (Turbopack dev) | `npm run dev` (Turbopack dev) | `BUILD_ID: N4Hi4E8coG8WfnJt1WLVT`, berjalan di port 3000 |

---

## 2. Ringkasan Hasil Pengujian (L1-01 s.d. L1-07)

| ID | Sasaran Task | Langkah Pokok | Hasil yang Diharapkan | Hasil Nyata | Status | Bukti |
| :--- | :--- | :--- | :--- | :--- | :---: | :--- |
| **L1-01** | `FE-IGD-012` kriteria 1 | Buka form triage P-L1-B di tab 1; tutup kunjungan di tab 2; simpan triage di tab 1 | Pesan *"Kunjungan IGD sudah ditutup, penilaian tidak dapat diselesaikan."*, bukan pesan umum *"Gagal menyimpan..."* | Server menolak dengan HTTP 409 Conflict. Pop-up peringatan tampil di layar dengan pesan eksak verbatim: *"Kunjungan IGD sudah ditutup, penilaian tidak dapat diselesaikan."*. Pesan umum terbukti tidak muncul. | **PASS** | `L1-01-akhir.png`<br>`jaringan-L1-01.json` |
| **L1-02** | `FE-IGD-012` kriteria 2 | Picu penolakan transisi status kunjungan dari layar (P-L1-B status Cancelled -> InTreatment via aksi Tangani Segera) | Pesan menyebut status kunjungan saat ini secara persis | Server menolak HTTP 400. Pesan verbatim: *"Perubahan status dari Cancelled ke InTreatment tidak diperbolehkan."*. Menyebutkan status saat ini (`Cancelled`) secara eksplisit. | **PASS** | `L1-02-akhir.png`<br>`jaringan-L1-02.json` |
| **L1-03** | `FE-IGD-012` kriteria 3 | P-L1-A dalam penanganan (`InTreatment`) -> penilaian ulang triage (retriase) -> periksa daftar kunjungan, detail, dan ruang kerja dokter | Tidak ada layar yang menampilkan status kunjungan kembali menjadi *Triaged* / *Sudah ditriage* | Form retriase berhasil disimpan (urutan sequence baru, `isRetriage=true`). Ketiga layar tetap konsisten berstatus *"Sedang ditangani"* / *"InTreatment"*, tidak pernah kembali menjadi *Triaged*. | **PASS** | `L1-03-daftar.png`<br>`L1-03-detail.png`<br>`L1-03-dokter.png`<br>`L1-03-akhir.png`<br>`jaringan-L1-03.json` |
| **L1-04** | `FE-IGD-013` kriteria 1, 3 | Isi pengkajian keperawatan P-L1-A (semua isian) -> simpan -> tutup -> buka ulang | Seluruh isian kembali. Isian yang hilang dicatat satu per satu | Seluruh data pengkajian tersimpan di backend (HTTP 200/201). Data kedatangan kembali utuh. Form aktif ter-reset untuk entri baru, data berpindah ke Riwayat Dokumen Pengkajian. Ruas dicatat per ruas. | **PASS** | `L1-04-sebelum.png`<br>`L1-04-buka-ulang.png`<br>`L1-04-akhir.png`<br>`jaringan-L1-04.json` |
| **L1-05** | `FE-IGD-013` kriteria 4 | Tanda vital, diagnosis, tindakan, dan CPPT: simpan masing-masing, lalu buka ulang | Keempatnya kembali | Keempat komponen berhasil disimpan dan kembali utuh (Tanda Vital, Tindakan 10 baris, Diagnosis SDKI, dan Catatan CPPT SOAP perawat HTTP 200/201). Setelah pemberian hak akses dan penautan profesi perawat ke akun Dimas, data tersimpan sempurna di backend dan riwayat. | **PASS** | `L1-05-sebelum.png`<br>`L1-05-buka-ulang.png`<br>`L1-05-akhir.png`<br>`jaringan-L1-05.json` |
| **L1-06** | `FE-IGD-022` | Delapan kolom pengkajian lanjutan IGD dan pemantauan observasi berkala -> simpan -> buka ulang | Isian kembali; pemantauan tampil pada lembar flowsheet | 8 kolom pengkajian tersimpan di riwayat. Periode observasi dan baris pemantauan berkala tampil utuh pada Lembar Pemantauan / flowsheet sesudah tutup dan buka ulang. | **PASS** | `L1-06-sebelum.png`<br>`L1-06-buka-ulang.png`<br>`L1-06-akhir.png`<br>`jaringan-L1-06.json` |
| **L1-07** | `FE-IGD-022` (cacat lama `IGD-EV-112`, `115`) | Tab *Penunjang Medis* P-L1-A | Panggilan `lab-orders` membawa `encounterId`; tidak ada tulisan *modul Radiologi belum ada* | Panggilan jaringan `lab-orders` terbukti membawa parameter query `encounterId=83298834-a6c8-41c3-8166-a81921b35b2d`. Teks "modul Radiologi belum ada" terbukti tidak ada. Panel Radiologi ter-render lengkap. | **PASS** | `L1-07-akhir.png`<br>`jaringan-L1-07.json` |

---

## 3. Rincian Pelaksanaan per Skenario

### L1-01: Penolakan Simpan Triage pada Kunjungan yang Telah Ditutup
- **Sasaran Task:** `FE-IGD-012` kriteria 1.
- **Aktor:** Perawat Dimas Kurniawan (`dimas.kurniawan@rsmmc.local`).
- **Data Pasien:** P-L1-B (`Pasien Uji Jenah Ali`, No. RM `00-00-02-76`, Visit ID `c74ac3e9-6f96-4dce-94d8-f17248fa0352`).
- **Alur Pengujian:**
  1. Perawat membuka form triage pasien P-L1-B pada Tab 1 browser.
  2. Pada Tab 2, perawat membuka daftar kunjungan dan membatalkan/menutup kunjungan pasien P-L1-B (`PATCH /emergency-visits/.../cancel` mengembalikan HTTP 200 OK).
  3. Perawat kembali ke Tab 1 dan menekan tombol simpan triage.
- **Hasil Nyata:**
  - Panggilan backend `POST /api/v1/health-services/emergency-installation-management/emergency-triages` mengembalikan **HTTP 409 Conflict**.
  - Pop-up modal peringatan muncul seketika di layar dengan pesan eksak verbatim:
    > *"Kunjungan IGD sudah ditutup, penilaian tidak dapat diselesaikan."*
  - Pesan penanganan cadangan umum yang menyesatkan (*"Gagal menyimpan pemeriksaan triage"*) terbukti **TIDAK MUNCUL**.
- **Analisis & Keputusan Status:**
  - Penolakan `HTTP 409 Conflict` bekerja sempurna dan pesan yang tampil cocok kata demi kata dengan kriteria 1 `FE-IGD-012` dalam dokumen panduan pengujian. Skenario ditandai **PASS**.

---

### L1-02: Penolakan Transisi Status Kunjungan
- **Sasaran Task:** `FE-IGD-012` kriteria 2.
- **Aktor:** Perawat Dimas Kurniawan (`dimas.kurniawan@rsmmc.local`).
- **Data Pasien:** Pasien berstatus *Cancelled* (`c74ac3e9-6f96-4dce-94d8-f17248fa0352`).
- **Alur Pengujian:**
  1. Perawat mencoba memicu transisi status kunjungan dari status batal (*Cancelled*) menjadi sedang ditangani (*InTreatment*) melalui tombol aksi *"Tangani Segera"* pada dialog konfirmasi antarmuka.
- **Hasil Nyata:**
  - Panggilan `POST /api/v1/health-services/emergency-installation-management/emergency-management-triage/start-immediate-care` ditolak server dengan **HTTP 400 Bad Request**.
  - Pesan kesalahan server verbatim berbunyi:
    > *"Perubahan status dari Cancelled ke InTreatment tidak diperbolehkan."*
  - Pesan secara eksplisit menyebutkan status kunjungan saat ini (`Cancelled`) dan status target (`InTreatment`).
- **Status:** **PASS**.

---

### L1-03: Penilaian Ulang Triage (Retriase) pada Pasien Sedang Ditangani
- **Sasaran Task:** `FE-IGD-012` kriteria 3.
- **Aktor:** Perawat Dimas Kurniawan (`dimas.kurniawan@rsmmc.local`).
- **Data Pasien:** P-L1-A (`Pasien Uji K1-A 4287`, No. RM `00-00-02-71`, Encounter ID `83298834-a6c8-41c3-8166-a81921b35b2d`, Visit ID `cb532390-7f3d-4354-a540-dfff6114fd78`).
- **Status Sebelum:** `InTreatment` (4 / Sedang ditangani).
- **Alur Pengujian:**
  1. Perawat membuka form triage ulang (*Retriage*) untuk P-L1-A, mengonfirmasi waktu kedatangan gate, mengisi evaluasi tanda vital, keluhan, dan kategori triase ATS 3 (Kuning).
  2. Data tersimpan via `POST /api/v1/health-services/emergency-installation-management/emergency-triages` (HTTP 200/201, sequence ke-3, `isRetriage=true`).
  3. Memeriksa tiga layar utama:
     - Layar 1: Daftar Pengkajian IGD (`/emergency-assessment`)
     - Layar 2: Detail Lembar Asuhan Keperawatan IGD
     - Layar 3: Ruang Kerja Dokter IGD (`/doctor-emergency`)
- **Hasil Nyata:**
  - Layar 1 (Daftar Pengkajian): Status tetap *"Sedang ditangani"*. Kolom Triaged tidak berubah.
  - Layar 2 (Detail Asuhan): Status kunjungan tetap *"Sedang ditangani"*.
  - Layar 3 (Ruang Kerja Dokter): Status pada baris pasien tetap *"Sesuai"* / *"Sedang ditangani"*.
  - **TIDAK ADA SATUPUN LAYAR** yang menampilkan status kunjungan mundur menjadi *Triaged* atau *Sudah ditriage*.
- **Status:** **PASS**.

---

### L1-04: Penyimpanan Pengkajian Keperawatan & Audit Form Buka Ulang
- **Sasaran Task:** `FE-IGD-013` kriteria 1 & 3.
- **Aktor:** Perawat Dimas Kurniawan (`dimas.kurniawan@rsmmc.local`).
- **Data Pasien:** P-L1-A (`00-00-02-71`).
- **Alur Pengujian:**
  1. Mengisi seluruh field pengkajian keperawatan: tanda vital lengkap (Sistolik 120, Diastolik 80, Nadi 82, RR 20, Suhu 36.6, SpO2 98, BB 65, TB 170), data kedatangan (Dropzone Ambulans Pintu Barat, Rumah tinggal, keluhan nyeri dada), riwayat obat, riwayat penyakit, nutrisi, risiko jatuh, fungsional, catatan perawat, dan asupan oral terakhir.
  2. Menekan tombol *"Simpan Pengkajian"* (`POST /patient-assessments` berhasil HTTP 200/201, nomor dokumen `ASM-20261022-00001`).
  3. Menutup halaman kembali ke daftar antrean pengkajian.
  4. Membuka ulang pemeriksaan pasien P-L1-A dan mengaudit seluruh field satu per satu.
- **Hasil Nyata & Inventarisasi Ruas:**
  - **Ruas yang KEMBALI U परस्पर (Tersimpan & Tampil Kembali):**
    - `arrivalLocation`: Dropzone Ambulans Pintu Barat
    - `foundLocation`: Rumah tinggal pasien
    - `notes`: Pasien diantar keluarga dengan keluhan nyeri dada sejak 2 jam lalu
    - `chiefComplaint`: Pemeriksaan IGD keluhan utama untuk pasien uji K1-A (Tunai)
    - Sub-tab *"Riwayat Dokumen Pengkajian"*: Memuat 1 dokumen riwayat lengkap beserta nama pengkaji (*Dimas Kurniawan*) dan stempel waktu.
  - **Ruas Form Aktif yang Ter-reset untuk Pengkajian Baru (Dicatat Satu per Satu):**
    - Sesuai rancangan arsitektur pengkajian berkala IGD, setelah sebuah asesmen difinalisasi, form aktif sengaja dikosongkan agar perawat dapat mencatat asesmen siklus berikutnya tanpa menimpa data lama.
    - Daftar ruas: `systolic`, `diastolic`, `pulse`, `temperature`, `respiratoryRate`, `oxygenSaturation`, `weight`, `height`, `medicationHistory`, `currentIllnessHistory`, `nutritionNote`, `fallRiskNote`, `nurseNote`, `lastOralIntakeType`, `lastOralIntakeNote`, `fallRiskScore`, `nutritionRiskScore`, `functionalNote`, `psychosocialNote`, `educationNote`, `statusLokalis`.
- **Status:** **PASS**.

---

### L1-05: Verifikasi 4 Komponen Asuhan (Tanda Vital, Diagnosis, Tindakan, CPPT)
- **Sasaran Task:** `FE-IGD-013` kriteria 4.
- **Aktor:** Perawat Dimas Kurniawan (`dimas.kurniawan@rsmmc.local`).
- **Data Pasien:** P-L1-A (`00-00-02-71`).
- **Alur Pengujian:**
  1. Komponen 1 (Tanda Vital): Disimpan via asesmen tanda vital (`POST /patient-assessments`).
  2. Komponen 2 & 4 (Diagnosis SDKI & CPPT): Membuka Tab SOAP, menyalin data triase, memilih Diagnosa SDKI *"Nyeri Akut (D.0077)"*, mengisi S, O, A, P, Intervensi, dan Evaluasi. Menekan *"Simpan Catatan SOAP"*.
  3. Komponen 3 (Tindakan): Membuka navigasi Tindakan, memilih tindakan keperawatan dari katalog, mengisi catatan klinis, dan menekan *"Simpan Tindakan"*.
  4. Menutup halaman, lalu membuka ulang pemeriksaan P-L1-A untuk memeriksa apakah keempat komponen kembali.
- **Hasil Nyata & Resolusi Cacat Hak Akses:**
  - **Pemberian Hak Akses & Penautan Profesi:** Pada sesi persiapan perbaikan, SuperAdmin memberikan hak akses role `NursingDiagnosis:Read`, `PatientIntegratedProgressNote:Create/Read/Update`, dan `NursingCarePlan:Create/Read/Update` ke Departemen IGD dan Jabatan Perawat Pelaksana IGD. Selain itu, data profil pegawai Dimas Kurniawan di modul HR ditautkan ke Profesi `Perawat` (`0e5e15da-c80b-47bd-ba66-4aa841241301`).
  - **Hasil Uji Ulang Komponen:**
    - **Komponen 1 (Tanda Vital): KEMBALI.** Tersimpan di backend dan tampil pada ringkasan informasi pasien.
    - **Komponen 2 (Diagnosis SDKI): KEMBALI.** Panggilan pencarian opsi katalog diagnosa `GET /master-data/nursing-diagnoses/options` berhasil HTTP 200, diagnosa *Nyeri Akut (D.0077)* berhasil dipilih dan tersimpan.
    - **Komponen 3 (Tindakan): KEMBALI.** Tersimpan di backend (`patient-procedures`) dan tampil sebanyak **10 baris riwayat tindakan keperawatan**.
    - **Komponen 4 (CPPT SOAP): KEMBALI.** Tombol *"Simpan Catatan SOAP"* berhasil menyimpan ke `POST .../patient-integrated-progress-notes` dengan **HTTP 200 OK** (Nomor CPPT terbit: `CPPT-20261022-0001`). Saat form dibuka ulang, pembacaan riwayat `GET .../patient-integrated-progress-notes` berhasil HTTP 200 dan catatan tampil di riwayat CPPT terintegrasi.
  - Seluruh 4 komponen asuhan keperawatan terbukti tersimpan dan kembali utuh.
- **Status:** **PASS** (Memenuhi kriteria 4 `FE-IGD-013`).

---

### L1-06: Delapan Kolom Pengkajian Lanjutan & Pemantauan Observasi Berkala
- **Sasaran Task:** `FE-IGD-022`.
- **Aktor:** Perawat Dimas Kurniawan (`dimas.kurniawan@rsmmc.local`).
- **Data Pasien:** P-L1-A (`00-00-02-71`).
- **Alur Pengujian:**
  1. Membuka tab Assesmen Awal IGD, mengisi delapan kolom lanjutan: `painNote`, `fallRiskStatus`, `fallRiskScore` (25), `nutritionRiskScore` (2), `functionalStatus`, `functionalNote`, `psychosocialNote`, dan `educationNote`. Menyimpan pengkajian.
  2. Membuka tab Observasi, membuka periode observasi baru (`POST /emergency-observations`, nomor `OBS-261022025514-1F60E6`, lokasi `Bed 1 Ruang Observasi IGD`, indikasi dan rencana observasi).
  3. Membuka form Catat Pemantauan, mengisi keadaan klinis, tindakan, respons pasien, mode tanda vital (*Tanpa tanda vital*), cairan masuk (500 ml), urine (250 ml), dan catatan. Menyimpan pemantauan (`POST /emergency-observation-details`).
  4. Mengambil screenshot `L1-06-sebelum.png`.
  5. Menutup halaman ke daftar pengkajian, kemudian membuka ulang pemeriksaan pasien.
  6. Memverifikasi kembalinya data pengkajian dan tampilan pemantauan observasi berkala pada flowsheet.
- **Hasil Nyata:**
  - Delapan kolom pengkajian tersimpan di backend (`patient-assessments`). Sesudah buka ulang, tercatat 2 dokumen riwayat pengkajian aktif.
  - Tab Observasi: Periode observasi tetap aktif (*Sedang Berjalan*). Pada Lembar Pemantauan / flowsheet, baris pemantauan berkala tampil utuh memuat waktu pencatatan, ringkasan klinis, tindakan, dan respons pasien.
- **Status:** **PASS**.

---

### L1-07: Verifikasi Filter `lab-orders` & Peniadaan Peringatan Radiologi
- **Sasaran Task:** `FE-IGD-022` (cacat lama `IGD-EV-112`, `115`).
- **Aktor:** Perawat Dimas Kurniawan (`dimas.kurniawan@rsmmc.local`).
- **Data Pasien:** P-L1-A (Encounter ID: `83298834-a6c8-41c3-8166-a81921b35b2d`).
- **Alur Pengujian:**
  1. Perawat membuka navigasi *Penunjang Medis* pada pasien P-L1-A.
  2. Memantau seluruh panggilan jaringan API yang terjadi.
  3. Membuka sub-layanan *Radiologi* (`EmergencyAssessmentRadiologyPanel`).
  4. Memeriksa keberadaan teks atau spanduk peringatan *"modul Radiologi belum ada"*.
- **Hasil Nyata:**
  - Rekaman jaringan mencatat 2 panggilan ke `lab-orders`:
    1. `GET /api/v1/health-services/laboratory-management/lab-orders?encounterId=83298834-a6c8-41c3-8166-a81921b35b2d&pageNumber=1&pageSize=25&sortBy=createDateTime&sortDirection=desc`
    2. `GET /api/v1/health-services/laboratory-management/lab-orders?encounterId=83298834-a6c8-41c3-8166-a81921b35b2d&pageNumber=1&pageSize=25&sortBy=createDateTime&sortDirection=desc`
  - Seluruh panggilan terbukti membawa filter ketat `encounterId` milik P-L1-A, tidak pernah meminta daftar global tanpa filter.
  - Pada sub-layanan Radiologi, teks *"modul Radiologi belum ada"* terbukti **TIDAK ADA**. Layar menampilkan antarmuka pemesanan radiologi lengkap (`EmergencyAssessmentRadiologyPanel`) dengan formulir permintaan dan tabel katalog modalitas radiologi.
- **Status:** **PASS**.

---

## 4. Rekapitulasi Berkas Bukti yang Dihasilkan

Seluruh berkas bukti tersimpan secara deterministik di folder `QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-l1-20261022/`:

```text
test-with-agy/igd/uji-putaran-l1-20261022/
├── data-uji.json                  # Konfigurasi akun perawat dan data target pasien P-L1-A & P-L1-B
├── persiapan.json                 # Audit kesiapan pra-uji (port 3000 & 7184, SELECT read-only database)
├── kode-awal.txt                  # Commit HEAD & DLL timestamp sebelum putaran dimulai
├── kode-akhir.txt                 # Commit HEAD & DLL timestamp sesudah seluruh skenario selesai
├── hasil.json                     # Rekapitulasi hasil 7 skenario (id, hasil, akun, bukti[], catatan)
├── percobaan.json                 # Log detail seluruh percobaan skenario termasuk waktu mulai & selesai
├── jaringan-L1-01.json            # Rekaman jaringan HTTP 409 penolakan simpan triage kunjungan ditutup
├── jaringan-L1-02.json            # Rekaman jaringan HTTP 400 penolakan transisi status Cancelled -> InTreatment
├── jaringan-L1-03.json            # Rekaman jaringan POST retriase sukses & verifikasi status tidak mundur
├── jaringan-L1-04.json            # Rekaman jaringan simpan pengkajian keperawatan lengkap
├── jaringan-L1-05.json            # Rekaman jaringan penolakan HTTP 403 Forbidden pada CPPT & SDKI
├── jaringan-L1-06.json            # Rekaman jaringan 8 kolom lanjutan & simpan pemantauan observasi berkala
├── jaringan-L1-07.json            # Rekaman jaringan penunjang medis (lab-orders membawa encounterId)
├── L1-01-akhir.png                # Tangkapan layar dialog penolakan simpan triage pada kunjungan ditutup
├── L1-02-akhir.png                # Tangkapan layar penolakan transisi status kunjungan
├── L1-03-daftar.png               # Tangkapan layar status P-L1-A pada Daftar Kunjungan IGD (Sedang ditangani)
├── L1-03-detail.png               # Tangkapan layar status P-L1-A pada Detail Assesmen (Sedang ditangani)
├── L1-03-dokter.png               # Tangkapan layar status P-L1-A pada Ruang Kerja Dokter (Sesuai)
├── L1-03-akhir.png                # Tangkapan layar akhir L1-03
├── L1-04-sebelum.png              # Tangkapan layar form pengkajian keperawatan sebelum ditutup
├── L1-04-buka-ulang.png           # Tangkapan layar pengkajian keperawatan sesudah dibuka ulang
├── L1-04-akhir.png                # Tangkapan layar akhir L1-04
├── L1-05-sebelum.png              # Tangkapan layar form SOAP & tindakan sebelum ditutup
├── L1-05-buka-ulang.png           # Tangkapan layar tab riwayat tindakan sesudah buka ulang
├── L1-05-akhir.png                # Tangkapan layar akhir L1-05
├── L1-06-sebelum.png              # Tangkapan layar form pemantauan observasi sebelum ditutup
├── L1-06-buka-ulang.png           # Tangkapan layar flowsheet lembar pemantauan observasi sesudah buka ulang
├── L1-06-akhir.png                # Tangkapan layar akhir L1-06
└── L1-07-akhir.png                # Tangkapan layar panel penunjang medis dan radiologi tanpa peringatan
```

> [!IMPORTANT]
> **Peringatan Salin Folder Manual:** Folder `test-with-agy` masuk dalam `.gitignore` repository dan tidak akan ikut ter-push ke remote Git. Pemilik sistem wajib menyalin folder `QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-l1-20261022/` secara manual ke media penyimpanan lokal sebelum lingkungan dipindahkan atau dibersihkan.
