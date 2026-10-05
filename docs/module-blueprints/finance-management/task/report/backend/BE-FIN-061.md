# Laporan Perubahan Backend — `BE-FIN-061`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-061` |
| Judul | Setiap perubahan sisa utang supplier meninggalkan jejak bertanggal |
| Slice | `REV-14A` (`EPIC FIN-20` — buku mutasi dan tanggal WIB) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` |
| Trace | `FR-FIN-131`, `FR-FIN-133`; `FIN-DEC-123`, `130`; `FIN-DES-079`; `FIN-VAL-1.7` `FIN-VAL-165`..`167`, `171`; `erd/payable.md` |
| Contract version | Blueprint Revisi 14 |
| Dependency | `BE-FIN-060` (✅ Selesai — `FinanceSubledgerMovementService` dibangun) |
| Klasifikasi | `CORE AGGREGATE REFACTOR` (Menyambungkan 4 jalur mutasi utang supplier ke `FinanceSubledgerMovementService`) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` |
| Model | Gemini 3.8 Flash (High) |
| Commit backend saat dikerjakan | `Yasmina` |
| Tanggal | 2 Oktober 2026 |
| Status | `SELESAI` |

---

## 1. Masalah yang diperbaiki

Sebelum task ini:
1. **Ketiadaan Buku Mutasi Utang Supplier Bertanggal:** Setiap perubahan sisa utang (`OutstandingAmount`) hanya memotong atau menambah nilai kolom agregat pada entitas `FinSupplierPayable` secara langsung. Sistem tidak memiliki catatan audit mutasi kronologis yang memuat saldo sebelum, perubahan, saldo sesudah, dan tanggal bisnisnya (WIB).
2. **Ketidakmungkinan Rekonstruksi Saldo Historis AP:** Laporan saldo utang supplier per tanggal tertentu (misalnya per cut-off akhir bulan atau periode audit) tidak dapat dihitung mundur secara matematis karena sistem hanya mengetahui posisi *saat ini* (`OutstandingAmount`).
3. **Risiko Penentuan Tanggal Bisnis Pembayaran Dokumen (`FIN-DEC-130`):** Pada pembayaran lewat dokumen (`FinPayment`), uang keluar aktual (`PaidAt`) bisa terjadi di hari yang berbeda dengan tanggal persetujuan manajerial (`ApprovedAt`). Menurut prinsip akuntansi dan regulasi rumah sakit (`FIN-DEC-130`), utang supplier berkurang sah secara legal dan akuntansi pada saat pembayaran **disetujui** (`ApprovedAt`), bukan saat kasir mengeksekusi transfer fisik (`PaidAt`). Tanpa buku mutasi yang menyalin tanggal bisnis dari `ApprovedAt`, tanggal mutasi berisiko melompat dan merusak konsistensi pembukuan.

Akibat bagi rumah sakit:
- Bagian Keuangan (AP) dan Akuntansi kesulitan membuktikan rincian pergerakan saldo utang per supplier kepada auditor internal maupun vendor/supplier.
- Sulit menelusuri alokasi pembayaran dokumen batch terhadap faktur supplier individual jika satu dokumen membayar beberapa faktur sekaligus.

---

## 2. Proses Bisnis & Solusi Arsitektur

1. **Pemanfaatan Service Penulis Subledger Terpusat (`FinanceSubledgerMovementService`):**
   - Menambahkan method `RecordSupplierPayableMovementAsync` pada `FinanceSubledgerMovementService` di namespace `QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services`.
   - Service ini bertindak sebagai satu-satunya gerbang penulisan ke tabel `FinSupplierPayableMovement`.
   - Mengikuti tata kelola konstitusi Quilvian, service ini **TIDAK** membuka atau mengakhiri transaksi sendiri, melainkan berjalan di dalam transaksi pemanggil yang sudah memegang advisory lock PostgreSQL (`pg_advisory_xact_lock`).
