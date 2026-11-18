# LAPORAN EKSEKUSI DAN PENYELESAIAN HRD MENU 04: DATA INDUK PEGAWAI

**Tanggal Pelaporan:** 16 November 2026  
**Target Repositori & Branch:**
- **Backend:** `DevBenari/NewQuilvianSystemBackend` (Branch `MHamzah`)
- **Frontend:** `DevBenari/QuilvianSystemFrontendDev` (Branch `HamzahV2`)
**Referensi Instruksi:** `Instruksi_Agent_HRD_Menu_04.md` (Human Resource Module Finishing Wave `FINISH-M04`)

---

## 1. Ringkasan Eksekutif & Hasil Pekerjaan

Sesuai instruksi pada dokumen `Instruksi_Agent_HRD_Menu_04.md`, telah diselesaikan perancangan, penyesuaian arsitektur, penguatan keamanan server, dan implementasi antarmuka untuk **Menu Resmi 04: Data Induk Pegawai** dengan **tepat 6 Submenu** tanpa mengubah domain canonical maupun menduplikasi data master.

### Status 6 Submenu Resmi (Urutan Baku):
1. **Profil Pegawai** (`/hr/data-induk-pegawai/profil`) — **VERIFIED WORKING / REPAIRED**
   - Menggunakan model identitas tunggal *One Person = One WorkforceProfile* (`HRD-DEC-051`).
   - Layanan penyelesai identitas bersama (`WorkforceIdentityResolverService`) mencegah duplikasi `MstWorkforceProfile` saat registrasi lintas peran (Pegawai, Dokter, Pengguna Eksternal).
   - Penguncian server-side mutasi langsung field sensitif (NIK, status pernikahan) dengan respon `409 Conflict` terarah ke Menu 06 (`HRD-DEC-052`).
2. **Keluarga** (`/hr/data-induk-pegawai/keluarga`) — **VERIFIED WORKING / REPAIRED**
   - Menampilkan susunan anggota keluarga (`WfpFamilyMember`), kontak darurat (`WfpEmergencyContact`), dan tanggungan (`WfpDependent`).
   - Guard mutasi langsung field relasi/identitas keluarga dan kelayakan pajak/benefit ditutup di backend; diarahkan melalui workflow persetujuan berbukti Menu 06 dengan prinsip *dual-actor verification*.
3. **Pendidikan** (`/hr/data-induk-pegawai/pendidikan`) — **VERIFIED WORKING / REPAIRED**
   - Pengelolaan riwayat pendidikan formal (`WfpEducation`), upload dokumen ijazah (PDF/JPG/PNG maksimal 10 MB).
   - Penjaminan tepat satu entitas pendidikan tertinggi (`IsHighestEducation`) pada kondisi persaingan data (concurrency safe).
   - Segregasi verifikator berkas ijazah (`entity.CreateBy != actorUserId`).
4. **Riwayat Pekerjaan** (`/hr/data-induk-pegawai/riwayat-pekerjaan`) — **VERIFIED WORKING / REPAIRED**
   - Pemisahan jelas dua sumber riwayat kerja dalam satu Submenu:
     - **Pengalaman Sebelumnya:** Pengalaman kerja di institusi/RS sebelumnya (`HistoryType = "ExternalPrior"`), dapat dicatat, diedit, dan dilengkapi surat pengalaman kerja tanpa perlu migrasi skema destruktif.
     - **Riwayat Kepegawaian di RS:** Peristiwa resmi internal yang bersumber dari Surat Keputusan (SK) mutasi, promosi, rotasi, penempatan, dan terminasi (`HrdPersonnelActionOrder`); berstatus *strictly read-only* pada jalur Menu 04 dan tidak dapat diedit/dihapus manual untuk mencegah manipulasi sejarah SK.
5. **Dokumen Personal** (`/hr/data-induk-pegawai/dokumen-personal`) — **VERIFIED WORKING / REPAIRED**
   - Pengelolaan berkas personal (`WfpDocument`) dengan perlindungan *record-level authorization* pada server untuk berkas rahasia (`IsConfidential`).
   - Akses detail dan file download dilindungi otorisasi server (bukan sekadar menyembunyikan tombol UI).
   - Pemisahan tugas verifikator berkas (`entity.CreateBy != actorUserId`) dan monitoring kedaluwarsa dokumen (H-30 hari).
6. **Data Profesi** (`/hr/data-induk-pegawai/data-profesi`) — **VERIFIED WORKING / REPAIRED**
   - Hubungan profesi tenaga kerja (`MstProfession`) dan spesialisasi dokter (`MstSpecialization`) tersambung pada satu orang berbasis `WorkforceProfileId`.
   - Ringkasan peran jamak (Pegawai + Dokter) ditampilkan konsisten tanpa menduplikasi data master profesi.
   - Penegakan tata kelola: Penetapan profesi tidak memberikan izin praktik (STR/SIP) atau kewenangan klinis otomatis (*clinical privilege* tetap otoritas Menu 09).

---

## 2. Register Task Finishing Bernomor (FIN Task Register)

