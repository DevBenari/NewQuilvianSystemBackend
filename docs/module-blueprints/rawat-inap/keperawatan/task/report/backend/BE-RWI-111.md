# Laporan Perubahan Backend — `BE-RWI-111`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-111` |
| Judul | Risiko jatuh, nyeri, dan edukasi sebagai dokumen tersendiri |
| Slice | Gelombang 4 — `KEP-V2-1` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-111` |
| Trace | `FR-KEP-047`, `FR-KEP-048`; `VAL-KEP-22a`, `VAL-KEP-22b`; `RWI-DEC-119`; api-contract 7.0–7.1; arsitektur 11.5.3, 11.6 |
| Contract version | `0.5.0` |
| Dependency | `BE-RWI-110` ✅ 17 September 2026 |
| Klasifikasi | `MEDIUM` — tiga nilai enum, penjaga penyelesaian, waktu kajian ulang |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/ClinicalManagement` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `fe7e60d4` (branch `MHamzah`) |
| Tanggal | 17 September 2026 |
| Status | ✅ Selesai — lima kriteria terpetakan; `dotnet build` **NOT RUN** atas keputusan pemilik |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices / ClinicalManagement` |
| Registry / prefix | `Cli` — `ACTIVE / LEGACY` |
| Keberlakuan | `TOUCHED LEGACY` (`PatientAssessmentType`) + `NEW CODE` |
| QBE relevan | `QBE-ENUM-001`, `QBE-VAL-001`, `QBE-SVC-001` |
| Database | Nol migration — enum bertipe `integer`; kolom `PainAssessmentState`/`PainReassessmentDueAt` lahir di K3 |

## 1. Masalah yang diperbaiki

Resiko Jatuh, Monitoring Nyeri, dan Assesment Edukasi punya siklus sendiri — dikaji, dinilai ulang, ditutup —
tetapi selama ini hanya kelompok isian di dalam satu pengkajian besar. Nyeri juga disimpan sebagai
`HasPain` bertipe `bool`, sehingga "belum dinilai" terbaca "tidak nyeri".

## 2. Proses bisnis

1. `PatientAssessmentType` bertambah `FallRisk = 6`, `PainMonitoring = 7`, `EducationAssessment = 8`; nilai lama
   tidak bergeser. Ketiganya dibuat per episode lewat `POST /patient-assessments` seperti jenis lain.
2. Monitoring Nyeri: Ns. Siti memilih "Nyeri" (`PainAssessmentState = HasPain`) skala 7 pukul 08.00. Server
   menurunkan `HasPain = true` dari keadaan itu. Bila instrumen nyeri yang berlaku punya
   `reassessmentMinutes = 60`, `PainReassessmentDueAt = 09.00`.
3. **Jalur tidak normal saat menyelesaikan:** keadaan `NotAssessed` → `422` "Pilih keadaan nyeri: tidak nyeri,
   nyeri, atau tidak dapat dinilai."; `HasPain` tanpa skala → `400` "Isi skala nyeri sesuai instrumen yang
   dipakai."; versi instrumen belum sah di produksi → `422`.
4. Ketiganya memakai instrumen berversi `BE-RWI-107` lewat pemetaan jenis dokumen → jenis instrumen
   (`FallRisk` → `FallRiskScale`, `PainMonitoring` → `PainScale`, `EducationAssessment` →
   `EducationAssessmentForm`).
5. Pengkajian lama berjenis `Initial` yang memuat isian nyeri dan risiko jatuh tetap terbaca apa adanya;
   `PainAssessmentState`-nya `NotAssessed` karena memang tidak pernah dinyatakan.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`PatientAssessmentType.cs`, `PatientAssessmentController.cs`, validation matrix 6.2, PRD bagian 29–31.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Enums/PatientAssessmentType.cs` | Tiga nilai baru |
| `Areas/HealthServices/ClinicalManagement/Services/NursingAssessmentDocumentService.cs` | `KindFor`, penurunan `HasPain`, `PainReassessmentDueAt`, `VAL-KEP-22a/b` |
| `Areas/HealthServices/ClinicalManagement/Controllers/PatientAssessmentController.cs` | Label jenis baru pada metadata; dokumen V2 memanggil service |
| `Areas/HealthServices/ClinicalManagement/Seeders/ClinicalInstrumentDraftSeeder.cs` | Draft `PAIN_MONITORING` dengan isian wajib `PAIN_STATE` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Aditif — tiga nilai enum (frontend wajib menyamakan, `RLN3-CAP-18`) |
| Database | `NOT APPLICABLE` |
| Keamanan/Auth | `NOT APPLICABLE` |

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Patient Assessment

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | `AssessmentType` 6, 7, 8 dengan `PainAssessmentState` | `PatientAssessment : Create` |
| `PATCH` | `/{id}/complete` | Monitoring Nyeri wajib keadaan nyeri | `PatientAssessment : Update` |
| `GET` | `/episodes/{episodeId}?assessmentType=7` | Riwayat per jenis dokumen | `PatientAssessment : Read` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Penelusuran kriteria 1–5 | Terpetakan | `PASS` | Bagian 6 |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Keputusan pemilik 17 September 2026 |
| Verifikasi proses bisnis runtime | Tidak dijalankan | `NOT RUN` | Menunggu build |

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Tiga jenis dokumen terdaftar dan dapat dibuat per episode | Terpenuhi | Enum 6–8; `CreateAssessment` generik per jenis |
| 2. Monitoring Nyeri mewajibkan keadaan nyeri | Terpenuhi | `EnsureCanCompleteAsync` → `422 PAIN_STATE_REQUIRED` |
| 3. Monitoring Nyeri menyimpan waktu kajian ulang | Terpenuhi | `ApplyDraftAsync` → `PainReassessmentDueAt` dari `reassessmentMinutes` |
| 4. Memakai instrumen berversi, bukan angka di kode | Terpenuhi | `KindFor` + `ResolveForPatientAsync` |
| 5. Dokumen lama tetap terbaca | Terpenuhi | Nol perubahan bentuk baris lama |
| DoD: `dotnet build` | **Dikecualikan atas keputusan pemilik 17 September 2026** | `NOT RUN` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Interval kajian ulang nyeri kosong pada draft sampai gate `G-04` diputuskan; `PainReassessmentDueAt` baru terisi setelah itu |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | Build belum dijalankan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M` enum dan controller |
| Langkah berikutnya | `BE-RWI-112` |
