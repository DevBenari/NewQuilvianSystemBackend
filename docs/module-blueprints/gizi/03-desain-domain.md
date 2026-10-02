# Desain Domain — Modul Gizi

| Field | Nilai |
|---|---|
| Blueprint ID | `gizi` |
| Revision | `2` — keputusan V1 |
| Status | **`READY`** — 25 September 2026 (sebelumnya `BLOCKED / WAITING BUSINESS DECISION`) |
| Prefix entity | `Gzi` |
| Base URL | `api/v1/health-services/nutrition-management/...` |
| Grup Swagger | `Health Services / Nutrition Management / ...` |
| Dasar | `GIZ-DEC-001` sampai `GIZ-DEC-014` |
| Pemilik proses | Kepala Instalasi Gizi / Kepala Unit Gizi (`GIZ-DEC-014`) |

> ## ✅ READY — blocker desain tertutup
>
> `GIZ-OQ-002`, `GIZ-OQ-004`, dan `GIZ-OQ-006` dijawab pemilik proses pada 25 September 2026 dan
> dicatat sebagai `GIZ-DEC-011`, `GIZ-DEC-012`, serta `GIZ-DEC-014`. Desain entity dan migration
> boleh berjalan.
>
> **Rumus kalkulasi ditunda (`GIZ-OQ-007` DEFERRED).** Pemilik proses memutuskan rumus tidak
> dibuat pada V1. Nilai kebutuhan nutrisi diinput dan difinalisasi ahli gizi. Registry rumus
> tetap dibangun dalam keadaan kosong, supaya penambahan rumus kelak cukup berupa satu baris
> master dan satu kelas perhitungan — bukan pembongkaran tabel.

## Prinsip yang mengikat desain ini

Pemilik proses meminta satu hal secara khusus: **jangan terlalu terikat pada satu rumus atau satu
daftar diet.** Permintaan itu diterjemahkan menjadi tiga aturan yang berlaku di seluruh dokumen.

| Aturan | Wujudnya pada tabel |
|---|---|
| Yang berubah menurut kebijakan rumah sakit disimpan sebagai **baris master**, bukan kolom | Parameter nutrisi, domain diagnosis, jenis diet, rumus — semuanya master |
| Yang berubah menurut waktu disimpan sebagai **revisi**, bukan menimpa | Kebutuhan nutrisi berrevisi; revisi lama tetap terbaca |
| Yang dipakai merawat pasien disimpan **berikut asal-usulnya** | Nilai kalkulasi, nilai final, rumus yang dipakai, masukan rumus, pengubah, dan waktunya |

Akibat praktisnya: menambah serat atau natrium ke daftar parameter kelak adalah **satu baris
master**, bukan kolom baru dan bukan migration. Mengganti rumus adalah **satu baris registry**,
dan nilai lama tetap dapat dijelaskan karena rumus yang dipakainya ikut tercatat.

## Bentuk keseluruhan

```text
RegPatientEncounter  (milik Registration, dibaca saja)
        |
        v
GziNutritionOrder                     satu order aktif per episode rawat inap
  Requested -> InProgress -> Closed | Cancelled
        |
        +-- GziNutritionCareRecord            satu per kunjungan ahli gizi
        |     |
        |     +-- GziNutritionCareRecordDiagnosis  diagnosis IDNT, boleh lebih dari satu
        |     +-- menunjuk revisi kebutuhan yang berlaku
        |     +-- menunjuk baris CPPT
        |
        +-- GziNutritionRequirement           satu revisi kebutuhan nutrisi
        |     +-- GziNutritionRequirementItem   satu baris per parameter
        |
        +-- GziNutritionOrderHistory          jejak perubahan status
        |
        +-- GziPatientDiet                    diet aktif dan riwayatnya
              |
              v
        GziProductionBatch -> GziProductionBatchDetail -> GziMealDelivery

MASTER
  GziNutritionDiagnosisDomain   NI, NC, NB
  GziNutritionDiagnosis         daftar IDNT, diisi admin
  GziNutritionParameter         energi, protein, lemak, karbohidrat, cairan
  GziNutritionFormula           registry rumus, dibangun kosong (GIZ-OQ-007 deferred)
  GziDietType, GziFoodForm, GziMealSchedule
```

## Master diagnosis gizi (`GIZ-DEC-011`)

### GziNutritionDiagnosisDomain

Kelompok besar diagnosis. Dibuat sebagai tabel, bukan enum, supaya domain keempat kelak cukup
ditambahkan sebagai baris — tanpa rilis ulang aplikasi.

