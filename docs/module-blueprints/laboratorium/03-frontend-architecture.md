# Laboratorium — Arsitektur Frontend

| Field | Value |
|---|---|
| Blueprint ID | `LAB-BP-001` |
| Revision | `8` |
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
| `LAB-FE-014` | Penempatan menu data induk | Konvensi project — **diamandemen 2026-09-24** (decision log revisi 54) | Data induk yang **khusus Laboratorium** berada di `health-services/laboratory-management/master-data/` dan tampil sebagai sub-grup **Master Data** pada menu Laboratorium, mengikuti pola `billing-management/master-data/`. Data induk **global** yang juga dipakai Laboratorium — Prosedur, Tarif, Satuan Ukur — tetap di `health-services/master-data/`. *(Bunyi semula: seluruh menu data induk di `health-services/master-data/`, dan `LAB-DEC-034` tidak berlaku di frontend.)* |

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
| ~~Pengisian dan validasi hasil~~ | **Diperbarui 2026-09-18.** `LAB-SIGN-001` ditutup `LAB-DEC-079`. **Pengisian** hasil Patologi Klinik sudah dibangun (`S4a`, `FE-LAB-23`); pengisian Mikrobiologi dan Patologi Anatomi dirancang pada **bagian 12**. **Validasi dan rilis tetap tidak dibuat** — `S4`, `S4d`, `S4e` tertahan `DEC-LAB-011` |
| Daftar pantau nilai kritis dan formulir pelaporan | Slice `S5` terblokir `LAB-P0-004`, `LAB-OPEN-014`, `DEC-LAB-012` |
| Layar koreksi hasil | Slice `S6` terblokir `DEC-LAB-014` |
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

---

## 11. Amandemen 2026-09-15 — Pemesanan per disiplin dari pendaftaran

Menurunkan `LAB-DEC-055` dan `LAB-DEC-056`. Bagian kiosk **tidak dirancang di sini** karena
`LAB-DEC-051`..`LAB-DEC-054` berstatus `draft` dan tertahan `LAB-COORD-008` serta
`LAB-COORD-009`.

### 11.1 Layar yang berubah

| Layar | Status | Perubahan |
|---|---|---|
| `health-services/laboratory-management/lab-patient-registrations` | `Diperbarui` | Sesudah identitas pasien terisi, petugas memilih **daftar pemeriksaan** pada satu pemilih, lalu menekan simpan **sekali** |
| Ketiga layar Pemeriksaan (PK, PA, Mikrobiologi) | **Tidak berubah** | Sudah menyaring `LabOrder.Discipline`; pemecahan membuat pasien muncul di menu yang benar tanpa satu baris kode layar pun berubah |
| `lab-orders/create` | **Tidak berubah** | Jalur lama tetap ada dan tetap membuat satu pesanan |

### 11.2 Kewenangan UI yang ditetapkan

| Butir | Ketentuan |
|---|---|
| Disiplin | **Tidak ditanyakan sama sekali.** `LAB-DEC-048` butir 6 sudah mencabut kotak pilihannya; layar hanya menampilkan hasil pemecahan sesudah simpan |
| Hasil pemecahan | Layar **wajib memberi tahu** bahwa pilihan tadi menjadi lebih dari satu pesanan, beserta disiplin masing-masing. Petugas yang menekan simpan sekali lalu melihat dua nomor pesanan tanpa penjelasan akan mengira sistemnya salah |
| Pemeriksaan belum digolongkan | Bila ada, layar menyebutkannya apa adanya: pemeriksaan itu tersimpan tetapi **tidak akan muncul di menu disiplin mana pun** sampai katalognya digolongkan (`AC-85`, `AC-87`) |
| Cito | Ditandai per pemeriksaan pada pemilih yang sama, bukan per pesanan (`LAB-DEC-026`) |

**Kenapa butir kedua masuk kewenangan UI, bukan sekadar saran.** Pemecahan adalah satu-satunya
tempat pada modul ini di mana **satu tindakan petugas menghasilkan lebih dari satu objek
bisnis**. Tanpa pemberitahuan, satu-satunya cara petugas mengetahuinya adalah menemukan sendiri
dua baris di layar lain — dan dugaan pertama yang wajar adalah ia tidak sengaja menekan simpan
dua kali.

### 11.3 Yang sengaja tidak dibuat

