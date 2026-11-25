# Planning — Pemindahan Menu Ruang Kerja Saya ke Side Menu (Layanan Mandiri Langsung ke Halaman)

| Field | Nilai |
| --- | --- |
| Tanggal | 25 November 2026 |
| Modul | Human Resource — Layanan Mandiri Pegawai (ESS) |
| Repository terdampak | `QuilvianSystemFrontendDev` (frontend saja; backend tidak diubah) |
| Status | Selesai diimplementasikan dan terverifikasi (25 November 2026) |
| Alur kerja | Analisis → Planning → Implementasi → Testing |
| Dokumen terkait | `PLANNING_RUANG_KERJA_SAYA_LAYANAN_MANDIRI.md` (workspace `/self-services` yang digantikan), `PLANNING_MENU_SIDEBAR_TANPA_DUPLIKAT_LAYANAN_MANDIRI.md`, `PLANNING_IMPLEMENTASI_STRUKTUR_MENU_SDM_QUILVIAN_V2.md` |

---

## 1. Latar belakang

Sebelumnya, seluruh layanan mandiri pegawai disatukan ke dalam satu halaman workspace **Ruang Kerja Saya** (`/self-services`), yang memuat navigasi kiri internal **LAYANAN SAYA** (Beranda + 8 kelompok layanan), tab sekunder di tengah layar, serta popover **Ringkasan & Aksi**. Panel profil kanan juga diarahkan ke parameter `?section=&tab=` di dalam workspace tersebut.

Sesuai arahan pemilik sistem (25 November 2026), pola workspace satu halaman tersebut **tidak digunakan lagi**: seluruh menu yang ada di dalam navigasi workspace dipindahkan ke **side menu** utama agar setiap butir menu yang diklik langsung membuka halaman Next.js App Router masing-masing tanpa shell workspace dan tanpa tab bertumpuk.

Kondisi saat ini berdasarkan penelusuran kode:

| Pintu / Komponen | Lokasi source (`QuilvianSystemFrontendDev`) | Isi | Kondisi |
| --- | --- | --- | --- |
| Workspace Ruang Kerja Saya | `src/app/self-services/page.jsx`, `src/components/view/self-services/workspace/*` | 1 halaman berisi 9 bagian (`Beranda` + 27 tab) lewat `?section=&tab=` | Ingin dilepas/tidak dipakai |
| Side menu kiri utama | `src/utils/menu-sidebar/menu-items.jsx` (baris 72–77) | Hanya 1 butir `Ruang Kerja Saya` → `/self-services` | Baru satu pintu ke workspace; 28 layanan belum tampil di sidebar |
| Panel profil kanan | `src/components/view/settings/sidebar-profile/user-profile-sidebar.jsx` (baris 62–73, 799–902, 1096–1101) | Tombol `Ruang Kerja Saya`, subjudul `"Dibuka di Ruang Kerja Saya"`, serta 7 tombol menuju `/self-services?section=...&tab=...` | Masih mengarahkan pengguna ke workspace |
| Rute halaman App Router mandiri | `src/app/` | 23 rute dari 28 layanan sudah punya `page.jsx`; 5 layanan baru hasil workspace belum punya `page.jsx` | Perlu dibuatkan 5 `page.jsx` baru agar klik di sidebar/profil tidak 404 |
| Alamat portal lama | `src/app/(dashboard)/hrd/self-service/page.jsx`, `src/app/(dashboard)/corporate/hrd/self-service/page.jsx` | Keduanya saat ini memanggil `redirect("/self-services")` | Perlu dipulihkan ke halaman Beranda mandiri (`UnifiedEssPortalView`) |

### 1.1 Temuan analisis sebelum pemindahan

| # | Jenis | Temuan | Bukti (`path:baris`) | Dampak bila tidak ditangani |
| ---: | --- | --- | --- | --- |
| 1 | Wajib dibuat | 5 layar baru yang dibuat untuk workspace belum punya file `page.jsx` mandiri di App Router | `cek-rute.ps1`: `/self-services/human-resource/attendance-dispensation`, `schedule-change`, `leave-cancellation`, `leave-recall`, `resignation` berstatus `TIDAK` | Klik menu baru dari side menu/panel profil akan menghasilkan 404 |
| 2 | Kurang aman | Kelima view baru tersebut (`AttendanceDispensationEmployeeView`, `ScheduleChangeEmployeeView`, `LeaveCancellationEmployeeView`, `LeaveRecallEmployeeView`, `ResignationEmployeeView`) tidak mengecek izin `Read` sebelum memanggil API karena sebelumnya bergantung pada guard tab workspace | `attendance-dispensation-employee-view.jsx:110-135`, `schedule-change-employee-view.jsx:82-111`, `leave-cancellation-employee-view.jsx:79-101`, `leave-recall-employee-view.jsx:46-66`, `resignation-employee-view.jsx:75-101` | Saat URL dibuka langsung oleh akun tanpa izin `My*`, halaman langsung menembak GET dan menampilkan error 403 |
| 3 | Putus alur | Tautan internal masih menunjuk ke `SELF_SERVICE_SHORTCUTS` (`/self-services?section=...&tab=...`) atau menyebut "tab Cuti" / "Ruang Kerja Saya" | `leave-employee-view.jsx:333,345`, `overtime-constants.js:105`, `approval-constants.js:14,16`, `use-payroll-payslip-detail.js:83`, `leave-cancellation-employee-view.jsx:288`, `leave-recall-employee-view.jsx:204` | Tombol `+ Ajukan Cuti`, `Kembali ke Cuti Saya`, `Gunakan untuk Cuti` (CTO), `Kembali` di detail slip gaji, dan `Riwayat Permohonan` di revisi akan memantul ke redirect atau alamat salah |
| 4 | Mismatch izin | Konfigurasi workspace memakai `MyOvertime:Read` untuk tab `overtime`, padahal view yang dipasang (`OvertimeEssView`) memanggil endpoint `OvertimePlan:ViewSelf/Create` dan `OvertimeRealizationEss:View/Create` | `self-service-workspace-constants.js:65`, `overtime-ess-view.jsx:73-80`, `OvertimePlanEssController.cs:37,69`, `OvertimeRealizationEssController.cs:37,73` | Jika menu sidebar memakai `MyOvertime:Read`, pegawai yang berhak lembur (`OvertimePlan:ViewSelf`) bisa tidak melihat menu, sedangkan pemilik `MyOvertime:Read` mendapat peringatan akses terbatas |
| 5 | Penjaga fitur BPJS | Mekanisme penyembunyian Klaim COB saat fitur BPJS nonaktif (`adaptPayrollMenuItems`) mencocokkan `key: "corporateMyCobClaims"` atau `pathname: "/ess/claims/cob-submission"` | `adapt-payroll-menu-items.js:11-17`, `tests/unit/bpjs-feature-deactivation.test.mjs:88-113` | Menggunakan key/pathname tersebut membuat Klaim COB otomatis tersembunyi bila BPJS dimatikan dan menjaga test BPJS tetap lulus |
| 6 | Duplikasi menu | Seluruh 28 rute tujuan **belum ada** di `menu-items.jsx` saat ini; menu SDM untuk atasan/admin tetap terpisah di rute `/corporate/hrd/*`, `/hr/*`, dan `/hrd/relations/grievance` | Verifikasi `menu-items.jsx` terhadap 28 rute target; `PLANNING_MENU_SIDEBAR_TANPA_DUPLIKAT_LAYANAN_MANDIRI.md` | Tidak ada konflik `pathname` ganda dan tidak melanggar aturan `PLANNING_IMPLEMENTASI_STRUKTUR_MENU_SDM_QUILVIAN_V2.md` (ESS tetap di luar SDM) |
| 7 | Unit test terdampak | Beberapa unit test secara eksplisit menguji struktur workspace (`/self-services` dan popover) atau mengasumsikan `user-profile-sidebar.jsx` menautkan `SELF_SERVICE_SHORTCUTS` | `tests/unit/employee-self-service-profile-change.test.mjs:61-111`, `tests/unit/self-service-menu-dedup.test.mjs:14-17`, `tests/unit/self-service-workspace-summary-popover.test.mjs` | Test perlu diperbarui mengikuti arsitektur side menu tanpa workspace |

