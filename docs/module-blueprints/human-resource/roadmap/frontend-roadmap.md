# Roadmap Delivery Frontend — Modul Human Resource

## Metadata

```yaml
module_id: human-resource
blueprint_id: HRD-BP-001
blueprint_shape: SINGLE
roadmap_revision: 2
status: ACTIVE
status_synced_at: "2026-10-28 — penuntasan implementasi antarmuka pengguna frontend HR sinkron Next.js App Router"
planning_updated_at: "2026-10-28 — seluruh task frontend non-blocked terimplementasi dan tervalidasi"
generated_at: "2026-10-26"
owners:
  - "Frontend Technical Owner — HR UI Lead"
approved_by:
  - "User Approved (26 Oktober 2026)"
source_commits:
  backend: "16f6b2ad28e0e9cbd374354abe240ff3b163ace0"
  frontend: "HamzahV2 (lint pass, 0 error)"
contract_versions:
  - "API Contract v1"
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

Roadmap frontend HR memuat **21 task** antarmuka pengguna:

```text
FE-HRD-021 ✅ ───> FE-HRD-001 ✅ ───┬─> FE-HRD-002 ✅
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
| `FE-HRD-010` | `S-B2` | Layar Administrasi Alokasi & Penyesuaian Saldo Cuti Karyawan | ✅ | [FE-HRD-010.md](../task/report/frontend/FE-HRD-010.md) |
| `FE-HRD-011` | `S-B3` | Layar Administrasi Verifikasi & Rekonsiliasi Jam Lembur HR | ✅ | [FE-HRD-011.md](../task/report/frontend/FE-HRD-011.md) |
| `FE-HRD-012` | `S-B4` | Layar Matriks Penjadwalan Kerja & Roster Shift Instalasi 24/7 | ✅ | [FE-HRD-012.md](../task/report/frontend/FE-HRD-012.md) |
| `FE-HRD-013` | `S-B5` | Layar Penggajian Restitusi & Potongan Cicilan Piutang RS | ✅ | [FE-HRD-013.md](../task/report/frontend/FE-HRD-013.md) |
| `FE-HRD-014` | `S-C1` | Layar Pengajuan Kredensial & Pengesahan Kewenangan Klinis Dokter | ⛔ | Terblokir `{DEC-HRD-008}` |
| `FE-HRD-015` | `S-C2` | Layar Manajemen Pelatihan Wajib, Kehadiran Diklat, dan Sertifikasi | ✅ | [FE-HRD-015.md](../task/report/frontend/FE-HRD-015.md) |
| `FE-HRD-016` | `S-C3` | Layar Siklus Evaluasi Kinerja, Pengisian Nilai KPI, dan Hasil Review | ✅ | [FE-HRD-016.md](../task/report/frontend/FE-HRD-016.md) |
| `FE-HRD-017` | `S-C4` | Layar Pemantauan Exit Clearance dan Kliring Aset Karyawan | ✅ | [FE-HRD-017.md](../task/report/frontend/FE-HRD-017.md) |
| `FE-HRD-018` | `S-C5` | Layar Pencatatan Hubungan Karyawan, Kasus Disiplin, dan Surat Peringatan | ✅ | [FE-HRD-018.md](../task/report/frontend/FE-HRD-018.md) |
| `FE-HRD-019` | `S-C6` | Layar Rekam Medis Kerja & Pemantauan Kesehatan Karyawan K3RS | ⛔ | Terblokir `{DEC-HRD-010}` |
| `FE-HRD-020` | `S-D3` | Layar Pengelolaan Kepesertaan Benefit Karyawan & Penjamin Tanggungan | ✅ | [FE-HRD-020.md](../task/report/frontend/FE-HRD-020.md) |
| `FE-HRD-021` | `IN-01` | Verifikasi dan Pemeliharaan 62 Kelompok Layar Master Data HR Terpasang | ✅ | Terpasang Aktif |

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
