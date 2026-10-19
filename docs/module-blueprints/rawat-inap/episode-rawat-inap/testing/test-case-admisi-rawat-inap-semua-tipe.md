# Dokumen Skenario & Test Case: Pengujian Komprehensif Seluruh Tipe Pendaftaran Admisi Rawat Inap

| Metadata Dokumen | Keterangan |
| :--- | :--- |
| **Modul** | Health Services / Rawat Inap (`inpatient-management`) |
| **Submodul** | Admisi Rawat Inap (`admissions`) |
| **Arsitektur Pengujian** | Sub-Routing Terstruktur & Isolasi Namespace Session Storage |
| **Klasifikasi Risiko** | P0 (Critical/Fatal), P1 (Compliance), P2 (Functional), P3 (Boundary/State), P4 (Visual UI), P5 (UX) |
| **Target URL** | Base URL: `http://localhost:3000/health-services/inpatient-management/admissions` |
| **Status Dokumen** | Resmi — Siap Dieksekusi untuk QA / Manual Testing / E2E Playwright |

---

## 1. Ringkasan Peta Rute & Tipe Pendaftaran

Sistem Admisi Rawat Inap Quilvian telah direfaktor dari arsitektur *Single Page Monolith* menjadi **Sub-Routing Terstruktur** dengan 5 alur pendaftaran terdedikasi:

```text
/health-services/inpatient-management/admissions/
├── (Portal Utama)       → Halaman pemilihan tipe pendaftaran & indikator antrean
├── new-patient          → Jalur 1: Pendaftaran Pasien Baru (12 langkah)
├── existing-patient     → Jalur 2: Pendaftaran Pasien Lama (Pencarian No. RM/NIK)
├── newborn              → Jalur 3: Admisi Bayi Baru Lahir (Tautan kasus ibu & VK)
├── transfer             → Jalur 4: Admisi Transfer Masuk dari IGD & Poliklinik
└── recovery             → Jalur 5: Admisi Pasien Pasca Operasi dari Kamar Pulih
```

---

## 2. Matriks Prioritas Pengujian

| Tingkat | Kategori | Dampak Kegagalan | Cakupan |
| :---: | :--- | :--- | :--- |
| **P0** | **Critical / Fatal** | Alur admisi macet total, data pasien tertukar, kamar/bed ganda (*conflict*), atau nomor episode gagal terbit. | Pembentukan episode backend, pelepasan hold bed, validasi DPJP. |
| **P1** | **Business & Clinical Compliance** | Pelanggaran regulasi RS (DPJP tidak dipilih, hak kelas penjamin salah, informed consent tidak tersimpan). | Verifikasi eligibility, aturan penjamin, informed consent ranap. |
| **P2** | **Functional & Workflow** | Formulir tidak bisa maju, tombol kembali salah tujuan, filter rujukan gagal memuat. | Validasi form, urutan stepper, filter dropdown, modal dialog. |
| **P3** | **Boundary & State Isolation** | Draf sesi bocor antar-mode (regresi ISSUE-029), refresh (F5) menghilangkan data, multi-tab corrupt. | `sessionStorage` namespace, rehidrasi F5, unmount state. |
| **P4** | **Visual & Responsive** | Tata letak kartu patah, stepper tumpang tindih, badge status tidak presisi. | Design tokens, status badge, CSS grid kartu, mobile/tablet layout. |
| **P5** | **UI / UX Usability** | Petugas admisi bingung, konfirmasi pembatalan tidak informatif, navigasi lambat. | Salinan bahasa Indonesia, breadcrumb, kejelasan pesan alert. |

---

## 3. Daftar Skenario Uji (Test Cases)

---

### GRUP A: Portal Utama & Navigasi Sub-Routing

#### TC-ADM-001: Verifikasi 5 Kartu Pilihan Tipe Pendaftaran di Portal Utama
* **Prioritas**: `P2`
* **Rute URL**: `/health-services/inpatient-management/admissions`
* **Prasyarat**: Petugas admisi telah login dan memiliki hak akses menu Rawat Inap.
* **Langkah Pengujian**:
  1. Buka URL `/health-services/inpatient-management/admissions`.
  2. Perhatikan komponen Hero di bagian atas layar.
  3. Periksa daftar kartu pilihan pendaftaran yang tampil.
