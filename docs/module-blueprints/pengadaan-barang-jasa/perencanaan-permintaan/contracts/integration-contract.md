# Perencanaan & Permintaan — Kontrak Integrasi

| Field | Nilai |
| --- | --- |
| Kontrak | `PBJ-PP-INT` · `last_changed_in: 1.2` (`PBJ-DEC-095`, 2026-11-26) · status `approved` (`PBJ-DEC-034`) |
| Pemilik | Pengguna sesi (juga pemilik `Wfl`, `PBJ-DEC-026`) · `approved_by: Pengguna sesi` · `approved_at: 2026-11-12`; versi 1.2 disahkan 2026-11-26 |
| Masukan | `PBJ-DEC-008`, `009`, `019`, `020`, `025`, `028`; capability map revisi 1. Versi 1.2: `PBJ-DEC-071`, `077`, `082`, `084`, `095`; `PBJ-PS-INT` 1.0 bagian 3; capability map revisi 2 `PBJ-CAP-019`..`022` |

Sub-modul ini **tidak memanggil sistem luar** (BLM, gudang, atau keuangan eksternal). Semua integrasinya
internal, di dalam proses aplikasi yang sama, dan sinkron. Integrasi stok ditunda (`PBJ-DEC-008`,
`PBJ-OQ-008`).

## 1. Yang dipakai sub-modul ini

| Mitra | Arah | Apa | Cara | Bila gagal |
| --- | --- | --- | --- | --- |
| `Wfl` — `WorkflowService` | Tulis | Membuat dan mengajukan instance approval **atas nama pemohon asli** | **1.1 (`PBJ-DEC-038`):** `CreateAndSubmitOnBehalfAsync(CreateWorkflowInstanceRequest, requesterUserId)` lewat `ProcurementApprovalGateway`, dengan `IdempotencyKey` wajib `<ReferenceType>:<DocumentId>:<SubmissionCount>`. Urutan **"Wfl dulu, baru status dokumen"**: instance dibuat dan diajukan lebih dulu, status dokumen diubah sesudahnya. (1.0: `CreateAsync` lalu `SubmitAsync` di dalam transaksi aksi — tidak dapat dipakai karena pengaju tercatat sebagai pengguna login, mis. verifikator) | Dokumen belum diubah; pengguna mengulang dengan kunci yang sama sehingga instance yang sudah terbentuk dipakai ulang, tidak digandakan. Definisi tidak aktif/tanpa langkah aktif → `PBJ-VAL-091` |
| `Wfl` — `WorkflowReferenceLifecycleService` | Panggil balik | Menerapkan hasil approval ke dokumen | `IWorkflowReferenceLifecycleHandler.SynchronizeAsync` setelah transaksi `Wfl` commit | Keputusan `Wfl` tetap sah; galat dicatat; dokumen tetap `PENDING_APPROVAL`; diulang lewat sinkron `Wfl` |
| Platform — `NumberSeriesAllocator` | Tulis | Nomor PR, RKB, RPK, KTL | `AllocateAsync(NumberAllocationRequest)` | Aksi gagal; nomor yang telanjur dialokasikan hangus (`DEC-PLT-008`) |
| HR — `MstDepartment`, `AspNetUserOrganization` | Baca | Unit dan cakupan pengguna | Query langsung, `AsNoTracking` | — |
| Health Services — `MstMeasurement`, `MstDrug` | Baca | Satuan dan rujukan obat di katalog | Query langsung | — |

## 2. Yang disediakan bagi sub-modul lain

Sub-modul lain memanggil service ini, tidak menulis tabelnya.

