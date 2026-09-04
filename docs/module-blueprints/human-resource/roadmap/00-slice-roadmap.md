# Human Resource — Slice Roadmap

| Field | Value |
| --- | --- |
| Blueprint ID | `HRD-BP-001` |
| Roadmap revision | `3` — revisi `1` PHASE 1; revisi `2` menyerap `HRD-DEC-019` dan memperbaiki hitungan slice serta definisi angka 68/67; **revisi `3` (`PHASE 2B.1`, 28 Agustus 2026) memperbarui classification `S-B4` (roster/shift-harian/darurat/siaga: `MISSING API` → target `EXTEND`, `HRD-DEC-026`), menambahkan catatan HR-on-behalf pada `S-B1` (`HRD-DEC-028`), Early Leave Permission pada `S-B2` (`HRD-DEC-029`), dan mesin SLA/eskalasi pada `S-A7` (`HRD-DEC-030`)** |
| Status | `DRAFT` |
| Backend SHA | `ecdc135` |
| Frontend SHA | `2a1cea784` |
| Masukan | `00-interview-decisions.md` rev `3`, `01-existing-capability-map.md` rev `1.1` |

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
| `HRD-PH-003` | `S-B1` s.d. `S-B4` | `READY` |
| `HRD-PH-004` | `S-B5` | `READY` sebagian |
| `HRD-PH-005` | `S-C1` | `BLOCKED` |
| `HRD-PH-006` | `S-C6` | `BLOCKED` |
| `HRD-PH-007` | `S-D1` s.d. `S-D5` | `BLOCKED` |
| `HRD-PH-008` | `S-C2` s.d. `S-C5` | `READY` |
| `HRD-PH-009` | `S-E` | **Bukan fase yang dijadwalkan.** Aturan lintas-slice yang berlaku sepanjang implementasi |

### 0.5 Daftar seluruh slice

Dua puluh enam slice. `S-D2` sampai `S-D5` dijelaskan dalam satu blok bersama karena
perlakuannya identik, tetapi keempatnya tetap dihitung sebagai slice tersendiri.

| Gelombang | Slice | Jumlah |
| --- | --- | ---: |
| `S0` fondasi | `S0-A`, `S0-B` | 2 |
| `A` memakai backend matang | `S-A1` s.d. `S-A7` | 7 |
| `B` administrasi waktu kerja | `S-B1` s.d. `S-B5` | 5 |
| `C` pengembangan orang dan slice terblokir | `S-C1` s.d. `S-C6` | 6 |
| `D` domain tanpa API | `S-D1` s.d. `S-D5` | 5 |
| `E` aturan ratchet | `S-E` | 1 |
| **Total** | | **26** |

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

### `S-A1` — Enam halaman daftar lintas-pegawai Administrasi Kepegawaian

