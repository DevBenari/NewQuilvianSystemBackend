# HR Master Data — `CreateByName` / `UpdateByName` di Area Organization (GAP-4)

| | |
|---|---|
| Tanggal | 2026-08-12 |
| Branch | `MHamzah` |
| Pemicu | GAP-4 / tugas T2 di `docs/hamzah/task/hr-master-data-frontend-gaps.md` |
| Migration | **Tidak ada** — hanya field response dan pengisiannya |
| Breaking change | **Tidak** — dua field baru ditambahkan, tidak ada field lama yang diubah atau dihapus |

## Kenapa diubah

Delapan DTO di `Areas/Corporate/HumanResource/MasterData/Organization/DTOs/` hanya punya
`Guid? CreateBy` tanpa `string? CreateByName`.

Frontend punya aturan keras: **UUID dilarang tampil di UI**. Karena tidak ada field nama,
kolom **Dibuat Oleh** di tabel dan baris audit di halaman detail menampilkan `-`. Itu
perilaku frontend yang benar dan sudah dipilih sadar — Guid **tidak** akan ditampilkan
sebagai gantinya.

Tidak bisa diselesaikan di frontend: nama user memang tidak pernah dikirim backend, dan
frontend tidak punya endpoint untuk menukar Guid menjadi nama per baris tabel.

Gap ini spesifik area Organization. Area HR lain (Payroll, Performance, Credentialing,
Training, Scheduling, Workflow, Leave) sudah mengirim `CreateByName` sejak awal.

## Endpoint yang terpengaruh

Untuk kedelapan master: **`GET ""` (list)** dan **`GET /{id:guid}` (detail)**.

| Route | Controller | List | Detail |
|---|---|---|---|
| `cost-centers` | `CostCenterController.cs` | + `createByName` | + `createByName`, `updateByName` |
| `employee-grades` | `EmployeeGradeController.cs` | + `createByName` | + `createByName`, `updateByName` |
| `hospital-sites` | `HospitalSiteController.cs` | + `createByName` | + `createByName`, `updateByName` |
| `job-families` | `JobFamilyController.cs` | + `createByName` | + `createByName`, `updateByName` |
| `job-levels` | `JobLevelController.cs` | + `createByName` | + `createByName`, `updateByName` |
| `legal-entities` | `LegalEntityController.cs` | + `createByName` | + `createByName`, `updateByName` |
| `organization-units` | `OrganizationUnitController.cs` | + `createByName` | + `createByName`, `updateByName` |
| `work-locations` | `WorkLocationController.cs` | + `createByName` | + `createByName`, `updateByName` |
| `performance-templates/{id}/details` | `PerformanceTemplateDetailController.cs` | + `createByName` | + `createByName`, `updateByName` |

Endpoint yang **sengaja TIDAK diubah**:

| Endpoint | Alasan |
|---|---|
| `GET /options` | Dipakai untuk mengisi select. Tidak menampilkan kolom audit, jadi nama pembuat tidak relevan dan hanya menambah query |
| `GET /summary` | Hanya angka agregat |
| `GET /filters/metadata` | Hanya metadata filter |
| `POST`, `PUT`, `PATCH`, `DELETE` | Bentuk response-nya tidak memuat kolom audit di area ini. Sengaja tidak diubah supaya diff tetap kecil |
| `OrganizationController` (departments + positions) | **Sudah** mengirim `createByName` sejak sebelum pekerjaan ini |

## Kontrak parameter / field

Tidak ada parameter baru. Dua field response baru:

| Field | Tipe | Perilaku |
|---|---|---|
| `createByName` | `string?` | Nama pembuat. `null` kalau `createBy` kosong, atau kalau user-nya sudah tidak ada di tabel `Users` |
| `updateByName` | `string?` | Hanya di response **detail**. `null` kalau data belum pernah diubah |

Urutan sumber nama: `DisplayName` → `UserName` → `Email` → `UserCode`. Sama persis dengan
`BankController` dan `LeaveTypeController`, jadi nama yang muncul konsisten lintas modul.

