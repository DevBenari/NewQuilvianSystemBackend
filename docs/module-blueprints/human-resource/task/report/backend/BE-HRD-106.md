# Laporan Perubahan Backend — `BE-HRD-106`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `BE-HRD-106` |
| Judul | Engine Pemrosesan Absensi Harian & Normalisasi Shift Malam Lintas Hari |
| Slice | S-B1 (AttendanceManagement) |
| Milestone | MVP-21 |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` |
| Traceability | HRD-DEC-104, HRD-DEC-105, HRD-DEC-106, VR-ATT-01 s.d. VR-ATT-04, State Transition Matrix v11 Bagian 15.1, AT-HRD-ATT-01 s.d. AT-HRD-ATT-04 |
| Dependency | `BE-HRD-105` ✅ |
| Klasifikasi | `CORE DAILY ATTENDANCE CALCULATION & OVERNIGHT SHIFT PROCESSING ENGINE` |
| Task Mode | `BACKEND` |
| Tanggal | 05 November 2026 |
| Status | Selesai (`PASS`) |

---

## 1. Ringkasan dan Masalah yang Diselesaikan

Di rumah sakit yang beroperasi 24 jam non-stop, terdapat tantangan fundamental dalam pengolahan data absensi biometrik:
1. **Shift Malam Lintas Hari (*Cross-Midnight Shift*, `HRD-DEC-104`)**: Perawat atau dokter jaga malam tap masuk pada pukul 21:00 (Hari H) dan tap keluar pada pukul 07:00 keesokan harinya (Hari H+1). Jika sistem presensi memotong data berdasarkan pergantian hari kalender tengah malam (00:00), maka tap masuk malam dianggap pulang tanpa absen keluar di hari H, dan tap keluar pagi dianggap masuk terlambat di hari H+1. Hal ini merupakan kesalahan perhitungan fatal bagi operasional rumah sakit.
2. **Evaluasi Toleransi Keterlambatan Grace Period (`HRD-DEC-105`)**: Rumah sakit memberlakukan masa toleransi 10 menit. Jika staf hadir dalam rentang 10 menit setelah jam mulai shift, staf dianggap Hadir Tepat Waktu (`Present`, `LateMinutes = 0`). Namun jika staf hadir pada menit ke-11 atau lebih, keterlambatan dihitung penuh dari jam jadwal mulai resmi (`LateMinutes = 11`, `Status = Late`).
3. **Perekaman Lupa Tap / Tap Tunggal (*IncompletePunch*, `HRD-DEC-106`)**: Jika pegawai hanya melakukan 1 kali tap (hanya tap masuk atau hanya tap keluar), sistem tidak boleh langsung memvonis mangkir atau hadir penuh, melainkan memberi status sementara `IncompletePunch` dengan alasan yang tercatat jelas agar dapat dikoreksi sebelum cut-off penggajian.

Task `BE-HRD-106` menyelesaikan seluruh persoalan ini di dalam mesin pengolahan presensi:
- **Penguncian Tanggal Shift Malam ke Hari H**: Log tap masuk malam hari H dan tap keluar pagi hari H+1 dipasangkan ke satu rekaman penugasan shift harian dengan `AttendanceDate = Hari H` (`HRD-DEC-104`).
- **Penerapan Logika Toleransi 10 Menit**: Mengkalkulasi ambang batas keterlambatan `lateThreshold = ScheduledStartAt + 10 menit`. Jika `FirstCheckIn <= lateThreshold`, maka `LateMinutes = 0`. Jika `FirstCheckIn > lateThreshold`, maka `LateMinutes = (FirstCheckIn - ScheduledStartAt).TotalMinutes` (`HRD-DEC-105`).
- **Status Sementara `IncompletePunch`**: Saat `daily.HasMissingPunch` bernilai `true`, status harian disetel ke `IncompletePunch` dan kolom `IncompletePunchReason` diisi secara otomatis (`HRD-DEC-106`).
- **Pengakuan Status Dinas Luar (`OnDuty`)**: Mendukung pengakuan kehadiran tugas dinas luar (`HRD-DEC-109`).

---

## 2. Alur Evaluasi dan Pemrosesan Absensi Harian

```mermaid
flowchart TD
    subgraph InputData["1. Input Data & Jadwal"]
        A["Log Mentah Biometrik (EventAt)"] --> C["Schedule Resolver & Capture Window"]
        B["Roster Terbit / Shift Pattern"] --> C
    end

    subgraph NormalisasiOvernight["2. Normalisasi Shift Malam Lintas Hari (HRD-DEC-104)"]
        C --> D{"Apakah Shift Malam Lintas Hari?"}
        D -->|Ya| E["Kunci ShiftDate = Tanggal Mulai (Hari H)<br/>Rentang Tangkap: s.d. Pagi Hari H+1"]
        D -->|Tidak| F["ShiftDate = Tanggal Kalender Hari H"]
    end

    subgraph EvaluasiKeterlambatan["3. Evaluasi Grace Period (HRD-DEC-105)"]
        E --> G{"Waktu Tap Masuk <= Jam Mulai + 10 Menit?"}
        F --> G
        G -->|Ya (<= 10 mnt)| H["LateMinutes = 0, Status: Present"]
        G -->|Tidak (> 10 mnt)| I["LateMinutes = ActualCheckIn - ScheduledStart<br/>Status: Late"]
    end

    subgraph DeteksiTapTunggal["4. Deteksi Tap Tunggal (HRD-DEC-106)"]
        H --> J{"Kelengkapan Tap (Masuk & Keluar)?"}
        I --> J
        J -->|Lengkap (2 Tap)| K["Hitung ActualWorkMinutes & Selesai"]
        J -->|Hanya 1 Tap (Missing Punch)| L["Status: IncompletePunch<br/>Catat IncompletePunchReason"]
    end
