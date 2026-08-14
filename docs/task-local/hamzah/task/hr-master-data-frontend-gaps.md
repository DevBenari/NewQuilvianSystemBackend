# HR Master Data — Daftar Kebutuhan Frontend yang Belum Didukung Backend

| | |
|---|---|
| Tanggal | 2026-08-11 |
| Branch | `MHamzah` |
| Pemicu | Audit master data HR di repo frontend (`docs/audit/audit-master-data-hr-2026-08-04.md`) + verifikasi ulang langsung ke kode backend pada 2026-08-11 |
| Migration | **Tidak ada** — dokumen ini tidak mengubah kode |
| Breaking change | **Tidak** — seluruh usulan di sini bersifat aditif (parameter opsional, field tambahan, endpoint baru) |

Dokumen ini **bukan laporan perubahan**, melainkan **register kebutuhan** yang dikumpulkan
untuk dipecah menjadi tugas. Tidak ada file kode yang disentuh saat dokumen ini dibuat.

---

## Ringkasan

| ID | Kebutuhan | Objek terdampak | Dampak kalau dibiarkan | Status |
|---|---|---|---|---|
| **GAP-1** | Filter rentang tanggal (`startDate`/`endDate`/`customPeriod`) | 35 controller | 34 halaman master data punya filter tanggal yang tampil tapi tidak berfungsi | ✅ **Selesai** 2026-08-11 |
| **GAP-2** | 6 master tanpa controller sama sekali | 6 model | 12+ form terpaksa memakai input UUID manual, tidak ada cara menambah datanya lewat UI | ✅ **Selesai** 2026-08-12 |
| **GAP-3** | `/options` hanya mengembalikan `Id` | 3 endpoint | Select relasi kosong tanpa label; frontend workaround ke endpoint list | ✅ **Selesai** 2026-08-11 |
| **GAP-4** | `CreateByName` / `UpdateByName` tidak ada | 8 DTO | Kolom **Dibuat Oleh** tampil `-` di 8 halaman master data | ✅ **Selesai** 2026-08-12 |

> 📋 **Ringkasan siap-uji per GAP yang sudah selesai** ada di `docs/hamzah/task-done/` —
> satu berkas per GAP, lengkap dengan tag Swagger, route, dan langkah pengujiannya:
> `gap-1-filter-rentang-tanggal.md`, `gap-2-controller-master-baru.md`,
> `gap-3-proyeksi-options.md`, `gap-4-create-by-name.md`.

> ✅ **Keempat GAP di tabel ini sudah tertutup.** Yang tersisa hanya T10 dan T11 di tabel
> *Usulan pemecahan tugas* — keduanya prioritas rendah dan tidak merusak apa pun.

Semua kebutuhan di atas **tidak memblokir** frontend. Frontend sudah menanganinya dengan
fallback yang jujur (filter tidak dikirim, input UUID manual, kolom `-`). Yang hilang adalah
fungsionalitas untuk pengguna, bukan kemampuan halaman untuk jalan.

### Cara dokumen ini diverifikasi

Bukan menyalin dokumen frontend apa adanya. Setiap angka di sini dihitung ulang dari kode
backend pada 2026-08-11:

- Filter tanggal dicek dari ada/tidaknya `[FromQuery] DateTime? startDate` pada action list
  tiap controller, bukan dari catatan audit lama.
- Angka di audit frontend (**21** entitas) sudah usang: audit itu hanya mencakup 36 entitas
  yang ada saat itu, sementara sekarang ada 63 modul master data HR di frontend.
  Angka yang benar sekarang: **35 controller**.

---

## GAP-1 — Filter rentang tanggal belum diterima 35 controller — ✅ SELESAI

> **Selesai 2026-08-11** — tugas T3 sampai T8 dikerjakan sekaligus. Seluruh 35 controller di
> bawah kini menerima `startDate`, `endDate`, dan `customPeriod`. Laporan di
> `docs/hamzah/report/hr-master-data-date-filter.md`.
>
> Pilot `benefit-type` sudah lebih dulu selesai dan ada di `origin/MHamzah` (commit
> `8ece36a`), laporan di `docs/hamzah/report/benefit-type-date-filter.md`.
>
> Yang dipakai adalah helper generik `WorkflowMasterDataSupport.ApplyDateFilter<T>()`,
> bukan menyalin helper per-controller seperti pilot — supaya logika tanggal tidak
> terduplikasi 35 kali. Semantiknya identik.
>
> Uraian di bawah dipertahankan sebagai catatan masalah aslinya.

### Masalahnya

