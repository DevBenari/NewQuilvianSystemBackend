# Laporan Perubahan Backend — `BE-HRD-005`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `BE-HRD-005` |
| Judul | Engine SLA, Eskalasi, dan Kotak Masuk Persetujuan Terpadu (Approval Inbox) |
| Slice | `S-A7` (Kotak Masuk Persetujuan Terpadu & SLA Workflow) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang 5 |
| Trace | `HRD-DEC-011`, `HRD-DEC-030`, `IN-18` |
| Contract version | API Contract v1 (Draft) |
| Dependency | `BE-HRD-004` (Selesai ✅) |
| Klasifikasi | `HIGH` |
| Task mode | `BACKEND` |
| Target verifikasi | `ApprovalInboxController.cs`, `ApprovalInboxService.cs`, `WorkflowService.cs`, `WorkflowReferenceLifecycleService.cs`, `ApprovalDelegationController.cs`, `Program.cs` |
| Model | Gemini 3.8 Flash (High) |
| Tanggal | 27 Oktober 2026 |
| Status | Selesai (`PASS`) |

---

## 1. Masalah yang diperbaiki & diverifikasi

1. **Kotak Masuk Terpadu Multi-Modul untuk Atasan Rumah Sakit**:
   Para kepala ruangan, dokter penanggung jawab, kepala instalasi, dan manajer SDM sebelumnya harus membuka berbagai menu terpisah untuk menyetujui pengajuan bawahan (cuti, lembur, tukar shift, koreksi absensi, dan perubahan profil). Melalui `ApprovalInboxController`, seluruh tugas persetujuan diagregasikan ke dalam satu antarmuka terpadu berbasis identitas atasan login.
2. **Pemantauan Batas Waktu Jatuh Tempo (*SLA DueAt Tracking*)**:
   Permohonan staf rumah sakit yang tertunda lama dapat mengganggu jadwal operasional pelayanan pasien. Sistem memantau status jatuh tempo (`DueStatus`: `overdue`, `dueToday`, `upcoming`) dan menyediakan mekanisme pendelegasian wewenang saat atasan berhalangan (`ApprovalDelegationController`).
3. **Penyelarasan Siklus Hidup Transaksi Atomik (*No Orphan Status*)**:
   Tindakan persetujuan (`Approve`), penolakan (`Reject`), atau permintaan perbaikan (`RequestRevision`) yang dieksekusi dari Approval Inbox disinkronisasikan secara atomik ke instance workflow (`TrxWorkflowInstance`) dan status entitas pemilik transaksi (`WfpLeaveRequest`, `TrxOvertimePlan`, `HrdAttendanceCorrectionRequest`, dsb.) melalui `WorkflowReferenceLifecycleService`.

---

## 2. Proses bisnis & alur kerja rumah sakit

```
[Staf Medis / Karyawan] ──► Mengajukan Permohonan (Cuti / Lembur / Shift / Presensi)
                                   │
                                   ▼
             [Sistem Menghasilkan Workflow Assignment Atasan]
                                   │
                                   ▼
[Kepala Ruangan / Supervisor Membuka Approval Inbox]
         │
         ├──► 1. Menarik Agregasi Permohonan: GET /approval-inbox/summary
         │       └─► Statistik: Perlu Tindakan, Jatuh Tempo (Overdue), Delegasi
         │
         ├──► 2. Meninjau Daftar Penugasan: GET /approval-inbox
         │       └─► Filter tipe permohonan, urgensi, dan tenggat waktu SLA
         │
         └──► 3. Eksekusi Tindakan Tunggal atau Batch:
                 ├─► Setuju: POST /approval-inbox/{id}/approve
                 ├─► Tolak: POST /approval-inbox/{id}/reject
                 ├─► Minta Revisi: POST /approval-inbox/{id}/request-revision
                 └─► Status transaksi pemilik otomatis tersinkronisasi 100%
```

* **Tujuan**: Mempercepat proses birokrasi persetujuan rumah sakit, mencegah keterlambatan penanganan kedaruratan jadwal kerja, serta memberikan visibilitas penuh bagi para pimpinan unit.
* **Pelaku**: Kepala Ruangan, Kepala Instalasi Medis, Direktur/Manajer HR, Atasan Delegasi.
* **Pemicu**: Masuknya pengajuan cuti, rencana lembur, tukar shift, atau koreksi kehadiran baru dari bawahan langsung.
* **Langkah & Aturan**:
  1. Penugasan persetujuan dipetakan berdasar matriks organisasi atau pendelegasian resmi yang aktif.
  2. SLA mengevaluasi `DueAt`; tugas yang melewati batas waktu ditandai `overdue` dan diprioritaskan di baris teratas.
  3. Tindakan persetujuan atau penolakan mengunci tugas dan memperbarui status dokumen asal secara langsung tanpa jeda inkonsistensi data.

