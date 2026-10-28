# Laporan Perubahan Backend — `BE-HRD-004`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `BE-HRD-004` |
| Judul | Verifikasi dan Pemantapan Endpoint Layanan Mandiri Karyawan (ESS: Cuti, Lembur, Jadwal, Absensi, dan Profil) |
| Slice | `S-A2..S-A6` (Layanan Mandiri Karyawan / Employee Self Service) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang 4 |
| Trace | `HRD-DEC-007`, `HRD-DEC-028`, `IN-19` |
| Contract version | API Contract v1 (Draft) |
| Dependency | `BE-HRD-003` (Selesai ✅) |
| Klasifikasi | `HIGH` |
| Task mode | `BACKEND` |
| Target verifikasi | `LeaveRequestService.cs`, `LeaveRequestWorkflowController.cs`, `OvertimePlanningService.cs`, `OvertimeWorkflowController.cs`, `ShiftSwapService.cs`, `ShiftSwapController.cs`, `ScheduleChangeService.cs`, `ScheduleChangeController.cs`, `AttendanceCorrectionService.cs`, `AttendanceCorrectionController.cs`, `EmployeeProfileChangeService.cs`, `EmployeeProfileChangeController.cs` |
| Model | Gemini 3.8 Flash (High) |
| Tanggal | 27 Oktober 2026 |
| Status | Selesai (`PASS`) |

---

## 1. Masalah yang diperbaiki & diverifikasi

1. **Penegakan Otentikasi dan Isolasi Identitas Pemohon ESS (`actorWorkforceProfileId`)**:
   Layanan mandiri karyawan harus memastikan bahwa pegawai hanya dapat mengajukan permohonan (cuti, lembur, tukar shift, perbaikan presensi, dan perubahan profil) atas nama dirinya sendiri berdasarkan token login aktif, tanpa celah pemalsuan identitas (*identity spoofing*).
2. **Validasi Kuota Saldo Cuti dan Pencegahan Saldo Minus (*Boundary Case*)**:
   Pengajuan cuti tahunan, cuti besar, atau cuti khusus wajib menghitung hari kerja efektif (dengan mengecualikan hari libur nasional dan libur mingguan) serta menolak pengajuan jika estimasi saldo akhir menjadi negatif.
3. **Deteksi Konflik Penjadwalan Lembur (*Overtime Overlap & Rest Guard*)**:
   Permohonan lembur wajib memeriksa tumpang-tindih waktu terhadap jadwal dinas aktif (`HasScheduleConflict`), cuti yang sedang berjalan (`HasLeaveConflict`), pelatihan wajib (`HasTrainingConflict`), serta pelanggaran batas minimum istirahat (`HasMinimumRestConflict`).
4. **Alur Fleksibel Tukar Shift dan Koreksi Presensi Mandiri**:
   Staf medis dengan sistem kerja rotasi memerlukan mekanisme tukar shift dua arah dengan rekan sejawat serta koreksi presensi mandiri dengan batas waktu yang tertutup saat periode presensi telah dikunci (*closed period guard*).

---

## 2. Proses bisnis & alur kerja rumah sakit

```
[Pegawai Rumah Sakit (Dokter / Perawat / Staf)]
                     │
         [Login ke Portal ESS / Mobile]
                     │
   ┌─────────────────┼─────────────────┬─────────────────┐
   ▼                 ▼                 ▼                 ▼
[Pengajuan Cuti]  [Pengajuan Lembur] [Tukar Shift]  [Koreksi Presensi]
   │                 │                 │                 │
   ├─► Cek Kuota &   ├─► Deteksi       ├─► Validasi staf ├─► Cek Periode
   │   Hari Libur    │   Jadwal Bentrok│   pengganti     │   Open/Closed
   │                 │   & Jeda Istirahat                │
   ▼                 ▼                 ▼                 ▼
[Persetujuan Atasan Langsung & Kepala Ruangan via Workflow Terpadu]
```

* **Tujuan**: Memberikan fasilitas mandiri bagi seluruh tenaga kesehatan dan pegawai rumah sakit untuk mengelola administrasi kehadiran, waktu kerja, dan hak cuti secara transparan tanpa birokrasi manual kertas.
* **Pelaku**: Pegawai (Pemohon), Rekan Sejawat (Mitra Tukar Shift / Pengganti), Kepala Ruangan / Supervisor (Approver), HR Verifier.
* **Pemicu**: Kebutuhan cuti tahunan/sakit, penugasan lembur darurat pelayanan rawat inap, kebutuhan penyesuaian jadwal dinas jaga, atau perbaikan presensi akibat kendala mesin absensi.
* **Langkah & Aturan**:
  1. Identitas pemohon diekstraksi authoritative dari klaim login pengguna (`actorWorkforceProfileId`).
  2. Kalkulasi hari cuti mengecualikan kalender libur rumah sakit; saldo dipreservasi sementara saat status `WaitingApproval` dan dipotong final saat `Approved`.
  3. Rencana dan klaim lembur memeriksa tumpang-tindih waktu jam kerja; lembur ditolak jika menabrak shift utama atau jam istirahat.
  4. Permohonan tukar shift membutuhkan konfirmasi persetujuan dari kedua belah pihak sebelum diteruskan ke kepala ruangan.

---

## 3. Spesifikasi Endpoint API (Gaya Swagger)

### Grup: `Corporate / Human Resource / Leave Management / Leave Request`
Tag: `[Tags("Corporate / Human Resource / Leave Management / Leave Requests")]`

