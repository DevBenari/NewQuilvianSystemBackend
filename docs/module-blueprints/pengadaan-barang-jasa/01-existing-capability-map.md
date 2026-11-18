# Pengadaan Barang & Jasa — Peta Kemampuan Existing

| Field | Nilai |
| --- | --- |
| Blueprint ID | `PBJ-BP-001` |
| Revisi peta | `1` — audit penuh pertama |
| Status | `draft` |
| Revisi blueprint / keputusan | revisi `1` / `decision_revision 2` (`PBJ-DEC-001`..`018`) |
| Masukan | BRD & PRD v1.0 DRAFT; `00-interview-decisions.md` |
| Backend SHA | `100c011d54147bb19ca4bd874fdfb200448ba46a` (branch `sukmagp`, working tree bersih) |
| Frontend SHA | `763d665b2db3f31cf370ddba68623dc8aa57cf18` (branch `sukmagpV2`) |
| Kontrak yang berlaku | Belum ada |
| Sifat audit | **Hanya membaca.** Tidak ada source aplikasi yang diubah. Database tidak disentuh |
| Tanggal | 12 November 2026 |

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
