# Laporan Perubahan Backend — `BE-HRD-013`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `BE-HRD-013` |
| Judul | Siklus Evaluasi Kinerja Karyawan, Skala Penilaian KPI, dan Finalisasi |
| Slice | `S-C3` (Manajemen Kinerja / Performance Management) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang 5 |
| Trace | `HRD-DEC-003`, `IN-13`, `HRD-DEP-002` |
| Contract version | API Contract v1 (Draft) |
| Dependency | `BE-HRD-012` (Selesai ✅) |
| Klasifikasi | `HIGH` |
| Task mode | `BACKEND` |
| Target verifikasi | `WfpPerformanceReviewController.cs`, `WfpPerformanceReviewDetailController.cs`, `WfpPerformanceReviewDtos.cs`, `WfpPerformanceReviewDetailDtos.cs`, `Program.cs` |
| Model | Gemini 3.8 Flash (High) |
| Tanggal | 27 Oktober 2026 |
| Status | Selesai (`PASS`) |

---

## 1. Masalah yang diperbaiki & diverifikasi

1. **Siklus Penilaian Kinerja Karyawan Tunggal per Periode**:
   Penilaian kinerja pegawai rumah sakit (evaluasi capaian KPI pelayanan, kepatuhan SOP, kedisiplinan, dan orientasi pasien) wajib terkunci pada 1 evaluasi aktif per siklus evaluasi (`TrxPerformanceCycle`). Sistem mencegah pembuatan penilaian ganda untuk pegawai yang sama dalam siklus berjalan.
2. **Integritas Penilaian yang Sudah Final**:
   Nilai evaluasi kinerja yang telah disahkan oleh atasan penilai dan disetujui direksi/HR tidak boleh diubah secara diam-diam. Setiap koreksi nilai pasca-finalisasi wajib melalui prosedur pengajuan revisi dengan jejak audit alasan perubahan.
3. **Privasi Hasil Kinerja**:
   Setiap pegawai hanya berhak melihat hasil evaluasi kinerja dan skor capaian miliknya sendiri, sementara pimpinan ruangan dan HRD memiliki wewenang agregasi unit.

---

## 2. Proses bisnis & alur kerja rumah sakit

```
[HR Performance Officer] ──► Membuka Siklus Evaluasi Kinerja (Misal Semester 1 / Tahunan)
                                      │
                                      ▼
             [Atasan Langsung / Kepala Ruangan / Kepala Instalasi]
                                      │
                                      ├──► 1. Membuat Review Kinerja: POST /performance-reviews
                                      │       └─► Validasi: Hanya 1 review aktif per siklus
                                      │
                                      ├──► 2. Mengisi Rincian Skor KPI: POST /details
                                      │       ├─► Bobot Indikator, Target, Capaian Aktual
                                      │       └─► Skala Nilai (A/B/C/D atau 1-5)
                                      │
                                      ├──► 3. Finalisasi Evaluasi Kinerja: PUT /performance-reviews/{id}
                                      │       └─► Status: Final (Terkunci dari modifikasi sepihak)
                                      │
                                      └──► 4. Pegawai Melihat Hasil Ulasan Mandiri: GET /performance-reviews
                                              └─► Umpan balik pengembangan karier & rekomendasi diklat
```

* **Tujuan**: Memberikan evaluasi obyektif terhadap kinerja dan kontribusi mutu staf rumah sakit secara adil, transparan, dan terlindungi kerahasiaannya.
* **Pelaku**: Kepala Ruangan / Penilai, HR Performance Officer, Pegawai yang Dinilai.
* **Pemicu**: Jadwal evaluasi kinerja semesteran / tahunan rumah sakit.
* **Langkah & Aturan**:
  1. Pegawai hanya dapat memiliki 1 lembar penilaian per siklus evaluasi.
  2. Rincian evaluasi memuat skor bobot indikator KPI terverifikasi.
  3. Lembar penilaian yang telah difinalisasi dikunci dari perubahan tanpa alur revisi formal.

---

## 3. Spesifikasi Endpoint API (Gaya Swagger)

### Grup: `Corporate / Human Resource / Performance Management / Performance Review`
Tag: `[Tags("Corporate / Human Resource / Performance Management / Performance Review")]`

