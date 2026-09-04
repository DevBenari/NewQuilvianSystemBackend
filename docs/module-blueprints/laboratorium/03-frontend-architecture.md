# Laboratorium — Arsitektur Frontend

| Field | Value |
|---|---|
| Blueprint ID | `LAB-BP-001` |
| Revision | `3` |
| Status | `draft` |
| Scope | Slice `S1a`, `S2`, `S3`, `S7`, `S10`, `S11`, `S13a`, `S14`, `S15` |
| Frontend SHA | `688daff90` |
| Wewenang UI | `LAB-DEC-010` |

> **Keadaan awal.** Frontend Laboratorium **tidak ada sama sekali** pada `688daff90`. Tidak ada
> route, tidak ada layar, tidak ada API service. Bukti: `01-existing-capability-map.md#CAP-21`.
> Seluruh isi dokumen ini adalah pembangunan dari nol.
>
> **Batas dokumen ini.** Yang ditetapkan adalah **kontrak fungsional** — layar apa yang
> dibutuhkan, siapa boleh melakukan apa, data dan status apa yang dikonsumsi, dan mana yang
> menjadi keleluasaan developer. Tata letak, warna, dan pilihan komponen **tidak** ditetapkan
> di sini.

---

## 1. Urutan Wewenang

```text
keamanan / privasi / invariant
  → arahan produk/UI yang disetujui
    → konvensi project
      → DEV_DISCRETION
```

| Tingkat | Keadaan pada modul ini |
|---|---|
| Keamanan, privasi, invariant | **Ada dan mengikat.** Lihat bagian 5 |
| Arahan produk/UI yang disetujui | **Tidak ada.** Belum pernah diterbitkan untuk Laboratorium |
| Konvensi project | **Ada dan mengikat** untuk letak menu dan penamaan route (`LAB-DEC-010`) |
| `DEV_DISCRETION` | Berlaku untuk sisanya |

---

## 2. Pola Berlapis yang Wajib Diikuti

`LAB-DEC-010` memerintahkan mengikuti pola modul Health Services yang sudah ada. Modul
`pharmacy-management@688daff90` dipakai sebagai acuan karena polanya paling lengkap.

| Lapis | Lokasi |
|---|---|
| Route halaman | `src/app/health-services/laboratory-management/<menu>/page.jsx` |
| Komponen fitur | `src/components/features/health-services/laboratory-management/<fitur>/` |
| Komponen tampilan | `src/components/view/health-services/laboratory-management/<menu>/` |
| Konstanta dan konfigurasi | `src/lib/constants/health-services/laboratory-management/` |
| Hook | `src/lib/hooks/health-services/laboratory-management/` |
| API service | `src/lib/services/health-services/laboratory-management/` |
| Style | `src/style/health-services/laboratory-management/<menu>/` |

Pemanggilan API memakai `src/lib/axiosInstance@688daff90`. Pengelolaan state memakai potongan
Redux mengikuti pola `src/lib/state/slice/@688daff90`.

**Yang tidak boleh.** Membuat pola penamaan route baru, atau menaruh berkas di luar tujuh lapis
di atas.

### 2.1 Penempatan menu data induk — mengikuti konvensi frontend yang sudah ada

> **Catatan penting.** `LAB-DEC-034` tentang penempatan data induk menurut cakupan pemakaian
> berlaku **hanya untuk backend**. Frontend **tidak mengikutinya**. Menu data induk di frontend
> tetap memakai konvensi yang sudah berjalan.

**Konvensi frontend yang berlaku pada `688daff90`.** Seluruh menu data induk Health Services
berada di satu tempat, tanpa memandang modul mana yang memilikinya:

| Lapis | Lokasi |
|---|---|
| Route halaman | `src/app/health-services/master-data/<menu>/` |
| Komponen tampilan | `src/components/view/health-services/master-data/<menu>/` |
| Konstanta dan konfigurasi | `src/lib/constants/health-services/master-data/` |

**Bukti bahwa ini memang konvensinya.** Folder `master-data` pusat berisi 28 menu, dan di
antaranya terdapat data induk yang sebenarnya milik modul tertentu:

| Menu | Sebenarnya milik | Tetap berada di `master-data` pusat |
|---|---|:---:|
| `drug`, `drug-category`, `drug-unit-conversions`, `drug-stock-policy`, `drug-storage-locations`, `drug-supplier` | Farmasi | Ya |
| `medical-record-access-purposes` | Rekam Medis | Ya |
| `insurance-tariffs`, `insurance-coverage-rules` | Insurance | Ya |
| `diagnosis-*`, `doctor-*`, `measurement*`, `procedure`, `room`, `bed` | Berbagai modul | Ya |

**Penerapan pada modul ini:**

| Menu data induk | Letak berkas frontend |
|---|---|
| Batas nilai pemeriksaan | `…/health-services/master-data/lab-value-bounds/` |
| Pilihan hasil terbatas | Menyatu dengan layar batas nilai di atas |
| Alasan penolakan sampel | `…/health-services/master-data/lab-rejection-reasons/` |
| Jenis pemeriksaan | `…/health-services/master-data/procedure/` — **sudah ada, dipakai ulang** |
| Tarif dan cakupan penjamin | `…/health-services/master-data/insurance-tariffs/` — **sudah ada, dipakai ulang** |

**Yang tetap berada di folder `laboratory-management`.** Hanya layar **operasional** —
pesanan, wadah dan pemeriksaan, daftar kerja, monitoring per disiplin. Layar data induk tidak.

**Dua pengecualian yang ada di kode dan sengaja tidak ditiru.** Terdapat
`src/components/view/health-services/billing-management/master-data/` dan
`src/components/view/health-services/patient-management/master-data/`. Keduanya menyimpang dari
pola dominan. Modul Laboratorium **mengikuti pola dominan**, bukan kedua pengecualian itu.

**Yang tidak boleh diduplikasi.** Pada `688daff90` sudah terdapat
`src/lib/constants/health-services/master-data/procedure-constants.jsx`,
`insurance-tariff-constants.jsx`, dan `tariff-category-constants.jsx`. Modul Laboratorium
**memakai ulang** ketiganya, tidak membuat salinan.

---

## 3. Kebutuhan Layar

### 3.1 Pesanan Laboratorium

| Aspek | Isi |
|---|---|
| Slice | `S1a` |
| Kegunaan | Dokter membuat pesanan, melihat pesanannya, dan menandai cito |
| Data yang dikonsumsi | Daftar dan detail pesanan, daftar jenis pemeriksaan berpenanda laboratorium |
| Status yang ditampilkan | `Requested`, `Accepted`, `InProcess`, `Completed`, `OnHold`, `Cancelled`, dan penanda kesegeraan |
| Endpoint | Grup Lab Order dan Lab Examination |

| Peran | Boleh melakukan |
|---|---|
| Dokter pemesan | Membuat pesanan, menambah pemeriksaan, menandai cito, membatalkan |
| Petugas laboratorium | Melihat saja |

### 3.2 Wadah dan Pemeriksaan

| Aspek | Isi |
|---|---|
| Slice | `S2` |
| Kegunaan | Petugas merencanakan wadah, mencatat pengambilan dan penerimaan, menyatakan layak atau menolak, meminta ambil ulang |
| Data yang dikonsumsi | Daftar wadah beserta pemeriksaan yang ditopangnya, daftar alasan penolakan aktif |
| Status yang ditampilkan | Seluruh status wadah dan status pemeriksaan |
| Endpoint | Grup Lab Specimen dan Lab Examination |

| Peran | Boleh melakukan |
|---|---|
| Perawat atau flebotomis | Mencatat pengambilan |
| Petugas penerimaan | Mencatat penerimaan |
| Petugas berwenang menetapkan kelayakan | Menyatakan layak, menolak, meminta ambil ulang |
| Dokter pemesan | Melihat saja |

**Yang wajib ditampilkan tanpa kecuali:** satu wadah beserta **seluruh** pemeriksaan yang
ditopangnya, dalam satu tampilan. Petugas harus melihat bahwa menolak wadah berarti
menggugurkan semua pemeriksaan itu, **sebelum** ia menekan tombol tolak.

### 3.3 Daftar Kerja

| Aspek | Isi |
|---|---|
| Slice | `S7` |
| Kegunaan | Petugas melihat antrean pekerjaan; kepala instalasi memantau keterlambatan cito |
| Data yang dikonsumsi | Daftar pekerjaan belum selesai, daftar pesanan cito yang lewat batas waktu |
| Status yang ditampilkan | Status wadah, penanda cito, sisa atau kelebihan waktu |
| Endpoint | Grup Lab Worklist |

### 3.4 Batas Nilai