| Yang dipertimbangkan | Kenapa ditolak |
|---|---|
| Pemilih disiplin pada layar pendaftaran | `LAB-DEC-048` butir 6 mencabutnya; menanyakan ulang jawaban yang sudah ada di katalog hanya menambah kesempatan menjawab keliru |
| Layar tersendiri untuk daftar pemeriksaan terpesan | Isinya sudah terlihat pada detail pesanan dan pada layar penerimaan specimen |
| Tombol "pecah pesanan" manual | Pemecahan diturunkan dari data, bukan dari keputusan petugas. Tombolnya akan menawarkan pilihan yang tidak boleh ada |

---

## 12. Amandemen 2026-09-18 — Pengisian hasil Mikrobiologi dan Patologi Anatomi (`S4b`, `S4c`)

Menurunkan `LAB-DEC-027` (BR-23), `LAB-DEC-080`, `LAB-DEC-081`, dan `LAB-DEC-084`; arsitektur
domain `LAB-DA-001` revision 6. Menyertai usulan `LAB-API-v1` `r24` yang **belum disetujui**.

### 12.1 Batas layar, dan ia sempit

| Yang dirancang | Yang **TIDAK** dirancang |
|---|---|
| Layar **mengisi** hasil Mikrobiologi dan Patologi Anatomi | Tombol **Validasi** dan **Rilis** — `S4`, `S4d`, `S4e` tertahan `DEC-LAB-011` |
| Dua layar data induk baru | Penanda nilai kritis pada hasil — `INV-28` |
| — | Penanda `Definitif` — `LAB-DEC-081` |
| — | **Lampiran gambar** pada laporan Patologi Anatomi — `DEC-LAB-016` |

> **Satu larangan tampilan yang wajib dipegang, dan ia paling mudah dilanggar tanpa sadar.**
> Layar **tidak boleh** menampilkan "status hasil" dalam bentuk apa pun — tidak sebagai lencana,
> tidak sebagai kolom, tidak sebagai penanda alur. `LAB-DEC-080` menetapkan nol status hasil
> disimpan, dan menampilkannya di layar akan **melahirkan status itu di kepala pengguna**
> walaupun tidak ada di basis data. Yang boleh ditampilkan: *"sudah diisi"* / *"belum diisi"*,
> diturunkan dari `resultEnteredAt`.

### 12.2 Peta butir menu

| Butir menu | Tingkat | Induk | Route | Layar | Hak akses penjaga |
|---|:---:|---|---|---|---|
| — (layar anak) | — | Pemeriksaan Mikrobiologi | `.../lab-monitoring/microbiology/[slug]/result` | Pengisian hasil Mikrobiologi | `LabExamination : Update` |
| — (layar anak) | — | Pemeriksaan Patologi Anatomi | `.../lab-monitoring/anatomic-pathology/[slug]/result` | Pengisian laporan Patologi Anatomi | `LabExamination : Update` |
| Organisme | 2 | Health Services / Master Data | `health-services/master-data/lab-organisms` | Pengelolaan daftar organisme | `LabOrganism : Read` |
| Antibiotik | 2 | Health Services / Master Data | `health-services/master-data/lab-antibiotics` | Pengelolaan panel antibiotik | `LabAntibiotic : Read` |

**Nol butir menu baru pada folder Laboratorium.** Kedua layar hasil dicapai dari baris pada menu
`Pemeriksaan` yang sudah ada — jalan masuk yang sama dengan `FE-LAB-23` pada Patologi Klinik.

**Kedua menu data induk berada di `master-data/`**, mengikuti `LAB-FE-014` dan konvensi yang
sudah dipakai `Jenis Specimen`. Modelnya tetap di folder Laboratorium; menunya tidak.

> **Diamandemen 2026-09-24 (decision log revisi 54).** Kedua layar — beserta seluruh data induk
> yang khusus Laboratorium — kini berada di `health-services/laboratory-management/master-data/`
> dan tampil pada sub-grup Master Data menu Laboratorium. Alamat lama diarahkan ke alamat baru.

### 12.3 Skema layar — Pengisian hasil Mikrobiologi

