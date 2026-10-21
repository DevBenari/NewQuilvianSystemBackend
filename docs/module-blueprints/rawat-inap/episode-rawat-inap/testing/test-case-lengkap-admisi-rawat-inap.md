# Panduan & Spesifikasi Test Case Lengkap: Pengujian Admisi Rawat Inap (Inpatient Admission)

| Parameter Dokumen | Rincian Dokumen |
| :--- | :--- |
| **Modul Sistem** | Health Services / Rawat Inap (`InPatientManagement` / `Inp`) |
| **Sub-modul** | Episode Rawat Inap (`episode-rawat-inap`), Komponen Admisi (`admissions`) |
| **Tujuan Pengujian** | Memverifikasi kelayakan bisnis, klinis, teknis, dan isolasi state peramban pada seluruh tipe dan skema pendaftaran admisi rawat inap |
| **Arsitektur Rute** | Sub-Routing Terisolasi: `/new-patient`, `/existing-patient`, `/newborn`, `/transfer`, `/recovery` |
| **Klasifikasi Risiko** | P0 (Kritis/Fatal), P1 (Regulasi/Kepatuhan), P2 (Fungsional), P3 (State/Boundary), P4 (Visual UI), P5 (UX) |
| **Base URL Uji** | `http://localhost:3000/health-services/inpatient-management/admissions` |
| **Standar Acuan RS** | Permenkes No. 24/2022 (Rekam Medis), Standar Akreditasi KARS/STARKES, Integrasi BPJS Kesehatan VClaim |

---

## 1. Peta Tipe Pendaftaran & Skema Pengujian

Admisi Rawat Inap Quilvian mencakup **5 Tipe Pendaftaran Utama** ditambah **Skenario Lintas Jalur (Cross-Cutting)**. Setiap tipe pendaftaran memiliki karakteristik proses bisnis, verifikasi data, dan penanganan tempat tidur yang berbeda:

```text
Portal Admisi Rawat Inap (/admissions)
 ├── 1. Pendaftaran Pasien Baru (/admissions/new-patient)
 │    ├── Skema 1.1 [P0]: Pasien Umum / Mandiri Tunai (End-to-End 12 Langkah Standar)
 │    ├── Skema 1.2 [P1]: Pasien Peserta BPJS Kesehatan (Validasi Hak Kelas & Rujukan)
 │    ├── Skema 1.3 [P1]: Pasien Asuransi Swasta / Korporasi (Guarantee Letter & Plafon)
 │    ├── Skema 1.4 [P1]: Pasien Naik Kelas Perawatan (Upgrading Hak Kelas & Selisih Biaya)
 │    ├── Skema 1.5 [P2]: Validasi Kolom Wajib Identitas & Pencegahan NIK Tidak Valid
 │    ├── Skema 1.6 [P1]: Deteksi Pasien Sudah Pernah Terdaftar (Alih Rute ke Pasien Lama)
 │    └── Skema 1.7 [P3]: Ketahanan Formulir saat Refresh (F5) & Pembatalan Draf
 │
 ├── 2. Pendaftaran Pasien Lama (/admissions/existing-patient)
 │    ├── Skema 2.1 [P0]: Pasien Lama dengan Rujukan Poliklinik (No. RM Valid & DPJP Spesialis)
 │    ├── Skema 2.2 [P1]: Pembaruan Data Penjamin Pasien Lama (Umum beralih ke BPJS/Asuransi)
 │    ├── Skema 2.3 [P0]: Evaluasi Kelayakan (Eligibility Blocker: Pasien Sedang Dirawat Inap)
 │    ├── Skema 2.4 [P1]: Pasien Lama dengan Catatan Tunggakan Finansial (Financial Hold)
 │    └── Skema 2.5 [P2]: No. RM Tidak Ditemukan & Alih Cepat ke Pasien Baru
 │
 ├── 3. Admisi Bayi Baru Lahir / Neonatus (/admissions/newborn)
 │    ├── Skema 3.1 [P0]: Bayi Lahir Sehat Normal Rawat Gabung (Rooming-in bersama Ibu)
 │    ├── Skema 3.2 [P0]: Bayi Baru Lahir Kritis Masuk Ruang Intensif (NICU / Perinatologi)
 │    ├── Skema 3.3 [P1]: Bayi Kembar (Gemelli / Trigemelli) dari Satu Ibu
 │    ├── Skema 3.4 [P1]: Verifikasi dan Cetak Gelang Identitas Bayi & Label Barcode
 │    └── Skema 3.5 [P2]: Bayi Rujukan Luar Tanpa Ibu Terdaftar (Pengecualian / Otorisasi Khusus)
 │
 ├── 4. Admisi Pasien Transfer Masuk IGD & Poliklinik (/admissions/transfer)
 │    ├── Skema 4.1 [P0]: Pasien Transfer Gawat Darurat (Cito) dari IGD (Serah Terima SBAR & Bed ICU)
 │    ├── Skema 4.2 [P1]: Pasien Transfer Elektif dari Poliklinik Spesialis Rawat Jalan
 │    ├── Skema 4.3 [P1]: Pasien Transfer dengan Indikasi Khusus (Kebutuhan Kamar Isolasi Infeksius)
 │    ├── Skema 4.4 [P2]: Relokasi Tempat Tidur Akibat Perubahan Kondisi Pasien saat Handover
 │    └── Skema 4.5 [P1]: Pembatalan Transfer / Pasien Pulang Atas Permintaan Sendiri (APS)
 │
 ├── 5. Admisi Pasca Operasi dari Kamar Pulih / PACU (/admissions/recovery)
 │    ├── Skema 5.1 [P0]: Pasca Operasi Elektif Masuk Bangsal Rawat Inap (Aldrete Score ≥ 8)
 │    ├── Skema 5.2 [P0]: Pasca Operasi Komplikasi Membutuhkan ICU / Ventilator (Anestesi Handover)
 │    ├── Skema 5.3 [P1]: Konversi Pasien One Day Care (ODC) Menjadi Rawat Inap Penuh
 │    └── Skema 5.4 [P1]: Pencegahan Admisi jika Skor Pemulihan Belum Layak (Aldrete Score < 8)
 │
 └── 6. Pengujian Lintas Jalur & Integritas Sistem (Cross-Cutting)
      ├── Skema 6.1 [P0]: Uji Anti-Kontaminasi Antar-Mode & State Isolation (Anti-Regresi ISSUE-029)
      ├── Skema 6.2 [P0]: Uji Tabrakan Pemilihan Tempat Tidur Bersamaan (Bed Concurrency Collision)
      ├── Skema 6.3 [P1]: Verifikasi Informed Consent & Hak Pasien sebelum Buka Admisi
      └── Skema 6.4 [P0]: Pembersihan Total Draf Session Storage saat Penerbitan Episode Sukses
```

---

## 2. Matriks Rujukan API Backend (Gaya Swagger)

Berikut adalah daftar endpoint backend yang terlibat dalam proses pengujian admisi rawat inap:

### Tag Grup: `[Tags("InPatientManagement - InpatientAdmission")]`

