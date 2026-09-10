# Farmasi — LEGACY MIGRATION `Trx*` → `Phm*` — Execution Plan Final

Status: **MENUNGGU PERSETUJUAN**. Dokumen ini adalah rencana; belum ada source yang diubah,
belum ada migration yang dibuat, belum ada database yang disentuh.

Dasar: QBE-NAM-001 (prefix `Trx*` tidak sah untuk kode baru), QBE-NAM-003 (LEGACY MIGRATION
menormalkan source dan tabel fisik bersama-sama, dalam satu campaign), QBE-DB-002 (dilarang
DROP+CREATE bila rename aman).

Preseden yang dipakai utuh: `Migrations/20260831000000_RenameMedicalRecordTrxTablesToMrcPrefix.cs`
(campaign `Trx*` → `Mrc*` milik Medical Record, sudah berjalan di database ini).

---

## 0. Ruang lingkup

**15 tabel, 118 baris.** Semua milik `Areas/HealthServices/PharmacyManagement`.

| # | Nama lama | Nama baru | Baris | Constraint | Index |
|---|---|---|---|---|---|
| 1 | `TrxStockRequestHistory` | `PhmStockRequestHistory` | 41 | 2 | 3 |
| 2 | `TrxStockRequestItem` | `PhmStockRequestItem` | 20 | 4 | 4 |
| 3 | `TrxStockRequest` | `PhmStockRequest` | 16 | 4 | 6 |
| 4 | `TrxDrugStockMutation` | `PhmDrugStockMutation` | 12 | 5 | 7 |
| 5 | `TrxStockTransferHistory` | `PhmStockTransferHistory` | 9 | 2 | 3 |
| 6 | `TrxDrugStockBalance` | `PhmDrugStockBalance` | 5 | 6 | 4 |
| 7 | `TrxStockTransferAllocation` | `PhmStockTransferAllocation` | 3 | 3 | 3 |
| 8 | `TrxDrugReturnHistory` | `PhmDrugReturnHistory` | 3 | 2 | 3 |
| 9 | `TrxStockTransfer` | `PhmStockTransfer` | 2 | 4 | 5 |
| 10 | `TrxStockTransferItem` | `PhmStockTransferItem` | 2 | 3 | 3 |
| 11 | `TrxDrugReturn` | `PhmDrugReturn` | 1 | 6 | 9 |
| 12 | `TrxDrugReturnItem` | `PhmDrugReturnItem` | 1 | 5 | 5 |
| 13 | `TrxDrugUsage` | `PhmDrugUsage` | 1 | 4 | 7 |
| 14 | `TrxDrugUsageItem` | `PhmDrugUsageItem` | 1 | 4 | 5 |
| 15 | `TrxDrugUsageAllocation` | `PhmDrugUsageAllocation` | 1 | 3 | 3 |

**Di luar ruang lingkup, dengan alasan:**

- `MstDrugBatch` — lihat keputusan di bagian 13.
- `MstOperatingRoomStockSource` → `OprStockSource` — campaign LEGACY MIGRATION tersendiri milik
  modul Operasi, belum direncanakan maupun diverifikasi (registry, catatan 2026-09-08).
- `PhmPrescriptionCopy` / `PhmPrescriptionCopyItem` — sudah lahir dengan prefix benar.

---

## 1. Urutan migration

**Satu migration tunggal, bukan lima belas.**

Nama: `RenamePharmacyTrxTablesToPhmPrefix`.

Alasan satu migration: rename tabel di PostgreSQL bersifat DDL transaksional. Satu migration =
satu transaksi = satu titik commit. Lima belas migration berarti lima belas titik di mana
`dotnet ef database update` bisa berhenti dan meninggalkan database setengah rename. Preseden
Medical Record juga memakai satu migration untuk sembilan tabel sekaligus. Lihat bagian 12.

Migration ini **hanya** berisi rename. Tidak ada perubahan kolom, tidak ada perubahan tipe,
tidak ada data seeding, tidak ada index baru. Apa pun perubahan skema lain menunggu migration
berikutnya yang terpisah.

---

## 2. Urutan rename source

Urutan ini penting karena kompilasi harus tetap mungkin diverifikasi di tiap langkah.
**29 file** terdampak (di luar `Migrations/`).