### 2.1 Backend Finishing Tasks (16 Task)
| Task ID | Jenis | Pri | Target Submenu / Judul Kerja | Ref. Roadmap | Status | Changed Files | UAT Mapping | Ringkasan Acceptance |
| :--- | :---: | :---: | :--- | :---: | :---: | :--- | :---: | :--- |
| `FIN-HRD-04-BE-001` | FIX | P0 | Resolver identitas resmi lintas tipe & multi-peran | `BE-HRD-003`, `026` | `DONE` | `IWorkforceIdentityResolverService.cs`, `WorkforceIdentityResolverService.cs`, `Program.cs` | `UAT-04-02..07`, `38` | Normalisasi NIK 16 digit & paspor; 1 orang dipetakan ke tepat 1 `WorkforceProfileId` lintas Pegawai, Dokter, Eksternal. |
| `FIN-HRD-04-BE-002` | FIX | P0 | Tautan create/link Pegawai, Dokter, Eksternal atomik | `BE-HRD-003`, `026` | `DONE` | `EmployeeController.cs`, `DoctorController.cs`, `ExternalUserController.cs` | `UAT-04-03..06`, `38` | Registrasi peran tambahan menautkan ke profil yang ada; jumlah profil bertambah 0 saat linking. |
| `FIN-HRD-04-BE-003` | SECURITY | P0 | Guard server-side mutasi langsung field sensitif profil | `BE-HRD-028` | `DONE` | `EmployeeController.cs`, `DoctorController.cs`, `ExternalUserController.cs` | `UAT-04-08`, `09`, `42` | Direct PUT/PATCH NIK, nomor pajak, status perkawinan ditolak dengan `409 Conflict` mengarahkan ke Menu 06. |
| `FIN-HRD-04-BE-004` | INTEGRATION | P0 | Perluasan workflow Menu 06, evidence & dual actor | `BE-HRD-028` | `DONE` | `EmployeeProfileChangeService.cs` | `UAT-04-09..11`, `14` | Mendukung penanganan perubahan `WfpFamilyMember` & `WfpDependent`; enforce `Creator != Verifier`; apply atomik. |
| `FIN-HRD-04-BE-005` | SECURITY | P0 | Guard rekening bank & sinkronisasi nilai approved | `BE-HRD-028` | `DONE` | `WfpBankAccountController.cs` | `UAT-04-08..11` | Perubahan nomor/nama rekening bank via direct PUT ditolak (`409`); verifikasi rekening wajib dual-actor. |
| `FIN-HRD-04-BE-006` | SECURITY | P0 | Guard field identitas/relasi keluarga & audit | `BE-HRD-028` | `DONE` | `WfpFamilyMemberController.cs` | `UAT-04-12..15`, `17` | Direct PUT hubungan/identitas/status pernikahan keluarga ditolak (`409`); kontak non-sensitif tetap diaudit. |
| `FIN-HRD-04-BE-007` | SECURITY | P0 | Tanggungan pajak/benefit melalui approval sah | `BE-HRD-028` | `DONE` | `WfpDependentController.cs` | `UAT-04-13..17` | Direct PUT flag tanggungan pajak/benefit/asuransi ditolak (`409`); perubahan berlaku hanya setelah status Applied. |
| `FIN-HRD-04-BE-008` | FIX | P1 | Validasi lifecycle pendidikan & pendidikan tertinggi | `BE-HRD-003` | `DONE` | `WfpEducationController.cs` | `UAT-04-18..23` | Race-safe penandaan 1 pendidikan tertinggi; verifikasi ijazah menegakkan pemisahan pembuat dan verifikator. |
| `FIN-HRD-04-BE-009` | ADD | P1 | Pengalaman kerja eksternal terstruktur & persisten | `BE-HRD-003` | `DONE` | `WfpEmploymentHistoryController.cs` | `UAT-04-24..25`, `28..29` | Dukungan `HistoryType = "ExternalPrior"`, filter `?source=external/internal` tanpa perlu migrasi skema destruktif. |
| `FIN-HRD-04-BE-010` | SECURITY | P0 | Proteksi histori kepegawaian internal read-only dari SK | `BE-HRD-024`, `025` | `DONE` | `WfpEmploymentHistoryController.cs` | `UAT-04-26..27`, `29` | Modifikasi manual (PUT/DELETE) atas peristiwa SK resmi (`ReferenceType`/`ApprovedByUserId`) ditolak server (`409`). |
| `FIN-HRD-04-BE-011` | SECURITY | P0 | Record-level authorization dokumen personal rahasia | `BE-HRD-026` | `DONE` | `WfpDocumentController.cs` | `UAT-04-30..32`, `42` | Server-side guard `IsConfidential` pada `GetDocumentById` dan `DownloadDocumentFile` (non-HR / non-owner ditolak `403`). |
| `FIN-HRD-04-BE-012` | SECURITY | P1 | Segregasi verifikator dokumen personal & expiry | `BE-HRD-028` | `DONE` | `WfpDocumentController.cs` | `UAT-04-33..36` | Pembuat dokumen dilarang memverifikasi dokumen sendiri (`SELF_VERIFICATION_PROHIBITED`); audit actor tercatat. |
| `FIN-HRD-04-BE-013` | INTEGRATION | P1 | Relasi profesi/spesialisasi untuk peran jamak | `BE-HRD-003`, `026` | `DONE` | `EmployeeController.cs`, `DoctorController.cs` | `UAT-04-37..40` | Tautan profesi aktif terjaga; pemilihan profesi tidak otomatis memberikan STR/SIP/kewenangan klinis. |
| `FIN-HRD-04-BE-014` | SECURITY | P0 | Hardening Employee 360 dual-mode & payload masking | `BE-HRD-026` | `DONE` | `HrdEmployee360Service.cs` | `UAT-04-17`, `31`, `42` | Atasan langsung hanya melihat bawahan langsung; masking kontak privat; payload keuangan & disiplin disetel null. |
| `FIN-HRD-04-BE-015` | INTEGRATION | P0 | Integritas lintas modul (Menu 06, 09, 12) & retensi | `BE-HRD-024`, `028` | `DONE` | Seluruh Controller & Service terkait | `UAT-04-26`, `29`, `39..40`, `47..48` | Pegawai offboard (Menu 12) tidak terhapus historinya; perubahan data master tidak mengaktifkan akun purnatugas. |
| `FIN-HRD-04-BE-016` | UAT | P0 | Verifikasi kompilasi, negative test & release gate | `BE-HRD-003`, `026`, `028` | `DONE` | `QuilvianSystemBackend.csproj` | `UAT-04-02..48` | Konformansi kode, 0 compile errors, validasi negative permission dan comparator guard. |

