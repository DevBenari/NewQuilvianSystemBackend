# Rawat Inap — PRD ke MVP

## 1. Identitas dokumen

| Field | Nilai |
| --- | --- |
| Produk | Quilvian Hospital Information System |
| Modul | Rawat Inap — `InPatientManagement`, prefix entity `Inp`, lifecycle registry `ACTIVE` sejak `RWI-DEC-068` |
| Blueprint ID | `RWI-BP-001` |
| `contract_version` | `0.4.0` |
| Status | `draft` — **belum disetujui manusia** |
| Repository target | `NewQuilvianSystemBackend` dan `QuilvianSystemFrontendDev` |
| Backend SHA baseline | `5afb54bd75281648010e50ef14f43ca1f80d8efd` |
| Frontend SHA baseline | `dec4fdeff07c3c96ad9f07f41f184c54cf771371` |
| Masukan | `02-backend-architecture.md` rev `0.3`; `contracts/api-contract.md`, `contracts/validation-matrix.md`, `contracts/permission-audit-matrix.md` rev `0.3.0`; `erd/01-inpatient-episode.md` dan `erd/data-dictionary.md` rev `0.3`; `00-interview-decisions.md` rev `5`; `evidence/03-hospital-domain-architecture.md` rev `0.1` (`DOMAIN_ARCHITECTURE_PARTIAL`) |
| Ringkasan cakupan | Satu pasien dapat dirawat inap dari admisi sampai episode ditutup dan tempat tidur kembali kosong, tanpa dokumentasi klinis, tanpa resep, dan tanpa jalur masuk IGD |

**Perubahan pada `contract_version` `0.3.0`.** Tiga keputusan penutupan butir organisasi
2026-08-21 masuk ke dokumen ini. **Satu kemampuan berpindah dari daftar ditunda ke dalam MVP**, dan
**satu epic baru lahir** — satu-satunya epic baru sejak dokumen ini disusun.

| Keputusan | Masuk ke |
| --- | --- |
| `RWI-DEC-064` jenis kelamin dan isolasi menjadi aturan yang **menolak** penempatan | Bagian 7 dan 8 (kemampuan berpindah), `EPIC RI-34` baru |
| `RWI-DEC-065` kebutuhan isolasi menjadi atribut episode | `EPIC RI-34`, `FR-RI-158` s.d. `FR-RI-161`, bagian 11, 13, dan 14 |
| `RWI-DEC-066` seluruh kamar tidak boleh ditempati campur, tanpa kolom baru pada `MstRoom` | `EPIC RI-34`, `FR-RI-154` s.d. `FR-RI-157` |

Satu gerbang keras sebelum produksi pada bagian 16 **dicabut** karena keputusannya sudah turun, dan
tiga pertanyaan memblokir pada bagian 20.2 tertutup.

**Perubahan pada `contract_version` `0.2.0`.** Empat keputusan Amendment Pass 2026-08-21 masuk ke
dokumen ini. Tidak ada kemampuan `MUST HAVE` yang dicabut dan tidak ada epic baru; keempatnya
menempel pada epic yang sudah ada.

| Keputusan | Masuk ke |
| --- | --- |
| `RWI-DEC-054` satu pasien satu episode | `EPIC RI-23`, `FR-RI-148` |
| `RWI-DEC-055` kepergian fisik pasien | `EPIC RI-28`, `FR-RI-149` s.d. `FR-RI-151` |
| `RWI-DEC-056` hubungan bayi dan ibu | `EPIC RI-33`, `FR-RI-152` |
| `RWI-DEC-057` versi resume pulang | `EPIC RI-27`, `FR-RI-153` |

**Penomoran.** Dokumen ini memakai rentang nomor **baru** — `EPIC RI-21` ke atas, `FR-RI-101` ke
atas — supaya tidak mendaur ulang nomor yang sudah dipakai PRD asli (`docs/Modul-RS/PRD-Modul-Rawat-Inap.md`)
untuk isi yang berbeda.

---

## 2. Ringkasan eksekutif

Rumah sakit hari ini **tidak dapat merawat inap satu pasien pun lewat sistem**. Tidak ada catatan
siapa menempati tempat tidur mana, tidak ada daftar pasien yang sedang dirawat, dan tidak ada cara
menutup perawatan selain mengubah data master secara manual.

MVP ini menyelesaikan satu hal: **membuat satu perjalanan rawat inap benar-benar bisa berjalan dari
awal sampai akhir tanpa ada petugas yang harus mengubah database.** Dari petugas admisi membuka
admisi, memesan tempat tidur, menempatkan pasien; perawat dan dokter berganti penanggung jawab;
pasien pindah kamar bila perlu; sampai DPJP menyatakan boleh pulang, resume ditandatangani, kasir
menyatakan lunas, episode ditutup, dan tempat tidur kembali kosong untuk pasien berikutnya.

Yang **belum** dikerjakan MVP ini: menulis pengkajian dan catatan dokter, membuat resep, menerima
pasien dari IGD, dan mengirim data ke SATUSEHAT. Keempatnya bukan karena tidak penting, melainkan
karena keputusannya belum turun dan sudah tercatat sebagai `DEC-INP-001`, `DEC-INP-002`,
`DEC-INP-003`, `DEC-INP-005`, `DEC-INP-006`, dan `DEC-INP-007`.

**Sejak `0.3.0`, MVP juga menolak penempatan yang tidak layak.** `DEC-INP-004` turun pada
2026-08-21: jenis kelamin dan kebutuhan isolasi tidak lagi sekadar menyaring hasil pencarian,
melainkan **menolak** penempatan dan perpindahan. Sistem tidak akan pernah menempatkan laki-laki di
kamar yang sedang dihuni perempuan, dan tidak akan pernah menempatkan pasien yang membutuhkan
isolasi di tempat tidur biasa — walaupun petugas memaksa.

---

## 3. Masalah produk

### 3.1 Yang sudah ada

| Yang sudah ada | Bukti kode |
| --- | --- |
| Master tempat tidur lengkap, termasuk penanda isolasi, jenis kelamin, boks bayi, dan dapat dipesan | `Areas/HealthServices/MasterData/Models/MstBed.cs:27-41` |
| Nilai `Reserved` dan `Occupied` sudah ada pada enum | `Areas/HealthServices/MasterData/Enums/BedStatus.cs:8` |
| Pencarian tempat tidur dengan 13 penyaring dan ringkasan jumlah | `Areas/HealthServices/MasterData/Controllers/BedController.cs:104-220` |
| Kunjungan sudah mengenal tipe rawat inap | `Areas/HealthServices/RegistrationManagement/Enums/EncounterType.cs:8` |
| Kelas pasien sudah punya penanda rawat inap dan tarif kamar harian | `Areas/HealthServices/MasterData/Models/MstPatientClass.cs:33,47` |
| Mesin hak akses yang mendaftarkan butir hak secara otomatis | `Seeders/AccessMenuSeeder.cs:22-60` |

### 3.2 Yang belum ada

| Yang belum ada | Bukti |
| --- | --- |
| Modul Rawat Inap | Tidak ada folder `Areas/HealthServices/InPatientManagement/`; tidak ada satu pun berkas berawalan `Inp` |
| Catatan penghunian tempat tidur | Dari 446 `DbSet`, tidak satu pun berkaitan dengan penempatan pasien |
| Mesin penggerak status tempat tidur | Satu-satunya penulis `MstBed.BedStatus` adalah CRUD master data |
| Daftar pasien dirawat | Tidak ada endpoint, view, maupun query census |
| Layar Rawat Inap di frontend | Tidak ada satu pun route; menu hanya mengenal Rawat Jalan dan IGD |
| Kemampuan transaksi Billing | `BillingManagement` hanya dua master dan satu service kosong |

### 3.3 Akibatnya hari ini

Petugas menempatkan Tn. Budi di tempat tidur `BD-RSMMC-00042`. Sistem tidak punya tempat untuk
menyimpan fakta itu. Yang bisa dilakukan hanya mengubah kolom status tempat tidur menjadi
`Occupied` lewat menu master data — tanpa jejak siapa yang menempati dan sejak kapan. Bila lupa
dikembalikan, kamar terlihat penuh selamanya padahal kosong.

---

## 4. Visi produk

Rantai keterhubungan data yang ingin dicapai, ditulis sebagai urutan:

1. Pasien terdaftar → **kunjungan** bertipe rawat inap dibuat atau dipakai.
2. Kunjungan → **episode rawat inap** dibuka, satu kunjungan tepat satu episode.
3. Episode → **DPJP** ditetapkan, berbentuk riwayat berperiode.
4. Episode → **pemesanan tempat tidur**, berlaku 2 jam.
5. Episode → **kebutuhan isolasi** direkam petugas admisi atau diputuskan DPJP.
6. Pemesanan → **Kelayakan Penempatan** diperiksa: tempat tidur, jenis kelamin, pencampuran kamar, dan isolasi → **penempatan tempat tidur**, dan episode menjadi aktif.
7. Penempatan → **census** menjawab siapa dirawat, di mana, oleh siapa, sudah berapa hari.
8. Penempatan → **perpindahan**, membentuk riwayat lokasi dan riwayat kelas.
9. Episode → **perawat penanggung jawab**, juga berbentuk riwayat.
10. Episode → **keputusan pulang** oleh DPJP → **resume pulang** ditandatangani.
11. Episode → **daftar periksa administrasi** dan **kelayakan keuangan**.
12. Episode → **penutupan**, dan tempat tidur kembali kosong.
13. Seluruh langkah di atas → **riwayat status** yang tidak dapat diubah.

---

## 5. Batas MVP

### 5.1 Titik mulai

1. Pasien sudah terdaftar pada modul Patient Management.
2. Master unit layanan, kamar, tempat tidur, dan kelas pasien sudah terisi lewat layar aplikasi, **beserta penanda jenis kelamin, isolasi, dan boks bayi pada tiap tempat tidur**. Sejak `0.3.0` penanda itu bukan lagi sekadar penyaring pencarian, melainkan penentu diterima atau ditolaknya penempatan, sehingga isian yang salah akan menolak penempatan yang sah.
3. Petugas admisi membuka layar admisi rawat inap.

### 5.2 Titik akhir

1. Episode berstatus `Closed`.
2. Tempat tidur yang tadinya ditempati terbaca `Available` pada pencarian berikutnya.
3. Riwayat status episode lengkap dan dapat ditelusuri urut.
4. Resume pulang tersimpan dan tertandatangani.
5. Episode muncul pada laporan pengecualian bila ditutup menembus gerbang keuangan.

---

## 6. Pelaku sasaran

| Pelaku | Tanggung jawabnya di dalam MVP |
| --- | --- |
| Petugas admisi | Membuka admisi, merekam kebutuhan isolasi sebagai catatan awal selagi episode `Draft`, memesan dan menempatkan tempat tidur, menandai butir administrasi, menutup episode |
| Perawat pelaksana | Melihat census, memindahkan pasien |
| Kepala ruangan | Menugaskan perawat penanggung jawab, mengalihkan DPJP, memindahkan pasien, menindaklanjuti daftar pantau |
| DPJP | Menetapkan dan memperbarui kebutuhan isolasi sebagai keputusan klinis, memindahkan pasien yang menjadi tanggung jawabnya, menyatakan pasien boleh pulang, menyusun dan menandatangani resume |
| Petugas kasir atau billing | Menandai kelayakan keuangan beserta catatannya |
| Supervisor | Membatalkan admisi setelah pasien dirawat, menutup menembus gerbang keuangan, membuka sesi koreksi |
| Admin master data | Mengisi master kamar dan tempat tidur, mengatur batas waktu dan butir administrasi |

---

## 7. Pemilihan kemampuan MVP

Uji yang dipakai untuk setiap kemampuan: *tanpa ini, apakah satu kasus nyata bisa selesai dari awal
sampai akhir?* dan *kalau tidak bisa, apakah ada jalan sementara yang aman dan tetap dapat diaudit?*

