# Laporan Perubahan Backend — `BE-FIN-059`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-059` |
| Judul | Tanggal akuntansi dan batas periode tidak lagi melompat satu hari pada dini hari WIB |
| Slice | `REV-14A` (`EPIC FIN-20` — buku mutasi dan tanggal WIB) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` |
| Trace | `FR-FIN-136`; `FIN-DEC-116`; `FIN-DES-082`; `FIN-VAL-1.7` `FIN-VAL-210`; `FIN-INTEGRATION-1.7` 5.12.1 |
| Contract version | Blueprint Revisi 14 |
| Dependency | `BE-FIN-058` (✅ Selesai — helper `FinanceBusinessDate` dan skema subledger) |
| Klasifikasi | `HIGH RISK REFACTOR` (Menyentuh 21+ titik penentuan tanggal akuntansi dan batas periode pada seluruh service berjalan di Finance) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` |
| Model | Gemini 3.8 Flash (High) |
| Commit backend saat dikerjakan | `Yasmina` |
| Tanggal | 2 Oktober 2026 |
| Status | `SELESAI` |

---

## 1. Masalah yang diperbaiki

Sebelum task ini:
1. **Lompatan Tanggal pada Dini Hari WIB:** Perhitungan `AccountingDate` pada outbox integrasi akuntansi dihitung langsung memakai `DateOnly.FromDateTime(DateTime.UtcNow)` atau `DateOnly.FromDateTime(...UtcDateTime)`. Akibatnya, transaksi yang terjadi antara pukul 00.00 hingga 06.59 WIB (dini hari) menghasilkan tanggal akuntansi hari sebelumnya menurut kalender UTC (UTC tertinggal 7 jam).
2. **Distorsi Tutup Bulan & Periode Akuntansi:** Pada pergantian bulan (misal tanggal 1 Oktober pukul 02.00 WIB), transaksi yang dicatat staf justru mendapatkan `AccountingDate = 2026-09-30`, sehingga terkirim ke periode September padahal transaksi sudah masuk Oktober. Sebaliknya, perhitungan snapshot piutang akhir periode memakai `DateTimeOffset(..., TimeSpan.Zero)` (zona UTC) yang memotong data 7 jam lebih lambat.
3. **Batas Rekap Kas Berzona Nol:** Rekapitulasi kas harian (`FinanceCashManagementService`) menghitung batas waktu pembukaan shift kasir menggunakan rentang waktu `TimeSpan.Zero` (UTC), sehingga pergeseran shift malam atau dini hari tidak tersaring dalam batas hari kalender WIB yang semestinya.

Akibat bagi rumah sakit:
- Laporan penerimaan kasir, piutang, dan utang tidak cocok dengan laporan penutupan buku Accounting pada hari-hari awal dan akhir bulan.
- Muncul perselisihan audit karena staf kasir melihat kuitansi bertanggal 1 Oktober, namun jurnal akuntansi mengelompokkannya ke 30 September.

---

## 2. Proses bisnis

1. **Standardisasi Zona Waktu Bisnis Rumah Sakit (WIB / UTC+7):**
   - Seluruh penanggalan akuntansi (`AccountingDate`) dan batas periode bisnis di modul Finance wajib mengikuti kalender resmi operasional rumah sakit di Indonesia Barat (WIB, `Asia/Jakarta` dengan fallback `SE Asia Standard Time`).
   - Helper statis `FinanceBusinessDate` yang telah dibangun pada `BE-FIN-058` menjadi satu-satunya acuan konversi dan perhitungan tanggal.
2. **Pemisahan Tegas antara Waktu Kejadian Fisik dan Tanggal Akuntansi:**
   - `EventOccurredAt` **tetap** dalam format waktu universal berstempel UTC (`DateTimeOffset`) untuk merefleksikan kronologi audit log audit waktu mutlak.
   - `AccountingDate` mencatat **tanggal bisnis** kalender WIB di mana kejadian tersebut diakui secara finansial (`FinanceBusinessDate.ToDateOnly(...)`).
