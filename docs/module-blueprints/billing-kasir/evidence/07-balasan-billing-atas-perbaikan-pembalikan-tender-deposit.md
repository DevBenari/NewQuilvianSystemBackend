# Balasan Billing untuk Finance — Penyelesaian Pembalikan Tender Top-Up Deposit dan Kompensasi Alokasi Tagihan

| Field | Nilai |
|---|---|
| Dari | Owner / Penggarap Modul Billing dan Kasir (`billing-kasir`, `BIL-CASH-001`) |
| Untuk | Yasmin, owner / penggarap modul Finance (AR/AP) |
| Tembusan | Owner Modul Accounting (`acc-general-ledger`, `ACC-GL-001`) |
| Tanggal | 28 September 2026 |
| Sifat | **Dokumen Balasan dan Penjelasan Solusi Teknis Terpasang.** Menjawab tuntas surat temuan pada `evidence/17` |
| Rujukan Temuan | `docs/module-blueprints/finance-management/evidence/17-permintaan-perbaikan-pembalikan-tender-deposit-untuk-billing.md` (`FIN-OQ-034`) |
| Status Masalah | **SELESAI & DITUTUP** via keputusan bisnis `BKC-DEC-128`–`131`, `FIN-DEC-077`, arsitektur `BKC-DES-051`–`054`, kontrak integrasi `BIL-INT-018`, dan implementasi backend `BE-BKC-079` |

Berkas ini berdiri sendiri dan dapat dibaca tanpa harus membuka blueprint teknis Billing.

---

## 1. Ringkasan Eksekutif untuk Pembaca Umum

Terima kasih atas surat temuan yang sangat jelas dan konstruktif dari tim Finance (`evidence/17`). Temuan Anda tepat sasaran: ketika sebuah tender pembayaran uang muka (deposit) non-tunai ditarik kembali oleh pihak perbankan (*settlement reversed / chargeback*), sistem Billing sebelumnya tidak mencatatkan mutasi pembalik apa pun pada deposit pasien. Celah ini semakin kritis jika dana deposit tersebut telah dipakai kasir untuk melunasi tagihan pasien (status tagihan sudah `CLOSED`), karena jalur pembalikan deposit lama menolak transaksi tersebut demi mencegah saldo minus, sehingga rumah sakit menanggung kerugian piutang yang lenyap tanpa ada uang riil yang masuk.

Menjawab pertanyaan inti Anda pada Bagian 5 `evidence/17`:
> *"Ketika tender yang mendanai top-up deposit menjadi `REVERSED`, apa yang seharusnya terjadi pada saldo deposit pasien — terutama bila dananya sudah terpakai melunasi tagihan?"*

Kami telah mengambil keputusan resmi bersama Product Owner (**`BKC-DEC-128`–`131`**, disahkan 28 September 2026), memperbarui arsitektur dan kontrak modul, serta mengimplementasikan perbaikannya secara tuntas di backend Billing (**`BE-BKC-079`**). 

**Inti Keputusan & Solusi yang Kami Terapkan:**
1. **Pilihan Solusi yang Diambil:** Kami memilih **kemungkinan kedua** yang Anda usulkan, dengan otomatisasi penuh berprinsip **LIFO (*Last In First Out*)**: jika saldo deposit tidak cukup untuk membalikkan nilai top-up, sistem **secara otomatis membatalkan alokasi pembayaran tagihan pasien dari yang paling baru**, memulihkan saldo deposit sementara ke kolam akun, lalu memotong saldo deposit secara utuh.
2. **Integritas Saldo Non-Negatif (`BKC-DEC-128`):** Saldo deposit pasien (`AvailableBalance`) **dijamin tidak pernah bernilai minus (< Rp 0)** dalam keadaan apa pun.
3. **Pemulihan Piutang Otomatis (`BKC-DEC-130`):** Tagihan invoice pasien yang alokasinya ditarik otomatis beralih status dari **`CLOSED` kembali ke `FINAL`**, sehingga sisa tagihan kembali muncul di kasir sebagai piutang terbuka yang wajib ditagihkan ulang.
4. **Penerbitan Mutasi Ganda untuk Finance (`BKC-DEC-131`, `BIL-INT-018`):** Sistem mencatat dua mutasi terpisah pada `BilDepositMovement`:
   - Mutasi **`RELEASE`**: mencatat pengembalian dana pemakaian alokasi tagihan ke kolam deposit.
   - Mutasi **`REVERSAL`**: mencatat penarikan dana deposit top-up ke perbankan (`ReversesMovementId` bertaut ke mutasi `TOP_UP` awal).
