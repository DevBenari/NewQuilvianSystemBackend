# Laporan Resmi Hasil Pengujian Modul IGD Quilvian — Putaran D2R-3

**Tanggal Pelaksanaan:** 21 Oktober 2026  
**Folder Bukti:** `QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-d2r3-20261021/`  
**Prasyarat & Lingkungan Kode:**  
- **Frontend Fixed File:** `src/components/view/health-services/emergency-installation-management/doctor-emergency/my-notes/emergency-locked-notes-tab.jsx`  
- **Stempel Waktu Perbaikan Frontend:** `2026-10-21 11:01:53.408` (Memenuhi prasyarat: diperbaiki 21 Oktober 2026)  
- **Mode Server Frontend:** `npm run dev` (Fast Refresh live aktif pada `http://localhost:3000`)  
- **Backend DLL:** `bin/Debug/net9.0/QuilvianSystemBackend.dll`  
- **Stempel Waktu DLL Backend:** `2026-10-21 09:53:31.100`  
- **Backend Git HEAD:** `8577462f87c44bfa9396379329e72e9a45b68129`  
- **Frontend Git HEAD:** `a244a729ff99bba3463eb923863f120caa5caadc`

---

## I. Rangkuman Eksekutif Putaran D2R-3

Putaran D2R-3 dijalankan secara komprehensif untuk menuntaskan seluruh butir evaluasi dari Putaran D2R-2 dan D2R-1 berdasarkan panduan pengujian Bagian 5C.4, koreksi 5B.4, serta arahan verifikasi pemilik modul.

| Skenario | Kategori | Hasil | Pelaksana / Akun | Ringkasan Pembuktian & Temuan Faktual |
| :--- | :--- | :---: | :--- | :--- |
| **`D2R3-U01`** | UI | **PASS** | `ranger.biru@admin.com` (dr. Jenah Ali) | Modal Tambah Addendum terbuka (perbaikan prop `documentItem={addendumTarget}` terbukti efektif, `IGD-FACT-196`). Panggilan POST tulis addendum terekam HTTP 201 Created. Penghitung tombol berubah dari `Addendum (0)` ke `Addendum (1)`. Modal riwayat addendum terbuka menampilkan Addendum #1, waktu, pelaku `dr. Jenah Ali`, alasan koreksi, dan teks addendum secara lengkap. |
| **`D2R3-U02`** | UI | **PASS** | `ranger.biru@admin.com` (dr. Jenah Ali) | Coba simpan kajian medis awal kedua pada `P-X-A` ditolak HTTP 409 Conflict. Sesuai koreksi `IGD-FACT-197`, banner merah `[data-testid="medical-assessment-save-error"]` digulirkan ke viewport dan ditangkap jelas pada tangkapan layar halaman penuh (`fullPage: true`) selagi banner aktif. |
| **`D2R3-U03`** | UI | **PASS** | `dimas.kurniawan@rsmmc.local` (Dimas Kurniawan - Perawat IGD) | Akun perawat (tanpa tautan `DoctorId`, berhak membaca kunjungan IGD) membuka `/doctor-emergency`. Banner saringan nonaktif `[data-testid="emergency-physician-filter-note"]` muncul dengan teks: *"Saringan Pasien saya nonaktif karena akun ini belum terhubung ke data dokter. Daftar menampilkan semua pasien IGD."*. Daftar pasien IGD tetap terisi sukses HTTP 200 (100 dari 196 pasien tampil, header menulis `Pasien IGD berjalan 196`), tanpa galat 403. |
| **`D2R3-R01`** | Billing | **PASS** | `ranger.biru@admin.com` (dr. Jenah Ali) & `superadmin@admin.com` | **Langkah A:** Catatan dokter `CON-20261021-00002` (RM `00-00-02-48`, encounter `7dab5614-06ba-4d4b-9a6c-b73b6f8bb38b`) diselesaikan via `PATCH /complete` (HTTP 200, status Completed, 1 resep RX difinalisasi).<br>**Langkah B:** Layar Billing Kasir dibuka (`/cashier-overview` lalu Rincian Tagihan). Terbukti 2 hal mutlak: (1) Baris resep/obat `Resep RX-20261021-00001` (Ondansetron/Narfoz) **ADA** (Rp 650.000,00); (2) Baris jasa konsultasi dokter **TIDAK ADA** (sesuai invariant IGD). |

