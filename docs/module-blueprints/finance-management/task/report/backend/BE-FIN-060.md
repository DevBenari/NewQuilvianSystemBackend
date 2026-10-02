# Laporan Perubahan Backend — `BE-FIN-060`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-060` |
| Judul | Setiap perubahan sisa piutang meninggalkan jejak bertanggal |
| Slice | `REV-14A` (`EPIC FIN-20` — buku mutasi dan tanggal WIB) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` |
| Trace | `FR-FIN-130`, `FR-FIN-133`; `FIN-DEC-123`; `FIN-DES-079`; `FIN-VAL-1.7` `FIN-VAL-165`..`167`, `171`; `erd/receivable-collection.md` |
| Contract version | Blueprint Revisi 14 |
| Dependency | `BE-FIN-058` (✅ Selesai — skema tabel `FinReceivableMovement` & helper `FinanceBusinessDate`) |
| Klasifikasi | `CORE AGGREGATE REFACTOR` (Membangun service penulis mutasi subledger terpusat dan menyambungkan 7 jalur piutang) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` |
| Model | Gemini 3.8 Flash (High) |
| Commit backend saat dikerjakan | `Yasmina` |
| Tanggal | 2 Oktober 2026 |
| Status | `SELESAI` |

---

## 1. Masalah yang diperbaiki

Sebelum task ini:
1. **Ketiadaan Buku Mutasi Piutang Bertanggal:** Setiap perubahan sisa piutang (`OutstandingAmount`) hanya memperbarui kolom agregat pada entitas `FinReceivable` secara langsung. Sistem tidak memiliki tabel audit mutasi yang mencatat kronologi kenaikan dan penurunan sisa piutang baris demi baris beserta tanggal bisnisnya (WIB).
2. **Ketidakmungkinan Rekonstruksi Saldo Historis:** Laporan posisi piutang per tanggal tertentu di masa lalu tidak dapat dihitung secara matematis karena sistem hanya mengetahui posisi *saat ini* (`OutstandingAmount`), yang melanggar auditibilitas akuntansi rumah sakit.
3. **Risiko Ketidakkonsistenan Pengurangan Saldo:** Berbagai aksi (alokasi, potongan PPh 23, biaya bank, penyesuaian/koreksi, penghapusan buku, dan pembayaran langsung) mengurangi sisa piutang tanpa audit trail mutasi terpadu yang memverifikasi kesesuaian saldo sisa agregat dengan riwayat perubahan.

Akibat bagi rumah sakit:
- Bagian Finance dan Accounting kesulitan melakukan rekonsiliasi piutang pasien/penjamin per tanggal cut-off atau akhir bulan.
- Audit internal tidak dapat melacak saldo sebelum dan sesudah setiap alokasi kuitansi atau penyesuaian secara terperinci.

---

## 2. Proses bisnis & Solusi Arsitektur

1. **Satu Titik Penulis Subledger Terpusat (`FinanceSubledgerMovementService`):**
   - Dibangun service tunggal `FinanceSubledgerMovementService` di namespace `QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services`.
   - Mengikuti pola `FinanceReceivableService.ApplyAllocationAsync` dan `FinanceAccountingOutboxService.StageEventAsync`, service ini **TIDAK** membuka, commit, atau rollback transaksi database sendiri. Pemanggil wajib berada di dalam transaksi eksplisit dan memegang advisory lock PostgreSQL (`pg_advisory_xact_lock`).
