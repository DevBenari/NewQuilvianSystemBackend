# Laporan Perubahan Backend — `BE-HRD-008`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `BE-HRD-008` |
| Judul | Administrasi Lembur, Verifikasi Surat Perintah Kerja Lembur (SPKL), dan Rekonsiliasi Penggajian |
| Slice | `S-B3` (Administrasi Lembur, Verifikasi SPKL, & Rekonsiliasi) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang 4 |
| Trace | `HRD-DEC-007`, `IN-09` |
| Contract version | API Contract v1 (Draft) |
| Dependency | `BE-HRD-006` (Selesai ✅) |
| Klasifikasi | `HIGH` |
| Task mode | `BACKEND` |
| Target verifikasi | `OvertimeVerificationController.cs`, `OvertimeVerificationService.cs`, `OvertimeReconciliationController.cs`, `OvertimeFinalReconciliationService.cs`, `OvertimeRateResolverService.cs`, `OvertimeAttendanceMatchingService.cs`, `OvertimePayrollHandoffController.cs` |
| Model | Gemini 3.8 Flash (High) |
| Tanggal | 27 Oktober 2026 |
| Status | Selesai (`PASS`) |

---

## 1. Masalah yang diperbaiki & diverifikasi

1. **Penegakan Gerbang Persetujuan Supervisor sebelum Lembur Masuk ke Payroll**:
   Sebelumnya, tanpa gerbang verifikasi resmi, jam lembur yang diajukan staf rawan langsung dibayarkan meskipun belum mendapatkan persetujuan dari supervisor/kepala ruangan. Melalui `OvertimeVerificationService`, hanya realisasi lembur yang telah berstatus `Approved` dan terbukti sinkron dengan log kehadiran mesin presensi (`IsAttendanceMatched == true`) yang berhak diverifikasi (*eligible*).
2. **Kalkulasi Pengali Lembur Sesuai Tipe Hari (Hari Kerja vs Hari Libur)**:
   Sesuai regulasi Depnaker dan kebijakan rumah sakit, perhitungan upah lembur memiliki rumus pengali bertingkat:
   - Hari kerja biasa: 1,5× upah per jam untuk jam pertama, dan 2,0× untuk jam-jam berikutnya.
   - Hari libur mingguan / libur nasional: 2,0×, 3,0×, hingga 4,0× upah per jam tergantung akumulasi durasi.
   `OvertimeRateResolverService` memvalidasi dan menyelesaikan pengali lembur (`RateMultiplier`) berbasis tipe kalender kerja rumah sakit secara deterministik.
3. **Penyelarasan Selisih Jam Lembur (*Variance Adjustment*) & Serah Terima ke Payroll**:
   Jika terdapat perbedaan antara jam yang direncanakan di SPKL dan jam aktual yang terekam di mesin absensi, HR Verifier dapat menetapkan `VerifiedMinutes` beserta alasan penyesuaian sebelum status resmi dikunci menjadi `Verified` dan diteruskan ke putaran penggajian (*payroll handoff*).

---

## 2. Proses bisnis & alur kerja rumah sakit

```
[Staf Medis / Non-Medis Selesai Lembur]
                    │
                    ▼
[1. Pengajuan Realisasi Lembur Mandiri / SPKL] ──► Status: Submitted
                    │
                    ▼
[2. Persetujuan Kepala Ruangan / Supervisor] ──► Status: Approved
                    │
                    ▼
[3. Verifikasi Administrasi oleh HRD] ──► POST /overtime/verifications/items/{id}/verify
   ├─► Cek kecocokan log mesin absensi (IsAttendanceMatched)
   ├─► Hitung faktor pengali hari kerja / libur (RateMultiplier)
   └─► Tentukan menit final layak bayar (VerifiedMinutes)
                    │
                    ▼
[4. Rekonsiliasi & Finalisasi HR] ──► Status: Verified
                    │
                    ▼
[5. Serah Terima Idempoten ke Modul Payroll] ──► POST /overtime/payroll-handoffs
```

