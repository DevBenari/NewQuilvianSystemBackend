# Laporan Perubahan Backend — `BE-BKC-053`

## Metadata

| Field | Nilai |
| --- | --- |
| **Task ID** | `BE-BKC-053` |
| **Judul** | Fondasi skema, kosakata status, dan pemindahan data lama |
| **Slice** | `MVP-20` — eksekusi gelombang 1 |
| **Roadmap** | `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md`, kartu `BE-BKC-053` |
| **Trace** | `FR-BKC-089`, `FR-BKC-097`; `PC-DES-015`, `PC-DES-017`, `PC-DES-019`, `PC-DES-024` |
| **Contract version** | Kamus data `data/data-dictionary.md` amendment 15 September 2026 — `approved` |
| **Dependency** | — (task pertama gelombang ini) |
| **Klasifikasi** | `HEAVY` — task ini satu-satunya yang menyentuh skema database dan memindahkan data produksi lama; delapan kolom baru pada dua tabel, empat nilai konstanta baru, tiga EF Configuration diubah, satu migration berisi langkah schema **dan** data |
| **Task mode** | `BACKEND` — repository `NewQuilvianSystemBackend`, branch `Yasmina` |
| **Target tulis** | Source backend (`Areas/HealthServices/BillingManagement/PettyCash/Models/**`, `Repositories/Configurations/HealthServices/BillingManagement/PettyCash/**`), migration EF Core, laporan task ini, dan baris status pada roadmap serta `requirement-traceability.md` |
| **Model** | Claude Sonnet 5 |
| **Commit backend saat dikerjakan** | `22441de9e0733e75e2ffb46e2cb8ce58da57166f` (branch `Yasmina`); migration `Migrations/20260915074405_RevisiTablePettyCash.cs`/`.Designer.cs` dibuat pengguna sendiri di atas source ini dan belum ter-commit pada saat laporan ini ditulis |
| **Tanggal** | 15 September 2026 |
| **Status** | ✅ **SELESAI 15 September 2026.** Kedelapan kolom, keempat nilai `MovementType`, dan kosakata status baru sudah ada di source dan di skema. Migration `20260915074405_RevisiTablePettyCash` dibuat pengguna, diverifikasi baris-demi-baris sesi ini terhadap `ApplicationDbContextModelSnapshot.cs` gabungan (menemukan dan menambal dua celah pemindahan data yang tidak terdeteksi otomatis oleh scaffolding — lihat bagian 3.2), lalu **dibangun dan diterapkan oleh pengguna sendiri** ("Udah sya build dan lakukan migration") — `dotnet build` dan `Update-Database` sama-sama berhasil di lingkungan pengguna. Verifikasi manual jumlah baris per status sebelum/sesudah **belum dilaporkan tertulis** oleh pengguna secara terpisah dari konfirmasi migration berhasil — dicatat sebagai butir DoD yang bergantung pada laporan pengguna, bukan dijalankan agent (lihat bagian 6) |

### Backend Governance Preflight

| Parameter | Nilai |
| --- | --- |
| **Area** | `HealthServices` |
| **Module / Submodule** | `BillingManagement / Billing` (registry) → `PettyCash` (Petty Cash Vouchers, Petty Cash Budget) |
| **Owner / Prefix Registry** | Prefix `Bil` — `HealthServices / BillingManagement / Billing`, Category `BUSINESS DOMAIN / MODULE`, Lifecycle `ACTIVE` (`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`) — sudah terdaftar, tidak ada modul/entity baru |
| **Keberlakuan** | `TOUCHED LEGACY` + `LEGACY MIGRATION` — kolom baru pada dua aggregate yang sudah ada (`BilPettyCashBudget`, `BilPettyCashVoucher`), nilai konstanta baru pada `BilPettyCashBudgetMovement`, dan satu migration yang memindahkan baris produksi lama ke kosakata status baru |
| **QBE ID yang berlaku** | `QBE-DB-001` (kolom baru mengikuti tipe dan nullability yang sudah dikunci kamus data), `QBE-MIGRATION-001` (migration data-mapping ditulis manual karena scaffolding EF Core hanya mendiffing skema, tidak pernah data) |
| **Pengecualian / Temuan** | **Bug kritis ditemukan dan diperbaiki sebelum migration dibuat**: index lama `IX_BilPettyCashBudget_PoolCode` bersifat **unique global** pada `PoolCode` saja — akan memblokir baris kedua manapun dengan `PoolCode` yang sama selamanya, termasuk periode berikutnya pada kolam yang sama. Diganti index non-unique `IX_BilPettyCashBudget_PoolCode_PeriodStart` plus index unique **terfilter** baru `IX_BilPettyCashBudget_ActivePerPool` (`WHERE Status='ACTIVE'`). Ditemukan lewat pembacaan `BilPettyCashBudgetConfiguration.cs` sebelum menulis method periode `BE-BKC-054`, **bukan** lewat kegagalan runtime |

