# Planning — Koreksi Penempatan Menu Layanan Mandiri ke Side Profil (`user-profile-sidebar`)

| Field | Nilai |
| --- | --- |
| Tanggal | 25 November 2026 |
| Modul | Human Resource — Layanan Mandiri Pegawai (ESS) & Side Profil |
| Repository terdampak | `QuilvianSystemFrontendDev` (frontend saja; backend tidak diubah) |
| Status | Selesai diimplementasikan dan terverifikasi (25 November 2026) |
| Alur kerja | Analisis → Planning → Implementasi → Testing |
| Dokumen terkait | Mengoreksi & menggantikan penempatan side menu kiri pada `PLANNING_PEMINDAHAN_MENU_WORKSPACE_KE_SIDE_MENU_LAYANAN_MANDIRI.md` |

---

## 1. Latar belakang & koreksi masalah yang sudah diterapkan

Pada pengerjaan sebelumnya, menu **Layanan Saya** (9 kelompok / 28 rute) terlanjur diletakkan ke **side menu utama sebelah kiri** (`src/utils/menu-sidebar/menu-items.jsx` tepat di bawah `Dashboard`). Padahal yang diinginkan pemilik sistem adalah:

1. **Workspace `/self-services` tidak lagi digunakan** dan setiap item langsung membuka halaman Next.js App Router masing-masing tanpa shell workspace.
2. **Menu layanan mandiri tidak ditaruh di side menu utama kiri** (`menu-items.jsx`), melainkan **dipindahkan ke menu Side Profil sebelah kanan** (`src/components/view/settings/sidebar-profile/user-profile-sidebar.jsx`) agar side menu kiri tetap berfokus pada navigasi modul sistem (*Dashboard*, *Administrator*, *Perusahaan / SDM*, *Pelayanan Medis*).

### 1.1 Audit kondisi saat ini (sesudah penerapan sebelumnya)

| Komponen / Aset | Kondisi saat ini | Status terhadap kebutuhan baru |
| --- | --- | --- |
| **Side menu kiri utama** (`src/utils/menu-sidebar/menu-items.jsx:70-350`) | Memuat pohon menu induk `"Layanan Saya"` (9 kelompok / 28 rute) di bawah `Dashboard` | **Salah tempat — wajib dicabut** dari `menu-items.jsx` sehingga kembali `Dashboard` → `Administrator` |
| **Side Profil kanan** (`src/components/view/settings/sidebar-profile/user-profile-sidebar.jsx` & `profile.css`) | Hanya menampilkan 2 bagian ringkas: `Kepegawaian` (3 tombol: *Kelola Data Saya*, *Absensi*, *Profil Dokter*, *Ganti Password*) dan `Pengajuan` (5 pintasan: *Pengajuan Cuti*, *Lembur*, *Perubahan Jadwal*, *Tukar Shift*, *Resign*) — belum memuat 28 layanan dari Workspace, pencarian, maupun filter izin/BPJS | **Perlu diperbarui** untuk menampung seluruh 9 bagian / 28 rute Workspace + kotak pencarian + filter izin & BPJS + penanda rute aktif |
| **5 route baru & guard `canRead`** (`src/app/self-services/human-resource/attendance-dispensation`, `schedule-change`, `leave-cancellation`, `leave-recall`, `resignation`) | Sudah dibuat beserta guard `canRead` dan `sourceChannel: "Web"` | **Tetap dipertahankan** (dipakai langsung saat diklik dari Side Profil) |
| **Beranda (`/hrd/self-service`) & redirect `/self-services`, `/corporate/hrd/self-service`** | `UnifiedEssPortalView` sudah dipulihkan; bookmark lama sudah redirect ke `/hrd/self-service`; berkas Workspace lama sudah dihapus | **Tetap dipertahankan** |
| **Tautan internal & `SELF_SERVICE_ROUTES`** (`self-service-request-constants.js`, `leave-employee-view.jsx`, `overtime-constants.js`, `approval-constants.js`, `use-payroll-payslip-detail.js`) | Sudah memakai rute App Router langsung | **Tetap dipertahankan** |
| **Unit test** (`self-service-side-menu.test.mjs`, `employee-self-service-profile-change.test.mjs`, `self-service-menu-dedup.test.mjs`) | Saat ini menguji keberadaan `key: "selfServices"` di `menu-items.jsx` | **Perlu disesuaikan** untuk memverifikasi `menu-items.jsx` bebas dari menu Workspace/`Layanan Saya` dan menguji seluruh menu di **Side Profil** |

### 1.2 Contoh skenario di rumah sakit

