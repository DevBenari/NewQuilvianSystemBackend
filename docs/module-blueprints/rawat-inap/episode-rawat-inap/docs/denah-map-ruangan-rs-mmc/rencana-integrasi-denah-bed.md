# Rencana Integrasi Denah dengan Bed Management — RS MMC

**Tanggal:** 15 Oktober 2026 · **Status:** rencana, menunggu keputusan pemilik di bagian 11.
**Basis source:** backend `MHamzah` @ `5f0c1412`, frontend `HamzahV2` @ `4d9bf608`.

**Lingkup.** Denah ruangan (`/health-services/inpatient-management/floor-plan`) dihubungkan
dengan data kamar dan tempat tidur (bed) yang nyata lewat API. Setelah itu denah menjadi
satu-satunya cara melihat, memilih, dan mengelola bed. Denah dipakai di Monitoring Bed, admisi,
transfer, dan master data. **Desain tampilan denah tidak diubah sama sekali.**

Dokumen ini melanjutkan [rencana-kelas-kamar.md](rencana-kelas-kamar.md), yang menetapkan kelas
setiap kamar pada denah.

## 1. Ringkasan

- Denah menjadi patokan bentuk bangunan RS MMC. Susunan datanya: **Lantai → Wing (ruang rawat per
  nurse station) → Kamar → Bed**. Data master di database disusun persis mengikuti denah: 106
  kamar dan 168 bed di Lantai 2, 3, 4, 5, 8, dan 9.
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
  - **tingkat ketergantungan pasien** (*High Dependency / Low Dependency*), yang melekat pada
    pasien selama dirawat, bukan pada bed.
- Penanda bed khusus laki-laki/perempuan diusulkan dihapus. Larangan sekamar laki-laki dan
  perempuan tetap berlaku, berdasarkan penghuni yang sedang ada.
- Data rawat inap di database pengembangan pribadi `QuilvianNewDevHamzah` dibersihkan sebelum
  integrasi diuji. Sebelum itu database dicadangkan, dan eksekusinya meminta konfirmasi tersendiri.
- Ada **8 keputusan** yang perlu dijawab pemilik (bagian 11). Setiap keputusan disertai
  rekomendasi.

## 2. Kondisi sekarang (hasil audit)

Bed Management sudah cukup lengkap. Yang paling banyak berubah adalah cara menampilkan dan susunan
datanya.

| Bagian | Yang sudah ada | Bukti | Dampak rencana |
|---|---|---|---|
| Master kamar | `MstRoom`: unit layanan, kelas, kode, nama, jenis kamar, nomor, lantai (teks bebas `FloorName`), kapasitas, penanda laki-laki/perempuan/bayi/isolasi/intensif/ODC, boleh admisi | `Areas/HealthServices/MasterData/Models/MstRoom.cs` | Ditambah lantai terstruktur dan kunci denah; penanda jenis kelamin dihapus (K-02) |
| Master bed | `MstBed`: kamar, kode, nama, nomor, `BedStatus` 8 nilai, penanda laki-laki/perempuan/bayi/isolasi/intensif/ODC, boleh dipesan | `.../Models/MstBed.cs`, `.../Enums/BedStatus.cs` | Penanda jenis kelamin dihapus; kode bed dibakukan |
| Enam status bed | Dihitung dari hunian, pesanan, kesiapan pembersihan, dan penutupan bed | `InpBedManagementQueryService.cs:15-18, 117-128`; `RWI-DEC-282` | Tetap |
| Pesanan bed | Berlaku 120 menit, kedaluwarsanya dihitung server | `RWI-DEC-283` | Tetap |
| Pembersihan | Housekeeping mencatat mulai dan selesai, lalu perawat ruangan mengesahkan bed siap | `RWI-DEC-278`, `280`, `282` | Tetap; dijalankan dari pop up kamar |
| Kebutuhan isolasi | Atribut episode `RequiresIsolation` beserta sumber, pencatat, dan waktunya; endpoint `PATCH episodes/{id}/isolation-requirement` | `InpEpisode.cs:77-88`; `InpatientEpisodeController.cs:379` | Pola yang sama dipakai untuk High/Low Dependency |
| Aturan jenis kelamin | Aturan keras: penanda bed **dan** larangan kamar campur berdasarkan penghuni | `RWI-RULE-012` bagian B; `RWI-DEC-064`, `066` | Perlu keputusan K-02 |
| Hak akses bed | Cakupan per unit layanan (`BedManagementOptions.UnitAssignments`) | `InpBedAccessService.cs:17` | Wing dijadikan unit layanan (K-01) |
| Papan bed (API) | `GET bed-occupancies/bed-board`: unit → kamar → bed, nama pasien, pesanan, perbandingan kelas | `InpatientBedOccupancyDtos.cs:157-266` | Ditambah data per lantai untuk denah |
| Tampilan kartu | `bed-monitoring-cards.jsx` (Monitoring Bed) dan `inpatient-bed-board.jsx` 1.261 baris (Pilih Bed admisi, transfer, detail episode, Workspace Keperawatan, admisi bayi) | frontend | **Dihapus**, diganti denah |
| Denah | Halaman data dummy: 106 kamar, kelas, legenda, pop up nama kamar | `floor-plan/`, commit `65b222bb` | Menjadi komponen bersama yang membaca API |
| Master data | Halaman daftar dan form Master Room serta Master Bed. Seeder **tidak pernah** membuat kamar/bed (`RWI-DEC-048`) | `InpatientMasterDataSeeder.cs:21-23` | Master Bed diganti editor denah; `RWI-DEC-048` diamandemen (K-07) |
| Pemakai lain | `MstRoom` juga dipakai Kamar Operasi (`RoomType.OperatingRoom`); `MstBed` hanya dipakai Rawat Inap | `OperatingRoomSchedulingService.cs` | Halaman Master Room **tetap ada** untuk kamar di luar denah |

## 3. Struktur denah di database

### 3.1 Susunan dan contoh

```
Lantai 3
├── Wing A — "Rawat Inap Lantai 3 A" (nurse station NS3A)
│   ├── Kelas I 330    → bed 330-A, 330-B
│   ├── Kelas II 329   → bed 329-A, 329-B, 329-C
│   └── VIP 301 … 309  → masing-masing satu bed, mis. 301-A
└── Wing B — "Rawat Inap Lantai 3 B" (nurse station NS3B)
    ├── Kelas II 322 … 320, VIP Isolasi 319
    └── VIP 310 … 317, SVIP 318
```