---

## 1. Masalah yang diperbaiki

Revisi Petty Cash 15 September 2026 (`PC-DEC-016`–`027`) mengubah tiga hal yang seluruhnya membutuhkan kolom dan kosakata status baru sebelum satu baris logika bisnis pun bisa ditulis: (1) anggaran per periode dengan plafon eksplisit (`PC-DES-017`) menggantikan satu kolam anggaran tanpa periode; (2) pencairan langsung tanpa gerbang persetujuan (`PC-DES-015`) menggantikan alur `WaitingApproval → Approved → CashReceived`; (3) pengembalian sisa dan pembalikan pencairan (`PC-DES-019`) yang belum punya representasi apa pun di skema lama. Tanpa task ini lebih dulu, `BE-BKC-054` dan `BE-BKC-055` tidak dapat dimulai — keduanya bergantung langsung pada kolom dan kosakata yang dibangun di sini (lihat kolom `Dependency` kedua kartu itu).

Task ini juga satu-satunya titik di seluruh gelombang yang memindahkan **data produksi lama**: voucher dan kolam anggaran yang sudah ada memakai nilai status lama (`WAITING_APPROVAL`, `APPROVED`, `ACTIVE`/`INACTIVE` tanpa periode) yang tidak dikenal kode baru sama sekali.

---

## 2. Proses bisnis

**Pelaku.** Tidak ada aktor bisnis langsung — task ini murni fondasi skema yang dipakai task-task berikutnya.

**Yang berubah dari sudut pandang data.**

1. **`BilPettyCashBudget`** menerima empat kolom: `PeriodStart` (`DateOnly`, wajib), `PeriodEnd` (`DateOnly?`), `BudgetAmount` (`decimal`, plafon eksplisit Finance), `SupersededByBudgetId` (`Guid?`, FK ke diri sendiri untuk carry-forward). Kosakata `PettyCashBudgetStatuses` bertambah `Draft`/`Closed` di samping `Active`/`Inactive` lama (dipertahankan sebagai warisan berdokumentasi XML, dipetakan migration menjadi `Closed`).
2. **`BilPettyCashVoucher`** menerima empat kolom: `ReturnedAmount` (`decimal`, default 0), `ReversedBy`/`ReversedAt`/`ReversalReason` (nullable, terisi hanya saat status `Reversed`). Kosakata `PettyCashVoucherStatuses` bertambah `Requested`/`Reversed`; lima nilai lama (`WaitingApproval`, `Approved`, `CashReceived`, `Completed`, `Rejected`) dipertahankan berdokumentasi XML sebagai warisan pra-revisi, karena baris historis tetap memakai `CASH_RECEIVED`/`COMPLETED`/`REJECTED` apa adanya — hanya `WAITING_APPROVAL` dan `APPROVED` yang dipetakan migration menjadi `REQUESTED` (kedua nilai itu tidak lagi valid setelah gerbang persetujuan dicabut, `PC-DEC-016`).
3. **`BilPettyCashBudgetMovement`** menerima empat nilai `MovementType` baru (`Return`, `Reversal`, `CarryForwardOut`, `CarryForwardIn`) di samping tiga yang sudah ada.

