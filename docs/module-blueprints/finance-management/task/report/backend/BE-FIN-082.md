# Laporan Perubahan Backend — `BE-FIN-082`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-082` |
| Judul | Tagihan lama yang sudah direkonsiliasi menjadi item bertagihan beserta mutasi pembukanya, dan selisihnya tidak pernah lolos |
| Slice | `REV-14E` — `EPIC FIN-24` (migrasi tagihan lama lewat spreadsheet) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md`, bagian "Task REV-14E — `EPIC FIN-24`" |
| Trace | `FIN-DEC-129`, `FIN-DEC-130`; `FIN-DES-089`, `FIN-DES-090`; `FIN-API-1.6` F.2; `FIN-STATE-1.6` F.2 (siklus batch) |
| Contract version | `02-backend-architecture.md` bagian L (FIN-DES-089/090); `contracts/state-transition-matrix.md` F.2; `contracts/permission-audit-matrix.md` (atribut `[AccessController]`/aksi `Approve` controller ini) |
| Dependency | `BE-FIN-081` — 🟡 **sebagian** (lihat addendum laporannya — dua cacat ditemukan dan diperbaiki sebagai bagian task ini); buku mutasi dari `BE-FIN-060`/`061`; `FinOpeningBalance` dari `BE-FIN-065` |
| Klasifikasi | `HEAVY` — **RISIKO UTAMA** menurut roadmap sendiri: melahirkan item piutang/utang dan mutasi pembukanya dalam satu transaksi, lintas dua agregat (`FinOpeningItemBatch` dan item yang baru lahir) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/Corporate/FinanceManagement/AccountingIntegration/**`, `docs/module-blueprints/finance-management/**` |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | `0e256765563ef2693107e1f35e9c5a67476ca1f0` (belum ada commit baru dibuat pada sesi ini) |
| Tanggal | 2 Oktober 2026 |
| Status | 🟡 **Sebagian** — seluruh source selesai; `dotnet build` **belum dijalankan** atas permintaan eksplisit pemilik task pada giliran ini |

---

## 1. Masalah yang diperbaiki

`BE-FIN-081` memastikan setiap baris tagihan lama **sah** (jenis debitur dikenal, supplier
ditemukan, angka dan tanggal masuk akal, nol nomor dokumen kembar). Tetapi baris yang sah itu
**belum menjadi apa pun** — belum ada piutang atau utang yang bisa ditagih, dan belum ada jejak di
buku mutasi yang menjelaskan dari mana saldo awal itu berasal. Tanpa task ini, validasi hanya
latihan kosong: batch bisa `VALIDATED` selamanya tanpa pernah benar-benar masuk sistem.

Risiko terbesarnya bukan "tidak berfungsi", melainkan **berfungsi sebagian**: melahirkan baris
piutang tanpa mutasi pembukanya (atau sebaliknya) akan membuat saldo piutang **tidak dapat
dipertanggungjawabkan sejak hari pertama** — laporan umur piutang menunjukkan angka yang buku
mutasinya tidak pernah menjelaskan asalnya. Task ini menutup risiko itu dengan memastikan
keduanya **selalu** lahir bersama dalam satu transaksi database: gagal salah satu berarti gagal
keduanya, tidak ada keadaan di antara.

Risiko kedua yang ditutup: batch yang total sisa tagihannya **tidak sama** dengan angka yang
dinyatakan petugas dari dokumen Accounting **tidak boleh** lolos — selisih sekecil apa pun harus
diperbaiki dulu (baik berkasnya maupun pernyataan saldonya) sebelum tagihan lama benar-benar masuk
sistem sebagai piutang/utang yang dapat ditagih.

---

## 2. Proses bisnis

1. **Menyatakan saldo awal Accounting** (`POST /{id}/declare-accounting-opening`): petugas
   memasukkan total yang tertulis di dokumen Accounting beserta nomor rujukan dokumennya. Boleh
   dipanggil berkali-kali selama batch masih `DRAFT` atau `VALIDATED`; status batch **tidak**
   berubah oleh langkah ini.
