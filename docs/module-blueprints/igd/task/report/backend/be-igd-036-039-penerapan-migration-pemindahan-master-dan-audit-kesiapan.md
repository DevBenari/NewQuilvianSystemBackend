# BE-IGD-036–039 — Penerapan Migration, Pemindahan Master Data IGD, dan Audit Kesiapan

## Metadata

| Field | Nilai |
| --- | --- |
| Task | `BE-IGD-036` sampai `BE-IGD-039` |
| Dasar keputusan | `IGD-DEC-092`, `IGD-DEC-104`, `IGD-DEC-106`, `IGD-DEC-107`; arahan owner 27 Agt 2026 |
| Commit dasar | `300922c` |
| Tanggal | 27 Agustus 2026 |
| Status | `BE-IGD-036`–`038` **selesai di working tree**, belum di-commit. `BE-IGD-039` **temuan, belum diperbaiki — menunggu keputusan** |

---

## Ringkasan

| Task | Judul | Hasil |
| --- | --- | --- |
| `BE-IGD-036` | Migration `ImplementIgdFullPatientJourney` diterapkan dan jalur simpan pengkajian dibuktikan | **Selesai** |
| `BE-IGD-037` | Master data IGD pindah ke modul `EmergencyInstallationManagement` | **Selesai** |
| `BE-IGD-038` | Dua kolom respons daftar yang selalu kosong diperbaiki | **Selesai** |
| `BE-IGD-039` | Kewenangan unit membandingkan dua domain identitas yang berbeda | **Temuan — belum diperbaiki** |

---

## `BE-IGD-036` — Migration diterapkan, pengkajian terbukti tersimpan

Migration `20260826090500_ImplementIgdFullPatientJourney` berstatus `Pending` sejak 26 Agustus.
Selama itu **pengkajian IGD tidak mungkin disimpan**, dan sebetulnya **login pun rusak** bagi
siapa pun yang menjalankan cabang ini: model EF sudah memuat `MstServiceUnit.OrganizationUnitId`
sedangkan kolomnya belum ada, dan jalur login ikut membaca tabel itu.

### Keadaan basis data sebelum penerapan

| Pemeriksaan | Hasil |
| --- | --- |
| `TrxPatientAssessment."QueueId"` | `NOT NULL` |
| `TrxDoctorConsultation."QueueId"` | `NOT NULL` |
| `MstServiceUnit."OrganizationUnitId"` | tidak ada |
| `TrxEmergencyDeparture` | belum ada; masih `TrxEmergencyTransfer` |
| **`IGD-UNK-03`** — baris `TrxEmergencyTransfer` dengan kolom penempatan terisi | **0** (tabelnya **0 baris**) |

`IGD-UNK-03` dengan demikian **terjawab**. Arsitektur bagian 6.2 melarang membuang
`FromRoomId`/`ToRoomId`/`FromBedId`/`ToBedId` selama jumlahnya belum diketahui; jumlahnya nol,
sehingga langkah itu tidak menghilangkan apa pun. Tabel arsip
`TrxEmergencyDepartureLegacyPlacement` tetap dibuat dan tetap kosong, sesuai rancangan.

### Cara penerapannya

`dotnet ef database update` **menolak jalan**:

```
PendingModelChangesWarning: The model for context 'ApplicationDbContext' has pending changes.
```

Drift itu **bukan** berasal dari IGD — snapshot repo ini memang sudah tidak sepadan dengan model
karena migration modul lain. Penerapan karena itu memakai script idempotent hasil
`dotnet ef migrations script --idempotent`, yang isinya **identik** dengan yang akan dijalankan
`database update`, termasuk baris `__EFMigrationsHistory`. Seluruhnya satu transaksi, dengan
empat pra-pemeriksaan lebih dulu.

### Sesudah penerapan

| Pemeriksaan | Hasil |
| --- | --- |
| `QueueId` pada kedua tabel | `NULL`-able |
| `MstServiceUnit."OrganizationUnitId"` | ada |
| Tabel | `TrxEmergencyDeparture`, `TrxEmergencyDepartureEvent`, `TrxEmergencyHandoverOrderItem`, `TrxEmergencyDepartureLegacyPlacement` |
| `__EFMigrationsHistory` | memuat `20260826090500_ImplementIgdFullPatientJourney` |
| `EncounterType` kunjungan IGD | 3 baris menjadi `2` (Emergency) |

