# Planning Penyesuaian Menu SDM Berdasarkan Analisis Bisnis

## 1. Metadata dokumen

| Field | Nilai |
| --- | --- |
| Modul | Human Resource / Sumber Daya Manusia |
| Blueprint | `HRD-BP-001` |
| Blueprint shape | `SINGLE` |
| Jenis dokumen | Planning dan rekam hasil penyesuaian navigasi frontend; bukan roadmap task |
| Status | `PARTIAL / VERIFYING` — Paket A telah diimplementasikan; verifikasi browser dan permission runtime belum tersedia; Paket B tetap ditunda |
| Revision dokumen | `3` — mencatat persetujuan, implementasi Paket A, bukti validasi, serta konflik kontrak Tindak Disiplin |
| Tanggal | 20 November 2026 |
| Masukan bisnis | Daftar 12 menu dan 61 submenu dari Business Analyst yang disampaikan pengguna pada 20 November 2026 |
| Referensi visual | Capture sidebar SDM yang memuat Diklat, Manajemen Kinerja & Karier, Hubungan Industrial & Terminasi, K3RS, SPPD, dan Layanan Mandiri |
| Source frontend hasil implementasi | `QuilvianSystemFrontendDev@51853893e9f229f0f22f5e86bc5f2e8c08946779` (`HamzahV2`) |
| Source backend yang diperiksa untuk konflik disiplin | `NewQuilvianSystemBackend@a7c37c56052ee390aa8f222edbd143736dd20786` (`MHamzah`) |
| Target source saat implementasi | `QuilvianSystemFrontendDev/src/utils/menu-sidebar/menu-items.jsx` |
| Persetujuan | Disetujui pengguna pada 20 November 2026; pengguna memerintahkan implementasi code terlebih dahulu dan pembaruan blueprint setelahnya |
| Batas implementasi | Hanya susunan menu dan test unit terkait; tidak menambah API, tabel, migration, atau workflow backend |

> **Catatan pemutakhiran:** revision 3 merekonsiliasi planning dengan commit frontend hasil
> implementasi. Blueprint manifest diperbarui secara terbatas; roadmap tidak diubah karena pengguna
> meminta pembaruan module blueprint setelah implementasi code.

## 2. Tujuan

Planning ini menyiapkan penyesuaian navigasi **Sumber Daya Manusia** agar 12 kelompok kebutuhan
Business Analyst tercakup tanpa menggandakan menu, halaman, data, atau workflow yang sudah ada.
Daftar BA diperlakukan sebagai **register kebutuhan fungsi**, bukan perintah untuk membuat 12
parent baru secara harfiah.

Perubahan yang direncanakan berfokus pada susunan `menu-items.jsx`. Planning ini tidak memberi
wewenang untuk:

1. menghapus halaman existing;
2. menghapus route atau mengubah kontrak API;
3. mengubah workflow bisnis backend;
4. membuat tabel atau migration;
5. mengubah menu di luar kebutuhan Business Analyst; atau
6. mengklaim menu siap dipakai sebelum lint, build, pemeriksaan permission, dan uji browser selesai.

## 3. Prinsip utama penyesuaian

### 3.1 Kebutuhan Business Analyst menjadi peta cakupan fungsi

Di dalam menu induk **Sumber Daya Manusia**, 12 kelompok berikut menjadi referensi cakupan bisnis.
Satu kelompok boleh dipenuhi oleh menu existing apabila fungsi, data, dan workflow-nya memang sama:

1. Dashboard SDM
2. Perencanaan Kebutuhan SDM
3. Rekrutmen & Seleksi
4. Data Induk Pegawai
5. Struktur Organisasi & Jabatan
6. Administrasi Kepegawaian
7. Penempatan, Mutasi & Promosi
8. Kontrak & Masa Kerja
9. Kompetensi & Kredensial
10. Penilaian Kerja
11. Disiplin & Pembinaan
12. Pemberhentian & Pensiun

Konsekuensinya, jumlah parent fisik setelah penyesuaian tidak wajib tepat 12. Contohnya,
**Data Induk Pegawai** dipenuhi oleh **Master Data → Karyawan/Dokter**, sedangkan **Struktur
Organisasi & Jabatan** tetap dipenuhi oleh kelompok Master Data organisasi yang sudah ada.

### 3.2 Menu di luar kebutuhan Business Analyst tidak dihapus

Menu existing yang tidak disebut dalam kebutuhan Business Analyst tetap dipertahankan. Label,
`key`, `pathname`, `requiredPermission`, dan urutan relatifnya tidak boleh berubah hanya karena
perombakan 12 menu.

Berdasarkan capture pengguna, perlakuannya adalah:

| Menu pada capture | Hubungan dengan kebutuhan BA | Rencana |
| --- | --- | --- |
| Pendidikan & Pelatihan RS (Diklat) | Tidak termasuk 12 menu BA | **Dibiarkan utuh** |
| Manajemen Kinerja & Karier | Sepadan dengan Menu 10 Penilaian Kerja | **Masuk scope penyesuaian**; parent dan lima fungsi utama diselaraskan, fungsi karier tambahan tetap dipertahankan |
| Hubungan Industrial & Terminasi | Sebagian berkaitan dengan Menu 11 dan 12 | **Disentuh terbatas**; fungsi disiplin/terminasi dipetakan tanpa menghapus fungsi hubungan industrial lain |
| Kesehatan & Keselamatan Kerja RS (K3RS) | Tidak termasuk 12 menu BA | **Dibiarkan utuh** |
| Perjalanan Dinas & Biaya (SPPD) | Tidak termasuk 12 menu BA | **Dibiarkan utuh** |
| Layanan Mandiri | Tidak termasuk 12 menu BA | **Dibiarkan utuh** |

Menu lain yang juga berada di luar 12 kelompok BA—misalnya Presensi & Kehadiran, Penjadwalan &
Roster, Cuti & Perizinan, Manajemen Lembur, Manajemen Manfaat, Layanan SDM & Meja Bantuan, dan
Kotak Masuk Persetujuan—juga tidak boleh dihapus atau dirombak pada pekerjaan ini.

### 3.3 Tidak membuat data atau halaman ganda

Jika halaman existing sudah memenuhi kebutuhan, implementasi hanya memindahkan atau mengganti
label menu yang menunjuk ke route tersebut. Implementasi tidak boleh membuat halaman kedua dengan
data yang sama.

