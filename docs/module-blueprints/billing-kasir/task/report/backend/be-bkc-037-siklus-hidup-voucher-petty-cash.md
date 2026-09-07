# BE-BKC-037 — Siklus hidup voucher kas kecil penuh

- TASK ID: `BE-BKC-037`
- TASK TYPE: Implementasi backend — aggregate transaksi ber-lifecycle baru (10 endpoint)
- COMPLEXITY: `HEAVY` (skor 11 — repository 0, berkas diperiksa >20 → 2, berkas diubah 6 → 2, logika bisnis kompleks/lifecycle+concurrency+idempotency → 2, kontrak API memakai kontrak yang sudah dikunci → 1, database dampak perilaku persistence signifikan lintas tiga tabel → 1, keamanan tujuh permission baru mengikuti pola baku → 1, UI/workflow tidak ada → 0, dinaikkan satu tingkat karena tiga faktor jatuh di tingkat berikutnya)
- CLASSIFICATION SCORE: 11
- MODEL: Claude Sonnet 5
- TASK MODE: `BACKEND` — repository `NewQuilvianSystemBackend`, branch `Yasmina`
- WRITE TARGET: Source backend (`Areas/HealthServices/BillingManagement/PettyCash/**`, satu perbaikan kecil pada `PettyCashBudgetService.cs` milik `BE-BKC-036`, `BillingManagementServiceCollectionExtensions.cs`), test backend (`Tests/QuilvianSystemBackend.UnitTests.InMemory/**`), dan laporan task ini

## 1. Apa yang dikerjakan dan kenapa

**Tujuan bisnis.** Ini adalah task yang menyatukan seluruh fondasi Petty Cash (`BE-BKC-033`–`036`)
menjadi satu alur kerja nyata: kasir/petugas administrasi mengajukan voucher, Kepala Kasir
menyetujui atau menolak, kasir menyerahkan uang, lalu bukti nota dimasukkan. Tanpa task ini,
seluruh tabel dan service yang sudah dibuat tidak dapat dipakai siapa pun.

**Yang ditambahkan.** `PettyCashVoucherService` — pemilik tunggal seluruh perpindahan status
voucher — beserta sepuluh endpoint publik pada `PettyCashVouchersController`:

1. `GET /filters/metadata`, `GET /summary`, `GET /`, `GET /{id}` — membaca voucher.
2. `POST /` — mengajukan voucher baru. Nomor dialokasikan server (`BE-BKC-034`), **tidak
   pernah** dikirim frontend.
3. `POST /{id}/approve` — Kepala Kasir menyetujui, dengan penjaga sisa anggaran yang benar-benar
   bebas (`BE-BKC-036`).
4. `POST /{id}/reject` — menolak beralasan; hasilnya permanen dan tidak dapat diubah lagi.
5. `POST /{id}/cancel` — pemohon membatalkan pengajuannya sendiri selagi belum diputuskan.
6. `POST /{id}/disburse` — "Uang Diberikan"; memanggil `PettyCashBudgetService.ApplyDisbursementAsync`
   (`BE-BKC-036`) di dalam transaction yang sama sehingga saldo dan status voucher berubah
   bersamaan atau tidak sama sekali.
7. `POST /{id}/proofs` — "Input Nota"; menyelesaikan voucher, dan juga menangani koreksi nomor
   nota pada voucher yang sudah selesai.

## 2. Proses bisnis

**Pelaku.** Kasir/petugas administrasi mengajukan, menyerahkan uang, dan memasukkan bukti nota.
Kepala Kasir/Finance Operations menyetujui atau menolak. Pemohon (siapa pun perannya) adalah
satu-satunya yang boleh membatalkan pengajuannya sendiri.

**Pemicu.** Ada kebutuhan pengeluaran kecil (transport, konsumsi rapat, ATK mendadak) yang lebih
cepat diselesaikan tunai daripada lewat proses pengadaan biasa.

**Prasyarat.** Kategori pengeluaran aktif tersedia (`BE-BKC-035`), dan kolam anggaran kas kecil
sudah berisi saldo (`BE-BKC-036`).

**Langkah utama:**

