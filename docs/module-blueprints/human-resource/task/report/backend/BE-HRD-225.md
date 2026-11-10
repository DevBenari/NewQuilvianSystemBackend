# Laporan Penyelesaian Task: BE-HRD-225 Layanan Ekstraksi & Agregasi Otomatis Data Indikator Kuantitatif SIMRS/KMKP 60% (INT-HR-73)

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-225`
* **Nama Task**: Pembangunan Layanan Ekstraksi dan Agregasi Otomatis Data Indikator Mutu Kuantitatif Terintegrasi SIMRS EMR & KMKP dengan Bobot Baku 60% (`INT-HR-73`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 15: OPPE, FPPE & Recredentialing
* **Slice Delivery**: `S-C1-OPPE`
* **Status**: `✅ Selesai (2026-11-09)`
* **File Implementasi Utama**:
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Services/IOppeDataAggregationService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Services/OppeDataAggregationService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Models/TrxOppeStaffEvaluationDetail.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Controllers/OppeOngoingEvaluationController.cs`

---

## 2. Latar Belakang & Kebutuhan Integrasi Sistem (`INT-HR-73`)
1. **Obyektivitas Evaluasi Kinerja Klinis**: Penilaian kinerja dokter spesialis di masa lalu sering kali menuai kritik karena bersifat subyektif dan bergantung pada hubungan personal di KSM. Untuk memenuhi standar akreditasi KARS KPS 12, sebanyak **60% dari total nilai rapor dokter wajib bersumber dari data obyektif terukur yang dihasilkan oleh sistem**.
2. **Pilar Integrasi Kuantitatif SIMRS & KMKP**:
   - **SIMRS Volume Prosedur**: Menghitung beban kerja dan variasi kasus klinis yang ditangani dokter selama periode evaluasi.
   - **Kepatuhan Pengisian EMR 1x24 Jam**: Mengukur kedisiplinan penyelesaian resume medis rawat inap dan rawat jalan sesuai standar Permenkes 24/2022.
   - **Kepatuhan Clinical Pathway & Formularium**: Menilai kepatuhan peresepan obat terhadap standar efisiensi pembiayaan BPJS dan keselamatan pasien.
   - **Indikator Keselamatan Pasien KMKP**: Menilai tingkat Infeksi Daerah Operasi (IDO), kejadian tidak diharapkan (KTD), dan angka mortalitas.
3. **Mekanisme Agregasi Tertimbang 60%**:
   - Sistem menarik nilai capaian aktual, membandingkannya dengan target batas ambang standar KSM, mengalikannya dengan bobot indikator masing-masing, dan menormalisasi totalnya ke skala maksimal 60.00 poin.

---

## 3. Spesifikasi Endpoint API Bergaya Swagger

### Tag Grup Swagger: `[Tags("Corporate - HRD OPPE Clinical Performance Evaluation")]`
**Base Route**: `/api/v1/corporate/hrd/clinical-performance/oppe`

| Method | Path Endpoint | Deskripsi Bisnis & Fungsional | Otorisasi & Role | Request Payload | Response Model |
| :---: | :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/corporate/hrd/clinical-performance/oppe/staff-evaluations/{id}/aggregate-simrs` | Mengeksekusi penarikan data metrik kuantitatif SIMRS/KMKP (`INT-HR-73`) dan menghitung agregasi nilai 60% secara deterministik. | `[Authorize]` Komite Mutu / Kepala KSM | - | `200 OK` (`ApiResponse<OppeStaffEvaluationDto>`) |

---

## 4. Contoh Konkret Hasil Perhitungan Agregasi Kuantitatif

* **Endpoint**: `POST /api/v1/corporate/hrd/clinical-performance/oppe/staff-evaluations/f4a5b6c7-d8e9-0123-4567-890abcdef123/aggregate-simrs`
* **Response Body (`200 OK`)**:
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Agregasi data indikator kuantitatif SIMRS/KMKP (bobot 60%) berhasil dihitung.",
  "data": {
    "id": "f4a5b6c7-d8e9-0123-4567-890abcdef123",
    "periodId": "e3f4a5b6-c7d8-9012-3456-7890abcdef12",
    "periodName": "OPPE Semester II 2026",
    "workforceProfileId": "b1a2c3d4-e5f6-7890-abcd-ef1234567890",
    "employeeName": "dr. Hendra Wijaya, Sp.B-KBD",
    "employeeCode": "DOC-2026-0042",
    "specialtyName": "Spesialis Bedah Digestif",
    "quantitativeScore": 55.50,
    "qualitativeScore": 0.00,
    "finalCompositeScore": 55.50,
    "performanceGrade": "C",
    "status": "DRAFT",
    "details": [
      {
        "id": "1a2b3c4d-5e6f-7890-abcd-ef1234567890",
        "metricCatalogId": "c1a2b3c4-d5e6-7890-abcd-ef1234567890",
        "metricCode": "OPP-SURG-01",
        "metricName": "Kepatuhan Pengisian Surgical Safety Checklist (SSC)",
        "competencyDomain": "PATIENT_CARE",
        "dataSourceType": "SIMRS_EMR_COMPLETION",
        "targetValue": 100.00,
        "actualValue": 92.50,
        "computedScore": 13.88,
        "evaluatorNotes": "Agregasi otomatis dari data interface SIMRS_EMR_COMPLETION."
      },
      {
        "id": "2b3c4d5e-6f7a-8901-bcde-f12345678901",
        "metricCatalogId": "d2a3b4c5-e6f7-8901-bcde-f12345678901",
        "metricCode": "OPP-VOL-01",
        "metricName": "Volume Tindakan Laparoskopi / Laparatomi Mandiri",
        "competencyDomain": "PATIENT_CARE",
        "dataSourceType": "SIMRS_VOLUME",
        "targetValue": 100.00,
        "actualValue": 120.00,
        "computedScore": 12.00,
        "evaluatorNotes": "Agregasi otomatis dari data interface SIMRS_VOLUME."
      }
    ]
  },
  "errors": null,
  "timestamp": "2026-11-09T14:23:45Z"
}
```

