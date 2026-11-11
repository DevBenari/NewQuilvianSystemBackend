# Laporan Task Tracked: BE-HRD-251 — Perhitungan Denda Penalti Wanprestasi Resign Dini

**ID Task**: `BE-HRD-251`  
**Modul/Sub-Modul**: Human Resource / Sub-modul 16: Learning, Training & Competency Management  
**Slice**: `S-C2` (Kontrak API `v20`)  
**Gelombang**: `MVP-50` (Ikatan Dinas, Penalti Wanprestasi & Portofolio SKP Kemenkes Plataran Sehat)  
**Tanggal Eksekusi**: 2026-11-09  
**Status**: `✅ Selesai`

---

## 1. Ringkasan Eksekutif
Task ini mengimplementasikan engine komputasi ganti rugi denda finansial akibat wanprestasi pengunduran diri dini (resign) selama periode ikatan dinas aktif (`HrdTrainingBondContract`, `VR-TRN-17`):
- **Formula Prorata Ganti Rugi**:
  $$\text{Denda} = \left(\frac{\text{Sisa Hari Retensi}}{\text{Total Hari Ikatan Dinas}}\right) \times \text{Pembiayaan Rumah Sakit} \times \text{Multiplier Factor}$$
- **Penerbitan Invoice Penalti**: Menerbitkan nomor tagihan resmi `INV-PENALTY-HRD-{Year}-{Sequence:D5}` untuk ditagihkan kepada staf bersangkutan atau diperhitungkan dalam formulir exit clearance.
- **Transisi Status Kontrak**: Memperbarui status kontrak ikatan dinas dari `ACTIVE` menjadi `BREACHED_PENALTY_ISSUED`, atau `FULFILLED` jika masa kontrak ternyata telah selesai saat tanggal efektif resign.

---

## 2. File yang Dibuat / Dimodifikasi
1. **DTO**:
   - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/DTOs/TrainingBondingDtos.cs`
     - `ProcessBreachPenaltyRequestDto`, `TrainingBondContractDto`.
2. **Service**:
   - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Services/ITrainingBondingService.cs`
   - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Services/TrainingBondingService.cs`
     - Validasi status kontrak aktif.
     - Penghitungan sisa masa hari retensi dan rasio prorata.
     - Komputasi besaran nilai denda kompensasi finansial (`CalculatedPenaltyAmount`).
     - Alokasi nomor seri invoice penalti tahunan.
     - Penguncian status pelanggaran kontrak (`IsBreached = true`).
3. **Controller**:
   - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Controllers/TrainingBondingsController.cs`
     - `POST /api/v1/corporate/hrd/training/bonding-contracts/{contractId}/process-breach-penalty`
4. **Pendaftaran Dependency Injection**:
   - `NewQuilvianSystemBackend/Program.cs` (`builder.Services.AddScoped<ITrainingBondingService, TrainingBondingService>()`).

---

## 3. Verifikasi Acceptance Criteria & Invarian
- **`VR-TRN-17`**: Formula penalti prorata berjalan presisi dan menghasilkan nilai denda yang adil dan transparan.
- **`AT-HRD-TRN-18`**: Pemrosesan wanprestasi menghasilkan nomor invoice dan denda ganti rugi terhitung secara otomatis.
