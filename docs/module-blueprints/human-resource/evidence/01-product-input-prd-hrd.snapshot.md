# PRD to MVP - Modul HRD Rumah Sakit Quilvian V2

**Target:** 100% Functional Coverage untuk baseline HRD rumah sakit yang disepakati  
**Tanggal baseline:** 26 Agustus 2026  
**Status dokumen:** Product & MVP Baseline  
**Target backend:** `DevBenari/NewQuilvianSystemBackend` branch `master`  
**Evidence backend legacy:** `DevBenari/QuilvianSystemBackendDev` branch `QuilvianSta`  
**Evidence frontend:** `DevBenari/QuilvianSystemFrontendDev` branch `QuilvianDevV2`

---

## 1. Ringkasan Eksekutif

Quilvian V2 sudah mempunyai fondasi HRD yang luas dan hospital-specific. Backend baru sudah memisahkan domain Attendance, Leave, Overtime, Payroll, Benefit, Credentialing, Learning & Development, Performance, Occupational Health, Recruitment, Lifecycle, Workforce Planning, Workforce Core, Workflow, Employee Relation, HR Service, Expense, Business Travel, dan domain pendukung lain.

Target PRD ini bukan membuat HRD baru dari nol. Strateginya adalah:

1. **REUSE_EXISTING** - mempertahankan domain, entity, policy, workflow, menu, dan API yang sudah memadai.
2. **EXTEND_EXISTING** - menyelesaikan model/domain yang sudah tersedia tetapi belum mempunyai workflow API/UI end-to-end.
3. **NEW** - hanya menambahkan capability yang belum terbukti tersedia namun diperlukan untuk menutup baseline HRD rumah sakit, terutama OPPE/FPPE, evidence akreditasi, dan integrasi credential-to-practice.

Target "100%" pada dokumen ini berarti **100% capability fungsional yang ditetapkan dalam PRD ini mempunyai alur end-to-end, owner, UI, API, persistence, audit, approval bila diperlukan, dan acceptance test**, serta seluruh KPS 1-19 mempunyai traceability ke capability sistem. Target ini **bukan pernyataan otomatis lulus akreditasi** dan bukan pengganti validasi SOP, regulasi ketenagakerjaan, perpajakan, profesi, dan kebijakan rumah sakit yang berlaku.

### Baseline penilaian

- Existing functional coverage: sekitar 83%.
- Existing operational readiness: sekitar 65-70% karena backend lebih matang daripada route/workspace frontend operasional.
- Target setelah MVP ini: 100% terhadap scope PRD yang didefinisikan di dokumen ini.

---

## 2. Masalah yang Diselesaikan

Saat ini fondasi data HRD V2 sudah luas, tetapi beberapa domain belum mempunyai alur produksi lengkap. Contoh utama:

- Workforce Planning mempunyai model staffing/manpower tetapi belum terlihat controller operasional lengkap.
- Recruitment mempunyai model candidate, screening, interview, job offer, hiring, dan medical check tetapi belum terlihat controller/service end-to-end pada struktur modul.
- Lifecycle sudah memodelkan onboarding, probation, termination, retirement, separation, exit clearance, asset return, dan offboarding, tetapi controller yang terlihat masih berfokus pada resignation.
- Learning & Development dan Performance mempunyai model yang lebih luas daripada API yang tersedia.
- Credentialing sudah kuat, tetapi OPPE/FPPE belum terbukti sebagai workflow HRD tersendiri.
- Frontend sidebar HR sudah kaya, tetapi `src/app/hr` yang terverifikasi saat baseline baru menunjukkan `master-data`, sehingga banyak capability operasional belum mempunyai workspace route yang siap dipakai.

Dampak bisnis bila gap ini tidak ditutup:

- proses hire-to-retire tidak dapat dilaksanakan dalam satu sistem;
- staffing rumah sakit tidak dapat ditelusuri dari kebutuhan unit sampai tenaga terisi;
- credentialing belum sepenuhnya terhubung ke evaluasi praktik berkelanjutan;
- evidence KPS tersebar atau harus disiapkan manual;
- attendance/leave/overtime/payroll berpotensi tidak mempunyai satu reconciliation chain yang utuh di UI;
- model backend yang sudah dibuat tidak menghasilkan nilai operasional bila tidak diaktifkan sebagai use case.

---

## 3. Tujuan Produk

### 3.1 Tujuan utama

Membentuk Modul HRD Quilvian sebagai **single operational HR platform rumah sakit** untuk seluruh siklus tenaga kerja:

`Perencanaan Tenaga -> Rekrutmen -> Hiring -> Onboarding -> Penempatan -> Jadwal -> Kehadiran -> Cuti/Lembur -> Payroll -> Kompetensi -> Kinerja -> Credentialing -> Kesehatan Staf -> Lifecycle -> Offboarding`

### 3.2 Outcome yang wajib tercapai

1. Satu profil pegawai menjadi source of truth kepegawaian.
2. Kebutuhan tenaga per unit dapat direncanakan, disetujui, dan dikonversi menjadi requisition.
3. Kandidat dapat diproses sampai hiring tanpa keluar dari workflow HRD.
4. Pegawai baru dapat di-onboard, diorientasikan, ditempatkan, dan dievaluasi masa percobaan.
5. Jadwal, attendance, leave, overtime, dan payroll terintegrasi dan dapat direkonsiliasi.
6. Dokter, perawat, dan tenaga kesehatan lain mempunyai credential, license, certification, privilege/SPK/RKK, monitoring expiry, dan recredentialing.
7. OPPE dan FPPE tersedia untuk kebutuhan evaluasi praktik profesional tenaga medis dan dapat mempengaruhi privilege/recredentialing melalui proses yang terkontrol.
8. Kompetensi, training, training wajib, sertifikat, dan development plan terdokumentasi.
9. Performance review berlaku untuk staf klinis dan nonklinis dengan kontribusi mutu/keselamatan bila relevan.
10. Kesehatan dan keselamatan staf mencakup MCU, vaksinasi, health surveillance, needle stick, occupational exposure, injury, fitness-to-work, restriction, return-to-work, serta tindak lanjut keselamatan staf.
11. Employee/manager self-service dan approval inbox tersedia.
12. Seluruh transaksi sensitif mempunyai role authorization, audit trail, attachment/evidence, dan histori keputusan.
13. Evidence KPS dapat ditelusuri dan diekspor tanpa pengumpulan manual lintas modul.

---

## 4. Non-Goals MVP

Capability berikut tidak menjadi syarat target 100% PRD ini kecuali kemudian ditetapkan sebagai requirement rumah sakit:

- AI automatic hiring decision atau AI candidate scoring final;
- predictive workforce forecasting berbasis machine learning;
- native mobile application khusus HR;
- firmware/driver mesin fingerprint atau biometric vendor;
- integrasi otomatis ke semua portal pemerintah bila API resmi tidak tersedia;
- learning content authoring seperti LMS enterprise penuh;
- succession planning/talent marketplace tingkat enterprise;
- global multi-country payroll.

Item tersebut dapat menjadi post-MVP tanpa mengurangi 100% coverage terhadap scope PRD ini.

---

## 5. Aktor dan Role

| Aktor | Tanggung jawab utama |
|---|---|
| HR Admin | master pegawai, organisasi, lifecycle, administrasi HR |
| HR Manager | approval, workforce plan, policy, audit, exception |
| Recruiter | requisition, vacancy, candidate pipeline, hiring |
| Kepala Unit / Manager | kebutuhan tenaga, jadwal, approval, evaluasi tim |
| Pegawai | self-service, profile change, leave/overtime request, dokumen |
| Payroll Officer | payroll period, input reconciliation, calculation, finalization |
| Finance | settlement/handoff payroll sesuai ownership yang disepakati |
| L&D / Diklat | competency, mandatory training, training plan, certificate |
| Komite Medik / Subkomite Kredensial | credentialing, privilege, recredentialing dokter |
| Komite Keperawatan | credentialing dan kewenangan klinis perawat |
| Credentialing Owner Nakes Lain | credentialing tenaga kesehatan lain sesuai tata kelola RS |
| K3RS / Occupational Health | health surveillance, exposure, injury, vaccination, fitness-to-work |
| PPI | kolaborasi exposure infeksi, vaksinasi/prophylaxis, follow-up |
| Employee Relation / HRBP | grievance, violation, sanction, disciplinary action |
| Auditor / Akreditasi | read-only evidence, report, traceability |
| IT / Administrator | role/access, technical configuration, audit support |

---

## 6. Evidence Existing dan Arah Reuse

### 6.1 Frontend V2

Existing navigation yang harus dipertahankan verbatim:

- `Sumber Daya Manusia`
  - `Master Data`
  - `Administrasi Kepegawaian`
- `Layanan Kepegawaian`
  - `Akun`
  - `Karyawan`
  - `Manajer`

Existing `Master Data` sudah mempunyai banyak child capability, antara lain:

- `Struktur Organisasi`
- `Dokter`
- `Karyawan`
- `Kompetensi`
- `Jadwal Kerja`
- `Kebutuhan Tenaga Kerja`
- payroll/benefit master
- `Katalog Kewenangan Klinis`
- `Persyaratan Kredensial`
- `Profesi`
- `Jenis Sertifikasi`
- `Jenis Lisensi`
- `Spesialisasi`
- training master
- performance master
- leave/overtime policy
- workflow dan approval master.

Existing `Administrasi Kepegawaian` sudah mencantumkan:

- `Perubahan Data Karyawan`
- `Penempatan Organisasi`
- `Penempatan Jabatan`
- `Relasi Atasan`
- `Riwayat Kepegawaian`
- `Penetapan Gaji`