5. **Blocker `FIN-OQ-034` DITUTUP:** Dengan terbitnya mutasi berpasangan ini, seluruh kebutuhan fakta untuk kejadian akuntansi `PEMBALIKAN-PENERIMAAN-UANG-MUKA` dan `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT` kini tersedia secara lengkap dan deterministik.

---

## 2. Contoh Berangka di Rumah Sakit: Sebelum vs Sesudah Perbaikan

Untuk memberikan gambaran yang transparan, berikut perbandingan alur menggunakan contoh angka yang Anda sampaikan pada `evidence/17` Bagian 3:

*Pasien menyetor uang muka deposit Rp 20.000.000 via kartu debit. Total tagihannya Rp 32.000.000.*

### Keadaan Sebelum Perbaikan (Temuan Anda):
1. Tender kartu debit Rp 20.000.000 `SUCCEEDED` → Mutasi `TOP_UP` Rp 20.000.000 ditulis, saldo deposit Rp 20.000.000.
2. Deposit dialokasikan ke tagihan → Mutasi `ALLOCATION` Rp 20.000.000; saldo deposit Rp 0; sisa tagihan pasien turun menjadi Rp 12.000.000.
3. Bank membatalkan transaksi kartu debit; status tender menjadi `REVERSED`.
4. **Hasil Lama (Celah):** Tidak ada mutasi deposit yang ditulis. Saldo deposit tetap Rp 0. Tagihan pasien tetap tercatat telah terbayar Rp 20.000.000. Rumah sakit kehilangan Rp 20.000.000 tanpa jejak, dan sinkronisasi Finance terblokir.

### Keadaan Sesudah Perbaikan (`BE-BKC-079` Terpasang):
1. Tender kartu debit Rp 20.000.000 `SUCCEEDED` → Mutasi `TOP_UP` Rp 20.000.000 ditulis, saldo deposit Rp 20.000.000.
2. Deposit dialokasikan ke tagihan → Mutasi `ALLOCATION` Rp 20.000.000; saldo deposit Rp 0; sisa tagihan pasien Rp 12.000.000.
3. Bank membatalkan transaksi kartu debit; status tender menjadi `REVERSED`.
4. **Eksekusi Otomatis Sistem Billing:**
   - Sistem mendeteksi `deficit = tender.Amount - AvailableBalance` = Rp 20.000.000 - Rp 0 = **Rp 20.000.000**.
   - Sistem mencari alokasi tagihan aktif dan membatalkan alokasi Rp 20.000.000 tersebut dengan menerbitkan baris kompensasi pembalik `BilPaymentAllocation` bertaut `ReversesAllocationId`.
   - Sistem mencatat baris mutasi `BilDepositMovement` tipe **`RELEASE`** sebesar **+Rp 20.000.000**, memulihkan saldo deposit menjadi Rp 20.000.000.
   - Sistem mencatat baris mutasi `BilDepositMovement` tipe **`REVERSAL`** sebesar **-Rp 20.000.000** bertaut `ReversesMovementId` ke mutasi `TOP_UP` awal, memotong saldo deposit kembali ke **Rp 0** (saldo akhir aman dan non-negatif).
   - Sistem memanggil `SyncClosureAsync` untuk tagihan invoice: sisa tagihan pasien naik kembali dari Rp 12.000.000 menjadi **Rp 32.000.000** (piutang terbuka kembali utuh), dan status tagihan yang sempat tertutup diselaraskan kembali ke **`FINAL`**.
   - Sistem menerbitkan sinyal pencabutan kelayakan finansial rawat inap (`InpatientClearance`) dengan alasan `PaymentReversed`.
