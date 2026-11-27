# Planning — Penataan Izin Akun SDM, Pengaman Menu Sidebar, dan Beranda Non-Klinis

| Field | Nilai |
| --- | --- |
| Tanggal | 26 November 2026 |
| Modul | Human Resource — Otorisasi Jabatan, Navigasi Sidebar (`menu-items.jsx`), dan Dashboard Beranda (`/`) |
| Repository terdampak | `QuilvianSystemFrontendDev` (source UI & unit test); `NewQuilvianSystemBackend` **tidak diubah source-nya** (hanya pembaruan data kebijakan `SysAccessPolicy` di DB dev `QuilvianNewDevHamzah` lewat API resmi `/api/v1/administrator/setting/role-access/policies` serta pembaruan dokumentasi planning ini) |
| Status | Disetujui dan diimplementasikan — 26 November 2026 |
| Alur kerja | Analisis → Planning → Implementasi → Testing (bila ada masalah: Plan & Repair → Implementasi → uji ulang) |

---

## 1. Latar belakang

Saat akun kerja Bagian SDM pada [AKUN_SDM_DATABASE_DEV.md](./AKUN_SDM_DATABASE_DEV.md) login ke aplikasi, susunan menu di sidebar dan tampilan Dashboard utama (`/`) belum mencerminkan peran masing-masing:

| Pintu masuk / Akun | Lokasi bukti | Isi yang tampil saat ini | Kondisi |
| --- | --- | --- | --- |
| **Hartini Sulistyowati** (`hartini.sulistyowati@rsmmc.local`, Kepala Bagian SDM) | `GET /v1/auth/permissions` (`1.598` izin), [sdm-1125-01-accounts.mjs](../../../../QuilvianSystemFrontendDev/test-with-agy/scripts/sdm-1125-01-accounts.mjs) | Hanya melihat `85` dari `198` butir SDM (`70` tanpa gerbang + `15` bergerbang: Kontrak, Penempatan/Mutasi, Presensi 3, Cuti 2, Lembur 1, Kotak Masuk lama 1). Di luar SDM ikut melihat *Payroll*, *Pengadaan Barang & Jasa* (*Antrean Persetujuan*), *Finance*, *CRM*, *Bank Darah*, *Administrator*, *Akuntansi*, *Logistik*, *Pelayanan Kesehatan*. | **113 menu SDM modern tersembunyi** (*Dashboard SDM*, *Rekrutmen*, *Onboarding*, *Kinerja*, *Diklat*, *Benefits*, *Kredensial*, *OPPE/FPPE*, *Hubungan Industrial*, *K3RS*, *Perencanaan Strategis SDM*, *Kotak Masuk V2*, *Layanan SDM*). |
| **Yudha Hermawan** (`yudha.hermawan@rsmmc.local`, Staf Adm. Kepegawaian) | `GET /v1/auth/permissions` (`1.110` izin) | Hanya melihat `78` dari `198` butir SDM (`70` tanpa gerbang + `8` bergerbang: *Kontrak & Masa Kerja* 4 dan *Penempatan, Mutasi & Promosi* 4). Menu *Pengelolaan Tenaga Kerja* dan *Kotak Masuk* hilang semua. | **120 menu SDM tersembunyi**, sementara *Bank Darah*, *CRM*, *Laboratorium*, *Rawat Jalan*, dan menu tanpa gerbang tetap tampil. |
| **Ns. Endang Purwanti** (`endang.purwanti@rsmmc.local`, Kepala Ruangan) | `GET /v1/auth/permissions` (`13` izin) | Melihat `74` butir SDM (`70` tanpa gerbang + `4` *Kontrak & Masa Kerja*). | Dashboard utama (`/`) menampilkan kartu `0` + **banner merah** `"Data dashboard belum dapat dimuat sepenuhnya"` karena `5` sumber dashboard menjawab `403`. |
| **Novi Ariyanti** (`novi.ariyanti@rsmmc.local`, Perawat Rawat Inap — tanpa akses SDM) | `GET /v1/auth/permissions` (`34` izin) | Masih melihat `70` butir SDM (*Master Data* HR `55`, *Administrasi Kepegawaian* `9` termasuk *Batch Data Awal*, *Pemberhentian & Pensiun* `6`), serta *Administrator*, *Manajemen Role*, *Akuntansi*, *Logistik*. | Sidebar tidak memfilter butir tanpa `requiredPermission`; jika diklik, backend menolak `403`. Dashboard utama menampilkan banner **kuning** `"Sebagian sumber dashboard belum tersedia"`. |
| **SuperAdmin** | [AccessPermissionService.cs](../../../../Services/Security/AccessPermissionService.cs) | Melihat seluruh `198` butir SDM (termasuk *Dashboard SDM*, *Rekrutmen & Seleksi*, *Penerimaan & Onboarding*) dan angka `Dokter 12 / Pasien 72 / Poliklinik 4`. | `UnrestrictedAccess = true` dan lolos `KioskReadPolicy`. |

### 1.1 Temuan utama

| No. | Temuan | Bukti path:baris | Dampak |
| ---: | --- | --- | --- |
| 1 | **Pembuat akun SDM uji hanya menyalin izin jabatan lama `Manajer HR` / `Staff HR` + izin Kontrak** | `QuilvianSystemFrontendDev/test-with-agy/scripts/sdm-1125-01-accounts.mjs:67-81`; `Seeders/AccessMenuSeeder.cs:11-15` | `AccessMenuSeeder` memang tidak pernah membuat `SysAccessPolicy`. Jabatan lama `Manajer HR` (`1.594` izin) dan `Staff HR` (`1.102` izin) berisi izin warisan sebelum modul HR baru dibangun, sehingga `113–120` menu SDM baru belum dicentang untuk akun SDM. |
| 2 | **Kebijakan warisan `Manajer HR` / `Staff HR` menyimpan ratusan izin non-SDM** | `GET /v1/auth/permissions` (`291` resource non-HR pada Hartini, `260` pada Yudha) | Akun SDM memegang izin *Bank Darah* (`BloodOrder`, `BloodComponent`), *CRM* (`PatientMembership`), *Finance*, *Rawat Jalan*, *Laboratorium*, dan *Billing*, sehingga menu klinis dan pengadaan ikut muncul di akun SDM. |
| 3 | **`70` butir SDM dan *Manajemen Role* belum memiliki `requiredPermission` di sidebar** | `QuilvianSystemFrontendDev/src/utils/menu-sidebar/menu-items.jsx:200-211,236-633,961-1063`; `filter-menu-items-by-permission.jsx:11-12` | `filterMenuItemsByPermission` membiarkan setiap butir tanpa `requiredPermission` tampil untuk semua user yang login, sehingga perawat/staf biasa ikut melihat menu *Master Data* HR, *Administrasi Kepegawaian*, *Pemberhentian & Pensiun*, dan *Akses Role*. |
| 4 | **Bentrok izin lintas modul `ApprovalInbox:Read` pada menu Pengadaan** | `QuilvianSystemFrontendDev/src/utils/menu-sidebar/menu-items.jsx:3268-3273`; `procurement-approval-inbox-view.jsx:59-67` | Menu *Pengadaan Barang & Jasa → Antrean Persetujuan* memakai `{ resource: "ApprovalInbox", action: "Read" }`. Begitu Hartini diberi `ApprovalInbox:Read` untuk persetujuan kontrak SDM, menu Pengadaan ikut terbuka di sidebar-nya. |
| 5 | **Dashboard utama (`/`) menembak 5 endpoint Kiosk/Klinis tanpa cek izin lebih dulu** | `QuilvianSystemFrontendDev/src/lib/services/home/home-dashboard.service.js:160-230`; `NewQuilvianSystemBackend/Program.cs:1979-2000` | Empat endpoint `kiosk/options` dipagari `KioskReadPolicy` (hanya `SuperAdmin`, `Administrator`, `KioskDevice`, `SystemUser`), sehingga pegawai biasa selalu menerima `403` dan melihat kartu `0` plus alert kuning/merah setiap kali login. |
| 6 | **Layanan mandiri pegawai tidak hilang — sudah dipindah ke Side Profil kanan** | `QuilvianSystemFrontendDev/src/lib/constants/self-services/self-service-profile-menu.js:1-35` | *Presensi Saya*, *Cuti Saya*, dan *Slip Gaji Saya* dibuka melalui menu profil di kanan atas, bukan di sidebar kiri SDM (keputusan `PLANNING_PEMINDAHAN_MENU_WORKSPACE_KE_SIDE_PROFIL.md`). |