| Kemampuan | ID kemampuan asal | Keputusan MVP |
| --- | --- | --- |
| Memilih pasien terdaftar untuk dirawat | `RWI-CAP-001` | Wajib; tanpa ini tidak ada yang bisa dirawat |
| Menentukan penjamin saat masuk | `RWI-CAP-002` | Wajib; menjadi konteks kelayakan keuangan |
| Menentukan DPJP | `RWI-CAP-003` | Wajib; kewenangan pulang dan perpindahan bergantung padanya |
| Mencari tempat tidur tersedia | `RWI-CAP-004` | Wajib; tanpa ini petugas tidak tahu ke mana pasien ditempatkan |
| Pemesanan tempat tidur dan kedaluwarsanya | `RWI-CAP-006` | Wajib; tanpa ini dua petugas merebut tempat tidur yang sama |
| Penempatan pasien pada tempat tidur | `RWI-CAP-007` | Wajib; tanpa ini pasien tidak punya lokasi |
| Episode beserta model statusnya | `RWI-CAP-008` | Wajib; seluruh catatan lain menempel padanya |
| Census pasien dirawat | `RWI-CAP-012` | Wajib; tanpa ini perawat tidak tahu siapa saja yang dirawat |
| Perhitungan lama dirawat | `RWI-CAP-013` | Wajib; dipakai census dan resume |
| Penugasan perawat penanggung jawab | `RWI-CAP-014` | Wajib; daftar pantau butuh orang yang jelas untuk ditagih |
| Perpindahan dan pindah kelas | `RWI-CAP-017` | Wajib; kamar penuh dan perubahan kondisi adalah kejadian sehari-hari |
| Resume pulang | `RWI-CAP-025` | Wajib; syarat penutupan episode |
| Daftar periksa administrasi | `RWI-CAP-028` | Wajib; syarat penutupan episode |
| Kelayakan keuangan | `RWI-CAP-027` | Wajib; syarat penutupan episode. Sumbernya penandaan manual, lihat bagian 15 |
| Penutupan episode dan pelepasan tempat tidur | `RWI-CAP-029` | Wajib; tanpa ini tempat tidur tidak pernah kembali kosong |
| Pencatatan kepergian fisik pasien | `RWI-CAP-029` | Wajib; tanpa ini tempat tidur tertahan berjam-jam setelah pasien pulang. Ditambahkan pada `0.2.0` |
| Satu pasien satu episode aktif | `RWI-CAP-008` | Wajib; tanpa ini satu pasien bisa tercatat dirawat di dua tempat. Ditambahkan pada `0.2.0` |
| Riwayat versi resume pulang | `RWI-CAP-025` | Wajib; koreksi resume adalah amandemen rekam medis dan harus dapat ditelusuri. Ditambahkan pada `0.2.0` |
| Penanda bayi dirawat gabung dengan ibunya | `RWI-CAP-032` | Wajib; menyangkut kepastian identitas. Ditambahkan pada `0.2.0` |
| Sesi koreksi episode | `RWI-CAP-030` | Wajib; tanpa ini kesalahan cara pulang tidak dapat dibetulkan sama sekali |
| Riwayat status episode | `RWI-CAP-037` | Wajib; sumber data laporan pengecualian dan daftar pantau |
| Daftar pantau | `RWI-CAP-039` | Wajib; dua dari tiga daftar tersedia pada MVP |
| Pengaturan yang dapat diubah admin | `RWI-CAP-034` | Wajib; angka tidak boleh ditanam di kode |
| Boks bayi sebagai tempat tidur | `RWI-CAP-032` | Wajib; masternya sudah siap, tinggal dipakai |
| Hak akses per peran | `RWI-CAP-035` | Wajib; dipakai ulang dari mesin yang sudah ada |
| Kewenangan per pasien untuk DPJP | `RWI-CAP-036` | Wajib; `RWI-DEC-023` dan `RWI-DEC-024` menuntutnya |
| Penolakan penempatan karena jenis kelamin dan isolasi | `RWI-CAP-033` | Wajib; tanpa ini sistem ikut menyebabkan pelanggaran privasi dan pengendalian infeksi. **Berpindah dari daftar ditunda pada `0.3.0`** lewat `RWI-DEC-064` |
| Kebutuhan isolasi sebagai atribut episode | `RWI-CAP-033` | Wajib; aturan di atas tidak dapat dijalankan tanpa tempat menyimpan datanya. Ditambahkan pada `0.3.0` lewat `RWI-DEC-065` |

---

## 8. Kemampuan yang ditunda

Setiap baris menyebut **alasan bersebab** dan **pengganti selama MVP berjalan**.

> **Satu baris keluar dari daftar ini pada `0.3.0`.** "Penolakan penempatan karena isolasi atau
> jenis kelamin" sebelumnya ditunda karena `DEC-INP-004` belum turun. Keputusannya turun 2026-08-21
> lewat `RWI-DEC-064` sampai `RWI-DEC-066`, sehingga kemampuan itu **masuk MVP** sebagai
> `EPIC RI-34`. Daftar ini kini berisi sembilan baris, bukan sepuluh.

| Kemampuan | ID kemampuan asal | Alasan ditunda | Pengganti selama MVP |
| --- | --- | --- | --- |
| Pengkajian awal, catatan dokter, CPPT, tindakan, visite | `RWI-CAP-015`, `018`, `019`, `023`, `024` | Mesin klinis mewajibkan antrean dan konsultasi, dan hanya mengizinkan satu konsultasi per kunjungan. Pelonggaran menyentuh dua modul `ACTIVE` yang pemiliknya belum menyetujui — `DEC-INP-001` | Dokumentasi klinis tetap ditulis di luar sistem sebagaimana hari ini. Modul ini menyediakan riwayat lokasi, riwayat DPJP, dan resume, sehingga rekam medis tetap punya kerangka waktunya |
| Resep rawat inap dan obat pulang | `RWI-CAP-021`, `RWI-CAP-022` | Sama seperti di atas; resep terikat konsultasi | Butir "obat pulang sudah diserahkan" tersedia pada daftar periksa administrasi dan **ditandai manual** petugas admisi |
| Serah terima IGD ke rawat inap | `RWI-CAP-038` | Menentukan kunjungan mana yang menjadi jangkar episode; menyentuh modul IGD — `DEC-INP-002`, yang pemiliknya bernama sejak `RWI-DEC-069`: Rizki Gunawan | Petugas admisi membuka admisi rawat inap secara manual untuk pasien yang datang dari IGD, memakai jalur pasien datang langsung |
| Persetujuan umum rawat inap | `RWI-CAP-031` | Keputusan hukum dan privasi, pemiliknya belum ditunjuk — `DEC-INP-003` | Persetujuan tetap dikumpulkan di atas kertas seperti hari ini. Butir daftar periksa dapat ditambahkan admin bila diinginkan |
| Pengiriman SATUSEHAT | Belum punya ID kemampuan | Belum pernah dibahas; pemilik dan isi kiriman belum ditentukan — `DEC-INP-005` | Data disimpan dalam bentuk riwayat yang dapat dibaca ulang, sehingga pengiriman kelak tinggal membaca |
| Serah terima klinis antar shift | Belum punya ID kemampuan | Ditandai `SAFETY_CHECK` oleh baseline; belum pernah dibahas — `DEC-INP-006` | Serah terima tetap dilakukan lisan dan tertulis di luar sistem. Modul mencatat siapa perawat penanggung jawab dan sejak kapan |
| Cara pulang meninggal dan kabur | Bagian `RWI-CAP-026` | Sisi klinisnya masih terbuka — `DEC-INP-007` | Tiga cara pulang lain tersedia. Untuk dua kasus ini, episode ditutup lewat jalur supervisor disertai alasan, dan tercatat pada laporan pengecualian |
| Daftar pantau kepatuhan pengkajian dan CPPT | Bagian `RWI-CAP-039` | Bergantung pada dokumentasi klinis — `DEC-INP-001` | Dua daftar pantau lain tersedia |
| Deposit, estimasi biaya, tagihan berjalan, klaim | `RWI-CAP-027` sebagian | `BillingManagement` belum punya kemampuan transaksi | Kelayakan keuangan ditandai manual kasir. Data lama dirawat dan riwayat kelas tersimpan lengkap sehingga charge kamar dapat direkonstruksi kelak |

---

## 9. Alur bisnis target

`FLOW-RI-MVP-001` — Satu pasien dirawat inap dari masuk sampai pulang.

1. Petugas admisi memilih pasien yang sudah terdaftar.
2. Sistem membuat kunjungan bertipe rawat inap, atau memakai kunjungan poliklinik yang sudah ada.
3. Petugas memilih penjamin, kelas perawatan, unit layanan, dan DPJP. Episode dibuat berstatus `Draft`.
4. Bila surat rujukan menyebut kebutuhan isolasi, petugas admisi merekamnya sebagai catatan awal selagi episode masih `Draft`.
5. Petugas mencari tempat tidur kosong, lalu memesannya. Hasil pencarian sudah tersaring oleh kedelapan aturan Kelayakan Penempatan. Tempat tidur terbaca `Reserved` selama 2 jam.
6. Pasien sampai di kamar. Petugas menekan konfirmasi masuk. Kelayakan Penempatan diperiksa **ulang** di sini — jenis kelamin, pencampuran kamar, dan isolasi termasuk di dalamnya. Bila salah satu gagal, penempatan ditolak dan isian admisi tetap utuh. Bila lolos, episode menjadi `Admitted`, tempat tidur `Occupied`, dan pasien muncul pada census.
7. Kepala ruangan menugaskan perawat penanggung jawab.
8. Bila DPJP kemudian mengubah kebutuhan isolasi, perubahannya diterima seketika. Bila tempat tidur yang sedang ditempati jadi tidak sesuai, episode muncul pada daftar pantau penempatan tidak sesuai sampai pasien dipindahkan.
9. Bila kamar perlu berganti, kepala ruangan, perawat, supervisor, atau DPJP memindahkan pasien. Kelayakan Penempatan diperiksa dengan aturan yang sama persis seperti penempatan awal. Penempatan lama ditutup dan yang baru dibuka dalam satu tindakan utuh.
10. Bila DPJP berhalangan, kepala ruangan atau supervisor mengalihkan tanggung jawab DPJP disertai alasan.
11. DPJP menyatakan pasien boleh pulang dan memilih cara pulangnya. Episode menjadi `DischargePending`. Tempat tidur **belum** dilepas.
12. DPJP menyusun resume pulang lalu menandatanganinya.
13. Petugas admisi menandai butir daftar periksa administrasi.
14. Petugas kasir menandai kelayakan keuangan `Cleared` disertai catatan.
15. Keluarga menjemput dan pasien meninggalkan kamar. Petugas ruangan mencatat kepergiannya. Tempat tidur **langsung bebas** dan boleh dipesan pasien berikutnya, walaupun episodenya belum ditutup.
16. Petugas admisi menutup episode. Episode menjadi `Closed`.
17. Bila kelayakan keuangan belum `Cleared` sementara pasien harus segera pulang, supervisor menutup episode disertai alasan. Episode ditandai dan masuk laporan pengecualian.
18. Bila kemudian ditemukan kesalahan catatan, supervisor membuka sesi koreksi, membetulkan, lalu menutup sesinya. Status episode tetap `Closed` sepanjang sesi, dan versi resume sebelumnya tersimpan.

---

## 10. Epic dan functional requirement

### `EPIC RI-21` — Fondasi episode dan data master

**Tujuan:** menyediakan tabel, master, dan mesin status sehingga episode dapat hidup.
**Disposisi backend:** `MISSING / NEW`

> **`FR-RI-101` — Episode menempel pada tepat satu kunjungan**
> Sistem menolak pembuatan episode kedua pada kunjungan yang sudah punya episode.
> **Contoh:** kunjungan `ENC-2026-09-000456` sudah punya episode `RI-2026-09-000123`. Percobaan
> membuka admisi kedua pada kunjungan itu ditolak dengan pesan "Kunjungan ini sudah punya episode
> rawat inap" dan kode 409.

