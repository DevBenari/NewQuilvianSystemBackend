# Validation Matrix — Modul Laboratorium

| Field | Value |
|---|---|
| Contract version | `LAB-VAL-v1` |
| Revision | `8` |
| `r8` approved_by / approved_at | Yoga Aji Pratama (`yogaaji452@gmail.com`) / **2026-09-18** |
| Isi amandemen `r8` | **`approved` — 2026-09-18.** Sebelas aturan `VAL-92`..`VAL-102` untuk laporan Patologi Anatomi **per pesanan**, menurunkan `LAB-DEC-085`..`LAB-DEC-088`, `LAB-DEC-091`, dan `LAB-DA-001` rev 7. **Satu aturan DICABUT: `VAL-88`** — ia menuntut tiga nama kolom yang dihardcode (makroskopik, mikroskopik, kesimpulan), sedangkan kewajiban ruas kini **bergantung kategori** dan ditegakkan `VAL-95` terhadap data induk keberlakuan. `VAL-83`, `VAL-84`, dan `VAL-89` **tetap berlaku bagi Mikrobiologi**. **Nol aturan `VAL-01`..`VAL-87` dan `VAL-89`..`VAL-91` berubah.** Disetujui bersama `LAB-API-v1` `r25` dan `LAB-PERM-v1` rev 7 pada hari yang sama. Lihat bagian 10 |
| `r7` approved_by / approved_at | Yoga Aji Pratama (`yogaaji452@gmail.com`) / **2026-09-18** |
| Isi amandemen `r7` | **`approved` — 2026-09-18.** Sembilan aturan `VAL-83`..`VAL-91` untuk pengisian hasil Mikrobiologi dan Patologi Anatomi, menurunkan `LAB-DEC-027`, `LAB-DEC-084`, dan `LAB-DA-001` rev 6. **Nol aturan `VAL-01`..`VAL-82` berubah.** Bagian 9.1 mencatat selisih pembukuan yang ditemukan saat menulisnya: `VAL-76`..`VAL-82` lahir pada `LAB-API-v1` `r18`/`r21` tetapi tidak pernah masuk matriks ini, dan **sengaja tidak ditambal diam-diam** |
| Status | `approved` — `VAL-01`..`VAL-50` dikunci 2026-09-02; **`VAL-51`..`VAL-63` disetujui pemilik modul 2026-09-14**; **`VAL-64`..`VAL-69` disetujui pemilik modul 2026-09-15** |; **`VAL-70`..`VAL-75` disetujui pemilik modul 2026-09-15**
| `r5` approved_by / approved_at | Yoga Aji Pratama (`yogaaji452@gmail.com`) / 2026-09-15 |
| Isi amandemen `r5` | **`approved` — 2026-09-15.** Enam aturan untuk pemesanan per disiplin. `VAL-64`..`VAL-67` menjaga isi permintaan pemesanan massal; `VAL-68` dan `VAL-69` menjaga agar wadah hanya memuat pemeriksaan yang memang dipesan, dan keduanya **hanya berlaku bila pesanannya punya baris terpesan** sehingga pesanan lama tidak berubah perilakunya. Tidak satu pun aturan `VAL-01`..`VAL-63` berubah. Menurunkan `LAB-DEC-055`..`LAB-DEC-057` |
| Isi amandemen `r6` | **`approved` — 2026-09-15.** Enam aturan menurunkan `LAB-DEC-061` dan `LAB-DEC-063`: konfirmasi pesanan hanya sah sekali dan menuntut dokter pemeriksa, serta pembatalan menuntut alasan dan hanya sah pada `Requested`/`Confirmed`. **`VAL-75` satu-satunya pengetatan**; lima lainnya menjaga tindakan yang belum ada sama sekali. Tidak satu pun aturan `VAL-01`..`VAL-69` berubah |
| `r4` approved_by / approved_at | Yoga Aji Pratama (`yogaaji452@gmail.com`) / 2026-09-14 |
| Isi amandemen `r4` | Tiga belas aturan baru untuk Penerimaan Sampling/Specimen: jenis specimen terkendali beserta jalan keluar `Lainnya` yang wajib berketerangan, volume yang wajib bersatuan tanpa batas minimum/maksimum, waktu penerimaan fisik, dan `Quantity` pemeriksaan. **`VAL-60` kemudian dicabut `LAB-DEC-050` pada 2026-09-14** karena ruas `Quantity` tidak jadi dibuat. **Tidak satu pun aturan `VAL-01`..`VAL-50` berubah.** `VAL-43` dan `VAL-44` tetap berlaku apa adanya sampai `LAB-REQ-005` dijawab |
| Batas penguncian | **Terkunci penuh sejak 2026-09-02.** `LAB-OPEN-021` dijawab: penamaan memakai prefix `Lab`, sehingga tidak ada lagi bagian yang dikecualikan |
| Owner | Yoga Aji Pratama |
| `approved_by` / `approved_at` | Yoga Aji Pratama (`yogaaji452@gmail.com`) / 2026-09-02 |
| Input revision | Decisions rev 20; `LAB-DA-001` rev 4 |
| Input hash | `sha256:6504b18a327b9966526bd1df8f3cb878d7f6d6519dacc1f7df16b1066729ae82` atas `00-interview-decisions.md`, dihitung 2026-09-02 |
| Scope | Slice `S1a`, `S2`, `S3`, `S7`, `S10`, `S11`, `S13a`, `S13b`, `S14`, `S15` |
| Backend SHA | `c87d9c0` |

Pesan bagi pengguna ditulis dalam Bahasa Indonesia yang dipahami petugas, bukan istilah teknis.

---

## 1. Pesanan dan Kesegeraan

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| `VAL-01` | Membuat pesanan | Kunjungan pasien tidak ditemukan | "Kunjungan pasien tidak ditemukan. Pastikan pasien sudah terdaftar." | `404` |
| `VAL-02` | Membuat pesanan | Jenis pemeriksaan tidak berpenanda laboratorium | "Tindakan yang dipilih bukan pemeriksaan laboratorium." | `422` |
| `VAL-03` | Menandai cito | Yang menandai bukan dokter pemesan | "Hanya dokter yang membuat pesanan ini yang boleh menandainya cito." | `403` |
| `VAL-04` | Menandai cito | Pesanan sudah selesai atau dibatalkan | "Pesanan ini sudah selesai atau dibatalkan, kesegeraannya tidak dapat diubah lagi." | `409` |