### 1.2 Contoh di rumah sakit

Seorang perawat shift malam (Ns. Novi) atau kepala bagian SDM (Ibu Hartini) ingin memeriksa sisa saldo cuti tahunannya lalu mengajukan cuti atau koreksi presensi setelah jaga malam.

- **Hari ini:** Saat membuka aplikasi, ia mengklik **Ruang Kerja Saya** di sidebar atau membuka panel profil kanan (yang masih menampilkan tombol *Ruang Kerja Saya* dan subjudul *Dibuka di Ruang Kerja Saya*). Setelah masuk `/self-services`, ia masih harus menavigasi menu kiri kedua (**LAYANAN SAYA**) dan bar tab sekunder di area tengah yang menyempit, sementara kartu *Hero* dipadatkan lewat CSS khusus workspace.
- **Sesudah:** Workspace ditiadakan. Pada side menu kiri utama langsung tersedia menu **Layanan Saya** berisi kelompok yang sama (*Beranda*, *Kehadiran & Jadwal*, *Cuti & Lembur*, *Gaji & Klaim*, *Data Kepegawaian*, *Kinerja & Diklat*, *Praktik Klinis*, *Pengaduan & K3*, dan *Riwayat Permohonan*). Mengklik **Cuti Saya** langsung membuka halaman `/self-services/human-resource/leave` secara penuh; mengklik **Pengajuan Cuti** di panel profil kanan langsung membuka `/self-services/human-resource/leave?form=baru`.

---

## 2. Tujuan

Menghilangkan halaman workspace **Ruang Kerja Saya** (`/self-services`) dan memindahkan seluruh menu layanan mandiri ke dalam **side menu** kiri utama serta mengarahkan tombol panel profil kanan langsung ke rute halaman Next.js masing-masing.

---

## 3. Gambaran layar

### 3.1 Side menu kiri dan halaman penuh (tanpa workspace)

```text
┌──────────────────────────────┬─────────────────────────────────────────────────────┐
│ SIDE MENU KIRI               │ HALAMAN PENUH (contoh: /self-services/human-resource/ │
│                              │ leave atau /self-services/human-resource/attendance-  │
│ [Cari menu...]               │ dispensation)                                       │
│                              │                                                     │
│ ▸ Dashboard                  │ ┌─────────────────────────────────────────────────┐ │
│ ▾ Layanan Saya               │ │ Hero (judul halaman penuh + tombol aksi utama)  │ │
│   • Beranda                  │ └─────────────────────────────────────────────────┘ │
│   ▸ Kehadiran & Jadwal       │ ┌──────────────┐ ┌──────────────┐ ┌──────────────┐  │
│   ▾ Cuti & Lembur            │ │ Kartu ringkas│ │ Kartu ringkas│ │ Kartu ringkas│  │
│     • Cuti Saya       [AKTIF]│ └──────────────┘ └──────────────┘ └──────────────┘  │
│     • Pembatalan Cuti        │                                                     │
│     • Panggilan Kembali Cuti │ ┌─────────────────────────────────────────────────┐ │
│     • Lembur Saya            │ │ Tabel DataTable lebar penuh (tanpa tab workspace│ │
│   ▸ Gaji & Klaim             │ │ maupun panel kiri kedua)                        │ │
│   ▸ Data Kepegawaian         │ └─────────────────────────────────────────────────┘ │
│   ▸ Kinerja & Diklat         │                                                     │
│   ▸ Praktik Klinis           │                                                     │
│   ▸ Pengaduan & K3           │                                                     │
│   • Riwayat Permohonan       │                                                     │
│                              │                                                     │
│ ── Administrator ──          │                                                     │
│ ▸ Master Data                │                                                     │
│ ...                          │                                                     │
└──────────────────────────────┴─────────────────────────────────────────────────────┘
```

### 3.2 Panel profil kanan (`user-profile-sidebar.jsx`)

```text
┌─────────────────────────────────────────────┐
│  [Avatar] Hartini Sulistyowati, S.Psi., M.M.│
│  Departemen | Jabatan | Hadir | Pulang      │
├─────────────────────────────────────────────┤
│  Kepegawaian (Layanan profil & absensi user)│
│   • Kelola Data Saya    → /self-services/human-resource/employee/profile-changes
│   • Absensi             → /self-services/human-resource/attendance
│   • Profil Dokter       → (jika dokter)
│   • Ganti Password      → /settings/self-services-profile
│                                             │
│  Pengajuan (Pintasan pengajuan layanan...)  │
│   • Pengajuan Cuti      → /self-services/human-resource/leave?form=baru
│   • Pengajuan Lembur    → /hr/self-service/overtime
│   • Perubahan Jadwal    → /self-services/human-resource/schedule-change
│   • Tukar Shift         → /self-services/human-resource/schedule
│   • Pengajuan Resign    → /self-services/human-resource/resignation
└─────────────────────────────────────────────┘
```

