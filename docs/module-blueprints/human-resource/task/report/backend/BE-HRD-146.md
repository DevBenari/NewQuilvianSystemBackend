# Laporan Penyelesaian Backend Task: BE-HRD-146

## 1. Identitas Task
* **ID Task**: `BE-HRD-146`
* **Nama Task**: Skema database PostgreSQL & migrasi EF Core untuk entitas Payroll V2 (`HrdPayrollSetting`, `TrxPayrollVarianceAlert`, `TrxPayrollHandoffBatch`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 10 — Pemrosesan Penggajian Pegawai Rumah Sakit (Payroll V2)
* **Slice / Milestone**: `Slice S-B5` · `Fase MVP-30: Fondasi Siklus Penggajian & Penghasilan Tetap`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-05
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Dalam tata kelola rumah sakit, modul penggajian (Payroll V2) membutuhkan fondasi persistensi basis data yang kuat, aman, dan patuh terhadap regulasi ketenagakerjaan serta sistem jaminan sosial nasional (BPJS Ketenagakerjaan & BPJS Kesehatan). 

Sebelum siklus penggajian dapat diproses, sistem membutuhkan:
1. **Konfigurasi Master Penggajian (`HrdPayrollSetting`)**: Menyimpan konfigurasi tarif persentase dan batas plafon iuran resmi BPJS Ketenagakerjaan (JKK sesuai tingkat risiko medis, JKM 0,30%, JHT pemberi kerja 3,70% & pekerja 2,00%, JP pemberi kerja 2,00% & pekerja 1,00% dengan plafon upah Rp 10.042.300), BPJS Kesehatan (pemberi kerja 4,00% & pekerja 1,00% dengan plafon upah Rp 12.000.000), tanggal cut-off presensi/lembur (tanggal 20), tanggal pembayaran gaji (tanggal 25), serta batas persentase ambang variansi peringatan audit gaji (default 15%).
2. **Peringatan Anomali & Variansi Penggajian (`TrxPayrollVarianceAlert`)**: Mendokumentasikan setiap lonjakan atau penurunan tajam penghasilan bersih pegawai ($\ge 15\%$) dibandingkan bulan sebelumnya, status mitigasi (`Investigating`, `Dismissed`, `Adjusted`), dan alasan verifikasi oleh tim pemeriksa HRD guna mencegah kesalahan nominal sebelum pencairan.
3. **Penyimpanan Batch Serah Terima Antar-Modul (`TrxPayrollHandoffBatch`)**: Menyimpan riwayat berkas serah terima terenkripsi (checksum SHA-256) saat penyerapan data lembur Sub-09, presensi Sub-07, atau pelepasan data ke Modul Keuangan (Finance).

---

## 3. Entitas & Konfigurasi Basis Data EF Core yang Dibuat

### A. Model Data Baru
1. `Areas/Corporate/HumanResource/PayrollManagement/Models/HrdPayrollSetting.cs`:
   - Menyimpan plafon upah BPJS Kesehatan, plafon JP, persentase JKK, JKM, JHT, JP, BPJS Kesehatan, hari kerja standar (22 hari), tanggal cut-off (20), tanggal bayar (25), batas persentase variansi gaji (15%).
2. `Areas/Corporate/HumanResource/PayrollManagement/Models/TrxPayrollVarianceAlert.cs`:
   - Relasi ke `TrxPayrollRun`, `TrxPayrollRunEmployee`, dan `MstWorkforceProfile`.
   - Kolom audit: `PreviousNetPay`, `CurrentNetPay`, `VariancePercentage`, `VarianceAmount`, `VarianceReason`, `ReviewStatus`, `ReviewedByUserId`.
3. `Areas/Corporate/HumanResource/PayrollManagement/Models/TrxPayrollHandoffBatch.cs`:
   - Menyimpan rekam jejak serah terima batch gaji ke Modul Finance: `HandoffBatchNumber`, `HandoffType`, `TotalRecords`, `TotalDisbursementAmount`, `ChecksumHash`, `IsAcknowledged`, `AcknowledgedAt`.

### B. Konfigurasi Entity Framework Core (Fluent API)
1. `Repositories/Configurations/Corporate/HumanResource/PayrollManagement/HrdPayrollSettingConfiguration.cs`
2. `Repositories/Configurations/Corporate/HumanResource/PayrollManagement/TrxPayrollVarianceAlertConfiguration.cs`
3. `Repositories/Configurations/Corporate/HumanResource/PayrollManagement/TrxPayrollHandoffBatchConfiguration.cs`
4. Pendaftaran `DbSet<HrdPayrollSetting>`, `DbSet<TrxPayrollVarianceAlert>`, dan `DbSet<TrxPayrollHandoffBatch>` pada `ApplicationDbContext.cs`.

### C. Berkas Migrasi Entity Framework Core
* **Nama Berkas**: `Migrations/20261105150000_AddPayrollV2CoreEntitiesAndIndices.cs`
* Berisi definisi tabel PostgreSQL lengkap beserta tipe data presisi moneter `decimal(18,2)`, foreign key cascade terkendali, dan indeks performa pencarian (`idx_payroll_variance_run_emp`, `idx_payroll_handoff_number`).
* *Catatan Operasional*: Sesuai wewenang kerja mandiri pengguna, eksekusi perintah `dotnet ef database update` diserahkan secara mandiri kepada pihak pengguna/DBA.

---

## 4. Alur Proses Bisnis & Contoh Kasus Rumah Sakit
* **Kasus Nyata Rumah Sakit**:
  Seorang perawat ICU (Sdri. Ns. Siti) yang pada bulan Oktober 2026 menerima gaji bersih Rp 7.500.000, pada bulan November 2026 memperoleh penugasan lembur darurat cito intensif dan insentif khusus penanganan wabah sehingga proyeksi gaji bersihnya melonjak menjadi Rp 11.250.000 (kenaikan 50%).
* **Perilaku Sistem**:
  1. Skema `TrxPayrollVarianceAlert` secara otomatis mencatat variansi 50% tersebut (melampaui ambang batas 15% pada `HrdPayrollSetting`).
  2. Sistem menandai status `PendingReview` dan memblokir persetujuan batch final sebelum Kepala HRD menelaah rincian lembur dan memberikan catatan persetujuan verifikasi.

---

## 5. Bukti Verifikasi & Definition of Done
* [x] Model C# lengkap dengan atribut validasi data dan relasi navigasi.
* [x] Fluent API EF Core terdaftar di `ApplicationDbContext`.
* [x] Berkas migrasi database C# siap dieksekusi oleh pengguna.
* [x] Nol modifikasi yang melanggar kontrak master workforce atau struktur keuangan.
