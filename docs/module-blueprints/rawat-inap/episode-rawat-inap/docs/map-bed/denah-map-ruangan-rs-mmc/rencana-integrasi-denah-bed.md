# Rencana Integrasi Denah dengan Bed Management — RS MMC

**Tanggal:** 15 Oktober 2026 · **Status:** semua keputusan produk sudah dijawab pemilik pada 15 Oktober
2026 (K-01–K-08, P-1–P-5, dan tiga pertanyaan closure pass) dan dicatat sebagai `RWI-DEC-298` s.d.
`323` di [Decision Log](../../../00-interview-decisions.md) revision 50. Kedua pass wawancara
**TUNTAS** (bagian 11). Desain blueprint (manifest anak revision 12, kontrak `0.13.0`) disetujui pemilik
16 Oktober 2026 (`RWI-DEC-324`), bersama aturan keras frontend `RWI-DEC-325`. Roadmap delivery
`DNH-DEL-20261016-01` terbit pada hari yang sama (bagian 10.1).
**Basis source:** backend `MHamzah` @ `5f0c1412`, frontend `HamzahV2` @ `4d9bf608`.

**Lingkup.** Denah ruangan (`/health-services/inpatient-management/floor-plan`) dihubungkan
dengan data kamar dan tempat tidur (bed) yang nyata lewat API. Setelah itu denah menjadi
satu-satunya cara melihat, memilih, dan mengelola bed. Denah dipakai di Monitoring Bed, admisi,
transfer, dan master data. **Desain tampilan denah tidak diubah sama sekali.**

Dokumen ini melanjutkan [rencana-kelas-kamar.md](rencana-kelas-kamar.md), yang menetapkan kelas
setiap kamar pada denah.

## 1. Ringkasan

- Denah menjadi patokan bentuk bangunan RS MMC. Susunan datanya: **Lantai → Kamar → Bed**, dan
  setiap kamar diberi **wing**, yaitu posisinya di lantai menurut salah satu dari delapan arah mata
  angin, mis. Barat Laut atau Tenggara (`RWI-DEC-300`, `313`). Data master di database disusun
  persis mengikuti denah: 106 kamar dan 168 bed di Lantai 2, 3, 4, 5, 8, dan 9.
- Satu komponen denah dipakai di empat tempat:
  - Bed Management, tab Monitoring Bed;
  - admisi, langkah Pilih Bed;
  - transfer pasien;
  - master data bed.

  Tampilan kartu bed yang ada sekarang **dihapus total**.

- Alur memilih bed:
  1. buka denah;
  2. pilih lantai;
  3. klik kamar, lalu pop up memperlihatkan siapa yang menempati setiap bed dan bed mana yang
     kosong;
  4. pilih bed kosong, lalu bed itu dipesan atau didaftarkan.
- Enam status bed (Tersedia, Terisi, Dipesan, Menunggu Pembersihan, Dalam Pembersihan, Tidak
  Tersedia) **sudah diputuskan** pada `RWI-DEC-282` dan tidak berubah. Ada dua hal baru:
  - **status kamar**, yang dihitung dari status bed-bednya;
  - **tingkat ketergantungan pasien** (_High Dependency / Low Dependency_), yang melekat pada
    pasien selama dirawat, bukan pada bed.
- **Tidak ada lagi pemisahan jenis kelamin.** Tidak ada bed maupun kamar khusus laki-laki atau
  perempuan, semua kamar boleh ditempati campur, dan jenis kelamin tidak diperiksa maupun ditampilkan
  saat memilih bed (`RWI-DEC-315`).
- Di peta, kamar penuh tidak lagi bergaris merah tetapi tetap bisa diklik. Kamar yang masih dapat
  menerima pasien diberi **outline hijau**, sehingga petugas admisi langsung tahu kamar mana yang bisa
  dipilih (`RWI-DEC-303`, `316`).
- Kelas kamar memakai 12 kelas resmi dari pemilik dengan urutannya: ICU, Kelas 3, Kelas 2, Kelas 1,
  Kelas VIP, VIP Deluxe, VIP Superior, Grand Royal, Premium, Suite, ODC, Isolasi (`RWI-DEC-307`).
  Ditambah kelas khusus Perinatologi untuk Ruang Bayi (`RWI-DEC-311`). Kamar SVIP 318, 401, dan 418
  menjadi VIP Superior (`RWI-DEC-310`).
  **Amandemen RWI-DEC-326 (16 Oktober 2026):** Nama denah mengikuti kelas yang sudah ada di database `MstPatientClass` tanpa me-rename baris master DB (menjaga integritas 18.513 tarif `MstTariff` per kelas). Kelas `Premium` **ditiadakan**. Kamar Lantai 8 nomor 801–810 dialihkan ke kelas resmi **`VVIP`**, kamar 811 ke **`LUXURY`**, dan kamar 812–813 ke **`SUITE`**.
- Data rawat inap di database pengembangan pribadi `QuilvianNewDevHamzah` dibersihkan lengkap
  sebelum integrasi diuji. Sebelum itu database dicadangkan, dan eksekusinya meminta konfirmasi
  tersendiri (`RWI-DEC-305`).
- Semua keputusan produk sudah dijawab (bagian 11). Yang tersisa adalah fakta dari RS dan Billing
  yang tidak menahan desain.

## 2. Kondisi sekarang (hasil audit)

Bed Management sudah cukup lengkap. Yang paling banyak berubah adalah cara menampilkan dan susunan
datanya.

| Bagian               | Yang sudah ada                                                                                                                                                                  | Bukti                                                           | Dampak rencana                                                                                               |
| -------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------ |
| Master kamar         | `MstRoom`: unit layanan, kelas, kode, nama, jenis kamar, nomor, lantai (teks bebas `FloorName`), kapasitas, penanda laki-laki/perempuan/bayi/isolasi/intensif/ODC, boleh admisi | `Areas/HealthServices/MasterData/Models/MstRoom.cs`             | Ditambah lantai terstruktur, wing, dan kunci denah; penanda jenis kelamin dihapus (`RWI-DEC-315`)            |
| Master bed           | `MstBed`: kamar, kode, nama, nomor, `BedStatus` 8 nilai, penanda laki-laki/perempuan/bayi/isolasi/intensif/ODC, boleh dipesan                                                   | `.../Models/MstBed.cs`, `.../Enums/BedStatus.cs`                | Penanda jenis kelamin dihapus; kode bed dibakukan                                                            |
| Enam status bed      | Dihitung dari hunian, pesanan, kesiapan pembersihan, dan penutupan bed                                                                                                          | `InpBedManagementQueryService.cs:15-18, 117-128`; `RWI-DEC-282` | Tetap                                                                                                        |
| Pesanan bed          | Berlaku 120 menit, kedaluwarsanya dihitung server                                                                                                                               | `RWI-DEC-283`                                                   | Tetap                                                                                                        |
| Pembersihan          | Housekeeping mencatat mulai dan selesai, lalu perawat ruangan mengesahkan bed siap                                                                                              | `RWI-DEC-278`, `280`, `282`                                     | Tetap; dijalankan dari pop up kamar                                                                          |
| Kebutuhan isolasi    | Atribut episode `RequiresIsolation` beserta sumber, pencatat, dan waktunya; endpoint `PATCH episodes/{id}/isolation-requirement`                                                | `InpEpisode.cs:77-88`; `InpatientEpisodeController.cs:379`      | Pola yang sama dipakai untuk High/Low Dependency                                                             |
| Aturan jenis kelamin | Hanya penanda bed: `BED_GENDER_MISMATCH` dan `PATIENT_GENDER_UNKNOWN`. Larangan kamar campur menurut penghuni sudah dicabut sejak 11 September 2026 (`RWI-DEC-101`)             | `InpBedOccupancyService.cs:1703-1738`; `RWI-FACT-072`           | **Dihapus seluruhnya** (`RWI-DEC-315`)                                                                       |
| Aturan boks bayi     | Pasien bayi hanya ke boks bayi dan sebaliknya; rawat gabung di kamar ibu; bayi yang butuh perawatan intensif ke bed/kamar intensif                                              | `InpNewbornAdmissionRules.cs:62-73`; `RWI-RULE-014`             | Tetap; editor denah harus dapat menambah boks bayi                                                           |
| Hak akses bed        | Cakupan per unit layanan (`BedManagementOptions.UnitAssignments`)                                                                                                               | `InpBedAccessService.cs:17`                                     | Unit rawat inap yang diakui RS lewat pemetaan admin (`RWI-DEC-323`); wing terpisah dari unit (`RWI-DEC-300`) |
| Papan bed (API)      | `GET bed-occupancies/bed-board`: unit → kamar → bed, nama pasien, pesanan, perbandingan kelas                                                                                   | `InpatientBedOccupancyDtos.cs:157-266`                          | Ditambah data per lantai untuk denah                                                                         |
| Tampilan kartu       | `bed-monitoring-cards.jsx` (Monitoring Bed) dan `inpatient-bed-board.jsx` 1.261 baris (Pilih Bed admisi, transfer, detail episode, Workspace Keperawatan, admisi bayi)          | frontend                                                        | **Dihapus**, diganti denah                                                                                   |
| Denah                | Halaman data dummy: 106 kamar, kelas, legenda, pop up nama kamar                                                                                                                | `floor-plan/`, commit `65b222bb`                                | Menjadi komponen bersama yang membaca API                                                                    |
| Master data          | Halaman daftar dan form Master Room serta Master Bed. Seeder **tidak pernah** membuat kamar/bed (`RWI-DEC-048`)                                                                 | `InpatientMasterDataSeeder.cs:21-23`                            | Master Bed diganti editor denah; `RWI-DEC-048` diamandemen (`RWI-DEC-306`)                                   |
| Pemakai lain         | `MstRoom` juga dipakai Kamar Operasi (`RoomType.OperatingRoom`); `MstBed` hanya dipakai Rawat Inap                                                                              | `OperatingRoomSchedulingService.cs`                             | Halaman Master Room **tetap ada** untuk kamar di luar denah                                                  |

## 3. Struktur denah di database

### 3.1 Susunan dan contoh

```
Lantai 3 — unit menurut pemetaan admin, mis. "Rawat Inap Lantai 3"
├── Barat Laut  Kelas 1 330 (bed 330-A, 330-B); Kelas 2 325 … 329 (masing-masing 3 bed)
├── Timur Laut  Kelas 2 320, 321 (dua kamar), 322; Isolasi 319
├── Barat Daya  Kelas VIP 301 … 306
├── Selatan     Kelas VIP 307 … 313
└── Tenggara    Kelas VIP 314 … 317; VIP Superior 318
```

Wing adalah **posisi kamar** menurut delapan arah mata angin, bukan unit layanan (`RWI-DEC-300`,
`313`). Arah bawaan dihitung dari posisi kamar di denah dengan bagian atas denah sebagai Utara
(`RWI-DEC-314`, asumsi), dan admin dapat mengoreksinya. Unit layanan, yang menjadi dasar hak akses
perawat dan tarif kamar, adalah unit rawat inap yang diakui RS; kamar dipetakan ke unit oleh admin
(`RWI-DEC-323`).

