# Laporan Perubahan Backend — `BE-HRD-FIN-REC-001` s.d. `BE-HRD-FIN-REC-020`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID / Instruksi | `BE-HRD-FIN-REC-001` s.d. `BE-HRD-FIN-REC-020` (Amandemen Finishing Menu 03 — Rekrutmen & Seleksi) |
| Branch & HEAD SHA | `MHamzah` @ `b1cb2333` (*uncommitted working tree*; tanpa `commit`/`push`/`merge`) |
| Slice canonical existing | `S-D2` — Rekrutmen & Seleksi (melengkapi `BE-HRD-052..064`, `FIN-HRD-REC-WIZ-01`, `FIN-HRD-REC-JRN-01..03`) |
| Rujukan instruksi | [`INSTRUKSI_AGENT_HRD_MENU_03_REKRUTMEN_DAN_SELEKSI.md`](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/docs/module-blueprints/human-resource/docs/instruksi/INSTRUKSI_AGENT_HRD_MENU_03_REKRUTMEN_DAN_SELEKSI.md), [`api-contract.md` §37](file:///c:/Users/Admin/Documents/Quilvian/Qulvian/NewQuilvianSystemBackend/docs/module-blueprints/human-resource/contracts/api-contract.md#L3465-L3498) |
| Target tulis | `Areas/Corporate/HumanResource/RecruitmentManagement/{Controllers,DTOs,Services}`, `Tests/QuilvianSystemBackend.HumanResourceTests/RecruitmentJourneyWorkflowTests.cs`, dokumen blueprint §36/§37/§1aj |
| Entity / Migration baru | **Tidak ada (`NO MIGRATION`)** — item yang membutuhkan perubahan skema EF Core atau kebijakan owner yang belum dikunci tetap dipisahkan sebagai `BLOCKED_SCHEMA_AND_POLICY_GATE` |
| Tanggal | 26 November 2026 |
| Status | 🟡 **IMPLEMENTED (Non-Schema Scope Verified) + `BLOCKED_SCHEMA_AND_POLICY_GATE` pada item schema/policy** — `dotnet build` (`0 Error(s)`), `Invoke-QbeConformanceCheck.ps1 -Mode Strict` (`0 VIOLATION` pada 16 berkas), `8/8` SQLite xUnit (`RecruitmentJourneyWorkflowTests`), dan verifikasi HTTP live `https://localhost:7196` lulus |

---

## 1. Tujuan, Alur Proses Bisnis & Skenario Rumah Sakit

Amandemen Finishing Menu 03 menutup celah kelayakan worklist, sinkronisasi otomatis antar-tahap, isolasi privasi medis/NIK, serta kestabilan endpoint backend agar tampilan *Candidate Workspace* (Gambar 1), *Pemeriksaan Seleksi Administrasi* (Gambar 2), *Wawancara HR / Asesmen Klinis* (Gambar 3), hingga *Handoff Onboarding* dapat dijalankan tanpa melangkahi aturan bisnis RS maupun membuka perubahan skema database tanpa izin.

**Alur Proses Bisnis End-to-End (Trigger → Hasil Akhir)**:
1. **Lowongan → Lamaran & Auto-Screening Atomik (`BE-002`, `BE-003`)**: Saat pelamar mengajukan lamaran (`ApplyVacancyAsync`), layanan memvalidasi status kandidat (menolak kandidat terminal/daftar hitam), menghindari duplikasi lamaran (HTTP `409 Conflict`), lalu memanggil `CandidateScreeningService.InitiateScreeningAsync` di dalam transaksi DB atomik untuk langsung membentuk tiket `Pending` dengan flag `IsStrRequired` mewarisi spesifikasi klinis lowongan.
2. **Rekonsiliasi & Worklist Tanpa Sesi Manual (`BE-004`, `BE-006`)**: Endpoint read-only `GET /screenings/reconcile-preview` mengaudit lamaran aktif yang belum punya tiket screening, sementara worklist `GET /evaluations/eligible-applications`, `GET /offers/eligible-applications`, `GET /mcu-clearance/eligible-offers`, `GET /talent-pool/eligible-applications`, dan `GET /handoffs/eligible-offers` menampilkan kandidat yang sudah memenuhi prasyarat meski petugas belum membuat sesi/transaksi awal secara manual.
3. **Finalisasi Seleksi Administrasi & Evaluasi (`BE-010`, `BE-013`)**: `EvaluateScreeningAsync` mewajibkan dokumen umum (`KTP`, `CV`, `Ijazah`) dan STR aktif/valid untuk lowongan klinis sebelum status `Passed`. Di tahap evaluasi, properti eksplisit `IsEvaluationSubmitted` dan `SessionStatus` mencegah sesi terjadwal dengan skor `0` terbaca keliru sebagai sesi final yang gagal.
4. **Penawaran, Privasi MCU, Talent Pool, & Handoff (`BE-014` s.d. `BE-018`)**: Pemberian penawaran tunduk pada `EvaluateOfferPrerequisites` (lolos screening, lulus evaluasi wajib tanpa veto klinis, sisa kuota aman); pengunduhan sertifikat & catatan dokter MCU dibatasi ketat ke izin `OccupationalHealthClearance:Evaluate`; pendaftaran Talent Pool memverifikasi kuota penuh dan bebas veto; serta promosi *Fast-Track* mengecek retensi, target `Published`, sisa kuota, dan syarat STR klinis sebelum serah terima ke Onboarding.

**Contoh Skenario Konkret Rumah Sakit (RS MMC)**:
Perawat *Aulia Rahma Pratama* (`CAN-20261126-0002`) mendaftar pada lowongan klinis *Perawat IGD* (`APP-20261126-0002`). Sistem otomatis membuka antrean `Pending` seleksi administrasi dengan label klinis `"Menunggu Pemeriksaan STR"`. Petugas HRD umum tidak dapat mengunduh sertifikat MCU miliknya (`403 Forbidden`), dan saat ia lolos screening + evaluasi HR + evaluasi klinis tanpa veto, lamaran otomatis muncul pada `GET /offers/eligible-applications` tanpa harus dicari manual oleh recruiter.

---

## 2. Daftar Berkas Backend yang Diubah (`NewQuilvianSystemBackend`)

| Berkas | Kategori | Ringkasan Perubahan |
| --- | --- | --- |
| `Services/CandidateScreeningService.cs`, `ICandidateScreeningService.cs`, `Controllers/CandidateScreeningController.cs`, `DTOs/CandidateScreeningDtos.cs` | Service / Controller / DTO | Shared initializer `InitiateScreeningAsync`, `GET /screenings/application/{applicationId}`, `GET /screenings/summary`, `GET /screenings/reconcile-preview`, `POST /screenings/reconcile`, label klinis `"Menunggu Pemeriksaan STR"`, validasi wajib KTP/CV/Ijazah + STR (`BE-002`, `BE-004`, `BE-006`, `BE-010`). |
| `Services/CandidateService.cs`, `DTOs/CandidateDtos.cs` | Service / DTO | Transaksi atomik apply + auto-screening, guard terminal/blacklist, deteksi `409 Conflict`, serta enrichment read-only direktori pelamar (`applicationsCount`, `latestApplicationStage`, `documentsCount`, `maskedNationalIdNumber`) (`BE-002`, `BE-003`, `BE-006`). |
| `Services/RecruitmentJourneyPolicy.cs`, `Services/RecruitmentJourneyService.cs`, `DTOs/RecruitmentJourneyDtos.cs` | Policy / Journey | Perbaikan `RoutePath` aksi ke `/offers/{id}` dan `/handoffs/*`, akurasi `NextAction`/`Blockers`, serta penyelarasan guard kelayakan offer & handoff (`BE-005`, `BE-014`). |
| `Services/CandidateEvaluationService.cs`, `ICandidateEvaluationService.cs`, `Controllers/CandidateEvaluationController.cs`, `DTOs/CandidateEvaluationDtos.cs` | Service / Controller / DTO | Endpoint `GET /evaluations/eligible-applications`, properti eksplisit `IsEvaluationSubmitted` & `SessionStatus`, penguatan validasi submit tanpa ketergantungan skema rubrik baru (`BE-006`, `BE-013`). |
| `Services/JobOfferService.cs`, `IJobOfferService.cs`, `Controllers/JobOfferController.cs`, `DTOs/JobOfferDtos.cs`, `Services/JobVacancyService.cs` | Service / Controller / DTO | Endpoint `GET /offers/eligible-applications`, penguatan prasyarat `EvaluateOfferPrerequisites`, validasi kuota & anti-duplikasi offer (`BE-006`, `BE-014`). |
| `Services/McuClearanceService.cs`, `IMcuClearanceService.cs`, `Controllers/McuClearanceController.cs`, `DTOs/McuClearanceDtos.cs` | Service / Controller / DTO | Endpoint `GET /mcu-clearance/eligible-offers` dan pembatasan ketat pengunduhan sertifikat medis ke izin `OccupationalHealthClearance:Evaluate` (`BE-006`, `BE-015`, `BE-019`). |
| `Services/TalentPoolService.cs`, `ITalentPoolService.cs`, `Controllers/TalentPoolController.cs`, `DTOs/TalentPoolDtos.cs` | Service / Controller / DTO | Endpoint `GET /talent-pool/eligible-applications`, penguatan kelayakan `EnrollAsync`, dan validasi ketat `FastTrackPromoteAsync` (retensi, target `Published`, kuota, STR klinis, anti-duplikasi) (`BE-016`, `BE-017`). |
| `Services/HiringHandoffService.cs`, `IHiringHandoffService.cs`, `Controllers/HiringHandoffController.cs`, `DTOs/HiringHandoffDtos.cs` | Service / Controller / DTO | Endpoint `GET /handoffs/eligible-offers` dan penguatan alur serah terima & pemulihan `NeedsOnboardingRecovery` (`BE-006`, `BE-018`). |
| `Tests/QuilvianSystemBackend.HumanResourceTests/RecruitmentJourneyWorkflowTests.cs` | Integrasi Test | Penyesuaian pemanggilan initializer screening (`screeningSvc.InitiateScreeningAsync`) dan total `8` `[Fact]` SQLite in-memory (`BE-002..020`). |
| `docs/module-blueprints/human-resource/{contracts/api-contract.md,roadmap/backend-roadmap.md,roadmap/frontend-roadmap.md,roadmap/requirement-traceability.md}` | Blueprint & Kontrak | Sinkronisasi kontrak `v31` §37, amandemen §36 BE/FE, dan traceability Bagian 1aj (`BE-001`). |

---

## 3. Dokumentasi Endpoint (Bergaya Swagger — Aditif `v31`, `NO MIGRATION`)

Base path: `api/v1/corporate/human-resource/recruitment`

### `[Tags("Corporate / Human Resource / Recruitment Management / Candidate Screening")]`
| Method | Path | Deskripsi | Auth (`AccessPermission`) | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/screenings/application/{applicationId:guid}` | Mengambil detail screening & ringkasan dokumen pelamar langsung per `ApplicationId` | `CandidateScreening : Read` | Route `{applicationId:guid}` | `200 ApiResponse<CandidateScreeningResponse>`, `404` |
| `GET` | `/screenings/summary` | Statistik worklist screening (`pendingCount`, `passedCount`, `rejectedCount`, `clinicalPendingCount`, `missingScreeningEligibleCount`) | `CandidateScreening : Read` | Query opsional | `200 ApiResponse<ScreeningSummaryResponse>` |
| `GET` | `/screenings/reconcile-preview` | Dry-run read-only melihat lamaran `Screening`/`Submitted` aktif tanpa tiket screening | `CandidateScreening : Read` | — | `200 ApiResponse<ScreeningReconcilePreviewResponse>` |
| `POST` | `/screenings/reconcile` | Rekonsiliasi idempoten membuat screening `Pending` untuk lamaran aktif yang belum berscreening | `CandidateScreening : Evaluate` | Body kosong / opsional | `200 ApiResponse<ScreeningReconcileResultResponse>`, `403`, `409` |

### `[Tags("Corporate / Human Resource / Recruitment Management / Candidate Evaluation")]`
| Method | Path | Deskripsi | Auth (`AccessPermission`) | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/evaluations/eligible-applications` | Antrean lamaran lolos administrasi beserta kesiapan sesi HR & Klinis walau sesi belum dibuat | `CandidateEvaluation : Read` | Query filter track/status & paging | `200 ApiResponse<PagedResult<EvaluationEligibleApplicationResponse>>` |

### `[Tags("Corporate / Human Resource / Recruitment Management / Job Offer")]`
| Method | Path | Deskripsi | Auth (`AccessPermission`) | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/offers/eligible-applications` | Lamaran layak penawaran (lolos screening + evaluasi wajib tanpa veto + bebas offer aktif) | `JobOffer : Read` | — | `200 ApiResponse<List<EligibleOfferApplicationResponse>>` |

### `[Tags("Corporate / Human Resource / Recruitment Management / MCU Clearance")]`
| Method | Path | Deskripsi | Auth (`AccessPermission`) | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/mcu-clearance/eligible-offers` | Penawaran `ConditionalAccepted` yang belum memiliki rujukan MCU | `OccupationalHealthClearance : Read` | — | `200 ApiResponse<List<EligibleMcuOfferResponse>>` |
| `GET` | `/mcu-clearance/certificates/attachments/download` | Unduh sertifikat medis MCU khusus pemeriksa berhak (pengetatan dari `Read` ke `Evaluate`) | `OccupationalHealthClearance : Evaluate` | Query `filePath` | `200 FileStreamResult`, `403`, `404` |

### `[Tags("Corporate / Human Resource / Recruitment Management / Talent Pool & Hiring Handoff")]`
| Method | Path | Deskripsi | Auth (`AccessPermission`) | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/talent-pool/eligible-applications` | Lamaran layak masuk Talent Pool cadangan (kuota lowongan penuh, lolos syarat, bebas veto) | `TalentPool : Read` | — | `200 ApiResponse<List<EligibleTalentPoolApplicationResponse>>` |
| `GET` | `/handoffs/eligible-offers` | Penawaran `FinalAccepted` + MCU `Fit` yang belum memiliki handoff aktif atau perlu recovery | `HiringHandoff : Read` | — | `200 ApiResponse<List<EligibleHandoffOfferResponse>>` |

---

## 4. Rekonsiliasi Status `BE-HRD-FIN-REC-001..020` & Gate Schema/Policy

| Kelompok ID FIN | Status Akhir | Keterangan Bukti / Gate |
| --- | --- | --- |
| `BE-HRD-FIN-REC-001`, `002`, `003`, `005`, `006`, `017`, `018`, `019`, `020` | ✅ **IMPLEMENTED** | Seluruh logika layanan non-schema, journey fix, eligible worklist, fast-track guards, handoff recovery, RBAC/privacy, dan suite verifikasi terbukti lulus. |
| `BE-HRD-FIN-REC-004` | 🟡 **IMPLEMENTED (`Preview`; Backfill DB Pending Write Auth)** | `GET /screenings/reconcile-preview` lulus `200 OK` live; `POST /screenings/reconcile` hanya diuji pada SQLite & RBAC `403` tanpa mengubah DB dev nyata. |
| `BE-HRD-FIN-REC-010`, `013`, `014`, `015`, `016` | 🟡 **PARTIAL (Implemented Non-Schema Scope)** | Gate KTP/CV/Ijazah/STR, `IsEvaluationSubmitted`, prasyarat offer, privasi medis MCU (`403` untuk recruiter biasa), dan kelayakan Talent Pool diimplementasikan. Bagian yang menunggu policy owner atau schema baru tetap terkunci (`BE-011/012` rubrik, mandat teknis non-klinis, 4-enum MCU & konfirmasi restriksi, formula ranking Talent Pool). |
| `BE-HRD-FIN-REC-007`, `008`, `009`, `011`, `012` | 🔒 **`BLOCKED_SCHEMA_AND_POLICY_GATE`** | Membutuhkan tabel/kolom EF Core migration baru (`VacancyRequirementTemplate`, `ApplicationRequirementSnapshot`, `ScreeningChecklistItem`, `EvaluationRubricTemplate`, `EvaluationSessionScoreDetail`) serta pengesahan parameter/kebijakan oleh Owner HRD. |

---

## 5. Hasil Verifikasi Backend

| Pemeriksaan | Hasil Aktual | Status | Scope & Catatan Jujur |
| --- | --- | --- | --- |
| `dotnet build .\QuilvianSystemBackend.csproj --no-restore` (ke `<scratch>/be-build-out`) | `Build succeeded. 0 Error(s), 610 Warning(s)` | `PASS` | Output kompilasi dijalankan pada port terisolasi `7196`. |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` (16 berkas controller & service Rekrutmen) | `VIOLATION: 0, REVIEW: 0, INFO: 0, Final result: PASS` | `PASS` | Lulus pasca-edit akhir BE. |
| xUnit SQLite in-memory via `<scratch>/RecruitmentRunner/RecruitmentRunner.csproj` (`RecruitmentJourneyWorkflowTests`) | **`Total tests: 8, Passed: 8, Failed: 0`** (`1,1522 Minutes`, `Build succeeded, 0 Warning, 0 Error`) | `PASS` (`SQLITE_XUNIT`) | Proyek utuh `QuilvianSystemBackend.HumanResourceTests.csproj` gagal kompilasi karena `6` error CS pre-existing di `WorkforcePayrollDataContractTests.cs` & `WorkforcePayrollDataSaveTests.cs` (tidak disentuh). |
| Verifikasi HTTP API Live DB Dev (`scratch/be-verify-7196.mjs` pada `https://localhost:7196`) | `ALL LIVE BE-7196 VERIFICATION PASSED`: `GET /candidates` `200 OK` (NIK `9999********0002`), `GET /applications/842c0d02-298f-407d-a369-ec7ef274a6d5/journey` `200 OK` (6 tahap), `/screenings/application/{id}` `200`, `/screenings/summary` `200`, `/screenings/reconcile-preview` `200`, `/evaluations/eligible-applications` `200` (`CAN-20261126-0002`), `/offers/eligible-applications` `200`, `/mcu-clearance/eligible-offers` `200`, `/talent-pool/eligible-applications` `200`, `/handoffs/eligible-offers` `200`, RBAC negatif `unauth: 401`, `asesorDenyOffer: 403`, `asesorDenyReconcile: 403`, `recDenyMcuDl: 403` | `PASS` (`LIVE_BE`) | Proses server terisolasi `task-794` dihentikan bersih sesudah UAT selesai; DB nyata `QuilvianNewDevHamzah` tetap bebas mutasi tulis. |

---

## 6. Risiko, Keputusan Owner Pending & Handoff

- **Keputusan Owner Pending (`BLOCKED_SCHEMA_AND_POLICY_GATE`)**: (1) pengecualian multi-dokumen non-STR & tambahan berkas perpanjangan (`BE-009/010`), (2) bobot, ambang kelulusan & pembulatan rubrik penilaian (`BE-011/012`), (3) mandat asesmen teknis non-klinis sebelum penawaran (`BE-014`), (4) pemecahan `Unfit` 4-nilai & alur konfirmasi `FitWithRestrictions` (`BE-015`), dan (5) rumus ranking + *tie-break* Talent Pool (`BE-016`).
- **Status Git & DB**: Tidak ada `git commit`, `git push`, `merge`, pembuatan EF migration baru, ataupun mutasi tulis ke database dev `QuilvianNewDevHamzah`. Checkpoint memory: `MEMORY_PENDING_SYNC` di luar lingkup repository.
