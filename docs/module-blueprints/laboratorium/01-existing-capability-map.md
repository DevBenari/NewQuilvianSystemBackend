# Laboratorium — Peta Kemampuan Existing

| Field | Value |
|---|---|
| Blueprint ID | `laboratorium` |
| Revision | `4` |
| Status | `draft` — **peta revision 1-2 `STALE`; sebagian revision 3 juga `STALE`**, lihat Impact Scan Revision 4 |
| Jenis audit | Revision 1: audit penuh. Revision 2: *impact scan* terbatas. Revision 3: *impact scan* terbatas atas kemampuan yang terdampak `LAB-DEC-037`..`LAB-DEC-045`. **Revision 4: *impact scan* terbatas atas permukaan yang disentuh `LAB-DEC-095`..`LAB-DEC-110`** |
| Sifat audit | **Read-only.** Tidak ada satu baris source aplikasi yang diubah |
| Product/domain owner | Yoga Aji Pratama (`yogaaji452@gmail.com`) |
| Backend SHA | Diaudit pada `466a7127` (branch `yoga`). Revision 1-2 diaudit pada `c87d9c0`; **298 commit** di antaranya. **`HEAD` bergeser ke `9067fa73` saat sesi 2026-09-14 berjalan** — `git diff 466a7127..9067fa73` atas `Areas/HealthServices/LaboratoryManagement`, `MstReferralInstitution.cs`, `ReferralInstitutionController.cs`, `EncounterPaymentType.cs`, dan `RegPatientEncounterGuarantor.cs` **kosong**, sehingga seluruh temuan di bawah tetap sahih |
| Frontend SHA | `9cd4cd03f` — `HEAD` pada 2026-09-14. Revision 1-2 diaudit pada `688daff90`; **155 commit** di antaranya |
| Masukan | Revision 1-2: `00-interview-decisions.md` revision 7. **Revision 3: revision 23**, keputusan `LAB-DEC-037` sampai `LAB-DEC-045` |
| Tanggal audit | Revision 1: 2026-09-01. Revision 2: 2026-09-02. Revision 3: 2026-09-14. **Revision 4: 2026-09-21** |
| Backend SHA revision 4 | `981e002c` (branch `yoga`). **149 commit** sejak `466a7127` |
| Frontend SHA revision 4 | `ebef7ebe5`. **111 commit** sejak `9cd4cd03f` |
| Masukan revision 4 | `00-interview-decisions.md` **revision 49**, keputusan `LAB-DEC-095` sampai `LAB-DEC-110` (amendment pass putaran 9, halaman Hasil Mikrobiologi) |

> **Cara membaca dokumen ini.**
> Dokumen ini menjawab pertanyaan "apa yang sudah ada di sistem", bukan "aturan bisnisnya
> bagaimana". Setiap baris membawa bukti berupa lokasi berkas dan nama simbol pada commit
> tertentu, supaya siapa pun bisa memeriksa ulang. Dokumen ini **tidak** merancang arsitektur
> dan **tidak** memberi izin menulis kode.

---

## Impact Scan Revision 4 — 2026-09-21

**Pemicu.** Amendment pass putaran 9 mengambil enam belas keputusan (`LAB-DEC-095`..`LAB-DEC-110`)
di atas peta yang dikunci pada BE `466a7127` + FE `9cd4cd03f`. Wawancara itu sendiri sudah
menandai peta berpotensi basi dan meminta scan ini dijalankan sebelum butir mana pun turun
menjadi task.

**Besar pergeserannya.** Backend **149 commit**, frontend **111 commit**. Area
`Areas/HealthServices/LaboratoryManagement` sendiri bertambah **+9.800 baris pada 55 berkas**
(`git diff --stat 466a7127..981e002c`).

> ### Kesimpulan pendek
>
> **Satu fakta dasar peta revision 1-3 sekarang SALAH TOTAL.** `F5` menyatakan *"frontend belum
> punya modul Laboratorium sama sekali"*. Hari ini frontend punya **17 route** dan **159 berkas**
> Laboratorium, termasuk route `lab-monitoring/microbiology`.
>
> **Satu keputusan putaran 9 berdiri di atas fakta yang salah dan harus dibuka ulang:**
> `LAB-DEC-108` memilih `MstDoctorSchedule` sebagai sumber "dokter yang sedang bertugas".
> Tabel itu **jadwal praktik poliklinik**, bukan daftar dokter jaga. Dibuka sebagai
> `LAB-CONFLICT-010`.
>
> **Satu keputusan justru lebih mudah dari dugaan:** `LAB-DEC-100` membutuhkan satuan baru pada
> `MstMeasurement`, dan tabel itu **punya endpoint tulis lengkap** — tidak mengulang jalan buntu
> `LAB-COORD-006` maupun `MST-POS-WRITE`.
>
> **Empat belas keputusan sisanya terbukti berdiri di atas fakta yang masih benar.**

### Bagian A — Fakta peta lama yang kini basi

| Fakta lama | Keadaan pada `981e002c` / `ebef7ebe5` | Status |
|---|---|---|
| `F5` — frontend nol modul Laboratorium | **17 route** di bawah `src/app/health-services/laboratory-management/`, **159 berkas** `src/**/*lab*`. Route `lab-monitoring/microbiology/page.jsx` memakai `components/view/health-services/laboratory-management/lab-monitoring/lab-monitoring-view` | ❌ **STALE — salah total** |
| Revision 3 butir 2 — `LabSpecimen` hanya punya `SpecimenDescription` teks bebas | `Models/LabSpecimen.cs:65` `SpecimenTypeId`, `:74` `SpecimenTypeOtherNote`, `:85` `VolumeAmount`, `:96` `VolumeUnitId`, `:137` `PhysicallyReceivedAt` — seluruhnya sudah dibangun | ❌ **STALE — sudah dikerjakan** |
| `LabOrder` nol kolom nomor order (`LAB-DEC-072`) | `Models/LabOrder.cs:38` `OrderNumber`, dialokasikan `Services/LabOrderNumberService.cs` | ❌ **STALE — sudah dikerjakan** |
| Data induk jenis specimen belum ada (`LAB-DEC-040`) | `Models/LabSpecimenType.cs` berdiri; `Seeders/LabSpecimenTypeSeeder.cs:37-43` mengisi **tepat tujuh nilai** — `BLOOD`, `URINE`, `BODYFLUID`, `SPUTUM`, `PUS`, `TISSUE`, dan `OTHER` bertanda `IsOtherBucket` | ❌ **STALE — sudah dikerjakan persis sesuai keputusan** |

### Bagian B — Verifikasi enam belas keputusan putaran 9

| Keputusan | Yang diandaikan | Keadaan pada `HEAD` | Klasifikasi |
|---|---|---|---|
| `LAB-DEC-095` hasil per pemeriksaan | `LabExamination` menjadi tempat hasil Mikrobiologi | `Models/LabExamination.cs:128-186` punya `ResultNumeric`, `ResultOptionId`, `ResultValueBoundId`, `ResultUnitSnapshot`, `ExaminedAt`, `ResultEnteredAt`, `ResultEnteredByUserId`. Tempatnya ada; **bentuk hasil Mikrobiologi belum** | `Extend` |
| `LAB-DEC-096` Waktu Efektif dari `CollectedAt` | Kolomnya sudah ada | `Models/LabSpecimen.cs:102` `CollectedAt` | `Ready to reuse` |
| `LAB-DEC-096` Waktu Issued dari `FinalizedAt` | Kolomnya sudah ada pada pembawa hasil Mikrobiologi | `FinalizedAt` **hanya ada pada `Models/LabPathologyReport.cs:79`** — itu per **order** milik Patologi Anatomi. `LabExamination` **nol** `FinalizedAt` | `Missing` |
| `LAB-DEC-097` Final ≠ rilis | `FinalizedAt`, `FinalizedByUserId`, dan jejak `Reopen` pada pembawa hasil Mikrobiologi | Polanya sudah terbukti di `LabPathologyReport.cs:79,85,96` (`ReopenCount`), tetapi **nol pada `LabExamination`** | `Missing` — pola `Ready to reuse`, kolomnya belum ada |
| `LAB-DEC-097` rilis tetap `S4d` | `ValidatedAt`/`ReleasedAt` belum dibangun | **Nol kemunculan** `ValidatedAt`, `ReleasedAt`, maupun `ValidatedByUserId` di seluruh `LaboratoryManagement` | `Missing` — sesuai dugaan `LAB-DEC-080` |
| `LAB-DEC-098` tingkat kedua specimen | `LabSpecimenType` satu tingkat dan `LabSpecimen` menunjuk satu jenis | `Models/LabSpecimenType.cs` nol kolom induk; `Models/LabSpecimen.cs:65` `SpecimenTypeId` **tunggal**, bukan koleksi | `Extend` — induknya siap, anak dan relasi banyak belum ada |
| `LAB-DEC-099` isi awal disaring | Dataset 1.767 entri tersedia untuk diperiksa | **Nol kemunculan** dataset itu di repository maupun blueprint | `Unknown` — bertaut `LAB-OPEN-040` |
| `LAB-DEC-100` satuan volume baru | `VolumeUnitId` menunjuk data induk yang dapat ditambah | `Models/LabSpecimen.cs:183` menunjuk `MstMeasurement`; `MasterData/Models/MstMeasurement.cs:40` punya penanda `IsForLaboratory`; `MasterData/Controllers/MeasurementController.cs:301,384,492` menyediakan **POST, PUT, dan DELETE** | `Ready to reuse` — **tidak mengulang `LAB-COORD-006`/`MST-POS-WRITE`** |
| `LAB-DEC-101` baris kepekaan | Tabel isolat dan antibiogram | **Nol kemunculan** `Isolate`, `Antibiogram`, maupun `Susceptibility` di seluruh `LaboratoryManagement` | `Missing` — inilah pekerjaan inti `S4b` |
| `LAB-DEC-101` antibiotik terkendali | `LabAntibiotic` berdiri | `Models/LabAntibiotic.cs`, migration `20260918085707_AddLabMicrobiologyMasterData`, layanan `Services/LabMicrobiologyMasterDataService.cs` (529 baris), controller `Controllers/LabAntibioticController.cs` | `Ready to reuse` |
| `LAB-DEC-102` nol subbakteri | `LabOrganism` cukup menampung spesies sebagai baris | `Models/LabOrganism.cs` punya `OrganismCode`, `OrganismName`, `IsActive` — nol kolom induk, dan memang tidak dibutuhkan | `Ready to reuse` |
| `LAB-DEC-103` data induk aturan kritis | Tabel aturan kritis Mikrobiologi | **Nol tabel.** `Models/LabValueBound.cs` yang ada bersumbu **angka** — batas bawah/atas — dan tidak dapat menampung kombinasi organisme + antibiotik + interpretasi | `Missing` |
| `LAB-DEC-104` kewajiban bergantung isi | Lapis validasi tersedia | `Services/LabExaminationService.cs` (320 baris) berdiri sebagai tempatnya; aturan bersyarat Mikrobiologi belum ada | `Extend` |
| `LAB-DEC-105` Analis diturunkan | `ResultEnteredByUserId` dicatat sistem | `Models/LabExamination.cs:186` | `Ready to reuse` |
| `LAB-DEC-106` `Definitif` sebagai fakta | Kolom siapa/kepada siapa/kapan | **Nol kolom.** `LabExamination` tidak punya satu pun ruas konsultasi | `Missing` |
| `LAB-DEC-107` specimen dapat disunting | Endpoint koreksi ruas specimen | `Controllers/LabSpecimenController.cs` hanya punya `POST by-order` dan **delapan aksi siklus hidup** — `collect`, `receive`, `accept`, `reject`, `request-recollection`, `hold`, `resume`, `cancel`. **Nol `PUT`, nol `PATCH`** | `Missing` |
| `LAB-DEC-107` perubahan berjejak | Jejak nilai lama tersedia | `Models/LabTransitionHistory.cs:40-59` mencatat `Action`, `FromStatus`, `ToStatus`, `ActorUserId`, `OccurredAt` — ia jejak **status**, bukan jejak **nilai ruas**. Nilai lama sebuah kolom tidak punya tempat | `Reuse with adapter` |
| `LAB-DEC-108` sumber dokter bertugas | `MstDoctorSchedule` memuat dokter yang sedang bertugas | **Lihat `LAB-CONFLICT-010` di bawah** | `Conflict` |
| `LAB-DEC-109` nol ruas HL7 | `HL7` nol di backend | `grep -ril "hl7"` atas seluruh `*.cs` dan `*.csproj` → **nol berkas**. Diverifikasi ulang pada `981e002c` | `Ready to reuse` — keputusan cocok dengan keadaan |
| `LAB-DEC-110` template cetak | Pembangkit berkas cetak | Nol pustaka PDF pada `.csproj`; tetap `LAB-COORD-011` | `Missing` |

