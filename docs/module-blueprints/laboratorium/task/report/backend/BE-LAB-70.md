# Laporan Perubahan Backend — `BE-LAB-70`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-70` |
| Judul | Skema validasi dan rilis |
| Slice | Gelombang `MVP-9a` — `EPIC-LAB-15`, fondasi `S4` validasi dan rilis hasil |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian **6ak.1** |
| Trace | Fondasi `FR-15.1`..`FR-15.13`; `LAB-DEC-080`, `LAB-DEC-150` butir 4, `LAB-DEC-017`, `LAB-COORD-002`; `INV-42`, `INV-43` |
| Contract version | `02-backend-architecture.md` 20.4-20.9 dan `erd/data-dictionary.md` bagian 17 — disetujui bersama `LAB-API-v1` `r34`, `LAB-VAL-v1` `r12`, `LAB-PERM-v1` rev 11, `LAB-STATE-v1` `r5`, `LAB-INT-v1` `r4` pada **2026-09-25**. **Satu koreksi disetujui pemilik modul 2026-09-29:** `ClinicalDocumentKind.LaboratoryResult = 15`, bukan `14` |
| Dependency | Backend `MVP-8` (`BE-LAB-67`..`69`) ⚠ berdiri pada kode, 2026-09-29 |
| Klasifikasi | `MEDIUM` — skor 6: repository 0, berkas diperiksa 2 (±22), berkas diubah 2 (12 source dan migration), logika bisnis 0, kontrak API 0, database **2** (tabel, kolom, FK, migration), keamanan/auth 0, UI/workflow 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/LaboratoryManagement/`, `Repositories/`, `Migrations/`; satu nilai enum di `Areas/HealthServices/MedicalRecordManagement/Enums/`; dokumen blueprint Laboratorium |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `017d1819` (branch `yoga`), di atas `BE-LAB-68` dan `BE-LAB-69` yang belum ter-commit |
| Tanggal | 2026-09-29 |
| Status | ✅ **`SELESAI`** — 2026-09-29 sore (bagian 8). Atas instruksi eksplisit pemilik modul, migration diterapkan ke `QuilvianNewDevYoga` sesudah `AddHemodialysisManagement`, lewat skrip per migration — **bukan** `database update`, yang akan ikut menerapkan dua belas migration modul lain. **Naik → turun → naik terbukti** pada database sungguhan; kesepuluh baris lama kosong pada ke-14 kolom; startup lolos dengan registri 1572. *Semula `SELESAI SEBAGIAN`: migration belum diterapkan* |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `LaboratoryManagement`; satu nilai enum pada `HealthServices` / `MedicalRecordManagement` |
| Pemilik / prefix registry | `Lab` — `ACTIVE` (`MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 27 dan 114). Entri 2026-09-02 menyatakan wewenang modul ini **mencakup source dan pembuatan migration**; **eksekusi database dan deployment tetap wewenang terpisah** |
| Keberlakuan | `NEW CODE` untuk kedua model dan configuration alasan serta kelima enum; `TOUCHED LEGACY` untuk `LabExamination`, configuration-nya, `ApplicationDbContext`, dan `ClinicalDocumentKind` |
| QBE yang berlaku | `QBE-NAM-001` (tanpa `Trx*`), `QBE-NAM-002` (prefix registry `Lab`), `QBE-ENT-001`..`003` (turunan `IdentityModel`, `Guid` Id, nullability sesuai domain, nol kolom presentasi), `QBE-CFG-001` (`IEntityTypeConfiguration` dengan key, index, dan relasi), `QBE-MOD-001` (kedua data induk di bawah `LaboratoryManagement`), `QBE-ENUM-001` (kelima enum milik Laboratorium; `ClinicalDocumentKind` tetap milik Rekam Medis, hanya ditambah satu nilai lewat `LAB-COORD-002`), `QBE-DEL-001` (index unik parsial `IsDelete = false`, nol penghapusan fisik). `QBE-DB-001`/`002` **tidak berlaku** — keduanya untuk `LEGACY MIGRATION` (rename), dan migration ini murni aditif |
| Governance yang dibaca | `AGENTS.md` backend; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `rules/backend/` suite Skill; `tooling/migrations/Update-MigrationHistory.ps1` |

