# BE-ACC-005 — Entity jurnal, baris jurnal, riwayat persetujuan, dan alokator nomor

- **TASK ID:** `BE-ACC-005`
- **TASK TYPE:** Implementasi backend, entity persisted
- **COMPLEXITY:** `MEDIUM` — empat entity saling berelasi, satu relasi `Cascade`, satu check constraint
- **MODEL:** Claude Opus 5
- **TASK MODE:** `BACKEND`
- **WRITE TARGET:** `NewQuilvianSystemBackend`
- **Tanggal:** 2 September 2026
- **Baseline:** `ACC-BP-001` revisi 6 `APPROVED`, roadmap revisi 2 `APPROVED`
- **HEAD saat mulai:** `a4df550` pada branch `rizkiG`, working tree bersih

## Validasi baseline

| Yang diperiksa | Tercatat | Nyata | Hasil |
|---|---|---|---|
| Blueprint revision | `6` | `6` | Cocok |
| 17 hash artefak canonical | manifest | dihitung ulang | **17/17 cocok** |
| `approved_backend_source_sha` | `aa837d7` | `aa837d7` | Cocok |
| `verification_backend_source_sha` | `e1ee173` | `a4df550` | **Berbeda — impact scan dijalankan** |
| Frontend | `5336c44` | `5336c44` | Cocok |

### Impact scan `e1ee173` → `a4df550`

`a4df550` adalah commit `feat(accounting): add accounting period entity (BE-ACC-004)`, tepat delapan berkas — persis yang disiapkan pada commit preparation `BE-ACC-004`.

| Pemeriksaan | Hasil |
|---|---|
| `Migrations/` tersentuh | **Tidak** |
| `ApplicationDbContextModelSnapshot.cs` tersentuh | **Tidak** |
| `Program.cs`, `tooling/`, `agents/`, `.github/`, `AGENTS.md` | **Tidak** |
| Modul lain | **Tidak** — 0 berkas |

**Dampak terhadap `BE-ACC-005`: nihil.** Dependency `BE-ACC-003` dan `BE-ACC-004` kini keduanya terlacak git. Task boleh berjalan.

## Backend Governance Preflight

| Field | Isi |
|---|---|
| Area | `Corporate` |
| Module | `AccountingManagement` |
| Submodule | `JournalManagement` |
| Pemilik / prefix registry | Rizki / **`Acc`** — terdaftar, lifecycle **`ACTIVE`** |
| Applicability | **`NEW CODE`** |
| QBE ID yang berlaku | `QBE-MOD-002` (terpenuhi), `QBE-MOD-003` (terpenuhi), `QBE-NAM-001` (tidak dilanggar — nol `Trx*` baru), `QBE-CFG-001` (terpenuhi — satu configuration per entity), `QBE-CODE-006` (dipenuhi lewat `AccNumberSeries` ber-scope; perilaku alokasinya `BE-ACC-010`) |
| `AGENTS.md` | Terbaca |
| Registry canonical | Terbaca dari `QuilvianEngineeringSkills/agents/rules/backend/engineering/` |

## 1. FILE YANG DIBUAT

Delapan berkas.

| Berkas | Baris |
|---|---:|
| `Areas/.../JournalManagement/Models/AccJournal.cs` | 122 |
| `Areas/.../JournalManagement/Models/AccJournalLine.cs` | 66 |
| `Areas/.../JournalManagement/Models/AccJournalApproval.cs` | 44 |
| `Areas/.../JournalManagement/Models/AccNumberSeries.cs` | 60 |
| `Repositories/Configurations/Corporate/AccountingManagement/JournalManagement/AccJournalConfiguration.cs` | 148 |
| `.../AccJournalLineConfiguration.cs` | 101 |
| `.../AccJournalApprovalConfiguration.cs` | 70 |
| `.../AccNumberSeriesConfiguration.cs` | 63 |

## 2. FILE YANG DIUBAH

| Berkas | Perubahan |
|---|---|
| `Repositories/ApplicationDbContext.cs` | +8 baris: 1 `using` + region baru berisi 4 `DbSet` |
| `Tests/.../AccountingFoundationTests.cs` | Guard diperbarui + 4 test baru |

