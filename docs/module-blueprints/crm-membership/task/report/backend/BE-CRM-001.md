# Laporan Perubahan Backend — `BE-CRM-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-CRM-001` |
| Judul | Skema membership |
| Slice | `MVP-0`, `EPIC CRM-01` — Fondasi data |
| Roadmap | `docs/module-blueprints/crm-membership/roadmap/backend-roadmap.md` § `BE-CRM-001` |
| Trace | `FR-CRM-001`, `FR-CRM-002`; `CRM-DEC-004`, `012`, `013`, `017`, `021`, `023`, `024`; `erd/data-dictionary.md` r1; `contracts/state-transition-matrix.md` v1 |
| Contract version | Kamus data r1 dan state `v1`, `approved` (Yoga Aji Pratama, 2026-10-13) |
| Dependency | `BE-CRM-000` (registry folder `PatientManagement/Membership`) — selesai 2026-10-13 |
| Klasifikasi | `MEDIUM`: model + konfigurasi + satu migration, tanpa endpoint dan tanpa perubahan perilaku runtime |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/`, `Repositories/`, `Migrations/`, dan laporan ini |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `761bfe9d` (branch `yoga`) |
| Tanggal | 2026-10-13 |
| Status | Selesai. Migration diterapkan ke `QuilvianNewDevYoga` 2026-10-13 bersama `BE-CRM-002` (bukti di laporan `BE-CRM-002` §8) |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `HealthServices` / `PatientManagement` / `Membership` (baru) dan `MasterData` (sudah ada); `Administrator` / `MasterData` untuk tingkat |
| Registry | `PatientManagement / Membership / Patient Membership` — `Pat` — `ACTIVE` (2026-10-13, Muhammad Hamzah); `Master / Reference / MasterData` — `Mst` — `ACTIVE` |
| Keberlakuan | `NEW CODE`: `PatMembershipStatusHistory`, tiga enum, satu konfigurasi. `TOUCHED LEGACY`: `MstPatientMembership`, `MstMembershipTier`, `MembershipStatus`, dua konfigurasi, `ApplicationDbContext` |
| QBE yang berlaku | `QBE-MOD-003` (folder didaftarkan sebelum model), `QBE-NAM-002` (prefix `Pat`), `QBE-NAM-001` (tanpa `Trx*`), `QBE-ENT-001`/`QBE-CFG-001` (entity + konfigurasi sepaket), `QBE-CODE-003` (riwayat tanpa nomor urut Max+1), `QBE-AUD-001` (riwayat sebagai data audit, bukan log) |
| Tidak berlaku pada task ini | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-CODE-001`/`002` (tidak ada service, endpoint, atau alokasi nomor) |

## 1. Masalah yang diperbaiki

Data membership per pasien sudah ada, tetapi tidak bisa menyimpan siklus hidup yang sudah
diputuskan owner. Tidak ada tempat untuk mencatat:

- status Menunggu Pembayaran, Terjadwal, dan Perlu Verifikasi;
- tanggal mulai periode, terpisah dari tanggal daftar;
- batas bayar 7 hari;
- nominal iuran yang dibayar dan nominal iuran per tingkat;
- membership mana yang diperpanjang atau di-upgrade;
- alasan pembatalan dan jejak riwayat status.

Contoh: Ibu Sari (data samaran) memperpanjang Gold. Sebelum task ini, sistem tidak punya
kolom untuk menyatakan "perpanjangan ini menunggu bayar sampai 20 Okt 2026 09.00 dan
menyambung Gold yang berakhir 12 Okt 2027".

Task ini hanya menyiapkan **wadah data**. Aturan yang mengisinya dikerjakan task
`BE-CRM-002` s.d. `BE-CRM-020`.

## 2. Proses bisnis

Tidak ada proses bisnis yang berubah pada task ini. Endpoint lama tetap bekerja seperti
sebelumnya. Perubahan yang terasa setelah migration diterapkan:

1. Membership baru yang dibuat tanpa status eksplisit tersimpan sebagai Menunggu Pembayaran
   (nilai 7), bukan Aktif. Endpoint lama selalu mengirim status dari request, sehingga
   perilakunya tidak berubah.
