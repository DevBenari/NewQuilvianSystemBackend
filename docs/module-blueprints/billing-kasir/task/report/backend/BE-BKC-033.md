# Laporan Perubahan Backend — `BE-BKC-033`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-033` |
| Judul | Fondasi skema Petty Cash: lima tabel, migration, dan data induk awal |
| Slice | `MVP-13` (fondasi) — Amendment 7 September 2026 (kedua), rumpun Petty Cash |
| Roadmap | `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` § Amendment 7 September 2026 (kedua) |
| Trace | `PC-DEC-001`, `PC-DEC-002`, `PC-DEC-010`, `PC-DEC-012`; `PC-DES-001`, `PC-DES-003`, `PC-DES-004`, `PC-DES-010`, `PC-DES-014`; `CAP-29` (Missing) |
| Contract version | `BIL-API-0.9`, `BIL-STATE-0.8`, `BIL-VALIDATION-0.8` — isi terkunci, approval `PC-DES-001`–`014` lewat `PC-DEC-015`. Task ini **tidak** menyentuh endpoint |
| Dependency | `PC-OQ-003` (prasyarat registry) — **terpenuhi**, lihat bagian 3.4 |
| Klasifikasi | `HEAVY` — skor 9 (repository 0, berkas diperiksa 2, berkas diubah 2, logika bisnis 1, kontrak API 0, database 2, keamanan 0, UI/workflow 2) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi + laporan tracked ini |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `dd31bc9` (branch `Yasmina`) |
| Tanggal | 7 September 2026 |
| Status | 🟡 **SEBAGIAN — diperbarui 7 September 2026 (verifikasi ulang).** Source model, configuration, dan `DbSet` ditulis. **Build kini LULUS** (`0 Error(s)`, diverifikasi ulang pada sesi ini) dan **migration `20260907062238_AddTablePettyCashModule` kini ADA** — keduanya dijalankan pemilik modul di luar sesi, lalu diperiksa langsung di sini. **Yang masih menahan `✅`:** eksekusi seed ke database tidak dapat diverifikasi dari sesi ini (tidak ada akses DB), review Finance atas lima kategori seed belum diminta, dan tiga ambiguitas kontrak belum diratifikasi. Lihat bagian 5 dan 6 |

---

## 1. Masalah yang diperbaiki

Rumah sakit mengeluarkan uang tunai kecil setiap hari — ongkos transport kurir, konsumsi rapat,
alat tulis, perbaikan kecil. Sampai hari ini pengeluaran itu **tidak tercatat di sistem sama
sekali**. Yang ada hanya kertas dan catatan manual, sehingga tidak ada seorang pun yang dapat
menjawab dua pertanyaan paling dasar: **berapa sisa uang kas kecil sekarang**, dan **siapa yang
menyetujui pengeluaran ini**.

> **Contoh nyata.** Seorang kurir menerima Rp 150.000 untuk ongkos kirim dokumen. Uangnya
> diambil dari laci, dicatat di buku tulis, dan notanya diselipkan di map. Ketika akhir bulan
> Finance menghitung, selisih Rp 150.000 itu tidak dapat ditelusuri siapa yang menyetujui,
> kapan, dan untuk keperluan apa.

Task ini **belum** menyelesaikan masalah tersebut. Ia hanya meletakkan **fondasi skema**-nya:
lima tabel tempat data itu kelak disimpan. Alur kerjanya sendiri dikerjakan `BE-BKC-035` sampai
`BE-BKC-038`.

---

## 2. Proses bisnis

Task ini **tidak menghasilkan perilaku yang dapat dipakai pengguna**. Tidak ada layar, tidak ada
endpoint, dan tidak ada tombol yang berubah. Yang dibuat adalah bentuk tabelnya saja.

Proses bisnis yang **kelak** ditopang kelima tabel ini, sebagai konteks:

1. Kasir/petugas administrasi membuat voucher: nama penerima, kategori, nominal, keperluan.
   Status awal `WAITING_APPROVAL` (`Menunggu Persetujuan`).
2. Kepala Kasir menyetujui atau menolak. Saat menyetujui, saldo **belum** berkurang — nominalnya
   hanya **dikomitmenkan** (`PC-DES-005`).
3. Kasir menyerahkan uang. Di titik inilah — dan hanya di sini — saldo kolam berkurang
   (`PC-DEC-009`), disertai satu baris ledger.
4. Penerima menyerahkan nota. Voucher menjadi `COMPLETED` (`Selesai`).

**Jalur tidak normal** yang sudah disiapkan bentuk tabelnya: pembatalan oleh pemohon memakai
penandaan `IsCancel` warisan `IdentityModel`, **bukan** status keenam (`PC-DES-007`); dan voucher
`REJECTED` bersifat terminal serta tidak memiliki jalur pengubah sama sekali (`PC-DES-013`).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

**Dokumen kontrak dan keputusan:**

| Berkas | Yang diambil darinya |
| --- | --- |
| `roadmap/backend-roadmap.md` § Amendment 7 September 2026 (kedua) | Kartu task `BE-BKC-033` — outcome, scope, dependency, acceptance, DoD |
| `02-backend-architecture.md` baris 1732–2241 | `PC-DES-001`–`014`, penjelasan kelima class, arsitektur folder target, rencana migration, rencana data master awal |
| `data/data-dictionary.md` baris 243–528 | Kontrak kolom, tipe, wajib/opsional, bawaan, index beserta filternya, DDL rujukan, penandaan kolom sensitif |
| `roadmap/README.md`, `04-prd-to-mvp.md` | Posisi gelombang `MVP-13` dan status `PC-OQ-003` |

**Aturan tata kelola canonical:**