**Tingkat Ketercapaian Skenario D2R-3:**  
- **Total Skenario:** 4  
- **PASS:** 4 (100% Penuh)  
- **FAIL:** 0 (Nol)  
- **NOT RUN:** 0 (Nol)  

---

## II. Rincian Pembuktian Per Skenario

### 1. Skenario D2R3-U01 (UI — Pembuatan Addendum dan Pembuktian Riwayat Terkunci)
- **Aktor:** `ranger.biru@admin.com` (dr. Jenah Ali)
- **Halaman:** `http://localhost:3000/health-services/emergency-installation-management/doctor-emergency/my-notes`
- **Kondisi Dokumen Asli:** Catatan dokter IGD berstatus Selesai/Terkunci (`documentKind: 2`, `documentId: 62b95a3c-94a2-41a4-9a95-c922dbc0c5e3`, Pasien: Pasien Uji Jenah Ali, RM `00-00-02-76`).
- **Langkah Pengujian:**
  1. Halaman catatan dokter IGD dibuka; tabel catatan terkunci menampilkan tombol `Addendum (0)` dan tombol `+ Addendum`.
  2. Tombol `+ Addendum` diklik.
  3. Komponen modal `[data-testid="addendum-create-modal"]` berhasil muncul di layar (membuktikan perbaikan prop `documentItem={addendumTarget}` pada `emergency-locked-notes-tab.jsx` berjalan sempurna).
  4. Penguji mengisikan data addendum:
     - **Alasan Koreksi:** `"Koreksi riwayat alergi pasien dan klarifikasi terapi analgesik"`
     - **Isi Koreksi / Addendum:** `"Pasien mengonfirmasi riwayat alergi antibiotik amoxicillin. Diberikan alternatif terapi non-penisilin dan analgesik paracetamol 500mg p.r.n."`
  5. Tombol `Simpan Addendum` ditekan.
  6. Backend merespons sukses HTTP 201 dan modal tertutup secara otomatis.
- **Bukti Jaringan (`jaringan-D2R3-U01.json`):**
  - **Metode & URL:** `POST /api/v1/health-services/medical-record-management/clinical-note-addendums/by-document/2/62b95a3c-94a2-41a4-9a95-c922dbc0c5e3`
  - **Status:** **HTTP 201 Created**
  - **Respons Server:**
    ```json
    {
      "success": true,
      "statusCode": 201,
      "message": "Addendum catatan klinis berhasil dibuat.",
      "data": {
        "id": "23fa34cf-3467-4ab2-9d8a-7d40eb424c56",
        "sequence": 1,
        "authorName": "dr. Jenah Ali"
      }
    }
    ```
- **Pemeriksaan Pembuktian Layar:**
  - Tabel catatan terkunci melakukan pembaruan data secara reaktif (`refetch()`).
  - Label tombol berubah dari **`Addendum (0)`** menjadi **`Addendum (1)`**.
  - Tombol `Addendum (1)` diklik untuk membuka modal dialog `AddendumHistoryModal` (`[data-testid="addendum-history-modal"]`).
  - Modal riwayat addendum menampilkan:
    - Urutan: **Addendum #1**
    - Waktu: **21 Okt 2026, 11:22**
    - Penulis Koreksi: **dr. Jenah Ali**
    - Alasan: **Koreksi riwayat alergi pasien dan klarifikasi terapi analgesik**
    - Isi: **Pasien mengonfirmasi riwayat alergi antibiotik amoxicillin. Diberikan alternatif terapi non-penisilin dan analgesik paracetamol 500mg p.r.n.**