---

## 1. Masalah yang diperbaiki

**Sistem belum punya tempat menyimpan siapa yang mengesahkan hasil laboratorium.** Cetakan hasil
Patologi Klinik memuat dua baris pengesah terpisah — *Validasi oleh* dan *Otorisasi oleh*
(`LAB-EVD-005`) — tetapi `LabExamination` hanya mencatat pengisian hasil dan Final. Task
`BE-LAB-73`..`77` akan membangun tindakan Validasi, Rilis, dan *Kembalikan ke analis*. Semuanya
membutuhkan tempat yang sudah berdiri lebih dulu.

Tempat itu mencakup:

| Yang dicatat | Contoh | Kenapa perlu disalin, bukan dirujuk |
| --- | --- | --- |
| Kapan dan oleh siapa hasil divalidasi | dr. Bima, 09.40 | — |
| **Jabatannya saat itu** | *Dokter Penanggung Jawab Laboratorium* | Bila jabatannya kelak dinamai ulang atau ia pindah jabatan, dokumen lama tidak boleh ikut berubah (`AC-239`) |
| **Dasar kewenangannya** | baris kredensial Human Resource yang berlaku | Membuktikan pengesah memang ditunjuk pada saat itu |
| **Alasan merangkap peran**, bila ada | *Shift tunggal, tidak ada dokter lain bertugas* | Penanda ini **tercetak**; ejaan alasan yang kelak dibetulkan tidak boleh mengubah bunyi hasil lama |

Task ini sengaja **tidak mengubah perilaku apa pun**: nol endpoint, nol aturan. Ia hanya menyiapkan
wadahnya.

---

## 2. Proses bisnis

Task ini tidak menjalankan proses bisnis, tetapi menyiapkan tempat bagi proses yang dibangun
`BE-LAB-73`..`77`. Urutannya diringkas di sini supaya arti setiap kolom terbaca.

1. Analis mengisi hasil dan menekan **Final** — sudah ada sejak `MVP-8`.
2. Dokter berkewenangan **memvalidasi**. Terisi: `ValidatedAt`, `ValidatedByUserId`, jabatan,
   nama jabatan saat itu, dan dasar kewenangan.
3. Bila pemvalidasi juga pengisi hasil (`INV-42`), ia wajib memilih alasan dari
   **`LabFourEyesExceptionReason`**. Nama alasannya disalin ke
   `ValidationExceptionReasonNameSnapshot`.
4. Perilis **merilis**. Terisi lima kolom `Released*` dengan pola yang sama. Bila perilis juga
   pemvalidasi (`INV-43`), alasannya ikut tercatat — jalur yang nyata pada shift malam.
5. Bila perilis menemukan kesalahan sebelum rilis, ia **mengembalikan ke analis** dengan alasan
   dari **`LabResultCorrectionReason`**, misalnya *Sampel tertukar*. `ValidatedAt` dikosongkan,
   dan jejaknya tetap pada `LabTransitionHistory`.

**Aturan yang dijaga skema ini.**

- **Kosong berarti belum terjadi.** Seluruh baris `LabExamination` yang sudah ada memang belum
  pernah divalidasi atau dirilis, sehingga **nol pengisian data lama** — mengisinya berarti
  mengarang pengesah.
- **Tetap fakta, bukan status** (`LAB-DEC-080`). `LabExaminationStatus` tetap empat nilai
  (`Ordered`, `ChargeEligible`, `Voided`, `Cancelled`). Keadaan *Tervalidasi* dan *Dirilis*
  diturunkan dari kolom waktu lewat enum `LabResultStatus`, yang **tidak disimpan**.
- **Alasan tidak pernah dihapus.** Keduanya dinonaktifkan, dan hasil yang sudah memakai alasan
  pengecualian menunjuknya lewat foreign key `Restrict`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / dokumen | Kenapa dibaca |
