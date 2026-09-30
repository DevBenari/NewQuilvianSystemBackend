# BE-FIN-031 — Migration skema Purchasing/AP (sebelas tabel + satu kolom pada tabel yang sudah berjalan)

- TASK ID: BE-FIN-031
- TASK TYPE: LEGACY MIGRATION (skema baru untuk entity `BE-FIN-029`/`030` yang sudah ada sebagai source, belum ada sebagai database)
- COMPLEXITY: HIGH (bukan dari logika bisnis — dari kewajiban menjaga `ApplicationDbContextModelSnapshot.cs`, satu berkas 121.000+ baris, tetap konsisten dengan sebelas entity baru)
- CLASSIFICATION SCORE: NOT SCORED
- MODEL: Claude Sonnet 5
- TASK MODE: BACKEND
- WRITE TARGET: `NewQuilvianSystemBackend` — `Migrations/**` (dua migration baru + `ApplicationDbContextModelSnapshot.cs`)

- BACKEND GOVERNANCE PREFLIGHT:
  - Area/Module/Submodule: `Corporate / Finance` / `FinanceManagement` / `Purchasing` (`ACTIVE` sejak `BE-FIN-027`) untuk sebelas tabel baru; `Payable` (`ACTIVE`, sudah lama) untuk kolom `FinSupplierPayable.SourcePurchasingInvoiceId`
  - Prefix: `Fin` — dipakai apa adanya, nol prefix baru
  - Keberlakuan: `LEGACY MIGRATION` — `QBE-NAM-003`, `QBE-DB-001`, `QBE-DB-002` berlaku. Model dan configuration sumbernya (`BE-FIN-029` ✅ source, `BE-FIN-030` ✅ source) sudah ada; task ini murni menerbitkan wewenang database untuk model yang sudah ditulis, bukan menulis model baru
  - Status registry: terpenuhi, tidak `BLOCKED`
  - QBE ID yang berlaku: `QBE-NAM-003`, `QBE-DB-001`, `QBE-DB-002`
  - **Empat wewenang terpisah** (`AGENTS.md`): implementasi source (selesai `BE-FIN-029`/`030`), **pembuatan migration** (task ini — disetujui eksplisit 26 September 2026 lewat `AskUserQuestion`, "Ya, buat berkas migration-nya"), **eksekusi migration** (belum, tidak diminta), deployment (tidak relevan). Rename/perubahan source **tidak pernah** memberi wewenang pekerjaan database — otorisasi task ini diminta dan didapat terpisah dari `BE-FIN-029`/`030`/`028`

- FILES INSPECTED:
  - `Areas/Corporate/FinanceManagement/Purchasing/Models/*.cs` (sebelas file, `BE-FIN-029`/`030`) — dibaca ulang penuh untuk memastikan setiap kolom, tipe, `MaxLength`, default, dan navigasi persis sama dengan yang dipakai migration ini
  - `Repositories/Configurations/Corporate/FinanceManagement/Purchasing/*.cs` (sebelas file) — dibaca ulang penuh untuk `HasCheckConstraint`, `HasIndex` (termasuk `HasFilter` parsial), `OnDelete`, dan urutan FK
  - `Areas/Corporate/FinanceManagement/Payable/Models/FinSupplierPayable.cs` dan `FinSupplierPayableConfiguration.cs` — konfirmasi bentuk kolom `SourcePurchasingInvoiceId` (`BE-FIN-030`)
  - `Models/IdentityModel.cs` — sepuluh kolom warisan (`CreateDateTime`, `CreateBy`, `UpdateDateTime`, `UpdateBy`, `DeleteDateTime`, `DeleteBy`, `CancelDateTime`, `CancelBy`, `IsCancel`, `IsDelete`) untuk memastikan urutan dan tipe kolom migration cocok pola existing
  - `Migrations/20260921000004_AddFinanceCashManagement.cs` (dan `.Designer.cs`) — preseden `CreateTable`/`CreateIndex`/`ForeignKey` inline/`Down()` urutan terbalik
  - `Migrations/20260922130000_AddFinanceMedicalServicePayable.cs` (dan `.Designer.cs`) — preseden `AddForeignKey`/`DropForeignKey` pasca-tabel untuk kolom pada tabel yang sudah berjalan
  - `Migrations/20260519184429_changeEmpAttendanceAddWorkSchedule.cs` — preseden `AddColumn<Guid>` nullable
  - `Migrations/MigrationMetadata.g.cs` — **temuan penting**, lihat bagian IMPLEMENTATION
  - `Migrations/ApplicationDbContextModelSnapshot.cs` bagian region properti (`FinSupplierPayable` baris ~5756), region relasi (~96067), region navigasi (~119785) — dipakai sebagai templat persis untuk sebelas entity baru
  - Perbandingan ukuran seluruh `Migrations/*AddFinance*.Designer.cs` — enam berkas besar (110.000–116.000+ baris), tiga berkas kecil (17–29 baris): `AddFinanceCashManagement`, `AddFinancePayment`, `AddFinanceMedicalServicePayable` — dibaca penuh untuk memastikan pola aslinya sebelum ditiru

