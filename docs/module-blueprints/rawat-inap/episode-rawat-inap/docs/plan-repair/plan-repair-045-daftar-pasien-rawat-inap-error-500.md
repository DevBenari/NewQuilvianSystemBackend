# PLAN-REPAIR-045 — Perbaikan Penyaringan Tanggal HTTP 500 pada Daftar Pasien Rawat Inap dan Layanan Rawat Inap Terkait

```yaml
plan_id: PLAN-REPAIR-EPS-045
issue: ../issue/issue-045-daftar-pasien-rawat-inap-error-500.md
status_rencana: DISETUJUI_IMPLEMENTASI_SELESAI_MENUNGGU_BUILD_MANDIRI
tanggal_rencana: "2026-11-25"
diputuskan_oleh: "Pemilik (USER)"
tanggal_keputusan: "2026-11-25"
basis_source_backend: "38b614eff7f27da3095a1c9b1cf31926e63d072f (MHamzah)"
basis_source_frontend: "32d67d384cf7f63ff1ad424a0206c53a15fdfcd8 (HamzahV2)"
```

---

## 1. Register status pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | --- | :---: | --- | --- | --- |
| `FIX-EPS-045-01` | `ISS-EPS-045-02` | Normalkan `StartDate` dan `EndDate` ke UTC (WIB `Asia/Jakarta`) via `InpDateRangeHelper` (`AppDateTimeHelper`) pada `InpEpisodeService.Reads.cs` | Backend | 1 | — | 🟡 IMPLEMENTASI SELESAI (MENUNGGU BUILD MANDIRI) | [InpDateRangeHelper.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Helpers/InpDateRangeHelper.cs), [InpEpisodeService.Reads.cs:422-436](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.Reads.cs#L422-L436) — 0 compiler error C# sebelum `MSB3027` file lock `PID 9064`; build penuh & uji runtime dilewati atas instruksi pemilik |
| `FIX-EPS-045-02` | `ISS-EPS-045-T1` | Normalkan parameter filter tanggal ke UTC via `InpDateRangeHelper` pada `InpCensusQueryService`, `InpDischargeService` (`Closure` & `Departure`), `InpIntegrationReplayService`, dan `InpMedicalRecordDocumentSource` | Backend | 2 | — | 🟡 IMPLEMENTASI SELESAI (MENUNGGU BUILD MANDIRI) | [InpCensusQueryService.cs:1226-1240](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpCensusQueryService.cs#L1226-L1240), [InpDischargeService.Closure.cs:599-612](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Closure.cs#L599-L612), [InpDischargeService.Departure.cs:69-80](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Departure.cs#L69-L80), [InpIntegrationReplayService.cs:19-31](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpIntegrationReplayService.cs#L19-L31), [InpMedicalRecordDocumentSource.cs:295-338](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpMedicalRecordDocumentSource.cs#L295-L338) |
| `FIX-EPS-045-03` | `ISS-EPS-045-T2` | Tambah batas `min`/`max` pada `FilterDatePicker` dan cegah baris lama tampil saat query daftar episode gagal | Frontend | 3 | — | 🟡 IMPLEMENTASI SELESAI (MENUNGGU BUILD MANDIRI) | [inpatient-episode-worklist-view.jsx:336-349](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/inpatient-episode-worklist-view.jsx#L336-L349), [use-inpatient-episode-worklist.jsx:253-254,519-520](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/QuilvianSystemFrontendDev/src/lib/hooks/health-services/inpatient-management/use-inpatient-episode-worklist.jsx#L253-L254); `check-css-usage` 0 temuan, `npm run lint:errors` PASS, `inpatient-episode-worklist.test.mjs` 23/23 PASS; `npm run build` dihentikan atas instruksi pemilik |

**Ringkasan: 3 dari 3 perbaikan sudah selesai diimplementasikan di source Backend dan Frontend setelah disetujui pemilik (`K-01=A`, `K-02=A`, `K-03=Ya`); build penuh (`dotnet build` & `npm run build`) serta verifikasi runtime pasca-build ditangguhkan untuk dikerjakan mandiri oleh pemilik sesuai instruksi (*"skip build, nanti saya build secara mandiri saja"*).**

---

## 2. Solusi terpilih per temuan

### ISS-EPS-045-02 & ISS-EPS-045-T1 — Kegagalan HTTP 500 akibat `DateTimeKind.Unspecified` dibandingkan dengan kolom PostgreSQL `timestamp with time zone`

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **A** | **Normalkan batas `StartDate` / `EndDate` (dan `From`/`To`/`ClosedFrom`/`ClosedTo`) menjadi UTC berbasis hari operasional WIB menggunakan `AppDateTimeHelper` (`Helpers/AppDateTimeHelper.cs`) di layer service** | Mengikuti konvensi resmi repository (`BbkBloodOrderService`, `LabDashboardService`, `OptReportService`, `OvertimeSelfServiceService`); tetap aman untuk pemanggil yang mengirim `YYYY-MM-DD` (tanpa offset) maupun ISO ber-`Z` / `+07:00`; tidak mengubah kontrak DTO atau database; cakupan waktu sesuai jam dinding rumah sakit (`Asia/Jakarta`) | Perlu impor `using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;` karena `AppDateTimeHelper` bersarang ganda |
| B | Menambahkan `DateTime.SpecifyKind(value, DateTimeKind.Utc)` langsung tanpa konversi zona waktu WIB | Sederhana (hanya mengubah `Kind`) | Menggeser batas hari sebesar 7 jam: admisi pukul `00:00–06:59 WIB` (mis. `2 Nov 2026 06:30 WIB` = `1 Nov 23:30 UTC`) akan terlempar ke hari sebelumnya |
| C | Mengubah `Program.cs` dengan mengaktifkan `AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true)` secara global | Menyembunyikan error di semua modul | Dilarang secara arsitektural karena memengaruhi perilaku timestamp seluruh modul rumah sakit di luar Rawat Inap dan bertentangan dengan standar Npgsql 9 di repository ini |
| D | Mengubah frontend untuk selalu mengirim ISO string berakhiran `Z` atau `+07:00` | Tidak menyentuh backend | Endpoint backend tetap rapuh bila dipanggil langsung dari Swagger, mobile, integrasi antar-modul, atau `InpatientEpisodeListQuery` kontrak `YYYY-MM-DD` |

**Solusi terpilih: Opsi A.**

1. Memakai kembali helper resmi repository `AppDateTimeHelper` (`OperationalDateToUtc` dan `ToUtc`) sebagaimana dipakai pada `BbkBloodOrderService.cs:207-210` dan `LabQueryDateRange.cs:54-71`, tanpa memperkenalkan helper baru yang duplikatif.
2. Menutup bug di sumber kebenaran backend sehingga pemanggilan langsung via Swagger, Postman, frontend web, maupun consumer lain sama-sama aman dan tidak pernah melempar `ArgumentException` Npgsql.
3. Menjamin ketepatan batas hari operasional rumah sakit (`Asia/Jakarta`, UTC+7): tanggal `2026-11-02` menyaring rentang `>= 2026-11-01T17:00:00Z` dan untuk tanggal akhir `2026-11-02` menyaring `< 2026-11-02T17:00:00Z` (inklusif sampai `23:59:59.999 WIB`).

**Kenapa bukan opsi lain.**  
Opsi B menggeser batas hari 7 jam dan menjatuhkan pasien yang masuk dini hari (`00:00–06:59 WIB`); Opsi C berisiko regresi global di seluruh modul; Opsi D hanya menambal satu pemanggil frontend dan membiarkan kontrak backend `YYYY-MM-DD` tetap meledak `500`.

### ISS-EPS-045-T2 — Pembersihan tampilan usang saat gagal & pembatasan silang pemilih tanggal (Frontend)

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **A** | **Tambahkan prop `max={filters.endDate}` / `min={filters.startDate}` pada `FilterDatePicker` di `inpatient-episode-worklist-view.jsx`, dan kosongkan daftar atau tampilkan status error secara konsisten saat request gagal** | Memanfaatkan fitur bawaan `FilterDatePicker` (`filter-date-picker.jsx:420-423`) tanpa mengubah base component; mencegah tampilan daftar lama menyesatkan petugas ketika filter baru gagal | Menyentuh repository frontend secara terpisah |
| B | Biarkan apa adanya (hanya perbaiki backend) | Nol risiko regresi frontend | Jika jaringan/server gagal pada request lain, baris lama tetap tertinggal di bawah banner merah |

**Solusi terpilih: Opsi A (dengan persetujuan pemilik K-03).**

---

## 3. Skema tampilan sebelum → sesudah

### SEBELUM (Kondisi Saat Ini)

```text
┌─ sidebar ─┬──────────────────────────────────────────────────────────────────────────┐
│           │ Health Services / Rawat Inap                                             │
│           │ Daftar Pasien Rawat Inap                                                 │
│           │                                                                          │
│           │ (Saat menyaring Tanggal mulai = 02 Nov 2026)                             │
│           │ [!] Terjadi kesalahan pada server.                           [Tutup]     │
│           │ [ Coba Lagi ]                                                            │
│           │ Ringkasan tidak tersedia.                                                │
│           │                                                                          │
│           │ [02 Nov 2026 ▾] [Tanggal akhir ▾] [Semua unit ▾] ... [25 baris ▾] [↻]   │
│           │ ──────────────────────────────────────────────────────────────────────── │
│           │ Tidak ada episode yang cocok dengan penyaring ini.                       │
└───────────┴──────────────────────────────────────────────────────────────────────────┘
```

### SESUDAH (Setelah Perbaikan `FIX-EPS-045-01` Diterapkan)

```text
┌─ sidebar ─┬──────────────────────────────────────────────────────────────────────────┐
│           │ Health Services / Rawat Inap                                             │
│           │ Daftar Pasien Rawat Inap                                                 │
│           │                                                                          │
│           │ [TOTAL EPISODE 3] [DISIAPKAN 1] [DIRAWAT 1] [RENCANA PULANG 0] [SELESAI 1]│
│           │                                                                          │
│           │ [02 Nov 2026 ▾] [Tanggal akhir ▾] [Semua unit ▾] ... [25 baris ▾] [↻]   │
│           │ ──────────────────────────────────────────────────────────────────────── │
│           │ NO | NOMOR EPISODE        | PASIEN             | LOKASI       | STATUS   │
│           │ 1  | RI-2611020001        | Ny. Siti Rahmawati | Bed 01 - Mlt | Dirawat  │
│           │    |                      | RM-000101          |              | [Detail] │
│           │ 2  | RI-2611020002        | Tn. Ahmad Santoso  | Belum ada    | Disiapkan│
│           │    |                      | RM-000102          |              | [Detail] │
└───────────┴──────────────────────────────────────────────────────────────────────────┘
```

*(Catatan: contoh data pada skema SESUDAH memakai data pseudonim untuk menggambarkan perilaku saat database berisi episode pada rentang tanggal yang dipilih.)*

### Tabel Wilayah, Tombol, dan Keadaan

| Elemen | Sebelum | Sesudah |
| --- | --- | --- |
| **Banner Peringatan** | Muncul *"Terjadi kesalahan pada server."* (HTTP 500) setiap kali `Tanggal mulai` atau `Tanggal akhir` dipilih | Tidak muncul banner error; `GET /episodes` mengembalikan HTTP `200 OK` |
| **Kartu Ringkasan** | Gagal dengan 500 sehingga muncul teks *"Ringkasan tidak tersedia."* | Menampilkan jumlah riil per status sesuai rentang tanggal (`200 OK` dari `GET /episodes/summary`) |
| **Tabel Episode** | Tetap kosong / menampilkan baris usang sebelum penyaringan | Menampilkan episode yang `CreateDateTime`-nya masuk ke rentang tanggal WIB yang dipilih |
| **Pemilih Tanggal (`FIX-EPS-045-03`)** | Dapat memilih `Tanggal akhir` < `Tanggal mulai` | `Tanggal mulai` dibatasi `max={filters.endDate}`, `Tanggal akhir` dibatasi `min={filters.startDate}` |

---

## 4. Rincian perbaikan

### FIX-EPS-045-01 — Normalisasi rentang tanggal query daftar dan ringkasan episode ke UTC (`InpEpisodeService.Reads.cs`)

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-045-02` |
| **Area** | Backend |
| **Jenis perubahan** | Source C# backend (`Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.Reads.cs`) |
| **Berkas yang diubah** | `Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.Reads.cs` |
| **Radius dampak** | Endpoint `GET /api/v1/health-services/inpatient-management/episodes` dan `GET /api/v1/health-services/inpatient-management/episodes/summary` (dipanggil oleh layar `FE-INP-16` Daftar Pasien Rawat Inap serta pemanggil eksternal/Swagger) |
| **Bergantung pada** | Keputusan `K-01`, `K-02` dan otorisasi tulis backend |

**Langkah.**

1. Tambahkan `using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;` pada `InpEpisodeService.Reads.cs`.
2. Pada method `BuildFilteredEpisodeQuery(InpatientEpisodeListQuery query)` baris `421-430`, ganti perbandingan mentah `query.StartDate.Value` dan `query.EndDate.Value.Date.AddDays(1)` menjadi konversi UTC berbasis zona operasional WIB:
   - Untuk `query.StartDate`:
     - Jika `TimeOfDay == TimeSpan.Zero` dan `Kind == DateTimeKind.Unspecified` (tanggal polos `YYYY-MM-DD`), gunakan `AppDateTimeHelper.OperationalDateToUtc(query.StartDate.Value)`.
     - Selain itu (jika pemanggil mengirim jam eksplisit atau offset/UTC), gunakan `AppDateTimeHelper.ToUtc(query.StartDate.Value)`.
     - Saring `filtered = filtered.Where(x => x.CreateDateTime >= startUtc);`.
   - Untuk `query.EndDate`:
     - Jika `TimeOfDay == TimeSpan.Zero` (tanggal polos `YYYY-MM-DD` sesuai kontrak inklusif sampai akhir hari), hitung `var exclusiveEndUtc = AppDateTimeHelper.ToUtc(query.EndDate.Value.Date.AddDays(1));` (untuk `Unspecified`, menjadi pukul `00:00 WIB` hari berikutnya dalam UTC; untuk `Utc`, menjadi `00:00Z` hari berikutnya) lalu saring `filtered = filtered.Where(x => x.CreateDateTime < exclusiveEndUtc);`.
     - Jika `TimeOfDay != TimeSpan.Zero`, hitung `var inclusiveEndUtc = AppDateTimeHelper.ToUtc(query.EndDate.Value);` lalu saring `filtered = filtered.Where(x => x.CreateDateTime <= inclusiveEndUtc);`.
3. Periksa kembali bahwa tidak ada query lain di `InpEpisodeService.Reads.cs` yang melewatkan `DateTime` ber-`Kind = Unspecified` ke PostgreSQL.

**Spesifikasi Endpoint Terkait (Gaya Swagger):**

### Health Services / Inpatient Management / Inpatient Episode

Base URL: `api/v1/health-services/inpatient-management/episodes`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Mengambil daftar episode rawat inap bertingkat dengan penyaring status, unit, kelas, kata kunci, dan rentang tanggal (`startDate`, `endDate`) | `InpatientEpisode : Read` | Query `InpatientEpisodeListQuery` | `ApiResponse<InpatientEpisodePagedResult>` |
| `GET` | `/summary` | Mengambil ringkasan jumlah episode per status memakai penyaring yang sama dengan daftar | `InpatientEpisode : Read` | Query `InpatientEpisodeListQuery` | `ApiResponse<InpatientEpisodeSummaryResponse>` |

Kode status:
- `200 OK`: Permintaan berhasil dan daftar/ringkasan dikembalikan (termasuk bila hasil 0 baris).
- `401 Unauthorized`: Pengguna belum login atau sesi kedaluwarsa.
- `403 Forbidden`: Pengguna tidak memiliki hak akses `InpatientEpisode : Read`.
- `500 Internal Server Error`: Gangguan tak terduga pada server (tidak boleh terjadi lagi hanya karena input tanggal `YYYY-MM-DD`).

**Acceptance criteria.**

1. Pemanggilan `GET /api/v1/health-services/inpatient-management/episodes?pageNumber=1&pageSize=25&sortBy=createDateTime&sortDirection=desc&startDate=2026-11-02` mengembalikan HTTP `200 OK` (bukan `500`), baik saat data ditemukan maupun saat hasilnya kosong.
2. Pemanggilan `GET /api/v1/health-services/inpatient-management/episodes/summary?startDate=2026-11-02` dan kombinasi `startDate` + `endDate` (misalnya `startDate=2026-11-02&endDate=2026-11-02`) sama-sama mengembalikan HTTP `200 OK`.
3. Episode contoh yang dibuka pukul `06:30 WIB` tanggal `2026-11-02` (tersimpan `2026-11-01T23:30:00Z`) dan pukul `23:45 WIB` tanggal `2026-11-02` (tersimpan `2026-11-02T16:45:00Z`) keduanya ikut muncul saat disaring `startDate=2026-11-02&endDate=2026-11-02`, sedangkan episode pukul `23:45 WIB` tanggal `2026-11-01` (`2026-11-01T16:45:00Z`) dan pukul `00:05 WIB` tanggal `2026-11-03` (`2026-11-02T17:05:00Z`) tidak ikut.
4. Pemanggil yang mengirim format ISO UTC (misalnya `startDate=2026-11-02T00:00:00Z`) tetap berjalan tanpa error `500`.

**Verifikasi.**
- `dotnet build QuilvianSystemBackend.sln` berhasil dengan 0 error.
- Verifikasi runtime terhadap endpoint `GET /episodes` dan `GET /episodes/summary` memakai PostgreSQL nyata dengan parameter `startDate=2026-11-02`, `endDate=2026-11-02`, dan kombinasi keduanya mengembalikan `200 OK`.

**Risiko.**  
Rendah. Tidak ada perubahan skema database, migrasi, atau kontrak JSON response. Risiko satu-satunya adalah perbedaan persepsi batas jam bila ada pemanggil lama yang mengasumsikan `YYYY-MM-DD` dibaca sebagai `00:00Z` alih-alih `00:00 WIB`; hal ini dikunci eksplisit oleh keputusan `K-02` agar selaras dengan seluruh modul lain (`AppDateTimeHelper`).

---

### FIX-EPS-045-02 — Normalisasi parameter tanggal pada layanan `InPatientManagement` sejenis (`Census`, `Monitoring`, `IntegrationOutbox`)

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-045-T1` |
| **Area** | Backend |
| **Jenis perubahan** | Source C# backend (4 file service di `Areas/HealthServices/InPatientManagement/Services/`) |
| **Berkas yang diubah** | `InpCensusQueryService.cs`; `InpDischargeService.Closure.cs`; `InpDischargeService.Departure.cs`; `InpIntegrationReplayService.cs` |
| **Radius dampak** | Layar **Pasien Sedang Dirawat** (`GET /census`, `GET /census/summary`), **Daftar Pantau** (`GET /monitoring/billed-pending-procedure-orders`, `GET /monitoring/departures-before-clearance`), dan **Integration Outbox** (`GET /integration-outbox`) |
| **Bergantung pada** | `K-01`, `K-02`, dan dapat memakai helper konversi rentang tanggal internal bersama agar tidak menduplikasi logika di keempat file |

**Langkah.**

1. Sediakan helper internal kecil bersama di modul `InPatientManagement` (atau di `InpEpisodeService.Reads.cs` / helper modul yang memanggil `AppDateTimeHelper`) untuk menormalkan batas bawah dan batas atas query `DateTime?` menjadi UTC yang aman untuk Npgsql:
   - Untuk parameter tanggal awal (`StartDate`, `ClosedFrom`, `From`): `NormalizeStartUtc(DateTime value)` -> jika `TimeOfDay == TimeSpan.Zero && Kind == Unspecified`, pakai `AppDateTimeHelper.OperationalDateToUtc(value)`, selain itu `AppDateTimeHelper.ToUtc(value)`.
   - Untuk parameter tanggal akhir (`EndDate`, `ClosedTo`, `To`): jika `TimeOfDay == TimeSpan.Zero`, pakai `< AppDateTimeHelper.ToUtc(value.Date.AddDays(1))` (eksklusif awal hari berikutnya); jika membawa jam spesifik (seperti `...T23:59:59` dari `inpatient-monitoring-utils.jsx`), pakai `<= AppDateTimeHelper.ToUtc(value)`!
2. Terapkan helper tersebut pada:
   - [InpCensusQueryService.cs:1225-1236](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpCensusQueryService.cs#L1225-L1236) (`StartDate`, `EndDate`)
   - [InpDischargeService.Closure.cs:599-607](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Closure.cs#L599-L607) (`ClosedFrom`, `ClosedTo`)
   - [InpDischargeService.Departure.cs:69-70](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Departure.cs#L69-L70) (`From`, `To`)
   - [InpIntegrationReplayService.cs:20-21](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpIntegrationReplayService.cs#L20-L21) (`From`, `To` — sekaligus memastikan pemeriksaan `request.From > request.To` dilakukan sesudah penormalan UTC agar tidak membandingkan `Kind` berbeda).

**Acceptance criteria.**

1. `GET /api/v1/health-services/inpatient-management/census?startDate=2026-11-02&endDate=2026-11-02` dan `/census/summary` mengembalikan `200 OK`.
2. `GET /api/v1/health-services/inpatient-management/monitoring/billed-pending-procedure-orders?closedFrom=2026-11-02T00:00:00&closedTo=2026-11-02T23:59:59` mengembalikan `200 OK`.
3. `GET /api/v1/health-services/inpatient-management/monitoring/departures-before-clearance?from=2026-11-02T00:00:00&to=2026-11-02T23:59:59` mengembalikan `200 OK`.
4. `GET /api/v1/health-services/inpatient-management/integration-outbox?from=2026-11-02&to=2026-11-02` mengembalikan `200 OK` (bukan `400` dengan pesan Npgsql).

**Verifikasi.**
- `dotnet build QuilvianSystemBackend.sln` berhasil dengan 0 error.
- Pengujian endpoint terhadap PostgreSQL nyata dengan tanggal polos `YYYY-MM-DD` dan ISO tanpa zona waktu (`...T00:00:00` / `...T23:59:59`).

**Risiko.**  
Rendah. Pastikan di `InpDischargeService.Departure.cs` dan `InpDischargeService.Closure.cs`, string seperti `2026-11-02T23:59:59` (dikirim frontend `inpatient-monitoring-utils.jsx:60,67`) tidak ikut ditambah 1 hari oleh aturan `TimeOfDay == Zero` — karena `TimeOfDay` bukan `Zero`, ia hanya dikonversi via `AppDateTimeHelper.ToUtc` menjadi `2026-11-02T16:59:59Z` (akhir hari WIB).

---

### FIX-EPS-045-03 — Penjagaan batas silang pemilih tanggal dan penanganan tampilan saat gagal (Frontend, Opsional)

| | |
| --- | --- |
| **Menutup** | `ISS-EPS-045-T2` |
| **Area** | Frontend |
| **Jenis perubahan** | Source JSX frontend (`QuilvianSystemFrontendDev`) |
| **Berkas yang diubah** | `src/components/view/health-services/inpatient-management/inpatient-episode-worklist-view.jsx`; `src/lib/hooks/health-services/inpatient-management/use-inpatient-episode-worklist.jsx`; (serta test unit terkait) |
| **Radius dampak** | Layar `Daftar Pasien Rawat Inap` (`FE-INP-16`) saja; tidak mengubah base component `FilterDatePicker` maupun `inpatient-management-slice.jsx` |
| **Bergantung pada** | `K-03` dan otorisasi tulis frontend |

**Langkah.**

1. Pada `inpatient-episode-worklist-view.jsx:336-346`, tambahkan `max={filters.endDate}` pada `FilterDatePicker` `startDate` dan `min={filters.startDate}` pada `FilterDatePicker` `endDate`.
2. Pada `use-inpatient-episode-worklist.jsx`, saat `load` gagal (`catch`), jangan biarkan baris lama dari penyaringan sebelumnya tetap ditampilkan sebagai hasil query yang gagal (misalnya dengan tidak meneruskan `list.items` saat `loadError` aktif di view/hook, atau mereset daftar tampilan yang terlihat ketika `loadError` muncul).

**Acceptance criteria.**

1. Pada `FilterDatePicker` **Tanggal akhir**, tanggal sebelum `Tanggal mulai` yang sedang dipilih tidak dapat diklik.
2. Saat `GET /episodes` mengembalikan error (mis. 500), layar menampilkan `InformationAlert` berisi pesan server dan **tidak** menampilkan baris sisa penyaringan sebelumnya seolah-olah cocok dengan filter saat ini.

**Verifikasi.**
- `npm run lint:errors`
- `npm run test:unit` (atau `tests/unit/inpatient-episode-worklist.test.mjs`)
- `npm run build`

**Risiko.**  
Rendah, terbatas pada satu layar daftar kerja episode.

---

## 5. Urutan pengerjaan

```text
K-01 & K-02 (keputusan pemilik + TASK MODE: BACKEND)
├── ⛔ FIX-EPS-045-01 (Backend — InpEpisodeService.Reads.cs)   prioritas 1
└── ⛔ FIX-EPS-045-02 (Backend — 4 service InPatientManagement) prioritas 2

K-03 (keputusan pemilik + TASK MODE: FRONTEND, opsional)
└── ⛔ FIX-EPS-045-03 (Frontend — worklist view & hook)        prioritas 3
```

---

## 6. Keputusan pemilik yang dibutuhkan

| No | Keputusan | Pilihan | Keputusan Pemilik (`2026-11-25`) | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| K-01 | Cakupan perbaikan backend: hanya `InpEpisodeService.Reads.cs` atau seluruh layanan `InPatientManagement` yang berpola sama? | Seluruh layanan / Hanya `InpEpisodeService.Reads.cs` | **Dipilih Opsi A** (*"iya perbaiki semua ya"*) | Tidak ada |
| K-02 | Zona waktu interpretasi tanggal `YYYY-MM-DD` pada query daftar: WIB (`Asia/Jakarta` via `AppDateTimeHelper`) atau UTC tengah malam? | WIB (`Asia/Jakarta`) / UTC | **Dipilih Opsi A** (*"iya saya setujui"*) | Tidak ada |
| K-03 | Apakah perbaikan UX ringan frontend (`min`/`max` date picker & pengosongan baris stale saat gagal) ikut dikerjakan? | Ya / Fokus backend saja | **Disetujui (Ya)** (*"iya perbaiki juga"*) | Tidak ada |

---

## 7. Dokumen hulu yang ikut direvisi

Tidak ada dokumen arsitektur, skema tampilan, maupun kontrak API yang perlu diubah karena kontrak `InpatientEpisodeListQuery` (`YYYY-MM-DD`) dan `FE-INP-16` sudah benar; cacat murni berada pada implementasi konversi tanggal backend (`BUG`).

---

## 8. Verifikasi menyeluruh

1. Pastikan database uji (mis. `QuilvianNewDevHamzah` atau `QuilvianNewDevMaster`) memiliki minimal satu baris `InpEpisode` yang dibuat pada tanggal uji (mis. `2026-11-02` waktu WIB, termasuk contoh yang dibuat pukul `00:00–06:59 WIB`).
2. Buka menu **Rawat Inap → Daftar Pasien Rawat Inap** pada browser.
3. Pastikan muat awal menampilkan seluruh kartu ringkasan dan baris episode dengan status `200 OK`.
4. Pilih **Tanggal mulai** = `02 Nov 2026` (`startDate=2026-11-02`): pastikan `GET /episodes` dan `GET /episodes/summary` keduanya menjawab `200 OK`, tidak ada banner *"Terjadi kesalahan pada server."*, dan baris yang dibuat sejak `2 Nov 2026 00:00 WIB` tampil dengan benar.
5. Pilih juga **Tanggal akhir** = `02 Nov 2026` (`endDate=2026-11-02`), ketik kata kunci pencarian nama pasien / No. RM / nomor episode, dan uji pada layar **Pasien Sedang Dirawat** (`Census`) serta **Daftar Pantau** bila `FIX-EPS-045-02` disetujui.

---

## 9. Riwayat

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-11-25 | Rencana perbaikan dibuat (`FIX-EPS-045-01` s.d. `FIX-EPS-045-03`), menunggu keputusan pemilik (`K-01` s.d. `K-03`) dan otorisasi tulis | `diagnose-module-issue` |
| 2026-11-25 | Disetujui pemilik (`K-01=A`, `K-02=A`, `K-03=Ya`); implementasi BE (`InpDateRangeHelper` + 6 layanan) & FE selesai; lint/CSS/unit test FE lulus; build penuh BE & FE ditangguhkan untuk build mandiri sesuai instruksi pemilik | Agent (Implementation) |