Contoh:

- Lowongan memakai `/hr/recruitment/vacancies`; tidak membuat daftar lowongan baru.
- Data Induk Pegawai memakai detail dan pengelolaan profil pada Master Data Karyawan/Dokter;
  enam route façade Data Induk tidak diaktifkan sebagai sidebar kedua.
- Pemberhentian memakai flow separation existing; tidak membuat record terminasi kedua.
- Atasan Langsung membaca `WfpManagerAssignment`; tidak membuat tabel hierarki pegawai baru.

### 3.4 Satu route memiliki satu penempatan utama

Satu `pathname` tidak boleh tampil sebagai dua menu utama dengan makna yang sama. Shortcut yang
memang dibutuhkan harus diberi label jelas dan tidak boleh terlihat sebagai dua sumber data.

Contoh konflik existing yang perlu diselesaikan saat implementasi:

- `/hr/workforce-core/manager-assignments` saat ini muncul sebagai **Relasi Atasan** pada
  Administrasi Kepegawaian dan **Atasan Langsung** pada Master Data.
- Konsep Uraian Tugas muncul sebagai master jabatan dan uraian efektif milik pegawai.
- OPPE/FPPE saat ini berada di Kompetensi & Kredensial, padahal sebagian fungsinya merupakan
  evaluasi kinerja klinis.

## 4. Ringkasan kondisi source saat ini

| Kebutuhan | Status kemampuan | Bukti source saat ini | Implikasi planning |
| --- | --- | --- | --- |
| Dashboard SDM terpadu | `MISSING` | Belum ada parent/route Dashboard SDM terpadu pada `menu-items.jsx` | Membutuhkan route, view, permission, dan kontrak ringkasan yang dirancang terpisah |
| Perencanaan Kebutuhan SDM | `REUSE WITH ADAPTER` | Grup `Perencanaan Tenaga Kerja` dan sembilan route planning sudah ada | Rename, reorder, dan petakan empat kebutuhan BA; fungsi tambahan tetap ada |
| Rekrutmen & Seleksi | `REUSE WITH ADAPTER` | Grup `Pengadaan & Rekrutmen`, route lowongan, kandidat, evaluasi, dan handoff sudah ada | Lima label BA menjadi entry point utama tanpa menggandakan workflow |
| Data Induk Pegawai | `READY TO REUSE` | Route Data Induk mengimpor kembali komponen detail/pengelolaan milik Master Data Karyawan dan membaca endpoint workforce yang sama | Master Data Karyawan/Dokter menjadi pemilik utama; blok sidebar Data Induk tetap nonaktif |
| Struktur Organisasi & Jabatan | `READY TO REUSE` | Struktur, unit, rumpun, uraian, dan shortcut atasan sudah berada di Master Data | Pertahankan pada Master Data; jangan membentuk parent kedua |
| Administrasi Kepegawaian | `REUSE WITH ADAPTER` | Parent dan fungsi surat/SK/dokumen/perubahan data sudah ada | Tempatkan lima kebutuhan BA sebagai item utama; item tambahan existing tidak dihapus |
| Penempatan, Mutasi & Promosi | `READY TO REUSE` | Lima submenu BA sudah ada | Pertahankan route; hanya validasi duplikasi dan permission |
| Kontrak & Masa Kerja | `READY TO REUSE` | Empat submenu BA sudah ada | Pertahankan route dan urutan |
| Kompetensi & Kredensial | `REUSE WITH ADAPTER` | Lima submenu BA sudah ada ditambah OPPE/FPPE dan fungsi teknis lain | Pertahankan lima item utama; pindahkan hanya item yang jelas dimiliki Penilaian Kerja |
| Penilaian Kerja | `REUSE WITH ADAPTER` | Grup `Manajemen Kinerja & Karier` beserta dashboard, KPI, cycle, appraisal, PIP, karier, dan senggah sudah ada | Rename parent dan petakan lima label BA; item karier tambahan tetap ada |
| Disiplin & Pembinaan | `REUSE WITH ADAPTER` | Fungsi sudah berada di `Hubungan Industrial & Terminasi` dan disciplinary action | Petakan kebutuhan BA di dalam owner existing; jangan membentuk parent disiplin kedua |
| Pemberhentian & Pensiun | `READY TO REUSE` | Enam submenu sudah ada | Pertahankan route dan perbaiki label `Clearense` menjadi `Clearance` |

## 5. Pemetaan kebutuhan BA ke owner navigasi dan route

Status pada tabel menggunakan taxonomy kemampuan canonical. `Rencana baru` berarti route target
belum boleh dianggap tersedia sebelum task desain dan implementasi terpisah disetujui.

### 5.1 Kebutuhan BA 01 — Dashboard SDM

| Urutan | Label target | Route/kemampuan target | Status | Tindakan planning |
| ---: | --- | --- | --- | --- |
| 1 | Ringkasan Data Pegawai | Dashboard agregat pegawai | `MISSING` | Rancang halaman ringkasan tanpa menyalin daftar Data Induk Pegawai |
| 2 | Distribusi Profesi | Agregasi profesi aktif | `MISSING` | Rancang grafik/tabel yang membaca sumber profesi existing |
| 3 | Pegawai Aktif/Nonaktif | Agregasi status workforce | `MISSING` | Gunakan definisi status backend, bukan menghitung dari label frontend |
| 4 | Kontrak Berakhir | Agregasi kontrak mendekati berakhir | `EXTEND` | Reuse data Kontrak & Masa Kerja dan separation contract expiry |
| 5 | Dokumen Kadaluarsa | Agregasi STR/SIP/sertifikat/dokumen yang memiliki masa berlaku | `EXTEND` | Tetapkan jenis dokumen dan horizon waktu sebelum implementasi |

Rekomendasi route frontend: satu landing page `/hr/dashboard` dengan lima kartu/panel, bukan lima
halaman daftar baru. Rekomendasi ini masih `DRAFT`; nama route final harus dikunci saat task
frontend disetujui.

**Dampak API:** belum ada perubahan API yang disetujui. Implementasi Dashboard SDM harus diawali
audit endpoint agregat existing. Jika belum tersedia, kontrak endpoint ringkasan wajib dirancang
dan disetujui lebih dahulu; dokumen ini tidak mengarang URL, DTO, atau permission backend.

### 5.2 Kebutuhan BA 02 — Perencanaan Kebutuhan SDM