*(Tombol **Ruang Kerja Saya** dihapus dan subjudul **"Dibuka di Ruang Kerja Saya"** diganti menjadi **"Pintasan pengajuan layanan mandiri"**.)*

---

## 4. Keputusan

Setiap baris berisi rekomendasi dan alternatifnya. Perubahan dari rekomendasi cukup disebutkan sebelum implementasi dimulai.

| No | Hal | Rekomendasi | Alternatif |
| ---: | --- | --- | --- |
| 1 | Struktur di side menu kiri (`menu-items.jsx`) | **1 menu induk `"Layanan Saya"`** tepat di bawah `Dashboard`, dengan `subMenu` berisi 9 kelompok/item (`Beranda`, 7 grup kategori, `Riwayat Permohonan`). Kelebihan: sidebar utama tetap rapi bagi admin/klinisi, otomatis tersembunyi bila seluruh anak tidak diizinkan, dan tetap berada di luar menu `Sumber Daya Manusia` (sesuai `PLANNING_IMPLEMENTASI_STRUKTUR_MENU_SDM_QUILVIAN_V2.md`). | Memakai pemisah `type: "label", label: "Layanan Saya"` lalu meletakkan kesembilan item langsung di level utama sidebar (mengurangi 1 klik awal, tetapi menambah 9 baris permanen di atas `Administrator`). |
| 2 | Nasib `/self-services` dan berkas Workspace | **`/self-services` dialihkan (`redirect("/hrd/self-service")`)** ke Beranda Layanan Saya agar tautan/bookmark lama tetap hidup tanpa menampilkan workspace. Seluruh berkas khusus workspace (`components/view/self-services/workspace/*`, `use-self-service-workspace.js`, `self-service-workspace-constants.js`, `self-service-workspace.module.css`, serta `self-service-workspace-summary-popover.test.mjs`) dihapus/dibersihkan sesudah konstanta rute dipindahkan ke `self-service-request-constants.js`. | Membiarkan berkas workspace tetap tersimpan di repo tanpa dipasang di route mana pun (menyisakan kode mati dan CSS/test yatim). |
| 3 | Halaman **Beranda** (`/hrd/self-service`) | Memulihkan `src/app/(dashboard)/hrd/self-service/page.jsx` untuk merender `UnifiedEssPortalView` (saldo cuti, tombol cepat pengajuan, dan 3 permohonan terakhir), serta mengalihkan `/corporate/hrd/self-service` dan `/self-services` ke `/hrd/self-service`. `QUICK_LINKS` di `UnifiedEssPortalView` diperbarui ke rute mandiri yang lengkap termasuk 5 rute baru. | Membuat halaman Beranda baru di `/self-services` tanpa shell workspace. |
| 4 | Tombol `Ruang Kerja Saya` di panel profil kanan | **Menghapus butir `Ruang Kerja Saya`** dari `serviceItems` di `user-profile-sidebar.jsx`, mengganti teks `"Dibuka di Ruang Kerja Saya"` menjadi `"Pintasan pengajuan layanan mandiri"`, dan mengarahkan semua 7 tombol langsung ke `pathname` halaman masing-masing. | Mengganti butir `Ruang Kerja Saya` menjadi `Beranda Layanan Saya` (`/hrd/self-service`). |
| 5 | 5 layar baru yang belum punya `page.jsx` | Membuat 5 `page.jsx` tipis di bawah `src/app/self-services/human-resource/`: `attendance-dispensation`, `schedule-change`, `leave-cancellation`, `leave-recall`, dan `resignation`. View yang sudah ada dipakai langsung (karena sudah memakai `<Hero>`, `administrator-region-page`, `DataTable`, dan `ConfirmModal`), ditambah **penjaga izin baca (`canRead`)** agar aman bila URL dibuka langsung tanpa izin. | Membuat 5 `page.jsx` tanpa menambah guard `canRead` di view-nya (akun tanpa izin yang membuka URL langsung akan melihat error 403). |
| 6 | Hak akses (`requiredPermission`) pada butir side menu | Mempertahankan cakupan izin seperti di workspace sebelumnya (butir `My*`, `WorkflowInboxInstance:ViewOwn`, dan 4 izin `Read` Diklat dijaga; butir terbuka tetap terbuka dan dijaga di dalam view), **dengan koreksi `Lembur Saya`** menjadi `[{ resource: "OvertimePlan", action: "ViewSelf" }, { resource: "OvertimePlan", action: "Create" }, { resource: "OvertimeRealizationEss", action: "View" }, { resource: "OvertimeRealizationEss", action: "Create" }]` agar sesuai endpoint aktual `OvertimeEssView`. | Menambahkan `requiredPermission` ketat pada seluruh 28 butir sekaligus (berisiko menyembunyikan menu bagi akun pegawai yang belum diberi izin spesifik di Akses Role). |
| 7 | Penjagaan Klaim COB (BPJS) | Memberi `key: "corporateMyCobClaims"` pada butir `Klaim Koordinasi Manfaat (COB)` (`/ess/claims/cob-submission`) di bawah `Gaji & Klaim` agar `adaptPayrollMenuItems` dan test `bpjs-feature-deactivation.test.mjs` tetap otomatis menyembunyikannya saat BPJS nonaktif. | Membuat key baru dan memodifikasi daftar `BPJS_ONLY_MENUS`. |
| 8 | Penggantian keputusan planning lama | Rencana ini menggantikan keputusan berbasis workspace pada `PLANNING_RUANG_KERJA_SAYA_LAYANAN_MANDIRI.md` dan penautan workspace pada `PLANNING_MENU_SIDEBAR_TANPA_DUPLIKAT_LAYANAN_MANDIRI.md`, sambil tetap mempertahankan penghapusan duplikat di dalam menu SDM (`corporateHumanResource`). | — |

---

## 5. Isi menu side menu (28 butir layanan mandiri)

Seluruhnya berada di bawah induk **Layanan Saya** (`key: "selfServices"`, ikon `RiUserLine`, posisi tepat di bawah `Dashboard`).

