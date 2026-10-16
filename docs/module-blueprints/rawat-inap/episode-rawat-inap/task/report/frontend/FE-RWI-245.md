# Laporan Perubahan Frontend - FE-RWI-245

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | FE-RWI-245 |
| Judul | Alur bayi pasien baru dan pasien lama |
| Slice | PLAN-REPAIR-EPS-016, BAYI-S01 sampai S09 |
| Roadmap | [frontend-roadmap-bayi-016.md](../../../roadmap/frontend-roadmap-bayi-016.md) |
| Trace / contract version | RWI-BAYI-016-1.0, disetujui pengguna 14 Oktober 2026; FIX-EPS-016-02/05/06/07/08 dan preferensi 09 |
| Wewenang UI | Skema sembilan tahap yang disetujui; berlaku pada entry new/existing, komposisi base existing; tidak merombak primitive base |
| Dependency | BE-RWI-231 source/build selesai; migration belum diterapkan |
| Klasifikasi | HEAVY, 10: repository source 0, diperiksa 2, diubah 2, bisnis 2, kontrak 1, database 0, keamanan 1, workflow 2 |
| Task mode | FRONTEND; laporan/traceability pada blueprint backend sesuai tata kelola |
| Target tulis | QuilvianSystemFrontendDev, HamzahV2 / origin/HamzahV2; dokumen task pada blueprint ini |
| Model | GPT-6 / Codex sesuai sesi; tanpa agent tambahan |
| Commit frontend | 88cb19f0c8edd998fb7a19701f0f52ac9aab839b |
| Commit backend rujukan | 9f8d82666e8d67dde203d64ffab640305efc90ab dan source BE-RWI-231 lokal |
| Tanggal | 2026-10-14 |
| Status | ✅ SELESAI SOURCE - 2026-10-15; build/migration selanjutnya dikerjakan pemilik sesuai instruksi; runtime/settlement tidak dinyatakan selesai |

## 1. Keadaan awal

Kategori bayi sebelumnya menuju form pasien umum dan stepper umum; metadata newborn serta hubungan ibu belum dikirim dengan benar. Pasien lama berisiko tetap melewati alur umum meskipun profilnya bayi. Pemilihan bed sebelum episode belum membawa konteks pasien untuk dinilai server.

## 2. Alur pengguna yang dihasilkan

1. Pasien baru memilih kategori bayi. Pasien lama memilih RM existing; profil bayi membuka cabang bayi tanpa membuat master kedua.
2. Konteks kelahiran lokal, rujukan luar atau rawat ulang dipilih. Ibu tetap diverifikasi pada identitas, bukan dipaksa mempunyai episode aktif untuk semua bayi.
3. Pada pasien baru, simpan nama bayi, tanggal/jam lahir bila diketahui, jenis kelamin, pembeda kembar, NIK bayi opsional dan kontak orang tua/wali. Ibu yang belum mempunyai RM dapat didaftarkan melalui master pasien pada tab lain, lalu dicari lagi. Pada pasien lama, identitas tersimpan diperiksa tanpa mengubah orang dewasa menjadi bayi.
4. Tentukan rawat gabung atau neonatal, sumber keputusan klinis, kebutuhan intensif dan DPJP bayi. DPJP tidak disalin dari ibu. Rawat gabung harus menunjuk episode ibu yang masih hadir.
5. Penjamin dipilih untuk bayi sendiri. Preferensi dua invoice ditagihkan/dibayar bersama disimpan sebagai handoff kasir; tidak menghapus invoice asal dan tidak membuat transaksi pembayaran.
6. Rencana deposit muncul sesuai kategori bayar. Tidak ada minimum Rp10 juta baru untuk bayi; nominal rencana tidak ditampilkan sebagai uang diterima.
7. Penempatan dan pemesanan digabung menjadi satu tahap dengan dua keadaan UI. Server menilai bed sejak sebelum EpisodeId; Draft baru dibuat saat melanjutkan bed terpilih, kemudian bed dipesan dengan operasi/versi existing.
8. Persetujuan hanya untuk orang tua/wali. Tinjau data tersimpan, lalu aktifkan penempatan. Tanpa consent tersimpan, aktivasi ditolak. Retry memakai operasi yang sama.
9. Dokumen akhir menampilkan gelang/label dari server dan waktu layanan aktual. Episode Admitted dapat membuka kembali dokumen tanpa aktivasi kedua. Pesan resume tidak meminta reservasi lagi pada episode yang sudah aktif.