---

## 3. Spesifikasi Endpoint API (Gaya Swagger)

### Grup: `Corporate / Human Resource / Workflow Management / Approval Inbox`
Tag: `[Tags("Corporate / Human Resource / Workflow Management / Approval Inbox")]`

| Method | Path | Deskripsi | Auth / Permission | Request Body / Query | Response Model |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `api/v1/corporate/human-resource/workflow/approval-inbox/filters/metadata` | Mengambil metadata opsi filter kotak masuk atasan | `ApprovalInbox:Read` | - | `ApiResponse<ApprovalInboxFilterMetadataResponse>` |
| `GET` | `api/v1/corporate/human-resource/workflow/approval-inbox/summary` | Mengambil statistik agregasi pengajuan yang menunggu persetujuan atasan | `ApprovalInbox:Read` | - | `ApiResponse<ApprovalInboxSummaryResponse>` |
| `GET` | `api/v1/corporate/human-resource/workflow/approval-inbox` | Mengambil daftar tugas persetujuan berhalaman | `ApprovalInbox:Read` | `ApprovalInboxPagedQueryRequest` | `ApiResponse<PagedResult<ApprovalInboxItemResponse>>` |
| `GET` | `api/v1/corporate/human-resource/workflow/approval-inbox/{assignmentId}` | Mengambil detail lengkap satu penugasan persetujuan | `ApprovalInbox:Read` | `assignmentId` (Guid) | `ApiResponse<ApprovalInboxDetailResponse>` |
| `POST` | `api/v1/corporate/human-resource/workflow/approval-inbox/{assignmentId}/approve` | Menyetujui permohonan bawahan | `ApprovalInbox:Approve` | `WorkflowApproveRequest` | `ApiResponse<WorkflowInstanceDetailResponse>` |
| `POST` | `api/v1/corporate/human-resource/workflow/approval-inbox/{assignmentId}/reject` | Menolak permohonan bawahan (Wajib catatan penolakan) | `ApprovalInbox:Reject` | `WorkflowRejectRequest` | `ApiResponse<WorkflowInstanceDetailResponse>` |
| `POST` | `api/v1/corporate/human-resource/workflow/approval-inbox/{assignmentId}/request-revision` | Mengembalikan permohonan ke staf untuk revisi | `ApprovalInbox:RequestRevision` | `WorkflowRequestRevisionRequest` | `ApiResponse<WorkflowInstanceDetailResponse>` |
| `POST` | `api/v1/corporate/human-resource/workflow/approval-inbox/batch/approve` | Menyetujui beberapa permohonan sekaligus secara batch | `ApprovalInbox:Approve` | `WorkflowBatchApproveRequest` | `ApiResponse<WorkflowBatchActionResponse>` |

---

## 4. Bukti implementasi kode

1. **Controller Kotak Masuk Persetujuan**:
   - Berkas: [ApprovalInboxController.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/WorkflowManagement/Controllers/ApprovalInboxController.cs)
   - Berkas: [ApprovalDelegationController.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/WorkflowManagement/Controllers/ApprovalDelegationController.cs)

2. **Domain Service Kotak Masuk & Penyelarasan Lifecycle**:
   - Berkas: [ApprovalInboxService.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/WorkflowManagement/Services/ApprovalInboxService.cs)
   - Berkas: [WorkflowReferenceLifecycleService.cs](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/Areas/Corporate/HumanResource/WorkflowManagement/Services/WorkflowReferenceLifecycleService.cs)

---

## 5. Ringkasan Kepatuhan & Status

* **Status Task**: Selesai (`PASS`) ✅
* **Acceptance Criteria 1**: `ApprovalInboxController` mengembalikan agregasi permohonan menunggu persetujuan per atasan -> **LULUS**.
* **Acceptance Criteria 2**: SLA engine mengecek `DueAt` dan memicu kategori status keterlambatan -> **LULUS**.
* **Acceptance Criteria 3**: Persetujuan dan penolakan memperbarui instance workflow dan status transaksi pemilik secara atomik -> **LULUS**.
