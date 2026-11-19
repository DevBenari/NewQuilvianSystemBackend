# Kiosk — Kontrak API

| Field | Nilai |
| --- | --- |
| Set kontrak | `KSK-CONTRACT-v1` |
| `last_changed_in` | `v1` |
| Status | `approved` |
| Owner | Sukma Giri Pratama |
| `approved_by` / `approved_at` | Sukma Giri Pratama / 2026-09-30 |
| `input_revision` | `00-interview-decisions.md` r2; `02-backend-architecture.md` r1 |
| Dampak kompatibilitas | **Aditif** untuk endpoint baru. **Melonggarkan** validasi `POST patient-encounters/kiosk` (`paymentType = 3` kini diterima); pemanggil lama yang hanya mengirim 1/2 tidak terdampak. |
| Traceability | KSK-RM-001..009, KSK-GUA-001/002, SEC-KSK-001..006, `KSK-DEC-006/011/013/016/017/018/019`, `KSK-DSN-001..006` |

Konvensi respons seluruh endpoint: `ApiResponse<T>` dengan bentuk `{ "success", "statusCode", "message", "data" }`. Enum dikirim sebagai **angka**.

---

### Health Services / Registration Management / Kiosk Patient Lookup

Base URL: `api/v1/health-services/registration-management/kiosk-patient-lookups`
Contract version: `v1` — status `draft`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Mencari satu pasien dari No. KTP, No. HP, nomor kartu asuransi, atau nomor member, lalu menjawab ditemukan / tidak ditemukan / cocok ganda / hubungi petugas | Policy `KioskRead` (akun perangkat Kiosk); rate limit `KioskPatientLookup` 10/menit/perangkat | `KioskPatientLookupRequest` | `ApiResponse<KioskPatientLookupResponse>` | **Tersedia** sejak `BE-KSK-001` (30 Sep 2026); rate limit **Tersedia** sejak `BE-KSK-002` (30 Sep 2026) |

#### `KioskPatientLookupRequest`

| Field | Tipe | Wajib | Batas | Keterangan |
| --- | --- | :---: | --- | --- |
| `searchType` | `int` (`KioskPatientLookupSearchType`) | Ya | `1` KTP, `2` HP, `3` kartu asuransi, `4` nomor member | Layar Cek No. RM hanya mengirim `1` atau `2` |
| `value` | `string` | Ya | Maks. 32 karakter; tanpa karakter kontrol, `<`, `>` | **Sensitif.** Dinormalkan di server (lihat `validation-matrix.md`) |

Contoh (data samaran):

```json
{ "searchType": 2, "value": "0812-3456-7890" }
```

#### `KioskPatientLookupResponse`

| Field | Tipe | Keterangan |
| --- | --- | --- |
| `result` | `int` (`KioskPatientLookupResult`) | `1` Found, `2` NotFound, `3` MultipleMatch, `4` ContactStaff |
| `nextAction` | `string` | `EXISTING_PATIENT_REGISTRATION` (Found), `NEW_PATIENT_REGISTRATION` (NotFound), `USE_IDENTITY_NUMBER_OR_CONTACT_STAFF` (MultipleMatch via HP), `CONTACT_STAFF` (MultipleMatch via KTP/kartu, ContactStaff) |
| `patient` | `KioskPatientCardResponse` atau `null` | Terisi **hanya** saat `result = 1` |

#### `KioskPatientCardResponse`

| Field | Tipe | Keterangan |
| --- | --- | --- |
| `patientId` | `Guid` | Dipakai layar untuk meneruskan pasien ke Pendaftaran Pasien Lama (`KSK-DEC-012`) |
| `medicalRecordNumber` | `string` | Contoh `00-00-12-34` |
| `patientCode` | `string` | Contoh `PAT-RSMMC-00012` |
| `fullName` | `string` | Nama lengkap, tidak dipotong (KSK-BASE-001) |
| `patientTypeName` | `string` | Label tipe pasien |
| `genderName` | `string?` | Label jenis kelamin |
| `bloodTypeName` | `string?` | Label golongan darah (bagian Kartu Pasien existing) |

Tidak ada KTP, HP, alamat, tanggal lahir, atau data medis dalam respons ini (SEC-KSK-004).

Contoh respons ditemukan (data samaran):

