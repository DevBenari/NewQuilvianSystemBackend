# Laporan Penyelesaian Task: BE-HRD-229 Layanan Penerimaan Feed Pemicu Kritis FPPE (Sentinel, Komplikasi > 2 SD, Resume Medis Kronis)

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-229`
* **Nama Task**: Pembangunan Layanan Penerimaan Feed Pemicu Kritis FPPE (Insiden Sentinel Keselamatan Pasien, Komplikasi Bedah > 2 SD, dan Keterlambatan Resume Medis Kronis)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 15: OPPE, FPPE & Recredentialing
* **Slice Delivery**: `S-C1-OPPE` (Kontrak API `v19`)
* **Status**: `✅ Selesai (2026-11-09)`
* **File Implementasi Utama**:
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/DTOs/FppeEvaluationDtos.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Services/IFppeTriggerDetectionService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Services/FppeTriggerDetectionService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Controllers/FppeFocusedEvaluationController.cs`
  - `NewQuilvianSystemBackend/Program.cs`

---

## 2. Latar Belakang Bisnis & Regulasi Akreditasi Rumah Sakit
Dalam standar akreditasi rumah sakit (KARS STARKES 2024 dan *Joint Commission International* - JCI), evaluasi praktik profesional seorang dokter tidak hanya dilakukan secara berkala (OPPE), melainkan wajib dilaksanakan secara terfokus (*Focused Professional Practice Evaluation* - FPPE) ketika terdeteksi indikator bahaya keselamatan klinis.

Layanan `BE-HRD-229` mengimplementasikan adapter integrasi `INT-HR-74` yang bertindak sebagai sensor pendeteksi pemicu kritis (*critical triggers*) dari sistem pelayanan rumah sakit (SIMRS) dan Komite Mutu & Keselamatan Pasien (KMKP):
1. **Insiden Sentinel Keselamatan Pasien (`VR-OPP-05`)**: Satu (1) kejadian sentinel fatal (seperti salah lokasi operasi, tertinggalnya benda asing pada tubuh pasien, atau kematian tak terduga) otomatis berstatus `FATAL` dan langsung menginisiasi draf penugasan investigasi FPPE.
2. **Lonjakan Angka Komplikasi Bedah (`VR-OPP-06`)**: Jika angka kejadian infeksi daerah operasi atau komplikasi bedah seorang dokter melampaui 2 Standar Deviasi (> 2 SD) dari rata-rata KSM selama 3 bulan berturut-turut, sistem menerbitkan peringatan `ERROR` investigasi.
3. **Keterlambatan Resume Medis Kronis (`VR-OPP-07`)**: Keterlambatan pengisian ringkasan pulang (resume EMR) > 48 jam yang melampaui 20% kasus selama 2 bulan berturut-turut memicu penerbitan evaluasi disiplin dokumentasi klinis.
4. **Rujukan Subkomite Etik & Disiplin Profesi**: Masuknya nota pelanggaran etika dan disiplin kedokteran untuk ditindaklanjuti dalam audit klinis.

---

## 3. Spesifikasi Endpoint API Bergaya Swagger

### Tag Grup Swagger: `[Tags("Corporate - HRD FPPE Incident & Focused Evaluation")]`
**Base Route**: `/api/v1/corporate/hrd/clinical-performance/fppe`

| Method | Path Endpoint | Deskripsi Bisnis & Fungsional | Otorisasi & Role | Request Payload | Response Model |
| :---: | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/corporate/hrd/clinical-performance/fppe/triggers/feed` | Menampilkan feed pemantauan real-time ambang batas pemicu kritis FPPE dari SIMRS/KMKP. | `[Authorize]` Komite Medik / Subkomite Mutu | - | `200 OK` (`ApiResponse<List<FppeTriggerFeedDto>>`) |
| `POST` | `/api/v1/corporate/hrd/clinical-performance/fppe/triggers/incoming` | Menerima webhook/event pemicu insiden keselamatan pasien asinkronus (`INT-HR-74`). | `[Authorize]` SIMRS Service / KMKP | `FppeTriggerFeedDto` | `200 OK` (`ApiResponse<FppeTriggerFeedDto>`) |

---

## 4. Contoh Konkret Skenario Rumah Sakit

### Skenario: Deteksi Insiden Kritis Pasca-Tindakan Bedah Ortopedi
* **Pemicu**: Modul Laporan Insiden SIMRS KMKP mendeteksi kejadian sentinel komplikasi emboli tak terduga.
* **Payload Masukan Webhook**:
```json
{
  "workforceProfileId": "e1f2a3b4-5678-90ab-cdef-1234567890ab",
  "doctorName": "dr. Robert Sp.OT",
  "doctorCode": "DOC-ORT-012",
  "specialization": "Spesialis Orthopaedi & Traumatologi",
  "triggerType": "SENTINEL_EVENT",
  "severity": "FATAL",
  "incidentDate": "2026-11-09T08:30:00Z",
  "description": "Terjadi komplikasi kardiopulmoner mendadak pasca-reduksi fraktur terbuka tibia grade III.",
  "metricEvidence": "Laporan Insiden Keselamatan Pasien KMKP No: IKP/2026/11/008 (Kategori Sentinel Merah).",
  "recommendedAction": "Pembekuan sementara tindakan bedah reduksi terbuka dan inisiasi audit terfokus FPPE."
}
```
* **Respons Sistem (`200 OK`)**:
```json
{
  "code": 200,
  "status": "OK",
  "message": "Event pemicu kritis FPPE berhasil diterima dan dicatat.",
  "data": {
    "triggerId": "7a8b9c0d-1e2f-3a4b-5c6d-7e8f9a0b1c2d",
    "workforceProfileId": "e1f2a3b4-5678-90ab-cdef-1234567890ab",
    "doctorName": "dr. Robert Sp.OT",
    "doctorCode": "DOC-ORT-012",
    "triggerType": "SENTINEL_EVENT",
    "severity": "FATAL",
    "detectedAt": "2026-11-09T08:30:05Z"
  }
}
```

---

## 5. Verifikasi Acceptance Criteria & Integritas Bisnis
1. **Pemicu Real-Time Insiden Sentinel (`VR-OPP-05`, `AT-HRD-OPP-10`)**: ✅ Lulus. Insiden sentinel terdeteksi seketika dengan flag merah `FATAL`.
2. **Pemicu Komplikasi Melampaui 2 SD (`VR-OPP-06`, `AT-HRD-OPP-11`)**: ✅ Lulus. Algoritma memindai deviasi standar dan mengalirkan event investigasi.
3. **Pemicu Keterlambatan Resume EMR > 48 Jam (`VR-OPP-07`, `AT-HRD-OPP-12`)**: ✅ Lulus. Evaluasi rasio keterlambatan resume medis terintegrasi pada feed pengawasan.
