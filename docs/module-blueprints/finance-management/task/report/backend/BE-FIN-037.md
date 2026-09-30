# Laporan Task Backend — BE-FIN-037

| Field | Nilai |
|---|---|
| **Task ID** | `BE-FIN-037` |
| **Judul** | Empat laporan Purchasing/AP dari data yang sudah ada |
| **Status** | 🟡 SEBAGIAN — source selesai, `dotnet build` tertunda milik pengguna |
| **Tanggal** | 29 September 2026 |
| **Backend Owner** | Backend Owner |

---

## 1. Backend Governance Preflight

| Field | Nilai |
|---|---|
| **Area** | Corporate |
| **Module / Submodule** | Finance Management / Purchasing |
| **Applicability** | `NEW CODE` |
| **Registry** | Terdaftar sejak `BE-FIN-027 ✅` — prefix `Fin`, status `ACTIVE` |
| **QBE ID berlaku** | QBE-NAM-004 (konsistensi prefix), QBE-MOD-003 (ownership terdaftar) |
| **Kontrak sumber** | `FIN-API-1.1` §B.6 (locked 25 September 2026), `FIN-PERM-1.1` §B.5 |
| **Keputusan relevan** | `FIN-DEC-059` (`/aging` dicabut), `FIN-DES-044` (nol tabel baru) |

---

## 2. Scope Implementasi

Kontrak menutup empat endpoint GET read-only di base URL
`api/v1/corporate/finance-management/purchasing/reports`:

| Endpoint | Response type | Status |
|---|---|---|
| `GET /summary` | `ApiResponse<PurchasingSummaryResponse>` | Dibuat |
| `GET /invoice-exchanges` | `ApiResponse<PagedResult<InvoiceExchangeReportResponse>>` | Dibuat |
| `GET /due-dates` | `ApiResponse<PagedResult<DueDateReportResponse>>` | Dibuat |
| `GET /reconciliation` | `ApiResponse<PagedResult<ReconciliationResponse>>` | Dibuat |
| GET /aging | DICABUT FIN-DEC-059 — tidak dibuat | — |

---

## 3. File yang Berubah/Dibuat

| File | Jenis | Keterangan |
|---|---|---|
| `Areas/.../Purchasing/Dtos/FinancePurchasingReportDtos.cs` | BARU | DTO query dan response keempat laporan (read-only) |
| `Areas/.../Purchasing/Services/FinancePurchasingReportService.cs` | BARU | Service keempat laporan — AsNoTracking seluruh query |
| `Areas/.../Purchasing/Controllers/FinancePurchasingReportsController.cs` | BARU | Controller empat endpoint GET — nol perintah pengubah |
| `Areas/.../Billing/BillingManagementServiceCollectionExtensions.cs` | DIUBAH | Tambah `AddScoped<FinancePurchasingReportService>()` |

---

## 4. Detail Teknis per Endpoint

### 4.1 GET /summary

- **Query**: `PurchasingSummaryQuery` — `PeriodFrom`, `PeriodTo`, `SupplierId` (semuanya nullable)
- **Proyeksi**: Lima DbSet — `FinPurchaseOrders`, `FinGoodsReceipts`, `FinInvoiceExchanges`, `FinPurchasingInvoices`, `FinSupplierPayables`
- **Catatan**: `FinGoodsReceipt` tidak punya `SupplierId` langsung — filter lewat navigation `PurchaseOrder.SupplierId`
- **Utang bersumber Purchasing**: hanya `FinSupplierPayable` yang `SourcePurchasingInvoiceId` terisi dan status bukan `PAID`/`CANCELLED`

### 4.2 GET /invoice-exchanges

- **Query**: `InvoiceExchangeReportQuery` — paging, `SupplierId`, `Status`, `ReceivedDateFrom`, `ReceivedDateTo`
- **Join**: Left-join ke `PurchasingInvoice` lewat navigation `FinInvoiceExchange.PurchasingInvoice` — Tukar Faktur tanpa invoice tetap muncul
- **Urutan**: `ReceivedDate` DESC, `ExchangeNumber` DESC

### 4.3 GET /due-dates

