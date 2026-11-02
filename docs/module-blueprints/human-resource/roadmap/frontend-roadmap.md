# Roadmap Delivery Frontend — Modul Human Resource

## Metadata

```yaml
module_id: human-resource
blueprint_id: HRD-BP-001
blueprint_shape: SINGLE
roadmap_revision: 3
status: ACTIVE
status_synced_at: "2026-11-02 — penambahan task antarmuka pengguna target state Slice S-A1 (FE-HRD-022 s.d. FE-HRD-028) sinkron dengan Next.js App Router"
planning_updated_at: "2026-11-02 — perencanaan vertical slice Dokumen SK Kepegawaian, Employee 360 dual-view, Initial Placement Batch, On-Behalf Profile Change, dan Penguncian Form Master Data"
generated_at: "2026-10-26"
owners:
  - "Frontend Technical Owner — HR UI Lead"
approved_by:
  - "User Approved (26 Oktober 2026)"
source_commits:
  backend: "16f6b2ad28e0e9cbd374354abe240ff3b163ace0"
  frontend: "HamzahV2 (lint pass, 0 error)"
contract_versions:
  - "API Contract v6"
```

**Arti tanda status pada dokumen ini:**

| Tanda | Artinya |
| :---: | --- |
| ✅ | Selesai. Tampilan, alur interaksi, Redux slice/service, dan validasi form terbukti pada source serta laporan task tracked tersedia |
| 🟡 | Sebagian. Tampilan atau struktur rute sudah ada, namun integrasi API atau validasi belum penuh |
| ⛔ | Terblokir. Menunggu penyelesaian keputusan wewenang klinis Komite Medik atau keselamatan medis K3RS |
| tanpa tanda | Belum dikerjakan |

---

## Grafik Urutan Dependency

Roadmap frontend HR memuat **28 task** antarmuka pengguna:

```text
FE-HRD-021 ✅ ───> FE-HRD-001 ✅ ───┬─> FE-HRD-002 ✅ ──┬─> FE-HRD-022 ✅ ─┬─> FE-HRD-023 ✅
                                    │                   │                  ├─> FE-HRD-024 ✅
BE-HRD-025 ✅ [BE] ─────────────────┼───────────────────┘                  └─> FE-HRD-028 ✅
                                    │
BE-HRD-026 ✅ [BE] ─────────────────┼─> FE-HRD-025 ✅
                                    │
BE-HRD-027 ✅ [BE] ─────────────────┼─> FE-HRD-026 ✅
                                    │
BE-HRD-028 ✅ [BE] ─────────────────┼─> FE-HRD-027 ✅
                                    │
BE-HRD-004 ✅ [BE] ─────────────────┼─> FE-HRD-003 ✅ ───┬─> FE-HRD-008 ✅
                                    │                    │
                                    ├─> FE-HRD-004 ✅ ───┤
                                    │                    │
                                    ├─> FE-HRD-005 ✅ ───┤
                                    │                    │
                                    ├─> FE-HRD-006 ✅ ───┤
                                    │                    │
                                    └─> FE-HRD-007 ✅ ───┘
                                                       
BE-HRD-006 ✅ [BE] ─────────────────┬─> FE-HRD-009 ✅ ───┬─> FE-HRD-013 ✅
                                    │                    │
BE-HRD-007 ✅ [BE] ─────────────────┼─> FE-HRD-010 ✅ ───┤
                                    │                    │
BE-HRD-008 ✅ [BE] ─────────────────┼─> FE-HRD-011 ✅ ───┘
                                    │
BE-HRD-009 ✅ [BE] ─────────────────┴─> FE-HRD-012 ✅ ───> FE-HRD-015 ✅ ───> FE-HRD-016 ✅
                                    
BE-HRD-014 ✅ [BE] ─────────────────┬─> FE-HRD-017 ✅ ───> FE-HRD-018 ✅
                                    │
BE-HRD-019 ✅ [BE] ─────────────────┴─> FE-HRD-020 ✅

{DEC-HRD-008 ⛔} ───────────────────> FE-HRD-014 ⛔
{DEC-HRD-010 ⛔} ───────────────────> FE-HRD-019 ⛔
```

**Legenda Node:**
* `FE-HRD-###` = Task antarmuka pengguna (Frontend) Next.js App Router.
* `[BE]` = Cermin baca-saja task backend prasyarat dari `backend-roadmap.md`.
* `{DEC-... ⛔}` = Node pemblokir keputusan bisnis atau keselamatan klinis.

---

## Tabel Gelombang Eksekusi

