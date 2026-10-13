# Laporan Perubahan Backend — `BE-RWI-205`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-205` |
| Judul | Persistence Dokumen Persetujuan Rawat Inap, Citra TTD Digital Base64 & Invariant Penanda Tangan Bayi Baru Lahir |
| Slice | Slice 1 (Fondasi Kontrak & Validasi); Alur Admisi Pendaftaran |
| Roadmap | [`../../../roadmap/backend-roadmap-admisi-pendaftaran.md`](../../../roadmap/backend-roadmap-admisi-pendaftaran.md) — kartu `BE-RWI-205` |
| Trace | `FR-RI-184`, `FR-RI-185`, `FR-RI-186`; `RWI-DEC-272`, `RWI-DEC-273`; `RWI-AC-393`, `RWI-AC-394`, `RWI-AC-395`; Validation `VAL-ADM-04`, `VAL-ADM-05`; API Contract `0.11.0` |
| Contract version | `0.11.0` **`approved`** (10 Oktober 2026) |
| Dependency | `BE-RWI-204` |
| Klasifikasi | `HIGH` — API consent persistensi, penegakan invariant hukum medis bayi baru lahir, digital signature base64 |
| Task mode | `BACKEND` |
| Target tulis | `InpAdmissionDocumentType.cs`, `InpAdmissionDocumentRules.cs`, `InpatientAdmissionConsentDtos.cs`, `InpatientAdmissionConsentService.cs`, `InpatientAdmissionConsentController.cs`, `Program.cs` |
| Tanggal | 10 Oktober 2026 |
| Status | ✅ Selesai. Seluruh kriteria penerimaan `RWI-AC-393`, `RWI-AC-394`, dan `RWI-AC-395` terimplementasi bersih dan tervalidasi. |

---

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `InPatientManagement` (Admission Consent & Documents) |
| Prefix registry | `Inp` — `ACTIVE` |
| Keberlakuan | `EXPANDED ARCHITECTURE` |
| QBE yang berlaku | `QBE-DTO-001`, `QBE-VAL-001`, `QBE-SVC-001`, `QBE-API-001` |
| Wewenang | Source: ya. Tidak ada perubahan struktur skema fisik tabel database (memanfaatkan dokumen admisi ber-snapshot JSONB dan relasi party & signature). |

---

## 1. Kebutuhan Bisnis

Berdasarkan revisi Tim Analisis Bisnis (Mba Ilma) pada alur pendaftaran admisi rawat inap:
1. **Formulir Persetujuan Rawat Inap Langsung di Admisi (`RWI-DEC-272`, `RWI-AC-393`):** Sebelum lembar persetujuan dicetak, formulir persetujuan umum rawat inap wajib diisi langsung di langkah pendaftaran admisi, disertai pembubuhan tanda tangan digital pasien/penanggung jawab. Isian formulir dan citra TTD digital base64 disimpan permanen ke episode rawat inap.
2. **Invariant Penanda Tangan Bayi Baru Lahir (`RWI-DEC-273`, `RWI-AC-394`):** Menegakkan perlindungan hukum medis; pasien dengan kategori Bayi Baru Lahir (`BayiBaruLahir`, neonatus, atau terikat `MotherEpisodeId`) dilarang menandatangani sendiri (`Self`). Penanda tangan secara mutlak wajib Orang Tua atau Wali yang sah. Pelanggaran ditolak server dengan kode status HTTP 422 `BAYI_PENANDATANGAN_WAJIB_WALI`.
3. **Workspace PPRI Print-Ready Tanpa Re-Input (`RWI-AC-395`):** Menyediakan endpoint pembacaan `GET /api/v1/inpatient-admissions/{episodeId}/consent` sehingga Workspace PPRI dapat langsung mengambil formulir terisi dan tanda tangan digital untuk dicetak tanpa memerlukan penginputan ulang.

---

## 2. Rincian Perubahan Source Code

### 2.1 `Areas/HealthServices/InPatientManagement/Enums/InpAdmissionDocumentType.cs`
- Menambahkan anggota enum `GeneralConsent = 7` untuk jenis dokumen Persetujuan Rawat Inap / General Consent.

### 2.2 `Areas/HealthServices/InPatientManagement/Services/InpAdmissionDocumentRules.cs`
- Menambahkan penanganan `InpAdmissionDocumentType.GeneralConsent` pada aturan bisnis:
  - `IsShipped`: Terdaftar sebagai dokumen yang aktif didukung.
  - `HasParty`: Memiliki pihak penanda tangan (`InpAdmissionDocumentParty`).
  - `RequiredSlots`: Membutuhkan slot tanda tangan `PatientOrFamily` dan `AdmissionOfficer`.
  - `DocumentName`: `"Persetujuan Rawat Inap (General Consent)"`.

### 2.3 `Areas/HealthServices/InPatientManagement/DTOs/InpatientAdmissionConsentDtos.cs`
- Membuat DTO kontrak terkunci:
  - `InpatientAdmissionConsentCreateDto`: `PatientCategory`, `SignerType`, `SignerName` (min 3 kar, max 200 kar), `SignerRelationship`, `SignerRelationshipText`, `SignerPhoneNumber` (max 13 kar), `SignerIdentityNumber`, `AgreedClauses` (12 butir), `SignatureImageBase64`, dan `SignedAt`.
  - `InpatientAdmissionConsentResponse`: Metadata consent lengkap, nomor dokumen bisnis `GCR-...`, status `Signed`, dan citra tanda tangan digital base64.

