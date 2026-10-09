# Laporan Task Backend — `RJ-DOC-REV-BE-018`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-BE-018` |
| Judul | Rincian rujukan |
| Roadmap | [doctor-consultation-roadmap.md](../../../roadmap/doctor-consultation-roadmap.md) bagian `19` |
| Keputusan | `RJ-DOC-DEC-071`, `072`, `074`..`078`, `083` |
| Kontrak | `RJ-DOC-REFERRAL-001@1.0.0` (**approved**) |
| Dependency | `RJ-DOC-REV-BE-017` ✅ |
| Task mode | `CROSS-REPO MODE` — backend |
| Branch / baseline | `sukmagp` @ `77caf434`, belum di-commit |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Tanggal | 8 Oktober 2026 |
| Status | ✅ `COMPLETE` |

## 1. Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RegistrationManagement` (`Reg`, `ACTIVE / LEGACY`) |
| Keberlakuan | `NEW CODE` (`EncounterReferralService`, `EncounterReferralDtos`); `TOUCHED LEGACY` (`PatientEncounterController`, `PatientEncounterDtos`, `OutpatientEncounterListService`, `OutpatientEncounterDtos`, `Program.cs`, `appsettings.json`) |
| QBE berlaku | QBE-SVC-001 (semua aturan dan akses data rujukan di service), QBE-VAL-001, QBE-TXN-001 (rincian ikut transaksi kunjungan), QBE-DTO-001, QBE-PERM-001, QBE-LOG-001 (log tanpa data kesehatan), QBE-AUD-001 (revisi di tabel sendiri, bukan log) |
| Jenis capability | Transaksi — sub-resource ter-scope induk: `GET`/`PUT /patient-encounters/{id}/referral` |

## 2. Yang dikerjakan

1. **`EncounterReferralService`**: validasi create (`RJ-VAL-PM-03..07`), menempelkan identitas perujuk ke kunjungan dan rincian ke context di dalam transaksi pemanggil, `GetAsync` (dengan `missingFields`, `isLocked`, surat aktif), dan `UpsertAsync`. `UpsertAsync` mencakup penguncian `RJ-VAL-PM-09`, unit tujuan tetap `RJ-VAL-PM-10`, `ExpectedRowVersion` `RJ-VAL-PM-11`, revisi `Completed`/`Corrected` berisi ruas yang berubah saja, dan penggantian `RowVersion`. Kelengkapan dihitung oleh `ApplyCompletion`/`ComputeMissingFields`, yang dipakai juga oleh `BE-019`.
2. **Create kunjungan** (`/admin`, `/kiosk`): request menerima `referralInstitutionId`, `referralDoctorId`, `referral`; validasi sebelum transaksi; rincian dibuat di transaksi yang sama; respons memuat `referral { id, isComplete, rowVersion }`. Jalur Kiosk tidak mewajibkan diagnosa/alasan.
3. **`GET`/`PUT /patient-encounters/{id}/referral`** di `PatientEncounterController`.
4. **Daftar Kunjungan RJ**: ruas `referralStatus` (`NotReferral`/`Complete`/`Incomplete`) dan query `referralStatus`.
5. Konfigurasi `HealthServices:Registration:ReferralDetailRequiredFrom = 2026-10-08`.

| Berkas | Status |
| --- | --- |
| `Areas/HealthServices/RegistrationManagement/Services/EncounterReferralService.cs` | Baru |
| `Areas/HealthServices/RegistrationManagement/DTOS/EncounterReferralDtos.cs` | Baru |
| `Areas/HealthServices/RegistrationManagement/DTOS/PatientEncounterDtos.cs` | Diperbarui |
| `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs` | Diperbarui |
| `Areas/HealthServices/RegistrationManagement/DTOS/OutpatientEncounterDtos.cs` | Diperbarui |
| `Areas/HealthServices/RegistrationManagement/Services/OutpatientEncounterListService.cs` | Diperbarui |
| `Program.cs`, `appsettings.json` | Diperbarui |

## 3. Endpoint

#### Health Services / Registration Management / Patient Encounter

