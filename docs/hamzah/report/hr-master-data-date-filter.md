# HR Master Data — Filter rentang tanggal untuk 35 controller (GAP-1 tuntas)

| | |
|---|---|
| Tanggal | 2026-08-11 |
| Branch | `MHamzah` |
| Pemicu | GAP-1 pada register `docs/hamzah/task/hr-master-data-frontend-gaps.md` (tugas T3–T8) |
| Migration | **Tidak ada** |
| Breaking change | **Tidak** — seluruh parameter opsional, `GET` tanpa parameter tidak berubah |

Menutup GAP-1 sepenuhnya. Pilot `benefit-type` sudah selesai lebih dulu (commit `8ece36a`);
pekerjaan ini menyelesaikan **35 controller sisanya** sekaligus, yaitu tugas T3 sampai T8.

## Kenapa diubah

Kontrak baku master data frontend mewajibkan tiga kontrol filter selalu tampil di setiap
halaman list: **Tanggal Mulai**, **Tanggal Akhir**, dan **Periode**. Untuk 35 controller ini
ketiga query param tersebut tidak diterima sama sekali, sehingga frontend menahan nilainya
lewat `unsupportedFilterKeys` — kontrolnya terlihat aktif tapi tidak mengubah hasil.

Tidak bisa diselesaikan di frontend: penyaringan harus terjadi sebelum paging dan
penghitungan `totalData`. Menyaring di sisi klien setelah data terpaging akan menghasilkan
jumlah halaman dan total yang salah.

27 controller lain sudah mendukungnya sejak awal, jadi ini **meratakan kontrak yang sudah
ada**, bukan menambah fitur baru.

## Kontrak parameter

| Parameter | Tipe | Perilaku |
|---|---|---|
| `startDate` | `DateTime?` | Batas awal inklusif, dinormalkan ke awal hari UTC |
| `endDate` | `DateTime?` | Batas akhir; di belakang layar eksklusif (`endDate` + 1 hari) supaya tanggal yang diminta ikut terhitung |
| `customPeriod` | `string?` | `today`, `last7days`, `thismonth`, `lastmonth`. **Diabaikan** kalau `startDate` atau `endDate` terisi |

- Kolom yang difilter: **`CreateDateTime`**.
- Ketiganya kosong → query **tidak berubah sama sekali**. Pemanggil lama aman.
- `customPeriod` yang tidak dikenali diperlakukan sama dengan kosong (tidak menyaring),
  bukan error.

## Endpoint yang terpengaruh

| Endpoint | Perubahan |
|---|---|
| `GET ""` (list) tiap entitas | Menerima `startDate`, `endDate`, `customPeriod`; disaring pada `CreateDateTime` sebelum `CountAsync` dan paging |
| `GET /filters/metadata` tiap entitas | `customPeriods[]` berisi 4 opsi; `defaultFilter` memuat `startDate`, `endDate`, `customPeriod` (semuanya `null`) |
| `GET /summary` | **Sengaja TIDAK diubah** — kartu ringkasan tetap menampilkan total keseluruhan, mengikuti `AllowanceTypeController` |
| `GET /options` | **Sengaja TIDAK diubah** — select relasi tidak memakai filter tanggal |
| `GET /{id:guid}`, `POST`, `PUT`, `PATCH`, `DELETE` | Tidak tersentuh |

## Pola yang dipakai

Register menyebut helper generik `WorkflowMasterDataSupport.ApplyDateFilter<T>()` sebagai
pilihan yang lebih disarankan, dan itu yang dipakai di sini:

```csharp
query = WorkflowMasterDataSupport.ApplyDateFilter(query, startDate, endDate, customPeriod);
```

Alasannya satu implementasi untuk 35 controller, bukan 35 salinan. Pilot `benefit-type`
menyalin helper `ResolveDateRange` + `ApplyDateFilter` ke dalam controller-nya sendiri;
menirunya 35 kali berarti menduplikasi ±40 baris logika tanggal yang identik di 35 berkas.
Semantiknya persis sama — `WorkflowMasterDataSupport.ResolveDateRange` dan versi salinan di
`BenefitTypeController` berperilaku identik. Helper itu bersyarat
`where T : IdentityModel`, dan seluruh 35 model master data ini turunan `IdentityModel`.

`BuildPeriodOptions()` tetap per-controller karena tipe DTO opsinya berbeda per entitas.

## File yang disentuh

**Controller — 35 berkas**, masing-masing empat sisipan: `using` helper, `CustomPeriods` di
metadata, tiga parameter `[FromQuery]` di action list, dan satu baris `ApplyDateFilter`.

| Area | Controller |
|---|---|
| Organization (8) | `CostCenter`, `EmployeeGrade`, `HospitalSite`, `JobFamily`, `JobLevel`, `LegalEntity`, `OrganizationUnit`, `WorkLocation` |
| PayrollAndBenefit (6) | `BenefitEligibilityRule`, `BenefitPlan`, `DeductionType`, `HazardAllowancePolicy`, `OnCallAllowancePolicy`, `ShiftAllowancePolicy` |
| LeaveAndOvertime (7) | `LeaveAdjustmentReason`, `LeaveCarryForwardPolicy`, `LeaveEntitlementPolicy`, `LeavePolicy`, `LeaveType`, `OvertimePolicy`, `OvertimeRate` |
| AttendanceAndSchedule (5) | `Shift`, `ShiftGroup`, `ShiftPattern`, `WorkCalendar`, `WorkSchedule` |
| Performance (5) | `KpiCatalog`, `PerformanceCycle`, `PerformanceRatingScale`, `PerformanceTemplate`, `PerformanceTemplateDetail` |
| EmployeeRelation (4) | `DisciplinaryActionType`, `EmployeeRelationCaseType`, `SanctionType`, `ViolationType` |

