# Nilai Aset — Requirement Traceability

| Field | Value |
|---|---|
| Roadmap revision | `REV-1` — 2026-11-12 |
| Blueprint / contract | `AST-BP-001` rev 1 · `AST-NLI-1.0.0` `approved` 2026-11-12 (`AST-DEC-051`) |
| Backend / Frontend SHA | `1f54a175` / `e49e8a81` |

Bukti backend = `V-BE`; tidak adanya automated test bukan gap (`rules/backend/TEST_POLICY.md`).

| Requirement | Decision / aturan | Desain | Task BE | Task FE | Verifikasi | Bukti | Status |
|---|---|---|---|---|---|---|---|
| `CAP-EXT-02` parameter nilai | `AST-DEC-027`; `AST-DES-032`, `033` | arsitektur §4.1 | `BE-AST-014`, `015` | `FE-AST-011` | `AT-NLI-07`, `33`, `34`; `UAT-NLI-01` | — | belum |
| Buku nilai saat aset disetujui / diimpor | `AST-DEC-006`, `023`; `AST-RCG-G19`, `G25` | `INT-REG-30`, `32` | `BE-AST-015` | — | `AT-NLI-08` | — | belum |
| `CAP-EXT-04` hibah masuk | `AST-DEC-040` | integration §3 | `BE-AST-015` | — | `AT-NLI-13` | — | belum |
| Pusat biaya pembebanan | `AST-DEC-036`, `047`; `AST-DES-011` | integration §3 | `BE-AST-026`, `027` | `FE-AST-019` | `AT-NLI-03`, `32`; `UAT-NLI-02` | — | belum |
| `CAP-09.4` penyusutan | `AST-DEC-013`, `014`, `039`; `AST-DES-009`, `010` | flowchart 01 | `BE-AST-027` | `FE-AST-020` | `AT-NLI-01..04`, `09`, `30`, `31`, `37`, `41`; `UAT-NLI-03`, `04` | — | belum |
| `CAP-09.3` perubahan nilai (tanpa revaluasi) | `AST-DEC-016`, `030`, `041`, `042` | state §3–4 | `BE-AST-028` | `FE-AST-021` | `AT-NLI-05`, `06`, `14`, `15`, `35`, `36`; `UAT-NLI-06`, `07` | — | belum |
| Kiriman ke Akuntansi tanpa dobel | `AST-DEC-015`, `038`; `AST-DES-034` | integration §2 | `BE-AST-029` | `FE-AST-023` | `AT-NLI-11`, `12`, `38`, `39` | — | belum |
| `CAP-09.5` rekonsiliasi | `AST-DES-035` | integration §4 | `BE-AST-030` | `FE-AST-023` | `AT-NLI-10`; `UAT-NLI-08` | — | belum |
| `CAP-09.1`, `09.2`, `11.6` nilai dan laporan | `AST-DEC-025` | API 2.5, 2.10 | `BE-AST-031` | `FE-AST-022` | `AT-NLI-40` | — | belum |
| Penyalaan integrasi | `AST-DEC-038`, `047`; `DEP-AST-01..04` | integration §5 | `BE-AST-032` ⛔ | — | `AT-NLI-16`; `UAT-NLI-05` | — | ⛔ `GATE-AST-ACC` |
| `CAP-09.3` revaluasi | `AST-DEC-032` | `EPIC AST-15` `OPEN DECISION` | — | — | — | — | `DEFERRED` — tidak dijadwalkan |

**Gap requirement → bukti:** tidak ada untuk MVP. Revaluasi sengaja `DEFERRED`. Penyalaan integrasi
tertahan gerbang lintas modul yang tercatat, bukan gap.