Kontrak baku master data di frontend mewajibkan tiga filter selalu tampil di setiap halaman
list: **Tanggal Mulai**, **Tanggal Akhir**, dan **Periode**. Untuk 35 controller di bawah,
ketiga query param itu tidak diterima sama sekali, sehingga frontend menahan nilainya
(`unsupportedFilterKeys`) dan filternya menjadi kontrol yang terlihat aktif tapi tidak
mengubah hasil.

**27 controller lain sudah mendukungnya.** Jadi ini meratakan kontrak yang sudah ada, bukan
menambah fitur baru.

### Pola acuan — sudah ada di repo, jangan bikin baru

Ada dua implementasi yang sudah terbukti:

| Pola | Lokasi | Cocok untuk |
|---|---|---|
| Helper generik | `Areas/Corporate/HumanResource/MasterData/Workflow/Controllers/WorkflowMasterDataSupport.cs:34-103` | Semua controller — `ApplyDateFilter<T>()` menerima `IQueryable<T> where T : IdentityModel` |
| Helper per-controller | `Areas/Corporate/HumanResource/MasterData/PayrollAndBenefit/Controllers/AllowanceTypeController.cs:753-797` | Kalau tidak mau menambah dependency lintas-area |

`WorkflowMasterDataSupport.ApplyDateFilter<T>()` lebih disarankan: satu implementasi,
generic, sudah dipakai 6 controller Workflow.

Titik sentuh per controller — mengikuti `BenefitTypeController` yang sudah dikerjakan:

1. Action list menerima `[FromQuery] DateTime? startDate`, `[FromQuery] DateTime? endDate`,
   `[FromQuery] string? customPeriod`.
2. Query dibungkus: `var query = ApplyDateFilter(BuildBaseQuery(), startDate, endDate, customPeriod);`
3. `GET /filters/metadata` mengisi `CustomPeriods = BuildPeriodOptions()`.
4. DTO `<Entitas>DefaultFilterResponse` menambah `StartDate`, `EndDate`, `CustomPeriod`;
   DTO `<Entitas>FilterMetadataResponse` menambah `CustomPeriods`.

### Kontrak parameter (harus seragam di 35 controller)

| Parameter | Tipe | Perilaku |
|---|---|---|
| `startDate` | `DateTime?` | Batas awal inklusif, dinormalkan ke awal hari UTC |
| `endDate` | `DateTime?` | Batas akhir; di belakang layar eksklusif (`endDate` + 1 hari) supaya tanggal yang diminta ikut terhitung |
| `customPeriod` | `string?` | `today`, `last7days`, `thismonth`, `lastmonth`. **Diabaikan** kalau `startDate` atau `endDate` terisi |

Kolom yang difilter: **`CreateDateTime`**.
Kalau ketiganya kosong → query tidak berubah sama sekali, jadi pemanggil lama aman.
`GET /summary` **tidak** ikut difilter tanggal — mengikuti `AllowanceTypeController`, supaya
kartu ringkasan tetap menampilkan total keseluruhan.

### Daftar controller — dipecah per area

**AttendanceAndSchedule — 5**
`Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/Controllers/`

| Route | Controller |
|---|---|
| `shifts` | `ShiftController.cs` |
| `shiftgroups` | `ShiftGroupController.cs` |
| `shiftpatterns` | `ShiftPatternController.cs` |
| `workcalendars` | `WorkCalendarController.cs` |
| `workschedules` | `WorkScheduleController.cs` |

**LeaveAndOvertime — 7**
`Areas/Corporate/HumanResource/MasterData/LeaveAndOvertime/Controllers/`

| Route | Controller |
|---|---|
| `leave-types` | `LeaveTypeController.cs` |
| `leave-policies` | `LeavePolicyController.cs` |
| `leave-entitlement-policies` | `LeaveEntitlementPolicyController.cs` |
| `leave-carry-forward-policies` | `LeaveCarryForwardPolicyController.cs` |
| `leave-adjustment-reasons` | `LeaveAdjustmentReasonController.cs` |
| `overtime-policies` | `OvertimePolicyController.cs` |
| `overtime-rates` | `OvertimeRateController.cs` |

**Organization — 8**
`Areas/Corporate/HumanResource/MasterData/Organization/Controllers/`

| Route | Controller |
|---|---|
| `legal-entities` | `LegalEntityController.cs` |
| `job-families` | `JobFamilyController.cs` |
| `job-levels` | `JobLevelController.cs` |
| `employee-grades` | `EmployeeGradeController.cs` |
| `cost-centers` | `CostCenterController.cs` |
| `hospital-sites` | `HospitalSiteController.cs` |
| `organization-units` | `OrganizationUnitController.cs` |
| `work-locations` | `WorkLocationController.cs` |

> `OrganizationController.cs` (departments + positions) di area yang sama **sudah**
> mendukung — jangan ikut disentuh.

