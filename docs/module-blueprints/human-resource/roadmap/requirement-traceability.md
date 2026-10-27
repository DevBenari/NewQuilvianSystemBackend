# Requirement Traceability — Modul Human Resource

## Metadata

```yaml
module_id: human-resource
blueprint_id: HRD-BP-001
blueprint_shape: SINGLE
roadmap_revision: 1
status: ACTIVE
status_synced_at: "2026-10-26 — penelusuran requirement bisnis ke keputusan, kontrak, task, dan bukti implementasi"
planning_updated_at: "2026-10-26"
generated_at: "2026-10-26"
owners:
  - "Module Lead & Quality Assurance"
source_commits:
  backend: "16f6b2ad28e0e9cbd374354abe240ff3b163ace0"
  frontend: "2cd1c0610981b649f16a11bc74ac47ce6123e679"
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
| `IN-01` | Master Data HR (Organisasi, Jabatan, Shift, Cuti, Payroll, KPI, dsb.) | `HRD-DEC-003`, `HRD-DEC-016` | API §MasterData, Validation §Mst | `BE-HRD-002` 🟡 | `FE-HRD-021` ✅ | ✅ Frontend 62 halaman aktif; BE 65 controller aktif |
| `IN-02` | Profil & Administrasi Kepegawaian Lintas-Pegawai (Workforce Core) | `HRD-DEC-012`, `HRD-DEC-019` | API §WorkforceCore | `BE-HRD-003` 🟡 | `FE-HRD-002` 🟡 | 🟡 BE detail profil siap; FE daftar lintas pegawai dipetakan |
| `IN-03` | Perencanaan Tenaga Kerja (Workforce Planning) | `HRD-DEC-004`, `HRD-Q-05` | — (Menunggu audit data) | `BE-HRD-017` ⛔ | — | ⛔ Terblokir keputusan skema destruktif `HRD-Q-05` |
| `IN-04` | Rekrutmen, Seleksi Pelamar, dan Hiring | `HRD-DEC-004`, `HRD-Q-05` | — (Menunggu audit data) | `BE-HRD-018` ⛔ | — | ⛔ Terblokir keputusan skema destruktif `HRD-Q-05` |
| `IN-05` | Onboarding, Evaluasi Probation, dan Exit Clearance Karyawan | `HRD-DEC-003`, `HRD-DEC-019` | API §Lifecycle, State §Separation | `BE-HRD-014` ✅ | `FE-HRD-017` 🟡 | ✅ BE controller exit clearance & gerbang bebas tanggungan Finance aktif; FE siap |
| `IN-06` | Penjadwalan Shift Kerja 24/7, On-Call, dan Tukar Shift | `HRD-DEC-006`, `HRD-DEC-026` | API §Scheduling, Validation §Roster | `BE-HRD-009` ✅ | `FE-HRD-012` 🟡 | ✅ BE controller & engine roster 24/7, validasi konflik & penggantian shift aktif |
| `IN-07` | Presensi Harian, Penutupan Periode, dan Koreksi Kehadiran | `HRD-DEC-028`, `HRD-DEC-029` | API §Attendance, State §Period | `BE-HRD-006` ✅ | `FE-HRD-009` 🟡 | ✅ BE 72 endpoint aktif, guard periode tertutup & jalur on-behalf selesai (HRD-DEC-028); FE siap |
| `IN-08` | Administrasi Cuti, Kuota, dan Penyesuaian Saldo Tahunan | `HRD-DEC-007`, `HRD-Q-06` | API §Leave, State §LeaveRequest | `BE-HRD-007` ✅ | `FE-HRD-010` 🟡 | ✅ BE penyesuaian saldo manual berdokumen & pemulihan idempoten pembatalan cuti aktif |
| `IN-09` | Administrasi Lembur, Verifikasi SPKL, dan Kompensasi | `HRD-DEC-007`, `HRD-Q-06` | API §Overtime, Validation §Ot | `BE-HRD-008` ✅ | `FE-HRD-011` 🟡 | ✅ BE verifikasi SPKL, pengali lembur & rekonsiliasi payroll aktif |
| `IN-10` | Perhitungan Payroll HR, Komponen Gaji, Angsuran, dan Restitusi | `HRD-DEC-009`, `HRD-Q-10` | API §Payroll, Integration §Finance | `BE-HRD-010` ✅ | `FE-HRD-013` 🟡 | ✅ BE kalkulasi payroll, pemotongan cicilan piutang RS & restitusi aktif |
| `IN-11` | Kredensial Medis, SPK/RKK Dokter, dan Kewenangan Klinis | `HRD-DEC-005`, `HRD-Q-08` | — (Menunggu Komite Medik) | `BE-HRD-011` ⛔ | `FE-HRD-014` ⛔ | ⛔ Terblokir persetujuan Komite Medik `HRD-DEC-005` |
| `IN-12` | Pelatihan Wajib RS, Sertifikasi, dan Pemenuhan Kompetensi | `HRD-DEC-003`, `HRD-Q-06` | API §Learning | `BE-HRD-012` ✅ | `FE-HRD-015` 🟡 | ✅ BE controller catatan pelatihan & asesmen kompetensi aktif; FE dipetakan |
| `IN-13` | Penilaian Kinerja Karyawan (KPI) dan Siklus Review Berkala | `HRD-DEC-003`, `HRD-Q-06` | API §Performance | `BE-HRD-013` ✅ | `FE-HRD-016` 🟡 | ✅ BE controller review kinerja, rincian skor KPI & kunci finalisasi aktif; FE dipetakan |
| `IN-14` | Kesehatan dan Keselamatan Kerja Staf (K3RS / MCU Pegawai) | `HRD-DEC-010`, `HRD-DEP-007` | — (Menunggu K3RS) | `BE-HRD-016` ⛔ | `FE-HRD-019` ⛔ | ⛔ Terblokir kebijakan privasi data medis staf K3RS |
| `IN-15` | Hubungan Industrial, Penanganan Kasus, dan Sanksi Disiplin | `HRD-DEC-003`, `HRD-Q-06` | API §EmployeeRelations | `BE-HRD-015` ✅ | `FE-HRD-018` 🟡 | ✅ BE controller tindakan disiplin berotorisasi, audit trail sanksi & kerahasiaan aktif |
| `IN-16` | Layanan Bantuan / Tiket Permohonan HR (Helpdesk Internal) | `HRD-DEC-004`, `HRD-Q-05` | — (Menunggu audit data) | `BE-HRD-020` ⛔ | — | ⛔ Terblokir keputusan skema destruktif `HRD-Q-05` |
| `IN-17` | Perjalanan Dinas dan Klaim Penggantian Biaya (Reimbursement) | `HRD-DEC-004`, `HRD-Q-05` | — (DEFERRED) | `BE-HRD-021` | — | DEFERRED — Prioritas rendah setelah alur utama operasional |
| `IN-18` | Workflow & Kotak Masuk Persetujuan Terpadu Atasan (Inbox) | `HRD-DEC-011`, `HRD-DEC-030` | API §Workflow, State §Instance | `BE-HRD-005` ✅ | `FE-HRD-008` 🟡 | ✅ BE controller approval inbox, pelacakan SLA DueAt & delegasi aktif |
| `IN-19` | Portal Layanan Mandiri Karyawan (ESS: Cuti, Lembur, Jadwal, Absensi) | `HRD-DEC-007`, `HRD-DEC-028` | API §SelfServices | `BE-HRD-004` ✅ | `FE-HRD-003..007` 🟡 | ✅ BE controller ESS, validasi kuota cuti & deteksi konflik lembur aktif; FE dipetakan |
| `S-D3` | Pendaftaran Benefit Pegawai, Asuransi Tanggungan, dan Plafon | `HRD-DEC-003`, `HRD-DEC-019` | API §Benefit, Validation §Benefit | `BE-HRD-019` ✅ | `FE-HRD-020` 🟡 | ✅ BE controller enrollment, tanggungan & mutasi saldo aktif; FE siap |
| `S0-A` | Pendaftaran Prefix Resmi `Hrd` dan `Wfp` pada Registry Modul | `HRD-DEC-019`, `QBE-MOD-002` | Dokumen Engineering Canonical | `BE-HRD-001` ✅ | — | ✅ Terbukti penuh pada registry (baris 11, 12, 118) |
| `S0-B` | Rute API Canonical Kebab-Case dan Compatibility Alias | `HRD-DEC-016`, `QBE-NAM` | API §MasterData | `BE-HRD-002` ✅ | `FE-HRD-001` 🟡 | ✅ Terbukti Penuh (8 master controller memiliki rute canonical & alias) |

---

## 2. Penelusuran Terbalik (Task Implementation ke Dokumen Bukti)

| Task ID | Nama Modul / Fitur | Berkas Source Backend Terkait | Berkas Antarmuka Frontend Terkait | Status Validasi |
| --- | --- | --- | --- | :---: |
| `FE-HRD-021` | 62 Master Data HR | `Areas/Corporate/HumanResource/MasterData/**` | `src/app/hr/master-data/**` | ✅ Terpasang & Berjalan |
| `BE-HRD-019` / `FE-HRD-020` | Benefit Management | `EmployeeBenefitEnrollmentController.cs`, `EmployeeBenefitDependentController.cs`, `EmployeeBenefitEnrollmentService.cs` | `src/app/hr/benefit/enrollments/**`, `employee-benefit-enrollment-service.js` | ✅ Terbukti Penuh (Laporan BE-HRD-019) |
| `BE-HRD-014` / `FE-HRD-017` | Exit Clearance | `ExitClearanceController.cs`, `ExitClearanceService.cs` | `src/app/hr/exit-clearance/**`, `exit-clearance-service.js` | ✅ Terbukti Penuh (Laporan BE-HRD-014) |
| `BE-HRD-010` | Payroll Restitution & Angsuran | `PayrollRestitutionController.cs`, `PayrollRunInstallmentDeductionController.cs` | Menunggu integrasi layar payroll | ✅ Terbukti Penuh (Laporan BE-HRD-010) |
| `BE-HRD-001` | Registry Prefix HR | `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | — | ✅ Terpasang & Berjalan ([Laporan](../task/report/backend/BE-HRD-001.md)) |
| `BE-HRD-002` | Kebab-Case Route Alias | 8 Master Data Controller HR | `src/lib/constants/hr/master-data/**` | ✅ Terpasang & Berjalan ([Laporan](../task/report/backend/BE-HRD-002.md)) |