5. **Hasil Baru untuk Finance & Accounting:**
   - Finance membaca mutasi `RELEASE` Rp 20.000.000 → Menerbitkan kejadian `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT`.
   - Finance membaca mutasi `REVERSAL` Rp 20.000.000 → Menerbitkan kejadian `PEMBALIKAN-PENERIMAAN-UANG-MUKA`.
   - Accounting membukukan jurnal berpasangan: Kas berkurang Rp 20.000.000, Piutang Pasien pulih kembali Rp 20.000.000.
   - Angka pembukuan buku besar, kas fisik bank, dan sistem billing kasir seimbang 100%.

---

## 3. Rincian Revisi Dokumen & Blueprint yang Telah Kami Lakukan

Sebagai tindak lanjut komprehensif, kami tidak hanya memperbaiki source code, melainkan memperbarui seluruh siklus tata kelola rekayasa Quilvian pada modul `billing-kasir`:

### 3.1 Keputusan Bisnis Baru (`00-interview-decisions.md`)
Kami merumuskan dan mengesahkan 4 keputusan bisnis baru (**`BKC-DEC-128`–`131`**) melalui sesi wawancara terstruktur (`/grill-me`):
- **`BKC-DEC-128`:** Perlindungan saldo deposit non-negatif (`AvailableBalance >= 0`). Penarikan saldo top-up yang melebihi saldo tersedia wajib menarik dana alokasi tagihan terlebih dahulu.
- **`BKC-DEC-129`:** Algoritma pembatalan alokasi tagihan wajib menggunakan urutan LIFO (*Last In First Out* — alokasi terbaru dibatalkan lebih dulu) untuk meminimalkan dampak pada tagihan periode lama yang sudah tutup buku.
- **`BKC-DEC-130`:** Penyelarasan status invoice pasien yang alokasinya ditarik kembali ke status `FINAL` via `SyncClosureAsync`, serta pencabutan izin kepulangan rawat inap (`PaymentReversed`).
- **`BKC-DEC-131`:** Pencatatan mutasi ganda `RELEASE` dan `REVERSAL` pada `BilDepositMovement` sebagai fakta transaksi resmi untuk Finance.

### 3.2 Dokumen Arsitektur Backend (`02-backend-architecture.md`)
Menambahkan sub-bab khusus pembalikan tender top-up deposit dan meratifikasi 4 keputusan arsitektur:
- **`BKC-DES-051`:** Eksekusi atomik `HandleDepositTopUpReversalAsync` dalam transaksi database serializable dan advisory lock PostgreSQL per akun deposit (`BIL_DEPOSIT_{accountId:N}`).
- **`BKC-DES-052`:** Mekanisme penulisan baris kompensasi alokasi pada `BilPaymentAllocation` bertaut `ReversesAllocationId` yang mematuhi constraint database `CK_BilPaymentAllocation_Amount > 0`.
- **`BKC-DES-053`:** Orkestrasi penyelarasan status penutupan tagihan `SyncClosureAsync` pasca-mutasi alokasi.
- **`BKC-DES-054`:** Penerbitan mutasi berpasangan `RELEASE` & `REVERSAL` dengan idempotency key dan correlation ID deterministik berbasis hash SHA-256.