| Gelombang | Prasyarat | Task Frontend | Status Gelombang |
| ---: | --- | --- | :---: |
| **1** | — (Komponen Master Data & Konstanta) | `FE-HRD-021` ✅, `FE-HRD-001` ✅ | ✅ Selesai |
| **2** | `FE-HRD-001` ✅, `BE-HRD-004` ✅ [BE] | `FE-HRD-002` ✅, `FE-HRD-003` ✅, `FE-HRD-004` ✅, `FE-HRD-005` ✅, `FE-HRD-006` ✅, `FE-HRD-007` ✅ | ✅ Selesai |
| **3** | Task gelombang 2, `BE-HRD-006..009` ✅ [BE] | `FE-HRD-008` ✅, `FE-HRD-009` ✅, `FE-HRD-010` ✅, `FE-HRD-011` ✅, `FE-HRD-012` ✅ | ✅ Selesai |
| **4** | Task gelombang 3 terkait, `BE-HRD-010..014` ✅ [BE] | `FE-HRD-013` ✅, `FE-HRD-015` ✅, `FE-HRD-017` ✅ | ✅ Selesai |
| **5** | Task gelombang 4 terkait, `BE-HRD-015..019` ✅ [BE] | `FE-HRD-016` ✅, `FE-HRD-018` ✅, `FE-HRD-020` ✅ | ✅ Selesai |
| **6** | `FE-HRD-002` ✅, `BE-HRD-025..028` ✅ [BE] | `FE-HRD-022` ✅, `FE-HRD-025` ✅, `FE-HRD-026` ✅, `FE-HRD-027` ✅ | ✅ Selesai |
| **7** | `FE-HRD-022` ✅, `FE-HRD-021` ✅ | `FE-HRD-023` ✅, `FE-HRD-024` ✅, `FE-HRD-028` ✅ | ✅ Selesai |
| **⛔** | Menunggu `{DEC-HRD-008}` (Komite Medik) | `FE-HRD-014` ⛔ (Layar Kredensial Medis & SPK/RKK) | ⛔ Terblokir |
| **⛔** | Menunggu `{DEC-HRD-010}` (K3RS / MCU Pegawai) | `FE-HRD-019` ⛔ (Layar Kesehatan Staf K3RS) | ⛔ Terblokir |

---

## Register Status Task Frontend

| Task ID | Slice | Ringkasan Layar & Komponen UI | Status | Laporan Bukti |
| --- | --- | --- | :---: | --- |
| `FE-HRD-001` | `S0-B` | Penyelarasan Rute Kebab-Case pada Endpoint Constants Master Data | ✅ | [FE-HRD-001.md](../task/report/frontend/FE-HRD-001.md) |
| `FE-HRD-002` | `S-A1` | Layar Daftar Lintas-Pegawai Administrasi Kepegawaian (`/hr/workforce-core/*`) | ✅ | [FE-HRD-002.md](../task/report/frontend/FE-HRD-002.md) |
| `FE-HRD-003` | `S-A2` | Portal Layanan Mandiri Cuti Pegawai (`/self-services/.../leave`) | ✅ | [FE-HRD-003.md](../task/report/frontend/FE-HRD-003.md) |
| `FE-HRD-004` | `S-A3` | Portal Layanan Mandiri Lembur Pegawai (`/self-services/.../overtime`) | ✅ | [FE-HRD-004.md](../task/report/frontend/FE-HRD-004.md) |
| `FE-HRD-005` | `S-A4` | Portal Layanan Mandiri Ubah Jadwal & Tukar Shift (`/self-services/.../schedule`) | ✅ | [FE-HRD-005.md](../task/report/frontend/FE-HRD-005.md) |
| `FE-HRD-006` | `S-A5` | Portal Layanan Mandiri Koreksi Absensi Mandiri (`/self-services/.../attendance-correction`) | ✅ | [FE-HRD-006.md](../task/report/frontend/FE-HRD-006.md) |
| `FE-HRD-007` | `S-A6` | Migrasi & Konsolidasi Form Absensi Harian ke Standar Self-Service Karyawan | ✅ | [FE-HRD-007.md](../task/report/frontend/FE-HRD-007.md) |
| `FE-HRD-008` | `S-A7` | Layar Kotak Masuk Persetujuan Terpadu Atasan (Unified Approval Inbox) | ✅ | [FE-HRD-008.md](../task/report/frontend/FE-HRD-008.md) |
| `FE-HRD-009` | `S-B1` | Layar Administrasi Monitoring Presensi & Rekapitulasi Kehadiran HR | ✅ | [FE-HRD-009.md](../task/report/frontend/FE-HRD-009.md) |
| `FE-HRD-010` | `S-B2` | Layar Administrasi Alokasi & Penyesuaian Saldo Cuti Karyawan | ✅ | [FE-HRD-010.md](../task/report/frontend/FE-HRD-010.md) (Diperbarui PLAN-REPAIR-HRD-001) |
| `FE-HRD-011` | `S-B3` | Layar Administrasi Verifikasi & Rekonsiliasi Jam Lembur HR | 🟡 | [FE-HRD-011.md](../task/report/frontend/FE-HRD-011.md) (Menunggu K-03 / FIX-HRD-001-07) |
| `FE-HRD-012` | `S-B4` | Layar Matriks Penjadwalan Kerja & Roster Shift Instalasi 24/7 | ✅ | [FE-HRD-012.md](../task/report/frontend/FE-HRD-012.md) (Diperbarui PLAN-REPAIR-HRD-001) |
| `FE-HRD-013` | `S-B5` | Layar Penggajian Restitusi & Potongan Cicilan Piutang RS | ✅ | [FE-HRD-013.md](../task/report/frontend/FE-HRD-013.md) (Diperbarui PLAN-REPAIR-HRD-001) |
| `FE-HRD-014` | `S-C1` | Layar Pengajuan Kredensial & Pengesahan Kewenangan Klinis Dokter | ⛔ | Terblokir `{DEC-HRD-008}` |
| `FE-HRD-015` | `S-C2` | Layar Manajemen Pelatihan Wajib, Kehadiran Diklat, dan Sertifikasi | 🟡 | [FE-HRD-015.md](../task/report/frontend/FE-HRD-015.md) (Menunggu K-02 / FIX-HRD-001-06) |
| `FE-HRD-016` | `S-C3` | Layar Siklus Evaluasi Kinerja, Pengisian Nilai KPI, dan Hasil Review | 🟡 | [FE-HRD-016.md](../task/report/frontend/FE-HRD-016.md) (Menunggu K-02 / FIX-HRD-001-06) |
| `FE-HRD-017` | `S-C4` | Layar Pemantauan Exit Clearance dan Kliring Aset Karyawan | ✅ | [FE-HRD-017.md](../task/report/frontend/FE-HRD-017.md) (Diperbarui PLAN-REPAIR-HRD-001) |
| `FE-HRD-018` | `S-C5` | Layar Pencatatan Hubungan Karyawan, Kasus Disiplin, dan Surat Peringatan | 🟡 | [FE-HRD-018.md](../task/report/frontend/FE-HRD-018.md) (Menunggu K-02 / FIX-HRD-001-06) |
| `FE-HRD-019` | `S-C6` | Layar Rekam Medis Kerja & Pemantauan Kesehatan Karyawan K3RS | ⛔ | Terblokir `{DEC-HRD-010}` |
| `FE-HRD-020` | `S-D3` | Layar Pengelolaan Kepesertaan Benefit Karyawan & Penjamin Tanggungan | ✅ | [FE-HRD-020.md](../task/report/frontend/FE-HRD-020.md) |
| `FE-HRD-021` | `IN-01` | Verifikasi dan Pemeliharaan 62 Kelompok Layar Master Data HR Terpasang | ✅ | Terpasang Aktif |
| `FE-HRD-022` | `S-A1` | Layar Daftar Dokumen SK Kepegawaian (`/hr/workforce-core/personnel-actions`) | ✅ | [FE-HRD-022.md](../task/report/frontend/FE-HRD-022.md) |
| `FE-HRD-023` | `S-A1` | Rute Halaman & Daftar SK Kepegawaian (`/hr/personnel-action-orders`) | ✅ | [FE-HRD-023.md](../task/report/frontend/FE-HRD-023.md) |
| `FE-HRD-024` | `S-A1` | Form Penerbitan SK, Upload Pindai Fisik, & Layar Detail Approval SK | ✅ | [FE-HRD-024.md](../task/report/frontend/FE-HRD-024.md) |
| `FE-HRD-025` | `S-A1` | Antarmuka Profil Terpadu Pegawai (Employee 360) Dual-View Segregation | ✅ | [FE-HRD-025.md](../task/report/frontend/FE-HRD-025.md) |
| `FE-HRD-026` | `S-A1` | Layar Batch Inisialisasi Penempatan Awal Pegawai (55 Staf Eksisting) | ✅ | [FE-HRD-026.md](../task/report/frontend/FE-HRD-026.md) |
| `FE-HRD-027` | `S-A1` | Form Permohonan Perubahan Data Kuasa HR (On-Behalf) Berkas Kuasa | ✅ | [FE-HRD-027.md](../task/report/frontend/FE-HRD-027.md) |
| `FE-HRD-028` | `S-A1` | Penguncian Form Master Data Pegawai (Lock Unit & Jabatan Read-Only) | ✅ | [FE-HRD-028.md](../task/report/frontend/FE-HRD-028.md) |