Pembagian wing di atas masih **usulan** berdasarkan letak kamar terhadap inti lift. RS perlu
mengonfirmasi kamar mana yang dilayani setiap nurse station (K-01).

### 3.2 Parameter baru dan yang berubah

| Parameter | Arti | Disimpan di | Contoh | Diisi oleh |
|---|---|---|---|---|
| Lantai | Lantai tempat kamar berada; menentukan tab denah | `MstRoom.FloorCode` (**baru**); `FloorName` tetap sebagai label | `3` / "Lantai 3" | Importer denah |
| Wing (ruang rawat) | Bagian lantai yang dilayani satu nurse station; dasar hak akses perawat | Satu `MstServiceUnit` per wing, dirujuk `MstRoom.ServiceUnitId` (sudah ada) | `RI-3A` "Rawat Inap Lantai 3 A" | Admin MasterData |
| Kunci denah | Penghubung satu kamar di database dengan satu bentuk kamar di gambar | `MstRoom.FloorPlanRoomKey` (**baru**, unik) | `lantai-3/k-329` | Importer denah |
| Nomor kamar | Nomor fisik kamar | `MstRoom.RoomNumber` (sudah ada) | `329` | Importer; mengikuti gambar |
| Kelas kamar | Kelas rawat yang berlaku | `MstRoom.PatientClassId` (sudah ada) | Kelas II | Admin lewat denah |
| Nama kamar | Format `<Kelas> <Nomor>` | `MstRoom.RoomName`, dibentuk otomatis | "Kelas II 329" | Otomatis |
| Kode bed | `<Nomor kamar>-<huruf urut>` | `MstBed.BedCode` (sudah ada) | `329-A` | Otomatis saat bed ditambah |
| Jumlah bed | Jumlah bed aktif di kamar | Dihitung dari `MstBed`; `MstRoom.Capacity` diselaraskan otomatis | 3 | Otomatis |
| Tingkat ketergantungan | High atau Low Dependency pasien selama dirawat | `InpEpisode.CareDependencyLevel` (**baru**) beserta pencatat, waktu, dan riwayatnya | High | Perawat/DPJP (K-03) |

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

**Contoh.** RS memutuskan ruang tanpa label di sebelah K. 322 dipakai menjadi kamar 324 Kelas II
dengan 3 bed. Admin membuka Master Bed, memilih Lantai 3, lalu mengklik ruang abu-abu itu. Ia
mengisi nomor 324, memilih Kelas II, menambah 3 bed, lalu menyimpan. Kamar "Kelas II 324" langsung
muncul di Monitoring Bed dan di pilihan bed admisi.

### 3.4 Jumlah data dari denah

| Lantai | Kamar | Bed | Rincian |
|---|---:|---:|---|
| 2 | 6 | 14 | ICU 4 bed (Ruang ICU 3 + R. ICU CK 1), ICU VIP 2, Ruang Bayi 8 |
| 3 | 29 | 48 | Kelas I 1 kamar/2 bed, Kelas II 9/27, VIP 17/17, SVIP 1/1, VIP Isolasi 1/1 |
| 4 | 27 | 43 | Kelas I 2/4, Kelas II 7/21, VIP 16/16, SVIP 2/2 |
| 5 | 11 | 30 | Kelas III 2/12, Kelas II 3/9, Kelas I 6/9 |
| 8 | 13 | 13 | Premium 10, Suite 2, Grand Royal 1 |
| 9 | 20 | 20 | VIP 14, VIP Deluxe 3, VIP Superior 3 |
| **Total** | **106** | **168** | |

Jumlah bed per kamar mengikuti data dummy di [rencana-kelas-kamar.md](rencana-kelas-kamar.md) bagian 5.
Setelah diimpor, angka ini dapat diubah dari editor denah.

## 4. Perombakan penanda kamar dan bed

| Kolom | Sekarang | Usulan | Alasan |
|---|---|---|---|
| `IsForMale`, `IsForFemale` (kamar dan bed) | Wajib diisi; dibaca aturan jenis kelamin B1–B2 | **Hapus** (K-02) | Permintaan pemilik. Kamar RS MMC tidak dikhususkan per jenis kelamin; larangan campur tetap dijaga dari penghuni |
| `IsForNewborn` (kamar dan bed) | Boks bayi | **Pertahankan** | Dipakai aturan boks bayi (B4–B5) dan Ruang Bayi Lantai 2 |
| `IsIsolationRoom`, `IsIsolationBed` | Aturan isolasi keras (A5–A6) | **Pertahankan** | Keputusan pengendalian infeksi `RWI-DEC-064`; dipakai VIP Isolasi 319 |
| `IsIntensiveCare`, `IsIntensiveCareBed` | Penanda ICU | **Pertahankan**, diisi otomatis dari kelas ICU/ICU VIP | General Consent memilih "Khusus (ICU/Isolasi)" dari penanda ini (`RWI-DEC-251`) |
| `IsOdcRoom`, `IsOdcBed` | ODC (One Day Care) | **Pertahankan**, tidak dipakai denah | ODC ada di formulir RS tetapi tidak ada di denah |
| `RoomType` | 13 nilai | **Pertahankan**, diisi otomatis dari kelas | Contoh: Kelas II → Rawat Inap; ICU → Intensif; Ruang Bayi → Bayi |
| `FloorName`, `LocationName` | Teks bebas | `FloorName` menjadi label otomatis "Lantai 3"; `LocationName` berisi nama wing | Supaya tidak ada dua sumber yang bisa berbeda |
| `Capacity` | Diisi manual | **Diselaraskan otomatis** dengan jumlah bed aktif | Mencegah kapasitas 3 tetapi bed hanya 2 |
| `IsAvailableForAdmission`, `IsReservable` | Penutup kamar/bed | **Pertahankan** | Dipakai status Tidak Tersedia (`RWI-DEC-285`) |
| `BedStatus` (8 nilai) | Status master | **Pertahankan** | `RWI-DEC-285` tidak mengubah enum. Available/Maintenance/Blocked/Inactive dipakai untuk menutup dan membuka bed |

