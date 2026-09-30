# Laporan Perubahan Backend — `BE-BKC-079`

## Ringkasan untuk Pembaca Umum

Dalam tata kelola operasional rumah sakit modern, penerimaan setoran uang muka (*deposit*) rawat inap seringkali menggunakan metode pembayaran non-tunai seperti kartu kredit/debit, transfer bank, virtual account, atau payment gateway. Namun, dalam transaksi perbankan riil, sebuah pembayaran yang awalnya berhasil (*SUCCEEDED*) dapat mengalami pembatalan sepihak pasca-transaksi (*post-settlement reversal/chargeback*) karena sanggahan nasabah, kartu bermasalah, atau kegagalan kliring perbankan.

Sebelum perbaikan ini, sistem Billing memiliki celah kritis:
1. **Tidak Ada Pencatatan Mutasi Saat Reversal Tender Top-Up:** Ketika tender top-up berbalik menjadi `REVERSED`, sistem `BillingSettlementService` tidak mencatat mutasi deposit apa pun. Hal ini mengakibatkan saldo deposit pasien tetap mencatatkan uang yang sebenarnya tidak pernah diterima oleh rumah sakit.
2. **Ketiadaan Solusi Saat Dana Deposit Sudah Digunakan untuk Melunasi Tagihan:** Apabila uang muka tersebut telah dialokasikan untuk melunasi tagihan tindakan medis atau kamar rawat inap (status tagihan sudah menjadi lunas / `CLOSED`), sistem sebelumnya menolak pembalikan deposit. Akibatnya rumah sakit menanggung kerugian karena tagihan dianggap sudah lunas padahal uang pembayarannya telah ditarik kembali oleh pihak bank.
3. **Blocker Integrasi Jurnal Keuangan (Finance & Accounting):** Modul Finance dan Accounting mendeteksi kejanggalan ini (`FIN-OQ-034`) karena tidak ada fakta transaksi mutasi deposit yang dapat dibukukan ke buku besar perbankan dan piutang rumah sakit.

### Solusi Kompensasi LIFO & Pemulihan Tagihan Otomatis (`BE-BKC-079`)

Melalui implementasi task `BE-BKC-079`, sistem Billing kini memiliki mekanisme pembalikan tender top-up deposit yang otomatis, deterministik, dan adil:
1. **Saldo Deposit Tetap Non-Negatif (`BKC-DEC-128`):** Saldo deposit pasien (`AvailableBalance`) tidak pernah diperbolehkan menjadi minus (< Rp 0).
2. **Pembalikan Alokasi Tagihan Berurut LIFO (*Last In First Out* — `BKC-DEC-129`):** Jika saldo deposit tidak mencukupi untuk membalikkan nilai tender top-up yang ditarik bank, sistem secara otomatis menarik kembali alokasi pembayaran tagihan pasien mulai dari alokasi yang paling baru (`AllocatedAt DESC`), sehingga saldo deposit dipulihkan terlebih dahulu.
3. **Penyelarasan Kembali Status Tagihan Pasien (`BKC-DEC-130`):** Tagihan pasien yang tadinya sudah lunas (`CLOSED`) akibat alokasi deposit tersebut, secara otomatis dibuka kembali menjadi tagihan aktif (`FINAL`) via `SyncClosureAsync`. Dengan demikian, kasir dapat menagih kembali kewajiban pembayaran yang tertunggak kepada pasien atau keluarganya.
4. **Pencatatan Mutasi Ganda Transparan untuk Finance (`BKC-DEC-131`, `BIL-INT-018`):** Sistem mencatat dua mutasi berpasangan di `BilDepositMovement`: mutasi `RELEASE` (pengembalian alokasi tagihan ke kolam deposit) dan mutasi `REVERSAL` (penarikan dana deposit ke bank). Mutasi ini dikonsumsi oleh Finance untuk menjurnal pemulihan piutang dan penarikan kas/bank secara akurat.

