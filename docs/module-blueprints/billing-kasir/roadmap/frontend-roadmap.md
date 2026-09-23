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
| Perbaikan 16 September 2026 (`BE-BKC-FIX-010`) | Field "Kategori Kena Pajak"/`taxableCategory` dihapus total dari form create/update, kolom list, dan teks deskripsi CRUD Tax Rule — mengikuti penghapusan kolom `TaxableCategory` di backend (`BKC-DEC-098`). Lint/test/build **belum dijalankan** (instruksi baku pengguna). Laporan: [BE-BKC-FIX-010](../task/report/frontend/BE-BKC-FIX-010.md) |

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

**Status 7 September 2026 (lanjutan 6)**: permintaan fitur baru langsung pengguna, dengan
referensi visual (screenshot) — halaman "Riwayat Pembayaran" (daftar SEMUA pembayaran lintas
invoice/pasien, beda dari Running Invoice yang tidak membawa info penjamin/pembayaran). Tiga
keputusan scope dikonfirmasi lewat `AskUserQuestion` sebelum implementasi: satu baris = satu
invoice; aksi "Lihat Kwitansi" menampilkan daftar SEMUA Kwitansi invoice itu; halaman menyertakan
SEMUA invoice ber-pembayaran (bukan cuma yang sudah lunas total). Diselesaikan lewat `BE-BKC-FIX-008`
(endpoint baru `GET .../invoices/payment-history`, dua-pass query, `ClaimMethod`/nama penjamin/
status Lunas-Cicilan/daftar Kwitansi per invoice) + `FE-BKC-FIX-011` (halaman baru, murni REUSE
base component, entri menu sidebar baru "Riwayat Pembayaran"). Helper
`derivePaymentInstallmentSummary` (label "Angsuran N/M", status per Kwitansi) disiapkan sebagai
fondasi untuk redesain `kwitansi-document.jsx` yang DIMINTA TAPI BELUM DIKERJAKAN — referensi
Kwitansi pengguna menunjukkan layout jauh berbeda dari implementasi saat ini (Total Tagihan/Rincian
Pembayaran/Sisa Pembayaran/nama petugas kasir/detail dokter), mengubah dokumen yang sudah dipakai
produksi untuk setiap pembayaran — menunggu keputusan cakupan pengguna sebelum dikerjakan (lihat
`task/report/frontend/FE-BKC-FIX-011.md` § 5). Lint (`eslint . --quiet` repo penuh) PASS 0 error.
`test:unit`/`build` tidak dijalankan pada giliran ini (instruksi eksplisit pengguna). **Belum
diverifikasi hidup**. Lihat `task/report/backend/BE-BKC-FIX-008.md` dan
`task/report/frontend/FE-BKC-FIX-011.md`.

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
| Status | 🟡 **Dikerjakan ulang 7 September 2026** setelah status "lint/test/build lulus 6 September" terbukti tidak sesuai realita (lihat Amendment 7 September 2026 § 1) — source sekarang BENAR-BENAR ada di disk (branch `QuilvianIntegrationFrontend`). `eslint . --quiet` (repo penuh) PASS 0 error, `npm run test:unit` PASS 440/440. `npm run build` sengaja TIDAK dijalankan pelaksana (pengguna eksplisit meminta menjalankannya sendiri). Menunggu verifikasi manual ter-autentikasi (acceptance 2–6, 9, 10); lihat `task/report/frontend/FE-BKC-018.md` |

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
| `FE-BKC-018` | `MVP-6` | 🟡 **Dikerjakan ulang 7 September 2026 — lint/test:unit lulus, `build` sengaja tidak dijalankan (pengguna), menunggu verifikasi manual ter-autentikasi** | — status "lulus 6 September" versi sebelumnya terbukti tidak sesuai realita (Amendment 7 September 2026 § 1); dikerjakan ulang dari nol pada sesi yang menemukan gap ini; lihat `task/report/frontend/FE-BKC-018.md` |
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

---

# Amendment 7 September 2026 — `FE-BKC-018` dibuka ulang; `FE-BKC-022` (Struk Pasien) baru

```yaml
roadmap_revision: 3
roadmap_status: DRAFT_FORWARD_TEST
frontend_branch_diperiksa: QuilvianIntegrationFrontend (checked out saat sesi ini)
frontend_head_diperiksa: 12f9242ce
```

## 0. Kenapa amendment ini ada

Pemilik modul meminta dua hal: (1) rencanakan tab "Invoice Asuransi" di Dokumen Kasir — laporan
lama (`FE-BKC-018.md`) menyebutnya sudah selesai, tetapi grep langsung ke seluruh
`QuilvianSystemFrontendDev/src` untuk `Invoice Asuransi` dan identifier terkait **tidak menemukan
satu pun hasil**; dan (2) selaraskan Struk Pasien dengan PDF referensi dari lingkungan staging yang
formatnya jauh lebih kaya dari implementasi lokal saat ini.

## 1. `FE-BKC-018` — dibuka ulang, BUKAN task baru

### 1.1 Bukti yang membatalkan status "selesai"

Laporan `task/report/frontend/FE-BKC-018.md` (belum diubah oleh sesi ini — hanya build skill yang
berwenang menulis di sana) mengklaim: delapan berkas baru/diubah termasuk
`invoice-asuransi-document.jsx`, thunk `getInsuranceInvoiceDocument`, `npm run lint:errors`
**PASS**, `npm run test:unit` **PASS** (440/440), `npm run build` **PASS** (275/275 halaman). Status
tertulis "Belum di-commit".

Verifikasi langsung sesi ini:

| Pemeriksaan | Perintah/cara | Hasil |
| --- | --- | --- |
| Berkas `invoice-asuransi-document.jsx` | `Glob **/invoice-asuransi-document.jsx` pada `QuilvianSystemFrontendDev/src` | **Tidak ditemukan** |
| String `INVOICE_ASURANSI`/`insuranceInvoiceDocument`/`InvoiceAsuransi` | `grep -r` pada seluruh `src/` | **Nol hasil** |
| Tab nyata pada `dokumen-kasir-view.jsx` saat ini | Pembacaan langsung berkas | Hanya dua tab nyata: `KWITANSI`, `STRUK_PASIEN`, ditambah enam tab placeholder — **tidak ada** tab ketiga |
| Commit yang berisi perubahan ini | `git log`/`git reflog -30` pada branch `QuilvianIntegrationFrontend` (checked out) dan `yasmina` (disebut laporan, tip `699935230`) | **Tidak ditemukan** pada keduanya |
| Stash yang menyimpan perubahan ini | `git stash list` | **Kosong** |

**Kesimpulan: pekerjaan yang diklaim laporan `FE-BKC-018.md` tidak ada di repository manapun yang
dapat diperiksa sesi ini.** Karena laporan itu sendiri menyatakan "Belum di-commit", dan tidak ada
jejak di reflog maupun stash, kemungkinan paling masuk akal adalah working tree tempat perubahan
itu ditulis sudah tidak dapat ditemukan lagi (mis. sandbox/sesi builder yang terpisah dan tidak
pernah tersinkron ke repository ini) — bukan sekadar "menunggu commit". Ini dicatat sebagai
`BKC-GAP-08` pada `requirement-traceability.md`.

### 1.2 Keputusan roadmap

