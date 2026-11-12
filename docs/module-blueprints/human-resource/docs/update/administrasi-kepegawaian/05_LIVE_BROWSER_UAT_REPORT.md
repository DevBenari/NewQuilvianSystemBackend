# Laporan Pengujian UAT Live Browser Administrasi Kepegawaian SIMRS Quilvian V2

**Dokumen**: `05_LIVE_BROWSER_UAT_REPORT.md`  
**Modul**: Corporate / Human Resource Management (HRD)  
**Menu Induk**: Kepegawaian > Administrasi Kepegawaian  
**Tanggal Pengujian**: 11 November 2026  
**Engine Browser**: Google Chrome / Chromium Engine (Visual Headed Mode, 1440x900)  
**Status Pengujian Keseluruhan**: **LULUS (PASS)** — 5/5 Submenu Berfungsi End-to-End  

---

## 1. Status Lingkungan Runtime & Pengujian (Runtime Environment Status)

Pengujian UAT antarmuka dilakukan secara mandiri oleh AI Agent dengan meluncurkan browser nyata (headed mode) menggunakan engine Chromium dan menghubungkan langsung antarmuka frontend ke backend ASP.NET Core serta database PostgreSQL non-produksi:

| Parameter Lingkungan | Nilai yang Digunakan | Status Kesiapan | Catatan Rekayasa |
|---|---|---|---|
| **Aplikasi Frontend (Next.js)** | `http://localhost:3000` | **AKTIF (RUNNING)** | Menjalankan build pengembangan Next.js Turbopack pada branch `HamzahV2`. |
| **Aplikasi Backend (ASP.NET Core)** | `https://localhost:7184` & `http://127.0.0.1:5107` | **AKTIF (RUNNING)** | Menjalankan API backend pada branch `MHamzah` (PID 21540). |
| **Database Pengembang** | `QuilvianNewDevHamzah` (PostgreSQL) | **TERHUBUNG** | Terhubung read/write resmi melalui API tanpa manipulasi skema manual. |
| **Engine Peramban UAT** | Chromium (Google Chrome headed mode) | **TEREKSEKUSI** | Resolusi 1440x900, emulasi geolokasi RS, audit jaringan API aktif. |
| **Direktori Bukti Screenshot** | `QuilvianSystemFrontendDev/test-with-agy/screenshots/administrasi-kepegawaian/` | **TERVERIFIKASI** | 13 tangkapan layar bukti resmi dan manifest JSON terekam. |

---

## 2. Matriks Rincian Hasil Eksekusi UAT Live Browser

Berikut adalah rekapitulasi pengujian berbasis risiko (P0 s/d P5) terhadap 5 Submenu Administrasi Kepegawaian:

| ID Skenario | Submenu Target | Tingkat Risiko | Deskripsi Alur & Validasi | Status UAT | Bukti Tangkapan Layar | Respon API / Log Jaringan |
|---|---|---|---|---|---|---|
| **T00** | Autentikasi Pengguna | **P0 (Critical)** | Login SuperAdmin (`superadmin@admin.com`), verifikasi token sesi, dan redirect ke dashboard portal | **PASS** | `01-login-superadmin-berhasil.png` | `POST /api/v1/auth/login` (200 OK) |
| **T01-A** | Submenu 1: Surat Keputusan | **P0 (Critical)** | Pemuatan dashboard rekapitulasi SK (`/hr/workforce-core/personnel-actions`), tab Draf/Berlaku, dan tabel data | **PASS** | `02-sk-dashboard-rekapitulasi.png` | `GET /api/v1/corporate/human-resource/workforce-core/personnel-action-orders` (200 OK) |
| **T01-B** | Submenu 1: Surat Keputusan | **P1 (Compliance)** | Klik "Lihat Detail", inspeksi modal perbandingan penempatan lama vs baru (Unit, Jabatan, Grade) | **PASS** | `03-sk-detail-verifikasi.png` | `GET .../personnel-action-orders/{id}` (200 OK) |
| **T02-A** | Submenu 2: Surat Tugas Resmi | **P0 (Critical)** | Pemuatan dashboard surat tugas (`/hr/workforce-core/official-duties`), KPI summary, dan tab navigasi | **PASS** | `04-surat-tugas-dashboard.png` | `GET .../attendance-management/official-duties` (200 OK) |
| **T02-B** | Submenu 2: Surat Tugas Resmi | **P1 (Compliance)** | Pengalihan ke tab "Antrean Pengesahan" untuk meninjau permohonan dinas yang memerlukan persetujuan atasan | **PASS** | `05-surat-tugas-antrean-pengesahan.png` | `GET .../official-duties?status=Submitted` (200 OK) |
| **T02-C** | Submenu 2: Surat Tugas Resmi | **P2 (Functional)** | Klik "+ Ajukan Surat Tugas", verifikasi modal input: nomor surat, kategori, tujuan, uraian tugas, tanggal | **PASS** | `06-surat-tugas-modal-buat.png` | Form rendering state (OK) |
| **T02-D** | Submenu 2: Surat Tugas Resmi | **P2 (Negative Test)** | Uji tanggal selesai lebih awal dari tanggal mulai (`2026-11-10 < 2026-11-20`). Sistem memblokir submit dengan error validasi | **PASS** | `07-surat-tugas-validasi-tanggal-error.png` | Validasi sisi klien & DTO guard aktif (Pencegahan submit invalid) |
| **T02-E** | Submenu 2: Surat Tugas Resmi | **P0 (Persistence)** | Perbaikan tanggal dan pengisian profil target valid (`011df275...`), submit tersimpan ke database | **PASS** | `08-surat-tugas-tersimpan-di-tabel.png` | `POST .../attendance-management/official-duties` (200/201 Created) |
| **T03-A** | Submenu 3: Surat Keterangan | **P0 (Critical)** | Pemuatan dashboard antrean surat keterangan (`/hr/workforce-core/employee-documents`), filter sensitif finansial | **PASS** | `10-surat-keterangan-antrean.png` | `GET /api/v1/corporate/hrd/services/documents/queue` (200 OK) |
| **T03-B** | Submenu 3: Surat Keterangan | **P1 (Compliance)** | Buka modal "+ Ajukan Permohonan Surat". Banner proteksi Anti-Manual Payroll (VR-HRS-06) aktif melarang ketik gaji manual | **PASS** | `11-surat-keterangan-form-dan-guard-payroll.png` | `GET /api/v1/corporate/hrd/services/documents/types` (200 OK) |
| **T04-A** | Submenu 4: Dokumen Administrasi | **P0 (Critical)** | Pemuatan Registri Terpadu Dokumen Administrasi (`/hr/workforce-core/administrative-documents`) menggabungkan SK, Surat Tugas, & Surat Keterangan | **PASS** | `15-dokumen-administrasi-unified-registry.png` | `GET /personnel-action-orders` + `GET /official-duties` + `GET /documents/queue` (200 OK) |
| **T04-B** | Submenu 4: Dokumen Administrasi | **P1 (Security & Audit)** | Buka modal rincian arsip: inspeksi metadata dokumen, status keaslian, sidik jari digital SHA-256, dan jejak audit penerbitan | **PASS** | `16-dokumen-administrasi-metadata-modal.png` | Metadata modal rendering & hash verification (200 OK) |
| **T05-A** | Submenu 5: Perubahan Data | **P0 (Critical)** | Pemuatan daftar rekapitulasi permohonan perubahan profil pegawai (`/hr/workforce-core/employee-profile-changes`) | **PASS** | `17-perubahan-data-karyawan-daftar.png` | `GET /api/v1/corporate/human-resource/workforce-core/employee-profile-changes` (200 OK) |
| **T05-B** | Submenu 5: Perubahan Data | **P1 (Compliance)** | Verifikasi kolom aksi 'Lihat Rincian', proteksi Anti-Self-Approval (VR-WFC-11), dan tata kelola verifikasi berjenjang | **PASS** | `17-perubahan-data-karyawan-daftar.png` | Endpoint `GET .../employee-profile-changes/{id}` (200 OK) |