Seorang perawat shift malam (Ns. Novi) atau kepala ruangan (Ibu Hartini) sedang membuka modul *Rawat Inap* atau *Dashboard*:

- **Sebelumnya (kondisi sekarang yang keliru):** Di side menu kiri aplikasi muncul daftar panjang *Layanan Saya* beserta sub-grupnya sebelum *Administrator* / *Pelayanan*, bercampur secara visual dengan modul kerja sistem, sementara di Side Profil kanan hanya ada 8 tombol dasar (belum ada *Slip Gaji Saya*, *Koreksi/Dispensasi Presensi*, *Pembatalan/Panggilan Kembali Cuti*, *Kinerja & Diklat*, *Praktik Klinis*, *Pengaduan & K3*, maupun *Riwayat Permohonan*).
- **Sesudah perbaikan (rencana ini):**
  1. Side menu kiri kembali bersih (*Dashboard* lalu langsung bagian *Administrator*, *Perusahaan / SDM*, dst.).
  2. Ketika pegawai mengklik foto/nama profil di pojok kanan atas, **Side Profil** terbuka dengan seluruh menu layanan mandiri tersusun rapi per kategori (*Beranda*, *Kehadiran & Jadwal*, *Cuti & Lembur*, *Gaji & Klaim*, *Data Kepegawaian*, *Kinerja & Diklat*, *Praktik Klinis*, *Pengaduan & K3*, *Riwayat Permohonan*, plus *Keamanan & Akun*) lengkap dengan kotak pencarian cepat *"Cari layanan..."*.
  3. Mengklik butir mana saja di Side Profil (misalnya **Dispensasi Presensi** atau **Slip Gaji Saya**) menutup Side Profil lalu langsung membuka halaman App Router terkait (`/self-services/human-resource/attendance-dispensation` atau `/payroll/my-payslips`).

---

## 2. Tujuan

Mencabut menu **Layanan Saya** dari side menu utama kiri (`menu-items.jsx`) dan memindahkan seluruh struktur menu bekas Workspace (9 bagian / 28 rute langsung) ke dalam **Side Profil** (`user-profile-sidebar.jsx`) secara rapi, terproteksi hak akses/BPJS, serta nyaman dinavigasi di desktop maupun ponsel.

---

## 3. Gambaran layar

### 3.1 Side menu kiri kembali bersih (tanpa `Layanan Saya` & tanpa `Ruang Kerja Saya`)

```text
┌──────────────────────────────┬─────────────────────────────────────────────────────┐
│ SIDE MENU KIRI               │ HALAMAN PENUH (contoh saat membuka Dispensasi /     │
│ [Cari menu...]               │ Slip Gaji setelah klik dari Side Profil kanan)      │
│                              │                                                     │
│ ▸ Dashboard                  │ ┌─────────────────────────────────────────────────┐ │
│                              │ │ Hero + Konten Halaman Penuh                     │ │
│ ── Administrator ──          │ └─────────────────────────────────────────────────┘ │
│ ▸ Master Data                │                                                     │
│ ▸ Manajemen Role             │                                                     │
│                              │                                                     │
│ ── Perusahaan ──             │                                                     │
│ ▸ Sumber Daya Manusia        │                                                     │
│ ▸ Penggajian                 │                                                     │
└──────────────────────────────┴─────────────────────────────────────────────────────┘
```

### 3.2 Side Profil kanan (`user-profile-sidebar.jsx`, lebar `min(430px, 100vw)`)

