# Laporan Perubahan Backend — `BE-HRD-009`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `BE-HRD-009` |
| Judul | Implementasi Engine Roster Shift 24/7 dan Penugasan Siaga (On-Call) |
| Slice | `S-B4` (Roster, Jadwal 24/7 & On-Call) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang 3 |
| Trace | `HRD-DEC-026`, `IN-06` |
| Contract version | API Contract v1 (Draft) |
| Dependency | `BE-HRD-002` (Selesai ✅) |
| Klasifikasi | `HIGH` |
| Task mode | `BACKEND` |
| Target tulis | `RosterSchedulingDtos.cs`, `RosterPeriodService.cs`, `RosterPeriodController.cs`, `ShiftReplacementService.cs`, `ShiftReplacementController.cs`, `OnCallAssignmentService.cs`, `OnCallAssignmentController.cs`, `Program.cs` |
| Model | Gemini 3.8 Flash (High) |
| Tanggal | 27 Oktober 2026 |
| Status | Selesai (`PASS`) |

---

## 1. Masalah yang diperbaiki

1. **Ketiadaan Controller dan Layanan API untuk Engine Roster (`HRD-DEC-026`)**:
   Meskipun tabel database EF Core untuk penjadwalan roster (`TrxRosterPeriod`, `TrxRosterAssignment`, `TrxShiftAssignment`, `TrxShiftReplacement`, `TrxOnCallAssignment`) telah dibuat pada migrasi skema sebelumnya, belum tersedia controller maupun domain service backend untuk mengelola siklus operasional roster rumah sakit 24/7.
2. **Ketiadaan Validasi Konflik Jadwal Ganda dan Jeda Istirahat Minimum Pegawai**:
   Rumah sakit beroperasi 24 jam sehari, 7 hari seminggu. Penjadwalan staf medis dan keperawatan tanpa mesin validasi berisiko menimbulkan kelelahan ekstrem (*fatigue risk*), kesalahan klinis (*medical error*), atau tumpang tindih shift (*double shift*). Diperlukan engine validasi otomatis yang memeriksa tumpang-tindih waktu shift dan memastikan jeda istirahat wajib minimal 11 jam sesuai standar ketenagakerjaan dan keselamatan rumah sakit.
3. **Ketiadaan Mekanisme Pencatatan Penggantian Shift Darurat (*Emergency Replacement*)**:
   Dalam operasional rumah sakit sehari-hari, staf jaga kerap berhalangan hadir mendadak (misalnya sakit atau kedaruratan keluarga). Rumah sakit membutuhkan pencatatan resmi penggantian shift mendadak yang mencatat identitas pegawai pengganti dan alasan operasional tanpa merusak riwayat penugasan asli.
4. **Ketiadaan Manajemen Penugasan Siaga (*On-Call Schedule & Activation*)**:
   Unit gawat darurat, kamar bedah, laboratorium cito, dan dokter spesialis memerlukan daftar penugasan siaga (on-call) 24/7 yang dapat diaktivasi sewaktu-waktu saat lonjakan pasien atau kedaruratan medis terjadi.

---

## 2. Proses bisnis & alur kerja rumah sakit

```
[Kepala Ruangan / Supervisor HR]
         │
         ├──► 1. Membuat Periode Roster (Draft) ──► POST /roster-periods
         │
         ├──► 2. Mengalokasikan Jadwal Shift Staf ──► POST /roster-periods/{id}/assignments
         │
         ├──► 3. Menjalankan Engine Validasi Otomatis ──► POST /roster-periods/{id}/validate
         │       ├─► Deteksi Jadwal Ganda (Double Shift Conflict)
         │       └─► Deteksi Pelanggaran Jeda Istirahat Wajib Minimum (< 11 Jam)
         │
         ├──► 4. Jika Validasi Lulus ──► Publikasikan Roster ──► POST /roster-periods/{id}/publish
         │
         └──► 5. Kejadian Khusus Selama Operasional Berjalan:
                 ├─► Penggantian Shift Darurat ──► POST /shift-replacements & /approve
                 ├─► Penjadwalan Siaga On-Call ──► POST /on-call-assignments
                 └─► Aktivasi Siaga Darurat ──► POST /on-call-assignments/{id}/activate
```