**Gap frontend penting:** sidebar sudah mendefinisikan capability lebih luas, tetapi route `src/app/hr` yang terverifikasi pada baseline baru berisi folder `master-data`. Karena itu semua menu operasional pada MVP harus mempunyai route/page nyata, bukan menu placeholder.

### 6.2 Backend Legacy V1

`Areas/HRD` pada V1 terutama berisi:

- `MasterData`
- `Pengajuan`

Legacy menyediakan evidence/reuse untuk antara lain:

- Karyawan dan dokumen karyawan;
- department/posisi/grade;
- jenis cuti, sisa cuti;
- jenis lembur;
- pengajuan cuti;
- pengajuan lembur;
- pengajuan rekrutmen;
- pengajuan resign;
- ticket/request sederhana.

**Keputusan target:** V1 digunakan sebagai evidence, sumber data migrasi, dan referensi behavior yang masih benar. V1 bukan arsitektur target.

### 6.3 Backend V2

Backend V2 menjadi source target. Domain yang sudah terlihat antara lain:

- AttendanceManagement
- BenefitManagement
- BusinessTravelManagement
- CredentialingManagement
- EmployeeRelationManagement
- ExpenseManagement
- HrServiceManagement
- LearningAndDevelopment
- LeaveManagement
- LifecycleManagement
- MasterData
- OccupationalHealthManagement
- OvertimeManagement
- PayrollManagement
- PerformanceManagement
- RecruitmentManagement
- SchedulingManagement
- WorkflowManagement
- WorkforceCore
- WorkforcePlanning
- WorkforceProfileManagement

---

## 7. Definisi Target 100%

MVP dinyatakan mencapai target 100% hanya jika seluruh gate berikut lulus:

1. Setiap capability pada Section 10 mempunyai UI operasional atau interface yang memang ditetapkan headless/integration-only.
2. Setiap UI mempunyai API yang dapat menyelesaikan main flow dan exception flow minimum.
3. Setiap data transaksional mempunyai persistence yang terbukti dan migration/configuration yang valid.
4. Setiap flow approval menggunakan workflow bersama bila applicable.
5. Setiap transaksi sensitif menyimpan actor, timestamp, status history, reason, dan audit trail.
6. Tidak ada menu MVP yang hanya tampil di sidebar tanpa route/page yang bekerja.
7. Tidak ada entity/model MVP yang hanya menjadi schema tanpa use case operasional.
8. Setiap KPS 1-19 mempunyai mapping capability dan evidence source.
9. Credentialing mencakup dokter, perawat, dan tenaga kesehatan lain sesuai scope RS.
10. OPPE, FPPE, dan recredentialing tenaga medis mempunyai workflow yang dapat diaudit.
11. Attendance, leave, overtime, allowance, dan data relevan dapat direkonsiliasi sebelum payroll final.
12. Employee file menyatukan evidence kompetensi, orientasi, job/assignment, performance, training, credential, dan health evidence dengan akses yang benar.
13. Privacy untuk health record dan dokumen sensitif diuji.
14. UAT end-to-end untuk seluruh golden flow dan exception critical lulus.
15. Tidak ada blocker severity Critical/High yang terbuka pada go-live gate.

---

## 8. Struktur Menu Target

### A. Menu Tersedia - REUSE

1. `Master Data`
2. `Administrasi Kepegawaian`
3. `Layanan Kepegawaian` sebagai supporting employee/manager self-service area.

### B. Submenu/Capability Tersedia - REUSE

Di bawah `Master Data`, pertahankan seluruh label existing yang relevan, termasuk `Struktur Organisasi`, `Karyawan`, `Dokter`, `Kompetensi`, `Jadwal Kerja`, `Kebutuhan Tenaga Kerja`, master payroll/benefit, credentialing, training, performance, leave/overtime, dan workflow.

Di bawah `Administrasi Kepegawaian`, pertahankan:

- `Perubahan Data Karyawan`
- `Penempatan Organisasi`
- `Penempatan Jabatan`
- `Relasi Atasan`
- `Riwayat Kepegawaian`
- `Penetapan Gaji`

### C. Menu Belum Tersedia / Usulan di bawah `Sumber Daya Manusia`

Menu operasional berikut diusulkan sebagai sibling dari `Master Data` dan `Administrasi Kepegawaian`. Nama ini adalah target proposal dan tidak mengganti label existing:

1. `Perencanaan Tenaga Kerja`
2. `Rekrutmen`
3. `Onboarding & Lifecycle`
4. `Jadwal & Kehadiran`
5. `Cuti & Izin`
6. `Lembur`
7. `Payroll & Benefit`
8. `Kredensial & Kewenangan Klinis`
9. `Kompetensi & Pelatihan`
10. `Kinerja`
11. `Kesehatan & Keselamatan Staf`
12. `Hubungan Karyawan`
13. `Layanan HR`
14. `Perjalanan Dinas & Expense`
15. `HR Analytics & Compliance`

### D. Submenu Usulan Utama

Contoh materialisasi child capability:

- `Perencanaan Tenaga Kerja`
  - Rencana Manpower Tahunan
  - Kebutuhan Staf Harian/Shift
  - Analisis Gap Staffing
  - Permintaan Headcount
  - Alokasi Workforce
- `Rekrutmen`
  - Job Requisition
  - Vacancy
  - Candidate Pipeline
  - Screening & Assessment
  - Interview
  - Reference/Background Check
  - Pre-Employment Medical Check
  - Job Offer
  - Hiring
- `Onboarding & Lifecycle`
  - Onboarding
  - Orientasi
  - Probation Review
  - Perubahan Status/Assignment
  - Resignation
  - Termination/Retirement
  - Exit Clearance
  - Offboarding
- `Kredensial & Kewenangan Klinis`
  - Credential & License
  - Primary Source Verification
  - Clinical Privilege
  - SPK/RKK
  - OPPE
  - FPPE
  - Recredentialing
  - Compliance Alert
- `Kesehatan & Keselamatan Staf`
  - Medical Examination
  - Vaccination
  - Health Surveillance
  - Needle Stick / Occupational Exposure
  - Injury
  - Fitness to Work
  - Work Restriction
  - Return to Work
  - Staff Safety Follow-up

---

## 9. Product Principles

1. **Reuse first.** Jangan membuat entity/workflow baru jika existing dapat diperluas secara aman.
2. **Backend-first untuk domain contract.** Business rule dan lifecycle harus stabil sebelum FE mengunci behavior.
3. **One workflow engine.** Approval HR memakai WorkflowManagement bersama, bukan approval custom per fitur.
4. **One employee identity.** Semua domain mengacu pada workforce/employee identity yang sama.
5. **Effective-dated changes.** Perubahan organisasi, jabatan, gaji, credential, privilege, policy, dan assignment yang berdampak historis harus mempertahankan effective date/history.
6. **No destructive correction.** Transaksi yang sudah final tidak dihapus diam-diam; gunakan correction/adjustment/reversal sesuai domain.
7. **Privacy by domain.** Occupational health dan sensitive employee documents mempunyai akses lebih ketat dari profil HR biasa.
8. **Accreditation evidence by design.** Evidence KPS bukan laporan manual tambahan; evidence terbentuk dari transaksi operasional.
9. **Emergency care safety.** Bila credential/privilege dipakai sebagai service gate, emergency override harus mengikuti SOP dan selalu diaudit; sistem tidak boleh menciptakan hambatan yang membahayakan pasien.
10. **Configurable hospital policy.** SLA, escalation, expiry threshold, approval matrix, staffing ratio, payroll rules, dan training rules tidak di-hardcode bila dapat berbeda antar-RS.

---

## 10. Capability Matrix PRD

| ID | Capability | Existing V2 | Target | Priority |
|---|---|---|---|---|
| HRD-01 | Employee 360 & Employee File | Kuat | REUSE + EXTEND | P0 |
| HRD-02 | Organization, Position, Manager & Assignment | Kuat | REUSE + HARDEN | P0 |
| HRD-03 | Workforce Planning & Staffing | Model kuat, workflow belum lengkap | EXTEND | P0 |
| HRD-04 | Recruitment & Hiring | Model kuat, API/UI belum lengkap | EXTEND | P0 |
| HRD-05 | Onboarding, Orientation & Probation | Model tersedia, API partial | EXTEND | P0 |
| HRD-06 | Lifecycle & Offboarding | Resignation matang, lainnya partial | EXTEND | P1 |
| HRD-07 | Scheduling, Shift, On-call & Swap | Fondasi kuat | REUSE + HARDEN | P0 |
| HRD-08 | Attendance & Correction | Fondasi kuat | REUSE + HARDEN | P0 |
| HRD-09 | Leave, Izin & Balance | Fondasi kuat | REUSE + EXTEND | P0 |
| HRD-10 | Overtime & Compensatory Leave | Fondasi kuat | REUSE + HARDEN | P0 |
| HRD-11 | Payroll, Tax, Insurance & Benefit | Fondasi kuat | REUSE + EXTEND | P0 |
| HRD-12 | Credential, License & Certification | Kuat | REUSE + HARDEN | P0 |
| HRD-13 | Clinical Privilege, SPK/RKK | Privilege kuat, product flow perlu ditutup | EXTEND | P0 |
| HRD-14 | OPPE, FPPE & Recredentialing | Recredential model ada; OPPE/FPPE belum explicit | NEW + EXTEND | P0 |
| HRD-15 | Competency & Learning | Model luas, API partial | EXTEND | P0 |
| HRD-16 | Performance Management | Model luas, API partial | EXTEND | P1 |
| HRD-17 | Occupational Health & Staff Safety | Model hospital-specific kuat | REUSE + EXTEND | P0 |
| HRD-18 | Employee Relation & Discipline | Domain/master tersedia | EXTEND | P1 |
| HRD-19 | HR Service, Employee & Manager Self-Service | Navigation tersedia, route operasional partial | EXTEND | P1 |
| HRD-20 | Business Travel & Expense | Domain tersedia | REUSE/EXTEND | P2 |
| HRD-21 | HR Analytics, Audit & Accreditation Evidence | Data tersebar | NEW aggregation | P0 |

