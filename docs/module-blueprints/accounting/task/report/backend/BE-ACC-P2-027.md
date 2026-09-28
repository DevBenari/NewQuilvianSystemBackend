# Laporan Perubahan Backend — `BE-ACC-P2-027`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-ACC-P2-027` |
| Judul | Entity dan migration saldo subledger |
| Slice | Wave D-1 — saldo subledger |
| Roadmap | [`roadmap/backend-roadmap-phase2.md`](../../../roadmap/backend-roadmap-phase2.md), kartu `BE-ACC-P2-027` (revisi 4, `APPROVED` 24 September 2026) |
| Trace | `ACC-DEC-071`, `076`, `087` (belum ada FR — coverage gap); sisi Finance `FIN-DEC-035` |
| Contract version | Kamus data bagian 12d (`approved` 24 September 2026); `02-backend-architecture.md` bagian 22.3, 22.6, 22.9, 22.10 (`approved`, `GATE-DESAIN-0924`) |
| Dependency | `BE-ACC-P2-019` ✅; `GATE-FIN-087` ✅ — dibuka 28 September 2026 atas `FIN-DEC-035` |
| Klasifikasi | `MEDIUM` — skor 4: berkas diperiksa 9–20 (1), berkas diubah 4–8 termasuk dokumen (1), database dampak entity/schema (2); repository, logika bisnis, API, keamanan, UI masing-masing 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — model, configuration, `DbSet`; laporan ini; baris status roadmap dan traceability. **Tidak** termasuk pembuatan maupun penerapan migration `AddAccSubledgerBalance` (dikerjakan Rizki), build, dan commit |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `2713774b` (branch `rizkiG`; tree identik dengan `origin/QuilvianIntegrationBackend` `579f9f61`), perubahan belum di-commit |
| Tanggal | 28 September 2026 |
| Status | **🟡 SEBAGIAN** — 28 September 2026. Acceptance (1)–(3) terpetakan ke source. **Belum:** build owner, dan acceptance (4) serta DoD "migration diterapkan" — keduanya menunggu migration `AddAccSubledgerBalance` yang dibuat dan diterapkan Rizki |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AccountingManagement` / `Reconciliation` — folder sudah berdiri (controller, DTO, service `BE-ACC-P2-013`); `Models/` dibuat bersama model pertamanya, seperti ditetapkan `02-backend-architecture.md` bagian 22.10 |
| Registry | `Acc` — `ACTIVE`, `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 39 (didaftarkan 8 September 2026). Submodule `Reconciliation` berada di bawah modul terdaftar yang sama, pola yang sama dengan `AccountingEvent` pada `BE-ACC-P2-019`. **Selisih yang dilaporkan:** salinan registry pada suite skill v1.17.1 **tidak** memuat baris `Acc`; registry repository yang berlaku (`ACC-TD-015`, `ACC-DEP-007` milik lead) |
| Keberlakuan | `NEW CODE` — satu entity, satu configuration; `ApplicationDbContext` hanya bertambah satu `using` dan satu region `DbSet` |
| QBE yang berlaku | `QBE-ENT-001` (mewarisi `IdentityModel`), `QBE-ENT-002`, `QBE-ENT-003` (nol kolom presentasi), `QBE-NAM-001`/`002` (prefix `Acc`, nama entity = tabel = configuration + `Configuration`, `DbSet` jamak), `QBE-MOD-001`/`002`/`003`, `QBE-CFG-001`, `QBE-AUD-001`. Tidak berlaku: `QBE-CODE-*` (tidak ada nomor bisnis), `QBE-SVC-001`/`QBE-API-001`/`QBE-PERM-001` (nol endpoint) |
| Governance yang dibaca | `AGENTS.md` backend; `rules/backend/` suite (`TASK_RULES`, `DATABASE_RULES`, `TASK_CLASSIFICATION`, `REVIEW_RULES`, `REPORT_TEMPLATE`); `BACKEND_ENGINEERING_CONTRACT.md`; registry suite dan repository |

## 1. Masalah yang diperbaiki

Finance akan mengirim **saldo subledger** per akun kontrol setiap akhir periode — misalnya "saldo
piutang penjamin per 30 November 2026 Rp 425.000.000" — supaya Accounting dapat mencocokkannya
dengan saldo akun kontrol di buku besar sebelum tutup bulan (`ACC-DEC-071`, `076`). Sampai task ini,
Accounting **tidak punya tempat** untuk menyimpan angka itu. Task ini menyediakan tempatnya dalam
bentuk entity dan configuration; tabelnya baru ada di database sesudah Rizki membuat dan
menerapkan migration.