**Akibat bagi aturan jenis kelamin bila penanda dihapus (K-02 pilihan A).**

| Aturan `RWI-RULE-012` bagian B | Sesudah |
|---|---|
| B1 — tolak bila penanda bed tidak menerima jenis kelamin pasien | **Gugur**, karena semua bed menerima keduanya |
| B2 — jenis kelamin belum tercatat hanya ke bed netral di kamar kosong | Menjadi: jenis kelamin belum tercatat **hanya ke kamar kosong** |
| B3 — kamar tidak boleh campur, dilihat dari penghuni yang sedang ada | **Tetap** |
| B4–B7 — pengecualian boks bayi, kamar satu bed, berlaku juga pada transfer | **Tetap** |

**Contoh.** Kamar Kelas II 329 kosong, sehingga pasien laki-laki maupun perempuan boleh masuk.
Setelah Tn. Budi menempati 329-A, bed 329-B dan 329-C hanya dapat dipilih pasien laki-laki.
Pop up menulis alasannya: "Kamar dihuni pasien laki-laki".

## 5. Status bed, status kamar, dan tingkat ketergantungan

### 5.1 Enam status bed (tetap, `RWI-DEC-282`)

| Status | Arti | Boleh dipilih? |
|---|---|---|
| Tersedia | Bed siap dan dapat dipesan saat ini | Ya |
| Terisi | Sedang ditempati pasien | Tidak |
| Dipesan | Sudah dipesan untuk pasien lain; berlaku 120 menit | Tidak |
| Menunggu Pembersihan | Pasien sudah keluar, bed belum dibersihkan | Tidak |
| Dalam Pembersihan | Housekeeping sedang membersihkan, termasuk tahap menunggu pengesahan perawat | Tidak |
| Tidak Tersedia | Ditutup dengan alasan (rusak, perbaikan, nonaktif) | Tidak |

### 5.2 Perubahan status bed

| Dari | Tindakan | Ke | Pelaku | Syarat |
|---|---|---|---|---|
| Tersedia | Pesan bed saat admisi | Dipesan | Petugas admisi | Lolos aturan kelayakan (bagian 6.4) |
| Dipesan | Pasien ditempatkan | Terisi | Petugas berwenang | Pesanan milik episode itu |
| Dipesan | Batal atau kedaluwarsa | Tersedia | Petugas atau sistem | Tanpa pembersihan, karena bed belum dipakai (`RWI-DEC-281`) |
| Tersedia | Transfer masuk | Terisi | Petugas transfer | Lolos aturan dan kategori kelas benar |
| Terisi | Pasien pulang atau pindah | Menunggu Pembersihan | Sistem | Otomatis (`RWI-DEC-281`) |
| Menunggu Pembersihan | Mulai pembersihan | Dalam Pembersihan | Housekeeping | — |
| Dalam Pembersihan | Selesai, menunggu pengesahan | Dalam Pembersihan (menunggu verifikasi) | Housekeeping | — |
| Dalam Pembersihan (menunggu verifikasi) | Sahkan siap | Tersedia | Perawat ruangan berwenang | — |
| Dalam Pembersihan (menunggu verifikasi) | Nyatakan belum siap | Menunggu Pembersihan | Perawat ruangan | Alasan wajib |
| Tersedia / Menunggu / Dalam Pembersihan | Tutup bed | Tidak Tersedia | Admin MasterData | Alasan wajib; bed Terisi atau Dipesan tidak boleh ditutup |
| Tidak Tersedia | Buka bed | Mengikuti kesiapannya | Admin MasterData | Membuka tidak otomatis berarti siap |

**Contoh perjalanan bed 329-C.** Pukul 08:00 bed Tersedia. Pukul 08:10 petugas admisi memesannya
untuk Tn. Andi, sehingga statusnya Dipesan sampai 10:10. Pukul 09:00 Tn. Andi tiba dan
ditempatkan, sehingga statusnya Terisi. Tiga hari kemudian Tn. Andi pulang, dan bed menjadi Menunggu
Pembersihan. Housekeeping mulai bekerja (Dalam Pembersihan), mencatat selesai (menunggu
verifikasi), lalu perawat ruangan mengesahkan. Bed kembali Tersedia.

### 5.3 Status kamar (usulan, K-04)

Status kamar **tidak disimpan**. Status itu dihitung dari bed-bed di kamar tersebut, dan aturan yang
paling atas yang berlaku:

| Urutan | Status kamar | Syarat |
|---:|---|---|
| 1 | Tidak Tersedia | Kamar ditutup, atau semua bed aktif berstatus Tidak Tersedia |
| 2 | Tersedia | Ada sekurang-kurangnya satu bed Tersedia |
| 3 | Persiapan | Tidak ada bed Tersedia, tetapi ada bed yang Menunggu atau Dalam Pembersihan |
| 4 | Penuh | Semua bed yang dapat dipakai berstatus Terisi atau Dipesan |

**Di peta (desain tetap).** Pada peta, garis merah "Kamar penuh" menandai status Penuh, dan badge
"kapasitas/terisi" tetap tampil.
- **Kapasitas** = bed yang dapat dipakai, yaitu tanpa bed Tidak Tersedia.
- **Terisi** = bed Terisi ditambah bed Dipesan, supaya badge selaras dengan garis merah.

Status Persiapan dan Tidak Tersedia tidak menambah tanda baru di peta. Keduanya tampil di judul pop
up kamar, sehingga peta tidak berubah dari desainnya.

| Isi kamar 3 bed | Status kamar | Badge | Garis merah |
|---|---|---|---|
| Terisi, Terisi, Tersedia | Tersedia | `3/2` | Tidak |
| Terisi, Dipesan, Terisi | Penuh | `3/3` | Ya |
| Terisi, Terisi, Menunggu Pembersihan | Persiapan | `3/2` | Tidak |
| Terisi, Terisi, Tidak Tersedia | Penuh | `2/2` | Ya |
| Tidak Tersedia ×3 | Tidak Tersedia | `0/0` | Tidak |

### 5.4 Tingkat ketergantungan pasien — High / Low Dependency (usulan, K-03)

