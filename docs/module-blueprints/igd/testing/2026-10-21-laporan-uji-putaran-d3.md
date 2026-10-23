# Laporan Hasil Pengujian Modul IGD Quilvian — Putaran D3 (Hari 1)
**Tanggal Pengujian:** Rabu, 21 Oktober 2026  
**Ruang Lingkup:** Prioritas 3 — Tindakan dan Tindak Lanjut IGD  
**Dokumen Panduan:** `NewQuilvianSystemBackend/docs/module-blueprints/igd/testing/2026-10-17-panduan-uji-sisa-modul-igd.md` (Bagian 1, Bagian 5B.4, dan Bagian 5)  
**Tujuan Pengujian:** Menyediakan bukti empiris berbasis data mentah dan rekaman interaksi untuk kriteria acceptance pada `BE-IGD-070`, `FE-IGD-054`, dan `FE-IGD-055`.

---

## 1. Ringkasan Kesiapan Lingkungan & Stempel Waktu

- **Frontend:** Next.js Turbopack dev server (`npm run dev`) aktif melayani `http://localhost:3000`.
- **Backend:** ASP.NET Core Kestrel aktif melayani `https://localhost:7184`.
- **Stempel Awal & Akhir Lingkungan (`kode-awal.txt` dan `kode-akhir.txt`):**
  - **Git Commit Backend:** `025bfc6d0bd0364170211d01f1039b216760ce08` (parent `0c30b39c`)
  - **Git Commit Frontend:** `52624e6cbefe828d42acd732d9c7508198484a54` (prefix `52624e6cb`)
  - **Backend DLL:** `bin/Debug/net9.0/QuilvianSystemBackend.dll` (LastWriteTime: `2026-10-21 15:33:08.700`)
  - **Frontend BUILD_ID:** `N4Hi4E8coG8WfnJt1WLVT` (Build tanggal 21 Oktober 2026)
  - **Log Backend Hari Ini:** `NewQuilvianSystemBackend/Logs/quilvian-backend-20261021.json`
  - **Status Kode:** *Clean* (tidak ada pengubahan berkas source code aplikasi maupun roadmap).

---

## 2. Data Pasien Uji

- **P-D3-A:** `IGD-261006035400-91A5AB`
  - No. RM: `00-00-02-42`
  - Nama: `Pasien Pengganti 1791258839713_791`
  - Encounter ID: `239e0350-7e57-426c-8908-e7f4ab5af1ed`
  - Emergency Visit ID: `b1058414-1237-4a74-b584-48b2339b289f`
  - Dokter DPJP: `dr. Jenah Ali` (`ranger.biru@admin.com`)
  - Peran: Pasien uji kunjungan berjalan dengan DPJP, awalnya tanpa diagnosis kerja aktif.
- **P-D3-B:** `IGD-261006030206-DBD2A2` (RM `00-00-02-37`, tanpa DPJP)
- **P-D3-C:** `IGD-261018123311-11F483` (RM `00-00-02-73`, status Selesai)

*Catatan:* Sesuai instruksi penugasan, skenario `D3-U01` sampai `D3-U07` dilewati karena `FE-IGD-050` dan `FE-IGD-051` sudah selesai sejak 19 Oktober 2026.

---

## 3. Tabel Ringkasan Hasil Pengujian Putaran D3

