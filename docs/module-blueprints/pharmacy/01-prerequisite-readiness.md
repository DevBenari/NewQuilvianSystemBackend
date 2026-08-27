# Farmasi — Prerequisite Readiness

| dependency_id | capability_or_module | dependency_type | owner | evidence | capability_status | required_by | blocking_impact | independent_continuation | source_sha | next_owner_or_action |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `PHA-DEP-001` | Billing/Kasir authoritative | `INTEGRATION` | Billing/Finance | `Areas/HealthServices/PharmacyManagement/Controllers/PrescriptionController.cs`; audit tidak menemukan transaksi pembayaran authoritative | `CONFLICT` | Payment gate, reservasi, reversal, refund | Generic `Prescription.Update` masih dapat menandai paid tanpa bukti transaksi Billing | Requirement dan desain inventory dapat dilanjutkan | `767470f742bc6f2eebadbd653a873f69d6f93121` | Identifikasi owner dan kontrak Billing yang authoritative |
| `PHA-DEP-002` | Ledger, saldo, reservasi, batch, dan mutasi stok Depo | `MODULE_FOUNDATION` | Pharmacy/Inventory | `01-existing-capability-map.md`, `PHA-CAP-004` sampai `PHA-CAP-006` | `MISSING` | Dispensing, transfer, retur, opname, recall | Implementasi transaksi stok tidak aman tanpa saldo dan ledger authoritative | Routing Depo dan desain kontrak dapat dilanjutkan | `767470f742bc6f2eebadbd653a873f69d6f93121` | Susun arsitektur domain setelah requirement slice dinyatakan siap |
| `PHA-DEP-003` | SOP kewenangan dan keselamatan Farmasi | `EXTERNAL` | Pharmacy/Clinical Governance | Keputusan `PHA-DEC-029`, `PHA-DEC-032`, `PHA-DEC-037` sampai `PHA-DEC-039` masih memerlukan approval governance | `UNKNOWN` | Permission, dual-check, retur, recall, clinical hard stop | Role dan kontrol keselamatan tidak boleh ditetapkan developer | Routing Depo dapat dilanjutkan | `not-source-owned` | Dapatkan SOP/approval owner terkait |
| `PHA-DEP-004` | Data lokasi Depo berdasarkan encounter | `PHASE` | Pharmacy + Master Data | `MstDrugStorageLocation` menyediakan `ServiceUnitId`, `ClinicId`, `StorageLocationType`, `IsPharmacyLocation`, `IsAllowDispensing`, `IsMainWarehouse`, dan `IsQuarantineLocation` | `REUSE WITH ADAPTER` | `PHA-DEPOT-ROUTING-v1` | Perlu resolver deterministik dan validasi tepat satu hasil | Dapat masuk desain setelah requirement gate | `767470f742bc6f2eebadbd653a873f69d6f93121` | Nilai kelengkapan requirement dan desain resolver |

## Kesimpulan gerbang

Modul berstatus `PARTIAL`. Routing Depo memiliki fondasi data yang dapat digunakan dengan adapter, sedangkan implementasi reservasi dan dispensing masih bergantung pada ledger stok dan integrasi Billing yang belum siap.

