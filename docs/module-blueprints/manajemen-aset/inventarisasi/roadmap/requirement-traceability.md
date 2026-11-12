# Inventarisasi Fisik — Requirement Traceability

| Field | Value |
|---|---|
| Roadmap revision | `REV-1` — 2026-11-12 |
| Blueprint / contract | `AST-BP-001` rev 1 · `AST-INV-1.0.0` `approved` 2026-11-12 (`AST-DEC-051`) |
| Backend / Frontend SHA | `1f54a175` / `e49e8a81` |

Bukti backend = `V-BE`; tidak adanya automated test bukan gap (`rules/backend/TEST_POLICY.md`).

| Requirement | Decision / aturan | Desain | Task BE | Task FE | Verifikasi | Bukti | Status |
|---|---|---|---|---|---|---|---|
| `CAP-07.1` jadwal | `AST-DEC-018`; `AST-RULE-030` | state §1 | `BE-AST-020`, `021` | `FE-AST-015` | `AT-INV-01`, `31` | — | belum |
| Penahanan unit saat opname | `AST-RULE-023`; `AST-DES-008`, `031` | `INT-INV-04` | `BE-AST-021` | — | `AT-INV-03`, `30`; `UAT-INV-02`, `03` | — | belum |
| `CAP-07.2` pemeriksaan fisik | `AST-DEC-010`, `018`; `AST-FE-003`; `AST-DES-027`, `029` | flowchart 01 | `BE-AST-022` | `FE-AST-016` | `AT-INV-04..06`, `13`, `32`, `33`, `38`; `UAT-INV-06` | — | belum |
| `CAP-07.3` pencocokan | `AST-RULE-031`; `AST-DES-028` | state §2 | `BE-AST-022` | `FE-AST-017` | `AT-INV-04` | — | belum |
| `CAP-07.4` selisih | `AST-RULE-032`; `AST-RCG-G10`, `G11` | state §2 | `BE-AST-022`, `023` | `FE-AST-017` | `AT-INV-34` | — | belum |
| `CAP-07.5` verifikasi | `AST-RCG-G22`; `AST-ASM-003` | state §1 | `BE-AST-023` | `FE-AST-017` | `AT-INV-07`, `35`; `UAT-INV-04` | — | belum |
| `CAP-07.6` berita acara | `AST-DEC-046` | API 2.1 `/document` | `BE-AST-023` | `FE-AST-017` | `AT-INV-12` | — | belum |
| Tindak lanjut selisih | `AST-DEC-019`; `AST-DES-007`, `030` | flowchart 02 | `BE-AST-023` | `FE-AST-017` | `AT-INV-07..10`, `36`; `UAT-INV-05` | — | belum |
| `CAP-01.5` inventaris tertunda | `AST-DEC-031` | API 2.3 | `BE-AST-024` | `FE-AST-018` | `AT-INV-11` | — | belum |
| `CAP-11.4` laporan inventaris | — | API 2.4 | `BE-AST-024` | `FE-AST-018` | `AT-INV-11` | — | belum |

**Gap requirement → bukti:** tidak ada.
