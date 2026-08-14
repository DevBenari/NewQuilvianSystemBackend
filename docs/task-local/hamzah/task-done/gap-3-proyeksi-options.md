# GAP-3 — `GET /options` hanya mengembalikan `Id` — ✅ SELESAI

| | |
|---|---|
| Status | ✅ Selesai 2026-08-11 |
| Cakupan | **3 endpoint options** + 1 DTO ditambah field |
| Tugas register | T1 |
| Commit | `8b9de69` — ⬜ belum di-push ke `origin/MHamzah` |
| Migration | **Tidak ada** |
| Breaking change | **Tidak** — bentuk response tidak berubah, hanya field yang tadinya kosong kini terisi |
| Laporan teknis | `docs/hamzah/report/options-projection-code-name.md` |
| Register asal | `docs/hamzah/task/hr-master-data-frontend-gaps.md` |

---

## Masalah yang ditutup

Tiga endpoint `/options` mengembalikan **HTTP 200 dengan daftar item**, tetapi proyeksinya
hanya mengisi `Id` dan membiarkan seluruh field lain pada nilai default:

```csharp
var response = new BenefitTypeOptionResponse();
response.Id = x.Id;      // ← hanya ini, sisanya tertinggal kosong
return response;
```

Akibatnya di frontend: select relasi berisi opsi yang **punya value tapi tanpa label** —
daftar terlihat kosong. Tidak ada error, request tetap 200, jadi gampang terlewat.

Sekarang ketiga proyeksi mengisi **seluruh field yang dideklarasikan DTO-nya**.

---

## Cara menguji di Swagger

Yang dicari: setiap item **wajib** punya `id`, kode, **dan** nama yang terisi.
Kalau ada yang masih `""` atau `null` pada kode/nama, berarti belum benar.

> ⚠️ Endpoint `/options` memakai parameter **`onlyActive`**, bukan `isActive` seperti
> endpoint list. Gampang tertukar saat menguji.

### 1. Benefit Type

**Tag Swagger:** Corporate / Human Resource / Master Data / **Benefit Type**

```http
GET /api/v1/corporate/human-resource/master-data/benefit-types/options?onlyActive=true
```

Bentuk item yang benar:

```json
{
  "id": "…",
  "benefitTypeCode": "BT-MMC-00001",
  "benefitTypeName": "Asuransi Kesehatan",
  "benefitCategory": "…",
  "fundingType": "…",
  "isTaxable": false,
  "requiresEnrollment": true,
  "allowsDependents": true
}
```

### 2. Benefit Plan

**Tag Swagger:** Corporate / Human Resource / Master Data / **Benefit Plan**

```http
GET /api/v1/corporate/human-resource/master-data/benefit-plans/options?onlyActive=true
```

DTO-nya **ditambah dua field baru**: `benefitPlanCode` dan `benefitPlanName` — sebelumnya
memang belum ada, jadi tidak mungkin terisi.

```json
{
  "id": "…",
  "benefitPlanCode": "BP-MMC-00001",
  "benefitPlanName": "Paket Rawat Inap A",
  "benefitTypeId": "…",
  "benefitTypeCode": "…",
  "benefitTypeName": "…",
  "legalEntityId": "…",
  "hospitalSiteId": "…",
  "organizationUnitId": "…",
  "employeeCategoryId": "…"
}
```

### 3. Deduction Type

**Tag Swagger:** Corporate / Human Resource / Master Data / **Deduction Type**

```http
GET /api/v1/corporate/human-resource/master-data/deduction-types/options?onlyActive=true
```

```json
{
  "id": "…",
  "payrollComponentId": "…",
  "payrollComponentCode": "…",
  "payrollComponentName": "…",
  "deductionTypeCode": "DT-MMC-00001",
  "deductionTypeName": "Potongan BPJS",
  "deductionCategory": "…",
  "calculationMethod": "…"
}
```

> `deduction-types/options` **belum pernah dilaporkan** di dokumen frontend mana pun —
> ditemukan saat penyisiran 2026-08-11. Belum terasa karena frontend belum memakai resource
> itu, tapi akan langsung menggigit begitu ada form yang butuh select jenis potongan.

---

## Ringkasan perubahan

| Endpoint | Yang diperbaiki |
|---|---|
| `GET /benefit-types/options` | Proyeksi mengisi seluruh field; DTO sudah punya `BenefitTypeCode` + `BenefitTypeName` sejak awal |
| `GET /benefit-plans/options` | Proyeksi mengisi seluruh field; DTO **ditambahi** `BenefitPlanCode` + `BenefitPlanName` |
| `GET /deduction-types/options` | Proyeksi mengisi seluruh field; DTO sudah punya `DeductionTypeCode` + `DeductionTypeName` sejak awal |

Yang **tidak** berubah: signature action, paging, urutan hasil, dan bentuk response. Tidak
ada query tambahan ke database — `Include` yang diperlukan memang sudah ada di
`BuildBaseQuery()`.

## Kenapa kode dan nama dikirim terpisah

Frontend **menampilkan `Name` saja** dan memakai `Code` untuk pencarian. Jadi keduanya harus
jadi **field terpisah** — menggabungnya di satu field `Label` saja tidak cukup.

## Dampak ke frontend

Tidak ada yang wajib. Saat ini `benefitTypes` dan `benefitPlans` di frontend masih
di-workaround ke **endpoint list** (`GET /benefit-types`, `GET /benefit-plans`) yang
proyeksinya memang sudah benar.

Setelah perbaikan ini, frontend **bisa** dikembalikan ke `/options` supaya konsisten dengan
22 resource lain — tapi itu perubahan frontend, bukan syarat. Perlu diingat kalau
dikembalikan: parameternya berganti dari `isActive` menjadi `onlyActive`.

## Status verifikasi

| Pemeriksaan | Hasil |
|---|---|
| `dotnet build` | Sukses, 0 error |
| **Uji Swagger** | **Belum dijalankan** |
| Penyesuaian frontend ke `/options` | **Belum dijalankan** — tidak wajib |
