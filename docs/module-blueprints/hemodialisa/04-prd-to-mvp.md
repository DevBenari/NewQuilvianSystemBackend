# Hemodialisa — PRD ke MVP

## 1. Identitas Dokumen

| Field | Nilai |
|---|---|
| Produk | Quilvian V2 |
| Modul | Hemodialisa |
| Blueprint ID | `HMD-BP-001` |
| Kode modul untuk penomoran | `HMD` |
| Contract version | `HMD-CONTRACT-v1` — status **`approved`** |
| Owner | Muhammad Hamzah |
| `approved_by` / `approved_at` | Muhammad Hamzah / 2026-09-18T15:40:07+07:00 |
| Backend target | `NewQuilvianSystemBackend`, branch `MHamzah`, baseline `190c91a0` |
| Frontend target | `QuilvianSystemFrontendDev`, branch `HamzahV2`, baseline `a38683142` |
| Kesiapan requirement | `READY_FOR_DOMAIN_DESIGN` — `HMD-RCG-001` r3 |
| Arsitektur domain | `DOMAIN_ARCHITECTURE_NOT_RUN` |
| `input_revision` | `02-backend-architecture.md` r1; seluruh berkas `contracts/` r1; `data/data-dictionary.md` r1 |
| Cakupan | Phase 1 saja — 25 Feature `MUST HAVE` ditambah satu kemampuan baru = **26 kemampuan** |

**Satu baris ringkasan cakupan:** satu pasien dapat menjalani satu sesi cuci darah lengkap —
dari permintaan masuk, dijadwalkan, dikerjakan, dipantau, sampai catatannya dikunci dan
tagihannya terbit.

> Dokumen ini **menurunkan**, tidak menciptakan. Setiap tabel, status, hak akses, dan endpoint
> yang disebut di sini sudah tercatat di arsitektur backend, kamus data, atau kontrak.

---

## 2. Ringkasan Eksekutif

Unit Hemodialisa rumah sakit hari ini bekerja tanpa dukungan sistem sama sekali. Jadwal ditulis
di papan, permintaan dari bangsal datang lewat telepon, catatan sesi ditulis di kertas, dan
tagihan dihitung terpisah setelah pasien pulang.

Akibatnya tiga hal yang mahal:

1. **Tidak ada yang bisa memastikan mesin yang dipakai memang laik.** Status mesin hanya
   diketahui orang yang kebetulan tahu.
2. **Permintaan cuci darah cito tidak pernah menjadi data.** Ketika ditanya berapa lama pasien
   menunggu sejak diminta sampai dikerjakan, tidak ada yang bisa menjawab.
3. **Catatan klinis dapat berubah tanpa jejak**, dan tindakan yang sudah dikerjakan bisa
   terlewat ditagih.

Rilis pertama ini menyelesaikan ketiganya dengan satu syarat sederhana: **satu pasien harus bisa
berjalan dari awal sampai akhir.** Bukan sekumpulan layar yang masing-masing berguna, melainkan
satu jalur utuh yang benar-benar dipakai unit setiap hari.

Yang **belum** dikerjakan pada rilis ini: pemantauan jangka panjang pasien, pelaporan ke luar
rumah sakit, dan pengukuran mutu. Ketiganya menunggu rilis berikutnya, dan ketiadaannya tidak
menghalangi satu pasien pun mendapat cuci darah yang tercatat dengan benar.

---

## 3. Masalah Produk

### 3.1 Keadaan sekarang, dengan buktinya

| Keadaan | Bukti |
|---|---|
| Tidak ada satu pun kode Hemodialisa di backend | Pencarian `hemodial` pada seluruh berkas `.cs` menghasilkan **nol** berkas |
| Tidak ada master alat medis di sistem | Folder data induk tidak memuat satu pun entity mesin atau peralatan |
| Tidak ada penjadwalan sumber daya untuk pasien | Yang ada hanya penjadwalan dinas pegawai, dan itu urusan berbeda |
| Dua layar Rawat Inap **sudah menunggu** modul ini | Kartu Hemodialisa pada ruang kerja dokter dan butir Hemodialisa pada ruang kerja perawat, keduanya bertanda belum tersedia |
| Kunci menu Hemodialisa sudah dipesan | Ada di berkas resolver sidebar, tetapi belum ada butir menu di baliknya |

### 3.2 Yang sudah ada dan tinggal dipakai

Ini bagian yang sering diremehkan: **Hemodialisa tidak dibangun dari nol.**

| Sudah ada | Dipakai untuk |
|---|---|
| Pasien, kunjungan, episode rawat inap | Konteks pasien, tanpa menyalin apa pun |
| Persetujuan tindakan dan tanda vital | Dipakai apa adanya |
| Tindakan pasien yang dapat ditagih | Jalur ke penagihan; sumber tagihannya **sudah terdaftar** |
| Keutuhan dan koreksi dokumen klinis | Penguncian catatan dan koreksi lewat tambahan |
| Kewenangan klinis petugas | Datanya lengkap; cara membacanya yang belum ada |
| Dua puluh satu komponen kerangka layar klinis | Sembilan di antaranya langsung menjawab kebutuhan Hemodialisa |
| Pola permintaan layanan penunjang | Sudah terbukti dua kali pada Laboratorium dan Radiologi |

### 3.3 Yang belum ada dan harus dibangun

Dua puluh dua tabel baru, sepuluh layanan, tujuh controller, dan sebelas layar. Rinciannya ada
di `02-backend-architecture.md` dan `03-frontend-architecture.md`.

---

## 4. Visi Produk

Rantai keterhubungan data yang ingin dicapai, ditulis sebagai urutan:

1. Seorang **pasien** yang sudah terdaftar di rumah sakit
2. datang lewat sebuah **kunjungan** — rawat jalan, rawat inap, atau gawat darurat
3. yang melahirkan sebuah **permintaan cuci darah** dari unit peminta
4. lalu dibukakan satu **program cuci darah** oleh petugas administrasi
5. yang di dalamnya dokter menilai **kelayakan**, mencatat **akses pembuluh darah**, meninjau
   **status serologi**, dan memutuskan **kebutuhan isolasi**
6. lalu membuat satu **instruksi cuci darah** yang berlaku
7. yang dipakai koordinator menjadwalkan satu **sesi** pada satu **mesin**, satu **station**,
   satu perawat, dan satu dokter penanggung jawab
8. yang hanya boleh berjalan bila **unit dinyatakan siap** pada shift itu
9. dan hanya boleh dimulai bila **dua belas butir persiapan** terpenuhi
10. lalu dipantau lewat **pengamatan berkala**, **pemberian obat**, dan **catatan kejadian**
11. sampai berakhir — selesai atau dihentikan — dan dinilai lewat **penilaian setelah tindakan**
12. lalu dokumentasinya **diselesaikan perawat** dan **disahkan dokter**
13. yang menghasilkan satu **tindakan pasien** yang diserahkan ke **penagihan**
14. dan sejak saat itu catatannya **terkunci**, hanya dapat dikoreksi lewat **tambahan**.

Empat belas langkah itu adalah satu rantai. Memutus salah satu membuat sisanya kehilangan
dasar.

---

## 5. Batas MVP

### 5.1 Titik mulai

MVP dimulai ketika keempat hal berikut sudah ada:

1. Pasien sudah terdaftar pada data induk pasien.
2. Ada permintaan cuci darah, **atau** pasien memang sudah menjalani program HD rutin.
3. Konteks kunjungan pasien dapat ditentukan.
4. Unit Hemodialisa sudah terdaftar sebagai unit layanan, dan mesin serta station-nya sudah
   didaftarkan.

### 5.2 Titik akhir

MVP selesai ketika kedelapan hal berikut terjadi:

1. Sesi cuci darah berakhir, baik selesai maupun dihentikan.
2. Penilaian setelah tindakan terisi.
3. Tujuan pasien setelah sesi tercatat.
4. Dokumentasi diselesaikan perawat.
5. Catatan disahkan dokter penanggung jawab sesi.
6. Catatan terkunci dan tidak dapat diubah langsung.
7. Tindakan pasien mencapai keadaan selesai, dan diserahkan ke penagihan bila memang dapat
   ditagih.
8. Koreksi setelah pengesahan tersedia lewat tambahan, beserta alasan dan penulisnya.

### 5.3 Yang berada di luar MVP

Tidak termasuk rilis pertama: pemantauan jangka panjang, perhitungan adekuasi, tren berat kering,
surveilans akses vaskular, pemantauan vaksinasi, pelaporan regulator, SATUSEHAT, penyerahan
insiden keselamatan, verifikasi administrasi JKN, indikator mutu, dialisis peritoneal,
transplantasi, operasi akses vaskular, pembacaan otomatis perangkat mesin, mesin alarm klinis
otomatis, dan pemakaian ulang dializer.