| No | Kelompok | Label di side menu | `key` | `pathname` tujuan | Status route | View / sumber layar | `requiredPermission` |
| ---: | --- | --- | --- | --- | --- | --- | --- |
| 1 | — | Beranda | `selfServiceHome` | `/hrd/self-service` | **Pulihkan** *(dari redirect ke workspace)* | `UnifiedEssPortalView` | — *(guard internal di view)* |
| 2 | Kehadiran & Jadwal | Presensi Saya | `selfServiceAttendance` | `/self-services/human-resource/attendance` | Ada | `AttendanceEmployeeView` | — *(core ESS `IsSystemOnly`)* |
| 3 | Kehadiran & Jadwal | Koreksi Presensi | `selfServiceAttendanceCorrection` | `/self-services/human-resource/attendance-correction` | Ada | `AttendanceCorrectionEmployeeView` | `MyAttendanceCorrection:Read` |
| 4 | Kehadiran & Jadwal | Dispensasi Presensi | `selfServiceAttendanceDispensation` | `/self-services/human-resource/attendance-dispensation` | **Baru** | `AttendanceDispensationEmployeeView` | `MyAttendanceDispensation:Read` |
| 5 | Kehadiran & Jadwal | Jadwal & Tukar Shift | `selfServiceSchedule` | `/self-services/human-resource/schedule` | Ada | `ScheduleEmployeeView` | `MySchedule:Read` \| `MyShiftSwap:Read` |
| 6 | Kehadiran & Jadwal | Perubahan Jadwal | `selfServiceScheduleChange` | `/self-services/human-resource/schedule-change` | **Baru** | `ScheduleChangeEmployeeView` | `MyScheduleChange:Read` |
| 7 | Cuti & Lembur | Cuti Saya | `selfServiceLeave` | `/self-services/human-resource/leave` | Ada | `LeaveEmployeeView` | `MyLeaveRequest:Read` |
| 8 | Cuti & Lembur | Pembatalan Cuti | `selfServiceLeaveCancellation` | `/self-services/human-resource/leave-cancellation` | **Baru** | `LeaveCancellationEmployeeView` | `MyLeaveCancellation:Read` |
| 9 | Cuti & Lembur | Panggilan Kembali Cuti | `selfServiceLeaveRecall` | `/self-services/human-resource/leave-recall` | **Baru** | `LeaveRecallEmployeeView` | `MyReturnToWork:Read` |
| 10 | Cuti & Lembur | Lembur Saya | `selfServiceOvertime` | `/hr/self-service/overtime` | Ada | `OvertimeEssView` | `OvertimePlan:ViewSelf/Create` \| `OvertimeRealizationEss:View/Create` |
| 11 | Gaji & Klaim | Slip Gaji Saya | `selfServicePayslips` | `/payroll/my-payslips` | Ada | `PayrollMyPayslipView` | `MyPayslip:Read` |
| 12 | Gaji & Klaim | Klaim Restitusi | `selfServiceRestitution` | `/ess/restitution-claims` | Ada | `EssRestitutionClaimsView` | — *(guard internal `RestitutionClaim:ViewOwn/Submit`)* |
| 13 | Gaji & Klaim | Klaim Koordinasi Manfaat (COB) | `corporateMyCobClaims` | `/ess/claims/cob-submission` | Ada | `EssCobSubmissionView` (`<BpjsFeatureGuard>`) | — *(disembunyikan `adaptPayrollMenuItems` saat BPJS off)* |
| 14 | Data Kepegawaian | Kelola Data Saya | `selfServiceProfileChanges` | `/self-services/human-resource/employee/profile-changes` | Ada | `EmployeeProfileChangesSelfServiceView` | `MyProfileChange:Read` |
| 15 | Data Kepegawaian | Keluarga & Tanggungan | `selfServiceDependents` | `/ess/my-dependents` | Ada | `EssMyDependentsView` | — *(guard internal `BenefitEnrollment:ViewOwn`)* |
| 16 | Data Kepegawaian | Dokumen & PIN | `selfServiceDocuments` | `/portal/services/my-documents` | Ada | `PortalMyDocumentsView` | — |
| 17 | Data Kepegawaian | Surat & Layanan HR | `selfServiceServiceRequests` | `/portal/services/requests` | Ada | `PortalServiceRequestsView` | — *(endpoint `HrServiceEss`)* |
| 18 | Data Kepegawaian | Pengunduran Diri | `selfServiceResignation` | `/self-services/human-resource/resignation` | **Baru** | `ResignationEmployeeView` | `MyResignation:Read` |
| 19 | Kinerja & Diklat | Evaluasi Kinerja | `selfServiceAppraisals` | `/portal/performance/appraisals` | Ada | `PerformanceAppraisalsView` | — *(guard internal `PerformanceAppraisal:ViewOwn`)* |
| 20 | Kinerja & Diklat | Sanggah Nilai | `selfServiceDisputes` | `/portal/performance/disputes` | Ada | `PerformanceDisputesView` | — *(guard internal `PerformanceCalibration:SubmitDispute`)* |
| 21 | Kinerja & Diklat | Dasbor Pelatihan | `selfServiceLearning` | `/portal/learning/dashboard` | Ada | `LearningDashboardView` | `MandatoryHospitalTraining` \| `TrainingSession` \| `TrainingCertificate` \| `AnnualTrainingPlan` (`Read`) |
| 22 | Praktik Klinis | Kredensialing | `selfServiceCredentialing` | `/portal/credentialing/application` | Ada | `PortalCredentialingWizardView` | — *(guard internal `SpkRkkIssuance:CreateApplication`)* |
| 23 | Praktik Klinis | Rapor OPPE | `selfServiceOppe` | `/portal/performance/clinical-oppe` | Ada | `ClinicalOppeDashboardView` | — *(guard internal `OppeOngoingEvaluation:ViewEvaluation`)* |
| 24 | Praktik Klinis | Peer Review | `selfServicePeerReview` | `/portal/performance/peer-review-entry` | Ada | `PeerReviewEvaluationFormView` | — *(guard internal `OppeOngoingEvaluation:AssessPeerReview`)* |
| 25 | Praktik Klinis | Portofolio SKP | `selfServiceSkp` | `/portal/learning/skp-portfolio` | Ada | `SkpTrackerView` | — *(guard internal `PlataranSehatSkp:Create/Verify`)* |
| 26 | Pengaduan & K3 | Lapor Pajanan (CITO) | `selfServiceNeedleStick` | `/portal/health/panic` | Ada | `NeedleStickPanicButton` (`standalone`) | — *(guard internal `NeedleStickEmergency:ReportNeedleStickEmergency`)* |
| 27 | Pengaduan & K3 | Pengaduan | `selfServiceGrievance` | `/portal/grievance` | Ada | `RelationsGrievanceView` | — *(guard internal `EmployeeGrievance:ViewOwn/Submit`)* |
| 28 | — | Riwayat Permohonan | `selfServiceMyRequests` | `/hrd/self-service/my-requests` | Ada | `UnifiedEssMyRequestsView` | `WorkflowInboxInstance:ViewOwn` |