**PayrollAndBenefit — 6**
`Areas/Corporate/HumanResource/MasterData/PayrollAndBenefit/Controllers/`

| Route | Controller |
|---|---|
| `deduction-types` | `DeductionTypeController.cs` |
| `benefit-plans` | `BenefitPlanController.cs` |
| `benefit-eligibility-rules` | `BenefitEligibilityRuleController.cs` |
| `hazard-allowance-policies` | `HazardAllowancePolicyController.cs` |
| `on-call-allowance-policies` | `OnCallAllowancePolicyController.cs` |
| `shift-allowance-policies` | `ShiftAllowancePolicyController.cs` |

**Performance — 5**
`Areas/Corporate/HumanResource/MasterData/Performance/Controllers/`

| Route | Controller |
|---|---|
| `performance-cycles` | `PerformanceCycleController.cs` |
| `performance-rating-scales` | `PerformanceRatingScaleController.cs` |
| `performance-templates` | `PerformanceTemplateController.cs` |
| `kpi-catalogs` | `KpiCatalogController.cs` |
| `performance-templates/{id}/details` | `PerformanceTemplateDetailController.cs` — **belum punya halaman frontend**, prioritas paling rendah |

**EmployeeRelation — 4**
`Repositories/Configurations/Corporate/HumanResource/MasterData/EmployeeRelation/Controllers/`

| Route | Controller |
|---|---|
| `violationtypes` | `ViolationTypeController.cs` |
| `sanctiontypes` | `SanctionTypeController.cs` |
| `actiontypes` | `DisciplinaryActionTypeController.cs` |
| `casetypes` | `EmployeeRelationCaseTypeController.cs` |

> ⚠️ Letak keempat controller ini **ganjil** — ada di dalam `Repositories/Configurations/`,
> bukan di `Areas/`. Rutenya tetap jalan karena ASP.NET memindai atribut `[Route]`, bukan
> lokasi folder. Memindahkannya ke `Areas/Corporate/HumanResource/MasterData/EmployeeRelation/`
> layak jadi tugas terpisah, **jangan** dicampur dengan pekerjaan filter tanggal.

### Inkonsistensi terkait — nama parameter `period` vs `customPeriod`

Tiga controller di bawah **sudah** menerima `startDate` dan `endDate`, tetapi parameter
periodenya bernama **`period`**, bukan `customPeriod`:

| Route | Controller | Baris |
|---|---|---|
| `doctors` | `Workforce/Controllers/DoctorController.cs` | 392-394 |
| `employees` | `Workforce/Controllers/EmployeeController.cs` | 464-466 |
| `external-users` | `Workforce/Controllers/ExternalUserController.cs` | 341-343 |

Frontend sudah menyesuaikan diri (mis. `employee-constants.jsx` memakai key `period`), jadi
**tidak rusak**. Tapi ini berarti ada dua konvensi di API yang sama.

Usulan: tambahkan `customPeriod` sebagai alias yang diterima berdampingan dengan `period`
(bukan mengganti), supaya tidak breaking. Jadikan tugas terpisah dengan prioritas rendah.

### Yang sudah mendukung — jangan disentuh

> Setelah GAP-1 selesai, **seluruh 59 controller master data HR memakai `customPeriod`**
> (24 di daftar bawah + 35 yang baru dikerjakan). Yang tersisa berbeda hanya 3 controller
> ber-`period` di bawah — itu tugas T10.

24 controller sudah memakai `customPeriod` sejak sebelum pekerjaan ini: `allowance-types`, `benefit-types`,
`payroll-components`, `payroll-component-categories`, `payroll-periods`, `salary-grades`,
`salary-structures`, `professions`, `specializations`, `certification-types`,
`license-types`, `competencies`, `clinical-privilege-catalogs`,
`credentialing-requirements`, `mandatory-training-rules`, `training-catalogs`,
`training-categories`, `organization` (departments + positions), `workflow-definitions`,
`workflow-steps`, `approval-matrices`, `approval-delegation-policies`, `request-reasons`,
`rejection-reasons`, `workforce-requirements`.

3 lagi memakai `period`: `doctors`, `employees`, `external-users`.

### Definition of Done per controller

```
[ ] Action list menerima startDate, endDate, customPeriod (opsional semua)
[ ] Query lewat ApplyDateFilter pada kolom CreateDateTime
[ ] GET /filters/metadata mengembalikan customPeriods[] berisi 4 opsi
[ ] defaultFilter memuat startDate, endDate, customPeriod
[ ] GET tanpa parameter menghasilkan output yang sama persis dengan sebelumnya
[ ] GET /summary tidak ikut berubah
```

