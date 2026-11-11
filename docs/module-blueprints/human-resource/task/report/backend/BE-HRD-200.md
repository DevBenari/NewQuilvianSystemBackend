# Laporan Penyelesaian Task: BE-HRD-200 Penarikan Data Gaji Final dari Sub-10 Payroll & Anti-Manual Guard (INT-HR-56)

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-200`
* **Nama Task**: Penarikan Data Gaji Final dari Sub-10 Payroll (`INT-HR-56`) & Anti-Manual Guard (`VR-HRS-06`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 13: HR Service Management & Request
* **Slice Delivery**: `S-D4` (HR Service Management)
* **Status**: `✅ Selesai (2026-11-09)`
* **File Implementasi Utama**:
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/HrServiceManagement/Services/IEmployeeDocumentIssuanceService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/HrServiceManagement/Services/EmployeeDocumentIssuanceService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/HrServiceManagement/DTOs/EmployeeDocumentIssuanceDtos.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/HrServiceManagement/Controllers/EmployeeDocumentIssuanceController.cs`

---

## 2. Latar Belakang & Kebutuhan Regulasi Perbankan / KPR
Dalam pengajuan permohonan kredit perbankan, Kredit Pemilikan Rumah (KPR), maupun visa perjalanan luar negeri, pegawai rumah sakit (dokter spesialis, perawat, apoteker, staf administrasi) membutuhkan **Surat Keterangan Penghasilan (Salary Certificate)** resmi berkop Rumah Sakit Quilvian.

Pada praktik lama atau sistem tanpa kontrol ketat, sering kali staf pemohon atau analis HR mengetikkan angka nominal gaji bruto/netto secara bebas pada formulir permohonan. Hal ini memicu risiko fatal:
1. **Manipulasi Angka Finansial (*Fraud / Falsification*)**: Potensi penggelembungan angka nominal gaji agar pengajuan KPR disetujui pihak bank, yang melanggar hukum perbankan dan mencoreng integritas Rumah Sakit Quilvian.
2. **Kompensasi Medis Tidak Sinkron**: Ketidaksesuaian antara slip gaji resmi yang diproses oleh Sub-10 Payroll dengan surat keterangan yang dikeluarkan oleh HRD.

Untuk menegakkan prinsip tata kelola rumah sakit yang berintegritas tinggi, task `BE-HRD-200` menerapkan aturan **`VR-HRS-06 Anti-Manual Guard`**:
- **Blokir Keras Input Manual**: Payload pengajuan permohonan surat dinas (`SubmitDocumentRequestDto`) DILARANG KERAS memuat field nominal gaji manual.
- **Tarik Otomatis dari Payroll Final (`INT-HR-56`)**: Nilai `SalaryBrutoAmount` dan `SalaryNettoAmount` wajib ditarik langsung dari slip gaji terakhir yang berstatus `Published` atau `Finalized` pada tabel `TrxPayrollPayslip` (Sub-10 Payroll).
- **Validasi Ketersediaan Data**: Jika staf belum memiliki slip gaji final yang diterbitkan oleh bagian Payroll, pengajuan ditolak seketika oleh sistem dengan pesan regulasi yang transparan.

---

## 3. Skenario Riil Rumah Sakit
* **Skenario 1 (Sukses)**: dr. Hendra, Sp.A (Dokter Spesialis Anak) mengajukan Surat Keterangan Penghasilan untuk lampiran KPR Bank BNI melalui menu ESS.
  - dr. Hendra hanya memilih jenis dokumen *"Surat Keterangan Penghasilan / KPR"* dan mengisi tujuan *"Pengajuan KPR Griya BNI"*.
  - Sistem mendeteksi jenis dokumen berkategori finansial sensitif (`IsFinancialSensitive = true`).
  - Sistem mengeksekusi query ke `TrxPayrollPayslips` yang berstatus `Published` milik dr. Hendra:
    - Diperoleh slip gaji periode Oktober 2026: Gaji Bruto = `Rp 38.500.000,00`, Gaji Netto = `Rp 34.200.000,00`, Slip ID = `payslip-guid`.
  - Sistem mengunci data tersebut ke tabel `TrxEmployeeDocumentIssuance` bersama ID slip gaji dan meneruskan status ke `PendingCbApproval`.
* **Skenario 2 (Ditolak Sistem)**: Budi (Staf Pramusaji Gizi yang baru bekerja 10 hari dan belum pernah melewati siklus penggajian bulanan resmi).
  - Budi mencoba mengajukan surat penghasilan.
  - Sistem tidak menemukan riwayat slip gaji berstatus `Published` / `Finalized` pada Sub-10.
  - Sistem melempar `InvalidOperationException`: *"Data slip gaji resmi Sub-10 Payroll belum tersedia untuk karyawan ini. Sesuai regulasi rumah sakit (VR-HRS-06), penerbitan Surat Keterangan Penghasilan/KPR dilarang menggunakan input manual dan hanya dapat diproses setelah slip gaji berstatus Finalized/Published diterbitkan."*

---

## 4. Spesifikasi Teknis & Endpoint API Bergaya Swagger

### Tag Grup Swagger: `[Tags("HR Services - Document Issuance & Verification")]`

| Method | Path Endpoint | Deskripsi Singkat | Otorisasi / Peran | Status Respons |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/corporate/hrd/services/documents/request` | Mengajukan permohonan surat naskah dinas resmi dengan penarikan otomatis slip gaji Sub-10 | `Authorize` (Staf Medis / HR) | `201 Created`, `400 Bad Request` |
| `GET` | `/api/v1/corporate/hrd/services/documents/{id}` | Mengambil detail permohonan penerbitan surat dinas beserta referensi slip gaji Sub-10 | `Authorize` | `200 OK`, `404 Not Found` |
| `GET` | `/api/v1/corporate/hrd/services/documents/queue` | Mengambil antrean permohonan naskah dinas dengan filter dokumen finansial | `Authorize` | `200 OK` |

### DTO Payload Pengajuan Permohonan (`SubmitDocumentRequestDto`):
```json
{
  "employeeDocumentTypeId": "8f3b2a1c-9d0e-4f5a-6b7c-8d9e0f1a2b3c",
  "requestedForWorkforceProfileId": null,
  "purpose": "Kelengkapan berkas verifikasi pengajuan KPR Bank Mandiri KC RS Quilvian",
  "languageCode": "id-ID",
  "numberOfCopies": 1,
  "neededByDate": "2026-11-15T00:00:00Z",
  "deliveryMethod": "Digital",
  "additionalNotes": "Mohon disertakan rincian slip gaji bulan terakhir yang sudah final."
}
```
*(Perhatikan: Tidak ada satupun field angka gaji manual pada payload di atas! Angka ditarik 100% oleh sistem dari Sub-10 Payroll)*.

---

## 5. Bukti Verifikasi & Keamanan
1. **Traceability Payroll ID**: Setiap penerbitan naskah dinas finansial menyimpan `PayrollPayslipId` yang merujuk langsung ke baris transaksi slip gaji di `TrxPayrollPayslip`.
2. **Kepatuhan Audit Keuangan RS**: Dokumen yang dihasilkan tidak dapat dipalsukan secara administratif karena terkoneksi dengan ledger penggajian rumah sakit.
3. **Kompilasi Sukses**: Lulus kompilasi .NET dengan `0 Error(s)`.