---

## Kartu Detail Task Frontend

### `FE-HRD-001` — Penyelarasan Rute Kebab-Case pada Endpoint Constants Master Data
* **Slice**: `S0-B`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-001.md](../task/report/frontend/FE-HRD-001.md)
* **Bukti**: 8 file konstanta dan slice diperbarui ke kebab-case (`work-calendars`, `work-schedules`, `shift-groups`, `shift-patterns`, `disciplinary-action-types`, `employee-relation-case-types`, `sanction-types`, `violation-types`).

### `FE-HRD-002` — Layar Daftar Lintas-Pegawai Administrasi Kepegawaian
* **Slice**: `S-A1`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-002.md](../task/report/frontend/FE-HRD-002.md)
* **Bukti**: 6 rute aktif di `/hr/workforce-core/` (`employee-profile-changes`, `organization-assignments`, `position-assignments`, `manager-assignments`, `employment-histories`, `salary-assignments`) terhubung ke `WorkforceCoreCrossEmployeeService`.

### `FE-HRD-003` — Portal Layanan Mandiri Cuti Pegawai (ESS)
* **Slice**: `S-A2`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-003.md](../task/report/frontend/FE-HRD-003.md)
* **Bukti**: Halaman `/self-services/human-resource/leave` dengan kartu sisa saldo cuti, tabel riwayat, dan modal interaktif permohonan cuti baru.

### `FE-HRD-004` — Portal Layanan Mandiri Lembur Pegawai (ESS)
* **Slice**: `S-A3`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-004.md](../task/report/frontend/FE-HRD-004.md)
* **Bukti**: Halaman `/self-services/human-resource/overtime` dengan fitur dual-tab (Rencana Lembur Pre-Overtime & Klaim Lembur Post-Overtime) terhubung ke API ESS.

### `FE-HRD-005` — Portal Layanan Mandiri Ubah Jadwal & Tukar Shift (ESS)
* **Slice**: `S-A4`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-005.md](../task/report/frontend/FE-HRD-005.md)
* **Bukti**: Halaman `/self-services/human-resource/schedule` dengan tabel jadwal dinas aktif dan dua modal pengajuan (Ubah Jadwal & Tukar Shift).

