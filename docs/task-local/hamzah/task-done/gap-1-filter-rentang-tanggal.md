# GAP-1 — Filter rentang tanggal — ✅ SELESAI

| | |
|---|---|
| Status | ✅ Selesai 2026-08-11 |
| Cakupan | **36 entitas** — 1 pilot (`benefit-type`) + 35 controller sisanya |
| Tugas register | T3, T4, T5, T6, T7, T8 |
| Migration | **Tidak ada** |
| Breaking change | **Tidak** — semua parameter opsional |
| Laporan teknis | `docs/hamzah/report/hr-master-data-date-filter.md`, `docs/hamzah/report/benefit-type-date-filter.md` |
| Register asal | `docs/hamzah/task/hr-master-data-frontend-gaps.md` |

---

## Apa yang sekarang bisa dilakukan

Setiap halaman list master data HR sekarang bisa disaring berdasarkan **tanggal dibuat**
(`CreateDateTime`). Sebelumnya tiga kontrol filter tanggal di frontend tampil aktif tapi
tidak mengubah hasil sama sekali.

## Tiga parameter baru — sama persis di 36 entitas

| Parameter | Tipe | Contoh nilai | Perilaku |
|---|---|---|---|
| `startDate` | `DateTime?` | `2026-08-01` | Batas awal **inklusif**, dinormalkan ke awal hari UTC |
| `endDate` | `DateTime?` | `2026-08-11` | Batas akhir **inklusif** — di belakang layar `endDate + 1 hari`, jadi data tanggal 11 ikut terhitung |
| `customPeriod` | `string?` | `today` | Pilihan: `today`, `last7days`, `thismonth`, `lastmonth` |

**Aturan penting saat menguji:**

- Ketiganya kosong → hasil **sama persis** seperti sebelum perubahan.
- `customPeriod` **diabaikan** kalau `startDate` atau `endDate` terisi.
- `customPeriod` bernilai asing (mis. `bulanlalu`) → diperlakukan seperti kosong, **bukan error**.
- `GET /summary` **tidak** ikut difilter — angka kartu ringkasan tetap total keseluruhan.
- `GET /options` **tidak** ikut difilter.

---

## Cara menguji di Swagger

Buka tag Swagger-nya, lalu pakai **tiga operasi** ini:

| Operasi | Yang dilihat |
|---|---|
| `GET /filters/metadata` | Harus muncul `customPeriods` berisi **4 opsi**, dan `defaultFilter` memuat `startDate`, `endDate`, `customPeriod` (nilainya `null`) |
| `GET` (list, tanpa route tambahan) | Tiga kolom isian baru: **startDate**, **endDate**, **customPeriod** |
| `GET /summary` | Angkanya **tidak boleh berubah** walau list menyusut |

### Urutan uji yang disarankan (contoh `job-families`)

```http
# 1. Baseline — kosongkan semua, catat totalData
GET /api/v1/corporate/human-resource/master-data/job-families

# 2. Metadata — pastikan customPeriods berisi 4 opsi
GET /api/v1/corporate/human-resource/master-data/job-families/filters/metadata

# 3. Rentang eksplisit — data tanggal 11 harus IKUT terhitung
GET /api/v1/corporate/human-resource/master-data/job-families?startDate=2026-08-01&endDate=2026-08-11

# 4. Periode preset
GET /api/v1/corporate/human-resource/master-data/job-families?customPeriod=thismonth

# 5. customPeriod diabaikan kalau ada startDate
#    hasilnya harus SAMA dengan nomor 3, bukan seperti "today"
GET /api/v1/corporate/human-resource/master-data/job-families?startDate=2026-08-01&endDate=2026-08-11&customPeriod=today

# 6. Ringkasan TIDAK ikut menyusut
GET /api/v1/corporate/human-resource/master-data/job-families/summary
```

Contoh isi `customPeriods` yang benar pada `filters/metadata`:

```json
"customPeriods": [
  { "value": "today",     "label": "Hari ini" },
  { "value": "last7days", "label": "7 hari terakhir" },
  { "value": "thismonth", "label": "Bulan ini" },
  { "value": "lastmonth", "label": "Bulan lalu" }
]
```

