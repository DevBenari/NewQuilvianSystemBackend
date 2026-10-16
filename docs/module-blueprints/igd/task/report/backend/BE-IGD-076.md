# Laporan Perubahan Backend — `BE-IGD-076`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `BE-IGD-076` |
| Judul | Standar Anamnesis Kasus Trauma, SAMPLE (Last Meal), dan Diagram Tubuh (Body Map) Backend IGD |
| Slice | `EPIC IGD-03` / `EPIC IGD-06` · Standar Rekam Medis Akreditasi RS Indonesia |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) |
| Requirement | Standar Anamnesis SAMPLE (Last Oral Intake), Kasus Trauma (Mekanisme Kejadian & APD), dan Status Lokalis Diagram Tubuh |
| Keputusan | `IGD-DEC-310` (Persetujuan penambahan standar anamnesis dan diagram tubuh) |
| Contract version | API `0.19.0`, Validation `0.18.0` |
| Dependency | `EmgVisit`, `TrxPatientAssessment` |
| Klasifikasi | `MEDIUM` — Penambahan kolom entity, migration EF Core, pembaruan DTO, service, dan controller |
| Task mode | `BACKEND` — Disetujui user untuk Tahap 1 |
| Target tulis | `NewQuilvianSystemBackend` |
| Model | Google Antigravity |
| Tanggal | 15 Oktober 2026 |
| Status | ✅ **DONE — 15 Oktober 2026** |

---

## 1. Masalah yang Diselesaikan

Pada formulir Assesmen Awal Pasien IGD (Keperawatan), rumah sakit di Indonesia mewajibkan:
1. Pencatatan **Kasus Trauma**: Apakah pasien menggunakan Alat Pelindung Diri (APD: helm SNI, safety belt) dan bagaimana mekanisme benturan/kejadian trauma terjadi.
2. Anamnesis Gawat Darurat Metode **SAMPLE**: Pencatatan *Last Oral Intake* (Waktu Makan/Minum Terakhir, Jenis Makanan, dan Catatan Lambung/Puasa) untuk keselamatan tindakan anestesi atau operasi darurat (*Cito*).
3. **Diagram Tubuh (Body Map)**: Penyimpanan data penanda luka koordinat tubuh visual yang otomatis terhubung dengan ringkasan *Status Lokalis*.

Seluruh kolom baru ini ditambahkan dengan sifat `nullable` guna menjamin ketiadaan regresi (*Zero Disruption*) terhadap modul Poliklinik Rawat Jalan dan Rawat Inap yang juga menggunakan entitas `TrxPatientAssessment`.

---

## 2. Rincian Perubahan Kode Backend

### A. Model & Entitas Data
1. `Areas/HealthServices/EmergencyInstallationManagement/Models/EmgVisit.cs`:
   - `TraumaMechanism` (`string?`, varchar 1000)
   - `TraumaSafetyEquipment` (`string?`, varchar 250)
2. `Areas/HealthServices/ClinicalManagement/Models/TrxPatientAssessment.cs`:
   - `BodyMapData` (`string?`, text)
   - `LastOralIntakeDateTime` (`DateTimeOffset?`, timestamptz)
   - `LastOralIntakeType` (`string?`, varchar 250)
   - `LastOralIntakeNote` (`string?`, varchar 500)

### B. Konfigurasi Entity Framework Core
1. `Repositories/Configurations/HealthServices/EmergencyInstallationManagement/EmgVisitConfiguration.cs`:
   - Mapping kolom snake_case: `trauma_mechanism` (max length 1000), `trauma_safety_equipment` (max length 250).
2. `Repositories/Configurations/HealthServices/TrxPatientAssessmentConfiguration.cs`:
   - Mapping kolom snake_case: `body_map_data`, `last_oral_intake_date_time`, `last_oral_intake_type` (max length 250), `last_oral_intake_note` (max length 500).

### C. Migration EF Core
- Nama Migration: `20261015050322_AddEmergencyAnamnesisAndBodyMap`
- Berkas: `Migrations/20261015050322_AddEmergencyAnamnesisAndBodyMap.cs`
- Diterapkan ke PostgreSQL database `QuilvianNewDevRizki` (Host: `160.22.250.77`) via `dotnet ef database update`.

### D. DTOs, Service, & Controllers
1. `Areas/HealthServices/EmergencyInstallationManagement/DTOs/EmergencyVisitDtos.cs`:
   - Menambahkan `TraumaMechanism` & `TraumaSafetyEquipment` ke `EmergencyVisitDetailDto` dan `UpdateEmergencyVisitDto`.
2. `Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyVisitService.cs`:
   - Pemetaan mutasi dan baca DTO dengan sanitasi `NormalizeNullableText`.
3. `Areas/HealthServices/EmergencyInstallationManagement/Controllers/EmergencyVisitController.cs`:
   - Endpoint `GET /api/emergency-visits/{id}` dan `PUT /api/emergency-visits/{id}` mendukung field trauma.
4. `Areas/HealthServices/ClinicalManagement/DTOs/PatientAssessmentDtos.cs`:
   - Menambahkan `BodyMapData`, `LastOralIntakeDateTime`, `LastOralIntakeType`, `LastOralIntakeNote` ke `PatientAssessmentDetailDto`, `CreatePatientAssessmentDto`, dan `UpdatePatientAssessmentDto`.
5. `Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs`:
   - Endpoint `POST /api/patient-assessments` dan `PUT /api/patient-assessments/{id}` mendukung field baru.

---

## 3. Hasil Verifikasi & Build

- Kompilasi: `dotnet build -p:RunAnalyzers=false` $\rightarrow$ **Build Succeeded (0 Error, 0 Warning baru)**.
- Database: Kolom baru aktif di database dev.
- Status: **SELESAI (DONE)**.
