# Laporan Perubahan Backend — `BE-LAB-19`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-19` |
| Judul | `LEGACY MIGRATION`: normalisasi dua tabel `Trx*` Laboratorium |
| Slice | `S2` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 3 |
| Trace | Instruksi pemilik modul 2026-09-03; `QBE-NAM-001`, `QBE-NAM-003`, `QBE-DB-001`, `QBE-DB-002`; catatan registry 2026-09-03 |
| Contract version | `NOT APPLICABLE` — kedua entity tidak pernah diekspos sebagai DTO, sehingga tidak ada kontrak API yang berubah |
| Dependency | `BE-LAB-16` — **`SELESAI`** |
| Klasifikasi | `HEAVY` — skor 10. Repository 0, berkas diperiksa 2, berkas diubah 2, logika bisnis 0, kontrak API 0, database 2, keamanan/auth 0, UI/workflow 0, ditambah dua tingkat karena `LEGACY MIGRATION` menyentuh tabel fisik dan migration dijalankan |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi, project test, registry, dan artefak blueprint |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `f103fff`, branch `yoga` |
| Tanggal | 2026-09-03 |
| Status | **`SELESAI`** — nol `Trx*` tersisa pada modul Laboratorium, tabel fisik ikut dinamai ulang, migration terbukti dua arah |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `LaboratoryManagement / Laboratory` |
| Pemilik dan prefix registry | Prefix `Lab`, lifecycle `ACTIVE` |
| Keberlakuan | **`LEGACY MIGRATION`** — kampanye terbatas yang dinyatakan eksplisit oleh pemilik modul, dicatat pada registry 2026-09-03 |
| QBE ID yang berlaku | `QBE-NAM-001`, `QBE-NAM-002`, `QBE-NAM-003`, `QBE-DB-001`, `QBE-DB-002`, `QBE-CFG-001`, `QBE-MOD-001`, `QBE-AUD-001` |
| QBE ID yang **tidak** berlaku | `QBE-API-001`, `QBE-DTO-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-SVC-001` — tidak ada endpoint, DTO, hak akses, maupun aturan bisnis yang berubah. `QBE-ENT-002`, `QBE-ENT-003` — tidak ada kolom yang berubah |
| Gerbang `BLOCKED — canonical governance unavailable` | Tidak aktif |

---

## 1. Masalah yang diperbaiki

Modul Laboratorium punya **dua nama untuk satu hal yang sama**. Sejak registry menaikkan
lifecycle `LaboratoryManagement` menjadi `ACTIVE` pada 2 September 2026, seluruh entity baru
memakai prefix `Lab` — `LabOrder`, `LabExamination`, `LabValueBound`, `LabValueOption`,
`LabValueBoundChangeRequest`, `LabValueBoundHistory`. Tetapi dua entity yang lahir lebih dulu
tertinggal dengan awalan `Trx`:

| Sebelum | Sesudah |
| --- | --- |
| `TrxLabSpecimen` | `LabSpecimen` |
| `TrxLabTransitionHistory` | `LabTransitionHistory` |

Akibatnya bukan sekadar tidak rapi:

> Seorang programmer membuka folder `Models` Laboratorium dan melihat sembilan berkas. Tujuh
> berawalan `Lab`, satu `Mst`, dua `Trx`. Tidak ada aturan yang dapat ia simpulkan dari situ.
> Ketika ia membuat entity berikutnya, ia harus menebak — dan tebakan yang salah menambah satu
> `Trx*` baru yang dilarang `QBE-NAM-001`.

Ada akibat yang lebih konkret. Kode yang sudah ada memanggil `_dbContext.TrxLabSpecimens`
sementara tetangganya memanggil `_dbContext.LabExaminations`, dan nama constraint di database
bercampur antara `FK_TrxLabSpecimen_...` dan `FK_LabExamination_...`. Setiap kali seseorang
membaca log kesalahan database, ia harus mengingat dua konvensi sekaligus untuk satu modul.

`MstLabRejectionReason` **tidak** termasuk masalah ini: ia data induk, dan `Mst` memang
prefixnya yang benar menurut registry.

---

## 2. Proses bisnis

