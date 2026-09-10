# Laporan Perubahan Backend — `BE-BKC-027`

## Metadata

| Field | Nilai |
| --- | --- |
| `TASK ID` | `BE-BKC-027` — Migration dua kolom penampung selisih dan kategori write-off |
| `TASK TYPE` | Perubahan entity + migration (murni additive, tanpa perutean logika bisnis) |
| `COMPLEXITY` | `MEDIUM` — perubahan entity/EF configuration (skor 1) plus pembuatan migration (skor 1); tidak ada mesin perhitungan uang yang disentuh, tidak ada endpoint baru |
| `CLASSIFICATION SCORE` | 2 |
| `MODEL` | Claude Sonnet 5 |
| `TASK MODE` | `BACKEND` |
| `WRITE TARGET` | `NewQuilvianSystemBackend`, branch `Yasmina` — `Areas/HealthServices/BillingManagement/Billing/Models/`, `Repositories/Configurations/HealthServices/BillingManagement/Billing/`, `Migrations/` |
| Gelombang | `MVP-11` bagian pertama (`EPIC BKC-09`) |
| Blueprint | `BIL-CASH-001` — kontrak dikunci 4 September 2026 |
| Kontrak berlaku | `BIL-API-0.7` — `approved` |
| Trace | `BKC-DEC-080`, `BKC-DEC-036`; `BKC-DES-024`, `BKC-DES-025` |
| Backend cabang saat mulai | `Yasmina`, working tree sudah memuat `BE-BKC-022`–`026` yang belum di-build/test pengguna secara resmi |
| Tanggal | 5 September 2026 |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `BillingManagement / Billing` |
| Submodule | — |
| Owner/prefix registry | Prefix `Bil`, kategori `BUSINESS DOMAIN / MODULE`, registry `ACTIVE` |
| Keberlakuan | `TOUCHED LEGACY` — dua entity yang **sudah ada** (`BilWriteOffCase`, `BilCalculationVersion`) mendapat field baru. Bukan `NEW CODE` (tidak ada entity baru), bukan `LEGACY MIGRATION` (tidak ada rename `Trx*`, tidak ada perubahan fisik yang menghapus/menormalkan kolom lama — murni tambah) |
| QBE ID yang berlaku | `QBE-ENT-002` (nullability/tipe mengikuti semantik domain — `Category` non-null string mengikuti pola `Status`, `NonBillableResidualAmount` non-null decimal mengikuti pola kolom uang lain di tabel yang sama), `QBE-ENT-003` (bukan field presentasi — keduanya data domain yang dipakai `BE-BKC-028`/`029`), `QBE-CFG-001` (Fluent API pada `IEntityTypeConfiguration<T>` yang sudah ada, bukan configuration baru) |
| QBE ID yang **tidak** berlaku | `QBE-NAM-*` (tidak ada entity/prefix baru), `QBE-DB-001/002` (bukan `LEGACY MIGRATION`, tidak ada rename atau dependensi fisik yang diaudit), `QBE-MOD-002/003` (module `Bil` sudah terdaftar `ACTIVE`) |

Tidak ada selisih governance dari task backend sebelumnya di modul ini.

---

## 1. Masalah yang diselesaikan

`BE-BKC-028` (perutean selisih yang tidak dapat ditagihkan ke ember terpisah dari piutang pasien)
dan `BE-BKC-029` (kategori/plafon/penjaga write-off selisih) butuh dua tempat penyimpanan baru yang
belum ada sama sekali di basis data: kategori pada `BilWriteOffCase` untuk membedakan write-off
piutang pasien biasa dari write-off selisih kontraktual, dan akumulator nominal selisih pada
`BilCalculationVersion`. Task ini **hanya** menyiapkan wadahnya di basis data — perutean nominal
dan aturan bisnisnya sendiri tetap scope `BE-BKC-028`/`029`.

## 2. Proses bisnis

Task ini tidak mengubah proses bisnis apa pun — tidak ada endpoint, tidak ada validasi, tidak ada
perubahan alur yang terlihat pengguna. Ini murni persiapan struktur data.