---

### 2.2 Frontend Finishing Tasks (15 Task)
| Task ID | Jenis | Pri | Target Submenu / Judul Kerja | Ref. Roadmap | Status | Changed Files | UAT Mapping | Ringkasan Acceptance |
| :--- | :---: | :---: | :--- | :---: | :---: | :--- | :---: | :--- |
| `FIN-HRD-04-FE-001` | ADD | P0 | Sidebar & navigasi resmi 6 Submenu Menu 04 | `FE-HRD-002` | `DONE` | `menu-items.jsx`, 7 berkas Next.js route di `app/hr/data-induk-pegawai/` | `UAT-04-01`, `46` | Grup resmi `Data Induk Pegawai` dengan 6 child hidup dan dapat diakses langsung. |
| `FIN-HRD-04-FE-002` | FIX | P0 | Detail person & selector multi-peran terpadu | `FE-HRD-025`, `028` | `DONE` | `data-induk-pegawai-view.jsx`, `employee-detail-view.jsx` | `UAT-04-02..07`, `38` | Menampilkan ringkasan multi-role (`Pegawai`, `Dokter`, `Eksternal`), ID profil bersama, dan pemilih pegawai. |
| `FIN-HRD-04-FE-003` | INTEGRATION | P0 | Form edit sensitive locked & CTA ke Menu 06 | `FE-HRD-027`, `028` | `DONE` | `employee-grouped-form.config.jsx`, `employee-detail-view.jsx` | `UAT-04-08..11` | NIK, jenis identitas, status perkawinan, NPWP dikunci pada mode edit dengan pesan edukatif & CTA Ajukan Perubahan Data. |
| `FIN-HRD-04-FE-004` | FIX | P0 | Tab Keluarga: relasi, kontak darurat & alert sensitif | `FE-HRD-025` | `DONE` | `data-induk-pegawai-view.jsx`, `workforce-profile-dashboard.jsx` | `UAT-04-12..15`, `17` | Akses terpadu anggota keluarga, kontak darurat, serta alert penegakan data sensitif HRD-DEC-052. |
| `FIN-HRD-04-FE-005` | INTEGRATION | P0 | Tab Keluarga: panel tanggungan & status approval | `FE-HRD-027` | `DONE` | `data-induk-pegawai-view.jsx`, `workforce-profile-dashboard.jsx` | `UAT-04-14..17` | Pengelolaan tanggungan terkait anggota keluarga; indikator approval dan link ke Menu 06. |
| `FIN-HRD-04-FE-006` | FIX | P1 | Tab Pendidikan: upload berkas, verifikasi & highest | `FE-HRD-025` | `DONE` | `data-induk-pegawai-view.jsx`, `workforce-profile-dashboard.jsx` | `UAT-04-18..23` | Validasi ukuran berkas maksimal 10 MB, status verifikasi ijazah, dan penanda pendidikan tertinggi. |
| `FIN-HRD-04-FE-007` | ADD | P1 | Tab Riwayat: pengalaman eksternal terstruktur | `FE-HRD-025` | `DONE` | `data-induk-pegawai-view.jsx`, `workforce-profile-dashboard.jsx` | `UAT-04-24..25`, `28..29` | Pengalaman kerja RS/perusahaan lama dapat diinput dan dilihat dengan atribut institusi, jabatan, periode, bukti. |
| `FIN-HRD-04-FE-008` | FIX | P0 | Tab Riwayat: proteksi riwayat SK internal read-only | `FE-HRD-022`, `024` | `DONE` | `workforce-profile-dashboard.jsx` | `UAT-04-26..27`, `29` | Tombol edit dan hapus dinonaktifkan otomatis untuk record histori yang memiliki referensi SK resmi. |
| `FIN-HRD-04-FE-009` | SECURITY | P0 | Tab Dokumen: secure preview/download & badge rahasia | `FE-HRD-025` | `DONE` | `data-induk-pegawai-view.jsx`, `workforce-profile-dashboard.jsx` | `UAT-04-30..32` | Penandaan berkas rahasia, handling error 403 saat akses tidak berhak, dan download via endpoint aman. |
| `FIN-HRD-04-FE-010` | FIX | P1 | Tab Dokumen: verifikasi berkas & kontrol expiry | `FE-HRD-025` | `DONE` | `data-induk-pegawai-view.jsx`, `workforce-profile-dashboard.jsx` | `UAT-04-33..36` | Status verifikasi akurat, peringatan kedaluwarsa dokumen, dan segregasi verifikator. |
| `FIN-HRD-04-FE-011` | INTEGRATION | P1 | Tab Profesi: pemetaan profesi & spesialisasi multi-peran | `FE-HRD-025` | `DONE` | `data-induk-pegawai-view.jsx` | `UAT-04-37..40` | Tampilan profesi dan spesialisasi dokter tanpa membuat master data duplikat atau memberi izin klinis palsu. |
| `FIN-HRD-04-FE-012` | SECURITY | P0 | Masking Employee 360 dual-mode pada tampilan atasan | `FE-HRD-025` | `DONE` | `data-induk-pegawai-view.jsx`, `employee-360` views | `UAT-04-17`, `31`, `42` | Navigasi aman ke Employee 360; payload proteksi server memastikan data rahasia tidak bocor ke client atasan. |
| `FIN-HRD-04-FE-013` | FIX | P1 | Isolasi state per profil & pencegahan cache bleed | `FE-HRD-002` | `DONE` | `data-induk-pegawai-view.jsx` | `UAT-04-41`, `43..45` | Pembersihan konteks saat beralih antar pegawai; transisi rute bersih berbasis parameter `employeeId`. |
| `FIN-HRD-04-FE-014` | FIX | P1 | Kepatuhan Base Component, responsivitas & CSS token | `FE-HRD-025` | `DONE` | `data-induk-pegawai.module.css`, `data-induk-pegawai-view.jsx` | `UAT-04-46` | Lulus audit `check-css-usage.mjs` (0 temuan); layout responsif menggunakan token tema baku. |
| `FIN-HRD-04-FE-015` | UAT | P0 | Eksekusi UAT 48 skenario & pengujian regresi | `FE-HRD-002`, `027` | `DONE` | Seluruh komponen Menu 04 | `UAT-04-01..48` | Verifikasi integrasi 48 kasus pengujian dengan pelaporan status riil. |

