# Laporan Perubahan Backend — `BE-HRD-097`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `BE-HRD-097` |
| Judul | Service Koreksi Mundur Jadwal (<= 3 Hari) & Event Rekalkulasi Absensi (`INT-HR-36`) |
| Slice | S-B4 (SchedulingManagement) |
| Milestone | MVP-19 |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` |
| Traceability | HRD-DEC-102, HRD-ASM-13, INT-HR-36, VR-SCH-17, 02-backend-architecture.md Bagian 19.9.4, AT-HRD-SCH-16, AT-HRD-SCH-17 |
| Dependency | `BE-HRD-088` ✅, `BE-HRD-095` ✅ |
| Klasifikasi | `CORE RETROACTIVE SCHEDULING & ATTENDANCE INTEGRATION` |
| Task Mode | `BACKEND` |
| Tanggal | 05 November 2026 |
| Status | Selesai (`PASS`) — Roslyn Build 0 Error |

---

## 1. Ringkasan dan Masalah yang Diselesaikan

Di rumah sakit, staf sering kali bertukar jam shift dinas secara mendadak pada malam hari karena kondisi darurat pelayanan (misal: perawat shift pagi diperbantukan ke shift malam saat ada insiden kecelakaan massal), namun Kepala Unit baru sempat mencatatkannya ke dalam sistem administrasi keesokan harinya atau 2 hari kemudian.

Jika sistem melarang seluruh perubahan pada tanggal yang sudah lewat, data jadwal kerja tidak akan mencerminkan kenyataan fisik, dan modul absensi (Sub-Modul 07) akan mencatat staf tersebut sebagai tidak hadir (*absent*) atau terlambat parah. Sebaliknya, jika koreksi mundur dibiarkan tanpa batas, rekapitulasi penggajian (*payroll*) yang sudah dibayarkan di bulan sebelumnya dapat rusak.

Task `BE-HRD-097` menghadirkan solusi seimbang berbasis aturan `HRD-DEC-102` & `VR-SCH-17`:
1. **Jendela Koreksi Mundur Mandiri (Maksimal 3 Hari Kalender)**: Kepala Unit diizinkan mengoreksi jadwal dinas tanggal mundur maksimal 3 hari ke belakang (`today - shiftDate <= 3`) dengan alasan koreksi wajib.
2. **Penolakan Koreksi > 3 Hari**: Upaya koreksi mundur lebih dari 3 hari ke belakang ditolak otomatis oleh sistem (HTTP 422 Unprocessable Entity) untuk mencegah manipulasi historis dan melindungi integritas cut-off payroll.
3. **Pemicu Rekalkulasi Absensi (`INT-HR-36`)**: Setiap kali koreksi mundur jadwal berhasil disimpan, sistem secara otomatis memicu event integrasi `RetroactiveShiftCorrectedEvent` ke Sub-Modul 07 (Presensi & Kehadiran) agar data keterlambatan, jam kerja efektif, dan kehadiran harian staf langsung dihitung ulang berdasarkan shift baru.

---

## 2. Alur Proses Bisnis Koreksi Mundur Jadwal

```mermaid
flowchart TD
    A["Kepala Unit Mengajukan Koreksi Jadwal Tanggal yang Telah Lewat"] --> B["PUT /shift-assignments/{id}/retroactive-correct"]
    B --> C["Hitung Selisih Hari: daysBack = today - shiftDate"]
    
    C --> D{"Periode Roster Berstatus Closed?"}
    D -->|Ya| E["Tolak (HTTP 403 Forbidden): Periode Roster Terkunci Mutlak"]
    D -->|Tidak| F{"daysBack > 3 Hari Kalender?"}
    
    F -->|Ya (> 3 Hari)| G["Tolak (HTTP 422): Koreksi Mundur > 3 Hari Ditolak bagi Kepala Unit (VR-SCH-17)"]
    F -->|Tidak (<= 3 Hari)| H["Ambil Master Shift Baru (MstShift)"]
    
    H --> I["Hitung Ulang ScheduledStartAt, ScheduledEndAt, & PlannedWorkMinutes"]
    I --> J["Update TrxShiftAssignment: IsManualOverride = True & Simpan Alasan Wajib"]
    J --> K["Commit Perubahan ke Database"]
    K --> L["Picukan Event INT-HR-36: RetroactiveShiftCorrectedEvent"]
    L --> M["Sub-Modul 07 (AttendanceManagement) Merekalkulasi Kehadiran Harian Staf"]
    M --> N["Kembalikan Response 200 OK: Jadwal Terkoreksi & Evaluasi Absensi Berhasil"]
```

---

## 3. Komponen Perubahan yang Diterapkan

1. **Metode Service (`ShiftReplacementService.RetroactiveCorrectionAsync` & `ShiftAssignmentService.RetroactiveCorrectionAsync`)**:
   - Menghitung nilai `daysBack = today.DayNumber - shiftAssignment.ShiftDate.DayNumber`.
   - Menegakkan batas validasi `VR-SCH-17` (menolak bila `daysBack > 3`).
   - Menghitung ulang waktu mulai, selesai, dan durasi kerja shift secara presisi sesuai master shift baru.
   - Mengisi `IsManualOverride = true`, alasan koreksi, dan mencatat `AssignmentSource = "RetroactiveCorrection"`.
   - Memicu event terstruktur `INT-HR-36` ke sistem pencocokan presensi.
2. **DTO Pendukung (`RosterSchedulingDtos.cs`)**:
   - `RetroactiveShiftCorrectionRequest`: Menerima `NewShiftId`, `ReasonCategory`, dan `Notes`.
   - `RetroactiveShiftCorrectionResponse`: Mengembalikan `ShiftAssignmentId`, flag `IsRetroactive`, hitungan `DaysBack`, penanda `AttendanceReevaluationTriggered = true`, dan pesan sukses.
3. **Endpoint Controller (`ShiftReplacementController.cs`)**:
   - `PUT /api/corporate/human-resource/scheduling/shift-assignments/{id}/retroactive-correct`
   - `POST /api/corporate/human-resource/scheduling/shift-assignments/{id}/retroactive-correct` (alias dukungan POST).

---

## 4. Spesifikasi Endpoint API Bergaya Swagger

| Method | Endpoint Path | Deskripsi | Auth / Role | Status |
| :--- | :--- | :--- | :--- | :--- |
| `PUT` | `/api/corporate/human-resource/scheduling/shift-assignments/{id}/retroactive-correct` | Mengoreksi jadwal tanggal mundur (<= 3 hari) dan memicu rekalkulasi absensi | Unit Head / Update | 200 OK / 422 Unprocessable |
| `POST` | `/api/corporate/human-resource/scheduling/shift-assignments/{id}/retroactive-correct` | Endpoint alias untuk koreksi mundur jadwal dinas staf | Unit Head / Update | 200 OK / 422 Unprocessable |

---

## 5. Bukti Verifikasi dan Pengujian

- **Kompilasi Roslyn**: Lulus `dotnet build NewQuilvianSystemBackend/QuilvianSystemBackend.csproj /t:Compile` dengan **0 Error(s)**.
- **Kasus Uji Penerimaan (`AT-HRD-SCH-16` & `AT-HRD-SCH-17`)**:
  - Terbukti koreksi mundur <= 3 hari berhasil disimpan dan memicu event `INT-HR-36`.
  - Terbukti koreksi mundur > 3 hari ditolak sistem dengan pesan kesalahan spesifik.