**DTO — 35 berkas**: `<Entitas>DefaultFilterResponse` ditambahi `StartDate`, `EndDate`,
`CustomPeriod`; `<Entitas>FilterMetadataResponse` ditambahi `CustomPeriods`.

**Berkas dengan penanganan khusus**

| Path | Kenapa berbeda |
|---|---|
| `LeaveAndOvertime/Services/LeaveAdjustmentReasonService.cs` | Satu-satunya entitas yang logika query-nya ada di service, bukan controller. `GetDataAsync` menerima tiga parameter baru, `GetFilterMetadata` mengisi `CustomPeriods`, dan `BuildPeriodOptions` ditaruh di service |
| `Performance/DTOs/PerformanceCycleDtos.cs` | Menampung `PerformanceCustomPeriodOptionResponse` yang dipakai bersama 5 controller Performance — mengikuti `PerformanceStringOptionResponse` dan `PerformanceSortOptionResponse` yang sudah lebih dulu dipakai bersama di area itu |
| `EmployeeRelation/DTOs/EmployeeRelationMasterCommonDtos.cs` | Menampung `EmployeeRelationCustomPeriodOptionResponse` bersama untuk 4 controller — mengikuti `EmployeeRelationSortOptionResponse` |

Empat area lainnya memakai tipe opsi periode per-entitas
(`<Entitas>CustomPeriodOptionResponse`), mengikuti `OrganizationCustomPeriodOptionResponse`
dan pilot `BenefitTypeCustomPeriodOptionResponse`.

## Yang sengaja tidak dikerjakan

| Hal | Alasan |
|---|---|
| Alias `customPeriod` untuk `doctors`, `employees`, `external-users` | Tugas T10 terpisah. Ketiganya sudah punya filter tanggal, hanya nama parameternya `period`. Frontend sudah menyesuaikan, jadi tidak rusak |
| Memindahkan 4 controller EmployeeRelation ke `Areas/` | Tugas T11 terpisah. Register meminta eksplisit agar **tidak** dicampur dengan pekerjaan filter tanggal |
| Menyeragamkan gaya format berkas AttendanceAndSchedule | Lima berkas di area itu memakai gaya terkompresi hasil formatter. Sisipan mengikuti gaya setempat agar diff tetap kecil dan mudah dibaca |

## Dampak ke frontend

Tidak ada yang wajib. Halaman tetap jalan seperti sekarang.

Yang bisa dilakukan supaya filternya benar-benar berfungsi: pindahkan `startDate`,
`endDate`, dan `customPeriod` dari `unsupportedFilterKeys` ke `supportedFilterKeys` di
berkas constants tiap entitas. Tidak ada perubahan komponen.

## Cara menguji

```http
### Perilaku lama harus sama persis — bandingkan dengan sebelum perubahan
GET /api/v1/corporate/human-resource/master-data/job-families?pageNumber=1&pageSize=25

### Rentang eksplisit — data tanggal 2026-08-11 harus ikut terhitung
GET /api/v1/corporate/human-resource/master-data/job-families?startDate=2026-08-01&endDate=2026-08-11

### Periode preset
GET /api/v1/corporate/human-resource/master-data/job-families?customPeriod=thismonth

### customPeriod diabaikan kalau ada startDate — hasilnya sama dengan rentang eksplisit
GET /api/v1/corporate/human-resource/master-data/job-families?startDate=2026-08-01&customPeriod=today

### Metadata memuat 4 opsi periode
GET /api/v1/corporate/human-resource/master-data/job-families/filters/metadata

### Ringkasan TIDAK ikut menyusut walau list menyusut
GET /api/v1/corporate/human-resource/master-data/job-families/summary
```

Entitas dengan route dan bentuk parameter khusus yang layak diuji terpisah:

- `GET /performance-templates/{performanceTemplateId}/details?customPeriod=today` — parameter
  rute tetap di posisi pertama.
- `GET /leave-adjustment-reasons?startDate=...` — satu-satunya yang lewat service.

## Status verifikasi

| Pemeriksaan | Hasil |
|---|---|
| `dotnet build QuilvianSystemBackend.csproj --no-incremental` | **125 Warning, 0 Error** — persis baseline repo |
| Warning baru dari berkas yang disentuh | **Tidak ada.** Warning yang muncul dari berkas tersebut (CS8619 di `GetActorNameMapAsync`, CS0108, CS0162) sudah ada sebelum perubahan |
| Penempatan `ApplyDateFilter` | Diperiksa skrip: tepat 1× per controller, seluruhnya di dalam action `[HttpGet]` polos — tidak ada yang nyasar ke `/options` atau `/summary` |
| Kelengkapan DTO | Diperiksa skrip: 35/35 DTO memuat `StartDate`, `EndDate`, `CustomPeriod`, dan `CustomPeriods` |
| Sifat aditif perubahan | Diperiksa `git diff --numstat`: penghapusan hanya pada baris signature/metadata yang digantikan versi lebih panjang. Tidak ada logika yang dibuang |
| Uji endpoint lewat Swagger | **Belum dijalankan** |
| Penyesuaian frontend | **Belum dijalankan** — tidak wajib, lihat bagian Dampak ke frontend |

> Catatan build: rebuild penuh pertama gagal di tahap **penyalinan berkas** (MSB3021/MSB3027)
> karena `QuilvianSystemBackend.exe` sedang dikunci proses aplikasi yang berjalan (PID 3028) —
> bukan kegagalan kompilasi, dan tidak ada satu pun error `CS`. Build diulang dengan output ke
> direktori terpisah (`-o`) dan lolos bersih: 125 warning, 0 error.
