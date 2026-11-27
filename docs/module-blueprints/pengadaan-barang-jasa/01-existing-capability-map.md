# Pengadaan Barang & Jasa — Peta Kemampuan Existing

| Field | Nilai |
| --- | --- |
| Blueprint ID | `PBJ-BP-001` |
| Revisi peta | `2` — revisi 1 (audit penuh pertama, 12 November 2026) + impact scan `pemilihan-penyedia` (bagian 7, 26 November 2026) |
| Status | `draft` |
| Revisi blueprint / keputusan | revisi `2` / `decision_revision 18` (`PBJ-DEC-001`..`081`) |
| Masukan | BRD & PRD v1.0 DRAFT; `00-interview-decisions.md` (scope pass `pemilihan-penyedia` `PBJ-DEC-066`..`081`) |
| Backend SHA | Revisi 1: `100c011d54147bb19ca4bd874fdfb200448ba46a`. Revisi 2: `d5fdc8faefb8724810fcd1fe42f9128a2fcc0ede` (branch `sukmagp`; working tree hanya berisi `00-interview-decisions.md` yang belum di-commit) |
| Frontend SHA | Revisi 1: `763d665b2db3f31cf370ddba68623dc8aa57cf18`. Revisi 2: `3e538667994feba533d2c93a142a6f333092066d` (branch `sukmagpV2`, bersih) |
| Kontrak yang berlaku | `perencanaan-permintaan` set 1.0 (API 1.4, INT 1.1, PERM 1.3); `manajemen-vendor` set 1.1 |
| Sifat audit | **Hanya membaca.** Tidak ada source aplikasi yang diubah. Database tidak disentuh |
| Tanggal | 12 November 2026 (revisi 1); 26 November 2026 (revisi 2) |

> **Baca bagian 7 lebih dulu** untuk kebutuhan `pemilihan-penyedia`. Status baris `PBJ-CAP-001`..`018` di bagian 3
> adalah keadaan 12 November 2026; status terkininya untuk sub-modul ini ada di bagian 7.1.

**Singkatan bukti.** `BE` = `NewQuilvianSystemBackend`, `FE` = `V2QuilvianSystemFrontendDev`. Bila SHA tidak
ditulis, berlaku SHA pada tabel di atas.

> **Cara membaca dokumen ini.** Dokumen ini menjawab "apa yang **sudah ada** di sistem hari ini" untuk
> kebutuhan modul Pengadaan. Setiap kemampuan diberi tepat satu status:
>
> | Status | Arti sederhana |
> | --- | --- |
> | `Ready to reuse` | Bisa langsung dipakai apa adanya |
> | `Reuse with adapter` | Bisa dipakai, tetapi Pengadaan perlu lapisan penghubung sendiri, tanpa mengubah pemiliknya |
> | `Extend` | Pemiliknya perlu ditambah kemampuan kecil supaya bisa dipakai |
> | `Repair` | Ada, tetapi rusak atau tidak sesuai aturan |
> | `Missing` | Belum ada sama sekali |
> | `Conflict` | Ada, tetapi bertentangan dengan keputusan Pengadaan, sehingga butuh keputusan manusia |
> | `Unknown` | Belum bisa dipastikan dari audit ini |
>
> Dokumen ini tidak merancang arsitektur dan tidak memberi izin menulis kode.

---

## 1. Batas audit

| Diaudit | Tidak diaudit |
| --- | --- |
| Mesin workflow/approval, penomoran dokumen, unggah berkas, jejak audit, hak akses, master unit, `MstSupplier`, master barang/stok, PO/GR/faktur/bayar Finance, halaman FE terkait | Isi lengkap 14.000+ baris service workflow; perilaku runtime (tidak ada eksekusi); repository BLM; notifikasi (merujuk audit `platform-notification`); laporan Finance selain jejak dokumen |

## 2. Ringkasan temuan

1. **Mesin approval bersama ternyata ada.** Registry mencatat `WorkflowManagement` sebagai
   `SHARED PLATFORM CAPABILITY` dengan prefix `Wfl`, walaupun foldernya ada di bawah `HumanResource`.
   Mesin ini sudah punya matriks persetujuan, kotak masuk, penjaga "tidak boleh menyetujui sendiri",
   delegasi, dan SLA. **Namun**, penghubung ke modul pemilik dokumen masih ditulis khusus untuk jenis
   dokumen HR, dan penyetuju dicari lewat data pegawai. Jadi syarat `PBJ-DEC-005` terpenuhi
   **sebagian**: mesinnya ada, tetapi perlu diperluas (`PBJ-CAP-001`, status `Extend`).
2. **Penomoran dokumen siap dipakai** (`NumberSeriesAllocator`).
3. **`MstSupplier` bertabrakan dengan dua keputusan.** Tabel ini menyimpan status blacklist sendiri
   (`IsBlacklisted`), padahal `PBJ-DEC-010` menaruh status vendor pengadaan di tabel pendamping. Nomor
   rekening juga dikirim tanpa disamarkan, padahal `PBJ-DEC-015` mewajibkan tampilan tersamarkan.
4. **Belum ada master barang/jasa umum.** Yang ada hanya master obat (`MstDrug`) dan satuan
   (`MstMeasurement`). Baris PR perlu keputusan cara mengisi barangnya.
5. **Jejak PO sampai pembayaran sudah tersedia per PO** di Finance, sehingga angka "ditagih" dan
   "dibayar" (`PBJ-DEC-009`) bisa dibaca. Namun, daftar PO belum bisa dicari berdasarkan nomor PO.