| --- | --- |
| `roadmap/backend-roadmap.md` 6ak dan 6ak.1 | Cakupan tujuh butir, verifikasi, dua jebakan |
| `erd/data-dictionary.md` 17.1-17.7 | Kolom, tipe, nilai bawaan, dan **DDL 17.6** sebagai acuan bentuk |
| `02-backend-architecture.md` 20.4-20.9 | Isi kelima enum, `ClinicalDocumentKind`, rencana migration, larangan menyentuh `JenisYangDitegakkan` |
| `Models/LabOrganism.cs`, `LabOrganismConfiguration.cs`, `MstLabRejectionReasonConfiguration.cs` | Pola data induk dan index unik parsial |
| `Models/LabExamination.cs`, `LabExaminationConfiguration.cs` | Tempat 14 kolom, pola kolom pengguna tanpa FK, komentar yang harus diperbarui |
| `MedicalRecordManagement/Enums/ClinicalDocumentKind.cs` | **Menemukan `14` sudah dipakai `HemodialysisSession`** |
| Seluruh pemakaian `ClinicalDocumentKind` di source | Memastikan nilai baru tidak terbuka di jalur Rekam Medis mana pun tanpa sengaja |
| `Migrations/MigrationMetadata.g.cs`, `tooling/migrations/Update-MigrationHistory.ps1` | Alur kerja migration repository ini sejak Designer diarsipkan |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/.../Models/LabResultCorrectionReason.cs` | **Baru.** `ReasonCode`, `ReasonName`, `Description`, `RequiresNote`, `IsActive`, `SortOrder` |
| `Areas/.../Models/LabFourEyesExceptionReason.cs` | **Baru.** Kolom sama — tabelnya sengaja terpisah (`LAB-DA-001` A5.4) |
| `Repositories/Configurations/.../LabResultCorrectionReasonConfiguration.cs` | **Baru.** Panjang kolom sesuai 17.1; nilai bawaan `false`/`true`/`0`; index unik **parsial** `ReasonCode WHERE "IsDelete" = false`; index `(IsActive, SortOrder)` |
| `Repositories/Configurations/.../LabFourEyesExceptionReasonConfiguration.cs` | **Baru.** Sama |
| `Areas/.../Models/LabExamination.cs` | **+14 properti nullable** dan dua navigasi ke `LabFourEyesExceptionReason`. Dua komentar lama yang menyatakan *"nol ValidatedAt maupun ReleasedAt"* diperbarui (butir 7) |
| `Repositories/Configurations/.../LabExaminationConfiguration.cs` | Panjang 200 untuk empat kolom snapshot; dua FK **`Restrict`**; index `(FinalizedAt, ValidatedAt)` dan `(ReleasedAt)` |
| `Repositories/ApplicationDbContext.cs` | Dua `DbSet` |
| `Areas/.../Enums/LaboratoryEnums.cs` | **Lima enum baru** — `LabPrivilegeKind`, `LabPrivilegeDenial` (8 nilai), `LabResultStatus` (5), `LabOrderResultProgress` (2), `LabValidationQueueStage` (2). Nilai persis tabel 20.4, mulai 1 |
| `Areas/HealthServices/MedicalRecordManagement/Enums/ClinicalDocumentKind.cs` | **`LaboratoryResult = 15`** — hanya menambah nilai. `JenisYangDitegakkan` **tidak disentuh** (20.9) |
| `Migrations/20260929041709_AddLabResultValidationAndRelease.cs` + `.Designer.cs` | **Baru**, dibangkitkan `dotnet ef migrations add` |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Ditulis ulang EF — lihat *Catatan snapshot* di bawah |

**Satu pengaman yang tidak terlihat di DDL.** `IsActive` diberi `DEFAULT true` di skema sesuai
17.6, ditambah `ValueGeneratedNever()` di configuration. Tanpa itu, EF menganggap `false`
sebagai *"belum diisi"* dan tidak mengirimnya. Alasan yang sengaja dibuat nonaktif lalu akan
**tersimpan aktif** oleh default database, tanpa satu pun galat.

**Koreksi nilai enum — disetujui pemilik modul 2026-09-29.** Rancangan menetapkan
`LaboratoryResult = 14`, tetapi angka itu sudah diambil `HemodialysisSession` lewat commit
`89028993` (2026-09-22). Commit itu masuk ke branch `yoga` sesudah rancangan diaudit pada
`ddeb5ed8`. Mengikuti rancangan apa adanya akan membuat **setiap hasil laboratorium yang dirilis
terbaca sebagai sesi hemodialisa** di rekam medis — dan jenis itu termasuk yang ditegakkan
penguncian dokumen. Pemilik modul memilih **15**. Angka itu dikoreksi pada lima tempat: rancangan
20.3 dan 20.4, kontrak integrasi `INT-08`, serta kamus data 17.4 dan 18. Kesepakatan
`LAB-COORD-002` menyangkut **adanya satu nilai**, bukan angkanya.

**Catatan snapshot — diff besar, isinya netral.** Diff `ApplicationDbContextModelSnapshot.cs`
tercatat +737/−956 baris, dan menyebut entity Finance serta Farmasi. Sebabnya dibuktikan,
tidak diduga:

| Pemeriksaan | Hasil |
| --- | --- |
| `has-pending-model-changes` **sebelum** menyunting apa pun | *"No changes have been made to the model since the last migration"* — snapshot lama sudah setara model |
| Jumlah blok `modelBuilder.Entity` per entity, lama vs baru | Hanya dua perubahan: +1 masing-masing untuk kedua tabel alasan, dan **−1 blok ganda** pada empat entity Farmasi (`PhmMedicationAdministration`, `…Revision`, `…Setting`, `PhmMedicationScheduleTime`) — sisa merge |
| Setiap baris yang "hilang" dari snapshot lama | 126 dari 127 baris unik **masih ada** di snapshot baru. Satu-satunya yang berbeda adalah baris pertama `// <auto-generated />`, yang kini diawali BOM |
| Isi `Up`/`Down` migration | **Hanya** perubahan Laboratorium — nol baris Finance maupun Farmasi |
| `has-pending-model-changes` **sesudah** migration | *"No changes have been made …"* |

