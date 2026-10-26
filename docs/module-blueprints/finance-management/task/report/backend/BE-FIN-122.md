# Laporan Perubahan Backend — `BE-FIN-122`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-122` |
| Judul | Petugas Finance dapat mencari dan menelusuri Purchasing Invoice, Pembayaran, dan Retur Pembelian lewat daftar berpaging bersaring |
| Slice | `REV-22` — Finance AP mengikuti BLM, daftar dokumen AP (`Slice B` yang tidak menunggu keputusan) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md`, bagian "AMENDMENT ROADMAP REVISI 22" |
| Trace | `AP-UAT-21` (dokumen proses bisnis AP BLM, 26 Oktober 2026); `FIN-DEC-045` |
| Contract version | `FIN-API-1.1` §B.4 dan §B.5, serta `FIN-API-1.0` (Payment) — locked. Ketiga `GET /` sudah terdaftar di kontrak dengan status "Rencana (belum tersedia)"; task ini memenuhinya tanpa mengubah kontrak. Dua delta kecil (filter jatuh tempo ditunda, filter retur dan pembayaran dirinci) dicatat pada roadmap `REV-22` |
| Dependency | Tidak ada |
| Klasifikasi | `MEDIUM` — skor 6: repository 0; berkas diperiksa 1 (sekitar 14); berkas diubah 2 (9 berkas source); logika bisnis 0 (hanya baca dan saring); kontrak API 1 (memenuhi kontrak yang sudah ada); database 1 (hanya query pada tabel yang ada); keamanan/auth 1 (memakai permission `Read` yang ada); UI/workflow 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` branch `Yasmina`: `Areas/Corporate/FinanceManagement/Purchasing/` dan `Payable/`, `docs/module-blueprints/finance-management/` |
| Model | Claude Sonnet 5.5 |
| Commit backend saat dikerjakan | `48011b09` (branch `Yasmina`). Working tree sudah memuat perubahan Billing milik pengguna yang tidak disentuh task ini |
| Tanggal | 2026-10-26 |
| Status | 🟡 **SEBAGIAN** — seluruh kriteria terpetakan ke source; `dotnet build` **NOT RUN** |

---

## 1. Masalah yang diperbaiki

Kontrak API Finance yang sudah terkunci mewajibkan tiga daftar berpaging, tetapi ketiganya belum ada kodenya: daftar Purchasing Invoice, daftar
Pembayaran, dan daftar Retur Pembelian. Komentar pada ketiga controller mengakui hal itu ("belum ada service-nya"). Akibatnya dokumen hanya bisa
dibuka bila ID-nya sudah diketahui; petugas tidak bisa bertanya "invoice mana milik Supplier A yang masih menunggu persetujuan?", "pembayaran apa
yang sedang diajukan?", atau "retur apa yang masih draft?". Dokumen proses bisnis AP (`AP-UAT-21`) mensyaratkan ketiganya dapat dibaca kembali dan
ditelusuri statusnya.

---

## 2. Proses bisnis

**Pelaku:** petugas AP, penyetuju Finance, Treasury, dan reviewer.

**Alur:**

1. Pengguna dengan hak `Read` pada resource terkait membuka daftar dan, bila perlu, menyaringnya.
2. Sistem menampilkan dokumen yang belum dihapus lunak, terbaru lebih dulu, per halaman (bawaan 25, paling banyak 100).
3. Pengguna memilih satu dokumen dan membuka rinciannya lewat endpoint `GET /{id}` yang sudah ada.

**Filter yang tersedia:**

| Daftar | Filter |
| --- | --- |
| Purchasing Invoice | `supplierId`, `status` |
| Retur Pembelian | `purchasingInvoiceId`, `supplierId` (lewat invoice asal), `status` |
| Pembayaran | `paymentType`, `payeeReferenceId`, `status` |

**Contoh:** `GET /api/v1/corporate/finance-management/purchasing/purchasing-invoices?supplierId=<id Supplier A>&status=PENDING_APPROVAL&pageNumber=1&pageSize=25`
mengembalikan hanya invoice Supplier A yang menunggu persetujuan, 25 per halaman.

**Jalur tidak normal:** `pageSize` di atas 100 atau `pageNumber` di bawah 1 ditolak `400` oleh validasi query; pengguna tanpa hak `Read` ditolak `403`;
nilai `status` ditulis huruf kecil tetap dikenali karena dinormalkan ke huruf besar.

