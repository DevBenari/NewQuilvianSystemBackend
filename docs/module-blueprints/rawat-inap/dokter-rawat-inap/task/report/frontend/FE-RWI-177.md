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

## Kelanjutan dan validasi - 6 Oktober 2026

Pemilik meminta melanjutkan seluruh roadmap sampai selesai di branch HamzahV2. Penetapan branch sebelumnya tetap berlaku. Dependency FE-RWI-196, lingkungan UAT dan delta backend FE-RWI-174 sedang dikonfirmasi sambil bagian frontend independen dilanjutkan.

- Pemulihan command unit Windows: `test:unit` memakai pola file `tests/unit/*.test.mjs`, yang telah terbukti didukung Node 24.13.0. Tidak menambah framework/dependency.
- Pemulihan lint: compatibility config dibatasi ke pola file yang sama dengan plugin Next existing, sehingga aturan tidak diterapkan ke file yang tidak memuat plugin.
- Perbaikan import dependency build: tiga service memakai `InstanceAxios`; tiga pemakai selector memakai path dan signature curried existing; import stylesheet transfer memakai berkas existing; panel alat/diet membaca penugasan dokter episode dari service existing, menyaring interval aktif dan memakai DoctorId.
- AUTOMATED TEST: `npm.cmd run test:unit` - FAIL: 2167/2177 PASS, 10 FAIL. Dua assertion source perlu mengikuti signature selector yang telah diperbaiki, delapan kegagalan lainnya masih pada scope menu/setting/monitoring. Tidak melonggarkan permission atau acceptance.
- `npm.cmd run lint` sedang berjalan. AUTOMATED TEST: `npm.cmd run build` - FAIL: kini hanya satu module-not-found, import `@/lib/state/slice/auth-slice` pada laci Pasca Operasi FE-RWI-196 yang masih menunggu konfirmasi pemilik. Sembilan error import lain telah teratasi.
- Browser in-app tidak tersedia: bootstrap berhasil tetapi pemilihan gagal dan daftar browser kosong. Playwright Chromium existing terpasang; pemeriksaan UI dengan respons terkontrol akan dibedakan secara tegas dari UAT backend nyata.

Status belum dinaikkan menjadi selesai dari hasil unit saja.

### Hasil lint dan test dependency - 6 Oktober 2026

AUTOMATED TEST: `npm.cmd run lint` - FAIL: 1 error, 914 warning. Kegagalan konfigurasi plugin sudah teratasi; satu error source existing sedang diperiksa.

AUTOMATED TEST: sembilan file unit terkait dokter dan import dependency - PASS: 44/44. Dua assertion dependency mengikuti signature selector existing yang benar tanpa mengurangi pemeriksaan resource/action.

Build masih FAIL pada satu import laci FE-RWI-196. Pemeriksaan UI terkontrol mulai dijalankan; belum ada hasil PASS yang diklaim.

### Perbaikan hook dependency

Satu error lint penuh berasal dari useMemo di ward-pre-op-drawer.jsx sesudah early return. Early return dipindahkan sesudah seluruh hook tanpa mengubah tampilan/alur klinis. AUTOMATED TEST: eslint --quiet pada laci, requester hook, utility opsi dokter dan test UI - PASS. Command lint penuh lint:errors sedang dijalankan ulang.

### Guard dokter aktif dan pemeriksaan UI awal

AUTOMATED TEST: `node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-ancillary-order.test.mjs` - PASS: 2/2. Penugasan selesai/mendatang/rusak dan ID penugasan tanpa DoctorId tidak menghasilkan opsi peminta; dokter aktif tidak digandakan.

AUTOMATED TEST: pemeriksaan Playwright UI awal - INTERRUPTED setelah satu timeout pada locator test: navigasi existing berperan radio, sedangkan test awal mencari button. Locator test telah disesuaikan ke radio; layar dokter dan Penunjang berhasil dimuat. Tidak mengubah role komponen aplikasi untuk meluluskan test. Pemeriksaan ulang sedang berjalan.

### Pemeriksaan browser terkontrol PASS

AUTOMATED TEST: `npx.cmd --no-install playwright test tests/e2e/inpatient-doctor-finishing.spec.mjs --workers=1 --reporter=line` - PASS: 3/3. Layar Next.js lokal dengan respons API terkontrol membuktikan Rehab placeholder tanpa request/pesanan; Lab tanpa tarif tetap dapat dikirim dan double-click hanya satu POST dengan ProcedureId/EncounterId benar; katalog gagal tidak menampilkan contoh dan retry memulihkan data. Chromium existing dipakai setelah Browser in-app tidak tersedia. Ini bukan bukti integrasi backend/worklist atau UAT klinis nyata.

Cakupan diperluas untuk tarif dari resolver, kegagalan resolver, katalog kosong, label harga resep dan isolasi/error 403/409 verifikasi; hasil perluasan masih menunggu.

### Lint penuh dan perluasan browser - 6 Oktober 2026

AUTOMATED TEST: `npm.cmd run lint:errors` - PASS (exit 0). Konfigurasi dan error conditional useMemo telah diperbaiki, tanpa menonaktifkan rules-of-hooks.

AUTOMATED TEST: `npm.cmd run test:unit` - FAIL: 2171/2179 PASS, 8 FAIL pada accounting-reconciliation (1), hemodialysis-sidebar-navigation (2), inpatient-monitoring (1), inpatient-setting (2), menu-permission-filter (1), petty-cash-finance-separation (1). Kegagalan import InstanceAxios/selector pada dependency sudah teratasi. Tidak mengubah source menu/setting/monitoring yang sedang dikerjakan pihak lain untuk menyamarkan kegagalan.

AUTOMATED TEST: perluasan Playwright - FAIL: 5/9 PASS. Lima skenario Lab/Rehab lulus; empat kegagalan terkait locator teks kosong, testId katalog resep, dan pemuatan permission pada fixture halaman verifikasi sedang ditelusuri. Belum diklaim sebagai kegagalan/pemenuhan runtime backend.

### Browser terkontrol sembilan skenario PASS

AUTOMATED TEST: Playwright inpatient-doctor-finishing.spec.mjs - PASS: 9/9. Rehab tanpa pesanan/request; Lab tanpa tarif dan double-click; katalog gagal/retry; katalog kosong; resolver gagal tanpa mengunci kirim; harga dari resolver berlabel; katalog resep berlabel/tanpa Rp 0/tanggungan tidak applicable tersembunyi; isolasi diet gagal dan verifikasi darah 403/409 dengan ExpectedVersion=12. Fixture permission sekarang memuat pasangan resource/action eksplisit seperti yang disyaratkan keputusan ketat existing. Locator teks/testId mengikuti source existing. Tidak ada aturan aplikasi yang dilonggarkan.

Hasil ini tetap terpisah dari UAT backend asli dan tidak membuka gerbang FE-RWI-172.