1. Kasir mengisi nama penerima (teks bebas — **bukan** dipilih dari daftar pegawai, karena
   penerima kas kecil tidak selalu pegawai), kategori, nominal, dan tujuan. Sistem membentuk
   nomor voucher otomatis (`PTC-YYYYMMDD-NNNN`) dan status langsung `Menunggu Persetujuan`.
2. Kepala Kasir meninjau. Sistem menghitung ulang **di server** berapa sisa anggaran yang
   benar-benar bebas — saldo kas kecil dikurangi seluruh voucher lain yang sudah disetujui tapi
   uangnya belum diserahkan. Bila nominal voucher melebihi sisa itu, persetujuan ditolak dan
   voucher tetap menunggu; anggaran boleh ditambah lebih dulu lalu dicoba lagi.
3. Setelah disetujui, kasir menekan "Uang Diberikan". Di titik **inilah**, dan hanya di titik
   ini, saldo kas kecil benar-benar berkurang — bukan saat disetujui.
4. Penerima menyerahkan nota/kwitansi. Kasir memasukkan nomornya dan voucher menjadi `Selesai`.
   Salah ketik nomor nota masih bisa dikoreksi setelah `Selesai` lewat aksi yang sama, tanpa
   mengubah status.

**Contoh berangka — kenapa ada dua tahap (disetujui vs uang diberikan).** Saldo kas kecil
Rp 5.000.000. Voucher A Rp 300.000 disetujui pukul 09.00 — **saldo tetap Rp 5.000.000**, tetapi
sistem mencatat komitmen Rp 300.000. Pukul 09.05, voucher B Rp 4.800.000 hendak disetujui.
Sistem membandingkan Rp 4.800.000 dengan sisa bebas Rp 5.000.000 − Rp 300.000 = Rp 4.700.000, dan
**menolaknya**. Tanpa komitmen ini, kedua voucher akan lolos disetujui dan totalnya
Rp 5.100.000 melampaui kas yang benar-benar ada. Pukul 09.10, kasir menekan "Uang Diberikan"
untuk voucher A — **baru di sini** saldo turun menjadi Rp 4.700.000, dan komitmennya kembali nol.

**Perubahan status** (kode persisted → label yang wajib ditampilkan layar apa adanya):

| Dari | Tindakan | Ke | Siapa | Syarat |
| --- | --- | --- | --- | --- |
| — | Ajukan | `WAITING_APPROVAL` (Menunggu Persetujuan) | Kasir/petugas administrasi | Nama, kategori aktif, nominal > 0, tujuan terisi |
| Menunggu Persetujuan | Setujui | `APPROVED` (Disetujui) | Kepala Kasir | Nominal ≤ sisa anggaran bebas |
| Menunggu Persetujuan | Tolak | `REJECTED` (Ditolak, **permanen**) | Kepala Kasir | Alasan wajib diisi |
| Menunggu Persetujuan | Batalkan | Tetap `WAITING_APPROVAL`, ditandai `IsCancel=true` | **Pemohon itu sendiri** | Belum diputuskan |
| Disetujui | Uang Diberikan | `CASH_RECEIVED` (Uang Diterima) | Kasir | Saldo saat itu masih cukup |
| Uang Diterima | Input Nota | `COMPLETED` (Selesai) | Kasir/petugas administrasi | Nomor nota terisi |
| Selesai | Koreksi nomor nota | Tetap `COMPLETED` | Kasir/petugas administrasi | Nomor nota baru terisi |

**Jalur tidak normal.**

- Voucher yang sudah `Ditolak` tidak menerima tindakan apa pun lagi — tidak ada satu pun endpoint
  yang dapat mengubahnya, dan lapisan kedua di kode menolaknya secara eksplisit bila entah
  bagaimana tercapai.
- Bukan pemohon yang mencoba membatalkan ditolak dengan kode `403`; voucher yang sudah diputuskan
  yang dicoba dibatalkan ditolak dengan kode `422` yang berbeda.
- Tombol yang tertekan dua kali dengan `Idempotency-Key` sama mengembalikan **hasil permintaan
  pertama**, tanpa mengulang efek sampingnya (tidak membuat voucher kedua, tidak menyerahkan uang
  dua kali).