Sesuai `status-task-roadmap.md` § 5 ("menurunkan status juga wajib, ditulis sebagai keputusan
bertanggal"): status `FE-BKC-018` **diturunkan** dari "lint/test/build lulus" menjadi **belum
dikerjakan**, per 7 September 2026, dengan alasan di atas. Task ID, judul, acceptance criteria,
kontrak, dan dependency **tidak berubah** — hanya statusnya. Riwayat lama pada § 1 (di atas) dan
pada `task/report/frontend/FE-BKC-018.md` **tidak dihapus**, dibiarkan sebagai jejak yang sudah
terbukti tidak dapat dipercaya.

| Field | Nilai baru |
| --- | --- |
| Status | **Diturunkan 7 September 2026** — belum dikerjakan. Klaim "lint/test/build lulus 6 September 2026" pada riwayat sebelumnya **tidak dapat diverifikasi ulang** — nol jejak source di repository manapun yang diperiksa. `BE-BKC-023` (dependency) tetap `DONE` dan terverifikasi langsung (lihat `backend-roadmap.md` § Amendment 7 September 2026) — task ini `READY_FOR_TASK_APPROVAL`, dikerjakan ulang dari nol |
| Dependency | `BE-BKC-023` — source terverifikasi ada di `BillingInvoicesController.cs` baris 303; jalankan `dotnet build` sekali lagi sebelum mengandalkannya (catatan regresi DI, lihat `backend-roadmap.md`) |
| Acceptance criteria | **Tidak berubah** dari isi tabel di atas (1–9 pada § "Acceptance criteria" `FE-BKC-018`) |
| Scope | **Tidak berubah** — satu tab baru sejajar Kwitansi/Struk Pasien, satu komponen cetak A4, satu pemanggilan data, perbaikan pengenalan tab dari query string |

**Update 7 September 2026 (lanjutan, sesi berbeda)**: task ini dikerjakan ulang dari nol sesuai
keputusan di atas. Source sekarang terverifikasi BENAR-BENAR ada di disk (branch
`QuilvianIntegrationFrontend`) — `invoice-asuransi-document.jsx` (baru), thunk
`getInsuranceInvoiceDocument` + tiga slot state (`billing-invoice-slice.jsx`), perbaikan pengenalan
tab lewat `DOKUMEN_KASIR_RECOGNIZED_TABS` (`billing-invoice-constants.js`, satu sumber kebenaran
dipakai view dan hook), `paperSize` opsional pada `buildPdf` (`use-dokumen-kasir.js`), dan blok
tampilan empat-keadaan pada `dokumen-kasir-view.jsx`. `eslint . --quiet` (repo penuh) PASS 0 error;
`npm run test:unit` PASS 440/440 tanpa regresi. `npm run build` sengaja TIDAK dijalankan pelaksana —
pengguna eksplisit meminta menjalankannya sendiri. Belum di-commit, belum diverifikasi manual
ter-autentikasi. Lihat `task/report/frontend/FE-BKC-018.md` (ditulis ulang, riwayat versi
sebelumnya yang tidak akurat tetap tercatat di § 0 laporan itu).

## 2. `FE-BKC-022` — Struk Pasien: penyelarasan terhadap referensi staging (~~**`BLOCKED` penuh**~~ **DITUTUP — lihat Amendment 8 September 2026 di bawah**)

**Status baris ini sudah usang.** Keempat elemen di bawah sudah dijawab lewat `/grill-me`
(`BKC-DEC-093`–`096`, `00-interview-decisions.md`, `approved`). Definisi task yang berlaku
sekarang ada di § "Amendment 8 September 2026 — `FE-BKC-022` dibuka kembali sebagai
`READY_FOR_TASK_APPROVAL`" di akhir berkas ini. Isi asli di bawah **dipertahankan apa adanya**
sebagai riwayat kenapa task ini pernah `BLOCKED` — jangan dihapus, jangan dibaca sebagai status
terkini.

| Field | Isi |
| --- | --- |
| Outcome yang diminta | Struk Pasien menampilkan breakdown finansial dan elemen verifikasi yang terlihat pada PDF referensi staging (`staging.quilvian-mmchospital.com`) |
| Trace | Lihat cross-check keputusan lengkap di `requirement-traceability.md` § Amendment 7 September 2026 |
| Status (SAAT DITULIS, 7 September 2026) | **`BLOCKED` penuh** — keempat elemen yang diminta **tidak satu pun** punya keputusan bisnis yang mengikat penempatannya di Struk Pasien |
| Dependency (SAAT DITULIS) | `/grill-me` — pertanyaan tertutup per elemen ada di tabel di bawah |

**PDF referensi staging BUKAN bukti requirement yang locked** (per instruksi eksplisit pemilik
modul) — ia dari environment/build yang berbeda, bukan dari `00-interview-decisions.md`. Setiap
elemen dicocokkan satu per satu terhadap keputusan yang ada sebelum dianggap boleh dibangun:

| Elemen pada PDF staging | Keputusan yang dicek | Hasil cross-check | Status |
| --- | --- | --- | --- |
| "Subtotal Mandiri" / "Subtotal Penjamin" / "Pajak (11%)" / "Harus Dibayar" berjenjang | `BKC-DEC-058` acceptance criteria #26 ("Struk Pasien identik dengan **tabel Tagihan Pasien**"); `BKC-DEC-062`, `070`–`077` (formula subtotal — tapi untuk **Ringkasan Pembayaran** Menu Pembayaran, bukan Struk Pasien) | `BKC-DEC-058` **hanya** mengunci kesamaan Struk Pasien dengan tabel item baris (obat/tindakan/racikan/biaya admin) — **tidak** menyebut breakdown subtotal/pajak/harus-dibayar sebagai bagian Struk Pasien. Formula subtotal itu sendiri sudah `approved`, tetapi penempatannya di Struk Pasien belum pernah diputuskan | **BLOCKED** — bukan keputusan bisnis baru (formulanya sudah dikunci), murni keputusan *penempatan konten dokumen* yang belum ditanyakan |
| `Penjamin` (nama penjamin perusahaan/korporasi) berdampingan dengan `Asuransi` (nama provider) | `BKC-DEC-067` (Company Guarantor eksplisit **di luar scope** untuk Invoice Asuransi — dokumen **berbeda** dari Struk Pasien) | Tidak ada satu baris pun di `00-interview-decisions.md` yang menyebut Struk Pasien menampilkan field Company Guarantor. `BKC-DEC-067` bicara soal dokumen lain (Invoice Asuransi); tidak otomatis menjawab Struk Pasien | **BLOCKED** — gap requirement murni, bukan perluasan `BKC-DEC-067` |
| QR code "Scan untuk verifikasi pembayaran" | — | **Nol** kemunculan kata "QR" di seluruh `00-interview-decisions.md` pada konteks dokumen kasir manapun | **BLOCKED** — tidak pernah dibahas |
| Blok tanda tangan "Kasir" / "Penerima" | — | **Nol** kemunculan kata "tanda tangan"/"signature" terkait Struk Pasien; satu-satunya rujukan "blok tanda tangan" ada pada Invoice Asuransi (`BKC-DEC-069` amendment, ditandai `DEV_DISCRETION` untuk dokumen **itu**, bukan Struk Pasien) | **BLOCKED** — tidak pernah dibahas untuk Struk Pasien |

**Pekerjaan yang TETAP bisa berjalan sendiri, tidak menunggu keempat blocker di atas:** tidak ada.
Keempat elemen yang diminta pemilik modul semuanya blocked; tidak ada sub-bagian task ini yang
punya evidence terkunci untuk dikerjakan terpisah. Item baris (obat/tindakan/racikan/biaya admin)
pada Struk Pasien **sudah benar** sesuai `BKC-DEC-058` dan tidak perlu disentuh — dikonfirmasi
langsung dari `struk-pasien-document.jsx` saat ini (tabel Item/Layanan/Qty/Harga/Total, footer
Total tunggal).

**Rekomendasi jalan ke depan:** jalankan `/grill-me` dengan empat pertanyaan tertutup di atas
sebagai closure pass tersendiri untuk Struk Pasien. Karena field/formula yang mendasari breakdown
subtotal **sudah** ada dan terkunci di backend (`GET /{id}/calculation-preview`), menyetujui elemen
pertama secara teknis **berisiko rendah dan cepat dikerjakan** begitu keputusan penempatannya
turun — dicatat di sini supaya keputusan berikutnya tidak perlu menunggu desain arsitektur baru,
hanya keputusan bisnis "tampilkan atau tidak". Tiga elemen lain (Company Guarantor, QR, tanda
tangan) membutuhkan penggalian requirement yang lebih dalam (sumber data Company Guarantor untuk
Struk Pasien belum pernah diaudit; mekanisme verifikasi QR — verifikasi ke mana, oleh siapa — belum
pernah didesain sama sekali).

---

# Amendment 7 September 2026 (kedua) — Rumpun baru: Petty Cash (Voucher Kas Kecil), gelombang `MVP-15`

```yaml
roadmap_revision: 4
roadmap_status: DRAFT_FORWARD_TEST
pemicu: /plan-module-delivery untuk rumpun Petty Cash, blueprint revision 1.0
blueprint_revision_dibaca: 1.0 (blueprint-manifest.md, status approved; readiness DESIGN_APPROVED
  untuk rumpun Petty Cash — kontrak terkunci)
scope_pass_ini: HANYA rumpun Petty Cash. FE-BKC-018 s.d. FE-BKC-022 (rumpun-rumpun lain) TIDAK
  disentuh dan TIDAK dinilai ulang pada pass ini — termasuk FE-BKC-022 yang tetap BLOCKED penuh
  menunggu /grill-me, seperti tercatat pada bagian di atas
input_keputusan_bisnis: PC-DEC-001–015 (00-interview-decisions.md, approved 7 September 2026)
input_keputusan_arsitektur: PC-DES-001–014 (03-frontend-architecture.md § Amendment 7 September
  2026, disetujui penuh lewat PC-DEC-015)
frontend_commit_sha: 12f9242ce62e4d80dbdb719f80bb0e7a2848474c (branch QuilvianIntegrationFrontend)
contracts: [BIL-API-0.9 (Petty Cash bagian aditif dan terisolasi), BIL-PERMISSION-0.7]
task_id_series: FE-BKC-023 s.d. FE-BKC-027 — dilanjutkan dari FE-BKC-022
governance_dependency: BKC-BLK-FE-001 — dicatat resolved pada amendment 2 September 2026, tetap
  wajib diverifikasi ulang oleh builder saat mulai eksekusi (bukan diasumsikan otomatis clear)
```

## 0. Ketergantungan penuh pada backend, dan kenapa itu bukan `BLOCKED`

Tidak ada satu pun endpoint Petty Cash yang sudah ada — seluruhnya **Rencana (belum tersedia)**
pada `contracts/api-contract.md`. Kelima task di bawah membaca angka dari `backend-roadmap.md`
§ Amendment 7 September 2026 (kedua) (`BE-BKC-033`–`038`), bukan menghitungnya sendiri.

Kelima task tetap ditandai **`READY_FOR_TASK_APPROVAL`** — bukan `BLOCKED` — karena tidak ada satu
pun keputusan bisnis atau kontrak yang belum disetujui menahannya; ketergantungannya murni
**sequencing pelaksanaan**: kode frontend dapat ditulis terhadap kontrak yang sudah terkunci, tetapi
verifikasi hidupnya baru bermakna setelah task backend pasangannya selesai dan terverifikasi.
Builder **MUST** memverifikasi status task backend terkait sebelum memulai verifikasi manual
ter-autentikasi — menulis source terhadap kontrak yang terkunci tidak perlu menunggu, tetapi klaim
"terverifikasi hidup" sebelum backend selesai tidak dapat dipercaya (pola yang sama seperti
peringatan `BKC-GAP-08` pada `requirement-traceability.md`).

**Aturan yang berlaku untuk seluruh task gelombang ini** (mengikuti pola yang sudah dipakai
gelombang `MVP-6`/`MVP-9`/`MVP-12`): seluruh rupiah pada kartu saldo dan kolom nominal **MUST**
berasal dari tanggapan server — layar **MUST NOT** menghitung, menjumlahkan, atau memformat ulang
angka bisnisnya sendiri (`PC-DES-005`). Warna, jarak, ikon, bentuk wadah presentasi (modal, drawer,
atau halaman terpisah untuk `FE-PC-02`/`FE-PC-03`), dan pilihan pustaka komponen adalah
`DEV_DISCRETION` — lihat tabel "Kewenangan UI" pada `03-frontend-architecture.md` § Amendment 7
September 2026 untuk daftar lengkap mana yang terkunci dan mana yang bebas.

## 🟡 `FE-BKC-023` — Monitoring Voucher Petty Cash (`FE-PC-01`) dan butir menu "Petty Cash"

| Field | Isi |
| --- | --- |
| **Status** | 🟡 **SEBAGIAN — 8 September 2026.** Route, view, hook, tiga Redux slice baru, dan butir menu sidebar selesai ditulis: kartu saldo (dua `SummaryGrid`, murni `REUSE`), saringan lengkap (pencarian/status/kategori/periode/jumlah baris), tabel dengan kolom Aksi diturunkan dari `availableActions`, keempat aksi transisi status (`Approve`/`Reject`/`Cancel`/`Disburse`) dengan `Idempotency-Key` yang dibentuk sekali per target. `npx eslint .` PASS; `npm run test:unit` PASS (445/445, tanpa regresi); `npm run build` PASS (route terkonfirmasi ada pada `.next/server/app/...`). `ATTACH_PROOF`/"+ Buat Voucher" **sengaja tidak diimplementasikan** — scope `FE-BKC-024`. **Yang MASIH menahan `✅`:** verifikasi manual ter-autentikasi (browser, akun berhak, data voucher nyata, uji retry idempotency "Uang Diberikan") **belum dilakukan** — tidak ada kredensial pada sesi ini. Bukti: [laporan](../task/report/frontend/FE-BKC-023.md) |
| Outcome | Kasir dan Kepala Kasir dapat memantau seluruh voucher, melihat saldo kas kecil berjalan, dan menjalankan aksi yang berhak langsung dari satu layar |
| Gelombang | `MVP-15` |
| Trace | `FR-BKC-045`,`047`–`053`; `FE-PC-01`; acceptance 48–51,53,55–58 (`03-frontend-architecture.md`) |
| Kontrak | `GET /petty-cash/vouchers/{filters/metadata,summary,/,{id}}`; `GET /petty-cash/budget/current`; keempat `POST` transisi status dipicu dari kolom Aksi layar ini |
| Reuse | `DataTable`/`StatusBadge`/`FilterSelect` pola master data existing; pola pendaftaran `menu-items.jsx` |
| Scope | Route `.../petty-cash/vouchers`; kartu "TOTAL PETTY CASH" (`currentBalance` beserta keterangan `reservedAmount`); saringan (pencarian, periode, status, kategori, jumlah baris); tabel dengan kolom Aksi **diturunkan dari `availableActions`**, bukan disimpulkan dari `status`; tombol "Uang Diberikan" mengirim `Idempotency-Key` yang sama saat retry jaringan; pendaftaran butir menu "Petty Cash" pada `src/utils/menu-sidebar/menu-items.jsx` (`subMenu` Billing Management) |
| Dependency | `BE-BKC-037` selesai dan terverifikasi hidup (sequencing, bukan gerbang); `BE-BKC-036` untuk kartu saldo. `BKC-BLK-FE-001` — verifikasi ulang wajib saat mulai eksekusi |
| Acceptance | Acceptance 48–51,53,55–58; `UAT-28`,`29`,`35` |
| Verifikasi | Component test kolom Aksi diturunkan dari `availableActions` (bukan dari `status` saja); test kartu saldo tidak dihitung ulang dari daftar voucher yang tampil; lint/build |
| Risiko/pemilik | Menyimpulkan kolom Aksi dari `status` alih-alih `availableActions` akan menampilkan tombol yang lalu ditolak `403` — mengabaikan aturan kepemilikan pembatalan (`PC-DEC-007`) yang hanya backend yang tahu. Owner Frontend/Billing |
| DoD | Butir menu terjangkau dari sidebar bagi yang berhak; tests/lint/build lulus; tidak ada UUID mentah dirender di layar |

## 🟡 `FE-BKC-024` — Buat Voucher dan Bukti Nota/Kasir (`FE-PC-02`, `FE-PC-03`)

| Field | Isi |
| --- | --- |
| **Status** | 🟡 **SEBAGIAN — 8 September 2026.** `CreateVoucherModal` (`FE-PC-02`) dan `AttachProofModal` (`FE-PC-03`, endpoint+modal sama untuk isi nota pertama maupun koreksi) selesai ditulis, mengisi kedua tombol yang sengaja dikecualikan `FE-BKC-023`. `voucherNumber` tidak pernah menjadi bagian form/payload (tinjauan kode). **Atas permintaan eksplisit pengguna, `npm run lint`/`test:unit`/`build` TIDAK dijalankan sesi ini** — bukti hanya tinjauan kode statis, bukan hasil tooling. **Yang MASIH menahan `✅`:** lint/test/build dan verifikasi manual (form Buat Voucher, Input Nota, Koreksi Nota, retry idempotency) belum dijalankan sama sekali — akan dilakukan pengguna sendiri. Bukti: [laporan](../task/report/frontend/FE-BKC-024.md) |
| Outcome | Petugas dapat mengajukan voucher baru dan memasukkan atau mengoreksi bukti nota pada voucher yang uangnya sudah diserahkan |
| Gelombang | `MVP-15` |
| Trace | `FR-BKC-045`,`051`; `PC-DES-008`,`012`; acceptance 53,54 |
| Kontrak | `POST /petty-cash/vouchers`; `POST /petty-cash/vouchers/{id}/proofs`; `GET /master-data/petty-cash-categories/options` |
| Reuse | Pola modal/form existing; `BaseSelectField` untuk dropdown kategori |
| Scope | `FE-PC-02` — form Nama Penerima, Kategori (dropdown kategori aktif), Nominal Voucher, Tujuan; Voucher Number kolom hanya-baca berketerangan otomatis, **tidak pernah dikirim** pada request; kategori kosong menampilkan keterangan biru dan menonaktifkan Simpan. `FE-PC-03` — isian nomor nota, dipakai juga untuk **mengoreksi** nomor nota pada voucher `Selesai` tanpa memindahkan status (`PC-DES-012`) |
| Dependency | `FE-BKC-023` (titik masuk tombol "+ Buat Voucher"/"Input Nota"); `BE-BKC-037`; `BE-BKC-035` (opsi kategori) |
| Acceptance | `UAT-28`; contoh koreksi nota pada `FR-BKC-051` |
| Verifikasi | Component test validasi field wajib (`recipientName`, `amount`>0, `purpose`); test tombol Simpan nonaktif saat kategori kosong; lint/build |
| Risiko/pemilik | Field `voucherNumber` **MUST NOT** pernah muncul sebagai isian yang dapat diketik atau dikirim pada payload — bertentangan dengan `PC-DES-008`. Owner Frontend/Billing |
| DoD | Tests/lint/build lulus; payload `POST /petty-cash/vouchers` terverifikasi tidak memuat `voucherNumber` (structural) |

## 🟡 `FE-BKC-025` — Detail Voucher (`FE-PC-04`)

| Field | Isi |
| --- | --- |
| **Status** | 🟡 **SEBAGIAN — 8 September 2026.** `VoucherDetailModal` selesai ditulis, dibuka lewat klik dua kali pada baris tabel monitoring (`FE-BKC-023`, `DataTable.onRowDoubleClick` bawaan). Menampilkan ringkasan voucher dan riwayat perintah (`GET /{id}`) sebagai tabel, label transisi status diambil dari `StatusOptions` yang sudah dimuat (`PC-DEC-013`). **Atas permintaan eksplisit pengguna, `npm run lint`/`test:unit`/`build` TIDAK dijalankan sesi ini** — bukti hanya tinjauan kode statis. **Yang MASIH menahan `✅`:** lint/test/build dan verifikasi manual (buka detail pada tiap status voucher) belum dijalankan sama sekali — akan dilakukan pengguna sendiri. Bukti: [laporan](../task/report/frontend/FE-BKC-025.md) |
| Outcome | Pengguna dapat membuka detail satu voucher beserta riwayat perintahnya — siapa mengajukan, menyetujui, menyerahkan, dan memasukkan nota |
| Gelombang | `MVP-15` |
| Trace | `FR-BKC-053` |
| Kontrak | `GET /petty-cash/vouchers/{id}` |
| Reuse | Pola halaman/panel detail existing di modul ini |
| Scope | Layar detail voucher; riwayat perintah (`BilPettyCashVoucherCommand`) sebagai tabel atau linimasa — bentuknya `DEV_DISCRETION` |
| Dependency | `FE-BKC-023` (titik masuk klik dua kali pada baris); `BE-BKC-037` |
| Acceptance | Contoh empat baris jejak pada `FR-BKC-053` |
| Verifikasi | Component test render riwayat perintah dari respons `GET /{id}`; lint/build |
| Risiko/pemilik | Riwayat perintah adalah satu-satunya jawaban "siapa menyetujui pengeluaran ini tujuh bulan lalu" — menampilkannya salah urutan atau salah pelaku merusak fungsi audit rumpun ini. Owner Frontend/Billing |
| DoD | Tests/lint/build lulus |

## 🟡 `FE-BKC-026` — Anggaran Kas Kecil (`FE-PC-05`) dan butir menu "Anggaran Kas Kecil"

| Field | Isi |
| --- | --- |
| **Status** | 🟡 **SEBAGIAN — 8 September 2026.** Layar baru selesai ditulis: tiga kartu saldo (`SummaryGrid`, `REUSE`), tombol Tambah Anggaran/Koreksi Saldo (dua modal `COMPOSE`), tabel riwayat pergerakan dengan saringan jenis+periode, butir menu "Anggaran Kas Kecil" terdaftar. `petty-cash-budget-slice.jsx` (milik `FE-BKC-023`) diperluas tiga thunk baru tanpa menyentuh export yang sudah dipakai. **Atas permintaan eksplisit pengguna, `npm run lint`/`test:unit`/`build` TIDAK dijalankan sesi ini** — bukti hanya tinjauan kode statis. **Yang MASIH menahan `✅`:** lint/test/build dan verifikasi manual (tambah anggaran, koreksi naik/turun termasuk yang ditolak `422`) belum dijalankan sama sekali — akan dilakukan pengguna sendiri. Bukti: [laporan](../task/report/frontend/FE-BKC-026.md) |
| Outcome | Finance dapat melihat saldo kas kecil, menambah dan mengoreksi anggaran beserta alasannya, dan menelusuri riwayat pergerakan baris per baris |
| Gelombang | `MVP-15` |
| Trace | `FR-BKC-054`,`056`,`057`,`059` |
| Kontrak | `GET /petty-cash/budget/current`, `GET /petty-cash/budget/movements`; `POST /petty-cash/budget/top-ups`, `POST /petty-cash/budget/adjustments` |
| Reuse | Pola kartu ringkasan dan tabel riwayat existing di modul ini |
| Scope | Tiga kartu angka (Saldo Saat Ini, Sudah Dijanjikan, Sisa yang Bebas) **diambil dari server apa adanya**, **tidak dihitung ulang** dari daftar voucher (`PC-DES-005`); tombol Tambah Anggaran dan Koreksi Saldo; tabel riwayat pergerakan dengan saringan jenis dan periode; pendaftaran butir menu "Anggaran Kas Kecil" (`subMenu` Billing Management) |
| Dependency | `BE-BKC-036` selesai dan terverifikasi hidup (sequencing, bukan gerbang) |
| Acceptance | `UAT-34`,`36`,`37` |
| Verifikasi | Component test ketiga kartu angka tidak dihitung ulang di layar; test pesan galat koreksi yang menyebut nominal yang sudah dijanjikan; lint/build |
| Risiko/pemilik | Menghitung "sisa yang bebas" sendiri di layar akan menyimpang dari server begitu ada satu voucher yang tidak ikut terkirim pada halaman yang sedang tampil. Owner Frontend/Billing |
| DoD | Butir menu terjangkau; tests/lint/build lulus |

## 🟡 `FE-BKC-027` — Kategori Petty Cash (`FE-PC-06`–`08`) dan butir menu "Kategori Petty Cash"

| Field | Isi |
| --- | --- |
| **Status** | 🟡 **SEBAGIAN — 8 September 2026.** Bentuk penuh tujuh-berkas selesai ditulis (list/detail/editor mengikuti `master-data-feature-standard.md`, memakai `hr/master-data/job-level` sebagai template struktural karena Tax Rule/Discount Policy/Room Charge Policy/Register ternyata tidak lengkap — lihat laporan § Keputusan), `categoryCode` diketik Finance saat tambah dan terkunci saat ubah, sembilan thunk memetakan sembilan endpoint `BE-BKC-035`, butir menu "Kategori Petty Cash" terdaftar. `master-data-petty-cash-category-slice.jsx` (milik `FE-BKC-023`) diperluas 8 thunk baru tanpa menyentuh export yang sudah dipakai. **Atas permintaan eksplisit pengguna, `npm run lint`/`test:unit`/`build` TIDAK dijalankan sesi ini** — bukti hanya tinjauan kode statis. **Yang MASIH menahan `✅`:** lint/test/build dan verifikasi manual (tambah, ubah, nonaktifkan, hapus kategori terpakai vs tidak terpakai) belum dijalankan sama sekali — akan dilakukan pengguna sendiri. Bukti: [laporan](../task/report/frontend/FE-BKC-027.md) |
| Outcome | Finance dapat menambah, mengubah, dan menonaktifkan kategori pengeluaran kas kecil lewat menu Master Data, tanpa memerlukan perubahan kode aplikasi |
| Gelombang | `MVP-15` |
| Trace | `FR-BKC-061`–`063` |
| Kontrak | 9 endpoint `Master Data / Petty Cash Category` |
| Reuse | `rules/frontend/master-data-feature-standard.md` — bentuk yang sama persis dengan Tax Rule, Discount Policy, dan Room Charge Policy yang sudah ada; **tidak digambar ulang** |
| Scope | Daftar dengan kartu ringkasan dan saringan (`FE-PC-06`); detail dengan tombol Kembali/Perbarui/Hapus (`FE-PC-07`); form tambah/ubah dengan `categoryCode` (wajib, unik, diketik Finance), `categoryName` (wajib), `description` (opsional), `isActive` (`FE-PC-08`); pendaftaran butir menu "Kategori Petty Cash" (`subItems` grup Master Data) |
| Dependency | `BE-BKC-035` selesai dan terverifikasi hidup (sequencing, bukan gerbang) |
| Acceptance | `UAT-39`,`40`,`41` |
| Verifikasi | Component test pesan "Kategori yang sudah dipakai voucher tidak dapat dihapus." saat percobaan hapus kategori terpakai; lint/build |
| Risiko/pemilik | Kode kategori diisi Finance sendiri (bukan sistem) — mengikuti pola `MstTaxRule.Code`, bukan pola baru. Owner Frontend/Finance |
| DoD | Butir menu terjangkau; tests/lint/build lulus |

---

# Amendment 8 September 2026 — `FE-BKC-022` dibuka kembali sebagai `READY_FOR_TASK_APPROVAL`

```yaml
roadmap_revision: 5
roadmap_status: DRAFT_FORWARD_TEST
pemicu: /plan-module-delivery menemukan BKC-DEC-093-096 (00-interview-decisions.md) sudah
  approved sejak 7 September 2026, sementara baris FE-BKC-022 di atas masih menulis BLOCKED
  menunggu /grill-me - dua berkas bergerak sendiri-sendiri tanpa saling mengabari
backend_source_diverifikasi_langsung: NewQuilvianSystemBackend (branch Yasmina, working tree
  ada perubahan tidak terkait di PettyCashVoucherService.cs dan tiga berkas test - TIDAK
  disentuh amendment ini, TIDAK relevan untuk Struk Pasien)
frontend_source_diverifikasi_langsung: QuilvianSystemFrontendDev (branch yasmina, HEAD 52d7de26e)
catatan_staleness: blueprint-manifest.md mencatat backend_commit_sha dd31bc9 dan
  frontend_commit_sha 12f9242c (branch QuilvianIntegrationFrontend) - KEDUANYA berbeda dari HEAD
  yang benar-benar diperiksa pass ini. Pass ini TIDAK mempercayai SHA yang tercatat; setiap
  klaim source di bawah dibaca ULANG langsung dari file saat ini, bukan dari catatan manifest.
  Pembaruan SHA manifest itu sendiri tetap pekerjaan /manage-module-blueprint, di luar scope
  plan-module-delivery.
```

## 0. Kenapa dibuka kembali, dan apa yang berubah dari catatan 7 September

`BKC-DEC-093`–`096` sudah **`approved`** (`00-interview-decisions.md`, amendment "Penutupan
`BKC-GAP-09`–`12`"). Tidak ada satu pun keputusan bisnis yang masih terbuka. Yang tersisa
sebelum task ini bisa dikerjakan `build-module-frontend` HANYA verifikasi kontrak — dan
verifikasi itu sudah dilakukan pass ini, langsung ke source, dengan hasil: **tidak ada
kontrak baru yang perlu ditambahkan sama sekali.**

| Elemen | Keputusan | Kontrak yang dicek | Hasil |
| --- | --- | --- | --- |
| Breakdown Subtotal Mandiri/Penjamin, Pajak Mandiri/Penjamin, Harus Dibayar | `BKC-DEC-093` | `GET /billing/invoices/{id}/calculation-preview` (`BillingInvoicesController.cs:288`) | **Sudah ada dan sudah dikonsumsi** — persis kontrak yang sama sudah dipakai `menu-pembayaran-view.jsx` untuk Ringkasan Pembayaran. Field mentahnya (`breakdown.items[].itemPrimaryAmount/itemUnresolvedAmount/...`, `breakdown.administrationFee`, `breakdown.roomCharge`, `breakdown.coverage.nonBillableResidualAmount`, dst.) dan thunk-nya (`previewBillingInvoiceCalculation`, `billing-invoice-slice.jsx`) **tidak berubah**. Tidak ada field baru, tidak ada endpoint baru |
| Field Penjamin (`BKC-DEC-094`) | `BKC-DEC-094` | `InvoiceDetailResponse.Patient` (`InvoicePatientSummaryResponse`, `BillingInvoiceDtos.cs:185-200`) — dipakai `GET /billing/invoices/{id}` (Menu Pembayaran DAN Dokumen Kasir sama-sama memanggil ini lewat `useBillingInvoiceDetail`) | **Sudah ada dan sudah dikonsumsi**, TAPI kontrak sebenarnya berbeda dari yang diasumsikan `BKC-DEC-094`. Lihat § 1 di bawah — ini koreksi bukti, bukan pembukaan ulang keputusan |
| Blok tanda tangan (`BKC-DEC-096`) | `BKC-DEC-096` | Tidak ada kontrak data — murni elemen layout cetak, `DEV_DISCRETION` per keputusan itu sendiri | Tidak relevan untuk kontrak; nama kasir diambil dari sesi pengguna yang sudah tersedia di seluruh halaman ini (`useAuth`/pola serupa yang sudah dipakai `kwitansi-document.jsx`) |
| QR verifikasi | `BKC-DEC-095` | — | **TETAP di luar scope task ini** — `BKC-DEC-095` menunda elemen ini secara eksplisit, bukan menugaskannya. Tidak ada task/sub-task untuk QR pada slice ini |

**Kesimpulan: `FE-BKC-022` adalah task FRONTEND MURNI. Tidak ada task backend baru yang
perlu direncanakan** — kedua kontrak yang dipakai (`calculation-preview` dan
`InvoicePatientSummaryResponse.GuarantorName`/`PaymentType`) sudah live di backend saat ini,
diverifikasi langsung dari source pass ini, bukan diasumsikan dari dokumen kontrak.

## 1. Koreksi bukti — premis `BKC-DEC-094` tentang field "Asuransi" tidak akurat

`BKC-DEC-094` disetujui dengan premis: field `Penjamin` (Company Guarantor) ditampilkan
**"berdampingan dengan field `Asuransi` (nama `MstInsuranceProvider`) yang sudah ada"**. Dua
bagian premis itu diperiksa langsung ke source pass ini dan **tidak sepenuhnya akurat**:

1. **Tidak ada field "Asuransi" pada Struk Pasien hari ini.** `struk-pasien-document.jsx`
   (diperiksa penuh) hanya menampilkan Invoice No., No. Kunjungan, No. Rekam Medis, Nama
   Pasien, tabel item, dan Total — nol field payer/penjamin. "Berdampingan dengan yang sudah
   ada" salah; keduanya (Asuransi maupun Penjamin) sama-sama BARU pada task ini.
2. **Backend tidak punya dua field payer yang bisa tampil bersamaan.** `TrxPatientEncounterGuarantor`
   adalah relasi **satu-ke-satu** per kunjungan (komentar model: "Sumber pembayaran
   satu-ke-satu milik encounter") dengan SATU `PaymentType` (`Cash` XOR `Insurance` XOR
   `CompanyGuarantor`) dan SATU `PaymentSourceNameSnapshot`, diekspos sebagai SATU field
   `InvoicePatientSummaryResponse.GuarantorName` — bukan dua field independen. Satu kunjungan
   tidak pernah punya provider asuransi DAN perusahaan penjamin aktif sekaligus.

**Ini TIDAK mengubah keputusan.** Maksud `BKC-DEC-094` — menampilkan nama pihak yang
menjamin pembayaran pada Struk Pasien, bukan hanya menyembunyikannya — tetap valid dan
tidak dipertanyakan ulang. Yang dikoreksi murni bentuknya: SATU baris berlabel dinamis
("Asuransi" bila `patient.paymentType === "Insurance"`, "Penjamin" bila `"CompanyGuarantor"`,
baris disembunyikan sepenuhnya bila `"Cash"`) yang membaca `patient.guarantorName` — BUKAN dua
baris tetap berdampingan yang salah satunya akan selalu kosong. Dicatat di sini sebagai
koreksi bukti (pola yang sama seperti `BKC-DEC-057` mengoreksi `BKC-DEC-054` berdasarkan bukti
legacy) — owner keputusan (Product/Domain Owner) berhak meninjau ulang bila keberatan, tetapi
tidak memblokir implementasi karena hasil akhirnya (nama penjamin terlihat pasien) sama persis
dengan yang disetujui.

## 2. Kenapa formulanya WAJIB diekstrak, bukan ditulis ulang di komponen baru

`subtotalMandiri`/`subtotalAsuransi`/`pajakMandiri`/`pajakAsuransi`/`harusDibayar` di
`menu-pembayaran-view.jsx` BUKAN penjumlahan sederhana — turunannya melewati breakdown per
komponen (`itemPrimaryAmount`/`itemUnresolvedAmount`/`itemNonBillableResidualAmount` per item,
plus `administrationFee`/`roomCharge`/`coverage`), sekitar 140 baris logika yang sudah tiga
kali diperbaiki karena versi sebelumnya menyimpang dari angka sebenarnya (lihat komentar
`FE-BKC-016`, `FE-BKC-019`, `FE-BKC-020` pada file yang sama). Menulis ulang formula ini dari
nol di `struk-pasien-document.jsx` mengulang persis pola bug yang tiga kali terjadi di file
itu sendiri — dua tempat yang menghitung angka finansial yang sama dengan rumus yang ditulis
terpisah selalu berisiko diam-diam menyimpang begitu salah satu diubah tanpa mengubah yang lain.

**MUST**: turunan `subtotalMandiri`/`subtotalAsuransi`/`pajakMandiri`/`pajakAsuransi`/
`harusDibayar` (dan komponen antaranya yang relevan) diekstrak menjadi SATU fungsi murni yang
dipanggil KEDUA tempat (`menu-pembayaran-view.jsx` dan hook di belakang Struk Pasien) dari
`calculationPreview` mentah yang sama. Lokasi file/nama fungsi persisnya `DEV_DISCRETION`
(mis. `billing-calculation-breakdown.js` di folder hooks modul ini, mengikuti pola penamaan
util yang sudah ada) — yang **MUST NOT** didiskresikan adalah keberadaan DUA rumus terpisah
untuk angka yang sama.

## 3. 🟡 `FE-BKC-022` — Struk Pasien: breakdown, Penjamin, tanda tangan

| Field | Isi |
| --- | --- |
| **Status** | 🟡 **SEBAGIAN — 8 September 2026.** Source ditulis penuh (4 berkas berubah/baru), `npx eslint --quiet` dan `npx eslint` (full severity) PASS 0 error/warning pada seluruh berkas berubah, `node --import ./tests/helpers/register.mjs --test tests/unit/` PASS 445/445 (440 existing + 5 baru untuk fungsi breakdown), `npm run build` `next build` exit 0 dengan kedua route terdampak terkonfirmasi ada di output. Kriteria acceptance 2–5 terpetakan penuh ke source; kriteria 1 (breakdown identik Ringkasan Pembayaran) terpetakan ke source dan terbukti lewat unit test rumus, TAPI belum diverifikasi visual dengan invoice nyata. Butir DoD "verifikasi manual ketiga jenis payer" **belum terpenuhi** — `NOT FEASIBLE` pada sesi ini (tidak ada kredensial login tersedia untuk builder, sengaja tidak diminta lewat chat). Bukti: [FE-BKC-022](../task/report/frontend/FE-BKC-022.md) |
| Outcome | Struk Pasien menampilkan breakdown Subtotal Mandiri/Penjamin, Pajak Mandiri/Penjamin, dan Harus Dibayar (sama seperti Ringkasan Pembayaran Menu Pembayaran); baris payer tunggal berlabel "Asuransi"/"Penjamin" sesuai jenis pembayaran kunjungan; blok tanda tangan cetak "Kasir"/"Penerima". QR verifikasi TIDAK termasuk (`BKC-DEC-095`, ditunda) |
| Gelombang | Susulan gelombang Dokumen Kasir (`FE-BKC-011`/`017`/`018`), belum ditetapkan ke `MVP-*` manapun — `04-prd-to-mvp.md` belum punya Epic/FR/UAT untuk `BKC-DEC-093`–`096`; **coverage gap non-blocking**, penulisan Epic/FR/UAT adalah pekerjaan `design-business-module`, bukan roadmap ini |
| Trace | `BKC-DEC-093`, `BKC-DEC-094` (dengan koreksi bukti § 1), `BKC-DEC-096`; `BKC-DEC-095` (QR, sengaja TIDAK ditrace ke task ini — ditunda) |
| Kontrak | `GET /billing/invoices/{id}/calculation-preview` (existing, TIDAK berubah); `GET /billing/invoices/{id}` → `InvoiceDetailResponse.Patient.GuarantorName`/`PaymentType` (existing, TIDAK berubah). **Nol endpoint baru, nol field DTO baru** |
| Reuse | WAJIB reuse (bukan reimplementasi) formula breakdown `menu-pembayaran-view.jsx` — lihat § 2 (ekstraksi fungsi murni); WAJIB reuse thunk `previewBillingInvoiceCalculation` dan selector `selectBillingCalculationPreview` (`billing-invoice-slice.jsx`) apa adanya, tanpa slice/thunk baru; layout dokumen mengikuti pola `data-flat-table` yang sudah dipakai `struk-pasien-document.jsx`/`kwitansi-document.jsx` |
| Scope | (a) `use-dokumen-kasir-page.js`: dispatch `previewBillingInvoiceCalculation(invoiceId)` saat Struk Pasien dibuka (pola `useEffect` sama seperti fetch Invoice Asuransi pada hook yang sama, baris ~93-100 — hanya fetch saat tab aktif, bukan selalu), lalu tambahkan `guarantorName`, `paymentType`, dan hasil fungsi breakdown (§ 2) ke `strukDocumentProps`; (b) `struk-pasien-document.jsx`: render baris breakdown di atas footer Total yang sudah ada, baris payer tunggal berlabel dinamis (kosong sepenuhnya bila `paymentType === "Cash"`), dan blok tanda tangan dua kolom di bawah tanggal cetak; (c) file baru berisi fungsi breakdown yang diekstrak (§ 2), diimpor kedua pemakai |
| Dependency | Tidak ada — kedua kontrak backend yang dipakai sudah live saat ini (diverifikasi § 0), tidak menunggu task backend apa pun |
| Acceptance criteria | 1) Invoice dengan penjamin asuransi: Struk Pasien menampilkan Subtotal Mandiri, Subtotal Asuransi, Pajak Mandiri, Pajak Asuransi (disembunyikan bila Rp 0, sama seperti Ringkasan Pembayaran), Harus Dibayar — keempat angka **identik** dengan yang tampil di Ringkasan Pembayaran Menu Pembayaran untuk invoice yang sama pada saat yang sama. 2) Baris payer menampilkan label "Asuransi" dan `guarantorName` untuk kunjungan `PaymentType=Insurance`, label "Penjamin" untuk `PaymentType=CompanyGuarantor`, dan baris tidak muncul sama sekali untuk `PaymentType=Cash`. 3) Blok tanda tangan menampilkan nama kasir yang sedang login di kolom "Kasir" dan kolom "Penerima" kosong untuk diisi manual. 4) Tidak ada elemen QR pada dokumen ini. 5) Item baris (obat/tindakan/racikan/biaya admin) yang sudah benar sejak `BKC-DEC-058` **tidak berubah** |
| Verifikasi | Component/unit test fungsi breakdown hasil ekstraksi (§ 2) dengan kasus invoice tunai, asuransi penuh, asuransi sebagian, dan `nonBillableResidualAmount > 0` — bandingkan hasilnya sama dengan yang sudah diuji `menu-pembayaran-view.jsx` untuk input yang sama; snapshot/manual-check tampilan tiga jenis payer; lint/build; verifikasi manual ter-autentikasi (cetak PDF nyata untuk ketiga jenis payer, cocokkan angka dengan Ringkasan Pembayaran) — pola yang sama seperti yang tertunda pada `FE-BKC-011` |
| Risiko/pemilik | Risiko utama adalah pelaksana MENGABAIKAN § 2 dan menulis ulang formula breakdown secara terpisah — ini persis pola bug yang sudah tiga kali terjadi di modul ini. Reviewer **MUST** menolak PR yang menduplikasi formula breakdown alih-alih mengekstraknya. Owner Frontend/Billing |
| DoD | Ketiga jenis payer (Tunai/Asuransi/Penjamin) diverifikasi manual menghasilkan PDF dengan angka yang cocok Ringkasan Pembayaran; unit test fungsi breakdown lulus; lint/build lulus; `BKC-DEC-093`/`094`/`096` masing-masing punya baris acceptance criteria yang terverifikasi eksplisit, bukan diklaim tercakup begitu saja |