* **Tujuan**: Menjamin kesinambungan pelayanan medis rumah sakit 24/7 tanpa risiko kekosongan jaga, sekaligus melindungi keselamatan pasien dan staf medis melalui pencegahan kelelahan kerja (*fatigue prevention*).
* **Pelaku**: Kepala Ruangan / Kepala Instalasi, HR Scheduling Administrator, Dokter / Perawat / Tenaga Penunjang Medis.
* **Pemicu**: Penyusunan jadwal rotasi bulanan, kedaruratan staf yang tidak dapat hadir, serta kebutuhan penyiagaan tim on-call kamar operasi / IGD.
* **Langkah & Aturan**:
  1. Periode roster disusun dengan status awal `Draft`.
  2. Alokasi shift staf dipetakan dengan waktu mulai dan selesai berdasarkan master data `MstShift`.
  3. Engine validasi otomatis memeriksa jadwal:
     - Jika pegawai memiliki shift tumpang-tindih pada hari/jam yang sama, sistem menandai `HasDoubleShiftConflict = true` dan `HasBlockingConflict = true`.
     - Jika jeda antara waktu selesai shift sebelumnya dan waktu mulai shift berikutnya kurang dari 11 jam, sistem menandai `HasMinimumRestConflict = true` dan `HasBlockingConflict = true`.
  4. Roster yang memiliki konflik pemblokiran dilarang dipublikasikan (`409 Conflict`) sampai supervisor memperbaiki jadwal atau memberikan override resmi.
  5. Penggantian shift mendadak mencatat staf asal, staf pengganti, jenis penggantian (`Emergency`), dan alasan operasional, lalu memperbarui status penugasan shift secara langsung.
  6. Penugasan on-call mencatat peran siaga (`Primary`, `Backup`, `Escalation`) dan dapat diaktivasi saat status kedaruratan terjadi.

---

## 3. Spesifikasi Endpoint API (Gaya Swagger)

### Grup: `Corporate / Human Resource / Scheduling Management / Roster Periods`
Tag: `[Tags("Corporate / Human Resource / Scheduling Management / Roster Periods")]`

| Method | Path | Deskripsi | Auth / Permission | Request Body / Query | Response Model |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `api/v1/corporate/human-resource/scheduling-management/roster-periods/summary` | Mengambil ringkasan statistik periode roster | `RosterPeriod:Read` | - | `ApiResponse<RosterPeriodSummaryResponse>` |
| `GET` | `api/v1/corporate/human-resource/scheduling-management/roster-periods` | Mengambil daftar periode roster berhalaman | `RosterPeriod:Read` | `pageNumber, pageSize, search, status, departmentId` | `ApiResponse<PagedResult<RosterPeriodResponse>>` |
| `GET` | `api/v1/corporate/human-resource/scheduling-management/roster-periods/{id}` | Mengambil detail lengkap periode roster beserta alokasi staf dan shift | `RosterPeriod:Read` | `id` (Guid) | `ApiResponse<RosterPeriodDetailResponse>` |
| `POST` | `api/v1/corporate/human-resource/scheduling-management/roster-periods` | Membuat periode roster baru | `RosterPeriod:Create` | `CreateRosterPeriodRequest` | `ApiResponse<RosterPeriodResponse>` (201 Created) |
| `POST` | `api/v1/corporate/human-resource/scheduling-management/roster-periods/{id}/assignments` | Menambahkan penugasan staf dan rincian shift ke dalam roster | `RosterAssignment:Create` | `CreateRosterAssignmentRequest` | `ApiResponse<RosterAssignmentResponse>` (201 Created) |
| `POST` | `api/v1/corporate/human-resource/scheduling-management/roster-periods/{id}/validate` | Menjalankan engine validasi konflik jadwal ganda dan jeda istirahat wajib | `RosterPeriod:Update` | `id` (Guid) | `ApiResponse<RosterValidationResultResponse>` |
| `POST` | `api/v1/corporate/human-resource/scheduling-management/roster-periods/{id}/publish` | Mempublikasikan periode roster secara resmi | `RosterPeriod:Update` | `id` (Guid) | `ApiResponse<RosterPeriodResponse>` |
| `POST` | `api/v1/corporate/human-resource/scheduling-management/roster-periods/{id}/close` | Menutup dan mengunci periode roster secara permanen | `RosterPeriod:Update` | `id` (Guid) | `ApiResponse<RosterPeriodResponse>` |

### Grup: `Corporate / Human Resource / Scheduling Management / Shift Replacements`
Tag: `[Tags("Corporate / Human Resource / Scheduling Management / Shift Replacements")]`