| Method | Path | Deskripsi | Auth / Permission | Request Body / Query | Response Model |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `api/v1/corporate/human-resource/workforce-profiles/{id}/performance-reviews/filters/metadata` | Mengambil metadata opsi filter review kinerja | `WfpPerformanceReview:Read` | - | `ApiResponse<WfpPerformanceReviewFilterMetadataResponse>` |
| `GET` | `api/v1/corporate/human-resource/workforce-profiles/{id}/performance-reviews/summary` | Mengambil ringkasan statistik penilaian kinerja pegawai | `WfpPerformanceReview:Read` | `id` (Guid) | `ApiResponse<WfpPerformanceReviewSummaryResponse>` |
| `GET` | `api/v1/corporate/human-resource/workforce-profiles/{id}/performance-reviews` | Mengambil daftar riwayat review kinerja berhalaman | `WfpPerformanceReview:Read` | `pageNumber, pageSize, search, status` | `ApiResponse<PagedResult<WfpPerformanceReviewListResponse>>` |
| `GET` | `api/v1/corporate/human-resource/workforce-profiles/{id}/performance-reviews/{reviewId}` | Mengambil rincian evaluasi kinerja beserta skor total | `WfpPerformanceReview:Read` | `reviewId` (Guid) | `ApiResponse<WfpPerformanceReviewDetailResponse>` |
| `POST` | `api/v1/corporate/human-resource/workforce-profiles/{id}/performance-reviews` | Membuat lembar penilaian kinerja baru untuk satu siklus | `WfpPerformanceReview:Create` | `CreatePerformanceReviewRequest` | `ApiResponse<WfpPerformanceReviewDetailResponse>` (201 Created) |
| `PUT` | `api/v1/corporate/human-resource/workforce-profiles/{id}/performance-reviews/{reviewId}` | Memperbarui atau memfinalisasi lembar penilaian kinerja | `WfpPerformanceReview:Update` | `UpdatePerformanceReviewRequest` | `ApiResponse<WfpPerformanceReviewDetailResponse>` |

### Grup: `Corporate / Human Resource / Performance Management / Performance Review Detail`
Tag: `[Tags("Corporate / Human Resource / Performance Management / Performance Review Detail")]`

| Method | Path | Deskripsi | Auth / Permission | Request Body / Query | Response Model |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `api/v1/corporate/human-resource/workforce-profiles/{id}/performance-reviews/{revId}/details` | Mengambil daftar rincian indikator KPI kinerja | `WfpPerformanceReviewDetail:Read` | `pageNumber, pageSize` | `ApiResponse<PagedResult<WfpPerformanceReviewDetailListResponse>>` |
| `POST` | `api/v1/corporate/human-resource/workforce-profiles/{id}/performance-reviews/{revId}/details` | Menginput skor dan capaian indikator KPI | `WfpPerformanceReviewDetail:Create` | `CreatePerformanceReviewDetailRequest` | `ApiResponse<WfpPerformanceReviewDetailResponse>` (201 Created) |

---

## 4. Bukti implementasi kode

1. **Controller Evaluasi Kinerja & Rincian KPI**:
   - Berkas: [WfpPerformanceReviewController.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Controllers/WfpPerformanceReviewController.cs)
   - Berkas: [WfpPerformanceReviewDetailController.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Controllers/WfpPerformanceReviewDetailController.cs)

2. **DTOs & Validasi**:
   - Berkas: [WfpPerformanceReviewDtos.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/DTOs/WfpPerformanceReviewDtos.cs)
   - Berkas: [WfpPerformanceReviewDetailDtos.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/DTOs/WfpPerformanceReviewDetailDtos.cs)

---

## 5. Ringkasan Kepatuhan & Status

* **Status Task**: Selesai (`PASS`) ✅
* **Acceptance Criteria 1**: Satu pegawai tidak dapat dinilai dua kali pada siklus yang sama -> **LULUS**.
* **Acceptance Criteria 2**: Penilaian yang sudah final tidak dapat diubah diam-diam; koreksi menghasilkan revisi -> **LULUS**.
* **Acceptance Criteria 3**: Pegawai melihat hasilnya sendiri, bukan hasil orang lain -> **LULUS**.
