# Radiologi — PRD ke MVP

## 1. Identitas Dokumen

| Field | Value |
|---|---|
| Produk | Quilvian Hospital System |
| Modul | Radiologi (`radiologi`), kode `RAD` |
| Contract version | `RAD-PRD-001` |
| Revision | `2` |
| Status | `approved` |
| Repository target | `NewQuilvianSystemBackend`, `QuilvianSystemFrontendDev` |
| Backend SHA baseline | `0e2eb105` |
| Frontend SHA baseline | `f66ed1885` |
| Input | `RAD-ARCH-BE-001`, `RAD-ARCH-FE-001`, `RAD-API-001`, `RAD-STATE-001`, `RAD-VAL-001`, `RAD-INT-001`, `RAD-PERM-001`, `RAD-TEST-001` |
| Kesiapan requirement | `RAD-RCG-001-r1` — `PARTIALLY_READY` |
| Kesiapan arsitektur domain | `RAD-DA-001-r1` — `DOMAIN_ARCHITECTURE_PARTIAL` |
| Owner | Yoga Aji Pratama |
| `approved_by` / `approved_at` | Yoga Aji Pratama, 2026-09-10 |
| Ringkasan cakupan | Melengkapi modul Radiologi yang mesin pemeriksaannya sudah berdiri, sampai hasil bacaan radiolog dapat dirilis dan dibaca dokter pengirim |

> **PERINGATAN PENTING.** Dokumen ini berstatus `draft` dan **belum boleh diteruskan ke
> `/plan-module-delivery`**. Dua pertanyaan pemblokir pada bagian 20 masih terbuka.

---

## 2. Ringkasan Eksekutif

Rumah sakit sudah memiliki sistem yang dapat menerima pesanan pemeriksaan radiologi,
menjadwalkannya, memastikan pasien aman diperiksa, mengambil citra, dan menilai mutunya.

Yang belum ada adalah **hasil bacaan dokter radiolog**. Padahal itulah yang dicari dokter
pengirim. Tanpa itu, seluruh rangkaian di atas berhenti pada "foto sudah diambil" dan tidak
menghasilkan apa pun yang dapat dipakai merawat pasien.

Lebih dari itu, ada masalah yang lebih mendesak: **modul ini saat ini tidak dapat menjalankan
satu pun pemeriksaan.** Gerbang keselamatan menolak pemeriksaan bila aturan keselamatannya
belum ditetapkan, dan sampai sekarang tidak ada satu pun cara untuk menetapkan aturan itu.

MVP ini menyelesaikan keduanya: membuat aturan keselamatan dapat dikelola dan disahkan, lalu
melengkapi rantai sampai hasil bacaan dirilis.

---

## 3. Masalah Produk

### Apa yang sudah ada

Dibuktikan dari source pada `0e2eb105`.

| Kemampuan | ID kemampuan | Bukti |
|---|---|---|
| Pesanan radiologi lengkap dengan 12 endpoint | `RAD-CAP-007` | `Controllers/RadOrderController.cs` |
| Study dan pengambilan citra, 14 endpoint | `RAD-CAP-009` | `Controllers/RadStudyController.cs` |
| Gerbang keselamatan yang menolak bila aturan belum ada | `RAD-CAP-015` | `Services/RadSafetyGateEvaluator.cs` |
| Penilaian mutu, pengulangan, penghentian | `RAD-CAP-009` | `Services/RadStudyService.cs` |
| Pencatatan bahan terpakai | `RAD-CAP-004` | `Models/RadAcquisitionConsumption.cs` |
| Penerbitan fakta kelayakan tagih ke Billing | `RAD-CAP-020` | `Services/RadStudyService.cs:930-960` |
| Riwayat perpindahan status | `RAD-CAP-014` | `Models/RadTransitionHistory.cs` |
| Konteks rawat inap pada pesanan | `RAD-CAP-006` | Migration `AddRadOrderInpatientContext` |

### Apa yang belum ada

| Kemampuan yang hilang | ID kemampuan | Akibatnya sekarang |
|---|---|---|
| Pengelolaan aturan keselamatan | `RAD-CAP-003` | **Tidak satu pun pemeriksaan dapat berjalan** |
| Pengelolaan alat dan butir keselamatan | `RAD-CAP-001`, `RAD-CAP-002` | Alat baru harus dimasukkan langsung ke database |
| Hasil bacaan radiolog | `RAD-CAP-010` | Bacaan ditulis di luar sistem |
| Koreksi hasil berversi | `RAD-CAP-011` | Tidak ada mekanisme koreksi yang aman |
| Seluruh tampilan frontend | `RAD-CAP-029` | Tidak ada satu pun layar Radiologi |
| Uji kontrak hak akses | `RAD-CAP-025` | Perubahan hak akses tidak akan ketahuan bila salah |

### Masalah yang paling mendesak

Gerbang keselamatan bersifat *fail-closed*: bila tidak ada aturan aktif untuk sebuah alat,
pemeriksaan **ditolak**. Itu perilaku yang benar dan diminta `RJ-BIL-DEC-014`.

Masalahnya, tidak ada endpoint, layar, maupun data awal untuk menetapkan aturan itu. Modul ini
terkunci dari dalam. Setiap upaya pengambilan citra akan ditolak dengan pesan "Aturan
keselamatan untuk modalitas ini belum ditetapkan".

---

## 4. Visi Produk

Rantai keterhubungan data yang ingin dicapai, ditulis sebagai urutan:

1. Pasien terdaftar dan memiliki kunjungan — milik Registration Management.
2. Dokter membuat pesanan radiologi yang menempel pada kunjungan itu.
3. Radiologi menerima dan menjadwalkan pesanan.
4. Radiografer memastikan identitas pasien benar.
5. Radiografer menjawab butir keselamatan yang **wajib menurut aturan yang sudah disahkan**.
6. Citra diambil, lalu dinilai layak atau tidak.
7. Citra yang layak menerbitkan fakta kelayakan tagih ke Billing.
8. Dokter radiolog menulis bacaan atas citra yang layak.
9. Bacaan disahkan lalu dirilis.
10. Dokter pengirim membaca hasilnya lewat rekam medis, **langsung dari modul Radiologi**.
11. Bila ada yang perlu diperbaiki, koreksi dibuat sebagai versi baru tanpa menghapus versi
    lama.

Setiap mata rantai dapat ditelusuri: `Pasien → Kunjungan → Pesanan → Study → Bacaan → Versi`.

---

## 5. Batas MVP

### Titik mulai

