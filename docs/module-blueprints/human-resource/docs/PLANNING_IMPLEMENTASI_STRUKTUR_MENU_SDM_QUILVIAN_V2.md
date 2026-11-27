# PLANNING & INSTRUKSI AGENT — RESTRUKTURISASI SIDEBAR SDM QUILVIAN V2

**Tanggal:** 25 November 2026 (WIB)  
**Status requirement:** APPROVED — struktur terakhir telah disetujui pengguna.  
**Jenis pekerjaan:** penataan hierarki navigasi frontend (bukan pengembangan fitur atau redesain halaman).  
**Status implementasi:** PLANNED / NOT RUN; dokumen ini bukan laporan bahwa pekerjaan sudah selesai.

## 1. Role dan mandat

Anda adalah Senior Frontend Engineer Quilvian V2 (Next.js/React) dengan tanggung jawab menjaga usability, konsistensi base component, dan kontrol akses. **Langsung implementasikan task yang telah disetujui dalam workspace yang berwenang setelah preflight.** Jangan berhenti pada analisis, desain mockup, atau membuat dokumen planning baru. Dokumen ini mengatur seluruh perubahan dalam satu paket finishing navigasi.

Gunakan task lokal `FIN-HRD-NAV-FE-001` s.d. `007` untuk pelacakan paket ini. ID ini adalah usulan penomoran pekerjaan finishing, bukan klaim ID roadmap canonical; rekonsiliasi dengan registry task setempat sebelum mencatat. Jangan mengganti ID task terdahulu.

### Governance dan batas otorisasi

- Baca `AGENTS.md`, `CLAUDE.md`, canonical `agents/rules/GLOBAL_RULES.md`, aturan frontend, dan aturan testing yang relevan sebelum coding. Instruksi ini secara eksplisit meminta runtime skill `quilvian-engineering-skills:build-module-frontend` (gunakan nama yang terdaftar pada IDE/vendor). Canonical governance tetap mengikat terlepas dari cara invoke skill. Bila governance tidak tersedia, laporkan `BLOCKED — canonical governance unavailable` sesuai aturan repo; bila skill yang diminta tidak tersedia, laporkan kondisi dan ikuti aturan runtime/repository, jangan mengarang bukti pemanggilan.
- Scope tulis: konfigurasi sidebar SDM, helper/renderer/CSS navigasi **hanya bila terbukti diperlukan**, dan test navigasi relevan. Jangan ubah source backend, database, seed, migrasi, API, service transaksi, halaman detail, form, maupun data.
- Gunakan base component, helper, icon pattern, design token dan renderer existing. Tidak membuat sidebar baru, library baru, atau salinan konfigurasi menu.
- Tidak otomatis commit, push, pull, merge, rebase, deploy, mengubah role/permission akun, atau menulis database. Bila branch workspace tidak sesuai, laporkan dan jangan beralih atau menulis branch lain tanpa izin. Bukti HEAD awal dokumen tidak menggantikan preflight aktual Agent.
- Jangan mengubah dokumen blueprint/roadmap backend atas nama pekerjaan ini tanpa izin terpisah. Jika aturan tracked-report mengharuskan pencatatan di lokasi lain, jelaskan target dan ikuti izin eksplisit yang sah; jangan mengubah source BE.

## 2. Context — keputusan pengguna dan evidence

### 2.1 Keputusan yang terkunci

1. Pertahankan `Sumber Daya Manusia` sebagai modul existing. Dashboard SDM tetap pada tingkat pertama (setelah modul dibuka, sebelum Master Data menurut struktur target).
2. `Master Data` SDM tetap existing: seluruh 10 kelompok dan 55 entri leaf, urutan internal, key, route, icon, dan metadata tidak disentuh. Tidak ditambahkan grup bernama HR Foundation.
3. Hanya kelompok 02–07 ditata ulang sesuai struktur pada Bab 3. Angka 01–07 merupakan nomor pembahasan, **bukan prefix label sidebar**. Teks `(Existing)` juga hanya anotasi dokumen, bukan bagian label.
4. Kotak Masuk Persetujuan Terpadu (V2) dan Layanan SDM & Meja Bantuan tetap sebagai akses langsung dalam SDM; pertahankan label aktual, seluruh isi, route, key dan permission. Tidak menggabungkan kedua halaman inbox.
5. Administrator, Payroll, Accounting, modul lain, Dashboard global, Ruang Kerja Saya (`/self-services`), dan Side Profile tidak diubah. ESS tidak dibuat kembali di SDM.
6. Seluruh halaman, endpoint, permission dan fungsi existing dipertahankan. Tidak membuat landing page baru, menu transaksi baru, atau shortcut Payroll baru dalam pekerjaan ini. Tidak melakukan perubahan visual halaman bisnis.

### 2.2 Bukti source yang diperiksa (GitHub read-only)

| File / simbol                                                                                                                                 | Fakta yang terverifikasi pada snapshot                                                                                                                                                                                                           |
| --------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `src/utils/menu-sidebar/menu-items.jsx`                                                                                                       | SDM saat ini memiliki kelompok menu transaksi sejajar. Master Data (10 grup, 55 leaf), Dashboard SDM, ESS global, dan Payroll terpisah sudah ada. Onboarding, batch kontrak, data penggajian serta OPPE/FPPE akan dipindahkan sesuai keputusan.  |
| `src/components/features/left-sidebar/left-sidebar-items-virtualized.jsx`                                                                     | Renderer membaca child tree rekursif, `MAX_MENU_DEPTH = 8`, `filterMenuTree`, `findBestActivePath`, `buildActivePathKeys`, penyimpanan expanded keys, dan permission filtering sebelum pencarian. Parent aktif dibedakan dari leaf aktif.        |
| `src/components/features/left-sidebar/left-sidebar-menu-handle.jsx`                                                                           | `getChildren`, `findFullPathByPathname`, helper penelusuran nested menu dan pemetaan path. Perlu pengujian breadcrumb/route yang lebih dalam.                                                                                                    |
| `src/utils/menu-sidebar/permission/filter-menu-items-by-permission.jsx`                                                                       | Filter rekursif menerima `requiredPermission` objek atau array OR; permission belum diketahui disembunyikan, grup tanpa anak yang berhak dibuang jika tidak punya route. Hanya visibility, bukan pengganti otorisasi API.                        |
| `src/utils/menu-sidebar/role/filter-menu-items-by-role.jsx`                                                                                   | Filter role existing dipertahankan; jangan refactor logic modul lain.                                                                                                                                                                            |
| `src/components/features/left-sidebar/left-sidebar.jsx`, `left-sidebar-wrapper.jsx`, `src/style/components/features/sidebar/left-sidebar.css` | Sidebar desktop/mini/drawer, scroll, style active dan search existing. Gunakan ulang.                                                                                                                                                            |
| `tests/unit/hr-master-data-sidebar-grouping.test.mjs`                                                                                         | HR-01/02 mengunci 10 grup dan 55 leaf Master Data. HR-03/07/08 mengasumsikan struktur lama; HR-08 mengunci lima OPPE/FPPE di Penilaian Kerja. Perlu penyesuaian assertion navigasi sesuai keputusan terbaru, bukan menghapus pengujian keamanan. |
| `tests/unit/menu-permission-filter.test.mjs`, `tests/unit/self-service-menu-dedup.test.mjs`, `tests/unit/sidebar-scroll.test.mjs`             | Regression coverage permission, ESS deduplication dan scroll; pertahankan.                                                                                                                                                                       |
| `package.json`, `AGENTS.md`, `CLAUDE.md` FE; `agents/rules/GLOBAL_RULES.md`, `agents/rules/frontend/test-policy.md` Skills                    | Runner Node test, lint, build, Playwright serta aturan kerja. Jangan tambah framework test.                                                                                                                                                      |
| BE `docs/module-blueprints/human-resource/00-business-overview.md`                                                                            | Referensi batas domain HR, payroll/Finance, lifecycle, dan kewenangan klinis. Tidak menjadi target tulis.                                                                                                                                        |

**Batas bukti:** snapshot ini berasal dari GitHub pada SHA di atas. ZIP FE yang terlampir adalah snapshot lebih lama (file menu jauh lebih pendek), sehingga tidak digunakan untuk inventaris terbaru; ZIP BE `master` bukan bukti branch `MHamzah`. Belum ada eksekusi aplikasi, verifikasi permission akun, atau perubahan kode oleh penyusun dokumen. Kesimpulan menyangkut konfigurasi navigasi, bukan kelulusan fungsi bisnis semua route. Rancangan lama yang mengusulkan landing page kelompok telah dipersempit oleh keputusan pengguna: cukup hierarki sidebar existing.

### 2.3 Rekapitulasi cakupan baseline

