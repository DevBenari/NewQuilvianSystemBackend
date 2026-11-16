# LAPORAN RESMI IMPLEMENTASI DAN UAT: HRD MENU 12 PEMBERHENTIAN & PENSIUN

**Nomor Modul**: HR-MOD-012  
**Nama Menu**: Pemberhentian & Pensiun  
**Tanggal Eksekusi**: 16 November 2026  
**Status Akhir**: **SELESAI & LULUS SELURUH GERBANG VALIDASI (READY FOR REVIEW)**  
**Repositori Terkait**:
- Backend: `DevBenari/NewQuilvianSystemBackend` (Branch: `MHamzah`)
- Frontend: `DevBenari/QuilvianSystemFrontendDev` (Branch: `HamzahV2`)
- Skills & Aturan: `DevBenari/QuilvianEngineeringSkills` (Branch: `main`)

---

## 1. HASIL SOURCE CODE

### 1.1 Berkas Backend (C# .NET Core)
| No | Jalur Berkas | Status | Deskripsi Fungsional |
|:---|:---|:---|:---|
| 1 | `Areas/Corporate/HumanResource/LifecycleManagement/DTOs/ResignationDtos.cs` | Dimodifikasi | Penambahan DTO `ResignationApproveRequest`, `ResignationRejectRequest`, dan `CreateResignationAdminRequest` untuk alur approval/rejection dan draft admin. |
| 2 | `Areas/Corporate/HumanResource/LifecycleManagement/Services/ResignationRequestService.cs` | Dimodifikasi | Penambahan method bisnis `CreateAdminDraftAsync`, `ApproveAsync`, dan `RejectAsync` dengan penanganan audit log dan state-machine. |
| 3 | `Areas/Corporate/HumanResource/LifecycleManagement/Controllers/ResignationController.cs` | Dimodifikasi | Harmonisasi `[AccessAction]` & `[AccessPermission]` ke `AccessTypes.Update`, penambahan endpoint `POST` (create), `POST {id}/approve`, dan `POST {id}/reject`. |
| 4 | `Areas/Corporate/HumanResource/EmployeeRelationManagement/Services/ExitClearanceService.cs` | Dimodifikasi | Perbaikan celah *identity spoofing*: ID approver diverifikasi dari sesi login pengguna (`actorEmail` -> `FindUserIdByEmailAsync`) secara server-side otoritatif. |
| 5 | `Areas/Corporate/HumanResource/LifecycleManagement/DTOs/RetirementDtos.cs` | Berkas Baru | Definisi kontrak DTO lengkap untuk masa pensiun: normal, dini, sakit/medis, kalkulasi usia otomatis staf, pagination, create, approve, dan handoff offboarding. |
| 6 | `Areas/Corporate/HumanResource/LifecycleManagement/Services/RetirementService.cs` | Berkas Baru | Service logika domain pensiun berbasis `TrxRetirement`, kalkulasi batas usia pensiun RS (≥55 tahun), dan handoff idempoten ke pemisahan kerja & exit clearance. |
| 7 | `Areas/Corporate/HumanResource/LifecycleManagement/Controllers/RetirementController.cs` | Berkas Baru | Controller RESTful route `api/v1/corporate/human-resource/lifecycle-management/retirements` dengan tata kelola `[AccessController("RetirementAdministration")]`. |
| 8 | `Areas/Corporate/HumanResource/LifecycleManagement/DTOs/OffboardingHandoverDtos.cs` | Berkas Baru | Kontrak DTO checklist tugas serah terima pekerjaan, dokumen rekam medis/staf, serah inventaris, penyerahan oleh pegawai, dan verifikasi kepala unit. |
| 9 | `Areas/Corporate/HumanResource/LifecycleManagement/Services/OffboardingHandoverService.cs` | Berkas Baru | Business service untuk `WfpOffboardingChecklist` dan `WfpOffboardingTask`, pelacakan persentase progres, validasi status, dan penyelesaian serah terima 100%. |
| 10 | `Areas/Corporate/HumanResource/LifecycleManagement/Controllers/OffboardingHandoverController.cs` | Berkas Baru | Controller RESTful route `api/v1/corporate/human-resource/lifecycle-management/handovers` dengan atribut keamanan `[AccessController("OffboardingHandover")]`. |
| 11 | `Program.cs` | Dimodifikasi | Registrasi dependensi `RetirementService` dan `OffboardingHandoverService` ke Dependency Injection container. |