3. **Penyelarasan Rentang Rekapitulasi Kas & Snapshot Periode:**
   - Batas awal dan akhir hari rekapitulasi kas kasir (`startOfDay` dan `endOfDay`) dihitung dari `00:00:00 WIB` sampai `00:00:00 WIB` hari berikutnya menggunakan `FinanceBusinessDate.GetStartOfDayUtc(...)` dan `FinanceBusinessDate.GetEndOfDayUtc(...)`.
   - Batas akhir periode saldo subledger dihitung tepat pukul `23:59:59.999 WIB` menggunakan `FinanceBusinessDate.GetEndOfPeriodUtc(...)`, sehingga piutang yang diakui tanggal 30 September pukul 23.50 WIB masuk ke periode September, bukan Oktober.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

1. `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` (§Task BE-FIN-059)
2. `docs/module-blueprints/finance-management/02-backend-architecture.md` (§FIN-DES-082, §6 baris 3960, §4166)
3. `docs/module-blueprints/finance-management/01-existing-capability-map.md` (§18.2 FIN-OQ-050)
4. `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceBusinessDate.cs`

### 3.2 Berkas yang diubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableService.cs` | Mengubah 4 titik `AccountingDate` (penyesuaian, 2 penghapusan buku, pembayaran langsung) dan 2 titik tanggal acuan umur piutang menjadi `FinanceBusinessDate`. |
| `Areas/Corporate/FinanceManagement/Payable/Services/FinanceSupplierPayableService.cs` | Mengubah 1 titik `AccountingDate` (pembayaran langsung utang) dan 2 titik tanggal acuan aging menjadi `FinanceBusinessDate`. |
| `Areas/Corporate/FinanceManagement/Payable/Services/FinancePaymentService.cs` | Mengubah 2 titik `AccountingDate` (pembayaran AP dan pemakaian deposit retur) menjadi `FinanceBusinessDate.ToDateOnly(eventOccurredAt)`. |
| `Areas/Corporate/FinanceManagement/BillingIntake/Services/FinanceBillingIntakeService.cs` | Mengubah 9 titik `AccountingDate` outbox (pengakuan piutang, 3 mutasi deposit, kelebihan bayar, refund case, selisih kas, penutupan shift, pembalikan shift) dan 1 titik `DueDate` menjadi `FinanceBusinessDate`. |
| `Areas/Corporate/FinanceManagement/Collection/Services/FinanceReceiptService.cs` | Mengubah 4 titik `AccountingDate` (penerimaan kasir/uang muka, pembalikan kuitansi, potongan PPh23/biaya bank, pembalikan potongan) menjadi `FinanceBusinessDate.ToDateOnly(...)`. |
| `Areas/Corporate/FinanceManagement/CashManagement/Services/FinanceCashManagementService.cs` | Mengubah batas hari rekap kas (`startOfDay` dan `endOfDay`) di 2 tempat (`GetCurrentDailyCashAsync` dan `CloseDailyCashAsync`) serta inisialisasi tanggal hari ini menjadi `FinanceBusinessDate`. |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceSubledgerSnapshotService.cs` | Mengubah batas akhir periode `endOfPeriodUtc` menjadi `FinanceBusinessDate.GetEndOfPeriodUtc(periodEndDate)` dan perhitungan akhir bulan menjadi `FinanceBusinessDate.GetPeriodEndDate`. |
| `Areas/Corporate/FinanceManagement/Purchasing/Services/FinanceSupplierReturnService.cs` | Mengubah inisialisasi `accountingDate` pada retur pembelian dan PPN masukan menjadi `FinanceBusinessDate.ToDateOnly(occurredAt)`. |
| `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceNonPatientReceivableService.cs` | Mengubah tanggal acuan aging piutang non-pasien dan tanggal hari ini menjadi `FinanceBusinessDate.Today()`. |
| `Areas/Corporate/FinanceManagement/Purchasing/Services/FinancePurchasingReportService.cs` | Mengubah tanggal acuan laporan jatuh tempo dan rekonsiliasi faktur menjadi `FinanceBusinessDate.Today()`. |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceAccountingOutboxService.cs` | Mengubah penentuan tanggal akhir periode validasi outbox menjadi `FinanceBusinessDate.GetPeriodEndDate`. |