| Kelompok                             | Entri leaf/pathname | Perlakuan                                 |
| ------------------------------------ | ------------------: | ----------------------------------------- |
| Dashboard SDM                        |                   1 | Pertahankan                               |
| 01. Master Data                      |                  55 | Pertahankan seluruh subtree               |
| 02. Siklus Kepegawaian               |                  40 | Tata ulang                                |
| 03. Pengelolaan Tenaga Kerja         |                  30 | Tata ulang                                |
| 04. Pengembangan & Kinerja           |                  16 | Tata ulang                                |
| 05. Kompensasi & Kesejahteraan       |                  10 | Tata ulang                                |
| 06. Tata Kelola & Kepatuhan          |                  29 | Tata ulang                                |
| 07. Perencanaan Strategis SDM        |                   9 | Tata ulang                                |
| Kotak Masuk Persetujuan Terpadu (V2) |                   5 | Pertahankan                               |
| Layanan SDM & Meja Bantuan           |                   3 | Pertahankan                               |
| **Total**                            |             **198** | **134 dipetakan ulang; 64 dipertahankan** |

Pada snapshot terdapat 198 entri `pathname` aktif (196 pathname unik) di SDM. Scope 02–07 berisi 134 key leaf berbeda dan 133 pathname unik. Hitung entri berdasarkan key dan lokasi, bukan hanya URL. Grup folder tidak dihitung sebagai leaf. Jika HEAD terbaru berubah, tampilkan delta terhadap baseline, jangan membuang entri baru untuk mengejar jumlah.

## 3. Task — hierarki menu target yang disetujui

Gunakan pohon berikut sebagai sumber kebenaran label, posisi, dan urutan target. Master Data dan dua menu pendukung tidak dijabarkan karena tidak diubah. Pada UI, jangan tampilkan angka atau anotasi Existing. Menu induk hanya folder yang bisa expand/collapse, kecuali node yang memang memiliki route existing.

```text
SUMBER DAYA MANUSIA
│
├── Dashboard SDM
├── 01. Master Data (Existing)
├── 02. Siklus Kepegawaian
│   ├── Rekrutmen & Seleksi
│   │   ├── Permintaan Requisition
│   │   ├── Lowongan
│   │   ├── Pelamar
│   │   ├── Seleksi Administrasi
│   │   ├── Tes/Wawancara
│   │   ├── Surat Penawaran (Offer)
│   │   ├── Pemantauan MCU Pra-Kerja
│   │   └── Talent Pool Cadangan
│   ├── Penerimaan & Onboarding
│   │   ├── Perlu Tindakan
│   │   ├── Onboarding
│   │   ├── Kasus Onboarding
│   │   ├── Template Onboarding
│   │   ├── Pendampingan Klinis
│   │   ├── Permintaan Akses IT
│   │   └── Pengaturan Lifecycle
│   ├── Kontrak & Masa Kerja
│   │   ├── Perjanjian Kerja
│   │   ├── Perpanjangan Kontrak
│   │   ├── Masa Percobaan
│   │   ├── Perubahan Status Kerja
│   │   └── Batch Data Awal Kontrak
│   ├── Penempatan, Mutasi & Promosi
│   │   ├── Penempatan Awal
│   │   ├── Perpindahan Unit
│   │   ├── Rotasi Promosi
│   │   ├── Uraian Tugas
│   │   └── Atasan Langsung
│   ├── Administrasi Kepegawaian
│   │   ├── Surat Keputusan
│   │   ├── Surat Tugas
│   │   ├── Surat Keterangan
│   │   ├── Dokumen Administrasi
│   │   ├── Pengajuan Perubahan Data
│   │   ├── Penempatan Organisasi
│   │   ├── Penempatan Jabatan
│   │   ├── Riwayat Kepegawaian
│   │   └── Batch Data Awal
│   └── Pemberhentian & Pensiun
│       ├── Pengunduran Diri
│       ├── Pensiun
│       ├── Akhir Kontrak
│       ├── Pemberhentian
│       ├── Serah Terima
│       └── Clearance
├── 03. Pengelolaan Tenaga Kerja
│   ├── Penjadwalan & Roster
│   │   ├── Matriks Roster Shift 24/7
│   │   ├── Riwayat Revisi Roster
│   │   ├── Penugasan Siaga (On-Call)
│   │   └── Pengaturan Penjadwalan
│   ├── Presensi & Kehadiran
│   │   ├── Monitoring Presensi Harian
│   │   ├── Pengaturan Presensi
│   │   ├── Koreksi Presensi
│   │   ├── Izin Pulang Cepat
│   │   ├── Dinas Luar
│   │   ├── Periode Presensi
│   │   └── Mesin Absen
│   ├── Cuti & Perizinan
│   │   ├── Persetujuan Cuti Staf
│   │   ├── Manajemen Saldo Staf
│   │   ├── Penyesuaian Saldo Cuti
│   │   ├── Pemanggilan Darurat (Recall)
│   │   └── Rekonsiliasi & Payroll Close
│   ├── Manajemen Lembur
│   │   ├── Persetujuan Lembur Ruangan
│   │   ├── Verifikasi Presensi & Discrepancy
│   │   ├── Eskalasi Direktur Medik
│   │   ├── Saldo Cuti Pengganti (CTO)
│   │   ├── Rekonsiliasi & Payroll Close
│   │   └── Pengaturan Kebijakan Lembur
│   └── Perjalanan Dinas & Biaya (SPPD)
│       ├── Dasbor Perjalanan Dinas
│       ├── Matriks Tarif & Kebijakan SPPD
│       ├── Pusat Penugasan & Riwayat SPPD
│       ├── Formulir Pengajuan SPPD Baru
│       ├── Konsol Uang Muka (Advances)
│       ├── Lembar Rekonsiliasi Settlement
│       ├── Radar Keterlambatan Overdue
│       └── Klaim Biaya Non-Trip Langsung
├── 04. Pengembangan & Kinerja
│   ├── Penilaian Kerja
│   │   ├── Sasaran Kinerja
│   │   ├── Indikator Individu
│   │   ├── Evaluasi Atasan
│   │   ├── Hasil Penilaian
│   │   ├── Tindak Lanjut
│   │   ├── Sidang Kalibrasi Manajemen RS
│   │   ├── Jenjang Karier Klinis PK I-V
│   │   └── Sanggah Banding Nilai Kinerja
│   └── Pendidikan & Pelatihan RS (Diklat)
│       ├── Dasbor Pelatihan & Radar
│       ├── Master Aturan Wajib STARKES
│       ├── Manajemen Sesi & Kelas Diklat
│       ├── Meja Asesmen Teori & OSCE
│       ├── Registri Portofolio e-Sertifikat
│       ├── Rencana Tahunan TNA & Anggaran
│       ├── Tata Kelola Ikatan Dinas Retensi
│       └── Portofolio 50 SKP Plataran Sehat
├── 05. Kompensasi & Kesejahteraan
│   ├── Data Penggajian Pegawai
│   └── Manajemen Manfaat (Benefits)
│       ├── Dasbor Analitik Manfaat
│       ├── Katalog Paket Manfaat
│       ├── Kepesertaan Asuransi Pegawai
│       ├── Enrollment Benefit Pegawai
│       ├── Persetujuan Perjanjian Angsuran
│       ├── Piutang Benefit Pegawai
│       ├── Verifikasi Tanggungan Keluarga
│       ├── Buku Besar & Hak Berobat RS
│       └── Meja Kerja Klaim COB
├── 06. Tata Kelola & Kepatuhan
│   ├── Kompetensi & Kredensial
│   │   ├── Data STR/ SIP
│   │   ├── Sertifikasi Kompetensi
│   │   ├── Persyaratan Jabatan
│   │   ├── Status Kredensial
│   │   ├── Masa Berlaku Kewenangan
│   │   ├── Kalkulator Rapor Komposit 3 Tahun
│   │   └── Konsol Sinkronisasi SIMRS EMR
│   ├── Pemantauan Kinerja Klinis
│   │   ├── Dashboard Rapor Kinerja Klinis (OPPE)
│   │   ├── Manajemen Periode & Matriks OPPE
│   │   ├── Pusat Deteksi Pemicu FPPE
│   │   ├── Pemantauan Proctoring FPPE
│   │   └── Sidang Pleno Putusan FPPE
│   ├── Hubungan Industrial & Terminasi
│   │   ├── Dasbor Hubungan Industrial & Radar
│   │   ├── Tindak Disiplin
│   │   ├── Pelanggaran
│   │   ├── Riwayat Penyelesaian
│   │   ├── Skorsing Investigasi (Paid Suspension)
│   │   ├── Pengaduan Staf & Whistleblowing
│   │   ├── Risalah Forum Bipartit RS
│   │   ├── Pengajuan PHK & Pesangon PP 35/2021
│   │   └── Exit Clearance Karyawan
│   └── Kesehatan & Keselamatan Kerja RS (K3RS)
│       ├── Dasbor K3RS & Metrik Zero Accident
│       ├── Rekam Kesehatan & Fitness to Work
│       ├── Pembatasan Tugas (Work Restrictions)
│       ├── Konsol Tertusuk Jarum CITO (PEP < 4 Jam)
│       ├── Radar Imunisasi Nakes STARKES
│       ├── Surveilans Bahaya & Paparan TLD Radiasi
│       ├── Pusat MCU Berkala STARKES KPS 11
│       └── Registrasi Insiden KAK/PAK & BPJS JKK
├── 07. Perencanaan Strategis SDM
│   ├── Analisa Beban Kerja
│   ├── Formasi Pegawai
│   ├── Pengajuan Kebutuhan Tenaga
│   ├── Evaluasi Kebutuhan
│   ├── Pengaturan Perencanaan
│   ├── Batch Formasi Awal
│   ├── Standar Kebutuhan Tenaga
│   ├── Antrean Rekrutmen
│   └── Rencana Tahunan
├── Kotak Masuk Persetujuan Terpadu (Existing)
└── Layanan SDM & Meja Bantuan (Existing)
```