```json
{
  "success": true,
  "statusCode": 200,
  "message": "Pasien ditemukan.",
  "data": {
    "result": 1,
    "nextAction": "EXISTING_PATIENT_REGISTRATION",
    "patient": {
      "patientId": "3f1c…",
      "medicalRecordNumber": "00-00-12-34",
      "patientCode": "PAT-RSMMC-00012",
      "fullName": "Budi Santoso Wirya Atmaja",
      "patientTypeName": "Umum",
      "genderName": "Laki-laki",
      "bloodTypeName": "O"
    }
  }
}
```

Contoh respons cocok ganda:

```json
{ "success": true, "statusCode": 200, "message": "Data perlu diverifikasi.",
  "data": { "result": 3, "nextAction": "USE_IDENTITY_NUMBER_OR_CONTACT_STAFF", "patient": null } }
```

#### Kode status

| Kode | Arti bagi pengguna Kiosk | Perilaku layar |
| --- | --- | --- |
| `200` | Pemeriksaan selesai; baca `result` | Tampilkan hasil sesuai `result` |
| `400` | Isian tidak sah (misalnya KTP bukan 16 digit) | Pesan validasi; **tidak** dianggap belum terdaftar |
| `401` | Sesi perangkat Kiosk habis | Kembali ke login perangkat |
| `403` | Akun bukan perangkat Kiosk | Pesan "Perangkat tidak berwenang" |
| `429` | Terlalu banyak percobaan dari perangkat ini | "Terlalu banyak percobaan. Silakan coba lagi sebentar." — **bukan** belum terdaftar |
| `500` / timeout / jaringan | Pemeriksaan gagal | "Data pasien belum dapat diperiksa. Silakan coba kembali." + Coba Lagi — **bukan** belum terdaftar |

---

### Health Services / Registration Management / Patient Encounter

