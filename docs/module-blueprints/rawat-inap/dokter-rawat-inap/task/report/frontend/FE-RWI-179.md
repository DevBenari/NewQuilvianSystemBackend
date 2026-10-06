# Laporan Perubahan Frontend — `FE-RWI-179`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | FE-RWI-179 |
| Judul | Rehab Medik di tab Penunjang dokter |
| Slice / Roadmap | D3 / frontend-roadmap-finishing.md revision 1 |
| Trace / Contract version | FR-RWF-035, RWI-DEC-108/222, RWI-AC-341/342; kontrak 0.7.0 approved; PRD 23.8 |
| Wewenang UI | Placeholder existing, DEV_DISCRETION dalam batas keputusan 222 |
| Dependency | Tidak ada; keputusan placeholder disetujui |
| Klasifikasi | MEDIUM — pencabutan satu form dan seluruh request Rehab |
| Task mode / Target tulis | FRONTEND; frontend source, backend laporan/roadmap/traceability saja |
| Model | GPT-6 |
| Commit frontend saat dikerjakan | 8740efa02601820372dabecac472de5909d17215, HamzahV2 / origin/HamzahV2 |
| Commit backend yang dijadikan rujukan | 0a10899435bcd4cd54e998a060465589b6652643 |
| Tanggal | 5 Oktober 2026 |
| Status | 🟡 Source dan test selesai; manual/build terintegrasi menunggu |

## 1. Keadaan yang ditemukan di awal

Form Rehab memakai ID tindakan buatan dan mengirim pesanan tindakan. Hook juga membaca riwayat tindakan untuk kartu Rehab. Hal ini bertentangan dengan keputusan RWI-DEC-222.

## 2. Proses bisnis dari sisi pengguna

1. Dokter memilih kartu Rehab Medik.
2. Panel existing menampilkan konteks pasien dan Integrasi belum tersedia.
3. Tidak ada form, tombol kirim atau pembacaan/pembuatan pesanan Rehab.
4. Dokter dapat kembali ke enam kartu. Hemodialisa tetap memakai integrasi existing.

Perubahan status pesanan: NOT APPLICABLE, kartu tidak membuat pesanan. Endpoint: NOT APPLICABLE, seluruh request Rehab dicabut.

## 3. Perubahan yang dikerjakan

| File frontend | Perubahan |
| --- | --- |
| supporting-service-tab.jsx | Render SupportingUnavailablePanel untuk Rehab; form dan riwayat Rehab dicabut |
| use-inpatient-supporting-service.jsx | Cabut effect riwayat, state, token dan mutation Rehab |
| inpatient-supporting-service-constants.jsx | Rehab isAvailable false; badge Integrasi belum tersedia |
| supporting-rehab-form.jsx | Dihapus; seluruh ID buatan ikut dicabut |
| tests/unit/inpatient-supporting-service-modernisasi.test.mjs | Ekspektasi lama diselaraskan dengan keputusan 222 |

Semua source berada di src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service, src/lib/hooks/health-services/inpatient-management, dan src/lib/constants/health-services/inpatient-management.

UI GATE: 1 elemen — REUSE 1, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Kartu tidak tersedia | SupportingUnavailablePanel | supporting-unavailable-panel.jsx; serviceKey, episode, onBack existing | REUSE | Gunakan panel existing tanpa perubahan base |

## 4. Validasi dan acceptance criteria

| Kriteria | Bukti/status |
| --- | --- |
| Nol ID tindakan buatan | Form dihapus |
| Nol request Rehab | Hook tidak memuat orderRehab/rehabToken/getPatientProceduresByEpisode; test PASS |
| Hemodialisa tidak berubah | Form dan service HD tidak disentuh |

AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-supporting-service-modernisasi.test.mjs — PASS (7/7)

AUTOMATED TEST: eslint tiga source — PASS, 0 error; satu warning pada pemuatan katalog yang sedang diperbaiki FE-RWI-173.

MANUAL TEST: NOT FEASIBLE — browser tidak tersedia pada sesi; No browser is available, daftar browser kosong.

Build terintegrasi menunggu. Tidak ada CSS baru; panel reuse tetap menggunakan token/typography existing.

## 5. Risiko, batasan dan langkah berikutnya

| Field | Nilai |
| --- | --- |
| Risiko | Verifikasi interaktif kartu belum dijalankan |
| Dependency backend | NOT APPLICABLE; tidak memanggil modul Rehab |
| Status Git | Perubahan belum di-commit; seluruh perubahan user dipertahankan |
| Langkah berikutnya | Lint, suite dan build penuh setelah seluruh task terintegrasi |

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