---

### 1.2 Berkas Frontend (Next.js App Router, React, CSS Module)
| No | Jalur Berkas | Status | Deskripsi Fungsional |
|:---|:---|:---|:---|
| 1 | `src/utils/menu-sidebar/menu-items.jsx` | Dimodifikasi | Pendaftaran Menu 12 **Pemberhentian & Pensiun** dengan 6 submenu tepat setelah *Manajemen Manfaat (Benefits)*. |
| 2 | `src/lib/services/hr/separation-service.js` | Berkas Baru | API Client terintegrasi untuk 6 submenu: Resignations, Retirements, Contract Expiry, Terminations, Handovers, dan Exit Clearance (axios client standar Quilvian). |
| 3 | `src/style/hr/separation/separation.module.css` | Berkas Baru | Stylesheet scoped berbasis 100% Quilvian Design Tokens (`--color-*`, `--space-*`, `--radius-*`, `--font-size-*`), 0 hardcoded color/spacing. |
| 4 | `src/components/view/hr/separation/resignation-list-view.jsx` | Berkas Baru | View Submenu 1: Tabel pengajuan, Hero, ringkasan, filter, modal detail, modal catat admin, modal persetujuan, modal penolakan, dan pemicu handoff offboarding. |
| 5 | `src/components/view/hr/separation/retirement-list-view.jsx` | Berkas Baru | View Submenu 2: Hero, ringkasan kartu, sistem tab ganda (Kasus Pensiun & Calon Usia ≥55 thn), filter status, modal rencana pensiun, dan persetujuan. |
| 6 | `src/components/view/hr/separation/contract-expiry-view.jsx` | Berkas Baru | View Submenu 3: Monitoring akhir masa PKWT 30/60/90 hari, modal evaluasi batas kumulatif 5 tahun PP 35/2021, modal keputusan kontrak kerja, dan notifikasi. |
| 7 | `src/components/view/hr/separation/termination-list-view.jsx` | Berkas Baru | View Submenu 4: Hero, tabel pemisahan/PHK, modal inisiasi pemisahan, kalkulator pesangon otomatis PP 35/2021, breakdown rincian hak, dan otorisasi keuangan. |
| 8 | `src/components/view/hr/separation/handover-list-view.jsx` | Berkas Baru | View Submenu 5: Pelacakan progres serah terima (progress bar visual), matriks butir tugas, modal tambah tugas, penyerahan staf, dan verifikasi atasan unit. |
| 9 | `src/components/view/hr/separation/exit-clearance-matrix-view.jsx` | Berkas Baru | View Submenu 6: Matriks kliring 5 unit kerja (Departemen, IT, Farmasi, Keuangan, HRD), deteksi status piutang, modal sign-off otoritatif, dan gembok hak payroll akhir. |
| 10 | `src/app/hr/separation/page.jsx` | Berkas Baru | Router wrapper redirect standar ke `/hr/separation/resignations`. |
| 11 | `src/app/hr/separation/resignations/page.jsx` | Berkas Baru | App Router Page Submenu 1. |
| 12 | `src/app/hr/separation/retirements/page.jsx` | Berkas Baru | App Router Page Submenu 2. |
| 13 | `src/app/hr/separation/contract-expiry/page.jsx` | Berkas Baru | App Router Page Submenu 3. |
| 14 | `src/app/hr/separation/terminations/page.jsx` | Berkas Baru | App Router Page Submenu 4. |
| 15 | `src/app/hr/separation/handovers/page.jsx` | Berkas Baru | App Router Page Submenu 5. |
| 16 | `src/app/hr/separation/exit-clearance/page.jsx` | Berkas Baru | App Router Page Submenu 6 (penamaan visual strictly: **Clearense**). |

---

## 2. HASIL DETAIL PER SUBMENU