---

## 11. Functional Requirements

### HRD-01 - Employee 360 & Employee File

**Tujuan:** seluruh data kepegawaian dan evidence KPS staf tersedia dalam satu profil terkontrol.

**Must-have:**

- profil identitas pegawai;
- alamat, kontak darurat, keluarga/tanggungan;
- rekening bank;
- pendidikan dan riwayat pekerjaan;
- kontrak/status kepegawaian;
- dokumen pegawai;
- assignment organisasi/jabatan/manager;
- salary assignment reference;
- training/certificate reference;
- credential/license reference untuk staf klinis;
- performance history;
- orientation evidence;
- occupational health summary reference dengan privacy segregation;
- document expiry alert;
- immutable audit untuk perubahan critical.

**Acceptance:** auditor yang berwenang dapat membuka employee file dan menelusuri evidence pendidikan, kompetensi, orientasi, uraian/assignment, riwayat kerja, performance, training, credential, dan health evidence tanpa menggabungkan file manual.

### HRD-02 - Organization, Position, Manager & Assignment

**Must-have:**

- legal entity/site/unit/cost center;
- job family/level/grade/position;
- organization hierarchy effective-dated;
- employee organization assignment;
- position assignment;
- manager assignment;
- staffing ownership per unit;
- history dan effective date;
- validation tidak boleh menghasilkan active overlapping assignment yang tidak diizinkan policy.

### HRD-03 - Workforce Planning & Staffing

**Reuse candidate:** `MstPositionHeadcountPlan`, `MstShiftSkillRequirement`, `MstStaffingRatio`, `MstStaffingStandard`, `MstWorkforceRequirement`, `TrxAnnualManpowerPlan`, `TrxDailyStaffingRequirement`, `TrxHeadcountRequest`, `TrxManpowerPlanDetail`, `TrxStaffingGapAnalysis`, `TrxWorkforceAllocation`.

**Main flow:**

1. Kepala unit mengusulkan kebutuhan jumlah dan kualifikasi staf.
2. Sistem membandingkan staffing standard, skill requirement, employee aktif, shift, dan workload input yang disepakati.
3. Gap staffing dihitung/disajikan.
4. Manpower plan diajukan ke approval matrix.
5. Approved gap dapat menjadi headcount request/job requisition.
6. Allocation dapat dipantau per unit/shift.
7. Perubahan kebutuhan memiliki reason dan history.

**Acceptance:** kebutuhan staf dapat ditelusuri dari plan -> approval -> headcount -> recruitment/hiring -> assignment terisi.

### HRD-04 - Recruitment & Hiring

**Reuse candidate:** `TrxJobRequisition`, `TrxJobRequisitionApproval`, `TrxJobVacancy`, `TrxCandidate`, `TrxCandidateApplication`, `TrxCandidateScreening`, `TrxCandidateAssessment`, `TrxCandidateInterview`, `TrxInterviewEvaluation`, `TrxReferenceCheck`, `TrxBackgroundCheck`, `TrxPreEmploymentMedicalCheck`, `TrxJobOffer`, `TrxCandidateHiring`.

**Main flow:**

`Approved Headcount -> Job Requisition -> Vacancy -> Candidate -> Screening -> Assessment -> Interview -> Reference/Background Check -> Pre-employment MCU -> Offer -> Hiring -> Employee/Onboarding`

**Business rule minimum:**

- requisition harus mengacu pada position/unit/headcount yang sah atau exception dengan approval;
- qualification/competency requirement dapat dibandingkan dengan kandidat;
- clinical role dapat mewajibkan credential pre-check;
- candidate rejection menyimpan reason;
- hiring tidak membuat employee duplicate;
- hiring handoff otomatis membuat onboarding case.

### HRD-05 - Onboarding, Orientation & Probation

**Reuse candidate:** `MstOnboardingTemplate`, `MstOnboardingTemplateTask`, `TrxEmployeeOnboarding`, `TrxEmployeeOnboardingTask`, `WfpOnboardingChecklist`, `WfpOnboardingTask`, `TrxProbationReview`.

**Must-have:**

- onboarding template per employee type/unit/profession;
- orientation rumah sakit;
- orientation unit;
- policy/patient safety/infection control orientation yang applicable;
- credentialing handoff untuk clinical staff;
- mandatory training assignment;
- access provisioning request/handoff;
- equipment/asset handoff bila applicable;
- probation review dan outcome;
- overdue task/escalation;
- evidence attachment/certificate.

**Coverage employee type:** pegawai tetap, kontrak, part-time, tenaga klinis, trainee/mahasiswa, volunteer/tenaga lain bila dikelola RS.

### HRD-06 - Lifecycle & Offboarding

**Reuse candidate:** resignation flow existing plus `TrxEmployeeSeparation`, `TrxTermination`, `TrxRetirement`, `TrxContractNonRenewal`, `TrxExitClearance`, `TrxExitInterview`, `TrxAssetReturn`, `TrxAccessRevocation`, offboarding template/checklist.

**Main flow:**

`Request/Decision -> Approval -> Effective Separation -> Final Payroll Input -> Exit Clearance -> Asset Return -> Access Revocation -> Document/Certificate -> Offboarding Closed`

**Rule:** effective separation tidak boleh menghilangkan histori employee/credential/performance/payroll.

### HRD-07 - Scheduling, Shift, On-call & Swap

**Must-have:**

- work calendar, shift, shift group, shift pattern;
- schedule assignment;
- schedule change;
- shift swap dengan approval bila policy meminta;
- on-call assignment/type;
- rest/grace/roster policy;
- conflict detection;
- history dan effective date;
- integration ke Attendance dan allowance.

### HRD-08 - Attendance & Correction

**Main flow target:**

`Raw Log -> Schedule Resolver -> Daily Processing -> Exception -> Correction/Approval -> Period Reconciliation -> Payroll Handoff`

**Must-have:**

- raw attendance log tidak diubah oleh hasil olahan;
- resolve jadwal yang berlaku;
- late/early/missing/incomplete rules configurable;
- attendance correction dengan reason, evidence, approval, audit;
- period close/lock;
- reopening hanya role tertentu dan harus auditable;
- payroll handoff idempotent;
- dashboard exception.

**Keputusan project - Izin Pulang Cepat:**

- pulang cepat ditangani melalui request terpisah;
- pegawai submit request dan manager/supervisor menerima approval;
- selama status pending, checkout attendance tidak diubah;
- setelah approved, checkout efektif menggunakan waktu pembuatan/submission request, bukan waktu approval;
- reject tidak mengubah checkout attendance;
- seluruh perubahan menyimpan reference request dan audit trail.

### HRD-09 - Leave, Izin & Balance

**Must-have:**

- entitlement/accrual;
- balance;
- leave request;
- approval;
- calendar conflict;
- carry forward;
- adjustment;
- cancellation;
- recall;
- final reconciliation;
- payroll integration;
- attachment mandatory untuk leave type tertentu configurable;
- izin non-cuti dan pulang cepat dapat menggunakan workflow yang konsisten tanpa mencampur saldo cuti.

### HRD-10 - Overtime & Compensatory Leave

**Main flow:**

`Plan/Request -> Approval -> Realization -> Verification -> Reconciliation -> Compensation Choice -> Payroll/Comp Leave`

**Must-have:** policy/rate, realisasi vs plan, exception reason, compensatory leave, period close, payroll handoff, audit.

### HRD-11 - Payroll, Tax, Insurance & Benefit

**Must-have:**

- payroll period;
- salary assignment/structure;
- component earnings/deductions;
- attendance/leave/overtime handoff;
- shift/on-call/hazard/transport allowance sesuai policy;
- benefit eligibility;
- insurance;
- tax rule/configuration sesuai scope implementasi;
- payroll calculation preview;
- variance/reconciliation;
- approval/finalization;
- payslip;
- correction/reversal policy;
- bank/payment handoff bila termasuk ownership HRD;
- audit dan period lock.

### HRD-12 - Credential, License & Certification

**Reuse candidate:** `WfpCredentialLicense`, `WfpCertification`, renewal request, compliance alert, credentialing application/document/verification/committee review/decision.

**Must-have:**

- credential application;
- document checklist per profession;
- license/certification validity;
- primary source verification record;
- reviewer/committee review;
- decision;
- expiry/renewal alert;
- suspension/revocation impact;
- document version/history;
- link ke workforce profile.

### HRD-13 - Clinical Privilege, SPK/RKK

**Reuse candidate:** `WfpClinicalPrivilege`, `TrxClinicalPrivilegeRequest`, `TrxClinicalPrivilegeAssessment`, `TrxClinicalPrivilegeApproval`, `TrxClinicalPrivilegeSuspension`, `TrxClinicalPrivilegeRevocation`.

**Must-have:**

- privilege catalog by profession/specialization;
- request and assessment;
- approval authority;
- effective date dan validity;
- SPK/RKK generation/reference;
- availability status untuk unit pelayanan;
- suspension/revocation;
- temporary/supervised privilege bila SOP memperbolehkan;
- audit reason;
- service integration contract untuk mengecek privilege aktif pada tindakan yang memerlukan kewenangan.