**Kenapa aman dijalankan tanpa mematikan layanan.** Kedua kolom `NOT NULL` dengan nilai bawaan,
sehingga baris lama otomatis terisi saat migration dijalankan (bukan oleh task ini). Nilai bawaannya
benar secara bisnis: seluruh `BilWriteOffCase` yang sudah ada memang write-off piutang pasien biasa
(`PATIENT_AR`), dan seluruh `BilCalculationVersion` yang sudah ada memang belum mengenal konsep
selisih tidak dapat ditagihkan sehingga nilainya nol.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Alasan |
| --- | --- |
| `docs/module-blueprints/billing-kasir/02-backend-architecture.md` baris 1371–1500 | Spesifikasi persis nama kolom, tipe, default, dan index gabungan |
| `Migrations/20260903015730_AddTariffIdToBilInvoiceItem.cs` | Referensi gaya migration additive terdekat (tambah kolom + index) |
| `Areas/.../Billing/Models/BilWriteOffCase.cs` | Lokasi field `Category` baru, berdampingan dengan `BillingWriteOffCaseStatuses` |
| `Areas/.../Billing/Models/BilCalculationVersion.cs` | Lokasi field `NonBillableResidualAmount` baru |
| `Repositories/Configurations/.../BilWriteOffCaseConfiguration.cs` | Pola Fluent API existing (`HasMaxLength`, `HasDefaultValue`, `HasIndex().HasFilter(...)`) yang sudah dipakai index lain di configuration yang sama |
| `Repositories/Configurations/.../BilCalculationVersionConfiguration.cs` | Pola `HasPrecision(18, 2)` yang dipakai seluruh kolom uang lain di tabel yang sama (`PatientAmount`, `UnresolvedCoverageAmount`, dst.) — dipakai persis sama untuk kolom baru |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/.../Billing/Models/BilWriteOffCase.cs` | `+Category` (`[Required, MaxLength(30)]`, default `BillingWriteOffCategories.PatientAr`); class baru `BillingWriteOffCategories` (`PatientAr = "PATIENT_AR"`, `NonBillableResidual = "NON_BILLABLE_RESIDUAL"`) berdampingan dengan `BillingWriteOffCaseStatuses` |
| `Areas/.../Billing/Models/BilCalculationVersion.cs` | `+NonBillableResidualAmount` (`decimal`) |
| `Repositories/Configurations/.../BilWriteOffCaseConfiguration.cs` | `+Property(x => x.Category).HasMaxLength(30).IsRequired().HasDefaultValue(BillingWriteOffCategories.PatientAr)`; `+HasIndex(x => new { x.InvoiceId, x.Category, x.Status }).HasFilter("\"IsDelete\" = false")` |
| `Repositories/Configurations/.../BilCalculationVersionConfiguration.cs` | `+Property(x => x.NonBillableResidualAmount).HasPrecision(18, 2)` |
| `Migrations/20260904232421_AddWriteOffCategoryAndNonBillableResidual.cs` | **Baru** — migration additive, isi lengkap di § 3.3 |
| `Migrations/20260904232421_AddWriteOffCategoryAndNonBillableResidual.Designer.cs` | **Baru** — snapshot point-in-time, dibuat otomatis oleh `dotnet ef migrations add`, tidak disunting manual (4,2 MB, di luar batas baca aman untuk direview baris per baris — lihat § 5) |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Diperbarui otomatis oleh `dotnet ef migrations add`; diperiksa lewat `grep` bertarget untuk memastikan **hanya** dua property dan satu index baru yang muncul, tidak ada drift lain (lihat § 5) |

Total: **4 berkas source disunting manual** (dua model, dua configuration) + **3 berkas migration
dihasilkan tool** (`.cs`, `.Designer.cs`, `ModelSnapshot.cs`). Tidak ada controller, DTO, atau
service yang disentuh — task ini murni lapisan entity/persistence.

### 3.3 Isi migration (`Up`)

```csharp
migrationBuilder.AddColumn<string>(
    name: "Category", schema: "public", table: "BilWriteOffCase",
    type: "character varying(30)", maxLength: 30, nullable: false,
    defaultValue: "PATIENT_AR");

migrationBuilder.AddColumn<decimal>(
    name: "NonBillableResidualAmount", schema: "public", table: "BilCalculationVersion",
    type: "numeric(18,2)", precision: 18, scale: 2, nullable: false,
    defaultValue: 0m);

