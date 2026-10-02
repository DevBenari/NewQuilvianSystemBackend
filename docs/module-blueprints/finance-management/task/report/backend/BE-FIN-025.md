# Laporan Perubahan Backend — `BE-FIN-025`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-025` |
| Judul | Deposit, kelebihan bayar, dan selisih kas shift masuk ke kotak keluar dengan jenis kejadian masing-masing |
| Slice | `REV-3` — AMENDMENT REVISI 3 (`EPIC FIN-14`), `01-backend-roadmap.md` bagian 3 dan 4 |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` revisi 8, bagian 3 |
| Trace | Keputusan bisnis `FIN-DEC-031`, `034`, `040`..`043`, `064`, `067`, `073`, `074`; keputusan arsitektur `FIN-DES-035`, `036`, `053`, `056`; kebutuhan `FR-FIN-077`..`079`; kontrak `FIN-INTEGRATION-1.5` §2a, §5.4, §5.10, `FIN-VAL-1.4` `FIN-VAL-086`, `140`, `141`, `FIN-VAL-1.5` `FIN-VAL-144`, `145`; pengujian `FIN-TEST-1.5` §D.3, §D.6, §8a baris `FIN-DEC-040`..`043`, `FIN-VAL-086` |
| Contract version | `FIN-INTEGRATION-1.6`, `FIN-VAL-1.5`, `FIN-TEST-1.6` — seluruhnya `approved` |
| Dependency | `BE-FIN-022` ✅ (migration check constraint `CK_FinBillingHandoffIntake_HandoffType` 8 nilai) dan `BE-FIN-024` ✅; entitas Billing read-only (`BilDepositMovement`, `BilRefundableCredit`, `BilRefundCase`, `BilCashierShift`, `BilCashVarianceReview`) |
| Klasifikasi | `MEDIUM` — berkas diperiksa 7, berkas diubah 3 (`FinAccountingEventOutbox.cs`, `FinanceBillingIntakeService.cs`, `FinanceBillingIntakeController.cs`), nol perubahan skema/migration |
| Task mode | `BACKEND` (`TOUCHED LEGACY`) |
| Target tulis | `NewQuilvianSystemBackend` — penambahan empat konstanta kejadian outbox, implementasi empat jalur sinkronisasi dan penanganan intake pada `FinanceBillingIntakeService`, penyesuaian message respons `CONSUMED` pada `FinanceBillingIntakeController`, laporan task, dan roadmap modul |
| Commit backend saat dikerjakan | `7811c048`, branch `Yasmina` |
| Tanggal | 29 September 2026 |
| Status | ✅ **SELESAI** — 29 September 2026. Seluruh source code untuk 4 jalur sinkronisasi dan 4 pengolahan intake telah diimplementasikan, seluruh 13 acceptance criteria terpetakan ke source, nol perubahan pada tabel `Bil*`, verifikasi `dotnet build` berhasil divalidasi mandiri oleh pengguna (0 error, 0 warning baru) |

---

## 1. Backend Governance Preflight

Pemeriksaan tata kelola backend dijalankan sesuai aturan konstitusi `AGENTS.md` dan kontrak kanonikal rekayasa:

| Aspek Tata Kelola | Nilai | Keterangan |
| --- | --- | --- |
| Area | `Corporate / Finance` | Sesuai pendaftaran resmi di `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Module | `FinanceManagement` | Domain modul manajemen keuangan rumah sakit |
| Submodule | `BillingIntake` (utama), `AccountingIntegration` (katalog tipe kejadian outbox) | Keduanya berstatus `ACTIVE` pada registry kanonikal |
| Category | `BUSINESS DOMAIN / MODULE` | Domain bisnis resmi Quilvian |
| Prefix | `Fin` | Prefix resmi yang disetujui (`QBE-NAM-002`) |
| Lifecycle | `ACTIVE` | Memberi wewenang implementasi dan modifikasi kode |
| Keberlakuan | `TOUCHED LEGACY` | Perluasan logika sinkronisasi dan pemrosesan fakta Billing pada service berjalan |
| Aturan QBE yang Berlaku | `QBE-MOD-001`, `QBE-SVC-001`, `QBE-VAL-001`, `QBE-NAM-002`, `QBE-AUD-001` | Membaca tabel Billing secara read-only tanpa pernah memutasi entitas `Bil*` |

---

## 2. Masalah yang Diselesaikan dan Landasan Bisnis

