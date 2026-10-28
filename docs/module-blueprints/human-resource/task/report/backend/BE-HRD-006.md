# Laporan Perubahan Backend — `BE-HRD-006`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `BE-HRD-006` |
| Judul | Administrasi Presensi Harian, Penutupan Periode Kehadiran, dan Jalur Koreksi Kehadiran On-Behalf HR |
| Slice | `S-B1` (Administrasi Presensi Harian, Penutupan Periode, & Koreksi Kehadiran) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang 3 |
| Trace | `HRD-DEC-022`, `HRD-DEC-028`, `IN-07` |
| Contract version | API Contract v1 (Draft) |
| Dependency | `BE-HRD-002` (Selesai ✅) |
| Klasifikasi | `MEDIUM` |
| Task mode | `BACKEND` |
| Target tulis | `HrdAttendanceCorrectionRequest.cs`, `HrdAttendanceCorrectionRequestConfiguration.cs`, `20261027104500_AddOnBehalfFieldsToAttendanceCorrectionRequest.cs`, `AttendanceCorrectionOnBehalfService.cs`, `AttendanceCorrectionController.cs`, `AttendanceCorrectionWorkflowLifecycleService.cs`, `AttendanceProcessingService.cs`, `AttendanceCorrectionDtos.cs`, `Program.cs` |
| Model | Gemini 3.8 Flash (High) |
| Commit backend saat dikerjakan | `16f6b2ad28e0e9cbd374354abe240ff3b163ace0` |
| Tanggal | 27 Oktober 2026 |
| Status | Selesai (`PASS`) |

---

## 1. Masalah yang diperbaiki

1. **Ketiadaan Jalur Koreksi On-Behalf bagi HR (`HRD-DEC-028`)**:
   Sebelumnya, implementasi pengajuan koreksi presensi (`AttendanceCorrectionService.CreateAsync`) mengunci validasi identitas pemohon secara kaku ke `daily.WorkforceProfileId == actorWorkforceProfileId`. Akibatnya, jika ada pegawai rumah sakit (misal tenaga perawat shift malam di ruang isolasi, dokter jaga darurat, atau staf lapangan) yang mengalami kendala teknis dan tidak dapat mengakses aplikasi Employee Self Service (ESS), HR Admin tidak memiliki sarana administratif resmi untuk mengajukan permohonan koreksi kehadiran atas nama (*on-behalf*) pegawai tersebut.

2. **Kelemahan Penurunan Status pada Sinkronisasi Persetujuan (`HRD-DEC-022`)**:
   Pada `AttendanceCorrectionWorkflowLifecycleService.SynchronizeAsync`, status permohonan yang sudah diterapkan (`Applied`) ke log kehadiran harian dapat tertimpa kembali menjadi `Approved` jika endpoint sinkronisasi workflow dipanggil ulang. Penurunan status ini membuka celah bug ganda (*double apply*) yang dapat merusak rekonsiliasi data kehadiran harian.

3. **Penegakan Penolakan Presensi pada Periode Terkunci (*Closed Period Guard*)**:
   Proses kalkulasi dan pemrosesan ulang kehadiran harian (`ProcessSingleAsync` dan `ProcessRangeAsync`) belum memvalidasi apakah tanggal kehadiran yang diproses berada dalam periode presensi yang sudah resmi ditutup (`Closed`). Hal ini berisiko menimbulkan mutasi presensi surut (*retroactive update*) yang tidak sah tanpa prosedur pembukaan resmi (*reopen*).

---

## 2. Proses bisnis & alur kerja rumah sakit

```
                                [KEDARURATAN STAF / KENDALA AKSES ESS]
                                                    │
                                                    ▼
[HR Administrator] ──► 1. Memvalidasi alasan kedaruratan & bukti fisik
         │
         ├──► 2. Memanggil POST /attendance/correction-requests/on-behalf
         │       ├─► Sistem memeriksa apakah periode presensi masih OPEN
         │       ├─► Menyimpan: Initiator (HR ID), Workforce Diwakili, Alasan On-Behalf, Timestamp
         │       └─► Memberi tanda IsOnBehalf = true dengan audit trail lengkap
         │
         ├──► 3. Permohonan masuk ke alur persetujuan workflow resmi
         │       └─► Atasan pegawai memvalidasi & menyetujui koreksi
         │
         └──► 4. Apply koreksi diterapkan ke HrdAttendanceDaily
                 └─► Status Applied dilindungi (HRD-DEC-022); tidak dapat diturunkan kembali
```

