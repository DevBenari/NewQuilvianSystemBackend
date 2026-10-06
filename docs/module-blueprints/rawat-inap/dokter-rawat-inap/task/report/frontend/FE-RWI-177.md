# Laporan Perubahan Frontend — `FE-RWI-177`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID / Judul | FE-RWI-177 — Harga obat di tab Resep berlabel perkiraan |
| Slice / Roadmap | D1 / frontend-roadmap-finishing.md revision 1 |
| Trace | RWI-DEC-218/219; RWI-AC-337 |
| Contract version | 0.7.0 approved; FE 11.4/11.5; prescribing-drugs existing |
| Wewenang UI | DEV_DISCRETION; label perkiraan mengikat |
| Dependency | Tidak ada |
| Klasifikasi | LIGHT — label pada tiga view resep; tanpa endpoint baru |
| Task mode / Target tulis | FRONTEND; source frontend; laporan/roadmap/traceability backend |
| Model | GPT-6 |
| Commit frontend / backend | 8740efa02601820372dabecac472de5909d17215 / 0a10899435bcd4cd54e998a060465589b6652643 |
| Branch | HamzahV2 / origin/HamzahV2 |
| Tanggal / Status | 5 Oktober 2026 / 🟡 Source tersedia; 38 test terkait PASS; build/validasi penuh dan UAT belum memenuhi DoD |

## 1. Keadaan yang ditemukan di awal

Harga obat/detail, katalog, biaya obat dan racikan serta total draft belum memakai label wajib yang seragam. Catalog memakai badge shared yang dapat membaca coverage fields tanpa IsCoverageApplicable.

## 2. Proses bisnis dari sisi pengguna

1. Dokter membuka Resep dan memilih obat.
2. Harga obat, katalog, subtotal obat/racikan dan total draft disertai perkiraan — tagihan final di kasir. Contoh: unit 25.000 tampil Rp 25.000 dengan label tersebut, sehingga pengguna tahu tagihan akhir diputuskan kasir.
3. Obat tanpa harga tetap menampilkan belum tersedia, bukan Rp 0.
4. Badge tanggungan katalog hanya tampil jika IsCoverageApplicable benar; shared price menerima coverage fields hanya ketika applicable.
5. Pengiriman resep mengikuti alur existing; endpoint, Farmasi, kasir dan primitive shared tidak diubah.

Perubahan status: NOT APPLICABLE — task hanya melabeli harga dan membatasi tampilan tanggungan.

## 3. Perubahan yang dikerjakan

| File frontend | Perubahan |
| --- | --- |
| prescription-regular-drug-form.jsx | Label di samping harga satuan |
| prescription-drug-catalog-panel.jsx | Label katalog dan guard IsCoverageApplicable |
| prescription-selected-items-card.jsx | Label biaya obat/racikan dan total draft |

Ketiganya berada di src/components/view/health-services/inpatient-management/physician-workspace/tabs/prescription. Tidak ada perubahan shared component atau backend.

UI GATE: 3 elemen — REUSE 3, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0

| Kebutuhan | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Harga detail | Form existing | regular-drug-form.jsx | REUSE | Tambah label |
| Harga/badge katalog | DoctorPrescriptionPriceSummary/CoverageBadges | Props value existing | REUSE | Batasi coverage pada view rawat inap |
| Biaya dan total | SelectedItemsCard existing | renderCost pada view existing | REUSE | Label seragam |

### Health Services / Clinical Management / Prescribing Drug

Base URL: api/v1/health-services/clinical-management/prescribing-drugs

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| GET | / | Katalog dan harga obat | PrescribingDrug : Read | Query prescribing existing | DTO prescribing existing |

API tidak diubah; tidak ada request tambahan task ini. Harga dan tanggungan tetap berasal dari respons prescribing-drugs.

## 4. Validasi dan acceptance criteria

| Kriteria | Bukti/status |
| --- | --- |
| Setiap harga berlabel perkiraan | Tiga view harga/subtotal/total diberi label |
| Tanpa harga — belum tersedia | Fallback existing dipertahankan |
| Tanggungan hanya applicable | Guard pada detail existing dan katalog ditambahkan |
| Farmasi/kasir tetap | Tidak ada perubahan di area tersebut atau shared primitive |

AUTOMATED TEST: eslint tiga view — PASS (0 error, 0 warning)

AUTOMATED TEST: SKIPPED (test baru opsional) — perubahan label/view; suite existing dijalankan dalam validasi integrasi.

