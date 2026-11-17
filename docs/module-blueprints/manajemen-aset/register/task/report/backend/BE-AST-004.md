# Laporan Perubahan Backend — `BE-AST-004`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-AST-004` |
| Judul | Migration `AddAssetManagementFoundation` |
| Slice | `REG-S1` Fondasi (gelombang `MVP-0`) |
| Roadmap | `docs/module-blueprints/manajemen-aset/register/roadmap/backend-roadmap.md` `REV-1`, kartu `BE-AST-004` |
| Trace | `AST-DES-003`; `02-module-map.md` bagian 3.2; kamus data `register` bagian 2–4 dan `nilai` bagian 2.1, 2.2, 3.1, 3.2, 4.3 |
| Contract version | `AST-REG-1.0.0` dan `AST-NLI-1.0.0` — `approved` 2026-11-12 (`AST-DEC-051`). Task ini tidak menyentuh endpoint |
| Dependency | `BE-AST-003` ✅ 2026-11-12; `BE-AST-014` [NLI] ✅ 2026-11-16 |
| Klasifikasi | `MEDIUM` — satu repository, berkas generated EF, dampak schema 25 tabel; tanpa logika bisnis, tanpa kontrak API, tanpa UI |
| Pembagian kerja | `dotnet ef migrations add` dan `dotnet ef database update` oleh **Rizki** (`migration-dikerjakan-rizki-sendiri`). Agent: pemeriksaan snapshot sebelum migration, pemeriksaan isi migration, bukti basis data (baca saja), laporan, penandaan status |
| Target tulis agent | Laporan ini; baris status roadmap `register` dan `requirement-traceability.md` sub-modul `register`. Agent tidak menyunting berkas migration maupun snapshot |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat diperiksa | `b4d9be05` (merge `QuilvianIntegrationBackend` → `rizkiG` oleh Rizki, di atas `9c5ccfaa` `BE-AST-014`); berkas migration belum di-commit |
| Mesin | PC kantor |
| Tanggal | 2026-11-16 |
| Status | ✅ Selesai — ketiga acceptance criteria terbukti dari isi migration, Designer, dan snapshot; `dotnet build` (DLL 10:15:04 WIB, memuat migration) dan `database update` oleh Rizki 16 November 2026; basis data terbukti memuat migration, 25 tabel, 535 kolom, 46 FK `Restrict`, 116 index, dan check constraint buku. **Riwayat:** 🟡 pada ±10.05 WIB karena DLL belum memuat migration dan basis data belum menerapkannya |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` |
| Module | `AssetManagement` (Manajemen Aset & Inventaris) |
| Submodule | `Register` dan `Valuation` (satu migration bersama, `AST-DES-003`); `MasterData` baris `Mst` |
| Pemilik / prefix registry | `AssetManagement` / `Ast` / `ACTIVE` (baris registry `BE-AST-001`) |
| Keberlakuan | `NEW CODE` — dua berkas migration baru (generated EF); `TOUCHED LEGACY` hanya untuk `ApplicationDbContextModelSnapshot.cs` (+2815 baris, 0 dihapus) |
| QBE ID yang berlaku | `QBE-DB-*` untuk migration aditif (tanpa rename, tanpa pengisian data); `QBE-MOD-002` untuk tabel baru. Tidak berlaku: `QBE-NAM-003`/`QBE-DB-001/002` (bukan legacy migration), `QBE-SVC/API/PERM/DTO` |
| Wewenang terpisah | Pembuatan migration dan eksekusi database adalah wewenang pemilik dan dikerjakan pemilik. Agent hanya membaca basis data dengan sesi `default_transaction_read_only=on` |

### Pemeriksaan snapshot sebelum migration

Pemeriksaan sebelum `migrations add` menemukan bahwa lima migration HR SQL mentah (`20261110040000`,
`20261110050000`, `20261111020000`, `20261111030000`, `20261111040000`, MHamzah1) tidak memperbarui snapshot
`rizkiG`. Akibatnya model memuat 25 tabel Aset **ditambah** `HrdNotification` dan `HrdSuspensionAccessAction`,
yang sudah ada di basis data. Migration yang dibuat saat itu akan ikut membuat ulang kedua tabel HR tersebut.

Rizki lalu menggabungkan `origin/QuilvianIntegrationBackend` ke `rizkiG` (`b4d9be05`) dan menjalankan
`database update` untuk empat migration HR integration. Hasil pemeriksaan ulang:

| Pemeriksaan | Hasil |
|---|---|
| Snapshot hasil merge vs kedua induknya | 1008 blok = 1004 blok integration + 4 tabel `Nvw*`; +435 baris, **0 baris integration terhapus**; tidak ada blok `rizkiG` yang hilang selain rename HR milik integration |
| Model source vs snapshot (nama tabel) | Selisih **tepat 25 tabel Aset**: 17 `Ast*` + 8 `MstAsset*` |
| Migration di basis data | 413 dari 413 diterapkan, 0 tertunda |

---

## 1. Masalah yang diperbaiki

Sesudah `BE-AST-002`, `BE-AST-003`, dan `BE-AST-014`, kedua puluh lima tabel modul Aset sudah ada di model EF,
tetapi belum ada di basis data. Akibatnya service register dan nilai (`BE-AST-005` dan seterusnya) belum bisa
menyimpan apa pun.

Contoh: monitor pasien yang dicatat lewat layar Pencatatan Aset nanti akan gagal disimpan dengan galat
"relation AstAsset does not exist" sampai migration ini diterapkan.

---

## 2. Proses bisnis

| Unsur | Isi |
|---|---|
| Tujuan | Membuat 25 tabel fondasi Aset di basis data dalam satu migration aditif yang dapat dibatalkan |
| Pelaku | Rizki (membuat dan menerapkan migration); agent (memeriksa) |
| Pemicu | Dependency `BE-AST-003` dan `BE-AST-014` selesai |
| Hasil akhir | Migration `20261116030049_AddAssetManagementFoundation` beserta Designer dan snapshot; setelah diterapkan, 25 tabel tersedia untuk `BE-AST-005` |

Migration ini **hanya menambah**. Tidak ada tabel modul lain yang diubah, tidak ada data yang diisi, dan tidak
butuh downtime. Bila dibatalkan, `Down()` menghapus kedua puluh lima tabel dalam urutan yang aman terhadap FK.

---

## 3. Perubahan yang diperiksa

### 3.1 Berkas

| Berkas | Pembuat | Isi |
| --- | --- | --- |
| `Migrations/20261116030049_AddAssetManagementFoundation.cs` | Rizki (`dotnet ef migrations add`) | 1838 baris. `Up()`: 25 `CreateTable` + 91 `CreateIndex`, tanpa operasi lain. `Down()`: 25 `DropTable` |
| `Migrations/20261116030049_AddAssetManagementFoundation.Designer.cs` | Rizki (generated) | `ProductVersion` `9.0.18`; 1033 blok `b.ToTable(`, sama dengan snapshot |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Rizki (generated) | +2815 baris, 0 dihapus; tepat 25 blok entity baru, semuanya `Ast*`/`MstAsset*` |
| `docs/module-blueprints/manajemen-aset/register/task/report/backend/BE-AST-004.md` | Agent | Laporan ini |
| `docs/module-blueprints/manajemen-aset/register/roadmap/backend-roadmap.md` | Agent | Tanda status `BE-AST-004` |
| `docs/module-blueprints/manajemen-aset/register/roadmap/requirement-traceability.md` | Agent | Bukti baris `CAP-02.1`–`02.6` |

Kedua puluh lima tabel: `MstAssetCategory`, `MstAssetCategoryDocumentRequirement`, `MstAssetClassification`,
`MstAssetDocumentType`, `MstAssetGroup`, `MstAssetLocation`, `MstAssetOwnershipType`, `MstAssetSetting`,
`AstAsset`, `AstAssetPlacement`, `AstAssetHandover`, `AstAssetHandoverItem`, `AstConditionReport`,
`AstAssetHistory`, `AstAssetDocument`, `AstAttachment`, `AstUtilization`, `AstUtilizationItem`,
`AstImportBatch`, `AstImportBatchRow`, `AstValuationParameter`, `AstDepreciationPolicy`, `AstAssetBook`,
`AstAssetValueEntry`, `AstAccountingOutbox` — seluruhnya di schema `public`.

FK keluar modul hanya menuju tabel yang sudah ada: `FinGoodsReceipt`, `FinPurchaseOrder`, `MstCostCenter`,
`MstEmployee`, `MstHospitalSite`, `MstLegalEntity`, `MstOrganizationUnit`, `MstRoom`. Semuanya `Restrict`.

### 3.2 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` |
| Database | Setelah diterapkan: 25 tabel baru, 535 kolom, 91 index, check constraint `CK_AstAssetBook_NotBelowResidual`. Tidak ada tabel lama yang berubah. **Belum diterapkan** saat laporan ditulis |
| Keamanan/Auth | Tidak ada endpoint. Kolom sensitif tetap sesuai laporan `BE-AST-002`, `003`, `014` |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Hitung operasi migration | `Up()`: 25 `CreateTable`, 91 `CreateIndex`, tidak ada `AddColumn`/`AlterColumn`/`Rename*`/`Drop*`/`Sql`. `Down()`: 25 `DropTable` | `PASS` | `grep migrationBuilder.*` |
| Pencocokan isi migration ↔ source (skrip Node) | **113 pemeriksaan, 0 gagal**: daftar 25 tabel persis, schema `public`, 91 index seluruhnya di tabel Aset, `Down()` menghapus 25 tabel yang sama, jumlah kolom tiap tabel = properti model + 10 kolom audit `IdentityModel` (total 535), PK tiap tabel, seluruh FK `Restrict`, check constraint buku persis DDL, tiga index unik parsial `BE-AST-003` dengan filter persis kamus, empat index unik `BE-AST-014` tanpa filter | `PASS` | Keluaran skrip |
| Uji mutasi pemeriksa (3 mutasi pada salinan migration: filter penempatan dibalik, FK `Cascade`, `OR`→`AND` pada check) | 3 dari 3 tertangkap | `PASS` | Pemeriksa tidak lolos-buta |
| Snapshot sebelum migration (hasil merge) | 1008 blok; 0 baris integration terhapus; selisih model ↔ snapshot tepat 25 tabel Aset | `PASS` | Bagian "Pemeriksaan snapshot" |
| Snapshot sesudah migration | +2815/−0 baris; 25 blok baru, semuanya Aset; 1033 blok = Designer | `PASS` | `git diff --numstat`, hitung `b.ToTable(` |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` | `Files evaluated: 3`; 0 temuan; `PASS` | `PASS` | Ketiga berkas `Migrations/` |
| `dotnet build` yang memuat migration | Dijalankan Rizki 16 November 2026. DLL `bin/Debug/net9.0/QuilvianSystemBackend.dll` bertanggal 10:15:04 WIB, sesudah berkas migration (10:00:49), dan memuat string `20261116030049_AddAssetManagementFoundation` | `PASS` | Jumlah warning tidak dilaporkan. Pada pemeriksaan pertama (±10.05) DLL masih 10:00:24 tanpa migration ini |
| Penerapan ke basis data `QuilvianNewDevRizki` (baca saja, `dotnet fsi` + Npgsql dari `bin`, `default_transaction_read_only=on`) | **Diterapkan.** Baris terakhir `__EFMigrationsHistory` = `20261116030049_AddAssetManagementFoundation` (`9.0.18`); 25 tabel `Ast*`/`MstAsset*` di `public`, namanya persis daftar bagian 3.1; 535 kolom (= isi migration); 46 FK (= 46 `table.ForeignKey` di `Up()`), 0 di antaranya bukan `Restrict`; 116 index (= 91 `CreateIndex` + 25 PK); `CK_AstAssetBook_NotBelowResidual` ada; 0 baris data | `PASS` | Pemeriksaan pertama ±10.05 WIB: belum diterapkan (dugaan `--no-build` dengan DLL lama). Diterapkan ulang oleh Rizki sesudah build |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.

**Perintah untuk pemilik:**

```powershell
dotnet build -p:RunAnalyzers=false
dotnet ef database update --no-build
```

Bila berhasil, keluaran `database update` memuat `Applying migration '20261116030049_AddAssetManagementFoundation'`.

**Perintah tersebut sudah dijalankan Rizki.** **Tidak dijalankan oleh agent:** `dotnet build`, `dotnet ef migrations add`, `dotnet ef database update`, dan
perintah tulis apa pun ke basis data.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Migration hanya menambah 25 tabel; tidak mengubah tabel modul lain | Terpenuhi | `Up()` hanya `CreateTable`/`CreateIndex`; daftar tabel persis 25; seluruh index di tabel Aset |
| 2. `Down()` menghapus 25 tabel | Terpenuhi | 25 `DropTable`, himpunan sama dengan `Up()` |
| 3. Snapshot memuat seluruh entity baru tanpa duplikasi blok entity branch lain | Terpenuhi | +25 blok, 0 baris dihapus, tidak ada nama tabel ganda; snapshot = Designer (1033 blok) |
| Cakupan: berkas migration, Designer, snapshot | Terpenuhi | Bagian 3.1 |
| `DoD-BE`: `V-BE` dijalankan dan dicatat apa adanya | Terpenuhi | Pemeriksaan isi, QBE, dan basis data oleh agent; `dotnet build` oleh Rizki, diperiksa dari DLL |
| Catatan kartu: penerapan ke database oleh pemilik | Terpenuhi | Rizki 16 November 2026; dibuktikan dari `__EFMigrationsHistory` dan katalog PostgreSQL |
| `DoD-BE`: laporan, roadmap, traceability, `git status --short`; tanpa commit kecuali diminta | Terpenuhi | Laporan ini; bagian 7 |

Rekap: **3 dari 3 acceptance criteria terpenuhi**, seluruh butir `DoD-BE` terpenuhi, dan migration sudah
diterapkan → `✅`. Riwayat: `🟡` pada ±10.05 WIB sebelum build dan penerapan.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Lima migration HR SQL mentah tanpa Designer (bagian "Pemeriksaan snapshot") tetap ada di riwayat. Snapshot integration sudah menutup selisih nama tabelnya. Selisih tingkat kolom tidak terlihat pada migration ini, karena `Up()` tidak memuat satu pun `AlterColumn`/`AddColumn` |
| Masalah yang diketahui | Grafik dependency `pergerakan` dan `inventarisasi` memuat cermin baca-saja node `BE-AST-004 [REG]`. Tanda status tidak disisipkan di sana karena akan menggeser kolom grafik; struktur grafik di luar wewenang penandaan status |
| Risiko tersisa | (1) Jumlah warning build tidak tercatat. (2) Sampai migration ini di-commit, branch lain yang membuat migration dari model yang sama akan ikut membawa 25 tabel Aset |
| Perubahan sampingan | `NONE`. Skrip pemeriksa, salinan mutasi, dan skrip query baca saja berada di scratchpad sesi |
| Interupsi | `NONE` |
| Langkah berikutnya | Commit `BE-AST-004`, lalu `BE-AST-005` service bersama (lima service + opsi konfigurasi) |

`git status --short` saat laporan ditulis:

```text
 M Migrations/ApplicationDbContextModelSnapshot.cs
 M docs/module-blueprints/manajemen-aset/register/roadmap/backend-roadmap.md
 M docs/module-blueprints/manajemen-aset/register/roadmap/requirement-traceability.md
 M docs/module-blueprints/nurse-view/roadmap/frontend-roadmap.md
 M docs/module-blueprints/nurse-view/roadmap/requirement-traceability.md
 M docs/module-blueprints/nurse-view/task/report/frontend/FE-NV-016.md
?? Migrations/20261116030049_AddAssetManagementFoundation.Designer.cs
?? Migrations/20261116030049_AddAssetManagementFoundation.cs
?? docs/module-blueprints/manajemen-aset/register/task/report/backend/BE-AST-004.md
```

Ketiga berkas `nurse-view` sudah berubah sebelum task ini dimulai dan bukan milik `BE-AST-004`.