| Urutan | Label target BA | Route existing yang direkomendasikan | Status | Catatan |
| ---: | --- | --- | --- | --- |
| 1 | Analisa Beban Kerja | `/hr/workforce-planning/staffing-workloads` | `READY TO REUSE` | Rename dari Angka Beban; tautkan juga ke standar kebutuhan bila dibutuhkan dalam halaman |
| 2 | Formasi Pegawai | `/hr/workforce-planning/position-formations` | `READY TO REUSE` | Rename dari Formasi Jabatan |
| 3 | Pengajuan Kebutuhan Tenaga | `/hr/workforce-planning/headcount-requests` | `READY TO REUSE` | Rename dari Permintaan Headcount |
| 4 | Evaluasi Kebutuhan | `/hr/workforce-planning/staffing-gaps` | `READY TO REUSE` | Rename dari Pemantauan Gap Tenaga |

Item existing `Pengaturan Perencanaan`, `Batch Formasi Awal`, `Standar Kebutuhan Tenaga`, `Antrean
Rekrutmen`, dan `Rencana Tahunan` tidak dihapus. Item tersebut tetap berada setelah empat item BA
atau dipertahankan sebagai fungsi lanjutan sesuai kemampuan sidebar existing.

### 5.3 Kebutuhan BA 03 — Rekrutmen & Seleksi

| Urutan | Label target BA | Route/entry point | Status | Catatan ownership |
| ---: | --- | --- | --- | --- |
| 1 | Lowongan | `/hr/recruitment/vacancies` | `READY TO REUSE` | Reuse Lowongan Kerja |
| 2 | Pelamar | `/hr/recruitment/candidates` | `READY TO REUSE` | Reuse Bank Pelamar |
| 3 | Seleksi Administrasi | Worklist kandidat berdasarkan tahap administrasi | `EXTEND` | Jangan memakai route detail dinamis sebagai menu; tentukan apakah filter kandidat cukup atau perlu route worklist |
| 4 | Tes/Wawancara | Worklist evaluasi HR dan klinis | `EXTEND` | Reuse route evaluasi detail existing; sediakan entry point daftar yang stabil |
| 5 | Onboarding | `/hr/recruitment/handoffs` | `REUSE WITH ADAPTER` | Artinya serah terima kandidat lulus ke lifecycle, bukan membuat kasus onboarding kedua |

Item `Permintaan Requisition`, `Surat Penawaran`, `Pemantauan MCU Pra-Kerja`, dan `Talent Pool`
tetap dipertahankan sebagai fungsi existing tambahan. Parent `Onboarding & Kontrak` existing juga
tidak dihapus karena parent tersebut mengelola pelaksanaan onboarding setelah handoff.

### 5.4 Kebutuhan BA 04 — Data Induk Pegawai

Kebutuhan ini **tidak menjadi parent sidebar baru**. Pemilik utamanya tetap **Master Data →
Karyawan** dan **Master Data → Dokter** karena halaman Data Induk yang dibuat sebelumnya memakai
komponen, hook, state, dan endpoint workforce yang sama.

| Urutan BA | Informasi yang dibutuhkan | Pemilik fungsi existing | Status | Keputusan navigasi |
| ---: | --- | --- | --- | --- |
| 1 | Profil Pegawai | Detail Karyawan/Dokter pada Master Data | `READY TO REUSE` | Buka detail dari daftar Karyawan atau Dokter |
| 2 | Keluarga | Tab/resource `familyMembers` pada profil workforce | `READY TO REUSE` | Tetap menjadi tab/detail pegawai; bukan submenu global |
| 3 | Pendidikan | Tab/resource `educations` pada profil workforce | `READY TO REUSE` | Tetap menjadi tab/detail pegawai; bukan submenu global |
| 4 | Riwayat Pekerjaan | Tab/resource `employmentHistories` pada profil workforce | `READY TO REUSE` | Tetap menjadi tab/detail pegawai; bukan submenu global |
| 5 | Dokumen Personal | Tab/resource `documents` pada profil workforce | `READY TO REUSE` | Tetap menjadi tab/detail pegawai; bukan submenu global |
| 6 | Data Profesi | Profil profesi/credential license Karyawan atau Dokter | `REUSE WITH ADAPTER` | Data fakta tampil pada profil; proses kredensial tetap dimiliki Menu 09 |

Route `/hr/data-induk-pegawai/*` yang telanjur ada boleh dipertahankan sebagai deep-link
kompatibilitas selama belum ada keputusan penghapusan. Namun blok `Data Induk Pegawai` pada
`menu-items.jsx` tetap dikomentari/nonaktif agar pengguna tidak melihat dua pintu untuk fungsi
yang sama.

### 5.5 Kebutuhan BA 05 — Struktur Organisasi & Jabatan

| Urutan | Label target BA | Route/entry point existing | Status | Catatan |
| ---: | --- | --- | --- | --- |
| 1 | Unit Kerja | `/hr/master-data/organization-unit` | `READY TO REUSE` | Gunakan satu istilah bisnis; identifier route tidak perlu diubah |
| 2 | Struktur Organisasi | `/hr/master-data/organization` | `READY TO REUSE` | Reuse halaman struktur existing |
| 3 | Jabatan | Konteks posisi pada organisasi existing | `REUSE WITH ADAPTER` | Sediakan entry point stabil ke daftar jabatan; jangan membuat master posisi kedua |
| 4 | Rumpun Jabatan | `/hr/master-data/job-family` | `READY TO REUSE` | Reuse master existing |
| 5 | Uraian Tugas | `/hr/master-data/job-description` | `REPAIR` | Ini master uraian jabatan, bukan uraian efektif seorang pegawai |
| 6 | Atasan Langsung | `/hr/workforce-core/manager-assignments` | `REUSE WITH ADAPTER` | Read-only untuk struktur; perubahan dilakukan melalui Menu 07 |

Seluruh item di atas tetap berada pada parent **Master Data → Organisasi & Struktur Jabatan**.
Tidak dibuat parent tingkat atas `Struktur Organisasi & Jabatan` kedua. Item Master Data lain
seperti Legal Entity, Fasilitas Rumah Sakit, Lokasi Kerja, Pusat Biaya, Level Jabatan, dan Grade
Karyawan tetap berada di tempatnya dan tidak diubah.

### 5.6 Kebutuhan BA 06 — Administrasi Kepegawaian