| ID | Aktor | Deskripsi Langkah | Hasil | Berkas Bukti | Catatan & Bukti Verbatim |
| :--- | :--- | :--- | :---: | :--- | :--- |
| **D3-A01** | Dokter (API) | `POST emergency-dispositions` dengan `dispositionStatus` `2`, `3`, `4`, lalu `1` | **PASS** | `jaringan-D3-A01.json` | Status 2, 3, 4 ditolak HTTP 400 Bad Request verbatim: *"Tindak lanjut baru selalu disimpan sebagai draf. Konfirmasi dilakukan sesudah draf tersimpan."*; panggilan status 1 berhasil disimpan sebagai Draft (`dispositionStatus: 1`). |
| **D3-A02** | Dokter (API) | Tambah diagnosis `Differential` saja via SOAP, lalu coba konfirmasi | **PASS** | `jaringan-D3-A02.json` | Ditolak HTTP 409 Conflict verbatim: *"Tindak lanjut belum dapat dikonfirmasi karena pasien belum punya diagnosis. Tambahkan diagnosis kerja pada catatan dokter lebih dulu."* |
| **D3-A03** | Dokter (API) | `PATCH disposition-status` ID acak, lalu transisi tidak sah (*Draft* ke *Executed*) tanpa diagnosis | **PASS** | `jaringan-D3-A03.json` | ID acak menghasilkan HTTP 404 verbatim *"Data tindak lanjut IGD tidak ditemukan."*; Transisi Draft ke Executed ditolak HTTP 400 Bad Request verbatim *"Perubahan status dari Draft ke Executed tidak diperbolehkan."* (bukan 409). |
| **D3-U08** | Dokter (UI) | Buat tindak lanjut Pulang via antarmuka web | **PASS** | `D3-U08-akhir.png`<br>`jaringan-D3-U08.json` | POST terkirim dengan `dispositionStatus: 1`, berhasil HTTP 200. Tampil kartu draf tindak lanjut pada daftar riwayat. |
| **D3-U09** | Dokter (UI) | Konfirmasi tindak lanjut tanpa diagnosis kerja aktif di modal UI | **PASS** | `D3-U09-akhir.png`<br>`jaringan-D3-U09.json` | Modal konfirmasi memanggil PATCH status yang ditolak HTTP 409. Kotak peringatan merah menampilkan pesan verbatim penolakan, tombol *"Buka Tab Catatan Dokter →"* tampil dan berhasil mengalihkan layar ke tab Catatan Dokter. Status tetap Draf. |
| **D3-U10** | Dokter (UI) | Tambah diagnosis kerja ICD-10 aktif (`A08.1` Primary), lalu konfirmasi | **PASS** | `D3-U10-akhir.png`<br>`jaringan-D3-U10.json` | PATCH status dengan `dispositionStatus: 2` berhasil HTTP 200. Modal tertutup otomatis tanpa reload halaman, status kartu berubah menjadi *"Dikonfirmasi"*. |
| **D3-U11** | Dokter (UI) | Periksa ketiadaan aksi Jalankan, buat draf kedua, lalu batalkan | **PASS** | `D3-U11-akhir.png`<br>`jaringan-D3-U11.json` | Terbukti 0 tombol "Jalankan" di layar dokter. Tombol Batalkan di modal nonaktif sebelum alasan diisi. Sesudah alasan diisi, pembatalan berhasil HTTP 200 tanpa pemeriksaan diagnosis dan kartu berstatus *"Dibatalkan"*. |
| **D3-U12** | Perawat (UI) | Inspeksi tab Tindak Lanjut di layar perawat | **PASS** | `D3-U12-akhir.png`<br>`jaringan-D3-U12.json` | Formulir buat tidak ada (0 tombol), tombol Konfirmasi tidak ada (0), tombol Batalkan tidak ada (0). Kartu draf tampil polos tanpa tombol aksi (0). Kartu Dikonfirmasi tepat hanya memiliki 1 tombol aksi: *"Jalankan"*. |
| **D3-U13** | Perawat (UI) | Perawat menekan tombol Jalankan pada tindak lanjut Pulang yang Dikonfirmasi | **PASS** | `D3-U13-akhir.png`<br>`jaringan-D3-U13.json` | Modal konfirmasi memuat kalimat penutupan sesuai AT-IGD-186/213. PATCH status dengan `dispositionStatus: 3` berhasil HTTP 200. Kunjungan IGD otomatis ditutup (`visitStatus: 9` Completed), lencana *"Selesai Dijalankan"* tampil di kartu, dan badge kunjungan menjadi *"Selesai"*. |

---

## 4. Rincian Eksekusi & Bukti per Skenario

### D3-A01 (API Dokter: Validasi Pembuatan Status Tindak Lanjut Baru)
- **Aktor:** `ranger.biru@admin.com` (dr. Jenah Ali, Dokter IGD)
- **Eksekusi:** Pengujian pengiriman `POST /api/v1/health-services/emergency-installation-management/emergency-dispositions` dengan variasi status.
- **Hasil:**
  - Status 2 (Confirmed): Ditolak HTTP 400 (`"Tindak lanjut baru selalu disimpan sebagai draf. Konfirmasi dilakukan sesudah draf tersimpan."`).
  - Status 3 (Executed): Ditolak HTTP 400 (pesan sama).
  - Status 4 (Cancelled): Ditolak HTTP 400 (pesan sama).
  - Status 1 (Draft): Diterima HTTP 200 dengan status draf (`dispositionStatus: 1`, ID `9caa4503-13ce-45f9-a10d-789bb35ed0d9`).
- **Bukti Rekam Jaringan:** `jaringan-D3-A01.json`

