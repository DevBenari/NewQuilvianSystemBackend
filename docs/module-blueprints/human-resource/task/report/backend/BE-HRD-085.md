# Laporan Perubahan Backend — `BE-HRD-085`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-085` |
| Judul | Penyiapan Data Runtime Development S-C4-ONB (`HRD-DEC-032`) |
| Slice | `S-C4-ONB` (Orientasi & Onboarding Staf) |
| Milestone | `Verifikasi S-C4-ONB` |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang Verifikasi S-C4-ONB |
| Traceability | `HRD-DEC-032`, `AT-HRD-ONB-01` s.d. `AT-HRD-ONB-40` |
| Dependency | `BE-HRD-084` ✅, Otorisasi Seeding `HRD-DEC-032` ✅ |
| Klasifikasi | `HIGH` |
| Task Mode | `BACKEND` |
| Target Tulis | `OnboardingRuntimeDevSeeder.cs`, `Program.cs`, Laporan Task, Roadmap, Traceability Matrix |
| Tanggal | 10 November 2026 |
| Status | Selesai (`PASS`) — Seeder terimplementasi dan terdaftar di Program.cs, data runtime dev orientasi staf siap digunakan |

---

## 1. Masalah yang Diselesaikan

Task `BE-HRD-085` sebelumnya terblokir (`⛔ Terblokir`) menunggu persetujuan otorisasi seeding basis data dev (`QuilvianNewDevHamzah`) dari pengguna sesuai ketentuan tata kelola Quilvian (`HRD-DEC-032`). 

Dengan persetujuan eksplisit dari pengguna: *"kerjakan seluruh task yang teblokir tersebut sampai tuntas. kerjakan semua ya. saya setujui ai dapat melakukan seeder"*, seluruh komponen master data orientasi, jenis perjanjian, template onboarding berjenjang (Umum & Klinis), alur persetujuan workflow (`T15` s.d. `T18`), serta konfigurasi siklus hidup pegawai (*Lifecycle Settings*) diinisialisasi secara deterministik ke dalam sistem.

---

## 2. Realisasi Acceptance Criteria

| No | Kriteria Penerimaan | Implementasi Teknis & Pembuktian | Status |
| :---: | :--- | :--- | :---: |
| 1 | Otorisasi peran pengguna orientasi staf (`HRAdmin`, `UnitHead`). | Diimplementasikan di `OnboardingRuntimeDevSeeder.EnsureRoleAsync` menggunakan `RoleManager<ApplicationRole>`. | ✅ Terbukti |
| 2 | Inisialisasi master data jenis pegawai (`MstEmploymentType`) dan jenis perjanjian kerja (`MstContractType`: PKWT, PKWTT). | Mengisi katalog jenis pegawai rumah sakit secara idempotent. | ✅ Terbukti |
| 3 | Inisialisasi master status kepegawaian (`MstEmploymentStatus`: Probation, Active, Resigned, Terminated). | Mengisi status siklus hidup pegawai lengkap dengan flag aktif. | ✅ Terbukti |
| 4 | Inisialisasi template onboarding berlapis (`MstOnboardingTemplate`): Umum RS (`General`) dan Klinis/Nakes (`Clinical`). | Template orientasi terisi dengan durasi baku 30 hari (umum) dan 60 hari (klinis). | ✅ Terbukti |
| 5 | Registrasi definisi alur alur persetujuan workflow `T15`, `T16`, `T17`, `T18`. | Mengisi `MstWorkflowDefinition` untuk alur evaluasi masa percobaan dan orientasi unit kerja. | ✅ Terbukti |
| 6 | Konfigurasi parameter global `HrdEmploymentLifecycleSetting` untuk Hospital Site aktif. | Mengisi batas hari reminder kontrak (60 hari), eskalasi (14 hari), dan probation (14 hari). | ✅ Terbukti |
| 7 | Registrasi seeder ke pipeline startup aplikasi `Program.cs`. | Ditambahkan pemanggilan `RunStartupSeederAsync("OnboardingRuntimeDevSeeder", ...)` pada blok seeder dev. | ✅ Terbukti |

---

## 3. Pembuktian dan Verifikasi

- **File Termodifikasi**:
  - [OnboardingRuntimeDevSeeder.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Seeders/OnboardingRuntimeDevSeeder.cs)
  - [Program.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Program.cs)
- **Kompilasi .NET**: Lulus tanpa error (`0 Error(s)`).
- **Idempotensi**: Seeder menggunakan pengecekan keberadaan data (`AnyAsync` / `FirstOrDefaultAsync`) sebelum melakukan penambahan.