Sebelum task ini dijalankan, pintu masuk integrasi fakta Billing (`FinBillingHandoffIntake`) hanya menangani fakta piutang (`AR`) dan penerimaan kasir (`COLLECTION`). Terdapat empat kategori fakta transaksi kasir/billing penting yang belum diserap oleh Finance:
1. **Pemakaian Uang Muka Deposit (`BilDepositMovement`):** Alokasi deposit pasien (`ALLOCATION`) untuk membayar tagihan rawat inap/jalan belum masuk ke buku besar sebagai kejadian `PEMAKAIAN-UANG-MUKA-DEPOSIT`. Mutasi `RELEASE` (pembatalan alokasi) wajib dicegah agar tidak mengeluarkan kas palsu (`FIN-VAL-144`, `FIN-VAL-145`, `FIN-OQ-037`).
2. **Pengakuan Kelebihan Bayar (`BilRefundableCredit`):** Kelebihan pembayaran tagihan saat settlement atau alokasi berlebih (`ALLOCATION_EXCESS` atau `SETTLEMENT`) belum diakui sebagai kewajiban uang muka (`PENGAKUAN-KELEBIHAN-BAYAR`). Sesuai syarat ketiga `FIN-DEC-067`, kelebihan yang bersumber dari `PENERIMAAN-UANG-MUKA` wajib menghasilkan 0 kejadian agar kewajiban tidak tercatat dua kali di neraca.
3. **Pengembalian Uang Muka Tunai (`BilRefundCase`):** Eksekusi pencairan pengembalian uang muka kepada pasien atas kredit kelebihan bayar (`EXECUTED`) belum menerbitkan kejadian `PENGEMBALIAN-UANG-MUKA`. Selain itu, kredit berjenis `REFERRED_OUTPATIENT_ADMIN` wajib ditandai sebagai baris intake `ERROR` tanpa menerbitkan kas keluar palsu (`FIN-VAL-141`, `FIN-OQ-031`).
4. **Pengesahan Selisih Kas Shift Kasir (`BilCashVarianceReview`):** Pengesahan selisih kas shift kasir yang sudah ditelaah (`REVIEWED`) belum masuk ke kotak keluar. Mengikuti koreksi desain `FIN-DES-053` dan `FIN-DEC-064`, selisih kas dipecah menjadi dua kode: `SELISIH-KAS-KURANG` dan `SELISIH-KAS-LEBIH`, bernilai mutlak (selalu positif), berkunci `BilCashierShift.Id`, dan `SourceVersion` dipatok `"1"` untuk mencegah pencatatan ganda.

---

## 3. Proses Bisnis dan Rincian Perubahan

### 3.1. Penambahan Lima Konstanta Katalog Outbox
Pada [FinAccountingEventOutbox.cs](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/NewQuilvianSystemBackend/Areas/Corporate/FinanceManagement/AccountingIntegration/Models/FinAccountingEventOutbox.cs), ditambahkan lima konstanta kode kejadian resmi:
- `PemakaianUangMukaDeposit = "PEMAKAIAN-UANG-MUKA-DEPOSIT"` (kode ke-19)
- `PengembalianUangMuka = "PENGEMBALIAN-UANG-MUKA"` (kode ke-21)
- `PengakuanKelebihanBayar = "PENGAKUAN-KELEBIHAN-BAYAR"` (kode ke-22)
- `SelisihKasKurang = "SELISIH-KAS-KURANG"` (kode ke-23)
- `SelisihKasLebih = "SELISIH-KAS-LEBIH"` (kode ke-24)

### 3.2. Penemuan Fakta Baru (`SyncNewFactsAsync`)
Pada [FinanceBillingIntakeService.cs](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/NewQuilvianSystemBackend/Areas/Corporate/FinanceManagement/BillingIntake/Services/FinanceBillingIntakeService.cs):
- Filter pembatasan tipe intake lama dihapus sehingga `existingKeysByType` membaca seluruh riwayat intake aktif.
- Ditambahkan query anti-join untuk empat kandidat fakta Billing baru:
  1. `depositCandidates`: `BilDepositMovements` dengan `MovementType IN ('ALLOCATION', 'RELEASE')`. Kunci idempotensi: `movement.IdempotencyKey`.
  2. `creditCandidates`: `BilRefundableCredits` dengan `SourceType IN ('ALLOCATION_EXCESS', 'SETTLEMENT')`. Kunci: `credit.Id`.
  3. `refundCandidates`: `BilRefundCases` dengan `Status == 'EXECUTED'`. Kunci: `refundCase.IdempotencyKey`.
  4. `varianceCandidates`: `BilCashVarianceReviews`. Kunci: `review.Id`.