---

## 2. Wadah Fisik

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| `VAL-05` | Merencanakan wadah | Tidak ada satu pun pemeriksaan disertakan | "Satu wadah harus memuat sekurang-kurangnya satu pemeriksaan." | `422` |
| `VAL-06` | Merencanakan wadah | Pesanan sudah dibatalkan | "Pesanan ini sudah dibatalkan, wadah baru tidak dapat ditambahkan." | `409` |
| `VAL-07` | Merencanakan wadah | Jenis pemeriksaan yang sama disertakan dua kali | "Pemeriksaan yang sama tidak boleh dimasukkan dua kali dalam satu wadah." | `422` |
| `VAL-08` | Menyatakan layak | Wadah belum pernah diterima di laboratorium | "Wadah ini belum tercatat tiba di laboratorium, jadi belum bisa dinyatakan layak." | `409` |
| `VAL-09` | Menyatakan layak | Petugas yang sama juga yang mengambil sampel | "Petugas yang mengambil sampel tidak boleh menyatakan kelayakannya." | `403` |
| `VAL-10` | Menolak wadah | Alasan penolakan tidak diisi | "Pilih alasan penolakan lebih dulu." | `422` |
| `VAL-11` | Menolak wadah | Alasan penolakan tidak dikenal atau sudah nonaktif | "Alasan penolakan yang dipilih tidak berlaku." | `422` |
| `VAL-12` | Menolak wadah | Alasan menuntut catatan, tetapi catatan kosong | "Alasan ini membutuhkan keterangan tambahan. Mohon isi catatannya." | `422` |
| `VAL-13` | Menolak wadah | Percobaan menolak sebagian pemeriksaan saja | "Penolakan berlaku untuk seluruh pemeriksaan pada wadah ini, karena semuanya berasal dari bahan yang sama." | `422` |
| `VAL-14` | Meminta ambil ulang | Sebab ambil ulang tidak diisi | "Pilih sebab pengambilan ulang lebih dulu." | `422` |
| `VAL-15` | Meminta ambil ulang | Sebab selain kesalahan internal, tetapi alasan kosong | "Pengambilan ulang dengan sebab ini membutuhkan alasan tertulis." | `422` |
| `VAL-16` | Seluruh perpindahan wadah | Wadah sedang diubah petugas lain | "Data ini baru saja diubah petugas lain. Mohon muat ulang lalu coba lagi." | `409` |

**Contoh `VAL-13`.** Petugas Budi membuka satu tabung serum yang menopang Fungsi hati dan
Fungsi ginjal. Serumnya keruh. Budi mencoba menolak Fungsi hati saja. Sistem menolak permintaan
itu dengan pesan di atas, karena kedua pemeriksaan berasal dari bahan yang sama — bila bahannya
tidak layak, keduanya tidak dapat dikerjakan.

---

## 3. Pemeriksaan Terpesan

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| `VAL-17` | Menambah pemeriksaan | Jenis pemeriksaan tidak berpenanda laboratorium | "Tindakan yang dipilih bukan pemeriksaan laboratorium." | `422` |
| `VAL-18` | Menambah pemeriksaan | Wadah penopang sudah dinyatakan layak atau ditolak | "Wadah ini sudah diputuskan, pemeriksaan baru tidak dapat ditambahkan ke wadah tersebut." | `409` |
| `VAL-19` | Membatalkan pemeriksaan | Pemeriksaan sudah gugur bersama wadah yang ditolak | "Pemeriksaan ini sudah gugur karena wadahnya ditolak." | `409` |
| `VAL-20` | Menambah pemeriksaan | Tarif jenis pemeriksaan tidak ditemukan | "Tarif untuk pemeriksaan ini belum diatur. Hubungi bagian data induk." | `422` |

---

## 4. Batas Nilai

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| `VAL-21` | Membuat batas nilai | Kombinasi pemeriksaan, jenis kelamin, dan kelompok umur sudah ada | "Batas nilai untuk kelompok pasien ini sudah ada. Ubah yang sudah ada, jangan membuat baru." | `409` |
| `VAL-22` | Membuat atau mengubah | Bentuk hasil angka, tetapi satuan kosong | "Pemeriksaan berhasil angka wajib punya satuan, misalnya g/dL." | `422` |
| `VAL-23` | Membuat atau mengubah | Bentuk hasil pilihan, tetapi daftar pilihan kosong | "Pemeriksaan berhasil pilihan wajib punya sekurang-kurangnya satu pilihan." | `422` |
| `VAL-24` | Membuat atau mengubah | Bentuk hasil angka, tetapi daftar pilihan diisi | "Pemeriksaan berhasil angka tidak boleh punya daftar pilihan." | `422` |
| `VAL-25` | Membuat atau mengubah | Batas normal bawah lebih besar daripada batas normal atas | "Batas normal bawah tidak boleh lebih besar daripada batas atas." | `422` |
| `VAL-26` | Membuat atau mengubah | Batas kritis bawah lebih besar daripada batas normal bawah | "Batas kritis bawah harus lebih rendah daripada batas normal bawah." | `422` |
| `VAL-27` | Membuat atau mengubah | Batas kritis atas lebih kecil daripada batas normal atas | "Batas kritis atas harus lebih tinggi daripada batas normal atas." | `422` |
| `VAL-28` | Mengubah lewat `PUT` biasa | Permintaan memuat perubahan batas kritis | "Perubahan batas kritis harus lewat pengajuan yang disetujui pihak klinis." | `422` |
| `VAL-29` | Batas waktu cito | Nilainya nol atau negatif | "Batas waktu cito harus lebih dari nol menit." | `422` |
| `VAL-30` | Menonaktifkan batas nilai | Tidak ada batas lain yang berlaku untuk pemeriksaan itu | "Ini satu-satunya batas nilai untuk pemeriksaan tersebut. Menonaktifkannya membuat hasil tidak dapat dinilai." | `422` |

**Contoh `VAL-26` dengan angka.** Kalium punya batas normal 3,5 sampai 5,1 mmol/L. Bila petugas
mengisi batas kritis bawah 4,0, sistem menolaknya — angka 4,0 masih berada di dalam rentang
normal, sehingga tidak masuk akal disebut kritis. Nilai yang benar untuk batas kritis bawah
adalah angka di bawah 3,5, misalnya 2,5.

