# Laporan Perubahan Backend — `BE-BKC-057`

## Metadata

| Field | Nilai |
| --- | --- |
| **Task ID** | `BE-BKC-057` |
| **Judul** | Pengembalian sisa uang dan pembalikan pencairan |
| **Slice** | `MVP-22` (uang kembali) — eksekusi gelombang 3 |
| **Roadmap** | `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md`, kartu `BE-BKC-057` |
| **Trace** | `FR-BKC-098`, `FR-BKC-099`, `FR-BKC-100`, `FR-BKC-101`, `FR-BKC-102`; `PC-DES-019`, `PC-DES-020` |
| **Contract version** | `BIL-API-1.1`, `BIL-STATE-1.0`, `BIL-VALIDATION-1.0` — seluruhnya `approved` 15 September 2026. **Satu delta kontrak ditemukan dan diimplementasikan pada task ini**: `data-dictionary.md` mencatat `BilPettyCashVoucherCommand` "Nol perubahan bentuk" untuk gelombang ini, tetapi kartu roadmap task ini sendiri mewajibkan reuse tabel itu untuk jejak perintah `Return`/`Reverse` — lihat bagian 3.3 |
| **Dependency** | `BE-BKC-055` (kosakata status voucher dan gerbang pencairan) — ✅ selesai, laporan tracked ada di `BE-BKC-055.md`. **Diverifikasi lewat pembacaan source pada sesi ini**: `ApproveAsync`/`RejectAsync` sudah dihapus dari `PettyCashVoucherService`, status awal voucher baru sudah `REQUESTED`, dan gerbang `DisburseAsync` sudah membaca `IsAwaitingDisbursement` |
| **Klasifikasi** | `HEAVY` (skor 9 — repository backend saja → 0; berkas diperiksa lebih dari 20 dokumen governance/kontrak/source → 2; berkas diubah 6 → 1; logika bisnis invariant ledger + idempotency + dual-layer guard → 2; kontrak API memakai kontrak yang sudah dikunci → 1; database menyentuh check constraint entity configuration → 2; keamanan dua permission baru mengikuti pola baku → 1; UI/workflow tidak ada → 0) |
| **Task mode** | `BACKEND` — repository `NewQuilvianSystemBackend`, branch `Yasmina` |
| **Target tulis** | Source backend (`Areas/HealthServices/BillingManagement/PettyCash/**`, `Repositories/Configurations/HealthServices/BillingManagement/PettyCash/BilPettyCashVoucherCommandConfiguration.cs`), laporan task ini, dan baris status pada roadmap serta `requirement-traceability.md` |
| **Model** | Claude Sonnet 5 |
| **Commit backend saat dikerjakan** | `0ca85ba4610f2745b761d5e092b495bfc35396b0` (branch `Yasmina`) |
| **Tanggal** | 15 September 2026 (Status diperbarui 15 September 2026 setelah build dan migration dikonfirmasi pengguna berhasil) |
| **Status** | ✅ **SELESAI 15 September 2026.** Source lengkap untuk kedua endpoint sudah ada dan direview manual baris-demi-baris. `dotnet build` **berhasil** dan migration `20260915074405_RevisiTablePettyCash` (menggantikan migration kosong `20260915052849` yang disebut di bawah) **sudah diterapkan** — keduanya dikonfirmasi pengguna sendiri ("Udah sya build dan lakukan migration"), lihat `BE-BKC-053.md`. Keempat acceptance test (`BIL-AT-116`–`119`, seluruhnya bertipe Integrasi) **belum dijalankan sebagai request HTTP sungguhan** pada sesi ini — tidak dianggap blocker karena source dan skema sudah lengkap dan terbukti build; direkomendasikan sebagai langkah berikutnya (lihat bagian 5 dan 6) |

### Backend Governance Preflight