### 5.1 Aturan tampil

| Kondisi | Yang terjadi |
| --- | --- |
| Pegawai tidak punya `requiredPermission` suatu submenu | Submenu disembunyikan oleh `filterMenuItemsByPermission`. Jika semua anak dalam satu grup tersembunyi (misalnya tidak punya izin apa pun di *Cuti & Lembur*), grup induk ikut disembunyikan. |
| Fitur BPJS nonaktif di backend | `adaptPayrollMenuItems` menyembunyikan `corporateMyCobClaims` (`/ess/claims/cob-submission`), sedangkan rute langsung tetap diblokir `<BpjsFeatureGuard>`. |
| Pengguna membuka salah satu dari 5 URL baru langsung tanpa izin `Read` | View menampilkan pesan peringatan ramah (`InformationAlert` info/warning) dan tidak memanggil endpoint `GET` list. |
| Pengguna membuka bookmark lama `/self-services` atau `/corporate/hrd/self-service` | Dialihkan ke `/hrd/self-service` (Beranda Layanan Saya). |
| Pengguna membuka `/hrd/self-service/revisions/[id]` dan menekan *Kembali* atau selesai *resubmit* | Kembali ke `/hrd/self-service/my-requests`. |
| Pengguna membuka `/payroll/my-payslips/[id]` dan menekan *Kembali* (tanpa history back) | Kembali ke `/payroll/my-payslips`. |

### 5.2 Pemetaan tombol panel profil kanan (`user-profile-sidebar.jsx`)

| Bagian | Tombol | Tujuan baru |
| --- | --- | --- |
| Kepegawaian | ~~Ruang Kerja Saya~~ | **Dihapus** (sesuai Keputusan 4) |
| Kepegawaian | Kelola Data Saya | `/self-services/human-resource/employee/profile-changes` |
| Kepegawaian | Absensi | `/self-services/human-resource/attendance` |
| Kepegawaian | Profil Dokter | `/settings/dokter/detail-dokter/${doctorId}` *(tidak berubah)* |
| Kepegawaian | Ganti Password | `/settings/self-services-profile` *(tidak berubah)* |
| Pengajuan | Pengajuan Cuti | `/self-services/human-resource/leave?form=baru` |
| Pengajuan | Pengajuan Lembur | `/hr/self-service/overtime` |
| Pengajuan | Perubahan Jadwal | `/self-services/human-resource/schedule-change` |
| Pengajuan | Tukar Shift | `/self-services/human-resource/schedule` |
| Pengajuan | Pengajuan Resign | `/self-services/human-resource/resignation` |
| Bawah | Profil, Pengaturan, Keluar | Tidak berubah |

### 5.3 Aturan validasi backend yang dijaga pada 5 rute baru

Validasi pada kelima view berikut sudah mengikuti kontrak DTO/service backend dan tetap dipertahankan saat berdiri sebagai halaman mandiri:

| Layar | Validasi backend yang wajib dipatuhi frontend | Sumber backend |
| --- | --- | --- |
| Dispensasi Presensi | `AttendanceDate` wajib; `DispensationType` wajib max 20 (`LateArrival`/`EarlyDeparture`); `StartAt` dan `EndAt` di tanggal kerja sama serta `EndAt > StartAt`; `Reason` wajib max 1000 karakter (min 10 di form); pembatalan memakai `rowVersion` | `AttendanceDispensationDtos.cs:6-24`, `attendance-dispensation-employee-view.jsx:88-99` |
| Perubahan Jadwal | `RequestType` wajib max 40; `RequestedDate`, `EffectiveStartDate` wajib; `RequestedShiftId` wajib di UI self-service (`ShiftChange`/`DayOffChange`); `Reason` pengajuan & pembatalan wajib max 1000 karakter (min 10 di form); `SourceChannel` diset `"Web"` | `ScheduleChangeDtos.cs:94-125` |
| Pembatalan Cuti | Hanya untuk cuti `Approved` atau `Taken`; `EffectiveCancellationDate` wajib berada di dalam rentang `StartDate`–`EndDate` cuti; tidak boleh ada pembatalan aktif pada cuti yang sama; `CancellationReason` wajib max 2000 karakter (min 10 di form); `SourceChannel` diset `"Web"` | `LeaveLifecycleDtos.cs:27-43`, `LeaveCancellationService.cs:90-104` |
| Panggilan Kembali Cuti | `ActualReturnToWorkDate` wajib berada di antara `RecallEffectiveDate` dan `OriginalLeaveEndDate`; `Notes` opsional max 2000 karakter | `LeaveLifecycleDtos.cs:107-112`, `LeaveRecallService.cs:261` |
| Pengunduran Diri | `ProposedLastWorkingDate` wajib dan harus sesudah hari ini; `ResignationReason` wajib max 2000 karakter (min 10 di form); `HandoverPlan` opsional max 2000 karakter; `Reason` pembatalan wajib max 500 karakter; `SourceChannel` diset `"Web"` | `ResignationDtos.cs:62-85` |

---

## 6. File yang dibuat, diubah, dan dihapus

Seluruhnya pada repository **`QuilvianSystemFrontendDev`**. Backend (`NewQuilvianSystemBackend`) tidak diubah.

### 6.1 File baru

| File | Isi |
| --- | --- |
| `src/app/self-services/human-resource/attendance-dispensation/page.jsx` | Route App Router tipis + metadata untuk `AttendanceDispensationEmployeeView` |
| `src/app/self-services/human-resource/schedule-change/page.jsx` | Route App Router tipis + metadata untuk `ScheduleChangeEmployeeView` |
| `src/app/self-services/human-resource/leave-cancellation/page.jsx` | Route App Router tipis + metadata untuk `LeaveCancellationEmployeeView` |
| `src/app/self-services/human-resource/leave-recall/page.jsx` | Route App Router tipis + metadata untuk `LeaveRecallEmployeeView` |
| `src/app/self-services/human-resource/resignation/page.jsx` | Route App Router tipis + metadata untuk `ResignationEmployeeView` |
| `tests/unit/self-service-side-menu.test.mjs` | Verifikasi 28 butir menu di bawah `Layanan Saya`, keunikan key/pathname, setiap pathname punya `page.jsx`, filter izin + BPJS tetap bekerja, dan tautan `user-profile-sidebar.jsx` tidak menunjuk ke `/self-services?` |

### 6.2 File yang diubah