**Yang sengaja belum ada:** kontrak menyebut daftar invoice "disaring supplier/status/jatuh tempo". Filter jatuh tempo **belum disediakan** karena Purchasing
Invoice tidak menyimpan tanggal jatuh tempo, dan basis tanggalnya (tanggal faktur, tanggal terima barang, atau tanggal Tukar Faktur) adalah keputusan terbuka
pemilik (`AP-DEC-03`). Menebaknya sekarang berarti mengunci aturan yang belum diputuskan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/contracts/api-contract.md` (§B.4, §B.5, bagian Payment)
- `Areas/Corporate/FinanceManagement/Purchasing/Controllers/FinancePurchaseOrdersController.cs` dan `FinanceInvoiceExchangesController.cs` (pola `GetList`)
- `Areas/Corporate/FinanceManagement/Purchasing/Services/FinanceInvoiceExchangeService.cs` dan `FinanceGoodsReceiptService.cs` (pola `GetPagedAsync`)
- `Areas/Corporate/FinanceManagement/Purchasing/Services/FinanceSupplierReturnService.cs` (`GetDepositsPagedAsync`)
- `Areas/Corporate/FinanceManagement/Purchasing/Controllers/FinancePurchasingInvoicesController.cs`, `FinanceSupplierReturnsController.cs`, dan `Payable/Controllers/FinancePaymentsController.cs`
- `Areas/Corporate/FinanceManagement/Purchasing/Dtos/*.cs` dan `Payable/Dtos/FinancePaymentDtos.cs`
- `Areas/Corporate/FinanceManagement/Purchasing/Models/FinSupplierReturn.cs` dan `Payable/Models/FinPayment.cs`
- `Responses/PagedResult.cs`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/Purchasing/Dtos/FinancePurchasingInvoiceDtos.cs` | `PurchasingInvoiceQuery` (supplier, status, paging) dan `using` untuk data annotation |
| `Areas/Corporate/FinanceManagement/Purchasing/Dtos/FinanceSupplierReturnDtos.cs` | `SupplierReturnQuery` (invoice asal, supplier, status, paging) |
| `Areas/Corporate/FinanceManagement/Payable/Dtos/FinancePaymentDtos.cs` | `PaymentQuery` (jenis, penerima, status, paging) dan `using` untuk data annotation |
| `Areas/Corporate/FinanceManagement/Purchasing/Services/FinancePurchasingInvoiceService.cs` | `GetPagedAsync`; `using` untuk `Dtos` dan `Responses` |
| `Areas/Corporate/FinanceManagement/Purchasing/Services/FinanceSupplierReturnService.cs` | `GetPagedAsync` (supplier disaring lewat `PurchasingInvoice` karena retur tidak punya kolom itu) |
| `Areas/Corporate/FinanceManagement/Payable/Services/FinancePaymentService.cs` | `GetPagedAsync`; `using` untuk `Responses` |
| `Areas/Corporate/FinanceManagement/Purchasing/Controllers/FinancePurchasingInvoicesController.cs` | Aksi `GetList`; komentar header diperbarui |
| `Areas/Corporate/FinanceManagement/Purchasing/Controllers/FinanceSupplierReturnsController.cs` | Aksi `GetList`; komentar header diperbarui |
| `Areas/Corporate/FinanceManagement/Payable/Controllers/FinancePaymentsController.cs` | Aksi `GetList`; komentar header diperbarui |
| `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` | `BE-FIN-122` didaftarkan pada amendment `REV-22` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tiga endpoint `GET /` baru, seluruhnya sudah terdaftar di kontrak terkunci. Endpoint existing tidak berubah. Response memakai DTO yang sudah ada (`PurchasingInvoiceResponse`, `SupplierReturnResponse`, `PaymentResponse`), bukan entity EF |
| Database | `NOT APPLICABLE` — nol entity, tabel, kolom, dan migration; hanya `SELECT` dengan `AsNoTracking`, urut `CreateDateTime` menurun, paging `Skip`/`Take` |
| Keamanan/Auth | Setiap `GetList` memakai `[AccessAction("Read", ...)]` dan `[AccessPermission(<resource>, "Read")]` yang identik dengan `GetById` pada controller yang sama, sehingga tidak ada baris baru di layar Akses Role dan siapa pun yang sudah boleh membaca rincian otomatis boleh membaca daftar. Daftar pembayaran memuat `payeeReferenceId`; bila penerimanya tenaga medis, nilai itu sensitif (`permission-audit-matrix.md` bagian Payment), tetapi rinciannya sudah terbuka bagi pemegang hak `Read` yang sama |

---

## 4. Dokumentasi endpoint

#### Corporate / Finance Management / Purchasing / Purchasing Invoice

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/finance-management/purchasing/purchasing-invoices` | Daftar Purchasing Invoice berpaging; query `supplierId`, `status`, `pageNumber`, `pageSize` | `FinancePurchasingInvoice : Read` |

#### Corporate / Finance Management / Purchasing / Supplier Return

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/finance-management/purchasing/supplier-returns` | Daftar Retur Pembelian berpaging; query `purchasingInvoiceId`, `supplierId`, `status`, `pageNumber`, `pageSize` | `FinanceSupplierReturn : Read` |

#### Corporate / Finance Management / Payment

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/finance-management/payments` | Daftar pembayaran keluar berpaging; query `paymentType`, `payeeReferenceId`, `status`, `pageNumber`, `pageSize` | `FinancePayment : Read` |

Catatan nama hak akses: pada Purchasing Invoice dan Retur Pembelian, nilai `ControllerName` pada `[AccessController]` (`PurchasingInvoice`, `SupplierReturn`)
berbeda dari nama resource pada `[AccessPermission]` (`FinancePurchasingInvoice`, `FinanceSupplierReturn`). Itu sah: sejak Phase A0, registry hak akses
diturunkan dari `[AccessPermission]` itu sendiri (`PermissionRegistryDescriptor`), sehingga baris yang bisa dicentang di layar Akses Role selalu identik dengan
yang dicari saat request masuk. `GetList` memakai nama resource yang sama dengan `GetById`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Instruksi pemilik: build dijalankan mandiri oleh pemilik, bukan otomatis oleh agent |
| QBE preflight | `TOUCHED LEGACY`, nol entity/migration baru; `QBE-API-001`, `QBE-DTO-001`, `QBE-PAGE-001`, `QBE-PERM-001`, `QBE-SVC-001` terpenuhi | `PASS` | Penelusuran source: controller hanya memetakan, query berada di service |
| Review diff dan scope | 9 berkas source + roadmap; tidak ada perubahan dependency, konfigurasi, atau migration | `PASS` | `git diff --numstat` dan `git status --short` (bagian 7) |
| Kesesuaian dengan kontrak | Tiga path, hak akses, nama `Query`, dan bentuk `ApiResponse<PagedResult<...>>` sama dengan `api-contract.md` §B.4, §B.5, dan bagian Payment | `PASS` | Pembacaan kontrak dan controller |
| Tipe `PagedResult<T>.Items` | `List<T>`; penugasan `Select(...).ToList()` cocok | `PASS` | `Responses/PagedResult.cs` baris 13 |
| Konflik route dan nama | `[HttpGet]` (kosong), `[HttpGet("{id:guid}")]`, dan `[HttpGet("deposits")]` tidak saling bentrok; nama `PaymentQuery`, `PurchasingInvoiceQuery`, `SupplierReturnQuery` unik di repository | `PASS` | Pencarian `class <nama>` pada seluruh `*.cs` |
| Pencocokan `[AccessAction]` dan `[AccessPermission]` pada `GetList` | Identik dengan `GetById` pada controller yang sama (nama aksi `Read` sama di kedua atribut; nama resource sama dengan `GetById`) | `PASS` | Pembacaan ketiga controller |
| Sumber identitas registry hak akses | Registry diturunkan dari `[AccessPermission]`, bukan dari `ControllerName`, sehingga `GetList` otomatis muncul pada baris `Read` yang sudah ada | `PASS` | `Services/Security/PermissionRegistryDescriptor.cs` baris 12-36 |
| Menjalankan ketiga endpoint terhadap database | Tidak dijalankan | `NOT RUN` | Server backend tidak dijalankan |

`AUTOMATED TEST: NOT APPLICABLE — task ini tidak disertai test otomatis; persetujuan pemilik atas test hanya mencakup BE-FIN-120 dan BE-FIN-121 (rules/backend/TEST_POLICY.md).`

Uji manual: `NOT FEASIBLE` — server backend tidak dijalankan pada sesi ini.

**Tidak dijalankan:** `dotnet build`, uji manual Swagger, dan uji filter terhadap data sungguhan. Semuanya menunggu pemilik.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. `GET .../purchasing-invoices` berpaging, terbaru dulu, dapat disaring `supplierId` dan `status` | Terpetakan ke source; belum dibuktikan build | `FinancePurchasingInvoiceService.GetPagedAsync`; `FinancePurchasingInvoicesController.GetList` |
| 2. `GET .../supplier-returns` dapat disaring `purchasingInvoiceId`, `supplierId`, `status` | Terpetakan ke source; belum dibuktikan build | `FinanceSupplierReturnService.GetPagedAsync`; `FinanceSupplierReturnsController.GetList` |
| 3. `GET .../payments` dapat disaring `paymentType`, `payeeReferenceId`, `status` | Terpetakan ke source; belum dibuktikan build | `FinancePaymentService.GetPagedAsync`; `FinancePaymentsController.GetList` |
| 4. `pageSize` > 100 atau `pageNumber` < 1 ditolak `400` | Terpetakan ke source; belum dibuktikan build | Atribut `[Range]` pada ketiga kelas query dengan `[ApiController]` |
| 5. Dokumen dihapus lunak tidak muncul | Terpetakan ke source; belum dibuktikan build | `Where(x => !x.IsDelete)` pada ketiga `GetPagedAsync` |
| 6. Pengguna tanpa hak `Read` ditolak `403` | Terpetakan ke source; belum dibuktikan build | `[AccessPermission]` pada ketiga `GetList` |
| DoD: preflight selesai | Terpenuhi | Metadata dan bagian 5 |
| DoD: `dotnet build` berhasil | **Belum terpenuhi** | `NOT RUN` |
| DoD: laporan tracked ada | Terpenuhi | Berkas ini |

Task ini **belum** dapat ditandai ✅ sampai pemilik mengonfirmasi `dotnet build`, sesuai `status-task-roadmap.md`.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `role-access-rules.md` bagian 3 pada suite skill masih menyebut argumen ke-1 `[AccessPermission]` harus sama dengan `ControllerName`. Source saat ini berbeda: registry diturunkan dari `[AccessPermission]` (Phase A0, `PermissionRegistryDescriptor`), sehingga ketidaksamaan itu tidak lagi menimbulkan 403 permanen. Selisih antara teks aturan dan source ini **dilaporkan**, bukan diperbaiki oleh task ini |
| Masalah yang diketahui | Filter jatuh tempo pada daftar invoice menunggu `AP-DEC-03`. Daftar tidak mendukung pencarian teks atau pengurutan pilihan pengguna (pola daftar Finance lain juga tidak) |
| Risiko tersisa | Rendah untuk kebenaran logika (query baca sederhana). Kebenaran runtime belum dibuktikan karena build belum dijalankan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat blok di bawah |
| Langkah berikutnya | Pemilik menjalankan `dotnet build`; lalu `BE-FIN-122` dinaikkan ke ✅ pada roadmap |

```text
M Areas/Corporate/FinanceManagement/Payable/Controllers/FinancePaymentsController.cs
M Areas/Corporate/FinanceManagement/Payable/Dtos/FinancePaymentDtos.cs
M Areas/Corporate/FinanceManagement/Payable/Services/FinancePaymentService.cs
M Areas/Corporate/FinanceManagement/Purchasing/Controllers/FinancePurchasingInvoicesController.cs
M Areas/Corporate/FinanceManagement/Purchasing/Controllers/FinanceSupplierReturnsController.cs
M Areas/Corporate/FinanceManagement/Purchasing/Dtos/FinancePurchasingInvoiceDtos.cs
M Areas/Corporate/FinanceManagement/Purchasing/Dtos/FinanceSupplierReturnDtos.cs
M Areas/Corporate/FinanceManagement/Purchasing/Services/FinancePurchasingInvoiceService.cs
M Areas/Corporate/FinanceManagement/Purchasing/Services/FinanceSupplierReturnService.cs
M docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md
?? docs/module-blueprints/finance-management/task/report/backend/BE-FIN-122.md
```

Berkas lain pada `git status --short` (Billing, test Pharmacy, dan berkas task `REV-22` lainnya) bukan bagian dari task ini.
