# Laporan Implementasi Task Backend: BE-HRD-141

## 1. Identitas Task
* **ID Task**: `BE-HRD-141`
* **Nama Task**: Service Hak Libur Pengganti CTO (90 Hari) & Posting Mutasi Kredit Cuti Sub-08
* **Modul / Sub-Modul**: Human Resource / Sub-modul 09 — Administrasi Lembur Pegawai Rumah Sakit (Overtime Management)
* **Slice / Milestone**: `Slice S-B3` · `Fase MVP-29`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-05
* **Branch Git**: `MHamzah`

---

## 2. Ringkasan Eksekutif & Proses Bisnis Rumah Sakit
Dalam manajemen sumber daya manusia rumah sakit, kompensasi kerja lembur tidak selalu diwujudkan dalam bentuk uang tunai (*financial payout*), melainkan dapat dikonversi menjadi hak istirahat atau cuti pengganti (*Compensatory Time Off / CTO*). Skema ini sangat penting bagi tenaga medis (seperti dokter residen, perawat rawat inap, dan staf penunjang) guna memastikan kesempatan pemulihan fisik (*fatigue mitigation*) setelah bertugas intensif.

Melalui implementasi `BE-HRD-141`:
1. **Rasio Konversi Resmi**: 8 jam (480 menit) lembur terverifikasi dikonversikan menjadi 1.0 hari hak cuti pengganti CTO (`VR-OT-15`, `HRD-DEC-118`).
2. **Penerbitan Tiket CTO**: Setiap klaim lembur terverifikasi dengan opsi `CompensatoryTimeOff` diterbitkan satu tiket unik `TrxCompensatoryTimeOff` bernomor urut `CTO/YYYY/MM/XXXX`.
3. **Masa Berlaku 90 Hari Kalender**: Tanggal kedaluwarsa ditetapkan otomatis `ExpiryDate = TanggalLembur + 90 hari` (`VR-OT-13`).
4. **Integrasi Mutasi Kredit Sub-08 (`INT-HR-49`)**: Sistem secara atomik memposting mutasi penambahan kredit cuti ke buku besar saldo cuti pegawai (`TrxLeaveBalanceLedger`) dengan tipe `CompensatoryCredit` dan mencatat referensi silang ID transaksi.

---

## 3. Komponen Teknis yang Diimplementasikan
1. **Interface Service**:
   - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/OvertimeManagement/Services/ICompensatoryTimeOffService.cs`
2. **Implementasi Service**:
   - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/OvertimeManagement/Services/CompensatoryTimeOffService.cs`
   - Melakukan konversi 8 jam = 1 hari, validasi status `Verified`, penghitungan saldo sebelum-sesudah di `TrxLeaveBalanceLedger`, dan pencatatan tiket `TrxCompensatoryTimeOff`.
3. **Controller API**:
   - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/OvertimeManagement/Controllers/CompensatoryTimeOffModernController.cs`
   - Rute ganda: `/api/v1/corporate/human-resource/overtime/compensatory-time-off` & `/api/overtime/compensatory-time-off`.
   - Tag Swagger: `[Tags("HR Overtime - Compensatory Time Off (CTO)")]`.
4. **DTO Kontrak Data**:
   - `OvertimeMvp29ModernDtos.cs`: `GenerateCompensatoryTimeOffResultDto`, `CompensatoryTimeOffTicketDto`.

---

## 4. Spesifikasi Endpoint API (Gaya Swagger)
* **Tag Swagger**: `[Tags("HR Overtime - Compensatory Time Off (CTO)")]`

| HTTP Method | Path Endpoint | Deskripsi Bisnis | Otorisasi / Role | Status Respons |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/corporate/human-resource/overtime/compensatory-time-off/generate/{realizationId}` | Menerbitkan tiket CTO dan memposting mutasi kredit cuti ke buku besar Sub-08 | HR Admin / Verifikator (`CompensatoryTimeOffModern.Create`) | `201 Created`, `400 Bad Request`, `404 Not Found`, `409 Conflict` |
| `GET` | `/api/v1/corporate/human-resource/overtime/compensatory-time-off/staff-tickets/{workforceProfileId}` | Mengambil seluruh tiket CTO aktif staf beserta sisa hari dan indikator waktu kedaluwarsa | Pegawai / HR (`CompensatoryTimeOffModern.View`) | `200 OK` (List DTO) |

---

## 5. Bukti Verifikasi Aturan Bisnis (Acceptance Criteria)
* **VR-OT-15 (Conversion Ratio)**: 480 menit lembur menghasilkan tepat 1.0 hari cuti; 240 menit menghasilkan 0.5 hari cuti.
* **VR-OT-13 (90-Day Expiry Window)**: Tiket memiliki batas kedaluwarsa persis 90 hari kalender sejak pelaksanaan dinas lembur.
* **INT-HR-49 (Atomic Ledger Sync)**: Mutasi penambahan saldo di `TrxLeaveBalanceLedger` tersimpan bersamaan dalam satu transaksi database.