1. Registry kepemilikan modul sudah mencatat `Rad` berstatus `ACTIVE`.
2. Aturan keselamatan sudah dapat disusun dan disahkan lewat antarmuka.
3. Sekurang-kurangnya satu aturan aktif tersedia untuk setiap alat yang dipakai.

### Titik akhir

1. Seorang pasien dapat menjalani satu pemeriksaan radiologi dari pesanan sampai hasil bacaan
   dirilis, seluruhnya di dalam sistem.
2. Dokter pengirim dapat membaca hasilnya dari rekam medis.
3. Bacaan yang sudah dirilis dapat dikoreksi lewat versi baru tanpa menghapus versi lama.
4. Seluruh perbuatan tercatat dan dapat ditelusuri.

---

## 6. Pelaku Sasaran

| Pelaku | Tanggung jawabnya di dalam MVP |
|---|---|
| Dokter pengirim | Memesan pemeriksaan; membaca hasil bacaan |
| Petugas pendaftaran radiologi | Menerima, menjadwalkan, menolak, membatalkan pesanan |
| Radiografer | Memverifikasi pasien, menjawab butir keselamatan, mengambil citra, menilai mutu, mencatat bahan terpakai |
| Dokter radiolog | Menulis, mengesahkan, merilis, dan mengoreksi bacaan |
| Admin Radiologi | Mengelola alat, butir keselamatan, dan menyusun draf aturan |
| Penanggung jawab klinis | Mengesahkan atau menolak aturan keselamatan |

---

## 7. Pemilihan Kemampuan MVP — `MUST HAVE`

Uji yang dipakai: tanpa kemampuan ini, apakah satu kasus nyata bisa selesai dari awal sampai
akhir? Kalau tidak, apakah ada jalan sementara yang aman dan dapat diaudit?

| Kemampuan | ID kemampuan asal | Keputusan MVP |
|---|---|---|
| Pengelolaan dan pengesahan aturan keselamatan | `RAD-CAP-003` | **Wajib.** Tanpa ini tidak satu pun pemeriksaan berjalan |
| Pengelolaan alat pencitraan | `RAD-CAP-001` | **Wajib.** Aturan keselamatan tidak dapat disusun tanpa alat terdaftar |
| Pengelolaan butir keselamatan | `RAD-CAP-002` | **Wajib.** Alasan sama |
| Hasil bacaan radiolog | `RAD-CAP-010` | **Wajib.** Tanpa ini rantai berhenti di "foto sudah diambil" |
| Koreksi hasil berversi | `RAD-CAP-011` | **Wajib.** Tanpa ini bacaan salah hanya bisa diperbaiki dengan menimpanya, dan itu dilarang |
| Penyajian hasil ke rekam medis | `RAD-CAP-018` | **Wajib.** Hasil yang tidak sampai ke dokter pengirim tidak berguna |
| Frontend alur pemeriksaan | `RAD-CAP-029` | **Wajib.** Backend tanpa layar tidak dapat dipakai siapa pun |
| Frontend pengelolaan data induk | `RAD-CAP-029` | **Wajib.** Alasan sama |
| Daftar kerja petugas per alat | `RAD-CAP-013` | **Wajib.** Tanpa ini petugas tidak tahu apa yang harus dikerjakan hari itu, dan harus menyaring daftar pesanan seluruh unit setiap kali |
| Penanda cito pada pesanan | `RAD-CAP-013` | **Wajib.** Tanpa ini pemeriksaan mendesak tenggelam di antara pemeriksaan rutin yang dipesan lebih dulu |
| Uji kontrak hak akses | `RAD-CAP-025` | **Wajib.** Modul menyentuh keselamatan pasien; perubahan hak akses harus ketahuan |

---

## 8. Kemampuan yang Ditunda

| Kemampuan | ID kemampuan asal | Alasan ditunda | Pengganti selama MVP |
|---|---|---|---|
| Pelewatan gerbang keselamatan darurat | — (`S5`) | Tanda tangan tata kelola klinis belum ada; `DEC-RAD-001` | Pemeriksaan darurat yang butir wajibnya tidak dapat dijawab **ditunda** sampai butirnya dapat dijawab, atau dilakukan di luar sistem dan dicatat pada serah terima pasien |
| Temuan kritis dan pemberitahuannya | `RAD-CAP-012` | Daftar apa yang dihitung kritis belum ada; `DEC-RAD-002` | Radiolog menghubungi dokter pengirim lewat telepon dan mencatatnya pada kesimpulan bacaan. Tidak ada pemantauan pengakuan |
| Pemantauan keterlambatan pesanan cito | `RAD-CAP-013` | Butuh penetapan batas waktu per jenis pemeriksaan, dan penanggung jawab klinisnya belum ditunjuk; `RAD-OPEN-009` | Pesanan cito sudah muncul di urutan atas daftar kerja. Yang belum ada hanya peringatan otomatis bila terlambat |
| Integrasi PACS dan DICOM | `RAD-CAP-028` | Sengaja tidak diaktifkan `RJ-BIL-GATE-DEC-004` | Citra disimpan pada sistem alat masing-masing seperti sekarang |
| Status `Draft` pada pesanan | `RAD-CAP-008` | Diputuskan tidak dipakai; `RAD-DEC-011` | Pesanan langsung terkirim saat disimpan |

---

## 9. Alur Bisnis Target

### `FLOW-RAD-MVP-001` — Dari pesanan sampai hasil dibaca

**Tujuan.** Seorang pasien menjalani pemeriksaan radiologi dan dokter pengirimnya menerima
hasil bacaan yang sah.

**Pemicu.** Dokter memutuskan pasien perlu diperiksa dengan alat pencitraan.

**Prasyarat.** Pasien punya kunjungan aktif; alat yang dipakai punya aturan keselamatan
berstatus `Active`.

**Langkah utama.**

1. Dokter memilih pemeriksaan dan alat, mengisi indikasi klinis, lalu menyimpan. Pesanan lahir
   berstatus `Requested`.
2. Petugas pendaftaran radiologi menerima pesanan. Status menjadi `Accepted`.
3. Petugas menjadwalkan pemeriksaan. Status menjadi `Scheduled`.
4. Radiografer membuat rencana pengambilan citra. Study lahir berstatus `Planned`.
5. Radiografer memastikan identitas pasien benar. Study menjadi `PatientVerified`.
6. Sistem menampilkan butir keselamatan yang wajib menurut aturan aktif untuk alat itu.
   Radiografer menjawab seluruhnya.