### 3.1 Perubahan parent yang wajib dilakukan

| Leaf / kelompok existing                  | Parent lama              | Parent target                                       | Aturan                                                                                  |
| ----------------------------------------- | ------------------------ | --------------------------------------------------- | --------------------------------------------------------------------------------------- |
| Rekrutmen & Seleksi                       | SDM                      | Siklus Kepegawaian                                  | Pertahankan 8 leaf selain Onboarding                                                    |
| Onboarding (`corporateHiringHandoffs`)    | Rekrutmen & Seleksi      | Penerimaan & Onboarding                             | Route handoff tetap `/hr/recruitment/handoffs`                                          |
| Onboarding & Kontrak                      | SDM                      | Siklus Kepegawaian → Penerimaan & Onboarding        | Rename folder, pertahankan key folder jika aman; pindah batch kontrak, masukkan handoff |
| Batch Data Awal Kontrak                   | Onboarding & Kontrak     | Kontrak & Masa Kerja                                | Jangan samakan dengan batch penempatan                                                  |
| Kontrak & Masa Kerja                      | SDM                      | Siklus Kepegawaian                                  | 4 leaf lama + batch kontrak                                                             |
| Penempatan, Mutasi & Promosi              | SDM                      | Siklus Kepegawaian                                  | 5 leaf existing                                                                         |
| Administrasi Kepegawaian                  | SDM                      | Siklus Kepegawaian                                  | 9 leaf, keluarkan Data Penggajian Pegawai                                               |
| Pemberhentian & Pensiun                   | SDM                      | Siklus Kepegawaian                                  | 6 leaf existing                                                                         |
| Penjadwalan, Presensi, Cuti, Lembur, SPPD | SDM                      | Pengelolaan Tenaga Kerja                            | Kelima subtree tetap utuh                                                               |
| Penilaian Kerja                           | SDM                      | Pengembangan & Kinerja                              | 8 leaf non-OPPE/FPPE                                                                    |
| Pendidikan & Pelatihan RS (Diklat)        | SDM                      | Pengembangan & Kinerja                              | 8 leaf existing                                                                         |
| Data Penggajian Pegawai                   | Administrasi Kepegawaian | Kompensasi & Kesejahteraan                          | Leaf langsung; bukan modul Payroll                                                      |
| Manajemen Manfaat (Benefits)              | SDM                      | Kompensasi & Kesejahteraan                          | 9 leaf existing                                                                         |
| Kompetensi & Kredensial                   | SDM                      | Tata Kelola & Kepatuhan                             | 7 leaf existing                                                                         |
| Lima OPPE/FPPE                            | Penilaian Kerja          | Tata Kelola & Kepatuhan → Pemantauan Kinerja Klinis | Parent baru; key, route, permission klinis tetap                                        |
| Hubungan Industrial & Terminasi, K3RS     | SDM                      | Tata Kelola & Kepatuhan                             | Kedua subtree tetap utuh                                                                |
| Perencanaan Kebutuhan SDM                 | SDM                      | Perencanaan Strategis SDM                           | Rename folder existing; 9 leaf langsung, tanpa folder perantara                         |

### 3.2 Pemetaan leaf ke route dan key existing (134 entri)

Tabel di bawah adalah manifest target. **Key dan pathname berasal dari konfigurasi FE snapshot**; `requiredPermission`, icon, `href` dan metadata lain harus dipertahankan dengan memindahkan object existing secara utuh, bukan membuat ulang object berdasarkan tabel. Jika ada permission yang sebelumnya tidak dicantumkan, jangan menambah atau menghapusnya dalam task navigasi. Periksa dan laporkan mismatch secara terpisah.

#### 02. Siklus Kepegawaian — 40 entri

| Menu induk dalam kelompok    | Submenu / leaf           | Key existing                         | Pathname existing                              |
| ---------------------------- | ------------------------ | ------------------------------------ | ---------------------------------------------- |
| Rekrutmen & Seleksi          | Permintaan Requisition   | `corporateJobRequisitions`           | `/hr/recruitment/requisitions`                 |
| Rekrutmen & Seleksi          | Lowongan                 | `corporateJobVacancies`              | `/hr/recruitment/vacancies`                    |
| Rekrutmen & Seleksi          | Pelamar                  | `corporateCandidates`                | `/hr/recruitment/candidates`                   |
| Rekrutmen & Seleksi          | Seleksi Administrasi     | `corporateCandidateScreenings`       | `/hr/recruitment/screenings`                   |
| Rekrutmen & Seleksi          | Tes/Wawancara            | `corporateCandidateEvaluations`      | `/hr/recruitment/evaluations`                  |
| Rekrutmen & Seleksi          | Surat Penawaran (Offer)  | `corporateJobOffers`                 | `/hr/recruitment/offers`                       |
| Rekrutmen & Seleksi          | Pemantauan MCU Pra-Kerja | `corporateMcuClearance`              | `/hr/recruitment/mcu-clearance`                |
| Rekrutmen & Seleksi          | Talent Pool Cadangan     | `corporateTalentPool`                | `/hr/recruitment/talent-pool`                  |
| Penerimaan & Onboarding      | Perlu Tindakan           | `corporateLifecycleWorklist`         | `/hr/lifecycle/worklist`                       |
| Penerimaan & Onboarding      | Onboarding               | `corporateHiringHandoffs`            | `/hr/recruitment/handoffs`                     |
| Penerimaan & Onboarding      | Kasus Onboarding         | `corporateOnboardingCases`           | `/hr/lifecycle/onboarding-cases`               |
| Penerimaan & Onboarding      | Template Onboarding      | `corporateOnboardingTemplates`       | `/hr/lifecycle/onboarding-templates`           |
| Penerimaan & Onboarding      | Pendampingan Klinis      | `corporateClinicalPreceptorships`    | `/hr/lifecycle/clinical-preceptorships`        |
| Penerimaan & Onboarding      | Permintaan Akses IT      | `corporateOnboardingAccessRequests`  | `/hr/lifecycle/onboarding-access-requests`     |
| Penerimaan & Onboarding      | Pengaturan Lifecycle     | `corporateLifecycleSettings`         | `/hr/lifecycle/settings`                       |
| Kontrak & Masa Kerja         | Perjanjian Kerja         | `corporateEmploymentContracts`       | `/hr/lifecycle/employment-contracts`           |
| Kontrak & Masa Kerja         | Perpanjangan Kontrak     | `corporateContractRenewals`          | `/hr/lifecycle/contract-renewals`              |
| Kontrak & Masa Kerja         | Masa Percobaan           | `corporateProbations`                | `/hr/lifecycle/probations`                     |
| Kontrak & Masa Kerja         | Perubahan Status Kerja   | `corporateEmploymentStatusChanges`   | `/hr/lifecycle/employment-status-changes`      |
| Kontrak & Masa Kerja         | Batch Data Awal Kontrak  | `corporateInitialContractBatches`    | `/hr/lifecycle/initial-contract-batches`       |
| Penempatan, Mutasi & Promosi | Penempatan Awal          | `corporateHrInitialPlacements`       | `/hr/workforce-core/initial-placements`        |
| Penempatan, Mutasi & Promosi | Perpindahan Unit         | `corporateHrUnitTransfers`           | `/hr/workforce-core/unit-transfers`            |
| Penempatan, Mutasi & Promosi | Rotasi Promosi           | `corporateHrRotationPromotions`      | `/hr/workforce-core/rotation-promotions`       |
| Penempatan, Mutasi & Promosi | Uraian Tugas             | `corporateHrEmployeeJobDescriptions` | `/hr/workforce-core/employee-job-descriptions` |
| Penempatan, Mutasi & Promosi | Atasan Langsung          | `corporateHrEmployeeManagers`        | `/hr/workforce-core/employee-managers`         |
| Administrasi Kepegawaian     | Surat Keputusan          | `corporatePersonnelActionOrders`     | `/hr/workforce-core/personnel-actions`         |
| Administrasi Kepegawaian     | Surat Tugas              | `corporateOfficialDuties`            | `/hr/workforce-core/official-duties`           |
| Administrasi Kepegawaian     | Surat Keterangan         | `corporateEmployeeDocuments`         | `/hr/workforce-core/employee-documents`        |
| Administrasi Kepegawaian     | Dokumen Administrasi     | `corporateAdministrativeDocuments`   | `/hr/workforce-core/administrative-documents`  |
| Administrasi Kepegawaian     | Pengajuan Perubahan Data | `corporateEmployeeProfileChanges`    | `/hr/workforce-core/employee-profile-changes`  |
| Administrasi Kepegawaian     | Penempatan Organisasi    | `corporateOrganizationAssignments`   | `/hr/workforce-core/organization-assignments`  |
| Administrasi Kepegawaian     | Penempatan Jabatan       | `corporatePositionAssignments`       | `/hr/workforce-core/position-assignments`      |
| Administrasi Kepegawaian     | Riwayat Kepegawaian      | `corporateEmploymentHistories`       | `/hr/workforce-core/employment-histories`      |
| Administrasi Kepegawaian     | Batch Data Awal          | `corporateInitialPlacementBatches`   | `/hr/workforce-core/initial-batches`           |
| Pemberhentian & Pensiun      | Pengunduran Diri         | `corporateResignations`              | `/hr/separation/resignations`                  |
| Pemberhentian & Pensiun      | Pensiun                  | `corporateRetirements`               | `/hr/separation/retirements`                   |
| Pemberhentian & Pensiun      | Akhir Kontrak            | `corporateContractExpiry`            | `/hr/separation/contract-expiry`               |
| Pemberhentian & Pensiun      | Pemberhentian            | `corporateTerminations`              | `/hr/separation/terminations`                  |
| Pemberhentian & Pensiun      | Serah Terima             | `corporateHandovers`                 | `/hr/separation/handovers`                     |
| Pemberhentian & Pensiun      | Clearance                | `corporateExitClearance`             | `/hr/separation/exit-clearance`                |

