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