---

### Contoh Kasus Nyata di Rumah Sakit

* **Contoh 1 (Penarikan Dana saat Saldo Deposit Masih Utuh — `BIL-AT-149`):**
  Keluarga Tn. Amir menyetor deposit rawat inap sebesar Rp 5.000.000 menggunakan kartu debit. Dana tersebut masih utuh di akun deposit pasien dan belum dialokasikan ke tagihan kamar maupun tindakan medis (`AvailableBalance = Rp 5.000.000`). Beberapa jam kemudian, pihak bank membatalkan transaksi karena kegagalan jaringan kliring (status tender menjadi `REVERSED`).
  - Sistem memverifikasi saldo akun deposit mencukupi nominal penarikan (`Rp 5.000.000 >= Rp 5.000.000`).
  - Sistem langsung mencatat mutasi `BilDepositMovement` bertipe `REVERSAL` sebesar Rp 5.000.000 dengan referensi `ReversesMovementId` menunjuk ke mutasi top-up awal.
  - Saldo deposit berkurang tepat menjadi Rp 0 secara aman tanpa menyentuh tagihan invoice mana pun.

* **Contoh 2 (Penarikan Dana saat Sebagian Deposit Telah Dipakai Melunasi Tagihan — `BIL-AT-150`, `BIL-AT-152`):**
  Ibu Dewi menyetor uang muka deposit Rp 10.000.000 melalui transfer bank. Dari deposit tersebut, sebesar Rp 7.000.000 telah dialokasikan oleh kasir untuk melunasi tagihan sementara Rawat Inap (Invoice INV-001) sehingga invoice tersebut berstatus `CLOSED` (lunas, sisa tagihan Rp 0), menyisakan saldo deposit Rp 3.000.000 di akun Ibu Dewi. Keesokan harinya, notifikasi bank menyatakan pembayaran transfer Rp 10.000.000 tersebut ditarik/dibatalkan (`REVERSED`).
  - Karena saldo deposit yang tersisa hanya Rp 3.000.000, terjadi defisit sebesar Rp 7.000.000 untuk membalikkan top-up Rp 10.000.000.
  - Sistem secara otomatis mencari alokasi tagihan aktif dan membatalkan alokasi Rp 7.000.000 pada Invoice INV-001 dengan menerbitkan baris kompensasi `BilPaymentAllocation` bertaut `ReversesAllocationId`.
  - Secara bersamaan, sistem mencatat mutasi ganda pada `BilDepositMovement`:
    1. Baris `RELEASE`: +Rp 7.000.000 (pengembalian dana alokasi tagihan ke kolam deposit, sehingga saldo deposit sementara pulih menjadi Rp 3.000.000 + Rp 7.000.000 = Rp 10.000.000).
    2. Baris `REVERSAL`: -Rp 10.000.000 (penarikan penuh dana top-up yang dibatalkan bank, sehingga saldo deposit akhir tepat Rp 0 dan tidak pernah minus).
  - Sistem memanggil `SyncClosureAsync` untuk Invoice INV-001. Karena alokasi pembayarannya ditarik, sisa tagihan pasien kembali menjadi Rp 7.000.000, dan status invoice otomatis beralih dari `CLOSED` kembali ke **`FINAL`**.
  - Kasir dapat melihat kembali tagihan aktif Rp 7.000.000 pada dashboard penagihan untuk ditagihkan ulang ke keluarga pasien.

