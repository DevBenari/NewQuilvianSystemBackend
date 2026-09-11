# BE-ACC-004 — Entity periode akuntansi

- **TASK ID:** `BE-ACC-004` — Entity periode akuntansi
- **TASK TYPE:** Implementasi backend, entity persisted
- **COMPLEXITY:** `LIGHT`
- **CLASSIFICATION SCORE:** rendah — satu entity, satu configuration, satu `DbSet`, tanpa endpoint dan tanpa migration
- **MODEL:** Claude Opus 5
- **TASK MODE:** `BACKEND`
- **WRITE TARGET:** `NewQuilvianSystemBackend`
- **Tanggal:** 2 September 2026
- **Baseline:** `ACC-BP-001` revisi 6 `APPROVED`, roadmap revisi 2 `APPROVED`
- **HEAD saat mulai:** `e1ee173` pada branch `rizkiG`

## Validasi baseline sebelum mulai

| Yang diperiksa | Tercatat | Nyata | Hasil |
|---|---|---|---|
| Blueprint revision | `6` | `6` | Cocok |
| 17 hash artefak canonical | manifest | dihitung ulang | **17/17 cocok** |
| `approved_backend_source_sha` | `aa837d7` | `aa837d7` | Cocok, tidak digeser |
| `verification_backend_source_sha` | `ca6b7e0` | `e1ee173` | **Berbeda — impact scan dijalankan** |
| `verification_frontend_source_sha` | `5336c44` | `5336c44` | Cocok |

### Impact scan atas pergeseran `ca6b7e0` → `e1ee173`

Dijalankan lebih dahulu sesuai aturan, sebelum satu langkah pun dikerjakan.

`e1ee173` adalah satu commit tunggal, *feat(accounting): add accounting foundation enums and tests*, berisi **pekerjaan `BE-ACC-001`, `BE-ACC-002`, dan `BE-ACC-003` yang di-commit owner**. Tidak ada perubahan dari pihak lain.

| Pemeriksaan | Hasil |
|---|---|
| `Migrations/` tersentuh | **Tidak** — 0 berkas |
| `ApplicationDbContextModelSnapshot.cs` tersentuh | **Tidak** |
| `Program.cs`, `tooling/`, `agents/`, `.github/`, `AGENTS.md` tersentuh | **Tidak** |
| Berkas modul lain tersentuh | **Tidak** — 0 berkas di luar Accounting dan `ApplicationDbContext.cs` |

**Dampak terhadap `BE-ACC-004`: nihil.** Pergeserannya justru menguntungkan — dependency `BE-ACC-001` dan `BE-ACC-003` kini bukan sekadar ada di working tree, melainkan sudah terlacak git. Task boleh berjalan.

## Backend Governance Preflight

| Field | Isi |
|---|---|
| Area | `Corporate` |
| Module | `AccountingManagement` |
| Submodule | `AccountingPeriod` |
| Pemilik / prefix registry | Rizki / **`Acc`** — terdaftar, lifecycle **`ACTIVE`** |
| Applicability | **`NEW CODE`** |
| QBE ID yang berlaku | `QBE-MOD-002` (terpenuhi — prefix `ACTIVE`), `QBE-MOD-003` (terpenuhi — Area/Module/Submodule terdaftar sebelum model dibuat), `QBE-NAM-001` (tidak dilanggar — nol `Trx*` baru), `QBE-CFG-001` (terpenuhi — configuration terpisah) |
| `AGENTS.md` | Terbaca |
| Registry canonical | Terbaca dari `QuilvianEngineeringSkills/agents/rules/backend/engineering/` |

## FILES INSPECTED

`docs/module-blueprints/accounting/erd/03-accounting-period.md`; `erd/data-dictionary.md` bagian 3; `contracts/state-transition-matrix.md` bagian periode; `contracts/validation-matrix.md` bagian 6; `roadmap/backend-roadmap.md` bagian `BE-ACC-004`; `blueprint-manifest.md`; `Areas/Corporate/AccountingManagement/AccountingPeriod/Enums/AccountingPeriodStatus.cs`; `Areas/Corporate/AccountingManagement/MasterData/ChartOfAccount/Models/AccChartOfAccount.cs`; `Repositories/Configurations/Corporate/AccountingManagement/MasterData/AccChartOfAccountConfiguration.cs`; `Repositories/ApplicationDbContext.cs`.