**Arti.** *High Dependency* (ketergantungan tinggi) adalah pasien yang membutuhkan pengamatan dan
bantuan perawat lebih ketat dibanding pasien bangsal biasa, tetapi tidak membutuhkan ICU. *Low
Dependency* (ketergantungan rendah) adalah pasien yang mandiri atau hanya sebagian dibantu.
Kriteria klinisnya ditetapkan RS lewat SOP keperawatan. Sistem mencatat pilihan dan riwayatnya,
tetapi tidak menilai sendiri.

**Mengapa melekat pada pasien, bukan pada bed.** Klien menyatakan bahwa dalam satu kamar Kelas II,
High atau Low Dependency ditentukan oleh pasiennya. Hal ini sejalan dengan praktik yang menilai
tingkat perawatan menurut kebutuhan pasien di mana pun ia dirawat, bukan menurut jenis ruangannya.
Karena itu kamar dan bed **tidak diberi penanda** HD/LD.

**Pencatatan** mengikuti pola kebutuhan isolasi yang sudah ada:
- Nilai disimpan pada episode rawat inap: High, Low, atau *belum dinilai*.
- Setiap perubahan dicatat dengan pelaku, waktu, sumber, dan catatan. Sumbernya bisa catatan awal
  admisi, penilaian perawat, atau keputusan DPJP.
- Riwayat lama tidak ditimpa.

**Ringkasan di kamar.** Kamar berlabel High Dependency bila ada **satu saja** penghuninya yang HD.
Label ini tampil di judul pop up beserta hitungannya, dan di setiap baris bed.

**Contoh — Kelas II 329.**

| Bed | Status | Penghuni | Ketergantungan |
|---|---|---|---|
| 329-A | Terisi | Tn. Budi S. | High |
| 329-B | Terisi | Tn. Andi P. | Low |
| 329-C | Tersedia | — | — |

Status kamar **Tersedia**, ketergantungan kamar **High Dependency (1 HD · 1 LD)**. Bed 329-C boleh
dipesan pasien laki-laki mana pun, baik HD maupun LD, kecuali K-03 memutuskan HD membatasi
penempatan.

## 6. Alur memilih bed

### 6.1 Admisi — langkah Pilih Bed

| Unsur | Isi |
|---|---|
| Tujuan | Pasien baru mendapat bed yang kosong dan sesuai |
| Pelaku | Petugas admisi memesan bed; perawat atau petugas berwenang menempatkan pasien |
| Pemicu | Langkah 5 "Pilih Bed" pada alur admisi (termasuk admisi bayi) |
| Prasyarat | Episode Draft dari langkah 1–4; jenis kelamin pasien; kebutuhan isolasi; kelas dan penjamin |

Langkah utama:

1. Denah terbuka. Lantai pertama yang tampil adalah lantai dengan bed Tersedia terbanyak, atau
   lantai yang terakhir dipakai petugas.
2. Petugas memilih tab lantai.
3. Petugas mengklik kamar. Pop up menampilkan setiap bed dengan statusnya, penghuninya (nama,
   No. RM, jenis kelamin, ketergantungan) sesuai hak akses, dan sisa waktu pesanan.
4. Bed Tersedia yang lolos aturan menampilkan tombol **Pilih bed ini**. Bed yang tidak lolos tetap
   terlihat beserta alasannya.
5. Petugas memilih bed lalu menekan konfirmasi. Sistem memesan bed itu selama 120 menit.
6. Pop up tertutup. Langkah admisi menampilkan "Kelas II 329 · bed 329-C", dan kelas admisi mengikuti
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
sistem menolak pilihan yang tidak sesuai perbandingan kelas (`RWI-DEC-275`, `276`). Petugas juga
mengisi alasan dan menyimpan. Transfer tersimpan utuh dalam satu langkah (`RWI-DEC-284`). Bed asal
menjadi Menunggu Pembersihan, dan bed tujuan menjadi Terisi.

### 6.3 Pop up kamar

Pop up memakai kartu yang sama dengan pop up denah sekarang: judul nama kamar, garis aksen berwarna
kelas, dan baris rincian. Daftar bed ditambahkan di bawahnya. Peta di belakangnya tidak berubah.

```
┌────────────────────────────────────────────────────────────────────┐
│                        ━━━━                                     ✕  │
│                      Kelas II 329                                  │
│     ● Kelas II | Tersedia | Laki-laki | High Dependency (1 · 1)    │
│ ────────────────────────────────────────────────────────────────── │
│ 329-A  ● Terisi    Tn. Budi S. · RM 00-12-34 · L · High Dependency │
│ 329-B  ● Terisi    Tn. Andi P. · RM 00-56-78 · L · Low Dependency  │
│ 329-C  ● Tersedia                                [ Pilih bed ini ] │
└────────────────────────────────────────────────────────────────────┘
```

| Mode | Dipakai di | Isi tambahan |
|---|---|---|
| Lihat | Monitoring Bed | Aksi per bed sesuai hak: mulai/selesai pembersihan, sahkan siap, tutup/buka bed, riwayat bed |
| Pilih | Admisi dan transfer | Tombol **Pilih bed ini** pada bed yang lolos aturan; alasan pada bed yang tidak lolos |
| Master | Master Bed | Ubah kelas, tambah/kurangi bed, tutup/buka bed, aktifkan ruang cadangan |

Warna titik status bed memakai warna legenda kartu yang sekarang, supaya artinya tetap sama.

### 6.4 Aturan yang diperiksa saat memilih bed

| Aturan | Sumber | Yang tampil di pop up |
|---|---|---|
| Kamar tidak boleh campur laki-laki dan perempuan | `RWI-RULE-012` B3 | "Kamar dihuni pasien laki-laki/perempuan" |
| Pasien butuh isolasi hanya ke bed isolasi, dan sebaliknya | `RWI-RULE-012` A5–A6 | "Bed isolasi" / "Pasien membutuhkan isolasi" |
| Boks bayi dikecualikan dari aturan jenis kelamin | `RWI-RULE-012` B4–B5 | — |
| Kategori kelas transfer harus sesuai | `RWI-DEC-275`–`277`, `291` | Kategori yang diizinkan per bed |
| Identitas penghuni hanya untuk yang berhak; Housekeeping tidak melihat identitas | `RWI-DEC-287` | Nama disamarkan, mis. "B*** S." |
| High Dependency membatasi penempatan (hanya bila K-03 memilih B) | K-03 | "Kamar khusus pasien HD" |

