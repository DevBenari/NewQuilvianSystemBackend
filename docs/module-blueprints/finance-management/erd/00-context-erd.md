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
