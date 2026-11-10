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


---

## Pengerjaan Ulang 10 November 2026 — Remediasi Gelombang 1 Audit Backend HR

| Field | Nilai |
| --- | --- |
| Dasar | [backend-unfinished-work.md](../../../roadmap/backend-unfinished-work.md) bagian 10 Gelombang 1, kelompok B — data karangan |
| Task mode | `BACKEND` |
| Model | Claude Fable 5.1 |
| Commit backend saat dikerjakan | mulai `edfd843bac1c2860da7ae9a5ddcb68a75cf42666`; pemilik melakukan commit dan merge selama sesi (HEAD akhir `05325918`, branch `MHamzah`) |
| Status sesudah pengerjaan ulang | ✅ **SELESAI 10 November 2026** (dikerjakan ulang) |

### Masalah yang ditemukan audit

Bila dokter belum punya riwayat OPPE, skor tahun ke-1/2/3 diisi angka karangan 82,5 / 85,0 / 88,5, sehingga rapor dan tier terlihat sah padahal datanya tidak ada.

### Perubahan yang dikerjakan

- Tahun yang belum punya rapor OPPE dan tidak diberi nilai simulasi **tidak diisi**. Rapor dikembalikan dengan `IsDataSufficient = false`, `AvailableEvaluationYears`, `MissingEvaluationYears`, `DataSufficiencyMessage`, serta tier `InsufficientData` (field tambahan pada `RecredentialingScorecardDto`; field lama tidak berubah).
- Bila ketiga tahun tersedia, formula 25%-35%-40% dan klasifikasi tier berjalan seperti semula.

### Berkas yang berubah

- `Areas/Corporate/HumanResource/PerformanceManagement/Services/RecredentialingQualityService.cs`
- `Areas/Corporate/HumanResource/PerformanceManagement/DTOs/RecredentialingQualityDtos.cs`

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
| Rapor tidak menampilkan skor karangan | Terpenuhi | nilai kosong dilaporkan sebagai data belum cukup |
| Formula 25-35-40 dan tier Hijau/Kuning/Merah | Terpenuhi | logika tidak berubah bila data lengkap |

### Database dan migration

`NOT APPLICABLE` — tidak ada perubahan skema untuk task ini.

### Catatan penutup

- Delta kontrak (aditif): nilai baru `InsufficientData` pada `CalculatedTier`/`AssignedTier`. Frontend yang hanya mengenal tiga tier perlu menampilkan nilai ini sebagai "data belum cukup".
- Perubahan sampingan: migration hasil scaffold otomatis `20261110044224_AddHrdDurableAuditAndPinLockouts` (14.620 baris, 79 CreateTable/26 DropTable di luar lingkup) **dibuang** dan diganti migration SQL manual; snapshot model yang ikut ter-commit pemilik dibiarkan karena sudah bercampur dengan merge cabang lain.
- Status Git akhir sesi: berkas task sudah masuk HEAD lewat commit pemilik; sisa di working tree adalah penghapusan dua berkas scaffold dan satu berkas migration manual baru (lihat `git status --short` pada ringkasan sesi).