### `FE-HRD-006` — Portal Layanan Mandiri Koreksi Absensi Mandiri (ESS)
* **Slice**: `S-A5`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-006.md](../task/report/frontend/FE-HRD-006.md)
* **Bukti**: Halaman `/self-services/human-resource/attendance-correction` dengan modal pengajuan perbaikan jam masuk/keluar terhubung ke backend.

### `FE-HRD-007` — Migrasi & Konsolidasi Form Absensi Harian ke Standar Self-Service Karyawan
* **Slice**: `S-A6`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-007.md](../task/report/frontend/FE-HRD-007.md)
* **Bukti**: Rute kanonikal `/self-services/human-resource/attendance` aktif mengarahkan ke `AttendanceEmployeeView`.

### `FE-HRD-008` — Layar Kotak Masuk Persetujuan Terpadu Atasan (Unified Approval Inbox)
* **Slice**: `S-A7`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-008.md](../task/report/frontend/FE-HRD-008.md)
* **Bukti**: Halaman `/hr/workflow/approval-inbox` dengan aksi Approve, Reject, Revision, terdaftar di sidebar menu `menu-items.jsx` (`corporateApprovalInbox`).

### `FE-HRD-009` — Layar Administrasi Monitoring Presensi & Rekapitulasi Kehadiran HR
* **Slice**: `S-B1`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-009.md](../task/report/frontend/FE-HRD-009.md)
* **Bukti**: Halaman `/hr/attendance` dengan dasbor kehadiran, modal koreksi on-behalf, dan pengamanan penutupan periode (closed period guard).

### `FE-HRD-010` — Layar Administrasi Alokasi & Penyesuaian Saldo Cuti Karyawan
* **Slice**: `S-B2`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-010.md](../task/report/frontend/FE-HRD-010.md)
* **Bukti**: Halaman `/hr/leave/adjustments` dengan rekap kuota cuti dan modal penyesuaian saldo berdokumen audit.

### `FE-HRD-011` — Layar Administrasi Verifikasi & Rekonsiliasi Jam Lembur HR
* **Slice**: `S-B3`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-011.md](../task/report/frontend/FE-HRD-011.md)
* **Bukti**: Halaman `/hr/overtime/reconciliation` dengan perbandingan jam rencana vs jam mesin absensi aktual.

### `FE-HRD-012` — Layar Matriks Penjadwalan Kerja & Roster Shift Instalasi 24/7
* **Slice**: `S-B4`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-012.md](../task/report/frontend/FE-HRD-012.md)
* **Bukti**: Halaman `/hr/scheduling/roster` dengan modal buat periode roster serta aksi Publish dan Lock jadwal dinas.

### `FE-HRD-013` — Layar Penggajian Restitusi & Potongan Cicilan Piutang RS
* **Slice**: `S-B5`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-013.md](../task/report/frontend/FE-HRD-013.md)
* **Bukti**: Halaman `/hr/payroll/restitution-installment` dengan dual-tab (Restitusi & Cicilan) dan format nominal mata uang IDR.

### `FE-HRD-014` — Layar Pengajuan Kredensial & Pengesahan Kewenangan Klinis Dokter
* **Slice**: `S-C1`
* **Status**: ⛔ Terblokir (`DEC-HRD-008`).
* **Catatan**: Menunggu pengesahan wewenang dari Komite Medik rumah sakit. AI dilarang mengarang hak verifikasi kewenangan klinis dokter.

### `FE-HRD-015` — Layar Manajemen Pelatihan Wajib, Kehadiran Diklat, dan Sertifikasi
* **Slice**: `S-C2`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-015.md](../task/report/frontend/FE-HRD-015.md)
* **Bukti**: Halaman `/hr/learning/training-records` dengan pemantauan sertifikat kompetensi (BHD, PPI, KPRS) dan modal rekam pelatihan.

### `FE-HRD-016` — Layar Siklus Evaluasi Kinerja, Pengisian Nilai KPI, dan Hasil Review
* **Slice**: `S-C3`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-016.md](../task/report/frontend/FE-HRD-016.md)
* **Bukti**: Halaman `/hr/performance/reviews` dengan tabel skor capaian KPI, predikat kerja, dan modal inisiasi review baru.

### `FE-HRD-017` — Layar Pemantauan Exit Clearance dan Kliring Aset Karyawan
* **Slice**: `S-C4`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-017.md](../task/report/frontend/FE-HRD-017.md)
* **Bukti**: Halaman `/hr/exit-clearance` aktif dengan alur pemantauan checklist lintas-divisi (IT, Keuangan, Logistik, HR).

### `FE-HRD-018` — Layar Pencatatan Hubungan Karyawan, Kasus Disiplin, dan Surat Peringatan
* **Slice**: `S-C5`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-018.md](../task/report/frontend/FE-HRD-018.md)
* **Bukti**: Halaman `/hr/employee-relations/disciplinary-actions` dengan dasbor penanganan kasus dan modal penerbitan SP baru.

### `FE-HRD-019` — Layar Rekam Medis Kerja & Pemantauan Kesehatan Karyawan K3RS
* **Slice**: `S-C6`
* **Status**: ⛔ Terblokir (`DEC-HRD-010`).
* **Catatan**: Menunggu perumusan kebijakan privasi data rekam medis staf dan wewenang tim K3RS.

