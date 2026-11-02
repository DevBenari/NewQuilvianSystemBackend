# Laporan Perubahan Backend — `BE-HRD-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-002` |
| Judul | Route canonical kebab-case dan compatibility alias pada 8 master data controller |
| Slice | `S0-B` (Fondasi Standarisasi Route Kebab-Case) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang 2 |
| Trace | `HRD-DEC-016`, `QBE-NAM` |
| Contract version | API Contract v1 (Draft) |
| Dependency | `BE-HRD-001` (Selesai ✅) |
| Klasifikasi | `LIGHT` |
| Task mode | `BACKEND` |
| Target tulis | 8 Controller Master Data HR |
| Model | Gemini 3.8 Flash (High) |
| Commit backend saat dikerjakan | `16f6b2ad28e0e9cbd374354abe240ff3b163ace0` |
| Tanggal | 27 Oktober 2026 |
| Status | Selesai (`PASS`) |

---

## 1. Masalah yang diperbaiki

Sebelumnya, delapan route master data HR menggunakan kata majemuk yang digabung tanpa tanda hubung (*non-hyphenated*): `workcalendars`, `workschedules`, `shiftgroups`, `shiftpatterns`, `actiontypes`, `casetypes`, `sanctiontypes`, dan `violationtypes`. Penamaan ini menyimpang dari konvensi URL baku sistem Quilvian yang menggunakan format *kebab-case* (misalnya `work-calendars`, `shift-groups`).

Namun, mematikan route lama secara langsung akan merusak integrasi frontend dan skrip eksisting yang sudah memanggil route lama (*breaking change*). Sesuai keputusan `HRD-DEC-016`, solusinya adalah menerapkan *compatibility alias*: controller melayani route canonical baru (*kebab-case*) berdampingan dengan route lama tanpa duplikasi controller maupun logic.

---

## 2. Proses bisnis

* **Tujuan**: Memastikan URL API modul HR konsisten menggunakan format kebab-case modern tanpa mengganggu operasional sistem yang sedang berjalan.
* **Pelaku**: Staf HR, Manajer Medis, serta modul internal lain yang mengonsumsi master data jam kerja dan hubungan karyawan.
* **Pemicu**: Standarisasi endpoint master data HR.
* **Langkah & Aturan**:
  1. Menambahkan atribut `[Route("...")]` dengan format kebab-case canonical di atas route lama pada 8 controller master data.
  2. Route lama tetap dipertahankan pada action dan controller yang sama persis sehingga kedua bentuk URL mengembalikan data yang identik.
  3. Memastikan tidak ada penggandaan file controller, service, maupun validasi.
* **Hasil Akhir**: Konsumen dapat memanggil `api/v1/corporate/human-resource/master-data/work-calendars` maupun `api/v1/corporate/human-resource/master-data/workcalendars` dengan hasil yang sama persis.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa
* `Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/Controllers/WorkCalendarController.cs`
* `Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/Controllers/WorkScheduleController.cs`
* `Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/Controllers/ShiftGroupController.cs`
* `Areas/Corporate/HumanResource/MasterData/AttendanceAndSchedule/Controllers/ShiftPatternController.cs`
* `Repositories/Configurations/Corporate/HumanResource/MasterData/EmployeeRelation/Controllers/DisciplinaryActionTypeController.cs`
* `Repositories/Configurations/Corporate/HumanResource/MasterData/EmployeeRelation/Controllers/EmployeeRelationCaseTypeController.cs`
* `Repositories/Configurations/Corporate/HumanResource/MasterData/EmployeeRelation/Controllers/SanctionTypeController.cs`
* `Repositories/Configurations/Corporate/HumanResource/MasterData/EmployeeRelation/Controllers/ViolationTypeController.cs`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `WorkCalendarController.cs` | Menambahkan atribut `[Route("api/v1/corporate/human-resource/master-data/work-calendars")]` di atas route legacy `workcalendars`. |
| `WorkScheduleController.cs` | Menambahkan atribut `[Route("api/v1/corporate/human-resource/master-data/work-schedules")]` di atas route legacy `workschedules`. |
| `ShiftGroupController.cs` | Menambahkan atribut `[Route("api/v1/corporate/human-resource/master-data/shift-groups")]` di atas route legacy `shiftgroups`. |
| `ShiftPatternController.cs` | Menambahkan atribut `[Route("api/v1/corporate/human-resource/master-data/shift-patterns")]` di atas route legacy `shiftpatterns`. |
| `DisciplinaryActionTypeController.cs` | Menambahkan atribut `[Route("api/v1/corporate/human-resource/master-data/action-types")]` di atas route legacy `actiontypes`. |
| `EmployeeRelationCaseTypeController.cs` | Menambahkan atribut `[Route("api/v1/corporate/human-resource/master-data/case-types")]` di atas route legacy `casetypes`. |
| `SanctionTypeController.cs` | Menambahkan atribut `[Route("api/v1/corporate/human-resource/master-data/sanction-types")]` di atas route legacy `sanctiontypes`. |
| `ViolationTypeController.cs` | Menambahkan atribut `[Route("api/v1/corporate/human-resource/master-data/violation-types")]` di atas route legacy `violationtypes`. |
| `backend-roadmap.md` | Pembaruan status task `BE-HRD-002` menjadi `✅` pada pohon dependensi, register, dan kartu task. |
| `requirement-traceability.md` | Pembaruan status penelusuran requirement `S0-B` menjadi `✅ Terbukti Penuh`. |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Penambahan rute kebab-case aditif pada 8 controller master data; tidak ada breaking change. |
| Database | `NOT APPLICABLE` — Tidak ada perubahan skema database atau migrasi EF Core. |
| Keamanan/Auth | `NOT APPLICABLE` — Atribut `[Authorize]`, `[AccessController]`, `[AccessAction]`, dan `[AccessPermission]` tetap utuh pada setiap method. |

