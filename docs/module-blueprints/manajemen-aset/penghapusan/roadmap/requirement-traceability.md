# Penghapusan & Pelepasan Aset — Requirement Traceability

| Field | Value |
|---|---|
| Roadmap revision | `REV-1` — 2026-11-12 |
| Blueprint / contract | `AST-BP-001` rev 1 · `AST-HPS-1.0.0` `approved` 2026-11-12 (`AST-DEC-051`) |
| Backend / Frontend SHA | `1f54a175` / `e49e8a81` |

Bukti backend = `V-BE`; tidak adanya automated test bukan gap (`rules/backend/TEST_POLICY.md`).

| Requirement | Decision / aturan | Desain | Task BE | Task FE | Verifikasi | Bukti | Status |
|---|---|---|---|---|---|---|---|
| `CAP-10.1` usulan | `AST-DEC-020`, `045`; `AST-DES-038` | state §1 | `BE-AST-033`, `034` | `FE-AST-024` | `AT-HPS-30`, `31`; `UAT-HPS-04` | — | belum |
| `CAP-10.2` pemeriksaan kelayakan | `AST-RCG-G22`; `AST-DES-019` | state §1 | `BE-AST-034` | `FE-AST-024` | `AT-HPS-03`, `32` | — | belum |
| `CAP-10.3` persetujuan berjenjang | `AST-DEC-020`, `049`; `AST-DES-039`, `040` | state §1 | `BE-AST-034` | `FE-AST-024` | `AT-HPS-01`, `02`, `33`, `34`; `UAT-HPS-02` | — | belum |
| `CAP-10.4` pemindahtanganan | `AST-DEC-022`; `AST-RULE-051`, `052` | state §1 | `BE-AST-035` | `FE-AST-025` | `AT-HPS-04`, `35`, `36`; `UAT-HPS-01` | — | belum |
| Penghapusan aset hilang | `AST-DEC-045` | state §1 | `BE-AST-035` | `FE-AST-025` | `AT-HPS-05`; `UAT-HPS-03` | — | belum |
| `CAP-10.5` berita acara dan finalisasi | `AST-DEC-046`; `AST-DES-013`, `041` | integration `INT-HPS-07`, `08` | `BE-AST-035` | `FE-AST-025` | `AT-HPS-06`, `07`, `10`, `37..39`, `42`; `UAT-HPS-05` | — | belum |
| `CAP-EXT-03` Pemulihan Aset | `AST-DEC-028`; `AST-DES-037` | state §2 | `BE-AST-036` | `FE-AST-026` | `AT-HPS-08`, `40`; `UAT-HPS-06`, `07` | — | belum |
| `CAP-10.6` riwayat aset dihapus | `AST-DEC-025` | API 2.3 | `BE-AST-037` | `FE-AST-026` | `AT-HPS-09`, `43` | — | belum |
| `CAP-11.5` laporan penghapusan | `AST-DEC-025` | API 2.4 | `BE-AST-037` | `FE-AST-026` | `AT-HPS-09` | — | belum |
| Kejadian penghapusan/pemulihan ke Akuntansi | `AST-DEC-015`, `047` | `../../nilai/contracts/integration-contract.md` §3 | `BE-AST-035`, `036` (penulisan); `BE-AST-032` [NLI] (penyalaan) | — | `AT-HPS-06`, `08` | — | belum; penyalaan ⛔ `GATE-AST-ACC` |

**Gap requirement → bukti:** tidak ada.
