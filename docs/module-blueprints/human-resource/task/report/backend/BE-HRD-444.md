# Laporan Perubahan Backend — `BE-HRD-444`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-444` |
| Judul | Pembatasan asesor pada evaluasi kandidat dan hak `CandidateEvaluation : ViewAll` |
| Slice | `S-PKB` — Paket B Navigasi SDM / `MVP-72` · `EPIC HRD-126` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 30, kartu `BE-HRD-444` |
| Trace | `FR-PKB-09`, `FR-PKB-10`; `HRD-DEC-233`, `HRD-ASM-95`, `HRD-DEC-077`; `VR-PKB-07`, `VR-PKB-08` |
| Contract version | `v29` — API 35.4, 35.6; Validation 34; Permission 33.1, 33.3 — **approved** 23 November 2026 |
| Dependency | — |
| Klasifikasi | `MEDIUM` — controller, service, interface, dan satu berkas hak eksplisit baru; perubahan perilaku akses |
| Task mode | `BACKEND` — instruksi pengguna 23 November 2026 |
| Target tulis | `Areas/Corporate/HumanResource/RecruitmentManagement/**`; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `7e4c3774` (branch `MHamzah`); source sudah di-commit pengguna di `9062bd61` |
| Tanggal | 23 November 2026 |
| Status | ✅ Selesai — 5/5 acceptance criteria terpetakan ke source; build 0 error, QBE Strict `PASS`, registri `IsValid=True` dengan `CandidateEvaluation.ViewAll`; uji API DB dev `NOT RUN` |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `RecruitmentManagement` |
| Pemilik / prefix registry | Human Resource, `Hrd` (`ACTIVE / LEGACY`) |
| Keberlakuan | `TOUCHED LEGACY`; berkas hak eksplisit `NEW CODE` (tanpa entity, tanpa migration) |
| QBE yang berlaku | `QBE-PERM-001`, `QBE-API-001`, `QBE-SVC-001`, `QBE-DTO-001`, `QBE-VAL-001` |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0; `role-access-rules.md` |

---

## 1. Masalah yang diperbaiki

Siapa pun yang memegang `CandidateEvaluation : Read` dapat melihat seluruh evaluasi kandidat, termasuk nilai dan catatan penilaian milik asesor unit lain. Pemegang `CandidateEvaluation : EvaluateClinical` juga dapat mengirim nilai uji klinis untuk evaluasi yang ditugaskan kepada asesor lain, termasuk memicu veto klinis (`HRD-DEC-077`) atas nama orang lain.

Contoh: Sari (asesor ICU) dapat membuka nilai wawancara kandidat IGD milik asesor Dodi dan bahkan mengirim nilai "Tidak Direkomendasikan" untuk evaluasi itu, yang langsung menggugurkan kandidat.

## 2. Proses bisnis

1. Recruiter menjadwalkan evaluasi dan menunjuk penilai (`EvaluatorUserId`) — tidak berubah.
2. Saat daftar Tes/Wawancara dibuka, controller memeriksa penanda **`CandidateEvaluation : ViewAll`**:
   - **Punya penanda** (Recruiter HR, Manajer SDM): semua evaluasi tampil.
   - **Tanpa penanda** (asesor): hanya evaluasi yang `EvaluatorUserId`-nya akun yang login.
