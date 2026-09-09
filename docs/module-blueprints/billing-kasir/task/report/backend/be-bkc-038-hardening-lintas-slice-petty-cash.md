# BE-BKC-038 — Hak akses, privasi, dan hardening lintas-slice Petty Cash

- TASK ID: `BE-BKC-038`
- TASK TYPE: Implementasi backend — capstone hardening lintas-slice (tanpa endpoint baru)
- COMPLEXITY: `MEDIUM` (skor 6 — repository 0, berkas diperiksa >20 → 2, berkas diubah 3 → 1, logika bisnis sedang (perbaikan audit log + query replay) → 1, kontrak API tidak berubah → 0, database tidak berubah → 0, keamanan pengujian RBAC end-to-end → 1, UI/workflow tidak ada → 0, evidence matrix lintas-slice → 1)
- CLASSIFICATION SCORE: 6
- MODEL: Claude Sonnet 5
- TASK MODE: `BACKEND` — repository `NewQuilvianSystemBackend`, branch `Yasmina`
- WRITE TARGET: Source backend (`Areas/HealthServices/BillingManagement/PettyCash/Services/PettyCashVoucherService.cs`), test backend (`Tests/QuilvianSystemBackend.UnitTests.InMemory/BillingManagement/PettyCashHardeningTests.cs` baru, `AccessPermissionEnforcementTests.cs` diperluas), dan laporan task ini

## 1. Apa yang dikerjakan dan kenapa

**Tujuan bisnis.** Ini adalah task penutup (capstone) rumpun Petty Cash: membuktikan — bukan
mengasumsikan — bahwa (a) hak akses seluruh sepuluh+empat+sembilan endpoint tiga controller baru
benar-benar tersambung ke layar Akses Role, (b) data sensitif (`RecipientName`, `Purpose`,
`RejectionReason`) tidak pernah bocor ke log aplikasi, dan (c) yang paling penting: kas kecil
**tidak pernah** menyentuh kas fisik shift kasir walau satu rupiah pun. Task ini murni verifikasi
dan hardening — tidak ada endpoint, model, atau migration baru.

**Temuan sesi ini (bukan cuma menulis test yang sudah pasti lulus).** Task ini menemukan dan
memperbaiki dua cacat nyata:

1. **BIL-AT-079 sebelumnya tidak benar-benar terbukti dapat lulus.** `LoggerService.WriteAsync`
   (infrastruktur bersama seluruh modul, bukan milik Petty Cash) hanya mencetak field bernama
   `Path`/`Method`/`Ip`/`UserId`/`Username`/`Email` yang diekstrak dari parameter `data` — properti
   lain pada objek anonim yang dikirim `AuditCommandAsync` (`VoucherId`, `Amount`,
   `StatusBefore`/`After`) **tidak pernah tercetak di mana pun**, murni *write-only*. Akibatnya
   klaim BIL-AT-079 "log memuat VoucherId, VoucherNumber, nominal, status, ActorUserId" sebelumnya
   tidak dapat dibuktikan benar oleh test apa pun — bukan karena datanya bocor, tapi karena
   sebaliknya: identitasnya sendiri juga tidak pernah tercetak. Diperbaiki dengan membentuk field
   non-sensitif itu langsung ke teks `message` (yang **memang** tercetak apa adanya oleh
   `LoggerService`), bukan lewat `data` — tanpa mengubah `LoggerService` itu sendiri, yang dipakai
   modul lain (§ 6).
