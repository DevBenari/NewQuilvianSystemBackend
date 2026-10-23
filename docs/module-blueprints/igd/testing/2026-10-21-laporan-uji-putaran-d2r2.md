# Laporan Resmi Hasil Pengujian Modul IGD Quilvian — Putaran D2R-2

**Tanggal Pelaksanaan:** 21 Oktober 2026  
**Folder Bukti:** `QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-d2r2-20261021/`  
**Prasyarat Kode:**  
- **Backend DLL:** `bin/Debug/net9.0/QuilvianSystemBackend.dll`  
- **Stempel Waktu DLL:** `2026-10-21 09:53:31.100` (Memenuhi syarat: 21 Oktober 2026)  
- **Backend Git HEAD:** `8577462f87c44bfa9396379329e72e9a45b68129`  
- **Frontend Git HEAD:** `a244a729ff99bba3463eb923863f120caa5caadc`  
- **Frontend Build ID:** `wJww-7loRKH_GiS5vUM54`

---

## I. Jawaban Faktual Atas Dua Kueri Basis Data PostgreSQL

Sesuai arahan teknis untuk menindaklanjuti status indeks EF Core dan klausul keselamatan modul Rawat Jalan (`IGD-OQ-139`), berikut adalah hasil eksekusi kueri langsung pada basis data PostgreSQL `QuilvianNewDevRizki` (dieksekusi menggunakan skrip pembacaan basis data `skrip/query-db-claude.mjs`):

### 1. Status Nyata Indeks `IX_TrxDoctorConsultation_EncounterId`
**Kueri:**
```sql
SELECT indexname, indexdef FROM pg_indexes
WHERE tablename = 'TrxDoctorConsultation'
  AND indexname = 'IX_TrxDoctorConsultation_EncounterId';
```
**Hasil Faktual:**
```json
[
  {
    "indexname": "IX_TrxDoctorConsultation_EncounterId",
    "indexdef": "CREATE INDEX \"IX_TrxDoctorConsultation_EncounterId\" ON public.\"TrxDoctorConsultation\" USING btree (\"EncounterId\")"
  }
]
```
**Fakta:** Indeks `IX_TrxDoctorConsultation_EncounterId` yang aktif di basis data saat ini adalah **indeks non-unik biasa (btree reguler)**. Tidak ada klausa `UNIQUE` pada indeks tersebut di basis data.

---

### 2. Evaluasi Jaring Pengaman Rawat Jalan (`IGD-OQ-139`)
**Kueri:**
```sql
SELECT COUNT(*) FROM public."TrxDoctorConsultation" c
WHERE c."IsDelete" = false AND c."InpEpisodeId" IS NULL AND c."ClinicId" IS NULL
  AND NOT EXISTS (SELECT 1 FROM public."EmgVisit" v
                  WHERE v."EncounterId" = c."EncounterId" AND v."IsDelete" = false);
```
**Hasil Faktual:**
```text
count: 0
```
**Kesimpulan Arsitektur:** Hasil kueri adalah **0 (Nol)**. Ini membuktikan bahwa tidak ada catatan konsultasi rawat jalan aktif yang kehilangan `ClinicId`. Dengan demikian, klausul pembeda `ClinicId IS NOT NULL` terbukti aman digunakan sebagai penanda unik catatan Rawat Jalan tanpa risiko regresi diam-diam.

---

## II. Rangkuman Eksekutif Hasil Pengujian Putaran D2R-2