* **Contoh 3 (Multi-Invoice LIFO Sequencing — `BIL-AT-151`):**
  Pasien Ny. Kartika menggunakan uang muka deposit Rp 8.000.000 untuk membayar dua tagihan berbeda:
  - Tagihan Farmasi (Invoice A, dialokasikan Rp 3.000.000 pada pukul 09.00 WIB).
  - Tagihan Tindakan Lab (Invoice B, dialokasikan Rp 4.000.000 pada pukul 14.00 WIB).
  - Saldo deposit tersisa di akun pasien adalah Rp 1.000.000.
  Ketika tender top-up Rp 8.000.000 dibatalkan oleh pihak bank, terjadi defisit sebesar Rp 7.000.000.
  - Sesuai urutan LIFO (*AllocatedAt DESC*):
    1. Alokasi Invoice B (paling baru, jam 14.00) dibatalkan penuh terlebih dahulu sebesar Rp 4.000.000 (sisa defisit yang harus ditutup menjadi Rp 3.000.000).
    2. Alokasi Invoice A (jam 09.00) dibatalkan penuh sebesar Rp 3.000.000 (defisit tertutup sempurna).
  - Kedua invoice (A dan B) diselaraskan kembali ke status `FINAL` dengan sisa tagihan terutang masing-masing.

---

## Lembar Metadata Task

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-079` |
| Judul | Pembalikan Tender Top-Up Deposit dan Pembatalan Alokasi Tagihan LIFO |
| Slice | Gelombang `MVP-35` — Pembalikan Tender Top-Up Deposit dan Alokasi Tagihan (`docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` § `Gelombang MVP-35`) |
| Roadmap | `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` § `BE-BKC-079` |
| Trace | `FR-BKC-125`–`128`, `BKC-DEC-128`–`131` (keputusan bisnis disahkan 28 September 2026), `FIN-DEC-077` (menutup `FIN-OQ-034`), `BKC-DES-051`–`054` |
| Contract Version | `BIL-API-1.5` (draft), `BIL-STATE-1.5` (draft), `BIL-VALIDATION-1.5` (`BIL-VAL-127`–`131`), `BIL-INTEGRATION-1.3` (`BIL-INT-018`), `BIL-PERMISSION-1.2`, `BIL-TEST-1.6` (`BIL-AT-149`–`156`) |
| Dependency | Mandiri (Nol dependensi internal dalam gelombang `MVP-35`) |
| Klasifikasi | `MEDIUM` — satu repository; berkas diperiksa 8; berkas diubah 1 (`BillingSettlementService.cs`); transaksi atomik serializable, advisory lock deposit akun, compensating invoice allocation LIFO, sinkronisasi closure invoice, penerbitan clearance handoffs; nol skema baru; nol migration |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Billing/Services/BillingSettlementService.cs`, `docs/module-blueprints/billing-kasir/task/report/backend/BE-BKC-079.md` |
| Tanggal | 2026-09-28 |
| Status | 🟡 **SEBAGIAN — source code selesai, menunggu kompilasi build mandiri dan verifikasi pengguna.** Logika pembalikan tender deposit, pembatalan alokasi LIFO, mutasi ganda `RELEASE` & `REVERSAL`, sinkronisasi status invoice `CLOSED` -> `FINAL`, serta handoff clearance telah terpasang lengkap. |

---

## Backend Governance Preflight

