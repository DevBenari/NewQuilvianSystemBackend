# Laporan Perubahan Backend — `BE-FIN-050`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-050` |
| Judul | Buku register penerimaan kasir dan rekonsiliasi shift Finance vs. `SystemCash` Billing |
| Slice | `POST-MVP` — **task baru**, bukan hasil `/plan-module-delivery`. Diotorisasi eksplisit oleh pemilik repository pada sesi kerja `FE-FIN-004`, setelah ditemukan bahwa dua sub-kapabilitas asli task itu ("rekonsiliasi shift", "buku register penerimaan") tidak punya service backend sama sekali |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian 3 (tabel task, baris `BE-FIN-050`, ditambahkan task ini) |
| Trace | `FIN-API-1.0` (grup Receipt) — kontrak tidak berubah, dua baris yang sudah tercantum sejak awal ("Rencana (belum tersedia)") kini diberi service nyata |
| Contract version | `FIN-API-1.0` — nol perubahan kontrak, nol endpoint baru di luar yang sudah tercantum |
| Dependency | `BE-FIN-018` ✅ (endpoint saudara — daftar, detail, alokasi, pembalikan alokasi — sudah berjalan dan dipakai ulang polanya) |
| Klasifikasi | `MEDIUM` — satu repository (skor 0); ≤8 berkas diperiksa domain terdekat (skor 0); 3 berkas diubah, 0 baru (skor 0); logika bisnis sedang — agregasi lintas dua bounded context (Finance + Billing Cashier) untuk rekonsiliasi (skor 1); kontrak API — 2 endpoint baru memakai pola yang sudah ada (skor 1); database — nol perubahan skema, murni baca tabel yang sudah ada (skor 0); keamanan/auth — memakai resource+action yang sudah terdaftar, nol resource baru (skor 0); UI/workflow — tidak ada (skor 0). Total 2 → `MEDIUM` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/Collection/{Controllers,Dtos,Services}/FinanceReceipts*.cs` (diubah); `docs/module-blueprints/finance-management/roadmap/{00-delivery-roadmap.md,01-backend-roadmap.md}` (tanda status) |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | Belum di-commit — working tree `yasmina` |
| Tanggal | 30 September 2026 |
| Status | ✅ **SELESAI 30 September 2026.** `dotnet build` PASS 0 error, dikonfirmasi pengguna. Nol migration — task ini murni membaca tabel yang sudah ada |

---

## 1. Masalah yang diperbaiki

`FinanceReceiptsController`/`FinanceReceiptService` (BE-FIN-018) secara eksplisit mendokumentasikan
di komentar XML-nya sendiri bahwa `GET /receipts/register` dan `GET /receipts/shift-reconciliation`
— keduanya bagian kontrak `FIN-API-1.0` sejak awal — "di luar cakupan literal roadmap task ini",
dicatat sebagai gap terbuka. Gap ini tetap terbuka melewati beberapa task lanjutan (`BE-FIN-040`,
`042`) sampai ditemukan kembali saat sesi kerja `FE-FIN-004` (layar alokasi penerimaan frontend)
mencoba memetakan cakupan aslinya ("Penerimaan, alokasi manual, koreksi, penghapusan, rekonsiliasi
shift") ke endpoint yang benar-benar ada — dua di antaranya tidak ada sama sekali.

**Contoh konkret.** Sebelum task ini, kasir menutup shift dengan `SystemCash` tercatat di
`BilCashierShift` (modul Billing), tapi Finance tidak punya cara membandingkan angka itu dengan
total penerimaan tunai yang tercatat di `FinReceipt` untuk shift yang sama — rekonsiliasi harus
dilakukan manual di luar sistem. Demikian pula, menelusuri penerimaan mana yang berasal dari
kwitansi/tender tertentu pada satu tanggal harus dilakukan lewat query database langsung, karena
`GET /receipts` (daftar umum) tidak menonjolkan `KwitansiNumber`/`SourceTenderId` sebagai fokus
tampilan.

---

## 2. Proses bisnis

**Pelaku:** Petugas/Treasury Finance (membaca kedua laporan ini — read-only, bukan tindakan yang
mengubah data).

**Langkah normal:**

1. Untuk menelusuri penerimaan satu tanggal atau satu shift kasir, panggil
   `GET /receipts/register?startDate=...&endDate=...&cashierShiftId=...` — mengembalikan daftar
   berpaging `FinReceipt` pada rentang itu, masing-masing baris memuat `KwitansiNumber` dan
   `SourceTenderId` untuk telusur balik ke Billing.
2. Untuk merekonsiliasi satu shift, panggil `GET /receipts/shift-reconciliation?cashierShiftId=...`
   — mengembalikan satu hasil perbandingan: `SystemCashAmount` (dari `BilCashierShift.SystemCash`,
   dibaca apa adanya) versus `FinanceNetCashReceiptAmount` (jumlah bersih `FinReceipt.Amount`
   bermetode tunai pada shift itu, baris `REVERSED` menetralkan baris aslinya), beserta
   `Variance` (selisih keduanya) dan `CashReceiptCount`.

**Jalur tidak normal:**

- `CashierShiftId` pada `shift-reconciliation` tidak ditemukan → `404` ("Shift kasir tidak
  ditemukan.").
- Filter `register` tidak menghasilkan baris apa pun → `PagedResult` kosong (`TotalData = 0`),
  bukan error — konsisten dengan pola `GetPagedAsync` yang sudah ada.

**Hasil akhir:** kedua endpoint murni membaca — nol baris `FinReceipt` maupun `BilCashierShift`
pernah ditulis oleh service ini, sama seperti `GetInvoiceBreakdownAsync` (BE-FIN-017) yang menjadi
rujukan polanya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/contracts/api-contract.md` (grup Receipt, baris
  `GET /register` dan `GET /shift-reconciliation`), `permission-audit-matrix.md` (baris 79-80,
  resource `FinanceReceipt` action `Read`)