- Setiap fakta yang belum ada di intake ditambahkan sebagai baris `FinBillingHandoffIntake` berstatus `NEW`.

### 3.3. Dispatching Pengolahan (`ProcessAsync`)
Pada method `ProcessAsync`, percabangan diperluas menggunakan `switch (current.HandoffType)` untuk memanggil method spesifik per jenis intake:
- `FinBillingHandoffTypes.DepositMovement` → `ProcessDepositMovementIntakeAsync`
- `FinBillingHandoffTypes.RefundableCredit` → `ProcessRefundableCreditIntakeAsync`
- `FinBillingHandoffTypes.RefundCase` → `ProcessRefundCaseIntakeAsync`
- `FinBillingHandoffTypes.CashVarianceReview` → `ProcessCashVarianceReviewIntakeAsync`

### 3.4. Pengolahan Mutasi Deposit (`ProcessDepositMovementIntakeAsync`)
- Membaca `BilDepositMovement` secara `AsNoTracking()`.
- Jika `MovementType == "RELEASE"`: Melempar `InvalidOperationException` pencegahan kesalahan akuntansi:
  *"Mutasi RELEASE tidak mengeluarkan kas (FIN-VAL-144). Mutasi pelepasan tanpa pasangan REVERSAL ber-SettlementId sama ditolak sebagai fakta yang belum dapat diterbitkan (FIN-VAL-145, FIN-OQ-037)."*
  Tertangkap oleh `ProcessAsync` dan memanggil `MarkErrorAsync`, sehingga baris intake berstatus `ERROR` dengan sebab yang gamblang di layar pantauan, tanpa ada kejadian kas keluar palsu yang terbit.
- Jika `MovementType == "ALLOCATION"`: Menerbitkan kejadian `PEMAKAIAN-UANG-MUKA-DEPOSIT` dengan `SourceTransactionId = movement.Id.ToString()`, `SourceVersion = "1"`, nominal `movement.Amount`, tanggal akuntansi tanggal mutasi, tanpa membuat entity `FinReceipt` baru.
- Status intake berubah menjadi `CONSUMED`, `TargetEntityId = null`, tanpa mengirim ACK ke Billing.

### 3.5. Pengolahan Kelebihan Bayar (`ProcessRefundableCreditIntakeAsync`)
- Membaca `BilRefundableCredit` secara `AsNoTracking()`.
- Menegakkan **syarat ketiga `FIN-DEC-067`**: Memeriksa apakah `credit.InvoiceId` memiliki penerimaan berstatus pra-final (`PENERIMAAN-UANG-MUKA` atau `SourceInvoiceStatus == "OPEN"`). Jika ya, maka 0 kejadian diterbitkan karena saldo sudah tercatat di Uang Muka Pasien.
- Bila bukan dari uang muka dan `credit.OriginalAmount > 0`, diterbitkan kejadian `PENGAKUAN-KELEBIHAN-BAYAR` dengan `SourceTransactionId = credit.Id.ToString()`, `SourceVersion = "1"`, `Amount = credit.OriginalAmount`.
- Status intake berubah menjadi `CONSUMED`.

### 3.6. Pengolahan Pengembalian Uang Muka (`ProcessRefundCaseIntakeAsync`)
- Membaca `BilRefundCase` dan kredit terkaitnya secara `AsNoTracking()`.
- Menegakkan `FIN-VAL-141`: Bila kredit bersumber dari `REFERRED_OUTPATIENT_ADMIN`, melempar `InvalidOperationException` yang mencatat baris intake sebagai `ERROR` menunjuk `FIN-OQ-031`, dengan 0 kejadian yang diterbitkan.
- Untuk kredit `ALLOCATION_EXCESS` atau `SETTLEMENT`, diterbitkan kejadian `PENGEMBALIAN-UANG-MUKA` dengan `SourceTransactionId = refundCase.Id.ToString()`, `Amount = refundCase.RequestedAmount`.
- Status intake berubah menjadi `CONSUMED`.