```text
+-----------------------------------------------------------------+
| A. Identitas pemeriksaan  [baca-saja]                           |
|    pasien . no. order . nama pemeriksaan . waktu diperiksa      |
+-----------------------------------------------------------------+
| B. Status temuan   ( ) Normal   ( ) Positif   ( ) Negatif       |
+-----------------------------------------------------------------+
| C. Isolat yang ditemukan                     [ + Tambah isolat ]|
|  +-----------------------------------------------------------+  |
|  | Isolat 1   [ organisme v ]  catatan ........  [ hapus ]   |  |
|  |   Kepekaan antibiotik            [ + Tambah antibiotik ]  |  |
|  |   +-------------------------------------------------+     |  |
|  |   | antibiotik v | kadar | zona (mm) | R/I/S | hapus |     |  |
|  |   +-------------------------------------------------+     |  |
|  +-----------------------------------------------------------+  |
+-----------------------------------------------------------------+
| D. [ Simpan hasil ]                                             |
|    Nol tombol Validasi. Nol tombol Rilis.                       |
+-----------------------------------------------------------------+
```

| Wilayah | Sumber data | Hak akses penjaga | Keadaan kosong | Keadaan gagal |
|---|---|---|---|---|
| A | `GET /lab-examinations/{id}` | `LabExamination : Read` | — | — |
| B | — | `LabExamination : Update` | — | `VAL-84` bila bentuk hasilnya tidak cocok |
| C organisme | `GET /lab-organisms/options` | `LabOrganism : Read` | "Daftar organisme belum diisi. Hubungi kepala instalasi." | `VAL-85` |
| C antibiotik | `GET /lab-antibiotics/options` | `LabAntibiotic : Read` | "Panel antibiotik belum diisi. Hubungi kepala instalasi." | `VAL-86`, `VAL-87`, `VAL-90` |
| D | `PUT /lab-examinations/{id}/result/microbiology` | `LabExamination : Update` | — | `VAL-83`, `VAL-89` |

> **Nol isolat adalah hasil yang sah.** Layar **tidak boleh** memaksa petugas menambahkan
> sekurang-kurangnya satu baris. Biakan yang tidak menumbuhkan apa pun adalah temuan, bukan
> formulir yang belum selesai — dan memaksa satu baris akan melahirkan isolat palsu.

### 12.4 Skema layar — Pengisian laporan Patologi Anatomi

```text
+-----------------------------------------------------------------+
| A. Identitas pemeriksaan  [baca-saja]                           |
+-----------------------------------------------------------------+
| B. Makroskopik   * wajib                                        |
|    [ area teks panjang ]                                        |
+-----------------------------------------------------------------+
| C. Mikroskopik   * wajib                                        |
|    [ area teks panjang ]                                        |
+-----------------------------------------------------------------+
| D. Kesimpulan    * wajib                                        |
|    [ area teks panjang ]                                        |
+-----------------------------------------------------------------+
| E. [ Simpan laporan ]                                           |
|    Gambar contoh BELUM ADA - lihat DEC-LAB-016                  |
+-----------------------------------------------------------------+
```

**Ketiganya wajib, dan penandanya wajib terlihat sebelum tombol ditekan** — bukan muncul sebagai
pesan galat setelah petugas mengira laporannya tersimpan (`VAL-88`, `INV-25`).

**Nol jalur simpan sebagian.** Tidak ada tombol "Simpan draft" pada layar ini. `INV-25`
menetapkan laporan tanpa kesimpulan tidak sah, dan menyediakan draft berarti menyediakan tempat
bagi laporan yang tidak sah untuk menunggu tanpa batas.

### 12.5 Kewenangan UI yang ditetapkan

| ID | Area | Tingkat wewenang | Ruang gerak |
|---|---|---|---|
| `LAB-FE-015` | **Nol status hasil ditampilkan** dalam bentuk apa pun | **Invariant** | **Wajib.** Turunan `LAB-DEC-080`. "Sudah diisi" diturunkan dari `resultEnteredAt`, bukan dari lencana status |
| `LAB-FE-016` | **Nol tombol Validasi dan Rilis** pada kedua layar | **Invariant keselamatan** | **Wajib.** Menyediakannya berarti menjanjikan wewenang yang belum ditetapkan (`DEC-LAB-011`) |
| `LAB-FE-017` | Organisme dan antibiotik **dipilih dari daftar**, nol pengetikan bebas | **Invariant** | **Wajib.** Turunan `LAB-DEC-084` dan `INV-30`. Komponen pemilihnya `DEV_DISCRETION` |
| `LAB-FE-018` | Ketiga ruas narasi Patologi Anatomi **bertanda wajib sebelum tombol ditekan** | **Invariant** | **Wajib terlihat.** Bentuk visualnya bebas |
| `LAB-FE-019` | Isolat dan kepekaannya **terlihat bersarang**, bukan dua daftar terpisah | Konvensi | Wajib hubungannya terlihat; tata letaknya `DEV_DISCRETION` |
| `LAB-FE-020` | **Penghapusan baris isolat meminta konfirmasi** | **Invariant** | **Wajib.** Baris yang hilang adalah temuan yang hilang; jejaknya menuntut alasan (`permission-audit-matrix` 8.4) |
| `LAB-FE-021` | Ketiga ruas narasi **tidak boleh muncul** pada layar non-klinis | **Privasi** | **Wajib.** Ia isi rekam medis, bukan catatan operasional |