---

## 5. Pengajuan Perubahan Batas Kritis

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| `VAL-31` | Mengajukan perubahan | Alasan pengajuan kosong | "Jelaskan alasan perubahan batas kritis ini." | `422` |
| `VAL-32` | Mengajukan perubahan | Sudah ada pengajuan berjalan untuk batas nilai yang sama | "Masih ada pengajuan yang belum diputuskan untuk batas nilai ini." | `409` |
| `VAL-33` | Menyetujui atau menolak | Yang memutuskan adalah pengaju sendiri | "Pengaju tidak boleh menyetujui pengajuannya sendiri." | `403` |
| `VAL-34` | Menyetujui atau menolak | Pengajuan sudah diputuskan sebelumnya | "Pengajuan ini sudah diputuskan." | `409` |
| `VAL-35` | Menarik pengajuan | Yang menarik bukan pengaju | "Hanya pengaju yang boleh menarik pengajuannya." | `403` |

---

## 6. Alasan Penolakan Sampel

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| `VAL-36` | Menambah alasan | Kode alasan sudah dipakai | "Kode alasan ini sudah dipakai data lain, jadi tidak bisa disimpan." | `409` |
| `VAL-37` | Mengubah alasan | Permintaan memuat penanda kesalahan internal atau penanda wajib catatan | "Kedua penanda ini hanya dapat diubah administrator sistem, karena menentukan siapa menanggung biaya pengambilan ulang." | `403` |
| `VAL-38` | Menonaktifkan alasan | Alasan sedang menjadi satu-satunya yang aktif | "Sekurang-kurangnya satu alasan penolakan harus tetap aktif." | `422` |

---

## 6b. Pendaftaran Pasien Datang Langsung dan Rujukan Luar

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| `VAL-40` | Pendaftaran | Identitas wajib tidak lengkap | Pesan dari Registrasi ditampilkan apa adanya, karena Registrasi yang memiliki aturannya | `422` |
| `VAL-41` | Pendaftaran | Registrasi menolak karena kewenangan | "Anda tidak berhak membuat kunjungan baru. Hubungi bagian pendaftaran." | `403` |
| `VAL-42` | Pendaftaran | Registrasi tidak dapat dihubungi | "Pendaftaran gagal karena layanan registrasi sedang tidak dapat diakses. Silakan coba lagi." | `503` |
| `VAL-43` | Pendaftaran rujukan luar | Instansi perujuk diketik bebas, tidak dipilih dari daftar | "Pilih instansi perujuk dari daftar. Bila belum ada, hubungi bagian data induk untuk menambahkannya." | `422` |
| `VAL-44` | Pendaftaran rujukan luar | Nomor surat rujukan kosong | "Nomor surat rujukan wajib diisi untuk pasien rujukan." | `422` |
| `VAL-45` | Pendaftaran | Permintaan yang sama dikirim dua kali | Dikembalikan kunjungan yang sama, bukan membuat yang baru | `200` |

**Kenapa `VAL-40` meneruskan pesan Registrasi apa adanya.** Aturan kelengkapan identitas pasien
adalah milik Registrasi, bukan Laboratorium. Menerjemahkan ulang pesannya berisiko membuat dua
aturan yang berbeda untuk hal yang sama.

**Contoh `VAL-45`.** Petugas menekan Simpan, jaringan lambat, lalu ia menekan Simpan lagi.
Permintaan kedua membawa kunci idempotensi yang sama, sehingga Registrasi mengembalikan
kunjungan yang sudah dibuat. Pasien **tidak** mendapat dua kunjungan pada hari yang sama.

---

## 6c. Katalog, Harga, dan Cakupan Penjamin

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| `VAL-46` | Menambah pemeriksaan | Disiplin pemeriksaan tidak sesuai disiplin pesanan | "Pemeriksaan ini bukan bagian dari {disiplin pesanan}. Buat pesanan terpisah untuk disiplin yang sesuai." | `422` |
| `VAL-47` | Menampilkan harga | Tidak ada tarif berlaku pada tanggal kejadian | "Tarif untuk pemeriksaan ini belum diatur. Hubungi bagian data induk." | `422` |
| `VAL-48` | Menampilkan cakupan | Tidak ada kontrak penjamin untuk pemeriksaan itu | Ditampilkan **tidak tercakup**. Ini bukan kesalahan dan tidak menghalangi pemesanan | — |
| `VAL-49` | Menampilkan katalog | Pemeriksaan berpenanda laboratorium tetapi belum punya disiplin | Tidak muncul pada daftar disiplin mana pun; kepala instalasi melihat keterangan "disiplin belum diatur" | — |
| `VAL-50` | Endpoint katalog dan tarif | Percobaan mengubah data lewat modul Laboratorium | "Tarif diubah lewat menu Data Induk, bukan dari sini." | `403` |

**Contoh `VAL-46`.** Petugas membuat pesanan berdisiplin Mikrobiologi, lalu mencoba menambahkan
Hemoglobin. Hemoglobin bertanda disiplin Patologi Klinik pada katalog, sehingga sistem
menolaknya dan menyarankan membuat pesanan Patologi Klinik terpisah.

**Kenapa `VAL-48` bukan kesalahan.** Pemeriksaan yang tidak ditanggung penjamin **tetap boleh**
dipesan — pasien membayar sendiri. Yang penting adalah keterangannya terlihat **sebelum**
pemeriksaan dikerjakan.

---

## 7. Daftar Kerja

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| `VAL-39` | Daftar pantau keterlambatan | Jenis pemeriksaan belum punya batas waktu cito | Pesanan itu **tidak** muncul pada daftar keterlambatan, dan diberi keterangan "batas waktu cito belum diatur" | — |

`VAL-39` bukan penolakan, melainkan perilaku yang harus disepakati: pesanan cito tanpa batas
waktu tidak dianggap terlambat, tetapi keadaannya ditampilkan agar kepala instalasi tahu ada
data induk yang belum lengkap.

---