- Voucher yang sudah pernah diserahkan uangnya ditolak bila dicoba diserahkan lagi, dijaga tiga
  lapis: kunci baris kolam anggaran, `RowVersion`, dan index unik di database.

**Hasil akhir.** Setiap rupiah kas kecil yang keluar punya jejak lengkap — siapa mengajukan,
siapa menyetujui, siapa menyerahkan, dan bukti apa yang diterima — tanpa satu pun kemungkinan
saldo bergerak dua kali akibat klik ganda atau permintaan yang diulang jaringan.

## 3. Health Services / Billing Management / Petty Cash / Vouchers

Base URL: `api/v1/health-services/billing-management/petty-cash/vouchers`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Metadata filter dan pilihan status untuk layar monitoring | `PettyCashVoucher : Read` | – | `ApiResponse<PettyCashVoucherFilterMetadataResponse>` |
| `GET` | `/summary` | Jumlah voucher per status dan total nominal yang menunggu persetujuan | `PettyCashVoucher : Read` | – | `ApiResponse<PettyCashVoucherSummaryResponse>` |
| `GET` | `/` | Daftar voucher dengan pencarian, filter status/kategori/periode, urutan, halaman | `PettyCashVoucher : Read` | Query `search`,`status`,`categoryId`,`startDate`,`endDate`,`sortBy`,`sortDirection`,`pageNumber`,`pageSize` | `ApiResponse<PagedResult<PettyCashVoucherResponse>>` |
| `GET` | `/{id}` | Detail satu voucher beserta riwayat perintahnya | `PettyCashVoucher : Read` | Path `id` | `ApiResponse<PettyCashVoucherDetailResponse>` |
| `POST` | `/` | Mengajukan voucher baru | `PettyCashVoucher : Create` | Header `Idempotency-Key`; `CreatePettyCashVoucherRequest` | `ApiResponse<PettyCashVoucherResponse>` (`201`) |
| `POST` | `/{id}/approve` | Menyetujui voucher | `PettyCashVoucher : Approve` | Header `Idempotency-Key`; `ApprovePettyCashVoucherRequest` | `ApiResponse<PettyCashVoucherResponse>` |
| `POST` | `/{id}/reject` | Menolak voucher beralasan | `PettyCashVoucher : Reject` | Header `Idempotency-Key`; `RejectPettyCashVoucherRequest` | `ApiResponse<PettyCashVoucherResponse>` |
| `POST` | `/{id}/cancel` | Pemohon membatalkan pengajuannya | `PettyCashVoucher : Cancel` | Header `Idempotency-Key`; `CancelPettyCashVoucherRequest` | `ApiResponse<PettyCashVoucherResponse>` |
| `POST` | `/{id}/disburse` | "Uang Diberikan" | `PettyCashVoucher : Disburse` | Header `Idempotency-Key`; `DisbursePettyCashVoucherRequest` | `ApiResponse<PettyCashVoucherResponse>` |
| `POST` | `/{id}/proofs` | "Input Nota" (atau koreksi bila sudah Selesai) | `PettyCashVoucher : AttachProof` | Header `Idempotency-Key`; `AttachPettyCashProofRequest` | `ApiResponse<PettyCashVoucherResponse>` |

**Kode status yang mungkin muncul:**

| Kode | Arti bagi pengguna |
| --- | --- |
| `200`/`201` | Permintaan berhasil; `201` khusus voucher baru yang benar-benar baru dibuat |
| `400` | Isian dasar tidak lengkap/tidak valid, atau `Idempotency-Key` kosong (`BIL-VAL-044`, sebagian `BIL-VAL-051`) |
| `403` | Bukan pemohon voucher ini yang mencoba membatalkan (`BIL-VAL-050`) |
| `404` | Voucher tidak ditemukan |
| `409` | Data sudah berubah petugas lain, permintaan idempotent isinya berbeda, nomor voucher bentrok (`BIL-VAL-058`), atau voucher sudah pernah dicairkan (`BIL-VAL-057`) |
| `422` | Aturan bisnis tidak terpenuhi — kategori nonaktif (`BIL-VAL-045`), status salah untuk aksi itu (`BIL-VAL-046`,`050`,`051`,`052`), anggaran tidak cukup (`BIL-VAL-047`,`048`), atau alasan wajib belum diisi (`BIL-VAL-049`) |

