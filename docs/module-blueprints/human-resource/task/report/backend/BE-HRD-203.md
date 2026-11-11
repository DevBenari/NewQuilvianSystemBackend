# Laporan Penyelesaian Task: BE-HRD-203 Generator QR Code SHA-256 & Enkripsi 4-Digit PIN Finansial

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-203`
* **Nama Task**: Generator QR Code SHA-256 & Enkripsi 4-Digit PIN Finansial (`HRS-B5`, `VR-HRS-09`, Masking Finansial Publik)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 13: HR Service Management & Request
* **Slice Delivery**: `S-D4` (HR Service Management)
* **Status**: `✅ Selesai (2026-11-09)`
* **File Implementasi Utama**:
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/HrServiceManagement/Services/IEmployeeDocumentIssuanceService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/HrServiceManagement/Services/EmployeeDocumentIssuanceService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/HrServiceManagement/DTOs/EmployeeDocumentIssuanceDtos.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/HrServiceManagement/Controllers/EmployeeDocumentIssuanceController.cs`

---

## 2. Latar Belakang & Kebutuhan Keamanan Privasi Data Finansial
Pihak eksternal seperti perbankan (analis kredit KPR), kedutaan besar, dan lembaga pembiayaan membutuhkan cara cepat dan andal untuk memverifikasi keaslian surat dinas yang diterbitkan oleh Rumah Sakit Quilvian secara mandiri tanpa harus menelepon bagian HRD rumah sakit.

Namun demikian, terdapat tantangan besar terkait **Privasi & Kerahasiaan Data Finansial Staf (UU Perlindungan Data Pribadi / PDP)**:
- Jika QR Code dicetak pada dokumen dan dapat dipindai oleh siapa saja (misal kurir pengantar berkas atau staf administrasi umum), rincian nominal penghasilan dokter atau perawat berisiko terekspos ke publik.
- Hal ini melanggar privasi ketenagakerjaan dan berpotensi menimbulkan kecemburuan sosial atau risiko keamanan finansial bagi tenaga medis.

Untuk menyelesaikan dilema antara **kemudahan verifikasi keabsahan** dan **kerahasiaan gaji**, task `BE-HRD-203` menerapkan arsitektur proteksi ganda:
1. **Generator Hash Kriptografis SHA-256 & QR Code**:
   - Setiap dokumen yang diterbitkan memperoleh `VerificationHash` unik 64-karakter heksadesimal yang diturunkan dari kombinasi nomor surat, ID pegawai, timestamp ticks, dan salt rahasia rumah sakit.
   - Sistem menghasilkan gambar QR Code resolusi tinggi menggunakan library `QRCoder` yang mengarahkan pemindai ke URL verifikasi resmi: `https://simrs.quilvian.co.id/public-verify/hr-document?hash={VerificationHash}`.
2. **Masking Nominal Finansial Publik (`HRS-B5`)**:
   - Saat siapapun memindai QR Code atau mengakses link verifikasi publik, sistem menampilkan status keabsahan dokumen (Nomor Surat, Nama Rumah Sakit, Nama Pegawai Tersamar, Tanggal Terbit, Nama Penandatangan).
   - Namun untuk dokumen finansial, angka nominal gaji secara default **DI-MASKING**: `Rp **.***.***`.
3. **Enkripsi PIN 4-Digit Keamanan Finansial (`VR-HRS-09`)**:
   - Sistem membuat 4-digit PIN acak (atau PIN pilihan pemohon) saat dokumen diterbitkan. PIN mentah hanya ditampilkan SATU KALI kepada pemohon untuk diserahkan secara privat kepada analis kredit bank.
   - Di database, PIN disimpan dalam bentuk terenkripsi satu arah dengan penggaraman dinamis (*Salted SHA-256 / PBKDF2 hash*).
   - Analis bank dapat membuka rincian nominal asli melalui endpoint `POST unlock-financial` hanya jika memasukkan 4-digit PIN yang benar.

---