Setelah backend selesai per entitas, frontend hanya perlu memindahkan 3 key dari
`unsupportedFilterKeys` ke `supportedFilterKeys` di file constants entitas itu — tidak ada
perubahan lain, tidak perlu deploy ulang komponen.

---

## GAP-2 — Enam master data belum punya controller sama sekali — ✅ SELESAI

> **Selesai 2026-08-12** — tugas T9. Keenam master kini punya controller + DTO lengkap
> (9 action masing-masing), sesuai Definition of Done di bawah. Laporan teknis di
> `docs/hamzah/report/hr-master-data-six-new-controllers.md`, ringkasan siap-uji di
> `docs/hamzah/task-done/gap-2-controller-master-baru.md`.
>
> Route yang dipakai: `workforce-types`, `employee-categories`, `employment-types`,
> `employment-statuses`, `contract-types`, `on-call-types`.
>
> Tidak ada migration, tidak ada file lama yang diedit — 12 berkas baru seluruhnya.
> Rebuild penuh: **125 warning (sama dengan baseline), 0 error**.
> Commit `29d8eda`, sudah ada di `origin/MHamzah`.
>
> Uraian di bawah dipertahankan sebagai catatan masalah aslinya.

### Masalahnya

Keenam master di bawah **sudah punya Model, EF Configuration, `DbSet`, dan tabel di
database**, tetapi tidak ada Controller, DTO, maupun endpoint apa pun — termasuk
`/options`.

Akibatnya di frontend: field relasinya tidak bisa dibuat sebagai select, sehingga terpaksa
menjadi **input teks UUID manual**. Dan tidak ada cara sama sekali untuk menambah atau
mengubah datanya lewat UI.

### Daftar master

| Model | Lokasi model | Tabel DB | Sudah ada `DbSet` |
|---|---|---|---|
| `MstWorkforceType` | `Areas/.../MasterData/Workforce/Models/MstWorkforceType.cs` | `public.MstWorkforceType` | ✅ `MstWorkforceTypes` |
| `MstEmployeeCategory` | `Areas/.../MasterData/Workforce/Models/MstEmployeeCategory.cs` | `public.MstEmployeeCategory` | ✅ `MstEmployeeCategories` |
| `MstEmploymentType` | `Areas/.../MasterData/Workforce/Models/MstEmploymentType.cs` | `public.MstEmploymentType` | ✅ `MstEmploymentTypes` |
| `MstEmploymentStatus` | `Areas/.../MasterData/Workforce/Models/MstEmploymentStatus.cs` | `public.MstEmploymentStatus` | ✅ `MstEmploymentStatuses` |
| `MstContractType` | `Areas/.../MasterData/Workforce/Models/MstContractType.cs` | `public.MstContractType` | ✅ `MstContractTypes` |
| `MstOnCallType` | `Areas/.../MasterData/AttendanceAndSchedule/Models/MstOnCallType.cs` | `public.MstOnCallType` | ✅ `MstOnCallTypes` |

**Tidak perlu migration.** Keenam tabel sudah ada di
`Migrations/ApplicationDbContextModelSnapshot.cs`. Pekerjaannya murni menambah
Controller + DTO.

### Field per model (untuk menyusun DTO)

| Model | Field bisnis |
|---|---|
| `MstWorkforceType` | `WorkforceTypeCode`, `WorkforceTypeName`, `Description`, `IsInternal`, `IsClinical`, `SortOrder`, `IsActive` |
| `MstEmployeeCategory` | `WorkforceTypeId` (FK), `EmployeeCategoryCode`, `EmployeeCategoryName`, `Description`, `IsClinical`, `RequiresCredentialing`, `SortOrder`, `IsActive` |
| `MstEmploymentType` | `EmploymentTypeCode`, `EmploymentTypeName`, `Description`, `IsPermanent`, `IsContractBased`, `RequiresContractEndDate`, `IsPayrollEligible`, `IsBenefitEligible`, `SortOrder`, `IsActive` |
| `MstEmploymentStatus` | `EmploymentStatusCode`, `EmploymentStatusName`, `Description`, `IsActiveEmployment`, `IsSchedulable`, `IsPayrollEligible`, `IsTerminalStatus`, `SortOrder`, `IsActive` |
| `MstContractType` | `ContractTypeCode`, `ContractTypeName`, `Description`, `DefaultDurationMonths`, `IsRenewable`, `RequiresEndDate`, `IsProbationApplicable`, `SortOrder`, `IsActive` |
| `MstOnCallType` | `OnCallTypeCode`, `OnCallTypeName`, `Description`, `ResponseTimeMinutes`, `MinimumCallHours`, `MaximumCallHours`, `IsRemoteAllowed`, `RequiresOnSitePresence`, `CountsAsWorkingTime`, `IsAllowanceEligible`, `IsActive` |

