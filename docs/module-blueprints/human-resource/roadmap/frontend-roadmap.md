# Roadmap Delivery Frontend — Modul Human Resource

## Metadata

```yaml
module_id: human-resource
blueprint_id: HRD-BP-001
blueprint_shape: SINGLE
roadmap_revision: 1
status: ACTIVE
status_synced_at: "2026-10-26 — inisialisasi canonical frontend delivery roadmap sinkron dengan Next.js App Router"
planning_updated_at: "2026-10-26 — penataan antarmuka pengguna berbasis design token dan base-features Quilvian"
generated_at: "2026-10-26"
owners:
  - "Frontend Technical Owner — HR UI Lead"
approved_by:
  - "User Approved (26 Oktober 2026)"
source_commits:
  backend: "16f6b2ad28e0e9cbd374354abe240ff3b163ace0"
  frontend: "2cd1c0610981b649f16a11bc74ac47ce6123e679"
contract_versions:
  - "API Contract v1 (Draft)"
```

**Arti tanda status pada dokumen ini:**

| Tanda | Artinya |
| :---: | --- |
| ✅ | Selesai. Tampilan, alur interaksi, Redux slice, dan validasi form terbukti pada source |
| 🟡 | Sebagian. Tampilan atau struktur rute sudah ada, namun integrasi API atau validasi belum penuh |
| ⛔ | Terblokir. Menunggu penyelesaian task backend prasyarat atau keputusan tata letak bisnis |
| tanpa tanda | Belum dikerjakan |

---

## Grafik Urutan Dependency

Roadmap frontend HR memuat **21 task** antarmuka pengguna:

```text
FE-HRD-021 ✅ ───> FE-HRD-001 ───┬─> FE-HRD-002
                                 │
BE-HRD-004 🟡 [BE] ──────────────┼─> FE-HRD-003 ───┬─> FE-HRD-008
                                 │                 │
                                 ├─> FE-HRD-004 ───┤
                                 │                 │
                                 ├─> FE-HRD-005 ───┤
                                 │                 │
                                 ├─> FE-HRD-006 ───┤
                                 │                 │
                                 └─> FE-HRD-007 ───┘
                                                   
BE-HRD-006 🟡 [BE] ──────────────┬─> FE-HRD-009 ───┬─> FE-HRD-013
                                 │                 │
BE-HRD-007 🟡 [BE] ──────────────┼─> FE-HRD-010 ───┤
                                 │                 │
BE-HRD-008 🟡 [BE] ──────────────┼─> FE-HRD-011 ───┘
                                 │
BE-HRD-009 🟡 [BE] ──────────────┴─> FE-HRD-012 ───> FE-HRD-015 ───> FE-HRD-016
                                 
BE-HRD-014 🟡 [BE] ──────────────┬─> FE-HRD-017 🟡 ─> FE-HRD-018
                                 │
BE-HRD-019 🟡 [BE] ──────────────┴─> FE-HRD-020 🟡

{DEC-HRD-008 ⛔} ─────────────────> FE-HRD-014 ⛔
{DEC-HRD-010 ⛔} ─────────────────> FE-HRD-019 ⛔
```

**Legenda Node:**
* `FE-HRD-###` = Task antarmuka pengguna (Frontend) Next.js App Router.
* `[BE]` = Cermin baca-saja task backend prasyarat dari `backend-roadmap.md`.
* `{DEC-... ⛔}` = Node pemblokir keputusan bisnis atau keselamatan klinis.

---

## Tabel Gelombang Eksekusi