**Kenapa `LAB-FE-019` bukan sekadar selera.** Kepekaan antibiotik **hanya bermakna terhadap satu
isolat tertentu** (`INV-27`). Dua daftar sejajar akan membuat petugas mengira antibiotik diuji
terhadap "pemeriksaan", bukan terhadap kuman tertentu — dan salah baca itu berujung pada terapi
yang keliru.

### 12.6 Kontrak penanganan state — tambahan

| Keadaan | Yang harus terjadi |
|---|---|
| Daftar organisme atau antibiotik kosong | Layar **menyebutkan penyebabnya dan siapa yang mengisinya**, bukan menampilkan pemilih kosong. Ini keadaan yang **pasti terjadi** pada hari pertama, sebelum data induknya diisi |
| Menyimpan hasil yang sudah pernah diisi | Diperlakukan sebagai penggantian utuh, bukan penambahan. Layar wajib memuat ulang isinya sesudah simpan |
| Baris isolat dihapus lalu disimpan | Layar wajib menunjukkan bahwa barisnya benar-benar hilang sesudah muat ulang, bukan hanya hilang dari tampilan |

### 12.7 Layar yang sengaja tidak dibuat

| Yang dipertimbangkan | Kenapa ditolak |
|---|---|
| Tombol Validasi/Rilis pada kedua layar | `DEC-LAB-011` belum dijawab. Tombol yang muncul lalu selalu gagal lebih buruk daripada tombol yang tidak ada |
| Lencana status hasil pada daftar Pemeriksaan | `LAB-FE-015`. Nol status hasil disimpan |
| Unggah gambar pada laporan Patologi Anatomi | `DEC-LAB-016` — privasi penyimpanan berkas klinis belum diputuskan |
| Penanda nilai kritis otomatis pada hasil Mikrobiologi | `INV-28`. Bakteri resisten adalah penilaian klinis, bukan perbandingan angka |
| Layar antibiogram rumah sakit | Ia **laporan**, bukan bagian pengisian hasil. Datanya baru terkumpul setelah slice ini berjalan |

### 12.8 Ketergantungan pengujian

| Yang diuji | Bergantung pada |
|---|---|
| Kedua layar hasil | Usulan `LAB-API-v1` `r24` **disetujui** lalu dibangun |
| Pemilih organisme dan antibiotik | Kedua data induk **sudah terisi** — bukan hanya tabelnya ada |

**Butir kedua bukan formalitas.** Layar pengisian hasil Mikrobiologi dengan daftar organisme
kosong **tidak dapat dipakai sama sekali**, dan tidak dapat diuji selain pada keadaan kosongnya.

---

## 13. Amandemen 2026-09-18 sore — Layar laporan Patologi Anatomi DIRANCANG ULANG

> **Bagian 12.4 dan bagian 12.7 baris gambar DICABUT sejauh menyangkut Patologi Anatomi.**
> Bagian 12 untuk Mikrobiologi — 12.3, `LAB-FE-017`, `LAB-FE-019`, `LAB-FE-020` — **tetap
> berlaku**.

Menurunkan `LAB-DA-001` rev 7 bagian A4 dan usulan `LAB-API-v1` `r25` yang **belum disetujui**.

### 13.1 Apa yang berubah dari rancangan pagi

| Bagian 12.4 (dicabut) | Bagian 13 (berlaku) |
|---|---|
| Satu layar berisi **tiga area teks tetap** | Formulir **dibangkitkan dari daftar parameter** yang dikirim server |
| Dicapai dari baris **pemeriksaan** | Dicapai dari baris **pesanan** — hasil PA melekat pada pesanan |
| Satu tombol `Simpan laporan` | `Simpan`, `Selesaikan`, dan `Buka Kembali` — tiga tindakan berbeda |
| Nol konteks klinis | Konteks klinis **ditampilkan baca-saja**; penulisnya layar pemesanan |