### 3.3 Kontrak Modul & Spesifikasi Teknis
- **`contracts/state-transition-matrix.md` (`BIL-STATE-1.5`):** Menetapkan matriks transisi status `BilPaymentAllocation` (alokasi pembalik kompensasi) dan `BilDepositMovement` (`RELEASE` & `REVERSAL`).
- **`contracts/validation-matrix.md` (`BIL-VALIDATION-1.5`):** Mendaftarkan aturan validasi `BIL-VAL-127` (saldo non-negatif), `BIL-VAL-128` (sekuens LIFO), `BIL-VAL-129` (guard status tender awal wajib `SUCCEEDED` & idempotensi), `BIL-VAL-130` (integritas foreign key pembalik), dan `BIL-VAL-131` (rollback atomik).
- **`contracts/integration-contract.md` (`BIL-INTEGRATION-1.3`):** Memperbarui spesifikasi antarmuka integrasi **`BIL-INT-018`** (`DEPOSIT_MOVEMENT`), memetakan konsumsi mutasi `RELEASE` dan `REVERSAL` oleh `FinBillingHandoffIntake`.
- **`data/data-dictionary.md`:** Mendokumentasikan tipe mutasi baru `RELEASE` dan `REVERSAL` pada entitas `BilDepositMovement`.
- **`flowcharts/pembalikan-tender-deposit.md`:** Menyusun flowchart interaktif alur penarikan deposit, pembatalan alokasi LIFO, dan penanganan jalur perkecualian.
- **`testing/acceptance-test-matrix.md` (`BIL-TEST-1.6`):** Menetapkan 8 acceptance criteria komprehensif (**`BIL-AT-149`–`156`**).
- **`04-prd-to-mvp.md` & `blueprint-manifest.md`:** Mendaftarkan gelombang rilis **`MVP-35`** pada manifest revisi `1.8`.
- **`roadmap/backend-roadmap.md` & `requirement-traceability.md`:** Memetakan task **`BE-BKC-079`** dengan ketertelusuran penuh ke kebutuhan fungsional `FR-BKC-125`–`128`.

---

## 4. Rincian Implementasi Source Code Backend (`BE-BKC-079`)

Implementasi telah kami selesaikan pada:
`NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Billing/Services/BillingSettlementService.cs`

Berikut poin-poin teknis yang telah terpasang dan aktif di kode:

1. **Titik Pemicu (`UpdateTenderStatusAsync`):**
   Ketika provider atau kasir memperbarui status tender menjadi `REVERSED` dari status awal `SUCCEEDED` pada settlement bertipe `DEPOSIT_TOP_UP`:
   ```csharp
   if (targetStatus == BillingTenderStatuses.Reversed
       && beforeTenderStatus == BillingTenderStatuses.Succeeded
       && tender.Settlement.Purpose == BillingSettlementPurposes.DepositTopUp)
   {
       reversedInvoiceClosureChanges = await HandleDepositTopUpReversalAsync(
           tender, actorUserId, result.OccurredAt, cancellationToken);
   }
   ```
2. **Penjaga Idempotensi & Validasi Status Awal:**
   - Tender yang belum pernah `SUCCEEDED` (misal masih `PENDING` atau `FAILED`) ditolak dengan pesan: *"Hanya tender berhasil yang dapat direversal."* (`BIL-VAL-129`, `BIL-AT-156`).
   - Jika mutasi `REVERSAL` untuk tender ini sudah pernah dicatat, sistem me-replay tanpa duplikasi mutasi (`BIL-AT-155`).
3. **Penyelidikan & Pembatalan Alokasi LIFO:**
   - Sistem mendeteksi defisit saldo deposit (`deficit = tender.Amount - account.AvailableBalance`).
   - Jika defisit > 0, sistem mengambil seluruh alokasi tagihan aktif yang bersumber dari akun deposit terkait dan mengurutkannya secara LIFO:
     `ORDER BY AllocatedAt DESC, CreateDateTime DESC, Id DESC`.
   - Sistem membuat baris `BilPaymentAllocation` baru dengan nominal positif `Amount = cancelAmount`, `ReversesAllocationId = original.Id`, dan `AllocatedAt = occurredAt`.
   - Database check constraint `CK_BilPaymentAllocation_Amount > 0` tetap terpenuhi, sementara seluruh kueri sisa tagihan Billing secara otomatis menghitung baris ber-`ReversesAllocationId` sebagai pengurang alokasi (`x.ReversesAllocationId.HasValue ? -x.Amount : x.Amount`).
4. **Pencatatan Mutasi Ganda di `BilDepositMovement`:**
   - **Baris `RELEASE`:** Diterbitkan jika ada alokasi tagihan yang dibatalkan, sebesar `totalReleasedFromAllocations`, dengan `IdempotencyKey` dan `CorrelationId` deterministik (`REL_IDEMPOTENCY_{tenderId}`, `REL_CORRELATION_{tenderId}`).
   - **Baris `REVERSAL`:** Diterbitkan sebesar `tender.Amount`, dengan `ReversesMovementId = originalTopUpMovement.Id`, serta `IdempotencyKey` dan `CorrelationId` deterministik (`REV_IDEMPOTENCY_{tenderId}`, `REV_CORRELATION_{tenderId}`).
   - Saldo deposit pasien diperbarui: `account.AvailableBalance = account.AvailableBalance + totalReleased - tender.Amount`. Dipastikan `AvailableBalance >= 0`.
