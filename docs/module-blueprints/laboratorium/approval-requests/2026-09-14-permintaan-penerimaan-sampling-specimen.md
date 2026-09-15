# Permintaan Persetujuan — Menu Penerimaan Sampling/Specimen Laboratorium

| Field | Value |
|---|---|
| `request_id` | `LAB-REQ-005` |
| `tanggal` | 2026-09-14 |
| `pengaju` | Yoga Aji Pratama — Product/Domain Owner Laboratorium (`yogaaji452@gmail.com`) |
| `rujukan` | `00-interview-decisions.md` revision `25`; `01-existing-capability-map.md` revision `3` |
| `status` | `menunggu jawaban` |
| `ditujukan kepada` | Pemilik `master-data`; pemilik `registration-management`; pemilik `billing-kasir` |
| `sifat` | Operasional. **Bukan** artefak desain — tidak masuk daftar hash manifest |

Dokumen ini dapat diteruskan apa adanya. Bagian 3, 4, dan 5 berdiri sendiri: penerima cukup
membaca bagian yang menyebut modulnya.

> **Catatan tentang penerima.** Blueprint Laboratorium belum mencatat nama pemilik untuk ketiga
> modul di atas. `LAB-REQ-001` dahulu dijawab `andryzainhome` (`andryzain01@gmail.com`) dan
> `sukmagp` — Sukma Giri Pratama (`sukmagiri11@gmail.com`) selaku **pemilik repository**. Bila
> wewenang atas ketiga modul ini juga ada pada mereka, cukup dinyatakan; bila tidak, mohon
> diteruskan kepada yang berwenang.

---

## 1. Satu paragraf untuk yang tidak punya waktu

Laboratorium akan membangun menu **Penerimaan Sampling/Specimen** untuk pasien rujukan luar dan
pasien datang langsung. Sebelas keputusan sudah dibuat dan sembilan di antaranya sepenuhnya di
dalam wewenang Laboratorium. **Dua tidak**, dan keduanya menyentuh tabel serta enum milik modul
lain. Tanpa jawaban atas dua hal itu, menunya tetap dapat dibangun — tetapi **dua bagiannya akan
mati**: petugas tidak dapat menerima pasien dari klinik yang belum terdaftar, dan layar tidak
dapat menampilkan siapa yang menanggung biayanya.

Yang diminta di sini **tiga hal**, bukan sebelas. Bagian 3, 4, dan 5.

---

## 2. Kenapa permintaan ini muncul sekarang

Pemilik modul melampirkan artifact **`Penerimaan Sampling Specimen Lab.md`** pada 2026-09-14,
berisi kebutuhan lapangan untuk satu layar penerimaan sampel. Artifact itu diperiksa terhadap
keputusan yang sudah dikunci, dan **tidak satu pun keputusan lama dicabut** — enam butir di
dalamnya justru dikembalikan kepada modul pemiliknya.

Pemeriksaan terhadap source code pada backend `466a7127` dan frontend `9cd4cd03f` kemudian
menemukan dua hal yang tidak diketahui sebelumnya. Keduanya menjadi isi permintaan ini.

---

## 3. Untuk pemilik `master-data` — data induk instansi perujuk tidak dapat diisi siapa pun

### 3.1 Apa yang ditemukan

`MstReferralInstitution` dan `MstReferralDoctor` **sudah ada** sejak `BE-EXT-02`, tabelnya
terdaftar, dan kunjungan sudah menunjuk ke sana. Tetapi:

| Yang diperiksa | Keadaan pada `466a7127` |
|---|---|
| Endpoint baca | Ada — `ReferralInstitutionController.cs`, hanya `GET /options` |
| Endpoint **tulis** | **Tidak ada satu pun.** Tidak ada `POST`, `PUT`, maupun `DELETE` |
| Service tulis | **Tidak ada.** `ReferralMasterDataService.cs` hanya punya dua method baca |
| Pengisi tabel hari ini | `Areas/HealthServices/LaboratoryManagement/Seeders/LabDummyDataSeeder.cs:498,513` |

### 3.2 Kenapa ini masalah, bukan sekadar kekurangan

Aturan `VAL-43` yang berlaku hari ini menolak pendaftaran rujukan bila instansinya belum
terdaftar, dengan pesan: *"Pilih instansi perujuk dari daftar. Bila belum ada, hubungi bagian
data induk untuk menambahkannya."*