2. Membership lama mendapat nilai bawaan: jenis pendaftaran `New` (1), iuran 0, dan
   `RowVersion` bernilai nol. `BE-CRM-002` mengubahnya menjadi `LegacyImport` / Perlu
   Verifikasi dan mengisi `RowVersion` baru.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`, `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`,
`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, `rules/backend/DATABASE_RULES.md`,
`rules/backend/TASK_RULES.md`, kamus data dan state matrix blueprint, `MstPatientMembership.cs`,
`MstMembershipTier.cs`, `MembershipStatus.cs`, kedua konfigurasinya, `ApplicationDbContext.cs`,
pola pembanding `HmdMachineStatusHistory` + konfigurasinya, `BilInvoiceConfiguration.cs`
(pola `RowVersion`), serta semua pembuat `MstPatientMembership` dan pengguna `MembershipStatus`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Administrator/MasterData/Enums/MembershipStatus.cs` | Tambah `PendingPayment = 7`, `Scheduled = 8`, `NeedsVerification = 9`. Nilai lama tidak diubah |
| `Areas/Administrator/MasterData/Models/MstMembershipTier.cs` | Tambah `AnnualFeeAmount` |
| `Areas/HealthServices/PatientManagement/MasterData/Models/MstPatientMembership.cs` | Tambah 10 properti (`RegistrationType`, `PeriodStartDate`, `PaymentDueAt`, `ActivatedAt`, `FeeAmountSnapshot`, `PreviousPatientMembershipId`, `CancellationCategory`, `CancellationReason`, `FeeChargeReferenceId`, `RowVersion`) + dua navigation. Bawaan status menjadi `PendingPayment` |
| `Areas/HealthServices/PatientManagement/Membership/Enums/MembershipRegistrationType.cs` | Baru |
| `Areas/HealthServices/PatientManagement/Membership/Enums/MembershipCancellationCategory.cs` | Baru |
| `Areas/HealthServices/PatientManagement/Membership/Enums/MembershipChangeSource.cs` | Baru |
| `Areas/HealthServices/PatientManagement/Membership/Models/PatMembershipStatusHistory.cs` | Baru |
| `Repositories/Configurations/HealthServices/MstPatientMembershipConfiguration.cs` | Konfigurasi kolom baru, default status 7, `RowVersion` concurrency token, FK self `Restrict`, 4 index non-unik |
| `Repositories/Configurations/Global/MstMembershipTierConfiguration.cs` | `AnnualFeeAmount` `numeric(18,2)` default 0 |
| `Repositories/Configurations/HealthServices/PatientManagement/Membership/PatMembershipStatusHistoryConfiguration.cs` | Baru: tabel, enum sebagai int, index `(PatientMembershipId, OccurredAt)`, FK `Restrict` |
| `Repositories/ApplicationDbContext.cs` | DbSet `PatMembershipStatusHistories` + `using` |
| `Migrations/20261013051652_ExtendPatientMembershipLifecycle.cs` + `.Designer.cs` | Baru, dibuat `dotnet ef migrations add` |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Diperbarui otomatis, hanya bagian membership (+154/−1 baris) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE`: tidak ada endpoint atau DTO yang berubah |
| Database | Migration `ExtendPatientMembershipLifecycle`: 10 kolom baru di `MstPatientMembership`, 1 kolom di `MstMembershipTier`, tabel `PatMembershipStatusHistory`, 5 index non-unik, 2 FK `Restrict`, dan default `MembershipStatus` 1 → 7. Semua kolom nullable atau berbawaan, jadi tanpa downtime. **Belum diterapkan** ke database mana pun |
| Keamanan/Auth | `NOT APPLICABLE`. Kolom sensitif baru (`CancellationReason`, `Reason`, `OldNotes`, `NewNotes`) belum dibaca maupun dilog kode mana pun |

## 4. Dokumentasi endpoint

