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
| Satu invoice dan charge idempotent (`DEC-013`–`018`,`040`) | CTX-01, API/Integration | `BE-BKC-005`,`008` | `FE-BKC-001`,`003` | `AT-001`–`004`,`020` | Covered/planned — `FE-BKC-003` (recalculate + void) source selesai 25 Agustus 2026, menunggu verifikasi manual; lihat `task/report/frontend/fe-bkc-003-hitung-ulang-dan-pembatalan-item-invoice.md` |
| Admin fee (`DEC-001`–`006`) | CPT-007, Validation | `BE-BKC-002`,`006` | `FE-BKC-002`,`012` | `AT-009`–`011` | Covered/planned — form create/update Administration Fee Policy dimigrasi ke `BaseEditorView` 28 Agustus 2026 (konsistensi UI, bukan perubahan bisnis); lihat `task/report/frontend/fe-bkc-012-konsistensi-base-component-form-master-data.md` |
| Diskon (`DEC-007`–`012`) | CPT-008/009, Permission | `BE-BKC-003`,`007` | `FE-BKC-002`,`004`,`012` | `AT-012`,`022` | Covered/planned — `FE-BKC-004` (ajukan diskon promo/dokter + approve dokter) source selesai 25 Agustus 2026, menunggu verifikasi manual; lihat `task/report/frontend/fe-bkc-004-diskon-promo-dan-approval-dokter.md`. Form create/update Discount Policy dimigrasi ke `BaseEditorView` 28 Agustus 2026; lihat `task/report/frontend/fe-bkc-012-konsistensi-base-component-form-master-data.md` |
| Tax/room/coverage (`DEC-019`–`023`,`041`,`043`) | CPT-021–024, Integration | `BE-BKC-004`,`006` | `FE-BKC-001`,`002`,`012` | `AT-010`,`011`,`013`,`021` | Covered/planned — form create/update Tax Rule dan Room Charge Policy dimigrasi ke `BaseEditorView` 28 Agustus 2026; lihat `task/report/frontend/fe-bkc-012-konsistensi-base-component-form-master-data.md` |
| Deposit/progress (`DEC-025`–`030`) | CTX-02, Patient Funds | `BE-BKC-009`,`011` | `FE-BKC-005` | `AT-007`,`008`,`020` | Covered/planned — `FE-BKC-005` (top-up + allocation, panel deposit di halaman invoice RANAP) source selesai 25 Agustus 2026, menunggu verifikasi manual; lihat `task/report/frontend/fe-bkc-005-deposit-rawat-inap-dan-progress-allocation.md` |
| Split tender (`DEC-028`–`030`,`036`) | Settlement State/API | `BE-BKC-010`,`012` | `FE-BKC-006`,`007` | `AT-005`,`006`,`017` | Covered/planned — `FE-BKC-006` (create settlement + tender rows, panel di halaman invoice) source selesai 25 Agustus 2026, menunggu `FE-BKC-007` untuk verifikasi tender tunai dan verifikasi manual; lihat `task/report/frontend/fe-bkc-006-split-tender-dan-reconciliation-status.md` |
| Refund (`DEC-032`,`033`) | CTX-03 | `BE-BKC-013` | `FE-BKC-008` | `AT-008`,`014` | Covered/planned — `FE-BKC-008` source selesai 25 Agustus 2026, menunggu verifikasi manual; refund tidak bisa diuji end-to-end tanpa akses database untuk menemukan `RefundableCreditId` (tidak ada endpoint pencarian); lihat `task/report/frontend/fe-bkc-008-pengecualian-finansial-refund-adjustment-write-off.md` |
| Write-off/adjustment (`DEC-034`,`035`,`042`) | CTX-03, Integration | `BE-BKC-014`,`016` | `FE-BKC-008` | `AT-014`,`015`,`021` | Covered/planned — `FE-BKC-008` (ajukan/setujui/reversal, panel di halaman invoice) source selesai 25 Agustus 2026, menunggu verifikasi manual; temuan gap tanpa satu pun endpoint GET untuk case (`ISSUE-FE-008`) dicatat di laporan; lihat `task/report/frontend/fe-bkc-008-pengecualian-finansial-refund-adjustment-write-off.md` |
| Shift kasir (`DEC-037`–`039`) | CTX-04 | `BE-BKC-012` | `FE-BKC-007`,`012` | `AT-016`,`017`,`022` | Covered/planned — `FE-BKC-007` (halaman baru, open/handover/close/review/reopen) source selesai 25 Agustus 2026, menunggu verifikasi manual; temuan gap `GET` by-id shift dan master data Register dicatat di laporan; lihat `task/report/frontend/fe-bkc-007-operasi-shift-kasir.md`. Form create/update Register dimigrasi ke `BaseEditorView` 28 Agustus 2026; lihat `task/report/frontend/fe-bkc-012-konsistensi-base-component-form-master-data.md` |
| Finalisasi/departure (`DEC-031`,`036`,`044`) | CTX-05, State | `BE-BKC-015` | `FE-BKC-009` | `AT-018`,`019`,`023` | Covered/planned |
| AR/AP final (`DEC-041`–`044`) | Integration/Handoff | `BE-BKC-016` | `FE-BKC-009` | `AT-019`,`021`,`023` | Covered, external dependency |
| Security/privacy/concurrency | Permission/Validation | `BE-BKC-001`,`017` | `FE-BKC-010` | `AT-020`,`022`,`024` | Covered/planned — evidence matrix `BE-BKC-017` diperbarui 28 Agustus 2026 (22/24 acceptance ID `Covered`; `AT-020` butuh Postgres nyata, `AT-023` blocked `BKC-BLK-INT-001`); lihat `evidence/06-be-bkc-017-acceptance-evidence-matrix.md` dan `task/report/backend/be-bkc-017-hardening-dan-acceptance-lintas-slice.md` |
| Menu Pembayaran, Dokumen Kasir: Kwitansi per tender, Struk Pasien (`DEC-045`–`058`, amendment 27-28 Agustus 2026, di luar roadmap revisi 1 asli) | Belum ada dokumen kontrak `0.4` terpisah — desain lahir langsung dari `/grill-me` amendment, bukan `design-business-module` | `BE-BKC-017` slice (kwitansi per tender di `AddTenderAsync`, endpoint `POST .../kwitansi` lama dihapus) | `FE-BKC-011` | Tidak terpetakan ke `BIL-AT-001`–`024` (acceptance test matrix ditulis sebelum amendment ini) | Source selesai, lint/build lulus, menunggu verifikasi manual; lihat `task/report/frontend/fe-bkc-011-dokumen-kasir-kwitansi-per-tender-dan-struk-pasien.md`. `BKC-DEC-052`–`058` masih `draft`, belum ada approval formal |

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