## 7. Master data bed lewat denah

| Unsur | Isi |
|---|---|
| Tujuan | Kamar dan bed di database selalu sama dengan keadaan fisik gedung |
| Pelaku | Admin MasterData (seluruh tim MasterData, `RWI-DEC-193`) |
| Pemicu | Kamar berubah kelas, bed bertambah/berkurang, bed rusak, atau ruang cadangan dipakai |
| Prasyarat | Data master denah sudah diimpor (bagian 8) |

Langkah utama:

1. Admin membuka **Master Bed**. Yang tampil langsung denah dengan desain yang sama.
2. Admin memilih lantai lalu mengklik kamar. Pop up mode master terbuka.
3. Admin dapat melakukan:
   - **ubah kelas**;
   - **tambah bed**, dengan kode berikutnya dibuat otomatis, mis. 329-D;
   - **kurangi bed**;
   - **tutup atau buka bed**, dengan alasan;
   - **pindahkan kamar ke wing lain**;
   - **aktifkan ruang cadangan** dengan nomor dan kelasnya.
4. Admin menyimpan. Nama kamar, kapasitas, dan badge di semua denah ikut berubah.

Aturan:

| Tindakan | Aturan | Contoh penolakan |
|---|---|---|
| Ubah kelas | Hanya bila kamar tidak berpenghuni dan tidak ada pesanan (rekomendasi K-05) | "Kelas II 329 masih dihuni 2 pasien" |
| Tambah bed | Kode otomatis huruf berikutnya; kapasitas ikut naik | — |
| Kurangi bed | Bed yang belum pernah dipakai dihapus; bed yang punya riwayat hunian hanya dinonaktifkan agar riwayat tetap ada | "329-A punya riwayat, dinonaktifkan" |
| Tutup bed | Alasan wajib; bed Terisi/Dipesan ditolak (`RWI-DEC-285`) | "329-A sedang Terisi" |
| Buka bed | Bed kembali mengikuti kesiapannya, tidak otomatis Tersedia | — |
| Aktifkan ruang cadangan | Nomor unik di lantai itu, kelas wajib, sekurang-kurangnya satu bed | "Nomor 324 sudah dipakai" |

Halaman **Master Room** (daftar tabel) tetap ada, karena `MstRoom` juga dipakai Kamar Operasi dan
ruangan lain di luar denah. Kamar yang punya kunci denah sebaiknya diubah dari editor denah.

## 8. Data master dari denah (impor)

### 8.1 Kelas rawat (`MstPatientClass`)

Usulan urutan `ClassLevel` dari rendah ke tinggi: Kelas III, Kelas II, Kelas I, VIP, VIP Deluxe,
VIP Superior, SVIP, Premium, Suite, Grand Royal. Ada juga kelas khusus: VIP Isolasi, ICU, ICU VIP,
dan Perinatologi (Ruang Bayi). Urutan ini dipakai untuk menentukan Down Grade, Up Grade, atau Same
Grade, sehingga **wajib diverifikasi tim MasterData** sebelum validasi lintas kelas aktif
(`RWI-DEC-291`). Contohnya, apakah Premium lebih tinggi dari SVIP. VIP Isolasi diusulkan sebagai
kelas tersendiri, karena SIRS Kemenkes mencatat tempat tidur isolasi sebagai kelompok sendiri; hal
ini ikut diverifikasi pada K-08.

### 8.2 Wing (unit layanan rawat inap) — usulan K-01

| Lantai | Unit | Kamar |
|---|---|---|
| 2 | ICU | Ruang ICU, R. ICU CK, R. ICU VIP ×2 |
| 2 | Perinatologi | R. Bayi ×2 |
| 3 | Rawat Inap Lantai 3 A (NS3A) | 325–330, VIP 301–309 |
| 3 | Rawat Inap Lantai 3 B (NS3B) | 319–322, VIP 310–317, SVIP 318 |
| 4 | Rawat Inap Lantai 4 A (NS A) | 425–428, SVIP 401, VIP 402–408 |
| 4 | Rawat Inap Lantai 4 B (NS B) | 420–423, 429, VIP 409–417, SVIP 418 |
| 5 | Rawat Inap Lantai 5 A (NS Lt 5 A) | 503–510 |
| 5 | Rawat Inap Lantai 5 B (sayap kanan) | 520–522 |
| 8 | Rawat Inap Lantai 8 | 801–813 |
| 9 | Rawat Inap Lantai 9 | 901–920 |

Nama unit di atas sementara. Bila RS punya nama resmi ruang rawat, misalnya nama bunga, nama itu yang
dipakai.

### 8.3 Cara impor

1. Daftar kamar diekspor dari data denah frontend: kunci denah, lantai, nomor, kelas, jumlah bed,
   dan wing.
2. Importer backend membuat atau memperbarui kamar dan bed menurut **kunci denah**. Importer aman
   dijalankan berulang dan tidak pernah menghapus kamar yang punya riwayat.
3. Importer dijalankan di `QuilvianNewDevHamzah` setelah data rawat inap dibersihkan (bagian 9),
   dan menghasilkan ringkasan: dibuat, diperbarui, dilewati.
4. Setelah impor, **database menjadi sumber kelas dan jumlah bed**. File data dummy okupansi
   dihapus, dan label "Kelas II 329" di peta dibentuk dari data API. Hasilnya tetap sama dengan
   tampilan sekarang.
5. Di produksi, importer yang sama dijalankan sekali oleh admin berwenang setelah RS mengesahkan
   susunan kamar (K-07).

## 9. Pembersihan data rawat inap di `QuilvianNewDevHamzah`

**Batas aman.**
- Hanya database pengembangan pribadi `QuilvianNewDevHamzah`. Database tim tidak boleh disentuh.
- Kredensial tidak ditulis di dokumen ini maupun di skrip yang di-commit; koneksinya ada di
  `appsettings.Development.json`.
- Eksekusi hanya dijalankan setelah pemilik mengonfirmasi daftar tabel dan jumlah barisnya, sesuai
  larangan otomatisasi database di `CLAUDE.md` backend.

