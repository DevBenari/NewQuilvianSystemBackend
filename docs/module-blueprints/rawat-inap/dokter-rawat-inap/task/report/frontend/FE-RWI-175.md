# Laporan Perubahan Frontend — `FE-RWI-175`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID / Judul | FE-RWI-175 — Perlu Diverifikasi enam sumber |
| Slice / Roadmap | D1 / frontend-roadmap-finishing.md revision 1 |
| Trace | FR-RWF-037, RWI-DEC-188/191, AC-RWF-035, NFR-RWF-11, INV-RWF-23 |
| Contract version | 0.7.0 approved; API 13.3/13.4; diet kontrak Keperawatan 0.6.0 |
| Wewenang UI | DEV_DISCRETION; enam sumber independen dan permission mengikat |
| Dependency | BE-RWI-161/162/166 ?; controller/DTO dibaca ulang |
| Klasifikasi | HEAVY — enam permission, worklist dan mutasi per pemilik |
| Task mode / Target tulis | FRONTEND; source frontend; laporan/roadmap/traceability backend |
| Model | GPT-6 |
| Commit frontend / backend | 8740efa02601820372dabecac472de5909d17215 / 0a10899435bcd4cd54e998a060465589b6652643 |
| Branch | HamzahV2 / origin/HamzahV2 |
| Tanggal / Status | 5 Oktober 2026 / 🟡 Source tersedia; 38 test terkait PASS; build/validasi penuh dan UAT belum memenuhi DoD |

## 1. Keadaan yang ditemukan di awal

Worklist hanya memuat tindakan, Lab/Radiologi. Gagalnya sumber tidak ditampilkan per bagian. Lab/Radiologi tidak dapat diverifikasi langsung. Dialog memakai prop isOpen/onClose yang tidak didukung ConfirmModal sehingga tidak terbuka sesuai kontrak komponen.

## 2. Proses bisnis dari sisi pengguna

1. Dokter membuka Perlu Review. CPPT tetap tersedia dalam kategori sendiri.
2. Enam sumber instruksi diperiksa memakai permission efektif yang ketat. Sumber tanpa permission tidak dirender atau diminta.
3. Tiap sumber memuat 25 baris dengan pagination server masing-masing. Gagalnya Diet hanya menampilkan Daftar diet gagal dimuat pada bagian Diet; sumber lain tetap tampil.
4. Dokter memilih Verifikasi pada pesanan. Dialog meminta konfirmasi; endpoint pemilik menerima ID dan ExpectedVersion untuk Gizi/darah/diet. Contoh: versi server 3 dikirim expectedVersion 3; layar tidak mengarang versi.
5. Berhasil — sumber terkait dimuat ulang. 403 — Hanya dokter yang memberi instruksi yang dapat memverifikasi. 409 — Pesanan ini sudah diverifikasi dan sumber dimuat ulang.
6. Kosong — Tidak ada yang perlu diverifikasi.

| Dari | Aksi | Ke | Pelaku/syarat |
| --- | --- | --- | --- |
| Pending | Verifikasi | Verified | Dokter instruksi, permission dan versi server sah |
| Pending | 409 | Muat ulang | Status server telah berubah; tidak mengirim ulang diam-diam |

## 3. Perubahan yang dikerjakan

| File frontend | Perubahan |
| --- | --- |
| use-physician-review-worklist.js | Request/state/abort/pagination independen, permission strict, verifikasi enam sumber |
| physician-needs-review-view.jsx | Section per sumber; base summary/filter/modal; pesan dan retry lokal |
| inpatient-instruction-review-constants.js | Resource, action, endpoint dan method dari controller asli |
| inpatient-instruction-review.service.js | GET worklist dan method verifikasi sesuai pemilik |
| inpatient-instruction-review-utils.js | Normalisasi ID/versi dan pesan 403/409 |

Source berada di src/lib/hooks, src/lib/constants, src/lib/services, src/utils/health-services/inpatient-management; view di src/components/view/health-services/inpatient-management/doctor-inpatient/needs-review.

UI GATE: 6 elemen — REUSE 6, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0