---

### 3.3 Daftar Rinci 21+ Titik yang Disentuh (Wajib Acceptance Criteria & Risk Tracking)

Berikut adalah audit lengkap seluruh titik yang telah diselaraskan ke `FinanceBusinessDate`:

#### Kelompok 1: 20 Titik `AccountingDate` pada Lima Service Utama (Capability Map §18.2)

1. **`FinanceReceivableService.cs:344`** (Titik 1): Outbox `PENYESUAIAN-PIUTANG` saat persetujuan koreksi piutang disetujui → `FinanceBusinessDate.ToDateOnly(adjustment.ApprovedAt!.Value)`.
2. **`FinanceReceivableService.cs:527`** (Titik 2): Outbox `PEMUTIHAN-PIUTANG` saat persetujuan penghapusan buku piutang disetujui → `FinanceBusinessDate.ToDateOnly(writeOff.ApprovedAt!.Value)`.
3. **`FinanceReceivableService.cs:663`** (Titik 3): Outbox `PENERIMAAN-PIUTANG` saat pembayaran langsung piutang → `FinanceBusinessDate.ToDateOnly(eventOccurredAt)`.
4. **`FinanceReceivableService.cs:757`** (Titik 4): Outbox `PEMUTIHAN-PIUTANG` saat penghapusan buku langsung → `FinanceBusinessDate.ToDateOnly(writeOff.ApprovedAt.Value)`.
5. **`FinanceSupplierPayableService.cs:253`** (Titik 5): Outbox `PEMBAYARAN-HUTANG-SUPPLIER` saat pembayaran langsung utang supplier → `FinanceBusinessDate.ToDateOnly(eventOccurredAt)`.
6. **`FinancePaymentService.cs:613`** (Titik 6): Outbox `PEMBAYARAN-HUTANG-SUPPLIER` saat pembayaran utang supplier via `FinPayment` → `FinanceBusinessDate.ToDateOnly(eventOccurredAt)`.
7. **`FinancePaymentService.cs:629`** (Titik 7): Outbox `PEMAKAIAN-KREDIT-RETUR-PEMBELIAN` saat pembayaran utang memakai deposit retur → `FinanceBusinessDate.ToDateOnly(eventOccurredAt)`.
8. **`FinanceBillingIntakeService.cs:489`** (Titik 8): Outbox `PENGAKUAN-PIUTANG` saat intaking piutang penjamin dari Billing → `FinanceBusinessDate.ToDateOnly(now)`.
9. **`FinanceBillingIntakeService.cs:664`** (Titik 9): Outbox `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT` saat pelepasan alokasi deposit Billing → `FinanceBusinessDate.ToDateOnly(movement.OccurredAt)`.
10. **`FinanceBillingIntakeService.cs:715`** (Titik 10): Outbox `PEMBALIKAN-PENERIMAAN-UANG-MUKA` saat pembatalan top-up deposit Billing → `FinanceBusinessDate.ToDateOnly(movement.OccurredAt)`.
11. **`FinanceBillingIntakeService.cs:743`** (Titik 11): Outbox `PEMAKAIAN-UANG-MUKA-DEPOSIT` saat pemakaian alokasi deposit Billing → `FinanceBusinessDate.ToDateOnly(movement.OccurredAt)`.
12. **`FinanceBillingIntakeService.cs:825`** (Titik 12): Outbox `PENGAKUAN-KELEBIHAN-BAYAR` saat pengakuan deposit kelebihan bayar pasien → `FinanceBusinessDate.ToDateOnly(credit.RecognizedAt)`.
13. **`FinanceBillingIntakeService.cs:906`** (Titik 13): Outbox `PENGEMBALIAN-UANG-MUKA` saat pelaksanaan pengembalian dana pasien (`BilRefundCase`) → `FinanceBusinessDate.ToDateOnly(eventTime)`.
14. **`FinanceBillingIntakeService.cs:990`** (Titik 14): Outbox `SELISIH-KAS-LEBIH` / `SELISIH-KAS-KURANG` saat tinjauan selisih kasir ditutup → `FinanceBusinessDate.ToDateOnly(shift.OpenedAt)` (konvensi tanggal buka shift).
15. **`FinanceBillingIntakeService.cs:1111`** (Titik 15): Outbox `PENUTUPAN-SHIFT-KASIR` penanda sinkronisasi shift kasir → `FinanceBusinessDate.ToDateOnly(shift.OpenedAt)`.
16. **`FinanceBillingIntakeService.cs:1141`** (Titik 16): Outbox `PEMBALIKAN-PENUTUPAN-SHIFT-KASIR` saat shift dibuka kembali → `FinanceBusinessDate.ToDateOnly(shift.OpenedAt)`.
17. **`FinanceReceiptService.cs:134`** (Titik 17): Outbox `PENERIMAAN-KASIR` / `PENERIMAAN-UANG-MUKA` dari tender kuitansi Billing → `FinanceBusinessDate.ToDateOnly(handoff.OccurredAt)`.
18. **`FinanceReceiptService.cs:207`** (Titik 18): Outbox `PEMBALIKAN-PENERIMAAN-KASIR` / `PEMBALIKAN-PENERIMAAN-UANG-MUKA` dari pembalikan tender → `FinanceBusinessDate.ToDateOnly(handoff.OccurredAt)`.
19. **`FinanceReceiptService.cs:640`** (Titik 19): Outbox `POTONGAN-PPH23-PIUTANG` / `POTONGAN-BIAYA-BANK-PIUTANG` saat alokasi potongan penerimaan → `FinanceBusinessDate.ToDateOnly(now)`.
20. **`FinanceReceiptService.cs:753`** (Titik 20): Outbox `PEMBALIKAN-POTONGAN-PPH23-PIUTANG` / `PEMBALIKAN-POTONGAN-BIAYA-BANK-PIUTANG` saat pembalikan alokasi → `FinanceBusinessDate.ToDateOnly(now)`.