| Kolom | Tipe | Keterangan |
|---|---|---|
| `Id` | `Guid` | |
| `DomainCode` | `varchar(20)` | Unik. V1: `NI`, `NC`, `NB` |
| `DomainName` | `varchar(200)` | |
| `Description` | `varchar(1000)?` | |
| `SortOrder` | `int` | |
| `IsActive` | `bool` | |

Tiga barisnya **diisi migration**, karena ketiganya disebut langsung oleh keputusan pemilik
proses — ini menjalankan keputusan, bukan menebak.

### GziNutritionDiagnosis

| Kolom | Tipe | Keterangan |
|---|---|---|
| `Id` | `Guid` | |
| `DiagnosisDomainId` | `Guid` | Menunjuk domain |
| `ParentDiagnosisId` | `Guid?` | Kode IDNT berjenjang, misalnya `NI-5.2` di bawah `NI-5` |
| `DiagnosisCode` | `varchar(30)` | Unik |
| `DiagnosisName` | `varchar(300)` | |
| `Description` | `varchar(1000)?` | |
| `Standard` | `varchar(50)` | Asal baris. V1 `IDNT` |
| `StandardVersion` | `varchar(30)?` | |
| `IsSelectable` | `bool` | Baris induk yang hanya berfungsi sebagai kelompok dimatikan pilihannya |
| `SortOrder`, `IsActive` | | |

**Masternya dibuat kosong.** Baseline IDNT menyebut standar yang dipakai, bukan membebaskan
sistem mengarang isinya. Daftarnya diimpor admin gizi.

**Kenapa bukan menumpang `MstDiagnosis`.** Alasannya ada tiga dan seluruhnya dapat diperiksa pada
source: `MstDiagnosis` berbentuk ICD dan memuat `IcdVersion`, `DiagnosisChapterId`, serta
`IsPrimaryDiagnosisAllowed` yang tak satu pun berlaku bagi diagnosis gizi; menambah kolom `Domain`
di sana berarti mengubah master milik modul MasterData demi satu modul lain; dan baris IDNT yang
tercampur akan ikut muncul di layar modul lain yang membaca `MstDiagnosis` tanpa menyaring tipe.
Perpindahan ini menggantikan `GIZ-DEC-009` dan berbiaya nol karena kedua tabel masih kosong.

### GziNutritionCareRecordDiagnosis

Diagnosis yang ditegakkan pada satu kunjungan. Tabel anak, bukan satu kolom.

| Kolom | Tipe | Keterangan |
|---|---|---|
| `Id` | `Guid` | |
| `CareRecordId` | `Guid` | |
| `NutritionDiagnosisId` | `Guid` | |
| `IsPrimary` | `bool` | Paling banyak satu per kunjungan, ditegakkan indeks tersaring |
| `Note` | `varchar(1000)?` | |
| `SortOrder` | `int` | |

**Kenapa tabel anak.** Praktik IDNT lazim menegakkan lebih dari satu diagnosis dalam satu
kunjungan. Menyimpannya sebagai satu kolom berarti perpindahan ke banyak diagnosis kelak menuntut
pemindahan data historis. Sekarang tabelnya masih kosong, jadi ongkos memilih bentuk yang benar
adalah nol.

## Kebutuhan nutrisi (`GIZ-DEC-012`)

### GziNutritionParameter

| Kolom | Tipe | Keterangan |
|---|---|---|
| `Id` | `Guid` | |
| `ParameterCode` | `varchar(30)` | Unik |
| `ParameterName` | `varchar(200)` | |
| `UnitCode` | `varchar(30)` | Satuan yang ditampilkan dan disimpan |
| `ValueScale` | `int` | Jumlah angka di belakang koma saat ditampilkan |
| `MinValue`, `MaxValue` | `numeric(12,3)?` | Batas wajar. Kosong berarti belum ditetapkan siapa pun |
| `SortOrder`, `IsActive` | | |

Lima barisnya diisi migration sesuai keputusan:

| Kode | Nama | Satuan | Batas |
|---|---|---|---|
| `ENERGY` | Energi | `kkal/hari` | 1 – 10000, dari aturan `GIZ006` yang sudah ada |
| `PROTEIN` | Protein | `gram/hari` | belum ditetapkan |
| `FAT` | Lemak | `gram/hari` | belum ditetapkan |
| `CARBOHYDRATE` | Karbohidrat | `gram/hari` | belum ditetapkan |
| `FLUID` | Cairan | `ml/hari` | belum ditetapkan |

Batas empat parameter terakhir **sengaja dikosongkan**. Batas energi berasal dari aturan yang
memang sudah disepakati; batas yang lain belum ditetapkan siapa pun, dan mengarangnya berarti
sistem menolak angka yang barangkali benar secara klinis.