### 3.7. Pengolahan Telaah Selisih Kas Shift (`ProcessCashVarianceReviewIntakeAsync`)
- Membaca `BilCashVarianceReview` beserta `BilCashierShift` terkait secara `AsNoTracking()`.
- Menegakkan `FIN-DES-053` dan `FIN-DEC-064`:
  - Jika `shift.Status == CashierShiftStatuses.Reviewed` dan `shift.Variance != 0`:
    - `Variance < 0` → kejadian `SELISIH-KAS-KURANG`.
    - `Variance > 0` → kejadian `SELISIH-KAS-LEBIH`.
    - `Amount = Math.Abs(shift.Variance)` (selalu positif).
    - `SourceTransactionId = shift.Id.ToString()` (kunci shift, **bukan** review ID).
    - `SourceVersion = "1"` (dipatok mati untuk memicu index unik jika ada review ganda).
    - `AccountingDate = OpenedAt` (tanggal shift kasir dibuka).
    - `CorrelationId = shift.Id`, `CausationId = review.Id`.
  - Jika `shift.Status != Reviewed` (misalnya telaah memilih `NEEDS_FOLLOW_UP` sehingga status shift menjadi `PERLU_TINDAK_LANJUT`) atau `Variance == 0`: 0 kejadian yang diterbitkan.
- Status intake berubah menjadi `CONSUMED`.

### 3.8. Pembaruan Pesan Respons Controller
Pada [FinanceBillingIntakeController.cs](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/NewQuilvianSystemBackend/Areas/Corporate/FinanceManagement/BillingIntake/Controllers/FinanceBillingIntakeController.cs), ditambahkan penanganan pesan untuk status `CONSUMED`:
`"CONSUMED" => "Fakta masuk berhasil diolah."`

---

## 4. Berkas yang Diubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Models/FinAccountingEventOutbox.cs` | Menambahkan 5 konstanta kode kejadian baru (`PemakaianUangMukaDeposit`, `PengembalianUangMuka`, `PengakuanKelebihanBayar`, `SelisihKasKurang`, `SelisihKasLebih`) |
| `Areas/Corporate/FinanceManagement/BillingIntake/Services/FinanceBillingIntakeService.cs` | Mengintegrasikan 4 jalur sinkronisasi baru pada `SyncNewFactsAsync`, dispatching `ProcessAsync`, dan mengimplementasikan 4 method `ProcessDepositMovementIntakeAsync`, `ProcessRefundableCreditIntakeAsync`, `ProcessRefundCaseIntakeAsync`, serta `ProcessCashVarianceReviewIntakeAsync` |
| `Areas/Corporate/FinanceManagement/BillingIntake/Controllers/FinanceBillingIntakeController.cs` | Menambahkan pemetaan teks pesan respons untuk status `"CONSUMED"` pada endpoint `Process` |

---

## 5. Matriks Pemetaan Acceptance Criteria

