# Laporan Penyelesaian Backend Task: BE-HRD-162

## 1. Identitas Task
* **ID Task**: `BE-HRD-162`
* **Nama Task**: Layanan konfigurasi kebijakan plafon tahunan restitusi & persentase batas pemotongan upah (`RestitutionSettingService`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 11 — Restitusi Klaim & Integrasi Pemotongan Penggajian
* **Slice / Milestone**: `Slice S-B6` · `Fase MVP-33: Fondasi Database, Kebijakan Plafon, Engine Guard 50% & Sinkronisasi Finance`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-06
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Setiap rumah sakit memiliki jenjang golongan profesi tenaga kerja yang beragam—mulai dari Dokter Spesialis (Grade I), Perawat dan Bidan (Grade II), Penunjang Medis dan Laboran (Grade III), hingga Tenaga Administrasi dan Umum (Grade IV). Masing-masing grade memiliki hak plafon bantuan kacamata kerja dan penggantian pengobatan darurat (*faskes luar*) yang berbeda sesuai kebijakan remunerasi rumah sakit (`HRD-DEC-130`).

Selain hak plafon klaim, Pasal 65 PP 36/2021 jo PP 49/2025 secara tegas mewajibkan perlindungan upah pekerja, di mana total seluruh pemotongan non-regulasi dilarang melebihi 50% dari upah bruto bulanan (*Gross Pay*). Layanan `RestitutionSettingService` dibangun untuk mengelola aturan-aturan ini secara terpusat, fleksibel, dan terukur.

---

## 3. Komponen Teknis yang Dibuat

### A. Data Transfer Objects (DTO)
File: `Areas/Corporate/HumanResource/PayrollManagement/DTOs/RestitutionSettingDtos.cs`
* `RestitutionSettingResponseDto`: Menampilkan data kebijakan lengkap beserta timestamp audit.
* `CreateRestitutionSettingRequestDto`: Payload pendaftaran/pembaruan kebijakan dengan validasi anotasi `[Range]`, `[Required]`, dan `[MaxLength]`.
* `UpdateRestitutionSettingRequestDto`: Payload modifikasi nilai plafon, masa kerja, dan persentase limit.
* `RestitutionSettingFilterDto`: Filter pencarian berdasarkan grade, kategori manfaat, dan status keaktifan.

### B. Interface & Service Implementasi
File:
* `Areas/Corporate/HumanResource/PayrollManagement/Services/IRestitutionSettingService.cs`
* `Areas/Corporate/HumanResource/PayrollManagement/Services/RestitutionSettingService.cs`
Metode Utama:
1. `GetAllSettingsAsync`: Mengambil daftar kebijakan aktif dengan filter dinamis.
2. `GetSettingByIdAsync`: Mengambil detail satu kebijakan berdasarkan ID.
3. `GetPolicyForGradeAndCategoryAsync`: Mengambil aturan plafon dan syarat masa kerja untuk staf rumah sakit dengan fallback otomatis ke `GRADE-ALL` bila belum ada aturan khusus grade.
4. `GetMaxDeductionPercentageAsync`: Mengambil batas persentase pemotongan upah (default 50.00% sesuai PP 36/2021).
5. `CreateOrUpdateSettingAsync`: Menyimpan atau memperbarui kebijakan secara upsert aman.
6. `SeedDefaultHospitalSettingsAsync`: Menyemai data master standar rumah sakit untuk Grade-All hingga Grade-IV.

### C. Master Data Awal yang Disemai
| Golongan Pegawai | Kategori Manfaat | Plafon Tahunan | Min. Masa Kerja | Batas Maks. Potongan | Akses Kasir Cito |
| :--- | :--- | :--- | :--- | :---: | :---: |
| `GRADE-ALL` | `KebijakanUmumPemotongan` | Rp 0 | 0 bulan | **50.00%** (PP 36/2021) | Tidak |
| `GRADE-I` (Dokter Spesialis) | `KacamataKerja` | Rp 2.500.000 | 6 bulan | 50.00% | Tidak |
| `GRADE-I` (Dokter Spesialis) | `FaskesLuarDarurat` | Rp 10.000.000 | 3 bulan | 50.00% | **Ya (Cito Nakes)** |
| `GRADE-II` (Perawat & Bidan) | `KacamataKerja` | Rp 1.500.000 | 6 bulan | 50.00% | Tidak |
| `GRADE-II` (Perawat & Bidan) | `FaskesLuarDarurat` | Rp 7.500.000 | 3 bulan | 50.00% | **Ya (Cito Nakes)** |
| `GRADE-III` (Laboran/Penunjang) | `KacamataKerja` | Rp 1.250.000 | 6 bulan | 50.00% | Tidak |
| `GRADE-III` (Laboran/Penunjang) | `FaskesLuarDarurat` | Rp 5.000.000 | 3 bulan | 50.00% | **Ya (Cito Nakes)** |
| `GRADE-IV` (Staf Umum/Admin) | `KacamataKerja` | Rp 1.000.000 | 12 bulan | 50.00% | Tidak |
| `GRADE-IV` (Staf Umum/Admin) | `FaskesLuarDarurat` | Rp 3.000.000 | 12 bulan | 50.00% | Tidak |

---

## 4. Spesifikasi Endpoint REST API (Swagger)
File Controller: `Areas/Corporate/HumanResource/PayrollManagement/Controllers/RestitutionSettingController.cs`
Tag Swagger: `[Tags("HR Deductions - Settings")]`

| Method | Path | Deskripsi | Otorisasi | Request / Query | Response Model |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/hr-deductions/settings` | Daftar seluruh konfigurasi kebijakan plafon | Bearer JWT | `RestitutionSettingFilterDto` | `List<RestitutionSettingResponseDto>` |
| `GET` | `/api/hr-deductions/settings/{id}` | Detail kebijakan berdasarkan ID | Bearer JWT | `id: Guid` | `RestitutionSettingResponseDto` |
| `GET` | `/api/hr-deductions/settings/policy` | Ambil kebijakan aktif per grade & kategori | Bearer JWT | `gradeLevelCode, benefitCategory` | `RestitutionSettingResponseDto` |
| `POST` | `/api/hr-deductions/settings` | Simpan / update aturan kebijakan plafon | Bearer JWT | `CreateRestitutionSettingRequestDto` | `RestitutionSettingResponseDto` |
| `POST` | `/api/hr-deductions/settings/seed-defaults` | Semai pengaturan default rumah sakit | Bearer JWT | None | `{ message: string }` |

---

## 5. Contoh Skenario Rumah Sakit Konkret
* **Skenario Evaluasi Hak Kacamata Dokter**:
  dr. Bambang (Spesialis Bedah, Grade I) baru bekerja selama 8 bulan. Sistem mengevaluasi `GetPolicyForGradeAndCategoryAsync("GRADE-I", "KacamataKerja")`:
  - Syarat masa kerja minimal: 6 bulan $\rightarrow$ **Terpenuhi** (8 bulan $\ge$ 6 bulan).
  - Plafon tahunan yang berhak diterima: **Rp 2.500.000**.