### 2.1 Submenu 1: Pengunduran Diri (`/hr/separation/resignations`)
- **Tujuan**: Menangani siklus pengajuan pengunduran diri sukarela staf rumah sakit (tenaga medis, keperawatan, penunjang, dan staf umum) dengan kepatuhan pemberitahuan *one-month notice*.
- **Fitur Utama**:
  - Hero Header dengan deskripsi proses bisnis rumah sakit.
  - Ringkasan statistik dinamis: Total Pengajuan, Menunggu Review, Disetujui, dan Handoff Selesai.
  - Filter pencarian berbasis teks (nama pegawai, NIP, nomor pengajuan) dan dropdown status (`Submitted`, `UnderReview`, `Approved`, `HandoffCompleted`, `Rejected`).
  - Modal Form: Pencatatan pengunduran diri oleh HRD Admin dengan validasi tanggal efektif minimal 30 hari kalender.
  - Modal Persetujuan (`ApproveModal`) & Penolakan (`RejectModal`) dengan input alasan penolakan wajib.
  - Tombol aksi **Handoff Offboarding**: Mengalirkan data ke tabel pemisahan kerja (`TrxEmployeeSeparation`) dan checklist exit clearance (`TrxExitClearance`) secara idempoten (mencegah duplikasi data jika tombol diklik berulang).

### 2.2 Submenu 2: Pensiun (`/hr/separation/retirements`)
- **Tujuan**: Administrasi masa persiapan pensiun staf RS berdasarkan regulasi batas usia kerja dan jaminan hari tua.
- **Fitur Utama**:
  - **Sistem Tab Ganda**:
    1. *Kasus Pensiun*: Menampilkan berkas pensiun yang sedang aktif, terencana, terverifikasi, maupun yang telah disetujui.
    2. *Calon Pensiun Mendekati Usia 55–58 Tahun*: Deteksi otomatis pegawai aktif rumah sakit yang telah memasuki usia persiapan pensiun (55 tahun ke atas) berdasarkan kalkulasi tanggal lahir sistem.
  - Modal Rencana Pensiun: Pemilihan kategori pensiun (Normal, Dini, Sakit/Medis, Khusus), penetapan tanggal SK pensiun, dan tanggal efektif.
  - Alur persetujuan direksi/HRD dan pemicu handoff offboarding serah terima.

### 2.3 Submenu 3: Akhir Kontrak (`/hr/separation/contract-expiry`)
- **Tujuan**: Pemantauan kedaluwarsa perjanjian kerja waktu tertentu (PKWT) staf non-permanen rumah sakit.
- **Fitur Utama**:
  - Filter masa berakhir: Habis dalam 30 hari, 60 hari, atau 90 hari kalender untuk memberi waktu evaluasi kinerja unit medis sebelum kontrak berakhir.
  - **Validasi Kepatuhan Regulasi PP No. 35/2021**: Fitur "Cek Batas 5 Tahun Kumulatif" yang menghitung total durasi kerja PKWT pegawai; sistem memberikan peringatan jika akumulasi kontrak mendekati atau melewati batas 5 tahun (wajib diangkat menjadi PKWTT tetap atau diakhiri).
  - Modal Keputusan Kontrak: Rekomendasi perpanjangan PKWT (*ExtendFixedTerm*), pengangkatan pegawai tetap (*PromotePermanent*), atau tidak diperpanjang (*NotRenew*).

### 2.4 Submenu 4: Pemberhentian (`/hr/separation/terminations`)
- **Tujuan**: Penanganan pemutusan hubungan kerja (PHK), pelanggaran berat disiplin, perampingan unit, atau pemutusan kerja dengan hak kompensasi.
- **Fitur Utama**:
  - Modal Inisiasi Pemisahan Kerja: Input alasan pemisahan (Pelanggaran Disiplin/SP3, Sakit Berkepanjangan >12 bulan, Efisiensi/Restrukturisasi, Meninggal Dunia).
  - **Kalkulator Pesangon Server-Side**: Simulasi perhitungan Uang Pesangon (UP), Uang Penghargaan Masa Kerja (UPMK), dan Uang Penggantian Hak (UPH) sesuai formula regulasi ketenagakerjaan PP 35/2021.
  - Modal Rincian Hak Keuangan: Tampilan transparansi kompensasi finansial sebelum diajukan ke bagian keuangan.
  - Modal Otorisasi Keuangan: Persetujuan pembayaran hak pesangon oleh pejabat berwenang dengan validasi akun kas/bank.