---

## Paralelisme gelombang ini

`FE-BKC-026` (Anggaran) dan `FE-BKC-027` (Kategori) tidak saling bergantung dan boleh paralel
begitu backend pasangannya siap. `FE-BKC-024` (Buat Voucher/Bukti Nota) dan `FE-BKC-025` (Detail)
sama-sama menunggu `FE-BKC-023` sebagai titik masuk, tetapi tidak saling bergantung satu sama lain.
Tidak ada task frontend Petty Cash yang mengubah rumus, status, atau perilaku bisnis milik rumpun
`billing-kasir` lainnya.

---

# Amendment 11 September 2026 — Rumpun baru: Edit Tagihan & Multi-Payer Coverage, gelombang `MVP-16`–`MVP-19`

```yaml
roadmap_revision: 5
roadmap_status: READY_FOR_TASK_APPROVAL
pemicu: /plan-module-delivery untuk rumpun Edit Tagihan & Multi-Payer, blueprint revision 1.1
blueprint_revision_dibaca: 1.1 (blueprint-manifest.md, status approved; MPY-DES-001-017 approved
  lewat MPY-DEC-012)
input_keputusan_bisnis: MPY-DEC-001-012 (seluruhnya approved 11 September 2026)
input_arsitektur_frontend: 03-frontend-architecture.md amendment 11 September 2026 — layar
  FE-MPY-01 s.d. FE-MPY-07, peta butir menu, skema fitur per layar, kewenangan UI
kemampuan_asal: CAP-40 (Reuse with adapter) — 01-existing-capability-map.md § 19
frontend_commit_sha: 0eafa76bf397a47ceb9d44a6f69006ee25f8ba51 (branch yasmina)
backend_commit_sha: d295c4d59b68d223edc597c8b165b7ef4282b49f (branch Yasmina)
contracts: [BIL-API-1.0, BIL-PERMISSION-0.8, BIL-TEST-1.0]
task_id_series: FE-BKC-028 s.d. FE-BKC-034 — dilanjutkan dari FE-BKC-027
koreksi_wajib_dibaca: Base component generik repository ini ada di
  src/components/features/base-features/, BUKAN di src/components/ui/. Dokumen sumber PDF menyebut
  lokasi yang keliru; yang berlaku hasil pembacaan langsung pada CAP-40
```