Ditambah register: `roadmap/backend-roadmap.md`, `MODULE-STATUS.md`, `blueprint-manifest.md`.

## 3. ENTITY YANG DIBUAT

### `AccJournal` — aggregate root, 21 kolom

`LegalEntityId`, `JournalNumber(30)`, `JournalTypeId`, `AccountingPeriodId`, `DocumentNumber(50)?`, `DocumentDate?`, `AccountingDate`, `Description(500)`, `JournalStatus`, `TotalDebit`, `TotalCredit`, `SubmittedBy?`/`SubmittedAt?`, `ApprovedBy?`/`ApprovedAt?`, `PostedBy?`/`PostedAt?`, `RejectionReason(500)?`, `ReversalOfJournalId?`, `CorrectionType?`.

`TotalDebit` dan `TotalCredit` diberi komentar tegas sebagai **salinan**, bukan sumber kebenaran — keputusan pengajuan dan pengesahan selalu menghitung ulang dari baris.

### `AccJournalLine` — 8 kolom

`JournalId`, `LineNumber`, `AccountId`, `CostCenterId?`, `Description(500)?`, `DebitAmount`, `CreditAmount`.

Sengaja **tanpa** `LegalEntityId` (`ACC-DEC-037`) — badan hukumnya diturunkan dari akun yang ditunjuk.

### `AccJournalApproval` — 6 kolom

`JournalId`, `ApprovalAction`, `ActionBy`, `ActionAt`, `Reason(500)?`.

### `AccNumberSeries` — 6 kolom

`SequenceKey(50)`, `ScopeKey(50)`, `ResetPolicy(20)`, `CurrentValue`, `LastAllocatedAt`.

Meniru bentuk `BilNumberSeries` tetapi **tabel terpisah**: `ACC-DEC-004` melarang Accounting menulis tabel Billing, dan `QBE-CODE-006` menuntut alokator atomik ber-scope. Ditempatkan di `JournalManagement` karena satu-satunya konsumennya adalah penomoran jurnal.

### Tiga kolom yang sengaja TIDAK dibuat

| Kolom | Alasan |
|---|---|
| `SourceDomain`, `SourceTransactionId` | MVP tidak punya jurnal otomatis (`ACC-DEC-009`) |
| `CurrencyCode` | MVP hanya IDR (`ACC-DEC-020`); `CurrencyCode` wajib pada envelope kejadian Phase 2, bukan pada tabel jurnal |

Ketiadaannya dijaga test, sehingga penambahannya harus lewat keputusan sadar.

## 4. CONFIGURATION YANG DIBUAT

Empat berkas di `Repositories/Configurations/Corporate/AccountingManagement/JournalManagement/`.

### Check constraint

```
CK_AccJournalLine_TepatSatuSisiTerisi
  ("DebitAmount" > 0 AND "CreditAmount" = 0)
  OR ("DebitAmount" = 0 AND "CreditAmount" > 0)
```

Dipasang lewat `ToTable(..., table => table.HasCheckConstraint(...))`, mengikuti preseden terbaru `BilNumberSeriesConfiguration`. Ini lapis kedua; service tetap memeriksanya lebih dahulu supaya pesannya terbaca pengguna.

Kamus data menegaskan invariant ini **dapat** dijaga constraint, berbeda dari tiga invariant lain yang tidak bisa.

### `decimal(18,2)`

`HasPrecision(18, 2)` pada empat kolom nilai, mengikuti konvensi repository.

### Index

| Tabel | Index | Sifat |
|---|---|---|
| `AccJournal` | `(LegalEntityId, JournalNumber)` | **Unique**, filter `IsDelete` |
| `AccJournal` | `JournalTypeId`, `AccountingPeriodId`, `AccountingDate`, `JournalStatus`, `ReversalOfJournalId` | Penelusuran |
| `AccJournalLine` | `(JournalId, LineNumber)` | **Unique**, filter `IsDelete` |
| `AccJournalLine` | `AccountId`, `CostCenterId` | Buku besar dan laporan per cost center |
| `AccJournalApproval` | `JournalId`, `ActionBy` | Riwayat per jurnal dan per pelaku |
| `AccNumberSeries` | `(SequenceKey, ScopeKey)` | **Unique** |