## 3. Peta kemampuan

| ID | Kebutuhan Pengadaan | Pemilik | Bukti (`repo/path#simbol@SHA`) | Status | Gap / penghubung | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `PBJ-CAP-001` | Mesin persetujuan bertingkat (`PBJ-DEC-005`, `007`, `016`) | WorkflowManagement (`Wfl`, shared platform) | `BE/docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 36; `BE/Areas/Corporate/HumanResource/WorkflowManagement/Models/TrxWorkflowInstance.cs#TrxWorkflowInstance` (`ReferenceType`, `ReferenceId`, `Amount`, `PriorityLevel`, `SlaDeadline`, `WorkflowDefinitionSnapshotJson`); `.../MasterData/Workflow/Models/MstApprovalMatrix.cs#MstApprovalMatrix` (`MinimumAmount`/`MaximumAmount`, `ApproverSourceType`: RequesterManager, Position, OrganizationUnit, Role, SpecificUser); `.../Services/AntiSelfApprovalGuardService.cs#ValidateAntiSelfApproval`; `.../Services/WorkflowReferenceLifecycleService.cs` baris 52–85 (konstanta jenis dokumen HR saja); `.../Services/IdempotentDownstreamExecutionService.cs` baris 41–43 | `Extend` | (1) Belum ada titik daftar untuk jenis dokumen di luar HR: jenis yang tidak dikenal dikembalikan `IsHandled = false` tanpa efek, sehingga status PR tidak ikut berubah saat disetujui. (2) Penjaga "tidak boleh menyetujui sendiri" bekerja dengan ID **pegawai**, sehingga pengguna Pengadaan harus terhubung ke data pegawai. (3) Kunci matriks berbasis organisasi HR (`OrganizationUnitId`, `DepartmentId`), bukan "jenis dokumen + urgensi". Yang sudah cocok: nominal, prioritas, SLA, snapshot definisi (sesuai invariant `PBJ-AC-003`) | Perluasan menyentuh mesin bersama milik modul lain dan butuh persetujuan pemiliknya (`PBJ-OQ-021`) |
| `PBJ-CAP-002` | Kotak masuk persetujuan (submenu 1.2, 11.1) | WorkflowManagement | `BE/.../WorkflowManagement/Controllers/ApprovalInboxController.cs` (`[Route("api/v1/corporate/human-resource/approval-inbox")]`, `[AccessPermission("ApprovalInbox", ...)]`); `FE/src/lib/services/hr/approval-inbox-service.js`; `FE/src/app/hr/workflow/approval-inbox/page.jsx` | `Reuse with adapter` | Kotak masuk sudah generik terhadap `ReferenceType`, tetapi rute dan menunya berada di HR. Pengadaan perlu tampilan sendiri, atau menautkan ke kotak masuk bersama | Mengikuti `PBJ-CAP-001` |
| `PBJ-CAP-003` | Nomor dokumen otomatis (PR, paket, kontrak, BAST, NCR) | Platform / NumberSeries (`Num`) | `BE/Areas/Platform/NumberSeriesManagement/Services/NumberSeriesAllocator.cs#AllocateAsync`; `BE/Program.cs` baris 1101 (`AddScoped<NumberSeriesAllocator>`); dipakai Finance (`FinanceReceivableNumberGenerator.cs`) dan Administrator | `Ready to reuse` | Pengadaan cukup mendaftarkan deret nomornya sendiri. Format nomor masih `PBJ-OQ-007` | Perilaku nomor saat transaksi batal mengikuti keputusan Platform (`CONF-PLT-002`) |
| `PBJ-CAP-004` | Unggah dan unduh berkas terlindungi (KAK, HPS, legalitas, BAST, foto NCR) | — | Hanya ada layanan khusus modul: `BE/.../WorkflowManagement/Services/WorkflowFileStorageService.cs`, `.../Services/WorkflowAttachmentService.cs`, `BE/.../LifecycleManagement/Services/IHrdLifecycleAttachmentService.cs`. Tidak ditemukan layanan berkas bersama di `Services/` atau `Areas/Platform` | `Missing` | Ada dua pilihan: memakai lampiran workflow (`TrxWorkflowAttachment`, terikat ke instance persetujuan) atau membuat penyimpanan berkas milik Pengadaan. Validasi tipe berkas BRD B6 (allowlist, signature, ukuran) belum terbukti ada di mana pun | Celah keamanan unggahan (UAT-15) bila dibuat ulang tanpa standar |
| `PBJ-CAP-005` | Jejak audit (siapa, kapan, alasan) | Infrastruktur dasar | `BE/Models/IdentityModel.cs#IdentityModel` (`CreateBy`, `UpdateBy`, `DeleteBy`, `CancelBy` + waktunya); riwayat status workflow `TrxWorkflowStatusHistory.cs`. Tidak ada tabel log kejadian bersama: `ApplicationDbContext` hanya punya satu `DbSet` audit, khusus HR (`HrdClinicalPeerReviewAuditLog`) | `Reuse with adapter` | Cap waktu dan pelaku tersedia di setiap tabel. Riwayat perubahan penting (nominal, status vendor, revisi rencana) harus dibuat sebagai tabel riwayat milik Pengadaan, sesuai pola modul lain | Laporan Audit (11.6) bergantung pada kelengkapan tabel riwayat yang dirancang |
| `PBJ-CAP-006` | Hak akses per menu dan aksi | Platform Authorization (`Sec`) | `BE/Attributes/AccessPermissionAttribute.cs`; `BE/Services/Security/PermissionRegistryDescriptor.cs` (identitas izin = pasangan resource + action pada `[AccessPermission]`, disemai otomatis) | `Ready to reuse` | Pengadaan cukup menamai resource-nya sendiri (mis. `ProcurementRequest : Approve`) | — |
| `PBJ-CAP-007` | Pembatasan data per unit (Unit A tidak melihat PR Unit B, UAT-03) | Platform Authorization / Workforce | `BE/Services/Security/OrganizationAuthorizationProjectionService.cs` (proyeksi `WfpOrganizationAssignment` → `AspNetUserOrganization`). `AspNetUserOrganization` hanya dipakai di controller HR (`EmployeeController`, `DoctorController`, `ExternalUserController`, `WfpOrganizationAssignmentController`) | `Reuse with adapter` | Data "pengguna ini milik unit mana" sudah ada. Penyaring query per unit belum ada sebagai layanan bersama, sehingga Pengadaan harus menerapkannya di service-nya sendiri | Pengguna tanpa penempatan organisasi tidak akan melihat PR unit mana pun |
| `PBJ-CAP-008` | Master unit peminta | HR Master Organisasi / Health Services | `BE/.../HumanResource/MasterData/Organization/Models/MstOrganizationUnit.cs`, `.../MstDepartment.cs`; `BE/Areas/HealthServices/MasterData/Models/MstServiceUnit.cs` | `Unknown` | Ada tiga master unit. Belum diputuskan mana yang disebut "unit" pada PR dan pagu (`PBJ-OQ-022`). Mesin workflow dan proyeksi hak akses memakai master HR | Memilih master yang berbeda dari mesin workflow membuat matriks per unit tidak bisa dipakai |
| `PBJ-CAP-009` | Identitas vendor (`PBJ-DEC-010`) | Administrator / Master Data | `BE/Areas/Administrator/MasterData/Models/MstSupplier.cs#MstSupplier` (`SupplierCode`, `LegalName`, `TaxNumber`, `BusinessLicenseNumber`, flag kategori `IsPharmacySupplier`/`IsMedicalDeviceSupplier`/...); `BE/.../Controllers/SupplierController.cs` (`[Tags("Administrator / Master Data / Supplier")]`, `GET options`, `[AccessPermission("Supplier", "Read")]`); `FE/src/app/administrator/master-data/supplier/page.jsx` | `Ready to reuse` | Identitas dan pencarian vendor siap. Field legalitas tunggal (`BusinessLicenseNumber`) tidak memuat masa berlaku dan berkas, jadi tetap dibutuhkan tabel pendamping | — |
| `PBJ-CAP-010` | Status vendor pengadaan: aktif/dibatasi/diblokir (5.5, UAT-07) | Administrator ↔ Pengadaan | `BE/.../MstSupplier.cs` `IsBlacklisted`, `BlacklistReason`, `IsActive` | `Conflict` | `PBJ-DEC-010` menaruh status pengadaan di tabel pendamping, padahal master sudah punya `IsBlacklisted`. Bila keduanya hidup, vendor bisa "diblokir" di satu tempat dan "aktif" di tempat lain (`PBJ-OQ-023`) | Pemilihan vendor yang seharusnya diblokir |
| `PBJ-CAP-011` | Tampilan rekening vendor tersamarkan (`PBJ-DEC-015`) | Administrator / Master Data | `BE/.../MstSupplier.cs` `BankName`, `BankAccountNumber`, `BankAccountName`; `BE/.../DTOs/SupplierDtos.cs` baris 71 dan 268 (`BankAccountNumber` dikirim utuh). Tidak ditemukan penyamaran pada `SupplierController` | `Reuse with adapter` | Rekening memang dikelola di master (sesuai `PBJ-DEC-015`). Pengadaan wajib menyamarkannya sendiri di DTO bacanya, karena endpoint master mengirim nomor utuh | Bila FE Pengadaan memanggil endpoint master langsung, nomor utuh bocor. Catatan untuk pemilik Master Data (di luar scope): endpoint master sendiri juga tidak menyamarkan |
| `PBJ-CAP-012` | Master barang/jasa untuk baris PR dan rencana | — | Tidak ada master barang umum. Yang ada: `BE/Areas/HealthServices/MasterData/Models/MstDrug.cs` (obat), `.../MstMeasurement.cs` dan `.../MstMeasurementConversion.cs` (satuan). Item PO Finance memakai snapshot nama (`FIN-DEC-274`) | `Missing` | Perlu keputusan: baris PR berisi teks bebas + satuan dari `MstMeasurement`, merujuk `MstDrug` untuk obat, atau master barang baru (`PBJ-OQ-024`) | Konsolidasi kebutuhan (2.2) sulit tanpa identitas barang, karena barang tidak boleh dicocokkan berdasarkan nama saja (selaras `FIN-DEC-275`) |
| `PBJ-CAP-013` | Data stok untuk cek stok | Farmasi (obat saja) | `BE/Areas/HealthServices/PharmacyManagement/Models/PhmDrugStock.cs` | `Missing` (stok umum) | Tidak dibutuhkan pada slice pertama karena cek stok manual (`PBJ-DEC-008`) | — |
| `PBJ-CAP-014` | Membaca PO berdasarkan nomor PO BLM (`PBJ-DEC-011`) | Finance Management | `BE/Areas/Corporate/FinanceManagement/Purchasing/Models/FinPurchaseOrder.cs` (`PONumber`, `SourceSystem`, `ExternalPurchaseOrderId`, `Status`); indeks unik `IX_FinPurchaseOrder_PONumber` (`BE/Migrations/ApplicationDbContextModelSnapshot.cs` baris 8512–8515); `BE/.../Controllers/FinancePurchaseOrdersController.cs` (`[Tags("Corporate / Finance Management / Purchasing / Purchase Order")]`, `GET`, `GET {id}`, `[AccessPermission("FinancePurchaseOrder", "Read")]`); `BE/.../DTOs/FinancePurchaseOrderDtos.cs#PurchaseOrderQuery` (filter hanya `SupplierId`, `Status`, `DateFrom`, `DateTo`) | `Extend` | Nomor PO unik sehingga aman dijadikan rujukan. Namun daftar PO **tidak** bisa disaring berdasarkan nomor, jadi Finance perlu menambah filter `PONumber` atau endpoint cari-per-nomor. Integrasi BLM belum hidup (`FIN-OQ-141`), sehingga PO BLM belum tentu ada di Finance saat nomor dicatat di Pengadaan | Pencocokan nomor PO baru bisa diverifikasi setelah PO BLM masuk Finance; sebelum itu nomor hanya teks rujukan |
| `PBJ-CAP-015` | Kuantitas diterima (monitoring, 8.1) | Finance Management | `BE/.../Purchasing/DTOs/FinancePurchasingDocumentTrailDtos.cs#PurchasingTrailItemProgress` (`OrderedQuantity`, `ReceivedQuantity`, `RemainingQuantity`, `AcceptedBaseQuantity`, `DamagedBaseQuantity`); `GET {id}/document-trail` pada `FinancePurchaseOrdersController.cs` baris 223 | `Reuse with adapter` | Dibaca per PO. Untuk dashboard banyak paket diperlukan pemanggilan per PO atau endpoint ringkasan baru | Beban query saat paket banyak |
| `PBJ-CAP-016` | Nilai ditagih dan dibayar (`PBJ-DEC-009`) | Finance Management | `BE/.../FinancePurchasingDocumentTrailDtos.cs` bagian `INVOICES`, `PAYMENTS` (`PurchasingTrailInvoice`) | `Reuse with adapter` | Sama dengan `PBJ-CAP-015`. Laporan Realisasi lintas periode butuh agregasi | Angka harus konsisten dengan laporan Finance |
| `PBJ-CAP-017` | Notifikasi tugas persetujuan dan tenggat | Platform Notification (draft) | `BE/docs/module-blueprints/platform-notification/01-existing-capability-map.md` (audit 2026-10-12, blueprint belum ada); `BE/.../WorkflowManagement/Services/UnifiedNotificationGatewayService.cs` | `Unknown` | Tidak diaudit ulang di sini | Tidak memblokir slice pertama |
| `PBJ-CAP-018` | Halaman FE Pengadaan dan menu | — | Tidak ada halaman maupun menu Pengadaan. Menu didefinisikan di `FE/src/utils/menu-sidebar/menu-items.jsx` (kelompok `corporateFinance` baris 1846) dan disaring di `FE/src/utils/menu-sidebar/role/filter-menu-items-by-role.jsx`; halaman Purchasing Finance di `FE/src/app/finance/purchasing/**` | `Missing` | 11 menu dan 61 submenu belum ada | — |

