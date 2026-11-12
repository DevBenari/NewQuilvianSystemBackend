# Pergerakan Aset — Requirement Traceability

| Field | Value |
|---|---|
| Roadmap revision | `REV-1` — 2026-11-12 |
| Blueprint / contract | `AST-BP-001` rev 1 · `AST-MOV-1.0.0` `approved` 2026-11-12 (`AST-DEC-051`) |
| Backend / Frontend SHA | `1f54a175` / `e49e8a81` |

Bukti backend = `V-BE`; tidak adanya automated test bukan gap (`rules/backend/TEST_POLICY.md`).

| Requirement | Decision / aturan | Desain | Task BE | Task FE | Verifikasi | Bukti | Status |
|---|---|---|---|---|---|---|---|
| `CAP-05.1` pengajuan pemindahan | `AST-DEC-011`, `048`; `AST-RULE-012` | state §1 | `BE-AST-016`, `017` | `FE-AST-012` | `AT-MOV-02`, `20..23` | — | belum |
| `CAP-05.2` persetujuan mutasi | `AST-DEC-011`, `048`; `AST-ASM-003` | state §1 | `BE-AST-017` | `FE-AST-012` | `AT-MOV-25`, `26`, `31`; `UAT-MOV-02` | — | belum |
| `CAP-05.3` serah terima antar unit | `AST-RULE-021`; `AST-DES-023`, `024` | `INT-MOV-02` | `BE-AST-017` | `FE-AST-012` | `AT-MOV-01`, `27..29`; `UAT-MOV-01`, `03` | — | belum |
| Koreksi mutasi | `AST-RULE-060`; `AST-DES-025` | state §1 | `BE-AST-017` | `FE-AST-012` | `AT-MOV-03`, `30` | — | belum |
| `CAP-05.4` peminjaman | `AST-DEC-012`; `AST-DES-021`, `022` | state §2–3 | `BE-AST-018` | `FE-AST-013` | `AT-MOV-04`, `05`, `32`, `34`; `UAT-MOV-04` | — | belum |
| `CAP-05.5` pengembalian | `AST-RCG-G07` | `INT-MOV-03` | `BE-AST-018` | `FE-AST-013` | `AT-MOV-06`, `33`; `UAT-MOV-05`, `06` | — | belum |
| `CAP-01.4` mutasi berjalan | — | API 2.3 | `BE-AST-019` | `FE-AST-014` | `AT-MOV-04` (kartu) | — | belum |
| `CAP-11.2` laporan mutasi | — | API 2.4 | `BE-AST-019` | `FE-AST-014` | `AT-MOV-07` | — | belum |
| Penahanan saat opname | `AST-RULE-023`; `AST-DES-026` | `INT-MOV-06` | `BE-AST-021` [INV] | — | `AT-MOV-24` | — | belum (dikerjakan `inventarisasi`) |
| Kepala Unit dari HR | `AST-UNK-004` | `INT-MOV-04` | `BE-AST-005` [REG] | — | `AT-MOV-35` | — | belum; data HR menahan go-live |

**Gap requirement → bukti:** tidak ada.
