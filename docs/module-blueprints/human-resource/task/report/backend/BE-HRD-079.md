# Laporan Perubahan Backend — `BE-HRD-079`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `BE-HRD-079` |
| Judul | Penilaian Masa Percobaan `T15` dan Kelulusan Otomatis (`IHrdProbationService`, `HrdProbationController`) |
| Slice | S-C4-ONB (LifecycleManagement) |
| Milestone | MVP-17 |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` |
| Traceability | HRD-DEC-083; HRD-TF-055; FR-ONB-035, FR-ONB-053; VR-ONB-55, VR-ONB-56, VR-ONB-57, VR-ONB-58, VR-ONB-59; INT-HR-30 |
| Dependency | BE-HRD-078 ✅ |
| Klasifikasi | `CORE PROBATION ASSESSMENT & AUTOMATIC PASSING` |
| Task Mode | `BACKEND` |
| Tanggal | 04 November 2026 |
| Status | Selesai (`PASS`) |

---

## 1. Ringkasan dan Masalah yang Diselesaikan

Masa percobaan (*probation*) merupakan tahapan kritis bagi tenaga kerja baru di rumah sakit (dokter umum, perawat klinis, apoteker, staf rekam medis, analis lab, staf administrasi) untuk dievaluasi kompetensi kerja, integritas, dan adaptasinya terhadap standar pelayanan rumah sakit. 

Sebelum task ini diselesaikan, sistem belum memiliki mekanisme terstruktur untuk:
1. **Pengajuan Hasil Evaluasi oleh Atasan Langsung**: Atasan (Kepala Ruangan/Kepala Instalasi) mengajukan rekomendasi kelulusan (`Pass`) atau ketidaklulusan (`Fail`) disertai bukti pertimbangan yang sah.
2. **Pemberlakuan Invariant Regulasi & Ketentuan RS**:
   - `VR-ONB-55`: Pengajuan evaluasi hanya sah saat status masa percobaan sedang berjalan (`Running`) dan sebelum atau tepat pada tanggal akhir masa percobaan (`EndDate`).
   - `VR-ONB-56`: Penilaian Tidak Lulus (`Fail`) **wajib menyertakan berkas dokumen pendukung** (lembar evaluasi kompetensi, catatan pelanggaran etika/SOP, atau berkas penilaian objektif atasan) sebagai perlindungan hukum rumah sakit.
   - `VR-ONB-57`: Hanya 1 penilaian terbuka (`Submitted` atau `Returned`) yang diperbolehkan dalam satu waktu per masa percobaan guna mencegah duplikasi proses persetujuan.
   - `VR-ONB-58`: Persetujuan keputusan penilaian sesudah tanggal akhir masa percobaan terlampaui **wajib ditolak** oleh sistem (`400 BadRequest`), karena status pegawai secara hukum ketenagakerjaan telah beralih otomatis menjadi pegawai definitif.
   - `VR-ONB-59`: **Dilarang keras menyediakan opsi atau endpoint perpanjangan masa percobaan** sesuai UU No. 13/2003 Pasal 60 (masa percobaan PKWT/PKWTT paling lama 3 bulan dan tidak boleh diperpanjang).
3. **Penyelarasan Alur Kerja Workflow T15 & Serah Terima Pemisahan Sub-Modul 20**:
   - Jika penilaian **Lulus (`Pass`)** disetujui HRD, masa percobaan ditutup dengan status `Passed`.
   - Jika penilaian **Tidak Lulus (`Fail`)** disetujui HRD, masa percobaan ditutup dengan status `Failed`, dan sistem secara transaksional menyerahkan pegawai ke Sub-Modul 20 (`TrxEmployeeSeparation` bertipe `ProbationFailed` dan draf `TrxExitClearance` dibuka melalui `BE-HRD-078`).
4. **Kelulusan Otomatis Lewat Scheduler Harian (`PassedAutomatically`)**:
   - Jika masa percobaan melewati tanggal akhir (`EndDate < today`) tanpa ada keputusan persetujuan yang diterbitkan HRD, maka sistem secara otomatis menetapkan status pegawai menjadi Lulus Otomatis (`PassedAutomatically`).
   - Seluruh pengajuan penilaian yang masih terbuka atau menggantung otomatis ditutup dengan status kedaluwarsa (`ClosedOnExpiry`).

---

## 2. Alur Proses Bisnis Rumah Sakit yang Didukung

```mermaid
flowchart TD
    subgraph PeriodeProbation["1. Monitoring Masa Percobaan"]
        A["Pegawai Baru Menjalani Masa Percobaan (Running)"] --> B{"Apakah Masuk Periode Reminder (H-14 s.d. H-0)?"}
        B -->|Ya| C["Muncul di Lifecycle Worklist Atasan & HRD (ProbationDue)"]
    end

    subgraph PengajuanPenilaian["2. Pengajuan Penilaian oleh Atasan (T15)"]
        C --> D["Atasan Input Hasil: Lulus (Pass) atau Tidak Lulus (Fail)"]
        D --> E{"Apakah Hasil Penilaian = Fail?"}
        E -->|Ya (VR-ONB-56)| F["Wajib Upload Berkas Bukti Evaluasi Kinerja"]
        E -->|Tidak| G["Upload Bukti Bersifat Opsional"]
        F --> H["Submit Penilaian Masa Percobaan (POST /assessments)"]
        G --> H
        H --> I["Status Probation: AwaitingDecision; Penilaian: Submitted; Instance T15 Terbentuk"]
    end

    subgraph PersetujuanHRD["3. Verifikasi & Keputusan HRD"]
        I --> J{"Pemeriksaan Batas Waktu (VR-ONB-58)"}
        J -->|Sudah Lewat EndDate| K["Persetujuan Ditolak Sistem (400 BadRequest)"]
        J -->|Sebelum/Pada EndDate| L{"Keputusan HRD via Workflow / Kotak Masuk"}
        L -->|Setuju Pass| M["Probation Status: Passed (Ditutup Resmi)"]
        L -->|Setuju Fail| N["Probation Status: Failed (Ditutup Resmi)"]
        N --> O["Handoff ke Sub-Modul 20: TrxEmployeeSeparation (ProbationFailed) & Exit Clearance"]
        L -->|Minta Revisi / Tolak| P["Penilaian: Rejected / Returned; Probation Kembali ke Running"]
    end

    subgraph SchedulerOtomatis["4. Scheduler Harian Jam 01:00 WIB"]
        A --> Q{"EndDate Terlampaui & Tanpa Keputusan Disetujui?"}
        Q -->|Ya| R["Probation Status: PassedAutomatically"]
        R --> S["Penilaian Terbuka Ditutup: ClosedOnExpiry"]
    end