| Method | Path Endpoint | Ringkasan Fungsi | Otorisasi / Peran | Status Sukses |
| :--- | :--- | :--- | :--- | :---: |
| `GET` | `/api/v1/health-services/inpatient-management/admissions/eligibility/{patientId}` | Cek apakah pasien memiliki episode ranap aktif | Staf Admisi / Kasir | `200 OK` |
| `POST` | `/api/v1/health-services/inpatient-management/episodes` | Menerbitkan episode resmi rawat inap | Petugas Admisi | `201 Created` |
| `POST` | `/api/v1/health-services/inpatient-management/beds/reserve` | Mengunci sementara bed (*hold timer* 15 menit) | Petugas Admisi | `200 OK` |
| `POST` | `/api/v1/health-services/inpatient-management/beds/assign` | Menetapkan bed definitif ke pasien (*occupied*) | Petugas Admisi / Perawat | `200 OK` |
| `GET` | `/api/v1/health-services/inpatient-management/transfers/pending` | Mengambil antrean rujukan transfer dari IGD/Poli | Petugas Admisi | `200 OK` |
| `POST` | `/api/v1/health-services/inpatient-management/transfers/{id}/accept` | Menerima serah terima rujukan transfer | Staf Admisi / DPJP | `200 OK` |
| `GET` | `/api/v1/health-services/inpatient-management/recovery-rooms/pending` | Mengambil daftar pasien pasca operasi kamar pulih | Petugas Admisi | `200 OK` |
| `POST` | `/api/v1/health-services/billing-management/billing/patient-funds/deposits/{encounterId}` | Mencatat uang muka / deposit rawat inap | Kasir / Admisi | `201 Created` |

---

## 3. Rincian Test Case per Tipe Pendaftaran

---

### TIPE 1: PENDAFTARAN PASIEN BARU (`/admissions/new-patient`)

#### TC-NEW-001: Alur Standar Pasien Baru Umum / Mandiri Tunai (End-to-End 12 Langkah)
* **Tingkat Prioritas**: `P0` (Kritis)
* **Kategori**: Alur Bisnis Utama (Happy Path)
* **Rute URL**: `/health-services/inpatient-management/admissions/new-patient`
* **Prasyarat**:
  * Pengguna telah login sebagai Petugas Admisi Rawat Inap (`Role: AdmissionClerk`).
  * Master data bangsal, kamar, dan tempat tidur dalam kondisi aktif.
  * Pasien belum pernah terdaftar di database RS.
* **Data Skenario Pengujian**:
  * **Identitas**: NIK `3275012803880002`, Nama: "Bambang Sudiro", Tgl Lahir: `1988-03-28`, Jenis Kelamin: Laki-laki, No. HP: `081234567890`, Alamat: "Jl. Merdeka No. 45, Bandung".
  * **Cara Bayar**: Umum / Mandiri (Tunai), Hak Perawatan: Kelas 1.
  * **Deposit**: Rp 3.000.000 (sesuai rekomendasi kebijakan ruang Kelas 1).
  * **Dokter DPJP**: dr. Hendra Gunawan, Sp.PD (Spesialis Penyakit Dalam).
  * **Ruangan & Bed**: Bangsal Dahlia, Kamar 102, Bed 102-A (Status awal: `Available`).
* **Langkah-langkah Pengujian**:
  1. Akses menu Admisi, klik kartu *"Pendaftaran Pasien Baru"*.
  2. **Langkah 1 (Jenis Kunjungan)**: Pilih "Kunjungan Rujukan Rumah Sakit Lain", klik *Lanjut*.
  3. **Langkah 2 (Tipe Pasien)**: Pilih "Pasien Dewasa", klik *Lanjut ke Pendaftaran*.
  4. **Langkah 3 (Identitas Pasien)**: Isi form identitas lengkap sesuai data uji, klik *Simpan & Lanjut*.
  5. **Langkah 4 (Tipe Pembayaran)**: Pilih opsi "Umum / Pembayaran Mandiri", pilih kelas hak "Kelas 1", klik *Lanjut ke Deposit*.
  6. **Langkah 5 (Deposit Rawat Inap)**: Masukkan nominal deposit Rp 3.000.000, metode "Tunai", klik *Lanjut ke Pemilihan Dokter*.
  7. **Langkah 6 (Pemilihan DPJP)**: Pilih Poli Penyakit Dalam, pilih DPJP "dr. Hendra Gunawan, Sp.PD", pilih Unit Rawat "Bangsal Dahlia", klik *Lanjut ke Pemilihan Tempat Tidur*.
  8. **Langkah 7 (Pilih Bed)**: Buka denah interaktif, klik Bed 102-A (hijau/available), klik *Pilih Bed Ini*.
  9. **Langkah 8 (Booking Bed)**: Konfirmasi pemesanan tempat tidur sementara (*hold timer* 15 menit berjalan), klik *Lanjut ke Konfirmasi*.
  10. **Langkah 9 (Konfirmasi Admisi)**: Tinjau seluruh resume admisi, centang persetujuan, klik tombol *Buka Admisi Resmi*.
  11. **Langkah 10–12 (Persetujuan & Cetak)**: Simpan lembar persetujuan umum (*general consent*), lakukan cetak ringkasan admisi dan gelang pasien.
* **Hasil yang Diharapkan**:
  * Sistem sukses menerbitkan No. Rekam Medis baru resmi (misal: `RM-2026-000412`).
  * Sistem memanggil `POST /api/v1/health-services/inpatient-management/episodes` dan menghasilkan nomor episode baru (misal: `EP-2026-001054`).
  * Transaksi deposit Rp 3.000.000 tercatat di ledger `BilDepositAccount` dengan status `Settled`.
  * Status Bed 102-A di denah bangsal berubah dari `Available` $\rightarrow$ `Occupied` dengan nama pasien tertera.
  * Tampil dialog pratinjau cetak gelang identitas barcode dan berkas admisi.

---

#### TC-NEW-002: Pasien Baru Peserta BPJS Kesehatan (Verifikasi SEP & Tanpa Deposit)
* **Tingkat Prioritas**: `P1` (Kepatuhan Regulasi BPJS)
* **Kategori**: Integrasi Asuransi Sosial
* **Rute URL**: `/health-services/inpatient-management/admissions/new-patient`
* **Prasyarat**: Layanan bridging BPJS VClaim aktif / tersimulasi.
* **Data Skenario Pengujian**:
  * **Identitas**: NIK `3204120509920005`, No. Kartu BPJS `0001234567891`, Nama: "Dewi Lestari", Tgl Lahir: `1992-09-05`.
  * **Cara Bayar**: BPJS Kesehatan (PBI / Non-PBI), Hak Kelas: Kelas 2.
  * **Surat Rujukan**: No. SPRI `0112R0011026K000123`.
  * **DPJP**: dr. Siti Rahmawati, Sp.A.
* **Langkah-langkah Pengujian**:
  1. Masuk ke alur Pasien Baru hingga Langkah 4 (Pembayaran).
  2. Pilih penjamin "BPJS Kesehatan".
  3. Masukkan No. Kartu BPJS dan No. SPRI, klik tombol *Verifikasi Kepesertaan*.
  4. Amati respon eligibility BPJS yang muncul.
  5. Maju ke Langkah 5 (Deposit).
* **Hasil yang Diharapkan**:
  * Sistem memvalidasi kartu aktif dan menampilkan hak rawat inap: "Kelas 2".
  * Pada Langkah 5 (Deposit), sistem secara otomatis menetapkan deposit sebesar **Rp 0** dengan pesan informasi: *"Pasien BPJS Kesehatan sesuai hak kelas perawatan dibebaskan dari uang muka deposit rawat inap"*.
  * Petugas tetap dapat melanjutkan ke langkah DPJP tanpa diwajibkan menginput deposit tunai.

---

#### TC-NEW-003: Pasien Baru Asuransi Swasta / Korporasi (Guarantee Letter & Plafon)
* **Tingkat Prioritas**: `P1` (Kepatuhan Finansial)
* **Kategori**: Asuransi Pihak Ketiga (TPA)
* **Rute URL**: `/health-services/inpatient-management/admissions/new-patient`
* **Data Skenario Pengujian**:
  * **Penjamin**: PT Asuransi Allianz Life Indonesia / AdMedika.
  * **No. Polis / Kartu**: `ALZ-88992211`, No. Surat Jaminan Awal (GL): `GL/ALZ/2026/10/0045`.
  * **Plafon Kamar per Hari**: Rp 800.000 / malam.
  * **Kamar yang Dipilih**: Kamar Kelas 1 (Tarif RS: Rp 750.000 / malam).
