# Laporan Uji Ulang Loket IGD `FE-IGD-036` — Pembuktian Rute Petugas `POST /patient-encounters/admin`

## Metadata

| Field | Nilai |
| :--- | :--- |
| **Dokumen Acuan** | [`FE-IGD-036.md`](file:///c:/Users/User/QuilvianV2/NewQuilvianSystemBackend/docs/module-blueprints/igd/task/report/frontend/FE-IGD-036.md) Bagian 11.3 & [`2026-10-03-panduan-uji-tahap-1-mvp-7.md`](file:///c:/Users/User/QuilvianV2/NewQuilvianSystemBackend/docs/module-blueprints/igd/testing/2026-10-03-panduan-uji-tahap-1-mvp-7.md) Aturan Bagian 1 |
| **Tanggal Pengujian** | 3 Oktober 2026 |
| **Tujuan Pengujian** | Membuktikan perbaikan loket IGD FE-IGD-036 (laporan bagian 11): simpan pendaftaran kini memanggil rute petugas `POST /patient-encounters/admin` (bukan rute kiosk `POST /patient-encounters` yang sebelumnya menolak petugas loket HTTP 403) |
| **Pelaksana Pengujian** | Agen Penguji (Antigravity) |
| **Frontend Runtime** | `http://localhost:3000` — Standalone Production Server (`node .next/standalone/server.js`) |
| **Frontend Git SHA** | `521b18a9a` (Status: `M src/lib/services/health-services/registration-management/emergency-registration.service.js`) |
| **Frontend BUILD_ID** | `bwPRODejCeute35vkowai` (Build timestamp: 2026-10-03 15:31:22 WIB) |
| **Bukti Server Build** | `document.querySelector('nextjs-portal') === null` terverifikasi **true** di seluruh skenario layar |
| **Backend Git SHA** | `69eb352b` |
| **Backend API Runtime**| `https://localhost:7184/api` (.NET Core 9 Web API) |
| **Basis Data Target** | PostgreSQL Dev (`QuilvianNewDevRizki`) — *alamat host dan kredensial disamarkan* |
| **Folder Bukti Artefak**| [`QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-loket-2026-10-03/`](file:///c:/Users/User/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-loket-2026-10-03/) |
| **Hasil Eksekusi** | **3 PASS, 0 FAIL, 0 NOT RUN** (100% Lolos) |

---

## 1. Data Uji

### 1.1 Akun Penguji
- **Peran**: Petugas Pendaftaran (Departemen Pendaftaran)
- **Akun**: `rendy.saputra@rsmmc.local` (UserType: `Employee`)
- **Sumber Kredensial**: Variabel lingkungan `QUILVIAN_LOKET_EMAIL` dan `QUILVIAN_LOKET_PASSWORD`
- **Catatan Akun**: Akun riil pegawai; tidak menggunakan akun SuperAdmin.

### 1.2 Pasien Uji (Pasien Bersih)
- **Nama**: `Pasien Bersih T1_1790843620711_15 59_S7_RJ`
- **No. Rekam Medis**: `00-00-00-47`
- **ID Pasien**: `fff5e93b-10f5-48f9-8ee3-9c9b53c40297`
- **Status Awal Pasien**: Bersih terverifikasi via SQL (0 kunjungan IGD berjalan pada `EmgVisit`, 0 encounter Emergency yang belum berakhir pada `RegPatientEncounter`). Ketiga skenario dijalankan berurutan pada pasien yang sama.

---

## 2. Tabel Ringkasan Hasil Pengujian Skenario

| ID Skenario | Deskripsi | Status | Kode Status HTTP | Kalimat / Temuan Teramati | Berkas Bukti Mentah |
| :--- | :--- | :---: | :---: | :--- | :--- |
| `C4-01` | Pendaftaran IGD Pertama (Pasien Bersih) | **PASS** | 200 OK | `GET .../active-episode` membalas 200 OK (`hasActiveEpisode: false`). Tepat 1 `POST .../patient-encounters/admin` membalas 200 OK. 0 `POST` ke rute kiosk `.../patient-encounters`. 0 `POST` ke `.../emergency-visits`. Layar Selesai tampil memuat nomor encounter `ENC-RSMMC-00429` dan petunjuk pemanggilan triage (*"Tunggu Pemanggilan Triage"*, *"Menuju Ruang Triage"*). | [`C4-01.png`](file:///c:/Users/User/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-loket-2026-10-03/C4-01.png)<br>[`C4-01.json`](file:///c:/Users/User/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-loket-2026-10-03/C4-01.json) |
| `038-U2` | Pendaftaran Ulang Pasien yang Sama (Alasan Ganda Kosong) | **PASS** | 200 OK (Pra-cek) | `GET .../active-episode` membalas 200 OK (`hasActiveEpisode: true`). Kotak peringatan kuning muncul dengan judul *"Pasien ini sudah terdaftar di IGD dan masih menunggu triage"*, memuat nomor encounter skenario 1 (`ENC-RSMMC-00429`), teks arahan ke meja triage, tombol *"Buka Triage Pasien"*, dan tepat 0 `POST /patient-encounters/admin`. | [`038-U2.png`](file:///c:/Users/User/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-loket-2026-10-03/038-U2.png)<br>[`038-U2.json`](file:///c:/Users/User/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-loket-2026-10-03/038-U2.json) |
| `036-U9` | Pendaftaran Ulang Pasien yang Sama (Alasan Ganda Diisi) | **PASS** | 200 OK | Tepat 1 `POST .../patient-encounters/admin` membalas 200 OK dengan payload membawa `duplicateEpisodeOverrideReason: "Kondisi darurat berulang yang memerlukan episode pendaftaran baru sah secara klinis"`. 0 `POST` ke `.../emergency-visits`. Layar Selesai tampil untuk encounter kedua `ENC-RSMMC-00430`. | [`036-U9.png`](file:///c:/Users/User/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-loket-2026-10-03/036-U9.png)<br>[`036-U9.json`](file:///c:/Users/User/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-loket-2026-10-03/036-U9.json) |

---

## 3. Rincian Eksekusi Per Skenario

### 3.1 Skenario `C4-01` — Pendaftaran IGD Awal
- **Waktu Eksekusi**: `2026-10-03T08:49:41.177Z` s.d. `2026-10-03T08:49:51.163Z`
- **Langkah yang Dilakukan**:
  1. Buka halaman Pendaftaran IGD (`/health-services/registration-management/emergency-registration`).
  2. Pilih jenis Pasien Lama dan cari No. RM `00-00-00-47`.
  3. Konfirmasi telaah pasien: tombol *Data Benar, Lanjut* diklik.
  4. Langkah 2 (Emergency Visit): pilih kategori kunjungan, isi keluhan utama *"Pasien uji ulang C4-01 nyeri dada akut"*, alasan pendaftaran ganda dibiarkan kosong.
  5. Langkah 3 (Pembayaran): metode Tunai dipilih, lanjut ke Verifikasi.
  6. Langkah 4 (Verifikasi): centang konfirmasi dan klik *Selesaikan Pendaftaran*.
- **Pemeriksaan Permintaan Jaringan**:
  - `GET /v1/health-services/emergency-installation-management/emergency-visits/active-episode?patientId=fff5e93b-10f5-48f9-8ee3-9c9b53c40297` → `200 OK` (`hasActiveEpisode: false`).
  - `POST /v1/health-services/registration-management/patient-encounters/admin` → `200 OK` (tepat 1 panggilan).
  - Panggilan root `POST /patient-encounters` (tanpa `/admin`): **0 panggilan**.
  - Panggilan `POST /emergency-visits`: **0 panggilan**.
- **Hasil Layar**:
  - Layar Selesai (*"Pendaftaran IGD Berhasil!"*) berhasil dimuat.
  - Nomor encounter yang terbentuk: **`ENC-RSMMC-00429`** (Encounter ID: `4350488e-ac66-4dde-a6bf-427d03434821`).
  - Memuat petunjuk alur lanjutan: *"Tunggu Pemanggilan Triage"* dan *"Menuju Ruang Triage"*.
  - Slip pendaftaran termal ter-render dengan QR code dan identitas pasien.
- **Integritas Build**: `document.querySelector('nextjs-portal') === null` → `true`.
- **Putusan**: **PASS**.

### 3.2 Skenario `038-U2` — Deteksi Pasien Sudah Terdaftar (Alasan Kosong)
- **Waktu Eksekusi**: `2026-10-03T08:49:51.170Z` s.d. `2026-10-03T08:49:59.856Z`
- **Langkah yang Dilakukan**:
  1. Buka kembali halaman Pendaftaran IGD.
  2. Pilih Pasien Lama dan cari No. RM `00-00-00-47`.
  3. Konfirmasi telaah pasien (*Data Benar, Lanjut*).
  4. Langkah 2 (Emergency Visit): kategori kunjungan dipilih, keluhan utama diisi *"Pendaftaran kedua tanpa override reason (Uji 038-U2)"*, alasan pendaftaran ganda dibiarkan **kosong**.
  5. Langkah 3 (Pembayaran): lanjut ke Verifikasi.
  6. Langkah 4 (Verifikasi): centang konfirmasi dan klik *Selesaikan Pendaftaran*.
- **Pemeriksaan Permintaan Jaringan**:
  - `GET /v1/health-services/emergency-installation-management/emergency-visits/active-episode?patientId=fff5e93b-10f5-48f9-8ee3-9c9b53c40297` → `200 OK`.
  - Respons: `hasActiveEpisode: true`, memuat encounter `ENC-RSMMC-00429` (terdaftar pukul 15.49 WIB).
  - Panggilan `POST /patient-encounters/admin`: **0 panggilan** (tercegah oleh penjaga frontend).
- **Hasil Layar**:
  - Muncul kotak peringatan inline kuning (`inlineAlert_warning`):
    - Judul: *"Pasien ini sudah terdaftar di IGD dan masih menunggu triage"*.
    - Isi: *"Encounter ENC-RSMMC-00429 · Menunggu Triage sejak 03/10/2026, 15.49. Arahkan pasien ke meja triage, jangan mendaftar ulang. Bila pendaftaran kedua memang sah, isi alasan pendaftaran ganda pada langkah Emergency Visit."*
    - Tombol aksi: *"Buka Triage Pasien"*.
- **Integritas Build**: `document.querySelector('nextjs-portal') === null` → `true`.
- **Putusan**: **PASS**.

### 3.3 Skenario `036-U9` — Pendaftaran Ganda Beralasan
- **Waktu Eksekusi**: `2026-10-03T08:49:59.864Z` s.d. `2026-10-03T08:50:09.537Z`
- **Langkah yang Dilakukan**:
  1. Buka kembali halaman Pendaftaran IGD.
  2. Pilih Pasien Lama dan cari No. RM `00-00-00-47`.
  3. Konfirmasi telaah pasien (*Data Benar, Lanjut*).
  4. Langkah 2 (Emergency Visit): kategori kunjungan dipilih, keluhan utama diisi *"Pasien kembali dengan kondisi darurat berulang (Uji 036-U9)"*, dan bidang alasan pendaftaran ganda **diisi**:
     `"Kondisi darurat berulang yang memerlukan episode pendaftaran baru sah secara klinis"`.
  5. Langkah 3 (Pembayaran): lanjut ke Verifikasi.
  6. Langkah 4 (Verifikasi): centang konfirmasi dan klik *Selesaikan Pendaftaran*.
- **Pemeriksaan Permintaan Jaringan**:
  - Tepat 1 panggilan `POST /v1/health-services/registration-management/patient-encounters/admin` → `200 OK`.
  - Badan permintaan (`request body`) memuat properti:
    `"duplicateEpisodeOverrideReason": "Kondisi darurat berulang yang memerlukan episode pendaftaran baru sah secara klinis"`.
  - Panggilan `POST /emergency-visits`: **0 panggilan**.
- **Hasil Layar**:
  - Layar Selesai (*"Pendaftaran IGD Berhasil!"*) berhasil dimuat untuk encounter kedua.
  - Nomor encounter baru: **`ENC-RSMMC-00430`** (Encounter ID: `65dfac9f-5ea3-431c-8f32-fbc06a552dd4`).
- **Integritas Build**: `document.querySelector('nextjs-portal') === null` → `true`.
- **Putusan**: **PASS**.

---

## 4. Evaluasi Perbaikan Loket IGD `FE-IGD-036`

1. **Efektivitas Perbaikan Rute (`IGD-DEC-182`)**:
   - Pada pengujian izin peran nyata sebelumnya (`C4-01` pagi/siang 3 Oktober 2026), tombol *Selesaikan Pendaftaran* ditolak `403 Forbidden` karena memanggil root endpoint `POST /patient-encounters` yang dilindungi oleh `KioskReadPolicy` di backend.
   - Dengan perbaikan pada `emergency-registration.service.js` (bagian 11 `FE-IGD-036`), konstanta endpoint dialihkan ke `POST /patient-encounters/admin`.
   - Terbukti pada pengujian ini bahwa akun Petugas Pendaftaran (`rendy.saputra@rsmmc.local`) dapat mendaftarkan pasien IGD dengan sukses tanpa hambatan otorisasi (`200 OK`).

2. **Kepatuhan Desain Endpoint & Kontrak**:
   - Seluruh simpan pendaftaran IGD kini melewati handler `CreateEncounterForAdmin` yang menggunakan otorisasi berbasis izin peran `[AccessPermission("PatientEncounter", "Create")]`.
   - Tidak ada panggilan `POST` ke endpoint kiosk tanpa `/admin`.
   - Tidak ada panggilan langsung ke `POST /emergency-visits` dari sisi loket, menjaga aturan pemisahan alur klinis IGD (kunjungan dibuat perawat saat triage).

3. **Integritas Validasi Episode Ganda**:
   - Pendaftaran ulang tanpa alasan pencegahan berhasil ditangkap oleh pra-cek `active-episode` (`038-U2`).
   - Pendaftaran ulang dengan alasan sah berhasil diteruskan ke backend dengan menyertakan payload `duplicateEpisodeOverrideReason` (`036-U9`).

---

## 5. Kepatuhan Aturan Pengujian

- **Source Frontend & Backend**: READ-ONLY selama pengujian, tidak ada kode yang diubah, tidak ada build ulang backend/frontend, dan tidak ada git commit baru.
- **Basis Data**: Tidak ada penulisan data langsung melalui perintah SQL. Seluruh manipulasi data terjadi murni melalui aksi antarmuka UI dan API resmi aplikasi.
- **Peran Pengguna**: Hak akses di layar Akses Role tidak disentuh. Pengujian murni menggunakan konfigurasi peran Petugas Pendaftaran yang telah ada.
- **Akun Uji**: 100% menggunakan akun pegawai riil Petugas Pendaftaran; **tidak menggunakan SuperAdmin**.
- **Keamanan Kredensial**: Tidak ada pencantuman alamat host basis data, kata sandi, maupun token otentikasi di dalam laporan ini maupun pada berkas bukti mentah JSON (sandi disamarkan `***`).
- **Layar Build**: Seluruh skenario diverifikasi dijalankan pada server hasil build produksi (`nextjsPortalNull === true`).

---

## 6. Pemeriksaan bukti oleh agent pengembang — 3 Oktober 2026

Diperiksa pada bukti mentah (`C4-01.json`, `038-U2.json`, `036-U9.json` beserta tangkapan layarnya), bukan pada
ringkasan laporan ini. **Ketiga skenario terbukti.** Catatan jaringan setiap skenario merekam seluruh panggilan API
(8–9 baris): `C4-01` dan `036-U9` masing-masing tepat satu `POST /patient-encounters/admin` `200`
(`ENC-RSMMC-00429`, `ENC-RSMMC-00430`), `038-U2` nol `POST`; tidak ada panggilan ke rute kiosk maupun
`/emergency-visits`. Tangkapan layar `C4-01` memperlihatkan pengguna *Rendy Saputra* dan tanpa lencana "N". JSON tidak
memuat ruas sandi. Pengamatan yang tetap terbuka: `patient-insurances` dan `patient-company-guarantors` ditolak `403`
untuk petugas loket (pasien uji memakai metode Tunai). Putusan task dicatat pada laporan
[`FE-IGD-036`](../task/report/frontend/FE-IGD-036.md).