| Urutan | Label target BA | Route existing | Status |
| ---: | --- | --- | --- |
| 1 | Surat Keputusan | `/hr/workforce-core/personnel-actions` | `READY TO REUSE` |
| 2 | Surat Tugas | `/hr/workforce-core/official-duties` | `READY TO REUSE` |
| 3 | Surat Keterangan | `/hr/workforce-core/employee-documents` | `READY TO REUSE` |
| 4 | Dokumen Administrasi | `/hr/workforce-core/administrative-documents` | `READY TO REUSE` |
| 5 | Pengajuan Perubahan Data | `/hr/workforce-core/employee-profile-changes` | `READY TO REUSE` |

Item existing lain pada Administrasi Kepegawaian tidak dihapus pada pekerjaan ini. Namun route
yang menjadi primary navigation Master Data atau Menu 07 tidak boleh tetap ditampilkan sebagai item
duplikat tanpa alasan dan label yang jelas.

### 5.7 Kebutuhan BA 07 — Penempatan, Mutasi & Promosi

| Urutan | Label target BA | Route existing | Status |
| ---: | --- | --- | --- |
| 1 | Penempatan Awal | `/hr/workforce-core/initial-placements` | `READY TO REUSE` |
| 2 | Perpindahan Unit | `/hr/workforce-core/unit-transfers` | `READY TO REUSE` |
| 3 | Rotasi Promosi | `/hr/workforce-core/rotation-promotions` | `READY TO REUSE` |
| 4 | Uraian Tugas | `/hr/workforce-core/employee-job-descriptions` | `REUSE WITH ADAPTER` |
| 5 | Atasan Langsung | `/hr/workforce-core/employee-managers` | `REUSE WITH ADAPTER` |

Untuk membedakan makna dari Menu 05, teks bantuan halaman disarankan menggunakan istilah
**Uraian Tugas Pegawai** dan **Atasan Langsung Pegawai**, sedangkan label sidebar tetap mengikuti
kebutuhan BA.

### 5.8 Kebutuhan BA 08 — Kontrak & Masa Kerja

| Urutan | Label target BA | Route existing | Status |
| ---: | --- | --- | --- |
| 1 | Perjanjian Kerja | `/hr/lifecycle/employment-contracts` | `READY TO REUSE` |
| 2 | Perpanjangan Kontrak | `/hr/lifecycle/contract-renewals` | `READY TO REUSE` |
| 3 | Masa Percobaan | `/hr/lifecycle/probations` | `READY TO REUSE` |
| 4 | Perubahan Status Kerja | `/hr/lifecycle/employment-status-changes` | `READY TO REUSE` |

Tidak direncanakan perubahan route. Pekerjaan hanya memastikan urutan, permission, active state,
dan deep-link tetap berfungsi.

### 5.9 Kebutuhan BA 09 — Kompetensi & Kredensial

| Urutan | Label target BA | Route existing | Status |
| ---: | --- | --- | --- |
| 1 | Data STR/SIP | `/corporate/hrd/credentialing/psv` | `READY TO REUSE` |
| 2 | Sertifikasi Kompetensi | `/corporate/hrd/learning/certificates` | `READY TO REUSE` |
| 3 | Persyaratan Jabatan | `/hr/master-data/competency` | `REUSE WITH ADAPTER` |
| 4 | Status Kredensial | `/corporate/hrd/credentialing/peer-review` | `READY TO REUSE` |
| 5 | Masa Berlaku Kewenangan | `/corporate/hrd/credentialing/recredentialing-cycle` | `READY TO REUSE` |

Fungsi kredensial lanjutan seperti kalkulator rapor komposit dan sinkronisasi SIMRS tetap di
parent ini. Item OPPE/FPPE yang merupakan evaluasi kinerja dipindahkan ke Menu 10 hanya setelah
dipastikan tidak menghasilkan `pathname` ganda.

### 5.10 Kebutuhan BA 10 — Penilaian Kerja

Parent existing `Manajemen Kinerja & Karier` diselaraskan menjadi `Penilaian Kerja`.

| Urutan | Label target BA | Route existing | Status | Catatan |
| ---: | --- | --- | --- | --- |
| 1 | Sasaran Kinerja | `/corporate/hrd/performance/cycles` | `READY TO REUSE` | Reuse siklus dan cascading target |
| 2 | Indikator Individu | `/corporate/hrd/performance/kpi-catalogs` | `READY TO REUSE` | Reuse katalog indikator KPI |
| 3 | Evaluasi Atasan | `/corporate/hrd/performance/appraisals` | `READY TO REUSE` | Reuse lembar evaluasi berjenjang |
| 4 | Hasil Penilaian | `/corporate/hrd/performance/dashboard` | `REUSE WITH ADAPTER` | Dashboard menjadi entry point hasil dan kalibrasi |
| 5 | Tindak Lanjut | `/corporate/hrd/performance/pip` | `READY TO REUSE` | Reuse Performance Improvement Plan |

Item `Sidang Kalibrasi`, `Jenjang Karier Klinis`, dan `Sanggah Banding Nilai Kinerja` tetap
dipertahankan setelah lima submenu BA. Fungsi OPPE/FPPE dapat ditempatkan sebagai submenu
tambahan karena merupakan penilaian klinis, tetapi pemindahannya harus menjaga permission dan
deep-link existing.

### 5.11 Kebutuhan BA 11 — Disiplin & Pembinaan

Kebutuhan ini dipenuhi di dalam parent existing **Hubungan Industrial & Terminasi**. Tidak dibuat
parent `Disiplin & Pembinaan` kedua. Lima butir BA boleh diwujudkan sebagai label, tab, atau filter
pada halaman existing; tidak semuanya harus menjadi submenu sidebar tersendiri.

| Urutan | Kebutuhan BA | Sumber kemampuan existing | Status | Representasi tanpa duplikasi |
| ---: | --- | --- | --- | --- |
| 1 | Catatan Pembinaan | Disciplinary action/coaching existing | `REUSE WITH ADAPTER` | Tab/filter pembinaan pada halaman tindakan disiplin; bukan parent baru |
| 2 | Teguran | `/hrd/relations/sanctions` | `REUSE WITH ADAPTER` | Gunakan kategori teguran/SP tanpa membuat record kedua |
| 3 | Pelanggaran | `/hrd/relations/incidents` | `READY TO REUSE` | Reuse laporan insiden/pelanggaran |
| 4 | Tindak Disiplin | `/hr/employee-relations/disciplinary-actions` | `READY TO REUSE` | Pastikan permission dan sumber data sama dengan hubungan industrial |
| 5 | Riwayat Penyelesaian | Riwayat kasus/sanksi existing | `EXTEND` | Tab/filter status selesai pada owner existing; jangan membuat daftar kasus kedua |