| Aspek | Isi |
| --- | --- |
| **Current State** | Enam menu menunjuk `/hr/workforce-core/*` yang tidak punya halaman. Namun kemampuannya **sudah ada dan sudah dipakai**: data yang sama dapat dilihat dan diubah dari halaman detail pegawai lewat editor profil, yang memanggil 14 controller `WorkforceCore` |
| **Target State** | Enam halaman daftar yang menampilkan data seluruh pegawai pada satu periode. Contoh: seluruh penetapan gaji yang berlaku bulan ini, bukan gaji satu orang |
| **Backend Impact** | `EXTEND`. Controller yang ada bersifat per profil, dengan pola `workforce-profiles/{workforceProfileId}/<sumber-daya>`. Daftar lintas-pegawai membutuhkan endpoint baru yang tidak terikat satu profil. `EmployeeProfileChangeController` sudah punya bentuk lintas-pegawai dan dapat menjadi contoh |
| **Frontend Impact** | Enam kelompok halaman baru di `src/app/hr/workforce-core/**`. Redux dan komponen editor yang sudah ada dipakai ulang, tidak dibuat baru |
| **Database Impact** | Tidak ada tabel baru. Kemungkinan perlu index untuk mendukung filter periode dan unit |
| **Dependency** | Tidak ada dependency modul lain |
| **Blocking Decision** | `HRD-DEC-012` sudah `approved` |
| **QBE Impact** | `NEW CODE` untuk endpoint daftar lintas-pegawai; wajib memakai `ApiResponse<T>` dan `PagedResult<T>`. Entity baru **tidak** dibuat, sehingga `QBE-MOD-002` tidak terpicu |
| **Migration Risk** | Rendah. Penambahan index dapat dijalankan tanpa mematikan layanan |
| **Acceptance Criteria** | 1. Setiap `pathname` di bawah `corporateHumanResource` punya `page.jsx` yang cocok. 2. HR Admin dapat melihat seluruh penetapan gaji yang berlaku pada satu periode tanpa membuka pegawai satu per satu. 3. Halaman detail pegawai tetap berfungsi seperti sebelumnya. 4. Tidak ada komponen editor baru yang menduplikasi yang sudah ada |
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
| **Current State** | 71 endpoint pada 9 controller, mencakup rekaman mentah, pemrosesan, harian, pengecualian, periode dengan tutup dan buka kembali, koreksi, pemantauan koreksi dengan perbaikan massal, dan serah terima payroll. Frontend memanggil nol. **Koreksi kehadiran atas nama pegawai (HR-on-behalf): `MISSING` sepenuhnya** — `AttendanceCorrectionService.CreateAsync` mensyaratkan `daily.WorkforceProfileId == actorWorkforceProfileId`, tidak ada jalur lain, dibuktikan `flows/07-attendance-correction.md` |
| **Target State** | HR dan petugas payroll dapat memantau kehadiran harian, menangani pengecualian, memproses ulang, menutup periode, dan membuka kembali bila perlu. **`HRD-DEC-028`, 28 Agustus 2026: HR Admin boleh membuat koreksi atas nama pegawai bila ESS tidak dapat diakses**, wajib menyimpan initiator, workforce diwakili, alasan, timestamp, bukti bila perlu, notifikasi pegawai, dan audit trail; persetujuan tetap lewat workflow koreksi yang berlaku |
| **Backend Impact** | Diharapkan nihil untuk kapabilitas yang sudah matang. **`EXTEND`** untuk jalur koreksi on-behalf, `HRD-DEC-028` |
| **Frontend Impact** | Beberapa kelompok halaman baru di `src/app/hr/attendance/**` |
| **Database Impact** | Tidak ada |
| **Dependency** | `HRD-DEP-006` untuk lampiran bukti koreksi |
| **Blocking Decision** | `HRD-DEC-028` sudah `approved` untuk koreksi on-behalf |
| **QBE Impact** | Frontend saja untuk kapabilitas existing. `NEW CODE` untuk jalur on-behalf |
| **Migration Risk** | Nihil |
| **Acceptance Criteria** | 1. Periode dapat ditutup, dan penutupan menolak bila masih ada pengecualian yang belum selesai sesuai aturan backend. 2. Membuka kembali periode hanya dapat dilakukan peran tertentu dan tercatat. 3. Rekaman mentah tidak pernah berubah oleh hasil olahan. 4. Pemrosesan ulang satu hari tidak mengubah hari lain. 5. Koreksi on-behalf menyimpan initiator, workforce diwakili, alasan, timestamp, dan notifikasi ke pegawai |
| **Release Status** | `READY` untuk kapabilitas existing. Koreksi on-behalf `READY` untuk dirancang sebagai `EXTEND` |

### `S-B2` — Administrasi cuti dan saldo

| Aspek | Isi |
| --- | --- |
| **Current State** | 93 endpoint pada 12 controller. Frontend memanggil nol untuk sisi administrasi |
| **Target State** | HR dapat mengelola hak cuti, menyesuaikan saldo dengan alasan, melihat kalender unit, dan menangani pengecualian |
| **Backend Impact** | Diharapkan nihil |
| **Frontend Impact** | Kelompok halaman baru di `src/app/hr/leave/**` |
| **Database Impact** | Tidak ada |
| **Dependency** | `HRD-DEP-002` |
| **Blocking Decision** | Nilai hak cuti per jenis pegawai adalah `HRD-Q-06`; itu isi master data, bukan penghalang |
| **QBE Impact** | Frontend saja |
| **Migration Risk** | Nihil |
| **Acceptance Criteria** | 1. Penyesuaian saldo selalu menyimpan alasan dan pelaku. 2. Saldo tidak pernah diubah tanpa jejak. 3. Perubahan hak cuti berlaku sesuai tanggal berlaku, bukan tanggal pencatatan |
| **Release Status** | `READY` |

**Catatan batas — Early Leave Permission bukan bagian slice ini.** `HRD-DEC-029`, 28 Agustus
2026, menetapkan Early Leave Permission (izin administratif pulang cepat) sebagai konsep
**terpisah** dari Hourly Leave (mode `IsHourly` pada `WfpLeaveRequest`, sudah tercakup `S-B2`).
Early Leave Permission adalah bagian attendance/permission flow (`S-B1`), bukan Leave Management.
Klasifikasi: **`NEW`/`EXTEND` sesuai hasil arsitektur nanti** — belum ada entity yang dibuat.
Lihat `flows/08-early-leave-permission.md`.