```

---

## 3. Komponen Perubahan yang Diterapkan

### A. Data Transfer Objects (`HrdProbationDtos.cs`)
1. **`HrdProbationResponse`**: Diperluas dengan daftar penilaian riwayat `List<ProbationAssessmentResponse> Assessments`, `WorkforceCode`, `FullName`, `ContractNumber`, `ReminderFromDate`, `ClosedAt`, `ClosedByUserId`, `EmployeeSeparationId`, dan `RowVersion`.
2. **`HrdProbationQuery`**: Menambahkan filter pencarian `OrganizationUnitId`, `DueOnly`, `EndDateFrom`, `EndDateTo`, `Status`, dan pencarian teks `Search`.
3. **`ProbationAssessmentResponse`**: Model data penilaian memuat `AssessmentNumber`, `Result`, `ReasonText`, `EvidenceFileName`, `AssessmentStatus`, `SubmittedByUserId`, `SubmittedByUserName`, `SubmittedAt`, `WorkflowInstanceId`, `DecidedByUserId`, `DecidedByUserName`, `DecidedAt`, `DecisionNote`, dan `RowVersion`.
4. **`SubmitProbationAssessmentRequest`**: Form input multipart untuk hasil penilaian (`Result`), uraian alasan pertimbangan (`ReasonText`), unggah berkas pendukung (`EvidenceFile`), dan `ProbationRowVersion`.
5. **`UpdateProbationAssessmentRequest`**: Form input pembaruan penilaian dikembalikan (`Returned`) dengan validasi `RowVersion`.
6. **`WithdrawAssessmentRequest`**: Model penarikan pengajuan dengan catatan alasan penarikan (`Reason`).

### B. Interface & Service Layer (`IHrdProbationService.cs` & `HrdProbationService.cs`)
1. **`SubmitAssessmentAsync`**:
   - Memvalidasi status masa percobaan (`VR-ONB-55`).
   - Memvalidasi batas waktu pengajuan sebelum `EndDate` (`VR-ONB-55`).
   - Memeriksa keunikan penilaian terbuka (`VR-ONB-57`).
   - Memvalidasi kewajiban lampiran berkas bukti jika hasil adalah `Fail` (`VR-ONB-56`).
   - Mengalokasikan nomor penilaian unik via `IHrdNumberSeriesService`.
   - Mengunggah berkas lampiran via `IHrdLifecycleAttachmentService`.
   - Mengubah status probation ke `AwaitingDecision` dan memulai alur workflow `T15`.
2. **`UpdateAssessmentAsync`**:
   - Hanya mengizinkan pengubahan pada penilaian berstatus `Returned`.
   - Memvalidasi batas waktu dan memperbarui dokumen lampiran jika diunggah ulang.
3. **`WithdrawAssessmentAsync`**:
   - Mengizinkan penarikan penilaian sebelum diputus (`Submitted` atau `Returned`), mengembalikan status probation ke `Running`.
4. **`DownloadEvidenceAsync`**:
   - Menyediakan aliran stream berkas bukti pendukung secara aman dengan verifikasi hak akses.
5. **`ApplyAssessmentDecisionAsync`**:
   - Memeriksa batas waktu persetujuan `EndDate` (`VR-ONB-58`).
   - Menutup status probation (`Passed` atau `Failed`).
   - Mengintegrasikan pemisahan kerja ke Sub-Modul 20 via `ResignationLifecycleHandoffService.HandoffLifecycleSeparationAsync` saat status `Fail`.

### C. Controller Layer (`HrdProbationController.cs`)
- Mengimplementasikan seluruh endpoint RESTful canonical berdasar spesifikasi Swagger dengan rute canonical `api/v1/corporate/human-resource/lifecycle-management/probations` dan alias `api/v1/corporate/human-resource/probations`.
- Tag Swagger: `Corporate / Human Resource / Lifecycle Management / Probation`.
- **Tidak ada endpoint perpanjangan masa percobaan** (`VR-ONB-59`).

### D. Workflow Integration (`HrdEmploymentLifecycleWorkflowIntegrationService.cs`)
- Memperbarui event penanganan penyelesaian workflow `HandleWorkflowCompletedAsync` untuk alur `T15`.
- Melakukan verifikasi `VR-ONB-58`, penutupan masa percobaan, dan penyerahan tiket pemisahan Sub-Modul 20 saat `Fail`.

### E. Scheduler Harian (`HrdEmploymentLifecycleSchedulerService.cs`)
- Memperbarui evaluasi langkah 3 harian: mendeteksi masa percobaan aktif/menunggu keputusan yang melewati `EndDate`.
- Menetapkan status `PassedAutomatically`, serta menutup seluruh penilaian terbuka yang tersisa dengan status `ClosedOnExpiry`.

---

## 4. Spesifikasi Endpoint API (Gaya Swagger)

Tag Grup: `[Tags("Corporate / Human Resource / Lifecycle Management / Probation")]`

| Method | Path | Deskripsi | Hak Akses / Otorisasi | Request Body / Parameters | Response Contract | Status Code |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/corporate/human-resource/lifecycle-management/probations` | Mengambil daftar masa percobaan terpaginasi | `[Authorize]` | Query: `HrdProbationQuery` (`status`, `organizationUnitId`, `dueOnly`, `endDateFrom`, `endDateTo`, `search`, `pageNumber`, `pageSize`) | `RecruitmentResult<PagedResult<HrdProbationResponse>>` | `200 OK` |
| `GET` | `/api/v1/corporate/human-resource/lifecycle-management/probations/{id}` | Mengambil detail lengkap masa percobaan beserta seluruh riwayat penilaiannya | `[Authorize]` | Path: `id` (Guid) | `RecruitmentResult<HrdProbationResponse>` | `200 OK`<br>`404 NotFound` |
| `POST` | `/api/v1/corporate/human-resource/lifecycle-management/probations/{id}/assessments` | Mengajukan hasil penilaian (Lulus/Tidak Lulus) oleh atasan dan memicu workflow T15 | `[Authorize]` | Path: `id` (Guid)<br>Form-Data: `result`, `reasonText`, `evidenceFile` (wajib bila Fail), `probationRowVersion` | `RecruitmentResult<ProbationAssessmentResponse>` | `201 Created`<br>`400 BadRequest`<br>`404 NotFound`<br>`409 Conflict` |
| `PUT` | `/api/v1/corporate/human-resource/lifecycle-management/probations/{id}/assessments/{assessmentId}` | Memperbarui penilaian yang dikembalikan (Returned/Revisi) dan mengajukan ulang | `[Authorize]` | Path: `id`, `assessmentId` (Guid)<br>Form-Data: `result`, `reasonText`, `evidenceFile`, `rowVersion` | `RecruitmentResult<ProbationAssessmentResponse>` | `200 OK`<br>`400 BadRequest`<br>`404 NotFound`<br>`409 Conflict` |
| `POST` | `/api/v1/corporate/human-resource/lifecycle-management/probations/{id}/assessments/{assessmentId}/withdraw` | Menarik pengajuan penilaian sebelum diputus final | `[Authorize]` | Path: `id`, `assessmentId` (Guid)<br>JSON: `WithdrawAssessmentRequest` (`reason`) | `RecruitmentResult<ProbationAssessmentResponse>` | `200 OK`<br>`400 BadRequest`<br>`404 NotFound` |
| `GET` | `/api/v1/corporate/human-resource/lifecycle-management/probations/{id}/assessments/{assessmentId}/evidence` | Mengunduh berkas fisik bukti pendukung penilaian kinerja | `[Authorize]` | Path: `id`, `assessmentId` (Guid) | Berkas biner (`Stream` fisik file) | `200 OK`<br>`404 NotFound` |

