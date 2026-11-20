# Laporan Perubahan Backend — `BE-HRD-419`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-419` |
| Judul | Bukti pemberitahuan tertulis H-7 |
| Slice | `S-FIN-M08` / Perpanjangan (alias `FIN-HRD-08-BE-004`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.5, kartu `BE-HRD-419` |
| Trace | UU 27/2026 Pasal 82; instruksi Menu 08 `FIN-HRD-08-BE-004` |
| Contract version | Kontrak Kontrak Kerja existing (`BE-HRD-065`, `BE-HRD-080`) |
| Dependency | `BE-HRD-418` ✅ |
| Klasifikasi | `MEDIUM` — model, migration, endpoint, scheduler |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | `HrdContractDecision`, konfigurasinya, `HrdEmploymentContractService`, `HrdEmploymentContractController`, `HrdEmploymentLifecycleSchedulerService`, migration `20261120092000`; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d17aa187` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 2/2 acceptance criteria terpetakan ke source; build, QBE, registri lulus; migration `20261120092000_AddHrdContractDecisionNoticeEvidence` ditulis, **belum dijalankan** |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `LifecycleManagement` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | QBE-TXN-001, QBE-VAL-001, QBE-CODE-003/004/006 (bila menyangkut nomor) |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Status pemberitahuan H-7 dihitung dari catatan teks: mengisi `EvidenceNotes` sembarang sudah menghasilkan `ON_TIME`, dan tampilan detail menghitung `ON_TIME` hanya dari tanggal pemberitahuan. Pegawai bisa "dianggap sudah diberi tahu" tanpa bukti yang dapat diperiksa. Tenggat yang terlewat juga tidak diekskalasi.

## 2. Proses bisnis

1. Petugas mencatat tanggal pemberitahuan (`POST …/decisions/{decisionId}/notify`); catatan teks tetap boleh, tetapi tidak menentukan status.
2. Petugas mengunggah artefak bukti (`POST …/decisions/{decisionId}/notice-evidence`, berkas surat bertanda terima/tangkapan kanal resmi). Bukti tersimpan sekali dan tidak dapat diganti.
3. Status: tanggal + artefak → `ON_TIME` (≤ H-7) atau `LATE`; tanpa artefak → `MISSING` sebelum tenggat, `LATE` sesudah tenggat; kontrak tanpa tanggal akhir → `NOT_APPLICABLE`.
4. Scheduler harian mengirim eskalasi sekali per keputusan ke pemegang `WorkforceEmploymentContract : RecordNotification` bila tenggat H-7 lewat tanpa artefak.

**Contoh.** Kontrak berakhir 31 Desember 2026 → tenggat 24 Desember. Pemberitahuan 20 Desember + berkas tanda terima → `ON_TIME`. Hanya catatan "sudah disampaikan lisan" → 25 Desember menjadi `LATE` dan HR menerima eskalasi.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`RecordEmployeeNotifiedAsync`, `MapToDecisionResponse`, `IHrdLifecycleAttachmentService`, `HrdEmploymentLifecycleSchedulerService`, `IHrdNotificationService`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/LifecycleManagement/Models/HrdContractDecision.cs` | Kolom `NoticeEvidencePath`, `NoticeEvidenceFileName`, `NoticeEvidenceUploadedAt`, `NoticeEvidenceUploadedByUserId` |
| `Repositories/Configurations/.../HrdContractDecisionConfiguration.cs` | Pemetaan kolom bukti |
| `Migrations/20261120092000_AddHrdContractDecisionNoticeEvidence.cs` | Migration manual idempoten (belum dijalankan) |
| `Areas/Corporate/HumanResource/LifecycleManagement/Services/HrdEmploymentContractService.cs` | `ComputeNoticeCompliance` (status dari artefak), `UploadNoticeEvidenceAsync` |
| `Areas/Corporate/HumanResource/LifecycleManagement/Services/IHrdEmploymentContractService.cs` | Kontrak method unggah bukti |
| `Areas/Corporate/HumanResource/LifecycleManagement/Controllers/HrdEmploymentContractController.cs` | Endpoint `notice-evidence` (multipart) |
| `Areas/Corporate/HumanResource/LifecycleManagement/Services/HrdEmploymentLifecycleSchedulerService.cs` | Eskalasi tenggat H-7 tanpa artefak (DedupKey per keputusan) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint baru `POST …/decisions/{decisionId}/notice-evidence` (multipart, `file`); status `ON_TIME` kini mensyaratkan artefak |
| Database | Migration baru, **belum dijalankan** — wajib sebelum dipakai (tanpa kolom ini query keputusan kontrak gagal `42703`) |
| Keamanan/Auth | Endpoint baru berbagi hak `WorkforceEmploymentContract : RecordNotification` |

## 4. Dokumentasi endpoint

#### Corporate / Human Resource / Lifecycle Management / Employment Contracts

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/corporate/human-resource/employment-contracts/{id}/decisions/{decisionId}/notify` | Catat tanggal pemberitahuan | `WorkforceEmploymentContract : RecordNotification` |
| `POST` | `/api/v1/corporate/human-resource/employment-contracts/{id}/decisions/{decisionId}/notice-evidence` | Unggah artefak bukti (sekali) | `WorkforceEmploymentContract : RecordNotification` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratchpad>` | Berhasil, 0 error, 608 warning (himpunan sama dengan sebelum perubahan) | `PASS` | Log build 20 November 2026 |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` (berkas Menu 08) | 9 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran: catatan teks tanpa berkas | Tidak pernah `ON_TIME` | `PASS` | `ComputeNoticeCompliance` |
| Penelusuran: unggah bukti kedua | `409` | `PASS` | `UploadNoticeEvidenceAsync` |
| Penelusuran: tenggat lewat tanpa artefak | `LATE` + eskalasi sekali | `PASS` | scheduler + DedupKey |
| Validator registri hak akses | `IsValid=True` | `PASS` | skrip `dotnet fsi` |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Tanpa artefak bukti tidak pernah `ON_TIME` | Terpenuhi | `ComputeNoticeCompliance` di pencatatan dan tampilan |
| (2) Lewat tenggat ditandai dan diekskalasi | Terpenuhi | status `LATE` + notifikasi scheduler |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Wajib sebelum dipakai | Jalankan migration `20261120092000_AddHrdContractDecisionNoticeEvidence` |
| Dampak frontend | Form pemberitahuan perlu menambah unggah berkas bukti |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit; tidak ada stage/commit/push |