**Safety rule:** clinical enforcement harus mempunyai emergency override sesuai SOP dan semua override harus diaudit.

### HRD-14 - OPPE, FPPE & Recredentialing

**Status:** capability gap utama untuk target 100%.

**OPPE must-have:**

- periode/evaluation cycle;
- configurable metric set per specialty/privilege;
- source data dapat berasal dari clinical quality, volume, complication/outcome, documentation compliance, behavior/professionalism, training/development, atau input committee yang disetujui;
- objective evidence dan source reference;
- reviewer;
- conclusion/recommendation;
- link ke privilege;
- trend/history;
- annual review evidence minimum sesuai kebijakan RS.

**FPPE must-have:**

- trigger: new privilege, concern, low volume, quality/safety signal, re-entry, atau reason configurable;
- scope privilege yang diawasi;
- supervisor/proctor;
- start/end criteria;
- observation evidence;
- conclusion;
- follow-up: maintain/limit/suspend/revoke/extend supervision sesuai authority.

**Recredentialing must-have:**

- due-date monitoring;
- package credential terbaru;
- OPPE/FPPE summary;
- committee review;
- decision;
- renewal/revision of SPK/RKK;
- full audit trail.

### HRD-15 - Competency & Learning

**Reuse candidate:** `WfpCompetencyAssessment`, `WfpTrainingRecord`, training plan/session/participant/attendance/assessment/result/evaluation/certificate, `TrxIndividualDevelopmentPlan`.

**Must-have:**

- competency catalog dan requirement per role;
- competency assessment;
- gap analysis;
- mandatory training assignment;
- training plan/session/enrollment;
- attendance;
- pre/post assessment bila applicable;
- result/evaluation;
- certificate dan expiry;
- individual development plan;
- BHD/BHL/resuscitation competence tracking sesuai role dan policy;
- overdue training alert;
- linkage ke employee file dan credentialing bila training menjadi requirement privilege.

### HRD-16 - Performance Management

**Reuse candidate:** `TrxPerformanceCycle`, `TrxEmployeeGoal`, `TrxEmployeeKpiTarget`, `TrxSelfAssessment`, `TrxManagerAssessment`, `TrxPeerFeedback`, `TrxPerformanceCheckIn`, `TrxCalibrationSession`, `TrxPerformanceImprovementPlan`, `WfpPerformanceReview`, `WfpPerformanceReviewDetail`.

**Must-have:**

- performance cycle;
- goal/KPI;
- self assessment;
- manager assessment;
- optional peer feedback;
- calibration;
- final review;
- acknowledgement;
- PIP;
- evidence/history;
- untuk staf klinis, indicator mutu/keselamatan/risiko dapat dimasukkan sesuai scope;
- performance ordinary tidak menggantikan OPPE untuk dokter.

### HRD-17 - Occupational Health & Staff Safety

**Reuse candidate:** `WfpHealthRecord`, `TrxEmployeeMedicalExamination`, `TrxEmployeeVaccination`, `TrxEmployeeHealthSurveillance`, `TrxEmployeeInjury`, `TrxNeedleStickIncident`, `TrxOccupationalExposure`, `TrxEmployeeFitnessToWork`, `TrxWorkRestriction`, `TrxReturnToWorkAssessment`.

**Must-have:**

- pre-employment/periodic/return-to-work exam as configured;
- vaccination/immunization/prophylaxis tracking;
- health surveillance by risk;
- needle stick/exposure workflow;
- injury/work incident record;
- counseling/follow-up reference;
- workplace violence incident/follow-up integration or dedicated case type;
- fitness-to-work;
- work restriction;
- return-to-work assessment;
- confidentiality and restricted role access;
- aggregate compliance dashboard without exposing clinical detail to unauthorized HR users.

### HRD-18 - Employee Relation & Discipline

**Must-have:**

- case intake;
- violation type;
- investigation/evidence;
- employee response;
- sanction/disciplinary action;
- approval;
- appeal/review if policy requires;
- closure;
- confidentiality;
- history dan report.

### HRD-19 - HR Service & Self-Service

**Existing navigation:** `Layanan Kepegawaian`, `Akun`, `Karyawan`, `Manajer`, `Dashboard Saya`, `Profil Karyawan Saya`, `Pengajuan Perubahan`, `Dokumen Saya`, `Data Kepegawaian Saya`, `Dashboard Manajer`, `Tim Saya`, `Persetujuan Saya`.

**Must-have:**

- employee dashboard;
- manager dashboard;
- profile change request;
- document access/request;
- leave/overtime/permission entry points;
- payslip access;
- schedule/attendance view;
- training/credential expiry notification;
- manager team overview;
- unified approval inbox;
- delegation;
- notification/history.

### HRD-20 - Business Travel & Expense

**Must-have minimum:** travel request, approval, itinerary/cost estimate, expense claim, receipt/evidence, verification, settlement/handoff, audit. Jika rumah sakit memutuskan ownership ada di Finance, HRD cukup menyediakan employee/travel workflow dan handoff contract.

### HRD-21 - HR Analytics, Audit & Accreditation Evidence

**Must-have:**

- headcount/FTE and staffing gap;
- turnover/hiring funnel;
- attendance/leave/overtime exception;
- payroll variance summary;
- credential/license/certificate expiry;
- training compliance;
- performance completion;
- occupational health compliance aggregate;
- OPPE/FPPE/recredential due;
- workflow SLA/outstanding approval;
- evidence export by employee/unit/period;
- KPS traceability report;
- audit event search for authorized auditor.

---

## 12. Traceability KPS 1-19 ke Capability HRD

Mapping ini dipakai sebagai product traceability. Detail evidence regulasi/SOP tetap harus divalidasi oleh owner akreditasi rumah sakit.

| KPS | Target capability sistem | Requirement MVP |
|---|---|---|
| KPS 1 | HRD-03 Workforce Planning | perencanaan jumlah/kualifikasi staf, monitoring staffing, gap dan allocation |
| KPS 2 | HRD-01, HRD-02, HRD-13 | employee assignment/job responsibility; clinical staff memakai kewenangan/assignment klinis yang relevan |
| KPS 3 | HRD-04 Recruitment | recruitment, candidate evaluation, appointment/hiring yang terdokumentasi |
| KPS 4 | HRD-15 Competency | initial competency evaluation untuk PPA/high-risk role sesuai requirement |
| KPS 5 | HRD-16 Performance | performance evaluation untuk staf nonklinis dan role yang applicable |
| KPS 6 | HRD-01 Employee File | standardized confidential employee file lengkap |
| KPS 7 | HRD-05 Onboarding/Orientation | orientation RS, unit, role, dan kategori staf yang applicable |
| KPS 8 | HRD-15 Learning | continuing education dan training berdasarkan kebutuhan |
| KPS 8.1 | HRD-15 Learning | tracking BHD/BHL/resuscitation training sesuai role dan interval policy |
| KPS 9 | HRD-17 Occupational Health | staff health/safety, exposure, vaccination, follow-up, workplace safety |
| KPS 10 | HRD-12 Credentialing | credential verification tenaga medis dan dokumen legal/professional |
| KPS 11 | HRD-13 Privileging | clinical privilege, SPK/RKK, availability di unit pelayanan |
| KPS 12 | HRD-14 OPPE | ongoing professional practice evaluation tenaga medis |
| KPS 13 | HRD-14 Recredentialing | recredentialing berbasis evidence termasuk OPPE/FPPE sesuai policy |
| KPS 14 | HRD-12 Credentialing | credentialing tenaga keperawatan |
| KPS 15 | HRD-13 Privileging | penugasan/kewenangan klinis keperawatan berdasarkan hasil kredensial |
| KPS 16 | HRD-16 Performance | evaluasi kinerja keperawatan termasuk mutu/keselamatan/risiko |
| KPS 17 | HRD-12 Credentialing | credentialing tenaga kesehatan lain |
| KPS 18 | HRD-13 Privileging | penugasan klinis tenaga kesehatan lain berdasarkan credential/competency |
| KPS 19 | HRD-16 Performance | evaluasi kinerja tenaga kesehatan lain termasuk mutu/keselamatan/risiko |

### KPS coverage gate

Target 100% dianggap tercapai bila seluruh row KPS di atas memiliki:

- owner proses;
- screen/workspace;
- API/business rule;
- persistence/evidence;
- audit trail;
- report/export evidence;
- UAT scenario yang lulus.

---

## 13. Golden Business Flows

### Flow A - Workforce Plan sampai Pegawai Aktif

1. Unit menyusun kebutuhan tenaga berdasarkan service need, staffing standard, qualification, skill, dan kondisi existing.
2. HR melakukan review dan gap analysis.
3. Manpower/headcount request diajukan melalui workflow.
4. Setelah approved, job requisition dibuat.
5. Recruiter membuka vacancy dan memproses candidate.
6. Candidate melewati screening, assessment, interview, dan verification sesuai role.
7. Untuk role yang membutuhkan, pre-employment medical check harus selesai.
8. Offer dibuat dan disetujui sesuai authority.
9. Candidate accepted dikonversi menjadi employee tanpa duplicate identity.
10. Onboarding case otomatis dibuat.
11. Employee menjalani orientation, assignment, mandatory training, dan credentialing bila clinical.
12. Employee menjadi active/ready sesuai gate yang applicable.

**Outcome:** kebutuhan tenaga dapat ditelusuri sampai posisi terisi oleh employee aktif.