## 7b. Penerimaan Sampling/Specimen — amandemen `r4`, 2026-09-14

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| `VAL-51` | Merencanakan wadah | Jenis specimen tidak diisi | "Pilih jenis specimen terlebih dahulu." | `422` |
| `VAL-52` | Merencanakan wadah | Jenis specimen dikirim sebagai teks, bukan pilihan dari daftar | "Jenis specimen harus dipilih dari daftar. Bila jenisnya belum ada, pilih Lainnya lalu tuliskan keterangannya." | `422` |
| `VAL-53` | Merencanakan wadah | Jenis terpilih adalah `Lainnya` tetapi keterangannya kosong | "Tuliskan jenis specimennya pada kolom keterangan." | `422` |
| `VAL-54` | Merencanakan wadah | Jenis terpilih **bukan** `Lainnya` tetapi keterangan `Lainnya` ikut dikirim | "Keterangan jenis hanya diisi bila jenisnya Lainnya." | `422` |
| `VAL-55` | Merencanakan wadah | Jenis specimen yang dipilih sudah dinonaktifkan | "Jenis specimen ini sudah tidak dipakai lagi. Pilih jenis lain." | `422` |
| `VAL-56` | Merencanakan wadah | Volume diisi tanpa satuan | "Pilih satuan volumenya." | `422` |
| `VAL-57` | Merencanakan wadah | Satuan yang dipilih bukan satuan laboratorium | "Satuan ini tidak dipakai laboratorium. Pilih dari daftar satuan yang tersedia." | `422` |
| `VAL-58` | Merencanakan wadah | Waktu penerimaan fisik berada di masa depan | "Waktu penerimaan tidak boleh melewati waktu sekarang." | `422` |
| `VAL-59` | Merencanakan wadah | Waktu penerimaan fisik mendahului waktu pengambilan specimen | "Waktu penerimaan tidak boleh lebih awal daripada waktu pengambilan." | `422` |
| ~~`VAL-60`~~ | ~~Menambah pemeriksaan~~ | **Dicabut `LAB-DEC-050` pada 2026-09-14.** Ruas `Quantity` tidak jadi dibuat, sehingga tidak ada yang perlu divalidasi | — |
| `VAL-61` | Menambah jenis specimen | Kode jenis sudah dipakai baris lain | "Kode jenis ini sudah dipakai data lain, jadi tidak bisa disimpan." | `409` |
| `VAL-62` | Menambah atau mengubah jenis specimen | Permintaan mencoba menyetel penanda `Lainnya` pada baris kedua | "Hanya boleh ada satu jenis Lainnya yang aktif." | `422` |
| `VAL-63` | Menonaktifkan jenis specimen | Baris yang dinonaktifkan adalah satu-satunya jenis `Lainnya` yang aktif | "Jenis Lainnya harus tetap aktif, karena menjadi jalan keluar ketika jenis specimen belum terdaftar." | `422` |

---

## 7c. Pemesanan per disiplin — amandemen `r5`, 2026-09-15

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| `VAL-64` | Membuat pesanan dari daftar pemeriksaan | Daftar pemeriksaan kosong | "Pilih sekurang-kurangnya satu pemeriksaan." | `422` |
| `VAL-65` | Membuat pesanan dari daftar pemeriksaan | Satu jenis pemeriksaan dipilih dua kali | "Pemeriksaan yang sama tidak boleh dipilih dua kali. Untuk pengerjaan ganda, tandai duplo saat mencatat wadah." | `422` |
| `VAL-66` | Membuat pesanan dari daftar pemeriksaan | Ada pilihan yang bukan pemeriksaan laboratorium, sudah dinonaktifkan, atau tidak ditemukan | "Ada pemeriksaan yang tidak dapat dipesan. Periksa kembali pilihan Anda." | `422` |
| `VAL-67` | Membuat pesanan dari daftar pemeriksaan | Kunjungan yang dituju sudah ditutup atau dibatalkan | "Kunjungan ini sudah selesai, pemeriksaan baru tidak dapat dipesankan." | `422` |
| `VAL-68` | Mencatat wadah | Pemeriksaan yang dimasukkan ke wadah **tidak ada** pada daftar terpesan pesanan itu | "Pemeriksaan ini tidak ada pada daftar yang dipesan untuk pasien ini." | `422` |
| `VAL-69` | Mencatat wadah | Pemeriksaan terpesan yang **sudah** masuk wadah lain dimasukkan lagi | "Pemeriksaan ini sudah masuk wadah lain." | `409` |


### Amandemen `r6` — Konfirmasi dan pembatalan beralasan, 2026-09-15

Menurunkan `LAB-DEC-061` dan `LAB-DEC-063`.

| ID | Kapan | Kondisi yang ditolak | Pesan | Kode |
|---|---|---|---|---|
| `VAL-70` | Mengonfirmasi pesanan | Pesanan sudah pernah dikonfirmasi | "Pesanan ini sudah dikonfirmasi." | `409` |
| `VAL-71` | Mengonfirmasi pesanan | Status pesanan bukan `Requested` | "Pesanan ini sudah melewati tahap konfirmasi." | `409` |
| `VAL-72` | Mengonfirmasi pesanan | Dokter pemeriksa belum dipilih | "Pilih dokter pemeriksa terlebih dahulu." | `422` |
| `VAL-73` | Mengonfirmasi pesanan | Dokter pemeriksa yang dipilih tidak ditemukan atau tidak aktif | "Dokter pemeriksa tidak ditemukan atau tidak aktif." | `422` |
| `VAL-74` | Membatalkan pesanan | Alasan pembatalan kosong atau hanya spasi | "Alasan pembatalan wajib diisi." | `422` |
| `VAL-75` | Membatalkan pesanan | Status pesanan bukan `Requested` maupun `Confirmed` | "Pesanan yang sudah diproses tidak dapat dibatalkan." | `409` |

**`VAL-74` menuntut alasannya ada, bukan menuntut berapa kali petugas ditanya.** Pop-up alert
konfirmasi akhir yang ditetapkan `LAB-DEC-063` adalah kewenangan UI dan **tidak** memiliki aturan
validasi di sini — backend tidak dapat, dan tidak perlu, membuktikan bahwa seseorang sempat
ditanya dua kali.

**`VAL-75` adalah satu-satunya pengetatan pada amandemen ini.** Ia mempersempit pembatalan yang
sebelumnya sah dari hampir semua status. Dampaknya pada data berjalan wajib diperiksa sebelum
ditegakkan: berapa pesanan hari ini berstatus `Accepted`, `InProcess`, atau `OnHold`, dan apakah
ada alur yang masih membatalkannya. Sisanya — `VAL-70` sampai `VAL-74` — menjaga tindakan yang
**belum ada sama sekali**, sehingga nol permintaan yang hari ini berhasil akan menjadi gagal.

