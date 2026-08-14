# Benefit Type, Benefit Plan, Deduction Type — Proyeksi `/options` Mengisi Code dan Name

| | |
|---|---|
| Tanggal | 2026-08-11 |
| Branch | `MHamzah` |
| Pemicu | GAP-3 pada `docs/hamzah/task/hr-master-data-frontend-gaps.md` (tugas T1) |
| Migration | **Tidak ada** |
| Breaking change | **Tidak** — hanya mengisi field yang selama ini terkirim kosong; tidak ada field yang dihapus atau berganti nama |

## Kenapa diubah

Ketiga endpoint `/options` mengembalikan HTTP 200 beserta daftar item dan paging yang benar,
tetapi proyeksinya hanya mengisi `Id`:

```csharp
var response = new BenefitTypeOptionResponse();
response.Id = x.Id;      // ← hanya ini, sisanya tetap nilai default
return response;
```

Akibatnya seluruh field lain terkirim sebagai string kosong. Select relasi di frontend
menerima daftar opsi yang **punya value tapi tanpa label** — pengguna melihat baris kosong
yang bisa diklik.

Ini tidak bisa diselesaikan di frontend. Nama entitas memang tidak pernah dikirim backend,
jadi tidak ada yang bisa dipetakan. Sebagai penambal sementara, frontend mengalihkan
`benefitTypes` dan `benefitPlans` ke **endpoint list** (`GET /benefit-types`,
`GET /benefit-plans`) yang proyeksinya sudah benar — konsekuensinya kedua resource itu
memakai parameter `isActive`, bukan `onlyActive` seperti options lainnya.

## Endpoint yang terpengaruh

| Endpoint | Perubahan |
|---|---|
| `GET /api/v1/corporate/human-resource/master-data/benefit-types/options` | Item mengisi `benefitTypeCode`, `benefitTypeName`, `benefitCategory`, `fundingType`, `isTaxable`, `requiresEnrollment`, `allowsDependents` |
| `GET /api/v1/corporate/human-resource/master-data/benefit-plans/options` | Item mengisi `benefitPlanCode`, `benefitPlanName` (field baru), `benefitTypeCode`, `benefitTypeName`, serta empat Guid relasi |
| `GET /api/v1/corporate/human-resource/master-data/deduction-types/options` | Item mengisi `deductionTypeCode`, `deductionTypeName`, `deductionCategory`, `calculationMethod`, `payrollComponentId`, `payrollComponentCode`, `payrollComponentName` |

Yang **tidak** diubah:

- Signature action, parameter (`onlyActive`, `search`, `pageNumber`, `pageSize`), urutan
  (`OrderBy` nama), dan filter — semuanya tetap.
- Bentuk amplop response (`ApiResponse` + `...OptionPagedResponse`) dan pesan suksesnya.
- `BuildBaseQuery()` ketiga controller — `Include` yang dibutuhkan sudah ada sejak awal
  (`BenefitPlan` → `BenefitType`, `DeductionType` → `PayrollComponent`), jadi tidak ada
  query tambahan ke database.
- Field `Label` gabungan `"Code - Name"` **tidak ditambahkan**. Frontend menampilkan `Name`
  saja dan memakai `Code` untuk pencarian, sehingga keduanya harus tetap terpisah.

## Kontrak field

| Field | Tipe | Perilaku |
|---|---|---|
| `benefitTypeCode` / `benefitTypeName` (pada `benefit-types`) | `string` | Selalu terisi, tidak pernah null |
| `benefitPlanCode` / `benefitPlanName` | `string` | **Field baru** pada `BenefitPlanOptionResponse` |
| `benefitTypeCode` / `benefitTypeName` (pada `benefit-plans`) | `string?` | Milik entitas induk. `null` kalau relasi `BenefitType` tidak ada |
| `payrollComponentCode` / `payrollComponentName` | `string?` | `null` kalau `PayrollComponentId` kosong — relasi ini memang opsional |
| Sisa field | mengikuti deklarasi DTO | Diisi apa adanya dari model |