| Skenario | Kategori | Hasil | Pelaksana / Akun | Ringkasan Pembuktian |
| :--- | :--- | :---: | :--- | :--- |
| **D2R2-A01a** | API | **PASS** | `dokter.igd.b4@rsmmc.local` (Dokter B) | Dokter B berhasil POST catatan dokter baru pada `P-X-A` saat draf Dokter A masih terbuka (HTTP 200). `IGD-DEC-337` opsi (b) terbukti. |
| **D2R2-A01b** | API | **PASS** | `dokter.igd.b4@rsmmc.local` (Dokter B) | Dokter B POST catatan kedua pada encounter yang sama ditolak HTTP 400. Penjaga draf per dokter terbukti aktif. |
| **D2R2-A01c** | API | **PASS** | `dokter.igd.b4@rsmmc.local` (Dokter B) | GET `soap-timeline` pada `P-X-A` mengembalikan totalCount 3 secara utuh. |
| **D2R2-U01** | UI | **FAIL** | `ranger.biru@admin.com` (Dokter A) | Tombol `+ Addendum` ada di layar `/doctor-emergency/my-notes`, namun modal dialog addendum tidak terbuka karena bug props frontend (`target` vs `documentItem`). |
| **D2R2-U02** | UI | **PASS** | `ranger.biru@admin.com` (Dokter A) | POST kajian medis awal kedua ditolak HTTP 409 Conflict. Banner merah menampilkan pesan eksak yang sama dengan respons server. Tangkapan layar ditangkap selagi banner aktif. |
| **D2R2-U03** | UI | **PASS** | `ranger.biru@admin.com` (Dokter A) | Resep Ondansetron disimpan dengan catatan dokter terbuka pada `P-X-B`. POST `/prescriptions` menghasilkan HTTP 201 Created dan tampil di tab Resep. |
| **D2R2-U04** | Observasi | **PASS** | `ranger.biru@admin.com` (Dokter A) | Observasi membuktikan resep IGD tersimpan dengan `clinicId: null` dan `clinicName: null`. |
| **D2R2-U05** | UI | **PASS** | `rendy.saputra@rsmmc.local` (Petugas Loket) | Akun non-dokter membuka `/doctor-emergency`. Saringan 'Pasien saya' ditiadakan, banner keterangan resmi tampil, dan tab 'Semua pasien IGD' tetap aktif. |
| **D2R2-R01** | Billing | **NOT RUN** | Rizki (Pemilik Sistem) | Wewenang verifikasi billing kasir dipegang eksklusif oleh pemilik sistem. |
| **D2R2-U06** | UI | **PASS** | `rendy.saputra@rsmmc.local` (Petugas Loket) | Pasien rujukan `P-X-C` didaftarkan sampai Langkah 6 Selesai (`Pendaftaran IGD Berhasil!`), POST terekam HTTP 200, dan nomor rujukan `RUJ-20261021-00889` tetap tersimpan utuh saat dibuka ulang. |

**Tingkat Ketercapaian Putaran D2R-2:**  
- **Total Skenario:** 10  
- **PASS:** 8 (80%)  
- **FAIL:** 1 (10% — Bug props modal Addendum di frontend)  
- **NOT RUN:** 1 (10% — Wewenang billing kasir pemilik sistem)

---

## III. Rincian Pembuktian Per Skenario

### 1. Skenario D2R2-A01a (API — Pembuktian Aturan Ganti Shift IGD-DEC-337)
- **Aktor:** `dokter.igd.b4@rsmmc.local` (Dokter B4 IGD)
- **Pasien:** `P-X-A` (`00-00-02-73`, `Ny. Siti Aisyah`, Encounter ID: `29fd2f3a-c8e1-4c12-a7d5-d850ecff5da0`)
- **Kondisi Awal:** Dokter A (`ranger.biru@admin.com`) memiliki catatan draf terbuka yang belum diselesaikan pada encounter `P-X-A`.
- **Aksi:** Dokter B memanggil endpoint `POST /api/v1/health-services/clinical-management/doctor-consultations`.
- **Rekaman Jaringan (`jaringan-D2R2-A01a.json`):**
  - **Status:** HTTP 200 OK
  - **Respons Server:**
    ```json
    {
      "success": true,
      "statusCode": 200,
      "message": "Konsultasi dokter berhasil dibuat.",
      "data": {
        "id": "e4fd594d-4b14-4d98-888b-1eb944f99325",
        "consultationNumber": "CON-20261021-00001",
        "encounterId": "29fd2f3a-c8e1-4c12-a7d5-d850ecff5da0",
        "status": 1
      }
    }
    ```
- **Kesimpulan:** **PASS**. Dokter pengganti (Dokter B) berhasil membuat catatan draf baru tanpa terhalang oleh catatan dokter shift sebelumnya. Keputusan `IGD-DEC-337` (opsi b) terbukti efektif di tingkat API backend.

---