### GziNutritionFormula

Registry rumus. **Dibangun kosong; pengisiannya ditunda (`GIZ-OQ-007` DEFERRED).**

| Kolom | Tipe | Keterangan |
|---|---|---|
| `Id` | `Guid` | |
| `FormulaCode` | `varchar(50)` | Unik |
| `FormulaName` | `varchar(200)` | |
| `FormulaVersion` | `varchar(30)` | Rumus yang direvisi menjadi baris baru, bukan menimpa |
| `Description` | `varchar(2000)?` | |
| `SourceReference` | `varchar(500)?` | Sumber resmi rumus, diisi admin saat mendaftarkan |
| `ImplementationKey` | `varchar(100)` | Kunci yang menghubungkan baris ini ke kelas perhitungan terdaftar di kode |
| `IsActive` | `bool` | |

**Kenapa registry, bukan rumus di dalam kode perhitungan.** Rumus kebutuhan gizi berbeda antar
rumah sakit dan antar kondisi pasien. Menanamkan satu rumus berarti satu kekeliruan berdampak
pada seluruh pasien sekaligus, dan kekeliruan itu sulit terlihat karena hasilnya tetap tampak
masuk akal. Dengan registry, rumus yang dipakai tercatat pada setiap nilai yang dihasilkannya.

**Kode tidak memuat satu pun rumus pada V1**, sesuai keputusan menunda `GIZ-OQ-007`. Yang ada
hanyalah antarmuka dan pencari implementasi. Bila tidak ada baris rumus aktif, kalkulasi tidak
dijalankan dan nilai kalkulasi dibiarkan kosong — bukan diisi angka asal, dan ahli gizi mengisi
nilai final sendiri.

### GziNutritionRequirement

Satu **revisi** kebutuhan nutrisi untuk satu order.

| Kolom | Tipe | Keterangan |
|---|---|---|
| `Id` | `Guid` | |
| `NutritionOrderId` | `Guid` | |
| `CareRecordId` | `Guid?` | Kunjungan yang melahirkan revisi ini |
| `RevisionNumber` | `int` | Mulai dari 1 |
| `IsCurrent` | `bool` | Paling banyak satu yang berlaku per order |
| `EffectiveFrom` | `timestamptz` | |
| `CalculationFormulaId` | `Guid?` | Rumus yang dipakai. Kosong berarti tanpa kalkulasi |
| `CalculationInput` | `jsonb?` | Salinan masukan rumus: berat, tinggi, umur, faktor |
| `CalculationPerformedAt` | `timestamptz?` | |
| `DeterminedByWorkforceId` | `Guid` | Ahli gizi yang menetapkan revisi |
| `ChangeReason` | `varchar(1000)?` | Wajib mulai revisi kedua |
| `Version` | `int` | Token konkurensi |

**Kenapa masukan rumus disalin.** Berat dan tinggi pasien berubah selama perawatan. Bila masukan
tidak disalin, angka kebutuhan lama tidak lagi dapat dijelaskan — hasilnya ada, tetapi tidak ada
yang tahu dari mana. Penyalinan saat transaksi seperti ini sah menurut aturan data bersama.

### GziNutritionRequirementItem

| Kolom | Tipe | Keterangan |
|---|---|---|
| `Id` | `Guid` | |
| `NutritionRequirementId` | `Guid` | |
| `NutritionParameterId` | `Guid` | Unik berpasangan dengan induknya |
| `CalculatedValue` | `numeric(12,3)?` | Hasil rumus. Kosong bila tidak ada rumus terdaftar |
| `FinalValue` | `numeric(12,3)` | Nilai yang dipakai merawat pasien |
| `AdjustmentReason` | `varchar(1000)?` | Wajib bila final berbeda dari kalkulasi |
| `AdjustedByWorkforceId` | `Guid?` | |
| `AdjustedAt` | `timestamptz?` | |

Kelima hal yang diminta pemilik proses — nilai kalkulasi, nilai final, alasan, pelaku, waktu —
disimpan **per parameter**, bukan per revisi. Ahli gizi lazimnya mengoreksi satu atau dua
parameter saja, dan alasan yang menempel pada revisi tidak dapat menerangkan parameter mana yang
dimaksud.

## Perubahan pada entity yang sudah ada