## 4. Berkas yang diperiksa

Governance dan kontrak: `AGENTS.md`, `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`,
`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, `rules/backend/TASK_RULES.md`,
`rules/backend/REVIEW_RULES.md`.

Blueprint modul: `roadmap/backend-roadmap.md` (kartu `BE-BKC-037`), `02-backend-architecture.md`
(bagian `PettyCashVoucherService`/`PettyCashVouchersController`, "Perpindahan status", "Security,
privacy, exception, dan concurrency"), `contracts/api-contract.md` (§ Petty Cash / Vouchers),
`contracts/state-transition-matrix.md` (§ Petty Cash — kosakata status, transisi sah dan tidak
sah, "Di mana saldo anggaran bergerak"), `contracts/validation-matrix.md` (`BIL-VAL-044`–`052`,
`057`,`058`), `contracts/permission-audit-matrix.md` (Resource `PettyCashVoucher`),
`data/data-dictionary.md` (§ `BilPettyCashVoucherCommand` — kolom Sensitif).

Model dan pola implementasi terdekat yang dipakai apa adanya: `BilPettyCashVoucher.cs`,
`BilPettyCashVoucherCommand.cs` (beserta EF configuration keduanya — check constraint
`Reason` wajib untuk `REJECT`/`CANCEL`, unique index parsial `IdempotencyKey` pada **dua**
tabel), `CashierShiftService.cs` (pola `Command<T>`/`ReplayAsync<T>`/`ChangeShiftAsync` generik,
kunci penasihat berlapis, `Success<T>`/`Failure`/`IsHandled` pada controller — **diikuti persis**
sebagai referensi utama task ini, disebut eksplisit pada komentar model
`BilPettyCashVoucherCommand`), `BillingDiscountService.cs` (resolusi nama pengguna terbaca).

## 5. Berkas yang diubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BillingManagement/PettyCash/Dtos/PettyCashVoucherDtos.cs` | **Baru.** Query, enam request, dan lima response DTO |
| `Areas/HealthServices/BillingManagement/PettyCash/Services/PettyCashVoucherService.cs` | **Baru.** Empat method baca, enam method tulis, infrastruktur bersama (`ChangeVoucherAsync`, `ReplayAsync`, `Command`, kunci penasihat) |
| `Areas/HealthServices/BillingManagement/PettyCash/Controllers/PettyCashVouchersController.cs` | **Baru.** Sepuluh endpoint |
| `Areas/HealthServices/BillingManagement/PettyCash/Services/PettyCashBudgetService.cs` | **Diubah** (milik `BE-BKC-036`). `AcquireLockAsync` diubah dari `private` ke `public` agar `PettyCashVoucherService.ApproveAsync` dapat mengambil kunci kolam yang sama; pesan `BIL-VAL-057` diperbaiki menjadi persis sesuai kontrak ("Uang untuk voucher ini sudah pernah diserahkan.") |
| `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` | **Diubah.** Satu baris `services.AddScoped<PettyCashVoucherService>();` ditambahkan |
| `Tests/QuilvianSystemBackend.UnitTests.InMemory/BillingManagement/PettyCashVoucherServiceTests.cs` | **Baru.** 16 unit test domain, termasuk satu tes siklus penuh (`BIL-AT-064`) |

Tidak ada model atau migration baru — task ini murni menambahkan lapisan orkestrasi lifecycle di
atas `BilPettyCashVoucher`/`BilPettyCashVoucherCommand` (`BE-BKC-033`), penomoran (`BE-BKC-034`),
kategori (`BE-BKC-035`), dan anggaran (`BE-BKC-036`) yang sudah ada.

## 6. Implementasi