**`VAL-68` dan `VAL-69` bersifat aditif, bukan pengetatan diam-diam.** Keduanya **hanya berlaku
bila pesanannya memiliki baris `LabOrderedProcedure`**. Pesanan yang dibuat lewat
`POST /lab-orders` yang lama tidak memilikinya, sehingga
`POST /lab-specimens/by-order/{labOrderId}` berperilaku persis seperti sebelumnya bagi mereka.

Batasan ini disengaja dan alasannya baru saja terbukti mahal: `BE-LAB-21` membuat
`specimenTypeId` wajib pada endpoint yang sedang dipakai, dan layar wadah menjawab `422` sejak
migrationnya diterapkan.

**Kenapa tidak ada aturan yang menolak pemesanan lintas disiplin.** Justru itu yang dilayani
`LAB-DEC-055`: petugas memilih bebas, dan sistem yang memecah. `VAL-46` tetap berlaku apa adanya
pada jalur lama — menambahkan pemeriksaan berdisiplin lain ke sebuah pesanan yang sudah berdiri
tetap ditolak, karena di sana tidak ada pemecahan yang bisa menolong.

**Kenapa `VAL-65` menyebut duplo pada pesannya.** Tanpa itu petugas yang benar-benar perlu
mengerjakan satu pemeriksaan dua kali akan mencoba memilihnya dua kali, ditolak, lalu tidak tahu
harus berbuat apa. `IsDuplo` adalah jawabannya sejak `LAB-DEC-026`, dan pesan penolakan adalah
tempat paling murah untuk mengajarkannya.

---

**Kenapa `VAL-63` ada.** `LAB-DEC-040` memilih `Lainnya` justru untuk mencegah jalan buntu di
meja penerimaan. Bila baris itu dapat dinonaktifkan, jalan buntunya kembali — dan kembalinya
diam-diam, lewat satu klik pada layar pengelolaan yang tidak terlihat hubungannya dengan
penerimaan sampel. Polanya sama dengan `VAL-38` pada alasan penolakan.

**Kenapa tidak ada aturan batas minimum atau maksimum volume.** `RULE-021` pada `LAB-EVD-001`
menyatakan tidak ada ketentuan bisnisnya, dan `LAB-DEC-041` menerima itu apa adanya. Sistem
**tidak** menolak volume yang kecil dan **tidak** menghitung sendiri apakah sampelnya cukup —
yang menyatakan sampel tidak cukup adalah petugas lewat penetapan kelayakan (`AC-63`).

**Yang sengaja belum ditulis.** Aturan validasi untuk pengusulan instansi perujuk dan untuk
metode pembayaran menunggu `LAB-REQ-005`. `VAL-43` dan `VAL-44` tetap berlaku apa adanya
sampai jawabannya datang.

---

## 8. Traceability

| Aturan | Decision ID | Acceptance criteria |
|---|---|---|
| `VAL-51` sampai `VAL-55`, `VAL-61` sampai `VAL-63` | `LAB-DEC-040` | AC-58, AC-59, AC-60, AC-61 |
| `VAL-56`, `VAL-57` | `LAB-DEC-041` | AC-62, AC-64 |
| `VAL-64` sampai `VAL-67` | `LAB-DEC-055`, `LAB-DEC-056` | AC-86, AC-87 |
| `VAL-68`, `VAL-69` | `LAB-DEC-057` | AC-91 |
| `VAL-70` sampai `VAL-73` | `LAB-DEC-061` | AC-94, AC-95 |
| `VAL-74`, `VAL-75` | `LAB-DEC-063` | AC-96, AC-97 |
| `VAL-58`, `VAL-59` | `LAB-DEC-042` | AC-66 |
| ~~`VAL-60`~~ | ~~`LAB-DEC-038`~~ | **Dicabut `LAB-DEC-050`** bersama `AC-52` |
| `VAL-03`, `VAL-04` | `LAB-DEC-013` | AC-18 |
| `VAL-05`, `VAL-07`, `VAL-13`, `VAL-18` | `LAB-DEC-024` | AC-35, AC-36 |
| `VAL-08` | `LAB-INH-008` | AC-12 |
| `VAL-09` | `LAB-INH-007` | — |
| `VAL-10` sampai `VAL-12` | `LAB-DEC-019` | AC-26 |
| `VAL-14`, `VAL-15` | `LAB-INH-011` | — |
| `VAL-16` | `INV-05` | — |
| `VAL-22` sampai `VAL-24` | `LAB-DEC-021` | AC-28 |
| `VAL-21`, `VAL-25` sampai `VAL-27` | `LAB-DEC-018` | AC-24 |
| `VAL-28`, `VAL-31` sampai `VAL-35` | `LAB-DEC-023` | AC-33 |
| `VAL-37` | `LAB-DEC-019`, `LAB-INH-010` | AC-26 |

---

## 9. Amandemen `r7` — Hasil Mikrobiologi dan Patologi Anatomi, 2026-09-18

> ### ✅ STATUS: `approved` — 2026-09-18
>
> Disetujui **Yoga Aji Pratama** selaku pemilik modul pada 2026-09-18, bersama `LAB-API-v1` `r24`
> dan `LAB-PERM-v1` revision 6. `approved_by` / `approved_at`:
> Yoga Aji Pratama (`yogaaji452@gmail.com`) / 2026-09-18.
>
> **Sembilan aturan baru, `VAL-83` sampai `VAL-91`. Nol aturan `VAL-01`..`VAL-82` berubah.**

### 9.1 Satu selisih pembukuan ditemukan saat menulis bagian ini

**`VAL-76` sampai `VAL-82` tidak pernah masuk ke matriks ini.** Ketujuhnya lahir pada
`LAB-API-v1` `r18` dan `r21` — penyaring NIK, Kategori Periode, dan pengisian hasil Patologi
Klinik — dan tertulis lengkap di sana, tetapi dokumen ini berhenti pada `VAL-75`.

Selisihnya **tidak ditambal diam-diam di sini**: menyalinnya sekarang berarti mengaku ia sudah
tercatat sejak `r18`, padahal tidak. Yang dilakukan adalah **mencatat keberadaan selisihnya**,
dan penomoran usulan ini melanjutkan dari `VAL-82` supaya nomornya tidak bertabrakan.