#### 03. Pengelolaan Tenaga Kerja — 30 entri

| Menu induk dalam kelompok       | Submenu / leaf                    | Key existing                         | Pathname existing                     |
| ------------------------------- | --------------------------------- | ------------------------------------ | ------------------------------------- |
| Penjadwalan & Roster            | Matriks Roster Shift 24/7         | `corporateSchedulingRoster`          | `/hr/scheduling/roster`               |
| Penjadwalan & Roster            | Riwayat Revisi Roster             | `corporateSchedulingRevisions`       | `/hr/scheduling/revisions`            |
| Penjadwalan & Roster            | Penugasan Siaga (On-Call)         | `corporateSchedulingOnCall`          | `/hr/scheduling/on-call`              |
| Penjadwalan & Roster            | Pengaturan Penjadwalan            | `corporateSchedulingSettings`        | `/hr/scheduling/settings`             |
| Presensi & Kehadiran            | Monitoring Presensi Harian        | `corporateAttendanceDaily`           | `/hr/attendance`                      |
| Presensi & Kehadiran            | Pengaturan Presensi               | `corporateAttendanceSetting`         | `/hr/attendance/settings`             |
| Presensi & Kehadiran            | Koreksi Presensi                  | `corporateAttendanceCorrections`     | `/hr/attendance/corrections`          |
| Presensi & Kehadiran            | Izin Pulang Cepat                 | `corporateAttendanceEarlyLeave`      | `/hr/attendance/early-leave`          |
| Presensi & Kehadiran            | Dinas Luar                        | `corporateAttendanceOfficialDuty`    | `/hr/attendance/official-duty`        |
| Presensi & Kehadiran            | Periode Presensi                  | `corporateAttendancePeriods`         | `/hr/attendance/periods`              |
| Presensi & Kehadiran            | Mesin Absen                       | `corporateAttendanceDevices`         | `/hr/attendance/devices`              |
| Cuti & Perizinan                | Persetujuan Cuti Staf             | `corporateLeaveApprovalInbox`        | `/hr/leave/approval-inbox`            |
| Cuti & Perizinan                | Manajemen Saldo Staf              | `corporateLeaveBalancesAdmin`        | `/hr/leave/balances-admin`            |
| Cuti & Perizinan                | Penyesuaian Saldo Cuti            | `corporateLeaveAdjustments`          | `/hr/leave/adjustments`               |
| Cuti & Perizinan                | Pemanggilan Darurat (Recall)      | `corporateLeaveRecalls`              | `/hr/leave/recalls`                   |
| Cuti & Perizinan                | Rekonsiliasi & Payroll Close      | `corporateLeaveReconciliations`      | `/hr/leave/reconciliations`           |
| Manajemen Lembur                | Persetujuan Lembur Ruangan        | `corporateOvertimeApprovals`         | `/hr/overtime/approvals`              |
| Manajemen Lembur                | Verifikasi Presensi & Discrepancy | `corporateOvertimeVerifications`     | `/hr/overtime/verifications`          |
| Manajemen Lembur                | Eskalasi Direktur Medik           | `corporateOvertimeDirectorInbox`     | `/hr/overtime/medical-director-inbox` |
| Manajemen Lembur                | Saldo Cuti Pengganti (CTO)        | `corporateOvertimeCompensatoryLeave` | `/hr/overtime/compensatory-leave`     |
| Manajemen Lembur                | Rekonsiliasi & Payroll Close      | `corporateOvertimeReconciliations`   | `/hr/overtime/reconciliations`        |
| Manajemen Lembur                | Pengaturan Kebijakan Lembur       | `corporateOvertimeSettings`          | `/hr/overtime/settings`               |
| Perjalanan Dinas & Biaya (SPPD) | Dasbor Perjalanan Dinas           | `corporateTravelDashboard`           | `/hrd/travel/dashboard`               |
| Perjalanan Dinas & Biaya (SPPD) | Matriks Tarif & Kebijakan SPPD    | `corporateTravelRatesAndPolicies`    | `/hrd/travel/rates-and-policies`      |
| Perjalanan Dinas & Biaya (SPPD) | Pusat Penugasan & Riwayat SPPD    | `corporateTravelRequests`            | `/hrd/travel/requests`                |
| Perjalanan Dinas & Biaya (SPPD) | Formulir Pengajuan SPPD Baru      | `corporateTravelCreateRequest`       | `/hrd/travel/requests/create`         |
| Perjalanan Dinas & Biaya (SPPD) | Konsol Uang Muka (Advances)       | `corporateTravelAdvances`            | `/hrd/travel/advances`                |
| Perjalanan Dinas & Biaya (SPPD) | Lembar Rekonsiliasi Settlement    | `corporateTravelSettlements`         | `/hrd/travel/settlements`             |
| Perjalanan Dinas & Biaya (SPPD) | Radar Keterlambatan Overdue       | `corporateTravelOverdue`             | `/hrd/travel/overdue-settlements`     |
| Perjalanan Dinas & Biaya (SPPD) | Klaim Biaya Non-Trip Langsung     | `corporateTravelExpenses`            | `/hrd/travel/expenses`                |

#### 04. Pengembangan & Kinerja — 16 entri

| Menu induk dalam kelompok          | Submenu / leaf                   | Key existing                         | Pathname existing                            |
| ---------------------------------- | -------------------------------- | ------------------------------------ | -------------------------------------------- |
| Penilaian Kerja                    | Sasaran Kinerja                  | `corporatePerformanceCycles`         | `/corporate/hrd/performance/cycles`          |
| Penilaian Kerja                    | Indikator Individu               | `corporatePerformanceKpiCatalogs`    | `/corporate/hrd/performance/kpi-catalogs`    |
| Penilaian Kerja                    | Evaluasi Atasan                  | `corporatePerformanceAppraisals`     | `/corporate/hrd/performance/appraisals`      |
| Penilaian Kerja                    | Hasil Penilaian                  | `corporatePerformanceDashboard`      | `/corporate/hrd/performance/dashboard`       |
| Penilaian Kerja                    | Tindak Lanjut                    | `corporatePerformancePip`            | `/corporate/hrd/performance/pip`             |
| Penilaian Kerja                    | Sidang Kalibrasi Manajemen RS    | `corporatePerformanceCalibration`    | `/corporate/hrd/performance/calibration`     |
| Penilaian Kerja                    | Jenjang Karier Klinis PK I-V     | `corporatePerformanceClinicalLadder` | `/corporate/hrd/performance/clinical-ladder` |
| Penilaian Kerja                    | Sanggah Banding Nilai Kinerja    | `corporatePerformanceDisputes`       | `/corporate/hrd/performance/disputes`        |
| Pendidikan & Pelatihan RS (Diklat) | Dasbor Pelatihan & Radar         | `corporateLearningDashboard`         | `/corporate/hrd/learning/dashboard`          |
| Pendidikan & Pelatihan RS (Diklat) | Master Aturan Wajib STARKES      | `corporateMandatoryRulesConfig`      | `/corporate/hrd/learning/mandatory-rules`    |
| Pendidikan & Pelatihan RS (Diklat) | Manajemen Sesi & Kelas Diklat    | `corporateTrainingSessions`          | `/corporate/hrd/learning/sessions`           |
| Pendidikan & Pelatihan RS (Diklat) | Meja Asesmen Teori & OSCE        | `corporateTrainingAssessments`       | `/corporate/hrd/learning/assessments`        |
| Pendidikan & Pelatihan RS (Diklat) | Registri Portofolio e-Sertifikat | `corporateTrainingCertificates`      | `/corporate/hrd/learning/certificates`       |
| Pendidikan & Pelatihan RS (Diklat) | Rencana Tahunan TNA & Anggaran   | `corporateAnnualPlansTna`            | `/corporate/hrd/learning/annual-plans`       |
| Pendidikan & Pelatihan RS (Diklat) | Tata Kelola Ikatan Dinas Retensi | `corporateTrainingBonds`             | `/corporate/hrd/learning/bonds`              |
| Pendidikan & Pelatihan RS (Diklat) | Portofolio 50 SKP Plataran Sehat | `corporateSkpTracker`                | `/corporate/hrd/learning/skp-tracker`        |

