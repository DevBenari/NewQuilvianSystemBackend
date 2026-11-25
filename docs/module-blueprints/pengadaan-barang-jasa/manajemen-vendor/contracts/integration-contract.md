# Manajemen Vendor — Kontrak Integrasi

| Field | Nilai |
| --- | --- |
| Kontrak | `PBJ-VD-INT` · `last_changed_in: 1.1` (`PBJ-DEC-064`, 2026-11-19) · status `approved` (`PBJ-DEC-062`) |
| Masukan | `PBJ-DEC-010`, `015`, `021`, `052`, `054`, `060`; laporan `BE-PBJ-VD-005` (D-1..D-4), `BE-PBJ-VD-007` (D-1, D-4) |

Tidak ada sistem luar (BLM, Finance eksternal, OSS/instansi penerbit izin) yang dipanggil. Semua integrasi bersifat
internal dalam satu basis data dan satu transaksi.

## 1. Yang dipakai sub-modul ini

| Kemampuan | Pemilik | Cara | Gagal |
| --- | --- | --- | --- |
| Identitas dan rekening `MstSupplier` | Administrator Master Data | Baca langsung lewat `ApplicationDbContext` | Supplier tidak ada → 404 |
| Penulisan status blokir | Administrator Master Data | `SupplierProcurementStatusService.ApplyAsync(supplierId, isBlacklisted, reason, statusRequestId)` dalam transaksi penerapan keputusan | Gagal → seluruh penerapan dibatalkan; usulan tetap `PENDING_APPROVAL`; sinkron ulang lewat `POST /vendor-status-requests/{id}/workflow/synchronize` (1.1, `VD-007 D-1`). Usulan `BLOCK` yang disetujui juga mengosongkan `IsPreferredSupplier`; `UNBLOCK` tidak mengembalikannya (1.1, `VD-007 D-4`) |
| Approval | `Wfl` lewat `ProcurementApprovalGateway` | `ReferenceType = PBJ_VENDOR_STATUS`; `IdempotencyKey = PBJ_VENDOR_STATUS:{id}:{SubmissionCount}` | Definisi tidak aktif → 409 `PBJ-VAL-091` |
| Lampiran dan riwayat | `perencanaan-permintaan` (dokumen bersama) | `ProcurementAttachmentService`, `ProcurementDocumentHistoryWriter` | — |
| Kategori | `perencanaan-permintaan` | Baca `MstProcurementItemCategory` | Kategori nonaktif → `PBJ-VAL-100` |

## 2. Yang disediakan bagi sub-modul lain

| Kemampuan | Konsumen | Kontrak |
| --- | --- | --- |
| Kelayakan vendor per kategori | `pemilihan-penyedia` (RFQ, penetapan) | `ProcurementVendorEligibilityService.CheckAsync(supplierId, itemCategoryId)` → `{ supplierId, supplierCode, supplierName, itemCategoryId, isEligible, isRestricted, reasons[], warnings[] }` (1.1, `VD-005 D-1`); `GetEligibleAsync(itemCategoryId)` untuk daftar pilihan. Hanya mencegah pilihan **baru** (`PBJ-DEC-054`). **(1.1)** Kategori nonaktif → semua vendor tidak layak untuk kategori itu (`VD-005 D-3`, aturan bisnis `PBJ-DEC-064`) |
| Status blokir | Finance, Master Data | Tetap dibaca dari `MstSupplier.IsBlacklisted` (`PBJ-DEC-021`) |
| Rekap penilaian | `kontrak-pelaksanaan` kelak (`S-V7`) | Tabel penilaian menyimpan `SourceType`; evaluasi per kontrak akan menulis `CONTRACT` |

## 3. Pesan dan versi

Tidak ada pesan asinkron. Perubahan bentuk `VendorEligibilityResponse` menaikkan `PBJ-VD-INT` dan wajib diberitahukan ke
desain `pemilihan-penyedia`.

**Perubahan 1.0 → 1.1 (`PBJ-DEC-064`, 19 November 2026).** `VendorEligibilityResponse` bertambah `supplierCode`,
`supplierName`, dan `warnings[]`, yaitu catatan yang tidak menggugurkan kelayakan: alasan Dibatasi dan dokumen wajib yang
segera kedaluwarsa. Perubahan ini aditif; konsumen lama tetap bisa membaca `isEligible` dan `reasons[]`.
**Contoh:** vendor Dibatasi sampai 30-06-2027 dengan Izin PBF berlaku 20 hari lagi → `isEligible = true`,
`isRestricted = true`, `reasons = []`, `warnings = ["Dibatasi sampai 30-06-2027: terlambat kirim tiga kali.",
"Dokumen wajib Izin PBF berakhir dalam 20 hari."]`. Desain `pemilihan-penyedia` wajib membaca versi 1.1 ini.
