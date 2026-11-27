# Laporan Perubahan Backend — `FIN-HRD-REC-JRN-01` s.d. `FIN-HRD-REC-JRN-03`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID / Instruksi | `FIN-HRD-REC-JRN-01` (Aggregasi Journey Read-Only), `FIN-HRD-REC-JRN-02` (Hardening Guard Urutan/Privasi/Veto), `FIN-HRD-REC-JRN-03` (Handoff Rehire, Kursi Headcount, & Recovery Atomik) |
| Judul | Recruitment Journey End-to-End Backend API & Workflow Guards |
| Slice canonical existing | `S-D2` — Rekrutmen & Seleksi (tahap 2 setelah `FIN-HRD-REC-WIZ-01`) |
| Rujukan | `INSTRUKSI_AGENT_HRD_RECRUITMENT_JOURNEY.md`, `HRD-DEC-073` s.d. `HRD-DEC-080`, `VR-REC-07..11`, `VR-PKB-07` |
| Target tulis | `Areas/Corporate/HumanResource/RecruitmentManagement/{Controllers,DTOs,Services}`, `Program.cs`, `Tests/QuilvianSystemBackend.HumanResourceTests/RecruitmentJourneyWorkflowTests.cs` |
| Entity / Migration baru | Tidak ada (`NO MIGRATION`, tanpa DDL/DML SQL langsung) |
| Tanggal | 25 November 2026 |
| Status | ✅ **IMPLEMENTED** — `dotnet build` (`0 Error(s)`), `QBE Strict` (`0 VIOLATION`, `13` berkas), `5/5` xUnit SQLite workflow test lulus, serta `3/3` verifikasi HTTP API (`JRN-BE-01..03`) di port terisolasi `7194`; pengujian DB dev atas data lamaran nyata mencatat `0` lamaran di `QuilvianNewDevHamzah` sehingga perilaku data penuh terverifikasi lewat xUnit SQLite in-memory |

---

## 1. Tujuan & Masalah yang Diselesaikan

Sebelum amandemen ini, data perjalanan lamaran seorang pelamar tersebar di 7 pengontrol/layanan terpisah tanpa tampilan agregat satu-pintu maupun penjagaan sinkron lintas tahap. Pelanggaran yang ditemukan dan ditutup pada tahap ini:

1. **Belum ada endpoint baca satu-pintu per `ApplicationId`**: Petugas harus memeriksa daftar lamaran, screening, evaluasi HR/klinis, surat penawaran, MCU, handoff, dan onboarding case satu per satu. Kini tersedia `GET /api/v1/corporate/human-resource/recruitment/applications/{applicationId}/journey` berbasis `AsNoTracking` yang merangkai tepat 6 tahap visual (*Pendaftaran*, *Seleksi Administrasi*, *Tes & Wawancara*, *Surat Penawaran*, *MCU Pra-Kerja*, *Serah Terima*).
2. **Privasi sensitif & isolasi hak akses (`FIN-HRD-REC-JRN-02`)**: Endpoint Journey menghormati `AccessPermissionService` per subdomain (`CandidateScreening`, `CandidateEvaluation` termasuk `ViewAll` per `VR-PKB-07`, `JobOffer`, `JobOfferApproval`, `OccupationalHealthClearance`, `HiringHandoff`, `TalentPool`, `WorkforceOnboardingCase`), menyamarkan `MaskedNationalIdNumber`, menyembunyikan kompensasi (`IsSalaryMasked`) dan catatan/sertifikat medis MCU (`IsMedicalDetailsMasked`) bagi non-medis, serta memunculkan status `RESTRICTED` tanpa kebocoran field bila hak baca tidak tersedia.
3. **Lubang aturan urutan, duplikasi, dan isolasi lamaran (`FIN-HRD-REC-JRN-02`)**:
   - `CandidateEvaluationService.ScheduleAsync` sebelumnya tidak mensyaratkan `ScreeningStatus.Passed`, dan evaluasi terjadwal yang belum disubmit (`UpdateDateTime == null`) bisa salah terbaca sebagai selesai saat `CheckAndAdvanceApplicationStageAsync`; pengajuan veto klinis juga sebelumnya langsung menutup `Candidate.Status = Rejected` meski pelamar masih punya lamaran aktif lain di lowongan berbeda. Kini diperbaiki dan diisolasi per lamaran.
   - `JobOfferService` & `JobOfferApprovalService` kini memakai `RecruitmentJourneyPolicy.EvaluateOfferPrerequisites` (wajib lulus evaluasi HR & klinis bila klinis, kuota & lowongan aktif), mengganti alokasi nomor `OFR` ke `NumberSeriesAllocator`, serta menutup bypass status `Approved` di `CandidateRespondAsync`.
   - `McuClearanceService` mengganti pengecekan role string kaku `"Doctor"/"DokterK3RS"/"K3RS"` dengan `AccessPermissionService.HasAccessAsync(user, "OccupationalHealthClearance", "Evaluate")`, menahan `FitWithRestrictions` pada `ConditionalAccepted` (`MCU_FIT_WITH_RESTRICTIONS_PENDING_CONFIRMATION`), dan melarang pengeditan sesudah `Unfit` atau pasca-handoff.
