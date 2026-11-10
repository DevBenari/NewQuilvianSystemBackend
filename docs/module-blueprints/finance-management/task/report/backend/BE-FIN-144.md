# Laporan Perubahan Backend — `BE-FIN-144`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-144` |
| Judul | Skema database cocok dengan model sebelum aplikasi baru aktif |
| Slice | `MVP-19` — Penampung Data PO/GR dari BLM (`EPIC FIN-26`) |
| Roadmap | `roadmap/01-backend-roadmap.md`, bagian "AMENDMENT ROADMAP REV-24" |
| Trace | `FIN-DES-138`; `FIN-DEC-273`; `FR-FIN-272`, `281` (sisi skema); kamus data 13; `02-backend-architecture.md` W.8, W.9 |
| Contract version | `data/data-dictionary.md` 13 (Revisi 23, approved 2026-11-09) |
| Dependency | `BE-FIN-142` 🟡, `BE-FIN-143` 🟡; wewenang membuat berkas migration diberikan pemilik pada 10 November 2026 |
| Klasifikasi | `MEDIUM` — skor 5: repository 0, berkas diperiksa 2 (>20), berkas diubah 0 (3 berkas, ±99% hasil turunan), logika bisnis 1, kontrak API 0, database 2, keamanan 0, UI 0 |
| Task mode | `BACKEND MODE` |
| Target tulis | `NewQuilvianSystemBackend`, branch `Yasmina`, folder `Migrations/` saja |
| Model | Claude Sonnet 5.5 |
| Commit backend saat dikerjakan | `98a9140c` (working tree memuat perubahan `BE-FIN-142` dan `BE-FIN-143` yang belum di-commit; tidak ditimpa) |
| Tanggal | 10 November 2026 |
| Status | ✅ **Selesai dengan pengecualian (10 November 2026).** Berkas migration, Designer, dan snapshot selesai; build migration, `dotnet build`, dan penerapan migration berhasil pada database yang dipakai pemilik; W.T2, W.T2b, W.T14–W.T17, pemeriksaan objek (Bagian A), dan sisa data uji (Bagian C) lulus — seluruhnya menurut laporan pemilik. **Dikecualikan atas keputusan pemilik, 10 November 2026: "untuk konfig dan perpindahan database nanti saja ... selesaikan statusnya. Tugas migrasi akan dilakukan di lain waktu":** W.T1 (aplikasi), W.T18 (uji `Down`), dan penerapan migration ke lingkungan lain/konfigurasi terkait. Catatan: database yang diperiksa tidak memuat PO/GR lama |

## Backend Governance Preflight

| Butir | Hasil |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `FinanceManagement` / `Purchasing` |
| Pemilik dan prefix (registry) | `Fin`, "Corporate / Finance — FinanceManagement / Purchasing / Pembelian", `ACTIVE` (`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`) |
| Keberlakuan | `NEW CODE` (tabel baru). **Bukan** `LEGACY MIGRATION`: tidak ada rename, `DROP`+`CREATE`, atau perubahan nama tabel |
| QBE yang berlaku | `QBE-MOD-001`..`003`, `QBE-NAM-001`, `002`, `004`. `QBE-DB-001`/`002` tidak berlaku (bukan rename). Aturan `DATABASE_RULES.md`: migration hanya dengan wewenang eksplisit — **diberikan untuk pembuatan berkas, bukan untuk eksekusi** |
| Wewenang | (1) Source: ada. (2) **Pembuatan berkas migration**: diberikan pemilik pada pesan task ini, secara manual, mengikuti snapshot. (3) Eksekusi database: **tidak diberikan**. (4) Deployment: tidak diberikan |
| Branch | `Yasmina` |
| Selisih governance | Sama dengan `BE-FIN-142`/`143` (salinan registry di suite lebih ringkas; salinan repository dipakai) |

---

## 1. Masalah yang Diperbaiki