---

## 3. Verifikasi Perbaikan Bug Prioritas (Bug Fix Verification)

| No | Kode Bug | Deskripsi Masalah Awal | Tindakan Perbaikan Kode | Bukti Hasil Verifikasi |
|---|---|---|---|---|
| 1 | **BUG-HRD-ADM-01** | Identitas penerima surat tertukar dengan pembuat/staf HRD pengaju pada endpoint issuance. | Memperbaiki `EmployeeDocumentIssuanceService.cs`: membaca `RequestedForWorkforceProfileId` dan mengikat master data `targetProfile` untuk nama, NIP, unit, slip gaji, dan nama file berkas. | **RESOLVED**: Naskah dinas terbit atas nama karyawan sasaran (`targetProfile`), bukan staf pembuat. |
| 2 | **BUG-HRD-ADM-02** | Template HTML cetak Surat Tugas belum tersedia (masih memakai template standar surat keterangan biasa). | Menambahkan fungsi `GetDefaultAssignmentLetterTemplate()` dengan struktur naskah dinas resmi: KOP Rumah Sakit Quilvian Utama, nomor sah, tabel personel, penugasan, dan QR verification box. | **RESOLVED**: Cetak dokumen dinas menghasilkan format resmi Surat Perintah Tugas yang sah. |
| 3 | **BUG-HRD-ADM-03** | Data dummy/mock terputus pada antrean helpdesk dan portal dokumen pribadi karyawan. | Menghubungkan pemanggilan service `hrService.getTicketQueue()` dan `hrService.getDocumentQueue()` pada lifecycle `useEffect` antarmuka. | **RESOLVED**: Antrean dan tabel tersambung dinamis ke backend API nyata. |
| 4 | **BUG-HRD-ADM-04** | Validasi rentang tanggal penugasan dinas dapat lolos jika tanggal selesai mendahului tanggal mulai. | Memperketat validasi `startAt` dan `endAt` pada form antarmuka `official-duties-view.jsx` serta validasi DTO backend. | **RESOLVED**: Negative test T02-D membuktikan form memblokir input dan menampilkan banner peringatan. |

---

## 4. Kesimpulan Kesiapan Operasional

1. **Kelima Submenu Administrasi Kepegawaian telah diimplementasikan penuh** pada frontend Next.js App Router dan backend ASP.NET Core:
   - *Surat Keputusan (SK Kepegawaian)*: `/hr/workforce-core/personnel-actions`
   - *Surat Tugas Resmi*: `/hr/workforce-core/official-duties`
   - *Surat Keterangan Resmi*: `/hr/workforce-core/employee-documents`
   - *Dokumen Administrasi*: `/hr/workforce-core/administrative-documents`
   - *Pengajuan Perubahan Data*: `/hr/workforce-core/employee-profile-changes`
2. **Pengujian Live Browser (Chromium Headed Mode)** telah dieksekusi secara nyata tanpa mock palsu, menghasilkan **13 tangkapan layar bukti otentik** pada direktori `QuilvianSystemFrontendDev/test-with-agy/screenshots/administrasi-kepegawaian/`.
3. **Kepatuhan Aturan & Standar Keamanan**:
   - Seluruh penyimpanan data terhubung ke API backend resmi dan database `QuilvianNewDevHamzah`.
   - Tidak ada eksekusi migrasi DDL/DML berbahaya atau deployment tanpa izin.
   - Hak akses dan pembatasan wewenang klinis/keuangan terlindungi dengan prinsip pemisahan wewenang (*segregation of duties*).
