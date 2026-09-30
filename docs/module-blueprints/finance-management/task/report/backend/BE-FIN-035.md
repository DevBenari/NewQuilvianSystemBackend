# Laporan Perubahan Backend — `BE-FIN-035`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-035` |
| Judul | Retur pembelian dicatat, menerbitkan Deposit Retur, dan tercatat di kotak keluar |
| Slice | `REV-4` — Purchasing/AP (`EPIC FIN-15`), gelombang `R4-7` |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian 3 (tabel task) dan bagian "Gelombang eksekusi — `REV-4`" |
| Trace | `FIN-DEC-047`, `061`; `FR-FIN-085` (penerbitan), `FR-FIN-098`; `FIN-API-1.2` §B.5 (kecuali `apply`, dicabut); `FIN-STATE-1.2` §B.5; `FIN-STATE-1.3` §C.2; `FIN-VAL-1.2` `110`, `111`; `FIN-INTEGRATION-1.3` §5.9 kode 28 |
| Contract version | `FIN-API-1.1` (base, `locked` 25 September 2026) diperluas AMENDMENT REVISI 4/5 — belum ada nomor versi API baru khusus task ini |
| Dependency | `BE-FIN-034` 🟡 (source selesai 26 September 2026, `dotnet build` tertunda milik pengguna — dipakai sebagai bukti kontrak `FinPurchasingInvoice`, bukan dipanggil langsung) |
| Klasifikasi | `MEDIUM` — satu repository (skor 0); ≤8 berkas diperiksa dari domain terdekat tapi lebih dari 8 total dibaca lintas kontrak (skor 1); 5 berkas diubah/dibuat (skor 1); logika bisnis sedang — lifecycle 3 status + transaksi dengan efek samping (skor 1); kontrak API — endpoint tambahan memakai pola yang sudah ada (skor 1); database — nol perubahan schema, hanya insert ke tabel yang sudah ada BE-FIN-030 (skor 0); keamanan/auth — action/permission baru mengikuti pola (skor 1); UI/workflow — tidak ada (skor 0). Total 5 → `MEDIUM` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/Purchasing/**` (service, controller, DTO baru), `Areas/Corporate/FinanceManagement/AccountingIntegration/Models/FinAccountingEventOutbox.cs` (tambah 1 konstanta), `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` (registrasi DI), `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` (tanda status) |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | Belum di-commit — lihat Status Git bagian 7 (working tree `Yasmina`, HEAD `45f2bb7c`) |
| Tanggal | 28 September 2026 |
| Status | 🟡 **SEBAGIAN.** Source lengkap; `dotnet build` **sengaja tidak dijalankan** atas instruksi eksplisit pengguna ("jangan lakukan build otomatis") — bukan kegagalan, tertunda menunggu pengguna |

---

## 1. Masalah yang diperbaiki

Sebelum task ini, tidak ada cara mencatat retur barang ke supplier di modul Purchasing/AP. Ketika
petugas AP menerima barang cacat/salah kirim dan mengembalikannya ke supplier, tidak ada jejak
apa pun di Finance — supplier tetap tercatat berutang penuh atas Purchasing Invoice yang barangnya
sudah dikembalikan, dan tidak ada kredit yang bisa dipakai memotong pembayaran berikutnya ke
supplier yang sama.

**Contoh konkret.** RS menerima 100 vial obat dari PT Contoh Farma senilai Rp 10.000.000
(Purchasing Invoice sudah `APPROVED`). Belakangan diketahui 10 vial rusak. Sebelum task ini: tidak
ada mekanisme mencatat retur 10 vial itu — utang ke PT Contoh Farma tetap penuh Rp 10.000.000
walau barangnya sudah dikembalikan sebagian. Sesudah task ini: petugas AP mencatat retur senilai
Rp 1.000.000, mengonfirmasinya, dan sistem menerbitkan Deposit Retur Rp 1.000.000 yang bisa dipakai
memotong pembayaran ke PT Contoh Farma kapan pun berikutnya (pemakaiannya sendiri baru berfungsi
setelah `BE-FIN-036`).

---

## 2. Proses bisnis

**Pelaku:** Petugas AP (mencatat, mengonfirmasi, membatalkan retur).

**Pemicu:** Barang yang sudah diterima dan tercatat di Purchasing Invoice `APPROVED` ternyata perlu
dikembalikan ke supplier (rusak, salah kirim, kelebihan kirim, dll).

**Langkah normal:**

1. Petugas AP mencatat retur (`POST /purchasing/supplier-returns`) atas satu Purchasing Invoice
   yang berstatus `APPROVED`, menyebut alasan dan baris item (deskripsi, kuantitas, harga satuan).
   Retur tersimpan berstatus `DRAFT`, nilainya dihitung backend dari baris item.
2. Petugas AP mengonfirmasi retur (`POST /{id}/confirm`). Dalam **satu transaksi**: status retur
   berpindah ke `CONFIRMED`, satu `FinSupplierReturnDeposit` diterbitkan berstatus `AVAILABLE`
   sebesar nilai retur penuh, dan satu kejadian Accounting `RETUR-PEMBELIAN` ditulis ke kotak
   keluar berstatus `PENDING`. Bila salah satu gagal, seluruhnya batal — retur tetap `DRAFT`.
3. Deposit yang terbit dapat dilihat lewat `GET /purchasing/supplier-returns/deposits`, disaring
   per supplier dan status. **Pemakaiannya** sebagai sumber dana pembayaran supplier bukan cakupan
   task ini — itu `BE-FIN-036`.

**Jalur tidak normal:**

- Retur diajukan atas invoice yang belum `APPROVED` (masih `DRAFT`/`PENDING_APPROVAL`/`REJECTED`/
  `CANCELLED`) → ditolak `422` (`FIN-VAL-110`).
- Nilai retur (jumlah baris item) melebihi `TotalAmount` invoice sumber → ditolak `422`
  (`FIN-VAL-111`).
- Retur tanpa baris item, atau baris berkuantitas ≤ 0, atau harga satuan negatif → ditolak `400`.
- Nilai retur nol (seluruh baris berharga Rp 0) → ditolak `400` — jaminan teknis supaya kejadian
  Accounting yang diterbitkan saat konfirmasi selalu bernilai positif (lihat bagian 7).
- Retur yang bukan `DRAFT` diminta konfirmasi atau dibatalkan → ditolak `422` menyebut status
  sebenarnya.
- Retur yang sudah `CONFIRMED` adalah status akhir — tidak dapat dibatalkan lewat jalur ini
  (deposit yang sudah terbit hanya dapat dibatalkan lewat aturan `FIN-STATE-1.3` §C.2, bukan
  cakupan task ini).
- `RowVersion` yang dikirim tidak cocok dengan data saat ini → `409`, klien diminta memuat ulang.

**Hasil akhir:** retur `CONFIRMED` selalu punya tepat satu Deposit Retur `AVAILABLE` senilai penuh,
dan tepat satu kejadian `RETUR-PEMBELIAN` di kotak keluar Accounting. `FinSupplierPayable` invoice
sumber **tidak tersentuh sama sekali** oleh retur — efeknya ke utang baru berlaku lewat pemakaian
deposit di dalam pembayaran (`BE-FIN-036`).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `AGENTS.md` (backend), `rules/backend/TASK_RULES.md`, `TASK_CLASSIFICATION.md`, `REPORT_TEMPLATE.md`,
  `TEST_POLICY.md`, `engineering/BACKEND_ENGINEERING_CONTRACT.md`, `engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`
- `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` (kartu task `BE-FIN-035`/`036`, grafik dependency, tabel gelombang)
- `docs/module-blueprints/finance-management/contracts/api-contract.md` §B.5
- `docs/module-blueprints/finance-management/contracts/state-transition-matrix.md` §B.5, §B.6, §C.1-C.2
- `docs/module-blueprints/finance-management/contracts/validation-matrix.md` §B.2 (`FIN-VAL-110`..`113`)
- `docs/module-blueprints/finance-management/contracts/permission-audit-matrix.md` (baris `FinanceSupplierReturn`, §C.1)
- `docs/module-blueprints/finance-management/contracts/integration-contract.md` §5.9 (kode 28, 29)
- `docs/module-blueprints/finance-management/02-backend-architecture.md` §FIN-DES-038, AMENDMENT REVISI 5 (§D.1-D.2, `FIN-DES-045`..`047`)
- Model dan configuration existing: `FinSupplierReturn(Item)`, `FinSupplierReturnDeposit`, `FinSupplierReturnDepositUsage` (ditulis `BE-FIN-030`, bentuk revisi 5 — dipakai apa adanya, nol perubahan)
- `FinPurchasingInvoice` model (status `APPROVED`, `TotalAmount`, `SupplierId`)
- `FinancePurchasingInvoiceService.cs` dan `FinancePurchasingInvoicesController.cs` (`BE-FIN-034`) — pola transaksi/maker-checker/outbox yang ditiru
- `FinanceInvoiceExchangeService.cs` dan `FinanceInvoiceExchangesController.cs` (`BE-FIN-033`) — pola `Cancel` sederhana
- `FinanceSupplierPayableService.cs` (pola `GetPagedAsync`/`PagedResult`/`Query`)
- `FinanceAccountingOutboxService.cs` (`StageEventAsync`, `ValidateRequest` — Amount harus > 0, tidak ada pengecualian nol seperti PPN)
- `PurchasingExceptions.cs` (exception yang dipakai ulang)
- `Attributes/AccessControllerAttribute.cs`, `AccessActionAttribute.cs`, `AccessPermissionAttribute.cs`, `Services/Security/PermissionRegistryDescriptor.cs`, `Filters/AccessPermissionFilter.cs` — dibaca untuk memastikan resource name `FinanceSupplierReturn` pada `[AccessPermission]` konsisten secara internal (lihat bagian 3.3)
- `BillingManagementServiceCollectionExtensions.cs` (satu-satunya tempat registrasi DI service Finance)
- `ApplicationDbContext.cs` — `DbSet` `FinSupplierReturn*` sudah terdaftar `BE-FIN-030`, tidak disentuh

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/Purchasing/Services/FinanceSupplierReturnService.cs` | **Baru.** `GetByIdAsync`, `CreateAsync` (DRAFT, `FIN-VAL-110`/`111`), `ConfirmAsync` (DRAFT→CONFIRMED, satu transaksi `Serializable` + advisory lock `FIN_SUPPLIER_RETURN_{id}`: terbitkan `FinSupplierReturnDeposit` + outbox `RETUR-PEMBELIAN`), `CancelAsync` (DRAFT→CANCELLED), `GetDepositsPagedAsync` (paged, saring supplier/status) |
| `Areas/Corporate/FinanceManagement/Purchasing/Controllers/FinanceSupplierReturnsController.cs` | **Baru.** `GET /{id}`, `POST /`, `POST /{id}/confirm` (delta kontrak), `POST /{id}/cancel` (delta kontrak), `GET /deposits` |
| `Areas/Corporate/FinanceManagement/Purchasing/Dtos/FinanceSupplierReturnDtos.cs` | **Baru.** Request/response DTO retur dan deposit, `SupplierReturnDepositQuery` |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Models/FinAccountingEventOutbox.cs` | Tambah satu konstanta `FinAccountingEventTypeCodes.ReturPembelian = "RETUR-PEMBELIAN"` (kode ke-28) |
| `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` | Tambah `services.AddScoped<FinanceSupplierReturnService>();` — satu-satunya tempat registrasi DI service Finance di repository ini |
| `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` | Tanda status `BE-FIN-035` → 🟡 di seluruh titik kemunculan (kartu task, grafik dependency, tabel gelombang `R4-7`/`R4-8`); pembaruan blocker `BE-FIN-036` (`BE-FIN-035` tidak lagi menahan, `BE-FIN-041` tetap menahan) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint baru sesuai `api-contract.md` §B.5: `GET /{id}`, `POST /`, `GET /deposits`. **Delta kontrak (dua endpoint tambahan, dicatat bukan didiamkan):** `POST /{id}/confirm` dan `POST /{id}/cancel` — tidak terdaftar `api-contract.md` maupun `permission-audit-matrix.md` §B.5, tetapi `state-transition-matrix.md` §B.5 mendefinisikan transisi `DRAFT→CONFIRMED` dan `DRAFT→CANCELLED` sebagai aksi terpisah, dan roadmap `BE-FIN-035` Cakupan eksplisit menyebut "catat, **konfirmasi**, **batal**, daftar deposit". Tanpa kedua endpoint ini retur tidak pernah bisa mencapai `CONFIRMED` (Deposit Retur tidak pernah terbit) maupun `CANCELLED` — kapabilitasnya tidak berfungsi. Dibuat memakai pola `POST /{id}/<aksi>` yang sudah baku di rumpun ini (identik `FinanceInvoiceExchangesController.Cancel`), bukan kebijakan baru. **`GET /` (daftar retur berpaging) belum dibangun** — gap yang sama seperti `FinancePurchaseOrdersController`/`FinanceGoodsReceiptsController`/`FinanceInvoiceExchangesController`/`FinancePurchasingInvoicesController` (`BE-FIN-032`/`033`/`034`), dan roadmap Cakupan `BE-FIN-035` sendiri tidak menyebut "daftar retur" (hanya "daftar deposit") |
| Database | **Nol perubahan schema.** `FinSupplierReturn(Item)`, `FinSupplierReturnDeposit`, `FinSupplierReturnDepositUsage` sudah dibuat `BE-FIN-030` dalam bentuk revisi 5 yang benar (`PaymentId`, bukan `PurchasingInvoiceId`, pada `Usage`) — task ini murni menulis baris ke tabel yang sudah ada. Migration: `NOT APPLICABLE` |
| Keamanan/Auth | Resource baru `FinanceSupplierReturn` (persis nama pada `permission-audit-matrix.md`). Action terdaftar: `Read` (dipakai `GetById` dan `GetDeposits`), `Create`. **Dua action tambahan mengikuti delta kontrak:** `Confirm`, `Cancel` — keduanya `AccessType = Update`, mengikuti pola action `Cancel` pada `FinanceInvoiceExchangesController`. `[AccessController(..., ControllerName = "SupplierReturn", ...)]` — `ControllerName` ini dipakai `PermissionRegistryDescriptor` hanya sebagai fallback nama tampilan bila `resourceName` pada `[AccessPermission]` sama persis dengannya (lihat `ResolveControllerName`/`RegisterAction`, `Services/Security/PermissionRegistryDescriptor.cs` baris ~502); karena `resourceName` di sini ("FinanceSupplierReturn") sengaja berbeda dari `ControllerName` ("SupplierReturn") — pola yang identik dipakai `FinancePurchasingInvoicesController`/`FinanceInvoiceExchangesController` dan lain-lain di rumpun ini — resource ditampilkan dengan nama `resourceName` apa adanya di layar Akses Role, bukan `DisplayName` controller. Ini kosmetik (nama yang tampil), **bukan** kegagalan penegakan — `AccessPermissionFilter`/`AccessPermissionService` memakai `resourceName` dari `[AccessPermission]` langsung sebagai kunci pemeriksaan, konsisten dengan seluruh controller Purchasing lain yang sudah berjalan. Tidak ada `IsInRole`/nama peran/departemen/`UserType` hardcode di mana pun pada berkas baru ini |

---

## 4. Dokumentasi endpoint

#### Corporate / Finance Management / Purchasing / Supplier Return

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/{id:guid}` | Rincian retur beserta baris item dan status Deposit Retur turunannya | `FinanceSupplierReturn : Read` |
| `POST` | `/` | Mencatat retur atas satu Purchasing Invoice yang sudah `APPROVED`; nilai dihitung backend dari baris item | `FinanceSupplierReturn : Create` |
| `POST` | `/{id:guid}/confirm` | **Delta kontrak** — mengonfirmasi retur `DRAFT`; menerbitkan Deposit Retur `AVAILABLE` dan kejadian `RETUR-PEMBELIAN`, satu transaksi | `FinanceSupplierReturn : Confirm` |
| `POST` | `/{id:guid}/cancel` | **Delta kontrak** — membatalkan retur yang masih `DRAFT` | `FinanceSupplierReturn : Cancel` |
| `GET` | `/deposits` | Daftar Deposit Retur berpaging, disaring per supplier dan status | `FinanceSupplierReturn : Read` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | — | `NOT RUN` | **Sengaja tidak dijalankan** — pengguna secara eksplisit menginstruksikan "jangan lakukan build otomatis" di tengah sesi ini sebelum build sempat selesai (baru sampai tahap `Determining projects to restore...`). Bukan kegagalan; menunggu instruksi build eksplisit dari pengguna |
| Retur atas invoice `DRAFT`/`PENDING_APPROVAL`/`REJECTED`/`CANCELLED` | Ditolak `PurchasingValidationException` → `422` | `NOT RUN` (review kode) | `CreateAsync` baris pemeriksaan `purchasingInvoice.Status != FinPurchasingInvoiceStatuses.Approved` |
| Nilai retur > `TotalAmount` invoice | Ditolak `422` | `NOT RUN` (review kode) | `CreateAsync` — perbandingan `Math.Round(totalAmount,2) > Math.Round(purchasingInvoice.TotalAmount,2)` |
| Retur tanpa baris item / kuantitas ≤ 0 / harga negatif / nilai nol | Ditolak `400` | `NOT RUN` (review kode) | `BuildItems`/`CreateAsync` |
| Konfirmasi retur bukan `DRAFT` | Ditolak `422` menyebut status sebenarnya | `NOT RUN` (review kode) | `ConfirmAsync` |
| Konfirmasi sukses | Deposit `AVAILABLE` = `TotalAmount`; outbox `RETUR-PEMBELIAN` `PENDING` sebesar `TotalAmount`; satu transaksi | `NOT RUN` (review kode) | `ConfirmAsync` — `StageEventAsync` dipanggil sebelum `SaveChangesAsync`/`CommitAsync` tunggal, pola identik `FinancePurchasingInvoiceService.ApproveAsync` |
| Pembatalan retur bukan `DRAFT` (termasuk yang sudah `CONFIRMED`) | Ditolak `422` | `NOT RUN` (review kode) | `CancelAsync` |
| `RowVersion` usang pada confirm/cancel | `409` "Data telah berubah" | `NOT RUN` (review kode) | `EnsureCurrent`/`Stale` |
| `GET /deposits` disaring `SupplierId`/`Status` | Hasil paged sesuai filter | `NOT RUN` (review kode) | `GetDepositsPagedAsync` |

Uji manual: `NOT FEASIBLE` — memerlukan aplikasi berjalan (`dotnet run`) dan database, keduanya
menunggu instruksi eksplisit terpisah; `dotnet build` sendiri belum dijalankan.

**AUTOMATED TEST:** `NOT APPLICABLE` — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).

**Tidak dijalankan:** `dotnet build`, seluruh eksekusi runtime/manual, dan pemeriksaan QBE
conformance formal (`Invoke-QbeConformanceCheck.ps1`) — ketiganya menunggu instruksi eksplisit
terpisah dari pengguna.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Retur atas invoice non-`APPROVED` ditolak | Terpenuhi (source) | `FinanceSupplierReturnService.CreateAsync` |
| Nilai retur > nilai invoice ditolak | Terpenuhi (source) | `CreateAsync` — `FIN-VAL-111` |
| Konfirmasi retur menerbitkan deposit `AVAILABLE` **dan** satu baris outbox `RETUR-PEMBELIAN` sebesar `TotalAmount` berstatus `PENDING`, satu transaksi | Terpenuhi (source) | `ConfirmAsync` |
| Deposit ber-baris `RESERVED`/`APPLIED` tidak dapat dibatalkan | `NOT APPLICABLE` untuk task ini — mekanisme `RESERVED`/`APPLIED` baru ada di `BE-FIN-036`; task ini hanya menerbitkan deposit `AVAILABLE` | — |
| `dotnet build` berhasil | **Belum terpenuhi** | Sengaja `NOT RUN` atas instruksi pengguna — lihat bagian 5 |
| `FIN-TEST-1.2` §B.4 (tiga baris pertama), `FIN-TEST-1.3` §C.2 baris `FIN-DEC-061` | **Belum dijalankan** — memerlukan aplikasi berjalan | — |

Task ini **belum** dapat ditandai `✅` — dua butir terakhir belum terpenuhi.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Diagnostik IDE menunjukkan satu hint "Using directive is unnecessary" pada `BillingManagementServiceCollectionExtensions.cs` baris 1 — pre-existing, tidak berkaitan dengan `using` yang ditambahkan task ini (tidak ada `using` baru ditambahkan pada berkas itu, hanya satu baris `AddScoped`) |
| Masalah yang diketahui | (1) `GET /` (daftar retur berpaging) belum dibangun — gap yang sama seperti empat controller Purchasing lain, dan Cakupan roadmap task ini sendiri tidak menyebutnya. (2) `FIN-VAL-111` dibandingkan hanya terhadap `TotalAmount` invoice tunggal, bukan akumulasi seluruh retur lain atas invoice yang sama — kontrak (`validation-matrix.md` §B.2) tidak mensyaratkan akumulasi ini secara eksplisit, tetapi bila satu invoice diretur dua kali dengan masing-masing di bawah `TotalAmount`, jumlah keduanya bisa melebihi nilai invoice tanpa tertangkap. Dicatat sebagai risiko, bukan diperbaiki tanpa keputusan eksplisit pemilik kontrak. (3) `GenerateReturnNumber` belum memakai provider number-series atomik (`QBE-CODE-001`..`006`) — pola yang sama dengan `BE-FIN-029`/`030`/`032`/`033`/`034` |
| Risiko tersisa | Nilai retur kumulatif lintas beberapa retur atas invoice yang sama tidak divalidasi (lihat di atas). `dotnet build` belum mengonfirmasi kompilasi bersih |
| Perubahan sampingan | `NONE` |
| Interupsi | Pengguna menginstruksikan "jangan lakukan build otomatis" saat `dotnet build` baru mencapai tahap `Determining projects to restore...` (belum menyentuh kompilasi). Pekerjaan dilanjutkan dari kondisi source yang sudah lengkap tanpa mengulang audit atau penyuntingan; laporan ini mencatat `dotnet build` sebagai `NOT RUN` apa adanya |
| Status Git | `git status --short` pada akhir pekerjaan: `M Areas/Corporate/FinanceManagement/AccountingIntegration/Models/FinAccountingEventOutbox.cs`, `M Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs`, `M docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md`, `?? Areas/Corporate/FinanceManagement/Purchasing/Controllers/FinanceSupplierReturnsController.cs`, `?? Areas/Corporate/FinanceManagement/Purchasing/Dtos/FinanceSupplierReturnDtos.cs`, `?? Areas/Corporate/FinanceManagement/Purchasing/Services/FinanceSupplierReturnService.cs`. Tidak ada perubahan lain milik pengguna yang tersentuh |
| Langkah berikutnya | (1) Pemilik repository menjalankan `dotnet build` dan mengonfirmasi hasilnya; (2) setelah build dikonfirmasi, jalankan `FIN-TEST-1.2` §B.4 dan `FIN-TEST-1.3` §C.2 secara manual/runtime; (3) lanjutkan `BE-FIN-041` (kolom `DepositAppliedAmount` pada `FinPayment`) — satu-satunya prasyarat `BE-FIN-036` yang masih belum dikerjakan setelah task ini; (4) pemilik kontrak memutuskan apakah delta `POST /{id}/confirm`/`POST /{id}/cancel` perlu ditambahkan formal ke `api-contract.md`/`permission-audit-matrix.md` §B.5 |
