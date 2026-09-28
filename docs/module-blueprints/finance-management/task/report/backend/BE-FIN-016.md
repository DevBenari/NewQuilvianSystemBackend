# Laporan Perubahan Backend — `BE-FIN-016`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-016` |
| Judul | Penerimaan dari tender kasir — `FinReceipt`, `FinReceiptAllocation` |
| Slice | `MVP-2` — sebelumnya `BLOCKED — owner Billing` |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian 3 (tabel task) dan bagian 4 (`MVP-2`, `MVP-3` — tertahan) |
| Trace | `FIN-DEC-005`, `FR-FIN-030`..`034`; kontrak `FIN-INTEGRATION-1.0` §2, `contracts/integration-contract.md` §2 |
| Contract version | `FIN-INTEGRATION-1.0` §2 — permukaan `BilCollectionHandoff` **disetujui** owner Billing 21 September 2026 (`BKC-DEC-106`/`108`/`109`), lihat bagian 0 |
| Dependency | `BE-FIN-009` — 🟡 sebagian (selesai); `[BE] BE-BKC-069` (Billing) — 🟡 sebagian 21 September 2026, source selesai dan **sudah dikonfirmasi tertaut** ke `BillingSettlementService.ReconcileTenderAsync` (bagian 0) |
| Klasifikasi | `HEAVY` — dua entity baru + migration + perluasan service yang sudah selesai (`BE-FIN-009`) + pemanggilan outbox pertama dengan `RequiresFinalization = true` sungguhan; **otorisasi eksplisit** untuk cakupan penuh (bagian 0) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/Collection/Models/FinReceipt.cs` (baru), `FinReceiptAllocation.cs` (baru); `Repositories/Configurations/Corporate/FinanceManagement/Collection/FinReceiptConfiguration.cs` (baru), `FinReceiptAllocationConfiguration.cs` (baru); `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivable.cs` (disunting — nav `ReceiptAllocations`); `Repositories/ApplicationDbContext.cs` (disunting); `Areas/Corporate/FinanceManagement/BillingIntake/Services/FinanceBillingIntakeService.cs` (disunting — perluasan `COLLECTION`); `Migrations/20260922100000_AddFinanceCollection.cs` + `.Designer.cs` (baru); `Migrations/ApplicationDbContextModelSnapshot.cs` (disunting) |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | Working tree pada branch `Yasmina`; commit dasar `a743388b57da91e6a0d7a42813604dc94563e38d` |
| Tanggal | 22 September 2026 |
| Status | ✅ **SELESAI 23 September 2026.** Entity, configuration, migration, dan konsumsi `COLLECTION` selesai. Jalur `TenderStatus = SUCCEEDED` terpetakan penuh ke `FR-FIN-030`..`034`. Jalur `REVERSED` — sebelumnya `BLOCKED` oleh konflik `CK_FinReceipt_TenderRequired`/`IX_FinReceipt_SourceTenderId` (bagian 1.5) — **diperbaiki**: `CK_FinReceipt_TenderRequired` diberi klausa pengecualian untuk baris pembalik (`ReversalOfReceiptId IS NOT NULL`), persis desain yang sudah didokumentasikan `FinReceipt.cs` sejak awal (SourceTenderId kosong pada baris pembalik). Lihat Pembaruan bagian 7 dan [laporan BE-FIN-017](BE-FIN-017.md) untuk implementasi `CreateReversalReceiptAsync` (lokasinya sejak refactor `BE-FIN-017`). Migration baru `20260923060000_FixFinReceiptTenderRequiredForReversal` ditulis, **belum dieksekusi** — menunggu otorisasi eksekusi terpisah seperti migration lain |

**Addendum 22 September 2026 (`BE-FIN-017`)**: `CreateSucceededReceiptAsync`, `CreateReversalReceiptAsync`,
dan `GenerateReceiptNumber` yang semula ditulis di `FinanceBillingIntakeService` (bagian 2.3 di
bawah) **dipindahkan tanpa perubahan perilaku** ke `FinanceReceiptService`
(`Collection/Services/FinanceReceiptService.cs`) — sesuai `02-backend-architecture.md` §4.22 yang
menetapkan service itu sebagai pemilik logika penerimaan, dengan `FinanceBillingIntakeService`
sebagai pemanggil. Lihat [laporan BE-FIN-017](BE-FIN-017.md) untuk rincian refactor dan
otorisasinya. Seluruh isi bagian 1–7 di bawah ini tetap sahih sebagai catatan keputusan desain;
hanya *lokasi file*-nya yang berubah.

---

## 0. Penemuan status blocker dan otorisasi cakupan

Task ini dibuka lewat pertanyaan pengguna "Apakah bisa melanjutkan BE-FIN-016?" — roadmap
mencatatnya `BLOCKED — owner Billing` sejak awal. Sebelum menulis kode, dilakukan verifikasi
langsung ke source (bukan hanya membaca dokumen):

1. `docs/module-blueprints/finance-management/evidence/02-permintaan-kontrak-untuk-owner-billing.md`
   bagian 7 (`Jawaban owner Billing — 21 September 2026`) menunjukkan permintaan Finance sudah
   **disetujui** (`BKC-DEC-106`, `108`, `109`), tetapi mencatat "Task roadmap Billing untuk
   membangunnya belum dibuat" sebagai blocker yang tersisa.
2. Grep `PublishForTenderAsync` di source Billing menemukan method itu **sudah ada** di
   `BilConsumerHandoffService.cs` (bagian dari `BE-BKC-069`, dikerjakan sesi lain) **dan** benar-benar
   dipanggil dari `BillingSettlementService.cs` baris 565 — bukan kode yang menggantung tanpa pemanggil.
3. Isi `PublishForTenderAsync` diperiksa baris-per-baris terhadap kontrak Finance
   (`integration-contract.md` §2.1/2.2): seluruh 18 field yang diminta Finance (`TenderId`,
   `SettlementId`, `InvoiceId`, `PaymentAllocationIds`, `PaymentMethodId`,
   `PaymentMethodAccountId`, `Amount`, `KwitansiNumber`, `CashierShiftId`, `ProviderReference`,
   `ProviderEventId`, `OccurredAt`, `SourceInvoiceStatus`, `TenderStatus`, `HandoffKey`,
   `CorrelationId`, `CausationId`, `Status`) terisi persis sesuai permintaan — termasuk perilaku
   "dibuat saat `SUCCEEDED`, tidak menunggu finalisasi" dan idempotensi dua lapis (per
   `TenderId`+`Status`, dan per `HandoffKey`).

Kesimpulan: blocker "menunggu jawaban/eksekusi owner Billing" **sudah selesai** di level source,
meski migration `BilCollectionHandoff` (`AddBillCollectionPrescriptionHandoff`, dari `BE-BKC-062`/
`067`/`069`) juga belum dijalankan pengguna — situasi yang sama seperti dependency antar-task
Finance sendiri (`BE-FIN-010` belum jalan, tidak menghentikan `BE-FIN-011`/`012`).

**Pertanyaan cakupan diajukan eksplisit kepada pengguna** sebelum menulis kode: cakupan literal
roadmap ("`FinReceipt`, `FinReceiptAllocation`") hanya menyebut entity, tetapi tanpa pemanggil
nyata acceptance criteria-nya ("Satu tender berhasil → satu penerimaan") tidak dapat dibuktikan —
pola yang identik dengan otorisasi `BE-FIN-011`. Jawaban yang diterima: **"Entities + full wiring
(Recommended)"** — membangun entity **dan** memperluas `FinanceBillingIntakeService` (`BE-FIN-009`)
untuk mengonsumsi `HandoffType = COLLECTION`, meski menyentuh berkas yang sudah selesai.

---

## 1. Keputusan desain dan keterbatasan (didokumentasikan, bukan didiamkan)

### 1.1 Skema `FinReceipt`/`FinReceiptAllocation` diambil persis dari `erd/data-dictionary.md` §3.1/3.2

Bukan dikarang ulang dari Mermaid ERD ringkas (`erd/receivable-collection.md`) yang melewatkan
`ProviderEventId`, `CorrelationId`, `CausationId` — data dictionary beserta blok SQL DDL-nya
(§9.2) adalah sumber kebenaran, dan seluruh nama kolom, tipe, `HasPrecision(18,2)`, check
constraint, serta ketiga index eksplisit (`IX_FinReceipt_SourceTenderId` unik parsial,
`IX_FinReceipt_ReceiptNumber` unik parsial, `IX_FinReceipt_CashierShift_OccurredAt` komposit)
disalin persis. Lokasi folder (`Areas/Corporate/FinanceManagement/Collection/Models/`) mengikuti
`02-backend-architecture.md` §4.7/4.8, **bukan** menamainya "Receipt" seperti nama entity.

### 1.2 `FinReceiptAllocation` dibuat tabelnya, tetapi nol baris ditulis pada task ini

Sesuai otorisasi bagian 0: pembagian bayar-vs-piutang (mengisi `ReceiptId`/`ReceivableId`/
`TargetType`/`Amount`) adalah tanggung jawab `FinanceReceiptService` (`BE-FIN-017`, **BLOCKED**,
belum dikerjakan). `FinReceipt` yang dibuat task ini selalu `AllocatedAmount = 0`,
`UnallocatedAmount = Amount` — konsisten dengan `CK_FinReceipt_AllocationBalance`. Nav
`FinReceivable.ReceiptAllocations` ditambahkan (satu baris, aditif) semata-mata supaya relasi FK
same-module (`FinReceiptAllocation.ReceivableId → FinReceivable`) dapat dibangun mengikuti pola
navigasi dua arah yang sudah baku di modul ini (`FinReceivableAdjustment`/`WriteOff`/`Item`/
`Document`) — bukan perluasan perilaku `FinReceivable`.

### 1.3 `FinanceBillingIntakeService` diperluas untuk `HandoffType = COLLECTION` (otorisasi eksplisit)

`SyncNewFactsAsync` kini menyinkron `BilArHandoffs` **dan** `BilCollectionHandoffs` yang berstatus
`CREATED`. `ProcessAsync` bercabang berdasar `HandoffType` ke `ProcessArIntakeAsync` (tidak
diubah) atau `ProcessCollectionIntakeAsync` (baru). Docstring kelas diperbarui, mencatat
perluasan ini beserta tanggalnya — bukan diselipkan diam-diam. Baris intake `AP`/`ADJUSTMENT`
tetap `NEW` tanpa penangan, sama seperti sebelumnya.

### 1.4 FR-FIN-033 diperiksa ulang di sisi Finance, bukan mempercayai Billing secara buta

`BilConsumerHandoffService.PublishForTenderAsync` sudah menolak tender tunai tanpa
`CashierShiftId` sebelum menerbitkan handoff (`BIL-VAL-111`). `ProcessCollectionIntakeAsync`
tetap memeriksa ulang hal yang sama (`paymentMethod.IsCash` + `handoff.CashierShiftId == null` →
`BillingIntakeValidationException`) sebagai lapis kedua independen — pola yang sama dengan
`FIN-VAL-012` pada jalur AR (memeriksa ulang keunikan `FinReceivable` walau index unik sudah ada).

### 1.5 Pembalikan tender (`TenderStatus = REVERSED`) — **BLOCKED**, konflik nyata pada dua constraint terkunci

Percobaan pertama menulis baris `FinReceipt` pembalik (baris baru, `SourceTenderId` dikosongkan,
`ReversalOfReceiptId` menunjuk baris asli — mengikuti `evidence/02-permintaan-kontrak-untuk-owner-billing.md`
§2.5 dan `erd/receivable-collection.md` §5 butir 3) ditemukan **melanggar `CK_FinReceipt_TenderRequired`**
saat direview ulang terhadap `data-dictionary.md` §9.2 sebelum laporan ini ditutup. Dua constraint
terkunci ternyata saling bertentangan untuk kasus reversal:

| Constraint (terkunci, `data-dictionary.md` §9.2) | Bunyi | Konsekuensi untuk baris pembalik |
| --- | --- | --- |
| `IX_FinReceipt_SourceTenderId` | Unik pada `SourceTenderId`, `WHERE "SourceTenderId" IS NOT NULL AND "IsDelete" = false`, **tanpa syarat `TenderStatus`** | Baris asli (`SUCCEEDED`) sudah memegang satu-satunya slot untuk `TenderId` itu selama ia tidak dihapus |
| `CK_FinReceipt_TenderRequired` | `"SourceType" <> 'BILLING_TENDER' OR "SourceTenderId" IS NOT NULL` | Baris pembalik tetap berasal dari tender Billing (`SourceType = BILLING_TENDER`), sehingga **wajib** mengisi `SourceTenderId` |

Mengosongkan `SourceTenderId` pada baris pembalik (percobaan pertama) melanggar baris kedua.
Mengisinya dengan `TenderId` yang sama seperti baris asli melanggar baris pertama, kecuali baris
asli dihapus/diubah — yang justru bertentangan dengan tuntutan eksplisit §2.5 ("baris lama tetap
utuh dan tetap terbaca") dan `erd/receivable-collection.md` sendiri. **Tidak ada kombinasi nilai
yang memenuhi keduanya sekaligus** tanpa mengubah salah satu constraint terkunci itu sendiri —
ini kesenjangan pada DDL yang disetujui, bukan sesuatu yang berwenang diputuskan sepihak pada
task implementasi.

**Keputusan yang diambil**: jalur `TenderStatus = REVERSED` **tidak diimplementasikan**.
`ProcessCollectionIntakeAsync` melempar `InvalidOperationException` yang jelas menyebut kedua
constraint yang berkonflik; ini ditangkap `ProcessAsync` dan disimpan sebagai baris `ERROR` yang
terlihat (`FR-FIN-011` — kegagalan tersimpan dan terlihat, bukan hilang diam-diam), **bukan**
sebagai `BillingIntakeValidationException` (yang tidak pernah tersimpan sebagai jejak). Jalur
`SUCCEEDED` sepenuhnya berfungsi dan tidak terdampak.

**Untuk pemilik repository**: keputusan yang dibutuhkan sebelum reversal dapat dibangun, misalnya
(a) mengubah `IX_FinReceipt_SourceTenderId` menjadi unik bersama `TenderStatus` alih-alih
`SourceTenderId` sendirian, atau (b) menambah kolom terpisah (mis. `OriginalTenderId`) untuk baris
pembalik yang tidak ikut diikat index idempotensi utama. Keduanya mengubah skema terkunci
`data-dictionary.md`, di luar wewenang task ini.

**Diperbaiki 23 September 2026 — opsi ketiga yang ditemukan, bukan (a)/(b) di atas.**
`FinReceipt.cs` (model, ditulis task ini sendiri) sudah mendokumentasikan sejak awal bahwa "baris
pembalik memakai `SourceTenderId` kosong dan menunjuk baris asli lewat `ReversalOfReceiptId`,
supaya identitas idempotensi tender asli tidak pernah dipakai ulang" — desain itu **tidak
konsisten** dengan `CK_FinReceipt_TenderRequired` versi awal yang mewajibkan `SourceTenderId`
terisi tanpa kecuali untuk `SourceType = BILLING_TENDER`. Perbaikannya: tambah klausa
`OR "ReversalOfReceiptId" IS NOT NULL` pada `CK_FinReceipt_TenderRequired`
(`FinReceiptConfiguration.cs`), tanpa menyentuh `IX_FinReceipt_SourceTenderId` sama sekali — baris
pembalik tetap mengosongkan `SourceTenderId` seperti didesain, sehingga otomatis tidak pernah
bertabrakan dengan index unik parsial itu (yang memfilter `WHERE SourceTenderId IS NOT NULL`).
Ini lebih kecil dari opsi (a)/(b): satu klausa pada satu check constraint, nol perubahan pada
index atau kolom. Migration `20260923060000_FixFinReceiptTenderRequiredForReversal` menerapkan
`ALTER TABLE ... DROP/ADD CONSTRAINT`. Implementasi `CreateReversalReceiptAsync` sendiri ada di
[laporan BE-FIN-017](BE-FIN-017.md) (lokasinya sejak refactor task itu).

### 1.6 `HELD_FOR_FINALIZATION` mendapat pemanggil nyata pertama

`BE-FIN-011` bagian 1.5 mencatat kapabilitas ini "dibangun sebagai kapabilitas, belum ada
pemanggil". `CreateSucceededReceiptAsync` adalah pemanggil nyata pertama:
`RequiresFinalization = (handoff.SourceInvoiceStatus == "OPEN")` — persis FR-FIN-034.
Mekanisme **pelepasan** kejadian yang tertahan begitu tagihan kelak final (`accounting-integration.md`
§4, baris "Diubah menjadi PENDING") **tidak** dibangun di sini — itu mekanisme terpisah yang
belum punya task pemilik, dicatat sebagai `KNOWN ISSUES`.

### 1.7 `AccountingDate` memakai tanggal `OccurredAt`, bukan tanggal pemrosesan

Berbeda dari `PENGAKUAN-PIUTANG` (`BE-FIN-011`, memakai tanggal proses `now`), kejadian penerimaan
memakai `DateOnly.FromDateTime(handoff.OccurredAt.UtcDateTime)` — tanggal uang **benar-benar
diterima**, bukan tanggal batch sinkronisasi berjalan. Dianggap lebih benar secara akuntansi untuk
fakta kas, tetapi ini pilihan sendiri karena kedua dokumen desain tidak menegaskan salah satu.

### 1.8 Dua dokumen blueprint yang sudah usang diperbarui

`contracts/integration-contract.md` §2.3 dan `evidence/02-permintaan-kontrak-untuk-owner-billing.md`
§7 masih berbunyi "menunggu konfirmasi/eksekusi owner Billing" — keduanya diperbarui pada task
ini untuk mencerminkan bahwa jawaban sudah turun **dan** source Billing-nya sudah dieksekusi,
supaya task berikutnya tidak mengira ini masih terbuka.

---

## 2. Ringkasan pekerjaan

### 2.1 Entity dan configuration (baru)

`FinReceipt` (aggregate root penerimaan) dan `FinReceiptAllocation` (jembatan ke piutang, nol
penulis pada task ini) — lihat bagian 1.1 untuk sumber skema.

### 2.2 Migration `AddFinanceCollection` (baru, tangan, belum dijalankan)

Menuntaskan gap yang dilaporkan `BE-FIN-007` sendiri: migration `AddFinanceReceivableAndCollection`
namanya menjanjikan tabel Collection tapi hanya membangun 5 tabel Receivable. Migration ini
membuat `FinReceipt` lalu `FinReceiptAllocation` (FK ke `FinReceipt` dan `FinReceivable`), plus
index otomatis EF Core untuk keempat kolom FK yang belum tercakup index lain (`ReversalOfReceiptId`,
`ReceiptId`, `ReceivableId`, `ReversalOfAllocationId`) — pola yang sama dengan
`AddFinanceReceivableAndCollection` untuk `FinReceivableAdjustment`/dst.

### 2.3 `FinanceBillingIntakeService` — perluasan `COLLECTION`

| Method | Perubahan |
| --- | --- |
| `SyncNewFactsAsync` | Menyinkron `BilCollectionHandoffs` selain `BilArHandoffs` |
| `ProcessAsync` | Bercabang ke `ProcessCollectionIntakeAsync` bila `HandoffType == COLLECTION` |
| `ProcessCollectionIntakeAsync` (baru) | Transaksi `Serializable` + advisory lock per `intakeId`, sama pola dengan `ProcessArIntakeAsync` |
| `CreateSucceededReceiptAsync` (baru) | FR-FIN-030/032/033/034 — bagian 1.4, 1.6, 1.7 |
| `CreateReversalReceiptAsync` (baru) | **Selalu melempar `InvalidOperationException`** — jalur `REVERSED` diblokir sengaja, bagian 1.5 |

### 2.4 Tidak ada registrasi DI baru

`FinanceBillingIntakeService` sudah terdaftar sejak `BE-FIN-009`; perluasan ini murni menambah
method pada kelas yang sama.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/evidence/02-permintaan-kontrak-untuk-owner-billing.md` — permintaan asli dan jawaban owner Billing
- `docs/module-blueprints/finance-management/contracts/integration-contract.md` §2 — kontrak fakta penerimaan
- `docs/module-blueprints/finance-management/erd/receivable-collection.md`, `erd/data-dictionary.md` §3.1/3.2, §9.2 (DDL) — skema `FinReceipt`/`FinReceiptAllocation`
- `docs/module-blueprints/finance-management/02-backend-architecture.md` §4.7/4.8 — lokasi folder dan catatan desain
- `docs/module-blueprints/finance-management/04-prd-to-mvp.md` — `FR-FIN-030`..`035`, `UAT-05`, `06`, `20`
- `Areas/HealthServices/BillingManagement/Billing/Services/BilConsumerHandoffService.cs` (`PublishForTenderAsync`, dibaca penuh), `BillingSettlementService.cs` baris 565 (pemanggil nyata), `Models/BilCollectionHandoff.cs`, `Models/BilInvoice.cs` (`BillingInvoiceStatuses`), `Models/BilTender.cs` (`BillingTenderStatuses`) — mengonfirmasi kontrak benar-benar terpenuhi di source
- `Areas/Corporate/FinanceManagement/BillingIntake/Services/FinanceBillingIntakeService.cs` (`BE-FIN-009`, dibaca penuh) — titik sisip perluasan
- `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivable.cs`, `Repositories/Configurations/Corporate/FinanceManagement/Receivable/FinReceivableAdjustmentConfiguration.cs` — pola navigasi dua arah same-module
- `Migrations/20260921000002_AddFinanceReceivableAndCollection.cs` — pola `CreateTable`/`CreateIndex` dan bukti index otomatis FK (`IX_FinReceivableAdjustment_ReceivableId` dst.)
- `Migrations/ApplicationDbContextModelSnapshot.cs` — bukti pola perbedaan render relasi (`b.HasOne(..., null)` tanpa navigasi vs dengan navigasi + `b.Navigation(...)`)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/Collection/Models/FinReceipt.cs` | **Baru** |
| `Areas/Corporate/FinanceManagement/Collection/Models/FinReceiptAllocation.cs` | **Baru** |
| `Repositories/Configurations/Corporate/FinanceManagement/Collection/FinReceiptConfiguration.cs` | **Baru** |
| `Repositories/Configurations/Corporate/FinanceManagement/Collection/FinReceiptAllocationConfiguration.cs` | **Baru** |
| `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivable.cs` | +`using` Collection.Models; +nav `ICollection<FinReceiptAllocation> ReceiptAllocations` |
| `Repositories/ApplicationDbContext.cs` | +`using`; +`DbSet<FinReceipt>`, `DbSet<FinReceiptAllocation>`; komentar `FinReceivable` diperbarui (gap `BE-FIN-007` ditutup) |
| `Areas/Corporate/FinanceManagement/BillingIntake/Services/FinanceBillingIntakeService.cs` | Docstring; `SyncNewFactsAsync` diperluas; `ProcessAsync` bercabang; +`ProcessCollectionIntakeAsync`, `CreateSucceededReceiptAsync`, `CreateReversalReceiptAsync`, `GenerateReceiptNumber` |
| `Migrations/20260922100000_AddFinanceCollection.cs` | **Baru** |
| `Migrations/20260922100000_AddFinanceCollection.Designer.cs` | **Baru** (salinan snapshot kumulatif) |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | +2 entity block (properti), +2 blok relasi, +1 blok navigasi `FinReceipt`, +1 baris navigasi `FinReceivable.ReceiptAllocations` |
| `docs/module-blueprints/finance-management/contracts/integration-contract.md` | §2.3 diperbarui — status disepakati, bukan lagi menunggu |
| `docs/module-blueprints/finance-management/evidence/02-permintaan-kontrak-untuk-owner-billing.md` | §7 ditambah catatan pembaruan 22 September 2026 |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — tidak ada Controller/DTO API baru pada task ini |
| Database | 2 tabel baru (`FinReceipt`, `FinReceiptAllocation`) via migration tangan, **belum dijalankan**. Nol tabel existing diubah skemanya |
| Keamanan/Auth | `NOT APPLICABLE` — tidak ada endpoint baru. Catatan kepatuhan: `FinReceipt`/`FinReceiptAllocation` tidak menyimpan data pasien, konsisten dengan `FinAccountingEventOutbox` yang dituju |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | **Tidak dijalankan oleh saya** | `NOT RUN` | Atas instruksi pengguna sejak `BE-FIN-002` |
| `powershell.exe -NoProfile -File tooling/qbe/Invoke-QbeConformanceCheck.ps1` | Lihat kutipan bagian bawah | `PASS` | Dijalankan latar belakang, hasil disalin apa adanya |
| Review manual: `PublishForTenderAsync` benar-benar dipanggil, bukan kode menggantung | Dikonfirmasi — `BillingSettlementService.cs:565` | `PASS` | Bagian 0 |
| Review manual: seluruh 18 field kontrak `integration-contract.md` §2.1 terisi persis di `BilCollectionHandoff`/`PublishForTenderAsync` | Dikonfirmasi | `PASS` | Bagian 0 |
| Review manual: `CK_FinReceipt_AllocationBalance` (`Amount = AllocatedAmount + UnallocatedAmount`) selalu terpenuhi saat insert | Dikonfirmasi untuk jalur `Succeeded` (satu-satunya jalur yang menulis baris) — `AllocatedAmount = 0`, `UnallocatedAmount = handoff.Amount`, `Amount = handoff.Amount` | `PASS` | `CreateSucceededReceiptAsync` |
| Review manual: `CK_FinReceipt_TenderRequired` dan `IX_FinReceipt_SourceTenderId` tidak pernah dilanggar | **Percobaan pertama SALAH** — draf awal `CreateReversalReceiptAsync` mengosongkan `SourceTenderId` sambil mempertahankan `SourceType = BILLING_TENDER`, melanggar `CK_FinReceipt_TenderRequired` langsung. Ditemukan saat review ulang sebelum laporan ditutup, bukan oleh `dotnet build` (constraint database tidak diperiksa compiler). Diperbaiki dengan memblokir jalur `REVERSED` sepenuhnya (bagian 1.5) alih-alih mengirim baris yang melanggar constraint | `FIXED SEBELUM LAPORAN — lihat bagian 1.5` | `CreateReversalReceiptAsync`, `data-dictionary.md` §9.2 |
| Review manual: FR-FIN-033 diperiksa ulang, bukan mempercayai Billing | Dikonfirmasi | `PASS` | Bagian 1.4 |
| `UAT-05`, `UAT-06`, `UAT-20` | **Tidak dapat dijalankan** — memerlukan database sungguhan dengan kedua migration (`AddFinanceCollection`, `AddBillCollectionPrescriptionHandoff`) diterapkan | `NOT RUN` | Bagian 3.3 |

`AUTOMATED TEST: NOT APPLICABLE` — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).

Uji manual: `NOT APPLICABLE` — kedua migration terkait belum dijalankan, tidak ada database untuk
diuji end-to-end.

Keluaran `Invoke-QbeConformanceCheck.ps1` (dijalankan latar belakang atas working tree penuh):

```
QBE Conformance Report
Checker mode: ReportOnly
Scope: WorkingTree
Files evaluated: 10
Generated files excluded (bin/obj): 0
Test-scope files excluded from QBE-ENT-001/QBE-CFG-001/QBE-MOD-002: 0
VIOLATION: 0
REVIEW: 0
INFO: 0
Findings: none
REPORT ONLY: No enforcement/blocking performed.
Final result: PASS
```

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria (persis roadmap) | Status | Bukti |
| --- | --- | --- |
| Satu tender berhasil → satu penerimaan | **Terpenuhi untuk `SUCCEEDED`** — `IX_FinReceipt_SourceTenderId` unik parsial + pemeriksaan ulang `AnyAsync` sebelum insert | Bagian 2.3, 5 |
| Nominal disalin apa adanya | **Terpenuhi** — `Amount = handoff.Amount`, nol perhitungan ulang | `CreateSucceededReceiptAsync` |
| `FR-FIN-033` — tunai wajib menyebut shift kasirnya | **Terpenuhi** — lapis kedua di sisi Finance (bagian 1.4) | Bagian 1.4 |
| `FR-FIN-034` — penerimaan sebelum final tetap tercatat, kejadian tertahan | **Terpenuhi** — `RequiresFinalization` dihitung dari `SourceInvoiceStatus` | Bagian 1.6 |
| Pembalikan tender (`TenderStatus = REVERSED`) | **Terpenuhi 23 September 2026** — konflik constraint diperbaiki, `CreateReversalReceiptAsync` terimplementasi (bagian 1.5; kode di [BE-FIN-017](BE-FIN-017.md)) | Bagian 1.5 |
| Cakupan `FinReceipt`, `FinReceiptAllocation` | **Terpenuhi** untuk jalur `SUCCEEDED`, diperluas ke konsumsi nyata atas otorisasi eksplisit (bagian 0) | Bagian 2 |

**Belum terpenuhi**: pembalikan tender (`BLOCKED`, bagian 1.5 — perlu keputusan pemilik repository
atas skema); `UAT-05`/`06`/`20` (butuh database sungguhan, dan `UAT-06`/`20` kemungkinan menguji
jalur reversal juga — lihat catatan); mekanisme pelepasan `HELD_FOR_FINALIZATION` saat tagihan
kelak final (bagian 1.6, di luar cakupan task ini).

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| **Pembaruan 23 September 2026** | Konflik constraint pada bagian 1.5 diperbaiki (`CK_FinReceipt_TenderRequired` diberi klausa pengecualian baris pembalik). Jalur `REVERSED` kini terimplementasi penuh — lihat [laporan BE-FIN-017](BE-FIN-017.md) bagian 1 untuk `CreateReversalReceiptAsync` (lokasinya sejak refactor task itu). Migration baru ditulis, belum dieksekusi. Status task dinaikkan menjadi ✅ SELESAI. Baris Peringatan/Masalah di bawah (ditulis 22 September 2026) dipertahankan sebagai riwayat |
| Peringatan (riwayat, sudah ditutup — lihat Pembaruan di atas) | (1) Pembalikan tender (`TenderStatus = REVERSED`) **sengaja diblokir** — `CK_FinReceipt_TenderRequired` dan `IX_FinReceipt_SourceTenderId` (keduanya terkunci `data-dictionary.md`) berkonflik untuk kasus ini, **MUST** diputuskan pemilik repository sebelum dibangun (bagian 1.5). Setiap tender yang dibalik akan tersimpan sebagai baris `ERROR` yang terlihat, bukan silent failure — tetapi tetap butuh penyelesaian sebelum jalur ini benar-benar dipakai produksi. (2) `FinReceiptAllocation` tidak punya penulis apa pun sampai `BE-FIN-017` dikerjakan — piutang tidak akan pernah berkurang dari penerimaan sampai saat itu |
| Masalah yang diketahui | (1) Reversal `FinReceipt` `BLOCKED` (bagian 1.5). (2) Mekanisme pelepasan kejadian `HELD_FOR_FINALIZATION` saat tagihan kelak final belum ada task pemilik (bagian 1.6) — sama seperti keterbatasan yang sudah dicatat `BE-FIN-010`/`011` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` pada task ini |
| Status Git | 6 berkas baru (`FinReceipt.cs`, `FinReceiptAllocation.cs`, 2 configuration, migration + Designer.cs), 4 berkas disunting (`FinReceivable.cs`, `ApplicationDbContext.cs`, `FinanceBillingIntakeService.cs`, `ApplicationDbContextModelSnapshot.cs`) — plus 2 dokumen blueprint diperbarui |
| Langkah berikutnya | (1) `dotnet build` oleh pengguna. (2) **Keputusan pemilik repository** atas konflik constraint reversal (bagian 1.5) — dua opsi diusulkan di sana, keduanya mengubah skema terkunci. (3) Otorisasi eksekusi migration `AddFinanceCollection` **dan** `AddBillCollectionPrescriptionHandoff` (keduanya harus jalan sebelum `UAT-05` dapat dibuktikan; `UAT-06`/`20` bila mencakup skenario reversal menunggu butir 2). (4) `BE-FIN-017` (pembagian bayar-vs-piutang, `FinanceReceiptService`) dapat mulai untuk jalur `SUCCEEDED` — dependency-nya (`BE-FIN-016`) sudah source-complete untuk jalur itu |