### 3.2 Parameter baru dan yang berubah

| Parameter              | Arti                                                                     | Disimpan di                                                                                         | Contoh                | Diisi oleh                                                    |
| ---------------------- | ------------------------------------------------------------------------ | --------------------------------------------------------------------------------------------------- | --------------------- | ------------------------------------------------------------- |
| Lantai                 | Lantai tempat kamar berada; menentukan tab denah                         | `MstRoom.FloorCode` (**baru**); `FloorName` tetap sebagai label                                     | `3` / "Lantai 3"      | Importer denah                                                |
| Wing                   | Posisi kamar pada lantai menurut salah satu dari delapan arah mata angin | Kolom wing **baru** pada `MstRoom` (`RWI-DEC-313`)                                                  | Barat Laut            | Importer denah (arah bawaan `RWI-DEC-314`); dikoreksi admin   |
| Unit layanan           | Dasar hak akses perawat dan tarif kamar; unit rawat inap yang diakui RS  | `MstServiceUnit`, dirujuk `MstRoom.ServiceUnitId` (sudah ada)                                       | "Rawat Inap Lantai 3" | Pemetaan kamar ke unit oleh admin (`RWI-DEC-323`)             |
| Kunci denah            | Penghubung satu kamar di database dengan satu bentuk kamar di gambar     | `MstRoom.FloorPlanRoomKey` (**baru**, unik)                                                         | `lantai-3/k-329`      | Importer denah                                                |
| Nomor kamar            | Nomor fisik kamar                                                        | `MstRoom.RoomNumber` (sudah ada)                                                                    | `329`                 | Importer; mengikuti gambar                                    |
| Kode kamar             | Kode sistem unik; tidak dibaca petugas                                   | `MstRoom.RoomCode` (sudah ada, unik)                                                                | `RM-RSMMC-00114`      | Otomatis, pengalokasi nomor bersama (desain backend 15.6)     |
| Kelas kamar            | Kelas rawat yang berlaku                                                 | `MstRoom.PatientClassId` (sudah ada)                                                                | Kelas 2               | Admin lewat denah                                             |
| Nama kamar             | Format `<Kelas> <Nomor>`                                                 | `MstRoom.RoomName`, dibentuk otomatis                                                               | "Kelas 2 329"         | Otomatis                                                      |
| Kode bed               | Kode sistem otomatis (`RWI-DEC-317`)                                     | `MstBed.BedCode` (sudah ada, unik)                                                                  | `BD-RSMMC-00187`      | Otomatis saat bed dibuat                                      |
| Label bed              | `<Nomor kamar>-<huruf urut>` yang dilihat petugas                        | `MstBed.BedName` dan `MstBed.BedNumber` (sudah ada)                                                 | `329-C` / `C`         | Importer dan editor denah (`RWI-DEC-317`)                     |
| Jumlah bed             | Jumlah bed aktif di kamar                                                | Dihitung dari bed aktif saat dibaca; `MstRoom.Capacity` tidak diselaraskan otomatis (`RWI-DEC-318`) | 3                     | Otomatis                                                      |
| Tingkat ketergantungan | High atau Low Dependency pasien selama dirawat                           | `InpEpisode.CareDependencyLevel` (**baru**) beserta pencatat, waktu, dan riwayatnya                 | High                  | Perawat penanggung jawab; DPJP boleh mengubah (`RWI-DEC-302`) |

**Kode harus unik.** `MstRoom.RoomCode` dan `MstBed.BedCode` masing-masing unik di seluruh
database. Kode kamar dan kode bed sama-sama kode sistem (`RWI-DEC-317`; desain backend 15.6), sehingga
aturan khusus hanya diperlukan untuk nomor kamar dan label bed:

- **Nomor ganda "K. 321" di Lantai 3.** Gambar memuat dua kamar bernomor 321. Keduanya bernomor 321
  dengan kunci denah berbeda (`lantai-3/k-321a`, `lantai-3/k-321b`), dan label bed kamar kedua
  melanjutkan hurufnya (`321-D` s.d. `321-F`) supaya petugas tidak tertukar. Label bed dijaga unik per
  lantai. Nomor resminya ditanyakan ke RS (RWI-OQ-142).
- **Kamar tanpa nomor di Lantai 2.** Nomor kamar diisi kode ruang pendek yang sekaligus menjadi awalan
  label bed, mis. `ICU-CK` dengan label bed `ICU-CK-A`, dan `BAYI-1` dengan label boks `BAYI-1-A`.
  Nama ruangannya tetap, mis. "R. ICU CK".

Kolom **Gedung** belum dibuat karena denah ini hanya satu gedung. Kolom itu baru diperlukan bila RS
juga merawat inap pasien di gedung lain.

### 3.3 Menghubungkan kamar di database dengan gambar, dan bila kamar bertambah

- Bentuk kamar (garis dan koordinat) tetap tersimpan di kode denah, sehingga tampilannya tidak
  berubah. Database menyimpan kunci denah setiap kamar. Denah menggambar semua bentuk kamar lalu
  mengisinya dengan data dari API yang kuncinya sama.
- Kamar di database yang tidak punya kunci denah tidak muncul di peta. Kamar seperti itu dicantumkan
  di peringatan "Kamar tanpa posisi di denah" pada halaman master.
- **Ruang cadangan.** Ruang tanpa nomor yang sudah tergambar ikut diberi bentuk dan kunci. Contohnya
  dua ruang di sebelah K. 322 (Lantai 3), satu ruang di antara 420 dan 429 (Lantai 4), dan beberapa
  ruang di Lantai 5. Ruang cadangan tampil abu-abu hanya di editor master, lalu dapat diaktifkan
  menjadi kamar.
- Ruang yang sama sekali **tidak ada di gambar** baru bisa ditambahkan setelah developer menggambar
  bentuknya. Denah adalah gambar gedung, jadi bentuknya tidak dibuat dari layar.

**Contoh.** RS memutuskan ruang tanpa label di sebelah K. 322 dipakai menjadi kamar 324 Kelas 2
dengan 3 bed. Admin membuka Master Bed, memilih Lantai 3, lalu mengklik ruang abu-abu itu. Ia
mengisi nomor 324, memilih Kelas 2, menambah 3 bed, lalu menyimpan. Kamar "Kelas 2 324" langsung
muncul di Monitoring Bed dan di pilihan bed admisi.

### 3.4 Jumlah data dari denah

| Lantai    |   Kamar |     Bed | Rincian                                                                                                         |
| --------- | ------: | ------: | --------------------------------------------------------------------------------------------------------------- |
| 2         |       6 |      14 | ICU 4 kamar/6 bed (Ruang ICU 3, R. ICU CK 1, R. ICU VIP 1+1); Perinatologi 2/8 (Ruang Bayi)                     |
| 3         |      29 |      48 | Kelas 1 1 kamar/2 bed, Kelas 2 9/27 (321 dua kamar), Kelas VIP 17/17, VIP Superior 1/1 (318), Isolasi 1/1 (319) |
| 4         |      27 |      43 | Kelas 1 2/4, Kelas 2 7/21, Kelas VIP 16/16, VIP Superior 2/2 (401, 418)                                         |
| 5         |      11 |      30 | Kelas 3 2/12, Kelas 2 3/9, Kelas 1 6/9                                                                          |
| 8         |      13 |      13 | Premium 10, Suite 2, Grand Royal 1                                                                              |
| 9         |      20 |      20 | Kelas VIP 14, VIP Deluxe 3, VIP Superior 3                                                                      |
| **Total** | **106** | **168** |                                                                                                                 |

Jumlah bed per kamar mengikuti data dummy di [rencana-kelas-kamar.md](rencana-kelas-kamar.md) bagian 5,
dengan nama kelas menurut `RWI-DEC-308`, `310`, dan `311`. Setelah diimpor, angka ini dapat diubah
dari editor denah.

## 4. Perombakan penanda kamar dan bed

| Kolom                                      | Sekarang                                                               | Keputusan                                                                                                          | Alasan                                                                                                                                                                                                |
| ------------------------------------------ | ---------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `IsForMale`, `IsForFemale` (kamar dan bed) | Wajib diisi; dibaca `BED_GENDER_MISMATCH` dan `PATIENT_GENDER_UNKNOWN` | **Hapus** (`RWI-DEC-315`)                                                                                          | Tidak ada lagi bed atau kamar khusus laki-laki/perempuan                                                                                                                                              |
| `IsForNewborn` (kamar dan bed)             | Boks bayi                                                              | **Pertahankan**                                                                                                    | Aturan boks bayi tetap (`InpNewbornAdmissionRules.cs:62-73`); dipakai Ruang Bayi Lantai 2 dan boks rawat gabung di kamar ibu                                                                          |
| `IsIsolationRoom`, `IsIsolationBed`        | Aturan isolasi keras (A5–A6)                                           | **Pertahankan**, diisi otomatis dari kelas Isolasi                                                                 | Keputusan pengendalian infeksi `RWI-DEC-064`; dipakai kamar Isolasi 319                                                                                                                               |
| `IsIntensiveCare`, `IsIntensiveCareBed`    | Penanda ICU                                                            | **Pertahankan**, diisi otomatis dari kelas ICU                                                                     | General Consent memilih "Khusus (ICU/Isolasi)" dari penanda ini (`RWI-DEC-251`). Bayi yang butuh perawatan intensif hanya ke bed atau kamar intensif, sehingga penanda Ruang Bayi menunggu RWI-OQ-142 |
| `IsOdcRoom`, `IsOdcBed`                    | ODC (One Day Care)                                                     | **Pertahankan**, tidak dipakai denah                                                                               | ODC ada di formulir RS tetapi tidak ada di denah                                                                                                                                                      |
| `RoomType`                                 | 13 nilai                                                               | **Pertahankan**, diisi otomatis dari kelas                                                                         | Kelas 3 s.d. Suite → `InpatientRoom`; ICU → `IntensiveCareRoom`; Isolasi → `IsolationRoom`; Perinatologi → `BabyRoom`                                                                                 |
| `FloorName`, `LocationName`                | Teks bebas                                                             | `FloorName` menjadi label otomatis "Lantai 3"; `LocationName` berisi lantai dan arah, mis. "Lantai 3 · Barat Laut" | Supaya tidak ada dua sumber yang bisa berbeda                                                                                                                                                         |
| `Capacity`                                 | Diisi manual                                                           | **Tidak dipakai denah**; kapasitas di denah dihitung dari bed aktif (`RWI-DEC-318`)                                | Mengubah `Capacity` adalah perubahan berbahaya yang menuntut semua bed kamar kosong (`RWI-FACT-073`)                                                                                                  |
| `IsAvailableForAdmission`, `IsReservable`  | Penutup kamar/bed                                                      | **Pertahankan**                                                                                                    | Dipakai status Tidak Tersedia (`RWI-DEC-285`)                                                                                                                                                         |
| `BedStatus` (8 nilai)                      | Status master                                                          | **Pertahankan**                                                                                                    | `RWI-DEC-285` tidak mengubah enum. Available/Maintenance/Blocked/Inactive dipakai untuk menutup dan membuka bed                                                                                       |