---

## 4. Dokumentasi endpoint

#### `[Tags("Corporate / Human Resource / Master Data / WorkCalendar")]`
* Route Canonical: `api/v1/corporate/human-resource/master-data/work-calendars`
* Compatibility Alias: `api/v1/corporate/human-resource/master-data/workcalendars`

#### `[Tags("Corporate / Human Resource / Master Data / WorkSchedule")]`
* Route Canonical: `api/v1/corporate/human-resource/master-data/work-schedules`
* Compatibility Alias: `api/v1/corporate/human-resource/master-data/workschedules`

#### `[Tags("Corporate / Human Resource / Master Data / ShiftGroup")]`
* Route Canonical: `api/v1/corporate/human-resource/master-data/shift-groups`
* Compatibility Alias: `api/v1/corporate/human-resource/master-data/shiftgroups`

#### `[Tags("Corporate / Human Resource / Master Data / ShiftPattern")]`
* Route Canonical: `api/v1/corporate/human-resource/master-data/shift-patterns`
* Compatibility Alias: `api/v1/corporate/human-resource/master-data/shiftpatterns`

#### `[Tags("Corporate / Human Resource / Master Data / Disciplinary Action Type")]`
* Route Canonical: `api/v1/corporate/human-resource/master-data/action-types`
* Compatibility Alias: `api/v1/corporate/human-resource/master-data/actiontypes`

#### `[Tags("Corporate / Human Resource / Master Data / Employee Relation Case Type")]`
* Route Canonical: `api/v1/corporate/human-resource/master-data/case-types`
* Compatibility Alias: `api/v1/corporate/human-resource/master-data/casetypes`

#### `[Tags("Corporate / Human Resource / Master Data / Sanction Type")]`
* Route Canonical: `api/v1/corporate/human-resource/master-data/sanction-types`
* Compatibility Alias: `api/v1/corporate/human-resource/master-data/sanctiontypes`

#### `[Tags("Corporate / Human Resource / Master Data / Violation Type")]`
* Route Canonical: `api/v1/corporate/human-resource/master-data/violation-types`
* Compatibility Alias: `api/v1/corporate/human-resource/master-data/violationtypes`

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Pemeriksaan Route Template 8 Controller | Kedua atribut `[Route]` terpasang berdampingan | `PASS` | Kode sumber C# di 8 controller |
| Uji Kompatibilitas Non-Breaking | Route lama tetap aktif tanpa perubahan DTO/Service | `PASS` | `git diff` membuktikan hanya penambahan atribut route |
| Verifikasi Arsitektur Bersih | Nol duplikasi controller atau class baru | `PASS` | Seluruh rute dilayani controller yang sama |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | :---: | --- |
| 1. Route canonical kebab-case aktif berdampingan dengan route legacy | Terpenuhi | 8 controller memiliki kedua atribut `[Route]` |
| 2. Tidak ada controller, service, atau validasi baru yang menduplikasi kode | Terpenuhi | Menggunakan satu implementasi controller yang sama |
| 3. Tidak ada breaking change terhadap pemanggil eksisting | Terpenuhi | Rute lama tetap merespons request seperti biasa |
| 4. Frontend dapat bermigrasi ke rute kebab-case secara bertahap | Terpenuhi | Endpoint baru langsung dapat dikonsumsi |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Rute lama tidak boleh dihapus sebelum seluruh konsumen (frontend constants dan integrasi luar) selesai dimigrasikan ke rute kebab-case. |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | `NONE` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | 8 controller termodifikasi, 1 laporan tracked dibuat. |
| Langkah berikutnya | Melanjutkan ke Gelombang 3: `BE-HRD-003` (endpoint daftar lintas-pegawai Administrasi Kepegawaian / Workforce Core). |
