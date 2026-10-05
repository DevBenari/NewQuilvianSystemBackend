# Laporan Perubahan Backend — `BE-FIN-063`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-063` |
| Judul | Petugas dapat membaca jejak perubahan saldo per piutang, per utang, dan kas |
| Slice | `REV-14A` (`EPIC FIN-20` — buku mutasi dan tanggal WIB) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` |
| Trace | `FR-FIN-140`; `FIN-DEC-123`; `FIN-API-1.5` F.5; `FIN-PERM-1.7` G.3 |
| Contract version | Blueprint Revisi 14 |
| Dependency | `BE-FIN-062` ✅ |
| Klasifikasi | `READ API / QUERY EXPANSION` (Membuka 3 endpoint baca berpaging buku mutasi subledger) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` |
| Model | Gemini 3.8 Flash (High) |
| Commit backend saat dikerjakan | `Yasmina` |
| Tanggal | 2 Oktober 2026 |
| Status | `SELESAI` |

---

## 1. Masalah yang diperbaiki

Sebelum task ini:
1. **Ketiadaan Permukaan Baca Mutasi (`FR-FIN-140`):** Seluruh pencatatan mutasi pada `FinReceivableMovement` (`BE-FIN-060`), `FinSupplierPayableMovement` (`BE-FIN-061`), dan `FinCashMovement` (`BE-FIN-062`) sudah berjalan di sisi backend, namun belum tersedia endpoint API bagi staf Finance dan Kasir untuk membaca dan menelusuri riwayat mutasi per dokumen atau buku kas umum.
2. **Ketiadaan DTO Query Berpaging & Filter Tanggal:** Staf keuangan membutuhkan fasilitas filter berpaging (`PageNumber`, `PageSize`), rentang tanggal (`DateFrom`, `DateTo`), dan jenis mutasi (`MovementType`) agar dapat memeriksa pergerakan saldo secara efisien tanpa membebani memori server.
3. **Kepatuhan Privasi Data Bebas (`Notes`):** Kolom catatan (`Notes`) pada ketiga buku mutasi dapat memuat teks bebas berisi nama debitur, supplier, atau keterangan sensitif rumah sakit. Sesuai matriks audit `FIN-PERM-1.7` G.3, kolom ini **wajib tidak masuk ke logger aplikasi**.

---

## 2. Proses Bisnis & Solusi Arsitektur

1. **Permukaan Baca Buku Mutasi Piutang (`Receivable Movement History`):**
   - **Endpoint:** `GET /api/v1/corporate/finance-management/receivables/{id:guid}/movements`
   - **Controller:** [`FinanceReceivablesController`](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/NewQuilvianSystemBackend/Areas/Corporate/FinanceManagement/Receivable/Controllers/FinanceReceivablesController.cs).
   - **Logika Layanan:** [`FinanceReceivableService.GetMovementsPagedAsync`](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/NewQuilvianSystemBackend/Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableService.cs).
   - **Perilaku:**
     - Memverifikasi keberadaan data induk `FinReceivable` berdasarkan `id` (mengembalikan `404 Not Found` bila tidak ada atau terhapus).
     - Menyaring data `FinReceivableMovements` berdasarkan `MovementType`, `DateFrom`, dan `DateTo`.
     - Mengurutkan kronologis secara default (`BusinessDate` asc, `OccurredAt` asc, `CreateDateTime` asc) atau sebaliknya via `SortDirection`.
     - Memulangkan `PagedResult<ReceivableMovementResponse>` berbungkus `ApiResponse<T>`.

2. **Permukaan Baca Buku Mutasi Utang Supplier (`Supplier Payable Movement History`):**
   - **Endpoint:** `GET /api/v1/corporate/finance-management/supplier-payables/{id:guid}/movements`
   - **Controller:** [`FinanceSupplierPayablesController`](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/NewQuilvianSystemBackend/Areas/Corporate/FinanceManagement/Payable/Controllers/FinanceSupplierPayablesController.cs).
   - **Logika Layanan:** [`FinanceSupplierPayableService.GetMovementsPagedAsync`](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/NewQuilvianSystemBackend/Areas/Corporate/FinanceManagement/Payable/Services/FinanceSupplierPayableService.cs).
   - **Perilaku:**
     - Memverifikasi keberadaan data induk `FinSupplierPayable` berdasarkan `id` (mengembalikan `404 Not Found` bila tidak ada atau terhapus).
     - Menyaring data `FinSupplierPayableMovements` berdasarkan `MovementType`, `DateFrom`, dan `DateTo`.
     - Mengurutkan kronologis secara default (`BusinessDate` asc, `OccurredAt` asc) atau sebaliknya via `SortDirection`.
     - Memulangkan `PagedResult<SupplierPayableMovementResponse>` berbungkus `ApiResponse<T>`.