Urutan: Konteks - Identitas - Perawatan/DPJP - Penjamin/Penagihan - Rencana Deposit kondisional - Penempatan/Pemesanan - Persetujuan Wali - Tinjau/Aktifkan - Dokumen. Deposit tersembunyi menghasilkan delapan tahap dengan nomor berurutan.

## 3. Perubahan yang dikerjakan

### 3.1 Referensi yang diperiksa

Admisi existing, base-form-control, BaseButton, BaseDetailCard, BaseCheckboxCard, ResourceFilterSelect, ConsentFormStep, AdmissionWorkspace/IdentityLabelsSection, hook operasi bed, flow/resume, service pasien/episode, util query bed/patient/encounter, serta kontrak dan implementasi backend task pasangan. Referensi visual: admisi pasien umum dan Workspace PPRI dalam module yang sama. Bentuk: workflow berlangkah existing, bukan CRUD master baru.

### 3.2 Berkas yang berubah pada scope task

Semua path berikut relatif terhadap frontend; perubahan eksternal pada working tree dipertahankan.

| Berkas | Perubahan |
| --- | --- |
| src/components/view/health-services/inpatient-management/inpatient-newborn-admission-steps.jsx | Komposisi delapan view domain untuk sembilan tahap, ringkasan, rencana deposit, aktivasi dan dokumen |
| src/lib/constants/health-services/inpatient-management/inpatient-newborn-admission-constants.js | Tahap, opsi sumber/perawatan/penagihan dan default |
| src/lib/hooks/health-services/inpatient-management/use-inpatient-newborn-admission.js | Pencarian ibu abort/pagination, pemulihan konteks, validasi dan aktivasi idempotent |
| src/utils/health-services/inpatient-management/inpatient-newborn-admission-utils.js | Payload/normalisasi dan validasi identitas/perawatan |
| src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx | Hubungkan cabang bayi pada kedua entry; pesan resume aktif; ringkasan penjamin tersimpan |
| src/components/view/health-services/inpatient-management/inpatient-admission-visit-category-step.jsx | Kategori bayi membuka konteks bayi secara eksplisit |
| src/components/view/health-services/inpatient-management/inpatient-admission-doctor-step.jsx | Props judul/tujuan berikutnya, default umum tetap |
| src/components/view/health-services/inpatient-management/inpatient-admission-payment-step.jsx | Props label berikutnya untuk urutan bayi |
| src/components/view/health-services/inpatient-management/inpatient-admission-bed-step.jsx | Label berikutnya menuju persetujuan wali |
| src/components/view/health-services/inpatient-management/inpatient-admission-consent-form-step.jsx | Pilihan signer bayi Parent/Guardian |
| src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx | Resolusi cabang bayi sembilan/delapan tahap untuk new/existing |
| src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-flow.jsx | Persistensi konteks dan pemulihan URL; bersihkan draft entry saat selesai |
| src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-patient.jsx | Simpan bayi eksplisit, validasi, retry data pendukung pada RM yang sudah dibuat dan reset |
| src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-doctor.jsx | Konteks bayi pada pembukaan episode |
| src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-resume.jsx | Pulihkan Draft; Admitted hanya untuk dokumen/konfirmasi bayi yang sesuai |
| src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-bed.jsx; use-inpatient-bed-board.jsx | Teruskan konteks pencarian dan stabilkan dependensi query |
| src/lib/services/health-services/inpatient-management/inpatient-episode.service.js | Pencarian ibu menerima filter dan AbortSignal |
| src/utils/health-services/inpatient-management/inpatient-admission-patient-utils.jsx | Mode newborn eksplisit, tanpa NIK/telepon ibu menjadi identitas bayi; default umum tetap |
| src/utils/health-services/inpatient-management/inpatient-admission-encounter-utils.jsx | Metadata newbornAdmission pada payload episode |
| src/utils/health-services/inpatient-management/inpatient-bed-utils.jsx | Query board/beds sebelum dan sesudah episode; tidak menghitung ulang kelayakan lokal |
| src/utils/health-services/inpatient-management/inpatient-episode-utils.jsx | Metadata bayi dan waktu pembuatan pada detail |
| src/utils/health-services/registration-management/emergency-management/emergency-registration.utils.js | Pertahankan flag bayi, ibu, jam dan urutan lahir saat normalisasi master |
| src/style/health-services/inpatient-management/inpatient-admission.module.css | Menutup blok signaturePlaceholderPrompt yang belum tertutup; mempertahankan style lain |
| src/style/health-services/inpatient-management/inpatient-consent-print.module.css | Scope aturan html/body cetak pada dokumen .printArea agar lolos CSS module purity |
| tests/unit/inpatient-newborn-admission.test.mjs | Uji perilaku payload, identitas, cabang bayi, rawat gabung/rujukan, query server dan preferensi billing |
| tests/unit/inpatient-bed-board.test.mjs | Uji melarang penyaringan kelayakan lokal, tetap mengizinkan kebutuhan dikirim ke server |