**Akibat bagi aturan jenis kelamin (`RWI-DEC-315`).** Sejak 11 September 2026 (`RWI-DEC-101`),
aturan jenis kelamin hanya membaca penanda bed; larangan kamar campur menurut penghuni
(`ROOM_GENDER_MIXED`) sudah dicabut. Versi awal dokumen ini keliru menyebut larangan itu masih
berlaku (`RWI-FACT-072`). Sisa aturannya kini dihapus seluruhnya:

| Aturan yang masih berjalan (`InpBedOccupancyService.cs:1703-1738`)                          | Sesudah                                                   |
| ------------------------------------------------------------------------------------------- | --------------------------------------------------------- |
| Tolak bila penanda bed tidak menerima jenis kelamin pasien (`BED_GENDER_MISMATCH`)          | **Dihapus**, penandanya ikut dihapus                      |
| Jenis kelamin belum tercatat hanya ke bed yang menerima keduanya (`PATIENT_GENDER_UNKNOWN`) | **Dihapus**                                               |
| Boks bayi dikecualikan dari kedua aturan di atas                                            | **Tidak diperlukan lagi**; aturan boks bayi sendiri tetap |
| Berlaku sama pada penempatan dan perpindahan                                                | **Dihapus** bersama aturannya                             |

Aturan isolasi (`RWI-RULE-012` bagian A) tidak berubah.

**Contoh.** Kamar Kelas 2 329 dihuni Tn. Budi di 329-A. Ny. Sari tetap dapat dipesankan bed 329-B
tanpa peringatan. Denah maupun pop up kamar tidak menampilkan jenis kelamin siapa pun.

## 5. Status bed, status kamar, dan tingkat ketergantungan

### 5.1 Enam status bed (tetap, `RWI-DEC-282`)

| Status               | Arti                                                                         | Boleh dipilih? |
| -------------------- | ---------------------------------------------------------------------------- | -------------- |
| Tersedia             | Bed siap dan dapat dipesan saat ini                                          | Ya             |
| Terisi               | Sedang ditempati pasien                                                      | Tidak          |
| Dipesan              | Sudah dipesan untuk pasien lain; berlaku 120 menit                           | Tidak          |
| Menunggu Pembersihan | Pasien sudah keluar, bed belum dibersihkan                                   | Tidak          |
| Dalam Pembersihan    | Housekeeping sedang membersihkan, termasuk tahap menunggu pengesahan perawat | Tidak          |
| Tidak Tersedia       | Ditutup dengan alasan (rusak, perbaikan, nonaktif)                           | Tidak          |

### 5.2 Perubahan status bed

| Dari                                    | Tindakan                     | Ke                                      | Pelaku                    | Syarat                                                      |
| --------------------------------------- | ---------------------------- | --------------------------------------- | ------------------------- | ----------------------------------------------------------- |
| Tersedia                                | Pesan bed saat admisi        | Dipesan                                 | Petugas admisi            | Lolos aturan kelayakan (bagian 6.4)                         |
| Dipesan                                 | Pasien ditempatkan           | Terisi                                  | Petugas berwenang         | Pesanan milik episode itu                                   |
| Dipesan                                 | Batal atau kedaluwarsa       | Tersedia                                | Petugas atau sistem       | Tanpa pembersihan, karena bed belum dipakai (`RWI-DEC-281`) |
| Tersedia                                | Transfer masuk               | Terisi                                  | Petugas transfer          | Lolos aturan dan kategori kelas benar                       |
| Terisi                                  | Pasien pulang atau pindah    | Menunggu Pembersihan                    | Sistem                    | Otomatis (`RWI-DEC-281`)                                    |
| Menunggu Pembersihan                    | Mulai pembersihan            | Dalam Pembersihan                       | Housekeeping              | —                                                           |
| Dalam Pembersihan                       | Selesai, menunggu pengesahan | Dalam Pembersihan (menunggu verifikasi) | Housekeeping              | —                                                           |
| Dalam Pembersihan (menunggu verifikasi) | Sahkan siap                  | Tersedia                                | Perawat ruangan berwenang | —                                                           |
| Dalam Pembersihan (menunggu verifikasi) | Nyatakan belum siap          | Menunggu Pembersihan                    | Perawat ruangan           | Alasan wajib                                                |
| Tersedia / Menunggu / Dalam Pembersihan | Tutup bed                    | Tidak Tersedia                          | Admin MasterData          | Alasan wajib; bed Terisi atau Dipesan tidak boleh ditutup   |
| Tidak Tersedia                          | Buka bed                     | Mengikuti kesiapannya                   | Admin MasterData          | Membuka tidak otomatis berarti siap                         |

**Contoh perjalanan bed 329-C.** Pukul 08:00 bed Tersedia. Pukul 08:10 petugas admisi memesannya
untuk Tn. Andi, sehingga statusnya Dipesan sampai 10:10. Pukul 09:00 Tn. Andi tiba dan
ditempatkan, sehingga statusnya Terisi. Tiga hari kemudian Tn. Andi pulang, dan bed menjadi Menunggu
Pembersihan. Housekeeping mulai bekerja (Dalam Pembersihan), mencatat selesai (menunggu
verifikasi), lalu perawat ruangan mengesahkan. Bed kembali Tersedia.

### 5.3 Status kamar (`RWI-DEC-303`, `316`)

Status kamar **tidak disimpan**. Status itu dihitung dari bed-bed di kamar tersebut, dan aturan yang
paling atas yang berlaku:

| Urutan | Status kamar   | Syarat                                                                      |
| -----: | -------------- | --------------------------------------------------------------------------- |
|      1 | Tidak Tersedia | Kamar ditutup, atau semua bed aktif berstatus Tidak Tersedia                |
|      2 | Tersedia       | Ada sekurang-kurangnya satu bed Tersedia                                    |
|      3 | Persiapan      | Tidak ada bed Tersedia, tetapi ada bed yang Menunggu atau Dalam Pembersihan |
|      4 | Penuh          | Semua bed yang dapat dipakai berstatus Terisi atau Dipesan                  |

**Di peta.** Badge "kapasitas/terisi" tetap tampil:

- **Kapasitas** = bed yang dapat dipakai, yaitu tanpa bed Tidak Tersedia.
- **Terisi** = bed Terisi ditambah bed Dipesan.

Garis merah "Kamar penuh" **dihapus** atas permintaan pemilik:

- Kamar **Penuh** tidak diberi warna merah maupun outline, tetapi tetap dapat diklik untuk melihat
  penghuninya.
- **Outline hijau** menandai kamar yang bisa dipilih (`RWI-DEC-316`):
  - di Monitoring Bed, pada kamar yang punya sekurang-kurangnya satu bed Tersedia;
  - pada admisi dan transfer, hanya pada kamar yang punya bed Tersedia **yang boleh ditempati
    pasien itu** menurut aturan isolasi dan boks bayi. Jenis kelamin tidak dipakai.
- Status Persiapan dan Tidak Tersedia tidak diberi outline. Keduanya tampil di judul pop up kamar.
- Apakah boks bayi rawat gabung di kamar biasa ikut dihitung pada badge dan outline masih ditanyakan
  (RWI-OQ-144).

| Isi kamar 3 bed                      | Status kamar   | Badge | Outline hijau (Monitoring) |
| ------------------------------------ | -------------- | ----- | -------------------------- |
| Terisi, Terisi, Tersedia             | Tersedia       | `3/2` | Ya                         |
| Terisi, Dipesan, Terisi              | Penuh          | `3/3` | Tidak (tetap bisa diklik)  |
| Terisi, Terisi, Menunggu Pembersihan | Persiapan      | `3/2` | Tidak                      |
| Terisi, Terisi, Tidak Tersedia       | Penuh          | `2/2` | Tidak (tetap bisa diklik)  |
| Tidak Tersedia ×3                    | Tidak Tersedia | `0/0` | Tidak                      |

**Contoh admisi.** Petugas mengadmisi pasien yang membutuhkan isolasi. Di Lantai 3 hanya kamar
Isolasi 319 yang ber-outline hijau, itu pun bila bednya Tersedia. Untuk pasien tanpa kebutuhan
isolasi, kamar Isolasi 319 justru tidak ber-outline.

### 5.4 Tingkat ketergantungan pasien — High / Low Dependency (`RWI-DEC-302`)

**Arti.** _High Dependency_ (ketergantungan tinggi) adalah pasien yang membutuhkan pengamatan dan
bantuan perawat lebih ketat dibanding pasien bangsal biasa, tetapi tidak membutuhkan ICU. _Low
Dependency_ (ketergantungan rendah) adalah pasien yang mandiri atau hanya sebagian dibantu.
Kriteria klinisnya ditetapkan RS lewat SOP keperawatan. Sistem mencatat pilihan dan riwayatnya,
tetapi tidak menilai sendiri.

**Mengapa melekat pada pasien, bukan pada bed.** Klien menyatakan bahwa dalam satu kamar Kelas 2,
High atau Low Dependency ditentukan oleh pasiennya. Hal ini sejalan dengan praktik yang menilai
tingkat perawatan menurut kebutuhan pasien di mana pun ia dirawat, bukan menurut jenis ruangannya.
Karena itu kamar dan bed **tidak diberi penanda** HD/LD.

**Pencatatan** mengikuti pola kebutuhan isolasi yang sudah ada:

- Nilai disimpan pada episode rawat inap: High, Low, atau _belum dinilai_.
- Setiap perubahan dicatat dengan pelaku, waktu, sumber, dan catatan. Sumbernya bisa catatan awal
  admisi, penilaian perawat, atau keputusan DPJP.
- Riwayat lama tidak ditimpa.

**Ringkasan di kamar.** Kamar berlabel High Dependency bila ada **satu saja** penghuninya yang HD.
Label ini tampil di judul pop up beserta hitungannya, dan di setiap baris bed.

**Contoh — Kelas 2 329.**

| Bed   | Status   | Penghuni    | Ketergantungan |
| ----- | -------- | ----------- | -------------- |
| 329-A | Terisi   | Tn. Budi S. | High           |
| 329-B | Terisi   | Ny. Sari W. | Low            |
| 329-C | Tersedia | —           | —              |

Status kamar **Tersedia**, ketergantungan kamar **High Dependency (1 HD · 1 LD)**. Bed 329-C boleh
dipesan pasien mana pun, baik HD maupun LD, karena HD/LD tidak membatasi penempatan.

## 6. Alur memilih bed

### 6.1 Admisi — langkah Pilih Bed

| Unsur     | Isi                                                                                           |
| --------- | --------------------------------------------------------------------------------------------- |
| Tujuan    | Pasien baru mendapat bed yang kosong dan sesuai                                               |
| Pelaku    | Petugas admisi memesan bed; perawat atau petugas berwenang menempatkan pasien                 |
| Pemicu    | Langkah 5 "Pilih Bed" pada alur admisi (termasuk admisi bayi)                                 |
| Prasyarat | Episode Draft dari langkah 1–4; kebutuhan isolasi; pasien bayi atau bukan; kelas dan penjamin |

