# Laporan Perubahan Frontend — `FE-RWI-174`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID / Judul | FE-RWI-174 — Konsultasi Gizi dan Bank Darah lewat adapter Rawat Inap |
| Slice / Roadmap | D1 / frontend-roadmap-finishing.md revision 1 |
| Trace | FR-RWF-031?038; RWI-DEC-171/188/219; IMP-RWF-01/02 |
| Contract version | 0.7.0 approved, API 13.2, FE 11.1/11.3 |
| Wewenang UI | DEV_DISCRETION; form bersama kedua ruang kerja dan master nyata mengikat |
| Dependency | BE-RWI-164 ?; DTO/controller adapter dibaca ulang |
| Klasifikasi | HEAVY — adapter, dua peran, master komponen dan duplicate confirmation |
| Task mode / Target tulis | FRONTEND; frontend source; backend laporan/roadmap/traceability saja |
| Model | GPT-6 |
| Commit frontend / backend | 8740efa02601820372dabecac472de5909d17215 / 0a10899435bcd4cd54e998a060465589b6652643 |
| Branch frontend | HamzahV2 / origin/HamzahV2, ditetapkan pengguna 5 Oktober 2026 |
| Tanggal / Status | 5 Oktober 2026 / 🟡 Source tersedia; 38 test terkait PASS; build/validasi penuh dan UAT belum memenuhi DoD |

## 1. Keadaan yang ditemukan di awal

Form mengirim langsung ke Gizi/Bank Darah, memakai dokter admisi sebagai peminta. Komponen darah dan unit layanan mempunyai GUID tertanam. Menu perawat masih placeholder. Baris prioritas/waktu darah lama tidak ada dalam DTO adapter dan tidak dipertahankan sebagai janji alur yang tidak disimpan server.

## 2. Proses bisnis dari sisi pengguna

1. Dokter atau perawat membuka Gizi/Bank Darah.
2. Penginput tampil dari akun login. Server mengganti peminta dengan dokter login pada pesanan dokter.
3. Perawat memilih dokter dari penugasan episode yang masih berlaku pada waktu sekarang. Penugasan kosong/gagal menampilkan pesan dan retry.
4. Gizi menerima prioritas dan alasan rujukan. Bank Darah menerima golongan diminta, komponen master, jumlah kantong dan catatan klinis. Contoh: perawat memesan 2 PRC atas instruksi dokter jaga; server menetapkan Pending.
5. Permintaan dikirim ke adapter Rawat Inap. Unit/pasien/encounter tidak dikirim dari form; server menurunkannya dari episode.
6. Bank Darah mendeteksi pesanan mirip — dialog wajib alasan — endpoint confirm-duplicate dengan kunci permintaan yang sama. Tombol terkunci saat mengirim.
7. Daftar dimuat ulang setelah berhasil. Data gagal tetap di form untuk mencoba ulang.

| Dari | Aksi | Ke | Pelaku/syarat |
| --- | --- | --- | --- |
| Form | Kirim dokter | NotRequired | Dokter berpenugasan; server menentukan |
| Form | Kirim perawat | Pending | Dokter instruksi dipilih dan divalidasi server |
| Form | VAL-BD-001 | Konfirmasi mirip | Alasan wajib; tidak mengirim ulang otomatis |

## 3. Perubahan yang dikerjakan

| File frontend | Perubahan |
| --- | --- |
| supporting-nutrition-form.jsx | Peminta akun/pilihan penugasan; penginput dan tarif belum tersedia |
| supporting-blood-bank-form.jsx | Master nyata, beberapa komponen, golongan enum, konfirmasi mirip |
| use-inpatient-supporting-service.jsx | Adapter; idempotency retry; lock; isolasi encounter; pemuatan per sumber |
| use-ancillary-order-requester.js | Akun Redux dan penugasan aktif episode |
| use-ancillary-blood-components.js | Master komponen, loading/empty/error/retry |
| inpatient-ancillary-order.service.js | GET penugasan/master; POST adapter dan confirm |
| nursing-ancillary-section.jsx | Form yang sama dipasang untuk Gizi/darah; kunci Lab/Rad tetap |
| supporting-history-section.jsx | Status pesanan/verifikasi, peminta, kolom penginput; tanpa nama karangan |

Jalur source: src/components/view/health-services/inpatient-management (workspace/tabs atau nursing-workspace/sections), src/lib/hooks/health-services/inpatient-management dan src/lib/services/health-services/inpatient-management. Hemodialisa tidak diubah.

UI GATE: 5 elemen — REUSE 5, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0

| Kebutuhan | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Field/pilihan | BaseInputField/BaseNativeSelectField/BaseTextAreaField | base-form-control.jsx props existing | REUSE | Field base |
| Aksi | BaseButton | base-button.jsx | REUSE | Primary/secondary |
| Pesan dan retry | InformationAlert | information-alert.jsx, message/children | REUSE | Error lokal |
| Konfirmasi mirip | ConfirmModal | confirm-modal.jsx; show, requireReason, onConfirm | REUSE | Alasan wajib |
| Daftar | SupportingHistorySection/ClinicalDataTable | section existing | REUSE | Kolom audit ditambahkan dalam view |

