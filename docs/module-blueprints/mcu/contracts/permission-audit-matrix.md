# Matriks Hak Akses dan Audit — Modul MCU

| Field | Nilai |
| --- | --- |
| Contract | `MCU-PERM-v1` |
| contract_version | `v1` — status `approved` |
| Owner | Yoga Aji Pratama |
| approved_by / approved_at | Yoga Aji Pratama / 2026-10-21 |
| input_revision | `api-contract.md` `MCU-API-v1`; decision log r5 bagian 6 |

String di bawah disalin persis ke `[AccessPermission(...)]`. Kolom *Dicatat logger* mengikuti konvensi:
GET tidak dicatat. **Payload log hanya nomor order/ID, controller, action, status lama/baru, pelaku** —
tidak boleh memuat kolom bertanda Sensitif pada kamus data. Pemetaan resource/action ke departemen dan
posisi diatur lewat `SysAccessPolicies` (Role Access), bukan role Identity.

## 1. Resource dan action

| Resource | Action | Peran yang dituju (keputusan) |
| --- | --- | --- |
| `McuPackage` | `Read`, `Create`, `Update` | Admin MCU (`MCU-DEC-001`); `Read` juga admisi |
| `McuReferenceData` | `Read`, `Create`, `Update` | Admin MCU; `Read` dokter & petugas MCU |
| `McuSchedule` | `Read`, `Update` | Admin MCU / kepala unit |
| `McuSchedule` | `ApproveQuotaOverride` | **Kepala unit MCU** (`MCU-DEC-029`) |
| `McuOrder` | `Read` | Seluruh peran MCU |
| `McuOrder` | `Create`, `Update`, `Cancel`, `CheckIn`, `Import` | Petugas admisi MCU (`MCU-DEC-003`, `021`, `033`, `038`) |
| `McuOrder` | `Recommend` | Dokter RJ/RI/IGD (`MCU-DEC-006`, `017`) |
| `McuOrder` | `ReassignDoctor` | **Kepala unit MCU** (`MCU-DEC-040`) |
| `McuExamination` | `Read`, `Start`, `Update` | Petugas/dokter MCU |
| `McuExamination` | `AddComponent`, `CancelComponent` | **Dokter MCU yang ditetapkan** (`MCU-DEC-015`, `036`) |
| `McuResume` | `Read` | Peran MCU |
| `McuResume` | `Update`, `Approve`, `Reopen` | **Dokter MCU yang ditetapkan** (`MCU-DEC-008`, `011`, `032`) |
| `McuResume` | `Verify` | Petugas MCU (`MCU-DEC-008`) |
| `McuDelivery` | `Read`, `Create`, `ManageConsent` | Petugas MCU (`MCU-DEC-019`, `027`) |
| `McuMedicalFeeSource` | `Read` | Medical Fee / Keuangan — menunggu `MCU-COORD-010` |

**"Dokter MCU yang ditetapkan"** = hak akses resource **dan** akun pengguna tertaut ke `MstDoctor` yang
sama dengan `McuOrder.McuDoctorId`. Hak akses saja tidak cukup; tanpa kecocokan, respons `403`
*"Hanya dokter MCU yang ditetapkan untuk peserta ini yang dapat melakukan tindakan ini."*