7. Radiografer menyatakan gerbang keselamatan lolos. Study menjadi `SafetyCleared`, dan versi
   aturan yang berlaku saat itu dibekukan pada study.
8. Citra diambil. Study berpindah `AcquisitionStarted` lalu `Acquired`.
9. Radiografer mencatat bahan yang terpakai.
10. Mutu citra dinilai. Bila layak, study menjadi `QualityAccepted` dan **satu fakta kelayakan
    tagih terbit ke Billing**.
11. Sistem membuat wadah hasil bacaan berstatus `Pending`.
12. Dokter radiolog menulis draf bacaan. Status menjadi `Drafted`.
13. Dokter radiolog mengesahkan. Status menjadi `Validated`.
14. Dokter radiolog merilis. Status menjadi `Released`.
15. Dokter pengirim membuka rekam medis dan membaca hasilnya, diambil langsung dari modul
    Radiologi.

**Hasil akhir.** Pasien punya satu pemeriksaan tercatat lengkap dengan bacaan yang sah, Billing
menerima satu fakta kelayakan tagih, dan seluruh perbuatan dapat ditelusuri.

**Jalur tidak normal.**

| Keadaan | Yang terjadi |
|---|---|
| Alat belum punya aturan keselamatan aktif | Langkah 7 ditolak. Petugas diarahkan menghubungi admin Radiologi |
| Butir wajib dinyatakan tidak aman | Langkah 7 ditolak beserta sebutan butir yang menahan |
| Citra dinilai tidak layak | Study menjadi `QualityRejected`. **Tidak ada** fakta tagih. Study pengulangan dibuat, study lama tetap ada |
| Acquisition dihentikan di tengah jalan | Study menjadi `Aborted`. Bahan yang terlanjur terpakai tetap dicatat. Billing menilai akibatnya |
| Bacaan ternyata keliru setelah dirilis | Koreksi dibuat sebagai versi baru. Versi lama tetap dapat dibaca |
| Draf ditulis residen | Pengesahan wajib oleh dokter radiolog yang berbeda |

---

## 10. Epic dan Functional Requirement

### `EPIC RAD-01` — Aturan keselamatan dapat dikelola dan disahkan

**Tujuan.** Membuka kunci modul. Tanpa epic ini tidak satu pun pemeriksaan berjalan.
**Disposisi backend:** `EXTEND` untuk `MstRadModalitySafetyRule`; `MISSING / NEW` untuk
endpoint pengelolaannya.

> **`FR-RAD-001` — Aturan hanya berlaku setelah disahkan**
>
> Aturan keselamatan berstatus `Draft` atau `PendingApproval` tidak ikut dinilai gerbang
> keselamatan.
>
> **Contoh:** Admin menyusun aturan "skrining kehamilan wajib untuk CT-Scan" dan
> menyimpannya sebagai draf. Radiografer lalu mencoba menyatakan sebuah study CT-Scan lolos
> keselamatan. Karena belum ada aturan `Active` untuk CT-Scan, permintaan ditolak dengan kode
> 409 dan pesan "Aturan keselamatan untuk modalitas ini belum ditetapkan".

> **`FR-RAD-002` — Pengesahan hanya oleh penanggung jawab klinis**
>
> Admin Radiologi tidak dapat mengesahkan aturan yang ia susun sendiri.
>
> **Contoh:** Admin Rina menyusun aturan lalu menekan Sahkan. Ditolak kode 403 dengan pesan
> "Hanya penanggung jawab klinis yang boleh mengesahkan aturan keselamatan". Status tetap
> `PendingApproval`.

> **`FR-RAD-003` — Pengesahan menaikkan versi tepat satu kali**
>
> **Contoh:** Aturan berversi 1 disahkan. Versinya menjadi 2, bukan 3. Study yang lolos setelah
> itu membekukan angka 2 pada kolom `SafetyRuleVersionAtClearance`.

> **`FR-RAD-004` — Penolakan wajib beralasan**
>
> **Contoh:** Penanggung jawab klinis menolak pengajuan tanpa mengisi alasan. Ditolak kode 400
> dengan pesan "Alasan penolakan wajib diisi".

> **`FR-RAD-005` — Papan kesiapan alat**
>
> Sistem menampilkan daftar alat yang belum punya aturan keselamatan aktif.
>
> **Contoh:** Rumah sakit punya 6 alat, 4 di antaranya sudah punya aturan aktif. Papan
> menampilkan 2 alat sisanya dengan peringatan "Pemeriksaan dengan alat ini akan ditolak sampai
> aturan disahkan".

### `EPIC RAD-02` — Hasil bacaan radiolog

**Tujuan.** Menampung kesimpulan dokter radiolog sehingga sampai ke dokter pengirim.
**Disposisi backend:** `MISSING / NEW`.

> **`FR-RAD-010` — Bacaan hanya lahir atas citra yang layak**
>
> **Contoh:** Study CT-Scan Tn. B dinilai tidak layak. Dokter radiolog mencoba menulis draf
> bacaan atasnya. Ditolak kode 422 dengan pesan "Citra pemeriksaan ini dinyatakan tidak layak
> dibaca, sehingga bacaan tidak dapat dibuat".

> **`FR-RAD-011` — Radiolog boleh mengesahkan drafnya sendiri**
>
> **Contoh:** dr. Sinta, Sp.Rad menulis draf pukul 21.40 lalu mengesahkan dan merilisnya pukul
> 21.42. Berhasil. Riwayat mencatat dr. Sinta sebagai penulis **dan** sebagai pengesah, dalam
> dua baris terpisah.

> **`FR-RAD-012` — Bukan-radiolog tidak boleh mengesahkan drafnya sendiri**
>
> **Contoh:** dr. Rian, residen tahun ke-3, menulis draf lalu menekan Sahkan. Ditolak kode 403
> dengan pesan "Draf yang Anda tulis harus disahkan dokter radiolog". Draf tetap berstatus
> `Drafted` dan masuk antrian pengesahan.

> **`FR-RAD-013` — Peran penulis dibekukan pada draf**
>
> **Contoh:** dr. Rian menulis draf pada Januari sebagai residen. Pada Juli ia lulus menjadi
> Sp.Rad. Bila ia lalu mencoba mengesahkan draf Januari itu, permintaannya **tetap ditolak**,
> karena peran yang dinilai adalah peran saat draf ditulis.

> **`FR-RAD-014` — Satu study paling banyak satu bacaan**
>
> **Contoh:** Study yang sudah punya bacaan dicoba dibuatkan bacaan kedua. Ditolak kode 409
> dengan pesan "Pemeriksaan ini sudah memiliki bacaan. Gunakan koreksi bila ingin mengubahnya".