```text
┌───────────────────────────────────────────────────────────┐
│  [X]                                          [👤 Profil] │
│                      [ Avatar ]                           │
│           Hartini Sulistyowati, S.Psi., M.M.              │
│  ┌───────────┬──────────┬──────────┬──────────┐           │
│  │Departemen │ Jabatan  │  Hadir   │  Pulang  │           │
│  └───────────┴──────────┴──────────┴──────────┘           │
├───────────────────────────────────────────────────────────┤
│  [ 🔍 Cari layanan mandiri...                        ]    │
│                                                           │
│  ┌─────────────────────────────────────────────────────┐  │
│  │ ⊞ Beranda Layanan Saya              Saldo & ringkas │→ │
│  └─────────────────────────────────────────────────────┘  │
│                                                           │
│  ▾ KEHADIRAN & JADWAL (5)                                 │
│    • Presensi Saya  [Belum Masuk]  → /self-services/...   │
│    • Koreksi Presensi              → /self-services/...   │
│    • Dispensasi Presensi           → /self-services/...   │
│    • Jadwal & Tukar Shift          → /self-services/...   │
│    • Perubahan Jadwal              → /self-services/...   │
│                                                           │
│  ▾ CUTI & LEMBUR (5)                                      │
│    • Ajukan Cuti Baru              → .../leave?form=baru  │
│    • Cuti Saya                     → .../leave            │
│    • Pembatalan Cuti               → .../leave-cancellation
│    • Panggilan Kembali Cuti        → .../leave-recall     │
│    • Lembur Saya                   → /hr/self-service/... │
│                                                           │
│  ▸ GAJI & KLAIM (2–3 sesuai status BPJS)                  │
│  ▸ DATA KEPEGAWAIAN (5)                                   │
│  ▸ KINERJA & DIKLAT (3)                                   │
│  ▸ PRAKTIK KLINIS (4–5, termasuk Profil Dokter)           │
│  ▸ PENGADUAN & K3 (2)                                     │
│                                                           │
│  ┌─────────────────────────────────────────────────────┐  │
│  │ 🕒 Riwayat Permohonan Saya                          │→ │
│  └─────────────────────────────────────────────────────┘  │
│                                                           │
│  ── AKUN & KEAMANAN ──                                    │
│    • Ganti Password                → /settings/self-ser.. │
├───────────────────────────────────────────────────────────┤
│  [ ⚙ Pengaturan ]                        [ ⏻ Keluar ]    │
└───────────────────────────────────────────────────────────┘
```

*(Saat mengetik di kotak pencarian, kelompok yang punya kecocokan otomatis terbuka dan hanya baris yang cocok yang ditampilkan. Saat membuka Side Profil di suatu halaman layanan mandiri, misalnya `/self-services/human-resource/schedule-change`, grup induk **Kehadiran & Jadwal** otomatis terbuka dan butir **Perubahan Jadwal** disorot aktif.)*

---

## 4. Keputusan

Setiap baris menyajikan rekomendasi terbaik dan alternatifnya. Perubahan dari rekomendasi cukup disebutkan sebelum implementasi dimulai.

| No | Hal | Rekomendasi | Alternatif |
| ---: | --- | --- | --- |
| 1 | Side menu kiri (`menu-items.jsx`) | **Menghapus seluruh blok `Layanan Saya` (`key: "selfServices"`)** dari `menu-items.jsx` sehingga side menu kiri kembali tepat `Dashboard` → `Administrator` → modul organisasi/medis (sama bersihnya dengan sebelum Workspace dibuat, serta menjaga `corporateHumanResource` tanpa duplikasi ESS). | Menyisakan 1 butir `Beranda Layanan Saya` (`/hrd/self-service`) di side menu kiri (tetap menimbulkan pertanyaan kenapa ada menu ESS di sidebar kiri bila pengguna menginginkannya di Side Profil). |
| 2 | Tata letak 28+ menu di **Side Profil** (`user-profile-sidebar.jsx`) | **Kelompok Akordeon (Expand/Collapse) + Pencarian Cepat (`Cari layanan mandiri...`) + Highlight Rute Aktif.** Secara default kelompok **Kehadiran & Jadwal**, **Cuti & Lembur**, dan kelompok yang memuat halaman aktif (`pathname` saat ini) terbuka; kelompok lain dapat dibuka-tutup per klik. Tiap sub-item memakai gaya kartu ringkas agar panel tidak terlalu tinggi/berat di-scroll. | Menampilkan seluruh 28 kartu `min-height: 76px` terbuka sekaligus tanpa akordeon dan tanpa pencarian (menghasilkan daftar >2.400 px ke bawah yang melelahkan digulir). |
| 3 | Nasib tombol lama di Side Profil (*Absensi*, *Ajukan Cuti `?form=baru`*, *Profil Dokter*, *Ganti Password*) | **Diintegrasikan rapi tanpa kehilangan fungsi lama:**<br>• **Presensi Saya** tetap menampilkan badge status hadir hari ini (`attendanceToday.state.label`).<br>• **Ajukan Cuti Baru** (`/self-services/human-resource/leave?form=baru`) diletakkan tepat di atas **Cuti Saya** dalam kelompok *Cuti & Lembur* sehingga alur 1-klik buat cuti dari Side Profil tetap ada.<br>• **Profil Dokter** tampil di kelompok *Praktik Klinis* (khusus `isDoctor`) dengan badge `Loading`/`Dokter`.<br>• **Ganti Password** tetap di bagian *Akun & Keamanan* sebelum footer *Pengaturan* & *Keluar*. | Membuat bagian terpisah *"Pintasan Cepat"* di paling atas berisi 7 tombol lama lalu mengulang lagi 28 item di bawahnya (membuat menu ganda di dalam Side Profil sendiri). |
| 4 | Penyaringan hak akses & fitur BPJS di Side Profil | Memakai **`useEffectivePermissions()`** + **`filterMenuItemsByPermission`** serta **`useSystemFeatures({ enabled: isOpen && isAuthenticated && permissionLoaded })`** + **`adaptPayrollMenuItems`** (dengan `key: "corporateMyCobClaims"`), persis mengikuti aturan fail-closed left sidebar: menu tanpa izin dan kelompok kosong disembunyikan otomatis; Klaim COB hanya tampil jika BPJS aktif; fetch sistem features hanya jalan saat panel dibuka. | Menampilkan semua 28 item tanpa cek izin di Side Profil (membuat pegawai tanpa izin melihat banyak menu yang saat diklik berujung alert *"Akses terbatas"*). |
| 5 | Penempatan definisi struktur menu Side Profil | Menyimpan data murni (tanpa JSX) daftar kelompok & rute Side Profil dalam file konstanta terdedikasi (`src/lib/constants/self-services/self-service-profile-menu.js`), lalu merender ikon (`react-icons/fa6`/`ri`) dan handler klik di `user-profile-sidebar.jsx`. Kelebihan: mudah diuji lewat unit test Node (`tests/unit/`) dan memisahkan data dari animasi Framer Motion. | Menaruh array konfigurasi langsung di dalam `user-profile-sidebar.jsx` (sulit diuji langsung dengan `filterMenuItemsByPermission` / `adaptPayrollMenuItems` di Node test). |
| 6 | Penutupan otomatis Side Profil & mode height pendek (`max-height: 430px`) | Mengklik item langsung memanggil `router.push(pathname)` lalu `safeClose()` (drawer menutup otomatis); state pencarian di-reset saat drawer ditutup. Mode layar pendek (`@media (max-height: 430px)`) tetap menjaga item akordeon ringkas agar tidak terpotong di viewport landscape HP. | Membiarkan Side Profil tetap terbuka setelah klik rute (menghalangi halaman tujuan di kanan layar). |