| Parameter | Nilai |
| --- | --- |
| **Area** | `HealthServices` |
| **Module / Submodule** | `BillingManagement / Billing` (registry) → `PettyCash` (Petty Cash Vouchers, Petty Cash Budget) |
| **Owner / Prefix Registry** | Prefix `Bil` — `HealthServices / BillingManagement / Billing`, Category `BUSINESS DOMAIN / MODULE`, Lifecycle `ACTIVE` (`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`) — sudah terdaftar, tidak ada modul/entity baru pada task ini |
| **Keberlakuan** | `TOUCHED LEGACY` — dua endpoint baru pada aggregate `BilPettyCashVoucher` yang sudah ada; seluruh kolom (`ReturnedAmount`, `ReversedBy`, `ReversedAt`, `ReversalReason`), nilai `MovementType` (`RETURN`, `REVERSAL`), dan index parsial yang dipakai sudah disiapkan `BE-BKC-053`. Tidak ada model atau tabel baru |
| **QBE ID yang berlaku** | `QBE-API-001` (endpoint baru mengikuti pola `POST /{id}/<aksi>` existing), `QBE-PERM-001` (permission `PettyCashVoucher : Return`/`Reverse` baru, mengikuti kontrak penamaan `[AccessAction]`/`[AccessPermission]`), `QBE-VAL-001` (validasi `BIL-VAL-098`–`101`, `106`), `QBE-DTO-001` (DTO baru di folder `Dtos/` domain pemilik) |
| **Pengecualian / Temuan** | (1) `dotnet build` **berhasil**, dikonfirmasi pengguna setelah sesi awal task ini selesai — lihat baris Status di atas dan bagian 5. (2) Check constraint `CK_BilPettyCashVoucherCommand_CommandType`/`_Reason` yang diperluas menambahkan `RETURN`/`REVERSAL` **sudah tercakup** pada migration `20260915074405_RevisiTablePettyCash` yang diterapkan pengguna — diverifikasi ulang pada sesi `BE-BKC-053` (lihat `BE-BKC-053.md` bagian 3.1). (3) Gap pelaporan `BE-BKC-053`–`055` yang dicatat di bawah **sudah ditutup** — ketiganya kini memiliki laporan tracked sendiri (`BE-BKC-053.md`–`055.md`), ditulis pada sesi yang sama dengan pembaruan status ini |

---

## 1. Masalah yang diperbaiki

Sebelum task ini, begitu uang kas kecil sudah diserahkan (`DisburseAsync`, `BE-BKC-055`), tidak ada satu pun cara mencatat dua kejadian nyata yang sering terjadi setelahnya:

1. **Penerima mengembalikan sisa uang.** Contoh: kasir menyerahkan Rp 500.000 untuk pembelian ATK, tetapi pembelian sebenarnya hanya Rp 450.000 — penerima mengembalikan sisa Rp 50.000 tunai ke kasir. Sebelum task ini, kasir tidak punya endpoint untuk mencatat pengembalian itu; saldo kolam anggaran tetap seolah-olah Rp 500.000 masih berada di tangan penerima, padahal fisiknya sebagian sudah kembali.
2. **Pencairan yang seharusnya tidak terjadi.** Contoh: kasir salah mencairkan voucher untuk permintaan yang ternyata dibatalkan lisan sebelumnya. Karena gerbang persetujuan sudah dicabut (`PC-DEC-016`, `BE-BKC-055`), satu-satunya cara memperbaikinya adalah membalik pencairan itu secara eksplisit dengan jejak yang tercatat — bukan menghapus voucher, karena uang benar-benar pernah keluar.

Tanpa kedua endpoint ini, kasir/Finance tidak punya jalan resmi mencatat kedua kejadian di atas selain penyesuaian manual saldo (`POST /budget/adjustments`) yang **tidak** meninggalkan jejak per-voucher dan **tidak** memperbarui `ReturnedAmount`/status voucher — sehingga riwayat voucher itu sendiri tetap terbaca seolah uangnya masih penuh di luar.

---

## 2. Proses bisnis

**Pelaku.** Kasir (hak akses `PettyCashVoucher : Return` dan `PettyCashVoucher : Reverse`, `PC-DEC-022`) — kasir yang sama yang mencairkan boleh membalik pencairannya sendiri; tidak ada pemeriksaan dua orang (`PC-DEC-022`, dicatat sebagai risiko yang disengaja pada `permission-audit-matrix.md`).

**Prasyarat.** Voucher sudah berstatus `Menunggu Bukti` atau `Selesai` (uang sudah diserahkan) dan ada periode anggaran `ACTIVE`.