Parent `Hubungan Industrial & Terminasi` tidak dihapus. Submenu di luar lima kebutuhan ini,
seperti skorsing investigasi, pengaduan/whistleblowing, dan forum bipartit, tetap berada pada
parent existing. Penyesuaian label atau tab dilakukan di tempat yang sama, bukan dengan
memindahkan fungsi ke parent baru.

### 5.12 Kebutuhan BA 12 — Pemberhentian & Pensiun

| Urutan | Label target BA | Route existing | Status |
| ---: | --- | --- | --- |
| 1 | Pengunduran Diri | `/hr/separation/resignations` | `READY TO REUSE` |
| 2 | Pensiun | `/hr/separation/retirements` | `READY TO REUSE` |
| 3 | Akhir Kontrak | `/hr/separation/contract-expiry` | `READY TO REUSE` |
| 4 | Pemberhentian | `/hr/separation/terminations` | `READY TO REUSE` |
| 5 | Serah Terima | `/hr/separation/handovers` | `READY TO REUSE` |
| 6 | Clearance | `/hr/separation/exit-clearance` | `REPAIR` |

Perubahan label `Clearense` menjadi `Clearance` hanya koreksi teks. Route tetap sama. Fungsi
pengajuan PHK atau exit clearance yang saat ini juga muncul pada Hubungan Industrial tidak boleh
menghasilkan dua menu utama yang membingungkan; ownership primary-nya adalah Menu 12.

## 6. Urutan target pada sidebar

Susunan berikut adalah target anti-tumpang-tindih di dalam parent **Sumber Daya Manusia**. Nomor
BA pada tanda kurung menunjukkan kebutuhan yang dipenuhi; nomor tersebut bukan parent tambahan.
Parent existing di luar scope tetap dipertahankan.

```text
Sumber Daya Manusia
├── Master Data                                  [EXISTING]
│   ├── Data Karyawan & Perencanaan SDM
│   │   ├── Karyawan                             [memenuhi BA 04]
│   │   └── Dokter                               [memenuhi BA 04]
│   └── Organisasi & Struktur Jabatan            [memenuhi BA 05]
├── Dashboard SDM                                [BA 01 — BARU setelah kontrak siap]
├── Perencanaan Kebutuhan SDM                    [BA 02 — ADAPTASI parent existing]
├── Rekrutmen & Seleksi                          [BA 03 — ADAPTASI parent existing]
├── Administrasi Kepegawaian                     [BA 06 — ADAPTASI parent existing]
├── Penempatan, Mutasi & Promosi                 [BA 07 — PERTAHANKAN]
├── Kontrak & Masa Kerja                         [BA 08 — PERTAHANKAN]
├── Kompetensi & Kredensial                      [BA 09 — RAPIKAN]
├── Penilaian Kerja                              [BA 10 — rename parent existing in-place]
├── Hubungan Industrial & Terminasi              [memenuhi BA 11; tidak membuat parent kedua]
├── Pemberhentian & Pensiun                      [BA 12 — PERTAHANKAN]
├── Presensi & Kehadiran                        [EXISTING — jangan diubah]
├── Onboarding & Kontrak                        [EXISTING — jangan dihapus]
├── Kotak Masuk Persetujuan                     [EXISTING — jangan diubah]
├── Penjadwalan & Roster                        [EXISTING — jangan diubah]
├── Cuti & Perizinan                            [EXISTING — jangan diubah]
├── Manajemen Lembur                            [EXISTING — jangan diubah]
├── Manajemen Manfaat                           [EXISTING — jangan diubah]
├── Layanan SDM & Meja Bantuan                  [EXISTING — jangan diubah]
├── Pendidikan & Pelatihan RS (Diklat)          [CAPTURE — jangan diubah]
├── Kesehatan & Keselamatan Kerja RS (K3RS)     [CAPTURE — jangan diubah]
├── Perjalanan Dinas & Biaya (SPPD)             [CAPTURE — jangan diubah]
└── Layanan Mandiri                             [CAPTURE — jangan diubah]
```

Tidak ada parent fisik `Data Induk Pegawai`, `Struktur Organisasi & Jabatan`, atau `Disiplin &
Pembinaan` kedua. Ketiga kebutuhan tersebut dipenuhi melalui owner existing sebagaimana diagram.

## 7. Aturan perubahan `menu-items.jsx`

Implementasi berikutnya wajib mengikuti aturan ini:

1. Gunakan object menu existing; jangan membuat konfigurasi sidebar kedua.
2. Pertahankan `key` existing ketika makna item tidak berubah.
3. Buat `key` baru hanya untuk parent/item yang benar-benar baru.
4. Setiap `key` harus unik pada seluruh sidebar.
5. Pertahankan `pathname` existing untuk kemampuan yang direuse.
6. Jangan membuat route alias hanya untuk menyesuaikan label bisnis.
7. Jangan menghapus `requiredPermission` saat memindahkan item.
8. Jika parent memiliki banyak submenu, permission anak tetap menjadi dasar visibilitas; parent
   tidak boleh tampil kosong.
9. Active-route harus tetap terbuka pada parent yang benar ketika pengguna masuk melalui
   deep-link detail seperti `[id]`.
10. Item yang belum memiliki route stabil, seperti worklist Seleksi Administrasi, Tes/Wawancara,
    dan Riwayat Penyelesaian, tidak boleh diarahkan ke placeholder atau halaman palsu.
11. Menu yang tidak tercantum pada scope perubahan harus menghasilkan diff nol, kecuali hanya
    bergeser karena item BA dipindahkan secara eksplisit.
12. Jangan mengubah icon global, warna, typography, atau CSS sidebar dalam task penataan menu.

## 8. Tahapan pelaksanaan yang direkomendasikan

Tahapan di bawah ini bukan task ID roadmap resmi. Task resmi hanya boleh dibuat melalui proses
delivery planning setelah scope ini disetujui.

### Tahap P0 — Kunci baseline dan matriks kepemilikan