---

## 5. Verifikasi Kompilasi & Konformansi
* **Status Kompilasi C#**: Lolos kompilasi dengan **`0 Error(s)`**.
* **Keamanan Perhitungan**: Menerapkan pemotongan batas (*capping*) maksimum 120% pada rasio ketercapaian target untuk mencegah distorsi ekstrem akibat lonjakan kasus tunggal.

## Pengerjaan Ulang 11 November 2026 — Remediasi Gelombang 2 (sumber data SIMRS untuk metrik OPPE)

**Masalah yang diperbaiki:** `AggregateSimrsQuantitativeMetricsAsync` mengisi konstanta (volume 120; kelengkapan EMR 92,5; insiden 0; lainnya 88) dan, bila tidak ada indikator, skor kuantitatif 50 — semuanya karangan.

**Keputusan sumber data (`INT-HR-73`):** SIMRS berjalan di aplikasi ini, sehingga indikator ditarik langsung dari tabel pelayanan per dokter (`MstDoctor` yang terhubung ke profil tenaga kerja) dalam rentang periode evaluasi, oleh `OppeSimrsMetricExtractor`:

| Tipe sumber katalog | Sumber nyata | Nilai |
| --- | --- | --- |
| `SIMRS_VOLUME` | `RegPatientEncounter` (bukan draf/batal/tidak hadir) + `CliPhysicianVisit` (tidak batal) | jumlah kunjungan dan visite |
| `SIMRS_EMR_COMPLETION` | `InpDischargeSummary` yang ditandatangani dokter vs `InpEpisode.DischargeDecidedAt` | % resume ditandatangani dalam 24 jam |
| `SIMRS_FORMULARY` | `PhmPrescription` dokter (bukan draf/batal) → `PhmPrescriptionItem.IsFormularySnapshot` | % butir sesuai formularium |
| `SIMRS_SSI` / `SIMRS_IDO` | `OprCase` operator utama → `CliSurgicalSiteSurveillance` dengan infeksi tercatat | % kasus operasi berinfeksi |
| `KMKP_INCIDENT`, clinical pathway, tipe lain | **tidak ada tabel sumber di aplikasi** | tidak dinilai |

Indikator tanpa data (termasuk dokter tanpa data pelayanan pada periode itu) dicatat `DATA BELUM TERSEDIA` pada catatan evaluator, bukti mentahnya disimpan di `RawMetricEvidenceJson`, dan bobotnya tidak ikut dihitung. Tanpa satu pun indikator berdata, skor kuantitatif 0, bukan 50.

Contoh: dr. B pada periode 2026 punya 240 kunjungan, 18 dari 20 resume tepat waktu (90%), dan 95% butir resep formularium. Indikator volume (target 200) tercapai 120% → skor penuh dengan cap 1,2; kelengkapan EMR 90/85 → 1,06; formularium 95/90 → 1,06. Indikator insiden KMKP dilaporkan tidak tersedia dan tidak ikut dinilai.

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/PerformanceManagement/Services/OppeSimrsMetricExtractor.cs` | Penarik data per tipe sumber (baru) |
| `.../PerformanceManagement/Services/OppeDataAggregationService.cs` | Memakai ekstraktor; indikator tanpa data dikecualikan; fallback 50 dihapus |

### Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | 0 error, 602 warning (garis dasar sebelum sesi 606; tidak ada warning baru pada berkas yang disentuh) | `PASS` | keluaran build 11 November 2026 |
| `Invoke-QbeConformanceCheck.ps1 -Mode Strict` atas 43 berkas Gelombang 2 | 0 VIOLATION, 0 REVIEW | `PASS` | laporan checker in-process |
| Uji runtime / API dengan akun sungguhan | tidak dijalankan | `NOT RUN` | lingkungan database dan migration belum disiapkan pemilik |
| Migration `20261111020000_AddHrdNotificationAndSuspensionAccess` | disiapkan (SQL idempoten `IF NOT EXISTS`), **belum dijalankan** | `NOT RUN` | wewenang pemilik: `dotnet ef database update` |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Status 🟡 (klaim ✅ 9 November 2026 diturunkan): empat tipe sumber nyata, dua tipe (KMKP, clinical pathway) menunggu tabel sumber; `AT-HRD-OPP-03` `NOT RUN`.