| Cakupan | Isi | Rekomendasi |
|---|---|---|
| A — sempit | Data bed operasional (penempatan, pesanan, upaya pembersihan, jejak siklus, tanda terima operasi) dan episode rawat inap beserta semua tabel `Inp*` turunannya (riwayat status, penugasan dokter/perawat, ringkasan pulang, clearance, dokumen admisi, tanda tangan, rujukan, surat deposit/selisih biaya, outbox) | — |
| B — lengkap | A ditambah data modul lain yang merujuk episode rawat inap: catatan klinis dan keperawatan, resep dan pemberian obat, order lab/radiologi, tagihan dan deposit rawat inap, kunjungan rawat inap, serta permintaan admisi dari IGD/Kamar Operasi | **Ya** — tidak meninggalkan data yatim yang merujuk episode yang sudah dihapus |
| Tidak disentuh | Master pasien, master lain, data rawat jalan/IGD/Kamar Operasi yang tidak terkait episode rawat inap | — |

Langkah:

1. **Petakan relasi** dari katalog database (`information_schema`). Hasilnya daftar tabel dan
   urutan hapus dari anak ke induk. Langkah ini hanya membaca.
2. **Cadangkan** seluruh database dengan `pg_dump -Fc` ke folder lokal di luar repository.
3. **Hitung baris** setiap tabel (*dry run*), lalu tunjukkan ke pemilik.
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

## 10. Rencana pekerjaan

### 10.1 Fase dan urutan

| Fase | Isi | Bergantung pada |
|---|---|---|
| 0 | Pemilik menjawab K-01–K-08. Jawabannya dicatat di Decision Log (`00-interview-decisions.md`), lalu blueprint dan kontrak diperbarui lewat rantai skill perencanaan sampai task resmi `BE-RWI`/`FE-RWI` terbit di roadmap | — |
| 1 | Pembersihan data rawat inap di DB Hamzah (bagian 9) | Fase 0 (K-06) dan konfirmasi eksekusi |
| 2 | Backend P-BE-01–06 | Fase 0 |
| 3 | Impor master denah ke DB Hamzah | Fase 1, P-BE-01, P-BE-02 |
| 4 | Frontend P-FE-01–09 | P-FE-01 bisa dimulai lebih dulu; sisanya menunggu API |
| 5 | Verifikasi ujung ke ujung dan UAT | Fase 3 dan 4 |

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

ID `P-BE`/`P-FE` adalah nomor kerja sementara. Task ID resmi diterbitkan saat Fase 0 selesai,
setelah ruang ID `BE-RWI`/`FE-RWI` yang dipakai bersama diperiksa.

### 10.2 Backend

| ID | Pekerjaan | Hasil |
|---|---|---|
| P-BE-01 | Skema: `MstRoom.FloorCode`, `MstRoom.FloorPlanRoomKey` (unik), `InpEpisode.CareDependencyLevel` beserta pencatat/waktu/sumber dan riwayatnya; hapus `IsForMale`/`IsForFemale` bila K-02 A. Migration dan pemutakhiran kontrak | Struktur data denah siap |
| P-BE-02 | Importer susunan denah: idempoten menurut kunci denah, membuat unit/wing, kamar, dan bed, serta melaporkan hasil | 106 kamar dan 168 bed masuk database |
| P-BE-03 | API denah per lantai: kamar dengan status kamar, kapasitas/terisi, jenis kelamin penghuni, ringkasan HD/LD; bed dengan status, penghuni sesuai hak, pesanan, kelayakan untuk episode tertentu, dan aksi yang diizinkan | Satu panggilan per lantai |
| P-BE-04 | API ketergantungan pasien: catat/ubah HD/LD dengan izin baru `InpatientEpisode : SetCareDependency`, beserta riwayatnya | HD/LD tercatat dan dapat ditelusuri |
| P-BE-05 | Editor master: ubah kelas dengan aturan K-05, tambah bed berkode otomatis, kurangi bed (hapus atau nonaktifkan), kapasitas tersinkron, aktifkan ruang cadangan. Sebisa mungkin memakai endpoint Master Room/Bed yang sudah ada | Master dapat dikelola dari denah |
| P-BE-06 | Aturan kelayakan: aturan jenis kelamin tanpa penanda (bagian 4) dan pembatasan HD bila K-03 memilih B | Penempatan dan transfer konsisten dengan keputusan |

### 10.3 Frontend

| ID | Pekerjaan | Hasil |
|---|---|---|
| P-FE-01 | Jadikan denah komponen bersama: komponen ke `src/components/features/health-services/inpatient-management/floor-plan/`, geometri ke `src/lib/constants/...`, CSS dipindah apa adanya. **Tampilan tidak berubah**, dibuktikan dengan perbandingan screenshot sebelum dan sesudah per lantai | Satu denah dipakai di semua layar |
| P-FE-02 | Data dari API per lantai (hook + service `InstanceAxios`); data dummy kelas dan okupansi dihapus | Peta menampilkan data nyata |
| P-FE-03 | Pop up kamar: daftar bed, enam status, penghuni sesuai hak, ringkasan status kamar dan HD/LD, mode lihat/pilih/master | Bagian 6.3 |
| P-FE-04 | Tab Monitoring Bed memakai denah; aksi pembersihan dan tutup/buka dari pop up | Kartu monitoring hilang |
| P-FE-05 | Admisi langkah Pilih Bed (termasuk admisi bayi) memakai denah mode pilih | Bagian 6.1 |
| P-FE-06 | Transfer (tab Bed Transfer, detail episode, Workspace Keperawatan) memakai denah untuk tujuan | Bagian 6.2 |
| P-FE-07 | Master Bed menjadi editor denah; halaman daftar Master Bed diganti | Bagian 7 |
| P-FE-08 | Input HD/LD di Workspace Keperawatan dan detail episode, beserta riwayatnya | Bagian 5.4 |
| P-FE-09 | Hapus kode kartu dan test lamanya (bagian 10.5) | Tidak ada dua tampilan bed |

### 10.4 API

Endpoint berlabel **Rencana (belum tersedia)** belum ada di kode.

#### Health Services / Inpatient Management / Bed Occupancy

Base URL: `api/v1/health-services/inpatient-management/bed-occupancies`