1. Catat SHA frontend terbaru sebelum edit.
2. Ekstrak seluruh parent, child, `key`, `pathname`, dan `requiredPermission` dari
   `menu-items.jsx`.
3. Tandai setiap item sebagai BA, pendukung BA, atau di luar scope.
4. Tetapkan owner existing untuk setiap kebutuhan BA sebelum mengusulkan parent/item baru.
5. Selesaikan konflik ownership route Menu 05/07, Menu 09/10, dan Menu 11/12.
6. Hasil: matriks before/after tanpa route yatim dan tanpa route primary ganda.

### Tahap P1 — Perubahan aman yang sudah memiliki route

1. Pertahankan blok sidebar Data Induk Pegawai dalam keadaan nonaktif.
2. Catat Master Data Karyawan/Dokter sebagai pemenuh kebutuhan BA 04 tanpa menambah link.
3. Rapikan label dan urutan Perencanaan Kebutuhan SDM.
4. Rapikan Administrasi Kepegawaian.
5. Pertahankan Menu 07 dan Menu 08.
6. Rapikan lima submenu utama Kompetensi & Kredensial.
7. Koreksi `Clearense` menjadi `Clearance`.
8. Jangan mengerjakan Dashboard atau route worklist yang masih `MISSING`/`EXTEND` pada tahap ini.

### Tahap P2 — Konsolidasi menu yang tumpang tindih

1. Pertahankan Struktur Organisasi & Jabatan di bawah Master Data; hilangkan hanya shortcut yang
   terbukti ganda setelah owner primary ditetapkan.
2. Selaraskan Manajemen Kinerja & Karier dengan kebutuhan Penilaian Kerja pada parent yang sama;
   jangan membuat parent Penilaian Kerja kedua.
3. Petakan Disiplin & Pembinaan sebagai label/tab/filter pada Hubungan Industrial & Terminasi;
   jangan membuat parent Disiplin & Pembinaan kedua.
4. Pertahankan fungsi hubungan industrial non-BA.
5. Pastikan tidak ada `pathname` primary yang muncul ganda.

### Tahap P3 — Entry point yang belum tersedia

1. Tentukan desain worklist Seleksi Administrasi.
2. Tentukan desain worklist Tes/Wawancara.
3. Tentukan entry point Riwayat Penyelesaian disiplin.
4. Audit endpoint dan permission Dashboard SDM.
5. Bila diperlukan source baru, pecah menjadi task frontend/backend terpisah dengan kontrak yang
   dikunci; jangan mencampurkannya ke task penataan menu.

### Tahap P4 — Validasi dan regresi

1. Jalankan pemeriksaan syntax/lint terarah.
2. Jalankan build frontend.
3. Uji seluruh route menu dengan akun yang memiliki dan tidak memiliki permission.
4. Uji active state, expand/collapse parent, browser refresh, dan deep-link detail.
5. Uji tampilan desktop dan lebar sidebar sempit.
6. Lakukan regresi khusus terhadap empat menu pada capture yang wajib tidak berubah: Diklat,
   K3RS, SPPD, dan Layanan Mandiri.
7. Dokumentasikan hasil uji browser per menu sesuai aturan live browser testing Quilvian.

## 9. Skenario bisnis validasi

### 9.1 Tujuan

Memastikan petugas SDM menemukan pekerjaan berdasarkan proses bisnis, bukan berdasarkan nama
tabel atau struktur teknis aplikasi.

### 9.2 Pelaku

| Pelaku | Kebutuhan |
| --- | --- |
| Admin SDM | Mengelola data induk, surat, kontrak, dan perubahan data |
| Perencana SDM | Menganalisis beban, formasi, permintaan, dan gap tenaga |
| Rekrutmen | Mengelola lowongan sampai serah terima onboarding |
| Atasan penilai | Menilai sasaran, indikator, evaluasi, dan tindak lanjut |
| Petugas hubungan karyawan | Mengelola pembinaan, pelanggaran, dan disiplin |
| Petugas terminasi | Mengelola resign, pensiun, akhir kontrak, serah terima, dan clearance |

### 9.3 Langkah utama

1. Pengguna membuka **Sumber Daya Manusia**.
2. Sistem hanya menampilkan parent dan submenu yang diizinkan permission pengguna.
3. Pengguna memilih menu existing yang menjadi pemilik salah satu kebutuhan bisnis BA.
4. Sistem membuka route existing tersebut tanpa melewati façade kedua dengan data yang sama.
5. Untuk proses lintas domain, sistem meneruskan pengguna ke workflow pemilik tanpa membuat data
   baru. Contoh: kandidat lulus diteruskan dari Rekrutmen ke Onboarding.
6. Ketika pengguna kembali atau me-refresh halaman, parent yang benar tetap terbuka dan submenu
   aktif tetap ditandai.

### 9.4 Jalur tidak normal

| Kondisi | Perilaku yang diharapkan |
| --- | --- |
| Pengguna tidak memiliki permission | Item disembunyikan atau akses ditolak sesuai pola existing; permission tidak boleh dilemahkan |
| Route belum tersedia | Item tidak dipublikasikan; jangan arahkan ke placeholder atau fake success |
| API mengembalikan 403 | Halaman menjelaskan pengguna tidak memiliki hak akses, bukan menampilkan data kosong seolah tidak ada data |
| API gagal/500 | Tampilkan state error dan retry sesuai pola halaman existing |
| Parent tidak memiliki satu pun child yang boleh dilihat | Parent tidak ditampilkan |
| Satu route ditemukan pada dua parent | Implementasi dihentikan sampai primary owner diputuskan |

### 9.5 Hasil akhir

Seluruh 12 kelompok kebutuhan Business Analyst dapat ditemukan melalui owner menu existing atau
fitur baru yang benar-benar belum tersedia, tanpa membuat navigasi kedua untuk fungsi yang sama.

## 10. Acceptance criteria planning

Planning dianggap siap diturunkan menjadi task implementasi setelah seluruh kriteria berikut
disetujui:

1. Dua belas kelompok kebutuhan BA dan 61 butirnya memiliki pemetaan owner existing, route, tab,
   filter, atau status gap yang eksplisit; tidak diwajibkan menjadi 12 parent fisik.