## 4. Kontrak as-is yang relevan

### Corporate / Finance Management / Purchasing / Purchase Order

Base URL: `api/v1/corporate/finance-management/purchasing/purchase-orders` (sudah tersedia)

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar PO, saring per supplier/status/tanggal | `FinancePurchaseOrder : Read` | `PurchaseOrderQuery` | `PagedResult<PurchaseOrderResponse>` |
| `GET` | `/{id}` | Detail satu PO | `FinancePurchaseOrder : Read` | — | `PurchaseOrderResponse` |
| `GET` | `/{id}/document-trail` | Jejak PO → GR → Tukar Faktur → Faktur → Pembayaran | `FinancePurchaseOrder : Read` | — | jejak dokumen |

Kode 403 berarti pengguna tidak punya izin baca PO. Petugas Pengadaan butuh izin baca ini dari pemilik Finance.

### Corporate / Human Resource / Workflow Management / Approval Inbox

Base URL: `api/v1/corporate/human-resource/approval-inbox` (sudah tersedia). Aksi: baca, `Approve`, `Reject`,
`RequestRevision`, `Return`, `Verify`, `Acknowledge`, masing-masing dengan `[AccessPermission("ApprovalInbox", ...)]`.

### Administrator / Master Data / Supplier

Base URL: `api/v1/administrator/master-data/suppliers` (sudah tersedia). `GET /options` untuk pilihan vendor,
`[AccessPermission("Supplier", "Read")]`. **Respons detail membawa nomor rekening utuh.**