### Bagian C — Pertentangan yang ditemukan scan ini

#### `LAB-CONFLICT-010` — `MstDoctorSchedule` bukan daftar dokter jaga

**`LAB-DEC-108` memilih tabel yang menjawab pertanyaan berbeda.**

Bukti: `Areas/HealthServices/MasterData/Models/MstDoctorSchedule.cs@981e002c`.

| Ruas | Nilainya | Artinya |
|---|---|---|
| `ClinicId` (baris 31) | **Wajib** | Setiap baris terikat pada satu poliklinik |
| `PracticeDay`, `StartTime`, `EndTime` | Hari dan jam praktik | Jadwal buka praktik, bukan penugasan jaga |
| `MaxPatientQuota`, `MaxAppointmentQuota`, `MaxWalkInQuota` | Kuota pasien | Ini alat pengaturan antrean poliklinik |
| `IsAllowKioskRegistration`, `IsTelemedicineAvailable` | Penanda pendaftaran | Seluruhnya urusan rawat jalan |
| `ScheduleType` | `WeeklyRecurring`, `SpecificDate`, `Temporary` | Nol nilai yang berarti "sedang jaga" |

**Kenapa ini penting dan bukan sekadar kerapian.**

> Hasil kritis paling sering muncul pukul dua pagi. Yang dicari petugas saat itu adalah dokter
> yang **sedang berjaga di bangsal**, bukan dokter yang **membuka praktik di Poli Penyakit
> Dalam setiap Selasa pukul 09.00**. `MstDoctorSchedule` hanya dapat menjawab pertanyaan
> kedua. Memakainya untuk pertanyaan pertama menghasilkan daftar nama yang **tidak ada di
> rumah sakit** pada jam hasil kritis itu keluar.

**Kandidat pengganti yang ditemukan scan ini — dan kenapa belum tentu cocok:**

| Kandidat | Bukti | Kelebihan | Hambatannya |
|---|---|---|---|
| `TrxOnCallAssignment` | `Areas/Corporate/HumanResource/SchedulingManagement/Models/TrxOnCallAssignment.cs` — `StartAt`, `EndAt`, `OnCallRole` (`Primary`), `AssignmentStatus`, `ExpectedResponseMinutes`, `ActivatedAt` | Ini **benar-benar** penugasan jaga bertenggat waktu | Menunjuk `WorkforceProfileId`, **bukan** `DoctorId`. Perlu jembatan dari profil ketenagakerjaan ke identitas dokter, dan tabelnya **milik Human Resource**, bukan Laboratorium |
| `TrxRosterAssignment` | Berkas sekerabat — `ProfessionId`, `SpecializationId`, `ShiftGroupId` | Dapat disaring per profesi/spesialisasi | Bersumbu **perencanaan** roster, bukan "siapa yang sedang jaga menit ini" |

**Kesimpulan scan.** `LAB-DEC-108` **tidak dicabut** oleh dokumen ini — mencabut keputusan bukan
wewenang audit. Yang dinyatakan: **fakta pendukungnya tidak berlaku**, dan keputusan itu perlu
dibuka ulang oleh pemilik modul. Bagian pertamanya — DPJP diambil dari pesanan — **tetap
sahih** dan tidak terdampak.

### Bagian D — Closure question untuk `/grill-me`

Ketiganya **tidak dijawab** oleh audit ini.

| ID | Pertanyaan | Pemilik | Memblokir |
|---|---|---|---|
| `LAB-CLOSE-010` | **Dari mana daftar "dokter yang sedang bertugas" diambil, sesudah `MstDoctorSchedule` terbukti jadwal praktik poliklinik?** Pilihan yang terlihat dari source: menumpang `TrxOnCallAssignment` milik Human Resource beserta jembatan profil-ke-dokter, atau kembali ke DPJP saja, atau mendirikan penugasan jaga milik Laboratorium sendiri | Yoga Aji Pratama, bersama pemilik `human-resource` bila kandidat pertama dipilih | Kolom Dokter Konfirmator. Membuka ulang `LAB-DEC-108` |
| `LAB-CLOSE-011` | **Jejak perubahan ruas specimen disimpan di mana?** `LabTransitionHistory` mencatat perpindahan **status**, sedangkan `LAB-DEC-107` menuntut nilai lama sebuah **ruas** tetap terbaca. Apakah jejak ruas ditumpangkan ke tabel itu dengan penyesuaian, atau berdiri sendiri | Yoga Aji Pratama | `LAB-DEC-107`. Tidak memblokir penyuntingannya, memblokir bentuk jejaknya |
| `LAB-CLOSE-012` | **Status temuan Mikrobiologi memakai daftar nilai yang mana?** `LabPathologyFindingStatus` yang sudah berdiri berisi `Normal`/`NeedsAttention`/`Critical`, sedangkan BR-56 menetapkan `Normal`/`Positif`/`Negatif` untuk tingkat isolat Mikrobiologi. Dipakai ulang dengan penyesuaian, atau daftar tersendiri | Yoga Aji Pratama + `DR-LAB-002` | Bentuk isolat pada `S4b` |

### Bagian E — Catatan pembukuan

- `LabResultForm` pada `Enums/LaboratoryEnums.cs:222` hanya mengenal **dua** bentuk, `Numeric`
  dan `Choice`, sedangkan BR-23 menetapkan **empat**. Patologi Anatomi menyelesaikannya dengan
  **tabel tersendiri** (`LabPathologyReport`), bukan dengan menambah nilai enum. Mikrobiologi
  kemungkinan mengikuti pola yang sama; itu keputusan arsitektur, bukan temuan audit.
- `LabDiscipline` pada baris 15 sudah memuat `Microbiology = 3`, sehingga penyaringan per
  disiplin **sudah berdiri** dan tidak perlu dibangun ulang.
- Peta ini **tidak** mengaudit ulang bagian di luar permukaan putaran 9. Bagian revision 1-3
  yang tidak disebut di sini **tetap berpotensi basi** mengingat besar pergeserannya.

---

## Impact Scan Revision 3 — 2026-09-14

**Pemicu.** `LAB-OPEN-022` dan `LAB-OPEN-023`, dibuka Amendment Pass putaran 2 pada decision log
revision 23. Peta ini dikunci pada BE `c87d9c0` + FE `688daff90`; `HEAD` sudah bergeser jauh.

**Besar pergeserannya.** Backend **298 commit**, frontend **155 commit**. Ini bukan pergeseran
kecil seperti revision 2, dan hasilnya **tidak** seperti revision 2 yang menemukan nol
perubahan.

> **Kesimpulan pendek.** Sembilan keputusan `LAB-DEC-037` sampai `LAB-DEC-045` diperiksa
> terhadap source pada `HEAD`. **Tujuh terbukti berdiri di atas fakta yang masih benar.**
> **Satu ternyata sudah dikerjakan kode dan bukan aturan baru** (`LAB-DEC-039`). **Satu berdiri
> di atas fakta yang salah dan harus dibuka ulang** (`LAB-DEC-044`).

### Verifikasi tujuh fakta dasar amendment