| Konsumen | Service / metode | Kegunaan | Aturan |
| --- | --- | --- | --- |
| `pemilihan-penyedia` | `ProcurementPurchaseRequestService.GetPackageableLinesAsync(query)` **(1.2)** | Baris PR siap dipaketkan: PR `APPROVED`, `PurchaseQuantity > 0`, `PackageId` kosong, dalam cakupan unit pengguna (`PBJ-DEC-071`) | Hanya baca |
| `pemilihan-penyedia` | `ProcurementPurchaseRequestService.MarkLinesPackagedAsync(lineIds, packageId)` **(1.2)** | Mengisi `PackageId`/`PackagedAt` baris; PR menjadi `CONVERTED_TO_PACKAGE` bila semua baris belinya sudah berpaket | Ikut transaksi pemanggil; idempoten untuk `packageId` yang sama; 409 `PBJ-VAL-121` bila baris sudah punya paket lain |
| `pemilihan-penyedia` | `ProcurementPurchaseRequestService.ReleaseLinesAsync(lineIds, packageId)` **(1.2)** | Mengosongkan `PackageId` baris saat paket batal/ditolak atau baris dikeluarkan; PR kembali `APPROVED` (`PBJ-DEC-077`) | Ikut transaksi pemanggil; hanya melepas baris yang `PackageId`-nya sama |
| `pemilihan-penyedia`, `kontrak-pelaksanaan` | `ProcurementBudgetCommitmentService.FindCurrentLineageAsync` / `CommitAsync` / `ReleaseAsync` | Mencatat dan melepas komitmen pagu (`PBJ-DEC-009`) | Kunci unik `(BudgetLineageKey, SourceDocumentType, SourceDocumentId)` di antara baris aktif mencegah komitmen ganda; satu dokumen boleh mengikat beberapa pagu (beda kategori) |
| Semua sub-modul | `ProcurementAttachmentService`, `ProcurementDocumentHistoryWriter`, `ProcurementDataScopeService`, `ProcurementCatalogService` | Fondasi bersama | `DocumentType` milik sub-modul pemanggil didaftarkan di `ProcurementDocumentTypes` |

Metode untuk `pemilihan-penyedia` dirancang di sini tetapi **dibangun bersama** sub-modul itu, karena belum
ada pemanggilnya.

**Perubahan 1.1 → 1.2 (`PBJ-DEC-095`, 2026-11-26).** Tidak memecah pemanggil mana pun, karena metode 1.1 yang diganti belum
pernah dibangun (`PBJ-CAP-019`..`021`).

| Versi 1.1 | Versi 1.2 | Sebab |
| --- | --- | --- |
| `GetApprovedForPackagingAsync` (PR utuh) | `GetPackageableLinesAsync` (per baris) | Paket dibentuk per baris PR (`PBJ-DEC-071`) |
| `MarkConvertedAsync(prIds, packageId)` | `MarkLinesPackagedAsync` + `ReleaseLinesAsync` | Per baris, dan baris dapat dilepas saat paket batal (`PBJ-DEC-071`, `077`) |
| `ProcurementPackagePlanService.GetApprovedAsync` sebagai sumber paket | Dihapus; rencana paket hanya rujukan lewat `PackagePlanId` dan `NeedPlanLineKey` | `PBJ-DEC-082` |
| Harga perkiraan PR dan pagu tanpa keterangan pajak | Dinyatakan **termasuk PPN** | `PBJ-DEC-084` |

Perubahan data pendukung: `PrcPurchaseRequestLine` mendapat `PackageId` (`uuid`, nullable, index) dan
`PackagedAt` (`timestamptz`, nullable); rinciannya di `pemilihan-penyedia/data/data-dictionary.md`. Kolom kepala
`PrcPurchaseRequest.ConvertedPackageId` tidak dipakai lagi sebagai penanda paket. `ProcurementAttachmentService`
mendapat saringan lampiran harga (`PBJ-DEC-079`) bagi pengguna tanpa izin `ProcurementPackage : ReadPrice`.

## 3. Pesan dan versi

Tidak ada pesan antar sistem. Perubahan tanda tangan metode di bagian 2 adalah perubahan kontrak dan menaikkan
versi `PBJ-PP-INT`.
