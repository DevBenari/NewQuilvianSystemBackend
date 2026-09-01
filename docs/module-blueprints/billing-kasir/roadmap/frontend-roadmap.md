# Roadmap Delivery Frontend — Billing dan Kasir

## Metadata

```yaml
blueprint_id: BIL-CASH-001
blueprint_revision: 0.4
roadmap_revision: 1
roadmap_status: DRAFT_FORWARD_TEST
source_frontend: e555bf2ad6848a1d6cc097ab8c6c5f5259edb151
contracts: [BIL-API-0.4, BIL-STATE-0.4, BIL-VALIDATION-0.4, BIL-PERMISSION-0.4, BIL-TEST-0.4]
governance_dependency: BKC-BLK-FE-001
```

`BKC-BLK-FE-001`: root `AGENTS.md` frontend belum ditemukan. Seluruh task FE boleh direncanakan, tetapi builder frontend harus berhenti sebelum write sampai governance canonical tersedia atau owner menetapkan penggantinya. Tata letak, ikon, dan pembagian komponen adalah `DEV_DISCRETION`; formula, status, permission, dan lifecycle bukan.

## `FE-BKC-001` — Daftar dan detail running invoice read-only

| Field | Isi |
| --- | --- |
| Outcome | Billing/Kasir dapat mencari kunjungan dan memahami item, versi, patient/guarantor portion, serta outstanding |
| Trace | `BIL-CPT-001`–`005`; `BIL-AT-001`,`013`,`024` |
| Kontrak | Invoice GET API `0.4`; State `0.4` |
| Reuse | Next.js App Router, Axios service, hook conventions, Redux store hanya bila state lintas halaman diperlukan |
| Scope | Route daftar/detail, loading/empty/error/unauthorized/stale states, masked sensitive data |
| Dependency | `BE-BKC-005`,`006`; `BKC-BLK-FE-001` |
| Acceptance | Satu row/encounter; breakdown terbaca; nilai status unknown aman; failure satu panel tidak merusak seluruh halaman |
| Verifikasi | Component/API mock tests, lint/build, keyboard/table semantics |
| Risiko/pemilik | Data finansial stale. Owner Frontend + Billing |
| DoD | Test/lint/build evidence; no sensitive browser log; route/navigation tercatat |

## `FE-BKC-002` — Workspace master policy

| Field | Isi |
| --- | --- |
| Outcome | Finance/IT dapat mengelola admin fee, discount, tax, dan room policy dengan periode efektif jelas |
| Trace | `BKC-DEC-001`–`012`,`041`,`043`; `BIL-AT-009`–`012` |
| Kontrak | Empat Master Data API `0.4` |
| Reuse | Form/table master existing |
| Scope | List/create/edit-before-effective/deactivate, overlap/error display, role action visibility |
| Dependency | `BE-BKC-002`–`004`; `BKC-BLK-FE-001` |
| Acceptance | Nominal/rate/period tampil human-readable; immutable history tidak menawarkan delete; 403/422 jelas |
| Verifikasi | Component tests per master, lint/build |
| Risiko/pemilik | UI menyiratkan update retroaktif. Owner Finance/Product |
| DoD | Empat workspace atau tab setara, tests, accessibility, no hardcoded business values |

## `FE-BKC-003` — Workspace charge, recalculation, dan void

| Field | Isi |
| --- | --- |
| Outcome | Billing dapat melihat asal item, menghitung ulang invoice OPEN, dan membatalkan item eligible dengan alasan |
| Trace | `BKC-DEC-013`–`024`; `BIL-AT-002`–`004`,`020`,`021` |
| Kontrak | Invoice command API/Validation `0.4` |
| Reuse | Detail `FE-BKC-001`, modal/form conventions |
| Scope | Recalculate action, void dialog, version conflict reload, provenance source minimal |
| Dependency | `BE-BKC-008`; `FE-BKC-001`; governance blocker |
| Acceptance | Double-submit aman; 409 meminta reload; final invoice read-only; reason wajib; item sensitif dimask |
| Verifikasi | Component tests success/invalid/conflict/403, lint/build |
| Risiko/pemilik | UI tidak boleh memberi hak void di luar source authority. Owner Billing/Security |
| DoD | Aksi mengikuti Available permission/state; tests dan audit correlation display selesai |
| Status | `SOURCE_DONE_PENDING_MANUAL_VERIFICATION` — source ditulis, lulus `lint`, `build`, dan `test:unit` (harness; 33/34 pass, 1 gagal pre-existing tidak terkait) di branch `yasmina`, belum di-commit. Smoke-test browser headless tanpa login menunjukkan 0 exception JS pada halaman yang diubah. Klik-coba ter-autentikasi dengan invoice nyata dan unit/component test baru untuk kode ini belum dilakukan. Laporan: [`task/report/frontend/fe-bkc-003-hitung-ulang-dan-pembatalan-item-invoice.md`](../task/report/frontend/fe-bkc-003-hitung-ulang-dan-pembatalan-item-invoice.md) |