| Gelombang | Boleh mulai setelah | Task Frontend |
| ---: | --- | --- |
| **1** | — (Komponen Master Data & Konstanta) | `FE-HRD-021` ✅, `FE-HRD-001` |
| **2** | `FE-HRD-001`, `BE-HRD-004` [BE] | `FE-HRD-002`, `FE-HRD-003`, `FE-HRD-004`, `FE-HRD-005`, `FE-HRD-006`, `FE-HRD-007` |
| **3** | Task gelombang 2, `BE-HRD-006..009` [BE] | `FE-HRD-008` (Inbox Persetujuan), `FE-HRD-009` (Presensi HR), `FE-HRD-010` (Cuti HR), `FE-HRD-011` (Lembur HR), `FE-HRD-012` (Roster Shift) |
| **4** | Task gelombang 3 terkait | `FE-HRD-013` (Payroll HR), `FE-HRD-015` (Pelatihan & Sertifikasi) |
| **5** | Task gelombang 4 terkait | `FE-HRD-016` (Kinerja Karyawan), `FE-HRD-018` (Kasus Hubungan Karyawan) |
| **Aktif** | Verifikasi & Pemeliharaan Layar Ada | `FE-HRD-017` 🟡 (Exit Clearance), `FE-HRD-020` 🟡 (Benefit Enrollment) |
| **⛔** | Menunggu `{DEC-HRD-008}` | `FE-HRD-014` ⛔ (Layar Kredensial Medis & SPK/RKK) |
| **⛔** | Menunggu `{DEC-HRD-010}` | `FE-HRD-019` ⛔ (Layar Kesehatan Staf K3RS) |

---

## Register Status Task Frontend

| Task ID | Slice | Ringkasan Layar & Komponen UI | Status | Dependency |
| --- | --- | --- | :---: | --- |
| `FE-HRD-001` | `S0-B` | Penyelarasan Rute Kebab-Case pada Endpoint Constants Master Data | 🟡 | `FE-HRD-021` |
| `FE-HRD-002` | `S-A1` | Layar Daftar Lintas-Pegawai Administrasi Kepegawaian (`/hr/workforce-core/*`) | 🟡 | `FE-HRD-001`, `BE-HRD-003` [BE] |
| `FE-HRD-003` | `S-A2` | Portal Layanan Mandiri Cuti Pegawai (`/self-services/.../leave`) | 🟡 | `FE-HRD-001`, `BE-HRD-004` [BE] |
| `FE-HRD-004` | `S-A3` | Portal Layanan Mandiri Lembur Pegawai (`/self-services/.../overtime`) | 🟡 | `FE-HRD-001`, `BE-HRD-004` [BE] |
| `FE-HRD-005` | `S-A4` | Portal Layanan Mandiri Ubah Jadwal & Tukar Shift (`/self-services/.../schedule`) | 🟡 | `FE-HRD-001`, `BE-HRD-004` [BE] |
| `FE-HRD-006` | `S-A5` | Portal Layanan Mandiri Koreksi Absensi & Pembaruan Profil Mandiri | 🟡 | `FE-HRD-001`, `BE-HRD-004` [BE] |
| `FE-HRD-007` | `S-A6` | Migrasi & Konsolidasi Form Absensi Harian ke Standar Self-Service Karyawan | 🟡 | `FE-HRD-001`, `BE-HRD-004` [BE] |
| `FE-HRD-008` | `S-A7` | Layar Kotak Masuk Persetujuan Terpadu Atasan (Unified Approval Inbox) | 🟡 | `FE-HRD-003..007`, `BE-HRD-005` [BE] |
| `FE-HRD-009` | `S-B1` | Layar Administrasi Monitoring Presensi & Rekapitulasi Kehadiran HR | 🟡 | `BE-HRD-006` [BE] |
| `FE-HRD-010` | `S-B2` | Layar Administrasi Alokasi & Penyesuaian Saldo Cuti Karyawan | 🟡 | `BE-HRD-007` [BE] |
| `FE-HRD-011` | `S-B3` | Layar Administrasi Verifikasi & Rekonsiliasi Jam Lembur HR | 🟡 | `BE-HRD-008` [BE] |
| `FE-HRD-012` | `S-B4` | Layar Matriks Penjadwalan Kerja & Roster Shift Instalasi 24/7 | 🟡 | `BE-HRD-009` [BE] |
| `FE-HRD-013` | `S-B5` | Layar Penggajian HR, Verifikasi Slip Gaji, dan Rekonsiliasi Batch | 🟡 | `FE-HRD-009..011`, `BE-HRD-010` [BE] |
| `FE-HRD-014` | `S-C1` | Layar Pengajuan Kredensial & Pengesahan Kewenangan Klinis Dokter | ⛔ | `{DEC-HRD-008 ⛔}` |
| `FE-HRD-015` | `S-C2` | Layar Manajemen Pelatihan Wajib, Kehadiran Diklat, dan Sertifikasi | 🟡 | `FE-HRD-012`, `BE-HRD-012` [BE] |
| `FE-HRD-016` | `S-C3` | Layar Siklus Evaluasi Kinerja, Pengisian Nilai KPI, dan Hasil Review | 🟡 | `FE-HRD-015`, `BE-HRD-013` [BE] |
| `FE-HRD-017` | `S-C4` | Layar Pemantauan Exit Clearance dan Kliring Aset Karyawan | 🟡 | `BE-HRD-014` [BE] |
| `FE-HRD-018` | `S-C5` | Layar Pencatatan Hubungan Karyawan, Kasus Disiplin, dan Surat Peringatan | 🟡 | `FE-HRD-017`, `BE-HRD-015` [BE] |
| `FE-HRD-019` | `S-C6` | Layar Rekam Medis Kerja & Pemantauan Kesehatan Karyawan K3RS | ⛔ | `{DEC-HRD-010 ⛔}` |
| `FE-HRD-020` | `S-D3` | Layar Pengelolaan Kepesertaan Benefit Karyawan & Penjamin Tanggungan | 🟡 | `BE-HRD-019` [BE] |
| `FE-HRD-021` | `IN-01` | Verifikasi dan Pemeliharaan 62 Kelompok Layar Master Data HR Terpasang | ✅ | — |