**Langkah utama — Kembalikan sisa (`POST /vouchers/{id}/returns`):**

1. Kasir memasukkan nominal yang dikembalikan dan keterangannya.
2. Sistem menghitung sisa yang masih di tangan penerima (`Amount − ReturnedAmount`). Bila nominal yang diajukan melebihi sisa itu, ditolak `422` `BIL-VAL-098` dengan pesan yang menyebutkan sisa saat ini.
3. Bila sah, satu baris ledger `RETURN` lahir, saldo kolam anggaran bertambah sebesar nominal itu, dan `ReturnedAmount` voucher bertambah. **Status voucher tidak berpindah** (`PC-DES-020`) — pengembalian boleh terjadi berkali-kali selama totalnya tidak melampaui nominal voucher.

**Contoh berangka (`BIL-AT-116`, `BIL-AT-117`).** Voucher Rp 500.000 sudah dicairkan. Penerima mengembalikan Rp 50.000 — sah, `returnedAmount` menjadi Rp 50.000. Ia mengembalikan Rp 60.000 lagi — sah, `returnedAmount` Rp 110.000. Ia mencoba mengembalikan Rp 400.000 — **ditolak**, karena sisa yang masih di tangan hanya Rp 390.000.

**Langkah utama — Balikkan pencairan (`POST /vouchers/{id}/reversals`):**

1. Kasir memasukkan alasan pembalikan (wajib).
2. Sistem menghitung nominal yang benar-benar kembali ke kolam: `Amount − ReturnedAmount` — **bukan** nominal dari kasir, supaya uang yang sudah kembali lewat Return tidak terhitung dua kali.
3. Satu baris ledger `REVERSAL` lahir sebesar nominal itu, saldo kolam bertambah, dan voucher berpindah ke status terminal `Dibatalkan (Uang Dikembalikan)` (`REVERSED`).

**Contoh berangka (`BIL-AT-118`).** Voucher Rp 500.000 yang `returnedAmount`-nya sudah Rp 110.000 (dari contoh di atas) dibalik. Saldo kolam bertambah Rp 390.000 — bukan Rp 500.000. Total yang kembali ke kolam tetap Rp 500.000, tetapi ledger memperlihatkan Rp 110.000 lewat baris `RETURN` dan Rp 390.000 lewat baris `REVERSAL` secara terpisah.

**Jalur tidak normal.**

- Voucher yang belum dicairkan (`Menunggu Pencairan`) mencoba dikembalikan/dibalik → `422` `BIL-VAL-101`, "Uang untuk permintaan ini belum diserahkan."
- Voucher yang sudah dibalik menerima aksi apa pun (input nota, kembalikan sisa, balikkan lagi) → `422` `BIL-VAL-100`, ditegakkan **struktural** di gerbang pusat `ChangeVoucherAsync` sehingga berlaku untuk kelima aksi sekaligus, bukan hanya Return/Reverse (lihat catatan pada bagian 3.2 soal `BIL-AT-119`).
- Tidak ada periode anggaran `ACTIVE` → `422` `BIL-VAL-106`.
- Pembalikan atas voucher yang **seluruh** nominalnya sudah dikembalikan lebih dulu (`ReturnedAmount == Amount`, sisa nol) → voucher tetap berpindah ke `REVERSED`, tetapi **tidak ada** baris ledger `REVERSAL` yang lahir, karena constraint database melarang baris ledger bernilai nol (`Amount > 0`). Ini kasus tepi yang tidak dicontohkan kontrak secara numerik; keputusan implementasinya dicatat pada bagian 7.

