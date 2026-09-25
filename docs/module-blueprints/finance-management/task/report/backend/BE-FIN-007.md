# Laporan Perubahan Backend — `BE-FIN-007`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-007` |
| Judul | Migration `AddFinanceBillingIntake` dan `AddFinanceReceivableAndCollection` |
| Slice | `MVP-1` — Pintu masuk fakta dan buku piutang (`EPIC FIN-02`, `FIN-03`) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian 3 (tabel task) dan bagian 4 (`MVP-1`) |
| Trace | `FIN-DES-001`; urutan migration sesuai `02-backend-architecture.md` §7 (migration 2 dan 3 dari 7) |
| Contract version | `NOT APPLICABLE` — migration tidak membawa kontrak API baru |
| Dependency | `BE-FIN-006` — 🟡 sebagian 21 September 2026 (5 entity+configuration piutang selesai), lihat [laporan](BE-FIN-006.md) |
| Klasifikasi | `HEAVY` — dampak database (6 tabel baru lintas 2 migration, FK, check constraint); berkas snapshot besar tersentuh; **gap cakupan ditemukan** (2 tabel Collection tidak dapat dibuat, lihat bagian 1) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Migrations/20260921000001_AddFinanceBillingIntake.cs` (+`.Designer.cs`), `Migrations/20260921000002_AddFinanceReceivableAndCollection.cs` (+`.Designer.cs`), `Migrations/ApplicationDbContextModelSnapshot.cs` (penambahan), `Repositories/ApplicationDbContext.cs` (perbarui komentar saja) |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | Working tree pada branch `Yasmina`; commit dasar `09101d0581695e20345a9efa8af3fce7c38b1ae4` |
| Tanggal | 21 September 2026 |
| Status | ✅ **SELESAI 23 September 2026 untuk cakupan task ini.** Migration 1 mencakup penuh (1 tabel, sesuai rencana). Migration 2 mencakup **5 dari 7 tabel** — 2 tabel Collection (`FinReceipt`, `FinReceiptAllocation`) sengaja tidak dibuat di sini (temuan bagian 1); keduanya belakangan dibangun sebagai bagian `BE-FIN-016` (masih `BLOCKED` menunggu Owner Billing, dilacak terpisah di sana). Migration task ini sudah dieksekusi ke database — dikonfirmasi pengguna 23 September 2026, lihat Pembaruan bagian 7 |

---

## 0. Otorisasi dan metode pembuatan

Pemilik repository meminta secara eksplisit pada task ini: **"Buatkan file migration dan
sesuaikan dengan app db context"** — persis pola otorisasi yang sama dengan `BE-FIN-003`
(ditulis tangan, tanpa `dotnet ef migrations add`, tidak dijalankan ke database). "Sesuaikan
dengan app db context" ditafsirkan secara harfiah: migration mengikuti **entity yang benar-benar
ada** di `ApplicationDbContext` saat ini, bukan rencana tujuh tabel pada roadmap — inilah yang
membuka temuan pada bagian 1.

---

## 1. Temuan: migration 2 tidak bisa mencakup 7 tabel seperti rencana roadmap

Roadmap `BE-FIN-007` mencantumkan Cakupan "1 + 7 tabel, aditif", dan `02-backend-architecture.md`
§7 menyebut migration `AddFinanceReceivableAndCollection` sebagai **"5 tabel piutang + 2 tabel
penerimaan"**. Sebelum menulis migration, saya memeriksa `ApplicationDbContext` dan menggrep
seluruh `Areas/` untuk `FinReceipt`/`FinReceiptAllocation`/`class\s+\w*Receipt\w*` — **nol hasil**.
Tidak ada satu pun entity Collection yang pernah dibuat, dan tidak ada satu pun task pada roadmap
(`BE-FIN-005`..`009`) yang secara eksplisit memiliki `FinReceipt`/`FinReceiptAllocation` pada
Cakupan-nya.

Ini melengkapi dua gap roadmap yang sudah dilaporkan sebelumnya (`BE-FIN-005` bagian 1 poin 3:
`FinanceBillingIntakeService` tanpa pemilik; `BE-FIN-006` bagian 7: kemungkinan `BE-FIN-008`
menutup gap `FinanceReceivableService`) — sekarang jelas **entity Collection sendiri** juga belum
punya pemilik task, bukan hanya service-nya. Membuat `FinReceipt`/`FinReceiptAllocation` sendiri
pada task ini akan jauh melampaui wewenang "buatkan file migration": kedua entity itu punya
desain sendiri yang belum diperiksa pada sesi ini (`FIN-DES-011` — `FinReceipt` tidak pernah
membuat `FinReceivable`; kunci idempotensi `SourceTenderId` dengan partial unique index,
`FIN-DES-010`; kolom `IsReversal`/`ReversalOfAllocationId`, `FIN-DES-012`) — mengarangnya di sini
berisiko salah dan melanggar batas vertical-slice yang diberi wewenang.

**Keputusan yang diambil**: migration `AddFinanceReceivableAndCollection` dibuat **hanya untuk 5
tabel piutang** yang benar-benar ada di `ApplicationDbContext` (dari `BE-FIN-006`). Nama migration
dipertahankan persis seperti roadmap (bukan diganti sepihak), mengikuti preseden `BE-FIN-003`
(nama `AddFinanceMasterData` dipertahankan walau isinya 3 dari 4 tabel). Cakupan "2 tabel
Collection" dicatat sebagai **belum terpenuhi**, bukan didiamkan.

**Rekomendasi ke pass desain**: buat task roadmap baru (atau perluas `BE-FIN-016`/`017` yang
sudah `BLOCKED` menunggu owner Billing?) untuk `FinReceipt`/`FinReceiptAllocation` beserta
migration susulannya — kemungkinan migration terpisah, bukan menyisip ke migration yang sudah
ada, karena migration ini sudah dianggap "selesai ditulis" untuk cakupannya sendiri.

---

## 2. Ringkasan pekerjaan

### 2.1 Menyusul-lengkapi `ApplicationDbContextModelSnapshot.cs`

`BE-FIN-005` dan `BE-FIN-006` menulis entity+configuration tetapi **tidak** memperbarui
`ApplicationDbContextModelSnapshot.cs` (di luar cakupan kedua task itu — murni entity+config).
Akibatnya snapshot sempat tidak sinkron dengan `ApplicationDbContext` yang sebenarnya. Sebelum
menulis migration baru, saya menyisipkan (bukan menulis ulang) blok `FinBillingHandoffIntake` dan
kelima entity `FinReceivable*` ke posisi alfabetis yang benar pada tiga bagian snapshot (definisi
properti, definisi relasi, definisi navigation-only) — pola identik `BE-FIN-003`.

### 2.2 Migration `AddFinanceBillingIntake` (`20260921000001`)

1 tabel: `FinBillingHandoffIntake`, dua check constraint (`HandoffType`, `Status`), dua index
(`IX_FinBillingHandoffIntake_Identity` unik parsial, `IX_FinBillingHandoffIntake_Status`). Nol
FK — `SourceHandoffId` sengaja bukan FK (lintas bounded context, `02-backend-architecture.md` §4.1).

`.Designer.cs`-nya **sengaja tidak** menyertakan lima tabel piutang — mencerminkan bahwa pada
titik migration ini, tabel piutang belum ada (dibuat migration 2). Ini beda dari cara saya
menyalin `.Designer.cs` pada `BE-FIN-003` (yang saat itu adalah satu-satunya migration baru pada
sesi itu) — di sini, karena dua migration ditulis bersamaan, `.Designer.cs` migration 1 disunting
manual agar hanya berisi model **sampai** migration 1, bukan model akhir. Tanpa penyesuaian ini,
riwayat migration akan salah menggambarkan tabel piutang sudah ada sejak migration 1.

### 2.3 Migration `AddFinanceReceivableAndCollection` (`20260921000002`)

5 tabel: `FinReceivable` (induk, 5 check constraint termasuk invariant seimbang), lalu
`FinReceivableAdjustment`, `FinReceivableDocument`, `FinReceivableItem`, `FinReceivableWriteOff`
(anak, FK `Restrict` ke `FinReceivable`). Urutan `CreateTable` menaruh induk lebih dulu; `Down()`
menghapus anak lebih dulu (urutan terbalik) agar FK tidak menghalangi `DropTable`.

`.Designer.cs`-nya berisi model akhir lengkap (termasuk `FinBillingHandoffIntake` dari migration
1) — benar, karena inilah target model setelah kedua migration diterapkan berurutan.

### 2.4 Pembaruan komentar `ApplicationDbContext.cs`

Komentar pada `DbSet<FinBillingHandoffIntake>` dan `DbSet<FinReceivable>` diperbarui — sebelumnya
menyebut "migration BE-FIN-007 belum dikerjakan" (sudah basi sejak task ini berjalan), kini
menyebut migration sudah dibuat-tapi-belum-dijalankan, dan mencatat gap Collection secara
eksplisit di titik yang akan dibaca implementer berikutnya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/02-backend-architecture.md` §7 (urutan dan isi 7 migration), §1.2 (kepemilikan data)
- `docs/module-blueprints/finance-management/01-existing-capability-map.md` — `FIN-CAP-009` (Collection Missing)
- Grep penuh `Areas/` untuk `FinReceipt`, `FinReceiptAllocation`, `class\s+\w*Receipt\w*` — nol hasil
- Laporan `BE-FIN-005.md`, `BE-FIN-006.md` — gap `FinanceBillingIntakeService`/`FinanceReceivableService` yang sudah dilaporkan sebelumnya
- `Migrations/20260921000000_AddFinanceMasterData.cs`/`.Designer.cs` (`BE-FIN-003`) — pola migration tangan + penyalinan snapshot yang dipakai ulang
- `Areas/Corporate/FinanceManagement/BillingIntake/{Models,Configurations}/FinBillingHandoffIntake*`, `Areas/Corporate/FinanceManagement/Receivable/{Models,Configurations}/FinReceivable*` (`BE-FIN-005`/`006`) — sumber kolom/index/constraint yang diterjemahkan ke migration

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Migrations/20260921000001_AddFinanceBillingIntake.cs` | **Baru.** `Up()`/`Down()` untuk `FinBillingHandoffIntake` |
| `Migrations/20260921000001_AddFinanceBillingIntake.Designer.cs` | **Baru.** Snapshot model sampai dengan migration ini (tanpa tabel piutang) |
| `Migrations/20260921000002_AddFinanceReceivableAndCollection.cs` | **Baru.** `Up()`/`Down()` untuk 5 tabel piutang. **Tidak** mencakup 2 tabel Collection — lihat bagian 1 |
| `Migrations/20260921000002_AddFinanceReceivableAndCollection.Designer.cs` | **Baru.** Snapshot model akhir lengkap |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Disisipkan blok `FinBillingHandoffIntake` + 5 `FinReceivable*` (properti, relasi, navigation-only) — menyusulkan pembaruan yang tertinggal dari `BE-FIN-005`/`006` |
| `Repositories/ApplicationDbContext.cs` | Komentar pada dua `DbSet` diperbarui agar tidak lagi menyebut migration "belum dikerjakan" |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` |
| Database | 6 tabel baru siap diterapkan (1 + 5), aditif, nol tabel existing tersentuh. **Belum dieksekusi** — otorisasi eksekusi migration tetap terpisah, belum diberikan |
| Keamanan/Auth | `NOT APPLICABLE` |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet ef migrations add` | **Sengaja tidak dijalankan** | `NOT RUN` | Instruksi eksplisit pemilik repository — migration ditulis tangan |
| `dotnet ef migrations list --no-build --verbose --configuration Release` | Dijalankan **pengguna** 21 September 2026 terhadap database `QuilvianNewDevYasmina` (`160.22.250.77`): `20260921000001_AddFinanceBillingIntake (Pending)`, `20260921000002_AddFinanceReceivableAndCollection (Pending)` — keduanya dikenali valid, terkoneksi nyata ke `__EFMigrationsHistory` | `PASS` | Kedua migration muncul benar berurutan setelah `20260921000000_AddFinanceMasterData`, seluruhnya `Pending` |
| `dotnet ef database update` / eksekusi migration | **Tidak dijalankan** | `NOT RUN` | Otorisasi eksekusi terpisah, belum diminta maupun diberikan |
| `dotnet build` | **Tidak dijalankan oleh saya** | `NOT RUN` | Atas instruksi pengguna sejak `BE-FIN-002`; kini mencakup 7 task berturut-turut yang belum pernah dikompilasi |
| `powershell.exe -NoProfile -File tooling/qbe/Invoke-QbeConformanceCheck.ps1` | Percobaan pertama pada sesi ini terputus (sesi sempat berhenti sebelum selesai, tanpa hasil); dijalankan ulang setelah `BE-FIN-008` selesai (working tree kumulatif mencakup berkas migration task ini): `Files evaluated: 34`, `VIOLATION: 0`, `Final result: PASS` | `PASS` | Kutipan di bawah |
| Review manual: kolom/index/constraint migration dicocokkan satu-per-satu terhadap `FinBillingHandoffIntakeConfiguration.cs` dan lima `FinReceivable*Configuration.cs` | Cocok persis | `PASS` | Perbandingan manual pada sesi ini |
| Review manual: urutan `CreateTable` (induk sebelum anak) dan `Down()` (anak sebelum induk) pada migration 2 | `FinReceivable` dibuat pertama; empat anak sesudahnya; `Down()` menghapus keempat anak dulu | `PASS` | `20260921000002_AddFinanceReceivableAndCollection.cs` |
| Review manual: `.Designer.cs` migration 1 tidak memuat tabel piutang (konsistensi riwayat migration) | Dikonfirmasi — 0 kemunculan kata "Receivable" pada berkas itu, seluruh 5 blok Receivable dihapus manual dari salinan snapshot | `PASS` | `grep -c Receivable 20260921000001_AddFinanceBillingIntake.Designer.cs` → `0` |
| Review scope: grep `FinReceipt`/`FinReceiptAllocation` sebelum menulis migration | Nol hasil di seluruh `Areas/` — dikonfirmasi tidak ada entity untuk dimigrasikan | `PASS` | Bagian 1 |

Keluaran `Invoke-QbeConformanceCheck.ps1`:

```
QBE Conformance Report
Checker mode: ReportOnly
Scope: WorkingTree
Files evaluated: 34
Generated files excluded (bin/obj): 0
Test-scope files excluded from QBE-ENT-001/QBE-CFG-001/QBE-MOD-002: 0
VIOLATION: 0
REVIEW: 0
INFO: 0
Findings: none
REPORT ONLY: No enforcement/blocking performed.
Final result: PASS
```

`AUTOMATED TEST: NOT APPLICABLE` — backend tidak memelihara project test otomatis.

Uji manual: `NOT APPLICABLE` — migration belum dijalankan, tidak ada database untuk diuji.

**Pembaruan 21 September 2026**: pengguna menjalankan `dotnet build` (Release, berhasil) dan
`dotnet ef migrations list --configuration Release` — keduanya migration task ini dikenali valid
dan `Pending` terhadap database `QuilvianNewDevYasmina` sungguhan (lihat tabel di atas). Risiko
"migration ditulis tangan tanpa verifikasi tooling" yang tercatat di bawah **sudah terbukti tidak
terjadi**; baris `NOT RUN` dipertahankan sebagai riwayat, bukan dihapus. `dotnet ef database
update` (eksekusi sungguhan) tetap belum dijalankan — itu wewenang terpisah yang belum diberikan.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria (persis roadmap) | Status | Bukti |
| --- | --- | --- |
| Urutan migration 2 lalu 3 sesuai `02-backend-architecture.md` bagian 7 | Terpenuhi — `AddFinanceBillingIntake` (`20260921000001`) sebelum `AddFinanceReceivableAndCollection` (`20260921000002`) | Nama berkas dan urutan `CreateTable`/`Down()` |
| Migration dijalankan | **Belum** — otorisasi eksekusi terpisah belum diberikan | Bagian 0, 5 |
| Nol tabel existing tersentuh | Terpenuhi — kedua migration murni `CreateTable`/`CreateIndex`, tidak menyentuh tabel yang sudah ada | Kedua berkas migration |
| Cakupan "1 + 7 tabel" | 🟡 **Sebagian** — 1 + 5 = 6 dari 8 tabel yang direncanakan. 2 tabel Collection tidak dapat dibuat (bagian 1) | Bagian 1 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| **Pembaruan 23 September 2026** | Pengguna mengonfirmasi migration task ini sudah dieksekusi ke database, `dotnet build` PASS. Gap `FinReceipt`/`FinReceiptAllocation` pada baris Peringatan di bawah sejak itu sudah punya task pemilik (`BE-FIN-016`, masih `BLOCKED` menunggu Owner Billing — dilacak di sana, bukan di sini). Status task ini dinaikkan menjadi ✅ SELESAI |
| Peringatan | **Gap roadmap baru**: `FinReceipt`/`FinReceiptAllocation` belum punya task pemilik (bagian 1) — perlu keputusan pass desain sebelum migration Collection bisa ditulis. Ini melengkapi (bukan menggantikan) gap `FinanceBillingIntakeService`/`FinanceReceivableService` yang sudah dilaporkan `BE-FIN-005`/`006` |
| Masalah yang diketahui | Tidak ada yang baru di luar yang sudah dicatat |
| Risiko tersisa | **Rendah** (diturunkan dari "Sedang-tinggi" semula) — `dotnet build` dan `dotnet ef migrations list` sudah membuktikan kedua migration valid terhadap database sungguhan. Risiko yang tersisa hanya pada eksekusi sungguhan (`dotnet ef database update`) dan pada gap `FinReceipt`/`FinReceiptAllocation` (bagian 1) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` pada task ini |
| Status Git | 6 berkas migration baru (3 pasang `.cs`/`.Designer.cs`), `ApplicationDbContextModelSnapshot.cs` bertambah, `ApplicationDbContext.cs` komentar diperbarui — di atas berkas `BE-FIN-002`..`006` yang sudah ada |
| Langkah berikutnya | (1) `dotnet build` mencakup seluruh `BE-FIN-002`..`007`. (2) Pass desain memutuskan pemilik task untuk `FinReceipt`/`FinReceiptAllocation` (bagian 1) sebelum migration Collection dapat ditulis. (3) Otorisasi eksekusi migration terpisah. (4) `BE-FIN-008` (`FinanceReceivableService`) dapat mulai ditulis terhadap 5 tabel piutang yang sudah ada, tidak perlu menunggu Collection |
