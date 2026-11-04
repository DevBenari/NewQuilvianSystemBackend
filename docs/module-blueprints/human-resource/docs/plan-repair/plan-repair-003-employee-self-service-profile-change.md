# PLAN-REPAIR-003 — Implementasi Layanan Mandiri Perubahan Data Profil Karyawan (Employee Self-Service Profile Change)

```yaml
plan_id: PLAN-REPAIR-HRD-003
blueprint_id: HRD-BP-001
sub_modul: S-A5 (Employee Self-Service) & S-A1 (Administrasi Kepegawaian)
status_rencana: SELESAI
tanggal_rencana: "2026-11-04"
diputuskan_oleh: "User (Pemilik Kebutuhan) — Master Prompt Implementasi Employee Self-Service Profile Change"
tanggal_keputusan: "2026-11-04"
basis_source_backend: "8b17b75a9bd3dabe79b95dff00067ed5e8a16196 (MHamzah)"
basis_source_frontend: "44990b3d3db14f9b04c6f68137822b56357e920a (HamzahV2)"
```

---

## 1. Latar Belakang dan Tujuan

Karyawan rumah sakit membutuhkan fasilitas mandiri untuk memperbarui informasi data pribadinya (seperti nomor telepon, alamat domisili, email, kontak darurat, serta data identitas resmi) secara langsung melalui antarmuka Layanan Mandiri tanpa harus meminta staf HR melakukan entri manual dari awal. Namun, data master resmi kepegawaian (`MstEmployee` dan `MstWorkforceProfile`) harus terlindungi dari manipulasi langsung, dan setiap pembaruan data wajib melalui alur verifikasi berkas bukti dan persetujuan Workflow Engine resmi.

Rencana perbaikan ini melengkapi kapabilitas Layanan Mandiri Karyawan (`S-A5`), menghubungkan verifikasi HRD pada Administrasi Kepegawaian (`S-A1`), menerapkan aturan Anti-Self-Approval, membuka hambatan integrasi HTTP 415 pada berkas unggahan multipart, serta mendokumentasikan keputusan bisnis terbaru (`HRD-DEC-058` s.d. `HRD-DEC-061`).

---

## 2. Register Status Pengerjaan (Fix Register)

