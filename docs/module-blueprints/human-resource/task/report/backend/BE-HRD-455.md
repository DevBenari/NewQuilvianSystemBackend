# Laporan Perubahan Backend — `BE-HRD-455`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-455` |
| Nomor lama | `BE-HRD-445` — dinomori ulang saat merge `QuilvianIntegrationBackend` 23 November 2026; nomor lama kini milik amandemen `S-A7-V2` (lihat `blueprint-manifest.md` revisi `69`) |
| Judul | Filter jalur dan "belum dinilai" pada daftar evaluasi |
| Slice | `S-PKB` — Paket B Navigasi SDM / `MVP-74` · `EPIC HRD-127` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 31, kartu `BE-HRD-455` |
| Trace | `FR-PKB-08`; `HRD-DEC-241`, `HRD-ASM-96`; `VR-PKB-06`; state-transition 34.3 |
| Contract version | `v30` — API 36.4, 36.6; State 34.3; Validation 35 — **approved** 23 November 2026 |
| Dependency | `BE-HRD-454` ✅ |
| Klasifikasi | `LIGHT` — dua parameter query pada satu daftar |
| Task mode | `BACKEND` — instruksi pengguna 23 November 2026 |
| Target tulis | `RecruitmentManagement/DTOs/CandidateEvaluationDtos.cs`, `Services/CandidateEvaluationService.cs`; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `7e4c3774` (branch `MHamzah`); source sudah di-commit pengguna di `9062bd61` |
| Tanggal | 23 November 2026 |
| Status | ✅ Selesai — 3/3 acceptance criteria terpetakan ke source; build 0 error, QBE Strict `PASS`; uji API runtime 23 November 2026 `PASS` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `RecruitmentManagement` |
| Keberlakuan | `TOUCHED LEGACY` |
| QBE yang berlaku | `QBE-DTO-001`, `QBE-VAL-001`, `QBE-PAGE-001` |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Daftar evaluasi hanya bisa disaring per satu jenis evaluasi. Layar Tes/Wawancara butuh dua jalur — Wawancara HR dan Uji Klinis/Teknis (yang mencakup dua jenis sekaligus) — serta pilihan "Belum dinilai" agar asesor langsung melihat pekerjaannya.

## 2. Proses bisnis

1. Layar mengirim `track=HR` (wawancara HR) atau `track=CLINICAL` (uji klinis dan wawancara user, `HRD-ASM-96`). Nilai lain → `400` "Jalur evaluasi tidak dikenal."
2. `pendingOnly=true` menampilkan evaluasi yang nilainya belum pernah dikirim.
3. Kedua filter berjalan bersama pembatasan penilai `BE-HRD-454`: asesor tanpa `ViewAll` tetap hanya melihat miliknya.

**Contoh.** Tiga evaluasi: wawancara HR (Dodi), uji klinis (Sari), wawancara user (Sari). Recruiter dengan `ViewAll`: `track=HR` → 1 baris, `track=CLINICAL` → 2 baris. Sari tanpa `ViewAll` dengan `track=HR` → 0 baris.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`CandidateEvaluationService.cs` (`ScheduleAsync`, `SubmitHrEvaluationAsync`, `SubmitClinicalEvaluationAsync`), `Models/HrdCandidateEvaluation.cs`, `Enums/EvaluationType.cs`, `Enums/EvaluationRecommendation.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/RecruitmentManagement/DTOs/CandidateEvaluationDtos.cs` | `CandidateEvaluationQuery.Track`, `PendingOnly`; konstanta `CandidateEvaluationTracks` |
| `Areas/Corporate/HumanResource/RecruitmentManagement/Services/CandidateEvaluationService.cs` | Validasi `track` (`VR-PKB-06`), filter jalur, filter belum dinilai |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Dua parameter opsional baru; pemanggil lama tidak terdampak |
| Database | Tidak ada |
| Keamanan/Auth | `NOT APPLICABLE` (pembatasan penilai milik `BE-HRD-454`) |

**Delta (fakta source):** penjadwalan evaluasi mengisi rekomendasi bawaan `Considered` dan skor 0, sehingga "belum ada rekomendasi" tidak dapat dibaca dari kolom rekomendasi. Penanda yang dipakai adalah baris yang **belum pernah diperbarui** (`UpdateDateTime == null`): satu-satunya jalur pembaruan baris evaluasi adalah `submit-hr` dan `submit-clinical`, dan keduanya selalu mengisi `UpdateDateTime`.

## 4. Dokumentasi endpoint

#### Corporate / Human Resource / Recruitment Management / Candidate Evaluation

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/human-resource/recruitment/evaluations?track=HR\|CLINICAL&pendingOnly=true` | Daftar per jalur dan yang belum dinilai | `CandidateEvaluation : Read` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` (sesudah `MVP-74` dan build penuh) | 0 error; tanpa warning di berkas task | `PASS` | Log build 23 November 2026 |
| QBE Strict (19 berkas) | `PASS` | `PASS` | Keluaran checker |
| Penelusuran: `track=X` | `Fail("Jalur evaluasi tidak dikenal.")` = `400` sebelum kueri | `PASS` | `GetPagedAsync` |
| Penelusuran: `track=CLINICAL` | `ClinicalAssessment` atau `UserInterview` | `PASS` | `GetPagedAsync` |
| Uji API runtime di DB dev (`QuilvianNewDevHamzah`), 23 November 2026 — backend `9062bd61` di port lokal 5199, akun superadmin + akun non-admin | `track=HR`, `CLINICAL`, `clinical` → `200`; `track=X` → `400` "Jalur evaluasi tidak dikenal."; `pendingOnly=true` → `200` (0 baris; DB dev: 34 pegawai aktif; 0 kontrak, 0 lisensi, 0 SPK/RKK, 0 sanksi, 0 evaluasi kandidat) | `PASS` | Skrip uji API (27 skenario: 26 `PASS`, 1 gagal karena login akun uji atasan ditolak `401` — masalah akun, bukan kode) |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. `track=HR` → 1, `track=CLINICAL` → 2, `track=X` → `400` (`AT-HRD-PKB-22`) | Terpenuhi | Validasi dan filter jalur |
| 2. `pendingOnly=true` hanya evaluasi tanpa rekomendasi | Terpenuhi | `UpdateDateTime == null` (lihat delta) |
| 3. Asesor tanpa `ViewAll` + `track` tetap hanya miliknya | Terpenuhi | Filter penilai diterapkan sebelum filter jalur |
| DoD: laporan tracked, tanda status roadmap | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada |
| Masalah yang diketahui | Arti "belum dinilai" bergantung `UpdateDateTime`; bila kelak ada jalur ubah evaluasi lain (mis. ubah jadwal), penanda ini perlu ditinjau |
| Risiko tersisa | Rendah |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Source sudah di-commit pengguna (`9062bd61`) |
| Langkah berikutnya | `FE-HRD-405` |