* **Tujuan**: Memastikan keabsahan setiap jam lembur staf rumah sakit, kesesuaian anggaran operasional keperawatan/medis, dan kepatuhan terhadap regulasi ketenagakerjaan.
* **Pelaku**: Pegawai Lembur, Kepala Ruangan / Supervisor, HR Overtime Verifier, Staf Payroll.
* **Pemicu**: Operasi darurat di luar jam dinas, lonjakan pasien rawat inap / IGD, atau penugasan khusus direksi.
* **Langkah & Aturan**:
  1. Realisasi lembur yang belum disetujui supervisor ditandai tidak layak (`Ineligible`) dan ditolak untuk diposting ke payroll.
  2. Perhitungan pengali jam lembur membedakan secara ketat hari kerja biasa dan hari libur resmi.
  3. Realisasi lembur yang telah diposting ke payroll (`PostedToPayroll`) dikunci secara permanen dan tidak dapat diverifikasi ulang tanpa prosedur pembatalan resmi.

---

## 3. Spesifikasi Endpoint API (Gaya Swagger)

### Grup: `Corporate / Human Resource / Overtime Management / Overtime Verification`
Tag: `[Tags("Corporate / Human Resource / Overtime Management / Overtime Verification")]`

| Method | Path | Deskripsi | Auth / Permission | Request Body / Query | Response Model |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `api/v1/corporate/human-resource/overtime/verifications` | Mengambil daftar realisasi lembur yang menunggu verifikasi HR | `OvertimeVerification:Read` | `OvertimeVerificationPagedQueryRequest` | `ApiResponse<PagedResult<OvertimeVerificationListResponse>>` |
| `GET` | `api/v1/corporate/human-resource/overtime/verifications/{id}` | Mengambil detail verifikasi lembur beserta bukti presensi | `OvertimeVerification:Read` | `id` (Guid) | `ApiResponse<OvertimeVerificationDetailResponse>` |
| `POST` | `api/v1/corporate/human-resource/overtime/verifications/items/{id}/verify` | Menyetujui dan menetapkan menit lembur terverifikasi | `OvertimeVerification:Verify` | `VerifyOvertimeDetailRequest` | `ApiResponse<OvertimeVerificationActionResponse>` |
| `POST` | `api/v1/corporate/human-resource/overtime/verifications/items/{id}/reject` | Menolak realisasi lembur (tidak ada bukti absensi / tidak sah) | `OvertimeVerification:Reject` | `RejectOvertimeDetailRequest` | `ApiResponse<OvertimeVerificationActionResponse>` |

### Grup: `Corporate / Human Resource / Overtime Management / Overtime Payroll Handoff`
Tag: `[Tags("Corporate / Human Resource / Overtime Management / Overtime Payroll Handoff")]`

| Method | Path | Deskripsi | Auth / Permission | Request Body / Query | Response Model |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `POST` | `api/v1/corporate/human-resource/overtime/payroll-handoffs` | Menyerahkan batch lembur berstatus Verified ke modul Payroll | `OvertimePayrollHandoff:Create` | `CreateOvertimePayrollHandoffRequest` | `ApiResponse<OvertimePayrollHandoffResponse>` (201 Created) |

---

## 4. Bukti implementasi kode

1. **Controller Verifikasi Lembur & Payroll Handoff**:
   - Berkas: [OvertimeVerificationController.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/OvertimeManagement/Controllers/OvertimeVerificationController.cs)
   - Berkas: [OvertimePayrollHandoffController.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/OvertimeManagement/Controllers/OvertimePayrollHandoffController.cs)

2. **Domain Service Verifikasi & Perhitungan Pengali**:
   - Berkas: [OvertimeVerificationService.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/OvertimeManagement/Services/OvertimeVerificationService.cs)
   - Berkas: [OvertimeRateResolverService.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/OvertimeManagement/Services/OvertimeRateResolverService.cs)
   - Berkas: [OvertimeAttendanceMatchingService.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/OvertimeManagement/Services/OvertimeAttendanceMatchingService.cs)

---

## 5. Ringkasan Kepatuhan & Status

* **Status Task**: Selesai (`PASS`) ✅
* **Acceptance Criteria 1**: Realisasi lembur yang belum disetujui supervisor ditandai tidak layak diperhitungkan dalam payroll run -> **LULUS**.
* **Acceptance Criteria 2**: Rekonsiliasi jam lembur menghitung faktor pengali hari kerja dan hari libur secara tepat (`RateMultiplier`) -> **LULUS**.