> **`FR-RAD-015` — Rilis hanya setelah pengesahan**
>
> **Contoh:** Bacaan berstatus `Drafted` dicoba dirilis. Ditolak kode 409 dengan pesan "Bacaan
> harus disahkan lebih dulu sebelum dirilis".

### `EPIC RAD-03` — Koreksi hasil berversi

**Tujuan.** Memperbaiki bacaan tanpa menghilangkan versi lama.
**Disposisi backend:** `MISSING / NEW`.

> **`FR-RAD-020` — Koreksi membuat versi baru, tidak menimpa**
>
> **Contoh:** Bacaan Tn. B dirilis pukul 08.00 sebagai versi 1. Pukul 09.00 dr. Sinta merilis
> koreksi. Setelahnya: versi 2 berlaku, versi 1 berstatus `Superseded`, dan **isi versi 1 tidak
> berubah satu huruf pun**. Keduanya tetap dapat dibaca.

> **`FR-RAD-021` — Alasan koreksi wajib**
>
> **Contoh:** Draf koreksi disimpan tanpa mengisi alasan. Ditolak kode 400 dengan pesan "Alasan
> koreksi wajib diisi".

> **`FR-RAD-022` — Versi yang sudah dirilis tidak dapat diubah**
>
> **Contoh:** Seseorang mencoba mengubah isi versi 1 yang sudah dirilis. Ditolak kode 403
> dengan pesan "Bacaan yang sudah dirilis tidak dapat diubah. Buat koreksi bila ada yang perlu
> diperbaiki".

> **`FR-RAD-023` — Aturan pengesahan berlaku sama pada koreksi**
>
> **Contoh:** dr. Rian menulis draf koreksi lalu mengesahkan sendiri. Ditolak kode 403, sama
> seperti pada draf pertama.

### `EPIC RAD-04` — Penyajian hasil ke rekam medis

**Tujuan.** Dokter pengirim membaca hasil tanpa berpindah modul.
**Disposisi backend:** `MISSING / NEW` untuk endpoint; `EXISTING / REUSE` untuk rekam medis.

> **`FR-RAD-030` — Rekam medis membaca langsung, tidak menyalin**
>
> **Contoh:** dr. Andi membaca hasil pukul 08.00. Pukul 09.00 hasilnya dikoreksi. Pukul 10.00
> dr. Andi membuka lagi dan **melihat versi koreksi**, tanpa ada langkah penyalinan apa pun di
> antaranya.

> **`FR-RAD-031` — Gangguan dibedakan dari kekosongan**
>
> **Contoh:** Modul Radiologi sedang tidak dapat dihubungi. Layar rekam medis menampilkan
> "Hasil radiologi sedang tidak dapat ditampilkan. Coba lagi beberapa saat" — **bukan** daftar
> kosong yang terbaca seolah pasien tidak punya pemeriksaan.

> **`FR-RAD-032` — Bacaan yang belum dirilis tidak tampil**
>
> **Contoh:** Draf bacaan Tn. B berstatus `Drafted`. Layar dokter pengirim **tidak**
> menampilkannya sama sekali.

### `EPIC RAD-05` — Frontend alur pemeriksaan

**Tujuan.** Membuat backend yang sudah ada dapat dipakai petugas.
**Disposisi backend:** `EXISTING / REUSE` — tidak ada perubahan backend.

> **`FR-RAD-040` — Butir keselamatan yang ditampilkan mengikuti alat**
>
> **Contoh:** Pasien menjalani MRI. Layar menampilkan butir "ada implan logam atau alat pacu
> jantung?" dan **tidak** menampilkan skrining kehamilan. Pasien yang sama besoknya menjalani
> CT-Scan; sekarang skrining kehamilan tampil dan butir implan logam tidak.

> **`FR-RAD-041` — Penanda pengulangan terlihat**
>
> **Contoh:** Daftar study satu pesanan menampilkan dua baris. Baris kedua diberi penanda
> "Pengulangan dari study ke-1" beserta sebabnya.

> **`FR-RAD-042` — Tombol Simpan tidak dapat ditekan dua kali**
>
> **Contoh:** Dokter menekan Simpan dua kali cepat saat membuat pesanan. Hanya satu pesanan
> tercatat.

### `EPIC RAD-06` — Frontend pengelolaan data induk

**Tujuan.** Admin dapat mendaftarkan alat dan menyusun aturan tanpa menyentuh database.
**Disposisi backend:** `MISSING / NEW` — endpoint dari `EPIC RAD-01`.

> **`FR-RAD-050` — Peringatan alat tanpa aturan aktif**
>
> **Contoh:** Layar pengelolaan menampilkan "Alat MRI belum punya aturan keselamatan aktif.
> Pemeriksaan dengan alat ini akan ditolak sampai aturan disahkan".

> **`FR-RAD-051` — Alat yang masih dipakai tidak dapat dinonaktifkan**
>
> **Contoh:** Admin mencoba menonaktifkan CT-Scan yang masih punya aturan aktif. Ditolak kode
> 409 dengan pesan "Alat ini masih dipakai aturan keselamatan yang berlaku. Nonaktifkan
> aturannya lebih dulu".

### `EPIC RAD-07` — Daftar kerja petugas dan penanda cito

**Tujuan.** Petugas tahu apa yang harus dikerjakan hari itu, dan yang mendesak didahulukan.
**Disposisi backend:** `EXTEND` untuk `RadOrder`; `MISSING / NEW` untuk endpoint daftar kerja.

> **`FR-RAD-060` — Daftar kerja dikelompokkan per alat**
>
> **Contoh:** Radiografer Tono bertugas di ruang CT-Scan pada 10 September. Ia membuka daftar
> kerja CT-Scan dan melihat tujuh pemeriksaan. Pemeriksaan MRI dan USG hari itu **tidak** muncul
> di layarnya.

> **`FR-RAD-061` — Daftar kerja tidak menyimpan apa pun**
>
> Daftar kerja dihitung dari pesanan dan study yang sudah ada, bukan dari tabel tersendiri.
>
> **Contoh:** Sebuah pesanan CT-Scan dibatalkan. Daftar kerja CT-Scan yang dibuka sesaat
> kemudian **langsung** tidak memuatnya lagi, tanpa ada proses penyelarasan apa pun.