## 0. Tiga hal yang mengikat seluruh task frontend rumpun ini

1. **Angka finansial tidak pernah dihitung di peramban.** Seluruh nominal — total, porsi pasien,
   porsi penjamin, selisih perbandingan payer — berasal dari server. Frontend hanya memformat.
2. **Komponen dasar dipakai ulang, bukan dibuat baru.** Tombol, tabel, lencana status, modal
   konfirmasi, peringatan, tumpukan pesan, dan isian pilihan seluruhnya sudah ada di
   `src/components/features/base-features/`. Membuat komponen dasar baru memerlukan gerbang
   keputusan komponen tersendiri.
3. **Tombol Edit Penjamin Perusahaan MUST NOT dibuat.** Perubahan kartu penjamin pasien tetap
   lewat Data Pasien/Registrasi (`MPY-DEC-003`). Yang boleh ditampilkan hanya keterangan
   hanya-baca beserta arahan ke mana perubahan dilakukan.

**Arti tanda status** sama dengan roadmap backend: ✅ selesai dan terbukti, 🟡 sebagian,
⛔ terblokir, tanpa tanda belum dikerjakan.

## Grafik Urutan Dependency

```text
BE-BKC-047 [BE] ─┬─> FE-BKC-028 ─┬─> FE-BKC-029 ─┐
                 │                │               │
BE-BKC-048 [BE] ─┤                └─> FE-BKC-030 ─┤
                 │                                 │
BE-BKC-049 [BE] ─┘                                 │
                                                   │
BE-BKC-050 [BE] ─> FE-BKC-031 ─────────────────────┴─> FE-BKC-034

BE-BKC-042 [BE] ─> FE-BKC-032

BE-BKC-043 [BE] ─> FE-BKC-033
```

