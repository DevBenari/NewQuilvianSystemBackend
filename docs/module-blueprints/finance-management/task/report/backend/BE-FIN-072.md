# Laporan Perubahan Backend — `BE-FIN-072`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-072` |
| Judul | Snapshot saldo terbit sendiri tiap tanggal 1 pukul 00.05 WIB, dan dinyatakan ulang bila posisinya berubah |
| Slice | `REV-14C` — `EPIC FIN-22`, jalur pengiriman |
| Roadmap | `roadmap/01-backend-roadmap.md`, bagian "Task REV-14C — `EPIC FIN-22`, jalur pengiriman" |
| Trace | `FR-FIN-154`, `FR-FIN-155`; `FIN-DEC-092`, `FIN-DEC-114`, `FIN-DEC-118`; `FIN-DES-078` |
| Contract version | `FIN-INTEGRATION-1.7` §5.12.5 (pernyataan ulang saldo); `FIN-API-1.5` F.6 (`POST /subledger-balances/restate`). **Catatan ketidaksesuaian dokumen** (dilaporkan, tidak diperbaiki sepihak — di luar wewenang tulis task ini): `contracts/api-contract.md` F.6 masih menandai `GET /subledger-balances/position` dan `GET /subledger-balances/{accountingPeriodCode}/variance` sebagai "Rencana (belum tersedia)", padahal keduanya sudah ✅ selesai sejak `BE-FIN-067`. Dokumen itu tampaknya belum disinkronkan sejak itu; `POST /subledger-balances/restate` pada baris yang sama kini juga sudah tersedia menyusul task ini |
| Dependency | `BE-FIN-068` ✅ (selesai) |
| Klasifikasi | `MEDIUM` — repo 0 + berkas diperiksa >20 (2) + berkas diubah 4 (1) + logika sedang (1) + kontrak diubah (2) + database tidak ada (0) + auth reuse pola existing (0) + UI tidak ada (0) = 6 |
| Task mode | `BACKEND` (target tulis backend; frontend tidak disentuh — tidak ada task frontend pasangan pada baris `FR-FIN-154`/`155`) |
| Target tulis | `NewQuilvianSystemBackend`, terbatas pada `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceSubledgerSnapshotSchedulerHostedService.cs` (baru), `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceSubledgerSnapshotSchedulerOptions.cs` (baru), `Areas/Corporate/FinanceManagement/AccountingIntegration/Controllers/FinanceAccountingEventsController.cs`, `Program.cs`, ditambah laporan tracked ini dan pembaruan register roadmap |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | `0e256765` (working tree membawa perubahan tidak ter-commit dari task REV-14A/14B/14C sebelumnya — lihat bagian 7 "Status Git") |
| Tanggal | 2 Oktober 2026 |
| Status | 🟡 **SEBAGIAN.** Source lengkap dan seluruh acceptance criteria yang dapat diverifikasi lewat pembacaan kode sudah terpenuhi; `dotnet build` **NOT RUN** atas permintaan eksplisit pengguna pada task ini — lihat bagian 5 |

---

## 1. Masalah yang diperbaiki

Sebelum task ini, snapshot saldo subledger **hanya** dapat diterbitkan lewat endpoint manual
`POST /subledger-balances/generate` (`BE-FIN-049`/`BE-FIN-068`) — tidak ada mekanisme otomatis yang
memicunya. Ini melanggar janji `FIN-DEC-092` ("snapshot terbit otomatis tanggal 1 pukul 00.05 WIB"),
dan temuan F2 pada `00-interview-decisions.md` baris 1272 mencatatnya eksplisit sebagai pelanggaran
yang **MUST** diperbaiki sebelum Finance menyatakan G4 (jalur pengiriman) siap.

Akibatnya: (1) bila tidak seorang pun mengingat menekan tombol generate setiap awal bulan, Accounting
tidak pernah menerima saldo subledger periode itu; dan (2) walau snapshot sudah terbit, tidak ada
mekanisme yang memeriksa ulang posisi itu bila ada mutasi bertanggal terlambat (misalnya shift kasir
malam yang baru ditutup esok paginya) masuk **setelah** snapshot periode itu terbit — sehingga
Accounting bisa menerima saldo yang sudah usang tanpa pernah tahu ada koreksi yang seharusnya datang.