* **Data Uji**: Tidak ada input data.
* **Hasil yang Diharapkan**:
  * Hero menampilkan judul *"Admisi Rawat Inap"* dengan eyebrow *"Health Services / Rawat Inap"*.
  * Tampil tepat 5 kartu pilihan pendaftaran:
    1. *Pendaftaran Pasien Baru* (Badge: "PASIEN BELUM TERDAFTAR")
    2. *Pendaftaran Pasien Lama* (Badge: "PASIEN SUDAH TERDAFTAR")
    3. *Admisi Bayi Baru Lahir* (Badge: "BAYI BARU LAHIR")
    4. *Admisi Transfer IGD & Poliklinik* (Badge: "PASIEN TRANSFER IGD / POLIKLINIK")
    5. *Admisi dari Kamar Pulih* (Badge: "PASIEN PASCA OPERASI")
  * Pada kartu Transfer dan Kamar Pulih, tampil jumlah antrean rujukan aktif jika ada data di server.

---

#### TC-ADM-002: Navigasi Sub-Rute Bersih Saat Kartu Diklik (Tanpa Residu Query)
* **Prioritas**: `P1`
* **Rute URL**: `/health-services/inpatient-management/admissions`
* **Prasyarat**: Berada di halaman portal utama.
* **Langkah Pengujian**:
  1. Klik kartu *"Pendaftaran Pasien Baru"*.
  2. Amati URL pada bilah peramban (browser address bar).
  3. Klik tombol *"Kembali"* di sudut kiri bawah layar.
  4. Klik kartu *"Pendaftaran Pasien Lama"*.
  5. Amati kembali URL pada bilah peramban.
* **Hasil yang Diharapkan**:
  * Langkah 1: Peramban berpindah secara fisik ke `/health-services/inpatient-management/admissions/new-patient?step=visit-type`.
  * Langkah 3: Peramban kembali ke portal `/health-services/inpatient-management/admissions`.
  * Langkah 4: Peramban berpindah ke `/health-services/inpatient-management/admissions/existing-patient?step=existing-patient`.
  * Tidak ada query `entry=new` atau `entry=existing` yang menumpuk di URL.

---

#### TC-ADM-003: Pengalihan Otomatis (*Redirect Backward Compatibility*) dari URL Legacy
* **Prioritas**: `P2`
* **Rute URL**: `/health-services/inpatient-management/admissions?entry=new`
* **Prasyarat**: Pengguna mengetik atau membuka link lama/bookmark.
* **Langkah Pengujian**:
  1. Masukkan URL `/health-services/inpatient-management/admissions?entry=new&step=payment` di browser address bar dan tekan Enter.
* **Hasil yang Diharapkan**:
  * Sistem secara transparan mengalihkan (*replace*) URL ke:
    `/health-services/inpatient-management/admissions/new-patient?entry=new&step=payment`
  * Layar langsung merender tahapan Pembayaran Pasien Baru tanpa galat 404.

---

### GRUP B: Jalur 1 — Pendaftaran Pasien Baru (`/admissions/new-patient`)

#### TC-ADM-010: Alur Lengkap Pendaftaran Pasien Baru (End-to-End 12 Langkah)
* **Prioritas**: `P0`
* **Rute URL**: `/health-services/inpatient-management/admissions/new-patient`
* **Prasyarat**: Pasien belum pernah terdaftar di rumah sakit (belum punya Nomor Rekam Medis).
* **Langkah Pengujian**:
  1. **Langkah 1 (Jenis Kunjungan)**: Pilih jenis kunjungan (misal: "Rujukan Dokter / RS Lain"), klik *Lanjut*.
  2. **Langkah 2 (Tipe Pasien)**: Pilih kategori pasien (misal: "Umum / Dewasa"), klik *Lanjut ke Pendaftaran*.
  3. **Langkah 3 (Pendaftaran)**: Isi form identitas pasien baru (NIK, Nama Lengkap, Tempat/Tgl Lahir, Jenis Kelamin, No. HP, Alamat lengkap), klik *Simpan & Lanjut*.
  4. **Langkah 4 (Pembayaran)**: Pilih kategori pembayaran (misal: "Tunai / Umum"), tentukan kelas hak perawatan (Kelas 1), klik *Lanjut ke Deposit*.
  5. **Langkah 5 (Deposit)**: Masukkan nominal uang muka (Rp 2.000.000), klik *Lanjut ke Pemilihan Dokter*.
  6. **Langkah 6 (Dokter / DPJP)**: Pilih Dokter Penanggung Jawab Pelayanan (DPJP) utama dan unit ruang perawatan (Ruang Dahlia), klik *Lanjut ke Pemilihan Tempat Tidur*.
  7. **Langkah 7 (Pilih Bed)**: Buka denah bangsal, pilih bed kosong yang berstatus *Available*, klik *Lanjut ke Booking Bed*.
  8. **Langkah 8 (Booking Bed)**: Konfirmasi pemesanan tempat tidur sementara (*hold timer* 15 menit berjalan), klik *Lanjut ke Konfirmasi*.
  9. **Langkah 9 (Konfirmasi)**: Tinjau seluruh ringkasan admisi, klik *Buka Admisi Resmi*.
  10. **Langkah 10–12 (Persetujuan & Cetak)**: Simpan formulir persetujuan rawat inap, cetak dokumen admisi & kartu pasien.