**Bagian data induk pun tidak punya layarnya.** Satu-satunya yang mengisi tabel itu hari ini
adalah seeder data contoh — dan seeder itu berada di folder Laboratorium, bukan Master Data.

> **Yang terjadi di meja penerimaan.** Sampel darah dari Klinik Sehat Sentosa tiba pukul 21.10.
> Kliniknya belum ada di daftar. Petugas menekan simpan, ditolak `422`, lalu diminta menghubungi
> bagian data induk yang sudah pulang. Sampelnya sendiri tidak menunggu — ia rusak.

### 3.3 Yang diminta

| No | Yang diminta | Kenapa |
|---:|---|---|
| 1 | **Izin dan komitmen membangun pengelolaan `MstReferralInstitution` dan `MstReferralDoctor`** — tambah, ubah, nonaktifkan | Tanpa ini daftar perujuk hanya dapat diisi lewat seeder atau SQL langsung |
| 2 | **Izin menambah status "menunggu persetujuan"** pada `MstReferralInstitution`, beserta layar persetujuannya | Supaya petugas lab dapat **mengusulkan**, dan penerimaan pasien tidak tertahan |
| 3 | **Kemampuan menggabungkan dua baris** yang ternyata institusi yang sama | Usulan kembar pasti terjadi; tanpa penggabungan, `LAB-DEC-035` gagal pada tujuannya sendiri |

### 3.4 Yang **tidak** diminta

Laboratorium **tidak** meminta kepemilikan data induk ini, **tidak** meminta hak menyetujui
usulannya sendiri, dan **tidak** meminta hak mengubah baris yang sudah disahkan. `LAB-DEC-035`
butir 4 dan `AC-50` tetap berlaku utuh. Yang diminta hanya satu tombol "usulkan" di layar
Laboratorium, dan sisanya dikerjakan Master Data.

### 3.5 Yang perlu Master Data putuskan sendiri

Tiga hal sengaja **tidak** diputuskan Laboratorium karena bukan wewenangnya (`LAB-OPEN-024`):

1. Berapa lama sebuah usulan boleh menggantung tanpa diputuskan.
2. Apa yang terjadi pada kunjungan yang sudah menunjuk usulan, bila usulan itu akhirnya
   **ditolak**.
3. Siapa yang berhak menggabungkan dua baris.

### 3.6 Utang teknis yang dilaporkan apa adanya

Data induk **global** instansi perujuk hari ini diisi oleh seeder milik **Laboratorium**. Itu
melanggar `AC-49`, yang menyatakan data induk global tidak berada di folder modul pemakainya.
Dicatat sebagai `LAB-DEBT-001`. Laboratorium tidak memperbaikinya sepihak karena berkasnya
menyentuh kepemilikan Master Data.

### 3.7 Yang terhalang bila tidak dijawab

`LAB-COORD-006` tetap terbuka. Bagian pendaftaran rujukan pada menu Penerimaan
Sampling/Specimen tidak dapat dirancang, dan formulir rujukan luar `FE-LAB-05` yang sudah
selesai sejak 2026-09-07 **tetap tidak dapat dipakai** — delapan skenario verifikasi manualnya
sudah menunggu sejak tanggal itu karena daftar perujuknya kosong.

---

## 4. Untuk pemilik `registration-management` — `Piutang Mitra` belum punya tempat

### 4.1 Apa yang ditemukan

`EncounterPaymentType@466a7127` hanya mengenal tiga nilai:

| Nilai | Label | Artinya |
|---|---|---|
| `Cash = 1` | Tunai | Pasien membayar sendiri |
| `Insurance = 2` | Asuransi | Ditanggung asuransi |
| `CompanyGuarantor = 3` | Penjamin Perusahaan | Ditanggung **tempat pasien bekerja** |

Rumah sakit menerima pasien dari klinik dan rumah sakit perujuk yang terikat **Perjanjian Kerja
Sama (PKS)**. Tagihannya tidak dibayar pasien di loket, melainkan ditagihkan kepada institusi
perujuk. Tidak ada nilai yang mewakili itu.

### 4.2 Kenapa `CompanyGuarantor` bukan jawabannya

