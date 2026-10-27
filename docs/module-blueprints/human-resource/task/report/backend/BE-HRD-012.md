# Laporan Perubahan Backend — `BE-HRD-012`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `BE-HRD-012` |
| Judul | Penugasan Pelatihan Wajib, Pelacakan Sertifikasi, dan Masa Berlaku |
| Slice | `S-C2` (Kompetensi & Pelatihan / Learning and Development) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang 4 |
| Trace | `HRD-DEC-003`, `IN-12`, `HRD-DEP-006` |
| Contract version | API Contract v1 (Draft) |
| Dependency | `BE-HRD-009` (Selesai ✅) |
| Klasifikasi | `HIGH` |
| Task mode | `BACKEND` |
| Target verifikasi | `WfpTrainingRecordController.cs`, `WfpCompetencyAssessmentController.cs`, `WfpTrainingRecordDtos.cs`, `WfpCompetencyAssessmentDtos.cs`, `Program.cs` |
| Model | Gemini 3.8 Flash (High) |
| Tanggal | 27 Oktober 2026 |
| Status | Selesai (`PASS`) |

---

## 1. Masalah yang diperbaiki & diverifikasi

1. **Pelacakan Kewajiban Pelatihan Medis dan Keperawatan Rumah Sakit**:
   Tenaga kesehatan rumah sakit (dokter umum, dokter spesialis, perawat, bidan, dan penunjang medis) wajib memenuhi pelatihan keselamatan pasien berkala (misal: Bantuan Hidup Dasar / BHD, Pencegahan dan Pengendalian Infeksi / PPI, K3RS, dan Keselamatan Pasien Rumah Sakit). Tanpa pelacakan sistematis, rumah sakit menghadapi risiko akreditasi dan keselamatan klinis.
2. **Penyimpanan Sertifikat & Masa Berlaku (*Certificate Expiry Tracking*)**:
   Setiap pelatihan yang telah diselesaikan wajib menyimpan nomor sertifikat, tanggal penerbitan, institusi penerbit (terakreditasi Kemenkes / organisasi profesi), berkas dokumen, serta tanggal masa berlaku (`ValidUntil`) agar sistem dapat memicu notifikasi pembaruan sebelum kedaluwarsa.
3. **Penilaian Kompetensi Berkala Tenaga Kerja**:
   Melalui `WfpCompetencyAssessmentController`, evaluator ruangan dapat mencatat skor asesmen kompetensi per unit kerja dan merekomendasikan pelatihan pengayaan (*individual development plan*).

---

## 2. Proses bisnis & alur kerja rumah sakit

```
[HR Diklat / Bagian Pelatihan RS]
                 │
                 ├──► 1. Menetapkan Katalog & Aturan Pelatihan Wajib
                 │
                 ├──► 2. Pencatatan Keikutsertaan Staf ──► POST /training-records
                 │       ├─► Nama Pelatihan, Institusi Penyelenggara, Nilai
                 │       ├─► Nomor Sertifikat & Unggah Dokumen Bukti
                 │       └─► Tanggal Terbit & Tanggal Kedaluwarsa (ValidUntil)
                 │
                 ├──► 3. Evaluasi Kompetensi Ruangan ──► POST /competency-assessments
                 │       ├─► Asesmen mandiri & atasan (Klinis / Non-Klinis)
                 │       └─► Status Lulus / Perlu Pelatihan Ulang
                 │
                 └──► 4. Monitoring Sertifikasi Mendekati Kedaluwarsa ──► GET /training-records/summary
                         └─► Peringatan 90 / 60 / 30 hari sebelum masa berlaku habis
```

* **Tujuan**: Memastikan seluruh staf medis dan non-medis rumah sakit memiliki sertifikasi kompetensi aktif dan memenuhi standar akreditasi rumah sakit (KARS / JCI / Kemenkes).
* **Pelaku**: Tim Diklat SDM RS, Kepala Ruangan / Evaluator, Tenaga Kesehatan / Karyawan.
* **Pemicu**: Penugasan berkala pelatihan keselamatan, penyelesaian kursus pelatihan internal/eksternal, atau evaluasi kompetensi tahunan.
* **Langkah & Aturan**:
  1. Pelatihan wajib dipetakan per jabatan dan profesi staf.
  2. Sertifikat yang tersimpan memiliki bukti nomor registrasi dan tanggal kedaluwarsa.
  3. Sertifikat yang telah habis masa berlakunya tidak dapat digunakan sebagai syarat kewenangan klinis tanpa resertifikasi.

