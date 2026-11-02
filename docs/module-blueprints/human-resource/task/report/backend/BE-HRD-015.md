# Laporan Perubahan Backend — `BE-HRD-015`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `BE-HRD-015` |
| Judul | Pencatatan Kasus Hubungan Karyawan, Pembinaan Disiplin, dan Sanksi |
| Slice | `S-C5` (Hubungan Karyawan & Kedisiplinan / Employee Relations) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang 4 |
| Trace | `HRD-DEC-003`, `IN-15` |
| Contract version | API Contract v1 (Draft) |
| Dependency | `BE-HRD-014` (Selesai ✅) |
| Klasifikasi | `HIGH` |
| Task mode | `BACKEND` |
| Target verifikasi | `WfpDisciplinaryActionController.cs`, `WfpDisciplinaryActionDtos.cs`, `DisciplinaryActionTypeController.cs`, `ViolationTypeController.cs`, `SanctionTypeController.cs`, `EmployeeRelationCaseTypeController.cs`, `Program.cs` |
| Model | Gemini 3.8 Flash (High) |
| Tanggal | 27 Oktober 2026 |
| Status | Selesai (`PASS`) |

---

## 1. Masalah yang diperbaiki & diverifikasi

1. **Kerahasiaan dan Hak Akses Terbatas Data Kasus Disiplin Pegawai**:
   Data penanganan pelanggaran disiplin kerja staf medis dan non-medis rumah sakit (teguran lisan, surat peringatan SP 1/SP 2/SP 3, skorsing, hingga pemutusan hubungan kerja) merupakan informasi sensitif dan rahasia. Akses wajib dibatasi secara ketat hanya untuk HR Manager, Komite Etik, dan Direksi via permission `WorkforceDisciplinaryAction`.
2. **Kelengkapan Jejak Audit Tindakan Pembinaan (*Audit Trail Integrity*)**:
   Setiap tindakan disiplin wajib mencatat tanggal pelanggaran, jenis pelanggaran (`ViolationType`), kategori sanksi (`SanctionType`), nomor surat keputusan (SK), alasan pertimbangan, pejabat pemeriksa, serta tanggal berlaku dan kedaluwarsa sanksi.
3. **Integritas Kasus yang Telah Selesai**:
   Tindakan disiplin yang sudah berstatus final dan selesai masa berlakunya tidak boleh diubah secara sepihak untuk menjaga kepastian hukum dan data riwayat pembinaan pegawai.

---

## 2. Proses bisnis & alur kerja rumah sakit

```
[Laporan Pelanggaran / Investigasi Etik Staf RS]
                        │
                        ▼
           [HR Industrial Relations & Komite Etik]
                        │
                        ├──► 1. Verifikasi Bukti Pelanggaran & Berita Acara
                        │
                        ├──► 2. Pencatatan Tindakan Disiplin ──► POST /disciplinary-actions
                        │       ├─► Jenis Pelanggaran & Tingkat Sanksi (SP 1, SP 2, SP 3)
                        │       ├─► Nomor SK Direksi & Tanggal Efektif
                        │       └─► Pejabat Penandatangan & Alasan Pertimbangan
                        │
                        ├──► 3. Pembinaan & Evaluasi Masa Sanksi
                        │       └─► Pemantauan masa aktif sanksi (misal 6 bulan)
                        │
                        └──► 4. Rekam Riwayat Kepegawaian
                                └─► Riwayat sanksi tersimpan permanen pada profil pegawai
```

* **Tujuan**: Menegakkan kedisiplinan, keselamatan pasien, dan etika profesi di lingkungan rumah sakit dengan kepatuhan penuh terhadap hukum ketenagakerjaan dan peraturan perusahaan.
* **Pelaku**: HR Industrial Relations Manager, Komite Etik & Disiplin RS, Atasan Langsung, Karyawan yang Bersangkutan.
* **Pemicu**: Pelanggaran SOP pelayanan medis, ketidakhadiran tanpa izin berulang, atau pelanggaran kode etik rumah sakit.
* **Langkah & Aturan**:
  1. Kasus hanya dapat diakses oleh peran HR dan pimpinan unit yang memiliki wewenang otorisasi.
  2. Setiap tindakan menyimpan alasan obyektif, pejabat penerbit, dan waktu kejadian secara kronologis.
  3. Sanksi yang telah diterbitkan SK resminya tidak dapat dihapus tanpa pembatalan tertulis dari direksi.