| Method | Path | Deskripsi | Auth / Permission | Request Body / Query | Response Model |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `api/v1/corporate/human-resource/scheduling-management/shift-replacements` | Mengambil daftar penggantian shift | `ShiftReplacement:Read` | `pageNumber, pageSize, shiftAssignmentId, status` | `ApiResponse<PagedResult<ShiftReplacementResponse>>` |
| `POST` | `api/v1/corporate/human-resource/scheduling-management/shift-replacements` | Mencatat permohonan penggantian shift mendadak | `ShiftReplacement:Create` | `CreateShiftReplacementRequest` | `ApiResponse<ShiftReplacementResponse>` (201 Created) |
| `POST` | `api/v1/corporate/human-resource/scheduling-management/shift-replacements/{id}/approve` | Menyetujui dan menerapkan penggantian shift ke jadwal aktif | `ShiftReplacement:Update` | `notes` (string) | `ApiResponse<ShiftReplacementResponse>` |

### Grup: `Corporate / Human Resource / Scheduling Management / On-Call Assignments`
Tag: `[Tags("Corporate / Human Resource / Scheduling Management / On-Call Assignments")]`

| Method | Path | Deskripsi | Auth / Permission | Request Body / Query | Response Model |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `api/v1/corporate/human-resource/scheduling-management/on-call-assignments` | Mengambil daftar jadwal penugasan siaga (on-call) | `OnCallAssignment:Read` | `pageNumber, pageSize, rosterPeriodId, workforceProfileId, status` | `ApiResponse<PagedResult<OnCallAssignmentResponse>>` |
| `POST` | `api/v1/corporate/human-resource/scheduling-management/on-call-assignments` | Menjadwalkan penugasan staf siaga on-call | `OnCallAssignment:Create` | `CreateOnCallAssignmentRequest` | `ApiResponse<OnCallAssignmentResponse>` (201 Created) |
| `POST` | `api/v1/corporate/human-resource/scheduling-management/on-call-assignments/{id}/activate` | Mengaktivasi pemanggilan tugas staf siaga darurat | `OnCallAssignment:Update` | `id` (Guid) | `ApiResponse<OnCallAssignmentResponse>` |

---

## 4. Bukti implementasi kode

1. **DTOs Lengkap**:
   - Berkas: [RosterSchedulingDtos.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/SchedulingManagement/DTOs/RosterSchedulingDtos.cs)
   - Memuat DTO request/response untuk `RosterPeriod`, `RosterAssignment`, `ShiftAssignment`, `ShiftReplacement`, dan `OnCallAssignment`.

2. **Domain Service Roster & Engine Validasi**:
   - Berkas: [RosterPeriodService.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/SchedulingManagement/Services/RosterPeriodService.cs)
   - Mengimplementasikan logika perhitungan durasi shift, deteksi `HasDoubleShiftConflict`, penegakan jeda istirahat minimum 11 jam `HasMinimumRestConflict`, publikasi roster, serta penguncian penutupan roster.

3. **Domain Service Penggantian Shift & On-Call**:
   - Berkas: [ShiftReplacementService.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/SchedulingManagement/Services/ShiftReplacementService.cs)
   - Berkas: [OnCallAssignmentService.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/SchedulingManagement/Services/OnCallAssignmentService.cs)
   - Mengimplementasikan alur penggantian darurat langsung ke jadwal operasional dan siklus aktivasi penugasan on-call.

4. **Controllers & Registrasi DI**:
   - Berkas: [RosterPeriodController.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/SchedulingManagement/Controllers/RosterPeriodController.cs)
   - Berkas: [ShiftReplacementController.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/SchedulingManagement/Controllers/ShiftReplacementController.cs)
   - Berkas: [OnCallAssignmentController.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/SchedulingManagement/Controllers/OnCallAssignmentController.cs)
   - Registrasi dependency injection pada [Program.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Program.cs#L968-L972).

---

## 5. Ringkasan Kepatuhan & Status

* **Status Task**: Selesai (`PASS`) ✅
* **Acceptance Criteria 1**: Validasi mendeteksi jadwal ganda pegawai pada shift yang sama atau shift berturut-turut tanpa jeda istirahat wajib (`HasDoubleShiftConflict`, `HasMinimumRestConflict`) -> **LULUS**.
* **Acceptance Criteria 2**: Penggantian shift mendadak (darurat) mencatat pengganti dan alasan operasional (`TrxShiftReplacement`) -> **LULUS**.
* **Perubahan Database**: Tidak memerlukan penambahan kolom/tabel baru (menggunakan tabel `TrxRosterPeriod`, `TrxRosterAssignment`, `TrxShiftAssignment`, `TrxShiftReplacement`, `TrxOnCallAssignment` yang sudah ada pada skema database).