### Basis data lebih maju daripada cabang ini

`__EFMigrationsHistory` memuat dua migration yang **berkasnya tidak ada** di cabang ini:

- `20260826025451_AddTableBillingKasirPArt3`
- `20260826101500_RenameClinicalMilestoneFactToBillingOwnership`

Keduanya diterapkan rekan tim dari cabang lain. Tidak menghalangi penerapan IGD — EF hanya
menerapkan yang belum tercatat — tetapi **wajib diperiksa sebelum merge**.

### Bukti jalur simpan

Dibuktikan lewat API backend yang dibangun dari working tree, dengan payload **persis** seperti
yang dibentuk layar (`buildPatientAssessmentPayload`, `queueId: null`):

```
POST /api/v1/health-services/clinical-management/patient-assessments
=> 200  ASM-20260827-00003  queueId=null  assessmentStatus=2 (Completed)
```

Dibaca ulang dari basis data, seluruh kolom terisi benar: tujuh kolom nyeri, `NurseNote`,
serta kolom turunan `BMI = 24.80`, `MeanArterialPressure = 109.67`, `EarlyWarningScore = 3`.

Tiga baris pengkajian uji sesudahnya **dibatalkan** lewat `PATCH /{id}/cancel` dengan alasan
tertulis, sehingga riwayatnya utuh tetapi tidak terbaca sebagai pengkajian klinis nyata.

---

## `BE-IGD-037` — Master data IGD pindah ke modul IGD

Menutup bagian ketiga `BE-IGD-013` yang **ditahan sejak 18 Agustus** karena roadmap tidak
menyatakan mana dari dua cara yang dimaksud. Owner memilih: master data IGD menjadi bagian
modul IGD, mengikuti pola `BillingManagement`.

### Struktur

```
Areas/HealthServices/EmergencyInstallationManagement/
├── Controllers/  DTOs/  Enums/  Models/  Services/      ← 9 controller transaksi
└── MasterData/
    ├── Controllers/ (6)  DTOs/ (6)  Models/ (6)  Seeders/ (1)  Services/ (1)

Repositories/Configurations/HealthServices/EmergencyInstallationManagement/
├── Trx*Configuration.cs            (11)
└── MasterData/Mst*Configuration.cs  (6)
```

Konfigurasi EF **tetap di `Repositories`** atas arahan owner, berbeda dari `BillingManagement`
yang menaruhnya di dalam folder modul. Ini penyimpangan yang disengaja dan dicatat di sini
supaya tidak dibaca sebagai kelalaian.

### Yang berubah

| Aspek | Sebelum | Sesudah |
| --- | --- | --- |
| Folder Areas | `Areas/HealthServices/MasterData/{Controllers,DTOs,Models,Seeders,Services}/` | `Areas/HealthServices/EmergencyInstallationManagement/MasterData/…` |
| Folder Configuration | `Repositories/Configurations/HealthServices/MasterData/EmergencyInstallationManagement/` | `Repositories/Configurations/HealthServices/EmergencyInstallationManagement/MasterData/` |
| Namespace | `…Areas.HealthServices.MasterData.EmergencyInstallationManagement.*` | `…Areas.HealthServices.EmergencyInstallationManagement.MasterData.*` |
| Route API | `…/master-data/emergency-installation-management/<res>` | `…/emergency-installation-management/master-data/<res>` |
| Tag Swagger | `… / Master Data / Emergency Installation Management / …` | `… / Emergency Installation Management / Master Data / …` |
| `moduleCode` | `HEALTH_SERVICE_MASTER_DATA` | `HEALTH_SERVICE_EMERGENCY_INSTALLATION_MANAGEMENT` |
| `SortOrder` controller | 1–6 | 10–15 (sesudah 9 controller transaksi) |

