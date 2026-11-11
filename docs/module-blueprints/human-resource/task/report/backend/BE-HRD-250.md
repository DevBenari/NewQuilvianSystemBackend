# Laporan Task Tracked: BE-HRD-250 — Generator Akta Ikatan Dinas Retensi Pelatihan Eksternal / Berbiaya Tinggi

**ID Task**: `BE-HRD-250`  
**Modul/Sub-Modul**: Human Resource / Sub-modul 16: Learning, Training & Competency Management  
**Slice**: `S-C2` (Kontrak API `v20`)  
**Gelombang**: `MVP-50` (Ikatan Dinas, Penalti Wanprestasi & Portofolio SKP Kemenkes Plataran Sehat)  
**Tanggal Eksekusi**: 2026-11-09  
**Status**: `✅ Selesai`

---

## 1. Ringkasan Eksekutif
Task ini mengimplementasikan generator penerbitan akta perjanjian kontrak ikatan dinas retensi tenaga kesehatan (`HrdTrainingBondContract`) bagi program pelatihan berbiaya tinggi atau berdurasi panjang:
- **Ambang Pemicu Ikatan Dinas**: Pelatihan yang dibiayai rumah sakit (`HospitalFinancedAmount`) > Rp 10.000.000,00 atau memiliki durasi penyelenggaraan > 30 hari kalender (`VR-TRN-16`).
- **Batasan Durasi Retensi**: Jangka waktu ikatan dinas wajib retensi kerja ditetapkan antara **12 sampai 36 bulan** (`VR-TRN-18`), dihitung sejak tanggal dimulainya ikatan dinas.
- **Standar Penomoran Akta**: Format nomor kontrak otomatis berurutan tahunan: `BOND-HRD-{Year}-{Sequence:D5}`.
- **Integritas Kontrak**: Menyertakan tanda tangan digital (`DigitalSignatureHash`) para pihak untuk memastikan keabsahan hukum dan akuntabilitas audit rumah sakit.

---

## 2. File yang Dibuat / Dimodifikasi
1. **DTO**:
   - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/DTOs/TrainingBondingDtos.cs`
     - `GenerateBondContractRequestDto`, `TrainingBondContractDto`.
2. **Service**:
   - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Services/ITrainingBondingService.cs`
   - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Services/TrainingBondingService.cs`
     - Validasi profil tenaga kerja di master profile.
     - Validasi ambang batas biaya rumah sakit dan batas masa retensi 12-36 bulan (`VR-TRN-16`, `VR-TRN-18`).
     - Pembuatan nomor kontrak unik berurutan dan penyimpanan entitas `HrdTrainingBondContract`.
3. **Controller**:
   - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Controllers/TrainingBondingsController.cs`
     - `POST /api/v1/corporate/hrd/training/bonding-contracts`
     - `GET /api/v1/corporate/hrd/training/bonding-contracts/{contractId}`
     - `GET /api/v1/corporate/hrd/training/bonding-contracts/staff/{workforceProfileId}`
     - `GET /api/v1/corporate/hrd/training/bonding-contracts/active`
4. **Pendaftaran Dependency Injection**:
   - `NewQuilvianSystemBackend/Program.cs` (`builder.Services.AddScoped<ITrainingBondingService, TrainingBondingService>()`).

---

## 3. Verifikasi Acceptance Criteria & Invarian
- **`VR-TRN-16`**: Ambang pemicu ikatan dinas tervalidasi dengan akurat.
- **`VR-TRN-18`**: Pembatasan durasi ikatan dinas maksimal 36 bulan diterapkan secara konsisten.
- **`AT-HRD-TRN-17`**: Pembuatan akta kontrak ikatan dinas retensi tenaga kerja berhasil diverifikasi dan disimpan dengan status `ACTIVE`.