`NOT APPLICABLE` dalam arti alur kerja rumah sakit. Task ini tidak mengubah satu pun perilaku
yang dilihat petugas: tidak ada endpoint yang berubah, tidak ada aturan validasi yang bergeser,
tidak ada status yang berpindah, dan tidak ada kolom yang bertambah maupun hilang. Yang berubah
hanya **nama**.

Yang berubah bagi programmer dan DBA:

| Aspek | Sebelum | Sesudah |
| --- | --- | --- |
| Class dan berkas | `TrxLabSpecimen.cs` | `LabSpecimen.cs` |
| Configuration | `TrxLabSpecimenConfiguration` | `LabSpecimenConfiguration` |
| DbSet | `_dbContext.TrxLabSpecimens` | `_dbContext.LabSpecimens` |
| Tabel | `public."TrxLabSpecimen"` | `public."LabSpecimen"` |
| Contoh constraint | `FK_TrxLabSpecimen_LabOrder_LabOrderId` | `FK_LabSpecimen_LabOrder_LabOrderId` |

Kontrak canonical menyatakan normalisasi `Trx*` **belum selesai** selama class, berkas,
configuration, DbSet, seluruh rujukan, dan tabel fisik belum dinormalkan bersama-sama
(`QBE-NAM-003`). Keenamnya dikerjakan dalam task ini.

---

## 3. Perubahan yang dikerjakan

### 3.1 Audit dependensi fisik — `QBE-DB-001`

Dilakukan **sebelum** rename, sebagaimana diwajibkan. Inventaris objek database yang namanya
memuat salah satu nama lama:

| Jenis | Jumlah | Contoh |
| --- | ---: | --- |
| Tabel | 2 | `TrxLabSpecimen`, `TrxLabTransitionHistory` |
| Primary key | 2 | `PK_TrxLabSpecimen` |
| Foreign key | 8 | `FK_TrxLabSpecimen_MstLabRejectionReason_RejectionReasonId`; `FK_LabExamination_TrxLabSpecimen_SpecimenId` — milik tabel **lain** yang menunjuk ke sini |
| Index | 9 | `IX_TrxLabSpecimen_LabOrderId_SpecimenSequence` |

Nama terpanjang yang terdampak adalah
`FK_TrxLabTransitionHistory_TrxPatientEncounter_EncounterId` — **58 huruf**, masih di bawah
batas 63 huruf Postgres. Karena kedua nama **memendek** tiga huruf setelah rename, tidak ada
nama baru yang terpotong dan tidak ada nama lama berakhiran `~` yang perlu dirapikan. Itulah
sebabnya migration ini tidak memerlukan langkah perapian seperti yang ada pada migration Rekam
Medis.

Satu nama memuat nama lama **dua kali**:
`FK_TrxLabSpecimen_TrxLabSpecimen_SupersededSpecimenId`, rantai pengambilan ulang yang menunjuk
tabelnya sendiri. Skrip berbasis `replace()` menangani keduanya sekaligus.

### 3.2 Berkas yang berubah

| Kelompok | Isi |
| --- | --- |
| Model | `TrxLabSpecimen.cs` → `LabSpecimen.cs`; `TrxLabTransitionHistory.cs` → `LabTransitionHistory.cs` |
| Configuration | `TrxLabSpecimenConfiguration.cs` → `LabSpecimenConfiguration.cs`; `TrxLabTransitionHistoryConfiguration.cs` → `LabTransitionHistoryConfiguration.cs`, beserta `ToTable` di dalamnya |
| `Repositories/ApplicationDbContext.cs` | `DbSet<LabSpecimen> LabSpecimens`; `DbSet<LabTransitionHistory> LabTransitionHistories` |
| Rujukan source | `LabOrder.cs`, `LabExamination.cs`, `LabValueBoundHistory.cs`, `LabSpecimenService.cs`, `LabExaminationService.cs`, `LabExaminationConfiguration.cs`, `LabValueBoundChangeRequestConfiguration.cs`, `LabValueOptionConfiguration.cs` |
| Uji | `LabExaminationTests.cs`, `LabExaminationEndpointTests.cs`, `LabOrderDisciplineTests.cs`, `BillingTestDatabaseFixture.cs`, `LaboratoryAuthorityTests.cs`, `LaboratorySpecimenLifecycleTests.cs` |
| Migration | `20260903094528_RenameLaboratoryTrxTablesToLabPrefix` beserta `.Designer.cs` |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Diperbarui EF mengikuti migration |
| Registry | `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` — catatan lifecycle 2026-09-03 |
| Blueprint | `erd/data-dictionary.md`, `erd/00-context-erd.md`, `erd/laboratory-operations.md`, `02-backend-architecture.md`, `contracts/permission-audit-matrix.md` |