Model `BE-FIN-142` (kolom baru pada empat tabel PO/GR) dan `BE-FIN-143` (empat tabel baru) hanya ada di source. Tanpa migration, aplikasi baru akan memilih kolom yang belum ada di database sehingga **daftar dan detail PO/GR yang sudah berjalan langsung gagal** (`FIN-DES-138`). Task ini membuat migration yang menjembatani model ke skema, sebagai satu paket dengan snapshot.

---

## 2. Proses Bisnis

Tidak ada proses bisnis yang berubah. Isi migration (`02-backend-architecture.md` W.9):

| Urutan | Langkah (`Up`) | Dampak pada data lama |
| ---: | --- | --- |
| 1 | Hapus check `CK_FinPurchaseOrder_ApprovalTier` lama | Tidak ada |
| 2 | Tambah 23 kolom pada empat tabel: `SourceSystem` NOT NULL bawaan `'QUILVIAN'`; sisanya nullable | Baris lama otomatis `QUILVIAN`; kolom BLM kosong. **Tanpa backfill** |
| 3 | Tambah 12 check (tier diganti + 11 baru) | Seluruh baris lama memenuhi (`QUILVIAN`, kolom BLM kosong, tier `TIER_1`/`TIER_2`) |
| 4 | Buat empat tabel: kotak masuk, pemetaan supplier, konflik, pembatalan eksternal (FK `Restrict`) | Tabel kosong |
| 5 | Buat 22 index (5 pada tabel lama, 17 pada tabel baru) | Index unik terfilter pada kolom yang seluruhnya `NULL` pada data lama |

`Down` membalik urutannya: hapus empat tabel, hapus 5 index, hapus 12 check, hapus 23 kolom, pasang kembali check tier lama.

**Contoh dampak.** PO lama bernomor "PO-2026-0001" sesudah migration: `SourceSystem = QUILVIAN`, `ExternalPurchaseOrderId = NULL`, tier tetap `TIER_1`; daftar dan detail PO tampil seperti sebelumnya.

**Jalur tidak normal.** `Down` memasang kembali check tier `IN ('TIER_1','TIER_2')`; bila sudah ada PO bertier `EXTERNAL`, langkah itu gagal. Mundur hanya aman selama belum ada PO BLM.

---

## 3. Perubahan yang Dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`; `docs/engineering/blocker-rantai-migration.md`; `rules/backend/DATABASE_RULES.md`; `QuilvianSystemBackend.csproj` (aturan pengarsipan Designer); `tooling/migrations/Update-MigrationHistory.ps1` (hanya diperiksa keberadaannya); `Migrations/MigrationMetadata.g.cs` dan `.g.props`; migration terakhir `20261106025752_AddOpticalCreateIdempotencyKey` (`.cs` dan `.Designer.cs`); `20261104044009_AddPatEmployeeBenefitCardSync.cs` (acuan bentuk `CreateTable`); `ApplicationDbContextModelSnapshot.cs`; delapan model dan delapan konfigurasi hasil `BE-FIN-142`/`143`; kamus data 13.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Migrations/20261110090000_AddFinPurchasingBlmStaging.cs` | **Baru** (±46 KB). `Up`: 1 `DropCheckConstraint`, 23 `AddColumn`, 12 `AddCheckConstraint`, 4 `CreateTable`, 22 `CreateIndex`. `Down`: 4 `DropTable`, 5 `DropIndex`, 12 `DropCheckConstraint`, 23 `DropColumn`, 1 `AddCheckConstraint` |
| `Migrations/20261110090000_AddFinPurchasingBlmStaging.Designer.cs` | **Baru** (±5,9 MB). Atribut `[DbContext]` + `[Migration("20261110090000_AddFinPurchasingBlmStaging")]` dan `BuildTargetModel` = isi snapshot baru (identik byte-per-byte pada bagian model) |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | **Diperbarui.** 4 entity PO/GR bertambah properti, index, dan check; 4 entity baru ditambahkan pada tiga bagian snapshot (definisi, relasi). Dibandingkan dengan snapshot sebelumnya memakai `diff --minimal`: **25 penambahan dan 3 penggantian** (dua `ToTable` tanpa check yang kini bercheck, dan satu SQL check tier); tidak ada baris lain yang berubah |