* **Data Uji**:
  * NIK: `3275012304900001` (16 digit acak belum terdaftar)
  * Nama: "Ahmad Subagyo"
  * Tanggal Lahir: `1990-04-23`
  * Jenis Kelamin: Laki-laki
* **Hasil yang Diharapkan**:
  * Di Langkah 3: Pasien baru tersimpan ke database, diterbitkan No. Rekam Medis resmi (misal `RM-2026-XXXXX`).
  * Di Langkah 9: Endpoint `POST /api/v1/.../episodes` berhasil dieksekusi dengan status HTTP 200/201, menerbitkan `EpisodeNumber` unik.
  * Tempat tidur berubah status dari *Available* $\rightarrow$ *Reserved* $\rightarrow$ *Occupied*.
  * Seluruh dokumen dapat dicetak (*preview PDF*).

---

#### TC-ADM-011: Validasi Kolom Wajib Identitas & Pencegahan NIK Tak Sah
* **Prioritas**: `P1`
* **Rute URL**: `/health-services/inpatient-management/admissions/new-patient?step=registration`
* **Langkah Pengujian**:
  1. Di langkah Pendaftaran Pasien Baru, biarkan kolom NIK dan Nama kosong.
  2. Klik tombol *Simpan & Lanjut*.
  3. Masukkan NIK tidak lengkap (misal hanya 12 digit: `327501230490`).
  4. Klik kembali tombol *Simpan & Lanjut*.
* **Hasil yang Diharapkan**:
  * Sistem menolak submit dan menampilkan pesan validasi berbahasa Indonesia di bawah masing-masing kolom: *"NIK wajib diisi 16 digit"*, *"Nama lengkap pasien wajib diisi"*.
  * Halaman tidak berpindah ke langkah Pembayaran.

---

#### TC-ADM-012: Pengalihan ke Pasien Lama jika NIK Terdeteksi Sudah Terdaftar
* **Prioritas**: `P1`
* **Rute URL**: `/health-services/inpatient-management/admissions/new-patient?step=registration`
* **Prasyarat**: Pasien dengan NIK `3201010101900001` sudah ada di database sebagai "Budi Santoso" (RM-001234).
* **Langkah Pengujian**:
  1. Masukkan NIK `3201010101900001` pada form pendaftaran pasien baru.
  2. Tekan tombol *Periksa NIK* atau scan KTP.
* **Hasil yang Diharapkan**:
  * Muncul peringatan informasi: *"Pasien dengan NIK ini sudah terdaftar atas nama Budi Santoso (No. RM: RM-001234)"*.
  * Tampil tombol aksi: *"Gunakan Pasien Lama Ini"*.
  * Saat tombol diklik, sistem berpindah rute ke:
    `/health-services/inpatient-management/admissions/existing-patient?step=existing-patient&nik=3201010101900001`
  * Form Pasien Lama langsung menampilkan data Budi Santoso tanpa perlu input ulang.

---

#### TC-ADM-013: Ketahanan Data Formulir saat Refresh Browser (F5 Persistence)
* **Prioritas**: `P3`
* **Rute URL**: `/health-services/inpatient-management/admissions/new-patient?step=payment`
* **Langkah Pengujian**:
  1. Petugas mengisi identitas pasien baru di Langkah 3, lalu maju ke Langkah 4 (Pembayaran).
  2. Pilih penjamin "BPJS Kesehatan" dan kelas "Kelas 2".
  3. Tekan tombol F5 (Refresh) pada peramban.
