# Laporan Penyelesaian Task: BE-HRD-233 Kalkulator Rapor Komposit 3 Tahun Rekredensialing (25%-35%-40%) & Klasifikasi Tier

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-233`
* **Nama Task**: Pembangunan Kalkulator Rapor Komposit 3 Tahun Rekredensialing dengan Rasio Progresif (25%-35%-40%) dan Otomasi Klasifikasi 3 Tier Eligibilitas
* **Modul / Sub-Modul**: Human Resource / Sub-modul 15: OPPE, FPPE & Recredentialing
* **Slice Delivery**: `S-C1-OPPE` (Kontrak API `v19`)
* **Status**: `✅ Selesai (2026-11-09)`
* **File Implementasi Utama**:
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/DTOs/RecredentialingQualityDtos.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Services/IRecredentialingCompositeProvider.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Services/RecredentialingQualityService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Controllers/RecredentialingQualityController.cs`

---

## 2. Latar Belakang & Formula Kalkulasi Deterministik 3 Tahun
Sesuai amanat Permenkes Kredensial Tenaga Medis dan standar STARKES KARS 2024, proses rekredensialing berkala dokter yang diselenggarakan setiap 3 tahun sekali mewajibkan evaluasi kinerja komprehensif berbasis data historis OPPE tahunan dan rekam jejak evaluasi terfokus FPPE (`INT-HR-75`, `HRD-DEC-155`).

### 1. Formula Progresif Rapor Komposit 3 Tahun (`VR-OPP-15`, `AT-HRD-OPP-19`)
Kinerja tahun terakhir diberikan bobot paling tinggi untuk merefleksikan kompetensi klinis terkini:
$$\text{FinalCompositeScore} = (0.25 \times \text{Year1Score}) + (0.35 \times \text{Year2Score}) + (0.40 \times \text{Year3Score})$$
*Contoh*: Jika dokter memperoleh skor Tahun 1 = 80.0, Tahun 2 = 85.0, dan Tahun 3 = 90.0:
$$\text{FinalCompositeScore} = (0.25 \times 80) + (0.35 \times 85) + (0.40 \times 90) = 20.0 + 29.75 + 36.0 = 85.75$$

### 2. Matriks Otomasi Klasifikasi 3 Tier Eligibilitas
Sistem memetakan hasil evaluasi ke dalam 3 kelompok perlakuan administratif:
1. **Tier Hijau (*Green Fast-Track*) (`VR-OPP-16`, `AT-HRD-OPP-20`)**:
   - Syarat: Skor komposit akhir $\ge 80.0$, nihil insiden sentinel, dan nihil pelanggaran FPPE.
   - Hak Istimewa: Rekomendasi perpanjangan SPK dan RKK tanpa hambatan (*fast-track*) tanpa perlu melalui sidang klarifikasi panjang.
2. **Tier Kuning (*Yellow Clarification Review*) (`VR-OPP-17`)**:
   - Syarat: Skor komposit berada pada rentang $65.0 - 79.9$, atau terdapat catatan penundaan resume EMR $> 20\%$.
   - Tindak Lanjut: Wajib menjalani wawancara klarifikasi dengan Ketua KSM dan menyepakati rencana perbaikan performa.
3. **Tier Merah (*Red Plenary Investigation*) (`VR-OPP-18`)**:
   - Syarat: Skor komposit $< 65.0$, atau terdapat riwayat kejadian sentinel keselamatan pasien (`VR-OPP-05`), atau kegagalan penugasan FPPE (`FAILED_RESTRICTED`).
   - Tindak Lanjut: Wajib disidangkan dalam pleno khusus Komite Medik untuk mempertimbangkan pembatasan wewenang klinis Level 3 supervisi atau pencabutan kewenangan mutlak.

---

## 3. Spesifikasi Endpoint API Bergaya Swagger

### Tag Grup Swagger: `[Tags("Corporate - HRD Recredentialing Quality & Composite Scorecard")]`
**Base Route**: `/api/v1/corporate/hrd/credentialing/recredentialing`

| Method | Path Endpoint | Deskripsi Bisnis & Fungsional | Otorisasi & Role | Request Payload | Response Model |
| :---: | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/corporate/hrd/credentialing/recredentialing/scorecards/{staffId}` | Mengambil kartu rapor komposit 3 tahunan (25%-35%-40%) dan rekomendasi tier kelayakan. | `[Authorize]` Komite Medik / Dokter | - | `200 OK` (`ApiResponse<RecredentialingScorecardDto>`) |
| `POST` | `/api/v1/corporate/hrd/credentialing/recredentialing/scorecards/{staffId}/calculate` | Menghitung kalkulasi komposit 3 tahun secara deterministik dari riwayat OPPE & insiden FPPE. | `[Authorize]` Subkomite Kredensial | `CalculateRecredScorecardRequestDto` | `200 OK` (`ApiResponse<RecredentialingScorecardDto>`) |

---

## 4. Contoh Konkret Respons Rapor Komposit 3 Tahun

* **Endpoint**: `POST /api/v1/corporate/hrd/credentialing/recredentialing/scorecards/e1f2a3b4-5678-90ab-cdef-1234567890ab/calculate`
* **Request Body**:
```json
{
  "year1Score": 80.0,
  "year2Score": 85.0,
  "year3Score": 90.0,
  "simulateSentinelIncident": false,
  "simulateEmrDelinquency": false
}
```
* **Response Body (`200 OK`)**:
```json
{
  "code": 200,
  "status": "OK",
  "message": "Kalkulasi rapor komposit 3 tahun dan klasifikasi tier berhasil diproses.",
  "data": {
    "workforceProfileId": "e1f2a3b4-5678-90ab-cdef-1234567890ab",
    "doctorName": "dr. Robert Sp.BS",
    "doctorCode": "DOC-BS-009",
    "specialization": "Spesialis Bedah Saraf",
    "year1Score": 80.0,
    "year2Score": 85.0,
    "year3Score": 90.0,
    "finalCompositeScore": 85.75,
    "calculatedTier": "GreenFastTrack",
    "assignedTier": "GreenFastTrack",
    "tierDescription": "Tier Hijau - Rekomendasi Jalur Cepat (Fast-Track) Perpanjangan SPK Tanpa Hambatan",
    "isOverridden": false,
    "hasSentinelIncident": false,
    "hasEmrDelinquency": false,
    "fppeHistorySummary": "Nihil riwayat penugasan FPPE dan kasus sentinel selama siklus 3 tahun.",
    "calculatedAt": "2026-11-09T14:55:00Z"
  }
}
```

---

## 5. Verifikasi Acceptance Criteria & Integritas Bisnis
1. **Algoritma Kalkulasi 25%-35%-40% (`VR-OPP-15`, `AT-HRD-OPP-19`)**: ✅ Lulus. Perhitungan komposit: $(0.25 \times 80) + (0.35 \times 85) + (0.40 \times 90) = 85.75$ terbukti akurat dan deterministik.
2. **Klasifikasi Tier Hijau Fast-Track (`VR-OPP-16`, `AT-HRD-OPP-20`)**: ✅ Lulus. Skor $\ge 80.0$ dengan 0 sentinel langsung memperoleh rekomendasi jalur cepat tanpa bias.