- **Query**: `DueDateReportQuery` — paging, `SupplierId`, `Status`, `DueDateFrom`, `DueDateTo`, `OverdueOnly`
- **Sumber jatuh tempo**: `EstimatedDueDate` dari `FinInvoiceExchange`; `ActualDueDate` dari `FinSupplierPayable.DueDate`
- **DaysUntilDue**: dihitung di service dari `(actualDue ?? estimatedDue).DayNumber - today.DayNumber`; positif = masih N hari; negatif = sudah lewat
- **Lookup payable**: satu query `ToDictionaryAsync` untuk invoice yang sudah di-page (bukan N+1)

### 4.4 GET /reconciliation

- **Query**: `ReconciliationQuery` — paging, `SupplierId`, `ReceivedDateFrom`, `ReceivedDateTo`, `OverdueOnly`
- **Filter wajib**: hanya `FinInvoiceExchange` berstatus `RECEIVED` (belum terhubung ke invoice)
- **DaysUntilDue**: `EstimatedDueDate.DayNumber - today.DayNumber`; urutan `EstimatedDueDate` ASC

---

## 5. Otorisasi

| Endpoint | Resource | Action | String `[AccessPermission]` |
|---|---|---|---|
| Keempat endpoint | `FinancePurchasingReport` | `Read` | `[AccessPermission("FinancePurchasingReport", "Read")]` |

Sesuai `FIN-PERM-1.1` §B.5 persis. Nol action mutasi.

---

## 6. Acceptance Criteria — Pemetaan ke Source

| Kriteria | Status | Bukti |
|---|---|---|
| Rekonsiliasi menampilkan Tukar Faktur yang belum menjadi invoice | Terpenuhi | `GetReconciliationAsync` filter `Status == RECEIVED` |
| Laporan jatuh tempo memakai `EstimatedDueDate`/`DueDate` dari backend | Terpenuhi | `EstimatedDueDate` dari `InvoiceExchange`, `ActualDueDate` dari `FinSupplierPayable.DueDate` |
| Seluruhnya read-only | Terpenuhi | Controller hanya `[HttpGet]`; service hanya `AsNoTracking` |
| Nol perintah pengubah pada controller laporan | Terpenuhi | Tidak ada `[HttpPost]`, `[HttpPut]`, `[HttpPatch]`, `[HttpDelete]` |
| `/aging` sengaja dikeluarkan | Terpenuhi | Tidak ada endpoint `/aging` sesuai `FIN-DEC-059` |
| Nol tabel laporan baru | Terpenuhi | `FIN-DES-044` — seluruh data dari entity existing |

---

## 7. Validasi

| Jenis | Status | Catatan |
|---|---|---|
| `dotnet build` | Belum — menunggu pengguna | Sesuai instruksi "jangan lakukan build otomatis" |
| Review source — nol perintah pengubah | Lulus | Controller 4 `[HttpGet]`; service 4 metode read-only |
| Review otorisasi — `[AccessPermission]` = `ControllerName` | Lulus | Keduanya `"FinancePurchasingReport"` |
| Review DI — registrasi service | Lulus | `services.AddScoped<FinancePurchasingReportService>()` di extension |

---

## 8. Keadaan Migration/Database

Tidak ada. `FIN-DES-044` menetapkan nol tabel laporan baru.

---

## 9. Risiko Tersisa

| # | Risiko | Mitigasi |
|---|---|---|
| 1 | `BE-FIN-034`/`035` masih 🟡 — laporan mengembalikan hasil kosong sampai data Purchasing ada di DB | Laporan read-only, kosong adalah hasil sah |
| 2 | `OverdueOnly` diterapkan setelah paging di memory — jumlah item bisa kurang dari `PageSize` | Konsisten dengan pola service lain; perlu dicatat untuk FE |
| 3 | `DaysUntilDue` bergantung `DateTime.Today` server | Zona waktu server MUST konsisten dengan operasional RS |

---

## 10. Status Git

```
?? Areas/Corporate/FinanceManagement/Purchasing/Controllers/FinancePurchasingReportsController.cs
?? Areas/Corporate/FinanceManagement/Purchasing/Dtos/FinancePurchasingReportDtos.cs
?? Areas/Corporate/FinanceManagement/Purchasing/Services/FinancePurchasingReportService.cs
 M Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs
```

Tidak ada stage, commit, atau push.