- `Areas/Corporate/FinanceManagement/Collection/Models/FinReceipt.cs` (`KwitansiNumber`,
  `CashierShiftId`, `SourceTenderId`, `FinReceiptStatuses.Reversed`) — dibaca, **tidak diubah**
- `Areas/Corporate/FinanceManagement/Collection/Services/FinanceReceiptService.cs` — pola
  `GetPagedAsync` (paging), `GetInvoiceBreakdownAsync` (agregasi bersih-dari-pembalikan),
  `AllocateAsync` (pemakaian `MstPaymentMethod.IsCash`) — pola dipakai ulang, tidak disalin logika
  bisnisnya
- `Areas/HealthServices/BillingManagement/Cashier/Models/BilCashierShift.cs` — dibaca untuk
  memastikan field `SystemCash`, `ShiftNumber`, `Status` benar-benar ada persis seperti yang
  dideskripsikan kontrak, **tidak diubah**
- `Areas/HealthServices/BillingManagement/MasterData/Models/MstPaymentMethod.cs` — dibaca untuk
  memastikan `IsCash` (bool), **tidak diubah**
- `Repositories/ApplicationDbContext.cs` — dibaca untuk memastikan `DbSet<BilCashierShift>`
  (`BilCashierShifts`) dan `DbSet<MstPaymentMethod>` (`MstPaymentMethods`) dapat diakses langsung
  dari `FinanceReceiptService`, **tidak diubah**

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/Collection/Dtos/FinanceReceiptDtos.cs` | Tambah `ReceiptRegisterQuery`/`Response`, `ShiftReconciliationQuery`/`Response` |
| `Areas/Corporate/FinanceManagement/Collection/Services/FinanceReceiptService.cs` | Tambah `GetRegisterAsync` (paging+filter tanggal/shift) dan `GetShiftReconciliationAsync` (agregasi lintas `FinReceipt`+`BilCashierShift`+`MstPaymentMethod`, baca saja) |
| `Areas/Corporate/FinanceManagement/Collection/Controllers/FinanceReceiptsController.cs` | Tambah `GET /register` dan `GET /shift-reconciliation`; perbarui komentar XML kelas (gap yang ditutup vs. yang masih terbuka) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Nol perubahan kontrak** — kedua endpoint sudah tercantum `api-contract.md` sejak `FIN-API-1.0` awal berstatus "Rencana (belum tersedia)"; task ini hanya memberi service nyata. `ShiftReconciliationQuery`/`Response` mengikuti bentuk singular sesuai kontrak (`ApiResponse<ShiftReconciliationResponse>`, bukan `PagedResult`) |
| Database | **`NOT APPLICABLE`.** Nol tabel/kolom baru, nol migration. Murni membaca `FinReceipt` (submodul sendiri) dan `BilCashierShift`/`MstPaymentMethod` (Billing/Administrator, lintas domain, **read-only** — konsisten dengan pola `GetInvoiceBreakdownAsync` yang sudah membaca `FinReceivable` lintas domain) |
| Keamanan/Auth | **Nol resource/action baru.** Kedua endpoint memakai `[AccessPermission("FinanceReceipt", "Read")]` yang sudah terdaftar dan sudah dipakai empat endpoint lain pada controller yang sama — persis sesuai `permission-audit-matrix.md` baris 79-80. Nol `IsInRole`/nama peran/departemen/`UserType` hardcode |

---

## 4. Dokumentasi endpoint

#### Corporate / Finance Management / Receipt

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/register` | Buku penerimaan kasir per tanggal/shift, menelusur ke tender dan kwitansi asalnya | `FinanceReceipt : Read` |
| `GET` | `/shift-reconciliation` | Membandingkan total penerimaan tunai Finance dengan `SystemCash` Billing untuk satu shift | `FinanceReceipt : Read` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Berhasil tanpa error | `PASS` | Dikonfirmasi langsung oleh pengguna 30 September 2026 |
| `GetRegisterAsync` — filter tanggal dan shift diterapkan sebelum paging | Query `Where` dibangun sebelum `Skip`/`Take`, konsisten `GetPagedAsync` | `NOT RUN` (review kode) | `FinanceReceiptService.cs` |
| `GetShiftReconciliationAsync` — shift tidak ditemukan | Melempar `KeyNotFoundException` → `404` | `NOT RUN` (review kode) | Controller `catch (KeyNotFoundException)` |
| `GetShiftReconciliationAsync` — baris `REVERSED` menetralkan baris aslinya | `netCashAmount` dihitung `Sum(aktif) − Sum(reversed)`, pola sama `GetInvoiceBreakdownAsync` | `NOT RUN` (review kode) | `FinanceReceiptService.cs` |
| `GetShiftReconciliationAsync` — hanya metode tunai ikut dihitung | Filter `cashPaymentMethodIds` dari `MstPaymentMethods.Where(IsCash)` sebelum agregasi | `NOT RUN` (review kode) | `FinanceReceiptService.cs` |