## 1. FILES CHANGED

Dua berkas dibuat, dua berkas diubah.

| Berkas | Jenis | Baris |
|---|---|---:|
| `Areas/Corporate/AccountingManagement/AccountingPeriod/Models/AccAccountingPeriod.cs` | **Baru** — entity | 84 |
| `Repositories/Configurations/Corporate/AccountingManagement/AccountingPeriod/AccAccountingPeriodConfiguration.cs` | **Baru** — configuration | 100 |
| `Repositories/ApplicationDbContext.cs` | **Diubah** — 1 `using` + 1 `DbSet` dalam region baru | +5 |
| `Tests/QuilvianSystemBackend.Tests/AccountingManagement/AccountingFoundationTests.cs` | **Diubah** — 1 guard diperbarui, 2 test baru | +85 |

## 2. ENTITY YANG DIBUAT

**`AccAccountingPeriod`** — tabel `public.AccAccountingPeriod`. Tiga belas kolom, persis kamus data bagian 3.

| Kolom | Tipe | Wajib | Bawaan | Index |
|---|---|:---:|---|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK |
| `LegalEntityId` | `Guid` | Ya | — | Unique bersama `PeriodCode` |
| `PeriodCode` | `string(7)` | Ya | — | Unique bersama `LegalEntityId` |
| `FiscalYear` | `int` | Ya | — | Index |
| `PeriodMonth` | `int` | Ya | — | — |
| `StartDate` | `date` | Ya | — | — |
| `EndDate` | `date` | Ya | — | — |
| `PeriodStatus` | `enum → int` | Ya | `1` (`Open`) | Index |
| `ClosedBy` | `Guid?` | Tidak | — | — |
| `ClosedAt` | `timestamptz?` | Tidak | — | — |
| `ReopenedBy` | `Guid?` | Tidak | — | — |
| `ReopenedAt` | `timestamptz?` | Tidak | — | — |
| `LastReasonNote` | `string(500)?` | Tidak | — | — |

Ditambah sepuluh kolom audit warisan `IdentityModel`.

Dua hal yang sengaja **tidak** dibuat:

| Yang tidak dibuat | Alasan |
|---|---|
| Navigasi ke `AccJournal` | `AccJournal` adalah cakupan `BE-ACC-005`. ERD menggambarkan relasi `AccAccountingPeriod ||--o{ AccJournal`, tetapi sisi jurnalnya belum ada, jadi relasinya dipasang nanti dari sisi `AccJournal` |
| Tabel riwayat periode | Keputusan sadar pada ERD: riwayat penutupan disimpan `LoggerService`, dan `LastReasonNote` hanya menyimpan alasan **terakhir**. Menambah tabel riwayat akan menduplikasi jejak audit platform |

## 3. CONFIGURATION YANG DIBUAT

`Repositories/Configurations/Corporate/AccountingManagement/AccountingPeriod/AccAccountingPeriodConfiguration.cs` — mengikuti cermin struktur `Areas/`, sama seperti seluruh configuration lain di repository ini.

### Relasi

| Relasi | Perilaku |
|---|---|
| `LegalEntityId` → `MstLegalEntity` | **`Restrict`** |

**`MstLegalEntity` tidak disentuh.** Memakai `.WithMany()` tanpa navigasi balik — pola yang sama persis dengan `AccChartOfAccount` pada `BE-ACC-003`, dan sejalan dengan ERD yang menyatakan `MstLegalEntity` **MUST NOT** disalin.

`Restrict` di sini bukan formalitas: periode adalah kerangka pembukuan. Kalau badan hukumnya terhapus dan periodenya ikut hilang, seluruh jurnal yang menunjuk periode itu kehilangan kerangkanya.

### Index

