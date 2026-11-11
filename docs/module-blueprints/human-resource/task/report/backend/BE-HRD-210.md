# Laporan Penyelesaian Task: BE-HRD-210 REST API Controller Lisensi Profesional

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-210`
* **Nama Task**: Pembangunan REST API Controller Lisensi Profesional Nakes & Dasbor Radar Pemantauan PSV Kedaluwarsa
* **Modul / Sub-Modul**: Human Resource / Sub-modul 14: Credentialing, License & Clinical Privilege
* **Slice Delivery**: `S-C1` (Credentialing, License & Clinical Privilege)
* **Status**: `✅ Selesai (2026-11-09)`
* **File Implementasi Utama**:
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/CredentialingManagement/Controllers/ProfessionalLicenseController.cs`
  - `NewQuilvianSystemBackend/Program.cs` (DI `ICredentialVerificationService` & `CredentialVerificationService`)

---

## 2. Latar Belakang & Wewenang REST API
Untuk memfasilitasi interaksi antarmuka frontend (Next.js App Router pada sub-modul kredensialing) dan integrasi sistem eksternal, controller ini dibangun dengan kepatuhan penuh terhadap standar API Quilvian v18:
1. **Tag Grup Swagger Terstandar**: Menggunakan anotasi resmi `[Tags("HR Credentialing - Professional Licenses & PSV Radar")]`.
2. **Keamanan & Otorisasi**: Dilengkapi atribut `[Authorize]` dan pembacaan identitas auditor/pengguna dari claims JWT token (`NameIdentifier` / `sub`).
3. **Endpoint Komprehensif**: Mendukung seluruh alur registrasi STR/SIP, eksekusi audit Verifikasi Sumber Primer (PSV) Kemenkes/KKI, pencarian lisensi per nakes, perpanjangan SIP 5 tahun, query radar pemantauan H-90 s.d. H-0, dan pemicu darurat manual fail-closed.

---

## 3. Spesifikasi Teknis & Endpoint API Bergaya Swagger

### Tag Grup Swagger: `[Tags("HR Credentialing - Professional Licenses & PSV Radar")]`
**Base Route**: `/api/v1/corporate/hrd/credentialing/licenses`

| HTTP Method | Path Endpoint | Deskripsi Bisnis & Fungsional | Otorisasi & Role | Request Payload | Response Model |
| :---: | :--- | :--- | :---: | :--- | :--- |
| `POST` | `/api/v1/corporate/hrd/credentialing/licenses` | Mendaftarkan lisensi baru nakes (STR Seumur Hidup atau SIP 5 Tahun Faskes 1-3). | `[Authorize]` Analis HR / Komite Kredensial | `RegisterProfessionalLicenseDto` (JSON) | `201 Created` (`ProfessionalLicenseDto`) |
| `GET` | `/api/v1/corporate/hrd/credentialing/licenses/{id}` | Mengambil detail lengkap lisensi profesional beserta status radar kedaluwarsa dan riwayat audit PSV. | `[Authorize]` Nakes / Analis HR | - | `200 OK` (`ProfessionalLicenseDto`) |
| `GET` | `/api/v1/corporate/hrd/credentialing/licenses/employee/{employeeId}` | Mengambil seluruh portofolio lisensi (STR dan SIP faskes 1 s.d. 3) milik seorang tenaga kerja. | `[Authorize]` Nakes / HR Manager | - | `200 OK` (`List<ProfessionalLicenseDto>`) |
| `POST` | `/api/v1/corporate/hrd/credentialing/licenses/{id}/verify-psv` | Melakukan audit Verifikasi Sumber Primer (PSV) ke portal SatuSehat SDMK Kemenkes / KKI. | `[Authorize]` Auditor Kredensial RS | `PsvVerificationRequestDto` (JSON) | `200 OK` (`PsvVerificationResultDto`) |
| `POST` | `/api/v1/corporate/hrd/credentialing/licenses/{id}/renew` | Mengajukan perpanjangan SIP 5 tahun baru dan me-reset status verifikasi PSV ke `PendingVerification`. | `[Authorize]` Nakes / Analis HR | `RenewProfessionalLicenseDto` (JSON) | `200 OK` (`ProfessionalLicenseDto`) |
| `GET` | `/api/v1/corporate/hrd/credentialing/licenses/radar/expiring` | Mengambil data radar pemantauan lisensi yang mendekati kedaluwarsa dengan filter opsional tier radar (`H90`, `H60`, `H30`, `H7`, `ExpiredH0`). | `[Authorize]` Direksi / HR / Komite Kredensial | Query parameter `?radarLevel=H30` | `200 OK` (`List<LicenseRadarAlertDto>`) |
| `POST` | `/api/v1/corporate/hrd/credentialing/licenses/radar/execute-fail-closed` | Pemicu eksekusi manual Fail-Closed untuk menangguhkan kewenangan klinis nakes yang telah kedaluwarsa. | `[Authorize]` Direktur Pelayanan Medik / HR Admin | - | `200 OK` (Ringkasan jumlah lisensi diproses) |