### 1.2 Contoh di rumah sakit

- **Hari ini:** Ibu **Hartini** (Kepala Bagian SDM RS MMC Jakarta) login ke sistem untuk memantau komposisi pegawai di *Dashboard SDM*, menindaklanjuti lowongan perawat di *Rekrutmen & Seleksi*, serta mengecek kasus *Onboarding* pegawai baru. Namun begitu login, layar Beranda justru menampilkan `Dokter 0 / Pasien 0 / Poliklinik 0` dengan peringatan kuning, menu *Dashboard SDM*, *Rekrutmen*, dan *Onboarding* hilang dari sidebar, sementara menu *Pengadaan Barang & Jasa* dan *Bank Darah* malah ikut muncul. Di sisi lain, **Ns. Novi** (Perawat Rawat Inap) ikut melihat menu *Master Data HR*, *Administrasi Kepegawaian*, *Batch Data Awal*, dan *Manajemen Role* meski ditolak server saat diklik.
- **Sesudah planning ini dikerjakan:** Saat Ibu Hartini dan Pak Yudha login, menu *Sumber Daya Manusia* menampilkan seluruh submodul kerja SDM sesuai porsi pembuat vs penyetuju (*Dashboard SDM*, *Rekrutmen & Seleksi*, *Penerimaan & Onboarding*, *Kontrak*, *Penjadwalan/Presensi/Cuti/Lembur/SPPD*, *Kinerja & Diklat*, *Benefits*, *Kredensial & K3RS*, *Perencanaan Strategis SDM*, *Kotak Masuk V2*, *Layanan SDM*), menu *Bank Darah* dan *Pengadaan Barang & Jasa* tidak lagi muncul di akun SDM, menu admin/HR tertutup dari sidebar Ns. Novi, dan halaman `/` menampilkan ringkasan serta pintasan kerja yang bersih tanpa banner galat `403`.

---

## 2. Tujuan

Menyelaraskan paket hak akses (`SysAccessPolicy`) kedua akun SDM pada [AKUN_SDM_DATABASE_DEV.md](./AKUN_SDM_DATABASE_DEV.md), memasang gerbang `requiredPermission` pada seluruh butir SDM dan *Manajemen Role* yang masih terbuka di sidebar, memisahkan izin menu *Antrean Persetujuan Pengadaan*, serta meniadakan panggilan Kiosk `403` pada Beranda `/` untuk akun non-klinis.

---

## 3. Gambaran layar

### 3.1 Sidebar kiri setelah penataan (per perbandingan peran)

```text
+-----------------------------------+   +-----------------------------------+   +-----------------------------------+
| HARTINI (Kepala Bagian SDM)       |   | YUDHA (Staf Adm. Kepegawaian)     |   | NOVI (Perawat Rawat Inap)         |
+-----------------------------------+   +-----------------------------------+   +-----------------------------------+
| [Dashboard]                       |   | [Dashboard]                       |   | [Dashboard]                       |
| PERUSAHAAN                        |   | PERUSAHAAN                        |   | PELAYANAN KESEHATAN               |
|  v Sumber Daya Manusia            |   |  v Sumber Daya Manusia            |   |  > Rawat Inap / Keperawatan       |
|    - Dashboard SDM                |   |    > Master Data (baca/kelola)    |   |    (Menu SDM & Manajemen Role     |
|    > Master Data                  |   |    v Siklus Kepegawaian           |   |     tidak lagi tampil di sidebar) |
|    v Siklus Kepegawaian           |   |      > Rekrutmen & Seleksi        |   |                                   |
|      > Rekrutmen & Seleksi        |   |      > Penerimaan & Onboarding    |   | (Layanan mandiri: Cuti/Presensi/  |
|      > Penerimaan & Onboarding    |   |      > Kontrak & Masa Kerja       |   |  Slip Gaji ada di Side Profil     |
|      > Kontrak & Masa Kerja       |   |      > Penempatan, Mutasi         |   |  pojok kanan atas)                |
|      > Penempatan, Mutasi         |   |      > Administrasi Kepegawaian   |   +-----------------------------------+
|      > Administrasi Kepegawaian   |   |      > Pemberhentian & Pensiun    |
|      > Pemberhentian & Pensiun    |   |    > Pengelolaan Tenaga Kerja     |
|    > Pengelolaan Tenaga Kerja     |   |      (Roster, Presensi, Cuti,     |
|    > Pengembangan & Kinerja       |   |       Lembur, SPPD)               |
|    > Kompensasi & Kesejahteraan   |   |    > Pengembangan & Kinerja       |
|    > Tata Kelola & Kepatuhan      |   |    > Kompensasi & Kesejahteraan   |
|    > Perencanaan Strategis SDM    |   |    > Tata Kelola & Kepatuhan      |
|    > Kotak Masuk Persetujuan (V2) |   |    > Layanan SDM & Meja Bantuan   |
|    > Layanan SDM & Meja Bantuan   |   |  (Tidak ada: Pengadaan, Bank      |
|  (Tidak ada: Pengadaan, Bank      |   |   Darah, Manajemen Role, dsb.)    |
|   Darah, CRM, Manajemen Role)     |   +-----------------------------------+
+-----------------------------------+
```

### 3.2 Beranda (`/`) untuk akun non-Kiosk / non-klinis (Hartini, Yudha, Endang, Novi)

```text
+---------------------------------------------------------------------------------------------------+
| [Logo MMC]  Metropolitan Medical Centre Dashboard                  [Sistem aktif dan sinkron]     |
|             Pantau performa layanan, aktivitas operasional, dan akses kerja sesuai peran Anda.    |
+---------------------------------------------------------------------------------------------------+
| [Info biru lembut — bukan alert merah/kuning]                                                     |
| Ringkasan kunjungan pasien dan poliklinik hanya ditampilkan untuk akun klinik/kiosk berwenang.     |
+---------------------------------------------------------------------------------------------------+
| PINTASAN KERJA SESUAI IZIN AKUN (hanya tampil kartu yang berhak)                                  |
| +-----------------------------------+ +-----------------------------------+ +-------------------+ |
| | Dashboard SDM                     | | Kotak Masuk Persetujuan (V2)      | | Layanan Mandiri   | |
| | Pantau statistik pegawai, kontrak | | Tinjau permohonan yang menunggu   | | Buka profil,      | |
| | berakhir, dan dokumen kedaluwarsa | | persetujuan Anda                  | | presensi, & cuti  | |
| | [Buka Dashboard SDM ->]           | | [Buka Kotak Masuk ->]             | | [Menu Profil]     | |
| +-----------------------------------+ +-----------------------------------+ +-------------------+ |
+---------------------------------------------------------------------------------------------------+
```

| Wilayah layar | Perilaku | Pemakai pola existing |
| --- | --- | --- |
| Hero Beranda | Pertahankan komponen `dashboard-hero` existing pada `dashboard-home.jsx` | Ya (`dashboard-home.jsx`) |
| Panel Ringkasan Klinis (`InformasiCardCount`, grafik kunjungan, tab poliklinik) | Hanya dimuat dan dirender jika akun berhak (`unrestrictedAccess` / `KioskRead` / `PatientEncounter:Read`) | Ya (`dashboard-home.jsx` + `home-dashboard-context.jsx`) |
| Panel Pintasan Peran (saat sumber klinis tidak berhak) | Kartu berbasis `InformationAlert` & `BaseButton` + pengecekan `useEffectivePermissions()` (`decidePermission`) | Ya (`InformationAlert`, `BaseButton`) |

---

## 4. Keputusan