### Flow B - Schedule sampai Payroll

1. Employee mempunyai organization/position assignment aktif.
2. Schedule/shift/on-call ditetapkan.
3. Raw attendance masuk dari source attendance.
4. Schedule resolver menentukan expected attendance.
5. Daily processing menghasilkan attendance result dan exception.
6. Pegawai/manager mengajukan correction/izin bila diperlukan.
7. Approved correction mengubah result melalui proses terkontrol, bukan mengubah raw log.
8. Leave/overtime/allowance yang relevan direkonsiliasi.
9. Attendance period ditutup.
10. Payroll menerima handoff idempotent.
11. Payroll menghitung earnings/deductions/benefit/tax/insurance sesuai configuration.
12. Payroll preview diverifikasi, disetujui, lalu difinalisasi.
13. Payslip tersedia dan payment handoff dilakukan sesuai ownership.

**Outcome:** nilai payroll dapat ditelusuri kembali ke source attendance/leave/overtime/policy.

### Flow C - Credential sampai Boleh Praktik

1. Clinical workforce profile memiliki profession/specialization yang benar.
2. Credentialing requirement ditentukan berdasarkan profession/role.
3. Staf mengirim/HR mengumpulkan credential document.
4. License/certification diverifikasi termasuk primary source verification sesuai policy.
5. Committee/reviewer menilai credential application.
6. Credentialing decision dibuat.
7. Clinical privilege diminta dan dinilai.
8. Authority menyetujui privilege dan menghasilkan/menautkan SPK/RKK.
9. Active privilege tersedia sebagai reference bagi unit pelayanan.
10. Expiry/suspension/revocation langsung mempengaruhi status privilege melalui rule yang terkontrol.
11. OPPE mengumpulkan evidence praktik secara berkelanjutan.
12. FPPE dijalankan bila trigger terpenuhi.
13. Recredentialing menggabungkan credential terbaru, OPPE, FPPE, dan committee decision.
14. SPK/RKK diperbarui dengan version/history.

**Outcome:** rumah sakit dapat menjelaskan siapa berwenang melakukan apa, berdasarkan evidence apa, dan berlaku sampai kapan.

### Flow D - Learning, Competency dan Performance

1. Role mempunyai competency dan mandatory training requirement.
2. Employee profile dibandingkan dengan requirement.
3. Gap menghasilkan assessment/training/development plan.
4. Employee mengikuti training/session dan hasil dicatat.
5. Certificate/competency status diperbarui.
6. Performance cycle mengumpulkan goal/KPI, self/manager assessment, dan evidence.
7. Review/calibration difinalisasi.
8. PIP dibuat bila diperlukan.
9. Untuk clinical roles, output competency/performance dapat menjadi input credentialing sesuai rule; performance umum tidak menggantikan OPPE.

### Flow E - Occupational Health & Staff Safety

1. Risk profile/role menentukan surveillance/health requirement.
2. Medical examination dan vaccination dicatat.
3. Exposure/injury/needle-stick incident dapat dilaporkan segera.
4. K3RS/PPI/Occupational Health melakukan assessment dan follow-up.
5. Prophylaxis/counseling/follow-up dicatat sesuai scope yang diperbolehkan.
6. Fitness-to-work atau work restriction dapat diterbitkan.
7. Return-to-work assessment menutup episode bila applicable.
8. HR hanya melihat summary yang diperlukan untuk employment action; detail kesehatan tetap restricted.

### Flow F - Separation & Offboarding

1. Separation dipicu oleh resignation, termination, retirement, non-renewal, atau reason valid lain.
2. Approval/decision dicatat.
3. Effective date dikunci.
4. Payroll menerima final input.
5. Exit clearance dan asset return diproses.
6. Access revocation dilakukan pada waktu yang tepat.
7. Credential/privilege active status diselesaikan sesuai policy.
8. Employment certificate/exit document dibuat bila applicable.
9. Employee menjadi inactive/separated tanpa kehilangan history.

---

## 14. Backend Evidence Summary dan Disposition

Status berikut adalah product-level disposition dari evidence repository yang dibaca. Physical table name tidak diinfer dari nama class; verifikasi mapping/migration tetap menjadi gate implementasi per task.

| Capability | Backend Legacy V1 | Backend V2 | Disposition |
|---|---|---|---|
| Employee master/file | PARTIAL - Karyawan, DokumenDetailKaryawan, pendidikan/sertifikat dan master terkait tersedia | TERSEDIA/PARTIAL - WorkforceCore/Profile/Master jauh lebih luas | migrate/reuse V2, mapping V1->V2 |
| Leave | PARTIAL - JenisCuti, SisaCuti, PengajuanCuti tersedia | TERSEDIA - domain Leave luas | reuse V2, migrate balances/history sesuai keputusan |
| Overtime | PARTIAL - JenisLembur dan PengajuanLembur | TERSEDIA - plan, realization, verification, reconciliation, handoff | reuse V2 |
| Recruitment | PARTIAL - PengajuanRekrutmen sederhana | PARTIAL - model recruitment lengkap, controller operasional belum terbukti | extend V2, jangan copy flow V1 secara mentah |
| Lifecycle | PARTIAL - PengajuanResign | PARTIAL - model onboarding/offboarding/separation luas, API dominan resignation | extend V2 |
| Workforce planning | BELUM_TERVERIFIKASI sebagai domain setara | PARTIAL - model staffing/manpower lengkap, API belum terbukti | extend V2 |
| Scheduling/Attendance | BELUM_TERVERIFIKASI sebagai chain modern | TERSEDIA/PARTIAL - master dan attendance processing kuat | reuse/harden V2 |
| Payroll/Benefit | PARTIAL/legacy-specific | TERSEDIA/PARTIAL | reuse V2, verify calculation/finalization rules |
| Credentialing | BELUM_TERVERIFIKASI sebagai domain setara | TERSEDIA/PARTIAL - credential/license/certification/privilege kuat | reuse + close workflow gaps |
| OPPE/FPPE | BELUM_TERVERIFIKASI | BELUM_TERVERIFIKASI sebagai entity/workflow HRD explicit | build explicit product capability after design audit |
| Learning | PARTIAL - keahlian/sertifikat/test evidence | PARTIAL - model training luas, controllers limited | extend V2 |
| Performance | BELUM_TERVERIFIKASI sebagai domain setara | PARTIAL - model luas, controllers review/detail | extend V2 |
| Occupational Health | BELUM_TERVERIFIKASI sebagai domain setara | TERSEDIA/PARTIAL - hospital-specific health models kuat | reuse + staff-safety extension |

### Backend implementation rule

Sebelum satu task masuk status Ready for Development:

1. identifikasi entity/model existing;
2. verifikasi `ApplicationDbContext`/DbSet/mapping;
3. verifikasi migration/schema yang berlaku;
4. verifikasi API/service existing;
5. pilih REUSE, EXTEND, atau NEW;
6. dilarang membuat duplicate table hanya karena endpoint belum ada.

---

## 15. Data dan Domain Ownership

### HRD owns

- workforce/employee profile dan employment history;
- organization/position/manager assignment dari sisi HR;
- workforce planning;
- recruitment/hiring;
- onboarding/lifecycle;
- schedule/attendance/leave/overtime;
- payroll configuration/processing sesuai boundary produk;
- credential/license/certification/privilege metadata;
- learning/competency/performance;
- occupational health workforce record;
- employee relation;
- workflow metadata HR dan HR service.

### Integration ownership yang perlu jelas

- **Finance:** payroll settlement, accounting posting, reimbursement payment, tax payment/reporting boundary.
- **Clinical modules:** source metrics OPPE/FPPE, tindakan/volume/outcome/quality reference, privilege check integration.
- **PPI/K3RS:** occupational exposure and staff safety process ownership.
- **Administrator/Identity:** account provisioning, role/access, revocation.
- **Document storage:** credential/certificate/employee document secure file storage.

Tidak boleh ada dua modul menjadi source of truth untuk fakta yang sama tanpa ownership rule eksplisit.

---

## 16. Workflow dan Approval Standard

Semua approval HR yang applicable harus menggunakan pola bersama:

`Draft -> Submitted -> Review/Approval -> Approved/Rejected/Returned/Cancelled -> Effective/Closed`

Nama status implementasi wajib mengikuti enum/status existing bila sudah ada; lifecycle di atas bersifat konseptual.

### Workflow minimum feature

- approval matrix by transaction type/unit/grade/amount bila applicable;
- manager hierarchy resolution;
- delegation;
- approver replacement/escalation;
- comment;
- attachment;
- reject/return reason;
- status history;
- notification;
- SLA/outstanding dashboard;
- audit actor dan timestamp;
- no self-approval bila policy melarang.

---

## 17. Security, Privacy dan Audit Requirements

### 17.1 RBAC

Minimal pisahkan:

- Employee Self-Service;
- Manager Self-Service;
- HR Admin;
- HR Manager;
- Recruiter;
- Payroll Officer;
- Credentialing Reviewer/Committee;
- L&D;
- Occupational Health/K3RS;
- Auditor Read-only;
- System Integration.

### 17.2 Sensitive data

Restricted data mencakup minimal:

- salary/payroll detail;
- bank account;
- health record;
- disciplinary investigation;
- credential document tertentu;
- candidate background/reference data;
- identification document.

### 17.3 Audit event minimum

Simpan event untuk:

- create/update/delete logical action;
- approval/reject/return/cancel;
- effective-date change;
- payroll finalization/reopen;
- attendance correction;
- credential verification/decision;
- privilege approval/suspension/revocation;
- OPPE/FPPE conclusion;
- health fitness/restriction status change;
- role/access critical operation;
- export sensitive report.