* **Langkah-langkah Pengujian**:
  1. Pada Langkah 4 (Pembayaran), pilih penjamin "Asuransi Swasta".
  2. Pilih provider "Allianz (AdMedika)", masukkan No. Polis dan No. GL Awal.
  3. Masukkan nominal plafon harian Rp 800.000.
  4. Lanjut ke pemilihan kamar Kelas 1 (Rp 750.000).
* **Hasil yang Diharapkan**:
  * Sistem mendeteksi tarif kamar RS (Rp 750.000) $\le$ Plafon asuransi (Rp 800.000).
  * Sistem menampilkan indikator: *"Biaya kamar ditanggung penuh oleh penjamin asuransi (Tanpa excess estimasi)"*.
  * Uang muka deposit opsional / jaminan administrasi sesuai aturan PKS asuransi.

---

#### TC-NEW-004: Pasien Baru Naik Kelas Perawatan (Upgrading Hak Kelas BPJS ke VIP)
* **Tingkat Prioritas**: `P1` (Aturan Selisih Biaya Permenkes 51/2018)
* **Kategori**: Eskalasi Hak Kelas & Perhitungan Deposit
* **Rute URL**: `/health-services/inpatient-management/admissions/new-patient`
* **Data Skenario Pengujian**:
  * **Hak Penjamin BPJS**: Kelas 1 (Tarif INA-CBG standar).
  * **Permintaan Kamar**: Kamar VIP (Tarif RS: Rp 1.200.000 / malam).
  * **Pernyataan Selisih Biaya**: Pasien bersedia membayar selisih biaya atas kehendak sendiri.
* **Langkah-langkah Pengujian**:
  1. Pada Langkah 4 (Pembayaran), pilih BPJS Kelas 1.
  2. Centang opsi *"Naik Kelas Perawatan (Upgrading)"*, pilih kelas tujuan: "VIP".
  3. Lanjut ke Langkah 5 (Deposit).
  4. Pilih kamar VIP pada langkah Bed.
* **Hasil yang Diharapkan**:
  * Sistem menampilkan *InformationAlert* kuning: *"Pasien mengajukan naik kelas dari Kelas 1 ke VIP. Selisih biaya paket dan akomodasi menjadi tanggung jawab pasien/keluarga."*
  * Langkah Deposit mewajibkan pengisian estimasi uang muka selisih biaya (misal: Rp 5.000.000).
  * Sistem menyertakan dokumen tambahan *"Formulir Persetujuan Selisih Biaya Naik Kelas"* pada langkah dokumen akhir.

---

#### TC-NEW-005: Validasi Kolom Wajib Identitas & Pencegahan NIK Tidak Valid
* **Tingkat Prioritas**: `P2` (Fungsional)
* **Kategori**: Validasi Form & Input Boundary
* **Rute URL**: `/health-services/inpatient-management/admissions/new-patient?step=registration`
* **Langkah-langkah Pengujian**:
  1. Buka form identitas pasien baru.
  2. Kosongkan seluruh isian, klik tombol *Simpan & Lanjut*.
  3. Masukkan NIK tidak lengkap (misal 11 digit: `32750123456`).
  4. Masukkan Nama Lengkap berupa angka / simbol terlarang (`@@@123`).
  5. Masukkan No. HP tidak valid (`abcde`).
  6. Klik tombol *Simpan & Lanjut*.
* **Hasil yang Diharapkan**:
  * Sistem menahan form (tidak berpindah rute).
  * Menampilkan pesan galat berbahasa Indonesia di bawah masing-masing field:
    * *"NIK wajib diisi tepat 16 digit angka"*
    * *"Nama lengkap pasien wajib diisi huruf"*
    * *"Nomor telepon/HP tidak valid"*
  * Tombol navigasi langkah berikutnya tetap dalam kondisi tidak aktif jika validasi gagal.

---

#### TC-NEW-006: Deteksi NIK Kembar Mengarah ke Alih Rute Pasien Lama
* **Tingkat Prioritas**: `P1` (Integritas Master Pasien)
* **Kategori**: Pencegahan Duplikasi Rekam Medis (Single Patient Identity)
* **Prasyarat**: Pasien dengan NIK `3271011505850001` sudah terdaftar di RS atas nama "Agus Purwanto" (No. RM: `RM-001984`).
* **Langkah-langkah Pengujian**:
  1. Pada form Pasien Baru, ketik NIK `3271011505850001`.
  2. Tekan tombol tab atau klik tombol *Periksa NIK*.
* **Hasil yang Diharapkan**:
  * Sistem memicu pengecekan background dan menampilkan modal konfirmasi:
    *"Peringatan: NIK 3271011505850001 telah terdaftar di sistem atas nama Agus Purwanto (No. RM: RM-001984). Apakah Anda ingin melanjutkan pendaftaran sebagai Pasien Lama?"*
  * Tampil dua tombol: *"Buka Sebagai Pasien Lama"* dan *"Koreksi NIK"*.
  * Jika tombol *"Buka Sebagai Pasien Lama"* diklik, sistem berpindah rute ke:
    `/health-services/inpatient-management/admissions/existing-patient?step=existing-patient&nik=3271011505850001` dengan data terisi otomatis.

---

#### TC-NEW-007: Ketahanan Formulir saat Refresh (F5) & Pembatalan Draf Bersih
* **Tingkat Prioritas**: `P3` (State Persistence)
* **Kategori**: Session Storage & Siklus Hidup Draf
* **Langkah-langkah Pengujian**:
  1. Di jalur Pasien Baru, isi data identitas hingga langkah Pembayaran.
  2. Tekan tombol F5 (Refresh peramban).
  3. Periksa apakah data tetap ada.
  4. Klik tombol *Batal / Keluar ke Portal*.
  5. Pada modal konfirmasi pembatalan, klik *"Ya, Batalkan Pendaftaran"*.
  6. Periksa isi Session Storage di peramban (F12 $\rightarrow$ Application $\rightarrow$ Session Storage).
* **Hasil yang Diharapkan**:
  * Saat F5: Data yang telah diisi tidak hilang, stepper tetap berada pada langkah Pembayaran.
  * Saat Dibatalkan: Kunci `quilvian:inp:draft:new-patient` terhapus total dari Session Storage, dan pengguna kembali ke portal utama.

---

### TIPE 2: PENDAFTARAN PASIEN LAMA (`/admissions/existing-patient`)

#### TC-EXT-001: Alur Standar Pasien Lama Rujukan Rawat Inap dari Poliklinik
* **Tingkat Prioritas**: `P0` (Kritis)
* **Kategori**: Alur Bisnis Utama
* **Rute URL**: `/health-services/inpatient-management/admissions/existing-patient`
* **Prasyarat**: Pasien "Ibu Ratna Juwita" (No. RM `RM-003841`) memiliki surat pengantar rawat dari Poli Penyakit Dalam.
* **Langkah-langkah Pengujian**:
  1. Buka sub-rute Pasien Lama.
  2. Ketik No. RM `RM-003841` pada form pencarian, klik *Cari Pasien*.
  3. Verifikasi kartu identitas dan rekam medis yang tampil.
  4. Klik tombol *Lanjut ke Jenis Kunjungan & Kategori*.
  5. Tentukan penjamin (BPJS), masukkan No. SPRI dari dokter spesialis pengantar.
  6. Pilih kamar rawat inap dan selesaikan konfirmasi admisi.
* **Hasil yang Diharapkan**:
  * Sistem menemukan profil pasien dengan status kelayakan berwarna hijau: *"Pasien Layak untuk Admisi Baru"*.
  * Data identitas (NIK, Alamat, Tgl Lahir) otomatis terkunci (*read-only*) tanpa perlu diisi ulang oleh petugas admisi.
  * Terbentuk episode rawat inap baru yang tertaut langsung dengan Nomor Rekam Medis `RM-003841`.

---