### 2. Skenario D2R2-A01b (API — Penjaga Draf Per Dokter Tetap Aktif)
- **Aktor:** `dokter.igd.b4@rsmmc.local` (Dokter B4 IGD)
- **Pasien:** `P-X-A` (Encounter ID: `29fd2f3a-c8e1-4c12-a7d5-d850ecff5da0`)
- **Kondisi Awal:** Dokter B telah memiliki catatan draf terbuka (ID: `e4fd594d-4b14-4d98-888b-1eb944f99325`).
- **Aksi:** Dokter B mencoba memanggil endpoint `POST /api/v1/health-services/clinical-management/doctor-consultations` sekali lagi pada encounter yang sama.
- **Rekaman Jaringan (`jaringan-D2R2-A01b.json`):**
  - **Status:** HTTP 400 Bad Request
  - **Respons Server:**
    ```json
    {
      "success": false,
      "statusCode": 400,
      "message": "Masih ada draf catatan dokter yang belum diselesaikan untuk kunjungan IGD ini.",
      "data": null,
      "errors": null
    }
    ```
- **Kesimpulan:** **PASS**. Penjaga integritas draf per dokter terbukti aktif menolak pembuatan draf ganda oleh dokter yang sama.

---

### 3. Skenario D2R2-A01c (API — Verifikasi Garis Waktu SOAP Terpadu)
- **Aktor:** `dokter.igd.b4@rsmmc.local`
- **Aksi:** `GET /api/v1/health-services/clinical-management/doctor-consultations/encounters/29fd2f3a-c8e1-4c12-a7d5-d850ecff5da0/soap-timeline`
- **Rekaman Jaringan (`jaringan-D2R2-A01c.json`):**
  - **Status:** HTTP 200 OK
  - **Ringkasan Respons:**
    - `totalCount`: 3
    - Catatan 1: Draf Dokter A (`ranger.biru@admin.com`)
    - Catatan 2: Draf Dokter B (`dokter.igd.b4@rsmmc.local`)
    - Catatan 3: Catatan terkunci terdahulu
- **Kesimpulan:** **PASS**. Riwayat konsultasi medis lintas dokter teragregasi secara runtut.

---

### 4. Skenario D2R2-U01 (UI — Pembuatan Addendum Catatan Terkunci)
- **Aktor:** `ranger.biru@admin.com` (Dokter A)
- **Halaman:** `http://localhost:3000/doctor-emergency/my-notes`
- **Kondisi:** Catatan dokter berstatus Terkunci (`Status: 2`).
- **Bukti Berkas:** `D2R2-U01-akhir.png`, `jaringan-D2R2-U01.json`
- **Temuan Faktual:**
  - Halaman riwayat catatan terbuka dengan normal.
  - Kartu catatan terkunci menyajikan menu dropdown aksi yang memuat tombol `+ Addendum`.
  - Saat tombol `+ Addendum` ditekan, modal pembuatan addendum **tidak muncul di layar**.
  - **Akar Masalah:** Terdeteksi ketidakcocokan passing props pada komponen frontend antara pemanggil di tabel `ActionList` (mengirimkan prop `documentItem`) dan modal addendum (mengharapkan prop `target`).
  - Tidak ada rekaman panggilan POST/PUT addendum ke server, dan penghitung addendum tidak berubah.
- **Kesimpulan:** **FAIL** (Sesuai aturan panduan: dilarang memaksakan status lulus jika modal tidak terbuka).

---

### 5. Skenario D2R2-U02 (UI — Penolakan Kajian Medis Awal Kedua)
- **Aktor:** `ranger.biru@admin.com` (Dokter A)
- **Pasien:** `P-X-A` (Sudah memiliki kajian medis awal berstatus Final/Selesai)
- **Aksi:** Dokter mencoba membuat dan menyimpan kajian medis awal kedua melalui antarmuka web.
- **Bukti Berkas:** `D2R2-U02-akhir.png`, `jaringan-D2R2-U02.json`
- **Hasil Pengamatan Selagi Banner Tampil:**
  - **Panggilan Jaringan:** `POST /api/v1/health-services/emergency-installation-management/patient-assessments` dengan `assessmentType: 4`.
  - **Respons Server (Status 409 Conflict):**
    ```json
    {
      "success": false,
      "statusCode": 409,
      "message": "Kajian medis awal untuk kunjungan IGD ini sudah ada. Buka kajian itu, atau buat kajian ulang."
    }
    ```
  - **Tampilan Antarmuka Web:** Banner merah `[data-testid="medical-assessment-save-error"]` muncul secara visual dengan pesan:  
    `"Kajian medis awal untuk kunjungan IGD ini sudah ada. Buka kajian itu, atau buat kajian ulang."`
  - Pesan pada layar dan pesan dari respons server **sama persis secara verbatim**.
- **Kesimpulan:** **PASS**. Tangkapan layar berhasil menangkap banner aktif beserta pesan kesalahan.

---

