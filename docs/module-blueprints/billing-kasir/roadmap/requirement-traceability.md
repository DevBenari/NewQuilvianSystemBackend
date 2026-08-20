# Requirement Traceability — Billing dan Kasir

## Metadata

```yaml
blueprint_id: BIL-CASH-001
blueprint_revision: 0.4
roadmap_revision: 1
status: DRAFT_FORWARD_TEST
decision_revision: 0.2
backend_source: c99f0a51577456c91831870892870f9ae633b4c2
frontend_source: e555bf2ad6848a1d6cc097ab8c6c5f5259edb151
```

## Pemetaan requirement ke delivery

| Requirement/decision | Design/contract | Backend | Frontend | Bukti | Status |
| --- | --- | --- | --- | --- | --- |
| Satu invoice dan charge idempotent (`DEC-013`–`018`,`040`) | CTX-01, API/Integration | `BE-BKC-005`,`008` | `FE-BKC-001`,`003` | `AT-001`–`004`,`020` | Covered/planned |
| Admin fee (`DEC-001`–`006`) | CPT-007, Validation | `BE-BKC-002`,`006` | `FE-BKC-002` | `AT-009`–`011` | Covered/planned |
| Diskon (`DEC-007`–`012`) | CPT-008/009, Permission | `BE-BKC-003`,`007` | `FE-BKC-002`,`004` | `AT-012`,`022` | Covered/planned |
| Tax/room/coverage (`DEC-019`–`023`,`041`,`043`) | CPT-021–024, Integration | `BE-BKC-004`,`006` | `FE-BKC-001`,`002` | `AT-010`,`011`,`013`,`021` | Covered/planned |
| Deposit/progress (`DEC-025`–`030`) | CTX-02, Patient Funds | `BE-BKC-009`,`011` | `FE-BKC-005` | `AT-007`,`008`,`020` | Covered/planned |
| Split tender (`DEC-028`–`030`,`036`) | Settlement State/API | `BE-BKC-010`,`012` | `FE-BKC-006`,`007` | `AT-005`,`006`,`017` | Covered/planned |
| Refund (`DEC-032`,`033`) | CTX-03 | `BE-BKC-013` | `FE-BKC-008` | `AT-008`,`014` | Covered/planned |
| Write-off/adjustment (`DEC-034`,`035`,`042`) | CTX-03, Integration | `BE-BKC-014`,`016` | `FE-BKC-008` | `AT-014`,`015`,`021` | Covered/planned |
| Shift kasir (`DEC-037`–`039`) | CTX-04 | `BE-BKC-012` | `FE-BKC-007` | `AT-016`,`017`,`022` | Covered/planned |
| Finalisasi/departure (`DEC-031`,`036`,`044`) | CTX-05, State | `BE-BKC-015` | `FE-BKC-009` | `AT-018`,`019`,`023` | Covered/planned |
| AR/AP final (`DEC-041`–`044`) | Integration/Handoff | `BE-BKC-016` | `FE-BKC-009` | `AT-019`,`021`,`023` | Covered, external dependency |
| Security/privacy/concurrency | Permission/Validation | `BE-BKC-001`,`017` | `FE-BKC-010` | `AT-020`,`022`,`024` | Covered/planned |

## Coverage acceptance test

| Test | Task utama | Jalur gagal tercakup |
| --- | --- | --- |
| `BIL-AT-001`–`004` | `BE-BKC-005`,`008`; `FE-BKC-001`,`003` | duplicate, incomplete, void invalid |
| `BIL-AT-005`–`008` | `BE-BKC-009`–`011`,`013`; `FE-BKC-005`,`006` | tender fail/pending, insufficient, excess credit |
| `BIL-AT-009`–`013` | `BE-BKC-002`–`007`; `FE-BKC-002`,`004` | duplicate fee, forbidden discount, coverage cap |
| `BIL-AT-014`–`017` | `BE-BKC-012`–`014`; `FE-BKC-007`,`008` | self-approval, reversal, variance, late settlement |
| `BIL-AT-018`–`021` | `BE-BKC-006`,`008`,`011`,`015`,`016`; `FE-BKC-003`,`005`,`009` | departure unpaid, conflict, post-final correction |
| `BIL-AT-022`–`024` | `BE-BKC-017`; `FE-BKC-010` | unauthorized, consumer down, privacy/a11y |

## Coverage gap dan blocker

| ID | Gap | Dampak | Owner | Status/aksi |
| --- | --- | --- | --- | --- |
| `BKC-BLK-FE-001` | Root governance frontend tidak ditemukan | Semua FE write tertahan | Frontend authority | Tetapkan/restore sebelum builder |
| `BKC-BLK-INT-001` | Schema/transport aktual AR dan AP belum dibuktikan di consumer | `BE-BKC-016` tidak dapat final | AR/AP + Integration | Contract discovery sebelum task approval |
| `BKC-BLK-PROV-001` | Provider payment/refund sandbox dan callback contract belum dipilih | E2E `AT-006`,`013` refund provider | Treasury/Integration | Adapter bisa dibangun terhadap interface; aktivasi menunggu provider |
| `BKC-BLK-DATA-001` | Nilai seed Finance/Inpatient belum dicantumkan | Master dapat dibuat tetapi tidak boleh diaktifkan | Finance/Inpatient | Serahkan nominal/rate/rules sebelum seed aktif |

Tidak ada requirement bisnis approved yang kehilangan task. Gap di atas adalah dependency implementasi/operasional, bukan izin untuk mengarang policy. Roadmap dianggap siap dieksekusi hanya per task yang disetujui, dimulai dari `BE-BKC-001` atau task master independen setelah fondasi tersedia.