### 13.2 Peta butir menu

| Butir menu | Induk | Route | Layar | Hak akses |
|---|---|---|---|---|
| — (layar anak) | Pemeriksaan Patologi Anatomi | `.../anatomic-pathology/[slug]/pathology-report` | Laporan PA per pesanan | `LabExamination : Update` |
| — (bagian layar) | Pemesanan PA | pada layar pemesanan yang **sudah ada** | Konteks klinis, diisi dokter pemesan | `LabOrder : Update` |
| Parameter Patologi Anatomi | Health Services / Master Data | `.../master-data/lab-pathology-parameters` | Pengelolaan parameter | `LabPathologyParameter : Read` |
| Kategori Patologi Anatomi | Health Services / Master Data | `.../master-data/lab-pathology-categories` | Kategori, keberlakuan parameter, **dan pemetaan jenis pemeriksaan** | `LabPathologyCategory : Read` |

> **Baris kedua menyentuh layar yang SUDAH BERJALAN** (`ARCH-GAP-LAB-07`). Bagian konteks klinis
> muncul pada layar pemesanan **hanya bila disiplin pesanannya Patologi Anatomi**, dan seluruh
> ruasnya opsional — sehingga alur pemesanan disiplin lain **nol berubah**.

### 13.3 Skema layar — Laporan Patologi Anatomi

```text
+-----------------------------------------------------------------+
| A. Identitas pasien dan pesanan        [baca-saja]              |
+-----------------------------------------------------------------+
| B. Konteks klinis dari dokter pemesan  [BACA-SAJA]              |
|    Diagnosa Awal . Riwayat Penyakit . Masa Terakhir Haid        |
|    Keterangan Klinis                                            |
|    (kosong -> "Belum diisi dokter pemesan")                     |
+-----------------------------------------------------------------+
| C. Pemeriksaan pada pesanan ini        [baca-saja]              |
|    nama . kategori PA . (harga mengikuti LAB-DEC-037)           |
|    -> kategori inilah yang menentukan isi wilayah D             |
+-----------------------------------------------------------------+
| D. Isian laporan   -- DIBANGKITKAN dari daftar parameter --     |
|    [ Makroskopik * ]  [ area teks ]                             |
|    [ Mikroskopik * ]  [ area teks ]                             |
|    [ Kesimpulan  * ]  [ area teks ]                             |
|    ... ruas IHK muncul bila pesanan memuat kategori IHK         |
|    * = wajib menurut data induk keberlakuan                     |
+-----------------------------------------------------------------+
| E. Status temuan  ( ) Normal ( ) Perlu Perhatian ( ) Kritis     |
|    Penanggung Jawab Analis [ pilih v ]                          |
+-----------------------------------------------------------------+
| F. [ Simpan ]  [ Selesaikan ]        [ Buka Kembali ]           |
|    Waktu Efektif : {turunan}   Waktu Issued : {turunan}         |
|    Nol tombol Validasi. Nol tombol Rilis. Nol kirim ke pasien.  |
+-----------------------------------------------------------------+
```

| Wilayah | Sumber data | Keadaan kosong | Keadaan gagal |
|---|---|---|---|
| B | `GET /lab-orders/{id}/pathology-context` | "Belum diisi dokter pemesan" — **bukan area kosong tanpa penjelasan** | — |
| C + D | `GET /lab-orders/{id}/pathology-report` | **`VAL-100`** — "Jenis pemeriksaan pada pesanan ini belum digolongkan… Hubungi kepala instalasi." | `VAL-92` |
| D simpan | `PUT /lab-orders/{id}/pathology-report` | — | `VAL-93`, `VAL-94`, `VAL-96`, `VAL-99` |
| F Selesaikan | `POST /…/finalize` | — | **`VAL-95`** — daftar ruas yang masih kosong ditampilkan |
| F Buka Kembali | `POST /…/reopen` | — | `VAL-97`, `VAL-98` |

### 13.4 Kewenangan UI yang ditetapkan