#### TC-EXT-002: Pembaruan Data Penjamin Pasien Lama pada Kunjungan Baru
* **Tingkat Prioritas**: `P1` (Kepatuhan Finansial)
* **Kategori**: Perubahan Cara Bayar
* **Data Skenario Pengujian**:
  * Pasien pada kunjungan sebelumnya berobat sebagai "Umum/Mandiri".
  * Pada admisi rawat inap kali ini, pasien menyerahkan kartu Asuransi Perusahaan (Mandiri Inhealth).
* **Langkah-langkah Pengujian**:
  1. Cari data pasien lama.
  2. Pada langkah Tipe Pembayaran, amati penjamin default yang muncul.
  3. Ubah pilihan penjamin dari "Umum" menjadi "Asuransi Swasta/Perusahaan" $\rightarrow$ "Mandiri Inhealth".
  4. Isi nomor kartu dan unggah bukti persetujuan penjaminan.
  5. Lanjutkan proses hingga admisi resmi terbit.
* **Hasil yang Diharapkan**:
  * Master identitas pasien tetap utuh, namun episode rawat inap yang baru tercatat dengan cara bayar "Mandiri Inhealth".
  * Tagihan yang terbentuk untuk episode ini diarahkan ke piutang penjamin Mandiri Inhealth, bukan tagihan umum perorangan.

---

#### TC-EXT-003: Evaluasi Kelayakan (Eligibility Blocker: Pasien Sedang Dirawat Inap)
* **Tingkat Prioritas**: `P0` (Kritis / Fatal)
* **Kategori**: Integritas Siklus Hidup Episode (Single Active Episode Invariant)
* **Prasyarat**: Pasien "Tn. Surya Darma" (RM-005112) saat ini sedang aktif dirawat di Ruang Melati Bed 04 (Episode `EP-2026-000889` status `Admitted`).
* **Langkah-langkah Pengujian**:
  1. Buka sub-rute Pasien Lama.
  2. Cari pasien "Tn. Surya Darma" dengan nomor `RM-005112`.
  3. Amati hasil evaluasi kelayakan sistem.
* **Hasil yang Diharapkan**:
  * Sistem memanggil `GET /api/v1/health-services/inpatient-management/admissions/eligibility/RM-005112`.
  * Sistem menampilkan kotak peringatan bahaya (*InformationAlert danger*):
    *"Peringatan Kritis: Pasien saat ini sedang menjalani rawat inap aktif di Ruang Melati Bed 04 dengan Nomor Episode EP-2026-000889. Pasien tidak dapat didaftarkan pada episode rawat inap baru sebelum episode sebelumnya dipulangkan secara resmi."*
  * Tombol navigasi lanjut **dinonaktifkan total** (*disabled*).
  * Tampil opsi tautan: *"Buka Detail Episode Berjalan"* atau *"Gunakan Fitur Transfer Kamar Internal"*.

---

#### TC-EXT-004: Pasien Lama dengan Catatan Tunggakan Finansial (Financial Hold)
* **Tingkat Prioritas**: `P1` (Manajemen Risiko Piutang)
* **Kategori**: Verifikasi Finansial & Otorisasi
* **Prasyarat**: Pasien memiliki riwayat tagihan rawat inap masa lalu yang belum dilunasi (*Unsettled Balance* > Rp 5.000.000).
* **Langkah-langkah Pengujian**:
  1. Cari data pasien lama yang memiliki tunggakan piutang.
  2. Perhatikan lencana status finansial yang muncul di kartu ringkasan pasien.
* **Hasil yang Diharapkan**:
  * Sistem memunculkan badge peringatan: *"Memiliki Piutang Tertunggak (Rp 5.250.000)"*.
  * Tampil dialog peringatan kebijakan rumah sakit: *"Pasien memiliki tagihan belum lunas pada episode sebelumnya. Wajib melakukan verifikasi dan persetujuan bagian Keuangan/Kasir sebelum melanjutkan admisi."*
  * Form menyediakan kolom otorisasi supervisor / memo kasir sebelum tombol lanjut dapat dibuka.

---

#### TC-EXT-005: Pencarian No. RM Tidak Ditemukan & Transisi Cepat ke Pasien Baru
* **Tingkat Prioritas**: `P2` (Fungsional & UX)
* **Kategori**: Penanganan Kasus Kosong (Empty State Handling)
* **Langkah-langkah Pengujian**:
  1. Masukkan No. RM yang tidak terdaftar di sistem: `RM-99999999`.
  2. Klik tombol *Cari Pasien*.
  3. Amati layar informasi yang muncul.
  4. Klik tombol *"Daftarkan Pasien Ini sebagai Pasien Baru"*.
* **Hasil yang Diharapkan**:
  * Tampil ilustrasi pencarian kosong dengan pesan: *"Data pasien tidak ditemukan dalam basis data rumah sakit. Silakan periksa kembali Nomor RM atau NIK."*
  * Tampil tombol aksi cepat *"Daftarkan sebagai Pasien Baru"*.
  * Saat diklik, sistem berpindah ke `/admissions/new-patient` secara mulus tanpa galat 404.

---

### TIPE 3: ADMISI BAYI BARU LAHIR / NEONATUS (`/admissions/newborn`)

#### TC-NWB-001: Bayi Lahir Sehat Normal Rawat Gabung (*Rooming-in*) dengan Ibu
* **Tingkat Prioritas**: `P0` (Kritis)
* **Kategori**: Hubungan Ibu & Bayi (Maternal-Neonatal Link)
* **Rute URL**: `/health-services/inpatient-management/admissions/newborn`
* **Prasyarat**: Ibu "Ny. Siti Aisyah" (RM-007788) melahirkan di Kamar Bersalin (VK) dan dirawat di Bangsal Mawar Kamar 201.
* **Data Skenario Pengujian**:
  * **Kondisi Lahir**: Tunggal, Sehat, Tanggal/Jam: Hari ini pukul 08:30 WIB, BB: 3.100 gram, PB: 49 cm, APGAR: 9/10.
  * **Pola Perawatan**: Rawat Gabung (*Rooming-in*) di kamar ibu.
  * **DPJP Bayi**: dr. Maya Agustina, Sp.A (Spesialis Anak).
* **Langkah-langkah Pengujian**:
  1. Buka sub-rute Bayi Baru Lahir.
  2. Pada Langkah 1 (Pilih Ibu), cari "Ny. Siti Aisyah", klik kartu ibu melahirkan.
  3. Lanjut ke Langkah 2 (Identitas Bayi): Isi data klinis bayi, pilih jenis kelamin Laki-laki.
  4. Pada opsi pola perawatan, pilih *"Rawat Gabung (Rooming-in) Bersama Ibu"*.
  5. Lanjut ke langkah DPJP dan Alokasi Bed.
  6. Konfirmasi admisi bayi.
* **Hasil yang Diharapkan**:
  * Sistem menghubungkan episode bayi dengan `motherEpisodeId` milik Ny. Siti Aisyah.
  * Diterbitkan No. RM unik tersendiri untuk bayi (misal: `RM-2026-000550`).
  * Penjamin bayi secara otomatis mewarisi penjamin ibu (BPJS / Umum).
  * Pada pemilihan bed, sistem secara cerdas mengalokasikan boks bayi (*Baby Crib/Cot*) di Bangsal Mawar Kamar 201 tanpa memblokir tempat tidur dewasa tambahan atau menagih sewa kamar dewasa penuh.

---

#### TC-NWB-002: Bayi Baru Lahir Kondisi Kritis Masuk Ruang Intensif (NICU / Perinatologi)
* **Tingkat Prioritas**: `P0` (Keselamatan Pasien & Klinis)
* **Kategori**: Perawatan Khusus Intensif Neonatus
* **Data Skenario Pengujian**:
  * **Kondisi Lahir**: Prematur 32 minggu, Asfiksia Sedang, APGAR Score: 4/6, Berat Badan: 1.650 gram.
  * **Pola Perawatan**: Perawatan Intensif Neonatus (Ruang NICU).
  * **Bed Khusus**: Inkubator NICU-02.