`NOT APPLICABLE`: task ini tidak menyentuh endpoint.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` (sebelum migration) | 0 error, 1 menit 18 detik | `PASS` | Log build scratchpad |
| `dotnet ef migrations add ExtendPatientMembershipLifecycle --no-build` | Migration terbentuk. Isinya hanya perubahan task ini | `PASS` | Daftar operasi `Up`/`Down` diperiksa satu per satu |
| `dotnet build` ulang (setelah migration) | 0 error, 285 peringatan lama, 48 detik | `PASS` | Peringatan berasal dari kode yang tidak disentuh, misalnya `CS8619` di `PatientMembershipController.cs:1159` |
| `dotnet ef migrations script 20261012080102_AddEmergencyAdmissionRequest ExtendPatientMembershipLifecycle` (offline) | SQL sesuai DDL kamus data | `PASS` | Lihat catatan di bawah |
| Selisih snapshot | Hanya entitas membership, tier, dan riwayat | `PASS` | `git diff` snapshot difilter, tidak ada baris di luar membership |
| QBE checker `ReportOnly`, 14 berkas | 0 VIOLATION, 0 REVIEW, 0 INFO | `PASS` | Keluaran `Invoke-QbeConformanceCheck.ps1` |
| `dotnet ef migrations list` / `database update` | — | `NOT RUN` | Butuh akses database. Tidak ada izin eksekusi DB pada task ini |
| `dotnet test` | — | `NOT RUN` | Tidak ada project test yang menyentuh membership. Task ini tanpa perilaku runtime |

Catatan perbandingan SQL dengan kamus data:

- Nama FK dipotong EF menjadi 63 karakter berakhiran `~`, misalnya
  `FK_MstPatientMembership_MstPatientMembership_PreviousPatientMe~`. Perilakunya sama, hanya
  namanya berbeda dari contoh DDL di kamus data, yang hanya dokumentasi bentuk.
- `RowVersion` untuk baris lama terisi `00000000-0000-0000-0000-000000000000`. Ini sesuai
  rencana, dan nilai baru diisi `BE-CRM-002`.
- Index unik bersyarat dan unik `FeeChargeReferenceId` sengaja **tidak** ada. Keduanya jatah `BE-CRM-002`.

Uji manual: `NOT APPLICABLE`, karena tidak ada perilaku yang dapat dicoba pengguna.

**Tidak dijalankan:** penerapan migration dan pemeriksaan daftar migration di database. Keduanya
butuh izin eksekusi database terpisah.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Kolom, tipe, dan bawaan persis kamus data | Terpenuhi | Skrip SQL offline vs `erd/data-dictionary.md` |
| (2) FK self dan FK riwayat `Restrict` | Terpenuhi | `ON DELETE RESTRICT` pada kedua FK di skrip SQL |
| (3) `Down` migration membalik semua perubahan | Terpenuhi (dibuktikan dengan membaca kode `Down`, belum dieksekusi) | `Down` menghapus FK, tabel, 4 index, 10 kolom, kolom tier, dan mengembalikan default status ke 1 |
| (4) Build `-p:RunAnalyzers=False` lulus | Terpenuhi | Dua build, 0 error |
| Verifikasi `migrations list` menunjukkan satu migration tertunda | **Diganti** dengan skrip SQL offline | Akses database tidak diizinkan. Bukti pengganti: skrip berisi tepat satu `INSERT INTO "__EFMigrationsHistory"` untuk migration ini |
| Laporan task tertulis | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Snapshot migration bisa bentrok bila branch lain membawa migration baru sebelum task ini digabung. Periksa ulang urutan migration sebelum merge |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | Bila migration diterapkan **tanpa** `BE-CRM-002`, membership lama tetap terbaca Aktif oleh kode lama (perilaku sama dengan sekarang), dan `RowVersion`-nya nol. Terapkan keduanya bersama |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE`. Percobaan pertama `migrations script` gagal karena assembly belum memuat migration baru. Diselesaikan dengan build ulang, tanpa perubahan kode |
| Status Git | `M` 7 berkas (enum, dua model, snapshot, DbContext, dua konfigurasi); `??` folder `Areas/HealthServices/PatientManagement/Membership/`, dua berkas migration, folder `Repositories/Configurations/HealthServices/PatientManagement/`, dan laporan ini. Belum di-commit |
| Langkah berikutnya | `BE-CRM-002` (data lama menjadi Perlu Verifikasi + index unik per status), `BE-CRM-003` (iuran di master tingkat), `BE-CRM-023` (respons diperluas). Ketiganya kini bisa mulai |