| ID Perbaikan | Menutup Masalah | Ringkasan Perbaikan | Area | Prioritas | Status | Bukti Verifikasi |
| :--- | :--- | :--- | :---: | :---: | :---: | :--- |
| `FIX-HRD-003-01` | Navigasi Menu | Penambahan menu **Layanan Mandiri → Kelola Data Saya** ke rute `/self-services/human-resource/employee/profile-changes` dengan penjagaan `MyProfileChange:Read` dan quick-link pada sidebar profil pengguna | FE | P0 | ✅ SELESAI | `src/utils/menu-sidebar/menu-items.jsx`, `user-profile-sidebar.jsx`, test `ESS-002`, `ESS-003` |
| `FIX-HRD-003-02` | Endpoint ESS Canonical | Penambahan endpoint profil berjalan (`current-profile`), ringkasan statistik (`summary`), unggah bukti (`evidence`), unduh bukti (`evidence/download`), dan pembatalan draf bukti pada controller mandiri | BE | P0 | ✅ SELESAI | `EmployeeProfileChangeSelfServiceController.cs`, test `ESS-001` |
| `FIX-HRD-003-03` | Multi-Kategori Transaksi | Dukungan kategori pengajuan `Multiple` (Multi-kategori) agar seluruh perubahan multi-field lintas kelompok dapat dibundel dalam 1 permohonan tanpa terbentur validasi `hasOpenRequest` | BE + FE | P0 | ✅ SELESAI | `EmployeeProfileChangeDtos.cs`, `EmployeeProfileChangeService.cs`, test `ESS-004` |
| `FIX-HRD-003-04` | Kewajiban Berkas Sensitif | Validasi ketat berkas bukti pendukung untuk perubahan field sensitif (NIK, Nama Lengkap, Tempat/Tgl Lahir, Status Pernikahan) sesuai mandat `HRD-DEC-052` | BE + FE | P1 | ✅ SELESAI | `EmployeeProfileChangeService.cs`, `employee-profile-changes-self-service-view.jsx`, test `ESS-004` |
| `FIX-HRD-003-05` | Anti-Self-Approval | Proteksi ganda (Backend + Frontend) yang melarang pemohon menyetujui atau memverifikasi pengajuannya sendiri meskipun memiliki hak akses HRD (Prinsip Four-Eyes) | BE + FE | P0 | ✅ SELESAI | `EmployeeProfileChangeService.cs:StartVerificationAsync`, `employee-profile-changes-view.jsx`, test `ESS-005` |
| `FIX-HRD-003-06` | HTTP 415 Bugfix | Penghapusan manual header `Content-Type` pada Axios Request Interceptor saat data berupa `FormData` sehingga browser otomatis menyematkan boundary multipart (menutup `FIX-HRD-002-09` On-Behalf) | FE | P1 | ✅ SELESAI | `src/lib/axiosInstance/InstanceAxios.jsx`, test `BUG-002` |
| `FIX-HRD-003-07` | Layar Layanan Mandiri | Pembuatan layar mandiri `employee-profile-changes-self-service-view.jsx` dengan live diff detector, kartu penempatan read-only, tombol tunggal "Simpan dan Ajukan Perubahan Data", dan tab Riwayat Pengajuan Saya | FE | P0 | ✅ SELESAI | `src/app/self-services/human-resource/employee/profile-changes/page.jsx`, test `ESS-004` |
| `FIX-HRD-003-08` | Layar Verifikasi HRD | Penyempurnaan `employee-profile-changes-view.jsx` dengan modal detail komparasi Nilai Lama vs Baru, unduhan berkas bukti, dan aksi Mulai Verifikasi | FE | P1 | ✅ SELESAI | `src/components/view/hr/workforce-core/employee-profile-changes-view.jsx`, test `ESS-005` |
| `FIX-HRD-003-09` | Redesain Native Profil Workforce | Menggunakan kembali antarmuka canonical `WorkforceProfileDashboard` secara 100% identik tanpa form sisi kanan, mengaktifkan edit per-menu Profil Workforce secara native (Ringkasan Profil, Alamat, Rekening Bank, Keluarga, dll.), serta menyajikan Modal Tinjauan Usulan Perubahan untuk Approval HRD | FE | P0 | ✅ SELESAI | `employee-profile-changes-self-service-view.jsx`, test `ESS-004` |

---

## 3. Solusi Terpilih dan Arsitektur Implementasi

### 3.1 Penentuan Kategori Transaksi ("Multiple")
Backend `TrxEmployeeProfileChangeRequest` memiliki aturan invariant bisnis `hasOpenRequest`: seorang pegawai tidak diizinkan memiliki lebih dari satu permohonan aktif pada saat yang sama. Jika karyawan mengubah Nomor Telepon (kategori `Contact`) dan Alamat (kategori `Address`) secara terpisah, pengajuan kedua akan ditolak.
**Solusi Terpilih:**
- Menambahkan nilai kategori `Multiple` (Multi-kategori) pada konstanta `RequestCategories` backend dan frontend.
- Frontend secara otomatis mendeteksi field-field yang berubah. Jika field berasal dari >1 kelompok, kategori otomatis diset ke `Multiple`, dan seluruh rincian field dimasukkan ke dalam `Details[i]`.