## 5. RELASI ANTAR ENTITY

| Dari | Ke | Kardinalitas | Perilaku hapus |
|---|---|---|---|
| `AccJournal.LegalEntityId` | `MstLegalEntity` | N:1 | `Restrict` |
| `AccJournal.JournalTypeId` | `AccJournalType` | N:1 | `Restrict` |
| `AccJournal.AccountingPeriodId` | `AccAccountingPeriod` | N:1 | `Restrict` |
| `AccJournal.ReversalOfJournalId` | `AccJournal` | N:1 (diri sendiri) | `Restrict` |
| `AccJournalLine.JournalId` | `AccJournal` | N:1 | **`Cascade`** |
| `AccJournalLine.AccountId` | `AccChartOfAccount` | N:1 | `Restrict` |
| `AccJournalLine.CostCenterId` | `MstCostCenter` | N:1, **boleh kosong** | `Restrict` |
| `AccJournalApproval.JournalId` | `AccJournal` | N:1 | `Restrict` |
| `AccNumberSeries` | — | berdiri sendiri | — |

**Satu-satunya `Cascade` adalah baris jurnal.** Baris tidak punya makna tanpa jurnalnya, dan penghapusan jurnal hanya mungkin saat masih `Draft`. Riwayat persetujuan justru `Restrict` karena ia bukti audit — kalau tertukar, jejak persetujuan bisa lenyap tanpa ada yang menyadari. Perbedaan ini dijaga test tersendiri.

**Entity yang sudah ada dipakai ulang, bukan diduplikasi:** `AccChartOfAccount`, `AccJournalType`, `AccAccountingPeriod`, `MstLegalEntity`, `MstCostCenter`. Nol entity kembar dibuat.

**`MstLegalEntity` dan `MstCostCenter` tidak disentuh** — seluruh relasi ke keduanya memakai `.WithMany()` tanpa navigasi balik, pola yang sama sejak `BE-ACC-003`.

## 6. DBCONTEXT IMPACT

+8 baris, murni penambahan:

```
#region CORPORATE - ACCOUNTING MANAGEMENT - JOURNAL MANAGEMENT
public DbSet<AccJournal> AccJournals { get; set; }
public DbSet<AccJournalLine> AccJournalLines { get; set; }
public DbSet<AccJournalApproval> AccJournalApprovals { get; set; }
public DbSet<AccNumberSeries> AccNumberSeries { get; set; }
#endregion CORPORATE - ACCOUNTING MANAGEMENT - JOURNAL MANAGEMENT
```

Configuration ditemukan otomatis lewat `ApplyConfigurationsFromAssembly`. `ApplicationDbContextModelSnapshot.cs` **tidak disentuh**.

## 7. VALIDATION YANG DIBUAT

### Lapis struktural — dibuat pada task ini

| Aturan | Cara penegakan |
|---|---|
| Tepat satu sisi baris terisi | **Check constraint** `CK_AccJournalLine_TepatSatuSisiTerisi` |
| Nomor jurnal unik per badan hukum | Unique index `(LegalEntityId, JournalNumber)` |
| Nomor baris unik dalam satu jurnal | Unique index `(JournalId, LineNumber)` |
| Satu deret per `(SequenceKey, ScopeKey)` | Unique index |
| Ketepatan angka | `decimal(18,2)` pada empat kolom nilai |
| Cost center boleh kosong | FK nullable ke `MstCostCenter` |
| Riwayat tidak ikut terhapus | `Restrict` pada `AccJournalApproval` |

### Lapis aturan bisnis — **bukan** cakupan task ini

Kamus data secara eksplisit menyatakan tiga invariant berikut **tidak dapat** ditegakkan constraint:

