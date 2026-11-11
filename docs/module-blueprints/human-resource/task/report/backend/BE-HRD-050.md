# Laporan Perubahan Backend — `BE-HRD-050`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-050` |
| Judul | Penyiapan Data Runtime Development S-D1 (`HRD-DEC-032`) |
| Slice | `S-D1` (Perencanaan Tenaga Kerja / Workforce Planning) |
| Milestone | `Verifikasi S-D1` |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang Verifikasi S-D1 |
| Traceability | `HRD-DEC-032`, `AT-HRD-D1-01` s.d. `AT-HRD-D1-21` |
| Dependency | `BE-HRD-049` ✅, Otorisasi Seeding `HRD-DEC-032` ✅ |
| Klasifikasi | `HIGH` |
| Task Mode | `BACKEND` |
| Target Tulis | `WorkforcePlanningRuntimeDevSeeder.cs`, `Program.cs`, Laporan Task, Roadmap, Traceability Matrix |
| Tanggal | 10 November 2026 |
| Status | Selesai (`PASS`) — Seeder terimplementasi dan terdaftar di Program.cs, data runtime dev siap digunakan |

---

## 1. Masalah yang Diselesaikan

Sebelumnya, task `BE-HRD-050` berstatus terblokir (`⛔ Terblokir`) karena menunggu persetujuan eksplisit dari pengguna terkait eksekusi seeder pada basis data dev (`QuilvianNewDevHamzah`) sesuai ketentuan tata kelola Quilvian (`HRD-DEC-032`). 

Setelah pengguna memberikan persetujuan eksplisit: *"kerjakan seluruh task yang teblokir tersebut sampai tuntas. kerjakan semua ya. saya setujui ai dapat melakukan seeder"*, seluruh komponen master data, peran otorisasi, dan alur workflow yang dibutuhkan untuk menjalankan modul Workforce Planning (`S-D1`) di lingkungan runtime dev diinisialisasi secara deterministik dan idempotent.

---

## 2. Realisasi Acceptance Criteria

| No | Kriteria Penerimaan | Implementasi Teknis & Pembuktian | Status |
| :---: | :--- | :--- | :---: |
| 1 | Seeder peran otorisasi pengguna (`HospitalDirector`, `HRManager`, `FinanceManager`, `Recruiter`). | Diimplementasikan di `WorkforcePlanningRuntimeDevSeeder.EnsureRoleAsync` menggunakan `RoleManager<ApplicationRole>`. | ✅ Terbukti |
| 2 | Registrasi definisi alur kerja workflow `T9`, `T10`, `T11`, `T12`, `T13` beserta hierarki langkah persetujuan. | Mengisi `MstWorkflowDefinition` dengan kode modul `WorkforcePlanning` dan langkah verifikasi lengkap. | ✅ Terbukti |
| 3 | Pengaturan parameter global `HrdWorkforcePlanningSetting` untuk Hospital Site aktif. | Mengisi batas hari `LongLeaveThresholdDays` (30 hari) dan `OverdueRequestThresholdDays` (14 hari). | ✅ Terbukti |
| 4 | Master tabel gaji tengah (`MstSalaryGrade`) untuk kalkulasi biaya formasi (`BE-HRD-038`). | Mengisi katalog kelas jabatan standar rumah sakit dari Grade I hingga Grade V. | ✅ Terbukti |
| 5 | Registrasi seeder ke pipeline startup aplikasi `Program.cs`. | Ditambahkan pemanggilan `RunStartupSeederAsync("WorkforcePlanningRuntimeDevSeeder", ...)` pada blok seeder dev. | ✅ Terbukti |

---

## 3. Pembuktian dan Verifikasi

- **File Termodifikasi**:
  - [WorkforcePlanningRuntimeDevSeeder.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Seeders/WorkforcePlanningRuntimeDevSeeder.cs)
  - [Program.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Program.cs)
- **Kompilasi .NET**: Lulus tanpa error (`0 Error(s)`).
- **Idempotensi**: Seeder menggunakan pengecekan keberadaan data (`AnyAsync` / `FirstOrDefaultAsync`) sebelum melakukan penambahan, sehingga aman dijalankan berulang kali.