| # | Klaim yang dipakai amendment | Keadaan pada `HEAD` | Hasil |
|---:|---|---|---|
| 1 | `LabExamination` tidak punya kolom Qty, hanya `IsDuplo` | `Models/LabExamination.cs:99` punya `IsDuplo`; tidak ada `Qty`/`Quantity` di model maupun `DTOs/LabExaminationDtos.cs` | ✅ **Benar** |
| 2 | `LabSpecimen` hanya punya `SpecimenDescription` teks bebas | `Models/LabSpecimen.cs:45` `SpecimenDescription`; tidak ada `SpecimenType` maupun `Volume` di model maupun `DTOs/LabSpecimenDtos.cs` | ✅ **Benar** |
| 3 | `ReceivedAt` diisi server, tidak ada waktu penerimaan fisik terpisah | `Models/LabSpecimen.cs:55` `ReceivedAt` + `ReceivedByUserId`; tidak ada kolom waktu fisik kedua | ✅ **Benar** |
| 4 | `MstReferralInstitution`/`MstReferralDoctor` belum punya status menunggu persetujuan | Keduanya **sudah ada**; hanya punya `IsActive`. Tidak ada `ApprovalStatus`, `IsApproved`, maupun `ProposedBy` | ✅ **Benar**, dengan temuan tambahan di bawah |
| 5 | Belum ada endpoint Billing yang menjawab metode pembayaran per kunjungan | **Fakta jauh berbeda dari dugaan** — lihat `CONF-02` | ❌ **Salah** |
| 6 | `AC-20` mengunci daftar pemeriksaan pada `Collected` | Kode mengunci pada `Accepted or Rejected`, bukan `Collected` — lihat di bawah | ⚠️ **Sudah benar, tapi bukan aturan baru** |
| 7 | Frontend punya tiga layar lab yang akan berdampingan dengan menu baru | Seluruh modul Laboratorium frontend **dibangun dari nol** sejak `688daff90`: 3.751 baris pada 31 berkas | ✅ **Benar**, dan `F5` kini **usang total** |

### Temuan 6 — `LAB-DEC-039` bukan aturan baru, melainkan pembetulan catatan

**Bukti.** `Areas/HealthServices/LaboratoryManagement/Services/LabExaminationService.cs:120-127`
pada `466a7127`:

```csharp
// VAL-18. Wadah yang sudah diputuskan tidak boleh bertambah isinya: kelayakan
// tagihnya sudah terbit, dan menambah pemeriksaan sesudahnya berarti menagihkan
// sesuatu yang tidak pernah ikut dinilai layak.
if (specimen.SpecimenStatus is LabSpecimenStatus.Accepted or LabSpecimenStatus.Rejected)
```

Kode **sudah** mengunci pada penetapan kelayakan, bukan pada `Collected`. Alasan yang ditulis di
comment-nya sama persis dengan alasan yang dipakai `LAB-DEC-039`. Tidak ada satu pun rujukan
`Collected` di `LabOrderService.cs` maupun `LabExaminationService.cs`.

**Artinya.** `AC-20` sudah tidak sesuai kode **sejak sebelum** amendment ini. `LAB-DEC-039`
tidak mengubah perilaku apa pun — ia menyamakan catatan dengan kenyataan yang sudah berjalan,
dan memberi `VAL-18` dasar keputusannya. Status: **`Ready to reuse`**, bukan `Extend`.

### Temuan 4 — data induk perujuk ada, tetapi tidak ada cara mengisinya

| Yang diperiksa | Keadaan pada `HEAD` |
|---|---|
| `MstReferralInstitution` | Ada — `Areas/HealthServices/MasterData/Models/MstReferralInstitution.cs`, terdaftar `ApplicationDbContext.cs:626`, tabel `MstReferralInstitution` |
| `MstReferralDoctor` | Ada — tertaut ke instansinya lewat `ReferralInstitutionId` |
| Kunjungan menunjuk ke sana | Ada — `RegPatientEncounter.cs:223` `ReferralInstitution` |
| Endpoint tulis | **Tidak ada satu pun.** `ReferralInstitutionController.cs` hanya punya `GET /options` |
| Service tulis | **Tidak ada.** `ReferralMasterDataService.cs` hanya punya `GetInstitutionOptionsAsync` dan `GetDoctorOptionsAsync` |
| Pengisi daftar saat ini | `Areas/HealthServices/LaboratoryManagement/Seeders/LabDummyDataSeeder.cs:498,513` |

**Yang perlu disadari.** `VAL-43` menyuruh petugas *"hubungi bagian data induk untuk
menambahkannya"* — padahal **bagian data induk pun tidak punya layarnya**. Satu-satunya yang
mengisi tabel itu hari ini adalah seeder data contoh, dan seeder itu **berada di folder
Laboratorium**, bukan Master Data.

`LAB-DEC-043` karena itu **lebih besar dari yang diperkirakan**: yang kurang bukan sekadar
status menunggu persetujuan, melainkan seluruh kemampuan pengelolaan data induknya. Status:
**`Missing`**, bukan `Extend`. `LAB-COORD-006` perlu menyebutkan ini apa adanya.

Catatan batas: seeder data induk global yang tinggal di folder Laboratorium adalah utang teknis
terhadap `AC-49`. Dicatat, tidak diperbaiki — audit ini read-only.

### Rangkuman status kemampuan yang terdampak

| Kemampuan | Status | Bukti |
|---|---|---|
| Qty pada baris pemeriksaan (`LAB-DEC-038`) | `Missing` | `Models/LabExamination.cs@466a7127` — tidak ada kolomnya |
| Jenis specimen terstruktur (`LAB-DEC-040`) | `Missing` | `Models/LabSpecimen.cs@466a7127` — hanya `SpecimenDescription` |
| Volume specimen (`LAB-DEC-041`) | `Missing` | `Models/LabSpecimen.cs@466a7127` — tidak ada kolomnya |
| Waktu penerimaan fisik (`LAB-DEC-042`) | `Extend` | `Models/LabSpecimen.cs:55@466a7127` — `ReceivedAt` ada, kolom kedua belum |
| Titik kunci pada kelayakan (`LAB-DEC-039`) | `Ready to reuse` | `Services/LabExaminationService.cs:123@466a7127` — sudah berjalan sebagai `VAL-18` |
| Data induk instansi perujuk (`LAB-DEC-043`) | `Missing` | `Controllers/ReferralInstitutionController.cs@466a7127` — hanya `GET /options` |
| Metode pembayaran per kunjungan (`LAB-DEC-044`) | `Conflict` | Lihat `CONF-02` |
| Menu baru berdampingan layar lama (`LAB-DEC-045`) | `Ready to reuse` | 31 berkas frontend lab pada `9cd4cd03f` |

### Perubahan penamaan yang membuat rujukan lama tidak lagi ditemukan

| Rujukan pada peta revision 1-2 dan decision log | Nama pada `466a7127` |
|---|---|
| `TrxPatientEncounter` | **`RegPatientEncounter`** |
| `TrxPatientEncounterGuarantor` | **`RegPatientEncounterGuarantor`** |
| `TrxLabSpecimen` | `LabSpecimen` |
| `TrxLabTransitionHistory` | `LabTransitionHistory` |

`BR-28` dan `BR-31` pada decision log masih menulis `TrxPatientEncounter@c87d9c0`. Buktinya
tetap sahih, tetapi **nama simbolnya sudah berubah** dan pencarian dengan nama lama akan
mengembalikan nol hasil.

### `F5` dicabut — frontend Laboratorium sudah berdiri penuh

Peta revision 1 mencatat *"Frontend belum punya modul Laboratorium sama sekali"* pada
`c79bb6ee4`. Pada `9cd4cd03f` yang berdiri:

| Route | Berkas |
|---|---|
| `laboratory-management/overview` | `laboratory-overview-view.jsx` |
| `laboratory-management/lab-orders` + `create` + `[slug]` + `[slug]/specimens` | `lab-order-list-view.jsx`, `lab-order-form-view.jsx`, `lab-order-detail-view.jsx`, `lab-specimen-workspace-view.jsx` |
| `laboratory-management/lab-patient-registrations` + `walk-in` + `external-referral` | `lab-patient-search-view.jsx`, `lab-patient-registration-form-view.jsx` |
| `laboratory-management/lab-worklists` + `cito-overdue` | `lab-worklist-view.jsx` |
| `laboratory-management/lab-monitoring/{clinical-pathology,anatomic-pathology,microbiology}` | `lab-monitoring-view.jsx` |
| `laboratory-management/lab-tariffs` | `lab-tariff-view.jsx` |

**Tidak ada route `lab-specimens` tersendiri.** `lab-specimen-workspace-view.jsx` hanya dipakai
`lab-orders/[slug]/specimens/page.jsx` — menguatkan dasar `LAB-DEC-045`, karena hari ini
penanganan wadah memang hanya dapat dicapai lewat sebuah pesanan yang sudah ada.

### Pembukuan manifest — menutup `LAB-OPEN-023`

Hash ulang dengan metode manifest (`tr -d '\r' | sha256sum`) pada 2026-09-14:

| Berkas | Hash manifest | Hash sekarang | Hasil |
|---|---|---|---|
| `00-interview-decisions.md` | `6504b18a…` | `37424863bd618ecac785136e8835ad5bbc5da4e5addf26e5f2785b2057c9df6b` | ❌ Berubah — revision 23 |
| `01-existing-capability-map.md` | `703a8dff…` | `703a8dffe23971ecc09f416a516e4d83e26cd834cae277a7875fab8c484f6117` | ✅ Sama (sebelum revision 3 ini ditulis) |
| `02-requirement-completeness-assessment.md` | `3de86c82…` | `3de86c8242a313a5a864a1eaa1cfffdb21149658789f01095d0ec847a9c072d1` | ✅ Sama |
| `03-domain-architecture.md` | `3279c0ef…` | `3279c0ef2309b52feab77d870f782f4ce02134b2457fa46bfd09d868d98493de` | ✅ Sama |

