# Kontrak Master Data — Sisi Backend

Dokumen ini menerjemahkan kontrak baku master data **frontend** menjadi kewajiban konkret
di backend. Dipakai oleh dua skill: `master-data-audit` (memeriksa) dan `master-data-set`
(mengerjakan).

Sumber aslinya ada di repo frontend dan **tetap jadi acuan tertinggi**:

| Dokumen frontend | Isi |
|---|---|
| `QuilvianFrontEnd/.claude/rules/rules-master-data.md` | Kontrak UI baku (filter 7 elemen, 4 kolom wajib, urutan field form) |
| `QuilvianFrontEnd/.claude/rules/no-uuid-display.md` | UUID dilarang tampil — sumber kewajiban field `*Name` |
| `QuilvianFrontEnd/.claude/commands/master-data-audit.md` | Daftar jebakan yang sudah terbukti |

> **Prinsip yang tidak berubah:** backend adalah sumber kebenaran kontrak API. Frontend yang
> menyesuaikan. Backend diubah hanya kalau kebutuhannya memang tidak bisa diselesaikan di
> frontend — bukan supaya frontend lebih enak ditulis.

---

## 1. Delapan endpoint baku per entitas

Satu master data yang lengkap punya delapan endpoint ini:

| # | Endpoint | Wajib | Keterangan |
|---|---|---|---|
| 1 | `GET /filters/metadata` | ✅ | `defaultFilter`, `sortOptions`, `sortDirections`, `customPeriods`, `pageSizeOptions` |
| 2 | `GET /summary` | ✅ | Kartu ringkasan. **Tidak** ikut difilter tanggal |
| 3 | `GET ""` | ✅ | List + paging + search + `isActive` + rentang tanggal |
| 4 | `GET /options` | ✅ | Untuk select relasi di master lain |
| 5 | `GET /{id:guid}` | ✅ | Detail |
| 6 | `POST ""` | ✅ | Create — kode **auto-generate** |
| 7 | `PUT /{id:guid}` | ✅ | Update |
| 8 | `PATCH /{id:guid}/status` | ✅ | Body `{ isActive: bool }` |
| 9 | `DELETE /{id:guid}` | ✅ | Soft delete |

Pola acuan paling sederhana: `JobFamilyController`. Untuk entitas dengan relasi:
`LegalEntityController`.

---

## 2. Filter rentang tanggal — `startDate`, `endDate`, `customPeriod`

Frontend **wajib** merender tiga kontrol tanggal di setiap halaman master data, bahkan
kalau backend belum mendukungnya. Kalau backend tidak menerima parameternya, kontrol itu
tampil aktif tapi tidak mengubah hasil — kontrol palsu yang menyesatkan user.

### Kontrak parameter

| Parameter | Tipe | Perilaku |
|---|---|---|
| `startDate` | `DateTime?` | Batas awal **inklusif**, dinormalkan ke awal hari UTC |
| `endDate` | `DateTime?` | Batas akhir; di belakang layar **eksklusif** (`endDate` + 1 hari) supaya tanggal yang diminta ikut terhitung |
| `customPeriod` | `string?` | `today`, `last7days`, `thismonth`, `lastmonth`. **Diabaikan** kalau `startDate` atau `endDate` terisi |

- Kolom yang difilter: **`CreateDateTime`**.
- Ketiganya kosong → query **tidak berubah sama sekali**, sehingga pemanggil lama aman.
- `GET /summary` **tidak** ikut difilter — kartu ringkasan menampilkan total keseluruhan.

### Pola yang sudah ada — jangan bikin baru

| Pola | Lokasi |
|---|---|
| Helper generik | `Areas/Corporate/HumanResource/MasterData/Workflow/Controllers/WorkflowMasterDataSupport.cs` — `ApplyDateFilter<T>()` untuk `IQueryable<T> where T : IdentityModel` |
| Helper per-controller | `Areas/.../PayrollAndBenefit/Controllers/AllowanceTypeController.cs` — disalin ke `BenefitTypeController` sebagai pilot |

Empat titik sentuh per controller:

1. Action list menerima `[FromQuery] DateTime? startDate`, `endDate`, `string? customPeriod`.
2. `var query = ApplyDateFilter(BuildBaseQuery(), startDate, endDate, customPeriod);`
3. `GET /filters/metadata` mengisi `CustomPeriods = BuildPeriodOptions()` (4 opsi).
4. DTO: `<Entitas>DefaultFilterResponse` + `StartDate`/`EndDate`/`CustomPeriod`;
   `<Entitas>FilterMetadataResponse` + `CustomPeriods`.

### Inkonsistensi yang sudah ada

`doctors`, `employees`, dan `external-users` memakai nama parameter **`period`**, bukan
`customPeriod`. Frontend sudah menyesuaikan diri. **Jangan mengganti namanya** — kalau perlu
diseragamkan, tambahkan `customPeriod` sebagai alias yang diterima berdampingan.

---

## 3. `GET /options` — Id + Code + Name sebagai field terpisah

Endpoint options adalah sumber data select relasi di master lain.

**Wajib:**

- Proyeksi mengisi **`Id`, kode, dan nama** — plus seluruh field lain yang dideklarasikan DTO.
- Kode dan nama dikirim sebagai **field terpisah**, jangan hanya digabung di `Label`.
  Frontend menampilkan **nama saja**, dan memakai kode untuk pencarian.

**Jebakan yang sudah terjadi:** proyeksi berbentuk