> Penjamin perusahaan menjawab pertanyaan **"di mana pasien ini bekerja"**. Piutang mitra
> menjawab pertanyaan **"siapa yang mengirim pasien ini kepada kami"**.
>
> Bila disatukan, laporan penjamin perusahaan akan memuat PT tempat pasien bekerja dan Klinik
> Sehat Sentosa dalam satu daftar, seolah-olah keduanya hal yang sama. Rumah sakit kemudian
> tidak dapat menjawab dua pertanyaan yang berbeda pemakainya: berapa besar tagihan korporat
> dari perusahaan mitra, dan berapa nilai kerja sama dengan klinik perujuk.

### 4.3 Yang diminta

| No | Yang diminta | Kenapa |
|---:|---|---|
| 4 | **Izin menambah satu nilai baru** pada `EncounterPaymentType` untuk piutang mitra, **secara aditif** — `Cash`, `Insurance`, dan `CompanyGuarantor` tidak bergeser | Agar piutang mitra menjadi penjamin kunjungan tersendiri |
| 5 | **Kesepakatan bahwa Registrasi yang menurunkan** nilai itu dari status PKS instansi perujuk, bukan Laboratorium | Laboratorium tidak boleh membaca penanda PKS maupun menjalankan aturan uang (`BR-25`, `AC-73`) |

**Presedennya sudah ada.** Comment pada enum itu mencatat `CompanyGuarantor = 3` sendiri
ditambahkan lewat `BE-RWI-035` secara aditif, dengan peringatan agar dua nilai sebelumnya tidak
bergeser. Yang diminta mengikuti cara yang sama, bukan membuat pola baru.

**Yang perlu disadari.** Enum ini dibaca setiap modul yang menyentuh penjamin kunjungan, dan
tata kelolanya (`RWI-ENC-PAYER-001`) dipegang Registrasi. Karena itu izinnya diminta, bukan
diasumsikan.

### 4.4 Satu hal yang perlu dikonfirmasi, bukan disetujui

`LabPatientRegistrationDtos.cs:80,83` dan `:120,122` menunjukkan Laboratorium **hari ini sudah
mengirim** `PaymentType` dan `PaymentMethodId` kepada Registrasi saat mendaftarkan pasien.
Keputusan `LAB-DEC-047` mempersempit maknanya:

| Jalur | Perlakuan |
|---|---|
| Pasien **datang langsung** | Petugas lab **tetap boleh** menyatakan penjaminnya — Registrasi yang memvalidasi dan berhak menolak |
| Pasien **rujukan luar** | Yang dikirim Laboratorium **diabaikan**; Registrasi menurunkannya dari status PKS |

Mohon dikonfirmasi bahwa perlakuan ini sesuai dengan aturan Registrasi. Bila Registrasi menilai
petugas lab tidak boleh menyatakan penjamin sama sekali, mohon dinyatakan — konsekuensinya
pasien datang langsung yang membawa kartu asuransi harus kembali mengantre di loket, dan
`LAB-DEC-028` kehilangan sebagian gunanya.

### 4.5 Yang terhalang bila tidak dijawab

`LAB-COORD-007` tetap terbuka. Bagian metode pembayaran pada menu Penerimaan
Sampling/Specimen tidak dapat dirancang.

---

## 5. Untuk pemilik `billing-kasir` — bagaimana piutang mitra ditagihkan

### 5.1 Yang diminta

| No | Yang diminta | Kenapa |
|---:|---|---|
| 6 | **Konfirmasi bahwa piutang mitra ditagihkan Billing**, bukan Laboratorium | Menegakkan `RJ-BIL-GATE-DEC-003` dan `LAB-DEC-029` |
| 7 | **Konfirmasi batas yang dipegang Laboratorium** sebagaimana bagian 5.2 | Agar tidak ada anggapan Laboratorium melangkahi wewenang finansial |

### 5.2 Batas yang dipegang Laboratorium, untuk diperiksa

| Yang **dilakukan** Laboratorium | Yang **tidak pernah** dilakukan Laboratorium |
|---|---|
| Menampilkan harga satuan, jumlah, subtotal, dan total saat memesan | Membentuk, mengubah, atau membatalkan tagihan |
| Menampilkan metode pembayaran secara **baca-saja** | Memutuskan apakah pasien membayar |
| Mengirim fakta kelayakan tagih saat wadah dinyatakan layak | Menerima uang tunai |
| Menyimpan salinan tarif saat kejadian pada baris pemeriksaan | Membaca penanda PKS untuk menyimpulkan sendiri |