`[BE]` = task backend pada [`backend-roadmap.md`](./backend-roadmap.md), cermin baca-saja.

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | `BE-BKC-047`, `BE-BKC-048`, `BE-BKC-049` | `FE-BKC-028` |
| 1 | `BE-BKC-050` | `FE-BKC-031` |
| 1 | `BE-BKC-042` | `FE-BKC-032` |
| 1 | `BE-BKC-043` | `FE-BKC-033` |
| 2 | `FE-BKC-028` | `FE-BKC-029`, `FE-BKC-030` — boleh paralel |
| 3 | `FE-BKC-029`, `FE-BKC-030`, `FE-BKC-031` | `FE-BKC-034` |

Keempat task gelombang 1 tidak saling bergantung dan boleh dikerjakan paralel begitu backend
pasangannya siap. Yang menentukan urutan sesungguhnya adalah kesiapan backend, bukan ketergantungan
antar layar.

**Kenapa `FE-BKC-028` menunggu ketiga endpoint edit, bukan hanya satu.** Kerangka halaman memuat
toolbar ketiga mode sekaligus, dan `edit-context` mengembalikan kewenangan untuk ketiganya. Merilis
kerangka sebelum ketiga endpoint ada akan menampilkan dua tombol mode yang pasti gagal saat ditekan
— cacat yang sudah terbaca sejak perencanaan, bukan temuan saat pengujian.

## 🟡 `FE-BKC-028` — Halaman Edit Tagihan dan panel Edit Asuransi

| Field | Isi |
| --- | --- |
| Outcome | Kasir dapat membuka halaman Edit Tagihan dari Menu Pembayaran, melihat penanggung kunjungan yang berlaku beserta rincian tagihan, membandingkan payer kandidat berdampingan, lalu mengganti penanggung kunjungan |
| Gelombang | `MVP-17` — eksekusi gelombang 1 |
| Layar | `FE-MPY-01`, `FE-MPY-02` |
| Trace | `FR-BKC-068`, `FR-BKC-070`, `FR-BKC-071`, `FR-BKC-072`; `MPY-DEC-003`, `MPY-DES-015`, `MPY-DES-017` |
| Kontrak | `GET /{id}/edit-context`, `POST /{id}/payer-comparison-preview`, `PUT /{id}/payment-source` (`BIL-API-1.0`); hak akses `BillingInvoice : Read` dan `Update` |
| Reuse | `BasePayerWorkspace` beserta enam ekspor turunannya sebagai dasar panel pemilihan payer — **penambahan properti MUST bersifat opsional berbawaan** supaya satu-satunya konsumennya hari ini, langkah pembayaran pada admisi Rawat Inap, tidak berubah perilakunya; tabel tagihan diekstrak bagian presentasionalnya dari Menu Pembayaran, **bukan** disalin seluruh berkasnya |
| Scope | Route halaman baru memakai pola token privat yang sudah ada; kerangka halaman beserta toolbar tiga mode; tabel tagihan dikelompokkan per kategori; panel Edit Asuransi beserta perbandingan berdampingan; modal konfirmasi sebelum simpan; pemberitahuan jumlah baris yang penanggungnya ikut direset |
| Dependency | `BE-BKC-047` [BE], `BE-BKC-048` [BE], `BE-BKC-049` [BE] — kerangka halaman memuat ketiga tombol mode sekaligus, sehingga ketiga endpoint edit **MUST** sudah ada sebelum halamannya dirilis |
| Acceptance | Acceptance frontend nomor 60, 61, 62, 66, 67, 68 pada [`03-frontend-architecture.md`](../03-frontend-architecture.md) — termasuk kartu kedaluwarsa tampil **nonaktif beserta alasan** dan bukan disembunyikan, serta label ringkasan mengikuti jenis penanggung tanpa pernah menampilkan dua baris subtotal sekaligus |
| Verifikasi | `npm run lint:errors`; `npm run test:unit`; `npm run build`; klik-coba ter-autentikasi bila environment tersedia |
| Risiko/pemilik | Menghitung selisih perbandingan di peramban melanggar `NFR-025` dan menghasilkan angka yang berbeda dari tagihan sesungguhnya. Menambah properti wajib pada `BasePayerWorkspace` akan merusak alur admisi Rawat Inap. Owner Frontend |
| DoD | Ketiga endpoint terpakai; nol perhitungan finansial di peramban; regresi langkah pembayaran admisi Rawat Inap terbukti tidak berubah; lint, test, dan build lulus; `git status --short` dilaporkan |
| Status | 🟡 **SEBAGIAN 14 September 2026.** Kerangka `FE-MPY-01` dan panel `FE-MPY-02` selesai ditulis di branch `yasmina`, belum di-commit. `npm run lint`/`test:unit`/`build` **TIDAK dijalankan** (instruksi baku pengguna); hanya `node --check` pada berkas non-JSX (lulus). Klik-coba ter-autentikasi belum dilakukan. Ditemukan delta kontrak `BE-BKC-047` — ditutup `BE-BKC-FIX-009` 12 September 2026. **Amendment 14 September 2026:** ketiga panel (`FE-MPY-02`/`03`/`04`, sebelumnya masing-masing punya tabel mini sendiri) di-refactor menjadi satu tabel tagihan dengan kolom Status inline per mode ("single table principle"); ditemukan constraint kontrak backend yang tidak bisa diubah pada task frontend ini — `Reason` tetap wajib diisi (`BIL-VAL-062`) dan tidak ada endpoint pratinjau untuk mode Edit Status Tagihan/Edit Billing. Kriteria yang belum terpenuhi: `lint`/`test`/`build` belum dijalankan; klik-coba ter-autentikasi belum dilakukan. Laporan: [`task/report/frontend/fe-bkc-028-halaman-edit-tagihan-dan-panel-edit-asuransi.md`](../task/report/frontend/fe-bkc-028-halaman-edit-tagihan-dan-panel-edit-asuransi.md) |

## 🟡 `FE-BKC-029` — Panel Edit Status Tagihan

| Field | Isi |
| --- | --- |
| Outcome | Kasir dapat menandai penanggung tiap baris biaya sebagai Pribadi, Asuransi, atau Penjamin, lalu menyimpannya sekaligus |
| Gelombang | `MVP-18` — eksekusi gelombang 2 |
| Layar | `FE-MPY-03` |
| Trace | `FR-BKC-074`, `FR-BKC-075`, `FR-BKC-076`; `MPY-DEC-004` |
| Kontrak | `PUT /{id}/item-payer-assignments` (`BIL-API-1.0`) |
| Reuse | Isian pilihan dan lencana status dari base component; kerangka halaman dari `FE-BKC-028` |
| Scope | Panel beserta **subjudul wajib "Ubah penanggung biaya per item"**; isian penanggung per baris; pilihan yang tidak tersedia tampil **nonaktif beserta alasannya**; penanda baris yang diubah; penghitung "n item diubah"; hanya baris yang berubah yang dikirim |
| Dependency | `FE-BKC-028` — endpoint `BE-BKC-048` sudah dijamin ada lewat prasyarat kerangka halaman |
| Acceptance | Acceptance frontend nomor 63 — pada kunjungan tunai, pilihan Asuransi dan Penjamin tampil nonaktif beserta alasannya |
| Verifikasi | `npm run lint:errors`; `npm run test:unit`; `npm run build` |
| Risiko/pemilik | Menyembunyikan pilihan yang tidak tersedia alih-alih menonaktifkannya membuat kasir menyangka fiturnya rusak. Menghilangkan subjudul membuat pengguna menyangka yang diubah status hidup-matinya tagihan. Owner Frontend |
| DoD | Panel berjalan; subjudul ada; pilihan nonaktif beserta alasan terbukti; lint, test, dan build lulus |
| Status | 🟡 **SEBAGIAN 14 September 2026.** Panel selesai ditulis di branch `yasmina`, belum di-commit. `npm run lint`/`test:unit`/`build` **TIDAK dijalankan** (instruksi baku pengguna). Klik-coba ter-autentikasi belum dilakukan. **Amendment 14 September 2026:** tabel mini panel ini dihapus dan digabung ke kolom Status tabel tunggal `FE-BKC-028` ("single table principle") — subjudul terkunci "Ubah penanggung biaya per item" dipertahankan verbatim. Constraint ditemukan: `Reason` tetap wajib (`BIL-VAL-062`), tidak ada endpoint pratinjau untuk mode ini. Laporan: [`task/report/frontend/fe-bkc-029-panel-edit-status-tagihan.md`](../task/report/frontend/fe-bkc-029-panel-edit-status-tagihan.md) |

## 🟡 `FE-BKC-030` — Panel Edit Billing

| Field | Isi |
| --- | --- |
| Outcome | Kasir dapat menentukan obat mana yang benar-benar dibawa pulang pasien — seluruhnya, sebagian, atau tidak sama sekali |
| Gelombang | `MVP-18` — eksekusi gelombang 2 |
| Layar | `FE-MPY-04` |
| Trace | `FR-BKC-078`, `FR-BKC-079`, `FR-BKC-080`; `MPY-DEC-009`, `MPY-DES-011` |
| Kontrak | `PUT /{id}/drug-billing-disposition` (`BIL-API-1.0`) |
| Reuse | Kotak centang dan tombol pilihan dari base component; kerangka halaman dari `FE-BKC-028` |
| Scope | Tiga tombol pilihan; kotak centang **hanya** pada baris obat yang layak dan **hanya** pada mode sebagian; isian jumlah **selalu hanya-baca**; panel penjelas bila jenis kunjungan tidak mendukung |
| Dependency | `FE-BKC-028` — endpoint `BE-BKC-049` sudah dijamin ada lewat prasyarat kerangka halaman |
| Acceptance | Acceptance frontend nomor 64 dan 65 — tombol nonaktif beserta keterangan pada kunjungan rawat inap, dan **tombol tambah/kurang jumlah obat tidak muncul sama sekali** |
| Verifikasi | `npm run lint:errors`; `npm run test:unit`; `npm run build` |
| Risiko/pemilik | Memunculkan kontrol jumlah pada layar ini melanggar `MPY-DEC-009` dan mengundang kasir mengubah resep dari konteks penagihan. Owner Frontend |
| DoD | Panel berjalan; kontrol jumlah terbukti tidak ada; kelayakan per jenis kunjungan terbukti; lint, test, dan build lulus |
| Status | 🟡 **SEBAGIAN 14 September 2026.** Panel selesai ditulis di branch `yasmina`, belum di-commit. `npm run lint`/`test:unit`/`build` **TIDAK dijalankan** (instruksi baku pengguna). Klik-coba ter-autentikasi belum dilakukan. **Amendment 14 September 2026:** tabel mini panel ini dihapus dan digabung ke kolom Status tabel tunggal `FE-BKC-028` ("single table principle") — larangan mutlak isian jumlah tetap dipertahankan. Constraint ditemukan: `Reason` tetap wajib (`BIL-VAL-062`), tidak ada endpoint pratinjau untuk mode ini. Laporan: [`task/report/frontend/fe-bkc-030-panel-edit-billing.md`](../task/report/frontend/fe-bkc-030-panel-edit-billing.md) |

## `FE-BKC-031` — Lembar tagihan penjamin perusahaan pada Dokumen Kasir