## 5. Konflik dan hal yang belum diketahui

| ID | Jenis | Uraian | Pemilik keputusan |
| --- | --- | --- | --- |
| `PBJ-OQ-021` | Conflict → Open Question | `PBJ-DEC-005` mengandaikan mesin approval platform. Mesin itu ada (`Wfl`), tetapi perlu diperluas agar bisa menerima jenis dokumen non-HR dan penyetuju berbasis peran/jenis dokumen. Pilihannya: (a) memperluas `Wfl` dengan persetujuan pemiliknya; (b) Pengadaan membuat persetujuan sendiri mengikuti pola Finance (`FinanceApprovalTierResolver`); (c) campuran: matriks milik Pengadaan, kotak masuk bersama | Pengguna sesi + pemilik `WorkflowManagement` |
| `PBJ-OQ-022` | Open Question | "Unit" pada PR dan pagu memakai `MstOrganizationUnit`/`MstDepartment` (HR) atau `MstServiceUnit` (Health Services)? | Pengguna sesi |
| `PBJ-OQ-023` | Conflict | Status blokir vendor: tetap memakai `MstSupplier.IsBlacklisted` (milik Administrator) atau status pengadaan sendiri di tabel pendamping? Bila keduanya ada, mana yang menang? | Pengguna sesi + pemilik Master Data |
| `PBJ-OQ-024` | Open Question | Identitas barang pada baris PR/rencana: teks bebas + satuan, rujukan `MstDrug` untuk obat, atau master barang baru milik Pengadaan? | Pengguna sesi |
| `PBJ-OQ-025` | Open Question | Penyimpanan berkas: lampiran workflow, atau penyimpanan berkas milik Pengadaan dengan validasi berkas BRD B6? | Pengguna sesi (keamanan: pemilik sistem) |
| `PBJ-UNK-001` | Unknown | Notifikasi (`PBJ-CAP-017`) | Ditunda; tidak memblokir slice pertama |