`LAB-OPEN-023` karena itu **lebih sempit** dari dugaan: hanya baris `decisions`, hash decision
log, hash capability map, dan kedua `*_commit_sha` pada manifest yang perlu diperbarui. Dua
masukan lain tidak tersentuh.

---

## Impact Scan Revision 2 — 2026-09-02

`blueprint-manifest.md` menandai peta ini `STALE` pada `CAP-11` dan bagian utang teknis, dan
mensyaratkan impact scan ulang sebelum peta dipakai menyusun roadmap. Scan itu dijalankan
2026-09-02. **Hasilnya: tidak ada satu status kemampuan pun yang berubah.** Penanda `STALE`
dicabut.

### Yang diperiksa dan hasilnya

| Yang diperiksa | Klaim revision 1 | Keadaan pada `HEAD` | Hasil |
|---|---|---|---|
| `CAP-11` — berkas producer | `Areas/HealthServices/ClinicalManagement/Services/ClinicalMilestoneFactProducer.cs` | Ada di path yang sama | ✅ Tetap |
| `CAP-11` — nilai enum | `ClinicalMilestoneKind` bernilai `ChargeEligibility` dan `ClinicalCancellation` | `ChargeEligibility = 1`, `ClinicalCancellation = 2` pada `Enums/ClinicalMilestoneFactEnums.cs:11-18` | ✅ Tetap |
| `CAP-11` — pemanggilan dari Lab | Dipanggil dari `LabSpecimenService` | `LabSpecimenService.cs:193` dan `LabOrderService.cs:355` | ✅ Tetap |
| Utang teknis — configuration di `Areas/` | `LaboratoryManagementConfigurations.cs` sudah dihapus | Tidak ditemukan di seluruh repository | ✅ Benar |
| Utang teknis — tiga configuration pindah | Berada di `Repositories/Configurations/HealthServices/LaboratoryManagement/` | Ada tiga: `MstLabRejectionReasonConfiguration.cs`, `TrxLabSpecimenConfiguration.cs`, `TrxLabTransitionHistoryConfiguration.cs` | ✅ Benar |
| Utang teknis — `LabOrderConfiguration.cs` masih longgar | Masih langsung di bawah `HealthServices/` | Benar, masih di sana | ⚠️ Tetap terbuka |
| Frontend SHA | `688daff90` | `HEAD` frontend memang `688daff90` | ✅ Tidak bergeser |

**Kenapa penggantian nama `TrxClinicalMilestoneFact` → `CliClinicalMilestoneFact` tidak
membatalkan `CAP-11`.** Migration `RenameClinicalMilestoneFactToCliPrefix` mengubah nama
**model dan tabelnya**. Bukti yang dikutip `CAP-11` seluruhnya menunjuk **service, enum, dan
method** — `ClinicalMilestoneFactProducer`, `ClinicalMilestoneKind`, `EmitChargeEligibilityAsync`,
`EmitClinicalCancellationAsync` — dan tidak satu pun dari nama itu ikut berubah. Karena itu
statusnya tetap `Ready to reuse`.

### Verifikasi silang atas kemampuan berisiko tinggi

Sekalian diperiksa ulang kemampuan yang paling menentukan besarnya pekerjaan roadmap:

| Kemampuan | Status revision 1 | Bukti pada `HEAD` | Hasil |
|---|---|---|---|
| `CAP-03` hasil pemeriksaan | `Missing` | `Areas/HealthServices/LaboratoryManagement/` hanya berisi 11 berkas: controller/DTO/service untuk LabOrder dan LabSpecimen, `LaboratoryEnums.cs`, dan empat model. Tidak ada model, service, maupun controller hasil | ✅ Tetap `Missing` |
| `CAP-07` batas nilai | `Missing` | Tidak ada model batas nilai di folder tersebut | ✅ Tetap `Missing` |
| `CAP-05` alasan penolakan | `Reuse with adapter` | `LabSpecimenController.cs:46` hanya `[HttpGet("rejection-reasons")]`. Tidak ada `HttpPost`, `HttpPut`, maupun `HttpDelete` untuk data induk ini | ✅ Tetap |
| `CAP-18` pemberitahuan | `Missing` | Tidak ada berkas `*Notification*.cs` dan tidak ada `DbSet<...Notification...>` di seluruh repository. Dua kecocokan teks yang muncul hanyalah komentar pada `MstWorkflowStep.cs:29` dan konstanta jenis langkah `WorkflowValueConstants.cs:53` — bukan kemampuan pemberitahuan | ✅ Tetap `Missing` |
| `CAP-21` frontend Laboratorium | `Missing` | Pencarian `laboratory-management`, `labOrder`, `labSpecimen`, `lab-order` pada `QuilvianSystemFrontendDev/src@688daff90` tetap nihil | ✅ Tetap `Missing` |
| `CAP-01` kesegeraan | `Extend` | `LabOrder.cs` tidak memuat `Urgency`, `Cito`, `Priority`, maupun `IsUrgent` | ✅ Tetap `Extend` |
| `CAP-02` migration sampel | `Ready to reuse` | `Migrations/20260815103436_initializeLabOrder.cs` dan `20260824091610_AddLaboratorySpecimenLifecycle.cs` ada | ✅ Tetap |
| `CAP-13`, `CAP-14`, `CAP-19` | `Ready to reuse` / `Reuse with adapter` | `Attributes/AccessPermissionAttribute.cs`, `Filters/AccessPermissionFilter.cs`, `Services/Security/AccessPermissionService.cs`, `Seeders/AccessMenuSeeder.cs`, dan `Hubs/QueueHub.cs` seluruhnya ada | ✅ Tetap |

### Dua koreksi faktual yang ditemukan scan ini

**Koreksi 1 — jumlah pengujian keliru satu.** `CAP-24` menyebut
`LaboratorySpecimenLifecycleTests.cs` berisi **19** pengujian. Hitungan sebenarnya pada `HEAD`
adalah **18** — 18 atribut `[Fact]`, nol `[Theory]`. Berkas itu memuat 19 method publik, satu
di antaranya method bantu, bukan pengujian. `LaboratoryAuthorityTests.cs` benar berisi 12.

Jadi total pengujian Laboratorium adalah **30, bukan 31**. Angka 31 juga dikutip
`approval-requests/2026-09-01-permintaan-koordinasi-lintas-modul.md` bagian 3.3 dan perlu ikut
diperbaiki. Koreksi ini tidak mengubah status `CAP-24` yang tetap `Ready to reuse`.

**Koreksi 2 — tanggal audit tidak mungkin benar.** Revision 1 menyatakan audit dijalankan
2026-09-01 pada backend `c87d9c0`. Menurut reflog, commit `c87d9c0` **baru dibuat 2026-09-02
pukul 08:51:14** oleh `pull --tags origin yoga`; sebelum itu `HEAD` berada di `c0b8549`.

| Fakta | Nilai |
|---|---|
| `c87d9c0` dibuat | 2026-09-02 08:51:14 +0700, hasil merge dari pull |
| `HEAD` sebelumnya | `c0b8549`, 2026-09-02 08:51:05 |
| Artefak blueprint terakhir ditulis | 2026-09-02 08:54–08:55 |

Karena artefaknya ditulis **setelah** commit itu ada, seluruh jangkar bukti `@c87d9c0`
**tetap sahih**. Yang keliru hanya label tanggalnya. Tidak ada bukti yang perlu dicabut, tetapi
tanggal audit pada revision 1 sebaiknya dibaca sebagai 2026-09-02.

Hal yang sama menutup pertanyaan pada `approval-requests/...` bagian 3.2: pernyataan "checkout
lokal 7 commit tertinggal" memang benar sebelum pukul 08:48, dan sudah tidak berlaku sesudahnya.

---

## Peringatan: SHA frontend sudah berubah

Decision log revision 7 mencatat frontend SHA `c79bb6ee4`. Saat audit ini dijalankan, frontend
sudah berada di `688daff90`.

| Repository | SHA di decision log | SHA saat audit | Keterangan |
|---|---|---|---|
| `NewQuilvianSystemBackend` | `c87d9c0` | `c87d9c0` | Sama, tidak ada pergeseran |
| `QuilvianSystemFrontendDev` | `c79bb6ee4` | `688daff90` | **Berubah** |

Audit ini memakai SHA terkini. Temuan pokok frontend tidak berubah: pada kedua SHA, modul
Laboratorium sama-sama **tidak ada sama sekali**. Jadi pergeseran ini tidak membatalkan
keputusan mana pun, tetapi angka SHA di decision log perlu diperbarui saat revisi berikutnya.

---

## Batas Audit

### Yang diaudit

Kemampuan yang dibutuhkan Rilis 1 menurut `LAB-DEC-001` sampai `LAB-DEC-014`, dikelompokkan
menjadi sepuluh klaster:

| Klaster | Yang dicari |
|---|---|
| Order/Result | Pesanan lab, sampel, hasil pemeriksaan |
| Identity/Master Owner | Katalog pemeriksaan, batas nilai, alasan penolakan |
| Episode/Transaction Owner | Kunjungan pasien dari Rawat Jalan, Rawat Inap, dan IGD |
| Actor/Workforce | Identitas petugas yang melakukan tindakan |
| Workflow/Status | Perpindahan status, riwayat, konkurensi |
| Documentation/Record | Penyajian dan penyimpanan hasil |
| Financial | Pengiriman fakta kelayakan tagih ke Billing |
| Authorization/Audit | Permission per aksi, jejak audit |
| External Integration | Pemberitahuan kepada dokter |
| Frontend consumer | Route, menu, layar, state, API service |

### Yang tidak diaudit

Mikrobiologi, Patologi Anatomi, Bank Darah, stok reagen, dan Radiologi — seluruhnya berada di
luar scope menurut `LAB-DEC-002` dan `LAB-DEC-014`.

---

## Ringkasan Hasil