#### 05. Kompensasi & Kesejahteraan — 10 entri

| Menu induk dalam kelompok    | Submenu / leaf                  | Key existing                          | Pathname existing                        |
| ---------------------------- | ------------------------------- | ------------------------------------- | ---------------------------------------- |
| (langsung)                   | Data Penggajian Pegawai         | `corporateWorkforcePayrollData`       | `/hr/workforce-core/payroll-data`        |
| Manajemen Manfaat (Benefits) | Dasbor Analitik Manfaat         | `corporateBenefitsDashboard`          | `/corporate/hrd/benefits/dashboard`      |
| Manajemen Manfaat (Benefits) | Katalog Paket Manfaat           | `corporateBenefitPlans`               | `/corporate/hrd/benefits/plans`          |
| Manajemen Manfaat (Benefits) | Kepesertaan Asuransi Pegawai    | `corporateBenefitEnrollments`         | `/corporate/hrd/benefits/enrollments`    |
| Manajemen Manfaat (Benefits) | Enrollment Benefit Pegawai      | `corporateEmployeeBenefitEnrollment`  | `/hr/benefit/enrollments`                |
| Manajemen Manfaat (Benefits) | Persetujuan Perjanjian Angsuran | `corporateHrdInstallmentPlanApproval` | `/hr/benefit/installment-approvals`      |
| Manajemen Manfaat (Benefits) | Piutang Benefit Pegawai         | `corporateEmployeeBenefitReceivables` | `/hr/benefit/receivables`                |
| Manajemen Manfaat (Benefits) | Verifikasi Tanggungan Keluarga  | `corporateBenefitDependents`          | `/corporate/hrd/benefits/dependents`     |
| Manajemen Manfaat (Benefits) | Buku Besar & Hak Berobat RS     | `corporateBenefitInhouseLedger`       | `/corporate/hrd/benefits/inhouse-ledger` |
| Manajemen Manfaat (Benefits) | Meja Kerja Klaim COB            | `corporateBenefitCobClaims`           | `/corporate/hrd/benefits/cob-claims`     |

#### 06. Tata Kelola & Kepatuhan — 29 entri

| Menu induk dalam kelompok               | Submenu / leaf                           | Key existing                                 | Pathname existing                                         |
| --------------------------------------- | ---------------------------------------- | -------------------------------------------- | --------------------------------------------------------- |
| Kompetensi & Kredensial                 | Data STR/ SIP                            | `corporateCredentialingPsv`                  | `/corporate/hrd/credentialing/psv`                        |
| Kompetensi & Kredensial                 | Sertifikasi Kompetensi                   | `corporateCompetencyCertifications`          | `/corporate/hrd/learning/certificates`                    |
| Kompetensi & Kredensial                 | Persyaratan Jabatan                      | `corporateJobRequirements`                   | `/hr/master-data/competency`                              |
| Kompetensi & Kredensial                 | Status Kredensial                        | `corporateCredentialingPeerReview`           | `/corporate/hrd/credentialing/peer-review`                |
| Kompetensi & Kredensial                 | Masa Berlaku Kewenangan                  | `corporateCredentialingRecredentialingCycle` | `/corporate/hrd/credentialing/recredentialing-cycle`      |
| Kompetensi & Kredensial                 | Kalkulator Rapor Komposit 3 Tahun        | `corporateRecredComposite`                   | `/corporate/hrd/credentialing/recred-composite-scorecard` |
| Kompetensi & Kredensial                 | Konsol Sinkronisasi SIMRS EMR            | `corporateRecredSimrsSync`                   | `/corporate/hrd/credentialing/recred-simrs-sync`          |
| Pemantauan Kinerja Klinis               | Dashboard Rapor Kinerja Klinis (OPPE)    | `corporateOppeDashboard`                     | `/corporate/hrd/performance/clinical-oppe`                |
| Pemantauan Kinerja Klinis               | Manajemen Periode & Matriks OPPE         | `corporateOppePeriods`                       | `/corporate/hrd/performance/oppe-periods`                 |
| Pemantauan Kinerja Klinis               | Pusat Deteksi Pemicu FPPE                | `corporateFppeTriggers`                      | `/corporate/hrd/performance/fppe-triggers`                |
| Pemantauan Kinerja Klinis               | Pemantauan Proctoring FPPE               | `corporateFppeProctoring`                    | `/corporate/hrd/performance/fppe-proctoring`              |
| Pemantauan Kinerja Klinis               | Sidang Pleno Putusan FPPE                | `corporateFppeConclusions`                   | `/corporate/hrd/performance/fppe-conclusions`             |
| Hubungan Industrial & Terminasi         | Dasbor Hubungan Industrial & Radar       | `corporateRelationsDashboard`                | `/hrd/relations/dashboard`                                |
| Hubungan Industrial & Terminasi         | Tindak Disiplin                          | `corporateRelationsSanctions`                | `/hrd/relations/sanctions`                                |
| Hubungan Industrial & Terminasi         | Pelanggaran                              | `corporateRelationsIncidents`                | `/hrd/relations/incidents`                                |
| Hubungan Industrial & Terminasi         | Riwayat Penyelesaian                     | `corporateRelationsResolutionHistory`        | `/hrd/relations/sanctions/history`                        |
| Hubungan Industrial & Terminasi         | Skorsing Investigasi (Paid Suspension)   | `corporateRelationsSuspensions`              | `/hrd/relations/suspensions`                              |
| Hubungan Industrial & Terminasi         | Pengaduan Staf & Whistleblowing          | `corporateRelationsGrievances`               | `/hrd/relations/grievance`                                |
| Hubungan Industrial & Terminasi         | Risalah Forum Bipartit RS                | `corporateRelationsBipartite`                | `/hrd/relations/bipartite`                                |
| Hubungan Industrial & Terminasi         | Pengajuan PHK & Pesangon PP 35/2021      | `corporateRelationsSeparations`              | `/hrd/relations/separations`                              |
| Hubungan Industrial & Terminasi         | Exit Clearance Karyawan                  | `corporateEmployeeExitClearance`             | `/hr/exit-clearance`                                      |
| Kesehatan & Keselamatan Kerja RS (K3RS) | Dasbor K3RS & Metrik Zero Accident       | `corporateOhsDashboard`                      | `/hrd/occupational-health/dashboard`                      |
| Kesehatan & Keselamatan Kerja RS (K3RS) | Rekam Kesehatan & Fitness to Work        | `corporateOhsHealthRecords`                  | `/hrd/occupational-health/records`                        |
| Kesehatan & Keselamatan Kerja RS (K3RS) | Pembatasan Tugas (Work Restrictions)     | `corporateOhsWorkRestrictions`               | `/hrd/occupational-health/restrictions`                   |
| Kesehatan & Keselamatan Kerja RS (K3RS) | Konsol Tertusuk Jarum CITO (PEP < 4 Jam) | `corporateOhsNeedleStick`                    | `/hrd/occupational-health/needlestick`                    |
| Kesehatan & Keselamatan Kerja RS (K3RS) | Radar Imunisasi Nakes STARKES            | `corporateOhsVaccinations`                   | `/hrd/occupational-health/vaccinations`                   |
| Kesehatan & Keselamatan Kerja RS (K3RS) | Surveilans Bahaya & Paparan TLD Radiasi  | `corporateOhsSurveillance`                   | `/hrd/occupational-health/surveillance`                   |
| Kesehatan & Keselamatan Kerja RS (K3RS) | Pusat MCU Berkala STARKES KPS 11         | `corporateOhsMcu`                            | `/hrd/occupational-health/mcu`                            |
| Kesehatan & Keselamatan Kerja RS (K3RS) | Registrasi Insiden KAK/PAK & BPJS JKK    | `corporateOhsInjuries`                       | `/hrd/occupational-health/injuries`                       |

#### 07. Perencanaan Strategis SDM — 9 entri