* **Tujuan**: Memberikan kewenangan terkontrol bagi Bagian HRD untuk membantu staf medis/non-medis yang terhalang mengajukan koreksi presensi secara mandiri, sekaligus melindungi integritas data penggajian agar periode yang telah ditutup tidak dimanipulasi secara siluman.
* **Pelaku**: HR Administrator, Kepala Ruangan/Atasan Medis, Staf yang Bersangkutan.
* **Pemicu**: Permintaan bantuan tertulis/lisan dari staf yang mengalami kendala akses ESS, kerusakan perangkat, atau penugasan darurat di lapangan.
* **Langkah & Aturan**:
  1. HR Administrator menerima berkas bukti pendukung dan alasan ketidakhadiran staf.
  2. HR Administrator membuka form koreksi on-behalf dan menginput ID pegawai, tanggal kehadiran, alasan permohonan, serta alasan khusus mengapa diajukan oleh HR (`OnBehalfReason`).
  3. Sistem memverifikasi bahwa tanggal kerja tidak berada di dalam periode yang berstatus `Closed`. Jika periode sudah ditutup, sistem menolak pengajuan dengan pesan kesalahan HTTP 409 Conflict.
  4. Sistem menyimpan record pada tabel `HrdAttendanceCorrectionRequest` dengan atribut `IsOnBehalf = true`, `InitiatedByUserId = hrActorUserId`, dan mencatat waktu notifikasi.
  5. Pengajuan mengikuti siklus verifikasi workflow approval terpadu.
* **Hasil Akhir**: Koreksi kehadiran staf terselesaikan secara akuntabel tanpa melanggar prinsip imutabilitas rekaman mentah kehadiran.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang dimodifikasi & dibuat

| Berkas | Status | Perubahan |
| :--- | :---: | :--- |
| `Areas/Corporate/HumanResource/AttendanceManagement/Models/HrdAttendanceCorrectionRequest.cs` | Diperbarui | Menambahkan 4 kolom On-Behalf: `InitiatedByUserId`, `IsOnBehalf`, `OnBehalfReason`, `OnBehalfNotifiedAt`, serta relasi navigasi `InitiatedByUser`. |
| `Repositories/Configurations/Corporate/HumanResource/AttendanceManagement/HrdAttendanceCorrectionRequestConfiguration.cs` | Diperbarui | Konfigurasi mapping EF Core untuk kolom on-behalf dan penambahan index komposit `(IsOnBehalf, RequestStatus)`. |
| `Migrations/20261027104500_AddOnBehalfFieldsToAttendanceCorrectionRequest.cs` | Baru | Berkas migrasi skema database EF Core untuk penambahan 4 kolom dan index on-behalf. |
| `Areas/Corporate/HumanResource/AttendanceManagement/DTOs/AttendanceCorrectionDtos.cs` | Diperbarui | Penambahan DTO `CreateAttendanceCorrectionOnBehalfRequest` dan penambahan properti on-behalf pada DTO list/detail. |
| `Areas/Corporate/HumanResource/AttendanceManagement/Services/AttendanceCorrectionOnBehalfService.cs` | Baru | Service khusus untuk validasi identitas HR, guard status periode, pembuatan draft koreksi atas nama staf, dan pencatatan audit trail lengkap. |
| `Areas/Corporate/HumanResource/AttendanceManagement/Controllers/AttendanceCorrectionController.cs` | Diperbarui | Penambahan endpoint `POST /on-behalf` dengan wewenang `AttendanceCorrection:CreateOnBehalf`. |
| `Areas/Corporate/HumanResource/AttendanceManagement/Services/AttendanceCorrectionWorkflowLifecycleService.cs` | Diperbarui | Pengetatan guard sinkronisasi status (`HRD-DEC-022`) agar status `Applied` tidak pernah diturunkan kembali ke `Approved`. |
| `Areas/Corporate/HumanResource/AttendanceManagement/Services/AttendanceProcessingService.cs` | Diperbarui | Penambahan guard penutupan periode pada `ProcessSingleAsync` dan `ProcessRangeAsync` yang menolak proses presensi jika tanggal masuk ke dalam `HrdAttendancePeriod` berstatus `Closed`. |
| `Program.cs` | Diperbarui | Pendaftaran `AttendanceCorrectionOnBehalfService` ke service collection (`AddScoped`). |
| `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` | Diperbarui | Pembaruan status task `BE-HRD-006` menjadi selesai (`✅`). |
| `docs/module-blueprints/human-resource/roadmap/requirement-traceability.md` | Diperbarui | Pembaruan status requirement `IN-07` menjadi terbukti penuh (`✅`). |

