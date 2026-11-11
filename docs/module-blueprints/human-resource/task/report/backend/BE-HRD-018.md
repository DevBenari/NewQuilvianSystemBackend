# Laporan Perubahan Backend — `BE-HRD-018`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-018` |
| Judul | Fondasi Skema, 12 Model Hrd, Enum, dan Migration Pembersihan 20 Tabel Legacy dengan EMPTY_TABLE_GUARD |
| Slice | `S-D2` (Rekrutmen & Hiring / Recruitment Management) |
| Milestone | `MVP-11` (Fondasi, Requisition, Lowongan & Direktori Pelamar) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang MVP-11 |
| Traceability | `HRD-DEC-073`, `HRD-DEC-074`, `HRD-DEC-075`, `02-backend-architecture.md` §17.4–§17.7, `Data Dictionary v8` Bagian 9 |
| Dependency | `BE-HRD-017` ✅ |
| Klasifikasi | `HEAVY` |
| Task Mode | `BACKEND` |
| Target Tulis | Models, Enums, EF Core Configurations, ApplicationDbContext, Migrations, Laporan Task, Roadmap |
| Tanggal | 04 November 2026 |
| Status | Selesai (`PASS`) |

---

## 1. Ringkasan dan Masalah yang Diselesaikan

Sebelumnya, sub-modul Rekrutmen dan Hiring menyimpan 20 model tabel legacy (`Mst*` dan `Trx*`) yang belum terstandarisasi dengan arsitektur Quilvian terkini, tidak memiliki relasi formal ke perencanaan formasi (*Workforce Planning*), dan tidak mendukung kebutuhan rekrutmen rumah sakit modern seperti penapisan Surat Tanda Registrasi (STR) klinis dan pemeriksaan MCU pra-kerja K3RS.

Melalui task `BE-HRD-018`, fondasi persistensi relasional untuk slice `S-D2` resmi dibangun secara bersih dengan:
1. Mematuhi keputusan arsitektur `HRD-DEC-073` & `HRD-DEC-074`: Membangun 12 entitas model ber-prefix kanonikal `Hrd*` pada namespace `Areas/Corporate/HumanResource/RecruitmentManagement/Models/`.
2. Menyediakan 15 enum domain rekrutmen lengkap untuk status alur kerja, jenis dokumen, kelaikan klinis, hingga tingkatan persetujuan penawaran.
3. Menggantikan 20 konfigurasi legacy dengan 12 konfigurasi EF Core baru lengkap dengan composite unique index, foreign key, dan kendali konkurensi token `RowVersion`.
4. Mendaftarkan seluruh 12 `DbSet<Hrd*>` pada `ApplicationDbContext` dan membersihkan pendaftaran legacy.
5. Menyediakan berkas migrasi EF Core `20261104113000_AddRecruitmentManagementCoreAndRetireLegacy.cs` dengan pengaman ketat `EMPTY_TABLE_GUARD` yang memastikan 20 tabel legacy benar-benar kosong (0 baris) sebelum di-drop, fail-closed bila ada data.

---

## 2. Alur Proses Bisnis Rumah Sakit yang Didukung

* **Tujuan**: Membangun fondasi relasional yang kokoh untuk mendukung seluruh siklus rekrutmen tenaga kerja rumah sakit dari hulu ke hilir:
  - **Job Requisition**: Permintaan rekrutmen resmi unit medis/keperawatan yang terikat ke sisa kuota kursi eksternal pada formasi WFP.
  - **Job Vacancy**: Pembukaan lowongan kerja publik/internal dengan penanda peran klinis `IsClinical` yang menentukan kewajiban STR.
  - **Candidate Directory**: Registrasi pelamar ber-NIK KTP valid dan deteksi rehire mantan karyawan.
  - **Screening & Clinical Test**: Penapisan berkas STR dan uji klinis dengan hak veto mutlak komite medik.
  - **Conditional Job Offer**: Surat penawaran kerja bersyarat dengan batas plafon gaji.
  - **Tiered Offer Approval**: Persetujuan penawaran bertingkat (T13 Kadiv SDM vs T14 Direktur).
  - **Pre-employment MCU**: Pemeriksaan kesehatan pra-kerja dengan masking privasi diagnosa medis oleh K3RS.
  - **Hiring Handoff**: Serah terima calon karyawan yang telah fit ke modul Onboarding.

---

## 3. Rincian Perubahan Teknis