## 2. Proses bisnis

1. Pada tutup periode, Finance mengirim pesan `SALDO-SUBLEDGER` ke kotak masuk kejadian
   Accounting (`POST api/v1/corporate/accounting/accounting-events`) dengan rincian periode dan
   kode akun kontrol (`ACC-DEC-087`, disepakati `FIN-DEC-035`).
2. Jalur pesan saldo (`BE-ACC-P2-028`, **belum dibangun**) menyimpan kejadiannya berstatus
   `Tercatat` tanpa jurnal, lalu **menulis atau mengganti satu baris** `AccSubledgerBalance` untuk
   pasangan badan hukum, periode, dan akun kontrol itu.
3. Koreksi dari Finance datang dengan `SourceVersion` lebih tinggi dan **mengganti** angka pada baris
   yang sama. Pesan lama yang datang terlambat tidak menimpa koreksi.
4. Penghalang rekonsiliasi (`BE-ACC-P2-014`, **belum dibangun**) membaca baris ini dan
   membandingkannya dengan saldo buku besar. Selisih sekecil apa pun, atau baris yang belum ada,
   menahan penutupan periode (`ACC-DEC-076`).

**Contoh.** Finance menyatakan saldo piutang penjamin November Rp 425.000.000 (versi 1), lalu
mengoreksinya menjadi Rp 424.500.000 (versi 2). Tabel ini berisi **satu** baris bernilai
Rp 424.500.000 yang menunjuk kejadian versi 2. Kedua kejadian tetap tersimpan di
`AccAccountingEvent` sebagai jejak.

Task ini hanya mengerjakan wadah pada langkah 2. Langkah 2 dan 4 sendiri milik `028` dan `014`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Untuk apa |
|---|---|
| `docs/module-blueprints/accounting/erd/data-dictionary.md` bagian 12d | Kolom, tipe, relasi, perilaku hapus |
| `docs/module-blueprints/accounting/02-backend-architecture.md` bagian 22.2, 22.3, 22.6, 22.9, 22.10 | Letak berkas, class diagram, index, status migration |
| `docs/module-blueprints/accounting/roadmap/backend-roadmap-phase2.md` kartu `027` | Cakupan dan acceptance |
| `Areas/Corporate/AccountingManagement/AccountingEvent/Models/AccAccountingEvent.cs`, `AccAccountingEventComponent.cs` | Pola entity terdekat (`BE-ACC-P2-019`) |
| `Repositories/Configurations/Corporate/AccountingManagement/AccountingEvent/AccAccountingEventConfiguration.cs`, `AccAccountingEventComponentConfiguration.cs` | Pola configuration terdekat, FK `Restrict` ke `MstLegalEntity` |
| Seluruh configuration `Repositories/Configurations/Corporate/AccountingManagement/**` (penelusuran `IsUnique`/`HasFilter`) | Konvensi unique index modul |
| `Repositories/ApplicationDbContext.cs` | Region `DbSet` Accounting, `ApplyConfigurationsFromAssembly` (baris 1107) |
| `Models/IdentityModel.cs` | Kolom audit bawaan |
| `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | Prefix `Acc` |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/AccountingManagement/Reconciliation/Models/AccSubledgerBalance.cs` | **Baru.** Entity mewarisi `IdentityModel`; delapan kolom kamus data 12d; empat navigasi |
| `Repositories/Configurations/Corporate/AccountingManagement/Reconciliation/AccSubledgerBalanceConfiguration.cs` | **Baru.** Tabel `public."AccSubledgerBalance"`, presisi `numeric(18,2)`, kolom `date`, kolom audit mengikuti pola modul, empat FK `Restrict`, satu unique index, satu index |
| `Repositories/ApplicationDbContext.cs` | `using ...Reconciliation.Models` dan region `CORPORATE - ACCOUNTING MANAGEMENT - RECONCILIATION` berisi `DbSet<AccSubledgerBalance> AccSubledgerBalances` — +5 baris, nol baris dihapus |

Nol perubahan pada `Program.cs`, controller, service, DTO, dan `Migrations/`.

**Bentuk tabel yang dihasilkan configuration:**