`AGENTS.md`; `rules/GLOBAL_RULES.md`; `rules/backend/TASK_RULES.md`; `rules/backend/TASK_CLASSIFICATION.md`;
`rules/backend/DATABASE_RULES.md`; `rules/backend/REVIEW_RULES.md`; `rules/backend/REPORT_TEMPLATE.md`;
`rules/backend/engineering/BACKEND_ENGINEERING_CONTRACT.md`;
`rules/backend/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `rules/rule-output/**`.

**Source pembanding terdekat (bukti pola, bukan wewenang):**

| Berkas | Pola yang diambil |
| --- | --- |
| `Models/IdentityModel.cs` | Sepuluh kolom audit warisan, termasuk `IsCancel`/`CancelDateTime`/`CancelBy` yang dipakai `PC-DES-007` |
| `.../Cashier/Models/BilCashierShiftCommand.cs` | Bentuk tabel jejak perintah beserta kelas konstanta `CommandTypes` |
| `.../Cashier/Models/BilCashierShift.cs` | `RowVersion` bertipe `Guid` beserta kelas konstanta `Statuses` |
| `.../MasterData/Models/MstTaxRule.cs` | Bentuk master data berkode bisnis yang diisi pengguna |
| `.../Cashier/BilCashierShiftCommandConfiguration.cs` | Gaya configuration: `ToTable`, presisi desimal, `timestamp with time zone`, index parsial |
| `.../MasterData/MstTaxRuleConfiguration.cs` | Unique index berfilter `IsDelete = false`, check constraint |
| `.../MasterData/MstAdministrationFeePolicyConfiguration.cs` | **Pola seed `HasData`** beserta Guid deterministik dan waktu tetap |
| `Repositories/ApplicationDbContext.cs` | Penempatan `DbSet` dan `ApplyConfigurationsFromAssembly` |

### 3.2 Berkas yang berubah

Sepuluh berkas **baru** dan satu berkas **diubah**. Seluruhnya hasil task ini.

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BillingManagement/MasterData/Models/MstPettyCashCategory.cs` | **Baru**, 24 baris. Master data kategori pengeluaran: `CategoryCode`, `CategoryName`, `Description`, `IsActive` |
| `Areas/HealthServices/BillingManagement/PettyCash/Models/BilPettyCashVoucher.cs` | **Baru**, 75 baris. Aggregate root voucher, 20 kolom bisnis + kelas `PettyCashVoucherStatuses` (5 nilai) |
| `Areas/HealthServices/BillingManagement/PettyCash/Models/BilPettyCashVoucherCommand.cs` | **Baru**, 63 baris. Jejak perintah append-only + kelas `PettyCashVoucherCommandTypes` (7 nilai) |
| `Areas/HealthServices/BillingManagement/PettyCash/Models/BilPettyCashBudget.cs` | **Baru**, 41 baris. Kolam anggaran + kelas `PettyCashBudgetStatuses` (2 nilai) |
| `Areas/HealthServices/BillingManagement/PettyCash/Models/BilPettyCashBudgetMovement.cs` | **Baru**, 54 baris. Ledger pergerakan + kelas `PettyCashBudgetMovementTypes` (3 nilai) |
| `Repositories/Configurations/HealthServices/BillingManagement/MasterData/MstPettyCashCategoryConfiguration.cs` | **Baru**, 57 baris. Mapping, unique index `CategoryCode`, **seed lima kategori** |
| `Repositories/Configurations/HealthServices/BillingManagement/PettyCash/BilPettyCashVoucherConfiguration.cs` | **Baru**, 66 baris. Mapping, 5 index, FK ke `MstPettyCashCategory` (`Restrict`), 3 check constraint |
| `Repositories/Configurations/HealthServices/BillingManagement/PettyCash/BilPettyCashVoucherCommandConfiguration.cs` | **Baru**, 51 baris. Mapping, 2 index, FK ke voucher (`Restrict`), 2 check constraint |
| `Repositories/Configurations/HealthServices/BillingManagement/PettyCash/BilPettyCashBudgetConfiguration.cs` | **Baru**, 72 baris. Mapping, 2 unique index, 2 check constraint, **seed satu kolam `HOSPITAL_MAIN`** |
| `Repositories/Configurations/HealthServices/BillingManagement/PettyCash/BilPettyCashBudgetMovementConfiguration.cs` | **Baru**, 63 baris. Mapping, 3 index, 2 FK (`Restrict`), 5 check constraint |
| `Repositories/ApplicationDbContext.cs` | **Diubah**, +8 baris: satu `using` dan lima `DbSet` |

Diff `ApplicationDbContext.cs` selengkapnya:

```diff
+using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Models;
...
+        // Petty Cash (Kas Kecil) — BE-BKC-033, PC-DES-001. Kolam anggaran terpisah
+        // dari kas fisik shift kasir (PC-DEC-001); tidak ada relasi ke BilCashierShift.
+        public DbSet<MstPettyCashCategory> MstPettyCashCategories { get; set; }
+        public DbSet<BilPettyCashBudget> BilPettyCashBudgets { get; set; }
+        public DbSet<BilPettyCashBudgetMovement> BilPettyCashBudgetMovements { get; set; }
+        public DbSet<BilPettyCashVoucher> BilPettyCashVouchers { get; set; }
+        public DbSet<BilPettyCashVoucherCommand> BilPettyCashVoucherCommands { get; set; }
```

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — task ini tidak membuat, mengubah, maupun menghapus satu pun endpoint. Controller dan DTO Petty Cash adalah lingkup `BE-BKC-035`–`037` |
| Database | **Lima tabel baru** beserta index, FK, dan check constraint-nya sudah dinyatakan di source. **Migration BELUM dibuat dan database BELUM disentuh** — lihat bagian 5 dan 7 |
| Keamanan/Auth | `NOT APPLICABLE` untuk permukaan endpoint — tidak ada action baru, sehingga `role-access-rules.md` belum berlaku pada task ini. Yang sudah diterapkan: lima kolom sensitif (`RecipientName`, `Purpose`, `RejectionReason`, `Reason`, `ResponseJson`) ditandai komentar `SENSITIF` pada model agar `BE-BKC-038` dapat menyaringnya dari log |

### 3.4 Backend Governance Preflight

| Field | Hasil |
| --- | --- |
| Area | `HealthServices` |
| Module | `BillingManagement` |
| Submodule | `PettyCash/` (baru) dan `MasterData/` (sudah ada) |
| Pemilik/prefix registry | Baris registry `HealthServices \| BillingManagement / Billing \| BUSINESS DOMAIN / MODULE \| Bil \| ACTIVE`; dan `Administrator / HealthServices \| Master / Reference \| MASTER / REFERENCE \| Mst \| ACTIVE` |
| Status registry | **APPROVED dan `ACTIVE`** untuk kedua prefix. Tidak ada prefix baru yang diajukan |
| Keberlakuan | `NEW CODE` |
| QBE ID yang diterapkan | `QBE-ENT-001`, `QBE-ENT-002`, `QBE-ENT-003`, `QBE-NAM-001`, `QBE-NAM-002`, `QBE-NAM-004`, `QBE-CFG-001`, `QBE-MOD-001`, `QBE-MOD-002`, `QBE-MOD-003`, `QBE-CODE-004`, `QBE-AUD-001` |
| QBE ID yang **belum** berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-DTO-001`, `QBE-PAGE-001`, `QBE-OPT-001`, `QBE-VAL-001`, `QBE-CODE-001`/`002`/`003` — seluruhnya menyangkut service dan endpoint, yang bukan lingkup task ini |

**Penutupan `PC-OQ-003` — tidak `BLOCKED BY QBE-MOD-002`.**

`PC-OQ-003` menanyakan apakah baris registry `BillingManagement / Billing` sudah dianggap
mencakup submodule `PettyCash/`. Jawabannya **ya**, dan buktinya diperiksa langsung pada
repository, bukan disimpulkan dari nama folder:

1. **Pemiliknya sudah terdaftar.** Prosedur pendaftaran pada registry langkah 2 berbunyi: "Bila
   pemiliknya sudah ada di tabel, tidak ada prefix baru: pakai prefix yang tercatat." Pemilik
   capability ini adalah `BillingManagement`, yang sudah terdaftar `ACTIVE` dengan prefix `Bil`.
2. **Preseden di dalam modul yang sama sudah ada dua.** `Areas/HealthServices/BillingManagement/`
   hari ini memuat empat submodule — `Billing/`, `Cashier/`, `MasterData/`, dan `Operational/` —
   dan **tidak satu pun** dari `Cashier/` maupun `Operational/` tercatat sebagai baris registry
   tersendiri. Keduanya memuat model persisted berprefix `Bil`: `BilCashierShift`,
   `BilCashierShiftCommand`, `BilCashVarianceReview`, `BilCashierShiftHandover` pada `Cashier/`;
   `BilFolio`, `BilChargeLine`, `BilChargeComponent`, `BilProcessingEffect` pada `Operational/`.
   `PettyCash/` adalah submodule kelima dengan pola yang persis sama.
3. **Tidak ada prefix baru yang diciptakan.** Empat tabel operasional memakai `Bil` dan satu
   tabel master memakai `Mst` — keduanya prefix terdaftar berstatus `ACTIVE`. `QBE-NAM-004`
   (larangan mengarang prefix) tidak tersentuh.

Karena itu `QBE-MOD-002` **tidak** memblokir task ini. Yang tetap disarankan sebagai kerapian
tata kelola — dan **bukan** blocker — adalah menambahkan nama folder `PettyCash` ke kolom
Module/pemilik pada baris registry itu, sebagaimana `Cashier` dan `Operational` juga belum
tercatat di sana. Itu perubahan pada berkas milik suite skill, di luar wewenang tulis task ini.

### 3.5 Keputusan implementasi dan selisih terhadap dokumen

| Hal | Yang dilakukan | Alasan |
| --- | --- | --- |
| Registrasi configuration | **Tidak** ditambahkan satu per satu | `ApplicationDbContext.cs` baris 753 memakai `builder.ApplyConfigurationsFromAssembly(...)`, sehingga kelima configuration ditemukan otomatis. Blueprint menulis "5 `DbSet` + 5 registrasi configuration"; **source yang berlaku** menurut `GLOBAL_RULES.md` bagian 3, dan selisihnya dilaporkan di sini |
| `ResponseJson` | Dipetakan `text` dengan bawaan `'{}'` | `data-dictionary.md` DDL menetapkan `text NOT NULL DEFAULT '{}'`. Preseden `BilCashierShiftCommandConfiguration` memakai `jsonb`. Acceptance task berbunyi "sesuai `data/data-dictionary.md`", sehingga kamus data yang diikuti. **Selisih terhadap preseden ini perlu diratifikasi pemilik arsitektur** |
| `IX_BilPettyCashBudget_ActiveSingleton` | Unique index parsial pada kolom `Status` berfilter `"Status" = 'ACTIVE' AND "IsDelete" = false` | Kamus data menyebut "unique pada ekspresi konstan", yang tidak dapat dinyatakan lewat `IEntityTypeConfiguration`. Bentuk yang dipakai **setara secara semantik**: dua baris aktif sama-sama bernilai `ACTIVE`, sehingga baris kedua ditolak. Menegakkan `PC-DES-014` tanpa menuntut SQL mentah di migration |
| Index yang dibuat | Hanya index yang **disebut namanya** pada tabel **Index:** kamus data | Kolom `RequestedBy`, `SubmittedAt` (voucher), `ActorUserId`, `CorrelationId` (command), `MovementType`, `ActorUserId`, `OccurredAt` (movement), `CategoryName`, `IsActive` (kategori) ditandai "Index" pada tabel kolom, tetapi **tidak muncul** pada daftar index bernama. Kedua daftar itu tidak konsisten satu sama lain. Daftar bernama yang diikuti karena ia lengkap dengan nama, filter, dan kegunaan. **Dicatat sebagai ambiguitas kontrak yang perlu diratifikasi**, bukan diputuskan sepihak |
| Check constraint | Ditambahkan 12 buah lintas empat tabel | Mengikuti preseden `MstTaxRuleConfiguration` dan `MstAdministrationFeePolicyConfiguration` di modul yang sama. Menegakkan invariant kamus data di level database: `Amount > 0`, saldo tidak negatif, kosakata status/tipe, `RejectionReason` wajib tepat pada `REJECTED`, `VoucherId` terisi hanya untuk `DISBURSEMENT`, `Reason` wajib untuk `REJECT`/`CANCEL`/`TOP_UP`/`ADJUSTMENT` |
| Seed | Ditulis sebagai `HasData` pada configuration | Preseden `MstAdministrationFeePolicyConfiguration` di folder yang sama. Guid dan waktu **deterministik** agar scaffolding migration tidak menghasilkan diff berbeda tiap kali dijalankan |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak menyentuh satu pun endpoint. Kartu task menyatakan eksplisit
"**tidak ada endpoint** pada task ini". Sebelas endpoint Petty Cash adalah lingkup `BE-BKC-035`
(9 endpoint master data), `BE-BKC-036` (4 endpoint anggaran), dan `BE-BKC-037` (10 endpoint
voucher).

---

## 5. Verifikasi

### 5.1 Verifikasi awal (7 September 2026, sesi implementasi) — historis

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Dihentikan atas instruksi pemilik modul sebelum perintah dieksekusi |
| `dotnet ef migrations add` | Tidak dijalankan | `NOT RUN` | Wewenang terpisah; tidak diberikan |
| `dotnet ef database update` | Tidak dijalankan | `NOT RUN` | Wewenang terpisah; tidak diberikan |
| Review struktural migration | Tidak dijalankan | `NOT RUN` | Tidak ada berkas migration untuk direview |
| Query hitung baris seed pada database lokal | Tidak dijalankan | `NOT RUN` | Database tidak disentuh sama sekali |

Uji manual: `NOT FEASIBLE` — tidak ada permukaan yang dapat dicoba pengguna pada task ini.

### 5.2 Verifikasi ulang (7 September 2026, sesi `BE-BKC-034`)

Pemilik modul menjalankan `dotnet ef migrations add` dan `dotnet build` sendiri di luar sesi dan
melaporkan keduanya berhasil. Klaim itu **tidak diterima begitu saja** — diperiksa ulang langsung
pada repository dan dengan menjalankan compiler sendiri:

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` (dijalankan ulang pada sesi ini) | **LULUS** | `PASS` | `186 Warning(s)`, **`0 Error(s)`**, `Time Elapsed 00:04:08.85`. Tidak ada satu pun warning yang berasal dari berkas Petty Cash |
| Berkas migration ada di `Migrations/` | **ADA** | `PASS` | `Migrations/20260907062238_AddTablePettyCashModule.cs` (25.559 byte) beserta `.Designer.cs`; `ApplicationDbContextModelSnapshot.cs` ikut diperbarui. Ketiganya bertanggal setelah implementasi `BE-BKC-033` |
| Kelima tabel dibuat migration | **LENGKAP** | `PASS` | `CreateTable` untuk `BilPettyCashBudget`, `MstPettyCashCategory`, `BilPettyCashVoucher`, `BilPettyCashBudgetMovement`, `BilPettyCashVoucherCommand` — kelimanya ada, dan `Down()` menghapus kelimanya |
| Urutan `CREATE TABLE` aman terhadap FK | **AMAN** | `PASS` | `MstPettyCashCategory` dibuat sebelum `BilPettyCashVoucher` yang ber-FK ke sana; `BilPettyCashVoucher` sebelum `BilPettyCashVoucherCommand` dan `BilPettyCashBudgetMovement`. **Selisih terhadap blueprint:** EF menempatkan `BilPettyCashBudget` di urutan pertama, bukan `MstPettyCashCategory` — tidak berbahaya karena tidak ada FK di antara keduanya |
| 13 index bernama ikut ter-scaffold | **LENGKAP** | `PASS` | `CreateIndex` menghasilkan 13 index bernama, termasuk `IX_BilPettyCashBudget_ActiveSingleton` dan `IX_MstPettyCashCategory_CategoryCode` |
| Seed ter-scaffold sebagai `InsertData` | **LENGKAP** | `PASS` | Satu baris `BilPettyCashBudget` (`HOSPITAL_MAIN`, `Kas Kecil Rumah Sakit`, `ACTIVE`) dan lima baris `MstPettyCashCategory` (`TRANSPORT`, `OPERASIONAL`, `KONSUMSI`, `MAINTENANCE`, `ATK`), Guid dan waktu deterministik sesuai rancangan |
| Baris seed benar-benar ada di database | Tidak dapat diverifikasi | `NOT RUN` | Sesi ini **tidak memiliki akses database**. `dotnet ef database update` dilaporkan pemilik modul telah dijalankan, tetapi tidak ada query hitung baris yang dapat dijalankan dari sini untuk membuktikannya |
| Review Finance atas lima kategori seed | Belum dilakukan | `NOT RUN` | Di luar kendali builder — butir DoD yang menuntut aksi organisasi |

**Yang berubah dan yang tidak.**

> **Kompilasi kini TERBUKTI.** `dotnet build` dijalankan sendiri pada sesi ini dan menghasilkan
> `0 Error(s)`. Peringatan "kode ini belum pernah dikompilasi" pada versi awal laporan ini
> **dicabut**.
>
> **Migration kini ADA dan strukturnya sudah direview.** Kelima `CREATE TABLE`, 13 index, dan
> seluruh `InsertData` seed diperiksa satu per satu, dan urutan FK-nya aman.
>
> **Eksekusi seed ke database tetap BELUM TERVERIFIKASI dari sesi ini.** Pemilik modul melaporkan
> `database update` berhasil, tetapi laporan ini **tidak** menyatakan baris seed sudah ada di
> database — tidak ada bukti query yang dapat dijalankan tanpa akses DB. Inilah, bersama review
> Finance, alasan status tetap 🟡 dan bukan ✅.

---

## 6. Acceptance criteria dan Definition of Done

### Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Kelima tabel sesuai `data/data-dictionary.md` | **Terpenuhi** | Kelima `CreateTable` ter-scaffold dan seluruh source lulus `dotnet build` (`0 Error(s)`). Bagian 5.2 |
| Seluruh index sesuai `data/data-dictionary.md` | **Sebagian** | 13 index bernama terbukti ada di migration. Ambiguitas daftar index kamus data (bagian 3.5) **masih belum diratifikasi** pemilik arsitektur — kolom yang ditandai "Index" pada tabel kolom tetapi tidak muncul pada daftar index bernama belum diputuskan nasibnya |
| Migration **dibuat** | **Terpenuhi** | `Migrations/20260907062238_AddTablePettyCashModule.cs`. Bagian 5.2 |
| Migration TIDAK dijalankan ke database tanpa otorisasi terpisah | **Terpenuhi** | Tidak ada perintah database yang dijalankan oleh builder, baik pada sesi implementasi maupun sesi verifikasi ulang. Eksekusi dilakukan pemilik modul dengan wewenangnya sendiri di luar sesi |
| Seed cocok dengan § "Rencana data master awal" | **Sebagian** | `InsertData` berisi lima kategori (`TRANSPORT`, `OPERASIONAL`, `KONSUMSI`, `MAINTENANCE`, `ATK`) dan satu kolam (`HOSPITAL_MAIN`, `ACTIVE`) — **cocok pada tingkat migration**. Bahwa baris itu benar-benar ada di database **tidak dapat diverifikasi** dari sesi ini (tanpa akses DB) |

### Definition of Done

| Butir | Status | Bukti |
| --- | --- | --- |
| Model + configuration + migration source **lulus build** | **Terpenuhi** | `dotnet build` dijalankan sendiri pada sesi verifikasi ulang: `0 Error(s)`, 186 warning, tidak satu pun dari berkas Petty Cash. Bagian 5.2 |
| Seed lima kategori direview Finance sebelum diaktifkan produksi | **Belum terpenuhi** | Review Finance belum diminta. Nilai `Description` kelima kategori disusun builder dan perlu dikonfirmasi. Butir ini menuntut aksi organisasi, bukan aksi builder |
| `git status --short` dilaporkan | **Terpenuhi** | Bagian 7 |
| DB tidak dijalankan | **Terpenuhi** | Tidak ada perintah database yang dijalankan builder |

**Kenapa status tetap 🟡 dan bukan ✅.** Tiga butir masih terbuka, dan tidak satu pun dapat
ditutup dari sesi ini tanpa mengarang bukti:

1. **Baris seed di database belum terbukti.** Pemilik modul melaporkan `database update` berhasil;
   sesi ini tidak punya akses DB untuk menghitung barisnya. Yang terbukti hanyalah bahwa
   `InsertData` ada **di dalam berkas migration**.
2. **Review Finance atas lima kategori seed belum diminta** — butir DoD eksplisit pada kartu task.
3. **Tiga ambiguitas kontrak masih menunggu ratifikasi** pemilik arsitektur (bagian 3.5 dan 7),
   salah satunya menyangkut acceptance criterion "seluruh index sesuai kamus data".

Butir yang **sudah tertutup** sejak versi awal laporan: kompilasi dan keberadaan migration.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Sesi verifikasi ulang: `dotnet build` menghasilkan 186 warning pada solution, **tidak satu pun** berasal dari kesebelas berkas task ini. Seluruhnya `CS1573`/`CS1574`/`CS1587` (XML doc) warisan modul lain |
| Masalah yang diketahui | Tiga butir menunggu ratifikasi pemilik arsitektur: (1) `ResponseJson` `text` versus preseden `jsonb`; (2) daftar index kamus data yang tidak konsisten antara tabel kolom dan tabel index bernama; (3) bentuk `IX_BilPettyCashBudget_ActiveSingleton` sebagai unique parsial pada kolom `Status` |
| Risiko tersisa | Risiko "belum pernah dikompilasi" **sudah hilang** — build lulus, migration ada, urutan FK-nya aman. Yang tersisa: (1) baris seed di database belum terbukti dari sesi ini; (2) review Finance atas `Description` lima kategori belum dilakukan, sehingga kategori itu **belum layak diaktifkan di produksi**; (3) tiga ambiguitas kontrak belum diratifikasi |
| Perubahan sampingan | `NONE` dari task ini. Working tree memuat perubahan pengguna yang **sudah ada sebelum** task ini dimulai dan **tidak disentuh**: `BillingInvoicesController.cs`, `BillingInvoiceService.cs`, `Program.cs`, `BillingPaymentHistoryDtos.cs`, seluruh berkas `docs/module-blueprints/billing-kasir/**` selain laporan ini dan penandaan status roadmap, serta `BE-BKC-FIX-008.md` dan `FE-BKC-FIX-011.md` |
| Interupsi | Instruksi pemilik modul di tengah sesi: hentikan `dotnet build`, jangan buat migration, tulis laporan sesuai keadaan sebenarnya. Diikuti apa adanya; tidak ada source yang dibatalkan atau ditambah sesudahnya |
| Verifikasi ulang | 7 September 2026, pada sesi `BE-BKC-034`. Pemilik modul menjalankan migration dan build sendiri di luar sesi. Klaim diperiksa ulang, **bukan** diterima begitu saja: `dotnet build` dijalankan sendiri (`0 Error(s)`) dan berkas migration dibaca isinya. Tidak ada source `BE-BKC-033` yang diubah pada verifikasi ulang — hanya laporan ini dan penandaan status roadmap |
| Status Git | Lihat blok di bawah |
| Langkah berikutnya | Langkah (1)–(3) versi awal — build, `migrations add`, `database update` — **sudah dikerjakan pemilik modul** dan langkah (1)–(2) sudah diverifikasi ulang di sini. Yang tersisa agar status naik ke `✅`: (a) buktikan baris seed ada di database (`SELECT count(*)` pada `MstPettyCashCategory` = 5 dan `BilPettyCashBudget` = 1) dan lampirkan hasilnya ke laporan ini; (b) Finance mereview `Description` lima kategori seed sebelum diaktifkan produksi; (c) pemilik arsitektur meratifikasi tiga ambiguitas kontrak pada bagian 3.5. `BE-BKC-035` dan `BE-BKC-036` kini **tidak lagi terhalang** ketiadaan tabel — tabelnya sudah terbentuk lewat migration |

Keluaran `git status --short` untuk berkas yang **dihasilkan task ini**:

```text
 M Repositories/ApplicationDbContext.cs
?? Areas/HealthServices/BillingManagement/MasterData/Models/MstPettyCashCategory.cs
?? Areas/HealthServices/BillingManagement/PettyCash/
?? Repositories/Configurations/HealthServices/BillingManagement/MasterData/MstPettyCashCategoryConfiguration.cs
?? Repositories/Configurations/HealthServices/BillingManagement/PettyCash/
```

Tidak ada berkas yang di-stage, di-commit, maupun di-push.
