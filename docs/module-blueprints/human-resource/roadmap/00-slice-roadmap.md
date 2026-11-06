# Human Resource — Slice Roadmap

| Field | Value |
| --- | --- |
| Blueprint ID | `HRD-BP-001` |
| Roadmap revision | `18` — revisi `17` menyerap target state Slice `S-C1-OPPE`; **revisi `18` (6 November 2026) menyerap keputusan `HRD-DEC-158` s.d. `HRD-DEC-163` dan asumsi `HRD-ASM-51` s.d. `HRD-ASM-54` untuk target state Slice `S-C2` (Learning, Training & Competency — Pelatihan Wajib Akreditasi STARKES 100%, Evaluasi OSCE, Radar Kedaluwarsa & Safeguard Shift Buddy System, Ikatan Dinas Nakes > Rp 10 Jt / > 1 Bulan, serta Visual Tracker 50 SKP Plataran Sehat Kemenkes), merencanakan 16 task backend (`BE-HRD-238` s.d. `BE-HRD-253`) dan 15 task frontend (`FE-HRD-216` s.d. `FE-HRD-230`) lintas gelombang MVP-48 s.d. MVP-50 & Verifikasi** |
| Status | `DRAFT` |
| Backend SHA | `16f6b2a` |
| Frontend SHA | `2cd1c06` |
| Masukan | `00-interview-decisions.md` rev `16`, `01-existing-capability-map.md` rev `1.1`, `02-backend-architecture.md` rev `2`, `03-frontend-architecture.md` rev `2` |

Dokumen ini memecah modul HR menjadi slice yang dapat dikerjakan. Ia **bukan** daftar task.
Pemecahan menjadi task backend dan frontend yang berukuran kecil adalah pekerjaan
`/plan-module-delivery`.

Setiap slice memuat sebelas hal yang sama: Current State, Target State, Backend Impact, Frontend
Impact, Database Impact, Dependency, Blocking Decision, QBE Impact, Migration Risk, Acceptance
Criteria, dan Release Status.

---

## 0. Cara membaca

### 0.1 Release Status

| Nilai | Artinya |
| --- | --- |
| `READY` | Boleh dirancang final dan boleh direncanakan menjadi task |
| `PARTIAL` | Sebagian boleh, sebagian tertahan. Batasnya disebut eksplisit di dalam slice |
| `BLOCKED` | Tidak boleh dirancang. Dependency-nya disebut namanya |
| `DEFERRED` | Boleh secara teknis, sengaja ditunda karena prioritas |

### 0.2 QBE Impact

Kolom ini menyebut aturan `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md` yang berlaku,
beserta kelas keberlakuannya: `NEW CODE`, `TOUCHED LEGACY`, atau `LEGACY MIGRATION`.

### 0.3 Aturan yang mengikat seluruh slice

1. **`HRD-DEC-016`** — kebab-case adalah route canonical; route lama tetap hidup sebagai
   compatibility alias yang memakai controller, service, dan business logic yang sama. Bukan
   hard breaking rename.
2. **`HRD-DEC-019`** — kebijakan penamaan canonical. `Mst` tetap master atau referensi. `Wfp`
   adalah prefix yang sah untuk keluarga workforce HR dan **bukan** legacy. `Hrd` canonical dan
   default untuk entity operasional HR baru. `Trx` legacy generik yang **hanya** di-ratchet saat
   entity itu materially touched. Tidak ada rename massal, tidak ada kampanye, tidak ada tenggat.
3. **`HRD-Q-05`** — keputusan skema yang merusak data ditahan sampai audit database.
4. **`HRD-Q-08`** — rilis slice kredensial ditahan sampai Komite Medik.
5. **`HRD-Q-10` dan `HRD-Q-11`** — bentuk serah terima payroll belum terselesaikan.
6. Proses bisnis yang sedang berjalan tidak boleh diputus hanya demi konsistensi penamaan.
7. Tidak ada source aplikasi, migration, controller, entity, frontend, maupun database yang
   disentuh dari alur blueprint ini.

### 0.4 Peta slice ke fase

| Fase | Slice | Status fase |
| --- | --- | --- |
| `HRD-PH-001` | `S0-A`, `S0-B` | `READY` |
| `HRD-PH-002` | `S-A1` s.d. `S-A7` | `READY` |
| `HRD-PH-003` | `S-B1` s.d. `S-B4` | `PLANNED` — `S-B4` (17 BE, 13 FE rev 8); `S-B1` (13 BE, 14 FE rev 9); `S-B2` (14 BE, 14 FE rev 10); `S-B3` direncanakan pada roadmap revision `11` (5 November 2026): 15 task backend (`BE-HRD-131` s.d. `BE-HRD-145`) dan 14 task frontend (`FE-HRD-116` s.d. `FE-HRD-129`) |
| `HRD-PH-004` | `S-B5`, `S-B6` | `PLANNED` — `S-B5` (15 BE, 14 FE rev 12); `S-B6` direncanakan pada roadmap revision `13` (5 November 2026): 15 task backend (`BE-HRD-161` s.d. `BE-HRD-175`) dan 14 task frontend (`FE-HRD-144` s.d. `FE-HRD-157`) |
| `HRD-PH-005` | `S-C1`, `S-C1-OPPE` | `PLANNED` — `S-C1` (16 BE, 15 FE rev 16); `S-C1-OPPE` direncanakan pada roadmap revision `17` (6 November 2026): 16 task backend (`BE-HRD-222` s.d. `BE-HRD-237`) dan 15 task frontend (`FE-HRD-201` s.d. `FE-HRD-215`) |
| `HRD-PH-006` | `S-C6` | `BLOCKED` |
| `HRD-PH-007` | `S-D1` s.d. `S-D5` | `READY` untuk `S-D1` dan `S-D2`; `PLANNED` untuk `S-D3` (15 BE, 14 FE rev 14) dan `S-D4` (5 November 2026, 15 task BE & 14 task FE, roadmap rev 15); `S-D5` `DEFERRED` |
| `HRD-PH-008` | `S-C2` s.d. `S-C5` | `READY` |
| `HRD-PH-009` | `S-E` | **Bukan fase yang dijadwalkan.** Aturan lintas-slice yang berlaku sepanjang implementasi |

### 0.5 Daftar seluruh slice

Dua puluh enam slice. `S-D2` sampai `S-D5` dijelaskan dalam satu blok bersama karena
perlakuannya identik, tetapi keempatnya tetap dihitung sebagai slice tersendiri.

| Gelombang | Slice | Jumlah |
| --- | --- | ---: |
| `S0` fondasi | `S0-A`, `S0-B` | 2 |
| `A` memakai backend matang | `S-A1` s.d. `S-A7` | 7 |
| `B` administrasi waktu kerja | `S-B1` s.d. `S-B6` | 6 |
| `C` pengembangan orang dan slice terblokir | `S-C1` s.d. `S-C6` | 6 |
| `D` domain tanpa API | `S-D1` s.d. `S-D5` | 5 |
| `E` aturan ratchet | `S-E` | 1 |
| **Total** | | **27** |

---

## 1. Gelombang S0 — Fondasi

### `S0-A` — Pendaftaran prefix yang sah pada registry

