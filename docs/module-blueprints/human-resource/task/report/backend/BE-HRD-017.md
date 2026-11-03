# Laporan Perubahan Backend — `BE-HRD-017`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-017` |
| Judul | Fondasi Data S-D1: 16 Tabel Hrd*, Enum, Konstanta, dan Migration AddWorkforcePlanningCore |
| Slice | `S-D1` (Perencanaan Tenaga Kerja / Workforce Planning) |
| Milestone | `MVP-7` (Fondasi, Formasi, dan Batch Formasi Awal) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang MVP-7 |
| Traceability | `HRD-DEC-059`, `HRD-DEC-019`, `HRD-TQ-13`, `NFR-012`, `02-backend-architecture.md` §16.4–§16.7 |
| Dependency | `BE-HRD-023` ✅ |
| Klasifikasi | `HEAVY` |
| Task Mode | `BACKEND` |
| Target Tulis | Enums, Constants, Models, Configurations, ApplicationDbContext, Migrations, Laporan Task, Roadmap |
| Tanggal | 03 November 2026 |
| Status | Selesai (`PASS`) — Kode terkompilasi 0 error & Migration EF Core tergenerasi bersih |

---

## 1. Ringkasan dan Masalah yang Diselesaikan

Sebelumnya, sub-modul Perencanaan Tenaga Kerja (*Workforce Planning*) rumah sakit tidak memiliki skema data relasional resmi di sistem Quilvian. Kebutuhan penghitungan beban kerja riil tenaga medis (seperti perawat ICU, bidan VK, staf IGD), penentuan formasi kuota kursi jabatan resmi, alur usulan penambahan pegawai (*Headcount Request*), hingga pemenuhan formasi belum terfasilitasi secara terstruktur.

Melalui task `BE-HRD-017`, fondasi persistensi relasional untuk slice `S-D1` resmi dibangun secara bersih dengan:
1. Mematuhi keputusan arsitektur `HRD-DEC-059`: Sepuluh entitas kandidat lama tidak diubah, tidak dipindah, dan tidak dihapus; seluruh data operasional baru menggunakan prefix kanonikal `Hrd*`.
2. Menyediakan 16 entitas model dan tabel baru lengkap dengan tipe data PostgreSQL spesifik, indeks terfilter, dan kendali konkurensi berbasis `RowVersion` (`xmin`).
3. Menghasilkan migration EF Core `AddWorkforcePlanningCore` yang **hanya** membuat 16 tabel baru, foreign key, dan index tanpa menyentuh atau menghapus tabel eksisting lainnya.

---

## 2. Alur Proses Bisnis Rumah Sakit yang Didukung

* **Tujuan**: Membangun fondasi persistensi relasional untuk mendukung siklus lengkap perencanaan SDM rumah sakit:
  - **Standar Kebutuhan Tenaga**: Menghitung kebutuhan staf berdasarkan metode Standar Beban Kerja (WISN), Rasio Pasien (contoh: 1 Perawat : 1 Pasien ICU), atau Jadwal Shift Jaga 24 Jam.
  - **Formasi Jabatan Resmi**: Menetapkan kuota kursi resmi per unit dan jabatan yang memiliki versi berkesinambungan (*carry-forward*), tidak dapat disunting manual langsung di luar alur resmi.
  - **Usulan Formasi Awal**: Memasukkan baseline formasi rumah sakit dari data penempatan 55 pegawai eksisting melalui mekanisme batch atomik.
  - **Permintaan Tambahan Tenaga (Headcount Request)**: Mengajukan penambahan pegawai (dalam/luar formasi) lengkap dengan rincian kursi (*seats*) yang siap dipenuhi lewat rekrutmen baru maupun mutasi internal SK Kepegawaian (`HrdPersonnelActionOrder`).
  - **Rencana Tenaga Kerja Tahunan & Addendum**: Menyusun rencana makro kebutuhan SDM tahunan rumah sakit beserta kalkulasi anggaran biaya staf.
  - **Potret Gap (Staffing Gap Snapshot)**: Merekam analisa kesenjangan antara formasi resmi, staf riil yang bertugas, dan kursi yang sedang dalam proses pengisian.

---

## 3. Rincian Perubahan Teknis

