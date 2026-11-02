# Laporan Perubahan Backend — `BE-MCU-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-MCU-002` |
| Judul | Skema 21 tabel MCU |
| Slice | `MVP-0` — fondasi data `EPIC MCU-01` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian *`BE-MCU-002`* |
| Trace | `FR-MCU-001`; `MCU-DEC-003`, `007`, `012`, `015`, `020`, `025`, `031`..`042`; `MCU-STATE-v1` (nilai enum status) |
| Contract version | [`erd/data-dictionary.md`](../../../erd/data-dictionary.md) (`MCU-BP-001` r1, `approved` 2026-10-21) — kolom, index, FK, dan DDL |
| Dependency | `BE-MCU-000` ✅ (registry `Mcu` / `ACTIVE`) |
| Klasifikasi | `HEAVY` — 21 entity baru, 21 configuration, satu migration besar; tanpa endpoint, tanpa perubahan perilaku |
| Task mode | `BACKEND` |
| Target tulis | Backend: `Areas/HealthServices/MedicalCheckupManagement/{Models,Enums}/`, `Repositories/Configurations/HealthServices/MedicalCheckupManagement/`, `Repositories/ApplicationDbContext.cs`, `Migrations/` (berkas migration saja), laporan ini, tanda status roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `e86d98aa` (branch `yoga`) |
| Tanggal | 2026-10-21 |
| Status | Selesai — keempat acceptance criteria terbukti. Migration **belum** diterapkan ke basis data mana pun |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `MedicalCheckupManagement / Medical Check-Up`, prefix `Mcu`, registry `ACTIVE` (2026-10-21) |
| Keberlakuan | `NEW CODE` |
| QBE yang berlaku | `QBE-ENT-001` (21 entity mewarisi `IdentityModel`), `QBE-ENT-002`, `QBE-NAM-001`/`002`/`004` (prefix `Mcu`, tanpa `Trx*`), `QBE-CFG-001` (21 configuration), `QBE-MOD-001`/`002`/`003` (folder terdaftar), `QBE-CODE-004` (index unik `OrderNumber`, `ImportNumber`), `QBE-ENUM-001` (enum milik modul), `QBE-ENT-003` (tanpa kolom presentasi; tanpa `SortOrder`) |
| Checker QBE | `PASS` — 44 berkas MCU nol temuan. Kontrol negatif sementara `ZzzControlThing` di folder tak terdaftar terkena `QBE-MOD-002` dan `QBE-CFG-001`, lalu dihapus — membuktikan entity benar-benar dinilai |

---

## 1. Masalah yang diperbaiki

Modul MCU belum punya tempat menyimpan data apa pun. Tanpa tabel, tidak satu pun langkah perjalanan
peserta — paket, kuota harian, booking, check-in, komponen, hasil, resume, penyerahan — dapat dibangun.

**Contoh.** Admin MCU ingin menyusun *Paket Basic* Rp 350.000 berisi Darah Lengkap, Gula Darah Puasa,
Thorax, dan Pemeriksaan Fisik. Sebelum task ini, paket, komponennya, dan nilai pengembaliannya tidak
punya tabel. Sesudah task ini, strukturnya ada — layarnya dibangun pada task berikutnya (`BE-MCU-005`).

---

## 2. Proses bisnis

Task ini tidak mengubah perilaku apa pun; ia menyiapkan wadah data untuk seluruh alur
`FLOW-MCU-MVP-001`. Pemetaan tabel ke langkah bisnis:

| Langkah peserta | Tabel | Aturan yang dijaga basis data |
| --- | --- | --- |
| Admin menyusun paket dan harga | `McuPackage`, `McuPackageComponent`, `McuPackageComponentRequirement`, `McuPackageCompanyPrice` | Kode paket unik; satu prosedur dan satu jenis Pemeriksaan MCU hanya sekali per paket |
| Admin menyusun formulir pemeriksaan dan kategori kelayakan | `McuExaminationType`, `McuExaminationField`, `McuFitnessCategory` | Kode unik; kode isian dan nomor butir unik per jenis |
| Kepala unit membuka hari dan kuota | `McuDailySchedule`, `McuQuotaOverride` | Satu baris per tanggal; `Version` mencegah dua booking serentak mengambil kursi terakhir |
| Admisi membooking / check-in | `McuOrder`, `McuOrderStatusHistory`, `McuOrderDoctorAssignment`, `McuOrderRequirementCheck`, `McuBookingImport` | Nomor order dan nomor impor unik; satu kunjungan paling banyak satu order; satu dokter MCU berlaku per order |
| Pemeriksaan berjalan | `McuOrderComponent`, `McuExaminationResult`, `McuExaminationResultValue`, `McuExaminationResultValueRevision` | Satu prosedur/jenis per order selama komponen belum dibatalkan; satu hasil per komponen; satu nilai per isian |
| Resume dan penyerahan | `McuResume`, `McuReleaseConsent`, `McuResultDelivery` | Nomor versi unik per order; satu versi berlaku per order; satu persetujuan pelepasan berlaku per order |

