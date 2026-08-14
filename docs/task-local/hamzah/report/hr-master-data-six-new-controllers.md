# HR Master Data — Enam Master Baru Mendapat Controller + DTO (GAP-2)

| | |
|---|---|
| Tanggal | 2026-08-12 |
| Branch | `MHamzah` |
| Pemicu | GAP-2 / tugas T9 di `docs/hamzah/task/hr-master-data-frontend-gaps.md` |
| Migration | **Tidak ada** — keenam tabel sudah ada di `ApplicationDbContextModelSnapshot.cs` |
| Breaking change | **Tidak** — seluruhnya endpoint baru, tidak ada kontrak lama yang disentuh |

## Kenapa diubah

Enam master di bawah sudah punya Model, EF Configuration, `DbSet`, dan tabel di database,
tetapi **tidak punya Controller, DTO, maupun endpoint apa pun** — termasuk `/options`.

Akibatnya di frontend: field relasi yang menunjuk ke keenam master ini tidak bisa dirender
sebagai select, sehingga terpaksa menjadi **input teks UUID manual**. Dan tidak ada cara
sama sekali menambah atau mengubah datanya lewat UI.

Ini tidak bisa diselesaikan di frontend. Tidak ada endpoint untuk dipanggil — bukan soal
bentuk data yang perlu diadaptasi, melainkan data yang memang tidak terjangkau HTTP.
Backend sendiri sudah memvalidasi FK ini di tempat lain
(`WfpDisciplinaryActionController.ActiveMasterExistsAsync<MstEmployeeCategory>`), jadi
datanya memang dipakai — hanya belum bisa dikelola.

## Endpoint yang terpengaruh

Tidak ada endpoint lama yang berubah. Enam controller **baru**, masing-masing sembilan
action dengan bentuk yang identik:

| Route dasar | Controller | Tag Swagger |
|---|---|---|
| `api/v1/corporate/human-resource/master-data/workforce-types` | `WorkforceTypeController.cs` | … / Master Data / Workforce Type |
| `api/v1/corporate/human-resource/master-data/employee-categories` | `EmployeeCategoryController.cs` | … / Master Data / Employee Category |
| `api/v1/corporate/human-resource/master-data/employment-types` | `EmploymentTypeController.cs` | … / Master Data / Employment Type |
| `api/v1/corporate/human-resource/master-data/employment-statuses` | `EmploymentStatusController.cs` | … / Master Data / Employment Status |
| `api/v1/corporate/human-resource/master-data/contract-types` | `ContractTypeController.cs` | … / Master Data / Contract Type |
| `api/v1/corporate/human-resource/master-data/on-call-types` | `OnCallTypeController.cs` | … / Master Data / On Call Type |

Action per controller — sama persis di keenamnya:

| Action | Keterangan |
|---|---|
| `GET /filters/metadata` | `defaultFilter`, `customPeriods`, `sortOptions`, `sortDirections`, `pageSizeOptions` |
| `GET /summary` | Total / aktif / nonaktif + dua hitungan khas tiap master. **Tidak** ikut difilter tanggal |
| `GET ""` | List + paging + search + filter klasifikasi + `startDate`/`endDate`/`customPeriod` |
| `GET /options` | Proyeksi mengisi `Id` + `Code` + `Name` sebagai field terpisah (kontrak GAP-3) |
| `GET /{id:guid}` | Detail, memuat `UpdateBy` + `UpdateByName` |
| `POST ""` | Kode **auto-generate** lewat `GenerateCodeAsync` |
| `PUT /{id:guid}` | Ubah data |
| `PATCH /{id:guid}/status` | Body `{ isActive: bool }` — bukan `/activate` + `/deactivate` |
| `DELETE /{id:guid}` | Soft delete, tanpa body alasan (mengikuti grup non-Cuti) |

## Kontrak parameter / field

### Filter tanggal — seragam dengan 59 controller lain

Memakai `WorkflowMasterDataSupport.ApplyDateFilter<T>()`, bukan helper baru.

| Parameter | Tipe | Perilaku |
|---|---|---|
| `startDate` | `DateTime?` | Batas awal inklusif, dinormalkan ke awal hari UTC |
| `endDate` | `DateTime?` | Batas akhir; di belakang layar eksklusif (`endDate` + 1 hari) |
| `customPeriod` | `string?` | `today`, `last7days`, `thismonth`, `lastmonth`. Diabaikan kalau `startDate`/`endDate` terisi |