Base URL: `api/v1/health-services/registration-management/patient-encounters`
Contract version: `v1` — status `draft`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/kiosk` (dan alias `/`) | Membuat kunjungan dari Kiosk dengan satu sumber pembayaran: Tunai, Asuransi, **atau Penjamin Perusahaan** | Policy `KioskRead`; `[AccessAction("Create", ...)]` existing (fallback kompatibilitas, sudah tercatat) | `PatientEncounterCreateRequest` (existing) | `ApiResponse<PatientEncounterCreateResponse>` (existing) | **Tersedia** sejak `BE-KSK-003` (30 Sep 2026): `paymentType = 3` diterima dengan validasi yang sama dengan `/admin` |
| `POST` | `/admin` | Jalur petugas | `PatientEncounter : Create` | sama | sama | Sudah ada — kontrak tidak berubah; defect simpan snapshot tanggal (`500`) diperbaiki `BE-KSK-003` (30 Sep 2026) |

Perubahan pada `PatientEncounterCreateRequest` untuk route kiosk: **tidak ada field baru**. Kombinasi yang kini sah:

| `paymentType` | Wajib | Harus kosong |
| --- | --- | --- |
| `1` Tunai | `paymentMethodId` | `patientInsuranceId`, `patientCompanyGuarantorId` |
| `2` Asuransi | `patientInsuranceId` | `patientCompanyGuarantorId` |
| `3` Penjamin Perusahaan | `patientCompanyGuarantorId` (relasi pasien–perusahaan aktif, eligible, dalam masa berlaku) | `patientInsuranceId` |

| Kode | Arti |
| --- | --- |
| `200` | Kunjungan dan sumber pembayarannya tersimpan |
| `400` | Kombinasi tidak konsisten, atau relasi perusahaan/asuransi tidak aktif/tidak eligible/di luar masa berlaku (pesan existing `LoadValidPatientCompanyGuarantorAsync`) |
| `401` / `403` | Seperti di atas |
| `500` | Gagal tersimpan; tidak ada yang tersimpan separuh |

---

### Endpoint existing yang dipakai tanpa perubahan

| Tag | Method | Path | Dipakai untuk |
| --- | --- | --- | --- |
| Health Services / Registration Management / Kiosk Scan Session | `POST` | `kiosk-scan-sessions/kiosk/scan-result` | Membentuk sesi kiosk **sekali di Step 3** (`KSK-DEC-014`); poliklinik tanpa `targetService`, Laboratorium dengan `targetService = 2` + `hasPhysicianRequest` (`KSK-DSN-007`) |
| Health Services / Patient Management / Patient | `GET` | `patients/kiosk/{id}` | Mengambil detail pasien untuk Step 1 terisi dan Review Data |
| Health Services / Patient Management / Patient | `GET` | `patients/kiosk?search=` | Pencarian nama / No. RM di Step 1 saja (bukan KTP/HP, `KSK-DEC-016`) |
| Patient Insurance / Patient Company Guarantor | `GET` | `patient-insurances/kiosk/options`, `patient-company-guarantors/kiosk/options` | Daftar kandidat penjamin untuk Kondisi A/B/C |
| Doctor Schedule | `GET` | `doctor-schedules/kiosk/...` | Cek Jadwal Dokter — tidak berubah |

### Endpoint existing yang berhenti dipakai Kiosk

| Method | Path | Alasan |
| --- | --- | --- |
| `PATCH` | `patient-insurances/kiosk/{id}/primary`, `patient-company-guarantors/kiosk/{id}/primary` | Mengubah penjamin utama level pasien; bertentangan dengan `KSK-DEC-008`. Endpoint **tidak dihapus** (`KSK-GAP-011`). |


---

## Amendment PO — Pendaftaran Pasien Online dan Check-in Kiosk

| Field | Nilai |
| --- | --- |
| Set kontrak | `KSK-CONTRACT-v2` |
| `last_changed_in` | `v2` (bagian ini saja; bagian di atas tetap `v1`) |
| Status | `approved` |
| Owner | Sukma Giri Pratama |
| `approved_by` / `approved_at` | Sukma Giri Pratama / 2026-11-19 (`KSK-DEC-054`) |
| `input_revision` | `00-interview-decisions.md` §13 (`KSK-DEC-026` s.d. `053`); `02-backend-architecture.md` *Amendment PO*; impact scan `CAP-PO-01` s.d. `37` |
| Dampak kompatibilitas | **Aditif**: tiga grup endpoint baru dan dua field opsional baru pada `POST patient-encounters/kiosk`. **Perubahan perilaku tanpa perubahan bentuk**: hitungan kuota (`doctor-schedule-quotas`, opsi jadwal kiosk, validasi kunjungan) kini ikut menghitung pendaftaran online Menunggu Check-in; kunjungan Rawat Jalan menutup pendaftaran online pasien pada tanggal yang sama |
| Traceability | AC 1–20 §13.9; `KSK-DEC-029` s.d. `053`; `KSK-DSN-PO-001` s.d. `017` |

Konvensi respons: `ApiResponse<T>`. Enum dikirim sebagai angka. Tanggal tanpa jam memakai `yyyy-MM-dd` dan dibaca sebagai kalender WIB.

### Health Services / Registration Management / Online Registration Public

Base URL: `api/v1/health-services/registration-management/online-registrations/public`
Contract version: `v2` — status `approved`. Tanpa login. Seluruh respons `Cache-Control: no-store`.

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/options/clinics` | Poli yang punya jadwal online pada tanggal itu | Tanpa login, `OnlineRegistrationPublicOptions` | Query `visitDate` | `ApiResponse<List<OnlineRegistrationClinicOption>>` (`id`, `name`) | **Rencana (belum tersedia)** |
| `GET` | `/options/doctor-schedules` | Jadwal dokter beserta sisa kuota appointment | Tanpa login, `OnlineRegistrationPublicOptions` | Query `clinicId`, `visitDate` | `ApiResponse<List<OnlineRegistrationScheduleOption>>` (`id`, `doctorName`, `startTime`, `endTime`, `sessionName`, `remainingAppointmentQuota`, `isFull`) | **Rencana (belum tersedia)** |
| `GET` | `/options/insurance-providers` | Penyedia asuransi untuk kartu baru | Tanpa login, `OnlineRegistrationPublicOptions` | — | `ApiResponse<List<OnlineRegistrationInsuranceProviderOption>>` (`id`, `name`) | **Rencana (belum tersedia)** |
| `GET` | `/options/referral-institutions` | Cari faskes perujuk | Tanpa login, `OnlineRegistrationPublicOptions` | Query `search` (min. 3 karakter) | `ApiResponse<List<OnlineRegistrationReferralInstitutionOption>>` (`id`, `code`, `name`), maks. 20 baris | **Rencana (belum tersedia)** |
| `POST` | `/patient-match` | Mencocokkan pasien lama | Tanpa login, `OnlineRegistrationPublicMatch`, captcha | `OnlineRegistrationPatientMatchRequest` | `ApiResponse<OnlineRegistrationPatientMatchResponse>` (`maskedName`, `payers[]`: `id`, `payerKind`, `providerName`, `maskedNumber`) | **Rencana (belum tersedia)** |
| `POST` | `` | Mengirim pendaftaran online | Tanpa login, `OnlineRegistrationPublicSubmit`, captcha | `multipart/form-data`: `payload` (JSON `OnlineRegistrationSubmitRequest`), `referralDocuments` (0–3 berkas) | `ApiResponse<OnlineRegistrationSubmitResponse>` (`registrationNumber`, `code`, `visitDate`, `clinicName`, `doctorName`, `startTime`, `endTime`, `emailStatus`, `whatsAppNumber`) | **Rencana (belum tersedia)** |