Artifact `Penerimaan Sampling Specimen Lab.md` sebetulnya meminta laboratorium **menerima
pembayaran tunai** sebelum pemeriksaan diteruskan. Permintaan itu **ditolak** oleh pemilik modul
Laboratorium sendiri lewat `LAB-DEC-037`, karena melanggar `RJ-BIL-GATE-DEC-003`. Dilaporkan di
sini agar Billing mengetahui bahwa kebutuhan itu ada di lapangan dan belum terlayani.

### 5.3 Satu hal yang perlu Billing putuskan sendiri

Artifact juga menyebut **Promo** yang dapat mengubah grand total. Laboratorium mengeluarkannya
dari scope lewat `LAB-DEC-037` karena menyentuh perhitungan uang. Belum ada keputusan siapa
yang memilikinya. Dilaporkan, tidak diminta.

---

## 6. Yang **tidak** diminta permintaan ini

Agar tidak salah baca, sembilan keputusan berikut sepenuhnya di dalam wewenang Laboratorium dan
**tidak memerlukan persetujuan siapa pun**:

| Keputusan | Isi |
|---|---|
| `LAB-DEC-037` | Batas scope amendment |
| `LAB-DEC-038` | Qty sebagai alat bantu layar yang memecah diri menjadi baris pemeriksaan |
| `LAB-DEC-039` | Titik kunci daftar pemeriksaan pada penetapan kelayakan |
| `LAB-DEC-040` | Jenis specimen menjadi data induk terkendali milik Laboratorium |
| `LAB-DEC-041` | Volume specimen selalu membawa satuannya |
| `LAB-DEC-042` | Waktu sistem dan waktu nyata penerimaan disimpan berdampingan |
| `LAB-DEC-045` | Menu tersendiri, layar pesanan dokter dipertahankan |

`LAB-DEC-039` perlu dicatat khusus: pemeriksaan source menemukan **kode sudah berperilaku
begitu** sejak sebelum keputusan ini dibuat — `LabExaminationService.cs:123` mengunci pada
`Accepted or Rejected` sebagai `VAL-18`. Keputusan itu tidak mengubah perilaku apa pun; ia
membetulkan `AC-20` yang sudah lama tidak sesuai kode.

---

## 7. Ringkasan — tujuh butir yang diminta

| No | Kepada | Yang diminta | Sifat |
|---:|---|---|---|
| 1 | `master-data` | Pengelolaan `MstReferralInstitution` dan `MstReferralDoctor` | Izin + komitmen membangun |
| 2 | `master-data` | Status "menunggu persetujuan" beserta layar persetujuannya | Izin + komitmen membangun |
| 3 | `master-data` | Kemampuan menggabungkan dua baris | Izin + komitmen membangun |
| 4 | `registration-management` | Satu nilai baru `EncounterPaymentType` secara aditif | Izin |
| 5 | `registration-management` | Registrasi yang menurunkan nilai itu dari status PKS | Kesepakatan |
| 6 | `billing-kasir` | Konfirmasi piutang mitra ditagihkan Billing | Konfirmasi |
| 7 | `billing-kasir` | Konfirmasi batas wewenang Laboratorium pada bagian 5.2 | Konfirmasi |

Ditambah satu konfirmasi pada bagian 4.4 dan tiga pertanyaan yang perlu diputuskan Master Data
sendiri pada bagian 3.5.

**Bila butir 1-3 dijawab,** `LAB-COORD-006` ditutup. **Bila butir 4-5 dijawab,** `LAB-COORD-007`
ditutup dan `LAB-DEC-046` naik dari `draft` menjadi `approved`. Setelah keduanya, seluruh menu
Penerimaan Sampling/Specimen dapat dirancang tanpa bagian yang menggantung.

---

## 8. Riwayat Revisi

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 1 | 2026-09-14 | Permintaan dibuka atas `LAB-COORD-006` dan `LAB-COORD-007`, hasil Amendment Pass putaran 2 dan 3 beserta impact scan capability map revision 3 | `menunggu jawaban` |