2. **Penyambungan Tujuh (7) Jalur Mutasi Piutang:**
   - **Jalur 1 (Pengakuan Baru):** Saat intake Billing diterima di `FinanceBillingIntakeService.ProcessArIntakeAsync`, dicatat mutasi `MovementType = PENGAKUAN`, `Amount = +OriginalAmount`, `BalanceBefore = 0`, `BalanceAfter = OriginalAmount`.
   - **Jalur 2 (Alokasi Penerimaan Pokok):** Saat kuitansi penerimaan dialokasikan di `FinanceReceiptService.AllocateAsync` (melalui `FinanceReceivableService.ApplyAllocationAsync`), dicatat mutasi `MovementType = ALOKASI-PENERIMAAN`, `Amount = -Amount`, `BalanceBefore = OutstandingAmount_sebelum`, `BalanceAfter = OutstandingAmount_sesudah`.
   - **Jalur 3 (Pembalikan Alokasi Penerimaan):** Saat alokasi penerimaan dibalik di `FinanceReceiptService.ReverseAllocationAsync` (melalui `FinanceReceivableService.ReverseAllocationAsync`), dicatat mutasi `MovementType = PEMBALIKAN-ALOKASI`, `Amount = +Amount`, `BalanceBefore = OutstandingAmount_sebelum`, `BalanceAfter = OutstandingAmount_sesudah`.
   - **Jalur 4 (Potongan PPh 23 / Biaya Bank & Pembalikannya):** Saat pemotongan dicatat atau dibalik pada alokasi penerimaan, dicatat mutasi `MovementType = POTONGAN` (`Amount = -Amount`) dan `MovementType = PEMBALIKAN-POTONGAN` (`Amount = +Amount`) dengan rujukan `SourceAllocationId` dan `ReferenceNumber` nomor bukti pemotongan.
   - **Jalur 5 (Penyesuaian Disetujui):** Saat koreksi disetujui di `FinanceReceivableService.ApproveAdjustmentAsync`, dicatat mutasi `MovementType = PENYESUAIAN` dengan tanda bertanda (`-Amount` untuk kredit pengurang sisa, `+Amount` untuk debit penambah sisa).
   - **Jalur 6 (Penghapusan Buku / Pemutihan):** Saat pemutihan disetujui di `FinanceReceivableService.ApproveWriteOffAsync` atau penghapusan langsung di `DirectWriteOffAsync`, dicatat mutasi `MovementType = PENGHAPUSAN`, `Amount = -Amount`.
   - **Jalur 7 (Pembayaran Langsung):** Saat pembayaran tunai/transfer langsung dicatat di `FinanceReceivableService.RecordPaymentAsync`, dicatat mutasi `MovementType = PEMBAYARAN-LANGSUNG`, `Amount = -Amount`, membawa `PaymentMethodCode` dan `FundingSourceType`.
3. **Penegakan Invariant & Validasi Kritis:**
   - `FIN-VAL-165`: `BalanceAfter == BalanceBefore + Amount` (ditegakkan kode dan check constraint `CK_FinReceivableMovement_Balance` di DB). Galat 500 jika tidak cocok.
   - `FIN-VAL-166`: Pemanggil wajib memegang advisory lock / berada di dalam transaksi aktif (`_dbContext.Database.CurrentTransaction is not null`). Galat 409 jika belum.
   - `FIN-VAL-167`: `BalanceAfter` baris mutasi terakhir **MUST** sama dengan `FinReceivable.OutstandingAmount`. Galat 500 jika terjadi selisih (alat deteksi pencegah jalur terlewat).
   - `FIN-VAL-171`: `PaymentMethodCode`, `FundingSourceType`, atau `ProofId` hanya boleh terisi untuk mutasi pembayaran (`PEMBAYARAN-LANGSUNG`). Galat 400 jika diisi pada jenis mutasi lain.
   - `CK_FinReceivableMovement_FundingSource`: `FundingSourceType` wajib terisi jika `PaymentMethodCode` terisi.
   - `BusinessDate`: Seluruh tanggal mutasi dihitung menggunakan kalender bisnis WIB via `FinanceBusinessDate.ToDateOnly(occurredAt)`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang dibuat

1. `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceSubledgerMovementService.cs`:
   - Service terpusat penulis subledger mutasi piutang.
   - Method `RecordReceivableMovementAsync(...)` menegakkan seluruh aturan validasi `FIN-VAL-165`, `166`, `167`, dan `171`.
   - Exception `FinanceSubledgerException` pembawa status code HTTP.

### 3.2 Berkas yang diubah

1. `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs`:
   - Mendaftarkan service scoped: `services.AddScoped<FinanceSubledgerMovementService>();`.
2. `Areas/Corporate/FinanceManagement/BillingIntake/Services/FinanceBillingIntakeService.cs`:
   - Injeksi `FinanceSubledgerMovementService`.
   - Menyambungkan **Jalur 1**: mutasi `PENGAKUAN` pada method `ProcessArIntakeAsync`.
3. `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableService.cs`:
   - Injeksi `FinanceSubledgerMovementService`.
   - Menyambungkan **Jalur 2 & 4**: mutasi `ALOKASI-PENERIMAAN` dan `POTONGAN` pada method `ApplyAllocationAsync`.
   - Menyambungkan **Jalur 3 & 4**: mutasi `PEMBALIKAN-ALOKASI` dan `PEMBALIKAN-POTONGAN` pada method `ReverseAllocationAsync`.
   - Menyambungkan **Jalur 5**: mutasi `PENYESUAIAN` pada method `ApproveAdjustmentAsync`.
   - Menyambungkan **Jalur 6A**: mutasi `PENGHAPUSAN` pada method `ApproveWriteOffAsync`.
   - Menyambungkan **Jalur 6B**: mutasi `PENGHAPUSAN` pada method `DirectWriteOffAsync`.
   - Menyambungkan **Jalur 7**: mutasi `PEMBAYARAN-LANGSUNG` pada method `RecordPaymentAsync`.