| Status | Jumlah | Arti singkat |
|---|---:|---|
| `Ready to reuse` | 11 | Sudah ada, terbukti jalan, bisa langsung dipakai |
| `Reuse with adapter` | 3 | Sudah ada, tetapi perlu penyesuaian kecil |
| `Extend` | 1 | Sudah ada, tetapi perlu tambahan kolom atau perilaku |
| `Missing` | 6 | Belum ada sama sekali |
| `Conflict` | 1 | Ada pertentangan antara kode dan keputusan terkunci |
| `Unknown` | 2 | Belum bisa dipastikan tanpa keputusan manusia |

**Kesimpulan singkat:** separuh perjalanan Laboratorium sudah dibangun dan terbukti dengan
pengujian, yaitu dari pesanan sampai sampel dinyatakan layak beserta pengiriman fakta tagihan.
Yang belum ada adalah **seluruh bagian hasil pemeriksaan**, **pemberitahuan kepada dokter**,
dan **seluruh tampilan frontend**.

---

## Tabel Kemampuan

Format bukti: `repository/path#simbol@SHA`.

### Klaster Order dan Result

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `CAP-01` | Pesanan laboratorium beserta siklus hidupnya | Laboratorium | `NewQuilvianSystemBackend/Areas/HealthServices/LaboratoryManagement/Models/LabOrder.cs#LabOrder@c87d9c0`; `Services/LabOrderService.cs@c87d9c0`; `Controllers/LabOrderController.cs@c87d9c0`; migration `Migrations/20260815103436_initializeLabOrder.cs@c87d9c0` | `Extend` | Tidak ada kolom tingkat kesegeraan. `LAB-DEC-013` mewajibkan penanda cito dan batas waktunya | Sedang. Penambahan kolom memerlukan migration baru pada tabel yang sudah berisi data |
| `CAP-02` | Siklus hidup sampel: rencana, ambil, terima, layak/tolak, ambil ulang | Laboratorium | `Models/TrxLabSpecimen.cs#TrxLabSpecimen@c87d9c0`; `Services/LabSpecimenService.cs@c87d9c0`; `Controllers/LabSpecimenController.cs@c87d9c0`; migration `Migrations/20260824091610_AddLaboratorySpecimenLifecycle.cs@c87d9c0` | `Ready to reuse` | Tidak ada | Rendah. Sudah lengkap dan teruji |
| `CAP-03` | Hasil pemeriksaan: isi nilai, verifikasi, validasi, rilis, koreksi | Laboratorium | Tidak ditemukan model, service, controller, enum, maupun migration mana pun yang menyimpan nilai hasil pada `Areas/HealthServices/LaboratoryManagement/@c87d9c0` | `Missing` | Seluruhnya harus dibangun. Ini inti Rilis 1 menurut `LAB-DEC-001` | Tinggi. Bagian terbesar pekerjaan Rilis 1 |
| `CAP-04` | Riwayat perpindahan status yang tidak bisa diubah | Laboratorium | `Models/TrxLabTransitionHistory.cs#TrxLabTransitionHistory@c87d9c0` — memuat `Scope`, `Action`, `FromStatus`, `ToStatus`, `ReasonCode`, `ReasonNote`, `ActorUserId`, `OccurredAt`, `CorrelationId` | `Ready to reuse` | Tidak ada. `LabTransitionScope` cukup ditambah nilai baru untuk hasil bila diperlukan | Rendah. Sudah memenuhi seluruh isian yang diminta `LAB-INH-013` |

**Penjelasan `CAP-01` untuk pembaca non-teknis.** Pesanan lab sudah bisa dibuat, ditahan,
dilanjutkan, dan dibatalkan. Yang belum ada hanyalah cara menandai sebuah pesanan sebagai
"cito" alias segera. Karena `LAB-DEC-013` mewajibkan penandaan itu, tabel pesanan perlu
ditambah kolom, dan itulah sebabnya statusnya `Extend` dan bukan `Ready to reuse`.

### Klaster Identity dan Master Owner

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `CAP-05` | Daftar alasan penolakan sampel yang terkendali | Laboratorium | `Models/MstLabRejectionReason.cs#MstLabRejectionReason@c87d9c0` — punya `ReasonCode`, `IsInternalHospitalError`, `RequiresNote`; dibaca lewat `Controllers/LabSpecimenController.cs#GetRejectionReasons@c87d9c0` | `Reuse with adapter` | Hanya tersedia endpoint baca. **Tidak ada** endpoint tambah, ubah, atau nonaktifkan, dan tidak ditemukan seeder yang mengisinya | Sedang. Bila tabel kosong di lingkungan baru, petugas tidak bisa menolak sampel sama sekali |
| `CAP-06` | Katalog jenis pemeriksaan laboratorium | `master-data` | `Areas/HealthServices/MasterData/Models/MstProcedure.cs#IsLaboratory@c87d9c0`; dipakai sebagai komponen pemeriksaan di `TrxLabSpecimen.ProcedureId@c87d9c0` | `Reuse with adapter` | Berfungsi sebagai katalog, tetapi tidak punya satuan hasil, jenis sampel, wadah, volume minimal, maupun metode. `LAB-DEC-001` memang menunda sisa katalog ke Rilis 2 | Rendah untuk Rilis 1, karena penundaannya sudah disetujui |
| `CAP-07` | Tabel batas nilai: satuan, batas normal, batas kritis, batas waktu cito | Laboratorium | Tidak ditemukan kolom maupun tabel penyimpan batas nilai di seluruh `Areas/@c87d9c0` | `Missing` | Seluruhnya harus dibangun. Diwajibkan `LAB-DEC-006` dan `LAB-DEC-013` | Tinggi. Tanpa ini, `LAB-DEC-004` tidak bisa dijalankan — sistem tidak akan tahu sebuah angka itu kritis |

### Klaster Episode dan Identitas Pasien

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `CAP-08` | Kunjungan pasien dari Rawat Jalan, Rawat Inap, dan IGD | `registration-management` | `Areas/HealthServices/RegistrationManagement/Models/TrxPatientEncounter.cs#EncounterType@c87d9c0`; `Enums/EncounterType.cs@c87d9c0` bernilai `Outpatient = 1`, `Emergency = 2`, `Inpatient = 3`, `MedicalCheckup = 4`, `Telemedicine = 5`. `LabOrder.EncounterId` menunjuk ke entity ini | `Ready to reuse` | Tidak ada | Rendah. `LAB-DEC-009` sudah terpenuhi di tingkat data tanpa perubahan apa pun |
| `CAP-09` | Identitas pasien dan dokter | `master-data` / `patient-management` | Diakses lewat `TrxPatientEncounter.PatientId` dan `TrxPatientEncounter.DoctorId@c87d9c0` | `Ready to reuse` | Tidak ada. Laboratorium cukup menempel pada kunjungan | Rendah |
| `CAP-10` | Tarif pemeriksaan beserta salinannya | `master-data` / `billing-kasir` | `Services/LabSpecimenService.cs#ResolveTariffAsync@c87d9c0`; salinan disimpan pada `TrxLabSpecimen.TariffId`, `TariffCodeSnapshot`, `UnitPriceSnapshot@c87d9c0` | `Ready to reuse` | Tidak ada | Rendah. Pola salinan tarif sudah benar: harga saat itu ikut tersimpan sehingga tidak berubah bila tarif induk diubah kemudian |

**Contoh kenapa `CAP-08` penting.** `LAB-DEC-009` memutuskan Laboratorium melayani ketiga unit
sekaligus. Sering kali keputusan seperti ini mahal karena data kunjungan tiap unit terpisah.
Di sini ternyata tidak: satu tabel `TrxPatientEncounter` sudah menampung ketiganya lewat kolom
`EncounterType`. Jadi keputusan itu bisa dijalankan tanpa tambahan pekerjaan data.

### Klaster Financial — batas kewenangan

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `CAP-11` | Pengiriman fakta kelayakan tagih dan pembatalan klinis ke Billing | `billing-kasir` (penerima), Laboratorium (pengirim) | `Areas/HealthServices/ClinicalManagement/Services/ClinicalMilestoneFactProducer.cs@c87d9c0`; dipanggil dari `LabSpecimenService.cs#EmitChargeEligibilityAsync` dan `#EmitClinicalCancellationAsync@c87d9c0`; jenis fakta di `Enums/ClinicalMilestoneFactEnums.cs#ClinicalMilestoneKind@c87d9c0` bernilai `ChargeEligibility` dan `ClinicalCancellation` | `Ready to reuse` | Tidak ada | Rendah. Sudah terpasang, terhubung, dan teruji |
| `CAP-12` | Laboratorium tidak boleh punya kolom atau method finansial | Laboratorium | Diuji otomatis oleh `Tests/QuilvianSystemBackend.BillingTests/Laboratory/LaboratoryAuthorityTests.cs#ModelLaboratorium_TidakMemilikiPropertiFinansialApaPun@c87d9c0` dan `#ServiceLaboratorium_TidakMemilikiMethodKewenanganFinansial@c87d9c0` | `Ready to reuse` | Tidak ada | Rendah. `AC-13` sudah dijaga pengujian otomatis, bukan sekadar niat |

**Penjelasan `CAP-11` dengan contoh.** Saat petugas menyatakan sampel layak periksa, sistem
otomatis mengirim satu "fakta" ke Billing yang berbunyi kira-kira: pemeriksaan ini sudah sah
untuk ditagihkan, kodenya sekian, harga saat itu sekian. Billing yang memutuskan apa yang
terjadi dengan uangnya. Laboratorium tidak pernah menyentuh angka tagihan. Mekanisme ini sudah
berjalan, jadi `AC-12` tidak perlu dibangun ulang.