| Kolom | Tipe PostgreSQL | Wajib | Catatan |
|---|---|:---:|---|
| `Id` | `uuid` | Ya | PK |
| `LegalEntityId` | `uuid` | Ya | FK `MstLegalEntity`, `Restrict` |
| `AccountingPeriodId` | `uuid` | Ya | FK `AccAccountingPeriod`, `Restrict` |
| `ChartOfAccountId` | `uuid` | Ya | FK `AccChartOfAccount`, `Restrict` |
| `Balance` | `numeric(18,2)` | Ya | **Tanpa** check constraint — boleh nol atau negatif |
| `AsOfDate` | `date` | Ya | Tanggal cut-off |
| `SourceVersionNumber` | `integer` | Ya | — |
| `AccountingEventId` | `uuid` | Ya | FK `AccAccountingEvent`, `Restrict` |
| `CreateDateTime`, `UpdateDateTime`, `DeleteDateTime`, `CancelDateTime` | `timestamp with time zone` | `Create` saja | Bawaan `CURRENT_TIMESTAMP` untuk `CreateDateTime` |
| `CreateBy`, `UpdateBy`, `DeleteBy`, `CancelBy`, `IsDelete`, `IsCancel` | `uuid` / `boolean` | Ya | Dari `IdentityModel`; `IsDelete`/`IsCancel` bawaan `false` |

| Index | Kolom | Unique |
|---|---|:---:|
| `IX_AccSubledgerBalance_LegalEntityId_AccountingPeriodId_ChartOfAccountId` | tiga kolom, berfilter `"IsDelete" = false` | Ya |
| `IX_AccSubledgerBalance_AccountingEventId` | `AccountingEventId` | Tidak |
| `IX_AccSubledgerBalance_AccountingPeriodId`, `IX_AccSubledgerBalance_ChartOfAccountId` | dibuat EF otomatis untuk FK yang bukan kolom terdepan index lain | Tidak |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
|---|---|
| Kontrak API | `NOT APPLICABLE` — nol endpoint. Jalur yang menulis tabel ini milik `BE-ACC-P2-028` |
| Database | **Satu tabel baru** `AccSubledgerBalance`. Migration `AddAccSubledgerBalance` **belum dibuat** — dibuat dan diterapkan Rizki. Tanpa henti layanan (tabel baru); mundur: hapus tabel |
| Keamanan/Auth | `NOT APPLICABLE`. `Balance` bertanda sensitif di kamus data; nol logger menyentuhnya pada task ini |

### 3.4 Keputusan implementasi yang perlu diketahui

| Hal | Yang dipilih | Alasan |
|---|---|---|
| Tipe `AsOfDate` | `DateTime` + kolom `date` | Konvensi seluruh tanggal bisnis Accounting (`ACC-DEC-040`, `AccAccountingEvent.AccountingDate`). **Delta dokumen:** class diagram bagian 22.3 menulis `DateOnly`; kamus data 12d menulis `date` — source mengikuti kamus data dan konvensi |
| Filter unique index | `"IsDelete" = false` | Konvensi seluruh unique index kunci bisnis Accounting (`AccEventType`, `AccChartOfAccount`, `AccAccountingPeriod`, dan lainnya). Pengecualian tanpa filter hanya kunci anti-ganda kejadian dan run jurnal berulang. Kamus data 12d tidak menyebut filter; tabel ini tidak punya jalur hapus, sehingga filter tidak mengubah perilaku |
| FK `AccountingEventId` ke `AccAccountingEvent` | `Restrict`, `WithMany()` tanpa koleksi balik | Kamus data 12d. `AccAccountingEvent` tidak diubah |
| Komentar kode | Nol baris `//` | Arahan owner 22 September 2026; maksud kelas dicatat di laporan ini |

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak membuat atau mengubah endpoint.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
|---|---|---|---|
| `git status --short` dan `git diff` | Tiga berkas source: dua baru, `ApplicationDbContext.cs` +5/−0; `Migrations/` tidak tersentuh | `PASS` | Bagian 7 |
| `git ls-files --eol` | `ApplicationDbContext.cs` tetap `w/crlf` seluruhnya (bukan `mixed`); dua berkas baru `w/lf`, sama dengan berkas saudaranya | `PASS` | — |
| Penelusuran nama ganda `class AccSubledgerBalance` / `AccSubledgerBalances` | Tepat satu entity dan satu `DbSet` | `PASS` | — |
| Penelusuran `//` pada dua berkas baru | Nol | `PASS` | — |
| `dotnet build ./QuilvianSystemBackend.csproj -p:RunAnalyzers=false` | Belum dijalankan — build dilakukan Rizki sendiri | `NOT RUN` | — |
| `dotnet ef migrations add AddAccSubledgerBalance` | Belum dijalankan — wewenang Rizki | `NOT RUN` | — |
| Automated test | Tidak dijalankan — bukan acceptance (`ACC-DEC-081`) | `NOT RUN` | — |