---

## Daftar endpoint — 36 entitas

Tag Swagger ditulis apa adanya supaya tinggal dicari di halaman Swagger.
Semua route diawali `api/v1/`, dan tiga parameter baru ada di operasi **`GET`** (list).

### Organization — 8

| Tag Swagger | Route list |
|---|---|
| Corporate / Human Resource / Master Data / **Cost Center** | `corporate/human-resource/master-data/cost-centers` |
| Corporate / Human Resource / Master Data / **Employee Grade** | `corporate/human-resource/master-data/employee-grades` |
| Corporate / Human Resource / Master Data / **Hospital Site** | `corporate/human-resource/master-data/hospital-sites` |
| Corporate / Human Resource / Master Data / **Job Family** | `corporate/human-resource/master-data/job-families` |
| Corporate / Human Resource / Master Data / **Job Level** | `corporate/human-resource/master-data/job-levels` |
| Corporate / Human Resource / Master Data / **Legal Entity** | `corporate/human-resource/master-data/legal-entities` |
| Corporate / Human Resource / Master Data / **Organization Unit** | `corporate/human-resource/master-data/organization-units` |
| Corporate / Human Resource / Master Data / **Work Location** | `corporate/human-resource/master-data/work-locations` |

### PayrollAndBenefit — 7 (termasuk pilot)

| Tag Swagger | Route list |
|---|---|
| Corporate / Human Resource / Master Data / **Benefit Type** 🏁 | `corporate/human-resource/master-data/benefit-types` |
| Corporate / Human Resource / Master Data / **Benefit Eligibility Rule** | `corporate/human-resource/master-data/benefit-eligibility-rules` |
| Corporate / Human Resource / Master Data / **Benefit Plan** | `corporate/human-resource/master-data/benefit-plans` |
| Corporate / Human Resource / Master Data / **Deduction Type** | `corporate/human-resource/master-data/deduction-types` |
| Corporate / Human Resource / Master Data / **Hazard Allowance Policy** | `corporate/human-resource/master-data/hazard-allowance-policies` |
| Corporate / Human Resource / Master Data / **On Call Allowance Policy** | `corporate/human-resource/master-data/on-call-allowance-policies` |
| Corporate / Human Resource / Master Data / **Shift Allowance Policy** | `corporate/human-resource/master-data/shift-allowance-policies` |

> 🏁 `benefit-type` adalah pilot yang selesai lebih dulu (commit `8ece36a`, sudah di `origin/MHamzah`).

### LeaveAndOvertime — 7

| Tag Swagger | Route list |
|---|---|
| Corporate / Human Resource / Master Data / Leave and Overtime / **Leave Type** | `corporate/human-resource/master-data/leave-types` |
| Corporate / Human Resource / Master Data / Leave and Overtime / **Leave Policy** | `corporate/human-resource/master-data/leave-policies` |
| Corporate / Human Resource / Master Data / Leave and Overtime / **Leave Entitlement Policy** | `corporate/human-resource/master-data/leave-entitlement-policies` |
| Corporate / Human Resource / Master Data / Leave and Overtime / **Leave Carry Forward Policy** | `corporate/human-resource/master-data/leave-carry-forward-policies` |
| Corporate / Human Resource / Master Data / Leave and Overtime / **Leave Adjustment Reason** ⚠️ | `corporate/human-resource/master-data/leave-adjustment-reasons` |
| Corporate / Human Resource / Master Data / Leave and Overtime / **Overtime Policy** | `corporate/human-resource/master-data/overtime-policies` |
| Corporate / Human Resource / Master Data / Leave and Overtime / **Overtime Rate** | `corporate/human-resource/master-data/overtime-rates` |

> ⚠️ `leave-adjustment-reasons` satu-satunya yang logika query-nya ada di
> `LeaveAdjustmentReasonService`, bukan di controller. **Layak diuji terpisah** karena
> jalur kodenya berbeda dari 35 lainnya.

### AttendanceAndSchedule — 5