Total 18 berkas source dan uji tersentuh rename, ditambah dua berkas yang berganti nama pada
masing-masing kelompok model dan configuration.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE`. Kedua entity tidak pernah menjadi kontrak transport — jawaban endpoint memakai `LabSpecimenResponse` dan `LabTransitionHistoryResponse`, nama DTO yang **sudah** berprefix `Lab` sejak awal dan tidak berubah. Tidak ada satu pun route, verb, ruas, atau nilai enum yang bergeser |
| Database | **Dua tabel, dua PK, delapan FK, dan sembilan index dinamai ulang.** Tidak ada kolom yang ditambah, dihapus, atau diubah tipenya. Migration **dibuat dan dijalankan**; rinciannya pada bagian 5.1 |
| Keamanan/Auth | `NOT APPLICABLE`. Nama resource pada `[AccessPermission]` sudah berbunyi `LabSpecimen` sejak sebelum task ini — string permissionnya tidak pernah memuat `Trx`. Tidak ada hak akses yang berubah, bertambah, maupun hilang |

### 3.4 Keputusan yang perlu diketahui

| No | Butir | Penjelasan |
| ---: | --- | --- |
| 1 | **EF menghasilkan DROP+CREATE; badan migrationnya diganti** | `dotnet ef migrations add` memperingatkan *"An operation was scaffolded that may result in the loss of data"* dan menghasilkan `DropTable` + `CreateTable`. Itu melanggar `QBE-DB-002`, yang melarang DROP+CREATE destruktif bila rename yang menjaga data masih aman. Badan `Up` dan `Down` diganti dengan skrip `ALTER ... RENAME` berbasis katalog Postgres. Berkas `.Designer.cs` beserta snapshot hasil generate **dipertahankan**, karena keduanya sudah benar menggambarkan model sesudah rename |
| 2 | Nama constraint dicari dari katalog, tidak diketik | Mengetik 19 nama satu per satu mengundang salah ketik yang baru ketahuan saat migration dijalankan. Skripnya membaca `pg_class`, `pg_constraint`, dan `pg_namespace`, lalu menamai ulang apa pun yang memuat nama lama — termasuk FK milik tabel lain yang menunjuk ke sini |
| 3 | Migration lama **tidak** disunting | Berkas migration yang sudah diterapkan adalah catatan sejarah. `20260824091610_AddLaboratorySpecimenLifecycle` tetap membuat tabel bernama `TrxLabSpecimen`, dan migration inilah yang menamainya ulang sesudahnya. Menyunting migration lama akan membuat riwayat basis data berbeda dari kenyataan |
| 4 | Dokumen blueprint **tidak** diganti seluruhnya | 131 sebutan tersebar di 21 dokumen. Yang diperbarui hanya lima dokumen yang menggambarkan **target**: ERD, kamus data, arsitektur backend, dan matriks kewenangan. Laporan task `BE-LAB-01` sampai `BE-LAB-16`, dokumen keputusan, capability map, dan permintaan approval **sengaja dibiarkan**: seluruhnya mengutip bukti pada commit tertentu, dan menulis ulangnya berarti memalsukan catatan tentang apa yang benar saat itu |
| 5 | `MstLabRejectionReason` dan `TrxPatientEncounter` tidak ikut | Yang pertama master data dengan prefix `Mst` yang sudah benar, dan catatan registry 2026-09-02 menetapkannya tidak dinamai ulang. Yang kedua milik modul Registration; menormalkannya batch tersendiri yang perlu koordinasi. Nama FK yang memuatnya karena itu tetap menyebut `TrxPatientEncounter` |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE`. Task ini tidak menambah, mengubah, maupun menghapus satu pun endpoint.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Audit katalog sebelum rename | 2 tabel, 2 PK, 8 FK, 9 index; nama terpanjang 58 huruf | `PASS` | Bagian 3.1 |
| `dotnet build QuilvianSystemBackend.sln` | Berhasil, `0 Error(s)` | `PASS` | Keluaran perintah |
| Nol `Trx*` tersisa pada modul Laboratorium | Pencarian `Trx` pada `Areas/.../LaboratoryManagement` dan configurationnya menghasilkan **kosong**, di luar `TrxPatientEncounter` yang milik modul lain | `PASS` | Keluaran `grep` |
| `Tests/QuilvianSystemBackend.Tests` | `Failed: 0, Passed: 184, Total: 184` | `PASS` | Keluaran perintah |
| `Tests/QuilvianSystemBackend.UnitTests.InMemory` | `Failed: 1, Passed: 889, Total: 890` | `EXISTING / ENVIRONMENT ISSUE` | Kegagalan `BillingFinalizationServiceTests`, terbuka sejak sebelum seluruh pekerjaan Laboratorium |
| Checker QBE atas 39 berkas modul Laboratorium | `VIOLATION: 0`, `REVIEW: 0`, `INFO: 0`, `Final result: PASS` | `PASS` | Keluaran perintah, mode `ExplicitFiles` — lihat bagian 7.1 |