> **`FR-RAD-062` — Pesanan cito berada di urutan atas**
>
> **Contoh:** Ada empat pesanan CT-Scan rawat jalan dari pukul 08.00 sampai 10.00, dan satu
> pesanan cito dari IGD pukul 10.30. Daftar kerja menampilkan pesanan IGD di **urutan pertama**,
> mendahului keempat pesanan yang lebih tua.

> **`FR-RAD-063` — Penanda cito terlihat tanpa membuka rincian**
>
> **Contoh:** Baris pesanan cito diberi penanda yang terbaca langsung dari daftar. Petugas tidak
> perlu membuka satu per satu untuk tahu mana yang mendesak.

> **`FR-RAD-064` — Penanda cito tercatat pelakunya**
>
> **Contoh:** dr. Andi menandai pesanan cito pukul 02.15. Sistem menyimpan nama dr. Andi dan
> waktu 02.15 pada pesanan itu.

> **`FR-RAD-065` — Pemanggil lama tetap berjalan**
>
> **Contoh:** Modul Rawat Jalan membuat pesanan tanpa mengirim field penanda cito sama sekali.
> Pesanan tetap berhasil dibuat dengan penanda bernilai tidak-cito. Tidak ada pemanggil lama
> yang rusak karena penambahan ini.

---

## 11. Model Status yang Diusulkan

Rincian lengkapnya di [contracts/state-transition-matrix.md](contracts/state-transition-matrix.md).

| Objek | Status | Invariant utama |
|---|---|---|
| Pesanan | `Requested`, `Accepted`, `Scheduled`, `InProgress`, `Completed`, `OnHold`, `CancelRequested`, `Cancelled`, `Rejected` | Tidak ada kolom finansial. `Draft` ada tetapi tidak dipakai |
| Study | `Planned`, `PatientVerified`, `SafetyCleared`, `AcquisitionStarted`, `Acquired`, `QualityAccepted`, `OnHold`, `Aborted`, `QualityRejected`, `RepeatRequired`, `Cancelled` | Acquisition ditolak sebelum identitas dan keselamatan tuntas. Pengulangan tidak menimpa asal |
| Hasil bacaan | `Pending`, `Drafted`, `Validated`, `Released`, `AmendmentDrafted`, `AmendmentValidated`, `AmendmentReleased` | Versi rilis tidak pernah berubah. Pengesah wajib radiolog |
| Versi bacaan | `Drafted`, `Validated`, `Released`, `Superseded` | `Released` bersifat beku |
| Aturan keselamatan | `Draft`, `PendingApproval`, `Active`, `Inactive` | Hanya `Active` yang dinilai gerbang |

---

## 12. Sasaran Arsitektur

| Kategori | Isinya |
|---|---|
| **Dipakai ulang** | `RadOrder`, `RadStudy`, `RadStudySafetyCheck`, `RadAcquisitionConsumption`, `RadTransitionHistory`, `MstRadModality`, `MstRadSafetyRequirement`, `RadSafetyGateEvaluator`, kontrak Billing `BIL-INTEGRATION-0.4` |
| **Diperluas** | `MstRadModalitySafetyRule` — enam kolom siklus pengesahan |
| **Baru** | `RadReport`, `RadReportVersion`, empat enum, dua service, empat controller, seluruh frontend |
| **Tidak dibuat** | Salinan pasien, dokter, prosedur, atau dokumen rekam medis. Daftar lengkapnya di `02-backend-architecture.md` bagian 11 |

---

## 13. Sasaran Kemampuan API

Bagian dari [contracts/api-contract.md](contracts/api-contract.md), tidak melebihinya.

### Health Services / Radiology Management / Rad Report

Base URL: `api/v1/health-services/radiology-management/rad-reports`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
|---|---|---|---|---|---|---|---|
| `POST` | `/by-study/{radStudyId}/draft` | Menulis draf bacaan | `RadReport : Create` | `CreateRadReportDraftRequest` | `ApiResponse<RadReportDetailResponse>` | `EPIC RAD-02` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/validate` | Mengesahkan bacaan | `RadReport : Validate` | `RadReportValidateRequest` | `ApiResponse<RadReportDetailResponse>` | `EPIC RAD-02` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/release` | Merilis bacaan | `RadReport : Release` | — | `ApiResponse<RadReportDetailResponse>` | `EPIC RAD-02` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/amendments` | Menulis draf koreksi | `RadReport : Amend` | `CreateRadReportAmendmentRequest` | `ApiResponse<RadReportDetailResponse>` | `EPIC RAD-03` | **Rencana (belum tersedia)** |
| `GET` | `/by-encounter/{encounterId}` | Hasil satu kunjungan, dipakai rekam medis | `RadReport : Read` | — | `ApiResponse<List<RadReportListResponse>>` | `EPIC RAD-04` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/versions` | Seluruh versi bacaan | `RadReport : Read` | — | `ApiResponse<List<RadReportVersionResponse>>` | `EPIC RAD-03` | **Rencana (belum tersedia)** |

### Health Services / Radiology Management / Master Data / Rad Safety Rule