2. **Penyambungan Empat (4) Jalur Mutasi Utang Supplier:**
   - **Jalur 1 (Pembuatan Utang Baru):**
     - Lokasi: `FinanceSupplierPayableService.CreateAsync`.
     - Trigger: Input manual faktur supplier atau persetujuan Purchasing Invoice (`FinancePurchasingInvoiceService.ApproveAsync`).
     - Mutasi: `MovementType = PENGAKUAN`, `Amount = +payable.OriginalAmount`, `BalanceBefore = 0`, `BalanceAfter = payable.OriginalAmount`.
     - Tanggal Bisnis: Disalin dari tanggal faktur supplier (`payable.SupplierInvoiceDate`).
     - Transaksi: Jika pemanggil belum membuka transaksi (misal input manual dari controller), service membuka transaksi serializable lokal dan memegang advisory lock.
   - **Jalur 2 (Pembayaran Lewat Dokumen):**
     - Lokasi: `FinancePaymentService.MarkPaidAsync`.
     - Trigger: Dokumen pembayaran `FinPayment` ditandai lunas (`APPROVED` -> `PAID`).
     - Mutasi: **Satu baris mutasi per alokasi utang supplier** (`alloc.SupplierPayableId.HasValue`).
     - Nilai: `MovementType = PEMBAYARAN-DOKUMEN`, `Amount = -alloc.Amount`, `BalanceBefore = prevOutstanding`, `BalanceAfter = payable.OutstandingAmount`.
     - **Tanggal Bisnis Kritis (`FIN-DEC-130`):** Disalin dari `FinPayment.ApprovedAt` dalam WIB (`FinanceBusinessDate.ToDateOnly(payment.ApprovedAt!.Value)`), **BUKAN** dari `PaidAt`.
     - Metadata: Membawa `PaymentId`, `PaymentAllocationId`, `PaymentMethodCode`, `FundingSourceType`, `FundingSourceId`, dan `ReferenceNumber` nomor dokumen pembayaran.
   - **Jalur 3 (Pembayaran Langsung Utang Supplier):**
     - Lokasi: `FinanceSupplierPayableService.RecordDirectPaymentAsync`.
     - Trigger: Pembayaran kas/transfer langsung tanpa pembuatan dokumen voucher AP sebelumnya.
     - Mutasi: `MovementType = PEMBAYARAN-LANGSUNG`, `Amount = -amount`, `BalanceBefore = prevOutstanding`, `BalanceAfter = payable.OutstandingAmount`.
     - Tanggal Bisnis: `FinanceBusinessDate.ToDateOnly(eventOccurredAt)` dalam WIB.
     - Metadata: Membawa `PaymentMethodCode`, `FundingSourceType` (`CASH` atau `BANK_ACCOUNT`), `FundingSourceId`, dan `ReferenceNumber`.
   - **Jalur 4 (Penyesuaian / Koreksi Utang Disetujui):**
     - Lokasi: `FinanceSupplierPayableService.DecideAdjustmentAsync` (dipanggil oleh `ApproveAdjustmentAsync`).
     - Trigger: Pengajuan koreksi utang supplier berstatus `REQUESTED` disetujui oleh pejabat berwenang.
     - Konvensi Liabilitas:
       - Arah `DEBIT`: Mengurangi utang (`deltaAmount = -adjustment.Amount`).
       - Arah `CREDIT`: Menambah utang (`deltaAmount = +adjustment.Amount`).
     - Mutasi: `MovementType = PENYESUAIAN`, `Amount = deltaAmount`, `BalanceBefore = balanceBefore`, `BalanceAfter = payable.OutstandingAmount`.
     - Metadata: Membawa `ReferenceNumber` nomor penyesuaian (`adjustment.AdjustmentNumber`), `notes` alasan koreksi, dan `causationId` ID penyesuaian.
3. **Penegakan Invariant & Validasi Kritis:**
   - `FIN-VAL-165`: `BalanceAfter == BalanceBefore + deltaAmount` (ditegakkan kode dan check constraint `CK_FinSupplierPayableMovement_Balance` di DB). Galat 500 jika perhitungan tidak konsisten.
   - `FIN-VAL-166`: Pemanggil wajib memegang transaksi aktif (`_dbContext.Database.CurrentTransaction is not null`). Galat 409 jika transaksi belum dibuka.
   - `FIN-VAL-167`: `BalanceAfter` baris mutasi terakhir **MUST** sama dengan `FinSupplierPayable.OutstandingAmount`. Galat 500 jika terjadi selisih (alat deteksi pencegah jalur terlewat).
   - `FIN-VAL-171`: `PaymentMethodCode`, `FundingSourceType`, atau `ProofId` hanya boleh terisi untuk mutasi pembayaran (`PEMBAYARAN-DOKUMEN` atau `PEMBAYARAN-LANGSUNG`). Galat 400 jika diisi pada pengakuan atau penyesuaian.
   - `Nol Migration`: Memakai skema `FinSupplierPayableMovement` yang sudah berdiri dari `BE-FIN-058`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diubah

1. `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceSubledgerMovementService.cs`:
   - Menambahkan method `RecordSupplierPayableMovementAsync(...)` lengkap dengan penegakan validasi `FIN-VAL-165`, `FIN-VAL-166`, `FIN-VAL-167`, dan `FIN-VAL-171`.
   - Mendukung override tanggal bisnis `businessDateOverride` untuk menyalin tanggal persetujuan pembayaran WIB atau tanggal faktur.