Tidak ada perubahan pada `MigrationMetadata.g.cs`/`.g.props`, folder `History/`, atau `QuilvianSystemBackend.csproj`.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` |
| Database | Skema bertambah (lihat 2). Pemilik melaporkan migration sudah diterapkan dan kolom serta tabel terbarui. Urutan rilis yang disetujui (`FIN-DEC-273`): database dulu, lalu aplikasi baru diaktifkan |
| Keamanan/Auth | `NOT APPLICABLE` |

### 3.4 Cara berkas dibuat

Karena `dotnet ef migrations add` dilarang, berkas dibangkitkan oleh skrip statis di luar repository (scratchpad sesi) yang membaca model, konfigurasi, dan snapshot, lalu menulis tiga berkas di atas. Skrip tidak dijalankan di dalam proyek dan tidak masuk repository. Hal yang dijaga agar hasilnya sama dengan keluaran EF:

| Aspek | Perlakuan | Dasar |
| --- | --- | --- |
| SQL check constraint | Diambil **langsung dari konfigurasi** (bukan diketik ulang); lalu dicocokkan lagi dengan snapshot | Verifikasi: 0 selisih pada 8 entity |
| Nama FK lebih dari 63 karakter | Dipotong 62 karakter + `~` (contoh `FK_FinPurchasingInboundMessage_FinGoodsReceipt_AppliedGoodsRec~`) | 995 contoh nama berakhiran `~` sepanjang tepat 63 pada migration hasil EF di repository ini |
| Index konvensi EF | `Cancellation.InboundMessageId` tidak punya `HasIndex` eksplisit, sehingga EF membuat `IX_FinPurchasingExternalCancellation_InboundMessageId` otomatis; dimasukkan ke migration dan snapshot. FK lain sudah tertutup index eksplisit atau awalan index gabungan | Aturan `ForeignKeyIndexConvention` EF |
| Urutan | Properti snapshot: `Id` lalu alfabetis; index diurut menurut nama kolom; check diurut menurut nama; kolom `CreateTable` mengikuti urutan deklarasi model lalu kolom audit `IdentityModel` | Pola snapshot dan migration yang ada |
| Kolom desimal | `numeric(18,6)` dengan `precision: 18, scale: 6` | Pola migration yang ada |
| Encoding | UTF-8 dengan BOM, akhir baris LF | Berkas sekitar |
| Penamaan | `20261110090000_AddFinPurchasingBlmStaging` (stempel lebih baru daripada migration terakhir `20261106025752`) | Roadmap `BE-FIN-144` |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak menyentuh endpoint.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` / `restore` | Berhasil | `PASS` (laporan pemilik) | Pemilik, 10 November 2026; keluaran perintah `dotnet build` dilampirkan kemudian oleh pemilik (build Release: `0 Error(s)`, `12 Warning(s)` (seluruhnya peringatan paket NU1902/NU1903 `SixLabors.ImageSharp` 3.1.11; tidak ada di folder Purchasing), `Build succeeded`) |
| "Build migration" | Berhasil menurut pemilik; perintah persisnya tidak disebut. Berkas migration, Designer, dan snapshot saya tidak berubah sesudahnya (ukuran sama) dan tidak ada migration tambahan di `Migrations/` | `PASS` (laporan pemilik; rincian tidak diketahui) | Pemilik, 10 November 2026; keluaran perintah tidak dilampirkan ke sesi ini |
| `dotnet ef database update` | Tidak dijalankan | `NOT RUN` | Wewenang eksekusi database belum diberikan |
| QBE preflight / conformance | Lulus | `PASS` | Bacaan registry; tidak ada rename atau `DROP`+`CREATE` |
| Review scope | Hanya tiga berkas di `Migrations/`; berkas milik `BE-FIN-142`/`143` tidak disentuh | `PASS` | `git status --short` |
| Bagian model `Designer` sama dengan snapshot baru | Sama | `PASS` | Perbandingan teks |
| Snapshot lama → baru hanya menambah (kecuali 3 penggantian yang disengaja) | 25 penambahan, 3 penggantian | `PASS` | `diff --minimal` |
| SQL check di snapshot sama dengan konfigurasi (8 entity) | 0 selisih | `PASS` | Skrip pencocokan |
| Spesifikasi kolom/index/FK sama dengan model dan konfigurasi `BE-FIN-142`/`143` | 0 kesalahan | `PASS` | Skrip pencocokan statis |
| Jumlah operasi `Up`/`Down` | Sesuai rencana W.9 (23 / 4 / 22 / 12 / 5) | `PASS` | Hitungan operasi |
| Panjang identitas (index, check, PK, FK) | Semua ≤ 63 karakter | `PASS` | Skrip |
| Keseimbangan `{}` dan `()` pada tiga berkas | Seimbang | `PASS` | Skrip statis (bukan kompilasi) |
| Kompilasi C# | Berhasil | `PASS` (laporan pemilik) | Pemilik, 10 November 2026; keluaran perintah `dotnet build` dilampirkan kemudian oleh pemilik (build Release: `0 Error(s)`, `12 Warning(s)` (seluruhnya peringatan paket NU1902/NU1903 `SixLabors.ImageSharp` 3.1.11; tidak ada di folder Purchasing), `Build succeeded`) |
| Migration diterapkan ke database | Berhasil menurut pemilik: kolom dan tabel terbarui | `PASS` (laporan pemilik) | Pemilik, 10 November 2026; database tujuan dan keluaran perintah tidak dilampirkan ke sesi ini |
| Uji W.T14–W.T17 (34 skenario) | Lulus semua; rincian di 5.1 | `PASS` (laporan pemilik) | Pemilik, 10 November 2026; keluaran Bagian B skrip verifikasi dilampirkan pada percakapan, Bagian A dan C tidak |
| Uji W.T1 (aplikasi: daftar/detail PO/GR lama tanpa galat kolom) | Belum dilaporkan. Dengan database kosong, memuat halaman daftar PO dan GR sudah cukup membuktikan model EF cocok dengan skema (query memilih semua kolom) | `NOT RUN` | Menunggu pemilik |
| Uji W.T2 (A.5 + tambahan W.T2b: data lama utuh) | A.5: semua `cek_nol` = 0, tetapi database memuat 0 PO dan 0 GR (hampa). W.T2b: PO dan GR yang disisipkan **tanpa** menyebut `SourceSystem` terisi `QUILVIAN` (PO bertier `TIER_1`); sisa data uji 0 | `PASS` (laporan pemilik); tidak ada baris lama nyata untuk diperiksa | Pemilik, 10 November 2026; keluaran NOTICE dan tangkapan layar hitungan sisa dilampirkan pada percakapan |
| Uji W.T18 (`Down`) | Sengaja tidak disertakan dalam skrip karena merusak skema; belum diputuskan dijalankan atau dikesampingkan | `NOT RUN` | Menunggu keputusan pemilik |