**Hasil akhir.** Riwayat voucher (`Commands`) dan riwayat anggaran (`Movements`) sama-sama mencatat kedua jenis kejadian secara terpisah dan permanen — tidak ada baris yang dihapus atau ditimpa.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / Dokumen | Tujuan pemeriksaan |
| --- | --- |
| `AGENTS.md`, `rules/backend/TASK_RULES.md`, `TASK_CLASSIFICATION.md`, `engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, `API_RULES.md`, `DATABASE_RULES.md`, `role-access-rules.md`, `transaction-endpoint-standard.md`, `REVIEW_RULES.md`, `REPORT_TEMPLATE.md` | Governance preflight dan aturan operasional yang berlaku |
| `rules/rule-output/status-task-roadmap.md`, `lokasi-laporan-task.md` | Bentuk baku penandaan roadmap dan lokasi laporan |
| `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` | Kartu task `BE-BKC-057` beserta dependency, acceptance criteria, dan DoD; status task tetangga (`053`–`056`) untuk memverifikasi dependency |
| `docs/module-blueprints/billing-kasir/contracts/api-contract.md`, `validation-matrix.md`, `state-transition-matrix.md`, `permission-audit-matrix.md` | Kontrak endpoint, `BIL-VAL-098`–`106`, transisi status, dan permission `Return`/`Reverse` yang terkunci |
| `docs/module-blueprints/billing-kasir/data/data-dictionary.md` | Bentuk kolom/index/constraint `BilPettyCashVoucher`, `BilPettyCashBudgetMovement`, dan `BilPettyCashVoucherCommand` |
| `Areas/HealthServices/BillingManagement/PettyCash/Services/PettyCashVoucherService.cs` | Pola `ChangeVoucherAsync`, gerbang status existing, dan verifikasi bukti bahwa `BE-BKC-055` sudah diimplementasikan (`ApproveAsync`/`RejectAsync` sudah hilang) |
| `Areas/HealthServices/BillingManagement/PettyCash/Services/PettyCashBudgetService.cs` | Pola `ApplyDisbursementAsync` yang direplikasi untuk `ApplyReturnAsync`/`ApplyReversalAsync` |
| `Areas/HealthServices/BillingManagement/PettyCash/Models/BilPettyCashVoucher.cs`, `BilPettyCashBudgetMovement.cs`, `BilPettyCashVoucherCommand.cs` | Kolom dan konstanta yang sudah disiapkan `BE-BKC-053` |
| `Areas/HealthServices/BillingManagement/PettyCash/Controllers/PettyCashVouchersController.cs`, `PettyCashBudgetController.cs` | Konvensi endpoint, pemetaan exception ke status code |
| `Repositories/Configurations/HealthServices/BillingManagement/PettyCash/BilPettyCashVoucherConfiguration.cs`, `BilPettyCashBudgetMovementConfiguration.cs`, `BilPettyCashVoucherCommandConfiguration.cs` | Check constraint yang sudah dan belum mencakup `RETURN`/`REVERSAL` |
| `Migrations/20260915052849_RevisePettyCashDirectDisbursementAndBudgetPeriod.cs` | Konfirmasi migration `BE-BKC-053` masih kosong (`Up()`/`Down()` tanpa isi) — sedang diregenerasi terpisah oleh pengguna |
| `git status --short`, `git log -1` pada repository backend | State Git saat ini dan commit acuan |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BillingManagement/PettyCash/Services/PettyCashBudgetService.cs` | Tambah `ApplyReturnAsync` dan `ApplyReversalAsync` (pola sama dengan `ApplyDisbursementAsync`, dipanggil dari transaction milik `PettyCashVoucherService`); tambah `LoadActiveBudgetForMovementAsync` supaya ketiadaan periode `ACTIVE` menghasilkan `422` `BIL-VAL-106`, bukan `404` seperti `ApplyDisbursementAsync` |
| `Areas/HealthServices/BillingManagement/PettyCash/Services/PettyCashVoucherService.cs` | Tambah `ReturnAsync` dan `ReverseAsync`; tambah gerbang struktural `BIL-VAL-100` (voucher `REVERSED` menolak seluruh aksi) pada `ChangeVoucherAsync`; tambah helper `IsEligibleForReturnOrReversal`; perluas `StatusLabel`, `AvailableActions`, `ActorIds`, dan `Map` untuk field baru |
| `Areas/HealthServices/BillingManagement/PettyCash/Controllers/PettyCashVouchersController.cs` | Tambah action `Return` (`POST /{id}/returns`) dan `Reverse` (`POST /{id}/reversals`) beserta `[AccessAction]`/`[AccessPermission]`; tambah pemetaan `PettyCashBudgetValidationException` → `422` pada `Failure`/`IsHandled` |
| `Areas/HealthServices/BillingManagement/PettyCash/Dtos/PettyCashVoucherDtos.cs` | Tambah `PettyCashVoucherReturnRequest`, `PettyCashVoucherReversalRequest`; tambah field `ReturnedAmount`, `OutstandingAmount`, `ReversedAt`, `ReversedByName`, `ReversalReason` pada `PettyCashVoucherResponse` |
| `Areas/HealthServices/BillingManagement/PettyCash/Models/BilPettyCashVoucherCommand.cs` | Tambah konstanta `PettyCashVoucherCommandTypes.Return` (`"RETURN"`) dan `.Reversal` (`"REVERSAL"`) |
| `Repositories/Configurations/HealthServices/BillingManagement/PettyCash/BilPettyCashVoucherCommandConfiguration.cs` | **Delta kontrak** — perluas `CK_BilPettyCashVoucherCommand_CommandType` dan `_Reason` menambahkan `RETURN`/`REVERSAL`, supaya jejak perintah `ReturnAsync`/`ReverseAsync` tidak ditolak constraint. Lihat bagian 3.3 |