| Index | Sifat |
|---|---|
| `(LegalEntityId, PeriodCode)` | **Unique**, filter `"IsDelete" = false` |
| `FiscalYear` | Pembangkitan dan penelusuran satu tahun buku |
| `PeriodStatus` | Penyaringan periode terbuka/tertutup |

Ketiganya persis seperti yang ditulis kamus data — tidak ada index tambahan yang dikarang.

Filter `"IsDelete" = false` mengikuti konvensi repository, sama seperti `BE-ACC-003`.

### Tipe kolom

`StartDate` dan `EndDate` memakai `date`, bukan `timestamp`. Ini mengikuti ERD, dan memang benar: batas periode adalah tanggal, bukan saat tertentu dalam sehari. `ClosedAt` dan `ReopenedAt` sebaliknya memakai `timestamp with time zone`, karena keduanya mencatat **peristiwa**.

## 4. DBCONTEXT IMPACT

`Repositories/ApplicationDbContext.cs` bertambah **5 baris**: satu `using` dan satu region berisi satu `DbSet`.

```
#region CORPORATE - ACCOUNTING MANAGEMENT - ACCOUNTING PERIOD
public DbSet<AccAccountingPeriod> AccAccountingPeriods { get; set; }
#endregion CORPORATE - ACCOUNTING MANAGEMENT - ACCOUNTING PERIOD
```

Murni penambahan; tidak ada baris existing yang diubah atau dihapus. Configuration tidak perlu didaftarkan manual karena `OnModelCreating` sudah memakai `ApplyConfigurationsFromAssembly`.

`ApplicationDbContextModelSnapshot.cs` **tidak disentuh**.

## 5. VALIDATION

### Lapis struktural — dibuat pada task ini

| Aturan | Cara penegakan |
|---|---|
| Satu badan hukum satu periode per kode | Unique index `(LegalEntityId, PeriodCode)` |
| Kode periode tepat 7 karakter | `[MaxLength(7)]` + `.HasMaxLength(7)` |
| Kolom wajib | `[Required]` + `.IsRequired()` |
| Periode baru lahir terbuka | Nilai bawaan `AccountingPeriodStatus.Open` di entity dan di kolom |
| Badan hukum tidak terhapus selama masih punya periode | `DeleteBehavior.Restrict` |
| Jejak penutupan boleh kosong | Empat kolom `ClosedBy`/`ClosedAt`/`ReopenedBy`/`ReopenedAt` bertipe nullable |

### Lapis aturan bisnis — **bukan** cakupan task ini

Seluruh aturan pada `contracts/validation-matrix.md` bagian 6 dan state transition periode butuh perilaku, bukan struktur. Tempatnya di service pada `BE-ACC-009` (API periode). Ditulis di sini supaya tidak dikira terlewat:

| Aturan | Kode | Menunggu |
|---|---|---|
| Tahun buku belum pernah dibangkitkan | `409` | `BE-ACC-009` |
| Tahun buku antara 2000–2100 | `400` | `BE-ACC-009` |
| Alasan wajib saat buka kembali (`ACC-DEC-027`) | `400` | `BE-ACC-009` |
| Hanya periode tertutup yang boleh dibuka | `409` | `BE-ACC-009` |
| `Closed` → buka kembali menjadi **`SoftClosed`**, bukan `Open` (`ACC-DEC-028`) | `409` | `BE-ACC-009` |
| Periode tidak dapat dihapus | `409` | `BE-ACC-009` |
| Dua lapis pemeriksaan status periode × jenis jurnal (`ACC-DEC-012`) | `422` | `BE-ACC-011` |

Aturan terakhir perlu ditegaskan karena paling sering disalahpahami: pada `SoftClosed`, jurnal umum **ditolak** sementara jurnal penyesuaian dan pembalik **masih diterima**. Itu perilaku service, dan tidak dapat diwakili struktur tabel.

**Tidak ada authorization mechanism atau security filter baru** yang dibuat, sesuai instruksi. `LegalEntityId` ditambahkan hanya sebagai kolom data model karena memang bagian kontrak; penegakannya menunggu `ACC-DEP-008`.

