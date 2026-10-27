# Revisi Rawat Inap

## 1. Pendaftaran Pasien Baru ✅

1. **Nomor handphone:** Batasi pengisian nomor handphone maksimal 13 digit.
2. **Alert status pembayaran:** Hilangkan alert status pembayaran karena tidak diperlukan.

### 1.2 Prioritas

1. **Input deposit pada admisi:** Pada proses admisi, petugas hanya menginput nominal deposit. Nominal tersebut kemudian diteruskan ke Billing Kasir.

2. **Tanda tangan General Consent:** Tanda tangan petugas PPRI pada General Consent harus sesuai dengan identitas pengguna yang sedang login.

3. **Alur pemilihan kamar:** Sesuaikan proses pemilihan kamar dengan urutan berikut:
   - Pilih lantai terlebih dahulu.
   - Tampilkan informasi ketersediaan berdasarkan _wing_.
   - Pilih kamar dan tempat tidur (_bed_).
   - Sediakan pilihan fasilitas berdasarkan daftar kelas perawatan.
   - Tambahkan kolom catatan pada proses pemilihan kamar.
   - Tampilkan status **Upgrade** apabila kelas kamar yang dipilih lebih tinggi daripada kelas fasilitas yang dipilih pasien, dan **Downgrade** apabila kelas kamar lebih rendah.

Sudah Di Kerjakan ✅

4. **Fasilitas kesehatan perujuk:** Ambil daftar fasilitas kesehatan atau instansi perujuk dari data master dan tampilkan sebagai pilihan (_selection_). Setelah instansi dipilih, tampilkan alamat dan nomor telepon instansi tersebut.

5. **Dokter perujuk:** Jadikan nama dokter perujuk sebagai kolom opsional.

6. **Pendaftaran bayi baru lahir:** Pada pendaftaran bayi baru lahir, lewati tahap pengisian identitas, metode pembayaran, dan deposit karena mengikuti data ibu. Daftar pasien yang ditampilkan untuk proses ini diambil dari pasien yang memiliki order operasi persalinan.

Sudah Di Kerjakan ✅

7. **Rencana penagihan ibu dan bayi:** Tempatkan proses rencana penagihan ibu dan bayi di modul Kasir, bukan di Admisi.

Sudah Di Kerjakan ✅

8. **Pemilihan ibu untuk bayi baru lahir:** Tambahkan pemilihan ibu pada proses pendaftaran bayi baru lahir. Tujuannya agar sistem dapat membuat kunjungan baru untuk bayi yang tetap terhubung dengan ibunya.

Sudah Di Kerjakan ✅

### 1.3 Tambahan Prioritas

1. **Status kerja sama asuransi:** Tambahkan informasi status **PKS** atau **Non-PKS** pada asuransi untuk menentukan mekanisme **cashless** atau **reimbursement**.
   - **Cashless:** Pasien dapat langsung pulang tanpa melakukan pembayaran.
   - **Reimbursement:** Rumah sakit menyediakan dokumen yang diperlukan pasien untuk mengajukan klaim reimbursement.

2. **Pilihan kamar bayi:** Saat pemilihan kamar untuk bayi baru lahir, tampilkan hanya daftar kamar bayi.

Sudah Di Kerjakan ✅

3. **Tombol pembatalan pendaftaran:** Tambahkan tombol **Cancel** untuk membatalkan proses pendaftaran yang sedang berlangsung. Seluruh data yang telah diinput dalam proses tersebut dihapus, kemudian pengguna diarahkan kembali ke halaman utama.

Sudah Di Kerjakan ✅

4. **Master asuransi dan penjamin perusahaan:** Gabungkan data asuransi dan penjamin perusahaan ke dalam satu master. Jika jenis asuransi yang dipilih adalah asuransi perusahaan, tampilkan kolom input nama perusahaan.

## 2. Manajemen Tempat Tidur

### 2.1 Tambahan Prioritas