---

## 6. Pelaku Sasaran

| Pelaku | Tanggung jawab di dalam MVP |
|---|---|
| Dokter atau perawat unit peminta | Membuat permintaan cuci darah dari bangsal, poliklinik, atau IGD |
| Petugas administrasi HD | Membuka dan menutup program cuci darah pasien |
| Koordinator unit HD | Menerima permintaan, menjadwalkan sesi, menugaskan petugas, memeriksa kesiapan unit, mengelola mesin dan station |
| Dokter dialisis | Menilai kelayakan, membuat dan mengaktifkan instruksi, memutuskan isolasi, menolak permintaan yang tidak diindikasikan |
| Dokter penanggung jawab sesi | Mengesahkan dan mengunci catatan sesi |
| Perawat dialisis | Seluruh pekerjaan sepanjang sesi, sampai menyelesaikan dokumentasi |
| Tim Pencegahan dan Pengendalian Infeksi | Meninjau status serologi dan memutuskan kebutuhan isolasi |
| Petugas Rekam Medis | Menjaga keutuhan catatan dan memproses koreksi |
| Petugas Kasir | Menambahkan tagihan bahan untuk sesi yang dihentikan, bila rumah sakit berhak |
| Auditor berwenang | Menelusuri jejak perubahan dan akses |
| Pemegang akun tata kelola klinis | Menetapkan butir persiapan yang boleh dilewati dan mengubah pengaturan unit |

---

## 7. Pemilihan Kemampuan MVP

Setiap kemampuan diuji dua pertanyaan: tanpa ini, bisakah satu kasus nyata selesai dari awal
sampai akhir? Kalau tidak, adakah jalan sementara yang aman dan dapat diaudit?

| Kemampuan | ID kemampuan asal | Keputusan MVP |
|---|---|---|
| Verifikasi identitas pasien dan konteks kunjungan | `CAP-01`, `CAP-02` | Wajib; tanpa ini tidak ada catatan yang punya pemilik |
| Konteks rawat inap opsional | `CAP-03` | Wajib; pasien HD sering sedang dirawat inap |
| Permintaan HD masuk | `CAP-36` (`HMD-CAP-001`) | Wajib; tanpa ini permintaan cito tidak pernah menjadi data dan dua layar Rawat Inap tetap mati |
| Program cuci darah pasien | `CAP-25` | Wajib; seluruh rantai menggantung padanya |
| Penilaian kelayakan klinis | `CAP-26` | Wajib; menjalankan HD tanpa keputusan kelayakan adalah risiko keselamatan |
| Status akses pembuluh darah | `CAP-27` | Wajib; menentukan apakah sesi dapat dijalankan |
| Tinjauan serologi | `CAP-27` | Wajib; menentukan kebutuhan isolasi |
| Keputusan isolasi | `CAP-16` | Wajib; menentukan mesin dan station yang boleh dipakai |
| Persetujuan tindakan | `CAP-04` | Wajib; dipakai apa adanya dari yang sudah ada |
| Instruksi cuci darah dan revisinya | `CAP-28` | Wajib; sesi tidak punya parameter tanpa ini |
| Penjadwalan sesi | `CAP-29` | Wajib; tanpa ini unit tidak punya daftar kerja dan tabrakan mesin tidak tercegah |
| Registry mesin beserta status laiknya | `CAP-13` | Wajib; menjalankan sesi pada mesin rusak adalah risiko langsung |
| Riwayat status mesin | `CAP-13` | Wajib; tanpa riwayat, alasan mesin pernah diblokir hilang |
| Station | `CAP-14` | Wajib; menentukan kapasitas unit |
| Kesiapan unit, air, dan bahan | `CAP-15` | Wajib; gerbang sebelum shift berjalan |
| Kompetensi dan kapasitas staf | `CAP-20`, `CAP-21` | Wajib **dalam bentuk pencatatan status**; penegakannya menunggu pembacaan dari Human Resource |
| Checklist sebelum tindakan | `CAP-30` | Wajib; pengaman keselamatan terakhir sebelum jarum masuk |
| Penilaian sebelum dan sesudah tindakan | `CAP-05`, `CAP-06` | Wajib; menentukan apakah target tercapai |
| Memulai sesi | `CAP-31` | Wajib |
| Pemantauan berkala | `CAP-32` | Wajib; tanpa ini tidak ada bukti pasien dipantau |
| Pemberian obat | `CAP-33`, `CAP-23` | Wajib; antikoagulan adalah bagian tak terpisahkan dari HD |
| Komplikasi | `CAP-34` | Wajib; kejadian tidak diinginkan harus terdokumentasi |
| Tujuan pasien setelah sesi | `CAP-35` | Wajib; menentukan tanggung jawab berikutnya |
| Finalisasi dan penguncian catatan | `CAP-07` | Wajib; tanpa ini catatan klinis tidak punya kekuatan |
| Serah terima ke penagihan | `CAP-10`, `CAP-11` | Wajib; tanpa ini layanan tidak pernah sampai ke tagihan |
| Audit dan koreksi rekam medis | `CAP-08`, `CAP-09` | Wajib; koreksi tanpa jejak sama buruknya dengan tidak bisa mengoreksi |

---

## 8. Kemampuan yang Ditunda

Setiap baris menyebut **penggantinya selama MVP berjalan**. Menunda tanpa pengganti membuat
pengguna kehilangan pekerjaan yang selama ini bisa dilakukan.

| Kemampuan | ID kemampuan asal | Alasan ditunda | Pengganti selama MVP |
|---|---|---|---|
| Perhitungan dan tren adekuasi | Phase 2 | Memerlukan pembacaan hasil laboratorium per pasien yang belum tersedia, dan metode perhitungannya belum disahkan badan klinis | Hasil laboratorium tetap dapat dirujuk per sesi; dokter menilai secara manual |
| Rencana dan hasil pemeriksaan berkala | Phase 2 | Memerlukan irama pemeriksaan yang belum ditetapkan rumah sakit | Pemeriksaan tetap dipesan lewat Laboratorium seperti biasa |
| Tren berat kering dan status cairan | Phase 2 | Analitik jangka panjang; datanya baru bermakna setelah puluhan sesi terkumpul | Berat badan sebelum dan sesudah tetap dicatat setiap sesi, sehingga datanya siap dipakai kelak |
| Surveilans akses vaskular | Phase 2 | Alur pemantauan jangka panjang | Kondisi akses tetap dinilai dan dicatat setiap sesi |
| Komplikasi akses dan rujukan | Phase 2 | Melibatkan bedah vaskular yang belum ada modulnya | Komplikasi dan instruksi rujukan tetap dicatat pada sesi |
| Pemantauan serologi dan vaksinasi berkala | Phase 2 | Alur pencegahan infeksi jangka panjang | Hasil serologi tetap dirujuk dan ditinjau; keputusan isolasi tetap berjalan |
| Dataset dan laporan tahunan | Phase 2 | Belum dibutuhkan untuk menjalankan satu sesi | Seluruh data sumbernya sudah tersimpan, siap dipetakan kelak |
| Integrasi SATUSEHAT | Phase 2 | Integrasi ke luar tidak boleh menghalangi pelayanan; infrastruktur pengirimannya juga belum berstatus milik platform | Data disiapkan dalam bentuk yang siap dipetakan |
| Penyerahan insiden keselamatan | Phase 2 | Sistem insiden rumah sakit belum ada di aplikasi ini | Komplikasi tetap dicatat lengkap sebagai fakta klinis |
| Verifikasi administrasi JKN dan SEP | Phase 3 | Konteks penjamin sudah dimiliki Registration; menghubungi BPJS langsung memerlukan kontrak eksternal | Penjamin pasien tetap ditampilkan dari data kunjungan |
| Indikator mutu dan audit layanan | Phase 3 | Definisi indikator resmi rumah sakit belum ada | Seluruh data sumbernya tersimpan |

---

## 9. Alur Bisnis Target

### `FLOW-HMD-MVP-001` — dari permintaan sampai tagihan

