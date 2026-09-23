# Laporan Perubahan Backend — `BE-FIN-019`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-019` |
| Judul | Utang supplier — `FinanceSupplierPayableService` (input manual dan koreksi) |
| Slice | `POST-MVP` — `EPIC FIN-07`, bebas dikerjakan sejak `MVP-1` selesai, tidak menunggu siapa pun |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian 3 (tabel task) dan bagian 5 (`### POST-MVP`) |
| Trace | `FIN-DES-015` (bagian supplier), `FIN-DEC-014`, `FIN-DEC-015`; kontrak `FIN-API-1.0` §payable supplier; `state-transition-matrix.md` §5 — **terkunci** |
| Dependency | `BE-FIN-009` — ✅ ada (`FinanceReceivablesController`/pola service sudah berdiri; BE-FIN-019 tidak memakainya langsung, dependency ini sekadar menandai "modul Finance sudah operasional") |
| Klasifikasi | `NEW CODE` — submodul `Payable` sudah terdaftar `Fin`/`ACTIVE` sejak `BE-FIN-001` (lihat Backend Governance Preflight) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | Working tree pada branch `Yasmina`; commit dasar `a743388b57da91e6a0d7a42813604dc94563e38d` |
| Tanggal | 22 September 2026 |
| Status | 🟡 **SEBAGIAN — entity, EF configuration, migration (ditulis tangan, belum dijalankan), dan `FinanceSupplierPayableService` (input manual + koreksi + pembatalan) selesai penuh; QBE `PASS`.** Belum ada controller — lihat bagian 1.5 |

---

## 0. Backend Governance Preflight