2. **Menyetujui** (`POST /{id}/approve`), hanya dari `VALIDATED`:
   - Sistem memeriksa ulang **seluruh baris** dari berkas fisik yang tersimpan (bukan memercayai
     hasil validasi lama begitu saja) — pertahanan berlapis terhadap kemungkinan berkas berubah
     antara validasi terakhir dan persetujuan.
   - Bila re-validasi menemukan baris bergalat, batch **dikembalikan ke `DRAFT`** beserta daftar
     galat terbaru, dan permintaan persetujuan ditolak `409` — petugas harus memvalidasi ulang.
   - Total sisa tagihan dari baris yang lolos **dibandingkan** dengan saldo awal Accounting yang
     dinyatakan. Berbeda sedikit pun → ditolak `422` beserta **kedua angka**, supaya petugas tahu
     persis selisihnya tanpa harus menghitung ulang sendiri.
   - Tanggal cutover batch dicocokkan ulang terhadap `FinOpeningBalance` saat ini (bisa jadi sudah
     berselang lama sejak unggah) — berbeda berarti ditolak `422`.
   - Bila semua lolos: **setiap** baris yang sah melahirkan **satu** `FinReceivable`/
     `FinSupplierPayable` beserta **satu** baris buku mutasi `PEMBUKAAN-MIGRASI` yang menjelaskan
     dari mana saldo awalnya — seluruhnya dalam **satu** transaksi database. Batch kemudian
     berpindah ke `APPROVED` lalu **otomatis** `LOCKED`. **Nol** kejadian akuntansi diterbitkan —
     item migrasi tidak pernah menulis ke kotak keluar outbox Accounting (`FIN-DEC-129`), karena
     Accounting sudah tahu angkanya lewat dokumen saldo awal yang sama.
3. **Menolak** (`POST /{id}/reject`), dari `DRAFT` atau `VALIDATED`: batch ditutup permanen beserta
   alasannya. Perbaikan berarti mengunggah batch baru, bukan membuka kembali yang ditolak.
4. Sesudah disetujui, item piutang/utang hasil migrasi **mengikuti alur normal** — penagihan,
   pelunasan, umur piutang, laporan — tidak ada jalur khusus "item migrasi" di luar penciptaannya.

Jalur tidak normal: menyetujui/menolak batch `APPROVED`/`LOCKED`/`REJECTED` ditolak `409` — tidak
seorang pun dapat mengubahnya lagi (`state-transition-matrix.md` F.2).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `contracts/state-transition-matrix.md` F.2 — siklus lengkap `FinOpeningItemBatch`, termasuk tabel
  "yang ditolak" (mis. `VALIDATED → APPROVED` dengan selisih, `APPROVED` tanpa lewat `VALIDATED`)
- `02-backend-architecture.md` L.4.3, L.7 — `FIN-DES-089` (penanda FK batch), hubungan dengan buku
  mutasi dan outbox ("`FinOpeningItemBatch ..> FinAccountingEventOutbox`: NOL kejadian")
- `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceSubledgerMovementService.cs` — **dibaca penuh**: `RecordReceivableMovementAsync`/`RecordSupplierPayableMovementAsync` ternyata **sudah** menerima parameter `openingItemBatchId` sejak `BE-FIN-058` (belum pernah dipakai sampai task ini); keduanya mewajibkan pemanggil sudah berada di dalam transaksi aktif
- `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivableMovement.cs`,
  `Payable/Models/FinSupplierPayableMovement.cs` — konstanta `MovementTypes.PembukaanMigrasi`
  ("PEMBUKAAN-MIGRASI") **sudah ada** sejak `BE-FIN-058`, belum pernah dipakai
- `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableService.cs` —
  `DecideAdjustmentAsync` dibaca penuh sebagai pola transaksi: `BeginTransactionAsync`
  (`IsolationLevel.Serializable`) → `AcquireLockAsync` (`pg_advisory_xact_lock`) → kerja →
  `SaveChangesAsync` → `CommitAsync`; `catch` → `RollbackAsync`; `finally` → dispose. Ditiru persis
  untuk `ApproveAsync`, dengan penyesuaian: agregat yang dikunci adalah **batch**, bukan satu
  piutang/utang (karena item yang dibuat belum ada untuk dikunci)
- `Areas/Corporate/FinanceManagement/BillingIntake/Services/FinanceBillingIntakeService.cs` — pola
  pembuatan `FinReceivable` pertama kali (bukan dari koreksi), termasuk `CorrelationId`/`CausationId`