* **Hasil yang Diharapkan**:
  * Halaman memuat ulang di `/health-services/inpatient-management/admissions/new-patient?step=payment`.
  * Hook merehidrasi draf dari `sessionStorage` (`quilvian:inp:draft:new-patient`).
  * Pilihan cara bayar BPJS Kesehatan dan Kelas 2 tetap terpilih dan utuh, tidak kembali kosong atau me-reset ke langkah 1.

---

### GRUP C: Jalur 2 — Pendaftaran Pasien Lama (`/admissions/existing-patient`)

#### TC-ADM-020: Pencarian Pasien Berdasarkan No. RM dan NIK
* **Prioritas**: `P1`
* **Rute URL**: `/health-services/inpatient-management/admissions/existing-patient`
* **Langkah Pengujian**:
  1. Masukkan No. RM yang valid pada kotak pencarian (misal `RM-004521`).
  2. Tekan tombol *Cari Pasien*.
  3. Verifikasi kartu identitas yang tampil.
* **Hasil yang Diharapkan**:
  * Tampil kartu identitas pasien memuat: No. RM, NIK, Nama Lengkap, Tanggal Lahir, Usia, Jenis Kelamin, dan Alamat.
  * Tampil indikator kelayakan admisi (*Eligibility Badge*): "Siap Melanjutkan Admisi" (warna hijau).
  * Tombol *Lanjut ke Jenis Kunjungan & Kategori* menjadi aktif (*enabled*).

---

#### TC-ADM-021: Evaluasi Kelayakan Pasien (Eligibility Gate — Pasien Sedang Dirawat)
* **Prioritas**: `P0`
* **Rute URL**: `/health-services/inpatient-management/admissions/existing-patient`
* **Prasyarat**: Pasien "Siti Aminah" saat ini sedang aktif dirawat inap di Ruang Mawar Bed 02 (Episode berstatus `Admitted`).
* **Langkah Pengujian**:
  1. Cari pasien "Siti Aminah" menggunakan No. RM-nya.
  2. Amati panel pemeriksaan kelayakan yang muncul.
* **Hasil yang Diharapkan**:
  * Tampil *InformationAlert* varian `danger` berwarna merah: *"Pasien ini sedang menjalani rawat inap aktif di Ruang Mawar Bed 02 (Episode: EP-2026-0089). Pasien tidak dapat didaftarkan rawat inap baru."*
  * Tombol navigasi lanjut **dinonaktifkan** (*disabled*).
  * Tampil tautan bantuan: *"Lihat Detail Episode Rawat Inap"* atau *"Gunakan Alur Transfer Internal"*.

---

#### TC-ADM-022: Transisi Cepat ke Pasien Baru jika No. RM Tidak Ditemukan
* **Prioritas**: `P2`
* **Rute URL**: `/health-services/inpatient-management/admissions/existing-patient`
* **Langkah Pengujian**:
  1. Masukkan No. RM acak yang tidak ada di sistem (misal `RM-999999`).
  2. Tekan tombol *Cari Pasien*.
  3. Perhatikan pesan kosong dan tombol bantuan.
  4. Klik tombol *"Daftarkan sebagai Pasien Baru"*.
* **Hasil yang Diharapkan**:
  * Tampil pesan: *"Pasien tidak ditemukan. Pastikan No. RM atau NIK yang Anda masukkan sudah benar."*
  * Tampil tombol *"Daftarkan sebagai Pasien Baru"*.
  * Saat diklik, sistem berpindah rute secara mulus ke:
    `/health-services/inpatient-management/admissions/new-patient`

---

### GRUP D: Jalur 3 — Admisi Bayi Baru Lahir (`/admissions/newborn`)

#### TC-ADM-030: Pencarian dan Penautan Episode Persalinan Ibu (VK / Kamar Operasi)
* **Prioritas**: `P0`
* **Rute URL**: `/health-services/inpatient-management/admissions/newborn`
* **Prasyarat**: Ibu "Ny. Dewi Sartika" baru melahirkan di Kamar Bersalin VK dan memiliki episode ranap aktif.
* **Langkah Pengujian**:
  1. Buka rute `/health-services/inpatient-management/admissions/newborn`.
  2. Pada Langkah 1 (Pilih Ibu Persalinan), cari nama "Ny. Dewi Sartika" atau pilih dari daftar kandidat ibu melahirkan.
  3. Klik kartu ibu tersebut.