## 6. Pemicu impact scan

Peta ini harus dinyatakan usang dan diperiksa ulang secara terbatas bila salah satu berubah:
`Areas/Corporate/HumanResource/WorkflowManagement/**`, `.../MasterData/Workflow/**`,
`Areas/Platform/NumberSeriesManagement/**`, `Areas/Administrator/MasterData/Models/MstSupplier.cs` beserta DTO-nya,
`Areas/Corporate/FinanceManagement/Purchasing/**`, `Services/Security/**`, `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`,
dan `FE/src/utils/menu-sidebar/**`.

**Tambahan revisi 2:** `Areas/Corporate/ProcurementManagement/Common/**`, `.../Request/Services/**` dan `.../Request/Models/**`,
`.../Planning/Services/ProcurementBudgetCommitmentService.cs`, `.../Planning/Services/ProcurementPackagePlanService.cs`,
`.../Vendor/Services/ProcurementVendorEligibilityService.cs`, kontrak `perencanaan-permintaan/contracts/integration-contract.md`
dan `manajemen-vendor/contracts/integration-contract.md`, serta `FE/src/components/view/corporate/procurement/shared/**` dan
`FE/src/lib/services/corporate/procurement/procurement-approval-service.js`.

---

## 7. Impact scan `pemilihan-penyedia` — 26 November 2026

### 7.0 Batas dan cara

| Hal | Isi |
| --- | --- |
| Pemicu | Scope pass `pemilihan-penyedia` (`PBJ-DEC-066`..`081`) memakai peta yang dibuat pada `100c011d`/`763d665b`. Sejak itu dua sub-modul Pengadaan sudah dibangun |
| Diperiksa | Kebutuhan K-05 dan K-07 beserta titik sentuhnya: PR dan rencana paket, pagu, kelayakan vendor, `Wfl`, nomor, lampiran, riwayat, cakupan data, izin, PO Finance, menu dan komponen FE Pengadaan |
| Tidak diperiksa | Perilaku runtime (tidak ada eksekusi); notifikasi (`PBJ-CAP-017` tetap `Unknown`); kebutuhan `kontrak-pelaksanaan` |
| Perubahan pada jalur pemicu revisi 1 | `WorkflowManagement` 11 commit dan `MasterData/Workflow` 2 commit (perluasan `PBJ-DEC-019`/`038`: `IWorkflowReferenceLifecycleHandler`, `CreateAndSubmitOnBehalfAsync`, `ActivationConditionCode`, ditambah perubahan HR); `Services/Security` 1 commit (Turnstile, tidak relevan); `SupplierController` 1 commit (kunci blokir `PBJ-DEC-060`); registry 7 commit (baris `Prc`); menu FE 57 commit. `NumberSeriesManagement`, `MstSupplier`, `SupplierDtos`, dan `FinanceManagement/Purchasing`: **0 commit** |

### 7.1 Status terkini baris lama yang dipakai `pemilihan-penyedia`