1. Dokter atau perawat unit peminta membuat permintaan cuci darah beserta alasan klinisnya.
2. Sistem memastikan pasien dan kunjungannya sah.
3. Koordinator unit HD menerima, menahan, atau meneruskan permintaan ke dokter untuk dinilai.
4. Petugas administrasi membuka program cuci darah bila pasien belum punya.
5. Dokter menilai kelayakan klinis pasien.
6. Akses pembuluh darah dicatat dan status serologi ditinjau.
7. Kebutuhan isolasi diputuskan.
8. Persetujuan tindakan diperiksa ketersediaannya.
9. Dokter membuat instruksi cuci darah lalu mengaktifkannya.
10. Koordinator menjadwalkan sesi pada tanggal, shift, mesin, dan station tertentu.
11. Koordinator menugaskan perawat dan dokter penanggung jawab sesi.
12. Sebelum shift dimulai, koordinator memeriksa kesiapan unit dan menyatakannya siap.
13. Pasien datang dan ditandai hadir.
14. Perawat mengisi penilaian sebelum tindakan.
15. Perawat memeriksa dua belas butir persiapan.
16. Perawat menyatakan sesi siap.
17. Perawat menekan Mulai; sistem memeriksa ulang mesin dan kunjungan tepat saat itu juga.
18. Sesi berjalan; waktu mulai diambil dari waktu server.
19. Perawat mencatat pengamatan berkala.
20. Perawat mencatat pemberian obat; faktanya diteruskan ke Farmasi.
21. Bila terjadi kejadian tidak diinginkan, perawat mencatatnya beserta penanganannya.
22. Cuci darah berakhir — selesai atau dihentikan beserta alasannya.
23. Perawat mengisi penilaian setelah tindakan dan tujuan pasien.
24. Perawat menyelesaikan dokumentasi; namanya dan waktunya tercatat.
25. Dokter penanggung jawab memeriksa catatan.
26. Dokter mengesahkan; dokumen didaftarkan ke daftar keutuhan rekam medis lalu dikunci.
27. Tindakan pasien diselesaikan.
28. Bila sesi berjalan sampai selesai, faktanya diserahkan ke penagihan.
29. Bila sesi dihentikan, tindakan ditandai tidak dapat ditagih beserta alasannya.
30. Koreksi setelah pengesahan dilakukan lewat tambahan, bukan dengan mengubah catatan asli.

---

## 10. Epic dan Functional Requirement

### `EPIC HMD-01` — Permintaan HD masuk

**Tujuan:** permintaan cuci darah dari luar unit menjadi data yang dapat diaudit, bukan catatan
telepon. **Disposisi:** `MISSING / NEW`.

> **`FR-HMD-001` — Permintaan wajib membawa konteks kunjungan**
>
> Sistem menolak permintaan yang tidak menyertakan kunjungan pasien yang sah.
>
> **Contoh:** dokter bangsal membuat permintaan untuk Bapak Darma, tetapi kunjungannya belum
> dibuat petugas pendaftaran. Permintaan ditolak dengan kode 400 dan pesan bahwa data kunjungan
> tidak ditemukan. Tidak ada satu baris permintaan pun tersimpan.

> **`FR-HMD-002` — Permintaan tidak pernah menjadi sesi dengan sendirinya**
>
> Permintaan yang diterima koordinator **tidak** membentuk sesi terjadwal. Sesi dibuat terpisah
> oleh koordinator, dengan mesin, station, dan perawat yang ia tentukan.
>
> **Contoh:** permintaan cito untuk Bapak Darma diterima pukul 14.10. Daftar kerja hari itu tidak
> bertambah satu baris pun sampai koordinator menjadwalkannya secara terpisah.

> **`FR-HMD-003` — Menahan dan menolak dipisahkan berdasarkan jenis alasannya**
>
> Koordinator boleh menahan permintaan dengan alasan operasional. Hanya dokter yang boleh
> menolaknya dengan alasan klinis.
>
> **Contoh:** koordinator membuka permintaan lalu menekan Tolak. Tombol itu tidak tersedia
> baginya. Yang tersedia adalah Tahan, dan ia mengisi alasan "seluruh mesin terpakai sampai
> pukul 18.00".

> **`FR-HMD-004` — Penolakan bersifat akhir**
>
> Permintaan yang sudah ditolak tidak dapat dikembalikan ke keadaan mana pun.
>
> **Contoh:** dokter menolak permintaan pukul 09.00. Pukul 15.00 kondisi pasien berubah dan HD
> menjadi diindikasikan. Yang dilakukan adalah membuat permintaan **baru**, sehingga penolakan
> pukul 09.00 tetap terbaca beserta alasannya.

### `EPIC HMD-02` — Program cuci darah dan kelayakan

**Tujuan:** setiap pasien HD punya satu program yang jelas batas awal dan akhirnya.
**Disposisi:** `MISSING / NEW`, dengan persetujuan tindakan `EXISTING / REUSE`.

> **`FR-HMD-010` — Satu pasien satu program aktif**
>
> Sistem menolak mengaktifkan program kedua selama masih ada program aktif untuk pasien yang
> sama.
>
> **Contoh:** Ibu Sinta punya program `HD-2026-00042` berstatus aktif. Petugas membuat program
> baru lalu menekan Aktifkan. Ditolak dengan kode 409. Program baru tetap tersimpan sebagai draf
> sehingga pekerjaan petugas tidak hilang.

> **`FR-HMD-011` — Program tidak dapat ditutup selama ada sesi yang belum disahkan**
>
> **Contoh:** petugas menutup program Ibu Sinta, padahal sesi tanggal 16 September masih menunggu
> pengesahan dokter. Ditolak dengan kode 422. Setelah dokter mengesahkan sesi itu, penutupan
> berhasil.

> **`FR-HMD-012` — Kelayakan selalu lahir dari keputusan manusia**
>
> Sistem tidak pernah menyimpulkan kelayakan sendiri dari data apa pun. Setiap penilaian mencatat
> dokter penilai, waktu, indikasi, keputusan, alasan, dan tindak lanjut bila keputusannya bukan
> layak.

> **`FR-HMD-013` — Status serologi dirujuk, tidak disalin**
>
> Hemodialisa menyimpan rujukan hasil, tanggal, ringkasan yang dibaca petugas, status tinjauan,
> dan keputusan operasional. Ia **tidak** menyimpan nilai hasil sebagai sumber kebenaran baru.

> **`FR-HMD-014` — Aturan isolasi Hepatitis B tidak berlaku otomatis untuk yang lain**
>
> **Contoh:** hasil anti-HCV Bapak Darma reaktif. Sistem **tidak** otomatis menandainya
> memerlukan mesin khusus. Yang terjadi adalah tinjauan dicatat, lalu tim PPI memutuskan
> kebutuhan isolasinya secara terpisah.

### `EPIC HMD-03` — Instruksi cuci darah

**Tujuan:** parameter setiap sesi berasal dari instruksi dokter yang tercatat dan tidak dapat
diubah diam-diam. **Disposisi:** `MISSING / NEW`.

> **`FR-HMD-020` — Instruksi aktif tidak dapat disunting**
>
> **Contoh:** dokter ingin menaikkan target penarikan cairan dari 2.000 ml menjadi 2.500 ml. Ia
> membuka instruksi aktif dan menekan Simpan. Ditolak dengan kode 423. Yang dilakukan adalah
> membuat instruksi baru bertarget 2.500 ml lalu mengaktifkannya; instruksi lama otomatis menjadi
> digantikan dan tetap terbaca.

> **`FR-HMD-021` — Satu program satu instruksi aktif**
>
> Mengaktifkan instruksi baru menggantikan yang lama dalam satu transaksi. Tidak pernah ada dua
> instruksi yang sama-sama merasa berlaku.

> **`FR-HMD-022` — Penyimpangan dicatat pada sesi, bukan dengan mengubah instruksi**
>
> **Contoh:** instruksi menargetkan penarikan 2.000 ml, tetapi tekanan darah pasien turun
> sehingga perawat hanya menarik 1.400 ml. Perawat mencatat jumlah sebenarnya dan alasannya pada
> sesi. Instruksi dokter tidak disentuh.

### `EPIC HMD-04` — Penjadwalan dan sumber daya

**Tujuan:** unit punya daftar kerja harian yang benar, dan tabrakan sumber daya mustahil terjadi.
**Disposisi:** `MISSING / NEW`.

> **`FR-HMD-030` — Tiga jenis tabrakan dicegah sekaligus**
>
> Sistem menolak jadwal yang membuat satu pasien, satu mesin, atau satu station terpakai dua kali
> pada rentang waktu yang bertumpang tindih.
>
> **Contoh:** koordinator A menjadwalkan Bapak Darma ke mesin `M-03` pukul 07.00–11.00.
> Koordinator B menjadwalkan Ibu Wulan ke mesin yang sama pukul 09.00–13.00 pada saat hampir
> bersamaan. Rentangnya bertumpang tindih dua jam. Satu berhasil, satu ditolak dengan kode 409.

> **`FR-HMD-031` — Mesin yang tidak siap tidak dapat dipakai**
>
> Mesin berstatus diblokir, dalam perawatan, atau dinyatakan tidak laik ditolak baik saat
> menjadwalkan maupun saat memulai sesi.