2. **Regresi `NullReferenceException` pada jalur replay idempotency**, ditemukan lewat regresi
   otomatis (bukan lewat perbaikan #1 secara langsung, tapi konsekuensinya): `AuditCommandAsync`
   yang kini membaca `command.Voucher.VoucherNumber` dipanggil juga dari `ReplayAsync` dengan
   `command` (`prior`) yang dimuat `AsNoTracking()` **tanpa** `.Include(x => x.Voucher)` —
   `Voucher` bernilai `null` pada command hasil query itu. Diperbaiki dengan menambahkan
   `.Include(x => x.Voucher)` pada satu query di `ReplayAsync` (§ 5).

## 2. Proses bisnis

Tidak ada proses bisnis baru — task ini memverifikasi proses bisnis yang **sudah** diimplementasikan
`BE-BKC-034`–`037` benar-benar berperilaku sesuai kontrak yang terkunci, lewat tiga sudut:

- **Kewenangan.** Kepala Kasir/Finance Operations menyetujui, kasir mencairkan, siapa pun boleh
  membaca — tetapi hanya bila admin sudah mencentang butir hak aksesnya di layar Akses Role.
  Task ini membuktikan butir itu benar-benar bisa dicentang (pasangan atribut benar) DAN benar-benar
  ditegakkan (401/403 sungguhan, bukan simulasi).
- **Privasi.** Nama penerima, tujuan pengeluaran, dan alasan tolak/batal adalah data personal/
  sensitif secara operasional — tidak boleh ada di log aplikasi yang diakses tim infra/observability
  lebih luas dari tim Finance.
- **Integritas keuangan lintas-modul.** Kas kecil dan kas shift kasir adalah dua kolam uang yang
  terpisah total secara desain (`PC-DEC-001`) — task ini membuktikannya berangka, bukan hanya
  membaca kode dan menyimpulkan.

## 3. Health Services / Billing Management / Petty Cash

Tidak ada endpoint baru atau berubah. Base URL ketiga controller (`.../petty-cash-categories`,
`.../petty-cash/budget`, `.../petty-cash/vouchers`) dan seluruh kontrak request/response tetap
persis seperti `BE-BKC-035`/`036`/`037`.

## 4. Berkas yang diperiksa

Governance dan kontrak: `AGENTS.md`, `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`,
`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`.

Blueprint modul: `roadmap/backend-roadmap.md` (kartu `BE-BKC-038`), `contracts/permission-audit-matrix.md`
(Resource `PettyCashCategory`/`PettyCashBudget`/`PettyCashVoucher`), `testing/acceptance-test-matrix.md`
(`BIL-AT-064`–`080` rinci per baris).

Source diperiksa (bukan diubah, dijadikan bukti "sudah benar"): ketiga controller
(`PettyCashCategoriesController.cs`, `PettyCashBudgetController.cs`, `PettyCashVouchersController.cs`)
— seluruh 23 action-nya diperiksa satu per satu lewat refleksi otomatis, bukan dibaca manual;
`Seeders/AccessMenuSeeder.cs` (dikonfirmasi membentuk baris Akses Role **langsung** dari atribut
`[AccessController]`/`[AccessAction]` lewat refleksi runtime atas `IActionDescriptorCollectionProvider`
— **tidak ada** daftar seeder terpisah yang bisa menyimpang dari atribut, berbeda dari kelas cacat
yang pernah ditemukan `BE-RWI-034` pada modul lain); `Services/Logging/LoggerService.cs` (dibaca
saja, TIDAK diubah — lihat § 6); `CashierShiftService.cs` (`ApplyCashReceiptAsync`/`CloseAsync`,
dipakai apa adanya untuk regresi § 8).

Pola implementasi terdekat yang dipakai apa adanya: `AccessPermissionEnforcementTests.cs`
(`BE-BKC-017`/`BIL-AT-022` — RBAC diuji lewat `AccessPermissionService`/`AccessPermissionFilter`
SUNGGUHAN, bukan reflection atas atribut semata); `InpatientRoleAccessContractTests.cs`
(`BE-RWI-034` — pola refleksi generik atas seluruh action controller); `CashierShiftServiceTests.cs`
(pola `ApplyCashReceiptAsync`+`CloseAsync` untuk membentuk shift dengan saldo diketahui).

## 5. Berkas yang diubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BillingManagement/PettyCash/Services/PettyCashVoucherService.cs` | **Diubah.** `AuditCommandAsync`: field non-sensitif (`VoucherId`, `VoucherNumber`, `Status`, `Amount`, `IsReplay`) dipindah dari parameter `data` (yang ternyata *write-only*, § 1) ke teks `message` (yang benar-benar tercetak); `ActorUserId` dikirim sebagai `Id` pada `data` agar terekstrak `LoggerService` ke field `UserId` yang sudah ada. `ReplayAsync`: satu `.Include(x => x.Voucher)` ditambahkan pada query `prior` agar `AuditCommandAsync` tidak `NullReferenceException` saat memutar ulang command lama (§ 1 butir 2) |
| `Tests/QuilvianSystemBackend.UnitTests.InMemory/BillingManagement/PettyCashHardeningTests.cs` | **Baru.** Enam unit test: pasangan `[AccessAction]`/`[AccessPermission]` seluruh 23 action tiga controller (refleksi generik, `BIL-AT-078` bagian 1), tidak ada `PUT`/`PATCH`/`DELETE` pada Voucher/Budget (`BIL-AT-072`) dibanding Category yang justru **wajib** punya ketiganya (master data), regresi `BilCashierShift` (`BIL-AT-077`), dan privasi log lintas tiga skenario (lifecycle penuh + reject + cancel, `BIL-AT-079`) |
| `Tests/QuilvianSystemBackend.UnitTests.InMemory/BillingManagement/AccessPermissionEnforcementTests.cs` | **Diperluas** (milik `BE-BKC-017`, direuse sesuai Scope task ini). Empat `[Fact]` baru: `Approve`/`TopUp`/`Read` Petty Cash ditolak `403` lewat `AccessPermissionService.HasAccessAsync` SUNGGUHAN tanpa butir hak akses (`BIL-AT-078` bagian 2), dan satu kontrol positif (diberi izin eksplisit → diterima) |

Tidak ada model, migration, DTO, atau endpoint baru. `PettyCashCategoryService`/
`PettyCashBudgetService`/kedua controller lain **tidak disentuh sama sekali** — sudah benar sejak
`BE-BKC-035`/`036`.

## 6. Implementasi

- **Kenapa `LoggerService.cs` sendiri TIDAK disentuh.** `WriteAsync` adalah infrastruktur bersama
  seluruh modul (Cashier, Billing Invoice/Deposit/Settlement/WriteOff, dan lainnya). Mengubah
  perilaku intinya (misalnya membuat seluruh `data` tercetak) berdampak ke setiap pemanggil
  `LoggerService` di seluruh aplikasi — jauh di luar proporsi task hardening satu submodule.
  Perbaikan BIL-AT-079 karena itu dilakukan seluruhnya di sisi pemanggil (`AuditCommandAsync`),
  memakai kontrak `LoggerService` yang **sudah ada** (`message` tercetak apa adanya) alih-alih
  menambah kontrak baru.
- **Kenapa pengujian pasangan atribut (refleksi) sudah cukup untuk BIL-AT-078 bagian 1** — bukan
  perlu daftar terpisah yang disinkronkan manual dengan `AccessMenuSeeder` seperti
  `InpatientRoleAccessContractTests`. `AccessMenuSeeder.SeedAsync` (dibaca ulang sesi ini)
  membentuk baris `SysControllerAccess`/`SysActionAccess` **langsung** dari
  `IActionDescriptorCollectionProvider` + refleksi atribut — sumber datanya sama persis dengan yang
  diperiksa test ini. Tidak ada kelas bug "terdaftar di tempat lain, tidak match" yang mungkin
  terjadi di sini.
- **Kenapa `BIL-AT-077` diuji lewat perbandingan berangka langsung (bukan dua skenario paralel).**
  `CloseAsync` menghitung `Variance = PhysicalCash − (OpeningCash + SystemCash)` — murni fungsi dari
  tiga field yang sama sekali tidak disentuh `PettyCashVoucherService`. Test membuktikan
  `shift.SystemCash` **identik** sebelum dan sesudah voucher kas kecil dicairkan, lalu menutup shift
  dan membuktikan `Variance = 0` — persis seperti voucher itu tidak pernah ada.
- **Kenapa `BIL-AT-080` butir (a) (kolam aktif kedua ditolak unique index parsial) TIDAK
  mendapat test baru sesi ini.** `IX_BilPettyCashBudget_ActiveSingleton` adalah unique index
  parsial pada level database — provider `InMemory` yang dipakai seluruh test unit modul ini TIDAK
  menegakkan unique index sama sekali (batasan provider, sama seperti `pg_advisory_xact_lock` pada
  `BIL-AT-066`/`071`). Menulis test InMemory untuknya akan lulus secara palsu (tidak benar-benar
  menguji constraint-nya) — lebih buruk daripada tidak ada test. Butir (b) dan (c) `BIL-AT-080`
  (koreksi ke negatif/di bawah komitmen) **sudah** tercakup `PettyCashBudgetServiceTests`
  (`BE-BKC-036`, tidak diubah sesi ini).
- **Kenapa `BIL-AT-075` (voucher `Uang Diterima` dibiarkan tanpa nota, waktu dimajukan) TIDAK
  mendapat test baru.** Tidak ada satu pun job latar/scheduler untuk Petty Cash pada codebase ini —
  ketiadaannya sendiri **adalah** buktinya (tidak ada kode yang bisa mengeskalasi, sehingga tidak
  mungkin ada perubahan status otomatis). Menulis test "memajukan waktu lalu memastikan tidak
  terjadi apa-apa" tanpa job yang benar-benar berjalan hanya akan menguji ketiadaan kode yang sudah
  jelas dari pembacaan source — dicatat sebagai bukti dokumentasi (§ 8), bukan test eksekusi.

## 7. Backend Governance Preflight

| Aspek | Nilai |
| --- | --- |
| Area | HealthServices |
| Module | BillingManagement / Billing |
| Submodule | PettyCash (mengikuti baris registry `Bil`, sama seperti `BE-BKC-033`–`037`) |
| Prefix entity | `Bil` — tidak ada entity baru |
| Keberlakuan | `NEW CODE` — perbaikan defect pada service yang sudah ada (`BE-BKC-037`) plus test hardening baru; tidak ada model/migration baru |
| Status registry | `ACTIVE`; `QBE-MOD-002`/`003` tidak berlaku |
| QBE ID yang berlaku | `QBE-SVC-001` — tidak berlaku langsung (tidak ada controller baru); `QBE-CODE-003` (dilarang mekanisme baru yang menduplikasi yang sudah ada) — dipatuhi, `AccessPermissionService`/`AccessPermissionFilter`/`CashierShiftService.ApplyCashReceiptAsync` dipakai apa adanya, tidak ditulis ulang |

## 8. Dampak kontrak API

Tidak ada. Task ini tidak mengubah satu pun endpoint, request, atau response. Evidence matrix
`BIL-AT-064`–`080` (DoD task ini):

| Test | Status | Bukti |
| --- | --- | --- |
| `BIL-AT-064` | `Covered` | `PettyCashVoucherServiceTests.FullLifecycle_SubmitApproveDisburseAttachProof_MovesThroughEveryStatus` (`BE-BKC-037`) |
| `BIL-AT-065`,`066` | `Covered` | `BillingNumberSeriesServiceTests.PettyCashVoucherNumber_*` (`BE-BKC-034`); concurrency sepuluh paralel diuji, `pg_advisory_xact_lock` sendiri hanya dapat dibuktikan provider relational |
| `BIL-AT-067`–`070` | `Covered` | `PettyCashVoucherServiceTests`/`PettyCashBudgetServiceTests` — contoh berangka persis kontrak (`BE-BKC-036`/`037`) |
| `BIL-AT-071` | `Partial` | Replay `Idempotency-Key` sama dan `Idempotency-Key` kosong tercakup; concurrency dua permintaan **benar-benar** bersamaan hanya dapat dibuktikan provider relational (`pg_advisory_xact_lock` dilewati provider `InMemory`) |
| `BIL-AT-072` | `Covered` **(baru sesi ini)** | `PettyCashHardeningTests.VoucherDanBudgetController_TidakPunyaEndpointPutPatchAtauDelete` + kontrol pembanding `PettyCashCategoryController_MasterDataSehinggaBolehPunyaPutPatchDelete` |
| `BIL-AT-073` | `Covered` | `PettyCashVoucherServiceTests.Cancel_*` (`BE-BKC-037`) |
| `BIL-AT-074` | `Covered` | Tercakup transitif oleh `FullLifecycle_*` — satu panggilan `DisburseAsync` langsung `CASH_RECEIVED`, tidak ada status antara pada state machine |
| `BIL-AT-075` | `Covered (by design absence)` | Tidak ada job latar Petty Cash sama sekali pada codebase — didokumentasikan § 6, bukan diuji eksekusi |
| `BIL-AT-076` | `Covered` | `PettyCashCategoryServiceTests.Delete_CategoryUsedByVoucher_ThrowsInUseException`/`UpdateStatus_*` (`BE-BKC-035`) |
| `BIL-AT-077` | `Covered` **(baru sesi ini)** | `PettyCashHardeningTests.PettyCashVoucherDisbursement_DoesNotMoveCashierShiftTotals` — uji regresi paling penting seluruh rumpun ini |
| `BIL-AT-078` | `Covered` **(baru sesi ini)** | `PettyCashHardeningTests.SetiapActionPadaKetigaController_AccessActionDanAccessPermissionSamaPersis` (bagian 1, atribut) + empat `[Fact]` baru `AccessPermissionEnforcementTests` (bagian 2, RBAC sungguhan) |
| `BIL-AT-079` | `Covered` **(baru sesi ini, dan memperbaiki cacat § 1)** | `PettyCashHardeningTests.FullLifecycleRejectAndCancel_NeverLogSensitiveFields_ButLogIdentifyingFields` |
| `BIL-AT-080` | `Partial` | Butir (b)/(c) `Covered` (`BE-BKC-036`); butir (a) (unique index parsial) hanya dapat dibuktikan provider relational (§ 6) |

Regresi empat jenis nomor existing (invoice/deposit/settlement/cashier-shift): **tidak terpengaruh**
— tidak ada perubahan pada `BillingNumberSeriesService.cs` sesi ini; `ExistingNumberTypes_KeepTheirFormats_AfterPettyCashExtension`
(`BE-BKC-034`) tetap berlaku tanpa perubahan.

## 9. Dampak database

Tidak ada. Tidak ada migration, model, atau perubahan schema.

## 10. Dampak keamanan

**Ini adalah task keamanan itu sendiri.** Dibuktikan sesi ini, bukan diasumsikan:

- Seluruh 23 action tiga controller (`PettyCashCategory`: 9, `PettyCashBudget`: 4,
  `PettyCashVoucher`: 10) memiliki `[AccessAction]` DAN `[AccessPermission]` dengan argumen yang
  sama persis satu sama lain dan dengan `ControllerName` pada `[AccessController]` — diverifikasi
  otomatis lewat refleksi, bukan dibaca manual satu per satu.
- `AccessType` pada seluruh 23 action bernilai salah satu dari `Read`/`Create`/`Update`/`Delete`.
- Pengguna tanpa butir hak akses eksplisit ditolak `HasAccessAsync` (lapis service, sama dengan
  yang dipakai `AccessPermissionFilter` sungguhan saat request HTTP masuk) untuk `Approve`
  voucher, `TopUp` anggaran, dan `Read` daftar voucher — tiga skenario yang eksplisit disebut
  `BIL-AT-078`.
- `RecipientName`, `Purpose`, `RejectionReason` (dari REJECT maupun CANCEL) dibuktikan **tidak
  pernah** tercetak ke log aplikasi lewat pencarian teks pada SELURUH keluaran `ILogger` yang
  benar-benar tertulis (bukan hanya memeriksa parameter yang dikirim ke logger) — mencakup tiga
  jalur (lifecycle selesai, reject, cancel) sekaligus dalam satu percobaan.
- Kas shift kasir (`BilCashierShift.SystemCash`) dibuktikan berangka **sama persis** sebelum dan
  sesudah voucher kas kecil dicairkan.

- VISUAL REFERENCE: NOT REQUIRED (tidak ada perubahan UI pada task backend ini)

## 11. Validasi

| Perintah/pemeriksaan | Hasil | Klasifikasi | Bukti/catatan |
| --- | --- | --- | --- |
| `dotnet build` | **LULUS** (0 Error, warning pre-existing saja) | `PASS` | Dijalankan sesi ini setelah seluruh perubahan `AuditCommandAsync`+`ReplayAsync` |
| `dotnet test --filter PettyCashHardeningTests\|AccessPermissionEnforcementTests` | **LULUS 24/24** | `PASS` | Enam test baru + empat test baru + 14 test `AccessPermissionEnforcementTests` existing |
| `dotnet test` gabungan (`PettyCash*`+`BillingNumberSeriesServiceTests`+`AccessPermissionEnforcementTests`+`CashierShiftServiceTests`) — 89 test | **1 GAGAL ditemukan** (`NullReferenceException` pada `Create_SameIdempotencyKeyTwice_ReturnsReplayWithoutSecondVoucher`) → **diperbaiki** (`.Include(x => x.Voucher)`, § 1 butir 2 dan § 5) → **dikonfirmasi lulus ulang, 89/89** | `PASS` | Perbaikan `.Include` dijalankan ulang dan dikonfirmasi lulus penuh (0 gagal) pada percobaan berikutnya |
| Review diff/scope | Dilakukan | `PASS` | `git status --short` menunjukkan hanya tiga berkas dalam lingkup task ini yang berubah (`PettyCashVoucherService.cs`, `PettyCashHardeningTests.cs` baru, `AccessPermissionEnforcementTests.cs` diperluas), plus berkas laporan/roadmap. Satu berkas frontend (`frontend-roadmap.md`) ditemukan termodifikasi 118 baris di luar kendali sesi ini — **tidak disentuh**, dicatat sebagai temuan (§ 13) |
| Review kesesuaian QBE | Dilakukan | `PASS` | `QBE-CODE-003` dipatuhi — seluruh mekanisme RBAC/logging/cashier-shift dipakai apa adanya, tidak ditulis ulang |
| Pemeriksaan rahasia | Dilakukan | `PASS` | Tidak ada credential/token/connection string pada berkas yang berubah |

`dotnet build` dan `dotnet test` (89/89, mencakup seluruh test Petty Cash + Number Series +
Access Permission + Cashier Shift) sudah diverifikasi lulus penuh sesi ini (8 September 2026),
termasuk konfirmasi ulang setelah perbaikan `.Include`. Task ini memenuhi Definition of Done-nya
sendiri.

- MANUAL TEST: NOT APPLICABLE (task backend murni, tidak ada UI untuk diuji manual)

## 12. Peringatan dan risiko yang tersisa

- `dotnet build`/`dotnet test` (89/89) sudah dikonfirmasi lulus penuh sesi ini, termasuk setelah
  perbaikan `.Include` (§ 11). Task ini ditandai `✅` pada roadmap.
- `BIL-AT-071` (concurrency dua permintaan benar-benar bersamaan) dan `BIL-AT-080` butir (a)
  (unique index parsial kolam anggaran) hanya dapat dibuktikan pada provider PostgreSQL — di luar
  jangkauan unit test `InMemory`, konsisten dengan keterbatasan yang sudah didokumentasikan
  `BE-BKC-034`/`036`.
- `BE-BKC-033` (tabel dasar) dan `BE-BKC-034` (penomoran, source+test lengkap tapi belum punya
  laporan task tersendiri) masih 🟡 pada roadmap — **tidak menghalangi** task ini (permission
  wiring, privasi log, dan regresi kas shift sama sekali tidak bergantung pada status keduanya),
  tetapi tetap tercatat sebagai gerbang MVP-13 yang belum tertutup.
- Ditemukan **satu berkas frontend** (`docs/module-blueprints/billing-kasir/roadmap/frontend-roadmap.md`)
  termodifikasi 118 baris di luar kendali sesi backend ini — kemungkinan pekerjaan frontend paralel
  yang sedang berjalan. **Tidak disentuh, tidak dibatalkan** — di luar WRITE TARGET task ini
  sepenuhnya (roadmap frontend, bukan backend).

## 13. Perubahan sampingan

- INCIDENTAL CHANGES: `AccessPermissionEnforcementTests.cs` (milik `BE-BKC-017`) diperluas dengan
  empat `[Fact]` baru — **bukan** perubahan di luar cakupan, task ini secara eksplisit
  mem-Reuse "Test harness modul" pada kartu roadmap-nya sendiri, dan perluasan test tidak mengubah
  satu baris pun kode/test yang sudah ada di berkas itu.
- Satu regresi ditemukan DAN diperbaiki dalam sesi yang sama (§ 1 butir 2, § 5) — bukan
  perubahan sampingan yang dibiarkan, melainkan bagian dari hardening yang dituntut task ini
  sendiri.

## 14. Interupsi

- INTERRUPTIONS: SATU. Sesi diminta menghentikan pemanggilan `dotnet build`/`dotnet test` sendiri
  ("untuk test ga usah di build, langsung selesaikan tugasnya saja") tepat saat satu proses
  verifikasi ulang (setelah perbaikan `.Include`) masih berjalan di latar belakang sejak sebelum
  instruksi itu tiba. Sesi tidak memulai pemanggilan `dotnet build`/`test` baru sesudahnya, tetapi
  proses yang sudah berjalan sebelum instruksi itu diperiksa hasilnya begitu selesai (bukan
  pemanggilan baru) — hasilnya 89/89 lulus, dipakai apa adanya pada § 11 tanpa mengulang atau
  menyunting ganda.

## 15. Status Git

```
 M Areas/HealthServices/BillingManagement/PettyCash/Services/PettyCashVoucherService.cs
 M Tests/QuilvianSystemBackend.UnitTests.InMemory/BillingManagement/AccessPermissionEnforcementTests.cs
?? Tests/QuilvianSystemBackend.UnitTests.InMemory/BillingManagement/PettyCashHardeningTests.cs
```

(Baris lain pada `git status --short` sesi ini — `BillingFinancialExceptionServiceTests.cs`,
`BillingNumberSeriesServiceTests.cs`, `PettyCashVoucherServiceTests.cs`, roadmap/laporan
`BE-BKC-035`/`036`/`037` — berasal dari sesi verifikasi sebelumnya (`BE-BKC-037`), belum
di-commit, bukan bagian task ini. `frontend-roadmap.md` bukan bagian task ini sama sekali, lihat
§ 12.) Belum di-stage maupun di-commit. Branch `Yasmina`.

## 16. Langkah berikutnya yang disarankan

1. ~~Pengguna menjalankan `dotnet build` dan `dotnet test`~~ — **selesai sesi ini (8 September
   2026)**: build LULUS, 89/89 test LULUS (termasuk konfirmasi ulang setelah perbaikan `.Include`).
   Lihat § 11.
2. Roadmap (`backend-roadmap.md` kartu `BE-BKC-038`, dan `requirement-traceability.md`) diperbarui
   menjadi `✅` pada sesi ini, ditautkan ke laporan ini; evidence matrix `BIL-AT-064`–`080`
   diperbarui (§ 8).
3. Rumpun Petty Cash (`BE-BKC-033`–`038`) akan **selesai penuh** begitu `BE-BKC-033` (bukti
   seed/review Finance) dan `BE-BKC-034` (laporan task tersendiri) menyusul — keduanya di luar
   scope task ini tetapi menjadi satu-satunya sisa pekerjaan rumpun ini.

- KNOWN ISSUES: `BIL-AT-071`/`075`/`080a` memiliki keterbatasan yang didokumentasikan (§ 6, § 12),
  bukan celah yang dilewatkan.
- STALE EVIDENCE / BLOCKED PHASES: NOT APPLICABLE (bukan `MODULE BLUEPRINT MODE`)
- BLUEPRINT STATUS/EVIDENCE: NOT APPLICABLE (bukan `MODULE BLUEPRINT MODE`)