### 5.1 Hasil uji W.T14–W.T17 pada database (laporan pemilik)

Pemilik menjalankan Bagian B skrip verifikasi (uji tulis yang digulung `ROLLBACK`) pada 10 November 2026 dan melampirkan keluaran NOTICE-nya: **34 dari 34 skenario `LULUS`**, tanpa `GAGAL` atau `TAK-TERDUGA`. Nama database tujuan tidak disebut. Bagian A dan C dilampirkan kemudian sebagai tangkapan layar; hasilnya di 5.2.

| ID | Skenario | Hasil | Constraint/index yang menolak |
|---|---|---|---|
| W.T14a | PO BLM tanpa ExternalPurchaseOrderId | `LULUS` (ditolak) | `CK_FinPurchaseOrder_ExternalIdentity` |
| W.T14b | PO BLM bertier TIER_1 | `LULUS` (ditolak) | `CK_FinPurchaseOrder_ExternalTier` |
| W.T14c | PO BLM sah (ID eksternal + EXTERNAL) | `LULUS` (tersimpan) | — |
| W.T14d | PO QUILVIAN bertier EXTERNAL | `LULUS` (ditolak) | `CK_FinPurchaseOrder_ExternalTier` |
| W.T14e | SourceSystem di luar QUILVIAN/BLM | `LULUS` (ditolak) | `CK_FinPurchaseOrder_SourceSystem` |
| W.T14f | PO lokal tetap dapat dibuat | `LULUS` (tersimpan) | — |
| W.T15a | PO BLM kedua dengan ID eksternal sama | `LULUS` (ditolak) | `IX_FinPurchaseOrder_SourceSystem_ExternalPurchaseOrderId` (unik) |
| W.T15b | Item PO BLM sah (1 BOX = 12 PCS) | `LULUS` (tersimpan) | — |
| W.T15c | Item PO dengan ExternalPurchaseOrderItemId sama | `LULUS` (ditolak) | `IX_FinPurchaseOrderItem_ExternalPurchaseOrderItemId` (unik) |
| W.T15d | GR BLM sah | `LULUS` (tersimpan) | — |
| W.T15e | GR BLM kedua dengan ID eksternal sama | `LULUS` (ditolak) | `IX_FinGoodsReceipt_SourceSystem_ExternalReceiveOrderId` (unik) |
| W.T15f | GR BLM tanpa ExternalReceiveOrderId | `LULUS` (ditolak) | `CK_FinGoodsReceipt_ExternalIdentity` |
| W.T16a | Baris GR BLM 6 PCS GOOD, sah 6 | `LULUS` (tersimpan) | — |
| W.T16b | Baris menunggu periksa, sah 0 | `LULUS` (tersimpan) | — |
| W.T16c | Baris DAMAGED dengan jumlah sah > 0 | `LULUS` (ditolak) | `CK_FinGoodsReceiptItem_AcceptedBaseQuantity` |
| W.T16d | Jumlah sah melebihi jumlah fisik | `LULUS` (ditolak) | `CK_FinGoodsReceiptItem_AcceptedBaseQuantity` |
| W.T16e | Isi per satuan 0 | `LULUS` (ditolak) | `CK_FinGoodsReceiptItem_QuantityPerBaseUnit` |
| W.T16f | Kondisi di luar tiga nilai (`LAINNYA`, sah 6) | `LULUS` (ditolak) | `CK_FinGoodsReceiptItem_AcceptedBaseQuantity` — lihat catatan di bawah tabel |
| W.T16g | Kolom BLM terisi sebagian | `LULUS` (ditolak) | `CK_FinGoodsReceiptItem_BlmFields` |
| W.T16h | Item PO dengan isi per satuan 0 | `LULUS` (ditolak) | `CK_FinPurchaseOrderItem_QuantityPerBaseUnit` |
| W.T16i | Item PO BLM tanpa ExternalProductId (sebagian terisi) | `LULUS` (ditolak) | `CK_FinPurchaseOrderItem_BlmFields` |
| W.T16j | GR/item lokal tanpa kolom BLM | `LULUS` (tersimpan) | — |
| W.T17a | EventId yang sama dua kali | `LULUS` (ditolak) | `IX_FinPurchasingInboundMessage_SourceSystem_EventId` (unik) |
| W.T17b | APPLIED tanpa AppliedGoodsReceiptId | `LULUS` (ditolak) | `CK_FinPurchasingInboundMessage_Applied` |
| W.T17c | Konflik RESOLVED tanpa kode, pelaku, dan waktu | `LULUS` (ditolak) | `CK_FinPurchasingSyncConflict_Resolved` |
| W.T17d | Konflik tanpa PO maupun GR | `LULUS` (ditolak) | `CK_FinPurchasingSyncConflict_Target` |
| W.T17e | Konflik CONVERSION_UNRESOLVED terbuka | `LULUS` (tersimpan) | — |
| W.T17f | Konflik terbuka kedua (kiriman dan jenis sama) | `LULUS` (ditolak) | `IX_FinPurchasingSyncConflict_OpenPerMessageType` (unik) |
| W.T17g | Permintaan batal tanpa RequestedByUserId | `LULUS` (ditolak) | `CK_FinPurchasingExternalCancellation_Requester` |
| W.T17h | CONFIRMED tanpa CancelledAt | `LULUS` (ditolak) | `CK_FinPurchasingExternalCancellation_Confirmed` |
| W.T17i | Permintaan batal sah | `LULUS` (tersimpan) | — |
| W.T17j | Pemetaan supplier aktif pertama | `LULUS` (tersimpan) | — |
| W.T17k | Pemetaan aktif kedua untuk supplier BLM yang sama | `LULUS` (ditolak) | `IX_FinPurchasingSupplierMap_SourceSystem_ExternalSupplierId` (unik) |
| W.T17l | Pemetaan lama yang dinonaktifkan boleh berdampingan | `LULUS` (tersimpan) | — |