> **`FR-HMD-032` — Kebutuhan isolasi menentukan mesin dan station yang sah**
>
> **Contoh:** Ibu Sinta memerlukan mesin khusus. Koordinator memilih mesin umum `M-05`. Ditolak
> dengan kode 422. Setelah ia memilih `M-09` yang ditandai khusus, jadwal berhasil tersimpan.

> **`FR-HMD-033` — Setiap perubahan status mesin meninggalkan riwayat**
>
> **Contoh:** teknisi memblokir mesin `M-03` pada 12 September dengan alasan kebocoran. Pada 15
> September ia menyatakannya siap kembali. Kedua perubahan itu terbaca lengkap beserta alasan,
> nama pengubah, dan waktunya — tiga bulan kemudian sekalipun.

> **`FR-HMD-034` — Status kewenangan petugas punya tiga kemungkinan**
>
> Penugasan mencatat salah satu dari terverifikasi, tidak berwenang, atau **belum dapat
> diverifikasi**.
>
> **Contoh:** koordinator menugaskan perawat Rina pada sesi pagi. Karena pembacaan kewenangan
> dari Human Resource belum tersedia, statusnya tersimpan sebagai belum dapat diverifikasi —
> **bukan** terverifikasi. Saat audit, perbedaan itulah yang menjawab pertanyaan apakah
> pemeriksaan pernah dilakukan.

### `EPIC HMD-05` — Kesiapan unit

**Tujuan:** shift tidak berjalan sebelum unit dinyatakan siap oleh orang yang bertanggung jawab.
**Disposisi:** `MISSING / NEW`.

> **`FR-HMD-040` — Unit siap hanya bila seluruh butir wajib terpenuhi**
>
> **Contoh:** koordinator menekan Nyatakan siap padahal butir obat dan bahan masih belum
> diperiksa. Ditolak dengan kode 422 beserta penjelasan butir mana yang kurang.

> **`FR-HMD-041` — Hasil pemeriksaan air punya masa berlaku yang dapat disetel**
>
> **Contoh:** hasil pemeriksaan air terakhir tanggal 9 Agustus, dan hari ini 18 September —
> berumur 40 hari. Masa berlaku disetel 30 hari, sehingga unit tidak dapat dinyatakan siap.
> Setelah pengaturan diubah menjadi 60 hari oleh pemegang wewenang, unit dapat dinyatakan siap
> tanpa satu baris kode pun berubah.

> **`FR-HMD-042` — Unit tidak siap tidak menghentikan sesi yang sedang berjalan**
>
> **Contoh:** pukul 09.30 koordinator menyatakan unit tidak siap karena pengolahan air bermasalah.
> Tiga sesi yang sudah berjalan sejak pukul 07.00 **tidak** dihentikan otomatis. Menghentikannya
> adalah keputusan klinis perawat dan dokter di tempat.

### `EPIC HMD-06` — Persiapan dan memulai sesi

**Tujuan:** tidak ada jarum yang masuk sebelum dua belas pengaman diperiksa. **Disposisi:**
`MISSING / NEW`, dengan tanda vital `EXISTING / REUSE`.

> **`FR-HMD-050` — Dua belas butir persiapan diperiksa sebelum sesi dinyatakan siap**
>
> Setiap butir menyimpan hasil, waktu, dan pemeriksanya.

> **`FR-HMD-051` — Butir persiapan hanya dapat dilewati bila memang ditandai boleh dilewati**
>
> Daftar butir yang boleh dilewati adalah **data**, bukan aturan yang ditanam di kode.
>
> **Contoh:** pada rilis pertama seluruh butir bertanda tidak boleh dilewati, sehingga tombol
> Lewati selalu menghasilkan penolakan. Setelah badan klinis menetapkan bahwa butir "tinjauan
> serologi" boleh dilewati dengan alasan tertulis, pemegang akun tata kelola klinis mengubah
> penandanya lewat layar master. Tombol itu langsung bekerja — tanpa penerapan kode baru.

> **`FR-HMD-052` — Sesi tidak dapat dinyatakan siap tanpa dokter penanggung jawab**
>
> **Contoh:** perawat menekan Nyatakan siap pada sesi yang belum punya dokter penanggung jawab.
> Ditolak dengan kode 422. Koordinator menetapkan dokternya, lalu perawat mengulangi.

> **`FR-HMD-053` — Syarat diperiksa ulang tepat sebelum sesi dimulai**
>
> **Contoh:** sesi dinyatakan siap pukul 06.50. Pukul 06.58 teknisi memblokir mesinnya. Pukul
> 07.05 perawat menekan Mulai. Ditolak dengan kode 422 karena mesin berubah status. Tanpa
> pemeriksaan ulang, sesi akan berjalan pada mesin yang sedang bermasalah.

> **`FR-HMD-054` — Menekan Mulai dua kali tetap menghasilkan satu sesi dan satu tindakan**
>
> **Contoh:** jaringan lambat, perawat menekan Mulai dua kali. Permintaan kedua membawa penanda
> yang sama, sehingga sistem mengembalikan sesi yang sudah terbentuk. Daftar tindakan pasien
> berisi tepat satu baris.

> **`FR-HMD-055` — Waktu mulai diambil dari waktu server**
>
> **Contoh:** jam pada komputer perawat tertinggal 40 menit. Waktu mulai yang tersimpan tetap
> waktu server, bukan 06.32 seperti yang tertulis di layar komputer itu.

### `EPIC HMD-07` — Pemantauan, obat, dan komplikasi

**Tujuan:** ada bukti pasien dipantau sepanjang sesi. **Disposisi:** `MISSING / NEW`, dengan
pemakaian obat `EXTEND` ke Farmasi.

> **`FR-HMD-060` — Riwayat pemantauan tidak pernah saling menimpa**
>
> **Contoh:** perawat mencatat lima pengamatan pada pukul 07.30, 08.00, 08.30, 09.00, dan 09.30.
> Kelimanya terbaca lengkap dan urut. Sistem **tidak** menyimpan "nilai terakhir" saja.

> **`FR-HMD-061` — Pemberian obat diteruskan ke Farmasi, stok bukan milik Hemodialisa**
>
> **Contoh:** perawat mencatat pemberian heparin 2.000 unit lewat jalur intravena. Catatan klinis
> tersimpan di sesi; pengurangan stok dilakukan Farmasi.

> **`FR-HMD-062` — Kegagalan penerusan tidak menghapus catatan klinis**
>
> **Contoh:** penerusan ke Farmasi gagal karena gangguan sesaat. Catatan pemberian heparin
> **tetap** tersimpan pada sesi — obatnya memang sudah masuk ke tubuh pasien. Penerusan masuk
> daftar untuk diulang.

> **`FR-HMD-063` — Sistem tidak menyimpulkan diagnosis dari nilai pemantauan**
>
> Turunnya tekanan darah tidak otomatis menjadi catatan komplikasi. Komplikasi selalu lahir dari
> penilaian manusia beserta tindakan penanganannya.

### `EPIC HMD-08` — Penutupan sesi dan pengesahan catatan

**Tujuan:** catatan klinis punya kekuatan hukum dan tidak dapat diubah diam-diam. **Disposisi:**
`EXTEND` terhadap Rekam Medis.

> **`FR-HMD-070` — Cuci darah selesai secara fisik tidak sama dengan catatan selesai**
>
> Sesi yang berakhir masih melalui dua langkah: dokumentasi diselesaikan perawat, lalu disahkan
> dokter.

> **`FR-HMD-071` — Dua pelaku disimpan terpisah**
>
> **Contoh:** perawat Rina menyelesaikan dokumentasi pukul 11.40; dokter Rahmat mengesahkan pukul
> 13.15. Kedua nama dan kedua waktu tersimpan pada kolom yang berbeda.

> **`FR-HMD-072` — Hanya dokter penanggung jawab sesi itu yang boleh mengesahkan**
>
> **Contoh:** dokter Sari punya hak akses pengesahan, tetapi bukan penanggung jawab sesi Ibu
> Sinta. Ia menekan Sahkan dan ditolak dengan kode 403. Mesin hak akses saja tidak menahan ini;
> aturan bisnis yang menahannya.

> **`FR-HMD-073` — Pengesahan dan pendaftaran dokumen adalah satu paket**
>
> **Contoh:** pendaftaran dokumen ke rekam medis gagal. Seluruh langkah dibatalkan; sesi tetap
> menunggu pengesahan. Tidak ada keadaan setengah terkunci.