Langkah utama:

1. Denah terbuka. Lantai pertama yang tampil adalah lantai dengan bed Tersedia terbanyak, atau
   lantai yang terakhir dipakai petugas.
2. Petugas memilih tab lantai.
3. Petugas mengklik kamar. Pop up menampilkan setiap bed dengan statusnya, penghuninya (nama,
   No. RM, ketergantungan) sesuai hak akses, dan sisa waktu pesanan.
4. Bed Tersedia yang lolos aturan menampilkan tombol **Pilih bed ini**. Bed yang tidak lolos tetap
   terlihat beserta alasannya.
5. Petugas memilih bed lalu menekan konfirmasi. Sistem memesan bed itu selama 120 menit.
6. Pop up tertutup. Langkah admisi menampilkan "Kelas 2 329 · bed 329-C", dan kelas admisi mengikuti
   kelas kamar (`FE-RWI-243`).

**Perubahan status:** Tersedia → Dipesan saat dipilih → Terisi saat pasien ditempatkan.

**Jalur tidak normal:**

- Bed sudah dipesan petugas lain lebih dulu. Server menolak (kode 409), pop up memuat ulang status,
  lalu petugas memilih bed lain.
- Pesanan kedaluwarsa. Bed kembali Tersedia dan langkah Pilih Bed meminta pemilihan ulang.
- Koneksi gagal. Sistem memeriksa ulang hasilnya sebelum petugas mengulang, supaya tidak terjadi
  pesanan ganda (`RWI-DEC-290`).
- Tidak ada bed Tersedia di lantai itu. Pop up atau judul lantai menyarankan memeriksa lantai lain.

**Hasil akhir:** bed berstatus Dipesan atas episode pasien, lalu Terisi setelah pasien tiba.

### 6.2 Transfer pasien

Transfer dibuka dari tab Bed Transfer, detail episode, atau Workspace Keperawatan. Lokasi asal
terisi otomatis. Petugas memilih tujuan lewat denah dengan langkah yang sama seperti bagian 6.1,
lalu memilih kategori Down Grade, Up Grade, atau Same Grade. Kategori itu dipilih manual, dan
sistem menolak pilihan yang tidak sesuai perbandingan kelas (`RWI-DEC-275`, `276`), kecuali bila
kelas asal atau tujuan adalah kelas khusus (`RWI-DEC-312`, bagian 8.1). Petugas juga mengisi alasan
dan menyimpan. Transfer tersimpan utuh dalam satu langkah (`RWI-DEC-284`). Bed asal
menjadi Menunggu Pembersihan, dan bed tujuan menjadi Terisi.

### 6.3 Pop up kamar

Pop up memakai kartu yang sama dengan pop up denah sekarang: judul nama kamar, garis aksen berwarna
kelas, dan baris rincian. Daftar bed ditambahkan di bawahnya. Peta di belakangnya tidak berubah.
Jenis kelamin tidak ditampilkan (`RWI-DEC-315`).

```
┌────────────────────────────────────────────────────────────────────┐
│                                ━━━━                             ✕  │
│                            Kelas 2 329                             │
│                       Lantai 3 · Barat Laut                        │
│        ● Kelas 2 | Tersedia | High Dependency (1 HD · 1 LD)        │
│ ────────────────────────────────────────────────────────────────── │
│ 329-A  ● Terisi    Tn. Budi S. · RM 00-12-34 · High Dependency     │
│ 329-B  ● Terisi    Ny. Sari W. · RM 00-56-78 · Low Dependency      │
│ 329-C  ● Tersedia                                [ Pilih bed ini ] │
└────────────────────────────────────────────────────────────────────┘
```

| Mode   | Dipakai di          | Isi tambahan                                                                                 |
| ------ | ------------------- | -------------------------------------------------------------------------------------------- |
| Lihat  | Monitoring Bed      | Aksi per bed sesuai hak: mulai/selesai pembersihan, sahkan siap, tutup/buka bed, riwayat bed |
| Pilih  | Admisi dan transfer | Tombol **Pilih bed ini** pada bed yang lolos aturan; alasan pada bed yang tidak lolos        |
| Master | Master Bed          | Ubah kelas, tambah/kurangi bed, tutup/buka bed, aktifkan ruang cadangan                      |

Warna titik status bed memakai warna legenda kartu yang sekarang, supaya artinya tetap sama.

### 6.4 Aturan yang diperiksa saat memilih bed

| Aturan                                                                                                                                | Sumber                                              | Yang tampil di pop up                        |
| ------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------- | -------------------------------------------- |
| Pasien butuh isolasi hanya ke bed isolasi, dan sebaliknya                                                                             | `RWI-RULE-012` A5–A6                                | "Bed isolasi" / "Pasien membutuhkan isolasi" |
| Pasien bayi hanya ke boks bayi; pasien bukan bayi tidak ke boks bayi                                                                  | `InpNewbornAdmissionRules.cs:62-65`                 | "Tempat ini khusus pasien bayi"              |
| Saat admisi, bayi rawat gabung hanya ke boks di kamar ibunya, dan bayi yang butuh perawatan intensif hanya ke bed atau kamar intensif | `RWI-RULE-014`; `InpNewbornAdmissionRules.cs:68-73` | Alasan penolakan per boks                    |
| Kategori transfer harus sesuai perbandingan kelas biasa; kelas khusus (ICU, ODC, Isolasi, Perinatologi) dipilih bebas                 | `RWI-DEC-275`–`277`, `291`, `312`                   | Kategori yang diizinkan per bed              |
| Identitas penghuni hanya untuk yang berhak; Housekeeping tidak melihat identitas                                                      | `RWI-DEC-287`                                       | Nama disamarkan, mis. "B\*\*\* S."           |
| High/Low Dependency **tidak** membatasi penempatan; hanya ditampilkan                                                                 | `RWI-DEC-302`                                       | Label HD/LD per bed dan ringkasan kamar      |
| Jenis kelamin tidak diperiksa dan tidak ditampilkan                                                                                   | `RWI-DEC-315`                                       | —                                            |

## 7. Master data bed lewat denah

| Unsur     | Isi                                                                                  |
| --------- | ------------------------------------------------------------------------------------ |
| Tujuan    | Kamar dan bed di database selalu sama dengan keadaan fisik gedung                    |
| Pelaku    | Admin MasterData (seluruh tim MasterData, `RWI-DEC-193`)                             |
| Pemicu    | Kamar berubah kelas, bed bertambah/berkurang, bed rusak, atau ruang cadangan dipakai |
| Prasyarat | Data master denah sudah diimpor (bagian 8)                                           |

Langkah utama:

1. Admin membuka **Master Bed**. Yang tampil langsung denah dengan desain yang sama.
2. Admin memilih lantai lalu mengklik kamar. Pop up mode master terbuka.
3. Admin dapat melakukan:
   - **ubah kelas**;
   - **tambah bed** dengan label berikutnya, mis. 329-D; kode sistemnya dibuat otomatis (`RWI-DEC-317`);
   - **tambah boks bayi** untuk rawat gabung, juga dengan label berikutnya (`RWI-RULE-014`);
   - **kurangi bed**;
   - **tutup atau buka bed**, dengan alasan;
   - **ubah arah wing kamar**;
   - **aktifkan ruang cadangan** dengan nomor dan kelasnya.
4. Admin menyimpan. Nama kamar dan badge di semua denah ikut berubah. Bed baru dan bed kamar yang
   kelasnya diubah tampil Tidak Tersedia sampai disahkan perawat (`RWI-AC-455`).

Aturan:

| Tindakan                | Aturan                                                                                                                                                                       | Contoh penolakan                     |
| ----------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------ |
| Ubah kelas              | Hanya bila kamar tidak berpenghuni dan tidak ada pesanan (`RWI-DEC-304`). Sesudah berubah, bed kamar itu Tidak Tersedia sampai disahkan perawat (`RWI-AC-455`)               | "Kelas 2 329 masih dihuni 2 pasien"  |
| Ubah arah wing          | Salah satu dari delapan arah (`RWI-DEC-313`); tidak memengaruhi penghuni                                                                                                     | —                                    |
| Tambah bed              | Boleh pada kamar berpenghuni. Label huruf berikutnya dan kode sistem otomatis (`RWI-DEC-317`); badge dihitung ulang (`RWI-DEC-318`); bed baru Tidak Tersedia sampai disahkan | —                                    |
| Tambah boks bayi        | Bed bertanda boks bayi, hanya untuk pasien bayi, mengikuti kelas kamar ibu. Cara menghitungnya pada badge menunggu RWI-OQ-144                                                | —                                    |
| Kurangi bed             | Bed yang belum pernah dipakai dihapus; bed yang punya riwayat hunian hanya dinonaktifkan agar riwayat tetap ada                                                              | "329-A punya riwayat, dinonaktifkan" |
| Tutup bed               | Alasan wajib; bed Terisi/Dipesan ditolak (`RWI-DEC-285`)                                                                                                                     | "329-A sedang Terisi"                |
| Buka bed                | Bed kembali mengikuti kesiapannya, tidak otomatis Tersedia                                                                                                                   | —                                    |
| Aktifkan ruang cadangan | Nomor unik di lantai itu, kelas wajib, sekurang-kurangnya satu bed                                                                                                           | "Nomor 324 sudah dipakai"            |

Halaman **Master Room** (daftar tabel) tetap ada, karena `MstRoom` juga dipakai Kamar Operasi dan
ruangan lain di luar denah. Kamar yang punya kunci denah sebaiknya diubah dari editor denah.

## 8. Data master dari denah (impor)

### 8.1 Kelas rawat (`MstPatientClass`) — ditetapkan pemilik (`RWI-DEC-307`, `310`, `311`, diamendemen `RWI-DEC-326`)

Master kelas memakai 12 kelas existing yang sudah ada di database `MstPatientClass` (tanpa membuat baris baru `Premium` dan tanpa me-rename nama kelas di DB, menjaga keutuhan 18.513 tarif per kelas di `MstTariff`). Kamar Lantai 8 nomor 801–810 dialihkan ke kelas resmi `VVIP` (`CLS-147`). Nomor urut adalah `ClassLevel`.