migrationBuilder.CreateIndex(
    name: "IX_BilWriteOffCase_InvoiceId_Category_Status", schema: "public",
    table: "BilWriteOffCase", columns: new[] { "InvoiceId", "Category", "Status" },
    filter: "\"IsDelete\" = false");
```

`Down` membalikkan persis tiga operasi ini (`DropIndex`, dua `DropColumn`) — tidak ada operasi lain.

### 3.4 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| `API CONTRACT IMPACT` | **Nihil.** Tidak ada DTO, controller, atau response yang berubah. Kolom baru belum dibaca/ditulis oleh kode aplikasi mana pun (itu scope `BE-BKC-028`/`029`) |
| `DATABASE IMPACT` | **Additive murni.** Dua kolom `NOT NULL DEFAULT` (metadata-only di PostgreSQL untuk `ADD COLUMN ... DEFAULT`, bukan table rewrite) dan satu index gabungan terfilter baru. Tidak ada kolom dihapus, diganti tipe, atau diganti nama. Tidak ada tabel baru |
| `SECURITY IMPACT` | **Nihil.** Tidak ada perubahan authorization, tidak ada data sensitif pada kolom baru (kategori enum string dan nominal uang agregat) |
| `VISUAL REFERENCE` | `NOT REQUIRED` — tidak ada perubahan yang terlihat pengguna |

## 4. Dokumentasi endpoint

**Tidak ada endpoint baru maupun berubah.** Task ini tidak menyentuh lapisan API.

## 5. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet ef migrations add` | **DIJALANKAN — otorisasi khusus task ini** | Tool EF Core | Lihat catatan di bawah; keluaran akhir `Done. To undo this action, use 'ef migrations remove'`, exit code `0` |
| `dotnet build` (sebagai bagian internal `dotnet ef migrations add`) | **LULUS** (side effect dari tool di atas, bukan dijalankan terpisah) | Tool EF Core | Keluaran `Build started... Build succeeded.` sebelum tool menghasilkan migration |
| `dotnet test` | **BELUM DIJALANKAN** | `BLOCKED` | Instruksi eksplisit pengguna: test dijalankan manual |
| `dotnet ef database update` | **TIDAK DIJALANKAN** | `BLOCKED` — `BKC-GATE-09` | Pengguna eksplisit menegaskan ulang di tengah task ini: "jangan lakukan update-database, biarkan saya melakukannya secara manual" |
| Review `Up`/`Down` migration | **LULUS** | Manual | Isi persis tiga operasi (§ 3.3): dua `AddColumn` dan satu `CreateIndex` pada `Up`; pasangannya persis pada `Down`. Tidak ada `DropColumn`/`RenameColumn`/`DropTable` yang tidak diharapkan |
| Review drift `ApplicationDbContextModelSnapshot.cs` | **LULUS** | Manual (`grep` bertarget, bukan pembacaan penuh — berkas terlalu besar) | `grep -n "NonBillableResidualAmount"` hanya menghasilkan satu titik (definisi property `BilCalculationVersion`, presisi `(18,2)` konsisten dengan kolom uang lain di tabel yang sama). `grep -n "\"Category\""` menunjukkan definisi baru pada blok `BilWriteOffCase` (`IsRequired`, `HasMaxLength(30)`, `HasDefaultValue("PATIENT_AR")`) dan index barunya (`HasIndex("InvoiceId", "Category", "Status").HasFilter(...)`) — tepat sesuai perubahan model, empat kemunculan lain kata "Category" pada berkas yang sama adalah property tak terkait milik entity lain (`MstTariffCategory` dan sejenisnya), tidak tersentuh |
| Review `Designer.cs` baris-per-baris | **TIDAK DILAKUKAN — di luar batas baca aman** | Diketahui/didokumentasikan | Berkas 4,2 MB (melebihi batas baca 256 KB tool ini), persis seperti referensi `AddTariffIdToBilInvoiceItem.Designer.cs`. Ini adalah snapshot point-in-time yang dihasilkan otomatis oleh `dotnet ef migrations add`, bukan disunting manual — risiko drift dimitigasi lewat review `ModelSnapshot.cs` (baris di atas) yang mencerminkan model final yang sama |
| Nilai bawaan pada baris lama | **Diverifikasi lewat source migration, bukan query basis data** | Manual | `defaultValue: "PATIENT_AR"` dan `defaultValue: 0m` pada `AddColumn` — di PostgreSQL, `ADD COLUMN ... DEFAULT` mengisi seluruh baris lama dengan nilai itu tanpa perlu `UPDATE` terpisah. Tidak diverifikasi lewat query basis data aktual karena eksekusi migration tidak diberi wewenang pada task ini |
| Cakupan diff | **LULUS** | Manual | Perubahan manual persis 4 berkas (§ 3.2) plus 3 berkas migration hasil tool; tidak ada controller/DTO/service tersentuh |