---

## Kartu Detail Task Frontend

### `FE-HRD-021` — Pemeliharaan dan Standarisasi 62 Layar Master Data HR (Selesai/Aktif)
* **Slice**: `IN-01`
* **Status**: ✅ Selesai (Terbukti aktif di codebase).
* **Bukti**: 62 folder halaman di `src/app/hr/master-data/*` lengkap dengan view di `src/components/view/hr/master-data/*`, Redux slices di `src/lib/state/slice/hr/master-data/*`, dan menu sidebar di `menu-items.jsx`.
* **Definition of Done**: Seluruh halaman master data dapat diakses, tabel paged result menampilkan data dari backend, filter pencarian berfungsi, form tambah/edit tervalidasi.

### `FE-HRD-017` — Layar Pemantauan Exit Clearance dan Kliring Aset Karyawan
* **Slice**: `S-C4`
* **Status**: 🟡 Sebagian (Rute, view, dan service aktif).
* **Bukti**: Halaman `src/app/hr/exit-clearance/page.jsx`, `exit-clearance-detail-view.jsx`, `exit-clearance-finance-status-card.jsx`, dan `exit-clearance-service.js`.
* **Pekerjaan Lanjutan**: Pengujian live browser pada tahapan approval lintas-departemen dan sinkronisasi status pengembalian inventaris.

### `FE-HRD-020` — Layar Pengelolaan Kepesertaan Benefit Karyawan & Tanggungan
* **Slice**: `S-D3`
* **Status**: 🟡 Sebagian (Rute, modal, tabel pergerakan saldo, dan service aktif).
* **Bukti**: Halaman `src/app/hr/benefit/enrollments/page.jsx`, `employee-benefit-enrollment-list-view.jsx`, `employee-benefit-enrollment-detail-view.jsx`, `employee-benefit-enrollment-create-modal.jsx`, dan `employee-benefit-enrollment-service.js`.
* **Pekerjaan Lanjutan**: Finalisasi validasi kuota plafon pada input klaim manual dan integrasi ke billing pasien jaminan perusahaan.

### `FE-HRD-002` — Layar Daftar Lintas-Pegawai Administrasi Kepegawaian
* **Slice**: `S-A1`
* **Rute**: `/hr/workforce-core/salary-assignments`, `/hr/workforce-core/organization-assignments`, `/hr/workforce-core/position-assignments`, `/hr/workforce-core/manager-assignments`, `/hr/workforce-core/employment-histories`, `/hr/workforce-core/employee-profile-changes`.
* **Target**: Menggantikan tautan menu sidebar yang saat ini belum memiliki berkas `page.jsx` agar menampilkan tabel rekapitulasi seluruh staf rumah sakit dengan pagination standar.

### `FE-HRD-008` — Layar Kotak Masuk Persetujuan Terpadu Atasan (Unified Approval Inbox)
* **Slice**: `S-A7`
* **Target**: Satu antarmuka terpusat bagi kepala ruangan / manajer medis / direksi untuk meninjau dan memberikan persetujuan (Setujui / Tolak / Minta Revisi) terhadap berbagai jenis pengajuan staf secara cepat.
