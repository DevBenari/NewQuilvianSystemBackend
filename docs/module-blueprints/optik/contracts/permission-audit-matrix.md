# Optik — Matriks Hak Akses dan Audit

| Field | Nilai |
| --- | --- |
| `contract_version` | `OPT-PERM-v1` — status `approved` (sisi Optik) |
| Owner | Yoga Aji Pratama; security: pemilik arsitektur backend |
| `approved_by` / `approved_at` | Yoga Aji Pratama / 2026-11-03 |
| Konstanta | `Areas/HealthServices/OpticalManagement/Constants/OpticalPermissions.cs`, `ModuleCode = "HEALTH_SERVICE_OPTICAL_MANAGEMENT"`, `ModuleName = "Health Service Optical Management"`, `AreaName = "HealthServices"` |

String disalin apa adanya ke `[AccessController(ControllerName = …)]`, `[AccessAction]`, dan
`[AccessPermission]`. Selisih satu huruf menghasilkan 403 permanen. Pemetaan ke departemen-jabatan
diatur lewat `SysAccessPolicies`. POST Role Access menimpa seluruh set, jadi baca dulu lalu kirim
ulang utuh.

## 1. Endpoint

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger |
| --- | --- | --- | --- | :---: |
| `GET visits/` | `OptVisit` | `Read` | `[AccessPermission("OptVisit", "Read")]` | Tidak |
| `GET visits/{encounterId}` | `OptVisit` | `Read` | `[AccessPermission("OptVisit", "Read")]` | Tidak |
| `POST visits/` | `OptVisit` | `Create` | `[AccessPermission("OptVisit", "Create")]` | Ya |
| `GET refraction-exams/`, `/{id}` | `OptRefractionExam` | `Read` | `[AccessPermission("OptRefractionExam", "Read")]` | Tidak |
| `POST refraction-exams/` | `OptRefractionExam` | `Create` | `[AccessPermission("OptRefractionExam", "Create")]` | Ya |
| `PUT refraction-exams/{id}/measurements` | `OptRefractionExam` | `Update` | `[AccessPermission("OptRefractionExam", "Update")]` | Ya |
| `POST refraction-exams/{id}/finalize` | `OptRefractionExam` | `Finalize` | `[AccessPermission("OptRefractionExam", "Finalize")]` | Ya |
| `POST refraction-exams/{id}/revisions` | `OptRefractionExam` | `Revise` | `[AccessPermission("OptRefractionExam", "Revise")]` | Ya |
| `POST refraction-exams/{id}/cancel` | `OptRefractionExam` | `Cancel` | `[AccessPermission("OptRefractionExam", "Cancel")]` | Ya |
| `GET prescriptions/`, `/{id}`, `/options` | `OptPrescription` | `Read` | `[AccessPermission("OptPrescription", "Read")]` | Tidak |
| `POST prescriptions/`, `/external` | `OptPrescription` | `Create` | `[AccessPermission("OptPrescription", "Create")]` | Ya |
| `POST prescriptions/{id}/revisions` | `OptPrescription` | `Revise` | `[AccessPermission("OptPrescription", "Revise")]` | Ya |
| (syarat tambahan di dalam create/revise) | `OptPrescription` | `OverrideRange` | `[AccessPermission("OptPrescription", "OverrideRange")]` — dicek service, bukan atribut endpoint | Ya |
| `GET orders/`, `/{id}`, `/{id}/payment-status` | `OptOrder` | `Read` | `[AccessPermission("OptOrder", "Read")]` | Tidak |
| `POST orders/` | `OptOrder` | `Create` | `[AccessPermission("OptOrder", "Create")]` | Ya |
| `PUT orders/{id}` | `OptOrder` | `Update` | `[AccessPermission("OptOrder", "Update")]` | Ya |
| `POST orders/{id}/bill` | `OptOrder` | `Bill` | `[AccessPermission("OptOrder", "Bill")]` | Ya |
| `POST orders/{id}/send-to-work` | `OptOrder` | `SendToWork` | `[AccessPermission("OptOrder", "SendToWork")]` | Ya |
| `POST orders/{id}/mark-ready` | `OptOrder` | `MarkReady` | `[AccessPermission("OptOrder", "MarkReady")]` | Ya |
| `POST orders/{id}/hand-over` | `OptOrder` | `HandOver` | `[AccessPermission("OptOrder", "HandOver")]` | Ya |
| `POST orders/{id}/cancel` | `OptOrder` | `Cancel` | `[AccessPermission("OptOrder", "Cancel")]` | Ya |
| `POST orders/{id}/cancellation-requests` | `OptOrder` | `RequestCancel` | `[AccessPermission("OptOrder", "RequestCancel")]` | Ya |
| `POST orders/{id}/cancellation-requests/{requestId}/decision` | `OptOrder` | `DecideCancel` | `[AccessPermission("OptOrder", "DecideCancel")]` | Ya |
| `POST orders/{id}/close-not-collected` | `OptOrder` | `CloseNotCollected` | `[AccessPermission("OptOrder", "CloseNotCollected")]` | Ya |
| `GET warranty-claims/`, `/{id}` | `OptWarrantyClaim` | `Read` | `[AccessPermission("OptWarrantyClaim", "Read")]` | Tidak |
| `POST warranty-claims/` | `OptWarrantyClaim` | `Create` | `[AccessPermission("OptWarrantyClaim", "Create")]` | Ya |
| `POST warranty-claims/{id}/examination` | `OptWarrantyClaim` | `Examine` | `[AccessPermission("OptWarrantyClaim", "Examine")]` | Ya |
| `POST warranty-claims/{id}/decision` | `OptWarrantyClaim` | `Decide` | `[AccessPermission("OptWarrantyClaim", "Decide")]` | Ya |
| `GET reference-data/metadata`, `/item-catalog`, `/settings` | `OptReferenceData` | `Read` | `[AccessPermission("OptReferenceData", "Read")]` | Tidak |
| `PUT reference-data/item-profiles/{drugId}` | `OptReferenceData` | `Update` | `[AccessPermission("OptReferenceData", "Update")]` | Ya |
| `PUT reference-data/settings` | `OptReferenceData` | `UpdateSetting` | `[AccessPermission("OptReferenceData", "UpdateSetting")]` | Ya |
| `GET reports/*` | `OptReport` | `Read` | `[AccessPermission("OptReport", "Read")]` | Tidak |