> **`VALIDATION` belum lengkap dan task ini belum boleh dinyatakan selesai.** `dotnet test` wajib
> dijalankan pengguna. Migration **tidak** dijalankan ke basis data mana pun.

### Catatan mengenai `dotnet ef migrations add`

Instruksi baku pengguna melarang menjalankan `dotnet build`/`dotnet test` secara otomatis. Task ini
menemukan bahwa pembuatan migration EF Core yang aman **hanya** dapat dilakukan lewat tool
`dotnet ef migrations add` — berkas `Designer.cs` adalah snapshot point-in-time berukuran besar
(4 MB pada migration referensi) yang tidak layak ditulis manual tanpa risiko merusak riwayat diff
130+ migration yang ada. Hal ini disampaikan ke pengguna secara eksplisit sebelum bertindak, dan
pengguna memberi **otorisasi khusus dan sekali pakai** untuk task ini: *"Izinkan dotnet ef
migrations add khusus task ini"*. Otorisasi ini **tidak** mencakup `dotnet ef database update` —
pengguna menegaskan ulang secara terpisah di tengah task ini agar eksekusi migration ke basis data
tetap dilakukan manual olehnya.

Sebagai prasyarat, tool CLI `dotnet-ef` (versi `9.0.18`, menyamai versi package
`Microsoft.EntityFrameworkCore.Tools` pada project) dipasang sebagai **global tool** karena belum
ada di mesin (`dotnet tool install --global dotnet-ef --version 9.0.18`) — tanpa tool ini command
`dotnet ef` tidak dapat dijalankan sama sekali. Ini adalah prasyarat teknis, bukan perubahan pada
repository.

Keluaran command sempat menampilkan baris `"Fatal","@x":"...HostAbortedException: The host was
aborted..."` — ini **bukan error sebenarnya**. `dotnet ef` menjalankan `Program.cs` aplikasi dalam
mode design-time untuk membaca `ApplicationDbContext`, lalu sengaja menghentikan host setelah
DbContext-nya didapat; baris log itu adalah perilaku normal EF Core design-time tooling, bukan
kegagalan migration. Migration tetap berhasil dibuat (`Done.`, exit code `0`).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Keadaan |
| --- | --- |
| 1. Migration bersifat menambah saja — tidak ada kolom dihapus/diganti nama | **Terpenuhi** — lihat § 3.3 dan § 5 |
| 2. Seluruh baris `BilWriteOffCase` lama bernilai `PATIENT_AR` | **Terpenuhi secara desain** (`AddColumn ... defaultValue: "PATIENT_AR"`); tidak diverifikasi lewat query basis data karena eksekusi migration tidak diberi wewenang — lihat § 5 |
| 3. Seluruh baris `BilCalculationVersion` lama bernilai nol pada kolom baru | **Terpenuhi secara desain** (`AddColumn ... defaultValue: 0m`); sama seperti di atas |
| 4. Berkas migration dibuat **dan direview**; hasil review dilampirkan | **Terpenuhi** — § 3.3 (isi lengkap `Up`/`Down`) dan § 5 (hasil review drift `ModelSnapshot.cs`) |
| 5. Migration **tidak** dijalankan ke basis data mana pun tanpa otorisasi tersendiri | **Terpenuhi** — `dotnet ef database update` tidak dijalankan; `BKC-GATE-09` tetap tertutup |
| Build lulus | **Terpenuhi** — sebagai bagian internal `dotnet ef migrations add` (§ 5) |
| `dotnet test` lulus | **BELUM** — menunggu pengguna |