> **`FR-RI-102` — Model status episode terkunci lima nilai**
> Hanya `Draft`, `Admitted`, `DischargePending`, `Closed`, dan `Cancelled` yang diterima.
> **Contoh:** permintaan yang mengirim nilai `InCare` ditolak. Tidak ada endpoint yang menerima
> status bebas.

> **`FR-RI-103` — Setiap perpindahan status meninggalkan tepat satu baris riwayat**
> **Contoh:** episode yang berjalan `Draft` → `Admitted` → `DischargePending` → `Closed`
> meninggalkan empat baris riwayat bernomor urut 1 sampai 4, masing-masing dengan pelaku, waktu,
> dan alasan.

> **`FR-RI-104` — Data master awal tersedia**
> Satu baris pengaturan berkode `DEFAULT` dan tiga butir daftar periksa administrasi.
> **Contoh:** `BedReservationMinutes` bernilai 120, dan butir `ADM-DOC`, `RETURN-ITEM`,
> `DISCHARGE-MED` ada, dengan `DISCHARGE-MED` bertanda tidak wajib.

### `EPIC RI-22` — Pencarian dan pemesanan tempat tidur

**Tujuan:** petugas dapat menemukan tempat tidur kosong dan menguncinya sementara.
**Disposisi backend:** `EXTEND` — master tempat tidur sudah ada, pemesanan belum

> **`FR-RI-105` — Pencarian menyembunyikan tempat tidur yang sedang dipesan**
> **Contoh:** `BD-RSMMC-00042` dipesan pukul 09:15. Pencarian pukul 09:20 tidak memuatnya.

> **`FR-RI-106` — Pemesanan gugur sendiri setelah batas waktu, dihitung saat dibaca**
> **Contoh:** pemesanan pukul 09:15 dengan batas 120 menit. Pembacaan pukul 11:14 masih mengunci;
> pembacaan pukul 11:16 sudah `Available`. Tidak ada proses latar belakang yang dijalankan.

> **`FR-RI-107` — Batas waktu dapat diubah admin dan langsung berlaku**
> **Contoh:** admin mengubah 120 menjadi 180 menit pukul 14:00. Pemesanan pukul 14:05 berlaku
> sampai 17:05. Pemesanan yang dibuat pukul 13:30 tetap memakai batas lama.

> **`FR-RI-108` — Satu tempat tidur hanya boleh punya satu pemesanan aktif**
> **Contoh:** episode A memesan `BD-RSMMC-00042`. Episode B memesan tempat tidur yang sama dan
> ditolak dengan kode 409.

### `EPIC RI-23` — Penempatan pasien dan pengaktifan episode

**Tujuan:** pasien punya lokasi yang tercatat, dan tempat tidur ganda mustahil terjadi.
**Disposisi backend:** `MISSING / NEW`

> **`FR-RI-109` — Pencegahan tempat tidur ganda**
> Sistem menolak penempatan pasien ke tempat tidur yang sedang ditempati.
> **Contoh:** Sdri. Wati menempatkan Tn. Budi ke `BD-RSMMC-00042` pukul 09.00.01. Pada saat hampir
> bersamaan Sdri. Rina menempatkan Ny. Sari ke tempat tidur yang sama. Permintaan Sdri. Rina
> ditolak dengan pesan "Tempat tidur BD-RSMMC-00042 sudah ditempati pasien lain" dan kode 409.
> Tidak ada satu pun data penempatan ganda yang tersimpan.

> **`FR-RI-110` — Penempatan menutup pemesanan dan mengubah salinan status tempat tidur dalam satu transaksi**
> **Contoh:** bila penulisan salinan status gagal, catatan penempatan juga tidak tersimpan, dan
> episode tetap `Draft`.

> **`FR-RI-111` — Pemesanan yang gugur tidak menghalangi penempatan bila tempat tidur masih kosong**
> **Contoh:** pemesanan Ny. Sari gugur pukul 11:15. Ny. Sari sampai pukul 11:40. Karena tempat
> tidur masih kosong, penempatan tetap berhasil tanpa peringatan apa pun.

> **`FR-RI-112` — Penolakan penempatan tidak menghapus isian admisi**
> **Contoh:** penempatan ditolak karena tempat tidur diambil pasien lain. Episode tetap `Draft`,
> dan penjamin, DPJP, serta kelas yang sudah diisi tetap tersimpan.

> **`FR-RI-148` — Satu pasien satu episode yang benar-benar hadir**
> Sistem menolak menempatkan pasien yang sudah punya episode berjalan.
> **Contoh:** Tn. Budi sedang dirawat di Melati 3B. Pukul 14:00 petugas lain mencoba
> menempatkannya di Anggrek 1A karena mengira ia pasien baru. Ditolak dengan pesan "Tn. Budi sudah
> dirawat pada episode RI-2026-09-000123 di Melati 3B" dan kode 409, sehingga petugas langsung tahu
> bahwa yang dibutuhkan adalah perpindahan.
>
> Sebaliknya, bila Tn. Budi sudah pulang pukul 10:15 dan kepergiannya sudah dicatat, lalu ia
> kembali pukul 12:00 dengan keluhan baru, admisi barunya **diterima** walaupun episode lama belum
> ditutup.

### `EPIC RI-24` — Census dan lama dirawat

**Tujuan:** perawat dan admisi tahu siapa dirawat, di mana, dan sudah berapa hari.
**Disposisi backend:** `MISSING / NEW`

> **`FR-RI-113` — Census menampilkan pasien `Admitted` dan `DischargePending` saja**
> **Contoh:** dari lima episode berstatus berbeda, census memuat tepat dua.

> **`FR-RI-114` — Lama dirawat dihitung dari selisih tanggal dengan hasil paling sedikit 1 hari**
> **Contoh:** masuk 21 September pukul 22:30, pulang 22 September pukul 06:00. Selisih jamnya 7,5
> jam, tetapi lama dirawat tercatat **1 hari**, bukan 0 hari.

> **`FR-RI-115` — Lama dirawat bertambah pada pergantian tanggal**
> **Contoh:** pasien masuk 21 September pukul 22:30. Pada 22 September pukul 00:30 lama dirawat
> sudah bernilai 1, bukan menunggu sampai 22 September pukul 22:30.

### `EPIC RI-25` — Penanggung jawab episode

**Tujuan:** sistem dapat menjawab siapa DPJP dan siapa perawat pada tanggal tertentu.
**Disposisi backend:** `MISSING / NEW`

> **`FR-RI-116` — DPJP berbentuk riwayat berperiode, bukan satu kolom yang ditimpa**
> **Contoh:** dr. Andi menjadi DPJP 21–23 September, dr. Rina 23–25 September. Pada 25 September
> sistem masih dapat menjawab bahwa perpindahan 22 September diminta dr. Andi selagi ia berwenang.

> **`FR-RI-117` — Satu episode aktif punya tepat satu DPJP aktif**
> **Contoh:** percobaan membuat penugasan DPJP kedua tanpa menutup yang pertama ditolak.

> **`FR-RI-118` — Pengalihan DPJP wajib beralasan**
> **Contoh:** pengalihan tanpa alasan ditolak dengan kode 400.

> **`FR-RI-119` — Episode boleh berjalan tanpa perawat penanggung jawab**
> **Contoh:** antara pukul 10:40 dan 11:00 episode Tn. Budi belum punya perawat. Selama 20 menit
> itu tidak ada satu pun tindakan yang tertahan, dan episode muncul pada daftar pantau kepala
> ruangan.

### `EPIC RI-26` — Perpindahan pasien dan pindah kelas

**Tujuan:** pasien dapat berpindah tempat tidur tanpa episode terputus, dan kelas tagihan mengikuti.
**Disposisi backend:** `MISSING / NEW`

> **`FR-RI-120` — Perpindahan bersifat utuh**
> **Contoh:** bila pembukaan penempatan baru gagal, penempatan lama **tidak** jadi ditutup. Tn.
> Budi tetap tercatat di `BD-RSMMC-00042`. Tidak pernah ada saat pasien tercatat tanpa tempat tidur.

> **`FR-RI-121` — Kelas yang ditagihkan mengikuti kamar yang ditempati**
> **Contoh:** Tn. Budi pindah dari Melati 3B kelas 2 ke Anggrek 1A kelas 1 pada 23 September pukul
> 09:30. Riwayat penempatan menunjukkan 2 hari kelas 2 dan 2 hari kelas 1.

> **`FR-RI-122` — Dokter yang bukan DPJP aktif tidak dapat memindahkan pasien**
> **Contoh:** dr. Rina, dokter jaga, mencoba memindahkan Tn. Budi yang DPJP-nya dr. Andi.
> Permintaan ditolak dengan kode 403 dan pesan "Hanya DPJP episode ini yang dapat memindahkan
> pasien." Tidak ada kolom keterangan yang dapat dipakai melewatinya.

> **`FR-RI-123` — Perpindahan wajib beralasan medis**
> **Contoh:** perpindahan tanpa alasan ditolak dengan kode 400.

### `EPIC RI-27` — Keputusan pulang dan resume

**Tujuan:** keputusan pulang tercatat, dan setiap episode meninggalkan ringkasan resmi.
**Disposisi backend:** `MISSING / NEW`

> **`FR-RI-124` — Hanya DPJP aktif yang menyatakan pasien boleh pulang**
> **Contoh:** dr. Rina yang bukan DPJP ditolak dengan kode 403.

> **`FR-RI-125` — Satu episode punya paling banyak satu resume pulang**
> **Contoh:** percobaan membuat resume kedua ditolak dengan kode 409.

> **`FR-RI-126` — Resume mengisi DPJP beserta periodenya secara otomatis**
> **Contoh:** resume Tn. Budi menampilkan "dr. Andi, 21–23 Sept; dr. Rina, 23–25 Sept" tanpa
> diketik ulang.

> **`FR-RI-127` — Isi wajib resume menyesuaikan cara pulang**
> **Contoh:** cara pulang `Referred` sementara tujuan rujukan kosong ditolak dengan kode 400.

> **`FR-RI-128` — Resume terkunci setelah episode ditutup**
> **Contoh:** percobaan mengubah resume episode `Closed` tanpa sesi koreksi ditolak dengan kode 409.

> **`FR-RI-153` — Perubahan resume yang sudah ditandatangani menyimpan versi lamanya**
> **Contoh:** resume Ibu Sari ditandatangani dr. Andi 15 Agustus dengan cara pulang "kabur". Pada
> 17 Agustus supervisor membuka sesi koreksi dan mengubahnya menjadi "atas permintaan sendiri".
> Sistem menyimpan salinan versi 15 Agustus lengkap dengan isi dan nama penandatangan lamanya.
> Menyunting resume yang **belum** ditandatangani tidak membuat versi apa pun.

### `EPIC RI-28` — Daftar periksa, kelayakan keuangan, dan penutupan

**Tujuan:** episode hanya dapat ditutup bila kelima syaratnya benar-benar terpenuhi.
**Disposisi backend:** `MISSING / NEW`

> **`FR-RI-129` — Kelima syarat penutupan diperiksa dan dilaporkan satu per satu**
> **Contoh:** permintaan tutup pukul 10:00 ditolak 422 dengan daftar: resume belum ditandatangani,
> kelayakan keuangan masih `Pending`. Tiga syarat lain sudah terpenuhi dan ikut ditampilkan.

> **`FR-RI-130` — Hanya kasir atau billing yang menandai kelayakan keuangan, dan catatan wajib**
> **Contoh:** petugas admisi menandai `Cleared` dan ditolak 403. Kasir menandai tanpa catatan dan
> ditolak 400.

> **`FR-RI-131` — Jalan keluar supervisor hanya menembus syarat keuangan**
> **Contoh:** supervisor menutup episode sementara resume belum ditandatangani, dan tetap ditolak
> 422. Keempat syarat lain tidak dapat dilewati siapa pun.

> **`FR-RI-132` — Penutupan melepas tempat tidur dalam satu tindakan**
> **Contoh:** episode `Closed` pukul 13:10, dan `BD-RSMMC-00105` muncul pada pencarian tempat tidur
> kosong pukul 13:11.