| Method | Path | Kegunaan | Hak akses | Request | Response | Keadaan |
|---|---|---|---|---|---|---|
| `GET` | `/floor-plan` | Data denah satu lantai: kamar, status kamar, bed, penghuni, dan kelayakan untuk episode tertentu | `InpatientBedOccupancy : Read` | Query `floorCode`, `purpose` (`monitoring`/`admission`/`transfer`), `episodeId` opsional | `FloorPlanOccupancyResponse` | Rencana (belum tersedia) |
| `POST` | `/reservations` | Memesan bed saat admisi | `InpatientBedOccupancy : Create` | `ReserveBedRequest` | Pesanan bed | Sudah ada |
| `PATCH` | `/reservations/{id}/cancel` | Membatalkan pesanan | `InpatientBedOccupancy : Update` | Alasan | Pesanan bed | Sudah ada |
| `POST` | `/placements` | Menempatkan pasien | `InpatientBedOccupancy : Create` | Penempatan | Penempatan | Sudah ada |
| `POST` | `/placements/transfer` | Memindahkan pasien | `InpatientBedOccupancy : Transfer` | Transfer | Penempatan baru | Sudah ada |

#### Health Services / Inpatient Management / Bed Management

Base URL: `api/v1/health-services/inpatient-management/bed-management`

| Method | Path | Kegunaan | Hak akses | Request | Response | Keadaan |
|---|---|---|---|---|---|---|
| `POST` | `/beds/{bedId}/cleaning-attempts` | Mulai pembersihan | `InpatientBedManagement : StartCleaning` | Versi bed | Status bed | Sudah ada |
| `PATCH` | `/cleaning-attempts/{attemptId}/complete` | Selesai pembersihan | `InpatientBedManagement : CompleteCleaning` | Versi bed | Status bed | Sudah ada |
| `POST` | `/beds/{bedId}/readiness-verifications` | Sahkan siap atau belum siap | `InpatientBedManagement : VerifyReadiness` | Hasil dan alasan | Status bed | Sudah ada |

#### Health Services / Inpatient Management / Inpatient Episode

Base URL: `api/v1/health-services/inpatient-management/episodes`

| Method | Path | Kegunaan | Hak akses | Request | Response | Keadaan |
|---|---|---|---|---|---|---|
| `PATCH` | `/{id}/care-dependency` | Mencatat atau mengubah High/Low Dependency | `InpatientEpisode : SetCareDependency` (baru) | `level`, `note` | Ringkasan episode | Rencana (belum tersedia) |
| `GET` | `/{id}/care-dependency-history` | Riwayat perubahan HD/LD | `InpatientEpisode : Read` | — | Daftar riwayat | Rencana (belum tersedia) |

#### Health Services / Master Data / Room

Base URL: `api/v1/health-services/master-data/rooms`

| Method | Path | Kegunaan | Hak akses | Request | Response | Keadaan |
|---|---|---|---|---|---|---|
| `GET` | `/floor-plan-layout` | Kamar dan bed satu lantai untuk editor master, termasuk ruang cadangan | `Room : Read` | Query `floorCode` | Susunan lantai | Rencana (belum tersedia) |
| `POST` | `/floor-plan-layout/import` | Impor susunan kamar dan bed dari denah | `Room : Create` | Manifest denah | Ringkasan impor | Rencana (belum tersedia) |
| `PUT` | `/{id}` | Ubah kamar (kelas, wing) | `Room : Update` | Data kamar | Kamar | Sudah ada; aturan K-05 ditambahkan |

#### Health Services / Master Data / Bed

Base URL: `api/v1/health-services/master-data/beds`

| Method | Path | Kegunaan | Hak akses | Request | Response | Keadaan |
|---|---|---|---|---|---|---|
| `POST` | `/` | Tambah bed | `Bed : Create` | Data bed | Bed | Sudah ada; kode otomatis ditambahkan |
| `PATCH` | `/{id}/availability` | Tutup atau buka bed | `Bed : Update` | Status dan alasan | Bed | Sudah ada |
| `DELETE` | `/{id}` | Hapus bed yang belum pernah dipakai | `Bed : Delete` | — | Hasil hapus | Sudah ada |

Arti kode status bagi pengguna:
- **409** — bed baru saja diambil atau diubah petugas lain; layar memuat ulang lalu petugas memilih
  lagi.
- **422** — pilihan melanggar aturan (jenis kelamin, isolasi, kelas, atau bed tidak siap); pesan
  menyebut aturannya.
- **403** — pengguna tidak punya hak untuk tindakan itu.

### 10.5 Kode kartu yang dihapus

| Berkas | Dipakai oleh | Pengganti |
|---|---|---|
| `src/components/view/health-services/inpatient-management/bed-management/bed-monitoring-cards.jsx` | Tab Monitoring Bed | Denah mode lihat |
| `src/components/features/health-services/inpatient-management/inpatient-bed-board.jsx` (1.261 baris) dan `src/style/.../inpatient-bed-board.module.css` | `inpatient-admission-bed-step.jsx`, `inpatient-bed-transfer.jsx`, `nursing-transfer-form-panel.jsx` | Denah mode pilih |
| `src/lib/hooks/.../use-inpatient-bed-board.jsx` | Papan kartu | Hook data denah per lantai |
| Bagian kartu di `src/style/.../bed-management.module.css` | Kartu monitoring | — |
| `src/components/view/health-services/master-data/bed/master-data-bed-view.jsx` (daftar) | Master Bed | Editor denah |
| Halaman dummy `floor-plan/_data/lantai-*.occupancy.js` | Denah dummy | Data API |
| Test unit yang menguji komponen di atas | — | Disesuaikan atau dihapus |

### 10.6 Verifikasi

- Alur ujung ke ujung di DB Hamzah: admisi → pesan → tempatkan → catat HD/LD → transfer →
  Menunggu Pembersihan → mulai → selesai → sahkan → Tersedia.
- Master: tambah bed, ubah kelas pada kamar kosong, tolak ubah kelas pada kamar berpenghuni,
  tutup/buka bed, aktifkan ruang cadangan.
- Aturan: kamar campur ditolak, isolasi ditolak/diterima, kategori transfer salah ditolak, dan
  identitas penghuni disamarkan bagi yang tidak berhak.
- Tampilan: screenshot denah setiap lantai sebelum dan sesudah tidak berubah selain data. Pop up
  diuji dalam ketiga mode, dan layar 390 px tidak bergeser horizontal.