- FILES CHANGED:
  - **Baru** `Migrations/20260926100000_AddPurchasingApRumpun.cs` — sebelas `CreateTable` (urutan aman terhadap FK: `FinPurchaseOrder` → `FinPurchaseOrderItem` → `FinGoodsReceipt` → `FinGoodsReceiptItem` → `FinInvoiceExchange` → `FinPurchasingInvoice` → `FinPurchasingInvoiceItem` → `FinSupplierReturn` → `FinSupplierReturnItem` → `FinSupplierReturnDeposit` → `FinSupplierReturnDepositUsage`), lalu seluruh `CreateIndex`; `Down()` urutan terbalik
  - **Baru** `Migrations/20260926100000_AddPurchasingApRumpun.Designer.cs` — **ringkas** (pola `AddFinancePayment`, lihat IMPLEMENTATION), tanpa `BuildTargetModel` bermuatan entity
  - **Baru** `Migrations/20260926110000_AddSourcePurchasingInvoiceIdToSupplierPayable.cs` — `AddColumn<Guid>` nullable, `CreateIndex`, `AddForeignKey` (`SetNull`) pada `FinSupplierPayable` yang sudah berjalan; `Down()` urutan terbalik (`DropForeignKey` → `DropIndex` → `DropColumn`)
  - **Baru** `Migrations/20260926110000_AddSourcePurchasingInvoiceIdToSupplierPayable.Designer.cs` — ringkas, pola sama
  - `Migrations/ApplicationDbContextModelSnapshot.cs` — **diperbarui manual** (bukan hasil `dotnet ef`): sebelas blok entity baru disisipkan di tiga region (properti/kunci/index/tabel; relasi; navigasi), plus entity `FinSupplierPayable` yang sudah ada disisipi properti `SourcePurchasingInvoiceId`, satu `HasIndex` baru, dan satu `HasOne`/`Navigation` baru. Baris entity lama **tidak ada yang dihapus atau diubah** — seluruh sisipan memakai `sed` dengan titik sisip persis (nomor baris diverifikasi sebelum dan sesudah tiap sisipan) dan diverifikasi ulang dengan membaca hasilnya