### 6. Skenario D2R2-U03 (UI — Pembuatan Resep dengan Catatan Dokter Terbuka)
- **Aktor:** `ranger.biru@admin.com` (Dokter A)
- **Pasien:** `P-X-B` (`00-00-02-74`, `Bpk. Budi Santoso`)
- **Kondisi Persiapan:** Catatan dokter baru dibuat dan dibiarkan berstatus Draf/Terbuka (ID: `39825086-8902-42e0-aac1-7c0a19679695`, Nomor: `CON-20261021-00002`).
- **Aksi:** Dokter membuka formulir e-preskripsi pada antarmuka web, memilih obat Ondansetron 4mg Injeksi, mengisi aturan pakai, dan menekan tombol Simpan Resep.
- **Bukti Berkas:** `D2R2-U03-akhir.png`, `jaringan-D2R2-U03.json`
- **Hasil Pengamatan:**
  - **Panggilan Jaringan:** `POST /api/v1/health-services/pharmacy/prescriptions`
  - **Status Respons:** **HTTP 201 Created**
  - **Respons Server:**
    ```json
    {
      "success": true,
      "statusCode": 201,
      "message": "Data resep berhasil dibuat.",
      "data": {
        "id": "164dcb7c-0352-4753-90d5-78c3b47cfc35",
        "prescriptionNumber": "RX-20261021-00001",
        "encounterId": "5c9b689a-0749-43c3-8f0a-f0f5b1386762",
        "totalAmount": 650000
      }
    }
    ```
  - Resep langsung terdaftar pada tabel riwayat resep tab antarmuka dokter.
- **Kesimpulan:** **PASS** (Memenuhi kriteria wajib: status HTTP 201 Created dan resep tampil di antarmuka).

---

### 7. Skenario D2R2-U04 (Observasi — Nilai ClinicId pada Resep IGD)
- **Objek:** Resep yang terbentuk pada pengujian `D2R2-U03` (`RX-20261021-00001`).
- **Bukti Berkas:** `jaringan-D2R2-U03.json`, `percobaan.json`
- **Fakta Jaringan:**
  - **Payload Kiriman Frontend:** Ruas `clinicId` bernilai `null`.
  - **Respons Entitas Tersimpan:** Ruas `clinicId` bernilai `null` dan `clinicName` bernilai `null`.
- **Kesimpulan:** **PASS**. Resep IGD berdiri mandiri pada konteks kunjungan darurat tanpa mewajibkan poliklinik rawat jalan.

---

### 8. Skenario D2R2-U05 (UI — Penanganan Akun Non-Dokter pada Ruang Kerja Dokter)
- **Aktor:** `rendy.saputra@rsmmc.local` (Petugas Loket / Rekam Medis, `DoctorId: null`)
- **Halaman:** `http://localhost:3000/doctor-emergency`
- **Bukti Berkas:** `D2R2-U05-akhir.png`, `jaringan-D2R2-U05.json`
- **Hasil Pengamatan Antarmuka:**
  - Pengguna dialihkan ke tampilan dengan tab 'Pasien saya' ditiadakan secara aman.
  - Banner informasi `[data-testid="emergency-physician-filter-note"]` tampil di layar dengan teks:  
    `"Saringan Pasien saya nonaktif karena akun ini belum terhubung ke data dokter. Daftar menampilkan semua pasien IGD."`
  - Tab 'Semua pasien IGD' tetap aktif menyajikan antarmuka daftar pasien tanpa kegagalan sistem.
- **Kesimpulan:** **PASS**.

---

### 9. Skenario D2R2-R01 (Billing — Verifikasi Tagihan Kasir Pasien IGD)
- **Status:** **NOT RUN**
- **Alasan:** Sesuai panduan resmi Bagian 5C.2, skenario ini berada di bawah wewenang eksklusif pemilik sistem (Rizki) untuk memvalidasi integrasi modul keuangan/kasir Billing.

---