| File | Perubahan |
| --- | --- |
| `src/utils/menu-sidebar/menu-items.jsx` | Ganti item `Ruang Kerja Saya` (`/self-services`) dengan menu induk `Layanan Saya` (`subMenu` 9 kelompok / 28 rute langsung); jaga CRLF dan tidak menyentuh menu SDM (`corporateHumanResource`) |
| `src/components/view/settings/sidebar-profile/user-profile-sidebar.jsx` | Hapus item `Ruang Kerja Saya`, ubah teks `"Dibuka di Ruang Kerja Saya"` menjadi `"Pintasan pengajuan layanan mandiri"`, dan arahkan `SELF_SERVICE_ROUTES` langsung ke rute halaman masing-masing |
| `src/lib/constants/self-services/self-service-request-constants.js` | Tambah `SELF_SERVICE_ROUTES` dan `SELF_SERVICE_LEAVE_FORM_PARAMS` sebagai konstanta tunggal rute mandiri (menggantikan `self-service-workspace-constants.js`) |
| `src/app/self-services/page.jsx` | Ubah menjadi `redirect("/hrd/self-service")` agar bookmark lama tetap terbuka ke Beranda Layanan Saya tanpa workspace |
| `src/app/(dashboard)/hrd/self-service/page.jsx` | Pulihkan agar merender `<UnifiedEssPortalView />` (Beranda Layanan Saya) |
| `src/app/(dashboard)/corporate/hrd/self-service/page.jsx` | Ubah redirect dari `/self-services` ke `/hrd/self-service` |
| `src/components/view/hr/approvals/unified-ess-portal-view.jsx` | Perbarui `QUICK_LINKS` agar mengarah ke rute mandiri kanonis (`/ess/restitution-claims` menggantikan `/hr/self-service/restitution-claims`, tambah pintasan ke 5 halaman baru bila relevan) dan selaraskan judul/teks (`Beranda Layanan Saya`) |
| `src/components/view/self-services/human-resource/leave/leave-employee-view.jsx` | Ganti referensi `SELF_SERVICE_SHORTCUTS` menjadi `SELF_SERVICE_ROUTES.leave` dan `SELF_SERVICE_ROUTES.leaveRequest`; lepas ketergantungan pada `self-service-workspace.module.css` |
| `src/components/view/self-services/human-resource/attendance-dispensation/attendance-dispensation-employee-view.jsx` | Tambah guard `canRead` (`MyAttendanceDispensation:Read`) + `InformationAlert` bila belum punya hak akses saat dibuka via URL langsung |
| `src/components/view/self-services/human-resource/schedule-change/schedule-change-employee-view.jsx` | Tambah guard `canRead` (`MyScheduleChange:Read`) + `InformationAlert`; ubah `sourceChannel` dari `"SelfServiceWorkspace"` ke `"Web"` |
| `src/components/view/self-services/human-resource/leave-cancellation/leave-cancellation-employee-view.jsx` | Tambah guard `canRead` (`MyLeaveCancellation:Read`) + `InformationAlert`; perbarui teks `"tab Cuti"` → `"halaman Cuti Saya"`; ubah `sourceChannel` ke `"Web"` |
| `src/components/view/self-services/human-resource/leave-recall/leave-recall-employee-view.jsx` | Tambah guard `canRead` (`MyReturnToWork:Read`) + `InformationAlert`; perbarui teks `"panel ringkasan Ruang Kerja Saya"` |
| `src/components/view/self-services/human-resource/resignation/resignation-employee-view.jsx` | Tambah guard `canRead` (`MyResignation:Read`) + `InformationAlert`; ubah `sourceChannel` ke `"Web"` |
| `src/lib/constants/hr/overtime/overtime-constants.js` | Ganti `OVERTIME_R35_ROUTES.leaveRequest` dari `SELF_SERVICE_SHORTCUTS.leaveRequestCompensatory` ke `/self-services/human-resource/leave?form=baru&jenisCuti=CTO` |
| `src/lib/constants/hr/approvals/approval-constants.js` | Ganti `essPortal` → `"/hrd/self-service"` dan `myRequests` → `"/hrd/self-service/my-requests"` |
| `src/lib/hooks/payroll/use-payroll-payslip-detail.js` | Ganti tujuan `goBack` untuk slip milik sendiri dari `SELF_SERVICE_SHORTCUTS.payslip` ke `PAYROLL_ROUTES.myPayslips` |
| `tests/unit/employee-self-service-profile-change.test.mjs` | Sesuaikan assertion `ESS-002` dan `ESS-003` agar mengecek rute langsung `/self-services/human-resource/employee/profile-changes` di `menu-items.jsx` dan `user-profile-sidebar.jsx` |
| `tests/unit/self-service-menu-dedup.test.mjs` | Sesuaikan assertion `ESS-DEDUP-04` agar tidak membaca `self-service-workspace-constants.js` yang dihapus |

### 6.3 File workspace yang dihapus (jika Keputusan 2 disetujui sesuai rekomendasi)

| File | Alasan |
| --- | --- |
| `src/components/view/self-services/workspace/*` (7 file) | Seluruh tampilan container workspace (`SelfServiceWorkspaceView`, `SelfServiceWorkspaceHeader`, `SelfServiceSectionContent`, `SelfServiceTabBar`, `SelfServiceHomeSection`, `SelfServiceSummaryPanel`, `SelfServiceSummaryPopover`) tidak lagi digunakan |
| `src/lib/hooks/self-services/use-self-service-workspace.js` | Hook pengendali state `?section=&tab=` workspace tidak lagi digunakan |
| `src/lib/constants/self-services/self-service-workspace-constants.js` | Konstanta tab workspace digantikan `SELF_SERVICE_ROUTES` di `self-service-request-constants.js` |
| `src/style/self-services/self-service-workspace.module.css` | CSS khusus shell workspace & pemadatan Hero tidak lagi digunakan |
| `tests/unit/self-service-workspace-summary-popover.test.mjs` | Test khusus komponen popover workspace yang dihapus |

*(Catatan: `src/components/ui/clinical-workspace/ClinicalWorkspaceShell.jsx` tidak perlu diubah kembali karena tambahan prop aksesibilitasnya opsional dan aman dipakai modul klinis seperti Rawat Inap, IGD, dan Hemodialisa.)*

---

## 7. Urutan pengerjaan

### Fase 1 — Rute halaman baru, guard view, dan perbaikan tautan internal

