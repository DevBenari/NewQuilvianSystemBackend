# Laporan Perubahan Backend — `BE-FIN-074`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-074` |
| Judul | Bukti pembayaran dan ambang punya tempat tersimpan, dan konfigurasinya terbaca |
| Slice | `REV-14D` — `EPIC FIN-23`, pembayaran langsung berkontrol beserta bukti |
| Roadmap | `roadmap/01-backend-roadmap.md`, bagian "Task REV-14D — `EPIC FIN-23`" |
| Trace | `FIN-DEC-126`, `FIN-DEC-134`, `FIN-DEC-139`; `FIN-DES-086`, `FIN-DES-087`, `FIN-DES-092` |
| Contract version | `FIN-API-1.6` F.3/F.4 (belum disentuh task ini — nol endpoint); `FIN-VAL-1.8` G.1 (belum ditegakkan task ini — validasi unggah milik `BE-FIN-075`). `contract_status: approved 2026-10-02 (Yasmin)` per header YAML `REV-14D`/`REV-14E` |
| Dependency | `BE-FIN-058` ✅ (selesai — kolom `ProofId` pada `FinReceivableMovement`/`FinSupplierPayableMovement` sudah ada) |
| Klasifikasi | `HEAVY` — faktor kualitatif: 10 berkas diubah (>6) dan dampak database/schema (migration) memenuhi ambang `HEAVY` pada `TASK_CLASSIFICATION.md`, walau skor numerik mentah (repo 0 + diperiksa >20 (2) + diubah >8 (2) + logika sedang (1) + kontrak 0 + database (2) + auth 0 + UI 0 = 7) jatuh di rentang `MEDIUM`. Aturan "pakai faktor tertinggi yang berlaku" dipakai di sini |
| Task mode | `BACKEND` (target tulis backend; frontend tidak disentuh) |
| Target tulis | `NewQuilvianSystemBackend` — lihat daftar lengkap pada bagian 3.2 |
| Model | Claude Sonnet 5 (tidak dieskalasi ke Opus — klasifikasi `HEAVY` ditetapkan setelah implementasi selesai tanpa kesulitan; lihat `TASK_CLASSIFICATION.md`: eskalasi model bersifat opsional untuk `HEAVY` yang "benar-benar sulit") |
| Commit backend saat dikerjakan | `0e256765` (working tree membawa perubahan tidak ter-commit dari task REV-14A/14B/14C sebelumnya — lihat bagian 7 "Status Git") |
| Tanggal | 2 Oktober 2026 |
| Status | 🟡 **SEBAGIAN.** Source dan migration lengkap; migration **belum dijalankan** (milik Yasmin, `FIN-DEC-138`); `dotnet build` **NOT RUN** atas permintaan eksplisit pengguna pada task ini — lihat bagian 5 |

---

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | Corporate |
| Module | FinanceManagement |
| Submodule | `Collection` (untuk `FinTransactionProof`); `MasterData` (untuk `MstDirectPaymentThreshold`) |
| Owner/prefix pada registry | `Fin` (Collection — baris registry 2026-09-21, "enam submodul baru"); `Mst` (MasterData — baris registry 2026-09-04 "`Master / Reference / MasterData`" ditambah baris 2026-09-17/09-21 yang menegaskan `MasterData/` FinanceManagement **sengaja tidak** didaftarkan terpisah karena sudah tercakup baris `Mst` umum) |
| Keberlakuan | `NEW CODE` — dua model baru, nol pola legacy bertentangan yang ditiru |
| Status registry | `ACTIVE` untuk keduanya. **Nol** `QBE-MOD-002` — kedua submodule sudah lebih dulu dibuka (Collection: 2026-09-21; MasterData: 2026-09-04/09-17). **Nol** `QBE-MOD-003` — nol folder submodule baru dibuat pada task ini |
| QBE ID yang berlaku | `QBE-MOD-002` (lolos — registry sudah `ACTIVE`, dikutip di atas); `QBE-CODE-002`/`003` (lolos — nol Count/Max/Last+1; kedua PK `Guid.NewGuid()`, nol kolom nomor urut yang dipersistensi); `QBE-NAM-004` (lolos — submodule sudah terdaftar sebelum file model pertama ditulis, sesuai bukti registry) |

