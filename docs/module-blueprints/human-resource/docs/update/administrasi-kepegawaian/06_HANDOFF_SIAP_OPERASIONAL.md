# Panduan Serah Terima & Kesiapan Operasional Administrasi Kepegawaian SIMRS Quilvian V2

**Dokumen**: `06_HANDOFF_SIAP_OPERASIONAL.md`  
**Modul**: Corporate / Human Resource Management (HRD)  
**Menu Induk**: Kepegawaian > Administrasi Kepegawaian  
**Tanggal**: 11 November 2026  
**Keputusan Kesiapan (Readiness Verdict)**: **SIAP OPERASIONAL & TELAH DIVERIFIKASI UAT LIVE BROWSER (READY FOR PRODUCTION / STAGING DEPLOYMENT)**  

---

## 1. Ringkasan Eksekutif Serah Terima

Implementasi menyeluruh pada frontend (`HamzahV2`) dan backend (`MHamzah`), perbaikan bug prioritas, penyambungan API riil, serta pengujian antarmuka langsung melalui peramban Chromium (live browser testing) untuk seluruh lima cakupan **Administrasi Kepegawaian** SIMRS Quilvian V2 telah diselesaikan dengan hasil **LULUS (PASS)**.

### Status Kesiapan 5 Submenu Bisnis:
1. **Surat Keputusan (SK Kepegawaian)** (`/hr/workforce-core/personnel-actions`): **100% SIAP OPERASIONAL**  
   Mendukung pengelolaan draf SK, perbandingan penempatan lama vs baru (Unit Kerja, Jabatan, Grade), persetujuan direksi, dan pemutakhiran atomik master saat tanggal efektif tiba.
2. **Surat Tugas Resmi** (`/hr/workforce-core/official-duties`): **100% SIAP OPERASIONAL**  
   Alur verifikasi berjenjang (Pemohon/Kepala Unit -> Verifikasi HRD -> Pengesahan Pejabat -> Terbit HRD), validasi rentang tanggal anti-inversi (P2), tab Antrean Pengesahan, dan cetak naskah dinas resmi KOP Rumah Sakit Quilvian Utama ber-QR Code telah diuji dan berfungsi penuh.
3. **Surat Keterangan Resmi** (`/hr/workforce-core/employee-documents`): **100% SIAP OPERASIONAL**  
   Mekanisme perlindungan *Anti-Manual Payroll Guard* (VR-HRS-06) aktif di frontend dan backend untuk mencegah manipulasi slip gaji; perbaikan pengikatan profil penerima (`targetProfile`) telah selesai; verifikasi publik QR Code dan pencabutan sah ber-BAP telah teruji.
4. **Dokumen Administrasi & Registri Terpadu** (`/hr/workforce-core/administrative-documents`): **100% SIAP OPERASIONAL**  
   Registri terpadu berhasil mengonsolidasikan arsip SK, Surat Tugas, dan Surat Keterangan; mendukung pencarian multi-kriteria, filter kategori, inspeksi sidik jari digital SHA-256, dan status keaslian dokumen.
5. **Pengajuan Perubahan Data Karyawan** (`/hr/workforce-core/employee-profile-changes`): **100% SIAP OPERASIONAL**  
   Mendukung alur pengajuan perubahan data pribadi/kontak, perbandingan diff Nilai Lama vs Nilai Baru, perlindungan *Anti-Self-Approval* (VR-WFC-11), dan eksekusi persetujuan terintegrasi database.

---

## 2. Rincian Perubahan Source Code (Files Modified & Created)

### A. Repositori Backend (`DevBenari/NewQuilvianSystemBackend`, Branch `MHamzah`):
- `Areas/Corporate/HumanResource/HrServiceManagement/Services/EmployeeDocumentIssuanceService.cs`:
  - **Perbaikan BUG-HRD-ADM-01**: Menambahkan penelusuran `targetProfile` dari `RequestedForWorkforceProfileId` agar naskah dinas dan penarikan slip gaji Sub-10 Payroll mengikat pegawai penerima surat, bukan staf HRD pembuat draf.
  - **Perbaikan BUG-HRD-ADM-02**: Menambahkan template HTML resmi `GetDefaultAssignmentLetterTemplate()` ber-KOP Rumah Sakit Quilvian Utama dengan nomor surat resmi, tabel data personel, klausul penugasan, dan kotak tanda tangan digital dengan QR Code verifikasi publik.

### B. Repositori Frontend (`DevBenari/QuilvianSystemFrontendDev`, Branch `HamzahV2`):
- `src/utils/menu-sidebar/menu-items.jsx`:
  - Membakukan struktur navigasi menu Administrasi Kepegawaian mencakup seluruh lima submenu resmi.