Audit harus mempunyai actor, timestamp, action, object reference, before/after atau change summary yang aman, source, dan correlation/reference bila tersedia.

---

## 18. Non-Functional Requirements

### Performance

- list/search employee dan transaction memakai server-side pagination;
- filter period/unit/status wajib tersedia untuk transaksi volume besar;
- dashboard tidak boleh melakukan full-table load di client;
- payroll/attendance batch harus resumable/idempotent bila retry diperlukan.

### Reliability

- transactional consistency untuk approval/finalization critical;
- idempotency untuk external/batch handoff;
- background scheduler mempunyai retry policy dan failure visibility;
- no silent partial success untuk payroll/credentialing critical flow.

### Observability

- structured log dengan correlation id;
- job execution log;
- integration error queue/monitoring;
- alert untuk scheduler critical gagal;
- audit berbeda dari technical log.

### Document handling

- type/size validation;
- malware scanning bila infrastructure mendukung;
- restricted download authorization;
- version/history untuk document critical;
- expiry metadata;
- no public URL untuk sensitive employee/health/credential documents.

### Time and locale

- business date/time menggunakan timezone rumah sakit; baseline Indonesia menggunakan Asia/Jakarta kecuali site mempunyai timezone berbeda;
- period close harus deterministic;
- display tanggal/jam konsisten antara FE dan BE.

### Data retention

Retention per jenis employee file, payroll, credential, health, recruitment, dan audit harus configurable/ditetapkan melalui policy legal rumah sakit sebelum production. PRD tidak meng-hardcode masa retensi tanpa keputusan legal/policy.

---

## 19. MVP Delivery Strategy

Target MVP dibagi menjadi vertical slice agar setiap fase menghasilkan proses bisnis yang dapat diuji. Semua fase tetap bagian dari target 100% PRD.

### MVP-0 - Foundation & Evidence Freeze

**Tujuan:** memastikan tidak ada duplicate build.

- inventory entity/table/API V2 per capability;
- verify legacy mapping yang perlu dimigrasikan;
- finalisasi business status/lifecycle;
- finalisasi role/approval matrix;
- finalisasi data privacy classification;
- finalisasi KPS traceability;
- contract untuk attachment, audit, notification, workflow;
- test data strategy.

**Exit:** seluruh P0 capability mempunyai disposition REUSE/EXTEND/NEW yang disetujui.

### MVP-1 - Employee Foundation & Workforce

Scope:

- HRD-01 Employee 360;
- HRD-02 Organization/Assignment;
- HRD-03 Workforce Planning;
- HRD-19 self-service foundation;
- workflow core.

**Golden demo:** kepala unit melihat staffing gap -> submit headcount -> HR approve -> headcount siap diteruskan ke recruitment.

### MVP-2 - Hire to Ready-for-Work

Scope:

- HRD-04 Recruitment;
- HRD-05 Onboarding/Orientation/Probation;
- HRD-06 lifecycle minimum;
- HRD-15 mandatory training initial;
- HRD-17 pre-employment health gate;
- HRD-12 credential pre-check untuk clinical role.

**Golden demo:** approved requisition -> candidate -> hiring -> onboarding -> orientation -> assignment -> ready-for-work.

### MVP-3 - Time to Pay

Scope:

- HRD-07 Scheduling;
- HRD-08 Attendance;
- HRD-09 Leave/Izin;
- HRD-10 Overtime;
- HRD-11 Payroll/Benefit;
- employee/manager self-service terkait waktu dan payroll.

**Golden demo:** schedule -> attendance exception -> correction/approval -> leave/overtime reconciliation -> payroll final -> payslip.

### MVP-4 - Credential to Practice

Scope:

- HRD-12 Credential/License/Certification;
- HRD-13 Clinical Privilege/SPK/RKK;
- HRD-14 OPPE/FPPE/Recredentialing;
- compliance alert;
- clinical service integration contract.

**Golden demo:** credential verified -> privilege approved -> SPK/RKK active -> OPPE -> FPPE bila perlu -> recredential -> privilege renewed/revised.

### MVP-5 - Competency, Performance & Staff Safety

Scope:

- HRD-15 Learning/Competency;
- HRD-16 Performance;
- HRD-17 Occupational Health & Staff Safety;
- HRD-18 Employee Relation.

**Golden demo:** competency gap -> training -> certificate -> performance review; exposure incident -> follow-up -> fitness/restriction -> return-to-work.

### MVP-6 - Operational Completion & Accreditation Evidence

Scope:

- HRD-19 self-service full;
- HRD-20 travel/expense minimum;
- HRD-21 analytics/compliance;
- evidence export;
- security/privacy hardening;
- load/batch testing;
- migration rehearsal;
- UAT end-to-end;
- go-live checklist.

**Exit:** seluruh gate Section 7 dan Section 25 lulus.

---

## 20. Backlog - Task dan Native Subtask Target

Availability existing tidak menghapus backlog. Task untuk capability REUSE berfokus pada validation, integration, hardening, dan end-to-end completion.

### Task HRD-01 - Finalisasi Employee 360 dan Employee File

- `01 - Audit Mapping Employee V1 ke Workforce/Employee V2`
- `02 - Finalisasi Profil, Employment, Assignment dan Manager History`
- `03 - Konsolidasi Dokumen, Pendidikan, Training, Credential dan Performance Reference`
- `04 - Terapkan Privacy Segregation untuk Health dan Sensitive Document`
- `05 - Bangun/Validasi Workspace Employee 360`
- `06 - Uji Employee File KPS dan Audit Trail`

### Task HRD-02 - Finalisasi Struktur Organisasi dan Penempatan

- `01 - Validasi Struktur Legal Entity, Site, Unit, Position, Grade dan Cost Center`
- `02 - Implementasi Effective-Dated Organization Assignment`
- `03 - Implementasi Position dan Manager Assignment`
- `04 - Validasi Overlap dan History Rule`
- `05 - Uji Perubahan Assignment End-to-End`

### Task HRD-03 - Operasionalisasi Workforce Planning

- `01 - Audit Model Workforce Planning Existing`
- `02 - Implementasi Manpower Plan dan Detail Workflow`
- `03 - Implementasi Daily/Shift Staffing Requirement`
- `04 - Implementasi Staffing Gap Analysis`
- `05 - Implementasi Headcount Request dan Approval`
- `06 - Implementasi Workforce Allocation dan Dashboard`
- `07 - Handoff Approved Headcount ke Recruitment`

### Task HRD-04 - Operasionalisasi Recruitment dan Hiring

- `01 - Implementasi Job Requisition dari Approved Headcount`
- `02 - Implementasi Vacancy dan Candidate Application`
- `03 - Implementasi Screening dan Assessment`
- `04 - Implementasi Interview dan Evaluation`
- `05 - Implementasi Reference dan Background Check`
- `06 - Integrasi Pre-Employment Medical Check`
- `07 - Implementasi Job Offer dan Approval`
- `08 - Implementasi Hiring ke Employee dan Onboarding Handoff`

### Task HRD-05 - Finalisasi Onboarding, Orientation dan Probation

- `01 - Finalisasi Onboarding Template per Employee Type`
- `02 - Implementasi Onboarding Task dan Checklist Workflow`
- `03 - Implementasi Orientation Evidence Rumah Sakit dan Unit`
- `04 - Assign Mandatory Training dan Credentialing Gate`
- `05 - Implementasi Probation Review`
- `06 - Implementasi Overdue Monitoring dan Escalation`

### Task HRD-06 - Finalisasi Lifecycle dan Offboarding

- `01 - Reuse dan Hardening Resignation Existing`
- `02 - Implementasi Termination, Retirement dan Contract Non-Renewal`
- `03 - Implementasi Employee Separation Lifecycle`
- `04 - Implementasi Exit Clearance dan Exit Interview`
- `05 - Implementasi Asset Return dan Access Revocation Handoff`
- `06 - Implementasi Offboarding Checklist dan Closure`

### Task HRD-07 - Hardening Scheduling dan Shift

- `01 - Validasi Work Calendar, Shift, Group dan Pattern`
- `02 - Finalisasi Schedule Assignment`
- `03 - Finalisasi Schedule Change dan Shift Swap`
- `04 - Finalisasi On-Call dan Rest/Grace/Conflict Rule`
- `05 - Integrasi Schedule ke Attendance dan Allowance`

### Task HRD-08 - Hardening Attendance dan Correction

- `01 - Validasi Raw Log Ingestion dan Idempotency`
- `02 - Validasi Schedule Resolver dan Daily Processing`
- `03 - Finalisasi Exception dan Correction Workflow`
- `04 - Implementasi Izin Pulang Cepat sesuai Keputusan Produk`
- `05 - Finalisasi Period Close, Reopen dan Monitoring`
- `06 - Finalisasi Payroll Handoff dan Reconciliation`

### Task HRD-09 - Finalisasi Leave dan Izin

- `01 - Validasi Entitlement, Accrual dan Balance`
- `02 - Finalisasi Leave Request dan Approval`
- `03 - Finalisasi Carry Forward dan Adjustment`
- `04 - Finalisasi Cancellation dan Recall`
- `05 - Tambahkan Izin Non-Cuti dan Attachment Rule bila Belum Tercakup`
- `06 - Finalisasi Reconciliation dan Payroll Integration`

### Task HRD-10 - Hardening Overtime

- `01 - Validasi Overtime Policy dan Rate`
- `02 - Finalisasi Plan/Request dan Approval`
- `03 - Finalisasi Realization dan Verification`
- `04 - Finalisasi Reconciliation dan Compensatory Leave`
- `05 - Finalisasi Payroll Handoff dan Period Close`