Nama dan tipe field disalin dari proyeksi **list** di masing-masing controller
(`BenefitTypeResponse`, `BenefitPlanResponse`, `DeductionTypeResponse`), bukan dikarang
ulang — supaya satu entitas tidak punya dua bentuk data yang berbeda.

## File yang disentuh

| File | Perubahan |
|---|---|
| `Areas/.../PayrollAndBenefit/Controllers/BenefitTypeController.cs` | Proyeksi `/options` diganti menjadi object initializer berisi 8 field |
| `Areas/.../PayrollAndBenefit/Controllers/BenefitPlanController.cs` | Proyeksi `/options` diganti menjadi object initializer berisi 10 field |
| `Areas/.../PayrollAndBenefit/Controllers/DeductionTypeController.cs` | Proyeksi `/options` diganti menjadi object initializer berisi 8 field |
| `Areas/.../PayrollAndBenefit/DTOs/BenefitPlanDtos.cs` | `BenefitPlanOptionResponse` menambah `BenefitPlanCode` + `BenefitPlanName` |

## Dampak ke frontend

Tidak ada perubahan frontend yang **wajib** — endpoint tetap kompatibel, hanya isinya jadi
lengkap.

Yang **bisa** dirapikan setelah ini: `benefitTypes` dan `benefitPlans` di
`src/lib/hooks/select/hr/hr-select-resources.js` dikembalikan dari endpoint list ke
`/options`, supaya seragam dengan 22 resource lain dan kembali memakai `onlyActive`.
Itu pekerjaan terpisah, bukan syarat.

`deduction-types/options` belum dipakai frontend mana pun, jadi perbaikannya bersifat
pencegahan — begitu ada form yang butuh select jenis potongan, endpoint-nya sudah benar.

## Cara menguji

```bash
# Ketiganya harus mengembalikan code + name terisi, bukan string kosong
GET /api/v1/corporate/human-resource/master-data/benefit-types/options
GET /api/v1/corporate/human-resource/master-data/benefit-plans/options
GET /api/v1/corporate/human-resource/master-data/deduction-types/options

# Paging dan pencarian harus berperilaku sama seperti sebelumnya
GET /api/v1/corporate/human-resource/master-data/benefit-types/options?search=bpjs&pageSize=5
GET /api/v1/corporate/human-resource/master-data/benefit-types/options?onlyActive=false

# Relasi opsional: deduction type tanpa PayrollComponentId
# → payrollComponentCode dan payrollComponentName harus null, bukan string kosong, dan tidak error
GET /api/v1/corporate/human-resource/master-data/deduction-types/options
```

Yang perlu diperiksa pada hasilnya: `totalData`, `totalPage`, dan urutan item **tidak boleh
berubah** dibanding sebelum perubahan ini — yang bertambah hanya isi tiap item.

## Status verifikasi

| Pemeriksaan | Hasil |
|---|---|
| `dotnet build` | **Sukses** — 2026-08-11, 0 Error |
| Warning baru dari 4 file yang disentuh | **Tidak ada** — 125 warning yang muncul seluruhnya sudah ada sebelumnya di file lain |
| Uji endpoint lewat Swagger | **Belum dijalankan** |
| Penyesuaian frontend | **Belum dikerjakan** — memang tidak diwajibkan |

### Catatan koreksi untuk laporan sebelumnya

`docs/hamzah/report/benefit-type-date-filter.md` menulis hasil build **"0 Warning, 0 Error"**.
Angka warning itu keliru: build tersebut berstatus *up-to-date* (selesai 3 detik, compiler
tidak dijalankan sama sekali karena `dotnet run` sudah mengompilasi lebih dulu), sehingga
tidak ada warning yang sempat dicetak. Rebuild penuh pada 2026-08-11 menunjukkan repo ini
memang membawa **125 warning bawaan**. Bagian **0 Error** dan kesimpulan "build sukses"
tetap benar. Laporan itu ikut dikoreksi pada commit ini.