### 3.3 Arsitektur dan keputusan komponen

Alur route - view - hook domain - service Axios - util/constants mengikuti pola existing. Route app tidak ditambah. Tidak membuat Redux store, library form, transport HTTP, base component atau entity frontend baru.

| Elemen | Status | Komponen/pola |
| --- | --- | --- |
| Hero/stepper/aksi | REUSE | Hero, InpatientAdmissionStepper, BaseButton; link memakai as=Link |
| Pilihan sumber/perawatan/invoice | REUSE | BaseCheckboxCard |
| Isian bayi | REUSE | BaseInputField, BaseDateField, BaseTimeField, BaseSelectField, BaseCheckboxField |
| Pencarian ibu/episode | REUSE | ResourceFilterSelect; serverSide, retry, pagination, selectedOption |
| Ringkasan/status | REUSE | BaseDetailCard, InformationAlert |
| Rangkaian konteks/identitas/perawatan | COMPOSE | View domain menyusun primitive existing |
| Penempatan dua keadaan | COMPOSE | BedSelectionStep + BedBookingStep, transaksi tetap terpisah |
| Consent/dokumen | REUSE | ConsentFormStep, AdmissionWorkspaceProvider, IdentityLabelsSection dan operasi cetak existing |

Untuk komposisi konteks/identitas/perawatan:
1. **Rekomendasi/terpilih:** susun base existing sesuai skema yang disetujui; konsisten, radius regresi kecil, biaya hanya view domain.
2. Alternatif: salin form umum; biaya maintenance lebih besar dan berisiko membawa kewajiban identitas dewasa.

Untuk komposisi penempatan:
1. **Rekomendasi/terpilih:** dua keadaan dalam satu tahap dengan komponen/transaksi existing; sesuai sembilan tahap, tanpa menggandakan operasi.
2. Alternatif: dua langkah terpisah; biaya kecil tetapi tidak memenuhi skema yang disetujui.

UI GATE: REUSE 6 kelompok, COMPOSE 2, NEW base component 0; tidak mengubah default primitive. Grep scope view baru tidak menemukan tombol/input/table mentah, style inline atau util typography Bootstrap. Dua koreksi CSS terkait admisi tidak menambah warna/font/important baru. Master-data checklist NOT APPLICABLE karena tidak membangun layar CRUD master baru.

## 4. State yang ditangani

| State | Perilaku |
| --- | --- |
| Memuat | Pencarian ibu/episode loading, aksi penyimpanan/aktivasi busy; resume menunggu pembacaan episode |
| Kosong | Ibu tidak ditemukan, episode ibu tidak tersedia, identitas/waktu belum dapat diverifikasi; tidak mengarang ibu atau waktu |
| Gagal | InformationAlert + coba lagi; AbortSignal mencegah respons pencarian lama menimpa pilihan baru; data isian dipertahankan |
| Tanpa hak akses | Aktivasi membutuhkan InpatientBedOccupancy : Create; cetak mengikuti InpatientAdmissionDocument : Print; backend tetap menegakkan izin |
| Penyimpanan sebagian | RM yang sudah dibuat dipakai ulang untuk retry dokumen/kontak; identitas master tidak dibuat kedua |
| Selesai | Reset konteks entry dan identitas lokal; dokumen dapat dibuka lagi dari episode aktif |

## 5. Endpoint yang dikonsumsi

