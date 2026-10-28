# Requirement Traceability — Modul Human Resource

## Metadata

```yaml
module_id: human-resource
blueprint_id: HRD-BP-001
blueprint_shape: SINGLE
roadmap_revision: 2
status: ACTIVE
status_synced_at: "2026-10-28 — penelusuran requirement bisnis ke keputusan, kontrak, task, dan bukti implementasi"
planning_updated_at: "2026-10-28"
generated_at: "2026-10-26"
owners:
  - "Module Lead & Quality Assurance"
source_commits:
  backend: "16f6b2ad28e0e9cbd374354abe240ff3b163ace0"
  frontend: "HamzahV2"
```

**Arti tanda status bukti:**
* ✅ = Terbukti penuh pada source code, laporan task tracked, atau pengujian live.
* 🟡 = Implementasi sebagian ada pada source code; pengujian atau pelaporan belum lengkap.
* ⛔ = Terblokir oleh keputusan bisnis terbuka, wewenang klinis Komite Medik, atau K3RS.
* tanpa tanda = Belum dikerjakan.

---

## 1. Matriks Rantai Telusur Requirement Bisnis (Traceability Matrix)

| Ref Scope | Deskripsi Kemampuan Bisnis | Keputusan Asal | Kontrak Acuan | Task Backend | Task Frontend | Status Bukti Terkini |
| --- | --- | --- | --- | --- | --- | :---: |
| `IN-01` | Master Data HR (Organisasi, Jabatan, Shift, Cuti, Payroll, KPI, dsb.) | `HRD-DEC-003`, `HRD-DEC-016` | API §MasterData, Validation §Mst | `BE-HRD-002` ✅ | `FE-HRD-021` ✅ | ✅ Frontend 62 halaman aktif; BE 65 controller aktif |
| `IN-02` | Profil & Administrasi Kepegawaian Lintas-Pegawai (Workforce Core) | `HRD-DEC-012`, `HRD-DEC-019` | API §WorkforceCore | `BE-HRD-003` ✅ | `FE-HRD-002` ✅ | ✅ BE controller aktif; FE 6 rute daftar lintas-pegawai aktif ([Laporan FE](../task/report/frontend/FE-HRD-002.md)) |
| `IN-03` | Perencanaan Tenaga Kerja (Workforce Planning) | `HRD-DEC-004`, `HRD-Q-05` | — (Menunggu audit data) | `BE-HRD-017` ⛔ | — | ⛔ Terblokir keputusan skema destruktif `HRD-Q-05` |
| `IN-04` | Rekrutmen, Seleksi Pelamar, dan Hiring | `HRD-DEC-004`, `HRD-Q-05` | — (Menunggu audit data) | `BE-HRD-018` ⛔ | — | ⛔ Terblokir keputusan skema destruktif `HRD-Q-05` |
| `IN-05` | Onboarding, Evaluasi Probation, dan Exit Clearance Karyawan | `HRD-DEC-003`, `HRD-DEC-019` | API §Lifecycle, State §Separation | `BE-HRD-014` ✅ | `FE-HRD-017` ✅ | ✅ BE controller exit clearance & gerbang kliring Finance aktif; FE aktif ([Laporan FE](../task/report/frontend/FE-HRD-017.md)) |
| `IN-06` | Penjadwalan Shift Kerja 24/7, On-Call, dan Tukar Shift | `HRD-DEC-006`, `HRD-DEC-026` | API §Scheduling, Validation §Roster | `BE-HRD-009` ✅ | `FE-HRD-012` ✅ | ✅ BE engine roster 24/7 aktif; FE matriks visual & aksi publish/lock aktif ([Laporan FE](../task/report/frontend/FE-HRD-012.md)) |
| `IN-07` | Presensi Harian, Penutupan Periode, dan Koreksi Kehadiran | `HRD-DEC-028`, `HRD-DEC-029` | API §Attendance, State §Period | `BE-HRD-006` ✅ | `FE-HRD-009` ✅ | ✅ BE 72 endpoint & guard closed period aktif; FE monitoring & modal on-behalf aktif ([Laporan FE](../task/report/frontend/FE-HRD-009.md)) |
| `IN-08` | Administrasi Cuti, Kuota, dan Penyesuaian Saldo Tahunan | `HRD-DEC-007`, `HRD-Q-06` | API §Leave, State §LeaveRequest | `BE-HRD-007` ✅ | `FE-HRD-010` ✅ | ✅ BE penyesuaian saldo berdokumen aktif; FE dasbor saldo & modal alokasi aktif ([Laporan FE](../task/report/frontend/FE-HRD-010.md)) |
| `IN-09` | Administrasi Lembur, Verifikasi SPKL, dan Kompensasi | `HRD-DEC-007`, `HRD-Q-06` | API §Overtime, Validation §Ot | `BE-HRD-008` ✅ | `FE-HRD-011` ✅ | ✅ BE verifikasi jam lembur aktif; FE rekonsiliasi rencana vs mesin absensi aktif ([Laporan FE](../task/report/frontend/FE-HRD-011.md)) |
| `IN-10` | Perhitungan Payroll HR, Komponen Gaji, Angsuran, dan Restitusi | `HRD-DEC-009`, `HRD-Q-10` | API §Payroll, Integration §Finance | `BE-HRD-010` ✅ | `FE-HRD-013` ✅ | ✅ BE kalkulasi payroll & restitusi aktif; FE monitoring dual-tab restitusi & cicilan aktif ([Laporan FE](../task/report/frontend/FE-HRD-013.md)) |
| `IN-11` | Kredensial Medis, SPK/RKK Dokter, dan Kewenangan Klinis | `HRD-DEC-005`, `HRD-Q-08` | — (Menunggu Komite Medik) | `BE-HRD-011` ⛔ | `FE-HRD-014` ⛔ | ⛔ Terblokir persetujuan Komite Medik `HRD-DEC-005` |
| `IN-12` | Pelatihan Wajib RS, Sertifikasi, dan Pemenuhan Kompetensi | `HRD-DEC-003`, `HRD-Q-06` | API §Learning | `BE-HRD-012` ✅ | `FE-HRD-015` ✅ | ✅ BE controller diklat aktif; FE pemantauan masa berlaku sertifikat & modal diklat aktif ([Laporan FE](../task/report/frontend/FE-HRD-015.md)) |
| `IN-13` | Penilaian Kinerja Karyawan (KPI) dan Siklus Review Berkala | `HRD-DEC-003`, `HRD-Q-06` | API §Performance | `BE-HRD-013` ✅ | `FE-HRD-016` ✅ | ✅ BE controller KPI & evaluasi aktif; FE lembar review berkala & skor KPI aktif ([Laporan FE](../task/report/frontend/FE-HRD-016.md)) |
| `IN-14` | Kesehatan dan Keselamatan Kerja Staf (K3RS / MCU Pegawai) | `HRD-DEC-010`, `HRD-DEP-007` | — (Menunggu K3RS) | `BE-HRD-016` ⛔ | `FE-HRD-019` ⛔ | ⛔ Terblokir kebijakan privasi data medis staf K3RS |
| `IN-15` | Hubungan Industrial, Penanganan Kasus, dan Sanksi Disiplin | `HRD-DEC-003`, `HRD-Q-06` | API §EmployeeRelations | `BE-HRD-015` ✅ | `FE-HRD-018` ✅ | ✅ BE controller disiplin aktif; FE dasbor kasus hubungan kerja & modal SP aktif ([Laporan FE](../task/report/frontend/FE-HRD-018.md)) |
| `IN-16` | Layanan Bantuan / Tiket Permohonan HR (Helpdesk Internal) | `HRD-DEC-004`, `HRD-Q-05` | — (Menunggu audit data) | `BE-HRD-020` ⛔ | — | ⛔ Terblokir keputusan skema destruktif `HRD-Q-05` |
| `IN-17` | Perjalanan Dinas dan Klaim Penggantian Biaya (Reimbursement) | `HRD-DEC-004`, `HRD-Q-05` | — (DEFERRED) | `BE-HRD-021` | — | DEFERRED — Prioritas rendah setelah alur utama operasional |
| `IN-18` | Workflow & Kotak Masuk Persetujuan Terpadu Atasan (Inbox) | `HRD-DEC-011`, `HRD-DEC-030` | API §Workflow, State §Instance | `BE-HRD-005` ✅ | `FE-HRD-008` ✅ | ✅ BE workflow approval aktif; FE Unified Approval Inbox & menu sidebar aktif ([Laporan FE](../task/report/frontend/FE-HRD-008.md)) |
| `IN-19` | Portal Layanan Mandiri Karyawan (ESS: Cuti, Lembur, Jadwal, Absensi) | `HRD-DEC-007`, `HRD-DEC-028` | API §SelfServices | `BE-HRD-004` ✅ | `FE-HRD-003..007` ✅ | ✅ BE portal mandiri aktif; FE modul cuti, lembur, tukar shift, koreksi absensi aktif ([Laporan FE-HRD-003](../task/report/frontend/FE-HRD-003.md) s/d [007](../task/report/frontend/FE-HRD-007.md)) |
| `S-D3` | Pendaftaran Benefit Pegawai, Asuransi Tanggungan, dan Plafon | `HRD-DEC-003`, `HRD-DEC-019` | API §Benefit, Validation §Benefit | `BE-HRD-019` ✅ | `FE-HRD-020` ✅ | ✅ BE controller enrollment aktif; FE kepesertaan asuransi & tanggungan aktif ([Laporan FE](../task/report/frontend/FE-HRD-020.md)) |
| `S0-A` | Pendaftaran Prefix Resmi `Hrd` dan `Wfp` pada Registry Modul | `HRD-DEC-019`, `QBE-MOD-002` | Dokumen Engineering Canonical | `BE-HRD-001` ✅ | — | ✅ Terbukti penuh pada registry (baris 11, 12, 118) |
| `S0-B` | Rute API Canonical Kebab-Case dan Compatibility Alias | `HRD-DEC-016`, `QBE-NAM` | API §MasterData | `BE-HRD-002` ✅ | `FE-HRD-001` ✅ | ✅ Terbukti Penuh (8 master data FE constants & BE controllers diselaraskan) ([Laporan FE](../task/report/frontend/FE-HRD-001.md)) |