### `S-B3` — Administrasi lembur

| Aspek | Isi |
| --- | --- |
| **Current State** | 78 endpoint pada 9 controller, mencakup rencana, realisasi, verifikasi, rekonsiliasi, penjadwal penutupan, dan serah terima. Frontend memanggil nol |
| **Target State** | HR dan atasan dapat merencanakan, memverifikasi, dan merekonsiliasi lembur sebelum diserahkan ke payroll |
| **Backend Impact** | Diharapkan nihil |
| **Frontend Impact** | Kelompok halaman baru di `src/app/hr/overtime/**` |
| **Database Impact** | Tidak ada |
| **Dependency** | `HRD-DEP-002` |
| **Blocking Decision** | Tarif dan aturan kompensasi adalah `HRD-Q-06` |
| **QBE Impact** | Frontend saja |
| **Migration Risk** | Nihil |
| **Acceptance Criteria** | 1. Lembur yang belum diverifikasi tidak ikut serah terima payroll. 2. Rekonsiliasi menunjukkan selisih antara rencana dan realisasi. 3. Nominal berasal dari backend |
| **Release Status** | `READY` |

### `S-B4` — Penjadwalan kerja

| Aspek | Isi |
| --- | --- |
| **Current State** | **`MISSING API` untuk operational roster core**, bukan sekadar "backend tipis". 22 endpoint pada 3 controller melayani hanya 3 dari 11 model (`WfpWorkScheduleAssignment`, `WfpScheduleChangeRequest`, `WfpShiftSwapRequest`). **Delapan model tanpa satu pun controller**: `TrxRosterPeriod`, `TrxRosterAssignment`, `TrxRosterPublication`, `TrxRosterApproval` (seluruh mesin roster), `TrxShiftAssignment` (penugasan shift harian), `TrxShiftReplacement` (penggantian shift), `TrxEmergencyStaffingRequest` (tenaga darurat), `TrxOnCallAssignment` (penugasan siaga aktual) — dibuktikan `PHASE 2B`, `flows/05-work-scheduling.md`. `DefaultWorkScheduleSeeder` mengisi satu jadwal bawaan berkode `SCH-RSMMC-DEFAULT` saat aplikasi start |
| **Target State** | Penyusunan jadwal per unit dan per periode, penugasan shift, deteksi bentrok, dan riwayat perubahan. **`HRD-DEC-026`, 28 Agustus 2026: untuk rumah sakit 24/7, roster period, roster assignment/publication, daily shift assignment, shift replacement, emergency staffing, dan actual on-call assignment adalah bagian target HR V2 — bukan `DEFERRED`.** Kedelapan entity di atas sudah model+EF+migration; perancangan ulang bebas **tidak berlaku** di sini seperti `S-D1`–`S-D5` |
| **Backend Impact** | **`EXTEND` terhadap schema existing**, `HRD-DEC-026`. Delapan model tanpa controller mendapat API baru di atas struktur yang sudah ada. **Larangan:** jangan membuat schema baru sebelum audit model existing, dan `HRD-Q-05` wajib terjawab lebih dulu bila perubahan destruktif ternyata diperlukan. Cakupan endpoint per entity ditetapkan saat desain |
| **Frontend Impact** | Kelompok halaman baru di `src/app/hr/scheduling/**` |
| **Database Impact** | Kemungkinan penambahan index. Tidak ada tabel baru |
| **Dependency** | `HRD-DEP-002` |
| **Blocking Decision** | `HRD-DEC-006` sudah memisahkan jadwal kerja dari jadwal praktik. `HRD-Q-09` sudah dijawab `HRD-DEC-013`: jam praktik di luar jadwal kerja menjadi pengecualian yang menunggu keputusan atasan. `HRD-DEC-027`, 28 Agustus 2026: penempatan jadwal current/future oleh HR pada periode editable **tidak** butuh approval tambahan (audit trail wajib); perubahan retroactive atau yang menyentuh periode locked **wajib** controlled correction/approval — jangan membuat approval untuk setiap edit kecil |
| **QBE Impact** | `NEW CODE` untuk endpoint tambahan pada delapan model yang di-`EXTEND`. Entity baru tidak diharapkan di luar itu; bila ternyata dibutuhkan, `QBE-MOD-002` berlaku dan entity wajib memakai prefix `Hrd` |
| **Migration Risk** | Rendah untuk penempatan individual. **Perlu audit dependency lebih dulu** untuk delapan model roster/shift-harian/darurat/siaga sebelum `EXTEND` dijalankan, sesuai `HRD-DEC-026` |
| **Acceptance Criteria** | 1. Bentrok jadwal terdeteksi sebelum disimpan. 2. Perubahan jadwal menyimpan alasan dan riwayat. 3. Jadwal pada periode kehadiran yang sudah tertutup tidak dapat diubah. 4. Jam praktik dokter di luar jadwal kerjanya muncul sebagai pengecualian, bukan lembur otomatis. 5. Penempatan current/future tidak memerlukan approval; perubahan retroactive/periode locked wajib lewat controlled correction |
| **Release Status** | `READY` untuk penempatan individual (`WfpWorkScheduleAssignment`, sudah `READY TO REUSE`). Roster/shift-harian/penggantian/tenaga-darurat/siaga tetap `READY` untuk dirancang sebagai `EXTEND`, tidak `DEFERRED`, per `HRD-DEC-026` |