## 6. TEST YANG DIBUAT

Berkas `AccountingFoundationTests.cs` naik dari 12 menjadi **14 test**.

| Test | Sifat | Membuktikan |
|---|---|---|
| `ModulAccounting_HanyaMemilikiEntityCakupanBeAcc004` | **Diperbarui** | Persis **tiga** entity yang boleh ada. Menggagalkan penambahan entity `BE-ACC-005` yang mendahului urutan task |
| `AccAccountingPeriod_SesuaiKamusData` | **Baru** | `PeriodCode` panjang 7, `LastReasonNote` panjang 500, kolom wajib, empat kolom jejak penutupan nullable, dan status bawaan `Open` |
| `AccAccountingPeriod_MenyimpanTigaStatusSebagaiInteger` | **Baru** | Nama tabel, `PeriodStatus` tersimpan sebagai `int`, tepat tiga nilai status `1/2/3`, unique index `(LegalEntityId, PeriodCode)`, seluruh FK `Restrict`, dan **belum adanya** navigasi ke jurnal |

Guard test diperbarui, bukan dihapus. Urutannya: `BE-ACC-001` menuntut nol entity → `BE-ACC-003` menuntut dua → `BE-ACC-004` menuntut tiga. Pagar urutan task tetap hidup.

Test ketiga memakai `TestDatabase` yang sudah baku (SQLite di memori, `EnsureCreated`), sehingga **tidak pernah** menyentuh database bersama, dan memeriksa model EF Core yang benar-benar terbentuk — bukan isi berkas configuration-nya.

## 7. BUILD RESULT

```
dotnet build QuilvianSystemBackend.sln
Build succeeded.
    199 Warning(s)
    0 Error(s)
```

**0 error.** 199 warning, **sama persis** dengan baseline `BE-ACC-001` dan `BE-ACC-003`. Pemeriksaan terarah memastikan **nol warning berasal dari berkas Accounting**.

## 8. TEST RESULT

| Perintah | Hasil |
|---|---|
| `dotnet test Tests/QuilvianSystemBackend.Tests --filter AccountingManagement` | **14 lulus**, 0 gagal, 19 detik |
| `dotnet test Tests/QuilvianSystemBackend.Tests` | **146 lulus**, 0 gagal, 2 m 8 s |
| `dotnet test QuilvianSystemBackend.Tests` (akar) | **815 lulus**, 0 gagal, 27 detik |

**961 test lulus, nol gagal, nol regresi.** Naik dari 959 pada `BE-ACC-003` karena dua test baru.

## 9. MIGRATION STATUS

**Tidak ada migration yang dibuat.**

| Pemeriksaan | Hasil |
|---|---|
| `Migrations/` berubah atau bertambah | **Tidak** — 0 berkas |
| `dotnet ef migrations add` dijalankan | **Tidak** |
| `dotnet ef database update` dijalankan | **Tidak** |
| Shared database disentuh | **Tidak** |

Migration adalah cakupan `BE-ACC-006`, dan tetap tertahan Migration Coordination Gate beserta `ACC-DEP-005`.

## 10. SNAPSHOT STATUS

`Migrations/ApplicationDbContextModelSnapshot.cs` **tidak berubah** — `git status` menghasilkan 0 berkas.

**Keadaan yang perlu diketahui:** model EF Core kini memuat **tiga** entity yang belum ada di snapshot — dua dari `BE-ACC-003`, satu dari task ini. Disengaja dan benar. `dotnet ef migrations add` pada `BE-ACC-006` akan menghasilkan `CreateTable` untuk ketiganya, dan itulah bahan pemeriksaan hitung-operasi yang diwajibkan `02-backend-architecture.md` bagian 8.

Selama `BE-ACC-006` belum dijalankan, build dan seluruh test tetap lulus karena tidak ada kode yang membaca ketiga tabel itu dari database.

## 11. ACCEPTANCE CRITERIA `BE-ACC-004`