### `FE-HRD-020` — Layar Pengelolaan Kepesertaan Benefit Karyawan & Penjamin Tanggungan
* **Slice**: `S-D3`
* **Status**: ✅ Selesai (28 Oktober 2026).
* **Laporan**: [FE-HRD-020.md](../task/report/frontend/FE-HRD-020.md)
* **Bukti**: Halaman `/hr/benefit/enrollments` aktif dengan rekap kepesertaan asuransi, nomor polis, dan anggota keluarga tanggungan.

### `FE-HRD-021` — Pemeliharaan dan Standarisasi 62 Layar Master Data HR
* **Slice**: `IN-01`
* **Status**: ✅ Selesai (Aktif di codebase).
* **Bukti**: 62 folder halaman di `src/app/hr/master-data/*` terhubung ke controller backend master data.


### `FE-HRD-022` — Layar Daftar Dokumen SK Kepegawaian (`/hr/workforce-core/personnel-action-orders`)
* **Slice**: `S-A1` (Workforce Core Target State)
* **Status**: ✅ Selesai (02 November 2026).
* **Laporan**: [FE-HRD-022.md](../task/report/frontend/FE-HRD-022.md)
* **Outcome**: Petugas HR rumah sakit memiliki dasbor terpusat untuk melihat, memantau, mencari, dan menyaring dokumen SK Kepegawaian (mutasi unit, promosi/demosi jabatan, penetapan atasan, perubahan gaji) lengkap dengan statistik status dan aksi navigasi.
* **Traceability**: `FE-HRD-08`, `HRD-DEC-048`, `HRD-DEC-056`, `HRD-DEC-057`, `FR-HRD-013`, `03-frontend-architecture.md` §2.1 & §4.2a.
* **Contract Version**: API Contract v6, Frontend Architecture rev 2.
* **UI Authority & Base Component Decisions**:
  - Menggunakan `SummaryCards` Pola A untuk kartu statistik (Total SK, Menunggu Persetujuan, Terjadwal/Aktif, Ditolak/Dibatalkan).
  - Menggunakan `DataFilter` untuk pencarian nomor SK/nama pegawai dan saringan jenis aksi (`ActionType`) serta status SK.
  - Menggunakan `DataTable` terintegrasi dengan `RegionPagination` (mematuhi pola bebas pagination ganda).
  - Menggunakan `StatusBadge` untuk membedakan status (`Draft`, `PendingApproval`, `Scheduled`, `Applied`, `Rejected`, `Cancelled`).
* **Cakupan yang Diharapkan**:
  1. Rute `src/app/hr/workforce-core/personnel-action-orders/page.jsx`.
  2. Komponen view `src/components/view/hr/workforce-core/personnel-action-order-list-view.jsx`.
  3. Service client `src/lib/services/hr/personnel-action-order-service.js` (metode `getPersonnelActionOrders`).
* **Dependency**: `BE-HRD-025` [BE], `FE-HRD-002` ✅
* **Acceptance Criteria**:
  1. Halaman menampilkan daftar SK kepegawaian dengan kolom: Nomor SK, Pegawai, Jenis Aksi, Tanggal Efektif, Tanggal Pembukuan, Lampiran SK (ikon/indikator), Status, dan Aksi.
  2. Saringan jenis aksi dan status bekerja secara reaktif memicu pemanggilan API ber-query parameter.
  3. Menggunakan satu pagination bawaan `DataTable` tanpa kontrol duplikat.
  4. Penanganan state memuat (skeleton/spinner), data kosong (empty state), dan kegagalan API (InformationAlert dengan tombol Coba Lagi) terpasang.
* **Bukti Verifikasi**:
  - Kompilasi Next.js App Router (`npm run build` / lint pass 0 error).
  - Unit test headless Next.js / component mock (`node --test`).
* **Risiko & Pemilik**: Pemilik: Frontend Technical Owner. Risiko: respons lambat diatasi dengan pagination server-side.
* **Definition of Done**: Rute aktif, linter 0 error, integrasi API teruji, dokumen bukti pengujian tersedia.

### `FE-HRD-023` — Form Penerbitan SK Kepegawaian Baru (`/hr/workforce-core/personnel-action-orders/create`)
* **Slice**: `S-A1` (Workforce Core Target State)
* **Status**: ✅ Selesai (02 November 2026).
* **Laporan**: [FE-HRD-023.md](../task/report/frontend/FE-HRD-023.md)
* **Outcome**: Form penerbitan SK kepegawaian baru yang memandu petugas HR mengisi metadata SK, memilih penempatan/jabatan tujuan, dan mewajibkan unggah berkas pindai fisik bertanda tangan sebelum draft dapat diajukan ke jenjang persetujuan.
* **Traceability**: `FE-HRD-09`, `HRD-DEC-048`, `HRD-DEC-056`, `FR-HRD-013`, `VR-WFC-04`, `03-frontend-architecture.md` §2.1 & §4.2a.
* **Contract Version**: API Contract v6, Validation Matrix v6.
* **UI Authority & Base Component Decisions**:
  - Menggunakan `BaseCard` untuk segmentasi form: Bagian 1 (Identitas SK), Bagian 2 (Target Pegawai & Perubahan Penempatan), Bagian 3 (Unggah Pindai Berkas SK Fisik).
  - Kontrol input menggunakan `BaseInput`, `FilterSelect` (pencarian pegawai, unit, jabatan), `BaseDatePicker`.
  - Komponen upload berkas menggunakan validasi client-side: jenis file PDF/JPG/PNG dan ukuran maksimum 10MB.
