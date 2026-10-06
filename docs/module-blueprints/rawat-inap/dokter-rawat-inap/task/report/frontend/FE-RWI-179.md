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