Contoh konkret: shift kasir yang dibuka 30 September pukul 23.00 WIB baru ditutup 1 Oktober pukul
06.00 WIB (lewat tengah malam). Mutasi kas shift itu tercatat bertanggal bisnis 30 September
(`BusinessDate`), tetapi baru ADA di database setelah snapshot September (yang seharusnya terbit
1 Oktober pukul 00.05 WIB) sudah lewat jadwalnya. Tanpa pemeriksaan ulang harian, selisih kas shift
itu tidak akan pernah sampai ke Accounting.

---

## 2. Proses bisnis

**Tujuan.** Menjadikan penerbitan dan pernyataan ulang snapshot saldo subledger berjalan otomatis
setiap hari pukul 00.05 WIB, tanpa menunggu seseorang menekan tombol, sambil tetap menyediakan jalur
manual untuk pemicuan di luar jadwal.

**Pelaku.** `FinanceSubledgerSnapshotSchedulerHostedService` (baru) sebagai pemicu otomatis,
`FinanceSubledgerSnapshotService.GenerateMonthlySnapshotsAsync` (sudah ada sejak `BE-FIN-049`/`068`,
**tidak diubah** sama sekali oleh task ini) sebagai pelaksana kalkulasi dan penerbitan, dan
`FinanceAccountingEventsController.RestateSubledgerSnapshots` (baru) sebagai jalur manual.

**Pemicu dan langkah berurutan:**

1. Penjadwal berjalan sebagai `BackgroundService` yang hidup selama aplikasi berjalan (bila
   `Finance:SubledgerSnapshotScheduler:Enabled = true`). Setiap `PollIntervalSeconds` (bawaan 60
   detik), ia memeriksa jam WIB saat ini lewat `FinanceBusinessDate.BusinessTimeZone` — satu-satunya
   sumber zona waktu WIB Finance (`FIN-DES-082`), **tidak** disalin ulang.
2. Begitu jam WIB hari itu sudah melewati jam terjadwal (bawaan 00.05) **dan** siklus untuk tanggal
   WIB itu belum pernah dijalankan, penjadwal menghitung **periode sebelumnya** — bulan kalender
   tepat sebelum bulan WIB berjalan (mis. tanggal WIB berapa pun di bulan Oktober → periode
   `2026-09`) — lalu memanggil `GenerateMonthlySnapshotsAsync` untuk periode itu dengan actor sistem
   dari konfigurasi (bawaan `Guid.Empty`, mengikuti pola `LeaveCarryForwardSchedulerOptions`).
