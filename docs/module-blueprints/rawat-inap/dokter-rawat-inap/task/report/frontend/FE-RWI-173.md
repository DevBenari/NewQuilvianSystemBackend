# Laporan Perubahan Frontend — `FE-RWI-173`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-173` |
| Judul | Form Lab dan Radiologi dokter tanpa harga tetap dan tanpa katalog contoh |
| Slice | D1 — Penunjang Medis |
| Roadmap | `roadmap/frontend-roadmap-finishing.md`, revision 1 |
| Trace | FR-RWF-033/034; RWI-DEC-108/218/219; RWI-AC-335/337; IMP-RWF-05 |
| Contract version | `0.7.0`, approved RWI-DEC-221; arsitektur frontend bagian 11 dan API bagian 13 |
| Wewenang UI | DEV_DISCRETION; label perkiraan mengikat; komponen dan token existing |
| Dependency | BE-RWI-163 ?; source controller dan DTO dicocokkan ulang |
| Klasifikasi | HEAVY — dua form, pemuatan katalog, resolver tarif dan pemisahan request |
| Task mode | FRONTEND |
| Target tulis | Source QuilvianSystemFrontendDev; laporan dan bukti roadmap/traceability backend |
| Model | GPT-6 |
| Commit frontend saat dikerjakan | `8740efa02601820372dabecac472de5909d17215`, HamzahV2 / origin/HamzahV2 ditetapkan pengguna 5 Oktober 2026 |
| Commit backend yang dijadikan rujukan | `0a10899435bcd4cd54e998a060465589b6652643`; working tree existing dipertahankan |
| Tanggal | 5 Oktober 2026 |
| Status | 🟡 Implementasi tersedia; verifikasi manual dan build terintegrasi masih menunggu |

## 1. Keadaan yang ditemukan di awal

Dua form memiliki katalog contoh dan harga tetap; Radiologi juga memakai ID modalitas buatan. Katalog Lab/Radiologi dimuat bersama sehingga kegagalan satu sumber menghilangkan sumber lain. Respons mutasi `false` diabaikan sehingga keranjang dapat terhapus walaupun pesanan gagal.

Hash SHA-256 arsitektur frontend, API contract dan acceptance matrix cocok persis dengan manifest approval 2 Oktober 2026. Snapshot HEAD sudah berubah; controller dan DTO relevan dibaca ulang sebagai bukti runtime as-is. Tidak ada perubahan kontrak target.

## 2. Proses bisnis dari sisi pengguna

1. Dokter membuka Penunjang Medis lalu Lab atau Radiologi.
2. Sistem memuat katalog asli. Katalog kosong menampilkan keadaan kosong; kegagalan menawarkan Coba lagi.
3. Dokter mencari pemeriksaan dan menambahkannya ke keranjang.
4. Sistem membaca tanggungan dan perkiraan harga. Contoh: tarif server 85.000 tampil Rp 85.000 dengan label perkiraan — tagihan final di kasir. Tarif tidak ada tampil tarif belum tersedia. Tanpa hak harga, angka disembunyikan.
5. Dokter mengirim pemeriksaan. Kegagalan tanggungan tidak menahan pesanan. Pemeriksaan berhasil dikeluarkan dari keranjang; pemeriksaan gagal tetap tersedia. Kunci submit mencegah klik ganda.

| Dari | Tindakan | Ke | Pelaku/syarat |
| --- | --- | --- | --- |
| Dipilih | Kirim berhasil | Pesanan tercatat | Dokter berwenang; validasi server |
| Dipilih | Kirim gagal | Tetap di keranjang | Pesan gagal tampil; dapat mencoba ulang |

## 3. Perubahan yang dikerjakan

### 3.1 File

Semua jalur berikut berada di frontend:

| File | Perubahan |
| --- | --- |
| `src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/supporting-laboratory-form.jsx` | Katalog nyata, tarif resolver, pengiriman defensif |
| `src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/supporting-radiology-form.jsx` | Katalog dan modalitas nyata, tarif resolver |
| `src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/supporting-service-tab.jsx` | State katalog dan retry per sumber |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-supporting-service.jsx` | Pemuatan katalog terpisah dan retry |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-ancillary-coverage.js` | Resolver dengan abort dan isolasi episode |
| `src/lib/hooks/health-services/inpatient-management/use-ancillary-diagnosis-search.js` | Pencarian ICD dipindahkan dari view ke hook; request lama dibatalkan |
| `src/lib/services/health-services/inpatient-management/inpatient-ancillary-order.service.js` | Request Axios bersama untuk adapter dan tanggungan |
| `src/utils/health-services/inpatient-management/inpatient-coverage-utils.js` | Normalisasi dan pemetaan harga, total perkiraan |
| `tests/unit/inpatient-coverage-estimates.test.mjs` | Tiga pengujian logika tarif |