5. **Penyelarasan Status Tagihan Invoice (`SyncClosureAsync`):**
   - Untuk setiap tagihan invoice yang alokasinya ditarik, sistem memanggil `_closureService.SyncClosureAsync(..., PrescriptionClearanceReasonCodes.PaymentReversed)`.
   - Karena alokasi pembayaran ditarik, sisa tagihan pasien kembali menjadi > 0, sehingga status invoice otomatis beralih dari `CLOSED` kembali ke **`FINAL`** (`BKC-DEC-130`, `BIL-AT-152`).
   - Menerbitkan surat clearance kelayakan pasien ke Farmasi dan Rawat Inap dengan alasan `PaymentReversed`.
6. **Nol Migration:**
   - Perubahan ini murni pada logika transaksi dan orkestrasi service existing. **Nol tabel baru, nol kolom baru, dan nol migration database EF Core.**

---

## 5. Bentuk Data Mutasi yang Dapat Finance Konsumsi Sekarang

Sesuai permintaan Anda pada Bagian 5 `evidence/17`, berikut bentuk konkret record yang ditulis Billing ke tabel `BilDepositMovement` dan siap dibaca oleh `FinBillingHandoffIntake`:

### 5.1 Mutasi Pengembalian Alokasi (`RELEASE`)
Ditulis saat ada alokasi tagihan pasien yang dibatalkan untuk menutup defisit saldo deposit:

| Kolom di `BilDepositMovement` | Nilai yang Ditulis | Penjelasan untuk Finance |
|---|---|---|
| `DepositAccountId` | `account.Id` | ID akun deposit pasien terkait |
| `MovementType` | `"RELEASE"` | Tipe mutasi resmi pelepasan alokasi tagihan |
| `Amount` | Nilai total alokasi yang dibatalkan (> 0) | Nominal dana tagihan yang dikembalikan ke kolam deposit |
| `SettlementId` | `tender.SettlementId` | ID settlement top-up yang memicu penarikan |
| `PaymentMethodId` | `tender.PaymentMethodId` | Metode pembayaran asal tender |
| `IdempotencyKey` | Deterministik berbasis `tender.Id` | Aman diulang (*replay-safe*) |
| `OccurredAt` | Waktu callback/reversal perbankan | Waktu kejadian pembalikan |
| `Reason` | `"Pelepasan alokasi tagihan untuk pembalikan tender top-up deposit."` | Keterangan audit |
| `ReversesMovementId` | `null` | Pelepasan alokasi tidak menunjuk langsung ke top-up |

> **Pemicu Jurnal Finance:** Mutasi ini adalah fakta pemicu kejadian **`PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT`** ke Accounting.

### 5.2 Mutasi Pembalikan Top-Up (`REVERSAL`)
Ditulis untuk membalikkan setoran uang muka top-up perbankan yang dibatalkan:

| Kolom di `BilDepositMovement` | Nilai yang Ditulis | Penjelasan untuk Finance |
|---|---|---|
| `DepositAccountId` | `account.Id` | ID akun deposit pasien terkait |
| `MovementType` | `"REVERSAL"` | Tipe mutasi resmi penarikan uang muka deposit |
| `Amount` | `tender.Amount` (> 0) | Nominal penuh tender top-up yang ditarik bank |
| `SettlementId` | `tender.SettlementId` | ID settlement top-up yang ditarik |
| `PaymentMethodId` | `tender.PaymentMethodId` | Metode pembayaran asal tender |
| `IdempotencyKey` | Deterministik berbasis `tender.Id` | Aman diulang (*replay-safe*) |
| `OccurredAt` | Waktu callback/reversal perbankan | Waktu kejadian pembalikan |
| `Reason` | `"Pembalikan tender top-up deposit."` | Keterangan audit |
| `ReversesMovementId` | `originalTopUpMovement.Id` | **Terisi!** Menunjuk tepat ke ID mutasi `TOP_UP` awal |