| Menu induk dalam kelompok | Submenu / leaf             | Key existing                                | Pathname existing                                  |
| ------------------------- | -------------------------- | ------------------------------------------- | -------------------------------------------------- |
| (langsung)                | Analisa Beban Kerja        | `corporateWorkforceStaffingWorkloads`       | `/hr/workforce-planning/staffing-workloads`        |
| (langsung)                | Formasi Pegawai            | `corporateWorkforcePositionFormations`      | `/hr/workforce-planning/position-formations`       |
| (langsung)                | Pengajuan Kebutuhan Tenaga | `corporateWorkforceHeadcountRequests`       | `/hr/workforce-planning/headcount-requests`        |
| (langsung)                | Evaluasi Kebutuhan         | `corporateWorkforceStaffingGaps`            | `/hr/workforce-planning/staffing-gaps`             |
| (langsung)                | Pengaturan Perencanaan     | `corporateWorkforcePlanningSetting`         | `/hr/workforce-planning/settings`                  |
| (langsung)                | Batch Formasi Awal         | `corporateWorkforceInitialFormationBatches` | `/hr/workforce-planning/initial-formation-batches` |
| (langsung)                | Standar Kebutuhan Tenaga   | `corporateWorkforceStaffingStandards`       | `/hr/workforce-planning/staffing-standards`        |
| (langsung)                | Antrean Rekrutmen          | `corporateWorkforceRecruitmentQueue`        | `/hr/workforce-planning/recruitment-queue`         |
| (langsung)                | Rencana Tahunan            | `corporateWorkforceManpowerPlans`           | `/hr/workforce-planning/manpower-plans`            |

## 4. Reasoning / execution workflow — task Agent

### FIN-HRD-NAV-FE-001 — Preflight dan snapshot sebelum perubahan

**Prioritas:** P0. **Output:** baseline inventaris dan daftar risiko.

- Pastikan repo/branch FE aktif `HamzahV2`; jalankan inspeksi read-only HEAD, status working tree, dan perbedaan dengan snapshot dokumen. Jangan menimpa perubahan lokal user. BE `MHamzah` hanya referensi.
- Baca `menu-items.jsx` utuh untuk subtree SDM (identifikasi lewat key root existing), beserta renderer aktif, helper breadcrumb, pencarian, permission/role filter, CSS, dan unit test yang disebut di Bab 2. Cari seluruh consumer konfigurasi menu, termasuk adaptasi Payroll, breadcrumb, akses role, menu search, dan persistence expanded state.
- Bentuk baseline mesin (flatten rekursif) untuk setiap leaf aktif: key, label, pathname/href, requiredPermission (termasuk array OR, urutan/semantik), metadata akses lain, parent path dan route keberadaan. Bedakan route aktif dari komentar. Simpan snapshot kelompok di luar scope untuk pembandingan; jangan publish data pribadi.
- Rekonsiliasi 134 key pada Bab 3 dengan source dan 64 entri yang harus tetap utuh. Periksa keberadaan page App Router dan konfigurasi route tanpa membuka data klinis/pegawai. Laporkan route yang memang sudah rusak sebelum perubahan sebagai pre-existing; jangan membuat placeholder page.
- Jalankan baseline test yang relevan bila environment mendukung. Catat kegagalan existing secara terpisah.

### FIN-HRD-NAV-FE-002 — Refactor struktur folder dan pemetaan leaf

**Prioritas:** P0. **Output:** tree target di konfigurasi menu existing.

1. Di `src/utils/menu-sidebar/menu-items.jsx`, ubah hanya subtree SDM sesuai Bab 3. Dashboard SDM berada sebelum Master Data sesuai urutan target; selain urutan relatif tersebut, jangan edit isi kedua node.
2. Buat folder baru `Siklus Kepegawaian`, `Pengelolaan Tenaga Kerja`, `Pengembangan & Kinerja`, `Kompensasi & Kesejahteraan`, `Tata Kelola & Kepatuhan`, dan `Pemantauan Kinerja Klinis` dengan key baru stabil, unik dan konsisten dengan konvensi repository. Nama key baru dipilih Agent setelah pemeriksaan collision; catat dalam laporan. `Perencanaan Strategis SDM` menggunakan grup planning existing, bukan folder tambahan. Pertahankan key grup lama jika memungkinkan untuk node yang tetap ada.
3. Reuse struktur `subMenu`/`subItems` yang telah didukung renderer. Pindahkan object leaf yang sama, tanpa mengubah key, label, route, icon, permission, metadata maupun fungsi. Rename hanya label folder `Onboarding & Kontrak` → `Penerimaan & Onboarding` dan `Perencanaan Kebutuhan SDM` → `Perencanaan Strategis SDM` sesuai struktur disetujui.
4. Jangan menambahkan `pathname` palsu pada folder. Jangan mengubah `requiredPermission` untuk mempermudah akses, tidak menambahkan guard parent yang akan menutupi anak sah, dan tidak menghidupkan kembali Data Induk Pegawai yang dikomentari.
5. Tempatkan Data Penggajian Pegawai sebagai leaf langsung kelompok 05. Jangan membuat duplikat di Administrasi Kepegawaian atau memindah modul Payroll. Pertahankan pengaturan Payroll apa adanya.
6. Pertahankan kotak masuk persetujuan (V2) serta meja bantuan sebagai sibling langsung. Pertahankan semua module lain, termasuk Administrator, ESS dan Side Profile.

### FIN-HRD-NAV-FE-003 — Integritas struktur, owner navigasi, dan kasus khusus

**Prioritas:** P0/P1. **Output:** pembuktian tidak ada leaf hilang atau ganda baru.

- Bandingkan baseline dan hasil dengan multiset key + pathname + label + requiredPermission + metadata. Yang boleh berubah adalah ancestor path folder dan label dua folder yang disetujui; untuk leaf tidak boleh berubah. Semua key leaf ada tepat sesuai jumlah baseline. Hitung 134 entri dipindah, 64 dipertahankan (198 total pada pinned SHA). Jangan menyamakan dua leaf hanya karena pathname sama.
- **Dua alias route existing dipertahankan:** `Kompetensi` di Master Data dan `Persyaratan Jabatan` memakai `/hr/master-data/competency`; `Sertifikasi Kompetensi` dan `Registri Portofolio e-Sertifikat` memakai `/corporate/hrd/learning/certificates`. Jangan menghapus salah satunya, mengganti permission, membuat route baru, atau mengubah halaman untuk menyelesaikan alias. Renderer boleh mempertahankan pemilihan active path canonical/deterministik untuk URL alias; uji kedua pintu masuk dengan hak akses yang sesuai. Jangan menjanjikan dua breadcrumb berbeda dari URL identik tanpa mekanisme existing.
- Pertahankan `Onboarding` (hiring handoff) dan `Kasus Onboarding` (case lifecycle) sebagai dua leaf berbeda. Pertahankan `Clearance` (`/hr/separation/exit-clearance`) dan `Exit Clearance Karyawan` (`/hr/exit-clearance`) karena route/proses berbeda. Pertahankan kedua rekonsiliasi Cuti/Lembur (URL berbeda), kedua enrollment benefit, dan kedua inbox persetujuan.
- OPPE dan FPPE pindah hanya dalam navigasi; lima key klinis ada tepat sekali pada `Pemantauan Kinerja Klinis`. Tidak mengubah kewenangan komite, evaluasi, scoring, credentialing, maupun otorisasi. Test lama HR-08 memang mengunci owner navigasi lama: keputusan pengguna terbaru secara eksplisit menggantikan posisi **sidebar** tersebut, bukan owner domain teknis. Catat perubahan ini sebagai supersession navigasi; bila perlu sinkronisasi dokumen canonical lintas repo, laporkan untuk izin terpisah.

### FIN-HRD-NAV-FE-004 — Renderer, state, breadcrumb, search dan responsive

**Prioritas:** P1. **Output:** navigasi bertingkat usable tanpa perubahan desain halaman.

- Mulai dengan konfigurasi saja; renderer saat ini rekursif dan mendukung depth hingga 8. Struktur baru menggunakan kedalaman setara Master Data bertingkat (modul → kelompok → menu → leaf). Tidak perlu membangun renderer baru. Hanya perbaiki helper/CSS bila uji menunjukkan defect nyata.
- Pastikan klik folder membuka/menutup anak tanpa navigasi atau API transaksi; klik leaf membuka route existing. Back/forward, reload dan direct deep link membuka ancestor yang tepat dan menandai satu leaf aktif sesuai mekanisme existing. Uji URL list, detail dan create yang sudah ada; exact/longest-prefix matching tetap berfungsi. Breadcrumb dan path helper harus menampilkan rantai baru bila mereka mengonsumsi tree sidebar; jangan ubah breadcrumb statis halaman di luar scope tanpa bukti.
- Pencarian sidebar menggunakan tree yang telah difilter permission; cari label dan pathname di kedalaman baru. Hasil harus membuka ancestor dan dapat diklik. Clear search mengembalikan navigasi normal. Jangan tampilkan leaf terlarang dalam hasil pencarian.
- Expanded state berbasis path key dapat berubah karena parent baru. Gunakan mekanisme existing yang menggabungkan active ancestor dengan sessionStorage; uji state lama, refresh, pergantian role/session, key stale, serta perubahan route. Jangan hapus storage global atau membuat storage sensitif baru; bila perlu sanitasi/migrasi key, batasi pada state navigasi terkait.
- Pertahankan warna teal, logo, typography, base component, radius, spacing, sidebar mini/drawer, icon pattern, footer dan page content. Uji label panjang pada kedalaman tambahan; tidak overlap, clipping yang menghilangkan arti, scroll horizontal atau tertutup footer. Uji scroll vertikal sampai item terakhir.
- Verifikasi desktop lebar, desktop sempit, tablet dan mobile drawer (mis. 1440×900, 1024×768, 768×1024, 390×844); gunakan breakpoint existing, jangan mengganti breakpoint global. Fokus keyboard terlihat, tombol folder dapat dioperasikan Enter/Space, `aria-expanded` sesuai state dan link aktif memakai `aria-current` jika mekanisme existing mendukung. Perbaiki regresi secara lokal.

