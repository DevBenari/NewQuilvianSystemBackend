# Laporan Perubahan Backend — `BE-BD-004`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BD-004` |
| Judul | Permintaan PMI dibuat, penerimaan dicatat, kantong lahir `Received` |
| Slice | `MVP-1` — jalur bekas `G4`; `RequestNumber` dari provider nomor bersama |
| Roadmap | `docs/module-blueprints/bank-darah/roadmap/backend-roadmap.md`, blok task `BE-BD-004` |
| Trace | `DEC-BD-002/003/008/020/025/036` · `ASM-BD-003` · `BD-AGG-02`, `BD-AGG-03`, `BD-DOM-03/04/05/15/16` · `BD-XINV-02`, `BD-XINV-03` · `INV-BD-017` · `contracts/api-contract.md` §Provider Request dan §Blood Unit · `contracts/state-transition-matrix.md` §2 dan §3 · `contracts/validation-matrix.md` §2 (`VAL-BD-006/007/014/015`) serta `VAL-BD-003` dan `VAL-BD-016` |
| Contract version | `v4` — **`approved`** (`Sukmagp` / `2026-09-03`) |
| Dependency | `G1` ✅ · `G2b` ✅ · `G4` ✅ · `BE-BD-003` ✅ |
| Klasifikasi | `HEAVY` — skor 11: satu repository (0), berkas diperiksa lebih dari 20 (2), berkas diubah lebih dari 8 (2), logika kompleks (2), memakai kontrak yang sudah disetujui (1), entity baru beserta migration (2), hak akses berkaitan tetapi bukan inti (1), satu workflow (1) |
| Task mode | `BACKEND` — dinyatakan eksplisit oleh pemilik pekerjaan |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/BloodBankManagement/**`, `Repositories/**`, `Migrations/**`, `Tests/**`, `Program.cs`, serta laporan, roadmap, traceability, dan `MODULE-STATUS` Bank Darah |
| Wewenang database | Pembuatan migration **dan** penerapannya **hanya** ke `QuilvianNewDevSukma` — keduanya dinyatakan eksplisit dan terpisah pada prompt task |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `d07dcf3` cabang `sukmagp` |
| Tanggal | `2026-09-11` |
| Status | ✅ **`SELESAI`** sejak roadmap revisi 9, 11 September 2026 — pemilik (`Sukmagp`) meneruskan `AC-BD-023`/`032` ke `BE-BD-015` dan `AC-BD-033` ke `BE-BD-006`, sehingga keenam kriteria yang tetap milik task ini terbukti penuh (bagian 6). Nol source, test, maupun migration berubah untuk penerusan ini. **Riwayat:** 🟡 **`SELESAI SEBAGIAN`** — seluruh pekerjaan di dalam scope `BE-BD-004` selesai dan seluruh validasi lulus: build `0 Error(s)`, 561 + 231 test, 9 uji PostgreSQL, migration diterapkan ke `QuilvianNewDevSukma`. **6 dari 9 acceptance criteria terbukti penuh.** `AC-BD-023` dan `AC-BD-032` terbukti pada seluruh bagian yang lahir di task ini, tetapi perpindahan kantongnya ke `PendingReview` baru ada pada `BE-BD-015`; `AC-BD-033` menuntut endpoint alokasi milik `BE-BD-006`. Lihat bagian 6 dan 7 |

---

## 1. Masalah yang diperbaiki

Sesudah `BE-BD-003`, rumah sakit sudah dapat mencatat **kebutuhan** darah seorang pasien. Tetapi
Bank Darah belum dapat mencatat **dari mana darahnya datang**. Rumah sakit ini tidak punya stok
umum (`DEC-BD-003`): setiap kantong diminta ke PMI atas nama satu pasien, dikirim secara manual,
lalu diterima fisik di BDRS. Tanpa catatan permintaan dan penerimaan, tidak ada satu kantong pun
yang tercatat ada, sehingga penyimpanan, alokasi, bukti kecocokan, dan pemberian tidak dapat
dimulai.

Tiga hal harus benar sejak kantong pertama diterima:

| Hal | Kenapa penting |
| --- | --- |
| **Sisa permintaan tidak pernah negatif** | Bila diminta 2 kantong lalu datang 3, sisa harus berhenti di 0 — bukan −1. Angka minus membuat layar dan laporan menyesatkan |
| **Kiriman berlebih tidak pernah ditolak** | Kantong yang sudah ada secara fisik wajib tercatat. Menolaknya berarti ada darah di kulkas yang tidak diketahui sistem |
| **Satu kebutuhan, satu permintaan berjalan** | Dua permintaan berjalan untuk kebutuhan yang sama berarti PMI dapat mengirim dua kali lipat |

**Contoh konkretnya.** Dokter memesan PRC 2 kantong untuk Pasien A. Petugas BDRS meminta ke PMI.
PMI mengirim **3** kantong. Tanpa aturan ini, sisa permintaan tercatat −1 dan kantong ketiga tidak
jelas statusnya. Dengan aturan ini: permintaan `Fulfilled`, sisa **0**, ketiga kantong tercatat,
dan kantong ketiga ditandai **berlebih** supaya kelak diputuskan manusia.

---

## 2. Proses bisnis

### 2.1 Tujuan dan pelaku

| Hal | Isi |
| --- | --- |
| Tujuan | Mencatat permintaan pasokan ke PMI untuk satu order, lalu mencatat setiap kantong yang benar-benar datang, tanpa sisa negatif dan tanpa menolak kelebihan |
| Pelaku | Petugas Bank Darah (buat permintaan, catat penerimaan, batalkan) · Sistem (tutup administratif saat kunjungan berakhir) |
| Pemicu | Ada order darah aktif (`BE-BD-003`) yang perlu dipenuhi |
| Hasil akhir | Permintaan bernomor `PMI-xxxxxxxx` beserta penerimaannya, dan kantong berstatus `Received` — belum tersimpan, belum dapat dialokasikan |

### 2.2 Langkah normal

1. Petugas memilih **order darah asal**. Hanya itu yang dikirim — pasien disalin dari order, dan
   jumlah yang diminta **diturunkan dari baris order per komponen**. Tidak ada isian jumlah yang
   dapat berbeda dari kebutuhan yang tercatat.
2. Sistem memeriksa: pelaku dikenali → order ada → order masih aktif → order punya kebutuhan → 
   kunjungan asal belum berakhir.
3. Sistem mengantre pada kunci per order, lalu memeriksa apakah order itu **sudah punya permintaan
   yang masih berjalan** (`BD-XINV-02`).
4. **Baru setelah seluruh pemeriksaan lolos**, nomor diminta dari `NumberSeriesAllocator` — deret
   `BBK_PROVIDER_REQUEST`, awalan `PMI`, 8 digit, tidak pernah diulang.
5. Permintaan tersimpan berstatus `Requested`. **Stok belum bertambah sama sekali** (`DEC-BD-002`).
6. Ketika PMI mengirim, petugas mencatat **penerimaan**: waktu terima fisik dan daftar kantong —
   nomor kantong dari PMI beserta komponennya. Petugas penerima diambil dari akun yang login.
7. Sistem mengantre pada kunci per permintaan, membaca ulang kantong yang sudah diterima, lalu
   menghitung **per komponen**: selama komponen itu masih punya sisa, kantongnya terhitung memenuhi
   permintaan; sesudah sisanya nol, kantong berikutnya ditandai **berlebih** (`IsExcess`).
8. Setiap kantong lahir berstatus `Received` bersama satu baris riwayat. Status permintaan dihitung
   ulang: sebagian terpenuhi → `PartiallyFulfilled`, seluruhnya → `Fulfilled`.

### 2.3 Jalur tidak normal

| Keadaan | Yang terjadi | Kode |
| --- | --- | --- |
| Order sudah punya permintaan yang masih berjalan | Ditolak; **nomor tidak terbit** | `422` `VAL-BD-006` |
| Order tidak aktif (dibatalkan, terpenuhi, kedaluwarsa) | Ditolak | `422` |
| Kunjungan asal order sudah berakhir | Ditolak — order itu akan kedaluwarsa | `422` |
| Order tanpa jumlah yang dapat diminta | Ditolak | `400` `VAL-BD-007` |
| Kiriman melebihi permintaan | **Diterima**, kantong berlebih ditandai; balasan membawa peringatan | `200` `VAL-BD-014` |
| Kantong komponen yang sama sekali tidak diminta | **Diterima**, seluruhnya ditandai berlebih | `200` `VAL-BD-014` |
| Komponen tidak ada di katalog | Ditolak | `400` `VAL-BD-003` |
| Nomor kantong PMI sudah pernah tercatat, atau tercantum dua kali | Ditolak; tidak ada kantong yang lahir | `422` |
| Waktu penerimaan di masa depan | Ditolak | `400` |
| Penerimaan pada permintaan `Fulfilled` atau `Cancelled` | Ditolak — status terminal | `422` |
| Penerimaan pada permintaan `ClosedEncounter` | **Diterima** — kantong susulan tetap dicatat, permintaan tidak aktif lagi | `200` |
| Pembatalan tanpa alasan dari daftar terkendali | Ditolak | `400` `VAL-BD-016` |
| Pembatalan dari layar usang | Ditolak | `409` |
| Kunjungan asal berakhir sementara permintaan masih kurang | Sistem menutup ke `ClosedEncounter`; **tidak ada endpoint** | — |

### 2.4 Status

**Permintaan** (`BbkProviderRequestStatus`):

| Status | Arti | Lahir pada task ini? |
| --- | --- | --- |
| `Requested` | Dicatat, menunggu kiriman | Ya |
| `PartiallyFulfilled` | Sebagian kantong sudah diterima | Ya |
| `Fulfilled` | Seluruh kebutuhan diterima, sisa 0. Terminal | Ya |
| `Cancelled` | Dibatalkan dengan alasan terkendali. Terminal | Ya — `POST /{id}/cancel` |
| `ClosedEncounter` | Kunjungan berakhir, masih menerima kantong susulan | Ya — lewat service; pemicu otomatisnya belum ada |

**Kantong** (`BbkBloodUnitStatus`): kesembilan nilai kontrak sudah didefinisikan, tetapi task ini
**hanya melahirkan `Received`**. `Stored`, `Available`, `PendingReview`, dan seterusnya lahir
bersama task pemiliknya — penyimpanan pada `BE-BD-015`, alokasi pada `BE-BD-006`.

### 2.5 Contoh berangka dari awal sampai akhir

Pasien A dirawat inap. Order `ORD-00000001` meminta **PRC 2** dan **Trombosit 1**.

| Waktu | Kejadian | Hasil |
| --- | --- | --- |
| Senin 09.00 | Petugas BDRS membuat permintaan | `PMI-00000001`, `Requested`, sisa **3** (PRC 2 + TC 1). Kantong di sistem: **0** |
| Senin 09.05 | Petugas lain mencoba membuat permintaan untuk order yang sama | **Ditolak** `VAL-BD-006`. Nomor tidak terbit |
| Senin 14.00 | PMI mengirim **3 PRC** | Dua PRC terhitung, PRC ketiga **berlebih**. Status `PartiallyFulfilled`, sisa **1** — trombositnya. Balasan: "Kiriman melebihi permintaan…" |
| Selasa 08.00 | PMI mengirim 1 trombosit | Status `Fulfilled`, sisa **0** |
| Selasa 08.30 | PMI mengirim 1 PRC lagi | **Ditolak** — permintaan sudah `Fulfilled` (lihat pertanyaan terbuka di bagian 8) |

Contoh kedua, kunjungan berakhir: permintaan PRC 3, diterima 2, lalu pasien pulang. Sistem menutup
permintaan ke `ClosedEncounter` dengan sisa 1 dan riwayat utuh. Bila kantong ketiga tetap datang,
ia dicatat, membawa rujukan permintaan asalnya, dan permintaan tetap `ClosedEncounter`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Kelompok | Berkas |
| --- | --- |
| Tata kelola | `AGENTS.md` · `CLAUDE.md` · suite skill `1.18.0`: `rules/backend/{TASK_RULES,DATABASE_RULES,REPORT_TEMPLATE}.md` · `rules/backend/engineering/{BACKEND_ENGINEERING_CONTRACT,MODULE_OWNERSHIP_PREFIX_REGISTRY}.md` · `transaction-endpoint-standard.md` · `role-access-rules.md` · `rules/rule-output/status-task-roadmap.md` |
| Kontrak modul | `roadmap/backend-roadmap.md` · `roadmap/requirement-traceability.md` · `contracts/api-contract.md` · `contracts/state-transition-matrix.md` · `contracts/validation-matrix.md` · `contracts/permission-audit-matrix.md` · `data/data-dictionary.md` · `02-backend-architecture.md` · `03-domain-architecture.md` · `00-interview-decisions.md` · `testing/acceptance-test-matrix.md` |
| Pola source terdekat | Seluruh berkas `BE-BD-003`: `BbkBloodOrderService.cs`, `BbkBloodOrderController.cs`, `BloodOrderDtos.cs`, `BbkBloodOrder.cs`, `BbkBloodOrderLine.cs`, `BbkTransitionHistory.cs`, ketiga configuration, `BbkEncounterStatusReader.cs`, `BloodOrderServiceTests*.cs`, `BloodOrderPostgresTests.cs` · `BloodBankRoleAccessContractTests.cs` |
| Dipakai ulang | `NumberSeriesAllocator` (`PLT-BE-003`) · `BbkEncounterStatusReader` · `BbkTransitionHistory` · `MstPatient` · `MstBloodComponent` · `MstBloodBankReason` + `BloodBankReasonSeeder.cs` · `BillingTestDatabaseFixture` |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BloodBankManagement/Enums/BbkProviderRequestStatus.cs` | **Baru.** Lima status sesuai matriks §2 |
| `Areas/HealthServices/BloodBankManagement/Enums/BbkBloodUnitStatus.cs` | **Baru.** Sembilan status sesuai matriks §3 |
| `Areas/HealthServices/BloodBankManagement/Models/BbkProviderRequest.cs` | **Baru.** Aggregate root `BD-AGG-02`; tanpa kolom jumlah maupun sisa |
| `Areas/HealthServices/BloodBankManagement/Models/BbkBloodUnitReceipt.cs` | **Baru.** Satu kedatangan fisik |
| `Areas/HealthServices/BloodBankManagement/Models/BbkBloodUnit.cs` | **Baru.** Aggregate root `BD-AGG-03`; dua kolom sengaja ditunda (bagian 7) |
| `Areas/HealthServices/BloodBankManagement/DTOs/ProviderRequestDtos.cs` | **Baru.** Permintaan dan balasan permintaan PMI |
| `Areas/HealthServices/BloodBankManagement/DTOs/BloodUnitDtos.cs` | **Baru.** Balasan kantong darah |
| `Areas/HealthServices/BloodBankManagement/DTOs/BloodBankCommonDtos.cs` | **Baru.** Pilihan penyaring, pengurutan, dan baris riwayat yang dipakai kedua controller baru |
| `Areas/HealthServices/BloodBankManagement/Services/BbkProviderRequestService.cs` | **Baru.** Buat, terima, batalkan, tutup administratif, baca |
| `Areas/HealthServices/BloodBankManagement/Services/BbkBloodUnitService.cs` | **Baru.** Pembacaan kantong saja |
| `Areas/HealthServices/BloodBankManagement/Services/BbkDisplayLabels.cs` | **Baru.** Label enum dibaca dari `[Display]` pada enum-nya sendiri |
| `Areas/HealthServices/BloodBankManagement/Controllers/BbkProviderRequestController.cs` | **Baru.** Delapan endpoint |
| `Areas/HealthServices/BloodBankManagement/Controllers/BbkBloodUnitController.cs` | **Baru.** Lima endpoint baca |
| `Repositories/Configurations/HealthServices/BloodBankManagement/BbkProviderRequestConfiguration.cs` | **Baru.** Index unik `RequestNumber`, index terfilter permintaan berjalan, index `BloodOrderId`, FK `Restrict`, `Version` concurrency token |
| `Repositories/Configurations/HealthServices/BloodBankManagement/BbkBloodUnitReceiptConfiguration.cs` | **Baru.** Index unik `(ProviderRequestId, Sequence)` |
| `Repositories/Configurations/HealthServices/BloodBankManagement/BbkBloodUnitConfiguration.cs` | **Baru.** Index unik `PmiBagNumber`, FK `Restrict` |
| `Repositories/ApplicationDbContext.cs` | Tiga `DbSet` pada region `BLOOD BANK MANAGEMENT` — `+4 / −0` |
| `Program.cs` | `AddScoped<BbkProviderRequestService>()` dan `AddScoped<BbkBloodUnitService>()` — `+2 / −0` |
| `Migrations/20260911032311_AddBbkProviderRequestAndBloodUnit.cs` + `.Designer.cs` | **Baru.** Tiga `CreateTable`, enam FK `Restrict`, empat belas index; `Down()` hanya tiga `DropTable`. **Nol operasi pada tabel modul lain** |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Tiga blok entity baru beserta relasinya — **`+306 / −0`** |
| `Tests/QuilvianSystemBackend.Tests/HealthServices/BankDarah/ProviderRequest/ProviderRequestServiceTests.cs` | **Baru.** Aturan bisnis, acceptance criteria, penjaga kolom kamus data |
| `Tests/QuilvianSystemBackend.Tests/HealthServices/BankDarah/ProviderRequest/ProviderRequestServiceTests.Endpoints.cs` | **Baru.** Bentuk kontrak endpoint dan pemetaan HTTP status |
| `Tests/QuilvianSystemBackend.Tests/HealthServices/BankDarah/MasterData/BloodBankRoleAccessContractTests.cs` | Dua controller didaftarkan; `BE-BD-004` masuk daftar task selesai; cakupan butir **20 → 25** — `+7 / −4` |
| `Tests/QuilvianSystemBackend.IntegrationTests.Postgres/BankDarah/ProviderRequestPostgresTests.cs` | **Baru.** Lima bukti yang hanya dapat dibuktikan PostgreSQL |
| `docs/module-blueprints/bank-darah/task/report/backend/BE-BD-004.md` | **Baru.** Laporan ini |
| `docs/module-blueprints/bank-darah/roadmap/backend-roadmap.md` | Status `BE-BD-004` 🟡 sebagian; `BE-BD-016` 25/39; ringkasan status dan grafik |
| `docs/module-blueprints/bank-darah/roadmap/requirement-traceability.md` | Bukti `BE-BD-004`, hitungan bukti 28 → 34, gap baru |
| `docs/module-blueprints/bank-darah/MODULE-STATUS.md` | Keadaan sekarang, status task, dan task berikutnya — ditambah penyegaran metadata pada langkah pendahulu (bagian 8) |
| `docs/module-blueprints/bank-darah/blueprint-manifest.md` | **Langkah pendahulu, bukan bagian task ini:** penyegaran metadata `manage-module-blueprint` sesudah `BE-BD-003` (bagian 8) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif murni.** Dua base URL baru, tiga belas endpoint. Nol endpoint existing berubah |
| Database | Tiga tabel baru — `BbkProviderRequest`, `BbkBloodUnitReceipt`, `BbkBloodUnit`. **Nol tabel existing diubah.** Migration `20260911032311_AddBbkProviderRequestAndBloodUnit` **dibuat dan diterapkan ke `QuilvianNewDevSukma`** — `139/139`, nol tertunda. **Belum** diterapkan ke `QuilvianNewDevTim01`, staging, maupun production |
| Keamanan/Auth | Lima butir hak akses lahir lewat `AccessMenuSeeder`: `BloodProviderRequest : Read`, `Create`, `Process`, `Update`, dan `BloodUnit : Read`. **Nol pemeriksaan nama peran, nama jabatan, atau `UserType`**. Petugas pembuat dan penerima diturunkan dari klaim login, tidak pernah dari isian. `PmiBagNumber` **tidak pernah** ditulis ke log. Nol endpoint `[AllowAnonymous]` |

### 3.4 Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `BloodBankManagement` |
| Submodule | `NOT APPLICABLE` |
| Pemilik / prefix registry | `BloodBankManagement / Blood Bank` → prefix **`Bbk`** |
| Status registry | **`ACTIVE`** — registry suite skill `1.18.0` baris 29, catatan lifecycle 3 September 2026 |
| Keberlakuan | **`NEW CODE`** |
| QBE ID yang berlaku | `QBE-ENT-001` ketiga entity mewarisi `IdentityModel` · `QBE-ENT-003` nol kolom jumlah/sisa/presentasi tersimpan · `QBE-NAM-001/002/004` prefix `Bbk`, nol `Trx*` · `QBE-CFG-001` tiga `IEntityTypeConfiguration` · `QBE-MOD-001/002/003` · `QBE-SVC-001` controller nol akses `ApplicationDbContext` · `QBE-API-001` · `QBE-PERM-001` · `QBE-LOG-001` · `QBE-DTO-001` · `QBE-VAL-001` · `QBE-TXN-001` · `QBE-PAGE-001` · `QBE-DEL-001` pembatalan lewat aksi, bukan hapus |
| `QBE-CODE-001..006` | **Berlaku penuh.** `RequestNumber` dialokasikan service lewat `NumberSeriesAllocator` (`006`); controller tidak menyentuh nomor (`002`); nol `Count+1`/`Max+1` untuk nomor bisnis (`003`) — urutan penerimaan di dalam satu permintaan memakai hitungan di bawah kunci penasihat **dan** index unik, sehingga berpelindung; index unik `IX_BbkProviderRequest_RequestNumber` (`004`); awalan `PMI`, 8 digit, `NEVER` milik Bank Darah (`005`) |
| Pengecualian QBE | `NONE` |

---

## 4. Dokumentasi endpoint

#### Health Services / Blood Bank Management / Provider Request

Base URL: `api/v1/health-services/blood-bank-management/provider-requests`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Konfigurasi penyaring, pengurutan, dan pilihan status | `BloodProviderRequest : Read` |
| `GET` | `/summary` | Jumlah permintaan per status dan permintaan yang pernah menerima kelebihan | `BloodProviderRequest : Read` |
| `GET` | `/` | Daftar permintaan dengan penyaring, pencarian nomor permintaan/nomor order/nama/rekam medis, dan halaman | `BloodProviderRequest : Read` |
| `GET` | `/{id}` | Detail permintaan: pemenuhan per komponen, riwayat penerimaan beserta kantongnya, riwayat status, aksi yang tersedia | `BloodProviderRequest : Read` |
| `GET` | `/{id}/status-history` | Riwayat perpindahan status | `BloodProviderRequest : Read` |
| `POST` | `/` | Buat permintaan atas nama satu pasien dari satu order aktif | `BloodProviderRequest : Create` |
| `POST` | `/{id}/receipts` | Catat penerimaan kantong, termasuk kelebihan | `BloodProviderRequest : Process` |
| `POST` | `/{id}/cancel` | Batalkan permintaan dengan alasan terkendali | `BloodProviderRequest : Update` |

#### Health Services / Blood Bank Management / Blood Unit

Base URL: `api/v1/health-services/blood-bank-management/blood-units`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Konfigurasi penyaring, pengurutan, dan pilihan status kantong | `BloodUnit : Read` |
| `GET` | `/summary` | Jumlah kantong per status dan jumlah kantong berlebih | `BloodUnit : Read` |
| `GET` | `/` | Daftar kantong; penyaring `unitStatus`, `isExcess`, `providerRequestId`, `bloodComponentId`, dan pencarian | `BloodUnit : Read` |
| `GET` | `/{id}` | Detail kantong beserta asal dan riwayat statusnya | `BloodUnit : Read` |
| `GET` | `/{id}/status-history` | Riwayat perpindahan status kantong | `BloodUnit : Read` |

**Bentuk transaksi, bukan master data.** Kedua controller sengaja **tidak** menyediakan
`GET /options`, `PATCH /{id}/status` generik, `PUT`, maupun `DELETE`. Arketipe permintaan PMI
adalah **aggregate ber-lifecycle**; permukaan kantong pada slice ini **read-only** — tindakannya
lahir bersama task pemiliknya.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln -nodeReuse:false -p:UseSharedCompilation=false` — build 1, sebelum migration | `0 Error(s)`, `210 Warning(s)`, 4 menit 49 detik | `PASS` | Sama dengan baseline 210. Nol warning menyebut berkas task ini |
| Build 2, sesudah migration | `0 Error(s)`, `210 Warning(s)`, 4 menit 44 detik | `PASS` | Sama dengan baseline 210 |
| `dotnet test --no-build` — `QuilvianSystemBackend.Tests` pada build 2 | `Failed: 0, Passed: 561, Total: 561` | `PASS` | Naik dari 498 — tepat 63 test baru. Dari `.trx`: nama lengkap memuat `BankDarah` **276** · `ProviderRequestServiceTests` **63** · `BloodOrderServiceTests` **79** · `BloodBankRoleAccessContractTests` **13**. Seluruhnya lulus |
| `dotnet test --no-build` — `UnitTests.Sqlite` | `Failed: 0, Passed: 231, Total: 231` | `PASS` | Membangun model relasional, sehingga FK, index, dan filter index ketiga tabel ikut terperiksa |
| `dotnet test --no-build` — `UnitTests.InMemory` | `Failed: 9, Passed: 896, Total: 905` | `EXISTING / ENVIRONMENT ISSUE` | Kesembilannya `BillingManagement`, **nama persis sama** dengan daftar baseline laporan `BE-BD-005`. Nol berkas Billing disentuh |
| `IntegrationTests.Postgres` penyaring `BankDarah` pada `QuilvianNewDevSukma` | **9 dari 9 lulus** | `PASS` | Lima baru: `SepuluhPermintaanSerentak_OrderSama_HanyaSatuYangLahir` · `DuaPenerimaanSerentak_KeduanyaDicatat_TepatSatuBerlebih_SisaTidakNegatif` · `NomorKantongPmi_DijagaIndexUnikFisikDiDatabase` · `PermintaanBerjalanKedua_DitolakIndexUnikTerfilter_PermintaanBatalTidakTerhitung` · `KantongTanpaPermintaanSah_DitolakForeignKeyDatabase`. Empat regresi `BloodOrderPostgresTests` tetap lulus |
| `ef migrations add AddBbkProviderRequestAndBloodUnit --no-build` | `Up()` tiga `CreateTable`, enam FK `Restrict`, tiga belas index; `Down()` tiga `DropTable` | `PASS` | Lalu satu index ditambahkan — lihat di bawah |
| `ef migrations has-pending-model-changes` pada build 2 | "No changes have been made to the model since the last migration." | `PASS` | Membuktikan migration, Designer, dan snapshot sama persis dengan model final |
| `ef migrations list` pada `QuilvianNewDevSukma` sebelum penerapan | 139 total, 138 terterapkan, **pending tepat 1: `AddBbkProviderRequestAndBloodUnit`** | `PASS` | Nol migration modul lain ikut terbawa |
| `ef database update 20260911032311_AddBbkProviderRequestAndBloodUnit` | `Done.`; sesudahnya **139 / 139, pending 0** | `PASS` | Pengaman skrip menolak eksekusi bila nama database bukan `QuilvianNewDevSukma` persis, atau bila pending bukan tepat satu milik task ini |

**Satu koreksi selama pengerjaan, dicatat apa adanya.** Sesudah migration dibangkitkan, pembacaan
ulang terhadap kamus data menemukan `BbkProviderRequest.BloodOrderId` wajib punya **index biasa**.
Index terfilter yang sudah ada hanya memuat permintaan yang masih berjalan, sehingga tidak melayani
pembacaan "seluruh permintaan milik order ini". Index biasa `IX_BbkProviderRequest_BloodOrderId`
ditambahkan pada configuration, migration, Designer, dan snapshot. Kebenarannya **tidak diandaikan**:
`has-pending-model-changes` pada build 2 menyatakan nol perubahan, dan penerapan ke PostgreSQL
berhasil.

Uji manual: `NOT FEASIBLE` — menuntut aplikasi berjalan beserta akun ber-hak-akses berbeda, dan
`AGENTS.md` melarang menjalankan aplikasi bila task tidak mewajibkan eksekusi runtime.

**Tidak dijalankan:**

| Pemeriksaan | Alasan |
| --- | --- |
| Panggilan HTTP ujung-ke-ujung dengan dua akun berbeda hak akses | `HasAccessAsync` meloloskan SuperAdmin sebelum satu baris hak akses dibaca, sehingga uji lewat Swagger menyesatkan. Penggantinya: contract test atribut hak akses dan test controller langsung, keduanya dijalankan |
| Uji PostgreSQL modul lain | Wewenang eksekusi hanya untuk uji yang diperlukan task ini |
| `BillingTests` | Tidak berkaitan dengan task ini |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-BD-005` — minta 3 PRC, diterima 2 hari pertama → `PartiallyFulfilled`, sisa 1 | **Terpenuhi** | `AC_BD_005_Minta3Prc_Diterima2HariPertama_PartiallyFulfilledSisa1` |
| `AC-BD-006` — permintaan belum dikirim, dibuat permintaan baru kebutuhan sama → ditolak `VAL-BD-006` | **Terpenuhi** | `AC_BD_006_PermintaanBelumDikirim_DibuatLagiUntukKebutuhanSama_Ditolak` (nomor pun tidak terbit) · `Controller_PermintaanGanda_Menjadi422_VAL_BD_006` · PostgreSQL: sepuluh permintaan serentak → **1 lahir, 9 ditolak**, dan index terfilter menolak permintaan berjalan kedua |
| `AC-BD-009` — permintaan dikirim, darah belum diterima fisik → stok tak bertambah | **Terpenuhi** | `AC_BD_009_PermintaanDikirim_DarahBelumDiterimaFisik_StokTidakBertambah` · `Controller_TanpaKlaimPengguna_TidakAdaPermintaanMaupunKantongYangLahir`. Ditegakkan struktural: tidak ada endpoint yang melahirkan kantong selain penerimaan |
| `AC-BD-022` — sisa 1 kantong saat kunjungan berakhir → `ClosedEncounter`, riwayat utuh | **Terpenuhi** | `AC_BD_022_Sisa1KantongSaatKunjunganBerakhir_ClosedEncounter_RiwayatUtuh` · `PenutupanSaatKunjunganMasihBerjalan_Ditolak`. Dipicu lewat service; pemicu otomatisnya belum ada — sama dengan kedaluwarsa order pada `BE-BD-003` |
| `AC-BD-023` — kantong datang setelah `ClosedEncounter` → penerimaan dicatat, kantong → `PendingReview` | **Diteruskan ke `BE-BD-015`** — roadmap revisi 9; bagian penerimaan terbukti di sini. **Riwayat:** sebagian | **Terbukti:** penerimaan dicatat, kantong membawa rujukan permintaan asal, permintaan tetap `ClosedEncounter` — `AC_BD_023_KantongDatangSetelahClosedEncounter_TetapDicatat_PermintaanTidakAktifLagi`. **Belum:** perpindahan kantong ke `PendingReview`. Matriks perpindahan status §3 (`v2`, `DEC-BD-036`) menetapkan kantong lahir `Received` dan masuk `PendingReview` **sesudah disimpan**; penyimpanan milik `BE-BD-015` |
| `AC-BD-031` — minta 2 PRC, datang 3 → `Fulfilled` sisa 0 (bukan −1); 3 kantong tercatat | **Terpenuhi** | `AC_BD_031_Minta2Prc_Datang3_FulfilledSisaNol_TigaKantongTercatat` · PostgreSQL: dua penerimaan serentak pada permintaan 3 kantong → keduanya dicatat, **4 kantong, tepat 1 berlebih**, `Fulfilled` |
| `AC-BD-032` — kantong ke-3 → `PendingReview` + alasan "kiriman melebihi permintaan", muncul di daftar #2 | **Diteruskan ke `BE-BD-015`** — roadmap revisi 9; penanda berlebih dan alasannya terbukti di sini. **Riwayat:** sebagian | **Terbukti:** kantong ketiga `IsExcess = true`, riwayatnya berbunyi "Kiriman melebihi permintaan.", balasan membawa `VAL-BD-014`, dan kantong berlebih dapat disaring — `AC_BD_032_KantongKetiga_DitandaiBerlebih_DenganAlasanKirimanMelebihiPermintaan` · `Controller_PenerimaanBerlebih_Menjadi200DenganPeringatan_VAL_BD_014`. **Belum:** status `PendingReview` dan kemunculannya di daftar #2 (`unitStatus=PendingReview`) — sebab yang sama dengan `AC-BD-023` |
| `AC-BD-033` — kantong berlebih dialokasikan langsung → ditolak `VAL-BD-033` | **Diteruskan ke `BE-BD-006`** — roadmap revisi 9. **Riwayat:** belum terpenuhi | Endpoint alokasi lahir pada `BE-BD-006`. Pada task ini **tidak ada satu jalur pun** yang dapat mengalokasikan kantong — detail kantong tidak menawarkan aksi apa pun — sehingga pelanggaran belum mungkin terjadi, tetapi penolakan `VAL-BD-033` belum punya source |
| `AC-BD-059` — kantong baru diterima dari PMI → `Received`, belum dapat dialokasikan | **Terpenuhi** | `AC_BD_059_KantongBaruDiterima_StatusReceived_BelumDapatDialokasikan` — status `Received`, aksi kosong, asal dan waktu terima tercatat |
| Sisa tidak pernah negatif di bawah penerimaan serentak (`BD-XINV-03`) | **Terpenuhi** | PostgreSQL `DuaPenerimaanSerentak_…` · `Version` naik tepat dua kali |
| Kelebihan dihitung per komponen | **Terpenuhi** | `KelebihanDihitungPerKomponen_PrcBerlebihTidakMenutupiTrombositYangKurang` · `KomponenYangTidakDiminta_TetapDicatatSebagaiBerlebih_SisaTidakBerubah` |
| Nomor kantong PMI unik | **Terpenuhi** | `NomorKantongPmiGanda_PadaSatuPenerimaanAtauSudahTercatat_Ditolak` · PostgreSQL `NomorKantongPmi_DijagaIndexUnikFisikDiDatabase` |
| Pembatalan beralasan terkendali | **Terpenuhi** | `PembatalanDenganAlasanTerkendali_Berhasil_TeksAlasanDisalinKeRiwayat` · `PembatalanTanpaAlasanTerkendali_Ditolak` (kosong, ketikan bebas, nonaktif) · `PembatalanDariLayarUsang_Ditolak409` |

### Definition of Done

| Butir DoD | Status |
| --- | --- |
| Seluruh AC roadmap lolos | **Terpenuhi sejak roadmap revisi 9** — keenam kriteria yang tetap milik task ini terbukti penuh; `AC-BD-023`/`032` kini milik `BE-BD-015`, `AC-BD-033` milik `BE-BD-006`. **Riwayat:** belum terpenuhi — 6 dari 9 terbukti penuh; `AC-BD-023` dan `AC-BD-032` sebagian, `AC-BD-033` belum. Ketiganya menuntut state yang lahir pada `BE-BD-015` dan `BE-BD-006`, bukan pekerjaan yang tertinggal di scope task ini |
| Sisa ≥ 0 dijaga token `Version` | **Terpenuhi** — dan dibuktikan di PostgreSQL |
| Kelebihan → `IsExcess` | **Terpenuhi** |
| `RequestNumber` dari number-series existing; `PmiBagNumber` dari PMI | **Terpenuhi** |
| Build lulus | **Terpenuhi** — `0 Error(s)`, `210 Warning(s)` = baseline |
| Test relevan lulus | **Terpenuhi** — 561 + 231 + 9; kegagalan `UnitTests.InMemory` bawaan dan terbukti tidak terkait |
| Migration berhasil di `QuilvianNewDevSukma` | **Terpenuhi** — `139/139`, pending 0 |
| Pending model changes diperiksa | **Terpenuhi** — nol perubahan |
| Laporan, roadmap, dan traceability tersinkron | **Terpenuhi** — berkas ini, `backend-roadmap.md`, `requirement-traceability.md`, `MODULE-STATUS.md` |

**Kenapa 🟡, bukan ✅.** Aturan status task menuntut setiap acceptance criteria terpetakan ke source
yang benar-benar ada. Tiga kriteria di atas menunjuk perpindahan status yang source-nya memang milik
task lain. Roadmap sudah dua kali memindahkan kriteria semacam ini ke task tempat penegakannya hidup
(`AC-BD-013` dari `BE-BD-002` ke `BE-BD-003`; `AC-BD-062/065/066/067` dari `BE-BD-014` ke
`BE-BD-015`), tetapi memindahkan acceptance criteria adalah **perubahan scope roadmap** — di luar
wewenang build skill. Keputusannya diserahkan kepada pemilik roadmap (bagian 8).

---

## 7. Delta kontrak

| Delta | Isi | Alasan |
| --- | --- | --- |
| **Nama tabel penerimaan** | `BbkBloodUnitReceipt`, bukan `BbkProviderReceipt` seperti tertulis pada kolom Scope kartu roadmap | Kamus data dan arsitektur backend kontrak `v4` sama-sama memakai `BbkBloodUnitReceipt`. Kontrak yang disetujui adalah sumber nama |
| **Tiga endpoint teknis pada Provider Request** | `GET /filters/metadata`, `GET /summary`, `GET /{id}/status-history` | Permukaan teknis baku standar endpoint transaksi *aggregate ber-lifecycle*, pola yang sama dengan `BE-BD-003`. Nol aturan bisnis baru |
| **Permukaan baca Blood Unit** | `GET /`, `GET /{id}` dari kontrak, ditambah `filters/metadata`, `summary`, `status-history` | `BloodUnit : Read` dipetakan ke `BE-BD-004` oleh daftar kontrak `BloodBankRoleAccessContractTests`, dan `AC-BD-009/059` menuntut kantong dapat dibaca. `GET /{id}/placements` milik `BE-BD-015`, `GET /{id}/corrections` milik `BE-BD-010`, dan penyaring `emergencyPendingEvidence` milik `BE-BD-008` — **tidak** dibuat |
| **Dua kolom `BbkBloodUnit` ditunda** | `CurrentPlacementId` dan `CompatibilityEvidenceIdUsed` | Keduanya FK ke tabel yang belum ada. Kolom `Scope` kartu `BE-BD-015` menyebut `BbkBloodUnit.CurrentPlacementId` sebagai pekerjaannya; `BbkCompatibilityEvidence` lahir pada `BE-BD-007`. Penjaga: `KolomTersimpan_SamaPersisDenganKamusData` |
| **Index unik terfilter `IX_BbkProviderRequest_BloodOrderId_Active`** | Unik atas `BloodOrderId` untuk `RequestStatus IN (0, 1)` yang tidak dihapus | Penjaga fisik `BD-XINV-02` — `02-backend-architecture.md` menyebut "pemeriksaan lintas-baris di service + unique guard". Index biasa `BloodOrderId` dari kamus data tetap ada |
| **Index unik `(ProviderRequestId, Sequence)`** | Tambahan builder | Urutan penerimaan dihitung di bawah kunci penasihat; index ini penjaga fisiknya |
| **"Kebutuhan yang sama" dibaca per order** | `VAL-BD-006` menahan permintaan kedua untuk **order** yang sama | Permintaan diturunkan dari satu order dan jumlahnya dari baris order itu. Dua order berbeda — termasuk order ganda yang dilanjutkan dengan alasan tertulis — adalah dua kebutuhan |
| **"Order aktif" dibaca dua arah** | Status order `Active`/`PartiallyFulfilled` **dan** kunjungannya belum berakhir | Tafsiran yang sama dengan penolakan order baru pada kunjungan berakhir di `BE-BD-003` |
| **Jumlah diminta dan kelebihan per komponen** | Diminta = jumlah baris order per komponen; kantong komponen yang tidak diminta seluruhnya berlebih | `02-backend-architecture.md` §F.1: "jumlah diminta diturunkan dari baris order". Baris order memang per komponen |
| **Kantong berlebih lahir `Received`** | Tidak langsung `PendingReview` | Matriks §3 `v2`: "Kantong tetap wajib disimpan lebih dulu". `INV-BD-017` revisi lama yang menulis "langsung masuk `PENDING_REVIEW`" sudah digantikan `DEC-BD-036` |
| **Alasan kelebihan sebagai salinan teks** | Riwayat kantong berlebih memuat `ReasonNote` "Kiriman melebihi permintaan." tanpa `ReasonCode` | Sistem tidak memilih kode alasan atas nama manusia. Kode alasan terkendali melekat saat kantong masuk `PendingReview` di `BE-BD-015` |
| **`VAL-BD-007` dipetakan pada order tanpa jumlah** | Karena jumlah diturunkan, tidak ada isian jumlah yang dapat kosong | Satu-satunya keadaan `VAL-BD-007` yang masih mungkin. `AC-BD-014` — "permintaan tanpa jumlah kantong" — **tidak dimiliki task backend mana pun** pada roadmap; dicatat sebagai temuan |
| **Nomor kantong ganda** | `422` tanpa kode `VAL-BD-*` | Kamus data menetapkan `PmiBagNumber` unik; matriks validasi tidak punya kode untuknya |
| **Format `RequestNumber`** | `PMI-00000001`, deret `BBK_PROVIDER_REQUEST`, 8 digit, `NEVER` | Milik modul (`QBE-CODE-005`); pola yang sama dengan `ORD` pada `BE-BD-003` |
| **Pembatalan permintaan memakai butir `Update`** | `BloodProviderRequest : Update` | Persis seperti api-contract `v4`, berbeda dengan `BloodOrder : Cancel` yang dipisah `DEC-BD-044` |
| **Waktu penerimaan** | Boleh kosong (berarti saat ini); ditolak bila lebih dari 5 menit di masa depan | Kamus data mewajibkan `ReceivedAt` sebagai waktu terima fisik; toleransi untuk selisih jam perangkat |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build solution `210 Warning(s)` pada kedua build, sama dengan baseline. Fixture PostgreSQL mencetak nama host database pada peringatan opt-in ke konsol lokal; nilainya **tidak** disalin ke laporan ini |
| Masalah yang diketahui | **(1)** `AC-BD-023`/`032` sebagian dan `AC-BD-033` belum — bagian 6. **(2)** Pemicu otomatis `ClosedEncounter` belum ada; perpindahannya tersedia di `BbkProviderRequestService.CloseForEncounterEndAsync` dan terbukti. **(3)** **Pertanyaan terbuka:** kantong yang datang **sesudah** permintaan `Fulfilled` ditolak, karena matriks §2 menyebut `Fulfilled` terminal dan hanya `ClosedEncounter` yang menerima kantong susulan. Ini dapat bertentangan dengan semangat `DEC-BD-025` "penerimaan tidak pernah ditolak karena kelebihan" bila PMI mengirim kantong tambahan dalam kiriman terpisah. **Butuh keputusan pemilik proses BDRS.** **(4)** **Pertanyaan terbuka:** kontrak tidak menetapkan kategori alasan untuk pembatalan permintaan PMI; task ini menerima alasan aktif mana pun dari daftar terkendali, sama seperti `BE-BD-011` untuk konflik golongan darah. **(5)** `AC-BD-014` tidak dimiliki task backend mana pun |
| Risiko tersisa | **(1)** Migration baru ada di `QuilvianNewDevSukma`; `QuilvianNewDevTim01`, staging, dan production belum. **(2)** Sampai pemicu `ClosedEncounter` ada, permintaan pada kunjungan yang sudah berakhir tetap tercatat berjalan — tetapi permintaan baru untuk order pada kunjungan berakhir sudah ditolak. **(3)** Uji PostgreSQL menaikkan pencacah deret `BBK_BLOOD_ORDER` dan `BBK_PROVIDER_REQUEST` di `QuilvianNewDevSukma` dan sengaja tidak dikembalikan (`INV-PLT-001`). **(4)** Uji PostgreSQL membaca satu `MstDoctor` yang sudah ada, karena membuat dokter berarti menulis ke tabel milik modul HR |
| Temuan di luar scope | **(1)** Kolom Scope kartu roadmap `BE-BD-004` memakai nama `BbkProviderReceipt` yang berbeda dari kontrak. **(2)** Sembilan kegagalan `BillingManagement` pada `UnitTests.InMemory` masih ada, milik pemilik Billing |
| Perubahan sampingan | **`NONE` pada hasil akhir.** Line ending sembilan belas berkas baru diseragamkan CRLF sebelum build 2; berkas tracked yang disunting tetap `w/crlf`. `HasDefaultValue(false)` pada dua kolom `bool` dibuang sebelum migration dibangkitkan, karena nilai bawaan CLR sudah `false` dan konfigurasi itu hanya memicu peringatan EF |
| Langkah pendahulu pada sesi yang sama | Atas perintah pemilik pekerjaan, `manage-module-blueprint` lebih dulu **hanya** menyegarkan metadata sesudah `BE-BD-003`: `backend_source_sha` `7fca34c` → `d07dcf3` beserta impact scan terbatas (21 berkas di luar `docs/`, nol berkas bukti peta kemampuan tersentuh), `build_evidence_sha` → `8e30aa9`, `skill_suite_version` → `1.18.0`. Revisi blueprint tetap `25`. Nol discovery, redesign, maupun perubahan scope |
| Interupsi | `NONE` |
| Status Git | Bagian 9 |
| Langkah berikutnya | **(1)** Keputusan pemilik roadmap: teruskan `AC-BD-023`/`032` (bagian `PendingReview`) ke `BE-BD-015` dan `AC-BD-033` ke `BE-BD-006`, mengikuti preseden `BE-BD-002`/`BE-BD-014` — sesudah itu `BE-BD-004` sah ✅ dan `BE-BD-015` terbuka. **(2)** `BE-BD-015` — penyimpanan kantong, jalur kritis. **(3)** `BE-BD-012`, terbuka bersamaan. **(4)** Jawaban dua pertanyaan terbuka di atas. **(5)** Penerapan migration ke database lain sebagai wewenang terpisah |

---

## 9. Status Git

```text
 M Migrations/ApplicationDbContextModelSnapshot.cs
 M Program.cs
 M Repositories/ApplicationDbContext.cs
 M Tests/QuilvianSystemBackend.Tests/HealthServices/BankDarah/MasterData/BloodBankRoleAccessContractTests.cs
 M docs/module-blueprints/bank-darah/MODULE-STATUS.md
 M docs/module-blueprints/bank-darah/blueprint-manifest.md
 M docs/module-blueprints/bank-darah/roadmap/backend-roadmap.md
 M docs/module-blueprints/bank-darah/roadmap/requirement-traceability.md
?? Areas/HealthServices/BloodBankManagement/Controllers/BbkBloodUnitController.cs
?? Areas/HealthServices/BloodBankManagement/Controllers/BbkProviderRequestController.cs
?? Areas/HealthServices/BloodBankManagement/DTOs/BloodBankCommonDtos.cs
?? Areas/HealthServices/BloodBankManagement/DTOs/BloodUnitDtos.cs
?? Areas/HealthServices/BloodBankManagement/DTOs/ProviderRequestDtos.cs
?? Areas/HealthServices/BloodBankManagement/Enums/BbkBloodUnitStatus.cs
?? Areas/HealthServices/BloodBankManagement/Enums/BbkProviderRequestStatus.cs
?? Areas/HealthServices/BloodBankManagement/Models/BbkBloodUnit.cs
?? Areas/HealthServices/BloodBankManagement/Models/BbkBloodUnitReceipt.cs
?? Areas/HealthServices/BloodBankManagement/Models/BbkProviderRequest.cs
?? Areas/HealthServices/BloodBankManagement/Services/BbkBloodUnitService.cs
?? Areas/HealthServices/BloodBankManagement/Services/BbkDisplayLabels.cs
?? Areas/HealthServices/BloodBankManagement/Services/BbkProviderRequestService.cs
?? Migrations/20260911032311_AddBbkProviderRequestAndBloodUnit.Designer.cs
?? Migrations/20260911032311_AddBbkProviderRequestAndBloodUnit.cs
?? Repositories/Configurations/HealthServices/BloodBankManagement/BbkBloodUnitConfiguration.cs
?? Repositories/Configurations/HealthServices/BloodBankManagement/BbkBloodUnitReceiptConfiguration.cs
?? Repositories/Configurations/HealthServices/BloodBankManagement/BbkProviderRequestConfiguration.cs
?? Tests/QuilvianSystemBackend.IntegrationTests.Postgres/BankDarah/ProviderRequestPostgresTests.cs
?? Tests/QuilvianSystemBackend.Tests/HealthServices/BankDarah/ProviderRequest/
?? docs/module-blueprints/bank-darah/task/report/backend/BE-BD-004.md
```

Seluruh baris milik task ini, kecuali `blueprint-manifest.md` dan sebagian `MODULE-STATUS.md` yang
milik langkah pendahulu penyegaran metadata pada sesi yang sama. Nol stage, commit, push, merge,
rebase, maupun deployment dilakukan.
