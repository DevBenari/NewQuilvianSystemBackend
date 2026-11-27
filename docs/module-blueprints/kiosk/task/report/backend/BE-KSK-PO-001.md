# Laporan Perubahan Backend — `BE-KSK-PO-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-KSK-PO-001` |
| Judul | Skema pendaftaran online dan migration |
| Slice | `EPIC KSK-PO-01` — fondasi data (`FR-KSK-PO-001`), gelombang teknis 1, rilis `MVP-0` |
| Roadmap | [online-registration-backend-roadmap.md](../../../roadmap/online-registration-backend-roadmap.md) — kartu `BE-KSK-PO-001` |
| Trace | `KSK-DSN-PO-001`, `002`, `003`, `014`; `INV-PO-03`, `05`, `07`, `08`; `data/data-dictionary.md` PO-D.2–D.4, DDL PO-D.6; `02-backend-architecture.md` PO.6, PO.9–PO.11 |
| Contract version | `KSK-CONTRACT-v2` — `approved` (Sukma Giri Pratama, 2026-11-19, `KSK-DEC-054`) |
| Dependency | — (tidak ada prasyarat) |
| Klasifikasi | `MEDIUM` — tiga tabel baru, lima enum baru, dua nilai enum tambahan, satu migration aditif |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/RegistrationManagement/{Enums,Models,Services}`, `Repositories/Configurations/HealthServices/`, `Repositories/ApplicationDbContext.cs`, `Program.cs`, `appsettings.json`, `Migrations/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `a86001e2` (`sukmagp`); source aplikasi identik dengan baseline `5e777833` |
| Tanggal | 2026-11-19 |
| Status | ✅ Selesai — migration dibuat dan diterapkan ke `QuilvianNewDevSukma` atas izin terpisah |

