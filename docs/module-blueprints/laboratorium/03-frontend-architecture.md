# Laboratorium — Arsitektur Frontend

| Field | Value |
|---|---|
| Blueprint ID | `LAB-BP-001` |
| Revision | `5` |
| Status | `draft` |
| Scope | Slice `S1a`, `S2`, `S3`, `S7`, `S10`, `S11`, `S13a`, `S14`, `S15`. **Revision 4 menambah menu Penerimaan Sampling/Specimen** (bagian 10). **Revision 5 menyerap `LAB-DEC-048`**: butir menu Pesanan Laboratorium dicabut, Monitoring dinamai ulang menjadi Pemeriksaan, dan disiplin diturunkan dari pemeriksaan yang dipilih |
| Frontend SHA | Revision 1-3: `688daff90`. **Revision 4: `9cd4cd03f`** — fakta `F5` dicabut capability map revision 3 |
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

---

## 10. Amandemen 2026-09-14 — Menu Penerimaan Sampling/Specimen

Menurunkan `LAB-DEC-045` beserta `LAB-FE-009`, `LAB-FE-010`, dan `LAB-FE-011` dari decision log
revision 26.

> **Keadaan awal sudah berubah total.** Catatan pembuka dokumen ini menyatakan frontend
> Laboratorium tidak ada sama sekali pada `688daff90`. Pada `9cd4cd03f` keadaannya terbalik:
> **31 berkas, 3.751 baris**, dan seluruh layar `MVP-1` sudah berdiri. Capability map revision 3
> mencabut fakta `F5`.

### 10.1 Dua jalur masuk, bukan satu

| Jalur | Menu | Dipakai untuk | Titik kunci daftar pemeriksaan |
|---|---|---|---|
| Pesanan dokter | **Tanpa butir menu sendiri** sejak `LAB-DEC-048`. Layar `lab-orders` dicapai dari ketiga menu `Pemeriksaan` | Pasien poliklinik, rawat inap, dan IGD | Sampel pertama `Collected` |
| **Penerimaan langsung** | **`Penerimaan Sampling/Specimen`, baru** | Pasien rujukan luar dan datang langsung | **Penetapan kelayakan wadah** |

Layar lama **tidak dihapus dan tidak diubah perilakunya** (`AC-76`). Keduanya memakai data dan
aturan yang sama; yang berbeda urutan penyajian dan titik kuncinya (`LAB-DEC-039`).

> **Diamandemen `LAB-DEC-048` pada 2026-09-14.** Butir menu **Pesanan Laboratorium dicabut**,
> dan ketiga menu `Monitoring` dinamai ulang menjadi **`Pemeriksaan`** Patologi Klinik, Patologi
> Anatomi, dan Mikrobiologi. Jalan masuk ke layar `lab-orders` berpindah: pembuatan pesanan dari
> pendaftaran pasien laboratorium, dan detail pesanan dari baris pada ketiga layar `Pemeriksaan`
> (`LAB-FE-012`).
>
> Disiplin pesanan juga berhenti ditanyakan kepada petugas — ia diturunkan dari pemeriksaan yang
> dipilih (`LAB-FE-013`). Sebelumnya disiplin boleh dikosongkan, dan pesanan berdisiplin kosong
> **tidak pernah muncul** di menu disiplin mana pun.

### 10.2 Peta butir menu

| Butir menu | Tingkat | Induk | Route | Layar | Hak akses penjaga |
|---|:---:|---|---|---|---|
| ~~Pesanan Laboratorium~~ | — | — | `.../lab-orders` | **Butir menunya dicabut** `LAB-DEC-048`; layarnya tetap ada dan dicapai dari menu `Pemeriksaan` | `LabOrder : Read` |
| Pemeriksaan Patologi Klinik | 2 | Laboratory Management | `.../lab-monitoring/clinical-pathology` | Daftar pemeriksaan per disiplin | `LabMonitoring : Read` |
| Pemeriksaan Patologi Anatomi | 2 | Laboratory Management | `.../lab-monitoring/anatomic-pathology` | Daftar pemeriksaan per disiplin | `LabMonitoring : Read` |
| Pemeriksaan Mikrobiologi | 2 | Laboratory Management | `.../lab-monitoring/microbiology` | Daftar pemeriksaan per disiplin | `LabMonitoring : Read` |
| Penerimaan Sampling/Specimen | 2 | Laboratory Management | `health-services/laboratory-management/specimen-receptions` | Daftar penerimaan | `LabSpecimen : Read` |
| — (layar anak) | — | Penerimaan Sampling/Specimen | `.../specimen-receptions/create` | Formulir penerimaan | `LabSpecimen : Plan` |
| — (layar anak) | — | Penerimaan Sampling/Specimen | `.../specimen-receptions/[slug]` | Detail penerimaan | `LabSpecimen : Read` |
| Jenis Specimen | 2 | Health Services / Master Data | `health-services/master-data/lab-specimen-types` | Pengelolaan jenis specimen | `LabSpecimenType : Read` |
| — (layar anak) | — | Jenis Specimen | `.../lab-specimen-types/other-usage` | Daftar pantau pemakaian `Lainnya` | `LabSpecimenType : Read` |

