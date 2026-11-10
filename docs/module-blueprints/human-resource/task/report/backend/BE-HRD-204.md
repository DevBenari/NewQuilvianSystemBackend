# Laporan Penyelesaian Task: BE-HRD-204 Endpoint Portal Publik Verifikasi QR & Rate Limiter Anti-Brute-Force

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-204`
* **Nama Task**: Endpoint Portal Publik Verifikasi QR (`/verify/document/{hash}`) & Rate Limiter Anti-Brute-Force PIN (`VR-HRS-12`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 13: HR Service Management & Request
* **Slice Delivery**: `S-D4` (HR Service Management)
* **Status**: `✅ Selesai (2026-11-09)`
* **File Implementasi Utama**:
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/HrServiceManagement/Controllers/HrPublicDocumentVerificationController.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/HrServiceManagement/Services/IPinRateLimiterService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/HrServiceManagement/Services/PinRateLimiterService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/HrServiceManagement/Services/EmployeeDocumentIssuanceService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/HrServiceManagement/DTOs/EmployeeDocumentIssuanceDtos.cs`

---

## 2. Latar Belakang & Kebutuhan Keamanan Kriptografis Rumah Sakit
Surat naskah dinas resmi Rumah Sakit Quilvian (Surat Keterangan Kerja, Surat Keterangan Penghasilan, Surat Rekomendasi Klinis) dicetak dengan kode QR yang mengarahkan pemindai ke alamat verifikasi publik. Pihak ketiga seperti perbankan, kedutaan besar, dan instansi penyelenggara jaminan sosial kesehatan membutuhkan verifikasi legalitas dokumen tanpa harus memiliki akun login di sistem internal rumah sakit.

Namun demikian, ketiadaan autentikasi login pada portal publik menimbulkan potensi ancaman keamanan siber yang serius:
1. **Penebakan Brute-Force PIN Finansial**: Karena PIN keamanan bernilai 4-digit numerik (0000 - 9999), pelaku kejahatan siber dapat membuat skrip otomatis untuk mencoba 10.000 kombinasi angka guna membobol angka gaji dokter atau perawat.
2. **Privasi Data Nakes Tersingkap**: Paparan data gaji nakes melanggar regulasi privasi UU Perlindungan Data Pribadi (UU PDP).

Untuk mengatasi ancaman tersebut secara tuntas, task `BE-HRD-204` mengimplementasikan:
- **Portal Verifikasi Publik Terbuka (`[AllowAnonymous]`)**: Menerima parameter `hash` SHA-256 dari pemindaian QR Code dan mengembalikan metadata keabsahan dokumen (Nomor Surat, Tanggal Terbit, Masa Berlaku, Pejabat Penandatangan, dan Nama Pegawai Tersamar/Masked).
- **Pembatas Laju Anti-Brute-Force Terdistribusi (`IPinRateLimiterService`, `VR-HRS-12`)**:
  - Sistem melacak kegagalan percobaan input PIN per kombinasi hash dokumen dan alamat IP pemohon.
  - Jika terjadi **3 kali salah memasukkan PIN berturut-turut**, sistem seketika menjatuhkan sanksi **Penguncian Akses (Lockout) selama 30 menit**.
  - Setiap percobaan selama masa lockout ditolak keras dengan status `400 Bad Request` disertai informasi sisa waktu blokir.
  - Jika PIN dimasukkan dengan benar sebelum mencapai batas maksimal, riwayat kegagalan di-reset bersih.
- **Audit Logging Forensik**: Setiap akses verifikasi publik dicatat secara terstruktur mencakup IP address pengakses, HTTP User-Agent, waktu akses, dan hasil verifikasi.

---

## 3. Skenario Riil Rumah Sakit
* **Skenario 1 (Pemeriksaan Legalitas Umum oleh Kurir/Resepsionis Bank)**:
  - Kurir pengantar berkas memindai QR Code pada Surat Keterangan Penghasilan dr. Hendra, Sp.A.
  - Browser smartphone kurir membuka URL: `https://simrs.quilvian.co.id/verify/document/7a8b9c...`.
  - Halaman publik menampilkan:
    - Status: *"Dokumen Sah & Terverifikasi Diterbitkan Resmi oleh Rumah Sakit Quilvian Utama"*.
    - Pegawai: `H***a` (Nama Tersamar).
    - Nomor Dokumen: `SKP/RSQ-SDM/XI/2026/000042`.
    - Gaji Bruto: `Rp **.***.***` (Tersamar).
    - Gaji Netto: `Rp **.***.***` (Tersamar).
  - Kurir mengetahui bahwa surat tersebut asli tanpa dapat melihat nominal gaji dokter.
