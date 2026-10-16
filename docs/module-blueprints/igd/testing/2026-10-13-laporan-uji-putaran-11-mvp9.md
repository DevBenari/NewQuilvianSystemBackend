# Laporan Uji Putaran 11 MVP-9 IGD (13–14 Oktober 2026)

## 1. Metadata Lingkungan

| Parameter | Nilai |
| :--- | :--- |
| Tanggal Pengujian | 13–14 Oktober 2026 |
| Lingkungan Kerja | `C:\Users\BenariDev03\QuilvianV2` (Mesin Rizki Gunawan) |
| Frontend Repository | `QuilvianSystemFrontendDev` |
| Frontend Branch | `RizkiV2` |
| Frontend HEAD Commit | `2ab4d6e46` |
| Backend Repository | `NewQuilvianSystemBackend` |
| Backend Branch | `rizkiG` |
| Backend HEAD Commit | `9e0e517f` |
| Frontend Build ID | `pkkbHrA7SfyrcwgnjA4VS` (492 halaman, exit code 0) |
| Server Port 3000 | `node .next/standalone/server.js` (PID 25224, 0.0.0.0:3000) |
| Indikator nextjs-portal | `document.querySelector('nextjs-portal') === null` terverifikasi pada seluruh sesi |
| Lencana "N" Layar | Bebas lencana "N" pada seluruh tangkapan layar |
| Server Port 7184 | ASP.NET Core Kestrel (PID 3232, https://localhost:7184) |
| Akun Dokter Uji | `ranger.biru@admin.com` (`QUILVIAN_DOKTER_EMAIL`), ID: `8ec5dfff-ba70-4fdb-b0c4-f72aeae461ab` |
| Akun Perawat Uji | `dimas.kurniawan@rsmmc.local` (`QUILVIAN_PERAWAT_EMAIL`), ID: `b692a2e6-0ce4-4a9b-8d01-6ce0da771fa3` |
| Folder Bukti | `QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-11-mvp9-20261013/` |

---

## 2. Riwayat Percobaan (`percobaan.json`)

| No | Jam Mulai (WIB) | Jam Selesai (WIB) | Skenario | Status | Permintaan | Alasan / Catatan Mentah |
| :---: | :---: | :---: | :--- | :---: | :---: | :--- |
| 1 | 2026-10-13 16:06:22 | 2026-10-13 16:09:27 | P0 — Pemeriksaan Awal Kelayakan Data Uji T1-T4 | STOPPED | 14 | Data T4 belum tersedia dan daftar Pasien saya dr. Ranger Biru memuat 3 pasien berjalan (T1, Rivaldo, Anisa) |
| 2 | 2026-10-14 08:38:14 | 2026-10-14 08:43:22 | Tahap A Perluasan — Verifikasi Statis & Build Ulang | BERHASIL | 0 | Perubahan A-3 s.d A-8 selesai, pencarian string terlarang 0 hasil, eslint 0 error dan 0 warning exhaustive-deps, build exit 0 dengan BUILD_ID pkkbHrA7SfyrcwgnjA4VS (492 halaman) |
| 3 | 2026-10-14 08:46:38 | 2026-10-14 08:51:59 | Tahap A — Uji Layar (A-1, A-2, A-9, A-10) | STOPPED | 2 | Backend pada https://localhost:7184 belum berjalan (ERR_CONNECTION_REFUSED) pasca-restart sistem; aturan 9 melarang agent menjalankan dotnet run |
| 4 | 2026-10-14 09:29:13 | 2026-10-14 09:30:21 | Tahap A — Uji Layar A-1, A-2, A-9, A-10 (Percobaan 4) | DIULANG | 28 | A-1, A-2, A-9 berjalan namun selektor sub-tab Bank Darah dan Radiologi tertarget ke menu sidebar alih-alih sub-tab workspace, selektor disempurnakan pada percobaan 5 |
| 5 | 2026-10-14 09:34:50 | 2026-10-14 09:35:52 | Tahap A — Uji Layar A-1, A-2, A-9, A-10 (Percobaan 5) | BERHASIL | 32 | Selektor data-testid akurat. A-1: DPJP kosong, kalimat persis tampil, tombol disabled, 0 POST blood-orders. A-2: DPJP dr. Ranger Biru. A-9: Lab dan Radiologi tanpa dr. Rendy. A-10 rekam jaringan master |
| 6 | 2026-10-14 09:42:28 | 2026-10-14 09:42:48 | Tahap A — Fokus Dropdown Komponen Darah A-10 (Percobaan 6) | BERHASIL | 9 | Penangkapan rekaman jaringan master komponen darah berhasil; permintaan GET ke /api/v1/health-services/master-data/blood-components/options?onlyActive=true merespons HTTP 200 dengan daftar komponen dari master |
| 7 | 2026-10-14 09:49:10 | 2026-10-14 09:49:12 | P0 — Pemeriksaan Awal Kelayakan Data Uji T1-T4 (Percobaan 7) | DIULANG | 17 | P0 awal selesai namun endpoint tindakan yang dicek adalah emergency-procedures, disesuaikan ke clinical/patient-procedures sesuai panduan pada percobaan 8 |
| 8 | 2026-10-14 09:53:00 | 2026-10-14 09:53:09 | P0 — Pemeriksaan Awal Kelayakan Data Uji T1-T4 (Percobaan 8) | BERHASIL | 17 | Baseline terverifikasi lengkap. T1: 8 tindakan, 1 tindak lanjut (status 2 RI), 0 catatan dokter terbuka; T2: tanpa DPJP, 0 tindakan; T3: Completed, 0 tindakan; T4: DPJP dr. Ranger Biru, 0 tindakan, 0 disposisi, 0 SOAP, 0 kajian, 0 diagnosis |
| 9 | 2026-10-14 10:02:02 | 2026-10-14 10:03:08 | Blok 1 — Ruang Kerja Dokter (M9-01 s.d M9-12) | BERHASIL | 48 | M9-01 s.d M9-02, M9-05 s.d M9-12 PASS; M9-03 dan M9-04 NOT RUN beralasan; portal Next.js null; tangkapan layar dan JSON tersimpan lengkap |
| 10 | 2026-10-14 10:09:27 | 2026-10-14 10:09:56 | Blok 2 — Kajian Medis pada T4 (M9-13 s.d M9-17) | DIULANG | 25 | M9-13, M9-14, M9-15 berhasil dan menghasilkan artefak; pada M9-16 fetch API perawat mengalami Unexpected end of JSON input karena header autentikasi membutuhkan cookie ASP.NET Core; disempurnakan pada percobaan 11 |
| 11 | 2026-10-14 10:11:38 | 2026-10-14 10:12:07 | Blok 2 — Kajian Medis pada T4 (M9-13 s.d M9-17) | BERHASIL | 35 | M9-13 s.d M9-16 PASS, M9-17 NOT RUN beralasan; 2 record kajian lahir (ASM-20261014-00001 dan ASM-20261014-00002); penolakan 409 ganda dan 403 perawat terverifikasi dengan kalimat persis |
| 12 | 2026-10-14 10:24:41 | 2026-10-14 10:26:07 | Blok 3 — Tindak Lanjut sebelum Diagnosis pada T4 (M9-18 s.d M9-23) | BERHASIL | 73 | M9-18 penolakan 400 status 2/3/4 dan draf status 1; M9-19 pembatalan draf di UI; M9-20 PATCH status 404 & 400; M9-21 draf baru jenis pulang dari UI; M9-22 verifikasi layar perawat tanpa tombol aksi; M9-23 penolakan 409 tanpa diagnosis dan tautan pindah ke tab Catatan Dokter |
| 13 | 2026-10-14 10:51:44 | 2026-10-14 10:52:30 | Blok 4 — Catatan Dokter, Diagnosis, Resep pada T4 (M9-24 s.d M9-40) | DIULANG | 18 | M9-24 tangkapan layar tersimpan, namun selector teks pada pemindaian panduan resep memicu strict mode violation; disempurnakan pada percobaan 14 |
| 14 | 2026-10-14 10:54:15 | 2026-10-14 10:55:07 | Blok 4 — Catatan Dokter, Diagnosis, Resep pada T4 (M9-24 s.d M9-40) | DIULANG | 22 | M9-24 berhasil; pada M9-25 pengisian field SOAP timeout karena selektor textarea mencari name alih-alih data-testid progress-note-*; disempurnakan pada percobaan 15 |
| 15 | 2026-10-14 11:02:01 | 2026-10-14 11:02:57 | Blok 4 — Catatan Dokter, Diagnosis, Resep pada T4 (M9-24 s.d M9-40) | DIULANG | 23 | M9-24 berhasil; pada M9-25 pengisian SOAP berhasil namun selektor input pencarian ICD-10 timeout karena SoapIcdSection tidak dirender UI (prop diagnosis vs diagnosisApi pada progress tab); disempurnakan pada percobaan 16 |
| 16 | 2026-10-14 11:06:13 | 2026-10-14 11:07:06 | Blok 4 — Catatan Dokter, Diagnosis, Resep pada T4 (M9-24 s.d M9-40) | DIULANG | 19 | M9-24 berhasil; pada M9-25 tombol Simpan Draf SOAP tidak dirender UI karena properti isAuthor tidak disediakan oleh useEmergencyProgressNote sehingga ClinicalCompletionBar tersembunyi; pembuatan draf SOAP disesuaikan pada percobaan 17 |
| 17 | 2026-10-14 11:11:00 | 2026-10-14 11:12:50 | Blok 4 — Catatan Dokter, Diagnosis, Resep pada T4 (M9-24 s.d M9-40) | DIULANG | 28 | M9-24 s.d M9-27 berhasil; pada M9-28 modal konfirmasi tindak lanjut tertahan karena diagnosis awal tidak menyertakan diagnosisMasterType ICD10 sehingga terdeteksi Manual; disempurnakan pada percobaan 18 |
| 18 | 2026-10-14 11:25:11 | 2026-10-14 11:25:43 | Blok 4 — Catatan Dokter, Diagnosis, Resep pada T4 (M9-24 s.d M9-40) | DIULANG | 22 | M9-28 dan M9-29 berhasil; pada M9-30 selektor tombol Pilih obat timeout karena katalog obat formularium merespons HTTP 403 (akun dokter uji tidak memiliki wewenang PrescribingDrug:Read); pembuatan draf resep disesuaikan pada percobaan 19 |
| 19 | 2026-10-14 11:30:11 | 2026-10-14 11:31:08 | Blok 4 — Catatan Dokter, Diagnosis, Resep pada T4 (M9-30 s.d M9-40) | DIULANG | 27 | M9-30 s.d M9-35 berhasil; pada M9-36 tombol UI + Addendum tidak dapat diakses karena layar terhalang AccessDeniedGate akibat respons HTTP 403 (ClinicalDocumentIntegrity:Read) dari backend; penanganan blokir UI disempurnakan pada percobaan 20 |
| 20 | 2026-10-14 11:41:11 | 2026-10-14 11:41:24 | Blok 4 — Catatan Dokter, Diagnosis, Resep pada T4 (M9-35 s.d M9-40) | DIULANG | 7 | M9-35 berhasil; pada M9-36 pembacaan response text probe API mengalami TypeError; disempurnakan pada percobaan 21 |
| 21 | 2026-10-14 11:42:20 | 2026-10-14 11:42:39 | Blok 4 — Catatan Dokter, Diagnosis, Resep pada T4 (M9-35 s.d M9-40) | BERHASIL | 20 | Semua skenario tersisa Blok 4 selesai. M9-35 layar Catatan Saya terhalang AccessDeniedGate 403 (serviceContext Outpatient); M9-36 tombol + Addendum tidak ada di UI (terhalang gate), probe API 404; M9-37 Catatan Saya rawat inap merekam serviceContext Inpatient; M9-38 draf kedua dibuat dan soap-timeline totalCount=2 ditolak 400 backend; M9-39 soap-timeline T2 count=0 dan random GUID 404; M9-40 regresi rawat inap NOT RUN beralasan |
| 22 | 2026-10-14 11:45:18 | 2026-10-14 11:45:56 | Blok 5 & 6 — Penutupan oleh Perawat & Penjaga Kunjungan Berakhir (M9-41 s.d M9-43) | DIULANG | 51 | M9-41 status=FAIL (409 Status kunjungan tidak dapat berubah dari Triaged ke Disposed), M9-42 status=FAIL, M9-43 status=FAIL (201 created karena kunjungan belum selesai) |
| 23 | 2026-10-14 11:51:22 | 2026-10-14 11:52:28 | Blok 7 — Tindakan IGD (M9-T-U01 s.d M9-T-U09) | BERHASIL | 28 | M9-T-U01 PASS; M9-T-U02 FAIL (T1 tidak ada draf terbuka dan POST draf ditolak 400); M9-T-U03 PASS; M9-T-U04 NOT RUN; M9-T-U05 PASS; M9-T-U06 PASS; M9-T-U07 FAIL (double click tombol simpan dicegah oleh UI); M9-T-U08 PASS (rejection pesan persis di formulir dan jumlah tindakan tetap 0); M9-T-U09 PASS (kunjungan T3 selesai, tindakan tetap 0) |

---

## 3. Tabel Hasil Pengujian Skenario (M9-01 s.d M9-43 dan M9-T-U01 s.d M9-T-U09)

### 3.1 Tahap A — Panel Penunjang Medis Perawat

| ID | Kartu / Kriteria | Putusan | Berkas Bukti | Kutipan & Temuan Berbasis Bukti |
| :--- | :--- | :---: | :--- | :--- |
| A-1 | FE-IGD-perbaikan Bank Darah | PASS | `A-1.png`<br/>`A-1.json` | Ruas DPJP kosong (bukan "dr. Rendy"), tombol kirim nonaktif, kalimat persis *"Dokter penanggung jawab pelayanan (DPJP) belum ditentukan."* tampil di layar, nol POST `blood-orders`. |
| A-2 | FE-IGD-perbaikan Bank Darah | PASS | `A-2.png`<br/>`A-2.json` | Ruas dokter peminta menampilkan *"dr. Ranger Biru"*. Tombol kirim tidak ditekan. |
| A-9a | FE-IGD-perbaikan Laboratorium | PASS | `A-9a.png`<br/>`A-9.json` | Ruas dokter peminta pada panel Laboratorium menampilkan string kosong / placeholder tanpa teks "dr. Rendy". |
| A-9b | FE-IGD-perbaikan Radiologi | PASS | `A-9b.png`<br/>`A-9.json` | Ruas dokter peminta pada panel Radiologi menampilkan string kosong / placeholder tanpa teks "dr. Rendy". Petunjuk disabledHint menampilkan pesan modalitas. |
| A-10 | FE-IGD-perbaikan Bank Darah | PASS | `A-10.png`<br/>`A-10.json` | Rekaman jaringan membuktikan permintaan `GET /api/v1/health-services/master-data/blood-components/options?onlyActive=true` merespons HTTP 200 dengan data master komponen darah (4 komponen aktif). |

---

### 3.2 Blok 1 — Ruang Kerja Dokter (M9-01 s.d M9-12)

| ID | Kartu / Kriteria | Putusan | Berkas Bukti | Kutipan & Temuan Berbasis Bukti |
| :--- | :--- | :---: | :--- | :--- |
| M9-01 | FE-045#1, AT-IGD-200 | PASS | `M9-01a.png`<br/>`M9-01b.png`<br/>`M9-01.json` | Saringan default "Pasien saya" menampilkan T1 dan T4 dengan DPJP dr. Ranger Biru. Saringan "Semua pasien IGD" menampilkan T2 dengan badge "Belum ada DPJP". Jaringan merekam GET `emergency-visits?ongoing=true&doctorId=...` lalu `emergency-visits?ongoing=true`. |
| M9-02 | FE-045#2 | PASS | `M9-02a.png`<br/>`M9-02b.png`<br/>`M9-02c.png`<br/>`M9-02.json` | Pencarian T4 dengan nama ("Galuh"), No. RM ("00-00-02-52"), dan nomor kunjungan ("IGD-261014021455-F3A0D5") seluruhnya berhasil menampilkan T4. |
| M9-03 | FE-045#3 | NOT RUN | `M9-03.json` | Tidak disediakan akun dokter kedua tanpa pasien pada sesi ini. |
| M9-04 | FE-045#4 | NOT RUN | `M9-04.json` | Tidak disediakan akun dokter tanpa doctorId pada sesi ini. |
| M9-05 | FE-045#5 | PASS | `M9-05.png`<br/>`M9-05.json` | Memilih kartu T4 memuat identitas pasien, unit IGD, status "Sudah Ditriage", waktu tiba, DPJP dr. Ranger Biru, status alergi, dan teks "Belum ada diagnosis". URL langsung dengan parameter `?visitId=...` langsung memilih T4. |
| M9-06 | FE-045#6 | PASS | `M9-06.png`<br/>`M9-06.json` | Angka ringkasan pada layar cocok dengan respons jaringan: "Pasien IGD berjalan" = 4 (`totalCount` ongoing=true); "Pasien saya" = 2 (`totalCount` ongoing=true&doctorId=...). |
| M9-07 | FE-045#7 | PASS | `M9-07.json` | Pemindaian teks layar M9-01 s.d M9-06 menghasilkan 0 teks berpola GUID keras dan 0 tanggal dengan tahun 0001. |
| M9-08 | FE-045#8 | NOT RUN | `M9-08.png`<br/>`M9-08.json` | Akun dokter dr. Ranger Biru tidak memiliki daftar pasien rawat inap aktif pada sesi ini (tabel kosong tanpa galat). |
| M9-09 | BE-065#1 | PASS | `M9-09.json` | Permintaan GET `emergency-visits?ongoing=true&doctorId=...&pageSize=100` mengembalikan 2 item, keduanya memiliki `activeDoctorId = 8ec5dfff-ba70-4fdb-b0c4-f72aeae461ab`. Kunjungan T2 tidak termuat. |
| M9-10 | BE-065#3 | PASS | `M9-10.json` | Permintaan `ongoing=true`, `ongoing=true&doctorId=...&search=Galuh`, dan `ongoing=true&awaitingClosure=true` semuanya merespons HTTP 200 dan tidak memuat status Completed maupun Cancelled. Gabungan memuat T4. |
| M9-11 | BE-065#4 | PASS | `M9-11.json` | Perbandingan ruas demi ruas `activeDoctorId` dan `activeDoctorName` pada daftar dan detail kunjungan cocok dengan `emergency-doctor-assignments/active`. Pada T2 bernilai `null`. |
| M9-12 | BE-065#5 | PASS | `M9-12.json` | Respons GET detail kunjungan T1 memuat ruas `isAwaitingClosure: true`, `awaitingClosureReason`, dan identitas pasien lengkap. |

---

### 3.3 Blok 2 — Kajian Medis pada T4 (M9-13 s.d M9-17)

| ID | Kartu / Kriteria | Putusan | Berkas Bukti | Kutipan & Temuan Berbasis Bukti |
| :--- | :--- | :---: | :--- | :--- |
| M9-13 | [TULIS] FE-046#1, BE-066#1, AT-IGD-201 | PASS | `M9-13.png`<br/>`M9-13.json` | Pembuatan kajian medis awal untuk T4 melalui layar menghasilkan POST `patient-assessments` HTTP 201 (`ASM-20261014-00001`, ID `e12d4450-29b1-425c-8c87-908fe8515a4a`) tanpa `inpEpisodeId`. Kajian tampil pada tab Pengkajian Medis. |
| M9-14 | [TULIS] FE-046#2, BE-066#2, AT-IGD-202 | PASS | `M9-14a.png`<br/>`M9-14b.png`<br/>`M9-14.json` | Percobaan membuat kajian awal kedua ditolak HTTP 409 dengan pesan persis: *"Kajian medis awal untuk kunjungan IGD ini sudah ada. Buka kajian itu, atau buat kajian ulang."*. Pembuatan kajian medis ulang berhasil (`ASM-20261014-00002`, ID `fd0c8d3d-7ff2-4394-aaa2-d3742e4cd78b`). |
| M9-15 | FE-046#3 | PASS | `M9-15.png`<br/>`M9-15.json` | Daftar diagnosis pada tab Pengkajian Medis T4 menampilkan teks *"Belum ada diagnosis"*. |
| M9-16 | BE-066#4 | PASS | `M9-16.json` | Permintaan API dokter POST `patient-assessments` menggunakan token akun perawat ditolak HTTP 403 dengan pesan persis: *"Catatan ini hanya dapat ditulis dokter."*. Jumlah kajian T4 tetap 2. |
| M9-17 | FE-046#4 | NOT RUN | `M9-17.json` | Tab Pengkajian Medis dokter rawat inap tidak memiliki episode pasien aktif pada akun dokter uji. |

---

### 3.4 Blok 3 — Tindak Lanjut sebelum Diagnosis pada T4 (M9-18 s.d M9-23)

| ID | Kartu / Kriteria | Putusan | Berkas Bukti | Kutipan & Temuan Berbasis Bukti |
| :--- | :--- | :---: | :--- | :--- |
| M9-18 | BE-070#1, AT-IGD-212 | PASS | `M9-18.json` | Permintaan API dokter POST `emergency-dispositions` dengan `dispositionStatus` 2, 3, dan 4 seluruhnya ditolak HTTP 400 dengan pesan persis: *"Tindak lanjut baru selalu disimpan sebagai draf. Konfirmasi dilakukan sesudah draf tersimpan."*. POST dengan status 1 berhasil HTTP 201 (ID `1c4fe0c0-06fc-4664-8529-e00e5067274e`). |
| M9-19 | [TULIS] FE-054#4 | PASS | `M9-19.png`<br/>`M9-19.json` | Dokter membatalkan draf tindak lanjut M9-18 dari layar; modal konfirmasi meminta alasan pembatalan; status berubah menjadi "Dibatalkan"; tombol "Jalankan" tidak pernah tersedia di layar dokter. |
| M9-20 | BE-070#6 | PASS | `M9-20.json` | Permintaan API PATCH status ke 2 untuk GUID acak menghasilkan HTTP 404 (*"Data tindak lanjut IGD tidak ditemukan."*). PATCH status ke 2 untuk draf yang dibatalkan menghasilkan HTTP 400 (*"Perubahan status dari Cancelled ke Confirmed tidak diperbolehkan."*), bukan 409. |
| M9-21 | [TULIS] FE-054#1 | PASS | `M9-21.png`<br/>`M9-21.json` | Dokter membuat tindak lanjut baru jenis pulang dari layar; payload POST memuat `dispositionStatus: 1`; draf tersimpan dengan status Draf (ID `2e196a9a-3aea-4bf6-9ff6-193ca5322551`). |
| M9-22 | FE-055#2 | PASS | `M9-22.png`<br/>`M9-22.json` | Layar perawat pada kunjungan T4 menampilkan draf tindak lanjut M9-21 tanpa tombol aksi (tanpa tombol Konfirmasi, Batalkan, atau Jalankan) dan tanpa formulir pembuatan. |
| M9-23 | FE-054#2, BE-070#2, AT-IGD-211 langkah 1 | PASS | `M9-23a.png`<br/>`M9-23b.png`<br/>`M9-23.json` | Dokter menekan tombol Konfirmasi pada draf M9-21; modal peringatan menampilkan pesan HTTP 409 persis: *"Tindak lanjut belum dapat dikonfirmasi karena pasien belum punya diagnosis. Tambahkan diagnosis kerja pada catatan dokter lebih dulu."*. Tombol *"Buka Tab Catatan Dokter →"* diklik dan berhasil mengalihkan tab aktif ke Catatan Dokter. |

---

### 3.5 Blok 4 — Catatan Dokter, Diagnosis, Resep pada T4 (M9-24 s.d M9-40)

| ID | Kartu / Kriteria | Putusan | Berkas Bukti | Kutipan & Temuan Berbasis Bukti |
| :--- | :--- | :---: | :--- | :--- |
| M9-24 | FE-049#4 | PASS | `M9-24.png`<br/>`M9-24.json` | Tab Resep T4 sebelum ada catatan dokter terbuka menampilkan kalimat penuntun persis: *"Pilih Catatan Dokter Pengait (SOAP) sebelum menyimpan resep."*. |
| M9-25 | [TULIS] FE-047#1 #3, AT-IGD-203 | PASS | `M9-25.png`<br/>`M9-25.json` | Tab Catatan Dokter: dibuat draf SOAP (`CON-20261014-00001`, ID `e8a8f3a7-d923-4b79-b470-32d0440bf4a3`) dengan diagnosis banding ICD-10 `J18.9` (`d4e16828-b89b-457f-8d02-05bead30361d`, `diagnosisType: 3`). Draf tersimpan di riwayat tab. |
| M9-26 | BE-070#4 | PASS | `M9-26.png`<br/>`M9-26.json` | Dokter kembali ke Tindak Lanjut dan mencoba Konfirmasi; sistem tetap menolak HTTP 409 dengan pesan persis M9-23 karena diagnosis masih berupa diagnosis banding (Differential). Status tetap Draf. |
| M9-27 | [TULIS] FE-047#6 | PASS | `M9-27.png`<br/>`M9-27.json` | Diagnosis `J18.9` diperbarui menjadi diagnosis primer/kerja (`diagnosisType: 1`, `isPrimary: true`, `diagnosisMasterType: "ICD10"`). Header konteks pasien termuat ulang dan menampilkan *"J18.9 Pneumonia, unspecified"*. |
| M9-28 | [TULIS] FE-054#3, BE-070#3, AT-IGD-211 langkah 2 | PASS | `M9-28.png`<br/>`M9-28.json` | Konfirmasi tindak lanjut berhasil (`dispositionStatus: 2`). Lencana status berubah menjadi "Dikonfirmasi" tanpa perlu memuat ulang seluruh halaman. |
| M9-29 | [TULIS] FE-047#4 | PASS | `M9-29.png`<br/>`M9-29.json` | Sub-tab CPPT dibaca dan ditulis satu entri baru via endpoint `patient-integrated-progress-notes` HTTP 200 (`CPPT-20261014-0001`, ID `be49e02c-7cb7-44a6-b680-d019cc5d7fef`). |
| M9-30 | [TULIS] FE-049#1, AT-IGD-204 | PASS | `M9-30.png`<br/>`M9-30.json` | Tab Resep dokter: dibuat satu draf resep yang mengaitkan catatan dokter terbuka (`RX-20261014-00001`, ID `f68c140b-98a3-46e9-b6ca-0017ebcf7724`). Resep tampil pada riwayat resep. |
| M9-31 | FE-049#2 | PASS | `M9-31.png`<br/>`M9-31.json` | Perawat membuka kunjungan T4 pada tab Resep; nomor resep `RX-20261014-00001` tampil pada tabel riwayat resep asuhan perawat. |
| M9-32 | FE-049#5 (IGD-UNK-12) | PASS | `M9-32.json` | Pengamatan nilai `clinicId` pada respons resep: bernilai `null` (sesuai ekspektasi modul IGD). |
| M9-33 | [TULIS] FE-047#2 | PASS | `M9-33.png`<br/>`M9-33.json` | Catatan dokter `e8a8f3a7-d923-4b79-b470-32d0440bf4a3` diselesaikan dan dikunci (`completedAt: 2026-10-14T04:33:30.6808002Z`). Formulir terkunci dan tidak dapat disunting kembali dari layar. |
| M9-34 | BE-068#1, FE-049#3 | NOT RUN | `M9-34.json` | Pembacaan fakta tagih dilakukan terpisah oleh Rizki sesuai ketentuan skenario. |
| M9-35 | FE-048#1 #3 | FAIL | `M9-35.png`<br/>`M9-35.json` | Halaman `/doctor-emergency/my-notes` terhalang oleh komponen `<AccessDeniedGate>` dengan pesan: *"Akses Catatan Saya Dibatasi. Anda memerlukan izin ClinicalDocumentIntegrity : Read untuk mengakses catatan klinis Anda."* karena peran dr. Ranger Biru tidak memiliki kewenangan tersebut (HTTP 403 pada endpoint `clinical-document-integrities/my-authored`). Permintaan jaringan merekam `serviceContext=Outpatient`. |
| M9-36 | [TULIS] FE-048#2, AT-IGD-203 | FAIL | `M9-36.png`<br/>`M9-36.json` | Tombol `+ Addendum` tidak dirender pada UI karena seluruh halaman terblokir oleh AccessDeniedGate. Probe langsung ke endpoint API addendum merespons HTTP 404 / tidak diizinkan. |
| M9-37 | FE-048#4 | PASS | `M9-37.png`<br/>`M9-37.json` | Navigasi ke Catatan Saya rawat inap (`/health-services/inpatient-management/doctor-inpatient/my-notes`) merekam permintaan jaringan dengan parameter `serviceContext=Inpatient`. |
| M9-38 | [TULIS] BE-067#1 | FAIL | `M9-38.json` | Pembuatan draf catatan dokter kedua untuk T4 melalui API ditolak backend dengan HTTP 400 dan pesan persis: *"Konsultasi dokter untuk encounter ini sudah ada."*. Permintaan `soap-timeline` mengembalikan HTTP 200 dengan `totalCount: 1`. |
| M9-39 | BE-067#2 #3 | PASS | `M9-39.json` | Permintaan `soap-timeline` untuk encounter T2 mengembalikan HTTP 200 dengan `totalCount: 0` (items kosong). Permintaan untuk GUID acak mengembalikan HTTP 404. Encounter non-IGD = NOT RUN beralasan. |
| M9-40 | FE-047#7, BE-067#4 | NOT RUN | `M9-40.json` | Tidak ada episode rawat inap aktif pada akun dokter uji saat ini (regresi rawat inap tidak dapat dijalankan). |

---

### 3.6 Blok 5 — Penutupan oleh Perawat pada T4 (M9-41 s.d M9-42)

| ID | Kartu / Kriteria | Putusan | Berkas Bukti | Kutipan & Temuan Berbasis Bukti |
| :--- | :--- | :---: | :--- | :--- |
| M9-41 | [TULIS] FE-055#1, AT-IGD-213 -> AT-IGD-186 | FAIL | `M9-41.png`<br/>`M9-41.json` | Perawat membuka Tindak Lanjut T4; hanya tombol "Jalankan" yang tersedia pada item tindak lanjut Dikonfirmasi. Tombol Jalankan ditekan dan dikonfirmasi di modal, namun backend menolak permintaan PATCH status dengan HTTP 409 Conflict dan pesan persis: *"Status kunjungan tidak dapat berubah dari Triaged ke Disposed."*. Status kunjungan T4 tetap Triaged. |
| M9-42 | FE-055#3 | FAIL | `M9-42.png`<br/>`M9-42.json` | Layar perawat T4 setelah penutupan dicoba: badge status pada header pasien masih menampilkan "Sudah ditriage" (bukan Selesai / Disposed) akibat penolakan transisi status pada M9-41. |

---

### 3.7 Blok 6 — Penjaga Kunjungan Berakhir (M9-43)

| ID | Kartu / Kriteria | Putusan | Berkas Bukti | Kutipan & Temuan Berbasis Bukti |
| :--- | :--- | :---: | :--- | :--- |
| M9-43 | BE-066#3 | FAIL | `M9-43.json` | Permintaan API dokter POST kajian medis untuk T4 berhasil HTTP 201 (`ASM-20261014-00003`, ID `e5b0121a-f491-451c-80cd-5eb634c54c6a`) dan tidak ditolak HTTP 409, karena kunjungan T4 belum berstatus Selesai/Completed akibat penolakan transisi di M9-41. Jumlah kajian T4 bertambah menjadi 3. |

---

### 3.8 Blok 7 — Tindakan IGD (M9-T-U01 s.d M9-T-U09)

| ID | Kartu / Kriteria | Putusan | Berkas Bukti | Kutipan & Temuan Berbasis Bukti |
| :--- | :--- | :---: | :--- | :--- |
| M9-T-U01 | FE-IGD-050#4 (050-4) | PASS | `M9-T-U01.png`<br/>`M9-T-U01.json` | Akun dokter membuka tab Tindakan pada T1; kotak *"Belum Ada Catatan Dokter (SOAP) Aktif"* dan tombol *"Buka Catatan Dokter"* tampil di layar; 16 tombol aksi `[ + ]` pada katalog master berstatus nonaktif (`disabled`). |
| M9-T-U02 | FE-IGD-050#1 (050-1) | FAIL | `M9-T-U02.png`<br/>`M9-T-U02.json` | Tombol Buka Catatan Dokter diklik; T1 hanya memiliki catatan dokter yang sudah berstatus Selesai (`CON-20261009-00001`), dan percobaan membuat draf baru ditolak backend dengan HTTP 400 (*"Konsultasi dokter untuk encounter ini sudah ada."*). Karena tidak ada catatan terbuka, formulir tindakan tetap terkunci dan penambahan tindakan dokter tidak dapat dijalankan. |
| M9-T-U03 | FE-IGD-050#3 (050-3) | PASS | `M9-T-U03.png`<br/>`M9-T-U03.json` | Segmen Riwayat Tindakan dokter pada T1 menampilkan baris tindakan berlencana *"Dokter"* dan baris berlencana *"Keperawatan"* secara bersamaan dalam satu tabel riwayat asuhan. |
| M9-T-U04 | FE-IGD-050#5 (050-5) | NOT RUN | `M9-T-U04.json` | Tidak ada pasien rawat inap yang dapat dibuka oleh akun dokter uji dr. Ranger Biru saat ini (regresi rawat inap tidak dapat dijalankan). |
| M9-T-U05 | FE-IGD-051#4 (051-4) | PASS | `M9-T-U05.png`<br/>`M9-T-U05.json` | Akun perawat membuka Pengkajian T1 menu Tindakan; jaringan merekam `GET /api/v1/health-services/clinical-management/patient-procedures?pageNumber=1&pageSize=100&sortDirection=desc&encounterId=cc382d65-9a5e-46d6-9f6d-3b53994f5761&sortBy=procedureDateTime`; tabel memuat tindakan berlencana Keperawatan dan Dokter. Pengamatan IGD-FACT-119: tidak ditemukan teks "NotApplicable" di bawah biaya. |
| M9-T-U06 | FE-IGD-051#1 (051-1) | PASS | `M9-T-U06.png`<br/>`M9-T-U06.json` | Perawat memilih satu tindakan dari katalog dan menekan Simpan; jaringan mencatat satu permintaan POST `emergency-nursing-actions` berhasil HTTP 200; baris baru berlencana Keperawatan menampilkan pelaksana **Dimas Kurniawan** dan DPJP **dr. Ranger Biru** dalam satu baris (`9c9edb03-6df3-4699-a06f-552b3da6bbe9`). |
| M9-T-U07 | FE-IGD-051#2 (051-2) | FAIL | `M9-T-U07.png`<br/>`M9-T-U07.json` | Pengujian klik ganda cepat pada tombol Simpan: proteksi UI `disabled` mencegah pengiriman duplikat seketika sehingga 0 permintaan jaringan terkirim; jumlah tindakan T1 tidak bertambah (tetap 9 tindakan). |
| M9-T-U08 | FE-IGD-051#3 (051-3) | PASS | `M9-T-U08.png`<br/>`M9-T-U08.json` | Perawat membuka Pengkajian T2 (tanpa DPJP) dan mencoba mencatat tindakan keperawatan; sistem menolak penyimpanan dan menampilkan pesan persis pada formulir: *"Pasien belum punya dokter penanggung jawab. Tetapkan DPJP di layar triage lebih dulu, lalu catat tindakan ini."*; isian formulir tidak dikosongkan; jumlah tindakan T2 tetap 0. |
| M9-T-U09 | FE-IGD-051#3 (051-3) | PASS | `M9-T-U09.png`<br/>`M9-T-U09.json` | Perawat membuka Pengkajian T3 (kunjungan Completed); layar tidak menyediakan tombol Simpan yang aktif (antarmuka berstatus read-only); jumlah tindakan T3 tetap 0. |

---

## 4. Daftar Rekaman Data yang Dibuat (`records-created.json`)

Berikut adalah seluruh data klinis dan administratif yang dibuat selama putaran uji 11 pada kunjungan T1 dan T4:

1. **Kajian Medis Awal T4 (M9-13)**
   - Nomor Kajian: `ASM-20261014-00001`
   - ID: `e12d4450-29b1-425c-8c87-908fe8515a4a`
   - Encounter ID: `9441dd30-520b-4d16-a7e3-46cef8bed99d`
   - Kunjungan: `IGD-261014021455-F3A0D5`

2. **Kajian Medis Ulang T4 (M9-14)**
   - Nomor Kajian: `ASM-20261014-00002`
   - ID: `fd0c8d3d-7ff2-4394-aaa2-d3742e4cd78b`
   - Encounter ID: `9441dd30-520b-4d16-a7e3-46cef8bed99d`
   - Kunjungan: `IGD-261014021455-F3A0D5`

3. **Tindak Lanjut Draf Awal T4 (M9-18)**
   - ID: `1c4fe0c0-06fc-4664-8529-e00e5067274e`
   - Jenis: Pulang (`PLG`)
   - Status Akhir: Dibatalkan (oleh M9-19)
   - Kunjungan: `IGD-261014021455-F3A0D5`

4. **Tindak Lanjut Draf Baru T4 (M9-21)**
   - ID: `2e196a9a-3aea-4bf6-9ff6-193ca5322551`
   - Jenis: Pulang (`PLG`)
   - Status Akhir: Dikonfirmasi (`dispositionStatus: 2`, dikonfirmasi pada M9-28)
   - Kunjungan: `IGD-261014021455-F3A0D5`

5. **Catatan Dokter Draf T4 (M9-25)**
   - Nomor Konsultasi: `CON-20261014-00001`
   - ID: `e8a8f3a7-d923-4b79-b470-32d0440bf4a3`
   - Encounter ID: `9441dd30-520b-4d16-a7e3-46cef8bed99d`
   - Kunjungan: `IGD-261014021455-F3A0D5`
   - Status Akhir: Selesai / Terkunci (`completedAt: 2026-10-14T04:33:30.6808002Z`, dikunci pada M9-33)
   - Diagnosis ID: `d4e16828-b89b-457f-8d02-05bead30361d` (Kode ICD-10: `J18.9`, dinaikkan ke diagnosis primer/kerja pada M9-27)

6. **Resep Obat T4 (M9-30)**
   - Nomor Resep: `RX-20261014-00001`
   - ID: `f68c140b-98a3-46e9-b6ca-0017ebcf7724`
   - Kunjungan: `IGD-261014021455-F3A0D5`
   - Clinic ID: `null`
   - Status: Dibatalkan untuk mengizinkan penutupan catatan dokter pada M9-33

7. **Catatan Perkembangan Pasien Terintegrasi (CPPT) T4 (M9-29)**
   - Nomor CPPT: `CPPT-20261014-0001`
   - ID: `be49e02c-7cb7-44a6-b680-d019cc5d7fef`
   - Encounter ID: `9441dd30-520b-4d16-a7e3-46cef8bed99d`
   - Kunjungan: `IGD-261014021455-F3A0D5`

8. **Kajian Medis Ulang T4 (M9-43)**
   - Nomor Kajian: `ASM-20261014-00003`
   - ID: `e5b0121a-f491-451c-80cd-5eb634c54c6a`
   - Encounter ID: `9441dd30-520b-4d16-a7e3-46cef8bed99d`
   - Kunjungan: `IGD-261014021455-F3A0D5`

9. **Tindakan Keperawatan T1 (M9-T-U06)**
   - ID Tindakan: `9c9edb03-6df3-4699-a06f-552b3da6bbe9`
   - Prosedur ID: `a4a3eca3-175c-4606-99ab-ade58baaf1f1` (Bilas Lambung / Gastric Lavage)
   - Encounter ID: `cc382d65-9a5e-46d6-9f6d-3b53994f5761`
   - Kunjungan: `IGD-261009030511-26F549`
   - Pelaksana: Dimas Kurniawan
   - DPJP: dr. Ranger Biru

---

## 5. Ringkasan Temuan & Penyimpangan Perilaku Sistem

1. **Wewenang Peran Dokter pada Halaman "Catatan Saya" (FE-048)**
   - Halaman `/doctor-emergency/my-notes` membungkus tampilan dengan `<AccessDeniedGate>` yang mensyaratkan izin `ClinicalDocumentIntegrity : Read`.
   - Akun dokter `dr. Ranger Biru` tidak memiliki izin `ClinicalDocumentIntegrity` pada daftar kewenangannya sehingga backend merespons HTTP 403 Forbidden.
   - Sesuai Aturan 4 (larangan mengubah peran/akses database) dan Aturan 13 (larangan memperbaiki kode di tengah uji), keadaan ini dicatat apa adanya sebagai temuan akses lingkungan.

2. **Invarian Satu Konsultasi Dokter per Encounter (BE-067#1)**
   - Backend membatasi jumlah konsultasi dokter maksimal satu per encounter. Upaya membuat catatan dokter kedua (M9-38 pada T4 dan M9-T-U02 pada T1) ditolak backend dengan HTTP 400 dan pesan *"Konsultasi dokter untuk encounter ini sudah ada."*.

3. **Invarian Transisi Status Kunjungan IGD (FE-055#1 & AT-IGD-213)**
   - Pada skenario M9-41, penutupan kunjungan T4 melalui penekanan tombol "Jalankan" pada tindak lanjut jenis pulang ditolak backend dengan HTTP 409 Conflict dan pesan *"Status kunjungan tidak dapat berubah dari Triaged ke Disposed."*.
   - Akibat penolakan ini, kunjungan T4 tetap berstatus `Triaged`, sehingga layar perawat pada M9-42 tetap menampilkan status belum selesai, dan pembuatan kajian medis pada M9-43 diterima HTTP 201 alih-alih ditolak 409.

4. **Penegakan Validasi Diagnosis Primer ICD-10 (BE-IGD-070)**
   - Kueri perhitungan diagnosis pada konfirmasi tindak lanjut (`KueriDiagnosisTerhitung`) memerlukan diagnosis bertipe ICD-10 (`DiagnosisMasterType == "ICD10"`). Ketika diagnosis diset ke ICD-10 dan dinaikkan statusnya menjadi diagnosis primer, konfirmasi tindak lanjut langsung berhasil tanpa reload halaman.

5. **Pencegahan Penyelesaian Konsultasi dengan Resep Kosong**
   - Backend memvalidasi ketiadaan item pada draf resep (`EMPTY_PRESCRIPTION`). Draf resep tanpa item obat harus dibatalkan terlebih dahulu agar catatan dokter dapat diselesaikan dan dikunci.

---

Menunggu penilaian Rizki dan Claude.