| Langkah | Yang diubah | File |
|---|---|---|
| S1 | Class entity + property navigasi di dalamnya | `Areas/.../PharmacyManagement/Models/` — `TrxDrugReturn.cs`, `TrxDrugStock.cs`, `TrxDrugUsage.cs`, `TrxStockRequest.cs`, `TrxStockTransfer.cs` (5 file, 15 class; 10 class bersarang di file yang tidak senama) |
| S2 | Nama file model mengikuti class utamanya | `TrxDrugReturn.cs`→`PhmDrugReturn.cs`, dst. (5 rename file) |
| S3 | `IEntityTypeConfiguration<T>` + argumen `ToTable(...)` | `Repositories/Configurations/HealthServices/PharmacyManagement/` — `DrugReturnConfigurations.cs`, `DrugStockConfigurations.cs`, `DrugUsageConfigurations.cs`, `StockTransferConfigurations.cs`, `TrxStockRequestConfigurations.cs` (5 file, 15 literal `ToTable`). File terakhir ikut di-rename menjadi `StockRequestConfigurations.cs` |
| S4 | `DbSet<T>` dan nama propertinya | `Repositories/ApplicationDbContext.cs` baris 659–678 (15 DbSet) |
| S5 | Service Farmasi | `Areas/.../PharmacyManagement/Services/` (7 file) |
| S6 | Service lintas modul | `Areas/.../OperatingRoomManagement/Services/OperatingRoomInventoryDispatchService.cs` (1 file) |
| S7 | Test | `Tests/.../PharmacyManagement/` (6 file) + `Tests/.../OperatingRoomManagement/` (2 file) |

Nama DbSet mengikuti bentuk jamak yang sudah ada: `TrxStockRequests` → `PhmStockRequests`.

Verifikasi setelah S7, sebelum menyentuh database:

```
dotnet build -p:SkipMigrationMetadata=true
grep -rnE 'Trx(Drug|Stock)' --include='*.cs' . | grep -v /Migrations/ | grep -vE '^\./(obj|bin)/'
```

Baris kedua harus kosong. Migration lama **tidak** diubah — migration adalah catatan sejarah
apa yang pernah dijalankan, dan mengubahnya membuat riwayat berbohong.

---

## 3. Urutan rename tabel

**Tidak ada urutan yang perlu dipatuhi.** Ini bukan pengabaian, melainkan konsekuensi dari cara
`ALTER TABLE ... RENAME TO` bekerja di PostgreSQL: foreign key mengikat OID tabel, bukan namanya.
Rename tabel induk tidak pernah memutus FK dari anaknya, ke arah mana pun urutannya.

Urutan yang dipakai tetap daun → akar, semata agar dapat dibaca dan dicocokkan manusia:

1. `TrxStockRequestHistory`, `TrxStockRequestItem` → `TrxStockRequest`
2. `TrxStockTransferAllocation` → `TrxStockTransferItem`, `TrxStockTransferHistory` → `TrxStockTransfer`
3. `TrxDrugReturnHistory`, `TrxDrugReturnItem` → `TrxDrugReturn`
4. `TrxDrugUsageAllocation` → `TrxDrugUsageItem` → `TrxDrugUsage`
5. `TrxDrugStockMutation`, `TrxDrugStockBalance`

---

## 4. Penanganan FK, index, dan constraint

Mengikuti persis skrip preseden Medical Record: satu blok PL/pgSQL `DO` yang berjalan di atas
katalog sistem, dalam tiga langkah per entri peta.

1. **Tabel.** `ALTER TABLE public.%I RENAME TO %I`
2. **Constraint.** Setiap constraint yang *namanya memuat* nama tabel lama, di tabel mana pun:
   `WHERE c.conname LIKE '%' || lama || '%'`. Ini menangkap PK, unique, check, **dan FK dari
   tabel lain yang menyebut nama tabel ini di namanya**. Rename constraint PK/unique otomatis
   ikut me-rename index penopangnya, jadi tidak perlu ditangani dua kali.
3. **Index sisa.** Index yang tidak ditopang constraint mana pun, di-rename terpisah.

Skrip berbasis katalog, bukan daftar nama constraint yang ditulis tangan. Konsekuensinya: skrip
ini **idempoten** dan tidak bisa gagal karena satu nama constraint meleset dari perkiraan.

**Tidak ada DROP dan CREATE di mana pun** (QBE-DB-002). FK tidak pernah dilepas, jadi tidak
pernah ada jendela waktu di mana integritas referensial tidak dijaga.

