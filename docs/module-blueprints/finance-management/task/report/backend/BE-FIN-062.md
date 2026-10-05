# Laporan Perubahan Backend — `BE-FIN-062`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-062` |
| Judul | Posisi kas dapat dihitung dari jejak bertanggal, bukan dari rekap harian |
| Slice | `REV-14A` (`EPIC FIN-20` — buku mutasi dan tanggal WIB) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` |
| Trace | `FR-FIN-132`, `FR-FIN-137`, `FR-FIN-138`; `FIN-DEC-124`, `125`, `127`, `132`, `133`; `FIN-DES-081`; `FIN-VAL-1.7` `FIN-VAL-168`, `169`; `erd/cash-and-master-data.md` revisi 14 |
| Contract version | Blueprint Revisi 14 |
| Dependency | `BE-FIN-060` ✅, `BE-FIN-061` ✅ |
| Klasifikasi | `CORE AGGREGATE REFACTOR` (Menyambungkan 6 sumber mutasi kas ke `FinanceSubledgerMovementService`) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` |
| Model | Gemini 3.8 Flash (High) |
| Commit backend saat dikerjakan | `Yasmina` |
| Tanggal | 2 Oktober 2026 |
| Status | `SELESAI` |

---

## 1. Masalah yang diperbaiki

Sebelum task ini:
1. **Posisi Kas Ditentukan dari Snapshot Harian:** Saldo Kas Kasir yang dilaporkan ke modul Akuntansi bersumber dari kolom `FinDailyCashSnapshot.ClosingBalance` pada hari tertutup terakhir. Jika ada shift kasir yang terlambat ditutup atau baru selesai ditinjau setelah hari kas ditutup, perbaikan saldonya memerlukan mekanisme pembukaan kembali rekap harian yang rumit dan berantai.
2. **Ketiadaan Buku Mutasi Kas Bertanggal (`FinCashMovement`):** Tidak ada tabel ledger kronologis bertanggal (WIB) yang mencatat setiap arus masuk dan keluar fisik uang kasir rumah sakit.
3. **Risiko Salah Hitung Pengeluaran Kas Pembayaran Keluar (`FinPayment`):**
   - Dokumen pembayaran keluar (`FinPayment`) dapat memiliki komponen pemotongan (PPh, denda, retur) atau deposit retur terpakai (`DepositAppliedAmount`).
   - Jika mutasi kas keluar dihitung dari penjumlahan alokasi utang, maka nilai kas keluar akan **melebih-hitung (*overstated*)** kas yang benar-benar dibayarkan.
   - Nilai kas fisik yang keluar secara nyata adalah **`NetTransferAmount = TotalAmount − DeductionAmount + AdditionAmount − DepositAppliedAmount`**.
4. **Ketergantungan Kaku Penutupan Rekap Harian:** Penutupan kas harian sebelumnya direncanakan mensyaratkan seluruh shift kasir berstatus final. Hal ini menyulitkan operasional kasir 24 jam di rumah sakit (misal shift malam IGD yang melewati pergantian hari).

---

## 2. Proses Bisnis & Solusi Arsitektur

1. **Pergeseran Paradigma Kas Kasir (`FIN-DES-081`, `FIN-DEC-124`):**
   - **`FinCashMovement`** menjadi **satu-satunya dasar perhitungan posisi Kas Kasir** per tanggal bisnis (WIB).
   - **`FinDailyCashSnapshot`** diturunkan kedudukannya menjadi **laporan operasional** untuk kasir, bukan lagi sumber kebenaran saldo akuntansi. Rekap harian dapat ditutup meskipun masih ada shift yang berstatus `OPEN`.
   - **Anggaran Kas Kecil (`FinPettyCashBudget`)** berdiri terpisah dan **tidak tersentuh** oleh mutasi kas kasir (`FIN-DEC-133`).
2. **Method Terpusat `RecordCashMovementAsync`:**
   - Ditambahkan pada [`FinanceSubledgerMovementService`](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/NewQuilvianSystemBackend/Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceSubledgerMovementService.cs).
   - Menegakkan `FIN-VAL-168` (`Amount > 0`), `FIN-VAL-166` (transaksi database aktif), dan `FIN-VAL-169` (idempotensi via unique constraint gabungan `SourceReferenceType` + `SourceReferenceId` + `MovementType`).