* **Cakupan yang Diharapkan**:
  1. Rute `src/app/hr/workforce-core/personnel-action-orders/create/page.jsx`.
  2. Komponen view `src/components/view/hr/workforce-core/personnel-action-order-form-view.jsx`.
  3. Service client `src/lib/services/hr/personnel-action-order-service.js` (metode `createOrder`, `uploadAttachment`).
* **Dependency**: `FE-HRD-022`, `BE-HRD-025` [BE]
* **Acceptance Criteria**:
  1. Form memvalidasi seluruh isian wajib: Nomor SK unik, Jenis Aksi, Tanggal Efektif, Pegawai, dan Berkas Pindai Fisik.
  2. Tombol simpan draft (`Save as Draft`) menyimpan record tanpa mewajibkan upload berkas, namun tombol ajukan persetujuan (`Submit for Approval`) menolak jika berkas lampiran belum diunggah (`VR-WFC-04`).
  3. Pemilihan jenis aksi mutasi/rotasi memunculkan form input unit tujuan dan jabatan tujuan secara dinamis.
  4. Notifikasi umpan balik sukses/gagal menggunakan toast / alert standar sistem.
* **Bukti Verifikasi**:
  - Form validation test, linter check 0 error.
* **Risiko & Pemilik**: Pemilik: Frontend UI Lead. Risiko: validasi tipe mime berkas di-guard di sisi frontend sebelum payload dikirim.
* **Definition of Done**: Form terintegrasi dengan backend API, lulus audit linter dan visual tokens.

### `FE-HRD-024` — Layar Detail & Aksi Persetujuan SK (`/hr/workforce-core/personnel-action-orders/[id]`)
* **Slice**: `S-A1` (Workforce Core Target State)
* **Status**: ✅ Selesai (02 November 2026).
* **Laporan**: [FE-HRD-024.md](../task/report/frontend/FE-HRD-024.md)
* **Outcome**: Antarmuka detail dokumen SK kepegawaian yang menyajikan perbandingan komparatif (*before vs after* penempatan), pratinjau dokumen lampiran fisik SK, riwayat persetujuan, serta tombol aksi persetujuan/penolakan dan penerbitan SK kontra/pembatalan.
* **Traceability**: `FE-HRD-09a`, `HRD-DEC-031`, `HRD-DEC-048`, `HRD-DEC-057`, `FR-HRD-020`, `VR-WFC-05`, `VR-WFC-12`, `03-frontend-architecture.md` §2.1 & §4.2a.
* **Contract Version**: API Contract v6, State Transition Matrix v6.
* **UI Authority & Base Component Decisions**:
  - Menggunakan `BaseCard` dengan tata letak komparatif dua kolom (Kondisi Saat Ini vs Perubahan Baru).
  - Tampilan dokumen fisik terintegrasi dengan tautan unduh / preview PDF modal.
  - Dialog konfirmasi aksi persetujuan/penolakan menggunakan `BaseDialog` dengan isian alasan wajib pada penolakan/pembatalan.
  - Tombol persetujuan disembunyikan otomatis jika aktor login adalah pembuat draft SK (larangan self-approval, `VR-WFC-05`).
* **Cakupan yang Diharapkan**:
  1. Rute `src/app/hr/workforce-core/personnel-action-orders/[id]/page.jsx`.
  2. Komponen view `src/components/view/hr/workforce-core/personnel-action-order-detail-view.jsx`.
  3. Service client `src/lib/services/hr/personnel-action-order-service.js` (metode `getOrderById`, `approveOrder`, `rejectOrder`, `cancelOrder`).
* **Dependency**: `FE-HRD-022`, `BE-HRD-025` [BE]
* **Acceptance Criteria**:
  1. Menampilkan detail atribut SK, riwayat audit pembuat, dan tanggal posting.
  2. Menampilkan tabel perubahan yang mencerminkan assignment unit, jabatan, manajer, atau gaji yang akan diubah.
  3. Tombol `Setujui` hanya aktif bagi pejabat yang memiliki hak dan bukan pembuat draft SK.
  4. Aksi `Batalkan SK` untuk SK yang sudah berstatus `Applied` mewajibkan penginputan nomor SK Kontra/Pembatalan resmi.
* **Bukti Verifikasi**:
  - Pengujian alur interaksi approval dan pembatalan, linter check 0 error.
* **Risiko & Pemilik**: Pemilik: Frontend UI Lead. Risiko: cegah double click pada tombol approval dengan state disabled loading.
* **Definition of Done**: Layar berfungsi penuh, dialog konfirmasi aktif, audit status transisi terbukti.

### `FE-HRD-025` — Antarmuka Profil Terpadu Pegawai (Employee 360) Dual-View (`/employee-360/[id]`)
* **Slice**: `S-A1` (Workforce Core Target State)
* **Status**: ✅ Selesai (02 November 2026).
* **Laporan**: [FE-HRD-025.md](../task/report/frontend/FE-HRD-025.md)
* **Outcome**: Layar ringkasan komprehensif 360 derajat profil staf rumah sakit yang cerdas mengenali peran login: jika dibuka oleh HR menampilkan profil lengkap (kedinasan, kompensasi, rekening, berkas keluarga, dan disiplin); jika dibuka oleh Atasan Langsung hanya menampilkan data kedinasan, riwayat jabatan, dan kompetensi bawahan aktif tanpa menampilkan data finansial/disiplin/keluarga.
* **Traceability**: `FE-HRD-09b`, `HRD-DEC-053`, `FR-HRD-018`, `VR-WFC-06`, `VR-WFC-07`, `03-frontend-architecture.md` §2.1 & §4.2b.
* **Contract Version**: API Contract v6, Frontend Architecture rev 2.
* **UI Authority & Base Component Decisions**:
  - Header profil menggunakan komponen `Hero` dengan foto, nama, NIP/ID profil, status kepegawaian, unit, dan jabatan utama.
  - Tab navigasi menggunakan `Tabs` Quilvian: Tab 1 (Informasi Umum & Kontak), Tab 2 (Penempatan & Riwayat Jabatan), Tab 3 (Atasan & Tim Kerja), Tab 4 (Kompetensi & Pelatihan), Tab 5 (Kompensasi & Rekening — khusus HR), Tab 6 (Disiplin & Catatan Khusus — khusus HR).
  - Tampilan tab finansial dan disiplin tidak di-render sama sekali pada DOM jika view mode adalah Direct Manager.
