# Benefit Type — Dukungan Filter Rentang Tanggal

| | |
|---|---|
| Tanggal | 2026-08-10 |
| Branch | `MHamzah` |
| Pemicu | Audit master data HR 2026-08-04 (repo frontend) |
| Migration | **Tidak ada** |
| Breaking change | **Tidak** — ketiga parameter opsional |

## Kenapa diubah

Kontrak baku master data di frontend mewajibkan filter Tanggal Mulai, Tanggal Akhir, dan
Periode selalu tampil di setiap halaman list. Dari 36 entitas master data HR, **21
controller** tidak menerima `startDate` / `endDate` / `customPeriod`, sehingga filternya
dirender tapi nilainya tidak pernah dikirim — filter yang terlihat aktif tapi tidak
mengubah hasil sama sekali.

15 entitas lain sudah mendukungnya. Jadi ini **meratakan kontrak yang sudah ada**, bukan
menambah fitur baru. `benefit-type` dikerjakan lebih dulu sebagai pilot.

## Endpoint yang terpengaruh

| Endpoint | Perubahan |
|---|---|
| `GET /api/v1/corporate/human-resource/master-data/benefit-types` | Menerima `startDate`, `endDate`, `customPeriod` |
| `GET /api/v1/corporate/human-resource/master-data/benefit-types/filters/metadata` | Response menambah `customPeriods[]`; `defaultFilter` menambah `startDate`, `endDate`, `customPeriod` |

`GET /summary` **tidak** ikut difilter tanggal — mengikuti perilaku entitas yang sudah ada
(`AllowanceTypeController`), supaya kartu ringkasan tetap menampilkan total keseluruhan.

## Kontrak parameter

| Parameter | Tipe | Keterangan |
|---|---|---|
| `startDate` | `DateTime?` | Batas awal, inklusif. Dinormalkan ke awal hari UTC |
| `endDate` | `DateTime?` | Batas akhir, **eksklusif** di belakang layar (`endDate` + 1 hari), sehingga tanggal yang diminta ikut terhitung |
| `customPeriod` | `string?` | `today`, `last7days`, `thismonth`, `lastmonth`. Diabaikan kalau `startDate`/`endDate` terisi |

Filter berjalan pada kolom `CreateDateTime`. Kalau ketiganya kosong, query tidak berubah
sama sekali — karena itu pemanggil lama (termasuk frontend versi sebelumnya) tidak
terpengaruh.

## File yang disentuh

| File | Perubahan |
|---|---|
| `Areas/Corporate/HumanResource/MasterData/PayrollAndBenefit/Controllers/BenefitTypeController.cs` | `GetBenefitTypes` menerima 3 parameter baru; query lewat `ApplyDateFilter`; `GetFilterMetadata` mengisi `CustomPeriods`; helper `ApplyDateFilter`, `ResolveDateRange`, `BuildPeriodOptions` ditambahkan di akhir kelas |
| `Areas/Corporate/HumanResource/MasterData/PayrollAndBenefit/DTOs/BenefitTypeDtos.cs` | `BenefitTypeDefaultFilterResponse` + `StartDate`/`EndDate`/`CustomPeriod`; kelas baru `BenefitTypeCustomPeriodOptionResponse`; `BenefitTypeFilterMetadataResponse` + `CustomPeriods` |

Helper-nya **disalin persis** dari `AllowanceTypeController.cs:753-797`, bukan implementasi
baru — supaya semantik rentang tanggal seragam di seluruh master data.

## Dampak ke frontend

`supportedFilterKeys` pada `benefit-type-constants.jsx` menambahkan `startDate`, `endDate`,
`customPeriod`. Tidak ada perubahan lain — `filterDefaults` sudah memuat ketiganya sejak
audit.

## Cara menguji

```bash
# Semua data (perilaku lama, harus tidak berubah)
GET /api/v1/corporate/human-resource/master-data/benefit-types

# Rentang eksplisit — data tanggal 10 harus ikut terhitung
GET /api/v1/corporate/human-resource/master-data/benefit-types?startDate=2026-08-01&endDate=2026-08-10

# Periode preset
GET /api/v1/corporate/human-resource/master-data/benefit-types?customPeriod=thismonth

# Metadata — pastikan customPeriods berisi 4 opsi
GET /api/v1/corporate/human-resource/master-data/benefit-types/filters/metadata
```

## Sisa pekerjaan

Daftar 20 entitas yang sempat ditulis di sini berasal dari audit frontend 2026-08-04 dan
**sudah usang** — audit itu hanya mencakup 36 entitas yang ada saat itu. Penyisiran ulang
langsung ke kode backend pada 2026-08-11 menemukan **35 controller** tanpa `startDate`.

Daftar yang berlaku sekarang ada di `docs/hamzah/task/hr-master-data-frontend-gaps.md`
(GAP-1), dipecah per area beserta urutan pengerjaannya. Jangan pakai daftar lama di atas.

## Status verifikasi

| Pemeriksaan | Hasil |
|---|---|
| `dotnet build` | **Sukses** — dijalankan 2026-08-11, 0 Warning, 0 Error (SDK 9.0.316 sudah terpasang) |
| Frontend `npx eslint` | 0 error |
| Frontend `npm run build` | sukses |
| Uji endpoint manual lewat Swagger | **Belum dijalankan** |