| ID | Area | Tingkat | Ruang gerak |
|---|---|---|---|
| `LAB-FE-022` | **Formulir dibangkitkan dari daftar parameter server**, bukan dari daftar ruas yang ditulis di kode | **Invariant** | **Wajib.** Turunan `LAB-DEC-086`. Menuliskan lima belas ruas di kode membuat parameter ke-16 menuntut rilis frontend |
| `LAB-FE-023` | **Nol tombol Validasi, Rilis, maupun Kirim ke Pasien** | **Invariant keselamatan** | **Wajib.** `S4e` tertahan `DEC-LAB-011` |
| `LAB-FE-024` | **`Selesaikan` terpisah dari `Simpan`**, dan disertai penegasan bahwa ia **bukan** rilis | **Invariant** | **Wajib.** `LAB-DEC-088`. Kata pada tombolnya `DEV_DISCRETION`; keberadaan pemisahannya tidak |
| `LAB-FE-025` | **Konteks klinis BACA-SAJA** pada layar ini | **Invariant** | **Wajib.** `INV-40` — penulisnya dokter pemesan |
| `LAB-FE-026` | **Waktu Efektif dan Waktu Issued baca-saja**, ditandai turunan | **Invariant** | **Wajib.** `INV-38`. Kotak isian untuk keduanya **dilarang** |
| `LAB-FE-027` | Ruas **wajib bertanda sebelum tombol ditekan**, dan `VAL-95` menampilkan daftar yang kosong | **Invariant** | **Wajib.** Formulir sampai lima belas ruas menuntutnya |
| `LAB-FE-028` | **`Buka Kembali` meminta alasan** | **Invariant** | **Wajib.** `VAL-97`; jejaknya wajib |
| `LAB-FE-029` | **Nol lencana status hasil** | **Invariant** | **Wajib.** `INV-36`. "Sudah selesai" diturunkan dari `FinalizedAt` |

**Kenapa `LAB-FE-022` bukan selera.** `LAB-DEC-086` memilih data induk parameter justru agar
bentuk hasil kelima kelak cukup menambah **baris data**. Formulir yang ruasnya ditulis di kode
membatalkan seluruh manfaat itu — dan membuat frontend menjadi tempat kedua yang harus tahu
parameter mana milik kategori mana.

### 13.5 Kontrak penanganan state — tambahan

| Keadaan | Yang harus terjadi |
|---|---|
| Pemetaan kategori belum diisi | **Keadaan yang pasti terjadi hari pertama.** Layar menyebut apa yang belum diatur **dan siapa yang mengaturnya** — bukan formulir kosong |
| Konteks klinis kosong | Ditandai "belum diisi dokter pemesan", **bukan** dibiarkan seolah tidak ada ruasnya |
| Laporan sudah final | Seluruh isian **dikunci**, dan satu-satunya jalan adalah `Buka Kembali` |
| Sesudah `Buka Kembali` | Layar memuat ulang dan menunjukkan bahwa laporan kembali dapat disunting |

### 13.6 Layar yang sengaja tidak dibuat

| Yang dipertimbangkan | Kenapa ditolak |
|---|---|
| Tombol Validasi / Rilis / Kirim ke Pasien | `DEC-LAB-011`; `LAB-FE-023` |
| Kotak isian Waktu Issued dan Waktu Efektif | `INV-38` — keduanya turunan |
| Lencana `Draft`/`Final` sebagai status | `INV-36`; `LAB-FE-029` |
| Unggah gambar pada laporan | `DEC-LAB-016` |
| Ruas HL7 | `LAB-COORD-012` |
| Pilihan bahasa dan preview Inggris | `LAB-COORD-013`, beserta izin privasinya |
| Bagian Informasi Specimen — lokasi, pola, metode | `S2b` belum siap |
| Tombol Konfirmasi DPJP/Dokter Lantai | `S5` |

---

## Amandemen 2026-09-21 — Halaman Hasil Mikrobiologi (`S4b`)

| Field | Nilai |
|---|---|
| Slice | `S4b` pengisian hasil Mikrobiologi |
| Masukan | decisions rev 50; `LAB-API-v1` `r26` |
| Frontend SHA | `ebef7ebe5` |
| Sifat | Halaman **baru**; route daftar pantau Mikrobiologi sudah ada dan tidak diubah |

### Keadaan frontend saat dirancang

Peta kemampuan revision 4 mencabut `F5`: frontend **sudah** punya modul Laboratorium — 17 route
dan 159 berkas. Yang relevan bagi slice ini:

| Sudah ada | Berkas | Dipakai bagaimana |
|---|---|---|
| Daftar pantau Mikrobiologi | `src/app/health-services/laboratory-management/lab-monitoring/microbiology/page.jsx` | Menjadi **titik masuk**; barisnya memperoleh tautan ke halaman hasil |
| Komponen daftar pantau | `components/view/health-services/laboratory-management/lab-monitoring/lab-monitoring-view` | Dipakai ulang apa adanya |
| Halaman hasil disiplin mana pun | — | **Nol.** Belum ada satu pun halaman pengisian hasil di frontend |

