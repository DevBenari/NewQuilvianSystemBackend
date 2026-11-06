# Laporan Penyelesaian Backend Task: BE-HRD-147

## 1. Identitas Task
* **ID Task**: `BE-HRD-147`
* **Nama Task**: Manajemen parameter konfigurasi master penggajian & pembekuan snapshot periode (`PayrollSettingService`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 10 — Pemrosesan Penggajian Pegawai Rumah Sakit (Payroll V2)
* **Slice / Milestone**: `Slice S-B5` · `Fase MVP-30: Fondasi Siklus Penggajian & Penghasilan Tetap`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-05
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Konfigurasi penggajian rumah sakit tidak boleh bersifat statis hardcoded di dalam program atau berubah secara tidak terkontrol di tengah siklus penggajian. Ketika batch penggajian bulanan diinisiasi, seluruh parameter acuan hukum (tarif BPJS Ketenagakerjaan, BPJS Kesehatan, plafon upah maksimal, tanggal cut-off, dan toleransi variansi) wajib dibekukan ke dalam snapshot JSON yang *immutable*. 

Hal ini menjamin bahwa jika pemerintah atau manajemen RS mengubah tarif BPJS pada pertengahan bulan, kalkulasi batch penggajian bulan berjalan tetap konsisten menggunakan acuan parameter saat batch pertama kali dibuka.

---

## 3. Komponen yang Diimplementasikan

### A. Data Transfer Objects (DTO)
* Berkas: `Areas/Corporate/HumanResource/PayrollManagement/DTOs/PayrollSettingDtos.cs`
* DTO yang dibuat:
  - `HrdPayrollSettingDto`: Representasi pembacaan data konfigurasi penggajian aktif.
  - `UpdateHrdPayrollSettingCommand`: Perintah pembaruan parameter penggajian dengan validasi atribut rentang persentase (0 s.d. 100%) dan nominal plafon positif.

### B. Antarmuka & Layanan Bisnis
* Berkas:
  - `Areas/Corporate/HumanResource/PayrollManagement/Services/IPayrollSettingService.cs`
  - `Areas/Corporate/HumanResource/PayrollManagement/Services/PayrollSettingService.cs`
* Fitur Utama:
  1. `GetSettingAsync`: Mengambil konfigurasi aktif atau menginisiasi konfigurasi default standar regulasi Indonesia jika data belum ada di database.
  2. `UpdateSettingAsync`: Memperbarui nilai parameter konfigurasi dengan pencatatan audit siapa yang mengubah (`UpdatedBy`) dan waktu pembaruan (`UpdatedDate`).
  3. `GetSettingSnapshotJsonAsync`: Mengonversi konfigurasi aktif menjadi format teks JSON ringkas untuk ditempelkan ke kolom `ConfigurationSnapshotJson` pada `TrxPayrollRun`.

### C. Controller REST API Bergaya Swagger
* Berkas: `Areas/Corporate/HumanResource/PayrollManagement/Controllers/PayrollSettingModernController.cs`
* Tag Swagger: `[Tags("HR Payroll - Settings")]`
* Rute Ganda: `/api/v1/corporate/human-resource/payroll/settings` dan `/api/payroll/settings`

| Method | Path | Deskripsi | Otorisasi & Akses | Request Body | Response Body |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/corporate/human-resource/payroll/settings` | Membaca konfigurasi tarif BPJS, plafon, dan ambang variansi aktif | Bearer JWT (`Read`) | - | `ApiResponse<HrdPayrollSettingDto>` |
| `PUT` | `/api/v1/corporate/human-resource/payroll/settings` | Memperbarui parameter master konfigurasi penggajian | Bearer JWT (`Update`) | `UpdateHrdPayrollSettingCommand` | `ApiResponse<HrdPayrollSettingDto>` |

---

## 4. Alur Proses Bisnis & Skenario Rumah Sakit
1. **Inisialisasi Awal**: Saat rumah sakit pertama kali menjalankan sistem payroll, `PayrollSettingService` secara otomatis menyiapkan parameter standar:
   - JKK: 0,89% (tingkat risiko kerja medis rumah sakit).
   - JKM: 0,30%.
   - JHT: 3,70% pemberi kerja, 2,00% pekerja.
   - JP: 2,00% pemberi kerja, 1,00% pekerja (plafon Rp 10.042.300).
   - BPJS Kesehatan: 4,00% RS, 1,00% pekerja (plafon Rp 12.000.000).
   - Cut-off presensi/lembur: Tanggal 20 setiap bulan.
   - Tanggal gajian: Tanggal 25 setiap bulan.
   - Ambang variansi peringatan: 15,00%.
2. **Pembekuan Snapshot**: Saat batch penggajian bulan November dibuka pada tanggal 21 November, `PayrollSettingService.GetSettingSnapshotJsonAsync()` menghasilkan salinan konfigurasi JSON yang dikunci mati pada `TrxPayrollRun`. Apabila pada tanggal 23 November pemerintah mengumumkan kenaikan plafon BPJS Kesehatan menjadi Rp 13.000.000, pembaruan master setting tidak akan mengubah batch bulan berjalan yang sedang diproses.

---

## 5. Bukti Verifikasi & Definition of Done
* [x] Service CRUD master setting beroperasi aman dengan pemulihan default otomatis.
* [x] Snapshot JSON serialize menghasilkan metadata parameter penggajian yang valid.
* [x] Controller REST API dilengkapi dokumentasi Swagger `[Tags("HR Payroll - Settings")]`.
* [x] Pendaftaran dependency injection terdaftar pada `Program.cs`.