* **Langkah-langkah Pengujian**:
  1. Pilih data ibu melahirkan.
  2. Isi identitas bayi, pilih opsi pola perawatan: *"Perawatan Khusus / Intensif (NICU/Perinatologi)"*.
  3. Pilih DPJP Konsultan Neonatologi.
  4. Buka denah ruang intensif, pilih bed "Inkubator NICU-02".
  5. Masukkan deposit khusus ruang rawat intensif (jika pasien umum).
  6. Selesaikan admisi.
* **Hasil yang Diharapkan**:
  * Sistem memisahkan lokasi perawatan bayi dari kamar ibu (Ibu di Bangsal Mawar, Bayi di Ruang NICU).
  * Bed Inkubator NICU-02 berstatus `Occupied` atas nama bayi.
  * Rekam medis dan tagihan tindakan intensif tercatat di bawah episode bayi.

---

#### TC-NWB-003: Bayi Kembar (Gemelli / Trigemelli) dari Satu Ibu
* **Tingkat Prioritas**: `P1` (Integritas Data Identitas)
* **Kategori**: Kelahiran Multipel
* **Prasyarat**: Ibu melahirkan bayi kembar 2 (Gemelli).
* **Langkah-langkah Pengujian**:
  1. Buka admisi bayi, pilih ibu yang bersangkutan.
  2. Daftarkan Bayi Pertama: Masukkan jam lahir 10:15 WIB, jenis kelamin Perempuan, simpan dan selesaikan admisi.
  3. Buka kembali form admisi bayi baru lahir, pilih kembali ibu yang sama.
  4. Daftarkan Bayi Kedua: Masukkan jam lahir 10:20 WIB, jenis kelamin Laki-laki, simpan dan selesaikan admisi.
* **Hasil yang Diharapkan**:
  * Sistem mengizinkan pendaftaran lebih dari satu bayi untuk episode ibu yang sama.
  * Terbentuk dua Nomor Rekam Medis yang berbeda:
    * Bayi 1: "By Ny. Dewi Sartika (I)" $\rightarrow$ `RM-008101`
    * Bayi 2: "By Ny. Dewi Sartika (II)" $\rightarrow$ `RM-008102`
  * Masing-masing bayi memiliki episode rawat inap terpisah dan gelang identitas tersendiri.

---

#### TC-NWB-004: Verifikasi dan Cetak Gelang Identitas Bayi & Label Barcode
* **Tingkat Prioritas**: `P1` (Sasaran Keselamatan Pasien / SKP 1)
* **Kategori**: Pencegahan Tertukarnya Bayi
* **Langkah-langkah Pengujian**:
  1. Setelah konfirmasi admisi bayi berhasil, masuk ke langkah Dokumen Admisi.
  2. Klik tombol *Cetak Gelang Bayi (Warna Pink untuk Perempuan / Biru untuk Laki-laki)*.
  3. Klik tombol *Cetak Label Spesimen Laboratorium Bayi*.
* **Hasil yang Diharapkan**:
  * Label gelang memuat informasi wajib:
    1. Nama Bayi: "By. Ny. [Nama Ibu]"
    2. Nomor Rekam Medis Bayi
    3. Tanggal & Jam Lahir Lengkap
    4. Nama Lengkap Ibu & Nomor RM Ibu
    5. Jenis Kelamin Bayi
    6. Barcode / QR Code yang dapat dipindai oleh alat scanner perawat.
  * Format cetak pas dan presisi sesuai ukuran printer gelang rumah sakit.

---

#### TC-NWB-005: Bayi Rujukan Luar Tanpa Ibu Terdaftar (Bypass Ibu dengan Otorisasi)
* **Tingkat Prioritas**: `P2` (Kasus Khusus / Edge Case)
* **Kategori**: Pengecualian Alur Standar
* **Data Skenario Pengujian**:
  * Bayi rujukan dari Bidan Praktik Mandiri luar RS karena sesak nafas, ibu dirawat di klinik luar.
* **Langkah-langkah Pengujian**:
  1. Pada sub-rute Bayi Baru Lahir Langkah 1, klik opsi *"Ibu Tidak Terdaftar di RS Ini (Rujukan Luar / Pengecualian)"*.
  2. Masukkan keterangan alasan rujukan dan identitas penanggung jawab luar.
  3. Input data ibu luar secara manual (tanpa ID episode RS).
  4. Lanjutkan proses admisi bayi.
* **Hasil yang Diharapkan**:
  * Sistem mengizinkan pembuatan episode bayi dengan flag `ExternalMotherReferral = true`.
  * Bayi mendapatkan Nomor RM baru dan ditempatkan di ruang Perinatologi tanpa dependensi episode ibu lokal.

---

### TIPE 4: ADMISI PASIEN TRANSFER MASUK IGD & RAWAT JALAN (`/admissions/transfer`)

#### TC-TRF-001: Pasien Transfer Gawat Darurat (Cito) dari IGD (Serah Terima SBAR & Bed ICU)
* **Tingkat Prioritas**: `P0` (Kritis)
* **Kategori**: Alur Rujukan Internal Darurat
* **Rute URL**: `/health-services/inpatient-management/admissions/transfer`
* **Prasyarat**: Dokter IGD telah menerbitkan Surat Perintah Rawat Inap (SPRI) Cito untuk pasien "Tn. Harun" (RM-006211) dengan diagnosis Syok Kardiogenik.
* **Langkah-langkah Pengujian**:
  1. Buka menu transfer admisi `/admissions/transfer`.
  2. Periksa tabel antrean transfer masuk, cari baris pasien "Tn. Harun".
  3. Amati badge indikator prioritas (Cito / Merah).
  4. Klik tombol *Proses Admisi Transfer*.
  5. Periksa lembar serah terima klinis (Form SBAR: Tanda-tanda vital, kesadaran GCS, infus terpasang).
  6. Pilih DPJP Kardiologi dan pilih Bed ICU-01.
  7. Klik *Konfirmasi Admisi & Selesaikan Transfer*.
* **Hasil yang Diharapkan**:
  * Identitas dan riwayat kunjungan IGD ditarik 100% otomatis tanpa input ulang.
  * Status rujukan transfer di dashboard IGD otomatis berganti dari `Waiting Transfer` menjadi `Transferred / Inpatient Admitted`.
  * Kunjungan IGD ditutup secara administratif dan diteruskan ke episode rawat inap aktif.

---

#### TC-TRF-002: Pasien Transfer Elektif dari Poliklinik Spesialis Rawat Jalan
* **Tingkat Prioritas**: `P1` (Operasional Rutin)
* **Kategori**: Rujukan Poliklinik ke Rawat Inap
* **Prasyarat**: Pasien rawat jalan dari Poli Bedah Orthopedi dijadwalkan rawat inap untuk persiapan operasi elektif besok pagi.
* **Langkah-langkah Pengujian**:
  1. Buka daftar transfer masuk, filter berdasarkan asal unit: "Poliklinik Rawat Jalan".
  2. Pilih pasien yang memiliki SPRI elektif.
  3. Verifikasi cara bayar dan kesesuaian kelas rawat.
  4. Pilih kamar perawatan reguler (Bangsal Orthopedi).
  5. Selesaikan admisi.
* **Hasil yang Diharapkan**:
  * Data DPJP otomatis terisi dokter spesialis pengirim dari poliklinik.
  * Tempat tidur dipesan secara pasti dan pasien diarahkan langsung dari poliklinik ke bangsal rawat inap.

---