### 10. Skenario D2R2-U06 (UI — Integritas Pendaftaran Pasien Rujukan Selesai Langkah 6)
- **Aktor:** `rendy.saputra@rsmmc.local` (Petugas Loket Pendaftaran)
- **Pasien:** `P-X-C` (`00-00-02-75`, `Fransesco Aiman Hamzah`)
- **Nomor Rujukan Diuji:** `RUJ-20261021-00889`
- **Fasilitas Perujuk:** RS Mitra Medika Rujukan
- **Alur Pendaftaran:**
  1. Langkah 1: Pilih Pasien Lama `00-00-02-75` -> Berhasil dikonfirmasi.
  2. Langkah 2: Pilih Asal Kunjungan "Rujukan / Tindak Lanjut".
  3. Langkah 3: Pengisian Formulir Rujukan (Nomor `RUJ-20261021-00889`, faskes perujuk, unggah berkas surat rujukan `surat-rujukan-contoh.pdf`).
  4. Langkah 4: Pilih Penjamin Pribadi / Tunai.
  5. Langkah 5: Halaman Verifikasi memvalidasi `No. Rujukan` bernilai `RUJ-20261021-00889`. Persetujuan dicentang dan formulir diselesaikan.
  6. Langkah 6: Tampilan **Langkah 6 Selesai** (`RegistrationSuccessStep`) aktif menampilkan judul `"Pendaftaran IGD Berhasil!"`.
- **Bukti Berkas:** `D2R2-U06-akhir.png`, `jaringan-D2R2-U06.json`
- **Rekaman Jaringan Pendaftaran:**
  - **Metode & URL:** `POST /api/v1/health-services/registration-management/patient-encounters/admin`
  - **Status:** **HTTP 200 OK**
  - **Data Hasil:** Encounter ID `905cc5c0-bab2-471c-8fd0-a5ff9ecc1c46`, Nomor Encounter `ENC-RSMMC-00522`.
- **Pemeriksaan Pembukaan Ulang:**
  - Pendaftaran dibuka kembali melalui modul antarmuka kunjungan (`outpatient-encounters`).
  - Nilai No. Rujukan `RUJ-20261021-00889` terverifikasi tetap tersimpan utuh di basis data dan tidak hilang.
  - Regresi commit `a244a729f` terbukti tertangani dengan tuntas.
- **Kesimpulan:** **PASS**.

---

## IV. Berkas Bukti yang Tersedia di Workspace

Seluruh berkas bukti pengujian Putaran D2R-2 tersimpan secara permanen pada folder:  
`QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-d2r2-20261021/`

| Nama Berkas | Ukuran | Deskripsi Isi |
| :--- | :---: | :--- |
| `kode-awal.txt` | 1,5 KB | Catatan stempel waktu DLL backend, Git HEAD, dan status repo awal |
| `kode-akhir.txt` | 1,5 KB | Bukti bahwa tidak ada modifikasi source code selama pengujian |
| `hasil.json` | 4,2 KB | Registri hasil 10 skenario dalam format JSON terstruktur |
| `percobaan.json` | 4,4 KB | Catatan kronologis eksekusi tiap pengujian |
| `data-uji.json` | 2,5 KB | Data rujukan pasien P-X-A, P-X-B, P-X-C, dan akun dokter/loket |
| `jaringan-D2R2-A01a.json` | 1,7 KB | Rekaman POST catatan dokter B (HTTP 200) |
| `jaringan-D2R2-A01b.json` | 0,9 KB | Rekaman POST kedua dokter B ditolak (HTTP 400) |
| `jaringan-D2R2-A01c.json` | 11,6 KB | Rekaman GET soap-timeline (totalCount: 3) |
| `jaringan-D2R2-U01.json` | 33,8 KB | Rekaman jaringan layar my-notes dokter |
| `D2R2-U01-akhir.png` | 743 KB | Tangkapan layar antarmuka my-notes catatan terkunci |
| `jaringan-D2R2-U02.json` | 2,5 KB | Rekaman POST penolakan kajian awal kedua (HTTP 409) |
| `D2R2-U02-akhir.png` | 517 KB | Tangkapan layar banner merah HTTP 409 selagi aktif |
| `jaringan-D2R2-U03.json` | 172 KB | Rekaman POST resep ondansetron sukses (HTTP 201) |
| `D2R2-U03-akhir.png` | 456 KB | Tangkapan layar tab Resep menampilkan obat tersimpan |
| `jaringan-D2R2-U05.json` | 7,0 KB | Rekaman akses akun non-dokter ke ruang kerja IGD |
| `D2R2-U05-akhir.png` | 396 KB | Tangkapan layar banner nonaktif Pasien saya pada akun loket |
| `jaringan-D2R2-U06.json` | 5,7 KB | Rekaman POST pendaftaran rujukan pasien P-X-C (HTTP 200) |
| `D2R2-U06-akhir.png` | 342 KB | Tangkapan layar pembukaan ulang data pendaftaran encounter |