**Tidak ada nama yang perlu dirapikan.** Preseden Medical Record membutuhkan langkah keempat
(`RapikanMaju`/`RapikanBalik`) karena `Trx`→`Mrc` mengubah panjang sebagian nama sehingga EF
memotongnya di titik lain. Di sini `Trx` dan `Phm` sama-sama tiga huruf, sehingga panjang setiap
nama tidak berubah dan titik potong 63-karakter milik EF jatuh persis di tempat yang sama. Sudah
diperiksa: 12 objek berada di 61–63 karakter, 9 di antaranya tepat 63 (sudah dipotong EF);
seluruhnya tetap 63 setelah substitusi. **Langkah keempat tidak diperlukan.**

Objek yang diverifikasi tidak ada dan karenanya tidak perlu ditangani: view, function, trigger,
rule, sequence bernama, default berbasis ekspresi, dan raw SQL di source yang menyebut
tabel-tabel ini.

---

## 5. Dependency antar entity

Seluruh **15 FK masuk** ke tabel-tabel ini berasal dari dalam kelompok 15 tabel itu sendiri,
ditambah FK ke `MstDrugBatch` dan master lain yang **arahnya keluar** (tidak terpengaruh rename).

**Nol FK dari modul lain masuk ke tabel-tabel ini.** Inilah alasan campaign ini dapat berdiri
sendiri tanpa mengoordinasikan modul lain.

---

## 6. Dependency Operasi

Satu file source: `OperatingRoomInventoryDispatchService`, menyentuh `TrxDrugStockBalance` dan
`TrxDrugStockMutation` lewat EF (bukan SQL mentah). Ikut di-rename pada langkah **S6**, dalam
commit yang sama.

`OprMaterialUsage` **tidak** punya FK database ke tabel Farmasi mana pun — integrasinya lewat
outbox dan pemanggilan service, bukan foreign key. Jadi rename ini tidak menyentuh skema Operasi.

Kolom `TrxDrugReturn.SourceOprMaterialUsageId` ada dan **0 baris terisi**.

---

## 7. Dependency Prescription Dispensing

`PrescriptionDispensingService` menulis ke `TrxDrugUsage` / `TrxDrugUsageItem` /
`TrxDrugUsageAllocation`. Ketiganya di-rename; service ikut pada **S5**.

Kolom `TrxDrugUsage.PrescriptionId` dan `TrxDrugUsageItem.PrescriptionItemId` adalah `Guid?`
tanpa FK database, dan **0 baris terisi**.

Dispensing adalah checkpoint yang tidak boleh berubah perilakunya. Rename ini **tidak mengubah
satu pun**: bukan kolom, bukan tipe, bukan constraint, bukan urutan operasi, bukan status enum.
Yang berubah hanya nama tabel dan nama class. Bukti yang menutup: `PrescriptionDispensingTests`
dan `OperatingRoomToPharmacyEndToEndTests` harus hijau setelah rename tanpa satu pun assertion
diubah — hanya nama tipe yang menyesuaikan.

---

## 8. Dependency Copy Resep

`PhmPrescriptionCopy` dan `PhmPrescriptionCopyItem` sudah memakai prefix benar dan **tidak ikut
di-rename**. Keduanya menyimpan snapshot dan tidak punya FK ke 15 tabel di atas. Copy Resep
membaca hasil dispensing lewat service, bukan lewat join lintas tabel.

Dampak: **nihil**. UI Copy Resep sedang ditahan (`PHA-DEC-061`) dan rename ini tidak menambah
blocker apa pun padanya.

---

## 9. Dependency Return

`TrxDrugReturn` / `TrxDrugReturnItem` / `TrxDrugReturnHistory` semuanya masuk ruang lingkup dan
di-rename bersama. Kolom `TrxDrugReturn.SourceDrugUsageId` adalah `Guid?` tanpa FK database, dan
**0 baris terisi**.

Alur retur yang harus tetap utuh setelah rename: retur → verifikasi apoteker → penerimaan
sebagian → sisanya ke lokasi karantina. Terlindungi oleh test E2E yang sudah ada
(100 vial → keluar 10 → retur 4 → diterima 3 → saldo 93).

---

## 10. Verifikasi row count

Dijalankan **sebelum** dan **sesudah** migration, pada database yang sama, lalu dibandingkan baris
per baris. Nilai "sebelum" yang diharapkan sudah tercatat di tabel bagian 0 (total 118).