> **`FR-RI-133` — Butir daftar periksa yang dinonaktifkan tidak lagi menahan penutupan**
> **Contoh:** admin menonaktifkan butir `DISCHARGE-MED`. Episode berikutnya dapat ditutup tanpa
> menandai butir itu, dan penandaan lama tetap tersimpan.

> **`FR-RI-149` — Kepergian fisik pasien melepas tempat tidur seketika**
> **Contoh:** Tn. Budi diputuskan boleh pulang pukul 09:20. Keluarga menjemput dan ia meninggalkan
> kamar pukul 10:15; perawat mencatatnya. Bed `BD-RSMMC-00105` langsung tersedia, dan pukul 10:40
> sudah dipesankan untuk Ny. Sari. Episode Tn. Budi baru ditutup pukul 13:10. Tanpa aturan ini,
> tempat tidur itu baru bebas pukul 13:10 — selisih dua setengah jam pada satu tempat tidur saja.

> **`FR-RI-150` — Kepergian fisik bukan penutupan**
> **Contoh:** setelah kepergian dicatat, episode Tn. Budi tetap berstatus rencana pulang, tetap
> wajib ditutup, dan tetap muncul pada daftar pantau penutupan tertunda. Yang berubah hanya tempat
> tidurnya.

> **`FR-RI-151` — Pasien yang sudah pergi tidak dapat dipindahkan dan tidak muncul di census**
> **Contoh:** percobaan memindahkan Tn. Budi pukul 11:00 ditolak dengan pesan "Pasien sudah tercatat
> meninggalkan ruangan". Census pukul 11:00 juga tidak lagi memuat namanya.

### `EPIC RI-29` — Riwayat status dan daftar pantau

**Tujuan:** setiap perubahan dapat ditelusuri, dan keterlambatan terlihat tanpa menghalangi kerja.
**Disposisi backend:** `MISSING / NEW`

> **`FR-RI-134` — Baris riwayat tidak dapat diubah dan tidak dapat dihapus**
> **Contoh:** tidak ada endpoint update maupun delete untuk riwayat status. Percobaan memanggilnya
> menghasilkan 404.

> **`FR-RI-135` — Perubahan yang dihitung sistem ditandai sebagai dilakukan sistem**
> **Contoh:** pemesanan yang gugur meninggalkan baris riwayat dengan penanda sistem dan tanpa nama
> orang, sehingga audit tidak salah menuduh siapa pun.

> **`FR-RI-136` — Tiga daftar pantau tersedia dan tidak menahan tindakan apa pun**
> **Contoh:** episode `DischargePending` yang belum ditutup lebih dari 4 jam muncul pada daftar
> pantau petugas admisi, tetapi penutupan tetap dapat dijalankan kapan saja.

> **`FR-RI-137` — Laporan selisih tempat tidur tersedia**
> **Contoh:** tempat tidur tertulis `Available` padahal masih ada penempatan aktif atas nama Tn.
> Budi muncul sebagai satu baris laporan lengkap dengan nama pasien dan waktu mulai.

### `EPIC RI-30` — Sesi koreksi episode

**Tujuan:** kesalahan catatan dapat dibetulkan tanpa mengganggu tempat tidur dan tanpa menambah lama dirawat.
**Disposisi backend:** `MISSING / NEW`

> **`FR-RI-138` — Sesi koreksi tidak mengubah status episode**
> **Contoh:** episode Ibu Sari ditutup 15 Agustus. Pada 17 Agustus supervisor membuka sesi koreksi
> dan mengubah cara pulang. Sepanjang 17 Agustus status episode tetap `Closed`, `MELATI-03` tetap
> ditempati pasien lain tanpa terganggu, dan lama dirawat tetap 3 hari.

> **`FR-RI-139` — Hanya supervisor, dan alasan wajib**
> **Contoh:** kepala ruangan membuka sesi koreksi dan ditolak 403. Supervisor tanpa alasan ditolak
> 400.

> **`FR-RI-140` — Menutup sesi wajib menyertakan daftar perubahan**
> **Contoh:** menutup sesi tanpa mengisi apa saja yang berubah ditolak 400. Ini satu-satunya jejak
> koreksi, karena status episode tidak berubah sehingga riwayat status tidak mencatat apa pun.

### `EPIC RI-31` — Pengaturan yang dapat diubah admin

**Tujuan:** seluruh angka berada di satu tempat dan tidak tertanam di kode.
**Disposisi backend:** `MISSING / NEW`

> **`FR-RI-141` — Lima angka dapat diubah dari satu layar dan berlaku pada pembacaan berikutnya**
> **Contoh:** admin mengubah ambang penutupan tertunda dari 4 jam menjadi 6 jam. Daftar pantau
> berikutnya memakai 6 jam tanpa aplikasi dinyalakan ulang.

> **`FR-RI-142` — Modul tetap berjalan bila pengaturan belum terisi**
> **Contoh:** pada lingkungan baru tanpa baris pengaturan, sistem memakai nilai bawaan dan mencatat
> peringatan, bukan gagal.

### `EPIC RI-32` — Perbaikan tempat tidur dan pembatasan wewenang status

**Tujuan:** admin tetap dapat menutup tempat tidur yang rusak, dan status penghunian hanya lahir dari Rawat Inap.
**Disposisi backend:** `EXTEND` — menyentuh `BedController` dan slice Redux frontend yang sudah ada

> **`FR-RI-143` — Tombol aktifkan dan nonaktifkan tempat tidur berfungsi**
> **Contoh:** hari ini tombol memanggil endpoint yang tidak ada dan selalu gagal 404. Setelah
> diperbaiki, menonaktifkan `BD-RSMMC-00042` berhasil dan tempat tidur itu hilang dari pencarian.

> **`FR-RI-144` — Menyetel status terisi atau dipesan lewat menu master data ditolak**
> **Contoh:** admin mencoba menyetel `BD-RSMMC-00042` menjadi `Occupied` dan ditolak 422 dengan
> pesan yang mengarahkan ke modul Rawat Inap.

> **`FR-RI-145` — Wewenang admin atas keadaan non-pasien tidak berkurang**
> **Contoh:** menyetel `Maintenance` tetap berhasil.

### `EPIC RI-33` — Bayi baru lahir dan boks bayi

**Tujuan:** bayi mendapat episode sendiri, dan boks bayi diperlakukan sebagai tempat tidur.
**Disposisi backend:** `EXISTING / REUSE` untuk masternya; bergantung pada `EPIC RI-23`

> **`FR-RI-146` — Boks bayi diperlakukan sebagai tempat tidur biasa**
> **Contoh:** boks `BOX-MELATI-03-A` didaftarkan sebagai tempat tidur bertanda `IsForNewborn` di
> kamar Melati 3. Bayi Ny. Sari mendapat episode dan kunjungan sendiri, lalu ditempatkan di boks
> itu. Census menampilkan dua baris: Ny. Sari dan bayinya.

> **`FR-RI-147` — Episode ibu dan bayi sepenuhnya terpisah**
> **Contoh:** menutup episode Ny. Sari tidak menutup episode bayinya, dan tidak melepas boks bayi.

> **`FR-RI-152` — Episode bayi menyimpan penanda rawat gabung dengan ibunya**
> **Contoh:** perawat membuka detail boks `BOX-MELATI-03-A` dan sistem menjawab bahwa penghuninya
> adalah bayi Ny. Sari yang dirawat di Melati 3. Tanpa penanda ini, satu-satunya petunjuk hanyalah
> kesamaan kamar. Penanda ini boleh kosong untuk episode yang bukan bayi rawat gabung, dan
> **tidak boleh** menunjuk episode milik pasien yang sama.

### `EPIC RI-34` — Kelayakan penempatan menurut jenis kelamin dan isolasi

**Tujuan:** sistem tidak pernah menempatkan pasien pada tempat tidur atau kamar yang secara privasi
atau pengendalian infeksi tidak layak baginya, walaupun petugas memaksa.
**Disposisi backend:** `MISSING / NEW`; menempel pada `EPIC RI-23` penempatan dan `EPIC RI-26`
perpindahan
**Dasar keputusan:** `RWI-DEC-064`, `RWI-DEC-065`, `RWI-DEC-066`, dirinci pada `RWI-RULE-012`

> **Kenapa epic ini baru lahir pada `0.3.0`.** Sampai `0.2.0`, penanda jenis kelamin dan isolasi
> pada master tempat tidur hanya **menyaring hasil pencarian**: petugas tetap dapat menempatkan
> pasien di mana pun ia mau. Pemilik berwenang mengubah arahnya menjadi aturan yang **menolak**.
> Sisi teknisnya murah — bentuk daftar aturan Kelayakan Penempatan memang dirancang sejak awal
> untuk ditambahi — tetapi dampaknya besar, sehingga diberi epic sendiri agar dapat diuji terpisah.

#### Bagian A — Pemisahan jenis kelamin

> **`FR-RI-154` — Penempatan ditolak bila penanda tempat tidur tidak menerima jenis kelamin pasien**
> **Contoh:** `MELATI-03-A` bertanda hanya menerima perempuan. Petugas mencoba menempatkan Tn. Budi
> di sana. Ditolak dengan pesan "Tempat tidur ini hanya untuk pasien perempuan" dan kode 422.
> Sebelum `0.3.0`, tempat tidur itu sekadar tidak muncul pada hasil pencarian, sementara penempatan
> paksa tetap berhasil.

> **`FR-RI-155` — Kamar tidak boleh ditempati campur laki-laki dan perempuan**
> Pemeriksaannya membaca **penghuni yang sedang ada**, bukan penanda pada master kamar. Tidak ada
> kolom "boleh campur" pada `MstRoom`, dan `RWI-DEC-066` menolaknya secara tegas.
> **Contoh:** Kamar Melati 3 berisi tiga tempat tidur. Pukul 08:00 Ny. Sari menempati `MELATI-03-A`.
> Pukul 10:00 petugas mencoba menempatkan Tn. Budi di `MELATI-03-B`. Ditolak dengan pesan "Kamar
> Melati 3 sedang dihuni pasien perempuan" dan kode 422. Pukul 10:30 Ny. Rina ditempatkan di
> `MELATI-03-B` dan **diterima**.
>
> Kamar berisi satu tempat tidur tidak pernah tersentuh aturan ini, karena tidak mungkin ada
> penghuni lain.

> **`FR-RI-156` — Boks bayi dikecualikan dari kedua sisi pemeriksaan**
> **Contoh:** bayi Ny. Sari berjenis kelamin laki-laki dan menempati `BOX-MELATI-03-A` di kamar
> ibunya. Penempatan bayi itu **berhasil** walaupun kamarnya sedang dihuni perempuan. Sebaliknya,
> ketika Ny. Rina hendak masuk ke `MELATI-03-B`, bayi laki-laki itu **tidak dihitung** sebagai
> penghuni, sehingga penempatan Ny. Rina tetap diterima.
>
> **Kenapa dua arah.** Bayi tidak boleh menutup kamar bagi pasien lain, dan bayi juga tidak boleh
> ditolak hanya karena jenis kelamin ibunya berbeda.

> **`FR-RI-157` — Pasien yang jenis kelaminnya belum tercatat hanya boleh masuk kamar kosong**
> **Contoh:** pasien tidak dikenal dari kecelakaan, jenis kelaminnya belum terisi. Ia hanya dapat
> ditempatkan pada tempat tidur yang menerima laki-laki dan perempuan sekaligus, **dan** hanya ke
> kamar yang belum ada penghuninya. Penempatan ke kamar berpenghuni ditolak dengan kode 422.

#### Bagian B — Kebutuhan isolasi

> **`FR-RI-158` — Kebutuhan isolasi adalah atribut episode, bukan atribut pasien**
> Melekat pada satu masa perawatan, bernilai tidak secara bawaan, dan tersimpan bersama siapa serta
> kapan terakhir menetapkannya.
> **Contoh:** Tn. Budi butuh isolasi pada episode September karena suspek penyakit menular. Ketika
> ia dirawat lagi pada Desember untuk patah tulang, episode barunya bernilai tidak — tanpa ada
> petugas yang perlu mematikannya.