**Catatan W.T16f.** Baris dengan kondisi `LAINNYA` dan jumlah sah 6 ditolak oleh `CK_FinGoodsReceiptItem_AcceptedBaseQuantity`, bukan `CK_FinGoodsReceiptItem_ConditionCode`. Itu wajar: PostgreSQL melaporkan check pertama yang gagal menurut urutan nama, dan `…_AcceptedBaseQuantity` mendahului `…_ConditionCode`; baris itu melanggar keduanya. Penolakan terbukti, tetapi check `ConditionCode` belum diuji **sendirian**. Untuk mengisolasinya cukup baris `LAINNYA` dengan jumlah sah 0 (maka check jumlah sah lolos dan hanya check kondisi yang gagal).

### 5.2 Hasil pemeriksaan objek dan sisa data (Bagian A dan C, tangkapan layar pemilik)

Dibaca dari tujuh tangkapan layar DBeaver yang dilampirkan pemilik pada 10 November 2026. Nama panjang terpotong di layar; pencocokan dilakukan dari awalan yang terbaca dan jumlah baris.

| Bagian | Pemeriksaan | Harapan | Hasil |
|---|---|---|---|
| A.1 | Riwayat migration | Satu baris `…_AddFinPurchasingBlmStaging` | `20261110090000_AddFinPurchasingBlmStaging` tercatat (satu baris) |
| A.2 | Empat tabel baru | Semua ada | `FinPurchasingInboundMessage`, `FinPurchasingSupplierMap`, `FinPurchasingSyncConflict`, `FinPurchasingExternalCancellation`: semuanya ada |
| A.3 | 23 kolom baru pada empat tabel lama | Semua ada; `SourceSystem` NOT NULL bawaan `'QUILVIAN'`; kolom BLM nullable | 23 dari 23 ada. `SourceSystem` pada `FinPurchaseOrder` dan `FinGoodsReceipt`: `is_nullable = NO`, bawaan `'QUILVIAN'::character varying`. Kolom lain `YES`/`NULL`. Tipe: `uuid`, `timestamp with time zone`, `numeric`, `character varying` sesuai kamus data |
| A.4 | 18 check dan 10 index | Semua ada | 28 dari 28 ada (dua tangkapan layar saling melengkapi: baris 1–26 dan 4–28) |
| A.4 | Definisi `CK_FinPurchaseOrder_ApprovalTier` | Memuat `TIER_1`, `TIER_2`, `EXTERNAL` | `CHECK (("ApprovalTier")::text = ANY (ARRAY['TIER_1','TIER_2','EXTERNAL']…))` |
| A.5 | Data lama utuh (W.T2) | Seluruh `cek_nol` = 0 | Semua 0. **Namun `info: jumlah PO` = 0 dan `info: jumlah GR` = 0**: tidak ada PO/GR di database itu, jadi hasil ini hampa |
| Tambahan | W.T2b: PO dan GR disisipkan tanpa kolom `SourceSystem` (meniru baris lama) | `SourceSystem` = `QUILVIAN`; hitungan sisa 0 | PO: `QUILVIAN` (tier `TIER_1`); GR: `QUILVIAN`; `PO uji-lama tersisa` = 0 |
| C | Sisa data uji sesudah `ROLLBACK` | 0 | `PO uji tersisa` = 0; `Kotak masuk uji tersisa` = 0 |