| `ClassLevel` | Kelas (DB Existing)                  | Kamar denah yang memakainya                                        | ID Database (`MstPatientClass`) |
| -----------: | ------------------------------------ | ------------------------------------------------------------------ | ------------------------------- |
|            1 | ICU                                  | Ruang ICU, R. ICU CK, R. ICU VIP ×2 (Lantai 2); nama ruangan tetap | `c012ea27-6826-446e-b935-43169747e4cf` |
|            2 | KELAS III                            | 505, 506                                                           | `976b37f4-23aa-4f93-b171-15157a3c277b` |
|            3 | KELAS II                             | 320–322 (321 dua kamar), 325–329, 420–423, 425–427, 508–510        | `3e6f18fc-950c-4205-a05f-5f4be3eda9ea` |
|            4 | KELAS I                              | 330, 428, 429, 503, 504, 507, 520–522                              | `0b1990eb-c539-4221-aaf9-34350b98c9fd` |
|            5 | VIP                                  | 301–317, 402–417, 902–906, 908, 911–916, 918, 919                  | `ddd81b12-078a-4418-8e5c-4eb9c889288f` |
|            6 | VIP DELUXE                           | 901, 917, 920; 318, 401, 418, 907, 909, 910                       | `74b042b4-690d-458c-92a1-b57c247c7052` |
|            7 | VVIP                                 | 801–810 (dialihkan dari Premium per `RWI-DEC-326`)                 | `2dc62399-4451-49f1-80a7-31374636e78e` |
|            8 | LUXURY                               | 811                                                                | `a92788e4-b57d-432c-ba91-f6fa04991e1d` |
|            9 | SUITE                                | 812, 813                                                           | `89f52b6d-a209-499a-8be4-0d5631407101` |
|           10 | ODC                                  | Tidak ada di denah                                                 | `02f04b94-e978-49dc-b6a3-2d197573d4ab` |
|           11 | ISOLASI                              | 319, dengan nama "Isolasi 319"                                     | `35dba8df-9b3d-4500-b0a9-38d37fad1a95` |
|           12 | PERINATOLOGI (kelas khusus)          | R. Bayi ×2 (Lantai 2)                                              | `eaad1fd0-5237-4733-a6b2-e08ffc821907` |

**Catatan:** Kelas `Premium` ditiadakan/tidak ditambahkan (`RWI-DEC-326`). Kamar 801–810 memakai kelas `VVIP` (`CLS-147`).

**Perbandingan Down/Up/Same Grade (`RWI-DEC-312`).** Perbandingan otomatis hanya berlaku antara kelas
biasa, yaitu Kelas 3 sampai Suite. Bila kelas asal atau tujuan adalah kelas khusus — ICU, ODC,
Isolasi, atau Perinatologi — petugas memilih kategorinya sendiri tanpa ditolak sistem.

**Contoh:** Kelas 3 → ICU boleh dicatat Up Grade. Sebaliknya, Kelas 2 → Kelas 1 yang dicatat Down
Grade tetap ditolak.

Nama kamar di denah mengikuti nama kelas yang berlaku: "Kelas 2 329", "Kelas VIP 301", "VVIP 801",
"Isolasi 319".

**Kelas yang sudah ada (`RWI-DEC-322`, diamendemen `RWI-DEC-326`).** Berdasarkan keputusan pemilik `RWI-DEC-326`, nama kelas pada tabel `MstPatientClass` di database tidak diubah agar seluruh 18.513 relasi tarif `MstTariff` per kelas tetap terhubung tanpa perubahan data DB. Kelas denah langsung memetakan ke nama dan Id kelas database yang sudah ada. Perubahan Id atau urutan kelas aktif diterapkan bersamaan dengan konfigurasi urutan kelas (`RWI-DEC-319`), supaya transfer tidak terhenti.

### 8.2 Wing (arah mata angin) dan unit layanan

**Wing** (`RWI-DEC-313`) ditulis dengan salah satu dari delapan arah: Utara, Timur Laut, Timur,
Tenggara, Selatan, Barat Daya, Barat, dan Barat Laut. Arah bawaan dihitung importer dengan aturan
`RWI-DEC-314`:

- blok kamar satu lantai dibagi kisi 3 × 3, dengan bagian atas denah sebagai Utara;
- kamar di sel tengah memakai arah terdekat dari pusat blok.

| Lantai | Arah bawaan setiap kamar                                                                                                                            |
| ------ | --------------------------------------------------------------------------------------------------------------------------------------------------- |
| 2      | Barat: R. Bayi ×2 · Timur: Ruang ICU · Timur Laut: R. ICU VIP · Tenggara: R. ICU VIP, R. ICU CK                                                     |
| 3      | Barat Laut: 325–330 · Timur Laut: 319–322 (321 dua kamar) · Barat Daya: 301–306 · Selatan: 307–313 · Tenggara: 314–318                              |
| 4      | Barat Laut: 425–428 · Timur Laut: 420–423, 429 · Barat Daya: 401–406 · Selatan: 407–412 · Tenggara: 413–418                                         |
| 5      | Barat Laut: 507–510 · Barat Daya: 503–506 · Tenggara: 520–522                                                                                       |
| 8      | Barat Laut: 809–811 · Utara: 807, 808 · Timur Laut: 805, 806 · Barat: 812 · Barat Daya: 801, 802, 813 · Selatan: 803, 804                           |
| 9      | Barat Laut: 915–918 · Utara: 913, 914 · Timur Laut: 910–912 · Barat: 919 · Timur: 909 · Barat Daya: 901–903, 920 · Selatan: 904 · Tenggara: 905–908 |

Bila RS memberi arah utara gedung yang sebenarnya (RWI-OQ-142), admin cukup mengoreksi arah kamar
dari editor denah.

**Unit layanan** (`RWI-DEC-323`) adalah dasar hak akses perawat dan tarif kamar. Kamar dikaitkan ke
unit rawat inap yang diakui RS lewat pemetaan kamar ke unit yang disiapkan admin. Unit baru hanya
dibuat bila RS memang mengelola bangsal per lantai atau per nurse station; unit yang dibuat baru butuh
tarif kamar dari Billing (RWI-OQ-143) dan penugasan akses. Denah menyaring per lantai lewat kolom
lantai, bukan lewat unit.

**Contoh.** Bila RS punya satu unit "Rawat Inap Lantai 3", seluruh 29 kamar Lantai 3 dipetakan ke
unit itu. Bila Lantai 3 dikelola dua nurse station, admin memetakan kamar ke dua unit, dan denah
Lantai 3 tetap menampilkan semua kamarnya.

### 8.3 Cara impor

1. Daftar kamar diekspor dari data denah frontend: kunci denah, lantai, nomor, kelas, jumlah bed,
   dan wing.
2. Admin menyiapkan pemetaan kamar ke unit rawat inap (`RWI-DEC-323`). Kamar tanpa pemetaan ditolak
   importer dan dilaporkan.
3. Importer backend membuat atau memperbarui kamar dan bed menurut **kunci denah**, lewat jalur
   mutasi master yang sudah ada. Importer aman dijalankan berulang dan tidak pernah menghapus kamar
   yang punya riwayat. Setiap bed diberi kode sistem dan label seperti "329-A" (`RWI-DEC-317`).
4. Importer dijalankan di `QuilvianNewDevHamzah` setelah data rawat inap dibersihkan (bagian 9). Di
   sana bed hasil impor ditetapkan siap sebagai data uji (`RWI-DEC-321`). Hasilnya berupa ringkasan:
   dibuat, diperbarui, dilewati, dan ditolak.
5. Setelah impor, **database menjadi sumber kelas dan jumlah bed**. File data dummy okupansi
   dihapus, dan label "Kelas 2 329" di peta dibentuk dari data API. Hasilnya tetap sama dengan
   tampilan sekarang.
6. Di produksi, importer yang sama dijalankan sekali oleh admin berwenang setelah RS mengesahkan
   susunan kamar (`RWI-DEC-306`). Importer **tidak** menetapkan bed siap. Setiap bed tampil Tidak
   Tersedia sampai perawat berwenang mengesahkannya, dan pengesahan boleh dibagi per lantai
   (`RWI-DEC-321`).

**Contoh.** Di produksi, perawat Lantai 3 memeriksa dan mengesahkan 48 bed lantainya sebelum bangsal
dibuka. Bed 329-C baru dapat dipesan sesudah disahkan.

## 9. Pembersihan data rawat inap di `QuilvianNewDevHamzah`

**Batas aman.**

- Hanya database pengembangan pribadi `QuilvianNewDevHamzah`. Database tim tidak boleh disentuh.
- Kredensial tidak ditulis di dokumen ini maupun di skrip yang di-commit; koneksinya ada di
  `appsettings.Development.json`.
- Eksekusi hanya dijalankan setelah pemilik mengonfirmasi daftar tabel dan jumlah barisnya, sesuai
  larangan otomatisasi database di `CLAUDE.md` backend.

| Cakupan        | Isi                                                                                                                                                                                                                                                                                                                | Keputusan                                                                      |
| -------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------ |
| A — sempit     | Data bed operasional (penempatan, pesanan, upaya pembersihan, jejak siklus, tanda terima operasi) dan episode rawat inap beserta semua tabel `Inp*` turunannya (riwayat status, penugasan dokter/perawat, ringkasan pulang, clearance, dokumen admisi, tanda tangan, rujukan, surat deposit/selisih biaya, outbox) | —                                                                              |
| B — lengkap    | A ditambah data modul lain yang merujuk episode rawat inap: catatan klinis dan keperawatan, resep dan pemberian obat, order lab/radiologi, tagihan dan deposit rawat inap, kunjungan rawat inap, serta permintaan admisi dari IGD/Kamar Operasi                                                                    | **Ya** — tidak meninggalkan data yatim yang merujuk episode yang sudah dihapus |
| Tidak disentuh | Master pasien, master lain, data rawat jalan/IGD/Kamar Operasi yang tidak terkait episode rawat inap                                                                                                                                                                                                               | —                                                                              |

Langkah:

1. **Petakan relasi** dari katalog database (`information_schema`). Hasilnya daftar tabel dan
   urutan hapus dari anak ke induk. Langkah ini hanya membaca.
2. **Cadangkan** seluruh database dengan `pg_dump -Fc` ke folder lokal di luar repository.
3. **Hitung baris** setiap tabel (_dry run_), lalu tunjukkan ke pemilik.
4. **Eksekusi dalam satu transaksi.** Hapus dari anak ke induk, lalu kembalikan semua bed menjadi
   siap:
   - set `InpBedReadiness` setiap bed ke `Ready`. Baris ini jangan dihapus, karena bed tanpa baris
     kesiapan terbaca Tidak Tersedia (`InpBedManagementQueryService.cs:121-127`);
   - set `MstBed.BedStatus` ke `Available` untuk bed yang tercatat Occupied, Reserved, atau
     Cleaning.
5. **Verifikasi.** Hitung ulang hasilnya 0, semua bed tampil Tersedia, dan aplikasi berjalan normal.
6. **Hapus master kamar/bed uji lama** (mis. Anggrek, Melati, Mawar) yang tidak ada di denah. Kamar
   Operasi dan ruangan non-rawat-inap tidak dihapus. Lalu jalankan impor denah (bagian 8).
7. Catat hasilnya di dokumen ini.

### 9.1 Hasil Eksekusi Pembersihan (16 Oktober 2026)

Langkah D-DNH-2 telah dieksekusi dengan sukses penuh dalam satu transaksi atomik ter-commit setelah konfirmasi pemilik:

- **Cadangan (Backup):** Seluruh data rawat inap dan entitas terkait dicadangkan ke `scratch/backup_ranap_20261016.json` (1.476.469 bytes, 49 tabel/kueri).
- **Hasil Pembersihan:**
  - `InpEpisode`: 35 baris terhapus (tersisa 0).
  - `InpBedPlacement`: 27 baris terhapus (tersisa 0).
  - `InpBedReservation`: 21 baris terhapus (tersisa 0).
  - `InpAdmissionDocument`: 24 baris terhapus (tersisa 0).
  - `InpStatusHistory`: 71 baris terhapus (tersisa 0).
  - Modul Klinis (`Cli*`, `Trx*`, `Hmd*`, `Phm*`, `Lab*`, `Rad*`, `Bil*` ranap): seluruh baris terkait episode ranap terhapus (tersisa 0).
  - Kunjungan Rawat Inap non-emergency: 4 baris `RegPatientEncounter` dan antrean terkait terhapus bersih.