2. Tidak ada halaman existing yang dihapus.
3. Tidak ada tabel, record, atau workflow baru hanya karena menu dipindahkan.
4. Setiap route memiliki satu primary navigation owner.
5. Data Induk Pegawai dipenuhi oleh Master Data Karyawan/Dokter; blok sidebar Data Induk tetap
   nonaktif dan route kompatibilitas tidak menjadi primary navigation.
6. Dashboard SDM tidak diimplementasikan sebelum sumber data dan kontrak agregat dipastikan.
7. Onboarding pada Rekrutmen dipahami sebagai handoff, sedangkan pelaksanaan onboarding tetap
   memakai lifecycle existing.
8. Uraian Tugas dan Atasan Langsung pada Master Data dibedakan dari data efektif pegawai pada
   Menu 07.
9. Diklat, K3RS, SPPD, dan Layanan Mandiri tidak mengalami perubahan label, `key`, `pathname`,
   permission, atau perilaku.
10. Fungsi karier tambahan pada Manajemen Kinerja tidak dihapus saat parent diselaraskan menjadi
    Penilaian Kerja.
11. Fungsi Disiplin & Pembinaan dipetakan pada Hubungan Industrial & Terminasi tanpa membuat
    parent kedua dan tanpa menghapus fungsi hubungan industrial non-BA.
12. Lint, build, route smoke test, permission test, dan live browser regression menghasilkan bukti
    yang dapat ditelusuri.

## 11. Risiko dan mitigasi

| Risiko | Dampak | Mitigasi |
| --- | --- | --- |
| Route muncul pada dua parent | Pengguna bingung dan active state salah | Tetapkan satu primary owner pada Tahap P0 |
| Permission hilang ketika object menu dipindahkan | Pengguna melihat menu tanpa hak | Salin dan verifikasi `requiredPermission`; uji akun positif dan negatif |
| Rename dianggap membuat domain baru | Data atau workflow menjadi ganda | Pertahankan route dan service existing; perubahan label tidak mengubah ownership |
| Menu existing non-BA ikut terhapus | Alur operasional lain rusak | Snapshot before/after dan regression capture |
| Dashboard menghitung data di browser | Angka tidak konsisten dan lambat | Gunakan kontrak agregat backend yang disetujui setelah audit |
| Route dinamis dipakai sebagai menu | Link tidak dapat dibuka tanpa ID | Buat atau reuse worklist stabil sebelum menu dipublikasikan |
| Sidebar terlalu panjang | Navigasi sulit dipindai | Pertahankan grouping, permission filtering, dan urutan bisnis; redesign visual adalah task terpisah |

## 12. Bukti source utama

| Bukti | Lokasi | Keterangan |
| --- | --- | --- |
| Parent Sumber Daya Manusia | `QuilvianSystemFrontendDev/src/utils/menu-sidebar/menu-items.jsx:213` | Root sidebar HR |
| Master struktur organisasi | `menu-items.jsx:223-292` | Struktur, unit, rumpun, uraian, dan atasan saat ini berada di Master Data |
| Blok sidebar Data Induk Pegawai | `menu-items.jsx:608-650` | Enam submenu tersedia tetapi dikomentari; dipertahankan nonaktif agar tidak menjadi navigasi kedua |
| Façade Data Induk memakai komponen Master Data | `src/components/view/hr/data-induk-pegawai/data-induk-pegawai-view.jsx:25-26,315-359` | Mengimpor dan merender `EmployeeDetailView` serta `EmployeeWorkforceManageView` milik Master Data |
| Detail Karyawan sebagai owner profil | `src/components/view/hr/master-data/employees/detail/employee-detail-view.jsx:11,146-160` | Menggunakan `WorkforceProfileDashboard` yang sama untuk profil dan resource workforce |
| Endpoint detail Karyawan | `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/MasterData/Workforce/Controllers/EmployeeController.cs:32-43,82-160` | Master Data Karyawan mengekspos tab ke endpoint dokumen, pendidikan, sertifikasi, dan kredensial workforce yang sama |
| Administrasi Kepegawaian | `menu-items.jsx:651-728` | Fungsi perubahan data, penempatan, SK, surat, dokumen, dan payroll |
| Penempatan, Mutasi & Promosi | `menu-items.jsx:729-785` | Lima submenu BA sudah tersedia |
| Kontrak & Masa Kerja | `menu-items.jsx:786-833` | Empat submenu BA sudah tersedia |
| Perencanaan Tenaga Kerja | `menu-items.jsx:834-930` | Sembilan route planning sudah tersedia |
| Pengadaan & Rekrutmen | `menu-items.jsx:995-1071` | Requisition, lowongan, kandidat, offer, MCU, talent pool, handoff |
| Pemberhentian & Pensiun | `menu-items.jsx:1487-1528` | Enam submenu sudah tersedia |
| Kompetensi & Kredensial | `menu-items.jsx:1564-1689` | Lima submenu BA dan fungsi OPPE/FPPE/recredentialing tambahan |
| Manajemen Kinerja & Karier | `menu-items.jsx:1780-1873` | Kemampuan KPI, cycle, appraisal, calibration, PIP, karier, dan senggah |
| Hubungan Industrial & Terminasi | `menu-items.jsx:1874-1970` | Kemampuan insiden, sanksi, skorsing, grievance, bipartit, separation, clearance |

Tabel di atas adalah bukti baseline sebelum implementasi pada commit
`0fc88be5c64e68d105c7c75adbca2e7104d8de4a`. Bukti source sesudah implementasi dicatat pada
bagian 15 dan menjadi rujukan terkini untuk Paket A.

## 13. Keputusan yang masih diperlukan untuk Paket B

1. Apakah istilah BA harus tampil sebagai label menu/tab persis, atau cukup tercatat sebagai
   cakupan fungsi pada owner menu existing?
2. Apakah Seleksi Administrasi cukup berupa filter pada Bank Pelamar, atau harus memiliki worklist
   tersendiri?
3. Apakah Tes/Wawancara cukup berupa worklist gabungan evaluasi HR dan klinis, atau dipisahkan?
4. Definisi Dokumen Kadaluarsa pada Dashboard: hanya STR/SIP, atau termasuk sertifikat,
   perjanjian, identitas, dan dokumen personal lain?
5. Horizon peringatan Dashboard: misalnya H-90, H-60, H-30, H-7, dan kedaluwarsa, atau kebijakan
   lain?
6. Apakah Riwayat Penyelesaian Disiplin menjadi tab/filter existing atau halaman worklist baru?

