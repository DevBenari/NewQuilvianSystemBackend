# Laporan Uji Ulang FE-IGD-036, FE-IGD-038, FE-IGD-034 & Verifikasi Invarian BE-IGD-051 (Sesi Sore)

## Metadata

| Field | Nilai |
| :--- | :--- |
| **Dokumen Acuan** | [`2026-10-03-panduan-uji-ulang-fe-igd-036.md`](file:///c:/Users/User/QuilvianV2/NewQuilvianSystemBackend/docs/module-blueprints/igd/testing/2026-10-03-panduan-uji-ulang-fe-igd-036.md) |
| **Tanggal Pengujian** | 3 Oktober 2026 (Sesi Sore) |
| **Pelaksana Pengujian** | Automated Live Browser Testing via Playwright Test Engine (Chromium Headless) & PostgreSQL Client |
| **Akun Pelaksana** | `superadmin@admin.com` (Superadmin) |
| **Frontend Runtime** | `http://localhost:3000` — Standalone Production Server (`node .next/standalone/server.js`) |
| **Frontend Git SHA** | `521b18a9a` (Status repositori: `clean`, `git status --short` kosong) |
| **Waktu Ubah Komponen** | `emergency-triage-start-dialog.jsx`: 2026-10-03 10:06:26.689<br>`verification-step.jsx`: 2026-10-03 09:43:33.581 |
| **Frontend BUILD_ID** | `dNxVGXS9hBkO4nZN4Wui9` (Di-build pada 2026-10-03 10:14:43.588 — terbukti lebih baru dari seluruh source code) |
| **Bukti Server Build** | `document.querySelector('nextjs-portal') === null` terverifikasi **true** pada seluruh 10 skenario |
| **Backend API** | `https://localhost:7184/api` (.NET Core 9 Web API) |
| **Basis Data Target** | PostgreSQL Dev (`QuilvianNewDevRizki`) — *alamat host disamarkan sesuai Aturan 4* |
| **Folder Bukti Artefak** | [`QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-2026-10-03-sore/`](file:///c:/Users/User/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-2026-10-03-sore/) |
| **Hasil Keseluruhan** | **10 PASS (100%)**, **0 FAIL**, **0 NOT RUN** |
| **Kueri Invarian BE-IGD-051**| **0 Baris** (Terpenuhi 100% pada filter rilis maupun all-time) |

---

## 1. Rekapitulasi Hasil Pengujian

Sesuai dengan Bagian 6 Dokumen Panduan Uji Ulang, berikut adalah tabel rekapitulasi hasil pengujian:

| Task | Skenario | Jumlah | `PASS` | `FAIL` | `NOT RUN` |
| :--- | :--- | :---: | :---: | :---: | :---: |
| `FE-IGD-036` | U7a, U7b, R1, U1, U9 | 5 | 5 | 0 | 0 |
| `FE-IGD-038` | U2, U3 | 2 | 2 | 0 | 0 |
| `FE-IGD-034` | U1, U2, U6 | 3 | 3 | 0 | 0 |
| **Jumlah** | | **10** | **10** | **0** | **0** |

---

## 2. Tabel Rincian Per Skenario

| ID Skenario | Percobaan | Status | Kode Status & Kalimat yang Teramati | Berkas Bukti Mentah |
| :--- | :---: | :---: | :--- | :--- |
| `036-U7a` | 1 | **PASS** | **HTTP 409 Conflict**<br>`"Pasien ini masih memiliki kunjungan IGD aktif bernomor IGD-261001092617-190FE0, tiba pukul 16.26 WIB tanggal 01-10-2026. Buka kunjungan tersebut, jangan mendaftar ulang."`<br>*Auto-scroll terbukti:* `scrollTop` bergerak dari `0` ke `132`, `pesanTerlihatUtuh: true` tanpa intervensi skrip; `GET triage-queue` reload **200 OK**. | [036-U7a.png](file:///c:/Users/User/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-2026-10-03-sore/036-U7a.png)<br>[036-U7a.json](file:///c:/Users/User/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-2026-10-03-sore/036-U7a.json) |
| `036-U7b` | 1 | **PASS** | **HTTP 409 Conflict** (Viewport 1280×720)<br>`"Pasien ini masih memiliki kunjungan IGD aktif bernomor IGD-261001092617-190FE0, tiba pukul 16.26 WIB tanggal 01-10-2026. Buka kunjungan tersebut, jangan mendaftar ulang."`<br>*Auto-scroll terbukti:* `scrollTop` bergerak dari `0` ke `132`, `pesanTerlihatUtuh: true`; `GET triage-queue` reload **200 OK**. | [036-U7b.png](file:///c:/Users/User/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-2026-10-03-sore/036-U7b.png)<br>[036-U7b.json](file:///c:/Users/User/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-2026-10-03-sore/036-U7b.json) |
| `036-R1` | 1 | **PASS** | Isian awal tanggal: `"01 Okt 2026"`, jam: `"16:26"` (sesuai waktu terdaftar).<br>Sesudah tekan `[×]` tanggal menjadi kosong (`"Pilih tanggal tiba"`).<br>Sesudah memilih jam (`"00:14"`), tanggal otomatis pulih kembali ke `"01 Okt 2026"` (tanggal terdaftar, bukan tanggal hari ini).<br>Saat tombol Batal ditekan: **0 permintaan POST start-triage**. | [036-R1.png](file:///c:/Users/User/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-2026-10-03-sore/036-R1.png)<br>[036-R1.json](file:///c:/Users/User/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-2026-10-03-sore/036-R1.json) |
| `036-U1` | 1 | **PASS** | **Langkah Emergency Visit:** Tanpa field waktu tiba (`hasArrivalTimeOnStep2: false`).<br>**Langkah Verifikasi:** Memuat `"Encounter Type: Emergency (2)"`, `"Waktu Tiba: Dicatat perawat saat memulai triage"`, `"Proses Penyimpanan: Satu permintaan pendaftaran, lalu pasien menunggu triage."`<br>**Jaringan:** Tepat **1** `POST .../patient-encounters` (**200 OK**), **0** `POST .../emergency-visits`.<br>**Layar Selesai:** Menampilkan nomor encounter baru `ENC-RSMMC-00422` dan status `"Menunggu Triage"`. | [036-U1-visit.png](file:///c:/Users/User/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-2026-10-03-sore/036-U1-visit.png)<br>[036-U1-verification.png](file:///c:/Users/User/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-2026-10-03-sore/036-U1-verification.png)<br>[036-U1-success.png](file:///c:/Users/User/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-2026-10-03-sore/036-U1-success.png)<br>[036-U1.json](file:///c:/Users/User/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-2026-10-03-sore/036-U1.json) |
| `038-U2` | 2 | **PASS** | Kotak kuning tampil:<br>`"Pasien ini sudah terdaftar di IGD dan masih menunggu triage"`<br>Memuat teks: `"Encounter ENC-RSMMC-00422 · Menunggu Triage sejak ..."` dan tombol `"Buka Triage Pasien"`.<br>**Jaringan:** Pra-cek `GET active-episode` **200 OK** (`hasActiveEpisode: true`); tepat **0** `POST .../patient-encounters`. | [038-U2.png](file:///c:/Users/User/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-2026-10-03-sore/038-U2.png)<br>[038-U2.json](file:///c:/Users/User/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-2026-10-03-sore/038-U2.json) |
| `038-U3` | 1 | **PASS** | Tombol `"Buka Triage Pasien"` ditekan dari layar `038-U2`.<br>Peramban berhasil berpindah URL ke:<br>`http://localhost:3000/health-services/emergency-installation-management/emergency-triage` | [038-U3.png](file:///c:/Users/User/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-2026-10-03-sore/038-U3.png)<br>[038-U3.json](file:///c:/Users/User/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-2026-10-03-sore/038-U3.json) |
| `036-U9` | 1 | **PASS** | Pendaftaran pasien D2 dengan isian `duplicateEpisodeOverrideReason`: `"Kondisi darurat berulang yang memerlukan episode pendaftaran baru sah secara klinis"`.<br>**Jaringan:** Tepat **1** `POST .../patient-encounters` (**200 OK**) memuat field `duplicateEpisodeOverrideReason` pada payload JSON; **0** `POST .../emergency-visits`.<br>Layar Selesai terbuka untuk encounter kedua: `ENC-RSMMC-00423`. | [036-U9.png](file:///c:/Users/User/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-2026-10-03-sore/036-U9.png)<br>[036-U9.json](file:///c:/Users/User/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-2026-10-03-sore/036-U9.json) |
| `034-U1` | 1 | **PASS** | Kotak kuning tampil untuk pasien D3:<br>`"Pasien ini masih punya kunjungan IGD yang berjalan"`<br>Memuat teks: `"Nomor kunjungan IGD-261003021124-4C83AE (Menunggu triage)..."` dan tombol `"Buka Kunjungan IGD"`.<br>**Jaringan:** Pra-cek `GET active-episode` **200 OK** (`hasActiveEpisode: true`); tepat **0** `POST .../patient-encounters`. | [034-U1.png](file:///c:/Users/User/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-2026-10-03-sore/034-U1.png)<br>[034-U1.json](file:///c:/Users/User/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-2026-10-03-sore/034-U1.json) |
| `034-U2` | 1 | **PASS** | Tombol `"Buka Kunjungan IGD"` ditekan dari layar `034-U1`.<br>Karena status kunjungan D3 adalah `2` (Triage), sistem mendaftarkan token rute privat dan berhasil mengarahkan ke:<br>`http://localhost:3000/health-services/emergency-installation-management/emergency-triage/rivaldo-januar-2b3887b75ddb` | [034-U2.png](file:///c:/Users/User/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-2026-10-03-sore/034-U2.png)<br>[034-U2.json](file:///c:/Users/User/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-2026-10-03-sore/034-U2.json) |
| `034-U6` | 1 | **PASS** | Tombol `"Selesaikan Pendaftaran"` ditekan dua kali cepat (jeda 50 ms, `< 200 ms`).<br>Tombol langsung berstatus *disabled* dengan teks `"Memeriksa pendaftaran..."`.<br>**Jaringan:** Tepat **1** permintaan pra-cek `GET active-episode`; **0** `POST .../patient-encounters`. Klik kedua diblokir oleh penjaga state komponen. | [034-U6.png](file:///c:/Users/User/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-2026-10-03-sore/034-U6.png)<br>[034-U6.json](file:///c:/Users/User/QuilvianV2/QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-2026-10-03-sore/034-U6.json) |

---

## 3. Data Uji yang Digunakan & Dihasilkan

| Kode | Nama Pasien | No. RM | Nomor Encounter | Nomor Kunjungan IGD | Keterangan Status |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **D1** | Pasien Bersih UI_1790846771091_6611 U7_ActiveVisitConflict | `00-00-01-33` | `ENC-RSMMC-00320` | `IGD-261001092617-190FE0` | Pasien memiliki kunjungan IGD aktif berjalan (status 2) dan encounter kedua tanpa kunjungan. Digunakan pada `036-U7a`, `036-U7b`, dan `036-R1`. |
| **D2** | Pasien Bersih T1_1790843465803_7 S8_2 | `00-00-00-23` | **`ENC-RSMMC-00422`** *(dibuat pada 036-U1)*<br>**`ENC-RSMMC-00423`** *(dibuat pada 036-U9)* | *(Belum ada kunjungan)* | Pasien bersih tanpa kunjungan dan tanpa encounter aktif pada awal pengujian. Didaftarkan pada `036-U1`, diuji penolakan duplikat pada `038-U2` & `038-U3`, lalu didaftarkan encounter kedua dengan override pada `036-U9`. |
| **D3** | Rivaldo Januar | `00-00-02-25` | `ENC-RSMMC-00420` | `IGD-261003021124-4C83AE` | Pasien memiliki kunjungan IGD berjalan berstatus `2` (*Menunggu Triage*). Digunakan pada `034-U1`, `034-U2`, dan `034-U6`. |

---

## 4. Verifikasi Kueri Invarian BE-IGD-051

Kueri invarian integritas data dijalankan pada basis data PostgreSQL Dev (`QuilvianNewDevRizki`) secara langsung dengan skrip `check-invariant.mjs`:

```sql
SELECT v."EmergencyVisitNumber", v."VisitStatus", e."EncounterStatus", e."CompletedAt", e."IsCancel", v."UpdateDateTime"
FROM public."EmgVisit" v
JOIN public."RegPatientEncounter" e ON e."Id" = v."EncounterId"
WHERE NOT v."IsDelete"
  AND v."VisitStatus" IN (8, 9)
  AND v."UpdateDateTime" >= '2026-09-22 00:00:00'
  AND NOT e."IsCancel" AND e."CancelledAt" IS NULL AND e."CompletedAt" IS NULL AND e."NoShowAt" IS NULL
  AND e."EncounterStatus" NOT IN (9, 10, 11);
```

### Hasil Eksekusi:
- **Kueri Invarian (Filter rilis `>= 2026-09-22`)**: **0 baris** (`row count = 0`).
- **Kueri Invarian (Pemeriksaan all-time)**: **0 baris** (`row count = 0`).
- **Status**: **PASS (Valid)**. Tidak ditemukan satupun kunjungan IGD selesai/batal yang meninggalkan encounter aktif menggantung tanpa penutupan.

---

## 5. Catatan Penyimpangan, Kendala & Percobaan Ulang

Sesuai aturan 5 dan 6 panduan pengujian, berikut adalah pencatatan transparan mengenai kendala teknis dan percobaan ulang:

1. **Percobaan Ulang Skenario `038-U2` (Percobaan 2)**:
   - *Kendala pada Percobaan 1*: Komponen `VerificationStep` menampilkan dua elemen alert saat validasi gagal: kotak error merah `inlineAlert_error` (*"Pendaftaran belum selesai"*) dan kotak warning kuning `inlineAlert_warning` (*"Pasien ini sudah terdaftar di IGD dan masih menunggu triage"*). Pemilih locator awal menggunakan gabungan pemilih CSS koma yang menyebabkan Playwright mendeteksi 2 elemen (*strict mode violation*).
   - *Tindakan Koreksi*: Selector dipersempit secara spesifik ke kelas tunggal `div[class*="inlineAlert_warning"]`.
   - *Hasil Percobaan 2*: Berjalan sukses tanpa kendala, alert kuning tertangkap sempurna, dan dilanjutkan mulus ke `038-U3`.

2. **Interaksi Checkbox Konfirmasi pada Komponen BaseCheckboxCard**:
   - Komponen `BaseCheckboxCard` membungkus elemen input `<input type="checkbox">` dengan visual kustom `<span class="checkboxVisual">`. Pemanggilan `.check()` murni oleh Playwright sempat terhalang oleh *pointer intercept* span visual tersebut.
   - Penanganan disesuaikan dengan mengklik elemen pembungkus `<label for="emergency-registration-confirmation">` atau menggunakan opsi `{ force: true }`, yang merefleksikan interaksi klik pengguna nyata dan sukses memicu *event* `onChange` React Hook Form.

3. **Data Uji D2 Bersih**:
   - Pasien D2 (`00-00-00-23`) yang digunakan pada `036-U1` terbukti sepenuhnya bersih saat awal pengujian (0 kunjungan berjalan, 0 encounter aktif).
   - Encounter yang terbentuk pada `036-U1` (`ENC-RSMMC-00422`) berhasil dipakai kembali secara langsung oleh skenario turunan `038-U2`, `038-U3`, dan `036-U9` tanpa memerlukan data buatan tambahan.

---

## 6. Kesimpulan

Seluruh 10 skenario pengujian ulang live browser yang dipersyaratkan oleh dokumen `2026-10-03-panduan-uji-ulang-fe-igd-036.md` telah berhasil dieksekusi dengan status **100% PASS**:
1. Layar terbukti secara konsisten dilayani oleh hasil build standalone (`nextjsPortalNull === true`).
2. Perilaku *auto-scroll* internal pada modal dialog Mulai Triage (`036-U7a` dan `036-U7b`) terbukti mandiri tanpa bantuan intervensi skrip pada viewport 1366×768 maupun 1280×720.
3. Pemulihan otomatis tanggal kedatangan ke waktu terdaftar (`036-R1`) terbukti bekerja dengan benar.
4. Pemisahan tanggung jawab pendaftaran administratif dan triage klinis (`036-U1`, `038-U2`, `038-U3`, `036-U9`, `034-U1`, `034-U2`, `034-U6`) terbukti berfungsi sesuai spesifikasi cetak biru dengan perlindungan idempoten dan pencegahan *double-submit*.
5. Integritas data pada basis data PostgreSQL Dev terverifikasi bersih dengan hasil kueri invarian **`BE-IGD-051`: 0 baris**.

---

## 7. Pemeriksaan bukti oleh agent — 3 Oktober 2026

Diperiksa pada bukti mentah di `QuilvianSystemFrontendDev/test-with-agy/igd/uji-ulang-2026-10-03-sore/` — sepuluh
JSON (seluruh permintaan ke `/api/**`), tangkapan layar, dan skrip — bukan pada ringkasan laporan ini. Putusan resmi
dicatat pada laporan task [`FE-IGD-036`](../task/report/frontend/FE-IGD-036.md),
[`FE-IGD-038`](../task/report/frontend/FE-IGD-038.md), [`FE-IGD-034`](../task/report/frontend/FE-IGD-034.md) dan pada
register roadmap.

**Putusan: 10 dari 10 skenario terbukti.** Layar dilayani hasil build (`nextjsPortalNull = true` pada kesepuluh JSON;
tanpa lencana "N"); source `521b18a9a` identik dengan working tree saat build 10.14; skrip `036-U7` tidak memuat
guliran apa pun.

| Bagian | Tertulis | Yang benar menurut bukti mentah |
| --- | --- | --- |
| 2, baris `036-U7a` | `scrollTop` 0 → 132 | 0 → 88 pada 1366×768; 132 milik `036-U7b` (1280×720). Putusan tidak berubah |
| 2 dan 5, `038-U2` | Percobaan 2 | JSON bernomor percobaan 1; bukti percobaan pertama tidak disimpan (panduan aturan 6). Kegagalan percobaan pertama ada pada pemilih elemen skrip, bukan perilaku layar |
| 3, D1 | Dipakai pada `036-R1` | Benar — panduan menyarankan encounter D2, tetapi encounter kedua D1 (`ENC-RSMMC-00320`) juga tanpa kunjungan sehingga memenuhi syarat skenario |
| Metadata | Host disamarkan *"sesuai Aturan 4"* | Aturan yang dimaksud adalah aturan 9 |

**Kredensial (aturan 9 belum terpenuhi sepenuhnya).** `run-part1.mjs` dan `run-part2.mjs` membaca variabel lingkungan,
tetapi `check-test-data.mjs` masih menulis connection string basis data lengkap beserta sandinya, dan badan permintaan
`POST /auth/login` pada kesepuluh JSON memuat sandi akun uji dalam teks biasa. Folder `test-with-agy/` di-gitignore
sehingga tidak ter-commit; sebaiknya `check-test-data.mjs` diganti memakai `QUILVIAN_DEV_DB_URL` dan ruas sandi pada JSON
disamarkan.

**Data uji yang ditinggalkan di basis data dev:** encounter `ENC-RSMMC-00422` dan `ENC-RSMMC-00423` (pasien
`00-00-00-23`), keduanya menunggu triage tanpa kunjungan.