| Invariant | Kenapa tidak bisa | Menunggu |
|---|---|---|
| Total debit = total kredit | Melibatkan seluruh baris satu jurnal | `BE-ACC-010` |
| Seluruh baris menunjuk akun milik badan hukum yang sama | Melibatkan tabel lain lewat dua tingkat relasi | `BE-ACC-010` |
| Penyetuju ≠ pembuat (`ACC-DEC-016`) | Membandingkan dua kolom yang diisi pada waktu berbeda | `BE-ACC-011` |

Ditambah: minimal dua baris, cost center wajib bila akun `Expense` (`ACC-DEC-019`), akun harus `IsPostable` dan aktif, alasan wajib saat menolak, dan pemeriksaan dua lapis status periode × jenis jurnal (`ACC-DEC-012`).

**Nol authorization mechanism, query filter, atau security policy dibuat.** `LegalEntityId` dipakai sesuai kontrak sebagai kolom data model; `ACC-DEP-008` tetap milik Security/Platform.

## 8. TEST YANG DIBUAT

14 → **18 test**.

| Test | Sifat | Membuktikan |
|---|---|---|
| `ModulAccounting_HanyaMemilikiEntityCakupanBeAcc005` | **Diperbarui** | Persis **tujuh** entity — seluruh entity `MVP-0`. Bertambahnya daftar ini berarti cakupan Phase 2 masuk terlalu dini |
| `SeluruhKolomNilai_MemakaiDecimal18Koma2` | **Baru** | Acceptance 1 — presisi dan skala keempat kolom nilai dibaca dari model EF |
| `JurnalDanAlokatorNomor_MemenuhiIndexDanRelasiKontrak` | **Baru** | Acceptance 2 dan 3 — tiga unique index, plus FK `MstCostCenter` ada dan **boleh kosong** |
| `PerilakuHapus_CascadeHanyaPadaBarisJurnal` | **Baru** | `Cascade` hanya pada baris; riwayat `Restrict`; seluruh FK jurnal `Restrict` termasuk pembalikan |
| `AccJournal_TidakMemilikiKolomYangSengajaDitunda` | **Baru** | `SourceDomain`, `SourceTransactionId`, `CurrencyCode` tidak ada — dan `AccJournalLine` tanpa `LegalEntityId` |

Test presisi desimal sengaja dibuat terpisah: salah presisi pada kolom uang tidak membuat build gagal, dan baru terlihat sebagai selisih rupiah berbulan-bulan kemudian. `NFR-008` menandainya sebagai risiko langsung.

Seluruh test model memakai `TestDatabase` (SQLite di memori, `EnsureCreated`) — **tidak pernah** menyentuh database bersama.

## 9. BUILD RESULT

```
dotnet build QuilvianSystemBackend.sln
Build succeeded.
    199 Warning(s)
    0 Error(s)
```

**0 error.** 199 warning, **sama persis** dengan baseline `BE-ACC-001`, `BE-ACC-003`, dan `BE-ACC-004`. Pemeriksaan terarah: **nol warning dari berkas Accounting**.

## 10. TEST RESULT

| Perintah | Hasil |
|---|---|
| filter `AccountingManagement` | **18 lulus**, 0 gagal, 26 detik |
| `Tests/QuilvianSystemBackend.Tests` | **150 lulus**, 0 gagal, 2 m 9 s |
| `QuilvianSystemBackend.Tests` (akar) | **815 lulus**, 0 gagal, 25 detik |

**965 test lulus, nol gagal, nol regresi.** Naik dari 961 pada `BE-ACC-004`.

Bukti tambahan yang berharga: `EnsureCreated()` berhasil membentuk keempat tabel beserta check constraint dan seluruh relasinya. Artinya bentuk DDL-nya sah, bukan sekadar lolos kompilasi.

## 11. MIGRATION STATUS

**Nol.** `Migrations/` tidak berubah dan tidak bertambah; `dotnet ef migrations add` dan `dotnet ef database update` **tidak** dijalankan; shared database **tidak** disentuh.

## 12. SNAPSHOT STATUS

`Migrations/ApplicationDbContextModelSnapshot.cs` **tidak berubah** — 0 berkas.