3. **Permukaan Baca Buku Mutasi Kas Umum (`Cash Movement Ledger`):**
   - **Endpoint:** `GET /api/v1/corporate/finance-management/daily-cash/cash-movements` (dengan alias rute kanonikal `/api/v1/corporate/finance-management/cash-movements`).
   - **Controller:** [`FinanceDailyCashController`](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/NewQuilvianSystemBackend/Areas/Corporate/FinanceManagement/CashManagement/Controllers/FinanceDailyCashController.cs).
   - **Logika Layanan:** [`FinanceCashManagementService.GetCashMovementsPagedAsync`](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/NewQuilvianSystemBackend/Areas/Corporate/FinanceManagement/CashManagement/Services/FinanceCashManagementService.cs).
   - **Perilaku:**
     - Menyaring data `FinCashMovements` berdasarkan rentang tanggal bisnis (`DateFrom`, `DateTo`), arah kas (`Direction`: `IN` / `OUT`), jenis mutasi (`MovementType`), tipe referensi sumber (`SourceReferenceType`), dan ID shift kasir (`CashierShiftId`).
     - Mengurutkan data secara default dari mutasi terbaru (`BusinessDate` desc, `OccurredAt` desc) atau urutan kronologis awal via `SortDirection = asc`.
     - Memulangkan `PagedResult<CashMovementResponse>` berbungkus `ApiResponse<T>`.

4. **Nol Resource Hak Akses Baru & Penegakan Role-Access:**
   - Seluruh endpoint memanfaatkan wewenang baca yang sudah terdaftar di sistem:
     - Mutasi Piutang: `[AccessPermission("FinanceReceivable", "Read")]`
     - Mutasi Utang Supplier: `[AccessPermission("FinanceSupplierPayable", "Read")]`
     - Mutasi Kas: `[AccessPermission("FinanceDailyCash", "Read")]`
   - Sesuai piagam governance, argumen ke-1 `AccessPermission` cocok 100% dengan `ControllerName` pada `[AccessController]`, dan argumen ke-2 cocok dengan nama aksi `[AccessAction("Read", ...)]`.

5. **Pengecualian Logger Privasi (`FIN-PERM-1.7` G.3):**
   - Mengikuti konvensi project Quilvian, seluruh operasi baca (`GET`) **tidak dicatat ke logger audit aplikasi**.
   - Kolom `Notes` hanya dialirkan sebagai properti DTO respons kepada pengguna yang telah terotorisasi dan tidak pernah bocor ke berkas log server.

---

## 3. Spesifikasi Endpoint Bergaya Swagger

### A. Grup: Corporate / Finance Management / Receivable
`[Tags("Corporate / Finance Management / Receivable")]`
Base Path: `/api/v1/corporate/finance-management/receivables`

| Method | Path | Deskripsi | Auth / Permission | Query Request | Response Model |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/{id:guid}/movements` | Membaca buku mutasi satu piutang berurut tanggal kronologis | `Bearer` / `FinanceReceivable : Read` | `ReceivableMovementQuery` (`PageNumber`, `PageSize`, `DateFrom`, `DateTo`, `MovementType`, `SortDirection`) | `200 OK`: `ApiResponse<PagedResult<ReceivableMovementResponse>>`<br>`404 Not Found`: `ApiResponse<object>` |

### B. Grup: Corporate / Finance Management / Supplier Payable
`[Tags("Corporate / Finance Management / Supplier Payable")]`
Base Path: `/api/v1/corporate/finance-management/supplier-payables`

| Method | Path | Deskripsi | Auth / Permission | Query Request | Response Model |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/{id:guid}/movements` | Membaca buku mutasi satu utang supplier berurut tanggal kronologis | `Bearer` / `FinanceSupplierPayable : Read` | `SupplierPayableMovementQuery` (`PageNumber`, `PageSize`, `DateFrom`, `DateTo`, `MovementType`, `SortDirection`) | `200 OK`: `ApiResponse<PagedResult<SupplierPayableMovementResponse>>`<br>`404 Not Found`: `ApiResponse<object>` |

### C. Grup: Corporate / Finance Management / Daily Cash
`[Tags("Corporate / Finance Management / Daily Cash")]`
Base Path: `/api/v1/corporate/finance-management/daily-cash`

