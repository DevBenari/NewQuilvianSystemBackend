# Laporan Perubahan Backend — `BE-PBJ-PS-004`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-PBJ-PS-004` |
| Judul | Skema Sourcing |
| Slice | `MVP-PS0` — skema (`04-prd-to-mvp.md` bagian 20; roadmap "Pemetaan ke gelombang MVP") |
| Roadmap | `pemilihan-penyedia/roadmap/backend-roadmap.md` revisi 1, kartu `BE-PBJ-PS-004` |
| Trace | `PBJ-DEC-070`, `071`, `074`, `078`, `083`, `090`, `091`, `096`; kamus data pemilihan-penyedia bagian 3–5; arsitektur §3, §6; `AT-PS-60`, `61`, `62` |
| Contract version | Kontrak `PBJ-PS-*` set `1.0` (`approved`, `PBJ-DEC-095`) |
| Dependency | `BE-PBJ-PS-003` ✅ |
| Klasifikasi | `MEDIUM` — skor 7: berkas diperiksa 9–20 (1), berkas diubah > 8 (2), logika sedang (1), database/migration (2), keamanan berkaitan (1) |
| Task mode | `BACKEND` (pemilik menjawab "lanjutkan" atas tawaran `BE-PBJ-PS-004`, 26 November 2026) |
| Wewenang database | Tiga konfirmasi terpisah dari pemilik, 26 November 2026: membuat migration, menerapkannya ke `QuilvianNewDevSukma`, `SELECT` skema dan uji batasan di dalam transaksi yang di-rollback |
| Target tulis | 14 model `Sourcing/Models/`, 14 konfigurasi `Repositories/Configurations/.../Sourcing/`, `ApplicationDbContext.cs`, migration `AddProcurementSourcing` |
| Model | Claude Opus 5.5 |
| Branch / commit | Backend `sukmagp` @ `45b22777` (perubahan `BE-PBJ-PS-001`..`003` dan dokumen blueprint belum di-commit), frontend `3e5386679` |
| Tanggal | 26 November 2026 |
| Status | ✅ **SELESAI.** 14 tabel Sourcing terpasang di `QuilvianNewDevSukma` dan cocok dengan kamus data; 26 uji batasan `PASS` (rollback); QBE Strict `PASS` 32 berkas; model tanpa perubahan tertunda |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `ProcurementManagement` / `Sourcing` — folder submodule baru |
| Registry | `Corporate / Procurement · ProcurementManagement / Sourcing / Pemilihan Penyedia · BUSINESS DOMAIN / MODULE · Prc · ACTIVE` (`PBJ-DEC-096`, change log 2026-11-26), terdaftar **sebelum** model pertama dibuat (`QBE-MOD-002`, `QBE-MOD-003`) |
| Keberlakuan | `NEW CODE` untuk 14 model, 14 konfigurasi, migration; `TOUCHED LEGACY` untuk `ApplicationDbContext.cs` |
| Arketipe endpoint | Tidak ada endpoint (di luar cakupan task) |
| QBE yang berlaku | `QBE-ENT-001`, `QBE-ENT-002`, `QBE-ENT-003`, `QBE-NAM-001`, `QBE-NAM-002` (`Prc`), `QBE-NAM-004`, `QBE-CFG-001`, `QBE-MOD-001..003`, `QBE-CODE-004`, `QBE-DEL-001` |
| Hasil checker | QBE Strict `PASS` (32 berkas, 0 temuan) |

## 1. Masalah yang diselesaikan

Pemilihan penyedia belum punya tempat untuk menyimpan transaksinya: paket, sumber baris PR, checklist, evaluator, RFQ,
undangan vendor, penawaran, evaluasi teknis, negosiasi, dan penetapan. Task ini membangun ke-14 tabel itu beserta penjaga
integritasnya di tingkat basis data. Penjaga itu tetap berlaku walaupun ada dua petugas yang bekerja bersamaan. Contohnya:
satu baris PR tidak bisa masuk dua paket hidup, satu paket hanya punya satu RFQ terbuka dan satu penetapan hidup, harga
negosiasi tidak bisa naik, dan nomor PO BLM tidak bisa dipakai dua paket.

## 2. Proses bisnis yang dilayani skema ini