### Route yang ditambahkan

| Route | Kegunaan |
|---|---|
| `/health-services/laboratory-management/lab-monitoring/microbiology/[slug]` | Halaman Hasil Pemeriksaan Mikrobiologi untuk satu No. Order |

Mengikuti pola `[slug]` yang sudah dipakai `lab-orders/[slug]` dan `lab-reception-reports/[slug]`.

### Susunan halaman

Halaman **bertingkat dua**, dan itu konsekuensi langsung `LAB-DEC-095`.

| Bagian | Isi | Sifat |
|---|---|---|
| Informasi pasien | NIK, nama, No. RM, umur, jenis kelamin, tipe kunjungan, unit layanan, penjamin | Baca-saja |
| Informasi pemeriksaan | No. registrasi, No. order, DPJP, dokter lab, Penanggung Jawab Analis, tanggal mulai | Baca-saja |
| Diagnosa pemeriksaan | Empat ruas konteks klinis | Baca-saja; sumbernya `LAB-OPEN-035` |
| **Informasi Specimen** | Jenis, Spesifik Specimen, volume + satuan, keterangan, waktu terima fisik | **Dapat disunting** sampai Final; tombol riwayat perubahan |
| **Daftar pemeriksaan** | Satu baris per pemeriksaan Mikrobiologi dalam order itu | Pemilih — ini tingkat pertama |
| **Form hasil** | Status temuan, isolat, antibiogram, Cito, Duplo | Tingkat kedua, mengikuti baris yang dipilih |
| Diagnostic Report | Waktu Efektif, Waktu Issued, catatan | Kedua waktu **baca-saja** |
| Konsultasi | Penanda `Definitif`, kepada siapa, kapan | Dapat diisi |
| Tombol aksi | Simpan Draft, Simpan Final, Reopen, Preview, Print | — |

> **Kenapa daftar pemeriksaan menjadi pemilih, bukan pengulangan form.** Satu order dapat
> memuat Kultur Darah dan Kultur Urin. Menampilkan dua form utuh sekaligus membuat layar
> sangat panjang dan membuat petugas mudah mengisi antibiogram pada baris yang salah.
> Pemilih membuat **satu hasil terlihat penuh pada satu waktu**, dan baris mana yang sedang
> diisi selalu terbaca.

### Perilaku yang wajib, dan alasannya

| Perilaku | Dasar |
|---|---|
| Ruas `Analis` tampil **baca-saja**, terisi nama pengguna yang sedang bekerja | `LAB-DEC-105` |
| `Waktu Efektif` dan `Waktu Issued` tampil **baca-saja** | `LAB-DEC-096` |
| Tombol `Simpan Final` **tidak** berlabel "kirim" maupun "sahkan", dan sesudah ditekan layar menyatakan hasil **belum dirilis** | `LAB-DEC-097` |
| Ketika `criticalRuleAvailable` bernilai salah, layar menampilkan keterangan bahwa aturan kritis belum disetel — **bukan** diam | `LAB-DEC-103` butir 5 |
| Ketika `onDutyScheduleAvailable` bernilai salah, pemilih dokter menampilkan pencarian dokter aktif beserta keterangan bahwa jadwal jaga belum tersedia | `LAB-DEC-111` |
| Menyimpan hasil **tanpa satu pun isolat** diizinkan dan tidak memunculkan peringatan | `LAB-DEC-104` |
| Volume specimen selalu berpasangan: satu kotak angka, satu pemilih satuan | `LAB-DEC-100` |
| Tombol riwayat perubahan pada Informasi Specimen menampilkan nilai lama | `LAB-DEC-112` |
| Sesudah Final, seluruh Informasi Specimen menjadi baca-saja | `LAB-DEC-107` butir 3 |

> **Butir "bukan diam" perlu ditekankan.** Penanda kritis yang tidak menyala terlihat **persis
> sama** dengan penanda yang belum punya aturan. Petugas yang melihat layar bersih akan
> menyimpulkan hasilnya aman — padahal yang terjadi adalah `DR-LAB-002` belum mengisi satu pun
> baris aturan. `AC-166` menguji tepat ini.

### Yang TIDAK dibangun pada slice ini