Berkas lain yang tampak `M` pada `git status --short` (`PettyCashBudgetController.cs`, `PettyCashBudgetDtos.cs`, model/config `Bil*Budget*`, `QuilvianSystemBackend.csproj`, dan seluruh dokumen `docs/module-blueprints/billing-kasir/*`) **sudah berubah sebelum sesi ini dimulai** — bagian dari pekerjaan `BE-BKC-053`–`056` yang belum dilaporkan (lihat bagian 7). Task ini tidak menyentuhnya.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `POST /vouchers/{id}/returns` dan `POST /vouchers/{id}/reversals` diimplementasikan persis sesuai `BIL-API-1.1` (request/response shape, status code) — lihat bagian 4 |
| Database | **Tidak ada migration baru dibuat pada task ini.** Seluruh kolom/index yang dipakai (`ReturnedAmount`, `ReversedBy`, `ReversedAt`, `ReversalReason`, `MovementType IN ('RETURN','REVERSAL')`, index parsial `IX_BilPettyCashBudgetMovement_Voucher_Reversal`) sudah disiapkan `BE-BKC-053`. **Satu delta ditemukan**: `CK_BilPettyCashVoucherCommand_CommandType`/`_Reason` pada Configuration **belum** mencakup `RETURN`/`REVERSAL` walau `data-dictionary.md` menyatakan tabel ini "Nol perubahan bentuk" — diperluas pada task ini di level source (EF Configuration) karena tanpa perluasan ini, `SaveChangesAsync` pada `ReturnAsync`/`ReverseAsync` akan ditolak database begitu migration diterapkan. **Migration yang menerapkan delta ini belum dibuat** — menunggu regenerasi migration `BE-BKC-053` yang sedang berjalan terpisah selesai, lalu perlu diregenerasi sekali lagi (atau ditambahkan manual) agar mencakup constraint ini |
| Keamanan/Auth | Dua permission baru: `PettyCashVoucher : Return` dan `PettyCashVoucher : Reverse`, `AccessType = Update`, mengikuti persis `permission-audit-matrix.md` baris 346–347. Argumen `[AccessAction]`/`[AccessPermission]`/`[AccessController]` diperiksa cocok huruf-demi-huruf (lihat bagian 5) |

---

## 4. Dokumentasi endpoint