> **`FR-RI-159` — Petugas admisi merekam catatan awal, DPJP mengambil keputusan klinis**
> Selagi episode `Draft`, petugas admisi boleh merekam nilainya berdasarkan surat atau keterangan
> dokter pengirim, dan hasilnya ditandai **catatan awal**. Setelah episode aktif, hanya **DPJP
> aktif** yang boleh mengubahnya, dan hasilnya ditandai **keputusan klinis**.
> **Contoh:** pukul 09:15 petugas admisi merekam "membutuhkan isolasi" untuk Tn. Budi berdasarkan
> surat rujukan puskesmas, tertandai catatan awal. Hari kedua dr. Andi selaku DPJP mengubahnya
> menjadi tidak, tertandai keputusan klinis atas namanya. Percobaan dr. Rina yang bukan DPJP
> mengubah nilai yang sama ditolak dengan kode 403.
>
> **Kenapa dibedakan, bukan disamakan.** Penempatan tidak boleh menunggu pengkajian klinis yang
> slice-nya masih di luar MVP — `DEC-INP-001`. Tetapi merekam keterangan orang lain berbeda dari
> memutuskan secara klinis, dan rekam medis harus dapat menunjukkan bedanya.

> **`FR-RI-160` — Tempat tidur isolasi dijaga dari dua arah**
> Pasien yang membutuhkan isolasi **hanya** boleh ke tempat tidur isolasi, dan pasien yang tidak
> membutuhkannya **tidak boleh** menempati tempat tidur isolasi.
> **Contoh:** percobaan menempatkan Tn. Budi yang butuh isolasi di `BD-RSMMC-00042` yang bukan
> isolasi ditolak dengan pesan "Pasien ini membutuhkan isolasi, sehingga hanya dapat ditempatkan
> pada tempat tidur isolasi". Sebaliknya, menempatkan pasien biasa di tempat tidur isolasi ditolak
> dengan pesan "Tempat tidur isolasi hanya untuk pasien yang membutuhkan isolasi", supaya kapasitas
> isolasi tidak habis terpakai sia-sia.

> **`FR-RI-161` — Perubahan kebutuhan isolasi tidak pernah ditahan; yang muncul adalah daftar pantau**
> Bila kebutuhan isolasi berubah menjadi ya sementara pasien sudah berbaring di tempat tidur biasa,
> perubahan itu **tetap diterima**. Episode itu muncul pada daftar pantau **penempatan tidak
> sesuai** sampai pasien dipindahkan.
> **Contoh:** pukul 14:00 dr. Andi menyatakan Tn. Budi di `MELATI-03-B` membutuhkan isolasi.
> Pencatatannya diterima seketika. Episode Tn. Budi muncul pada daftar pantau, dan hilang dari sana
> begitu ia dipindahkan ke tempat tidur isolasi pukul 15:20.
>
> **Kenapa tidak ditahan.** Menahan pencatatan klinis demi menjaga aturan penempatan adalah urutan
> terbalik. Yang benar: fakta klinis dicatat lebih dulu, lalu sistem menunjukkan bahwa
> penempatannya perlu dibetulkan.

> **`FR-RI-162` — Aturan yang sama berlaku pada perpindahan, bukan hanya penempatan**
> Kedelapan aturan Kelayakan Penempatan dipanggil dari dua tindakan: menempatkan dan memindahkan.
> **Contoh:** memindahkan Tn. Budi ke `ANGGREK-01-B` di kamar yang sedang dihuni pasien perempuan
> ditolak dengan alasan dan kode yang sama persis seperti penempatan awal.

---

## 11. Model status yang diusulkan

| Objek | Status | Invariant utama |
| --- | --- | --- |
| Episode | `Draft`, `Admitted`, `DischargePending`, `Closed`, `Cancelled` | `Admitted` wajib punya tepat satu penempatan aktif. `DischargePending` wajib punya satu **sampai kepergian pasien dicatat**, setelah itu nol |
| Kehadiran pasien | Bukan status yang disimpan; diturunkan dari status episode dan waktu kepergian | Satu pasien paling banyak satu episode yang benar-benar hadir |
| Kebutuhan isolasi | Bukan status berperiode; satu penanda pada episode beserta asalnya — catatan awal admisi atau keputusan klinis DPJP | Yang tersimpan hanya **nilai yang berlaku sekarang**, bukan riwayat. Selagi `Draft` boleh disetel petugas admisi; setelah aktif hanya DPJP aktif |
| Pemesanan tempat tidur | `Active`, `Consumed`, `Expired`, `Cancelled` | Satu tempat tidur paling banyak satu pemesanan aktif |
| Penempatan tempat tidur | `Aktif`, `Berakhir` | Satu tempat tidur paling banyak satu penempatan aktif |
| Kelayakan keuangan | `Pending`, `Cleared`, `Blocked` | Hanya `Cleared` yang membuka penutupan |
| Resume pulang | Belum ditandatangani, Tertandatangani | Satu episode paling banyak satu resume **yang berlaku**; versi sebelumnya tersimpan sebagai salinan |
| Sesi koreksi | `Terbuka`, `Tertutup` | Satu episode paling banyak satu sesi terbuka |

Rincian lengkap beserta perpindahan yang **tidak sah** ada pada
[`contracts/state-transition-matrix.md`](./contracts/state-transition-matrix.md).

---

## 12. Sasaran arsitektur

| Kelompok | Isinya |
| --- | --- |
| **Dipakai ulang apa adanya** | Pasien, kunjungan, penjamin, tempat tidur, kamar, unit layanan, kelas pasien, dokter, pegawai, mesin hak akses, pola `ApiResponse`, pola seeder |
| **Diperluas** | Perilaku `BedController.UpdateBedAvailability`; slice Redux tempat tidur di frontend |
| **Baru** | Sebelas tabel transaksi berawalan `Inp`, dua master `MstInpatient*`, enam service, lima controller modul, dua controller master |

Tidak satu pun tabel milik modul lain berubah bentuknya. **Tiga belas** tabel baru, nol perubahan
kolom pada tabel existing.

**`0.3.0` tidak menambah satu tabel pun.** Kebutuhan isolasi masuk sebagai enam kolom pada
`InpEpisode` beserta satu enum `InpIsolationSource`, dan aturan pencampuran kamar dijalankan dengan
membaca penghuni yang sedang ada. `RWI-DEC-066` menolak menambah kolom "boleh campur" pada
`MstRoom`, sehingga janji "nol perubahan kolom pada tabel modul lain" tetap utuh.

Rincian lengkap ada pada [`02-backend-architecture.md`](./02-backend-architecture.md).

---

## 13. Sasaran kemampuan API

Seluruh endpoint di bawah adalah bagian dari
[`contracts/api-contract.md`](./contracts/api-contract.md) dan tidak melebihinya.

### Health Services / Inpatient Management / Inpatient Episode

Base URL: `api/v1/health-services/inpatient-management/episodes`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Membuka admisi | `InpatientEpisode : Create` | `OpenAdmissionRequest` | `ApiResponse<InpatientEpisodeDetailResponse>` | `EPIC RI-21` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/cancel` | Membatalkan admisi | `InpatientEpisode : Update` | `CancelAdmissionRequest` | `ApiResponse<InpatientEpisodeDetailResponse>` | `EPIC RI-21` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/doctor-assignments` | Mengalihkan DPJP | `InpatientEpisode : Update` | `HandoverDoctorRequest` | `ApiResponse<InpatientDoctorAssignmentResponse>` | `EPIC RI-25` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/nurse-assignments` | Menugaskan perawat | `InpatientEpisode : Update` | `AssignNurseRequest` | `ApiResponse<InpatientNurseAssignmentResponse>` | `EPIC RI-25` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/status-history` | Riwayat status | `InpatientEpisode : Read` | – | `ApiResponse<List<InpatientStatusHistoryResponse>>` | `EPIC RI-29` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/correction-sessions` | Membuka sesi koreksi | `InpatientEpisode : Reopen` | `OpenCorrectionSessionRequest` | `ApiResponse<InpatientCorrectionSessionResponse>` | `EPIC RI-30` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/isolation-requirement` | Menetapkan atau mengubah kebutuhan isolasi | `InpatientEpisode : SetIsolation` | `SetIsolationRequirementRequest` | `ApiResponse<InpatientEpisodeDetailResponse>` | `EPIC RI-34` | **Rencana (belum tersedia)** |

### Health Services / Inpatient Management / Bed Occupancy

Base URL: `api/v1/health-services/inpatient-management/bed-occupancies`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/available-beds` | Mencari tempat tidur yang benar-benar dapat ditempati | `InpatientBedOccupancy : Read` | Query | `ApiResponse<AvailableBedPagedResult>` | `EPIC RI-22` | **Rencana (belum tersedia)** |
| `POST` | `/reservations` | Memesan tempat tidur | `InpatientBedOccupancy : Create` | `ReserveBedRequest` | `ApiResponse<BedReservationResponse>` | `EPIC RI-22` | **Rencana (belum tersedia)** |
| `POST` | `/placements` | Menempatkan pasien dan mengaktifkan episode | `InpatientBedOccupancy : Create` | `PlacePatientRequest` | `ApiResponse<BedPlacementResponse>` | `EPIC RI-23` | **Rencana (belum tersedia)** |
| `POST` | `/placements/transfer` | Memindahkan pasien | `InpatientBedOccupancy : Transfer` | `TransferPatientRequest` | `ApiResponse<BedPlacementResponse>` | `EPIC RI-26` | **Rencana (belum tersedia)** |

### Health Services / Inpatient Management / Inpatient Discharge

Base URL: `api/v1/health-services/inpatient-management/discharges`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/{episodeId}/decide` | Menyatakan pasien boleh pulang | `InpatientDischarge : Update` | `DecideDischargeRequest` | `ApiResponse<InpatientEpisodeDetailResponse>` | `EPIC RI-27` | **Rencana (belum tersedia)** |
| `POST` | `/{episodeId}/record-departure` | Mencatat pasien sudah meninggalkan ruangan | `InpatientDischarge : RecordDeparture` | `RecordDepartureRequest` | `ApiResponse<InpatientEpisodeDetailResponse>` | `EPIC RI-28` | **Rencana (belum tersedia)** |
| `PATCH` | `/{episodeId}/summary/sign` | Menandatangani resume | `InpatientDischarge : Sign` | `SignDischargeSummaryRequest` | `ApiResponse<DischargeSummaryResponse>` | `EPIC RI-27` | **Rencana (belum tersedia)** |
| `POST` | `/{episodeId}/financial-clearance` | Menandai kelayakan keuangan | `InpatientFinancialClearance : Update` | `MarkFinancialClearanceRequest` | `ApiResponse<FinancialClearanceResponse>` | `EPIC RI-28` | **Rencana (belum tersedia)** |
| `GET` | `/{episodeId}/closure-readiness` | Memeriksa kelima syarat penutupan | `InpatientDischarge : Read` | – | `ApiResponse<ClosureReadinessResponse>` | `EPIC RI-28` | **Rencana (belum tersedia)** |
| `POST` | `/{episodeId}/close` | Menutup episode | `InpatientEpisode : Close` | `CloseEpisodeRequest` | `ApiResponse<InpatientEpisodeDetailResponse>` | `EPIC RI-28` | **Rencana (belum tersedia)** |
| `POST` | `/{episodeId}/close-with-override` | Menutup menembus gerbang keuangan | `InpatientEpisode : CloseOverride` | `CloseEpisodeOverrideRequest` | `ApiResponse<InpatientEpisodeDetailResponse>` | `EPIC RI-28` | **Rencana (belum tersedia)** |

### Health Services / Inpatient Management / Inpatient Census

Base URL: `api/v1/health-services/inpatient-management/census`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar pasien dirawat beserta lokasi dan lama dirawat | `InpatientCensus : Read` | Query | `ApiResponse<CensusPagedResult>` | `EPIC RI-24` | **Rencana (belum tersedia)** |

### Health Services / Inpatient Management / Inpatient Monitoring