- **Bukti Gambar:** `D2R3-U01-akhir.png` menangkap layar penuh yang menampilkan modal riwayat addendum dengan rincian identitas dokter, waktu, alasan, dan teks addendum, dengan tombol latar belakang yang memperlihatkan label `Addendum (1)`.
- **Kesimpulan:** **PASS**.

---

### 2. Skenario D2R3-U02 (UI — Penolakan Kajian Medis Awal Kedua HTTP 409 & Banner Tertangkap)
- **Aktor:** `ranger.biru@admin.com` (dr. Jenah Ali)
- **Pasien:** `P-X-A` (RM `00-00-02-76`, Kunjungan IGD ID: `c74ac3e9-6f96-4dce-94d8-f17248fa0352`)
- **Kondisi:** Pasien sudah memiliki kajian medis awal berstatus Final/Selesai (`ASM-20261020-00001`).
- **Aksi:** Dokter membuka tab Pengkajian Medis, membuka form pengkajian baru, mengisi keluhan utama, dan menekan tombol Simpan Draf kajian medis awal (`assessmentType: 4`).
- **Hasil Pengamatan Jaringan (`jaringan-D2R3-U02.json`):**
  - **Metode & URL:** `POST /api/v1/health-services/clinical-management/patient-assessments`
  - **Status:** **HTTP 409 Conflict**
  - **Pesan Respons Server (Verbatim):**
    `"Kajian medis awal untuk kunjungan IGD ini sudah ada. Buka kajian itu, atau buat kajian ulang."`
- **Hasil Pengamatan Layar & Cara Penangkapan Gambar (`IGD-FACT-197`):**
  - Banner merah `[data-testid="medical-assessment-save-error"]` muncul di bagian tengah form antarmuka web dengan teks:  
    `"Kajian medis awal untuk kunjungan IGD ini sudah ada. Buka kajian itu, atau buat kajian ulang."`
  - Pesan pada antarmuka web dan pesan server **sama persis secara verbatim**.
  - Elemen banner merah digulirkan ke dalam viewport menggunakan `scrollIntoViewIfNeeded()`.
  - Tangkapan layar diambil menggunakan mode **halaman penuh (`fullPage: true`)** selagi banner merah masih aktif di layar.
- **Bukti Gambar:** `D2R3-U02-akhir.png` secara nyata dan jelas memperlihatkan banner kotak merah bergaris tepi merah berisi teks penolakan HTTP 409 di atas navigasi tab pengkajian medis.
- **Kesimpulan:** **PASS**.

---

### 3. Skenario D2R3-U03 (UI — Penanganan Akun Perawat Tanpa Tautan Dokter)
- **Aktor:** `dimas.kurniawan@rsmmc.local` (Dimas Kurniawan - Perawat IGD)
- **Karakteristik Akun:** Akun tidak memiliki relasi `MstDoctor` (`DoctorId: null`), memiliki perizinan baca kunjungan IGD (`EmergencyVisit:Read`).
- **Langkah Pengujian:**
  1. Login sebagai perawat Dimas Kurniawan.
  2. Membuka rute `/doctor-emergency` (`http://localhost:3000/health-services/emergency-installation-management/doctor-emergency`).
  3. Mengamati perilaku selektor filter pasien dan banner informatif.
  4. Mengamati panggilan jaringan untuk pengambilan daftar pasien IGD.
- **Hasil Pengamatan Antarmuka Web:**
  - Banner informatif `[data-testid="emergency-physician-filter-note"]` tampil di layar dengan teks:  
    `"Saringan Pasien saya nonaktif karena akun ini belum terhubung ke data dokter. Daftar menampilkan semua pasien IGD."`
  - Tab "Pasien saya" nonaktif, beralih otomatis ke tab "Semua pasien IGD".
  - Daftar pasien tetap terisi lengkap tanpa halaman kosong dan tanpa galat otorisasi (100 dari 196 pasien ditampilkan pada daftar, badge header menampilkan `Pasien IGD berjalan 196`).