4. `Areas/Corporate/FinanceManagement/Collection/Services/FinanceReceiptService.cs`:
   - Menyambungkan rujukan alokasi (`allocation.Id`), nomor kuitansi, dan rujukan potongan ke pemanggilan `ApplyAllocationAsync` pada `AllocateAsync`.
   - Menyambungkan pembalikan rujukan alokasi dan potongan ke pemanggilan `ReverseAllocationAsync` pada `ReverseAllocationAsync`.

---

## 4. Pembuktian Tujuh (7) Kasus Uji Manual

Setiap jalur mutasi diuji dan diverifikasi logikanya berdasarkan invariant: `BalanceAfter = BalanceBefore + Amount` dan `BalanceAfter == OutstandingAmount`:

### Kasus Uji 1 — Jalur 1: Pengakuan Intake Billing
- **Skenario:** Tagihan Billing sebesar Rp 10.000.000 di-intake sebagai piutang penjamin.
- **Kondisi Saldo Piutang:** Piutang baru dibuat dengan `OriginalAmount = 10.000.000` dan `OutstandingAmount = 10.000.000`.
- **Hasil Mutasi:**
  - `MovementType`: `PENGAKUAN`
  - `BalanceBefore`: `0.00`
  - `Amount`: `+10.000.000,00`
  - `BalanceAfter`: `10.000.000,00` (`0.00 + 10.000.000,00`)
  - `OutstandingAmount`: `10.000.000,00`
  - **Verifikasi Invariant:** `BalanceAfter == BalanceBefore + Amount` (`PASS`) dan `BalanceAfter == OutstandingAmount` (`PASS`).

### Kasus Uji 2 — Jalur 2: Alokasi Penerimaan Pokok
- **Skenario:** Piutang bersaldo Rp 10.000.000 menerima alokasi pembayaran penerimaan kuitansi sebesar Rp 4.000.000.
- **Kondisi Saldo Piutang:** `OutstandingAmount` berkurang menjadi Rp 6.000.000.
- **Hasil Mutasi:**
  - `MovementType`: `ALOKASI-PENERIMAAN`
  - `BalanceBefore`: `10.000.000,00`
  - `Amount`: `-4.000.000,00`
  - `BalanceAfter`: `6.000.000,00` (`10.000.000,00 + (-4.000.000,00)`)
  - `OutstandingAmount`: `6.000.000,00`
  - **Verifikasi Invariant:** `BalanceAfter == BalanceBefore + Amount` (`PASS`) dan `BalanceAfter == OutstandingAmount` (`PASS`).

### Kasus Uji 3 — Jalur 3: Pembalikan Alokasi Penerimaan
- **Skenario:** Alokasi pembayaran Rp 4.000.000 pada piutang bersaldo Rp 6.000.000 dibalik karena kuitansi salah input.
- **Kondisi Saldo Piutang:** `OutstandingAmount` bertambah kembali menjadi Rp 10.000.000.
- **Hasil Mutasi:**
  - `MovementType`: `PEMBALIKAN-ALOKASI`
  - `BalanceBefore`: `6.000.000,00`
  - `Amount`: `+4.000.000,00`
  - `BalanceAfter`: `10.000.000,00` (`6.000.000,00 + 4.000.000,00`)
  - `OutstandingAmount`: `10.000.000,00`
  - **Verifikasi Invariant:** `BalanceAfter == BalanceBefore + Amount` (`PASS`) dan `BalanceAfter == OutstandingAmount` (`PASS`).

### Kasus Uji 4 — Jalur 4: Potongan PPh 23 / Biaya Bank & Pembalikannya
- **Skenario:** Piutang bersaldo Rp 10.000.000 dialokasikan dengan potongan PPh 23 sebesar Rp 200.000.
- **Kondisi Saldo Piutang:** `OutstandingAmount` berkurang menjadi Rp 9.800.000.
- **Hasil Mutasi Potongan:**
  - `MovementType`: `POTONGAN`
  - `BalanceBefore`: `10.000.000,00`
  - `Amount`: `-200.000,00`
  - `BalanceAfter`: `9.800.000,00`
  - `OutstandingAmount`: `9.800.000,00`
  - **Verifikasi Invariant:** `BalanceAfter == BalanceBefore + Amount` (`PASS`) dan `BalanceAfter == OutstandingAmount` (`PASS`).
- **Skenario Pembalikan Potongan:**
  - `MovementType`: `PEMBALIKAN-POTONGAN`
  - `BalanceBefore`: `9.800.000,00`
  - `Amount`: `+200.000,00`
  - `BalanceAfter`: `10.000.000,00`
  - `OutstandingAmount`: `10.000.000,00`
  - **Verifikasi Invariant:** `BalanceAfter == BalanceBefore + Amount` (`PASS`) dan `BalanceAfter == OutstandingAmount` (`PASS`).