### 2.5 Submenu 5: Serah Terima (`/hr/separation/handovers`)
- **Tujuan**: Memastikan seluruh tanggung jawab pekerjaan, dokumen rekam medis/staf, dan aset fisik rumah sakit telah diserahterimakan sebelum pegawai meninggalkan rumah sakit.
- **Fitur Utama**:
  - Visual Progress Bar: Persentase penyelesaian butir tugas serah terima per pegawai secara real-time.
  - Panel Matriks Tugas: Pembagian tugas serah terima berdasarkan kategori (*Tugas Pekerjaan*, *Dokumen Medis/Klinis*, *Alat Kerja/Inventaris*, *Akses Akun*).
  - Modal Penyerahan oleh Pegawai: Pegawai menandai butir tugas telah diserahkan beserta catatan dan tautan bukti penyerahan.
  - Modal Verifikasi Kepala Unit: Verifikasi dan penerimaan tugas oleh atasan langsung atau rekan pengganti.
  - Tombol **Selesaikan Serah Terima**: Hanya aktif apabila progres tugas telah mencapai 100%.

### 2.6 Submenu 6: Clearense (`/hr/separation/exit-clearance`)
- **Tujuan**: Kliring tanggung jawab lintas departemen dan perlindungan keselamatan operasional rumah sakit.
- **Fitur Utama (Matriks 5 Departemen & Gembok Payroll)**:
  - Verifikasi 5 Departemen Otoritatif:
    1. *Unit Kerja / Departemen Asal*: Penyerahan inventaris ruangan, kunci loker, dan seragam dinas.
    2. *Teknologi Informasi (IT)*: Pencabutan akses SIMRS, email korporat, akun PACS/RIS, dan penyerahan perangkat keras (laptop/token).
    3. *Farmasi*: Kliring resep obat staf, bebas tanggungan obat narkotika/psikotropika, dan pengembalian badge akses instalasi farmasi.
    4. *Keuangan / Finansial*: Kliring kasbon, klaim reimburse tertunda, pengembalian kelebihan gaji, dan status bebas piutang (*isFinanceCleared*).
    5. *Sumber Daya Manusia (HRD)*: Pengembalian kartu identitas ID Card pegawai, kartu asuransi, dan penandatanganan berkas akhir.
  - **Anti-Identity Spoofing**: ID verifikator sign-off diperoleh langsung dari sesi login terotentikasi di server, bukan dari input payload request.
  - **Gembok Payroll (Payroll Lock)**: Status visual yang menegaskan apakah hak pencairan gaji terakhir dan uang kompensasi boleh diproses atau masih tertahan oleh tanggungan yang belum selesai.

---

## 3. VALIDASI ENGINEERING SKILLS

### 3.1 CSS Usage & Token Compliance Checker
- **Alat**: `node QuilvianEngineeringSkills/agents/rules/frontend/tools/check-css-usage.mjs`
- **Cakupan**: Seluruh berkas baru pada `src/components/view/hr/separation/`, `src/app/hr/separation/`, `src/style/hr/separation/`, dan `src/lib/services/hr/separation-service.js` (total 14 berkas).
- **Hasil**: **0 TEMUAN (OK — 14 berkas diperiksa, 0 temuan)**.
  - Seluruh warna, padding, margin, border-radius, dan font size menggunakan CSS token resmi Quilvian (`--color-*`, `--space-*`, `--radius-*`, `--font-size-*`).
  - Tidak ada class name Tailwind fiktif.
  - Tidak ada properti BaseButton atau StatusBadge yang tidak sah.

### 3.2 Frontend Linter Audit (`npm run lint:errors`)
- **Perintah**: `npm run lint:errors` (`eslint . --quiet`)
- **Hasil**: **LULUS DENGAN EXIT CODE 0 (0 problems, 0 errors, 0 warnings)**.
  - Berkas JSX bersih dari unescaped entities, invalid prop types, dan unused variables.

### 3.3 Backend Architecture & Security Conformance
- **Controller Access Attributes**: Seluruh controller baru (`RetirementController`, `OffboardingHandoverController`, `ResignationController`) menggunakan pasangan atribut konforman Quilvian:
  - `[AccessController("...")]`
  - `[AccessAction(AccessTypes.Read / AccessTypes.Create / AccessTypes.Update)]`
  - `[AccessPermission(..., AccessTypes.Read / AccessTypes.Create / AccessTypes.Update)]`