---

## 5. Daftar lengkap isi menu di Side Profil

Seluruh item menggunakan tipe rute App Router langsung (tanpa `/self-services?section=...&tab=...`).

| No | Kelompok di Side Profil | Label menu | `key` | `pathname` tujuan | Penjagaan izin / kondisi tampil |
| ---: | --- | --- | --- | --- | --- |
| 1 | *(Kartu utama atas)* | Beranda Layanan Saya | `selfServiceHome` | `/hrd/self-service` | Terbuka *(portal ringkas saldo & riwayat)* |
| 2 | Kehadiran & Jadwal | Presensi Saya *(+ badge hadir hari ini)* | `selfServiceAttendance` | `/self-services/human-resource/attendance` | Terbuka |
| 3 | Kehadiran & Jadwal | Koreksi Presensi | `selfServiceAttendanceCorrection` | `/self-services/human-resource/attendance-correction` | `MyAttendanceCorrection:Read` |
| 4 | Kehadiran & Jadwal | Dispensasi Presensi | `selfServiceAttendanceDispensation` | `/self-services/human-resource/attendance-dispensation` | `MyAttendanceDispensation:Read` |
| 5 | Kehadiran & Jadwal | Jadwal & Tukar Shift | `selfServiceSchedule` | `/self-services/human-resource/schedule` | `MySchedule:Read` \| `MyShiftSwap:Read` |
| 6 | Kehadiran & Jadwal | Perubahan Jadwal | `selfServiceScheduleChange` | `/self-services/human-resource/schedule-change` | `MyScheduleChange:Read` |
| 7 | Cuti & Lembur | Ajukan Cuti Baru *(pintasan form)* | `selfServiceLeaveRequest` | `/self-services/human-resource/leave?form=baru` | `MyLeaveRequest:Create` \| `MyLeaveRequest:Read` |
| 8 | Cuti & Lembur | Cuti Saya | `selfServiceLeave` | `/self-services/human-resource/leave` | `MyLeaveRequest:Read` |
| 9 | Cuti & Lembur | Pembatalan Cuti | `selfServiceLeaveCancellation` | `/self-services/human-resource/leave-cancellation` | `MyLeaveCancellation:Read` |
| 10 | Cuti & Lembur | Panggilan Kembali Cuti | `selfServiceLeaveRecall` | `/self-services/human-resource/leave-recall` | `MyReturnToWork:Read` |
| 11 | Cuti & Lembur | Lembur Saya | `selfServiceOvertime` | `/hr/self-service/overtime` | `OvertimePlan:ViewSelf/Create` \| `OvertimeRealizationEss:View/Create` |
| 12 | Gaji & Klaim | Slip Gaji Saya | `selfServicePayslips` | `/payroll/my-payslips` | `MyPayslip:Read` |
| 13 | Gaji & Klaim | Klaim Restitusi | `selfServiceRestitution` | `/ess/restitution-claims` | Terbuka *(guard internal view)* |
| 14 | Gaji & Klaim | Klaim Koordinasi Manfaat (COB) | `corporateMyCobClaims` | `/ess/claims/cob-submission` | Disembunyikan `adaptPayrollMenuItems` saat BPJS off |
| 15 | Data Kepegawaian | Kelola Data Saya | `selfServiceProfileChanges` | `/self-services/human-resource/employee/profile-changes` | `MyProfileChange:Read` |
| 16 | Data Kepegawaian | Keluarga & Tanggungan | `selfServiceDependents` | `/ess/my-dependents` | Terbuka |
| 17 | Data Kepegawaian | Dokumen & PIN | `selfServiceDocuments` | `/portal/services/my-documents` | Terbuka |
| 18 | Data Kepegawaian | Surat & Layanan HR | `selfServiceServiceRequests` | `/portal/services/requests` | Terbuka |
| 19 | Data Kepegawaian | Pengunduran Diri | `selfServiceResignation` | `/self-services/human-resource/resignation` | `MyResignation:Read` |
| 20 | Kinerja & Diklat | Evaluasi Kinerja | `selfServiceAppraisals` | `/portal/performance/appraisals` | Terbuka |
| 21 | Kinerja & Diklat | Sanggah Nilai | `selfServiceDisputes` | `/portal/performance/disputes` | Terbuka |
| 22 | Kinerja & Diklat | Dasbor Pelatihan | `selfServiceLearning` | `/portal/learning/dashboard` | `MandatoryHospitalTraining` \| `TrainingSession` \| `TrainingCertificate` \| `AnnualTrainingPlan` (`Read`) |
| 23 | Praktik Klinis | Kredensialing | `selfServiceCredentialing` | `/portal/credentialing/application` | Terbuka |
| 24 | Praktik Klinis | Rapor OPPE | `selfServiceOppe` | `/portal/performance/clinical-oppe` | Terbuka |
| 25 | Praktik Klinis | Peer Review | `selfServicePeerReview` | `/portal/performance/peer-review-entry` | Terbuka |
| 26 | Praktik Klinis | Portofolio SKP | `selfServiceSkp` | `/portal/learning/skp-portfolio` | Terbuka |
| 27 | Praktik Klinis | Profil Dokter *(bila akun dokter)* | `selfServiceDoctorProfile` | `/settings/dokter/detail-dokter/${doctorId}` | `isDoctor === true` *(komponen khusus di `user-profile-sidebar.jsx`)* |
| 28 | Pengaduan & K3 | Lapor Pajanan (CITO) | `selfServiceNeedleStick` | `/portal/health/panic` | Terbuka |
| 29 | Pengaduan & K3 | Pengaduan | `selfServiceGrievance` | `/portal/grievance` | Terbuka |
| 30 | *(Kartu mandiri bawah)* | Riwayat Permohonan Saya | `selfServiceMyRequests` | `/hrd/self-service/my-requests` | `WorkflowInboxInstance:ViewOwn` |
| 31 | Akun & Keamanan | Ganti Password | `selfServiceChangePassword` | `/settings/self-services-profile` | Terbuka *(header `Profil` tetap sama)* |