### 9.2 Aturan yang diusulkan

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| `VAL-83` | Mengisi hasil Mikrobiologi atau Patologi Anatomi | Pemeriksaan sudah gugur karena wadahnya ditolak, atau sudah dibatalkan | "Pemeriksaan ini sudah tidak berjalan, hasilnya tidak dapat diisi." | `409` |
| `VAL-84` | Mengisi hasil | Bentuk hasil pemeriksaan tidak cocok dengan jalur yang dipakai | "Pemeriksaan ini bukan pemeriksaan {Mikrobiologi/Patologi Anatomi}. Gunakan layar yang sesuai." | `422` |
| `VAL-85` | Mencatat isolat | Organisme yang dipilih tidak ditemukan atau sudah dinonaktifkan | "Organisme ini sudah tidak dipakai lagi. Pilih dari daftar yang tersedia." | `422` |
| `VAL-86` | Mencatat kepekaan | Antibiotik yang dipilih tidak ditemukan atau sudah dinonaktifkan | "Antibiotik ini sudah tidak ada pada panel uji. Pilih dari daftar yang tersedia." | `422` |
| `VAL-87` | Mencatat kepekaan | Satu antibiotik dimasukkan dua kali pada isolat yang sama | "Antibiotik ini sudah diuji pada kuman tersebut. Satu antibiotik cukup sekali." | `422` |
| ~~`VAL-88`~~ | ~~Mengisi hasil Patologi Anatomi~~ | ~~Makroskopik, mikroskopik, atau kesimpulan kosong atau hanya berisi spasi~~ | **DICABUT `r8` — 2026-09-18.** Digantikan `VAL-95`, yang menguji kelengkapan terhadap **data induk keberlakuan**, bukan terhadap tiga nama kolom | — |
| `VAL-89` | Mengisi hasil | Waktu pemeriksaan berada di masa depan | "Waktu pemeriksaan tidak boleh melewati waktu sekarang." | `422` |
| `VAL-90` | Mencatat kepekaan | Zona hambat diisi nol atau negatif | "Lebar zona hambat harus lebih dari nol milimeter." | `422` |
| `VAL-91` | Menambah organisme atau antibiotik | Kodenya sudah dipakai baris lain | "Kode ini sudah dipakai data lain, jadi tidak bisa disimpan." | `409` |

### 9.3 Catatan yang menentukan cara menegakkannya

**`VAL-85` dan `VAL-86` hanya berlaku pada baris BARU.** Hasil lama yang menunjuk organisme yang
kemudian dinonaktifkan **tetap sah dan tetap terbaca** (`INV-31`). Menegakkan "aktif" saat
membaca akan membuat temuan pasien menghilang karena data induknya dirapikan — kelas kesalahan
yang sama dengan mengubah batas nilai secara surut, dan itu yang sudah dicegah `S4a` lewat
`ResultValueBoundId`.

**`VAL-87` ditegakkan dua lapis**: pada aturan bisnis, dan pada index unik parsial
`(LabMicrobiologyIsolateId, LabAntibioticId)` dengan pembatas `IsDelete = false`. Pembatas itu
wajib — tanpanya, analis yang menghapus baris lalu memilih antibiotik yang sama lagi akan
ditolak basis data tanpa sebab yang masuk akal baginya. Modul ini sudah pernah membayar persis
kesalahan itu lewat `LAB-CONFLICT-005`.

~~**`VAL-88` menolak, bukan menyimpan sebagai draft.** Laporan patologi tanpa kesimpulan tidak
dapat dipakai dokter untuk memutuskan apa pun, dan `INV-25` menurunkannya langsung dari BR-23.~~

> **Catatan ini ikut gugur bersama `VAL-88` pada `r8` (2026-09-18).** Yang bertahan dari isinya
> adalah **niatnya**, dan niat itu kini dijaga `VAL-95`: laporan tetap tidak boleh difinalkan
> selama isian **wajib** masih kosong. Yang gugur adalah anggapan bahwa "wajib" selalu berarti
> persis tiga kolom itu — `LAB-DEC-086` menetapkan kewajibannya **bergantung kategori**.

**`VAL-89` mengikuti `VAL-82` dan `LAB-DEC-064`** — aturan yang sama, diterapkan pada waktu
kejadian.

**Nol aturan validasi untuk penilaian kritis.** Hasil Mikrobiologi dan narasi Patologi Anatomi
**tidak dinilai kritis** (`INV-28`), sehingga tidak ada yang perlu divalidasi. Ditulis eksplisit
agar ketiadaannya tidak terbaca sebagai kelalaian.

### 9.4 Traceability usulan

| Aturan | Decision ID | Invariant |
|---|---|---|
| `VAL-83`, `VAL-84`, `VAL-89` | `LAB-DEC-027` | `INV-24` |
| `VAL-85`, `VAL-86`, `VAL-91` | `LAB-DEC-084` | `INV-30`, `INV-31` |
| `VAL-87`, `VAL-90` | `LAB-DEC-027` | `INV-27` |
| ~~`VAL-88`~~ | ~~`LAB-DEC-027`~~ | **Dicabut** bersama `INV-25` — lihat bagian 10 |

---

## 10. Amandemen `r8` — Laporan Patologi Anatomi per pesanan, 2026-09-18

> ### ✅ STATUS: `approved` — **DISETUJUI 2026-09-18**
>
> Disetujui bersama `LAB-API-v1` `r25` dan `LAB-PERM-v1` rev 7.
> `approved_by` / `approved_at`: Yoga Aji Pratama (`yogaaji452@gmail.com`) / **2026-09-18**.

### 10.1 Satu aturan dicabut

**`VAL-88` DICABUT.** Ia menuntut *"makroskopik, mikroskopik, dan kesimpulan ketiganya wajib
terisi"* — benar terhadap BR-23, dan BR-23 sendiri yang ternyata tidak lengkap. Kewajiban ruas
kini **bergantung kategori** dan ditegakkan `VAL-95` terhadap data induk keberlakuan, bukan
terhadap tiga nama kolom yang dihardcode.

**`VAL-83`, `VAL-84`, `VAL-89` tetap berlaku bagi Mikrobiologi.** Yang dicabut hanya `VAL-88`.