### D3-A02 (API Dokter: Penolakan Konfirmasi Tanpa Diagnosis Kerja)
- **Aktor:** `ranger.biru@admin.com`
- **Eksekusi:** Catatan SOAP `CON-20261021-00003` dibuat, diagnosis ditambahkan hanya tipe `Differential` (`diagnosisType: 3`, ICD-10 `A09`). Dilakukan pemanggilan `PATCH emergency-dispositions/{id}/disposition-status` dengan `dispositionStatus: 2`.
- **Hasil:** Ditolak HTTP 409 Conflict dengan pesan verbatim:
  `"Tindak lanjut belum dapat dikonfirmasi karena pasien belum punya diagnosis. Tambahkan diagnosis kerja pada catatan dokter lebih dulu."`
- **Bukti Rekam Jaringan:** `jaringan-D3-A02.json`

### D3-A03 (API Dokter: Penanganan ID Tidak Valid & Transisi Tidak Sah)
- **Aktor:** `ranger.biru@admin.com`
- **Eksekusi:** 
  1. `PATCH emergency-dispositions/{random-guid}/disposition-status` dengan status 2 -> menghasilkan HTTP 404 `"Data tindak lanjut IGD tidak ditemukan."`.
  2. `PATCH emergency-dispositions/{id}/disposition-status` dari status Draft (1) langsung ke Executed (3) pada draf tanpa diagnosis -> menghasilkan HTTP 400 Bad Request verbatim `"Perubahan status dari Draft ke Executed tidak diperbolehkan."` (bukan 409).
- **Bukti Rekam Jaringan:** `jaringan-D3-A03.json`

### D3-U08 (UI Dokter: Pembuatan Draf Tindak Lanjut Pulang)
- **Aktor:** `ranger.biru@admin.com`
- **Layar:** Ruang Kerja Dokter IGD (`/doctor-emergency?visitId=b1058414-1237-4a74-b584-48b2339b289f`), tab Tindak Lanjut.
- **Eksekusi:** Dokter memilih jenis tindak lanjut "Pulang / Rawat Jalan" (`PLG`), mengisi alasan dan kondisi klinis, lalu menekan tombol "Simpan Draf Tindak Lanjut".
- **Hasil:** Panggilan `POST emergency-dispositions` mengirim payload dengan `dispositionStatus: 1` dan tersimpan sukses HTTP 200. Kartu draf tindak lanjut tampil pada daftar riwayat.
- **Bukti Tangkapan Layar:** `D3-U08-akhir.png`
- **Bukti Rekam Jaringan:** `jaringan-D3-U08.json`

### D3-U09 (UI Dokter: Penolakan Konfirmasi Tanpa Diagnosis di Antarmuka)
- **Aktor:** `ranger.biru@admin.com`
- **Eksekusi:** Dokter membuka segmen Riwayat, menekan tombol "Konfirmasi" pada kartu draf, lalu menekan tombol "Konfirmasi" di dalam modal.
- **Hasil:** Server menolak dengan HTTP 409. Di dalam modal yang masih terbuka, banner bahaya (`InformationAlert variant="danger"`) menampilkan pesan penolakan verbatim secara utuh. Tombol navigasi `"Buka Tab Catatan Dokter →"` tampil dan ketika diklik berhasil memindahkan tampilan ke tab Catatan Dokter (`doctor-note`). Status kartu tetap Draf.
- **Bukti Tangkapan Layar:** `D3-U09-akhir.png` (diambil selagi alert penolakan dan tombol navigasi aktif di dalam modal).
- **Bukti Rekam Jaringan:** `jaringan-D3-U09.json`

### D3-U10 (UI Dokter: Konfirmasi Sukses Pasca Penambahan Diagnosis Kerja)
- **Aktor:** `ranger.biru@admin.com`
- **Eksekusi:** Diagnosis kerja ICD-10 aktif (`A08.1` Primary, isPrimary: true) ditambahkan pada catatan dokter aktif encounter terkait. Dokter kembali ke tab Tindak Lanjut -> Riwayat, menekan tombol "Konfirmasi" pada kartu draf, lalu mengonfirmasi di modal.
- **Hasil:** Panggilan `PATCH disposition-status` dengan `dispositionStatus: 2` berhasil HTTP 200. Modal tertutup otomatis tanpa memuat ulang seluruh halaman, dan status kartu berubah menjadi *"Dikonfirmasi"*.
- **Bukti Tangkapan Layar:** `D3-U10-akhir.png`
- **Bukti Rekam Jaringan:** `jaringan-D3-U10.json`