41 berkas ikut disesuaikan: 11 controller/model/service IGD transaksi, `Program.cs`,
`ApplicationDbContext.cs`, satu berkas test, dan `ApplicationDbContextModelSnapshot.cs`.

### Dua hal yang mudah terlewat

**1. `ApplicationDbContextModelSnapshot.cs` wajib ikut.** EF menyimpan nama tipe entity sebagai
**string** di snapshot. Tanpa penyelarasan, `migrations add` berikutnya akan membaca keenam
entity master sebagai "dibuang lalu dibuat lagi" dan menghasilkan migration yang menghapus
tabel berisi data. Berkas `*.Designer.cs` **sengaja tidak** disentuh: itu potret riwayat, dan
EF tidak membacanya untuk mendiff model.

**2. Pendaftaran hak akses lama harus ditutup.** `AccessMenuSeeder.EnsureControllerAsync`
mencari controller berdasarkan pasangan `(ModuleId, ControllerName)`. Perpindahan modul
membuatnya **membuat baris baru**, bukan memindahkan yang lama. Tanpa penanganan, layar
Manajemen Role menampilkan keenam master **dua kali** dan petugas tidak punya cara membedakan
mana yang menegakkan izin.

Ditangani `NormalizeEmergencyMasterDataModuleMoveAsync`, mengikuti pola
`NormalizeEmployeeSelfServiceLegacyEntriesAsync` yang sudah ada untuk perpindahan serupa.
Baris lama **ditutup**, bukan dihapus.

Diperiksa lebih dulu: **nol baris `SysAccessPolicy`** menunjuk keenam controller itu, sehingga
tidak ada izin yang hangus.

### Bukti sesudah seeder berjalan

| `ModuleCode` | Controller | `IsDelete` | Terlihat di Role Access | Action aktif |
| --- | --- | --- | --- | ---: |
| `HEALTH_SERVICE_EMERGENCY_INSTALLATION_MANAGEMENT` | keenamnya | `false` | ya | 4 |
| `HEALTH_SERVICE_MASTER_DATA` | keenamnya | `true` | tidak | 0 |

Keenam endpoint dijalankan pada route baru: **200**, dengan data seeder utuh — 6 level triage,
17 indikator, 6 cara kedatangan, 11 jenis kasus, 9 jenis tindak lanjut, 1 pengaturan.
Route lama: **404**.

---

## `BE-IGD-038` — Dua kolom respons daftar yang selalu kosong

Ditemukan saat membuktikan jalur simpan: kolom yang **dideklarasikan layar** ternyata tidak
pernah ada di respons daftar. Ini tidak pernah tampil sebagai galat — kolomnya hanya kosong
selamanya, dan datanya tersimpan utuh di basis data.

### 1. `PatientAssessmentResponse.NurseNote`

`NurseNote` hanya ada di `PatientAssessmentDetailResponse`. Tabel **Riwayat Assesmen Awal** pada
layar pengkajian IGD membacanya langsung dari hasil daftar, tanpa membuka detail satu per satu,
sehingga kolom "Catatan perawat" selalu kosong.

Diperbaiki: `NurseNote` dinaikkan ke kelas dasar `PatientAssessmentResponse`, deklarasi
kembarnya di kelas turunan dibuang (`CS0108`), dan proyeksi daftar mengisinya. **Aditif** —
nol pemakai lama terganggu.

### 2. `EmergencyProcedureDetailResponse` — empat kolom identitas tindakan

Tab **Tindakan** menampilkan `procedureName`, `performedAt`, `quantity`, `performedByName`.
Keempatnya **tidak ada** di DTO: baris ini hanya menyimpan rincian khas IGD — skin test, ATS,
rute obat — sedangkan identitas tindakannya milik `TrxPatientProcedure`. Akibatnya baris tampil
tanpa judul dan tiga kolom kosong; hanya `notes` yang terisi.

Diperbaiki dengan mengambil dari tindakan induk lewat navigasi `PatientProcedure`:

| Kolom | Sumber |
| --- | --- |
| `ProcedureName` | `PatientProcedure.ProcedureNameSnapshot` |
| `PerformedAt` | `CompletedAt` ?? `StartedAt` ?? `ProcedureDateTime` |
| `Quantity` | `PatientProcedure.Quantity` |
| `PerformedByName` | `PatientProcedure.Doctor.FullName` |