---

## 4. Contoh Konkret Skenario Penggunaan API

### Contoh Permintaan Registrasi SIP 5 Tahun Faskes Utama (SIP-1)
* **Endpoint**: `POST /api/v1/corporate/hrd/credentialing/licenses`
* **Request Body**:
```json
{
  "workforceProfileId": "b1a2c3d4-e5f6-7890-abcd-ef1234567890",
  "licenseType": "SIP",
  "licenseNumber": "SIP-503/442/DS/XI/2026",
  "issuer": "Dinas Kesehatan Kota Medan / DPMPTSP",
  "practiceLocation": "Rumah Sakit Quilvian Utama",
  "issueDate": "2026-11-09T00:00:00Z",
  "expiredDate": "2031-11-09T00:00:00Z",
  "isLifetimeStr": false,
  "sipFaskesPracticeNumber": "SIP-1",
  "sipFaskesName": "Rumah Sakit Quilvian Utama",
  "description": "Surat Izin Praktik Dokter Spesialis Anak Tempat Praktik Pertama"
}
```
* **Response Body (`201 Created`)**:
```json
{
  "id": "e4f5a6b7-c8d9-0123-ef45-678901234567",
  "workforceProfileId": "b1a2c3d4-e5f6-7890-abcd-ef1234567890",
  "employeeName": "dr. Maya Sarah, Sp.A",
  "employeeNumber": "DOC-2026-0044",
  "professionName": "Dokter Spesialis Anak",
  "licenseType": "SIP",
  "licenseNumber": "SIP-503/442/DS/XI/2026",
  "issuer": "Dinas Kesehatan Kota Medan / DPMPTSP",
  "practiceLocation": "Rumah Sakit Quilvian Utama",
  "issueDate": "2026-11-09T00:00:00Z",
  "expiredDate": "2031-11-09T00:00:00Z",
  "isLifetimeStr": false,
  "isPsvVerified": false,
  "psvVerifiedAt": null,
  "psvVerificationSource": null,
  "psvVerificationUrl": null,
  "sipFaskesPracticeNumber": "SIP-1",
  "sipFaskesName": "Rumah Sakit Quilvian Utama",
  "expiryRadarStatus": "Normal",
  "daysRemaining": 1826,
  "blocksClinicalServiceWhenInvalid": true,
  "blocksSchedulingWhenInvalid": true,
  "isRevoked": false,
  "revocationReason": null,
  "isActive": true
}
```

---

## 5. Verifikasi Kompilasi & Kesiapan Produksi
* **Status Kompilasi C#**: Lolos kompilasi dengan **`0 Error(s)`**.
* **Integrasi Dependency Injection**: Service `ICredentialVerificationService` dan Background Worker `LicenseExpiryMonitoringWorker` telah terdaftar dan siap melayani request API maupun penjadwalan pemantauan otomatis.