| Aspek | Isi |
| --- | --- |
| **Current State** | Registry mencatat satu baris untuk HR: `Corporate / SelfServices \| Human Resource \| BUSINESS DOMAIN \| Hrd \| ACTIVE / LEGACY`. Kenyataan pemakaian di source: `Hrd` 15 entity, `Wfp` 40 entity, `Mst` 104 entity, `Trx` 178 entity. **`Wfp` belum punya baris registry sama sekali**, padahal ia keluarga entity workforce dan profil HR yang dipakai luas |
| **Target State** | Registry mengenali `Hrd` sebagai prefix operasional canonical dan default modul HR, dan mengenali `Wfp` sebagai prefix yang sah untuk keluarga workforce dan profil HR. `Mst` tetap mengikuti kepemilikan master yang berlaku. Registry **tidak** menyatakan seluruh `Trx` milik HR |
| **Backend Impact** | Tidak ada perubahan source aplikasi. Yang berubah hanya `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, dan itu dikerjakan lewat task implementasi terpisah — bukan dari alur blueprint ini |
| **Frontend Impact** | Tidak ada |
| **Database Impact** | Tidak ada |
| **Dependency** | `HRD-DEP-001` |
| **Blocking Decision** | Tidak ada. `HRD-DEC-019` justru mewajibkan langkah ini lebih dulu sebelum entity baru dibuat |
| **QBE Impact** | `QBE-MOD-002` — modul yang memiliki entity operasional wajib punya entri registry yang disetujui sebelum entity pertama dibuat, dan prefix tidak boleh disimpulkan dari nama folder. Kelas: dokumentasi tata kelola, bukan `NEW CODE` |
| **Migration Risk** | Nihil. Tidak ada skema, tabel, maupun kolom yang disentuh |
| **Acceptance Criteria** | 1. Registry mengenali `Hrd` sebagai prefix operasional canonical dan default modul HR. 2. Registry mengenali `Wfp` sebagai prefix yang sah untuk keluarga workforce HR. 3. `Mst` tetap mengikuti kepemilikan master dan referensi yang berlaku, tanpa perubahan. 4. Registry **tidak** menyatakan seluruh `Trx` milik HR. 5. Kepemilikan entity `Trx*` ditentukan dari domain, lokasi berkas, dan bukti — bukan dari prefixnya. 6. Tidak ada kolom, kategori, maupun nilai lifecycle baru yang dikarang; seluruhnya memakai kosakata yang sudah tersedia di registry |
| **Release Status** | `READY` |

Bentuk pendaftaran yang benar mengikuti kolom yang memang sudah ada di registry — `Area`,
`Module/owner`, `Category`, `Prefix`, dan `Lifecycle` — dengan nilai lifecycle dari kosakata yang
sudah tersedia: `PLANNED`, `ACTIVE`, `LEGACY`, atau `DEPRECATED`. Bila registry sudah menyediakan
mekanisme baris terpisah atau alias, pakai mekanisme itu.

**`Wfp` tidak boleh diberi label legacy yang akan dihapus.** Ia prefix yang sah dan tetap dipakai
untuk entity baru yang memang termasuk keluarga workforce. Pelabelan sebagai legacy hanya sah
bila ada keputusan manusia terpisah di kemudian hari.

Slice ini adalah prasyarat bagi pembuatan entity HR baru mana pun. Membuat entity operasional
baru sebelum registry mengenali prefixnya adalah pelanggaran `QBE-MOD-002`.


### `S0-B` — Route canonical kebab-case beserta compatibility alias

| Aspek | Isi |
| --- | --- |
| **Current State** | Delapan route master data memakai kata gabung tanpa pemisah: `actiontypes`, `casetypes`, `sanctiontypes`, `violationtypes`, `workcalendars`, `workschedules`, `shiftgroups`, `shiftpatterns`. Route lain sudah kebab-case. Empat yang pertama dimiliki controller yang tersimpan di folder yang salah |
| **Target State** | Setiap route punya nama canonical kebab-case. Nama lama tetap dilayani sebagai alias oleh **action yang sama**. Frontend berpindah bertahap ke nama canonical |
| **Backend Impact** | Delapan controller mendapat tambahan route template. Tidak ada controller, service, DTO, atau validasi baru. Swagger menampilkan kedua nama, dengan nama lama ditandai deprecated |
| **Frontend Impact** | Delapan baris konstanta di `src/lib/constants/hr/master-data/**` berpindah ke nama canonical, satu per satu, tidak harus serentak |
| **Database Impact** | Tidak ada |
| **Dependency** | Tidak ada |
| **Blocking Decision** | `HRD-DEC-016` mengikat bentuknya. `HRD-Q-15` menahan **penghapusan** alias, bukan pembuatannya |
| **QBE Impact** | `QBE-NAM` untuk penamaan route baru. Kelas: `TOUCHED LEGACY` — perbaikan terbatas pada controller yang memang disentuh |
| **Migration Risk** | Nihil untuk database. Risiko konsumen ditiadakan oleh alias; tidak ada satu pun pemanggil yang rusak |
| **Acceptance Criteria** | 1. `work-calendars` dan `workcalendars` mengembalikan hasil identik dari action yang sama. 2. Tidak ada controller, service, atau validasi baru yang dibuat untuk melayani nama lama. 3. Swagger menampilkan nama lama sebagai deprecated. 4. Frontend dapat berpindah satu entity pada satu waktu tanpa memutus entity lain |
| **Release Status** | `READY` |

Contoh bentuk yang benar dan yang salah:

> **Benar.** Satu action `Get()` melayani dua route template, `master-data/work-calendars` dan
> `master-data/workcalendars`.
>
> **Salah.** Dibuat `WorkCalendarController` baru untuk nama canonical, sementara
> `WorkCalendarLegacyController` melayani nama lama. Dua implementasi akan berbeda perilakunya
> cepat atau lambat.

---

## 2. Gelombang A — Memakai backend yang sudah matang

Seluruh slice pada gelombang ini memakai endpoint yang **sudah ada**. Pekerjaannya dominan
frontend, dan itu yang membuat risikonya paling rendah sekaligus nilainya paling cepat terasa.

### `S-A1` — Administrasi Kepegawaian, Dokumen SK, dan Employee 360 (Workforce Core)

Slice `S-A1` dibagi menjadi dua tahap pelaksanaan terstruktur:
1. **Tahap 1 (Selesai — `PASS`)**: Penyediaan 6 layar dan rute daftar lintas-pegawai Administrasi Kepegawaian (Penempatan Organisasi, Penugasan Jabatan, Penetapan Manajer, Riwayat Kerja, Penetapan Gaji, dan Perubahan Profil). Diimplementasikan lewat task `BE-HRD-003` dan `FE-HRD-002`.
2. **Tahap 2 (Target State — `READY FOR IMPLEMENTATION`)**: Tata kelola dokumen Surat Keputusan (SK) Kepegawaian (`HrdPersonnelActionOrder`), pembatasan mutasi retroaktif oleh periode presensi terbuka, integritas 1 NIK = 1 Profil, Employee 360 dual-view (HR vs Manager), inisialisasi batch penempatan awal 55 pegawai, dan penguncian form master data pegawai menjadi read-only berbasis wewenang SK (`HRD-DEC-046` s.d. `HRD-DEC-057`).

| Aspek | Isi |
| --- | --- |
| **Current State** | 6 layar daftar lintas-pegawai telah aktif dan terhubung ke backend (Tahap 1). Namun, mutasi penempatan dan jabatan saat ini masih bisa diubah langsung pada form master data pegawai tanpa SK resmi; pegawai eksisting (55 staf) belum memiliki catatan penempatan organisasi, jabatan, atasan, dan gaji yang terstruktur; dan belum tersedia agregat SK terpadu untuk mutasi, promosi, rotasi, demosi, serta penugasan sementara |
| **Target State** | 1. Seluruh perubahan organisasi, jabatan, atasan, dan gaji wajib melalui agregat `HrdPersonnelActionOrder` dengan nomor SK unik, tanggal berlaku, tanggal pembukuan, alasan kedinasan, dan tautan wajib berkas pindai fisik ber-TTD (`HRD-DEC-048`, `056`).<br>2. Form Master Data Pegawai mengunci field unit dan jabatan menjadi read-only (`HRD-DEC-047`).<br>3. Mutasi retroaktif divalidasi ketat terhadap status periode presensi terbuka (`HRD-DEC-050`).<br>4. Tersedia layar Profil Terpadu Pegawai (Employee 360) dengan dual-view: HR View (lengkap) vs Direct Manager View (terbatas pada profil kedinasan bawahan langsung aktif, data kompensasi/rekening/disiplin/keluarga disembunyikan ketat) (`HRD-DEC-053`).<br>5. Inisialisasi data penempatan awal 55 pegawai eksisting dijalankan via mekanisme batch satu kali yang tervalidasi (`HRD-DEC-054`).<br>6. Perubahan data profil mandiri via ESS dan kuasa HR (on-behalf) wajib diverifikasi petugas HR berbeda (`HRD-DEC-052`) |
| **Backend Impact** | `NEW CODE` & `EXTEND`:<br>- Entitas baru `HrdPersonnelActionOrder`, `HrdPersonnelActionOrderAttachment`, `HrdInitialPlacementBatch`, `HrdInitialPlacementBatchRow`<br>- Perluasan kolom audit persetujuan dan FK `PersonnelActionOrderId` pada tabel penempatan (`Wfp*Assignment`) dan permohonan profil (`TrxEmployeeProfileChangeRequest`)<br>- Controller baru: `HrdPersonnelActionOrderController`, `HrdEmployee360Controller`, `HrdInitialPlacementBatchController`<br>- Controller perluasan: `EmployeeProfileChangeController` (tambah `POST /on-behalf`) |
| **Frontend Impact** | - 7 komponen/layar baru di `src/app/hr/workforce-core/**`:<br>  * `FE-HRD-08`: Daftar SK Kepegawaian (`/personnel-action-orders`)<br>  * `FE-HRD-09`: Form Penerbitan SK (`/personnel-action-orders/create`)<br>  * `FE-HRD-09a`: Detail & Persetujuan SK (`/personnel-action-orders/[id]`)<br>  * `FE-HRD-09b`: Employee 360 Dual-View (`/employee-360/[id]`)<br>  * `FE-HRD-09c`: Batch Penempatan Awal (`/initial-placement-batch`)<br>  * `FE-HRD-09d`: Pengajuan Kuasa HR (`/employee-profile-changes/on-behalf`)<br>  * Form Master Data Pegawai: kunci input unit & jabatan jadi read-only |
| **Database Impact** | Migration 7 (`AddPersonnelActionOrderAndAssignmentApprovalFields`) menambahkan tabel baru dan kolom FK tanpa menghapus tabel penempatan yang ada |
| **Dependency** | Tahap 1: —<br>Tahap 2: Menunggu kelulusan Migration 7 dan modul penyimpanan dokumen terkelola (attachment scan SK) |
| **Blocking Decision** | Tidak ada. Seluruh keputusan `HRD-DEC-046` s.d. `HRD-DEC-057` telah `approved` pada revisi 16 |
| **QBE Impact** | `NEW CODE` untuk entitas operasional baru ber-prefix `Hrd` (mematuhi `QBE-MOD-002` dan entri registry resmi). Menggunakan `ApiResponse<T>`, logging, dan authorisasi `[AccessAction]` |
| **Migration Risk** | Rendah-Sedang. Tabel baru dan kolom nullable ditambahkan ke skema; inisialisasi batch baseline 55 pegawai dijalankan dalam transaksi atomik ber-rollback |
| **Acceptance Criteria** | 1. Tersedia API dan layar CRUD Dokumen SK Kepegawaian (Draft, Submit, Approve, Apply, Cancel).<br>2. Form Master Data Pegawai tidak mengizinkan pemindahan unit atau jabatan secara langsung.<br>3. Sistem menolak SK mutasi retroaktif bila tanggal efektif jatuh pada periode presensi tertutup.<br>4. Tampilan Employee 360 menyembunyikan kompensasi, rekening, NIK, keluarga, dan disiplin bagi Atasan Langsung.<br>5. Batch penempatan awal 55 pegawai berhasil dieksekusi tanpa melanggar aturan 1 penempatan utama per tanggal.<br>6. Pengajuan perubahan data oleh HR atas nama pegawai (on-behalf) mewajibkan verifikasi petugas berbeda |
| **Release Status** | `READY` |

### `S-A2` — Layanan mandiri cuti

| Aspek | Isi |
| --- | --- |
| **Current State** | Backend menyediakan 93 endpoint cuti dan lima controller layanan mandiri: pengajuan, saldo, kalender, pembatalan, dan kembali kerja. Frontend memanggil **nol** di antaranya. Pegawai tidak dapat mengajukan cuti dari sistem |
| **Target State** | Pegawai dapat melihat saldo, melihat kalender cuti unitnya, mengajukan cuti, membatalkan pengajuan, dan mencatat kembali kerja |
| **Backend Impact** | Diharapkan nihil. Bila ada kebutuhan yang tidak terlayani endpoint existing, itu `EXTEND` dan wajib dilaporkan, bukan diam-diam ditambal di frontend |
| **Frontend Impact** | Halaman baru di `src/app/self-services/human-resource/employee/leave/**` sesuai `HRD-DEC-007`. Redux slice dan hook baru mengikuti pola `attendance-capture-slice.jsx` |
| **Database Impact** | Tidak ada |
| **Dependency** | `HRD-DEP-002` untuk jalur persetujuannya |
| **Blocking Decision** | Tidak ada untuk pengajuannya. Nilai kebijakan seperti hak cuti per jenis pegawai adalah `HRD-Q-06`, tetapi itu isi master data, bukan penghalang alurnya |
| **QBE Impact** | Frontend saja. Tidak ada aturan QBE backend yang terpicu bila tidak ada `EXTEND` |
| **Migration Risk** | Nihil |
| **Acceptance Criteria** | 1. Pegawai dapat mengajukan cuti dan melihat saldonya berkurang sesuai aturan backend. 2. Pengajuan yang dibatalkan mengembalikan saldo sesuai aturan backend, bukan aturan yang dihitung frontend. 3. Frontend tidak menghitung ulang saldo maupun kelayakan; seluruhnya mengikuti jawaban backend. 4. Keadaan memuat, kosong, gagal, dan coba lagi tertangani |
| **Release Status** | `READY` |

### `S-A3` — Layanan mandiri lembur

| Aspek | Isi |
| --- | --- |
| **Current State** | 78 endpoint lembur tersedia, mencakup rencana, realisasi, verifikasi, dan rekonsiliasi. `OvertimeSelfServiceController` tersedia. Frontend memanggil nol |
| **Target State** | Pegawai dapat mengajukan lembur, melihat status pengajuannya, dan melihat hasil verifikasi |
| **Backend Impact** | Diharapkan nihil |
| **Frontend Impact** | Halaman baru di bawah `self-services/human-resource/employee/overtime/**` |
| **Database Impact** | Tidak ada |
| **Dependency** | `HRD-DEP-002` |
| **Blocking Decision** | Tarif dan kelayakan lembur adalah `HRD-Q-06`; keduanya isi master data |
| **QBE Impact** | Frontend saja |
| **Migration Risk** | Nihil |
| **Acceptance Criteria** | 1. Pengajuan lembur muncul di kotak masuk atasan yang benar. 2. Frontend tidak menghitung tarif; nominal berasal dari backend. 3. Pengajuan ganda untuk rentang waktu yang sama ditolak backend dan pesannya terbaca pengguna |
| **Release Status** | `READY` |

### `S-A4` — Layanan mandiri ubah jadwal dan tukar shift

| Aspek | Isi |
| --- | --- |
| **Current State** | `ScheduleChangeSelfServiceController` dan `ShiftSwapSelfServiceController` tersedia. Model `WfpScheduleChangeRequest` dan `WfpShiftSwapRequest` ada. Frontend memanggil nol |
| **Target State** | Pegawai dapat mengajukan perubahan jadwal, dan mengajukan tukar shift dengan rekan yang dituju |
| **Backend Impact** | Diharapkan nihil |
| **Frontend Impact** | Dua alur terpisah. Tukar shift melibatkan dua pihak, sehingga tampilannya berbeda dari ubah jadwal biasa |
| **Database Impact** | Tidak ada |
| **Dependency** | `HRD-DEP-002` |
| **Blocking Decision** | Tidak ada |
| **QBE Impact** | Frontend saja |
| **Migration Risk** | Nihil |
| **Acceptance Criteria** | 1. Tukar shift memerlukan persetujuan rekan yang dituju **dan** atasan, sesuai aturan backend. 2. Tukar shift yang melanggar aturan istirahat ditolak backend dan alasannya terbaca. 3. Jadwal yang sudah masuk periode kehadiran tertutup tidak dapat diubah |
| **Release Status** | `READY` |

### `S-A5` — Layanan mandiri koreksi kehadiran, perubahan data, dan pengunduran diri

| Aspek | Isi |
| --- | --- |
| **Current State** | `AttendanceCorrectionSelfServiceController`, `EmployeeProfileChangeSelfServiceController`, dan `ResignationSelfServiceController` tersedia. Frontend memanggil nol. Koreksi kehadiran sudah punya jalur unggah bukti di `AttendanceCorrectionController.cs:128` |
| **Target State** | Pegawai dapat mengajukan koreksi kehadiran beserta bukti, mengajukan perubahan data pribadi, dan mengajukan pengunduran diri |
| **Backend Impact** | Diharapkan nihil |
| **Frontend Impact** | Tiga alur terpisah. Koreksi kehadiran memerlukan unggah lampiran |
| **Database Impact** | Tidak ada |
| **Dependency** | `HRD-DEP-002`, `HRD-DEP-006` untuk lampiran bukti |
| **Blocking Decision** | Tidak ada |
| **QBE Impact** | Frontend saja |
| **Migration Risk** | Nihil |
| **Acceptance Criteria** | 1. Koreksi kehadiran menyimpan alasan, bukti, pelaku, dan waktu. 2. Rekaman kehadiran mentah **tidak berubah** oleh koreksi; yang berubah hasil olahannya. 3. Perubahan data pribadi tidak langsung berlaku sebelum disetujui. 4. Pengunduran diri tidak menghapus riwayat kepegawaian |
| **Release Status** | `READY` |

### `S-A6` — Pemindahan halaman absensi ke konvensi baku

| Aspek | Isi |
| --- | --- |
| **Current State** | Halaman absensi berada di `src/app/karyawan/Absensi-Karyawan/FormAbsensi/page.jsx`, memakai Bahasa Indonesia dan PascalCase. Dashboard pegawai yang setara berada di `src/app/self-services/human-resource/employee/dashboard/` |
| **Target State** | Halaman absensi berada di `src/app/self-services/human-resource/employee/attendance/`. Folder `src/app/karyawan/**` tidak dipakai lagi untuk halaman baru |
| **Backend Impact** | Tidak ada |
| **Frontend Impact** | Pemindahan berkas route. Komponen `attendance-employee-view.jsx` tidak perlu dipindah karena sudah berada di lokasi yang benar |
| **Database Impact** | Tidak ada |
| **Dependency** | Tidak ada |
| **Blocking Decision** | `HRD-DEC-007` sudah `approved` |
| **QBE Impact** | Frontend saja |
| **Migration Risk** | Nihil untuk data. Perlu diperiksa apakah ada tautan yang menunjuk alamat lama |
| **Acceptance Criteria** | 1. Tidak ada `page.jsx` layanan mandiri HR di luar `src/app/self-services/human-resource/`. 2. Fungsi pencatatan kehadiran tetap bekerja persis seperti sebelumnya. 3. Bila alamat lama masih ditautkan dari mana pun, disediakan pengalihan |
| **Release Status** | `READY` |

Catatan: pekerjaan pada empat berkas absensi yang saat ini belum di-commit di frontend perlu
diselesaikan lebih dulu, supaya pemindahan tidak bertabrakan dengan perubahan yang sedang
berjalan.

### `S-A7` — Kotak masuk persetujuan terpadu

| Aspek | Isi |
| --- | --- |
| **Current State** | `WorkflowManagement` menyediakan 48 endpoint sebagai mesin persetujuan bersama, termasuk `ApprovalInboxController` yang sudah menyatukan query lintas domain. **Tidak ada satu pun antarmuka persetujuan** di frontend, dan tidak ada folder `src/app/manajer`. **SLA/eskalasi: `MISSING` sebagai mesin penegakan.** `DueAt`/`ReminderAfterHours`/`EscalationAfterHours`/`AutoApproveAfterHours`/`AutoRejectAfterHours` ada sebagai field konfigurasi pada `MstWorkflowStep`, tapi **tidak ada** `BackgroundService`/`IHostedService` yang membacanya dan bertindak — dibuktikan `flows/09-unified-approval.md` |
| **Target State** | Satu halaman berisi seluruh pengajuan yang menunggu persetujuan orang yang sedang login, lintas jenis transaksi: cuti, lembur, tukar shift, ubah jadwal, koreksi kehadiran, perubahan data, dan pengunduran diri. **`HRD-DEC-030`, 28 Agustus 2026: reminder/escalation engine adalah target `EXTEND`** — `DueAt`/`ReminderAfterHours`/`EscalationAfterHours` harus benar-benar dieksekusi scheduled processing. `AutoApproveAfterHours`/`AutoRejectAfterHours` **default OFF**, hanya aktif bila `WorkflowDefinitionId` transaksi secara eksplisit mengizinkan — dilarang berlaku otomatis ke seluruh transaksi HR |
| **Backend Impact** | `EXTEND`. Perlu satu endpoint yang menjawab "apa yang menunggu persetujuan saya", lintas jenis transaksi, dengan bentuk ringkasan yang seragam — **sudah ada** lewat `ApprovalInboxController`. **`EXTEND` tambahan**: mesin penegakan SLA/eskalasi/auto-approve/auto-reject, `HRD-DEC-030`. Aturan bisnis tetap milik masing-masing domain |
| **Frontend Impact** | Halaman baru untuk atasan. Baris ringkasan seragam, detail tetap dibuka di halaman transaksi masing-masing |
| **Database Impact** | Tidak ada tabel baru. Kemungkinan perlu index pada penugasan penyetuju dan status instance |
| **Dependency** | `HRD-DEP-002` |
| **Blocking Decision** | `HRD-DEC-011` dan `HRD-DEC-018` sudah `approved`. `HRD-Q-12` dan `HRD-Q-13` masih terbuka, tetapi keduanya menyangkut rincian, bukan bentuk dasarnya |
| **QBE Impact** | `NEW CODE` untuk endpoint ringkasan; wajib `ApiResponse<T>` dan `PagedResult<T>`. Tidak boleh membuat entity baru untuk menampung ringkasan |
| **Migration Risk** | Rendah. Penambahan index dapat dijalankan tanpa mematikan layanan |
| **Acceptance Criteria** | 1. Tiga pengajuan berbeda jenis dengan atasan yang sama muncul pada satu halaman. 2. Aturan saldo cuti tetap dipakai untuk cuti, dan aturan kelayakan lembur tetap dipakai untuk lembur — kotak masuk tidak menyeragamkan keduanya. 3. Batas waktu tanggapan dan jalur eskalasi tiap jenis transaksi tetap berbeda dan tetap berlaku. 4. Orang yang tidak berwenang tidak melihat pengajuan yang bukan haknya |
| **Release Status** | `READY` |

`HRD-DEC-018` adalah pagar terpenting slice ini. Yang boleh diseragamkan hanya bentuk baris,
cara memfilter, penanda status, dan cara berpindah ke detail. Workflow, policy, permission,
validasi, SLA, dan eskalasi tetap milik domain masing-masing.

---

## 3. Gelombang B — Administrasi waktu kerja

### `S-B1` — Administrasi kehadiran

| Aspek | Isi |
| --- | --- |
| **Current State** | 71 endpoint pada 9 controller, mencakup rekaman mentah, pemrosesan harian, pengecualian, periode tutup/buka kembali, koreksi, dan serah terima payroll. Frontend memanggil nol untuk fitur baru. |
| **Target State** | Presensi staf RS 24/7 komprehensif: normalisasi shift malam lintas hari ke tanggal mulai (`ShiftDate = Hari H`, `HRD-DEC-104`), toleransi keterlambatan 10 menit (`HRD-DEC-105`), konversi otomatis 1 punch `IncompletePunch` ke `Absent` saat tutup periode (`HRD-DEC-106`), izin pulang cepat non-deductible (`HRD-DEC-107`, menutup `HRD-Q-47`), penutupan periode ber-guard koreksi pending & buka kembali darurat HR Manager ber-rollback payroll (`HRD-DEC-108`), serta penugasan dinas luar OnDuty (`HRD-DEC-109`). Termasuk jalur koreksi on-behalf kuasa HR (`HRD-DEC-028`). |
| **Backend Impact** | **`EXTEND`**: migration EF Core 4 model baru (`HrdAttendanceSetting`, `HrdEarlyLeavePermission`, `HrdOfficialDutyRequest`, `HrdAttendancePeriodReopenLog`), refaktor `AttendanceDailyProcessingService`, implementasi guard tutup periode & reopen service ber-rollback payroll, serta consumer event koreksi roster (`INT-HR-41`). |
| **Frontend Impact** | 6 layar baru (`FE-HRD-144` s.d. `FE-HRD-149`), 1 drawer log biometrik mentah, modal koreksi on-behalf, dialog guard tutup periode, modal buka kembali darurat, dan penyelarasan portal ESS presensi. |
| **Database Impact** | 4 tabel operasional baru dengan prefix `Hrd`, indeks performa, dan relasi referensial aman. |
| **Dependency** | `BE-HRD-006` ✅ (fondasi presensi existing), `BE-HRD-097` (event mutasi jadwal roster dari S-B4). |
| **Blocking Decision** | Tidak ada. Seluruh keputusan `HRD-DEC-104` s.d. `HRD-DEC-109` sudah `approved` pada 4 November 2026. |
| **QBE Impact** | `NEW CODE` untuk controller, service, dan entity baru. Seluruh endpoint wajib `ApiResponse<T>`, DTO terpisah dari model domain. |
| **Migration Risk** | Rendah. Tidak ada modifikasi tabel legacy yang merusak data. |
| **Acceptance Criteria** | Memenuhi seluruh 20 kasus uji penerimaan `AT-HRD-ATT-01` s.d. `AT-HRD-ATT-20` dan 5 skenario UAT `UAT-ATT-01` s.d. `UAT-ATT-05`. |
| **Release Status** | `PLANNED` (13 task BE-HRD-104 s.d. BE-HRD-116, 14 task FE-HRD-088 s.d. FE-HRD-101 pada roadmap revision 9) |

### `S-B2` — Administrasi cuti dan saldo

| Aspek | Isi |
| --- | --- |
| **Current State** | 93 endpoint pada 12 controller dengan celah audit kebebasan `/reverse` tanpa guard status (`HRD-Q-35`) dan ketiadaan alur approval berjenjang leave recall darurat (`HRD-Q-36`). |
| **Target State** | Alokasi hak cuti 12 hari & carry-forward max 6 hari s.d. 30 Juni via buku besar mutasi `TrxLeaveBalanceLedger` (`HRD-DEC-110`), evaluasi ketersediaan staf unit 24/7 *Safe Staffing Coverage Warning* ke modul Roster 06 (`HRD-DEC-111`), formulir cuti mandiri ESS & half-day 0.5 hari (`HRD-ASM-20`), cuti sakit $\ge 2$ hari wajib surat dokter ber-SIP dengan proteksi privasi medis K3RS (`HRD-DEC-112`), alur leave recall darurat berotorisasi HR Manager dengan pemulihan saldo otomatis (`HRD-DEC-113`), penutupan celah `/reverse` HTTP 409 Conflict (`HRD-DEC-115`), dan rekonsiliasi retrospektif saat cut-off payroll (`HRD-DEC-114`). |
| **Backend Impact** | Pembuatan entitas `TrxLeaveBalanceLedger`, `TrxLeaveRecallRequest`, perluasan `WfpLeaveRequest` & `TrxLeaveAdjustment`, refaktor guard status `/reverse` 409 pada `LeaveExecutionProcessorService`, scheduler carry-forward & kedaluwarsa 30 Juni, service coverage warning & privasi K3RS. |
| **Frontend Impact** | 6 layar baru (`FE-HRD-150` s.d. `FE-HRD-155`) di `/hr/leave/**`, banner coverage warning, modal upload surat dokter ber-SIP, modal inisiasi & approval leave recall, modal HR adjustment. |
| **Database Impact** | Migration EF Core `AddLeaveLedgerCoverageWarningAndRecallExtensions` (tabel `trx_leave_balance_ledgers`, `trx_leave_recall_requests`, kolom baru pada `wfp_leave_requests`, `trx_leave_adjustments`). |
| **Dependency** | `BE-HRD-007` ✅ (fondasi cuti existing), `BE-HRD-087` (Roster Period dari S-B4), `BE-HRD-104` (Presensi harian S-B1). |
| **Blocking Decision** | Nihil — seluruhnya telah disetujui pada `HRD-DEC-110` s.d. `HRD-DEC-115` dan `HRD-ASM-19` s.d. `HRD-ASM-22`. |
| **QBE Impact** | `QBE-CONFORMANCE` untuk backend C# ASP.NET Core & linter/build untuk frontend Next.js App Router. |
| **Migration Risk** | Sangat rendah (bersifat non-breaking aditif / nullable columns). |
| **Acceptance Criteria** | 20 kriteria uji penerimaan `AT-HRD-LEV-01` s.d. `AT-HRD-LEV-20` dan 5 skenario `UAT-LEV-01` s.d. `UAT-LEV-05`. |
| **Release Status** | **`PLANNED`** — Desain disetujui pengguna 4 November 2026 (`HRD-DEC-110` s.d. `HRD-DEC-115`). Direncanakan 14 task backend (`BE-HRD-117` s.d. `BE-HRD-130`) dan 14 task frontend (`FE-HRD-102` s.d. `FE-HRD-115`) pada roadmap revision `10` lintas gelombang `MVP-24` s.d. `MVP-26` & Verifikasi. |

**Catatan batas — Early Leave Permission bukan bagian slice ini.** `HRD-DEC-029`, 28 Agustus
2026, menetapkan Early Leave Permission (izin administratif pulang cepat) sebagai konsep
**terpisah** dari Hourly Leave (mode `IsHourly` pada `WfpLeaveRequest`, sudah tercakup `S-B2`).
Early Leave Permission adalah bagian attendance/permission flow (`S-B1`), bukan Leave Management.

### `S-B3` — Administrasi lembur

| Aspek | Isi |
| --- | --- |
| **Current State** | 78 endpoint pada 9 controller, mencakup rencana, realisasi, verifikasi, rekonsiliasi, penjadwal penutupan, dan serah terima. Model operasional eksisting meliputi `TrxOvertimePlan`, `TrxOvertimeRealization`, `TrxOvertimeVerification`, `TrxCompensatoryTimeOff`, `TrxOvertimePeriod`, `TrxOvertimeRequestApproval`, dll |
| **Target State** | Administrasi lembur rumah sakit menyeluruh: pengajuan SPKL terencana H-1 & klaim darurat medis cito pasca-dinas H+1 (`HRD-DEC-116`), pembatasan anti-self-approval mutlak (`HRD-DEC-117`), persetujuan eskalasi Direktur Pelayanan Medik untuk kondisi cito penyelamatan nyawa yang melampaui kuota PP 35/2021 (`HRD-DEC-118`), verifikasi silang mesin biometrik presensi dengan aturan batas minimum 30 menit & kewajiban upload bukti logbook fisik jika selisih > 15 menit (`HRD-DEC-119`), hak cuti pengganti lembur (Compensatory Time-Off / CTO) berlaku 90 hari kalender (`HRD-DEC-120`), serta manajemen penutupan periode cut-off tanggal 20 dengan rekonsiliasi rapel retrospektif ke payroll bulan berikutnya (`HRD-DEC-121`). Telah dirancang pada `02-backend-architecture.md` bagian 22 dan `03-frontend-architecture.md` bagian 18 |
| **Backend Impact** | **`EXTEND` terhadap model operasional lembur eksisting & penambahan service kebijakan lembur**. 15 task backend direncanakan (`BE-HRD-131` s.d. `BE-HRD-145`): fondasi skema database (`BE-HRD-131`), service kebijakan kuota RS (`BE-HRD-132`), DTO & validasi FluentValidation (`BE-HRD-133`), service SPKL pra-dinas H-1 & penugasan tim (`BE-HRD-134`), service klaim darurat cito H+1 & nomor RM (`BE-HRD-135`), engine kuota PP 35/2021 & deteksi eskalasi (`BE-HRD-136`), workflow approval 2-tahap anti-self-approval (`BE-HRD-137`), otorisasi Direktur Medik darurat cito (`BE-HRD-138`), verifikasi silang presensi biometrik & min-duration rule (`BE-HRD-139`), manajemen discrepancy presensi & bukti fisik (`BE-HRD-140`), service hak libur pengganti CTO 90 hari (`BE-HRD-141`), scheduler hangus CTO kedaluwarsa (`BE-HRD-142`), manajemen periode cut-off tgl 20 & retrospektif (`BE-HRD-143`), serah terima data lembur terkunci ke Payroll (`BE-HRD-144`), dan verifikasi E2E QBE arsitektur (`BE-HRD-145`) |
| **Frontend Impact** | 14 task frontend (`FE-HRD-116` s.d. `FE-HRD-129`) mencakup API service client Axios, Redux slice, komponen form dual-mode SPKL & cito, drawer investigasi discrepancy biometrik, kotak masuk persetujuan ruangan & Direktur Medik, pemantau tiket saldo CTO 90 hari, tabel rekonsiliasi rapel, serta 6 layar: `FE-HRD-156` (Pengajuan SPKL & Klaim Cito ESS), `FE-HRD-157` (Kotak Masuk Approval Lembur Ruangan), `FE-HRD-158` (Pusat Verifikasi Presensi & Discrepancy HR), `FE-HRD-159-A` (Kotak Masuk Approval Direktur Medik), `FE-HRD-159` (Monitoring Saldo Cuti Pengganti CTO), `FE-HRD-160` (Penutupan Periode Lembur & Payroll Close), dan `FE-HRD-161` (Pengaturan Kebijakan & Kuota Lembur RS) |
| **Database Impact** | Tabel konfigurasi batas lembur RS `hrd_overtime_settings`, kolom otorisasi khusus eskalasi Direktur Medik, nomor RM dan bukti logbook fisik dinas, kolom status tiket saldo CTO 90 hari, serta indeks performa query lembur per pegawai, unit kerja, dan rentang tanggal cut-off |
| **Dependency** | `HRD-DEP-002` |
| **Blocking Decision** | `HRD-DEC-116` s.d. `HRD-DEC-121` telah menyelesaikan seluruh keputusan bisnis: model dual pengajuan, pelarangan self-approval, eskalasi darurat klinis, batas toleransi presensi, kedaluwarsa CTO 90 hari, dan batas cut-off tgl 20. Tidak ada blocking decision terbuka |
| **QBE Impact** | `NEW CODE` untuk service logic validasi kuota, workflow approval ruangan & Direktur Medik, scheduler hangus CTO, dan handoff payroll; `TOUCHED LEGACY` untuk DDL tabel lembur eksisting |
| **Migration Risk** | Rendah — skema database memperluas tabel eksisting dengan kolom nullable/default aman dan tabel konfigurasi baru. Tidak ada migrasi destruktif |
| **Acceptance Criteria** | 1. SPKL pra-dinas diajukan H-1 dan klaim darurat medis cito diajukan H+1 dengan nomor RM pasien. 2. Self-approval diblokir mutlak oleh sistem. 3. Pelampauan kuota PP 35/2021 pada kondisi cito wajib diotorisasi Direktur Medik. 4. Durasi lembur aktual di bawah 30 menit otomatis diakui 0 jam. 5. Saldo tiket CTO hangus otomatis setelah 90 hari kalender. 6. Lembur terverifikasi pasca-cut-off tanggal 20 dirapel retrospektif ke payroll bulan berikutnya |
| **Release Status** | **`PLANNED`** — Desain disetujui pengguna 5 November 2026 (`HRD-DEC-116` s.d. `HRD-DEC-121`). Direncanakan 15 task backend (`BE-HRD-131` s.d. `BE-HRD-145`) dan 14 task frontend (`FE-HRD-116` s.d. `FE-HRD-129`) pada roadmap revision `11` lintas gelombang `MVP-27` s.d. `MVP-29` & Verifikasi |

### `S-B4` — Penjadwalan kerja

| Aspek | Isi |
| --- | --- |
| **Current State** | **`MISSING API` untuk operational roster core**, bukan sekadar "backend tipis". 22 endpoint pada 3 controller melayani hanya 3 dari 11 model (`WfpWorkScheduleAssignment`, `WfpScheduleChangeRequest`, `WfpShiftSwapRequest`). **Delapan model tanpa satu pun controller**: `TrxRosterPeriod`, `TrxRosterAssignment`, `TrxRosterPublication`, `TrxRosterApproval` (seluruh mesin roster), `TrxShiftAssignment` (penugasan shift harian), `TrxShiftReplacement` (penggantian shift), `TrxEmergencyStaffingRequest` (tenaga darurat), `TrxOnCallAssignment` (penugasan siaga aktual) — dibuktikan `PHASE 2B`, `flows/05-work-scheduling.md`. `DefaultWorkScheduleSeeder` mengisi satu jadwal bawaan berkode `SCH-RSMMC-DEFAULT` saat aplikasi start |
| **Target State** | Penyusunan jadwal per unit dan per periode, penugasan shift harian, deteksi bentrok fisik/hukum (`B1`–`B7`), peringatan beralasan (`W1`–`W7`), pengaturan kebijakan penjadwalan HR (`HrdSchedulingSetting`), penggantian shift mendadak (*override*) langsung aktif tanpa menurunkan ke Draft (`HRD-DEC-101`), batas toleransi koreksi mundur maksimal 3 hari ke belakang (`HRD-DEC-102`), penguncian mutlak periode `Closed` saat cut-off payroll, serta tata kelola siaga on-call dokter & staf dengan pemantauan SLA respon menit dan integrasi Lab (`LAB-DEC-111` / `INT-HR-39`) dan Payroll (`INT-HR-40`). Telah dirancang pada `02-backend-architecture.md` bagian 19 dan `03-frontend-architecture.md` bagian 15. |
| **Backend Impact** | **`EXTEND` terhadap 8 model operational roster existing + 1 entity konfigurasi baru `HrdSchedulingSetting`**, `HRD-DEC-026`. 17 task backend direncanakan (`BE-HRD-087` s.d. `BE-HRD-103`): fondasi skema setting & on-call (`BE-HRD-087`), setting controller (`BE-HRD-088`), mesin validasi keselamatan B1–B7 & W1–W7 (`BE-HRD-089`), integrasi kesiapan staf `INT-HR-31` (`BE-HRD-090`), integrasi kualifikasi klinis `INT-HR-37` (`BE-HRD-091`), siklus roster unit (`BE-HRD-092`), salin periode & pola shift massal (`BE-HRD-093`), query matriks roster & pantau manajer (`BE-HRD-094`), penugasan shift harian (`BE-HRD-095`), penggantian shift darurat langsung efektif (`BE-HRD-096`), koreksi mundur 3 hari & pemicu hitung ulang kehadiran `INT-HR-38` (`BE-HRD-097`), penguncian periode closed (`BE-HRD-098`), manajemen on-call (`BE-HRD-099`), pemanggilan call-in & SLA menit (`BE-HRD-100`), integrasi Lab `INT-HR-39` (`BE-HRD-101`), handoff Payroll/Lembur `INT-HR-40` (`BE-HRD-102`), dan verifikasi E2E (`BE-HRD-103`). |
| **Frontend Impact** | 13 task frontend (`FE-HRD-075` s.d. `FE-HRD-087`) mencakup service client, Redux store, komponen matriks bulanan, drawer validasi B1–B7/W1–W7, modal salin/pola shift, modal ganti petugas, modal call-in & SLA, serta 6 layar: `FE-HRD-138` (Daftar Roster Unit), `FE-HRD-139` (Matriks Penyusunan & Terbit Roster), `FE-HRD-140` (Pantau Roster Bidang), `FE-HRD-141` (Jadwal Siaga On-Call Unit), `FE-HRD-142` (Riwayat Pemanggilan Cito & Respon), dan `FE-HRD-143` (Pengaturan Kebijakan Penjadwalan HR). |
| **Database Impact** | 1 tabel baru `hrd_scheduling_settings`, 4 kolom baru pada `trx_on_call_assignments` (`is_called_in`, `called_in_at`, `arrived_at`, `response_minutes`), serta penambahan indeks performa untuk pencarian shift dan validasi tumpang tindih waktu. |
| **Dependency** | `HRD-DEP-002` |
| **Blocking Decision** | `HRD-DEC-006` sudah memisahkan jadwal kerja dari jadwal praktik. `HRD-DEC-098` s.d. `HRD-DEC-103` telah menyelesaikan seluruh aturan penerbitan mandiri, validasi pemblokir vs peringatan, penggantian darurat langsung efektif, toleransi koreksi mundur, dan keselamatan kontrak Lab on-call. Tidak ada blocking decision terbuka. |
| **QBE Impact** | `NEW CODE` untuk endpoint tambahan pada delapan model yang di-`EXTEND` dan 1 entity `HrdSchedulingSetting`. Prefix `Hrd` dipatuhi per `QBE-MOD-002`. |
| **Migration Risk** | Rendah — penambahan kolom on-call bersifat nullable, tabel konfigurasi diisi seeder nilai default aman. Tidak ada penghapusan skema atau migrasi destruktif. |
| **Acceptance Criteria** | 1. Roster unit bergiliran terbit mandiri oleh Kepala Ruangan setelah lolos validasi otomatis `B1`–`B7` dan pengisian alasan `W1`–`W7`. 2. Kepala Bidang menerima notifikasi dan memiliki hak pantau *read-only*. 3. Penggantian petugas pasca-terbit langsung efektif tanpa mengubah status roster menjadi Draft. 4. Koreksi tanggal mundur dibatasi maksimal 3 hari ke belakang dan memicu hitung ulang absensi. 5. Periode `Closed` mengunci jadwal secara mutlak. 6. Status on-call tetap `Active` saat dipanggil untuk menjamin integritas kontrak Lab. |
| **Release Status** | **`PLANNED`** — Desain disetujui pengguna 4 November 2026 (`HRD-DEC-097` s.d. `HRD-DEC-103`). Direncanakan 17 task backend (`BE-HRD-087` s.d. `BE-HRD-103`) dan 13 task frontend (`FE-HRD-075` s.d. `FE-HRD-087`) pada roadmap revision `8` lintas gelombang `MVP-18` s.d. `MVP-20` & Verifikasi. |

### `S-B5` — Payroll sisi HR

| Aspek | Isi |
| --- | --- |
| **Current State** | 49 endpoint operasional legacy pada 6 controller. Blueprint arsitektur target V2 Slice `S-B5` telah dikunci lengkap pada Kontrak `v14` (7 entitas, 6 grup Swagger, 18 permissions, 14 audit events, 25 aturan validasi). |
| **Target State** | Petugas payroll menginisiasi siklus batch pasca cut-off tgl 20 dengan precondition gate (Sub-07, 08, 09 closed), kalkulasi gaji otomatis berparameter transparan PPh 21 TER PMK 168/2023 & BPJS plafon, deteksi deviasi net pay >= 20% (variance alert), otorisasi 3-jenjang SoD anti-self-approval, pembekuan immutable, serah terima berkas Finance SHA-256 (`INT-HR-53`), callback penanganan penolakan/rollback terkendali (`INT-HR-54`), dan distribusi slip gaji digital ESS ber-PIN 6-digit tgl 25. |
| **Backend Impact** | 7 entitas baru di schema `hrm` (`trx_payroll_runs`, `trx_payroll_item_details`, `trx_payroll_variance_alerts`, `trx_payroll_approvals`, `trx_payroll_handoff_batches`, `trx_payroll_payslips`, `hrd_payroll_settings`), DDL Postgres & migrasi EF Core, 15 task backend (`BE-HRD-146` s.d. `BE-HRD-160`). |
| **Frontend Impact** | 6 layar baru (`FE-HRD-162` s.d. `FE-HRD-167`), wizard inisiasi batch, lembar kerja kalkulasi gaji, drawer investigasi variansi, inbox approval eksekutif, portal slip gaji ESS ber-PIN, 14 task frontend (`FE-HRD-130` s.d. `FE-HRD-143`). |
| **Database Impact** | 7 tabel baru di schema `hrm` beserta indeks komposit performa kueri per unit kerja dan pegawai. |
| **Dependency** | `HRD-DEP-004` (Terpenuhi oleh Sub-07, Sub-08, dan Sub-09 yang telah selesai dirancang). |
| **Blocking Decision** | Ditutup penuh oleh `HRD-DEC-122` s.d. `HRD-DEC-127` dan asumsi `HRD-ASM-27` s.d. `HRD-ASM-30`. `HRD-Q-10` dan `HRD-Q-11` terjawab tuntas melalui kontrak serah terima berkas Finance SHA-256 (`INT-HR-53`) dan callback reconciler rollback terkendali ke `NeedCorrection` (`INT-HR-54`). |
| **QBE Impact** | Kepatuhan penuh QBE backend (repository pattern, DI, Clean Architecture) dan design token frontend Quilvian. |
| **Migration Risk** | Rendah: tabel payroll V2 independen tanpa mengubah skema legacy Finance. |
| **Acceptance Criteria** | 1. Pra-syarat presensi, cuti, lembur terkunci cut-off tgl 20 diverifikasi ketat (`PAY-B1`). 2. Perhitungan PPh 21 TER PMK 168/2023 presisi. 3. Variansi deviasi >= 20% memblokir submission tanpa justifikasi min 20 karakter. 4. Anti-self-approval HR Manager aktif. 5. Otorisasi final mengunci data permanen (*immutable*). 6. Paket serah terima Finance ber-checksum SHA-256. 7. Slip gaji ESS dilindungi PIN 6-digit & lockout 30 menit. |
| **Release Status** | `PLANNED` — Desain disetujui pengguna 5 November 2026 (`HRD-DEC-122` s.d. `HRD-DEC-127`). Direncanakan 15 task backend (`BE-HRD-146` s.d. `BE-HRD-160`) dan 14 task frontend (`FE-HRD-130` s.d. `FE-HRD-143`) pada roadmap revision `12` lintas gelombang `MVP-30` s.d. `MVP-32` & Verifikasi. |

### `S-B6` — Restitusi & Integrasi Pemotongan Penggajian (Restitution & Payroll Deduction Integration)

| Aspek | Isi |
| --- | --- |
| **Current State** | 6 endpoint legacy restitusi dan cicilan dasar. Blueprint arsitektur target V2 Slice `S-B6` telah dikunci lengkap pada Kontrak `v15` (6 entitas baru, 4 grup Swagger, 11 permissions, 12 audit events, 25 aturan validasi). |
| **Target State** | Sinkronisasi dua arah saldo cicilan piutang dari Departemen Keuangan H-3 cut-off tgl 17 (`INT-HR-57`), penegakan mutlak batas maksimal potongan non-regulasi 50% Gross Pay sesuai Pasal 65 PP 36/2021 jo PP 49/2025 dengan prioritas berjenjang (Ganti Rugi $
ightarrow$ Kasbon RS $
ightarrow$ Koperasi $
ightarrow$ Paguyuban) dan carry-over otomatis, portal pengajuan klaim mandiri kacamata & medis di ESS ber-kuota plafon tahunan dan alur approval 3-tier anti-self-approval, restitusi koreksi salah potong masa lalu tanpa revisi slip final (`PAY-B6`) berstatus non-pajak PPh 21, pelunasan utang penutupan PHK/Resign pada Exit Clearance Sub-Modul 20 (`INT-HR-60`), serta voucher pencairan tunai kasir darurat cito nakes 24/7 (`INT-HR-59`) dengan penguncian mutlak flag pencegah bayar ganda. |
| **Backend Impact** | 6 entitas baru (`hrd_restitution_settings`, `trx_employee_restitution_claims`, `trx_payroll_restitutions`, `trx_payroll_deduction_schedules`, `trx_payroll_deduction_items`, `trx_cashier_disbursement_vouchers`), DDL PostgreSQL & migrasi EF Core, 15 task backend (`BE-HRD-161` s.d. `BE-HRD-175`). |
| **Frontend Impact** | 6 layar baru (`FE-HRD-168` s.d. `FE-HRD-173`), dasbor pemantauan rasio potong 50%, portal ESS pengajuan klaim staf, lembar kerja angsuran pinjaman, drawer investigasi koreksi salah potong, panel verifikasi approval & QR voucher cito, 14 task frontend (`FE-HRD-144` s.d. `FE-HRD-157`). |
| **Database Impact** | 6 tabel baru di schema `hrm` beserta indeks unik nomor klaim/voucher dan indeks komposit performa evaluasi kuota per pegawai. |
| **Dependency** | `HRD-DEP-004` (Terpenuhi oleh Sub-10 Payroll `S-B5` yang telah selesai dirancang dan di-plan). |
| **Blocking Decision** | Ditutup penuh oleh `HRD-DEC-128` s.d. `HRD-DEC-133` dan asumsi `HRD-ASM-31` s.d. `HRD-ASM-34`. Kepatuhan regulasi pemotongan upah ditegakkan lewat Pasal 65 PP 36/2021 jo PP 49/2025. |
| **QBE Impact** | Kepatuhan penuh QBE backend (repository pattern, DI registration, Clean Architecture) dan design token frontend Quilvian. |
| **Migration Risk** | Rendah: tabel restitusi dan pemotongan V2 terisolasi tanpa mengubah skema AR Keuangan. |
| **Acceptance Criteria** | 1. Total potongan non-regulasi tidak pernah melebihi 50% Gross Pay (`DED-B1`). 2. Hirarki prioritas berjenjang 1-4 dieksekusi konsisten dan sisa tagihan tercatat `deferred_amount` (`DED-B2`). 3. Sinkronisasi H-3 Finance tgl 17 dan callback tgl 25 berjalan atomik. 4. ESS menolak klaim kacamata melebihi kuota plafon tahunan golongan atau kuitansi > 60 hari. 5. Anti-self-approval atasan aktif. 6. Koreksi salah potong masa lalu linked ke slip lama dan bebas pajak PPh 21. 7. Klaim cair di kasir berstatus `DisbursedViaCashier` dikunci mutlak dari slip gaji tgl 25. 8. Sisa pinjaman staf PHK dipotong hingga 100% dari pesangon penutupan Sub-20. |
| **Release Status** | `PLANNED` — Desain disetujui pengguna 5 November 2026 (`HRD-DEC-128` s.d. `HRD-DEC-133`). Direncanakan 15 task backend (`BE-HRD-161` s.d. `BE-HRD-175`) dan 14 task frontend (`FE-HRD-144` s.d. `FE-HRD-157`) pada roadmap revision `13` lintas gelombang `MVP-33` s.d. `MVP-35` & Verifikasi. |
---

## 4. Gelombang C — Pengembangan orang

### `S-C2` — Kompetensi dan pelatihan (Learning, Training & Competency)

| Aspek | Isi |
| --- | --- |
| **Current State** | 18 endpoint pada 2 controller lama; master katalog dasar ada namun belum terintegrasi dengan akreditasi rumah sakit, ujian OSCE, maupun radar kedaluwarsa nakes |
| **Target State** | Standar 5 pelatihan wajib akreditasi STARKES KPS 8 (100% pegawai aktif, siklus 2 tahun), pemisahan presensi (80%/100%) vs kelulusan kompetensi via post-test teori (>=75) dan simulasi OSCE klinis langsung (>=80) ber-e-sign assessor, radar kedaluwarsa H-90/60/30 & safeguard dinas shift *buddy system* ke Sub-06 (`INT-HR-79`), prasyarat penurunan RKK Level 4 ke Level 3 supervisi ke Sub-14 (`INT-HR-77`), TNA & kontrol pagu anggaran diklat departemen, penerbitan otomatis akta ikatan dinas retensi kerja nakes > Rp 10 Jt / > 1 bulan beserta tagihan denda wanprestasi resign sepihak, visual tracker target akumulasi 50 SKP dalam 5 tahun, verifikasi API Plataran Sehat Kemenkes (`INT-HR-80`), serta agregasi bukti CPD ke pilar *Medical Knowledge* evaluasi OPPE Sub-15 (`INT-HR-78`) |
| **Backend Impact** | `EXTEND`. 16 task backend (`BE-HRD-238` s.d. `BE-HRD-253`) mencakup 10 model tabel relasional Postgres (`mst_mandatory_training_rules`, `trx_training_plans`, `trx_training_sessions`, `trx_training_enrollment_requests`, `trx_training_attendances`, `trx_training_assessments`, `trx_training_certificates`, `trx_training_budgets`, `hrd_training_bond_contracts`, `trx_plataran_sehat_skp_records`), 24 endpoint API Kontrak `v20`, 8 aturan bisnis `TRN-B1`..`B8`, dan 6 background worker `TRN-W1`..`W6` |
| **Frontend Impact** | `EXTEND`. 15 task frontend (`FE-HRD-216` s.d. `FE-HRD-230`) untuk 8 layar antarmuka (`FE-HRD-216` s.d. `FE-HRD-223`), 5 komponen modal/drawer, dan Redux slice `learningDevelopmentSlice.ts` |
| **Database Impact** | 10 tabel relasional baru/terekspansi dengan UUID PK, audit columns, soft-delete, dan foreign keys terindeks lengkap |
| **Dependency** | `HRD-DEP-006` (Sertifikat & e-Dossier Sub-02), `INT-HR-77` (Sub-14 Kredensialing), `INT-HR-78` (Sub-15 OPPE), `INT-HR-79` (Sub-06 Scheduling), `INT-HR-80` (Plataran Sehat Kemenkes) |
| **Blocking Decision** | Selesai & Approved — `HRD-DEC-158` s.d. `HRD-DEC-163` (Revisi 30) |
| **QBE Impact** | `NEW CODE` untuk controller, services, background workers, dan client API contracts |
| **Migration Risk** | Rendah — tabel baru terisolasi dalam skema EF Core dengan penamaan konvensi Quilvian |
| **Acceptance Criteria** | Seluruh 25 aturan validasi (`VR-TRN-01` s.d. `VR-TRN-25`), 22 skenario acceptance test (`AT-HRD-TRN-01` s.d. `AT-HRD-TRN-22`), dan 5 skenario UAT terpenuhi tanpa celah |
| **Release Status** | `PLANNED` |

### `S-C3` — Manajemen kinerja

| Aspek | Isi |
| --- | --- |
| **Current State** | 18 endpoint pada 2 controller, model berjumlah 11. Master data sudah lengkap: siklus, skala, template, dan katalog KPI |
| **Target State** | Siklus penilaian berjalan, atasan menilai, pegawai melihat hasilnya, dan riwayat tersimpan |
| **Backend Impact** | `EXTEND` |
| **Frontend Impact** | Kelompok halaman baru untuk sisi transaksi |
| **Database Impact** | Kemungkinan `EXTEND` kolom |
| **Dependency** | `HRD-DEP-002` bila penilaian memerlukan persetujuan berjenjang |
| **Blocking Decision** | Tidak ada untuk staf nonklinis |
| **QBE Impact** | `NEW CODE` untuk endpoint tambahan |
| **Migration Risk** | Rendah |
| **Acceptance Criteria** | 1. Satu pegawai tidak dapat dinilai dua kali pada siklus yang sama. 2. Penilaian yang sudah final tidak dapat diubah diam-diam; koreksi menghasilkan revisi. 3. Pegawai melihat hasilnya sendiri, bukan hasil orang lain |
| **Release Status** | `READY` |

Catatan batas: kontribusi mutu dan keselamatan pasien untuk tenaga medis dirancang pada `S-C1-OPPE` yang telah berstatus `PLANNED` (Roadmap Revisi 17).

### `S-C4` — Lifecycle dan offboarding

| Aspek | Isi |
| --- | --- |
| **Current State** | Rasio paling timpang di seluruh modul: **21 model, 1 controller, 7 endpoint**. Yang matang hanya pengunduran diri |
| **Target State** | Onboarding, orientasi, evaluasi masa percobaan, pemberhentian, pensiun, penyelesaian administrasi keluar, pengembalian aset, dan penutupan offboarding |
| **Backend Impact** | `EXTEND` besar. Dua puluh model sudah ada tetapi belum berperilaku |
| **Frontend Impact** | Kelompok halaman baru yang cukup luas |
| **Database Impact** | Model sudah ada. Kemungkinan `EXTEND` kolom, bukan tabel baru |
| **Dependency** | `HRD-DEP-003` untuk pembuatan dan pencabutan akun |
| **Blocking Decision** | Lama masa percobaan dan hasilnya adalah `HRD-Q-06` |
| **QBE Impact** | `NEW CODE` untuk endpoint baru. Untuk model existing, kelas keberlakuan ditentukan **per entity**, bukan digeneralisasi untuk seluruh slice — lihat tabel di bawah |
| **Migration Risk** | Sedang. Penambahan kolom pada 20 model perlu direncanakan bertahap, tidak sekaligus |
| **Acceptance Criteria** | 1. Pemberhentian tidak menghilangkan riwayat kepegawaian, kredensial, kinerja, maupun payroll. 2. Penyelesaian administrasi keluar tidak dapat ditutup selama masih ada aset yang belum dikembalikan. 3. Pencabutan akses tercatat sebagai bagian offboarding |
| **Release Status** | `READY` |

**Kelas keberlakuan QBE ditentukan per entity.** Slice ini menyentuh 21 model dengan dua keluarga
prefix yang perlakuannya berbeda, sehingga tidak boleh digeneralisasi:

| Keluarga entity | Contoh di slice ini | Bila materially touched | Kelas QBE |
| --- | --- | --- | --- |
| `Wfp*` | `WfpOnboardingChecklist`, `WfpOnboardingTask`, `WfpOffboardingChecklist`, `WfpOffboardingTask` | **Tetap `Wfp*`.** Tidak ada rename | `NEW CODE` untuk kolom baru; **bukan** `TOUCHED LEGACY` |
| `Trx*` milik HR | `TrxEmployeeOnboarding`, `TrxProbationReview`, `TrxEmployeeSeparation`, `TrxExitClearance` | Ratchet menjadi `Hrd*` pada task yang sama | `TOUCHED LEGACY` |
| `Mst*` | `MstOnboardingTemplate`, `MstOnboardingTemplateTask` | **Tetap `Mst*`** bila memang master atau referensi | `NEW CODE` untuk kolom baru |

Dasarnya `HRD-DEC-019`: `Wfp` adalah prefix yang sah dan **bukan** legacy, sehingga menyentuhnya
tidak menjadikannya `TOUCHED LEGACY` dan tidak memicu rename apa pun. Hanya `Trx*` yang
kepemilikannya terbukti milik HR yang tunduk pada aturan ratchet.

Entity yang **tidak** materially touched tidak berubah kelasnya, apa pun prefixnya. Definisi
material touch ada di `00-interview-decisions.md` bagian 16.5.

Catatan batas: serah terima kredensial untuk tenaga klinis saat onboarding **tidak** dirancang di
sini.

### `S-C4-ONB` — Onboarding dan Kontrak Kerja (sub-modul 05), dipisah dari `S-C4` pada 4 November 2026

| Aspek | Isi |
| --- | --- |
| **Asal** | Bagian onboarding, kontrak, dan masa percobaan dari `S-C4`. Bagian exit clearance tetap `S-C4` (`BE-HRD-014` ✅, `FE-HRD-017` ✅) |
| **Desain** | `approved` — Pengguna, 4 November 2026 (`HRD-DEC-096`): `02-backend-architecture.md` bagian 18, `03-frontend-architecture.md` bagian 14, kontrak `v9`, `04-prd-to-mvp.md` bagian 23 |
| **Keputusan** | `HRD-DEC-081` s.d. `HRD-DEC-090`, `HRD-DEC-095`, `HRD-DEC-096` |
| **Perlakuan model kandidat** | Menggantikan tabel di atas untuk bagian onboarding: `TrxEmployeeOnboarding`, `TrxEmployeeOnboardingTask`, `WfpOnboardingChecklist`, `WfpOnboardingTask`, `TrxProbationReview`, `TrxContractNonRenewal` **dibuang** dengan pengaman tabel kosong dan diganti entity `Hrd*` baru (`HRD-DEC-090`); `MstOnboardingTemplate*` tetap `Mst*` dan `Diperbarui` |
| **Task** | Backend `BE-HRD-063` s.d. `BE-HRD-086`; frontend `FE-HRD-058` s.d. `FE-HRD-074` (roadmap revision `7`) |
| **Gelombang rilis** | `MVP-14` s.d. `MVP-17` dan Verifikasi (`04-prd-to-mvp.md` 23.20) |
| **Dependency** | `S-D2` (`BE-HRD-057`, `BE-HRD-061`, `FE-HRD-053`, `FE-HRD-056`); `HRD-DEP-003` untuk peran kerja; `INT-HR-31` untuk penahan jadwal di Penjadwalan |
| **Gerbang** | `HRD-DEC-032` (pemetaan peran) menahan `BE-HRD-085` dan `BE-HRD-086` |
| **Release Status** | `READY` |

### `S-C5` — Hubungan karyawan dan kedisiplinan

| Aspek | Isi |
| --- | --- |
| **Current State** | 10 endpoint pada 1 controller, model berjumlah 8. Master data lengkap di frontend: jenis pelanggaran, jenis sanksi, jenis tindakan disiplin, jenis kasus |
| **Target State** | Pencatatan kasus, tindakan disiplin, sanksi, dan riwayatnya |
| **Backend Impact** | `EXTEND` |
| **Frontend Impact** | Kelompok halaman baru untuk sisi transaksi |
| **Database Impact** | Kemungkinan `EXTEND` kolom |
| **Dependency** | `HRD-DEP-002` |
| **Blocking Decision** | Kepemilikan proses kekerasan di tempat kerja dan konseling adalah `HRD-Q-06` |
| **QBE Impact** | `NEW CODE` untuk endpoint tambahan. Empat controller master data domain ini tersimpan di folder yang salah, lihat `HRD-TF-004`; pemindahannya digabungkan ke `S0-B` |
| **Migration Risk** | Rendah |
| **Acceptance Criteria** | 1. Kasus kedisiplinan hanya dapat dibaca peran yang berwenang. 2. Setiap tindakan menyimpan alasan, pelaku, dan waktu. 3. Kasus yang sudah ditutup tidak dapat diubah diam-diam |
| **Release Status** | `READY` |

---

## 5. Slice yang terblokir

### `S-C1` — Kredensial, Lisensi Praktik STR/SIP, SPK/RKK & Penegakan Kewenangan Klinis SIMRS

| Aspek | Isi |
| --- | --- |
| **Current State** | 46 endpoint pada 5 controller, 18 model termasuk `WfpCredentialLicense` dan `WfpClinicalPrivilege`. Frontend memanggil nol. Tata kelola belum memisahkan 3 komite mandiri, verifikasi primer PSV SatuSehat SDMK belum ada, dan penegakan batas wewenang ke SIMRS masih draft (`HRD-DEC-005`). |
| **Target State** | Tata kelola kredensialing 3 komite mandiri (Medik, Keperawatan, Nakes Lain) berjalan; verifikasi keabsahan primer (PSV) STR seumur hidup & radar SIP 5 tahun H-90 s.d. fail-closed H-0; asesmen panel Mitra Bestari (Peer Review min 2 reviewer); penerbitan dan pengesahan tunggal SPK & RKK oleh Direktur Utama RS; penegakan gerbang wewenang klinis performa tinggi SIMRS (`INT-HR-69`) dengan mekanisme Hard-Stop tegas, co-signature supervisi 24 jam, dan bypass cito darurat penyelamatan nyawa ber-audit KMKP; fast-track temporary (maks 3 bln) & emergency (maks 72 jam); serta siklus rekredensialing 3 tahunan terintegrasi dengan OPPE Sub-15. |
| **Backend Impact** | Penambahan tabel `mst_credentialing_committees`, `trx_spk_rkk_issuances`, `trx_clinical_privilege_emergency_override_logs`; ekstensi tabel `wfp_credential_licenses` dan `wfp_clinical_privileges`; 27 endpoint REST API Swagger Kontrak `v18`; scheduler cron job `SipExpirationMonitorJob` dan `RecredentialingReminderJob`. |
| **Frontend Impact** | 8 Layar Baru (`FE-HRD-187` s.d. `FE-HRD-194`) mencakup portal self-service nakes, dashboard PSV staf kredensialing, meja kerja Mitra Bestari, sidang pleno komite, konsol Direktur Utama, registri RKK & audit SIMRS, konsol penugasan cepat, dan radar rekredensialing 3 tahun. |
| **Database Impact** | Migrasi PostgreSQL skema baru, penambahan kolom PSV dan level supervisi, indeks komposit performa tinggi gate query, zero-downtime online migration. |
| **Dependency** | `HRD-DEP-001` (Workforce Core Sub-02), `HRD-DEP-002` (Scheduling Roster Sub-06), `INT-HR-69` (SIMRS/EMR Service Gate), `INT-HR-70` (Sub-15 OPPE Query). |
| **Blocking Decision** | **NIHIL / TELAH DISELESAIKAN:** Keputusan `HRD-DEC-146` s.d. `HRD-DEC-151` telah disetujui, dan posisi sementara `HRD-DEC-005` resmi diselesaikan menjadi *approved* dengan protokol Hard-Stop bertingkat. |
| **QBE Impact** | `NEW CODE` untuk controllers, services, dan DTO baru; `TOUCHED LEGACY` untuk adaptasi `WfpCredentialLicense` dan `WfpClinicalPrivilege`. |
| **Migration Risk** | Rendah — penambahan kolom nullable / default value tanpa memutus data existing. |
| **Acceptance Criteria** | Lulus seluruh 22 skenario pengujian penerimaan `AT-HRD-CRD-01` s.d. `AT-HRD-CRD-22` dan 10 kriteria selesai (Definition of Done). |
| **Release Status** | `PLANNED` (16 task backend `BE-HRD-206` s.d. `BE-HRD-221`, 15 task frontend `FE-HRD-186` s.d. `FE-HRD-200`, roadmap rev 16). |

### `S-C6` — Kesehatan dan keselamatan kerja staf

| Aspek | Isi |
| --- | --- |
| **Current State** | 9 endpoint pada 1 controller, 10 model termasuk `WfpHealthRecord`. Frontend memanggil nol |
| **Target State** | **Tidak ditetapkan** |
| **Backend Impact** | Tidak ditetapkan |
| **Frontend Impact** | Tidak ditetapkan |
| **Database Impact** | Tidak ditetapkan |
| **Dependency** | `HRD-DEP-007` |
| **Blocking Decision** | `HRD-DEP-007`, ditambah `HRD-DEC-010` yang masih `draft` menunggu K3RS |
| **QBE Impact** | Tidak dinilai |
| **Migration Risk** | Tidak dinilai |
| **Acceptance Criteria** | Tidak ditetapkan |
| **Release Status** | `BLOCKED` |

Yang boleh dicatat: `HRD-DEC-010` sebagai posisi sementara, yaitu rekam kesehatan kerja hanya
dapat dibaca K3RS dan pegawai yang bersangkutan, sementara pihak lain hanya melihat status
kelayakan kerja tanpa isi medis.

### `S-D1` s.d. `S-D5` — Lima domain tanpa API

| Slice | Domain | Model | Controller |
| --- | --- | ---: | ---: |
| `S-D1` | `WorkforcePlanning` | 11 | 0 |
| `S-D2` | `RecruitmentManagement` | 20 | 0 |
| `S-D3` | `BenefitManagement` | 9 | 0 |
| `S-D4` | `HrServiceManagement` | 8 | 0 |
| `S-D5` | `BusinessTravelManagement` dan `ExpenseManagement` | 20 | 0 |

| Aspek | Isi |
| --- | --- |
| **Current State** | **68 model berada di dalam enam domain yang tidak punya satu pun controller.** Satu di antaranya, `MstWorkforceRequirement`, sudah punya API lewat domain lain, sehingga **67 entity** benar-benar belum punya API. Seluruh 68 model punya konfigurasi EF dan terdaftar sebagai `DbSet`, dan tabelnya sudah dibuat migration `20260726161839_initializeBigModulHRD2` |
| **Target State** | **Model data** diturunkan ulang dari proses bisnis rumah sakit sesuai `HRD-DEC-004`. Model existing berstatus kandidat, bukan jawaban. Istilah "ERD" pada decision itu bermakna penurunan model data; hasilnya ditulis sebagai `classDiagram` pada `02-backend-architecture.md` ditambah `data/data-dictionary.md`, **bukan** sebagai folder `erd/` |
| **Backend Impact** | Tidak ditetapkan sebelum `HRD-Q-05` dijawab |
| **Frontend Impact** | Tidak ditetapkan |
| **Database Impact** | **Berpotensi merusak.** Penurunan ulang akan menghasilkan migration yang mengubah atau membuang tabel yang sudah ada, bukan membuat di ruang kosong |
| **Dependency** | `HRD-Q-05` |
| **Blocking Decision** | `HRD-Q-05` — belum diketahui apakah tabel-tabel itu sudah berisi data dari impor manual atau migrasi V1. Audit source membuktikan aplikasi tidak dapat menulis ke sana, tetapi tidak dapat membuktikan tidak ada yang mengisinya lewat jalur lain |
| **QBE Impact** | `QBE-DB-001` audit dependency fisik sebelum rename atau perubahan. `QBE-DB-002` dilarang `DROP` lalu `CREATE` bila perubahan yang mempertahankan data masih aman. `QBE-MOD-002` entity operasional baru wajib memakai prefix `Hrd` sesuai `HRD-DEC-019` |
| **Migration Risk** | **Tinggi.** Ini satu-satunya kelompok slice yang berpotensi menghasilkan migration destruktif |
| **Acceptance Criteria** | Tidak ditetapkan. Kriteria yang berlaku sekarang: **tidak ada migration destruktif yang boleh direncanakan sebelum isi tabel diketahui** |
| **Release Status** | **`S-D1` dan `S-D2` `READY` sejak 3 November 2026; desain disetujui dan task direncanakan pada hari yang sama.** Untuk `S-D1`: `BE-HRD-017`, `BE-HRD-029` s.d. `BE-HRD-051`, `FE-HRD-029` s.d. `FE-HRD-043` (`HRD-DEC-058` s.d. `HRD-DEC-071`). Untuk `S-D2`: `BE-HRD-018`, `BE-HRD-052` s.d. `BE-HRD-062`, `FE-HRD-044` s.d. `FE-HRD-057` (`HRD-DEC-073` s.d. `HRD-DEC-080`). `HRD-Q-05` dan `HRD-BLK-004` ditutup untuk 20 tabel `RecruitmentManagement` lewat `HRD-DEC-074` dijaga `EMPTY_TABLE_GUARD` (audit membuktikan 20 tabel kosong 0 baris). `S-D3` s.d. `S-D4` tetap `BLOCKED`. `S-D5` `DEFERRED`. |


### `S-D3` — Benefit Management (Target State Terinci)

| Aspek | Isi |
| --- | --- |
| **Current State** | 9 model legacy `BenefitManagement` tanpa controller aktif. Telah ditinjau pada wawancara Pass 12 (`HRD-DEC-134` s.d. `HRD-DEC-139`). |
| **Target State** | 7 model EF Core baru: `MstBenefitPlan`, `HrdBenefitGradeEligibility`, `TrxEmployeeInsuranceEnrollment`, `TrxEmployeeBenefitDependent`, `HrdBenefitBalanceMovement`, `TrxBenefitClaim`, `TrxBenefitClaimDocument`. 4 grup API controller Swagger-tagged (`Plans & Eligibility`, `Enrollments & Dependents`, `In-House Healthcare & Balance`, `COB Claims & Settlement`). 6 layar antarmuka Next.js App Router (`FE-HRD-174` s.d. `FE-HRD-179`). |
| **Backend Impact** | 15 task backend (`BE-HRD-176` s.d. `BE-HRD-190`) mencakup migrasi non-breaking, master paket bertingkat 3-lapisan, pendaftaran cepat BPJS TK H+0 Onboarding, aktivasi bertahap pasca-probation, verifikasi kuota keluarga (1 pasangan + 3 anak), buku besar saldo plafon non-negatif terintegrasi billing SIMRS (`INT-HR-61`), klaim COB berlampiran mandatory SEP BPJS, reset hangus tahunan 1 Januari, dan penonaktifan via Exit Clearance Sub-20. |
| **Frontend Impact** | 14 task frontend (`FE-HRD-158` s.d. `FE-HRD-171`) mencakup dasbor analitik manfaat, katalog paket & matriks grade staf, pelacak aktivasi asuransi pegawai, meja kerja verifikasi dokumen kependudukan HR, portal mandiri ESS keluarga saya, inquiry hak berobat nakes internal & mutasi buku besar, formulir pengajuan klaim COB mandatory SEP, dan verifikasi build/linter CI/CD. |
| **Database Impact** | Migrasi non-breaking Entity Framework Core menambahkan 7 tabel baru di schema `public`. |
| **Migration Risk** | Rendah. Tabel baru tidak mengganggu data eksisting, penegakan integritas menggunakan check constraint dan foreign key restrict. |
| **Acceptance Criteria** | 20 skenario acceptance tests `AT-HRD-BEN-01` s.d. `AT-HRD-BEN-20` dan 10 butir DoD pada `04-prd-to-mvp.md` Bagian 30.5 terpenuhi lengkap. |
| **Release Status** | `PLANNED` (Roadmap Revisi 14, 5 November 2026). Siap untuk eksekusi task per task. |


### `S-D4` — HR Service Management & Request / Employee Document Service (Target State Terinci)

| Aspek | Isi |
| --- | --- |
| **Current State** | 8 model legacy `HrServiceManagement` tanpa controller aktif. Telah ditinjau pada wawancara Pass 13 (`HRD-DEC-140` s.d. `HRD-DEC-145`). |
| **Target State** | 8 model EF Core baru + 1 tabel histori audit: `MstHrServiceCategory`, `MstHrServiceType`, `MstEmployeeDocumentType`, `TrxHrServiceRequest`, `TrxHrServiceRequestAttachment`, `TrxHrServiceRequestComment`, `TrxEmployeeDocumentRequest`, `TrxEmployeeDocumentIssuance`, `HrdProfileChangeHistory`. 4 grup API controller Swagger-tagged (`Catalog & SLA`, `Tickets & Queue`, `ESS Requests & My Documents`, `Document Issuance & Public Verification`). 7 layar antarmuka Next.js App Router (`FE-HRD-180` s.d. `FE-HRD-186`). |
| **Backend Impact** | 15 task backend (`BE-HRD-191` s.d. `BE-HRD-205`) mencakup migrasi non-breaking, katalog layanan & penegakan batas waktu SLA 3-tier (Cito 4 jam, Standar 24 jam, Khusus 72 jam) berbasis jam operasional RS, mesin penomoran surat resmi terpusat gapless berbasis locking database atomik, penarikan otomatis nominal gaji dari slip gaji final Sub-10 (`INT-HR-56`), gerbang approval C&B dan anti-self-approval, stempel QR Code publik SHA-256 dengan proteksi nominal gaji via 4-digit PIN BCrypt, pemutakhiran profil otomatis ke Sub-02 (`INT-HR-65`), pengarsipan abadi e-Dossier Sub-02 (`INT-HR-66`), dan protokol pencabutan keabsahan dokumen (revokasi) ber-BAP. |
| **Frontend Impact** | 14 task frontend (`FE-HRD-172` s.d. `FE-HRD-185`) mencakup Redux slice helpdesk, dasbor operasional KPI & antrean tiket berkas ber-countdown SLA, modal re-assign cepat, editor template surat token dinamis, formulir mandiri ESS multi-langkah & drag-and-drop attachment, drawer pemrosesan tiket dengan penampil lampiran side-by-side & percakapan klarifikasi, modal otorisasi surat resmi & generator QR, portal publik verifikasi keabsahan dokumen (`/verify/document/[hash]`) berdialog PIN 4-digit, repositori arsip dokumen saya ESS, serta audit verifikasi build/linter CI/CD. |
| **Database Impact** | Migrasi non-breaking Entity Framework Core menambahkan 8 tabel operasional baru dan 1 tabel histori audit di schema `public`. |
| **Migration Risk** | Rendah. Tabel baru tidak mengganggu data eksisting, penegakan integritas data menggunakan foreign key restrict dan check constraint status. |
| **Acceptance Criteria** | 20 skenario acceptance tests `AT-HRD-HRS-01` s.d. `AT-HRD-HRS-20` dan 10 butir DoD pada `04-prd-to-mvp.md` Bagian 31.5 terpenuhi lengkap. |
| **Release Status** | `PLANNED` (Roadmap Revisi 15, 5 November 2026). Siap untuk eksekusi task per task. |

---

### `S-D2` — Rekrutmen dan Hiring (Target State Terinci)

| Aspek | Isi |
| --- | --- |
| **Current State** | 20 model di `RecruitmentManagement` dengan 0 controller. Audit membuktikan seluruh 20 tabel kosong (0 baris di dev). |
| **Target State** | 12 model baru ber-prefix `Hrd` pada namespace `RecruitmentManagement`: `HrdJobRequisition`, `HrdJobVacancy`, `HrdCandidate`, `HrdCandidateApplication`, `HrdCandidateDocument`, `HrdCandidateScreening`, `HrdCandidateEvaluation`, `HrdJobOffer`, `HrdJobOfferApproval`, `HrdPreEmploymentMedicalCheck`, `HrdTalentPool`, `HrdCandidateHiringHandoff`. 9 grup controller API RESTful Swagger-tagged. 13 layar frontend Next.js App Router (`FE-HRD-110` s.d. `FE-HRD-122`). |
| **Backend Impact** | 12 task backend (`BE-HRD-018` fondasi & migration, `BE-HRD-052` s.d. `BE-HRD-062`). Migration membersihkan 20 tabel legacy dilindungi pengaman `EMPTY_TABLE_GUARD`. |
| **Frontend Impact** | 14 task frontend (`FE-HRD-044` s.d. `FE-HRD-057`) dengan komponen base form Quilvian, penegakan hak veto klinis unit, proteksi privasi diagnosa medis K3RS, dan alur promosi cepat talent pool. |
| **Database Impact** | Migration `RetireLegacyRecruitmentAndWfpCandidateTables` dan `AddRecruitmentManagementCoreEntities`. Aman dari data loss karena tabel legacy berstatus 0 baris. |
| **Dependency** | `BE-HRD-017` (Fondasi WFP), `BE-HRD-041` (Kursi Headcount WFP), `BE-HRD-003` (Workforce Core), `BE-HRD-002` (Mst Salary Grade). |
| **Blocking Decision** | **TIDAK ADA.** Seluruh keputusan bisnis (`HRD-DEC-073` s.d. `HRD-DEC-080`) telah disahkan pemilik teknis dan produk. |
| **QBE Impact** | `NEW CODE` pada seluruh entitas dan controller baru; `LEGACY MIGRATION` dengan `EMPTY_TABLE_GUARD` untuk tabel lama. |
| **Migration Risk** | **Rendah terkendali.** Tabel kosong dibuktikan audit dev; migration fail-closed jika data > 0. |
| **Acceptance Criteria** | Terpenuhinya kriteria `AT-HRD-D2-01` s.d. `AT-HRD-D2-12` dan skenario `UAT-REC-01` s.d. `UAT-REC-07`. |
| **Release Status** | **`READY`** sejak 3 November 2026. |

Satu pengecualian yang wajib diingat: `MstWorkforceRequirement` milik `WorkforcePlanning`
**sudah** dilayani `WorkforceRequirementController` dan sudah dipakai frontend lewat
`/hr/master-data/workforce-requirement`. Entity itu **tidak** ikut diturunkan ulang. Jumlah yang
benar untuk perancangan ulang bebas adalah **67 entity, bukan 68**.

---

## 6. `S-E` — Ratchet legacy `Trx` saat disentuh

**`S-E` bukan gelombang pengiriman.** Ia bukan pekerjaan yang dijadwalkan setelah gelombang A
sampai D selesai, dan ia tidak punya tanggal mulai maupun tanggal selesai.

`S-E` adalah **aturan lintas-slice** yang berlaku sepanjang implementasi. Ia menempel pada task
apa pun di slice mana pun, kapan pun task itu kebetulan menyentuh entity `Trx*` milik HR.

### 6.1 Isi aturan

| Prefix | Yang berlaku |
| --- | --- |
| `Wfp*` | **Tetap `Wfp*`.** Tidak diubah, tidak dimigrasikan, tidak dilabeli legacy |
| `Mst*` | **Tetap `Mst*`** bila entity-nya memang master atau referensi |
| Entity operasional HR baru | **Wajib `Hrd*`** |
| `Trx*` milik HR yang tidak disentuh | **Dibiarkan berjalan.** Tidak ada yang perlu dikerjakan |
| `Trx*` milik HR yang materially touched | **Menjadi `Hrd*` pada task yang sama** |

### 6.2 Yang secara tegas tidak berlaku

1. Tidak ada rename massal.
2. Tidak ada kampanye migration yang tujuannya mengejar seluruh `Trx*` sekaligus.
3. Tidak ada tenggat untuk membersihkan seluruh `Trx*`.
4. Tidak ada target bahwa seluruh entity operasional HR pada akhirnya harus menjadi `Hrd*`.
5. Proses bisnis yang sedang berjalan **tidak boleh diputus** hanya demi konsistensi penamaan.

### 6.3 Kapan ratchet berlaku

Ratchet berlaku ketika sebuah task **materially touched** entity itu, yaitu mengubah salah satu:

| Yang berubah | Contoh |
| --- | --- |
| Entity atau class persistence | Menambah properti pada `TrxJobRequisition` |
| Konfigurasi Entity Framework | Mengubah konfigurasi relasi entity itu |
| Tabel atau kolom fisik | Menambah kolom baru |
| Relasi atau foreign key | Menambah relasi ke entity lain |
| Index atau constraint | Menambah unique constraint |
| Lifecycle persistence | Mengubah perilaku soft-delete atau audit |
| Migration yang memang mengenai entity itu | Migration yang menyentuh tabelnya |

Ratchet **tidak** berlaku untuk:

| Yang dikerjakan | Alasan |
| --- | --- |
| Pekerjaan frontend saja | Tidak menyentuh persistence |
| Membaca entity dari controller atau service | Membaca bukan mengubah |
| Dokumentasi | Tidak menyentuh source |
| Perubahan tampilan | Tidak menyentuh persistence |
| Perbaikan bug controller atau service yang tidak mengubah kontrak persistence | Bentuk datanya tidak berubah |
| Refactor yang tidak mengubah entity maupun schema | Bentuk datanya tidak berubah |

Contoh supaya batasnya jelas:

> Seluruh slice `S-A2` layanan mandiri cuti adalah pekerjaan frontend yang memanggil endpoint
> yang sudah ada. Slice itu menyentuh `WfpLeaveRequest` hanya sebagai pembaca. **Tidak ada satu
> pun rename yang terjadi di sepanjang `S-A2`.**
>
> Sebaliknya, bila `S-A7` kotak masuk terpadu ternyata memerlukan penambahan index pada
> `TrxWorkflowInstance`, entity itu menjadi materially touched. Ratchet berlaku, dan entity itu
> dinormalkan menjadi `HrdWorkflowInstance` **di dalam task yang sama**, bukan dijadwalkan
> terpisah.

### 6.4 Aturan pelaksanaan bila ratchet memang berlaku

| Aturan | Isi |
| --- | --- |
| `QBE-NAM-003` | Nama class di source dan nama tabel fisik dinormalkan **bersamaan**, tidak boleh salah satu saja |
| `QBE-DB-001` | Audit foreign key, index, constraint, dependency, dan riwayat migration lebih dulu sebelum rename |
| `QBE-DB-002` | Pakai rename yang mempertahankan data. Dilarang `DROP` lalu `CREATE` selama rename masih aman |
| Cakupan | Ikuti cakupan task atau domain yang sedang dikerjakan. Jangan melebar ke entity tetangga yang tidak disentuh |
| Kepemilikan | Hanya entity yang kepemilikannya **terbukti** milik HR. Empat puluh entity `Trx*` adalah milik Health Services dan tidak ikut |

### 6.5 Rangkuman slice

| Aspek | Isi |
| --- | --- |
| **Current State** | Empat gaya penamaan hidup berdampingan: `Hrd` 15 entity, `Trx` 178, `Mst` 104, `Wfp` 40. `AttendanceManagement` sudah dinormalkan lewat tiga migration pada 19, 21, dan 22 Agustus 2026 |
| **Target State** | Tidak ada target keadaan akhir yang menyeluruh. Yang ada aturan: entity baru memakai `Hrd`, dan `Trx*` milik HR berubah menjadi `Hrd*` hanya saat disentuh |
| **Backend Impact** | Menempel pada task lain. Tidak ada task tersendiri untuk `S-E` |
| **Frontend Impact** | Tidak ada, selama nama route dan bentuk response tidak berubah. Ini yang membedakannya dari `S0-B` |
| **Database Impact** | Penggantian nama tabel yang mempertahankan data, hanya pada entity yang disentuh |
| **Dependency** | `HRD-PRE-001` — registry diperbarui lebih dulu lewat `S0-A` |
| **Blocking Decision** | `HRD-DEC-019` mengikat bentuknya |
| **QBE Impact** | `QBE-NAM-003`, `QBE-DB-001`, `QBE-DB-002`. Kelas keberlakuan: `TOUCHED LEGACY`, bukan `LEGACY MIGRATION`, karena tidak ada kampanye terbatas yang dijadwalkan |
| **Migration Risk** | Rendah per kejadian, karena cakupannya selalu sebesar task yang sedang berjalan. Tidak pernah ada migration besar |
| **Acceptance Criteria** | 1. Tidak ada satu pun migration yang menyentuh seluruh HR. 2. Tidak ada task yang tujuannya semata-mata rename. 3. `Wfp*` dan `Mst*` tidak berubah. 4. Entity HR baru tidak memakai `Trx`. 5. Rename mempertahankan data, dan nama source serta tabel fisik dinormalkan bersamaan. 6. Proses bisnis yang berjalan tidak terputus |
| **Release Status** | `READY` sebagai aturan yang berlaku terus-menerus, bukan sebagai gelombang yang dijadwalkan |

## 7. Ringkasan status

| Release Status | Jumlah slice | Slice |
| --- | ---: | --- |
| `READY` | **15** | `S0-A`, `S0-B` (2), `S-A1` s.d. `S-A7` (7), `S-C3` s.d. `S-C5` (3), `S-E` (1), `S-D1` (1), `S-D2` (1) |
| `PLANNED` | **8** | `S-B4` (1, rev 8), `S-B1` (1, rev 9), `S-B2` (1, rev 10), `S-B3` (1, rev 11), `S-B5` (1, rev 12), `S-B6` (1, rev 13), `S-D3` (1, rev 14), `S-D4` (1, sejak 5 November 2026 — 15 task BE, 14 task FE rev 15) |
| `PARTIAL` | **0** | Tidak ada slice PARTIAL |
| `PLANNED` | 10 | `S-B1`, `S-B2`, `S-B3`, `S-B5`, `S-B6`, `S-D1`, `S-D3`, `S-D4`, `S-C1`, `S-C1-OPPE` |
| `BLOCKED` | 1 | `S-C6` |
| `DEFERRED` | 1 | `S-D5` |
| **Total** | **27** | |

Tidak ada slice `BLOCKED` yang boleh dinaikkan menjadi `READY` berdasarkan asumsi. Kenaikan
status hanya sah bila dependency yang disebut namanya benar-benar terpenuhi, dan buktinya
dicatat pada [`01-prerequisite-readiness.md`](../01-prerequisite-readiness.md).

---

## 8. Yang sengaja tidak ada di dokumen ini

- **Task backend dan frontend.** Pemecahan slice menjadi task berukuran kecil adalah pekerjaan
  `/plan-module-delivery`.
- **Tanggal.** Urutan dinyatakan sebagai gelombang, bukan jadwal.
- **Estimasi.** Tidak ada angka hari maupun poin.
- **Desain.** Slice menyatakan dampak, bukan bentuk tabel, endpoint, atau layar.