| # | Kriteria | Hasil | Bukti |
|---|---|:---:|---|
| 1 | Tiga nilai status tersimpan sebagai integer | ✅ | `AccAccountingPeriod_MenyimpanTigaStatusSebagaiInteger` — `GetProviderClrType()` bernilai `int`, dan ketiga nilai `Open=1`, `SoftClosed=2`, `Closed=3` diperiksa satu per satu |
| 2 | `PeriodCode` panjang 7 | ✅ | `AccAccountingPeriod_SesuaiKamusData` — `MaxLength` bernilai 7, dan bentuk `2026-09` terbukti tepat 7 karakter |
| 3 | Build lulus | ✅ | 0 error, nol warning baru |

Ketentuan tambahan dari cakupan roadmap:

| Ketentuan | Hasil | Bukti |
|---|:---:|---|
| Unique index `(LegalEntityId, PeriodCode)` | ✅ | Diperiksa lewat model EF Core |
| Satu `DbSet` | ✅ | `AccAccountingPeriods` |
| Satu configuration | ✅ | `AccAccountingPeriodConfiguration` |

## 12. DEFINITION OF DONE

| Butir | Hasil |
|---|:---:|
| Build lulus | ✅ 0 error |
| Tanpa migration | ✅ 0 berkas migration, snapshot tidak berubah |

## SECURITY IMPACT

`NONE` sebagai perubahan. Tidak ada endpoint, atribut hak akses, jalur autentikasi, authorization mechanism, maupun security filter yang dibuat atau disentuh. `ACC-DEP-008` tetap terbuka dan tidak disentuh dari sini.

## API CONTRACT IMPACT

`NONE`. Tidak ada controller, endpoint, atau DTO yang dibuat.

## WARNINGS

199 warning, seluruhnya pre-existing pada modul lain dan identik dengan baseline sebelumnya: `MSB3277` di `BillingTests`, `xUnit2029`/`xUnit2031` di `InPatientManagement`, dan satu `CS8603` di `LaboratoryAuthorityTests`. Nol dari berkas Accounting, dan tidak ada yang diperbaiki karena di luar cakupan.

## KNOWN ISSUES

1. **Model EF Core mendahului snapshot** — tiga entity kini belum ada di snapshot. Disengaja; diselesaikan `BE-ACC-006`.
2. **`ACC-DEP-008` tetap terbuka** — kolom `LegalEntityId` ada, penegakannya belum. Menahan `BE-ACC-007` ke atas. Milik Security/Platform.
3. **Gerbang CI QBE masih mati** (`ACC-DEP-007`) — kesesuaian QBE diverifikasi manual terhadap registry canonical suite skill. Milik lead.
4. **Utang teknis pre-existing** — sisa folder `agents/rules/`, dan dua project bernama `QuilvianSystemBackend.Tests`. Tidak disentuh.

## MANUAL TEST

`NOT APPLICABLE` — tabelnya belum berdiri di database mana pun dan belum ada endpoint yang membacanya.

## INCIDENTAL CHANGES

`NONE`.

## INTERRUPTIONS

`NONE`.

## GIT STATUS

```
 M Repositories/ApplicationDbContext.cs
 M Tests/QuilvianSystemBackend.Tests/AccountingManagement/AccountingFoundationTests.cs
?? Areas/Corporate/AccountingManagement/AccountingPeriod/Models/
?? Repositories/Configurations/Corporate/AccountingManagement/AccountingPeriod/
```

Tidak ada stage, commit, push, pull, merge, rebase, maupun deploy.

## NEXT RECOMMENDED STEP

`BE-ACC-005` — entity jurnal, baris jurnal, dan riwayat persetujuan. Dependency-nya `BE-ACC-004` kini terpenuhi, dan ia juga tidak tertahan `ACC-DEP-008`. Ia menutup `MVP-0` di sisi entity, menyisakan `BE-ACC-006` yang punya gerbang migration tersendiri.

**Menunggu instruksi eksplisit owner.** Instruksi task ini secara tegas melarang melanjutkan ke `BE-ACC-005`.
