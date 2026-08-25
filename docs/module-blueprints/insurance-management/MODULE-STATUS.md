# Insurance Management — Module Status

| Field | Value |
| --- | --- |
| Blueprint ID | `INS-BP-001` |
| Module name | Insurance Management |
| Revision | `1` |
| Module status | `PARTIAL` |
| Current phase | `INS-PH-001` |
| Last verified at | `2026-08-14T00:00:00+07:00` |
| Backend source SHA | `cd6b7cfd34f79448445db5018a07040abead35a6` |
| Frontend source SHA | `91df72cf05224c25c681f6f86b176c83e9610240` |

## Phase state

| Completed phases | Active phases | Blocked phases |
| --- | --- | --- |
| None | `INS-PH-001` — close product decisions | `INS-PH-002`, `INS-PH-003`, `INS-PH-004` |

## Delivery state

| Backend | Frontend | Integration | Verification |
| --- | --- | --- | --- |
| `NOT_STARTED` | `NOT_STARTED` | `NOT_STARTED` | `NOT_STARTED` |

## G5-H pilot context

Ini adalah G5-H lifecycle validation test fixture / pilot artifact dengan hasil validasi `PASS`. Insurance Management production work belum diotorisasi; module status sengaja tetap `PARTIAL`, `INS-PH-001` tetap `IN_PROGRESS`, dan `INS-DEC-001`–`INS-DEC-006` tetap `OPEN`. Completion pilot bukan berarti Insurance Management `DONE`, dan pilot artifact ini tidak mengotorisasi application implementation.

## Blockers and owners

| Blocker ID | Summary | Owner | Affected phase | Independent continuation |
| --- | --- | --- | --- | --- |
| `INS-BLK-001` | Batas modul belum memutus apakah mencakup eligibility, Guarantee Letter, pre-authorization, klaim, rekonsiliasi, dan/atau collection. | Product/business owner | `INS-PH-002`–`INS-PH-004` | Audit foundation dan penutupan keputusan (`INS-PH-001`) aman. |
| `INS-BLK-002` | State machine, actor, SLA, dan approval untuk claim/GL belum terbukti di source atau disetujui. | Insurance operations owner | `INS-PH-002`–`INS-PH-004` | Reuse master data dan coverage runtime dapat dianalisis tanpa mengubah rule. |
| `INS-BLK-003` | Kontrak provider eksternal, identitas pasien yang diizinkan, idempotency, kegagalan/retry, dan rekonsiliasi belum ditetapkan. | Integration/security owner | `INS-PH-003`–`INS-PH-004` | Tidak ada external call yang aman untuk dirancang final. |

## Stale evidence

| Artifact/evidence | Recorded SHA | Current SHA | Required impact review |
| --- | --- | --- | --- |
| `02-existing-capability-map.md` | Captured above | Verify before implementation | Required if either source SHA changes |

## Next recommended task

Tidak ada production action yang diotorisasi setelah pilot. Jika user secara eksplisit mengaktifkan Insurance Management sebagai real production/module initiative, resume dari `INS-PH-001`; hanya pada saat itu business decision closure/interview dapat dijalankan.