### 10.2 Aturan yang berlaku

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
|---|---|---|---|---|
| `VAL-92` | Membuka atau menyimpan laporan PA | Pesanan tidak ditemukan, sudah dibatalkan, atau **bukan berdisiplin Patologi Anatomi** | "Pesanan ini bukan pemeriksaan Patologi Anatomi." | `404` / `422` |
| `VAL-93` | Menyimpan nilai | Parameter yang dikirim **tidak berlaku** bagi kategori pesanan ini | "Isian ini tidak dipakai pada jenis pemeriksaan tersebut." | `422` |
| `VAL-94` | Menyimpan nilai | Satu parameter dikirim dua kali dalam satu permintaan | "Isian yang sama dikirim dua kali." | `422` |
| `VAL-95` | Memfinalkan | Masih ada parameter **wajib** yang kosong | "Laporan belum dapat diselesaikan. Isian berikut masih kosong: {daftar}." | `422` |
| `VAL-96` | Menyimpan nilai | Laporan sudah final | "Laporan ini sudah diselesaikan. Buka kembali lebih dulu bila perlu diubah." | `409` |
| `VAL-97` | Membuka kembali | Alasan kosong atau hanya spasi | "Tuliskan alasan membuka kembali laporan ini." | `422` |
| `VAL-98` | Membuka kembali | Laporan belum pernah difinalkan | "Laporan ini belum pernah diselesaikan." | `409` |
| `VAL-99` | Menyimpan nilai | Parameter atau kategorinya sudah dinonaktifkan | "Isian ini sudah tidak dipakai lagi." | `422` |
| `VAL-100` | Membuka atau memfinalkan laporan | Pemeriksaan pada pesanan itu **nol punya pemetaan kategori** | "Jenis pemeriksaan pada pesanan ini belum digolongkan ke kategori Patologi Anatomi. Hubungi kepala instalasi." | `422` |
| `VAL-101` | Menambah parameter atau kategori | Kodenya sudah dipakai | "Kode ini sudah dipakai data lain, jadi tidak bisa disimpan." | `409` |
| `VAL-102` | Menulis konteks klinis | Pesanan bukan Patologi Anatomi | "Konteks klinis hanya diisi untuk pemeriksaan Patologi Anatomi." | `422` |

### 10.3 Catatan yang menentukan cara menegakkannya

**`VAL-95` wajib menyebut parameter mana saja yang kosong.** Menolak dengan *"laporan belum
lengkap"* saja akan membuat patolog menebak ruas mana yang terlewat pada formulir berisi sampai
lima belas isian.

**`VAL-100` adalah keadaan yang PASTI terjadi pada hari pertama**, sebelum pemetaan jenis
pemeriksaan diisi. Pesannya wajib menyebut **apa yang belum diatur dan siapa yang mengaturnya** —
bukan hanya menolak. Tanpa itu, patolog pertama yang membuka layar akan mengira sistemnya rusak.

**`VAL-99` berlaku hanya bagi nilai BARU.** Laporan lama yang menunjuk parameter yang kemudian
dinonaktifkan **tetap terbaca utuh** (`INV-37`) — pola yang sama dengan `VAL-85`/`VAL-86` pada
Mikrobiologi, dan dengan `ResultValueBoundId` pada `S4a`.

**Nol aturan untuk `Waktu Issued` dan `Waktu Efektif`.** Keduanya **tidak diterima** dari
pemanggil (`INV-38`), sehingga tidak ada yang perlu divalidasi. Ditulis eksplisit agar
ketiadaannya tidak terbaca sebagai kelalaian.

### 10.4 Traceability

| Aturan | Decision ID | Invariant |
|---|---|---|
| `VAL-92`, `VAL-102` | `LAB-DEC-085`, `LAB-DEC-091` | `INV-32`, `INV-40` |
| `VAL-93`, `VAL-94`, `VAL-95`, `VAL-99` | `LAB-DEC-086` | `INV-33`, `INV-34`, `INV-37` |
| `VAL-96`, `VAL-97`, `VAL-98` | `LAB-DEC-088` | `INV-35`, `INV-36` |
| `VAL-100` | `LAB-DEC-087` | `INV-39` |
| `VAL-101` | `LAB-DEC-086` | — |

---

## 11. Amandemen `r9` — `S4b` sesudah putaran 9 dan 10, 2026-09-21

| Field | Nilai |
|---|---|
| `contract_version` | `LAB-VAL-v1` |
| Revision | `r9` |
| Status | **`approved`** |
| `approved_by` / `approved_at` | Yoga Aji Pratama (`yogaaji452@gmail.com`) / 2026-09-21 |
| `input_revision` | decisions rev 50; `LAB-API-v1` `r26` |

### 11.1 Aturan validasi yang ditambahkan

| ID | Aturan | Pesan bagi pengguna | Dasar |
|---|---|---|---|
| `VAL-103` | Hasil Mikrobiologi **tidak dapat disimpan** bila `microbiologyFinding` kosong | "Status temuan wajib dipilih sebelum hasil disimpan." | `LAB-DEC-104` |
| `VAL-104` | `specimenTypeOtherNote` **wajib** terisi bila jenis specimen yang dipilih bertanda `IsOtherBucket` | "Jenis specimen `Lainnya` wajib disertai keterangan." | `LAB-DEC-098`, menegakkan `LAB-DEC-040` butir 3 |
| `VAL-105` | Satu specimen **tidak boleh** memuat Spesifik Specimen yang sama dua kali | "Rincian specimen itu sudah dipilih." | `LAB-DEC-098` |
| `VAL-106` | Baris aturan kritis yang **ketiga ruas penilainya kosong** ditolak | "Aturan kritis harus menyebut sedikitnya organisme, antibiotik, atau hasil kepekaan." | `LAB-DEC-103` |
| `VAL-107` | `reopen` ditolak bila hasil **belum** pernah `finalize` | "Hasil ini belum pernah dinyatakan selesai, jadi tidak ada yang perlu dibuka kembali." | `LAB-DEC-097` |
| `VAL-108` | `consultedAt` **tidak boleh** berada di masa depan | "Waktu konsultasi tidak boleh melewati waktu sekarang." | `LAB-DEC-106` |
| `VAL-109` | `PATCH /correction` ditolak bila hasil pemeriksaan yang memakai specimen itu **sudah** `finalize` | "Hasil sudah dinyatakan selesai; buka kembali hasilnya lebih dulu sebelum mengoreksi specimen." | `LAB-DEC-107` butir 3 |
| `VAL-110` | `volumeUnitId` **wajib** menunjuk `MstMeasurement` bertanda `IsForLaboratory` | "Satuan itu tidak berlaku untuk specimen laboratorium." | `LAB-DEC-100` |
| `VAL-111` | `LabSpecimenDetailType` yang **nonaktif** tidak dapat dipakai pada pilihan **baru**, tetapi pilihan lama yang sudah menunjuknya tetap sah dan tetap terbaca | "Rincian specimen itu sudah tidak dipakai lagi." | Mengikuti pola `VAL-85`/`VAL-86` dan `INV-31` |

