# GAP-4 — `CreateByName` / `UpdateByName` area Organization — ✅ SELESAI

| | |
|---|---|
| Status | ✅ Selesai 2026-08-12 |
| Cakupan | **9 DTO + 9 controller** (8 Organization + 1 Performance template detail) |
| Tugas register | T2 |
| Commit | ⬜ belum di-commit — menunggu user |
| Migration | **Tidak ada** |
| Breaking change | **Tidak** — dua field baru, tidak ada field lama yang berubah |
| Laporan teknis | `docs/hamzah/report/organization-create-by-name.md` |
| Register asal | `docs/hamzah/task/hr-master-data-frontend-gaps.md` |

---

## Masalah yang ditutup

Delapan DTO area Organization hanya punya `Guid? CreateBy` tanpa `string? CreateByName`.
Karena frontend melarang UUID tampil di UI, kolom **Dibuat Oleh** menampilkan `-` di
delapan halaman master data.

Sekarang backend mengirim namanya, dan kolom itu terisi **tanpa perubahan frontend apa pun**
— frontend sudah membaca `createByName` lewat `dataKeys`.

---

## Yang perlu diperhatikan saat menguji

> ⚠️ Hanya **list** dan **detail** yang mendapat field baru. `/options`, `/summary`, dan
> `/filters/metadata` **sengaja tidak diubah**.

> ⚠️ `updateByName` hanya ada di response **detail**, tidak di list.

> ⚠️ `null` adalah hasil yang **benar** untuk data yang `createBy`-nya kosong atau yang user
> pembuatnya sudah tidak ada di tabel `Users`. Frontend menampilkannya sebagai `-`.

---

## Delapan route Organization

Semua diuji dengan cara yang sama. Tag Swagger: Corporate / Human Resource / Master Data / …

| # | Route | Nama halaman frontend |
|---|---|---|
| 1 | `cost-centers` | Pusat Biaya |
| 2 | `employee-grades` | Grade Karyawan |
| 3 | `hospital-sites` | Fasilitas Rumah Sakit |
| 4 | `job-families` | Rumpun Jabatan |
| 5 | `job-levels` | Level Jabatan |
| 6 | `legal-entities` | Legal Entity |
| 7 | `organization-units` | Unit Organisasi |
| 8 | `work-locations` | Lokasi Kerja |

### Uji list

```http
GET /api/v1/corporate/human-resource/master-data/{route}?pageSize=10
```

Setiap item **wajib** punya `createByName` di samping `createBy`:

```json
{
  "id": "…",
  "jobFamilyCode": "JF-MMC-00001",
  "jobFamilyName": "Keperawatan",
  "isActive": true,
  "createDateTime": "2026-08-01T03:11:20Z",
  "createBy": "8f2c…",
  "createByName": "Muhammad Hamzah"
}
```

Kalau `createByName` tidak muncul sama sekali di JSON, berarti DTO belum terpasang.
Kalau muncul tapi selalu `null` padahal `createBy` terisi, berarti pengisiannya belum jalan.

### Uji detail

```http
GET /api/v1/corporate/human-resource/master-data/{route}/{id}
```

Menambah `updateByName`:

```json
{
  "createBy": "8f2c…",
  "createByName": "Muhammad Hamzah",
  "updateDateTime": null,
  "updateBy": null,
  "updateByName": null
}
```

Ubah datanya lewat `PUT`, lalu panggil detail lagi — `updateBy` dan `updateByName` harus
terisi.

### Uji endpoint yang sengaja tidak berubah

```http
GET /…/{route}/options?onlyActive=true      → TIDAK ada createByName
GET /…/{route}/summary                       → tidak berubah
GET /…/{route}/filters/metadata              → tidak berubah
```

---

## Route ke-9 — Performance Template Detail

**Tag Swagger:** Corporate / Human Resource / Master Data / Performance Template Detail

```http
GET /api/v1/corporate/human-resource/master-data/performance-templates/{templateId}/details
GET /api/v1/corporate/human-resource/master-data/performance-templates/{templateId}/details/{id}
```

Register menandainya prioritas rendah karena **belum punya halaman frontend**. Dikerjakan
supaya GAP-4 tertutup penuh, bukan karena ada yang menunggu.

---

## Uji yang paling penting: pastikan batch, bukan per baris

Ini inti Definition of Done GAP-4 — bukan sekadar field-nya muncul.

1. Aktifkan log SQL EF Core.
2. Panggil list dengan `pageSize=100` pada data yang ramai.
3. Hitung query ke tabel `Users`.

| Hasil | Artinya |
|---|---|
| **1 query** ke `Users` | ✅ Benar — batch |
| **100 subquery** berkorelasi di dalam query list | ❌ Salah — itu pola lama `OrganizationController` |

Pola subquery-per-baris memang ada di repo (`OrganizationController.cs:177-186`), tapi
**sengaja tidak dipakai** di sini karena register meminta batch.

---

## Ringkasan perubahan

| Berkas | Perubahan |
|---|---|
| 9 DTO | `string? CreateByName` di response list, `string? UpdateByName` di response detail |
| 9 Controller | Pengisian di list (batch + `foreach`), pengisian di detail, plus dua helper privat |

Yang **tidak** berubah: proyeksi SQL list yang lama, paging, urutan hasil, filter, dan
seluruh field lama. Hanya bertambah satu query kecil ke `Users` per request list/detail.

Sumber nama: `DisplayName` → `UserName` → `Email` → `UserCode`, sama persis dengan
`BankController` dan `LeaveTypeController` supaya konsisten lintas modul.

## Dampak ke frontend

**Tidak ada yang perlu diubah.** Frontend sudah membaca
`["createByName", "CreateByName", "createdByName", "CreatedByName"]`. Kolom **Dibuat Oleh**
di delapan halaman terisi otomatis begitu backend ini naik.

## Status verifikasi

| Pemeriksaan | Hasil |
|---|---|
| `dotnet build --no-incremental` | **Sukses — 125 Warning, 0 Error** (sama persis dengan baseline) |
| Warning baru dari 18 berkas yang disentuh | **Tidak ada** |
| Keseragaman edit | ✅ Terverifikasi — tiap DTO +2 field, tiap controller 2 map + 1 helper + 3 pemakaian |
| **Uji Swagger** | **Belum dijalankan** |
| **Hitung query lewat log SQL** | **Belum dijalankan** |