> **`FR-HMD-074` — Catatan yang sudah disahkan tidak dapat diubah langsung**
>
> **Contoh:** dua hari kemudian perawat menyadari berat badan akhir salah ketik — 58,5 kg padahal
> seharusnya 55,8 kg. Ia membuka catatan dan menekan Simpan. Ditolak dengan kode 423. Perbaikan
> dilakukan lewat koreksi; kedua angka tetap terbaca, yang asli sebagai catatan awal dan yang
> benar sebagai koreksi beserta alasan, penulis, dan waktunya.

### `EPIC HMD-09` — Serah terima ke penagihan

**Tujuan:** layanan yang dikerjakan sampai ke tagihan, dan yang tidak tuntas tidak tertagih
otomatis. **Disposisi:** `EXISTING / REUSE`.

> **`FR-HMD-080` — Sesi yang selesai menerbitkan tepat satu tagihan**
>
> Sumber tagihannya adalah sumber tindakan yang **sudah terdaftar**. Hemodialisa tidak membuat
> sumber tagihan baru.

> **`FR-HMD-081` — Sesi yang dihentikan tidak menagih otomatis**
>
> **Contoh:** Bapak Darma mulai pukul 07.12; pukul 07.52 tekanan darahnya turun tajam dan sesi
> dihentikan. Tindakan tetap tercatat lengkap, ditandai tidak dapat ditagih, beserta alasan
> penghentiannya. Tidak ada baris tagihan hemodialisa yang terbit. Bila rumah sakit berhak
> menagih bahan yang terpakai, kasir menambahkannya dari katalog tarif — terpisah dan terlacak
> sendiri.

> **`FR-HMD-082` — Kegagalan penagihan tidak membuka kembali catatan klinis**
>
> **Contoh:** penyerahan gagal tiga kali karena gangguan. Sesi **tetap** berstatus disahkan.
> Koordinator menjalankan ulang penyerahan, dan hanya satu tagihan terbentuk.

> **`FR-HMD-083` — Dua kolom dokter diisi dua orang yang berbeda perannya**
>
> **Contoh:** instruksi dibuat dokter Rahmat tiga minggu lalu; sesi hari ini ditangani dokter
> Sari sebagai penanggung jawab. Pada tindakan yang diserahkan ke penagihan, kolom dokter berisi
> Sari dan kolom dokter pemberi instruksi berisi Rahmat.

---

## 11. Model Status yang Diusulkan

Daftar lengkap beserta perpindahan yang sah dan tidak sah ada di
`contracts/state-transition-matrix.md`. Di sini hanya daftar dan invariant utamanya.

| Objek | Status | Invariant utama |
|---|---|---|
| Permintaan HD | `Requested`, `Accepted`, `OnHold`, `Rejected`, `Cancelled`, `Fulfilled` | Penolakan bersifat akhir. Permintaan tidak pernah menjadi sesi sendiri |
| Program HD | `Draft`, `Active`, `Suspended`, `Closed` | Satu pasien satu program aktif. Program tertutup tidak dapat dibuka kembali |
| Instruksi cuci darah | `Draft`, `Active`, `Superseded`, `Cancelled` | Satu program satu instruksi aktif. Instruksi aktif tidak dapat disunting |
| Sesi HD | `Planned`, `Scheduled`, `CheckedIn`, `PreCheck`, `Held`, `Ready`, `InProgress`, `Stopped`, `Completed`, `AwaitingFinalization`, `Finalized`, `Cancelled` | Sesi yang sudah berjalan tidak dapat dibatalkan. Sesi yang sudah disahkan tidak dapat diubah |
| Mesin | `Ready`, `Blocked`, `Maintenance`, `NotEligible` | Setiap perubahan meninggalkan riwayat. Tiga status terakhir tidak dapat dipakai |
| Station | `Available`, `Blocked`, `Maintenance` | — |
| Kesiapan unit | `Draft`, `Ready`, `NotReady` | Siap hanya bila seluruh butir wajib terpenuhi. Status `ConditionallyReady` sengaja **tidak** dipakai |
| Verifikasi kewenangan | `Verified`, `NotAuthorized`, `NotVerifiable` | Nilai ketiga tidak boleh ditulis sebagai nilai pertama |

---

## 12. Sasaran Arsitektur

| Kelompok | Isi |
|---|---|
| **Dipakai ulang apa adanya** | Pasien, kunjungan, episode rawat inap, dokter, profil petugas, unit layanan, ruang, master tindakan, persetujuan tindakan, tanda vital, tindakan pasien, jalur ke penagihan, keutuhan dokumen, koreksi dokumen, jejak akses rekam medis, sembilan komponen kerangka layar klinis |
| **Diperluas** | Jenis unit layanan bertambah satu nilai; jenis dokumen klinis bertambah satu nilai; **dan himpunan jenis dokumen yang ditegakkan aturannya bertambah satu nilai** — dua tempat, bukan satu |
| **Dibuat baru** | Dua puluh dua tabel ber-prefix `Hmd`, sepuluh layanan, tujuh controller, sebelas layar, satu migration |
| **Sengaja tidak dibuat** | Salinan pasien, kunjungan, persetujuan, tanda vital, hasil laboratorium, stok obat, tagihan, keutuhan dokumen, dan sumber tagihan baru. Daftar lengkap beserta alasannya ada di `02-backend-architecture.md` |

**Satu hal yang wajib dibaca sebelum implementasi dimulai:** menambahkan jenis dokumen sesi HD
pada daftar jenis **tidak cukup**. Ada himpunan tertutup kedua di dalam layanan keutuhan dokumen
yang menentukan apakah penguncian benar-benar berlaku. Bila langkah kedua terlewat, sesi akan
terlihat sudah disahkan tetapi **tetap dapat disunting**, dan tidak ada pesan error apa pun yang
menjelaskannya.

---

## 13. Sasaran Kemampuan API

Seluruhnya bagian dari `contracts/api-contract.md` dan tidak melebihinya. Yang ditampilkan di
sini adalah endpoint yang membuktikan requirement; daftar lengkapnya ada di kontrak.

### Health Services / Hemodialysis Management / Hemodialysis Order

Base URL: `api/v1/health-services/hemodialysis-management/hemodialysis-orders`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
|---|---|---|---|---|---|---|---|
| `POST` | `/` | Membuat permintaan HD | `HemodialysisOrder : Create` | `CreateHmdOrderRequest` | `ApiResponse<HmdOrderResponse>` | `EPIC HMD-01` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/accept` | Menerima permintaan | `HemodialysisOrder : Accept` | `AcceptHmdOrderRequest` | `ApiResponse<HmdOrderResponse>` | `EPIC HMD-01` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/reject` | Menolak permintaan | `HemodialysisOrder : Reject` | `RejectHmdOrderRequest` | `ApiResponse<HmdOrderResponse>` | `EPIC HMD-01` | **Rencana (belum tersedia)** |

### Health Services / Hemodialysis Management / Hemodialysis Schedule

Base URL: `api/v1/health-services/hemodialysis-management/hemodialysis-sessions`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
|---|---|---|---|---|---|---|---|
| `GET` | `/worklist` | Daftar kerja unit | `HemodialysisSchedule : Read` | `HmdWorklistQuery` | `ApiResponse<PagedResult<HmdWorklistItemResponse>>` | `EPIC HMD-04` | **Rencana (belum tersedia)** |
| `POST` | `/` | Menjadwalkan sesi | `HemodialysisSchedule : Create` | `CreateHmdSessionRequest` | `ApiResponse<HmdSessionResponse>` | `EPIC HMD-04` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}/staff-assignments` | Menetapkan petugas | `HemodialysisSchedule : Update` | `AssignHmdStaffRequest` | `ApiResponse<List<HmdStaffAssignmentResponse>>` | `EPIC HMD-04` | **Rencana (belum tersedia)** |

### Health Services / Hemodialysis Management / Hemodialysis Session

Base URL: `api/v1/health-services/hemodialysis-management/hemodialysis-sessions`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
|---|---|---|---|---|---|---|---|
| `PUT` | `/{id}/checklist` | Menyimpan hasil persiapan | `HemodialysisSession : Update` | `SaveHmdChecklistRequest` | `ApiResponse<List<HmdSessionChecklistResponse>>` | `EPIC HMD-06` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/checklist/{itemId}/override` | Melewati satu butir persiapan | `HemodialysisSession : OverrideChecklist` | `OverrideHmdChecklistRequest` | `ApiResponse<HmdSessionChecklistResponse>` | `EPIC HMD-06` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/ready` | Menyatakan sesi siap | `HemodialysisSession : DeclareReady` | — | `ApiResponse<HmdSessionResponse>` | `EPIC HMD-06` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/start` | Memulai cuci darah | `HemodialysisSession : Start` | `StartHmdSessionRequest` | `ApiResponse<HmdSessionResponse>` | `EPIC HMD-06` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/observations` | Mencatat pemantauan | `HemodialysisObservation : Create` | `CreateHmdObservationRequest` | `ApiResponse<HmdObservationResponse>` | `EPIC HMD-07` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/submit-documentation` | Menyelesaikan dokumentasi | `HemodialysisSession : SubmitDocumentation` | — | `ApiResponse<HmdSessionResponse>` | `EPIC HMD-08` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/finalize` | Mengesahkan dan mengunci | `HemodialysisRecord : Finalize` | `FinalizeHmdSessionRequest` | `ApiResponse<HmdSessionResponse>` | `EPIC HMD-08` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/billing-handoff/retry` | Mengulang penyerahan tagihan | `HemodialysisSession : RetryBillingHandoff` | — | `ApiResponse<HmdBillingHandoffResponse>` | `EPIC HMD-09` | **Rencana (belum tersedia)** |

