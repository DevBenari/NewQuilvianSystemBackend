# Finance Management — Peta Antar Bounded Context

| Field | Nilai |
|---|---|
| Blueprint ID | `FIN-BP-001` revisi `1` |
| Status | `draft` |
| Backend SHA | `09101d05` |

Dokumen ini menunjukkan **arah ketergantungan** antar konteks, bukan kolom. Kolom ada di empat
berkas ERD rinci dan di `data-dictionary.md`.

## 1. Arah ketergantungan

```mermaid
erDiagram
    BILLING ||--o{ FIN_BILLING_INTAKE : "mengirim fakta (satu arah)"
    MEDICAL_FEE ||--o{ FIN_PAYABLE : "fee yang sudah disetujui (satu arah)"
    ADMINISTRATOR ||--o{ FIN_PAYABLE : "MstSupplier (rujukan)"
    ADMINISTRATOR ||--o{ FIN_PURCHASING : "MstSupplier (rujukan)"
    FIN_BILLING_INTAKE ||--o{ FIN_RECEIVABLE : "melahirkan piutang"
    FIN_BILLING_INTAKE ||--o{ FIN_COLLECTION : "melahirkan penerimaan"
    FIN_COLLECTION ||--o{ FIN_RECEIVABLE : "mengalokasikan pelunasan"
    FIN_COLLECTION ||--o{ FIN_CASH : "mengisi posisi kas harian"
    FIN_MASTER_DATA ||--o{ FIN_CASH : "rekening tujuan setoran"
    FIN_MASTER_DATA ||--o{ FIN_PAYABLE : "rekening sumber pembayaran"
    FIN_PURCHASING ||--o{ FIN_PAYABLE : "Purchasing Invoice Approved -> FinSupplierPayable"
    BILLING ||--o{ FIN_RECEIVABLE : "dokumen per-invoice penjamin (rujukan, dipakai ulang)"
    FIN_RECEIVABLE ||--o{ FIN_ACCOUNTING_OUTBOX : "menerbitkan kejadian"
    FIN_COLLECTION ||--o{ FIN_ACCOUNTING_OUTBOX : "menerbitkan kejadian"
    FIN_PAYABLE ||--o{ FIN_ACCOUNTING_OUTBOX : "menerbitkan kejadian"
    FIN_PETTY_CASH ||--o{ FIN_ACCOUNTING_OUTBOX : "menerbitkan kejadian"
    FIN_PURCHASING ||--o{ FIN_ACCOUNTING_OUTBOX : "menerbitkan kejadian PPN Masukan (tertahan, menunggu ratifikasi)"
    FIN_ACCOUNTING_OUTBOX ||--o{ ACCOUNTING : "mengirim (satu arah, belum aktif)"
```

## 2. Yang dibaca setiap panah

| Panah | Yang dipertukarkan | Sifat |
|---|---|---|
| Billing → Billing Intake | `BilArHandoff`, `BilApHandoff`, `BilHandoffAdjustment`, `BilCollectionHandoff` | Satu arah. Finance memberi ACK, tidak menulis isi handoff |
| Medical Fee → Payable | `DoctorServiceFee` yang sudah disetujui | Satu arah. Finance tidak menghitung ulang nilai fee |
| Administrator → Payable | `MstSupplier` lewat `SupplierId` | Rujukan baca. Tidak disalin (`FIN-DEC-014`) |
| Billing Intake → Receivable | Piutang baru dari handoff AR | Di dalam satu transaksi |
| Billing Intake → Collection | Penerimaan baru dari handoff penerimaan | Di dalam satu transaksi |
| Collection → Receivable | `FinReceiptAllocation` mengurangi `OutstandingAmount` | Manual penuh (`FIN-DEC-011`) |
| Collection → Cash | Penerimaan tunai mengisi `CashReceiptAmount` kas harian | Diringkas saat penutupan hari |
| Master Data → Cash / Payable | `MstBankAccount` sebagai tujuan setoran dan sumber pembayaran | Rujukan baca |
| Empat konteks → Outbox | Baris kejadian ditulis di transaksi yang sama (`FIN-DES-017`) | Wajib satu transaksi |
| Outbox → Accounting | `POST` kejadian 12 field | **Belum aktif.** Endpoint penerima belum dibangun (`FIN-CAP-018`) |
| Administrator → Purchasing | `MstSupplier` lewat `SupplierId` | Rujukan baca, sama seperti Payable — tidak disalin |
| Purchasing → Payable | Purchasing Invoice `Approved` menciptakan `FinSupplierPayable` | `FIN-DEC-045`; jalur input manual tetap ada sebagai fallback (`FIN-DES-040`) |
| Billing → Receivable | Dokumen per-invoice penjamin (`BillingCompanyGuarantorInvoiceDocumentService`) dirujuk sebagai rincian baris `FinReceivableInvoiceBatch` | Layanan baca, tidak disalin (`FIN-CAP-030`) |
| Purchasing → Outbox | Kejadian `PPN-MASUKAN-PEMBELIAN` | **Tertahan** — menunggu ratifikasi Accounting (`FIN-OQ-020`, `FIN-DEC-046`) |

## 3. Yang sengaja tidak ada panahnya