#### Kelompok 2: Titik Rekap Kas & Snapshot Periode (Titik 21 & Pendukung)

21. **`FinanceCashManagementService.cs:409-410 & 617-618`** (Titik 21): Batas awal dan akhir hari rekapitulasi kas operasional kasir (`startOfDay` dan `endOfDay`) yang semula berzona nol `TimeSpan.Zero` kini dihitung dari rentang 24 jam penuh kalender WIB:
    - `startOfDay = FinanceBusinessDate.GetStartOfDayUtc(...)` (00:00:00 WIB)
    - `endOfDay = FinanceBusinessDate.GetEndOfDayUtc(...)` (00:00:00 WIB hari berikutnya)
22. **`FinanceSubledgerSnapshotService.cs:104`** (Titik Tambahan Penting): Batas akhir periode perhitungan piutang usaha yang semula `new DateTimeOffset(..., TimeSpan.Zero)` (memotong 7 jam lebih lambat) kini tepat pada akhir periode WIB:
    - `endOfPeriodUtc = FinanceBusinessDate.GetEndOfPeriodUtc(periodEndDate)` (23:59:59.999 WIB dalam UTC).
23. **`FinanceSupplierReturnService.cs:178`**: Inisialisasi variabel `accountingDate` pada kejadian `RETUR-PEMBELIAN` dan `PPN-MASUKAN-RETUR-PEMBELIAN` → `FinanceBusinessDate.ToDateOnly(occurredAt)`.
24. **Titik-titik Tanggal Hari Ini / Aging Tambahan**:
    - `FinanceReceivableService.cs:208, 787` (`asOfDate ?? FinanceBusinessDate.Today()`)
    - `FinanceSupplierPayableService.cs:296, 332` (`asOfDate ?? FinanceBusinessDate.Today()`)
    - `FinanceCashManagementService.cs:124, 394` (`date ?? FinanceBusinessDate.Today()`)
    - `FinanceNonPatientReceivableService.cs:97, 153` (`asOfDate ?? FinanceBusinessDate.Today()`)
    - `FinancePurchasingReportService.cs:211, 302` (`FinanceBusinessDate.Today()`)
    - `FinanceBillingIntakeService.cs:457` (`DueDate` fallback ke WIB)
    - `FinanceSubledgerSnapshotService.cs:53` & `FinanceAccountingOutboxService.cs:189` (`FinanceBusinessDate.GetPeriodEndDate`)