- **Security Check**: Penanganan verifikasi aktor pada `ExitClearanceService.cs` telah ditutup dari celah identity spoofing dengan memverifikasi ID user melalui `_uow.FindUserIdByEmailAsync(actorEmail)`.
- **Entity & Model Harmony**:
  - `MstWorkforceProfile` menggunakan `DisplayName` dan `ProfileCode`, berelasi dengan `PrimaryDepartment` (`DepartmentName`) dan `PrimaryPosition` (`PositionName`).
  - Kalkulasi usia pensiun dan masa kerja merujuk langsung ke `MstEmployee` (`BirthDate` dan `JoinDate`).
  - `ApplicationUser` menggunakan `DisplayName`.
  - Pola pagination `PagedResult<T>` dikonstruksi secara eksplisit sesuai struktur respons Quilvian standar.

### 3.4 Backend Build Verification (`dotnet build`)
- **Perintah**: `dotnet build --no-restore -v minimal`
- **Hasil**: **LULUS DENGAN EXIT CODE 0 (0 Error(s), 598 Warning(s))**.
  - Waktu kompilasi bersih: 07 menit 09 detik.
  - Seluruh modul backend, controller baru, DTO, dan business service berhasil dikompilasi tanpa satupun error.

---

## 4. HASIL UAT HEADED CHROMIUM (LIVE BROWSER TESTING)

Pengujian antarmuka pengguna dilakukan secara langsung (live browser testing) dengan Chromium visual (Google Chrome Headed Mode) pada resolusi layar 1440x900 px. Seluruh bukti hasil screenshot disimpan secara eksklusif pada direktori `test-with-agy/`.

### 4.1 Ringkasan Eksekusi Skenario Uji

| No | Nama Skenario | Route / Endpoint | Level Risiko | Status Uji | Bukti Screenshot |
|:---|:---|:---|:---:|:---:|:---|
| 1 | Visualisasi Daftar & Form Pengunduran Diri | `/hr/separation/resignations` | P1 (Business Compliance) | **PASS** | `test-with-agy/01_pengunduran_diri_list.png` |
| 2 | Tab Kasus Pensiun & Calon Usia 55-58 Tahun | `/hr/separation/retirements` | P1 (Business Compliance) | **PASS** | `test-with-agy/02_pensiun_kasus_tab.png` |
| 3 | Evaluasi Akhir Kontrak PKWT (PP 35/2021) | `/hr/separation/contract-expiry` | P0 (Critical Compliance) | **PASS** | `test-with-agy/03_akhir_kontrak_list.png` |
| 4 | Administrasi Pemisahan Kerja & Kalkulasi Pesangon | `/hr/separation/terminations` | P0 (Financial Liability) | **PASS** | `test-with-agy/04_pemberhentian_list.png` |
| 5 | Matriks Tugas & Progres Serah Terima (Handover) | `/hr/separation/handovers` | P2 (Operational Continuity)| **PASS** | `test-with-agy/05_serah_terima_list.png` |
| 6 | Matriks Kliring 5 Unit Kerja & Gembok Payroll | `/hr/separation/exit-clearance` | P0 (Clinical & Asset Safety) | **PASS** | `test-with-agy/06_clearense_matrix_tab.png` |
| 7 | Navigasi Sidebar Menu 12 (Pemberhentian & Pensiun) | Seluruh 6 Submenu | P3 (Visual & Navigation) | **PASS** | `test-with-agy/07_sidebar_menu_12.png` |

---

## 5. STATUS AKHIR DAN KEPATUHAN KONSTITUSI

1. **Aturan Presedensi dan Larangan Khusus**:
   - **TIDAK ADA git commit** otomatis yang dijalankan.
   - **TIDAK ADA git push** otomatis ke remote origin mana pun.
   - **TIDAK ADA git merge** atau pembuatan Pull Request yang dilakukan tanpa wewenang terpisah.
   - **TIDAK ADA eksekusi migrasi database langsung** (`dotnet ef database update` atau manipulasi DDL langsung) tanpa persetujuan eksplisit.
   - Seluruh berkas uncommitted pengguna yang sudah ada sebelumnya (`base-form-control.jsx`, `employee-documents-view.jsx`, `official-duties-view.jsx`, `use-hr-select.js`, `hr-service.js`) tetap **utuh dan tidak disentuh**.
2. **Kepatuhan Penamaan Visual**:
   - Submenu 6 menggunakan label tampilan **Clearense** secara konsisten sesuai instruksi penugasan.
3. **Kesimpulan**:
   - Seluruh target penugasan Menu 12 Pemberhentian & Pensiun telah terpenuhi 100% dan siap diajukan untuk tinjauan tim (Review Ready).