4. **Atomisitas Handoff, Rehire Dedup, & Partial Recovery (`FIN-HRD-REC-JRN-03`)**: `HiringHandoffService` & `TalentPoolService.FastTrackPromoteAsync` kini memakai `NumberSeriesAllocator`, memvalidasi kursi headcount (`SeatStatus` masih `Open`/`Linked`), menggunakan kembali `PreviousWorkforceProfileId` atau NIK di `MstEmployees` untuk kandidat *Rehire* (`HRD-DEC-075`), membungkus persistensi `Handoff -> HrdOnboardingCase` ke transaksi DB dengan rollback, serta mendukung pemulihan idempoten bila `HrdOnboardingCase` hilang (`NeedsOnboardingRecovery`).

**Contoh skenario RS MMC**: Perawat *Ns. Nadia* melamar di lowongan klinis Perawat IGD (aktif di tahap MCU *Laik Kerja dengan Batasan*) sekaligus melamar di lowongan Staf Administrasi SDM. Saat komite klinis menginput veto pada lamaran lain, hanya lamaran tersebut yang gugur permanen — lamaran IGD tetap berjalan dan penawaran tidak dapat langsung masuk ke Serah Terima sebelum batasan medis dikonfirmasi dan hasil akhir menjadi `Fit` + `FinalAccepted`. Bila pelamar berstatus *Rehire*, serah terima mendaur ulang `MstWorkforceProfile` lama (*EMP-2021-018*) alih-alih mencetak profil pegawai kembar.

---

## 2. Daftar Berkas & Audit Dampak (`NewQuilvianSystemBackend`)

| Berkas | Status | Ringkasan Perubahan |
| --- | --- | --- |
| `Areas/Corporate/HumanResource/RecruitmentManagement/DTOs/RecruitmentJourneyDtos.cs` | Baru | Kontrak DTO kuat `RecruitmentJourneyResponse`, `JourneyStageDto` (`RegistrationDetail`, `ScreeningDetail`, `EvaluationDetail`, `OfferDetail`, `McuDetail`, `HandoffDetail`), `JourneyBlockerDto`, `JourneyActionDto`, `JourneyNextActionDto`, `JourneyHistoryItemDto`, dan `JourneyViewerPermissionsDto`. |
| `Areas/Corporate/HumanResource/RecruitmentManagement/Services/IRecruitmentJourneyService.cs` | Baru | Antarmuka `GetApplicationJourneyAsync(Guid applicationId, ClaimsPrincipal user, CancellationToken ct)`. |
| `Areas/Corporate/HumanResource/RecruitmentManagement/Services/RecruitmentJourneyPolicy.cs` | Baru | Helper statik kanonik `IsFastTrackApplication`, `IsEvaluationSubmitted`, `IsTerminalApplication`, `EvaluateOfferPrerequisites`, `EvaluateHandoffPrerequisites`, `MaskNationalIdNumber`, dan `AllocateSequenceAsync`. |
| `Areas/Corporate/HumanResource/RecruitmentManagement/Services/RecruitmentJourneyService.cs` | Baru | Agregasi read-only (`AsNoTracking`) 6 tahap, perhitungan blockers & `AllowedActions`, `RoutePath` deep-link App Router akurat, masker PII/gaji/medis berbasis `AccessPermissionService`, dan deteksi `NeedsOnboardingRecovery`. |
| `Areas/Corporate/HumanResource/RecruitmentManagement/Controllers/CandidateController.cs` | Diubah | Menambahkan `GET /api/v1/corporate/human-resource/recruitment/applications/{applicationId:guid}/journey` dengan `[AccessAction("Read", "Melihat Kandidat", ...)]` dan `[AccessPermission("Candidate", "Read")]`. |
| `Program.cs` | Diubah | Mendaftarkan `IRecruitmentJourneyService` / `RecruitmentJourneyService` pada kontainer DI. |
| `CandidateScreeningService.cs` | Diubah | Guard kandidat terminal/blacklisted, tolak evaluasi ulang screening non-`Pending` (`409`), dan prioritaskan dokumen milik `ApplicationId`. |
| `CandidateEvaluationService.cs` | Diubah | Wajibkan `ScreeningStatus.Passed` (`422`), tolak `ClinicalAssessment` untuk lowongan non-klinis, bedakan terjadwal vs ter-submit (`UpdateDateTime != null`), cegah submit ulang (`409`), dan isolasi veto per lamaran. |
| `JobOfferService.cs` & `JobOfferApprovalService.cs` | Diubah | Terapkan `EvaluateOfferPrerequisites`, cek kuota/status lowongan, `NumberSeriesAllocator` seri `OFR`, tutup bypass `Approved` pada `CandidateRespondAsync`, dan guard status non-terminal di approval. |
| `McuClearanceService.cs` | Diubah | Gunakan `AccessPermissionService`, masking medis untuk non-pemeriksa MCU, paksa `ConditionalAccepted` sebelum register (`HRD-DEC-078`), tahan `FitWithRestrictions`, serta kunci edit sesudah `Unfit`/handoff. |
| `HiringHandoffService.cs` | Diubah | Guard `EvaluateHandoffPrerequisites`, validasi `HeadcountRequestSeat` (`Open`/`Linked`), dedup rehire `MstWorkforceProfile` (`HRD-DEC-075`), transaksi DB + pemulihan idempoten kasus onboarding, dan masker NIK. |
| `TalentPoolService.cs` | Diubah | Alokasi kode atomik `APP` dan `OFR` via `NumberSeriesAllocator` saat `FastTrackPromoteAsync`. |
| `Tests/QuilvianSystemBackend.HumanResourceTests/RecruitmentJourneyWorkflowTests.cs` | Baru | `5` `[Fact]` workflow end-to-end dengan SQLite in-memory (guard evaluasi, veto terisolasi, offer/MCU/handoff/rehire/partial-recovery, masking & `RESTRICTED`, serta verifikasi registri izin controller). |