**Definition of Done belum sepenuhnya tercapai.** Yang tersisa: `dotnet test` manual oleh pengguna,
dan otorisasi terpisah untuk `dotnet ef database update` bila/ketika pengguna siap menjalankannya.

## 7. Catatan penutup

| Field | Nilai |
| --- | --- |
| `WARNINGS` | Migration ini **belum** dijalankan ke basis data mana pun. Kolom baru tidak akan ada secara fisik sampai `dotnet ef database update` dijalankan secara manual oleh pengguna — sesuai instruksinya |
| `KNOWN ISSUES` | `Designer.cs` (4,2 MB) tidak direview baris-per-baris karena melebihi batas baca tool; mitigasi lewat review `ModelSnapshot.cs` yang mencerminkan model akhir yang sama — lihat § 5 |
| `MANUAL TEST` | `NOT FEASIBLE` pada sesi ini — tidak ada perubahan yang terlihat pengguna (murni lapisan data) |
| `INCIDENTAL CHANGES` | Instalasi tool CLI global `dotnet-ef 9.0.18` sebagai prasyarat teknis command yang diotorisasi — bukan perubahan repository, dicatat untuk transparansi |
| `INTERRUPTIONS` | Tidak ada |
| `GIT STATUS` | 4 berkas source disunting manual + 3 berkas migration dihasilkan tool untuk task ini, belum di-stage. Working tree juga memuat perubahan tersendiri dari `BE-BKC-022`–`026` yang belum di-build/test resmi oleh pengguna. Tidak ada stage, commit, push, maupun operasi Git lain yang dilakukan |
| `NEXT RECOMMENDED STEP` | Jalankan `dotnet test` secara manual mencakup seluruh perubahan yang menumpuk (`BE-BKC-022` s.d. `027`). Migration boleh dijalankan ke basis data development kapan pun pengguna siap (otorisasi terpisah, `BKC-GATE-09`). Task backend berikutnya sesuai urutan: `BE-BKC-028` (`BLOCKED` oleh sequencing `BE-BKC-027` + `MVP-7`) |

## Update 6 September 2026 — migration dieksekusi, `BKC-GATE-09` ditutup untuk task ini

Pengguna mengonfirmasi `dotnet build`/`dotnet test` atas seluruh backlog `BE-BKC-022`–`030` lulus,
dan menjalankan `dotnet ef database update` secara manual ke database dev
(`QuilvianNewDevYasmina`). Dibuktikan langsung lewat query read-only (otorisasi eksplisit pengguna,
konsisten dengan audit `BE-BKC-031`):

| Pemeriksaan | Hasil |
| --- | --- |
| `20260904232421_AddWriteOffCategoryAndNonBillableResidual` tercatat di `__EFMigrationsHistory` | **Ya** |
| Kolom fisik `BilWriteOffCase.Category` | **Ada** — `character varying`, `NOT NULL`, default `'PATIENT_AR'::character varying` |
| Kolom fisik `BilCalculationVersion.NonBillableResidualAmount` | **Ada** — `numeric`, `NOT NULL`, default `0.0` |
| Baris lama `BilWriteOffCase`/`BilCalculationVersion` di database dev | **Nol baris pada keduanya** — database dev belum punya transaksi write-off/kalkulasi apa pun, sehingga acceptance criteria #2/#3 (nilai bawaan pada baris lama) terpenuhi secara vakum (tidak ada baris lama untuk diperiksa), bukan diverifikasi atas data sungguhan |
| Index yang dihasilkan | Additive-only — `IX_BilWriteOffCase_InvoiceId_Category_Status` baru; tidak ada index/kolom lama yang hilang |

**`BKC-GATE-09` ditutup untuk migration ini.** Acceptance criteria #5 ("migration tidak dijalankan
tanpa otorisasi tersendiri") tetap terpenuhi — otorisasi eksekusi diberikan eksplisit oleh pengguna
sendiri sebelum menjalankannya. Definition of Done task ini sekarang **tercapai**. Detail audit
lengkap lintas task `BE-BKC-027`–`030`: `task/report/backend/BE-BKC-032.md`.