### Siapa yang menunggu

| Field frontend | Dipakai di master data |
|---|---|
| `employeeCategoryId` | `benefit-eligibility-rule`, `benefit-plan`, `hazard-allowance-policy`, `mandatory-training-rule`, `on-call-allowance-policy`, `performance-template`, `salary-structure`, `shift-allowance-policy`, `leave-policy`, `overtime-policy`, `approval-matrix` |
| `employmentTypeId` | sama seperti di atas |
| `workforceTypeId` | `leave-policy` |
| `employmentStatusId` | `leave-policy` |
| `contractTypeId` | `leave-policy` |
| `onCallTypeId` | `on-call-allowance-policy`, `shift` |

Backend sendiri sudah memvalidasi FK ini di beberapa tempat
(`WfpDisciplinaryActionController.ActiveMasterExistsAsync<MstEmployeeCategory>`), jadi
datanya memang dipakai — hanya belum bisa dikelola.

### Definition of Done per master

Mengikuti pola `JobFamilyController` (paling sederhana) atau `LegalEntityController`:

```
[ ] GET  /filters/metadata      defaultFilter, sortOptions, sortDirections, customPeriods, pageSizeOptions
[ ] GET  /summary
[ ] GET  ""                     list + paging + search + isActive + startDate/endDate/customPeriod
[ ] GET  /options               proyeksi WAJIB mengisi Id + Code + Name (lihat GAP-3)
[ ] GET  /{id:guid}
[ ] POST ""                     kode auto-generate lewat GenerateCodeAsync
[ ] PUT  /{id:guid}
[ ] PATCH /{id:guid}/status     body { isActive: bool }
[ ] DELETE /{id:guid}
[ ] DTO memuat CreateBy + CreateByName, UpdateBy + UpdateByName (lihat GAP-4)
[ ] Tidak ada migration
```

Catatan kontrak yang harus diikuti supaya frontend tidak perlu perlakuan khusus:

- **Kode auto-generate**, bukan diinput user. Enam master Workflow yang memakai kode input
  user adalah pengecualian yang sudah terlanjur, jangan ditiru.
- **Ubah status pakai `PATCH /{id}/status`**, bukan `/activate` + `/deactivate`.
- `SortOrder` pada `PUT` sebaiknya **tidak ditimpa tanpa syarat** — pola
  `entity.SortOrder = request.SortOrder` membuat urutan tereset ke 0 setiap update karena
  field itu tidak ditampilkan di form. Pakai `request.SortOrder ?? entity.SortOrder` atau
  buat propertinya nullable.

Setelah endpoint tersedia, penyesuaian frontend hanya: daftarkan resource di
`src/lib/hooks/select/hr/hr-select-resources.js`, lalu ubah field terkait dari input teks
menjadi `type: "select"` + `optionResource`.

---

## GAP-3 — `GET /options` hanya mengembalikan `Id` — ✅ SELESAI

> **Selesai 2026-08-11** (tugas T1). Commit `8b9de69`, laporan di
> `docs/hamzah/report/options-projection-code-name.md`.
> Ketiga proyeksi kini mengisi seluruh field DTO, dan `BenefitPlanOptionResponse`
> sudah punya `BenefitPlanCode` + `BenefitPlanName`. Signature action, paging, urutan,
> dan bentuk response tidak berubah; tidak ada query tambahan karena `Include` yang
> diperlukan memang sudah ada di `BuildBaseQuery()`.
>
> Uraian di bawah dipertahankan sebagai catatan masalah aslinya.

### Masalahnya

Tiga endpoint options mengembalikan HTTP 200 dengan daftar item, tetapi proyeksinya hanya
mengisi `Id` dan membiarkan seluruh field lain pada nilai default:

```csharp
var response = new BenefitTypeOptionResponse();
response.Id = x.Id;      // ← hanya ini
return response;
```

Select di frontend jadi berisi opsi tanpa label sama sekali.

| Endpoint | File | Baris | Kondisi DTO |
|---|---|---|---|
| `GET /benefit-types/options` | `PayrollAndBenefit/Controllers/BenefitTypeController.cs` | 245-247 | DTO **sudah punya** `BenefitTypeCode` + `BenefitTypeName` — tinggal diisi |
| `GET /benefit-plans/options` | `PayrollAndBenefit/Controllers/BenefitPlanController.cs` | 250-252 | DTO **belum punya** `BenefitPlanCode`/`BenefitPlanName`, perlu ditambah dulu (`BenefitPlanDtos.cs:61-71`) |
| `GET /deduction-types/options` | `PayrollAndBenefit/Controllers/DeductionTypeController.cs` | 248-250 | DTO **sudah punya** `DeductionTypeCode` + `DeductionTypeName` (`DeductionTypeDtos.cs:53-63`) — tinggal diisi |

