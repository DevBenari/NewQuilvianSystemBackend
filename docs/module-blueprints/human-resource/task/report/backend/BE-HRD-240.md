# Laporan Penyelesaian Task: BE-HRD-240 Engine Radar Kedaluwarsa Bertingkat (H-90/60/30) & Penyiaran Notifikasi Massal

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-240`
* **Nama Task**: Engine Radar Kedaluwarsa Bertingkat (H-90/60/30) & Penyiaran Notifikasi Massal (`TrainingExpiryRadarService`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 16: Learning, Training & Competency Management
* **Slice Delivery**: `S-C2` (Kontrak API `v20`)
* **Status**: `✅ Selesai (2026-11-09)`
* **File Implementasi Utama**:
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Services/ITrainingExpiryRadarService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Services/TrainingExpiryRadarService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Workers/MandatoryTrainingExpiryRadarWorker.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Controllers/MandatoryHospitalTrainingController.cs`
  - `NewQuilvianSystemBackend/Program.cs`

---

## 2. Latar Belakang & Algoritma Radar Kedaluwarsa Multi-Tier
Sertifikat pelatihan wajib (BHD, PPI, SKP, K3RS, Komunikasi SBAR) yang kedaluwarsa merupakan salah satu temuan fatal dalam survei akreditasi rumah sakit (STARKES KARS/JCI) dan berisiko tinggi terhadap keselamatan pasien serta keselamatan kerja tenaga medis. Untuk mencegah kelalaian resertifikasi, dibangun sistem engine radar otomatis yang memantau tanggal kedaluwarsa secara harian dan bertingkat (`VR-TRN-09`, `AT-HRD-TRN-03`, `AT-HRD-TRN-04`).

### 1. Klasifikasi 4 Zona Radar Kedaluwarsa (`VR-TRN-09`)
Engine radar mengelompokkan sisa hari masa berlaku sertifikat ($\Delta t = \text{ExpiryDate} - \text{CurrentDate}$) ke dalam 4 tier eskalasi:

```text
┌─────────────────────────────────────────────────────────────────────────────┐
│                       MATRIKS ZONA RADAR KEDALUWARSA                       │
├──────────────┬──────────────────┬─────────────────┬─────────────────────────┤
│ Zona Radar   │ Rentang Sisa Hari│ Tingkat Bahaya  │ Tindakan & Kanal Alert  │
├──────────────┼──────────────────┼─────────────────┼─────────────────────────┤
│ Zona H-90    │ 61 - 90 Hari     │ Info / Awal     │ Notifikasi ESS Dashboard│
│ Zona H-60    │ 31 - 60 Hari     │ Warning Sedang  │ Email Pegawai & Karu    │
│ Zona H-30    │ 1 - 30 Hari      │ Peringatan Kritis WhatsApp, Email & ESS   │
│ Zona H-0     │ <= 0 Hari        │ Overdue (Fatal) │ Status Expired + Event  │
└──────────────┴──────────────────┴─────────────────┴─────────────────────────┘
```

1. **Zona H-90 (Info Awal)**: Sisa hari 61-90 hari. Muncul sebagai pengingat awal pada modul Employee Self-Service (ESS).
2. **Zona H-60 (Peringatan Sedang)**: Sisa hari 31-60 hari. Notifikasi email terkirim otomatis ke pegawai dan Kepala Ruangan untuk merencanakan alokasi cuti/dinas pelatihan.
3. **Zona H-30 (Peringatan Kritis)**: Sisa hari 1-30 hari. Alert multi-kanal prioritas tinggi terkirim (WhatsApp Gateway, Email, dan ESS Banner) serta pendaftaran otomatis ke antrean batch kelas penyegar berikutnya.
4. **Zona H-0 (Overdue / Kedaluwarsa)**: Sisa hari $\le 0$ hari. Sertifikat berstatus `Expired` / `Overdue`, memicu audit log `MandatoryTrainingExpiryRadarTriggered`, dan memicu sinyal intervensi keselamatan kerja.

### 2. Background Worker Harian (`TRN-W1`)
Layanan `MandatoryTrainingExpiryRadarWorker` didaftarkan sebagai `IHostedService` di `Program.cs`. Worker ini berjalan secara otonom di latar belakang setiap 24 jam sekali (pukul 02:00 dini hari), melakukan audit pemindaian seluruh sertifikat aktif di database tanpa membebani performa operasional jam kerja rumah sakit.