3. Membuka detail evaluasi milik penilai lain tanpa penanda → `403` "Evaluasi ini ditugaskan kepada penilai lain."
4. Mengirim nilai uji klinis (`submit-clinical`) hanya boleh oleh penilai yang ditugaskan. Penanda `ViewAll` **tidak** membuka hak ini → bukan penilai = `403` "Hanya penilai yang ditugaskan yang dapat mengirim nilai uji ini.", data evaluasi tidak berubah.
5. Penanda diberikan admin lewat Manajemen Role → Akses Role; kode tidak mengenal nama peran.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`RecruitmentManagement/Controllers/CandidateEvaluationController.cs`, `Services/CandidateEvaluationService.cs`, `Services/ICandidateEvaluationService.cs`, `DTOs/CandidateEvaluationDtos.cs`, `DTOs/RecruitmentResult.cs`, `Models/HrdCandidateEvaluation.cs`, `EmployeeRelationManagement/MultiDeptExitClearanceExplicitPermissions.cs` (pola), `Shared/HumanResource/Helpers/HrActorClaims.cs`, `Controllers/AuthController.cs` (klaim `user_id` = `NameIdentifier` = `ApplicationUser.Id`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/RecruitmentManagement/RecruitmentEvaluationExplicitPermissions.cs` (baru) | `[assembly: AccessExplicitPermission]` `CandidateEvaluation : ViewAll` (Read) + konstanta `RecruitmentEvaluationPermissions` |
| `Areas/Corporate/HumanResource/RecruitmentManagement/Controllers/CandidateEvaluationController.cs` | Injeksi `AccessPermissionService`; `GetPaged`/`GetById` menghitung `canViewAll` dan meneruskan `User`; deklarasi `403` pada Swagger |
| `Areas/Corporate/HumanResource/RecruitmentManagement/Services/ICandidateEvaluationService.cs` | `GetPagedAsync`/`GetByIdAsync` menerima `ClaimsPrincipal user` dan `bool canViewAll` |
| `Areas/Corporate/HumanResource/RecruitmentManagement/Services/CandidateEvaluationService.cs` | Filter `EvaluatorUserId == actor` tanpa `ViewAll`; detail milik penilai lain `403`; `submit-clinical` menolak bukan penilai; pemuatan ulang sesudah tulis memakai `LoadResponseAsync` privat |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Route, DTO request/response, dan atribut akses keempat endpoint tidak berubah. Perilaku baru: daftar tersaring dan `403` sesuai kontrak 35.4/35.6 |
| Database | Tidak ada; tanpa migration |
| Keamanan/Auth | Hak baru `CandidateEvaluation : ViewAll` (penanda, tanpa endpoint). **Wajib diberikan ke Recruiter HR dan Manajer SDM sebelum rilis**; tanpa itu mereka hanya melihat evaluasi yang ditugaskan kepada dirinya |

## 4. Dokumentasi endpoint

#### Corporate / Human Resource / Recruitment Management / Candidate Evaluation

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/human-resource/recruitment/evaluations` | Daftar Tes/Wawancara, tersaring menurut penilai tanpa `ViewAll` | `CandidateEvaluation : Read`; cakupan penuh dengan `CandidateEvaluation : ViewAll` |
| `GET` | `/api/v1/corporate/human-resource/recruitment/evaluations/{id}` | Detail; milik penilai lain `403` tanpa `ViewAll` | `CandidateEvaluation : Read` |
| `POST` | `/api/v1/corporate/human-resource/recruitment/evaluations/{id}/submit-clinical` | Kirim nilai uji klinis; hanya penilai yang ditugaskan | `CandidateEvaluation : EvaluateClinical` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` (sesudah `MVP-72` dan build penuh Paket B) | 0 error; tidak ada warning di berkas task ini | `PASS` | Log build 23 November 2026 |
| QBE Strict (19 berkas Paket B) | 0 VIOLATION, 0 REVIEW, `PASS` | `PASS` | Keluaran checker |
| Validator registri hak akses | `IsValid=True`, 2.662 identitas; snapshot memuat `CandidateEvaluation.ViewAll (Read)` di samping `Read`, `Evaluate`, `EvaluateClinical` | `PASS` | Keluaran skrip `dotnet fsi` |
| Penelusuran: asesor tanpa `ViewAll` | `dbQuery.Where(e => e.EvaluatorUserId == actorUserId)`; ID kosong → `401` (tidak pernah mencocokkan evaluasi tanpa penilai) | `PASS` | `GetPagedAsync` |
| Penelusuran: detail milik penilai lain | `Fail(..., 403)` dengan pesan kontrak | `PASS` | `GetByIdAsync` |
| Penelusuran: `submit-clinical` oleh pemegang `ViewAll` bukan penilai | Ditolak `403` sebelum nilai diubah dan sebelum `SaveChangesAsync` | `PASS` | `SubmitClinicalEvaluationAsync` |
| Uji API dua akun (recruiter dan asesor) di DB dev | Belum dijalankan | `NOT RUN` | Butuh izin menyalakan backend pada DB dev bersama |

Uji manual: `NOT FEASIBLE` (menunggu izin).

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Akun `ViewAll` melihat evaluasi dua penilai (`AT-HRD-PKB-18` bagian backend) | Terpenuhi | Tanpa filter penilai bila `canViewAll` |
| 2. Asesor tanpa `ViewAll` hanya melihat evaluasi miliknya (`AT-HRD-PKB-19`) | Terpenuhi | Filter `EvaluatorUserId` |
| 3. Detail milik penilai lain → `403` pesan kontrak (`AT-HRD-PKB-20`) | Terpenuhi | `GetByIdAsync` |
| 4. `EvaluateClinical` + `ViewAll` mengirim nilai milik penilai lain → `403`, evaluasi tidak berubah (`AT-HRD-PKB-21`) | Terpenuhi | Pemeriksaan sebelum mutasi |
| 5. `ViewAll` tampil di Akses Role; validator registri bersih; atribut akses keempat endpoint tidak berubah | Terpenuhi | Snapshot registri; diff controller hanya menambah injeksi dan `ProducesResponseType` |
| DoD: laporan tracked, tanda status roadmap | Terpenuhi | Berkas ini; roadmap bagian 30 |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning baru |
| Masalah yang diketahui | `submit-hr` (wawancara HR) tidak dibatasi penilai — sesuai kontrak (hanya `submit-clinical` yang diubah) |
| Risiko tersisa | Recruiter kehilangan visibilitas bila `ViewAll` belum diberikan saat rilis — langkah rilis di `BE-HRD-452` |
| Perubahan sampingan | `NONE` (sempat terjadi konversi CRLF pada `CandidateEvaluationService.cs` oleh skrip penyuntingan; dipulihkan ke LF sebelum build) |
| Interupsi | `NONE` |
| Status Git | Source sudah di-commit pengguna (`9062bd61`); working tree `Areas/` bersih |
| Langkah berikutnya | `BE-HRD-445`; `FE-HRD-397` |