| Field | Isi |
| --- | --- |
| Outcome | Finance dapat membuka dan mencetak lembar tagihan yang ditujukan kepada perusahaan penjamin |
| Gelombang | `MVP-19` — eksekusi gelombang 1 |
| Layar | `FE-MPY-05` |
| Trace | `FR-BKC-082`, `FR-BKC-083`, `FR-BKC-084`; `MPY-DEC-006`, `MPY-DES-013` |
| Kontrak | `GET /{id}/company-guarantor-invoice-document` (`BIL-API-1.0`); hak akses `BillingInvoice : Read` **dipakai ulang** |
| Reuse | Pola komponen lembar Invoice Asuransi yang sudah ada — presentasional murni, seluruh isinya diterima sebagai properti |
| Scope | Satu tab baru pada halaman Dokumen Kasir; komponen lembar beserta identitas perusahaan, identitas karyawan, rincian biaya, dan keterangan rute penggantian biaya; nama berkas memakai nomor tagihan |
| Dependency | `BE-BKC-050` [BE] |
| Acceptance | Lembar terbit hanya untuk kunjungan berpenjamin perusahaan; tab tidak tersedia atau menampilkan keterangan pada kunjungan tunai dan berasuransi pribadi |
| Verifikasi | `npm run lint:errors`; `npm run test:unit`; `npm run build` |
| Risiko/pemilik | Merender data penjamin perusahaan memakai komponen lembar Invoice Asuransi akan mencampur dua dokumen berbeda debitur. Owner Frontend |
| DoD | Tab berjalan; ketiga jenis kunjungan berperilaku benar; nama berkas terbukti memakai nomor tagihan; lint, test, dan build lulus |
| Status | `SOURCE_DONE_PENDING_MANUAL_VERIFICATION` — tab selesai ditulis di branch `yasmina`, belum di-commit. `npm run lint`/`test:unit`/`build` **TIDAK dijalankan** (instruksi baku pengguna); hanya `node --check` pada berkas non-JSX (lulus). Klik-coba ter-autentikasi belum dilakukan. Tab SELALU tampil (tidak disembunyikan) untuk kunjungan non-perusahaan, mengikuti pola sibling Invoice Asuransi yang sudah ada persis — memilih opsi pertama Acceptance ("tab tidak tersedia **atau** menampilkan keterangan"). Laporan: [`task/report/frontend/fe-bkc-031-lembar-tagihan-penjamin-perusahaan-dokumen-kasir.md`](../task/report/frontend/fe-bkc-031-lembar-tagihan-penjamin-perusahaan-dokumen-kasir.md) |

## `FE-BKC-032` — Master Data Rute Reimbursement Penjamin Perusahaan

| Field | Isi |
| --- | --- |
| Outcome | Admin master data dapat mengelola rute penggantian biaya tiap perusahaan penjamin lewat menu tersendiri |
| Gelombang | `MVP-16` — eksekusi gelombang 1 |
| Layar | `FE-MPY-06` |
| Trace | `FR-BKC-083`; `MPY-DEC-008` |
| Kontrak | Grup `Administrator / Master Data / Company Guarantor Reimbursement Route`, 9 endpoint (`BIL-API-1.0`); hak akses `CompanyGuarantorReimbursementRoute : *` |
| Reuse | Bentuk daftar dan formulir master data yang sudah baku di repository ini |
| Scope | Layar daftar beserta saringan dan halaman; formulir tambah/ubah; **isian asuransi mitra disembunyikan dan dikosongkan** ketika jenis rute "Menanggung sendiri" dipilih; **pendaftaran butir menu** pada berkas menu sidebar |
| Dependency | `BE-BKC-042` [BE] |
| Acceptance | Acceptance frontend nomor 69 — butir menu terdaftar dan dapat dijangkau peran yang berwenang |
| Verifikasi | `npm run lint:errors`; `npm run test:unit`; `npm run build`; butir menu terlihat pada sidebar saat masuk sebagai peran berwenang |
| Risiko/pemilik | Layar selesai tetapi butir menunya lupa didaftarkan — kejadian nyata pada modul ini sebelumnya, dan itu sebabnya pendaftaran menu menjadi acceptance criteria task ini, bukan pekerjaan terpisah. Owner Frontend |
| DoD | CRUD berjalan; butir menu terdaftar dan terjangkau; aturan sembunyikan-dan-kosongkan terbukti; lint, test, dan build lulus |
| Status | `SOURCE_DONE_PENDING_MANUAL_VERIFICATION` — fitur master data 7 berkas + 2 registrasi selesai ditulis di branch `yasmina`, belum di-commit. `npm run lint`/`test:unit`/`build` **TIDAK dijalankan** (instruksi baku pengguna); hanya `node --check` pada berkas non-JSX (lulus). Klik-coba ter-autentikasi belum dilakukan. Entitas tidak punya field kode bisnis; `companyGuarantorId` hanya ada pada form create (tidak ada di update, bukan sekadar disabled — sesuai kontrak backend); aturan sembunyikan-dan-kosongkan `insuranceProviderId` pada SELF diimplementasikan lewat `hidden` dinamis + pengosongan nilai di `handleChange` dan `buildPayload`. Select relasi memakai `select-resource-registry.js` yang sudah ada (bukan pola manual `optionMap`+dispatch dari standar bagian 8) — kemungkinan celah dokumentasi, lihat laporan. Laporan: [`task/report/frontend/fe-bkc-032-master-data-rute-reimbursement-penjamin-perusahaan.md`](../task/report/frontend/fe-bkc-032-master-data-rute-reimbursement-penjamin-perusahaan.md) |

## `FE-BKC-033` — Master Data Aturan Tanggungan Penjamin Perusahaan

| Field | Isi |
| --- | --- |
| Outcome | Admin master data dapat mengelola aturan tanggungan tiap perusahaan penjamin dengan pola yang sama seperti aturan tanggungan asuransi |
| Gelombang | `MVP-16` — eksekusi gelombang 1 |
| Layar | `FE-MPY-07` |
| Trace | `FR-BKC-066`, `FR-BKC-067`; `MPY-DEC-008` |
| Kontrak | Grup `Health Services / Master Data / Company Guarantor Coverage Rule`, 9 endpoint (`BIL-API-1.0`); hak akses `CompanyGuarantorCoverageRule : *` |
| Reuse | Bentuk layar Aturan Tanggungan Asuransi yang sudah ada — **semirip mungkin**, supaya admin tidak mempelajari pola baru |
| Scope | Layar daftar dan formulir; **isian urun biaya hanya-baca dan terisi otomatis** mengikuti persentase tanggungan karena nilainya diturunkan server; **pendaftaran butir menu** |
| Dependency | `BE-BKC-043` [BE] |
| Acceptance | Acceptance frontend nomor 69; isian urun biaya terbukti hanya-baca |
| Verifikasi | `npm run lint:errors`; `npm run test:unit`; `npm run build`; butir menu terlihat pada sidebar |
| Risiko/pemilik | Membiarkan urun biaya dapat diketik membuat admin menyangka nilainya tersimpan, padahal server menimpanya. Owner Frontend |
| DoD | CRUD berjalan; butir menu terdaftar; urun biaya hanya-baca terbukti; lint, test, dan build lulus |
| Status | `SOURCE_DONE_PENDING_MANUAL_VERIFICATION` — fitur master data 7 berkas + 2 registrasi selesai ditulis di branch `yasmina`, belum di-commit. Backend (`BE-BKC-043`) sudah selesai penuh sebelum task ini dimulai, dikonfirmasi dari source. `npm run lint`/`test:unit`/`build` **TIDAK dijalankan** (instruksi baku pengguna); hanya `node --check` pada berkas non-JSX (lulus). Klik-coba ter-autentikasi belum dilakukan. Dibangun semirip mungkin dengan layar Aturan Cakupan Asuransi (`BaseGroupedEditorView`) sesuai arahan, tetapi memperbaiki beberapa cacat yang ditemukan pada rujukannya (field mati isCovered/isExcluded tidak ditiru, patientClassId jadi select asli bukan teks bebas, ItemType TariffCategory bukan ServiceCategory) — detail lengkap di laporan. Urun Biaya dikunci hanya-baca dan diturunkan otomatis dari Persentase Tanggungan, dengan backend tetap sebagai wasit akhir. Laporan: [`task/report/frontend/fe-bkc-033-master-data-aturan-tanggungan-penjamin-perusahaan.md`](../task/report/frontend/fe-bkc-033-master-data-aturan-tanggungan-penjamin-perusahaan.md) |

## `FE-BKC-034` — Aksesibilitas, privasi, dan regresi lintas layar

| Field | Isi |
| --- | --- |
| Outcome | Seluruh layar rumpun ini terbukti dapat dipakai dengan papan ketik, tidak menampilkan data rahasia di tempat yang tidak semestinya, dan tidak merusak layar yang sudah ada |
| Gelombang | `MVP-19` — eksekusi gelombang 3 |
| Trace | `NFR-025`; `MPY-DEC-006`; acceptance frontend nomor 70 |
| Kontrak | `BIL-PERMISSION-0.8`, `BIL-TEST-1.0` |
| Reuse | Pola audit aksesibilitas dan privasi yang sudah dipakai rumpun sebelumnya di modul ini |
| Scope | Penelusuran papan ketik pada keempat layar; pemeriksaan nol UUID pada layar maupun URL; pemeriksaan nomor polis dan nomor karyawan tidak tampil di tempat yang tidak semestinya; **regresi langkah pembayaran admisi Rawat Inap** sesudah `BasePayerWorkspace` dipakai ulang; regresi Menu Pembayaran dan Dokumen Kasir |
| Dependency | `FE-BKC-029`, `FE-BKC-030`, `FE-BKC-031` |
| Acceptance | Acceptance frontend nomor 70 — langkah pembayaran admisi Rawat Inap berperilaku sama persis; nol UUID tampil di layar maupun URL |
| Verifikasi | `npm run lint:errors`; `npm run test:unit`; `npm run build`; penelusuran papan ketik manual |
| Risiko/pemilik | Pemakaian ulang komponen bersama tanpa regresi konsumen lamanya adalah cara paling umum merusak modul yang tidak sedang dikerjakan. Owner Frontend |
| DoD | Keempat layar lolos penelusuran papan ketik; regresi Rawat Inap terbukti; lint, test, dan build lulus |
| Status | `SOURCE_DONE_PENDING_MANUAL_VERIFICATION` — audit kode selesai di branch `yasmina`. **Satu regresi nyata ditemukan dan diperbaiki**: ekstraksi tabel Menu Pembayaran (`FE-BKC-028`) kehilangan kelas CSS `margin-bottom: 0` sehingga membuka jarak 96px yang tidak pernah ada sebelumnya — sudah diperbaiki lewat prop `className` opsional. Acceptance `#70` (regresi Rawat Inap) terbukti *by construction*: `base-payer-workspace.jsx` dan konsumen aslinya nol baris berubah menurut `git diff`. 9 dari 10 berkas pra-eksisting lain terbukti murni tambahan tanpa baris dihapus/diubah. Nol UUID dan pemakaian nomor polis/karyawan sudah ditinjau kode dan sesuai konteks. `npm run lint`/`test:unit`/`build` dan penelusuran papan ketik manual di browser **TIDAK/BELUM dijalankan** (instruksi baku pengguna / NOT FEASIBLE tanpa environment ter-autentikasi). Laporan: [`task/report/frontend/fe-bkc-034-aksesibilitas-privasi-dan-regresi-lintas-layar.md`](../task/report/frontend/fe-bkc-034-aksesibilitas-privasi-dan-regresi-lintas-layar.md) |

## Paralelisme dan urutan ringkas

Empat task gelombang 1 (`FE-BKC-028`, `031`, `032`, `033`) tidak saling bergantung sama sekali —
yang menentukan kapan masing-masing boleh mulai hanyalah kesiapan endpoint backend pasangannya.
Dua panel pada gelombang 2 (`029`, `030`) sama-sama menunggu kerangka halaman `FE-BKC-028`, tetapi
tidak saling bergantung dan boleh paralel.

Dua task master data (`032`, `033`) adalah **jalur tercepat menuju manfaat nyata**: begitu keduanya
selesai bersama `BE-BKC-042`/`043`, admin sudah dapat mengisi aturan tanggungan, dan perbaikan
perhitungan kunjungan berpenjamin perusahaan dari `BE-BKC-044` langsung terasa pada tagihan —
tanpa menunggu satu pun layar edit selesai.


---

# Amendment 15 September 2026 — Revisi rumpun Petty Cash: satu halaman kerja

| Field | Isi |
| --- | --- |
| Blueprint | `BIL-CASH-001`, revisi `1.2`, status **approved** |
| Masukan keputusan bisnis | `PC-DEC-016`–`PC-DEC-026`, seluruhnya `approved` 15 September 2026 |
| Masukan keputusan arsitektur | `PC-DES-015`–`PC-DES-025`, `approved`; layar dirinci `03-frontend-architecture.md` amendment 15 September 2026 |
| Kontrak yang berlaku | `BIL-API-1.1`, `BIL-STATE-1.0`, `BIL-PERMISSION-0.9` — seluruhnya `approved` |
| Frontend SHA | `1f2f2c93c9e4369db6c60246776de4c3bd52b3af` (branch `yasmina`) |
| Task | `FE-BKC-035`–`FE-BKC-038` (empat task) |

## 0. Kenapa seluruh task di sini menunggu backend

Layar Petty Cash yang berjalan hari ini memanggil `POST /vouchers/{id}/approve` dan
`/reject`, menampilkan `reservedAmount`, dan membandingkan status dengan nilai `WAITING_APPROVAL`
serta `APPROVED`. Keempat hal itu **hilang atau berganti** pada revisi backend.

Karena itu tidak ada satu pun task frontend di gelombang ini yang boleh dimulai sebelum task
backend pasangannya selesai. Ini bukan kehati-hatian berlebih: mengerjakannya lebih dulu berarti
menulis layar terhadap kontrak yang sedang berubah, lalu menulisnya ulang.