```csharp
var response = new XOptionResponse();
response.Id = x.Id;      // ← hanya ini, sisanya tertinggal di nilai default
return response;
```

mengembalikan HTTP 200 dengan item yang **punya value tapi tanpa label**. Tidak ada error,
request tetap 200, select hanya "tidak ada isinya". Sudah pernah terjadi di
`benefit-types`, `benefit-plans`, dan `deduction-types`.

Nama field diambil dari **proyeksi list** di controller yang sama — jangan dikarang ulang,
supaya satu entitas tidak punya dua bentuk data yang berbeda.

Perhatikan beda nama parameter: options memakai `onlyActive`, list memakai `isActive`.

---

## 4. Field audit — `CreateByName` / `UpdateByName`

Frontend punya aturan keras: **UUID dilarang tampil di UI**. Konsekuensinya, DTO yang hanya
punya `Guid? CreateBy` tanpa `string? CreateByName` membuat kolom **Dibuat Oleh** menampilkan
`-`. Itu perilaku yang benar dan disengaja — Guid **tidak** akan ditampilkan sebagai gantinya.

**Wajib:**

- DTO list menambah `string? CreateByName`.
- DTO detail menambah `string? CreateByName` **dan** `string? UpdateByName`.
- Controller mengisinya lewat `GetActorNameMapAsync` — **batch**, jangan query per baris.
- Nilai `null` tetap dikirim sebagai `null`. Frontend menampilkan `-`, itu benar.

Pola acuan: `Areas/Administrator/MasterData/DTOs/BankDtos.cs` (`CreateBy` + `CreateByName`).

Frontend sudah siap membaca field ini (`dataKeys: ["createByName", "CreateByName",
"createdByName", "CreatedByName"]`) — begitu backend mengirimnya, kolom terisi otomatis
tanpa perubahan frontend apa pun.

---

## 5. Create, Update, Status, Delete

### Kode auto-generate

Kode entitas **dihasilkan backend** lewat `GenerateCodeAsync`, bukan diinput user.
Frontend tidak merender field kode di form dan tidak mengirimnya di payload create.

> Enam master area Workflow memakai kode input user. Itu **pengecualian yang sudah
> terlanjur** — jangan ditiru untuk master baru.

### `SortOrder` pada `PUT` — jangan ditimpa tanpa syarat

`sortOrder` dilarang tampil di form frontend. Kalau `[HttpPut]` menulis

```csharp
entity.SortOrder = request.SortOrder;   // ❌ model binding memberi 0
```

maka **setiap update mereset urutan ke 0**. Pakai salah satu:

```csharp
entity.SortOrder = request.SortOrder ?? entity.SortOrder;   // properti dibuat nullable
```

Aturan yang sama berlaku untuk **semua** field yang tidak dirender frontend tapi tetap ada
di request DTO.

### Ubah status

Pakai **`PATCH /{id:guid}/status`** dengan body `{ isActive: bool }`.
Bukan `/activate` + `/deactivate` terpisah.

### Delete

Sebagian grup (Cuti & Lembur) menerima body alasan, grup lain tidak. **Perbedaan ini
dibiarkan** — frontend sudah menyesuaikan per master lewat `requireReason`. Jangan
menyeragamkannya tanpa diminta.

---

## 6. Yang sengaja TIDAK jadi kewajiban backend

Supaya audit tidak menghasilkan pekerjaan mubazir:

| Hal | Alasan |
|---|---|
| `Label = Code + " - " + Name` di `/options` | Frontend menampilkan nama saja. Boleh ada, tapi bukan pengganti field terpisah |
| Endpoint options untuk field ber-akhiran `Id` yang bukan FK | mis. `timeZoneId` — string IANA, bukan relasi |
| Enum untuk field string bebas | mis. `violationCategory`, `severityLevel` — frontend memakai input teks |
| Mengembalikan option master lain di `/filters/metadata` | Bukan tempatnya. Select relasi memakai `/options` lewat registry frontend |
| Filter klasifikasi domain yang sudah ada (`isDefault`, `legalEntityId`) | Frontend sengaja tidak merendernya demi baris filter seragam. Query param-nya tetap berguna |

---

## 7. Definition of Done — satu entitas dianggap patuh kalau

```
[ ] GET /filters/metadata   defaultFilter, sortOptions, sortDirections,
                            customPeriods (4 opsi), pageSizeOptions
[ ] GET /summary            ada, dan TIDAK ikut difilter tanggal
[ ] GET ""                  paging + search + isActive + startDate/endDate/customPeriod
[ ] GET /options            proyeksi mengisi Id + Code + Name sebagai field terpisah
[ ] GET /{id:guid}          ada
[ ] POST ""                 kode auto-generate lewat GenerateCodeAsync
[ ] PUT /{id:guid}          SortOrder tidak ditimpa tanpa syarat
[ ] PATCH /{id}/status      body { isActive: bool }
[ ] DELETE /{id:guid}       ada
[ ] DTO list                CreateBy + CreateByName
[ ] DTO detail              CreateBy + CreateByName, UpdateBy + UpdateByName
[ ] Kompatibilitas          GET tanpa parameter menghasilkan output yang sama persis
                            seperti sebelum perubahan
```

Butir terakhir yang paling sering terlewat. Seluruh penambahan di dokumen ini bersifat
**aditif** — parameter opsional, field tambahan, endpoint baru. Tidak ada yang boleh
mengubah perilaku pemanggil lama.