1. Tambahkan `SELF_SERVICE_ROUTES` dan `SELF_SERVICE_LEAVE_FORM_PARAMS` pada `self-service-request-constants.js`.
2. Buat 5 `page.jsx` baru untuk Dispensasi Presensi, Perubahan Jadwal, Pembatalan Cuti, Panggilan Kembali Cuti, dan Pengunduran Diri.
3. Tambahkan guard `canRead` + `InformationAlert` serta bersihkan penyebutan `"SelfServiceWorkspace"` / `"tab Cuti"` / `"Ruang Kerja Saya"` pada kelima view tersebut.
4. Perbarui `leave-employee-view.jsx`, `overtime-constants.js`, `approval-constants.js`, `use-payroll-payslip-detail.js`, dan `unified-ess-portal-view.jsx` ke rute langsung.
5. Pulihkan `/hrd/self-service/page.jsx` (`UnifiedEssPortalView`) dan alihkan `/self-services/page.jsx` + `/corporate/hrd/self-service/page.jsx` ke `/hrd/self-service`.

Endpoint backend yang digunakan oleh 5 rute baru (seluruhnya sudah tersedia dan tidak diubah):

#### Self Services / Human Resource / Attendance Dispensation

Base URL: `api/v1/self-services/human-resource/attendance-dispensations`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Daftar pengajuan dispensasi presensi milik pegawai | `MyAttendanceDispensation:Read` |
| `POST` | `/` | Mengajukan dispensasi terlambat / pulang awal | `MyAttendanceDispensation:Create` |
| `POST` | `/{id}/cancel` | Membatalkan pengajuan dispensasi milik sendiri | `MyAttendanceDispensation:Cancel` |

#### Self Services / Human Resource / Schedule Change

Base URL: `api/v1/self-services/human-resource/schedule-change-requests`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Daftar pengajuan perubahan jadwal milik pegawai | `MyScheduleChange:Read` |
| `GET` | `/summary` | Ringkasan jumlah pengajuan perubahan jadwal | `MyScheduleChange:Read` |
| `GET` | `/shift-options` | Pilihan shift pengganti | `MyScheduleChange:Read` |
| `POST` | `/` | Membuat draf pengajuan perubahan jadwal | `MyScheduleChange:Create` |
| `POST` | `/{id}/submit` | Mengirim pengajuan ke atasan langsung | `MyScheduleChange:Submit` |
| `POST` | `/{id}/cancel` | Membatalkan pengajuan | `MyScheduleChange:Cancel` |
| `DELETE` | `/{id}` | Menghapus draf yang belum dikirim | `MyScheduleChange:Delete` |

#### Self Services / Human Resource / Leave Cancellation

Base URL: `api/v1/self-services/human-resource/leave/cancellations`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Daftar pembatalan cuti yang sudah disetujui | `MyLeaveCancellation:Read` |
| `POST` | `/` | Membuat draf pembatalan cuti (`Approved`/`Taken`) | `MyLeaveCancellation:Create` |
| `POST` | `/{id}/submit` | Mengirim permohonan pembatalan ke atasan | `MyLeaveCancellation:Submit` |

#### Self Services / Human Resource / Return To Work

Base URL: `api/v1/self-services/human-resource/leave/return-to-work`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Daftar panggilan kembali dari cuti milik pegawai | `MyReturnToWork:Read` |
| `POST` | `/{recallId}/acknowledge` | Mengonfirmasi tanggal masuk kembali aktual | `MyReturnToWork:Acknowledge` |

#### Self Services / Human Resource / Resignation

Base URL: `api/v1/self-services/human-resource/resignation-requests`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Daftar pengajuan pengunduran diri milik pegawai | `MyResignation:Read` |
| `GET` | `/summary` | Ringkasan status pengajuan resign | `MyResignation:Read` |
| `POST` | `/` | Membuat draf pengajuan resign | `MyResignation:Create` |
| `POST` | `/{id}/submit` | Mengirim pengajuan ke atasan dan SDM | `MyResignation:Submit` |
| `POST` | `/{id}/cancel` | Membatalkan pengajuan resign | `MyResignation:Cancel` |
| `DELETE` | `/{id}` | Menghapus draf yang belum dikirim | `MyResignation:Delete` |

### Fase 2 — Side menu kiri (`menu-items.jsx`), panel profil kanan, dan pembersihan workspace

1. Ubah `menu-items.jsx` (pertahankan CRLF): ganti `selfServiceWorkspace` menjadi pohon menu `Layanan Saya` berisi 28 rute langsung (tabel Bagian 5).
2. Perbarui `user-profile-sidebar.jsx`: hapus tombol `Ruang Kerja Saya`, ubah subjudul bagian `Pengajuan`, dan arahkan seluruh tombol langsung ke `SELF_SERVICE_ROUTES`.
3. Hapus berkas workspace yang tidak lagi terpakai (Bagian 6.3).

### Fase 3 — Unit test, pemeriksaan build, dan uji browser

1. Perbarui `employee-self-service-profile-change.test.mjs`, `self-service-menu-dedup.test.mjs`, dan tambahkan `tests/unit/self-service-side-menu.test.mjs`.
2. Jalankan `cek-rute.ps1 -FromMenu` untuk memastikan nol rute 404 pada seluruh menu.
3. Jalankan rangkaian validasi Bagian 8, lalu lengkapi Bagian 10 pada dokumen ini.

---

## 8. Testing

| No | Pemeriksaan | Hasil yang diharapkan |
| ---: | --- | --- |
| 1 | ESLint seluruh file berubah dibanding HEAD | 0 error baru; warning tidak bertambah |
| 2 | `check-css-usage.mjs` pada file JSX yang disentuh | 0 temuan baru |
| 3 | Unit test penuh (`npm run test:unit`) dibanding baseline HEAD | Tidak ada test baru yang gagal; `HR-01..12`, `ESS-001..005`, `ESS-DEDUP-01..04`, `bpjs-feature-deactivation`, dan `self-service-side-menu` lulus |
| 4 | `cek-rute.ps1 -FromMenu` | Seluruh 28 rute baru di bawah `Layanan Saya` berstatus `ADA` |
| 5 | `npm run build` | Berhasil (`✓ Compiled successfully`), seluruh 5 `page.jsx` baru terdaftar |
| 6 | Uji browser (Chrome headed) — Side menu & routing langsung | Klik setiap butir di `Layanan Saya` langsung berpindah ke rute halamannya (tanpa shell workspace); parent/child sidebar aktif sesuai route; pencarian menu menemukan butir |
| 7 | Uji browser — Panel profil kanan | Tombol `Ruang Kerja Saya` dan teks `"Dibuka di Ruang Kerja Saya"` hilang; klik `Kelola Data Saya`, `Absensi`, `Pengajuan Cuti` (masuk `?form=baru`), `Lembur`, `Perubahan Jadwal`, `Tukar Shift`, dan `Pengajuan Resign` mendarat di halaman masing-masing |
| 8 | Uji browser — Redirect & guard izin | `/self-services` dan `/corporate/hrd/self-service` dialihkan ke `/hrd/self-service`; akun pegawai tanpa izin SDM hanya melihat menu layanan yang diizinkan; akun yang membuka 5 URL baru tanpa izin `Read` tidak melempar 403/crash |
| 9 | Lebar ponsel 390 px | Side menu drawer, panel profil, dan 5 halaman baru tampil rapi tanpa scroll horizontal |
| 10 | Regresi komponen bersama | Ruang Kerja Keperawatan Rawat Inap (`ClinicalWorkspaceShell`) dan detail slip gaji (`/payroll/my-payslips/[id]`) tetap berjalan normal |