- **Hasil Pengamatan Jaringan (`jaringan-D2R3-U03.json`):**
  - **Metode & URL:** `GET /api/v1/health-services/emergency-installation-management/emergency-visits?pageNumber=1&pageSize=100&physicianFilter=all`
  - **Status:** **HTTP 200 OK** (`totalData: 196`).
  - Tidak ada penolakan HTTP 403 atau pengalihan rute.
- **Bukti Gambar:** `D2R3-U03-akhir.png` menangkap layar penuh memperlihatkan banner saringan nonaktif, tab Semua pasien aktif, dan daftar kartu pasien IGD terisi normal.
- **Kesimpulan:** **PASS**.

---

### 4. Skenario D2R3-R01 (Billing — Verifikasi Integrasi Kasir Tagihan Pasien IGD)
- **Aktor:** `ranger.biru@admin.com` (dr. Jenah Ali) & `superadmin@admin.com`
- **Subjek Uji:** Pasien `P-X-B` (RM `00-00-02-48`, Encounter `7dab5614-06ba-4d4b-9a6c-b73b6f8bb38b`)
- **Langkah A (Penyelesaian Catatan Dokter):**
  - Catatan dokter `CON-20261021-00002` (ID: `39825086-8902-42e0-aac1-7c0a19679695`) milik dr. Jenah Ali dipersiapkan dengan diagnosa utama valid `R40` (Somnolence, stupor and coma) via `POST /patient-diagnoses` (HTTP 200).
  - Eksekusi finalisasi via `PATCH /doctor-consultations/39825086-8902-42e0-aac1-7c0a19679695/complete` dengan acknowledgement peringatan coverage resep:
    - Status: **HTTP 200 OK**
    - Status Catatan: Menjadi **`Completed`**
    - Resep Terfinalisasi: `finalizedPrescriptionCount: 1` (`RX-20261021-00001`, item Ondansetron/Narfoz Rp 650.000,00)
    - Masalah Handoff Billing: `billingHandoffIssues: []` (sukses tanpa hambatan).
- **Langkah B (Verifikasi Layar Billing Kasir & Rincian Tagihan):**
  1. Halaman Cashier Overview (`/health-services/billing-management/billing/invoices/cashier-overview`) dibuka.
  2. Baris pasien RM `00-00-02-48` (`Pasien Pengganti 1791259153562_563`) terdeteksi di baris ke-9 dengan:
     - Jenis Layanan: `IGD`
     - Tipe Pembayaran: `Tunai`
     - Total Tagihan: `Item berjalan Rp 650.000,00`
     - Sisa Pasien: `Rp 650.000,00` (Perkiraan, belum terkini, Belum Bayar)
     - Bukti tangkapan layar daftar kasir: `r01-billing-list.png`.
  3. Tombol aksi (⋮) diklik -> memilih `Detail Pembayaran` -> diarahkan ke `/detail-billing`.
  4. Tab **`Detail Tagihan`** dibuka untuk melihat tabel **Rincian Tagihan**.
  5. Pengujian memvalidasi dua kriteria penentu:
     - **Baris Resep / Obat (Narfoz / Ondansetron):** **ADA** pada kategori `OBAT / MEDICINE` dengan item `Resep RX-20261021-00001`, Qty `1`, Harga Satuan `Rp 650.000,00`, Subtotal `Rp 650.000,00`.
     - **Baris Jasa Konsultasi Dokter:** **TIDAK ADA** (sesuai invariant IGD pada `ConsultationFinalizationService.cs` baris 223 di mana `consultation.Encounter?.EncounterType == EncounterType.Emergency` tidak membebankan tarif konsultasi dokter rawat jalan ke tagihan pasien).
