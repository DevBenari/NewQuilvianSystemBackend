# Farmasi — Integration Contract Routing Depo

Contract version: `PHA-INT-ROUTING-v1`; status `approved`; owner Pharmacy Backend; disetujui product/domain owner 21 Agustus 2026.

| Producer | Consumer | Contract | Mode | Idempotency/kegagalan |
| --- | --- | --- | --- | --- |
| Registration | Resolver Farmasi | Encounter snapshot berdasarkan ID | Sinkron internal | Tidak ditemukan menghasilkan rejection |
| Master Data | Resolver Farmasi | Query lokasi eligible | Sinkron internal | Nol/ganda menghasilkan rejection |
| Resolver | Workflow resep | `PharmacyDepotRoutingResult` | In-process | Result tidak mengubah state; caller berhenti saat gagal |

Timeout mengikuti cancellation token request. Tidak ada retry otomatis di dalam resolver; retry berada pada caller dan selalu membaca konfigurasi terbaru. Tidak ada dead-letter karena tidak ada event eksternal.

Traceability: `PHA-DEC-040`, `PHA-DEC-041`, `PHA-DEP-004`, `PHA-DA-001`.