*(Footer Side Profil tetap: **Pengaturan** → `/settings/user-settings` dan **Keluar**.)*

---

## 6. File yang dibuat, diubah, dan dihapus

Seluruhnya pada repository **`QuilvianSystemFrontendDev`**. Backend (`NewQuilvianSystemBackend`) tidak diubah.

| Aksi | File | Isi perubahan |
| --- | --- | --- |
| **Baru** | `src/lib/constants/self-services/self-service-profile-menu.js` | Definisi data murni 9 bagian / seluruh rute langsung layanan mandiri Side Profil (`SELF_SERVICE_PROFILE_MENU_ITEMS`), `requiredPermission`, dan helper pencocokan route aktif + filter pencarian teks. |
| **Ubah** | `src/utils/menu-sidebar/menu-items.jsx` | Hapus blok menu induk `Layanan Saya` (`key: "selfServices"`) dari posisi di bawah `Dashboard` agar side menu kiri kembali bersih (*Dashboard* langsung ke pemisah *Administrator*). Pertahankan CRLF. |
| **Ubah** | `src/components/view/settings/sidebar-profile/user-profile-sidebar.jsx` | Integrasikan `useEffectivePermissions`, `useSystemFeatures`, `filterMenuItemsByPermission`, dan `adaptPayrollMenuItems`; tambahkan kotak pencarian *"Cari layanan mandiri..."*, kartu Beranda, 7 grup akordeon, kartu Riwayat Permohonan, *Akun & Keamanan*, highlight halaman aktif, serta auto-reset pencarian saat panel ditutup. Pertahankan CRLF. |
| **Ubah** | `src/components/view/settings/sidebar-profile/profile.css` | Tambah kelas CSS berbasis gaya glass/dark panel existing untuk kotak pencarian (`.profile-search-box`), header akordeon grup (`.profile-group-toggle`, `.profile-group-count`, `.profile-group-list`), baris submenu ringkas (`.profile-submenu-item`), dan status aktif (`.is-active`), termasuk aturan responsif `max-width: 576px` & `max-height: 430px`. |
| **Ubah** | `tests/unit/employee-self-service-profile-change.test.mjs` | Perbarui `ESS-002` & `ESS-003` agar mengecek ketiadaan menu ESS di `menu-items.jsx` dan memverifikasi keberadaan `Kelola Data Saya` + `MyProfileChange:Read` pada `self-service-profile-menu.js` & `user-profile-sidebar.jsx`. |
| **Ubah** | `tests/unit/self-service-menu-dedup.test.mjs` | Perbarui `ESS-DEDUP-04` agar memverifikasi izin Diklat (`MandatoryHospitalTraining`) pada `self-service-profile-menu.js`. |
| **Ubah** | `tests/unit/self-service-side-menu.test.mjs` | Balik assertion `SM-01..SM-04` agar memverifikasi: (a) `menu-items.jsx` TIDAK memuat `selfServiceWorkspace` maupun `selfServices`, (b) `SELF_SERVICE_PROFILE_MENU_ITEMS` memuat 28+ rute langsung unik & semuanya punya `page.jsx`, (c) `filterMenuItemsByPermission` + `adaptPayrollMenuItems` menyaring izin & COB BPJS dengan tepat, (d) `user-profile-sidebar.jsx` merender modul tersebut dan menjaga fitur pencarian & akordeon. |