| Langkah bisnis | Tabel | Penjaga di basis data |
| --- | --- | --- |
| Tim Pengadaan membuat paket dari baris PR | `PrcPackage`, `PrcPackageItem`, `PrcPackageItemSource` | Satu barang katalog per paket; satu baris PR di satu sumber yang belum dilepas; jumlah positif |
| Menyusun persiapan: metode, checklist, evaluator, kriteria | `PrcPackage`, `PrcPackageChecklistItem`, `PrcPackageEvaluator`, `PrcTechnicalCriterion` | Metode di luar rentang wajib alasan; satu jenis dokumen per checklist; satu penugasan aktif per evaluator; nomor butir kriteria unik |
| Mengirim RFQ ke vendor layak | `PrcRfq`, `PrcRfqInvitation` | Satu RFQ `DRAFT`/`SENT` per paket; nomor RFQ unik; RFQ gagal wajib alasan; vendor Dibatasi wajib alasan |
| Mencatat penawaran | `PrcQuotation`, `PrcQuotationLine` | Satu penawaran `RECORDED` per vendor per RFQ; PPN 0–100; batal wajib alasan; harga positif, harga terkini ≤ harga awal |
| Evaluasi teknis dan negosiasi | `PrcTechnicalEvaluationResult`, `PrcNegotiation`, `PrcNegotiationLine` | Satu hasil per baris × kriteria × evaluator; `FAIL` wajib alasan; putaran unik; harga negosiasi tidak naik |
| Penetapan penyedia dan nomor PO | `PrcPackageAward`, `PrcPackage` | Satu penetapan hidup per paket; di atas HPS, bukan terendah, vendor Dibatasi, dicabut, dan tidak dapat diterapkan — semuanya wajib alasan; nomor paket dan nomor PO unik |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kamus data pemilihan-penyedia bagian 3–6, arsitektur §3–§6, `PrcPurchaseRequest`/`PrcPurchaseRequestConfiguration`
(pola `RowVersion`), `PrcVendorStatusRequestConfiguration` (pola index unik berfilter status), `PrcDocumentHistoryConfiguration`
(pola `jsonb`), `MstSupplier`, `PrcPackagePlan`, `AccessExplicitPermissionAttribute` dan `ProcurementRequestExplicitPermissions.cs`
(syarat penanda izin), `ApplicationDbContext.cs`, registry modul.

### 3.2 Berkas yang berubah

| Berkas | Status | Isi |
| --- | --- | --- |
| `Areas/Corporate/ProcurementManagement/Sourcing/Models/` (14 berkas) | Baru | `PrcPackage`, `PrcPackageItem`, `PrcPackageItemSource`, `PrcPackageChecklistItem`, `PrcPackageEvaluator`, `PrcRfq`, `PrcRfqInvitation`, `PrcQuotation`, `PrcQuotationLine`, `PrcTechnicalCriterion`, `PrcTechnicalEvaluationResult`, `PrcNegotiation`, `PrcNegotiationLine`, `PrcPackageAward` |
| `Repositories/Configurations/Corporate/ProcurementManagement/Sourcing/` (14 berkas) | Baru | Konfigurasi EF: 16 index unik (parsial), 28 CHECK, 33 FK `RESTRICT`, `RowVersion` sebagai token konkurensi pada paket, RFQ, penawaran, penetapan |
| `Repositories/ApplicationDbContext.cs` | Diperbarui | 14 `DbSet` + `using ...Sourcing.Models` |
| `Migrations/20261126091428_AddProcurementSourcing.cs` + `.Designer.cs` | Baru | 14 `CreateTable`, 48 `CreateIndex`; `Down` 14 `DropTable` |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Diperbarui | Hasil generator EF |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — tidak ada endpoint |
| Database | 14 tabel baru di schema `public`; tidak ada tabel lama yang berubah. Aman tanpa henti layanan karena hanya menambah tabel kosong. Langkah mundur: `Down` menghapus ke-14 tabel; hanya dijalankan sebelum ada data produksi. **Diterapkan** ke `QuilvianNewDevSukma` 26 November 2026; lingkungan lain belum |
| Keamanan/Auth | Kolom harga ditandai SENSITIF di XML doc model (kamus data penanda `S`). Penyaringan harga di DTO dibangun task berikutnya. Penanda izin `ReadAllUnits`/`ReadPrice` dipindah ke `BE-PBJ-PS-005` (D-2) |

**Selisih terhadap desain (delta), dicatat apa adanya.**