### Klaster Authorization dan Audit

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `CAP-13` | Kewenangan berbeda untuk tiap tindakan lab | Platform | `Attributes/AccessPermissionAttribute.cs@c87d9c0`; `Filters/AccessPermissionFilter.cs@c87d9c0`; `Services/Security/AccessPermissionService.cs#HasAccessAsync@c87d9c0`. Lab memakai `[AccessPermission("LabSpecimen","Collect")]`, `("LabSpecimen","Receive")`, `("LabSpecimen","Accept")`, dan seterusnya pada `Controllers/LabSpecimenController.cs@c87d9c0` | `Ready to reuse` | Tidak ada | Rendah |
| `CAP-14` | Pendaftaran otomatis permission ke basis data | Platform | `Seeders/AccessMenuSeeder.cs@c87d9c0`, dijalankan saat aplikasi mulai lewat `Program.cs:974@c87d9c0`. Controller lab sudah membawa `[AccessController(...)]` dan `[AccessAction(...)]` sehingga ikut terdaftar sendiri | `Ready to reuse` | Tidak ada | Rendah. Permission untuk endpoint hasil yang baru akan terdaftar otomatis asalkan atributnya dipasang |
| `CAP-15` | Identitas petugas pelaku tindakan | Platform | `Services/LabSpecimenService.cs#GetCurrentUserId@c87d9c0` lewat `IHttpContextAccessor`; tersimpan pada `TrxLabSpecimen.CollectedByUserId`, `ReceivedByUserId`, `DecidedByUserId@c87d9c0` | `Ready to reuse` | Tidak ada | Rendah |
| `CAP-16` | Penegakan prinsip empat mata: pengisi hasil tidak boleh memvalidasi hasil yang sama | Laboratorium | Tidak ditemukan. Sistem permission bekerja per aksi, **bukan** per orang pada satu baris data. `AccessPermissionService.HasAccessAsync@c87d9c0` hanya menjawab "boleh atau tidak", tidak pernah membandingkan pelaku sebelumnya | `Missing` | Harus dibangun sebagai aturan di dalam service hasil, bukan lewat permission | **Tinggi.** Ini invariant keselamatan `LAB-DEC-003`. Bila keliru dianggap bisa ditutup permission, `AC-01` tidak akan terpenuhi |
| `CAP-17` | Perlindungan dua petugas bertindak bersamaan | Laboratorium | `LabOrder.Version` dan `TrxLabSpecimen.Version@c87d9c0`; diuji oleh `LaboratorySpecimenLifecycleTests.cs#DuaPetugasMenetapkanLayakBersamaan_SalahSatuDitolak@c87d9c0` | `Ready to reuse` | Tidak ada. Pola yang sama tinggal diterapkan pada tabel hasil | Rendah |

**Penjelasan `CAP-16`, temuan paling penting dalam audit ini.** Sistem izin yang ada menjawab
pertanyaan "apakah orang ini boleh memvalidasi hasil?". Ia tidak bisa menjawab "apakah orang
ini yang tadi mengetik hasilnya?". Padahal `LAB-DEC-003` justru menuntut pertanyaan kedua.
Artinya prinsip empat mata harus ditulis sebagai aturan di dalam layanan hasil, dengan
membandingkan pengisi dan validator pada baris hasil yang sama. Ini juga berarti jalur
pengecualian beserta penandanya harus disimpan di tabel hasil, bukan di tabel izin.

### Klaster Notifikasi dan Integrasi Eksternal

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `CAP-18` | Pemberitahuan tersimpan untuk dokter, dengan status sudah dibaca | Platform | Tidak ditemukan layanan notifikasi umum, tabel notifikasi, surel, SMS, maupun WhatsApp di seluruh `NewQuilvianSystemBackend@c87d9c0` | `Missing` | Seluruhnya harus dibangun. Diwajibkan `LAB-DEC-012` | **Tinggi.** Ini kemampuan milik platform, bukan khusus Laboratorium. Membangunnya di dalam modul Laboratorium berisiko menjadi duplikasi ketika modul lain membutuhkan hal yang sama |
| `CAP-19` | Pengiriman seketika ke pengguna yang sedang online | Platform | `Hubs/QueueHub.cs@c87d9c0` dipetakan ke `/hubs/queues` oleh `Program.cs:1091@c87d9c0`; pengelompokan peserta per *nurse station cluster* lewat `QueueHub#JoinNurseStationCluster@c87d9c0` | `Reuse with adapter` | Hub yang ada khusus antrean dan mengelompokkan peserta berdasarkan nurse station, bukan berdasarkan dokter. Perlu hub baru atau perluasan pengelompokan | Sedang. Teknologinya sudah terbukti jalan, tinggal pola pengelompokannya yang berbeda |
| `CAP-20` | Klien realtime di sisi frontend | Platform | `QuilvianSystemFrontendDev/src/lib/signalr/signalrHubClient.jsx@688daff90` (klien umum) dan `src/lib/realtime/queue-realtime-client.js@688daff90` (khusus antrean) | `Ready to reuse` | Tidak ada. Klien umumnya sudah terpisah dari kebutuhan antrean | Rendah |

### Klaster Frontend

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `CAP-21` | Seluruh tampilan Laboratorium | Frontend | Pencarian `laboratory-management`, `lab-order`, `labOrder`, dan `labSpecimen` pada `QuilvianSystemFrontendDev/src@688daff90` **tidak menghasilkan satu berkas pun**. Tidak ada route `src/app/health-services/laboratory-*` | `Missing` | Seluruhnya dibangun dari nol: route, layar, state, API service, dan konstanta | **Tinggi.** Porsi pekerjaan frontend Rilis 1 adalah seratus persen |
| `CAP-22` | Pola berlapis untuk membangun modul baru | Frontend | Modul `pharmacy-management@688daff90` memakai tujuh lapis konsisten: `src/app/health-services/pharmacy-management/`, `src/components/features/health-services/pharmacy-management/`, `src/components/view/health-services/pharmacy-management/`, `src/lib/constants/health-services/pharmacy-management/`, `src/lib/hooks/health-services/pharmacy-management/`, `src/lib/services/health-services/pharmacy-management/`, `src/style/health-services/pharmacy-management/` | `Ready to reuse` | Tidak ada | Rendah. `LAB-DEC-010` memang memerintahkan mengikuti pola modul sejenis, dan polanya jelas |
| `CAP-23` | Pemanggilan API dan pengelolaan state | Frontend | `src/lib/axiosInstance@688daff90`; potongan state Redux di `src/lib/state/slice/@688daff90` | `Ready to reuse` | Tidak ada | Rendah |

### Klaster Pengujian

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `CAP-24` | Bukti otomatis bahwa aturan sampel dan batas kewenangan ditegakkan | Laboratorium | `Tests/QuilvianSystemBackend.BillingTests/Laboratory/LaboratorySpecimenLifecycleTests.cs@c87d9c0` berisi 18 pengujian (dikoreksi dari 19 pada impact scan 2026-09-02), antara lain `#SebelumDinyatakanLayak_TidakAdaTagihanYangTerbentuk`, `#PengambilanUlangKesalahanInternal_HanyaMenghasilkanSatuTagihan`, `#PembatalanSetelahLayak_TidakMenghapusTagihanDanMemakaiRevisiBaru`, `#SampelDitolak_TidakMenerbitkanFaktaApaPun`. `LaboratoryAuthorityTests.cs@c87d9c0` berisi 12 pengujian batas kewenangan | `Ready to reuse` | Tidak ada. Pengujian hasil pemeriksaan harus ditambahkan sendiri | Rendah. Justru menjadi contoh gaya pengujian yang bisa ditiru untuk slice hasil |

---

## Conflict

### `CONF-01` — Status `Draft` pada pesanan lab tidak pernah bisa tercapai

| Field | Isi |
|---|---|
| Status | `Conflict` |
| Tingkat | Sedang |
| Memblokir | `DESIGN` bagian pembuatan pesanan |

**Apa yang ditemukan.** `LAB-INH-001` — keputusan terkunci dari `RJ-BIL-GATE-DEC-003` —
menyatakan alur pesanan dimulai dari `Draft`, lalu ke `Requested`. Di dalam kode, nilai `Draft`
memang ada pada `Enums/LaboratoryEnums.cs#LabOrderStatus.Draft@c87d9c0`, tetapi:

1. Pembuatan pesanan **selalu** langsung berstatus `Requested`, lihat
   `Services/LabOrderService.cs:136#OrderStatus = LabOrderStatus.Requested@c87d9c0`.
2. Tidak ada satu pun endpoint atau method yang menetapkan status menjadi `Draft`.
3. Satu-satunya tempat `Draft` disebut adalah pemeriksaan penjagaan di
   `Services/LabSpecimenService.cs:228@c87d9c0`, yaitu baris yang berbunyi
   "jika status pesanan `Draft` atau `Requested`". Baris itu tidak pernah benar-benar bertemu
   nilai `Draft` karena tidak ada yang membuatnya.

**Kenapa ini penting.** `LAB-INH-006` menyatakan dokter boleh mengubah pesanan secara langsung
**sampai** status `Requested`. Bila `Draft` tidak pernah ada, maka praktis dokter tidak punya
ruang menyunting sama sekali: begitu pesanan dibuat, ia langsung terkunci. Pertanyaannya
menjadi keputusan bisnis, bukan keputusan teknis.

**Contoh nyata.** dr. Rina sedang menyusun pesanan berisi lima pemeriksaan untuk pasien Andi.
Di tengah pengisian ia sadar salah memilih satu pemeriksaan. Dengan keadaan kode saat ini,
pesanan sudah berstatus `Requested` sejak tombol Simpan ditekan, sehingga koreksi harus lewat
jalur pembatalan — bukan sekadar menyunting draf.

**Yang perlu diputuskan manusia:** lihat pertanyaan penutup `Q-LAB-01`.

### `CONF-02` — `LAB-DEC-044` berdiri di atas fakta yang salah (ditemukan revision 3)

> **Ditutup 2026-09-14** oleh `LAB-DEC-046` dan `LAB-DEC-047` pada decision log revision 25.
> Uraian di bawah dipertahankan apa adanya sebagai catatan temuan; jangan dihapus.