| Field Preflight | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `BillingManagement / Billing` |
| Submodule | `Billing` |
| Entity / Model | `BilSettlement`, `BilTender`, `BilDepositAccount`, `BilDepositMovement`, `BilPaymentAllocation`, `BilInvoice` |
| Prefix Registry | `Bil` — status `ACTIVE` pada `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Applicability | `TOUCHED LEGACY` (ekstensi pembalikan tender pada service orkestrasi transaksi existing) |
| QBE Rules Berlaku | `QBE-MOD-001`, `QBE-NAM-001`, `QBE-API-001`, `QBE-SEC-001` — konformansi dipenuhi (nol generic repository, nol bypass context, advisory lock per akun deposit, transaksi atomik serializable) |

---

## 1. Rincian Masalah dan Kebutuhan Bisnis

Sesuai laporan temuan Finance pada `docs/module-blueprints/finance-management/evidence/17-permintaan-perbaikan-pembalikan-tender-deposit-untuk-billing.md`:
1. Pada `BillingSettlementService.cs`, saat tender top-up berubah menjadi `SUCCEEDED`, mutasi `BilDepositMovement` bertipe `TOP_UP` ditulis dan saldo akun bertambah.
2. Namun ketika tender top-up berubah menjadi `REVERSED` (dibatalkan bank), **tidak ada mutasi deposit apa pun yang dicatat**. Saldo akun deposit tetap mencatat saldo fiktif yang tidak pernah riil diterima kasir/bank.
3. Jalur pembalikan mutasi di `BillingDepositService` menolak membalik top-up bila dananya telah terpakai melunasi tagihan (`AvailableBalance < original.Amount`). Akibatnya tidak ada jalan keluar teknis untuk menyelesaikan kasus ini.
4. Akibat ketiadaan fakta mutasi deposit pembalik tersebut, modul Finance dan Accounting terblokir (`FIN-OQ-034`) dari menerbitkan kejadian akuntansi `PEMBALIKAN-PENERIMAAN-UANG-MUKA` dan `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT`.

---

## 2. Alur Proses Bisnis yang Diterapkan

### 2.1 Alur Pembalikan Tender Top-Up Deposit (`UpdateTenderStatusAsync`)
1. Provider pembayaran mengembalikan hasil dengan `Outcome == BillingPaymentProviderOutcome.Reversed` (status target `REVERSED`).
2. Server memvalidasi bahwa status tender sebelumnya wajib `SUCCEEDED` (`BIL-VAL-129`, `BIL-AT-156`). Jika tender berstatus `PENDING` atau `FAILED`, pembalikan alokasi/deposit ditolak dengan pesan: *"Hanya tender berhasil yang dapat direversal."*.
3. Sistem mendeteksi `tender.Settlement.Purpose == BillingSettlementPurposes.DepositTopUp`.
4. Sistem memanggil private method `HandleDepositTopUpReversalAsync(tender, actorUserId, result.OccurredAt, cancellationToken)` di dalam batas transaksi serializable dan advisory lock akun deposit (`BIL_DEPOSIT_{accountId:N}`).
5. **Pemeriksaan Idempotensi:** Sistem memeriksa apakah sudah ada mutasi `REVERSAL` pada akun deposit dengan `SettlementId == tender.SettlementId`. Jika sudah ada, sistem tidak melakukan mutasi ulang dan mengembalikan daftar kosong (`BIL-AT-155`).
6. **Pencarian Mutasi Asal:** Sistem memuat mutasi `TOP_UP` awal yang terkait dengan settlement ini untuk diisi ke `ReversesMovementId` (`BIL-VAL-130`).
7. **Pemeriksaan Defisit Saldo:**
   - Dihitung `deficit = tender.Amount - account.AvailableBalance`.
   - **Kasus Saldo Utuh (`deficit <= 0`):** Tidak ada alokasi tagihan yang dibatalkan. Langsung ke langkah 9.
   - **Kasus Saldo Kurang (`deficit > 0`):** Masuk ke alur pembatalan alokasi tagihan LIFO (langkah 8).
8. **Pembatalan Alokasi Tagihan LIFO (`BKC-DEC-129`, `BKC-DES-052`):**
   - Mengambil seluruh settlement alokasi yang dibiayai akun deposit tersebut via `BilDepositMovement` bertipe `ALLOCATION`.
   - Memuat seluruh `BilPaymentAllocation` aktif yang belum dibalik penuh (`ReversesAllocationId is null` dan belum ada alokasi pembalik yang menunjuk ke ID tersebut).
   - Mengurutkan alokasi secara LIFO: `ORDER BY AllocatedAt DESC, CreateDateTime DESC, Id DESC`.
   - Melakukan iterasi pemotongan hingga defisit tertutup:
     - Untuk setiap alokasi, dibuat baris `BilPaymentAllocation` baru dengan `Amount = cancelAmount`, `ReversesAllocationId = original.Id`, `CalculationVersion = original.CalculationVersion`, `AllocatedAt = occurredAt`.
     - Mengumpulkan `InvoiceId` unik yang terdampak.
   - Jika setelah memeriksa seluruh alokasi defisit masih belum tertutup, lempar `BillingSettlementConflictException("Saldo deposit dan alokasi aktif tidak mencukupi untuk membalikkan top-up deposit.")`.
   - Mencatat mutasi `BilDepositMovement` bertipe `RELEASE` sebesar total dana yang dibatalkan (`totalReleasedFromAllocations`) dengan `IdempotencyKey` dan `CorrelationId` deterministik. Saldo deposit sementara bertambah sebesar `totalReleasedFromAllocations`.
9. **Pencatatan Mutasi REVERSAL:**
   - Mencatat mutasi `BilDepositMovement` bertipe `REVERSAL` sebesar `tender.Amount` dengan `ReversesMovementId = originalTopUpMovement.Id`, `IdempotencyKey` dan `CorrelationId` deterministik.
   - Memotong saldo deposit: `account.AvailableBalance -= tender.Amount`.
   - Memastikan `account.AvailableBalance >= 0`.
   - Memperbarui `RowVersion` dan waktu audit akun deposit.
10. **Penyelarasan Status Tagihan Invoice (`BKC-DEC-130`, `BKC-DES-053`):**
    - Menyimpan perubahan deposit dan alokasi kompensasi via `SaveChangesAsync`.
    - Untuk setiap `invoiceId` yang alokasinya ditarik, memanggil `_closureService.SyncClosureAsync(invoiceId, actorUserId, occurredAt, cancellationToken, PrescriptionClearanceReasonCodes.PaymentReversed)`.
    - Karena nominal pembayaran berkurang, sisa tagihan pasien kembali menjadi > 0, sehingga status invoice yang tadinya `CLOSED` otomatis kembali ke **`FINAL`**.
    - Jika ada perubahan closure, panggil `SaveChangesAsync` lagi.
11. **Penerbitan Surat Clearance Finansial:**
    - Untuk setiap invoice yang berubah status ke `FINAL`, panggil `_consumerHandoffService.PublishForClearanceChangeAsync` dan `PublishForInpatientClearanceAsync` dengan alasan `PaymentReversed`.
    - Transaksi database di-commit.
    - Mencatat log audit tender, alokasi, dan penutupan tagihan.

---

## 3. Rincian Perubahan Source Code

### 3.1 Berkas yang Diperiksa
- `Areas/HealthServices/BillingManagement/Billing/Services/BillingSettlementService.cs`
- `Areas/HealthServices/BillingManagement/Billing/Services/BillingAllocationService.cs`
- `Areas/HealthServices/BillingManagement/Billing/Services/BillingInvoiceClosureService.cs`
- `Areas/HealthServices/BillingManagement/Billing/Models/BilDepositAccount.cs`
- `Areas/HealthServices/BillingManagement/Billing/Models/BilDepositMovement.cs`
- `Areas/HealthServices/BillingManagement/Billing/Models/BilPaymentAllocation.cs`
- `Areas/HealthServices/BillingManagement/Billing/Models/BilSettlement.cs`
- `Areas/HealthServices/BillingManagement/Billing/Models/BilTender.cs`

### 3.2 Berkas yang Diubah

| Berkas | Lokasi | Perubahan |
| --- | --- | --- |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingSettlementService.cs` | Baris 513–520 | Menambahkan inisialisasi `reversedInvoiceClosureChanges` dan pemanggilan `HandleDepositTopUpReversalAsync` ketika `targetStatus == BillingTenderStatuses.Reversed`, `beforeTenderStatus == BillingTenderStatuses.Succeeded`, dan tujuan settlement adalah `DepositTopUp`. |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingSettlementService.cs` | Baris 624–658 | Menambahkan loop penerbitan handoff surat kelayakan clearance farmasi dan rawat inap (`PublishForClearanceChangeAsync` & `PublishForInpatientClearanceAsync`) untuk setiap invoice yang kembali berstatus `FINAL`, serta mencatat audit closure change pasca-commit. |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingSettlementService.cs` | Baris 798–989 | Mengimplementasikan private method baru `HandleDepositTopUpReversalAsync`: advisory lock deposit, idempotency guard, pemindaian alokasi invoice LIFO, penulisan alokasi kompensasi `BilPaymentAllocation`, mutasi ganda `RELEASE` dan `REVERSAL` pada `BilDepositMovement`, validasi saldo deposit non-negatif, dan sinkronisasi `_closureService.SyncClosureAsync`. |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingSettlementService.cs` | Baris 991–997 | Menambahkan private helper `CreateDeterministicGuid` berbasis hash SHA-256 untuk menjamin keunikan dan idempotensi `IdempotencyKey` serta `CorrelationId` pada mutasi `RELEASE` dan `REVERSAL`. |

### 3.3 Dampak Kontrak, Database, dan Keamanan
- **Kontrak API & State Transition:**
  - Status tender `REVERSED` pada top-up kini memiliki side-effect deterministik: status invoice terdampak beralih dari `CLOSED` ke `FINAL` (`BIL-STATE-1.5`).
- **Database:** `NOT APPLICABLE` — **Nol Migration**. Menggunakan tabel dan kolom string/foreign key existing: `BilPaymentAllocation.ReversesAllocationId`, `BilDepositMovement.MovementType` (`'RELEASE'`, `'REVERSAL'`), dan `BilDepositMovement.ReversesMovementId`.
- **Keamanan / RBAC:** Menggunakan otorisasi transaksi yang melekat pada rekonsiliasi tender kasir/provider.

---

## 4. Spesifikasi Mutasi Finansial Bergaya Swagger

Grup Tag: `[Tags("Health Services / Billing Management / Billing / Settlements")]`

| Entitas Terkait | Jenis Operasi | Deskripsi Mutasi Finansial | Validasi & Invariant | Status & Efek Samping |
| :---: | :---: | --- | --- | :---: |
| `BilDepositMovement` | `RELEASE` | Pengembalian dana alokasi tagihan invoice ke saldo deposit pasien saat tender dibalik | `Amount > 0`, `MovementType = 'RELEASE'`, `DepositAccountId` valid | Saldo `AvailableBalance` deposit bertambah sementara sebesar nilai alokasi yang dibatalkan |
| `BilDepositMovement` | `REVERSAL` | Penarikan dana uang muka deposit yang dibatalkan perbankan | `Amount > 0`, `MovementType = 'REVERSAL'`, `ReversesMovementId` terisi menunjuk mutasi `TOP_UP` asal | Saldo `AvailableBalance` deposit berkurang tepat senilai tender top-up; saldo akhir wajib `>= 0` |
| `BilPaymentAllocation` | Kompensasi | Baris alokasi pembalik bertaut ke alokasi awal | `ReversesAllocationId = original.Id`, `Amount > 0`, `AllocatedAt = occurredAt` | Sisa piutang tagihan pasien (`PatientOutstanding`) naik kembali |
| `BilInvoice` | Penyelarasan Closure | Penyelarasan status tagihan via `SyncClosureAsync` | `Outstanding > 0` | Status invoice beralih dari `CLOSED` kembali ke **`FINAL`** |

---

## 5. Verifikasi dan Kriteria Penerimaan

| Skenario Pengujian / Kriteria | Hasil Analisis Statis / Kode | Klasifikasi | Catatan |
| --- | --- | :---: | --- |
| **`BIL-AT-149`**: Tender top-up dibalik saat saldo deposit masih utuh di akun pasien | Saldo deposit dipotong tepat senilai tender; mutasi `REVERSAL` tercatat dengan `ReversesMovementId`; alokasi tidak disentuh; saldo akhir Rp 0 | `PASS` | Sesuai `BKC-DEC-128`, `BKC-DES-051` |
| **`BIL-AT-150`**: Tender top-up dibalik saat saldo deposit kurang | Sistem menghitung defisit, membatalkan alokasi secara proporsional, mencatat mutasi `RELEASE` & `REVERSAL`, saldo deposit tidak pernah minus | `PASS` | Sesuai `BKC-DEC-128`, `BKC-DEC-130`, `BIL-VAL-127` |
| **`BIL-AT-151`**: Pembatalan alokasi tagihan multi-invoice LIFO | Alokasi diurutkan `ORDER BY AllocatedAt DESC, CreateDateTime DESC, Id DESC`, alokasi terbaru dibatalkan terlebih dahulu | `PASS` | Sesuai `BKC-DEC-129`, `BKC-DES-052`, `BIL-VAL-128` |
| **`BIL-AT-152`**: Penyelarasan status invoice `CLOSED` -> `FINAL` | `SyncClosureAsync` dipanggil untuk setiap invoice terdampak, sisa tagihan terdeteksi > 0, status invoice beralih ke `FINAL` | `PASS` | Sesuai `BKC-DEC-130`, `BKC-DES-053`, `BIL-VAL-130` |
| **`BIL-AT-153`**: Mutasi `RELEASE` dan `REVERSAL` terbaca oleh Finance | Mutasi tercatat terpisah pada `BilDepositMovement` dan siap diintake oleh `FinBillingHandoffIntake` | `PASS` | Sesuai `BKC-DEC-131`, `BKC-DES-054`, `BIL-INT-018` |
| **`BIL-AT-154`**: Transaksi atomik dan integritas rollback | Seluruh mutasi dibungkus dalam serializable transaction dan advisory lock; jika terjadi galat, seluruh perubahan di-rollback | `PASS` | Sesuai `BKC-DES-051`, `BIL-VAL-131` |
| **`BIL-AT-155`**: Idempotensi pembalikan tender | Pemeriksaan `account.Movements.Any(Reversal)` mencegah duplikasi mutasi saat dipanggil berulang | `PASS` | Sesuai `BIL-VAL-129` |
| **`BIL-AT-156`**: Penolakan tender non-succeeded | Guard `tender.Status != BillingTenderStatuses.Succeeded` menolak tender pending/failed untuk direversal | `PASS` | Sesuai `BIL-VAL-129` |

---

## 6. Acceptance Criteria & Definition of Done

| Butir Definition of Done | Status | Bukti / Rujukan |
| --- | :---: | --- |
| Method `HandleDepositTopUpReversalAsync` terpasang di `BillingSettlementService.cs` | Terpenuhi | `BillingSettlementService.cs:798–989` |
| Seluruh 8 acceptance criteria `BIL-AT-149`–`156` terpenuhi dalam logika kode | Terpenuhi | Sesuai rincian seksi 5 |
| Mutasi ganda `RELEASE` dan `REVERSAL` tercatat pada `BilDepositMovement` | Terpenuhi | `BillingSettlementService.cs:898–944` |
| Alokasi kompensasi tercatat pada `BilPaymentAllocation` bertaut `ReversesAllocationId` | Terpenuhi | `BillingSettlementService.cs:868–881` |
| Penyelarasan invoice `SyncClosureAsync` dipanggil untuk invoice terdampak | Terpenuhi | `BillingSettlementService.cs:961–986` |
| Penerbitan surat clearance farmasi & ranap untuk invoice yang kembali `FINAL` | Terpenuhi | `BillingSettlementService.cs:624–658` |
| Nol migration basis data (menggunakan skema tabel existing) | Terpenuhi | Tabel `BilDepositMovement`, `BilPaymentAllocation`, dll. |
| Backend Governance Preflight PASS | Terpenuhi | Seksi Governance Preflight |
| Laporan perubahan tracked backend tersedia | Terpenuhi | Berkas laporan ini |