MANUAL TEST: NOT FEASIBLE — browser tidak tersedia pada sesi. Build/suite penuh menunggu integrasi.

## 5. Risiko, batasan dan langkah berikutnya

| Field | Nilai |
| --- | --- |
| Risiko | Verifikasi visual harga belum dilakukan |
| UI consistency | Tidak ada CSS baru; tabel view dipasangi data-flat-table=true |
| Status Git | Tidak stage/commit/push; seluruh perubahan user dipertahankan |
| Langkah berikutnya | Validasi unit/lint/build terintegrasi lalu UAT harga resep |

## Validasi akhir yang tersedia - 5 Oktober 2026

| Pemeriksaan | Hasil dan bukti |
| --- | --- |
| AUTOMATED TEST: eslint pada 31 source task | PASS: 0 error, 8 warning effect existing pada tiga hook tindakan. Perintah memakai node ./node_modules/eslint/bin/eslint.js dengan daftar file task |
| AUTOMATED TEST: tujuh file unit terkait | PASS: 38/38 test melalui node --import ./tests/helpers/register.mjs --test (coverage-estimates, instruction-review, supporting-service-modernisasi, supporting-service-v2, physician-needs-review, nursing-procedure-and-ancillary, nursing-procedure-parity) |
| AUTOMATED TEST: suite penuh dengan daftar file eksplisit | FAIL: 2168/2177 PASS, 9 FAIL di luar task dokter. Daftar file dipasok dari rg --files tests/unit -g '*.test.mjs' |
| AUTOMATED TEST: npm.cmd run lint / lint:errors | BLOCKED: konfigurasi existing eslint.config.mjs menerapkan react-hooks/rules-of-hooks tanpa plugin pada sebagian file. Tidak diubah task ini |
| AUTOMATED TEST: npm.cmd run test:unit | BLOCKED: Node 24.13.0 pada Windows menolak directory import tests/unit (ERR_UNSUPPORTED_DIR_IMPORT); seluruh file telah dijalankan eksplisit di atas |
| AUTOMATED TEST: npm.cmd run build | FAIL: Turbopack melaporkan 10 module-not-found pada file roadmap lain; rincian di bawah |
| MANUAL TEST | NOT FEASIBLE: browser runtime tidak memiliki browser tersambung (agent.browsers.list() = []). Backend lokal aktif, pembacaan Lab worklist tanpa sesi menghasilkan HTTP 401. Akun/perawatan UAT dan sesi pengujian belum tersedia |
| UI consistency / grep | Tidak ada tombol mentah, DEFAULT_LAB_EXAMS, DEFAULT_RAD_PROCEDURES, harga 120000, admissionDoctorId peminta atau label perkiraan rusak pada baris yang ditambahkan; enam tabel tersentuh memakai data-flat-table. Tidak menambah CSS/base component |
| Review scope | git diff --check pada scope task PASS setelah whitespace dibersihkan. Hook tindakan poliklinik dan form HD tidak memiliki diff. Source backend tidak ditulis. Perubahan user/agent lain dipertahankan; tidak stage/commit/push/deploy |

Sembilan kegagalan suite yang di luar task: accounting-reconciliation (1), hemodialysis-sidebar-navigation (2), inpatient-monitoring (1), inpatient-setting (2), menu-permission-filter (1), operating-room-hardening (1), petty-cash-finance-separation (1). Ketiganya assertion task yang terdampak perubahan label, lokasi kolom dan Rehab placeholder sudah disesuaikan terhadap keputusan baru dan lulus; assertion lain tidak dilonggarkan.

Build gagal pada transfer-handover.service.js, inpatient-report.service.js dan ward-pre-op.service.js (@/lib/axios); transfer-handover-drawer.jsx, post-op-summary-drawer.jsx, ward-pre-op-drawer.jsx dan inpatient-room-transfer-report-view.jsx (@/lib/state/slice/auth-slice); nursing-equipment-section.jsx dan inpatient-diet-panel.jsx (import pendukung); transfer-handover-drawer.jsx (inpatient-nursing-workspace.module.css). File tersebut berasal dari scope roadmap lain yang sedang dikerjakan. Tidak dibuat alias palsu atau diubah pekerjaan agent lain.

Status tetap sebagian karena bukti manual dan build penuh belum memenuhi DoD. Lint terbatas/test terkait yang lulus tidak dinyatakan sebagai penyelesaian acceptance ujung ke ujung.