### 5.1 Status migration dan database

| Langkah | Perintah | Hasil |
| ---: | --- | --- |
| 1 | `dotnet ef migrations add RenameLaboratoryTrxTablesToLabPrefix` | Menghasilkan scaffold **destruktif**; badannya diganti |
| 2 | `dotnet ef migrations list` | Tepat **satu** migration tertunda, yaitu milik task ini |
| 3 | `dotnet ef database update` | `Done.` Tabel dinamai ulang |
| 4 | `dotnet ef database update 20260903071535_AddLabExamination` | `Done.` Jalur `Down` mengembalikan nama lama |
| 5 | `dotnet ef database update` | `Done.` Diterapkan kembali; penanda `(Pending)` hilang |

| Aspek | Nilai |
| --- | --- |
| Database sasaran | `QuilvianNewDevYoga` — basis data pengembangan pemilik modul, **remote** pada `160.22.250.77` |
| Keamanan jalur `Down` | Seluruhnya `RENAME`; tidak ada `DROP`. Menjalankan `Down` mengembalikan nama lama tanpa menyentuh satu baris pun |
| Jumlah baris saat dijalankan | `0` pada `TrxLabSpecimen`, diverifikasi pemilik modul lewat `SELECT COUNT(*)` |
| Lingkungan lain | **Belum dijalankan.** Skripnya aman untuk lingkungan yang datanya tidak nol karena memakai `RENAME`, tetapi menjalankannya di luar dev pemilik tetap merupakan wewenang terpisah |
| Deployment | **Tidak dilakukan** |

Uji manual: `NOT APPLICABLE`. Tidak ada permukaan baru yang dapat ditembak.

**Tidak dijalankan:**

| Pemeriksaan | Alasan |
| --- | --- |
| Suite `QuilvianSystemBackend.IntegrationTests.Postgres` | Terhalang `QUILVIAN_BILLING_TEST_DB` yang belum diisi. Berkas ujinya ikut direname dan ikut terkompilasi, tetapi eksekusinya tetap terhalang |
| Suite `QuilvianSystemBackend.UnitTests.Sqlite` | Tidak menyentuh entity Laboratorium; terakhir dijalankan pada pemeriksaan pasca-merge dengan hasil `176 lulus, 0 gagal` |
| Kueri langsung ke `pg_indexes` sesudah rename | `AGENTS.md` melarang menjalankan perintah database sekadar untuk memvalidasi source. Bukti penerapan diambil dari `dotnet ef migrations list` yang menunjukkan penanda `(Pending)` hilang, dan dari jalur `Down` yang berhasil dibalik lalu diterapkan kembali |

---

## 6. Acceptance criteria dan Definition of Done

### 6.1 Acceptance criteria

`NOT APPLICABLE`. Task lahir dari instruksi langsung pemilik modul, bukan dari acceptance
criteria blueprint.