### Health Services / Patient Management / Master Data / Patient

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| GET/POST | /v1/health-services/patient-management/master-data/patients/admin | Cari dan simpan identitas bayi/ibu | Patient : Read/Create |
| GET | /v1/health-services/patient-management/master-data/patients/admin/{id} | Verifikasi master tersimpan | Patient : Read |

Kontak/identitas pendukung memakai service admin existing; tanpa dokumen identitas palsu bila NIK bayi belum ada.

### Health Services / Inpatient Management / Inpatient Episode

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| GET | /episodes/active-mothers | Cari episode milik pasien ibu terverifikasi | InpatientEpisode : Read |
| POST/GET | /episodes, /episodes/{id} | Draft dan pemulihan konteks/penjamin via encounter existing | InpatientEpisode : Create/Read |

### Health Services / Inpatient Management / Bed Occupancy

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| GET | /bed-occupancies/bed-board, /available-beds | Daftar dan kelayakan dari server | InpatientBedOccupancy : Read |
| POST | /bed-occupancies/reservations, /placements | Pesan dan aktifkan dengan versi/operation existing | InpatientBedOccupancy : Create |

### InpatientAdmissionConsent

GET/POST /v1/inpatient-admissions/{episodeId}/consent mengikuti service existing; Parent/Guardian untuk bayi. Hak InpatientEpisode : Read/Create sesuai controller existing.

### Health Services / Inpatient Management / Inpatient Admission Workspace

GET /episodes/{id}/admission-workspace/identity-labels dan endpoint print/log existing; data cetak dari server, hak Print existing. Link persetujuan/dokumen menuju route episode yang benar.

## 6. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| npm run lint:errors | Pernah selesai exit 0 setelah implementasi awal; pengulangan penuh saat disk penuh berhenti native -1073740791; pengulangan akhir dengan node --max-old-space-size=4096 eslint . --quiet selesai exit 0 | PASS final; EXISTING / ENVIRONMENT ISSUE saat interupsi |
| node --test scope newborn + bed-board | 21/21 lulus | PASS |
| npm run test:unit final | 2.869 uji, 2.842 lulus, 27 gagal | EXISTING / ENVIRONMENT ISSUE untuk 27 kegagalan |
| Snapshot read-only HEAD pada folder temp | 2.853 uji, 2.818 lulus, 35 gagal; seluruh 27 kegagalan final juga ada pada HEAD; nol kegagalan baru | PASS pembandingan regresi |
| Playwright headed pada preview terisolasi dengan API tiruan | 5/5 skenario lulus, final 27,4 detik | PASS frontend; bukan bukti database/API asli |
| npm run build / Turbopack final | Compiled successfully 69 detik; TypeScript selesai; 492/492 halaman terbentuk; copy standalone gagal ENOSPC | SOURCE COMPILE PASS; FULL BUILD EXISTING / ENVIRONMENT ISSUE |
| Fallback Webpack | Batas heap 4 GB lalu percobaan 8 GB menemukan CSS module purity pada modul Billing/IGD lain | EXISTING / ENVIRONMENT ISSUE; tidak memperbaiki modul lain |
| Review visual | Screenshot Dokumen Bayi: nama ibu, RM, urutan kembar dan waktu aktif tampil; pesan resume diperbaiki sesudah review | PASS scope desktop; responsif rinci/print fisik NOT RUN |
| Anti-regresi UI view baru | Tidak ada primitive baru, tombol/table mentah atau style inline; base defaults tetap | PASS |

AUTOMATED TEST: scope Node - PASS. AUTOMATED TEST: Playwright headed - PASS 5/5. AUTOMATED TEST: suite penuh - FAIL 27 existing, nol kegagalan baru dibanding HEAD. MANUAL TEST: PASS untuk review visual desktop dari hasil browser; NOT FEASIBLE untuk PostgreSQL nyata, printer fisik dan settlement lintas invoice karena belum menjadi runtime terotorisasi.

Bukti browser: frontend/test-with-agy/episode-bayi-016.spec.mjs, playwright.newborn.config.mjs, episode-bayi-016-documents.png. Folder ini diabaikan Git sesuai repo. Domain API seluruhnya diintersep; tidak menulis pasien ke backend rumah sakit. Preview port 3710 milik task sudah dihentikan; server pengguna port 3000 tidak dihentikan. Log command sementara tidak memuat secret dalam laporan.

