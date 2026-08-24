# Blueprint Manifest — Modul Operasi

| Field | Nilai |
|---|---|
| `blueprint_id` | `operations` |
| `revision` | `3` |
| `status` | `approved` |
| Product/domain owner | Pemilik kebutuhan |
| API owner | Belum ditetapkan |
| Security owner | Belum ditetapkan |
| Frontend authority | Pemilik kebutuhan untuk scope; detail UI `DEV_DISCRETION` |
| `approved_by` / `approved_at` | Pemilik kebutuhan / 2026-08-21 |
| Backend SHA | `767470f742bc6f2eebadbd653a873f69d6f93121` |
| Frontend SHA | `400104f2a0f3239c14c40f5905b419977a538450` |
| Requirement readiness | `READY_FOR_DOMAIN_DESIGN` |
| Domain architecture | revision 1 / `DOMAIN_ARCHITECTURE_READY` |
| API contract | `opr-api-v1` / `approved` |
| Integration contract | `opr-integration-v1` / `approved` |
| State contract | `opr-state-v1` / `approved` |
| Compatibility | Modul baru; tidak memutus endpoint existing |

## Input Revision dan Hash

| Input | Revision | SHA-256 |
|---|---:|---|
| `00-interview-decisions.md` | 6 | `CE952136A125B238CFC101904C42CC7BD85A73C290545D785D64A6AE25FB734F` |
| `01-existing-capability-map.md` | 2 | `A3AAC5454A38667BD2FF0B64375CFD5D3B32886D009AC7183EE9305802FF6158` |
| `02-requirement-completeness-assessment.md` | 3 | `261DCEA7996E02A596DD5766DB70D09D95D2B476C2A2A965321ADA09D6AE9950` |
| `03-domain-architecture.md` | 1 | `3E06A61A9500C25C9DFEB8F6F82672C1854B926115EF4619397DDD2D1DBBA5AD` |

## Artifact Hashes

| Artefak draft | SHA-256 |
|---|---|
| `02-backend-architecture.md` | `0B05472F1CE6C44ECA9E097585B5A66EE2631AC6A057BC913E585F3A1DBC27FD` |
| `03-frontend-architecture.md` | `55E0217B19E520E6120BAF10B3DE263B9FB38DE1A85CD4C7670C5A6F42BDE872` |
| `erd/00-context-erd.md` | `29E3971F3ADEA568D6D2530A62402724ADB22914A50772C10F50FECC744E6A63` |
| `erd/operating-room-management.md` | `345FEA7801E9EEF445B6F7F1DDA40C4248B34A0579D10A535BC44768E91AA9A4` |
| `erd/data-dictionary.md` | `70595306BDA325070B845DCACE3354AF5A0E2B2C1616895E27326C41D6477ADD` |
| `contracts/api-contract.md` | `AC7D998C66D9274FB291A52CBBFEC5B575D1B40B4553D2C8B8D8CE0E9BC22176` |
| `contracts/integration-contract.md` | `61E9FEC2592135E780AD9294C8CF473451FFCF54BED3085AA6BA2A8FAEAC3058` |
| `contracts/permission-audit-matrix.md` | `CC3AD77CD8D5B24EE07DB46EA1CE08B535D1AF4205C204888FEEDF74C6FCA4DC` |
| `contracts/state-transition-matrix.md` | `A2401F00E89A8F88A7F8F8ECF056668C61ADBCFE570DD3237E2B3049F369BC37` |
| `contracts/validation-matrix.md` | `1A85F88C23B63E9F4F67576C865C31BDF84F8F41C5FE4B9B73B99D276433E311` |
| `testing/acceptance-test-matrix.md` | `F43D6FEB1CAC62322E3C974533884BD3CD46E37A1BA968747CD14D5FA93B9A1F` |

Hash manifest tidak dicatat di dalam dirinya sendiri karena perubahan hash akan berulang. Perubahan material setelah approval wajib menaikkan revision dan memicu impact scan backend/frontend.

## Riwayat Revision

| Revision | Tanggal | Perubahan | Dampak |
|---:|---|---|---|
| 2 | 2026-08-21 | Approval awal blueprint | Menjadi dasar seluruh task `BE-OPR-001` sampai `BE-OPR-011` |
| 3 | 2026-08-24 | `OPS-DEC-026` ditambahkan: sign-off kesiapan disimpan sebagai `OprStatusHistory` beridentitas `Action = "ReadinessSignOff"` | Tidak mengubah skema maupun endpoint. Menutup celah antara `opr-api-v1` yang mensyaratkan `POST /sign-offs` dan ERD yang belum memuat tabel sign-off |

Dua penyimpangan lain terhadap kontrak masih terbuka dan belum berstatus keputusan: permission baca di luar `opr-permission-v1` pada enam endpoint `GET`, serta validasi item wajib checklist yang belum bersumber dari master template. Keduanya tercatat pada `testing/readiness-report.md` dan menunggu owner.

## Traceability

Blueprint mempertahankan `OPS-REQ-001`–`OPS-REQ-011`, `OPS-DEC-001`–`OPS-DEC-026`, dan `OPS-CON-001`–`OPS-CON-018`.