**Backend Governance Preflight**

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `HealthServices` / `RegistrationManagement` / Registration |
| Owner/prefix registry | `Reg` = *Registration*, `ACTIVE / LEGACY` (`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 25) |
| Keberlakuan | `NEW CODE` |
| QBE yang berlaku | `QBE-MOD-001`, `QBE-NAM-001`, `QBE-NAM-002` (prefix `Reg`, bukan `Trx*`), `QBE-ENT-001`/`002`/`003` (entity mewarisi `IdentityModel`, kunci `Guid`), `QBE-CFG-001` (`IEntityTypeConfiguration<T>` lengkap), `QBE-ENUM-001` (enum disimpan sebagai integer), `QBE-DB-001`/`002` (migration aditif dan dapat dibalik) |
| QBE yang tidak berlaku | `QBE-API-001`, `QBE-PERM-001`, `QBE-SVC-001`, `QBE-CODE-*` — belum ada endpoint, service, maupun alokasi nomor pada task ini |

## 1. Masalah yang diperbaiki

Backend belum punya tempat untuk menyimpan pendaftaran yang diisi pasien sendiri sebelum menjadi kunjungan (`F-PO-1`, `CAP-PO-01`). Task ini membangun wadah itu: satu tabel pendaftaran online, satu tabel berkas surat rujukan, dan satu tabel riwayat, beserta status dan konfigurasinya. Belum ada layar atau endpoint yang memakainya; pemakainya mulai `BE-KSK-PO-002`.

## 2. Proses bisnis

Tabel baru mengikuti siklus hidup yang disetujui (`contracts/state-transition-matrix.md` PO-S.1):

1. Pasien mengirim halaman online. Satu baris `RegOnlineRegistration` lahir dengan status **Menunggu Check-in** (`1`), bersama riwayat `SUBMITTED`. Bila pasien mengunggah surat rujukan, setiap berkas menjadi satu baris `RegOnlineRegistrationReferralDocument`.
2. Pada hari kunjungan, check-in di kiosk mengubah status menjadi **Menjadi Kunjungan** (`2`) dan mengisi `EncounterId`.
3. Bila tidak dipakai, status berakhir sebagai **Didaftarkan lewat Jalur Lain** (`3`), **Tidak Datang** (`4`), atau **Dibatalkan** (`5`).

Penjaga di tingkat basis data:

| Penjaga | Contoh yang ditolak database |
| --- | --- |
| Kode unik hanya di antara Menunggu Check-in (`IX_RegOnlineRegistration_CodeHash_Awaiting`, `WHERE "Status" = 1`) | Dua pendaftaran aktif dengan hash kode sama. Kode yang sudah dipakai (status akhir) tidak menghalangi kode baru yang kebetulan sama |
| Satu kunjungan hanya milik satu pendaftaran (`IX_RegOnlineRegistration_EncounterId`, `WHERE "EncounterId" IS NOT NULL`) | Dua pendaftaran yang menunjuk kunjungan yang sama |
| Kontak wajib salah satu (`CK_RegOnlineRegistration_Contact`) | Pendaftaran tanpa No. WhatsApp maupun email (`KSK-DEC-043`) |
| Seluruh relasi `ON DELETE RESTRICT` | Menghapus pasien, jadwal, atau kunjungan yang masih dirujuk pendaftaran online |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `RegPriorityReservation.cs`, `RegPriorityReservationHistory.cs` beserta configuration-nya (pola acuan); `RegPatientGeneralConsentConfiguration.cs` (pemetaan `integer[]`); `PriorityReservationOptions.cs`; `Repositories/ApplicationDbContext.cs`; `QuilvianSystemBackend.csproj` (aturan arsip Designer migration); namespace `MstClinic`, `MstDoctorSchedule`, `MstPatient`, `MstPatientInsurance`, `MstPatientCompanyGuarantor`, `MstInsuranceProvider`, `MstReferralInstitution`, `RegEncounterReferralDocument`, `Gender`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/RegistrationManagement/Enums/OnlineRegistrationStatus.cs` | **Baru.** 1 Menunggu Check-in, 2 Menjadi Kunjungan, 3 Didaftarkan lewat Jalur Lain, 4 Tidak Datang, 5 Dibatalkan |
| `.../Enums/OnlineRegistrationPatientMode.cs`, `OnlineRegistrationPatientType.cs`, `OnlineRegistrationEventSource.cs`, `OnlineRegistrationEmailStatus.cs` | **Baru.** Nilai sesuai `02-backend-architecture.md` PO.6 |
| `.../Enums/EncounterRegistrationSource.cs` | Tambah `OnlineRegistration = 7` |
| `.../Enums/ReferralCaptureSource.cs` | Tambah `OnlineRegistration = 3` |
| `.../Models/RegOnlineRegistration.cs` | **Baru.** Aggregate root, kolom sesuai PO-D.2 |
| `.../Models/RegOnlineRegistrationReferralDocument.cs` | **Baru.** Sesuai PO-D.3 |
| `.../Models/RegOnlineRegistrationHistory.cs` | **Baru.** Sesuai PO-D.4, beserta konstanta kode kejadian |
| `.../Services/OnlineRegistrationOptions.cs`, `OnlineRegistrationClosureOptions.cs` | **Baru.** Section `HealthServices:Registration:OnlineRegistration` |
| `Repositories/Configurations/HealthServices/RegOnlineRegistration*Configuration.cs` (3 berkas) | **Baru.** Panjang kolom, konversi enum, bawaan, index, unique bersyarat, check constraint, FK `Restrict` |
| `Repositories/ApplicationDbContext.cs` | Tiga `DbSet` |
| `Program.cs` | `Configure<OnlineRegistrationOptions>` |
| `appsettings.json` | Bagian `HealthServices:Registration:OnlineRegistration` (tanpa nilai rahasia) |
| `Migrations/20261119134155_AddRegOnlineRegistration.cs`, `.Designer.cs`, `ApplicationDbContextModelSnapshot.cs` | Migration baru beserta snapshot |

**Delta terhadap desain (tidak mengubah kontrak):**

| Desain | Source | Alasan |
| --- | --- | --- |
| Configuration di `Repositories/Configurations/HealthServices/RegistrationManagement/` | `Repositories/Configurations/HealthServices/` | Seluruh configuration `Reg*` yang ada (`RegPriorityReservation`, `RegPatientEncounter`, dan lainnya) berada di folder itu; source yang berlaku |
| Section `Registration:OnlineRegistration` | `HealthServices:Registration:OnlineRegistration` | `Registration` di `appsettings.json` berada di bawah `HealthServices`, sama dengan `PriorityReservation` |
| Kamus data mendaftar 7 index pada `RegOnlineRegistration` | Migration membuat 12 index | EF otomatis menambah index untuk setiap kolom FK yang belum tertutup index komposit (`ClinicId`, `PatientInsuranceId`, `PatientCompanyGuarantorId`, `NewInsuranceProviderId`, `ReferralInstitutionId`). Ditambah 1 index FK pada tabel dokumen |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — belum ada endpoint. Nilai enum baru `7` dan `3` belum dipakai kode mana pun |
| Database | Tiga tabel baru, 15 index, 11 FK `RESTRICT`, satu check constraint. Tidak ada tabel lama yang diubah. Migration **diterapkan** ke `QuilvianNewDevSukma` pada 2026-11-19 atas izin terpisah pemilik |
| Keamanan/Auth | Kolom sensitif ditandai di model (komentar XML) sesuai PO-D.2–D.4. Kode dan token hanya disimpan sebagai hash. Tidak ada rahasia di `appsettings.json` |

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak membuat atau mengubah endpoint.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build ... -o <scratchpad>` | Build succeeded, 0 error; 0 warning di berkas baru | `PASS` | Keluaran build |
| `dotnet ef migrations has-pending-model-changes --configuration Release` sebelum migration | "Changes have been made to the model" | `PASS` (diharapkan) | Keluaran EF |
| `dotnet ef migrations add AddRegOnlineRegistration --configuration Release` | Done | `PASS` | Konfigurasi Release dipakai karena backend pengguna sedang berjalan dari `bin/Debug` (berkas terkunci) |
| Isi migration | 3 `CreateTable`, 15 `CreateIndex`, 11 FK `ReferentialAction.Restrict`, 1 `CheckConstraint`; `Down` = 3 `DropTable`; tidak ada operasi pada tabel lain | `PASS` | Pemeriksaan isi berkas migration |
| `dotnet ef migrations has-pending-model-changes` sesudah migration | "No changes have been made to the model since the last migration." | `PASS` | Keluaran EF |
| `dotnet ef migrations script AddProcurementVendorManagement AddRegOnlineRegistration` | SQL cocok dengan DDL PO-D.6: tipe kolom (`date`, `integer[]` bawaan `'{}'`, `text`, `timestamp with time zone`, `uuid`), panjang, unique bersyarat, check constraint, FK `RESTRICT` | `PASS` | Skrip SQL di scratchpad (tidak di-commit) |
| `dotnet ef migrations list` sebelum penerapan | Hanya `20261119134155_AddRegOnlineRegistration (Pending)`; tidak ada migration modul lain yang tertinggal | `PASS` | Keluaran EF |
| Target database | `QuilvianNewDevSukma` (hanya nama database yang dibaca dari konfigurasi; nilai rahasia tidak ditampilkan) | `PASS` | Pemeriksaan konfigurasi |
| `dotnet ef database update AddRegOnlineRegistration` | "Applying migration '20261119134155_AddRegOnlineRegistration'. Done." | `PASS` | Keluaran EF |
| `dotnet ef migrations list` sesudah penerapan | 0 Pending | `PASS` | Keluaran EF |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` | 24 berkas, VIOLATION 0, REVIEW 0 | `PASS` | Keluaran QBE |
| Kueri baca-saja ke tabel baru di DB | Tidak dijalankan; eksekusi DB langsung di luar izin task | `NOT RUN` | Bukti penerapan berasal dari `migrations list` |
| `dotnet test` | — | `NOT RUN` | Tidak ada project test (pola Bank Darah) |

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Kolom, tipe, panjang, bawaan, index (termasuk unique bersyarat `CodeHash` saat `Status = 1` dan `EncounterId` bukan null), check constraint kontak, dan FK `Restrict` persis kamus data | Terpenuhi | Isi migration dan skrip SQL; index FK tambahan dicatat sebagai delta |
| (2) `Down` menghapus ketiga tabel | Terpenuhi | `Down`: `DropTable` History, ReferralDocument, lalu RegOnlineRegistration |
| (3) `has-pending-model-changes` bersih sesudah migration | Terpenuhi | "No changes have been made…" |
| (4) Build lulus | Terpenuhi | Build succeeded |
| (5) `appsettings.json` tidak memuat `SecretKey` maupun `Password` berisi | Terpenuhi | Bagian `Turnstile` dan `Email:Smtp` kosong; bagian `OnlineRegistration` tanpa rahasia |
| DoD: penerapan ke DB hanya dengan izin | Terpenuhi | Izin terpisah pemilik untuk membuat dan menerapkan migration, 2026-11-19 |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Backend yang sedang berjalan (`bin/Debug`) masih memakai kode lama. Tabel baru sudah ada di DB, dan kode lama tidak menyentuhnya, sehingga aman. Muat ulang backend sebelum menguji task berikutnya |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | Designer migration (6,6 MB) ikut dikompilasi sampai diarsipkan lewat `tooling/migrations/Update-MigrationHistory.ps1`, sesuai aturan csproj. Pengarsipan tidak dijalankan di task ini |
| Perubahan sampingan | `NONE` di repository. Keluaran build Release berada di `bin/Release` (tidak dilacak); skrip SQL di scratchpad |
| Interupsi | `NONE` |
| Status Git | 6 berkas `M` (`EncounterRegistrationSource.cs`, `ReferralCaptureSource.cs`, `ApplicationDbContextModelSnapshot.cs`, `Program.cs`, `ApplicationDbContext.cs`, `appsettings.json`) dan 15 berkas `??` baru milik task ini, ditambah berkas `BE-KSK-PO-003` yang belum di-commit serta dokumen kiosk |
| Langkah berikutnya | `BE-KSK-PO-002` (kuota menghitung pendaftaran online dan kunci kuota per jadwal) |