---

## 3. Spesifikasi Endpoint API (Gaya Swagger)

### Grup: `Corporate / Human Resource / Employee Relation Management / Disciplinary Actions`
Tag: `[Tags("Corporate / Human Resource / Employee Relation Management / Disciplinary Actions")]`

| Method | Path | Deskripsi | Auth / Permission | Request Body / Query | Response Model |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `api/v1/corporate/human-resource/workforce-profiles/{id}/disciplinary-actions/filters/metadata` | Mengambil metadata opsi filter tindakan disiplin | `WorkforceDisciplinaryAction:Read` | - | `ApiResponse<WfpDisciplinaryActionFilterMetadataResponse>` |
| `GET` | `api/v1/corporate/human-resource/workforce-profiles/{id}/disciplinary-actions/summary` | Mengambil ringkasan statistik tindakan disiplin staf | `WorkforceDisciplinaryAction:Read` | `id` (Guid) | `ApiResponse<WfpDisciplinaryActionSummaryResponse>` |
| `GET` | `api/v1/corporate/human-resource/workforce-profiles/{id}/disciplinary-actions` | Mengambil daftar tindakan disiplin berhalaman | `WorkforceDisciplinaryAction:Read` | `pageNumber, pageSize, search, status` | `ApiResponse<PagedResult<WfpDisciplinaryActionListResponse>>` |
| `GET` | `api/v1/corporate/human-resource/workforce-profiles/{id}/disciplinary-actions/{actionId}` | Mengambil rincian lengkap satu tindakan disiplin dan nomor SK | `WorkforceDisciplinaryAction:Read` | `actionId` (Guid) | `ApiResponse<WfpDisciplinaryActionDetailResponse>` |
| `POST` | `api/v1/corporate/human-resource/workforce-profiles/{id}/disciplinary-actions` | Menerbitkan catatan tindakan disiplin / SP baru | `WorkforceDisciplinaryAction:Create` | `CreateDisciplinaryActionRequest` | `ApiResponse<WfpDisciplinaryActionDetailResponse>` (201 Created) |
| `PUT` | `api/v1/corporate/human-resource/workforce-profiles/{id}/disciplinary-actions/{actionId}` | Memperbarui status pelaksanaan pembinaan disiplin | `WorkforceDisciplinaryAction:Update` | `UpdateDisciplinaryActionRequest` | `ApiResponse<WfpDisciplinaryActionDetailResponse>` |

---

## 4. Bukti implementasi kode

1. **Controller Hubungan Karyawan & Tindakan Disiplin**:
   - Berkas: [WfpDisciplinaryActionController.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/EmployeeRelationManagement/Controllers/WfpDisciplinaryActionController.cs)
   - Berkas master data: [DisciplinaryActionTypeController.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Repositories/Configurations/Corporate/HumanResource/MasterData/EmployeeRelation/Controllers/DisciplinaryActionTypeController.cs), [ViolationTypeController.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Repositories/Configurations/Corporate/HumanResource/MasterData/EmployeeRelation/Controllers/ViolationTypeController.cs), [SanctionTypeController.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Repositories/Configurations/Corporate/HumanResource/MasterData/EmployeeRelation/Controllers/SanctionTypeController.cs), [EmployeeRelationCaseTypeController.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Repositories/Configurations/Corporate/HumanResource/MasterData/EmployeeRelation/Controllers/EmployeeRelationCaseTypeController.cs).

2. **DTOs & Validasi**:
   - Berkas: [WfpDisciplinaryActionDtos.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/EmployeeRelationManagement/DTOs/WfpDisciplinaryActionDtos.cs)

---

## 5. Ringkasan Kepatuhan & Status

* **Status Task**: Selesai (`PASS`) ✅
* **Acceptance Criteria 1**: Kasus kedisiplinan hanya dapat dibaca peran yang berwenang (`WorkforceDisciplinaryAction:Read`) -> **LULUS**.
* **Acceptance Criteria 2**: Setiap tindakan menyimpan alasan, pelaku, dan waktu kejadian -> **LULUS**.
* **Acceptance Criteria 3**: Kasus yang sudah ditutup tidak dapat diubah diam-diam -> **LULUS**.
