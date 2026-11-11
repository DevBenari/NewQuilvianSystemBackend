# Laporan Penyelesaian Backend Task: BE-HRD-165

## 1. Identitas Task
* **ID Task**: `BE-HRD-165`
* **Nama Task**: Engine prioritas berjenjang 1-4, pemangkasan parsial & penjadwalan sisa carry-over otomatis ke bulan berikutnya (`DED-B2`, `DED-B3`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 11 — Restitusi Klaim & Integrasi Pemotongan Penggajian
* **Slice / Milestone**: `Slice S-B6` · `Fase MVP-33: Fondasi Database, Kebijakan Plafon, Engine Guard 50% & Sinkronisasi Finance`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-06
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Ketika total kewajiban cicilan seorang pegawai rumah sakit melampaui batas pagu 50% Gross Pay (`DED-B1`), sistem tidak boleh membatalkan seluruh potongan secara serampangan atau memotong cicilan tanpa aturan yang jelas. 

Sesuai konsensus bisnis `HRD-DEC-128`, ditetapkan aturan operasional mutlak:
1. **Hirarki Prioritas Berjenjang Mutlak 1 s.d. 4 (`DED-B2`)**:
   - **Prioritas 1 — Ganti Rugi Kasir / Kerusakan Aset RS (`GantiRugiAsetKasir`)**: Kewajiban pertama yang wajib diselesaikan demi kepatuhan pertanggungjawaban kas dan aset medis.
   - **Prioritas 2 — Pinjaman Kasbon Pokok Rumah Sakit (`KasbonPinjamanRS`)**: Piutang resmi operasional rumah sakit kepada staf.
   - **Prioritas 3 — Cicilan Koperasi Karyawan Rumah Sakit (`CicilanKoperasi`)**: Tagihan simpan pinjam atau belanja mitra koperasi.
   - **Prioritas 4 — Iuran Sosial Paguyuban (`IuranPaguyuban`)**: Iuran sukarela/paguyuban karyawan RS.
2. **Pemangkasan Parsial & Carry-Over Otomatis Tanpa Denda Bunga (`DED-B3`, `VR-DED-03`)**:
   Komponen yang kuotanya tidak mencukupi dipotong sebagian (`PartiallyDeducted`), dan sisanya diberi status `DeferredDueToSalaryCap` untuk secara otomatis dijadwalkan ulang ke bulan berikutnya tanpa dikenakan denda bunga maupun penalti tambahan.

---

## 3. Komponen Teknis yang Dibuat

### A. Algoritma Alokasi Berjenjang & Carry-Over
File: `Areas/Corporate/HumanResource/PayrollManagement/Services/PayrollDeductionEngineService.cs`
Metode: `ExecutePayrollDeductionsAsync` dan `ExecuteBatchPayrollDeductionsAsync`.
Alur Logika:
1. Ambil seluruh jadwal cicilan aktif (`TrxPayrollDeductionSchedule`) ber-saldo pokok $> 0$, urutkan secara mutlak `OrderBy(PriorityLevel).ThenBy(StartDate)`.
2. Inisialisasi kuota tersedia: `remainingQuota = 50% * GrossPay`.
3. Telusuri setiap cicilan:
   - Jika `scheduledAmount <= remainingQuota`:
     * Potong penuh: `actualDeductedAmount = scheduledAmount`, `deferredAmount = 0m`.
     * Status eksekusi: `DeductedInFull`.
     * Kurangi kuota: `remainingQuota -= scheduledAmount`.
   - Jika `scheduledAmount > remainingQuota` dan `remainingQuota > 0`:
     * Potong sebagian: `actualDeductedAmount = remainingQuota`, `deferredAmount = scheduledAmount - remainingQuota`.
     * Status eksekusi: `PartiallyDeducted`.
     * Alasan tunda: Dicatat resmi pada `DeferralReason`.
     * Kuota habis: `remainingQuota = 0m`.
   - Jika `remainingQuota == 0`:
     * Tidak terpotong: `actualDeductedAmount = 0m`, `deferredAmount = scheduledAmount`.
     * Status eksekusi: `DeferredDueToSalaryCap`.
     * Alasan tunda: Dicatat bahwa kuota 50% telah habis teralokasi oleh prioritas lebih tinggi.
4. **Persistensi & Mutasi Saldo**:
   - Simpan entitas rincian eksekusi pada `TrxPayrollDeductionItem`.
   - Kurangi sisa pokok pinjaman pada jadwal (`RemainingPrincipal -= actualDeductedAmount`).
   - Jika pokok pinjaman telah habis, tandai `ScheduleStatus = "Completed"`.
   - Jadwal dengan sisa tertunda otomatis siap diproses kembali pada siklus penggajian bulan berikutnya (`VR-DED-03`).

---

## 4. Spesifikasi Endpoint REST API (Swagger)
File Controller: `Areas/Corporate/HumanResource/PayrollManagement/Controllers/PayrollDeductionScheduleController.cs`
Tag Swagger: `[Tags("HR Deductions - Engine")]`

| Method | Path | Deskripsi | Otorisasi | Request / Query | Response Model |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/hr-deductions/engine/execute` | Eksekusi pemotongan gaji payroll run ber-hard-cap 50% & carry-over | Bearer JWT | `ProcessPayrollDeductionsRequestDto` | `DeductionSimulationResultDto` |

---

## 5. Contoh Skenario Rumah Sakit Konkret
* **Skenario Nyata Pemangkasan Parsial Koperasi Akibat Mentok 50%**:
  - Gaji Kotor Pegawai: **Rp 5.000.000** $\rightarrow$ Batas Maksimal 50%: **Rp 2.500.000**.
  - Daftar Kewajiban Pegawai:
    * Tagihan A (Prioritas 1 - Selisih Kasir Farmasi): **Rp 1.000.000**
    * Tagihan B (Prioritas 2 - Kasbon Emergency RS): **Rp 1.000.000**
    * Tagihan C (Prioritas 3 - Belanja Koperasi RS): **Rp 1.000.000**
  - **Hasil Eksekusi Engine**:
    1. Tagihan A (Prioritas 1): Terpotong **Rp 1.000.000** (Penuh). Sisa kuota: Rp 1.500.000.
    2. Tagihan B (Prioritas 2): Terpotong **Rp 1.000.000** (Penuh). Sisa kuota: Rp 500.000.
    3. Tagihan C (Prioritas 3): Kuota hanya tersisa Rp 500.000 $\rightarrow$ Terpotong parsial **Rp 500.000**, sisa **Rp 500.000 ditunda (*carry-over*)** ke bulan berikutnya berstatus `PartiallyDeducted` tanpa bunga (`DED-B3`).
  - **Total Terpotong**: Tepat **Rp 2.500.000 (50.00%)**. Hak hidup layak karyawan terlindungi 100%.
