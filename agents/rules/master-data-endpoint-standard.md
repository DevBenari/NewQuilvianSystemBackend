# Standar Endpoint Master Data

| Field | Nilai |
| --- | --- |
| Status | Baseline wajib untuk setiap capability master data |
| Sumber asal | `NewQuilvianSystemBackend/docs/update-skilss/Standar-Endpoint-Master-Data.pdf` |
| Diverifikasi terhadap | 34 controller pada `Areas/HealthServices/MasterData/Controllers/` |
| Presedensi | `AGENTS.md` > `agents/rules/engineering/` > `agents/rules/` > dokumen ini. Bila source repository berbeda, source yang berlaku dan selisihnya dilaporkan |

Dokumen ini mengunci **bentuk permukaan endpoint** untuk master data. Ia tidak mengubah gate
approval, batas kewenangan tulis, maupun pemisahan wewenang migration/database pada `SKILL.md`.

---

## 1. Surface baseline yang wajib ada

Setiap capability master data baru mengekspos sembilan endpoint berikut, dengan urutan
deklarasi yang sama di dalam controller. Base URL memakai route bertversi milik area/domain
pemiliknya, contoh `api/v1/health-services/master-data/service-units`.

| No | Method | Path | Kegunaan | Dipakai frontend untuk |
| ---: | --- | --- | --- | --- |
| 1 | `GET` | `/filters/metadata` | Mengambil konfigurasi filter, sort, enum, pagination, dan metadata form | Merender halaman index dan form secara konsisten |
| 2 | `GET` | `/summary` | Mengambil ringkasan jumlah data | Card statistik di halaman index |
| 3 | `GET` | `/` | Mengambil list data dengan filter, search, sort, dan pagination | Table utama master data |
| 4 | `GET` | `/options` | Mengambil data ringan untuk dropdown atau select | Dropdown di form lain |
| 5 | `GET` | `/{id}` | Mengambil detail satu data | Halaman detail dan pengisian form update |
| 6 | `POST` | `/` | Membuat data baru | Form create |
| 7 | `PUT` | `/{id}` | Mengubah seluruh field bisnis satu data | Simpan perubahan |
| 8 | `PATCH` | `/{id}/status` | Mengubah status aktif/nonaktif saja | Toggle aktif di table dan detail |
| 9 | `DELETE` | `/{id}` | Menandai data terhapus tanpa menghapus fisik | Tombol hapus |

Delapan endpoint pertama berasal dari PDF standar. `PATCH /{id}/status` ditambahkan karena
sudah menjadi pola nyata pada 26 dari 34 controller master data — PDF menyebutnya sebagai
"endpoint tambahan", tetapi source memperlakukannya sebagai bagian baseline. Ikuti source.

Aturan yang tidak boleh dilanggar:

- Bungkus semua response dengan `ApiResponse<T>`. List memakai `PagedResult<T>`.
- Pertahankan `[Authorize]`, `[AccessController]`, `[AccessAction]`, dan `[AccessPermission]`
  pada setiap action, mengikuti domain pemiliknya.
- Constraint route `{id:guid}` dipakai bila identifier-nya `Guid`.
- `GET /` dan `GET /options` adalah dua endpoint berbeda dengan tujuan berbeda. Jangan
  memakai list utama sebagai sumber dropdown, dan jangan memakai `/options` sebagai table.

Bukti rujukan bentuk kontrak: `Areas/HealthServices/MasterData/Controllers/ServiceUnitController.cs`
dan `Areas/HealthServices/MasterData/DTOs/ServiceUnitDtos.cs`.

---

## 2. Kontrak per endpoint

### 2.1 `GET /filters/metadata`

Endpoint ini dipanggil paling awal saat frontend membuka halaman master data. Ia tidak
mengembalikan data utama, melainkan konfigurasi halaman.

Field yang wajib ada pada response:

| Field | Isi |
| --- | --- |
| `DefaultFilter` | Nilai awal seluruh filter, termasuk `SortBy`, `SortDirection`, `PageNumber`, `PageSize` |
| `CustomPeriods` | Pilihan periode siap pakai beserta `UsesStartDate` dan `UsesEndDate` |
| `SortOptions` | Pasangan `Value` dan `Label` untuk setiap kolom yang boleh diurutkan |
| `SortDirections` | `asc` dan `desc` |
| `PageSizeOptions` | Contoh pada Service Unit: `10, 25, 50, 100` |
| `<Enum>Options` | Satu list per enum yang dipakai filter, contoh `ServiceUnitTypeOptions` |
| `QueryParameters` | Daftar query parameter beserta `Type`, `Required`, `Description`, `Example` |
| `CreateFields` | Metadata field form create |
| `UpdateFields` | Metadata field form update |
| `DateFormat` | Format tanggal yang dipakai frontend, contoh `yyyy-MM-dd` |
| `ResetButtonLabel` | Label tombol reset filter |

Setiap entri `CreateFields` dan `UpdateFields` memuat `Name`, `Label`, `Section`, `InputType`,
`IsRequiredOnCreate`, `IsRequiredOnUpdate`, `RequiredType`, `MaxLength`, `OptionsSource`,
`Description`, `Example`, dan `SortOrder`.

Contoh:

```http
GET /api/v1/health-services/master-data/service-units/filters/metadata
```

Field yang dideklarasikan wajib benar-benar didukung oleh `GET /`. Metadata yang menjanjikan
filter yang tidak diproses list adalah cacat kontrak, bukan sekadar dokumentasi yang usang.

### 2.2 `GET /summary`

Mengembalikan angka ringkasan yang dihitung dari data yang belum ditandai terhapus.

Pola isinya sama untuk semua master data, tetapi nama field menyesuaikan entity:

| Entity | Field ringkasan |
| --- | --- |
| Service Unit | `TotalServiceUnit`, `ActiveServiceUnit`, `InactiveServiceUnit`, `RegistrationAvailableServiceUnit`, `KioskAvailableServiceUnit`, `AppointmentAvailableServiceUnit`, `QueueRequiredServiceUnit`, `DoctorRequiredServiceUnit`, `ScreeningRequiredServiceUnit` |
| Clinic | `TotalClinic`, `ActiveClinic`, `InactiveClinic`, `QueueRequiredClinic`, `DoctorRequiredClinic` |
| Drug | `TotalDrug`, `ActiveDrug`, `InactiveDrug`, `LowStockPolicyDrug`, `PrescriptionOnlyDrug` |

Aturan minimum: selalu ada total, jumlah aktif, dan jumlah nonaktif. Sisanya adalah pencacah
per flag operasional yang memang dimiliki entity itu.

Contoh tampilan card di halaman index:

> Total Service Unit: 12 — Aktif: 10 — Nonaktif: 2 — Tersedia Registrasi: 8 — Tersedia Kiosk: 4

### 2.3 `GET /` — list untuk table utama

Query parameter baseline:

| Kelompok | Parameter |
| --- | --- |
| Rentang tanggal | `startDate`, `endDate`, `customPeriod` |
| Pencarian | `search` |
| Status | `isActive` |
| Enum dan flag | Satu parameter per enum dan per flag bisnis entity |
| Pengurutan | `sortBy` (default kolom urutan bisnis), `sortDirection` (default `asc`) |
| Pagination | `pageNumber` (default `1`), `pageSize` (default `25`) |

Controller wajib menormalkan paging, memvalidasi rentang tanggal, menerapkan search ke
beberapa field yang relevan, lalu mengembalikan `PagedResult<T>`.

Contoh:

```http
GET /api/v1/health-services/master-data/service-units
    ?search=rawat&isActive=true&serviceUnitType=1
    &sortBy=sortOrder&sortDirection=asc&pageNumber=1&pageSize=25
```

Bentuk response:

```json
{
  "data": {
    "pageNumber": 1,
    "pageSize": 25,
    "totalData": 100,
    "totalPage": 4,
    "items": []
  }
}
```

`totalPage` dihitung backend. Frontend tidak boleh menghitung ulang dari panjang `items`.