Contoh `OnlineRegistrationSubmitRequest` (data contoh, bukan data asli):

```json
{
  "patientMode": 2,
  "newPatient": { "identityNumber": "3174000000000001", "fullName": "Contoh Pasien", "birthDate": "1990-09-12", "gender": 2 },
  "contact": "contoh@contoh.id",
  "patientType": 3,
  "paymentType": 1,
  "visitDate": "2026-11-25",
  "clinicId": "…",
  "doctorScheduleId": "…",
  "referral": { "referralInstitutionId": "…", "referralDate": "2026-11-20", "referralNumber": "R-001", "referralDoctorName": "dr. Contoh", "diagnosisNote": null, "reason": null },
  "consent": { "signerType": 1, "signerName": "Contoh Pasien", "agreedClauses": [1, 2, 3], "signatureImageBase64": "data:image/png;base64,…" },
  "turnstileToken": "…"
}
```

Pasien lama mengirim `"patientMode": 1` dan `"existingPatient": { "identityType": 1, "identityValue": "00123456", "birthDate": "1990-09-12" }` (1 = NIK, 2 = No. RM) sebagai ganti `newPatient`.

Arti kode status bagi pasien:

| Kode | Artinya |
| --- | --- |
| `200` | Berhasil |
| `400` | Isian tidak sah, captcha gagal, tanggal di luar H+1 s.d. H+30, berkas terlalu besar |
| `404` | Data pasien tidak cocok (pesan seragam, tidak menyebut bagian yang salah) |
| `409` | Kuota habis, sudah ada pendaftaran atau kunjungan pada tanggal itu, jadwal tidak lagi tersedia |
| `429` | Terlalu banyak percobaan dari jaringan ini, coba lagi nanti |
| `503` | Layanan captcha tidak dapat dihubungi; coba lagi atau daftar di loket |

### Health Services / Registration Management / Online Registration Check-In

Base URL: `api/v1/health-services/registration-management/online-registrations/kiosk`
Contract version: `v2` — status `approved`.

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/check-in/verify` | Memeriksa kode atau identitas + tanggal lahir, lalu menerbitkan token check-in 15 menit | `KioskRead`, `OnlineRegistrationCheckIn` per akun kiosk | `OnlineRegistrationCheckInVerifyRequest` (`mode` 1 = kode, 2 = identitas; `code`; `identityType`; `identityValue`; `birthDate`) | `ApiResponse<OnlineRegistrationCheckInPackage>` | **Rencana (belum tersedia)** |

Arti kode status bagi pasien di kiosk:

| Kode | Artinya |
| --- | --- |
| `200` | Pendaftaran ditemukan, lanjutkan check-in |
| `400` | Isian tidak lengkap |
| `404` | Kode atau tanggal lahir tidak sesuai, atau pendaftaran sudah tidak berlaku (pesan seragam) |
| `409` | Belum tanggal kunjungan. Pesan menyebut tanggal kunjungan, karena pemohon sudah membuktikan kode dan tanggal lahir |
| `429` | Terlalu banyak percobaan di kiosk ini |

### Health Services / Registration Management / Patient Encounter (diperluas)

Base URL: `api/v1/health-services/registration-management/patient-encounters`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/kiosk` | Membuat kunjungan. Bila `onlineRegistrationId` dan `onlineRegistrationCheckInToken` diisi, kunjungan menjadi check-in pendaftaran online | `KioskRead` | `PatientEncounterCreateRequest` + `onlineRegistrationId` (`Guid?`) + `onlineRegistrationCheckInToken` (`string?`, maks. 64) | `ApiResponse<PatientEncounterCreateResponse>` (tidak berubah) | **Diperbarui (rencana)** |
| `POST` | `` (petugas) | Sama seperti sebelumnya. `onlineRegistrationId` ditolak `400` | `PatientEncounter : Create` | Tidak berubah | Tidak berubah | **Diperbarui (rencana)** |
| `GET` | `/doctor-schedule-quotas` | Sisa kuota kini mengurangi pendaftaran online Menunggu Check-in | `PatientEncounter : Read` | Tidak berubah | Tidak berubah | **Perilaku diperbarui (rencana)** |