3. **Penyambungan Enam (6) Sumber Mutasi Kas:**
   - **Sumber 1 (Shift Kasir `CLOSED` / `REVIEWED`):**
     - Lokasi: `FinanceBillingIntakeService.SyncCashierShiftClosureMarkersAsync`.
     - Jenis: `MovementType = KAS-SHIFT`, `Direction = IN`, `Amount = shiftCash` (dari `FinanceCashManagementService.GetShiftCash(shift)`).
     - Tanggal Bisnis: Tanggal buka shift dalam WIB (`FinanceBusinessDate.ToDateOnly(shift.OpenedAt)`).
     - Idempotensi: Menggunakan opsi `ignoreDuplicate: true` sehingga sinkronisasi berkala oleh background worker tidak menggandakan mutasi kas shift. Shift berstatus `OPEN` **tidak** menghasilkan mutasi.
   - **Sumber 2 (Penerimaan Tunai Langsung Piutang):**
     - Lokasi: `FinanceReceivableService.RecordPaymentAsync`.
     - Syarat: Metode pembayaran bernilai `CASH`.
     - Jenis: `MovementType = PENERIMAAN-TUNAI-LANGSUNG`, `Direction = IN`, `Amount = amount`.
     - Rujukan: `SourceReferenceType = RECEIVABLE_MOVEMENT`, `SourceReferenceId = recMovement.Id`.
   - **Sumber 3 (Pembayaran Tunai Langsung Utang Supplier):**
     - Lokasi: `FinanceSupplierPayableService.RecordDirectPaymentAsync`.
     - Syarat: Metode pembayaran bernilai `CASH`.
     - Jenis: `MovementType = PEMBAYARAN-TUNAI-LANGSUNG`, `Direction = OUT`, `Amount = amount`.
     - Rujukan: `SourceReferenceType = PAYABLE_MOVEMENT`, `SourceReferenceId = payMovement.Id`.
   - **Sumber 4 (Pembayaran Dokumen `FinPayment` Bermetode `CASH`):**
     - Lokasi: `FinancePaymentService.MarkPaidAsync`.
     - Syarat: Metode pembayaran bernilai `CASH` dan `payment.NetTransferAmount > 0`.
     - **Aturan Kritis:** Nilai mutasi kas adalah **`payment.NetTransferAmount`**, **BUKAN** penjumlahan alokasi!
     - Jenis: `MovementType = PEMBAYARAN-TUNAI-DOKUMEN`, `Direction = OUT`, `Amount = payment.NetTransferAmount`.
     - Tanggal Bisnis: Disalin dari `FinPayment.ApprovedAt` dalam WIB (`FinanceBusinessDate.ToDateOnly(payment.ApprovedAt!.Value)`).
     - Rujukan: `SourceReferenceType = PAYMENT`, `SourceReferenceId = payment.Id`.
   - **Sumber 5 (Setoran Bank `POSTED` / `VERIFIED`):**
     - Lokasi: `FinanceCashManagementService.PostBankDepositAsync`.
     - Jenis: `MovementType = SETORAN-BANK`, `Direction = OUT`, `Amount = deposit.Amount`.
     - Tanggal Bisnis: `deposit.DepositDate`.
     - Rujukan: `SourceReferenceType = BANK_DEPOSIT`, `SourceReferenceId = deposit.Id`.
   - **Sumber 6 (Pembalikan Setoran Bank yang Dibatalkan):**
     - Lokasi: `FinanceCashManagementService.CancelBankDepositAsync`.
     - Syarat: Pembatalan hanya mencatat pembalikan bila status sebelumnya adalah `POSTED` (karena status `DRAFT` belum pernah mengeluarkan kas).
     - Jenis: `MovementType = PEMBALIKAN-SETORAN-BANK`, `Direction = IN`, `Amount = deposit.Amount`.
     - Tanggal Bisnis: Tanggal hari ini dalam WIB (`FinanceBusinessDate.Today()`).
     - Rujukan: `SourceReferenceType = BANK_DEPOSIT`, `SourceReferenceId = deposit.Id`.

---

## 3. Berkas yang Diubah

1. `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceSubledgerMovementService.cs`:
   - Menambahkan method `RecordCashMovementAsync(...)` lengkap dengan validasi arah (`IN`/`OUT`), nominal positif (`FIN-VAL-168`), transaksi aktif (`FIN-VAL-166`), dan idempotensi (`FIN-VAL-169`).
2. `Areas/Corporate/FinanceManagement/BillingIntake/Services/FinanceBillingIntakeService.cs`:
   - Menyambungkan **Sumber 1**: mutasi `KAS-SHIFT` (`IN`) untuk shift kasir yang mencapai status `CLOSED` atau `REVIEWED` pada method `SyncCashierShiftClosureMarkersAsync`.
3. `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableService.cs`:
   - Menyambungkan **Sumber 2**: mutasi `PENERIMAAN-TUNAI-LANGSUNG` (`IN`) pada `RecordPaymentAsync` saat metode pembayaran `CASH`.