Bila rentang tanggal tidak valid, kembalikan `400` dengan pesan yang menjelaskan sebabnya,
bukan list kosong.

### 2.4 `GET /options`

Feed ringan untuk dropdown, lookup, select, autocomplete, dan relasi antar master data.

- `onlyActive` bernilai `true` secara default, sehingga pemanggil hanya menerima data aktif.
- Menerima `search` dan filter yang relevan untuk mempersempit pilihan.
- Tetap ber-pagination, tetapi payload per item jauh lebih ringan daripada list utama.
- Urutkan mengikuti urutan bisnis, lalu nama.

Contoh dropdown unit yang melayani registrasi:

```http
GET /api/v1/health-services/master-data/service-units/options
    ?onlyActive=true&isAvailableForRegistration=true
```

### 2.5 `GET /{id}`

Mengembalikan detail satu data, lebih lengkap daripada satu baris list — termasuk field
naratif seperti `Description` dan jejak audit siapa membuat dan siapa mengubah.

Bila data tidak ada atau sudah ditandai terhapus, kembalikan `404` dengan pesan yang bisa
ditampilkan apa adanya, contoh "Data tidak ditemukan atau sudah dihapus."

### 2.6 `POST /`

Membuat data baru. Kode bisnis tidak dikirim frontend; backend yang mengalokasikannya dengan
pola dan awalan milik modul, contoh `SU-RSMMC-00001`.

Response create mengembalikan minimal `Id`, kode bisnis hasil generate, nama, tipe, dan
`IsActive`, supaya frontend bisa menampilkan toast sukses, mengarahkan ke detail, menyegarkan
table, dan memperlihatkan kode yang baru terbentuk.

Catatan UX untuk frontend: tampilkan kode sebagai kolom hanya-baca bertuliskan
"Dibuat otomatis oleh sistem", bukan sebagai isian wajib.

Alokasi kode wajib deterministik dan aman di level database. Lihat batasan pada bagian 5.

### 2.7 `PUT /{id}`

Update penuh terhadap field bisnis. Cari data berdasarkan `Id` dan pastikan belum ditandai
terhapus, lalu perbarui seluruh field yang boleh diubah beserta jejak audit.

`PUT` diperlakukan sebagai full update, bukan partial update. Perubahan satu aspek saja
memakai endpoint `PATCH` khusus, bukan `PUT` dengan sebagian field.

Alur frontend: ambil data lewat `GET /{id}` → isi form → user mengubah → submit `PUT /{id}` →
arahkan ke detail atau index.

### 2.8 `PATCH /{id}/status`

Mengubah status aktif tanpa mengirim seluruh body update. Request-nya minimal:

```json
{ "isActive": false }
```

Response memakai bentuk yang sama dengan response update, sehingga frontend bisa langsung
memperbarui baris table tanpa memanggil ulang list.

### 2.9 `DELETE /{id}`

Selalu soft delete, tidak pernah hard delete. Yang terjadi pada data:

| Field | Nilai setelah hapus |
| --- | --- |
| `IsDelete` | `true` |
| `IsActive` | `false` |
| `DeleteDateTime` | Waktu UTC saat penghapusan |
| `DeleteBy` | Pengguna yang sedang login |

Sebelum menandai terhapus, periksa seluruh relasi pemakainya. Contoh nyata pada Service Unit:
penghapusan ditolak bila unit sudah dipakai Clinic, Room, atau jadwal dokter.

Kode status yang wajib ditangani frontend:

| Kode | Arti bagi pengguna |
| --- | --- |
| `200` | Data berhasil dihapus |
| `400` | Data tidak bisa dihapus karena masih dipakai data lain |
| `404` | Data tidak ditemukan atau sudah dihapus sebelumnya |
| `401` | Pengguna belum login |
| `403` | Pengguna tidak punya hak akses untuk tindakan ini |

Data master yang sudah dipakai transaksi atau master lain tidak boleh dihapus begitu saja.

---

## 3. Endpoint tambahan di luar baseline