### 11.2 Aturan yang sengaja TIDAK dibuat

| Yang ditolak | Alasan |
|---|---|
| "Seluruh ruas form hasil wajib diisi" | **`LAB-DEC-104` menolaknya secara tegas.** Kultur steril adalah hasil yang sah dan nol isolat; mewajibkan organisme membuatnya mustahil disimpan. Ini pengulangan kesalahan `VAL-88` yang sudah dicabut |
| Wajib mengisi MIC atau zona hambat | `LAB-DEC-101` — difusi cakram menghasilkan mm, dilusi menghasilkan MIC, dan mewajibkan salah satunya memaksa analis mengisi angka karangan |
| Pembandingan kronologis `issuedAt` terhadap `effectiveAt` | Keduanya kini **turunan** (`LAB-DEC-096`); nol yang dapat diketik, sehingga nol yang perlu dibandingkan. `LAB-DEC-023` tetap berlaku sebagai keputusan, hanya kehilangan objeknya |
| Menolak isolat ketika status temuan `Negatif` | `INV-26` menyebutnya *usulan arsitektur*, bukan keputusan pemilik modul. Dibiarkan terbuka sebagai `LAB-OPEN-042` daripada ditegakkan diam-diam |

### 11.3 Traceability `r9`

| Aturan | Keputusan | Invariant |
|---|---|---|
| `VAL-103`, `VAL-104` | `LAB-DEC-104`, `LAB-DEC-098` | `INV-26` |
| `VAL-105`, `VAL-111` | `LAB-DEC-098` | `INV-31` (pola) |
| `VAL-106` | `LAB-DEC-103` | Mempersempit `INV-28` |
| `VAL-107`, `VAL-109` | `LAB-DEC-097`, `LAB-DEC-107` | — |
| `VAL-108` | `LAB-DEC-106` | — |
| `VAL-110` | `LAB-DEC-100` | — |

---

## 12. Amandemen `r10` — `S4b` sesudah bukti cetak, 2026-09-21

| Field | Nilai |
|---|---|
| `contract_version` | `LAB-VAL-v1` |
| Revision | `r10` |
| Status | **`approved`** |
| `approved_by` / `approved_at` | Yoga Aji Pratama (`yogaaji452@gmail.com`) / 2026-09-21 |
| `input_revision` | decisions rev 52; `LAB-API-v1` `r27` |

### 12.1 Aturan yang ditambahkan

| ID | Aturan | Pesan bagi pengguna | Dasar |
|---|---|---|---|
| `VAL-112` | `concentrationUnitId` **wajib** bila `concentration` terisi | "Pilih satuan untuk nilai kadar." | `LAB-DEC-115` |
| `VAL-113` | `resultOverrideReason` **wajib** bila interpretasi yang dikirim berbeda dari hitungan sistem | "Interpretasi berbeda dari hitungan sistem; tuliskan alasannya." | `LAB-DEC-123` |
| `VAL-114` | `result` **wajib** bila breakpoint untuk kombinasi itu **tidak tersedia** | "Breakpoint untuk kuman dan antibiotik ini belum disetel, jadi interpretasinya harus diisi sendiri." | `LAB-DEC-123` |
| `VAL-115` | `lowerMm` tidak boleh melebihi `upperMm` pada data induk breakpoint | "Batas bawah tidak boleh lebih besar daripada batas atas." | `LAB-DEC-122` |
| `VAL-116` | `zoneDiameterMm` **tidak boleh negatif**; nilai `0` **diterima** | "Lebar zona tidak boleh kurang dari nol." | `LAB-DEC-128` |
| `VAL-117` | Isolat bertanda **tidak diuji kepekaannya** tidak boleh memiliki satu pun baris kepekaan | "Kuman ini ditandai tidak diuji, jadi tidak boleh punya baris antibiotik." | `LAB-DEC-126` |
| `VAL-118` | Bagian isolat dan antibiogram **ditolak** pada pemeriksaan yang profilnya menyatakan tidak memakai set bakteri | "Pemeriksaan ini tidak memakai set bakteri." | `LAB-DEC-125` |
| `VAL-119` | Satu kombinasi organisme dan antibiotik hanya boleh punya **satu** breakpoint aktif | "Breakpoint untuk pasangan ini sudah ada." | `LAB-DEC-122` |

### 12.2 Aturan yang sengaja TIDAK dibuat

| Yang ditolak | Alasan |
|---|---|
| Mewajibkan `zoneDiameterMm` terisi | Metode dilusi nol menghasilkan zona sama sekali; mewajibkannya membuat hasil jamur `LAB-EVD-005` mustahil disimpan (`LAB-DEC-128`) |
| Menolak zona `0` | `0` adalah **temuan terkuat** bahwa kuman kebal — sebelas dari 22 baris pada `LAB-EVD-006` bernilai `0` dan seluruhnya `R` |
| Mewajibkan `cultureType` dan `susceptibilityMethod` | Enam varian cetak belum pernah dilihat; `r27` bagian 22.8 menjelaskannya |
| Menolak interpretasi yang berbeda dari hitungan | **Resistensi intrinsik** menuntut penilaian di luar rumus. Yang diwajibkan alasannya, bukan kepatuhannya (`VAL-113`) |
| Mewajibkan `resultQualifier` | Sama dengan dua penanda di atas — menunggu varian yang belum terlihat |

### 12.3 Traceability `r10`

| Aturan | Keputusan |
|---|---|
| `VAL-112` | `LAB-DEC-115` |
| `VAL-113`, `VAL-114` | `LAB-DEC-123` |
| `VAL-115`, `VAL-119` | `LAB-DEC-122` |
| `VAL-116` | `LAB-DEC-128` |
| `VAL-117` | `LAB-DEC-126` |
| `VAL-118` | `LAB-DEC-125` |