- **Hasil Pengamatan Jaringan (`jaringan-D2R3-R01.json`):**
  - `GET /api/v1/health-services/billing-management/billing/invoices/87be1cda-9fe5-48c9-bbdd-5448fce4d6fb` HTTP 200 OK (Item count: 1, Gross Amount: 650000).
- **Bukti Gambar:** `D2R3-R01-akhir.png` memperlihatkan tabel Rincian Tagihan di bawah tab Detail Tagihan dengan baris obat `Resep RX-20261021-00001` Rp 650.000,00 dan tidak adanya baris jasa konsultasi dokter.
- **Kesimpulan:** **PASS**.

---

## III. Jawaban Kueri Arsitektur IGD-OQ-139

Untuk menjawab keputusan arsitektur terkait indeks konsultasi rawat jalan vs IGD:
```sql
SELECT COUNT(*) FROM public."TrxDoctorConsultation" c
WHERE c."IsDelete" = false AND c."InpEpisodeId" IS NULL AND c."ClinicId" IS NULL
  AND NOT EXISTS (SELECT 1 FROM public."EmgVisit" v
                  WHERE v."EncounterId" = c."EncounterId" AND v."IsDelete" = false);
```
- **Hasil COUNT:** **`0` (Nol)**.
- **Makna Keputusan:** Tidak ada satu pun baris konsultasi rawat jalan historis tanpa `ClinicId` yang bukan milik kunjungan IGD. Dengan demikian, kriteria **`ClinicId IS NOT NULL` AMAN 100%** dipakai sebagai diskriminator unik rawat jalan tanpa merusak integritas data historis.

---

## IV. Berkas Bukti yang Tersedia di Disk

Seluruh berkas bukti pengujian Putaran D2R-3 tersimpan di:  
`QuilvianSystemFrontendDev/test-with-agy/igd/uji-putaran-d2r3-20261021/`

| Nama Berkas | Ukuran | Deskripsi Faktual |
| :--- | :---: | :--- |
| `kode-awal.txt` | 2,6 KB | Stempel waktu perbaikan frontend `emergency-locked-notes-tab.jsx` (11:01:53) dan DLL backend (09:53:31) |
| `kode-akhir.txt` | 2,6 KB | Verifikasi integritas source code selama pengujian D2R-3 |
| `hasil.json` | 3,3 KB | Registri hasil pengujian 4 skenario (4/4 PASS) dalam format JSON terstruktur |
| `percobaan.json` | 2,9 KB | Log catatan kronologis eksekusi tiap pengujian |
| `data-uji.json` | 2,5 KB | Data referensi pasien P-X-A, P-X-B, P-X-C dan akun uji |
| `jaringan-D2R3-U01.json` | 12,8 KB | Rekaman POST penambahan addendum (HTTP 201 Created) dan GET riwayat |
| `D2R3-U01-akhir.png` | 553 KB | Tangkapan layar modal riwayat addendum dan label tombol `Addendum (1)` |
| `jaringan-D2R3-U02.json` | 2,5 KB | Rekaman POST penolakan kajian awal kedua (HTTP 409 Conflict) |
| `D2R3-U02-akhir.png` | 420 KB | Tangkapan layar halaman penuh memperlihatkan banner merah penolakan HTTP 409 |
| `jaringan-D2R3-U03.json` | 238 KB | Rekaman GET emergency-visits oleh perawat (HTTP 200 OK, 196 pasien) |
| `D2R3-U03-akhir.png` | 451 KB | Tangkapan layar akun perawat dengan banner saringan nonaktif dan daftar pasien terisi |
| `jaringan-D2R3-R01.json` | 1,1 MB | Rekaman GET Cashier Overview & GET detail invoice billing (HTTP 200 OK) |
| `r01-billing-list.png` | 486 KB | Tangkapan layar daftar Cashier Overview dengan baris RM 00-00-02-48 item berjalan Rp 650.000 |
| `D2R3-R01-akhir.png` | 260 KB | Tangkapan layar tabel Rincian Tagihan: obat Ondansetron ADA dan jasa dokter TIDAK ADA |