**Catatan keluaran W.T2b.** Keluaran memuat pesan `there is already a transaction in progress` dan `there is no transaction in progress`, serta pasangan hasil `LULUS` yang muncul dua kali. Dua pesan itu adalah peringatan PostgreSQL biasa, bukan galat: `BEGIN` dijalankan saat klien (DBeaver, mode commit manual) sudah berada dalam transaksi, dan `ROLLBACK` dijalankan saat tidak ada transaksi terbuka. Hasil ganda menunjukkan skrip dijalankan dua kali. Yang menentukan adalah hitungan sisa 0 sesudahnya, yang berarti tidak ada baris uji yang tersimpan.

Uji manual: `NOT APPLICABLE` (tidak ada database yang disentuh).

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

**Tidak dijalankan oleh agen (build dijalankan pemilik, lihat laporan pemilik di tabel):** semua perintah `dotnet`, serta SQL apa pun ke database.

---

## 6. Acceptance Criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Satu migration aditif tanpa backfill, urutan langkah W.9 | Terpenuhi menurut review diff | Berkas `.cs`; bagian 2 |
| 2. Snapshot konsisten dengan model | Terpenuhi menurut pencocokan statis; pemilik melaporkan build migration berhasil (rincian perintah tidak dilampirkan, sehingga bukan bukti penuh bahwa EF tidak melihat selisih) | Bagian 5 |
| 3. `Down` mengembalikan skema | Terpenuhi menurut review diff (dengan catatan pada baris EXTERNAL) | Berkas `.cs` |
| 4. Diterapkan ke database uji lalu W.T1, W.T2, W.T14–W.T18 lulus | **Terpenuhi sebagian, sisanya dikecualikan pemilik:** diterapkan; W.T2, W.T2b, W.T14–W.T17 lulus. W.T1 dan W.T18 **dikecualikan atas keputusan pemilik** (ditunda) | Bagian 5, 5.1, 5.2 |
| DoD: migration direview dan diterapkan ke database uji | Terpenuhi (direview, diterapkan, hasil uji dicatat). Butir W.T1 dan W.T18 **dikecualikan atas keputusan pemilik** | 5.1, 5.2 |
| DoD: laporan tracked | Terpenuhi | Berkas ini |