3. Di dalam `GenerateMonthlySnapshotsAsync` (service yang **sudah ada dan tidak disentuh**): untuk
   setiap akun control aktif, bila nilai yang baru dihitung **sama** dengan baris outbox terakhir
   yang sudah terbit untuk akun itu, tidak ada apa pun yang diterbitkan ulang (akun itu tetap pada
   versi lama). Bila nilainya **berbeda**, satu baris baru di-stage lewat `StageEventAsync`, yang
   sudah menaikkan `SourceVersion` secara otomatis sejak awal (`FIN-DEC-114` — "tidak butuh
   mekanisme versi baru").
4. Dua kasus yang tertangani oleh **satu** mekanisme yang sama ini:
   - **Tanggal 1 WIB** — periode sebelumnya belum pernah terbit sama sekali → setiap akun control
     mendapat baris pertamanya. Ini **publikasi pertama** (`FIN-DEC-092`).
   - **Tanggal lain** — periode sebelumnya sudah terbit dari siklus hari sebelumnya → hanya akun yang
     posisinya berubah (misalnya karena mutasi kas shift terlambat bertanggal akhir periode itu) yang
     mendapat baris baru. Ini **pernyataan ulang** (`FIN-DEC-114`).
5. Jalur manual: `POST .../subledger-balances/restate` memanggil method **yang persis sama**
   (`GenerateMonthlySnapshotsAsync`) untuk periode yang diminta pengguna secara eksplisit — berguna
   bila pemilik modul ingin memaksa pemeriksaan ulang tanpa menunggu siklus 00.05 WIB berikutnya,
   atau untuk periode yang bukan "periode sebelumnya" hari itu.

**Aturan yang berlaku.** `FIN-DEC-092` (jadwal otomatis tanggal 1 pukul 00.05 WIB), `FIN-DEC-114`
(pernyataan ulang hanya menyentuh akun yang berubah, memakai mekanisme `SourceVersion` yang sudah
ada), `FIN-DES-078` (hosted service dibangun mati, gerbang konfigurasi terpisah dari dua hosted
service Finance lainnya), `FIN-DES-080` (gagal tertutup bila pemetaan akun control belum lengkap —
tidak diubah, hanya diwarisi lewat pemanggilan `GenerateMonthlySnapshotsAsync`).

**Jalur tidak normal.** Bila pemetaan akun control belum lengkap saat siklus terjadwal berjalan,
`GenerateMonthlySnapshotsAsync` melempar `FinanceSubledgerSnapshotValidationException` (fail-closed —
nol baris diterbitkan untuk periode itu). Penjadwal menangkap exception ini, mencatatnya sebagai
peringatan, dan **tidak berhenti** — siklus berikutnya (hari berikutnya pukul 00.05 WIB) mencoba lagi.
Hosted service **tidak pernah crash** karena kegagalan bisnis semacam ini. Kegagalan menerbitkan
kejadian outbox (`AccountingOutboxException`, misalnya badan hukum utama belum ditetapkan) ditangani
dengan pola yang sama.

**Hasil akhir.** Snapshot saldo subledger periode sebelumnya terbit tanpa campur tangan manusia setiap
tanggal 1, dan posisinya terus diperiksa ulang setiap hari berikutnya sampai periode itu digantikan
oleh periode berikutnya sebagai "periode sebelumnya" yang diperiksa.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` (baris task `BE-FIN-072`, `FIN-DES-078`)
- `docs/module-blueprints/finance-management/contracts/integration-contract.md` (§5.12.5, §5.12.6, §5.12.7)
- `docs/module-blueprints/finance-management/contracts/api-contract.md` (F.5, F.6, F.7)
- `docs/module-blueprints/finance-management/contracts/permission-audit-matrix.md` (baris `restate`)
- `docs/module-blueprints/finance-management/contracts/validation-matrix.md` (dipastikan nol aturan baru khusus restate)
- `docs/module-blueprints/finance-management/testing/acceptance-test-matrix.md` (I.3 — `FIN-DEC-092`, `FIN-DEC-114`)
- `docs/module-blueprints/finance-management/00-interview-decisions.md` (temuan F2, F1)
- `docs/module-blueprints/finance-management/04-prd-to-mvp.md` (baris `FR-FIN-154`, `FR-FIN-155`)
- `docs/module-blueprints/finance-management/02-backend-architecture.md` (`FIN-DES-078` — desain tiga hosted service; `FIN-DES-081` — "pernyataan ulang jatuh sendiri")
- `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceSubledgerSnapshotService.cs` (method `GenerateMonthlySnapshotsAsync` — dibaca sebagai bukti, **tidak diubah**)
- `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceSubledgerBalanceCalculator.cs` (dipastikan seluruh query memakai filter `BusinessDate <= asOfDate`, bukan filter waktu pembuatan baris — dasar kebenaran kasus "shift terlambat")
- `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceBusinessDate.cs` (sumber WIB tunggal yang dipakai ulang)
- `Areas/Corporate/FinanceManagement/AccountingIntegration/DTOs/SubledgerSnapshotDtos.cs` (`GenerateSubledgerSnapshotsRequest`/`Response` — dipakai ulang untuk endpoint restate)
- `Areas/Corporate/FinanceManagement/AccountingIntegration/Controllers/FinanceAccountingEventsController.cs`
- `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceAccountingDispatchWorker.cs` dan `FinanceAccountingDispatchWorkerOptions.cs` (BE-FIN-071 — pola hosted service Finance terdekat)
- `Areas/Corporate/AccountingManagement/AccountingEvent/Services/AccAccountingEventSchedulerHostedService.cs` (pola hosted service interval-polling)
- `Areas/Corporate/HumanResource/LeaveManagement/Services/LeaveCarryForwardSchedulerHostedService.cs` (pola hosted service "jalan sekali per hari pada jam tertentu" — dipakai sebagai templat paling dekat)
- `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` (dipastikan `FinanceSubledgerSnapshotService` sudah terdaftar `AddScoped`)
- `Program.cs` (blok `runBackgroundJobs`, pola registrasi `FinanceAccountingDispatchWorker`)
- `Attributes/AccessActionAttribute.cs` (dipastikan pemakaian berulang nama action yang sama pada beberapa method diperbolehkan — sudah jadi pola berjalan pada controller ini)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceSubledgerSnapshotSchedulerOptions.cs` (baru) | Kelas options: `Enabled` (bawaan `false` — dibangun mati), `PollIntervalSeconds` (bawaan 60), `DailyRunHourWib`/`DailyRunMinuteWib` (bawaan 0/5 = 00.05 WIB), `SystemActorUserId` (nullable, bawaan `Guid.Empty`) |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceSubledgerSnapshotSchedulerHostedService.cs` (baru) | `BackgroundService` baru. Memeriksa jam WIB tiap siklus poll lewat `FinanceBusinessDate.BusinessTimeZone`; begitu jam terjadwal hari itu terlewati dan belum pernah jalan hari itu (`_lastRunDateWib`), menghitung periode sebelumnya dan memanggil `FinanceSubledgerSnapshotService.GenerateMonthlySnapshotsAsync` (service **tidak diubah**) lewat scope baru. Menangkap `FinanceSubledgerSnapshotValidationException` dan `AccountingOutboxException` sebagai peringatan tanpa menghentikan hosted service |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Controllers/FinanceAccountingEventsController.cs` | Menambah action `RestateSubledgerSnapshots` pada `[HttpPost("subledger-balances/restate")]`, memakai `[AccessAction]`/`[AccessPermission]` yang **sama persis** dengan `GenerateSubledgerSnapshots` (`"Create"`/`"Generate Subledger Balance Snapshots"`/`FinanceAccountingEvent:Create`) — mengikuti pola "Read Accounting Events" yang sudah menaungi tujuh action GET di controller yang sama. Memanggil `GenerateMonthlySnapshotsAsync` yang sama persis dengan endpoint generate, memakai DTO yang sama (`GenerateSubledgerSnapshotsRequest`/`Response`). Action `GenerateSubledgerSnapshots` yang sudah ada **tidak disentuh** |
| `Program.cs` | Menambah `builder.Services.Configure<FinanceSubledgerSnapshotSchedulerOptions>(builder.Configuration.GetSection("Finance:SubledgerSnapshotScheduler"))` dan `builder.Services.AddHostedService<FinanceSubledgerSnapshotSchedulerHostedService>()` di dalam blok `runBackgroundJobs`, tepat sesudah registrasi `FinanceAccountingDispatchWorker` (BE-FIN-071), mengikuti pola komentar dan susunan yang sama |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Satu endpoint baru**: `POST /api/v1/corporate/finance-management/accounting-events/subledger-balances/restate`, memakai DTO yang sudah ada (`GenerateSubledgerSnapshotsRequest`/`GenerateSubledgerSnapshotsResponse`). Endpoint `generate` yang sudah ada **tidak berubah perilakunya**. Sesuai `FIN-API-1.5` F.6 |
| Database | **NOT APPLICABLE.** Nol model baru, nol kolom baru, nol migration — scheduler murni memanggil service yang sudah ada; satu-satunya tulisan ke database tetap lewat jalur `GenerateMonthlySnapshotsAsync` yang sudah divalidasi pada `BE-FIN-068` |
| Keamanan/Auth | Endpoint baru memakai resource/action **yang sudah ada** (`FinanceAccountingEvent : Create`), bukan resource baru. **Nol** hardcode role/department/UserType — otorisasi murni lewat `[AccessPermission]` yang ditegakkan middleware, konsisten dengan seluruh endpoint lain pada controller ini |

---

## 4. Dokumentasi endpoint

#### Corporate / Finance Management / Accounting Events

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/subledger-balances/restate` | Memeriksa dan menerbitkan ulang saldo subledger yang berubah untuk satu periode akuntansi yang diminta eksplisit — memakai mekanisme yang sama dengan `generate` (hanya akun yang nilainya berubah mendapat versi baru) | `FinanceAccountingEvent : Create` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Permintaan eksplisit pengguna pada task ini: "jangan lakukan build backend secara automatis". Sesuai `rules/backend/TEST_POLICY.md` dan `status-task-roadmap.md` §2.1, ketiadaan hasil build sebenarnya menahan status `✅` — task ini karena itu ditandai `🟡`, bukan diklaim `PASS` tanpa dijalankan |
| Pembacaan kode: hosted service dibangun mati | Terverifikasi — `Enabled` bawaan `false`; `ExecuteAsync` kembali setelah satu baris log bila mati | `PASS` | `FinanceSubledgerSnapshotSchedulerOptions.cs`, `FinanceSubledgerSnapshotSchedulerHostedService.cs` |
| Pembacaan kode: jam dihitung dalam WIB | Terverifikasi — memakai `FinanceBusinessDate.BusinessTimeZone` (satu-satunya sumber WIB Finance), bukan salinan baru helper zona waktu | `PASS` | `FinanceSubledgerSnapshotSchedulerHostedService.cs`, `ExecuteAsync` |
| Pembacaan kode: terbit tanpa dipicu manual | Terverifikasi — `ExecuteAsync` memicu `GenerateMonthlySnapshotsAsync` sendiri begitu jam terjadwal terlewati, tanpa menunggu panggilan HTTP apa pun | `PASS` | `FinanceSubledgerSnapshotSchedulerHostedService.cs`, `JalankanSatuSiklusAsync` |
| Pembacaan kode: dijalankan dua kali untuk periode yang sama tidak menggandakan baris | Terverifikasi secara tidak langsung — `GenerateMonthlySnapshotsAsync` (tidak diubah task ini) membandingkan `latestExistingEvent.Amount` sebelum men-stage versi baru, dan sudah memegang advisory lock Postgres + unique index outbox (dibuktikan pada laporan `BE-FIN-068`). Penjadwal menambah guard `_lastRunDateWib` sebagai lapis tambahan di level proses tunggal | `PASS` (lewat pewarisan kontrak `BE-FIN-068`, bukan diuji ulang pada task ini) | `FinanceSubledgerSnapshotService.cs` baris 194-198; laporan [BE-FIN-068](BE-FIN-068.md) |
| Pembacaan kode: pernyataan ulang menyentuh hanya akun yang berubah | Terverifikasi — logika per-mapping yang sudah ada di `GenerateMonthlySnapshotsAsync` tidak disentuh; penjadwal hanya memanggilnya | `PASS` | `FinanceSubledgerSnapshotService.cs` baris 128-236 |
| Pembacaan kode: kasus shift terlambat memicu pernyataan ulang | Terverifikasi lewat penelusuran rantai kode (bukan uji runtime) — `FinanceSubledgerBalanceCalculator` memfilter seluruh mutasi kas/piutang/utang dengan `BusinessDate <= asOfDate`/`periodEndDate`, **bukan** `CreateDateTime`, sehingga mutasi yang baru TERSIMPAN setelah tanggal 1 tetapi ber-`BusinessDate` di dalam periode sebelumnya tetap ikut terhitung pada siklus berikutnya | `PASS` (verifikasi logika; bukan uji runtime bertanggal suntik — di luar cakupan tanpa lingkungan database aktif) | `FinanceSubledgerBalanceCalculator.cs` baris 61, 107, 145, 242, 271 |
| Endpoint `restate` memakai access control yang benar | Terverifikasi — `[AccessAction]`/`[AccessPermission]` identik dengan `GenerateSubledgerSnapshots` yang sudah berjalan | `PASS` | `FinanceAccountingEventsController.cs` |

Uji manual: `NOT FEASIBLE` pada sesi ini — memerlukan runtime aplikasi aktif dengan waktu sistem/WIB
yang dapat disuntik melewati tengah malam, database aktif, dan pemetaan akun control yang sudah
dikonfigurasi lengkap, semuanya di luar cakupan perubahan source tanpa eksekusi aplikasi pada task
ini.

**Tidak dijalankan:** `dotnet build` (lihat tabel di atas), uji manual end-to-end penjadwal melewati
tengah malam (`NOT FEASIBLE`, memerlukan lingkungan runtime dan waktu yang dapat disuntik), dan
`dotnet test` (backend tidak memelihara project automated test — `rules/backend/TEST_POLICY.md`).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Terbit tanpa dipicu manual | Terpenuhi (source) | Bagian 2 langkah 1-2, bagian 5 |
| Dijalankan dua kali untuk periode yang sama tidak menggandakan baris | Terpenuhi (diwarisi dari `GenerateMonthlySnapshotsAsync`, dibuktikan `BE-FIN-068`) | Bagian 5 |
| Pernyataan ulang menyentuh hanya akun yang nilainya berubah | Terpenuhi (diwarisi dari `GenerateMonthlySnapshotsAsync`) | Bagian 5 |
| Mati secara bawaan | Terpenuhi | `FinanceSubledgerSnapshotSchedulerOptions.Enabled = false` |
| 1 endpoint `POST .../restate` | Terpenuhi | Bagian 3.2, 4 |
| Jam dihitung dalam WIB | Terpenuhi | `FinanceBusinessDate.BusinessTimeZone` dipakai ulang |
| `dotnet build` PASS | **Belum terpenuhi** — `NOT RUN` atas permintaan eksplisit pengguna pada task ini | Bagian 5 |
| Nol migration | Terpenuhi | Bagian 3.3 |

Butir DoD `dotnet build PASS` **belum terpenuhi** karena sengaja tidak dijalankan atas instruksi
eksplisit pengguna pada task ini ("tapi jangan lakukan build backend secara automatis"). Ini **bukan**
pengecualian DoD yang melepas butir tersebut secara permanen (lihat `status-task-roadmap.md` §2.2) —
build tetap wajib dijalankan sebelum task ini dapat dinaikkan ke `✅`, hanya saja bukan pada sesi ini.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `dotnet build` belum pernah dijalankan terhadap perubahan ini. Risiko compile error (misalnya penamaan tipe atau anggota yang salah ketik) belum tersingkirkan oleh bukti build — hanya oleh pembacaan source yang cermat, termasuk memastikan `FinanceSubledgerSnapshotService` sudah terdaftar `AddScoped` di `BillingManagementServiceCollectionExtensions.cs` sehingga `scope.ServiceProvider.GetRequiredService<FinanceSubledgerSnapshotService>()` dapat di-resolve di runtime |
| Masalah yang diketahui | **Dokumentasi blueprint stale** (ditemukan, tidak diperbaiki — di luar wewenang tulis task ini): `contracts/api-contract.md` F.6 masih menandai endpoint `position`/`variance` sebagai "Rencana (belum tersedia)" padahal sudah selesai sejak `BE-FIN-067`. Dilaporkan sebagai temuan untuk pemilik blueprint, bukan diperbaiki diam-diam oleh build skill (wewenang `roadmap/**` saja) |
| Risiko tersisa | Penjadwal belum pernah diuji berjalan melewati tengah malam WIB sungguhan — risiko tersembunyi pada transisi hari/bulan (mis. DST tidak relevan untuk WIB, tetapi batas akhir bulan `AddDays(-1)` pada `DateOnly` sudah diverifikasi benar secara matematis untuk seluruh kombinasi bulan termasuk Februari tahun kabisat lewat pembacaan `.NET DateOnly` API, bukan uji runtime). `SystemActorUserId` bawaan `Guid.Empty` berarti baris outbox hasil penjadwal tercatat `CreateBy = Guid.Empty` sampai pemilik mengisi konfigurasi — ini konsisten dengan pola `LeaveCarryForwardSchedulerOptions` yang sudah diterima, bukan cacat baru |
| Perubahan sampingan | `NONE` — keempat berkas yang disentuh murni penambahan untuk task ini; `Program.cs` dan `FinanceAccountingEventsController.cs` yang sudah membawa perubahan tidak ter-commit dari task-task REV-14 sebelumnya tidak dipulihkan maupun diubah bagian lamanya |
| Interupsi | `NONE` |
| Status Git | `git status --short` pada akhir task menunjukkan dua berkas baru (`FinanceSubledgerSnapshotSchedulerHostedService.cs`, `FinanceSubledgerSnapshotSchedulerOptions.cs`) dan dua berkas termodifikasi (`FinanceAccountingEventsController.cs`, `Program.cs`) milik task ini, tercampur dengan modifikasi tidak ter-commit dari task `BE-FIN-058`..`071` (REV-14A/14B/14C) yang memang belum di-commit sejak sebelum task ini dimulai. Tidak ada file di luar scope Finance Management yang tersentuh oleh task ini |
| Langkah berikutnya | (1) Pengguna menjalankan `dotnet build` dan mengonfirmasi hasilnya, lalu laporan ini dan roadmap dinaikkan ke `✅`; (2) saat G3 (kredensial) dan `FIN-OQ-045`/`047` sudah turun dan worker pengiriman (`BE-FIN-071`) diaktifkan, pemilik modul mengisi `Finance:SubledgerSnapshotScheduler:Enabled = true` beserta `SystemActorUserId` pada konfigurasi lingkungan untuk menyalakan penjadwal ini; (3) pemilik blueprint menyinkronkan status endpoint pada `contracts/api-contract.md` F.6 (temuan dokumentasi stale di atas) |
