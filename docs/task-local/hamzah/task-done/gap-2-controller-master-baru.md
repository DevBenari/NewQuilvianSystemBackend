# GAP-2 — Enam master data belum punya controller — ✅ SELESAI

| | |
|---|---|
| Status | ✅ Selesai 2026-08-12 |
| Cakupan | **6 controller baru + 6 berkas DTO baru** (12 file, semuanya baru) |
| Tugas register | T9 |
| Commit | ⬜ belum di-commit — menunggu user |
| Migration | **Tidak ada** — keenam tabel sudah ada di database |
| Breaking change | **Tidak** — seluruhnya endpoint baru |
| Laporan teknis | `docs/hamzah/report/hr-master-data-six-new-controllers.md` |
| Register asal | `docs/hamzah/task/hr-master-data-frontend-gaps.md` |

---

## Masalah yang ditutup

Keenam master punya Model, EF Configuration, `DbSet`, dan tabel — tapi **tidak ada satu pun
endpoint**. Route-nya menghasilkan **404**, jadi frontend terpaksa memakai input teks UUID
manual di 12+ form, dan datanya tidak bisa ditambah lewat UI sama sekali.

Sekarang keenamnya punya sembilan action dengan bentuk yang identik satu sama lain.

---

## Yang perlu diperhatikan saat menguji

> ⚠️ `/options` memakai parameter **`onlyActive`**, sedangkan endpoint list memakai
> **`isActive`**. Gampang tertukar.

> ⚠️ **Kode auto-generate.** Jangan kirim `workforceTypeCode` dkk. di body `POST` — field
> itu memang tidak ada di request DTO. Kode diisi backend.

> ⚠️ Ubah status pakai **`PATCH /{id}/status`** dengan body `{ "isActive": false }`,
> bukan `/activate` + `/deactivate`.

---

## 1. Workforce Type

**Tag Swagger:** Corporate / Human Resource / Master Data / **Workforce Type**
**Prefix kode:** `WFT-RSMMC-00001`

```http
POST /api/v1/corporate/human-resource/master-data/workforce-types
{
  "workforceTypeName": "Tenaga Medis",
  "description": "Dokter dan tenaga klinis",
  "isInternal": true,
  "isClinical": true
}
```

```http
GET  /api/v1/corporate/human-resource/master-data/workforce-types?isClinical=true&customPeriod=today
GET  /api/v1/corporate/human-resource/master-data/workforce-types/options?onlyActive=true
GET  /api/v1/corporate/human-resource/master-data/workforce-types/summary
```

Bentuk item `/options` yang benar — ketiganya **wajib terisi**:

```json
{
  "id": "…",
  "workforceTypeCode": "WFT-RSMMC-00001",
  "workforceTypeName": "Tenaga Medis",
  "isInternal": true,
  "isClinical": true
}
```

**Uji penjagaan delete:** buat employee category yang menunjuk workforce type ini, lalu
`DELETE` workforce type-nya → harus **400**, bukan terhapus.

---

## 2. Employee Category

**Tag Swagger:** Corporate / Human Resource / Master Data / **Employee Category**
**Prefix kode:** `ECT-RSMMC-00001`

Satu-satunya master di GAP-2 yang punya **relasi**: `workforceTypeId` (opsional).

```http
POST /api/v1/corporate/human-resource/master-data/employee-categories
{
  "employeeCategoryName": "Perawat Pelaksana",
  "workforceTypeId": "<id dari langkah 1>",
  "isClinical": true,
  "requiresCredentialing": true
}
```

Response list dan detail **mengirim nama relasinya**, bukan hanya UUID:

```json
{
  "id": "…",
  "employeeCategoryCode": "ECT-RSMMC-00001",
  "employeeCategoryName": "Perawat Pelaksana",
  "workforceTypeId": "…",
  "workforceTypeCode": "WFT-RSMMC-00001",
  "workforceTypeName": "Tenaga Medis",
  "isClinical": true,
  "requiresCredentialing": true
}
```

**Uji validasi FK:** kirim `workforceTypeId` acak / milik workforce type nonaktif → harus
**400** dengan pesan "Workforce type tidak ditemukan atau tidak aktif."

Filter relasi: `GET …/employee-categories?workforceTypeId=<id>`

---

## 3. Employment Type

**Tag Swagger:** Corporate / Human Resource / Master Data / **Employment Type**
**Prefix kode:** `EMT-RSMMC-00001`

```http
POST /api/v1/corporate/human-resource/master-data/employment-types
{
  "employmentTypeName": "Kontrak (PKWT)",
  "isPermanent": false,
  "isContractBased": true,
  "requiresContractEndDate": true,
  "isPayrollEligible": true,
  "isBenefitEligible": true
}
```

**Uji validasi konsistensi** — keduanya harus **400**:

| Payload | Pesan yang diharapkan |
|---|---|
| `isPermanent: true` **dan** `isContractBased: true` | "…tidak boleh permanen sekaligus berbasis kontrak." |
| `requiresContractEndDate: true` tapi `isContractBased: false` | "…hanya berlaku untuk employment type berbasis kontrak." |

---

## 4. Employment Status

