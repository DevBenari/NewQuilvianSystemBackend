# Integration Contract — Modul Operasi

Contract `opr-integration-v1`; status `approved`; approved by pemilik kebutuhan pada 2026-08-21; compatibility additive.

| ID | Producer → Consumer | Pemicu | Payload minimum | Idempotency/correlation | Gagal dan rekonsiliasi |
|---|---|---|---|---|---|
| `OPR-INT-001` | Operasi → Inventory/Farmasi | Pemakaian/retur/waste tercatat | case, encounter, item ID, quantity/unit, outcome, batch/serial, occurredAt | `case:usage:revision`; correlation case ID | Simpan `Pending/Failed`, retry; downstream tidak boleh mutasi ganda |
| `OPR-INT-002` | Operasi → Billing | Layanan aktual selesai/material berubah | case, encounter, procedure/tariff, komponen, quantity, tipe create/correct/reverse | `case:charge:component:revision` | Billing authoritative; simpan hasil dan rekonsiliasi per component |
| `OPR-INT-003` | Clinical → Operasi | Validasi procedure/consent | patient procedure, consent type/status/version | correlation case ID | `Ready` ditolak bila invalid, kecuali emergency bypass sah |
| `OPR-INT-004` | HR/Credentialing → Operasi | Penjadwalan/start | workforce, role, active/privilege validity, period | correlation schedule ID | Tolak assignment invalid; bila layanan belum ada tandai dependency blocked |
| `OPR-INT-005` | Operasi → Unit tujuan | Recovery release/handover | encounter, destination, condition summary, device/therapy/risk/instruction, sender | handover ID | Case belum `Completed` sampai diterima; retry tidak membuat handover kedua |
| `OPR-INT-006` | Operasi → Notification | Jadwal/status/handover berubah | event, recipient references, safe summary | event ID | Gagal notifikasi tidak rollback transaksi klinis; retry terpisah |

Transport sinkron/asinkron diputuskan saat capability downstream tersedia. Kontrak bisnis tetap: operasi lokal harus bertahan saat downstream sementara gagal, delivery dapat dipantau, dan retry idempotent.

Payload tidak membawa seluruh catatan klinis. Informasi sensitif hanya dikirim bila diperlukan oleh consumer dan pengguna berwenang.