| Kebutuhan | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Header | Hero | hero.jsx | REUSE | Header existing |
| Ringkasan | SummaryGrid | summary-grid.jsx, items | REUSE | Metrik server |
| Kategori | DataFilter | data-filter.jsx, tabs | REUSE | Tab bawaan |
| Data/pagination | DataTable | data-table.jsx, pageNumber/onPageChange | REUSE | Tabel per sumber |
| Error/aksi | InformationAlert/BaseButton | base-features existing | REUSE | Error/retry lokal |
| Konfirmasi | ConfirmModal | show, onCancel, onConfirm, loading | REUSE | Props canonical |

### API pemilik sumber

| Grup Swagger | Base URL |
| --- | --- |
| Health Services / Clinical Management / Patient Procedure | api/v1/health-services/clinical-management/patient-procedures |
| Health Services / Laboratory Management / Lab Order | api/v1/health-services/laboratory-management/lab-orders |
| Health Services / Radiology Management / Rad Order | api/v1/health-services/radiology-management/rad-orders |
| Health Services / Nutrition Management / Nutrition Order | api/v1/health-services/nutrition-management/orders |
| Health Services / Blood Bank Management / Blood Order | api/v1/health-services/blood-bank-management/blood-orders |
| Health Services / Nutrition Management / Patient Diet | api/v1/health-services/nutrition-management/diets |

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| GET | /instruction-verification-worklist | Pending milik dokter login | VerifyInstruction untuk Gizi/darah/diet; Read untuk tindakan/Lab/Rad | pageNumber, pageSize | PagedResult DTO pemilik |
| PATCH | /{id}/verify-instruction tindakan | Verifikasi tindakan | PatientProcedure : Verify | Tanpa body bisnis | PatientProcedureResponse |
| PUT | /{id}/verify-instruction Lab/Rad | Verifikasi penunjang | LabOrder/RadOrder : Verify | Tanpa body bisnis | DTO order pemilik |
| POST | /{id}/verify-instruction Gizi/darah/diet | Verifikasi dengan versi | Resource : VerifyInstruction | ExpectedVersion | DTO order/diet pemilik |

403 berarti dokter tidak berwenang; 409 berarti pesanan telah berubah/diverifikasi. Permission tampilan mengikuti hak verifikasi walaupun GET lama masih memakai Read.

## 4. Validasi dan acceptance criteria

| Kriteria | Bukti/status |
| --- | --- |
| Milik dokter A, tidak tampil pada B | Worklist server menentukan akun login; runtime NOT RUN |
| Audit verifikasi darah | POST memakai ID/ExpectedVersion, audit diturunkan backend; runtime NOT RUN |
| Diet gagal, lainnya tetap tampil | Request/state/error per sumber independen |
| 403 | Pesan khusus tersedia |
| 409 | Pesan khusus dan muat ulang sumber tersedia |
| Tanpa permission | Strict decide allowed saja; tidak meminta/merender source denied/unknown |

AUTOMATED TEST: eslint lima source — PASS (0 error; warning effect awal ditangani dengan load async sebelum integrasi akhir)

MANUAL TEST: NOT FEASIBLE — browser tidak tersedia; dua akun dokter dan satu akun perawat belum dapat diuji interaktif. Build/suite penuh menunggu integrasi.

## 5. Risiko, batasan dan langkah berikutnya

| Field | Nilai |
| --- | --- |
| Delta backend | Worklist Gizi/darah belum mengirim EpisodeId. Tombol Buka Pasien disembunyikan pada baris itu agar tidak membuat tautan kosong; verifikasi langsung tetap tersedia |
| UI consistency | Tidak ada CSS baru; inline warna/typography pada view yang disentuh dicabut |
| Status Git | Tidak stage/commit/push; perubahan user dipertahankan |
| Langkah berikutnya | Uji multiakun dan validasi terintegrasi |

### Penyempurnaan verifikasi

physician-needs-review-table-columns.jsx memisahkan definisi kolom. Tombol verifikasi gizi/darah/diet hanya tersedia ketika Version integer dari server tersedia; service juga menolak payload tanpa versi. Tiga test murni baru membuktikan penjagaan versi, tidak mengarang EpisodeId dari EncounterId, dan pesan 403/409. Penanda Tepat Waktu hanya dipakai pada CPPT yang mempunyai aturan batas waktu.

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