Status akhir **✅ dengan pengecualian**: kriteria 1–3 terpenuhi; build, penerapan, W.T2, W.T14–W.T17, dan pemeriksaan objek berhasil menurut pemilik. W.T1, W.T18, dan penerapan ke lingkungan lain dikecualikan atas keputusan pemilik dan **tercatat sebagai pekerjaan ditunda**, bukan dianggap lulus.

---

## 7. Catatan Penutup

| Hal | Isi |
| --- | --- |
| Peringatan | (1) Berkas ini **ditulis dengan skrip, bukan oleh EF**. Sebelum diterapkan, jalankan `dotnet ef migrations has-pending-model-changes` (atau `migrations add` percobaan di cabang sementara lalu bandingkan) oleh pemilik; bila EF melaporkan selisih, perbaiki snapshot. (2) Tinjau SQL-nya dulu dengan `dotnet ef migrations script 20261106025752_AddOpticalCreateIdempotencyKey AddFinPurchasingBlmStaging` sebelum `database update` (oleh pemilik; tidak dijalankan di sini) |
| Temuan di luar scope | **Snapshot di HEAD sudah memuat entity dan kolom yang tidak ada di Designer migration terakhir** (`FinReceivableInstallment`, `FinReceivableInstallmentPlan`, `FinReceivableInvoiceBatch`, `TrxPayrollPayment`, `TrxPayrollRun`, `TrxPayrollVariableInput`, `MstPatientRelationship`, `PatEmployeeBenefitCardSync`; 11 selisih). Penyebab yang masuk akal: Designer migration terakhir dibangkitkan di cabang lain sebelum perubahan itu digabung. Bagian model Designer baru mengikuti snapshot, jadi memuatnya juga. **Tidak diaudit** apakah migration masing-masing sudah ada dan sudah diterapkan di setiap database; itu bukan bagian task ini |
| Pengarsipan Designer | Menurut `QuilvianSystemBackend.csproj`, Designer terbaru tetap dikompilasi utuh dan pemangkasan dilakukan lewat `tooling/migrations/Update-MigrationHistory.ps1`. Skrip itu **tidak dijalankan**. Designer baru ±5,9 MB menambah beban kompilasi; pemangkasan dapat dilakukan pemilik kemudian |
| Rantai migration | `docs/engineering/blocker-rantai-migration.md` (29 September 2026) mencatat rantai repository dan basis data pengembangan pernah berselisih. Keadaan terkini rantai itu **tidak diperiksa**; migration ini hanya aditif di atas keadaan apa pun, dan bergantung pada tabel `FinPurchaseOrder`, `FinPurchaseOrderItem`, `FinGoodsReceipt`, `FinGoodsReceiptItem`, `MstSupplier` sudah ada di database |
| Masalah yang diketahui | (1) Stempel `20261110090000` dipilih manual; migration dari cabang lain yang bertanggal lebih awal tetapi belum diterapkan akan berada di belakangnya. (2) Tidak ada index eksplisit untuk `Cancellation.InboundMessageId`; dihasilkan otomatis oleh konvensi EF (lihat 3.4) |
| Risiko tersisa | (1) Aplikasi belum dibuktikan membaca PO/GR dengan skema baru (W.T1). (2) Jalur `Down` belum pernah dijalankan; ia gagal bila sudah ada PO bertier `EXTERNAL`. (3) Pengisian baris lama yang nyata belum pernah teruji karena database yang diperiksa kosong; bawaan kolom terbukti (W.T2b) dan PostgreSQL mengisi baris lama dengan bawaan yang sama saat `ADD COLUMN ... DEFAULT`, jadi risikonya kecil. (4) Check `ConditionCode` belum diuji sendirian (catatan W.T16f). (5) Database tujuan penerapan tidak disebut |
| Perubahan sampingan | `NONE` |
| Interupsi | Satu perintah pembanding teks berat (`difflib`) melewati batas waktu dan dihentikan; tidak ada berkas repository yang terdampak |
| Status Git | `??` dua berkas migration; `M` `ApplicationDbContextModelSnapshot.cs`; ditambah berkas `BE-FIN-142`/`143` yang belum di-commit. Tidak ada berkas staged |
| Langkah berikutnya | **Pekerjaan ditunda (keputusan pemilik):** (1) W.T1: halaman daftar PO/GR tanpa galat kolom; (2) W.T18: uji `Down` pada database buangan; (3) penerapan migration ke lingkungan lain (staging/produksi) beserta konfigurasi terkait, termasuk skrip SQL manual bila lingkungan itu tidak memakai `dotnet ef database update`; (4) uji runtime `BE-FIN-145`..`147` (W.T5–W.T13, W.T3, W.T4) pada data BLM nyata |