Kode status tambahan untuk `POST /kiosk` dengan pendaftaran online: `400` token atau isian tidak sesuai pendaftaran; `404` pendaftaran tidak berlaku; `409` jadwal sudah tidak berlaku, kuota habis, atau data diubah bersamaan.

### Health Services / Registration Management / Online Registration

Base URL: `api/v1/health-services/registration-management/online-registrations`
Contract version: `v2` — status `approved`.

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `` | Daftar pendaftaran online | `OnlineRegistration : Read` | `OnlineRegistrationListQuery` (`visitDateFrom`, `visitDateTo`, `status`, `search` min. 3, `pageNumber`, `pageSize` = 25) | `ApiResponse<PagedResult<OnlineRegistrationListItemResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/summary` | Jumlah per status pada rentang tanggal | `OnlineRegistration : Read` | Query `visitDateFrom`, `visitDateTo` | `ApiResponse<OnlineRegistrationSummaryResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Detail beserta riwayat | `OnlineRegistration : Read` | — | `ApiResponse<OnlineRegistrationDetailResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/referral-documents/{documentId}` | Membuka berkas surat rujukan | `OnlineRegistration : Read` | — | Berkas (`application/pdf`, `image/jpeg`, `image/png`) | **Rencana (belum tersedia)** |
| `PUT` | `/{id}/schedule` | Mengubah tanggal dan/atau jadwal dokter di poli yang sama | `OnlineRegistration : Update` | `OnlineRegistrationScheduleChangeRequest` (`visitDate`, `doctorScheduleId`, `expectedRowVersion`) | `ApiResponse<OnlineRegistrationDetailResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}/contact` | Mengubah No. WA atau email | `OnlineRegistration : Update` | `OnlineRegistrationContactChangeRequest` (`contact`, `expectedRowVersion`) | `ApiResponse<OnlineRegistrationDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/cancel` | Membatalkan dengan alasan | `OnlineRegistration : Cancel` | `OnlineRegistrationCancelRequest` (`reason` 5–500, `expectedRowVersion`) | `ApiResponse<OnlineRegistrationDetailResponse>` | **Rencana (belum tersedia)** |

`OnlineRegistrationListItemResponse`: `id`, `registrationNumber`, `status`, `statusName`, `visitDate`, `clinicName`, `doctorName`, `startTime`, `patientMode`, `patientName`, `medicalRecordNumber` (kosong untuk pasien baru sebelum check-in), `identityNumber`, `patientTypeName`, `paymentTypeName`, `contact`, `emailStatus`, `submittedAt`, `encounterNumber`, `canChangeSchedule`, `canChangeContact`, `canCancel`, `rowVersion`.

`OnlineRegistrationDetailResponse`: seluruh field item daftar ditambah `birthDate`, `gender`, penjamin (`payerName`, `maskedCardNumber`, `newInsurance`), rujukan (`referralInstitutionName`, `referralNumber`, `referralDate`, `referralDoctorName`, `diagnosisNote`, `reason`, `documents[]`: `id`, `originalFileName`, `contentType`, `sizeBytes`), `hasConsent`, `consentSignedAt`, `checkedInAt`, `closedAt`, `cancelReason`, `histories[]` (`eventCode`, `eventName`, `fromStatus`, `toStatus`, `sourceName`, `actorName`, `reason`, `fromVisitDate`, `toVisitDate`, `fromScheduleLabel`, `toScheduleLabel`, `fromContact`, `toContact`, `occurredAt`). Kode **tidak pernah** dikembalikan.

`OnlineRegistrationSummaryResponse`: `awaitingCheckIn`, `checkedIn`, `registeredElsewhere`, `noShow`, `cancelled`, `total`.

Arti kode status bagi petugas: `400` isian tidak sah; `403` tidak punya hak akses; `404` pendaftaran tidak ditemukan; `409` status sudah bukan Menunggu Check-in, kuota jadwal baru habis, pasien sudah punya pendaftaran pada tanggal baru, atau data diubah pihak lain (muat ulang).