> `deduction-types/options` **belum pernah dilaporkan** di dokumen frontend mana pun.
> Ditemukan saat penyisiran 2026-08-11. Belum terasa karena frontend belum memakai
> resource itu — tapi akan langsung menggigit begitu ada form yang butuh select jenis potongan.

### Kondisi frontend saat ini

`benefitTypes` dan `benefitPlans` di-workaround ke **endpoint list** (`GET /benefit-types`,
`GET /benefit-plans`) yang proyeksinya benar. Konsekuensinya endpoint itu memakai
parameter `isActive`, bukan `onlyActive` seperti options.

Setelah GAP-3 diperbaiki, frontend bisa dikembalikan ke `/options` supaya konsisten dengan
22 resource lain — tapi itu perubahan frontend, bukan syarat.

### Definition of Done

```
[ ] Proyeksi /options mengisi Id + Code + Name untuk ketiga endpoint
[ ] BenefitPlanOptionResponse ditambahi BenefitPlanCode + BenefitPlanName
[ ] Field Label (kalau ada) tetap "Code - Name" mengikuti pola WfpCredentialLicenseController.cs:64-76
```

Catatan: frontend **menampilkan `Name` saja** dan memakai `Code` untuk pencarian. Jadi
`Code` dan `Name` harus dikirim sebagai **field terpisah**, jangan hanya digabung di `Label`.

---

## GAP-4 — `CreateByName` / `UpdateByName` tidak ada di 8 DTO area Organization — ✅ SELESAI

> **Selesai 2026-08-12** — tugas T2. Kedelapan DTO Organization kini mengirim
> `createByName` di list dan `createByName` + `updateByName` di detail, diisi lewat
> pemetaan **batch** (satu query ke `Users` per request), bukan subquery per baris.
> `PerformanceTemplateDetailDtos.cs` yang disebut "plus, prioritas rendah" di bawah
> **ikut dikerjakan**, jadi GAP-4 tertutup penuh — total 9 DTO + 9 controller.
>
> Laporan teknis di `docs/hamzah/report/organization-create-by-name.md`, ringkasan siap-uji
> di `docs/hamzah/task-done/gap-4-create-by-name.md`.
>
> Tidak ada migration. Tidak ada perubahan frontend yang dibutuhkan.
>
> Uraian di bawah dipertahankan sebagai catatan masalah aslinya.

### Masalahnya

Delapan DTO di `Areas/Corporate/HumanResource/MasterData/Organization/DTOs/` hanya punya
`Guid? CreateBy` tanpa `string? CreateByName`.

Frontend punya aturan keras: **UUID dilarang tampil di UI**. Karena tidak ada field nama,
kolom **Dibuat Oleh** di tabel dan baris audit di halaman detail menampilkan `-`. Itu
perilaku yang benar dan sudah dipilih sadar — Guid **tidak** akan ditampilkan sebagai
gantinya.

| DTO | Master data |
|---|---|
| `CostCenterDtos.cs` | Pusat Biaya |
| `EmployeeGradeDtos.cs` | Grade Karyawan |
| `HospitalSiteDtos.cs` | Fasilitas Rumah Sakit |
| `JobFamilyDtos.cs` | Rumpun Jabatan |
| `JobLevelDtos.cs` | Level Jabatan |
| `LegalEntityDtos.cs` | Legal Entity |
| `OrganizationUnitDtos.cs` | Unit Organisasi |
| `WorkLocationDtos.cs` | Lokasi Kerja |

Plus `Performance/DTOs/PerformanceTemplateDetailDtos.cs` — belum punya halaman frontend,
prioritas rendah.

**Gap ini spesifik area Organization.** Area HR lain (Payroll, Performance, Credentialing,
Training, Scheduling, Workflow, Leave) sudah mengirim `CreateByName`.
`OrganizationDtos.cs` di area yang sama juga sudah benar — bisa dipakai sebagai pembanding
terdekat.

### Pola acuan

`Areas/Administrator/MasterData/DTOs/BankDtos.cs:33-34` (`CreateBy` + `CreateByName`),
diisi lewat `GetActorNameMapAsync` / `GetActorName` di controller-nya.

### Definition of Done

```
[ ] 8 DTO list + detail menambah string? CreateByName
[ ] DTO detail menambah string? UpdateByName
[ ] Controller mengisi lewat GetActorNameMapAsync (batch, jangan query per baris)
[ ] Nilai null tetap dikirim sebagai null — frontend menampilkan "-", itu benar
```

