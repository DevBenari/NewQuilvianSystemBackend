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

## Amendment 2 September 2026 — Form "Buat Invoice Manual (Testing)" berbasis katalog tarif + coverage

```yaml
input_blueprint_revision: 0.5
input_blueprint_status: approved
approved_by: Product/Domain Owner (2 September 2026 13:53 WIB)
source_frontend_at_design: 60febdcdbb39de6cebc2d825906bce949f3b5af3
contracts: [BIL-API-0.4 (amendment 2 Sep 2026), BIL-PERMISSION-0.4 (amendment)]
```

**Catatan status `BKC-BLK-FE-001`**: blocker ini tercatat sejak roadmap `0.4` sebagai "root `AGENTS.md` frontend belum ditemukan". Pada sesi desain amendment ini (2 September 2026), `QuilvianSystemFrontendDev/AGENTS.md` **sudah ditemukan dan terbaca** — governance frontend tampak sudah tersedia. Ini **kemungkinan** membuat blocker tersebut resolved, tetapi belum diverifikasi ulang secara formal terhadap seluruh task lama (`FE-BKC-001`–`013`) pada roadmap ini. Builder task baru di bawah tetap **wajib** memverifikasi keberadaan/isi `AGENTS.md` frontend saat mulai eksekusi, bukan mengasumsikan otomatis clear dari catatan ini.