*(Catatan: 5 `page.jsx` baru, guard `canRead` + `sourceChannel: "Web"` di 5 view baru, pemulihan `UnifiedEssPortalView` di `/hrd/self-service`, redirect `/self-services` & `/corporate/hrd/self-service`, dan penghapusan berkas Workspace lama sudah terpasang dari fase sebelumnya dan tetap dipertahankan tanpa perlu diubah lagi.)*

---

## 7. Urutan pengerjaan

1. **Fase 1 — Konstanta menu & pembersihan side menu kiri:**
   - Buat `src/lib/constants/self-services/self-service-profile-menu.js` dari spesifikasi Bagian 5.
   - Hapus blok `selfServices` pada `src/utils/menu-sidebar/menu-items.jsx` sambil menjaga CRLF.
2. **Fase 2 — Implementasi Side Profil akordeon + pencarian + izin:**
   - Perbarui `src/components/view/settings/sidebar-profile/profile.css` untuk komponen pencarian, grup akordeon, baris kompak, dan active highlight.
   - Perbarui `src/components/view/settings/sidebar-profile/user-profile-sidebar.jsx` memakai `useEffectivePermissions`, `useSystemFeatures`, `filterMenuItemsByPermission`, dan `adaptPayrollMenuItems`, lalu verifikasi CRLF.
3. **Fase 3 — Penyesuaian unit test & audit statis:**
   - Perbarui `employee-self-service-profile-change.test.mjs`, `self-service-menu-dedup.test.mjs`, dan `self-service-side-menu.test.mjs`.
   - Jalankan ESLint, `check-css-usage.mjs`, `cek-rute.ps1 -FromMenu`, `npm run test:unit` vs HEAD baseline, dan `npm run build`.
4. **Fase 4 — Uji live Chrome CDP & mobile 390 px:**
   - Uji `superadmin`, `hrB_penyetuju`, dan `bukanAtasan_nonAdmin` pada Chrome headed + cek overflow mobile `390px` terhadap **Side Profil**, penutupan otomatis drawer setelah navigasi, pencarian layanan, penyembunyian izin, dan kepastian side menu kiri tidak lagi memuat *Layanan Saya*.

### Spesifikasi endpoint yang dibaca Side Profil (sudah tersedia, tanpa ubah backend)

Side Profil hanya mengonsumsi dua endpoint baca existing (lewat Redux/store cache bersama):

