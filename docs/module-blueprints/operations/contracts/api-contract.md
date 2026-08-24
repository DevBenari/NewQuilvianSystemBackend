# API Contract — Modul Operasi

Contract `opr-api-v1`; status `approved`; approved by pemilik kebutuhan pada 2026-08-21; owner API organisasi belum ditetapkan; input architecture revision 1. Semua endpoint di dokumen ini **Rencana (belum tersedia)**.

## Health Services / Operating Room Management / Cases

Base URL: `api/v1/health-services/operating-room-management/cases`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar kasus dengan filter/paging | `OperatingRoomCase : Read` | `OprCasePagedQuery` | `ApiResponse<PagedResult<OprCaseSummaryResponse>>` | Rencana |
| `GET` | `/{id}` | Workspace lengkap kasus | `OperatingRoomCase : Read` | - | `ApiResponse<OprCaseDetailResponse>` | Rencana |
| `POST` | `/` | Membuat permintaan operasi | `OperatingRoomCase : Create` | `CreateOprCaseRequest` | `ApiResponse<OprCaseDetailResponse>` | Rencana |
| `PUT` | `/{id}` | Memperbaiki data permintaan sebelum mulai | `OperatingRoomCase : Update` | `UpdateOprCaseRequest` + version | `ApiResponse<OprCaseDetailResponse>` | Rencana |
| `PATCH` | `/{id}/schedule` | Menetapkan/revisi jadwal dan tim | `OperatingRoomSchedule : Update` | `ScheduleOprCaseRequest` | `ApiResponse<OprScheduleResponse>` | Rencana |
| `PATCH` | `/{id}/postpone` | Menunda kasus | `OperatingRoomSchedule : Update` | `PostponeOprCaseRequest` | `ApiResponse<OprCaseStatusResponse>` | Rencana |
| `PATCH` | `/{id}/start` | Memulai operasi | `OperatingRoomExecution : Update` | `StartOprCaseRequest` | `ApiResponse<OprCaseStatusResponse>` | Rencana |
| `PATCH` | `/{id}/cancel` | Membatalkan sebelum mulai | `OperatingRoomCase : Cancel` | `CancelOprCaseRequest` | `ApiResponse<OprCaseStatusResponse>` | Rencana |

## Health Services / Operating Room Management / Preparation

Base URL: `api/v1/health-services/operating-room-management/cases/{caseId}/preparation`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Membaca consent, persiapan, checklist, sign-off | `OperatingRoomPreparation : Read` | - | `ApiResponse<OprPreparationResponse>` | Rencana |
| `PUT` | `/checklists/{phase}` | Menyimpan checklist fase | `OperatingRoomPreparation : Update` | `SaveOprChecklistRequest` | `ApiResponse<OprChecklistResponse>` | Rencana |
| `POST` | `/sign-offs` | Memberikan sign-off sesuai aktor | `OperatingRoomPreparation : Update` | `CreateOprReadinessSignOffRequest` | `ApiResponse<OprPreparationResponse>` | Rencana |
| `POST` | `/emergency-bypass` | Mencatat bypass darurat | `OperatingRoomPreparation : Update` | `CreateOprEmergencyBypassRequest` | `ApiResponse<OprPreparationResponse>` | Rencana |

## Health Services / Operating Room Management / Execution

Base URL: `api/v1/health-services/operating-room-management/cases/{caseId}/execution`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `PUT` | `/operation-record` | Menyimpan/finalisasi catatan operasi | `OperatingRoomExecution : Update` | `SaveOprExecutionRecordRequest` | `ApiResponse<OprExecutionRecordResponse>` | Rencana |
| `POST` | `/operation-record/addenda` | Menambah koreksi catatan final | `OperatingRoomExecution : Update` | `CreateOprExecutionAddendumRequest` | `ApiResponse<OprExecutionAddendumResponse>` | Rencana |
| `PUT` | `/anesthesia-record` | Menyimpan/finalisasi catatan anestesi | `OperatingRoomAnesthesia : Update` | `SaveOprAnesthesiaRecordRequest` | `ApiResponse<OprAnesthesiaRecordResponse>` | Rencana |
| `POST` | `/materials` | Mencatat pemakaian/retur/waste | `OperatingRoomMaterial : Update` | `CreateOprMaterialUsageRequest` | `ApiResponse<OprMaterialUsageResponse>` | Rencana |
| `PUT` | `/recovery` | Menyimpan pemantauan/keputusan recovery | `OperatingRoomAnesthesia : Update` | `SaveOprRecoveryRequest` | `ApiResponse<OprRecoveryResponse>` | Rencana |
| `POST` | `/handovers` | Mengirim handover | `OperatingRoomHandover : Update` | `CreateOprHandoverRequest` | `ApiResponse<OprHandoverResponse>` | Rencana |
| `PATCH` | `/handovers/{id}/accept` | Unit tujuan menerima handover | `OperatingRoomHandover : Update` | `AcceptOprHandoverRequest` | `ApiResponse<OprCaseStatusResponse>` | Rencana |

## Health Services / Operating Room Management / Reports

Base URL: `api/v1/health-services/operating-room-management/reports`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/operations` | Laporan kasus/tindakan/durasi/status | `OperatingRoomCase : Read` | `OprReportQuery` | `ApiResponse<PagedResult<OprOperationReportRow>>` | Rencana |
| `GET` | `/utilization` | Pemakaian ruang dan penundaan | `OperatingRoomCase : Read` | rentang waktu/ruang | `ApiResponse<OprUtilizationReport>` | Rencana |
| `GET` | `/materials` | Traceability material/implant | `OperatingRoomMaterial : Read` | filter item/batch/serial | `ApiResponse<PagedResult<OprMaterialReportRow>>` | Rencana |

Kode respons: `200/201` berhasil; `400` format/data kurang; `401` belum login; `403` tidak berwenang; `404` kasus tidak ditemukan; `409` benturan, transition ilegal, version stale, atau duplicate conflict; `422` aturan klinis/prasyarat belum terpenuhi; `500` kesalahan tak terduga dengan correlation ID aman.

Request command wajib membawa `idempotencyKey` dan `expectedVersion` bila mengubah aggregate. Response detail membawa `availableActions` agar frontend tidak menebak tindakan berikutnya.