Pertanyaan tersebut tidak menghalangi perubahan aman pada Tahap P1, tetapi menghalangi publikasi
menu Dashboard, Seleksi Administrasi, Tes/Wawancara, dan Riwayat Penyelesaian.

## 14. Paket delivery yang disetujui

Planning ini disetujui pengguna pada 20 November 2026 dengan dua paket terpisah:

1. **Paket A — Reorganisasi menu berbasis route existing.** Cakup rename, reorder, penetapan
   primary navigation, mempertahankan Data Induk sebagai route kompatibilitas non-sidebar,
   permission, active-route, dan regresi menu non-BA. Paket ini tidak membuat API atau halaman
   bisnis baru.
2. **Paket B — Gap layar/entry point.** Cakup Dashboard SDM, Seleksi Administrasi,
   Tes/Wawancara, serta Riwayat Penyelesaian hanya setelah keputusan bagian 13 dan kontrak yang
   dibutuhkan disetujui.

Pemisahan ini menjaga perubahan navigasi yang aman dapat berjalan tanpa menunggu kebutuhan baru
yang masih memerlukan keputusan bisnis atau kontrak backend. Paket A telah diimplementasikan;
Paket B belum diberi wewenang implementasi.

## 15. Rekam hasil implementasi Paket A — 20 November 2026

### 15.1 Hasil yang diterapkan

| Area | Hasil implementasi | Bukti source frontend |
| --- | --- | --- |
| Administrasi Kepegawaian | Lima fungsi BA ditempatkan di urutan awal. `Relasi Atasan` tidak dibuat sebagai pintu kedua; kepemilikan tetap pada Master Data | `menu-items.jsx:652-718`; `hr-master-data-sidebar-grouping.test.mjs:356-379` |
| Perencanaan Kebutuhan SDM | Parent diselaraskan dan empat fungsi BA menjadi urutan awal; lima fungsi existing non-BA dipertahankan | `menu-items.jsx:827-918`; test `HR-07` baris `326-336` |
| Rekrutmen & Seleksi | Label parent, Lowongan, Pelamar, dan handoff Onboarding diselaraskan; requisition, offer, MCU, dan talent pool tetap ada | `menu-items.jsx:988-1059`; test `HR-07` baris `338-354` |
| Data Induk Pegawai | Blok sidebar tetap nonaktif; Master Data Karyawan/Dokter tetap menjadi owner sehingga tidak ada navigasi kedua | `menu-items.jsx:609-650`; test `HR-07` baris `375-379` |
| Pemberhentian & Pensiun | Ejaan `Clearense` dikoreksi menjadi `Clearance` tanpa mengubah `key` atau route | `menu-items.jsx:1515-1518` |
| Kompetensi & Kredensial | Lima fungsi BA tetap di owner ini; OPPE/FPPE tidak lagi digandakan pada credentialing | `menu-items.jsx:1557-1629`; test `HR-08` baris `387-419` |
| Penilaian Kerja | Parent diselaraskan; lima fungsi BA menjadi urutan awal; OPPE/FPPE dan fungsi karier existing tetap tersedia pada satu owner kinerja | `menu-items.jsx:1723-1862`; test `HR-08` baris `382-419` |
| Hubungan Industrial & Terminasi | `Teguran` dan `Pelanggaran` memakai route existing; fungsi hubungan industrial non-BA tidak dihapus | `menu-items.jsx:1867-1958`; test `HR-08` mulai baris `422` |
| Menu di luar kebutuhan BA | Diklat, K3RS, SPPD, Layanan Mandiri, dan kelompok existing lain dipertahankan | Diff commit dan test regresi leaf menu |

Commit hasil implementasi:
`QuilvianSystemFrontendDev@51853893e9f229f0f22f5e86bc5f2e8c08946779` pada branch
`HamzahV2`.

### 15.2 Hasil verifikasi

| Pemeriksaan | Hasil | Bukti |
| --- | --- | --- |
| Lint error | `PASS` | `npm run lint:errors` |
| ESLint target | `PASS` | Pemeriksaan target `menu-items.jsx` dan test menu HR |
| Pemeriksaan penggunaan CSS | `PASS` | 0 temuan |
| Test unit menu HR | `PASS` | 8/8 pada `tests/unit/hr-master-data-sidebar-grouping.test.mjs`; termasuk `HR-07` dan `HR-08` |
| Build produksi | `PASS` | `npm run build`; 890 route/page dihasilkan dan standalone disiapkan |
| Seluruh unit test frontend | `PARTIAL` | 5.104 lulus dan 73 gagal dari 5.177; kegagalan berada pada modul existing di luar perubahan menu; file test target lulus seluruhnya |
| Uji browser dan permission runtime | `PENDING` | Belum dijalankan; Paket A belum boleh dinyatakan `DONE` |

### 15.3 Cakupan yang ditunda atau terblokir

| Kebutuhan | Status | Alasan |
| --- | --- | --- |
| Dashboard SDM | `DEFERRED / PACKAGE B` | Sumber data agregat, definisi dokumen kedaluwarsa, dan horizon peringatan belum dikunci |
| Seleksi Administrasi | `DEFERRED / PACKAGE B` | Belum diputuskan apakah filter Bank Pelamar atau worklist tersendiri |
| Tes/Wawancara | `DEFERRED / PACKAGE B` | Belum diputuskan bentuk worklist dan pembagian evaluasi HR/klinis |
| Riwayat Penyelesaian | `DEFERRED / PACKAGE B` | Belum diputuskan sebagai filter, tab, atau worklist |
| Tindak Disiplin | `CONFLICT — NOT PUBLISHED` | Service frontend memakai `/v1/hr/workforce-profile/disciplinary-actions`, sedangkan kontrak detail backend berada di `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/disciplinary-actions`; diperlukan perbaikan kontrak/service dan keputusan entry point sebelum item sidebar dipublikasikan |

### 15.4 Putusan status

Paket A berstatus **`PARTIAL / VERIFYING`**. Perubahan struktur aman sudah berada pada source dan
lulus lint, test target, serta build. Status belum dinaikkan menjadi `DONE` karena belum ada bukti
uji browser headed dan matriks permission akun positif/negatif. Paket B tetap di luar implementasi
ini. Tidak ada API, entity, tabel, migration, atau workflow backend yang dibuat oleh perubahan menu.
