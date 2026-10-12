# Keputusan yang menunggu pemilik proses — Farmasi, Gizi, Operasi

Disusun 7 Oktober 2026. Berkas ini bukan analisis terbuka: setiap bagian sudah diselesaikan
sejauh yang dapat ditentukan dari kontrak dan source hari ini, dan yang tersisa **hanya pilihan**
beserta rekomendasi paling aman. Tujuannya supaya pemilik proses tinggal memilih, tanpa
investigasi ulang.

| Nomor | Hal | Modul | Bentuk keputusan |
|---|---|---|---|
| [K-1](#k-1) | Nilai kolom pembayaran resep setelah clearance dicabut | Farmasi | pilih satu dari tiga |
| [K-2](#k-2) | Cakupan keunikan nomor serial implant | Operasi | pilih satu dari tiga |
| [K-3](#k-3) | Menutup race nomor serial implant | Operasi | pilih satu dari tiga |
| [K-4](#k-4) | Konsumen pertama fakta Gizi | Gizi | sebutkan tujuannya |
| [K-5](#k-5) | Interval skrining ulang gizi (`GIZ-OQ-005`) | Gizi | pilih satu dari tiga |

---

## Yang TIDAK lagi menjadi pertanyaan

Tiga hal yang sebelumnya tercatat sebagai keputusan terbuka ternyata **sudah terjawab di
source**, dan karena itu dikeluarkan dari daftar:

### Pengakuan surat clearance — sudah terkunci

`PrescriptionFinancialClearanceService.ConsumeAsync` sudah menetapkan seluruhnya:

| Keadaan | Perilaku |
|---|---|
| Surat bernomor versi lebih lama | diabaikan tanpa suara — surat dapat tiba tidak berurutan, dan yang lebih tua harus kalah |
| `CLEARED` dengan `FinancialOutcome` dikenali | salinan ditulis, `PaymentStatus` disalin dari hasil Billing, resep dilepas ke antrean |
| `CLEARED` dengan `FinancialOutcome` **tidak** dikenali | **fail-closed** (`PHA-DEC-067`): salinan dicatat apa adanya, tetapi **tidak** dijadikan izin dan resep **tidak** dipindahkan. Farmasi tidak menebak |

### Apakah pemenuhan ditarik mundur saat dicabut — sudah terkunci

**Tidak.** `PHA-DEC-069`: pencabutan tidak menurunkan `FulfillmentStatus` dan tidak mengembalikan
racikan menjadi bahan. Resep yang sudah bergerak lebih jauh dibiarkan pada keadaannya, dan surat
pemulihan tidak mengulang antrean, telaah, maupun penyiapan yang sudah selesai. Penahanan
pekerjaannya dipasang `PHA-BE-005` sebagai gerbang yang membaca salinan proyeksi.

Alasannya masuk akal dan sebaiknya tidak diubah: menurunkan status akan membuang pekerjaan
apoteker yang sudah sah dikerjakan, dan racikan yang sudah dicampur tidak dapat dipisahkan lagi.

### Cakupan keunikan serial implant V1 — sudah terkunci

Satu kasus operasi, dan itu memang disengaja: implant yang sama dapat muncul lagi pada kasus
lain, misalnya saat revisi implant yang gagal. Yang belum diputuskan hanya apakah cakupannya
perlu **diperluas** — itu [K-2](#k-2).

---

## K-1

### Nilai kolom pembayaran resep setelah clearance dicabut

**Keadaan sekarang.** Pada cabang `REVOKED`, `ConsumeAsync` **tidak menyentuh resep sama sekali**:

```csharp
else
{
    // Pencabutan tidak menurunkan keadaan pemenuhan dan tidak mengembalikan racikan
    // menjadi bahan. Penahanan pekerjaannya dipasang PHA-BE-005 sebagai gerbang yang
    // membaca salinan ini; keadaan pemenuhan sengaja dibiarkan apa adanya.
    result.Revoked++;
}
```

Akibatnya `prescription.PaymentStatus` **tetap bernilai lama** — misalnya `Paid` — padahal izinnya
sudah dicabut. Keadaan yang otoritatif ada di proyeksi (`ClearanceStatus = REVOKED`), dan gerbang
`PHA-BE-005` membaca proyeksi itu, bukan kolom ini. Jadi **keselamatannya tidak terganggu**: obat
tetap tertahan.

**Yang terganggu adalah apa yang dibaca manusia.** Layar atau laporan yang menampilkan
`PaymentStatus` akan menyebut resep itu "Lunas" padahal pembayarannya sudah dibalik.

**Catatan penting soal nilai yang tersedia.** `PrescriptionPaymentStatus` memuat `NotBilled`,
`BillingGenerated`, `WaitingForPayment`, `PartiallyPaid`, `Paid`, `InsuranceApproved`,
`PaymentWaived`. **Tidak ada nilai yang berarti "pernah lunas lalu dicabut".**

| Opsi | Isi | Konsekuensi |
|---|---|---|
| **A** (rekomendasi) | biarkan apa adanya, dan **larang** layar memakai `PaymentStatus` sebagai sumber kebenaran — gerbang dan tampilan membaca proyeksi | Nol perubahan perilaku, nol risiko. Yang perlu dikerjakan hanya frontend dan laporan. Kolom itu memang sudah dinamai "salinan kenyamanan" di source |
| **B** | setel ulang ke `WaitingForPayment` saat dicabut | Layar langsung benar tanpa perubahan frontend. Tetapi **menghapus jejak** bahwa resep itu pernah lunas, dan `WaitingForPayment` berarti "belum pernah dibayar" — itu tidak jujur untuk pembayaran yang dibalik |
| **C** | tambah nilai enum baru, misalnya `ClearanceRevoked` | Paling jujur secara semantik. Tetapi menambah nilai enum menyentuh migration, kontrak API, dan setiap pembaca yang melakukan `switch` atas enum ini — termasuk modul lain |

**Rekomendasi: A.** Risikonya nol dan ia tidak menyembunyikan apa pun. Kalau nanti terbukti ada
layar yang benar-benar tidak dapat membaca proyeksi, C lebih baik daripada B — B menukar satu
kesalahan tampilan dengan kesalahan data.

---

## K-2

### Cakupan keunikan nomor serial implant

V1 menegakkan keunikan **dalam satu kasus operasi** (`OPR014`), dan hanya atas baris `Used` yang
belum digantikan koreksi. `Returned` dan `Wasted` sengaja tidak menutup nomor serinya, karena
keduanya justru menyatakan implant itu tidak jadi terpasang. Dijaga 8 uji pada
`MaterialSerialTests`.

| Opsi | Cakupan | Konsekuensi |
|---|---|---|
| **A** (rekomendasi) | tetap satu kasus operasi | Nol perubahan. Benar untuk revisi implant: implant yang sama memang dapat dicatat lagi pada kasus berbeda |
| **B** | satu pasien | Menangkap pencatatan ganda antar kasus pada pasien yang sama, tetapi menolak revisi implant yang sah pada pasien itu kecuali diberi pengecualian |
| **C** | seluruh rumah sakit | Paling ketat, dan menangkap serial yang tertukar antar pasien. Tetapi setiap implant yang dilepas lalu dipasang kembali — dan setiap kekeliruan input lama — akan mengunci nomor itu selamanya |

**Rekomendasi: A**, sampai ada kejadian nyata yang menunjukkan sebaliknya. Kalau pernah terjadi
satu serial tercatat pada dua pasien, B menjadi pilihan yang tepat, bukan C.

---

## K-3

### Menutup race nomor serial implant

**Kesimpulan teknis: unique partial index PostgreSQL TIDAK dapat merepresentasikan aturan ini,
dan memaksakannya akan salah.**

Sebabnya spesifik. Aturannya bukan "serial unik per kasus", melainkan "serial unik per kasus di
antara baris `Used` yang **belum digantikan koreksi**". Syarat terakhir itu menuntut pemeriksaan
**antar baris** pada tabel yang sama:

```csharp
!_dbContext.OprMaterialUsages.Any(koreksi =>
    koreksi.CorrectionOfUsageId == x.Id && !koreksi.IsDelete)
```

Saringan partial index hanya dapat menyebut kolom pada baris itu sendiri. Memasang indeks tanpa
syarat itu akan **menolak koreksi yang sah** — pencatatan yang salah akan mengunci nomor serinya
selamanya, yang persis dihindari desain sekarang.

Karena itu `OPR014` dipertahankan di tingkat aplikasi. Risiko sisanya **nyata dan perlu diakui**:
dua permintaan bersamaan dapat lolos pemeriksaan bersama-sama lalu keduanya tersimpan. Jendelanya
sempit — antara pembacaan `AnyAsync` dan `SaveChanges` — tetapi tidak nol.

| Opsi | Isi | Konsekuensi |
|---|---|---|
| **A** (rekomendasi) | terima risiko sisanya, dokumentasikan, tangani lewat koreksi | Nol perubahan. Dampak sebuah race adalah satu baris pemakaian ganda yang **dapat dikoreksi** — bukan obat yang salah diberikan, bukan tagihan yang hilang. Dua perawat mencatat serial yang sama pada kasus yang sama dalam hitungan milidetik juga bukan pola kerja nyata |
| **B** | advisory lock PostgreSQL atas `(caseId, serial)` | Menutup race-nya bersih **secara konsep**, pola yang sama sudah dipakai `DrugReturnService` dan `BilConsumerHandoffService`. **Tetapi tidak bersih untuk dipasang sekarang**: `CreateUsageAsync` tidak membuka transaksi eksplisit, sehingga `pg_advisory_xact_lock` akan dilepas segera setelah pernyataannya selesai dan tidak menjaga apa pun. Memasangnya menuntut pembungkusan transaksi baru pada jalur yang sudah memuat logika replay/idempotensi dan efek samping outbox — perubahan batas transaksi yang tidak dapat dinyatakan benar hanya dengan membacanya |
| **C** | reservasi nomor serial sebagai tabel tersendiri | Menutup race dengan indeks unik biasa pada tabel reservasi, tanpa menyentuh aturan koreksi. Tetapi menambah tabel, migration, dan satu keadaan baru yang harus dibersihkan bila pencatatannya gagal di tengah |

**Rekomendasi: A sekarang, B bila Anda menghendaki jaminannya.** Saya tidak memasang B tanpa
persetujuan karena ia mengubah batas transaksi pada jalur milik orang lain; kalau Anda setujui,
pekerjaannya jelas: buka transaksi di awal `CreateUsageAsync`, ambil
`pg_advisory_xact_lock(hashtext(caseId || serial))` sebelum pemeriksaan, commit setelah
`SaveChanges`, dan tambahkan tiruan `hashtext`/`pg_advisory_xact_lock` pada `TestDatabase` Operasi
seperti yang sudah ada pada `TestDatabase` Farmasi.

---

## K-4

### Konsumen pertama fakta Gizi

**Keadaan sekarang: Gizi nol menerbitkan fakta.** Tidak ada producer, tidak ada outbox, tidak ada
satu pun jalur keluar. Diet yang dibaca dapur berhenti di dalam modul.

Yang **tidak** dapat saya tentukan sendiri: **siapa yang mengonsumsi dan untuk apa.** Tanpa itu,
sebuah producer hanya akan menulis ke tabel yang tidak dibaca siapa pun — pekerjaan yang terlihat
selesai tetapi tidak menghasilkan apa pun, dan bentuk payload-nya hampir pasti keliru karena
dikarang tanpa pembacanya.

**Pola yang tersedia untuk dipakai**, supaya tidak ada yang perlu dirancang dari nol:

| Pola | Contohnya | Bentuknya |
|---|---|---|
| `ClinicalMilestoneFactProducer` | Clinical, dipakai Farmasi dan Operasi | menerbitkan fakta klinis, lalu `BillingFolioService` mengubahnya menjadi tagihan |
| `InpIntegrationOutbox` | Rawat Inap | outbox tabel dengan worker pengirim dan penanda penerimaan |

**Antarmuka yang dibutuhkan, konkret.** Begitu Anda menyebut tujuannya, yang perlu ditetapkan
hanya empat hal — dan ketiga yang pertama sudah dapat dibaca dari source Gizi hari ini:

1. **Peristiwa pemicunya.** Kandidat yang sudah ada keadaannya di source: diet pasien ditetapkan
   (`GziPatientDiet` menjadi `Active`), diet dihentikan, diet diverifikasi dokter penetap
   (`VerifyDietInstructionAsync`), dan batch produksi didistribusikan.
2. **Isi faktanya.** Sudah tersedia: `PatientId`, `EncounterId`, jenis diet, bentuk makanan,
   jadwal makan, jumlah porsi, waktu berlaku.
3. **Kunci idempotensinya.** Pola yang sudah terbukti di Farmasi: GUID deterministik dari
   `SHA256("gzi-diet|{dietId}|{version}")`.
4. **Tujuan dan maknanya bagi tujuan itu** — **ini yang hanya Anda dapat tentukan.**

Dua kandidat tujuan yang paling mungkin, beserta akibatnya:

| Tujuan | Maknanya | Yang perlu disiapkan pihak sana |
|---|---|---|
| **Billing** | diet pasien menjadi tagihan makanan | kategori tarif gizi, dan keputusan apakah ditagih per porsi atau per hari |
| **Rawat Inap / papan perawatan** | diet aktif tampil pada layar perawatan pasien | pembaca pada layar itu; tidak ada tagihan yang terlibat |

**Rekomendasi: sebutkan Billing lebih dulu bila diet memang ditagih**, karena di situlah
kehilangan uang terjadi bila faktanya tidak terbit. Bila diet **tidak** ditagih, tujuan papan
perawatan jauh lebih murah dan tidak menyentuh uang sama sekali.

---

## K-5

### Interval skrining ulang gizi (`GIZ-OQ-005`)

**Hasil audit: tidak ada angka di mana pun.** Blueprint Gizi, keputusan `GIZ-DEC-*` yang sudah
disetujui, dan source tidak memuat satu pun interval skrining ulang untuk pasien yang pada
skrining awal dinyatakan **tidak berisiko**. Jadi angkanya tidak dapat diturunkan dari apa pun
yang sudah ada, dan saya tidak mengarangnya.

| Opsi | Interval | Konsekuensi |
|---|---|---|
| **A** (rekomendasi) | **7 hari** | Sejalan dengan praktik yang umum dipakai pada rawat inap, dan satu minggu cukup menangkap penurunan gizi yang berkembang perlahan tanpa membebani ahli gizi. Pasien yang pulang sebelum tujuh hari tidak pernah terkena skrining ulang |
| **B** | 3 hari | Lebih aman secara klinis, tetapi beban kerjanya kira-kira dua kali lipat dan sebagian besar hasilnya akan tetap "tidak berisiko" |
| **C** | tidak ada skrining ulang otomatis; hanya atas permintaan dokter atau perawat | Nol beban tambahan, tetapi penurunan gizi yang berkembang perlahan dapat terlewat sepenuhnya — dan itu justru kelompok yang paling mungkin terlewat tanpa jadwal |

**Rekomendasi: A.** Tetapi ini keputusan klinis, bukan teknis, dan sebaiknya disahkan pemilik
proses gizi — bukan saya. Angka berapa pun yang Anda pilih dapat dipasang sebagai konfigurasi,
bukan konstanta, sehingga dapat disesuaikan tanpa rilis baru.

**`GIZ-OQ-007` (rumus kalkulasi kebutuhan nutrisi) tetap TIDAK diisi**, sesuai instruksi, sampai
ada keputusan eksplisit.