| ID | Status 12 Nov | Status 26 Nov | Bukti (`@d5fdc8fa` / `@3e5386679`) | Arti bagi `pemilihan-penyedia` |
| --- | --- | --- | --- | --- |
| `PBJ-CAP-001` Mesin persetujuan | `Extend` | `Reuse with adapter` | `BE/.../ProcurementManagement/Common/Services/ProcurementApprovalGateway.cs#SubmitAsync`, `#WithdrawAsync`, `#CloseReturnedAsync`; `.../ProcurementWorkflowLifecycleHandler.cs#CanHandle` (semua `ReferenceType` berawalan `PBJ_`); `BE/.../MasterData/Workflow/Models/MstWorkflowStep.cs#ActivationConditionCode`; `.../Common/Seeders/ProcurementWorkflowDefinitionSeeder.cs` (7 definisi, fail-closed) | Perluasan `Wfl` sudah selesai. Paket cukup menambah dua definisi (persiapan, penetapan) dan penerap statusnya di Pengadaan sendiri. Syarat lama tetap: penyetuju wajib punya data pegawai |
| `PBJ-CAP-002` Kotak masuk | `Reuse with adapter` | `Reuse with adapter` | `FE/src/app/corporate/procurement/approvals/inbox`; `FE/src/lib/constants/corporate/procurement/procurement-constants.jsx` baris 198–203 (daftar jenis dokumen); `FE/src/lib/services/corporate/procurement/procurement-approval-service.js` baris 24–30 (peta `documentType` → endpoint) | Antrean Persetujuan Pengadaan sudah ada; jenis dokumen paket cukup didaftarkan di dua peta FE itu |
| `PBJ-CAP-003` Nomor dokumen | `Ready to reuse` | `Ready to reuse` | `BE/.../Common/Constants/ProcurementNumberSeries.cs` (`ResetPolicy = Never`, prefix `PR`/`RKB`/`RPK`/`KTL`/`UBV`) | Tambah `PKT` dan `RFQ` (`PBJ-ASM-008`) |
| `PBJ-CAP-004` Lampiran | `Missing` | `Extend` | `BE/.../Common/Models/PrcAttachment.cs` (`AttachmentCategory` teks bebas, versi); `.../Common/Services/ProcurementAttachmentService.cs` (allowlist ekstensi baris 49, cek signature baris 147); `.../IProcurementDocumentAccessProvider.cs` (hak per **dokumen**: None/Read/Edit) | Unggah aman sudah ada. **Belum ada** pembatasan per kategori lampiran, padahal `PBJ-DEC-079` meminta lampiran harga tertutup bagi evaluator. Hak akses hari ini berlaku untuk seluruh lampiran satu dokumen |
| `PBJ-CAP-005` Jejak audit | `Reuse with adapter` | `Ready to reuse` | `BE/.../Common/Models/PrcDocumentHistory.cs`; `.../Common/Services/ProcurementDocumentHistoryWriter.cs`; `.../Common/Constants/ProcurementDocumentHistoryActions.cs` | Riwayat paket, RFQ, perpanjangan batas waktu, pembatalan memakai penulis yang sama |
| `PBJ-CAP-006` Hak akses | `Ready to reuse` | `Ready to reuse` | Tidak berubah | — |
| `PBJ-CAP-007` Cakupan per unit | `Reuse with adapter` | `Reuse with adapter` | `BE/.../Common/Services/ProcurementDataScopeService.cs#ResolveAsync` (aksi `ReadAllUnits` per resource, lalu penempatan departemen) | Paket berisi baris banyak unit; unit peminta melihat paket bila **salah satu** baris berasal dari unitnya. Aturan itu ditulis di service paket |
| `PBJ-CAP-009` Identitas vendor | `Ready to reuse` | `Ready to reuse` | `MstSupplier` 0 commit sejak revisi 1 | — |
| `PBJ-CAP-010` Status blokir | `Conflict` | `Ready to reuse` | Ditutup `PBJ-DEC-021`/`060`; `BE/Areas/Administrator/MasterData/Services/SupplierProcurementStatusService.cs`; dibaca lewat kelayakan (`PBJ-CAP-023`) | `pemilihan-penyedia` tidak membaca `IsBlacklisted` langsung |
| `PBJ-CAP-012` Katalog barang/jasa | `Missing` | `Ready to reuse` | `BE/.../MasterData/Models/MstProcurementItem.cs`, `MstProcurementItemCategory.cs`; `PrcPurchaseRequestLine.ItemId` | Baris paket memakai `ItemId` dari baris PR |
| `PBJ-CAP-014` PO menurut nomor | `Extend` | `Extend` | `BE/.../FinanceManagement/Purchasing/DTOs/FinancePurchaseOrderDtos.cs#PurchaseOrderQuery` baris 79–86 (masih tanpa `PONumber`); jalur Purchasing 0 commit | Sesuai `PBJ-DEC-011`, nomor PO di paket cukup teks rujukan unik **di Pengadaan**. Pencocokan ke Finance tetap menunggu filter Finance dan `FIN-OQ-141` |
| `PBJ-CAP-018` Menu FE | `Missing` | `Missing` (menu 4 dan 6) | `FE/src/utils/menu-sidebar/menu-items.jsx`: 25 rute `/corporate/procurement/**`, tidak satu pun untuk menu 4, 6, atau 11.3 | Nama submenu menunggu `PBJ-OQ-038` |

### 7.2 Kemampuan baru yang dibutuhkan `pemilihan-penyedia`