### FIN-HRD-NAV-FE-005 — Pertahankan permission dan isolasi perubahan

**Prioritas:** P0. **Output:** hasil parity per permission/role.

- Jangan mengubah daftar `requiredPermission`, action name, resource, OR-array, effective permission hook, backend auth guard atau AccessDeniedGate hanya untuk membuat menu terlihat.
- Untuk kombinasi izin sama sebelum/sesudah, himpunan key leaf yang terlihat harus sama. Folder kosong tanpa route tidak tampil. Permission loading, unknown, denied atau gagal tetap fail-closed pada item gated; jangan memunculkan item terlarang sesaat.
- Uji full permission (akun test berwenang), partial (mis. HR hanya rekrutmen atau kontrak), role kepala unit, komite klinis, payroll dan pegawai ESS-only, serta no permission untuk suatu subtree. Role aktual dan haknya berasal dari fixture/akun test sah, bukan asumsi nama jabatan. Permission API tetap menentukan akses. Jangan mengubah role asli atau membuka data sensitif untuk pengujian.
- Screenshot user sebelumnya memperlihatkan pesan `Ups! Akses Ditolak`; itu merupakan gejala existing yang belum dibuktikan akar masalahnya. Scope ini tidak memperbaiki permission akun tersebut. Jika muncul pada role yang sebelumnya berhak, investigasi apakah regresi navigasi atau pre-existing, rekam evidence, jangan bypass guard. Direct URL terlarang tetap ditolak sesuai baseline.
- Bandingkan subtree Administrator, Master Data SDM, Payroll, ESS, Side Profile, modul lain, serta inbox dan helpdesk. Isi dan kontrol akses harus identik. Diff source hanya area yang berizin.

### FIN-HRD-NAV-FE-006 — Unit test, lint, build dan regression

**Prioritas:** P0/P1. **Output:** bukti command dan hasil aktual.

- Pertahankan test `HR-01` dan `HR-02` Master Data tanpa pelonggaran. Audit seluruh test yang mengandalkan posisi grup lama (tidak terbatas HR-03/07/08); ubah lookup langsung `hrMenu.subMenu.find(...)` menjadi pencarian tree rekursif bila sesuai, dan assertion parent path yang baru. `HR-07`: handoff berada di Penerimaan & Onboarding, planning tetap sembilan leaf dalam folder rename. `HR-08`: delapan Penilaian Kerja, lima OPPE/FPPE tepat sekali di Pemantauan Kinerja Klinis, bukan disalin ke Kompetensi & Kredensial. Semua invariant key, label leaf, permission dan route harus tetap diuji.
- Periksa assertion test lain seperti HR-09 (alias sementara), seluruh test `hr-master-data-sidebar-grouping`, `menu-permission-filter`, `self-service-menu-dedup`, `sidebar-scroll` serta modul lain yang memakai helper bersama. Jangan sekadar mengubah expected count, menghapus test gagal, atau skip untuk mencapai PASS.
- Tambahkan/uji menggunakan runner existing: manifest target 134 key, seluruh parent path, no missing/extra key, no new duplicate, parity permission termasuk OR array, empty group pruning, Master Data dan ESS tidak berubah, search/active path/breadcrumb bila helper dapat diuji. Tidak menambah dependency/framework.
- Jalankan command sesuai `AGENTS.md` dan policy serta environment berizin. Contoh command existing:

```bash
# Node test terfokus (sesuaikan daftar file yang benar-benar berubah)
node --import ./tests/helpers/register.mjs --test tests/unit/hr-master-data-sidebar-grouping.test.mjs tests/unit/menu-permission-filter.test.mjs tests/unit/self-service-menu-dedup.test.mjs tests/unit/sidebar-scroll.test.mjs
npm run lint:errors
npm run test:unit
npm run build
```

- Catat command, exit code, jumlah test, error/warning, serta build artifact bila diizinkan. Jalankan lint file berubah sesuai standar repo. Full build dan unit test tidak berarti UAT live PASS. `npm run test:e2e` hanya bila diperlukan dan environment mendukung; jangan jalankan `npm run test:uat` otomatis tanpa mandat eksplisit. Jika ada larangan build atau resource lingkungan, ikuti dan laporkan `BLOCKED/NOT RUN` beserta alasan nyata. Jangan ubah konfigurasi build/version/dependency untuk menutupi kegagalan.

### FIN-HRD-NAV-FE-007 — UAT navigasi browser dan handoff

**Prioritas:** P0/P1. **Output:** hasil uji browser nyata + satu laporan paket.

Uji navigasi pada environment development/test yang berizin dan build yang sesuai. Gunakan browser nyata/headed jika tersedia sesuai governance testing; tidak melakukan transaksi HR, approval, mutasi DB, penggajian atau offboarding nyata. Jangan menguji dengan data/kredensial pribadi yang tidak diperlukan. UAT di bawah mencakup navigasi dan regression, bukan UAT seluruh proses Employee Lifecycle. Bila perlu tester manusia, dokumentasikan langkah yang belum dijalankan, jangan klaim klik atau screenshot fiktif.

| ID         | Kategori | Langkah / skenario                                                                      | Expected                                                                                                       |
| ---------- | -------- | --------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------- |
| NAV-UAT-01 | P0       | Login akun test berwenang, buka SDM                                                     | Dashboard, Master Data, 02–07, inbox dan helpdesk dalam urutan target; hanya yang berhak terlihat              |
| NAV-UAT-02 | P0       | Expand semua kelompok, telusuri manifest 134 leaf                                       | Semua entri yang diizinkan ada pada parent benar; tidak ada leaf hilang/ganda baru                             |
| NAV-UAT-03 | P0       | Buka setiap leaf 02–07 yang berhak; catat key/URL/hasil                                 | URL sesuai manifest; halaman existing terbuka, tidak ada 404/403 baru, tidak ada crash; alias dicatat terpisah |
| NAV-UAT-04 | P0       | Akun partial permission dan akses OR-array                                              | Himpunan leaf visible sama dengan baseline; grup tanpa akses hilang                                            |
| NAV-UAT-05 | P0       | Akun tanpa izin subtree, search leaf terlarang dan direct URL                           | Tidak bocor di sidebar/search; direct access tetap ditolak sesuai otorisasi existing                           |
| NAV-UAT-06 | P0       | Buka Ruang Kerja Saya dari sidebar dan Side Profile                                     | `/self-services` dan tab/actions existing tidak berubah; tanpa ESS duplikat di SDM                             |
| NAV-UAT-07 | P0       | Buka Master Data SDM dan Administrator                                                  | Subtree, label, route, izin dan urutan internal existing tetap                                                 |
| NAV-UAT-08 | P0       | Buka Payroll dan modul lain yang memakai sidebar                                        | Tidak ada regresi label, route, akses, atau tampilan                                                           |
| NAV-UAT-09 | P1       | Siklus → Rekrutmen; Onboarding; Kontrak                                                 | Handoff dan case berbeda, batch kontrak di Kontrak; semua route cocok                                          |
| NAV-UAT-10 | P1       | Buka Data Penggajian Pegawai melalui Kompensasi                                         | Route existing; tidak ada leaf tersebut di Administrasi Kepegawaian; Payroll tetap modul terpisah              |
| NAV-UAT-11 | P1       | Buka lima menu OPPE/FPPE dengan role yang sah                                           | Seluruh leaf hanya berada di Pemantauan Kinerja Klinis; permission tetap                                       |
| NAV-UAT-12 | P1       | Buka kedua pintu alias kompetensi/sertifikat                                            | URL dan akses sesuai baseline; active state deterministik; tidak ada halaman baru                              |
| NAV-UAT-13 | P1       | Buka Clearance dan Exit Clearance Karyawan, kedua inbox, kedua rekonsiliasi             | Route berbeda tetap dipertahankan, tidak tertukar                                                              |
| NAV-UAT-14 | P1       | Cari `Perjanjian Kerja`, `Pengajuan Kebutuhan Tenaga`, `OPPE`, `MCU` dan pathname       | Hasil berhak tampil, ancestor terbuka, link tepat; clear search normal                                         |
| NAV-UAT-15 | P1       | Klik leaf, refresh, back/forward, buka direct URL detail existing                       | Ancestor dan leaf aktif benar; breadcrumb sesuai tree bila dinamis; tidak reset halaman                        |
| NAV-UAT-16 | P1       | Buka beberapa folder, tutup, reload dengan state lama                                   | State navigasi bekerja sesuai pola existing; tidak ada crash dari key lama                                     |
| NAV-UAT-17 | P1       | Uji lebar 1440, 1024, 768, 390 px; mini/drawer                                          | Label dapat dibaca, scroll mencapai leaf terakhir, tidak overflow horizontal, navigasi drawer berfungsi        |
| NAV-UAT-18 | P1       | Navigasi keyboard dan focus                                                             | Folder dapat dioperasikan, fokus terlihat, status expand/active sesuai                                         |
| NAV-UAT-19 | P1       | Bandingkan screenshot halaman existing sebelum/sesudah (contoh kontrak, rekrutmen, ESS) | Konten, form, warna dan layout halaman bisnis tidak berubah                                                    |
| NAV-UAT-20 | P0       | Review final diff dan hasil regression                                                  | Tidak ada perubahan backend/DB, route/permission, ESS/Payroll/Master Data; semua test relevan tercatat         |