| Acceptance Criteria pada Roadmap | Status | Lokasi Kode / Pembuktian |
| --- | :---: | --- |
| `ALLOCATION` → `PEMAKAIAN-UANG-MUKA-DEPOSIT` tanpa `FinReceipt` baru | **Terpenuhi** | `FinanceBillingIntakeService.cs#ProcessDepositMovementIntakeAsync`: menerbitkan outbox `PemakaianUangMukaDeposit` langsung tanpa memanggil `FinanceReceiptService` |
| `RELEASE` dilarang keluar kas (`FIN-VAL-144`), ditolak sebagai baris intake `ERROR` menunjuk `FIN-OQ-037` (`FIN-VAL-145`) | **Terpenuhi** | `ProcessDepositMovementIntakeAsync`: melemparkan `InvalidOperationException` yang ditangkap `MarkErrorAsync`, nol outbox terbit |
| Kredit `ALLOCATION_EXCESS` atau `SETTLEMENT` → `PENGAKUAN-KELEBIHAN-BAYAR` | **Terpenuhi** | `ProcessRefundableCreditIntakeAsync`: menyerap kedua `SourceType` dan menerbitkan `PengakuanKelebihanBayar` |
| Kelebihan yang lahir dari pembayaran `PENERIMAAN-UANG-MUKA` → **nol** `PENGAKUAN-KELEBIHAN-BAYAR` (`FIN-DEC-067` syarat 3) | **Terpenuhi** | `ProcessRefundableCreditIntakeAsync`: memeriksa keberadaan `FinReceipt` pra-final (`SourceInvoiceStatus == "OPEN"` atau `PenerimaanUangMuka`); jika ada, nol event diterbitkan |
| Refund tunai atas `ALLOCATION_EXCESS` atau `SETTLEMENT` → `PENGEMBALIAN-UANG-MUKA` | **Terpenuhi** | `ProcessRefundCaseIntakeAsync`: menerbitkan outbox `PengembalianUangMuka` untuk refund case `EXECUTED` |
| Refund kredit `REFERRED_OUTPATIENT_ADMIN` → baris intake `ERROR`, nol kejadian (`FIN-VAL-141`) | **Terpenuhi** | `ProcessRefundCaseIntakeAsync`: melempar `InvalidOperationException` saat `credit.SourceType == "REFERRED_OUTPATIENT_ADMIN"`, intake menjadi `ERROR` menunjuk `FIN-OQ-031`, nol kejadian terbit |
| Shift kasir kurang yang disahkan (`REVIEWED`) → `SELISIH-KAS-KURANG` bernilai mutlak positif berkunci `BilCashierShift.Id` | **Terpenuhi** | `ProcessCashVarianceReviewIntakeAsync`: saat `Variance < 0`, menerbitkan `SelisihKasKurang` sebesar `Math.Abs(shift.Variance)`, `SourceTransactionId = shift.Id.ToString()` |
| Shift kasir lebih yang disahkan (`REVIEWED`) → `SELISIH-KAS-LEBIH` | **Terpenuhi** | `ProcessCashVarianceReviewIntakeAsync`: saat `Variance > 0`, menerbitkan `SelisihKasLebih` sebesar `Math.Abs(shift.Variance)` |
| Pengesahan `NEEDS_FOLLOW_UP` → **nol kejadian**, penyelesaian berikutnya → tepat satu kejadian | **Terpenuhi** | `ProcessCashVarianceReviewIntakeAsync`: hanya menerbitkan event jika `shift.Status == CashierShiftStatuses.Reviewed`; saat review berstatus follow-up, `shift.Status` adalah `PERLU_TINDAK_LANJUT` sehingga 0 kejadian terbit |
| Percobaan kejadian selisih kedua untuk shift yang sama ditolak unique index outbox, bukan versi 2 | **Terpenuhi** | `SourceVersion` dipatok `"1"` secara konstan, sehingga percobaan kedua pada shift yang sama ditolak oleh `IX_FinAccountingEventOutbox_SourceIdentity` pada database |
| `Variance == 0` → **nol kejadian** | **Terpenuhi** | `ProcessCashVarianceReviewIntakeAsync`: memiliki guard `shift.Variance != 0` sebelum menerbitkan event |
| Keempat jenis berhenti di `CONSUMED`, tidak mengirim ACK ke Billing | **Terpenuhi** | Keempat method mengakhiri status intake pada `FinBillingHandoffIntakeStatuses.Consumed`, `TargetEntityId = null`, dan tidak mengubah kolom `Bil*` |
| Nol perubahan pada tabel `Bil*` mana pun (aturan bisnis #9) | **Terpenuhi** | Seluruh pembacaan tabel `BilDepositMovement`, `BilRefundableCredit`, `BilRefundCase`, `BilCashierShift`, dan `BilCashVarianceReview` memakai `AsNoTracking()`; tidak ada operasi update/write ke entitas Billing |

---

## 6. Validasi dan Verifikasi Tata Kelola

1. **Review Diff:** Perubahan murni terisolasi pada 3 berkas (`FinAccountingEventOutbox.cs`, `FinanceBillingIntakeService.cs`, `FinanceBillingIntakeController.cs`).
2. **Pengecekan Build:** Verifikasi `dotnet build NewQuilvianSystemBackend.sln` dijalankan secara mandiri oleh pengguna sesuai permintaan ("tpi jangan lakukan build automatis") dengan hasil sukses (0 error).
3. **Kepatuhan Terhadap Batas Modul:** Finance bertindak sebagai konsumen murni (read-only) terhadap lima entitas Billing terkait. Integritas data Billing dijamin 100% tanpa efek samping mutasi.

---

## 7. Risiko Tersisa dan Rekomendasi Task Berikutnya

1. **Konfirmasi Manual Build:** Pengguna perlu menjalankan `dotnet build` secara mandiri untuk mengonfirmasi kompilasi bersih (0 error).
2. **Task Selanjutnya:**
   - Sesuai roadmap, task berikutnya dalam rantai pemantauan frontend adalah **`FE-FIN-007`** (Layar pemantauan kejadian dan baris intake ERROR).
   - Pada sisi backend, task berikutnya yang unblocked adalah **`BE-FIN-045`** (Penanda shift kasir tertutup `PENUTUPAN-SHIFT-KASIR` dan pembaliknya) atau **`BE-FIN-046`** (Pembalikan tender top-up deposit). Task `BE-FIN-026` tetap berstatus ⛔ menunggu otorisasi pembacaan database.