Model EF Core kini memuat **tujuh** entity yang belum ada di snapshot. `BE-ACC-006` akan menghasilkan **tujuh** `CreateTable`, persis seperti yang diperkirakan blueprint revisi 4 saat `AccNumberSeries` ditambahkan.

## 13. ACCEPTANCE CRITERIA `BE-ACC-005`

| # | Kriteria | Hasil | Bukti |
|---|---|:---:|---|
| 1 | `decimal(18,2)` untuk seluruh kolom nilai | ✅ | `SeluruhKolomNilai_MemakaiDecimal18Koma2` — presisi 18 skala 2 pada `TotalDebit`, `TotalCredit`, `DebitAmount`, `CreditAmount` |
| 2 | Unique index `(LegalEntityId, JournalNumber)`, `(JournalId, LineNumber)`, `(SequenceKey, ScopeKey)` | ✅ | `JurnalDanAlokatorNomor_MemenuhiIndexDanRelasiKontrak` — ketiganya diperiksa dari model EF |
| 3 | FK ke `MstCostCenter` ada dan boleh kosong | ✅ | Test yang sama — `IsRequired == false`, `DeleteBehavior.Restrict` |
| 4 | Build lulus | ✅ | 0 error, nol warning baru |

Ketentuan tambahan dari cakupan roadmap:

| Ketentuan | Hasil |
|---|:---:|
| `JournalId` pada baris memakai `Cascade`, sisanya `Restrict` | ✅ |
| Check constraint "tepat satu sisi terisi" | ✅ |
| Tanpa `SourceDomain`, `SourceTransactionId`, `CurrencyCode` | ✅ |
| Empat configuration, empat `DbSet` | ✅ |

## 14. DEFINITION OF DONE

| Butir | Hasil |
|---|:---:|
| Build lulus | ✅ 0 error |
| Tanpa migration | ✅ 0 berkas, snapshot tidak berubah |

## 15. KNOWN ISSUES / DEFERRED

### A. Dua pertentangan antar artefak canonical — **SELESAI 2 September 2026**

> **Terselesaikan.** Owner memutuskan keduanya pada hari yang sama lewat **`ACC-DEC-039`** dan
> **`ACC-DEC-040`**, dan keduanya **menguatkan pilihan yang sudah diimplementasikan** di sini.
> **Nol berkas kode berubah.** `erd/data-dictionary.md` diperbaiki agar DDL contohnya tidak lagi
> bertentangan dengan diagram ERD-nya sendiri. Uraian di bawah dipertahankan sebagai catatan
> bagaimana pertentangan itu ditemukan.

Ditemukan saat membaca kontrak. **Tidak** diputuskan sepihak; dicatat di sini.

**A1. Nama entity riwayat.** Instruksi task menyebut `AccJournalApprovalHistory`. Seluruh artefak canonical — `erd/02-journal.md`, `erd/00-context-erd.md`, `erd/data-dictionary.md` bagian 6 beserta DDL-nya, dan `roadmap/backend-roadmap.md` — menyebut **`AccJournalApproval`**.

Saya memakai **`AccJournalApproval`**, sesuai aturan bahwa artefak canonical adalah sumber kebenaran handoff. Bila yang dikehendaki `AccJournalApprovalHistory`, itu rename di satu berkas entity, satu configuration, satu `DbSet`, dan lima artefak blueprint — murah selama belum ada migration.

**A2. Tipe kolom tanggal: ERD vs DDL.** Keduanya di dalam artefak canonical dan saling bertentangan:

| Kolom | `erd/*.md` diagram | `erd/data-dictionary.md` DDL | Yang saya pakai |
|---|---|---|---|
| `AccJournal.AccountingDate` | `date` | `timestamp` | **`date`** |
| `AccJournal.DocumentDate` | `date` | `timestamp` | **`date`** |
| `AccJournal.SubmittedAt`/`ApprovedAt`/`PostedAt` | `timestamp` | `timestamp` | `timestamp with time zone` |
| `AccJournalApproval.ActionAt` | `timestamp` | `timestamp` | `timestamp with time zone` |

