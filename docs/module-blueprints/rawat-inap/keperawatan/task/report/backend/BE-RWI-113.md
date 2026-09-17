# Laporan Perubahan Backend — `BE-RWI-113`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-113` |
| Judul | Evaluasi Awal MPP (migration K3) |
| Slice | Gelombang 3 — `KEP-V2-1` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-113` |
| Trace | `FR-KEP-053`, `FR-KEP-054`, `FR-KEP-055`; `RWI-DEC-115`, `RWI-DEC-118`, `RWI-DEC-131`, `RWI-DEC-140`, `RWI-DEC-150` (`G-09`); `VAL-KEP-23a`–`c`; `INT-KEP-07`, `INT-KEP-12`; kamus data 11.7; state matrix 5.3; api-contract 7.3 |
| Contract version | `0.5.0` |
| Dependency | `BE-RWI-107` ✅ 17 September 2026 |
| Klasifikasi | `HEAVY` — tabel baru, nomor bisnis, penjaga ganda hak akses + penempatan unit |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/ClinicalManagement`, `Repositories/`, `Migrations/`, `Program.cs` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `fe7e60d4` (branch `MHamzah`) |
| Tanggal | 17 September 2026 |
| Status | ✅ Selesai — lima kriteria terpetakan, kriteria 5 sebagai dependency yang **sengaja dibiarkan terbuka**; migration K3 ditulis, tidak dijalankan; `dotnet build` **NOT RUN** atas keputusan pemilik |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices / ClinicalManagement` |
| Registry / prefix | `Cli` — `ACTIVE / LEGACY`; entity baru `CliCaseManagementEvaluation` |
| Keberlakuan | `NEW CODE` |
| QBE relevan | `QBE-ENT-001`, `QBE-NAM-002`, `QBE-CFG-001`, `QBE-SVC-001`, `QBE-PERM-001`, `QBE-CODE-001/002/003/004/006`, `QBE-VAL-001`, `QBE-DEL-001`, `QBE-LOG-001` |
| Hak akses baru | Resource `CaseManagementEvaluation` : `Read`, `Create`, `Update`, `Amend` |
| Nomor bisnis | `EvaluationNumber` dari `NumberSeriesAllocator` deret `CLI_CASE_MANAGEMENT_EVALUATION`, awalan `MPP`, reset harian, 4 digit; unique index |
| Database | Tabel dan FK jawaban checklist pada migration K3; belum diterapkan |

## 1. Masalah yang diperbaiki

Evaluasi Awal adalah dokumen Manajer Pelayanan Pasien — kebutuhan sosial, pembiayaan, rencana pulang — yang pada
V1 bercampur dengan pengkajian keperawatan. Tanpa dokumen tersendiri, tidak ada yang menjamin hanya MPP yang
menulisnya dan hanya satu dokumen yang hidup per perawatan.

## 2. Proses bisnis

1. Ns. Dewi, MPP Melati, membuka Evaluasi Awal Budi → `GET /episodes/{episodeId}` → `null` → "Belum diisi — milik MPP".
2. Checklist → `GET /checklist/resolve?episodeId=` (instrumen `CaseManagementChecklist`, delapan bagian PRD 33).
3. Dewi membuat konsep → `POST /` dengan `InstrumentVersionId` dan `Responses`. Penjaga berlapis:
   butir `CaseManagementEvaluation : Create` (layar Akses Role) **dan** penempatan di unit episode saat simpan.
   Nomor `MPP-...` terbit dari provider nomor bersama.
4. Dewi menyimpan ulang → `PUT /{id}` dengan `ExpectedUpdateDate`; menyelesaikan → `PATCH /{id}/complete`
   (checklist wajib `Approved` di produksi, isian wajib lengkap).
5. Ns. Siti (perawat pelaksana) membaca dokumen yang sama tanpa `AvailableActions`.
6. **Jalur tidak normal:**
   - Budi dipindah ke Anggrek 14.00; Dewi menyimpan 14.10 → `403` "Evaluasi Awal hanya ditulis MPP yang ditempatkan di unit pasien ini."
   - Konsep atau dokumen selesai kedua → `409` "Pasien ini sudah punya Evaluasi Awal. Lengkapi lewat addendum."
     Dijaga index unik parsial `UX_CliCaseManagementEvaluation_Episode_Active` (`EvaluationStatus IN (1, 2)`).
   - MPP lain menyunting konsep Dewi → `403` "Konsep ini ditulis MPP lain."
   - Kiriman ulang berkunci `Idempotency-Key` sama → `200` dokumen yang sama.
   - Membatalkan tanpa alasan → `400`; hanya konsep yang dapat dibatalkan.
   - Perawatan ditutup → `422`.
7. **Addendum** → `501` "Addendum Evaluasi Awal belum tersedia…" sampai jenis dokumen `14` disetujui pemilik
   `MedicalRecordManagement` (`INT-KEP-12`). Ini dependency yang diketahui, bukan cacat.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kamus data 11.7, state matrix 5.3, validation matrix 6.2, integration contract 8.1 dan 8.6, api-contract 7.3,
`InpatientClinicalContextService.IsNurseOnDutyAtUnitAsync`, `NursingActorService.cs`, `SlidingScaleOrderService.cs` (pola nomor bisnis).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Enums/CaseManagementEvaluationStatus.cs` | Baru |
| `Areas/HealthServices/ClinicalManagement/Models/CliCaseManagementEvaluation.cs` | Baru |
| `Repositories/Configurations/HealthServices/ClinicalManagement/ClinicalInstrumentConfigurations.cs` | Konfigurasi `CliCaseManagementEvaluation` |
| `Migrations/20260917101000_AddAssessmentV2AndCaseManagementEvaluation.cs`, snapshot | Tabel, index, FK jawaban checklist |
| `Areas/HealthServices/ClinicalManagement/Services/InpatientClinicalContextService.cs` | `IsEmployeeAssignedToUnitAsync` (logika lama; nama lama menjadi pembungkus) — `INT-KEP-07` |
| `Areas/HealthServices/ClinicalManagement/Services/NursingEpisodeWriteGuard.cs` | Baru — penjaga tulis bersama |
| `Areas/HealthServices/ClinicalManagement/DTOs/CaseManagementEvaluationDtos.cs` | Baru |
| `Areas/HealthServices/ClinicalManagement/Services/CaseManagementEvaluationService.cs` | Baru |
| `Areas/HealthServices/ClinicalManagement/Controllers/CaseManagementEvaluationController.cs` | Baru |
| `Program.cs` | Registrasi |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Grup baru sesuai 7.3; ditambah `GET /checklist/resolve` (alasan sama dengan `BE-RWI-107`) |
| Database | Tabel baru `CliCaseManagementEvaluation` |
| Keamanan/Auth | Resource baru; `CancelReason` sensitif tidak masuk payload logger |

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Case Management Evaluation

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/episodes/{episodeId}` | Evaluasi Awal hidup satu episode, atau `null` | `CaseManagementEvaluation : Read` |
| `GET` | `/checklist/resolve` | Checklist yang berlaku | `CaseManagementEvaluation : Read` |
| `GET` | `/{id}` | Detail beserta checklist | `CaseManagementEvaluation : Read` |
| `POST` | `/` | Membuat konsep | `CaseManagementEvaluation : Create` |
| `PUT` | `/{id}` | Menyimpan ulang konsep | `CaseManagementEvaluation : Update` |
| `PATCH` | `/{id}/complete` | Menyelesaikan | `CaseManagementEvaluation : Update` |
| `PATCH` | `/{id}/cancel` | Membatalkan konsep beralasan | `CaseManagementEvaluation : Update` |
| `POST` | `/{id}/addendums` | `501` sampai `INT-KEP-12` | `CaseManagementEvaluation : Amend` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Penelusuran proses bisnis kriteria 1–5 | Terpetakan | `PASS` | Bagian 6 |
| Unique parsial pada migration, konfigurasi, snapshot | Filter `"EvaluationStatus" IN (1, 2) AND "IsDelete" = false` identik pada ketiganya | `PASS` | Generator sesi ini |
| `dotnet build`, verifikasi skema Postgres | Tidak dijalankan | `NOT RUN` | Keputusan pemilik 17 September 2026 |

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Jenis tersendiri milik `ClinicalManagement`, delapan bagian checklist berversi | Terpenuhi | `CliCaseManagementEvaluation` + jawaban `CaseManagementChecklist` |
| 2. Hanya MPP di unit episode yang menulis; perawat lain membaca | Terpenuhi | `[AccessPermission]` Create/Update + `NursingEpisodeWriteGuard.EnsureCanWriteAsync` |
| 3. Satu Evaluasi Awal hidup per episode, unique parsial | Terpenuhi | `UX_CliCaseManagementEvaluation_Episode_Active` + pemeriksaan service |
| 4. `Draft` → `Completed` → `Cancelled` sesuai 5.3 | Terpenuhi | `CompleteAsync` dan `CancelAsync` hanya dari `Draft` |
| 5. Addendum menunggu jenis dokumen `14` — dicatat apa adanya | Terpenuhi sebagai **dependency terbuka yang diketahui** | Endpoint `501`; `IsAddendumAvailable = false` beserta alasannya |
| DoD: `dotnet build`, verifikasi skema | **Dikecualikan atas keputusan pemilik 17 September 2026** | `NOT RUN` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Pendaftaran keutuhan dokumen (`RegisterAsync`/`SignAsync`) belum dipanggil karena jenis dokumen `14` belum ada |
| Masalah yang diketahui | `INT-KEP-12` terbuka — pemilik `MedicalRecordManagement` Yoga Aji Pratama |
| Risiko tersisa | Checklist belum disahkan (gate `G-07`) — penyelesaian ditolak di produksi |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Berkas baru `??`; `M` context service, konfigurasi, snapshot, `Program.cs` |
| Langkah berikutnya | Persetujuan `INT-KEP-12`; frontend Evaluasi Awal |