| Tag Swagger | Route list |
|---|---|
| Corporate / Human Resource / Master Data / **Shift** | `corporate/human-resource/master-data/shifts` |
| Corporate / Human Resource / Master Data / **ShiftGroup** | `corporate/human-resource/master-data/shiftgroups` |
| Corporate / Human Resource / Master Data / **ShiftPattern** | `corporate/human-resource/master-data/shiftpatterns` |
| Corporate / Human Resource / Master Data / **WorkCalendar** | `corporate/human-resource/master-data/workcalendars` |
| Corporate / Human Resource / Master Data / **WorkSchedule** | `corporate/human-resource/master-data/workschedules` |

> Perhatikan: route lima entitas ini **tanpa tanda hubung** (`shiftgroups`, bukan
> `shift-groups`). Itu memang bentuk aslinya, bukan salah ketik.

### Performance — 5

| Tag Swagger | Route list |
|---|---|
| Corporate / Human Resource / Master Data / **KPI Catalog** | `corporate/human-resource/master-data/kpi-catalogs` |
| Corporate / Human Resource / Master Data / **Performance Cycle** | `corporate/human-resource/master-data/performance-cycles` |
| Corporate / Human Resource / Master Data / **Performance Rating Scale** | `corporate/human-resource/master-data/performance-rating-scales` |
| Corporate / Human Resource / Master Data / **Performance Template** | `corporate/human-resource/master-data/performance-templates` |
| Corporate / Human Resource / Master Data / **Performance Template Detail** ⚠️ | `corporate/human-resource/master-data/performance-templates/{performanceTemplateId}/details` |

> ⚠️ `performance-template-detail` butuh `performanceTemplateId` di **route**, jadi isi dulu
> id template yang valid sebelum menguji filter tanggalnya. Belum punya halaman frontend.
>
> Catatan `performance-cycles`: entitas ini punya kolom bisnisnya sendiri
> (`periodStartDate` / `periodEndDate`). `startDate` dan `endDate` yang baru **bukan** itu —
> keduanya menyaring **tanggal dibuat**.

### EmployeeRelation — 4

| Tag Swagger | Route list |
|---|---|
| Corporate / Human Resource / Master Data / **Violation Type** | `corporate/human-resource/master-data/violationtypes` |
| Corporate / Human Resource / Master Data / **Sanction Type** | `corporate/human-resource/master-data/sanctiontypes` |
| Corporate / Human Resource / Master Data / **Disciplinary Action Type** | `corporate/human-resource/master-data/actiontypes` |
| Corporate / Human Resource / Master Data / **Employee Relation Case Type** | `corporate/human-resource/master-data/casetypes` |

> Route empat entitas ini juga **tanpa tanda hubung**, dan dua di antaranya tidak memakai
> nama entitasnya (`actiontypes`, `casetypes`).

---

## Yang sengaja TIDAK berubah

| Hal | Alasan |
|---|---|
| `GET /summary` di semua entitas | Kartu ringkasan harus tetap menampilkan total keseluruhan |
| `GET /options` di semua entitas | Select relasi tidak perlu filter tanggal |
| `doctors`, `employees`, `external-users` | Sudah punya filter tanggal, tapi nama parameternya `period` bukan `customPeriod`. Penyeragaman jadi tugas terpisah (T10) |
| Lokasi 4 controller EmployeeRelation | Masih di `Repositories/Configurations/`, bukan `Areas/`. Pemindahan jadi tugas terpisah (T11) |

## Status verifikasi

| Pemeriksaan | Hasil |
|---|---|
| `dotnet build --no-incremental` | **125 warning, 0 error** — sama dengan baseline repo |
| Warning baru dari berkas yang disentuh | Tidak ada |
| Penempatan filter | Diperiksa skrip: tepat 1× per controller, seluruhnya di action list — tidak ada yang nyasar ke `/options` atau `/summary` |
| Kelengkapan DTO | Diperiksa skrip: 35/35 memuat `StartDate`, `EndDate`, `CustomPeriod`, `CustomPeriods` |
| **Uji Swagger** | **Belum dijalankan** — inilah yang ditunggu dari dokumen ini |
