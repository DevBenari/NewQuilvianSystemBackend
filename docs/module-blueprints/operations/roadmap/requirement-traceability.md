# Requirement Traceability — Modul Operasi

| Requirement | Decision/design | Contract | Backend | Frontend | Bukti target | Status coverage |
|---|---|---|---|---|---|---|
| `OPS-REQ-001` | `OPS-DEC-003/014`, `OPS-CON-001/002` | API cases, `OPR001/002` | `BE-OPR-003` | `FE-OPR-001` | Create/duplicate/concurrency tests | Covered |
| `OPS-REQ-002` | `OPS-DEC-013/025` | `opr-state-v1` | `BE-OPR-003..007` | `FE-OPR-001..005` | State transition suite | Covered |
| `OPS-REQ-003` | `OPS-DEC-004/007/016` | Schedule API, `OPR003` | `BE-OPR-004` | `FE-OPR-002` | Parallel conflict tests | Covered |
| `OPS-REQ-004` | `OPS-DEC-017` | `OPR004/005`, `OPR-INT-004` | `BE-OPR-004` | `FE-OPR-002` | Team/credential contract tests | Partial: external credential blocked |
| `OPS-REQ-005` | `OPS-DEC-005/006/018` | Preparation API, `OPR006/007` | `BE-OPR-005` | `FE-OPR-003` | Readiness/bypass tests | Covered |
| `OPS-REQ-006` | `OPS-DEC-010/011/019` | Execution API, `OPR010` | `BE-OPR-006/007` | `FE-OPR-004/005` | Final/addendum/anesthesia tests | Covered |
| `OPS-REQ-007` | `OPS-DEC-009/020` | Material API, `OPR008/009`, INT-001 | `BE-OPR-008/009` | `FE-OPR-006/007` | Idempotency/consumer tests | Partial: inventory consumer blocked |
| `OPS-REQ-008` | `OPS-DEC-012/021/025` | Recovery/handover API, `OPR011` | `BE-OPR-007` | `FE-OPR-005` | Handover completion tests | Partial: downstream consumer blocked |
| `OPS-REQ-009` | `OPS-DEC-008/022` | State cancel/StoppedEarly | `BE-OPR-006` | `FE-OPR-004` | Illegal cancel tests | Covered |
| `OPS-REQ-010` | `OPS-DEC-023` | `OPR-INT-002` | `BE-OPR-009` | `FE-OPR-007` | Charge/reversal contract tests | Partial: Billing consumer blocked |
| `OPS-REQ-011` | `OPS-DEC-024` | Reports/notification | `BE-OPR-010/011` | `FE-OPR-008/009` | Report/notif resilience tests | Covered |

## Coverage Gap

Tidak ada gap requirement internal. Coverage eksternal yang belum dapat dibuktikan: credential/privilege resolver, mutasi Inventory/Farmasi, transaksi Billing, dan consumer handover Rawat Inap/ICU. Task terkait tetap `BLOCKED`, bukan dianggap selesai.

## Aturan Approval Task

Roadmap tidak mengotorisasi builder. Sebelum implementasi, pilih tepat satu task ID, pastikan dependency terpenuhi, kunci acceptance criteria, tentukan task mode/write target/branch, lalu minta approval eksplisit.