- **`PettyCashVoucherService` tidak pernah menulis `BilPettyCashBudget.CurrentBalance` sendiri.**
  `DisburseAsync` memanggil `PettyCashBudgetService.ApplyDisbursementAsync` di dalam transaction
  miliknya sendiri (`PC-DES-004`), lalu mengubah status voucher, lalu satu `SaveChangesAsync`
  menyimpan keduanya sekaligus. Bila `ApplyDisbursementAsync` menolak (saldo tidak cukup atau
  sudah pernah dicairkan), seluruh transaction — termasuk perubahan status voucher — dibatalkan.
- **Idempotensi memakai pola replay `ResponseJson`** persis seperti `BilCashierShiftCommand`:
  permintaan kedua dengan `Idempotency-Key` sama mengembalikan hasil yang **disimpan** dari
  permintaan pertama (`response.IsReplay = true`), bukan menjalankan ulang logikanya.
  Perbandingan isi memakai `PayloadHash` (SHA-256 atas field yang relevan) — **bukan**
  `CommandType` tersimpan, karena aksi "Input Nota" bisa tercatat sebagai `ATTACH_PROOF` atau
  `PROOF_CORRECTED` tergantung status voucher saat itu, sementara kunci replay harus tetap stabil
  untuk kedua kemungkinan tersebut.
- **`BilPettyCashVoucher.IdempotencyKey`** (kolom khusus milik voucher, terpisah dari
  `BilPettyCashVoucherCommand.IdempotencyKey`) diisi saat `CreateAsync` sebagai jaring pengaman
  tingkat database tambahan — bukan jalur replay utama, yang tetap lewat tabel Command persis
  seperti transisi lainnya. Ini konsisten dengan pola berlapis yang sudah dipakai modul ini
  (kunci penasihat + `RowVersion` + index unik).
- **`ApproveAsync` mengambil kunci penasihat kolam anggaran** (`AcquireLockAsync` milik
  `PettyCashBudgetService`, kini publik) sebelum membaca `GetCurrentAsync`, supaya dua
  persetujuan bersamaan tidak sama-sama lolos memeriksa komitmen sebelum salah satunya benar-benar
  tercatat — inilah penerapan `PC-DES-005` untuk mencegah race condition yang dicontohkan pada
  § 2 di atas.
- **`AttachProofAsync` menentukan `CommandType` yang benar-benar tercatat** (`ATTACH_PROOF` vs
  `PROOF_CORRECTED`) berdasarkan status voucher **setelah** voucher dimuat di dalam transaction —
  bukan sebelum, karena hanya saat itu status sebenarnya diketahui.
- **Privasi.** `RecipientName`, `Purpose`, `RejectionReason`, dan `ResponseJson` (yang memuat
  keduanya) **tidak pernah** masuk payload custom logger (`AuditCommandAsync`); yang masuk hanya
  `VoucherId`, `CommandType`, `ActorUserId`/`ActorRole`, status sebelum/sesudah, nominal, dan
  identitas correlation — sesuai `02-backend-architecture.md` § Privasi.
- **Kasir tidak memerlukan shift aktif** untuk menyerahkan uang kas kecil — `DisburseAsync`
  sengaja **tidak** memanggil `CashierShiftService.RequireActiveShiftAsync` maupun menyentuh
  `BilCashierShift` sama sekali, sesuai `state-transition-matrix.md` § "Yang tidak berubah pada
  rumpun lain" (`PC-DEC-001`).

## 7. Backend Governance Preflight

| Aspek | Nilai |
| --- | --- |
| Area | HealthServices |
| Module | BillingManagement / Billing |
| Submodule | PettyCash (mengikuti baris registry `Bil`, sama seperti `BE-BKC-033`–`036`) |
| Prefix entity | `Bil` — tidak ada entity baru; `BilPettyCashVoucher`/`BilPettyCashVoucherCommand` sudah `ACTIVE` sejak `BE-BKC-033` |
| Keberlakuan | `NEW CODE` — service, controller, DTO baru di atas model yang sudah disetujui |
| Status registry | `ACTIVE`; `QBE-MOD-002`/`003` tidak berlaku karena tidak ada model persisted baru |
| QBE ID yang berlaku | `QBE-SVC-001` (controller dilarang akses `ApplicationDbContext` langsung) — dipatuhi; `QBE-CODE-002` (controller dilarang membentuk nomor) — dipatuhi, nomor tetap dialokasikan `BillingNumberSeriesService` di dalam service; `QBE-CODE-003` (dilarang mekanisme baru yang menduplikasi yang sudah ada) — dipatuhi, `ApplyDisbursementAsync`/`AllocatePettyCashVoucherNumberAsync` dipakai apa adanya, tidak ditulis ulang |

