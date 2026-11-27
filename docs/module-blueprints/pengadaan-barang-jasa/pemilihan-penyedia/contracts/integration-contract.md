# Pemilihan Penyedia — Kontrak Integrasi

| Field | Nilai |
| --- | --- |
| Kontrak | `PBJ-PS-INT` · `last_changed_in: 1.1` · status `approved` (`PBJ-DEC-095`; 1.1 `PBJ-DEC-097`, 2026-11-27: status `PBJ-VAL-091` = 409) |
| Pemilik | Pengguna sesi (juga pemilik `Wfl` dan Product Owner `perencanaan-permintaan`, `PBJ-DEC-026`) · `approved_by: Pengguna sesi` · `approved_at: 2026-11-26` |
| Masukan | `PBJ-DEC-011`, `071`, `076`, `078`, `082`, `084`; `PBJ-PP-INT` 1.1; `PBJ-VD-INT` 1.1; capability map revisi 2 `PBJ-CAP-019`..`025` |
| Kompatibilitas | Menuntut amendment `PBJ-PP-INT` 1.1 → 1.2 (bagian 3), disetujui bersama desain ini (`PBJ-DEC-095`). Tidak ada sistem luar yang dipanggil: RFQ dan PO BLM tetap di luar sistem |

## 1. Yang dipakai sub-modul ini

| Mitra | Arah | Apa | Cara | Bila gagal |
| --- | --- | --- | --- | --- |
| `perencanaan-permintaan` — `ProcurementPurchaseRequestService` | Baca | Baris PR siap paket | `GetPackageableLinesAsync(query)` — baris PR `APPROVED`, `PurchaseQuantity > 0`, `PackageId` kosong, dalam cakupan pengguna | Daftar kosong |
| `perencanaan-permintaan` — `ProcurementPurchaseRequestService` | Tulis lewat pemilik | Menandai baris masuk paket; PR `CONVERTED_TO_PACKAGE` bila semua baris beli berpaket | `MarkLinesPackagedAsync(lineIds, packageId)` — ikut transaksi pemanggil; idempoten untuk `packageId` sama; 409 `PBJ-VAL-121` bila baris punya paket lain | Seluruh aksi paket digagalkan |
| `perencanaan-permintaan` — `ProcurementPurchaseRequestService` | Tulis lewat pemilik | Melepas baris (paket batal/ditolak, baris dilepas); PR kembali `APPROVED` | `ReleaseLinesAsync(lineIds, packageId)` — hanya melepas baris yang `PackageId`-nya sama | Seluruh aksi digagalkan |
| `perencanaan-permintaan` — `ProcurementBudgetCommitmentService` | Tulis lewat pemilik | Komitmen per garis pagu saat penetapan diterapkan; pelepasan saat cabut/batal | `FindCurrentLineageAsync(fiscalYear, departmentId, itemCategoryId, fundSourceId)`, `CommitAsync("PBJ_PACKAGE_AWARD", awardId, lineage, amount)`, `ReleaseAsync("PBJ_PACKAGE_AWARD", awardId)` — ikut transaksi, idempoten | Garis pagu tidak ditemukan → penetapan tidak diterapkan, galat dicatat untuk sinkron ulang, dan Tim Pengadaan diminta menyusun pagu. PR boleh melebihi pagu (`PBJ-DEC-030`), tetapi komitmen butuh garis pagu yang ada (`PBJ-DEC-094`) |
| `manajemen-vendor` — `ProcurementVendorEligibilityService` | Baca | Kelayakan vendor | `GetEligibleAsync(itemCategoryId)` per kategori paket, lalu irisan; `CheckAsync(supplierId, itemCategoryId)` saat penetapan diajukan dan diterapkan | Pengecualian → aksi digagalkan, tidak ada vendor dianggap layak (fail-closed) |
| Administrator — `MstSupplier` | Baca | Nama, kode, penanda kena pajak dan persentase PPN sebagai nilai bawaan penawaran | Query `AsNoTracking` | — |
| `Wfl` — `ProcurementApprovalGateway` | Tulis | Persiapan (`PBJ_PACKAGE`) dan penetapan (`PBJ_PACKAGE_AWARD`) | `SubmitAsync` (atas nama pengaju, kunci `<ReferenceType>:<Id>:<SubmissionCount>`), `WithdrawAsync`, `CloseReturnedAsync` | Dokumen tidak berubah; definisi tidak aktif → 409 `PBJ-VAL-091` (1.1, `PBJ-DEC-097`) |
| `Wfl` — `ProcurementWorkflowLifecycleHandler` | Panggil balik | Hasil keputusan | `IProcurementWorkflowStatusApplier` baru (`ProcurementSourcingStatusApplier`) untuk dua `ReferenceType` di atas | Keputusan `Wfl` tetap sah; galat dicatat; diulang lewat `/workflow/synchronize` |
| Platform — `NumberSeriesAllocator` | Tulis | Nomor `PKT`, `RFQ` | `AllocateAsync` | Aksi gagal; nomor hangus (`DEC-PLT-008`) |
| Common — `ProcurementAttachmentService`, `ProcurementDocumentHistoryWriter`, `ProcurementDataScopeService` | Tulis/baca | Lampiran, riwayat, cakupan unit | Pola `perencanaan-permintaan` | — |