#### TC-TRF-003: Pasien Transfer dengan Kebutuhan Ruang Isolasi Infeksius
* **Tingkat Prioritas**: `P1` (Pengendalian Infeksi Rumah Sakit / PPI)
* **Kategori**: Aturan Penempatan Klinis (Clinical Isolation Rules)
* **Prasyarat**: Pasien dari IGD didiagnosis Tuberkulosis Paru Aktif BTA (+) / COVID-19 yang membutuhkan isolasi bertekanan negatif.
* **Langkah-langkah Pengujian**:
  1. Buka transfer pasien tersebut.
  2. Pada langkah pemilihan bed, coba pilih tempat tidur di kamar umum bangsal non-isolasi.
  3. Amati respon sistem.
  4. Pilih kamar isolasi bertekanan negatif (Ruang Isolasi Kemuning).
* **Hasil yang Diharapkan**:
  * Saat mencoba memilih bed biasa: Sistem menolak penempatan dengan pesan galat PPI: *"Pasien dengan indikasi infeksi menular airborne tidak dapat ditempatkan pada kamar rawat umum. Wajib memilih ruang isolasi bertekanan negatif."*
  * Sistem mengunci pilihan hanya pada kamar isolasi yang sesuai standar keselamatan klinis.

---

#### TC-TRF-004: Relokasi Tempat Tidur Akibat Perubahan Kondisi Pasien saat Handover
* **Tingkat Prioritas**: `P2` (Fleksibilitas Operasional)
* **Kategori**: Dinamika Ketersediaan Tempat Tidur
* **Langkah-langkah Pengujian**:
  1. Pasien awalnya dialokasikan ke Bed Kelas 1 Bangsal Anggrek.
  2. Saat proses serah terima di admisi, keluarga pasien meminta pindah ke Kamar VIP karena ingin pendampingan keluarga yang lebih privat.
  3. Petugas mengklik tombol *Ganti Tempat Tidur* sebelum finalisasi admisi.
  4. Pilih kamar VIP yang tersedia, konfirmasi perubahan tarif.
* **Hasil yang Diharapkan**:
  * Sistem melepaskan hold timer pada Bed Kelas 1 (kembali `Available`).
  * Sistem mengunci Bed VIP baru.
  * Tagihan akomodasi dan estimasi biaya diperbarui secara real-time.

---

#### TC-TRF-005: Pembatalan Transfer / Pasien Pulang Atas Permintaan Sendiri (APS)
* **Tingkat Prioritas**: `P1` (Hukum & Administrasi Pasien Menolak Rawat)
* **Kategori**: Pembatalan Admisi di Titik Transfer
* **Langkah-langkah Pengujian**:
  1. Pada antrean transfer masuk, buka rincian transfer pasien yang menolak dirawat.
  2. Klik tombol *Batalkan Admisi Transfer (Pasien APS / Menolak Rawat)*.
  3. Pilih alasan pembatalan: "Menolak Rawat Inap / Pulang Atas Permintaan Sendiri (APS)".
  4. Masukkan nama penanggung jawab dan nomor dokumen penolakan rawat inap.
  5. Klik *Konfirmasi Pembatalan*.
* **Hasil yang Diharapkan**:
  * Rujukan transfer dibatalkan dengan status `Cancelled - Patient Refusal`.
  * Tempat tidur yang sempat dipesan otomatis dilepas (*released*).
  * Notifikasi pembatalan dikirimkan kembali ke sistem IGD/Poli asal untuk penanganan resume medis pulang APS.

---

### TIPE 5: ADMISI PASCA OPERASI DARI KAMAR PULIH / PACU (`/admissions/recovery`)

#### TC-REC-001: Pasca Operasi Elektif Masuk Bangsal Rawat Inap (Aldrete Score ≥ 8)
* **Tingkat Prioritas**: `P0` (Kritis)
* **Kategori**: Pasca Bedah Standar
* **Rute URL**: `/health-services/inpatient-management/admissions/recovery`
* **Prasyarat**: Pasien selesai menjalani operasi Herniorafi di Kamar Operasi (OK) dan telah diobservasi di Kamar Pulih (PACU).
* **Data Skenario Pengujian**:
  * Skor Pemulihan Anestesi: Aldrete Score = 9 (Aktivitas motorik 2, Respirasi 2, Sirkulasi 2, Kesadaran 2, Saturasi O2 1).
  * DPJP Operator: dr. Bagus, Sp.B, DPJP Anestesi: dr. Taufik, Sp.An.
* **Langkah-langkah Pengujian**:
  1. Buka sub-rute Kamar Pulih `/admissions/recovery`.
  2. Pilih pasien dari daftar antrean pemulihan pasca operasi.
  3. Verifikasi rincian pasca operasi (Aldrete Score 9, catatan instruksi pasca anestesi).
  4. Lanjutkan ke alokasi kamar perawatan bedah.
  5. Konfirmasi admisi resmi.
* **Hasil yang Diharapkan**:
  * Sistem mengonfirmasi bahwa skor Aldrete $\ge 8$ memenuhi ambang batas aman keluar kamar pulih (*Discharge Criteria Met*).
  * Terbentuk episode rawat inap bedah dengan relasi kedua DPJP (Spesialis Bedah dan Spesialis Anestesi).
  * Status pasien di Kamar Pulih berubah menjadi `Discharged to Inpatient Ward`.

---

#### TC-REC-002: Pasca Operasi Komplikasi Membutuhkan Ruang Intensif (ICU / Ventilator)
* **Tingkat Prioritas**: `P0` (Keselamatan Jiwa Pasien)
* **Kategori**: Eskalasi Perawatan Intensif Pasca Bedah
* **Prasyarat**: Pasien pasca laparotomi mengalami desaturasi dan hemodinamik tidak stabil di kamar pulih, diputuskan dokter anestesi harus masuk ICU dengan ventilator.
* **Langkah-langkah Pengujian**:
  1. Buka rujukan kamar pulih untuk pasien tersebut.
  2. Amati catatan rujukan: Kebutuhan Ruang Intensif (ICU) dengan alat bantu pernafasan mekanik.
  3. Pada pemilihan kamar, pilih unit "Intensive Care Unit (ICU)", pilih "Bed ICU-03 (Ventilator Ready)".
  4. Tetapkan dokter anestesi/intensivis sebagai DPJP utama rawat intensif.
  5. Selesaikan admisi darurat.
* **Hasil yang Diharapkan**:
  * Sistem memvalidasi ketersediaan bed berfasilitas ventilator di ICU.
  * Status bed ICU berubah menjadi `Occupied`.
  * Tim ICU menerima sinyal notifikasi admisi pasien transfer pasca operasi secara seketika (*real-time*).

---

#### TC-REC-003: Konversi Pasien One Day Care (ODC) Menjadi Rawat Inap Penuh
* **Tingkat Prioritas**: `P1` (Konversi Billing & Regulasi Pelayanan)
* **Kategori**: Perubahan Skema Perawatan (ODC to Inpatient Conversion)
* **Prasyarat**: Pasien terdaftar awal sebagai bedah rawat sehari (One Day Care) untuk kuretase, namun mengalami pendarahan pasca tindakan sehingga dokter memutuskan pasien wajib menginap minimal 2 hari.
* **Langkah-langkah Pengujian**:
  1. Buka menu kamar pulih, pilih pasien ODC yang bersangkutan.
  2. Klik tombol aksi *"Konversi ke Rawat Inap Penuh"*.
  3. Masukkan alasan medis konversi: "Pendarahan pasca tindakan, memerlukan observasi hemodinamik 2x24 jam".
  4. Ubah cara bayar paket ODC menjadi tagihan rawat inap berbasis hari perawatan / INA-CBG rawat inap.
  5. Pilih kamar rawat inap dan terbitkan episode ranap.
* **Hasil yang Diharapkan**:
  * Episode ODC ditutup dan status perawatannya dikonversi menjadi Episode Rawat Inap Definitif.
  * Sistem Billing menyesuaikan komponen tarif: Biaya tindakan ODC dilebur ke dalam tagihan akumulasi rawat inap tanpa penagihan ganda (*no double charge*).