* **Hasil yang Diharapkan**:
  * Sistem mengunci `motherEpisodeId` milik Ny. Dewi Sartika.
  * Tampil rincian persalinan ibu: Jam persalinan, DPJP Obgyn penolong persalinan, dan ruangan ibu saat ini.
  * Tombol *Lanjut ke Identitas Bayi* menjadi aktif.

---

#### TC-ADM-031: Pengisian Identitas Bayi & Penentuan Pola Perawatan (Rawat Gabung vs Neonatus)
* **Prioritas**: `P1`
* **Rute URL**: `/health-services/inpatient-management/admissions/newborn?step=newborn-identity`
* **Langkah Pengujian**:
  1. Isi data identitas bayi: Nama Bayi (misal: "Bayi Ny. Dewi Sartika"), Jenis Kelamin (Laki-laki), Tanggal/Jam Lahir, Berat Badan (3.200 gram), Panjang Badan (50 cm), Skor APGAR (9/10).
  2. Tentukan jenis perawatan: Pilih *"Rawat Gabung (Rooming-in) bersama Ibu"*.
  3. Klik *Lanjut ke Tim Medis*.
* **Hasil yang Diharapkan**:
  * Sistem secara otomatis mewarisi penjamin dan kelas perawatan ibu ke bayi.
  * Pada langkah DPJP, form terfokus pada pemilihan DPJP Spesialis Anak (Pediatri).
  * Pada langkah Bed, sistem menawarkan opsi penempatan tempat tidur bayi (*Crib*) di kamar ibu tanpa menagihkan kamar dewasa terpisah.

---

#### TC-ADM-032: Cetak Gelang Identitas Bayi & Dokumen Admisi
* **Prioritas**: `P1`
* **Rute URL**: `/health-services/inpatient-management/admissions/newborn?step=newborn-documents`
* **Langkah Pengujian**:
  1. Selesaikan konfirmasi admisi bayi.
  2. Pada langkah Dokumen, klik tombol *Cetak Gelang Bayi* dan *Cetak Label Identitas*.
* **Hasil yang Diharapkan**:
  * Gelang bayi mencantumkan: Nama Bayi, No. RM Bayi, Nama Ibu ("Ibu: Ny. Dewi Sartika"), Tanggal/Jam Lahir, Jenis Kelamin, serta Barcode/QR Code yang valid.
  * Terbuka jendela cetak peramban (*Print Preview*).

---

### GRUP E: Jalur 4 — Admisi Transfer IGD & Poliklinik (`/admissions/transfer`)

#### TC-ADM-040: Tampilan Daftar Antrean Rujukan Transfer Berstatus Confirmed
* **Prioritas**: `P1`
* **Rute URL**: `/health-services/inpatient-management/admissions/transfer`
* **Prasyarat**: Pasien "Tn. Hendra" di IGD telah dibuatkan surat permintaan rawat inap (SPRI) oleh dokter IGD dan berstatus `Confirmed`.
* **Langkah Pengujian**:
  1. Buka rute `/health-services/inpatient-management/admissions/transfer`.
  2. Periksa tabel antrean transfer masuk.
* **Hasil yang Diharapkan**:
  * Baris pasien "Tn. Hendra" tampil dalam tabel dengan badge asal *"IGD"*, diagnosa kerja, dokter perujuk, dan indikator prioritas (*Cito* / Rutin).
  * Tampil tombol aksi *"Proses Admisi Ranap"*.

---

#### TC-ADM-041: Tinjauan Serah Terima & Pemilihan Bed Pasien Transfer
* **Prioritas**: `P0`
* **Rute URL**: `/health-services/inpatient-management/admissions/transfer`
* **Langkah Pengujian**:
  1. Klik *Proses Admisi Ranap* pada pasien "Tn. Hendra".
  2. Tinjau form serah terima klinis (Tanda-tanda vital asal IGD, alat medis terpasang, catatan obat).
  3. Lanjut ke pemilihan bed dan konfirmasi admisi ranap.
* **Hasil yang Diharapkan**:
  * Data identitas dan penjamin langsung terisi otomatis dari data kunjungan IGD (tidak perlu input ulang).
  * DPJP rawat inap yang ditunjuk menerima transfer pasien.
  * Setelah admisi dikonfirmasi, status rujukan transfer di IGD otomatis berubah menjadi `Completed`, dan pasien resmi terdaftar di Rawat Inap.

