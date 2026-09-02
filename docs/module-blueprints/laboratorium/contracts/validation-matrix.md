# Validation Matrix — Modul Laboratorium

| Field | Value |
|---|---|
| Contract version | `LAB-VAL-v1` |
| Revision | `3` |
| Status | `approved` — dikunci 2026-09-02 |
| Batas penguncian | Terkunci **kecuali** penamaan `MstLabValueBound` dan `MstLabValueOption`, yang menunggu `LAB-OPEN-021` |
| Owner | Yoga Aji Pratama |
| `approved_by` / `approved_at` | Yoga Aji Pratama (`yogaaji452@gmail.com`) / 2026-09-02 |
| Input revision | Decisions rev 20; `LAB-DA-001` rev 4 |
| Input hash | `sha256:75d285252aa5bce7fcaf5d90242da0d30fbd58a92a16aca3377683243be45f61` atas `00-interview-decisions.md`, dihitung 2026-09-02 |
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

## 8. Traceability

| Aturan | Decision ID | Acceptance criteria |
|---|---|---|
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