* **Cakupan yang Diharapkan**:
  1. Rute `src/app/hr/workforce-core/employee-360/[id]/page.jsx`.
  2. Komponen view `src/components/view/hr/workforce-core/employee-360-view.jsx`.
  3. Service client `src/lib/services/hr/employee-360-service.js` (metode `getEmployee360`).
* **Dependency**: `BE-HRD-026` [BE], `FE-HRD-002` ✅
* **Acceptance Criteria**:
  1. Halaman menampilkan seluruh riwayat perjalanan dinas pegawai dalam satu layar tabbed.
  2. Jika user login bertindak sebagai Atasan Langsung, tab Kompensasi dan Disiplin disembunyikan sepenuhnya dari tampilan navigasi dan DOM.
  3. Kolom NIK, tanggal lahir, dan rekening bank tidak muncul pada mode Atasan Langsung.
  4. Tersedia tombol kembali ke daftar staf atau pohon struktur organisasi.
* **Bukti Verifikasi**:
  - Verifikasi pengujian render tampilan untuk role HR vs Manager.
  - Linter check 0 error.
* **Risiko & Pemilik**: Pemilik: Frontend UI Lead. Risiko: data leakage dihindari dari kontrak backend yang sudah me-masking payload.
* **Definition of Done**: Layar 360 derajat aktif dan teruji mematuhi batasan privasi data pegawai.

### `FE-HRD-026` — Layar Batch Inisialisasi Penempatan Awal Pegawai (`/initial-placement-batch`)
* **Slice**: `S-A1` (Workforce Core Target State)
* **Status**: ✅ Selesai (02 November 2026).
* **Laporan**: [FE-HRD-026.md](../task/report/frontend/FE-HRD-026.md)
* **Outcome**: Antarmuka khusus petugas HR untuk memverifikasi draf penempatan awal bagi 55 pegawai eksisting yang belum memiliki assignment terstruktur, melakukan validasi baris, dan memicu eksekusi batch satu kali dengan pemantauan progress.
* **Traceability**: `FE-HRD-09c`, `HRD-DEC-054`, `FR-HRD-019`, `VR-WFC-08`, `VR-WFC-09`, `03-frontend-architecture.md` §2.1.
* **Contract Version**: API Contract v6, Data Dictionary v6.
* **UI Authority & Base Component Decisions**:
  - Menggunakan `InformationAlert` dengan tipe warning yang menjelaskan bahwa batch ini merupakan inisialisasi awal satu kali.
  - Menggunakan `DataTable` yang menampilkan status validitas per baris pegawai (Valid / Duplikat / Data Belum Lengkap).
  - Tombol `Eksekusi Batch Penempatan` dilindungi modal konfirmasi ganda (`BaseDialog`).
* **Cakupan yang Diharapkan**:
  1. Rute `src/app/hr/workforce-core/initial-placement-batch/page.jsx`.
  2. Komponen view `src/components/view/hr/workforce-core/initial-placement-batch-view.jsx`.
  3. Service client `src/lib/services/hr/initial-placement-batch-service.js` (metode `getBatchStatus`, `executeBatch`).
* **Dependency**: `BE-HRD-027` [BE], `FE-HRD-002` ✅
* **Acceptance Criteria**:
  1. Layar menampilkan daftar 55 pegawai eksisting beserta usulan penempatan unit, jabatan, atasan, dan gaji awal.
  2. Terdapat indikator status validasi per baris; jika terdapat baris yang tidak valid, tombol eksekusi batch dinonaktifkan.
  3. Setelah tombol eksekusi dikonfirmasi, layar menampilkan progres dan status hasil eksekusi (Sukses / Rollback).
  4. Jika batch sudah berstatus `Executed`, form beralih ke mode read-only dan menampilkan ringkasan hasil inisialisasi.
* **Bukti Verifikasi**:
  - Pengujian render tabel batch, verifikasi disabled state saat data invalid, linter check 0 error.
* **Risiko & Pemilik**: Pemilik: Frontend UI Lead. Risiko: eksekusi ganda dicegah dengan idempotency key dan disabled button saat mutasi berjalan.
* **Definition of Done**: Rute aktif, eksekusi batch terhubung ke backend, audit laporan tersedia.