**Kenapa Jenis Specimen berada di `master-data/`, bukan di folder Laboratorium.** `AC-49`
membedakan kedua sisi dengan sengaja: di **backend** data induk khusus Laboratorium tinggal di
folder Laboratorium, sedangkan di **frontend** seluruh menu data induk tetap di
`health-services/master-data/` mengikuti konvensi yang sudah ada (`LAB-DEC-034` butir frontend,
dipersempit revision 17). Jadi `LabSpecimenType.cs` ada di folder Laboratorium, tetapi menunya
ada bersama data induk lain.

**Nama route `specimen-receptions` bersifat usulan.** Penamaan akhir mengikuti `LAB-FE-001` —
pola modul Health Services yang sudah ada. Yang **tidak** boleh berubah adalah jumlah menunya:
dua jalur masuk tetap dua butir menu, tidak disatukan (`LAB-FE-009`).

### 10.3 Skema fitur — Formulir Penerimaan

```text
+-----------------------------------------------------------------+
| A. Identifikasi Pasien                                          |
|    NIK / No. RM . hasil pencarian . tombol daftarkan baru       |
+-----------------------------------------------------------------+
| B. Data Pemeriksaan                                             |
|    kategori lab . instalasi perujuk . dokter . diagnosa awal    |
|    tanggal registrasi . [metode pembayaran - baca-saja]         |
+-------------------------------+---------------------------------+
| C. Wadah / Specimen           | D. Daftar Pemeriksaan           |
|    jenis specimen             |    nama . harga . subtotal      |
|    keterangan (bila Lainnya)  |    cito . hapus                 |
|    volume + satuan            |                                 |
|    waktu penerimaan fisik     |    -- Grand Total --            |
+-------------------------------+---------------------------------+
| E. Penetapan Kelayakan   [ Layak ]  [ Tidak Layak ]             |
|    PERINGATAN: setelah ditetapkan, daftar pemeriksaan terkunci  |
+-----------------------------------------------------------------+
```

| Wilayah | Isi | Sumber data | Hak akses penjaga tombol | Keadaan kosong | Keadaan gagal |
|---|---|---|---|---|---|
| A | Pencarian dan identifikasi pasien | `GET /lab-patient-registrations/patient-search` | `LabPatientRegistration : Read`; daftar baru `: Create` | "Belum ada pasien yang cocok. Periksa kembali NIK atau No. RM." | Pesan dari Registrasi diteruskan apa adanya (`VAL-40`) |
| B | Data pemeriksaan dan perujuk | `GET /referral-institutions/options`; grup Lab Catalog | `LabPatientRegistration : Create` | "Daftar instansi perujuk masih kosong." | **Lihat 10.6** — dua bagian masih terbuka |
| C | Wadah dan specimen | `GET /lab-specimen-types/options`; daftar satuan ber-`IsForLaboratory` | `LabSpecimen : Plan` | "Daftar jenis specimen belum diisi. Hubungi kepala instalasi." | `VAL-51` sampai `VAL-59` |
| D | Daftar pemeriksaan | `GET /lab-catalog/examinations`; `POST /lab-examinations` | `LabExamination : Create` dan `: Delete` | "Belum ada pemeriksaan dipilih." | `VAL-46`, `VAL-07` |
| E | Penetapan kelayakan | `POST /lab-specimens/{id}/accept` dan `/reject` | `LabSpecimen : Accept` | — | `VAL-18` bila wadah sudah diputuskan |

### 10.4 Empat hal yang wajib, bukan `DEV_DISCRETION`