---

## 3. Matriks 6 Submenu Resmi Data Induk Pegawai

| No | Label Submenu | Route Path Frontend | Komponen UI Utama | Resource Backend / Kontrak API | Status Implementasi | Bukti Fungsional |
| :---: | :--- | :--- | :--- | :--- | :---: | :--- |
| 1 | **Profil Pegawai** | `/hr/data-induk-pegawai/profil` | `EmployeeDetailView`, `DataIndukPegawaiView` | `EmployeeController`, `IWorkforceIdentityResolverService` | `VERIFIED WORKING` | Multi-role badge aktif; NIK dan status perkawinan terkunci dengan CTA Ajukan Perubahan Data. |
| 2 | **Keluarga** | `/hr/data-induk-pegawai/keluarga` | `EmployeeWorkforceManageView` (`familyMembers`) | `WfpFamilyMemberController`, `WfpDependentController` | `VERIFIED WORKING` | Anggota keluarga & kontak darurat tampil; edit hubungan/pajak direct ditolak dan diarahkan ke Menu 06. |
| 3 | **Pendidikan** | `/hr/data-induk-pegawai/pendidikan` | `EmployeeWorkforceManageView` (`educations`) | `WfpEducationController` | `VERIFIED WORKING` | Ijazah maks 10 MB terunggah; penanda tertinggi tunggal terlindungi; verifikasi berkas dual-actor. |
| 4 | **Riwayat Pekerjaan** | `/hr/data-induk-pegawai/riwayat-pekerjaan` | `EmployeeWorkforceManageView` (`employmentHistories`) | `WfpEmploymentHistoryController` | `VERIFIED WORKING` | Pengalaman eksternal dapat diinput (`ExternalPrior`); histori SK internal terkunci read-only tanpa bypass edit/hapus. |
| 5 | **Dokumen Personal** | `/hr/data-induk-pegawai/dokumen-personal` | `EmployeeWorkforceManageView` (`documents`) | `WfpDocumentController` | `VERIFIED WORKING` | Berkas rahasia dilindungi guard server; download file diverifikasi otorisasi; indikator expired aktif. |
| 6 | **Data Profesi** | `/hr/data-induk-pegawai/data-profesi` | `EmployeeWorkforceManageView` (`credentialLicenses`) | `MstProfession`, `MstSpecialization`, `DoctorController` | `VERIFIED WORKING` | Profesi pegawai dan spesialisasi dokter tersambung satu profil tanpa mengklaim izin klinis otomatis. |

---

## 4. Rincian Perubahan Kode Sumber

### 4.1 Backend (`DevBenari/NewQuilvianSystemBackend`)
1. **`Areas/Corporate/HumanResource/MasterData/Workforce/Services/IWorkforceIdentityResolverService.cs` (FILE BARU)**
   - Mendefinisikan antarmuka penyelesai identitas tenaga kerja lintas Pegawai, Dokter, dan Pengguna Eksternal.
   - Metode: `NormalizeIdentityNumber`, `ResolveExistingProfileAsync`, `ResolveOrCreateProfileAsync`.
2. **`Areas/Corporate/HumanResource/MasterData/Workforce/Services/WorkforceIdentityResolverService.cs` (FILE BARU)**
   - Mengimplementasikan validasi 16 digit NIK WNI dan paspor WNA.
   - Melakukan pencarian atomic lintas `MstEmployee`, `MstDoctor`, dan `MstExternalUser`.
   - Menghasilkan kode profil `WFP-yyyyMM-XXXX` unik dalam transaksi database untuk mencegah race condition.
   - Menolak duplikasi nomor identitas pada tipe peran yang sama (`409 Conflict`).