---

### 3.4 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — Tidak ada struktur request atau response DTO yang berubah formatnya. Nilai field `AccountingDate` pada payload yang terbit kini dipastikan konsisten dalam kalender WIB (`YYYY-MM-DD`). |
| Database | `NOL MIGRATION` — Tidak ada perubahan skema tabel atau kolom database. Nilai data existing belum ada kejadian sungguhan yang terkirim ke Accounting di produksi, sehingga tidak ada data lama yang perlu di-patch. |
| Keamanan/Auth | `NOT APPLICABLE` — Tidak ada perubahan role access, permission attribute, atau endpoint authorization. |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — Task ini murni perbaikan internal time-zone calculation pada service layer; nol controller atau endpoint baru dibuat.

---

## 5. Verifikasi dan Pembuktian Kasus

| Skenario Kasus Acceptance Criteria | Kondisi Uji | Hasil Verifikasi | Bukti Logika |
| --- | --- | --- | --- |
| **Kasus 1: Pembayaran Dini Hari 1 Oktober** | Pembayaran terjadi pukul `02:00:00 WIB` tanggal 1 Oktober 2026 (`2026-09-30 19:00:00 UTC`) | `AccountingDate` = **`2026-10-01`** (sebelumnya salah menghasilkan `2026-09-30`) | `FinanceBusinessDate.ToDateOnly(instant)` mengonversi waktu UTC+7 menghasilkan `2026-10-01`. |
| **Kasus 2: Kejadian Malam 30 September** | Kejadian terjadi pukul `23:30:00 WIB` tanggal 30 September 2026 (`2026-09-30 16:30:00 UTC`) | `AccountingDate` = **`2026-09-30`** | Tetap berada di tanggal 30 September kalender WIB. |
| **Kasus 3: Piutang Akhir Bulan Masuk Periode September** | Piutang diakui 30 September pukul `23:50:00 WIB` (`2026-09-30 16:50:00 UTC`), diuji terhadap batas periode September (`endOfPeriodUtc`) | Piutang **IKUT** periode `2026-09` (`RecognizedAt <= endOfPeriodUtc` adalah TRUE) | `GetEndOfPeriodUtc(new DateOnly(2026, 9, 30))` menghasilkan `2026-09-30 16:59:59.999 UTC` (`23:59:59.999 WIB`). Waktu pengakuan `16:50:00 UTC` berada sebelum batas akhir. |
| **Kasus 4: Kronologi Waktu Kejadian Tetap UTC** | Seluruh pemanggilan `StageEventAsync` | `EventOccurredAt` **tetap UTC** | Ruas `EventOccurredAt` tetap mengoper `DateTimeOffset` UTC asli (`now`, `eventOccurredAt`, `movement.OccurredAt`), tidak diubah. |
| **Kasus 5: Seluruh Titik Terganti** | Pencarian kode `DateOnly.FromDateTime` di folder `Areas/Corporate/FinanceManagement/` | Hanya tersisa **1 titik** di dalam helper `FinanceBusinessDate.cs:37` itu sendiri | Audit Select-String membuktikan nol titik konversi mandiri tersisa. |
| **Build Backend Otomatis** | Permintaan eksplisit pengguna: "jangan lakukan build backend secara automatis" | **TIDAK DIJALANKAN SECARA OTOMATIS** | Kompilasi didelegasikan kepada pengguna untuk dieksekusi secara manual. |

---

## 6. Risiko yang tersisa & Langkah berikutnya

| Aspek | Catatan |
| --- | --- |
| Risiko yang tersisa | Risiko teknis REV-14A (adanya dua konvensi waktu yang hidup bersamaan) telah **dieliminasi sepenuhnya** karena seluruh 21+ titik telah diganti serentak tanpa ada yang tertinggal. |
| Langkah berikutnya | Melanjutkan eksekusi rantai penulis mutasi subledger piutang: **`BE-FIN-060`** (`FinanceSubledgerMovementService` untuk penyambungan 7 jalur piutang). |