| Butir | Yang wajib | Yang bebas |
|---|---|---|
| `LAB-FE-010` | Wilayah **C dan D terlihat berdampingan** sebelum kelayakan ditetapkan | Tata letaknya — berdampingan mendatar, bertumpuk, atau dua kolom |
| `LAB-FE-010` | **Peringatan penguncian** terlihat sebelum tombol kelayakan ditekan | Bentuk visual peringatannya |
| `LAB-FE-011` | Metode pembayaran **baca-saja**; tidak ada kotak pilihan pada jalur rujukan | Bentuk tampilannya |
| `LAB-FE-009` | Penamaan menu membuat jelas siapa memakai jalur yang mana | Kata-kata persisnya |

**Kenapa C dan D wajib berdampingan.** `LAB-DEC-039` mengunci daftar pemeriksaan tepat pada
penetapan kelayakan, dan `AC-37` menerbitkan kelayakan tagih pada detik yang sama. Petugas perlu
melihat tabung dan daftarnya sekaligus: ia memilih tiga pemeriksaan, melihat sampelnya hanya
cukup untuk dua, menghapus satu, **lalu** menetapkan layak. Bila keduanya terpisah layar,
penguncian datang tanpa peringatan yang terlihat.

### 10.5 Kolom Jumlah **tidak dibuat**

> **Diamandemen `LAB-DEC-050` pada 2026-09-14.** Bagian ini semula merancang kolom Jumlah/Qty
> sebagai alat bantu isi cepat yang memecah diri menjadi beberapa baris. **Rancangan itu
> dicabut.**

**Kenapa.** `BE-LAB-23` menemukan `LabExamination` punya index unik `(SpecimenId, ProcedureId)`
di tingkat database, dipasang atas dasar `BR-20` dan `AC-35`. Dua baris untuk jenis pemeriksaan
yang sama pada satu wadah tidak mungkin ada.

**Yang berlaku pada wilayah D:**

| Hal | Keadaan |
|---|---|
| Kolom **Jumlah/Qty** | **Tidak ada.** Tidak di layar, tidak di permintaan API |
| Petugas perlu dua pemeriksaan | Memilih **dua butir katalog** yang berbeda — Glukosa Puasa dan Glukosa 2 Jam PP memang dua butir terpisah |
| Pengerjaan ganda satu pemeriksaan | Penanda **`IsDuplo`** pada baris itu, sesuai `LAB-DEC-026` |

**Penguncian yang dimaksud `RULE-025`** karena itu berarti baris **tidak dapat ditambah** —
dan sesuai `LAB-DEC-049`, pembatalan **tidak ikut terkunci**.

**Satu hal yang wajib terlihat petugas.** Membatalkan pemeriksaan sesudah wadah dinyatakan
`Layak` tidak serta-merta membatalkan tagihannya; kelayakan tagihnya sudah terbit dan
koreksinya dikerjakan Billing. Layar wajib mengatakan itu pada saat tombol batal ditekan,
bukan membiarkannya menjadi kejutan di loket (`BR-44`).

### 10.6 Dua wilayah yang belum dapat diselesaikan

| Wilayah | Keadaan | Yang sudah pasti | Yang menunggu |
|---|---|---|---|
| B — instansi perujuk belum terdaftar | Terblokir `LAB-COORD-006` | Nama perujuk **tidak pernah** diketik bebas (`AC-69`) | Bentuk layar usulan dan endpointnya |
| B — metode pembayaran | Terblokir `LAB-COORD-007` | Baca-saja; saat sumbernya tidak terjawab, wajib menulis *belum dapat ditentukan*, bukan dikosongkan (`LAB-FE-011`) | Sumber datanya |

Keduanya digambar pada skema 10.3 sebagai wilayah yang **ada**, supaya tata letaknya tidak perlu
dirombak ketika `LAB-REQ-005` dijawab. Isinya belum dikunci.

### 10.7 Layar yang sengaja tidak dibuat

| Yang dipertimbangkan | Kenapa ditolak |
|---|---|
| Satu layar untuk kedua jalur masuk | `LAB-DEC-045`. Titik kuncinya berbeda; satu layar yang perilakunya berubah-ubah tanpa alasan yang terlihat lebih membingungkan daripada dua layar |
| Route `lab-specimens` tersendiri | Penanganan wadah per pesanan sudah dicapai lewat `lab-orders/[slug]/specimens`. Menambah jalan kedua ke layar yang sama tidak menambah kemampuan |
| Menu Jenis Specimen di dalam folder Laboratorium | `AC-49` — seluruh menu data induk frontend berada di `health-services/master-data/` |
| Tombol `Pemeriksaan Diproses` tersendiri | `AC-56`. Penguncian melekat pada aksi penetapan kelayakan yang sudah ada |