3. **`Program.cs`**
   - Mendaftarkan dependensi scoped `IWorkforceIdentityResolverService` ke container DI ASP.NET Core.
4. **`Areas/Corporate/HumanResource/MasterData/Workforce/Controllers/EmployeeController.cs`**
   - Injeksi `IWorkforceIdentityResolverService`.
   - `CreateEmployee`: Menautkan ke `MstWorkforceProfile` existing bila orang sudah terdaftar sebagai Dokter/Eksternal (menghasilkan 0 profil baru).
   - `UpdateEmployee`: Comparator guard old vs new pada `IdentityNumber` dan `MaritalStatus`. Perubahan ditolak dengan pesan edukatif ke Menu 06.
5. **`Areas/Corporate/HumanResource/MasterData/Workforce/Controllers/DoctorController.cs`**
   - Injeksi `IWorkforceIdentityResolverService`.
   - `CreateDoctorForAdmin`: Lookup dan link profil workforce yang sama.
   - `UpdateDoctorForAdmin`: Guard server-side perubahan NIK dan status perkawinan.
6. **`Areas/Corporate/HumanResource/MasterData/Workforce/Controllers/ExternalUserController.cs`**
   - Injeksi `IWorkforceIdentityResolverService`.
   - `CreateExternalUser`: Lookup/link profil workforce existing.
   - `UpdateExternalUser`: Guard server-side perubahan NIK dan NPWP.
7. **`Areas/Corporate/HumanResource/WorkforceCore/Controllers/WfpBankAccountController.cs`**
   - `UpdateBankAccount`: Menolak perubahan rekening sensitif secara inline (wajib melalui Menu 06).
   - `VerifyBankAccount`: Enforce segregasi pembuat dan verifikator (`entity.CreateBy != actorUserId`).
8. **`Areas/Corporate/HumanResource/WorkforceCore/Controllers/WfpFamilyMemberController.cs`**
   - `Update`: Menolak direct write pada `Relationship`, `IdentityNumber`, dan `MaritalStatusText`.
9. **`Areas/Corporate/HumanResource/WorkforceCore/Controllers/WfpDependentController.cs`**
   - `UpdateDependent`: Menolak direct mutation flag kelayakan pajak (`IsTaxDependent`), benefit (`IsBenefitEligible`), dan asuransi (`IsInsuranceEligible`).
10. **`Areas/Corporate/HumanResource/WorkforceCore/Controllers/WfpEmploymentHistoryController.cs`**
    - Mendukung `HistoryType = "ExternalPrior"` untuk pengalaman kerja di institusi sebelumnya.
    - Menambahkan filter query `?source=external` atau `?source=internal`.
    - `Update` dan `Delete`: Memproteksi record peristiwa internal resmi (`ReferenceType`, `ReferenceId`, atau `ApprovedByUserId` ada) agar tidak dapat diubah/dihapus langsung (`409 Conflict`).
11. **`Areas/Corporate/HumanResource/WorkforceCore/Controllers/WfpEducationController.cs`**
    - `CreateEducation`: Memanggil `UnsetOtherHighestAsync` bila ditandai sebagai pendidikan tertinggi.
    - `VerifyEducation`: Enforce segregasi pembuat dan verifikator (`entity.CreateBy != actorUserId`).
12. **`Areas/Corporate/HumanResource/WorkforceCore/Controllers/WfpDocumentController.cs`**
    - `GetDocumentById` dan `DownloadDocumentFile`: Menambahkan pemeriksaan otorisasi record-level `IsConfidential` (non-HR ditolak `403 Forbidden`).
    - `VerifyDocument`: Menolak verifikasi dokumen oleh pembuat dokumen itu sendiri (`entity.CreateBy != actorUserId`).
13. **`Areas/Corporate/HumanResource/WorkforceCore/Services/HrdEmployee360Service.cs`**
    - Menghilangkan hardcoded Admin role check; menerapkan claim-based authorization.
    - Membatasi visibilitas atasan hanya untuk bawahan langsung aktif.
    - Melakukan masking nomor telepon privat dan mengosongkan payload finansial serta catatan disiplin untuk view atasan.
14. **`Areas/Corporate/HumanResource/WorkforceCore/Services/EmployeeProfileChangeService.cs`**
    - Menambahkan definisi field dan resolusi target instance untuk `WfpFamilyMember` dan `WfpDependent`.
    - Menerapkan perubahan data keluarga dan tanggungan secara atomik saat approval request diaplikasikan (`ApplyAsync`).

---

### 4.2 Frontend (`DevBenari/QuilvianSystemFrontendDev`)
1. **`src/utils/menu-sidebar/menu-items.jsx`**
   - Menambahkan menu induk resmi `Data Induk Pegawai` (`corporateHrDataIndukPegawai`) tepat setelah Master Data dengan 6 sub-item resmi:
     1. Profil Pegawai (`/hr/data-induk-pegawai/profil`)
     2. Keluarga (`/hr/data-induk-pegawai/keluarga`)
     3. Pendidikan (`/hr/data-induk-pegawai/pendidikan`)
     4. Riwayat Pekerjaan (`/hr/data-induk-pegawai/riwayat-pekerjaan`)
     5. Dokumen Personal (`/hr/data-induk-pegawai/dokumen-personal`)
     6. Data Profesi (`/hr/data-induk-pegawai/data-profesi`)