### Health Services / Hemodialysis Management / Master Data / Hemodialysis Checklist Item

Base URL: `api/v1/health-services/hemodialysis-management/master-data/hemodialysis-checklist-items`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
|---|---|---|---|---|---|---|---|
| `PATCH` | `/{id}/overridable` | Menetapkan butir boleh dilewati | `HemodialysisChecklistItem : SetOverridable` | `SetOverridableRequest` | `ApiResponse<HmdChecklistItemResponse>` | `EPIC HMD-06` | **Rencana (belum tersedia)** |

Endpoint terakhir adalah yang membuat kebijakan pelewatan butir persiapan dapat dicabut **tanpa
penerapan kode baru**.

---

## 14. Matriks Kewenangan

Diturunkan dari `contracts/permission-audit-matrix.md`, memakai string yang persis.

| Peran kerja | Butir hak akses utama |
|---|---|
| Dokter atau perawat unit peminta | `HemodialysisOrder : Create`, `: Read`, `: Cancel` |
| Petugas administrasi HD | `HemodialysisEpisode : Read`, `: Create`, `: Update`, `: ChangeStatus` |
| Koordinator unit HD | `HemodialysisOrder : Accept`, `: Hold`; `HemodialysisSchedule : Create`, `: Update`, `: Cancel`, `: Read`; `HemodialysisUnitReadiness : DeclareReady`, `: DeclareNotReady`; `HemodialysisMachine : ChangeStatus` |
| Dokter dialisis | `HemodialysisOrder : Reject`; `HemodialysisEligibility : Decide`; `HemodialysisPrescription : Activate`; `HemodialysisIsolation : Decide`; `HemodialysisSession : OverrideChecklist` |
| Dokter penanggung jawab sesi | `HemodialysisRecord : Finalize` |
| Perawat dialisis | `HemodialysisSession : CheckIn`, `: Update`, `: DeclareReady`, `: Start`, `: Stop`, `: Complete`, `: SubmitDocumentation`; `HemodialysisObservation : Create`; `HemodialysisMedication : Administer`; `HemodialysisComplication : Create` |
| Tim PPI | `HemodialysisSerology : Read`, `: Create`; `HemodialysisIsolation : Decide` |
| Pemegang akun tata kelola klinis | `HemodialysisChecklistItem : SetOverridable`; `HemodialysisSetting : Update` |
| Auditor berwenang | Seluruh butir `: Read` |

**Empat kewenangan yang tidak dapat dijaga mesin hak akses saja** — dan karenanya dijaga aturan
bisnis — ada di `contracts/permission-audit-matrix.md` bagian 4. Yang paling penting: mesin hak
akses tahu seorang dokter boleh mengesahkan, tetapi tidak tahu **sesi yang mana** yang ia
tanggung jawabi.

---

## 15. Batas Integrasi dan Penagihan

Hemodialisa **tidak membuat sendiri**:

| Yang tidak dibuat | Pemiliknya |
|---|---|
| Pasien, kunjungan, jenis kunjungan baru | Patient Management, Registration Management |
| Persetujuan tindakan | Clinical Management |
| Hasil laboratorium, termasuk salinannya | Laboratorium |
| Stok, batch, dan penyaluran obat | Farmasi |
| Harga, tagihan, pembayaran, klaim, dan **sumber tagihan baru** | Billing dan Kasir |
| Keutuhan dokumen dan mekanisme koreksi | Rekam Medis |
| Data kompetensi, nomor STR, SIP, dan sertifikat | Human Resource |
| Sistem insiden keselamatan pasien | Belum ada di aplikasi ini |

**Hemodialisa tidak memanggil satu pun sistem di luar rumah sakit pada rilis ini.** Tidak ada
SATUSEHAT, tidak ada BPJS, tidak ada pembacaan perangkat mesin.

---

## 16. Guardrail Regulasi

| Kewajiban | Wujudnya pada MVP |
|---|---|
| Identifikasi pasien | Butir persiapan pertama memeriksa kecocokan identitas; konteks pasien yang gagal diverifikasi menghentikan seluruh penulisan data klinis |
| Keutuhan rekam medis | Catatan yang sudah disahkan tidak dapat diubah langsung; koreksi lewat tambahan beserta alasan, penulis, dan waktunya |
| Keterlusuran | Sepuluh kejadian penting meninggalkan jejak yang melekat pada data, bukan hanya pada log aplikasi |
| Kerahasiaan | Status serologi hanya terbaca peran yang membutuhkannya; tidak tampil pada daftar kerja harian maupun daftar pasien; tidak masuk payload logger |
| Otorisasi | Ditegakkan di server pada setiap endpoint; menonaktifkan tombol di layar bukan pengaman |
| Kompetensi tenaga | Status kewenangan petugas dicatat pada setiap penugasan, dengan nilai jujur ketika pemeriksaan belum dapat dilakukan |
| Kelaikan peralatan | Status mesin beserta riwayat perubahannya; mesin tidak laik ditolak pada penjadwalan maupun saat memulai |

**Dokumen ini tidak mengklaim kepatuhan hukum final.** Prosedur klinis dan kebijakan rumah sakit
tetap harus disahkan pemiliknya, dan itu tercatat sebagai gerbang go-live pada bagian 20.

---

## 17. Kebutuhan Non-Fungsional

| ID | Kebutuhan | Wujudnya |
|---|---|---|
| `NFR-001` | Keutuhan transaksi | Memulai sesi dan mengesahkan catatan masing-masing satu transaksi utuh. Gagal di tengah membatalkan seluruhnya |
| `NFR-002` | Pencegahan tabrakan | Tiga jenis tabrakan diperiksa di dalam transaksi, didukung index basis data |
| `NFR-003` | Kekebalan terhadap pengiriman ganda | Memulai sesi membawa penanda idempotency; penyerahan tagihan yang diulang tidak pernah menghasilkan tagihan ganda |
| `NFR-004` | Waktu authoritative | Waktu mulai, selesai, pengesahan, pemberian obat, dan pengamatan memakai waktu server |
| `NFR-005` | Jejak audit | Sepuluh kejadian penting meninggalkan jejak yang melekat pada data |
| `NFR-006` | Privasi | Kolom sensitif tidak masuk payload logger dan tidak dipakai sebagai contoh berisi data asli |
| `NFR-007` | Gagal tertutup | Konteks pasien yang gagal diverifikasi menghentikan seluruh penulisan data klinis |
| `NFR-008` | Catatan final tetap final | Kegagalan penagihan tidak pernah membuka kembali catatan yang sudah disahkan |
| `NFR-009` | Empat keadaan layar | Memuat, kosong, gagal, dan berisi digambar untuk setiap daftar |
| `NFR-010` | Tidak ada data basi | Berpindah pasien mengosongkan layar lebih dulu |
| `NFR-011` | Kebijakan berupa data | Butir persiapan yang boleh dilewati, batas rasio perawat, masa berlaku hasil air, dan sakelar penegakan kewenangan seluruhnya dapat diubah tanpa penerapan kode |

`NFR-011` adalah kebutuhan yang paling mudah terlewat dan paling mahal bila dilanggar. Ia yang
membuat empat keputusan klinis yang belum turun dapat diterapkan kelak tanpa membongkar apa pun.

---

## 18. Skenario UAT

Setiap epic `MUST HAVE` punya sekurang-kurangnya satu skenario berhasil dan satu gagal. Seluruh
nama adalah samaran.

> **`UAT-01` — Permintaan cito dari bangsal berhasil** (`EPIC HMD-01`)
>
> **Kondisi awal:** Bapak Darma sedang dirawat di bangsal, kunjungannya sah.
> **Langkah:** dokter bangsal membuka layanan penunjang, memilih Hemodialisa, mengisi alasan
> klinis, menandai cito, lalu mengirim.
> **Hasil yang diharapkan:** permintaan muncul pada daftar permintaan masuk unit HD, bertanda
> cito, lengkap dengan asal bangsalnya.

