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
| Commit backend saat dikerjakan | `2713774b` (branch `rizkiG`; tree identik dengan `origin/QuilvianIntegrationBackend` `579f9f61`). Source, migration, dan laporan versi 🟡 di-commit Rizki sebagai **`7509e18c`**, sudah di `origin/rizkiG` |
| Tanggal | 28 September 2026 |
| Status | **✅ SELESAI — 28 September 2026.** 4 dari 4 acceptance terpenuhi. Migration `20260928041937_AddAccSubledgerBalance` dibuat dan diterapkan Rizki (`database update` → `Done.`); isinya diperiksa agent: `CreateTable` 1, `CreateIndex` 4, FK `Restrict` 4, `Down` hanya `DropTable`. Snapshot nol kehilangan blok (bagian 5.1). Build Rizki berhasil. UAT tidak relevan — nol endpoint. **Riwayat:** 🟡 pada hari yang sama, menunggu build dan migration |

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
| `IX_AccSubledgerBalance_LegalEntityId_AccountingPeriodId_ChartO~` — dipotong EF karena batas 63 karakter nama PostgreSQL | tiga kolom, berfilter `"IsDelete" = false` | Ya |
| `IX_AccSubledgerBalance_AccountingEventId` | `AccountingEventId` | Tidak |
| `IX_AccSubledgerBalance_AccountingPeriodId`, `IX_AccSubledgerBalance_ChartOfAccountId` | dibuat EF otomatis untuk FK yang bukan kolom terdepan index lain | Tidak |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
|---|---|
| Kontrak API | `NOT APPLICABLE` — nol endpoint. Jalur yang menulis tabel ini milik `BE-ACC-P2-028` |
| Database | **Satu tabel baru** `AccSubledgerBalance`. Migration `20260928041937_AddAccSubledgerBalance` **dibuat dan diterapkan Rizki** 28 September 2026 di `QuilvianNewDevRizki`. Tanpa henti layanan (tabel baru); mundur: hapus tabel |
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
| `dotnet build -p:RunAnalyzers=false` (Rizki) | `Build succeeded`. Tangkapan layar menunjukkan build inkremental sesudah `database update`; jumlah warning tidak tampil. Assembly berisi entity task ini juga terbukti tidak langsung: `migrations add --no-build` membangkitkan migration `AccSubledgerBalance` | `PASS` | Tangkapan layar Rizki, 28 September 2026 |
| `dotnet ef migrations add AddAccSubledgerBalance --no-build` (Rizki) | Migration `20260928041937_AddAccSubledgerBalance` terbentuk; `migrations list` menampilkannya satu-satunya `(Pending)` | `PASS` | Tangkapan layar Rizki; commit `7509e18c` |
| `dotnet ef database update --no-build` (Rizki) | `Applying migration '20260928041937_AddAccSubledgerBalance'.` → `Done.` | `PASS` | Tangkapan layar Rizki |
| Isi migration diperiksa agent terhadap harapan di bawah | `CreateTable` 1, `CreateIndex` 4 (satu unique berfilter `"IsDelete" = false`), empat FK `ReferentialAction.Restrict` ke `AccAccountingEvent`, `AccAccountingPeriod`, `AccChartOfAccount`, `MstLegalEntity`; kolom dan tipe sama dengan bagian 3.2; nol operasi tabel lain; `Down` hanya `DropTable` | `PASS` | `Migrations/20260928041937_AddAccSubledgerBalance.cs` |
| Snapshot diperiksa agent | Nol blok hilang — lihat bagian 5.1 | `PASS` | `git show 7509e18c -- Migrations/ApplicationDbContextModelSnapshot.cs` |
| Automated test | Tidak dijalankan — bukan acceptance (`ACC-DEC-081`) | `NOT RUN` | — |

### 5.1 Snapshot: kenapa −1026 baris tetapi nol kehilangan

Diff snapshot `7509e18c` tercatat **+701 / −1026** baris, dan jumlah `b.ToTable(` turun **749 → 746**.
Sekilas itu tampak seperti kejadian lama "snapshot kehilangan blok modul lain". Pemeriksaan
menunjukkan sebaliknya:

| Pemeriksaan | Hasil |
|---|---|
| Nama tabel unik sebelum → sesudah | **745 → 746**; satu-satunya nama baru `AccSubledgerBalance`, **nol** nama hilang |
| Blok ganda di snapshot sebelumnya | **Empat** blok Pharmacy tercatat dua kali: `PhmMedicationAdministration`, `PhmMedicationAdministrationRevision`, `PhmMedicationAdministrationSetting`, `PhmMedicationScheduleTime`. Ganda yang sama ada di tip integration `579f9f61`; asalnya commit `0ca1a1f3` (laporan `BE-RWI-123`..`126`), bukan Accounting |
| Himpunan baris unik sebelum dibanding sesudah | Baris yang hanya ada **sebelum**: satu — `// <auto-generated />`, kini ditulis EF dengan BOM di depannya. Baris yang hanya ada **sesudah**: 13, seluruhnya milik `AccSubledgerBalance` |

