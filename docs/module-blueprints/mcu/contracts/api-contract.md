# Kontrak API — Modul MCU

| Field | Nilai |
| --- | --- |
| Contract | `MCU-API-v1` |
| contract_version | `v1` — status `approved` |
| Owner | Yoga Aji Pratama |
| approved_by / approved_at | Yoga Aji Pratama / 2026-10-21 |
| input_revision | Decision log r5; `02-backend-architecture.md` r1; `MCU-HDA-001` r1; `MCU-EVD-002` |
| Dampak kompatibilitas | Seluruh endpoint baru. Tidak ada endpoint existing yang berubah bentuk; dua service existing diperluas di sisi internal (Registrasi, Lab/Rad) |

**Semua endpoint di dokumen ini berstatus Rencana (belum tersedia).** Base URL bersama:
`api/v1/health-services/medical-checkup-management`. Seluruh respons dibungkus `ApiResponse<T>`;
daftar memakai `PagedResult<T>`. Sesuai keputusan pemilik, controller **tidak** memakai `<summary>` XML
pada action, dan `filters/metadata` serta `summary` dideklarasikan tepat sesudah konstruktor.

**Kode status umum** — berlaku untuk seluruh grup:

| Kode | Arti bagi pengguna |
| --- | --- |
| `200` | Berhasil |
| `400` | Isian tidak lengkap atau tidak sah; pesan menyebut isian yang salah (lihat `validation-matrix.md`) |
| `403` | Pengguna tidak punya hak akses, **atau** bukan dokter MCU yang ditetapkan untuk order ini |
| `404` | Data tidak ditemukan |
| `409` | Keadaan data tidak mengizinkan tindakan — status tidak sesuai, kuota penuh, belum lunas, diubah petugas lain (`Version` berbeda) |

---

### Health Services / Medical Checkup Management / Mcu Package

Base URL: `.../packages`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Metadata penyaring | `McuPackage : Read` | - | `FilterMetadataResponse` | **Rencana (belum tersedia)** |
| `GET` | `/` | Daftar paket | `McuPackage : Read` | `McuPackagePagedQuery` (search, isActive) | `PagedResult<McuPackageListResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/options` | Pilihan paket aktif untuk booking | `McuPackage : Read` | `?companyGuarantorId&date` | `List<McuPackageOptionResponse>` (harga yang berlaku) | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Detail paket + komponen + persyaratan + harga perusahaan | `McuPackage : Read` | - | `McuPackageDetailResponse` | **Rencana (belum tersedia)** |
| `POST` | `/` | Buat paket | `McuPackage : Create` | `CreateMcuPackageRequest` | `McuPackageDetailResponse` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Ubah paket dan isinya | `McuPackage : Update` | `UpdateMcuPackageRequest` | `McuPackageDetailResponse` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/status` | Aktifkan/nonaktifkan | `McuPackage : Update` | `McuPackageStatusRequest` | `object` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/company-prices` | Tambah harga perusahaan | `McuPackage : Update` | `CreateMcuCompanyPriceRequest` | `McuCompanyPriceResponse` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/company-prices/{priceId}/end` | Akhiri masa berlaku harga perusahaan | `McuPackage : Update` | `EndMcuCompanyPriceRequest` | `object` | **Rencana (belum tersedia)** |

### Health Services / Medical Checkup Management / Mcu Reference Data

Base URL: `...` (dua sumber daya)

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/examination-types` | Daftar jenis Pemeriksaan MCU | `McuReferenceData : Read` | `?search&isActive` | `PagedResult<McuExaminationTypeResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/examination-types/{id}` | Detail jenis + isian | `McuReferenceData : Read` | - | `McuExaminationTypeDetailResponse` | **Rencana (belum tersedia)** |
| `POST` | `/examination-types` | Buat jenis + isian | `McuReferenceData : Create` | `CreateMcuExaminationTypeRequest` | `McuExaminationTypeDetailResponse` | **Rencana (belum tersedia)** |
| `PUT` | `/examination-types/{id}` | Ubah jenis + isian | `McuReferenceData : Update` | `UpdateMcuExaminationTypeRequest` | `McuExaminationTypeDetailResponse` | **Rencana (belum tersedia)** |
| `GET` | `/fitness-categories` | Daftar kategori kelayakan | `McuReferenceData : Read` | `?isActive` | `List<McuFitnessCategoryResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/fitness-categories` | Buat kategori | `McuReferenceData : Create` | `CreateMcuFitnessCategoryRequest` | `McuFitnessCategoryResponse` | **Rencana (belum tersedia)** |
| `PUT` | `/fitness-categories/{id}` | Ubah kategori | `McuReferenceData : Update` | `UpdateMcuFitnessCategoryRequest` | `McuFitnessCategoryResponse` | **Rencana (belum tersedia)** |