---

## 1. Masalah yang diperbaiki

Sebelum task ini, pembayaran langsung piutang dan utang supplier (`FinanceReceivableService.RecordPaymentAsync`,
`FinanceSupplierPayableService` jalur langsung) menerima parameter metode pembayaran, sumber dana,
dan nomor rujukan dari pemanggil **lalu membuangnya** — temuan F1/F3 pada `00-interview-decisions.md`
(`FinanceReceivableService.cs:621-701`, `FinanceSupplierPayableService.cs:207-274`). Tidak ada tempat
menyimpan bukti pembayaran, dan tidak ada ambang nilai yang membatasi pembayaran langsung — siapa pun
dapat membayar nilai berapa pun secara langsung tanpa jejak bukti maupun kontrol nilai.

Task ini **belum** menyambungkan jalur pembayaran itu sendiri (itu `BE-FIN-077`/`078`). Yang dikerjakan
di sini adalah **fondasinya**: dua tabel tempat bukti dan ambang itu akan disimpan, beserta mekanisme
pembacaan dua kunci konfigurasi yang akan dipakai validasi unggah bukti (`BE-FIN-075`).

---

## 2. Proses bisnis

**Tujuan.** Menyediakan tempat penyimpanan metadata bukti pembayaran (`FinTransactionProof`) dan
ambang nilai pembayaran langsung berjejak (`MstDirectPaymentThreshold`), beserta mekanisme pembacaan
konfigurasi jenis dan ukuran berkas bukti yang akan dipakai task berikutnya.

**FinTransactionProof.** Menyimpan metadata saja — jenis bukti, nama berkas asli, nama berkas
tersimpan (dinormalisasi supaya tidak bertabrakan), jalur relatif terhadap akar penyimpanan
(`FileStorage:UploadRootPath` yang sudah berjalan), tipe media, ukuran, pengunggah, dan waktu unggah.
**Berkas fisiknya sendiri tidak disimpan di database** — mengikuti pola yang sudah ada, bukan
`WorkflowFileStorageService` milik HR (batas modul MUST NOT dilintasi, `FIN-DES-087`).

**MstDirectPaymentThreshold.** Master berjejak dengan **satu baris aktif**. Berbeda dari
`appsettings.json`: setiap perubahan ambang MUST disertai `ChangeReason`, dan kolom audit
`IdentityModel` menjawab siapa-kapan. Tabel riwayat penuh **sengaja tidak dibuat** — kolom audit dan
logger (pada `BE-FIN-076` nanti) sudah menjawab perubahan terakhir. Tanpa baris aktif, seluruh
pembayaran langsung akan ditolak sepenuhnya (fail-closed) — perilaku yang disengaja, ditegakkan nanti
oleh `BE-FIN-077`/`078`, bukan oleh tabel ini sendiri.