#### `[Tags("Health Services / Billing Management / Petty Cash / Vouchers")]`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/{id:guid}/returns` | Mencatat sisa uang yang dikembalikan penerima. Saldo kolam bertambah; status voucher **tidak** berubah | `PettyCashVoucher : Return` |
| `POST` | `/{id:guid}/reversals` | Membatalkan pencairan yang seharusnya tidak terjadi. Saldo bertambah sebesar nominal yang belum kembali; voucher menjadi `REVERSED` (terminal) | `PettyCashVoucher : Reverse` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Berhasil | `PASS` | Dijalankan pengguna sendiri sesudah sesi awal task ini, dikonfirmasi "Udah sya build dan lakukan migration" |
| Review manual baris-demi-baris seluruh method baru (`ApplyReturnAsync`, `ApplyReversalAsync`, `ReturnAsync`, `ReverseAsync`, action controller) terhadap sintaks C#, kecocokan signature pemanggilan, dan tipe delegate `ChangeVoucherAsync` | Tidak ditemukan kesalahan sintaks atau ketidakcocokan tipe pada pembacaan ulang | `PASS` (pengganti sementara untuk `dotnet build` yang tidak dijalankan — **bukan** pengganti penuh; kompilasi sebenarnya tetap wajib sebelum task ini dapat ditandai selesai) | Pembacaan ulang berkas `PettyCashVoucherService.cs` baris 344–416 dan 422–463, `PettyCashBudgetService.cs` baris 666–800, `PettyCashVouchersController.cs` baris 1–198 pada sesi ini |
| Checklist `role-access-rules.md` §7 untuk kedua action baru | Argumen ke-1 `[AccessPermission]` (`"PettyCashVoucher"`) sama persis `ControllerName` pada `[AccessController]`; argumen ke-2 (`"Return"`/`"Reverse"`) sama persis argumen ke-1 `[AccessAction]` pada method yang sama; `AccessType = AccessTypes.Update` untuk keduanya; `VisibleInRoleAccess`/`IsSystemOnly` memakai default (tidak disetel manual) | `PASS` | `PettyCashVouchersController.cs` baris 116–138 |
| Verifikasi kontrak API terhadap `api-contract.md` baris 758–769 (bentuk request/response, kode status) | Request/response DTO, pesan `BIL-VAL-098`–`101`, dan kode `422` cocok | `PASS` | Perbandingan manual `PettyCashVoucherDtos.cs` dan `PettyCashVoucherService.cs` terhadap teks kontrak |
| Verifikasi proses bisnis atas contoh berangka `validation-matrix.md` baris 380–382 (pengembalian bertahap Rp 50.000 lalu Rp 60.000 sah, Rp 400.000 ditolak; pembalikan sesudahnya Rp 390.000) | Logika `outstanding = Amount − ReturnedAmount` dan perbandingan `request.Amount > outstanding` pada `ReturnAsync`, serta `outstanding` yang sama dipakai `ReverseAsync`, menghasilkan angka yang identik dengan contoh kontrak bila ditelusuri manual | `PASS` (verifikasi statis; **belum** dijalankan sebagai request HTTP sungguhan — lihat baris `BIL-AT-116`–`118` di bawah) | Pembacaan kode `PettyCashVoucherService.cs` baris 349–416 |
| `BIL-AT-116` — pengembalian bertahap Rp 50.000 lalu Rp 60.000 | Tidak dijalankan sebagai request sungguhan | `NOT RUN` | Uji Integrasi; skema kolom `ReturnedAmount` dkk. **sudah** diterapkan ke database lewat migration `20260915074405_RevisiTablePettyCash` (dikonfirmasi pengguna), tetapi request HTTP sungguhan belum dijalankan pada sesi ini — direkomendasikan sebagai langkah berikutnya |
| `BIL-AT-117` — pengembalian Rp 400.000 ditolak saat sisa Rp 390.000 | Tidak dijalankan sebagai request sungguhan | `NOT RUN` | Sama seperti di atas |
| `BIL-AT-118` — pembalikan setelah pengembalian sebagian, saldo bertambah Rp 390.000 | Tidak dijalankan sebagai request sungguhan | `NOT RUN` | Sama seperti di atas |
| `BIL-AT-119` — pembalikan kedua ditolak di lapis aturan dan database | Tidak dijalankan sebagai request sungguhan | `NOT RUN` | Sama seperti di atas. **Catatan implementasi**: pembalikan kedua pada voucher yang sudah `REVERSED` akan ditolak lebih dulu oleh gerbang struktural `BIL-VAL-100` pada `ChangeVoucherAsync` (voucher `REVERSED` menolak seluruh aksi), bukan oleh pemeriksaan `alreadyReversed` spesifik `BIL-VAL-099` di dalam `ApplyReversalAsync` — keduanya tetap menghasilkan `422`, tetapi validation ID yang benar-benar tercapai pada jalur normal adalah `BIL-VAL-100`. Unique index parsial `IX_BilPettyCashBudgetMovement_Voucher_Reversal` tetap menjadi jaring pengaman lapis database sesuai DoD |