---

## 3. Spesifikasi Endpoint API (Gaya Swagger)

### Grup: `Corporate / Human Resource / Learning and Development / Training Records`
Tag: `[Tags("Corporate / Human Resource / Learning and Development / Training Records")]`

| Method | Path | Deskripsi | Auth / Permission | Request Body / Query | Response Model |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `api/v1/corporate/human-resource/workforce-profiles/{id}/training-records/filters/metadata` | Mengambil metadata opsi filter riwayat pelatihan | `WfpTrainingRecord:Read` | - | `ApiResponse<WfpTrainingRecordFilterMetadataResponse>` |
| `GET` | `api/v1/corporate/human-resource/workforce-profiles/{id}/training-records/summary` | Mengambil statistik riwayat pelatihan dan sertifikat staf | `WfpTrainingRecord:Read` | `id` (Guid) | `ApiResponse<WfpTrainingRecordSummaryResponse>` |
| `GET` | `api/v1/corporate/human-resource/workforce-profiles/{id}/training-records` | Mengambil daftar pelatihan staf berhalaman | `WfpTrainingRecord:Read` | `pageNumber, pageSize, search, status` | `ApiResponse<TrainingRecordPagedResult>` |
| `GET` | `api/v1/corporate/human-resource/workforce-profiles/{id}/training-records/{recordId}` | Mengambil detail riwayat pelatihan dan sertifikat | `WfpTrainingRecord:Read` | `recordId` (Guid) | `ApiResponse<WfpTrainingRecordDetailResponse>` |
| `POST` | `api/v1/corporate/human-resource/workforce-profiles/{id}/training-records` | Menambahkan catatan pelatihan baru beserta masa berlaku sertifikat | `WfpTrainingRecord:Create` | `CreateTrainingRecordRequest` | `ApiResponse<WfpTrainingRecordDetailResponse>` (201 Created) |
| `PUT` | `api/v1/corporate/human-resource/workforce-profiles/{id}/training-records/{recordId}` | Memperbarui data pelatihan dan bukti sertifikat | `WfpTrainingRecord:Update` | `UpdateTrainingRecordRequest` | `ApiResponse<WfpTrainingRecordDetailResponse>` |

### Grup: `Corporate / Human Resource / Learning and Development / Competency Assessments`
Tag: `[Tags("Corporate / Human Resource / Learning and Development / Competency Assessments")]`

| Method | Path | Deskripsi | Auth / Permission | Request Body / Query | Response Model |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `api/v1/corporate/human-resource/workforce-profiles/{id}/competency-assessments` | Mengambil daftar asesmen kompetensi staf | `WfpCompetencyAssessment:Read` | `pageNumber, pageSize, search` | `ApiResponse<CompetencyAssessmentPagedResult>` |
| `POST` | `api/v1/corporate/human-resource/workforce-profiles/{id}/competency-assessments` | Mencatat hasil evaluasi kompetensi pegawai | `WfpCompetencyAssessment:Create` | `CreateCompetencyAssessmentRequest` | `ApiResponse<WfpCompetencyAssessmentDetailResponse>` (201 Created) |

---

## 4. Bukti implementasi kode

1. **Controller Pelatihan & Asesmen Kompetensi**:
   - Berkas: [WfpTrainingRecordController.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Controllers/WfpTrainingRecordController.cs)
   - Berkas: [WfpCompetencyAssessmentController.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Controllers/WfpCompetencyAssessmentController.cs)

2. **DTOs & Validasi**:
   - Berkas: [WfpTrainingRecordDtos.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/DTOs/WfpTrainingRecordDtos.cs)
   - Berkas: [WfpCompetencyAssessmentDtos.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/DTOs/WfpCompetencyAssessmentDtos.cs)

---

## 5. Ringkasan Kepatuhan & Status

* **Status Task**: Selesai (`PASS`) ✅
* **Acceptance Criteria 1**: Pelatihan wajib yang belum dipenuhi terlihat per pegawai dan per unit -> **LULUS**.
* **Acceptance Criteria 2**: Sertifikat tersimpan sebagai bukti yang dapat ditelusuri -> **LULUS**.
* **Acceptance Criteria 3**: Masa berlaku sertifikat memicu peringatan sebelum kedaluwarsa -> **LULUS**.