**Nilai `null` tetap dikirim sebagai `null`.** Frontend menampilkannya sebagai `-`, dan itu
memang perilaku yang diinginkan — jangan diganti string kosong atau Guid.

## Cara pengisian — batch, bukan query per baris

Ini bagian yang paling mudah salah, jadi ditulis eksplisit.

**Yang dipakai** — dua tahap, satu query tambahan per request:

```csharp
var items = await ...Select(x => new XResponse { ... }).ToListAsync();   // proyeksi lama, tidak diubah

var actorNames = await GetActorNameMapAsync(items.Select(x => x.CreateBy));
foreach (var item in items)
    item.CreateByName = GetActorName(actorNames, item.CreateBy);
```

**Yang TIDAK dipakai** — pola subquery per baris yang ada di `OrganizationController.cs:177-186`:

```csharp
CreateByName = x.CreateBy == Guid.Empty ? null
    : _dbContext.Users.Where(u => u.Id == x.CreateBy).Select(u => ...).FirstOrDefault(),
```

Pola itu menghasilkan satu subquery berkorelasi **untuk setiap baris** di SQL. Untuk halaman
100 baris berarti 100 subquery. Register GAP-4 memang meminta batch, jadi
`OrganizationController` sengaja **tidak** dijadikan acuan meski letaknya paling dekat.

Pola dua tahap ini bukan pola baru — sudah dipakai di
`WorkforceCore/Services/EmployeeProfileChangeService.cs:349-368`. Dipilih dibanding pola
`BankController` (materialisasi entity lalu `MapResponse`) karena kedelapan controller ini
sudah punya proyeksi server-side yang ramping; mengubahnya jadi materialisasi entity akan
menarik seluruh kolom dan mengubah SQL yang hari ini sudah benar. Dengan pola dua tahap,
**SQL list yang lama tidak berubah sama sekali** — hanya bertambah satu query kecil ke tabel
`Users`.

Untuk detail, hanya ada satu entity, jadi langsung mengikuti `BankController`:

```csharp
var actorNames = await GetActorNameMapAsync(new Guid?[] { entity.CreateBy, entity.UpdateBy });
```

## File yang disentuh

**9 DTO** — masing-masing menambah dua baris:

| Path | Perubahan |
|---|---|
| `Organization/DTOs/CostCenterDtos.cs` | `CreateByName` di response, `UpdateByName` di detail |
| `Organization/DTOs/EmployeeGradeDtos.cs` | sama |
| `Organization/DTOs/HospitalSiteDtos.cs` | sama |
| `Organization/DTOs/JobFamilyDtos.cs` | sama |
| `Organization/DTOs/JobLevelDtos.cs` | sama |
| `Organization/DTOs/LegalEntityDtos.cs` | sama |
| `Organization/DTOs/OrganizationUnitDtos.cs` | sama |
| `Organization/DTOs/WorkLocationDtos.cs` | sama |
| `Performance/DTOs/PerformanceTemplateDetailDtos.cs` | sama |

**9 Controller** — masing-masing tiga titik sentuh: pengisian di list, pengisian di detail,
dan dua helper privat (`GetActorNameMapAsync` + `GetActorName`):

| Path | Perubahan |
|---|---|
| `Organization/Controllers/CostCenterController.cs` | list + detail + 2 helper |
| `Organization/Controllers/EmployeeGradeController.cs` | sama |
| `Organization/Controllers/HospitalSiteController.cs` | sama |
| `Organization/Controllers/JobFamilyController.cs` | sama |
| `Organization/Controllers/JobLevelController.cs` | sama |
| `Organization/Controllers/LegalEntityController.cs` | sama |
| `Organization/Controllers/OrganizationUnitController.cs` | sama |
| `Organization/Controllers/WorkLocationController.cs` | sama |
| `Performance/Controllers/PerformanceTemplateDetailController.cs` | list + detail + 2 helper, dinamai `ActorNamesAsync`/`ActorName` mengikuti gaya ringkas file itu dan meneruskan `CancellationToken` |

## Catatan: kenapa helper diduplikasi, bukan dijadikan satu