- Build, lint, dan unit test backend/frontend dibandingkan dengan garis dasar.

## 11. Keputusan yang perlu dijawab pemilik

Setiap keputusan punya satu rekomendasi. Bila tidak ada jawaban, pekerjaan berjalan dengan
rekomendasi tersebut setelah Fase 0 dicatat.

**K-01 — Wing.** Wing dimodelkan sebagai apa, dan kamar mana masuk wing mana?
- **A. Wing = unit layanan (ruang rawat per nurse station) — Rekomendasi.** Hak akses perawat per
  unit sudah ada, sehingga perawat NS3A otomatis hanya mengelola wing A. Pembagian kamar mengikuti
  tabel 8.2 setelah RS mengonfirmasinya.
- **B. Kolom wing terpisah pada kamar.** Lebih sederhana, tetapi hak akses tetap per unit, sehingga
  ada dua pengelompokan yang bisa berbeda.
- **C. Tanpa wing; satu unit per lantai.** Paling sederhana, tetapi perawat satu lantai dapat
  mengelola seluruh lantai.

**K-02 — Penanda bed laki-laki/perempuan.**
- **A. Hapus penanda; larangan kamar campur tetap dari penghuni — Rekomendasi.** Kamar menjadi
  fleksibel tanpa melanggar keputusan PPI/privasi `RWI-DEC-064` (bagian 4).
- **B. Hapus penanda sekaligus larangan kamar campur.** Laki-laki dan perempuan boleh sekamar. Ini
  mengubah keputusan pengendalian infeksi dan privasi, sehingga perlu persetujuan pemilik klinis.
- **C. Pertahankan seperti sekarang.**

**K-03 — High/Low Dependency.** Siapa yang mencatat, dan apakah nilainya membatasi penempatan?
- **A. Perawat penanggung jawab mencatat dan memperbarui, DPJP boleh mengubah, admisi boleh
  merekam catatan awal; tidak membatasi penempatan — Rekomendasi.** Sesuai pernyataan klien bahwa
  HD/LD ditentukan pasien. Nilainya menjadi informasi untuk perawat dan ringkasan kamar.
- **B. Seperti A, tetapi pasien HD hanya boleh ditempatkan di kamar tertentu.** Kamar perlu penanda
  "boleh HD", sehingga bertentangan dengan prinsip HD melekat pada pasien.
- **C. Dihitung otomatis dari pengkajian ketergantungan keperawatan.** Butuh kriteria skor resmi
  dari RS lebih dulu.

**K-04 — Status kamar dan badge.**
- **A. Empat status kamar turunan (bagian 5.3); badge terisi = Terisi + Dipesan; kamar HD bila ada
  satu penghuni HD — Rekomendasi.**
- **B. Seperti A, tetapi badge hanya menghitung Terisi.** Bisa muncul garis merah "penuh" dengan
  badge `3/2`, yang membingungkan.
- **C. Kamar HD bila mayoritas penghuninya HD.**

**K-05 — Ubah kelas kamar yang sedang dihuni.**
- **A. Ditolak sampai kamar kosong dan tanpa pesanan — Rekomendasi.** Kelas penghuni dan
  tagihannya tidak berubah diam-diam.
- **B. Diizinkan dan berlaku untuk penempatan berikutnya.** Penghuni lama tetap di kelas lama, tetapi
  aturannya rumit dan berisiko salah tagih.

**K-06 — Cakupan pembersihan data DB Hamzah.**
- **B. Lengkap, termasuk data klinis/obat/order/tagihan yang merujuk episode rawat inap; master
  kamar/bed uji lama dihapus — Rekomendasi.**
- **A. Sempit, hanya episode dan data bed.** Lebih cepat, tetapi data modul lain dapat merujuk
  episode yang sudah tidak ada.

**K-07 — Sumber master kamar/bed (amandemen `RWI-DEC-048`).**
- **A. Denah resmi menjadi sumber susunan kamar dan bed. Importer terkontrol dipakai di dev/UAT
  sekarang dan dijalankan sekali di produksi oleh admin setelah RS mengesahkan susunannya —
  Rekomendasi.**
- **B. Importer hanya untuk dev/UAT.** Di produksi kamar dan bed diisi manual lewat editor denah.

**K-08 — Urutan kelas (`ClassLevel`) di bagian 8.1.**
- **A. Pakai urutan usulan sebagai awal; validasi lintas kelas aktif setelah tim MasterData
  mengesahkan — Rekomendasi** (`RWI-DEC-291`).
- **B. Pemilik menetapkan urutan sekarang**, mis. posisi Premium, Suite, dan Grand Royal terhadap
  SVIP.

**Fakta yang perlu dari RS MMC (tidak menahan Fase 2):**
- daftar nurse station dan kamar yang dilayaninya;
- nama resmi ruang rawat;
- konfirmasi kamar 520–522 dan dua ruang tanpa nomor di Lantai 3;
- kriteria klinis High/Low Dependency.

## 12. Rujukan

- Keputusan dan aturan terkait: `RWI-RULE-012`, `RWI-DEC-048`, `064`, `066`, `193`, `251`,
  `275`–`294` di `00-interview-decisions.md`; roadmap Bed Management (`BM-DEL-20261011-01`).
- Source: `MstRoom.cs`, `MstBed.cs`, `BedStatus.cs`, `InpBedManagementQueryService.cs`,
  `InpatientBedOccupancyDtos.cs`, `InpBedAccessService.cs`, `InpEpisode.cs`,
  `InpatientEpisodeController.cs`, `InpatientMasterDataSeeder.cs`; frontend `floor-plan/`,
  `bed-monitoring-cards.jsx`, `inpatient-bed-board.jsx`.
- Tingkat perawatan berdasarkan kebutuhan pasien (Level 0–3, *Comprehensive Critical Care*), dikutip
  NCEPOD: <https://ncepod.org.uk/2005report/method.html>
- Klasifikasi ketergantungan pasien (minimal, partial, total care) untuk kebutuhan tenaga perawat:
  <https://jkp.fkep.unpad.ac.id/index.php/jkp/article/download/66/63/125>
- Pedoman HCU, KMK 834/MENKES/SK/VII/2010: <https://pasal.id/peraturan/permen/kepmenkes-834-menkes-sk-vii-2010>
