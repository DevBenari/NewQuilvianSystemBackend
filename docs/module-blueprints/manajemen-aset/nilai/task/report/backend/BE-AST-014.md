# Laporan Perubahan Backend — `BE-AST-014`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-AST-014` |
| Judul | Tabel dasar nilai |
| Slice | `NLI-S1` Tabel dasar, parameter, buku nilai, kotak keluar (gelombang `MVP-0`) |
| Roadmap | `docs/module-blueprints/manajemen-aset/nilai/roadmap/backend-roadmap.md` `REV-1`, kartu `BE-AST-014` |
| Trace | `AST-DES-003`; `CAP-EXT-02`; `data/data-dictionary.md` bagian 1, 2.1, 2.2, 3.1, 3.2, 4.3, 5; `02-backend-architecture.md` bagian 4.1, 5.1, 5.4, 5.6, 6, 7 |
| Contract version | `AST-NLI-1.0.0` — `approved` 2026-11-12 (`AST-DEC-051`). Task ini tidak menyentuh endpoint |
| Dependency | `BE-AST-002` ✅ 2026-11-12 (`MstAssetGroup`); `AstAsset` dari `BE-AST-003` ✅ 2026-11-12 |
| Klasifikasi | `MEDIUM` — satu repository, 12 berkas berubah, dampak entity/schema, kolom rupiah sensitif; tanpa logika bisnis, tanpa kontrak API, tanpa UI |
| Task mode | `BACKEND` — perintah Rizki (pemilik modul) "kerjakan sesuai urutan langkah selanjutnya", 2026-11-16, sesudah review keadaan modul yang menyebut `BE-AST-014` sebagai langkah pertama |
| Target tulis | `Areas/Corporate/AssetManagement/Valuation/{Enums,Models}/`; `Repositories/Configurations/Corporate/AssetManagement/Valuation/`; `Repositories/ApplicationDbContext.cs`; laporan ini; baris status roadmap `nilai` dan `register`, `requirement-traceability.md` sub-modul `nilai` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `94ad334e8df08a1772938422fb30674c67952a66` (branch `rizkiG`, sama dengan `origin/rizkiG`, 0 di depan / 0 di belakang) |
| Mesin | PC kantor |
| Tanggal | 2026-11-16 |
| Status | ✅ Selesai — keempat acceptance criteria terpetakan ke source; `V-BE` lengkap: QBE `PASS`, pencocokan statis, uji mutasi, review diff, dan `dotnet build` oleh Rizki 16 November 2026 berhasil (DLL 09:07:33 WIB memuat seluruh tipe baru). Belum di-commit. **Riwayat:** 🟡 sebagian pada 16 November 2026 sebelum build, karena butir `dotnet build` belum dijalankan |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` |
| Module | `AssetManagement` (Manajemen Aset & Inventaris) |
| Submodule | `Valuation` (Nilai Aset) |
| Pemilik / prefix registry | `AssetManagement / Valuation / Nilai Aset` / `Ast` / `ACTIVE` (baris registry `BE-AST-001`) |
| Keberlakuan | `NEW CODE` (11 berkas baru); `TOUCHED LEGACY` hanya untuk `ApplicationDbContext.cs` (+9 baris aditif) |
| QBE ID yang berlaku | `QBE-ENT-001`, `QBE-ENT-002`, `QBE-ENT-003`, `QBE-NAM-001`, `QBE-NAM-002`, `QBE-NAM-004`, `QBE-CFG-001`, `QBE-MOD-001`, `QBE-MOD-002`, `QBE-MOD-003`, `QBE-ENUM-001`, larangan `SortOrder`. Tidak berlaku: `QBE-SVC/API/PERM/DTO/CODE-001..003` (tanpa controller/service/alokator), `QBE-NAM-003`/`QBE-DB-001/002` (bukan legacy migration) |
| Governance terbaca | `AGENTS.md` backend, `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` (baris 49–54, 89, 159), `tooling/qbe/README.md`; tidak ada yang berubah sejak `BE-AST-003` |

### Pemeriksaan snapshot

HEAD `94ad334e` sama dengan `origin/rizkiG`. Working tree sebelum task hanya memuat tiga berkas `nurse-view` milik
sesi lain (lihat bagian 7); berkas itu tidak disentuh. Nama 5 class dan 3 enum baru masing-masing hanya punya satu
deklarasi di source. Target FK `MstLegalEntity` dan `MstCostCenter`
(`HumanResource/MasterData/Organization`), `MstAssetGroup` (`BE-AST-002`), dan `AstAsset` (`BE-AST-003`) tidak
berubah sejak dirujuk. Blueprint `nilai` tetap `AST-NLI-1.0.0` `approved`.

---

## 1. Masalah yang diperbaiki

Migration fondasi `BE-AST-004` dirancang berisi 25 tabel: 20 tabel `register` dan 5 tabel dasar `nilai`
(`AST-DES-003`). Sesudah `BE-AST-003`, kedua puluh tabel register sudah ada di model EF, tetapi kelima tabel nilai
belum. Akibatnya migration fondasi belum bisa dibuat, dan service register `BE-AST-007` tidak punya tempat untuk
menulis buku nilai saat aset disetujui.

Contoh: monitor pasien seharga Rp45.000.000 yang disetujui sebagai aset belum bisa dicek terhadap batas
kapitalisasi badan hukumnya. Buku nilainya juga belum bisa dibuat (umur 60 bulan, residu 10% menurut kebijakan
kelompok Alat Kedokteran), dan kejadian akuntansinya belum bisa diantrekan ke kotak keluar.

Task ini hanya mendefinisikan kelima tabel itu di source aplikasi. Tabelnya baru ada di database sesudah
migration `BE-AST-004` dibuat dan diterapkan oleh pemilik.

---

## 2. Proses bisnis

| Unsur | Isi |
|---|---|
| Tujuan | Menyediakan bentuk data parameter nilai, kebijakan penyusutan per kelompok, buku nilai per aset, riwayat perubahan nilai, dan kotak keluar kejadian ke Akuntansi |
| Pelaku | Kepala Akuntansi (parameter, kebijakan), Petugas Aset (lewat penyusutan dan perubahan nilai), sistem (buku, entri, kotak keluar). Semuanya lewat service `BE-AST-015` dan `BE-AST-025..031`; task ini hanya bentuk datanya |
| Pemicu | Langkah pertama `NLI-S1`, prasyarat `BE-AST-004` |
| Hasil akhir | 5 entity, 5 configuration, 5 DbSet, dan 3 enum siap masuk migration `BE-AST-004` |

| Tabel | Perannya | Contoh |
|---|---|---|
| `AstValuationParameter` | Batas kapitalisasi dan batas persetujuan Direktur untuk penghapusan dan pemanfaatan, per badan hukum dan bulan berlaku | Badan hukum RS, berlaku 1 Des 2026: kapitalisasi Rp1.000.000, penghapusan Rp50.000.000, pemanfaatan Rp100.000.000 |
| `AstDepreciationPolicy` | Umur ekonomis dan persentase residu per kelompok aset, per bulan berlaku | Alat Kedokteran, berlaku 1 Des 2026: 60 bulan, residu 10,00% |
| `AstAssetBook` | Satu buku nilai per aset: harga, biaya tercatat, residu, umur, akumulasi penyusutan dan penurunan nilai, status buku | Monitor `AST-ALKMED-000001`: biaya Rp45.000.000, residu Rp4.500.000, 3 dari 60 bulan sudah disusutkan |
| `AstAssetValueEntry` | Riwayat setiap perubahan nilai (menu 9.3); tidak diubah dan tidak dihapus | Penyusutan Des 2026: akumulasi penyusutan +Rp675.000, pusat biaya IGD |
| `AstAccountingOutbox` | Antrean kejadian ke modul Akuntansi untuk seluruh sub-modul Aset | `AST-PENYUSUTAN` periode Des 2026, Rp12.400.000, status `Held` sampai kode dinyalakan |

Aturan yang dijaga **database** sejak task ini (berlaku setelah migration):

1. **Satu parameter per badan hukum per bulan berlaku.** Contoh: parameter badan hukum RS yang berlaku
   1 Des 2026 sudah ada. Baris kedua untuk badan hukum dan bulan yang sama ditolak database, sehingga perubahan
   batas harus berlaku mulai bulan lain.
2. **Satu kebijakan per kelompok per bulan berlaku**, dengan pola yang sama.
3. **Satu buku per aset.** Buku kedua untuk monitor yang sama ditolak database, termasuk sesudah aset dihapus,
   karena buku tetap memegang riwayatnya.
4. **Nilai buku tidak boleh di bawah residu untuk aset yang dikapitalisasi.** Contoh: monitor dengan biaya
   Rp45.000.000 dan residu Rp4.500.000 sudah berakumulasi penyusutan Rp40.500.000, sehingga nilai bukunya sama
   dengan residu. Penyusutan tambahan Rp1 ditolak database lewat `CK_AstAssetBook_NotBelowResidual`. Barang yang
   tidak dikapitalisasi (`IsCapitalized = false`) tidak terkena aturan ini.
5. **Kejadian akuntansi tidak terkirim ganda.** Pasangan `(SourceTransactionId, SourceVersion)` unik, jadi bila
   proses penyusutan Des 2026 diulang karena gangguan, baris kotak keluar kedua dengan sumber dan versi yang sama
   ditolak database.
6. **Penimpaan diam-diam pada buku dicegah** lewat token `RowVersion`. Bila proses penyusutan dan persetujuan
   perubahan nilai menyimpan buku yang sama hampir bersamaan, simpanan kedua gagal dan dijawab `409` oleh service.
7. Tidak ada hapus berantai: seluruh FK `Restrict`.

Yang **bukan** tanggung jawab tabel dan menjadi tugas service berikutnya: rentang umur 1–600 bulan dan residu
0–100%, aturan baris pertama parameter boleh bulan berjalan (`AST-DES-032`), keputusan kapitalisasi
(`AST-RULE-001`), larangan mengubah/menghapus `AstAssetValueEntry`, penahanan kode `Held`, dan penyaringan kolom
sensitif dari respons dan log (`BE-AST-015`, `BE-AST-025..031`).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`data/data-dictionary.md` sub-modul `nilai` (seluruhnya); `02-backend-architecture.md` bagian 2–8;
`contracts/api-contract.md` (endpoint `DELETE`), `contracts/state-transition-matrix.md` (larangan menghapus
entri); kartu dan tabel task roadmap `nilai` dan `register`; laporan `BE-AST-003`; preseden
`AstAssetPlacement`/`AstConditionReport`/`AstImportBatch`/`AstAsset` beserta configuration-nya,
`AssetRegisterEnums.cs`, `FinAccountingEventOutboxConfiguration.cs` (pola `HasCheckConstraint` di dalam
`ToTable`); model target FK `MstLegalEntity`, `MstCostCenter`; `Repositories/ApplicationDbContext.cs`
(`ApplyConfigurationsFromAssembly`); `tooling/qbe/README.md`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/AssetManagement/Valuation/Enums/AssetValuationEnums.cs` | Baru — `AssetBookStatus`, `ValueEntryType`, `AccountingOutboxStatus`, nilai persis arsitektur bagian 5.4 |
| `.../Valuation/Models/AstValuationParameter.cs` | Baru — 9 kolom kamus 2.1; navigation `LegalEntity` |
| `.../Valuation/Models/AstDepreciationPolicy.cs` | Baru — 7 kolom kamus 2.2; navigation `AssetGroup` |
| `.../Valuation/Models/AstAssetBook.cs` | Baru — 18 kolom kamus 3.1; navigation `Asset`, `DepreciationPolicy` |
| `.../Valuation/Models/AstAssetValueEntry.cs` | Baru — 14 kolom kamus 3.2; navigation `Asset`, `CostCenter` |
| `.../Valuation/Models/AstAccountingOutbox.cs` | Baru — 17 kolom kamus 4.3; tanpa navigation |
| `Repositories/Configurations/Corporate/AssetManagement/Valuation/Ast*Configuration.cs` (5 berkas) | Baru — tabel `public."<Entity>"`, panjang kolom, presisi `numeric(18,2)` dan `numeric(5,2)`, `date`, `text`, enum `HasConversion<int>()`, default DDL, index, unik, check constraint, FK `Restrict`, token `RowVersion` pada buku, kolom audit seperti tabel register |
| `Repositories/ApplicationDbContext.cs` | +1 `using`, +1 region `CORPORATE - ASSET MANAGEMENT - VALUATION` berisi 5 DbSet; 9 baris ditambah, 0 baris lain berubah |
| `docs/module-blueprints/manajemen-aset/nilai/task/report/backend/BE-AST-014.md` | Laporan ini |
| `docs/module-blueprints/manajemen-aset/nilai/roadmap/backend-roadmap.md` | Tanda status `BE-AST-014` |
| `docs/module-blueprints/manajemen-aset/register/roadmap/backend-roadmap.md` | Tanda status node `BE-AST-014 [NLI]` pada grafik dependency |
| `docs/module-blueprints/manajemen-aset/nilai/roadmap/requirement-traceability.md` | Bukti baris `CAP-EXT-02` |