| Method | Path | Perubahan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/health-services/registration-management/patient-encounters/admin` | + `referralInstitutionId`, `referralDoctorId`, `referral`; respons + `referral` | `PatientEncounter : Create` |
| `POST` | `…/patient-encounters/kiosk` | Sama, tanpa wajib diagnosa/alasan | Policy `KioskRead` |
| `GET` | `…/patient-encounters/{encounterId}/referral` | Baru | `PatientEncounter : Read` |
| `PUT` | `…/patient-encounters/{encounterId}/referral` | Baru | `PatientEncounter : Update` |

#### Health Services / Registration Management / Outpatient Encounter

| Method | Path | Perubahan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `…/outpatient-encounters` | + `referralStatus` (respons dan query) | `OutpatientEncounter : Read` (tetap) |

## 4. Delta terhadap desain

| Delta | Alasan |
| --- | --- |
| Endpoint rujukan berada di `PatientEncounterController`, bukan `EncounterReferralController` baru | Aturan akses: argumen pertama `[AccessPermission]` wajib sama dengan `ControllerName`. Resource-nya `PatientEncounter`, jadi endpoint harus di controller itu agar dapat dicentang di Akses Role. Logika tetap di service |
| Kunci konfigurasi `HealthServices:Registration:ReferralDetailRequiredFrom` (desain: `Registration:…`) | Mengikuti bagian `HealthServices:Registration` yang sudah ada |
| `PUT` menerima isian parsial; kelengkapan dihitung | Melengkapi bertahap. Layar petugas tetap mewajibkan isian lengkap (`RJ-VAL-PM-06` pada create admin) |
| Unit tujuan rincian baru lewat `PUT` diturunkan dari kunjungan (berklinik → Poliklinik, selain itu → Laboratorium) | Mencegah unit tujuan berbeda dari kunjungan yang sebenarnya |
| Kunjungan Laboratorium tidak tampil di Daftar Kunjungan RJ | Daftar itu hanya memuat kunjungan RJ berklinik (kontrak `RJ-DOC-ENCLIST-001`). Rujukan Lab yang belum lengkap terlihat lewat `GET …/referral`. Dicatat untuk `FE-020` |

## 5. Verifikasi

| Perintah / skenario | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet build -c Release` | 0 Error | `PASS` |
| EF `has-pending-model-changes` | Tidak ada perubahan model | `PASS` |
| QBE Strict | VIOLATION 0, REVIEW 0 | `PASS` |
| `dotnet test` | `NOT RUN` — tidak ada project test | — |
| Runtime HTTP backend uji 7185 | **17/17 PASS** | `PASS` |

| ID | Skenario | Hasil |
| --- | --- | --- |
| E1–E5 | Create admin: tanpa diagnosa `PM-06`; dokter bukan milik institusi `PM-04`; tanggal +2 jam `PM-07`; Radiologi `PM-05`; tanpa nomor `PM-03` | PASS |
| E6 | Diagnosa tidak ada → `400`, jumlah kunjungan pasien tidak berubah | PASS |
| E7 | Create admin + rujukan → `200`, `referral.isComplete = false` (belum ada surat) | PASS |
| E8 | `GET`: institusi mitra, snapshot mitra, diagnosa `E11.7`, `missingFields = ["documents"]`, tidak terkunci, Poliklinik, `captureSource` Staff | PASS |
| E9 | `PUT` koreksi alasan → `200`, `rowVersion` berganti, revisi `Corrected` memuat `referralReason` | PASS |
| E10 | `rowVersion` basi → `409 PM-11` | PASS |
| E11 | Ubah unit tujuan → `400 PM-10` | PASS |
| E12 | `PUT` ke kunjungan status 6 (`ENC-RSMMC-00101`) → `409 PM-09` (ditolak sebelum ada perubahan) | PASS |
| E13–E14 | Kiosk + rujukan tanpa diagnosa/alasan → `200`; `captureSource` Kiosk; `missingFields` diagnosa, alasan, surat | PASS |
| E15–E16 | Daftar RJ `referralStatus=Incomplete` memuat kedua kunjungan uji; `NotReferral` tidak; nilai salah → `400` | PASS |
| E17 | Registrasi lab `external-referral`, lalu `PUT` membuat rincian Laboratorium, snapshot mitra `false` | PASS |

Data uji: tiga kunjungan dibatalkan (`PATCH …/admin/{id}/cancel`, `200`). Master perujuk `PMTEST-KSS` (mitra), `PMTEST-PKM`, `dr. PMTEST Rina Lestari`, dan `dr. PMTEST Budi Santoso` dipertahankan sebagai data uji `RJ-DOC-DEC-079`.

## 6. Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Isian wajib kosong → kode yang tepat | Terpenuhi | E1–E5 |
| 2. Create poli + rujukan satu transaksi; gagal salah satu → tidak ada keduanya | Terpenuhi | E6, E7 |
| 3. `PUT` jalur Lab membuat rincian; koreksi menulis revisi; ubah unit → `PM-10` | Terpenuhi | E17, E9, E11 |
| 4. Status ≥ 6 → `409 PM-09`; `RowVersion` basi → `409 PM-11` | Terpenuhi | E12, E10 |
| 5. Radiologi → `PM-05` | Terpenuhi | E4 |
| 6. List RJ: `Incomplete` untuk rujukan Kiosk tanpa diagnosa; kunjungan sebelum tanggal konfigurasi tidak ditandai | Terpenuhi — cabang tanggal lama terbukti lewat kode (`ResolveReferralStatus`); tidak ada kunjungan rujukan lama tanpa rincian di DB uji | E15 |

Transisi revisi `Completed` (tidak lengkap → lengkap) membutuhkan surat dan diuji pada `BE-019`.

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Migration | Tidak ada (skema dari `BE-017`) |
| Risiko | Server dev 7184 perlu restart untuk memuat kode ini |
| Task berikutnya | `RJ-DOC-REV-BE-019`, `RJ-DOC-REV-BE-021` |
| Perubahan sampingan | `NONE` |