> **`UAT-02` — Koordinator mencoba menolak permintaan** (`EPIC HMD-01`, jalur gagal)
>
> **Kondisi awal:** ada permintaan berstatus menunggu.
> **Langkah:** koordinator membuka permintaan itu dan mencari tombol Tolak.
> **Hasil yang diharapkan:** tombol Tolak **tidak tersedia** baginya. Yang tersedia Terima dan
> Tahan.

> **`UAT-03` — Program HD dibuka dan diaktifkan** (`EPIC HMD-02`)
>
> **Kondisi awal:** Ibu Sinta belum punya program HD.
> **Langkah:** petugas administrasi membuka program baru, memilih dokter penanggung jawab, lalu
> mengaktifkan.
> **Hasil yang diharapkan:** program berstatus aktif dan muncul pada daftar pasien HD.

> **`UAT-04` — Program kedua ditolak** (`EPIC HMD-02`, jalur gagal)
>
> **Kondisi awal:** Ibu Sinta sudah punya program aktif.
> **Langkah:** petugas membuat program kedua lalu menekan Aktifkan.
> **Hasil yang diharapkan:** ditolak dengan pesan bahwa pasien sudah punya program aktif. Daftar
> pasien tetap menampilkan satu program aktif.

> **`UAT-05` — Instruksi diganti, riwayatnya utuh** (`EPIC HMD-03`)
>
> **Kondisi awal:** ada instruksi aktif bertarget penarikan 2.000 ml.
> **Langkah:** dokter membuat instruksi baru bertarget 2.500 ml lalu mengaktifkannya.
> **Hasil yang diharapkan:** instruksi baru aktif, yang lama menjadi digantikan, dan keduanya
> terbaca pada riwayat.

> **`UAT-06` — Instruksi aktif dicoba disunting** (`EPIC HMD-03`, jalur gagal)
>
> **Langkah:** dokter membuka instruksi aktif, mengubah angka, menekan Simpan.
> **Hasil yang diharapkan:** ditolak dengan pesan bahwa instruksi aktif tidak dapat diubah,
> beserta saran membuat instruksi baru.

> **`UAT-07` — Dua koordinator merebut mesin yang sama** (`EPIC HMD-04`, jalur gagal)
>
> **Kondisi awal:** mesin `M-03` kosong pada 18 September.
> **Langkah:** dua koordinator menjadwalkan dua pasien berbeda ke mesin itu pada rentang waktu
> bertumpang tindih, pada waktu hampir bersamaan.
> **Hasil yang diharapkan:** satu berhasil, satu ditolak dengan pesan yang terbaca. Daftar kerja
> menampilkan tepat satu sesi pada mesin itu.

> **`UAT-08` — Penjadwalan normal berhasil** (`EPIC HMD-04`)
>
> **Kondisi awal:** pasien punya program dan instruksi aktif; mesin dan station kosong.
> **Langkah:** koordinator menjadwalkan sesi, menugaskan perawat dan dokter penanggung jawab.
> **Hasil yang diharapkan:** sesi muncul pada daftar kerja beserta perawat dan dokternya.

> **`UAT-09` — Unit dinyatakan siap** (`EPIC HMD-05`)
>
> **Langkah:** koordinator memeriksa seluruh butir kesiapan lalu menyatakan unit siap.
> **Hasil yang diharapkan:** status unit menjadi siap dan terlihat pada pita di daftar kerja.

> **`UAT-10` — Hasil pemeriksaan air kedaluwarsa** (`EPIC HMD-05`, jalur gagal)
>
> **Kondisi awal:** hasil pemeriksaan air berumur 40 hari; masa berlaku disetel 30 hari.
> **Langkah:** koordinator menekan Nyatakan siap.
> **Hasil yang diharapkan:** ditolak dengan pesan bahwa hasil pemeriksaan air sudah kedaluwarsa.

> **`UAT-11` — Sesi dimulai dengan persiapan lengkap** (`EPIC HMD-06`)
>
> **Langkah:** perawat mengisi penilaian, memeriksa dua belas butir, menyatakan siap, lalu
> menekan Mulai.
> **Hasil yang diharapkan:** sesi berjalan, waktu mulai terisi waktu server, dan satu tindakan
> pasien terbentuk.

> **`UAT-12` — Butir persiapan dicoba dilewati** (`EPIC HMD-06`, jalur gagal)
>
> **Kondisi awal:** satu butir belum terpenuhi; seluruh butir bertanda tidak boleh dilewati.
> **Langkah:** dokter menekan Lewati dan mengisi alasan.
> **Hasil yang diharapkan:** ditolak dengan pesan bahwa butir itu tidak dapat dilewati.

> **`UAT-13` — Tombol Mulai ditekan dua kali** (`EPIC HMD-06`, jalur gagal)
>
> **Langkah:** perawat menekan Mulai, jaringan lambat, ia menekan lagi.
> **Hasil yang diharapkan:** satu sesi berjalan dan satu tindakan pasien. Bukan dua.

> **`UAT-14` — Pemantauan berkala tercatat lengkap** (`EPIC HMD-07`)
>
> **Langkah:** perawat mencatat lima pengamatan sepanjang sesi.
> **Hasil yang diharapkan:** kelimanya terbaca urut waktu, tidak ada yang tertimpa.

> **`UAT-15` — Pemberian obat tanpa dosis** (`EPIC HMD-07`, jalur gagal)
>
> **Langkah:** perawat mencatat pemberian obat tanpa mengisi dosis.
> **Hasil yang diharapkan:** ditolak dengan pesan bahwa catatan pemberian obat belum lengkap.

> **`UAT-16` — Catatan disahkan dan terkunci** (`EPIC HMD-08`)
>
> **Langkah:** perawat menyelesaikan dokumentasi, dokter penanggung jawab mengesahkan.
> **Hasil yang diharapkan:** sesi berstatus disahkan; nama penyelesai dan pengesah berbeda dan
> keduanya tercatat.

> **`UAT-17` — Perawat mencoba mengesahkan** (`EPIC HMD-08`, jalur gagal)
>
> **Langkah:** perawat membuka bagian finalisasi.
> **Hasil yang diharapkan:** tombol Sahkan **tidak tersedia** baginya.

> **`UAT-18` — Catatan final dicoba diubah** (`EPIC HMD-08`, jalur gagal)
>
> **Kondisi awal:** sesi sudah disahkan dua hari lalu; berat badan akhir salah ketik.
> **Langkah:** perawat membuka catatan dan menekan Simpan.
> **Hasil yang diharapkan:** ditolak beserta arahan memakai koreksi rekam medis. Setelah koreksi
> dibuat, kedua angka terbaca: yang asli dan yang benar beserta alasannya.

> **`UAT-19` — Sesi selesai menerbitkan tagihan** (`EPIC HMD-09`)
>
> **Langkah:** sesi berjalan sampai selesai lalu disahkan.
> **Hasil yang diharapkan:** kasir melihat tepat satu baris tagihan hemodialisa pada tagihan
> pasien.

> **`UAT-20` — Sesi dihentikan tidak menagih** (`EPIC HMD-09`, jalur gagal)
>
> **Kondisi awal:** Bapak Darma mulai pukul 07.12, dihentikan pukul 07.52.
> **Langkah:** perawat menghentikan sesi dan mengisi alasan; dokumentasi diselesaikan; dokter
> mengesahkan.
> **Hasil yang diharapkan:** tidak ada baris tagihan hemodialisa yang terbit. Pada catatan
> klinis, tindakan tetap tercatat lengkap beserta alasan penghentiannya.

> **`UAT-21` — Penagihan gagal tidak membuka catatan** (`EPIC HMD-09`, jalur gagal)
>
> **Langkah:** penyerahan digagalkan secara sengaja, lalu dijalankan ulang tiga kali.
> **Hasil yang diharapkan:** sesi **tetap** berstatus disahkan sepanjang waktu; setelah berhasil,
> tepat satu tagihan terbentuk.

> **`UAT-22` — Konteks pasien gagal diverifikasi** (lintas epic, jalur gagal)
>
> **Langkah:** ruang kerja sesi dibuka dalam keadaan konteks pasien tidak dapat diverifikasi.
> **Hasil yang diharapkan:** tidak ada data klinis tampil, tidak ada tombol yang menulis data,
> hanya pesan beserta tombol coba lagi.

---

## 19. Definition of Done

Setiap butir dijawab **ya** atau **belum**, beserta buktinya.