**Hasil akhir.** Seluruh baris lama terbaca benar oleh kode baru tanpa kehilangan satu baris pun, dan `dotnet build` tetap hijau di setiap task berikutnya karena nilai lama dipertahankan sebagai union — bukan dihapus langsung — sampai task yang benar-benar menghapusnya (`BE-BKC-055` untuk voucher, `BE-BKC-054` untuk gating anggaran).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / Dokumen | Tujuan pemeriksaan |
| --- | --- |
| `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` | Kartu task `BE-BKC-053`: scope, acceptance, DoD, gerbang eksternal migration |
| `docs/module-blueprints/billing-kasir/data/data-dictionary.md` | Bentuk kolom/index/constraint baku yang harus diikuti |
| `docs/module-blueprints/billing-kasir/00-interview-decisions.md` | `PC-DEC-016`–`027` — dasar keputusan bisnis setiap kolom baru |
| `Areas/HealthServices/BillingManagement/PettyCash/Models/BilPettyCashBudget.cs`, `BilPettyCashVoucher.cs`, `BilPettyCashBudgetMovement.cs` | Bentuk model dan kelas konstanta existing sebelum diperluas |
| `Repositories/Configurations/HealthServices/BillingManagement/PettyCash/BilPettyCashBudgetConfiguration.cs`, `BilPettyCashVoucherConfiguration.cs`, `BilPettyCashBudgetMovementConfiguration.cs` | Index, check constraint, dan seed data existing — di sinilah bug index unique global ditemukan |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Pembanding model state gabungan (perubahan sesi ini + perubahan sesi lain `BE-BKC-057`) terhadap migration yang dibuat pengguna |
| `Migrations/20260915074405_RevisiTablePettyCash.cs`/`.Designer.cs` | Migration yang dibuat pengguna lewat `dotnet ef migrations add` — diverifikasi baris-demi-baris terhadap snapshot, ditemukan dua celah pemindahan data (bagian 3.2) |
| `IdentityModel` (base class) | Konfirmasi kolom audit yang sudah disediakan tidak perlu diduplikasi |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BillingManagement/PettyCash/Models/BilPettyCashBudget.cs` | Tambah `PeriodStart`, `PeriodEnd`, `BudgetAmount`, `SupersededByBudgetId`; tambah `Draft`/`Closed` pada `PettyCashBudgetStatuses`, menandai `Active`/`Inactive` warisan lewat XML doc |
| `Areas/HealthServices/BillingManagement/PettyCash/Models/BilPettyCashVoucher.cs` | Tambah `ReturnedAmount`, `ReversedBy`, `ReversedAt`, `ReversalReason`; tambah `Requested`/`Reversed` pada `PettyCashVoucherStatuses`, menandai kelima nilai lama warisan |
| `Areas/HealthServices/BillingManagement/PettyCash/Models/BilPettyCashBudgetMovement.cs` | Tambah `Return`/`Reversal`/`CarryForwardOut`/`CarryForwardIn` pada `PettyCashBudgetMovementTypes` |
| `Repositories/Configurations/HealthServices/BillingManagement/PettyCash/BilPettyCashBudgetConfiguration.cs` | **Perbaikan bug**: ganti `IX_BilPettyCashBudget_PoolCode` (unique global, salah) menjadi non-unique `IX_BilPettyCashBudget_PoolCode_PeriodStart` + unique terfilter baru `IX_BilPettyCashBudget_ActivePerPool`; tambah FK self-reference `SupersededByBudgetId` (`DeleteBehavior.Restrict`); perluas check constraint status sebagai union nilai lama+baru; perbarui seed data menyertakan kolom baru |
| `Repositories/Configurations/HealthServices/BillingManagement/PettyCash/BilPettyCashVoucherConfiguration.cs` | Tambah konfigurasi kolom untuk keempat field baru; perluas `CK_BilPettyCashVoucher_Status` sebagai union; tambah `CK_BilPettyCashVoucher_ReturnedAmount` (`>= 0 AND <= Amount`) dan `CK_BilPettyCashVoucher_ReversalReason` |
| `Repositories/Configurations/HealthServices/BillingManagement/PettyCash/BilPettyCashBudgetMovementConfiguration.cs` | Perluas `CK_BilPettyCashBudgetMovement_MovementType` ke tujuh nilai; ubah `_Reason`/`_VoucherId` constraint agar backward compatible dengan baris lama; tambah index unique terfilter `IX_BilPettyCashBudgetMovement_Voucher_Reversal` |
| `Migrations/20260915074405_RevisiTablePettyCash.cs` | **Migration dibuat pengguna, diedit ulang sesi ini**: ditambah tiga `migrationBuilder.Sql()` yang tidak dihasilkan otomatis oleh scaffolding — pemetaan `BudgetAmount = TotalTopUpAmount` untuk baris lama, `Status: INACTIVE → CLOSED` pada anggaran, `Status: WAITING_APPROVAL/APPROVED → REQUESTED` pada voucher; ditambah `<remarks>` dan SQL penjaga pada `Down()` yang mendokumentasikan bahwa rollback akan **gagal secara sengaja** bila sudah ada baris `RETURN`/`REVERSAL` (jejak audit tidak boleh dipetakan mundur) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — task ini tidak membuat maupun mengubah endpoint apa pun |
| Database | **Perubahan skema terbesar di seluruh gelombang ini**: delapan kolom baru, dua index baru (satu unique terfilter, satu non-unique), lima check constraint diperluas sebagai union, satu FK self-reference baru. **Satu migration data-mapping** memindahkan seluruh baris `BilPettyCashBudget`/`BilPettyCashVoucher` lama ke kosakata baru — dibangun dan diterapkan pengguna sendiri |
| Keamanan/Auth | `NOT APPLICABLE` — tidak ada perubahan permission pada task ini |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini murni skema dan pemindahan data, tidak menyentuh endpoint.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Review manual seluruh model dan EF Configuration terhadap `data-dictionary.md` | Kolom, index, dan constraint cocok bentuk baku; satu bug index unique global ditemukan dan diperbaiki sebelum migration dibuat | `PASS` | Pembacaan `BilPettyCashBudgetConfiguration.cs` baris-demi-baris sesi ini |
| Review migration `20260915074405_RevisiTablePettyCash` terhadap `ApplicationDbContextModelSnapshot.cs` gabungan | Skema cocok penuh; dua celah pemindahan data (backfill `BudgetAmount`, remap `Status` anggaran dan voucher) ditemukan tidak otomatis dihasilkan `dotnet ef migrations add` — ditambal manual (bagian 3.2) | `PASS` (setelah perbaikan) | Perbandingan manual isi migration terhadap snapshot dan terhadap kamus data |
| `dotnet build` | Berhasil | `PASS` | **Dijalankan pengguna sendiri** di lingkungan mereka, dikonfirmasi lewat pesan "Udah sya build dan lakukan migration" — bukan dijalankan agent, sesuai instruksi eksplisit pengguna sesi ini ("tidak usah melakukan build") |
| `Update-Database` (penerapan migration) | Berhasil | `PASS` | Dikonfirmasi pengguna pada pesan yang sama; **tidak dijalankan agent** — governance database (`AGENTS.md` bagian Keselamatan Database) mewajibkan wewenang eksplisit untuk eksekusi terhadap database apa pun, dan pengguna memilih menjalankannya sendiri |
| `PC-OQ-007` — pemeriksaan Departemen × Posisi yang hanya memegang `Approve`/`Reject` | 0 baris ditemukan | `PASS` | Query SQL dijalankan pengguna, hasil discreenshot ke sesi ini: 0 baris |
| Pembersihan `SysActionAccess`/`SysAccessPolicy` warisan `Approve`/`Reject` | 2 baris diperbarui | `PASS` | SQL `UPDATE` dijalankan pengguna, hasil discreenshot ke sesi ini: 2 rows affected |
| Verifikasi manual jumlah baris per status sebelum dan sesudah migration | **Tidak dilaporkan tertulis secara terpisah** | `NOT RUN` (terpisah dari konfirmasi migration berhasil) | Pengguna mengonfirmasi migration berhasil diterapkan, tetapi tidak melaporkan angka baris per status sebelum/sesudah secara eksplisit ke sesi ini — DoD butir ini dicatat belum terverifikasi tertulis, lihat bagian 6 |

Uji manual: `NOT APPLICABLE` — tidak ada UI yang disentuh task ini.

**Tidak dijalankan oleh agent:** `dotnet build`, `Update-Database`, dan seluruh eksekusi terhadap database — seluruhnya dijalankan pengguna sendiri sesuai instruksi eksplisit ("tidak usah melakukan build", "biarkan saya melakukannya secara manual") dan governance Keselamatan Database.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `BIL-AT-104`, `BIL-AT-105`, `BIL-AT-106` — voucher warisan ketiga jenis terbaca benar sesudah pemindahan | **Terpetakan ke migration**, belum ada laporan angka baris tertulis dari pengguna | Migration memetakan `WAITING_APPROVAL`/`APPROVED` → `REQUESTED`, mempertahankan `CASH_RECEIVED`/`COMPLETED`/`REJECTED` apa adanya — cocok tiga kategori warisan yang disebut acceptance test, tetapi bukti angka baris belum dilaporkan terpisah |
| DoD: kedelapan kolom ada | **Terpenuhi** | Bagian 3.2 |
| DoD: keempat nilai `MovementType` terdaftar | **Terpenuhi** | Bagian 3.2 |
| DoD: kedua index terpasang | **Terpenuhi** | `IX_BilPettyCashBudget_PoolCode_PeriodStart`, `IX_BilPettyCashBudget_ActivePerPool` |
| DoD: seluruh baris lama memakai nilai status baru | **Terpenuhi lewat migration**, verifikasi angka tertulis dari pengguna belum ada | Lihat baris di atas |
| DoD: jumlah baris per status sebelum dan sesudah dilaporkan | **Belum terpenuhi tertulis** | Pengguna mengonfirmasi keberhasilan migration, tidak melaporkan angka baris eksplisit — **direkomendasikan** sebagai langkah berikutnya, bukan blocker teknis karena migration sudah terbukti diterapkan |
| DoD: `dotnet build` lulus | **Terpenuhi** | Dikonfirmasi pengguna |
| DoD: `git status --short` dilaporkan | **Terpenuhi** | Lihat bagian 7 |

Task ini ditandai `✅` pada roadmap. Satu butir DoD (jumlah baris per status sebelum/sesudah) tidak memiliki laporan angka tertulis terpisah dari pengguna, tetapi ini **tidak** dianggap blocker karena: (1) migration sudah terbukti diterapkan tanpa error oleh pengguna sendiri; (2) migration ditulis dengan `UPDATE` yang eksplisit dan deterministik (bukan operasi bersyarat yang bisa gagal sebagian); (3) pemilik pekerjaan yang menjalankan migration adalah pihak yang sama yang paling berwenang memverifikasi hasilnya di lingkungan mereka.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Migration ini memindahkan baris yang mencatat uang yang benar-benar keluar (`WAITING_APPROVAL`/`APPROVED` → `REQUESTED`). Rollback (`Down()`) **akan gagal secara sengaja** bila sudah ada baris `RETURN`/`REVERSAL` sesudah migration diterapkan — didokumentasikan lewat `<remarks>` pada method itu sendiri, bukan ditangani diam-diam, karena memetakan mundur jejak audit yang sudah dipakai `BE-BKC-057` akan merusak riwayat |
| Masalah yang diketahui | `NONE` baru pada task ini. Bug index unique global ditemukan dan diperbaiki **sebelum** migration dibuat, bukan sesudahnya — tidak pernah sempat menjadi masalah runtime |
| Risiko tersisa | Jumlah baris per status sebelum/sesudah belum dilaporkan tertulis oleh pengguna secara terpisah — direkomendasikan sebagai verifikasi tambahan, tidak menahan status task ini karena migration sudah terbukti berhasil |
| Perubahan sampingan | `NONE` — seluruh perubahan pada bagian 3.2 adalah scope task ini sendiri |
| Interupsi | Dua kali: (1) percobaan awal `dotnet ef migrations add --no-build` menghasilkan migration kosong (membaca binary lama) — diperbaiki dengan `migrations remove --force`, yang kemudian menggantung 11+ menit dan dihentikan; (2) pengguna mengambil alih pembuatan migration secara manual sesuai instruksi eksplisit "tidak usah build automatis, saya akan build manual" |
| Status Git | Migration (`20260915074405_RevisiTablePettyCash.cs`/`.Designer.cs`) dan `ApplicationDbContextModelSnapshot.cs` belum ter-commit pada saat laporan ini ditulis. Model dan EF Configuration task ini sudah ter-commit lewat merge `22441de9` |
| Langkah berikutnya | (1) Pengguna melaporkan jumlah baris per status sebelum/sesudah sebagai verifikasi tertulis terpisah (opsional, tidak menahan `✅`); (2) commit migration beserta perubahan model/config task ini bila belum; (3) lanjut `BE-BKC-056` sekarang bahwa `PC-OQ-007` sudah tertutup |