## 3. Skenario Riil Rumah Sakit
* **Skenario**: dr. Budi Santoso, Sp.B (Dokter Spesialis Bedah) mengajukan Surat Keterangan Penghasilan untuk KPR di Bank Mandiri.
* **Alur Eksekusi**:
  1. Dokumen diterbitkan dengan QR Code dan PIN Keamanan: `7429`. dr. Budi mencatat PIN ini.
  2. dr. Budi menyerahkan cetakan dokumen kepada analis kredit Bank Mandiri (Bpk. Fajar) beserta PIN `7429`.
  3. Saat dokumen masih berada di kurir pengiriman, kurir mencoba memindai QR Code dengan smartphone miliknya:
     - Portal publik menampilkan:
       - *"Dokumen Sah & Terverifikasi Diterbitkan Resmi oleh Rumah Sakit Quilvian."*
       - Nama: `d***o`
       - Gaji Bruto: `Rp **.***.***` (Tersamar)
       - Gaji Netto: `Rp **.***.***` (Tersamar)
     - Kurir tidak dapat melihat nominal gaji dokter. Privasi data gaji dr. Budi terlindungi 100%.
  4. Analis kredit Bank Mandiri (Bpk. Fajar) menerima berkas, membuka link verifikasi, dan memasukkan PIN `7429`:
     - Sistem memverifikasi kecocokan hash PIN menggunakan `CryptographicOperations.FixedTimeEquals` (aman dari serangan *timing attack*).
     - Sistem membuka data asli: Gaji Bruto = `Rp 42.500.000,00`, Gaji Netto = `Rp 37.800.000,00` untuk periode penggajian Oktober 2026.
     - Analis kredit bank yakin 100% surat tersebut asli dan valid dari RS Quilvian.

---

## 4. Spesifikasi Teknis & Endpoint API Bergaya Swagger

### Tag Grup Swagger: `[Tags("HR Services - Document Issuance & Verification")]`

| Method | Path Endpoint | Deskripsi Singkat | Otorisasi / Peran | Status Respons |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/corporate/hrd/services/documents/public-verify/{hash}` | Endpoint Publik: Verifikasi keabsahan dokumen via QR Code (Nominal Finansial Masked) | `[AllowAnonymous]` (Publik) | `200 OK` |
| `POST` | `/api/v1/corporate/hrd/services/documents/unlock-financial` | Endpoint Publik: Membuka rincian nominal gaji asli menggunakan 4-digit PIN terverifikasi | `[AllowAnonymous]` (Publik / Bank) | `200 OK`, `400 Bad Request` |

### Contoh Respons Verifikasi Publik (Masked):
```json
{
  "isValid": true,
  "verificationStatusMessage": "Dokumen Sah & Terverifikasi Diterbitkan Resmi oleh Rumah Sakit Quilvian.",
  "issuanceNumber": "ISS-DOC-20261109-7A8B9C",
  "officialDocumentNumber": "SKP/RSQ-SDM/XI/2026/000042",
  "documentTypeName": "Surat Keterangan Penghasilan / KPR",
  "employeeNameMasked": "d***o",
  "employeeNumberMasked": "1***2",
  "issuedByHospital": "Rumah Sakit Quilvian Utama",
  "issuedAt": "2026-11-09T04:30:00Z",
  "validUntil": "2027-02-07T04:30:00Z",
  "signerName": "Drs. Hendro Wibowo, M.M.",
  "signerTitle": "Manajer Sumber Daya Manusia",
  "isFinancialSensitive": true,
  "isFinancialDataMasked": true,
  "maskedSalaryBruto": "Rp **.***.***",
  "maskedSalaryNetto": "Rp **.***.***",
  "requiresPinToUnlock": true
}
```

### Request Membuka Kunci Finansial (`UnlockFinancialDetailsRequestDto`):
```json
{
  "verificationHash": "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
  "securityPin": "7429"
}
```

### Respons Sukses Membuka Kunci Finansial (`FinancialDetailsUnlockedDto`):
```json
{
  "isSuccess": true,
  "message": "PIN 4-Digit Terverifikasi. Rincian data finansial resmi berhasil dibuka.",
  "officialDocumentNumber": "SKP/RSQ-SDM/XI/2026/000042",
  "employeeFullName": "dr. Budi Santoso, Sp.B",
  "employeeNumber": "19820315-201201-1-018",
  "salaryBrutoAmount": 42500000.00,
  "salaryNettoAmount": 37800000.00,
  "formattedSalaryBruto": "Rp 42.500.000,00",
  "formattedSalaryNetto": "Rp 37.800.000,00",
  "currency": "IDR",
  "payslipPeriodSnapshot": "01 Okt 2026 s.d. 31 Okt 2026",
  "verifiedAtUtc": "2026-11-09T04:35:00Z"
}
```

---

## 5. Bukti Verifikasi & Keamanan
1. **Perlindungan Terhadap Timing Attack**: Pembandingan PIN menggunakan `CryptographicOperations.FixedTimeEquals`, mencegah peretas menebak PIN dengan menganalisis durasi komputasi server.
2. **Kerahasiaan Hash Salted**: Hash PIN disimpan bersama salt unik (`{salt}:{hash}`) sehingga tidak rentan terhadap serangan kamus (*rainbow tables*).
3. **Penyimpanan Gambar QR Code Terisolasi**: Berkas PNG QR code disimpan di direktori `Storage/documents/qrcodes/` yang aman.
4. **Kompilasi Sukses**: Lulus kompilasi .NET dengan `0 Error(s)`.