| ID | Kebutuhan (keputusan) | Pemilik | Bukti (`@d5fdc8fa` / `@3e5386679`) | Status | Gap / penghubung | Risiko |
| --- | --- | --- | --- | --- | --- | --- |
| `PBJ-CAP-019` | Daftar baris PR `APPROVED` yang siap dipaketkan (`PBJ-DEC-071`) | `perencanaan-permintaan` | Kontrak `perencanaan-permintaan/contracts/integration-contract.md` (`PBJ-PP-INT` 1.1) mencantumkan `ProcurementPurchaseRequestService.GetApprovedForPackagingAsync`; `rg` di `Areas/Corporate/ProcurementManagement` **tidak menemukan** metode itu | `Missing` | Kontrak menjanjikan, kode belum ada (laporan kesiapan `perencanaan-permintaan` G-10). Harus dibangun di service pemilik, per **baris**, dengan sisa jumlah beli yang belum dipaketkan | Bila `pemilihan-penyedia` membaca tabel PR langsung, aturan `perencanaan-permintaan` dilangkahi |
| `PBJ-CAP-020` | Menandai baris PR masuk paket, dan melepasnya saat paket batal (`PBJ-DEC-071`, `077`) | `perencanaan-permintaan` | `BE/.../Request/Models/PrcPurchaseRequest.cs` baris 91 `ConvertedPackageId` (tingkat **kepala** PR); `PrcPurchaseRequestLine.cs` tanpa rujukan paket; `.../Request/Services/ProcurementPurchaseRequestVerificationService.cs` baris 327 memakai `ConvertedPackageId`; kontrak `MarkConvertedAsync(prIds, packageId)`; metodenya tidak ada di kode | `Extend` | Model dan kontrak dirancang satu paket per PR. `PBJ-DEC-071` membutuhkan rujukan paket **per baris**, status `CONVERTED_TO_PACKAGE` setelah semua baris beli masuk paket, dan pelepasan saat batal. Butuh migration di tabel milik `perencanaan-permintaan` dan amendment `PBJ-PP-INT` | Perubahan menyentuh sub-modul yang sudah `READY_WITH_CONDITIONS`; regresi layar PR dan antrean verifikasi perlu diuji ulang |
| `PBJ-CAP-021` | Rencana paket yang disetujui sebagai sumber paket (`PBJ-DEC-028`) | `perencanaan-permintaan` | Kontrak mencantumkan `ProcurementPackagePlanService.GetApprovedAsync`; metode **tidak ditemukan**. `BE/.../Planning/Models/PrcPackagePlanLine.cs` berisi `NeedPlanLineKey`, `DepartmentId`, `ItemId`, `AllocatedQuantity`; `PrcPurchaseRequestLine.NeedPlanLineKey` ada | `Missing` | Hubungan rencana paket → PR hanya lewat `NeedPlanLineKey`. Baris rencana paket **tidak** membawa sumber dana per baris PR, sehingga komitmen per baris PR (`PBJ-DEC-078`) tetap membutuhkan baris PR. Lihat pertanyaan `PBJ-OQ-039` | Paket yang dibuat dari rencana paket saja tidak punya dasar komitmen pagu |
| `PBJ-CAP-022` | Komitmen pagu per garis pagu dan pelepasannya (`PBJ-DEC-077`, `078`) | `perencanaan-permintaan` | `BE/.../Planning/Services/ProcurementBudgetCommitmentService.cs#FindCurrentLineageAsync(fiscalYear, departmentId, itemCategoryId, fundSourceId)`, `#CommitAsync`, `#ReleaseAsync`; komentar kelas: ikut transaksi pemanggil, idempoten, contoh `PBJ_AWARD`; kontrak INT: kunci unik `(BudgetLineageKey, SourceDocumentType, SourceDocumentId)`, satu dokumen boleh mengikat beberapa pagu | `Ready to reuse` | Pemanggil menjumlahkan baris PR per garis pagu, lalu memanggil `CommitAsync` sekali per garis. Belum ada pemanggil, jadi perilaku runtime belum terbukti | Selisih pembulatan antar garis (aturan `PBJ-DEC-078`) |
| `PBJ-CAP-023` | Kelayakan vendor untuk RFQ dan penetapan (`PBJ-DEC-076`, `PBJ-ASM-006`) | `manajemen-vendor` | `BE/.../Vendor/Services/ProcurementVendorEligibilityService.cs#CheckAsync(supplierId, itemCategoryId)`, `#GetEligibleAsync(itemCategoryId)`; aturan baris 124–126 (`IsActive`, `IsBlacklisted`, kategori aktif) dan 145 (`IsRestricted`); `.../Vendor/DTOs/VendorEligibilityDtos.cs#VendorEligibilityResponse` (`IsEligible`, `IsRestricted`, `Reasons`, `Warnings`) | `Reuse with adapter` | Service menilai **satu** kategori. Paket dengan beberapa kategori butuh irisan di sisi `pemilihan-penyedia` (layak untuk semua kategori). Pemeriksaan ulang saat pengajuan dan saat penyetuju terakhir ditulis di penerap status paket | Penerap status `Wfl` berjalan sesudah keputusan `Wfl` commit; bila vendor tidak layak pada saat itu, dokumen perlu jalur "gagal diterapkan" yang jelas |
| `PBJ-CAP-024` | Jenjang tambahan "Melebihi HPS" dan "metode di luar rentang" (`PBJ-DEC-068`, `073`) | `Wfl` + Pengadaan | `BE/.../Common/Constants/ProcurementWorkflowCodes.cs` (`PBJ_OVER_BUDGET`, `PBJ_OUT_OF_PLAN`); `MstWorkflowStep.ActivationConditionCode`; seeder baris 25 dan 186 | `Reuse with adapter` | Pola langkah bersyarat sudah ada. Tambah dua kode kondisi milik paket dan isi kondisinya saat paket diajukan | — |
| `PBJ-CAP-025` | Menyembunyikan HPS dan harga dari evaluator (`PBJ-DEC-079`) | Platform Authorization + Pengadaan | `BE/.../Common/Services/ProcurementDataScopeService.cs` baris 62–66 memanggil `_accessPermissionService.HasAccessAsync(user, resource, action)`; pola aksi tambahan `ReadAllUnits` (`BE/.../Request/ProcurementRequestExplicitPermissions.cs`) | `Reuse with adapter` | Mekanisme cek izin per aksi sudah ada. Pengadaan perlu satu aksi tambahan (mis. "baca harga") dan penyaring kolom di DTO paket/penawaran. Lampiran harga bergantung pada `PBJ-CAP-004` | Penyaringan hanya di FE akan membocorkan harga lewat API (`PBJ-AC-029`) |
| `PBJ-CAP-026` | Evaluator hanya melihat paket yang ditugaskan (`PBJ-DEC-074`, `080`) | Pengadaan | `ProcurementDataScopeService` hanya mengenal "semua unit" atau "unit penempatan"; tidak ada cakupan per penugasan di `Areas/Corporate/ProcurementManagement` | `Missing` | Perlu cakupan baru "per penugasan" untuk evaluator, termasuk untuk `IProcurementDocumentAccessProvider` lampiran | Evaluator dari unit pengguna bisa melihat paket lain bila cakupan unit dipakai |
| `PBJ-CAP-027` | Master metode pengadaan berkonfigurasi (`PBJ-DEC-068`, `069`) | Pengadaan | `BE/.../MasterData/Models/MstProcurementType.cs` hanya `TypeCode`, `TypeName`, `IsGoods`, `Description`, `IsActive` (ini **jenis**, bukan metode); tidak ada model metode | `Missing` | Master baru: rentang HPS, minimum penawaran, wajib evaluasi teknis, dokumen wajib | — |
| `PBJ-CAP-028` | Checklist dokumen wajib persiapan (`PBJ-ASM-007`) | Pengadaan | `MstProcurementType` tanpa daftar dokumen; `MstProcurementCategoryDocumentRequirement` milik `manajemen-vendor` adalah dokumen **legalitas vendor** per kategori, bukan dokumen paket | `Missing` | Jangan memakai ulang kewajiban dokumen vendor untuk checklist paket; konsepnya berbeda | Salah pakai tabel vendor membuat aturan vendor dan paket saling memengaruhi |
| `PBJ-CAP-029` | Komponen FE bersama untuk layar paket dan RFQ | Frontend `perencanaan-permintaan` | `FE/src/components/view/corporate/procurement/shared/` (`procurement-approval-panel`, `procurement-attachment-panel`, `procurement-document-tabs`, `procurement-history-panel`, `procurement-list-filter`, `procurement-status-badge`) | `Reuse with adapter` | Daftarkan jenis dokumen dan jenis status baru (pola `kind` dari laporan `FE-PBJ-VD-002`). Panel lampiran belum bisa memisah lampiran teknis dan harga (`PBJ-CAP-004`) | — |
| `PBJ-CAP-030` | Folder dan registry submodule `pemilihan-penyedia` | Registry | `BE/docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 55–59: `Procurement`, `Planning`, `Request`, `Common`, `Vendor` | `Missing` | Perlu baris submodule baru di bawah `ProcurementManagement` (pola `PBJ-DEC-063`) sebelum model pertama | Model di folder tanpa baris registry gagal QBE |

### 7.3 Kontrak as-is yang menjadi titik sentuh

**`perencanaan-permintaan` → `pemilihan-penyedia` (`PBJ-PP-INT` 1.1):** tiga dari empat metode yang dijanjikan belum ada di
kode (`GetApprovedForPackagingAsync`, `MarkConvertedAsync`, `GetApprovedAsync`). Hanya `ProcurementBudgetCommitmentService`
yang ada. Bentuk `MarkConvertedAsync(prIds, packageId)` juga sudah tidak cocok dengan `PBJ-DEC-071` (per baris).

**`manajemen-vendor` → `pemilihan-penyedia` (`PBJ-VD-INT` 1.1):** `CheckAsync` dan `GetEligibleAsync` ada dan cocok dengan
kontrak. Contoh respons tidak layak: `{ "isEligible": false, "isRestricted": false, "reasons": ["Dokumen wajib Izin Pedagang
Besar Farmasi kedaluwarsa sejak 16-11-2026."] }`.

### 7.4 Conflict dan Unknown

| ID | Jenis | Uraian | Pemilik keputusan |
| --- | --- | --- | --- |
| ~~`PBJ-OQ-039`~~ (ditutup `PBJ-DEC-082`, 26 November 2026: isi paket selalu baris PR, rencana paket rujukan) | Conflict → Open Question | `PBJ-DEC-028` membolehkan paket dibuat dari rencana paket **dan/atau** PR, tetapi `PBJ-DEC-071` dan `PBJ-DEC-078` mengandaikan isi paket selalu baris PR (komitmen pagu butuh unit dan sumber dana dari baris PR). Baris rencana paket tidak membawa sumber dana per baris PR (`PBJ-CAP-021`). Perlu diputuskan apakah rencana paket hanya menjadi **rujukan** pembanding, sementara isi paket selalu baris PR yang disetujui | Pengguna sesi |
| `PBJ-UNK-002` | Unknown | Perilaku penerap status `Wfl` bila pemeriksaan ulang kelayakan vendor gagal **setelah** penyetuju terakhir menyetujui (`PBJ-DEC-076`). Pola `perencanaan-permintaan`: keputusan `Wfl` tetap sah dan kegagalan sinkron dicatat untuk diulang. Belum terbukti cocok untuk penolakan karena aturan bisnis | Desain (`design-business-module`), mungkin butuh keputusan pengguna |
| `PBJ-UNK-001` | Unknown | Notifikasi (`PBJ-CAP-017`) | Tetap ditunda |

### 7.5 Ringkasan

- **Siap dipakai ulang:** nomor, riwayat dokumen, hak akses, identitas vendor, status blokir, katalog, komitmen pagu.
- **Pakai dengan penghubung:** persetujuan `Wfl` (dua definisi baru), kotak masuk, cakupan unit, kelayakan vendor (irisan
  kategori), jenjang bersyarat, penyaringan harga per izin, komponen FE bersama.
- **Perlu diperluas di pemilik lain:** lampiran (visibilitas per kategori), rujukan paket per baris PR (`perencanaan-permintaan`),
  filter nomor PO Finance (tetap, tidak memblokir).
- **Belum ada:** daftar baris PR siap paket, rencana paket disetujui, cakupan evaluator per penugasan, master metode, checklist
  dokumen paket, baris registry submodule, menu 4 dan 6.
- **Butuh keputusan:** `PBJ-OQ-039` (rencana paket sebagai sumber isi paket).