Pembersihan .next/standalone hasil pengemasan parsial ditolak automatic approval review dengan pesan blocked by policy. Tidak memakai cara lain untuk melewati penolakan. Ini bukan bukti deployment siap.

## 7. Acceptance criteria dan Definition of Done

| Kriteria roadmap | Status | Bukti |
| --- | --- | --- |
| 1. Tahap bayi new/existing dan RM reuse | Terpenuhi | Resolusi stepper, 2 skenario browser, unit urutan 9/8 tahap |
| 2. Identitas/ibu/kelahiran/kembar tanpa NIK fiktif dan retry | Terpenuhi pada source/FE tiruan | Unit payload/validasi, browser POST bayi sekali dan pasien lama tanpa POST master |
| 3. Sumber/perawatan/DPJP/penjamin/preferensi/deposit diteruskan dan dipulihkan | Terpenuhi pada source/FE tiruan | Payload context, detail normalization, UI persisted draft, label langkah sesuai tujuan |
| 4. Pencarian ibu dan bed sebelum EpisodeId | Terpenuhi pada source/FE tiruan | Abort/pagination/retry hook; browser bergerak care?payer?deposit?bed dan query subject |
| 5. Consent sebelum aktivasi dan retry operasi | Terpenuhi pada source/FE tiruan | Browser menolak tanpa consent lalu menghasilkan tepat satu placement setelah consent |
| 6. Dokumen/gelang/waktu serta billing sebagai preferensi | Terpenuhi pada source/FE tiruan | Browser dokumen Admitted tanpa placement kedua; gelang kembar; waktu belum diketahui tidak ditebak |
| 7. Lint/unit/build/interaksi dan laporan | Sebagian | Scope/browser lulus, full suite gagal existing; packaging standalone belum lulus akibat ENOSPC |

DoD runtime/production belum ditutup. Tidak mengklaim siap deploy atau pembayaran dua invoice sudah dijalankan kasir.

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Pengemasan butuh ruang disk; 27 kegagalan suite existing; beberapa stylesheet luar scope tidak kompatibel fallback Webpack |
| Masalah yang diketahui | Ibu tidak dikenal memakai prosedur identitas sementara yang belum tersedia. Profil bayi legacy yang belum lengkap diarahkan ke koreksi master berwenang. Konteks/payer terkunci setelah Draft dibuat; koreksi bukan dilakukan diam-diam lewat tombol mundur |
| Dependency backend | Migration enam kolom dan backend baru belum diterapkan/dijalankan terhadap DB; browser tiruan tidak menggantikan UAT integrasi |
| Perubahan sampingan | Dua blocker CSS admisi diperbaiki; file status Playwright tracked dipulihkan, bukti tetap di test-with-agy; perubahan pengguna lain dipertahankan |
| Interupsi | Disk penuh dan limit heap saat build; automatic approval review menolak penghapusan hasil standalone parsial |
| Langkah berikutnya | Sediakan ruang untuk packaging, ulang build; pemilik menerapkan migration sesuai AGENTS.md dan menjalankan backend baru; UAT controlled, print fisik dan audit handoff Billing |

Status Git akhir pemeriksaan (termasuk perubahan eksternal yang dipertahankan):