**Apa yang ditulis keputusan itu.** `LAB-DEC-044` menetapkan Laboratorium *"memanggil Billing
dengan penunjuk kunjungan, menerima jawaban `Piutang Mitra` atau `Tunai`"*, dan membuka
`LAB-COORD-007` untuk meminta endpoint baca baru kepada pemilik `billing-kasir`.

**Tiga hal yang ditemukan scan ini membantahnya.**

**Pertama, metode pembayaran per kunjungan sudah ada dan bukan milik Billing.**
`Areas/HealthServices/RegistrationManagement/Models/RegPatientEncounterGuarantor.cs@466a7127`
menyimpan `PaymentType`, `PaymentMethodId`, `InsuranceProviderId`, `CompanyGuarantorId`,
`IsPrimary`, `Priority`, beserta belasan kolom salinan. Penjamin kunjungan adalah milik
**Registrasi**, ditetapkan saat pendaftaran — bukan sesuatu yang perlu ditanyakan ke Billing.

**Kedua, Laboratorium sudah mengirim metode pembayaran, bukan membacanya.**
`DTOs/LabPatientRegistrationDtos.cs:80,83` dan `:120,122` pada `466a7127` **sudah memuat**
`EncounterPaymentType PaymentType` dan `PaymentMethodId` sebagai bagian permintaan pendaftaran.
Artinya arah datanya berlawanan dengan yang diasumsikan `LAB-DEC-044`: layar Laboratorium
**menyodorkan** metode pembayaran kepada Registrasi, dan Registrasi yang menyimpannya.

**Ketiga, `Piutang Mitra` tidak ada.**
`Areas/HealthServices/RegistrationManagement/Enums/EncounterPaymentType.cs@466a7127` hanya
mengenal tiga nilai:

| Nilai | Label |
|---|---|
| `Cash = 1` | Tunai |
| `Insurance = 2` | Asuransi |
| `CompanyGuarantor = 3` | Penjamin Perusahaan |

Tidak ada nilai untuk piutang mitra rumah sakit perujuk. Yang paling mirip, `CompanyGuarantor`,
comment-nya menyebut *"hubungan pasien dengan perusahaan"* — yaitu tempat pasien bekerja, bukan
rumah sakit yang merujuknya. Keduanya bukan hal yang sama, dan menumpangkan satu pada yang lain
akan membuat laporan penjamin perusahaan memuat rumah sakit perujuk.

Comment yang sama juga memperingatkan: *"nilai `Cash` dan `Insurance` tidak boleh bergeser"* —
enum itu punya tata kelolanya sendiri (`RWI-ENC-PAYER-001`), sehingga menambah nilai baru bukan
perubahan sepele.

**Akibatnya.** `LAB-DEC-044` tidak dapat dipakai apa adanya, dan `LAB-COORD-007` ditujukan ke
modul yang salah. Yang sebenarnya terbuka bukan "minta endpoint baca ke Billing", melainkan:
apakah **`Piutang Mitra` menjadi nilai keempat** pada `EncounterPaymentType` milik Registrasi,
atau statusnya diturunkan dari kerja sama rumah sakit perujuk lewat jalan lain.

**Yang perlu diputuskan manusia:** lihat pertanyaan penutup `Q-LAB-06` dan `Q-LAB-07`.

---

## Unknown

### `UNK-01` — Apakah hasil laboratorium harus masuk ke dokumen rekam medis

| Field | Isi |
|---|---|
| Status | `Unknown` |
| Memblokir | `DESIGN` bagian penyajian hasil |

Modul `rekam-medis` ada dan aktif di `Areas/HealthServices/MedicalRecordManagement/@c87d9c0`.
Decision log Laboratorium menempatkan penyimpanan dokumen rekam medis **di luar scope**, dan
menyebut Laboratorium hanya "menyerahkan hasil sebagai isi rekam medis". Namun tidak ada
keputusan yang menyatakan bentuk penyerahan itu: apakah hasil lab cukup dibaca lewat layar
Laboratorium, ataukah harus tersalin menjadi dokumen di rekam medis. Audit tidak boleh
menebaknya. Lihat `Q-LAB-03`.

### `UNK-02` — Siapa pemilik kemampuan pemberitahuan tersimpan

| Field | Isi |
|---|---|
| Status | `Unknown` |
| Memblokir | `DESIGN` bagian pemberitahuan |

`LAB-DEC-012` mewajibkan pemberitahuan tersimpan dibangun pada Rilis 1. Audit membuktikan
kemampuan itu belum ada di mana pun (`CAP-18`). Yang belum jelas adalah siapa yang memilikinya:
bila dibangun di dalam modul Laboratorium, modul lain yang kelak membutuhkan pemberitahuan akan
membangun versinya sendiri dan terjadi duplikasi. Ini pertanyaan kepemilikan modul, bukan
pertanyaan teknis. Lihat `Q-LAB-02`.

---

## Kontrak As-Is

Endpoint yang benar-benar ada pada `c87d9c0`, disajikan seperti tampilan Swagger.

### `[Tags("Health Services / Laboratory Management / Lab Order")]`

Base route: `api/v1/health-services/laboratory-management/lab-orders`
Seluruh endpoint memerlukan login (`[Authorize]`).

| Method | Path | Permission | Ringkasan |
|---|---|---|---|
| `GET` | `/` | `LabOrder / Read` | Menampilkan daftar pesanan lab |
| `GET` | `/{id}` | `LabOrder / Read` | Menampilkan satu pesanan lab |
| `POST` | `/` | `LabOrder / Create` | Membuat pesanan lab baru, langsung berstatus `Requested` |
| `PUT` | `/{id}/start-process` | `LabOrder / Process` | Menandai pesanan mulai dikerjakan |
| `PUT` | `/{id}/complete` | `LabOrder / Process` | Menandai pesanan selesai |
| `PUT` | `/{id}/hold` | `LabOrder / Hold` | Menahan sementara pesanan |
| `PUT` | `/{id}/resume` | `LabOrder / Hold` | Melanjutkan pesanan yang ditahan |
| `PUT` | `/{id}/cancel` | `LabOrder / Update` | Membatalkan pesanan |

### `[Tags("Health Services / Laboratory Management / Lab Specimen")]`

Base route: `api/v1/health-services/laboratory-management/lab-specimens`
Seluruh endpoint memerlukan login (`[Authorize]`).

| Method | Path | Permission | Ringkasan |
|---|---|---|---|
| `GET` | `/rejection-reasons` | `LabSpecimen / Read` | Daftar alasan penolakan sampel |
| `GET` | `/by-order/{labOrderId}` | `LabSpecimen / Read` | Daftar sampel milik satu pesanan |
| `GET` | `/by-order/{labOrderId}/history` | `LabSpecimen / Read` | Riwayat perpindahan status |
| `POST` | `/by-order/{labOrderId}` | `LabSpecimen / Plan` | Menambah sampel pada pesanan |
| `POST` | `/{id}/collect` | `LabSpecimen / Collect` | Mencatat pengambilan sampel |
| `POST` | `/{id}/receive` | `LabSpecimen / Receive` | Mencatat sampel tiba di lab |
| `POST` | `/{id}/accept` | `LabSpecimen / Accept` | Menyatakan sampel layak periksa |
| `POST` | `/{id}/reject` | `LabSpecimen / Accept` | Menolak sampel dengan alasan terkendali |
| `POST` | `/{id}/request-recollection` | `LabSpecimen / Accept` | Meminta pengambilan ulang sampel |
| `POST` | `/{id}/hold` | `LabSpecimen / Hold` | Menahan sampel sementara |
| `POST` | `/{id}/resume` | `LabSpecimen / Hold` | Melanjutkan sampel yang ditahan |
| `POST` | `/{id}/cancel` | `LabSpecimen / Update` | Membatalkan sampel |

**Catatan kontrak.** Menolak sampel dan meminta pengambilan ulang memakai permission yang sama
dengan menyatakan layak, yaitu `LabSpecimen / Accept`. Ini **sesuai** `LAB-INH-007`, yang
memang menyebut "penerimaan/penolakan" sebagai satu kewenangan. Yang dipisah tegas adalah
pengambilan dan penetapan layak — dan pemisahan itu dijaga pengujian
`LaboratoryAuthorityTests.cs#PermissionPengambilanDanPenetapanLayak_TidakBolehSama@c87d9c0`.

### Tabel basis data yang sudah ada

| Tabel | Model | Migration |
|---|---|---|
| `LabOrder` | `LabOrder` | `20260815103436_initializeLabOrder` |
| `TrxLabSpecimen` | `TrxLabSpecimen` | `20260824091610_AddLaboratorySpecimenLifecycle` |
| `TrxLabTransitionHistory` | `TrxLabTransitionHistory` | `20260824091610_AddLaboratorySpecimenLifecycle` |
| `MstLabRejectionReason` | `MstLabRejectionReason` | `20260824091610_AddLaboratorySpecimenLifecycle` |

Terdaftar di `Repositories/ApplicationDbContext.cs:648-654@c87d9c0`.
Layanan terdaftar di `Program.cs:285-286@c87d9c0`.

---

## Ketidakcocokan Frontend dan Backend

| Temuan | Keterangan |
|---|---|
| Seluruh 20 endpoint Laboratorium **tidak punya satu pun pemakai di frontend** | Backend Laboratorium sudah berjalan selama beberapa bulan tanpa layar. Artinya alur pesanan dan sampel selama ini hanya bisa dijalankan lewat pemanggilan API langsung, bukan oleh petugas lab lewat aplikasi |
| Tidak ada menu Laboratorium | Permission `LabOrder` dan `LabSpecimen` terdaftar otomatis lewat `AccessMenuSeeder`, tetapi tidak ada menu yang menampilkannya kepada pengguna |

Ini bukan kerusakan, melainkan konsekuensi wajar dari urutan pengerjaan: backend dibangun lebih
dulu sebagai bagian dari pekerjaan Billing. Tetapi konsekuensinya perlu dicatat: **belum ada
bukti sama sekali bahwa alur ini pernah dipakai petugas sungguhan.**

---

## Pemicu Impact Scan

