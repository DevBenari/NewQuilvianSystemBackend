# Laporan Penyelesaian Task Backend: BE-HRD-132

## 1. Informasi Task

| Atribut | Nilai |
| :--- | :--- |
| **ID Task** | `BE-HRD-132` |
| **Nama Task** | Service pengaturan kebijakan kuota & parameter operasional lembur RS (`OvertimeSettingService`) |
| **Sub-Modul** | 09. Overtime Management (Administrasi Lembur Pegawai Rumah Sakit) |
| **Slice / Milestone** | `S-B3` · `MVP-27` |
| **Tanggal Selesai** | 2026-11-05 |
| **Status Konformansi** | ✅ Selesai (Memenuhi Seluruh Kriteria Penerimaan) |
| **Rujukan Keputusan** | `HRD-DEC-117`, `HRD-DEC-118`, `HRD-DEC-121`, `HRD-ASM-23`, `HRD-ASM-25` |

---

## 2. Deskripsi & Tujuan Bisnis

Untuk menegakkan kepatuhan hukum ketenagakerjaan dan transparansi administrasi rumah sakit, seluruh batasan waktu, ambang batas, dan jadwal cut-off lembur tidak boleh di-*hardcode* di dalam kode aplikasi yang kaku, melainkan dikelola terpusat melalui service parameter operasional.

Task ini menyediakan layanan konfigurasi dan pembacaan parameter operasional lembur rumah sakit yang mencakup:
1. **Kuota Regulasi PP 35/2021 (`HRD-DEC-117`)**: Batas maksimal 4 jam per hari dan 18 jam per minggu kalender (Senin s.d. Minggu).
2. **Aturan Pembulatan & Ambang Minimal (`HRD-ASM-23`, `OT-B8`)**: Interval pembulatan durasi lembur fisik ke kelipatan 30 menit ke bawah (*30-minute floor interval*), dengan durasi di bawah 30 menit bernilai 0 jam kompensasi.
3. **Masa Berlaku Saldo Cuti Pengganti (CTO) (`HRD-DEC-118`, `HRD-ASM-25`)**: Masa kedaluwarsa saldo libur pengganti selama 90 hari kalender (3 bulan) sejak diterbitkan.
4. **Tanggal Cut-off Bulanan (`HRD-DEC-121`)**: Tanggal penutupan transaksi lembur bulanan (default tanggal 20 setiap bulan).
5. **Toleransi Anomali Mesin Biometrik (`HRD-DEC-119`)**: Toleransi perbedaan waktu antara SPKL dan tap mesin presensi maksimal 30 menit sebelum ditandai sebagai *Flagged Discrepancy*.
6. **Tenggat Pelaporan Klaim Cito Pasca-Dinas (`HRD-DEC-116`)**: Batas maksimal pelaporan darurat medis cito H+1 hari kerja.

---

## 3. Komponen yang Dikerjakan

1. **DTOs (`OvertimeSettingDtos.cs`)**:
   - `OvertimeSettingDto`: Data transfer object lengkap pembacaan konfigurasi lembur RS.
   - `UpdateOvertimeSettingCommand`: Command pembaruan parameter kuota dan operasional lembur.

2. **Interface & Service (`IOvertimeSettingService.cs` & `OvertimeSettingService.cs`)**:
   - `GetSettingsAsync`: Mengembalikan parameter kuota baku dan operasional aktif dengan *thread-safe caching*.
   - `UpdateSettingsAsync`: Memvalidasi dan memperbarui parameter kebijakan dengan pencatatan jejak audit perubahan.

3. **Controller Bergaya Swagger (`OvertimeSettingController.cs`)**:
   - Tag grup Swagger: `[Tags("HR Overtime - Settings")]`.
   - Rute ganda: `/api/v1/corporate/human-resource/overtime-settings` dan `/api/overtime-settings`.
   - Endpoint:
     - `GET /`: Mengambil konfigurasi kuota dan parameter aktif.
     - `PUT /`: Memperbarui konfigurasi parameter operasional lembur.

---

## 4. Contoh Skenario Penggunaan Nyata di Rumah Sakit

### Skenario: Penyesuaian Tanggal Cut-off Libur Lebaran
* **Kondisi**: Manajemen rumah sakit memutuskan memajukan cut-off lembur dari tanggal 20 menjadi tanggal 17 karena libur bersama Idul Fitri.
* **Tindakan HR Admin**:
  - HR Admin memanggil endpoint `PUT /api/v1/corporate/human-resource/overtime-settings` dengan parameter `MonthlyCutoffDay = 17`.
  - Service mencatat perubahan pada log audit sistem dan seluruh proses validasi cut-off otomatis merujuk ke tanggal 17 untuk periode berjalan.

---

## 5. Kepatuhan Kriteria Penerimaan

| Kriteria Penerimaan | Status | Bukti Implementasi |
| :--- | :---: | :--- |
| Service mengembalikan parameter kuota regulasi harian (4 jam) dan mingguan (18 jam) | ✅ Lolos | Terkonfigurasi pada `DailyMaxHoursRegular = 4m` dan `WeeklyMaxHoursRegular = 18m` sesuai PP 35/2021. |
| Service mengembalikan parameter pembulatan kelipatan 30 menit ke bawah dan ambang batas minimal 30 menit | ✅ Lolos | Terkonfigurasi pada `RoundingIntervalMinutes = 30` dan `MinimumThresholdMinutes = 30`. |
| Service mengembalikan masa berlaku saldo CTO selama 90 hari kalender | ✅ Lolos | Terkonfigurasi pada `CtoValidityDays = 90`. |
| Endpoint controller bergaya Swagger terpasang dengan izin akses resmi | ✅ Lolos | Tag `[Tags("HR Overtime - Settings")]` dan atribut `[AccessPermission("OvertimeSetting", ...)]` terpasang. |