## `FE-BKC-004` — Diskon promo dan approval dokter

| Field | Isi |
| --- | --- |
| Outcome | Kasir memilih promo yang sah dan dokter menyetujui diskon share miliknya |
| Trace | `BKC-DEC-007`–`012`; `BIL-AT-012`,`022` |
| Kontrak | BillingDiscount/DoctorDiscount API `0.4` |
| Reuse | Invoice detail dan current-user permission |
| Scope | Discount picker, preview effect, pending approval, doctor approval inbox/action |
| Dependency | `BE-BKC-007`; `FE-BKC-001`; governance blocker |
| Acceptance | Admin fee tidak selectable; master promo tanpa approval; doctor approval actor benar; Finance exception terlihat |
| Verifikasi | Component/security tests, lint/build |
| Risiko/pemilik | Menampilkan share dokter ke pihak tidak berhak. Owner Doctor/Finance/Security |
| DoD | Role-specific UI dan negative paths terbukti |
| Status | `SOURCE_DONE_PENDING_MANUAL_VERIFICATION` — source ditulis, lulus `lint`, `build`, dan `test:unit` (harness; 33/34 pass, 1 gagal pre-existing tidak terkait) di branch `yasmina`, belum di-commit. Smoke-test browser headless tanpa login menunjukkan 0 exception JS. Klik-coba ter-autentikasi (ajukan diskon, approve dokter, kasus eskalasi Finance) dan unit/component test baru belum dilakukan. Laporan: [`task/report/frontend/fe-bkc-004-diskon-promo-dan-approval-dokter.md`](../task/report/frontend/fe-bkc-004-diskon-promo-dan-approval-dokter.md) |

## `FE-BKC-005` — Deposit rawat inap dan progress allocation

| Field | Isi |
| --- | --- |
| Outcome | Kasir melihat saldo deposit, menerima top-up, dan mengalokasikan cicilan tanpa menutup running invoice |
| Trace | `BKC-DEC-025`–`030`; `BIL-AT-007`,`008`,`020` |
| Kontrak | Patient Funds API/State `0.4` |
| Reuse | Money input/payment selection patterns existing |
| Scope | Ledger, top-up form, allocation preview, available/outstanding after action, refundable credit panel |
| Dependency | `BE-BKC-009`,`011`; governance blocker |
| Acceptance | Contoh Rp8 juta/Rp5 juta benar; invoice tetap OPEN; insufficient/conflict error jelas; double-submit aman |
| Verifikasi | Component/E2E mock tests, lint/build, decimal formatting |
| Risiko/pemilik | Pengguna menyamakan saldo deposit dengan pembayaran. Owner Cashier/Billing |
| DoD | Label dana belum dialokasikan dan ledger history jelas; tests lulus |
| Status | `SOURCE_DONE_PENDING_MANUAL_VERIFICATION` — source ditulis, lulus `lint`, `build`, dan `test:unit` (38/38 pass) di branch `yasmina`, belum di-commit. Smoke-test browser headless tanpa login menunjukkan 0 exception JS. Reversal top-up dan panel refundable credit permanen di luar scope (lihat laporan). Klik-coba ter-autentikasi belum dilakukan. Laporan: [`task/report/frontend/fe-bkc-005-deposit-rawat-inap-dan-progress-allocation.md`](../task/report/frontend/fe-bkc-005-deposit-rawat-inap-dan-progress-allocation.md) |

## `FE-BKC-006` — Split tender dan reconciliation status