| Method | Path | Deskripsi | Auth / Permission | Query Request | Response Model |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/cash-movements`<br>*(Alias: `/api/v1/corporate/finance-management/cash-movements`)* | Membaca buku mutasi kas bersaring tanggal, arah, dan jenis mutasi | `Bearer` / `FinanceDailyCash : Read` | `CashMovementQuery` (`PageNumber`, `PageSize`, `DateFrom`, `DateTo`, `Direction`, `MovementType`, `SourceReferenceType`, `CashierShiftId`, `SortDirection`) | `200 OK`: `ApiResponse<PagedResult<CashMovementResponse>>` |

---

## 4. Berkas yang Diubah / Dibuat

1. [`Areas/Corporate/FinanceManagement/Receivable/DTOs/FinanceReceivableDtos.cs`](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/NewQuilvianSystemBackend/Areas/Corporate/FinanceManagement/Receivable/DTOs/FinanceReceivableDtos.cs):
   - Menambahkan DTO `ReceivableMovementQuery` dan `ReceivableMovementResponse`.
2. [`Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableService.cs`](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/NewQuilvianSystemBackend/Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableService.cs):
   - Menambahkan method `GetMovementsPagedAsync` dengan validasi eksistensi piutang, filter berpaging, dan proyeksi DTO.
3. [`Areas/Corporate/FinanceManagement/Receivable/Controllers/FinanceReceivablesController.cs`](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/NewQuilvianSystemBackend/Areas/Corporate/FinanceManagement/Receivable/Controllers/FinanceReceivablesController.cs):
   - Menambahkan endpoint `GET /{id:guid}/movements` dengan atribut `[AccessAction("Read", ...)]` dan `[AccessPermission("FinanceReceivable", "Read")]`.
4. [`Areas/Corporate/FinanceManagement/Payable/Dtos/FinanceSupplierPayableDtos.cs`](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/NewQuilvianSystemBackend/Areas/Corporate/FinanceManagement/Payable/Dtos/FinanceSupplierPayableDtos.cs):
   - Menambahkan import `System.ComponentModel.DataAnnotations` serta DTO `SupplierPayableMovementQuery` dan `SupplierPayableMovementResponse`.
5. [`Areas/Corporate/FinanceManagement/Payable/Services/FinanceSupplierPayableService.cs`](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/NewQuilvianSystemBackend/Areas/Corporate/FinanceManagement/Payable/Services/FinanceSupplierPayableService.cs):
   - Menambahkan method `GetMovementsPagedAsync` dengan validasi eksistensi utang, filter berpaging, dan proyeksi DTO.
6. [`Areas/Corporate/FinanceManagement/Payable/Controllers/FinanceSupplierPayablesController.cs`](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/NewQuilvianSystemBackend/Areas/Corporate/FinanceManagement/Payable/Controllers/FinanceSupplierPayablesController.cs):
   - Menambahkan endpoint `GET /{id:guid}/movements` dengan atribut `[AccessAction("Read", ...)]` dan `[AccessPermission("FinanceSupplierPayable", "Read")]`.
7. [`Areas/Corporate/FinanceManagement/CashManagement/DTOs/FinanceCashManagementDtos.cs`](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/NewQuilvianSystemBackend/Areas/Corporate/FinanceManagement/CashManagement/DTOs/FinanceCashManagementDtos.cs):
   - Menambahkan import `System.ComponentModel.DataAnnotations` serta DTO `CashMovementQuery` dan `CashMovementResponse`.
8. [`Areas/Corporate/FinanceManagement/CashManagement/Services/FinanceCashManagementService.cs`](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/NewQuilvianSystemBackend/Areas/Corporate/FinanceManagement/CashManagement/Services/FinanceCashManagementService.cs):
   - Menambahkan method `GetCashMovementsPagedAsync` dengan filter berpaging (`Direction`, `MovementType`, `DateFrom`, `DateTo`, dll).
9. [`Areas/Corporate/FinanceManagement/CashManagement/Controllers/FinanceDailyCashController.cs`](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/NewQuilvianSystemBackend/Areas/Corporate/FinanceManagement/CashManagement/Controllers/FinanceDailyCashController.cs):
   - Menambahkan endpoint `GET cash-movements` (dan alias rute kanonikal) dengan atribut `[AccessAction("Read", ...)]` dan `[AccessPermission("FinanceDailyCash", "Read")]`.
10. [`docs/module-blueprints/finance-management/contracts/api-contract.md`](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/NewQuilvianSystemBackend/docs/module-blueprints/finance-management/contracts/api-contract.md):
    - Memperbarui status ketiga endpoint pada tabel bagian `F.5` menjadi `Tersedia (BE-FIN-063)`.
11. [`docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md`](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/NewQuilvianSystemBackend/docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md):
    - Menandai selesai `BE-FIN-063 ✅` pada grafik dependency dan tabel roadmap gelombang `REV-14A`.
12. [`docs/module-blueprints/finance-management/roadmap/00-delivery-roadmap.md`](file:///c:/Users/admin/source/repos/FEBEQuilvianV2/NewQuilvianSystemBackend/docs/module-blueprints/finance-management/roadmap/00-delivery-roadmap.md):
    - Memperbarui status requirement `FR-FIN-140` menjadi parsial (backend selesai penuh, frontend `FE-FIN-025` dan `FE-FIN-026` direncanakan).

---

## 5. Verifikasi & Pengujian Logika

| Kasus Uji | Skenario | Hasil yang Diharapkan | Status |
| :--- | :--- | :--- | :---: |
| **Uji 1: Baca Mutasi Piutang Eksis** | Memanggil `GET /receivables/{id}/movements` untuk ID piutang valid | Mengembalikan `200 OK` berisi `PagedResult<ReceivableMovementResponse>` berurut tanggal dengan saldo sebelum dan sesudah. | `PASS` |
| **Uji 2: Piutang Tidak Ditemukan** | Memanggil `GET /receivables/{id}/movements` untuk ID acak | Mengembalikan `404 Not Found` dengan pesan jelas bahwa piutang tidak ditemukan. | `PASS` |
| **Uji 3: Filter Mutasi Piutang** | Memanggil dengan parameter `MovementType=ALOKASI-PENERIMAAN` dan rentang `DateFrom`..`DateTo` | Query SQL menyaring baris yang cocok, `TotalData` dan pagination terhitung akurat. | `PASS` |
| **Uji 4: Baca Mutasi Utang Supplier Eksis** | Memanggil `GET /supplier-payables/{id}/movements` untuk ID utang supplier valid | Mengembalikan `200 OK` berisi baris mutasi pengakuan, penyesuaian, dan pembayaran dokumen per alokasi. | `PASS` |
| **Uji 5: Utang Supplier Tidak Ditemukan** | Memanggil `GET /supplier-payables/{id}/movements` untuk ID acak | Mengembalikan `404 Not Found` dengan pesan jelas bahwa utang tidak ditemukan. | `PASS` |
| **Uji 6: Baca Buku Kas Umum** | Memanggil `GET /daily-cash/cash-movements` tanpa filter | Mengembalikan seluruh arus kas masuk dan kas keluar berurutan dari yang terbaru (`desc`). | `PASS` |
| **Uji 7: Filter Buku Kas Arah & Jenis** | Memanggil `GET /daily-cash/cash-movements?direction=OUT&movementType=SETORAN-BANK` | Mengembalikan hanya baris kas keluar setoran bank. | `PASS` |
| **Uji 8: Pengecekan Logger Privasi** | Memeriksa seluruh endpoint `GET` mutasi | Tidak ada pemanggilan ke `_loggerService.AuditAsync`. Kolom `Notes` aman dari logger. | `PASS` |
| **Uji 9: Kesesuaian Hak Akses Role** | Memeriksa controller & action permission | Menggunakan `Read` yang sudah ada pada masing-masing resource, argumen `[AccessPermission]` identik dengan `[AccessController]`. | `PASS` |

---

## 6. Status Database & Migration

- **Nol Migration:** Tidak ada penambahan atau pengubahan schema database (ketiga tabel subledger sudah dibuat pada `BE-FIN-058`).
- **Nol Resource Hak Akses Baru:** Seluruh endpoint terhubung ke role permissions `FinanceReceivable : Read`, `FinanceSupplierPayable : Read`, dan `FinanceDailyCash : Read` yang sudah ada.

---

## 7. Kesimpulan & Langkah Selanjutnya

Dengan selesainya `BE-FIN-063`, seluruh rangkaian implementasi gelombang **`REV-14A` (`EPIC FIN-20` — buku mutasi dan tanggal WIB)** telah **tuntas 100%** (`BE-FIN-058` ✅, `BE-FIN-059` ✅, `BE-FIN-060` ✅, `BE-FIN-061` ✅, `BE-FIN-062` ✅, `BE-FIN-063` ✅).

Langkah selanjutnya adalah beralih ke gelombang **`REV-14B` (`EPIC FIN-21` — pemetaan akun control dan saldo awal cutover)**:
- **`BE-FIN-064`**: Skema pemetaan akun control (`FinSubledgerControlAccount`) dan saldo awal cutover (`FinOpeningBalance`) beserta migrasi EF Core `AddFinanceSubledgerSetup`.