| Yang tidak terhubung | Alasan |
|---|---|
| Accounting → Finance | Kontrak `ACC-XMOD-0.2` satu arah. Accounting adalah muara |
| Finance → Billing | Finance tidak pernah menulis ke tabel Billing, termasuk `BilCashierShift` (aturan bisnis #12) |
| Petty Cash ↔ Collection | Kas kecil dan kas kasir adalah dua kolam terpisah. Pencairan kas kecil MUST NOT mengubah kas kasir, dan sebaliknya (aturan bisnis #15) |
| Finance → Medical Fee | Finance tidak pernah mengubah status atau nilai fee dokter |
| Finance → Billing (Purchasing/AP) | `FIN_PURCHASING` tidak pernah menulis ke tabel Billing; dokumen per-invoice penjamin (`BillingCompanyGuarantorInvoiceDocumentService`) dipanggil murni sebagai layanan baca (`FIN-CAP-030`) |

## 4. Daftar berkas ERD rinci

| Berkas | Konteks yang dirinci |
|---|---|
| `receivable-collection.md` | Billing Intake, Receivable, Collection, AR Invoice Agregat (AMENDMENT REVISI 4), Potongan AR (AMENDMENT REVISI 4) |
| `payable.md` | Supplier Payable, Doctor Payable, Payment |
| `purchasing-ap.md` | **Baru (AMENDMENT REVISI 4)** — Purchase Order, Tanda Terima Barang, Tukar Faktur, Purchasing Invoice, Retur Pembelian, Deposit Retur |
| `cash-and-master-data.md` | Bank Deposit, Daily Cash, dan seluruh master Finance |
| `accounting-integration.md` | Outbox, Attempt, Subledger Period Balance |
| `data-dictionary.md` | Kamus data seluruh kolom dan bentuk DDL |

---

## Revisi 14 — Peta konteks sesudah buku mutasi dan jalur pengiriman

Dua hal berubah pada peta konteks, dan keduanya bukan penambahan entity:

1. **Arah ketergantungan ke Accounting menjadi dua arah secara nyata.** Sebelum revisi 14, Finance
   hanya **menulis** baris kotak keluar yang tidak pernah terkirim. Sesudah `FIN-DES-078`, Finance
   benar-benar memanggil kotak masuk Accounting.
2. **Posisi saldo berpindah sumber.** Tiga buku mutasi menjadi lapisan di dalam Finance yang
   menjawab "posisi pada tanggal", memisahkan pertanyaan itu dari agregat yang hanya menyimpan
   posisi *sekarang*.

```mermaid
erDiagram
    BILLING_KASIR {
        string BilCashierShift "dibaca saja"
        string BilArHandoff "dibaca saja"
        string BilApHandoff "dibaca saja"
    }
    FINANCE_SUBLEDGER {
        string FinReceivable "posisi sekarang"
        string FinSupplierPayable "posisi sekarang"
        string FinReceivableMovement "BARU posisi per tanggal"
        string FinSupplierPayableMovement "BARU posisi per tanggal"
        string FinCashMovement "BARU posisi kas per tanggal"
    }
    FINANCE_CUTOVER {
        string FinOpeningBalance "BARU saldo awal"
        string FinOpeningItemBatch "BARU batch migrasi"
        string FinSubledgerControlAccountMap "BARU pemetaan akun"
    }
    FINANCE_OUTBOX {
        string FinAccountingEventOutbox "kotak keluar"
        string FinAccountingEventAttempt "riwayat kirim"
    }
    ACCOUNTING {
        string AccAccountingEvent "kotak masuk, milik Accounting"
        string ChartOfAccounts "bagan akun, milik Accounting"
    }
    BILLING_KASIR ||..o{ FINANCE_SUBLEDGER : "fakta masuk, nol tulisan balik"
    FINANCE_CUTOVER ||..o{ FINANCE_SUBLEDGER : "titik awal dan item migrasi"
    FINANCE_SUBLEDGER ||..o{ FINANCE_OUTBOX : "kejadian dan saldo"
    FINANCE_CUTOVER ||..o{ FINANCE_OUTBOX : "pemetaan menentukan baris saldo"
    FINANCE_OUTBOX ||..o{ ACCOUNTING : "BARU dikirim worker, satu arah"
    ACCOUNTING ||..o{ FINANCE_CUTOVER : "kode akun control (G2) dan saldo awal (G5), lewat surat"
```

### Batas yang MUST dijaga pada peta ini

| Batas | Isi |
|---|---|
| Finance → Billing | **Nol tulisan.** Shift, handoff, dan tender dibaca saja (`FIN-OOS-001`..`004`) |
| Finance → Accounting | **Satu arah**, lewat kotak masuk yang sudah dikontrakkan. Finance **MUST NOT** membaca tabel `Acc*` — termasuk saldo awal, yang karena itu **dinyatakan petugas** (`FIN-DES-090`) |
| Accounting → Finance | Lewat **surat**, bukan kode: kode akun control definitif (G2) dan angka saldo awal (G5) |
| Saldo rekening bank | **Milik Accounting** (`FIN-DEC-137`). Finance menyimpan master rekening dan identitas rekening pada transaksi, bukan saldonya |
| Posisi per tanggal | **Hanya** dari buku mutasi. Membaca `OutstandingAmount` atau `ClosingBalance` sebagai jawaban akan menghidupkan kembali cacat yang revisi 14 perbaiki |
