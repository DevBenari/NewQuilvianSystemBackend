# Laporan Penyelesaian Task: BE-HRD-224 Tata Kelola Periode Siklus OPPE Tahunan/Semesteran & Inisiasi Rapor Dokter

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-224`
* **Nama Task**: Pembangunan Tata Kelola Periode Siklus OPPE Tahunan/Semesteran, Penegakan Validasi Rasio Bobot Baku 60:40 (`VR-OPP-01`), dan Otomasi Inisiasi Draf Rapor Dokter
* **Modul / Sub-Modul**: Human Resource / Sub-modul 15: OPPE, FPPE & Recredentialing
* **Slice Delivery**: `S-C1-OPPE`
* **Status**: `✅ Selesai (2026-11-09)`
* **File Implementasi Utama**:
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Models/TrxOppeEvaluationPeriod.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Models/TrxOppeStaffEvaluation.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/DTOs/OppeEvaluationDtos.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Services/OppeDataAggregationService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Controllers/OppeOngoingEvaluationController.cs`

---

## 2. Latar Belakang & Aturan Bisnis `VR-OPP-01`
1. **Siklus Evaluasi Rutin Rumah Sakit**: Sesuai kebijakan tata kelola komite medik, evaluasi OPPE dilaksanakan dalam rentang waktu terstruktur: baik per semester (Semester 1 & 2) maupun tahunan penuh (*full year*).
2. **Aturan Pembobotan Baku `VR-OPP-01` (`HRD-DEC-152`)**:
   - Komponen evaluasi OPPE terbagi secara ketat menjadi dua pilar:
     - **Pilar Kuantitatif SIMRS / KMKP**: Bobot 60.00% (terukur secara otomatis dari data rekam medis dan insiden mutu).
     - **Pilar Kualitatif Telaah Sejawat (Peer Review) KSM**: Bobot 40.00% (penilaian sejawat berbasis bukti perilaku dan etika).
   - Sistem memvalidasi bahwa total persentase pembobotan `QuantitativeWeightPercentage + QualitativeWeightPercentage` **wajib tepat 100.00%**. Pengajuan periode baru ditolak jika total bobot menyimpang dari 100%.
3. **Inisiasi Masif Draf Rapor Dokter**:
   - Setelah periode diaktifkan, Subkomite Mutu dapat menginisiasi lembar draf evaluasi secara massal untuk seluruh dokter aktif yang memiliki penugasan klinis di rumah sakit.
   - Hal ini memastikan tidak ada tenaga medis yang luput dari evaluasi mutu tahunan.

---

## 3. Spesifikasi Endpoint API Bergaya Swagger

### Tag Grup Swagger: `[Tags("Corporate - HRD OPPE Clinical Performance Evaluation")]`
**Base Route**: `/api/v1/corporate/hrd/clinical-performance/oppe`

| Method | Path Endpoint | Deskripsi Bisnis & Fungsional | Otorisasi & Role | Request Payload | Response Model |
| :---: | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/corporate/hrd/clinical-performance/oppe/periods` | Menampilkan seluruh riwayat dan daftar periode siklus OPPE aktif. | `[Authorize]` Komite Medik / HR | Query `?year=` | `200 OK` (`ApiResponse<List<OppeEvaluationPeriodDto>>`) |
| `POST` | `/api/v1/corporate/hrd/clinical-performance/oppe/periods` | Membuka periode siklus OPPE baru dengan verifikasi total bobot 100% (`VR-OPP-01`). | `[Authorize]` Ketua Komite Medik / Subkomite Mutu | `CreateOppeEvaluationPeriodRequestDto` | `201 Created` (`ApiResponse<OppeEvaluationPeriodDto>`) |
| `POST` | `/api/v1/corporate/hrd/clinical-performance/oppe/periods/{id}/initiate-staff` | Menginisiasi secara otomatis lembar draf rapor evaluasi bagi seluruh staf dokter pada periode terpilih. | `[Authorize]` Sekretariat Komite Medik | - | `200 OK` (`ApiResponse<int>`) |

---

## 4. Contoh Konkret Skenario Pembukaan Periode OPPE

* **Endpoint**: `POST /api/v1/corporate/hrd/clinical-performance/oppe/periods`
* **Request Body**:
```json
{
  "periodCode": "OPPE-2026-SEM2",
  "periodName": "Evaluasi Praktik Profesional Berkelanjutan (OPPE) Semester II Tahun 2026",
  "evaluationYear": 2026,
  "semester": 2,
  "startDate": "2026-07-01T00:00:00Z",
  "endDate": "2026-12-31T23:59:59Z",
  "quantitativeWeightPercentage": 60.00,
  "qualitativeWeightPercentage": 40.00
}
```
* **Response Body (`201 Created`)**:
```json
{
  "success": true,
  "statusCode": 201,
  "message": "Periode evaluasi kinerja klinis OPPE berhasil dibuka.",
  "data": {
    "id": "e3f4a5b6-c7d8-9012-3456-7890abcdef12",
    "periodCode": "OPPE-2026-SEM2",
    "periodName": "Evaluasi Praktik Profesional Berkelanjutan (OPPE) Semester II Tahun 2026",
    "evaluationYear": 2026,
    "semester": 2,
    "startDate": "2026-07-01T00:00:00Z",
    "endDate": "2026-12-31T23:59:59Z",
    "status": "OPEN",
    "quantitativeWeightPercentage": 60.00,
    "qualitativeWeightPercentage": 40.00,
    "totalStaffCount": 0
  },
  "errors": null,
  "timestamp": "2026-11-09T14:23:00Z"
}
```

---

## 5. Verifikasi Kompilasi & Konformansi
* **Status Kompilasi C#**: Lolos kompilasi dengan **`0 Error(s)`**.
* **Keamanan Transaksi**: Pembuatan lembar evaluasi menggunakan validasi idempotensi sehingga eksekusi berulang tidak memicu duplikasi data staf dokter pada periode yang sama.