### 2.4 `Areas/HealthServices/InPatientManagement/Services/InpatientAdmissionConsentService.cs`
- Menangani siklus penyimpanan `SaveConsentAsync` dan pembacaan `GetConsentAsync`:
  - **Validasi `VAL-ADM-04`:** Memverifikasi kelengkapan 12 butir klausul persetujuan, kehadiran tanda tangan digital base64, dan nama penanda tangan minimal 3 karakter. Jika tidak lengkap, kembalikan HTTP 400 Bad Request.
  - **Validasi `VAL-ADM-05`:** Memeriksa apakah pasien merupakan Bayi Baru Lahir (`PatientCategory`, `MotherEpisodeId`, atau `IsNewborn`). Jika ya dan `SignerType == "Self"` / `SignerRelationship == "Self"`, tolak dengan HTTP 422 Unprocessable Entity dan kode `BAYI_PENANDATANGAN_WAJIB_WALI`.
  - **Persistensi Data:** Menyimpan atau memperbarui baris `InpAdmissionDocument` berstatus `Completed` dengan `SnapshotJson` yang memuat seluruh payload persetujuan dan tanda tangan digital base64, serta mencatat `InpAdmissionDocumentParty` dan `InpAdmissionDocumentSignature`.
  - **Pembacaan:** Mengambil data persetujuan tersimpan beserta citra TTD digital base64 dari snapshot dokumen admisi episode.

### 2.5 `Areas/HealthServices/InPatientManagement/Controllers/InpatientAdmissionConsentController.cs`
- Mengimplementasikan endpoint API bergaya Swagger:
  - `POST /api/v1/inpatient-admissions/{episodeId}/consent`
  - `GET /api/v1/inpatient-admissions/{episodeId}/consent`
  - Alias rute canonical: `/api/v1/health-services/inpatient-management/episodes/{episodeId}/consent` dan `admission-consents`.
  - Tag: `[Tags("InpatientAdmissionConsent")]`
  - Hak akses: `InpatientEpisode : Create` untuk penyimpanan, `InpatientEpisode : Read` untuk pembacaan.

### 2.6 `Program.cs`
- Mendaftarkan dependensi `builder.Services.AddScoped<InpatientAdmissionConsentService>();` ke dalam kontainer Dependency Injection.

---

## 3. Bukti Verifikasi dan Acceptance Criteria

| ID Kriteria | Kriteria Penerimaan | Status | Bukti Implementasi & Hasil Pengujian |
|---|---|:---:|---|
| `RWI-AC-393` | Formulir persetujuan rawat inap & tanda tangan digital tersimpan permanen ke episode rawat inap | ✅ LULUS | Endpoint `POST /api/v1/inpatient-admissions/{episodeId}/consent` menerima payload persetujuan dan citra PNG base64, menyimpannya ke `InpAdmissionDocument` (Status `Completed`, Snapshot JSONB, Party, Signature). |
| `RWI-AC-394` | Pasien berkategori Bayi Baru Lahir menolak penanda tangan mandiri (`Self`) dengan HTTP 422 | ✅ LULUS | Penegakan invariant `VAL-ADM-05` di `InpatientAdmissionConsentService` memverifikasi `isBaby && isSelfSigner`. Jika melanggar, dikembalikan HTTP 422 dengan pesan `"Pasien bayi baru lahir tidak dapat menandatangani sendiri; penanda tangan wajib orang tua atau wali yang sah."` dan error code `"BAYI_PENANDATANGAN_WAJIB_WALI"`. |
| `RWI-AC-395` | Dokumen persetujuan rawat inap dapat dibaca dalam status siap cetak tanpa re-input | ✅ LULUS | Endpoint `GET /api/v1/inpatient-admissions/{episodeId}/consent` mengembalikan seluruh data persetujuan yang telah ditandatangani termasuk `signatureImageBase64`, `documentNumber`, dan data penanda tangan. |

---

## 4. Status Database dan Migration
- **Pemanfaatan Skema Existing:** Menggunakan tabel `InpAdmissionDocument`, `InpAdmissionDocumentParty`, dan `InpAdmissionDocumentSignature` yang sudah ada dari migrasi sebelumnya, dengan memanfaatkan kolom `SnapshotJson` (tipe PostgreSQL `jsonb`) dan enum `DocumentType = 7`.
- **Tidak Memerlukan Migrasi Tambahan:** Perubahan ini tidak memerlukan eksekusi `dotnet ef database update` atau pembuatan file migrasi baru.

---

## 5. Handoff ke Task Berikutnya
- Seluruh task backend untuk amandemen admisi pendaftaran (`BE-RWI-204` & `BE-RWI-205`) telah **SELESAI 100%**.
- Siap dilanjutkan ke eksekusi task frontend:
  1. **`FE-RWI-222`**: Input Masking No HP Kontak Darurat & Penyembunyian Dropdown Unit Ranap.
  2. **`FE-RWI-223`**: Langkah 1 Jenis Kunjungan (Umum & Rujukan Teks) & Langkah 2 Filter 3 Kategori Pasien Baru.
  3. **`FE-RWI-224`**: Langkah 1 Split Screen Pasien Lama & Langkah 2 Kategori Kunjungan.
  4. **`FE-RWI-225`**: Dedicated Step Formulir Persetujuan Rawat Inap & Kanvas TTD Digital.
  5. **`FE-RWI-226`**: Integrasi Citra TTD Digital pada Lembar Cetak & General Consent PPRI Print-Ready.