Baseline sembilan endpoint adalah lantai, bukan langit-langit. Bila sebuah capability
membutuhkan endpoint lain agar benar-benar berfungsi, **buat endpoint itu di slice yang sama**.
Jangan menunda, jangan menyerahkannya ke task lain, dan jangan memaksakan kebutuhan itu masuk
ke `PUT /{id}`.

### 3.1 Kapan endpoint tambahan wajib dibuat

Buat langsung, tanpa menunggu putaran approval baru, bila salah satu terpenuhi:

1. Kontrak to-be yang disetujui atau acceptance criteria task menyebutkan endpoint itu.
2. Ada aspek data yang berubah sendiri di luar siklus edit form penuh — misalnya ketersediaan,
   status tinjauan, atau penugasan.
3. Konsumen yang berbeda membutuhkan bentuk data yang berbeda dari endpoint yang sama —
   misalnya layar admin dan layar kiosk.
4. Ada relasi atau sub-data yang punya form sendiri dan tidak wajar dikirim menyatu di `PUT`.

Yang tetap harus dihentikan dan ditanyakan lebih dulu: endpoint yang mengubah aturan bisnis,
kewenangan, atau alur persetujuan yang belum diputuskan. Menambah permukaan teknis boleh;
mengarang kebijakan tidak.

Setiap endpoint tambahan dicatat pada laporan task tracked sebagai delta terhadap kontrak,
lengkap dengan alasannya.

### 3.2 Katalog pola yang sudah ada di source

Pakai pola yang sudah terbukti, jangan menciptakan tata bahasa route baru.

| Pola | Bentuk | Kapan dipakai | Contoh nyata |
| --- | --- | --- | --- |
| Ubah satu aspek | `PATCH /{id}/<aspek>` | Satu bagian data berubah sendiri di luar form penuh | `PATCH /{id}/availability` pada Bed; `PATCH /{id}/review-status` pada tiga controller Diagnosis Recommendation |
| Perintah eksplisit | `PATCH /{id}/activate` dan `PATCH /{id}/deactivate` | Kontrak yang disetujui memang menuntut dua perintah terpisah tanpa body | `InsuranceCoverageRule`, `DoctorSchedule` |
| Permukaan per audiens | `<audiens>/…` di depan seluruh sub-surface | Satu entity dilayani ke beberapa jenis pengguna dengan payload dan hak akses berbeda | `admin/…` dan `kiosk/…` pada Clinic dan DoctorSchedule |
| Feed pilihan khusus | `GET /<konteks>-options` | Dropdown dengan aturan penyaringan yang tidak cukup diwakili `/options` | `GET /clinical-options` dan `GET /icd9-procedure-options` pada Diagnosis |
| Sub-resource | `GET /{id}/<sub>` dan `PUT /{id}/<sub>` | Sekelompok field punya form dan hak akses sendiri | `GET` dan `PUT /{id}/clinical-information` pada Drug |

Catatan penting soal `activate`/`deactivate`: `PATCH /{id}/status` tetap bentuk baku. Pasangan
`activate` dan `deactivate` hanya ditambahkan ketika kontrak yang disetujui memintanya. Bila
keduanya ada, keduanya wajib menghasilkan perubahan status yang identik — jangan sampai dua
jalur berbeda menghasilkan perilaku berbeda.

### 3.3 Syarat bentuk untuk endpoint tambahan

Endpoint tambahan tunduk pada aturan yang sama dengan baseline:

- Route bertversi dan berada di bawah base URL entity yang sama.
- `ApiResponse<T>` untuk sukses maupun gagal.
- Punya `[AccessAction]` dan `[AccessPermission]` sendiri yang sesuai jenis tindakannya.
- Punya DTO request dan response sendiri di folder `DTOs/` domain pemiliknya. Contoh:
  `UpdateBedAvailabilityRequest` memuat `BedStatus` dan `Description`, bukan seluruh field Bed.
- Menghormati soft delete: data yang sudah ditandai terhapus tidak boleh ikut berubah.
- Mencatat jejak audit `UpdateDateTime` dan `UpdateBy` seperti endpoint update lainnya.
- Terdaftar di `/filters/metadata` bila ia menambah filter, atau di dokumentasi endpoint modul
  bila ia menambah aksi.