**Contoh komitmen tanpa garis pagu:** PR Rawat Inap 2027 sumber dana Hibah disetujui walaupun pagu Hibah Rawat Inap belum ada.
Saat penetapan PKT-000012 disetujui, garis pagu itu tidak ditemukan → penetapan tidak diterapkan, layar paket menampilkan
"Pagu Rawat Inap 2027 · Hibah · Alat Kesehatan belum ada", dan Tim Pengadaan meminta Keuangan menyusun pagunya, lalu menekan
sinkron ulang.

## 2. Yang disediakan bagi sub-modul lain

| Konsumen | Service / metode | Kegunaan | Aturan |
| --- | --- | --- | --- |
| `kontrak-pelaksanaan` (belum dirancang) | `ProcurementPackageService.GetOrderedAsync` (bentuk akhir dikunci saat `kontrak-pelaksanaan` dirancang) | Paket `ORDERED` sebagai dasar SPK/kontrak | Hanya baca |
| `perencanaan-permintaan` (layar PR) | `GET /procurement-packages/request-summary` | Nomor, status, dan vendor terpilih paket untuk baris PR unit pengguna | Tanpa harga |
| `perencanaan-permintaan` (laporan "direncanakan vs dipaketkan", `PBJ-DEC-082`) | `ProcurementPackageService.GetPackagedQuantityByNeedPlanLineKeysAsync` (`POST-MVP`) | Jumlah dipaketkan per kunci baris rencana | Hanya baca; dipakai saat laporan dibangun (`POST-MVP`) |

## 3. Amendment `PBJ-PP-INT` 1.1 → 1.2 (disetujui `PBJ-DEC-095`, 2026-11-26)

| Bagian 1.1 | Usulan 1.2 | Sebab |
| --- | --- | --- |
| `GetApprovedForPackagingAsync` (PR utuh) | `GetPackageableLinesAsync` (per baris) | `PBJ-DEC-071` |
| `MarkConvertedAsync(prIds, packageId)` | `MarkLinesPackagedAsync(lineIds, packageId)` + `ReleaseLinesAsync(lineIds, packageId)` | `PBJ-DEC-071`, `077` |
| `ProcurementPackagePlanService.GetApprovedAsync` sebagai sumber paket | Dihapus dari daftar konsumen; rencana paket hanya rujukan lewat `PackagePlanId` dan `NeedPlanLineKey` | `PBJ-DEC-082` |
| Nilai harga perkiraan PR dan pagu | Dinyatakan termasuk PPN | `PBJ-DEC-084` |

Ketiga metode belum ada di kode (`PBJ-CAP-019`..`021`), sehingga amendment ini tidak memecah pemanggil mana pun.

## 4. Yang tidak diintegrasikan

| Hal | Sebab |
| --- | --- |
| Pengiriman RFQ ke vendor (email) | Di luar sistem (`PBJ-DEC-067`, `087`) |
| Pembuatan PO di BLM | Manual; hanya nomor yang dicatat (`PBJ-DEC-011`). Pencocokan ke PO Finance menunggu filter nomor PO Finance (`PBJ-CAP-014`) dan `FIN-OQ-141` |
| Notifikasi aktif di luar `Wfl` | `PBJ-OQ-015`; selama itu tugas `Wfl` dan tanda di daftar (`PBJ-DEC-088`) |