```sql
-- SEBELUM (nama lama)
SELECT 'StockRequestHistory' t, count(*) FROM public."TrxStockRequestHistory"
UNION ALL SELECT 'StockRequestItem', count(*) FROM public."TrxStockRequestItem"
UNION ALL SELECT 'StockRequest', count(*) FROM public."TrxStockRequest"
UNION ALL SELECT 'DrugStockMutation', count(*) FROM public."TrxDrugStockMutation"
UNION ALL SELECT 'StockTransferHistory', count(*) FROM public."TrxStockTransferHistory"
UNION ALL SELECT 'DrugStockBalance', count(*) FROM public."TrxDrugStockBalance"
UNION ALL SELECT 'StockTransferAllocation', count(*) FROM public."TrxStockTransferAllocation"
UNION ALL SELECT 'DrugReturnHistory', count(*) FROM public."TrxDrugReturnHistory"
UNION ALL SELECT 'StockTransfer', count(*) FROM public."TrxStockTransfer"
UNION ALL SELECT 'StockTransferItem', count(*) FROM public."TrxStockTransferItem"
UNION ALL SELECT 'DrugReturn', count(*) FROM public."TrxDrugReturn"
UNION ALL SELECT 'DrugReturnItem', count(*) FROM public."TrxDrugReturnItem"
UNION ALL SELECT 'DrugUsage', count(*) FROM public."TrxDrugUsage"
UNION ALL SELECT 'DrugUsageItem', count(*) FROM public."TrxDrugUsageItem"
UNION ALL SELECT 'DrugUsageAllocation', count(*) FROM public."TrxDrugUsageAllocation"
ORDER BY 1;
-- SESUDAH: skrip identik dengan Trx diganti Phm. Hasil harus sama persis, total 118.
```

Rename tabel secara teknis tidak dapat mengubah isi. Perbandingan ini bukan untuk membuktikan
data selamat, melainkan untuk membuktikan bahwa yang berpindah nama memang tabel yang sama —
bukan tabel baru yang kosong. Selisih berapa pun = **rollback**.

Pemeriksaan bahwa tidak ada tabel kembar tertinggal:

```sql
SELECT relname FROM pg_class c JOIN pg_namespace s ON s.oid = c.relnamespace
WHERE s.nspname = 'public' AND c.relkind = 'r'
  AND (relname LIKE 'TrxDrug%' OR relname LIKE 'TrxStock%');
-- Harus 0 baris.
```

---

## 11. Verifikasi FK

Jumlah FK harus **tetap sama** sebelum dan sesudah — 15 constraint FK masuk, ditambah FK keluar
ke master. Yang berubah hanya namanya.

```sql
-- Jumlah FK yang menyentuh kelompok tabel ini (jalankan dgn 'Trx' sebelum, 'Phm' sesudah)
SELECT count(*) FROM pg_constraint c
JOIN pg_class t ON t.oid = c.conrelid JOIN pg_class r ON r.oid = c.confrelid
WHERE c.contype = 'f' AND (t.relname LIKE 'Phm%' OR r.relname LIKE 'Phm%');

-- Tidak ada FK yang jadi tidak valid
SELECT conname FROM pg_constraint WHERE contype = 'f' AND NOT convalidated;
-- Harus 0 baris.

-- Tidak ada nama objek yang masih memuat Trx pada kelompok Farmasi
SELECT conname FROM pg_constraint WHERE conname ~ '(TrxDrug|TrxStock)'
UNION ALL
SELECT relname FROM pg_class c JOIN pg_namespace s ON s.oid = c.relnamespace
WHERE s.nspname = 'public' AND c.relkind = 'i' AND relname ~ '(TrxDrug|TrxStock)';
-- Harus 0 baris. Sebelum migration: 127 baris.
```

Pembuktian integritas referensial masih ditegakkan sungguhan (bukan sekadar terdaftar):

```sql
BEGIN;
ALTER TABLE public."PhmDrugStockMutation" VALIDATE CONSTRAINT
  "FK_PhmDrugStockMutation_MstDrugBatch_DrugBatchId";
ROLLBACK;
```

Sesudah migration, `dotnet ef migrations add ProbeSetelahRename -p:SkipMigrationMetadata=true`
harus menghasilkan migration **kosong**. Itu bukti model EF dan database benar-benar sinkron.
File probe dihapus dengan `rm`, **bukan** `dotnet ef migrations remove` — perintah itu pernah
mengembalikan snapshot ke Designer basi dan membuat `migrations add` berikutnya menghasilkan
570 `CreateTable` yang merusak.