| Aspek | Isi |
|---|---|
| Slice | `S3` |
| Kegunaan | Kepala instalasi mengelola satuan, batas normal, daftar pilihan, dan batas waktu cito. Mengajukan perubahan batas kritis |
| Data yang dikonsumsi | Daftar dan detail batas nilai, daftar pilihan, riwayat perubahan, daftar pengajuan |
| Endpoint | Grup Lab Value Bound dan Lab Critical Bound Approval |

| Peran | Boleh melakukan |
|---|---|
| Kepala instalasi laboratorium | Mengubah satuan, batas normal, pilihan, batas waktu cito. **Mengajukan** perubahan batas kritis |
| Pemegang kewenangan persetujuan batas kritis | Menyetujui atau menolak pengajuan |
| Petugas laboratorium | Melihat saja |

**Bentuk isian mengikuti bentuk hasil.** Bila pemeriksaan berbentuk angka, yang ditampilkan
adalah isian satuan dan empat batas. Bila berbentuk pilihan, yang ditampilkan adalah daftar
pilihan beserta penanda di luar rujukan dan penanda kritis. Keduanya **tidak** ditampilkan
bersamaan.

### 3.5 Alasan Penolakan Sampel

| Aspek | Isi |
|---|---|
| Slice | `S11` |
| Kegunaan | Kepala instalasi mengelola daftar alasan penolakan |
| Data yang dikonsumsi | Daftar alasan beserta penandanya |
| Endpoint | Grup Lab Rejection Reason |

| Peran | Boleh melakukan |
|---|---|
| Kepala instalasi laboratorium | Menambah, mengubah nama dan keterangan, mengatur urutan, mengaktifkan atau menonaktifkan |
| Administrator sistem | Menyetel penanda kesalahan internal dan penanda wajib catatan |

---

## 4. Kontrak Penanganan State

Berlaku untuk seluruh layar di atas.

| Keadaan | Yang harus terjadi |
|---|---|
| Sedang memuat | Penanda muat yang jelas. Tombol tindakan dinonaktifkan selama proses berjalan |
| Kosong | Keterangan yang menjelaskan **kenapa** kosong dan apa langkah berikutnya. Bukan sekadar "tidak ada data" |
| Gagal | Pesan dari server ditampilkan apa adanya, disertai tombol coba lagi |
| Data basi | Setelah setiap tindakan berhasil, daftar terkait dimuat ulang. Daftar kerja dimuat ulang saat layar kembali difokuskan |
| Kirim ganda | Tombol tindakan dikunci sejak ditekan sampai jawaban server datang. Ini mencegah dua permintaan menyatakan layak untuk wadah yang sama |
| Bentrok `409` | Pesan "data baru saja diubah petugas lain" disertai tombol muat ulang. **Tidak boleh** memaksa kirim ulang otomatis |
| Tanpa hak akses `403` | Tombol tindakan yang tidak boleh dipakai **disembunyikan atau dinonaktifkan**, bukan dibiarkan lalu gagal saat ditekan |

**Kenapa "kirim ganda" penting di sini.** Menyatakan wadah layak menerbitkan kelayakan tagih.
Walaupun sisi server sudah idempoten dan terbukti lewat pengujian, mengunci tombol mencegah
petugas ragu apakah tindakannya berhasil.

---

## 5. Matriks Kewenangan UI