Untuk NAV-UAT-03, buat rekap per leaf `key | expected route | actual route | role test | PASS/FAIL/BLOCKED/NOT RUN | evidence`. Jangan menyatakan 134/134 PASS hanya dari 20 skenario ringkas. Jika akun tidak memiliki izin untuk semua leaf, tandai coverage yang belum diuji. Gunakan data fiktif atau halaman daftar read-only; hindari screenshot berisi data personal pegawai. Jangan memberikan akses baru untuk mengejar coverage.

## 5. Stop conditions / acceptance criteria

Paket dianggap selesai untuk scope navigasi jika semua kriteria berikut terpenuhi dengan evidence. Status `PASS` hanya sesudah uji aktual; jika blocked, sebutkan kriteria yang belum terbukti.

| AC    | Kriteria objektif                                                                                                                                                                       |
| ----- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| AC-01 | Hierarki SDM tepat mengikuti Bab 3; Dashboard SDM di atas Master Data, tanpa angka 01–07 pada label UI.                                                                                 |
| AC-02 | Snapshot 198 entri aktif/196 pathname unik dipertahankan; 134 leaf scope memiliki parent target, 64 leaf lain utuh. Jika HEAD drift, rekonsiliasi semua delta sebelum menyatakan lulus. |
| AC-03 | Tidak ada perubahan key, label leaf, pathname/href, requiredPermission/metadata akses dan icon leaf. Tidak ada duplikasi baru atau fitur hilang.                                        |
| AC-04 | Master Data SDM tepat 10 kelompok dan 55 leaf baseline; isi internal, urutan, key, label dan izin identik.                                                                              |
| AC-05 | Administrator, Payroll, ESS, Side Profile, modul lain, inbox dan helpdesk tetap existing. Tidak ada ESS baru, dashboard baru, landing page baru atau salinan Payroll.                   |
| AC-06 | Lima OPPE/FPPE hanya berada di Pemantauan Kinerja Klinis; tidak ada perubahan business logic/clinical permission.                                                                       |
| AC-07 | Handoff onboarding, batch kontrak dan data penggajian berada di lokasi baru yang tepat; alias dan route berbeda existing tidak digabung.                                                |
| AC-08 | Permission parity: full, partial, OR-array dan denied; parent kosong hilang; pencarian tidak membocorkan item terlarang; direct URL tetap dijaga.                                       |
| AC-09 | Expand/collapse, search, active ancestor, deep link, refresh, back/forward, breadcrumb dan session state bekerja tanpa regresi.                                                         |
| AC-10 | Desktop/mini/drawer/mobile tidak mengalami clipping, overlap atau overflow yang menghalangi navigasi; keyboard dan fokus berfungsi.                                                     |
| AC-11 | Tidak ada redesain halaman existing, perubahan API, service transaksi, workflow, authorization, entity, migrasi atau database.                                                          |
| AC-12 | Test lama disesuaikan hanya untuk perubahan navigasi disetujui; test invariant Master Data, ESS, permission dan modul lain tetap terjaga.                                               |
| AC-13 | Lint, unit test, build sesuai governance dan verifikasi browser dilaporkan dengan command/evidence aktual, termasuk kegagalan/pre-existing/blocker.                                     |
| AC-14 | Agent menyerahkan diff ringkas, mapping sebelum/sesudah, dan satu laporan penyelesaian paket. Tidak commit/push/deploy otomatis.                                                        |

**Hentikan bagian terdampak** bila branch/otorisasi tidak sah, canonical governance tidak tersedia, perubahan memerlukan backend/permission/schema baru, route hilang yang tidak dapat direkonsiliasi, atau ditemukan perubahan yang bertentangan dengan scope. Selesaikan pekerjaan independen yang tetap aman. Jangan menyamarkan blocker dengan mock UI, placeholder route, permission bypass, atau test palsu.

## 6. Output Agent

Setelah benar-benar mengerjakan source, berikan satu laporan `LAPORAN_IMPLEMENTASI_RESTRUKTURISASI_MENU_SDM.md` melalui lokasi yang diizinkan (ikuti aturan tracked-report jika berlaku; jangan menulis lintas repo tanpa otorisasi). Isi minimum:

1. Repo, branch, SHA awal/akhir working tree, mode dan target tulis; daftar file berubah; status canonical governance/runtime skill.
2. Daftar task FIN-HRD-NAV-FE-001–007 dan status per task (`DONE`, `PARTIAL`, `BLOCKED`, `NOT RUN`); root cause navigasi dan perubahan aktual, bukan klaim generik.
3. Hierarki akhir, hasil perbandingan manifest per key/route/permission, jumlah entri sebelum/sesudah, dua alias existing, dan delta jika HEAD berubah; konfirmasi Master Data 55 leaf tetap.
4. Bukti bahwa Payroll, Administrator, ESS, Side Profile, inbox, helpdesk, route dan permission tidak berubah; catat key folder baru dan consumer terkait yang diperbarui.
5. Ringkasan lint/unit/build dengan command, exit code dan hasil; jelaskan test lama yang disesuaikan karena parent path baru; pisahkan baseline failure dari regresi.
6. Matriks 20 UAT navigasi, rekap per leaf, role/permission test tanpa kredensial, screenshot yang tidak mengungkap data personal; daftar NOT RUN/BLOCKED.
7. Matriks AC-01–14 dengan PASS/FAIL/BLOCKED/NOT RUN dan bukti. Catat keputusan tambahan, keterbatasan dan defect yang harus ditindaklanjuti terpisah.
8. Konfirmasi tidak ada commit/push/deploy, perubahan DB/role, atau perubahan source BE tanpa izin.

**Handoff berikutnya (di luar scope):** setelah navigasi dinyatakan siap, lanjutkan UAT proses Employee Lifecycle melalui Perencanaan Strategis SDM → Pengajuan Kebutuhan Tenaga → Requisition → Rekrutmen & Seleksi → Penerimaan & Onboarding → Kontrak → Penempatan → perubahan selama masa kerja → pemberhentian. Uji master/permission yang diperlukan terlebih dahulu. Ini bukan mandat menambahkan/memperbaiki transaksi bisnis pada paket sidebar.

## Lampiran — Referensi pinned

- [Konfigurasi menu FE](https://github.com/DevBenari/QuilvianSystemFrontendDev/blob/32d67d384cf7f63ff1ad424a0206c53a15fdfcd8/src/utils/menu-sidebar/menu-items.jsx)
- [Renderer sidebar FE](https://github.com/DevBenari/QuilvianSystemFrontendDev/blob/32d67d384cf7f63ff1ad424a0206c53a15fdfcd8/src/components/features/left-sidebar/left-sidebar-items-virtualized.jsx)
- [Helper navigasi](https://github.com/DevBenari/QuilvianSystemFrontendDev/blob/32d67d384cf7f63ff1ad424a0206c53a15fdfcd8/src/components/features/left-sidebar/left-sidebar-menu-handle.jsx)
- [Filter permission](https://github.com/DevBenari/QuilvianSystemFrontendDev/blob/32d67d384cf7f63ff1ad424a0206c53a15fdfcd8/src/utils/menu-sidebar/permission/filter-menu-items-by-permission.jsx)
- [Test grouping HR](https://github.com/DevBenari/QuilvianSystemFrontendDev/blob/32d67d384cf7f63ff1ad424a0206c53a15fdfcd8/tests/unit/hr-master-data-sidebar-grouping.test.mjs)
- [AGENTS.md FE](https://github.com/DevBenari/QuilvianSystemFrontendDev/blob/32d67d384cf7f63ff1ad424a0206c53a15fdfcd8/AGENTS.md)
- [Business Overview HRD — referensi BE](https://github.com/DevBenari/NewQuilvianSystemBackend/blob/38b614eff7f27da3095a1c9b1cf31926e63d072f/docs/module-blueprints/human-resource/00-business-overview.md)
- [Global Rules](https://github.com/DevBenari/QuilvianEngineeringSkills/blob/62b7ead4595161ec8f9bb7fc2dd96b3a7b29f2a5/agents/rules/GLOBAL_RULES.md)
- [Frontend test policy](https://github.com/DevBenari/QuilvianEngineeringSkills/blob/62b7ead4595161ec8f9bb7fc2dd96b3a7b29f2a5/agents/rules/frontend/test-policy.md)