Tiga task baru (`FE-BKC-014`–`016`) mengoperasikan `BKC-DEC-059`–`062`. Detail desain lengkap: [`03-frontend-architecture.md`](../03-frontend-architecture.md#amendment-2-september-2026--form-buat-invoice-manual-testing-berbasis-katalog-tarif--coverage), [`04-prd-to-mvp.md`](../04-prd-to-mvp.md).

## `FE-BKC-014` — Dropdown tarif dan harga read-only pada form testing (pasien tunai)

| Field | Isi |
| --- | --- |
| Outcome | Kasir/penguji memilih item dari katalog tarif resmi; harga terisi otomatis, tidak dapat diketik manual |
| Trace | `BKC-DEC-059`,`061`; `FR-BKC-001`,`002`,`004`; `UAT-01`,`02` (`04-prd-to-mvp.md`) |
| Kontrak | `POST catalog-charges` (API amendment); `GET Tariff/options` (existing, reuse) |
| Reuse | `getTariffOptions`/`selectTariffOptions` (`master-data-tariff-slice.jsx`, `CAP-02` Ready to reuse); pola serverSide-searchable `BaseSelectField` yang sudah dipakai field `encounterId` pada form yang sama |
| Scope | Ganti field "Nama Item/Layanan" (text bebas) jadi dropdown searchable terfilter kategori+`ServiceUnitId`/`ClinicId`/`PatientClassId` encounter; ganti field "Harga (Rp)" jadi teks read-only terisi `NormalPrice`; thunk `addCatalogCharge` baru (`billing-invoice-slice.jsx`); ganti submit `use-create-manual-invoice.js` ke thunk baru |
| Dependency | `BE-BKC-018`,`019`; `BKC-BLK-FE-001` — lihat catatan status di atas, verifikasi ulang wajib sebelum eksekusi |
| Acceptance | `UAT-01`,`02`; disambiguasi multi-baris tarif nama sama tampil berlabel scope (`BKC-DEC-061`) |
| Verifikasi | Component test dropdown filter+search; test submit tanpa field harga; lint/build |
| Risiko/pemilik | Kasir salah pilih baris tarif berscoping mirip. Owner Frontend/Billing |
| DoD | Field harga tidak punya `onChange`; tests/lint/build lulus; tidak ada field harga di request payload (structural, `BIL-VAL-026`) |

**Status 3 September 2026**: Source selesai (dropdown tarif, harga read-only, thunk
`addCatalogCharge`). `BKC-BLK-FE-001` diverifikasi ulang: resolved. Verifikasi manual langsung
(login sungguhan, data dev nyata) menemukan bug backend pra-eksisting yang memblokir dropdown tarif
selalu kosong (filter scope strict-equality pada `TariffController`) — sudah diperbaiki lewat task
terpisah `BE-BKC-FIX-001` (source saja, backend belum di-build ulang/restart). Verifikasi
ujung-ke-ujung penuh (pilih tarif → submit → invoice tersimpan) tertunda sampai itu terjadi. Belum
ditandai selesai; lihat `task/report/frontend/FE-BKC-014.md` § 6 dan § 8, serta
`task/report/backend/BE-BKC-FIX-001.md`.

**Status 3 September 2026 (lanjutan)**: laporan bug pengguna terpisah (screenshot) — dropdown
Tarif Layanan tetap kosong sampai diketik pencarian walau kategori sudah dipilih. Diperbaiki lewat
task ad-hoc `FE-BKC-FIX-002`: field `tariffId` di-override `requireSearch: false` (resource
`tariffs` bersama di registry TIDAK diubah). Terverifikasi hidup — dropdown kini menampilkan hasil
langsung begitu dibuka (tanpa mengetik), search dan scroll/paginasi tetap berfungsi. Ditemukan pula
temuan sampingan pra-eksisting: `FilterSelect` selalu merender satu baris opsi pseudo di posisi
pertama berisi teks placeholder — lihat `task/report/frontend/FE-BKC-FIX-002.md` § 8.

**Status 3 September 2026 (lanjutan 2)**: temuan sampingan di atas diperbaiki lewat task ad-hoc
`FE-BKC-FIX-003`, MELALUI `base-component-decision-gate` (base component bersama `BaseSelectField`,
dipakai ~481 field `type: "select"` di 111 berkas) — pengguna memilih opsi rekomendasi:
baris "clear ke kosong" hanya tampil untuk field opsional yang sudah punya nilai terpilih, tidak
pernah untuk field wajib. Terverifikasi hidup pada field opsional (`categoryId`) dan field wajib
(`tariffId`); lihat `task/report/frontend/FE-BKC-FIX-003.md`.

**Status 3 September 2026 (lanjutan 3)**: permintaan UX tambahan pengguna diselesaikan lewat task
ad-hoc `FE-BKC-FIX-004` — field Tarif Layanan/Qty kini `disabled` (REUSE `field.disabled`, tanpa
gate) selama Kategori Tarif belum dipilih. Dua permintaan lain (dropdown tarif refresh saat
kategori diganti; form dikosongkan setelah submit sukses) ternyata SUDAH terpenuhi oleh source
`FE-BKC-014` — diverifikasi hidup lewat alur penuh (kunjungan → kategori → tarif → qty → submit
sukses → reset), bukan diimplementasikan ulang. Lihat `task/report/frontend/FE-BKC-FIX-004.md`.

**Status 3 September 2026 (lanjutan 4)**: laporan bug pengguna terpisah (screenshot) — kata kunci
pencarian dan hasil dropdown Tarif Layanan lama masih terbawa setelah Kategori Tarif diganti.
Diperbaiki lewat task ad-hoc `FE-BKC-FIX-005`, MELALUI `base-component-decision-gate` (properti
opsional baru `field.remountKey` pada `BaseEditorForm`, base component bersama dipakai di seluruh
form editor aplikasi, opt-in murni) — pengguna memilih opsi rekomendasi: remount total field
`tariffId` setiap Kategori Tarif berganti. Terverifikasi hidup dengan dua kategori berbeda yang
sama-sama punya data (bukan hanya kategori kosong): kotak pencarian dan hasil lama terbukti benar-
benar bersih. Lihat `task/report/frontend/FE-BKC-FIX-005.md`.

**Status 3 September 2026 (lanjutan 5)**: laporan bug pengguna langsung (dua screenshot) — badge
status coverage per baris item di Menu Pembayaran (`FE-BKC-016`) selalu "Penjamin" untuk invoice
pasien asuransi, bahkan saat item tidak coverable dan Subtotal Asuransi invoice-nya Rp 0 (root
cause: badge dibaca dari cara bayar KUNJUNGAN, bukan hasil kalkulasi coverage sesungguhnya).
Diperbaiki lewat task ad-hoc `FE-BKC-FIX-006` — badge kini memakai field
`breakdown.items[].coverable` (SUDAH ADA di kontrak API `calculation-preview`, baru dikonsumsi
sekarang) dikombinasikan dengan status coverage aktual invoice, lewat keputusan eksplisit
`AskUserQuestion`. Terverifikasi hidup untuk kasus yang dilaporkan; jalur "Penjamin" sungguhan
TIDAK bisa diverifikasi hidup (tidak ada invoice dengan coverage aktual di database dev saat ini)
— lihat `task/report/frontend/FE-BKC-FIX-006.md` § 6 dan § 8.

**Status 3 September 2026 (lanjutan 6)**: pengguna melaporkan invoice Allianz mereka seharusnya
punya item tercover, tapi semua tetap "Tunai" — investigasi menemukan dua root cause BACKEND
(bukan frontend) yang membuat coverage asuransi tidak pernah benar-benar diterapkan ke item
manapun di sistem ini: (1) data `MstTariffCategory.IsCoveredByInsuranceDefault` ter-backfill
`false` untuk semua kategori (migration 2 September yang keliru), (2) pencocokan rule asuransi
tidak pernah bisa match (rujukan item diambil dari idempotency key, bukan domain reference).
Diperbaiki lewat task ad-hoc `BE-BKC-FIX-002` (source + migration data disiapkan, EKSEKUSI
migration ke database tetap wewenang pengguna). Lihat `task/report/backend/BE-BKC-FIX-002.md`
dan `task/report/frontend/FE-BKC-FIX-006.md` (update lanjutan).

**Status 4 September 2026**: pengujian langsung pengguna atas invoice Allianz nyata (setelah
`BE-BKC-FIX-002` di-rebuild) menemukan DUA masalah baru: (1) badge status per item kembali salah
(semua "Penjamin") karena `coverable` (kategori) jadi `true` di mana-mana setelah `BE-BKC-FIX-002`,
menghapus daya beda yang tadinya disediakan `FE-BKC-FIX-006`; (2) Subtotal Mandiri/Asuransi
(FE-BKC-016) diam-diam menggelembungkan Mandiri sebesar jumlah yang masih "Penjamin Belum
Terverifikasi". Root cause sama: waterfall coverage cuma mengembalikan total gabungan, bukan per
komponen. Diperbaiki lewat task ad-hoc `BE-BKC-FIX-003` (backend, melacak hasil per komponen) +
`FE-BKC-FIX-008` (frontend, badge 3-status dan split eksak) - disetujui pengguna lewat
`AskUserQuestion`. **Belum diverifikasi hidup** - menunggu backend di-rebuild ulang dan frontend
dev server restart. Lihat `task/report/backend/BE-BKC-FIX-003.md` dan
`task/report/frontend/FE-BKC-FIX-008.md`.

**Status 4 September 2026 (lanjutan)**: pengujian lebih lanjut pada invoice yang sama memunculkan
dua keputusan bisnis baru (backend murni, tidak ada perubahan frontend) — disetujui lewat
`AskUserQuestion` — dan diselesaikan lewat `BE-BKC-FIX-004`: (1) item tanpa rule asuransi SAMA
SEKALI otomatis Mandiri (bukan lagi "Menunggu Verifikasi"); (2) PPN Obat/Alkes memperhitungkan
rawat jalan vs rawat inap, bukan cuma kategori. Belum diverifikasi hidup. Lihat
`task/report/backend/BE-BKC-FIX-004.md`.

**Status 4 September 2026 (lanjutan 2)**: pengguna menambahkan dua rule Allianz baru menyasar
kategori Drug/Pharmacy (`ItemType="ServiceCategory"` + `TariffCategoryId`) — item Drug tetap
"Tunai" di Menu Pembayaran walau dropdown preview tarif sudah benar menunjukkan "Tercover" untuk
rule yang sama. Root cause BACKEND murni: `RegistrationBillingCoverageAdapter.Matches()` memakai
gerbang tunggal `rule.ItemType == component.CoverageItemType`, yang memaksa item kategori
Pharmacy/Drug/Consumable-Alkes SELALU bertag `"Drug"` sebelum dimensi `TariffCategoryId` sempat
diperiksa — beda dengan engine preview (`InsuranceCoverageService.FindCoverageRuleAsync`) yang
memakai OR-chain per-dimensi tanpa gerbang tunggal. Diperbaiki lewat task ad-hoc `BE-BKC-FIX-005`
— `Matches()` diselaraskan ke pola OR-chain `InsuranceCoverageService` persis. Belum diverifikasi
hidup — menunggu backend di-rebuild. Lihat `task/report/backend/BE-BKC-FIX-005.md`.

**Status 4 September 2026 (lanjutan 3)**: setelah `BE-BKC-FIX-005` di-rebuild dan diverifikasi hidup
(item Drug sekarang benar "Penjamin"), pengguna mengubah rule Radiology jadi CoveragePercent=75/
CoPaymentPercent=25 dan bertanya kenapa Subtotal Mandiri masih ada padahal badge "Penjamin" —
diverifikasi lewat query backend langsung: perhitungan SUDAH benar (co-payment rule memang membuat
sebagian item tetap jadi tanggungan pasien meski berbadge "Penjamin"). Pengguna lalu meminta fitur
baru: info nominal co-payment ditampilkan di tabel item, untuk semua item coverage asuransi yang
tidak 100%. Diselesaikan lewat `FE-BKC-FIX-009` — baris "Co-payment pasien: Rp{nominal}" muncul di
bawah badge "Penjamin" untuk item yang residual pasiennya >0, murni REUSE `styles.sectionHint`.
Source selesai, lint bersih, **belum diverifikasi hidup** (dev server Turbopack belum reload -
staleness, bukan bug). Temuan sampingan (dilaporkan, belum diperbaiki): `CalculateCoveredAmount()`
menumpuk `CoveragePercent` dan `CoPaymentPercent` sebagai DUA pengurang independen (75% dikurangi
25% lagi dari eligible = hasil akhir cuma 50%, bukan 75%) - menunggu keputusan pengguna apakah ini
memang dimaksudkan atau perlu diperbaiki jadi pasangan komplementer. Lihat
`task/report/frontend/FE-BKC-FIX-009.md`.

**Status 4 September 2026 (lanjutan 4)**: pengguna mengonfirmasi temuan sampingan `FE-BKC-FIX-009`
§ 5 — `CoveragePercent` dan `CoPaymentPercent` seharusnya SALING MELENGKAPI (jumlah 100), bukan dua
pengurang independen; `CoveragePercent` jadi satu-satunya input, `CoPaymentPercent` dihitung
otomatis. Diselesaikan (BACKEND murni) lewat `BE-BKC-FIX-006` — `CalculateCoveredAmount()` (billing)
dan `ResolveTariffInternalAsync` (preview tarif, KEDUA engine diselaraskan sama seperti pelajaran
`BE-BKC-FIX-005`) tidak lagi mengurangi `CoPaymentPercent` secara terpisah; `InsuranceCoverageRuleController`
kini menurunkan `CoPaymentPercent` server-side dari `CoveragePercent` (`100 - CoveragePercent`),
mengabaikan nilai yang dikirim client. Data lama tidak perlu migrasi - cukup buka+simpan ulang rule
lewat form. **Belum diverifikasi hidup** — menunggu rebuild. Lihat `task/report/backend/BE-BKC-FIX-006.md`.
Dependency frontend diselesaikan lewat `FE-BKC-FIX-010` — field "Persentase Co-Payment" pada form
master data Insurance Coverage Rule (create/update) kini read-only, nilainya otomatis mengikuti
`100 - Persentase Coverage` secara live (REUSE override `getFieldDisabled`/`getDisabledReason`
yang sudah disediakan `BaseGroupedEditorForm` lewat `formProps`), dan rule LAMA yang datanya sudah
tidak konsisten langsung menampilkan nilai turunan yang benar begitu form dibuka. Source selesai,
lint bersih, **belum diverifikasi hidup** (dev server Turbopack belum reload). Lihat
`task/report/frontend/FE-BKC-FIX-010.md`.

**Status 4 September 2026 (lanjutan 5)**: permintaan pengguna langsung, independen dari investigasi
coverage/PPN — kolom "Satuan" pada tabel item Menu Pembayaran selalu "-" untuk semua item karena
`InvoiceItemResponse` (`GET .../invoices/{id}`, sumber tabel item, BEDA dari `breakdown.items[]`
calculation-preview) tidak punya field Unit sama sekali. Diselesaikan (BACKEND murni, tidak ada
perubahan frontend - field sudah dibaca `item?.unit`) lewat `BE-BKC-FIX-007` — untuk item kategori
Drug/Pharmacy/Consumable-Alkes (`Category.IsPharmacy`), `Unit` kini diisi `MeasurementName` lewat
rantai `Tariff.Drug.DispenseUnitMeasurement` (navigation property sudah ada, tanpa migration).
Kategori lain/item tanpa TariffId tetap null → frontend fallback "-" seperti sebelumnya. **Belum
diverifikasi hidup** — menunggu rebuild backend. Lihat `task/report/backend/BE-BKC-FIX-007.md`.

**Status 3 September 2026 (lanjutan 7)**: permintaan UX langsung pengguna diselesaikan lewat task
ad-hoc `FE-BKC-FIX-007` — tombol "Batal" tidak lagi `router.push` ke daftar invoice, sekarang
murni mereset form di tempat (`setForm(buildEmptyForm())`, pola sama dengan reset pasca-submit
sukses). REUSE murni, tanpa gate. Terverifikasi hidup — URL tidak berubah, seluruh field kembali
ke placeholder. Lihat `task/report/frontend/FE-BKC-FIX-007.md`.

## `FE-BKC-015` — Badge coverage dan disclaimer pada form testing (pasien asuransi)

| Field | Isi |
| --- | --- |
| Outcome | Kasir/penguji melihat status coverage per tarif sebelum memilih, dengan disclaimer bahwa ini perkiraan |
| Trace | `BKC-DEC-060`; `FR-BKC-005`,`006`; `UAT-03`,`04` |
| Kontrak | `GET catalog-charges/coverage-preview` (API amendment) |
| Reuse | Pola loading/skeleton per-opsi existing pada base component; `FE-BKC-014` sebagai dasar dropdown |
| Scope | Thunk `getCatalogChargeCoveragePreview`; badge 3 status (Tercover/Tercover Sebagian/Tidak Tercover) per opsi dropdown untuk pasien asuransi; teks disclaimer "Perkiraan — angka final dihitung ulang saat tagihan diproses di Menu Pembayaran"; fail-open bila preview gagal dimuat (tidak memblokir submit) |
| Dependency | `BE-BKC-020`; `FE-BKC-014` |
| Acceptance | `UAT-03`,`04`; badge tersembunyi total untuk pasien tunai; preview tidak dipanggil per keystroke |
| Verifikasi | Component test 3 status badge; test fail-open saat API error; lint/build |
| Risiko/pemilik | Kasir salah mengira badge = angka final — dimitigasi disclaimer wajib. Owner Frontend/Billing |
| DoD | Disclaimer tampil setiap kali badge tampil; tests/lint/build lulus |

**Status 3 September 2026**: Source selesai — termasuk perluasan `FilterSelect` (prop opsional
`renderOption`, disetujui eksplisit pengguna lewat gate keputusan) untuk merender badge per opsi.
Disclaimer kondisional dan regresi kunjungan tunai/asuransi **terverifikasi hidup**. Badge visual
itu sendiri **belum bisa diverifikasi** — terhalang `BE-BKC-FIX-001` yang sama dengan `FE-BKC-014`
(belum di-build/restart). Tanpa file test (instruksi eksplisit pengguna). Belum ditandai selesai;
lihat `task/report/frontend/FE-BKC-015.md` § 6 dan § 8.

## `FE-BKC-016` — Subtotal Mandiri dan Subtotal Asuransi terpisah di Menu Pembayaran

| Field | Isi |
| --- | --- |
| Outcome | Kasir melihat dua subtotal berdampingan untuk invoice manapun, bukan satu total dikurangi baris penjamin |
| Trace | `BKC-DEC-062`; `FR-BKC-008`; `CAP-09` |
| Kontrak | Tidak ada kontrak API baru — murni komposisi ulang field `CalculationResponse` existing (`patientAmount`, `primaryAmount`, `excessAmount`) |
| Reuse | `displayedCalculation` yang sudah dikonsumsi `menu-pembayaran-view.jsx` (`harusDibayar`, `subtotalAsuransi` existing) — `CAP-09` Ready to reuse untuk data |
| Scope | Ganti komposisi tampilan: dua baris sejajar "Subtotal Mandiri"/"Subtotal Asuransi" menggantikan "Subtotal Tagihan" tunggal + baris pengurang "Ditanggung Penjamin"; baris "Penjamin Belum Terverifikasi" (`unresolvedCoverageAmount`) dipertahankan apa adanya |
| Dependency | Tidak ada dependency backend baru dari slice ini (data sudah tersedia); **disarankan** dikerjakan setelah `BE-BKC-021` supaya angka yang ditampilkan sudah mencerminkan gating yang diperbarui, tapi tidak teknis wajib menunggu |
| Acceptance | Invoice dengan `patientAmount` dan `primaryAmount`>0 menampilkan dua baris terpisah dengan nominal benar |
| Verifikasi | Component test tampilan dua subtotal dengan beberapa kombinasi nilai (termasuk salah satu nol); lint/build |
| Risiko/pemilik | Perubahan visual pada layar yang sudah dipakai kasir produksi — pastikan `unresolvedCoverageAmount` tidak ikut hilang. Owner Frontend/Billing |
| DoD | Tests/lint/build lulus; tidak ada perubahan formula, murni tampilan |

**Status 3 September 2026**: Source selesai — dua baris "Subtotal Mandiri"/"Subtotal Asuransi"
menggantikan "Subtotal Tagihan"+"Ditanggung Penjamin", identitas aljabar dijaga (tidak ada
perubahan formula). Lint bersih. **Verifikasi visual terhalang** — ditemukan Menu Pembayaran untuk
invoice apa pun saat ini HTTP 500 di backend yang berjalan (`column TariffId does not exist` —
migration `BE-BKC-018` belum dijalankan ke database, temuan baru, di luar scope task ini, berdampak
lebih luas dari `BE-BKC-FIX-001`). Tanpa file test (instruksi eksplisit pengguna). Belum ditandai
selesai; lihat `task/report/frontend/FE-BKC-016.md` § 6 dan § 8.

### Paralelisme slice ini

`FE-BKC-014` dan `FE-BKC-016` boleh paralel (tidak saling bergantung). `FE-BKC-015` menunggu `FE-BKC-014` (menambah badge ke dropdown yang sama) dan `BE-BKC-020`.

## `FE-BKC-017` — Dokumen Kasir: modal menjadi halaman terpisah

| Field | Isi |
| --- | --- |
| Outcome | Tombol "Dokumen Kasir" (umum maupun per-tender "Cetak Kwitansi") tidak lagi membuka modal — kasir dinavigasikan ke halaman tersendiri dengan isi identik (Kwitansi per tender, Struk Pasien, 6 tab placeholder), tombol Cetak (unduh PDF) dan tombol Kembali ke Menu Pembayaran |
| Trace | `BKC-DEC-063`–`064` (amendment `00-interview-decisions.md` 3 September 2026); `03-frontend-architecture.md` amendment 3 September 2026 |
| Kontrak | Tidak ada kontrak API baru — murni perubahan wadah presentasi frontend. Data yang ditampilkan sama persis dengan modal existing (`BKC-DEC-052`–`058`, tidak diamendemen) |
| Reuse | `KwitansiDocument`, `StrukPasienDocument`, `useDokumenKasir` (PDF/share, `html2pdf.js`) dipakai ulang apa adanya; `useBillingInvoiceDetail`+`useBillingSettlement` dipakai ulang untuk data loading halaman baru (bukan `useMenuPembayaran` penuh); pola halaman referensi terdekat: `InpatientConsentPrintView` (Hero + area dokumen + action bar Kembali/Cetak) |
| Scope | Route baru `[slug]/pembayaran/dokumen-kasir` (query string `?tab=&tenderId=`), route builder `BILLING_INVOICE_ROUTES.dokumenKasir`, ganti dua titik pemicu di `menu-pembayaran-view.jsx` dari `onClick` buka-modal menjadi navigasi, hapus `dokumen-kasir-modal.jsx` (tidak ada konsumen lain — dikonfirmasi lewat pencarian referensi source), loading/error state halaman baru (invoice belum dimuat, tender tidak ditemukan) |
| Dependency | Tidak ada dependency backend baru. Bergantung pada `menu-pembayaran-view.jsx`/`use-dokumen-kasir.js` existing (`FE-BKC-011`) sebagai baseline perilaku yang dipertahankan |
| Acceptance | Lihat acceptance 29–31 pada `03-frontend-architecture.md` amendment 3 September 2026 |
| Verifikasi | Lint/build; grep anti-regresi memastikan tidak ada reference mati ke `dokumen-kasir-modal.jsx`; verifikasi manual ter-autentikasi (buka dari kedua titik pemicu, cetak PDF, tombol Kembali) — direkomendasikan, belum tentu bisa dijalankan builder tanpa kredensial |
| Risiko/pemilik | Kehilangan `activeTender` di tengah navigasi bila `tenderId` tidak valid/tender tidak ditemukan pada `settlement.tenders` hasil load ulang — halaman baru **MUST** menampilkan pesan yang jelas (bukan crash/blank), bukan diam-diam fallback ke tab lain. Owner Frontend/Billing |
| DoD | Lint/build lulus bersih; `dokumen-kasir-modal.jsx` terhapus; kedua titik pemicu menavigasi ke route baru; `git status --short` dilaporkan |

**Status 3 September 2026**: Source selesai — route baru `[slug]/pembayaran/dokumen-kasir`, hook
`use-dokumen-kasir-page.js`, view `dokumen-kasir-view.jsx`, route builder `dokumenKasir` di
`billing-invoice-constants.js`; kedua titik pemicu di `menu-pembayaran-view.jsx` diganti navigasi;
`dokumen-kasir-modal.jsx` dan composition `useDokumenKasir` di `use-menu-pembayaran.js` dihapus.
Lint (`eslint . --quiet`) **PASS** 0 error pada file berubah/baru. `test:unit` **PASS** 434/434,
tanpa regresi (tidak ada test baru — task presentational murni, konsisten pola `FE-BKC-011`/`016`).
`npm run build` **BLOCKED/INCONCLUSIVE** — `.next/standalone` terkunci oleh instance app yang
sedang berjalan di lingkungan builder; pengguna memilih tidak menghentikan proses itu. Verifikasi
manual ter-autentikasi (buka dari kedua titik pemicu, cetak PDF, tombol Kembali) **belum
dijalankan** — tidak ada kredensial di lingkungan builder. Lihat
`task/report/frontend/FE-BKC-017.md`.

---

# Amendment 4 September 2026 — Gelombang `MVP-6`, `MVP-9`, dan `MVP-12`

## Metadata gelombang ini

```yaml
roadmap_revision: 2
roadmap_status: DRAFT_FORWARD_TEST
approval_gate: BLUEPRINT_APPROVED — kontrak dikunci 4 September 2026 oleh Product/Domain Owner (wewenang ganda Finance/AR, `BKC-DEC-085`)
approved_by: [Product/Domain Owner]
approved_at: 2026-09-04
blueprint_revision_dibaca: 0.8 (manifest, approved) + 0.9 (02-backend-architecture.md, masih draft — BKC-GATE-06 terpisah)
blueprint_status: approved (revisi 0.8) — readiness DESIGN_APPROVED
cakupan: MVP-6 (lembar Invoice Asuransi), MVP-9 (Menu Pembayaran), MVP-12 (Pengecualian Finansial)
epic: [EPIC BKC-05, EPIC BKC-06, EPIC BKC-07, EPIC BKC-09]
task_id_series: FE-BKC-018 s.d. FE-BKC-021 — dilanjutkan dari FE-BKC-017
frontend_commit_sha_pada_manifest: 00210f9a5fb2f4f69e57b8c90c57c63c788da792
frontend_commit_sha_terverifikasi: belum diperiksa — repository frontend tidak tersedia pada sesi ini (BKC-GAP-07). Kontrak dikunci TIDAK menghapus kebutuhan verifikasi ini
catatan_baseline_backend: HEAD backend terbukti 52 commit dan 237 berkas source di depan baseline manifest; baseline frontend patut diduga bergerak serupa dan wajib diperiksa sebelum task frontend disetujui
contracts: [BIL-API-0.7 (approved), BIL-PERMISSION-0.6 (approved)]
input_revisions:
  03-frontend-architecture.md: 0.7 (amendment 3 September kedua dan 4 September)
  04-prd-to-mvp.md: 0.8
```

## 0. Dua peringatan yang menentukan gelombang ini

> **Pertama: setiap task frontend di bawah menunggu backend yang belum boleh dikerjakan.** Ketiga
> gelombang ini membaca angka, bukan menghitungnya. Dipasang lebih dulu, ketiganya akan menampilkan
> Rp 0 pada kolom yang justru paling penting — dan Rp 0 yang salah jauh lebih berbahaya daripada
> layar yang belum ada, karena petugas menyimpulkan "tidak ada yang perlu ditindaklanjuti".

> **Kedua: keputusan rupa tetap milik pelaksana.** Warna, jarak, ikon, bentuk tab, lebar kolom, dan
> pilihan pustaka komponen adalah `DEV_DISCRETION`. Yang mengikat hanya: dari mana angka diambil,
> baris mana yang tampil, hak akses tiap tombol, dan makna keadaan kosong serta gagal.

## 1. Aturan yang berlaku untuk seluruh task gelombang ini

| Aturan | Isi |
| --- | --- |
| Sumber angka | Seluruh rupiah berasal dari tanggapan server. Layar **tidak boleh** menghitung, menyaring, atau menjumlahkan ulang rupiah sendiri |
| Keadaan wajar versus galat | Keadaan seperti "pasien ini bayar tunai" adalah keadaan bisnis normal. Layar menampilkannya sebagai keterangan biru, **bukan** pesan galat merah |
| Anomali data | Ditampilkan sebagai peringatan kuning di atas Ringkasan Pembayaran, **tidak pernah** sebagai baris subtotal, dan **tidak pernah** sebagai galat merah |
| Tombol pembayaran | **Tetap aktif** walaupun ada peringatan anomali. Pasien tetap harus bisa membayar |
| Privasi | Nomor polis dan nomor anggota boleh tampil di lembar cetak, tetapi **tidak boleh** masuk `console.log`, telemetri, maupun penyimpanan peramban |
| Status tidak hanya warna | Setiap penanda memakai teks lengkap, bukan hanya lencana berwarna |

## `FE-BKC-018` — Tab "Invoice Asuransi" pada halaman Dokumen Kasir

| Field | Isi |
| --- | --- |
| Outcome | Kasir dapat membuka satu tab, membaca lembar Invoice Asuransi yang siap diserahkan ke perusahaan asuransi, dan mengunduhnya sebagai PDF A4 |
| Gelombang | `MVP-6` (`EPIC BKC-05`) |
| Trace | `FR-BKC-018`, `FR-BKC-019`; `BKC-DEC-065`–`069`; `03-frontend-architecture.md` § Amendment 3 September 2026 (kedua) |
| Kontrak | `BIL-API-0.5`/`0.6` — `GET /{id}/insurance-invoice-document`; hak akses `BillingInvoice : Read` (dipakai ulang) |
| Reuse | Pola `KwitansiDocument`/`StrukPasienDocument` apa adanya; halaman Dokumen Kasir hasil `FE-BKC-017`; `html2pdf.js` yang sudah dipakai |
| Scope | Satu komponen lembar baru; satu tab baru sejajar Kwitansi dan Struk Pasien, **sebelum** enam tab placeholder; satu pemanggilan data beserta tiga slot keadaan dan penyeleksinya; parameter ukuran kertas opsional pada pembuat PDF; perbaikan pemilihan tab dari alamat halaman |
| Dependency | `BE-BKC-023` **selesai dan terverifikasi** (source selesai; `dotnet test` masih menunggu pengguna); `BKC-GATE-03` **ditutup** |
| Status | ~~`BLOCKED` oleh `BE-BKC-023` dan `BKC-GATE-03`~~ **`lint:errors`/`test:unit`/`build` lulus 6 September 2026** — `BKC-GATE-03` ditutup lewat `BKC-DEC-092` (Security Owner); `BE-BKC-023` terverifikasi lulus pengguna. Menunggu verifikasi manual ter-autentikasi; lihat `task/report/frontend/FE-BKC-018.md` |

**Kenapa ukuran kertasnya berubah menjadi A4.** Tabel Invoice Asuransi punya kolom tambahan
"Ditanggung Asuransi" dan "Porsi Pasien". Pada kertas A5 selebar 148 mm, kolom paling kanan
terpotong — persis kolom yang paling dibutuhkan pihak asuransi. Perubahannya berbentuk **pilihan
tambahan** dengan bawaan tetap A5, sehingga Kwitansi dan Struk Pasien **tidak berubah sama sekali**.

**Satu perbaikan yang ikut terbawa.** Pemilihan tab dari alamat halaman hari ini hanya mengenali
dua jalur, sehingga tautan `?tab=INVOICE_ASURANSI` akan mendarat di tab yang salah. Perbaikannya
membuat setiap nilai tab yang dikenali dihormati, dan hanya jatuh ke Struk Pasien bila nilainya
kosong atau tidak dikenali. Ini juga memperbaiki tautan tab lain yang hari ini diam-diam diabaikan.

Aksi per peran:

| Aksi | Kasir | Petugas Billing | Keterangan |
| --- | :---: | :---: | --- |
| Membuka tab "Invoice Asuransi" | Ya | Ya | Gerbang hak akses sama dengan seluruh halaman Dokumen Kasir |
| Menekan "Cetak Invoice Asuransi" | Ya | Ya | Tombol hanya muncul saat tab aktif **dan** lembar dinyatakan dapat dicetak |
| Mengubah isi lembar dari layar | **Tidak** | **Tidak** | Lembar murni baca; koreksi angka lewat item tagihan atau Pengecualian Finansial |

Acceptance criteria:

1. Membuka halaman dengan `?tab=INVOICE_ASURANSI` mendarat langsung di tab Invoice Asuransi, bukan di Struk Pasien.
2. Untuk pasien asuransi dengan sedikitnya satu baris tercover, lembar menampilkan nama perusahaan asuransi, nomor polis, dan tabel berisi kolom rupiah yang ditanggung per baris; jumlah kolom itu sama dengan total tanggungan di kaki tabel.
3. Baris yang tidak ditanggung asuransi **tidak** muncul di lembar, meskipun muncul di Struk Pasien pada tagihan yang sama.
4. Untuk kunjungan tunai, tab menampilkan keterangan biru bahwa lembar tidak dapat diterbitkan — tanpa lembar, tanpa tombol cetak, dan **bukan** pesan galat merah.
5. Untuk tagihan yang difinalkan sebelum pembaruan sistem, tab menampilkan total tanggungan beserta keterangan bahwa rincian per baris tidak tersedia, dan tombol cetak tidak muncul.
6. Menekan cetak menghasilkan PDF A4 yang seluruh kolom tabelnya terbaca utuh tanpa terpotong di sisi kanan.
7. Cetak Kwitansi dan Struk Pasien **tetap** menghasilkan PDF A5 seperti sebelumnya.
8. Tab ini **tidak** memicu permintaan data selama kasir belum membukanya.
9. Peringatan yang tidak kosong tetap ditampilkan meskipun tabelnya sudah terisi — peringatan yang disembunyikan karena tabel sudah terisi adalah peringatan yang gagal bekerja.
10. Lembar dari tagihan yang masih berjalan mencantumkan keterangan "Tagihan masih berjalan — angka dapat berubah sampai tagihan difinalkan."

Bukti verifikasi: pemeriksaan gaya penulisan kode dan uji unit; satu berkas PDF A4 hasil cetak;
verifikasi manual ter-autentikasi untuk kelima keadaan penjamin. **Build dijalankan pengguna secara
manual**, bukan oleh pelaksana task.

Risiko dan pemilik: lembar memuat nomor polis. Nama berkas PDF memakai nomor tagihan, **bukan** nama
pasien, supaya nama pasien tidak ikut tersebar lewat nama berkas di folder unduhan. Owner Frontend +
Security.

Definition of Done: sepuluh acceptance terpenuhi; tidak ada regresi pada Kwitansi dan Struk Pasien;
tidak ada nomor polis di log peramban; hasil build dilaporkan pengguna.

## `FE-BKC-019` — Ringkasan Pembayaran yang benar-benar menjumlah

| Field | Isi |
| --- | --- |
| Outcome | Kasir melihat ringkasan yang menjumlah persis ke Total Tagihan, tanpa baris menggantung yang tidak dapat ditagihkan kepada siapa pun |
| Gelombang | `MVP-9` bagian pertama (`EPIC BKC-06`) |
| Trace | `FR-BKC-025`, `FR-BKC-026`; `BKC-DEC-075`; `03-frontend-architecture.md` § Amendment 4 September 2026 |
| Kontrak | `BIL-API-0.6`/`0.7` — field baru pada `breakdown.coverage` |
| Reuse | Blok Ringkasan Pembayaran hasil `FE-BKC-016`; tidak ada layar baru |
| Scope | Menghapus baris "Penjamin Belum Terverifikasi"; menampilkan baris "Selisih Tidak Ditagihkan (kontrak penjamin)" yang **menjumlahkan dua field** dan hanya muncul bila nilainya lebih dari nol; memastikan seluruh baris menjumlah ke Total Tagihan |
| Dependency | `BE-BKC-024`, `BE-BKC-025`, dan `BE-BKC-028` — ketiganya `DONE`, `dotnet build`/`test` dikonfirmasi lulus pengguna |
| Status | ~~`BLOCKED` oleh ketiga task backend tersebut~~ **`lint:errors`/`test:unit`/`build` lulus 6 September 2026** — menunggu verifikasi manual ter-autentikasi; lihat `task/report/frontend/FE-BKC-019.md` |

**Contoh berangka.** Tagihan Rp 425.000 dengan Subtotal Mandiri Rp 85.000, Subtotal Asuransi
Rp 340.000, pajak Rp 0, dan selisih Rp 0. Jumlahnya Rp 425.000, tanpa selisih satu rupiah pun.

**Satu baris, dua sumber.** Baris "Selisih Tidak Ditagihkan" **wajib** menjumlahkan nominal
menggantung dan nominal selisih tidak dapat ditagihkan menjadi satu angka. Kasir tidak
berkepentingan membedakan sebabnya; pemisahannya baru berguna di layar Pengecualian Finansial.
Karena kedua field selalu dijumlahkan di layar ini, angka yang dilihat kasir **tidak berubah sama
sekali** oleh perpindahan field di backend. Bila layar lupa menjumlahkan salah satunya, kasir akan
melihat selisih menghilang tanpa ada yang mengubah tagihan.

Acceptance criteria:

1. `BIL-AT-053` — baris "Penjamin Belum Terverifikasi" **tidak ada** di halaman.
2. Subtotal Mandiri + Subtotal Asuransi + Pajak Mandiri + Pajak Asuransi + Selisih Tidak Ditagihkan menjumlah persis ke Total Tagihan.
3. Baris "Selisih Tidak Ditagihkan" hanya muncul bila nilainya lebih besar dari nol.
4. Baris tersebut menjumlahkan kedua field sumber, dan hasilnya sama dengan sebelum perpindahan field di backend.
5. Kolom cadangan untuk penjamin kedua tidak lagi ditampilkan — nilainya permanen nol.

Risiko dan pemilik: ini layar yang paling sering dipakai kasir. Kesalahan penjumlahan langsung
terlihat sebagai uang yang hilang atau bertambah. Owner Frontend + Billing/Finance.

Definition of Done: kelima acceptance terpenuhi; angka pada tagihan pasien tunai tidak berubah sama
sekali; hasil build dilaporkan pengguna.

## `FE-BKC-020` — Peringatan anomali data dan penanda per baris

| Field | Isi |
| --- | --- |
| Outcome | Kasir melihat masalah data pendaftaran sebagai peringatan yang dapat ditindaklanjuti, dan tetap dapat menerima pembayaran sampai tuntas |
| Gelombang | `MVP-9` bagian kedua (`EPIC BKC-07`) |
| Trace | `FR-BKC-027`, `FR-BKC-029`; `BKC-DEC-073`; `BKC-DES-011` |
| Kontrak | `BIL-API-0.6` — `hasDataAnomaly`, `anomalyCodes`, `anomalyMessages`, `dataAnomalyAmount` |
| Reuse | Komponen peringatan yang sudah ada; penanda per baris hasil `FE-BKC-FIX-006`/`FIX-008` |
| Scope | Peringatan kuning di atas Ringkasan Pembayaran berisi kalimat dari server; penanda baris bernilai "anomali data" untuk baris yang terdampak; nominal anomali **tidak** dijadikan baris subtotal |
| Dependency | `BE-BKC-025` **selesai dan terverifikasi hidup** |
| Status | ~~`BLOCKED` oleh `BE-BKC-025`~~ **`lint:errors`/`test:unit`/`build` lulus 6 September 2026** — menunggu verifikasi manual ter-autentikasi; lihat `task/report/frontend/FE-BKC-020.md` |

**Contoh berangka.** Biaya yang memenuhi syarat Rp 440.000 dengan kelayakan penjamin belum
dicentang. Perhitungan berhasil: Subtotal Mandiri Rp 440.000, Subtotal Asuransi Rp 0, Total Tagihan
Rp 440.000. Kalimat "Rp 440.000" muncul di dalam peringatan kuning di atas ringkasan, sementara
Ringkasan Pembayaran hanya memuat Subtotal Mandiri, Subtotal Asuransi, dan Total Tagihan. Kasir
menerima Rp 440.000 tanpa hambatan.

Acceptance criteria:

1. `BIL-AT-054` — peringatan kuning tampil di atas Ringkasan Pembayaran; tombol pembayaran **tetap aktif**; pembayaran dapat diselesaikan sampai tuntas.
2. Kalimat peringatan diambil apa adanya dari server; layar **tidak** mengarang kalimatnya sendiri.
3. Nominal anomali **tidak pernah** muncul sebagai baris di dalam Ringkasan Pembayaran.
4. Peringatan tidak disampaikan hanya lewat warna — teksnya lengkap dan terbaca pembaca layar.
5. Penanda per baris pada tagihan pasien tunai **tetap** "Tunai" untuk semua baris. Daftar hasil yang kosong berarti "seluruhnya pasien", **bukan** "data belum termuat".

Risiko dan pemilik: bila peringatan ini ditampilkan sebagai galat merah, kasir akan mengira sistem
rusak dan berhenti menagih — padahal pasien justru sedang bisa membayar. Owner Frontend + Billing.

Definition of Done: kelima acceptance terpenuhi; satu tangkapan layar tersanitasi dilampirkan;
hasil build dilaporkan pengguna.

## `FE-BKC-021` — Layar Pengecualian Finansial untuk selisih yang tidak dapat ditagihkan

| Field | Isi |
| --- | --- |
| Outcome | Petugas keuangan melihat berapa selisih yang belum ditanggung pada satu tagihan, mengajukannya dengan nominal yang sudah terisi, dan atasannya menyetujui |
| Gelombang | `MVP-12` (`EPIC BKC-09`) |
| Trace | `FR-BKC-040`–`FR-BKC-044`; `BKC-DEC-080`, `BKC-DEC-036`; `BKC-DES-023`, `BKC-DES-024` |
| Kontrak | `BIL-API-0.7` — `nonBillableResidualRemaining`, field `category` pada pengajuan dan tanggapan |
| Reuse | Layar Pengecualian Finansial hasil `FE-BKC-008` beserta seluruh alur pengajuan, persetujuan, dan pembatalannya |
| Scope | Menampilkan sisa selisih yang belum ditanggung; pilihan kategori pada formulir pengajuan; pengisian awal nominal; peringatan pada layar finalisasi bila masih ada selisih yang belum ditanggung |
| Dependency | `BE-BKC-029` **selesai dan terverifikasi hidup** |
| Status | ~~`BLOCKED` oleh `BE-BKC-029`~~ **`lint:errors`/`test:unit`/`build` lulus 6 September 2026 untuk acceptance 1–6.** Acceptance 7 **TIDAK dikerjakan** — diblokir `BKC-GAP-01` (desain `BKC-DEC-090` belum ditulis). Menunggu verifikasi manual ter-autentikasi; lihat `task/report/frontend/FE-BKC-021.md` |

**Proses bisnis yang dilayani layar ini.**

1. **Tujuan** — selisih yang tidak dapat ditagihkan kepada siapa pun berakhir sebagai keputusan bernama pelaku.
2. **Pelaku** — petugas keuangan mengajukan; atasannya menyetujui. Layar **tidak** mengajukan sendiri.
3. **Pemicu** — petugas membuka layar Pengecualian Finansial pada tagihan yang memuat selisih.
4. **Prasyarat** — sisa selisih pada tagihan itu lebih besar dari nol.
5. **Langkah utama** — (a) petugas membaca "Selisih tidak dapat ditagihkan yang belum ditanggung: Rp 60.000"; (b) menekan tombol pengajuan; (c) nominalnya sudah terisi dan kategorinya sudah terpilih; (d) petugas menuliskan alasannya; (e) atasan membaca dan menyetujui.
6. **Aturan bisnis** — nominal dibatasi sisa selisihnya sendiri, **bukan** sisa tagihan pasien.
7. **Perubahan status** — kasus berpindah `SUBMITTED` → `POSTED`. Status **tagihan tidak berpindah**.
8. **Jalur tidak normal** — pembatalan membuka kembali selisihnya untuk diajukan ulang, dan riwayat kedua catatan tetap terbaca.
9. **Hasil akhir** — sisa selisih menjadi nol; sisa tagihan pasien **tetap seperti semula**.

**Contoh berangka.** Tagihan dengan selisih Rp 60.000 dan sisa tagihan pasien Rp 25.000. Pengajuan
Rp 60.000 **diterima** walaupun melebihi Rp 25.000. Pengajuan Rp 75.000 **ditolak** walaupun masih
di bawah Total Tagihan Rp 425.000. Sesudah disetujui, sisa tagihan pasien **tetap Rp 25.000**, dan
kwitansi pasien tidak menyebut angka Rp 60.000 sama sekali.

Acceptance criteria:

1. Sisa selisih dibaca dari server dan **tidak** dihitung ulang di layar dari daftar kasus. Perhitungan uang di sisi layar akan menyimpang dari server begitu ada satu kasus yang tidak ikut terkirim.
2. Formulir pengajuan menyediakan pilihan kategori, dan nominalnya terisi awal dari sisa selisih.
3. Pengajuan yang melebihi sisa selisih ditolak, beserta pesan yang menyebut **selisihnya** — bukan menyebut tagihan pasien.
4. Pengaju tidak dapat menyetujui pengajuannya sendiri; pesannya menyebut pemeriksaan dua orang.
5. Pengajuan kategori selisih yang ditandai sebagai pelunasan penuh ditolak.
6. Sesudah pengajuan disetujui, sisa tagihan pasien pada layar **tidak berubah** dan status tagihan **tidak berpindah**.
7. Layar finalisasi menampilkan peringatan bila masih ada selisih yang belum ditanggung, tetapi **tidak memblokir** finalisasi. Ini keadaan yang dipilih sengaja (`BKC-DEC-090`), bukan kelalaian.

Risiko dan pemilik: bila layar menghitung sendiri sisa selisihnya, angkanya akan berbeda dari
plafon yang dijaga server, dan petugas akan melihat pengajuannya ditolak tanpa alasan yang terlihat.
Alasan pengajuan **tidak boleh** memuat nomor polis, nomor anggota, nama pasien, maupun diagnosis.
Owner Frontend + Finance/AR + Security.

Definition of Done: ketujuh acceptance terpenuhi; write-off piutang pasien yang sudah berjalan tidak
berubah tampilannya; hasil build dilaporkan pengguna.

## 2. Ringkasan status task gelombang ini

| Task | Gelombang | Status | Yang menahan |
| --- | --- | --- | --- |
| `FE-BKC-018` | `MVP-6` | **`lint:errors`/`test:unit`/`build` lulus 6 September 2026 — menunggu verifikasi manual ter-autentikasi** | — `BKC-GATE-03` ditutup 5 September 2026 (`BKC-DEC-092`); lihat `task/report/frontend/FE-BKC-018.md` |
| `FE-BKC-019` | `MVP-9` | **`lint:errors`/`test:unit`/`build` lulus 6 September 2026 — menunggu verifikasi manual** | — dependency `DONE`; lihat `task/report/frontend/FE-BKC-019.md` |
| `FE-BKC-020` | `MVP-9` | **`lint:errors`/`test:unit`/`build` lulus 6 September 2026 — menunggu verifikasi manual** | — dependency `DONE`; lihat `task/report/frontend/FE-BKC-020.md` |
| `FE-BKC-021` | `MVP-12` | **`lint:errors`/`test:unit`/`build` lulus 6 September 2026 untuk acceptance 1–6 — menunggu verifikasi manual** | — dependency `DONE`; acceptance 7 diblokir `BKC-GAP-01` (desain `BKC-DEC-090` belum ditulis); lihat `task/report/frontend/FE-BKC-021.md` |

Keempatnya sudah lulus lint/test/build lokal dan tidak lagi menunggu backend. Satu pertanyaan
bisnis masih terbuka: acceptance 7 pada `FE-BKC-021` menunggu desain `BKC-DEC-090` (`BKC-GAP-01`) —
bukan `DEV_DISCRETION`, sengaja tidak dikerjakan tanpa desain yang disetujui. Seluruh keputusan rupa
lain yang belum diambil pada keempat layar ini sudah dinyatakan `DEV_DISCRETION` dan memang menjadi
wewenang pelaksana.

## 3. Catatan pekerjaan frontend yang sudah berjalan di luar penomoran roadmap

`FE-BKC-012`, `FE-BKC-013`, dan `FE-BKC-FIX-001` sudah dikerjakan dan dilaporkan, tetapi belum
pernah masuk dokumen roadmap ini — laporannya ada di `task/report/frontend/`. Ketiganya tercatat
pada `requirement-traceability.md`. Perapian penomorannya adalah pekerjaan pemeliharaan roadmap,
bukan bagian gelombang ini.