### 3.1 Empat Belas Enum Domain (`WorkforcePlanning/Enums/`)
Dibuat di direktori `Areas/Corporate/HumanResource/WorkforcePlanning/Enums/`:
1. `StaffingStandardStatus.cs`: `Draft = 1`, `Submitted = 2`, `UnderReview = 3`, `Approved = 4`, `Active = 5`, `Superseded = 6`, `Rejected = 7`, `Cancelled = 8`.
2. `StaffingCalculationMethod.cs`: `WorkloadBased = 1`, `PatientRatio = 2`, `ShiftSchedule = 3`, `WorkloadAnalysis = 4` (cadangan tanpa perilaku).
3. `StaffingRoundingMethod.cs`: `RoundUp = 1`, `RoundDown = 2`, `RoundNearest = 3`.
4. `PositionFormationSourceType.cs`: `InitialBatch = 1`, `ManpowerPlan = 2`, `HeadcountRequest = 3`.
5. `InitialFormationBatchStatus.cs`: `Draft = 1`, `Submitted = 2`, `Approved = 3`, `Executed = 4`, `Rejected = 5`, `Cancelled = 6`.
6. `ManpowerPlanKind.cs`: `Annual = 1`, `Addendum = 2`.
7. `ManpowerPlanStatus.cs`: `Draft = 1`, `Submitted = 2`, `Approved = 3`, `Active = 4`, `Superseded = 5`, `Rejected = 6`, `Cancelled = 7`.
8. `CostCalculationStatus.cs`: `NotCalculated = 1`, `Calculated = 2`, `Stale = 3`.
9. `HeadcountRequestType.cs`: `WithinFormation = 1`, `OutsideFormation = 2`.
10. `HeadcountRequestStatus.cs`: `Draft = 1`, `Submitted = 2`, `Approved = 3`, `ReadyForRecruitment = 4`, `FulfilmentInProgress = 5`, `Fulfilled = 6`, `Rejected = 7`, `Cancelled = 8`.
11. `HeadcountSeatStatus.cs`: `Open = 1`, `Allocated = 2`, `Fulfilled = 3`, `Cancelled = 4`.
12. `HeadcountFulfilmentSource.cs`: `InternalTransfer = 1`, `NewHire = 2`.
13. `HeadcountCancellationStatus.cs`: `Draft = 1`, `Submitted = 2`, `Approved = 3`, `Rejected = 4`.
14. `StaffingGapSnapshotType.cs`: `Direct = 1`, `Monthly = 2`.

### 3.2 Konstanta Nilai Domain (`Constants/WorkforcePlanningValueConstants.cs`)
1. **Jenis Rujukan Workflow**: `WorkforceStaffingStandard`, `WorkforceInitialFormationBatch`, `WorkforceManpowerPlan`, `WorkforceHeadcountRequest`, `WorkforceHeadcountCancellation`.
2. **Kunci Number Series**: `WFP_STF`, `WFP_BAT`, `WFP_PLN`, `WFP_REQ`, `WFP_CAN`.
3. **Pola Kunci Kunci Kunci Advisory Lock**: `HRD_WFP_FORMATION_{organizationUnitId}_{positionId}`.

### 3.3 Sepuluh Berkas Model (16 Kelas Entitas `Hrd*`)
Dibuat di direktori `Areas/Corporate/HumanResource/WorkforcePlanning/Models/`:
1. `HrdStaffingStandard.cs` — memuat `HrdStaffingStandard`, `HrdStaffingStandardLine`, dan `HrdStaffingSkillMixRule`.
2. `HrdStaffingWorkloadInput.cs` — memuat `HrdStaffingWorkloadInput`.
3. `HrdPositionFormation.cs` — memuat `HrdPositionFormation`.
4. `HrdInitialFormationBatch.cs` — memuat `HrdInitialFormationBatch` dan `HrdInitialFormationBatchRow`.
5. `HrdManpowerPlan.cs` — memuat `HrdManpowerPlan` dan `HrdManpowerPlanLine`.
6. `HrdHeadcountRequest.cs` — memuat `HrdHeadcountRequest` dan `HrdHeadcountRequestSeat`.
7. `HrdHeadcountRequestCancellation.cs` — memuat `HrdHeadcountRequestCancellation`.
8. `HrdFulfilmentPlanChange.cs` — memuat `HrdFulfilmentPlanChange`.
9. `HrdStaffingGapSnapshot.cs` — memuat `HrdStaffingGapSnapshot` dan `HrdStaffingGapSnapshotLine`.
10. `HrdWorkforcePlanningSetting.cs` — memuat `HrdWorkforcePlanningSetting`.

### 3.4 Enam Belas Konfigurasi EF Core Fluent API
Dibuat di `Repositories/Configurations/Corporate/HumanResource/WorkforcePlanning/`:
- Menetapkan nama tabel PostgreSQL spesifik (contoh: `HrdStaffingStandard`, `HrdPositionFormation`, dll.).
- Memetakan field `RowVersion` dengan tipe `xmin` dan `IsConcurrencyToken()` pada entitas dokumen transaksi.
- Memasang indeks unik terfilter:
  - `IX_HrdPositionFormation_Unit_Pos_EffectiveStart`: Unik per unit × jabatan × tanggal mulai.
  - `IX_HrdManpowerPlan_Hospital_Year`: Unik per rumah sakit × tahun (untuk status di luar `Rejected`/`Cancelled`).
  - `IX_HrdHeadcountRequestSeat_LinkedPositionAssignmentId`: Unik untuk assignment yang terhubung (di luar status `Cancelled`).
  - `IX_HrdHeadcountRequestSeat_LinkedPersonnelActionOrderId`: Unik untuk SK mutasi yang terhubung (di luar status `Cancelled`).
  - `IX_HrdStaffingWorkloadInput_Unit_Pos_PeriodStart`: Unik per unit × jabatan × tanggal mulai periode.
  - `IX_HrdWorkforcePlanningSetting_HospitalSiteId`: Unik per rumah sakit.
