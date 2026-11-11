# Laporan Penyelesaian Backend Task: BE-HRD-187

## 1. Identitas Task
* **ID Task**: `BE-HRD-187`
* **Nama Task**: Buku besar mutasi saldo subsidi plafon (`HrdBenefitBalanceMovement`) & penanganan ekses tagihan medis ke payroll Sub-11 (`INT-HR-63`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 12 — Benefit Management (Manajemen Manfaat, Asuransi BPJS & Tanggungan Pegawai Rumah Sakit)
* **Slice / Milestone**: `Slice S-D3` · `Fase MVP-38: Integrasi SIMRS Kasir, Buku Besar Plafon, Klaim COB & Kliring Exit Clearance`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-09
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Integritas finansial subsidi kesehatan internal rumah sakit bergantung pada pencatatan transaksi yang transparan, dapat diaudit (*auditable*), dan tidak dapat dimanipulasi secara sepihak. Setiap penambahan kuota, pemakaian perawatan kasir, dan penyesuaian wajib tercatat dalam format buku besar mutasi berganda (*double-entry balance movement ledger*).

Fitur-fitur utama yang disediakan:
1. **Histori Mutasi Buku Besar Plafon (`FR-BEN-14`)**:
   - Merekam setiap pergerakan saldo:
     * `AnnualAllocation`: Alokasi pagu tahunan awal (nominal positif).
     * `HospitalCareUsage`: Pemakaian berobat di loket kasir SIMRS (nominal negatif).
     * `ManualAdjustment`: Koreksi manual berizin HR Manajer (bisa positif/negatif).
     * `YearEndExpiryReset`: Pembersihan sisa plafon hangus per 1 Januari.
   - Setiap baris mutasi menyimpan potret saldo sebelum (`BalanceBefore`) dan saldo sesudah (`BalanceAfter`).
2. **Penyesuaian Manual Berita Acara (`ManualAdjustment`)**:
   - HR Manajer dapat melakukan penyesuaian saldo (misal: pengembalian kuota akibat retur obat SIMRS atau pembatalan tindakan radiologi).
   - Penyesuaian wajib mencantumkan alasan formal minimal 10 karakter dan nomor referensi pembatalan SIMRS.
3. **Pendelegasian Ekses Biaya Medis ke Pemotongan Payroll Sub-11 (`INT-HR-63`, `FR-BEN-15`)**:
   - Jika staf berobat di RS Quilvian dan biaya perawatannya melampaui sisa plafon tahunan, sisa tagihan yang tidak tertutup dapat didelegasikan ke modul Payroll Sub-11 (`S-B6`) untuk dipotongkan dari gaji bulanan staf secara bertahap atau sekaligus, sehingga kasir RS tidak perlu menahan nakes di loket.

---

## 3. Komponen Teknis yang Dibuat & Dimodifikasi

### A. Model Basis Data Entity Framework Core
File: `Areas/Corporate/HumanResource/BenefitManagement/Models/HrdBenefitBalanceMovement.cs`
* Kolom: `Id`, `WorkforceProfileId`, `MovementType`, `Amount`, `BalanceBefore`, `BalanceAfter`, `ReferenceNumber`, `Description`, `OccurredAt`, `ActorUserId`.
* Check constraint: Saldo akhir tidak boleh negatif (`BalanceAfter >= 0.00`).

### B. Data Transfer Objects (DTO)
File: `Areas/Corporate/HumanResource/BenefitManagement/DTOs/BenefitInHouseAndClaimDtos.cs`
* `BenefitBalanceMovementDto`: Format serialisasi riwayat mutasi untuk ditampilkan pada portal ESS atau layar HR.
* `AdjustBalanceManualRequestDto`: Payload penyesuaian manual memuat ID pegawai, nominal penyesuaian, alasan, dan nomor referensi pembatalan.

### C. Logika Bisnis & Validasi Layanan
File: `Areas/Corporate/HumanResource/BenefitManagement/Services/BenefitInHouseService.cs`
* Metode: `GetLedgerMovementsAsync` dan `AdjustLedgerBalanceAsync`.
* Validasi alasan penyesuaian minimal 10 karakter dan pencatatan audit log `[AUDIT-BEN-07]`.

---

## 4. Contoh Skenario Rumah Sakit Konkret
* **Skenario 1 — Pengembalian Saldo Akibat Pembatalan Tindakan USG**:
  - Dokter spesialis membatalkan permohonan tindakan USG Abdomen staf yang sebelumnya sudah terpotong di kasir sebesar Rp 450.000.
  - HR Manajer memanggil `POST /ledger/adjust` dengan `AdjustmentAmount = 450000`, `Reason = "Pembatalan USG Abdomen SIMRS No REG-20261109-12"`, `ReferenceNumber = "REV-SIMRS-88"`.
  - Saldo staf bertambah kembali Rp 450.000 (misal dari Rp 2.000.000 menjadi Rp 2.450.000).
  - Mutasi `ManualAdjustment` tercatat rapi di buku besar.

* **Skenario 2 — Delegasi Ekses Tagihan ke Slip Gaji Bulan Berjalan**:
  - Perawat Anton mengalami operasi darurat usus buntu dengan sisa ekses Rp 1.500.000.
  - Kasir memilih opsi *Pendelegasian Ekses ke Payroll*.
  - Event `INT-HR-63` menerbitkan transaksi potongan upah di Sub-11 dengan kategori `MedicalCareExcess` yang akan memotong gaji Anton pada tanggal gajian 25 November.

---

## 5. Verifikasi & Integritas Bukti
* **Audit Trail Buku Besar**: Seluruh pergerakan saldo memiliki referensi transaksi yang jelas.
* **Kepatuhan Acceptance Test**: Memenuhi kriteria penerimaan `AT-HRD-BEN-12` s.d. `AT-HRD-BEN-14`.