| No | Hal | Rekomendasi | Alternatif |
| ---: | --- | --- | --- |
| 1 | **Target jabatan yang diubah izinnya di DB Dev** | Perbarui **hanya dua jabatan baru** di [AKUN_SDM_DATABASE_DEV.md](./AKUN_SDM_DATABASE_DEV.md): `Human Resource × Kepala Bagian SDM` (**Hartini**) dan `Human Resource × Staf Administrasi Kepegawaian` (**Yudha**) melalui API resmi `POST /api/v1/administrator/setting/role-access/policies`. Jabatan lama `Manajer HR` dan `Staff HR` (yang menurut [FE-HRD-409.md](../task/report/frontend/FE-HRD-409.md#L37) dipakai 21 akun lama) tidak disentuh. | Sekaligus mengubah `Manajer HR` dan `Staff HR` (berisiko mengubah hak akun lain tanpa audit terpisah), atau hanya menambahkan 2–3 izin saja lewat UI. |
| 2 | **Pembersihan izin non-SDM warisan** | Cabut izin klinis/logistik/pengadaan warisan (`Blood*`, `PatientMembership`, `Lab*`, `Billing*`, `FinancePurchaseOrder`, dll.) dari `Kepala Bagian SDM` dan `Staf Administrasi Kepegawaian`, pertahankan **hanya** izin HR (`HUMAN_RESOURCE_*`), kesiapan Payroll terkait HR (`WorkforcePayrollData`, `PayrollDataReadiness`), serta master referensi baca-saja yang memang dipakai formulir HR (`Bank:Read`, `SalaryStructure:Read` per `HRD-DEC` pengisian data gaji). | Mempertahankan 291 resource non-HR warisan pada akun SDM (membuat akun SDM tetap melihat menu *Bank Darah* dan *CRM*). |
| 3 | **Pemisahan tugas (*maker-checker*) antara Hartini dan Yudha** | **Yudha** (Staf Adm. Kepegawaian) memegang izin operasional pembuat (`Read`, `Create`, `Update`, `Submit`, kelola draf/tiket/onboarding/rekrutmen/presensi/roster/mutasi, **tanpa** izin persetujuan final/penutupan periode/nominal gaji `ViewAmount` pada kontrak dan `ApprovalInbox`). **Hartini** (Kepala Bagian SDM) memegang izin penyetuju dan pengawasan (`Read`, `Approve`, `Reject`, `RequestRevision`, `ViewAmount`, tutup periode/verifikasi, `HrdWorkforceDashboard:Read`, `WorkflowInboxInstance:ViewInbox`, `ApprovalInbox:Read/Approve/Reject`, `EmployeeDocumentIssuance:AccessAllEmployees`, `WorkforceOnboardingCase:ReadAllUnits`). | Memberi seluruh izin penuh ke kedua akun (melanggar prinsip *four-eyes* / pembuat tidak boleh menyetujui ajuan sendiri). |
| 4 | **Penutupan butir sidebar tanpa `requiredPermission`** | Pasang `requiredPermission` **hanya pada level leaf** (sesuai `HR-13`: folder tidak boleh memakai `requiredPermission`) untuk seluruh **70 butir SDM** (*Master Data* HR 55, *Administrasi Kepegawaian* 9, *Pemberhentian & Pensiun* 6) dan **Manajemen Role → Akses Role** (`RoleAccess:Read`), lalu sesuaikan assertion `HR-15` di [hr-master-data-sidebar-grouping.test.mjs](../../../../QuilvianSystemFrontendDev/tests/unit/hr-master-data-sidebar-grouping.test.mjs#L1016-L1032). Butir non-SDM lain (*Administrator → Master Data*, *Akuntansi*, *Logistik*, *Pelayanan Kesehatan*) dibiarkan pada task per modul masing-masing. | Menutup sekaligus seluruh ~200+ item non-SDM di `menu-items.jsx` dalam satu PR (berisiko mematahkan unit test puluhan modul lain di luar SDM). |
| 5 | **Pemisahan gerbang *Antrean Persetujuan Pengadaan*** | Ubah `requiredPermission` pada `corporateProcurementApprovalInbox` (`/corporate/procurement/approvals/inbox`) dari `{ resource: "ApprovalInbox", action: "Read" }` menjadi daftar izin baca dokumen Pengadaan: `[{ resource: "ProcurementRequest", action: "Read" }, { resource: "ProcurementRequestVerification", action: "Read" }, { resource: "ProcurementNeedPlan", action: "Read" }, { resource: "ProcurementBudget", action: "Read" }, { resource: "ProcurementPackagePlan", action: "Read" }]`. | Menambah operator `AND` pada `filterMenuItemsByPermission.jsx` (mengubah komponen bersama yang diuji ketat oleh `FE-BD-006`). |
| 6 | **Perilaku Dashboard utama (`/`) untuk non-Kiosk / non-klinis** | Di `home-dashboard-context.jsx` dan `dashboard-home.jsx`, tunda pemuatan 5 sumber klinis hingga status izin dimuat (`loaded`); sumber hanya dipanggil jika akun punya akses (`unrestrictedAccess === true`, atau role `SuperAdmin`/`Administrator`/`Kiosk`, atau untuk kunjungan jika memegang `PatientEncounter:Read` beserta akses Kiosk). Bila sumber Kiosk tidak berhak, jangan panggil `kiosk/options`, jangan tampilkan kartu `0` maupun alert kuning/merah, dan tampilkan panel **Pintasan Kerja Sesuai Izin**. | Membiarkan `home-dashboard.service.js` memanggil semua endpoint lalu menelan error `403` secara diam-diam (tetap memicu traffic `403` di jaringan dan menampilkan angka `0` yang menyesatkan). |

> **Catatan keterkaitan dokumen lama:** Keputusan nomor 4 memperbarui asumsi pada komentar [hr-master-data-sidebar-grouping.test.mjs:1027](../../../../QuilvianSystemFrontendDev/tests/unit/hr-master-data-sidebar-grouping.test.mjs#L1027) (yang sebelumnya menyebut *Master Data Kompetensi* tanpa `requiredPermission`). Susunan hierarki, key, label, urutan, dan `pathname` dari `198` menu SDM sesuai `PLANNING_IMPLEMENTASI_STRUKTUR_MENU_SDM_QUILVIAN_V2.md` tetap dipertahankan **100% utuh** — tidak ada menu yang dihapus, ditambah, atau dipindah.

---

## 5. Isi

### 5.1 Matriks Pembaruan Izin DB Dev (`Kepala Bagian SDM` vs `Staf Adm. Kepegawaian`)

Pembaruan dilakukan dengan membaca template katalog resmi `GET /v1/administrator/setting/role-access/resources/structured` lalu menyimpan paket bersih lewat `POST /api/v1/administrator/setting/role-access/policies`:

| No | Kelompok / Modul | Izin yang Diberikan ke **Hartini** (`Kepala Bagian SDM`) | Izin yang Diberikan ke **Yudha** (`Staf Adm. Kepegawaian`) | Catatan Pemisahan Tugas |
| ---: | --- | --- | --- | --- |
| 1 | **Dashboard SDM** | `HrdWorkforceDashboard:Read` | `HrdWorkforceDashboard:Read` | Keduanya dapat memantau ringkasan SDM (alternatif: baca untuk Kabag saja jika diinginkan). |
| 2 | **Master Data HR** (55 butir) | `Read`, `Create`, `Update`, `Delete` pada seluruh controller Master Data HR (`Organization`, `OrganizationUnit`, `Employee`, `Doctor`, `Competency`, `Shift`, `WorkSchedule`, `LeaveType`, `PerformanceCycle`, `TrainingCatalog`, `BenefitPlan`, `WorkflowDefinition`, `ApprovalMatrix`, `ViolationType`, `SanctionType`, dst.) | `Read`, `Create`, `Update` (tanpa `Delete` untuk master kritis seperti `Organization`, `ApprovalMatrix`, `WorkflowDefinition`, `Employee`) | Sesuai prinsip `be-sec-014`: manajer penuh, staf operasional tanpa hapus data inti/workflow. |
| 3 | **Rekrutmen & Seleksi** | `JobRequisition`, `JobVacancy`, `Candidate`, `CandidateScreening`, `CandidateEvaluation` (`Read`, `Evaluate`, `ViewAll`), `JobOffer`, `JobOfferApproval` (`Read`, `Approve`), `OccupationalHealthClearance`, `TalentPool`, `HiringHandoff` (`Read`, `Execute`) | `JobRequisition` (`Read`, `Create`, `Update`, `Submit`), `JobVacancy` (`Read`, `Create`, `Update`, `Publish`), `Candidate` (`Read`, `Create`, `Update`, `UploadDocument`), `CandidateScreening` (`Read`, `Evaluate`), `CandidateEvaluation` (`Read`, `ViewAll`), `JobOffer` (`Read`, `Create`, `Update`), `OccupationalHealthClearance` (`Read`), `TalentPool` (`Read`, `Create`, `Promote`), `HiringHandoff` (`Read`) | Hartini menyetujui *JobOfferApproval* dan mengeksekusi serah terima; Yudha memproses administrasi kandidat & lowongan. |
| 4 | **Penerimaan & Onboarding** | `WorkforceLifecycleWorklist:Read`, `WorkforceOnboardingCase` (`Read`, `ReadAllUnits`, `Reassign`, `DeclareReady`, `CancelJoin`), `WorkforceOnboardingTemplate` (CRUD), `WorkforcePreceptorship` (`Read`, `Manage`), `WorkforceOnboardingAccessRequest` (`Read`, `Complete`, `Retry`), `WorkforceLifecycleSetting` (`Read`, `Update`) | `WorkforceLifecycleWorklist:Read`, `WorkforceOnboardingCase` (`Read`, `ReadAllUnits`, `CompleteItem`), `WorkforceOnboardingTemplate` (`Read`), `WorkforcePreceptorship` (`Read`), `WorkforceOnboardingAccessRequest` (`Read`, `Retry`), `WorkforceLifecycleSetting` (`Read`) | Hartini mengesahkan *Siap Kerja* (`DeclareReady`) & membatalkan bergabung (`CancelJoin`). |
| 5 | **Kontrak & Masa Kerja** | Tetap: `WorkforceEmploymentContract` (`Read`, `ReadAllUnits`, `ViewAmount`), `WorkforceProbation` (`Read`), ditambah `WorkforceInitialContractBatch` (`Read`) | Tetap: `WorkforceEmploymentContract` (`Read`, `ReadAllUnits`, `MarkSigned`, `SubmitDecision`, `RecordNotification`, `RequestCorrection`, `UploadDocument`), `WorkforceProbation` (`Read`), ditambah `WorkforceInitialContractBatch` (`Read`, `Create`, `Update`, `Submit`) | Menjaga pemisahan *maker-checker* yang sudah diuji di `PLANNING_PERSETUJUAN_T15_T18_KONTRAK_DAN_MASA_KERJA.md`. |
| 6 | **Penempatan, Mutasi, Administrasi & Pemberhentian** | `HrdPersonnelActionOrder` (`Read`, `Approve`, `Reject`, `Cancel`), `HrdInitialPlacementBatch` (`Read`, `Approve`), `HrdEmployee360:Read`, `EmployeeProfileChange` (`Read`, `Update`), `Wfp*` (`Read`, `ReadAll`, `ViewAmount` pada `WfpSalaryAssignment`), `OfficialDutyRequest` (`Read`, `Approve`, `Reject`), `EmployeeDocumentIssuance` (`Read`, `ApproveDraft`, `Issue`, `Download`, `Revoke`, `AccessAllEmployees`), `AdministrativeDocumentRegistry:Read`, `ResignationRequest` (`Read`, `Approve`, `Reject`, `Handoff`), `RetirementAdministration` (`Read`, `Approve`, `Handoff`), `OffboardingHandoverAdministration` (`Read`, `VerifyTask`, `VerifyAllTasks`, `Finalize`), `ExitClearance` (`Read`, `ClearHr`, `Complete`) | `HrdPersonnelActionOrder` (`Read`, `Create`, `Update`), `HrdInitialPlacementBatch` (`Read`, `Create`, `Update`), `HrdEmployee360:Read`, `EmployeeProfileChange` (`Read`, `Create`, `Update`), `Wfp*` (`Read`, `ReadAll`, `Create`, `Update` — tanpa `ViewAmount`), `OfficialDutyRequest` (`Read`, `Create`), `EmployeeDocumentIssuance` (`Read`, `Request`, `Download`, `AccessAllEmployees`), `AdministrativeDocumentRegistry:Read`, `ResignationRequest` (`Read`, `Create`, `Synchronize`), `RetirementAdministration` (`Read`, `Create`, `Update`), `OffboardingHandoverAdministration` (`Read`, `ManageTasks`, `SubmitTask`), `ExitClearance` (`Read`, `Create`, `Update`) | Yudha menyiapkan SK/mutasi/surat dinas/berkas pensiun; Hartini menyetujui, menerbitkan, dan memfinalisasi clearance HR. |
| 7 | **Pengelolaan Tenaga Kerja** (Roster, Presensi, Cuti, Lembur, SPPD) | `RosterPeriod` (`Read`, `ManageAllUnits`), `OnCallAssignment:Read`, `HrdSchedulingSetting` (`Read`, `Manage`), `AttendanceDaily:Read`, `AttendanceSetting` (`Read`, `Update`), `AttendanceCorrectionMonitoring` (`Read`, `Synchronize`, `Apply`), `EarlyLeavePermission` (`Read`, `Approve`, `Reject`), `AttendancePeriod` (`Read`, `Process`, `Close`, `Reopen`), `AttendanceDeviceIncident:Read`, `LeaveApproval` (`View`, `Approve`), `LeaveBalanceLedger:Read`, `LeaveAdjustment` (`Read`, `Post`, `Reverse`), `LeaveRecall` (`View`, `Read`, `Approve`), `LeaveReconciliation` (`View`, `Close`), `OvertimeApproval` (`View`, `Approve`), `OvertimeVerificationModern` (`View`, `Verify`), `CompensatoryTimeOffModern` (`View`, `Execute`), `OvertimePeriodModern` (`View`, `Close`, `Reopen`), `OvertimeSetting` (`View`, `Manage`), `BusinessTravelRequest:Read`, `BusinessTravelAdvance` (`Read`, `Approve`), `BusinessTravelSettlement` (`Read`, `Verify`, `Close`, `ApproveMissingReceipt`), `TravelRatePolicy` (`ViewRate`, `ManageRate`, `MonitorOverdue`), `DirectExpenseClaim` (`Read`, `Verify`, `Approve`) | `RosterPeriod:Read`, `OnCallAssignment` (`Read`, `Create`, `Update`), `HrdSchedulingSetting:Read`, `AttendanceDaily:Read`, `AttendanceSetting:Read`, `AttendanceCorrectionMonitoring` (`Read`, `Synchronize`), `AttendanceCorrection` (`Read`, `CreateOnBehalf`), `EarlyLeavePermission` (`Read`, `Create`), `AttendancePeriod` (`Read`, `Create`, `Update`), `AttendanceDeviceIncident` (`Read`, `Create`), `LeaveBalanceLedger:Read`, `LeaveAdjustment` (`Read`, `Create`, `Update`, `Submit`), `LeaveRecall` (`View`, `Read`, `Create`, `Submit`), `LeaveReconciliation:View`, `OvertimeVerificationModern:View`, `CompensatoryTimeOffModern:View`, `OvertimePeriodModern:View`, `OvertimeSetting:View`, `BusinessTravelRequest` (`Read`, `Create`, `Update`), `BusinessTravelAdvance` (`Read`, `Request`), `BusinessTravelSettlement` (`Read`, `Submit`), `TravelRatePolicy:ViewRate`, `DirectExpenseClaim` (`Read`, `Create`) | Menutup periode & menyetujui pengajuan/selisih tetap di Hartini; monitoring & input operasional oleh Yudha. |
| 8 | **Pengembangan & Kinerja** | `PerformanceCycleGoal` (View/Manage Cycle/Goal/Metric), `PerformanceAppraisal:ViewSubordinates`, `PerformanceCalibration` (View/Manage Calibration & Dispute), `PerformancePipAndCareer` (View/Manage PIP & ClinicalCareer), `MandatoryHospitalTraining` (`Read`, `Manage`), `TrainingSession`, `TrainingCertificate` (`Read`, `Issue`), `AnnualTrainingPlan` (`Read`, `Approve`, `Reject`), `TrainingBonding` (`Read`, `Manage`), `PlataranSehatSkp` (`Read`, `Verify`, `Sync`) | `PerformanceCycleGoal` (`ViewCycle`, `ViewGoal`, `ViewMetric`), `PerformanceCalibration:ViewCalibration`, `PerformancePipAndCareer:ViewPip`, `MandatoryHospitalTraining:Read`, `TrainingSession` (`Read`, `Create`, `Update`), `TheoryPostTestAssessment:Read`, `ClinicalOsceAssessment:Read`, `TrainingCertificate:Read`, `AnnualTrainingPlan` (`Read`, `Create`, `Update`, `Submit`), `TrainingBonding:Read`, `PlataranSehatSkp` (`Read`, `Sync`) | Asesmen klinis OSCE/OPPE tetap mengikuti batasan medis; HRD mengelola siklus, kelas diklat, dan ikatan dinas. |
| 9 | **Kompensasi & Tata Kelola Kepatuhan** | `WorkforcePayrollData` (`Read`, `ViewAmount`), `BenefitPlanEligibility` (`Read`, `Manage`), `BenefitEnrollment` (`Read`, `Manage`, `VerifyDependent`), `EmployeeBenefitEnrollment` (`Read`, `Approve`, `Activate`), `HrdInstallmentPlanApproval` (`Read`, `Approve`, `Reject`), `EmployeeBenefitReceivableReport:Read`, `InHouseHealthcare:Read`, `BenefitClaim` (`Read`, `Verify`, `Settle`), `ProfessionalLicense` (`Read`, `VerifyPsv`), `LearningAndDevelopment:Read`, `SpkRkkIssuance:Read`, `RecredentialingQuality` (`Read`, `ViewSyncAudit`), `OppeOngoingEvaluation:ViewPeriod`, `FppeFocusedEvaluation:Read`, `EmployeeDisciplinary` (View/Investigate/Issue/Suspend), `EmployeeGrievance` (`Mediate`, `WhistleblowerReview`), `BipartiteForum` (CRUD), `EmployeeSeparation` (`Read`, `Initiate`, `CalculateSeverance`, `ViewSeverance`), `MultiDeptExitClearance` (`Read`, `SignoffChecklist`, `Finalize`), seluruh modul **K3RS** (`OccupationalHealthRecord:ViewSummary`, `WorkRestrictionAndInjury`, `NeedleStickEmergency`, `StaffVaccination`, `OccupationalMedicalExamination` — tanpa membuka catatan klinis terbatas `ViewClinicalDetail` jika tidak diperlukan) | `WorkforcePayrollData:Read` (tanpa `ViewAmount`), `BenefitPlanEligibility:Read`, `BenefitEnrollment:Read`, `EmployeeBenefitEnrollment` (`Read`, `Create`, `Update`, `Submit`), `HrdInstallmentPlanApproval:Read`, `EmployeeBenefitReceivableReport:Read`, `InHouseHealthcare:Read`, `BenefitClaim:Read`, `ProfessionalLicense:Read`, `LearningAndDevelopment:Read`, `SpkRkkIssuance:Read`, `RecredentialingQuality:Read`, `EmployeeDisciplinary` (`ViewIncidents`, `ViewSanctions`, `CreateIncident`), `BipartiteForum` (`Read`, `Schedule`, `RecordMinutes`), `EmployeeSeparation:Read`, `MultiDeptExitClearance:Read`, K3RS baca-saja (`ViewSummary`/`Read`) | Data gaji (`ViewAmount`) hanya untuk Hartini; catatan medis klinis karyawan tetap terjaga privasinya. |
| 10 | **Perencanaan Strategis SDM, Kotak Masuk V2 & Layanan SDM** | Seluruh `WorkforceStaffing*`, `WorkforcePositionFormation`, `WorkforceHeadcountRequest`, `WorkforcePlanningSetting`, `WorkforceInitialFormationBatch`, `WorkforceRecruitmentQueue`, `WorkforceManpowerPlan` (`Read`, `Update`, `Create`); `WorkflowInboxInstance:ViewInbox`, `WorkflowApprovalAction` (Approve/Reject/Return), `ApprovalInbox` (`Read`, `Approve`, `Reject`, `RequestRevision`), `WorkflowDelegation` (`ViewDelegationList`, `ViewAllDelegations`, `CreateDelegation`, `RevokeDelegation`, `ViewOwn`), `WorkflowSlaMatrix` (`ViewMatrix`, `ConfigureMatrix`, `ViewSlaOverdue`, `ManualEscalate`, `ExportSlaReport`); `HrServiceTicket` (`Read`, `Assign`, `Process`), `HrServiceCatalog` (CRUD), `EmployeeDocumentIssuance` | `WorkforceStaffingWorkload`, `WorkforcePositionFormation`, `WorkforceHeadcountRequest` (`Read`, `Create`, `Update`), `WorkforceStaffingGap` (`Read`, `Create`), `WorkforcePlanningSetting:Read`, `WorkforceInitialFormationBatch` (`Read`, `Create`, `Update`), `WorkforceStaffingStandard:Read`, `WorkforceRecruitmentQueue:Read`, `WorkforceManpowerPlan` (`Read`, `Create`, `Update`); `WorkflowDelegation` (`ViewOwn`, `CreateDelegation`, `RevokeDelegation`); `HrServiceTicket` (`Read`, `Assign`, `Process`), `HrServiceCatalog:Read`, `EmployeeDocumentIssuance` (`Read`, `Request`, `Download`, `AccessAllEmployees`) | Yudha tidak mendapat `ApprovalInbox` / `WorkflowInboxInstance` sebagai penyetuju, namun tetap mendapat akses meja bantuan, perencanaan tenaga, dan layanan surat. |
| 11 | **Referensi lintas modul & Layanan Mandiri (ESS)** | `Profile` (`Read`, `Update`), `Bank:Read`, `SalaryStructure:Read`, `PayrollDataReadiness:Read`, serta seluruh resource `My*` (`MyAttendanceCorrection`, `MyLeaveRequest`, `MyLeaveBalance`, `MyLeaveCalendar`, `MyLeaveCancellation`, `MyOvertime`, `MyProfileChange`, `MySchedule`, `MyScheduleChange`, `MyAttendanceDispensation`, `MyShiftSwap`, `MyResignation`, `MyPayslip`) | Sama (`Profile`, `Bank:Read`, `SalaryStructure:Read`, `PayrollDataReadiness:Read`, dan seluruh resource `My*`) | Agar formulir HR (seperti pengisian rekening/gaji) dan menu Side Profil kanan berfungsi penuh. |

### 5.2 Pemetaan `requiredPermission` Tambahan di Sidebar ([menu-items.jsx](../../../../QuilvianSystemFrontendDev/src/utils/menu-sidebar/menu-items.jsx))

#### A. Manajemen Role (1 butir)
| Menu / Label | Key | Pathname | `requiredPermission` target | Sumber controller backend |
| --- | --- | --- | --- | --- |
| Akses Role | `administratorRoleAccess` | `/administrator/settings/role-access` | `{ resource: "RoleAccess", action: "Read" }` | `RoleAccessController.cs` |

#### B. Administrasi Kepegawaian (9 butir)
| No | Label | Key | Pathname | `requiredPermission` target | Sumber controller backend |
| ---: | --- | --- | --- | --- | --- |
| 1 | Surat Keputusan | `corporatePersonnelActionOrders` | `/hr/workforce-core/personnel-actions` | `{ resource: "HrdPersonnelActionOrder", action: "Read" }` | `HrdPersonnelActionOrderController.cs` |
| 2 | Surat Tugas | `corporateOfficialDuties` | `/hr/workforce-core/official-duties` | `{ resource: "OfficialDutyRequest", action: "Read" }` | `OfficialDutyRequestController.cs` |
| 3 | Surat Keterangan | `corporateEmployeeDocuments` | `/hr/workforce-core/employee-documents` | `{ resource: "EmployeeDocumentIssuance", action: "Read" }` | `EmployeeDocumentIssuanceController.cs` |
| 4 | Dokumen Administrasi | `corporateAdministrativeDocuments` | `/hr/workforce-core/administrative-documents` | `[{ resource: "AdministrativeDocumentRegistry", action: "Read" }, { resource: "HrdPersonnelActionOrder", action: "Read" }, { resource: "OfficialDutyRequest", action: "Read" }, { resource: "EmployeeDocumentIssuance", action: "Read" }]` | `AdministrativeDocumentRegistryController.cs` + `administrative-documents-view.jsx` |
| 5 | Pengajuan Perubahan Data | `corporateEmployeeProfileChanges` | `/hr/workforce-core/employee-profile-changes` | `{ resource: "EmployeeProfileChange", action: "Read" }` | `EmployeeProfileChangeController.cs` |
| 6 | Penempatan Organisasi | `corporateOrganizationAssignments` | `/hr/workforce-core/organization-assignments` | `{ resource: "WfpOrganizationAssignment", action: "ReadAll" }` | `WorkforceCoreCrossEmployeeController.cs` |
| 7 | Penempatan Jabatan | `corporatePositionAssignments` | `/hr/workforce-core/position-assignments` | `{ resource: "WfpPositionAssignment", action: "ReadAll" }` | `WorkforceCoreCrossEmployeeController.cs` |
| 8 | Riwayat Kepegawaian | `corporateEmploymentHistories` | `/hr/workforce-core/employment-histories` | `{ resource: "WfpEmploymentHistory", action: "ReadAll" }` | `WorkforceCoreCrossEmployeeController.cs` |
| 9 | Batch Data Awal | `corporateInitialPlacementBatches` | `/hr/workforce-core/initial-batches` | `{ resource: "HrdInitialPlacementBatch", action: "Read" }` | `HrdInitialPlacementBatchController.cs` |

#### C. Pemberhentian & Pensiun (6 butir)
| No | Label | Key | Pathname | `requiredPermission` target | Sumber controller backend |
| ---: | --- | --- | --- | --- | --- |
| 1 | Pengunduran Diri | `corporateResignations` | `/hr/separation/resignations` | `{ resource: "ResignationRequest", action: "Read" }` | `ResignationController.cs` |
| 2 | Pensiun | `corporateRetirements` | `/hr/separation/retirements` | `{ resource: "RetirementAdministration", action: "Read" }` | `RetirementController.cs` |
| 3 | Akhir Kontrak | `corporateContractExpiry` | `/hr/separation/contract-expiry` | `{ resource: "WorkforceEmploymentContract", action: "Read" }` | `HrdEmploymentContractController.cs` (`contract-expiry-view.jsx`) |
| 4 | Pemberhentian | `corporateTerminations` | `/hr/separation/terminations` | `{ resource: "EmployeeSeparation", action: "Read" }` | `EmployeeSeparationController.cs` |
| 5 | Serah Terima | `corporateHandovers` | `/hr/separation/handovers` | `{ resource: "OffboardingHandoverAdministration", action: "Read" }` | `OffboardingHandoverController.cs` |
| 6 | Clearance | `corporateExitClearance` | `/hr/separation/exit-clearance` | `[{ resource: "MultiDeptExitClearance", action: "Read" }, { resource: "ExitClearance", action: "Read" }]` | `ExitClearanceController.cs` + `exit-clearance-matrix-view.jsx` |

#### D. Master Data SDM (55 butir — seluruhnya `action: "Read"` kecuali *Atasan Langsung*)
| Subgrup | Pemetaan `key` → `{ resource, action: "Read" }` (kecuali disebut lain) |
| --- | --- |
| Organisasi & Struktur Jabatan (`12`) | `corporateOrganizationUnit` → `OrganizationUnit:Read`; `corporateDepartment` → `Organization:Read`; `corporatePosition` → `Organization:Read`; `corporateLegalEntity` → `LegalEntity:Read`; `corporateHospitalSite` → `HospitalSite:Read`; `corporateWorkLocation` → `WorkLocation:Read`; `corporateCostCenter` → `CostCenter:Read`; `corporateJobFamily` → `JobFamily:Read`; `corporateJobLevel` → `JobLevel:Read`; `corporateEmployeeGrade` → `EmployeeGrade:Read`; `corporateJobDescription` → `JobDescription:Read`; `corporateOrganizationManagerAssignments` → `WfpManagerAssignment:ReadAll` |
| Data Karyawan & Perencanaan SDM (`4`) | `corporateEmployee` → `Employee:Read`; `corporateDoctor` → `Doctor:Read`; `corporateExternalUser` → `ExternalUser:Read`; `corporateWorkforceRequirement` → `WorkforceRequirement:Read` |
| Profesi, Kompetensi & Kredensial (`7`) | `corporateProfession` → `Profession:Read`; `corporateSpecialization` → `Specialization:Read`; `corporateCompetency` → `Competency:Read`; `corporateClinicalPrivilegeCatalog` → `ClinicalPrivilegeCatalog:Read`; `corporateCredentialingRequirement` → `CredentialingRequirement:Read`; `corporateCertificationType` → `CertificationType:Read`; `corporateLicenseType` → `LicenseType:Read` |
| Jadwal Kerja & Shift (`5`) | `corporateWorkCalendar` → `WorkCalendar:Read`; `corporateWorkSchedule` → `WorkSchedule:Read`; `corporateShift` → `Shift:Read`; `corporateShiftGroup` → `ShiftGroup:Read`; `corporateShiftPattern` → `ShiftPattern:Read` |
| Cuti & Lembur (`7`) | `corporateLeaveType` → `LeaveType:Read`; `corporateLeavePolicy` → `LeavePolicy:Read`; `corporateLeaveEntitlementPolicy` → `LeaveEntitlementPolicy:Read`; `corporateLeaveCarryForwardPolicy` → `LeaveCarryForwardPolicy:Read`; `corporateLeaveAdjustmentReason` → `LeaveAdjustmentReason:Read`; `corporateOvertimePolicy` → `OvertimePolicy:Read`; `corporateOvertimeRate` → `OvertimeRate:Read` |
| Benefit Karyawan (`3`) | `corporateBenefitType` → `BenefitType:Read`; `corporateBenefitPlan` → `BenefitPlan:Read`; `corporateBenefitEligibilityRule` → `BenefitEligibilityRule:Read` |
| Kinerja & KPI (`4`) | `corporatePerformanceCycle` → `PerformanceCycle:Read`; `corporateKpiCatalog` → `KpiCatalog:Read`; `corporatePerformanceTemplate` → `PerformanceTemplate:Read`; `corporatePerformanceRatingScale` → `PerformanceRatingScale:Read` |
| Pelatihan & Pengembangan (`3`) | `corporateTrainingCategory` → `TrainingCategory:Read`; `corporateTrainingCatalog` → `TrainingCatalog:Read`; `corporateMandatoryTrainingRule` → `MandatoryTrainingRule:Read` |
| Disiplin & Hubungan Kerja (`4`) | `corporateViolationType` → `ViolationType:Read`; `corporateSanctionType` → `SanctionType:Read`; `corporateDisciplinaryActionType` → `DisciplinaryActionType:Read`; `corporateEmployeeRelationCaseType` → `EmployeeRelationCaseType:Read` |
| Workflow & Persetujuan (`6`) | `corporateWorkflowDefinition` → `WorkflowDefinition:Read`; `corporateWorkflowStep` → `WorkflowStep:Read`; `corporateApprovalMatrix` → `ApprovalMatrix:Read`; `corporateApprovalDelegationPolicy` → `ApprovalDelegationPolicy:Read`; `corporateRequestReason` → `RequestReason:Read`; `corporateRejectionReason` → `RejectionReason:Read` |

#### E. Pengadaan Barang & Jasa (1 butir) & Dashboard Utama (`/`)
| Bagian | Perubahan | Izin / Syarat Tampil |
| --- | --- | --- |
| `corporateProcurementApprovalInbox` (`/corporate/procurement/approvals/inbox`) | Ganti `requiredPermission` dari `{ resource: "ApprovalInbox", action: "Read" }` menjadi izin modul Pengadaan (`ProcurementRequest`, `ProcurementRequestVerification`, `ProcurementNeedPlan`, `ProcurementBudget`, `ProcurementPackagePlan` aksi `Read`) | Tampil hanya bagi pemegang hak Pengadaan. |
| Dashboard Utama (`/`) | Cek `useEffectivePermissions()` sebelum memanggil `fetchHomeDashboardSnapshot`; sumber Kiosk hanya dipanggil bila `unrestrictedAccess === true` atau role `SuperAdmin`/`Administrator`/`Kiosk` | Non-Kiosk menampilkan panel pintasan peran (`HrdWorkforceDashboard:Read`, `WorkflowInboxInstance:ViewInbox`, `ApprovalInbox:Read`, dan pintasan Profil/Layanan Mandiri) tanpa alert `403`. |

### 5.3 Aturan tampil

| Kondisi | Yang terjadi |
| --- | --- |
| Daftar izin belum selesai dimuat atau gagal (`UNKNOWN`) | Seluruh butir ber-`requiredPermission` disembunyikan; Beranda `/` menunggu `permissionsLoaded` sebelum memutuskan apakah perlu memuat endpoint Kiosk. |
| Akun pegawai biasa tanpa izin SDM (**Novi**) | Seluruh `198` butir di bawah *Sumber Daya Manusia* serta *Manajemen Role* disembunyikan dari sidebar; membuka URL langsung tetap dijaga `403` dan `AccessDeniedGate`. |
| Akun Kepala Ruangan (**Endang**) | Hanya melihat subgrup *Kontrak & Masa Kerja* (4 butir) di bawah *Sumber Daya Manusia*; di Beranda `/` tampil panel ringkasan aman + pintasan ke *Perjanjian Kerja* / *Masa Percobaan* / *Side Profil* tanpa banner merah `403`. |
| Akun SDM (**Hartini**, **Yudha**) | Melihat menu SDM lengkap sesuai peran masing-masing; tidak lagi melihat *Pengadaan Barang & Jasa* atau *Bank Darah*; di Beranda `/` melihat pintasan ke *Dashboard SDM* dan *Kotak Masuk* (Hartini) tanpa kartu `0` dan banner kuning. |
| Akun **SuperAdmin** | Tetap melihat seluruh `198` butir SDM, seluruh menu aplikasi, dan grafik/kartu Beranda `/` (`Dokter`, `Pasien`, `Poliklinik`, `Registrasi Pasien`) seperti semula. |

---

## 6. File yang dibuat dan diubah

Backend **tidak mengalami perubahan source C# maupun skema database**. Pembaruan di sisi backend hanya berupa pengisian kebijakan `SysAccessPolicy` di DB dev `QuilvianNewDevHamzah` melalui API resmi *Role Access* menggunakan skrip utilitas lokal di folder `test-with-agy/scripts/` (folder yang di-ignore Git).

### 6.1 File baru

| File | Isi |
| --- | --- |
| `QuilvianSystemFrontendDev/test-with-agy/scripts/sdm-1126-sync-role-policies.mjs` | Skrip idempoten lokal (di luar Git) untuk menyinkronkan izin bersih `Kepala Bagian SDM` dan `Staf Administrasi Kepegawaian` lewat `GET /resources/structured` + `POST /policies` dan memverifikasi hasilnya. |

### 6.2 File yang diubah

| File | Perubahan |
| --- | --- |
| `QuilvianSystemFrontendDev/src/utils/menu-sidebar/menu-items.jsx` | Memasang `requiredPermission` pada 70 leaf SDM yang belum bergerbang + `administratorRoleAccess`, serta mengganti `requiredPermission` pada `corporateProcurementApprovalInbox`. |
| `QuilvianSystemFrontendDev/src/lib/context/home/home-dashboard-context.jsx` | Memeriksa hak akses/peran (`unrestrictedAccess` / `KioskRead`) sebelum menembak 5 endpoint Kiosk/Klinis, dan mengekspor status `clinicalDashboardAllowed`. |
| `QuilvianSystemFrontendDev/src/components/view/home/dashboard-home.jsx` | Menampilkan kartu klinis hanya ketika `clinicalDashboardAllowed === true`; bila tidak, menampilkan panel Pintasan Kerja sesuai izin efektif (`useEffectivePermissions`) memakai `InformationAlert` dan `BaseButton`. |
| `QuilvianSystemFrontendDev/tests/unit/hr-master-data-sidebar-grouping.test.mjs` | Menyesuaikan `HR-15` (yang menguji dua pintu kompetensi `Competency:Read` dan `CompetencyMaster:Read`) serta memperluas `HR-16` agar memverifikasi seluruh `198` leaf SDM tertutup saat izin nol (`denied`). |
| `QuilvianSystemFrontendDev/tests/unit/home-dashboard-access.test.mjs` *(opsional)* | Unit test baru (jika diperlukan saat implementasi) untuk memastikan sumber Kiosk tidak dipanggil bagi non-Kiosk / non-SuperAdmin. |
| `NewQuilvianSystemBackend/docs/module-blueprints/human-resource/docs/AKUN_SDM_DATABASE_DEV.md` | Memperbarui ringkasan jumlah izin terbaru pada Bagian 3 setelah kebijakan `Kepala Bagian SDM` dan `Staf Administrasi Kepegawaian` ditata ulang. |

---

## 7. Urutan pengerjaan

### Fase 1 — Penataan Kebijakan Izin DB Dev via API Resmi (`Role Access`)
1. Buat skrip idempoten `sdm-1126-sync-role-policies.mjs` yang membaca katalog aksi resmi dari backend dan membentuk paket izin bersih untuk `Kepala Bagian SDM` (Hartini) dan `Staf Administrasi Kepegawaian` (Yudha) sesuai tabel bagian 5.1.
2. Jalankan skrip untuk menyimpan kebijakan ke `QuilvianNewDevHamzah` lewat `POST /api/v1/administrator/setting/role-access/policies`.
3. Verifikasi via `GET /v1/auth/permissions` untuk keempat akun (`hartini`, `yudha`, `endang`, `novi`) memastikan izin SDM lengkap, izin non-SDM warisan bersih (`0` izin Bank Darah/CRM/Procurement), dan pemisahan *maker-checker* tetap berlaku.
4. Perbarui angka total izin pada [AKUN_SDM_DATABASE_DEV.md](./AKUN_SDM_DATABASE_DEV.md).

#### Administrator / Setting / Role Access
Base URL: `api/v1/administrator/setting/role-access`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/resources/structured` | Mengambil katalog modul, controller, dan action aktif untuk menyusun daftar izin target | `RoleAccess : Read` |
| `GET` | `/policies?departmentId=&positionId=` | Membaca kebijakan izin saat ini sebelum penulisan (*backup* / pembandingan) | `RoleAccess : Read` |
| `POST` | `/policies` | Menyimpan paket izin final per pasangan Departemen × Jabatan (`Kepala Bagian SDM`, `Staf Adm. Kepegawaian`) | `RoleAccess : Update` |

#### Auth / Effective Permissions
Base URL: `api/v1/auth`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/permissions` | Memverifikasi daftar `Resource:Action` efektif dan nilai `unrestrictedAccess` sesudah penyimpanan policy | Terautentikasi (`[Authorize]`) |

### Fase 2 — Pemasangan `requiredPermission` Sidebar & Pemisahan Pengadaan
1. Pasang `requiredPermission` pada `administratorRoleAccess` dan 70 butir leaf SDM pada `menu-items.jsx` sesuai tabel bagian 5.2 (tanpa mengubah label, `key`, `pathname`, maupun struktur folder).
2. Ganti `requiredPermission` pada `corporateProcurementApprovalInbox` menjadi izin modul Pengadaan.
3. Sesuaikan unit test pada `hr-master-data-sidebar-grouping.test.mjs` (`HR-15`, `HR-16`), lalu jalankan `npm run test:unit`.

### Fase 3 — Penyesuaian Beranda (`/`) untuk Akun Non-Klinis & Uji Browser Menyeluruh
1. Perbarui `home-dashboard-context.jsx` dan `dashboard-home.jsx` agar tidak menembak endpoint Kiosk untuk akun non-klinis dan menampilkan pintasan kerja sesuai izin.
2. Jalankan `npm run lint:errors`, `check-css-usage.mjs`, `npm run test:unit`, dan `npm run build`.
3. Uji langsung di browser Chrome headed dengan akun `hartini`, `yudha`, `endang`, `novi`, dan `superadmin`, pada viewport desktop dan ponsel `390 px`, serta pastikan `0` error konsol dan `0` respons `403` tak diinginkan saat login normal.

---

## 8. Testing

| No | Pemeriksaan | Hasil yang diharapkan |
| ---: | --- | --- |
| 1 | ESLint file yang berubah dibanding `HEAD` | `0` error baru |
| 2 | Pemeriksa CSS (`check-css-usage.mjs`) pada berkas JSX yang diubah | `0` temuan pelanggaran token/Tailwind |
| 3 | Unit test penuh (`npm run test:unit`) dibanding baseline `HEAD` | Seluruh test lulus (`HR-13`, `HR-15`, `HR-16`, `menu-permission-filter`, `self-service-menu-dedup`, `approval-inbox-legacy-actions`) |
| 4 | `npm run build` | Berhasil tanpa error (`exit 0`) |
| 5 | Uji browser (headed) — **Hartini & Yudha** | Seluruh modul kerja SDM tampil sesuai peran masing-masing; *Bank Darah* dan *Pengadaan Barang & Jasa* tidak muncul; Beranda `/` tampil bersih tanpa `403` dan tanpa kartu `0`; Side Profil kanan berfungsi |
| 6 | Uji browser (headed) — **Endang & Novi** | **Novi** tidak melihat menu *Sumber Daya Manusia* maupun *Manajemen Role* di sidebar; **Endang** hanya melihat *Kontrak & Masa Kerja*; Beranda `/` tidak menampilkan banner merah/kuning `403` |
| 7 | Uji browser (headed) — **SuperAdmin** | Seluruh menu termasuk SDM lengkap dan angka Beranda (`Dokter`, `Pasien`, `Poliklinik`) tetap tampil normal |
| 8 | Lebar ponsel `390 px` | Sidebar drawer dan Beranda `/` bebas scroll horizontal (`scrollWidth <= clientWidth`) |
| 9 | Regresi komponen bersama (`filterMenuItemsByPermission`, `useEffectivePermissions`, `ProcurementApprovalInboxView`) | Filter menu Bank Darah, Side Profil, dan layar Antrean Persetujuan Pengadaan tetap bekerja normal |

---

## 9. Di luar cakupan

| Hal | Alasan |
| --- | --- |
| Mengubah izin jabatan lama `Manajer HR` dan `Staff HR` | Dipakai oleh 21 akun existing ([FE-HRD-409.md](../task/report/frontend/FE-HRD-409.md)); pengubahan butuh keputusan pemilik tersendiri agar tidak memengaruhi pengguna lain. |
| Menambahkan `requiredPermission` ke modul di luar SDM (*Administrator → Master Data*, *Akuntansi*, *Logistik*, *Pelayanan Kesehatan*) | Menyentuh puluhan modul dan unit test di luar cakupan SDM; dikerjakan terpisah oleh pemilik modul masing-masing. |
| Mengubah kontrak backend atau policy `KioskReadPolicy` | Melonggarkan `KioskReadPolicy` di backend berisiko membuka data pasien/dokter ke seluruh pegawai non-klinis; penanganan cukup di frontend + `RoleAccess` resmi. |
| Menjalankan migration database atau mengubah kode C# backend | Tidak ada perubahan tabel, DTO, ataupun controller backend yang dibutuhkan untuk rencana ini. |

---

## 10. Hasil implementasi

Implementasi dan verifikasi end-to-end selesai dijalankan pada **26 November 2026**:

1. **Fase 1 (Kebijakan Izin DB Dev via API Resmi):**
   - Skrip [sdm-1126-sync-role-policies.mjs](../../../../QuilvianSystemFrontendDev/test-with-agy/scripts/sdm-1126-sync-role-policies.mjs) dijalankan via `POST /api/v1/administrator/setting/role-access/policies`:
     - **Kepala Bagian SDM (Hartini):** `1.598` → **`814`** izin (seluruh modul kerja SDM lengkap + `HrdWorkforceDashboard`, `WorkflowInboxInstance:ViewInbox`, `ApprovalInbox`, `ViewAmount`, ESS, `Bank:Read`, `SalaryStructure:Read`, `PayrollDataReadiness:Read,Import`; `0` izin warisan `Blood*`, `PatientMembership`, `Procurement*`, `RoleAccess`, `MedicalDirectorApprove`, maupun `ViewClinicalDetail`).
     - **Staf Administrasi Kepegawaian (Yudha):** `1.110` → **`664`** izin (operator/pembuat modul SDM + ESS & referensi form tanpa `ApprovalInbox`, `WorkflowInboxInstance:ViewInbox`, `ViewAmount`, penutupan periode, dan tanpa `Delete` pada master kritikal).
     - **Endang** (`13` izin) dan **Novi** (`34` izin), serta jabatan lama `Manajer HR` & `Staff HR` tidak diubah.
   - Ringkasan total izin pada [AKUN_SDM_DATABASE_DEV.md](./AKUN_SDM_DATABASE_DEV.md) diperbarui.
2. **Fase 2 (Gerbang Sidebar & Pemisahan Pengadaan):**
   - Pada [menu-items.jsx](../../../../QuilvianSystemFrontendDev/src/utils/menu-sidebar/menu-items.jsx), seluruh `198/198` butir leaf SDM kini bergerbang `requiredPermission` (`70` leaf SDM baru bergerbang + `128` leaf sebelumnya), ditambah `administratorRoleAccess` (`RoleAccess:Read`).
   - `corporateProcurementApprovalInbox` dipindah dari `ApprovalInbox:Read` ke izin baca modul Pengadaan (`ProcurementRequest`, `ProcurementRequestVerification`, `ProcurementNeedPlan`, `ProcurementBudget`, `ProcurementPackagePlan`).
   - Penyesuaian minor di luar daftar awal berkas: pada `Yudha`, folder *Kotak Masuk Persetujuan Terpadu (V2)* tetap tampil karena hanya berisi leaf *Pendelegasian Wewenang (Plh)* (`WorkflowDelegation:ViewOwn` / `CreateDelegation`), sedangkan leaf penyetuju (*Antrean Kotak Masuk Atasan* dan *Kotak Masuk Persetujuan*) tertutup rapat.
3. **Fase 3 (Penjaga Beranda `/` Non-Klinis):**
   - Ditambahkan utilitas murni [home-dashboard.utils.js](../../../../QuilvianSystemFrontendDev/src/utils/home/home-dashboard.utils.js) (`resolveHomeDashboardAccessState`, `hasClinicalHomeDashboardAccess`, `getPermittedHomeShortcuts`), CSS Module berbasis token [home-dashboard-shortcuts.module.css](../../../../QuilvianSystemFrontendDev/src/style/home/dashboard/home-dashboard-shortcuts.module.css), dan unit test baru [home-dashboard-access.test.mjs](../../../../QuilvianSystemFrontendDev/tests/unit/home-dashboard-access.test.mjs).
   - Di [home-dashboard-context.jsx](../../../../QuilvianSystemFrontendDev/src/lib/context/home/home-dashboard-context.jsx) dan [dashboard-home.jsx](../../../../QuilvianSystemFrontendDev/src/components/view/home/dashboard-home.jsx), permintaan ke 5 endpoint klinis/Kiosk ditahan sampai `permissionsLoaded === true` dan hanya dijalankan untuk akun berwenang (`unrestrictedAccess` / `isSuperAdmin` / `isKioskAccount` / role `SuperAdmin`/`Administrator`/`Kiosk`/`KioskDevice`/`SystemUser`).
   - Penyempurnaan UX setelah uji visual: kartu `Presensi Saya` disederhanakan (`excludeIfPresentKey: "selfServicePortal"`) agar tidak menumpuk saat portal *Layanan Mandiri Pegawai* sudah hadir dan seluruh kartu `Hartini` (`6` kartu) / `Yudha` (`5` kartu) tampil seimbang dalam satu baris di atas footer; kelas jarak `mb-5 pb-4` ditambahkan agar catatan panduan tidak tertutup footer lengket.
4. **Hasil Verifikasi Akhir (setelah suntingan terakhir):**
   - **ESLint (`npx eslint --max-warnings=0`):** `0` error dan `0` warning pada seluruh `6` berkas JS/JSX yang diubah & ditambahkan (dibanding `HEAD`: turun dari `1` warning menjadi `0`).
   - **`check-css-usage.mjs`:** `0` temuan pada `home-dashboard.utils.js`, `menu-items.jsx`, `home-dashboard-context.jsx`, serta `home-dashboard-shortcuts.module.css`; pada `dashboard-home.jsx` hanya tersisa `1` temuan pra-ada di luar perubahan (`dashboard-hero__stats`), `0` temuan baru.
   - **Unit test terfokus (`home-dashboard-access`, `hr-master-data-sidebar-grouping`, `self-service-menu-dedup`):** `23/23 PASS` (`0 FAIL`).
   - **Unit test penuh vs `HEAD` (`baseline-unit-test.ps1 -SkipHead`):** `5.561` test (`5.412 pass`, `149 fail` vs `HEAD 5.216 pass / 186 fail` — `39` test lama kini lulus, `0` regresi frontend; `2` `calon regresi` berasal dari kontrak file backend modul lain yang berstatus `# SKIP` di salinan temp `HEAD`). Note pra-ada luar perubahan: `M0` Bank Darah juga gagal identik pada `HEAD`.
   - **Build produksi (`npm run build`):** `exit 0` (`✓ Compiled successfully in 117s`, `941/941` static pages, standalone siap).
   - **Uji Browser Chrome Headed (`sdm1126-01-menu-dashboard.mjs`, port CDP `9345`):** **`11/11 PASS, 0 FAIL`** mencakup Beranda & sidebar `hartini`, klik `/hr/dashboard`, Side Profil ESS, `yudha`, `endang`, `novi`, Beranda `superadmin`, serta regresi `/corporate/procurement/approvals/inbox` (`0` HTTP 5xx, `0` unexpected 403, `0` pageErrors). *(Catatan observasi non-blokir pra-ada: klik Side Profil Hartini memicu `404 GET /UserActive/{id}` bawaan komponen profil).*
   - **Lebar ponsel `390 px` (`sdm1126-diag-mobile.mjs`):** **`offenders: []`** (`LULUS`) untuk `hartini` (`vw: 375, rootScroll: 266, rootClient: 266`) maupun `superadmin` (`vw: 375, rootScroll: 278, rootClient: 266`).