### `S-B5` — Payroll sisi HR

| Aspek | Isi |
| --- | --- |
| **Current State** | 49 endpoint pada 6 controller. Jalur masuk dari kehadiran tersedia lewat `AttendancePayrollHandoffController` dengan `execute`, `repair`, dan `rollback` per `payrollRunId`. Frontend memanggil nol |
| **Target State** | Petugas payroll dapat menyiapkan periode, merekonsiliasi masukan dari kehadiran, cuti, dan lembur, menjalankan perhitungan, dan menyerahkan hasilnya |
| **Backend Impact** | Diharapkan nihil untuk perhitungan. **Bentuk serah terima ke Finance tidak dirancang pada slice ini** |
| **Frontend Impact** | Kelompok halaman baru di `src/app/hr/payroll/**`, terbatas sampai serah terima dijalankan |
| **Database Impact** | Tidak ada |
| **Dependency** | `HRD-DEP-004` |
| **Blocking Decision** | `HRD-DEC-009` sudah final: tanggung jawab HR berhenti setelah `execute`. **`HRD-Q-10` dan `HRD-Q-11` masih terbuka**: bentuk data serah terima dan perilaku bila Finance menolak batch |
| **QBE Impact** | Frontend saja untuk bagian yang boleh dikerjakan |
| **Migration Risk** | Nihil untuk bagian yang boleh dikerjakan |
| **Acceptance Criteria** | 1. Tidak ada endpoint HR yang mengubah status pembayaran. 2. Rantai berhenti pada `payroll-handoff/.../execute`. 3. Kehadiran, cuti, dan lembur yang belum selesai tidak ikut perhitungan. 4. Serah terima bersifat idempoten: menjalankannya dua kali tidak menghasilkan dua penyerahan |
| **Release Status** | `PARTIAL` — perhitungan dan rekonsiliasi `READY`; bentuk serah terima dan penanganan penolakan `BLOCKED` |

---

## 4. Gelombang C — Pengembangan orang

### `S-C2` — Kompetensi dan pelatihan

| Aspek | Isi |
| --- | --- |
| **Current State** | 18 endpoint pada 2 controller, sementara model berjumlah 13. Master data pelatihan sudah lengkap di frontend: katalog, kategori, dan aturan pelatihan wajib |
| **Target State** | Penugasan pelatihan wajib, pencatatan kehadiran pelatihan, penilaian kompetensi, dan penerbitan sertifikat |
| **Backend Impact** | `EXTEND`. Cakupan ditetapkan saat desain |
| **Frontend Impact** | Kelompok halaman baru untuk sisi transaksi; master data sudah ada |
| **Database Impact** | Kemungkinan `EXTEND` kolom pada model existing. Tidak ada tabel baru yang diharapkan |
| **Dependency** | `HRD-DEP-006` untuk sertifikat |
| **Blocking Decision** | Interval pelatihan wajib per peran adalah `HRD-Q-06`; itu isi master data |
| **QBE Impact** | `NEW CODE` untuk endpoint tambahan. Entity baru wajib `Hrd` bila memang dibutuhkan |
| **Migration Risk** | Rendah |
| **Acceptance Criteria** | 1. Pelatihan wajib yang belum dipenuhi terlihat per pegawai dan per unit. 2. Sertifikat tersimpan sebagai bukti yang dapat ditelusuri. 3. Masa berlaku sertifikat memicu peringatan sebelum kedaluwarsa |
| **Release Status** | `READY` |