Base URL: `api/v1/health-services/radiology-management/master-data/rad-safety-rules`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
|---|---|---|---|---|---|---|---|
| `POST` | `/` | Menyusun draf aturan | `RadSafetyRule : Create` | `CreateRadSafetyRuleRequest` | `ApiResponse<RadSafetyRuleResponse>` | `EPIC RAD-01` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/submit` | Mengajukan pengesahan | `RadSafetyRule : Submit` | — | `ApiResponse<RadSafetyRuleResponse>` | `EPIC RAD-01` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/approve` | Mengesahkan; versi naik | `RadSafetyRule : Approve` | — | `ApiResponse<RadSafetyRuleResponse>` | `EPIC RAD-01` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/reject` | Menolak pengajuan | `RadSafetyRule : Reject` | `RadSafetyRuleRejectRequest` | `ApiResponse<RadSafetyRuleResponse>` | `EPIC RAD-01` | **Rencana (belum tersedia)** |
| `GET` | `/coverage` | Alat yang belum punya aturan aktif | `RadSafetyRule : Read` | — | `ApiResponse<List<RadModalityCoverageResponse>>` | `EPIC RAD-01` | **Rencana (belum tersedia)** |

Endpoint data induk alat dan butir keselamatan mengikuti pola CRUD standar; daftar lengkapnya
di kontrak API.

---

## 14. Matriks Kewenangan

String permission sama persis dengan
[contracts/permission-audit-matrix.md](contracts/permission-audit-matrix.md).

| Tindakan | String permission | Siapa |
|---|---|---|
| Menulis draf bacaan | `[AccessPermission("RadReport", "Create")]` | Radiolog, residen, radiografer, bantuan AI |
| Mengesahkan bacaan | `[AccessPermission("RadReport", "Validate")]` | **Dokter radiolog saja** |
| Merilis bacaan | `[AccessPermission("RadReport", "Release")]` | Dokter radiolog |
| Mengoreksi bacaan | `[AccessPermission("RadReport", "Amend")]` | Radiolog, residen, radiografer, bantuan AI |
| Membaca bacaan | `[AccessPermission("RadReport", "Read")]` | Dokter pengirim, Radiologi |
| Menyusun draf aturan | `[AccessPermission("RadSafetyRule", "Create")]` | Admin Radiologi |
| Mengesahkan aturan | `[AccessPermission("RadSafetyRule", "Approve")]` | **Penanggung jawab klinis saja** |
| Mengelola alat | `[AccessPermission("RadModality", "Create")]` dan seterusnya | Admin Radiologi |

**Dua pemisahan wewenang wajib dijaga saat menyusun peran:**

1. `RadReport : Validate` **tidak dengan sendirinya** membolehkan pengesahan sendiri. Service
   memeriksa `AuthorRoleSnapshot`.
2. `RadSafetyRule : Create` dan `RadSafetyRule : Approve` **wajib dipegang peran berbeda**.

---

## 15. Batas Integrasi dan Billing

| Yang **tidak boleh** dibuat sendiri modul Radiologi | Pemilik sebenarnya |
|---|---|
| Perhitungan tarif, tagihan, void, refund, pembayaran | Billing Management |
| Data pasien, kunjungan, dokter, prosedur | Modul masing-masing |
| Stok kontras, film, BHP | Pharmacy / Inventory |
| Penyimpanan berkas citra | Di luar scope; tidak diaktifkan |
| Salinan hasil bacaan di modul lain | Dilarang `RAD-DEC-006` |

**Aturan tagih yang mengikat:** satu fakta per study, terbit hanya saat citra dinyatakan layak.
Pesanan dibuat, diterima, dijadwalkan, dan **hasil bacaan dirilis** semuanya **bukan** pemicu
tagihan.

---

## 16. Guardrail Regulasi

| Kewajiban | Bagaimana MVP memenuhinya |
|---|---|
| Hasil pemeriksaan penunjang menjadi bagian rekam medis | Dibaca langsung dari modul Radiologi; tidak ada salinan yang berpotensi basi |
| Hasil klinis tidak boleh dihapus atau ditimpa | Koreksi selalu versi baru; tidak ada endpoint hapus |
| Perbuatan atas rekam medis dapat ditelusuri | Setiap versi menyimpan penulis, peran penulis, pengesah, waktu, dan alasan |
| Bacaan wajib disahkan tenaga yang berkompeten | Pengesah wajib dokter radiolog; aturan ditegakkan service, bukan hanya UI |
| Data medis tidak boleh bocor ke tempat yang aturan aksesnya berbeda | Kolom sensitif dilarang masuk log; daftarnya di kamus data |
| Keselamatan radiasi | Gerbang keselamatan bersifat fail-closed; versi aturan dibekukan pada setiap study |

---

## 17. Kebutuhan Non-Fungsional

| ID | Kebutuhan | Cara memenuhinya |
|---|---|---|
| `NFR-001` | Perubahan status bersifat satu kesatuan | Satu `SaveChanges` per tindakan; gagal berarti seluruhnya batal |
| `NFR-002` | Dua orang tidak boleh sama-sama berhasil mengubah data yang sama | Token konkurensi pada pesanan, study, bacaan, dan versi |
| `NFR-003` | Fakta ke Billing tidak boleh terkirim dua kali | Penanda `BillingFactSubmitted`, diset setelah pengiriman berhasil |
| `NFR-004` | Seluruh perbuatan dapat ditelusuri | `RadTransitionHistory` dan riwayat versi bacaan; keduanya hanya bertambah |
| `NFR-005` | Koreksi tidak menghilangkan riwayat | Versi lama menjadi `Superseded`, isinya tidak berubah |
| `NFR-006` | Waktu disimpan dalam UTC | Mengikuti pola service yang sudah ada |
| `NFR-007` | Identitas pelaku diambil dari sesi | Tidak menerima identitas pelaku dari kiriman |
| `NFR-008` | Data medis tidak masuk log | Kolom sensitif ditandai di kamus data |
| `NFR-009` | Penghapusan bersifat penandaan | Warisan `IdentityModel`; tidak ada baris yang benar-benar hilang |

---

## 18. Skenario UAT

Ditulis supaya penguji non-teknis dapat menjalankannya sendiri.

> **`UAT-01` — Satu pemeriksaan dari pesanan sampai hasil dibaca** (jalur berhasil,
> `EPIC RAD-01` sampai `RAD-05`)
>
> **Kondisi awal:** Pasien samaran "Pasien A" punya kunjungan aktif. Alat CT-Scan sudah punya
> aturan keselamatan berstatus aktif.
>
> **Langkah:** Dokter memesan CT-Scan. Petugas menerima dan menjadwalkan. Radiografer
> memverifikasi pasien, menjawab butir keselamatan, mengambil citra, menilai citra layak.
> Radiolog menulis, mengesahkan, dan merilis bacaan. Dokter membuka rekam medis.
>
> **Hasil yang diharapkan:** Dokter melihat kesimpulan bacaan. Billing menerima tepat satu
> fakta kelayakan tagih. Riwayat memuat seluruh perpindahan status beserta pelakunya.

> **`UAT-02` — Alat belum punya aturan keselamatan** (jalur gagal, `EPIC RAD-01`)
>
> **Kondisi awal:** Alat MRI belum punya satu pun aturan berstatus aktif.
>
> **Langkah:** Radiografer mencoba menyatakan sebuah study MRI lolos keselamatan.
>
> **Hasil yang diharapkan:** Ditolak dengan pesan "Aturan keselamatan untuk modalitas ini belum
> ditetapkan...". Study tetap berstatus `PatientVerified`. Papan kesiapan alat menampilkan MRI
> sebagai belum siap.

> **`UAT-03` — Admin mengesahkan aturannya sendiri** (jalur gagal, `EPIC RAD-01`)
>
> **Kondisi awal:** Admin Radiologi sudah menyusun draf aturan dan mengajukannya.
>
> **Langkah:** Admin yang sama menekan Sahkan.
>
> **Hasil yang diharapkan:** Ditolak dengan pesan "Hanya penanggung jawab klinis yang boleh
> mengesahkan aturan keselamatan". Status tetap menunggu persetujuan.

> **`UAT-04` — Radiolog mengesahkan bacaannya sendiri** (jalur berhasil, `EPIC RAD-02`)
>
> **Kondisi awal:** Seorang dokter radiolog bertugas sendirian.
>
> **Langkah:** Ia menulis draf bacaan, lalu mengesahkan dan merilisnya.
>
> **Hasil yang diharapkan:** Berhasil. Riwayat menampilkan dirinya sebagai penulis dan sebagai
> pengesah dalam dua baris terpisah.

> **`UAT-05` — Residen mengesahkan drafnya sendiri** (jalur gagal, `EPIC RAD-02`)
>
> **Kondisi awal:** Seorang residen sudah menulis draf bacaan.
>
> **Langkah:** Residen yang sama menekan Sahkan.
>
> **Hasil yang diharapkan:** Ditolak dengan pesan "Draf yang Anda tulis harus disahkan dokter
> radiolog". Draf tetap berstatus draf dan masuk antrian pengesahan.

> **`UAT-06` — Koreksi bacaan yang sudah dirilis** (jalur berhasil, `EPIC RAD-03`)
>
> **Kondisi awal:** Sebuah bacaan sudah dirilis sebagai versi 1.
>
> **Langkah:** Radiolog membuat koreksi dengan alasan "ada temuan tambahan pada lapangan paru
> kanan", lalu mengesahkan dan merilisnya.
>
> **Hasil yang diharapkan:** Versi 2 berlaku. **Versi 1 masih dapat dibuka dan isinya tidak
> berubah.** Alasan koreksi tampil bersama versi 2.

> **`UAT-07` — Mencoba mengubah bacaan yang sudah dirilis** (jalur gagal, `EPIC RAD-03`)
>
> **Kondisi awal:** Sebuah bacaan sudah dirilis.
>
> **Langkah:** Seseorang mencoba mengubah isinya langsung.
>
> **Hasil yang diharapkan:** Ditolak dengan pesan "Bacaan yang sudah dirilis tidak dapat
> diubah. Buat koreksi bila ada yang perlu diperbaiki".

> **`UAT-08` — Rekam medis saat Radiologi bermasalah** (jalur gagal, `EPIC RAD-04`)
>
> **Kondisi awal:** Modul Radiologi sengaja dibuat tidak dapat dihubungi.
>
> **Langkah:** Dokter membuka rekam medis pasien yang punya hasil radiologi.
>
> **Hasil yang diharapkan:** Layar menampilkan "Hasil radiologi sedang tidak dapat
> ditampilkan..." — **bukan** daftar kosong.

> **`UAT-09` — Koreksi langsung terlihat di rekam medis** (jalur berhasil, `EPIC RAD-04`)
>
> **Kondisi awal:** Dokter sudah membaca versi 1 sebuah bacaan.
>
> **Langkah:** Radiolog merilis koreksi. Dokter membuka rekam medis lagi.
>
> **Hasil yang diharapkan:** Dokter melihat versi koreksi, tanpa perlu tindakan apa pun dari
> siapa pun.

> **`UAT-10` — Butir keselamatan berbeda antar alat** (jalur berhasil, `EPIC RAD-05`)
>
> **Kondisi awal:** MRI mewajibkan butir implan logam; CT-Scan mewajibkan skrining kehamilan.
>
> **Langkah:** Radiografer membuka isian keselamatan untuk study MRI, lalu untuk study CT-Scan.
>
> **Hasil yang diharapkan:** Butir yang tampil berbeda sesuai alatnya.

> **`UAT-11` — Dua radiolog mengesahkan bersamaan** (jalur gagal, `EPIC RAD-02`)
>
> **Kondisi awal:** Sebuah draf bacaan menunggu pengesahan.
>
> **Langkah:** Dua radiolog menekan Sahkan pada waktu hampir bersamaan.
>
> **Hasil yang diharapkan:** Satu berhasil, satu ditolak dengan pesan "Data ini baru saja
> diubah petugas lain...". Hanya **satu** pengesahan tersimpan.

> **`UAT-13` — Pesanan cito didahulukan di daftar kerja** (jalur berhasil, `EPIC RAD-07`)
>
> **Kondisi awal:** Ada empat pesanan CT-Scan rawat jalan dari pagi, dan satu pesanan cito dari
> IGD yang dibuat paling akhir.
>
> **Langkah:** Radiografer membuka daftar kerja CT-Scan.
>
> **Hasil yang diharapkan:** Pesanan cito berada di urutan pertama dengan penanda yang terlihat
> tanpa membuka rincian. Empat pesanan lain menyusul menurut waktu pesanan.

> **`UAT-14` — Daftar kerja tanpa memilih alat** (jalur gagal, `EPIC RAD-07`)
>
> **Kondisi awal:** —
>
> **Langkah:** Membuka daftar kerja tanpa memilih alat.
>
> **Hasil yang diharapkan:** Ditolak dengan pesan bahwa alat wajib dipilih. Tidak ada daftar
> gabungan seluruh unit yang ditampilkan.

> **`UAT-12` — Menonaktifkan alat yang masih dipakai** (jalur gagal, `EPIC RAD-06`)
>
> **Kondisi awal:** CT-Scan masih punya aturan keselamatan aktif.
>
> **Langkah:** Admin mencoba menonaktifkan CT-Scan.
>
> **Hasil yang diharapkan:** Ditolak dengan pesan "Alat ini masih dipakai aturan keselamatan
> yang berlaku...".

---

## 19. Definition of Done

Setiap butir dijawab "ya" atau "belum", disertai buktinya.

| Butir | Bukti |
|---|---|
| Registry mencatat `Rad` berstatus `ACTIVE` beserta entri riwayatnya | `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Aturan keselamatan dapat disusun, diajukan, disahkan, dan ditolak lewat antarmuka | `UAT-03`, `FR-RAD-001` sampai `FR-RAD-004` |
| Setiap alat yang dipakai punya sekurang-kurangnya satu aturan aktif | Papan kesiapan alat, `FR-RAD-005` |
| Seluruh tabel master MVP sudah terisi | Rencana data master awal pada `02-backend-architecture.md` bagian 9 |
| Satu pasien dapat berjalan dari pesanan sampai hasil dibaca | `UAT-01` |
| Bacaan non-spesialis tidak dapat dirilis tanpa diperiksa spesialis | `UAT-05`, `FR-RAD-012`, `FR-RAD-013` |
| Bacaan yang sudah dirilis tidak dapat diubah, hanya dikoreksi berversi | `UAT-06`, `UAT-07` |
| Versi lama tetap dapat dibaca setelah koreksi | `UAT-06` |
| Rekam medis menampilkan versi berlaku tanpa penyalinan | `UAT-09` |
| Gangguan dibedakan dari kekosongan pada layar hasil | `UAT-08` |
| Fakta kelayakan tagih terbit tepat satu kali per study yang layak | Matriks test bagian 7 |
| Hasil bacaan dirilis **tidak** menerbitkan fakta tagih | Matriks test bagian 7 |
| Uji kontrak hak akses ada dan lulus | Matriks test bagian 8 |
| Kolom sensitif tidak muncul di log | Matriks test bagian 9 |
| Migration 1 tidak mematikan pemeriksaan yang sedang berjalan | Matriks test bagian 10 |
| Dua orang tidak dapat sama-sama berhasil mengubah data yang sama | `UAT-11` |