### 3.2 Keputusan base component

UI GATE: 5 elemen — REUSE 5, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Pencarian katalog | DataFilter | `src/components/features/base-features/data-filter.jsx`, props search/reset | REUSE | Pakai pencarian bawaan |
| Field form | BaseInputField/BaseNativeSelectField/BaseTextAreaField | `base-form-control.jsx`; form Penunjang existing | REUSE | Props existing |
| Aksi | BaseButton | `base-button.jsx`; size sm, variant primary/secondary | REUSE | Tombol base |
| Error/retry | InformationAlert | `information-alert.jsx`, message/children | REUSE | Pesan dan tombol Coba lagi |
| Status | ClinicalStatusBadge/ClinicalSafetyAlert | `components/ui/doctor-clinical-base` existing | REUSE | Pertahankan referensi ruang kerja dokter |

### 3.3 Health Services / Inpatient Management / Inpatient Ancillary Order

Base URL: `api/v1/health-services/inpatient-management/episodes/{episodeId}/ancillary-orders`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| GET | `/coverage-status` | Tanggungan dan perkiraan per pemeriksaan | InpatientEpisode : Read; harga mengikuti Create per modul | ItemType, ItemIds berulang | ApiResponse<List<CoverageStatusItem>> |

Laboratory/Radiology memakai ProcedureId. `NOT_PERMITTED` menyembunyikan harga; `NOT_ESTIMABLE` menampilkan tarif belum tersedia; error resolver tidak memblokir Kirim. Endpoint katalog dan POST pesanan existing dipakai ulang, tanpa perubahan backend.

## 4. Validasi dan acceptance criteria

| Kriteria | Bukti/status |
| --- | --- |
| Nol katalog contoh/harga tetap | PASS — rg pada dua form tidak menemukan DEFAULT_LAB_EXAMS, DEFAULT_RAD_PROCEDURES, 120000, 250000 atau mod-xray |
| Katalog kosong | Implementasi empty state tersedia; MANUAL NOT RUN |
| Katalog gagal dan retry | State error/denied dan Coba lagi tersedia; MANUAL NOT RUN |
| BPJS kelas 2 dan perkiraan | Mapping label/harga server; tiga test PASS; skenario pasien MANUAL NOT RUN |
| Tarif kosong tidak memblokir | Test harga kosong PASS; disabled Kirim mengikuti keranjang/submitting saja |
| Resolver gagal tidak memblokir | Test pesan error PASS; error tidak termasuk guard submit |

AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/inpatient-coverage-estimates.test.mjs — PASS (3/3)

AUTOMATED TEST: eslint pada sembilan source terkait — PASS, 0 error; 2 warning set-state-in-effect dicatat untuk perbaikan sebelum validasi akhir.

MANUAL TEST: NOT FEASIBLE — browser runtime tidak memiliki browser tersedia (`No browser is available`; daftar browser kosong). Backend workflow dan akun dokter/perawat belum dapat diakses interaktif. Tidak ada klaim UAT PASS.

Build penuh, npm run lint dan suite unit penuh akan dicatat setelah seluruh slice terintegrasi. Typography/inline style legacy yang tidak terkait harga tidak direka ulang. Tidak ada CSS baru atau perubahan base component.

## 5. Risiko, batasan dan langkah berikutnya

| Field | Nilai |
| --- | --- |
| Risiko | Katalog existing mengambil halaman pertama; UAT dengan data nyata belum terbukti |
| Backend | Read-only; controller/DTO resolver tersedia, dependency ditandai selesai oleh pemilik |
| Git | Perubahan user sebelumnya dipertahankan; tidak stage/commit/push |
| Langkah berikutnya | Integrasikan task berikutnya, jalankan lint/suite/build penuh, lalu verifikasi interaktif bila runtime tersedia |

### Penyempurnaan form

Form memeriksa permission Create modul masing-masing. Label katalog dipisahkan menggunakan delimiter server; modalitas harus masih berasal dari katalog saat dikirim. Saat form ditutup atau pasien berganti, loop keranjang tidak meneruskan item berikutnya dan hasil request lama tidak mengubah pesan pasien baru. Permintaan yang sudah dikirim tetap menuju episode asalnya.

Delta existing yang belum menjadi cakupan task ini: beberapa isian Lab (prioritas, diagnosis dan catatan) tidak dimuat DTO CreateLabOrderRequest. Helper hasil legacy inpatient-supporting-service-utils.jsx juga masih mempunyai fallback hasil contoh, yang tidak disentuh task katalog ini. Temuan dicatat untuk pemilik; tidak dianggap sebagai bukti hasil klinis atau UAT.

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