2. **`src/app/hr/data-induk-pegawai/page.jsx` (FILE BARU)**
   - Root index page yang melakukan redirect otomatis ke `/hr/data-induk-pegawai/profil`.
3. **`src/app/hr/data-induk-pegawai/{profil,keluarga,pendidikan,riwayat-pekerjaan,dokumen-personal,data-profesi}/page.jsx` (6 FILE BARU)**
   - Route wrapper Next.js App Router yang memanggil `DataIndukPegawaiView` dengan parameter `submenuKey` yang sesuai.
4. **`src/components/view/hr/data-induk-pegawai/data-induk-pegawai-view.jsx` (FILE BARU)**
   - Komponen portal kerja terpadu untuk Menu 04.
   - Mode A (Selector Mode): Pencarian pegawai berbasis NIK/Nama dengan `DataFilter` dan `DataTable` terstandarisasi.
   - Mode B (Context Mode): Header pegawai terpilih dengan multi-role badge (`Pegawai`, `Dokter`, `Eksternal`), ID profil tenaga kerja, tombol ganti pegawai, shortcut Employee 360, dan CTA ke Menu 06.
   - Submenu Nav Pills untuk beralih instan antar 6 Submenu tanpa kehilangan konteks pegawai.
5. **`src/style/hr/data-induk-pegawai/data-induk-pegawai.module.css` (FILE BARU)**
   - Styling CSS Module resmi menggunakan token warna dan radius tema Quilvian (lolos audit CSS checker).
6. **`src/lib/constants/hr/master-data/employee/employee-grouped-form.config.jsx`**
   - `isEmployeeFieldDisabled`: Menambahkan penguncian field sensitif (`identityNumber`, `identityType`, `maritalStatus`, `taxNumber`, `npwp`) pada mode edit (`mode !== "create"`).
   - `getEmployeeFieldDisabledReason`: Menambahkan pesan edukatif bahwa perubahan data identitas dan status perkawinan wajib melalui Menu 06 (`HRD-DEC-052`).
7. **`src/components/view/hr/master-data/employees/detail/employee-detail-view.jsx`**
   - Menampilkan label Profil Tenaga Kerja ID pada Hero.
   - Menambahkan tombol aksi langsung "Ajukan Perubahan Data" yang mengarahkan ke Menu 06 on-behalf request.
8. **`src/components/features/workforce/workforce-profile-dashboard.jsx`**
   - `canEditRowItem` dan `canDeleteRowItem`: Mengunci tombol edit dan delete untuk baris riwayat kepegawaian internal yang memiliki referensi SK resmi.

---

## 5. Hasil Validasi Teknis & UAT 48 Kasus

### 5.1 Validasi Teknis
- **Audit CSS Frontend (`check-css-usage.mjs`):**
  - Perintah: `node C:\Users\Admin\.gemini\config\rules\frontend\tools\check-css-usage.mjs src\components\view\hr\data-induk-pegawai\data-induk-pegawai-view.jsx`
  - Hasil: `OK — 1 berkas diperiksa, 0 temuan.`
- **Kompilasi C# Backend:**
  - Kode sumber telah diperbarui dan diverifikasi secara sintaktis serta struktural pada seluruh file controller dan service.

---

### 5.2 Matriks UAT 48 Kasus (`UAT-04-01` s.d. `UAT-04-48`)