- **Pemulihan Kesiapan & Status Bed:**
  - `InpBedReadiness`: Seluruh 70 bed direset ke `State = 4` (`Ready`) dengan `VerifiedByUserId` terkonfirmasi dan `LastReleasedPlacementId = NULL`.
  - `MstBed`: Seluruh 70 bed direset ke `BedStatus = 1` (`Available`).
- **Integritas Master & Modul Lain:**
  - `MstPatient`: 35 pasien utuh tanpa perubahan.
  - `MstTariff`: 351.802 tarif utuh tanpa perubahan.
  - `MstRoom`: 69 kamar utuh (kamar operasi dan non-ranap tidak disentuh).

Database `QuilvianNewDevHamzah` kini dalam kondisi bersih sempurna dan siap untuk langkah impor denah **D-DNH-3**.

### 9.2 Hasil Eksekusi Impor Master Denah (D-DNH-3) — 16 Oktober 2026

Langkah operasional D-DNH-3 telah dieksekusi dengan sukses penuh pada database `QuilvianNewDevHamzah` (PostgreSQL `160.22.250.77:5432`) dengan bukti runtime terverifikasi:

1. **Pembersihan Kamar & Bed Uji Lama:**
   - 3 kunjungan IGD pada `RegPatientEncounter` dilepas referensinya (`RoomId = NULL`).
   - 70 baris kesiapan `InpBedReadiness` lama dibersihkan.
   - 70 bed lama pada `MstBed` dihapus bersih.
   - 22 kamar rawat inap uji coba lama (`RM-IPD-*`, `RM-ICU-*`, `RM-BABY-*`, dsb.) dihapus dari `MstRoom`.
   - 47 kamar non-rawat-inap (Kamar Operasi, Poliklinik, Radiologi, Laboratorium, MCU) tetap utuh.

2. **Eksekusi Impor Master Denah (`FloorPlanImportService.ImportAsync`):**
   - **Tahap 1 — Dry Run:** 100% PASS tanpa penolakan (Rooms: Created=106, Rejected=0; Beds: Created=168, Rejected=0).
   - **Tahap 2 — Impor Definitif (`DryRun = false`):**
     - Run ID: `a2ada068-7c15-4e8d-a3df-ca7ba31975a5`
     - Manifest Hash: `eeec408a713df29d50f369ea1a4ecc9f485060fde0c1dbcd5dcdb70e2627b9b0`
     - Kamar Terbuat (`MstRoom`): **106 kamar** (Lantai 2: 10, Lantai 3: 28, Lantai 4: 28, Lantai 5: 28, Lantai 8: 10, Lantai 9: 2).
     - Bed Terbuat (`MstBed`): **168 bed** (semua berstatus `Available = 1`).
     - Kesiapan Bed (`InpBedReadiness`): **168 bed langsung disahkan siap (`State = 4 / Ready`)** dengan `ReadyEvidenceReference = "RWI-DEC-321"` per konfigurasi `BedManagement:FloorPlanImport:MarkImportedBedsReady = true` (`appsettings.Development.json`).
   - **Tahap 3 — Uji Idempotensi:** Impor kedua dijalankan dengan manifest yang sama persis: Rooms: Unchanged=106, Created=0; Beds: Unchanged=168, Created=0. Tidak ada duplikasi atau perubahan sesuai `INV-DNH-10`.

3. **Verifikasi Langsung Katalog Database PostgreSQL (`QuilvianNewDevHamzah`):**
   - Total `MstRoom` berkunci denah (`FloorPlanRoomKey != null`): **106 kamar** (Lolos 100%).
   - Total `MstBed` aktif: **168 bed** (Lolos 100%).
   - Total `InpBedReadiness` berstatus `Ready`: **168 baris** (Lolos 100%).
   - Total `MstFloorPlanImportRun`: **2 rekaman audit run**.

## 10. Rencana pekerjaan

### 10.1 Fase dan urutan

| Fase | Isi                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              | Bergantung pada                                                        |
| ---- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------- |
| 0    | Keputusan pemilik. **Selesai:** K-01–K-08, P-1–P-5, dan closure pass sudah dijawab dan dicatat (`RWI-DEC-298` s.d. `323`). Setelah itu blueprint dan kontrak diperbarui lewat rantai skill perencanaan sampai task resmi `BE-RWI`/`FE-RWI` terbit di roadmap Desain blueprint draft sudah ditulis 15 Oktober 2026 (manifest anak revision 12, kontrak `0.13.0`); disetujui 16 Oktober 2026 (`RWI-DEC-324`); roadmap `DNH-DEL-20261016-01` terbit dengan task `BE-RWI-232` s.d. `240` dan `FE-RWI-246` s.d. `253` | —                                                                      |
| 1    | Pembersihan data rawat inap di DB Hamzah (bagian 9)                                                                                                                                                                                                                                                                                                                                                                                                                                                              | **Selesai 16 Oktober 2026** — dicadangkan penuh (`backup_ranap_20261016.json`), eksekusi atomik tuntas, 35 episode bersih, 70 bed kembali ke Available dan Ready (`RWI-DEC-305`, `RWI-AC-445`) |
| 2    | Backend P-BE-01–06                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               | Fase 0                                                                 |
| 3    | Impor master denah ke DB Hamzah                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  | **Selesai 16 Oktober 2026** — 106 kamar dan 168 bed berstatus Ready di DB Hamzah (`RWI-DEC-321`, `D-DNH-3`) |
| 4    | Frontend P-FE-01–09                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              | P-FE-01 bisa dimulai lebih dulu; sisanya menunggu API                  |
| 5    | Verifikasi ujung ke ujung dan UAT                                                                                                                                                                                                                                                                                                                                                                                                                                                                                | Fase 3 dan 4                                                           |

```text
Fase 0 Keputusan
├── Fase 1 Bersihkan data DB Hamzah
├── P-BE-01 Skema
│   ├── P-BE-02 Importer ──→ Fase 3 Impor master
│   ├── P-BE-03 API denah per lantai
│   ├── P-BE-04 API ketergantungan
│   ├── P-BE-05 API editor master
│   └── P-BE-06 Aturan kelayakan
└── P-FE-01 Komponen denah bersama
    └── P-FE-02 Data API (butuh P-BE-03 dan Fase 3)
        └── P-FE-03 Pop up kamar
            ├── P-FE-04 Monitoring Bed
            ├── P-FE-05 Admisi Pilih Bed
            ├── P-FE-06 Transfer
            ├── P-FE-07 Master Bed (butuh P-BE-05)
            └── P-FE-08 Input HD/LD (butuh P-BE-04)
                └── P-FE-09 Hapus kode kartu ──→ Fase 5 Verifikasi
```

ID `P-BE`/`P-FE` adalah nomor kerja sementara. Task ID resmi terbit 16 Oktober 2026 pada roadmap
`DNH-DEL-20261016-01` ([backend](../../roadmap/backend-roadmap-integrasi-denah.md),
[frontend](../../roadmap/frontend-roadmap-integrasi-denah.md),
[traceability](../../roadmap/requirement-traceability-integrasi-denah.md)):

| Nomor kerja     | Task resmi                                                                                                                                              |
| --------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------- |
| P-BE-01         | `BE-RWI-232` (lantai, wing, kunci, kode kamar), `BE-RWI-234` (HD/LD), `BE-RWI-240` (hapus jenis kelamin); kelas resmi lewat langkah operasional D-DNH-1 |
| P-BE-02         | `BE-RWI-237`                                                                                                                                            |
| P-BE-03         | `BE-RWI-238` (Monitoring), `BE-RWI-239` (Pilih Bed)                                                                                                     |
| P-BE-04         | `BE-RWI-234`                                                                                                                                            |
| P-BE-05         | `BE-RWI-235` (writer master), `BE-RWI-236` (susunan lantai, ruang cadangan)                                                                             |
| P-BE-06         | `BE-RWI-233` (kelas khusus), `BE-RWI-240` (jenis kelamin)                                                                                               |
| P-FE-01         | `FE-RWI-246`                                                                                                                                            |
| P-FE-02 s.d. 04 | `FE-RWI-247`                                                                                                                                            |
| P-FE-05         | `FE-RWI-248`                                                                                                                                            |
| P-FE-06         | `FE-RWI-249`                                                                                                                                            |
| P-FE-07         | `FE-RWI-250` (editor), `FE-RWI-251` (impor)                                                                                                             |
| P-FE-08         | `FE-RWI-252`                                                                                                                                            |
| P-FE-09         | `FE-RWI-253`                                                                                                                                            |

### 10.2 Backend

| ID      | Pekerjaan                                                                                                                                                                                                                                                                                                                                                                                                                                | Hasil                                              |
| ------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------- |
| P-BE-01 | Skema: `MstRoom.FloorCode`, `MstRoom.FloorPlanRoomKey` (unik), `InpEpisode.CareDependencyLevel` beserta pencatat/waktu/sumber dan riwayatnya; kolom wing delapan arah pada `MstRoom` (`RWI-DEC-313`); hapus `IsForMale`/`IsForFemale` (`RWI-DEC-315`); master kelas mengikuti 12 kelas resmi ditambah Perinatologi, dengan kelas lama setara diganti nama dan Id tetap (`RWI-DEC-307`, `311`, `322`). Migration dan pemutakhiran kontrak | Struktur data denah siap                           |
| P-BE-02 | Importer susunan denah: idempoten menurut kunci denah, memakai pemetaan kamar ke unit yang disiapkan admin (`RWI-DEC-323`), membuat kamar beserta lantai dan arah wing (`RWI-DEC-314`) serta bed berkode sistem dan berlabel (`RWI-DEC-317`), menetapkan bed siap hanya di dev/UAT (`RWI-DEC-321`), lalu melaporkan hasil                                                                                                                | 106 kamar dan 168 bed masuk database               |
| P-BE-03 | API denah per lantai: kamar dengan status kamar, kapasitas/terisi, arah wing, ringkasan HD/LD; bed dengan status, penghuni sesuai hak, pesanan, kelayakan untuk episode tertentu, dan aksi yang diizinkan                                                                                                                                                                                                                                | Satu panggilan per lantai                          |
| P-BE-04 | API ketergantungan pasien: catat/ubah HD/LD dengan izin baru `InpatientEpisode : SetCareDependency`, beserta riwayatnya                                                                                                                                                                                                                                                                                                                  | HD/LD tercatat dan dapat ditelusuri                |
| P-BE-05 | Editor master: ubah kelas hanya untuk kamar kosong (`RWI-DEC-304`), tambah bed atau boks bayi berkode otomatis, ubah arah wing, kurangi bed (hapus atau nonaktifkan), kapasitas dihitung dari bed aktif (`RWI-DEC-318`), aktifkan ruang cadangan. Sebisa mungkin memakai endpoint Master Room/Bed yang sudah ada                                                                                                                         | Master dapat dikelola dari denah                   |
| P-BE-06 | Aturan kelayakan: hapus `BED_GENDER_MISMATCH` dan `PATIENT_GENDER_UNKNOWN` beserta test-nya (`RWI-DEC-315`); kelas khusus dikecualikan dari perbandingan Down/Up Grade (`RWI-DEC-312`); aturan isolasi dan boks bayi tetap. HD/LD tidak membatasi penempatan (`RWI-DEC-302`)                                                                                                                                                             | Penempatan dan transfer konsisten dengan keputusan |