Base URL: `api/v1/health-services/inpatient-management/monitoring`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/pending-closures` | Daftar pantau penutupan tertunda | `InpatientMonitoring : Read` | Query | `ApiResponse<PendingClosurePagedResult>` | `EPIC RI-29` | **Rencana (belum tersedia)** |
| `GET` | `/bed-drift` | Laporan selisih tempat tidur | `InpatientMonitoring : Read` | Query | `ApiResponse<BedDriftPagedResult>` | `EPIC RI-29` | **Rencana (belum tersedia)** |
| `GET` | `/isolation-mismatch` | Daftar pantau episode yang kebutuhan isolasinya tidak cocok dengan tempat tidur yang ditempati | `InpatientMonitoring : Read` | Query | `ApiResponse<IsolationMismatchPagedResult>` | `EPIC RI-34` | **Rencana (belum tersedia)** |

### Health Services / Master Data / Inpatient Setting

Base URL: `api/v1/health-services/master-data/inpatient-settings`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `PUT` | `/{id}` | Mengubah nilai pengaturan | `InpatientSetting : Update` | `UpdateInpatientSettingRequest` | `ApiResponse<InpatientSettingResponse>` | `EPIC RI-31` | **Rencana (belum tersedia)** |

### Health Services / Master Data / Bed

Base URL: `api/v1/health-services/master-data/beds`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `PATCH` | `/{id}/availability` | Menolak nilai terisi dan dipesan | `Bed : Update` | `UpdateBedAvailabilityRequest` | `ApiResponse<BedUpdateResponse>` | `EPIC RI-32` | **Rencana perubahan perilaku** |

---

## 14. Matriks kewenangan

String hak akses di bawah sama persis dengan
[`contracts/permission-audit-matrix.md`](./contracts/permission-audit-matrix.md).

| Tindakan | Peran | String yang dipakai |
| --- | --- | --- |
| Membuka admisi | Petugas admisi, Supervisor | `[AccessPermission("InpatientEpisode", "Create")]` |
| Memesan dan menempatkan tempat tidur | Petugas admisi, Supervisor | `[AccessPermission("InpatientBedOccupancy", "Create")]` |
| Memindahkan pasien | Kepala ruangan, Perawat, DPJP, Supervisor | `[AccessPermission("InpatientBedOccupancy", "Transfer")]` |
| Mengalihkan DPJP, menugaskan perawat, membatalkan admisi | Kepala ruangan, Supervisor | `[AccessPermission("InpatientEpisode", "Update")]` |
| Menyatakan pasien boleh pulang, menyusun resume | DPJP | `[AccessPermission("InpatientDischarge", "Update")]` |
| Menandatangani resume | DPJP | `[AccessPermission("InpatientDischarge", "Sign")]` |
| Menandai kelayakan keuangan | Kasir, Billing | `[AccessPermission("InpatientFinancialClearance", "Update")]` |
| Mencatat pasien sudah meninggalkan ruangan | Petugas admisi, Perawat, Kepala ruangan, Supervisor | `[AccessPermission("InpatientDischarge", "RecordDeparture")]` |
| Menutup episode | Petugas admisi, Supervisor | `[AccessPermission("InpatientEpisode", "Close")]` |
| Menutup menembus gerbang keuangan | Supervisor | `[AccessPermission("InpatientEpisode", "CloseOverride")]` |
| Membuka dan menutup sesi koreksi | Supervisor | `[AccessPermission("InpatientEpisode", "Reopen")]` |
| Melihat census dan daftar pantau | Seluruh peran klinis dan admisi | `[AccessPermission("InpatientCensus", "Read")]`, `[AccessPermission("InpatientMonitoring", "Read")]` |
| Menetapkan kebutuhan isolasi | Petugas admisi selagi episode `Draft`; DPJP aktif setelah episode aktif | `[AccessPermission("InpatientEpisode", "SetIsolation")]` |
| Mengubah pengaturan dan butir administrasi | Admin master data | `[AccessPermission("InpatientSetting", "Update")]`, `[AccessPermission("InpatientClearanceItem", "Update")]` |

**Kewenangan yang tidak dijaga mesin hak akses.** **Empat** penjaga berikut ditulis di dalam
service, karena mesin hak akses hanya mengenal peran terhadap endpoint: `GUARD-INP-01` perpindahan
oleh DPJP, `GUARD-INP-02` keputusan pulang, `GUARD-INP-03` penandatanganan resume, dan
`GUARD-INP-04` perubahan kebutuhan isolasi setelah episode aktif.

`GUARD-INP-04` adalah alasan kenapa `SetIsolation` dimiliki dua peran sekaligus pada tabel di atas.
Mesin hak akses hanya dapat menjawab "peran ini boleh memanggil endpoint ini"; ia tidak dapat
membedakan petugas admisi yang menyetel nilai selagi `Draft` dari dokter yang bukan DPJP episode
tersebut. Pembedaannya dikerjakan service.

---

## 15. Batas integrasi dan billing

### 15.1 Yang **tidak boleh** dibuat sendiri modul ini

| Yang tidak boleh dibuat | Pemiliknya |
| --- | --- |
| Salinan pasien, dokter, pegawai, tempat tidur, kamar, unit layanan, kelas pasien | Patient Management, HR Workforce, Master Data |
| Faktur, tagihan berjalan, tarif, perhitungan biaya, refund, klaim | Billing Management dan Insurance Management |
| Tabel pengkajian, catatan dokter, diagnosis, tindakan, resep versi Rawat Inap | Clinical Management dan Pharmacy Management |
| Antrean untuk pasien rawat inap | Laporan antrean poliklinik tidak boleh tercemar |
| Mesin hak akses baru | Sudah ada dan dipakai ulang |

### 15.2 Kelayakan keuangan pada MVP

`BillingManagement` belum punya kemampuan transaksi, sehingga nilai `Pending`, `Cleared`, dan
`Blocked` tidak ada sumbernya. Pada MVP, nilai itu **disimpan pada episode dan ditandai manual**
petugas kasir, disertai nama penandai, waktu, dan catatan wajib.

Ini bukan sistem billing mini. Yang disimpan hanya **pernyataan kelayakan**, bukan angka tagihan.

**Risiko yang diterima secara sadar:** kasir dapat menandai `Cleared` tanpa ada tagihan yang sungguh
dibuat, karena memang belum ada tagihan yang bisa diperiksa sistem. Tercatat sebagai `RWI-RISK-003`.
Yang membedakan dari penanda serupa yang bermasalah di modul lain: penandaan ini menyimpan pelaku
dan waktu, ditampilkan jelas sebagai penandaan manual, dan bersifat sementara.

Ketika `BillingManagement` operasional, **sumber nilainya berpindah, aturan penutupannya tidak
berubah**.

### 15.3 Charge kamar

Tidak satu pun charge kamar tercatat selama MVP. Yang dijamin arsitektur adalah **datanya dapat
direkonstruksi**: dari riwayat penempatan, kelas dan lamanya menempati setiap kamar terbaca lengkap.

Keputusan apakah episode lama ikut ditagihkan mundur adalah keputusan keuangan yang belum ada
pemiliknya.

---

## 16. Guardrail regulasi

| Kewajiban | Yang dipenuhi MVP | Yang belum |
| --- | --- | --- |
| Rekam medis elektronik | Resume pulang tersimpan, tertandatangani, dan terkunci setelah episode ditutup. Riwayat lokasi, DPJP, dan status tersimpan lengkap | Pengkajian, catatan dokter, dan CPPT belum masuk sistem — `DEC-INP-001` |
| Keterlacakan tindakan | Setiap perubahan status meninggalkan jejak yang tidak dapat diubah, lengkap dengan pelaku dan waktu | — |
| Koreksi rekam medis | Koreksi hanya lewat sesi koreksi supervisor, beralasan, daftar perubahannya tersimpan, dan versi resume sebelumnya tersalin | — |
| Pengendalian infeksi dan privasi kamar | Penempatan dan perpindahan **ditolak** bila jenis kelamin tidak cocok, bila kamar sedang dihuni jenis kelamin berbeda, atau bila kebutuhan isolasi tidak cocok dengan sifat tempat tidur. Kebutuhan isolasi tersimpan beserta siapa dan kapan menetapkannya | Kebutuhan isolasi tersimpan sebagai **nilai berlaku**, bukan riwayat. Bila kelak audit pengendalian infeksi menuntut rentang tanggalnya, dibutuhkan Amendment Pass |
| Masa simpan data | — | **Belum diputuskan** — `RWI-OQ-035`, keputusan hukum |
| Interoperabilitas nasional | — | **Belum diputuskan** — `DEC-INP-005` |
| Persetujuan pasien | — | **Belum diputuskan** — `DEC-INP-003` |

**Yang wajib disadari:** **tiga** baris terakhir adalah gerbang keras. Modul ini **tidak boleh**
dipakai melayani pasien sungguhan sebelum ketiganya terjawab, walaupun MVP-nya sudah selesai
dikerjakan.

**Satu gerbang keras dicabut pada `0.3.0`.** Pengendalian infeksi dan privasi kamar sebelumnya
tercatat "belum diputuskan" dan menahan pemakaian modul untuk pasien sungguhan. `RWI-DEC-064`
sampai `RWI-DEC-066` menurunkan keputusannya pada 2026-08-21, dan `EPIC RI-34` mengerjakannya di
dalam MVP. Gerbangnya kini berpindah bentuk: bukan lagi menunggu keputusan, melainkan menunggu
`EPIC RI-34` benar-benar lolos uji.

---

## 17. Kebutuhan non-fungsional

| ID | Kebutuhan | Isinya |
| --- | --- | --- |
| `NFR-001` | Keutuhan tindakan | Penempatan, perpindahan, pembatalan, dan penutupan bersifat utuh: berhasil seluruhnya atau tidak ada yang berubah |
| `NFR-002` | Pencegahan tabrakan | Satu tempat tidur tidak pernah dipegang dua episode, dan satu pasien tidak pernah punya dua episode yang hadir. Dijaga penguncian baris ditambah **empat** unique index parsial |
| `NFR-003` | Jejak audit | Setiap perpindahan status meninggalkan baris riwayat yang tidak dapat diubah, ditulis dalam transaksi yang sama |
| `NFR-004` | Otorisasi | Hak akses per peran memakai mesin yang sudah ada; kewenangan per pasien dijaga di dalam service |
| `NFR-005` | Koreksi | Kesalahan dibetulkan lewat sesi koreksi, bukan lewat penyuntingan diam-diam |
| `NFR-006` | Penanganan waktu | Seluruh waktu UTC. Kedaluwarsa dihitung saat dibaca, tanpa program penjadwal. Lama dirawat dari selisih tanggal |
| `NFR-007` | Privasi | Kolom sensitif tidak masuk log dan tidak tampil pada daftar |
| `NFR-008` | Test regresi | Setiap task yang menyentuh modul milik pihak lain membawa test regresi jalur lama |

---

## 18. Skenario UAT

Setiap epic `MUST HAVE` punya sekurang-kurangnya satu skenario berhasil dan satu skenario gagal.

> **`UAT-01` — Satu pasien dari masuk sampai pulang** (`EPIC RI-21` s.d. `RI-28`)
> **Kondisi awal:** master kamar dan tempat tidur terisi; `BD-RSMMC-00042` tersedia.
> **Langkah:** petugas admisi membuka admisi Tn. Budi, memesan tempat tidur, menempatkan; kepala
> ruangan menugaskan perawat; DPJP menyatakan boleh pulang; DPJP menandatangani resume; petugas
> menandai tiga butir administrasi; kasir menandai lunas; petugas menutup episode.
> **Hasil yang diharapkan:** episode `Closed`, `BD-RSMMC-00042` kembali tersedia, riwayat status
> memuat empat baris berurutan, resume tersimpan tertandatangani.

> **`UAT-02` — Dua petugas merebut tempat tidur yang sama** (`EPIC RI-23`)
> **Kondisi awal:** `BD-RSMMC-00042` tersedia.
> **Langkah:** dua petugas menempatkan pasien berbeda ke tempat tidur itu pada waktu hampir
> bersamaan.
> **Hasil yang diharapkan:** satu berhasil, satu ditolak dengan pesan yang terbaca pengguna. Census
> menampilkan tepat satu pasien pada tempat tidur tersebut.

> **`UAT-03` — Pemesanan gugur sendiri** (`EPIC RI-22`)
> **Kondisi awal:** pemesanan dibuat pukul 09:15 dengan batas 2 jam.
> **Langkah:** buka daftar tempat tidur pukul 11:14, lalu pukul 11:16. Tidak ada proses latar
> belakang yang dijalankan.
> **Hasil yang diharapkan:** pukul 11:14 masih terkunci; pukul 11:16 sudah tersedia.

> **`UAT-04` — Memesan tempat tidur yang sudah dipesan** (`EPIC RI-22`, gagal)
> **Hasil yang diharapkan:** ditolak dengan pesan "Tempat tidur ini sudah dipesan untuk pasien
> lain". Tidak ada pemesanan kedua yang tersimpan.

> **`UAT-05` — Lama dirawat pasien yang menginap semalam** (`EPIC RI-24`)
> **Kondisi awal:** pasien masuk 21 September pukul 22:30.
> **Langkah:** buka census pada 22 September pukul 06:00.
> **Hasil yang diharapkan:** lama dirawat tertulis **1 hari**, dan layar menjelaskan bahwa itu
> hitungan hari rawat, bukan lama waktu sebenarnya.

> **`UAT-06` — Census tidak menampilkan episode yang belum aktif** (`EPIC RI-24`, gagal)
> **Kondisi awal:** ada episode `Draft`, `Closed`, dan `Cancelled`.
> **Hasil yang diharapkan:** ketiganya **tidak** muncul di census.

> **`UAT-07` — Pengalihan DPJP dan buktinya** (`EPIC RI-25`)
> **Langkah:** dr. Andi memindahkan pasien pada hari kedua. Pada hari ketiga DPJP dialihkan ke
> dr. Rina. Buka riwayat DPJP pada hari kelima.
> **Hasil yang diharapkan:** riwayat menampilkan dua baris berperiode, dan perpindahan hari kedua
> masih terbukti diminta dokter yang saat itu berwenang.

> **`UAT-08` — Dokter jaga mencoba memindahkan pasien orang lain** (`EPIC RI-26`, gagal)
> **Langkah:** dr. Rina yang bukan DPJP memindahkan Tn. Budi.
> **Hasil yang diharapkan:** ditolak dengan pesan "Hanya DPJP episode ini yang dapat memindahkan
> pasien". Tidak ada kolom keterangan yang dapat dipakai melewatinya.

> **`UAT-09` — Perpindahan gagal di tengah jalan** (`EPIC RI-26`, gagal)
> **Langkah:** paksa kegagalan saat penempatan baru dibuka.
> **Hasil yang diharapkan:** Tn. Budi tetap tercatat di tempat tidur semula. Tidak pernah ada saat
> pasien tercatat tanpa tempat tidur.

> **`UAT-10` — Resume ditandatangani orang yang salah** (`EPIC RI-27`, gagal)
> **Langkah:** dokter yang bukan DPJP aktif menandatangani resume.
> **Hasil yang diharapkan:** ditolak dengan pesan yang menyebut alasannya.

> **`UAT-11` — Menutup episode yang syaratnya belum lengkap** (`EPIC RI-28`, gagal)
> **Langkah:** petugas admisi menutup episode pukul 10:00, sementara resume belum ditandatangani
> dan kasir belum menandai lunas.
> **Hasil yang diharapkan:** ditolak, dan layar menampilkan **kelima syarat** beserta tanda sudah
> atau belum, bukan satu kalimat umum.

> **`UAT-12` — Supervisor menutup menembus gerbang keuangan** (`EPIC RI-28`)
> **Langkah:** kasir tidak di tempat, pasien harus segera pulang. Supervisor menutup disertai alasan.
> **Hasil yang diharapkan:** episode `Closed`, ditandai, dan muncul pada laporan pengecualian.

> **`UAT-13` — Supervisor mencoba menembus syarat selain keuangan** (`EPIC RI-28`, gagal)
> **Langkah:** supervisor menutup sementara resume belum ditandatangani.
> **Hasil yang diharapkan:** tetap ditolak. Jalan keluar hanya menembus syarat keuangan.

> **`UAT-14` — Koreksi cara pulang setelah episode ditutup** (`EPIC RI-30`)
> **Kondisi awal:** episode Ibu Sari ditutup 15 Agustus; `MELATI-03` sudah ditempati pasien lain.
> **Langkah:** pada 17 Agustus supervisor membuka sesi koreksi, mengubah cara pulang, menutup sesi
> beserta daftar perubahan.
> **Hasil yang diharapkan:** cara pulang berubah; status episode tetap Selesai; `MELATI-03` tidak
> terganggu; lama dirawat tetap 3 hari; Ibu Sari tidak muncul di census.

> **`UAT-15` — Menutup sesi koreksi tanpa daftar perubahan** (`EPIC RI-30`, gagal)
> **Hasil yang diharapkan:** ditolak. Ini satu-satunya jejak koreksi.

> **`UAT-16` — Riwayat pemesanan yang gugur** (`EPIC RI-29`)
> **Hasil yang diharapkan:** baris riwayat bertanda dilakukan sistem, tanpa nama orang.

> **`UAT-17` — Mencoba menghapus riwayat status** (`EPIC RI-29`, gagal)
> **Hasil yang diharapkan:** tidak ada endpoint yang menyediakannya.

> **`UAT-18` — Admin mengubah batas pemesanan** (`EPIC RI-31`)
> **Langkah:** ubah dari 2 jam menjadi 3 jam pukul 14:00.
> **Hasil yang diharapkan:** pemesanan pukul 14:05 berlaku sampai 17:05. Pemesanan yang dibuat
> pukul 13:30 tetap memakai batas lama.

> **`UAT-19` — Modul berjalan tanpa baris pengaturan** (`EPIC RI-31`, gagal)
> **Kondisi awal:** lingkungan baru tanpa `MstInpatientSetting`.
> **Hasil yang diharapkan:** modul tetap berjalan memakai nilai bawaan, dan peringatan tercatat.

> **`UAT-20` — Menonaktifkan tempat tidur yang rusak** (`EPIC RI-32`)
> **Langkah:** admin menonaktifkan `BD-RSMMC-00042` dari halaman detail.
> **Hasil yang diharapkan:** berhasil, dan tempat tidur hilang dari pencarian. Hari ini tombol ini
> selalu gagal.

> **`UAT-21` — Admin mencoba menyetel tempat tidur menjadi terisi** (`EPIC RI-32`, gagal)
> **Hasil yang diharapkan:** ditolak dengan pesan yang mengarahkan ke modul Rawat Inap. Menyetel
> Perbaikan tetap berhasil.

> **`UAT-22` — Bayi dirawat gabung dengan ibunya** (`EPIC RI-33`)
> **Kondisi awal:** boks `BOX-MELATI-03-A` terdaftar sebagai tempat tidur di kamar Melati 3.
> **Langkah:** bayi Ny. Sari didaftarkan, dibuatkan episode sendiri, lalu ditempatkan di boks itu.
> **Hasil yang diharapkan:** census menampilkan dua baris. Menutup episode Ny. Sari tidak menutup
> episode bayinya.

> **`UAT-24` — Tempat tidur bebas sejak pasien meninggalkan kamar** (`EPIC RI-28`)
> **Kondisi awal:** Tn. Budi berstatus rencana pulang di `BD-RSMMC-00105`.
> **Langkah:** keluarga menjemput pukul 10:15. Perawat mencatat kepergiannya. Pukul 10:40 petugas
> admisi memesan tempat tidur itu untuk Ny. Sari. Episode Tn. Budi baru ditutup pukul 13:10.
> **Hasil yang diharapkan:** pemesanan pukul 10:40 berhasil. Episode Tn. Budi tetap berstatus
> rencana pulang sampai 13:10 dan tetap muncul pada daftar pantau penutupan tertunda.

> **`UAT-25` — Mencatat kepergian pasien yang belum diputuskan pulang** (`EPIC RI-28`, gagal)
> **Langkah:** perawat mencatat kepergian pada episode yang masih berstatus sedang dirawat.
> **Hasil yang diharapkan:** ditolak dengan pesan yang menyebut bahwa DPJP harus menyatakan pasien
> boleh pulang lebih dulu. Tempat tidur tidak berubah.

> **`UAT-26` — Satu pasien tidak dapat dirawat di dua tempat** (`EPIC RI-23`, gagal)
> **Kondisi awal:** Tn. Budi sedang dirawat di Melati 3B.
> **Langkah:** petugas lain menempatkan Tn. Budi di Anggrek 1A.
> **Hasil yang diharapkan:** ditolak dengan pesan yang menyebut nomor episode dan lokasi yang
> sedang ditempati, sehingga petugas tahu yang dibutuhkan adalah perpindahan.

> **`UAT-27` — Koreksi resume menyimpan versi lamanya** (`EPIC RI-27`)
> **Kondisi awal:** resume Ibu Sari sudah ditandatangani dr. Andi dengan cara pulang "kabur".
> **Langkah:** supervisor membuka sesi koreksi, mengubah cara pulang menjadi "atas permintaan
> sendiri", lalu menutup sesi.
> **Hasil yang diharapkan:** resume yang berlaku menampilkan cara pulang baru, dan versi lama
> beserta nama penandatangannya tetap dapat dibaca.

> **`UAT-28` — Perawat menemukan bayi siapa yang ada di boks** (`EPIC RI-33`)
> **Kondisi awal:** bayi Ny. Sari ditempatkan di `BOX-MELATI-03-A`.
> **Langkah:** perawat membuka detail boks tersebut.
> **Hasil yang diharapkan:** sistem menyebut bahwa penghuninya bayi Ny. Sari yang dirawat di
> Melati 3, bukan hanya menampilkan nama bayi tanpa hubungan.

> **`UAT-29` — Kamar tidak menjadi campur** (`EPIC RI-34`, gagal)
> **Kondisi awal:** Kamar Melati 3 berisi tiga tempat tidur. Ny. Sari menempati `MELATI-03-A`.
> **Langkah:** petugas admisi menempatkan Tn. Budi di `MELATI-03-B`.
> **Hasil yang diharapkan:** ditolak dengan kode 422 dan pesan yang **menyebut nama kamarnya**,
> supaya petugas langsung tahu kamar mana yang terhalang. Berikutnya Ny. Rina ditempatkan di tempat
> tidur yang sama dan **berhasil**.

> **`UAT-30` — Bayi tidak menutup kamar dan tidak tertutup kamar** (`EPIC RI-34`)
> **Kondisi awal:** Ny. Sari di `MELATI-03-A`, bayinya laki-laki.
> **Langkah:** perawat menempatkan bayi di boks `BOX-MELATI-03-A`, lalu petugas menempatkan Ny.
> Rina di `MELATI-03-B`.
> **Hasil yang diharapkan:** keduanya **berhasil**. Penempatan bayi tidak ditolak walaupun jenis
> kelaminnya berbeda dari penghuni kamar, dan kehadiran bayi laki-laki itu tidak menghalangi
> Ny. Rina.

> **`UAT-31` — Tempat tidur isolasi dijaga dari dua arah** (`EPIC RI-34`, gagal)
> **Kondisi awal:** Tn. Budi bertanda membutuhkan isolasi. `BD-RSMMC-00042` bukan tempat tidur
> isolasi; `ISO-01-A` adalah tempat tidur isolasi. Ny. Rina tidak membutuhkan isolasi.
> **Langkah:** petugas menempatkan Tn. Budi di `BD-RSMMC-00042`, lalu menempatkan Ny. Rina di
> `ISO-01-A`.
> **Hasil yang diharapkan:** keduanya ditolak dengan kode 422 dan pesan yang berbeda — yang pertama
> menyebut pasien membutuhkan isolasi, yang kedua menyebut kapasitas isolasi tidak boleh terpakai
> pasien biasa.

> **`UAT-32` — Petugas admisi merekam, DPJP memutuskan** (`EPIC RI-34`)
> **Kondisi awal:** episode Tn. Budi masih `Draft`, surat rujukan menyebut suspek penyakit menular.
> **Langkah:** petugas admisi menyalakan kebutuhan isolasi disertai keterangan. Setelah episode
> aktif, dr. Rina yang bukan DPJP mencoba mematikannya. Kemudian dr. Andi selaku DPJP aktif
> mematikannya.
> **Hasil yang diharapkan:** yang pertama tersimpan bertanda **catatan awal** atas nama petugas
> admisi. Percobaan dr. Rina ditolak dengan kode 403. Perubahan dr. Andi tersimpan bertanda
> **keputusan klinis** atas namanya. Percobaan menyalakan tanpa keterangan ditolak dengan kode 400.

> **`UAT-33` — Perubahan isolasi tidak pernah ditahan** (`EPIC RI-34`)
> **Kondisi awal:** Tn. Budi sedang berbaring di `MELATI-03-B` yang bukan tempat tidur isolasi.
> **Langkah:** dr. Andi menyalakan kebutuhan isolasi pukul 14:00. Petugas membuka daftar pantau
> penempatan tidak sesuai. Pukul 15:20 Tn. Budi dipindahkan ke `ISO-01-A`.
> **Hasil yang diharapkan:** pencatatan pukul 14:00 **diterima**, tidak ditahan. Episode Tn. Budi
> muncul pada daftar pantau di antara pukul 14:00 dan 15:20, lalu hilang dari sana setelah
> dipindahkan. Perpindahan itu sendiri lolos karena tempat tidur tujuannya isolasi.

> **`UAT-23` — Membatalkan admisi setelah pasien dirawat** (`EPIC RI-21`, gagal)
> **Langkah:** petugas admisi membatalkan episode berstatus Sedang dirawat.
> **Hasil yang diharapkan:** ditolak. Hanya supervisor atau kepala ruangan yang boleh.

---

## 19. Definition of Done

| Butir | Bukti |
| --- | --- |
| Satu pasien dapat berjalan dari admisi sampai tempat tidur dilepas | `UAT-01` |
| Tempat tidur ganda tidak mungkin terjadi | `UAT-02`, `UAT-04` |
| Pemesanan gugur sendiri tanpa program penjadwal | `UAT-03` |
| Lama dirawat dihitung dari selisih tanggal dan terbaca jelas maknanya | `UAT-05` |
| Census hanya menampilkan pasien yang benar-benar sedang dirawat | `UAT-06` |
| Sistem dapat menjawab siapa DPJP pada tanggal tertentu | `UAT-07` |
| Dokter yang bukan DPJP tidak dapat memindahkan pasien | `UAT-08` |
| Pasien tidak pernah tercatat tanpa tempat tidur | `UAT-09` |
| Resume hanya dapat ditandatangani DPJP aktif | `UAT-10` |
| Kelima syarat penutupan diperiksa dan dilaporkan satu per satu | `UAT-11` |
| Jalan keluar supervisor hanya menembus syarat keuangan | `UAT-12`, `UAT-13` |
| Koreksi tidak mengganggu tempat tidur dan tidak menambah lama dirawat | `UAT-14` |
| Setiap koreksi meninggalkan jejak | `UAT-15` |
| Perubahan yang dihitung sistem tidak menuduh orang | `UAT-16` |
| Riwayat status tidak dapat dihapus | `UAT-17` |
| Seluruh angka dapat diubah admin dan berlaku pada pembacaan berikutnya | `UAT-18`, `UAT-19` |
| Admin tetap dapat menutup tempat tidur yang rusak | `UAT-20` |
| Status penghunian hanya lahir dari modul Rawat Inap | `UAT-21` |
| Bayi mendapat episode sendiri di boks kamar ibunya | `UAT-22` |
| Sistem dapat menjawab bayi siapa yang berada di boks kamar mana | `UAT-28` |
| Tempat tidur bebas sejak pasien meninggalkan kamar, tanpa menunggu penutupan | `UAT-24` |
| Kepergian hanya dapat dicatat setelah DPJP menyatakan pasien boleh pulang | `UAT-25` |
| Satu pasien tidak pernah tercatat dirawat di dua tempat | `UAT-26` |
| Koreksi resume yang sudah ditandatangani menyimpan versi lamanya | `UAT-27` |
| Pembatalan setelah pasien dirawat hanya oleh peran yang berwenang | `UAT-23` |
| Kamar tidak pernah menjadi campur laki-laki dan perempuan | `UAT-29` |
| Boks bayi dikecualikan dari kedua sisi pemeriksaan jenis kelamin | `UAT-30` |
| Kapasitas isolasi terjaga dari dua arah | `UAT-31` |
| Catatan awal admisi dapat dibedakan dari keputusan klinis DPJP | `UAT-32` |
| Pencatatan klinis tidak pernah ditahan demi aturan penempatan | `UAT-33` |
| Aturan penempatan berlaku sama pada perpindahan | `UAT-29` dijalankan ulang lewat perpindahan; `RWI-AC-133` |
| Seluruh tabel master MVP sudah terisi | Rencana data master awal pada `02-backend-architecture.md` bagian 8 |
| Setiap task yang menyentuh modul lain membawa test regresi | `RWI-AC-114`, `testing/acceptance-test-matrix.md` bagian 12 |

---

## 20. Urutan pengiriman dan pertanyaan terbuka

### 20.1 Gelombang pengiriman

Ditulis sebagai gelombang, bukan tanggal. Penjadwalan tetap wewenang manusia.

| Gelombang | Epic yang tercakup | Syarat mulai |
| --- | --- | --- |
| `MVP-0` | `EPIC RI-21` fondasi, `EPIC RI-31` pengaturan, `EPIC RI-32` perbaikan tempat tidur | Blueprint disetujui; persetujuan pemilik Master Data untuk `RI-32` |
| `MVP-1` | `EPIC RI-22` pemesanan, `EPIC RI-23` penempatan beserta aturan satu pasien satu episode, `EPIC RI-24` census, **`EPIC RI-34` kelayakan penempatan** | `MVP-0` selesai; master kamar dan tempat tidur terisi **beserta penanda jenis kelamin, isolasi, dan boks bayi yang benar** |
| `MVP-2` | `EPIC RI-25` penanggung jawab, `EPIC RI-26` perpindahan | `MVP-1` selesai |
| `MVP-3` | `EPIC RI-27` pulang, resume, dan versi resume; `EPIC RI-28` penutupan dan pencatatan kepergian fisik | `MVP-2` selesai |
| `MVP-4` | `EPIC RI-29` riwayat dan daftar pantau, `EPIC RI-30` sesi koreksi, `EPIC RI-33` bayi beserta penanda rawat gabung | `MVP-3` selesai |
| `POST-MVP` | Seluruh kemampuan yang ditunda pada bagian 8 | Di luar cakupan rilis pertama; masing-masing menunggu Decision ID-nya |

**`MVP-1` adalah gelombang pertama yang menghasilkan data nyata.** Setelah gelombang itu selesai,
rumah sakit sudah dapat mencatat siapa menempati tempat tidur mana — kemampuan yang hari ini sama
sekali tidak ada.

**`EPIC RI-34` sengaja ditaruh di `MVP-1`, bukan digeser ke gelombang belakang.** Alasannya: aturan
penempatan yang menolak harus sudah berlaku **sejak data penempatan pertama lahir**. Bila epic ini
menyusul di `MVP-4`, gelombang sebelumnya akan lebih dulu menghasilkan penempatan yang melanggar,
dan penempatan yang sudah telanjur ada tidak dapat ditolak surut. Konsekuensinya, syarat mulai
`MVP-1` bertambah: penanda pada master tempat tidur harus **benar**, bukan sekadar terisi.

Tidak ada satu pun epic berstatus `OPEN DECISION` yang masuk gelombang mana pun. Sembilan
kemampuan yang ditunda pada bagian 8 seluruhnya berada di `POST-MVP`.

### 20.2 Pertanyaan terbuka sebelum development lock

**Empat pertanyaan yang memblokir seluruhnya tertutup pada 2026-08-21.** Daftar di bawah
mempertahankan barisnya beserta jawabannya, bukan menghapusnya, supaya pembaca berikutnya tahu
kenapa keputusannya berbunyi demikian.

| Pertanyaan | Siapa yang menjawab | Status | Memblokir |
| --- | --- | --- | :---: |
| Siapa nama orang atau komite yang berwenang menyetujui modul ini? | Manajemen rumah sakit | **Tertutup** `RWI-DEC-061` — Muhammad Hamzah, ditunjuk 2026-08-21. Jabatan formalnya belum diisi | ~~Ya~~ |
| Apakah pemilik `MasterData` menyetujui pembatasan endpoint ketersediaan tempat tidur? | Pemilik `MasterData` | **Tertutup** `RWI-DEC-062` — keempat modul tetangga berada di bawah kepemilikan yang sama, dan persetujuannya diberikan | ~~Ya~~ |
| Siapa yang bertanggung jawab mengisi master kamar dan tempat tidur, dan kapan batasnya? | Manajemen rumah sakit | **Tertutup** `RWI-DEC-063` — Admin Master Data / Tim Master Data, target 22 Agustus 2026. Gerbangnya baru benar-benar tertutup ketika datanya terisi, bukan ketika penanggung jawabnya ditunjuk | Sampai data terisi |
| Apakah kebutuhan isolasi dan pemisahan jenis kelamin menolak penempatan, atau hanya menyaring? | Pemilik klinis dan privasi | **Tertutup** `RWI-DEC-064` s.d. `RWI-DEC-066` — **menolak**. Dikerjakan `EPIC RI-34` di dalam MVP | ~~Ya, untuk produksi~~ |
| Berapa lama riwayat status disimpan sebelum boleh diarsipkan? | Pemilik keamanan dan privasi | Sudah dijawab `RWI-DEC-060`, menunggu pemilik hukum. Tidak memblokir MVP | Tidak |
| Apakah kepergian fisik pasien dicatat sebagai kejadian tersendiri? | Pemilik proses | **Tertutup** `RWI-DEC-055` — ya, dan tempat tidur bebas sejak saat itu | Tidak |
| Apakah satu pasien boleh punya dua episode aktif sekaligus? | Pemilik proses | **Tertutup** `RWI-DEC-054` — tidak, dijaga unique index parsial | Tidak |
| Apakah resume pulang perlu riwayat versi? | Pemilik klinis | **Tertutup** `RWI-DEC-057` — ya, versi sebelumnya tersalin saat koreksi | Tidak |
| Apakah bayi dan ibunya perlu penanda rawat gabung? | Pemilik proses | **Tertutup** `RWI-DEC-056` — ya, kolom opsional rujukan episode ibu | Tidak |

### 20.3 Yang masih menahan, dan bentuknya bukan pertanyaan

| Butir | Bentuknya | Menahan apa |
| --- | --- | --- |
| Master kamar dan tempat tidur terisi **dan penandanya benar** | Pekerjaan data, bukan keputusan | `MVP-1`, termasuk `EPIC RI-34`. Penanda jenis kelamin, isolasi, dan boks bayi yang salah setel akan menolak penempatan yang sah, atau lebih buruk, meloloskan yang tidak sah |
| Test regresi jalur lama untuk modul yang disentuh | Pekerjaan uji | `EPIC RI-32` dan seluruh task yang menyentuh modul lain — `NFR-008` |
| Perbaikan pemanggilan tombol tempat tidur di frontend | Pekerjaan perbaikan | `EPIC RI-32` |
| Masa simpan data, interoperabilitas nasional, persetujuan pasien | Keputusan hukum dan klinis | Melayani pasien sungguhan, bukan pengerjaan MVP — bagian 16 |

Sesuai kontrak, dokumen ini tetap berstatus `draft` sampai ada approval manusia. Yang berubah pada
`0.3.0`: **tidak ada lagi pertanyaan memblokir yang menahan `/plan-module-delivery`.**