**Tag Swagger:** Corporate / Human Resource / Master Data / **Employment Status**
**Prefix kode:** `EMS-RSMMC-00001`

```http
POST /api/v1/corporate/human-resource/master-data/employment-statuses
{
  "employmentStatusName": "Aktif",
  "isActiveEmployment": true,
  "isSchedulable": true,
  "isPayrollEligible": true,
  "isTerminalStatus": false
}
```

**Uji validasi:** `isTerminalStatus: true` bersama `isActiveEmployment: true` → **400**,
"Status terminal tidak boleh ditandai sebagai kepegawaian aktif."

---

## 5. Contract Type

**Tag Swagger:** Corporate / Human Resource / Master Data / **Contract Type**
**Prefix kode:** `CTT-RSMMC-00001`

```http
POST /api/v1/corporate/human-resource/master-data/contract-types
{
  "contractTypeName": "PKWT 12 Bulan",
  "defaultDurationMonths": 12,
  "isRenewable": true,
  "requiresEndDate": true,
  "isProbationApplicable": false
}
```

`defaultDurationMonths` boleh `null` (kontrak tanpa durasi baku), tapi kalau diisi harus
> 0 — kirim `0` atau negatif → **400**.

---

## 6. On Call Type

**Tag Swagger:** Corporate / Human Resource / Master Data / **On Call Type**
**Prefix kode:** `OCT-RSMMC-00001`

Satu-satunya master di GAP-2 yang **tidak punya `sortOrder`** — modelnya memang tidak
punya field itu.

```http
POST /api/v1/corporate/human-resource/master-data/on-call-types
{
  "onCallTypeName": "On Call Bedah",
  "responseTimeMinutes": 30,
  "minimumCallHours": 4,
  "maximumCallHours": 12,
  "isRemoteAllowed": false,
  "requiresOnSitePresence": true,
  "countsAsWorkingTime": true,
  "isAllowanceEligible": true
}
```

**Uji validasi** — keduanya harus **400**:

| Payload | Pesan yang diharapkan |
|---|---|
| `maximumCallHours` < `minimumCallHours` | "Maksimum jam panggilan tidak boleh lebih kecil…" |
| `isRemoteAllowed: true` **dan** `requiresOnSitePresence: true` | "…tidak boleh mengizinkan remote sekaligus mewajibkan kehadiran di tempat." |

**Uji penjagaan delete:** on call type yang masih dipakai shift → `DELETE` harus **400**.

---

## Uji yang berlaku untuk keenamnya

### Filter tanggal

```http
GET …/{route}?startDate=2026-08-01&endDate=2026-08-12
GET …/{route}?customPeriod=thismonth
GET …/{route}                              ← tanpa parameter, harus mengembalikan semua
```

`customPeriod` diabaikan kalau `startDate`/`endDate` terisi. Kolom yang difilter
`createDateTime`. `GET /summary` **sengaja tidak** ikut difilter — angkanya tetap total
keseluruhan.

### `sortOrder` tidak boleh tereset

Berlaku untuk lima master (semua kecuali On Call Type):

```http
PUT …/{route}/{id}    body TANPA field "sortOrder"
→ sortOrder entity harus TETAP, bukan berubah jadi 0
```

Ini penyimpangan sengaja dari beberapa controller lama yang menimpa `SortOrder` tanpa
syarat. Frontend tidak merender field itu, jadi menimpanya akan mereset urutan tiap kali
user menyimpan form.

### Kolom "Dibuat Oleh"

`createByName` dan `updateByName` harus berisi nama user, bukan `null`, kalau request
dikirim dengan token yang user-nya ada di tabel `Users`. `null` tetap sah dan frontend
menampilkannya sebagai `-`.

---

## Ringkasan perubahan

| Master | Route | Relasi | Penjagaan delete | `sortOrder` |
|---|---|---|---|---|
| Workforce Type | `workforce-types` | — | ✅ employee category | ✅ |
| Employee Category | `employee-categories` | `workforceTypeId` | — | ✅ |
| Employment Type | `employment-types` | — | — | ✅ |
| Employment Status | `employment-statuses` | — | — | ✅ |
| Contract Type | `contract-types` | — | — | ✅ |
| On Call Type | `on-call-types` | — | ✅ shift | — (tidak ada di model) |

## Dampak ke frontend

Tidak ada yang wajib. Belum ada halaman frontend untuk keenam master ini.

Yang jadi mungkin setelah ini: daftarkan resource di
`src/lib/hooks/select/hr/hr-select-resources.js`, lalu ubah `employeeCategoryId`,
`employmentTypeId`, `workforceTypeId`, `employmentStatusId`, `contractTypeId`, dan
`onCallTypeId` dari input teks UUID menjadi `type: "select"` + `optionResource`.

## Status verifikasi

| Pemeriksaan | Hasil |
|---|---|
| `dotnet build --no-incremental` | **Sukses — 125 Warning, 0 Error** (sama persis dengan baseline) |
| Warning baru dari 12 berkas baru | **Tidak ada** |
| **Uji Swagger** | **Belum dijalankan** |
| **Penyesuaian frontend** | **Belum dijalankan** — tidak wajib |