Catatan batas: kompetensi dan pelatihan dirancang sebagai kemampuan administratif. Keterkaitannya
dengan kewenangan klinis **tidak** dirancang di sini, karena itu bagian `S-C1` yang `BLOCKED`.

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

Catatan batas: kontribusi mutu dan keselamatan pasien untuk tenaga medis **tidak** dirancang di
sini. Itu bagian OPPE yang `BLOCKED`.

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

### `S-C1` — Kredensial, kewenangan klinis, SPK/RKK, OPPE, dan FPPE

| Aspek | Isi |
| --- | --- |
| **Current State** | 46 endpoint pada 5 controller, 18 model termasuk `WfpCredentialLicense`, `WfpClinicalPrivilege`, `WfpCertification`, `WfpComplianceAlert`. Frontend memanggil nol untuk sisi transaksi. OPPE dan FPPE **tidak ada sama sekali**: tidak ada model, tidak ada controller, tidak ada endpoint |
| **Target State** | **Tidak ditetapkan.** Menetapkannya sekarang berarti mengarang batas kewenangan praktik dokter |
| **Backend Impact** | Tidak ditetapkan |
| **Frontend Impact** | Tidak ditetapkan |
| **Database Impact** | Tidak ditetapkan |
| **Dependency** | `HRD-DEP-005`, `HRD-DEP-007` |
| **Blocking Decision** | `HRD-DEP-007` — belum ada `requirement-completeness-gate` maupun `hospital-domain-architect` untuk slice ini. `HRD-Q-08` — pengesahan Komite Medik atas `HRD-DEC-005` belum ada |
| **QBE Impact** | Tidak dinilai |
| **Migration Risk** | Tidak dinilai |
| **Acceptance Criteria** | Tidak ditetapkan. Satu-satunya kriteria yang berlaku sekarang: **tidak ada artefak desain untuk slice ini yang boleh ditulis** |
| **Release Status** | `BLOCKED` |

Yang boleh dicatat tanpa melanggar pemblokiran: `HRD-DEC-005` sebagai posisi sementara yang
fail-safe, yaitu kredensial kedaluwarsa memberi peringatan tercatat dan tidak menghentikan
pelayanan. Itu posisi, bukan desain.

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
| **Target State** | ERD diturunkan ulang dari proses bisnis rumah sakit sesuai `HRD-DEC-004`. Model existing berstatus kandidat, bukan jawaban |
| **Backend Impact** | Tidak ditetapkan sebelum `HRD-Q-05` dijawab |
| **Frontend Impact** | Tidak ditetapkan |
| **Database Impact** | **Berpotensi merusak.** Penurunan ulang akan menghasilkan migration yang mengubah atau membuang tabel yang sudah ada, bukan membuat di ruang kosong |
| **Dependency** | `HRD-Q-05` |
| **Blocking Decision** | `HRD-Q-05` — belum diketahui apakah tabel-tabel itu sudah berisi data dari impor manual atau migrasi V1. Audit source membuktikan aplikasi tidak dapat menulis ke sana, tetapi tidak dapat membuktikan tidak ada yang mengisinya lewat jalur lain |
| **QBE Impact** | `QBE-DB-001` audit dependency fisik sebelum rename atau perubahan. `QBE-DB-002` dilarang `DROP` lalu `CREATE` bila perubahan yang mempertahankan data masih aman. `QBE-MOD-002` entity operasional baru wajib memakai prefix `Hrd` sesuai `HRD-DEC-019` |
| **Migration Risk** | **Tinggi.** Ini satu-satunya kelompok slice yang berpotensi menghasilkan migration destruktif |
| **Acceptance Criteria** | Tidak ditetapkan. Kriteria yang berlaku sekarang: **tidak ada migration destruktif yang boleh direncanakan sebelum isi tabel diketahui** |
| **Release Status** | `S-D1` s.d. `S-D4` `BLOCKED`. `S-D5` `DEFERRED` — secara teknis sama, tetapi prioritasnya paling rendah, dan tetap tidak boleh jalan sebelum `HRD-Q-05` |

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
| `READY` | **18** | `S0-A`, `S0-B` (2), `S-A1` s.d. `S-A7` (7), `S-B1` s.d. `S-B4` (4), `S-C2` s.d. `S-C5` (4), `S-E` (1) |
| `PARTIAL` | 1 | `S-B5` |
| `BLOCKED` | 6 | `S-C1`, `S-C6`, `S-D1`, `S-D2`, `S-D3`, `S-D4` |
| `DEFERRED` | 1 | `S-D5` |
| **Total** | **26** | |

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