### Health Services / Inpatient Management / Inpatient Ancillary Order

Base URL: api/v1/health-services/inpatient-management/episodes/{episodeId}/ancillary-orders

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| POST | /nutrition-consultations | Pesan gizi | NutritionOrder : Create | RequesterDoctorId?, Priority, ReasonForReferral, IdempotencyKey | GziOrderDetailResponse |
| POST | /blood-orders | Pesan darah | BloodOrder : Create | RequestingDoctorId?, RequestedBloodGroup?, Lines, ClinicalNote?, IdempotencyKey | BloodOrderDetailDto |
| POST | /blood-orders/confirm-duplicate | Pesanan mirip beralasan | BloodOrder : Create | Request sebelumnya + DuplicateOverrideReason | BloodOrderDetailDto |
| GET | /coverage-status | Tarif komponen darah | InpatientEpisode : Read | Blood, ItemIds komponen master | CoverageStatusItem[] |

400 dokter wajib dipilih; 403 dokter tidak berpenugasan; 422 VAL-BD-001 membuka konfirmasi. Nilai field audit berasal dari server.

### Health Services / Master Data / Blood Component

Base URL: api/v1/health-services/master-data/blood-components

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| GET | /options | Komponen aktif | BloodComponent : Read | onlyActive=true | BloodComponentOptionResponse[] |

### Health Services / Inpatient Management / Inpatient Episode

Base URL: api/v1/health-services/inpatient-management/episodes

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| GET | /{id}/doctor-assignments | Penugasan dokter | InpatientEpisode : Read | Episode ID | InpatientDoctorAssignmentResponse[] |

## 4. Validasi dan acceptance criteria

| Kriteria | Bukti/status |
| --- | --- |
| Dokter peminta akun login, NotRequired | Adapter memakai actor; form mengirim null; runtime NOT RUN |
| Perawat 2 PRC, Pending/penginput perawat | Form/master/pilihan dokter tersedia; runtime NOT RUN |
| Dokter tanpa penugasan | Opsi dibatasi interval; pesan backend diteruskan; runtime NOT RUN |
| Nol admissionDoctorId/GUID tertanam | Kedua form tidak memakai admissionDoctorId, GUID komponen atau unit |
| Pesanan mirip | VAL-BD-001 dan ConfirmModal requireReason; runtime NOT RUN |
| Tarif belum tersedia | Darah lewat resolver; Gizi sesuai aturan tetap kontrak, lihat delta |
| Menu perawat terpasang | Form bersama dipasang; hanya Rehab tetap unavailable |
| Hemodialisa | Form/service HD tidak disentuh |

AUTOMATED TEST: eslint delapan source terkait — PASS (0 error, 0 warning)

MANUAL TEST: NOT FEASIBLE — browser tidak tersedia; tidak ada browser tersambung untuk akun dokter/perawat dan worklist asli. Suite unit dan build penuh dicatat setelah integrasi.

## 5. Delta backend, risiko dan langkah berikutnya

| Temuan | Dampak/penanganan |
| --- | --- |
| Coverage Nutrition wajib ItemIds, tetapi kontrak/form konsultasi tidak menyediakan ID item gizi | Tidak mengarang ID atau memakai episode sebagai ID layanan. Gizi menampilkan tarif belum tersedia sesuai aturan tetap Nutrition NOT_ESTIMABLE; panggilan resolver Nutrition menunggu kontrak ID sah |
| GziOrderSummaryResponse/BloodOrderListDto tidak menyediakan nama penginput | Kolom memakai nama jika tersedia, selain itu Nama penginput belum tersedia. Tidak mengubah GUID menjadi nama dokter/perawat secara tebakan |
| UAT belum dijalankan | Status tetap sebagian; tidak mengklaim outcome lintas modul sudah terbukti |

| Field | Nilai |
| --- | --- |
| UI consistency | Komponen base dan CSS existing; tidak menambah CSS/global typography |
| Status Git | Perubahan user dipertahankan; tidak stage/commit/push |
| Langkah berikutnya | Validasi terintegrasi dan penyelesaian field audit/kontrak Nutrition oleh pemilik backend |

### Penyempurnaan integrasi

- Kunci retry memasukkan EpisodeId dan payload; kunci lama tidak dipakai untuk pasien lain.
- Alasan dialog pesanan mirip diterima dari argumen kedua ConfirmModal; kegagalan konfirmasi tampil di dalam dialog.
- Golongan diminta harus dipilih eksplisit. Unknown (0) dan NotDisclosed (99) mengikuti enum server; tidak lagi diam-diam mengirim null.
- Daftar Gizi memakai PatientId yang didukung server, membaca seluruh halaman pasien lalu menyaring EncounterId; bukan mengirim filter EncounterId yang diabaikan backend.
- Daftar darah membaca Components dari DTO list. Kolom status verifikasi memanfaatkan helper enum yang sama dengan tindakan. Tidak mengasumsikan jumlah kantong atau crossmatch dari field kosong.
- supporting-history-table-columns.jsx memisahkan definisi kolom dari section. Respons mutation yang selesai setelah form ditutup tidak mengubah form pasien baru.

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