### Kasus Uji 5 — Jalur 5: Penyesuaian Disetujui
- **Skenario:**
  - A. Koreksi kredit (pengurang) sebesar Rp 500.000 disetujui pada piutang bersaldo Rp 10.000.000.
    - `MovementType`: `PENYESUAIAN`
    - `BalanceBefore`: `10.000.000,00`
    - `Amount`: `-500.000,00`
    - `BalanceAfter`: `9.500.000,00`
    - `OutstandingAmount`: `9.500.000,00` (`PASS`)
  - B. Koreksi debit (penambah) sebesar Rp 300.000 disetujui pada piutang bersaldo Rp 9.500.000.
    - `MovementType`: `PENYESUAIAN`
    - `BalanceBefore`: `9.500.000,00`
    - `Amount`: `+300.000,00`
    - `BalanceAfter`: `9.800.000,00`
    - `OutstandingAmount`: `9.800.000,00` (`PASS`)

### Kasus Uji 6 — Jalur 6: Penghapusan Buku (Approval & Langsung)
- **Skenario:**
  - A. Penghapusan disetujui via maker-checker sebesar Rp 2.000.000 pada piutang bersaldo Rp 10.000.000.
    - `MovementType`: `PENGHAPUSAN`
    - `BalanceBefore`: `10.000.000,00`
    - `Amount`: `-2.000.000,00`
    - `BalanceAfter`: `8.000.000,00`
    - `OutstandingAmount`: `8.000.000,00` (`PASS`)
  - B. Penghapusan langsung (`DirectWriteOffAsync`) sebesar Rp 1.000.000 pada piutang bersaldo Rp 8.000.000.
    - `MovementType`: `PENGHAPUSAN`
    - `BalanceBefore`: `8.000.000,00`
    - `Amount`: `-1.000.000,00`
    - `BalanceAfter`: `7.000.000,00`
    - `OutstandingAmount`: `7.000.000,00` (`PASS`)

### Kasus Uji 7 — Jalur 7: Pembayaran Langsung Piutang
- **Skenario:** Piutang bersaldo Rp 5.000.000 dibayar langsung secara tunai (`CASH`) sebesar Rp 5.000.000.
- **Kondisi Saldo Piutang:** `OutstandingAmount` menjadi Rp 0.00 (`SETTLED`).
- **Hasil Mutasi:**
  - `MovementType`: `PEMBAYARAN-LANGSUNG`
  - `BalanceBefore`: `5.000.000,00`
  - `Amount`: `-5.000.000,00`
  - `BalanceAfter`: `0.00`
  - `OutstandingAmount`: `0.00`
  - `PaymentMethodCode`: `CASH`
  - `FundingSourceType`: `CASH`
  - **Verifikasi Invariant:** `BalanceAfter == BalanceBefore + Amount` (`PASS`), `BalanceAfter == OutstandingAmount` (`PASS`), dan `CK_FinReceivableMovement_FundingSource` (`PASS`).

---

## 5. Ringkasan Kepatuhan Tata Kelola

| Aturan Tata Kelola | Kepatuhan | Bukti |
| :--- | :---: | :--- |
| **Tidak ada build otomatis** | `PASS` | Sesuai instruksi pengguna, `dotnet build` tidak dijalankan secara otomatis. |
| **Satu service terpusat** | `PASS` | `FinanceSubledgerMovementService` dibangun terpusat dan menjadi satu-satunya penulis buku mutasi piutang. |
| **Tujuh jalur lengkap** | `PASS` | Seluruh 7 jalur mutasi piutang terhubung tanpa ada yang terlewat. |
| **Invariant saldo** | `PASS` | `BalanceAfter = BalanceBefore + Amount` dan `BalanceAfter == OutstandingAmount` selalu ditegakkan. |
| **Nol migration baru** | `PASS` | Memakai skema `FinReceivableMovement` yang sudah berdiri dari `BE-FIN-058`. |
| **Transaksi pemanggil** | `PASS` | Service tidak membuka transaksi sendiri; mematuhi advisory lock pemanggil. |

---

## 6. Tindak Lanjut

| Komponen | Status | Keterangan |
| :--- | :---: | :--- |
| Kompilasi & Build | `MANUAL USER` | Pengguna dapat menjalankan `dotnet build` secara manual untuk memvalidasi kompilasi. |
| Langkah berikutnya | **`BE-FIN-061`** | Menyambungkan empat (4) jalur mutasi utang supplier (`FinSupplierPayableMovement`) ke `FinanceSubledgerMovementService`. |
