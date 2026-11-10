# Laporan Penyelesaian Task: BE-HRD-199 Pemutakhiran Profil Sub-02 Otomatis & Snapshot Histori (INT-HR-65)

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-199`
* **Nama Task**: Pemutakhiran Profil Sub-02 Otomatis & Snapshot Histori Pasca-Approval Tiket Sah (`INT-HR-65`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 13: HR Service Management & Request
* **Slice Delivery**: `S-D4` (HR Service Management)
* **Status**: `✅ Selesai (2026-11-09)`
* **File Implementasi Utama**:
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/HrServiceManagement/Services/IWorkforceProfileSyncService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/HrServiceManagement/Services/WorkforceProfileSyncService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/HrServiceManagement/DTOs/ProfileSyncDtos.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/HrServiceManagement/Services/HrServiceTicketService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/HrServiceManagement/Controllers/HrServiceTicketController.cs`

---

## 2. Latar Belakang & Kebutuhan Bisnis Rumah Sakit
Dalam operasional harian Rumah Sakit Quilvian, tenaga kesehatan (dokter spesialis, perawat instalasi gawat darurat, apoteker) maupun staf penunjang medis kerap mengajukan permohonan perubahan data pribadi kepegawaian melalui modul Employee Self-Service (ESS), seperti:
1. **Perubahan Rekening Bank Payroll**: Penggantian bank penyalur gaji (misal pindah ke Bank Mandiri / BCA) yang membutuhkan verifikasi ketat sebelum digunakan oleh modul Sub-10 Payroll.
2. **Pembaruan Status Pernikahan & Kontak Darurat**: Penyesuaian status pernikahan dari Lajang menjadi Menikah untuk hak tunjangan keluarga BPJS Kesehatan, serta pembaruan nomor telepon darurat instalasi rawat inap.

Sebelum fitur ini diimplementasikan, staf HR harus mencatat tiket secara manual lalu membuka layar profil staf satu per satu untuk mengetik ulang data baru. Hal ini rentan kesalahan ketik (*typo*) nomor rekening payroll yang berisiko transfer gaji nyasar, serta ketiadaan jejak audit historis sebelum-sesudah (*audit snapshot trail*).

Melalui integrasi `INT-HR-65`, begitu tiket permohonan perubahan profil berstatus **`Approved`** atau **`Resolved`** oleh Supervisor/Analis SDM yang berwenang, sistem secara **otomatis, atomik, dan transaksional** memutakhirkan tabel master profil (`WfpBankAccount`, `MstEmployee`, `MstWorkforceProfile`) serta merekam bukti forensik perubahan nilai lama dan nilai baru ke dalam tabel audit log `HrdProfileChangeHistory`.

---

## 3. Skenario Riil Rumah Sakit
* **Skenario**: Ns. Dewi Kartika (Perawat Pelaksana ICU) menikah dan membuka rekening tabungan bank baru di Bank Mandiri KC RS Quilvian untuk keperluan penyaluran gaji bulanan.
* **Alur Eksekusi**:
  1. Ns. Dewi mengajukan tiket helpdesk SDM jenis *Perubahan Rekening Bank Payroll* melalui portal ESS dengan melampirkan foto buku tabungan.
  2. Analis SDM (Bpk. Rudi) memeriksa kesesuaian nama pada buku tabungan dengan KTP.
  3. Bpk. Rudi menekan tombol **"Setujui Permohonan" (Approve / Resolve)**.
  4. Sistem membuka transaksi atomik database (`IDbContextTransaction`):
     - Membaca data rekening lama Ns. Dewi di tabel `WfpBankAccount`.
     - Mengubah nomor rekening, nama bank, kantor cabang, dan menandai rekening tersebut sebagai `IsVerified = true`, `VerifiedAt = DateTime.UtcNow`, dan `VerifiedByUserId = Rudi`.
     - Menyimpan snapshot nilai lama (*old values JSON*) dan nilai baru (*new values JSON*) ke dalam tabel `HrdProfileChangeHistory`.
     - Melakukan `CommitAsync()`.
  5. Saat penggajian bulan berikutnya dijalankan pada modul Sub-10 Payroll, sistem langsung membaca rekening terverifikasi tanpa intervensi manual.

---

## 4. Spesifikasi Teknis & Endpoint API Bergaya Swagger

### Tag Grup Swagger: `[Tags("HR Services - Tickets & Queue")]`

| Method | Path Endpoint | Deskripsi Singkat | Otorisasi / Peran | Status Respons |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/corporate/hrd/services/tickets/{id}/status` | Mengubah status tiket (memicu sinkronisasi atomik profil jika status `Approved` / `Resolved`) | `SuperAdmin`, `HRAdmin`, `HRManager`, `HRStaff` | `200 OK`, `400 Bad Request` |
| `GET` | `/api/v1/corporate/hrd/services/tickets/{id}/profile-change-history` | Mengambil rekam jejak historis snapshot perubahan profil dari tiket tertentu | `SuperAdmin`, `HRAdmin`, `HRManager`, `HRStaff` | `200 OK`, `404 Not Found` |
| `GET` | `/api/v1/corporate/hrd/services/tickets/employee/{employeeId}/profile-change-history` | Mengambil seluruh riwayat pemutakhiran profil tenaga kerja spesifik | `SuperAdmin`, `HRAdmin`, `HRManager`, `HRStaff` | `200 OK` |

### Contoh Struktur Data Audit Log (`HrdProfileChangeHistory`):
```json
{
  "id": "7f8b9c1d-2e3a-4b5c-6d7e-8f9a0b1c2d3e",
  "hrServiceRequestId": "3a1c2e4f-5b6a-7d8e-9f0a-1b2c3d4e5f6a",
  "requestNumber": "TKT-202611-00042",
  "employeeId": "1b2c3d4e-5f6a-7b8c-9d0e-1f2a3b4c5d6e",
  "employeeName": "Ns. Dewi Kartika, S.Kep",
  "componentModified": "BankAccount",
  "oldValuesJson": "{\"Id\":\"9a8b...\",\"BankName\":\"Bank BNI\",\"AccountNumber\":\"0123456789\",\"AccountHolderName\":\"Dewi Kartika\",\"IsVerified\":true}",
  "newValuesJson": "{\"BankName\":\"Bank Mandiri\",\"AccountNumber\":\"1370019283746\",\"AccountHolderName\":\"Dewi Kartika\",\"BankBranch\":\"KC RS Quilvian\",\"AccountType\":\"Savings\"}",
  "approvedByUserId": "e4f5a6b7-c8d9-0e1f-2a3b-4c5d6e7f8a9b",
  "approvedByUserName": "rudi.hrd",
  "appliedAtUtc": "2026-11-09T04:30:00Z"
}
```

---

## 5. Bukti Verifikasi & Keamanan
1. **Transaksi Atomik Database (`IDbContextTransaction`)**: Bila terjadi kendala saat menyimpan snapshot histori audit, seluruh mutasi pada rekening bank atau status kepegawaian di-*rollback* secara utuh, mencegah data parsial.
2. **Kepatuhan Audit Medis & Ketenagakerjaan**: Jejak audit mencatat identitas pengguna penilai (`ApprovedByUserId`) dan stempel waktu UTC (`AppliedAtUtc`) yang tidak dapat diubah (*immutable audit trail*).
3. **Kompilasi Sukses**: Lulus kompilasi .NET dengan `0 Error(s)`.