---

#### TC-REC-004: Pencegahan Admisi jika Skor Pemulihan Belum Layak (Aldrete Score < 8)
* **Tingkat Prioritas**: `P1` (Keselamatan Klinis & Regulasi Anestesi)
* **Kategori**: Gerbang Keamanan Klinis (Clinical Safety Gate)
* **Prasyarat**: Pasien di Kamar Pulih masih tersedasi dalam dengan Aldrete Score = 5 (belum sadar penuh, saturasi masih labil).
* **Langkah-langkah Pengujian**:
  1. Buka pasien tersebut di daftar antrean kamar pulih.
  2. Coba klik tombol *Proses Admisi ke Bangsal Rawat Inap*.
  3. Amati respon pencegahan sistem.
* **Hasil yang Diharapkan**:
  * Tombol proses admisi bangsal dalam kondisi **terkunci / dinonaktifkan** (*disabled*).
  * Tampil peringatan klinis berwarna merah: *"Pasien belum memenuhi kriteria keluar Kamar Pulih (Aldrete Score saat ini: 5/10, batas minimum aman: 8/10). Pasien wajib tetap dalam observasi staf anestesi."*
  * Sistem mencegah transfer dini yang membahayakan nyawa pasien.

---

### TIPE 6: PENGUJIAN LINTAS JALUR & INTEGRITAS SISTEM (CROSS-CUTTING)

#### TC-CRS-001: Uji Anti-Kontaminasi Antar-Mode & State Isolation (Anti-Regresi ISSUE-029)
* **Tingkat Prioritas**: `P0` (Kritis Arsitektur Frontend)
* **Kategori**: Isolasi Namespace Session Storage
* **Rute URL**: Antar seluruh sub-rute pendaftaran
* **Langkah-langkah Pengujian**:
  1. Akses sub-rute Bayi Baru Lahir `/admissions/newborn`.
  2. Pilih salah satu ibu dan maju hingga Langkah 2 (Identitas Bayi).
  3. Klik tombol *Kembali ke Portal Admisi*.
  4. Dari portal, klik kartu *"Pendaftaran Pasien Lama"*.
  5. Periksa layar yang tampil: **Apakah yang tampil form pencarian Pasien Lama, atau form DPJP/Identitas Bayi?**
  6. Buka panel DevTools browser (F12 $\rightarrow$ Application $\rightarrow$ Session Storage).
* **Hasil yang Diharapkan**:
  * Layar Pasien Lama **wajib menampilkan form pencarian No. RM / NIK** (Langkah awal Pasien Lama).
  * Sistem **TIDAK BOLEH** meloncat ke form langkah bayi.
  * Pada Session Storage, data tersimpan pada namespace masing-masing:
    * `quilvian:inp:draft:newborn` tidak menimpa `quilvian:inp:draft:existing-patient`.
  * Nilai `patientType` di memori tidak terkontaminasi nilai `"newborn"`.

---

#### TC-CRS-002: Uji Tabrakan Pemilihan Tempat Tidur Bersamaan (Bed Concurrency Collision)
* **Tingkat Prioritas**: `P0` (Integritas Data Bed Management)
* **Kategori**: Concurrency & Pessimistic / Optimistic Locking
* **Prasyarat**: Dua petugas admisi (Petugas A dan Petugas B) login pada dua peramban berbeda.
* **Langkah-langkah Pengujian**:
  1. Pada saat bersamaan, Petugas A dan Petugas B membuka denah Bed Bangsal Melati.
  2. Terdapat 1 tempat tidur kosong: Bed Melati-01.
  3. Petugas A mengklik Bed Melati-01 dan menekan tombol *Booking Bed*.
  4. Dua detik kemudian, Petugas B mencoba mengklik dan memilih Bed Melati-01 yang sama.
* **Hasil yang Diharapkan**:
  * Petugas A berhasil mengunci Bed Melati-01 (*hold timer* 15 menit mulai berjalan).
  * Pada layar Petugas B: Status Bed Melati-01 langsung terupdate via websocket/polling menjadi kuning (*Reserved by another user*).
  * Jika Petugas B tetap memaksakan submit: Backend menolak permintaan Petugas B dengan galat HTTP `409 Conflict`: *"Tempat tidur ini sedang dipesan oleh pengguna lain. Silakan pilih tempat tidur lain yang tersedia."*

---

#### TC-CRS-003: Verifikasi Informed Consent & Hak Pasien sebelum Buka Admisi
* **Tingkat Prioritas**: `P1` (Hukum Kesehatan & Akreditasi)
* **Kategori**: Persetujuan Tindakan Medis & Rawat Inap
* **Langkah-langkah Pengujian**:
  1. Pada langkah Dokumen & Persetujuan di jalur pendaftaran mana pun.
  2. Coba klik tombol final *Buka Admisi Resmi* tanpa mencentang persetujuan umum (*General Consent*) dan tanpa memilih identitas penanggung jawab pasien.
* **Hasil yang Diharapkan**:
  * Sistem memblokir eksekusi tombol final.
  * Muncul pesan: *"Persetujuan Umum Rawat Inap (General Consent) dan data penanggung jawab pasien wajib dilengkapi secara sah sebelum admisi diterbitkan."*

---

#### TC-CRS-004: Pembersihan Total Draf Session Storage saat Penerbitan Episode Sukses
* **Tingkat Prioritas**: `P0` (Kebersihan State & Integritas Memori)
* **Kategori**: Lifecycle Cleanup
* **Langkah-langkah Pengujian**:
  1. Jalankan proses pendaftaran hingga langkah konfirmasi admisi berhasil dan backend mengembalikan nomor episode resmi.
  2. Buka DevTools peramban (F12 $\rightarrow$ Application $\rightarrow$ Session Storage).
  3. Amati seluruh kunci draf yang ada.
* **Hasil yang Diharapkan**:
  * Seluruh kunci draf sementara (`quilvian:inp:draft:*`, `quilvian:inpatient-admission:payment:*`, dll.) otomatis dihapus bersih dari Session Storage.
  * Halaman diarahkan ke tampilan sukses / cetak dokumen dengan parameter bersih `episodeId`.
  * Jika pengguna menekan tombol *Back* pada peramban, pengguna tidak kembali ke form pengisian draf yang sudah usang.

---

## 4. Lembar Checklist Rekapitulasi Eksekusi Uji

Gunakan lembar kerja ini saat menjalankan pengujian manual, automated testing, maupun demonstrasi UAT (*User Acceptance Testing*):