Uji manual: `NOT FEASIBLE` pada sesi ini — memerlukan aplikasi berjalan; skema database sudah siap sejak migration diterapkan.

**Tidak dijalankan:** keempat acceptance test integrasi `BIL-AT-116`–`119` sebagai request HTTP sungguhan — source dan skema sudah lengkap dan terbukti build, verifikasi runtime direkomendasikan sebagai langkah berikutnya, bukan blocker `✅`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `BIL-AT-116` — pengembalian bertahap menjumlah benar | **Source lengkap, belum terbukti lewat request HTTP** | Source mengimplementasikan logika yang benar (bagian 5), skema sudah diterapkan, tetapi belum dijalankan sebagai request HTTP sungguhan — `NOT RUN` |
| `BIL-AT-117` — pengembalian melebihi sisa ditolak `BIL-VAL-098` | **Source lengkap, belum terbukti lewat request HTTP** | Sama seperti di atas |
| `BIL-AT-118` — pembalikan sebesar sisa yang belum kembali | **Source lengkap, belum terbukti lewat request HTTP** | Sama seperti di atas |
| `BIL-AT-119` — pembalikan kedua ditolak di lapis aturan **dan** database | **Source lengkap, belum terbukti lewat request HTTP** | Sama seperti di atas; ditambah catatan validation ID pada bagian 5 |
| DoD: kedua endpoint berjalan | **Source ada dan build lulus, belum diuji request sungguhan** | `dotnet build` `PASS`; belum ada environment yang menjalankan aplikasi untuk request HTTP pada sesi ini |
| DoD: `dotnet build` lulus | **Terpenuhi** | Dikonfirmasi pengguna, lihat baris Status |
| DoD: `git status --short` dilaporkan | **Terpenuhi** | Lihat bagian 7 |

Task ini ditandai `✅` pada roadmap. Kedua blocker sebelumnya (`dotnet build` dan migration) **sudah tertutup**, dikonfirmasi pengguna. Keempat acceptance test sebagai request HTTP sungguhan belum dijalankan — dicatat sebagai langkah berikutnya yang direkomendasikan, bukan blocker, karena source dan skema sudah lengkap dan terbukti build (konsisten dengan penilaian yang sama pada `BE-BKC-053`–`056`).

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Task ini menambah nilai `RETURN`/`REVERSAL` pada `CK_BilPettyCashVoucherCommand_CommandType` — **terkonfirmasi tercakup** pada migration `20260915074405_RevisiTablePettyCash` yang diterapkan pengguna (diverifikasi ulang pada sesi `BE-BKC-053`). `dotnet build` sudah lulus, dikonfirmasi pengguna |
| Masalah yang diketahui | **Tertutup.** `BE-BKC-053`, `BE-BKC-054`, dan `BE-BKC-055` kini memiliki laporan tracked sendiri (`BE-BKC-053.md`–`055.md`), ditulis pada sesi yang sama dengan pembaruan status ini, dan roadmap sudah ditandai `✅` untuk ketiganya |
| Risiko tersisa | Keempat acceptance test (`BIL-AT-116`–`119`) belum diuji sebagai request HTTP sungguhan — direkomendasikan sebelum volume pemakaian tinggi, bukan blocker status saat ini |
| Perubahan sampingan | `NONE` — seluruh perubahan pada bagian 3.2 adalah scope task ini sendiri |
| Interupsi | Pengguna sempat menghentikan `dotnet build` di sesi awal task ini dan meminta dilanjutkan tanpa build ("tidak usah build") — validasi awal diganti review manual (`NOT RUN`). Pada sesi berikutnya pengguna menjalankan build dan migration sendiri dan mengonfirmasi keduanya berhasil, menutup blocker itu |
| Status Git | Ditambahkan pada task ini: 6 berkas source (lihat bagian 3.2) plus laporan ini. Berkas lain yang tampak `M` pada `git status --short` sudah ter-commit lewat merge `22441de9` |
| Langkah berikutnya | Jalankan `BIL-AT-116`–`119` sebagai request HTTP sungguhan begitu environment tersedia; tidak ada pekerjaan backend lain tersisa untuk task ini |