#### Auth / Permission
Base URL: `api/v1/auth`
| Method | Path | Kegunaan | Auth |
| --- | --- | --- | --- |
| `GET` | `/permissions/effective` | Daftar izin efektif sesi untuk menyaring menu Side Profil (`useEffectivePermissions`) | Token sesi login (`[Authorize]`) |

#### System / Features
Base URL: `api/v1/system`
| Method | Path | Kegunaan | Auth |
| --- | --- | --- | --- |
| `GET` | `/features` | Membaca flag `bpjsEnabled` fail-closed untuk menampilkan/menyembunyikan *Klaim COB* (`useSystemFeatures`) | Token sesi login (`[Authorize]`) |

---

## 8. Testing

| No | Pemeriksaan | Hasil yang diharapkan |
| ---: | --- | --- |
| 1 | ESLint + CRLF check seluruh berkas berubah | 0 error baru; CRLF terjaga pada `menu-items.jsx`, `user-profile-sidebar.jsx`, `employee-self-service-profile-change.test.mjs` |
| 2 | `check-css-usage.mjs` pada `user-profile-sidebar.jsx` | Tidak ada temuan kelas CSS baru yang tidak terdefinisi di CSS |
| 3 | Unit test (`SM`, `ESS`, `ESS-DEDUP`, `HR`, `bpjs-feature-deactivation`) | Seluruhnya lulus tanpa regresi terhadap baseline HEAD |
| 4 | `cek-rute.ps1` terhadap seluruh rute `self-service-profile-menu.js` + `cek-rute.ps1 -FromMenu` | Seluruh 29 pathname di Side Profil berstatus `ADA` |
| 5 | `npm run build` | Lulus (`exit 0`) dan standalone bundle siap |
| 6 | Uji browser Chrome CDP — Side menu kiri bersih | Tidak ada butir `Ruang Kerja Saya` ataupun `Layanan Saya` di side menu kiri; menu *Sumber Daya Manusia* admin/petugas tetap utuh |
| 7 | Uji browser Chrome CDP — Side Profil (`superadmin` & `hrB_penyetuju`) | Seluruh 9 bagian / 29+ item tampil di Side Profil; klik setiap item menutup drawer & membuka rute App Router langsung (termasuk `?form=baru` pada *Ajukan Cuti Baru*); pencarian menyaring layanan secara live; halaman aktif otomatis tersorot di Side Profil |
| 8 | Uji browser Chrome CDP — Filter izin & BPJS (`bukanAtasan_nonAdmin`) | Item tanpa izin & kelompok *Cuti & Lembur*/*Riwayat Permohonan* tersembunyi di Side Profil; *Klaim COB* tersembunyi bila BPJS off; tidak ada 403/5xx saat membuka Side Profil |
| 9 | Lebar ponsel `390 px` & tinggi pendek (`640x360`) | Panel Side Profil terbuka penuh (`width: 100vw`), akordeon & kotak pencarian tidak melebihi lebar layar (`scrollWidth <= clientWidth`), dan tombol footer (*Pengaturan* / *Keluar*) tetap terjangkau |

---

## 9. Di luar cakupan

| Hal | Alasan |
| --- | --- |
| Perubahan kode backend / endpoint / permission | Seluruh endpoint, DTO, dan `page.jsx` sudah siap |
| Mengembalikan menu *Layanan Mandiri* ke dalam `Sumber Daya Manusia` (`corporateHumanResource`) di sidebar kiri | Keputusan menu SDM memisahkan menu admin/atasan dari layanan pribadi pegawai; seluruh layanan pribadi kini berada terpusat di **Side Profil** |
| Mengubah desain layar isi di halaman-halaman `/portal/*` atau `/ess/*` | Fokus koreksi adalah memindahkan menu layanan dari side menu kiri & workspace ke **Side Profil** |

---

## 10. Hasil implementasi

Seluruh koreksi telah diimplementasikan dan diverifikasi pada repository `QuilvianSystemFrontendDev` (backend tidak diubah):

1. **Pembersihan side menu kiri (`src/utils/menu-sidebar/menu-items.jsx`)**: Blok pohon menu induk `Layanan Saya` (`key: "selfServices"`) dihapus dari posisi di bawah `Dashboard`, sehingga sidebar kiri kembali bersih langsung dari `Dashboard` ke pemisah `Administrator` sambil menjaga struktur menu SDM (`corporateHumanResource`) tanpa duplikasi layanan pribadi.
2. **Menu terstruktur di Side Profil (`src/lib/constants/self-services/self-service-profile-menu.js`, `src/components/view/settings/sidebar-profile/user-profile-sidebar.jsx`, dan `profile.css`)**:
   - Memindahkan seluruh 9 bagian / 29 rute langsung (28 rute bekas Workspace + pintasan *Ajukan Cuti Baru* `?form=baru`) ke dalam Side Profil beserta integrasi item *Presensi Saya* (badge kehadiran harian), *Profil Dokter* (muncul di kelompok *Praktik Klinis* khusus akun dokter), dan *Ganti Password* (*Akun & Keamanan*).
   - Menambahkan kotak pencarian cepat (`Cari layanan mandiri...`), kelompok akordeon (terbuka default untuk *Kehadiran & Jadwal*, *Cuti & Lembur*, dan kelompok yang memuat rute aktif), highlight rute aktif (`aria-current="page"`), reset pencarian saat menutup panel, serta penyaringan fail-closed memakai `useEffectivePermissions`, `filterMenuItemsByPermission`, `useSystemFeatures`, dan `adaptPayrollMenuItems` (`corporateMyCobClaims` tersembunyi bila BPJS nonaktif).
3. **Unit test (`tests/unit/`)**: `self-service-side-menu.test.mjs` (`SM-01..SM-05`), `employee-self-service-profile-change.test.mjs` (`ESS-002`, `ESS-003`), dan `self-service-menu-dedup.test.mjs` (`ESS-DEDUP-04`) diperbarui agar menegaskan `menu-items.jsx` bersih dari `Ruang Kerja Saya` maupun `Layanan Saya`, serta menguji struktur 29 rute Side Profil, izin akses, BPJS COB, pencarian, dan penanda rute aktif.

### Ringkasan hasil verifikasi & uji browser

| Pemeriksaan | Hasil | Detail singkat |
| --- | --- | --- |
| ESLint + CRLF (`menu-items.jsx`, `user-profile-sidebar.jsx`, `profile.css`, unit test) | **0 error**, CRLF utuh | 4 warning pra-eksis pada hooks awal `user-profile-sidebar.jsx`; seluruh berkas CRLF memiliki `LF_only = 0` |
| `check-css-usage.mjs` (`user-profile-sidebar.jsx`) | **0 temuan baru** | Hanya 1 temuan pra-eksis (`profile-avatar-initials`) |
| `cek-rute.ps1 -FromMenu` | **3 pre-existing `TIDAK`** | Seluruh menu `menu-items.jsx` valid (3 rute `TIDAK` hanya pra-eksis di modul registrasi & kebijakan BPJS lama) |
| Unit test terarah (`SM`, `ESS`, `ESS-DEDUP`, `HR`, `BPJS`) | **39 PASS**, 3 pra-eksis | `SM-01..05`, `ESS-001..003,005`, `ESS-DEDUP-01..04`, `HR-01..16` lulus; gagal pra-eksis: `ESS-004`, `bpjs:119`, `bpjs:263`. Baseline `npm run test:unit`: 5.398 PASS / 149 FAIL vs HEAD 5.204 PASS / 186 FAIL (0 regresi layanan mandiri) |
| `npm run build` (`next build` + `prepare-standalone.mjs`) | **Lulus (`exit 0`)** | 935 halaman statis terkompilasi penuh tanpa error Suspense/`useSearchParams`; standalone bundle siap |
| Chrome CDP — SuperAdmin (`sm-1125-01-superadmin.mjs`) | **37 PASS / 0 FAIL** | Sidebar kiri bersih, 9 bagian & 29 item di Side Profil diklik langsung menuju App Router + drawer menutup otomatis, pencarian live, highlight rute aktif, guard BPJS di `/ess/claims/cob-submission`, `Ganti Password`, serta redirect `/self-services` & `/corporate/hrd/self-service` |
| Chrome CDP — Pegawai & Akun Terbatas (`sm-1125-02-pegawai-guard.mjs`) | **14 PASS / 0 FAIL** | Form 5 halaman baru + intersepsi `page.route` (`sourceChannel: "Web"`), penyembunyian grup `Cuti & Lembur` + `Riwayat Permohonan` di Side Profil untuk akun terbatas (`bukanAtasan_nonAdmin`), guard `Akses terbatas`, `/payroll/my-payslips`, serta Side Profil di 390 px |
| Responsif 390 px (`sm-1125-diag-mobile.mjs`) | **6 / 6 LULUS** | Beranda (`/hrd/self-service`) dan ke-5 halaman baru memiliki `offenders: []` (`rootScroll: 360`, `rootClient: 360`) |

Artefak skrip dan tangkapan layar uji tersimpan di `QuilvianSystemFrontendDev/test-with-agy/scripts/sm-1125-*` dan `QuilvianSystemFrontendDev/test-with-agy/screenshots/sm-1125/`.