### 10.3 Frontend

| ID      | Pekerjaan                                                                                                                                                                                                                                                                                  | Hasil                                                              |
| ------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------ |
| P-FE-01 | Jadikan denah komponen bersama: komponen ke `src/components/features/health-services/inpatient-management/floor-plan/`, geometri ke `src/lib/constants/...`, CSS dipindah apa adanya. **Tampilan tidak berubah**, dibuktikan dengan perbandingan screenshot sebelum dan sesudah per lantai | Satu denah dipakai di semua layar                                  |
| P-FE-02 | Data dari API per lantai (hook + service `InstanceAxios`); data dummy kelas dan okupansi dihapus; label kelas memakai nama resmi dari master (`RWI-DEC-308`, `310`, `311`)                                                                                                                 | Peta menampilkan data nyata                                        |
| P-FE-03 | Pop up kamar: daftar bed, enam status, penghuni sesuai hak, ringkasan status kamar dan HD/LD, mode lihat/pilih/master                                                                                                                                                                      | Bagian 6.3                                                         |
| P-FE-04 | Tab Monitoring Bed memakai denah; aksi pembersihan dan tutup/buka dari pop up                                                                                                                                                                                                              | Kartu monitoring hilang                                            |
| P-FE-05 | Admisi langkah Pilih Bed (termasuk admisi bayi) memakai denah mode pilih                                                                                                                                                                                                                   | Bagian 6.1                                                         |
| P-FE-06 | Transfer (tab Bed Transfer, detail episode, Workspace Keperawatan) memakai denah untuk tujuan                                                                                                                                                                                              | Bagian 6.2                                                         |
| P-FE-07 | Master Bed menjadi editor denah; halaman daftar Master Bed diganti                                                                                                                                                                                                                         | Bagian 7                                                           |
| P-FE-08 | Input HD/LD di Workspace Keperawatan dan detail episode, beserta riwayatnya                                                                                                                                                                                                                | Bagian 5.4                                                         |
| P-FE-09 | Hapus kode kartu dan test lamanya (bagian 10.5), serta isian dan pesan jenis kelamin di `room-constants.jsx`, `room-utils.jsx`, `bed-constants.jsx`, `bed-utils.jsx`, dan `inpatient-placement-utils.jsx` (`RWI-DEC-315`)                                                                  | Tidak ada dua tampilan bed dan tidak ada sisa aturan jenis kelamin |

**Aturan keras pemilik (`RWI-DEC-325`, 16 Oktober 2026).** Untuk seluruh task frontend di atas,
references skill `build-module-frontend` dan base component tidak wajib; Bed Management fleksibel dan
mengutamakan tampilan yang sempurna dan bagus; tampilan floor plan yang sudah didesain rapi tidak boleh
diubah. Tampilan ini akan menjadi contoh untuk tampilan lain.

### 10.4 API

Endpoint berlabel **Rencana (belum tersedia)** belum ada di kode. Rincian final request, response, dan
kode galat ada di [API contract bagian 14](../../contracts/api-contract.md); tabel di bawah adalah ringkasannya.

#### Health Services / Inpatient Management / Bed Occupancy

Base URL: `api/v1/health-services/inpatient-management/bed-occupancies`

| Method  | Path                        | Kegunaan                                                                                                                                | Hak akses                          | Request                                                            | Response            | Keadaan                  |
| ------- | --------------------------- | --------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------- | ------------------------------------------------------------------ | ------------------- | ------------------------ |
| `GET`   | `/floor-plan`               | Data denah satu lantai untuk Pilih Bed admisi atau tujuan transfer: kamar, status kamar, bed, penghuni, dan kelayakan untuk episode itu | `InpatientBedOccupancy : Read`     | Query `floorCode`, `purpose` (`Admission`/`Transfer`), `episodeId` | `FloorPlanResponse` | Rencana (belum tersedia) |
| `POST`  | `/reservations`             | Memesan bed saat admisi                                                                                                                 | `InpatientBedOccupancy : Create`   | `ReserveBedRequest`                                                | Pesanan bed         | Sudah ada                |
| `PATCH` | `/reservations/{id}/cancel` | Membatalkan pesanan                                                                                                                     | `InpatientBedOccupancy : Update`   | Alasan                                                             | Pesanan bed         | Sudah ada                |
| `POST`  | `/placements`               | Menempatkan pasien                                                                                                                      | `InpatientBedOccupancy : Create`   | Penempatan                                                         | Penempatan          | Sudah ada                |
| `POST`  | `/placements/transfer`      | Memindahkan pasien                                                                                                                      | `InpatientBedOccupancy : Transfer` | Transfer                                                           | Penempatan baru     | Sudah ada                |

#### Health Services / Inpatient Management / Bed Management

Base URL: `api/v1/health-services/inpatient-management/bed-management`

| Method  | Path                                      | Kegunaan                                                                                                    | Hak akses                                   | Request           | Response            | Keadaan                  |
| ------- | ----------------------------------------- | ----------------------------------------------------------------------------------------------------------- | ------------------------------------------- | ----------------- | ------------------- | ------------------------ |
| `GET`   | `/floor-plan`                             | Data denah satu lantai untuk Monitoring Bed: kamar, status kamar, bed, penghuni sesuai hak, ringkasan HD/LD | `InpatientBedManagement : Read`             | Query `floorCode` | `FloorPlanResponse` | Rencana (belum tersedia) |
| `POST`  | `/beds/{bedId}/cleaning-attempts`         | Mulai pembersihan                                                                                           | `InpatientBedManagement : StartCleaning`    | Versi bed         | Status bed          | Sudah ada                |
| `PATCH` | `/cleaning-attempts/{attemptId}/complete` | Selesai pembersihan                                                                                         | `InpatientBedManagement : CompleteCleaning` | Versi bed         | Status bed          | Sudah ada                |
| `POST`  | `/beds/{bedId}/readiness-verifications`   | Sahkan siap atau belum siap                                                                                 | `InpatientBedManagement : VerifyReadiness`  | Hasil dan alasan  | Status bed          | Sudah ada                |

#### Health Services / Inpatient Management / Inpatient Episode

Base URL: `api/v1/health-services/inpatient-management/episodes`

| Method  | Path                            | Kegunaan                                   | Hak akses                                     | Request         | Response          | Keadaan                  |
| ------- | ------------------------------- | ------------------------------------------ | --------------------------------------------- | --------------- | ----------------- | ------------------------ |
| `PATCH` | `/{id}/care-dependency`         | Mencatat atau mengubah High/Low Dependency | `InpatientEpisode : SetCareDependency` (baru) | `level`, `note` | Ringkasan episode | Rencana (belum tersedia) |
| `GET`   | `/{id}/care-dependency-history` | Riwayat perubahan HD/LD                    | `InpatientEpisode : Read`                     | —               | Daftar riwayat    | Rencana (belum tersedia) |

#### Health Services / Master Data / Room

Base URL: `api/v1/health-services/master-data/rooms`

| Method | Path                        | Kegunaan                                                               | Hak akses                       | Request                                   | Response           | Keadaan                                     |
| ------ | --------------------------- | ---------------------------------------------------------------------- | ------------------------------- | ----------------------------------------- | ------------------ | ------------------------------------------- |
| `GET`  | `/floor-plan-layout`        | Kamar dan bed satu lantai untuk editor master, termasuk ruang cadangan | `Room : Read`                   | Query `floorCode`                         | Susunan lantai     | Rencana (belum tersedia)                    |
| `POST` | `/floor-plan-layout/rooms`  | Aktifkan ruang cadangan menjadi kamar beserta bed-nya                  | `Room : Create`                 | Nomor, kelas, unit, arah, jumlah bed/boks | Kamar dan bed baru | Rencana (belum tersedia)                    |
| `POST` | `/floor-plan-layout/import` | Periksa (dry run) atau jalankan impor susunan kamar dan bed dari denah | `Room : ImportFloorPlan` (baru) | Manifest denah + pemetaan unit            | Ringkasan impor    | Rencana (belum tersedia)                    |
| `PUT`  | `/{id}`                     | Ubah kamar (kelas, wing)                                               | `Room : Update`                 | Data kamar                                | Kamar              | Sudah ada; aturan `RWI-DEC-304` ditambahkan |

#### Health Services / Master Data / Bed

Base URL: `api/v1/health-services/master-data/beds`

| Method   | Path                 | Kegunaan                            | Hak akses      | Request           | Response    | Keadaan                              |
| -------- | -------------------- | ----------------------------------- | -------------- | ----------------- | ----------- | ------------------------------------ |
| `POST`   | `/`                  | Tambah bed                          | `Bed : Create` | Data bed          | Bed         | Sudah ada; kode otomatis ditambahkan |
| `PATCH`  | `/{id}/availability` | Tutup atau buka bed                 | `Bed : Update` | Status dan alasan | Bed         | Sudah ada                            |
| `DELETE` | `/{id}`              | Hapus bed yang belum pernah dipakai | `Bed : Delete` | —                 | Hasil hapus | Sudah ada                            |

Arti kode status bagi pengguna:

- **409** — bed baru saja diambil atau diubah petugas lain; layar memuat ulang lalu petugas memilih
  lagi.
- **422** — pilihan melanggar aturan (isolasi, boks bayi, kategori kelas, atau bed tidak siap); pesan
  menyebut aturannya.
- **403** — pengguna tidak punya hak untuk tindakan itu.

### 10.5 Kode kartu yang dihapus

| Berkas                                                                                                                                                  | Dipakai oleh                                                                                        | Pengganti                  |
| ------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------- | -------------------------- |
| `src/components/view/health-services/inpatient-management/bed-management/bed-monitoring-cards.jsx`                                                      | Tab Monitoring Bed                                                                                  | Denah mode lihat           |
| `src/components/features/health-services/inpatient-management/inpatient-bed-board.jsx` (1.261 baris) dan `src/style/.../inpatient-bed-board.module.css` | `inpatient-admission-bed-step.jsx`, `inpatient-bed-transfer.jsx`, `nursing-transfer-form-panel.jsx` | Denah mode pilih           |
| `src/lib/hooks/.../use-inpatient-bed-board.jsx`                                                                                                         | Papan kartu                                                                                         | Hook data denah per lantai |
| Bagian kartu di `src/style/.../bed-management.module.css`                                                                                               | Kartu monitoring                                                                                    | —                          |
| `src/components/view/health-services/master-data/bed/master-data-bed-view.jsx` (daftar)                                                                 | Master Bed                                                                                          | Editor denah               |
| Halaman dummy `floor-plan/_data/lantai-*.occupancy.js`                                                                                                  | Denah dummy                                                                                         | Data API                   |
| Test unit yang menguji komponen di atas                                                                                                                 | —                                                                                                   | Disesuaikan atau dihapus   |