### 3.2 Perlindungan Master Data dan Lifecycle Transaksi
Data master resmi (`MstEmployee` dan `MstWorkforceProfile`) sama sekali tidak dimutasi saat tombol "Simpan dan Ajukan Perubahan Data" ditekan.
- Status awal permohonan: `Draft`
- Setelah diunggah berkas bukti (jika sensitif) dan disubmit: status menjadi `Submitted` (Menunggu Verifikasi HRD).
- HRD memeriksa berkas dan klik "Mulai Verifikasi": status menjadi `UnderVerification`.
- Persetujuan diselesaikan melalui Workflow Engine resmi: status menjadi `Approved`.
- Integrasi `EmployeeProfileChangeWorkflowIntegrationService` mengeksekusi metode `ApplyAsync`: data diterapkan ke master resmi, dan status akhir menjadi `Applied`.

### 3.3 Penegakan Anti-Self-Approval (Four-Eyes Principle)
Sesuai kepatuhan tata kelola rumah sakit (`HRD-DEC-031` dan `HRD-DEC-058`):
- Backend memeriksa pada `StartVerificationAsync`: jika `verifierUserId == request.SubmittedByUserId` atau `verifierWorkforceProfileId == request.WorkforceProfileId`, sistem melempar `InvalidOperationException` ("Anda tidak diizinkan memverifikasi pengajuan perubahan data milik Anda sendiri.").
- Frontend memeriksa identitas pengguna login dari Redux auth slice (`currentUser.workforceProfileId` dan `currentUser.userId`). Jika cocok dengan pemohon, banner peringatan anti-self-approval ditampilkan dan tombol verifikasi dinonaktifkan.

### 3.4 Penyelesaian Masalah Universal HTTP 415 Multipart
Pada `InstanceAxios.jsx`, instance dibuat dengan header bawaan `"Content-Type": "application/json"`. Ketika permintaan dikirim dengan objek `FormData`, ketiadaan penghapusan header bawaan menyebabkan ASP.NET Core menolak payload karena tidak ditemukannya parameter boundary multipart (`HTTP 415 Unsupported Media Type`).
**Solusi Terpilih:**
Pada interceptor permintaan `InstanceAxios`:
```javascript
if (typeof FormData !== "undefined" && config?.data instanceof FormData && config?.headers) {
  delete config.headers["Content-Type"];
  delete config.headers["content-type"];
}
```
Solusi ini menyelesaikan kendala pada seluruh formulir unggah berkas di frontend, termasuk pengajuan atas nama (`createOnBehalfProfileChange`) dan lampiran bukti mandiri (`uploadMyProfileChangeEvidence`).

---

## 4. Kriteria Keberhasilan (Acceptance Criteria)

1. Karyawan yang login dapat membuka halaman `/self-services/human-resource/employee/profile-changes`.
2. Halaman otomatis memuat snapshot data diri karyawan login tanpa parameter URL bebas.
3. Form mendeteksi perbedaan antara data awal dan data yang diedit secara real-time.
4. Field unit, jabatan, dan status administratif terkunci (read-only).
5. Jika data sensitif (NIK, Status Pernikahan, Nama, Tempat/Tgl Lahir) diubah, sistem mewajibkan unggah bukti pendukung dan memvalidasi tipe serta ukuran berkas (maks. 5MB).
6. Tombol "Simpan dan Ajukan Perubahan Data" mengeksekusi Draf -> Unggah Bukti -> Submit Workflow secara berurutan dan transaksional.
7. Tidak ada mutasi master data sebelum verifikasi dan persetujuan sah.
8. Karyawan dapat memantau riwayat pengajuan dan membatalkan pengajuan selama status masih `Draft` atau `Submitted`.
9. HRD dapat melihat perbandingan data lama vs baru pada modal detail dan mengunduh berkas bukti.
10. Aturan Anti-Self-Approval mencegah persetujuan oleh pemohon sendiri.
11. Seluruh 24 unit test passing dan ESLint bebas error.
12. Layar Layanan Mandiri me-reuse antarmuka canonical `WorkforceProfileDashboard` tanpa form sisi kanan, memungkinkan pengubahan data langsung per-menu Profil Workforce (Ringkasan Profil, Alamat, Rekening Bank, Keluarga, dll.), serta menyajikan modal review komparasi data usulan sebelum dikirim ke HRD.