## 2. Pemetaan peran usulan

Peran berasal dari keputusan pemilik (`OPT-DEC-005`, `010`, `015`, `018`, `027`, `040`). Jabatan
nyata di HR belum diperiksa (`OPT-CAP-07`). Selain izin, service menegakkan profesi RO/Sp.M dan izin
praktik berlaku.

| Resource : Action | RO | Sp.M | Petugas penjualan | Kepala Optik | Admin Optik | Admisi |
| --- | :---: | :---: | :---: | :---: | :---: | :---: |
| `OptVisit : Read` | ✔ | ✔ | ✔ | ✔ | — | ✔ |
| `OptVisit : Create` | — | — | ✔ | ✔ | — | ✔ |
| `OptRefractionExam : Read` | ✔ | ✔ | ✔ | ✔ | — | — |
| `OptRefractionExam : Create/Update/Finalize/Revise/Cancel` | ✔ | ✔ | ✘ | — | — | — |
| `OptPrescription : Read` | ✔ | ✔ | ✔ | ✔ | — | — |
| `OptPrescription : Create/Revise` | ✔ | ✔ | ✘ | — | — | — |
| `OptPrescription : OverrideRange` | ✘ | ✔ | ✘ | ✘ | ✘ | ✘ |
| `OptOrder : Read` | ✔ | ✔ | ✔ | ✔ | — | — |
| `OptOrder : Create/Update/Bill/SendToWork/MarkReady/HandOver/Cancel/RequestCancel` | — | — | ✔ | ✔ | — | — |
| `OptOrder : DecideCancel/CloseNotCollected` | ✘ | ✘ | ✘ | ✔ | — | — |
| `OptWarrantyClaim : Read/Create` | ✔ | ✔ | ✔ | ✔ | — | — |
| `OptWarrantyClaim : Examine` | ✔ | ✔ | ✘ | — | — | — |
| `OptWarrantyClaim : Decide` | ✘ | ✘ | ✘ | ✔ | — | — |
| `OptReferenceData : Read` | ✔ | ✔ | ✔ | ✔ | ✔ | — |
| `OptReferenceData : Update/UpdateSetting` | — | — | — | — | ✔ | — |
| `OptReport : Read` | — | — | — | ✔ | ✔ | — |
| `BillingDiscount : Create` (Billing) | — | — | ✔ | ✔ | — | — |

"—" = tidak diatur keputusan; bawaan tidak diberikan (*fail-closed*).

## 3. Audit

| Kejadian | Jejak tahan lama | Logger (teks pesan) |
| --- | --- | --- |
| Pemeriksaan final/revisi | `OptRefractionExamRevision` (pelaku, waktu, alasan) + snapshot izin | "OPT-EXM-… difinalkan oleh {aktor}" |
| Resep dibuat/disalin/direvisi/lewat rentang | `OptPrescriptionVersion` | "OPT-RX-… versi {n} oleh {aktor}" |
| Transisi pesanan | `OptOrderStatusHistory` | "OPT-ORD-… {dari} → {ke} oleh {aktor}" |
| Pengajuan/keputusan batal | `OptOrderCancellation` | "Batal OPT-ORD-… {disetujui/ditolak} oleh {aktor}" |
| Keluhan | `OptWarrantyClaim` | "OPT-CLM-… {status} oleh {aktor}" |
| Pengaturan | `OptSettingRevision` | "Pengaturan Optik diubah oleh {aktor}" |

Payload logger **tidak boleh** memuat kolom bertanda Sensitif (nilai ukur, resep, keluhan, temuan,
nama pengambil). `LoggerService.AuditAsync` mengabaikan objek `data`, jadi isi pesan ditulis di teks
pesan. Ruas `Id`/`Name`/`Path` di payload jangan dipakai karena menimpa pelaku.