| Item | Hasil |
| --- | --- |
| Area / Module / Submodule | Corporate / FinanceManagement / **Payable** |
| Baris registry | `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 17: `Corporate / Finance \| FinanceManagement / Payable / Utang \| BUSINESS DOMAIN / MODULE \| Fin \| ACTIVE` — terdaftar eksplisit sejak `BE-FIN-001` (catatan 2026-09-21) |
| Applicability | `NEW CODE` |
| QBE ID yang berlaku | `QBE-ENT-001` (audit/soft-delete), `QBE-NAM-00x` (penamaan `Fin`/`Mst`), `QBE-CODE-002/003` (larangan Count/Max/Last+1), `QBE-DB-001/002` (migration/database sebagai wewenang terpisah), `QBE-MOD-002/003` (registry submodule) |
| Hasil | Seluruh QBE ID di atas **PASS** — lihat bagian 5 |

Registry sudah mencakup `Payable` sebelum file model pertama ditulis (prasyarat `QBE-MOD-003`) — tidak ada pekerjaan registry baru yang diperlukan pada task ini.

---

## 1. Keputusan desain dan batas cakupan (didokumentasikan, bukan didiamkan)

### 1.1 Cakupan literal roadmap: 3 entity, plus service karena Acceptance Criteria menuntutnya

Kolom Cakupan roadmap untuk `BE-FIN-019` hanya menyebut tiga entity (`FinSupplierPayable`,
`FinSupplierPayableItem`, `FinPayableAdjustment`). Namun kolom Acceptance Criteria yang sama
berbunyi **"Input manual utang dan koreksinya"** — sebuah entity murni tidak dapat "menginput"
atau "mengoreksi" apa pun, sehingga acceptance criteria itu sendiri menuntut lapisan service.
`02-backend-architecture.md` §4.22 juga sudah menamai service ini secara eksplisit:
`FinanceSupplierPayableService` di `Payable/Services/FinanceSupplierPayableService.cs`,
tanggung jawab "Input manual utang supplier dan koreksinya" — **persis** kalimat acceptance
criteria roadmap. Karena arsitektur sudah mengunci nama dan tanggung jawabnya, tidak ada
keputusan bisnis baru yang diarang di sini — hanya menjalankan apa yang sudah tertulis eksplisit
di dua dokumen kontrak yang berbeda dan saling menguatkan. Ini konsisten dengan preseden
"Entities + full wiring" yang dipilih pemilik repository untuk `BE-FIN-016` di task sebelumnya
dalam sesi yang sama.

### 1.2 `FinDoctorPayable` tidak pernah dibuat — `FinPayableAdjustment` langsung memakai bentuk final `MedicalServicePayableId`

`data-dictionary.md` §4.7 (revisi 1) mendefinisikan `FinPayableAdjustment` dengan
`DoctorPayableId`, tetapi Amendment Revisi 2 (`A.5`) menggantinya menjadi `MedicalServicePayableId`
menunjuk `FinMedicalServicePayable` — dan `01-backend-roadmap.md` bagian 5 secara eksplisit
menyatakan `FinDoctorPayable`/`FinDoctorPayableItem` **"Dibatalkan pada revisi 2... Tidak pernah
dibuat, jadi tidak ada yang perlu dimigrasikan."** Karena itu, task ini membangun `FinPayableAdjustment`
langsung dengan nama kolom akhir (`MedicalServicePayableId`), bukan `DoctorPayableId` yang sudah
mati sejak sebelum task ini dimulai — tidak ada rename yang perlu dilakukan nanti.

### 1.3 `MedicalServicePayableId` disiapkan tanpa foreign key — mengikuti preseden "kolom disiapkan lebih dulu"

`FinMedicalServicePayable` adalah cakupan `BE-FIN-021`, yang **BLOCKED** menunggu modul Medical
Fee (`FIN-CAP-021`) — tabelnya belum ada. `FinPayableAdjustment.MedicalServicePayableId` karena
itu ditambahkan sekarang sebagai kolom `Guid?` polos **tanpa** foreign key constraint (berbeda
dari `SupplierPayableId` yang FK penuh ke `FinSupplierPayable`, tabel yang sudah ada). Ini
mengikuti preseden yang sudah eksplisit di `01-backend-roadmap.md` bagian 5 untuk `EPIC FIN-04`
("Kolomnya sudah disiapkan sehingga skema tidak perlu berubah lagi nanti"). Menambahkan FK
constraint-nya nanti pada `BE-FIN-021` adalah `ALTER TABLE ADD CONSTRAINT` aditif murni —
kolomnya akan selalu `NULL` sampai saat itu karena `FinanceSupplierPayableService` **tidak
pernah** membuat baris `PayableType = MEDICAL_SERVICE` (`DecideAdjustmentAsync` menolaknya
eksplisit di baris kode, lihat bagian 2). Check constraint `CK_FinPayableAdjustment_PayableType`
tetap mengizinkan kedua nilai (`SUPPLIER`, `MEDICAL_SERVICE`) sejak sekarang — sesuai bentuk akhir
`data-dictionary.md` Amendment `A.5` — supaya `BE-FIN-021` tidak perlu mengubah check constraint
yang sudah ada, hanya menambah FK.

### 1.4 Invariant `OriginalAmount = OutstandingAmount + PaidAmount + AdjustedAmount` adalah ekstrapolasi, bukan kutipan langsung kontrak

`data-dictionary.md` §4.1 dan `02-backend-architecture.md` §9 (tabel invariant) **tidak**
menuliskan rumus keseimbangan untuk `FinSupplierPayable` secara eksplisit — berbeda dari
`FinReceivable` yang punya `FIN-VAL-011` tertulis jelas. Namun bentuk kolomnya
(`OriginalAmount`/`OutstandingAmount`/`PaidAmount`/`AdjustedAmount`) identik strukturnya dengan
dekomposisi `FinReceivable`. `CK_FinSupplierPayable_Balance` ditambahkan sebagai penerapan pola
yang sama, secara transparan ditandai sebagai ekstrapolasi teknis (bukan kutipan kontrak) baik di
komentar model maupun di laporan ini — supaya pemilik repository dapat mengoreksi bila memang
tidak diinginkan. Constraint ini tidak pernah gagal pada kode yang ditulis task ini karena
`PaidAmount` MUST tetap nol sampai `BE-FIN-020` ada.

### 1.5 Konvensi Direction dibalik dari piutang — dikutip literal dari kontrak, bukan diasumsikan

`state-transition-matrix.md` §5 menulis literal: **"DEBIT mengurangi utang, CREDIT menambah"** —
kebalikan `FinReceivableAdjustment` (`CREDIT` mengurangi piutang, `DEBIT` menambah). Ini konsisten
dengan konvensi akuntansi standar (piutang adalah akun aset bersaldo normal debit; utang adalah
akun liabilitas bersaldo normal kredit), bukan kesalahan pengetikan pada salah satu kontrak.
`DecideAdjustmentAsync` (bagian 2) menerapkan `DEBIT` mengurangi `OutstandingAmount` +
menambah `AdjustedAmount`, dan `CREDIT` sebaliknya — tepat kebalikan urutan operasi
`FinanceReceivableService.DecideAdjustmentAsync`. Validasi batas atas juga dibalik: `DEBIT`
(yang mengurangi) MUST NOT melebihi sisa utang, sejajar dengan `CREDIT` pada Receivable.

### 1.6 Belum ada controller

Tidak ada baris task roadmap manapun (`01-backend-roadmap.md`) yang memberi wewenang eksplisit
membangun `FinanceSupplierPayablesController` meski `02-backend-architecture.md` §4.22/4.23 sudah
menamainya. Sama seperti pola `BE-FIN-008`→`BE-FIN-009` (Receivable) dan `BE-FIN-018`
(Collection/allocation), task ini berhenti di lapisan service — `CreateAsync`/
`RequestAdjustmentAsync`/`ApproveAdjustmentAsync`/`RejectAdjustmentAsync`/`CancelAsync` belum
dapat dipanggil lewat HTTP. Dicatat sebagai langkah berikutnya, bukan gap yang menahan task ini.

### 1.7 Sengaja belum menyambungkan kejadian Accounting

Katalog `FinAccountingEventTypeCodes` sudah memiliki `PENGAKUAN-HUTANG-SUPPLIER` dan
`PENYESUAIAN-HUTANG` — tetapi task ini **sengaja tidak memanggil**
`FinanceAccountingOutboxService.StageEventAsync` sama sekali. Alasannya siklus hidup yang identik
dengan Receivable: `FinanceReceivableService` (`BE-FIN-006`/`008`) dibangun lebih dulu tanpa
panggilan outbox, baru disambungkan belakangan oleh `BE-FIN-011` sebagai task tersendiri.
Menyambungkannya sekarang berarti melampaui Cakupan literal `BE-FIN-019` (tiga entity di atas)
dan mengarang wewenang task yang belum diberikan. `FinSupplierPayable` juga tidak memiliki kolom
`CorrelationId`/`CausationId` (berbeda dari `FinReceivable`) karena tidak ada fakta upstream
lintas modul untuk dilacak — task pengait outbox nanti dapat memakai `payable.Id` sendiri sebagai
akar korelasi tanpa kolom tambahan.

### 1.8 `Batalkan` adalah aksi langsung, bukan maker-checker

`state-transition-matrix.md` §5 baris "`OUTSTANDING` \| Batalkan \| `CANCELLED` \| Penyetuju AP"
tidak melalui status `REQUESTED` — berbeda dari koreksi (`FinPayableAdjustment`, maker-checker
penuh). `FinSupplierPayable.Status` sendiri tidak memiliki nilai `REQUESTED`, mengonfirmasi
`CancelAsync` memang dirancang sebagai satu langkah oleh peran berwenang (`Penyetuju AP`), bukan
dua langkah pengaju/penyetuju.

---

## 2. Ringkasan pekerjaan

### 2.1 Entity dan konfigurasi EF baru

| Berkas | Isi |
| --- | --- |
| `Areas/Corporate/FinanceManagement/Payable/Models/FinSupplierPayable.cs` | Aggregate root; `PayableNumber`, `SupplierId` (FK `MstSupplier`), `SupplierInvoiceNumber`/`Date`, `Description?`, `OriginalAmount`/`OutstandingAmount`/`PaidAmount`/`AdjustedAmount`, `DueDate`, `PaymentTermDays`, `Status`, `RowVersion`, nav `Items`/`Adjustments` |
| `Areas/Corporate/FinanceManagement/Payable/Models/FinSupplierPayableItem.cs` | Rincian; `PayableId`, `Description`, `Quantity`, `UnitPrice`, `Amount` |
| `Areas/Corporate/FinanceManagement/Payable/Models/FinPayableAdjustment.cs` | Koreksi polimorfik; `PayableType`, `SupplierPayableId?` (FK), `MedicalServicePayableId?` (tanpa FK, bagian 1.3), `Direction`, `Amount`, `Reason`, status maker-checker `REQUESTED`/`APPROVED`/`REJECTED` |
| `Repositories/Configurations/Corporate/FinanceManagement/Payable/FinSupplierPayableConfiguration.cs` | `CK_FinSupplierPayable_Status/Outstanding/Balance`; unique index `PayableNumber`; unique composite `SupplierId+SupplierInvoiceNumber`; index `Status+DueDate`; FK `Supplier` |
| `Repositories/Configurations/Corporate/FinanceManagement/Payable/FinSupplierPayableItemConfiguration.cs` | FK `Payable`, nol check constraint |
| `Repositories/Configurations/Corporate/FinanceManagement/Payable/FinPayableAdjustmentConfiguration.cs` | `CK_FinPayableAdjustment_PayableType/Direction/Status/Amount/MakerChecker/ExactlyOnePayable/PayableTypeMatch`; unique index `AdjustmentNumber`; FK `SupplierPayable` |

### 2.2 `FinanceSupplierPayableService` (baru)

| Method | Kegunaan |
| --- | --- |
| `CreateAsync` | Input manual (`FIN-DEC-015`); menghitung `OriginalAmount` dari jumlah item (bukan menerima lalu memvalidasi), menyalin `PaymentTermDays` dari `MstSupplier`, menghitung `DueDate`, menegakkan `409` faktur dobel dengan pesan literal kontrak |
| `RequestAdjustmentAsync` | Ajukan koreksi (`PayableType` selalu `SUPPLIER` dari service ini) |
| `ApproveAdjustmentAsync` / `RejectAdjustmentAsync` → `DecideAdjustmentAsync` | Maker-checker penuh; `Serializable` + advisory lock `FIN_PAYABLE_{id}`; menegakkan arah Debit/Credit terbalik (bagian 1.5); menolak `PayableType != SUPPLIER` |
| `CancelAsync` | Aksi langsung (bagian 1.8); menolak selain `OUTSTANDING` |

### 2.3 Migration dan snapshot

- `Migrations/20260922110000_AddFinanceSupplierPayable.cs` + `.Designer.cs` (baru, **belum
  dijalankan**) — membuat `FinSupplierPayable` lebih dulu, lalu `FinPayableAdjustment` (FK ke
  `FinSupplierPayable`), lalu `FinSupplierPayableItem` (FK ke `FinSupplierPayable`); termasuk
  index FK `IX_FinPayableAdjustment_SupplierPayableId` yang ditambahkan tangan (auto-generated
  EF, bukan bagian `HasIndex` eksplisit — pola yang sama dengan temuan `BE-FIN-016`).
- `Migrations/ApplicationDbContextModelSnapshot.cs` — disunting 3 tempat (properti, relasi,
  navigasi) diselipkan alfabetis di antara blok `MasterData` dan `PettyCash` di setiap section.

### 2.4 DI dan DbContext

- `Repositories/ApplicationDbContext.cs`: `using` + 3 `DbSet` baru (`FinSupplierPayables`,
  `FinSupplierPayableItems`, `FinPayableAdjustments`).
- `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs`:
  `using` + `services.AddScoped<FinanceSupplierPayableService>();` — mengikuti preseden
  (seluruh service Finance terdaftar di berkas DI Billing ini, lihat bagian 3.4).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/contracts/state-transition-matrix.md` §5 (dibaca penuh)