`Include(...).ThenInclude(...)` ditambahkan pada daftar dan `GET /{id}`; balasan `POST`/`PUT`
memuat navigasinya eksplisit supaya bentuknya sama dengan balasan daftar.

Waktu selesai lebih dipercaya daripada waktu rencana, lalu turun ke waktu mulai dan waktu
tindakan, supaya baris yang belum tuntas tetap punya waktu yang berarti alih-alih kosong.

### Pelajaran

Kolom yang dideklarasikan layar tetapi tidak ada di respons **tidak pernah melempar galat**.
Satu-satunya cara menemukannya adalah membandingkan daftar kolom layar dengan isi respons
sungguhan. Pemeriksaan itu dijalankan atas seluruh sembilan bagian layar pengkajian; sesudah
perbaikan, **nol kolom hilang** pada bagian yang punya data.

---

## `BE-IGD-039` — TEMUAN: kewenangan unit membandingkan dua domain identitas berbeda

**Belum diperbaiki. Ini menghalangi `MVP-6` dan tiga langkah transfer pasien.**

`EmergencyUnitAuthorityService.PeriksaAsync` memutuskan kewenangan begini:

```csharp
x.DepartmentId == unit.OrganizationUnitId.Value
```

Kedua sisi menunjuk tabel yang **berbeda**:

| Kolom | Foreign key menunjuk ke |
| --- | --- |
| `AspNetUserOrganization."DepartmentId"` | `MstDepartment` |
| `MstServiceUnit."OrganizationUnitId"` | `MstOrganizationUnit` |

Diperiksa di basis data: **nol** id `MstOrganizationUnit` yang kebetulan sama dengan id
`MstDepartment` (5 simpul organisasi, 10 departemen). Perbandingan itu karena itu **tidak akan
pernah benar**.

Akibatnya: begitu Master Data mengisi `MstServiceUnit.OrganizationUnitId`, ketiga langkah yang
sekarang ditolak dengan "unit belum dipetakan" akan **tetap ditolak**, hanya berganti pesan
menjadi "Anda tidak bertugas di unit ini" — untuk **setiap** pengguna. Jadi `MVP-6` bukan hanya
menunggu data; kodenya juga belum benar.

### Jembatan yang tampaknya benar

`MstOrganizationUnit` punya kolom `DepartmentId` (nullable) yang menunjuk `MstDepartment`.
Bacaan yang masuk akal: seorang pengguna berwenang atas satu unit layanan bila ia ditugaskan
pada **departemen yang menaungi simpul organisasi** unit itu. Perbaikannya berarti menyelesaikan
`OrganizationUnit.DepartmentId` lebih dulu, baru dibandingkan.

**Tidak saya kerjakan** karena ini mengubah aturan otorisasi, dan `IGD-DEC-092` sendiri masih
berstatus keputusan sementara yang menunggu Security/Privacy owner.

### Temuan menyertai: jalan keluar beralasan tidak ada di kode

Pesan penolakannya berbunyi *"Lanjutkan dengan menyertakan alasan, atau minta Master Data
melengkapi pemetaan unit ini."* Padahal:

- `Hasil.UnitBelumDipetakan` **dideklarasikan dan diisi, tetapi tidak pernah dibaca** oleh
  pemanggil mana pun di seluruh repo.
- Nol request DTO pada `arrive`, `accept-handover`, dan `order-items` punya kolom alasan
  penembusan.

Komentar di kode menyebut "jalan keluar beralasan yang dipanggil terpisah oleh controller",
tetapi controller-nya tidak pernah memanggilnya. `IGD-DEC-092` mensyaratkan **fail-closed
*beserta* jalan keluar beralasan**; kode baru memenuhi separuhnya. Pesan galat itu menyuruh
petugas melakukan sesuatu yang API-nya tidak menerima.

---

## Audit kesiapan — perjalanan pasien penuh sampai transfer

Dijalankan terhadap instance yang dibangun dari working tree, memakai kunjungan uji berpasien
belum teridentifikasi supaya nol data pasien nyata tersentuh.