~~~text
 M src/app/health-services/inpatient-management/bed-board/page.jsx
 M src/components/view/health-services/inpatient-management/admission-transfers/inpatient-admission-transfer-list.jsx
 M src/components/view/health-services/inpatient-management/admission-transfers/inpatient-admission-transfer-list.module.css
 M src/components/view/health-services/inpatient-management/admission-workspace/admission-workspace-sections.jsx
 M src/components/view/health-services/inpatient-management/admission-workspace/sections/general-consent/general-consent-edit-form.jsx
 M src/components/view/health-services/inpatient-management/admission-workspace/sections/general-consent/general-consent-section.jsx
 M src/components/view/health-services/inpatient-management/bed-management/bed-management-view.jsx
 M src/components/view/health-services/inpatient-management/inpatient-admission-bed-step.jsx
 M src/components/view/health-services/inpatient-management/inpatient-admission-confirmation-step.jsx
 M src/components/view/health-services/inpatient-management/inpatient-admission-consent-form-step.jsx
 M src/components/view/health-services/inpatient-management/inpatient-admission-deposit-step.jsx
 M src/components/view/health-services/inpatient-management/inpatient-admission-doctor-step.jsx
 M src/components/view/health-services/inpatient-management/inpatient-admission-existing-patient-step.jsx
 M src/components/view/health-services/inpatient-management/inpatient-admission-payment-step.jsx
 M src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx
 M src/components/view/health-services/inpatient-management/inpatient-admission-visit-category-step.jsx
 M src/components/view/health-services/inpatient-management/nursing-workspace/sections/billing/nursing-billing-section.jsx
 M src/components/view/health-services/registration-management/emergency-registration/new-patient-form.jsx
 M src/lib/constants/health-services/inpatient-management/bed-management-constants.js
 M src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx
 M src/lib/constants/health-services/inpatient-management/inpatient-admission-workspace-constants.js
 M src/lib/constants/health-services/inpatient-management/inpatient-dashboard-constants.jsx
 M src/lib/hooks/health-services/inpatient-management/use-admission-general-consent.js
 M src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-bed.jsx
 M src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-doctor.jsx
 M src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-flow.jsx
 M src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-patient.jsx
 M src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-resume.jsx
 M src/lib/hooks/health-services/inpatient-management/use-inpatient-bed-board.jsx
 M src/lib/hooks/health-services/registration-management/outpatient-registration/use-outpatient-registration.js
 M src/lib/services/health-services/inpatient-management/inpatient-admission-patient.service.js
 M src/lib/services/health-services/inpatient-management/inpatient-episode.service.js
 M src/lib/services/health-services/registration-management/emergency-region.service.js
 M src/style/health-services/inpatient-management/admission-workspace.module.css
 M src/style/health-services/inpatient-management/inpatient-admission.module.css
 M src/style/health-services/inpatient-management/inpatient-consent-print.module.css
 M src/style/health-services/inpatient-management/nursing-billing-section.module.css
 M src/utils/health-services/inpatient-management/inpatient-admission-encounter-utils.jsx
 M src/utils/health-services/inpatient-management/inpatient-admission-patient-utils.jsx
 M src/utils/health-services/inpatient-management/inpatient-bed-utils.jsx
 M src/utils/health-services/inpatient-management/inpatient-episode-utils.jsx
 M src/utils/health-services/registration-management/emergency-management/emergency-registration.utils.js
 M src/utils/menu-sidebar/menu-items.jsx
 M tests/unit/inpatient-admission-workspace.test.mjs
 M tests/unit/inpatient-bed-board.test.mjs
 M tests/unit/inpatient-newborn-admission.test.mjs
 M tests/unit/inpatient-transfer-billing-relocation.test.mjs
?? src/components/view/health-services/inpatient-management/inpatient-newborn-admission-steps.jsx
?? src/lib/constants/health-services/inpatient-management/inpatient-newborn-admission-constants.js
?? src/lib/hooks/health-services/inpatient-management/use-inpatient-newborn-admission.js
?? src/utils/health-services/inpatient-management/inpatient-newborn-admission-utils.js
?? tests/unit/inpatient-admission-supporting-doctor.test.mjs
?? tests/unit/inpatient-existing-patient-search.test.mjs
~~~

## Handoff source kepada pemilik - 15 Oktober 2026

Pengguna menginstruksikan implementasi kode saja; dotnet build dan penerapan migration dikerjakan sendiri oleh pengguna. Tidak ada perintah build, migration atau pengujian baru yang dijalankan setelah instruksi tersebut. Helper preview/test milik task sudah tidak berjalan. Pemeriksaan source/read-only memastikan implementasi pasien baru/lama serta tiga artefak migration tetap tersedia pada commit terbaru.

Scope source admisi selesai. Validasi build dan penerapan database berikutnya menjadi tanggung jawab pemilik sesuai instruksi terbaru; catatan kendala packaging tanggal 14 tetap dipertahankan sebagai riwayat dan bukan pekerjaan yang diteruskan agent. Settlement dua invoice tetap di luar slice source admisi ini; pilihan Combined adalah preferensi/handoff Billing.

Pembaruan konteks source: frontend 60f7764a08c032e2ead66c9ea744e44e2497f6b6, backend bcc663df321aabbc07888385605aa196c74cc6f3. Perubahan working tree denah/transfer di luar slice ini dipertahankan. Percobaan perluasan skenario browser hari ini dihentikan; tidak dicatat sebagai hasil lulus baru.