**Jalur tidak normal yang ditangkap basis data**, bukan hanya service: dua petugas menyimpan paket
berkode sama → ditolak index unik; dua koreksi resume serentak sama-sama menjadi versi berlaku →
ditolak `IX_McuResume_Order_Current`; menghapus master yang masih dirujuk → ditolak FK `Restrict`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`erd/data-dictionary.md` (seluruhnya, termasuk DDL bagian 6), `02-backend-architecture.md` §4 dan §6,
model dan configuration modul Hemodialisa (preseden 22 tabel `Hmd*`), `RadReportConfiguration.cs`,
`LabResultCorrectionReasonConfiguration.cs` (pola `HasDefaultValue(true).ValueGeneratedNever()`),
`IdentityModel.cs`, `ApplicationDbContext.cs`, `QuilvianSystemBackend.csproj` (pengelolaan Designer
migration), registry, dan kontrak engineering.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/MedicalCheckupManagement/Enums/MedicalCheckupEnums.cs` (baru) | 15 enum (`McuOrderStatus` sembilan status, `McuOrderComponentStatus`, dst.), nilai mulai 1 |
| `Areas/HealthServices/MedicalCheckupManagement/Models/*.cs` (baru, 21 berkas) | 21 entity `Mcu*` ber-`[Table]`, mewarisi `IdentityModel`; kolom sensitif ditandai `SENSITIF` di komentar |
| `Repositories/Configurations/HealthServices/MedicalCheckupManagement/*.cs` (baru, 21 berkas) | Tabel, kunci, konversi enum `int`, presisi uang `18,2`, nilai bawaan, 19 index unik (berfilter sesuai DDL), FK `Restrict` |
| `Repositories/ApplicationDbContext.cs` | 21 `DbSet` jamak dalam region `MEDICAL CHECKUP MANAGEMENT` + satu `using` |
| `Migrations/20261021073245_AddMedicalCheckupManagement.cs` + `.Designer.cs` (baru) | 21 `CreateTable`, 81 `CreateIndex`; `Down()` 21 `DropTable` |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Hanya penambahan entity `Mcu*` (enam baris "terhapus" pada diff adalah penjajaran Git; jumlah kemunculan sama persis dengan HEAD) |
| `Tests/QuilvianSystemBackend.McuTests/` | Penyesuaian harness `BE-MCU-001` pada perubahan konstruktor `OutpatientQueueClassificationService` dan aturan nomor `RJ-PRI-DEC-004` dari merge terbaru (bukan bagian skema) |
| `docs/module-blueprints/mcu/task/report/backend/BE-MCU-001.md` | Koreksi contoh nomor antrean akibat `RJ-PRI-DEC-004` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — tidak ada endpoint |
| Database | 21 tabel baru di schema `public`; tidak ada tabel lain yang disentuh. **Migration belum diterapkan** ke basis data mana pun; penerapan butuh izin terpisah (gunakan skrip idempotent bila ada migration lain tertunda) |
| Keamanan/Auth | Kolom sensitif (`ParticipantName`, `ParticipantNik`, `ParticipantBirthDate`, `ParticipantPhone`, `Note`, `FailureSummary`, `ConclusionText`, `ValueText`, `PreviousValueText`, teks resume, `DestinationText`) ditandai di model; larangan log dan masking ditegakkan task service berikutnya |

### 3.4 Delta terhadap kamus data

| Butir | Kamus/DDL | Implementasi | Alasan |
| --- | --- | --- | --- |
| Tipe waktu | DDL menulis `timestamp` | `timestamp with time zone` | Konvensi Npgsql untuk `DateTime` UTC di seluruh repo; DDL kamus menyatakan diri dokumentasi bentuk, bukan skrip |
| Nama constraint FK | Sebagian disingkat, contoh `FK_McuPackageCompanyPrice_McuPackage` | Nama bawaan EF, contoh `FK_McuPackageCompanyPrice_McuPackage_PackageId` | Konvensi seluruh repo; DDL sendiri sudah memakai nama bawaan di beberapa FK |
| FK yang di DDL hanya disebut "pola yang sama" | Komentar DDL | Dipasang lengkap sesuai tabel kolom kamus (dokter, `ApplicationUser`, prosedur, modalitas) | Tabel kolom kamus yang lebih rinci dijadikan acuan |
| Index FK tambahan | Tidak tercantum | Index pendukung FK dari EF (contoh `IX_McuOrderComponent_PerformingDoctorId`) | Bawaan EF untuk setiap FK; tidak mengubah aturan |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task skema tanpa endpoint.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | Berhasil, 0 error (90 detik; ulang 59 detik sesudah migration) | `PASS` | Keluaran build |
| `dotnet ef migrations add AddMedicalCheckupManagement --no-build` | Tiga berkas migration; isi hanya 21 tabel MCU | `PASS` | §3.2 |
| `dotnet ef migrations script 20261020084310_AddLabTransitionHistoryActionOccurredAtIndex AddMedicalCheckupManagement --no-build` (offline) | 728 baris SQL; terlampir di §8 | `PASS` | Lampiran |
| Pencocokan SQL ↔ DDL kamus | 21 tabel; 19 index unik — nama, kolom, dan filter sama persis dengan DDL; 66 FK seluruhnya `ON DELETE RESTRICT`, 0 `CASCADE`; 4 kolom `numeric(18,2)`; 6 kolom tanggal `date`; kolom wajib/boleh kosong `McuOrder` sesuai | `PASS` | §8 |
| `SortOrder` pada SQL | 0 kemunculan | `PASS` | `grep -c SortOrder` |
| `Down()` | 21 `DropTable` tabel MCU, tidak ada operasi lain | `PASS` | Audit berkas migration |
| Checker QBE `-Path` (44 berkas MCU + `ApplicationDbContext.cs` + kontrol) | MCU 0 temuan; kontrol `Zzz*` terkena `QBE-MOD-002`/`QBE-CFG-001` | `PASS` | Keluaran checker |
| `dotnet test Tests/QuilvianSystemBackend.McuTests` | 17 lulus — model EF lengkap dengan 21 tabel MCU terbentuk di SQLite | `PASS` | `Passed! - Failed: 0, Passed: 17` |

Uji manual: `NOT APPLICABLE` — tidak ada perilaku pengguna.

**Tidak dijalankan:** penerapan migration ke devYoga atau basis data lain (di luar wewenang task).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Skrip SQL migration offline cocok dengan DDL kamus data — nama tabel, kolom wajib/boleh kosong, unique filtered index (`WHERE "IsDelete" = false`), FK `Restrict` | Terpenuhi | §5 baris pencocokan; delta yang disengaja di §3.4 |
| (2) Tidak ada kolom `SortOrder` (isian memakai `FieldNumber`) | Terpenuhi | 0 kemunculan; `FieldNumber` unik per jenis |
| (3) Checker QBE `PASS` untuk seluruh berkas model/configuration baru | Terpenuhi | 44 berkas, 0 temuan, kontrol negatif terbukti |
| (4) `Down()` menghapus 21 tabel dan tidak menyentuh tabel lain | Terpenuhi | Audit berkas migration |
| DoD: skrip SQL terlampir; migration belum diterapkan kecuali diizinkan | Terpenuhi | §8; tidak ada eksekusi basis data |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Merge `808a0451` (nomor antrean `RJ-PRI-DEC-004`, konstruktor klasifikasi antrean baru) membuat harness `BE-MCU-001` gagal kompilasi dan dua ekspektasi nomor salah. Sudah disesuaikan; perilaku service `BE-MCU-001` tidak berubah dan otomatis ikut aturan loket yang baru |
| Masalah yang diketahui | Tidak ada |
| Risiko tersisa | Migration ini memuat FK ke tabel milik modul lain (`LabOrder`, `RadOrder`, `BilInvoiceItem`, `BilRefundCase`, `RegPatientEncounter`, `MstPatient`, `AspNetUsers`); penerapan ke basis data menuntut seluruh migration modul itu sudah diterapkan lebih dulu. Kolom `CoveringMcuOrderId` pada Lab/Rad **belum** ada — itu `BE-MCU-003` |
| Perubahan sampingan | Kontrol negatif QBE `Areas/HealthServices/ZzzControlManagement/` dibuat dan dihapus pada langkah yang sama; `git status` memastikan tidak tersisa |
| Interupsi | `NONE` (satu penulisan berkas lewat heredoc gagal parse sebelum ada berkas tertulis; ditulis ulang per berkas) |
| Status Git | ` M Migrations/ApplicationDbContextModelSnapshot.cs`, ` M Repositories/ApplicationDbContext.cs`, `?? Areas/HealthServices/MedicalCheckupManagement/`, `?? Migrations/20261021073245_AddMedicalCheckupManagement.Designer.cs`, `?? Migrations/20261021073245_AddMedicalCheckupManagement.cs`, `?? Repositories/Configurations/HealthServices/MedicalCheckupManagement/`, perubahan docs MCU; `Tests/QuilvianSystemBackend.McuTests/` diabaikan `.gitignore` (perlu `git add -f`) |
| Langkah berikutnya | `BE-MCU-003` (kolom `CoveringMcuOrderId` Lab/Rad + migration 2), `BE-MCU-004`, `BE-MCU-005`, `BE-MCU-010` kini terbuka. Penerapan migration ke devYoga menunggu izin pemilik |

---

## 8. Lampiran — skrip SQL migration offline

Dihasilkan `dotnet ef migrations script 20261020084310_AddLabTransitionHistoryActionOccurredAtIndex AddMedicalCheckupManagement --no-build`.
**Bukan untuk dijalankan langsung**: penerapan memakai mekanisme migration proyek atas izin pemilik.

<details>
<summary>Tampilkan skrip (728 baris)</summary>

```sql
START TRANSACTION;
CREATE TABLE public."McuDailySchedule" (
    "Id" uuid NOT NULL,
    "ScheduleDate" date NOT NULL,
    "IsOpen" boolean NOT NULL DEFAULT TRUE,
    "Quota" integer NOT NULL,
    "UsedQuota" integer NOT NULL DEFAULT 0,
    "ApprovedOverrideQuota" integer NOT NULL DEFAULT 0,
    "DutyMcuDoctorId" uuid,
    "Version" integer NOT NULL DEFAULT 0,
    "CreateDateTime" timestamp with time zone NOT NULL,
    "CreateBy" uuid NOT NULL,
    "UpdateDateTime" timestamp with time zone,
    "UpdateBy" uuid NOT NULL,
    "DeleteDateTime" timestamp with time zone,
    "DeleteBy" uuid NOT NULL,
    "CancelDateTime" timestamp with time zone,
    "CancelBy" uuid NOT NULL,
    "IsCancel" boolean NOT NULL,
    "IsDelete" boolean NOT NULL,
    CONSTRAINT "PK_McuDailySchedule" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_McuDailySchedule_MstDoctor_DutyMcuDoctorId" FOREIGN KEY ("DutyMcuDoctorId") REFERENCES public."MstDoctor" ("Id") ON DELETE RESTRICT
);

CREATE TABLE public."McuExaminationType" (
    "Id" uuid NOT NULL,
    "ExaminationTypeCode" character varying(30) NOT NULL,
    "ExaminationTypeName" character varying(150) NOT NULL,
    "Description" character varying(500),
    "IsActive" boolean NOT NULL DEFAULT TRUE,
    "CreateDateTime" timestamp with time zone NOT NULL,
    "CreateBy" uuid NOT NULL,
    "UpdateDateTime" timestamp with time zone,
    "UpdateBy" uuid NOT NULL,
    "DeleteDateTime" timestamp with time zone,
    "DeleteBy" uuid NOT NULL,
    "CancelDateTime" timestamp with time zone,
    "CancelBy" uuid NOT NULL,
    "IsCancel" boolean NOT NULL,
    "IsDelete" boolean NOT NULL,
    CONSTRAINT "PK_McuExaminationType" PRIMARY KEY ("Id")
);

CREATE TABLE public."McuFitnessCategory" (
    "Id" uuid NOT NULL,
    "FitnessCategoryCode" character varying(30) NOT NULL,
    "FitnessCategoryName" character varying(100) NOT NULL,
    "Description" character varying(500),
    "IsActive" boolean NOT NULL DEFAULT TRUE,
    "CreateDateTime" timestamp with time zone NOT NULL,
    "CreateBy" uuid NOT NULL,
    "UpdateDateTime" timestamp with time zone,
    "UpdateBy" uuid NOT NULL,
    "DeleteDateTime" timestamp with time zone,
    "DeleteBy" uuid NOT NULL,
    "CancelDateTime" timestamp with time zone,
    "CancelBy" uuid NOT NULL,
    "IsCancel" boolean NOT NULL,
    "IsDelete" boolean NOT NULL,
    CONSTRAINT "PK_McuFitnessCategory" PRIMARY KEY ("Id")
);

CREATE TABLE public."McuPackage" (
    "Id" uuid NOT NULL,
    "PackageCode" character varying(30) NOT NULL,
    "PackageName" character varying(150) NOT NULL,
    "Description" character varying(1000),
    "GeneralTariffId" uuid NOT NULL,
    "IsActive" boolean NOT NULL DEFAULT TRUE,
    "CreateDateTime" timestamp with time zone NOT NULL,
    "CreateBy" uuid NOT NULL,
    "UpdateDateTime" timestamp with time zone,
    "UpdateBy" uuid NOT NULL,
    "DeleteDateTime" timestamp with time zone,
    "DeleteBy" uuid NOT NULL,
    "CancelDateTime" timestamp with time zone,
    "CancelBy" uuid NOT NULL,
    "IsCancel" boolean NOT NULL,
    "IsDelete" boolean NOT NULL,
    CONSTRAINT "PK_McuPackage" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_McuPackage_MstTariff_GeneralTariffId" FOREIGN KEY ("GeneralTariffId") REFERENCES public."MstTariff" ("Id") ON DELETE RESTRICT
);

CREATE TABLE public."McuQuotaOverride" (
    "Id" uuid NOT NULL,
    "DailyScheduleId" uuid NOT NULL,
    "AdditionalQuota" integer NOT NULL,
    "Reason" character varying(500) NOT NULL,
    "ApprovedByUserId" uuid NOT NULL,
    "ApprovedAt" timestamp with time zone NOT NULL,
    "CreateDateTime" timestamp with time zone NOT NULL,
    "CreateBy" uuid NOT NULL,
    "UpdateDateTime" timestamp with time zone,
    "UpdateBy" uuid NOT NULL,
    "DeleteDateTime" timestamp with time zone,
    "DeleteBy" uuid NOT NULL,
    "CancelDateTime" timestamp with time zone,
    "CancelBy" uuid NOT NULL,
    "IsCancel" boolean NOT NULL,
    "IsDelete" boolean NOT NULL,
    CONSTRAINT "PK_McuQuotaOverride" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_McuQuotaOverride_AspNetUsers_ApprovedByUserId" FOREIGN KEY ("ApprovedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuQuotaOverride_McuDailySchedule_DailyScheduleId" FOREIGN KEY ("DailyScheduleId") REFERENCES public."McuDailySchedule" ("Id") ON DELETE RESTRICT
);

CREATE TABLE public."McuExaminationField" (
    "Id" uuid NOT NULL,
    "ExaminationTypeId" uuid NOT NULL,
    "FieldCode" character varying(50) NOT NULL,
    "FieldLabel" character varying(150) NOT NULL,
    "FieldType" integer NOT NULL,
    "UnitName" character varying(30),
    "OptionValues" character varying(1000),
    "IsRequired" boolean NOT NULL DEFAULT FALSE,
    "FieldNumber" integer NOT NULL,
    "CreateDateTime" timestamp with time zone NOT NULL,
    "CreateBy" uuid NOT NULL,
    "UpdateDateTime" timestamp with time zone,
    "UpdateBy" uuid NOT NULL,
    "DeleteDateTime" timestamp with time zone,
    "DeleteBy" uuid NOT NULL,
    "CancelDateTime" timestamp with time zone,
    "CancelBy" uuid NOT NULL,
    "IsCancel" boolean NOT NULL,
    "IsDelete" boolean NOT NULL,
    CONSTRAINT "PK_McuExaminationField" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_McuExaminationField_McuExaminationType_ExaminationTypeId" FOREIGN KEY ("ExaminationTypeId") REFERENCES public."McuExaminationType" ("Id") ON DELETE RESTRICT
);

CREATE TABLE public."McuBookingImport" (
    "Id" uuid NOT NULL,
    "ImportNumber" character varying(30) NOT NULL,
    "CompanyGuarantorId" uuid NOT NULL,
    "PackageId" uuid NOT NULL,
    "ExaminationDate" date NOT NULL,
    "FileName" character varying(255) NOT NULL,
    "TotalRows" integer NOT NULL,
    "CreatedRows" integer NOT NULL,
    "FailedRows" integer NOT NULL,
    "FailureSummary" character varying(4000),
    "CreateDateTime" timestamp with time zone NOT NULL,
    "CreateBy" uuid NOT NULL,
    "UpdateDateTime" timestamp with time zone,
    "UpdateBy" uuid NOT NULL,
    "DeleteDateTime" timestamp with time zone,
    "DeleteBy" uuid NOT NULL,
    "CancelDateTime" timestamp with time zone,
    "CancelBy" uuid NOT NULL,
    "IsCancel" boolean NOT NULL,
    "IsDelete" boolean NOT NULL,
    CONSTRAINT "PK_McuBookingImport" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_McuBookingImport_McuPackage_PackageId" FOREIGN KEY ("PackageId") REFERENCES public."McuPackage" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuBookingImport_MstCompanyGuarantor_CompanyGuarantorId" FOREIGN KEY ("CompanyGuarantorId") REFERENCES public."MstCompanyGuarantor" ("Id") ON DELETE RESTRICT
);

CREATE TABLE public."McuPackageCompanyPrice" (
    "Id" uuid NOT NULL,
    "PackageId" uuid NOT NULL,
    "CompanyGuarantorId" uuid NOT NULL,
    "TariffId" uuid NOT NULL,
    "EffectiveStartDate" date NOT NULL,
    "EffectiveEndDate" date,
    "CreateDateTime" timestamp with time zone NOT NULL,
    "CreateBy" uuid NOT NULL,
    "UpdateDateTime" timestamp with time zone,
    "UpdateBy" uuid NOT NULL,
    "DeleteDateTime" timestamp with time zone,
    "DeleteBy" uuid NOT NULL,
    "CancelDateTime" timestamp with time zone,
    "CancelBy" uuid NOT NULL,
    "IsCancel" boolean NOT NULL,
    "IsDelete" boolean NOT NULL,
    CONSTRAINT "PK_McuPackageCompanyPrice" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_McuPackageCompanyPrice_McuPackage_PackageId" FOREIGN KEY ("PackageId") REFERENCES public."McuPackage" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuPackageCompanyPrice_MstCompanyGuarantor_CompanyGuarantor~" FOREIGN KEY ("CompanyGuarantorId") REFERENCES public."MstCompanyGuarantor" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuPackageCompanyPrice_MstTariff_TariffId" FOREIGN KEY ("TariffId") REFERENCES public."MstTariff" ("Id") ON DELETE RESTRICT
);

CREATE TABLE public."McuPackageComponent" (
    "Id" uuid NOT NULL,
    "PackageId" uuid NOT NULL,
    "ComponentType" integer NOT NULL,
    "ProcedureId" uuid,
    "RadModalityId" uuid,
    "ExaminationTypeId" uuid,
    "RefundAmount" numeric(18,2) NOT NULL DEFAULT 0.0,
    "MedicalFeeAmount" numeric(18,2) NOT NULL DEFAULT 0.0,
    "CreateDateTime" timestamp with time zone NOT NULL,
    "CreateBy" uuid NOT NULL,
    "UpdateDateTime" timestamp with time zone,
    "UpdateBy" uuid NOT NULL,
    "DeleteDateTime" timestamp with time zone,
    "DeleteBy" uuid NOT NULL,
    "CancelDateTime" timestamp with time zone,
    "CancelBy" uuid NOT NULL,
    "IsCancel" boolean NOT NULL,
    "IsDelete" boolean NOT NULL,
    CONSTRAINT "PK_McuPackageComponent" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_McuPackageComponent_McuExaminationType_ExaminationTypeId" FOREIGN KEY ("ExaminationTypeId") REFERENCES public."McuExaminationType" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuPackageComponent_McuPackage_PackageId" FOREIGN KEY ("PackageId") REFERENCES public."McuPackage" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuPackageComponent_MstProcedure_ProcedureId" FOREIGN KEY ("ProcedureId") REFERENCES public."MstProcedure" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuPackageComponent_MstRadModality_RadModalityId" FOREIGN KEY ("RadModalityId") REFERENCES public."MstRadModality" ("Id") ON DELETE RESTRICT
);

CREATE TABLE public."McuOrder" (
    "Id" uuid NOT NULL,
    "OrderNumber" character varying(30) NOT NULL,
    "PackageId" uuid NOT NULL,
    "PackageCodeSnapshot" character varying(30) NOT NULL,
    "PackageNameSnapshot" character varying(150) NOT NULL,
    "PatientId" uuid,
    "ParticipantName" character varying(150) NOT NULL,
    "ParticipantNik" character varying(16),
    "ParticipantBirthDate" date,
    "ParticipantPhone" character varying(20),
    "PatientMatchMethod" integer,
    "PatientMatchConfirmedByUserId" uuid,
    "Source" integer NOT NULL,
    "SourceEncounterId" uuid,
    "RecommendingDoctorId" uuid,
    "CompanyGuarantorId" uuid,
    "BookingImportId" uuid,
    "ExaminationDate" date NOT NULL,
    "DailyScheduleId" uuid NOT NULL,
    "OrderStatus" integer NOT NULL,
    "ArrivalConfirmedAt" timestamp with time zone,
    "EncounterId" uuid,
    "CheckInChannel" integer,
    "CheckedInAt" timestamp with time zone,
    "CheckedInByUserId" uuid,
    "McuDoctorId" uuid,
    "AppliedTariffId" uuid,
    "PackageInvoiceItemId" uuid,
    "ClearanceBasis" integer,
    "ExaminationStartedAt" timestamp with time zone,
    "NoShowAt" timestamp with time zone,
    "CancelReason" character varying(500),
    "Version" integer NOT NULL DEFAULT 0,
    "CreateDateTime" timestamp with time zone NOT NULL,
    "CreateBy" uuid NOT NULL,
    "UpdateDateTime" timestamp with time zone,
    "UpdateBy" uuid NOT NULL,
    "DeleteDateTime" timestamp with time zone,
    "DeleteBy" uuid NOT NULL,
    "CancelDateTime" timestamp with time zone,
    "CancelBy" uuid NOT NULL,
    "IsCancel" boolean NOT NULL,
    "IsDelete" boolean NOT NULL,
    CONSTRAINT "PK_McuOrder" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_McuOrder_AspNetUsers_CheckedInByUserId" FOREIGN KEY ("CheckedInByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuOrder_AspNetUsers_PatientMatchConfirmedByUserId" FOREIGN KEY ("PatientMatchConfirmedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuOrder_BilInvoiceItem_PackageInvoiceItemId" FOREIGN KEY ("PackageInvoiceItemId") REFERENCES public."BilInvoiceItem" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuOrder_McuBookingImport_BookingImportId" FOREIGN KEY ("BookingImportId") REFERENCES public."McuBookingImport" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuOrder_McuDailySchedule_DailyScheduleId" FOREIGN KEY ("DailyScheduleId") REFERENCES public."McuDailySchedule" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuOrder_McuPackage_PackageId" FOREIGN KEY ("PackageId") REFERENCES public."McuPackage" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuOrder_MstCompanyGuarantor_CompanyGuarantorId" FOREIGN KEY ("CompanyGuarantorId") REFERENCES public."MstCompanyGuarantor" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuOrder_MstDoctor_McuDoctorId" FOREIGN KEY ("McuDoctorId") REFERENCES public."MstDoctor" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuOrder_MstDoctor_RecommendingDoctorId" FOREIGN KEY ("RecommendingDoctorId") REFERENCES public."MstDoctor" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuOrder_MstPatient_PatientId" FOREIGN KEY ("PatientId") REFERENCES public."MstPatient" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuOrder_MstTariff_AppliedTariffId" FOREIGN KEY ("AppliedTariffId") REFERENCES public."MstTariff" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuOrder_RegPatientEncounter_EncounterId" FOREIGN KEY ("EncounterId") REFERENCES public."RegPatientEncounter" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuOrder_RegPatientEncounter_SourceEncounterId" FOREIGN KEY ("SourceEncounterId") REFERENCES public."RegPatientEncounter" ("Id") ON DELETE RESTRICT
);

CREATE TABLE public."McuPackageComponentRequirement" (
    "Id" uuid NOT NULL,
    "PackageComponentId" uuid NOT NULL,
    "RequirementText" character varying(300) NOT NULL,
    "IsConfirmationRequired" boolean NOT NULL DEFAULT TRUE,
    "CreateDateTime" timestamp with time zone NOT NULL,
    "CreateBy" uuid NOT NULL,
    "UpdateDateTime" timestamp with time zone,
    "UpdateBy" uuid NOT NULL,
    "DeleteDateTime" timestamp with time zone,
    "DeleteBy" uuid NOT NULL,
    "CancelDateTime" timestamp with time zone,
    "CancelBy" uuid NOT NULL,
    "IsCancel" boolean NOT NULL,
    "IsDelete" boolean NOT NULL,
    CONSTRAINT "PK_McuPackageComponentRequirement" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_McuPackageComponentRequirement_McuPackageComponent_PackageC~" FOREIGN KEY ("PackageComponentId") REFERENCES public."McuPackageComponent" ("Id") ON DELETE RESTRICT
);

CREATE TABLE public."McuOrderComponent" (
    "Id" uuid NOT NULL,
    "McuOrderId" uuid NOT NULL,
    "PackageComponentId" uuid,
    "ComponentType" integer NOT NULL,
    "ProcedureId" uuid,
    "RadModalityId" uuid,
    "ExaminationTypeId" uuid,
    "ComponentNameSnapshot" character varying(200) NOT NULL,
    "IsPackageCovered" boolean NOT NULL DEFAULT TRUE,
    "RefundAmountSnapshot" numeric(18,2) NOT NULL DEFAULT 0.0,
    "MedicalFeeAmountSnapshot" numeric(18,2) NOT NULL DEFAULT 0.0,
    "ComponentStatus" integer NOT NULL,
    "ScheduledDate" date NOT NULL,
    "LabOrderId" uuid,
    "RadOrderId" uuid,
    "PerformingDoctorId" uuid,
    "CompletedAt" timestamp with time zone,
    "CancelReason" character varying(500),
    "CancelledByDoctorId" uuid,
    "RefundCaseId" uuid,
    "MedicalFeeReleasedAt" timestamp with time zone,
    "AddedByDoctorId" uuid,
    "CreateDateTime" timestamp with time zone NOT NULL,
    "CreateBy" uuid NOT NULL,
    "UpdateDateTime" timestamp with time zone,
    "UpdateBy" uuid NOT NULL,
    "DeleteDateTime" timestamp with time zone,
    "DeleteBy" uuid NOT NULL,
    "CancelDateTime" timestamp with time zone,
    "CancelBy" uuid NOT NULL,
    "IsCancel" boolean NOT NULL,
    "IsDelete" boolean NOT NULL,
    CONSTRAINT "PK_McuOrderComponent" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_McuOrderComponent_BilRefundCase_RefundCaseId" FOREIGN KEY ("RefundCaseId") REFERENCES public."BilRefundCase" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuOrderComponent_LabOrder_LabOrderId" FOREIGN KEY ("LabOrderId") REFERENCES public."LabOrder" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuOrderComponent_McuExaminationType_ExaminationTypeId" FOREIGN KEY ("ExaminationTypeId") REFERENCES public."McuExaminationType" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuOrderComponent_McuOrder_McuOrderId" FOREIGN KEY ("McuOrderId") REFERENCES public."McuOrder" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuOrderComponent_McuPackageComponent_PackageComponentId" FOREIGN KEY ("PackageComponentId") REFERENCES public."McuPackageComponent" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuOrderComponent_MstDoctor_AddedByDoctorId" FOREIGN KEY ("AddedByDoctorId") REFERENCES public."MstDoctor" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuOrderComponent_MstDoctor_CancelledByDoctorId" FOREIGN KEY ("CancelledByDoctorId") REFERENCES public."MstDoctor" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuOrderComponent_MstDoctor_PerformingDoctorId" FOREIGN KEY ("PerformingDoctorId") REFERENCES public."MstDoctor" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuOrderComponent_MstProcedure_ProcedureId" FOREIGN KEY ("ProcedureId") REFERENCES public."MstProcedure" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuOrderComponent_MstRadModality_RadModalityId" FOREIGN KEY ("RadModalityId") REFERENCES public."MstRadModality" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuOrderComponent_RadOrder_RadOrderId" FOREIGN KEY ("RadOrderId") REFERENCES public."RadOrder" ("Id") ON DELETE RESTRICT
);

CREATE TABLE public."McuOrderDoctorAssignment" (
    "Id" uuid NOT NULL,
    "McuOrderId" uuid NOT NULL,
    "DoctorId" uuid NOT NULL,
    "AssignedAt" timestamp with time zone NOT NULL,
    "AssignedByUserId" uuid NOT NULL,
    "Reason" character varying(500),
    "EndedAt" timestamp with time zone,
    "CreateDateTime" timestamp with time zone NOT NULL,
    "CreateBy" uuid NOT NULL,
    "UpdateDateTime" timestamp with time zone,
    "UpdateBy" uuid NOT NULL,
    "DeleteDateTime" timestamp with time zone,
    "DeleteBy" uuid NOT NULL,
    "CancelDateTime" timestamp with time zone,
    "CancelBy" uuid NOT NULL,
    "IsCancel" boolean NOT NULL,
    "IsDelete" boolean NOT NULL,
    CONSTRAINT "PK_McuOrderDoctorAssignment" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_McuOrderDoctorAssignment_AspNetUsers_AssignedByUserId" FOREIGN KEY ("AssignedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuOrderDoctorAssignment_McuOrder_McuOrderId" FOREIGN KEY ("McuOrderId") REFERENCES public."McuOrder" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuOrderDoctorAssignment_MstDoctor_DoctorId" FOREIGN KEY ("DoctorId") REFERENCES public."MstDoctor" ("Id") ON DELETE RESTRICT
);

CREATE TABLE public."McuOrderStatusHistory" (
    "Id" uuid NOT NULL,
    "McuOrderId" uuid NOT NULL,
    "FromStatus" integer,
    "ToStatus" integer NOT NULL,
    "ChangedAt" timestamp with time zone NOT NULL,
    "ChangedByUserId" uuid,
    "Reason" character varying(500),
    "CreateDateTime" timestamp with time zone NOT NULL,
    "CreateBy" uuid NOT NULL,
    "UpdateDateTime" timestamp with time zone,
    "UpdateBy" uuid NOT NULL,
    "DeleteDateTime" timestamp with time zone,
    "DeleteBy" uuid NOT NULL,
    "CancelDateTime" timestamp with time zone,
    "CancelBy" uuid NOT NULL,
    "IsCancel" boolean NOT NULL,
    "IsDelete" boolean NOT NULL,
    CONSTRAINT "PK_McuOrderStatusHistory" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_McuOrderStatusHistory_AspNetUsers_ChangedByUserId" FOREIGN KEY ("ChangedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuOrderStatusHistory_McuOrder_McuOrderId" FOREIGN KEY ("McuOrderId") REFERENCES public."McuOrder" ("Id") ON DELETE RESTRICT
);

CREATE TABLE public."McuReleaseConsent" (
    "Id" uuid NOT NULL,
    "McuOrderId" uuid NOT NULL,
    "CompanyGuarantorId" uuid NOT NULL,
    "ConsentGivenAt" timestamp with time zone NOT NULL,
    "RecordedByUserId" uuid NOT NULL,
    "EvidenceType" integer NOT NULL,
    "EvidenceReference" character varying(200),
    "RevokedAt" timestamp with time zone,
    "RevokedByUserId" uuid,
    "RevokeReason" character varying(500),
    "CreateDateTime" timestamp with time zone NOT NULL,
    "CreateBy" uuid NOT NULL,
    "UpdateDateTime" timestamp with time zone,
    "UpdateBy" uuid NOT NULL,
    "DeleteDateTime" timestamp with time zone,
    "DeleteBy" uuid NOT NULL,
    "CancelDateTime" timestamp with time zone,
    "CancelBy" uuid NOT NULL,
    "IsCancel" boolean NOT NULL,
    "IsDelete" boolean NOT NULL,
    CONSTRAINT "PK_McuReleaseConsent" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_McuReleaseConsent_AspNetUsers_RecordedByUserId" FOREIGN KEY ("RecordedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuReleaseConsent_AspNetUsers_RevokedByUserId" FOREIGN KEY ("RevokedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuReleaseConsent_McuOrder_McuOrderId" FOREIGN KEY ("McuOrderId") REFERENCES public."McuOrder" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuReleaseConsent_MstCompanyGuarantor_CompanyGuarantorId" FOREIGN KEY ("CompanyGuarantorId") REFERENCES public."MstCompanyGuarantor" ("Id") ON DELETE RESTRICT
);

CREATE TABLE public."McuResume" (
    "Id" uuid NOT NULL,
    "McuOrderId" uuid NOT NULL,
    "VersionNumber" integer NOT NULL,
    "ResumeStatus" integer NOT NULL,
    "IsCurrent" boolean NOT NULL DEFAULT TRUE,
    "GeneralConditionText" character varying(4000),
    "ImportantFindingsText" character varying(4000),
    "RecommendationText" character varying(4000),
    "FitnessCategoryId" uuid,
    "WorkRecommendationText" character varying(1000),
    "AuthorDoctorId" uuid,
    "ApprovedByDoctorId" uuid,
    "ApprovedAt" timestamp with time zone,
    "ReturnReason" character varying(500),
    "VerifiedByUserId" uuid,
    "VerifiedAt" timestamp with time zone,
    "ReopenReason" character varying(500),
    "ReopenedByDoctorId" uuid,
    "ReopenedAt" timestamp with time zone,
    "CreateDateTime" timestamp with time zone NOT NULL,
    "CreateBy" uuid NOT NULL,
    "UpdateDateTime" timestamp with time zone,
    "UpdateBy" uuid NOT NULL,
    "DeleteDateTime" timestamp with time zone,
    "DeleteBy" uuid NOT NULL,
    "CancelDateTime" timestamp with time zone,
    "CancelBy" uuid NOT NULL,
    "IsCancel" boolean NOT NULL,
    "IsDelete" boolean NOT NULL,
    CONSTRAINT "PK_McuResume" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_McuResume_AspNetUsers_VerifiedByUserId" FOREIGN KEY ("VerifiedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuResume_McuFitnessCategory_FitnessCategoryId" FOREIGN KEY ("FitnessCategoryId") REFERENCES public."McuFitnessCategory" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuResume_McuOrder_McuOrderId" FOREIGN KEY ("McuOrderId") REFERENCES public."McuOrder" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuResume_MstDoctor_ApprovedByDoctorId" FOREIGN KEY ("ApprovedByDoctorId") REFERENCES public."MstDoctor" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuResume_MstDoctor_AuthorDoctorId" FOREIGN KEY ("AuthorDoctorId") REFERENCES public."MstDoctor" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuResume_MstDoctor_ReopenedByDoctorId" FOREIGN KEY ("ReopenedByDoctorId") REFERENCES public."MstDoctor" ("Id") ON DELETE RESTRICT
);

CREATE TABLE public."McuExaminationResult" (
    "Id" uuid NOT NULL,
    "McuOrderComponentId" uuid NOT NULL,
    "ConclusionText" character varying(2000),
    "FilledByUserId" uuid NOT NULL,
    "FilledAt" timestamp with time zone NOT NULL,
    "CreateDateTime" timestamp with time zone NOT NULL,
    "CreateBy" uuid NOT NULL,
    "UpdateDateTime" timestamp with time zone,
    "UpdateBy" uuid NOT NULL,
    "DeleteDateTime" timestamp with time zone,
    "DeleteBy" uuid NOT NULL,
    "CancelDateTime" timestamp with time zone,
    "CancelBy" uuid NOT NULL,
    "IsCancel" boolean NOT NULL,
    "IsDelete" boolean NOT NULL,
    CONSTRAINT "PK_McuExaminationResult" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_McuExaminationResult_AspNetUsers_FilledByUserId" FOREIGN KEY ("FilledByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuExaminationResult_McuOrderComponent_McuOrderComponentId" FOREIGN KEY ("McuOrderComponentId") REFERENCES public."McuOrderComponent" ("Id") ON DELETE RESTRICT
);

CREATE TABLE public."McuOrderRequirementCheck" (
    "Id" uuid NOT NULL,
    "McuOrderId" uuid NOT NULL,
    "McuOrderComponentId" uuid NOT NULL,
    "RequirementTextSnapshot" character varying(300) NOT NULL,
    "Outcome" integer NOT NULL,
    "Note" character varying(500),
    "CheckedByUserId" uuid NOT NULL,
    "CheckedAt" timestamp with time zone NOT NULL,
    "CreateDateTime" timestamp with time zone NOT NULL,
    "CreateBy" uuid NOT NULL,
    "UpdateDateTime" timestamp with time zone,
    "UpdateBy" uuid NOT NULL,
    "DeleteDateTime" timestamp with time zone,
    "DeleteBy" uuid NOT NULL,
    "CancelDateTime" timestamp with time zone,
    "CancelBy" uuid NOT NULL,
    "IsCancel" boolean NOT NULL,
    "IsDelete" boolean NOT NULL,
    CONSTRAINT "PK_McuOrderRequirementCheck" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_McuOrderRequirementCheck_AspNetUsers_CheckedByUserId" FOREIGN KEY ("CheckedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuOrderRequirementCheck_McuOrderComponent_McuOrderComponen~" FOREIGN KEY ("McuOrderComponentId") REFERENCES public."McuOrderComponent" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuOrderRequirementCheck_McuOrder_McuOrderId" FOREIGN KEY ("McuOrderId") REFERENCES public."McuOrder" ("Id") ON DELETE RESTRICT
);

CREATE TABLE public."McuResultDelivery" (
    "Id" uuid NOT NULL,
    "McuOrderId" uuid NOT NULL,
    "McuResumeId" uuid NOT NULL,
    "RecipientType" integer NOT NULL,
    "ContentType" integer NOT NULL,
    "Channel" integer NOT NULL,
    "DestinationText" character varying(200),
    "DeliveredAt" timestamp with time zone NOT NULL,
    "DeliveredByUserId" uuid NOT NULL,
    "Note" character varying(500),
    "RequiresCorrectionNotice" boolean NOT NULL DEFAULT FALSE,
    "CorrectionNoticeRecordedAt" timestamp with time zone,
    "CreateDateTime" timestamp with time zone NOT NULL,
    "CreateBy" uuid NOT NULL,
    "UpdateDateTime" timestamp with time zone,
    "UpdateBy" uuid NOT NULL,
    "DeleteDateTime" timestamp with time zone,
    "DeleteBy" uuid NOT NULL,
    "CancelDateTime" timestamp with time zone,
    "CancelBy" uuid NOT NULL,
    "IsCancel" boolean NOT NULL,
    "IsDelete" boolean NOT NULL,
    CONSTRAINT "PK_McuResultDelivery" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_McuResultDelivery_AspNetUsers_DeliveredByUserId" FOREIGN KEY ("DeliveredByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuResultDelivery_McuOrder_McuOrderId" FOREIGN KEY ("McuOrderId") REFERENCES public."McuOrder" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuResultDelivery_McuResume_McuResumeId" FOREIGN KEY ("McuResumeId") REFERENCES public."McuResume" ("Id") ON DELETE RESTRICT
);

CREATE TABLE public."McuExaminationResultValue" (
    "Id" uuid NOT NULL,
    "ExaminationResultId" uuid NOT NULL,
    "ExaminationFieldId" uuid NOT NULL,
    "ValueText" character varying(500),
    "CreateDateTime" timestamp with time zone NOT NULL,
    "CreateBy" uuid NOT NULL,
    "UpdateDateTime" timestamp with time zone,
    "UpdateBy" uuid NOT NULL,
    "DeleteDateTime" timestamp with time zone,
    "DeleteBy" uuid NOT NULL,
    "CancelDateTime" timestamp with time zone,
    "CancelBy" uuid NOT NULL,
    "IsCancel" boolean NOT NULL,
    "IsDelete" boolean NOT NULL,
    CONSTRAINT "PK_McuExaminationResultValue" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_McuExaminationResultValue_McuExaminationField_ExaminationFi~" FOREIGN KEY ("ExaminationFieldId") REFERENCES public."McuExaminationField" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuExaminationResultValue_McuExaminationResult_ExaminationR~" FOREIGN KEY ("ExaminationResultId") REFERENCES public."McuExaminationResult" ("Id") ON DELETE RESTRICT
);

CREATE TABLE public."McuExaminationResultValueRevision" (
    "Id" uuid NOT NULL,
    "ResultValueId" uuid NOT NULL,
    "PreviousValueText" character varying(500),
    "ResumeVersionNumber" integer NOT NULL,
    "ChangedByUserId" uuid NOT NULL,
    "ChangedAt" timestamp with time zone NOT NULL,
    "CreateDateTime" timestamp with time zone NOT NULL,
    "CreateBy" uuid NOT NULL,
    "UpdateDateTime" timestamp with time zone,
    "UpdateBy" uuid NOT NULL,
    "DeleteDateTime" timestamp with time zone,
    "DeleteBy" uuid NOT NULL,
    "CancelDateTime" timestamp with time zone,
    "CancelBy" uuid NOT NULL,
    "IsCancel" boolean NOT NULL,
    "IsDelete" boolean NOT NULL,
    CONSTRAINT "PK_McuExaminationResultValueRevision" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_McuExaminationResultValueRevision_AspNetUsers_ChangedByUser~" FOREIGN KEY ("ChangedByUserId") REFERENCES "AspNetUsers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_McuExaminationResultValueRevision_McuExaminationResultValue~" FOREIGN KEY ("ResultValueId") REFERENCES public."McuExaminationResultValue" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_McuBookingImport_CompanyGuarantorId" ON public."McuBookingImport" ("CompanyGuarantorId");

CREATE UNIQUE INDEX "IX_McuBookingImport_ImportNumber" ON public."McuBookingImport" ("ImportNumber");

CREATE INDEX "IX_McuBookingImport_PackageId" ON public."McuBookingImport" ("PackageId");

CREATE INDEX "IX_McuDailySchedule_DutyMcuDoctorId" ON public."McuDailySchedule" ("DutyMcuDoctorId");

CREATE UNIQUE INDEX "IX_McuDailySchedule_ScheduleDate" ON public."McuDailySchedule" ("ScheduleDate") WHERE "IsDelete" = false;

CREATE UNIQUE INDEX "IX_McuExaminationField_Type_FieldCode" ON public."McuExaminationField" ("ExaminationTypeId", "FieldCode") WHERE "IsDelete" = false;

CREATE UNIQUE INDEX "IX_McuExaminationField_Type_FieldNumber" ON public."McuExaminationField" ("ExaminationTypeId", "FieldNumber") WHERE "IsDelete" = false;

CREATE UNIQUE INDEX "IX_McuExaminationResult_Component" ON public."McuExaminationResult" ("McuOrderComponentId") WHERE "IsDelete" = false;

CREATE INDEX "IX_McuExaminationResult_FilledByUserId" ON public."McuExaminationResult" ("FilledByUserId");

CREATE INDEX "IX_McuExaminationResultValue_ExaminationFieldId" ON public."McuExaminationResultValue" ("ExaminationFieldId");

CREATE UNIQUE INDEX "IX_McuExaminationResultValue_Result_Field" ON public."McuExaminationResultValue" ("ExaminationResultId", "ExaminationFieldId") WHERE "IsDelete" = false;

CREATE INDEX "IX_McuExaminationResultValueRevision_ChangedByUserId" ON public."McuExaminationResultValueRevision" ("ChangedByUserId");

CREATE INDEX "IX_McuExaminationResultValueRevision_ResultValueId" ON public."McuExaminationResultValueRevision" ("ResultValueId");

CREATE UNIQUE INDEX "IX_McuExaminationType_ExaminationTypeCode" ON public."McuExaminationType" ("ExaminationTypeCode") WHERE "IsDelete" = false;

CREATE UNIQUE INDEX "IX_McuFitnessCategory_Code" ON public."McuFitnessCategory" ("FitnessCategoryCode") WHERE "IsDelete" = false;

CREATE INDEX "IX_McuOrder_AppliedTariffId" ON public."McuOrder" ("AppliedTariffId");

CREATE INDEX "IX_McuOrder_BookingImportId" ON public."McuOrder" ("BookingImportId");

CREATE INDEX "IX_McuOrder_CheckedInByUserId" ON public."McuOrder" ("CheckedInByUserId");

CREATE INDEX "IX_McuOrder_CompanyGuarantorId" ON public."McuOrder" ("CompanyGuarantorId");

CREATE INDEX "IX_McuOrder_DailyScheduleId" ON public."McuOrder" ("DailyScheduleId");

CREATE UNIQUE INDEX "IX_McuOrder_EncounterId" ON public."McuOrder" ("EncounterId") WHERE "EncounterId" IS NOT NULL;

CREATE INDEX "IX_McuOrder_ExaminationDate_OrderStatus" ON public."McuOrder" ("ExaminationDate", "OrderStatus");

CREATE INDEX "IX_McuOrder_McuDoctorId" ON public."McuOrder" ("McuDoctorId");

CREATE UNIQUE INDEX "IX_McuOrder_OrderNumber" ON public."McuOrder" ("OrderNumber");

CREATE INDEX "IX_McuOrder_PackageId" ON public."McuOrder" ("PackageId");

CREATE INDEX "IX_McuOrder_PackageInvoiceItemId" ON public."McuOrder" ("PackageInvoiceItemId");

CREATE INDEX "IX_McuOrder_ParticipantNik" ON public."McuOrder" ("ParticipantNik");

CREATE INDEX "IX_McuOrder_PatientId" ON public."McuOrder" ("PatientId");

CREATE INDEX "IX_McuOrder_PatientMatchConfirmedByUserId" ON public."McuOrder" ("PatientMatchConfirmedByUserId");

CREATE INDEX "IX_McuOrder_RecommendingDoctorId" ON public."McuOrder" ("RecommendingDoctorId");

CREATE INDEX "IX_McuOrder_SourceEncounterId" ON public."McuOrder" ("SourceEncounterId");

CREATE INDEX "IX_McuOrderComponent_AddedByDoctorId" ON public."McuOrderComponent" ("AddedByDoctorId");

CREATE INDEX "IX_McuOrderComponent_CancelledByDoctorId" ON public."McuOrderComponent" ("CancelledByDoctorId");

CREATE INDEX "IX_McuOrderComponent_ComponentStatus" ON public."McuOrderComponent" ("ComponentStatus");

CREATE INDEX "IX_McuOrderComponent_ExaminationTypeId" ON public."McuOrderComponent" ("ExaminationTypeId");

CREATE INDEX "IX_McuOrderComponent_LabOrderId" ON public."McuOrderComponent" ("LabOrderId");

CREATE INDEX "IX_McuOrderComponent_MedicalFeeReleasedAt" ON public."McuOrderComponent" ("MedicalFeeReleasedAt");

CREATE UNIQUE INDEX "IX_McuOrderComponent_Order_ExaminationType" ON public."McuOrderComponent" ("McuOrderId", "ExaminationTypeId") WHERE "ExaminationTypeId" IS NOT NULL AND "ComponentStatus" <> 6 AND "IsDelete" = false;

CREATE UNIQUE INDEX "IX_McuOrderComponent_Order_Procedure" ON public."McuOrderComponent" ("McuOrderId", "ProcedureId") WHERE "ProcedureId" IS NOT NULL AND "ComponentStatus" <> 6 AND "IsDelete" = false;

CREATE INDEX "IX_McuOrderComponent_PackageComponentId" ON public."McuOrderComponent" ("PackageComponentId");

CREATE INDEX "IX_McuOrderComponent_PerformingDoctorId" ON public."McuOrderComponent" ("PerformingDoctorId");

CREATE INDEX "IX_McuOrderComponent_ProcedureId" ON public."McuOrderComponent" ("ProcedureId");

CREATE INDEX "IX_McuOrderComponent_RadModalityId" ON public."McuOrderComponent" ("RadModalityId");

CREATE INDEX "IX_McuOrderComponent_RadOrderId" ON public."McuOrderComponent" ("RadOrderId");

CREATE INDEX "IX_McuOrderComponent_RefundCaseId" ON public."McuOrderComponent" ("RefundCaseId");

CREATE UNIQUE INDEX "IX_McuOrderDoctorAssignment_Active" ON public."McuOrderDoctorAssignment" ("McuOrderId") WHERE "EndedAt" IS NULL AND "IsDelete" = false;

CREATE INDEX "IX_McuOrderDoctorAssignment_AssignedByUserId" ON public."McuOrderDoctorAssignment" ("AssignedByUserId");

CREATE INDEX "IX_McuOrderDoctorAssignment_DoctorId" ON public."McuOrderDoctorAssignment" ("DoctorId");

CREATE INDEX "IX_McuOrderRequirementCheck_CheckedByUserId" ON public."McuOrderRequirementCheck" ("CheckedByUserId");

CREATE INDEX "IX_McuOrderRequirementCheck_McuOrderComponentId" ON public."McuOrderRequirementCheck" ("McuOrderComponentId");

CREATE INDEX "IX_McuOrderRequirementCheck_McuOrderId" ON public."McuOrderRequirementCheck" ("McuOrderId");

CREATE INDEX "IX_McuOrderStatusHistory_ChangedByUserId" ON public."McuOrderStatusHistory" ("ChangedByUserId");

CREATE INDEX "IX_McuOrderStatusHistory_Order_ChangedAt" ON public."McuOrderStatusHistory" ("McuOrderId", "ChangedAt");

CREATE INDEX "IX_McuPackage_GeneralTariffId" ON public."McuPackage" ("GeneralTariffId");

CREATE UNIQUE INDEX "IX_McuPackage_PackageCode" ON public."McuPackage" ("PackageCode") WHERE "IsDelete" = false;

CREATE INDEX "IX_McuPackageCompanyPrice_CompanyGuarantorId" ON public."McuPackageCompanyPrice" ("CompanyGuarantorId");

CREATE INDEX "IX_McuPackageCompanyPrice_PackageId_CompanyGuarantorId" ON public."McuPackageCompanyPrice" ("PackageId", "CompanyGuarantorId");

CREATE INDEX "IX_McuPackageCompanyPrice_TariffId" ON public."McuPackageCompanyPrice" ("TariffId");

CREATE INDEX "IX_McuPackageComponent_ExaminationTypeId" ON public."McuPackageComponent" ("ExaminationTypeId");

CREATE UNIQUE INDEX "IX_McuPackageComponent_PackageId_ExaminationTypeId" ON public."McuPackageComponent" ("PackageId", "ExaminationTypeId") WHERE "ExaminationTypeId" IS NOT NULL AND "IsDelete" = false;

CREATE UNIQUE INDEX "IX_McuPackageComponent_PackageId_ProcedureId" ON public."McuPackageComponent" ("PackageId", "ProcedureId") WHERE "ProcedureId" IS NOT NULL AND "IsDelete" = false;

CREATE INDEX "IX_McuPackageComponent_ProcedureId" ON public."McuPackageComponent" ("ProcedureId");

CREATE INDEX "IX_McuPackageComponent_RadModalityId" ON public."McuPackageComponent" ("RadModalityId");

CREATE INDEX "IX_McuPackageComponentRequirement_PackageComponentId" ON public."McuPackageComponentRequirement" ("PackageComponentId");

CREATE INDEX "IX_McuQuotaOverride_ApprovedByUserId" ON public."McuQuotaOverride" ("ApprovedByUserId");

CREATE INDEX "IX_McuQuotaOverride_DailyScheduleId" ON public."McuQuotaOverride" ("DailyScheduleId");

CREATE UNIQUE INDEX "IX_McuReleaseConsent_Active" ON public."McuReleaseConsent" ("McuOrderId") WHERE "RevokedAt" IS NULL AND "IsDelete" = false;

CREATE INDEX "IX_McuReleaseConsent_CompanyGuarantorId" ON public."McuReleaseConsent" ("CompanyGuarantorId");

CREATE INDEX "IX_McuReleaseConsent_RecordedByUserId" ON public."McuReleaseConsent" ("RecordedByUserId");

CREATE INDEX "IX_McuReleaseConsent_RevokedByUserId" ON public."McuReleaseConsent" ("RevokedByUserId");

CREATE INDEX "IX_McuResultDelivery_DeliveredByUserId" ON public."McuResultDelivery" ("DeliveredByUserId");

CREATE INDEX "IX_McuResultDelivery_McuResumeId" ON public."McuResultDelivery" ("McuResumeId");

CREATE INDEX "IX_McuResultDelivery_Order" ON public."McuResultDelivery" ("McuOrderId");

CREATE INDEX "IX_McuResultDelivery_RequiresCorrectionNotice" ON public."McuResultDelivery" ("RequiresCorrectionNotice");

CREATE INDEX "IX_McuResume_ApprovedByDoctorId" ON public."McuResume" ("ApprovedByDoctorId");

CREATE INDEX "IX_McuResume_AuthorDoctorId" ON public."McuResume" ("AuthorDoctorId");

CREATE INDEX "IX_McuResume_FitnessCategoryId" ON public."McuResume" ("FitnessCategoryId");

CREATE UNIQUE INDEX "IX_McuResume_Order_Current" ON public."McuResume" ("McuOrderId") WHERE "IsCurrent" = true AND "IsDelete" = false;

CREATE UNIQUE INDEX "IX_McuResume_Order_Version" ON public."McuResume" ("McuOrderId", "VersionNumber");

CREATE INDEX "IX_McuResume_ReopenedByDoctorId" ON public."McuResume" ("ReopenedByDoctorId");

CREATE INDEX "IX_McuResume_VerifiedByUserId" ON public."McuResume" ("VerifiedByUserId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261021073245_AddMedicalCheckupManagement', '9.0.18');

COMMIT;

```

</details>