## 8. Dampak kontrak API

Mengimplementasikan sepuluh endpoint yang sudah dikunci di `contracts/api-contract.md` § Petty
Cash / Vouchers (revisi kontrak `BIL-API-0.9`, status **DESIGN_APPROVED** menurut
`blueprint-manifest.md`). Detail bentuk request untuk `Approve`/`Reject`/`Cancel`/`Disburse`/
`AttachProof` **tidak** dijabarkan rinci pada kontrak (hanya nama tipe DTO-nya) — bentuknya
diturunkan dari `contracts/validation-matrix.md` dan `contracts/state-transition-matrix.md` yang
memang merinci syarat setiap transisi. Ini didokumentasikan sebagai delta, bukan keputusan
sepihak: `ExpectedRowVersion` (penjaga concurrency, pola baku seluruh modul Billing) ada di
kelima request tersebut; `RejectionReason`/`Reason` ada di `Reject`/`Cancel` sesuai
`BIL-VAL-049`/constraint `CommandType IN ('REJECT','CANCEL')`; `ProofReferenceNumber` ada di
`AttachProof` sesuai `BIL-VAL-051`. `CorrelationId`/`CausationId` (wajib pada
`BilPettyCashVoucherCommand`, tidak disebut kontrak sebagai field request) dibentuk **server**
per aksi, bukan dikirim client — berbeda dari pola `BillingDepositService` yang mewajibkan
keduanya dari client; delta ini dipilih karena kontrak `CreatePettyCashVoucherRequest` yang
dijabarkan rinci **tidak** menyertakan keduanya sama sekali. Tidak ada perubahan pada endpoint
modul Billing lain.

## 9. Dampak database

Tidak ada migration baru, tidak ada perubahan schema. Task ini membaca dan menulis ke tabel
`BilPettyCashVoucher` dan `BilPettyCashVoucherCommand` (migration
`20260907062238_AddTablePettyCashModule`, `BE-BKC-033`), dan memanggil
`PettyCashBudgetService`/`PettyCashCategoryService` yang sudah menulis/membaca
`BilPettyCashBudget`/`BilPettyCashBudgetMovement`/`MstPettyCashCategory` tanpa mengubah cara
kerja tabel-tabel itu sendiri.

## 10. Dampak keamanan

Tujuh permission baru pada Resource `PettyCashVoucher` (`Read`, `Create`, `Approve`, `Reject`,
`Cancel`, `Disburse`, `AttachProof`) sesuai `contracts/permission-audit-matrix.md` — argumen
pertama `[AccessAction]` dan argumen kedua `[AccessPermission]` **disalin persis sama** untuk
setiap action (diverifikasi manual per action saat menulis controller, sesuai peringatan
kontrak: kelalaian menyamakan keduanya menghasilkan `403` permanen yang tidak dapat diperbaiki
dari layar Akses Role). `RecipientName`/`Purpose`/`RejectionReason`/`ResponseJson` dikecualikan
dari custom logger (lihat § 6). Modul Petty Cash **tidak menyentuh data pasien sama sekali**.

- VISUAL REFERENCE: NOT REQUIRED (tidak ada perubahan UI pada task backend ini)

## 11. Validasi