## 2. Endpoint

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger |
| --- | --- | --- | --- | :---: |
| `GET packages/filters/metadata`, `GET packages`, `GET packages/options`, `GET packages/{id}` | `McuPackage` | `Read` | `[AccessPermission("McuPackage", "Read")]` | Tidak |
| `POST packages` | `McuPackage` | `Create` | `[AccessPermission("McuPackage", "Create")]` | Ya |
| `PUT packages/{id}`, `PATCH packages/{id}/status`, `POST packages/{id}/company-prices`, `PATCH .../company-prices/{priceId}/end` | `McuPackage` | `Update` | `[AccessPermission("McuPackage", "Update")]` | Ya |
| `GET examination-types`, `GET examination-types/{id}`, `GET fitness-categories` | `McuReferenceData` | `Read` | `[AccessPermission("McuReferenceData", "Read")]` | Tidak |
| `POST examination-types`, `POST fitness-categories` | `McuReferenceData` | `Create` | `[AccessPermission("McuReferenceData", "Create")]` | Ya |
| `PUT examination-types/{id}`, `PUT fitness-categories/{id}` | `McuReferenceData` | `Update` | `[AccessPermission("McuReferenceData", "Update")]` | Ya |
| `GET daily-schedules` | `McuSchedule` | `Read` | `[AccessPermission("McuSchedule", "Read")]` | Tidak |
| `PUT daily-schedules` | `McuSchedule` | `Update` | `[AccessPermission("McuSchedule", "Update")]` | Ya |
| `POST daily-schedules/{scheduleDate}/quota-overrides` | `McuSchedule` | `ApproveQuotaOverride` | `[AccessPermission("McuSchedule", "ApproveQuotaOverride")]` | Ya |
| `GET orders/filters/metadata`, `GET orders/summary`, `GET orders`, `GET orders/{id}`, `GET orders/{id}/history`, `GET booking-imports/{id}` | `McuOrder` | `Read` | `[AccessPermission("McuOrder", "Read")]` | Tidak |
| `POST orders/bookings`, `POST orders/walk-ins` | `McuOrder` | `Create` | `[AccessPermission("McuOrder", "Create")]` | Ya |
| `POST orders/recommendations` | `McuOrder` | `Recommend` | `[AccessPermission("McuOrder", "Recommend")]` | Ya |
| `PATCH orders/{id}/reschedule`, `PATCH orders/{id}/arrival-confirmation` | `McuOrder` | `Update` | `[AccessPermission("McuOrder", "Update")]` | Ya |
| `PATCH orders/{id}/cancel` | `McuOrder` | `Cancel` | `[AccessPermission("McuOrder", "Cancel")]` | Ya |
| `GET booking-imports/template`, `POST booking-imports` | `McuOrder` | `Import` | `[AccessPermission("McuOrder", "Import")]` | `POST` saja |
| `GET orders/{id}/patient-matches`, `GET orders/{id}/check-in/preview`, `POST orders/{id}/check-in` | `McuOrder` | `CheckIn` | `[AccessPermission("McuOrder", "CheckIn")]` | `POST` saja |
| `PATCH orders/{id}/mcu-doctor` | `McuOrder` | `ReassignDoctor` | `[AccessPermission("McuOrder", "ReassignDoctor")]` | Ya |
| `GET kiosk/orders/lookup`, `POST kiosk/orders/{id}/check-in`, `POST kiosk/walk-ins` | — | — | Kebijakan kiosk (`[Authorize(Policy = KioskReadPolicy)]`, sama dengan `PatientEncounterController`) | `POST` saja |
| `GET orders/{id}/components`, `POST .../components/refresh`, `GET .../examination-result` | `McuExamination` | `Read` | `[AccessPermission("McuExamination", "Read")]` | Tidak |
| `POST orders/{id}/start-examination` | `McuExamination` | `Start` | `[AccessPermission("McuExamination", "Start")]` | Ya |
| `PUT .../components/{componentId}/examination-result`, `PATCH .../components/{componentId}/reschedule` | `McuExamination` | `Update` | `[AccessPermission("McuExamination", "Update")]` | Ya |
| `POST orders/{id}/components` | `McuExamination` | `AddComponent` | `[AccessPermission("McuExamination", "AddComponent")]` | Ya |
| `PATCH .../components/{componentId}/cancel` | `McuExamination` | `CancelComponent` | `[AccessPermission("McuExamination", "CancelComponent")]` | Ya |
| `GET orders/{id}/resumes` | `McuResume` | `Read` | `[AccessPermission("McuResume", "Read")]` | Tidak |
| `PUT orders/{id}/resumes/current` | `McuResume` | `Update` | `[AccessPermission("McuResume", "Update")]` | Ya |
| `POST orders/{id}/resumes/current/approve` | `McuResume` | `Approve` | `[AccessPermission("McuResume", "Approve")]` | Ya |
| `POST orders/{id}/resumes/current/return`, `POST orders/{id}/resumes/current/verify` | `McuResume` | `Verify` | `[AccessPermission("McuResume", "Verify")]` | Ya |
| `POST orders/{id}/resumes/reopen` | `McuResume` | `Reopen` | `[AccessPermission("McuResume", "Reopen")]` | Ya |
| `GET orders/{id}/result-document`, `GET .../deliveries`, `GET .../release-consents` | `McuDelivery` | `Read` | `[AccessPermission("McuDelivery", "Read")]` | Tidak |
| `POST orders/{id}/deliveries`, `PATCH .../deliveries/{deliveryId}/correction-notice` | `McuDelivery` | `Create` | `[AccessPermission("McuDelivery", "Create")]` | Ya |
| `POST orders/{id}/release-consents`, `PATCH .../release-consents/{consentId}/revoke` | `McuDelivery` | `ManageConsent` | `[AccessPermission("McuDelivery", "ManageConsent")]` | Ya |
| `GET medical-fee-sources` | `McuMedicalFeeSource` | `Read` | `[AccessPermission("McuMedicalFeeSource", "Read")]` | Tidak |

## 3. Kejadian audit yang wajib

| Kejadian | Tempat jejak tahan lama | Isi log aplikasi (tanpa data sensitif) |
| --- | --- | --- |
| Setiap perpindahan status order | `McuOrderStatusHistory` | *"Order MCU-RSMMC-000123: Terjadwal → Hadir oleh {user}"* |
| Penetapan/penggantian dokter MCU | `McuOrderDoctorAssignment` | Nomor order, dokter lama/baru (nama dokter boleh) |
| Pencocokan pasien dikonfirmasi petugas | `McuOrder.PatientMatchMethod` + `PatientMatchConfirmedByUserId` | Nomor order, cara pencocokan — **tanpa NIK** |
| Pembatalan komponen | `McuOrderComponent` (alasan, dokter, refund) | Nomor order, nama komponen, nominal refund |
| Ubah isian sesudah buka ulang | `McuExaminationResultValueRevision` | Nomor order, jenis pemeriksaan — **tanpa nilai** |
| Approve / verifikasi / buka ulang resume | `McuResume` | Nomor order, versi, pelaku |
| Penyerahan | `McuResultDelivery` | Nomor order, saluran, jenis isi — **tanpa alamat/nomor tujuan** |
| Persetujuan pelepasan / pencabutan | `McuReleaseConsent` | Nomor order, jenis bukti |
| Pelampauan kuota | `McuQuotaOverride` | Tanggal, jumlah, pelaku |
| Impor rombongan | `McuBookingImport` | Nomor impor, jumlah baris berhasil/gagal |

## 4. Pemisahan tugas

| Aturan | Penegakan |
| --- | --- |
| Pemverifikasi resume ≠ dokter yang approve versi itu | Service menolak `403` bila akun pemverifikasi tertaut ke `ApprovedByDoctorId` |
| Petugas MCU tidak mengubah isi klinis resume | `Verify` tidak membawa ruas isi; `Update` hanya untuk dokter yang ditetapkan |
| Ringkasan perusahaan tanpa nilai/diagnosis | DTO ringkasan tidak memuat ruas klinis selain kategori kelayakan dan `WorkRecommendationText` |