---

## 4. Varian yang sah dan bentuk yang tidak boleh ditiru

| Bentuk | Status | Keterangan |
| --- | --- | --- |
| Sembilan endpoint baseline | Wajib | Berlaku untuk setiap master data baru |
| Master data pengaturan tunggal | Varian sah | Entity yang hanya punya satu baris konfigurasi cukup `GET /` dan `PUT /{id}`. Contoh: `InpatientSettingController`. Metadata, summary, options, dan delete tidak berlaku karena tidak ada koleksi data |
| Controller enam endpoint tanpa metadata, summary, dan patch status | Legacy | Enam controller `Emergency*` masih memakai bentuk lama. Jangan dijadikan contoh untuk master data baru. Menyentuhnya berarti `TOUCHED LEGACY`, bukan izin menyalin bentuknya |
| Master data koleksi tanpa `/filters/metadata`, `/summary`, atau `/options` | Belum lengkap | Contoh: `InpatientClearanceItemController`. Perlakukan sebagai kekurangan yang dilaporkan, bukan sebagai preseden |

---

## 5. Yang tidak boleh disalin dari implementasi rujukan

`ServiceUnitController` adalah rujukan otoritatif untuk **bentuk kontrak endpoint**, bukan
untuk arsitektur di dalamnya. Untuk `NEW CODE`, aturan QBE pada
`agents/rules/engineering/BACKEND_ENGINEERING_CONTRACT.md` mengalahkan pola legacy yang bertentangan:

| Pola yang terlihat di rujukan | Aturan yang berlaku untuk kode baru |
| --- | --- |
| Controller memakai `ApplicationDbContext` langsung | QBE-SVC-001 — CRUD dan orkestrasi domain dimiliki Module Service |
| Controller membentuk kode bisnis sendiri | QBE-CODE-002 — controller tidak mengalokasikan nomor bisnis |
| Alokasi kode bergaya `Count`, `Max`, atau `Last + 1` | QBE-CODE-003 — alokasi wajib atomik dan aman di database |
| `SortOrder` generik yang dipersistensi | Urutan bisnis nyata memakai field semantik. `SortOrder` pada DTO, form, permission, dan UI tetap sah |
| Entity EF dikembalikan langsung | QBE-DTO-001 — kontrak API selalu lewat DTO |

QBE-OPT-001 menyatakan metadata dan options disediakan hanya bila dikonsumsi. Untuk master data
yang punya halaman index dan form, `/filters/metadata` dan `/options` memang dikonsumsi, jadi
keduanya tetap wajib. Untuk entity yang tidak punya halaman index, lihat varian pada bagian 4.

---

## 6. Checklist sebelum task master data dianggap selesai

1. Sembilan endpoint baseline ada, kecuali varian yang sudah dibenarkan pada bagian 4 dan
   dicatat alasannya.
2. Setiap endpoint punya `[AccessAction]` dan `[AccessPermission]` yang benar.
3. Semua response terbungkus `ApiResponse<T>`; list memakai `PagedResult<T>` dengan
   `pageNumber`, `pageSize`, `totalData`, `totalPage`, dan `items`.
4. Filter, sort, dan page size yang diumumkan `/filters/metadata` benar-benar didukung `GET /`.
5. Field `/summary` sudah menyesuaikan entity dan dihitung hanya dari data yang belum terhapus.
6. `/options` mengembalikan data aktif secara default dan payload-nya lebih ringan dari list.
7. Kode bisnis dibuat backend, dan frontend tidak diminta mengisinya.
8. `PUT` adalah full update; perubahan satu aspek punya endpoint `PATCH` sendiri.
9. `DELETE` adalah soft delete, memeriksa seluruh relasi pemakai, dan mengembalikan `400`
   dengan pesan yang jelas ketika data masih dipakai.
10. Endpoint tambahan yang dibuat sudah dicatat di laporan task tracked sebagai delta kontrak
    beserta alasannya.
