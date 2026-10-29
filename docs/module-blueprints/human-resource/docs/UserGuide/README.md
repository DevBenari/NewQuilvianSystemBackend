# Buku Panduan Pengguna (User Guide) Sistem HRD Rumah Sakit Quilvian SIMRS

Selamat datang di **Buku Panduan Pengguna (*User Guide*) Sistem Human Resource (HRD) Quilvian SIMRS**. 

Dokumen ini disusun khusus sebagai panduan praktis bagi manajemen rumah sakit, kepala ruangan, dokter, perawat, staf penunjang medis, dan tim HRD untuk mempelajari, menguji, dan mengoperasikan fitur-fitur sistem HRD yang telah **selesai dibangun penuh (*End-to-End*)** pada antarmuka web.

---

## 1. Peta Navigasi Panduan Pengguna per Sub-Modul

Berikut adalah daftar folder panduan pengguna untuk **13 Sub-Modul** yang telah berstatus **Selesai Penuh** dan siap diuji coba di peramban (*web browser*):

| No | Folder Sub-Modul | Nama Sub-Modul | Sasaran Pengguna Utama | Tautan Panduan Lengkap |
| :---: | :--- | :--- | :--- | :--- |
| **01** | `01-master-data-hr` | **Master Data HR** | HR Superadmin & Tim Tata Kelola | [Buka Panduan](./01-master-data-hr/PANDUAN_PENGGUNA.md) |
| **02** | `02-workforce-core` | **Administrasi Profil Kepegawaian** | HR Staff & Personalia | [Buka Panduan](./02-workforce-core/PANDUAN_PENGGUNA.md) |
| **06** | `06-scheduling-roster` | **Penjadwalan Roster & On-Call 24/7** | Kepala Ruangan & Manajer Pelayanan | [Buka Panduan](./06-scheduling-roster/PANDUAN_PENGGUNA.md) |
| **07** | `07-attendance-management` | **Presensi & Kehadiran Harian** | Seluruh Staf & HR Attendance Officer | [Buka Panduan](./07-attendance-management/PANDUAN_PENGGUNA.md) |
| **08** | `08-leave-management` | **Manajemen Cuti & Izin Staf** | Seluruh Staf & Kepala Ruangan | [Buka Panduan](./08-leave-management/PANDUAN_PENGGUNA.md) |
| **09** | `09-overtime-management` | **Manajemen Lembur Staf** | Staf Medis/Umum & HR Payroll | [Buka Panduan](./09-overtime-management/PANDUAN_PENGGUNA.md) |
| **11** | `11-restitution-deduction` | **Restitusi & Potongan Cicilan Gaji** | Staf HR Payroll & Keuangan | [Buka Panduan](./11-restitution-deduction/PANDUAN_PENGGUNA.md) |
| **12** | `12-benefit-management` | **Pendaftaran Benefit & Tanggungan** | HR Benefit Officer & Karyawan | [Buka Panduan](./12-benefit-management/PANDUAN_PENGGUNA.md) |
| **16** | `16-learning-development` | **Pelatihan Wajib Akreditasi & Diklat** | Tim Diklat HR & Seluruh Staf RS | [Buka Panduan](./16-learning-development/PANDUAN_PENGGUNA.md) |
| **17** | `17-performance-management` | **Evaluasi Kinerja & Nilai KPI** | Kepala Instalasi & HR Kinerja | [Buka Panduan](./17-performance-management/PANDUAN_PENGGUNA.md) |
| **19** | `19-employee-relations-discipline` | **Hubungan Kerja & Disiplin SP** | HR Hubungan Industrial & Komite Etik | [Buka Panduan](./19-employee-relations-discipline/PANDUAN_PENGGUNA.md) |
| **20** | `20-exit-clearance-offboarding` | **Kliring Aset & Exit Clearance** | Karyawan Resign, HR, IT, Finance | [Buka Panduan](./20-exit-clearance-offboarding/PANDUAN_PENGGUNA.md) |
| **21** | `21-ess-approval-inbox` | **Layanan Mandiri (ESS) & Kotak Masuk Persetujuan** | Seluruh Staf & Pimpinan Unit RS | [Buka Panduan](./21-ess-approval-inbox/PANDUAN_PENGGUNA.md) |

---

## 2. Persiapan Sebelum Mulai Menguji Sistem

Agar Anda dapat mencoba seluruh skenario di peramban, pastikan sistem berjalan lokal di komputer Anda:

1. **Aplikasi Backend Aktif**:
   ```powershell
   cd "c:\Users\Admin\Documents\Quilvian\Qulvian\NewQuilvianSystemBackend"
   dotnet run
   ```
2. **Aplikasi Frontend Web Aktif**:
   ```powershell
   cd "c:\Users\Admin\Documents\Quilvian\Qulvian\QuilvianSystemFrontendDev"
   npm run dev
   ```
3. **Akses Web**: Buka peramban (Google Chrome, Microsoft Edge, atau Mozilla Firefox) dan buka alamat `http://localhost:3000`.

---

## 3. Pembagian Peran Pengguna (*User Personas*)

Untuk mempermudah simulasi alur kerja rumah sakit yang nyata, gunakan peran berikut:

1. **Pegawai / Staf Pelaksana (Dokter / Perawat / Staf Administrasi)**:
   - Bertugas di ruang pelayanan (misal IGD atau Rawat Inap).
   - Memakai menu **Layanan Mandiri (*Self-Services*)**: Mengisi presensi, melihat jadwal dinas, mengajukan cuti, mengajukan lembur darurat, dan mengajukan tukar shift.
2. **Atasan Langsung (Kepala Ruangan / Kepala Instalasi / Manajer Pelayanan)**:
   - Bertanggung jawab atas kelancaran operasional satu unit/ruangan.
   - Memakai menu **Penjadwalan Roster** dan **Kotak Masuk Persetujuan (*Approval Inbox*)**.
3. **Administrator HR & Tim Operasional SDM**:
   - Bertanggung jawab atas kepatuhan data kepegawaian, verifikasi jam lembur, alokasi saldo cuti, penutupan periode absensi, pelatihan diklat, dan integrasi penggajian.
   - Memakai menu **Operasional HR** (`/hr/*`).

---

## 4. Standar Tampilan & Komponen Antarmuka yang Digunakan

Seluruh layar pada modul HRD Quilvian menggunakan komponen baku yang seragam:
* **Kartu Ringkasan (*SummaryCards*)**: Kotak informasi angka di bagian atas halaman (misal: Total Staf Hadir, Sisa Saldo Cuti, Permohonan Menunggu).
* **Kotak Pencarian & Filter (*DataFilter*)**: Memungkinkan pencarian cepat nama staf, unit kerja, atau status permohonan.
* **Tabel Data (*DataTable*)**: Penyajian daftar data dengan navigasi halaman (*Pagination*).
* **Penanda Status (*StatusBadge*)**: Lencana warna penanda status:
  - 🟢 Hijau = Selesai, Disetujui (*Approved*), Tepat Waktu, atau Aktif.
  - 🟡 Kuning = Menunggu Persetujuan (*Pending*), Butuh Revisi (*Revision*), atau Menjelang Kedaluwarsa.
  - 🔴 Merah = Ditolak (*Rejected*), Terlambat, Kedaluwarsa, atau Diberi Sanksi SP.
* **Jendela Aksi (*ConfirmModal*)**: Kotak dialog yang muncul saat menambah data atau menyetujui dokumen.
* **Pesan Notifikasi (*Toast*)**: Notifikasi hijau/merah di pojok kanan layar sebagai tanda proses berhasil/gagal.