---

## 3. Dokumentasi Endpoint (Bergaya Swagger)

### `[Tags("Corporate / Human Resource / Recruitment Management / Candidate")]`
Base path: `api/v1/corporate/human-resource/recruitment`

| Method | Path | Deskripsi | Auth (`AccessPermission`) | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/applications/{applicationId:guid}/journey` | Ringkasan perjalanan lamaran 6 tahap (`read-only`, masker PII/medis/gaji & izin subdomain) | `Candidate : Read` (+ cek granular `CandidateScreening`, `CandidateEvaluation`, `JobOffer`, `OccupationalHealthClearance`, `HiringHandoff`, `TalentPool`, `WorkforceOnboardingCase`) | Route `{applicationId:guid}` | `ApiResponse<RecruitmentJourneyResponse>` (`200`, `400`, `403`, `404`) |

---

## 4. Hasil Verifikasi Backend

| Pemeriksaan | Hasil Aktual | Status | Bukti / Scope |
| --- | --- | --- | --- |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` (13 berkas app BE) | `Files evaluated: 13`, `VIOLATION: 0`, `REVIEW: 0`, `INFO: 0` | `PASS` | Dijalankan pasca-edit akhir BE |
| `dotnet build .\QuilvianSystemBackend.csproj --no-restore -o <scratch>/be-run` | `Build succeeded. 0 Error(s)` (`6` NuGet warning baseline `SixLabors.ImageSharp` 3.1.11) | `PASS` | Output DLL dijalankan pada port terisolasi `7194` |
| Startup `PermissionRegistryValidator` (server `7194`) | `Permission registry valid. 2661 identitas kanonik dari 2661 kemampuan terdaftar.` | `PASS` | Log server terisolasi `7194` |
| `dotnet test <scratch>/temp-tests/JrnRunner.csproj` (`RecruitmentJourneyWorkflowTests`) | **`5/5 Passed`** (`41.38s`): guard screening->evaluasi, jadwal belum submit, veto terisolasi, offer/MCU/handoff/rehire/partial-recovery, serta RBAC masking & registri permission | `PASS` (`SQLITE_XUNIT`) | Harness xUnit SQLite in-memory |
| `test-with-agy/scripts/jrn-1125-be-api-test.mjs` (`https://localhost:7194/api`) | **`3/3 PASS`**: `JRN-BE-01` (`swaggerAuth=200`, `candidates=200`), `JRN-BE-02` (`empty=400`, `missing=404`, `asesor=403`), `JRN-BE-03` (`SKIP_DATA`: `0` lamaran di DB dev `QuilvianNewDevHamzah`) | `PASS` (`LIVE_BE` + catatan DB kosong) | `be-api-results.json` |

---

## 5. Catatan Jujur & Batasan

- Proyek `Tests/QuilvianSystemBackend.HumanResourceTests/QuilvianSystemBackend.HumanResourceTests.csproj` secara utuh di `HEAD` gagal kompilasi karena berkas unrelated milik payroll yang rusak pre-existing (`WorkforcePayrollDataSaveTests.cs`, `WorkforcePayrollDataContractTests.cs`). Pengujian xUnit modul ini dijalankan melalui proyek terisolasi sementara `<scratch>/temp-tests/JrnRunner.csproj` tanpa mengubah berkas payroll.
- DB dev `QuilvianNewDevHamzah` hanya memiliki `7` kandidat terdaftar (`Registered`) tanpa lamaran (`0` `HrdJobVacancy` / `HrdCandidateApplication`), sehingga skenario multi-tahap teruji penuh pada layer logika layanan melalui `5` tes xUnit SQLite (`SQLITE_XUNIT`) serta kontrak HTTP `400`/`403`/`404` pada `7194`.
- Daemon sementara port `7194` telah dihentikan bersih; server user pada `7184` dan `3000` tidak disentuh dan belum di-commit/migration.