1. **Tab tempat tidur yang belum dikonfirmasi:** Tambahkan tab khusus untuk menampilkan daftar tempat tidur pasien yang belum dikonfirmasi sehingga pengguna tidak perlu menggulir halaman terlalu jauh. Tampilkan data dalam bentuk tabel dengan informasi berikut:
   - Nomor registrasi.
   - Nama pasien.
   - Nomor rekam medis (No. RM).
   - Status pasien prioritas.
   - Penjamin.
   - Waktu pemesanan.
   - Status dependensi: **Low**, **Moderate**, atau **High**.
   - Lantai.
   - _Wing_.
   - Nama kamar.
   - Tempat tidur (_bed_).
   - Status deposit.
   - Nama petugas yang melakukan konfirmasi.
   - Aksi **Konfirmasi**.

Sudah Di Kerjakan ✅

2. **Tab riwayat penggunaan tempat tidur:** Tambahkan tab riwayat penggunaan yang mencatat seluruh aktivitas tempat tidur, mulai dari pemesanan, konfirmasi, hingga perpindahan tempat tidur apabila terjadi. Tampilkan riwayat dalam bentuk tabel, terutama dengan informasi waktu berikut:
   - Waktu pemesanan tempat tidur.
   - Waktu konfirmasi tempat tidur.
   - Waktu perpindahan tempat tidur.

Sudah Di Kerjakan ✅

## 3. Daftar Pasien Rawat Inap

### 3.1 Tambahan Non-Prioritas

1. **Tampilan detail episode:** Pada kartu informasi pasien di halaman Detail Episode, tampilkan nama dan nomor rekam medis secara vertikal (atas dan bawah), bukan dipisahkan dengan tanda strip.

Sudah Di Kerjakan ✅

2. **Label kelas perawatan:** Ubah label **Kelas Perawatan Ditagihkan** menjadi **Kelas Perawatan**.

Sudah Di Kerjakan ✅

3. **Waktu admisi:** Gunakan waktu ketika proses pendaftaran berhasil disimpan sebagai waktu admisi.

Sudah Di Kerjakan ✅

4. **Tata letak Workspace PPRI:** Bagi tampilan Workspace PPRI menjadi dua kartu agar tidak terlalu lebar ketika ditampilkan dalam satu bagian penuh.

Sudah Di Kerjakan ✅

5. **Alert izin pulang:** Tampilkan status izin pulang dalam bentuk alert di bawah kartu informasi utama.
   - **Belum boleh pulang:** Gunakan tampilan peringatan (_warning_).
   - **Boleh pulang:** Gunakan tampilan keberhasilan (_success_).

Sudah Di Kerjakan ✅

### 3.2 Prioritas

1. **Riwayat episode rawat inap:** Catat seluruh proses yang terjadi dalam satu episode rawat inap pasien, mulai dari pendaftaran hingga proses Billing.

Sudah Di Kerjakan ✅

2. **Edukasi pasien:** Tambahkan formulir input edukasi pasien.

### 3.3 Tambahan Prioritas

1. **Identitas bayi baru lahir pada gelang pasien:** Jika pasien yang didaftarkan adalah bayi baru lahir, tampilkan nama pada gelang dengan format **By. Ny. [Nama Ibu]**. Jika bayi kembar, tambahkan penanda, misalnya **(Kembar 1)**. Nomor kembar bertambah secara berurutan untuk bayi yang terhubung dengan ibu yang sama.

2. **Estimasi Biaya:** Perhitungan estimasi Biaya mengikuti penjelasan yang sebelumnya diberikan melalui ChatGPT.

https://chatgpt.com/share/e/6ad09df7-2b1c-83ec-98e2-ff0a0ec399d7

3. **Benefit makan pasien:** Tambahkan benefit makan pasien.

---

## Catatan dari Dokumen Sumber

- Pada bagian **Manajemen Tempat Tidur**, status deposit tertulis dua kali sebagai "belum deposit". Jenis status lainnya tidak dijelaskan dalam dokumen sumber sehingga tidak ditambahkan asumsi baru.
- Metode **perhitungan estimasi biaya** mengacu pada penjelasan di ChatGPT yang tidak tercantum dalam dokumen sumber.
- Rincian mengenai **benefit makan pasien** belum dijabarkan dalam dokumen sumber.