---

## 5. Bukti Verifikasi dan Pengujian

- **Kompilasi Roslyn**: Terverifikasi bersih melalui `dotnet build NewQuilvianSystemBackend/QuilvianSystemBackend.csproj /t:Compile` dengan **0 Error(s)**.
- **Kepatuhan Invariant Bisnis**:
  - `VR-ONB-55` diuji pada `SubmitAssessmentAsync` dan `UpdateAssessmentAsync`.
  - `VR-ONB-56` diuji: Penolakan langsung (`400 BadRequest`) jika `result == Fail` tanpa lampiran berkas bukti.
  - `VR-ONB-57` diuji: Penolakan konflik (`409 Conflict`) jika sudah ada penilaian berstatus `Submitted` atau `Returned`.
  - `VR-ONB-58` diuji: Penolakan langsung (`400 BadRequest`) pada persetujuan keputusan sesudah `EndDate`.
  - `VR-ONB-59` dipatuhi secara ketat: Tidak ada endpoint perpanjangan masa percobaan pada controller maupun service.
- **Integrasi Sub-Modul 20**: Panggilan method `HandoffLifecycleSeparationAsync` teruji terhubung saat penetapan hasil `Fail`.
- **Scheduler Harian**: Evaluasi otomatis step 3 teruji menutup masa percobaan kedaluwarsa (`PassedAutomatically`) dan menutup penilaian terbuka (`ClosedOnExpiry`).