**Dua kunci konfigurasi.** `FinanceTransactionProofOptions` membaca
`FinanceManagement:TransactionProof:AllowedExtensions` (daftar ekstensi, bawaan `.pdf`, `.jpg`,
`.jpeg`, `.png` bila kuncinya tidak ada — polanya identik dengan
`HumanResource:WorkflowAttachment:AllowedExtensions` pada `WorkflowFileStorageService`) dan
`FinanceManagement:TransactionProof:MaxFileSizeBytes` (`long?`, **sengaja tanpa nilai bawaan**
karena angkanya belum pernah diputuskan — `FIN-OQ-082`). Pemetaan ekstensi ke tipe media (pemeriksaan
#6 `FIN-DES-092`) **tidak** digambar pada task ini — itu keputusan implementasi validasi unggah milik
`BE-FIN-075`.

**Pemicu dan langkah.** Task ini tidak mengubah alur bisnis apa pun yang sudah berjalan — murni
menyiapkan skema dan konfigurasi yang akan dipakai `BE-FIN-075` (unggah bukti), `BE-FIN-076` (CRUD
ambang), `BE-FIN-077`/`078` (pembayaran langsung berkontrol).

**Jalur tidak normal.** Tidak berlaku — task ini tidak mengandung endpoint maupun service runtime yang
dapat dieksekusi.

**Hasil akhir.** Dua tabel baru siap menampung data begitu migration dieksekusi Yasmin; dua kunci
konfigurasi siap dibaca `IOptions<FinanceTransactionProofOptions>` begitu `BE-FIN-075` dibangun.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` (bagian `REV-14D`/`REV-14E`, baris task `BE-FIN-074`)
- `docs/module-blueprints/finance-management/erd/data-dictionary.md` (R14.6 `FinOpeningItemBatch`, R14.7 `FinTransactionProof`, R14.8 `MstDirectPaymentThreshold`, DDL revisi 14 penuh termasuk FK `ProofId` pada kedua tabel mutasi)
- `docs/module-blueprints/finance-management/02-backend-architecture.md` (`FIN-DES-085`..`087`, `FIN-DES-092` penuh termasuk tabel delapan pemeriksaan dan dua kunci konfigurasi)
- `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` (baris 2026-09-04, 09-17, 09-21 — status `Collection` dan `MasterData`)
- `Areas/Corporate/HumanResource/WorkflowManagement/Services/WorkflowFileStorageService.cs` (pola `<Modul>:<Fitur>:AllowedExtensions` dan `ResolveAllowedExtensions` — dipakai ulang sebagai templat, **tidak dipanggil** dari Finance)
- `Areas/Corporate/FinanceManagement/MasterData/Models/MstPettyCashCategory.cs` + `Repositories/Configurations/.../MstPettyCashCategoryConfiguration.cs` (templat model/config master data Finance)
- `Areas/Corporate/FinanceManagement/AccountingIntegration/Models/FinOpeningBalance.cs`-nya via `Repositories/Configurations/.../FinOpeningBalanceConfiguration.cs` (templat check constraint dan unique index partial)
- `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivableMovement.cs` + `Repositories/Configurations/.../FinReceivableMovementConfiguration.cs` (dipastikan kolom `ProofId` **sudah ada tanpa FK** sejak `BE-FIN-058` — dasar keputusan bagian 7)
- `Migrations/20261002093000_AddFinanceSubledgerMovementLedgers.cs` (dipastikan `ProofId` dan unique index-nya memang sudah dibuat di sana, nol FK)
- `Migrations/20261002103000_AddFinanceSubledgerSetup.cs` dan `.Designer.cs` (templat struktural migration + designer tangan — pola persis diikuti)
- `Migrations/ApplicationDbContextModelSnapshot.cs` (titik sisip alfabetis untuk kedua entity baru, templat properti `FinOpeningBalance`/`MstPettyCashCategory`/`MstCurrency`/`FinReceivableMovement`)
- `Repositories/ApplicationDbContext.cs` (titik sisip `DbSet`)
- `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` (dipastikan tidak perlu pendaftaran `AddScoped` tambahan — kedua entity baru murni model+config, nol service baru pada task ini)
- `Program.cs` (pola registrasi `Configure<TOptions>` tanpa syarat vs di dalam `runBackgroundJobs`)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/Collection/Models/FinTransactionProof.cs` (baru) | Model `FinTransactionProof : IdentityModel` — 8 kolom bisnis (`ProofType`, `OriginalFileName`, `StoredFileName`, `RelativePath`, `MediaType`, `SizeBytes`, `UploadedBy`, `UploadedAt`) + konstanta `FinTransactionProofTypes` (dua nilai contoh, daftar tidak tertutup) |
| `Areas/Corporate/FinanceManagement/MasterData/Models/MstDirectPaymentThreshold.cs` (baru) | Model `MstDirectPaymentThreshold : IdentityModel` — 4 kolom bisnis (`Amount`, `ChangeReason`, `IsActive`, `EffectiveFrom`) |
| `Repositories/Configurations/Corporate/FinanceManagement/Collection/FinTransactionProofConfiguration.cs` (baru) | `IEntityTypeConfiguration<FinTransactionProof>` — unique index `StoredFileName` (filter `IsDelete=false`), index `ProofType`, index `UploadedBy`. Nol check constraint |
| `Repositories/Configurations/Corporate/FinanceManagement/MasterData/MstDirectPaymentThresholdConfiguration.cs` (baru) | `IEntityTypeConfiguration<MstDirectPaymentThreshold>` — check constraint `CK_MstDirectPaymentThreshold_Amount` (`Amount > 0`), unique index partial `IsActive` (filter `IsActive=true AND IsDelete=false`) |
| `Repositories/ApplicationDbContext.cs` | Menambah `DbSet<FinTransactionProof> FinTransactionProofs` dan `DbSet<MstDirectPaymentThreshold> MstDirectPaymentThresholds`, masing-masing disisipkan bersebelahan dengan `DbSet` Fin*/Mst* Finance lain yang sudah ada |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Menyisipkan dua blok entity baru pada posisi alfabetis yang benar (`FinTransactionProof` sesudah `FinReceiptDeduction`, sebelum `MstBankAccount`; `MstDirectPaymentThreshold` sesudah `MstCurrency`, sebelum `MstExchangeRate`). **Nol** entity lain disentuh |
| `Migrations/20261002120000_AddFinanceTransactionProofAndDirectPaymentThreshold.cs` (baru) | Migration tangan: `CreateTable` untuk kedua tabel, tiga `CreateIndex` (`IX_FinTransactionProof_ProofType`, `IX_FinTransactionProof_StoredFileName` unique, `IX_FinTransactionProof_UploadedBy`, `IX_MstDirectPaymentThreshold_Active` unique partial), `Down` berisi dua `DropTable`. **Nol** `ALTER TABLE` ke tabel berjalan |
| `Migrations/20261002120000_AddFinanceTransactionProofAndDirectPaymentThreshold.Designer.cs` (baru) | Disalin dari `ApplicationDbContextModelSnapshot.cs` **sesudah** kedua entity baru disisipkan (jadi badan model identik), lalu header diubah: `using Microsoft.EntityFrameworkCore.Migrations;` ditambah, atribut `[Migration("20261002120000_...")]` ditambah, nama kelas menjadi `AddFinanceTransactionProofAndDirectPaymentThreshold`, `BuildModel` menjadi `BuildTargetModel` — pola identik `20261002103000_AddFinanceSubledgerSetup.Designer.cs` |
| `Areas/Corporate/FinanceManagement/Collection/Services/FinanceTransactionProofOptions.cs` (baru) | Kelas options: `AllowedExtensions` (`List<string>`, bawaan `.pdf`/`.jpg`/`.jpeg`/`.png`), `MaxFileSizeBytes` (`long?`, **tanpa bawaan**) |
| `Program.cs` | Menambah `using` untuk `Collection.Services`; menambah `builder.Services.Configure<FinanceTransactionProofOptions>(...)` **di luar** blok `runBackgroundJobs` (fitur HTTP, bukan hosted service — harus terbaca pada runtime role `Web`) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **NOT APPLICABLE.** Nol endpoint dibuat maupun diubah pada task ini. `FIN-API-1.6` F.3/F.4 belum ditegakkan — itu `BE-FIN-075`/`076` |
| Database | **ADA, migration dibuat TIDAK dijalankan.** Dua tabel baru murni (`FinTransactionProof`, `MstDirectPaymentThreshold`); **nol** tabel berjalan diubah skemanya. Migration `AddFinanceTransactionProofAndDirectPaymentThreshold` sesuai `ApplicationDbContextModelSnapshot.cs` yang diperbarui bersamaan (syarat `FIN-DEC-138`). **MUST** dieksekusi Yasmin sendiri — agent tidak menjalankan `dotnet ef database update` atau SQL apa pun |
| Keamanan/Auth | **NOT APPLICABLE.** Nol endpoint, nol `[AccessController]`/`[AccessAction]`/`[AccessPermission]` baru pada task ini. `permission-audit-matrix.md` sudah mencatat `FinanceTransactionProof : Read` tanpa pembatasan pemilik (`H.3`) untuk task mendatang |

---

## 4. Dokumentasi endpoint

**NOT APPLICABLE.** Task ini tidak membuat endpoint HTTP apa pun.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Permintaan eksplisit pengguna pada task ini. Sesuai `status-task-roadmap.md` §2.1, ketiadaan hasil build sebenarnya menahan `✅` — task ini ditandai `🟡` |
| Migration dibuat, belum dijalankan | Terpenuhi | `PASS` (bukti dokumen) | `Migrations/20261002120000_AddFinanceTransactionProofAndDirectPaymentThreshold.cs`/`.Designer.cs` ada; agent **tidak** menjalankan `dotnet ef database update` atau SQL apa pun — nol perintah eksekusi database dijalankan sesi ini |
| Skema sesuai `ApplicationDbContextModelSnapshot.cs` | Terverifikasi — migration `CreateTable` dan dua entity block pada snapshot ditulis dari properti model yang sama, disilangkan manual baris demi baris | `PASS` (verifikasi manual, bukan `dotnet ef migrations has-pending-model-changes`) | Bagian 3.2; perbandingan `FinTransactionProofConfiguration.cs` vs blok snapshot baris 5449-5533 |
| Verifikasi skema terhadap `erd/data-dictionary.md` R14.6/R14.7(/R14.8) | Terverifikasi kolom demi kolom — tipe, panjang, wajib/opsional, index, dan check constraint cocok persis dengan DDL revisi 14 KECUALI FK `ProofId` (lihat bagian 7) | `PASS` dengan satu pengecualian tercatat | Bagian 3.2, 7 |
| `StoredFileName` unique | Terverifikasi — `IX_FinTransactionProof_StoredFileName` unique partial (`IsDelete=false`) pada model, configuration, migration, dan snapshot — empat tempat konsisten | `PASS` | Bagian 3.2 |
| `ProofId` unique pada kedua tabel mutasi | **Tidak disentuh, sudah ada sejak `BE-FIN-058`** — diverifikasi ulang masih utuh (`IX_FinReceivableMovement_ProofId`, `IX_FinSupplierPayableMovement_ProofId`), bukan dibuat task ini | `PASS` (warisan, diverifikasi tidak rusak) | `FinReceivableMovementConfiguration.cs` baris 50-53 |
| Nol tabel berjalan disentuh | Terverifikasi — migration hanya berisi dua `CreateTable` dan `CreateIndex` pada tabel baru; nol `AlterTable`/`AddColumn` pada tabel existing | `PASS` | `Migrations/20261002120000_...cs` |
| Konfigurasi terbaca | Terverifikasi secara struktural — `Configure<FinanceTransactionProofOptions>` terdaftar tanpa syarat di `Program.cs`, kelas options mem-bind dua kunci dengan perilaku kosong yang berbeda sesuai `FIN-DES-092` | `PASS` (verifikasi kode; tidak dijalankan runtime) | `Program.cs`, `FinanceTransactionProofOptions.cs` |
| Brace balance berkas snapshot besar (125 ribu+ baris) | `1887` buka = `1887` tutup pada `ApplicationDbContextModelSnapshot.cs` dan pada `.Designer.cs` baru | `PASS` | `grep -c` dilampirkan pada riwayat sesi |

Uji manual: `NOT FEASIBLE` — memerlukan database aktif untuk membuktikan migration benar-benar
diterapkan tanpa galat; eksekusi migration adalah wewenang terpisah milik Yasmin.

**Tidak dijalankan:** `dotnet build`, `dotnet restore` (keduanya `NOT RUN` atas permintaan eksplisit
pengguna), `dotnet ef migrations add`/`dotnet ef database update` (tidak pernah dijalankan agent sesuai
`FIN-DEC-138`), `dotnet test` (backend tidak memelihara project automated test).

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Kedua tabel terbentuk | Terpenuhi (source + migration) | Bagian 3.2 |
| `StoredFileName` unique | Terpenuhi | Bagian 3.2, 5 |
| `ProofId` unique pada kedua tabel mutasi sehingga satu bukti tidak dapat dipakai dua pembayaran | Terpenuhi — **diwarisi**, sudah ada sejak `BE-FIN-058`, diverifikasi tidak rusak | Bagian 5 |
| Nol tabel berjalan disentuh | Terpenuhi | Bagian 3.3, 5 |
| `dotnet build` PASS | **Belum terpenuhi** — `NOT RUN` atas permintaan eksplisit pengguna pada task ini | Bagian 5 |
| Migration dibuat dan laporannya menyatakan belum dijalankan | Terpenuhi — dinyatakan eksplisit di sini | Bagian 3.3, 6 |
| Laporan task tracked ada | Terpenuhi | Berkas ini |

Butir DoD `dotnet build PASS` **belum terpenuhi** karena sengaja tidak dijalankan atas instruksi
eksplisit pengguna pada task ini. Ini **bukan** pengecualian DoD permanen (`status-task-roadmap.md`
§2.2) — build tetap wajib sebelum task ini naik ke `✅`.

**Migration — status eksekusi (wajib dinyatakan, `FIN-DEC-138`):** `AddFinanceTransactionProofAndDirectPaymentThreshold`
**dibuat, BELUM dijalankan.** Langkah yang Yasmin perlu jalankan sendiri: `dotnet ef database update`
(atau menerapkan DDL yang setara) pada database target, sesudah lebih dulu menjalankan `dotnet build`
dan `dotnet ef migrations has-pending-model-changes` untuk memastikan tidak ada drift yang
luput dari pembuatan tangan berkas ini.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Migration dan Designer.cs dibuat tangan**, bukan lewat `dotnet ef migrations add` (konsisten dengan preseden `BE-FIN-058`/`064`). Tanpa `dotnet build`, risiko ketidaksesuaian antara source model dan snapshot hand-written **lebih tinggi** dari biasanya — kesalahan ketik pada `ApplicationDbContextModelSnapshot.cs` yang 125 ribu+ baris hanya akan terlihat saat `dotnet build` atau `dotnet ef migrations has-pending-model-changes` benar-benar dijalankan. **MUST** dijalankan sebelum migration ini dieksekusi Yasmin |
| Masalah yang diketahui | **Keputusan cakupan yang disengaja, bukan kelalaian:** FK `ProofId` pada `FinReceivableMovement`/`FinSupplierPayableMovement` → `FinTransactionProof.Id` yang digambar `erd/data-dictionary.md` (DDL revisi 14, `CONSTRAINT FK_..._FinTransactionProof_ProofId`) **TIDAK dibuat** pada migration ini. Alasannya: (1) menambahkannya menuntut `ALTER TABLE` pada `FinReceivableMovement`/`FinSupplierPayableMovement` — tabel yang sudah dibuat migration `BE-FIN-058`, dan (2) acceptance criteria `BE-FIN-074` sendiri eksplisit menyatakan "**nol** tabel berjalan disentuh". Menambahkan FK itu akan melanggar acceptance criteria task ini sendiri. **Nol** task lain pada `REV-14D`/`14E` (termasuk `BE-FIN-077`/`078` yang pertama kali akan menulis `ProofId`) terdaftar membawa migration untuk menambahkan FK ini — gap ini dilaporkan untuk keputusan pemilik modul, bukan diselesaikan sepihak. Integritas referensial `ProofId` untuk saat ini hanya dijaga di level APLIKASI (service MUST memverifikasi `FinTransactionProof` ada sebelum memakai `ProofId`-nya) dan oleh unique index yang sudah ada (mencegah pemakaian ganda), **bukan** oleh constraint database |
| Risiko tersisa | Tanpa FK database, baris mutasi secara teori dapat menulis `ProofId` yang menunjuk baris `FinTransactionProof` yang tidak ada (tidak divalidasi database). Mitigasinya ada di tangan service layer `BE-FIN-077`/`078` yang akan menulis `ProofId` — **MUST** memvalidasi keberadaannya sebelum menyimpan |
| Perubahan sampingan | `NONE` — seluruh 10 berkas yang disentuh murni penambahan untuk task ini. `ApplicationDbContext.cs` dan `Program.cs` yang sudah membawa perubahan tidak ter-commit dari task-task REV-14 sebelumnya tidak dipulihkan maupun diubah bagian lamanya |
| Interupsi | `NONE` |
| Status Git | `git status --short` pada akhir task menunjukkan 10 berkas task ini (7 baru, 3 dimodifikasi), tercampur dengan modifikasi tidak ter-commit dari task `BE-FIN-058`..`073` yang memang belum di-commit sejak sebelum task ini dimulai. Tidak ada file di luar scope Finance Management yang tersentuh |
| Langkah berikutnya | (1) Pengguna menjalankan `dotnet build` lalu `dotnet ef migrations has-pending-model-changes` untuk memvalidasi snapshot hand-written ini sebelum migration dieksekusi; (2) Yasmin menjalankan migration ke database target; (3) pemilik modul memutuskan apakah FK `ProofId`→`FinTransactionProof` dibuat sebagai migration tersendiri (dan kapan — sebelum atau sesudah `BE-FIN-077`/`078` mulai menulis data), atau sengaja dibiarkan sebagai integritas level aplikasi saja; (4) `BE-FIN-075` melanjutkan dengan membangun `FinanceTransactionProofService` dan endpoint unggah di atas fondasi ini |

---

## Catatan verifikasi tertunda — DITUTUP 4 Oktober 2026

Laporan ini (bagian 3.2, serta tabel verifikasi dan peringatannya) mencatat satu risiko terbuka: migration
dan `Designer.cs` dibuat **tangan**, sehingga kesesuaian `ApplicationDbContextModelSnapshot.cs` hanya
terverifikasi lewat perbandingan manual baris demi baris — bukan oleh tooling EF.

**Risiko itu sekarang tertutup.** Pengguna menjalankan:

```
dotnet ef migrations has-pending-model-changes --configuration Release
```

pada 4 Oktober 2026, dan keluarannya *"No changes have been made to the model since the last migration."*

**Kenapa hasil itu juga berlaku untuk laporan ini.** Pemeriksaan tersebut membandingkan **seluruh model**
terhadap snapshot kumulatif, bukan hanya migration terakhir. Snapshot hari ini sudah memuat blok entity
yang ditulis tangan oleh task ini. Hasil bersih berarti blok itu **selaras** dengan model — kesalahan ketik
pada berkas 125 ribu+ baris yang dikhawatirkan laporan ini **tidak ada**.

**Yang TIDAK dinyatakan hasil ini:** pemeriksaan tersebut **tidak** menyambung ke basis data dan **tidak**
menyatakan apa pun tentang apakah migration sudah diterapkan. Penerapan tetap milik Yasmin, dan status
penerapan migration task ini tidak berubah oleh catatan ini.