---

## 12. Menjamin database tidak pernah setengah rename

Ini persyaratan terpenting dalam dokumen ini, dan dijamin oleh empat lapis:

1. **DDL transaksional.** PostgreSQL — tidak seperti MySQL atau Oracle — menjalankan
   `ALTER TABLE ... RENAME` di dalam transaksi. EF Core membungkus satu migration dalam satu
   transaksi. Gagal di tabel ke-9 berarti seluruh 15 rename ikut batal. Tidak ada kondisi antara
   yang bertahan.
2. **Satu migration.** Lihat bagian 1. Tidak ada titik commit di tengah campaign.
3. **Skrip idempoten.** Blok `DO` berjalan atas katalog: entri yang tabel lamanya sudah tidak ada
   dilewati diam-diam. Menjalankan ulang skrip tidak merusak dan tidak menggandakan.
4. **Larangan target migration.** `dotnet ef database update` dijalankan **tanpa argumen target**.
   Menyebut migration target membuat EF *me-revert* seluruh migration setelahnya — pada sesi
   sebelumnya perilaku ini menghapus 32 tabel di database ini dan memaksa `pg_restore`. Perintah
   yang sah hanya: `dotnet ef database update -p:SkipMigrationMetadata=true`.

---

## 13. Keputusan: `MstDrugBatch`

**Keputusan: tetap `MstDrugBatch`. Tidak di-rename. Ownership dicatat sebagai Farmasi.**

Bukti:

- File-nya **sudah** berada di `Areas/HealthServices/PharmacyManagement/Models/MstDrugBatch.cs` —
  bukan di `Areas/HealthServices/MasterData/`. Secara source, ownership Farmasi sudah menjadi
  kenyataan; tidak ada yang perlu dipindahkan.
- Seluruh 5 FK masuk berasal dari tabel Farmasi. Satu-satunya pembaca lintas modul adalah
  `OperatingRoomInventoryDispatchService`, dan itu membaca, bukan memiliki.
- **Tidak ada aturan yang dilanggar.** Yang di-deprecate QBE-NAM-001 adalah `Trx*`, bukan `Mst*`.
  `BACKEND_ENGINEERING_CONTRACT.md` baris 48 menyatakan eksplisit: "`Mst*` adalah master/reference
  dan tidak deprecated." Batch obat memang data induk — nomor lot, kedaluwarsa, sumber — bukan
  transaksi.
- Preseden langsung: Laboratorium menetapkan `MstLabRejectionReason` diperlakukan legacy dan tidak
  dinamai ulang meski modulnya memakai prefix `Lab` (registry, 2026-09-02). Bank Darah memperoleh
  perlakuan sama pada 2026-09-07.

Menamainya `PhmDrugBatch` berarti me-rename tabel yang tidak melanggar apa pun, menambah 3 baris
data dan 5 FK ke dalam ruang lingkup migration, tanpa menutup satu pun temuan QBE. Biaya ada,
manfaat tidak.

Yang perlu dilakukan hanya administratif: mencatat di registry bahwa data induk Farmasi memakai
`Mst` dan `MstDrugBatch` berada di bawah kepemilikan `PharmacyManagement` — mengikuti bentuk
catatan Laboratorium. **Tidak termasuk pekerjaan ini dan menunggu persetujuan terpisah.**

---

## 14. Backup

Wajib, tepat sebelum migration, pada database yang akan diubah:

```bash
pg_dump -h localhost -U postgres -d QuilvianNewDevIkbal -Fc \
  -f "backup-sebelum-rename-phm-$(date +%Y%m%d-%H%M).dump"
```

Format custom (`-Fc`), bukan SQL polos, karena hanya format itu yang mendukung
`pg_restore --data-only --disable-triggers` — jalur yang sudah terbukti memulihkan database ini
pada insiden 32 tabel. Backup diverifikasi terbaca sebelum migration dijalankan:

```bash
pg_restore -l backup-sebelum-rename-phm-*.dump | grep -c 'TABLE DATA'
```

Backup yang belum pernah dibuka bukan backup.

---

## 15. Rollback

Tiga tingkat, dipilih menurut sejauh mana migration sempat berjalan:

| Situasi | Tindakan |
|---|---|
| Migration gagal di tengah | **Tidak ada tindakan.** Transaksi sudah rollback sendiri (bagian 12). Database masih memakai nama lama seutuhnya. Perbaiki penyebab, jalankan ulang. |
| Migration sukses, verifikasi bagian 10/11 gagal, atau aplikasi bermasalah | Jalankan `Down`. Metode `Down` berisi `Skrip(PetaBalik)` — peta yang sama dengan pasangan lama/baru ditukar, dan sama idempotennya. Source dikembalikan dengan `git revert` pada commit campaign. |
| Kerusakan di luar dugaan | `pg_restore` dari dump bagian 14. |

`Down` **wajib diuji di dev sebelum `Up` dijalankan di mana pun selain dev**: jalankan `Up`,
verifikasi, jalankan `Down`, pastikan 127 objek bernama `Trx*` kembali persis dan row count tetap
118, lalu jalankan `Up` lagi. Migration rename yang `Down`-nya belum pernah dijalankan bukan
migration yang punya rollback.

---

## 16. Cara deployment

Berurutan, berhenti pada kegagalan mana pun:

| # | Langkah | Kriteria lulus |
|---|---|---|
| 1 | Cabang dari `Ikbal`, kerja di branch campaign terpisah | — |
| 2 | Rename source S1–S7 (bagian 2) | `dotnet build -p:SkipMigrationMetadata=true` sukses; grep `Trx(Drug\|Stock)` kosong |
| 3 | Jalankan seluruh test unit + E2E | Hijau, tanpa assertion yang diubah |
| 4 | `dotnet ef migrations add RenamePharmacyTrxTablesToPhmPrefix -p:SkipMigrationMetadata=true` | Terbentuk; isinya **diganti tangan** dengan skrip PL/pgSQL berbasis peta (bagian 4). EF akan mengusulkan DROP+CREATE — itu ditolak QBE-DB-002 |
| 5 | Baca ulang migration hasil edit dari awal sampai akhir | Tidak ada satu pun `DROP` atau `CreateTable` |
| 6 | Row count + object count **sebelum** (bagian 10, 11) | Tercatat: 118 baris, 127 objek `Trx*` |
| 7 | Backup (bagian 14) | Dump terverifikasi terbaca |
| 8 | `dotnet ef database update -p:SkipMigrationMetadata=true` — **tanpa argumen target** | Sukses |
| 9 | Verifikasi bagian 10 dan 11 | 118 baris; 0 objek `Trx*`; 0 FK tidak valid |
| 10 | Uji `Down` lalu `Up` lagi (bagian 15) | Kembali dan maju keduanya bersih |
| 11 | Jalankan aplikasi; asap-uji Dispensing, Retur, Transfer, Stock Request, dan pemakaian material Operasi | Berfungsi |
| 12 | `Invoke-QbeConformanceCheck.ps1 -Mode Strict` | 63 → **3** pelanggaran |
| 13 | Commit tunggal berisi source + migration, lalu laporkan | Menunggu instruksi push |

Deployment ke database selain dev pemilik, dan push, adalah wewenang terpisah dan **tidak
termasuk** rencana ini.

---

## 17. Hasil QBE yang diharapkan

**63 → 3.** Ketiga sisanya seluruhnya berasal dari satu false positive yang sudah diverifikasi,
bukan dari kode yang salah.

`OperatingRoomDemoSeeder` adalah `public static class` — tidak dapat mewarisi apa pun, tidak punya
`[Table]`, dan tidak punya `DbSet`. Ia tertangkap semata karena `Test-PersistedEntity` di
`tooling/qbe/Invoke-QbeConformanceCheck.ps1` mencari `DbSet<` di mana saja dalam isi file; baris
770 kebetulan memuat parameter `DbSet<TEntity> set,` dan baris 788 memuat
`if (created is IdentityModel audited)`.

Ini **bug tooling**, dicatat sebagai temuan tersendiri. Source tidak diubah untuk membuat QBE
hijau, dan tidak ada exception yang dibuat untuknya.

---

## Yang secara eksplisit tidak dilakukan rencana ini

- Membuat migration — menunggu persetujuan atas dokumen ini
- Mengubah database
- Mengubah `QBE_EXCEPTIONS.json`
- Mengubah source apa pun demi membuat QBE hijau
- Me-rename `MstDrugBatch` (bagian 13) atau `MstOperatingRoomStockSource` (campaign terpisah)
- Push
