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


---

## Pengerjaan Ulang 10 November 2026 — Remediasi Gelombang 1 Audit Backend HR

| Field | Nilai |
| --- | --- |
| Dasar | [backend-unfinished-work.md](../../../roadmap/backend-unfinished-work.md) bagian 10 Gelombang 1, kelompok C — data penting hanya di memori |
| Task mode | `BACKEND` |
| Model | Claude Fable 5.1 |
| Commit backend saat dikerjakan | mulai `edfd843bac1c2860da7ae9a5ddcb68a75cf42666`; pemilik melakukan commit dan merge selama sesi (HEAD akhir `05325918`, branch `MHamzah`) |
| Status sesudah pengerjaan ulang | 🟡 **SEBAGIAN 10 November 2026** (dikerjakan ulang; klaim ✅ 9 November 2026 diturunkan) |

### Masalah yang ditemukan audit

Penghitung salah-PIN pembukaan nominal surat finansial (maks 3 kali per 30 menit per dokumen per IP) disimpan di memori proses dan hilang saat restart. PIN 4 digit dibuat `Random.Shared`, bukan pembangkit acak kriptografis.

### Perubahan yang dikerjakan

- Tabel baru `HrdPinAttemptRecord`; `IPinRateLimiterService` menjadi asinkron (`CheckRateLimitAsync`, `RecordFailedAttemptAsync`, `ResetAttemptsAsync`) dan registrasinya diubah dari Singleton ke Scoped karena kini memakai `ApplicationDbContext`.
- PIN 4 digit dibuat `RandomNumberGenerator.GetInt32(1000, 10000)`.

### Berkas yang berubah

- `Areas/Corporate/HumanResource/HrServiceManagement/Services/PinRateLimiterService.cs`
- `Areas/Corporate/HumanResource/HrServiceManagement/Services/EmployeeDocumentIssuanceService.cs`
- `Areas/Corporate/HumanResource/HrServiceManagement/Models/HrdPinAttemptRecord.cs` (baru)
- `Repositories/Configurations/Corporate/HumanResource/HrServiceManagement/HrdPinAttemptRecordConfiguration.cs` (baru)
- `Program.cs`
- `Repositories/ApplicationDbContext.cs`
- `Migrations/20261110050000_AddHrdDurableAuditAndPinLockouts.cs`

### Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` **0 error**, 606 warning (garis dasar sebelum sesi 578; selisih berasal dari merge cabang lain yang masuk HEAD pada sesi yang sama, bukan dari berkas task ini) | 0 error | `PASS` |
| `Invoke-QbeConformanceCheck.ps1 -Mode Strict` atas 58 berkas yang disentuh: **0 VIOLATION, 0 REVIEW, PASS** | 0 temuan | `PASS` |
| Review diff dan scope | hanya berkas di atas yang disentuh untuk task ini | `PASS` |
| Uji runtime / API | tidak dijalankan pada sesi ini | `NOT RUN` |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

### Acceptance criteria yang disentuh pengerjaan ulang

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Rate limiting 3x per 30 menit bertahan lintas restart dan instance | Terpenuhi di source | tabel `HrdPinAttemptRecord` dengan index unik `AttemptKey` |
| PIN acak kriptografis | Terpenuhi | `RandomNumberGenerator` |

### Yang belum terpenuhi

- Migration belum dijalankan; `AT-HRD-HRS-19` `NOT RUN`.

### Database dan migration

Migration SQL idempoten `Migrations/20261110050000_AddHrdDurableAuditAndPinLockouts.cs` disiapkan; **belum dijalankan** ke database mana pun (wewenang pemilik, `dotnet ef database update`). Snapshot model EF tidak dipakai sebagai dasar migration ini karena sudah lama tidak sinkron dengan migration SQL manual slice sebelumnya (lihat catatan risiko).

### Catatan penutup

- Perubahan sampingan: migration hasil scaffold otomatis `20261110044224_AddHrdDurableAuditAndPinLockouts` (14.620 baris, 79 CreateTable/26 DropTable di luar lingkup) **dibuang** dan diganti migration SQL manual; snapshot model yang ikut ter-commit pemilik dibiarkan karena sudah bercampur dengan merge cabang lain.
- Status Git akhir sesi: berkas task sudah masuk HEAD lewat commit pemilik; sisa di working tree adalah penghapusan dua berkas scaffold dan satu berkas migration manual baru (lihat `git status --short` pada ringkasan sesi).


## Pembaruan 11 November 2026 — Status Migration

Migration task ini sudah dijalankan ke database pengembangan `QuilvianNewDevHamzah` (`dotnet ef database update`, 0 migration tertunda). Pernyataan "migration belum dijalankan" pada bagian di atas sudah tidak berlaku. Uji penerimaan dengan akun sungguhan tetap `NOT RUN`, jadi statusnya tetap 🟡 bila bagian di atas menyatakan demikian. Rincian ada di `roadmap/backend-unfinished-work.md` bagian 14.
