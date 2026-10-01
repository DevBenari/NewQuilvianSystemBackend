# Laporan Task Backend: BE-RWI-134 — Pembuatan Entitas Basis Data & Seeder Master Data SDKI, SLKI, dan SIKI

## 1. Identitas Task
- **Task ID**: `BE-RWI-134`
- **Modul**: Pelayanan Kesehatan — Rawat Inap (*Inpatient Nursing Workspace*)
- **Sub-Modul**: Standarisasi Data Klinis Asuhan Keperawatan (PPNI: SDKI, SLKI, SIKI) — Menutup Keputusan Terbuka `OQ-RI-011` / `RWI-DEC-090`
- **Area / Module**: `HealthServices` / `MasterData` (Prefix: `Mst`)
- **Status Database**: Entitas master baru terdaftar di `ApplicationDbContext` (schema `public`)
- **Status**: **SELESAI (100% Implemented & Verified)**

---

## 2. Ringkasan Perubahan

### A. Pembuatan 5 Model Entitas Master Data 3S PPNI
Berkas baru pada `Areas/HealthServices/MasterData/Models/`:
1. **`MstNursingDiagnosisGroup.cs`**:
   - Menampung kelompok diagnosis SDKI: Fisiologis (`KAT-FIS`), Psikologis (`KAT-PSI`), Perilaku (`KAT-PRL`), Relasional (`KAT-REL`), dan Lingkungan (`KAT-LING`).
2. **`MstNursingDiagnosis.cs`**:
   - Menampung katalog utama diagnosis keperawatan SDKI (misal: `D.0077` Nyeri Akut, `D.0130` Hipertermia, `D.0001` Bersihan Jalan Napas Tidak Efektif).
   - Terhubung dengan `NursingDiagnosisId` pada `CliNursingCarePlanItem`.
3. **`MstNursingDiagnosisEtiology.cs`**:
   - Menampung faktor penyebab yang berhubungan (`b.d`) untuk masing-masing diagnosa SDKI.
4. **`MstNursingDiagnosisOutcome.cs`**:
   - Menampung Standar Luaran Keperawatan Indonesia (SLKI) resmi PPNI (kode `L.xxxxx`, nama luaran, dan kriteria ekspektasi/hasil evaluasi).
5. **`MstNursingDiagnosisIntervention.cs`**:
   - Menampung Standar Intervensi Keperawatan Indonesia (SIKI) resmi PPNI (kode `I.xxxxx`) yang dikelompokkan ke dalam 4 pilar utama:
     - `PillarType = 1`: Observasi
     - `PillarType = 2`: Terapeutik
     - `PillarType = 3`: Edukasi
     - `PillarType = 4`: Kolaborasi

### B. Konfigurasi Fluent API Entity Framework Core
Berkas baru pada `Repositories/Configurations/HealthServices/`:
- `MstNursingDiagnosisGroupConfiguration.cs`
- `MstNursingDiagnosisConfiguration.cs`
- `MstNursingDiagnosisEtiologyConfiguration.cs`
- `MstNursingDiagnosisOutcomeConfiguration.cs`
- `MstNursingDiagnosisInterventionConfiguration.cs`

Menerapkan konvensi database PostgreSQL:
- Skema tabel `public`
- Indeks unik pada kode diagnosa (`Code`) dan kode kelompok (`GroupCode`)
- Indeks pencarian pada nama diagnosa (`Name`)
- Hubungan referensial dengan relasi *cascade delete* dari diagnosa induk ke etiologi, luaran, dan intervensi.

### C. Pendaftaran DbSet pada DbContext
- Berkas: `Repositories/ApplicationDbContext.cs`
- Menambahkan 5 `DbSet`:
  - `MstNursingDiagnosisGroups`
  - `MstNursingDiagnoses`
  - `MstNursingDiagnosisEtiologies`
  - `MstNursingDiagnosisOutcomes`
  - `MstNursingDiagnosisInterventions`

### D. Pembuatan Seeder 10 Diagnosa Prioritas Rawat Inap
Berkas baru: `Areas/HealthServices/MasterData/Seeders/MstNursingDiagnosisSeeder.cs`:
- Mengisi 5 Kategori SDKI utama.
- Mengisi 10 diagnosis prioritas ruang rawat inap lengkap dengan etiologi, luaran SLKI, dan 4 pilar intervensi SIKI bawaan:
  1. `D.0077` — Nyeri Akut (SLKI: `L.08066`, SIKI: `I.08238` Manajemen Nyeri 4 Pilar)
  2. `D.0130` — Hipertermia (SLKI: `L.14134`, SIKI: `I.15506` Manajemen Hipertermia 4 Pilar)
  3. `D.0001` — Bersihan Jalan Napas Tidak Efektif (SLKI: `L.01001`, SIKI: `I.01006` Latihan Batuk Efektif 4 Pilar)
  4. `D.0005` — Pola Napas Tidak Efektif (SLKI: `L.01004`, SIKI: `I.01011` Manajemen Jalan Napas 4 Pilar)
  5. `D.0143` — Risiko Jatuh (SLKI: `L.14138`, SIKI: `I.14540` Pencegahan Jatuh 4 Pilar)
  6. `D.0142` — Risiko Infeksi (SLKI: `L.14137`, SIKI: `I.14539` Pencegahan Infeksi 4 Pilar)
  7. `D.0019` — Defisit Nutrisi (SLKI: `L.03030`, SIKI: `I.03119` Manajemen Nutrisi 4 Pilar)
  8. `D.0056` — Intoleransi Aktivitas (SLKI: `L.05047`, SIKI: `I.05178` Manajemen Energi 4 Pilar)
  9. `D.0129` — Gangguan Integritas Kulit/Jaringan (SLKI: `L.14125`, SIKI: `I.14564` Perawatan Integritas Kulit 4 Pilar)
  10. `D.0023` — Hipovolemia (SLKI: `L.03028`, SIKI: `I.03116` Manajemen Hipovolemia 4 Pilar)

---

## 3. Bukti Verifikasi
- Struktur model C# sesuai dengan konvensi `IdentityModel` dan `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` (Prefix `Mst`).
- Pendaftaran Fluent API terintegrasi penuh melalui `ApplyConfigurationsFromAssembly`.
- Semua kode baru bebas dari kesalahan sintaks dan siap dipakai oleh Service API layer pada Task 2.
