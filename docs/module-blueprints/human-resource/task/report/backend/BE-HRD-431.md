# Laporan Perubahan Backend — `BE-HRD-431`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-431` |
| Judul | Asesmen reviewer dan sidang pleno berjejak |
| Slice | `S-FIN-M09` / Status Kredensial (alias `FIN-HRD-09-BE-006`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 28.6, kartu `BE-HRD-431` |
| Trace | M09-P0-07; instruksi Menu 09 `FIN-HRD-09-BE-006` |
| Contract version | Kontrak Kompetensi & Kredensial existing (`BE-HRD-207` s.d. `BE-HRD-220`); tidak ada endpoint baru |
| Dependency | `BE-HRD-430` ✅ |
| Klasifikasi | `MEDIUM` |
| Task mode | `BACKEND` (instruksi pengguna 20 November 2026) |
| Target tulis | `CredentialingWorkflowService`, `CredentialingWorklistQueryService`, `SpkRkkIssuanceController`; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `d17aa187` (branch `MHamzah`), belum di-commit |
| Tanggal | 20 November 2026 |
| Status | ✅ Selesai — 2/2 acceptance criteria terpetakan ke source; build, QBE lulus; uji API `NOT RUN` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `CredentialingManagement` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | QBE-TXN-001, QBE-VAL-001, QBE-CODE-003/004/006 (bila menyangkut nomor) |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Asesmen disimpan ke `TrxClinicalPrivilegeAssessment` dengan `ClinicalPrivilegeRequestId = application.Id`, padahal foreign key di database (`FK_TrxClinicalPrivilegeAssessment_TrxClinicalPrivilegeRequest_…`) menunjuk tabel `TrxClinicalPrivilegeRequest` — setiap penyimpanan asesmen gagal pelanggaran FK di runtime. Status/peran salah dijawab `400` seragam.

## 2. Proses bisnis

1. Reviewer membuka worklist (`GET …/peer-review-worklist`) — permohonan `InReview` yang punya baris panel atas akunnya.
2. Kirim asesmen (`POST …/assessments`): hanya anggota panel aktif, atas akun sendiri, satu kali; butir harus menunjuk katalog yang diajukan pemohon, tanpa ganda. Hasil disimpan pada baris panel reviewer itu (`Recommendation=Completed`, rincian butir di `ReviewEvidenceJson`).
3. Matriks pleno (`GET …/plenary-matrix`) menampilkan hasil per reviewer berdampingan sehingga beda skor terlihat.
4. Pleno (`POST …/plenary-session`): status `InReview`, ketua bukan pemohon, minimal 2 reviewer berbeda sudah menilai, keputusan `Recommended`/`ConditionallyRecommended`/`Rejected`, butir keputusan bersumber dari katalog yang diajukan. Panel tidak dihapus.
5. Kode: aktor salah `403`, status salah `409`, validasi `422`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`TrxClinicalPrivilegeAssessmentConfiguration.cs` (FK), migration `20260726161839_initializeBigModulHRD2` (FK di DB), service/worklist kredensial.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/CredentialingManagement/Services/CredentialingWorkflowService.cs` | Asesmen & pleno membaca/menulis baris panel; `EnsureItemsHaveSource`; ketua bukan pemohon; validasi keputusan |
| `Areas/Corporate/HumanResource/CredentialingManagement/Services/CredentialingWorklistQueryService.cs` | Worklist reviewer, lembar telaah, matriks pleno dari baris panel |
| `Areas/Corporate/HumanResource/CredentialingManagement/Controllers/SpkRkkIssuanceController.cs` | Pemetaan `403`/`409`/`422` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Route/DTO tetap (`AssessmentId` pada matriks kini ID baris panel); Kode galat aksi tulis kredensial kini: `403` aktor bukan pemilik peran, `409` status tidak mengizinkan aksi, `422` validasi bisnis (sebelumnya `400` untuk semuanya) |
| Database | Tanpa migration; tidak lagi menulis `TrxClinicalPrivilegeAssessment` untuk telaah Mitra Bestari |
| Keamanan/Auth | Hak existing `AssessPeer` / `ApproveCommittee` |

## 4. Dokumentasi endpoint

#### HR Credentialing - Committees, Peer Review & SPK-RKK Issuance

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/hrd/credentialing/applications/peer-review-worklist` | Worklist reviewer | `SpkRkkIssuance : AssessPeer` |
| `GET` | `/api/v1/corporate/hrd/credentialing/applications/{id}/peer-review-sheet` | Lembar telaah | `SpkRkkIssuance : AssessPeer` |
| `POST` | `/api/v1/corporate/hrd/credentialing/applications/{id}/assessments` | Kirim asesmen | `SpkRkkIssuance : AssessPeer` |
| `GET` | `/api/v1/corporate/hrd/credentialing/applications/{id}/plenary-matrix` | Matriks lintas reviewer | `SpkRkkIssuance : ApproveCommittee` |
| `POST` | `/api/v1/corporate/hrd/credentialing/applications/{id}/plenary-session` | Sahkan pleno | `SpkRkkIssuance : ApproveCommittee` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratchpad>` | Berhasil, 0 error, 608 warning (himpunan sama dengan sebelum perubahan) | `PASS` | Log build 20 November 2026 |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` (14 berkas Menu 09) | 14 berkas: 0 VIOLATION, 0 REVIEW, PASS | `PASS` | Keluaran checker |
| Penelusuran: simpan asesmen | Tidak lagi melanggar FK (baris panel ber-FK ke permohonan) | `PASS` | konfigurasi `TrxCredentialingCommitteeReview` |
| Penelusuran: bukan anggota panel / kirim dua kali / permohonan bukan InReview | `403` / `409` / `409` | `PASS` | `SubmitPeerReviewAssessmentAsync` |
| Uji API di DB dev | Belum dijalankan | `NOT RUN` | Wewenang tulis DB dev belum diberikan |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Relasi FK asesmen benar | Terpenuhi | baris panel ber-FK `CredentialingApplicationId` |
| (2) State/peran salah → `403`/`409`/`422` | Terpenuhi | pemetaan controller |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Dampak frontend | Tampilkan pesan `403`/`409`/`422` secara berbeda |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan belum di-commit; tidak ada stage/commit/push |
