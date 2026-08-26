# Farmasi — Module Status

| Field | Value |
| --- | --- |
| Blueprint ID | `PHA-BP-001` |
| Module name | Farmasi |
| Revision | `3` |
| Module status | `PARTIAL`; blueprint routing Depo revision `3` sudah `APPROVED` |
| Current phase | `PHA-PH-008` — menunggu approval task resolver routing Depo |
| Last verified at | `2026-08-20T14:36:54+07:00` |
| Backend source SHA | `767470f742bc6f2eebadbd653a873f69d6f93121` |
| Frontend source SHA | `400104f2a0f3239c14c40f5905b419977a538450` |

## Phase state

| Completed phases | Active phases | Blocked phases |
| --- | --- | --- |
| `PHA-PH-001` sampai `PHA-PH-007`, termasuk roadmap routing `PHA-RM-001-r1` | `PHA-PH-008` menunggu approval `PHA-BE-001` | Integrasi workflow, reservasi, dan penyerahan menunggu keputusan terkait |

## Delivery state

| Backend | Frontend | Integration | Verification |
| --- | --- | --- | --- |
| `NOT_STARTED` | `NOT_STARTED` | `NOT_STARTED` | `NOT_STARTED` |

## Blockers and owners

| Blocker ID | Summary | Owner | Affected phase | Independent continuation |
| --- | --- | --- | --- | --- |
| `PHA-DEP-001` | Billing/Kasir belum terbukti sebagai transaksi authoritative untuk pembayaran, jaminan, reversal, dan refund | Billing/Finance | Kontrak integrasi dan implementasi payment gate | Penilaian requirement serta desain inventory dapat dilanjutkan |
| `PHA-DEP-002` | Saldo, ledger, reservasi atomik, batch, dan mutasi stok belum tersedia | Pharmacy/Inventory | Implementasi dispensing dan persediaan | Arsitektur domain dan roadmap dapat disusun setelah requirement siap |
| `PHA-DEP-003` | SOP dan approval formal untuk kewenangan apoteker, checker kedua, retur, recall, serta obat khusus belum tersedia | Pharmacy/Clinical Governance | Permission dan safety control | Slice routing Depo dapat dirancang secara independen |

## Stale evidence

| Artifact/evidence | Recorded SHA | Current SHA | Required impact review |
| --- | --- | --- | --- |
| `00-interview-decisions.md` | `36d7eca7cd3d4b3f1f6520a6fe9340936cced320` | `767470f742bc6f2eebadbd653a873f69d6f93121` | Sinkronisasi metadata keputusan; keputusan bisnis tetap berasal dari persetujuan owner |
| `01-existing-capability-map.md` | `39b8b69f...` | `767470f742bc6f2eebadbd653a873f69d6f93121` | Impact scan Farmasi sudah dilakukan setelah penggabungan repository; map perlu dinormalisasi ke struktur template pada fase desain |

## Next recommended task

Setujui roadmap `PHA-RM-001-r1` dan task `PHA-BE-001`, aktifkan `TASK MODE: BACKEND`, izinkan write backend lokal, dan konfirmasi branch `Ikbal`. Setelah itu jalankan `build-module-backend` hanya untuk `PHA-BE-001`.

## Status contract

Status `PARTIAL` dipakai karena keputusan dan audit kemampuan sudah tersedia, serta kontrak routing Depo telah disetujui. Namun, arsitektur target, kontrak lintas modul, dan task delivery belum cukup lengkap untuk implementasi source aplikasi.