### Task HRD-11 - Finalisasi Payroll dan Benefit

- `01 - Audit Salary Structure, Component, Benefit dan Deduction Existing`
- `02 - Finalisasi Input Handoff Attendance/Leave/Overtime`
- `03 - Finalisasi Allowance Shift/On-Call/Hazard/Transport`
- `04 - Finalisasi Tax, Insurance dan Benefit Eligibility`
- `05 - Implementasi Payroll Preview dan Variance Reconciliation`
- `06 - Implementasi Approval, Finalization, Lock dan Controlled Reopen`
- `07 - Implementasi Payslip dan Payment/Finance Handoff`

### Task HRD-12 - Hardening Credential dan License

- `01 - Finalisasi Credentialing Requirement per Profession`
- `02 - Finalisasi Application dan Document Checklist`
- `03 - Implementasi Primary Source Verification Evidence`
- `04 - Finalisasi License/Certification Validation dan Renewal`
- `05 - Finalisasi Committee Review dan Decision`
- `06 - Finalisasi Compliance Alert dan Expiry Monitoring`

### Task HRD-13 - Finalisasi Clinical Privilege dan SPK/RKK

- `01 - Validasi Privilege Catalog dan Eligibility`
- `02 - Finalisasi Privilege Request dan Assessment`
- `03 - Finalisasi Approval dan Effective Period`
- `04 - Implementasi SPK/RKK Versioned Evidence`
- `05 - Finalisasi Suspension dan Revocation`
- `06 - Implementasi Privilege Availability Contract untuk Clinical Module`
- `07 - Implementasi Audited Emergency Override Contract sesuai SOP`

### Task HRD-14 - Pembuatan OPPE, FPPE dan Recredentialing End-to-End

- `01 - Finalisasi OPPE Metric Model dan Source Contract`
- `02 - Implementasi OPPE Period, Evidence Collection dan Review`
- `03 - Implementasi FPPE Trigger, Scope dan Proctoring`
- `04 - Implementasi FPPE Conclusion dan Privilege Recommendation`
- `05 - Integrasikan OPPE/FPPE ke Recredentialing Application`
- `06 - Finalisasi Recredentialing Committee Review dan Decision`
- `07 - Update SPK/RKK berdasarkan Recredentialing Outcome`
- `08 - Bangun Due/Overdue Dashboard dan Audit Evidence`

### Task HRD-15 - Operasionalisasi Competency dan Learning

- `01 - Finalisasi Competency Requirement per Role`
- `02 - Finalisasi Competency Assessment dan Gap`
- `03 - Implementasi Training Plan, Session dan Enrollment`
- `04 - Implementasi Attendance, Assessment, Result dan Evaluation`
- `05 - Implementasi Certificate dan Expiry Monitoring`
- `06 - Implementasi Individual Development Plan`
- `07 - Implementasi Mandatory Training termasuk BHD/BHL Tracking`
- `08 - Integrasi Training/Competency ke Employee File dan Credentialing`

### Task HRD-16 - Operasionalisasi Performance Management

- `01 - Finalisasi Performance Cycle dan Template`
- `02 - Implementasi Goal dan KPI Target`
- `03 - Implementasi Self dan Manager Assessment`
- `04 - Implementasi Check-In, Peer Feedback dan Calibration bila Dipakai`
- `05 - Finalisasi Performance Review dan Acknowledgement`
- `06 - Implementasi Performance Improvement Plan`
- `07 - Integrasi Quality/Safety Indicator untuk Clinical Staff sesuai Scope`

### Task HRD-17 - Finalisasi Occupational Health dan Staff Safety

- `01 - Validasi Health Record dan Medical Examination Flow`
- `02 - Finalisasi Vaccination dan Health Surveillance`
- `03 - Implementasi Needle Stick dan Occupational Exposure Workflow`
- `04 - Finalisasi Employee Injury dan Follow-Up`
- `05 - Implementasi Counseling/Staff Safety/Workplace Violence Follow-Up`
- `06 - Finalisasi Fitness-to-Work dan Work Restriction`
- `07 - Finalisasi Return-to-Work Assessment`
- `08 - Implementasi Privacy Segregation dan Compliance Dashboard`

### Task HRD-18 - Operasionalisasi Employee Relation dan Discipline

- `01 - Finalisasi Case Intake dan Classification`
- `02 - Implementasi Investigation dan Evidence`
- `03 - Implementasi Employee Response dan Review`
- `04 - Implementasi Sanction/Disciplinary Action dan Approval`
- `05 - Implementasi Closure, Confidentiality dan Reporting`

### Task HRD-19 - Finalisasi Employee dan Manager Self-Service

- `01 - Aktifkan Route dan Dashboard Employee`
- `02 - Aktifkan Profil, Dokumen dan Employment Self-Service`
- `03 - Integrasikan Leave/Izin/Overtime/Schedule/Attendance/Payslip`
- `04 - Aktifkan Manager Dashboard dan Team View`
- `05 - Aktifkan Unified Approval Inbox dan Delegation`
- `06 - Implementasi Notification dan History`

### Task HRD-20 - Finalisasi Business Travel dan Expense Minimum

- `01 - Validasi Domain Travel/Expense Existing`
- `02 - Implementasi Travel Request dan Approval`
- `03 - Implementasi Expense Claim dan Evidence`
- `04 - Implementasi Verification dan Finance/Payroll Handoff`
- `05 - Implementasi Audit dan Employee History`

### Task HRD-21 - Pembuatan HR Analytics dan Accreditation Evidence

- `01 - Definisikan KPI dan Access Scope Dashboard`
- `02 - Implementasi Workforce/Recruitment Dashboard`
- `03 - Implementasi Time/Payroll Compliance Dashboard`
- `04 - Implementasi Credential/Training/Performance Compliance Dashboard`
- `05 - Implementasi Occupational Health Aggregate Dashboard`
- `06 - Implementasi OPPE/FPPE/Recredential Due Dashboard`
- `07 - Implementasi KPS Traceability dan Evidence Export`
- `08 - Implementasi Authorized Audit Search`

---

## 21. Data Migration Strategy V1 -> V2

### Prinsip

- jangan copy schema V1 menjadi V2;
- mapping berdasarkan business meaning;
- preserve source id/reference untuk traceability;
- lakukan dry-run sebelum production;
- historical data yang tidak dapat dipetakan masuk exception report, bukan silently dropped.

### Candidate migration scope

1. Karyawan -> employee/workforce profile V2.
2. DokumenDetailKaryawan -> employee document/profile document target.
3. Riwayat pendidikan/sertifikat/pengalaman -> V2 employee profile/learning/credential sesuai semantics.
4. Department/position/grade -> V2 organization/job/grade mapping.
5. JenisCuti/SisaCuti/PengajuanCuti -> V2 leave master/balance/history sesuai reconciliation decision.
6. JenisLembur/PengajuanLembur -> V2 overtime history bila dibutuhkan.
7. PengajuanRekrutmen -> historical requisition/request reference bila business value masih diperlukan.
8. PengajuanResign -> lifecycle history bila belum tersedia di target.

### Migration acceptance

- count reconciliation per object;
- referential integrity lulus;
- duplicate employee report zero unresolved critical;
- balance cuti tervalidasi HR;
- active employee assignment tervalidasi unit/HR;
- document path/access tervalidasi;
- rollback plan tersedia.

---

## 22. API and Integration Contract Principles

1. API list wajib mendukung pagination/filter/sort yang konsisten.
2. Mutating endpoint harus mengembalikan current state/version.
3. Workflow action memakai actor authorization dari server.
4. Handoff mempunyai correlation/reference id.
5. Batch handoff harus idempotent.
6. Critical status transition divalidasi di backend, bukan hanya FE.
7. Attachment upload menggunakan controlled file endpoint/service.
8. Clinical privilege check contract minimal mengembalikan employee/provider, privilege, status, effective period, restriction/supervision, dan reason/reference yang aman.
9. OPPE clinical source integration tidak boleh copy seluruh clinical record ke HRD; simpan metric/evidence reference yang dibutuhkan.
10. Health data integration mengikuti minimum necessary principle.

---

## 23. Reporting Minimum Set

### Operational

- Employee List dan Employee File completeness.
- Headcount by unit/position/status.
- Staffing gap by unit/shift.
- Recruitment funnel dan aging.
- Onboarding completion dan overdue.
- Schedule coverage dan conflict.
- Attendance exception dan correction aging.
- Leave balance/liability summary sesuai scope.
- Overtime plan vs realization.
- Payroll variance dan payroll completion.
- Credential/license/certification expiry.
- Clinical privilege active/suspended/expiring.
- OPPE/FPPE/recredential due.
- Training compliance/expiry.
- Performance review completion.
- Occupational health compliance aggregate.
- Employee relation case aging sesuai privacy.
- Workflow outstanding/SLA.

### Accreditation evidence

- KPS evidence matrix by standard/element/employee/unit/period.
- Credentialing evidence pack.
- SPK/RKK/privilege evidence pack.
- OPPE/FPPE/recredentialing evidence pack.
- Employee file completeness pack.
- Training/orientation compliance pack.
- Staff health/safety evidence summary.

---

## 24. Acceptance Test Matrix - Minimum Golden Scenarios

