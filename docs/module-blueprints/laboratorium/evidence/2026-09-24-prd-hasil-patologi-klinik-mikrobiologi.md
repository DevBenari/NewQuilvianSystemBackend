# `LAB-EVD-008` — PRD Hasil Pemeriksaan Patologi Klinik & Mikrobiologi (2026-09-24)

| Field | Nilai |
|---|---|
| Evidence ID | `LAB-EVD-008` |
| Berkas | PRD *Modul Laboratorium - Hasil Pemeriksaan Patologi Klinik & Mikrobiologi*, status dokumen `Draft Requirement` |
| Diserahkan | Pemilik modul, 2026-09-24, ditempel langsung pada sesi `grill-me` |
| Penulis PRD | **Belum disebutkan** |
| Kedudukan | **Bukti untuk direkonsiliasi butir per butir, bukan baseline baru** (`LAB-DEC-133`) |
| Dibaca pada | Backend `ddeb5ed8` (branch `yoga`), frontend `72607a087` |
| Melahirkan | Amendment pass putaran 14; `LAB-DEC-133`; pertentangan `PRD1-CONF-01`..`PRD1-CONF-14`; butir baru `PRD1-NEW-01`..`PRD1-NEW-06` |

## 1. Cara membaca berkas ini

Berkas ini hanya **menyimpan PRD apa adanya** supaya bukti tidak hilang. Pelajaran itu sudah
dibayar sekali: putaran 1 dan 2 hanya menyebut nama berkas buktinya, dan berkasnya kini tidak
dapat ditemukan lagi (lihat baris `LAB-EVD-002` pada decision log).

Rekonsiliasinya — setiap klaim PRD disandingkan dengan keputusan yang sudah berlaku, beserta
statusnya — **tidak** ditulis di sini, melainkan pada decision log bagian
[Amendment Pass Putaran 14](../00-interview-decisions.md). Alasannya sederhana: statusnya
berubah setiap kali pemilik modul menjawab, dan satu tabel yang diperbarui di dua tempat pasti
suatu hari berbeda isinya.

**Yang perlu diketahui pembaca sebelum memakai PRD ini sebagai acuan:** PRD ditulis seolah
blueprint Laboratorium belum ada. Empat belas klaimnya bertentangan dengan keputusan yang sudah
`approved`. Contoh paling tegas:

> PRD menulis *"Dokter Laboratorium dapat mengisi hasil ... dan merupakan satu-satunya role yang
> dapat melakukan Simpan Final"*. Bila dibaca apa adanya, satu orang mengetik angka Kalium 7,5
> lalu mengesahkannya sendiri. Prinsip empat mata `LAB-DEC-003` — ditandatangani pihak klinis
> 2026-09-17 — justru melarang itu, karena salah ketik 3,5 menjadi 7,5 hanya tertangkap bila
> orang kedua memeriksanya.

Karena itu **jangan menurunkan task langsung dari lampiran di bawah**. Turunkan dari decision
log.

## Lampiran A — Isi PRD apa adanya

Disalin tanpa perubahan isi dari teks yang ditempel pemilik modul.

````markdown
# Product Requirement Document (PRD)

# Modul Laboratorium - Hasil Pemeriksaan Patologi Klinik & Mikrobiologi

## 1. Informasi Dokumen

| Item          | Detail                                                           |
| ------------- | ---------------------------------------------------------------- |
| Modul         | Laboratorium                                                     |
| Sub Modul     | Hasil Pemeriksaan Patologi Klinik & Mikrobiologi                 |
| Jenis Dokumen | Product Requirement Document                                     |
| Status        | Draft Requirement                                                |
| Platform      | Hospital Information System                                      |
| Target User   | Petugas Laboratorium, Analis, Dokter Laboratorium, Dokter Klinis |

---

# 2. Latar Belakang

Modul Laboratorium digunakan untuk mengelola proses hasil pemeriksaan laboratorium mulai dari pemeriksaan berjalan, pengisian hasil, penyimpanan draft, finalisasi hasil, distribusi hasil pasien, komunikasi hasil kritis, hingga pencetakan laporan.

Pada Hasil Pemeriksaan Patologi Klinik, halaman detail dibuka berdasarkan No. Order dari datatable hasil pemeriksaan. Hasil berada pada status **Dalam Pemeriksaan** selama proses pengerjaan dan berubah menjadi **Selesai** setelah dilakukan Simpan Final oleh Dokter Laboratorium.

Pada Hasil Pemeriksaan Mikrobiologi, satu halaman merepresentasikan satu No. Order. Satu order dapat memiliki banyak item pemeriksaan, namun menghasilkan satu hasil pemeriksaan mikrobiologi pada level order.

---

# 3. Tujuan Produk

## 3.1 Tujuan Utama

Menyediakan sistem digital Laboratorium yang:

1. Mengurangi proses manual pencatatan hasil pemeriksaan.
2. Menjamin integritas hasil laboratorium.
3. Memastikan hanya user berwenang yang dapat melakukan finalisasi hasil.
4. Mendukung komunikasi hasil kritis secara cepat.
5. Menyediakan histori perubahan dan audit trail.
6. Mendukung kebutuhan cetak dan distribusi hasil pasien.

---

# 4. Scope Modul

## 4.1 Included

### Hasil Pemeriksaan Patologi Klinik

* Detail hasil pemeriksaan.
* Identitas pasien.
* Timeline pemeriksaan.
* Input hasil pemeriksaan.
* Nilai rujukan.
* Flag abnormal.
* Critical value.
* Draft dan Final.
* Amendment.
* Print dan download hasil.
* Pengiriman hasil pasien melalui WhatsApp.
* QR Result Viewer.

### Hasil Pemeriksaan Mikrobiologi

* Informasi pasien.
* Informasi pemeriksaan.
* Diagnosis.
* Informasi specimen.
* Pengisian hasil mikrobiologi.
* Set bakteri.
* Antibiogram.
* Diagnostic Report.
* Preview bilingual.
* Print hasil mikrobiologi.
* Konfirmasi hasil kritis.

---

# 5. Actor & Role

## 5.1 Petugas Laboratorium

Hak akses:

* Melihat hasil pemeriksaan.
* Melakukan preview.
* Print hasil.
* Download hasil.
* Mengirim hasil final ke pasien.
* Mengelola master tertentu sesuai hak akses.

Petugas Laboratorium tidak dapat mengubah hasil Patologi Klinik.

---

## 5.2 Dokter Laboratorium

Hak akses:

* Mengisi hasil pemeriksaan.
* Mengubah hasil sebelum final.
* Simpan Draft.
* Simpan Final.
* Melakukan Amendment.
* Finalisasi hasil koreksi.

Dokter Laboratorium merupakan satu-satunya role yang dapat melakukan Simpan Final.

---

## 5.3 Analis Laboratorium

Hak akses:

* Mengisi data pemeriksaan mikrobiologi.
* Mengelola organisme.
* Mengelola antibiogram.

---

## 5.4 DPJP / Dokter Lantai

Hak akses:

* Menerima informasi hasil kritis.
* Melakukan tindak lanjut klinis.

Nomor WhatsApp dokter berasal dari Master Dokter dan bersifat read-only pada proses komunikasi.

---

# 6. Functional Requirement

# 6.1 Hasil Pemeriksaan Patologi Klinik

## FR-PK-001 Detail Pemeriksaan

Sistem harus menampilkan:

* No Laboratorium.
* No Order.
* Nomor transaksi.
* Nomor mutasi.
* Tanggal order.
* Data pasien.
* Dokter.
* Informasi klinis.
* Timeline pemeriksaan.

---

## FR-PK-002 Input Hasil Pemeriksaan

Dokter Laboratorium dapat:

* Mengisi hasil.
* Mengubah hasil.
* Menyimpan Draft.

Status:

```
Dalam Pemeriksaan
        |
        |
  Simpan Final
        |
        V
     Selesai
```

---

## FR-PK-003 Critical Result

Sistem harus:

* Mengevaluasi nilai hasil terhadap master nilai kritis.
* Memberikan indikator:

| Status   | Tampilan |
| -------- | -------- |
| Low (L)  | Kuning   |
| High (H) | Kuning   |
| Critical | Merah    |

---

## FR-PK-004 Amendment

Jika hasil sudah Final:

* Tidak boleh edit langsung.
* Harus melalui Amendment.
* Wajib menyimpan alasan perubahan.
* Menyimpan nilai sebelum dan sesudah.
* Menyimpan user dan waktu perubahan.

---

## FR-PK-005 Distribusi Hasil Pasien

Prasyarat:

Status hasil:

```
Selesai
```

Aksi:

* Print.
* Download.
* Kirim WhatsApp.

Ketentuan:

* Nomor pasien berasal dari Master Pasien.
* Nomor tidak dapat diedit.
* File dikirim berupa PDF.
* Pengiriman berhasil menambah counter jumlah hasil terkirim.

---

# 6.2 Hasil Pemeriksaan Mikrobiologi

## FR-MB-001 Informasi Pasien

Data otomatis:

* NIK.
* Nama pasien.
* No RM.
* Umur.
* Jenis kelamin.
* Tipe kunjungan.
* Unit layanan.
* Penjamin.

---

## FR-MB-002 Informasi Specimen

Sistem menyediakan:

* Specimen.
* Spesifik specimen.
* Volume specimen.
* Lokasi specimen.
* Metode pengambilan.
* Tanggal penerimaan.

Specimen dapat berasal dari proses penerimaan sampling namun tetap dapat diedit pada halaman hasil.

---

## FR-MB-003 Spesifik Specimen Manual

User dapat:

1. Memilih Lainnya.
2. Input nama specimen.
3. Sistem validasi duplikasi.
4. Jika valid:

   * Simpan.
   * Tampilkan sebagai checkbox.
   * Bisa digunakan kembali.