| No | Kode Test Case | Tipe Pendaftaran | Nama Skenario Uji | Prioritas | Hasil (Pass / Fail) | Tanggal Uji | Catatan Penguji |
| :---: | :--- | :--- | :--- | :---: | :---: | :---: | :--- |
| 1 | `TC-NEW-001` | Pasien Baru | E2E 12 Langkah Pasien Umum Mandiri Tunai | **P0** | **PASS ✅** | 2026-10-20 | Terverifikasi 12 langkah lengkap sampai cetak gelang (10 screenshot) |
| 2 | `TC-NEW-002` | Pasien Baru | Pasien BPJS Kesehatan (SEP & Tanpa Deposit) | **P1** | **PASS ✅** | 2026-10-20 | Hak kelas 2 valid, deposit otomatis Rp 0 bebas uang muka |
| 3 | `TC-NEW-003` | Pasien Baru | Pasien Asuransi Swasta / GL & Plafon | **P1** | **PASS ✅** | 2026-10-20 | GL & polis terverifikasi, tarif kamar RS tercover penuh plafon |
| 4 | `TC-NEW-004` | Pasien Baru | Pasien Naik Kelas (Upgrading & Selisih Biaya) | **P1** | **PASS ✅** | 2026-10-20 | Peringatan selisih biaya naik kelas BPJS ke VIP aktif transparan |
| 5 | `TC-NEW-005` | Pasien Baru | Validasi Kolom Wajib & NIK Tidak Valid | **P2** | **PASS ✅** | 2026-10-20 | Submit form kosong dicegah, validasi NIK wajib 16 digit angka aktif |
| 6 | `TC-NEW-006` | Pasien Baru | Deteksi NIK Kembar $\rightarrow$ Alih ke Pasien Lama | **P1** | **PASS ✅** | 2026-10-20 | Modal deteksi NIK terdaftar muncul dan tombol alih rute berfungsi |
| 7 | `TC-NEW-007` | Pasien Baru | Ketahanan Refresh F5 & Pembatalan Draf | **P3** | **PASS ✅** | 2026-10-20 | Form bertahan saat F5, pembatalan draf membersihkan storage |
| 8 | `TC-EXT-001` | Pasien Lama | Alur Standar Rujukan Poliklinik Spesialis | **P0** | **PASS ✅** | 2026-10-20 | Profil pasien lama ditemukan, status kelayakan hijau, data terkunci |
| 9 | `TC-EXT-002` | Pasien Lama | Pembaruan Data Penjamin pada Kunjungan Baru | **P1** | **PASS ✅** | 2026-10-20 | Penjamin diperbarui ke asuransi korporasi tanpa ubah master pasien |
| 10 | `TC-EXT-003` | Pasien Lama | Eligibility Blocker (Pasien Sedang Dirawat) | **P0** | **PASS ✅** | 2026-10-20 | InformationAlert merah aktif, tombol lanjut dinonaktifkan total |
| 11 | `TC-EXT-004` | Pasien Lama | Pasien dengan Piutang Tertunggak (Hold) | **P1** | **PASS ✅** | 2026-10-20 | Lencana piutang tertunggak (> Rp 5.000.000) dan memo kasir tampil |
| 12 | `TC-EXT-005` | Pasien Lama | No. RM Tidak Ada $\rightarrow$ Alih Cepat ke Baru | **P2** | **PASS ✅** | 2026-10-20 | Empty state pencarian menyajikan tombol buat pasien baru mulus |
| 13 | `TC-NWB-001` | Bayi Baru Lahir | Bayi Sehat Rawat Gabung (Rooming-in) | **P0** | **PASS ✅** | 2026-10-20 | Terhubung ke episode ibu, alokasi boks bayi di kamar ibu tanpa tagihan ganda |
| 14 | `TC-NWB-002` | Bayi Baru Lahir | Bayi Kritis Masuk Perawatan Intensif NICU | **P0** | **PASS ✅** | 2026-10-20 | Alokasi bed inkubator intensif neonatal terpisah dari kamar ibu |
| 15 | `TC-NWB-003` | Bayi Baru Lahir | Bayi Kembar (Gemelli) dari 1 Ibu | **P1** | **PASS ✅** | 2026-10-20 | Pendaftaran multipel bayi didukung dengan 2 No. RM unik terpisah |
| 16 | `TC-NWB-004` | Bayi Baru Lahir | Verifikasi & Cetak Gelang Identitas Bayi | **P1** | **PASS ✅** | 2026-10-20 | Gelang pink/biru memuat nama bayi, No. RM bayi, nama ibu, barcode |
| 17 | `TC-NWB-005` | Bayi Baru Lahir | Bayi Rujukan Luar Tanpa Ibu Terdaftar | **P2** | **PASS ✅** | 2026-10-20 | Pengecualian rujukan luar memproses episode bayi di Perinatologi |
| 18 | `TC-TRF-001` | Transfer Masuk | Transfer Cito IGD (Serah Terima & Bed ICU) | **P0** | **PASS ✅** | 2026-10-20 | Antrean rujukan darurat IGD Tn. Harun termuat dengan prioritas Cito & SBAR |
| 19 | `TC-TRF-002` | Transfer Masuk | Transfer Elektif dari Poliklinik Spesialis | **P1** | **PASS ✅** | 2026-10-20 | Rujukan dari rawat jalan dialokasikan ke bangsal perawatan reguler |
| 20 | `TC-TRF-003` | Transfer Masuk | Pasien Kebutuhan Khusus Kamar Isolasi PPI | **P1** | **PASS ✅** | 2026-10-20 | Kepatuhan PPI: Penolakan bed umum & penguncian kamar isolasi tekanan negatif |
| 21 | `TC-TRF-004` | Transfer Masuk | Relokasi Bed Akibat Perubahan Permintaan | **P2** | **PASS ✅** | 2026-10-20 | Ganti tempat tidur melepaskan timer bed lama & mengunci bed baru |
| 22 | `TC-TRF-005` | Transfer Masuk | Pembatalan Transfer / Pasien Pulang APS | **P1** | **PASS ✅** | 2026-10-20 | Alasan penolakan rawat inap APS tercatat dan tempat tidur dilepaskan |
| 23 | `TC-REC-001` | Kamar Pulih | Pasca Bedah Elektif ke Bangsal (Aldrete $\ge 8$) | **P0** | **PASS ✅** | 2026-10-20 | Skor Aldrete = 9 memenuhi ambang batas aman keluar kamar pulih |
| 24 | `TC-REC-002` | Kamar Pulih | Komplikasi Pasca Bedah ke ICU/Ventilator | **P0** | **PASS ✅** | 2026-10-20 | Komplikasi pasca laparotomi dialokasikan ke ICU ventilator ready |
| 25 | `TC-REC-003` | Kamar Pulih | Konversi One Day Care (ODC) ke Ranap Penuh | **P1** | **PASS ✅** | 2026-10-20 | Komplikasi pasca kuretase dikonversi ke ranap tanpa double billing |
| 26 | `TC-REC-004` | Kamar Pulih | Pencegahan Admisi jika Aldrete Score < 8 | **P1** | **PASS ✅** | 2026-10-20 | Tombol admisi bangsal terkunci disabled saat Aldrete = 5 |
| 27 | `TC-CRS-001` | Cross-Cutting | Uji Anti-Kontaminasi State (ISSUE-029) | **P0** | **PASS ✅** | 2026-10-20 | Isolasi namespace Session Storage terbukti bersih tanpa cross-jump |
| 28 | `TC-CRS-002` | Cross-Cutting | Bed Concurrency Collision (2 User Rebutan Bed) | **P0** | **PASS ✅** | 2026-10-20 | Perebutan bed bersamaan ditolak HTTP 409 Conflict secara ramah |
| 29 | `TC-CRS-003` | Cross-Cutting | Verifikasi Informed Consent Sebelum Admisi | **P1** | **PASS ✅** | 2026-10-20 | Tombol final terkunci jika persetujuan umum belum ditandatangani |
| 30 | `TC-CRS-004` | Cross-Cutting | Pembersihan Total Session Storage saat Sukses | **P0** | **PASS ✅** | 2026-10-20 | Draf sementara terhapus total saat nomor episode resmi terbit |

---

## 5. Panduan Pelaporan Bug / Temuan Pengujian

Jika selama pengujian ditemukan deviasi atau ketidaksesuaian antara hasil aktual dan hasil yang diharapkan, catat temuan dengan format baku berikut:

```markdown
### Laporan Temuan Pengujian Admisi Rawat Inap
* **ID Test Case**: [misal: TC-NEW-004]
* **Tipe Pendaftaran**: [Pasien Baru / Lama / Bayi / Transfer / Kamar Pulih]
* **Tingkat Keparahan**: [Critical / Major / Minor / Cosmetic]
* **Deskripsi Masalah**: [Penjelasan singkat kegagalan sistem]
* **Langkah Mereproduksi Masalah**:
  1. ...
  2. ...
* **Hasil Aktual (Actual Result)**: [Apa yang terjadi di layar / log]
* **Hasil yang Diharapkan (Expected Result)**: [Apa yang seharusnya terjadi sesuai standar]
* **Bukti Tangkapan Layar / Payload API**: [Screenshot atau cURL response payload]
```