Frontend **sudah siap** membaca field ini (`dataKeys: ["createByName", "CreateByName",
"createdByName", "CreatedByName"]`). Begitu backend mengirimnya, kolom terisi otomatis
tanpa perubahan frontend apa pun.

---

## Yang sengaja TIDAK diminta berubah

Supaya tidak ada pekerjaan yang mubazir:

| Hal | Status | Alasan |
|---|---|---|
| Endpoint master data yang sudah ada | ✅ Lengkap | Seluruh 63 modul master data HR frontend punya route backend yang cocok. Tidak ada endpoint yang hilang |
| `Label = Code + " - " + Name` di `/options` | ✅ Biarkan | Frontend memilih sendiri menampilkan `Name` saja. `Code` dipakai untuk pencarian. Sudah diselesaikan di frontend |
| `timeZoneId` di `hospital-site` & `work-calendar` | ✅ Biarkan | Bukan foreign key — string IANA dengan default `"Asia/Jakarta"`. Tidak perlu endpoint options |
| `violationCategory`, `severityLevel` di `violationtypes` | ✅ Biarkan | Memang string bebas tanpa enum. Frontend memakai input teks, bukan select |
| Filter klasifikasi domain yang sudah ada (`isDefault`, `legalEntityId`, dst.) | ✅ Biarkan | Frontend sengaja tidak merendernya demi baris filter yang seragam. Query param-nya tetap berguna |
| Semantik delete berbeda antar grup | ✅ Biarkan | Grup Cuti & Lembur menerima body alasan, grup lain tidak. Frontend sudah menyesuaikan per master |

---

## Usulan pemecahan tugas

Diurutkan dari rasio manfaat/usaha tertinggi:

| # | Tugas | Scope | Status | Ketergantungan |
|---|---|---|---|---|
| T1 | GAP-3 — perbaiki proyeksi `/options` | 3 controller + 1 DTO | ✅ **Selesai** — commit `8b9de69` | — |
| T2 | GAP-4 — `CreateByName`/`UpdateByName` area Organization | 8 DTO + 8 controller (+1 Performance) | ✅ **Selesai** 2026-08-12 | — |
| T3 | GAP-1a — filter tanggal area Organization | 8 controller | ✅ **Selesai** 2026-08-11 | — |
| T4 | GAP-1b — filter tanggal area PayrollAndBenefit | 6 controller | ✅ **Selesai** 2026-08-11 | — |
| T5 | GAP-1c — filter tanggal area LeaveAndOvertime | 7 controller | ✅ **Selesai** 2026-08-11 | — |
| T6 | GAP-1d — filter tanggal area AttendanceAndSchedule | 5 controller | ✅ **Selesai** 2026-08-11 | — |
| T7 | GAP-1e — filter tanggal area Performance | 5 controller | ✅ **Selesai** 2026-08-11 | — |
| T8 | GAP-1f — filter tanggal area EmployeeRelation | 4 controller | ✅ **Selesai** 2026-08-11 | — |
| T9 | GAP-2 — controller baru 6 master | 6 controller + 6 DTO baru | ✅ **Selesai** 2026-08-12 | T1 sudah selesai, jadi pola `/options` yang benar sudah tersedia sebagai acuan |
| T10 | Alias `customPeriod` untuk `doctors`, `employees`, `external-users` | 3 controller | ⬜ | Prioritas rendah, tidak merusak apa pun |
| T11 | Pindahkan 4 controller EmployeeRelation dari `Repositories/Configurations/` ke `Areas/` | 4 file + namespace | ⬜ | Prioritas rendah, **jangan** dicampur T8 |

T3–T8 akhirnya dikerjakan sekaligus dalam satu pekerjaan, bukan dipecah per area: titik
sentuhnya identik di keenam area, sehingga memecahnya justru menambah risiko dua area
memakai semantik yang berbeda.

Rencana awal menggabung T3 dengan T2 karena menyentuh delapan controller Organization yang
sama. Itu tidak jadi dilakukan — T3 sudah selesai lewat pekerjaan GAP-1 menyeluruh. **T2
tetap perlu dikerjakan**, dan sekarang berdiri sendiri: 8 DTO + 8 controller Organization,
menambah `CreateByName`/`UpdateByName`.

---

## Pekerjaan yang sudah jalan