### 3.2 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| :--- | :--- |
| **Kontrak API** | Penambahan 1 endpoint baru `POST api/v1/corporate/human-resource/attendance/correction-requests/on-behalf` sesuai spesifikasi `contracts/api-contract.md`. |
| **Database** | Penambahan 4 kolom dan index pada tabel `HrdAttendanceCorrectionRequest` (`IsOnBehalf`, `InitiatedByUserId`, `OnBehalfReason`, `OnBehalfNotifiedAt`). Berkas migrasi EF Core telah disiapkan di `Migrations/`. |
| **Keamanan & Otorisasi** | Menggunakan hak akses `AttendanceCorrection:CreateOnBehalf` yang hanya dapat diakses oleh peran administratif HR, tidak terbuka untuk employee biasa. |

---

## 4. Dokumentasi endpoint

#### `[Tags("Corporate / Human Resource / Attendance Management / Attendance Correction Administration")]`
**Base URL**: `api/v1/corporate/human-resource/attendance/correction-requests`

| Method | Path | Deskripsi | Hak Akses | Request Body | Response |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `POST` | `/on-behalf` | Mengajukan permohonan koreksi kehadiran atas nama staf oleh HR Admin | `[AccessPermission("AttendanceCorrection", "CreateOnBehalf")]` | `CreateAttendanceCorrectionOnBehalfRequest` (`WorkforceProfileId`, `AttendanceDailyId`, `CorrectionType`, `Reason`, `OnBehalfReason`, `Details`, `ExceptionIds`) | `ApiResponse<AttendanceCorrectionDetailResponse>` |
| `POST` | `/{id:guid}/workflow/synchronize` | Sinkronisasi status workflow permohonan (diperketat dari penurunan status `Applied`) | `[AccessPermission("AttendanceCorrection", "Synchronize")]` | Route parameter `id` | `ApiResponse<AttendanceCorrectionSynchronizationResponse>` |

---

## 5. Bukti verifikasi & pemenuhan Acceptance Criteria

1. **Acceptance Criteria 1 (Penolakan Presensi pada Periode Terkunci)**:
   * Terbukti pada `AttendanceProcessingService.cs` (`ProcessSingleAsync` dan `ProcessRangeAsync`) yang secara eksplisit memverifikasi `p.PeriodStatus == AttendanceValueConstants.AttendancePeriodStatus.Closed` dan mengembalikan HTTP 409 Conflict bila ada presensi yang disentuh pada periode yang terkunci.
2. **Acceptance Criteria 2 (Jalur On-Behalf Mencatat Inisiator & Alasan)**:
   * Terbukti pada `AttendanceCorrectionOnBehalfService.cs` yang menyimpan `InitiatedByUserId = hrActorUserId`, `IsOnBehalf = true`, `OnBehalfReason`, dan mencatat `OnBehalfNotifiedAt`.
3. **Pencegahan Celah `HRD-DEC-022`**:
   * Terbukti pada `AttendanceCorrectionWorkflowLifecycleService.SynchronizeAsync` yang memblokir penimpaan status saat `previousStatus` bernilai `Applied`.