```

---

## 3. Komponen Perubahan yang Diterapkan

1. **Konstanta Nilai Status (`AttendanceValueConstants.cs`)**:
   - Menambahkan konstanta resmi `AttendanceStatus.IncompletePunch = "IncompletePunch"` dan `AttendanceStatus.OnDuty = "OnDuty"`.
2. **Mesin Pemrosesan (`AttendanceProcessingService.cs`)**:
   - Metode `BuildCaptureWindow`: Memperpanjang rentang penangkapan log mentah untuk shift malam lintas hari hingga 8 jam pasca `ScheduledEndAt` (pagi hari H+1), menjaga `AttendanceDate` tetap terkunci pada tanggal hari H.
   - Metode `EvaluateAndApplyAttendanceRules`:
     - Menghitung batas toleransi keterlambatan 10 menit secara otomatis.
     - Mengisi `IncompletePunchReason` ketika terjadi punch tunggal.
     - Menandai status `OnDuty` ketika terhubung dengan penugasan dinas luar.

---

## 4. Bukti Verifikasi dan Pengujian

- **Kasus Uji Penerimaan Terpenuhi**:
  - `AT-HRD-ATT-01`: Tap masuk jam 21:00 hari H dan tap keluar 07:00 hari H+1 terkunci sempurna pada tanggal shift hari H (`HRD-DEC-104`).
  - `AT-HRD-ATT-02`: Kedatangan dengan selisih <= 10 menit menghasilkan `LateMinutes = 0` dan berstatus `Present` (`HRD-DEC-105`).
  - `AT-HRD-ATT-03`: Kedatangan pada menit ke-11 menghasilkan `LateMinutes = 11` dan berstatus `Late` (`HRD-DEC-105`).
  - `AT-HRD-ATT-04`: Kehadiran yang hanya memiliki 1 kali tap diberi status `IncompletePunch` dengan alasan yang tercatat (`HRD-DEC-106`).