### 10.6 Verifikasi

- Alur ujung ke ujung di DB Hamzah: admisi → pesan → tempatkan → catat HD/LD → transfer →
  Menunggu Pembersihan → mulai → selesai → sahkan → Tersedia.
- Master: tambah bed dan boks bayi, ubah kelas pada kamar kosong, tolak ubah kelas pada kamar
  berpenghuni, ubah arah wing, tutup/buka bed, aktifkan ruang cadangan.
- Aturan: kamar campur diterima tanpa peringatan, isolasi ditolak/diterima, boks bayi hanya untuk
  pasien bayi, kategori transfer salah antar kelas biasa ditolak (kelas khusus dipilih bebas), dan
  identitas penghuni disamarkan bagi yang tidak berhak.
- Tampilan: screenshot denah setiap lantai sebelum dan sesudah tidak berubah selain data. Pop up
  diuji dalam ketiga mode, dan layar 390 px tidak bergeser horizontal.
- Build, lint, dan unit test backend/frontend dibandingkan dengan garis dasar.

## 11. Keputusan pemilik — pass TUNTAS 15 Oktober 2026

### 11.1 Jawaban yang sudah dicatat

Rincian, batas, dan kriteria penerimaannya ada di
[Decision Log](../../../00-interview-decisions.md), bagian "Amendment Pass Integrasi Denah ↔ Bed
Management" dan "Closure Pass Integrasi Denah ↔ Bed Management", revision 50.

| No               | Pertanyaan                      | Jawaban pemilik                                                                                                                                                           | Decision ID                          |
| ---------------- | ------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------ |
| K-01             | Wing                            | Posisi kamar menurut arah mata angin; **bukan** unit layanan                                                                                                              | `RWI-DEC-300`                        |
| K-02             | Penanda laki-laki/perempuan     | A — penanda dihapus. Kemudian diperluas oleh peringatan keras (baris terakhir)                                                                                            | `RWI-DEC-301`, superseded oleh `315` |
| K-03             | High/Low Dependency             | A — perawat penanggung jawab mencatat, DPJP boleh mengubah, admisi catatan awal; tidak membatasi penempatan                                                               | `RWI-DEC-302`                        |
| K-04             | Status kamar dan badge          | A — empat status turunan, terisi = Terisi + Dipesan, kamar HD bila ada satu penghuni HD. **Ditambah:** kamar penuh tanpa merah dan tanpa outline tetapi tetap bisa diklik | `RWI-DEC-303`                        |
| K-05             | Ubah kelas kamar berpenghuni    | A — ditolak sampai kamar kosong dan tanpa pesanan                                                                                                                         | `RWI-DEC-304`                        |
| K-06             | Pembersihan DB Hamzah           | B — lengkap; master kamar/bed uji lama dihapus                                                                                                                            | `RWI-DEC-305`                        |
| K-07             | Sumber master kamar/bed         | A — denah resmi menjadi sumber; importer terkontrol                                                                                                                       | `RWI-DEC-306`                        |
| K-08             | Urutan kelas                    | B — 12 kelas resmi pemilik beserta urutannya                                                                                                                              | `RWI-DEC-307`, `308`                 |
| P-1              | Kamar SVIP 318, 401, 418        | B — VIP Superior                                                                                                                                                          | `RWI-DEC-310`                        |
| P-2              | Ruang Bayi Lantai 2             | A — kelas khusus Perinatologi (`ClassLevel` 13)                                                                                                                           | `RWI-DEC-311`                        |
| P-3              | Kelas khusus pada Down/Up Grade | A — dikecualikan dari perbandingan otomatis                                                                                                                               | `RWI-DEC-312`                        |
| P-4              | Mata angin wing                 | B — delapan arah per posisi kamar                                                                                                                                         | `RWI-DEC-313`                        |
| P-5              | Outline kamar yang bisa dipilih | A — hijau; pada admisi/transfer mengikuti kelayakan pasien                                                                                                                | `RWI-DEC-316`                        |
| Peringatan keras | Jenis kelamin                   | Tidak ada bed/kamar khusus laki-laki atau perempuan; semua boleh campur; jenis kelamin tidak dipakai                                                                      | `RWI-DEC-315`                        |
| Closure 1        | Kesiapan bed hasil impor        | A — siap langsung hanya di dev/UAT; di produksi setiap bed disahkan perawat                                                                                               | `RWI-DEC-321`                        |
| Closure 2        | Kelas yang sudah ada            | A — kelas setara diganti nama dengan Id tetap; baris baru hanya untuk kelas baru                                                                                          | `RWI-DEC-322`                        |
| Closure 3        | Unit layanan kamar denah        | A — pemetaan kamar ke unit rawat inap yang diakui RS; denah menyaring per lantai                                                                                          | `RWI-DEC-323`                        |

**Koreksi audit (`RWI-FACT-072`).** Versi awal dokumen ini menyebut larangan kamar campur masih
berlaku, dan pertanyaan K-02 disusun dari anggapan itu. Ternyata larangan tersebut sudah dicabut
`RWI-DEC-101` pada 11 September 2026, dan source hanya memeriksa penanda bed. Hasil akhirnya tidak
berubah, karena `RWI-DEC-315` menghapus seluruh aturan jenis kelamin.

Asumsi yang dipakai sampai pemilik menyatakan lain:

- arah bawaan dihitung dengan kisi 3 × 3 (`RWI-DEC-314`);
- kode bed tetap kode sistem, label "329-A" disimpan sebagai nama bed (`RWI-DEC-317`);
- kapasitas dan status kamar dihitung dari bed aktif (`RWI-DEC-318`);
- kelas khusus dikenali dari jenis kelas, dan perubahan kelas diterapkan bersama konfigurasi urutan
  kelas (`RWI-DEC-319`);
- tabel tab Pemantauan ikut diganti denah (`RWI-DEC-320`);
- kode kamar tetap kode sistem; nomor ganda dan kamar tanpa nomor dibedakan lewat kunci denah dan
  label bed (bagian 3.2; desain backend 15.6).

Asumsi lama `RWI-DEC-309` (unit satu per lantai) digantikan `RWI-DEC-323`.

### 11.2 Masih terbuka, tidak menahan desain

**RWI-OQ-144 — Boks bayi rawat gabung di kamar biasa: ikut dihitung pada badge kamar dan outline
Monitoring?** Boks rawat gabung adalah bed bertanda boks bayi di kamar ibu (`RWI-RULE-014`) dan
hanya dapat dipakai pasien bayi. Ruang Bayi tidak termasuk, karena semua bednya boks.

- **A. Tidak dihitung; boks tampil sebagai baris tersendiri di pop up kamar — Rekomendasi.** Badge
  tetap menggambarkan bed pasien dewasa, sehingga kamar tidak tampak bisa dipilih hanya karena
  boksnya kosong.
- **B. Dihitung seperti bed lain.** Lebih sederhana, tetapi kamar Kelas 2 dengan 3 bed terisi dan
  1 boks kosong tampil `4/3` dan ber-outline hijau.
- **C. Other.**

Pertanyaan pihak lain:

- **RWI-OQ-141** — kriteria klinis High/Low Dependency menurut SOP keperawatan RS.
- **RWI-OQ-142** — fakta RS:
  - arah utara gedung sebenarnya;
  - nama resmi unit rawat inap dan cakupan nurse station, untuk pemetaan `RWI-DEC-323`;
  - konfirmasi kamar 520–522, dua ruang tanpa nomor, dan nomor ganda "K. 321" di Lantai 3;
  - apakah Ruang Bayi Lantai 2 berkemampuan perawatan intensif neonatal. Bayi yang membutuhkan
    perawatan intensif hanya dapat ditempatkan di bed atau kamar bertanda intensif
    (`InpNewbornAdmissionRules.cs:71-73`).
- **RWI-OQ-143** — tarif kamar untuk kelas yang benar-benar baru dan untuk unit yang dibuat baru, oleh
  Billing.

## 12. Rujukan

- Keputusan dan aturan terkait: `RWI-RULE-012`, `RWI-RULE-014`, `RWI-DEC-048`, `064`, `066`, `101`,
  `193`, `251`, `275`–`294`, `298`–`323`, `RWI-FACT-072`, dan `073` di `00-interview-decisions.md`;
  roadmap Bed Management (`BM-DEL-20261011-01`).
- Source: `MstRoom.cs`, `MstBed.cs`, `BedStatus.cs`, `InpBedManagementQueryService.cs`,
  `InpatientBedOccupancyDtos.cs`, `InpBedOccupancyService.cs`, `InpNewbornAdmissionRules.cs`,
  `InpBedAccessService.cs`, `InpEpisode.cs`, `InpatientEpisodeController.cs`,
  `InpatientMasterDataSeeder.cs`; frontend `floor-plan/`, `bed-monitoring-cards.jsx`,
  `inpatient-bed-board.jsx`.
- Desain blueprint draft 15 Oktober 2026: [backend 15](../../02-backend-architecture.md),
  [frontend 16](../../03-frontend-architecture.md), [kamus data 22](../../data/data-dictionary.md),
  [API 14](../../contracts/api-contract.md), [PRD 27](../../04-prd-to-mvp.md), dan
  [manifest anak bagian 17](../../blueprint-manifest.md).
- Bukti kemampuan current: [BM-IMP-20261015-01](../../../evidence/denah-bed-impact-scan-20261015.md).
  Empat temuannya sudah diputuskan pada closure pass:
  - kode bed dibuat otomatis `BD-RSMMC-#####`; `329-A` tampil dari nama bed (`RWI-DEC-317`, bagian 3.2);
  - kapasitas kamar dihitung dari bed aktif, tidak diselaraskan (`RWI-DEC-318`, bagian 4 dan 7);
  - bed baru dan bed sesudah ubah kelas Tidak Tersedia sampai disahkan perawat; bed impor di dev/UAT
    langsung siap (`RWI-DEC-321`, `RWI-AC-455`, bagian 7 dan 8.3);
  - tarif kamar dicocokkan per unit layanan; kamar dipetakan ke unit yang diakui RS sehingga tarif
    yang ada tetap berlaku (`RWI-DEC-323`, bagian 8.2).
- Tingkat perawatan berdasarkan kebutuhan pasien (Level 0–3, _Comprehensive Critical Care_), dikutip
  NCEPOD: <https://ncepod.org.uk/2005report/method.html>
- Klasifikasi ketergantungan pasien (minimal, partial, total care) untuk kebutuhan tenaga perawat:
  <https://jkp.fkep.unpad.ac.id/index.php/jkp/article/download/66/63/125>
- Pedoman HCU, KMK 834/MENKES/SK/VII/2010: <https://pasal.id/peraturan/permen/kepmenkes-834-menkes-sk-vii-2010>