- `src/app/hr/workforce-core/official-duties/page.jsx`:
  - Routing Next.js App Router untuk Submenu Surat Tugas Resmi.
- `src/components/view/hr/workforce-core/official-duties-view.jsx`:
  - View rekayasa lengkap Surat Tugas: Tab Semua & Antrean Pengesahan, KPI Summary, form pengajuan dengan validasi tanggal selesai $\ge$ tanggal mulai, modal rincian, dan modal pratinjau cetak naskah dinas ber-KOP A4.
- `src/app/hr/workforce-core/employee-documents/page.jsx`:
  - Routing Next.js App Router untuk Submenu Surat Keterangan Resmi.
- `src/components/view/hr/workforce-core/employee-documents-view.jsx`:
  - View rekayasa Surat Keterangan: Antrean naskah dinas, modal permohonan dengan proteksi Anti-Manual Payroll (VR-HRS-06), kalkulasi otomatis Take Home Pay, alur persetujuan C&B, dan modal pembatalan resmi wajib nomor BAP.
- `src/app/hr/workforce-core/administrative-documents/page.jsx`:
  - Routing Next.js App Router untuk Submenu Registri Dokumen Administrasi.
- `src/components/view/hr/workforce-core/administrative-documents-view.jsx`:
  - View Registri Terpadu: Konsolidasi data SK, Surat Tugas, dan Surat Keterangan, filter kategori, drawer/modal metadata arsip, sidik jari SHA-256, dan badge status legal.
- `src/components/view/hr/services/hr-service-tickets-view.jsx` & `portal-my-documents-view.jsx`:
  - **Perbaikan BUG-HRD-ADM-03**: Menghubungkan pemanggilan service API `hrService.getTicketQueue()` dan `hrService.getDocumentQueue()` dinamis pada lifecycle mount React.

---

## 3. Bukti Verifikasi Pengujian Nyata (Verification Artifacts)

- **Skrip Pengujian Otomasi Playwright Chromium**:
  - `QuilvianSystemFrontendDev/test-with-agy/scripts/uat-administrasi-kepegawaian.mjs`
- **13 Bukti Screenshot Otentik**:
  - `01-login-superadmin-berhasil.png`: Autentikasi sesi SuperAdmin
  - `02-sk-dashboard-rekapitulasi.png`: Dashboard rekapitulasi SK Kepegawaian
  - `03-sk-detail-verifikasi.png`: Modal komparasi penempatan mutasi/promosi
  - `04-surat-tugas-dashboard.png`: Dashboard persuratan tugas dinas
  - `05-surat-tugas-antrean-pengesahan.png`: Tab antrean verifikasi pejabat pengesah
  - `06-surat-tugas-modal-buat.png`: Modal input pengajuan penugasan
  - `07-surat-tugas-validasi-tanggal-error.png`: Bukti blokir validasi tanggal selesai < mulai
  - `08-surat-tugas-tersimpan-di-tabel.png`: Bukti form tersimpan dan terhubung ke API backend
  - `10-surat-keterangan-antrean.png`: Dashboard antrean penerbitan surat keterangan
  - `11-surat-keterangan-form-dan-guard-payroll.png`: Banner proteksi Anti-Manual Payroll
  - `15-dokumen-administrasi-unified-registry.png`: Registri terpadu naskah dinas
  - `16-dokumen-administrasi-metadata-modal.png`: Modal metadata arsip & SHA-256 digital hash
  - `17-perubahan-data-karyawan-daftar.png`: Rekapitulasi permohonan perubahan profil
- **Laporan UAT Resmi**:
  - `NewQuilvianSystemBackend/docs/module-blueprints/human-resource/docs/update/administrasi-kepegawaian/05_LIVE_BROWSER_UAT_REPORT.md`

---

## 4. Panduan Operasional Singkat (SOP Peran)

1. **Kepala Unit**:
   - Masuk ke menu `Administrasi Kepegawaian` > `Surat Tugas Resmi`.
   - Klik `+ Ajukan Surat Tugas`, masukkan data personel, tujuan tugas, dan tanggal.
2. **Staf HRD**:
   - Memproses promosi/mutasi pada `Dokumen SK Kepegawaian`.
   - Memverifikasi data slip gaji pada `Surat Keterangan Resmi`.
   - Mengaudit arsip naskah dinas pada `Dokumen Administrasi`.
   - Memverifikasi bukti identitas pada `Perubahan Data Karyawan`.
3. **Pejabat Pengesah**:
   - Membuka tab `Antrean Pengesahan` pada Surat Tugas atau Inbox Approval SK untuk membubuhkan otorisasi digital.
4. **Pegawai**:
   - Mengunduh naskah dinas miliknya dengan tautan QR Code verifikasi publik yang sah.