Seluruh berkas `.cs` baru **tanpa satu pun baris komentar** (`//`, `///`, `/* */`). Ini berbeda dari berkas
`BE-AST-002/003`, yang masih memuat 342 baris komentar menunggu keputusan Rizki.

Keputusan teknis di dalam cakupan:

| Keputusan | Alasan |
|---|---|
| Kelima index unik **tanpa** filter `IsDelete` | Kamus menulis "Unik" tanpa syarat. Kontrak `AST-NLI-1.0.0` tidak memberi jalur hapus untuk kelima tabel ini (satu-satunya `DELETE` milik pemetaan pusat biaya, `BE-AST-026`), dan matriks status melarang menghapus entri |
| Unik buku per aset lewat `HasIndex(AssetId).IsUnique()` dengan relasi `WithMany()` | Pola register: FK ke tabel lain tanpa koleksi balik, sehingga `AstAsset` (`BE-AST-003`) tidak disentuh. Nama index persis DDL: `IX_AstAssetBook_AssetId` |
| `AstAssetValueEntry` merujuk `AstAsset`, bukan `AstAssetBook` | Kamus 3.2 memberi FK `AstAsset`; panah "buku → entri" pada diagram kelas 4.1 hanya hubungan logis lewat aset yang sama, tanpa kolom `BookId` |
| `AstAccountingOutbox.CostCenterId`, `SourceDocumentId`, `AccountingEventId` tanpa FK | Kamus 4.3 tidak memberi kolom relasi, dan DDL bagian 5 menulis `"CostCenterId" uuid` tanpa constraint. Dokumen sumber berasal dari berbagai tabel dan `AccountingEventId` milik modul Akuntansi |
| Default database hanya untuk `DepreciatedMonths`, `AccumulatedDepreciation`, `AccumulatedImpairment` (`0`) dan `AttemptCount` (`0`). Bawaan lain (`ResidualValue`, `ResidualValuePercent`, `HasEstimateOverride`, `BookStatus`, `Status`, `SourceVersion = "1"`, `RowVersion`) hanya lewat initializer C# | Persis DDL kamus bagian 5. Aturan yang sama dengan `BE-AST-003` |
| Tanpa check constraint untuk `UsefulLifeMonths` 1–600 dan `ResidualValuePercent` 0–100 | Kamus menulisnya di kolom keterangan, bukan sebagai `CHECK`; acceptance criteria hanya meminta check nilai buku ≥ residu. Rentang dijaga service (`BE-AST-015`). Sama dengan keputusan `SourceType` pada `BE-AST-003` |
| Index `(SourceDocumentType, SourceDocumentId)` komposit pada kotak keluar | Kamus menulis satu "Index" untuk pasangan kolom itu |
| Index tambahan untuk FK `DepreciationPolicyId` dan `CostCenterId` | Pola register: setiap FK yang bukan kolom pertama index komposit punya index sendiri (EF juga akan membuatnya sendiri) |
| `DocumentDate` dan `SubledgerJson` ikut dibuat walau tidak tercantum di DDL bagian 5 | DDL adalah cuplikan; tabel kamus 4.3 adalah sumber kolom lengkap |
| Hanya 3 dari 6 enum sub-modul `nilai` | `ValueChangeType`, `ValueChangeStatus`, `DepreciationAdjustmentReason` milik tabel urutan 4 (`BE-AST-025`). Kartu task hanya menyebut ketiga enum ini |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — tidak ada controller, DTO, atau endpoint |
| Database | Lima entity baru masuk model EF, sehingga model kini memuat 25 tabel aset tanpa migration. **Belum ada migration** (`BE-AST-004`), **belum ada perubahan database**. Aplikasi tetap berjalan karena `PendingModelChangesWarning` sudah diabaikan |
| Keamanan/Auth | Tidak ada endpoint. Kolom sensitif menurut kamus: seluruh kolom rupiah (`CapitalizationThreshold`, `DisposalDirectorThreshold`, `UtilizationDirectorThreshold`, `AcquisitionCost`, `CapitalizedCost`, `ResidualValue`, `AccumulatedDepreciation`, `AccumulatedImpairment`, ketiga `*Delta`, `Amount`) serta `ComponentsJson` dan `SubledgerJson`. Karena aturan nol komentar, penandanya ada di laporan ini, bukan di model. Penyaringannya dari respons dan log adalah tugas DTO/service. `LastError` menurut kamus tidak boleh memuat kredensial atau payload; itu juga dijaga oleh worker `BE-AST-029` |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — endpoint nilai (`contracts/api-contract.md` 2.1–2.10) adalah cakupan `BE-AST-015` dan
`BE-AST-026..031`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` (WorkingTree) | `Files evaluated: 12`; `VIOLATION: 0`, `REVIEW: 0`, `INFO: 0`; `Findings: none`; `Final result: PASS` | `PASS` | 12 = 11 berkas baru + `ApplicationDbContext.cs`; berkas `nurse-view` yang berubah berupa `.md`, jadi tidak dievaluasi |
| Pencocokan statis kamus 2.1/2.2/3.1/3.2/4.3 + DDL bagian 5 + arsitektur 5.4 ↔ source (skrip Node) | **389 pemeriksaan, 0 gagal** atas 65 kolom pada 5 tabel: jumlah kolom model = kamus, tipe dan nullability, `[MaxLength]`/`HasMaxLength`, `[Required]`/`IsRequired`, presisi, `date`, `text`, `HasConversion<int>()`, initializer bawaan, default DDL, setiap FK (jumlah, `Restrict`, `IsRequired(false)`, tipe navigation), setiap index (unik/tidak, tanpa filter, nama DDL), jumlah index unik, check constraint persis DDL, token `RowVersion` hanya pada buku, nilai ketiga enum, 5 DbSet, nol baris komentar, tidak ada `SortOrder`, nama tabel/schema, `IdentityModel` | `PASS` | Keluaran skrip |
| Uji mutasi pencocok (salinan di scratchpad, 11 mutasi: token `RowVersion` dihapus, unik kotak keluar dilepas, `>=` jadi `>` pada check, `Held = 5`, `MaxLength(150→200)`, FK `Cascade`, presisi `5,2→18,2`, `DateOnly?` jadi `DateOnly`, kolom `DocumentDate` dihapus, bawaan `BookStatus` diganti, satu komentar disisipkan) + 1 kontrol | 11 dari 11 tertangkap (1–2 kegagalan masing-masing); kontrol 0 gagal | `PASS` | Pencocok tidak lolos-buta |
| Akhiran baris dan kolisi nama | 11 berkas `.cs` baru CRLF seragam tanpa BOM; `ApplicationDbContext.cs` tetap 1557 baris CRLF, 0 LF tunggal; 8 nama tipe baru masing-masing satu deklarasi | `PASS` | Hitung byte lewat Node; `grep` |
| Review diff dan cakupan | `ApplicationDbContext.cs` +9 baris, 0 dihapus; 11 berkas baru; tidak ada `Migrations/`, `Program.cs`, `*.csproj`, `AstAsset`, atau modul lain yang berubah | `PASS` | `git diff`, `git status --short` |
| `dotnet build` | Dijalankan Rizki 16 November 2026 dan dilaporkan berhasil. Agent memeriksa hasilnya: `bin/Debug/net9.0/QuilvianSystemBackend.dll` (dan salinan `obj/`) bertanggal 2026-11-16 09:07:33 WIB, **sesudah** suntingan source terakhir (08:36:43 WIB); tidak ada berkas `.cs` modul aset yang lebih baru dari DLL; DLL memuat 18 dari 18 penanda — 5 model, 5 configuration, 3 enum, namespace `…Valuation.Models` dan `…Valuation.Enums`, DbSet `AstAccountingOutboxes` dan `AstAssetValueEntries`, serta literal `CK_AstAssetBook_NotBelowResidual`; literal `IX_AstAccountingOutbox_Source` dan `IX_AstAssetBook_AssetId` juga ada | `PASS` | Build yang gagal tidak menerbitkan DLL baru, jadi DLL ini membuktikan 0 error. **Jumlah warning tidak dilaporkan.** Sebelum build, butir ini berstatus `NOT RUN` dan menahan `✅` (DLL saat itu bertanggal 2026-11-13 14:04:31) |
| Validitas model EF saat runtime (check constraint, unik, default) | Belum terbukti | `NOT RUN` | Terbukti paling lambat saat `dotnet ef migrations add` pada `BE-AST-004` |
| Kontrak API di Swagger, skenario `AT-NLI-*` | — | `NOT APPLICABLE` | Tidak ada endpoint |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.
Uji manual: `NOT APPLICABLE` — belum ada perilaku runtime sebelum migration dan endpoint ada.

**Perintah build untuk pemilik** (tanpa migration baru):

```powershell
dotnet build QuilvianSystemBackend.csproj -p:SkipMigrationMetadata=true -p:RunAnalyzers=false -m:1 -nodeReuse:false
```

Build dianggap berhasil bila DLL `bin/Debug/net9.0/QuilvianSystemBackend.dll` terbit dengan cap waktu sesudah
2026-11-16 08:36:43 WIB dan memuat tipe `AstAssetBook` serta `AstAccountingOutboxConfiguration`.

**Tidak dijalankan:** `dotnet ef` apa pun, migration, dan perintah database. `dotnet build` dijalankan Rizki, bukan agent.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Kolom sesuai data dict | Terpenuhi | 65 kolom pada 5 model; pencocokan statis 389/0, termasuk jumlah kolom model = kamus |
| 2. Unik `(LegalEntityId, EffectiveFromPeriod)`, `(AssetGroupId, EffectiveFromPeriod)`, `AssetId` pada buku, `(SourceTransactionId, SourceVersion)` | Terpenuhi | `IX_AstValuationParameter_Entity_Period`, `IX_AstDepreciationPolicy_Group_Period`, `IX_AstAssetBook_AssetId`, `IX_AstAccountingOutbox_Source` — semuanya `IsUnique()` tanpa filter, dan tidak ada index unik lain |
| 3. Check constraint nilai buku ≥ residu | Terpenuhi | `CK_AstAssetBook_NotBelowResidual`, ekspresi persis DDL kamus bagian 5 |
| 4. Tidak membuat migration sendiri — masuk `BE-AST-004` | Terpenuhi | Tidak ada berkas di `Migrations/` yang berubah; `dotnet ef` tidak dijalankan |
| Cakupan: 5 model, 3 enum, configuration di `Repositories/Configurations/Corporate/AssetManagement/Valuation/` | Terpenuhi | Bagian 3.2 |
| `DoD-BE`: `V-BE` dijalankan dan dicatat apa adanya | Terpenuhi | QBE, pencocok, uji mutasi, review diff oleh agent; `dotnet build` oleh Rizki 16 November 2026, hasilnya diperiksa dari DLL (bagian 5). Swagger dan `AT-NLI-*` `NOT APPLICABLE` |
| `DoD-BE`: laporan, roadmap, traceability, `git status --short`; tanpa commit kecuali diminta | Terpenuhi | Laporan ini; bagian 7; tidak di-commit |

Rekap: **4 dari 4 acceptance criteria terpenuhi** dan seluruh butir `DoD-BE` terpenuhi → `✅`.
Riwayat: sebelum build, butir `dotnet build` belum terpenuhi dan status task `🟡`.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada peringatan tool |
| Masalah yang diketahui | **Rekomendasi untuk `BE-AST-015`/`BE-AST-025` (bukan cakupan task ini):** `AstAssetValueEntry` "tidak diubah/dihapus" (arsitektur 5.1) baru dijaga konvensi. Untuk menjaganya, `AstAssetValueEntry` bisa ditambahkan ke penjaga `ApplicationDbContext.StampEpisodeVersions` bersama `AstAssetHistory` (rekomendasi `BE-AST-003`). Keputusannya diambil pada task service |
| Risiko tersisa | (1) Jumlah warning build tidak tercatat. (2) Validitas model EF baru terbukti pada `BE-AST-004`. (3) Sampai `BE-AST-004` selesai, migration lain yang dibuat dari branch `rizkiG` akan ikut membawa 25 tabel aset. (4) Check constraint bergantung pada nama kolom; penggantian nama kolom rupiah buku harus ikut mengubah ekspresinya |
| Perubahan sampingan | `NONE`. Skrip pencocok dan salinan mutasi berada di scratchpad sesi |
| Interupsi | `NONE` |
| Langkah berikutnya | `BE-AST-004` migration fondasi, 25 tabel: periksa snapshot EF lebih dulu, lalu `dotnet ef migrations add` dijalankan Rizki sendiri |

`git status --short` di akhir task:

```text
 M Repositories/ApplicationDbContext.cs
 M docs/module-blueprints/manajemen-aset/nilai/roadmap/backend-roadmap.md
 M docs/module-blueprints/manajemen-aset/nilai/roadmap/requirement-traceability.md
 M docs/module-blueprints/manajemen-aset/register/roadmap/backend-roadmap.md
 M docs/module-blueprints/nurse-view/roadmap/frontend-roadmap.md
 M docs/module-blueprints/nurse-view/roadmap/requirement-traceability.md
 M docs/module-blueprints/nurse-view/task/report/frontend/FE-NV-016.md
?? Areas/Corporate/AssetManagement/Valuation/
?? Repositories/Configurations/Corporate/AssetManagement/Valuation/
?? docs/module-blueprints/manajemen-aset/nilai/task/
```

Ketiga berkas `nurse-view` sudah berubah sebelum task ini dimulai dan **bukan** milik `BE-AST-014`. Tidak ada
yang di-stage atau di-commit.