---

## 9. Di luar cakupan

| Hal | Alasan |
| --- | --- |
| Perubahan source code backend / endpoint / DTO / migration | Seluruh endpoint, permission, dan DTO backend sudah tersedia |
| Perubahan isi menu SDM admin (`corporateHumanResource`) | Sudah diatur terpisah di `PLANNING_IMPLEMENTASI_STRUKTUR_MENU_SDM_QUILVIAN_V2.md` dan `PLANNING_MENU_SIDEBAR_TANPA_DUPLIKAT_LAYANAN_MANDIRI.md` |
| Pengaturan izin peran pegawai di layar **Akses Role** | Konfigurasi data per rumah sakit, bukan perubahan kode |
| Perombakan ulang tampilan internal halaman-halaman lama (`/portal/*`, `/ess/*`, `/hr/self-service/overtime`) | Fokus tugas ini memindahkan navigasi workspace ke side menu dan langsung ke halaman |
| Penghapusan rute alias historis (`/ess/payslips`, `/hr/self-service/payslips`, `/self-services/human-resource/overtime`, `/hr/leave/my-balance`, `/hr/leave/request`) | Dibiarkan tetap hidup agar bookmark dan tautan lama tidak putus |

---

## 10. Hasil implementasi (diisi setelah implementasi dan testing)

Seluruh perubahan sisi frontend telah selesai dikerjakan di repository `QuilvianSystemFrontendDev` (tanpa mengubah source backend):

1. **Rute App Router & guard layar baru**: 5 halaman baru (`attendance-dispensation`, `schedule-change`, `leave-cancellation`, `leave-recall`, `resignation`) dibuat di bawah `src/app/self-services/human-resource/`, view terkait diberi guard `canRead` + `InformationAlert` serta pesan/channel `"Web"`.
2. **Pemulihan portal & penghapusan Workspace**: `/hrd/self-service` memulihkan `UnifiedEssPortalView` (`Beranda Layanan Saya`), `/self-services` dan `/corporate/hrd/self-service` dialihkan (`redirect`) ke `/hrd/self-service`, tautan internal (`leave-employee-view.jsx`, `overtime-constants.js`, `approval-constants.js`, `use-payroll-payslip-detail.js`, `schedule-employee-view.jsx`) mengarah ke rute langsung melalui `SELF_SERVICE_ROUTES`, dan seluruh berkas shell Workspace lama pada Bagian 6.3 telah dihapus.
3. **Side menu & panel profil kanan**: `src/utils/menu-sidebar/menu-items.jsx` mengganti `Ruang Kerja Saya` menjadi menu induk `Layanan Saya` dengan 9 bagian / 28 rute langsung (menjaga key `corporateMyCobClaims` dan memperbaiki izin `Lembur Saya`); `user-profile-sidebar.jsx` menghapus tombol `Ruang Kerja Saya`, mengganti subjudul menjadi `"Pintasan pengajuan layanan mandiri"`, dan menautkan langsung `ESS_ROUTES`.

### Ringkasan hasil verifikasi & testing

| Pemeriksaan | Hasil | Detail singkat |
| --- | --- | --- |
| `cek-rute.ps1 -FromMenu` | Lulus (`0` missing di Layanan Saya) | Seluruh 28 rute baru di bawah `Layanan Saya` berstatus `ADA` (3 status `TIDAK` pra-eksis hanya pada rute modul lain di luar Layanan Saya) |
| ESLint (`npx eslint` atas 22 berkas baru/diubah) | 0 error | 5 warning pra-eksis di `user-profile-sidebar.jsx` dan `use-payroll-payslip-detail.js`; seluruh berkas berubah bersih CRLF (`LF_only = 0`) |
| `check-css-usage.mjs` (9 berkas JSX disentuh) | 0 temuan baru | Hanya 3 temuan pra-eksis pada `user-profile-sidebar.jsx` |
| Unit test terarah (`SM`, `ESS`, `ESS-DEDUP`, `HR`) | 28 PASS, 1 pra-eksis | `SM-01..04`, `ESS-001..003,005`, `ESS-DEDUP-01..04`, `HR-01..16` lulus; `ESS-004` gagal pra-eksis; perbandingan full `npm run test:unit` vs `HEAD` tidak memunculkan regresi layanan mandiri |
| `npm run build` (`next build` + `prepare-standalone`) | Lulus (`exit 0`) | Seluruh 5 route baru terkompilasi sebagai halaman statis App Router |
| Browser Chrome CDP (`sm-1125-01-superadmin.mjs`) | **41 PASS / 0 FAIL** | 28 klik menu langsung tanpa workspace shell/404, pencarian menu, guard BPJS di `/ess/claims/cob-submission`, panel profil + 7 pintasan, redirect `/self-services` & `/corporate/hrd/self-service` |
| Browser Chrome CDP (`sm-1125-02-pegawai-guard.mjs`) | **14 PASS / 0 FAIL** | Validasi & submit berintersep `page.route` (`sourceChannel: "Web"`) pada 5 halaman baru, penyembunyian grup tanpa izin untuk akun terbatas + guard `Akses terbatas`, `/payroll/my-payslips`, drawer 390 px |
| Responsif 390 px (`sm-1125-diag-mobile.mjs`) | **6 / 6 LULUS** | Beranda Layanan Saya dan 5 halaman baru: `offenders: []` (`rootScroll: 360`, `rootClient: 360`) |

Artefak skrip dan tangkapan layar uji tersimpan di `QuilvianSystemFrontendDev/test-with-agy/scripts/sm-1125-*` dan `QuilvianSystemFrontendDev/test-with-agy/screenshots/sm-1125/`.