| Tugas | Commit | Laporan | Sudah di `origin/MHamzah` |
|---|---|---|---|
| GAP-1 pilot — `benefit-type` filter tanggal | `8ece36a` | `report/benefit-type-date-filter.md` | ✅ Ya |
| T1 / GAP-3 — proyeksi `/options` | `8b9de69` | `report/options-projection-code-name.md` | ✅ Ya |
| T3–T8 / GAP-1 — filter tanggal 35 controller | sudah masuk riwayat | `report/hr-master-data-date-filter.md` | ✅ Ya |
| T9 / GAP-2 — controller + DTO 6 master baru | `29d8eda` | `report/hr-master-data-six-new-controllers.md` | ✅ Ya |
| T2 / GAP-4 — `CreateByName`/`UpdateByName` | ⬜ **Belum di-commit** | `report/organization-create-by-name.md` | ⬜ Belum |

> Diperiksa ulang 2026-08-12: `HEAD` = `origin/MHamzah` = `29d8eda`. Seluruh pekerjaan
> T1, T3–T8, dan T9/GAP-2 sudah ada di remote berikut laporan dan berkas `task-done`-nya.
> Yang belum masuk hanya berkas GAP-4.

**Keempat GAP sudah tertutup.** Yang tersisa hanya T10 (alias `customPeriod` untuk
`doctors`, `employees`, `external-users`) dan T11 (pindahkan 4 controller EmployeeRelation
dari `Repositories/Configurations/` ke `Areas/`) — keduanya prioritas rendah dan tidak
merusak apa pun kalau dibiarkan.

Hambatan SDK yang sempat dicatat di sini **sudah hilang**: .NET SDK 9.0.316 sesuai
`global.json` sudah terpasang pada 2026-08-11, sehingga `dotnet build` bisa dijalankan dan
seluruh pekerjaan di atas terverifikasi.

Catatan build yang perlu diingat saat mengerjakan T2 dan seterusnya: `dotnet build` polos
akan **melewati kompilasi** kalau tidak ada file yang berubah sejak build/`dotnet run`
terakhir — hasilnya selesai dalam hitungan detik dan melaporkan 0 Warning yang menyesatkan.
Untuk mengukur warning dengan benar, pakai `dotnet build QuilvianSystemBackend.csproj
--no-incremental`. Baseline repo ini saat rebuild penuh: **125 warning, 0 error**.

Riwayat commit memuat `5bcbcb2` dengan judul cacat (`@ feat: ...`) — isinya identik dengan
`8ece36a` yang menggantikannya lewat amend, dan sudah terlanjur ter-push. Dibiarkan saja
karena membersihkannya menuntut force push yang dilarang RULE 2.

---

## Status verifikasi

| Pemeriksaan | Hasil |
|---|---|
| Penyisiran controller HR master data (filter tanggal) | Dijalankan 2026-08-11 — 35 controller tanpa `startDate`, 27 sudah mendukung. **Ke-35 sudah dikerjakan**, lihat `report/hr-master-data-date-filter.md` |
| Build setelah GAP-1 | Dijalankan 2026-08-11 — rebuild penuh **125 warning, 0 error**, tidak ada warning baru dari berkas yang disentuh |
| Penyisiran DTO HR master data (`CreateByName`) | Dijalankan 2026-08-11 — 9 DTO tanpa `CreateByName` (8 Organization + 1 Performance detail) |
| Penyisiran proyeksi `/options` | Dijalankan 2026-08-11 — 3 endpoint hanya mengisi `Id`. **Sudah diperbaiki**, lihat GAP-3 |
| Pencocokan endpoint frontend ↔ route backend | Dijalankan 2026-08-11 — 47 endpoint frontend, semuanya punya route backend yang cocok |
| Keberadaan tabel 6 master GAP-2 | Diverifikasi di `ApplicationDbContextModelSnapshot.cs` — keenam tabel ada. **Controller + DTO-nya sudah dibuat**, lihat `report/hr-master-data-six-new-controllers.md` |
| Build setelah GAP-2 | Dijalankan 2026-08-12 — rebuild penuh **125 warning, 0 error**; 12 berkas baru tidak menyumbang warning sama sekali |
| Uji Swagger 6 endpoint baru GAP-2 | **Belum dijalankan** — langkahnya ada di `task-done/gap-2-controller-master-baru.md` |
| Build setelah GAP-4 | Dijalankan 2026-08-12 — rebuild penuh **125 warning, 0 error**; 18 berkas yang disentuh tidak menyumbang warning |
| Uji Swagger 9 endpoint GAP-4 | **Belum dijalankan** — langkahnya ada di `task-done/gap-4-create-by-name.md`, termasuk cara memastikan pemetaan nama benar-benar batch |
| `dotnet build` | **Sukses** — dijalankan 2026-08-11 setelah SDK 9.0.316 terpasang. Rebuild penuh: 125 warning bawaan, 0 error |

Dokumen ini diperbarui setiap ada tugas yang selesai — kolom **Status** pada tabel Ringkasan
dan tabel Usulan pemecahan tugas adalah sumber rujukan posisi terkini.
