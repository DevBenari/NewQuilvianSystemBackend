# Laporan Perubahan Backend — `BE-FIN-010`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-010` |
| Judul | Kotak keluar kejadian Accounting — `FinAccountingEventOutbox`, `FinAccountingEventAttempt` + migration `AddFinanceAccountingOutbox` |
| Slice | `MVP-5` — Kotak keluar kejadian (`EPIC FIN-11`) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian 3 (tabel task) dan bagian 4 (`MVP-5`) |
| Trace | `FIN-DEC-002`, `FIN-DEC-004`, `FIN-DES-017`, `FIN-DES-018`, `FIN-DES-019`; `FR-FIN-070`..`075`; kontrak `FIN-INTEGRATION-1.0` §5 (`ACC-XMOD-0.2`) |
| Contract version | `NOT APPLICABLE` — task ini tidak membawa kontrak API baru (Controller/DTO ada di `BE-FIN-011`/`012`) |
| Dependency | `BE-FIN-007` — 🟡 sebagian 21 September 2026 (migration 1+2 ditulis tangan, belum dijalankan), lihat [laporan](BE-FIN-007.md). Migration task ini (`AddFinanceAccountingOutbox`) hanya butuh migration 1-3 ada lebih dulu secara berurutan, tidak bergantung pada `MstBankAccount` |
| Klasifikasi | `HEAVY` — dampak database (2 tabel baru, FK, 2 check constraint, 2 unique index); berkas snapshot besar tersentuh |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/AccountingIntegration/Models/*.cs` (baru), `Repositories/Configurations/Corporate/FinanceManagement/AccountingIntegration/*.cs` (baru), `Repositories/ApplicationDbContext.cs` (DbSet+using), `Migrations/20260921000003_AddFinanceAccountingOutbox.cs`+`.Designer.cs` (baru), `Migrations/ApplicationDbContextModelSnapshot.cs` (penambahan) |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | Working tree pada branch `Yasmina`; commit dasar `09101d0581695e20345a9efa8af3fce7c38b1ae4` |
| Tanggal | 21 September 2026 |
| Status | ✅ **SELESAI 23 September 2026.** Cakupan task (2 tabel + migration) terpenuhi penuh sesuai roadmap. `FinSubledgerPeriodBalance` **sengaja tidak** dibuat — bukan bagian Cakupan `BE-FIN-010` (lihat bagian 1). `dotnet build` PASS dan migration sudah dieksekusi — dikonfirmasi pengguna 23 September 2026, lihat Pembaruan bagian 7 |

---

## 0. Otorisasi dan metode pembuatan

Berbeda dari `BE-FIN-005`/`006` (entity+config murni, migration dipisah ke `BE-FIN-007`), roadmap
`BE-FIN-010` membundel migration **dalam task yang sama** ("Cakupan: `FinAccountingEventOutbox`,
`FinAccountingEventAttempt` + migration `AddFinanceAccountingOutbox`"), namun kolom Risiko/pemilik
tetap mencatat eksplisit **"migration butuh otorisasi terpisah"**. Mengikuti pola yang sudah
berjalan sejak `BE-FIN-003`, saya tidak berasumsi bundling Cakupan = otorisasi migration, dan
bertanya lebih dulu kepada pemilik repository setelah entity+configuration selesai. Jawaban yang
diterima: **"Buat file migration (tanpa menjalankan)"** — migration ditulis tangan, tanpa
`dotnet ef migrations add`, tidak dijalankan ke database. Pola ini identik `BE-FIN-003`/`007`.

---

## 1. Keputusan desain yang diambil (bukan gap — didokumentasikan untuk transparansi)

### 1.1 `EventTypeCode` tidak diberi check constraint

Data dictionary (`erd/data-dictionary.md` §7.1) hanya membatasi `EventTypeCode` sebagai
`varchar(50)` wajib, **tanpa** daftar nilai yang diizinkan di level database — berbeda dari
`DeliveryStatus` dan `CurrencyCode` yang eksplisit punya `CHECK ... IN (...)`/`CHECK ... = 'IDR'`.
Ini konsisten dengan `contracts/integration-contract.md` §5.4: katalog 17 kode (`FIN-DEC-002`)
masih **"disetujui sisi Finance, menunggu ratifikasi Accounting"** — belum final, dan `AccEventType`
milik Accounting sendiri belum diisi seeder (`ACC-DEC-075`: "kejadian berjenis yang belum
terdaftar akan berstatus Tertahan di sisi Accounting", bukan ditolak Finance). Memasang check
constraint pada kode yang belum diratifikasi akan mengunci Finance ke daftar yang bisa berubah.
17 kode itu tetap dicatat sebagai `FinAccountingEventTypeCodes` (kelas konstanta string) di
`FinAccountingEventOutbox.cs` untuk dipakai `FinanceAccountingOutboxService` (`BE-FIN-011`) nanti,
tanpa mengikat skema database padanya.

### 1.2 `FinSubledgerPeriodBalance` sengaja TIDAK dibuat

`erd/data-dictionary.md` §9.5 menaruh DDL `FinSubledgerPeriodBalance` bersebelahan dengan kedua
tabel outbox, dan `02-backend-architecture.md` §5 (file tree) mencantumkan
`Models/FinSubledgerPeriodBalance.cs` di folder yang sama. Namun roadmap `BE-FIN-010` (tabel task
bagian 3) hanya mencantumkan `FinAccountingEventOutbox`, `FinAccountingEventAttempt` pada kolom
Cakupan, dan `erd/accounting-integration.md` §2 menyebut field-nya sendiri **"masih draf, menunggu
`FIN-OQ-011`"** (open question belum terjawab). Membuatnya di task ini akan melampaui wewenang dan
berisiko salah karena nama kolomnya belum final. **Tidak dibuat** — akan jadi task terpisah
setelah `FIN-OQ-011` terjawab.

### 1.3 Cakupan file dibatasi ketat pada Model + Configuration + migration

`02-backend-architecture.md` §5 (file tree) mencantumkan juga `Controllers/FinanceAccountingEventsController.cs`,
`Dtos/FinanceAccountingEventDtos.cs`, `Services/FinanceAccountingOutboxService.cs`, dan
`Services/FinanceAccountingDeliveryWorker.cs` di folder `AccountingIntegration/` yang sama. Roadmap
menaruh `FinanceAccountingOutboxService` secara eksplisit di `BE-FIN-011` (dependency: `BE-FIN-010`)
dan delivery worker di `EPIC FIN-12` yang berstatus `OPEN DECISION`
("**MUST NOT masuk gelombang pengiriman mana pun**" — endpoint Accounting belum dibangun,
`FIN-CAP-018`). Task ini murni entity+configuration+migration, tidak menyentuh keempatnya.

---

## 2. Ringkasan pekerjaan

### 2.1 Entity dan configuration

`FinAccountingEventOutbox` (induk) — 24 kolom, dua lapis anti-dobel (`IX_..._EventNumber` unik
parsial; `IX_..._SourceIdentity` unik parsial atas `SourceModule`+`SourceTransactionId`+
`EventTypeCode`+`SourceVersion`, `FIN-DES-019`), index `DeliveryStatus`+`AccountingDate`, dua check
constraint (`CurrencyCode = 'IDR'`; `DeliveryStatus IN (...)` enam nilai termasuk
`HELD_FOR_FINALIZATION`, `FIN-DES-018`), `RowVersion` optimistic concurrency. Navigasi
`ICollection<FinAccountingEventAttempt> Attempts`.

`FinAccountingEventAttempt` (anak, append-only, **tanpa** `RowVersion`) — FK `Restrict` ke
`Outbox`, unique index gabungan `(OutboxId, AttemptNumber)` yang sekaligus menutupi kebutuhan index
FK by-convention (tidak ada index tunggal `OutboxId` terpisah — konsisten pola EF Core yang sudah
dipakai `FinReceivableItem` dkk pada `BE-FIN-006`/`007`).

Kedua file `*Configuration.cs` mengikuti pola persis `FinReceivableConfiguration`/
`FinReceivableItemConfiguration` (`BE-FIN-006`): `ToTable` dengan check constraint di delegate,
`HasIndex().HasFilter("\"IsDelete\" = false")` untuk index unik parsial, kolom audit `IdentityModel`
diberi `HasColumnType`/`HasDefaultValue` seragam.

### 2.2 `ApplicationDbContext.cs`

`using` baru untuk namespace `AccountingIntegration.Models` (disisipkan alfabetis sebelum
`BillingIntake.Models`); dua `DbSet` baru disisipkan setelah `FinReceivableWriteOffs` dengan
komentar yang mencatat migration belum dijalankan dan bahwa worker/endpoint di luar lingkup task
ini.

### 2.3 `ApplicationDbContextModelSnapshot.cs`

Disisipkan tiga blok pada posisi alfabetis yang benar (pola identik `BE-FIN-003`/`007`):
properti `FinAccountingEventAttempt` lalu `FinAccountingEventOutbox` (sebelum blok
`FinBillingHandoffIntake`, karena `AccountingIntegration` < `BillingIntake` alfabetis) di bagian
definisi properti; blok relasi `FinAccountingEventAttempt.HasOne(...Outbox...)` sebelum blok
`MstBankAccount` di bagian relasi (`FinBillingHandoffIntake` sendiri tidak muncul di bagian ini —
tidak punya FK); blok navigation-only `FinAccountingEventOutbox.Navigation("Attempts")` sebelum
blok `FinPettyCashBudget` di bagian navigation-only.

### 2.4 Migration `AddFinanceAccountingOutbox` (`20260921000003`)

Migration keempat berturut-turut (setelah `20260921000000/1/2`). `Up()`: `CreateTable` Outbox lalu
Attempt (induk lebih dulu, FK anak mengarah ke induk), lalu empat `CreateIndex` (dua unik parsial
Outbox, satu index biasa Outbox, satu unik Attempt). `Down()`: `DropTable` Attempt lalu Outbox
(urutan terbalik, FK tidak menghalangi). `.Designer.cs` dibuat dengan menyalin
`ApplicationDbContextModelSnapshot.cs` yang **sudah** disisipi (2.3) — sah karena migration ini
adalah migration terakhir yang ada saat ini, sehingga model kumulatifnya = seluruh model saat ini,
tidak perlu dipangkas seperti migration 1 pada `BE-FIN-007` (yang harus mengecualikan tabel
piutang yang lahir belakangan).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian 3 (baris `BE-FIN-010`) dan bagian 4 (`MVP-5`)
- `docs/module-blueprints/finance-management/erd/data-dictionary.md` §7.1/§7.2 (kolom) dan §9.5 (DDL)
- `docs/module-blueprints/finance-management/erd/accounting-integration.md` (seluruh berkas — status entity, dua lapis anti-dobel, `HELD_FOR_FINALIZATION`, larangan data pasien, `FIN-CAP-018`)
- `docs/module-blueprints/finance-management/02-backend-architecture.md` §2.7 (`FIN-DES-017..020`), §3.6 (class diagram), §4.19/§4.20 (rincian entity), §5 (file tree), §7 (urutan migration), §9 (invariant)
- `docs/module-blueprints/finance-management/00-interview-decisions.md` (`FIN-DEC-001`, `FIN-DEC-002`, `FIN-DEC-004`, aturan bisnis #4/#5/#6)
- `docs/module-blueprints/finance-management/contracts/integration-contract.md` §5 (katalog 17 kode, 12 field wajib, penanganan balasan, larangan)
- `docs/module-blueprints/finance-management/04-prd-to-mvp.md` (`FR-FIN-070`..`075`, `UAT-07`/`17`/`18`/`19`, `EPIC FIN-12` `OPEN DECISION`)
- `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` — baris `AccountingIntegration` (`Fin`, `ACTIVE`, sudah terdaftar sejak `BE-FIN-001`)
- Grep `FinAccountingEventOutbox`/`FinAccountingEventAttempt` di seluruh `Areas/`/`Repositories/`/`Migrations/` — nol hasil sebelum task ini (greenfield)
- `Areas/Corporate/AccountingManagement/MasterData/EventType/Models/AccEventType.cs`, `.../PostingRule/Models/AccPostingRule.cs` — dikonfirmasi konsep terpisah (sisi penerima/posting Accounting), tidak ada yang dipakai ulang
- `Areas/Corporate/FinanceManagement/BillingIntake/{Models,Configurations}/FinBillingHandoffIntake*`, `Areas/Corporate/FinanceManagement/Receivable/{Models,Configurations}/FinReceivable*` (`BE-FIN-005`/`006`) — pola konvensi yang direplikasi
- `Migrations/20260921000002_AddFinanceReceivableAndCollection.cs`/`.Designer.cs` (`BE-FIN-007`) — pola migration tangan + penyalinan snapshot yang dipakai ulang

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Models/FinAccountingEventOutbox.cs` | **Baru.** Entity + `FinAccountingEventDeliveryStatuses`, `FinAccountingEventSourceModules`, `FinAccountingEventTypeCodes` |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Models/FinAccountingEventAttempt.cs` | **Baru.** Entity append-only |
| `Repositories/Configurations/Corporate/FinanceManagement/AccountingIntegration/FinAccountingEventOutboxConfiguration.cs` | **Baru** |
| `Repositories/Configurations/Corporate/FinanceManagement/AccountingIntegration/FinAccountingEventAttemptConfiguration.cs` | **Baru** |
| `Repositories/ApplicationDbContext.cs` | `using` baru + 2 `DbSet` baru dengan komentar |
| `Migrations/20260921000003_AddFinanceAccountingOutbox.cs` | **Baru.** `Up()`/`Down()` untuk 2 tabel |
| `Migrations/20260921000003_AddFinanceAccountingOutbox.Designer.cs` | **Baru.** Snapshot model kumulatif sampai migration ini (= model saat ini) |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Disisipkan blok `FinAccountingEventAttempt`+`FinAccountingEventOutbox` (properti, relasi, navigation-only) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — tidak ada Controller pada task ini |
| Database | 2 tabel baru siap diterapkan, aditif, nol tabel existing tersentuh. **Belum dieksekusi** — otorisasi eksekusi migration tetap terpisah, belum diberikan |
| Keamanan/Auth | `NOT APPLICABLE`. Catatan desain: `PayloadJson`/`ComponentsJson` MUST NOT memuat data pasien/`DoctorId` (`FR-FIN-073`) — ditegakkan oleh service penulis (`BE-FIN-011`), bukan oleh skema tabel ini |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet ef migrations add` | **Sengaja tidak dijalankan** | `NOT RUN` | Otorisasi eksplisit pemilik repository — migration ditulis tangan |
| `dotnet build` | **Tidak dijalankan oleh saya** | `NOT RUN` | Atas instruksi pengguna sejak `BE-FIN-002` |
| `dotnet ef database update` / eksekusi migration | **Tidak dijalankan** | `NOT RUN` | Otorisasi eksekusi terpisah, belum diminta maupun diberikan |
| `powershell.exe -NoProfile -File tooling/qbe/Invoke-QbeConformanceCheck.ps1` (setelah entity+configuration, sebelum migration) | `Files evaluated: 44`, `VIOLATION: 0`, `Final result: PASS` | `PASS` | Kutipan bagian bawah |
| `powershell.exe -NoProfile -File tooling/qbe/Invoke-QbeConformanceCheck.ps1` (working tree penuh, termasuk migration+snapshot) | `Files evaluated: 46`, `VIOLATION: 0`, `Final result: PASS` | `PASS` | Kutipan bagian bawah |
| Review manual: kolom/index/constraint migration dicocokkan terhadap `FinAccountingEventOutboxConfiguration.cs`/`FinAccountingEventAttemptConfiguration.cs` | Cocok persis | `PASS` | Perbandingan manual pada sesi ini |
| Review manual: urutan `CreateTable` (induk sebelum anak) dan `Down()` (anak sebelum induk) | `FinAccountingEventOutbox` dibuat pertama; `FinAccountingEventAttempt` sesudahnya; `Down()` menghapus Attempt dulu | `PASS` | `20260921000003_AddFinanceAccountingOutbox.cs` |
| Review manual: `EventTypeCode` tidak dibatasi check constraint (bagian 1.1) | Dikonfirmasi — hanya `DeliveryStatus` dan `CurrencyCode` yang punya `CHECK` pada `ToTable`/migration | `PASS` | Bagian 1.1; berkas configuration dan migration |
| Grep `FinSubledgerPeriodBalance` di `Areas/`/`Migrations/` | Nol hasil — dikonfirmasi tidak ikut terbuat (bagian 1.2) | `PASS` | Bagian 1.2 |

`AUTOMATED TEST: NOT APPLICABLE` — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).

Uji manual: `NOT APPLICABLE` — migration belum dijalankan, tidak ada database untuk diuji.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria (persis roadmap) | Status | Bukti |
| --- | --- | --- |
| Fakta dan kejadian tersimpan atau batal bersama; satu fakta satu kejadian | **Skema siap** (dua lapis anti-dobel, `FK Restrict`) — penegakan transaksional sesungguhnya adalah tanggung jawab `FinanceAccountingOutboxService` (`BE-FIN-011`), belum ada pada task ini | Bagian 2.1, 1.3 |
| DoD: data pasien tidak ikut ke muatan kejadian | **Skema tidak memiliki kolom identitas pasien** — `PayloadJson`/`ComponentsJson` bertipe bebas, larangan ditegakkan di level service penulis, dicatat sebagai catatan desain pada model (`FinAccountingEventOutbox.cs`) | Bagian 3.3 |
| Migration dijalankan | **Belum** — otorisasi eksekusi terpisah belum diberikan | Bagian 0, 5 |
| Cakupan `FinAccountingEventOutbox`, `FinAccountingEventAttempt` + migration | **Terpenuhi penuh** | Bagian 2 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| **Pembaruan 23 September 2026** | Pengguna mengonfirmasi `dotnet build` PASS dan migration sudah dieksekusi ke database. Status task dinaikkan menjadi ✅ SELESAI |
| Peringatan | Katalog 17 `EventTypeCode` (`FIN-DEC-002`) belum diratifikasi Accounting — nilai yang dikirim `FinanceAccountingOutboxService` (`BE-FIN-011`) nanti bisa berubah tanpa migration schema baru (sesuai desain bagian 1.1), tetapi perlu dipantau saat ratifikasi selesai |
| Masalah yang diketahui | Tidak ada yang baru di luar yang sudah dicatat pada `BE-FIN-005`..`009` |
| Risiko tersisa | **Sedang** — migration belum diverifikasi `dotnet ef`/`dotnet build` sungguhan (menunggu pengguna), sama seperti seluruh migration tangan sebelumnya sebelum diverifikasi |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` pada task ini |
| Status Git | 6 berkas baru (2 model, 2 configuration, 2 migration `.cs`/`.Designer.cs` — 4 file), `ApplicationDbContextModelSnapshot.cs` bertambah, `ApplicationDbContext.cs` disunting — di atas berkas `BE-FIN-002`..`009` yang sudah ada |
| Langkah berikutnya | (1) `dotnet build` + `dotnet ef migrations list --configuration Release` oleh pengguna. (2) Otorisasi eksekusi migration terpisah. (3) `BE-FIN-011` (`FinanceAccountingOutboxService`) dapat mulai — service **wajib** ikut transaksi pemanggil (`FIN-DES-017`), tidak membuka transaksi sendiri |