Peta ini menjadi basi dan wajib dipindai ulang secara terbatas bila salah satu terjadi:

| Pemicu | Yang perlu diperiksa ulang |
|---|---|
| Backend bergerak dari `c87d9c0` | `CAP-01` sampai `CAP-07`, `CAP-11` sampai `CAP-19`, `CAP-24`, dan seluruh kontrak as-is |
| Frontend bergerak dari `688daff90` | `CAP-20` sampai `CAP-23` |
| Ada migration baru menyentuh tabel berawalan `Lab` | `CAP-01` sampai `CAP-05` dan tabel basis data |
| `ClinicalMilestoneFactProducer` berubah | `CAP-11` dan pengujian `CAP-24` |
| `AccessMenuSeeder` atau `AccessPermissionService` berubah | `CAP-13` dan `CAP-14` |
| Blueprint `rawat-jalan` menerbitkan amendment baru menyentuh Laboratorium | Seluruh keputusan warisan `LAB-INH-001` sampai `LAB-INH-013` |

---

## Pertanyaan Penutup untuk `/grill-me`

Pertanyaan berikut **tidak dijawab oleh audit ini**. Semuanya memerlukan keputusan Yoga Aji
Pratama sebagai pemilik modul, sebagian bersama pihak lain.

### `Q-LAB-01` — Apakah pesanan lab perlu tahap draf?

**Latar:** `CONF-01`. Keputusan terkunci `LAB-INH-001` menyebut alur dimulai dari `Draft`,
tetapi kode tidak pernah membuat status itu, sehingga pesanan langsung terkunci begitu dibuat.

**Yang harus diputuskan:** apakah dokter diberi tahap draf untuk menyusun pesanan sebelum
dikirim ke lab, atau alur langsung `Requested` seperti sekarang diterima sebagai perilaku sah
dan `LAB-INH-001` yang perlu diamandemen.

**Pemilik keputusan:** Yoga Aji Pratama, dengan rujukan ke pemilik blueprint `rawat-jalan`
karena `LAB-INH-001` diwarisi dari sana.

### `Q-LAB-02` — Pemberitahuan tersimpan dibangun sebagai milik siapa?

**Latar:** `CAP-18` dan `UNK-02`. Kemampuan ini belum ada di mana pun, sedangkan `LAB-DEC-012`
mewajibkannya di Rilis 1.

**Yang harus diputuskan:** apakah pemberitahuan tersimpan dibangun sebagai kemampuan platform
yang bisa dipakai semua modul, atau dibangun khusus untuk Laboratorium lebih dulu dengan risiko
duplikasi di kemudian hari.

**Pemilik keputusan:** Yoga Aji Pratama bersama pemilik platform.

### `Q-LAB-03` — Apakah hasil lab harus tersalin ke rekam medis?

**Latar:** `UNK-01`. Modul `rekam-medis` sudah ada, tetapi bentuk penyerahan hasil belum pernah
diputuskan.

**Yang harus diputuskan:** apakah hasil laboratorium cukup dibaca dari layar Laboratorium, atau
harus menjadi dokumen tersimpan di rekam medis pasien.

**Pemilik keputusan:** Yoga Aji Pratama bersama pemilik modul `rekam-medis`.

### `Q-LAB-04` — Siapa yang mengisi daftar alasan penolakan sampel?

**Latar:** `CAP-05`. Tabel alasan penolakan hanya punya endpoint baca. Tidak ada layar
pengelolaan dan tidak ditemukan pengisian awal.

**Yang harus diputuskan:** apakah daftar alasan diisi sekali oleh tim teknis sebagai data awal
yang tetap, atau perlu layar pengelolaan agar kepala instalasi bisa menambah dan menonaktifkan
alasan sendiri.

**Pemilik keputusan:** Yoga Aji Pratama.

### `Q-LAB-05` — Di mana batas nilai dan batas waktu cito disimpan?

**Latar:** `CAP-06` dan `CAP-07`. Katalog pemeriksaan saat ini menumpang `MstProcedure` milik
`master-data`, sedangkan tabel batas nilai yang diwajibkan `LAB-DEC-006` dan `LAB-DEC-013`
belum ada.

**Yang harus diputuskan:** apakah batas nilai ditambahkan sebagai kolom pada `MstProcedure`
milik `master-data`, atau menjadi tabel tersendiri milik Laboratorium yang menunjuk ke
`MstProcedure`.

**Pemilik keputusan:** Yoga Aji Pratama bersama pemilik `master-data`.

### `Q-LAB-06` — `Piutang Mitra` menjadi nilai keempat, atau diturunkan lewat jalan lain?

> **Dijawab 2026-09-14** oleh `LAB-DEC-046`: nilai keempat, ditambahkan secara aditif.
> Berstatus `draft` sampai pemilik `registration-management` menyetujui.

**Kenapa ditanyakan.** `EncounterPaymentType@466a7127` hanya mengenal `Cash`, `Insurance`, dan
`CompanyGuarantor`. `LAB-EVD-001` meminta `Piutang Mitra` untuk rumah sakit perujuk berstatus
PKS, dan nilai itu tidak ada. `CompanyGuarantor` bukan penggantinya — comment-nya menyatakan
nilai itu untuk hubungan pasien dengan **tempatnya bekerja**, bukan rumah sakit yang merujuk.

**Yang harus diputuskan:** apakah `EncounterPaymentType` bertambah satu nilai `PartnerReceivable`
milik Registrasi, atau piutang mitra diperlakukan sebagai bentuk khusus dari penjamin yang sudah
ada, atau rumah sakit memang tidak mengenal skema ini dan `RULE-012` pada `LAB-EVD-001`
ditolak.

**Yang perlu disadari:** enum itu punya tata kelola sendiri (`RWI-ENC-PAYER-001`) dan comment-nya
melarang nilai lama bergeser. Menambah nilai bukan perubahan sepele, dan berdampak ke seluruh
modul yang membaca penjamin kunjungan.

**Pemilik keputusan:** Yoga Aji Pratama bersama pemilik `registration-management` dan
`billing-kasir`.

### `Q-LAB-07` — Metode pembayaran di layar penerimaan: dibaca, atau tetap disodorkan?

> **Dijawab 2026-09-14** oleh `LAB-DEC-047`: dibedakan menurut jalur masuk — diturunkan untuk
> rujukan luar, dinyatakan petugas untuk datang langsung.

**Kenapa ditanyakan.** `LAB-DEC-044` menetapkan Laboratorium **membaca** metode pembayaran dari
Billing dan menampilkannya baca-saja. Tetapi `LabPatientRegistrationDtos.cs:80,83,120,122`
menunjukkan Laboratorium hari ini justru **mengirimkannya** ke Registrasi sebagai bagian
pendaftaran. Arah datanya berlawanan, dan keduanya tidak dapat berlaku bersamaan.

**Yang harus diputuskan:** apakah petugas lab tetap boleh memilih metode pembayaran saat
mendaftarkan pasien — sebagaimana kode berjalan hari ini — atau kemampuan itu dicabut dan
diganti tampilan baca-saja sebagaimana `LAB-DEC-044`.

**Yang perlu disadari:** bila kemampuan memilih dicabut, `RegisterLabWalkInRequest` dan
`RegisterLabExternalReferralRequest` yang sudah dipakai `FE-LAB-05` harus berubah, dan pasien
datang langsung yang membayar tunai kehilangan cara menyatakannya di titik pendaftaran.
Bila dipertahankan, `BR-25` perlu diperiksa ulang — memilih metode pembayaran lebih dekat ke
*memutuskan apakah pasien membayar* daripada sekadar menampilkan.

**Pemilik keputusan:** Yoga Aji Pratama bersama pemilik `billing-kasir`.

---

## Riwayat Revisi

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 1 | 2026-09-01 | Audit penuh pertama pada backend `c87d9c0` dan frontend `688daff90`. 24 kemampuan diklasifikasikan, 1 conflict dan 2 unknown dicatat, 5 pertanyaan penutup diajukan | `draft` |
| 3 | 2026-09-14 | *Impact scan* terbatas atas kemampuan yang terdampak `LAB-DEC-037`..`LAB-DEC-045`, menutup `LAB-OPEN-022` dan `LAB-OPEN-023`. Pergeseran **jauh lebih besar** dari revision 2: BE 298 commit, FE 155 commit. Tujuh dari sembilan keputusan amendment terbukti berdiri di atas fakta yang masih benar. **`LAB-DEC-039` ternyata sudah dikerjakan kode** sebagai `VAL-18` — bukan aturan baru, melainkan pembetulan `AC-20` yang sudah lama tidak sesuai kode. **`LAB-DEC-044` berdiri di atas fakta yang salah** dan dibuka sebagai `CONF-02`: metode pembayaran per kunjungan sudah ada pada `RegPatientEncounterGuarantor` milik Registrasi, Laboratorium justru sudah mengirimkannya lewat `LabPatientRegistrationDtos`, dan `Piutang Mitra` tidak ada pada `EncounterPaymentType`. `LAB-DEC-043` naik dari `Extend` menjadi `Missing` — data induk perujuk **tidak punya endpoint tulis sama sekali**, dan satu-satunya pengisinya adalah `LabDummyDataSeeder`. `F5` dicabut: frontend Laboratorium kini berdiri penuh, 31 berkas. Empat entity berganti nama, `TrxPatientEncounter` menjadi `RegPatientEncounter`. Dua pertanyaan penutup baru `Q-LAB-06` dan `Q-LAB-07` | `draft` |
| 2 | 2026-09-02 | *Impact scan* terbatas atas `CAP-11` dan bagian utang teknis, sesuai penanda `STALE` pada manifest. **Tidak ada status kemampuan yang berubah**; `STALE` dicabut. Sepuluh kemampuan berisiko tinggi diverifikasi silang. Dua koreksi faktual: jumlah pengujian `CAP-24` 19 → 18, dan tanggal audit revision 1 yang tidak mungkin benar karena `c87d9c0` baru dibuat 2026-09-02 | `draft` |