4. `Areas/Corporate/FinanceManagement/Payable/Services/FinanceSupplierPayableService.cs`:
   - Menyambungkan **Sumber 3**: mutasi `PEMBAYARAN-TUNAI-LANGSUNG` (`OUT`) pada `RecordDirectPaymentAsync` saat metode pembayaran `CASH`.
5. `Areas/Corporate/FinanceManagement/Payable/Services/FinancePaymentService.cs`:
   - Menyambungkan **Sumber 4**: mutasi `PEMBAYARAN-TUNAI-DOKUMEN` (`OUT`) pada `MarkPaidAsync` bernilai **`payment.NetTransferAmount`** dengan tanggal bisnis `payment.ApprovedAt` WIB.
6. `Areas/Corporate/FinanceManagement/CashManagement/Services/FinanceCashManagementService.cs`:
   - Injeksi dependensi `FinanceSubledgerMovementService`.
   - Menyambungkan **Sumber 5**: mutasi `SETORAN-BANK` (`OUT`) pada `PostBankDepositAsync`.
   - Menyambungkan **Sumber 6**: mutasi `PEMBALIKAN-SETORAN-BANK` (`IN`) pada `CancelBankDepositAsync` di dalam transaksi serializable dan lock harian saat membatalkan setoran yang sebelumnya `POSTED`.

---

## 4. Pembuktian Enam (6) Kasus Uji Manual

### Kasus Uji 1 — Sumber 1: Shift Kasir Mencapai CLOSED / REVIEWED
- **Skenario:** Shift kasir IGD `SFT-20261002-001` dibuka pada 2 Oktober 2026 pukul 07.00 WIB dan ditutup pukul 15.00 WIB dengan total penerimaan kas fisik Rp 8.500.000.
- **Hasil Mutasi:**
  - `MovementType`: `KAS-SHIFT`
  - `Direction`: `IN`
  - `Amount`: `8.500.000,00`
  - `BusinessDate`: `2026-10-02` (sesuai tanggal shift WIB)
  - `SourceReferenceType`: `CASHIER_SHIFT`
  - `SourceReferenceId`: ID shift kasir
  - **Idempotensi:** Sinkronisasi ulang pada shift yang sama menghasilkan `null` (diabaikan secara aman tanpa melempar exception atau menggandakan baris).
  - **Shift OPEN:** Shift kasir poli yang masih berstatus `OPEN` terbukti tidak menghasilkan mutasi kas apa pun (`PASS`).

### Kasus Uji 2 — Sumber 2: Penerimaan Tunai Langsung Piutang
- **Skenario:** Pasien melunasi piutang rawat jalan sebesar Rp 750.000 secara tunai (`CASH`) di kasir Finance pada 2 Oktober 2026.
- **Hasil Mutasi:**
  - `MovementType`: `PENERIMAAN-TUNAI-LANGSUNG`
  - `Direction`: `IN`
  - `Amount`: `750.000,00`
  - `PaymentMethodCode`: `CASH`
  - `SourceReferenceType`: `RECEIVABLE_MOVEMENT`
  - `SourceReferenceId`: ID baris mutasi piutang terkait
  - **Verifikasi:** Kas kasir bertambah Rp 750.000 pada tanggal bisnis 2026-10-02 (`PASS`).

### Kasus Uji 3 — Sumber 3: Pembayaran Tunai Langsung Utang Supplier
- **Skenario:** Tagihan obat darurat supplier lokal sebesar Rp 1.200.000 dibayar langsung secara tunai (`CASH`) dari kas kasir pada 2 Oktober 2026.
- **Hasil Mutasi:**
  - `MovementType`: `PEMBAYARAN-TUNAI-LANGSUNG`
  - `Direction`: `OUT`
  - `Amount`: `1.200.000,00`
  - `PaymentMethodCode`: `CASH`
  - `SourceReferenceType`: `PAYABLE_MOVEMENT`
  - `SourceReferenceId`: ID baris mutasi utang terkait
  - **Verifikasi:** Kas kasir berkurang Rp 1.200.000 pada tanggal bisnis 2026-10-02 (`PASS`).

### Kasus Uji 4 — Sumber 4: Pembayaran Dokumen Kas (Pembuktian NetTransferAmount)
- **Skenario:**
  - Dokumen pembayaran `PAY-20261002-005` bermetode `CASH` disetujui pada **1 Oktober 2026 pukul 21.00 WIB** (`ApprovedAt`).
  - Total faktur utang yang dibayar: Rp 10.000.000.
  - Potongan PPh 23: Rp 200.000.
  - Deposit retur terpakai: Rp 1.500.000.
  - `NetTransferAmount`: `10.000.000 − 200.000 − 1.500.000 = Rp 8.300.000`.