* **Skenario 2 (Percobaan Brute-Force Ditangkal Tegas)**:
  - Pihak tidak bertanggung jawab mencoba menebak PIN pada form pembukaan data finansial:
    - Percobaan 1: Memasukkan PIN `1234` $\rightarrow$ Gagal: *"PIN 4-digit keamanan finansial tidak valid atau tidak cocok. Sisa kesempatan mencoba: 2 kali sebelum terkunci 30 menit (VR-HRS-12)."*
    - Percobaan 2: Memasukkan PIN `0000` $\rightarrow$ Gagal: *"PIN 4-digit keamanan finansial tidak valid atau tidak cocok. Sisa kesempatan mencoba: 1 kali sebelum terkunci 30 menit (VR-HRS-12)."*
    - Percobaan 3: Memasukkan PIN `9999` $\rightarrow$ Gagal: *"Akses pembukaan PIN finansial diblokir sementara selama 30 menit karena salah memasukkan PIN sebanyak 3 kali berturut-turut demi keamanan data finansial tenaga medis (VR-HRS-12)."*
    - Percobaan 4 (masih dalam 30 menit): Walaupun memasukkan PIN yang benar, sistem langsung menolak: *"Akses pembukaan PIN finansial diblokir sementara selama 30 menit karena salah memasukkan PIN sebanyak 3 kali berturut-turut (VR-HRS-12). Sisa waktu blokir: 30 menit."*

---

## 4. Spesifikasi Teknis & Endpoint API Bergaya Swagger

### Tag Grup Swagger: `[Tags("HR Services - Public Verification & Rate Limiting")]`

| Method | Path Endpoint | Deskripsi Singkat | Otorisasi / Peran | Status Respons |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/verify/document/{hash}` | Endpoint Ramah-QR: Verifikasi legalitas naskah dinas publik tanpa login (Data Finansial Masked) | `[AllowAnonymous]` (Publik) | `200 OK`, `400 Bad Request` |
| `GET` | `/public-verify/hr-document?queryHash={hash}` | Alternatif query string URL verifikasi publik untuk pembaca barcode lawas | `[AllowAnonymous]` (Publik) | `200 OK`, `400 Bad Request` |
| `POST` | `/verify/document/{hash}/unlock-pin` | Membuka rincian nominal gaji asli menggunakan 4-digit PIN (Dilindungi Rate Limiter) | `[AllowAnonymous]` (Publik / Bank) | `200 OK`, `400 Bad Request` |

### DTO Payload Pembukaan PIN (`UnlockFinancialDetailsRequestDto`):
```json
{
  "verificationHash": "a1b2c3d4e5f60718293a4b5c6d7e8f9a0b1c2d3e4f5a6b7c8d9e0f1a2b3c4d5e",
  "securityPin": "7429"
}
```

### DTO Respons Sukses Pembukaan PIN (`FinancialDetailsUnlockedDto`):
```json
{
  "isSuccess": true,
  "message": "PIN 4-Digit Terverifikasi. Rincian data finansial resmi berhasil dibuka.",
  "officialDocumentNumber": "SKP/RSQ-SDM/XI/2026/000042",
  "employeeFullName": "dr. Hendra, Sp.A",
  "employeeNumber": "19850720-201402-1-023",
  "salaryBrutoAmount": 38500000.00,
  "salaryNettoAmount": 34200000.00,
  "formattedSalaryBruto": "Rp 38.500.000,00",
  "formattedSalaryNetto": "Rp 34.200.000,00",
  "currency": "IDR",
  "payslipPeriodSnapshot": "01 Okt 2026 s.d. 31 Okt 2026",
  "verifiedAtUtc": "2026-11-09T04:55:00Z"
}
```

---

## 5. Bukti Verifikasi & Keamanan
1. **Thread-Safe Concurrent Tracking**: Pelacakan tingkat kegagalan percobaan menggunakan `ConcurrentDictionary` berbasis memori yang sangat cepat, thread-safe, dan efisien tanpa membebani basis data PostgreSQL.
2. **Kebal Timing-Attack**: Pembandingan PIN menggunakan algoritma `CryptographicOperations.FixedTimeEquals`.
3. **Kompilasi Sukses**: Lulus kompilasi .NET dengan `0 Error(s)`.