| Entity | Perubahan | Alasan |
|---|---|---|
| `GziNutritionCareRecord` | `NutritionDiagnosisId` **dicabut**, diganti tabel anak `GziNutritionCareRecordDiagnosis` | `GIZ-DEC-011`: diagnosis pindah master dan boleh lebih dari satu |
| `GziNutritionCareRecord` | `DietPrescription varchar(500)` **dicabut**, diganti `PatientDietId Guid?` | `GIZ-DEC-012`: diet dari master, bukan teks bebas |
| `GziNutritionCareRecord` | `EnergyRequirementKcal` **dicabut**, diganti `NutritionRequirementId Guid?` | Kebutuhan tidak lagi satu angka, dan pemiliknya tabel kebutuhan |
| `GziPatientDiet` | ditambah `NutritionRequirementId Guid?` | Menautkan diet ke revisi kebutuhan yang mendasarinya |
| `GziPatientDiet` | `EnergyRequirementKcal` **dipertahankan** sebagai salinan | Dapur memasak dari angka saat pesanan dibuat. Ini salinan yang sengaja, sejalan dengan `EnergyRequirementKcalSnapshot` pada `GziProductionBatchDetail` |

Seluruh pencabutan aman karena seluruh tabel Gizi berisi **0 baris** pada database uji. Tidak ada
data yang hilang.

## Aturan validasi

Yang sudah ada dipertahankan; `GIZ005` berubah rujukan masternya.

| Kode | Aturan |
|---|---|
| `GIZ005` | Diagnosis harus baris `GziNutritionDiagnosis` yang aktif dan dapat dipilih |
| `GIZ006` | Nilai final harus berada di dalam batas master parameter, bila batasnya ada |
| `GIZ014` | Parameter yang dikirim harus aktif pada master |
| `GIZ015` | Seluruh parameter aktif wajib punya nilai final pada satu revisi |
| `GIZ016` | Alasan perubahan wajib bila nilai final berbeda dari nilai kalkulasi |
| `GIZ017` | Alasan revisi wajib mulai revisi kedua |
| `GIZ018` | Paling banyak satu diagnosis primer per kunjungan |
| `GIZ019` | Rumus yang dirujuk harus terdaftar dan aktif |
| `GIZ020` | Satu order hanya boleh punya satu revisi kebutuhan yang berlaku |

## Kontrak API tambahan

Base: `api/v1/health-services/nutrition-management`

| Metode | Alamat | Guna |
|---|---|---|
| `GET`/`POST` | `/masters/nutrition-diagnosis-domains` | Master domain diagnosis |
| `GET`/`POST` | `/masters/nutrition-diagnoses` | Master diagnosis IDNT, dengan saring domain dan pencarian kode |
| `GET`/`POST` | `/masters/nutrition-parameters` | Master parameter nutrisi |
| `GET`/`POST` | `/masters/nutrition-formulas` | Registry rumus |
| `GET` | `/orders/{id}/requirements` | Seluruh revisi kebutuhan, terbaru di atas |
| `GET` | `/orders/{id}/requirements/current` | Revisi yang berlaku |
| `POST` | `/orders/{id}/requirements` | Menetapkan revisi baru |
| `POST` | `/orders/{id}/requirements/calculate` | Pratinjau kalkulasi tanpa menyimpan |

Seluruh perintah yang mengubah data membawa `idempotencyKey` dan `expectedVersion`, mengikuti pola
yang sudah berjalan.

## Hak akses

| Controller | Aksi |
|---|---|
| `NutritionOrder` | `Read`, `Create`, `Update`, `Cancel` |
| `NutritionCareRecord` | `Read`, `Update` |
| `NutritionRequirement` | `Read`, `Update` |
| `NutritionMaster` | `Read`, `Update` |

Izin diberikan lewat `SysAccessPolicy` per Departemen dan Jabatan, bukan per orang. `GIZ-DEC-014`
menetapkan pemilik prosesnya, **bukan** permission baru: baris jabatan "Kepala Instalasi Gizi"
adalah data master yang keberadaannya tidak dapat dipastikan dari source, sehingga tidak
diterjemahkan menjadi kode.

## Yang sengaja tidak dibuat

| Yang ditolak | Alasan |
|---|---|
| Entity skrining gizi | Sudah ada di `TrxPatientAssessment` |
| Rumus kebutuhan gizi di dalam kode | `GIZ-OQ-007` ditunda atas keputusan pemilik proses. Tempatnya disiapkan, isinya tidak dikarang |
| Enum diagnosis gizi | Isi master adalah data; enum menjadikannya ketetapan kode |
| Batas wajar protein, lemak, karbohidrat, cairan | Belum ditetapkan siapa pun |
| Kolom tetap per zat gizi | Melanggar permintaan agar penambahan parameter cukup lewat master |
| Menu, siklus menu, standar porsi, resep, stok bahan | Di luar scope `GIZ-DEC-013` |