Kolom yang difilter: **`CreateDateTime`**. Ketiganya kosong → query tidak berubah.

### Filter klasifikasi per master

| Master | Filter khusus |
|---|---|
| Workforce Type | `isInternal`, `isClinical` |
| Employee Category | `workforceTypeId`, `isClinical`, `requiresCredentialing` |
| Employment Type | `isPermanent`, `isContractBased` |
| Employment Status | `isActiveEmployment`, `isTerminalStatus` |
| Contract Type | `isRenewable`, `isProbationApplicable` |
| On Call Type | `isRemoteAllowed`, `isAllowanceEligible` |

Semuanya opsional; `isActive` dan `search` tersedia di keenamnya.

### `SortOrder` — sengaja nullable di request

Lima master punya `SortOrder` (On Call Type tidak). Field ini **tidak dirender frontend**,
jadi pola `entity.SortOrder = request.SortOrder` akan mereset urutan ke 0 setiap update.

Maka di `Create<X>Request` tipenya `int?`, dan:

- `POST` → `SortOrder = request.SortOrder ?? 0`
- `PUT` → `entity.SortOrder = request.SortOrder ?? entity.SortOrder`

Artinya: **payload yang tidak menyertakan `sortOrder` tidak mengubah urutan yang sudah ada.**

### Audit trail

Setiap DTO list memuat `CreateBy` + `CreateByName`; DTO detail menambah `UpdateBy` +
`UpdateByName`. Diisi lewat `GetActorNameMapAsync` (batch satu query, bukan per baris),
mengikuti `LeaveTypeController`. Nilai `null` tetap dikirim sebagai `null`.

### Validasi konsistensi yang ditambahkan

Bukan bagian DoD, tapi murah dan mencegah data yang saling bertentangan:

| Master | Aturan |
|---|---|
| Employee Category | `workforceTypeId` (kalau diisi) harus menunjuk workforce type yang aktif dan belum dihapus |
| Employment Type | Tidak boleh `isPermanent` **dan** `isContractBased` sekaligus; `requiresContractEndDate` hanya untuk yang berbasis kontrak |
| Employment Status | `isTerminalStatus` tidak boleh berbarengan dengan `isActiveEmployment` |
| Contract Type | `defaultDurationMonths` harus > 0 kalau diisi |
| On Call Type | `maximumCallHours` ≥ `minimumCallHours`; tidak boleh `isRemoteAllowed` **dan** `requiresOnSitePresence` sekaligus |

Keenamnya juga menolak nama duplikat (case-insensitive, mengabaikan baris terhapus).

### Penjagaan delete

Hanya di dua master yang punya navigasi balik di model:

| Master | Ditolak kalau |
|---|---|
| Workforce Type | Masih dipakai employee category yang belum dihapus |
| On Call Type | Masih dipakai shift yang belum dihapus |

Empat master lain tidak punya koleksi navigasi, jadi tidak ada penjagaan yang bisa dilakukan
tanpa menebak-nebak tabel pemakainya. Dibiarkan apa adanya — soft delete tetap aman karena
FK yang menunjuk ke sana bersifat nullable.

## File yang disentuh

Seluruhnya **file baru** — tidak ada file lama yang diedit.

| Path | Isi |
|---|---|
| `Areas/.../Workforce/DTOs/WorkforceTypeDtos.cs` | 12 DTO workforce type |
| `Areas/.../Workforce/Controllers/WorkforceTypeController.cs` | 9 action |
| `Areas/.../Workforce/DTOs/EmployeeCategoryDtos.cs` | 12 DTO employee category |
| `Areas/.../Workforce/Controllers/EmployeeCategoryController.cs` | 9 action, termasuk FK ke workforce type |
| `Areas/.../Workforce/DTOs/EmploymentTypeDtos.cs` | 12 DTO employment type |
| `Areas/.../Workforce/Controllers/EmploymentTypeController.cs` | 9 action |
| `Areas/.../Workforce/DTOs/EmploymentStatusDtos.cs` | 12 DTO employment status |
| `Areas/.../Workforce/Controllers/EmploymentStatusController.cs` | 9 action |
| `Areas/.../Workforce/DTOs/ContractTypeDtos.cs` | 12 DTO contract type |
| `Areas/.../Workforce/Controllers/ContractTypeController.cs` | 9 action |
| `Areas/.../AttendanceAndSchedule/DTOs/OnCallTypeDtos.cs` | 12 DTO on call type |
| `Areas/.../AttendanceAndSchedule/Controllers/OnCallTypeController.cs` | 9 action |