| ID | Scenario | Expected result |
|---|---|---|
| AT-01 | Create/update employee dan assignment | history/effective date/audit benar |
| AT-02 | Workforce gap menjadi approved headcount | traceability plan->request tersedia |
| AT-03 | Recruitment sampai hiring | candidate berubah menjadi employee tanpa duplicate |
| AT-04 | Onboarding dan orientation | checklist/evidence selesai dan overdue terdeteksi |
| AT-05 | Schedule dan attendance normal | daily result sesuai schedule |
| AT-06 | Missing/incorrect attendance correction | approval dan audit mengubah processed result, raw log tetap |
| AT-07 | Izin Pulang Cepat approved | checkout efektif memakai submission time; pending tidak mengubah checkout |
| AT-08 | Leave accrual/request/cancel/recall | balance dan history konsisten |
| AT-09 | Overtime realization | verified amount/hours masuk payroll handoff dengan benar |
| AT-10 | Payroll finalization | input traceable, variance checked, period lock bekerja |
| AT-11 | Credential application | requirement/document/verification/decision dapat ditelusuri |
| AT-12 | License/certificate expiry | alert muncul sesuai threshold configurable |
| AT-13 | Clinical privilege approval | SPK/RKK/reference active dan dapat dicek unit pelayanan |
| AT-14 | Privilege suspended | service contract mengembalikan status yang benar dan audit tersedia |
| AT-15 | OPPE review | metric/evidence/reviewer/conclusion tersimpan |
| AT-16 | FPPE triggered | scope/proctor/evidence/conclusion lengkap |
| AT-17 | Recredentialing | OPPE/FPPE + credential menghasilkan decision dan SPK/RKK version baru |
| AT-18 | Mandatory training | assignment, attendance, result, certificate dan expiry terhubung employee |
| AT-19 | Performance review | cycle sampai final/PIP bekerja |
| AT-20 | Needle stick/exposure | case, assessment, follow-up dan privacy bekerja |
| AT-21 | Fitness/restriction/return to work | status employment-relevant dapat dilihat role yang berwenang tanpa membuka detail tidak perlu |
| AT-22 | Separation/offboarding | clearance, asset, access, final payroll handoff, history selesai |
| AT-23 | Employee self-service | employee hanya melihat data sendiri yang diizinkan |
| AT-24 | Manager approval | manager hanya melihat team/request sesuai authority |
| AT-25 | Accreditation evidence export | KPS traceability menghasilkan evidence pack yang dapat ditelusuri |

---

## 25. Production Readiness Gate - Target 100%

Semua item berikut wajib `PASS` sebelum Modul HRD dinyatakan mencapai target 100% PRD:

### Functional

- [ ] HRD-01 sampai HRD-21 mempunyai business owner.
- [ ] HRD-01 sampai HRD-21 mempunyai FE/API/persistence path yang valid sesuai scope.
- [ ] seluruh P0 golden flow lulus UAT.
- [ ] exception flow critical lulus.
- [ ] tidak ada menu operasional placeholder.

### KPS

- [ ] KPS 1-19 mempunyai mapping capability.
- [ ] evidence source dapat dibuktikan.
- [ ] employee file completeness dapat diukur.
- [ ] credentialing/privileging/recredentialing evidence dapat diekspor.
- [ ] OPPE/FPPE tersedia dan auditable.
- [ ] training/orientation/staff-health evidence tersedia.

### Data

- [ ] migration rehearsal lulus.
- [ ] duplicate employee critical = 0 unresolved.
- [ ] leave balance reconciliation lulus.
- [ ] organization/assignment active data tervalidasi.
- [ ] credential/expiry data active tervalidasi.

### Security & Privacy

- [ ] RBAC test lulus.
- [ ] payroll confidentiality test lulus.
- [ ] occupational health privacy test lulus.
- [ ] credential/document download authorization lulus.
- [ ] audit event critical lengkap.

### Integration

- [ ] attendance->payroll idempotency test lulus.
- [ ] leave/overtime->payroll reconciliation lulus.
- [ ] hiring->onboarding handoff lulus.
- [ ] credential/privilege->clinical contract lulus.
- [ ] OPPE source integration lulus untuk source yang disepakati.
- [ ] lifecycle->access revocation/finance handoff lulus sesuai ownership.

### Operations

- [ ] scheduler/job monitoring tersedia.
- [ ] backup/restore path tervalidasi oleh infrastructure owner.
- [ ] error monitoring dan support runbook tersedia.
- [ ] user guide/SOP mapping tersedia.
- [ ] UAT sign-off HR + clinical credentialing owner + payroll + occupational health/K3RS sesuai scope.

---

## 26. Definition of Done per Capability

Satu capability hanya boleh berstatus Done bila:

1. requirement dan business rule disetujui owner;
2. existing evidence sudah diaudit untuk mencegah duplicate build;
3. backend contract selesai;
4. persistence mapping/migration terverifikasi;
5. authorization selesai;
6. audit trail selesai;
7. frontend route/page selesai bila applicable;
8. loading/error/empty/permission state selesai;
9. integration test lulus;
10. business acceptance scenario lulus;
11. documentation dan operational note tersedia;
12. tidak ada Critical/High defect terbuka.

---

## 27. Prioritas Gap dari Existing Menuju 100%

### P0 - wajib ditutup terlebih dahulu

1. Workforce Planning API/UI end-to-end.
2. Recruitment API/UI end-to-end.
3. Onboarding/orientation/probation operational workflow.
4. Frontend route operasional HR selain master-data.
5. Employee file KPS completeness.
6. Attendance/leave/overtime/payroll reconciliation end-to-end.
7. Credential verification + SPK/RKK operational closure.
8. OPPE/FPPE explicit capability.
9. Recredentialing linked to OPPE/FPPE.
10. Mandatory training/BHD-BHL tracking.
11. Occupational health privacy dan staff-safety completion.
12. KPS analytics/evidence export.

### P1 - wajib sebelum final production completeness

1. Lifecycle selain resignation.
2. Performance full workflow.
3. Employee relation/discipline.
4. Self-service/manager full operational route.
5. workflow SLA/delegation/escalation hardening.

### P2 - dapat dikerjakan setelah core P0/P1 stabil namun tetap bagian scope PRD

1. business travel/expense minimum;
2. advanced analytics beyond accreditation/operational minimum;
3. UX optimization dan bulk action noncritical.

---

## 28. Keputusan Produk yang Harus Dikunci Sebelum Implementation Freeze

Beberapa nilai tidak boleh diasumsikan oleh developer dan harus menjadi configuration/decision owner:

1. formula staffing ratio dan sumber workload per unit;
2. employee type dan kategori non-employee yang dikelola HRD;
3. probation duration dan approval outcome;
4. leave entitlement detail per employee type;
5. overtime rate/eligibility dan compensatory leave rule;
6. payroll tax/insurance/payment ownership;
7. credential requirement per profession;
8. primary source verification method/source;
9. SPK/RKK approval authority per profession;
10. temporary/supervised privilege policy;
11. OPPE metric set per specialty;
12. FPPE trigger dan completion criteria;
13. recredential cycle dan due reminder configuration;
14. BHD/BHL training interval per role;
15. health surveillance/vaccination requirement per risk profile;
16. workplace violence/counseling process ownership;
17. employee/health/payroll/credential document retention;
18. hard-block vs warning untuk expired credential/privilege per clinical scenario dan emergency override SOP;
19. approval SLA/escalation per transaction;
20. data migration cut-off dan history depth.

Dokumen tetap dapat digunakan untuk development foundation sebelum semua angka di atas final, tetapi production release untuk flow terkait harus menunggu decision owner.

---

## 29. Source Evidence

### Repository evidence

- Frontend V2: `DevBenari/QuilvianSystemFrontendDev`, branch `QuilvianDevV2`.
  - `src/utils/menu-sidebar/menu-items.jsx`
  - `src/app/hr/**`
- Backend legacy: `DevBenari/QuilvianSystemBackendDev`, branch `QuilvianSta`.
  - `Areas/HRD/MasterData/**`
  - `Areas/HRD/Pengajuan/**`
  - `Repositories/ApplicationDbContext.cs`
  - relevant migrations.
- Backend V2: `DevBenari/NewQuilvianSystemBackend`, branch `master`.
  - `Areas/Corporate/HumanResource/**`
  - `Repositories/ApplicationDbContext.cs`
  - `Repositories/Configurations/Corporate/HumanResource/**`
  - relevant migrations.

### Hospital accreditation baseline

Product traceability mengacu pada Standar Akreditasi Rumah Sakit yang berlaku dan instrumen survei KPS yang memuat kebutuhan perencanaan staf, recruitment, employee file, orientation, education/training, staff health/safety, serta credentialing/privileging/performance untuk tenaga medis, keperawatan, dan tenaga kesehatan lain.

Regulatory evidence harus selalu direvalidasi terhadap dokumen Kementerian Kesehatan yang berlaku saat production readiness review.

---

## 30. Final Product Statement

Setelah seluruh MVP-0 sampai MVP-6 dan Production Readiness Gate selesai, Quilvian HRD ditargetkan memiliki rantai end-to-end berikut:

`Plan Workforce -> Recruit -> Hire -> Onboard -> Assign -> Schedule -> Attend -> Leave/Overtime -> Pay -> Develop -> Evaluate -> Credential -> Privilege -> OPPE/FPPE -> Recredential -> Protect Staff Health -> Serve Employee -> Separate/Offboard -> Audit & Accreditation Evidence`

Pada kondisi tersebut, target **100% functional coverage** berarti seluruh capability HRD rumah sakit yang didefinisikan pada PRD ini telah dimaterialisasikan sebagai proses operasional yang dapat diuji dan ditelusuri, dengan prinsip reuse terhadap fondasi Quilvian V2 yang sudah ada.