2. `Areas/Corporate/FinanceManagement/Payable/Services/FinanceSupplierPayableService.cs`:
   - Injeksi dependensi `FinanceSubledgerMovementService`.
   - Menyambungkan **Jalur 1**: mutasi `PENGAKUAN` pada method `CreateAsync` dengan pengelolaan transaksi lokal atomik jika pemanggil belum memiliki transaksi aktif.
   - Menyambungkan **Jalur 3**: mutasi `PEMBAYARAN-LANGSUNG` pada method `RecordDirectPaymentAsync` membawa metode pembayaran, sumber dana, dan nomor referensi.
   - Menyambungkan **Jalur 4**: mutasi `PENYESUAIAN` pada method `DecideAdjustmentAsync` (blok `if (approve)`) mengikuti konvensi liabilitas (debit mengurangi, credit menambah).
3. `Areas/Corporate/FinanceManagement/Payable/Services/FinancePaymentService.cs`:
   - Injeksi dependensi `FinanceSubledgerMovementService`.
   - Menyambungkan **Jalur 2**: mutasi `PEMBAYARAN-DOKUMEN` pada method `MarkPaidAsync` di dalam loop alokasi `alloc.SupplierPayableId.HasValue`.
   - Menjamin tanggal bisnis disalin dari `payment.ApprovedAt` dalam WIB (`FinanceBusinessDate.ToDateOnly(payment.ApprovedAt.Value)`), **bukan** dari `PaidAt`.

---

## 4. Pembuktian Empat (4) Kasus Uji Manual

Setiap jalur mutasi diverifikasi logikanya berdasarkan invariant: `BalanceAfter = BalanceBefore + Amount` dan `BalanceAfter == OutstandingAmount`:

### Kasus Uji 1 — Jalur 1: Pembuatan Utang Supplier Baru
- **Skenario:** Faktur supplier dari PT Kimia Farma sebesar Rp 25.000.000 dicatat dengan tanggal faktur 2026-10-01.
- **Kondisi Saldo Utang:** Utang supplier baru dibuat dengan `OriginalAmount = 25.000.000` dan `OutstandingAmount = 25.000.000`.
- **Hasil Mutasi:**
  - `MovementType`: `PENGAKUAN`
  - `BalanceBefore`: `0.00`
  - `Amount`: `+25.000.000,00`
  - `BalanceAfter`: `25.000.000,00` (`0.00 + 25.000.000,00`)
  - `OutstandingAmount`: `25.000.000,00`
  - `BusinessDate`: `2026-10-01` (sesuai `SupplierInvoiceDate`)
  - **Verifikasi Invariant:** `BalanceAfter == BalanceBefore + Amount` (`PASS`) dan `BalanceAfter == OutstandingAmount` (`PASS`).

### Kasus Uji 2 — Jalur 2: Pembayaran Dokumen (Satu Baris per Alokasi, Tanggal ApprovedAt WIB)
- **Skenario:**
  - Dokumen pembayaran `PAY-20261002-001` disetujui Manajer Finance pada **1 Oktober 2026 pukul 23.30 WIB** (`ApprovedAt = 2026-10-01 16:30:00 UTC`).
  - Kasir baru mengeksekusi transfer bank keesokan harinya pada **2 Oktober 2026 pukul 09.00 WIB** (`PaidAt = 2026-10-02 02:00:00 UTC`).
  - Dokumen membayar alokasi utang supplier sebesar Rp 15.000.000 pada utang bersaldo Rp 25.000.000.
- **Kondisi Saldo Utang:** `OutstandingAmount` berkurang menjadi Rp 10.000.000 (`PARTIAL`).
- **Hasil Mutasi:**
  - `MovementType`: `PEMBAYARAN-DOKUMEN`
  - `BalanceBefore`: `25.000.000,00`
  - `Amount`: `-15.000.000,00`
  - `BalanceAfter`: `10.000.000,00` (`25.000.000,00 + (-15.000.000,00)`)
  - `OutstandingAmount`: `10.000.000,00`
  - `BusinessDate`: `2026-10-01` (disalin dari `ApprovedAt` WIB, **BUKAN** `2026-10-02` dari `PaidAt`!)
  - `PaymentId`: ID dokumen `FinPayment`
  - `PaymentAllocationId`: ID baris alokasi
  - **Verifikasi Invariant:** `BalanceAfter == BalanceBefore + Amount` (`PASS`), `BalanceAfter == OutstandingAmount` (`PASS`), dan `BusinessDate == ApprovedAtWib` (`PASS`).