**Yang tetap dipakai ulang, bukan ditulis ulang:** komponen, hook, dan slice Petty Cash yang
sudah ada seluruhnya dipertahankan dan dipindahkan menjadi panel di dalam halaman gabungan.
Modul ini sudah punya preseden mahal dari menulis ulang logika yang sudah benar — rumus Subtotal
dan Pajak pada Menu Pembayaran menyimpang tiga kali karena ditulis ulang di tempat berbeda.

## Grafik Urutan Dependency

```text
BE-BKC-058 [BE] ─> FE-BKC-035 ─┐
                               │
BE-BKC-054 [BE] ───────────────┴─> FE-BKC-037

BE-BKC-055 [BE] ─> FE-BKC-036 ─┐
                               │
BE-BKC-057 [BE] ───────────────┴─> FE-BKC-038
```

`[BE]` = task backend pada `backend-roadmap.md`, cermin baca-saja.

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | `BE-BKC-058` | `FE-BKC-035` |
| 1 | `BE-BKC-055` | `FE-BKC-036` — boleh paralel dengan `FE-BKC-035` |
| 2 | `FE-BKC-035`, `BE-BKC-054` | `FE-BKC-037` |
| 2 | `FE-BKC-036`, `BE-BKC-057` | `FE-BKC-038` |

## 1. Pemetaan gelombang MVP ke gelombang eksekusi

| Gelombang MVP | Task | Yang dapat diverifikasi bisnis sesudahnya |
| --- | --- | --- |
| `MVP-21` (pencairan langsung) | `FE-BKC-036` | Kasir tidak lagi melihat tombol Setujui/Tolak, dan dapat mencairkan langsung dari daftar |
| `MVP-22` (uang kembali) | `FE-BKC-038` | Kasir dapat mencatat sisa uang yang dikembalikan dan membatalkan pencairan yang salah |
| `MVP-23` (halaman gabungan) | `FE-BKC-035`, `FE-BKC-037` | Monitoring dan anggaran tampil pada satu halaman; Finance mengelola periode dari halaman yang sama |

## `FE-BKC-035` — Halaman kanonik Petty Cash dan pengalihan route lama

| Field | Isi |
| --- | --- |
| Outcome | Satu halaman menampilkan ringkasan, panel anggaran, monitoring permintaan, dan riwayat pergerakan; penanda halaman lama tidak mati |
| Gelombang | `MVP-23` — eksekusi gelombang 1 |
| Trace | `FR-BKC-103`, `FR-BKC-104`, `FR-BKC-105`, `FR-BKC-106`, `FR-BKC-108`; `PC-DES-025` |
| Kontrak | `GET /budget/overview`, `GET /vouchers`, `GET /budget/movements` (`BIL-API-1.1`) |
| Layar | `FE-PC-09` (baru); `FE-PC-01` dan `FE-PC-05` menjadi panel |
| Reuse | **Wajib**: `petty-cash-vouchers-view.jsx` dan `petty-cash-budget-view.jsx` dipakai ulang sebagai panel; kedua hook dan kedua slice dipertahankan |
| Scope | Halaman baru pada route induk `/health-services/billing-management/petty-cash`; kedua route lama menjadi pengalihan permanen; sidebar kehilangan butir Anggaran Kas Kecil dan `pathname` butir Petty Cash berubah; lima kartu ringkasan dari satu panggilan |
| Dependency | `BE-BKC-058` [BE] |
| Acceptance | `UAT-63`, `UAT-64`; tabel wilayah pada `03-frontend-architecture.md` — setiap wilayah memuat sumber data, hak akses tombol, bunyi keadaan kosong dan gagal |
| Verifikasi | `npm run build` lulus; verifikasi manual di browser atas ketiga wilayah; verifikasi kedua route lama mengarah ke halaman kanonik; verifikasi kartu gagal-memuat menampilkan tanda hubung, bukan `Rp 0` |
| Kewenangan UI | Penggabungan halaman, route kanonik, sumber data per wilayah, dan urutan wilayah **dikunci**. Bentuk wadah panel, warna, jarak, ikon, dan component library `DEV_DISCRETION` |
| Risiko/pemilik | Pendaftaran butir menu **MUST** menjadi acceptance criteria task ini, bukan pekerjaan yang menganggur. Modul ini punya preseden lima halaman selesai tetapi tidak terjangkau sampai task menu tersendiri dikerjakan. Owner Frontend |
| DoD | Halaman kanonik terjangkau dari sidebar; kedua route lama mengalihkan; kelima kartu dari satu panggilan; keadaan memuat, kosong, dan gagal tertangani per wilayah; `npm run build` lulus; `git status --short` dilaporkan |
| Status | **Source selesai — belum diverifikasi.** `npm run lint:errors`/`test:unit`/`build` dan verifikasi manual browser belum dijalankan (instruksi eksplisit pengguna sesi ini); hanya `node --check` pada berkas logika non-JSX yang lulus. Detail dan temuan (termasuk `api-contract.md` yang basi untuk `GET /overview`/`/periods`): [task/report/frontend/fe-bkc-035-halaman-kanonik-petty-cash-dan-pengalihan-route-lama.md](../task/report/frontend/fe-bkc-035-halaman-kanonik-petty-cash-dan-pengalihan-route-lama.md) |

## `FE-BKC-036` — Kosakata status baru dan pembuangan aksi persetujuan

| Field | Isi |
| --- | --- |
| Outcome | Layar tidak lagi menampilkan tombol Setujui maupun Tolak, dan seluruh label status memakai kosakata baru |
| Gelombang | `MVP-21` — eksekusi gelombang 1 |
| Trace | `FR-BKC-088`, `FR-BKC-107`; `PC-DES-015`, `PC-DES-021` |
| Kontrak | `POST /vouchers/{id}/disburse`, `POST /vouchers/{id}/cancel`, `GET /vouchers` (`BIL-API-1.1`); `BIL-STATE-1.0` |
| Layar | `FE-PC-01` (panel monitoring), `FE-PC-02`, `FE-PC-04` |
| Reuse | `use-petty-cash-vouchers.js` dan `petty-cash-voucher-slice.jsx` dipertahankan; hanya aksi dan konstanta yang berubah |
| Scope | Aksi `approve`/`reject` dibuang dari hook, slice, dan komponen; konstanta status dan labelnya diperbarui menjadi `Menunggu Pencairan`, `Menunggu Bukti`, `Selesai`, `Dibatalkan (Uang Dikembalikan)`, `Ditolak (arsip)`; aksi per baris diambil dari `availableActions` server, **bukan** disimpulkan layar dari status; judul modal berubah menjadi Buat Permintaan |
| Dependency | `BE-BKC-055` [BE] |
| Acceptance | `UAT-55`, `UAT-56`, `UAT-57`; tabel Penanda status pada `03-frontend-architecture.md` |
| Verifikasi | `npm run build` lulus; verifikasi manual bahwa tombol Setujui/Tolak tidak ada di layar mana pun; verifikasi voucher warisan tampil `Menunggu Pencairan` dengan tombol Cairkan tersedia; verifikasi baris `Ditolak (arsip)` hanya menampilkan Detail |
| Kewenangan UI | Label status dan hak akses tombol **dikunci**. Bentuk penanda status dan tata letak aksi baris `DEV_DISCRETION` |
| Risiko/pemilik | Layar yang masih membandingkan status dengan `WAITING_APPROVAL` atau `APPROVED` akan diam-diam menyembunyikan tombol Cairkan — gagalnya tidak berupa error, melainkan tombol yang tidak pernah muncul. Owner Frontend |
| DoD | Tidak ada pemanggilan endpoint persetujuan tersisa; seluruh label memakai kosakata baru; aksi baris digerakkan server; `npm run build` lulus; `git status --short` dilaporkan |
| Status | **Source selesai — belum diverifikasi.** `npm run lint:errors`/`test:unit`/`build` dan verifikasi manual browser belum dijalankan (instruksi eksplisit pengguna sesi ini); `node --check` pada berkas logika non-JSX lulus. Ditemukan `GET /vouchers/summary` juga berganti bentuk response (di luar Kontrak eksplisit task ini tapi dalam wewenang tulisnya) — detail: [task/report/frontend/fe-bkc-036-kosakata-status-baru-dan-pembuangan-aksi-persetujuan.md](../task/report/frontend/fe-bkc-036-kosakata-status-baru-dan-pembuangan-aksi-persetujuan.md) |

## `FE-BKC-037` — Layar Kelola Periode Anggaran

| Field | Isi |
| --- | --- |
| Outcome | Finance dapat membuat, mengaktifkan, dan menutup periode anggaran dari halaman Petty Cash, dan melihat ke mana sisa saldonya berpindah |
| Gelombang | `MVP-23` — eksekusi gelombang 2 |
| Trace | `FR-BKC-092`, `FR-BKC-095`, `FR-BKC-096`; `PC-DES-017`, `PC-DES-018` |
| Kontrak | `GET /budget/periods`, `POST /budget/periods`, `POST /budget/periods/{id}/activate`, `POST /budget/periods/{id}/close` (`BIL-API-1.1`) |
| Layar | `FE-PC-12` (baru) |
| Reuse | Pola modal master data yang sudah dipakai `adjust-budget-modal.jsx` dan `top-up-budget-modal.jsx` |
| Scope | Layar anak berisi daftar periode beserta status dan aksinya; modal buat periode; dialog tutup periode yang **MUST** menampilkan sisa saldo dan meminta periode penerus ketika sisanya lebih besar dari nol, disertai kalimat bahwa sisa itu dipindahkan — bukan hilang |
| Dependency | `FE-BKC-035` (halaman induknya harus sudah ada), `BE-BKC-054` [BE] |
| Acceptance | `UAT-58`, `UAT-59`, `UAT-60`; tabel wilayah `FE-PC-12` pada `03-frontend-architecture.md` |
| Verifikasi | `npm run build` lulus; verifikasi manual alur buat lalu aktifkan lalu tutup; verifikasi pesan `BIL-VAL-102`, `103`, dan `104` ditampilkan apa adanya dari server; verifikasi dialog tutup menampilkan sisa saldo dan tujuan pemindahannya |
| Kewenangan UI | Isi dialog tutup periode dan sumber datanya **dikunci**. Bentuk wadah (modal, laci, halaman anak) `DEV_DISCRETION` |
| Risiko/pemilik | Dialog tutup tanpa kalimat pemindahan membuat Finance tidak punya cara tahu ke mana uangnya pergi. Owner Frontend bersama Finance |
| DoD | Ketiga aksi daur hidup berjalan dari layar; dialog tutup menampilkan sisa dan periode penerus; pesan penolakan server tampil apa adanya; `npm run build` lulus; `git status --short` dilaporkan |
| Status | **Source selesai — belum diverifikasi.** `npm run lint:errors`/`test:unit`/`build` dan verifikasi manual browser belum dijalankan (instruksi eksplisit pengguna sesi ini); `node --check` pada berkas logika non-JSX lulus. Ditemukan ketiga endpoint periode TIDAK menerima header Idempotency-Key (beda dari pola Petty Cash lain) — detail: [task/report/frontend/fe-bkc-037-layar-kelola-periode-anggaran.md](../task/report/frontend/fe-bkc-037-layar-kelola-periode-anggaran.md) |

## `FE-BKC-038` — Kembalikan sisa uang dan batalkan pencairan

| Field | Isi |
| --- | --- |
| Outcome | Kasir dapat mencatat sisa uang yang dikembalikan penerima dan membatalkan pencairan yang salah, langsung dari baris permintaan |
| Gelombang | `MVP-22` — eksekusi gelombang 2 |
| Trace | `FR-BKC-098`, `FR-BKC-099`, `FR-BKC-100`, `FR-BKC-102`; `PC-DES-020` |
| Kontrak | `POST /vouchers/{id}/returns`, `POST /vouchers/{id}/reversals` (`BIL-API-1.1`); `BIL-VAL-098`–`BIL-VAL-101` |
| Layar | `FE-PC-10`, `FE-PC-11` (keduanya baru); `FE-PC-04` diperbarui |
| Reuse | Pola modal beralasan wajib yang sudah dipakai `attach-proof-modal.jsx`; kunci idempotensi pada aksi finansial yang sudah ada |
| Scope | Dua modal beserta aksinya pada hook dan slice; detail voucher diperbarui menampilkan saldo sebelum/sesudah dan riwayat pengembalian; sisa yang masih di tangan penerima ditampilkan sebagai batas nominal pengembalian |
| Dependency | `FE-BKC-036` (kosakata status dan aksi baris dari server sudah benar), `BE-BKC-057` [BE] |
| Acceptance | `UAT-61`, `UAT-62` |
| Verifikasi | `npm run build` lulus; verifikasi manual pengembalian bertahap dua kali; verifikasi pengembalian melebihi sisa ditolak dengan pesan server; verifikasi pembalikan kedua ditolak; verifikasi baris `Dibatalkan (Uang Dikembalikan)` hanya menampilkan Detail |
| Kewenangan UI | Alasan wajib pada kedua aksi dan penampilan sisa di tangan penerima **dikunci**. Bentuk wadah (modal atau laci) `DEV_DISCRETION` |
| Risiko/pemilik | Keduanya memindahkan uang. Tombol **MUST** dinonaktifkan selama pengiriman **dan** memakai kunci idempotensi — keduanya, bukan salah satu. Owner Frontend |
| DoD | Kedua aksi berjalan dari baris permintaan; batas nominal pengembalian terbaca pengguna sebelum mengirim; pengiriman ganda tidak menambah saldo dua kali; `npm run build` lulus; `git status --short` dilaporkan |
| Status | **Source selesai — belum diverifikasi.** `npm run lint:errors`/`test:unit`/`build` dan verifikasi manual browser belum dijalankan (instruksi eksplisit pengguna sesi ini); `node --check` pada berkas logika non-JSX lulus. Ditemukan `PettyCashVoucherCommandResponse` tidak mengekspos nominal per kejadian Return, dan label movement type `RETURN`/`REVERSAL`/`CARRY_FORWARD_OUT`/`CARRY_FORWARD_IN` terlewat sejak `FE-BKC-037` (sudah dilengkapi) — detail: [task/report/frontend/fe-bkc-038-kembalikan-sisa-uang-dan-batalkan-pencairan.md](../task/report/frontend/fe-bkc-038-kembalikan-sisa-uang-dan-batalkan-pencairan.md) |