| Field | Isi |
| --- | --- |
| Outcome | Kasir membagi pembayaran dan hanya mengganti bagian yang gagal/pending |
| Trace | `BKC-DEC-028`–`030`,`036`; `BIL-AT-005`,`006`,`017` |
| Kontrak | Settlement/Tender API/State `0.4` |
| Reuse | Existing payment-selection components dan Axios hooks |
| Scope | Tender rows, exact-total validation, submit/status polling, receipt summary, retry preserving idempotency key |
| Dependency | `BE-BKC-010`,`012`; governance blocker |
| Acceptance | Tunai Rp300 ribu tetap sukses saat QRIS Rp700 ribu gagal; outstanding Rp700 ribu; PENDING tidak auto-resubmit |
| Verifikasi | Component/API/E2E tests dan idempotency assertion |
| Risiko/pemilik | Browser refresh kehilangan key. Owner Frontend/Treasury |
| DoD | Payment draft recovery yang aman, tests/lint/build, no provider payload logs |
| Status | `SOURCE_DONE_PENDING_MANUAL_VERIFICATION` — source ditulis, lulus `lint`/`build`/`test:unit` (38/38) di branch `yasmina`, belum di-commit. Dikerjakan sebelum `FE-BKC-007` atas keputusan pemilik task — tender tunai belum bisa diverifikasi manual sampai shift kasir tersedia; tender non-tunai selalu Pending di environment ini karena provider payment belum terintegrasi (`BKC-BLK-PROV-001`, bukti bukan bug). Recovery settlement lewat localStorage per invoice untuk mitigasi risiko refresh browser. Laporan: [`task/report/frontend/fe-bkc-006-split-tender-dan-reconciliation-status.md`](../task/report/frontend/fe-bkc-006-split-tender-dan-reconciliation-status.md) |

## `FE-BKC-007` — Operasi shift kasir

| Field | Isi |
| --- | --- |
| Outcome | Kasir membuka, handover, dan menutup shift; Kepala Kasir meninjau selisih/reopen |
| Trace | `BKC-DEC-037`–`039`; `BIL-AT-016`,`017`,`022` |
| Kontrak | Cashier Shift API/State/Permission `0.4` |
| Reuse | Auth/permission and money table conventions |
| Scope | Current shift header, open/handover/close, system-vs-physical, variance review/reopen |
| Dependency | `BE-BKC-012`; governance blocker |
| Acceptance | Dua aktor handover; variance tetap terlihat; unauthorized action tersembunyi dan 403 aman; late noncash tidak mengubah physical |
| Verifikasi | Role/component tests, lint/build |
| Risiko/pemilik | Saldo kas termasuk data sensitif internal. Owner Kepala Kasir/Security |
| DoD | State/action matrix UI terbukti dan accessible |
| Status | `SOURCE_DONE_PENDING_MANUAL_VERIFICATION` — source ditulis (route baru `/health-services/billing-management/cashier/shifts`), lulus `lint`/`build`/`test:unit` (38/38) di branch `yasmina`, belum di-commit. Temuan penting: backend belum punya `GET` by-id untuk shift lain maupun master data Register — tiga aksi (confirm handover, review variance, reopen) memakai relay Shift ID/Row Version manual, bukan pencarian otomatis. Klik-coba ter-autentikasi (butuh ≥2 akun kasir + 1 Kepala Kasir) belum dilakukan. Laporan: [`task/report/frontend/fe-bkc-007-operasi-shift-kasir.md`](../task/report/frontend/fe-bkc-007-operasi-shift-kasir.md) |

## `FE-BKC-008` — Financial exception workbench

| Field | Isi |
| --- | --- |
| Outcome | Billing/Finance mengajukan, menyetujui, menolak, dan membalik refund/write-off/adjustment dengan histori utuh |
| Trace | `BKC-DEC-032`–`035`,`042`; `BIL-AT-014`,`015`,`021`,`022` |
| Kontrak | Financial Exceptions API/State/Permission `0.4` |
| Reuse | Approval list/detail patterns jika tersedia |
| Scope | Case list/detail, maker/approver actions, impact preview, execution status, reversal link |
| Dependency | `BE-BKC-013`,`014`; governance blocker |
| Acceptance | Write-off tidak dilabel PAID; maker cannot approve; partial outcomes visible; original history immutable |
| Verifikasi | Role/state/component tests, lint/build |
| Risiko/pemilik | High-risk financial action. Owner Finance/Security |
| DoD | Explicit confirmation, reason, audit timeline, negative tests selesai |
| Status | Source selesai 25 Agustus 2026, lulus lint/build/`test:unit`, menunggu verifikasi manual. Panel dibangun di halaman invoice detail (bukan workbench mandiri) karena backend tidak punya satu pun endpoint GET untuk case (`ISSUE-FE-008`, gap paling signifikan sejauh ini) — case dilacak lokal per invoice, approve/reverse case lain lewat relay ID manual. Refund tidak bisa diuji end-to-end tanpa akses database (tidak ada endpoint pencarian `RefundableCreditId`). Lihat `task/report/frontend/fe-bkc-008-pengecualian-finansial-refund-adjustment-write-off.md`. |

## `FE-BKC-009` — Preview dan finalisasi invoice