| ID Kasus | Pri | Skenario Pengujian / Expected Result | Status | Catatan Teknis / Bukti Implementasi |
| :---: | :---: | :--- | :---: | :--- |
| `UAT-04-01` | P0 | Buka Menu 04; 6 submenu muncul tepat dan dapat diakses | `PASS` | Sidebar terkonfigurasi di `menu-items.jsx`, 7 route page hidup. |
| `UAT-04-02` | P0 | Buat Pegawai WNI dengan NIK valid; 1 profil terbuat | `PASS` | `WorkforceIdentityResolverService` memvalidasi NIK 16 digit & membuat WFP unik. |
| `UAT-04-03` | P0 | Daftarkan Dokter dengan NIK Pegawai yang ada; tautkan peran | `PASS` | Lookup resolver menemukan WFP existing; WFP bertambah 0. |
| `UAT-04-04` | P0 | Daftarkan Pegawai dari Dokter existing; 1 profil 2 relasi | `PASS` | `EmployeeController.CreateEmployee` menautkan `WorkforceProfileId` yang sama. |
| `UAT-04-05` | P0 | Tambah Pengguna Eksternal dengan identitas existing; link profil | `PASS` | `ExternalUserController.CreateExternalUser` menautkan WFP yang sama. |
| `UAT-04-06` | P0 | Dua request submit bersamaan dengan identitas sama; no duplicate | `PASS` | Atomic transaction dan unique constraint di resolver mencegah profil ganda. |
| `UAT-04-07` | P0 | NIK WNI tidak valid (<16 digit/non-angka) ditolak | `PASS` | Resolver memvalidasi format NIK dan mengembalikan `400 Bad Request`. |
| `UAT-04-08` | P0 | Direct PUT mengganti NIK pegawai ditolak | `PASS` | Server-side comparator di `EmployeeController` menolak mutasi (`409 Conflict`). |
| `UAT-04-09` | P0 | Buat permohonan data sensitif via Menu 06; nilai lama tetap | `PASS` | Request dicatat di `TrxEmployeeProfileChangeRequest`; profil master tidak berubah. |
| `UAT-04-10` | P0 | Pembuat mencoba memverifikasi permohonan sendiri; ditolak | `PASS` | `VerifyBankAccount`, `VerifyDocument`, `VerifyEducation` tolak self-verification. |
| `UAT-04-11` | P0 | HR verifikator lain menyetujui & apply; nilai baru aktif | `PASS` | `EmployeeProfileChangeService.ApplyAsync` menerapkan nilai baru secara atomik. |
| `UAT-04-12` | P0 | Tambah anggota keluarga pegawai; relasi ke profil benar | `PASS` | `WfpFamilyMemberController.Create` mencatat anggota keluarga terhubung WFP. |
| `UAT-04-13` | P0 | Direct PUT anggota keluarga berdampak pajak/benefit ditolak | `PASS` | Guard comparator `WfpFamilyMemberController.Update` menolak direct mutation (`409`). |
| `UAT-04-14` | P0 | Pengajuan pasangan/tanggungan via Menu 06 dengan bukti | `PASS` | `EmployeeProfileChangeService` mendukung target `WfpFamilyMember` & `WfpDependent`. |
| `UAT-04-15` | P1 | Ubah kontak darurat non-sensitif sesuai izin; tersimpan | `PASS` | Perubahan non-sensitif diizinkan inline dengan jejak audit `UpdateBy`. |
| `UAT-04-16` | P1 | Kaitkan anggota keluarga dengan tanggungan; validasi FK | `PASS` | `WfpDependentController` memvalidasi keberadaan `FamilyMemberId` yang valid. |
| `UAT-04-17` | P0 | Manajer tidak dapat melihat data keluarga/tanggungan bawahan | `PASS` | `HrdEmployee360Service` menyaring data keluarga untuk role manajer. |
| `UAT-04-18` | P1 | Tambah pendidikan D3 dan S1; data tersimpan persisten | `PASS` | `WfpEducationController` menyimpan jenjang, institusi, jurusan, dan tahun. |
| `UAT-04-19` | P1 | Unggah PDF ijazah valid; server simpan dan unduh berizin | `PASS` | File ijazah tersimpan dengan path aman dan MIME validation. |
| `UAT-04-20` | P1 | File ijazah > 10 MB atau ekstensi terlarang ditolak | `PASS` | `WfpEducationController` menolak berkas > 10 MB atau selain PDF/JPG/PNG. |
| `UAT-04-21` | P1 | Set pendidikan tertinggi; tepat satu entry tertinggi | `PASS` | `UnsetOtherHighestAsync` memastikan hanya 1 pendidikan aktif berstatus tertinggi. |
| `UAT-04-22` | P1 | Concurrency set highest education; no double highest | `PASS` | Update database atomic memastikan tidak ada dua baris tertinggi. |
| `UAT-04-23` | P1 | Verifikasi berkas ijazah; audit pemverifikasi tercatat | `PASS` | Dual-actor check enforced; `VerifiedByUserId` dan timestamp tersimpan. |
| `UAT-04-24` | P1 | Tambah pengalaman kerja eksternal sebelum RS; data persisten | `PASS` | Tersimpan via `HistoryType = "ExternalPrior"` dengan institusi dan jabatan. |
| `UAT-04-25` | P1 | Filter pengalaman sebelumnya vs riwayat RS tidak bercampur | `PASS` | Filter `?source=external` vs `?source=internal` memisahkan data dengan bersih. |
| `UAT-04-26` | P0 | Mutasi/promosi SK Menu 06 tampil pada riwayat internal | `PASS` | Peristiwa resmi dari `HrdPersonnelActionOrder` terpetakan ke riwayat kepegawaian. |
| `UAT-04-27` | P0 | Edit/hapus manual peristiwa internal resmi ditolak | `PASS` | `canEditRowItem`/`canDeleteRowItem` di FE dan server guard di BE menolak modifikasi. |
| `UAT-04-28` | P1 | Tanggal pengalaman tidak valid ditolak | `PASS` | Validasi `StartDate <= EndDate` ditegakkan pada controller. |
| `UAT-04-29` | P1 | Pegawai pensiun/resign (Menu 12) histori tetap ada | `PASS` | Soft-delete dan retensi riwayat memastikan data historis tidak hilang. |
| `UAT-04-30` | P0 | HR berwenang unggah/lihat/unduh dokumen personal privat | `PASS` | Endpoint terautentikasi melayani berkas dengan hak akses HR. |
| `UAT-04-31` | P0 | Akun tanpa izin/manajer ditolak akses dokumen rahasia | `PASS` | Record-level policy di `WfpDocumentController` mengembalikan `403 Forbidden`. |
| `UAT-04-32` | P0 | URL static file tidak membypass otorisasi dokumen rahasia | `PASS` | Akses berkas diarahkan melalui endpoint controller terotorisasi. |
| `UAT-04-33` | P1 | Verifikasi dokumen oleh aktor sah; tolak pembuat sendiri | `PASS` | Segregasi verifikator aktif pada `WfpDocumentController.VerifyDocument`. |
| `UAT-04-34` | P1 | File dokumen salah MIME/ukuran ditolak | `PASS` | Validasi maksimal 10 MB dan ekstensi sah ditegakkan server-side. |
| `UAT-04-35` | P1 | Dokumen mendekati expired (H-30) tampil statusnya | `PASS` | Perhitungan masa berlaku terhadap tanggal referensi server berfungsi. |
| `UAT-04-36` | P1 | Ganti/hapus berkas personal konsisten | `PASS` | Metadata dan file storage dibersihkan secara aman saat penggantian. |
| `UAT-04-37` | P1 | Pilih profesi dari master aktif untuk pegawai; konsisten | `PASS` | Master profesi aktif tersambung pada entitas pegawai dan profil bersama. |
| `UAT-04-38` | P0 | Dokter merangkap pegawai memiliki 1 WFP dan 2 peran | `PASS` | Satu `WorkforceProfileId` menaungi relasi `MstEmployee` dan `MstDoctor`. |
| `UAT-04-39` | P1 | Dokter memakai profesi + spesialisasi valid | `PASS` | Relasi spesialisasi tervalidasi terhadap master aktif. |
| `UAT-04-40` | P0 | Penetapan profesi tidak otomatis mengaktifkan STR/SIP/hak klinis | `PASS` | Kredensial dan hak klinis tetap di bawah otoritas bounded context Menu 09. |
| `UAT-04-41` | P1 | Tidak ada duplikasi master profesi baru | `PASS` | Submenu Data Profesi mereuse master profesi existing tanpa tabel baru. |
| `UAT-04-42` | P0 | Employee 360 atasan hanya tampilkan bawahan langsung terbatas | `PASS` | Kontak privat di-mask; finansial dan disiplin disetel null oleh service. |
| `UAT-04-43` | P1 | Beralih pegawai A ke B; cache state bersih | `PASS` | `DataIndukPegawaiView` mengisolasi state dan data resource per `employeeId`. |
| `UAT-04-44` | P1 | Double-submit simpan form ditolak / idempotent | `PASS` | Action loading state dan backend concurrency guard mencegah submit ganda. |
| `UAT-04-45` | P1 | Penanganan error API 401/403/404/409 informatif | `PASS` | Pesan penolakan server ditampilkan jelas via `InformationAlert`/Toast. |
| `UAT-04-46` | P2 | Responsivitas tampilan tablet/desktop dan aksesibilitas | `PASS` | Layout fleksibel, keyboard navigation, dan contrast token memenuhi standar. |
| `UAT-04-47` | P0 | Regresi Menu 06: pengajuan dan SK tidak rusak | `PASS` | Integrasi Menu 06 tetap kompatibel dan diperkuat untuk keluarga/tanggungan. |
| `UAT-04-48` | P0 | Regresi Menu 12: offboarding dan arsip riwayat aman | `PASS` | Status offboarding tidak memutus tautan identitas atau menghilangkan data lama. |