## Paralelisme dan urutan ringkas

`FE-BKC-035` dan `FE-BKC-036` boleh paralel: yang pertama menyusun halaman gabungan, yang kedua
membereskan kosakata dan aksi di dalam panel monitoring. Keduanya menyentuh berkas yang berbeda,
tetapi **MUST** dikoordinasikan pada satu titik — `petty-cash-vouchers-view.jsx` disentuh
keduanya. Sepakati urutan commit sebelum mulai.

Dua task gelombang 2 masing-masing menempel pada satu task gelombang 1, sehingga tidak ada
titik sempit di roadmap ini. Yang menentukan kecepatan seluruhnya adalah backend.

---

# Amendment 18 September 2026 — Penutupan gap `FINAL`→`CLOSED`, gelombang `MVP-24`

`roadmap_revision: 3` · status `DRAFT_FORWARD_TEST` · blueprint revisi `1.3` **approved** · frontend SHA `1f2f2c93c9e4369db6c60246776de4c3bd52b3af` · masukan: `BKC-DEC-100`–`105`, `BKC-DES-028`–`035` (seluruhnya `approved`).

## Satu task, dan sengaja bukan task fitur

Amendment backend revisi `1.3` **tidak menuntut satu pun perubahan source frontend**, dan itu kesimpulan berbukti, bukan perkiraan: status `CLOSED` sudah terdaftar pada opsi filter maupun peta badge di `billing-invoice-constants.js` (`01-existing-capability-map.md` § 21). Badge dan filter akan bekerja apa adanya begitu backend mulai mengirim nilai itu.

Yang tetap dibutuhkan adalah **verifikasi**, karena satu hal berubah diam-diam: nilai yang selama ini praktis tidak pernah muncul kini menjadi keadaan normal. Kode yang menangani `CLOSED` dengan benar di atas kertas belum tentu pernah benar-benar dijalani.

## `FE-BKC-039` — Verifikasi penanganan status `Closed` yang kini benar-benar muncul

| Field | Isi |
| --- | --- |
| Outcome | Terbukti — lewat pembacaan source, bukan asumsi — bahwa layar menangani tagihan yang **baru** berpindah ke `Closed` sama benarnya dengan tagihan `Final`, termasuk saat tagihan itu **kembali** ke `Final` |
| Gelombang | `MVP-24` |
| Trace | `BKC-DES-029`, `BKC-DES-031`; `03-frontend-architecture.md` amendment 18 September 2026 |
| Kontrak | `BIL-API-1.2` — **nol perubahan bentuk**, dua perubahan nilai: `status` kini dapat bernilai `CLOSED` pada alur normal, dan `closedAt` kini benar-benar terisi |
| Reuse | Seluruhnya. `BILLING_INVOICE_STATUS_OPTIONS` dan `BILLING_INVOICE_STATUS_BADGE_CONFIG` sudah memuat `CLOSED` |
| Scope | **Pembacaan source, bukan penulisan fitur.** Tiga hal yang diperiksa: (1) setiap pemakaian `isFinal` atau pemeriksaan status setara yang **mengunci** aksi penyuntingan memperlakukan `CLOSED` sama seperti `FINAL`; (2) layar yang menampilkan waktu penutupan tidak mengandaikan nilainya selalu kosong; (3) tagihan yang kembali dari `CLOSED` ke `FINAL` muncul lagi pada daftar yang masih punya sisa. **Perbaikan source hanya ditulis bila ditemukan gap** — dan bila ditemukan, gap itu dilaporkan sebagai temuan, bukan disenyapkan |
| Dependency | `BE-BKC-061` — butuh tagihan `CLOSED` yang lahir normal untuk diverifikasi, bukan yang sudah `CLOSED` sejak dulu |
| Acceptance | `UAT-69` (aksi sunting tetap terkunci pada tagihan `Closed`), `UAT-68` (tagihan yang kembali ke `Final` muncul lagi sebagai punya sisa) |
| Verifikasi | Pembacaan source `isFinal` dan seluruh turunannya; `npm run lint` dan `npm run build` **hanya bila ada source yang benar-benar diubah**; verifikasi manual pada layar daftar tagihan, Menu Pembayaran, dan Riwayat Pembayaran |
| Risiko/pemilik | `MODULE-STATUS.md` mencatat gap `isFinal`/`CLOSED` pernah ada dan **sudah** diperbaiki 30 Agustus 2026. Perbaikan itu lahir ketika `CLOSED` praktis tidak pernah muncul, sehingga cakupannya belum pernah teruji terhadap tagihan yang baru berpindah. **Jangan menganggap catatan "sudah diperbaiki" sebagai bukti** — itu persis jenis asumsi yang melahirkan gap ini. Owner Frontend |
| DoD | Ketiga butir pemeriksaan dilaporkan satu per satu beserta berkas dan barisnya; bila nol gap ditemukan, laporan menyebut **apa yang diperiksa**, bukan sekadar "tidak ada masalah"; bila ada gap, perbaikannya masuk laporan yang sama; `git status --short` dilaporkan |
| Status | ✅ **SELESAI 18 September 2026 — nol gap ditemukan, nol source diubah.** Ketiga butir diverifikasi lewat pembacaan source dengan sitasi baris persis: (1) `isFinal`/`isFinalOrClosed` di `billing-invoices-view.jsx:94` dan `menu-pembayaran-view.jsx:180` sudah menyamakan `CLOSED` dengan `FINAL`; `ledgerMutable` yang sengaja TIDAK mengecualikan `FINAL` diverifikasi cocok dengan gerbang backend (write-off `PATIENT_AR` boleh diajukan atas invoice `FINAL`). (2) `BilInvoice.ClosedAt` ternyata **tidak ditampilkan di layar manapun** untuk domain Billing — pencarian menyeluruh `closedAt` hanya menemukan domain Inpatient/Cashier Shift/Nutrition, jadi tidak ada yang "berasumsi kosong". (3) Daftar tagihan fetch-per-view tanpa cache status sisi klien — tagihan yang kembali `CLOSED`→`FINAL` otomatis benar tanpa kode khusus. **Verifikasi manual sungguhan (klik tombol pada invoice `CLOSED` nyata) tertahan** — backend belum lulus build. Bukti: [laporan](../task/report/frontend/fe-bkc-039-verifikasi-penanganan-status-closed.md) |

## Kewenangan UI

Nihil yang baru. Amendment ini tidak menambah menu, route, tab, modal, maupun kontrol, sehingga tidak ada ruang `DEV_DISCRETION` yang dibuka. Kosakata label (`Open`, `Final`, `Closed`, `Settled by Write-off`) **MUST** tetap memakai peta label yang sudah ada — **MUST NOT** diterjemahkan ulang menjadi "Lunas" di satu layar saja.

Badge `Lunas`/`Cicilan` pada Riwayat Pembayaran dan Kwitansi **bukan** status invoice dan **tidak disentuh** amendment ini. Keduanya menjawab pertanyaan yang berbeda: badge `Lunas` menjawab "berapa yang sudah dibayar", status `Closed` menjawab "apakah tagihan ini masih berjalan".

---

# Gelombang `MVP-27` — Layar pemeriksaan surat ke modul konsumen

| Field | Nilai |
| --- | --- |
| Blueprint | `BIL-CASH-001` revisi `1.4` · status `approved` |
| Masukan | `BKC-DEC-108`, `BKC-DEC-109` — `approved` 21 September 2026 |
| Contract version berlaku | `BIL-API-1.3`, `BIL-PERMISSION-1.1` — keduanya `approved` |
| Frontend SHA | `1b138b9aac7a50524fd751a47c9a76e0a55f8803` |

## Grafik Urutan Dependency

```text
[BE] BE-BKC-069 ─> 🟡 FE-BKC-040
```

Legenda: `[BE]` adalah cermin baca-saja milik `backend-roadmap.md`. Task itu dihitung dan
dijadwalkan di roadmap backend, bukan di sini.

| Gelombang eksekusi | Task | Dapat berjalan paralel? |
| --- | --- | --- |
| 1 | 🟡 `FE-BKC-040` | Tunggal pada gelombang ini |

Jumlah pasangan prasyarat→task: **satu**, sama persis dengan isi kolom `Dependency` di bawah.

## Task

### 🟡 `FE-BKC-040` — Layar Surat ke Modul Konsumen

| Field | Isi |
| --- | --- |
| Outcome | Petugas berwenang dapat melihat surat yang belum diambil Finance maupun Farmasi, dan mencatat pengakuan penerimaan bila diperlukan pemulihan |
| Jejak | `BKC-DEC-108`, `BKC-DEC-109`; skema layar `BIL-SCR-41` pada `03-frontend-architecture.md` |
| Contract | `BIL-API-1.3` — `GET /consumer-handoffs/pending` dan `PATCH /{id}/acknowledge` |
| Kemampuan existing yang dipakai | Pola daftar bersaring, komponen tabel, penanganan keadaan memuat/kosong/gagal yang sudah berjalan di modul ini |
| Cakupan yang diharapkan | Satu layar, satu butir menu tingkat dua di bawah induk yang sudah ada, tanpa layar anak |
| Dependency | `[BE] BE-BKC-069` |
| Acceptance criteria | Daftar menampilkan jenis, modul tujuan, waktu terbit, dan rujukan tagihan; penyaring jenis dan rentang tanggal bekerja; tombol akui **disembunyikan** bagi peran tak berwenang, bukan sekadar dinonaktifkan; keadaan kosong berbunyi sebagai kabar baik, bukan kegagalan; tombol akui terkunci sampai jawaban kembali |
| Bukti verifikasi | Verifikasi terhadap kontrak `BIL-API-1.3`; verifikasi hak akses dengan akun non-superadmin untuk kedua peran; verifikasi keadaan memuat, kosong, gagal, dan pengiriman ganda; bukti mengikuti kebijakan test frontend yang berlaku di repository ini |
| Risiko | Keadaan kosong yang berbunyi seperti kegagalan akan membuat petugas menyangka layarnya rusak, padahal tidak adanya surat menggantung justru keadaan yang diinginkan |
| Pemilik | Frontend + Billing |
| Definition of Done | Layar terjangkau dari butir menu; kedua endpoint terpakai sesuai kontrak; peran tak berwenang tidak melihat tombol akui; nol tombol menerbitkan maupun menghapus surat |
| Status | 🟡 **SEBAGIAN 22 September 2026.** Seluruh source (route, view, hook, constants, Redux slice, item menu) ditemukan sudah lengkap dan sesuai `BIL-SCR-41` — nol gap pada acceptance criteria maupun DoD. Satu perbaikan kualitas kode (pola `setState`-dalam-`useEffect`) diterapkan pada hook. `npx eslint` pada berkas fitur PASS 0 error/warning; `npm run lint:errors` (repo penuh) PASS untuk fitur ini (4 error pre-existing tidak terkait pada domain lain); `npm run test:unit` PASS untuk fitur ini (9 gagal pre-existing tidak terkait pada domain `FE-RWI-*`/`accounting-reconciliation`); `npm run build` PASS, exit code 0. **Yang MASIH menahan `✅`:** verifikasi manual ter-autentikasi (klik tombol Akui, dua peran berbeda, konflik `409`) — `NOT FEASIBLE`, menunggu migration backend `AddBillCollectionPrescriptionHandoff` (`BE-BKC-062`/`067`) diterapkan dan akun uji dua peran. Bukti: [laporan](../task/report/frontend/FE-BKC-040.md) |

## Kewenangan UI

| Hal | Kewenangan |
| --- | --- |
| Keberadaan layar, isi wilayah, sumber data per bagian, hak akses tiap tombol | Terkunci `03-frontend-architecture.md` |
| Bunyi keadaan kosong dan gagal | Terkunci — keduanya menyangkut kejelasan bagi petugas |
| Urutan butir menu, penamaan tampilan, warna, jarak, ikon, component library | **`DEV_DISCRETION`** |

## Yang sengaja tidak dibuat

Tombol menerbitkan ulang surat dan tombol menghapus surat **MUST NOT** dibuat. Penerbitan hanya
terjadi di dalam transaksi peristiwa finansial, dan baris surat bersifat tetap (`BKC-DEC-109`).