### 6.2 Definition of Done menurut roadmap

| Butir DoD | Status | Bukti |
| --- | --- | --- |
| Nol `Trx*` tersisa pada modul Laboratorium | **Terpenuhi** | Pencarian menghasilkan kosong di luar `TrxPatientEncounter` milik modul Registration |
| Tabel fisik ikut dinamai ulang | **Terpenuhi** | Migration diterapkan; bagian 5.1 langkah 3 |
| Migration jalan dua arah | **Terpenuhi** | Bagian 5.1 langkah 3, 4, dan 5 |
| Checker QBE lolos | **Terpenuhi** | `VIOLATION: 0` atas 39 berkas modul |
| Registry mencatat wewenangnya | **Terpenuhi** | Catatan lifecycle 2026-09-03 pada `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |

Tidak ada butir DoD yang belum terpenuhi.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `dotnet ef migrations add` memperingatkan *"may result in the loss of data"* untuk scaffold yang dihasilkannya. Peringatan itu benar untuk scaffold aslinya, dan justru itulah sebabnya badannya diganti — lihat bagian 3.4 butir 1 |
| Masalah yang diketahui | Lihat bagian 7.1 dan 7.2 |
| Risiko tersisa | **Rendah.** Tidak ada kolom, kontrak, maupun perilaku yang berubah. Risiko terbesarnya adalah lingkungan yang belum menjalankan migration ini: kode baru memanggil tabel `LabSpecimen` sementara basis datanya masih bernama `TrxLabSpecimen`. Selama migration belum dijalankan di sana, aplikasi akan gagal pada kueri Laboratorium mana pun |
| Perubahan sampingan | `Migrations/ApplicationDbContextModelSnapshot.cs` diperbarui EF sebagai bagian normal pembuatan migration |
| Interupsi | Checker QBE mode `WorkingTree` menggantung — lihat bagian 7.1 |
| Status Git | Tidak ada operasi Git yang dijalankan dari sesi ini |
| Langkah berikutnya | 1. **`BE-LAB-11`** — penahannya sudah dicabut; keenam kolom dipindahkan keluar dari `LabSpecimen`. 2. **`BE-LAB-12`** — endpoint wadah, yang menutup `AC-36` dan menghentikan tulis ganda salinan tarif. 3. Menjalankan migration ini ke lingkungan lain, dengan wewenang terpisah |

### 7.1 Temuan tooling — checker QBE mode `WorkingTree` menggantung

Setelah rename, `tooling/qbe/Invoke-QbeConformanceCheck.ps1` tanpa argumen **tidak pernah
selesai**; dua kali dijalankan, keduanya melewati 300 dan 600 detik tanpa menghasilkan satu baris
keluaran pun, dan prosesnya harus dihentikan.

Pemeriksaan yang dilakukan: tidak ada `.git/index.lock`, dan `git status --short` sendiri
menjawab dalam sekejap. Dugaan paling sesuai buktinya: skrip memanggil `git` lewat proses
terpisah dan menunggu proses itu selesai; ketika keluaran `git` cukup besar — dan setelah rename
ini daftar berkas berubahnya memang besar — pipa keluarannya penuh dan kedua pihak saling
menunggu.

**Ini bukan akibat kode task ini**, melainkan sifat skripnya. Sebagai jalan keluar, checker
dijalankan mode `ExplicitFiles` atas 39 berkas modul Laboratorium dan menghasilkan
`Final result: PASS`. Perbaikan skripnya milik pemilik tooling QBE.

### 7.2 Registry canonical perlu disinkronkan

Catatan lifecycle 2026-09-03 ditulis pada `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`
di repository ini. Menurut `GLOBAL_RULES.md`, sumber canonical registry adalah
`QuilvianEngineeringSkills/agents/rules/backend/engineering/`, dan salinan yang terpasang sebagai
plugin adalah hasil generate.

Task ini **tidak** menyentuh repository skill — wewenang lintas repository tidak diberikan.
Menyalin catatan ini ke canonical lalu menjalankan `tooling/sync-rules.ps1` menjadi langkah
pemilik suite Skill. Selama belum, `/plugin update` berikutnya akan menghadirkan registry tanpa
catatan ini.
