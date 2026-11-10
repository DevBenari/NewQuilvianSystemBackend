# Laporan Implementasi Task Backend: BE-HRD-142

## 1. Identitas Task
* **ID Task**: `BE-HRD-142`
* **Nama Task**: Background Scheduler Eksekusi Hangus Saldo CTO Kedaluwarsa 90 Hari
* **Modul / Sub-Modul**: Human Resource / Sub-modul 09 — Administrasi Lembur Pegawai Rumah Sakit (Overtime Management)
* **Slice / Milestone**: `Slice S-B3` · `Fase MVP-29`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-05
* **Branch Git**: `MHamzah`

---

## 2. Ringkasan Eksekutif & Proses Bisnis Rumah Sakit
Peraturan ketenagakerjaan dan kebijakan internal rumah sakit menetapkan bahwa hak cuti pengganti lembur (*Compensatory Time Off / CTO*) wajib dimanfaatkan oleh pegawai dalam kurun waktu 90 hari kalender sejak timbulnya hak tersebut. Tujuannya adalah agar nakes segera mengambil waktu istirahat yang proporsional dan tidak menimbun saldo libur hingga bertahun-tahun.

Melalui implementasi `BE-HRD-142`:
1. Sistem menjalankan rutinitas pemeriksaan harian untuk memindai seluruh tiket CTO aktif yang telah melewati batas `ExpiryDate` (`VR-OT-13`).
2. Sisa hari dan jam pada tiket yang kedaluwarsa secara otomatis dihanguskan (`IsExpired = true`, `CompensatoryStatus = "Expired"`).
3. **Larangan Konversi Tunai (`VR-OT-14`)**: Sisa saldo yang hangus tidak dapat dicairkan kembali menjadi uang tunai.
4. **Integrasi Mutasi Debet Sub-08 (`INT-HR-49`)**: Sistem secara otomatis memposting mutasi pengurangan saldo (`CompensatoryForfeiture`) pada buku besar `TrxLeaveBalanceLedger` agar saldo cuti pegawai berkurang secara transparan dan berjejak audit.

---

## 3. Komponen Teknis yang Diimplementasikan
1. **Interface Service**:
   - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/OvertimeManagement/Services/ICompensatoryTimeOffExpirationService.cs`
2. **Implementasi Service**:
   - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/OvertimeManagement/Services/CompensatoryTimeOffExpirationService.cs`
   - Mengevaluasi tiket kedaluwarsa, menandai hangus, memposting debet saldo cuti di `TrxLeaveBalanceLedger`, dan menghasilkan rekapitulasi audit.
3. **Endpoint Pemanggil Manual / Scheduler**:
   - Diintegrasikan pada `CompensatoryTimeOffModernController.cs` rute `POST /run-expiry`.
   - Tag Swagger: `[Tags("HR Overtime - Compensatory Time Off (CTO)")]`.
4. **DTO Kontrak Data**:
   - `OvertimeMvp29ModernDtos.cs`: `CompensatoryExpirationRunResultDto`.

---

## 4. Spesifikasi Endpoint API (Gaya Swagger)
* **Tag Swagger**: `[Tags("HR Overtime - Compensatory Time Off (CTO)")]`

| HTTP Method | Path Endpoint | Deskripsi Bisnis | Otorisasi / Role | Status Respons |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/corporate/human-resource/overtime/compensatory-time-off/run-expiry` | Memicu eksekusi penghangusan tiket CTO yang melewati 90 hari kalender | HR Admin / Scheduler Runner (`CompensatoryTimeOffModern.Execute`) | `200 OK` |

---

## 5. Bukti Verifikasi Aturan Bisnis (Acceptance Criteria)
* **VR-OT-13 (Automatic Forfeiture)**: Seluruh tiket dengan `ExpiryDate < Today` dan `RemainingHours > 0` statusnya berubah menjadi `Expired`.
* **VR-OT-14 (No Cash Conversion)**: Sisa jam hangus di-nol-kan dan tidak diikutkan dalam serah terima payroll finansial.
* **Audit Debet Transparan**: Terbentuk entri `CompensatoryForfeiture` pada `TrxLeaveBalanceLedger` dengan nilai minus hari yang dihanguskan.