---

## 20. Urutan Pengiriman dan Pertanyaan Terbuka

### Gelombang pengiriman

| Gelombang | Isi | Epic | Syarat mulai |
|---|---|---|---|
| `MVP-0` | Registry dinaikkan; migration 1; siklus pengesahan aturan keselamatan; CRUD alat dan butir | `EPIC RAD-01` | Blueprint disetujui **dan** `RAD-DEC-007` dijalankan |
| `MVP-1` | Frontend pengelolaan data induk; pengisian data master awal | `EPIC RAD-06` | `MVP-0` selesai |
| `MVP-2` | Migration 2; hasil bacaan dan koreksi berversi | `EPIC RAD-02`, `EPIC RAD-03` | `MVP-0` selesai |
| `MVP-3` | Frontend alur pemeriksaan dan hasil bacaan | `EPIC RAD-05` | `MVP-1` dan `MVP-2` selesai |
| `MVP-4` | Penyajian hasil ke rekam medis; daftar kerja dan penanda cito | `EPIC RAD-04`, `EPIC RAD-07` | `MVP-2` selesai |
| `POST-MVP` | Pelewatan gerbang darurat; temuan kritis; pemantauan keterlambatan cito | — | Di luar cakupan rilis pertama |

**Catatan tentang `EPIC RAD-07`.** Migration 3 tidak bergantung pada migration 1 maupun 2,
sehingga bagian penanda cito **dapat dimajukan** ke gelombang mana pun bila dibutuhkan lebih
cepat. Daftar kerja itu sendiri tidak butuh migration sama sekali.

