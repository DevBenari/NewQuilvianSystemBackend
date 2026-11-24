# Laporan Perubahan Backend — `BE-HRD-399`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-399` |
| Judul | Profesi dan spesialisasi untuk orang dengan peran jamak |
| Slice | `S-FIN-M04` / Data Profesi (alias `FIN-HRD-04-BE-013`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.2, kartu `BE-HRD-399` |
| Trace | `HRD-DEC-051`; instruksi Menu 04 `FIN-HRD-04-BE-013`; `UAT-04-37` s.d. `40` |
| Contract version | Kontrak Data Induk Dokter existing |
| Dependency | `BE-HRD-388` ✅ |
| Klasifikasi | `LIGHT` |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | `MasterData/Workforce/Controllers/DoctorController.cs`; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d17aa187` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 2/2 acceptance criteria terpetakan ke source; build dan QBE lulus; uji API `NOT RUN` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `MasterData/Workforce` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | QBE-VAL-001 |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Profesi (Pegawai, Pengguna Eksternal) dan profesi + spesialisasi (Dokter) sudah divalidasi aktif dan saling cocok. Audit menemukan bahwa Data Induk Dokter **menerima `CredentialingStatus` dan `ClinicalPrivilegeStatus` langsung dari form** pada create, `PUT`, dan `PATCH /status`. Kedua kolom itu adalah cermin keputusan alur kredensial (Menu 09), Komite Medik, rekredensial, proctoring, dan radar pelatihan (`TrainingExpiryRadarService` menangguhkan, `RecredentialingQualityService` mencabut). Contoh risikonya: kewenangan klinis dr. D dicabut Komite Medik (`Revoked`), lalu petugas Data Induk mengedit alamat dr. D dan form ikut mengirim status `Active` — kewenangannya aktif kembali tanpa keputusan apa pun.

## 2. Proses bisnis

1. Dokter didaftarkan di Data Induk dengan status awal: kredensial `NotStarted`/`Pending`, kewenangan klinis `NotApplicable`/`Pending`. Status lain saat create → `409 CREDENTIAL_STATUS_MANAGED_BY_CREDENTIALING`.
2. Status berubah hanya lewat alur kredensial (Menu 09) dan service Komite Medik/rekredensial/proctoring.
3. `PUT` atau `PATCH /status` Data Induk yang mengirim status berbeda dari yang tersimpan → `409`. Form yang mengirim nilai tersimpan apa adanya tetap lolos.
4. Profesi/spesialisasi tetap divalidasi aktif dan sesuai profesi; menetapkannya tidak membuat STR/SIP atau kewenangan klinis apa pun.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`DoctorController.cs` (create, `PUT`, `PATCH /status`, `ValidateDoctorMasterReferencesAsync`), `EmployeeController.cs`/`ExternalUserController.cs` (validasi profesi), penulis lain `ClinicalPrivilegeStatus` (`TrainingExpiryRadarService`, `ProctoringWorkflowService`, `RecredentialingQualityService`), enum `CredentialingStatus`/`ClinicalPrivilegeStatus`, frontend `doctor-grouped-form.config.jsx` (form mengirim nilai tersimpan).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `MasterData/Workforce/Controllers/DoctorController.cs` | `IsPreCredentialingState` pada create; penolakan perubahan status kredensial pada `PUT` dan `PATCH /status`; `PUT` tidak lagi menulis kedua kolom |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Route/DTO tetap; kode galat baru `CREDENTIAL_STATUS_MANAGED_BY_CREDENTIALING` (409) |
| Database | Tanpa migration |
| Keamanan/Auth | Menutup pengaktifan ulang kewenangan klinis lewat Data Induk (keselamatan klinis) |

## 4. Dokumentasi endpoint

#### Corporate / Human Resource / Master Data / Doctor

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/corporate/human-resource/master-data/doctors` | Daftar dokter dengan status pra-kredensial | `Doctor : Create` |
| `PUT` | `…/doctors/{id}` | Ubah data; status kredensial tidak berubah | `Doctor : Update` |
| `PATCH` | `…/doctors/{id}/status` | Ubah status aktif/kepegawaian; status kredensial tidak berubah | `Doctor : Update` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Berhasil, 0 error, 608 warning (himpunan warning identik dengan sebelum perubahan) | `PASS` | Keluaran build |
| QBE Strict berkas Menu 04 | 20 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran: create dengan `ClinicalPrivilegeStatus = Active` | `409` | `PASS` (penelusuran) | `IsPreCredentialingState` |
| Penelusuran: `PUT` dokter `Revoked` dengan nilai `Active` | `409` | `PASS` (penelusuran) | guard `PUT` |
| Penelusuran: spesialisasi tidak sesuai profesi | `400` | `PASS` (penelusuran) | `ValidateDoctorMasterReferencesAsync` |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Relasi profesi aktif valid | Terpenuhi | Validasi profesi aktif di tiga controller; spesialisasi wajib milik profesi |
| (2) Penetapan profesi tidak mengubah kredensial Menu 09 | Terpenuhi | Status kredensial/kewenangan tidak dapat ditetapkan atau diubah dari Data Induk |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Masalah yang diketahui | Form frontend Dokter masih menampilkan kedua status sebagai pilihan yang dapat diubah; sebaiknya dijadikan hanya-baca (task frontend Menu 04) |
| Perubahan sampingan | `NONE` |
| Status Git | Lihat `BE-HRD-402` |
| Langkah berikutnya | `BE-HRD-400` |