### D3-U11 (UI Dokter: Penjagaan Wewenang Dokter & Pembatalan Ber-alasan)
- **Aktor:** `ranger.biru@admin.com`
- **Eksekusi:** 
  1. Pemeriksaan tombol di antarmuka dokter memastikan tidak ada tombol "Jalankan" (0 tombol).
  2. Dokter membuat draf tindak lanjut kedua via formulir.
  3. Dokter menekan tombol "Batalkan" pada draf kedua.
  4. Modal pembatalan meminta alasan: tombol "Batalkan" di modal dalam status nonaktif (*disabled*) selama alasan masih kosong.
  5. Dokter mengisi alasan pembatalan: *"Pembatalan draf tindak lanjut oleh DPJP: rencana penanganan diubah (D3-U11)."*, tombol menjadi aktif.
  6. Dokter mengonfirmasi pembatalan.
- **Hasil:** Panggilan `PATCH disposition-status` dengan `dispositionStatus: 4` dan alasan pembatalan berhasil HTTP 200 tanpa pemeriksaan diagnosis kerja. Status kartu berubah menjadi *"Dibatalkan"*.
- **Bukti Tangkapan Layar:** `D3-U11-akhir.png`
- **Bukti Rekam Jaringan:** `jaringan-D3-U11.json`

### D3-U12 (UI Perawat: Hak Akses & Tampilan Layar Tindak Lanjut Perawat)
- **Aktor:** `dimas.kurniawan@rsmmc.local` (Dimas Kurniawan, Perawat IGD)
- **Layar:** Pemeriksaan Pasien IGD (`/emergency-assessment/b1058414-1237-4a74-b584-48b2339b289f`), tab Tindak Lanjut.
- **Eksekusi:** Perawat membuka tab Tindak Lanjut dan menginspeksi elemen antarmuka yang tampil.
- **Hasil:** 
  - Tidak ada formulir pembuatan tindak lanjut (0 tombol).
  - Tidak ada tombol "Konfirmasi" di layar (0 tombol).
  - Tidak ada tombol "Batalkan" di layar (0 tombol).
  - Kartu draf tampil bersih tanpa tombol aksi (0 aksi).
  - Kartu Dikonfirmasi tepat hanya memiliki satu tombol aksi yaitu *"Jalankan"*.
- **Bukti Tangkapan Layar:** `D3-U12-akhir.png`
- **Bukti Rekam Jaringan:** `jaringan-D3-U12.json`

### D3-U13 (UI Perawat: Pelaksanaan Tindak Lanjut & Penutupan Kunjungan)
- **Aktor:** `dimas.kurniawan@rsmmc.local`
- **Eksekusi:** Perawat menekan tombol "Jalankan" pada kartu tindak lanjut Pulang yang berstatus Dikonfirmasi. Modal konfirmasi muncul memuat kalimat penutupan sesuai `AT-IGD-186`/`213`. Perawat menekan "Jalankan".
- **Hasil:** 
  - Panggilan `PATCH disposition-status` dengan `dispositionStatus: 3` berhasil HTTP 200.
  - Kunjungan IGD otomatis ditutup (`visitStatus: 9` Completed).
  - Lencana *"Selesai Dijalankan"* dan *"Dijalankan"* tampil pada kartu tindak lanjut.
  - Lencana status kunjungan pada panel Informasi Pasien berubah menjadi *"Selesai"*.
- **Bukti Tangkapan Layar:** `D3-U13-akhir.png`
- **Bukti Rekam Jaringan:** `jaringan-D3-U13.json`

---

## 5. Status Berkas Hasil Pengujian di Folder Putaran D3

Seluruh berkas bukti tersimpan lengkap di:  
`QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-d3-20261021/`

- `kode-awal.txt` dan `kode-akhir.txt`
- `persiapan.json` dan `data-uji.json`
- `percobaan.json` dan `hasil.json`
- `D3-U08-akhir.png`, `D3-U09-akhir.png`, `D3-U10-akhir.png`, `D3-U11-akhir.png`, `D3-U12-akhir.png`, `D3-U13-akhir.png`
- `jaringan-D3-A01.json`, `jaringan-D3-A02.json`, `jaringan-D3-A03.json`, `jaringan-D3-U08.json`, `jaringan-D3-U09.json`, `jaringan-D3-U10.json`, `jaringan-D3-U11.json`, `jaringan-D3-U12.json`, `jaringan-D3-U13.json`

*Pemberitahuan untuk Pemilik Sistem:* Folder `test-with-agy/` berada di dalam `.gitignore`. Salin folder ini secara manual ke komputer lokal Anda untuk pengarsipan permanen.