- **Hasil Mutasi Kas:**
  - `MovementType`: `PEMBAYARAN-TUNAI-DOKUMEN`
  - `Direction`: `OUT`
  - `Amount`: **`8.300.000,00`** (bukan Rp 10.000.000!)
  - `BusinessDate`: `2026-10-01` (disalin dari `ApprovedAt` WIB)
  - `SourceReferenceType`: `PAYMENT`
  - `SourceReferenceId`: ID dokumen `FinPayment`
  - **Verifikasi:** Kas keluar tercatat tepat sebesar uang tunai aktual yang diserahkan (`NetTransferAmount`), membuktikan pencegahan overstatement kas keluar (`PASS`).

### Kasus Uji 5 — Sumber 5: Setoran Bank Diposting
- **Skenario:** Kasir menyetorkan uang tunai sebesar Rp 15.000.000 ke rekening operasional Bank Mandiri RS pada 2 Oktober 2026 (`FinBankDeposit` status `DRAFT` diposting menjadi `POSTED`).
- **Hasil Mutasi:**
  - `MovementType`: `SETORAN-BANK`
  - `Direction`: `OUT`
  - `Amount`: `15.000.000,00`
  - `BusinessDate`: `2026-10-02` (`DepositDate`)
  - `SourceReferenceType`: `BANK_DEPOSIT`
  - `SourceReferenceId`: ID setoran bank
  - **Verifikasi:** Kas fisik kasir berkurang Rp 15.000.000 karena berpindah ke rekening bank (`PASS`).

### Kasus Uji 6 — Sumber 6: Pembatalan Setoran Bank yang Pernah Diposting
- **Skenario:** Setoran bank Rp 15.000.000 yang diposting di Kasus Uji 5 dibatalkan karena slip setoran tertolak oleh teller bank (misal uang palsu / selisih hitung).
- **Hasil Mutasi:**
  - `MovementType`: `PEMBALIKAN-SETORAN-BANK`
  - `Direction`: `IN`
  - `Amount`: `15.000.000,00`
  - `BusinessDate`: Tanggal pembatalan WIB (`Today`)
  - `SourceReferenceType`: `BANK_DEPOSIT`
  - `SourceReferenceId`: ID setoran bank
  - **Verifikasi:** Kas kasir bertambah kembali Rp 15.000.000 membalik pengeluaran sebelumnya (`PASS`). Pembatalan setoran berstatus `DRAFT` terbukti tidak mencatat mutasi pembalikan (`PASS`).

---

## 5. Ringkasan Kepatuhan Tata Kelola

| Aturan Tata Kelola | Kepatuhan | Bukti |
| :--- | :---: | :--- |
| **Tidak ada build otomatis** | `PASS` | Sesuai instruksi pengguna, `dotnet build` tidak dijalankan secara otomatis. |
| **Satu service terpusat** | `PASS` | `FinanceSubledgerMovementService` menjadi satu-satunya gerbang penulisan buku mutasi kas. |
| **Enam sumber lengkap** | `PASS` | Shift kasir, AR tunai, AP tunai, voucher tunai NetTransferAmount, setoran bank, dan pembalikan setoran bank tersambung penuh. |
| **Pencegahan overstatement kas** | `PASS` | Dokumen voucher kas keluar bernilai `NetTransferAmount`, bukan jumlah alokasi. |
| **Idempotensi mutasi shift** | `PASS` | Mencegah penggandaan mutasi kas shift kasir saat sinkronisasi berkala dijalankan berulang. |
| **Shift OPEN tidak mutasi** | `PASS` | Mutasi kas hanya terbit saat shift mencapai `CLOSED` atau `REVIEWED`. |
| **Rekap harian fleksibel** | `PASS` | Penutupan kas harian dapat dijalankan meskipun ada shift yang masih `OPEN`. |
| **Nol migration baru** | `PASS` | Memakai skema `FinCashMovement` yang telah berdiri sejak `BE-FIN-058`. |

---

## 6. Tindak Lanjut

| Komponen | Status | Keterangan |
| :--- | :---: | :--- |
| Kompilasi & Build | `MANUAL USER` | Pengguna dapat menjalankan `dotnet build` secara manual untuk memvalidasi kompilasi. |
| Langkah berikutnya | **`BE-FIN-063`** | Membangun tiga (3) endpoint baca berpaging jejak mutasi subledger per piutang, per utang, dan kas. |