**Mengapa `EPIC RAD-01` mendahului `EPIC RAD-02`.** Walaupun hasil bacaan adalah alasan utama
MVP ini ada, tanpa aturan keselamatan yang dapat disahkan **tidak akan pernah ada citra layak
untuk dibaca**. Mendahulukan hasil bacaan akan menghasilkan kemampuan yang tidak dapat diuji
dengan data nyata.

Tidak ada gelombang yang memuat epic berstatus `OPEN DECISION`.

### Pertanyaan terbuka sebelum development lock

| Pertanyaan | Siapa yang menjawab | Dampak bila belum dijawab | Memblokir |
|---|---|---|:---:|
| ~~Kapan registry `Rad` dinaikkan menjadi `ACTIVE`?~~ **TERJAWAB 2026-09-10** — salinan backend dinaikkan atas persetujuan Muhammad Hamzah; salinan canonical sudah `ACTIVE` sejak 2026-09-09 | Pemegang registry | — | ~~Ya~~ **Tidak lagi** |
| Peran mana di Quilvian yang setara dengan dokter radiolog dan penanggung jawab klinis? (`DEC-RAD-004`) | Pemilik modul + Administrator | `EPIC RAD-01` dan `EPIC RAD-02` tidak dapat menegakkan aturan pengesahan | **Ya** |
| Isi awal aturan keselamatan disiapkan tim atau diketik admin? (`DEC-RAD-005`) | Penanggung jawab klinis | `MVP-1` dapat berjalan, tetapi modul belum dapat dipakai sampai datanya terisi | Tidak |
| Apakah tata kelola klinis menyetujui pelewatan gerbang darurat? (`DEC-RAD-001`) | Tata kelola klinis | Hanya memengaruhi `POST-MVP` | Tidak |
| Apa saja yang dihitung temuan kritis? (`DEC-RAD-002`) | Tata kelola klinis | Hanya memengaruhi `POST-MVP` | Tidak |
| Bagaimana bentuk daftar kerja petugas? (`DEC-RAD-003`) | Pemilik modul + kepala unit | Hanya memengaruhi `POST-MVP` | Tidak |

### Akibat pertanyaan pemblokir yang tersisa — diperbarui 2026-09-10

Satu dari dua pertanyaan pemblokir sudah terjawab. Registry `Rad` kini `ACTIVE` pada kedua
salinan, sehingga `QBE-MOD-002` tidak lagi menahan `RadReport` dan `RadReportVersion`.
Gelombang `MVP-0` dapat dimulai begitu pemetaan peran tersedia.

**Yang tersisa satu: pemetaan empat sebutan peran ke peran Quilvian (`DEC-RAD-004`).**

Dokumen ini **tetap berstatus `draft` dan belum boleh diteruskan ke `/plan-module-delivery`**
sampai pemetaan itu ada. Tanpa peta peran, aturan pengesahan hasil bacaan dan aturan pengesahan
aturan keselamatan tidak dapat ditegakkan — keduanya hanya menjadi tulisan di dokumen.

---

## 21. Riwayat Revisi

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 1 | 2026-09-09 | PRD ke MVP pertama. Enam epic, 24 functional requirement, 12 skenario UAT, 16 butir Definition of Done, lima gelombang pengiriman. Dua pertanyaan pemblokir dicatat. | `draft` |
| 3 | 2026-09-10 | Registry `Rad` naik menjadi `ACTIVE`, sehingga satu dari dua pertanyaan pemblokir terjawab. `MVP-0` tidak lagi tertahan `QBE-MOD-002`. Sisa pemblokir: pemetaan peran `DEC-RAD-004`. | `draft` |
| 2 | 2026-09-09 | `EPIC RAD-07` daftar kerja dan penanda cito ditambahkan setelah `DEC-RAD-003` ditutup. Enam functional requirement dan dua skenario UAT baru. Masuk gelombang `MVP-4`. Pemantauan keterlambatan cito pindah ke `POST-MVP`. | `draft` |