| Yang ditolak | Alasan |
|---|---|
| Tombol validasi dan rilis | `S4d`, tertahan `DEC-LAB-011` |
| Tombol kirim hasil ke pasien | `LAB-COORD-011`; dan `LAB-DEC-097` menegaskan Final bukan rilis |
| Preview dan cetak Bahasa Inggris | `LAB-COORD-013` |
| Pilihan `HL7` | `LAB-DEC-109` |
| Tombol menambah Spesifik Specimen baru dari halaman hasil | `LAB-DEC-098` butir 5 — hanya kepala instalasi yang menaikkan nilai tetap |
| Tata letak cetak yang pasti | `LAB-OPEN-039` — kedua screenshot belum diserahkan |

### Kewenangan keputusan UI

Mengikuti hierarki `LAB-DEC-010`. Yang **bukan** kewenangan developer karena ia menyangkut
keselamatan atau invariant:

- label tombol `Simpan Final` dan pernyataan "belum dirilis" sesudahnya;
- keterangan ketika aturan kritis belum disetel;
- keterangan ketika jadwal jaga belum tersedia;
- sifat baca-saja pada `Analis`, `Waktu Efektif`, dan `Waktu Issued`.

Selebihnya — susunan kolom, jenis kontrol, penempatan tombol riwayat, gaya tabel antibiogram —
**`DEV_DISCRETION`**, mengikuti `page-composition-patterns` dan `base-component-catalog`.

---

## Amandemen 2026-09-21 (kedua) — layar `S4b` sesudah bukti cetak

Menurunkan decision log **rev 52** dan `LAB-API-v1` `r27`.

### Yang bertambah pada layar hasil

| Bagian | Perilaku | Dasar |
|---|---|---|
| Kualifikasi hasil | Pemilih `Definitif` / `Sementara`, tampil dekat judul karena ia yang dicetak paling menonjol | `LAB-DEC-114` |
| Jenis biakan | Pemilih Bakteri / Jamur — **mengubah kata pada pratinjau label**, bukan bentuk tabel | `LAB-DEC-124` |
| Metode uji | Pemilih Difusi cakram / Dilusi — **mengubah kolom mana yang tampil** | `LAB-DEC-124` |
| Tabel antibiogram | Difusi: `Antibiotik`, `UG`, `R-S`, `Zona/mm`, `Hasil`. Dilusi: `Antibiotik`, `Kadar`, `Satuan`, `Hasil` | `LAB-DEC-122`, `124` |
| Kolom `UG` dan `R-S` | **Baca-saja**, terisi dari data induk | `LAB-DEC-122` |
| Kolom `Hasil` | **Terisi sendiri** begitu zona diketik; dapat ditimpa, dan penimpaan **membuka kotak alasan yang wajib** | `LAB-DEC-123` |
| Isolat tanpa uji | Penanda pada baris isolat; mencentangnya **menyembunyikan tabel antibiogram** baris itu | `LAB-DEC-126` |
| Bagian set bakteri | **Tidak tampil sama sekali** bila profil pemeriksaan menyatakan tidak memakainya | `LAB-DEC-125` |

### Perilaku yang wajib, dan alasannya

| Perilaku | Dasar |
|---|---|
| Ketika `breakpointAvailable` bernilai salah, kolom `Hasil` kembali dapat diketik **disertai keterangan** bahwa breakpoint belum disetel | `LAB-DEC-123` |
| Zona `0` dapat diketik dan **berbeda tampilannya** dari kolom yang dikosongkan | `LAB-DEC-128` |
| Kolom `Hasil` yang ditimpa **ditandai terlihat**, beserta nilai hitungan aslinya | `LAB-DEC-123` |
| `Petugas Otorisasi` pada pratinjau cetak tampil **kosong** selama hasil belum dirilis | `LAB-DEC-120` |

> **Dua keterangan "bukan diam" kini ada dua, dan keduanya bukan `DEV_DISCRETION`:** aturan
> kritis belum disetel (`r26`), dan breakpoint belum disetel (`r27`). Keduanya membuat layar
> terlihat bersih padahal sesuatu belum siap.

### Yang TIDAK dibangun

Susunan dua isolat berantibiogram, tampilan hasil nol pertumbuhan, dan pengulangan kop pada
halaman kedua — **ketiganya belum pernah terlihat** (`LAB-OPEN-039`), dan menebaknya berarti
mengulang kesalahan `LAB-DEC-116`.