**Isi migration yang diharapkan** — dipakai Rizki memeriksa hasil `migrations add`:

- `CreateTable` **tepat satu**, `AccSubledgerBalance`, dengan kolom seperti bagian 3.2.
- `CreateIndex` **empat**, seperti tabel index bagian 3.2; yang unique berfilter `"IsDelete" = false`.
- Empat FK `onDelete: ReferentialAction.Restrict` ke `MstLegalEntity`, `AccAccountingPeriod`, `AccChartOfAccount`, `AccAccountingEvent`.
- **Nol** `AddColumn`, `AlterColumn`, `DropTable`, `DropIndex`, atau `Sql` — bila ada operasi untuk tabel lain, itu perubahan model modul lain yang ikut dari integration dan **tidak** boleh ikut migration ini.
- Snapshot bertambah satu blok `b.ToTable("AccSubledgerBalance", "public")` beserta relasinya, **nol baris dihapus**.
- `Down` hanya `DropTable("AccSubledgerBalance")`.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
|---|---|---|
| (1) Unique `(LegalEntityId, AccountingPeriodId, ChartOfAccountId)` | Terpenuhi di source | `AccSubledgerBalanceConfiguration.cs`: `HasIndex(new { LegalEntityId, AccountingPeriodId, ChartOfAccountId }).IsUnique()` berfilter `"IsDelete" = false`. Berlaku di database sesudah migration |
| (2) Empat FK `Restrict` | Terpenuhi di source | Empat `HasOne(...).WithMany().HasForeignKey(...).OnDelete(DeleteBehavior.Restrict)` ke `MstLegalEntity`, `AccAccountingPeriod`, `AccChartOfAccount`, `AccAccountingEvent` |
| (3) `Balance` boleh negatif | Terpenuhi di source | `decimal` `HasPrecision(18, 2)` tanpa check constraint dan tanpa validasi tanda |
| (4) Snapshot tanpa deletion | **Belum terpenuhi** | Menunggu migration `AddAccSubledgerBalance` oleh Rizki |
| DoD: source berubah | Terpenuhi | Bagian 3.2 |
| DoD: migration diterapkan | **Belum terpenuhi** | Menunggu Rizki |
| DoD: laporan task tertulis | Terpenuhi | Berkas ini |
| Build owner 0 error | **Belum** | Menunggu Rizki |

## 7. Catatan penutup

| Hal | Isi |
|---|---|
| Peringatan | Komentar XML `///` sempat tertulis pada model lalu dihapus sebelum laporan ini, mengikuti arahan tanpa baris `//` |
| Masalah yang diketahui | Class diagram `02-backend-architecture.md` 22.3 menulis `AsOfDate` sebagai `DateOnly`; source memakai `DateTime` + `date` (bagian 3.4). Perlu dirapikan saat amandemen dokumen berikutnya |
| Risiko tersisa | (a) Merge integration → `rizkiG` membawa perubahan model modul lain; bila ada yang belum punya migration, `migrations add` akan ikut membangkitkannya — periksa isi migration terhadap bagian 5 sebelum `database update`. (b) Tabel kosong sampai `BE-ACC-P2-028` berdiri. (c) Begitu `BE-ACC-P2-014` aktif, `ACC-DEC-076` menahan setiap penutupan yang akun kontrolnya belum punya baris di tabel ini (T6, `MODULE-STATUS.md`) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M Repositories/ApplicationDbContext.cs`; `?? Areas/Corporate/AccountingManagement/Reconciliation/Models/`; `?? Repositories/Configurations/Corporate/AccountingManagement/Reconciliation/`; ditambah laporan ini dan baris status roadmap serta traceability |
| Langkah berikutnya | Rizki: build → `migrations add AddAccSubledgerBalance` → periksa isi terhadap bagian 5 → `database update`. Sesudah itu laporan ini dinaikkan ke ✅, lalu `BE-ACC-P2-028` atas perintah Rizki |

**Perintah untuk Rizki** (dari folder `NewQuilvianSystemBackend`):

```bash
dotnet build ./QuilvianSystemBackend.csproj -p:RunAnalyzers=false
dotnet ef migrations add AddAccSubledgerBalance --no-build
git diff --stat -- Migrations/ApplicationDbContextModelSnapshot.cs
dotnet ef database update --no-build
dotnet ef migrations list --no-build
```