- `docs/module-blueprints/finance-management/erd/data-dictionary.md` §4.1, 4.2, 4.7, Amendment A.5, A.6 (dibaca penuh)
- `docs/module-blueprints/finance-management/02-backend-architecture.md` §4.9, 4.10, 4.15, 4.22, 4.23, §9, §10, Amendment Revisi 2 (dibaca penuh)
- `docs/module-blueprints/finance-management/04-prd-to-mvp.md` §EPIC FIN-07/08/09, bagian 20 (dibaca)
- `docs/module-blueprints/finance-management/00-interview-decisions.md` — `FIN-DEC-014`, `FIN-DEC-015`
- `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris `Payable`
- `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableService.cs`,
  `Models/FinReceivable.cs`, `Models/FinReceivableAdjustment.cs` (dibaca penuh — cetakan pola)
- `Repositories/Configurations/Corporate/FinanceManagement/Receivable/*.cs` (dibaca penuh — cetakan konfigurasi)
- `Areas/Administrator/MasterData/Models/MstSupplier.cs`,
  `Areas/Corporate/FinanceManagement/MasterData/Models/MstBankAccount.cs` +
  `MstBankAccountConfiguration.cs` (dibaca penuh — preseden FK lintas Area ke Administrator)
- `Migrations/20260921000002_AddFinanceReceivableAndCollection.cs`,
  `20260921000000_AddFinanceMasterData.cs` (dibaca penuh — cetakan migration)
- `Areas/Corporate/FinanceManagement/AccountingIntegration/Models/FinAccountingEventOutbox.cs`,
  `Services/FinanceAccountingOutboxService.cs` (dibaca — dasar keputusan bagian 1.7)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/Payable/Models/FinSupplierPayable.cs` | Baru |
| `Areas/Corporate/FinanceManagement/Payable/Models/FinSupplierPayableItem.cs` | Baru |
| `Areas/Corporate/FinanceManagement/Payable/Models/FinPayableAdjustment.cs` | Baru |
| `Areas/Corporate/FinanceManagement/Payable/Services/FinanceSupplierPayableService.cs` | Baru |
| `Repositories/Configurations/Corporate/FinanceManagement/Payable/FinSupplierPayableConfiguration.cs` | Baru |
| `Repositories/Configurations/Corporate/FinanceManagement/Payable/FinSupplierPayableItemConfiguration.cs` | Baru |
| `Repositories/Configurations/Corporate/FinanceManagement/Payable/FinPayableAdjustmentConfiguration.cs` | Baru |
| `Migrations/20260922110000_AddFinanceSupplierPayable.cs` | Baru |
| `Migrations/20260922110000_AddFinanceSupplierPayable.Designer.cs` | Baru |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Disunting — 3 blok baru di 3 section |
| `Repositories/ApplicationDbContext.cs` | +`using`, +3 `DbSet` |
| `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` | +`using`, +1 `AddScoped` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — tidak ada Controller/DTO API baru (bagian 1.6) |
| Database | 3 tabel baru (`FinSupplierPayable`, `FinSupplierPayableItem`, `FinPayableAdjustment`), migration ditulis tangan, **belum dijalankan** |
| Keamanan/Auth | `NOT APPLICABLE` — tidak ada endpoint. `SupplierId`/`SupplierInvoiceNumber` bukan data pasien |

### 3.4 Catatan lintas berkas

`FinanceSupplierPayableService` didaftarkan di
`BillingManagementServiceCollectionExtensions.cs` mengikuti preseden **seluruh** service Finance
lain (`FinanceReceivableService`, `FinanceReceiptService`, `FinanceCashManagementService`, dst.)
— bukan keputusan baru task ini, melainkan pola DI existing yang diikuti apa adanya
(`AGENTS.md`: "Ikuti kode yang sudah ada. Jangan menciptakan arsitektur baru").

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — lihat bagian 1.6.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | **Tidak dijalankan oleh saya** | `NOT RUN` | Atas instruksi pengguna sejak `BE-FIN-002` |
| `powershell.exe -NoProfile -File tooling/qbe/Invoke-QbeConformanceCheck.ps1` | Lihat kutipan di bawah | `PASS` | Dijalankan latar belakang, 22 berkas dievaluasi (working tree kumulatif termasuk `BE-FIN-018`) |
| Review manual: `CK_FinPayableAdjustment_ExactlyOnePayable`/`PayableTypeMatch` konsisten dengan `FIN-DES-016` | Dikonfirmasi — pola `num_nonnulls` sama dengan `CK_CliAssessmentInstrumentResponse_OneOwner` (precedent lain di repo) | `PASS` | Bagian 2.1 |
| Review manual: `DecideAdjustmentAsync` menolak `PayableType != SUPPLIER` | Dikonfirmasi lewat pembacaan kode — baris eksplisit sebelum lock diambil | `PASS` | Bagian 2.2 |
| Review manual: `DEBIT` tidak boleh melebihi sisa utang (bukan `CREDIT`, dibalik dari Receivable) | Dikonfirmasi — kondisi `Direction == Debit && Amount > OutstandingAmount` | `PASS` | Bagian 1.5 |
| Review manual: faktur dobel (`SupplierId`+`SupplierInvoiceNumber`) ditolak dengan pesan literal kontrak | Dikonfirmasi — `AnyAsync` sebelum insert + `PayableConflictException("Faktur supplier ini sudah pernah diinput")` | `PASS` | Bagian 2.2 |
| Review manual: `OriginalAmount` dihitung dari item, tidak diterima sebagai input terpisah | Dikonfirmasi — `CreateAsync` tidak memiliki parameter `originalAmount` | `PASS` | Bagian 1.4 |
| Review manual: `CancelAsync` menolak `PARTIAL`/`PAID`/`CANCELLED` | Dikonfirmasi — `payable.Status != Outstanding` | `PASS` | Bagian 1.8 |
| Review manual: migration `Up()` membuat tabel dalam urutan aman FK (`FinSupplierPayable` → `FinPayableAdjustment`/`FinSupplierPayableItem`) | Dikonfirmasi | `PASS` | Bagian 2.3 |
| `UAT` terkait `EPIC FIN-07` | **Tidak ada UAT bernomor eksplisit di kontrak untuk epic ini** (berbeda dari `EPIC FIN-05/06`) — dicatat sebagai temuan, bukan diarang | `NOT APPLICABLE` | `04-prd-to-mvp.md` bagian 20 |

`AUTOMATED TEST: NOT APPLICABLE` — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).

Uji manual runtime: `NOT APPLICABLE` — migration belum dijalankan, tidak ada jalur HTTP untuk diuji end-to-end.

Keluaran `Invoke-QbeConformanceCheck.ps1` (dijalankan latar belakang atas working tree penuh):

```
QBE Conformance Report
Checker mode: ReportOnly
Scope: WorkingTree
Files evaluated: 22
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
| Input manual utang dan koreksinya | **Terpenuhi** — `CreateAsync`, `RequestAdjustmentAsync`, `ApproveAdjustmentAsync`, `RejectAdjustmentAsync` | Bagian 2.2 |
| Reuse `MstSupplier` (`FIN-DEC-014`) — tidak membuat master Supplier baru | **Terpenuhi** — FK langsung ke `MstSupplier` milik Administrator, nol tabel Supplier baru | Bagian 2.1 |
| Cakupan tiga entity (`FinSupplierPayable`, `FinSupplierPayableItem`, `FinPayableAdjustment`) | **Terpenuhi** | Bagian 2.1 |
| DoD modul #2 (`IdentityModel`, nol hard delete), #3 (prefix `Fin`), #4 (`string` + `Statuses` + check constraint), #5 (`RowVersion`), #7 (`Serializable` lintas-agregat), #8 (partial unique index `WHERE IsDelete = false`), #9 (`HasPrecision(18,2)`), #10 (lokasi EF configuration), #15 (migration aditif) | **Seluruhnya terpenuhi** | Bagian 2.1, 2.3 |
| DoD modul #6 (`Idempotency-Key`) | **Belum berlaku** — hanya relevan di lapisan controller/HTTP, belum ada (bagian 1.6), sama seperti `BE-FIN-008` sebelum `BE-FIN-009` | Bagian 1.6 |
| DoD modul #14 (kejadian Accounting) | **Sengaja belum** — lihat bagian 1.7 | Bagian 1.7 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Masalah yang diketahui | (1) Belum ada controller (bagian 1.6). (2) Belum tersambung ke `FinAccountingEventOutbox` (bagian 1.7), meski `EventTypeCode`-nya sudah ada di katalog. (3) `CK_FinSupplierPayable_Balance` adalah ekstrapolasi dari pola `FinReceivable`, bukan kutipan langsung kontrak (bagian 1.4) — mohon dikonfirmasi pemilik repository |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` pada task ini |
| Status Git | 9 berkas baru, 3 berkas disunting |
| Langkah berikutnya | (1) `dotnet build` oleh pengguna. (2) Otorisasi eksekusi migration `AddFinanceSupplierPayable`. (3) Task controller baru (`FinanceSupplierPayablesController`, `02-backend-architecture.md` §4.23) untuk mengekspos `FinanceSupplierPayableService` lewat HTTP — termasuk penambahan `Idempotency-Key` di titik itu. (4) Task pengait outbox (pola `BE-FIN-011`) untuk `PENGAKUAN-HUTANG-SUPPLIER`/`PENYESUAIAN-HUTANG`. (5) `BE-FIN-020` (`FinancePaymentService`) akan menjadi penulis `PaidAmount` — MUST tetap satu-satunya jalur, mengikuti pola `FinanceReceivableService.ApplyAllocationAsync`. (6) `BE-FIN-021` menambahkan FK `MedicalServicePayableId` (aditif, bagian 1.3) setelah `FinMedicalServicePayable` ada |