| Method | Path | Deskripsi | Auth / Permission | Request Body / Query | Response Model |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `api/v1/corporate/human-resource/leave-management/leave-requests/summary` | Mengambil ringkasan pengajuan cuti pegawai yang sedang login | `LeaveRequest:Read` | `LeaveRequestQueryRequest` | `ApiResponse<LeaveRequestSummaryResponse>` |
| `GET` | `api/v1/corporate/human-resource/leave-management/leave-requests/balance-options` | Mengambil pilihan jenis cuti dan sisa saldo hak cuti pegawai | `LeaveRequest:Read` | `asOfDate` (DateOnly) | `ApiResponse<List<LeaveRequestBalanceOptionResponse>>` |
| `POST` | `api/v1/corporate/human-resource/leave-management/leave-requests` | Mengajukan permohonan cuti baru (Draft/Submit) | `LeaveRequest:Create` | `CreateLeaveRequestRequest` | `ApiResponse<LeaveRequestActionResponse>` (201 Created) |
| `POST` | `api/v1/corporate/human-resource/leave-management/leave-requests/{id}/submit` | Mengirim pengajuan cuti ke alur persetujuan atasan | `LeaveRequest:Submit` | `SubmitLeaveRequestRequest` | `ApiResponse<LeaveRequestActionResponse>` |

### Grup: `Corporate / Human Resource / Overtime Management / Overtime Planning`
Tag: `[Tags("Corporate / Human Resource / Overtime Management / Overtime Planning")]`

| Method | Path | Deskripsi | Auth / Permission | Request Body / Query | Response Model |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `POST` | `api/v1/corporate/human-resource/overtime-management/overtime-plans` | Membuat rencana lembur staf / tim | `OvertimePlan:Create` | `CreateOvertimePlanRequest` | `ApiResponse<OvertimePlanResponse>` (201 Created) |
| `POST` | `api/v1/corporate/human-resource/overtime-management/overtime-plans/{planId}/details` | Menambahkan rincian penugasan lembur pegawai dengan validasi konflik | `OvertimePlan:Create` | `CreateOvertimePlanDetailRequest` | `ApiResponse<OvertimePlanDetailResponse>` |
| `POST` | `api/v1/corporate/human-resource/overtime-management/overtime-plans/{id}/validate` | Memvalidasi seluruh konflik jadwal lembur (shift, cuti, istirahat minimum) | `OvertimePlan:Update` | `id` (Guid) | `ApiResponse<OvertimePlanValidationResponse>` |

### Grup: `Corporate / Human Resource / Scheduling Management / Shift Swap`
Tag: `[Tags("Corporate / Human Resource / Scheduling Management / Shift Swap")]`

| Method | Path | Deskripsi | Auth / Permission | Request Body / Query | Response Model |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `POST` | `api/v1/corporate/human-resource/scheduling-management/shift-swap-requests` | Mengajukan pertukaran shift mandiri antar staf | `ShiftSwapRequest:Create` | `CreateShiftSwapRequest` | `ApiResponse<ShiftSwapResponse>` (201 Created) |
| `POST` | `api/v1/corporate/human-resource/scheduling-management/shift-swap-requests/{id}/peer-accept` | Rekan sejawat menyetujui pertukaran shift sebelum ke atasan | `ShiftSwapRequest:Update` | `notes` (string) | `ApiResponse<ShiftSwapResponse>` |

---

## 4. Bukti implementasi kode

1. **Layanan Mandiri Cuti**:
   - Berkas: [LeaveRequestService.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LeaveManagement/Services/LeaveRequestService.cs)
   - Metode `BuildMyQuery` dan `GetActorContextAsync` mengunci identitas ke `WorkforceProfileId` login.
   - Validasi saldo dan kuota via `LeaveRequestCalculationService.CalculateForActorAsync` menolak pengajuan dengan saldo defisit.

2. **Layanan Mandiri Lembur & Deteksi Konflik**:
   - Berkas: [OvertimePlanningService.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/OvertimeManagement/Services/OvertimePlanningService.cs)
   - Memvalidasi bendera konflik: `HasScheduleConflict`, `HasLeaveConflict`, `HasTrainingConflict`, `HasMinimumRestConflict`, dan `HasWorkHourLimitConflict`. Penugasan hanya dapat dipublikasikan jika `IsPublishable == true`.

3. **Layanan Mandiri Jadwal & Presensi**:
   - Berkas: [ShiftSwapService.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/SchedulingManagement/Services/ShiftSwapService.cs)
   - Berkas: [ScheduleChangeService.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/SchedulingManagement/Services/ScheduleChangeService.cs)
   - Berkas: [AttendanceCorrectionService.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/AttendanceManagement/Services/AttendanceCorrectionService.cs)

---

## 5. Ringkasan Kepatuhan & Status

* **Status Task**: Selesai (`PASS`) ✅
* **Acceptance Criteria 1**: Endpoint memvalidasi bahwa permohonan hanya dibuat untuk profil pegawai yang sedang terautentikasi -> **LULUS**.
* **Acceptance Criteria 2**: Validasi kuota dan saldo cuti menolak pengajuan yang melebihi hak -> **LULUS**.
* **Acceptance Criteria 3**: Pengajuan lembur mendeteksi tumpang tindih waktu (jadwal reguler, cuti, pelatihan, istirahat) -> **LULUS**.