### Health Services / Medical Checkup Management / Mcu Daily Schedule

Base URL: `.../daily-schedules`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Jadwal + sisa kuota + dokter jaga per tanggal | `McuSchedule : Read` | `?from&to` | `List<McuDailyScheduleResponse>` (memuat `dutyMcuDoctorId`, `dutyMcuDoctorName`) | **Rencana (belum tersedia)** |
| `PUT` | `/` | Atur hari buka/kuota dan dokter MCU jaga untuk satu rentang tanggal | `McuSchedule : Update` | `UpsertMcuDailyScheduleRangeRequest` (from, to, isOpen, quota, dutyMcuDoctorId?) | `List<McuDailyScheduleResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{scheduleDate}/quota-overrides` | Setujui pelampauan kuota | `McuSchedule : ApproveQuotaOverride` | `CreateMcuQuotaOverrideRequest` | `McuDailyScheduleResponse` | **Rencana (belum tersedia)** |

### Health Services / Medical Checkup Management / Mcu Order

Base URL: `.../orders`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Metadata penyaring Daftar Pasien MCU | `McuOrder : Read` | - | `FilterMetadataResponse` | **Rencana (belum tersedia)** |
| `GET` | `/summary` | Ringkasan jumlah per status untuk tanggal | `McuOrder : Read` | `?date` | `McuOrderSummaryResponse` | **Rencana (belum tersedia)** |
| `GET` | `/` | **Daftar Pasien MCU** | `McuOrder : Read` | `McuOrderPagedQuery` (date, status, source, companyGuarantorId, mcuDoctorId, search) | `PagedResult<McuOrderListResponse>` (termasuk *n dari m komponen tuntas*) | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Detail order | `McuOrder : Read` | - | `McuOrderDetailResponse` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/history` | Riwayat status & dokter | `McuOrder : Read` | - | `List<McuOrderHistoryResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/bookings` | Booking (telepon/WhatsApp) | `McuOrder : Create` | `CreateMcuBookingRequest` (packageId, examinationDate, patientId **atau** identitas sementara, companyGuarantorId?) | `McuOrderDetailResponse` | **Rencana (belum tersedia)** |
| `POST` | `/recommendations` | Rekomendasi dokter RJ/RI/IGD → order *Terjadwal* | `McuOrder : Recommend` | `CreateMcuRecommendationRequest` (sourceEncounterId, packageId, examinationDate) | `McuOrderDetailResponse` | **Rencana (belum tersedia)** |
| `POST` | `/walk-ins` | Pendaftaran *walk-in* di admisi (langsung check-in) | `McuOrder : Create` + `McuOrder : CheckIn` | `CreateMcuWalkInRequest` | `McuOrderDetailResponse` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/reschedule` | Jadwal ulang order *Terjadwal*/*Tidak Hadir* | `McuOrder : Update` | `RescheduleMcuOrderRequest` (newDate, version) | `McuOrderDetailResponse` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/cancel` | Batal sebelum datang **atau** batal penuh sesudah check-in | `McuOrder : Cancel` | `CancelMcuOrderRequest` (reason, version) | `McuOrderDetailResponse` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/arrival-confirmation` | Konfirmasi datang (dipakai kanal WhatsApp kelak) | `McuOrder : Update` | `version` | `object` | **Rencana (belum tersedia)** — kelak |

### Health Services / Medical Checkup Management / Mcu Booking Import

Base URL: `.../booking-imports`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/template` | Unduh templat daftar peserta | `McuOrder : Import` | - | berkas `.xlsx` | **Rencana (belum tersedia)** |
| `POST` | `/` | Unggah daftar rombongan | `McuOrder : Import` | `multipart/form-data`: berkas + `companyGuarantorId`, `packageId`, `examinationDate` | `McuBookingImportResponse` (baris berhasil/gagal) | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Hasil impor | `McuOrder : Read` | - | `McuBookingImportResponse` | **Rencana (belum tersedia)** |

### Health Services / Medical Checkup Management / Mcu Check In

Base URL: `.../orders/{id}`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/patient-matches` | Calon pasien cocok (NIK + tanggal lahir) | `McuOrder : CheckIn` | - | `McuPatientMatchResponse` (cocok penuh / NIK sama tanggal lahir beda / tidak ada) | **Rencana (belum tersedia)** |
| `GET` | `/check-in/preview` | Persyaratan yang harus ditandai, harga yang akan dipakai, dokter jaga sebagai isian bawaan dokter MCU (`MCU-DEC-042`) | `McuOrder : CheckIn` | - | `McuCheckInPreviewResponse` | **Rencana (belum tersedia)** |
| `POST` | `/check-in` | Check-in | `McuOrder : CheckIn` | `McuCheckInRequest` (patientId, matchConfirmation?, mcuDoctorId, requirementOutcomes[], version) | `McuOrderDetailResponse` | **Rencana (belum tersedia)** |
| `PATCH` | `/mcu-doctor` | Ganti dokter MCU | `McuOrder : ReassignDoctor` | `ReassignMcuDoctorRequest` (doctorId, reason, version) | `McuOrderDetailResponse` | **Rencana (belum tersedia)** |

Pasien baru dibuat lebih dulu lewat endpoint Patient Management yang sudah ada
(`POST api/v1/health-services/patient-management/master-data/patients`), dengan isian awal dari identitas
sementara; MCU tidak membuat pasien sendiri.

### Health Services / Medical Checkup Management / Mcu Kiosk

Base URL: `.../kiosk` — kebijakan otorisasi kiosk yang sama dengan `POST patient-encounters/kiosk`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/orders/lookup` | Cari booking hari ini milik pasien yang teridentifikasi di kiosk | Kebijakan kiosk | `?patientId` | `List<McuKioskOrderResponse>` (tanpa NIK utuh) | **Rencana (belum tersedia)** |
| `POST` | `/orders/{id}/check-in` | Check-in booking dari kiosk | Kebijakan kiosk | `McuKioskCheckInRequest` | `McuKioskCheckInResponse` | **Rencana (belum tersedia)** |
| `POST` | `/walk-ins` | Pendaftaran MCU *walk-in* dari kiosk | Kebijakan kiosk | `McuKioskWalkInRequest` | `McuKioskCheckInResponse` | **Rencana (belum tersedia)** |

**Dokter MCU untuk kiosk — `MCU-DEC-042` (menutup `DES-MCU-001`, 2026-10-21).** Check-in dan *walk-in*
kiosk menetapkan dokter jaga tanggal itu (`McuDailySchedule.DutyMcuDoctorId`) sebagai dokter MCU pemeriksa.
Tanpa dokter jaga, kedua endpoint menolak `409` *"Dokter MCU hari ini belum ditetapkan. Silakan menuju
admisi MCU."* (`VAL-MCU-47`). Grup ini tetap di luar MVP (`EPIC MCU-08`, `04-prd-to-mvp.md` bagian 20)
karena juga menunggu perubahan modul kiosk (`MCU-COORD-006`).

### Health Services / Medical Checkup Management / Mcu Examination

Base URL: `.../orders/{id}`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/start-examination` | Cek lunas/terjamin, buat pesanan Lab/Rad bertanda paket | `McuExamination : Start` | `version` | `McuOrderDetailResponse` | **Rencana (belum tersedia)** |
| `GET` | `/components` | Komponen + status + tautan pesanan | `McuExamination : Read` | - | `List<McuOrderComponentResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/components/refresh` | Sinkron status dari Lab/Rad dan skrining | `McuExamination : Read` | - | `List<McuOrderComponentResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/components` | Tambah pemeriksaan di luar paket | `McuExamination : AddComponent` | `AddMcuComponentRequest` (componentType, procedureId/examinationTypeId, radModalityId?) | `McuOrderComponentResponse` | **Rencana (belum tersedia)** |
| `PATCH` | `/components/{componentId}/cancel` | Batalkan komponen (+refund) | `McuExamination : CancelComponent` | `CancelMcuComponentRequest` (reason, version) | `McuOrderComponentResponse` | **Rencana (belum tersedia)** |
| `PATCH` | `/components/{componentId}/reschedule` | Pindah tanggal komponen susulan | `McuExamination : Update` | `RescheduleMcuComponentRequest` | `McuOrderComponentResponse` | **Rencana (belum tersedia)** |
| `GET` | `/components/{componentId}/examination-result` | Isian + hasil Pemeriksaan MCU | `McuExamination : Read` | - | `McuExaminationResultResponse` | **Rencana (belum tersedia)** |
| `PUT` | `/components/{componentId}/examination-result` | Isi/ubah hasil Pemeriksaan MCU | `McuExamination : Update` | `SaveMcuExaminationResultRequest` (values[], conclusion, complete: bool) | `McuExaminationResultResponse` | **Rencana (belum tersedia)** |

### Health Services / Medical Checkup Management / Mcu Resume

Base URL: `.../orders/{id}/resumes`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Seluruh versi | `McuResume : Read` | - | `List<McuResumeResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/current` | Simpan draf resume berlaku | `McuResume : Update` | `SaveMcuResumeRequest` | `McuResumeResponse` | **Rencana (belum tersedia)** |
| `POST` | `/current/approve` | Approve dokter | `McuResume : Approve` | `version` | `McuResumeResponse` | **Rencana (belum tersedia)** |
| `POST` | `/current/return` | Kembalikan ke dokter (salah isi) | `McuResume : Verify` | `ReturnMcuResumeRequest` (reason) | `McuResumeResponse` | **Rencana (belum tersedia)** |
| `POST` | `/current/verify` | Verifikasi administratif | `McuResume : Verify` | `version` | `McuResumeResponse` | **Rencana (belum tersedia)** |
| `POST` | `/reopen` | Buka ulang (versi baru) | `McuResume : Reopen` | `ReopenMcuResumeRequest` (reason) | `McuResumeResponse` | **Rencana (belum tersedia)** |

### Health Services / Medical Checkup Management / Mcu Delivery

Base URL: `.../orders/{id}`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/result-document` | Data cetak hasil (`variant=full` / `fitness-summary`) | `McuDelivery : Read` | `?variant` | `McuResultDocumentResponse` / `McuFitnessSummaryDocumentResponse` | **Rencana (belum tersedia)** |
| `GET` | `/deliveries` | Riwayat penyerahan | `McuDelivery : Read` | - | `List<McuResultDeliveryResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/deliveries` | Catat penyerahan | `McuDelivery : Create` | `CreateMcuResultDeliveryRequest` | `McuResultDeliveryResponse` | **Rencana (belum tersedia)** |
| `PATCH` | `/deliveries/{deliveryId}/correction-notice` | Catat pemberitahuan koreksi kepada penerima versi lama | `McuDelivery : Create` | `RecordCorrectionNoticeRequest` | `McuResultDeliveryResponse` | **Rencana (belum tersedia)** |
| `GET` | `/release-consents` | Persetujuan pelepasan | `McuDelivery : Read` | - | `List<McuReleaseConsentResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/release-consents` | Catat persetujuan | `McuDelivery : ManageConsent` | `CreateMcuReleaseConsentRequest` | `McuReleaseConsentResponse` | **Rencana (belum tersedia)** |
| `PATCH` | `/release-consents/{consentId}/revoke` | Cabut persetujuan | `McuDelivery : ManageConsent` | `RevokeMcuReleaseConsentRequest` | `McuReleaseConsentResponse` | **Rencana (belum tersedia)** |

### Health Services / Medical Checkup Management / Mcu Medical Fee Source

Base URL: `.../medical-fee-sources` — **menunggu `MCU-COORD-010`**; bentuk final mengikuti kesepakatan dengan
pemilik Medical Fee.

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Komponen selesai bernilai jasa (komponen, dokter, nilai, waktu) | `McuMedicalFeeSource : Read` | `?releasedFrom&releasedTo&doctorId` | `PagedResult<McuMedicalFeeSourceResponse>` | **Rencana (belum tersedia)** |

---

## Perubahan internal pada modul lain (bukan endpoint baru)

| Modul | Perubahan | Kontrak publik berubah? |
| --- | --- | --- |
| Registrasi | `EncounterIntakeService.RegisterAsync` menerima jenis kunjungan `MedicalCheckup` dan klinik MCU (antrean & skrining dari master klinik) | Tidak |
| Laboratorium | Jalur internal membuat pesanan bertanda `CoveringMcuOrderId` + `InstructingDoctorId`; respons detail pesanan Lab boleh menampilkan *Dicakup paket MCU* | Hanya penambahan ruas baca |
| Radiologi | Sama dengan Laboratorium | Hanya penambahan ruas baca |
| Billing | Baris paket lewat `AddCatalogChargeAsync` yang ada. **Refund diperluas** (`MCU-DEC-043`): `POST invoices/{id}/refunds` memakai `RequestedAmount` ≤ sisa yang dapat direfund per baris, dan `refundable-items` menampilkan sisa per baris, bukan 0/penuh. Disetujui Yasmina, pemilik Billing, 2026-10-21 (`MCU-COORD-003`) | Ya — perilaku `RequestedAmount` berubah dari diabaikan menjadi dipakai; bentuk request tidak berubah |