---

## 3. Spesifikasi Endpoint API Bergaya Swagger

### Tag Grup Swagger: `[Tags("Corporate - HRD Mandatory Hospital Training & Certification")]`
**Base Route**: `/api/v1/corporate/hrd/learning/certificates`

| Method | Path Endpoint | Deskripsi Bisnis & Fungsional | Otorisasi & Role | Request Payload | Response Model |
| :---: | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/corporate/hrd/learning/certificates/expiring-radar` | Mengambil data radar seluruh sertifikat pegawai yang akan kedaluwarsa dalam zona H-90, H-60, H-30, dan Overdue. | `[Authorize]` Staff / Diklat | Filter: `radarZone`, `departmentId` | `200 OK` (`ApiResponse<TrainingExpiryRadarSummaryDto>`) |
| `POST` | `/api/v1/corporate/hrd/learning/certificates/expiring-radar/broadcast-alerts` | Mengirimkan siaran peringatan massal multi-kanal (WhatsApp, Email, ESS) untuk staf di zona radar tertentu. | `[Authorize]` Diklat / SuperAdmin | `BroadcastRadarAlertRequestDto` | `200 OK` (`ApiResponse<BroadcastRadarAlertResultDto>`) |

---

## 4. Contoh Konkret Respons Radar & Siaran Peringatan

### Skenario 1: Ringkasan Radar Kedaluwarsa Rumah Sakit
* **Endpoint**: `GET /api/v1/corporate/hrd/learning/certificates/expiring-radar`
* **Response Body (`200 OK`)**:
```json
{
  "code": 200,
  "status": "OK",
  "message": "Data radar sertifikat kedaluwarsa berhasil dihimpun.",
  "data": {
    "totalMonitoredCertificates": 1250,
    "countH90Zone": 45,
    "countH60Zone": 28,
    "countH30Zone": 12,
    "countOverdueZone": 4,
    "evaluatedAt": "2026-11-09T02:00:00Z",
    "criticalAlerts": [
      {
        "certificateId": "c1c2c3c4-1111-2222-3333-444455556666",
        "staffId": "s1s2s3s4-5555-6666-7777-888899990000",
        "staffName": "Ns. Ratna Wulandari, S.Kep",
        "departmentName": "Instalasi Gawat Darurat (IGD)",
        "trainingName": "Bantuan Hidup Dasar (BHD / BLS)",
        "expiryDate": "2026-11-25T00:00:00Z",
        "daysRemaining": 16,
        "radarZone": "H30_Critical",
        "recommendedAction": "Wajib didaftarkan pada Batch BHD-2026-B4 tanggal 18 November 2026."
      }
    ]
  }
}
```

### Skenario 2: Eksekusi Siaran Peringatan Massal Multi-Kanal
* **Endpoint**: `POST /api/v1/corporate/hrd/learning/certificates/expiring-radar/broadcast-alerts`
* **Request Body**:
```json
{
  "targetZone": "H30_Critical",
  "channels": ["WhatsApp", "Email", "ESS"]
}
```
* **Response Body (`200 OK`)**:
```json
{
  "code": 200,
  "status": "OK",
  "message": "Siaran peringatan kedaluwarsa berhasil dikirimkan.",
  "data": {
    "targetZone": "H30_Critical",
    "totalRecipients": 12,
    "whatsappDelivered": 12,
    "emailDelivered": 12,
    "essNotificationsPushed": 12,
    "failedCount": 0,
    "broadcastedAt": "2026-11-09T08:30:00Z"
  }
}
```

---

## 5. Verifikasi Acceptance Criteria & Integritas Bisnis
1. **Ketepatan Kategorisasi Zona Radar (`VR-TRN-09`, `AT-HRD-TRN-03`)**: ✅ Lulus. Perhitungan sisa hari secara presisi membedakan zona H-90, H-60, H-30, dan Overdue tanpa selisih hari.
2. **Kesiapan Background Worker `TRN-W1`**: ✅ Lulus. Worker terdaftar di `Program.cs` dan beroperasi dengan penanganan error tangguh (*fault-tolerant*).
3. **Penyebaran Alert Multi-Kanal (`AT-HRD-TRN-04`)**: ✅ Lulus. Pemanggilan siaran peringatan mencakup WhatsApp, Email, dan notifikasi banner ESS.