| Perintah/pemeriksaan | Hasil | Klasifikasi | Bukti/catatan |
| --- | --- | --- | --- |
| `dotnet build` | Tidak dijalankan sesi ini | `NOT RUN` | Sesuai instruksi baku pengguna: build/test backend dijalankan manual oleh pengguna, bukan otomatis oleh sesi |
| `dotnet test --filter PettyCashVoucherServiceTests` | Tidak dijalankan sesi ini | `NOT RUN` | Sama seperti di atas — menunggu pengguna menjalankan dan melaporkan hasilnya |
| Review diff/scope | Dilakukan | `PASS` | `git status --short` menunjukkan hanya berkas dalam lingkup Petty Cash Voucher (plus satu perbaikan kecil pada `PettyCashBudgetService.cs` milik `BE-BKC-036`) yang tersentuh |
| Review kesesuaian QBE | Dilakukan | `PASS` | `QBE-SVC-001`/`QBE-CODE-002`/`QBE-CODE-003` dipatuhi; tidak ada model persisted baru |
| Pemeriksaan rahasia | Dilakukan | `PASS` | Tidak ada credential/token/connection string; field Sensitif dikecualikan dari audit log (§ 6) |

16 unit test domain ditulis pada `PettyCashVoucherServiceTests.cs`:

- **Alur penuh** (`BIL-AT-064`): submit → approve → disburse → attach-proof, memverifikasi
  status, saldo anggaran (`CurrentBalance`/`ReservedAmount`), dan urutan empat baris
  `BilPettyCashVoucherCommand`. Ditambah satu tes koreksi nomor nota (`PROOF_CORRECTED`).
- **Create**: kategori nonaktif ditolak (`BIL-VAL-045`), nominal nol ditolak (`BIL-VAL-044`),
  `Idempotency-Key` sama dua kali tidak membuat voucher kedua.
- **Approve**: contoh berangka persis dari `state-transition-matrix.md` (saldo 5 juta, voucher A
  300 ribu disetujui, voucher B 4,8 juta ditolak karena melebihi sisa bebas 4,7 juta —
  `BIL-VAL-047`); status salah ditolak.
- **Reject**: alasan kosong ditolak (`BIL-VAL-049`); voucher `Ditolak` tidak menerima aksi
  lanjutan apa pun (`BIL-VAL-052`, immutability).
- **Cancel**: bukan pemohon ditolak `403` (`BIL-VAL-050`); pemohon berhasil membatalkan
  (`IsCancel=true`, status tetap `WAITING_APPROVAL`); voucher yang sudah disetujui tidak bisa
  dibatalkan lagi.
- **Disburse**: tanpa persetujuan ditolak (`BIL-VAL-046`); saldo dikoreksi turun **setelah**
  persetujuan membuat pencairan ditolak (`BIL-VAL-048`, memanggil `PettyCashBudgetService.AdjustAsync`
  sungguhan untuk membuktikan integrasi lintas service, bukan mock).
- **Concurrency**: `RowVersion` usang ditolak `409`.
- **Read**: `GetSummaryAsync` menghitung jumlah per status dan total nominal menunggu dengan
  benar.
- **DI**: resolusi `PettyCashVoucherService` lewat `AddBillingManagement()`.

**Belum ada satu pun yang benar-benar dieksekusi** — status di atas menunggu pengguna
menjalankan `dotnet test` secara manual. Kunci `pg_advisory_xact_lock` dan concurrency pencairan
ganda sungguhan (`BIL-AT-071`) hanya dapat dibuktikan pada provider PostgreSQL, di luar jangkauan
unit test `InMemory`.

- MANUAL TEST: NOT APPLICABLE (task backend murni, tidak ada UI untuk diuji manual)

## 12. Peringatan dan risiko yang tersisa

- Test dan build **belum diverifikasi berjalan** sesi ini. Task ini **belum boleh ditandai
  selesai** sampai pengguna menjalankan `dotnet build`/`dotnet test` dan hasilnya dilaporkan
  kembali.
- **Perubahan pada `PettyCashBudgetService.cs`** (visibilitas `AcquireLockAsync` dan perbaikan
  pesan `BIL-VAL-057`) berarti `BE-BKC-036` juga perlu diverifikasi ulang build/test-nya bersamaan
  dengan task ini, bukan dianggap sudah final dari laporan sebelumnya.