| Tahap | Hasil |
| --- | --- |
| `POST emergency-visits` | 200 — `Arrived` |
| `PATCH registration-status` → `Provisional` | 200 |
| `PATCH visit-status` → `WaitingForTriage` | 200 |
| `POST emergency-triages` (`Completed`) | 200 — kunjungan naik ke **`Triaged`** |
| `PATCH visit-status` → `InTreatment` → `AwaitingDisposition` | 200 |
| `POST emergency-dispositions` → `Confirmed` → `Executed` | 200 — kunjungan naik ke **`Disposed`** |
| `POST emergency-departures` | 201 |
| `POST submit-handover` | 200 |
| `POST depart` | 200 |
| `POST arrive` | **403** — `BE-IGD-039` |
| `POST accept-handover` | **403** — `BE-IGD-039` |
| `POST order-items` | **403** — `BE-IGD-039` |
| `PATCH complete` | 409 — benar; `PhysicalStatus` masih `Departed`, akibat lanjutan |

**14 langkah lulus, 3 terhalang, 1 akibat lanjutan.** Rantai status penuh
`Arrived → WaitingForTriage → Triaged → InTreatment → AwaitingDisposition → Disposed`
terbukti bekerja, begitu pula transfer sampai `depart`.

Dua percobaan pertama sempat gagal karena **skripnya**, bukan backend, dan keduanya layak
dicatat karena mudah terulang:

1. `PATCH registration-status` **tidak menyentuh** `VisitStatus`. Menyelesaikan pendaftaran saja
   tidak memindahkan kunjungan dari `Arrived`.
2. `EmergencyTriageStatus.Completed = 3`, bukan `2`. Triase yang dibuat dengan status
   `InProgress` **tidak** menaikkan kunjungan ke `Triaged`, dan itu memang benar.

Seluruh kunjungan uji beserta 21 baris turunannya sudah di-soft-delete. Tiga kunjungan IGD asli
tidak tersentuh.

---

## Verifikasi

```text
dotnet build ./QuilvianSystemBackend.sln --configuration Release
=> 0 Error(s), 151 Warning(s)      (jumlah warning tidak berubah dari sebelum perubahan)

dotnet test ./QuilvianSystemBackend.sln --configuration Release
=> Total 761, Passed 759, Failed 2
```

Dua kegagalan tetap milik `InPatientManagement` — `InpStatusHistoryAndMonitoringTests` dan
`InpCorrectionAndNewbornTests`, wilayah Muhammad Hamzah, sudah gagal sebelum pekerjaan ini.

| Pemeriksaan tambahan | Hasil |
| --- | --- |
| Sisa rujukan namespace lama di kode hidup | **0** |
| Git mencatat perpindahan sebagai rename | 26 berkas |
| 15 endpoint IGD dijalankan sungguhan | seluruhnya **200** |
| Kontrak OpenAPI memuat `nurseNote` dan empat kolom Tindakan | ya |

## Batas operasi

- Migration **diterapkan** ke basis data bersama atas persetujuan owner 27 Agt 2026.
- Nol commit, push, atau deploy.
- `BE-IGD-039` **tidak** diperbaiki — menunggu keputusan otorisasi.
- Pemetaan `MstServiceUnit.OrganizationUnitId` **tidak** diisi: 0 dari 18 unit, dan itu
  pekerjaan Master Data.

## Yang harus dijawab sebelum push ke server

1. **`BE-IGD-039`** — apakah kewenangan unit dibaca lewat `MstOrganizationUnit.DepartmentId`?
   Tanpa ini `MVP-6` tidak dapat bekerja meski datanya diisi.
2. **Jalan keluar beralasan** — dibuat, atau pesan galatnya yang dikoreksi supaya tidak
   menjanjikan jalan yang tidak ada?
3. **Frontend dan backend wajib naik bersamaan.** Route master data IGD berubah; frontend yang
   lama akan `404` terhadap backend baru, dan sebaliknya.
4. **Dua migration billing** yang sudah ada di basis data tetapi berkasnya tidak ada di cabang
   ini wajib diperiksa saat merge.