| ID | Area | Tingkat wewenang | Ruang gerak |
|---|---|---|---|
| `LAB-FE-001` | Letak menu dan penamaan route | Konvensi project | **Wajib** mengikuti pola Health Services yang ada. Tidak boleh membuat pola baru |
| `LAB-FE-002` | Tata letak, tab/modal/drawer, warna, komponen | Developer | `DEV_DISCRETION`, selama memakai komponen dan gaya yang sudah dipakai modul lain |
| `LAB-FE-006` | Urutan daftar kerja: cito di atas biasa | **Invariant keselamatan** | **Wajib.** Urutannya tidak boleh diserahkan pada selera tampilan |
| `LAB-FE-008` | Penanda cito pada layar pesanan | Konvensi project | `DEV_DISCRETION`, asalkan hanya dokter pemesan yang dapat menandainya |
| `LAB-FE-009` | Menampilkan seluruh pemeriksaan yang ditopang satu wadah sebelum tombol tolak | **Invariant keselamatan** | **Wajib ada.** Bentuk visualnya bebas, keberadaannya tidak |
| `LAB-FE-010` | Peringatan bahwa menolak wadah menggugurkan seluruh pemeriksaannya | **Invariant keselamatan** | **Wajib muncul** sebelum penolakan dikonfirmasi |
| `LAB-FE-011` | Isian batas kritis ditampilkan sebagai **pengajuan**, bukan penyimpanan langsung | **Invariant keselamatan** | **Wajib.** Tidak boleh ada jalur simpan langsung untuk batas kritis |
| `LAB-FE-012` | Penanda terkunci pada kolom kesalahan internal dan kolom wajib catatan | **Invariant keselamatan** | **Wajib terlihat**, bukan sekadar gagal saat disimpan |
| `LAB-FE-013` | Bentuk isian batas nilai mengikuti bentuk hasil | Konvensi project | Wajib mengikuti; bentuk visualnya `DEV_DISCRETION` |
| `LAB-FE-014` | Penempatan menu data induk | Konvensi project | **Wajib** mengikuti konvensi frontend yang sudah ada: seluruh menu data induk berada di `health-services/master-data/`. `LAB-DEC-034` **tidak berlaku** di frontend |

### Yang **tidak** ditetapkan di sini dan sengaja dibiarkan terbuka

| Hal | Alasan |
|---|---|
| Nama menu yang dibaca pengguna | Belum ada arahan produk. Developer memakai penamaan yang konsisten dengan modul lain |
| Susunan kolom pada tabel | `DEV_DISCRETION` |
| Apakah memakai modal atau halaman terpisah | `DEV_DISCRETION` |
| Warna penanda cito dan penanda kritis | `DEV_DISCRETION`, asalkan dapat dibedakan pengguna dengan gangguan penglihatan warna |

---

## 6. Aksesibilitas dan Perilaku Responsif

| Aspek | Ketentuan |
|---|---|
| Pembedaan penanda | Penanda cito dan penanda kritis **tidak boleh** dibedakan hanya dengan warna. Wajib disertai teks atau ikon |
| Tabel lebar | Daftar kerja dan daftar batas nilai wajib dapat digulir mendatar di layar kecil, tanpa memaksa seluruh halaman ikut bergulir |
| Barcode | Ditampilkan sebagai teks yang dapat disalin, bukan hanya gambar |
| Bahasa | Seluruh label dan pesan dalam Bahasa Indonesia |

---

## 7. Privasi di Sisi Tampilan

| Data | Ketentuan |
|---|---|
| Catatan penolakan dan alasan ambil ulang | Bertanda sensitif pada kamus data. **Tidak** ditampilkan pada layar yang dapat dilihat petugas non-klinis, misalnya papan pemantauan umum |
| Barcode wadah | Tidak memuat identitas pasien. **Tidak boleh** ditambahi nama pasien pada label yang dicetak dari frontend |

---

## 8. Ketergantungan Pengujian

| Yang diuji | Bergantung pada |
|---|---|
| Layar pesanan dan cito | Grup endpoint Lab Order tersedia |
| Layar wadah dan pemeriksaan | Grup endpoint Lab Specimen dan Lab Examination tersedia |
| Layar daftar kerja | Grup endpoint Lab Worklist tersedia, dan data batas waktu cito sudah terisi |
| Layar batas nilai | Grup endpoint Lab Value Bound dan Lab Critical Bound Approval tersedia |
| Layar alasan penolakan | Grup endpoint Lab Rejection Reason tersedia |

Seluruh layar bergantung pada kewenangan yang sudah terdaftar. Karena pendaftarannya otomatis
lewat `AccessMenuSeeder`, layar hanya dapat diuji setelah backend dijalankan sekurang-kurangnya
satu kali dengan controller barunya.

---

## 9. Layar yang Sengaja Tidak Dibuat

| Layar | Alasan |
|---|---|
| Pengisian dan validasi hasil | Slice `S4` terblokir `LAB-SIGN-001` |
| Daftar pantau nilai kritis dan formulir pelaporan | Slice `S5` terblokir |
| Layar koreksi hasil | Slice `S6` terblokir |
| Kotak pemberitahuan dokter | Slice `S8` terblokir `LAB-COORD-001`, dan kepemilikannya ada di platform |
| Penyuntingan pesanan oleh dokter | Slice `S1b` terblokir `LAB-AMD-001` |

Kelimanya **tidak boleh** dibangun lebih dulu "sekalian", karena perilakunya belum diputuskan.
