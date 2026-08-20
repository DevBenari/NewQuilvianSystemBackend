# Billing dan Kasir — Module Status

| Field | Value |
| --- | --- |
| Blueprint ID | `BIL-CASH-001` |
| Revision | `0.4` |
| Blueprint status | `APPROVED` |
| Roadmap revision/status | `1` / `DRAFT_FORWARD_TEST` |
| Current phase | `BKC-PH-001 — READY_FOR_TASK_APPROVAL` |
| Approved at | 20 Agustus 2026, 13:41 WIB |
| Approval evidence | Product/Domain Owner menyatakan “saya aprove” pada percakapan |
| Backend snapshot | `c99f0a51577456c91831870892870f9ae633b4c2` (`Yasmina`) |
| Frontend snapshot | `e555bf2ad6848a1d6cc097ab8c6c5f5259edb151` (`yasmina`) |

## Phase state

| Phase | Status | Evidence |
| --- | --- | --- |
| Discovery/capability/gate/domain | `DONE` | Decision `0.2`, gate/domain `0.3` |
| Business module design | `DONE` | Blueprint dan enam kontrak `0.4` approved |
| Design approval | `DONE` | `BIL-APR-001` ditutup 20 Agustus 2026 |
| Delivery planning | `DONE` | Roadmap revision `1`: 17 BE + 10 FE task |
| Task approval | `READY` | Task pertama yang direkomendasikan `BE-BKC-001` |
| Implementation/verification | `NOT_STARTED` | Belum ada source, migration execution, atau test delivery |

## Delivery dependency

| ID | Dependency | Dampak |
| --- | --- | --- |
| `BKC-BLK-FE-001` | Governance root frontend belum ditemukan | Menahan seluruh frontend write |
| `BKC-BLK-INT-001` | Consumer contract AR/AP belum dibuktikan | Menahan final `BE-BKC-016` |
| `BKC-BLK-PROV-001` | Provider sandbox/callback belum dipilih | Menahan provider E2E, tidak menahan interface |
| `BKC-BLK-DATA-001` | Nilai seed Finance/Inpatient belum diserahkan | Menahan aktivasi master, tidak menahan model/API |

## Next recommended task

Tinjau dan setujui tepat satu task. Urutan aman dimulai dari `BE-BKC-001` (fondasi module dan test harness). Setelah approval task dan write authority backend eksplisit, task tersebut dapat diberikan kepada `$build-module-backend`. Builder tidak boleh mengerjakan task lain sekaligus atau menjalankan migration ke database tanpa izin terpisah.