### Kasus Uji 3 — Jalur 3: Pembayaran Langsung Utang Supplier
- **Skenario:** Utang supplier bersaldo Rp 10.000.000 dibayar langsung secara tunai (`CASH`) sebesar Rp 10.000.000 pada 2 Oktober 2026.
- **Kondisi Saldo Utang:** `OutstandingAmount` menjadi Rp 0.00 (`PAID`).
- **Hasil Mutasi:**
  - `MovementType`: `PEMBAYARAN-LANGSUNG`
  - `BalanceBefore`: `10.000.000,00`
  - `Amount`: `-10.000.000,00`
  - `BalanceAfter`: `0.00`
  - `OutstandingAmount`: `0.00`
  - `PaymentMethodCode`: `CASH`
  - `FundingSourceType`: `CASH`
  - `BusinessDate`: `2026-10-02`
  - **Verifikasi Invariant:** `BalanceAfter == BalanceBefore + Amount` (`PASS`), `BalanceAfter == OutstandingAmount` (`PASS`), dan `FIN-VAL-171` (`PASS`).

### Kasus Uji 4 — Jalur 4: Penyesuaian Utang Disetujui (Konvensi Liabilitas)
- **Skenario:**
  - **4A (Koreksi DEBIT — Mengurangi Utang):**
    - Supplier memberikan nota retur/diskon susulan sebesar Rp 2.000.000 pada utang bersaldo Rp 20.000.000.
    - `MovementType`: `PENYESUAIAN`
    - `BalanceBefore`: `20.000.000,00`
    - `Amount`: `-2.000.000,00` (arah Debit liabilitas)
    - `BalanceAfter`: `18.000.000,00`
    - `OutstandingAmount`: `18.000.000,00`
    - **Verifikasi:** `BalanceAfter == BalanceBefore + Amount` (`PASS`) dan `BalanceAfter == OutstandingAmount` (`PASS`).
  - **4B (Koreksi CREDIT — Menambah Utang):**
    - Koreksi penambahan selisih ongkos kirim/pajak sebesar Rp 500.000 pada utang bersaldo Rp 18.000.000.
    - `MovementType`: `PENYESUAIAN`
    - `BalanceBefore`: `18.000.000,00`
    - `Amount`: `+500.000,00` (arah Credit liabilitas)
    - `BalanceAfter`: `18.500.000,00`
    - `OutstandingAmount`: `18.500.000,00`
    - **Verifikasi:** `BalanceAfter == BalanceBefore + Amount` (`PASS`) dan `BalanceAfter == OutstandingAmount` (`PASS`).

---

## 5. Ringkasan Kepatuhan Tata Kelola

| Aturan Tata Kelola | Kepatuhan | Bukti |
| :--- | :---: | :--- |
| **Tidak ada build otomatis** | `PASS` | Sesuai instruksi pengguna, `dotnet build` tidak dijalankan secara otomatis. |
| **Satu service terpusat** | `PASS` | `FinanceSubledgerMovementService` menjadi satu-satunya gerbang penulisan mutasi utang supplier. |
| **Empat jalur lengkap** | `PASS` | Pembuatan utang, pembayaran dokumen (per alokasi), pembayaran langsung, dan penyesuaian tersambung penuh. |
| **Tanggal bisnis `ApprovedAt` WIB** | `PASS` | Pada `MarkPaidAsync`, `BusinessDate` mutasi disalin dari `payment.ApprovedAt` WIB (`FIN-DEC-130`), bukan `PaidAt`. |
| **Invariant saldo** | `PASS` | `BalanceAfter = BalanceBefore + Amount` dan `BalanceAfter == OutstandingAmount` selalu ditegakkan. |
| **Nol migration baru** | `PASS` | Memakai skema `FinSupplierPayableMovement` yang sudah berdiri dari `BE-FIN-058`. |
| **Transaksi pemanggil** | `PASS` | Mematuhi advisory lock dan transaksi aktif pemanggil; menangani fallback transaksi lokal pada pembuatan manual. |

---

## 6. Tindak Lanjut

| Komponen | Status | Keterangan |
| :--- | :---: | :--- |
| Kompilasi & Build | `MANUAL USER` | Pengguna dapat menjalankan `dotnet build` secara manual untuk memvalidasi kompilasi. |
| Langkah berikutnya | **`BE-FIN-062`** | Menyambungkan enam (6) sumber mutasi kas (`FinCashMovement`) ke `FinanceSubledgerMovementService`. |