- IMPLEMENTATION:

  **Otorisasi.** Permintaan task menyebut "kerjakan BE-FIN-031" tanpa menyebut migration secara eksplisit. Karena `AGENTS.md` menetapkan pembuatan migration sebagai wewenang terpisah dari penugasan task, dan roadmap `01-backend-roadmap.md` baris `BE-FIN-031` sendiri sudah menulis "Otorisasi terpisah WAJIB", pekerjaan dihentikan sebelum satu baris migration pun ditulis dan ditanyakan eksplisit lewat `AskUserQuestion`. Pengguna menjawab "Ya, buat berkas migration-nya" — mengotorisasi **pembuatan berkas**, secara eksplisit bukan eksekusinya. Seluruh pekerjaan di bawah ini adalah pembuatan berkas; **tidak ada perintah `dotnet ef`, `dotnet build`, atau eksekusi database apa pun yang dijalankan.**

  **Temuan penting — pola `Migration.Designer.cs` ringkas sudah ada di repository ini.** Sebelum menulis migration, dilakukan pemeriksaan ukuran seluruh `Migrations/*AddFinance*.Designer.cs`. Enam di antaranya (`AddFinanceMasterData`, `AddFinanceBillingIntake`, `AddFinanceReceivableAndCollection`, `AddFinanceAccountingOutbox`, `AddFinanceCollection`, `AddFinanceSupplierPayable`) berukuran 110.000–116.000+ baris — salinan penuh seluruh model saat itu, sesuai keluaran `dotnet ef migrations add` standar. Tapi **tiga migration Finance yang lebih baru** (`AddFinanceCashManagement`, `AddFinancePayment`, `AddFinanceMedicalServicePayable`) memiliki `Designer.cs` hanya 17–29 baris — **tanpa** `BuildTargetModel` bermuatan entity, hanya atribut `[DbContext]`/`[Migration]` dan (pada dua yang terakhir) anotasi `ProductVersion` kosong.

  Penjelasannya ditemukan di `Migrations/MigrationMetadata.g.cs`: repository ini punya tooling (`tooling\migrations\Update-MigrationHistory.ps1`) yang **sengaja** mengarsipkan `BuildTargetModel` migration lama ke `Migrations\History\` dan menyisakan kerangka atribut kosong pada `Designer.cs`-nya, karena `BuildTargetModel` "hanya dibutuhkan oleh migration bermuatan data serta oleh 'migrations remove' pada migration terbaru" — sumber kebenaran model untuk `dotnet ef migrations add` berikutnya dan pemeriksaan "pending model changes" tetap **satu-satunya** `ApplicationDbContextModelSnapshot.cs`, bukan `Designer.cs` per migration. Berkas itu sendiri berkomentar "Jangan disunting manual" dan default `dotnet build` **tidak** mengompilasi ulang seluruh `Designer.cs` (butuh flag eksplisit `-p:FullMigrationMetadata=true`).

  Dengan bukti ini, kedua migration baru task ini ditulis dengan `Designer.cs` **ringkas** (pola persis `AddFinancePayment`) alih-alih mereproduksi tangan seluruh 121.000+ baris `BuildTargetModel` — meniru presedens yang sudah diterima repository ini, bukan jalan pintas baru. Konsekuensinya: `ApplicationDbContextModelSnapshot.cs` (bukan `Designer.cs` migration ini) **wajib** benar dan lengkap, karena itulah yang dipakai `dotnet ef` di masa depan dan pemeriksaan model saat runtime. Berkas itu diperbarui manual pada task ini — lihat FILES CHANGED.

  **Migration 1 — `AddPurchasingApRumpun`.** Sebelas `CreateTable` untuk seluruh entity `BE-FIN-029` (tujuh tabel Purchase Order/Tanda Terima Barang/Tukar Faktur/Purchasing Invoice) dan `BE-FIN-030` (empat tabel Retur Pembelian/Deposit Retur, bentuk revisi 5 — `FinSupplierReturnDepositUsage.PaymentId` menunjuk `FinPayment`, **bukan** `PurchasingInvoiceId`). Urutan tabel dipilih agar setiap `table.ForeignKey(...)` inline hanya merujuk tabel yang sudah dibuat baris sebelumnya pada migration yang sama (pola persis `AddFinanceReceivableAndCollection`, FK inline di tabel anak). Seluruh `CreateIndex` (termasuk dua unique parsial `WHERE "IsDelete" = false` dan satu unique parsial `WHERE "Status" <> 'RELEASED' AND "IsDelete" = false` untuk `IX_FinSupplierReturnDepositUsage_ActivePerPayment`) ditulis setelah seluruh `CreateTable`, mengikuti pola migration existing.

  **Migration 2 — `AddSourcePurchasingInvoiceIdToSupplierPayable`.** Sengaja dipisah dari migration 1 agar migration pembuatan tabel baru tidak menyentuh `FinSupplierPayable` yang sudah berjalan — `AddColumn` nullable, satu index, satu `AddForeignKey` (`SetNull`) yang bergantung pada `FinPurchasingInvoice` sudah ada (karenanya file ini bertimestamp setelah migration 1).

  **Perbaikan panjang identifier.** Tiga nama constraint FK hasil konvensi penuh (`FK_<Anak>_<Induk>_<Kolom>`) melebihi batas 63 karakter PostgreSQL (`NAMEDATALEN`): `FK_FinPurchasingInvoiceItem_FinPurchasingInvoice_PurchasingInvoiceId` (68), `FK_FinSupplierPayable_FinPurchasingInvoice_SourcePurchasingInvoiceId` (68), `FK_FinSupplierReturnDepositUsage_FinSupplierReturnDeposit_SupplierReturnDepositId` (81). Ketiganya dipendekkan ke pola `FK_<Anak>_<Kolom>` (tetap unik, tanpa nama tabel induk pada nama constraint): `FK_FinPurchasingInvoiceItem_PurchasingInvoiceId`, `FK_FinSupplierPayable_SourcePurchasingInvoiceId`, `FK_FinSupplierReturnDepositUsage_SupplierReturnDepositId`. Nama constraint ini **tidak** muncul di `ApplicationDbContextModelSnapshot.cs` (tidak ada `.HasConstraintName()` eksplisit pada configuration mana pun) sehingga perubahan ini murni pada berkas migration, nol dampak pada perbandingan model EF Core. Sesudah perbaikan, identifier terpanjang tepat 63 karakter (`FK_FinGoodsReceiptItem_FinPurchaseOrderItem_PurchaseOrderItemId`) — pas di batas, bukan melebihi.

- BLUEPRINT STATUS/EVIDENCE: NOT APPLICABLE
- API CONTRACT IMPACT: Nol. Tidak ada controller/endpoint pada task ini.
- DATABASE IMPACT: **Berkas migration dan snapshot selesai ditulis; database belum tersentuh.** Sebelas tabel baru + satu kolom pada `FinSupplierPayable`, sesuai rencana `02-backend-architecture.md` D.7 dan `data-dictionary.md` D.4. `Up()` aditif murni (nol `ALTER`/`DROP` pada kolom/tabel lain), `Down()` menghapus bersih dalam urutan FK terbalik. **Belum diterapkan ke database mana pun** — eksekusi (`dotnet ef database update` atau setara) tidak diotorisasi pada task ini.
- SECURITY IMPACT: Nol. Tidak ada controller/endpoint/permission pada task ini.
- VISUAL REFERENCE: NOT APPLICABLE
- VALIDATION:

  | Command/check | Hasil | Klasifikasi | Bukti/catatan |
  |---|---|---|---|
  | `dotnet build` | **SENGAJA TIDAK DIJALANKAN** | VALIDASI TERTUNDA | Instruksi eksplisit pengguna — pengguna menjalankan sendiri. **Bukan klaim PASS**, dan task ini bertumpuk di atas `BE-FIN-028`/`029`/`030` yang juga belum terverifikasi compiler |
  | `dotnet ef database update` / eksekusi migration | **TIDAK DIJALANKAN** | TIDAK DIOTORISASI | Wewenang eksekusi migration terpisah dari wewenang pembuatan berkas (`AGENTS.md`); hanya pembuatan berkas yang disetujui pada task ini |
  | Kesesuaian kolom Migration.cs vs Model+Configuration, per entity (sebelas tabel) | Nama kolom, tipe SQL, `maxLength`, `precision`, `nullable`, `defaultValue` cocok satu-satu | PASS (review) | Baca ulang sebelas model + sebelas configuration sebelum menulis tiap `CreateTable` |
  | Kesesuaian `CheckConstraint`/`ForeignKey`/`CreateIndex` (termasuk filter index parsial) vs Configuration | Teks SQL check constraint dan filter index disalin persis dari `HasCheckConstraint`/`HasFilter` pada configuration | PASS (review) | Perbandingan string manual, bukan disalin ingatan |
  | Urutan `CreateTable` aman terhadap FK inline | Setiap FK inline hanya menunjuk tabel yang sudah dibuat baris sebelumnya pada migration yang sama | PASS (review) | Runut manual grafik dependency sebelum menulis |
  | Urutan `Down()` terbalik dari urutan `Up()` | Sebelas `DropTable` persis kebalikan sebelas `CreateTable` | PASS (review) | Baca ulang berdampingan |
  | Panjang seluruh identifier `PK_`/`FK_`/`IX_`/`CK_` pada kedua migration ≤ 63 karakter | Terpanjang tepat 63 setelah perbaikan tiga nama FK | PASS (review) | `awk` mengukur panjang setiap string identifier hasil `grep` |
  | `ApplicationDbContextModelSnapshot.cs` — titik sisip diverifikasi sebelum dan sesudah setiap sisipan | Baris di kedua sisi titik sisip (blok `FinSupplierPayableItem` sebelumnya, blok `FinPettyCashBudget` sesudahnya) dibaca ulang dan cocok pada tiga region | PASS (review) | Pembacaan berkas sebelum dan sesudah `sed`, penghitungan jumlah baris yang tersisip dicocokkan dengan jumlah baris berkas sisipan |
  | `ApplicationDbContextModelSnapshot.cs` — sebelas entity baru + satu entity yang diperbarui memakai sintaks region yang sama (alfabetis properti, urutan chained method, `b.Navigation` untuk sisi prinsipal 1:1) | Ditelusuri dari tiga contoh existing (`FinSupplierPayable`, relasi 1:1 `MstWorkforceProfile`/`MstDoctor`, relasi 1:1 `OprCase`) sebelum menulis | PASS (review) | Baca ulang tiga presedens berbeda untuk memastikan sintaks `HasForeignKey` satu-argumen vs dua-argumen (1:1) benar |

  **Klasifikasi keseluruhan validasi task ini: REVIEW ONLY, bukan BUILD-VERIFIED, bukan MIGRATION-APPLIED.**

- WARNINGS: Task ini bertumpuk di atas `BE-FIN-028`/`029`/`030` yang **juga** belum ter-`build`. Bila salah satunya punya kesalahan kompilasi, migration ini otomatis ikut cacat karena bergantung penuh pada bentuk model tersebut. Disarankan `dotnet build` dijalankan mencakup `BE-FIN-028`, `029`, `030` **dan** review berkas migration ini sekaligus, sebelum mempertimbangkan eksekusi migration.
- KNOWN ISSUES:
  1. `dotnet build` belum dijalankan — validasi tertunda milik pengguna, mencakup dua migration baru ini.
  2. Migration **belum dieksekusi** ke database mana pun — sebelas tabel baru dan satu kolom baru tidak dapat dipakai sampai eksekusi diotorisasi dan dijalankan terpisah.
  3. `Designer.cs` kedua migration ini ringkas (tanpa `BuildTargetModel` bermuatan entity), mengikuti presedens repository ini (`AddFinancePayment`, dst). Ini **berarti** bila pengguna nanti menjalankan `dotnet ef migrations remove` pada migration `AddSourcePurchasingInvoiceIdToSupplierPayable` (migration terbaru) sebelum menjalankan `dotnet ef migrations add` berikutnya, EF Core kemungkinan butuh `BuildTargetModel` migration itu — pada titik itu disarankan menjalankan `dotnet ef migrations add` biasa sekali agar tooling meregenerasi `Designer.cs` penuh secara otomatis, atau menjalankan `tooling\migrations\Update-MigrationHistory.ps1` sesuai maksud aslinya.
  4. `ApplicationDbContextModelSnapshot.cs` diperbarui **manual**, bukan oleh `dotnet ef`. Risiko residual: kemungkinan ada anotasi/urutan chained-method langka yang tidak tercakup tiga contoh presedens yang diperiksa (region properti, relasi, navigasi) — review lapis kedua lewat `dotnet build` (yang memicu pemeriksaan "pending model changes" bila context di-resolve) adalah pembuktian akhir yang belum dilakukan pada task ini.
  5. Nomor bisnis (`PONumber`, `GRNumber`, `ExchangeNumber`, `InvoiceNumber`, `ReturnNumber`) tetap belum punya service alokasinya (`QBE-CODE-001..006`) — tidak berubah oleh task ini, sudah tercatat sejak `BE-FIN-029`/`030`.

- STALE EVIDENCE / BLOCKED PHASES: NOT APPLICABLE
- MANUAL TEST: NOT FEASIBLE — memerlukan `dotnet build` sukses dan eksekusi migration lebih dulu, keduanya belum diotorisasi/dijalankan.
- INCIDENTAL CHANGES: NONE
- INTERRUPTIONS: NONE
- GIT STATUS (akhir pekerjaan task ini):

  ```text
  M  Migrations/ApplicationDbContextModelSnapshot.cs
  ?? Migrations/20260926100000_AddPurchasingApRumpun.cs
  ?? Migrations/20260926100000_AddPurchasingApRumpun.Designer.cs
  ?? Migrations/20260926110000_AddSourcePurchasingInvoiceIdToSupplierPayable.cs
  ?? Migrations/20260926110000_AddSourcePurchasingInvoiceIdToSupplierPayable.Designer.cs
  ```

  (Perubahan lain pada working tree — berkas `BE-FIN-027`/`028`/`029`/`030` dan seluruh dokumen blueprint AMENDMENT REVISI 4/5 — sudah ada sebelum task ini dimulai; bukan hasil task ini.)

- NEXT RECOMMENDED STEP: **Pengguna menjalankan `dotnet build`** mencakup `BE-FIN-028`, `029`, `030`, dan kedua migration `BE-FIN-031` sekaligus. Sesudah terkonfirmasi sukses, dan bila eksekusi migration diotorisasi terpisah, `dotnet ef database update` (atau setara) di lingkungan pengembangan adalah langkah database pertama yang membuat sebelas tabel ini benar-benar dapat dipakai. `BE-FIN-032` (Purchase Order/Tanda Terima Barang) dan `BE-FIN-033` (Tukar Faktur) adalah task berikutnya yang bergantung pada `BE-FIN-031`; `BE-FIN-038` dan `BE-FIN-041` tetap tidak berprasyarat bila ingin dikerjakan paralel.