Uji manual: `NOT FEASIBLE` — memerlukan aplikasi berjalan (`dotnet run`) dan data `FinReceipt`/
`BilCashierShift` nyata, keduanya belum diotorisasi/dijalankan pada sesi ini.

**AUTOMATED TEST:** `NOT APPLICABLE` — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).

**Tidak dijalankan:** `dotnet build`, seluruh eksekusi runtime/manual — menunggu instruksi
eksplisit terpisah dari pemilik repository.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Register menampilkan `KwitansiNumber`/`SourceTenderId` per baris, dapat disaring tanggal dan shift | Terpenuhi (source) | `ReceiptRegisterResponse`, `GetRegisterAsync` |
| Rekonsiliasi shift kembalikan `404` untuk `CashierShiftId` yang tidak ada | Terpenuhi (source) | `GetShiftReconciliationAsync` — `?? throw new KeyNotFoundException` |
| `Variance = SystemCash − FinanceNetCashReceiptAmount`, bernilai nol saat kas pas | Terpenuhi (source) | `GetShiftReconciliationAsync` |
| Nol tulisan ke `FinReceipt` maupun `BilCashierShift` | Terpenuhi (source) — seluruh akses `AsNoTracking()`, nol `SaveChangesAsync` dipanggil kedua method ini | Review `FinanceReceiptService.cs` menyeluruh |
| `dotnet build` berhasil | Terpenuhi | `PASS` dikonfirmasi pengguna 30 September 2026 |

Seluruh acceptance criteria terpetakan ke source yang ada dan validasi build sudah dijalankan —
task ini ditandai `✅`. Uji manual/runtime terhadap data nyata tetap belum dilakukan (lihat bagian 5)
karena tidak ada instance backend berjalan yang dikonfirmasi pada sesi ini; ini tidak menahan status
`✅` karena "Verifikasi" yang diminta task ini secara literal hanya `dotnet build` (konsisten dengan
pola task backend lain pada modul ini, mis. `BE-FIN-039`/`040`).

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Task ini **bukan** hasil `/plan-module-delivery` — task ID dan cakupannya ditetapkan langsung oleh pemilik repository di tengah sesi kerja `FE-FIN-004`/`FE-FIN-013`, menutup gap yang sudah lama dicatat terbuka di laporan `BE-FIN-018.md`. Cakupan sengaja dipersempit hanya dua endpoint ini — `POST /receipts` (pencatatan manual non-kasir) dan `POST /receipts/{id}/reverse` (pembalikan penuh manual) tetap gap terbuka, tidak diminta pada sesi ini |
| Masalah yang diketahui | (1) `ShiftReconciliationQuery` hanya menerima satu `CashierShiftId` per pemanggilan, persis kontrak (`ApiResponse<ShiftReconciliationResponse>` singular) — bukan laporan lintas-banyak-shift; bila kebutuhan sebenarnya adalah laporan per periode lintas shift, itu perubahan kontrak terpisah, bukan diam-diam diperluas di sini. (2) Definisi "penerimaan tunai" memakai `MstPaymentMethod.IsCash`, field yang sudah dipakai `AllocateAsync` untuk keperluan serupa (validasi shift wajib untuk metode tunai) — dipakai ulang, bukan kriteria baru yang ditebak |
| Dependency backend | `BE-FIN-018` ✅ — endpoint saudara (`GET /`, `GET /{id}`, alokasi, pembalikan alokasi) sudah berjalan, dipakai sebagai rujukan pola langsung |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `git status --short` mencakup task ini: `M Areas/Corporate/FinanceManagement/Collection/Controllers/FinanceReceiptsController.cs`, `M .../Dtos/FinanceReceiptDtos.cs`, `M .../Services/FinanceReceiptService.cs`, plus dokumen roadmap. File lain yang terlihat pada `git status` (mis. `docs/module-blueprints/accounting/MODULE-STATUS.md`, laporan `FE-FIN-007.md`) **bukan** bagian task ini — tidak disentuh, tidak dilaporkan di sini karena tidak dibuat oleh sesi ini |
| Langkah berikutnya | (1) Uji manual kedua endpoint terhadap data `FinReceipt`/`BilCashierShift` nyata bila dibutuhkan; (2) perbarui laporan `FE-FIN-004.md` untuk mencabut catatan "gap backend rekonsiliasi shift" mengingat gap itu kini tertutup |