Jadi −1026 baris adalah **penghapusan empat blok ganda** ditambah **perpindahan urutan** blok Finance
(`FinPurchaseOrder`, `FinPettyCashBudget`, dan beberapa lainnya) akibat EF menulis ulang snapshot
dari model — bukan kehilangan definisi. Migration yang tidak memuat satu pun operasi selain
`AccSubledgerBalance` menguatkan hal ini: bila ada entity yang benar-benar hilang, EF akan
membangkitkan `DropTable`.

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
| (1) Unique `(LegalEntityId, AccountingPeriodId, ChartOfAccountId)` | Terpenuhi | Source: `HasIndex(...).IsUnique()` berfilter `"IsDelete" = false`. Migration: `CreateIndex` `unique: true`, `filter: "\"IsDelete\" = false"`; diterapkan |
| (2) Empat FK `Restrict` | Terpenuhi | Source: empat `OnDelete(DeleteBehavior.Restrict)`. Migration: empat `onDelete: ReferentialAction.Restrict` |
| (3) `Balance` boleh negatif | Terpenuhi | `numeric(18,2)` tanpa check constraint, di source maupun migration |
| (4) Snapshot tanpa deletion | Terpenuhi | Nol nama tabel dan nol baris definisi yang hilang; −1026 baris adalah empat blok ganda Pharmacy dan perpindahan urutan (bagian 5.1) |
| DoD: source berubah | Terpenuhi | Bagian 3.2; commit `7509e18c` |
| DoD: migration diterapkan | Terpenuhi | `database update` → `Done.` (Rizki, 28 September 2026) |
| DoD: laporan task tertulis | Terpenuhi | Berkas ini |
| Build owner 0 error | Terpenuhi | `Build succeeded`; jumlah warning tidak tampil pada build inkremental |

## 7. Catatan penutup

| Hal | Isi |
|---|---|
| Peringatan | Komentar XML `///` sempat tertulis pada model lalu dihapus sebelum laporan ini, mengikuti arahan tanpa baris `//` |
| Masalah yang diketahui | Class diagram `02-backend-architecture.md` 22.3 menulis `AsOfDate` sebagai `DateOnly`; source memakai `DateTime` + `date` (bagian 3.4). Perlu dirapikan saat amandemen dokumen berikutnya |
| Risiko tersisa | (a) Snapshot `7509e18c` menghapus empat blok ganda Pharmacy yang juga ada di integration. Saat PR `rizkiG` → integration, bagian snapshot itu tampil sebagai penghapusan di wilayah Pharmacy — **benar**, tetapi perlu dijelaskan di deskripsi PR supaya tidak dikira kerusakan, dan pemilik Pharmacy/lead perlu tahu asal gandanya (`0ca1a1f3`). Bila terjadi konflik snapshot, jangan pilih satu sisi utuh. (b) Tabel kosong sampai `BE-ACC-P2-028` berdiri. (c) Begitu `BE-ACC-P2-014` aktif, `ACC-DEC-076` menahan setiap penutupan yang akun kontrolnya belum punya baris di tabel ini (T6, `MODULE-STATUS.md`) |
| Perubahan sampingan | `NONE` dari agent. Snapshot hasil `migrations add` membersihkan empat blok ganda modul lain — efek alami EF, dipertahankan karena justru benar (bagian 5.1) |
| Interupsi | `NONE` |
| Status Git | Source, migration, snapshot, dan laporan versi 🟡 sudah di-commit Rizki `7509e18c` dan di-push. Sesudah itu hanya dokumen yang berubah: laporan ini, baris status roadmap dan traceability |
| Langkah berikutnya | `BE-ACC-P2-028` — jalur pesan saldo — atas perintah Rizki. Sesudah itu `BE-ACC-P2-014`, dengan risiko T6 ditimbang lebih dulu |

**Perintah yang dijalankan Rizki** (dari folder `NewQuilvianSystemBackend`; disimpan sebagai riwayat):

```bash
dotnet build ./QuilvianSystemBackend.csproj -p:RunAnalyzers=false
dotnet ef migrations add AddAccSubledgerBalance --no-build
git diff --stat -- Migrations/ApplicationDbContextModelSnapshot.cs
dotnet ef database update --no-build
dotnet ef migrations list --no-build
```