- **`ExpectedRowVersion` untuk `Approve`/`Disburse` tidak dijaga eksplisit non-empty di level
  DTO** (`Guid` bukan `Guid?`) — kekosongan tetap tertangkap oleh `ValidateExpectedVersion` di
  level service (melempar `PettyCashVoucherBadRequestException` bila `Guid.Empty`), sehingga
  tidak ada celah, hanya dicatat karena polanya sedikit berbeda dari DTO nullable di modul lain.
- **`GET /` query `customPeriod`** diterima pada DTO tetapi **belum diimplementasikan
  perilakunya** (tidak dijabarkan rinci pada kontrak) — frontend diharapkan mengirim
  `startDate`/`endDate` langsung untuk sementara. Dicatat sebagai keputusan proporsional, bukan
  kelalaian, karena tidak ada acceptance test yang menuntut logika periode bernama.
- **`BE-BKC-033`** (tabel dasar) masih 🟡 pada roadmap menyangkut bukti seed dan review Finance —
  tidak menghalangi source task ini, tetapi wajib tertutup sebelum gelombang `MVP-14` dinyatakan
  naik.
- **`BIL-AT-077`** (regresi `BilCashierShift` tidak bergerak akibat voucher) **belum** diuji pada
  task ini — sesuai desain, regresi lintas-slice ini adalah tanggung jawab `BE-BKC-038` sebagai
  capstone hardening.

## 13. Perubahan sampingan

- INCIDENTAL CHANGES: Satu baris pesan (`BIL-VAL-057`) dan satu perubahan visibilitas method
  (`private` → `public`) pada `PettyCashBudgetService.cs`, milik `BE-BKC-036`. **Bukan** perubahan
  di luar cakupan — keduanya diperlukan langsung oleh integrasi `DisburseAsync`/`ApproveAsync`
  pada task ini, dan dicatat eksplisit di § 5 dan § 12, bukan disembunyikan sebagai bagian task
  lain.

## 14. Interupsi

- INTERRUPTIONS: NONE — task dikerjakan dalam satu sesi berkelanjutan tanpa interupsi.

## 15. Status Git

```
 M Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs
 M Areas/HealthServices/BillingManagement/PettyCash/Services/PettyCashBudgetService.cs
 M docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md
 M docs/module-blueprints/billing-kasir/roadmap/requirement-traceability.md
?? Areas/HealthServices/BillingManagement/PettyCash/Controllers/PettyCashVouchersController.cs
?? Areas/HealthServices/BillingManagement/PettyCash/Dtos/PettyCashVoucherDtos.cs
?? Areas/HealthServices/BillingManagement/PettyCash/Services/PettyCashVoucherService.cs
?? Tests/QuilvianSystemBackend.UnitTests.InMemory/BillingManagement/PettyCashVoucherServiceTests.cs
```

(Baris lain pada `git status --short` sesi ini berasal dari task `BE-BKC-035`/`036` yang belum
di-commit, bukan bagian task ini.) Belum di-stage maupun di-commit. Branch `Yasmina`.

## 16. Langkah berikutnya yang disarankan

1. Pengguna menjalankan `dotnet build` dan `dotnet test` secara manual untuk `BE-BKC-035`,
   `036`, dan `037` sekaligus (karena `037` mengubah satu berkas milik `036`), lalu melaporkan
   hasil sebenarnya.
2. Setelah build/test terbukti lulus, perbarui tabel status roadmap dan
   `requirement-traceability.md`, menautkannya ke laporan ini.
3. Lanjutkan ke `BE-BKC-038` — hardening lintas-slice: wiring permission penuh pada ketiga
   controller baru, scrub log, evidence matrix `BIL-AT-064`–`080` lengkap, dan regresi
   `BilCashierShift` (`BIL-AT-077`) yang **belum** disentuh task mana pun sampai saat ini.

- KNOWN ISSUES: `customPeriod` belum berperilaku (§ 12); tidak ada yang lain ditemukan pada
  scope task ini.
- STALE EVIDENCE / BLOCKED PHASES: NOT APPLICABLE (bukan `MODULE BLUEPRINT MODE`)
- BLUEPRINT STATUS/EVIDENCE: NOT APPLICABLE (bukan `MODULE BLUEPRINT MODE`)
