# Panduan Operasional & Roadmap Alur Kerja: Pendaftaran hingga Triage IGD

**Rumah Sakit:** Metropolitan Medical Centre (MMC Hospital)  
**Sistem:** Quilvian Hospital System V2 — Modul Instalasi Gawat Darurat (IGD)  
**Target Pengguna:** Analis Sistem, Penguji Kualitas (QA), Staf Operasional IGD  
**Tujuan Dokumen:** Pedoman Teknis Pengujian Langsung dan Skenario Demo Siap Pakai (Selasa, 13 Oktober 2026)  
**Berkas Word (.docx):** [`Panduan_Alur_Pendaftaran_dan_Triage_IGD_QuilvianV2.docx`](file:///c:/Users/User/QuilvianV2/Panduan_Alur_Pendaftaran_dan_Triage_IGD_QuilvianV2.docx)

---

## BAB 1. Pendahuluan & Arsitektur Alur Utama (Encounter-First)

Instalasi Gawat Darurat (IGD) memiliki karakteristik pelayanan medis darurat yang berpacu dengan waktu. Berbeda fundamental dengan loket Rawat Jalan yang menerapkan nomor antrean poliklinik dan jam perjanjian terjadwal, alur IGD dirancang dengan filosofi **Encounter-First** (keselamatan pasien di atas birokrasi pendaftaran).

### 1.1. Konsep Utama: Encounter-First vs Kunjungan Klinis
1. **Encounter Gawat Darurat:**
   Terbit secara instan saat pasien didaftarkan di loket IGD. Berfungsi sebagai entitas administratif awal rumah sakit tanpa antrean poliklinik, mengizinkan pasien langsung dikirim ke stasiun perawat triage.
2. **Kelahiran Kunjungan IGD (Visit):**
   Kunjungan klinis IGD lahir secara resmi saat perawat triage menekan **"Mulai Triage"** atau aksi cepat **"Tangani Segera"**. Kunjungan mengikat penilaian klinis, tanda-tanda vital, klasifikasi prioritas ATS/ESI, penempatan ruang, dan penugasan DPJP.
3. **Penutupan Bersama (Unified Discharge):**
   Ketika rangkaian pelayanan gawat darurat selesai atau pasien dipindahkan (ke Ruang Rawat Inap, Kamar Operasi, atau Pulang), Encounter dan Kunjungan IGD ditutup secara serentak dalam satu gerbang validasi.

> **Prinsip Utama `IGD-DEC-139` (Satu Pasien, Satu Episode Terbuka):**  
> Seorang pasien hanya boleh memiliki satu episode gawat darurat terbuka pada satu waktu. Sistem secara otomatis mencegah pendaftaran ganda (*double-registration*) kecuali petugas menyertakan alasan sah yang dapat dipertanggungjawabkan.

---

## BAB 2. Tahap I: Pendaftaran Pasien IGD (Loket Pendaftaran)

Proses pendaftaran dilakukan oleh Petugas Loket Pendaftaran IGD melalui menu utama sistem.

### 2.1. Navigasi Antarmuka
- **Akses Menu:** Sidebar Kiri → Kelompok **Instalasi Gawat Darurat** → Submenu **Pendaftaran Pasien**
- **Rute URL:** `http://localhost:3000/health-services/registration-management/emergency-registration`

### 2.2. Langkah Kerja Pendaftaran Langkah-demi-Langkah
Formulir pendaftaran menerapkan model *Stepper* terstruktur dengan langkah-langkah berikut:
1. **Langkah 1: Identifikasi & Pemilihan Pasien (Patient Entry Choice):**
   Tersedia 3 jalur registrasi:
   - *Pasien Lama:* Mencari rekam medis melalui No. RM, NIK, atau Nama Pasien.
   - *Pasien Baru:* Menginput biodata lengkap pasien yang baru pertama kali berobat.
   - *Pasien Tanpa Identitas / Mr. X:* Sistem secara otomatis menerbitkan **Rekam Pengganti darurat** (`IGD-DEC-151`) untuk korban kecelakaan/tidak sadar tanpa identitas.
2. **Langkah 2: Penjamin & Cara Pembayaran (Payer & Payment Method):**
   Pilih jenis penjaminan pasien: **Umum (Biaya Pribadi)**, **BPJS Kesehatan** (menginput No. Kartu BPJS / SEP Gawat Darurat), atau **Asuransi Swasta / Perusahaan Rekanan**. Sistem memungkinkan penambahan penjamin cadangan.
3. **Langkah 3: Rujukan & Moda Kedatangan (Emergency Referral):**
   Pilih moda transportasi kedatangan: Datang Sendiri, Ambulans RS, Ambulans 118, Rujukan Puskesmas, Rujukan RS Lain, atau Pengantar Kepolisian (kasus visum/kecelakaan lalu lintas).
4. **Langkah 4: Keluhan Utama & Triage Awal Kasus (Visit Step):**
   Petugas mencatat keluhan utama yang disampaikan pengantar/pasien (misalnya: sesak napas berat, nyeri dada kiri menjalar, luka robek kecelakaan kerja) dan memilih jenis kasus (Trauma / Non-Trauma / Kebidanan / Bedah).
5. **Langkah 5: Verifikasi & Finalisasi Pendaftaran (Verification Step):**
   Sistem menampilkan ringkasan data. Petugas mengonfirmasi data dan menekan tombol **"Simpan Pendaftaran Gawat Darurat"**. Sistem menerbitkan Nomor Encounter IGD (contoh: `IGD-261001034358-B98C47`) dan pasien langsung masuk ke daftar antrean Triage.

> **Fitur Proteksi: Penanganan Pendaftaran Ganda (Episode Guard):**  
> Jika pasien yang sama didaftarkan kembali padahal masih memiliki kunjungan IGD yang belum ditutup, backend akan menolak pendaftaran dan menampilkan nomor episode yang masih aktif. Petugas dapat memilih membuka episode lama atau mengisi alasan pendaftaran ganda (maksimal 500 karakter) jika kasusnya terpisah secara klinis.

---

## BAB 3. Tahap II: Triage Pasien IGD (Perawat Triage)

Triage adalah jantung operasional IGD. Perawat IGD menilai kegawatan pasien secara cepat, mengklasifikasikan prioritas, dan menempatkan pasien pada zona ruangan yang tepat.

### 3.1. Navigasi Antarmuka
- **Akses Menu:** Sidebar Kiri → Kelompok **Instalasi Gawat Darurat** → Submenu **Triage Pasien**
- **Rute URL:** `http://localhost:3000/health-services/emergency-installation-management/emergency-triage`

### 3.2. Membuka Pasien dari Antrean 'Menunggu Triage'
Pada tabel Triage Pasien, perawat melihat daftar pasien yang baru saja didaftarkan di loket dengan status **"Menunggu Triage"**. Setiap baris menampilkan waktu terdaftar, nama pasien, nomor RM, dan keluhan awal.

### 3.3. Dua Pintu Keputusan Tindakan: Standar vs Fast-Track
- **Jalur A — Tombol "Mulai Triage" (Kasus Standar):**  
  Digunakan untuk pasien sadar/stabil. Perawat memeriksa waktu tiba (terisi otomatis dari waktu terdaftar, dapat disesuaikan) dan mengisi penilaian triage lengkap secara langsung.
- **Jalur B — Tombol "Tangani Segera" (Kasus Resusitasi Kritis - `IGD-DEC-143`):**  
  Khusus untuk pasien henti napas/jantung, syok berat, atau kejang hebat. Tombol ini **TIDAK MEMERLUKAN FORMULIR APAPUN**. Satu kali klik langsung melahirkan kunjungan IGD dengan status **"Sedang Ditangani"**, membebaskan perawat untuk segera melakukan tindakan resusitasi tanpa hambatan administrasi. Pengisian formulir triage disusulkan setelah pasien stabil.

### 3.4. Pengisian Formulir Penilaian Triage Klinis
Formulir Triage memuat parameter klinis terstandar nasional dan internasional:
1. **Tanda-Tanda Vital (TTV):**  
   Tekanan Darah (mmHg), Nadi (x/menit), Laju Pernapasan (x/menit), Suhu Tubuh (°C), Saturasi Oksigen SpO2 (%), Skala Nyeri (0-10 Wong-Baker/NRS), dan Kesadaran GCS (Eye, Verbal, Motorik).
2. **Tanda Bahaya (Red Flags):**  
   Centang tanda bahaya yang terdeteksi: Nyeri dada kardiak, penurunan kesadaran akut, perdarahan hebat, distres pernapasan, fraktur terbuka.
3. **Klasifikasi Kategori Prioritas Triase:**

| Kategori Prioritas | Label Warna | Kriteria Kegawatan Klinis | Target Respons Medis |
| :--- | :---: | :--- | :--- |
| **Prioritas 1 (Resusitasi)** | Merah | Ancaman henti napas / henti jantung akut, jalan napas tersumbat, syok anafilaktik/hipovolemik berat. | Segera / 0 Menit (Langsung ke Ruang Resusitasi) |
| **Prioritas 2 (Emergensi)** | Kuning | Berpotensi mengancam jiwa/organ jika tidak ditangani segera, nyeri hebat, fraktur mayor, aritmia stabil. | < 10 – 15 Menit (Ruang Tindakan Kritis / Bedah) |
| **Prioritas 3 (Urgen / Semi)** | Hijau | Kondisi stabil, tanda vital normal, nyeri ringan-sedang, cedera minor, demam tanpa komplikasi. | < 30 – 60 Menit (Ruang Tindakan Non-Kritis / Observasi) |
| **Prioritas 4 (DOA)** | Hitam | *Death on Arrival* (pasien tiba di IGD dalam kondisi sudah meninggal dunia dengan tanda pasti kematian). | Protokol Pemulasaraan Jenazah / Ruang Forensik |

4. **Penempatan Ruang & Rencana:**  
   Pilih zona ruang perawatan: Ruang Resusitasi, Ruang Kritis, Ruang Tindakan Bedah, Ruang Tindakan Non-Bedah, atau Ruang Observasi.
5. **Simpan Triage:**  
   Menekan tombol **"Simpan Penilaian Triage"**. Kunjungan IGD resmi tercatat dengan nomor kunjungan unik, dan sistem mengalihkan tampilan ke panel detail Riwayat Triage Pasien.

---

## BAB 4. Tahap III: Penetapan DPJP Dokter Jaga IGD (Fitur Baru)

Berdasarkan standar akreditasi rumah sakit dan modul terbaru (**BE-IGD-056 + FE-IGD-037**), setiap pasien IGD yang selesai ditriage WAJIB memiliki Dokter Penanggung Jawab Pelayanan (DPJP) yang jelas dan terdokumentasi.

### 4.1. Lokasi Akses Penetapan DPJP
- **Pintu A (Panel Riwayat Triage):** Pada rute `/health-services/emergency-installation-management/emergency-triage/[encounterId]`, terdapat banner penugasan DPJP dengan tombol **"Tetapkan Dokter Sekarang"** (jika belum ada dokter) atau **"Alihkan Dokter"**.
- **Pintu B (Ruang Kerja Dokter IGD):** Pada rute `/health-services/emergency-installation-management/doctor-emergency`, kartu informasi pasien memuat ruas **"Dokter PJ IGD"**.

### 4.2. Logika Dua Kelompok Dokter Jaga (Roster Validation)
Saat modal penugasan dokter dibuka, sistem secara dinamis menyusun pilihan dokter pada jam tersebut:
1. **Kelompok 1: Dokter Jaga IGD Saat Ini (On-Duty Doctors):**  
   Menampilkan dokter yang memiliki jadwal dinas aktif di unit Emergency pada jam penugasan (contoh: *dr. Farhan Malik* — Shift Pagi / Malam). Jika dipilih, tombol simpan aktif langsung tanpa memerlukan isian alasan.
2. **Kelompok 2: Di Luar Jadwal Jaga — Wajib Alasan (Off-Duty Overrides):**  
   Menampilkan dokter umum/emergency aktif yang tidak memiliki jadwal dinas pada jam tersebut (contoh: *dr. Ranger Biru*). Jika dokter ini dipilih, kolom teks **"Alasan Memilih Dokter di Luar Jadwal Jaga"** otomatis muncul dan **WAJIB diisi** (maksimal 500 karakter). Tombol simpan terkunci jika alasan kosong.
3. **Kondisi Roster Kosong (Zero Schedule On-Duty):**  
   Bila pada jam tersebut tidak ada dokter yang memiliki jadwal jaga IGD, sistem secara otomatis menampilkan pesan panduan di atas pemilih: *"Tidak ada dokter berjadwal jaga IGD pada jam ini. Pilih dokter di luar jadwal jaga dan isi alasannya."*, dan hanya menampilkan kelompok di luar jadwal.

> **Tampilan Banner Penanda Override Pasca-Penetapan:**  
> Bila dokter yang ditugaskan berasal dari kelompok di luar jadwal jaga, banner DPJP secara transparan menampilkan catatan: *"Di luar jadwal jaga — alasan: [Alasan yang diinput perawat]"*. Hal ini mempermudah audit medik dan transparansi operasional.

### 4.3. Fitur Alihkan Dokter (Handover DPJP)
Saat terjadi pergantian shift dinas (misal pergantian dari Shift Siang ke Shift Malam) atau kondisi dokter jaga sebelumnya harus menangani pasien operasi emergensi, perawat menekan tombol **"Alihkan Dokter"**. Formulir serah terima meminta dokter baru, alasan pengalihan, dan riwayat penugasan dokter sebelumnya tetap tersimpan lengkap di log audit.

---

## BAB 5. Tahap IV: Kelanjutan Pelayanan Medis

Setelah pasien memiliki Prioritas Triage dan DPJP yang ditugaskan, rangkaian pelayanan klinis IGD berlanjut ke dua sub-modul utama:
1. **Pengkajian Pasien / Asuhan Keperawatan (`/emergency-assessment`):**  
   Perawat melakukan pengkajian asuhan keperawatan lengkap, evaluasi skala risiko jatuh (*Morse / Humpty Dumpty*), pengkajian dekubitus (*Braden*), implementasi keperawatan, dan observasi tanda vital berkala di lembar grafik.
2. **Ruang Kerja Pemeriksaan Dokter (`/doctor-emergency`):**  
   Dokter penanggung jawab (DPJP) membuka ruang kerja pemeriksaan pasien, melihat nomor triase dan tanda vital, menyusun catatan medis SOAP, mengajukan permintaan penunjang CITO (Laboratorium Darah, Rontgen/CT-Scan Radiologi), meresepkan terapi obat darurat, serta memutuskan Disposisi Akhir (Rawat Inap / Kamar Operasi / Rujuk / Pulang Rawat Jalan).

---

## BAB 6. Skenario Demo Siap Pakai untuk Analis (Demo Selasa)

Berikut adalah panduan urutan skenario demo terarah berdurasi 10–12 menit yang dapat langsung dibawakan oleh Analis di depan stakeholder atau manajemen pada hari Selasa:

### 6.1. Lembar Panduan Skenario Demo Langkah-demi-Langkah

| Menit | Aksi di Antarmuka | Narasi & Penjelasan Analis | Poin Kunci yang Ditunjukkan |
| :---: | :--- | :--- | :--- |
| **00 - 02** | Buka menu Pendaftaran Pasien IGD.<br>Cari pasien lama 'Rivaldo' atau daftar pasien baru 'Tn. Budi Santoso'. | *"Kita mulai dari loket IGD. Pasien datang dalam kondisi darurat kecelakaan. Pendaftaran bersifat Encounter-First tanpa antrean poliklinik."* | Kemudahan pencarian pasien, pemilihan penjamin BPJS/Umum, rujukan datang sendiri. |
| **02 - 04** | Simpan pendaftaran. Tunjukkan nomor registrasi/encounter terbit. | *"Sistem langsung menerbitkan encounter IGD dan mengirim data pasien ke stasiun perawat triage secara real-time."* | Tidak ada nomor antrean kaku; kecepatan registrasi < 1 menit. |
| **04 - 07** | Pindah ke menu Triage Pasien.<br>Klik 'Mulai Triage' pada Tn. Budi Santoso.<br>Input TTV dan pilih Prioritas 2 Kuning. | *"Perawat triage langsung menerima pasien. Tanda vital diisi, sistem mengklasifikasikan ke Prioritas 2 (Kuning/Emergensi) dengan zona Ruang Tindakan Bedah."* | Otomatisasi waktu tiba, input tanda vital lengkap, badge visual prioritas kuning. |
| **07 - 10** | Simpan Triage. Halaman beralih ke Riwayat Triage.<br>Klik 'Tetapkan Dokter Sekarang'.<br>Tunjukkan dua kelompok dokter. | *"Inilah fitur terbaru standar kepatuhan DPJP IGD. Sistem memilah dokter jaga yang sedang dinas vs dokter di luar jadwal."* | Tunjukkan Kelompok 1 (Dokter Jaga) dan Kelompok 2 (Wajib Alasan). |
| **10 - 12** | Pilih dokter jaga, klik simpan.<br>Lalu demokan tombol 'Alihkan Dokter' jika ada pergantian dinas. | *"DPJP resmi terpasang di banner pasien beserta waktu penugasan. Perawat juga dapat melakukan serah terima tugas (handover) saat pergantian shift."* | Banner hijau DPJP aktif, catatan override, dan riwayat penugasan transparan. |

### 6.2. Antisipasi Pertanyaan Kritis dari Pengguna / Stakeholder
- **Tanya: "Bagaimana jika pasien datang dalam kondisi henti jantung/resusitasi, apakah harus isi form triage dulu?"**  
  *Jawab:* "Tidak perlu. Sistem menyediakan tombol Fast-Track **Tangani Segera** di antrean triage. Cukup 1 kali klik tanpa isian, status pasien langsung menjadi Sedang Ditangani dan penanganan medis dapat berjalan seketika. Formulir triage diisi susulan setelah pasien stabil."
- **Tanya: "Bagaimana jika dokter spesialis ortopedi atau bedah ingin ditugaskan di IGD?"**  
  *Jawab:* "Dokter spesialis di IGD dicatat sebagai **Konsulen Medis**. DPJP IGD tetap dipegang oleh dokter jaga umum/emergency yang bertugas di ruangan IGD untuk menjamin kontinuitas penanganan gawat darurat (sesuai `IGD-DEC-261`)."
- **Tanya: "Bagaimana jika terjadi pemadaman jadwal atau tidak ada dokter jaga yang terjadwal?"**  
  *Jawab:* "Sistem memiliki proteksi **Roster Kosong**. Sistem akan memandu perawat untuk memilih dokter umum yang tersedia di rumah sakit dengan mencantumkan alasan darurat (*override reason*), sehingga pelayanan tidak pernah macet."

---

## BAB 7. Matriks Daftar Ceklis Pengujian (Test Checklist Matrix)

Daftar periksa verifikasi mandiri bagi Analis Sistem sebelum sesi demo dimulai:

| No | Item Pengujian Fungsional | Kriteria Keberhasilan | Status Uji |
| :---: | :--- | :--- | :---: |
| 1 | Login Akun Perawat IGD | Berhasil masuk dengan akun perawat, menu IGD terbuka di sidebar kiri. | **[ OK ] Siap Demo** |
| 2 | Pendaftaran Pasien IGD | Encounter gawat darurat tersimpan, nomor registrasi IGD terbit tanpa nomor antrean. | **[ OK ] Siap Demo** |
| 3 | Pencegahan Pendaftaran Ganda | Pasien dengan episode terbuka ditolak dengan pesan nomor encounter aktif. | **[ OK ] Terverifikasi** |
| 4 | Antrean Menunggu Triage | Pasien yang baru didaftarkan langsung muncul di daftar triage teratas. | **[ OK ] Siap Demo** |
| 5 | Aksi 'Tangani Segera' | Satu klik tanpa formulir langsung mengubah status menjadi 'Sedang Ditangani'. | **[ OK ] Siap Demo** |
| 6 | Pengisian TTV & Prioritas Triage | Prioritas 1 s.d. 4 terhitung tepat, penempatan zona ruang tersimpan. | **[ OK ] Siap Demo** |
| 7 | Modal Pemilih DPJP (2 Kelompok) | Menampilkan kelompok 'Dokter Jaga IGD' dan 'Di luar jadwal jaga — wajib alasan'. | **[ OK ] Siap Demo** |
| 8 | Validasi Alasan Override | Memilih dokter di luar jadwal mengunci tombol simpan bila alasan kosong; batas 500 karakter. | **[ OK ] Siap Demo** |
| 9 | Banner DPJP & Teks Override | Banner menampilkan nama dokter, waktu, dan teks override hasil pangkasan server. | **[ OK ] Siap Demo** |
| 10 | Alihkan Dokter (Handover) | Handover dokter berhasil mencatat riwayat penugasan berkesinambungan. | **[ OK ] Siap Demo** |