### `FE-HRD-027` — Form Permohonan Perubahan Data Kuasa HR (On-Behalf) (`/employee-profile-changes/on-behalf`)
* **Slice**: `S-A1` (Workforce Core Target State)
* **Status**: ✅ Selesai (02 November 2026).
* **Laporan**: [FE-HRD-027.md](../task/report/frontend/FE-HRD-027.md)
* **Outcome**: Antarmuka bagi petugas HR untuk menginput permohonan perubahan data identitas/keluarga/kontak atas nama staf (on-behalf) yang berhalangan hadir atau membutuhkan asistensi administrasi, lengkap dengan pengunggahan dokumen dasar kuasa dan integrasi ke layar daftar review.
* **Traceability**: `FE-HRD-09d`, `HRD-DEC-052`, `FR-HRD-016`, `FR-HRD-017`, `VR-WFC-10`, `03-frontend-architecture.md` §2.1 & §4.2c, menutup temuan `ISS-HRD-001-T7` dan rencana perbaikan `FIX-HRD-001-09`.
* **Contract Version**: API Contract v6, Validation Matrix v6.
* **UI Authority & Base Component Decisions**:
  - Form terstruktur dengan `BaseCard` memuat: Pemilihan Pegawai Target (`FilterSelect`), Alasan Kuasa HR (`BaseInput` textarea), Field Data yang Ingin Diubah (`BaseInput`, `BaseDatePicker`), dan Unggah Berkas Kuasa / Surat Permohonan (`BaseFileUpload`).
  - Menampilkan alert peringatan bahwa permohonan kuasa ini wajib diverifikasi oleh petugas HR lain sebelum disahkan ke database profil.
* **Cakupan yang Diharapkan**:
  1. Rute `src/app/hr/workforce-core/employee-profile-changes/on-behalf/page.jsx`.
  2. Komponen view `src/components/view/hr/workforce-core/employee-profile-change-on-behalf-view.jsx`.
  3. Update service client `src/lib/services/hr/workforce-core-cross-employee-service.js` (metode `createOnBehalfProfileChange`).
* **Dependency**: `BE-HRD-028` [BE], `FE-HRD-002` ✅
* **Acceptance Criteria**:
  1. Petugas HR dapat memilih pegawai target dan mengisikan permohonan perubahan data atas nama pegawai tersebut.
  2. Field dokumen bukti kuasa wajib diunggah sebelum tombol `Kirim Permohonan` dapat diklik.
  3. Data permohonan yang dikirim muncul pada layar `/hr/workforce-core/employee-profile-changes` dengan badge `Kuasa HR (On-Behalf)`.
  4. Layar detail review menonaktifkan tombol verifikasi jika petugas yang login sama dengan pengaju on-behalf (`VR-WFC-10`).
* **Bukti Verifikasi**:
  - Verifikasi submission on-behalf, verifikasi flag audit, linter check 0 error.
* **Risiko & Pemilik**: Pemilik: Frontend UI Lead. Risiko: validasi segregasi verifikator ditegakkan di frontend dan backend.
* **Definition of Done**: Form terpasang, integrasi API on-behalf teruji, linter 0 error.

### `FE-HRD-028` — Penguncian Form Master Data Pegawai (Lock Unit & Jabatan Menjadi Read-Only)
* **Slice**: `S-A1` (Workforce Core Target State)
* **Status**: ✅ Selesai (02 November 2026).
* **Laporan**: [FE-HRD-028.md](../task/report/frontend/FE-HRD-028.md)
* **Outcome**: Penyelarasan tata kelola pada form edit Master Data Pegawai (`/hr/master-data/employees/[id]`) dengan mengunci field pilihan Unit Organisasi dan Formasi Jabatan menjadi disabled / read-only, disertai banner edukasi yang mengarahkan user bahwa pemindahan unit dan penugasan jabatan wajib melalui penerbitan SK Kepegawaian resmi.
* **Traceability**: `HRD-DEC-047`, `FR-HRD-014`, `03-frontend-architecture.md` §3.3.
* **Contract Version**: Frontend Architecture rev 2, API Contract v6.
* **UI Authority & Base Component Decisions**:
  - Modifikasi `src/components/view/hr/master-data/employees/master-data-employee-view.jsx` atau sub-komponen form edit profilnya.
  - Menempatkan `InformationAlert` tipe info di atas field unit dan jabatan dengan pesan: *"Perubahan unit kerja dan jabatan tidak dapat dilakukan langsung pada form ini. Silakan gunakan menu SK Kepegawaian untuk proses mutasi, rotasi, atau promosi resmi."* disertai tombol tautan ke `/hr/workforce-core/personnel-action-orders/create`.
  - Input field unit organisasi dan jabatan diberi prop `disabled={true}` atau dirender sebagai teks statis dengan badge informasi.
* **Cakupan yang Diharapkan**:
  1. Modifikasi berkas form edit pegawai di `src/components/view/hr/master-data/employees/`.
* **Dependency**: `FE-HRD-021` ✅, `FE-HRD-022`
* **Acceptance Criteria**:
  1. Pengguna tidak dapat lagi mengganti unit kerja atau jabatan secara langsung dari form master data pegawai.
  2. Banner petunjuk muncul dengan jelas mengarahkan alur ke modul SK Kepegawaian.
  3. Field data profil lainnya (alamat, kontak, data pribadi non-kedinasan) tetap dapat disunting seperti biasa.
* **Bukti Verifikasi**:
  - Pemeriksaan visual form edit master data pegawai, linter check 0 error.
* **Risiko & Pemilik**: Pemilik: Frontend UI Lead. Risiko: mencegah regresi pada fungsi edit master data pegawai lainnya.
* **Definition of Done**: Field unit & jabatan terkunci read-only, banner edukasi terpasang, linter pass 0 error.