---

### GRUP F: Jalur 5 — Admisi Kamar Pulih Pasca Operasi (`/admissions/recovery`)

#### TC-ADM-050: Deteksi Permintaan Rawat Inap dari Kamar Pulih (PACU/RR)
* **Prioritas**: `P1`
* **Rute URL**: `/health-services/inpatient-management/admissions/recovery`
* **Prasyarat**: Pasien pasca operasi bedah digestif dinyatakan dokter anestesi memerlukan ruang ICU/HCU atau ruang rawat biasa.
* **Langkah Pengujian**:
  1. Buka rute `/health-services/inpatient-management/admissions/recovery`.
  2. Pilih rujukan pasien yang bersangkutan dari daftar kamar pulih.
  3. Lanjutkan ke langkah penjamin dan kamar rawat inap.
* **Hasil yang Diharapkan**:
  * Rincian tindakan operasi, jam keluar OK, dan skor pemulihan (Aldrete Score) terbaca di panel informasi.
  * DPJP Anestesi dan DPJP Bedah otomatis tertaut.
  * Selesai admisi, kamar perawatan intensif/bangsal langsung terisi atas nama pasien tersebut.

---

### GRUP G: Pengujian Keandalan, Isolasi State & Session Storage (Anti-Regresi ISSUE-029)

#### TC-ADM-060: Uji Anti-Kontaminasi State Antar-Mode (Regresi ISSUE-029)
* **Prioritas**: `P0` *(Kritis — Memverifikasi masalah lama tidak terulang)*
* **Rute URL**: Antar-sub-rute
* **Langkah Pengujian**:
  1. Buka `/health-services/inpatient-management/admissions`.
  2. Klik kartu *"Admisi Bayi Baru Lahir"* $\rightarrow$ masuk ke `/admissions/newborn`.
  3. Klik salah satu ibu dan maju ke Langkah 2 Bayi.
  4. Klik tombol *Kembali* hingga sampai ke portal utama `/admissions`.
  5. Sekarang klik kartu *"Pendaftaran Pasien Lama"*.
  6. Periksa layar yang tampil: **Apakah yang tampil form pencarian Pasien Lama, atau form DPJP Bayi?**
  7. Buka DevTools peramban (F12) $\rightarrow$ tab *Application* $\rightarrow$ *Session Storage*.
* **Hasil yang Diharapkan**:
  * Layar Pasien Lama **wajib menampilkan pencarian No. RM / NIK** (Langkah 1 dari Pasien Lama).
  * **TIDAK BOLEH** meloncat ke Langkah 3 Bayi (Perawatan dan DPJP Bayi).
  * Pada Session Storage, kunci `quilvian:inp:draft:newborn` tidak mencemari `quilvian:inp:draft:existing-patient`.
  * Nilai `patientType` di memori adalah string kosong / sesuai mode Pasien Lama, **bukan `"newborn"`**.

---

#### TC-ADM-061: Uji Isolasi Multi-Tab Peramban Secara Bersamaan
* **Prioritas**: `P1`
* **Prasyarat**: Menggunakan 1 jendela peramban yang sama.
* **Langkah Pengujian**:
  1. Di **Tab 1**: Buka `/health-services/inpatient-management/admissions/new-patient` dan ketik nama pasien "Tn. Bambang Hermawan".
  2. Buka **Tab 2** (tab baru): Buka `/health-services/inpatient-management/admissions/existing-patient` dan cari pasien "Ny. Sri Wahyuni".
  3. Kembali ke **Tab 1** dan refresh halaman (F5).
  4. Periksa apakah nama "Tn. Bambang Hermawan" tetap ada di Tab 1.
  5. Kembali ke **Tab 2** dan periksa apakah nama "Ny. Sri Wahyuni" tetap ada di Tab 2.
* **Hasil yang Diharapkan**:
  * Data di Tab 1 tidak tertimpa oleh data di Tab 2.
  * Kedua tab beroperasi secara independen tanpa *race condition* atau data collision.

---