> **Pemicu Jurnal Finance:** Mutasi ini adalah fakta pemicu kejadian **`PEMBALIKAN-PENERIMAAN-UANG-MUKA`** ke Accounting.

---

## 6. Dampak terhadap Pendeteksi Baca-Saja & Pantauan Finance

Menjawab Bagian 6 pada surat Anda (`evidence/17`):
1. **Pemeriksaan Baca-Saja Anda Akan Lolos:** Pendeteksi baca-saja yang Anda pasang di Finance — yang memeriksa keberadaan mutasi pembalik saat tender top-up berstatus `REVERSED` — kini akan menemukan mutasi `REVERSAL` yang bersesuaian secara konsisten.
2. **Baris Gagal Akan Bersih:** Baris sinkronisasi Finance yang sebelumnya ditandai gagal kini akan berhasil diproses (*succeeded*) secara otomatis tanpa galat.
3. **Kejadian Terbit Sempurna:** Dua kejadian akuntansi yang sebelumnya tertahan kini dapat diterbitkan dengan pasangan jurnal yang benar:
   - Kejadian `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT` dipicu oleh mutasi `RELEASE`.
   - Kejadian `PEMBALIKAN-PENERIMAAN-UANG-MUKA` dipicu oleh mutasi `REVERSAL`.
4. **Lawan Jurnal Accounting:** Lawan jurnal yang sudah Anda sepakati bersama owner Accounting (Debit Piutang Pasien, Kredit Kas/Bank) kini sepenuhnya sejalan dengan keadaan fisik di lapangan: kas/bank berkurang di rekening koran, dan piutang pasien di kasir Billing benar-benar kembali terbuka untuk ditagihkan.

---

## 7. Status Blocker dan Langkah Selanjutnya

| Butir | Status Sebelumnya | Status Sekarang | Tindakan Selanjutnya |
|---|:---:|:---:|---|
| Temuan Celah Pembalikan Deposit | Belum tertangani | **SELESAI (`BE-BKC-079`)** | Source code terpasang di `BillingSettlementService.cs` |
| Isu Integrasi `FIN-OQ-034` | Open / Blocker | **DITUTUP (`FIN-DEC-077`)** | Kontrak `BIL-INT-018` resmi berlaku |
| Task Finance `BE-FIN-036` | Tertahan | **UNBLOCKED** | Finance dapat melanjutkan implementasi intake `DEPOSIT_MOVEMENT` |
| Rilis Gelombang `MVP-35` Billing | Draft | **TERLAKSANA** | Laporan tracked tersedia di `BE-BKC-079.md` |

Dengan surat balasan ini, koordinasi lintas modul terkait pembalikan tender top-up deposit kami nyatakan **tuntas**. Tim Finance dipersilakan melanjutkan pengerjaan intake kejadian deposit pada task `BE-FIN-036` dengan tenang.

Bila ada hal teknis atau pengujian integrasi yang membutuhkan data uji bersama di sandbox, silakan hubungi kami kapan saja.

---

## 8. Dokumen Rujukan

| Berkas | Lokasi | Keterangan |
|---|---|---|
| Laporan Perubahan Backend `BE-BKC-079` | `docs/module-blueprints/billing-kasir/task/report/backend/BE-BKC-079.md` | Laporan implementasi lengkap dengan analisis statis 8 kriteria penerimaan |
| Dokumen Arsitektur Billing | `docs/module-blueprints/billing-kasir/02-backend-architecture.md` | Keputusan arsitektur `BKC-DES-051`–`054` |
| Kontrak Integrasi Billing | `docs/module-blueprints/billing-kasir/contracts/integration-contract.md` | Kontrak antarmuka `BIL-INT-018` (`DEPOSIT_MOVEMENT`) |
| Flowchart Pembalikan Deposit LIFO | `docs/module-blueprints/billing-kasir/flowcharts/pembalikan-tender-deposit.md` | Alur proses bisnis dan jalur perkecualian visual |
| Roadmap Backend Billing | `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` | Bagian Gelombang `MVP-35` |
