# Laporan Perubahan Frontend — `FE-RWI-176`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID / Judul | FE-RWI-176 — Katalog tindakan rawat inap dengan perkiraan harga |
| Slice / Roadmap | D1 / frontend-roadmap-finishing.md revision 1 |
| Trace | FR-RWF-070/034, RWI-DEC-165/108/218/219, UAT-RWF-31, IMP-RWF-06 |
| Contract version | 0.7.0 approved; FE 11.3/11.5; API 13.2/13.5 |
| Wewenang UI | DEV_DISCRETION; katalog per careSetting/audience dan label perkiraan mengikat |
| Dependency | BE-RWI-160/163 ?; endpoint source dibaca ulang |
| Klasifikasi | HEAVY — tiga hook dan dua pemilih klinis |
| Task mode / Target tulis | FRONTEND; source frontend; laporan/roadmap/traceability backend |
| Model | GPT-6 |
| Commit frontend / backend | 8740efa02601820372dabecac472de5909d17215 / 0a10899435bcd4cd54e998a060465589b6652643 |
| Branch | HamzahV2 / origin/HamzahV2 |
| Tanggal / Status | 5 Oktober 2026 / 🟡 Source tersedia; 38 test terkait PASS; build/validasi penuh dan UAT belum memenuhi DoD |

## 1. Keadaan yang ditemukan di awal

Ketiga hook belum mengirim careSetting/audience. Panel dokter dan perawat mengisi harga nol dan Ditanggung meski master-options tidak menyediakan tarif. Panel perawat memakai file tersendiri sehingga harus ikut diperbaiki untuk memenuhi outcome perawat, bukan hanya hook.

## 2. Proses bisnis dari sisi pengguna

1. Dokter/perawat membuka Order Tindakan bangsal.
2. Katalog diminta dengan careSetting=Inpatient; audience Doctor atau Nurse. Perawat dapat melihat tindakan khusus perawat sesuai saringan server.
3. Sistem meminta coverage-status per ProcedureId pada episode aktif.
4. Harga dan tanggungan mengikuti respons; kosong tampil tarif belum tersedia, tanpa hak harga angka disembunyikan. Contoh: unit 85.000 dan jumlah 2 — perkiraan 170.000, dengan label tagihan final di kasir. Tarif yang belum tersedia dihitung sebagai pemeriksaan tanpa tarif, tidak diubah menjadi nol.
5. Dokter/perawat mengonfigurasi dan menyimpan tindakan melalui alur existing. Tarif tidak dikirim sebagai kebenaran tagihan dan tidak menahan simpan.

Status klinis pesanan tetap mengikuti alur existing; task ini tidak mengubah lifecycle tindakan.

## 3. Perubahan yang dikerjakan

| File frontend | Perubahan |
| --- | --- |
| src/lib/hooks/health-services/inpatient-management/use-inpatient-procedure-tab.jsx | Inpatient/Doctor pada opsi dan pencarian |
| src/lib/hooks/health-services/inpatient-management/use-inpatient-nursing-procedure.jsx | Inpatient/Nurse; hapus fallback Ditanggung/harga nol |
| src/lib/hooks/health-services/inpatient-management/use-inpatient-prescription-procedure.jsx | Inpatient/Doctor |
| src/lib/services/health-services/clinical-management/patient-procedure.service.js | Signal Axios dipisah dari query; pemanggil poliklinik tetap identik |
| physician-workspace/tabs/procedure/procedure-form-panel.jsx | Harga, tanggungan dan total resolver |
| nursing-workspace/sections/procedure/nursing-procedure-order-panel.jsx | Sama untuk perawat |
| inpatient-coverage-utils.js dan use-inpatient-ancillary-coverage.js | Dipakai ulang dari FE-RWI-173 |

Kedua view berada di src/components/view/health-services/inpatient-management. use-doctor-procedure.js rawat jalan tidak diubah.

UI GATE: 4 elemen — REUSE 4, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0

| Kebutuhan | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Form/pemilih | ProcedureFormPanel dan NursingProcedureOrderPanel existing | Props/catalog/form existing | REUSE | Perbaiki harga pada panel existing |
| Aksi | BaseButton | base-button.jsx | REUSE | Tombol base |
| Tanggungan | StatusBadge | status-badge.jsx | REUSE | Label server saja |
| Tabel | Tabel panel existing | data-flat-table=true dipasang | REUSE | Pertahankan struktur; typography canonical |

### Health Services / Clinical Management / Patient Procedure

Base URL: api/v1/health-services/clinical-management/patient-procedures

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| GET | /master-options | Katalog bangsal | PatientProcedure : Read | careSetting=Inpatient, audience=Doctor/Nurse, search, take | Master options existing |

### Health Services / Inpatient Management / Inpatient Ancillary Order

Base URL: api/v1/health-services/inpatient-management/episodes/{episodeId}/ancillary-orders

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| GET | /coverage-status | Perkiraan tindakan | InpatientEpisode : Read; harga PatientProcedure : Create | itemType=Procedure, itemIds berulang | CoverageStatusItem[] |

Error tanggungan hanya mengubah pesan harga; bukan status klinis atau hak simpan.

## 4. Validasi dan acceptance criteria

| Kriteria | Bukti/status |
| --- | --- |
| Katalog bangsal | Semua call tiga hook mengirim Inpatient; rawat jalan tidak diubah; runtime NOT RUN |
| Tindakan khusus perawat | audience Nurse pada opsi/pencarian perawat |
| Ditanggung hanya data server | Fallback removed; badge hanya true/false dari resolver |
| Harga kosong bukan Rp 0 | Unit nullable; NOT_ESTIMABLE — tarif belum tersedia |
| Label perkiraan | Semua unit/subtotal/total pada dua panel dilabeli perkiraan |
| Poliklinik tidak berubah | use-doctor-procedure.js dan pemanggil poliklinik tidak disentuh; regresi runtime NOT RUN |

AUTOMATED TEST: eslint enam source — PASS (0 error, 8 warning effect legacy pada dua hook)

MANUAL TEST: NOT FEASIBLE — browser tidak tersedia untuk verifikasi dokter/perawat dan regresi poliklinik. Build/suite terintegrasi menunggu.

## 5. Risiko, batasan dan langkah berikutnya

| Field | Nilai |
| --- | --- |
| Delta backend | Laporan BE-RWI-160 mencatat poliklinik perlu careSetting=Outpatient untuk UAT-RWF-31; perubahan caller tersebut dilarang scope FE-RWI-176 dan belum dilakukan |
| UI consistency | Tidak ada CSS baru; tabel diberi kontrak typography. Inline layout dan badge Utama/Cito legacy dipertahankan; tidak menjadi bukti konsistensi visual PASS |
| Status Git | Source belum di-commit; perubahan user dipertahankan |
| Langkah berikutnya | Unit/lint/build terintegrasi, lalu UAT dokter/perawat/poliklinik dan tindak lanjut caller rawat jalan oleh pemilik |

### Penyempurnaan total

Subtotal memperhitungkan isFreeOfCharge hanya jika tarif server tersedia. Tarif kosong tetap ditandai belum tersedia. Empat test murni tarif membuktikan nol sah, harga tersembunyi, jumlah pemeriksaan tanpa tarif dan FOC.

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