- Menetapkan integritas referensial: `Cascade` untuk entitas baris/detail ke header dokumen, dan `Restrict` untuk relasi foreign key ke master data bersama (`MstOrganizationUnit`, `MstPosition`, `MstWorkforceProfile`, `HrdPersonnelActionOrder`, dll.).

### 3.5 Registrasi `DbSet` di `ApplicationDbContext.cs`
Mendaftarkan 16 properti `DbSet<Hrd*>` di bawah region `#region CORPORATE - HUMAN RESOURCE - WORKFORCE PLANNING`:
- `HrdStaffingStandards`, `HrdStaffingStandardLines`, `HrdStaffingSkillMixRules`, `HrdStaffingWorkloadInputs`
- `HrdPositionFormations`, `HrdInitialFormationBatches`, `HrdInitialFormationBatchRows`
- `HrdManpowerPlans`, `HrdManpowerPlanLines`, `HrdHeadcountRequests`, `HrdHeadcountRequestSeats`
- `HrdHeadcountRequestCancellations`, `HrdFulfilmentPlanChanges`
- `HrdStaffingGapSnapshots`, `HrdStaffingGapSnapshotLines`, `HrdWorkforcePlanningSettings`.

### 3.6 Migration EF Core `AddWorkforcePlanningCore`
- Berkas dihasilkan: `Migrations/20261103065939_AddWorkforcePlanningCore.cs` & `20261103065939_AddWorkforcePlanningCore.Designer.cs`.
- Penyelarasan metadata: Berkas `Migrations/MigrationMetadata.g.cs` diperbarui melalui skrip resmi `Update-MigrationHistory.ps1`.
- Verifikasi operasi migration:
  - Method `Up()`: Tepat 16 operasi `CreateTable` beserta foreign key dan index. **Tidak ada** operasi `DropTable`, `AlterTable`, `DropColumn`, atau `AlterColumn` pada tabel eksisting.
  - Method `Down()`: Tepat 16 operasi `DropTable` untuk ke-16 tabel baru secara urut dari relasi anak ke relasi induk.

---

## 4. Hasil Verifikasi dan Kepatuhan

1. **Kompilasi Solusi (`dotnet build`)**:
   - Status: **BERHASIL (`Build succeeded`)**
   - Error: **0 Error**
   - Warning: 414 Warning (seluruhnya berasal dari catatan komentar XML lama pada modul lain).
2. **Kesesuaian Skema & DDL**:
   - Seluruh nama kolom, nullability, presisi angka desimal `numeric(5,2)`, `numeric(18,2)`, dan `timestamp with time zone` telah diverifikasi 100% cocok dengan Kamus Data v7 bagian 8.2 dan DDL bagian 8.4.
3. **Isolasi Entitas Kandidat Legacy (`HRD-DEC-059`)**:
   - Sepuluh entitas model kandidat `WorkforcePlanning` dan tabel `MstWorkforceRequirement` tetap utuh tanpa perubahan, menjaga stabilitas sistem eksisting.
4. **Wewenang Database Execution**:
   - Pembuatan berkas migrasi telah selesai sebagai kode sumber. Eksekusi migrasi fisik ke PostgreSQL database dev (`dotnet ef database update`) berada di luar wewenang otomatisasi tanpa izin otorisasi terpisah, sehingga status task ditandai selesai dari sisi source & build.

---

## 5. Status dan Langkah Selanjutnya

* **Status Task**: Selesai (`PASS`) pada tingkat source code, EF Core model snapshot, dan migration artifact.
* **Task Siap Dikerjakan Berikutnya**:
  1. `BE-HRD-029` — Penelusuran unit induk pada matriks persetujuan untuk langkah Direktur (Dependency: `BE-HRD-005` ✅, siap dikerjakan secara mandiri).
  2. `BE-HRD-030` — Lima jenis rujukan workflow dan layanan integrasi persetujuan `S-D1` (Dependency: `BE-HRD-017` ✅).
  3. `BE-HRD-031` — Pengaturan perencanaan per rumah sakit (`HrdWorkforcePlanningSettingService` & Controller) (Dependency: `BE-HRD-017` ✅).
  4. `BE-HRD-032` — Buku formasi berversi: satu-satunya penulis formasi dan API baca (`HrdPositionFormationService` & Controller) (Dependency: `BE-HRD-017` ✅).