| Field | Isi |
| --- | --- |
| Outcome | Billing melihat checklist final, debtor AR, AP dokter, dan departure exception sebelum mengunci invoice |
| Trace | `BKC-DEC-031`,`036`,`041`–`044`; `BIL-AT-018`,`019`,`023` |
| Kontrak | Finalization API/Integration status `0.4` |
| Reuse | Invoice detail dan approval/confirmation components |
| Scope | Preview checklist, calculation version, debtor breakdown, confirm, retryable handoff status, read-only final state |
| Dependency | `BE-BKC-015`,`016`; `FE-BKC-001`; governance blocker |
| Acceptance | Missing order/debtor blocks; death/DAMA reason visible; AP not-ready distinct; retry tidak membuat second finalization |
| Verifikasi | Component/API/E2E tests, lint/build |
| Risiko/pemilik | Debtor evidence sensitive. Owner Billing/Finance/Security |
| DoD | Finalization states and failure recovery tested; masked data |
| Status | `SOURCE_DONE_PENDING_MANUAL_VERIFICATION` — source sudah ada dan ter-commit sejak `2dcea2f8f` ("update billing-kasir part 3 fe") tetapi belum pernah dilaporkan (dokumentasi tertinggal dari source, pola sama seperti rekonsiliasi `ISSUE-FE-003`). Diverifikasi ulang 30 Agustus 2026: lulus `lint:errors`, `test:unit` (44/44), `build`. Satu gap kecil ditemukan dan diperbaiki — `isFinal` di `billing-invoice-detail-view.jsx` sebelumnya tidak mencakup status `CLOSED` (transisi `FINAL → CLOSED` di `contracts/state-transition-matrix.md`), sehingga banner read-only final tidak akan muncul untuk invoice yang sudah `CLOSED`; saat ini belum berdampak runtime karena backend belum pernah meng-assign status `Closed` (`BKC-BLK-INT-001`). Tidak ada unit test khusus untuk slice/hook finalisasi. Klik-coba ter-autentikasi belum dilakukan. Laporan: [`task/report/frontend/fe-bkc-009-preview-dan-finalisasi-invoice.md`](../task/report/frontend/fe-bkc-009-preview-dan-finalisasi-invoice.md) |

## `FE-BKC-010` — Accessibility, privacy, dan regression lintas workspace

| Field | Isi |
| --- | --- |
| Outcome | Seluruh Billing/Kasir dapat digunakan keyboard, status tidak hanya warna, dan data sensitif tidak bocor |
| Trace | `BIL-AT-024` dan seluruh UI requirements |
| Kontrak | Permission/Validation/Test `0.4` |
| Reuse | Semua FE slice |
| Scope | Accessibility audit, log/analytics scan, responsive/error/unknown-state regression, critical E2E journeys |
| Dependency | `FE-BKC-001`–`009`; governance blocker diselesaikan |
| Acceptance | WCAG-oriented checks; mask field; no browser sensitive log; critical journeys pass |
| Verifikasi | Lint/build/component/E2E/a11y tooling sesuai repo |
| Risiko/pemilik | Test tooling belum diketahui sampai governance dibaca. Owner Frontend/QA/Security |
| DoD | Evidence `BIL-AT-024`, zero critical accessibility/privacy finding |
| Status | `PARTIALLY_DONE` — diaudit 30 Agustus 2026 lewat pembacaan source langsung (bukan tooling otomatis, lihat blocker). `BIL-AT-024` (tak ada field sensitif di log; status tidak hanya warna; fokus/label valid) **terpenuhi** untuk seluruh 9 slice, terbukti lewat base component bersama (`StatusBadge`, `DataTable`) dan sampel label/htmlFor lintas 4 slice. Sempat salah mencatat "gap regresi `FE-BKC-008`" pada draf pertama laporan — **dikoreksi**: `ISSUE-FE-008` (tidak ada endpoint `GET`) sudah closed sejak backend commit `f5e2106`, frontend sudah memakai endpoint `GET` nyata, bukan `localStorage`, tidak ada data yang hilang saat refresh. **Belum bisa dipenuhi**: scan a11y otomatis (tidak ada tooling axe/setara di repo, instalasi butuh otorisasi dependency terpisah) dan "critical E2E journeys" (tidak ada spec Billing sama sekali di `tests/e2e/`, butuh environment ter-autentikasi). Laporan: [`task/report/frontend/fe-bkc-010-accessibility-privacy-dan-regression-lintas-workspace.md`](../task/report/frontend/fe-bkc-010-accessibility-privacy-dan-regression-lintas-workspace.md) |

## Paralelisme

Setelah API tersedia, `FE-BKC-001` dan `002` dapat paralel. `003`/`004` memakai detail invoice; `005`/`007` boleh paralel; `006` menunggu payment dan shift cash. `008` terpisah dari `009`, kemudian `010` terakhir. Tidak ada FE task yang boleh mengubah rumus atau status backend.