#### TC-ADM-062: Uji Pembatalan Draf (*Reset Confirmation Dialog*) & Pembersihan Storage
* **Prioritas**: `P2`
* **Rute URL**: `/health-services/inpatient-management/admissions/new-patient`
* **Langkah Pengujian**:
  1. Di halaman Pasien Baru, isi beberapa kolom formulir.
  2. Klik tombol *Batal / Kembali ke Portal*.
  3. Pada modal konfirmasi *"Batalkan Pendaftaran?"*, klik tombol *"Ya, Batalkan"*.
  4. Buka DevTools peramban $\rightarrow$ *Session Storage*.
* **Hasil yang Diharapkan**:
  * Modal konfirmasi tertutup.
  * Peramban kembali ke portal `/health-services/inpatient-management/admissions`.
  * Draf pada `quilvian:inp:draft:new-patient` telah dihapus (*cleaned up*).
  * Jika membuka kembali `/new-patient`, formulir dalam keadaan kosong bersih.

---

#### TC-ADM-063: Uji Pembersihan Total Storage saat Admisi Berhasil Diterbitkan
* **Prioritas**: `P0`
* **Langkah Pengujian**:
  1. Jalankan proses admisi hingga selesai sampai tombol *Buka Admisi Resmi* berhasil dieksekusi.
  2. Periksa isi *Session Storage* di peramban.
* **Hasil yang Diharapkan**:
  * Seluruh kunci draf sementara (`quilvian:inp:draft:*`, `quilvian:inpatient-admission:payment:*`, `quilvian:inpatient-admission:deposit:*`) otomatis dihapus dari *Session Storage*.
  * Yang tersisa di URL hanyalah `episodeId` resmi untuk pencetakan dokumen admisi.

---

## 4. Lembar Hasil Eksekusi Uji (Checklist Template)

| ID Uji | Skenario | Prioritas | Tipe Pendaftaran | Hasil Uji (Pass / Fail) | Catatan Penguji |
| :--- | :--- | :---: | :--- | :---: | :--- |
| **TC-ADM-001** | Verifikasi 5 Kartu Pilihan Tipe | P2 | Portal Utama | [ ] | |
| **TC-ADM-002** | Navigasi Sub-Rute Bersih | P1 | Portal Utama | [ ] | |
| **TC-ADM-003** | Redirect Backward Compatibility | P2 | Portal Utama | [ ] | |
| **TC-ADM-010** | Alur E2E Pasien Baru (12 Langkah) | P0 | Pasien Baru | [ ] | |
| **TC-ADM-011** | Validasi Kolom Wajib Identitas | P1 | Pasien Baru | [ ] | |
| **TC-ADM-012** | Deteksi NIK Kembar $\rightarrow$ Pasien Lama | P1 | Pasien Baru | [ ] | |
| **TC-ADM-013** | Ketahanan Form saat F5 Refresh | P3 | Pasien Baru | [ ] | |
| **TC-ADM-020** | Pencarian No. RM / NIK | P1 | Pasien Lama | [ ] | |
| **TC-ADM-021** | Eligibility Gate (Pasien Sedang Dirawat) | P0 | Pasien Lama | [ ] | |
| **TC-ADM-022** | Pasien Tidak Ditemukan $\rightarrow$ Pasien Baru | P2 | Pasien Lama | [ ] | |
| **TC-ADM-030** | Penautan Episode Persalinan Ibu | P0 | Bayi Baru Lahir | [ ] | |
| **TC-ADM-031** | Identitas Bayi & Rawat Gabung | P1 | Bayi Baru Lahir | [ ] | |
| **TC-ADM-032** | Cetak Gelang Identitas Bayi | P1 | Bayi Baru Lahir | [ ] | |
| **TC-ADM-040** | Antrean Transfer IGD/Poli Confirmed | P1 | Transfer IGD/Poli | [ ] | |
| **TC-ADM-041** | Serah Terima & Pemilihan Bed Transfer | P0 | Transfer IGD/Poli | [ ] | |
| **TC-ADM-050** | Deteksi Rujukan Kamar Pulih Pasca Operasi | P1 | Kamar Pulih | [ ] | |
| **TC-ADM-060** | Anti-Kontaminasi State (Regresi ISSUE-029) | P0 | Isolasi State | [ ] | |
| **TC-ADM-061** | Isolasi Multi-Tab Bersamaan | P1 | Isolasi Storage | [ ] | |
| **TC-ADM-062** | Pembatalan Draf & Reset Storage | P2 | Siklus Draf | [ ] | |
| **TC-ADM-063** | Pembersihan Total saat Admisi Selesai | P0 | Siklus Draf | [ ] | |