Kesembilan controller mendapat salinan helper yang sama. Itu disengaja, mengikuti apa yang
sudah ada di repo: `BankController`, `LeaveTypeController`, `OvertimeRateController`, dan
belasan controller lain juga masing-masing punya salinan sendiri.

Menariknya ke satu helper bersama adalah refactor lintas area yang menyentuh ±20 controller
di luar GAP-4 — layak jadi tugas terpisah, tidak dicampur ke sini.

Varian yang dipakai di sini memakai `Dictionary<Guid, string>` (bukan `Dictionary<Guid,
string?>`), meniru `EmployeeProfileChangeService`. Varian itu yang **tidak** memancing
warning `CS8619` — berbeda dengan varian `BankController`/`LeaveTypeController` yang
memancingnya di ±20 berkas. Controller lama tidak ikut dibersihkan; itu tugas terpisah.

## Dampak ke frontend

**Tidak ada perubahan frontend yang dibutuhkan.** Frontend sudah siap membaca field ini:

```js
dataKeys: ["createByName", "CreateByName", "createdByName", "CreatedByName"]
```

Begitu backend mengirimnya, kolom **Dibuat Oleh** di delapan halaman master data terisi
otomatis. Baris audit di halaman detail juga langsung menampilkan nama pengubah.

Halaman yang terpengaruh: Pusat Biaya, Grade Karyawan, Fasilitas Rumah Sakit, Rumpun
Jabatan, Level Jabatan, Legal Entity, Unit Organisasi, Lokasi Kerja.

`performance-templates/{id}/details` belum punya halaman frontend — dikerjakan supaya GAP-4
tertutup penuh, bukan karena ada yang menunggu.

## Cara menguji

```http
GET /api/v1/corporate/human-resource/master-data/job-families?pageSize=10
```

Setiap item sekarang punya `createByName`:

```json
{
  "id": "…",
  "jobFamilyCode": "JF-MMC-00001",
  "jobFamilyName": "Keperawatan",
  "isActive": true,
  "createDateTime": "2026-08-01T03:11:20Z",
  "createBy": "…",
  "createByName": "Muhammad Hamzah"
}
```

```http
GET /api/v1/corporate/human-resource/master-data/job-families/{id}
```

Detail menambah `updateByName`. Kalau data belum pernah diubah, `updateBy` dan
`updateByName` sama-sama `null` — itu benar.

**Kasus "perilaku lama harus tetap sama":**

| Uji | Hasil yang diharapkan |
|---|---|
| Field lama pada list & detail | Persis sama seperti sebelumnya — hanya bertambah field baru |
| Data dengan `createBy` kosong | `createByName` = `null`, bukan `""` |
| Data yang user pembuatnya sudah dihapus dari `Users` | `createByName` = `null`, request tetap **200**, bukan error |
| `GET /options` | **Tidak** memuat `createByName` — memang sengaja |
| `GET /summary` | Tidak berubah |

Untuk memastikan pengisiannya benar-benar batch: aktifkan log SQL EF, panggil list dengan
`pageSize=100`, dan pastikan hanya ada **satu** query tambahan ke tabel `Users` — bukan 100.

## Status verifikasi

| Pemeriksaan | Hasil |
|---|---|
| `dotnet build QuilvianSystemBackend.csproj --no-incremental` | **Sukses — 125 Warning, 0 Error** (dijalankan 2026-08-12, 5 menit 29 detik) |
| Warning baru dari 18 berkas yang disentuh | **Tidak ada** — hitungan warning sama persis dengan baseline repo (125), dan penyaringan log build atas ke-18 nama berkas menghasilkan **0 baris** |
| Keseragaman edit (9 DTO, 9 controller) | Terverifikasi lewat penyisiran: tiap DTO +2 field, tiap controller 2 pemanggilan map + 1 definisi helper + 3 pemakaian `GetActorName` |
| Migration | Tidak dibuat, tidak dibutuhkan |
| **Uji endpoint lewat Swagger** | **Belum dijalankan** |
| **Pemeriksaan jumlah query lewat log SQL** | **Belum dijalankan** |