Prefix kode dan `SortOrder` menu — dipilih dari nilai yang belum terpakai di modul
`HUMAN_RESOURCE_MASTER_DATA`:

| Master | Prefix kode | `AccessController.SortOrder` |
|---|---|---|
| Workforce Type | `WFT-RSMMC-` | 76 |
| Employee Category | `ECT-RSMMC-` | 77 |
| Employment Type | `EMT-RSMMC-` | 78 |
| Employment Status | `EMS-RSMMC-` | 79 |
| Contract Type | `CTT-RSMMC-` | 80 |
| On Call Type | `OCT-RSMMC-` | 81 |

## Catatan penyimpangan kecil dari pola acuan

`GetActorNameMapAsync` di `LeaveTypeController` memancing warning `CS8619` (nullability
`Dictionary<Guid, string>` vs `Dictionary<Guid, string?>`) — warning itu sudah ada di ±20
controller lain. Menyalinnya apa adanya akan menambah **6 warning baru**.

Di keenam controller baru, proyeksinya diberi cast eksplisit:

```csharp
.Select(x => new { x.Id, Name = (string?)(x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode) })
```

Semantik dan SQL yang dihasilkan identik — cast ke nullable reference type tidak
menghasilkan node ekspresi baru. Yang berubah hanya anotasi nullability di sisi C#.
Controller lama **tidak** ikut disentuh; membersihkan warning bawaan itu tugas terpisah.

## Dampak ke frontend

Tidak ada yang wajib sekarang — belum ada halaman frontend untuk keenam master ini, dan
tidak ada kontrak lama yang berubah.

Yang **bisa** dikerjakan setelah ini, sesuai catatan di register:

1. Daftarkan keenam resource di `src/lib/hooks/select/hr/hr-select-resources.js`.
2. Ubah field berikut dari input teks UUID menjadi `type: "select"` + `optionResource`:

| Field frontend | Master data yang memakainya |
|---|---|
| `employeeCategoryId` | `benefit-eligibility-rule`, `benefit-plan`, `hazard-allowance-policy`, `mandatory-training-rule`, `on-call-allowance-policy`, `performance-template`, `salary-structure`, `shift-allowance-policy`, `leave-policy`, `overtime-policy`, `approval-matrix` |
| `employmentTypeId` | sama seperti di atas |
| `workforceTypeId` | `leave-policy` |
| `employmentStatusId` | `leave-policy` |
| `contractTypeId` | `leave-policy` |
| `onCallTypeId` | `on-call-allowance-policy`, `shift` |

Halaman CRUD untuk keenam master itu sendiri juga sekarang mungkin dibuat — sebelumnya
tidak.

## Cara menguji

Langkah lengkap per master ada di `docs/hamzah/task-done/gap-2-controller-master-baru.md`.
Ringkasnya:

```http
POST /api/v1/corporate/human-resource/master-data/workforce-types
{ "workforceTypeName": "Tenaga Medis", "isInternal": true, "isClinical": true }
→ 200, workforceTypeCode terisi otomatis "WFT-RSMMC-00001"
```

```http
GET /api/v1/corporate/human-resource/master-data/workforce-types/options?onlyActive=true
→ setiap item punya id + workforceTypeCode + workforceTypeName yang TERISI
```

Kasus "perilaku lama harus tetap sama" tidak berlaku di sini — keenam route sebelumnya
menghasilkan **404**, jadi tidak ada pemanggil lama yang bisa terganggu.

Yang perlu dipastikan tidak reset:

```http
PUT /…/workforce-types/{id}   body TANPA "sortOrder"
→ sortOrder entity tetap seperti sebelumnya, bukan 0
```

## Status verifikasi

| Pemeriksaan | Hasil |
|---|---|
| `dotnet build QuilvianSystemBackend.csproj --no-incremental` | **Sukses — 125 Warning, 0 Error** |
| Warning baru dari 12 berkas yang ditambahkan | **Tidak ada** — hitungan warning sama persis dengan baseline repo (125) |
| Enam route terdaftar di atribut `[Route]` | Terverifikasi lewat penyisiran `Areas/` |
| Migration | Tidak dibuat, tidak dibutuhkan |
| **Uji endpoint lewat Swagger** | **Belum dijalankan** |
| **Penyesuaian frontend** | **Belum dijalankan** — tidak wajib |