Alasan pilihan saya:

- **Tanggal akuntansi adalah tanggal, bukan saat.** Ia menentukan periode dan dipakai laporan; menyimpan jam padanya membuka celah beda hari akibat zona waktu.
- **Konsisten dengan `BE-ACC-004` yang sudah di-commit**, yang memakai `date` untuk `StartDate`/`EndDate` dan `timestamp with time zone` untuk `ClosedAt`/`ReopenedAt` — pertentangan ERD-vs-DDL yang sama sudah muncul di sana.
- **Konvensi repository**: `MstCostCenter.EffectiveStartDate` memakai `date`; seluruh kolom waktu `IdentityModel` memakai `timestamp with time zone`.

**Ini keputusan yang masih dapat dibatalkan tanpa biaya** selama `BE-ACC-006` belum jalan. Setelah migration terbit, mengubahnya menjadi perubahan tipe kolom pada tabel berisi data.

### B. Deferred — kebutuhan milik task berikutnya, sengaja tidak dikerjakan

| Kebutuhan | Milik |
|---|---|
| Perilaku alokasi nomor: `pg_advisory_xact_lock` di dalam transaction, reset `MONTHLY` | `BE-ACC-010` |
| Penegakan keseimbangan debit-kredit, minimal dua baris, akun satu badan hukum | `BE-ACC-010` |
| Penyetuju ≠ pembuat, alasan wajib saat menolak | `BE-ACC-011` |
| Cost center wajib bila akun `Expense` | `BE-ACC-010` |
| Pemeriksaan dua lapis status periode × jenis jurnal | `BE-ACC-011` |
| Controller, DTO, service | `BE-ACC-007` ke atas |
| Migration tujuh `CreateTable` | `BE-ACC-006` |

Nol di antaranya dikerjakan pada task ini. Nol controller, service, dan DTO Accounting yang ada — diverifikasi lewat pemeriksaan filesystem.

### C. Berlanjut dari task sebelumnya

1. **Model EF mendahului snapshot** — tujuh entity. Disengaja; `BE-ACC-006`.
2. **`ACC-DEP-008` terbuka** — menahan `BE-ACC-007` ke atas. Milik Security/Platform.
3. **Gerbang CI QBE mati** (`ACC-DEP-007`) — kesesuaian diverifikasi manual terhadap registry suite. Milik lead.
4. **Utang teknis pre-existing** — sisa folder `agents/rules/`, dua project bernama `QuilvianSystemBackend.Tests`. Tidak disentuh.

## API CONTRACT IMPACT

`NONE`. Nol controller, endpoint, DTO. Nol integrasi Finance, Billing, atau AR/AP. Nol posting otomatis. Nol workflow engine.

## SECURITY IMPACT

`NONE` sebagai perubahan. Nol authorization mechanism, query filter, atau security policy.

## MANUAL TEST

`NOT APPLICABLE` — tabelnya belum berdiri di database mana pun.

## INCIDENTAL CHANGES

`NONE`.

## GIT STATUS

```
 M Repositories/ApplicationDbContext.cs
 M Tests/QuilvianSystemBackend.Tests/AccountingManagement/AccountingFoundationTests.cs
?? Areas/Corporate/AccountingManagement/JournalManagement/Models/
?? Repositories/Configurations/Corporate/AccountingManagement/JournalManagement/
```

Tidak ada stage, commit, push, pull, merge, rebase, maupun deploy.

## NEXT RECOMMENDED STEP

**Putuskan dulu dua pertentangan pada bagian 15.A** — keduanya murah sekarang dan mahal setelah `BE-ACC-006`.

Sesudah itu `BE-ACC-006` — migration pertama dan data master awal. Ia **tidak** otomatis boleh jalan: tertahan Migration Coordination Gate, `ACC-DEP-005`, dan wewenang migration yang terpisah dari wewenang source.

`MVP-0` di sisi entity sudah **tuntas** — tujuh entity berdiri, nol migration.

**Menunggu instruksi eksplisit owner.**