---

## FR-MB-004 Form Hasil Pemeriksaan

Field:

* Pemeriksaan.
* Analis.
* Hasil pemeriksaan.
* Cito.
* Diplo.
* Definitif.
* Set Bakteri.

---

## FR-MB-005 Set Bakteri & Antibiogram

Sistem mendukung:

* Organisme.
* Subbakteri.
* Antibiotik.
* Interpretasi:

```
S = Sensitive
I = Intermediate
R = Resistant
```

Breakpoint menggunakan konfigurasi laboratorium dan tidak hard-code.

---

# 7. Business Process

# BP-001 Pengisian Hasil Patologi Klinik

1. User membuka menu Hasil Pemeriksaan.
2. Membuka detail berdasarkan No Order.
3. Sistem menampilkan data pasien.
4. Dokter Lab mengisi hasil.
5. Simpan Draft.
6. Evaluasi nilai kritis.
7. Konsultasi eksternal jika diperlukan.
8. Simpan Final.
9. Status berubah menjadi Selesai.

---

# BP-002 Pengisian Hasil Mikrobiologi

1. User membuka No Order.
2. Sistem menampilkan data pasien dan pemeriksaan.
3. User melengkapi specimen.
4. User mengisi hasil.
5. User memilih bakteri dan antibiotik.
6. User menyimpan Draft.
7. Dokter Lab melakukan Final.

---

# BP-003 Hasil Kritis

Flow:

```
Hasil Pemeriksaan
        |
        |
Evaluasi Critical Rule
        |
        |
Critical ditemukan
        |
        |
Konfirmasi Dokter
        |
        |
WhatsApp
```

Sistem mencatat waktu pengiriman.

---

# 8. Status Management

| Status            | Deskripsi                               |
| ----------------- | --------------------------------------- |
| Draft             | Data masih dapat diedit                 |
| Dalam Pemeriksaan | Pemeriksaan sedang berjalan             |
| Definitif         | Sudah dikonsultasikan namun belum final |
| Final/Selesai     | Hasil terkunci                          |
| Amendment         | Koreksi hasil final                     |

---

# 9. UI Requirement

## Halaman Detail

Komponen:

### Header

* Identitas Rumah Sakit.
* Judul pemeriksaan.
* QR Code.

### Patient Card

* Nama pasien.
* RM.
* Umur.
* Jenis kelamin.

### Examination Card

* Nomor order.
* Dokter.
* Pemeriksaan.
* Status.

### Result Table

Kolom:

* Parameter.
* Hasil.
* Satuan.
* Nilai rujukan.
* Flag abnormal.

---

# 10. Integration

Integrasi:

## Master Pasien

Digunakan untuk:

* Identitas pasien.
* Nomor telepon.
* Email.

## Master Dokter

Digunakan untuk:

* DPJP.
* Dokter konfirmasi hasil kritis.

## Order Laboratorium

Menyediakan:

* Nomor order.
* Pemeriksaan.
* Data transaksi.

## WhatsApp Gateway

Digunakan untuk:

* Notifikasi hasil kritis.
* Pengiriman hasil pasien.

---

# 11. Audit Trail

Sistem wajib mencatat:

* User.
* Waktu.
* Aktivitas.
* Data sebelum perubahan.
* Data sesudah perubahan.

Minimal digunakan pada:

* Amendment.
* Master Nilai Kritis.
* Distribusi hasil.

---

# 12. Non Functional Requirement

## Security

* Role based access.
* Data final tidak dapat diedit.
* Semua perubahan tercatat.

## Performance

* Loading data harus menggunakan indikator proses.
* Detail hasil harus dapat dibuka berdasarkan No Order.

## Usability

* Form tidak menggunakan modal untuk input utama.
* Informasi harus mudah dibaca.
* Mendukung print multi halaman.

---

# 13. Acceptance Criteria

## Patologi Klinik

| Scenario               | Expected Result        |
| ---------------------- | ---------------------- |
| Dokter Lab input hasil | Berhasil simpan draft  |
| Dokter Lab finalisasi  | Status menjadi Selesai |
| User edit hasil final  | Ditolak                |
| Amendment dilakukan    | Audit trail tersimpan  |
| Kirim hasil pasien     | PDF terkirim           |

---

## Mikrobiologi

| Scenario           | Expected Result         |
| ------------------ | ----------------------- |
| Membuka order      | Data pasien tampil      |
| Input specimen     | Data tersimpan          |
| Set bakteri        | Antibiogram muncul      |
| Critical ditemukan | Notifikasi dokter aktif |
| Finalisasi         | Hanya Dokter Lab        |

---

# 14. Future Enhancement

Potensi pengembangan:

* Integrasi LIS.
* Integrasi SATUSEHAT DiagnosticReport.
* Auto notification WhatsApp.
* Dashboard monitoring SLA hasil kritis.
* Analitik performa laboratorium.

---

# End Document
````
