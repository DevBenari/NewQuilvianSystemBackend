# Register Aset — Requirement Traceability

| Field | Value |
|---|---|
| Roadmap revision | `REV-1` — 2026-11-12 |
| Blueprint / contract | `AST-BP-001` rev 1 · `AST-REG-1.0.0` `approved` 2026-11-12 (`AST-DEC-051`) |
| Backend / Frontend SHA | `1f54a175` / `e49e8a81` |
| Masukan | decision revision `5`; hash masukan pada `../../blueprint-manifest.md` |

Kolom **Bukti** diisi build skill sesudah task selesai (tautan laporan). Untuk backend, bukti yang
dipetakan adalah `V-BE` (QBE, review diff, build, kontrak API, proses bisnis) — tidak adanya automated
test **bukan** gap (`rules/backend/TEST_POLICY.md`).

| Requirement | Decision / aturan | Desain | Task BE | Task FE | Verifikasi | Bukti | Status |
|---|---|---|---|---|---|---|---|
| Registry `Ast` | `AST-DEC-004` | `02-module-map.md` §1 | `BE-AST-001` | — | QBE | — | belum |
| `CAP-02.1`–`02.6`, `CAP-EXT-05` master | `AST-DEC-006`, `026`, `034`; `AST-DES-014`, `015` | arsitektur §4.1 | `BE-AST-002`, `004`, `006` | `FE-AST-001`, `002` | `AT-REG-34`, `35` | — | belum |
| `CAP-03.1` pencatatan | `AST-DEC-006..008`, `043` | state §1 | `BE-AST-003`, `007` | `FE-AST-003` | `AT-REG-01`, `30`, `31`, `33`; `UAT-REG-01`, `02` | — | belum |
| `CAP-03.2` data perolehan | `AST-DEC-008`, `AST-DES-002` | data dict §3.1 | `BE-AST-007` | `FE-AST-004` | `AT-REG-02` | — | belum |
| `CAP-03.3` nomor inventaris | `AST-DEC-009`, `033`; `AST-DES-014` | integration `INT-REG-10` | `BE-AST-005`, `007` | `FE-AST-003` | `AT-REG-01`, `11`, `32` | — | belum |
| `CAP-03.4` label | `AST-DEC-010` | `INT-REG-11` | `BE-AST-008` | `FE-AST-005` | `AT-REG-13` | — | belum |
| `CAP-03.5`, `08.2`, `08.3` dokumen dan pengamanan | `AST-RCG-G14`, `AST-ASM-002` | API 2.12 | `BE-AST-011` | `FE-AST-008` | `AT-REG-14`; `UAT-REG-07` | — | belum |
| `CAP-03.6`, `11.1` daftar inventaris | `AST-DEC-025`; `AST-DES-016` | API 2.8, 2.17 | `BE-AST-007`, `013` | `FE-AST-004`, `010` | `AT-REG-12`, `46` | — | belum |
| `CAP-04.1`–`04.4` penempatan | `AST-DEC-044`; `AST-DES-006` | state §4 | `BE-AST-009` | `FE-AST-006` | `AT-REG-04`, `05`, `36..38`; `UAT-REG-03`, `04` | — | belum |
| `CAP-06.1`–`06.6`, `11.3` kondisi | `AST-DEC-017` | state §2–3 | `BE-AST-010`, `013` | `FE-AST-007`, `010` | `AT-REG-06`, `07`, `39`, `40`; `UAT-REG-05`, `06` | — | belum |
| `CAP-08.1`, `08.5` pemanfaatan | `AST-DEC-021`, `029`; `AST-DES-018` | state §5 | `BE-AST-011` | `FE-AST-008` | `AT-REG-08`, `09`, `41`, `42`; `UAT-REG-08` | — | belum |
| `CAP-08.4` aset menganggur | `AST-RCG-G13`; `AST-DES-017` | API 2.8 `/idle` | `BE-AST-011` | `FE-AST-008` | `AT-REG-15` | — | belum |
| `CAP-EXT-01` impor data awal | `AST-DEC-023`, `037`, `050` | state §6 | `BE-AST-012` | `FE-AST-009` | `AT-REG-10`, `43..45`; `UAT-REG-09`, `10` | — | belum |
| `CAP-01.1`–`01.3`, `01.6` dashboard | `AST-DEC-025` | API 2.16 | `BE-AST-013` | `FE-AST-010` | `AT-REG-16`; `UAT-REG-11` | — | belum |
| `CAP-11.7` History & Audit | `AST-DES-004` | data dict §3.5 | `BE-AST-005`, `013` | `FE-AST-010` | `AT-REG-12` | — | belum |
| Hak lihat rupiah | `AST-DEC-025`, `AST-FE-004` | permission §2 | `BE-AST-007` | `FE-AST-004` | `AT-REG-47` | — | belum |
| Lampiran fail-closed | `AST-DES-020` | integration §2 | `BE-AST-005` | `FE-AST-007` | `AT-REG-48`; `UAT-REG-13` | — | belum |
| Keterjangkauan layar | `frontend-menu-and-screen-template.md` | `02-module-map.md` §3.1 | — | `FE-AST-001..010` | `AT-REG-49` | — | belum |
| `CAP-EXT-04` hibah (dipicu register) | `AST-DEC-040` | `INT-REG-31` | `BE-AST-007` (+ `BE-AST-015` [NLI]) | — | `AT-REG-03` | — | belum |

**Gap requirement → bukti:** tidak ada. Setiap kemampuan sub-modul ini punya task dan skenario verifikasi.