| # | Desain | Yang dibangun | Alasan |
| ---: | --- | --- | --- |
| D-1 | Kamus data: `PrcTechnicalCriterion.SortOrder int` | `CriterionNo int` (nomor butir 1, 2, 3, …), CHECK ≥ 1, unik per item paket | Kontrak engineering melarang `SortOrder` presentasi tersimpan untuk kode baru; urutan memakai field semantik (preseden `BE-PBJ-PS-001` D-1). Kamus data perlu diselaraskan |
| D-2 | Kartu roadmap: `ProcurementSourcingExplicitPermissions.cs` (`ReadAllUnits`, `ReadPrice`) di task ini | **Tidak dibuat; dipindah ke `BE-PBJ-PS-005`** | `AccessExplicitPermissionAttribute` mewajibkan `ResourceName` sudah terdaftar dari pemindaian endpoint; penanda ke resource tanpa endpoint "ditolak keras saat registry disusun", sehingga aplikasi gagal start. Resource `ProcurementPackage` baru punya endpoint di `BE-PBJ-PS-005` |
| D-3 | Kartu roadmap: pendaftaran DI | Tidak berubah | Belum ada service; pendaftaran masuk task yang membuat service-nya (preseden `BE-PBJ-VD-001` D-5) |
| D-4 | Kamus data: aturan "wajib bila …" dan rentang nilai di kolom Validasi | Diwujudkan sebagai 28 CHECK: daftar status (paket, RFQ, penawaran, penetapan, hasil evaluasi), alasan wajib (metode di luar rentang, RFQ gagal, Dibatasi, batal penawaran, `FAIL`, di atas HPS, bukan terendah, cabut, tidak dapat diterapkan), jumlah/harga positif, PPN 0–100, harga terkini ≤ awal, harga negosiasi ≤ sebelumnya, hitungan ≥ 0/1 | Turunan langsung aturan kamus data (preseden `BE-PBJ-VD-001` D-4); service tetap memvalidasi lebih dulu dengan kode `PBJ-VAL` yang benar |
| D-5 | Kamus data: `PrcQuotation.SupplierId` hanya disebut dalam index unik | Ditambah FK `MstSupplier` (`RESTRICT`) | Konsisten dengan `PrcRfqInvitation` dan `PrcPackageAward`; mencegah penawaran untuk vendor fiktif |
| D-6 | — | Index unik tambahan: `PrcTechnicalCriterion (PackageItemId, CriterionNo)`, `PrcNegotiationLine (NegotiationId, QuotationLineId)` | Satu nomor butir per item dan satu harga baru per baris per putaran |
| D-7 | — | Nama index unik parsial ditulis eksplisit dan ≤ 63 karakter, misalnya `IX_PrcPackageItemSource_LiveRequestLine`, `IX_PrcPackageAward_LivePerPackage` | Pelajaran `BE-PBJ-PS-001` D-10: nama panjang dipotong PostgreSQL dan membuat model ≠ basis data |
| D-8 | Kamus data: bawaan `DRAFT`/`RECORDED`/`PENDING_APPROVAL`/`true`/`1` | Diisi model, bukan default basis data | Pola konfigurasi Pengadaan (`BE-PBJ-PS-001` D-7) |
| D-9 | — | `EvaluatorUserId`, `RecordedByUserId`, `PicUserId`, `AssignedByUserId`, `DepartmentId`, `FundSourceId`, `ItemCategoryId` tanpa FK fisik, hanya index bila kamus data menyebut index | Sesuai kamus data (salinan/kunci), pola `PrcPurchaseRequest` |

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — tidak ada endpoint pada task ini (kartu task, "Di luar cakupan").

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -c Release` | 0 error, 620 warning; **0** dari berkas baru/berubah | `PASS` | Log build disaring per berkas |
| `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path <32 berkas>` | VIOLATION 0, REVIEW 0, INFO 0; `PASS` (termasuk `QBE-MOD-002` untuk folder `Sourcing`) | `PASS` | Laporan QBE |
| `dotnet ef migrations list` | 0 `(Pending)` | `PASS` | Keluaran perintah |
| `dotnet ef migrations add AddProcurementSourcing` | 14 `CreateTable`, 48 `CreateIndex`, 14 `DropTable`; tanpa `AddColumn`/`AddForeignKey`/`AlterColumn` (tanpa drift); 0 kolom FK bayangan; 0 nama index > 63 karakter | `PASS` | Pemeriksaan berkas migration |
| `dotnet ef database update AddProcurementSourcing` | "Applying migration '20261126091428_AddProcurementSourcing'. Done." | `PASS` | Keluaran perintah |
| `dotnet ef migrations has-pending-model-changes` | "No changes have been made to the model since the last migration." | `PASS` | Keluaran perintah |
| Skema terhadap kamus data (bagian 5.1) | 14 tabel cocok | `PASS` | Bagian 5.1 |
| Uji batasan (bagian 5.2) | 26 / 26 | `PASS` | Bagian 5.2 |
| Start aplikasi | Tidak ada permukaan runtime baru; model EF sudah tervalidasi oleh `migrations add`/`has-pending-model-changes` | `NOT RUN` | — |
| `dotnet test` | Tidak ada project test | `NOT RUN` | — |

Uji manual: `NOT APPLICABLE` (tidak ada endpoint).

### 5.1 Skema terhadap kamus data (`AT-PS-62`)

Diperiksa dari `information_schema.columns`, `pg_indexes`, dan `pg_constraint`. Kolom bisnis = di luar sepuluh kolom
`IdentityModel`.

| Tabel | Kolom bisnis | Unik (parsial) | CHECK | FK (semua `RESTRICT`) | Cocok |
| --- | ---: | --- | ---: | ---: | :---: |
| `PrcPackage` | 29 | `PackageNumber`, `PurchaseOrderNumber` bila terisi | 4 | 4 | Ya |
| `PrcPackageItem` | 9 | (`PackageId`, `ItemId`) | 2 | 2 | Ya |
| `PrcPackageItemSource` | 9 | `PurchaseRequestLineId` bila `IsReleased = false` | 1 | 3 | Ya |
| `PrcPackageChecklistItem` | 6 | (`PackageId`, `DocumentTypeId`) | 0 | 2 | Ya |
| `PrcPackageEvaluator` | 5 | (`PackageId`, `EvaluatorUserId`) bila aktif | 0 | 1 | Ya |
| `PrcRfq` | 11 | `RfqNumber` bila terisi; `PackageId` bila `DRAFT`/`SENT` | 3 | 2 | Ya |
| `PrcRfqInvitation` | 5 | (`RfqId`, `SupplierId`) | 1 | 2 | Ya |
| `PrcQuotation` | 16 | (`RfqId`, `SupplierId`) bila `RECORDED` | 3 | 4 | Ya (D-5) |
| `PrcQuotationLine` | 8 | (`QuotationId`, `PackageItemId`) | 2 | 3 | Ya |
| `PrcTechnicalCriterion` | 4 | (`PackageItemId`, `CriterionNo`) | 1 | 1 | Ya (D-1, D-6) |
| `PrcTechnicalEvaluationResult` | 8 | (`QuotationLineId`, `CriterionId`, `EvaluatorUserId`) | 2 | 3 | Ya |
| `PrcNegotiation` | 6 | (`QuotationId`, `RoundNo`) | 1 | 1 | Ya |
| `PrcNegotiationLine` | 5 | (`NegotiationId`, `QuotationLineId`) | 1 | 2 | Ya (D-6) |
| `PrcPackageAward` | 22 | `PackageId` bila `PENDING_APPROVAL`/`RETURNED`/`APPROVED` | 7 | 3 | Ya |

Semua index unik juga berlaku hanya untuk `IsDelete = false`. Angka uang `numeric(18,2)`, jumlah `numeric(18,3)`, PPN
`numeric(5,2)`, `RankingSnapshotJson` `jsonb`.

### 5.2 Uji batasan di dalam transaksi yang di-rollback

Satu blok `DO` dijalankan di antara `BEGIN` dan `ROLLBACK`, dan setiap kasus memakai sub-transaksi. Sesudahnya `PrcPackage`,
`PrcPackageAward`, dan `PrcQuotation` kembali berisi 0 baris.

| No | Skenario | Diharapkan | Hasil |
| ---: | --- | --- | --- |
| 1 | Status paket `LAIN` | Ditolak CHECK | `PASS` |
| 2 | Metode di luar rentang tanpa alasan | Ditolak CHECK | `PASS` |
| 3 | Dua paket bernomor sama (dua paket tanpa nomor diterima) | Ditolak unik | `PASS` |
| 4 | Dua paket bernomor PO sama | Ditolak unik | `PASS` |
| 5 | Barang yang sama dua kali di satu paket | Ditolak unik | `PASS` |
| 6 | Jumlah item 0 | Ditolak CHECK | `PASS` |
| 7 | Baris PR yang sama di dua sumber hidup (sumber yang sudah dilepas tidak menghalangi) | Ditolak unik | `PASS` |
| 8 | Dua RFQ terbuka untuk satu paket | Ditolak unik | `PASS` |
| 9 | RFQ `FAILED` tanpa alasan | Ditolak CHECK | `PASS` |
| 10 | RFQ `FAILED` beralasan di samping RFQ terbuka | Diterima | `PASS` |
| 11 | Dua penawaran `RECORDED` vendor sama di satu RFQ | Ditolak unik | `PASS` |
| 12 | PPN 101% | Ditolak CHECK | `PASS` |
| 13 | Penawaran `VOID` tanpa alasan | Ditolak CHECK | `PASS` |
| 14 | Harga terkini 2.600.000 > harga awal 2.500.000 | Ditolak CHECK | `PASS` |
| 15 | Hasil `FAIL` tanpa alasan | Ditolak CHECK | `PASS` |
| 16 | Dua hasil untuk baris × kriteria × evaluator yang sama | Ditolak unik | `PASS` |
| 17 | Harga negosiasi naik 2.500.000 → 2.550.000 | Ditolak CHECK | `PASS` |
| 18 | Dua penetapan hidup untuk satu paket | Ditolak unik | `PASS` |
| 19 | Penetapan `REJECTED` di samping penetapan hidup | Diterima | `PASS` |
| 20 | Penetapan Rp 44.000.000 di atas HPS Rp 42.750.000 tanpa alasan | Ditolak CHECK | `PASS` |
| 21 | Bukan terendah tanpa alasan | Ditolak CHECK | `PASS` |
| 22 | `REVOKED` tanpa alasan | Ditolak CHECK | `PASS` |
| 23 | Dua penugasan aktif evaluator sama (yang nonaktif tidak menghalangi) | Ditolak unik | `PASS` |
| 24 | Jenis dokumen yang sama dua kali di checklist | Ditolak unik | `PASS` |
| 25 | Jenis pengadaan fiktif | Ditolak FK | `PASS` |
| 26 | Hapus fisik paket yang berisi item | Ditolak FK `RESTRICT` | `PASS` |

26 skenario, 26 `PASS`.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-PS-60` registry `Sourcing` `ACTIVE` sebelum model pertama; QBE `QBE-MOD-002` lolos untuk folder `Sourcing` | Terpenuhi | Baris registry 2026-11-26 (`PBJ-DEC-096`); QBE Strict `PASS` 32 berkas |
| `AT-PS-61` build | Terpenuhi untuk backend | Bagian 5 |
| `AT-PS-62` 14 tabel cocok kamus data dan `has-pending-model-changes` bersih | Terpenuhi, dengan delta D-1, D-5, D-6 | Bagian 5.1; keluaran perintah |
| Uji batasan index unik dan CHECK dengan rollback | Terpenuhi | Bagian 5.2 |
| Cakupan kartu: penanda izin eksplisit dan DI | **Dipindah**, bukan terlewat | D-2 → `BE-PBJ-PS-005`; D-3 → task pembuat service |
| Laporan | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tabel masih kosong; semua aturan bisnis dengan kode `PBJ-VAL` tetap tugas service (`BE-PBJ-PS-005` dst.). CHECK di basis data adalah jaring pengaman terakhir, bukan pengganti validasi service |
| Masalah yang diketahui | Kamus data masih memuat `SortOrder` pada `PrcTechnicalCriterion` (D-1) dan belum menyebut FK `PrcQuotation.SupplierId` (D-5); perlu diselaraskan pemilik desain. `BE-PBJ-PS-005` wajib membuat `ProcurementSourcingExplicitPermissions.cs` bersama controller paket (D-2) |
| Risiko tersisa | `Down` migration menghapus tabel beserta isinya; jangan dijalankan setelah ada data produksi |
| Perubahan sampingan | `NONE` — generator EF hanya menambah migration baru dan memperbarui snapshot untuk 14 tabel ini |
| Interupsi | Satu kali penulisan `ApplicationDbContext.cs` lewat skrip gagal (`OSError: [Errno 22]`, berkas terkunci sesaat); berkas diperiksa utuh, lalu diubah ulang lewat Edit |
| Status Git | Belum di-commit. Baru: 14 model, 14 konfigurasi, migration + Designer, laporan ini. Diubah: `ApplicationDbContext.cs`, `ApplicationDbContextModelSnapshot.cs`, roadmap, traceability, `MODULE-STATUS.md`. Ditambah perubahan `BE-PBJ-PS-001`..`003` dan dokumen blueprint yang belum di-commit |
| Langkah berikutnya | `BE-PBJ-PS-005` (paket draf + tiga metode baris PR + penanda izin eksplisit) — gelombang 4; `FE-PBJ-PS-001` boleh paralel |