Jadi snapshot lama memuat blok duplikat sisa merge. EF menulisnya ulang secara kanonik tanpa
mengubah arti model.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **`NOT APPLICABLE`.** Nol endpoint, nol DTO. Validator registri tetap **1564 kunci** |
| Database | **Satu migration, belum diterapkan.** `AddLabResultValidationAndRelease`: dua tabel baru, 14 kolom nullable **tanpa nilai bawaan** pada `LabExamination` (di PostgreSQL hanya mengubah metadata; nol baris ditulis ulang), dua FK `ON DELETE RESTRICT`, enam index. **Nol pengisian data**, nol seeder. **Status penerapan: belum diterapkan di lingkungan mana pun** |
| Keamanan/Auth | **`NOT APPLICABLE`** — nol izin baru. Kolom baru tidak memuat data sensitif. Kolom pengguna, jabatan, dan kewenangan sengaja **tanpa FK** (pola `ResultEnteredByUserId`; kewenangan milik Human Resource) |

---

## 4. Dokumentasi endpoint

**`NOT APPLICABLE`** — task ini tidak menambah maupun mengubah endpoint. Tindakan Validasi,
Rilis, dan *Kembalikan* lahir pada `BE-LAB-73`..`75`; layar kedua data induk alasan pada
`BE-LAB-71`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` (sesudah migration dan koreksi enum) | **0 error**, 230 warning, 53 detik. **Nol warning** dari berkas yang disentuh. Lima warning yang menyebut kata `ClinicalDocumentKind` milik `ClinicalNoteAddendumService.cs` dan sudah ada sebelumnya | `PASS` | Keluaran build |
| `dotnet ef migrations has-pending-model-changes` sebelum dan sesudah | *No changes* keduanya | `PASS` | Keluaran perintah |
| `dotnet ef migrations list --no-connect` | `20260929041709_AddLabResultValidationAndRelease` terakhir, sesudah `20260926110000_AddSourcePurchasingInvoiceIdToSupplierPayable` | `PASS` | Keluaran perintah |
| **Skrip SQL maju dibaca sebelum dijalankan** (`dotnet ef migrations script`, offline) | 14 `ADD` kolom **tanpa `DEFAULT`**; kedua tabel dengan `DEFAULT FALSE`/`TRUE`/`0`; **kedua index unik memuat `WHERE "IsDelete" = false`**; **kedua FK `ON DELETE RESTRICT`**; nol `UPDATE` atau `INSERT` data selain baris `__EFMigrationsHistory`; semuanya dalam satu `START TRANSACTION … COMMIT` | `PASS` | Skrip di scratchpad sesi |
| **Skrip SQL mundur dibaca** | Kebalikan persis: lepas dua FK, jatuhkan dua tabel, enam index, dan 14 kolom; hapus baris riwayat migration | `PASS` | Skrip di scratchpad sesi |
| Bentuk skema terhadap DDL 17.6 | **Sama**, dengan dua selisih teknis di bawah | `PASS` | Perbandingan skrip dengan 17.6 |
| `PermissionRegistryValidator` | Lolos, **1564 kunci** — sama dengan sebelum task ini | `PASS` | Harness validator |
| Regresi `BE-LAB-68` dan `BE-LAB-69` (EF InMemory, model memuat kedua entity baru) | 19/19 dan 31/31 | `PASS` | Harness |
| `LabExaminationStatus` | Tetap `Ordered`, `ChargeEligible`, `Voided`, `Cancelled`; diff `LaboratoryEnums.cs` hanya **menambah** baris | `PASS` | Kode |
| **Migration diterapkan ke basis data pengembangan, lalu `Down` diuji** | Tidak dijalankan | `NOT RUN` | Lihat *Tidak dijalankan* |
| Aplikasi start tanpa galat | Tidak dapat dibuktikan | `EXISTING / ENVIRONMENT ISSUE` | Startup berhenti di seeder Hemodialisa sejak `BE-LAB-67` |

**Dua selisih teknis dari DDL 17.6, keduanya tidak mengubah arti.**

| Selisih | Sebab |
| --- | --- |
| Dua index tambahan: `IX_LabExamination_ValidationExceptionReasonId` dan `IX_LabExamination_ReleaseExceptionReasonId` | EF membuat index pada setiap kolom FK. PostgreSQL tidak membuatnya sendiri, dan tanpanya pemeriksaan `RESTRICT` saat alasan dihapus memindai seluruh `LabExamination` |
| Nama FK dipangkas: `…ReleaseExceptionR~` dan `…ValidationExcepti~` | Nama pada DDL 69 dan 72 karakter, melebihi batas 63 karakter PostgreSQL. Npgsql memangkasnya secara deterministik |

Uji manual: **`NOT APPLICABLE`** — nol perilaku yang dapat dilihat pengguna.

**Tidak dijalankan:**

- **`dotnet ef database update` maupun `Down` terhadap database mana pun.** Satu-satunya database
  pengembangan dipakai bersama, dan eksekusi database adalah wewenang terpisah dari pembuatan
  migration (registry, entri 2026-09-02). Selain itu, database itu **belum menerapkan**
  `20260922044002_AddHemodialysisManagement`. Karena migration diterapkan berurutan, `database
  update` ke migration ini akan ikut menerapkan migration Hemodialisa milik modul lain. Tidak ada
  PostgreSQL lokal maupun Docker di mesin ini untuk uji terpisah.
- `tooling/migrations/Update-MigrationHistory.ps1` — menurut skrip itu sendiri opsional. Migration
  baru tetap di `Migrations/` dan ikut dikompilasi.
- Analyzer — build memakai `-p:RunAnalyzers=False`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Bentuk skema sama dengan DDL kamus data 17.6 | ✅ **Terpenuhi** pada skrip SQL | Dua selisih teknis di bagian 5 tidak mengubah arti |
| Seluruh baris `LabExamination` yang sudah ada bernilai **kosong** pada ke-14 kolom — nol pengisian data lama | ✅ **Terpenuhi** pada skrip | `ADD` tanpa `DEFAULT`; nol `UPDATE`. Belum diamati pada database |
| `LabExaminationStatus` **tetap empat nilai** (`LAB-DEC-080`) | ✅ **Terpenuhi** | Kode |
| DoD — migration berjalan maju dan mundur | **Belum terpenuhi** | Skrip keduanya terbaca benar, tetapi belum dijalankan (bagian 5) |
| DoD — nol endpoint baru | ✅ **Terpenuhi** | Registri tetap 1564 |
| DoD — nol perubahan perilaku | ✅ **Terpenuhi** | Regresi 19/19 dan 31/31; nol service disentuh |
| DoD — laporan `BE-LAB-70.md` | ✅ **Terpenuhi** | Berkas ini |
| Jebakan 1 — index unik **tanpa** pembatas `IsDelete` | ✅ **Dihindari** | Kedua index `WHERE "IsDelete" = false` |
| Jebakan 2 — menyentuh `JenisYangDitegakkan` milik Rekam Medis | ✅ **Dihindari** | Hanya satu nilai enum ditambahkan |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 230 warning compiler, **nol** dari berkas yang disentuh |
| Masalah yang diketahui | **1. Database pengembangan tertinggal migration.** Sekurang-kurangnya `AddHemodialysisManagement` belum diterapkan; itu pula yang menghentikan startup sejak `BE-LAB-67`. Selama itu, migration apa pun yang lahir sesudahnya — termasuk yang ini — tidak dapat diterapkan sendirian. **2. Snapshot lama memuat blok duplikat sisa merge** pada empat entity Farmasi. Migration ini menormalkannya secara netral, tetapi merge berikutnya dapat mengulanginya |
| Risiko tersisa | **Sedang pada penerapannya, rendah pada kodenya.** Penambahan index dan FK pada `LabExamination` mengambil kunci tabel singkat saat diterapkan; di data pengembangan tidak terasa, tetapi di produksi sebaiknya di luar jam sibuk. Jalur `Down` aman **hanya sebelum ada satu pun validasi** (20.7) — sesudahnya, cukup mundurkan kodenya |
| Perubahan sampingan | Snapshot migration ditulis ulang secara kanonik oleh EF (bagian 3.2). Di luar repository: harness dan skrip SQL di scratchpad sesi |
| Interupsi | `NONE` |
| Status Git | Berkas `BE-LAB-70`: `??` dua model, dua configuration, dua berkas migration, laporan ini; ` M` `LabExamination.cs`, `LabExaminationConfiguration.cs`, `ApplicationDbContext.cs`, `LaboratoryEnums.cs`, `ClinicalDocumentKind.cs`, `ApplicationDbContextModelSnapshot.cs`, `02-backend-architecture.md`, `contracts/integration-contract.md`, `erd/data-dictionary.md`, `roadmap/backend-roadmap.md`, `roadmap/traceability.md`. Perubahan `BE-LAB-68`/`69` yang belum ter-commit ikut ada. **Nol operasi Git dijalankan** |
| Langkah berikutnya | **1.** Pemilik database pengembangan menerapkan migration yang tertunda **berurutan** — Hemodialisa lebih dulu — lalu `AddLabResultValidationAndRelease`, dan menguji `Down` sebelum ada data validasi. **2.** `BE-LAB-71` (dua data induk alasan) dan `BE-LAB-72` (pembaca kewenangan) kini dapat dikerjakan pada kode; verifikasi runtime `BE-LAB-71` membutuhkan langkah 1. **3.** Beri tahu pemilik Rekam Medis bahwa nilai `15` kini dipakai Laboratorium |

---

## 8. Verifikasi runtime susulan — 2026-09-29 sore

**Instruksi eksplisit pemilik modul, 2026-09-29:** terapkan migration yang tertunda di database dev
bersama `QuilvianNewDevYoga`, **Hemodialisa lebih dulu, lalu migration task ini**.

**Yang ternyata tertunda lebih banyak dari dua.** `dotnet ef migrations list` menandai **14**
migration `(Pending)` dari sebelas modul. Selain Hemodialisa dan task ini, ada Gizi (dua), Bank
Darah, Farmasi, Keuangan (tiga), Akuntansi, Rawat Inap, Kas Kecil, Billing, dan Pembelian.
`dotnet ef database update` **tidak dipakai**, sebab perintah itu menerapkan seluruhnya dan
kedua belas lainnya di luar instruksi.

**Cara yang dipakai.** Satu skrip SQL per migration dibangkitkan secara offline dengan
`dotnet ef migrations script <sebelumnya> <migration>`, dibaca lebih dulu, lalu dijalankan apa adanya.
Setiap skrip membawa `START TRANSACTION … COMMIT` dan mencatat baris `__EFMigrationsHistory`-nya
sendiri, sehingga EF tetap mengenali keduanya sebagai sudah diterapkan. Connection string dibaca
dari `appsettings.Development.json`; kata sandinya tidak melewati baris perintah.

| Langkah | Hasil | Klasifikasi |
| --- | --- | --- |
| Preflight baca-saja | 22 tabel Hemodialisa belum ada; kesepuluh tabel rujukan FK-nya ada; kedua tabel alasan belum ada; `LabExamination` 43 kolom, 10 baris | `PASS` |
| `20260922044002_AddHemodialysisManagement` | Skrip dibaca: 22 `CREATE TABLE`, 116 index, satu baris riwayat — **nol** `ALTER`/`UPDATE`/`DROP` atas tabel yang sudah ada. Diterapkan dalam 678 ms; 22 tabel `Hmd*` berdiri | `PASS` |
| Migration task ini — **naik** | `LabExamination` 43 → **57** kolom; dua tabel alasan; empat index baru pada `LabExamination`; dua index unik **`WHERE ("IsDelete" = false)`** dan dua FK **`ON DELETE RESTRICT`** terbaca dari katalog PostgreSQL; satu baris riwayat | `PASS` |
| Kesepuluh baris `LabExamination` lama | **10 dari 10 kosong pada ke-14 kolom** — nol pengisian data lama | `PASS` |
| **Turun** (`Down`), dijalankan saat kedua tabel alasan **0 baris** | Kembali **43** kolom; kedua tabel, keempat index, kedua FK, dan baris riwayat hilang | `PASS` |
| **Naik lagi** | 57 kolom, dua tabel, empat index, satu baris riwayat | `PASS` |
| `migrations list` sesudahnya | Kedua migration tidak lagi `(Pending)`; **12 milik modul lain tetap tertunda**, tidak disentuh | `PASS` |
| Startup Development | **Seluruh seeder lolos**, termasuk `HemodialysisMasterDataSeeder`; *"Permission registry valid. 1572 identitas kanonik dari 1572 kemampuan terdaftar."*; `/health` `200` | `PASS` |

**Galat yang tercatat sesudah startup, dan bukan milik task ini.**
`AccAccountingEventSchedulerHostedService` menulis `42P01 relation "public.AccAccountingEvent" does
not exist` setiap siklus — tabelnya milik migration Akuntansi yang sengaja belum diterapkan. Penjadwal
Absensi menulis pelanggaran FK `HrdAttendanceProcessingRun` — masalah data Human Resource yang sudah ada.
**Nol galat** menyebut tabel Hemodialisa maupun Laboratorium. Aplikasi tetap melayani permintaan.

**Status baru: ✅ `SELESAI`.** Butir DoD *"migration berjalan maju dan mundur"* kini terpenuhi pada
database sungguhan. Menurut 20.7, `Down` aman **hanya sebelum ada satu pun validasi**. Sejak
kedua daftar alasan terisi (bagian 8 `BE-LAB-71.md`), yang dimundurkan cukup kodenya.

**Untuk pemilik modul lain:** dua belas migration di atas masih tertunda di `QuilvianNewDevYoga`.
Penjadwal Akuntansi menjadi gejala pertama yang terlihat.