---

## 2. Penelusuran Terbalik (Task Implementation ke Dokumen Bukti)

| Task ID | Nama Modul / Fitur | Berkas Source Backend Terkait | Berkas Antarmuka Frontend Terkait | Status Validasi | Laporan Tracked |
| --- | --- | --- | --- | :---: | --- |
| `FE-HRD-001` | Rute Kebab-Case Master Data | 8 Master Data Controller HR | `src/lib/constants/hr/master-data/**`, slice | ✅ Lulus Linter | [FE-HRD-001.md](../task/report/frontend/FE-HRD-001.md) |
| `FE-HRD-002` | Workforce Core Cross-Employee | `WorkforceManagementController.cs` | `src/app/hr/workforce-core/**`, view, service | ✅ Lulus Linter | [FE-HRD-002.md](../task/report/frontend/FE-HRD-002.md) |
| `FE-HRD-003` | ESS Cuti Pegawai | `EmployeeSelfServiceLeaveController.cs` | `src/app/self-services/human-resource/leave/**`, view | ✅ Lulus Linter | [FE-HRD-003.md](../task/report/frontend/FE-HRD-003.md) |
| `FE-HRD-004` | ESS Lembur Pegawai | `EmployeeSelfServiceOvertimeController.cs` | `src/app/self-services/human-resource/overtime/**`, view | ✅ Lulus Linter | [FE-HRD-004.md](../task/report/frontend/FE-HRD-004.md) |
| `FE-HRD-005` | ESS Ubah Jadwal & Tukar Shift | `EmployeeSelfServiceScheduleController.cs` | `src/app/self-services/human-resource/schedule/**`, view | ✅ Lulus Linter | [FE-HRD-005.md](../task/report/frontend/FE-HRD-005.md) |
| `FE-HRD-006` | ESS Koreksi Absensi Mandiri | `EmployeeSelfServiceAttendanceCorrectionController.cs` | `src/app/self-services/human-resource/attendance-correction/**` | ✅ Lulus Linter | [FE-HRD-006.md](../task/report/frontend/FE-HRD-006.md) |
| `FE-HRD-007` | Konsolidasi Form Absensi | `EmployeeAttendanceController.cs` | `src/app/self-services/human-resource/attendance/page.jsx` | ✅ Lulus Linter | [FE-HRD-007.md](../task/report/frontend/FE-HRD-007.md) |
| `FE-HRD-008` | Unified Approval Inbox | `WorkflowInstanceController.cs` | `src/app/hr/workflow/approval-inbox/**`, `menu-items.jsx` | ✅ Lulus Linter | [FE-HRD-008.md](../task/report/frontend/FE-HRD-008.md) |
| `FE-HRD-009` | Monitoring Presensi & On-Behalf | `AttendanceAdministrationController.cs` | `src/app/hr/attendance/**`, view, service | ✅ Lulus Linter | [FE-HRD-009.md](../task/report/frontend/FE-HRD-009.md) |
| `FE-HRD-010` | Saldo Cuti & Penyesuaian HR | `LeaveAdministrationController.cs` | `src/app/hr/leave/adjustments/**`, view, service | ✅ Lulus Linter | [FE-HRD-010.md](../task/report/frontend/FE-HRD-010.md) |
| `FE-HRD-011` | Rekonsiliasi Jam Lembur HR | `OvertimeReconciliationController.cs` | `src/app/hr/overtime/reconciliation/**`, view, service | ✅ Lulus Linter | [FE-HRD-011.md](../task/report/frontend/FE-HRD-011.md) |
| `FE-HRD-012` | Matriks Roster Shift & On-Call | `RosterScheduleAdministrationController.cs` | `src/app/hr/scheduling/roster/**`, view, service | ✅ Lulus Linter | [FE-HRD-012.md](../task/report/frontend/FE-HRD-012.md) |
| `FE-HRD-013` | Penggajian Restitusi & Cicilan | `PayrollRestitutionController.cs`, `PayrollRunInstallmentDeductionController.cs` | `src/app/hr/payroll/restitution-installment/**` | ✅ Lulus Linter | [FE-HRD-013.md](../task/report/frontend/FE-HRD-013.md) |
| `FE-HRD-015` | Pelatihan Wajib & Sertifikasi | `TrainingRecordController.cs` | `src/app/hr/learning/training-records/**`, view, service | ✅ Lulus Linter | [FE-HRD-015.md](../task/report/frontend/FE-HRD-015.md) |
| `FE-HRD-016` | Evaluasi Kinerja & Nilai KPI | `PerformanceReviewController.cs` | `src/app/hr/performance/reviews/**`, view, service | ✅ Lulus Linter | [FE-HRD-016.md](../task/report/frontend/FE-HRD-016.md) |
| `FE-HRD-017` | Exit Clearance Staf | `ExitClearanceController.cs` | `src/app/hr/exit-clearance/**`, `exit-clearance-service.js` | ✅ Lulus Linter | [FE-HRD-017.md](../task/report/frontend/FE-HRD-017.md) |
| `FE-HRD-018` | Kasus Disiplin & SP Staf | `DisciplinaryActionController.cs` | `src/app/hr/employee-relations/disciplinary-actions/**` | ✅ Lulus Linter | [FE-HRD-018.md](../task/report/frontend/FE-HRD-018.md) |
| `FE-HRD-020` | Benefit Enrollment & Tanggungan | `EmployeeBenefitEnrollmentController.cs` | `src/app/hr/benefit/enrollments/**`, service | ✅ Lulus Linter | [FE-HRD-020.md](../task/report/frontend/FE-HRD-020.md) |
| `FE-HRD-021` | 62 Master Data HR | `Areas/Corporate/HumanResource/MasterData/**` | `src/app/hr/master-data/**` | ✅ Terpasang & Berjalan | — |
| `BE-HRD-001` | Registry Prefix HR | `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | — | ✅ Lulus Konformansi | [BE-HRD-001.md](../task/report/backend/BE-HRD-001.md) |
| `BE-HRD-002` | Kebab-Case Route Alias | 8 Master Data Controller HR | `src/lib/constants/hr/master-data/**` | ✅ Lulus Build | [BE-HRD-002.md](../task/report/backend/BE-HRD-002.md) |