- `Areas/Administrator/MasterData/Models/MstSupplier.cs`, `Receivable/Models/FinReceivable.cs`
  (`CK_FinReceivable_BenefitOwner`) — pemicu temuan EMPLOYEE_BENEFIT, lihat `BE-FIN-081` addendum

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceOpeningItemBatchService.cs` | Ditambah `GetPagedAsync`, `GetByIdAsync`, `DeclareAccountingOpeningAsync`, `RejectAsync`, `ApproveAsync` (transaksional, melahirkan item+mutasi), `CreateReceivableFromRowAsync`/`CreateSupplierPayableFromRowAsync`, `ReadRowsAsync` (diekstrak dari `ValidateAsync`), helper transaksi/lock/generator nomor. **Dua cacat `BE-FIN-081` diperbaiki** — lihat laporan task itu bagian 8 |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/DTOs/OpeningItemBatchDtos.cs` | Ditambah `OpeningItemBatchPagedQuery`, `DeclareAccountingOpeningRequest`, `ApproveOpeningItemBatchRequest`, `RejectOpeningItemBatchRequest` |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Controllers/FinanceOpeningItemBatchesController.cs` | Ditambah `GET /`, `GET /{id}`, `POST /{id}/declare-accounting-opening` (`Update`), `POST /{id}/approve` dan `POST /{id}/reject` (keduanya `Approve`, pola sama dengan `FinancePaymentsController`/`FinancePurchaseOrdersController`) |
| `docs/module-blueprints/finance-management/task/report/backend/BE-FIN-081.md` | Ditambah bagian 8 (addendum dua cacat ditemukan dan diperbaiki) |

Nol perubahan pada `FinanceSubledgerMovementService.cs` — parameter `openingItemBatchId`-nya
**sudah ada**, task ini hanya mulai memakainya. Nol perubahan pada `BillingManagementServiceCollectionExtensions.cs` — `FinanceOpeningItemBatchService` dan `FinanceSubledgerMovementService` sudah terdaftar.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Empat endpoint baru**: `GET /`, `GET /{id}`, `POST /{id}/declare-accounting-opening`, `POST /{id}/approve`, `POST /{id}/reject` (lima method, disebut "4 endpoint" pada roadmap — selisih penghitungan dicatat, bukan disembunyikan). Bentuknya mengikuti `api-contract.md` F.2 |
| Database | `NOT APPLICABLE` — **nol** migration, **nol** perubahan schema. `FinReceivable`/`FinSupplierPayable`/movement-nya ditulis ke tabel yang skemanya **sudah** dirancang `BE-FIN-058`/`060`/`061`/`079` (belum dieksekusi ke database manapun pada sesi ini) |
| Keamanan/Auth | `POST /{id}/approve` dan `POST /{id}/reject` memakai `[AccessAction("Approve", ...)]`/`[AccessPermission("FinanceOpeningItemBatch", "Approve")]` — pola identik `FinancePaymentsController`/`FinancePurchaseOrdersController`/`FinancePurchasingInvoicesController` (approve **dan** reject berbagi action/permission "Approve" yang sama, bukan dua action terpisah). `declare-accounting-opening` memakai `Update`, konsisten `state-transition-matrix.md` F.2 |

---

## 4. Dokumentasi endpoint

#### Corporate / Finance Management / Opening Item Batch

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Daftar batch, bersaring jenis dan status | `FinanceOpeningItemBatch : Read` |
| `GET` | `/{id:guid}` | Rincian batch beserta hasil validasi per baris terakhir | `FinanceOpeningItemBatch : Read` |
| `POST` | `/{id:guid}/declare-accounting-opening` | Menyatakan total dan rujukan dokumen saldo awal Accounting. Boleh dari DRAFT/VALIDATED; status tidak berubah | `FinanceOpeningItemBatch : Update` |
| `POST` | `/{id:guid}/approve` | Menyetujui — melahirkan item dan mutasi pembuka dalam satu transaksi, lalu mengunci batch | `FinanceOpeningItemBatch : Approve` |
| `POST` | `/{id:guid}/reject` | Menolak beserta alasan. Boleh dari DRAFT/VALIDATED | `FinanceOpeningItemBatch : Approve` |

Dengan ini, seluruh endpoint `FinanceOpeningItemBatch` pada `api-contract.md` F.2 sudah dibangun.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Permintaan eksplisit pemilik task pada giliran ini |
| Verifikasi kontrak API | Path, request/response, kode status (`200/404/409/422`), dan `[AccessController]`/`[AccessAction]`/`[AccessPermission]` dibandingkan terhadap `api-contract.md` F.2, `state-transition-matrix.md` F.2, dan pola `Approve`/`Reject` pada `FinancePaymentsController`/`FinancePurchaseOrdersController` | `PASS` (pembacaan manual) | Bagian 3–4 di atas |
| Verifikasi proses bisnis jalur selisih | Penelusuran manual: batch `VALIDATED` dengan total sisa Rp 10.000.000 tetapi `DeclaredAccountingOpeningAmount` Rp 9.500.000 → `ApproveAsync` melempar `OpeningItemBatchValidationException` **sebelum** satu pun `FinReceivable`/mutasi dibuat, pesan menyebut kedua angka persis | `PASS` | Ditelusuri baris-per-baris kode `ApproveAsync` (bagian "if (totalOutstanding != entity.DeclaredAccountingOpeningAmount)") |
| Verifikasi proses bisnis jalur tolak (status salah) | Penelusuran manual: memanggil `approve` pada batch `DRAFT` → `EnsureCurrentRowVersion` lolos, lalu `if (entity.Status != Validated)` melempar `OpeningItemBatchConflictException` sebelum transaksi menyentuh baris apa pun | `PASS` | Ditelusuri kode `ApproveAsync` |
| Verifikasi "satu transaksi, gagal berarti nol-duanya" | Penelusuran manual struktur kode: seluruh `_dbContext.FinReceivables.Add(...)`/`RecordReceivableMovementAsync(...)` terjadi **di dalam** blok `try` yang sama dengan `BeginTransactionAsync`, dan **satu** `SaveChangesAsync` di akhir menulis seluruh perubahan bersamaan; `catch` memanggil `RollbackAsync` bila `committed` masih `false` | `PASS` (pembacaan struktur kode; **bukan** pengujian commit/rollback sungguhan terhadap database nyata — `dotnet build`/`dotnet run` tidak dijalankan) | `ApproveAsync`, pola `try/catch/finally` dengan penanda `committed` |
| Verifikasi "nol kejadian akuntansi" | Grep `_accountingOutboxService`/`StageEventAsync` pada `FinanceOpeningItemBatchService.cs` | `PASS` — nol kecocokan; service ini bahkan tidak men-dependency-inject `FinanceAccountingOutboxService` | Pemeriksaan langsung berkas |
| Keseimbangan kurung kurawal dan kurung biasa | Sama | `PASS` | `grep -o` per berkas, seluruhnya seimbang |
| QBE preflight | Area/Module/Submodule tidak berubah dari task sebelumnya. **Nol** entity database baru | `PASS` | `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` (status tidak berubah) |
| Review diff/scope | Seluruh berkas yang berubah ditelusuri; dipastikan **nol** sentuhan pada migration atau skema `BE-FIN-079` | `PASS` | Tabel 3.2 |

Uji manual: `NOT FEASIBLE` pada sesi ini — menjalankan server sungguhan menuntut `dotnet build`/`dotnet run` beserta database yang migration-nya sudah diterapkan (belum terjadi). Khususnya, **perilaku commit/rollback transaksi sungguhan dan penguncian advisory lock PostgreSQL tidak pernah diuji berjalan** — hanya diverifikasi lewat pembacaan struktur kode terhadap pola yang sudah terbukti bekerja di `FinanceReceivableService`.

**Tidak dijalankan:** `dotnet restore`/`dotnet build`/`dotnet run` (permintaan eksplisit); pengujian
otomatis (`rules/backend/TEST_POLICY.md` — tidak diminta eksplisit).

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Batch tidak dapat disetujui bila total sisanya berbeda dari saldo awal Accounting — penolakan menampilkan kedua angka | **Terpenuhi** | `ApproveAsync`, pesan `OpeningItemBatchValidationException` menyebut `totalOutstanding` dan `entity.DeclaredAccountingOpeningAmount` persis |
| Persetujuan melahirkan item dan mutasi pembuka dalam satu transaksi, gagal berarti nol-duanya | **Terpenuhi secara struktur kode** (lihat keterbatasan verifikasi, bagian 5) | `ApproveAsync` — satu `BeginTransactionAsync`, satu `SaveChangesAsync`, satu `CommitAsync`; penanda `committed` mencegah rollback-setelah-commit ganda |
| Item migrasi menerbitkan nol kejadian akuntansi | **Terpenuhi** | Grep `StageEventAsync` — nol kecocokan (bagian 5) |
| Sesudah diposting item mengikuti penagihan, pelunasan, umur piutang, dan snapshot yang normal | **Terpenuhi secara desain** | `FinReceivable`/`FinSupplierPayable` yang dibuat **tidak** punya kolom/flag pembeda selain `OpeningItemBatchId` — seluruh query/layar yang sudah ada memperlakukannya sama seperti baris biasa, tidak ada jalur khusus ditambahkan |
| Batch APPROVED/LOCKED/REJECTED menolak tindakan lanjutan `409` | **Terpenuhi** | `ApproveAsync`/`RejectAsync`/`DeclareAccountingOpeningAsync` seluruhnya memeriksa `entity.Status` terhadap himpunan status yang diizinkan sebelum bertindak |
| `dotnet build` | **Belum terpenuhi** | Sengaja `NOT RUN` atas permintaan eksplisit |
| Nol migration | **Terpenuhi** | Tabel 3.2 |
| Laporan task tracked ada | **Terpenuhi** | Berkas ini |

**Kesimpulan status:** 🟡 **Sebagian**, bukan ✅. Dua hal menahan status ✅: (a) `dotnet build`
sengaja `NOT RUN`, dan (b) perilaku transaksi/lock sungguhan terhadap database PostgreSQL nyata
**belum pernah diuji berjalan** — keduanya butuh lingkungan eksekusi yang sengaja tidak dipakai
sesi ini.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **RISIKO UTAMA** menurut penandaan roadmap sendiri. `ApproveAsync` adalah kode paling kritis pada seluruh `EPIC FIN-24` — melahirkan data finansial sungguhan tanpa pernah dikompilasi atau dijalankan sesi ini. Pemilik modul **MUST** menguji jalur ini di lingkungan staging sebelum dipakai produksi, bukan hanya mempercayai laporan ini |
| Masalah yang diketahui | (1) **Advisory lock dikunci pada Id batch** (`FIN_OPENING_ITEM_BATCH_{id:N}`), **bukan** pada item piutang/utang yang baru lahir — karena item itu belum ada untuk dikunci sebelum transaksi dimulai. Ini konsisten dengan prinsip "kunci agregat yang sedang bertransisi", tetapi berbeda dari pola `DecideAdjustmentAsync` yang mengunci agregat yang DIUBAH (piutang), bukan yang MEMINTA perubahan (adjustment) — perbedaan ini disengaja dan dijelaskan di bagian 3.1, bukan kealpaan. (2) `DueDate`/`SupplierInvoiceDate` pada item yang dibuat memakai fallback `batch.CutoverDate` bila `TryParseDate` gagal — secara teori **tidak pernah tercapai** karena baris sudah lolos re-validasi yang mensyaratkan kedua tanggal itu valid, tetapi kode tetap menyediakan nilai aman (bukan melempar exception tak tertangani) untuk kasus itu |
| Risiko tersisa | (1) **Nol pengujian commit/rollback sungguhan** (bagian 5) — risiko tersisa terbesar pada task ini, mengingat sifatnya yang melahirkan data finansial. (2) `GenerateReceivableNumber`/`GeneratePayableNumber` memakai pola yang sama dengan seluruh generator nomor serumpun (`{PREFIX}-{tanggal}-{GUID}`) — **KNOWN ISSUE** bersama, bukan baru pada task ini. (3) Deskripsi item (`FinReceivableItem.Description`/`FinSupplierPayableItem.Description`) jatuh ke nomor dokumen bila kolom `Keterangan` pada berkas kosong — pilihan wajar mengingat `Description` wajib diisi (`[Required]`) pada kedua model item, tetapi berarti item migrasi tanpa keterangan akan terlihat agak generik pada layar rincian |
| Perubahan sampingan | `NONE` di luar dua perbaikan `BE-FIN-081` yang **disengaja** dan dicatat eksplisit (laporan `BE-FIN-081` bagian 8) |
| Interupsi | `NONE` |
| Status Git | Lihat `git status --short` terkini; akumulasi task-task sebelumnya pada sesi ini tetap belum di-commit, ditambah berkas task ini pada tabel 3.2. Nol `git add`/`git commit`/`git push` dilakukan pada task ini |
| Langkah berikutnya | (1) Pemilik modul memprioritaskan pengujian `ApproveAsync` di staging — ini bagian paling berisiko seluruh `EPIC FIN-24`. (2) Pemilik menjalankan `dotnet build` dan memperbarui status laporan ini ke ✅ bila PASS. (3) `EPIC FIN-24` (`BE-FIN-079`..`082`) kini lengkap untuk jalur CSV — `BE-FIN-083` (pembaca XLSX) tetap `⛔ BLOCKED FIN-OQ-081`, menunggu wewenang paket dan nama/versinya |