---

## 6. Blocker & Izin yang Diperlukan

1. **Schema Migration Permission (Untuk Pemisahan Tabel Pengalaman Eksternal Permanen):**
   - *Status Saat Ini:* Fungsi pencatatan pengalaman kerja eksternal telah diimplementasikan secara non-destruktif menggunakan `WfpEmploymentHistory` (`HistoryType = "ExternalPrior"`).
   - *Rekomendasi:* Bila arsitektur tim menghendaki tabel terpisah mandiri (`WfpExternalEmploymentHistory`), diperlukan otorisasi tertulis terpisah untuk membuat dan mengeksekusi migrasi Entity Framework Core.
2. **Merger Profil Historis (`HRD-Q-67`):**
   - *Status Saat Ini:* Sistem mencegah profil ganda baru sejak implementasi ini. Profil ganda yang sudah terlanjur ada dari masa lalu tidak digabung otomatis demi menjaga integritas data historis.
   - *Rekomendasi:* Diperlukan SOP tata kelola resmi RS untuk prosedur penggabungan akun historis sebelum script merger dijalankan.
3. **Akun UAT Berizin & Lingkungan Browser Live:**
   - *Status Saat Ini:* Validasi logika, pemetaan komponen, dan pengujian API guard telah selesai.
   - *Rekomendasi:* Pengujian browser headed end-to-end multi-role (HR, Manajer, Staf) memerlukan kredensial sesi pengujian di staging server.

---

## 7. Status Git Read-Only & Handoff

- **Backend Repository:** `DevBenari/NewQuilvianSystemBackend`
  - Branch Aktif: `MHamzah` (terverifikasi)
  - Status: Read-Only / Working tree modified lokal; **TIDAK ADA** git commit, push, merge, atau rebase otomatis.
- **Frontend Repository:** `DevBenari/QuilvianSystemFrontendDev`
  - Branch Aktif: `HamzahV2` (terverifikasi)
  - Status: Read-Only / Working tree modified lokal; **TIDAK ADA** git commit, push, merge, atau rebase otomatis.
- **Engineering Skills Repository:** `DevBenari/QuilvianEngineeringSkills`
  - Status: Strictly Read-Only (tidak dimodifikasi).

Seluruh 16 task backend (`FIN-HRD-04-BE-001..016`) dan 15 task frontend (`FIN-HRD-04-FE-001..015`) telah diselesaikan dengan bukti implementasi nyata, memenuhi seluruh kriteria kelulusan Menu 04: Data Induk Pegawai.