### 3.1 Lima Belas Enum Domain (`RecruitmentManagement/Enums/`)
Dibuat di direktori `Areas/Corporate/HumanResource/RecruitmentManagement/Enums/`:
1. `JobRequisitionStatus.cs`: `Draft`, `Submitted`, `Approved`, `Rejected`, `Cancelled`, `Closed`.
2. `JobVacancyStatus.cs`: `Draft`, `Published`, `Closed`, `Cancelled`.
3. `CandidateStatus.cs`: `Registered`, `Active`, `Hired`, `Rejected`, `Withdrawn`, `TalentPool`.
4. `ApplicationStage.cs`: `Screening`, `Interview`, `ClinicalTest`, `Offering`, `MedicalCheck`, `Handoff`, `Completed`, `Rejected`.
5. `DocumentType.cs`: `Ktp`, `Cv`, `Ijazah`, `Transkrip`, `Str`, `Sip`, `SertifikatPelatihan`, `SuratPengalamanKerja`, `Skck`, `PasFoto`, `SuratBebasNarkoba`, `Lainnya`.
6. `ScreeningStatus.cs`: `Pending`, `Passed`, `Rejected`.
7. `ClinicalScreeningResult.cs`: `NotApplicable`, `Valid`, `Expired`, `FakeOrInvalid`.
8. `EvaluationType.cs`: `HrInterview`, `UserInterview`, `ClinicalCompetencyTest`, `PsychologicalTest`.
9. `EvaluationRecommendation.cs`: `Recommended`, `RecommendedWithCondition`, `NotRecommended`.
10. `JobOfferStatus.cs`: `Draft`, `PendingApproval`, `Approved`, `Sent`, `Accepted`, `Rejected`, `Expired`, `Negotiation`.
11. `JobOfferApprovalTier.cs`: `Tier1HrHead`, `Tier2Director`.
12. `ApprovalStatus.cs`: `Pending`, `Approved`, `Rejected`.
13. `McuClearanceResult.cs`: `Fit`, `FitWithRestrictions`, `Unfit`, `PendingFurtherTests`.
14. `TalentPoolStatus.cs`: `Active`, `Invited`, `Hired`, `Archived`.
15. `HandoffStatus.cs`: `Draft`, `TransferredToOnboarding`, `Completed`.

### 3.2 Dua Belas Model Entitas Domain (`RecruitmentManagement/Models/`)
1. `HrdJobRequisition.cs`: Permintaan penambahan pegawai dari alokasi formasi WFP.
2. `HrdJobVacancy.cs`: Lowongan kerja yang diterbitkan lengkap dengan kuota kursi dan flag `IsClinical`.
3. `HrdCandidate.cs`: Data master kandidat dengan NIK KTP, kontak, dan flag `IsRehire`.
4. `HrdCandidateApplication.cs`: Riwayat lamaran pekerjaan per lowongan dan tahapan seleksi aktif.
5. `HrdCandidateDocument.cs`: Berkas dokumen pelamar (CV, Ijazah, STR, dll).
6. `HrdCandidateScreening.cs`: Hasil penapisan berkas administrasi dan validitas STR.
7. `HrdCandidateEvaluation.cs`: Penilaian wawancara HR, user, dan uji kompetensi klinis.
8. `HrdJobOffer.cs`: Surat penawaran kerja bersyarat dengan rincian komponen gaji.
9. `HrdJobOfferApproval.cs`: Jejak persetujuan penawaran bertingkat (T13 vs T14).
10. `HrdPreEmploymentMedicalCheck.cs`: Rekomendasi kelaikan kerja pra-kerja K3RS (dengan masking diagnosa klinis).
11. `HrdTalentPool.cs`: Bank talenta kandidat cadangan berpotensi tinggi.
12. `HrdCandidateHiringHandoff.cs`: Dokumen serah terima kandidat yang diterima ke modul Onboarding.

### 3.3 Konfigurasi EF Core & ApplicationDbContext
1. 12 file konfigurasi EF Core di `Repositories/Configurations/Corporate/HumanResource/RecruitmentManagement/`.
2. Didaftarkan di `ApplicationDbContext.cs` via `ApplyConfigurationsFromAssembly`.
3. 12 properti `DbSet<Hrd*>` menggantikan 20 `DbSet` legacy.

### 3.4 Migrasi Database (`20261104113000_AddRecruitmentManagementCoreAndRetireLegacy.cs`)
- Dilengkapi blok SQL verifikasi `EMPTY_TABLE_GUARD` untuk memastikan tabel legacy kosong sebelum di-drop.
- Menghasilkan pembuatan 12 tabel baru dengan indeks unik:
  - `IX_HrdCandidate_NationalIdNumber` (Filter `IsDelete = false`)
  - `IX_HrdJobRequisition_RequisitionNumber`
  - `IX_HrdJobVacancy_VacancyNumber`
  - `IX_HrdJobOffer_OfferNumber`

---

## 4. Bukti Verifikasi
- 12 Model Hrd, 15 Enum, 12 Konfigurasi EF Core, dan berkas migrasi telah tersedia secara lengkap.
- Seluruh referensi tipe di entitas terkait (`TrxEmployeeOnboarding` & `WfpOnboardingChecklist`) telah disinkronkan ke `HrdCandidateHiringHandoff`.