| Butir | Bukti |
|---|---|
| Baris registry `Hmd` sudah ada di kedua salinan | Pemeriksa kesesuaian backend lolos untuk entity `Hmd*` |
| Pasien dan kunjungan yang sudah ada dipakai ulang, tidak disalin | Tabel kepemilikan data pada `02-backend-architecture.md` |
| Permintaan HD dari bangsal menjadi data yang dapat diaudit | `UAT-01` |
| Koordinator tidak dapat menolak permintaan | `UAT-02` |
| Satu pasien hanya punya satu program HD aktif | `UAT-03`, `UAT-04` |
| Instruksi aktif tidak dapat disunting, penggantiannya terlacak | `UAT-05`, `UAT-06` |
| Tabrakan pasien, mesin, dan station tidak mungkin terjadi | `UAT-07`, `UAT-08` |
| Mesin tidak laik tidak dapat dipakai | Uji integrasi mesin diblokir pada matriks uji bagian 3 |
| Setiap perubahan status mesin punya riwayat | Uji integrasi riwayat status pada matriks uji bagian 3 |
| Unit tidak dapat dinyatakan siap dengan butir wajib yang belum terpenuhi | `UAT-09`, `UAT-10` |
| Dua belas butir persiapan diperiksa sebelum sesi dinyatakan siap | `UAT-11` |
| Tidak ada butir persiapan yang dapat dilewati sebelum badan klinis menetapkan | `UAT-12` |
| Sesi tidak dapat dinyatakan siap tanpa dokter penanggung jawab | Uji integrasi pada matriks uji bagian 5 |
| Syarat diperiksa ulang tepat sebelum sesi dimulai | Uji integrasi mesin berubah status pada matriks uji bagian 5 |
| Menekan Mulai dua kali menghasilkan satu sesi dan satu tindakan | `UAT-13` |
| Waktu kritis memakai waktu server | Uji integrasi waktu palsu pada matriks uji bagian 5 |
| Riwayat pemantauan tidak saling menimpa | `UAT-14` |
| Pemakaian obat diteruskan ke Farmasi, dan kegagalannya tidak menghapus catatan klinis | `UAT-15` dan uji integrasi pada matriks uji bagian 6 |
| Dokumentasi dan pengesahan dikerjakan dua orang berbeda | `UAT-16`, `UAT-17` |
| Hanya dokter penanggung jawab sesi yang dapat mengesahkan | Uji integrasi pada matriks uji bagian 7 |
| **Catatan yang sudah disahkan benar-benar terkunci** | `UAT-18` **dan** uji khusus Temuan Kritis 1 pada matriks uji bagian 7 |
| Sesi selesai menerbitkan tepat satu tagihan | `UAT-19` |
| Sesi dihentikan tidak menerbitkan tagihan otomatis | `UAT-20` |
| Kegagalan penagihan tidak membuka kembali catatan | `UAT-21` |
| Dua kolom dokter terisi sesuai perannya | Uji integrasi pada matriks uji bagian 8 |
| Hak akses ditegakkan di server pada setiap endpoint | Uji otorisasi pada matriks uji bagian 9 |
| Status serologi tidak tampil pada layar yang dilihat banyak orang | Uji penerimaan privasi pada matriks uji bagian 9 |
| Konteks pasien yang gagal diverifikasi menghentikan seluruh penulisan | `UAT-22` |
| Seluruh layar dapat dicapai lewat menu atau layar induknya | Uji keterjangkauan pada matriks uji bagian 10 |
| Seluruh tabel master rilis pertama sudah terisi | Rencana data master awal pada `02-backend-architecture.md` bagian 9 |
| Kebijakan dapat diubah tanpa penerapan kode baru | Uji integrasi pengaturan pada matriks uji bagian 3, 4, dan 5 |

**Yang sengaja tidak dipakai sebagai butir:** "kualitas kode baik", "sudah dites", dan
"performanya bagus". Ketiganya tidak dapat dijawab ya atau belum tanpa berdebat.

---

## 20. Urutan Pengiriman dan Pertanyaan Terbuka

### 20.1 Gelombang pengiriman

| Gelombang | Isi | Epic | Syarat mulai |
|---|---|---|---|
| `MVP-0` | Baris registry, dua puluh dua tabel, migration, konfigurasi, pendaftaran layanan, butir hak akses, data master awal, dua perluasan enum **dan** himpunan jenis dokumen yang ditegakkan | — | Blueprint disetujui pemilik |
| `MVP-1` | Mesin, station, pengaturan unit, butir persiapan, kesiapan unit | `EPIC HMD-05` | `MVP-0` selesai |
| `MVP-2` | Permintaan HD masuk, program HD, kelayakan, akses vaskular, serologi, isolasi, instruksi cuci darah | `EPIC HMD-01`, `EPIC HMD-02`, `EPIC HMD-03` | `MVP-1` selesai |
| `MVP-3` | Penjadwalan, daftar kerja, penugasan petugas | `EPIC HMD-04` | `MVP-2` selesai |
| `MVP-4` | Persiapan, memulai sesi, pemantauan, obat, komplikasi | `EPIC HMD-06`, `EPIC HMD-07` | `MVP-3` selesai |
| `MVP-5` | Penilaian setelah tindakan, tujuan pasien, penyelesaian dokumentasi, pengesahan, penguncian, koreksi, serah terima penagihan | `EPIC HMD-08`, `EPIC HMD-09` | `MVP-4` selesai |
| `POST-MVP` | Seluruh kemampuan pada bagian 8 | — | Di luar cakupan rilis pertama |

Tidak ada epic berstatus `OPEN DECISION` pada gelombang mana pun. Seluruh sembilan epic
berdisposisi `MISSING / NEW`, `EXTEND`, atau `EXISTING / REUSE`.

**Urutan `MVP-1` sebelum `MVP-2` disengaja.** Tanpa mesin dan station yang terdaftar, tidak ada
sesi yang dapat dijadwalkan sama sekali — sehingga membangun program dan instruksi lebih dulu
akan menghasilkan data yang belum bisa dipakai.

### 20.2 Pertanyaan terbuka sebelum development lock

| Pertanyaan | Siapa yang menjawab | Dampak bila belum dijawab | Memblokir |
|---|---|---|:---:|
| Siapa badan tata kelola klinis modul ini? | Manajemen rumah sakit | Tidak ada pihak sah yang bertanggung jawab atas keputusan keselamatan pada modul ini. Modul **tidak boleh** melayani pasien sungguhan | **Go-live**, bukan development |
| Butir persiapan mana yang boleh dilewati dokter? | Badan klinis | Seluruh butir tetap tidak boleh dilewati. Unit terkunci pada aturan paling ketat | **Go-live**, bukan development |
| Siapa berwenang mengesahkan catatan sesi? | Badan klinis | Berjalan dengan aturan bawaan dua langkah oleh dokter penanggung jawab sesi | **Go-live**, bukan development |
| Siapa berwenang menolak permintaan HD? | Badan klinis | Berjalan dengan aturan bawaan: menahan oleh koordinator, menolak oleh dokter | **Go-live**, bukan development |
| Apakah orang kedua perlu ditunjuk agar penyusun tidak sekaligus mengesahkan? | Badan klinis | Pengaturan bawaan menghendaki dua orang berbeda. Bila unit hanya punya satu dokter, alur pengesahan dapat macet | **Go-live**, bukan development |
| Kapan pembacaan hasil laboratorium per pasien tersedia? | Pemilik Laboratorium | Status serologi dicatat manual oleh petugas. Tidak menghalangi satu sesi pun | Tidak |
| Kapan pembacaan kewenangan klinis tersedia? | Pemilik Human Resource | Status kewenangan tercatat sebagai belum dapat diverifikasi. Penegakan tetap mati | Tidak |
| Berapa masa berlaku hasil pemeriksaan air menurut kebijakan unit? | Koordinator unit HD | Berjalan dengan nilai awal 30 hari, dapat diubah kapan saja lewat pengaturan | Tidak |
| Apakah batas tiga pasien per perawat diberlakukan sebagai batas keras? | Badan klinis | Berjalan sebagai peringatan, tidak menolak penjadwalan | Tidak |

**Tidak satu pun pertanyaan di atas memblokir development.** Kelima yang pertama memblokir
**go-live**, dan itu perbedaan yang disengaja: seluruh mekanismenya dibangun sekarang, dan yang
menunggu hanyalah nilai kebijakannya — yang seluruhnya berupa data, bukan kode.

Karena itu dokumen ini **boleh** diteruskan ke perencanaan pengiriman setelah pemilik
menyetujuinya, dengan catatan gerbang go-live tetap terbuka dan tercatat.
