# Roadmap Delivery Backend — Modul Human Resource

## Metadata

```yaml
module_id: human-resource
blueprint_id: HRD-BP-001
blueprint_shape: SINGLE
roadmap_revision: 2
status: ACTIVE
status_synced_at: "2026-11-02 — penambahan task implementasi target state Slice S-A1 (BE-HRD-023 s.d. BE-HRD-028) sinkron dengan HEAD backend 16f6b2a dan frontend 2cd1c06"
planning_updated_at: "2026-11-02 — perencanaan vertical slice Dokumen SK Kepegawaian, Employee 360, Initial Placement Batch, dan On-Behalf Profile Change"
generated_at: "2026-10-26"
owners:
  - "Technical Owner — HR Module Lead"
approved_by:
  - "User Approved (26 Oktober 2026)"
source_commits:
  backend: "16f6b2ad28e0e9cbd374354abe240ff3b163ace0"
  frontend: "2cd1c0610981b649f16a11bc74ac47ce6123e679"
contract_versions:
  - "API Contract v6"
  - "State Transition Matrix v6"
  - "Validation Matrix v6"
  - "Permission Audit Matrix v1"
  - "Integration Contract v1"
```

**Arti tanda status pada dokumen ini:**

| Tanda | Artinya |
| :---: | --- |
| ✅ | Selesai. Acceptance criteria dan DoD **terbukti**, buktinya ada pada laporan task |
| 🟡 | Sebagian. Source-nya sudah ada, tetapi acceptance criteria belum terbukti penuh. **Belum selesai** |
| ⛔ | Terblokir. Prasyaratnya belum terpenuhi (keputusan bisnis, keselamatan klinis, atau gerbang otoritas) |
| tanpa tanda | Belum dikerjakan |

---

## Grafik Urutan Dependency

Roadmap backend HR memuat **28 task** yang diorganisasi secara bertahap:

```text
BE-HRD-001 ✅ ──┬─> BE-HRD-002 ✅ ──┬─> BE-HRD-003 ✅ ──┬─> BE-HRD-004 ✅ ───> BE-HRD-005 ✅
              │                 │                 │
              │                 │                 └─> BE-HRD-023 ✅ ──> BE-HRD-024 ✅ ─┬─> BE-HRD-025 ✅
              │                 │                                                  ├─> BE-HRD-026 ✅
              │                 │                                                  ├─> BE-HRD-027 ✅
              │                 │                                                  └─> BE-HRD-028 ✅
              │                 │
              │                 ├─> BE-HRD-006 ✅ ──┬─> BE-HRD-007 ✅ ───> BE-HRD-010 ✅
              │                 │                 │
              │                 │                 └─> BE-HRD-008 ✅ ───┘
              │                 │
              │                 ├─> BE-HRD-009 ✅ ──> BE-HRD-012 ✅ ───> BE-HRD-013 ✅
              │                 │
              │                 ├─> BE-HRD-014 ✅ ──> BE-HRD-015 ✅
              │                 │
              │                 ├─> BE-HRD-019 ✅
              │                 │
              │                 └─> BE-HRD-022 ✅
              │
              ├─> {DEC-HRD-008 ⛔} ─> BE-HRD-011 ⛔
              │
              ├─> {DEC-HRD-010 ⛔} ─> BE-HRD-016 ⛔
              │
              └─> {DEC-HRD-004 ⛔} ─┬─> BE-HRD-017 ⛔
                                    │
                                    ├─> BE-HRD-018 ⛔
                                    │
                                    ├─> BE-HRD-020 ⛔
                                    │
                                    └─> BE-HRD-021
```

**Legenda Node:**
* `BE-HRD-###` = Task implementasi / verifikasi backend HRD.
* `{DEC-HRD-008 ⛔}` = Gerbang keselamatan klinis & persetujuan Komite Medik atas SPK/RKK Dokter.
* `{DEC-HRD-010 ⛔}` = Gerbang privasi rekam kesehatan staf & pengesahan K3RS.
* `{DEC-HRD-004 ⛔}` = Keputusan audit data eksisting atas 67 model tanpa API sebelum restrukturisasi skema.

---

## Tabel Gelombang Eksekusi

| Gelombang | Boleh mulai setelah | Task Backend |
| ---: | --- | --- |
| **1** | — (Fondasi tanpa prasyarat) | `BE-HRD-001` ✅ |
| **2** | `BE-HRD-001` | `BE-HRD-002` ✅ |
| **3** | `BE-HRD-002` | `BE-HRD-003` ✅, `BE-HRD-006` ✅, `BE-HRD-009` ✅, `BE-HRD-014` ✅, `BE-HRD-019` ✅, `BE-HRD-022` ✅ |
| **4** | Task gelombang 3 terkait | `BE-HRD-004` ✅, `BE-HRD-007` ✅, `BE-HRD-008` ✅, `BE-HRD-012` ✅, `BE-HRD-015` ✅, `BE-HRD-023` ✅ |
| **5** | Task gelombang 4 terkait | `BE-HRD-005` ✅, `BE-HRD-010` ✅, `BE-HRD-013` ✅, `BE-HRD-024` ✅ |
| **6** | `BE-HRD-024` | `BE-HRD-025` ✅, `BE-HRD-026` ✅, `BE-HRD-027` ✅, `BE-HRD-028` ✅ |
| **⛔** | Menunggu `{DEC-HRD-008}` | `BE-HRD-011` ⛔ (Kredensial & SPK/RKK Klinis) |
| **⛔** | Menunggu `{DEC-HRD-010}` | `BE-HRD-016` ⛔ (Kesehatan Kerja Staf & MCU K3RS) |
| **⛔** | Menunggu `{DEC-HRD-004}` | `BE-HRD-017` ⛔ (Perencanaan Tenaga Kerja), `BE-HRD-018` ⛔ (Rekrutmen & Hiring), `BE-HRD-020` ⛔ (Tiket Layanan HR) |
| **DEFERRED** | Prioritas bisnis terjadwal | `BE-HRD-021` (Perjalanan Dinas & Reimbursement) |

---

## Register Status Task Backend

| Task ID | Slice | Ringkasan Tugas | Status | Dependency |
| --- | --- | --- | :---: | --- |
| `BE-HRD-001` | `S0-A` | Pendaftaran prefix operasional `Hrd` dan keluarga workforce `Wfp` pada registry | ✅ | — |
| `BE-HRD-002` | `S0-B` | Route canonical kebab-case dan compatibility alias pada 8 master data controller | ✅ | `BE-HRD-001` |
| `BE-HRD-003` | `S-A1` | Endpoint daftar lintas-pegawai Administrasi Kepegawaian (Workforce Core) | ✅ | `BE-HRD-002` |
| `BE-HRD-004` | `S-A2..S-A6` | Verifikasi dan pemantapan endpoint Layanan Mandiri Karyawan (ESS) | ✅ | `BE-HRD-003` |
| `BE-HRD-005` | `S-A7` | Engine SLA, eskalasi, dan query agregasi Kotak Masuk Persetujuan Terpadu | ✅ | `BE-HRD-004` |
| `BE-HRD-006` | `S-B1` | Administrasi Presensi Harian, Penutupan Periode, dan Jalur On-Behalf HR | ✅ | `BE-HRD-002` |
| `BE-HRD-007` | `S-B2` | Administrasi Cuti, Penyesuaian Saldo Bertahap, dan Rekonsiliasi Tahunan | ✅ | `BE-HRD-006` |
| `BE-HRD-008` | `S-B3` | Administrasi Lembur, Verifikasi Surat Perintah Kerja Lembur, dan Rekonsiliasi | ✅ | `BE-HRD-006` |
| `BE-HRD-009` | `S-B4` | Engine Roster Shift 24/7, Penggantian Shift Darurat, dan Penugasan Siaga | ✅ | `BE-HRD-002` |
| `BE-HRD-010` | `S-B5` | Kalkulasi Payroll HR, Pemotongan Angsuran, dan Restitusi Penggajian | ✅ | `BE-HRD-007`, `BE-HRD-008` |
| `BE-HRD-011` | `S-C1` | Kredensial Medis, SPK/RKK Dokter, dan Komite Medik | ⛔ | `{DEC-HRD-008 ⛔}` |
| `BE-HRD-012` | `S-C2` | Penugasan Pelatihan Wajib, Pelacakan Sertifikasi, dan Masa Berlaku | ✅ | `BE-HRD-009` |
| `BE-HRD-013` | `S-C3` | Siklus Evaluasi Kinerja Karyawan, Skala Penilaian KPI, dan Finalisasi | ✅ | `BE-HRD-012` |
| `BE-HRD-014` | `S-C4` | Transaksi Exit Clearance, Pengembalian Aset, dan Offboarding Karyawan | ✅ | `BE-HRD-002` |
| `BE-HRD-015` | `S-C5` | Pencatatan Kasus Hubungan Karyawan, Pembinaan Disiplin, dan Sanksi | ✅ | `BE-HRD-014` |
| `BE-HRD-016` | `S-C6` | Kesehatan dan Keselamatan Kerja Staf (K3RS) | ⛔ | `{DEC-HRD-010 ⛔}` |
| `BE-HRD-017` | `S-D1` | Perencanaan Kebutuhan Tenaga Kerja (Workforce Planning) | ⛔ | `{DEC-HRD-004 ⛔}` |
| `BE-HRD-018` | `S-D2` | Rekrutmen, Seleksi Pelamar, dan Penawaran Kerja (Hiring) | ⛔ | `{DEC-HRD-004 ⛔}` |
| `BE-HRD-019` | `S-D3` | Pendaftaran Benefit Karyawan, Penjaminan Tanggungan, dan Mutasi Saldo | ✅ | `BE-HRD-002` |
| `BE-HRD-020` | `S-D4` | Layanan Bantuan dan Tiket Kepegawaian (HR Helpdesk) | ⛔ | `{DEC-HRD-004 ⛔}` |
| `BE-HRD-021` | `S-D5` | Pengajuan Perjalanan Dinas dan Klaim Biaya (Reimbursement) | tanpa tanda | `{DEC-HRD-004 ⛔}` |
| `BE-HRD-022` | `S-E` | Audit Ratchet Penamaan Entitas Transaksi Legacy (`Trx` ke `Hrd`) | ✅ | `BE-HRD-002` |
| `BE-HRD-023` | `S-A1` | Model, Konfigurasi EF Core, dan Migration Skema `HrdPersonnelActionOrder` & Approval Fields | ✅ | `BE-HRD-003` |
| `BE-HRD-024` | `S-A1` | Service Mutasi & Persetujuan SK Kepegawaian (`HrdPersonnelActionOrderService`) & Validasi | ✅ | `BE-HRD-023` |
| `BE-HRD-025` | `S-A1` | Controller API Manajemen Dokumen SK Kepegawaian & Unggah Pindai Berkas Fisik | ✅ | `BE-HRD-024` |
| `BE-HRD-026` | `S-A1` | Controller & Service Profil Terpadu Pegawai (Employee 360) dengan Dual-Mode Masking Keamanan | ✅ | `BE-HRD-024` |
| `BE-HRD-027` | `S-A1` | Service & Controller Inisialisasi Penempatan Baseline Pegawai Eksisting (55 Staf) | ✅ | `BE-HRD-024` |
| `BE-HRD-028` | `S-A1` | Endpoint Kuasa HR (On-Behalf) Perubahan Profil Pegawai & Penguncian Form Master Data | | `BE-HRD-024`

---

## Kartu Detail Task Backend

### `BE-HRD-001` — Pendaftaran Prefix Operasional `Hrd` dan Workforce `Wfp` pada Registry
* **Slice**: `S0-A`
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-001.md))
* **Outcome**: Dokumen `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` secara resmi memuat entri prefix `Hrd` (operasional canonical) dan `Wfp` (workforce profile HR) tanpa menandai `Wfp` sebagai legacy terhapus.
* **Acceptance Criteria**:
  1. Registry memuat baris `Corporate / SelfServices | Human Resource | BUSINESS DOMAIN | Hrd | ACTIVE`.
  2. Registry memuat baris `Corporate / SelfServices | Human Resource | BUSINESS DOMAIN | Wfp | ACTIVE`.
  3. Tidak ada entity `Trx*` yang diklaim secara borongan milik HR; pemilikan didasarkan pada domain dan bukti.
* **Definition of Done**: Berkas registry diperbarui, `Invoke-QbeConformanceCheck.ps1` lulus validasi modul.

### `BE-HRD-002` — Route Canonical Kebab-Case dan Compatibility Alias
* **Slice**: `S0-B`
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-002.md))
* **Outcome**: Delapan master data controller yang sebelumnya memakai non-hyphenated route memiliki alias kebab-case resmi tanpa merusak pemanggil lama.
* **Acceptance Criteria**:
  1. Route `work-calendars`, `work-schedules`, `shift-groups`, `shift-patterns`, `action-types`, `case-types`, `sanction-types`, `violation-types` aktif.
  2. Route lama tetap dilayani oleh action yang sama dengan header / swagger attribute deprecated.
* **Definition of Done**: `dotnet build` sukses, Swagger JSON memproyeksikan route canonical dan alias.

### `BE-HRD-003` — Endpoint Daftar Lintas-Pegawai Administrasi Kepegawaian
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-003.md))
* **Slice**: `S-A1`
* **Outcome**: HR Admin dapat menarik daftar rekapitulasi penempatan organisasi, penempatan jabatan, relasi atasan, riwayat kepegawaian, dan penetapan gaji lintas-seluruh pegawai pada satu periode kerja.
* **Acceptance Criteria**:
  1. Menyediakan endpoint paged result `GET /api/v1/corporate/human-resource/workforce-core/salary-assignments`.
  2. Menyediakan endpoint paged result untuk penempatan organisasi dan jabatan lintas pegawai.
  3. Mendukung filter berdasarkan unit kerja, status aktif, dan rentang tanggal berlaku.
* **Definition of Done**: Controller & service mengembalikan `ApiResponse<PagedResult<T>>`, authorization `[AccessAction]` terpasang.

### `BE-HRD-004` — Verifikasi dan Pemantapan Endpoint Layanan Mandiri Karyawan (ESS)
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-004.md))
* **Slice**: `S-A2..S-A6`
* **Outcome**: Endpoint pengajuan cuti, lembur, permohonan tukar shift, koreksi absensi, dan update profil mandiri diverifikasi bekerja sesuai identitas pengguna yang login (`actorWorkforceProfileId`).
* **Acceptance Criteria**:
  1. Endpoint memvalidasi bahwa permohonan hanya dibuat untuk profil pegawai yang sedang terautentikasi.
  2. Validasi kuota dan saldo cuti menolak pengajuan yang melebihi hak.
  3. Pengajuan lembur mendeteksi tumpang tindih waktu.
* **Definition of Done**: Seluruh controller ESS teruji menangani kasus normal dan boundary (saldo habis / jadwal bentrok).

### `BE-HRD-005` — Engine SLA, Eskalasi, dan Kotak Masuk Persetujuan Terpadu
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-005.md))
* **Slice**: `S-A7`
* **Outcome**: Atasan dapat menarik seluruh transaksi bawahan yang membutuhkan persetujuannya dalam satu endpoint terpadu, dan background job eskalasi mengecek batas SLA.
* **Acceptance Criteria**:
  1. `ApprovalInboxController` mengembalikan agregasi permohonan menunggu persetujuan per atasan.
  2. SLA engine mengecek `DueAt` dan memicu notifikasi peringatan.
  3. Persetujuan dan penolakan memperbarui instance workflow dan status transaksi pemilik secara atomik.
* **Definition of Done**: Service terintegrasi dengan `WorkflowManagement`, tidak ada data status yang orphan.

### `BE-HRD-006` — Administrasi Presensi Harian, Penutupan Periode, dan Jalur On-Behalf HR
* **Slice**: `S-B1`
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-006.md))
* **Outcome**: HR Admin dapat memantau presensi harian seluruh staf RS, mengunci periode presensi, dan memproses permohonan koreksi kehadiran atas nama staf (on-behalf) dengan audit trail lengkap.
* **Acceptance Criteria**:
  1. Periode kehadiran yang sudah ditutup (`Closed`) menolak pencatatan presensi retroactive tanpa pembukaan resmi.
  2. Jalur koreksi on-behalf mencatat ID HR initiator, alasan kedaruratan, dan bukti verifikasi.
* **Definition of Done**: 71 endpoint attendance diverifikasi, proteksi periode terkunci terbukti aktif.

### `BE-HRD-007` — Administrasi Cuti, Penyesuaian Saldo Bertahap, dan Rekonsiliasi
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-007.md))
* **Slice**: `S-B2`
* **Outcome**: Pengelolaan saldo cuti tahunan, cuti besar, dan izin istirahat pegawai terpelihara dengan mutasi saldo yang akuntabel.
* **Acceptance Criteria**:
  1. Penyesuaian saldo manual oleh HR wajib menyertakan alasan penyesuaian dan referensi dokumen.
  2. Pembatalan cuti yang telah disetujui memulihkan saldo secara idempoten.
* **Definition of Done**: Mutasi saldo cuti tercatat di tabel log penyesuaian, audit trail tersimpan.

### `BE-HRD-008` — Administrasi Lembur dan Rekonsiliasi Penggajian
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-008.md))
* **Slice**: `S-B3`
* **Outcome**: HR memverifikasi realisasi jam lembur terhadap surat perintah kerja lembur (SPKL) sebelum diteruskan ke perhitungan payroll.
* **Acceptance Criteria**:
  1. Realisasi lembur yang belum disetujui supervisor ditandai tidak layak diperhitungkan dalam payroll run.
  2. Rekonsiliasi jam lembur menghitung faktor pengali hari kerja dan hari libur secara tepat.
* **Definition of Done**: Rekonsiliasi lembur menghasilkan status `VerifiedForPayroll`.

### `BE-HRD-009` — Penjadwalan Roster Kerja 24/7 dan Penugasan Siaga (On-Call)
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-009.md))
* **Slice**: `S-B4`
* **Outcome**: Penugasan shift harian dan jadwal jaga tenaga kesehatan per instalasi (Rawat Inap, IGD, ICU, Bedah) terkelola tanpa tabrakan jadwal.
* **Acceptance Criteria**:
  1. Validasi mendeteksi jadwal ganda pegawai pada shift yang sama atau shift berturut-turut tanpa jeda istirahat wajib.
  2. Penggantian shift mendadak (darurat) mencatat pengganti dan alasan operasional.
* **Definition of Done**: Controller roster penugasan shift terhubung ke model roster period dan unit kerja RS.

### `BE-HRD-010` — Kalkulasi Payroll HR, Pemotongan Angsuran, dan Restitusi
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-010.md))
* **Slice**: `S-B5`
* **Outcome**: Proses hitung gaji kotor, potongan ketidakhadiran, potongan pinjaman/angsuran, tunjangan shift, restitusi klaim, hingga penghasilan bersih per periode penggajian.
* **Acceptance Criteria**:
  1. Perhitungan memasukkan data sah dari presensi dan lembur yang sudah berstatus final.
  2. `PayrollRestitutionController` dan `PayrollRunInstallmentDeductionController` terintegrasi pada putaran payroll.
  3. Serah terima batch payroll ke modul Finance bersifat idempoten.
* **Definition of Done**: Perhitungan payroll dapat dieksekusi, di-rollback bila ada anomali, dan dikunci sebelum serah terima.

### `BE-HRD-012` — Penugasan Pelatihan Wajib, Pelacakan Sertifikasi, dan Masa Berlaku
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-012.md))
* **Slice**: `S-C2`
* **Outcome**: Penugasan pelatihan keselamatan pasien (BHD, PPI, K3RS), pencatatan sertifikat, masa berlaku sertifikat, dan asesmen kompetensi staf rumah sakit terpantau.
* **Acceptance Criteria**:
  1. Pelatihan wajib yang belum dipenuhi terlihat per pegawai dan per unit.
  2. Sertifikat tersimpan sebagai bukti yang dapat ditelusuri.
  3. Masa berlaku sertifikat memicu peringatan sebelum kedaluwarsa.
* **Definition of Done**: Controller & model training record dan competency assessment terhubung ke profil kepegawaian.

### `BE-HRD-013` — Siklus Evaluasi Kinerja Karyawan, Skala Penilaian KPI, dan Finalisasi
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-013.md))
* **Slice**: `S-C3`
* **Outcome**: Siklus evaluasi berkala KPI staf berjalan transparan, 1 review aktif per siklus, dan privasi hasil terjaga.
* **Acceptance Criteria**:
  1. Satu pegawai tidak dapat dinilai dua kali pada siklus yang sama.
  2. Penilaian yang sudah final tidak dapat diubah diam-diam; koreksi menghasilkan revisi.
  3. Pegawai melihat hasilnya sendiri, bukan hasil orang lain.
* **Definition of Done**: Controller review kinerja dan rincian KPI terhubung ke model siklus dan profil staf.

### `BE-HRD-014` — Transaksi Exit Clearance, Pengembalian Aset, dan Offboarding
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-014.md))
* **Slice**: `S-C4`
* **Outcome**: Pengelolaan proses berhenti kerja karyawan (resign/pensiun/habis kontrak), mencakup checklist penyerahan aset unit, perpustakaan, kartu akses, dan status kliring keuangan.
* **Acceptance Criteria**:
  1. `ExitClearanceController` memvalidasi status kelulusan setiap unit verifikator (IT, GA, Farmasi, Keuangan).
  2. Penutupan exit clearance menandai profil pegawai sebagai non-aktif tanpa menghapus data riwayat.
* **Definition of Done**: Controller & service exit clearance terintegrasi, laporan verifikasi tercatat.

### `BE-HRD-015` — Pencatatan Kasus Hubungan Karyawan, Pembinaan Disiplin, dan Sanksi
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-015.md))
* **Slice**: `S-C5`
* **Outcome**: Penanganan kasus pelanggaran disiplin kerja staf medis dan non-medis, penerbitan surat peringatan (SP 1, SP 2, SP 3), dan riwayat pembinaan kepegawaian tercatat aman.
* **Acceptance Criteria**:
  1. Kasus kedisiplinan hanya dapat dibaca peran yang berwenang.
  2. Setiap tindakan menyimpan alasan, pelaku, dan waktu kejadian.
  3. Kasus yang sudah ditutup tidak dapat diubah diam-diam.
* **Definition of Done**: Controller & model tindakan disiplin terhubung ke master jenis pelanggaran dan sanksi.

### `BE-HRD-019` — Pendaftaran Benefit Karyawan, Tanggungan, dan Mutasi Saldo
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-019.md))
* **Slice**: `S-D3`
* **Outcome**: Pengelolaan kepesertaan asuransi kesehatan staf/tanggungan, alokasi plafon rawat jalan/inap, dan pencatatan mutasi saldo benefit pegawai rumah sakit.
* **Acceptance Criteria**:
  1. `EmployeeBenefitEnrollmentController` dan `EmployeeBenefitDependentController` mengelola eligibilitas dan plafon.
  2. Setiap mutasi saldo benefit (klaim/top-up tahunan) tercatat pada `HrdBenefitBalanceMovement`.
  3. Validasi mencegah pendaftaran tanggungan melebihi batas ketentuan paket benefit.
* **Definition of Done**: Service benefit enrollment & balance management terverifikasi, relasi ke penjamin perusahaan pasien terhubung.

### `BE-HRD-022` — Audit Ratchet Penamaan Entitas Transaksi Legacy (`Trx` ke `Hrd`)
* **Status**: ✅ Selesai (27 Oktober 2026, [Laporan Tracked](../task/report/backend/BE-HRD-022.md))
* **Slice**: `S-E`
* **Outcome**: Penegakan aturan konstitusi rekayasa bahwa penggantian prefix `Trx` ke `Hrd` hanya terjadi saat entity mengalami mutasi skema material, mencegah rename massal destruktif.
* **Acceptance Criteria**:
  1. Ratchet penamaan hanya dipicu saat entity benar-benar disentuh skemanya (`AT-HRD-E-01`).
  2. Ratchet tidak dipicu oleh pekerjaan query atau dokumentasi (`AT-HRD-E-02`).
* **Definition of Done**: Kepatuhan audit ratchet diverifikasi penuh pada seluruh entitas HR.


### `BE-HRD-023` — Model, Konfigurasi EF Core, dan Migration Skema `HrdPersonnelActionOrder` & Approval Fields
* **Slice**: `S-A1` (Workforce Core Target State)
* **Status**: Selesai (✅) — Lihat [Laporan Tracked](../task/report/backend/BE-HRD-023.md)
* **Outcome**: Skema database modul HR diperkaya dengan entitas agregat `HrdPersonnelActionOrder`, entitas lampiran berkas `HrdPersonnelActionOrderAttachment`, entitas batch `HrdInitialPlacementBatch` & `HrdInitialPlacementBatchRow`, serta perluasan kolom `PersonnelActionOrderId` (FK nullable) dan audit approval fields pada `WfpOrganizationAssignment`, `WfpPositionAssignment`, `WfpManagerAssignment`, dan `WfpSalaryAssignment` via Migration 7 (`AddPersonnelActionOrderAndAssignmentApprovalFields`).
* **Traceability**: `HRD-DEC-048`, `HRD-DEC-056`, `HRD-DEC-057`, `FR-HRD-013`, `02-backend-architecture.md` §5.6 & §8.2 Migration 7, `data/data-dictionary.md` §2.8, §2.10.
* **Contract Version**: Data Dictionary v6, Backend Architecture rev 2.
* **Kemampuan Existing yang Dipakai**: Entitas `WfpOrganizationAssignment`, `WfpPositionAssignment`, `WfpManagerAssignment`, `WfpSalaryAssignment`, `MstWorkforceProfile`.
* **Cakupan yang Diharapkan**:
  1. Definisi model entity C# di `Areas/Corporate/HumanResource/Models/` atau direktori domain HR canonical.
  2. Fluent API configuration pada DbContext mencakup konfigurasi relasi, constraint `IsUnique` untuk `OrderNumber`, dan cascade delete restrict.
  3. Migration script EF Core `AddPersonnelActionOrderAndAssignmentApprovalFields`.
* **Dependency**: `BE-HRD-003` ✅
* **Acceptance Criteria**:
  1. Entitas `HrdPersonnelActionOrder` memuat seluruh atribut wajib (OrderNumber, ActionType, WorkforceProfileId, EffectiveDate, PostingDate, Status, Reason, ReferenceDocumentNumber, Notes) sesuai kamus data.
  2. Entitas penempatan (`Wfp*Assignment`) memiliki kolom `PersonnelActionOrderId` (FK nullable) yang mereferensikan `HrdPersonnelActionOrder`.
  3. Index unik pada `OrderNumber` terdefinisi dengan benar pada EF Core metadata.
  4. Perintah migrasi database dapat dieksekusi tanpa error (`dotnet ef database update --no-build`).
* **Bukti Verifikasi**:
  - Review diff skema EF Core model snapshot.
  - Kompilasi project backend (`dotnet build`) sukses 0 error.
  - Conformance QBE (`Invoke-QbeConformanceCheck.ps1`) lulus untuk entitas ber-prefix `Hrd`.
* **Risiko & Pemilik**: Pemilik: Backend Technical Lead. Risiko: lock schema pada tabel penempatan aktif diatasi dengan nullable foreign key.
* **Definition of Done**: Migration terdaftar di snapshot EF Core, model terkompilasi 0 error, audit skema QBE lulus.

### `BE-HRD-024` — Service Mutasi & Persetujuan SK Kepegawaian (`HrdPersonnelActionOrderService`) & Validasi
* **Slice**: `S-A1` (Workforce Core Target State)
* **Status**: Selesai (✅) — Lihat [Laporan Tracked](../task/report/backend/BE-HRD-024.md)
* **Outcome**: Layanan bisnis inti untuk siklus hidup Surat Keputusan Kepegawaian (`Draft`, `PendingApproval`, `Scheduled`, `Applied`, `Rejected`, `Cancelled`) terimplementasi lengkap dengan penegakan seluruh aturan bisnis dan validasi integritas data rumah sakit.
* **Traceability**: `HRD-DEC-031`, `HRD-DEC-048`, `HRD-DEC-049`, `HRD-DEC-050`, `HRD-DEC-056`, `HRD-DEC-057`, `FR-HRD-013`, `FR-HRD-014`, `FR-HRD-015`, `FR-HRD-020`, `VR-WFC-01` s.d. `VR-WFC-05`, `VR-WFC-11` s.d. `VR-WFC-13`.
* **Contract Version**: State Transition Matrix v6, Validation Matrix v6.
* **Kemampuan Existing yang Dipakai**: `IWorkforceProfileService`, `IAttendanceAdministrationService` (pemeriksaan status periode presensi tertutup).
* **Cakupan yang Diharapkan**:
  1. Layanan interface `IHrdPersonnelActionOrderService` dan implementasinya di Application Layer backend.
  2. Mesin transisi status SK yang mengikat dan memvalidasi setiap fase perubahan.
  3. Integrasi pemeriksaan status periode kehadiran tertutup sebelum menyetujui mutasi berlaku surut (retroaktif).
  4. Pembuatan otomatis entitas penempatan aktif (`Wfp*Assignment`) saat status SK mencapai `Applied`.
* **Dependency**: `BE-HRD-023`
* **Acceptance Criteria**:
  1. Memvalidasi bahwa setiap pegawai hanya memiliki tepat 1 penempatan unit dan jabatan utama aktif pada satu tanggal berlaku (`VR-WFC-01`, `VR-WFC-02`).
  2. Menolak SK mutasi dengan tanggal efektif berlaku surut jika tanggal tersebut berada pada periode presensi yang sudah ditutup/closed (`VR-WFC-03`, `HRD-DEC-050`).
  3. Menolak pengajuan persetujuan SK jika belum terdapat berkas pindai fisik bertanda tangan (`VR-WFC-04`, `HRD-DEC-056`).
  4. Menolak persetujuan jika aktor penyetuju adalah pembuat draft SK (larangan self-approval, `VR-WFC-05`, `HRD-DEC-031`).
  5. Saat SK disetujui dan tanggal efektif tiba (`Applied`), sistem secara atomik memperbarui assignment aktif dan menonaktifkan assignment lama.
  6. Pembatalan SK yang sudah berstatus `Applied` hanya dapat dilakukan melalui penerbitan SK Pembatalan / Kontra resmi (`VR-WFC-12`, `HRD-DEC-057`).
* **Bukti Verifikasi**:
  - Pemeriksaan kelengkapan guard clauses pada service method.
  - Verifikasi log audit transaksi perpindahan penempatan.
  - Kompilasi project backend (`dotnet build`) sukses 0 error.
* **Risiko & Pemilik**: Pemilik: Backend HR Domain Engineer. Risiko: kegagalan konsistensi atomik saat aktivasi multi-assignment diatasi dengan DbContext transaction scope.
* **Definition of Done**: Service terdaftar di ServiceCollection DI, mematuhi matrix transisi status v6 dan matriks validasi v6.

### `BE-HRD-025` — Controller API Manajemen Dokumen SK Kepegawaian & Unggah Pindai Berkas Fisik
* **Slice**: `S-A1` (Workforce Core Target State)
* **Status**: Selesai (✅) — Lihat [Laporan Tracked](../task/report/backend/BE-HRD-025.md)
* **Outcome**: Controller RESTful resmi ber-tag Swagger `[Tags("Corporate / Human Resource / Workforce Core / Personnel Action Order")]` aktif melayani query paged list SK, detail SK, aksi siklus hidup (create draft, submit, approve, reject, cancel), serta endpoint unggah dan unduh berkas pindai fisik SK.
* **Traceability**: `HRD-DEC-048`, `HRD-DEC-056`, `HRD-DEC-057`, `FR-HRD-013`, `contracts/api-contract.md` §PersonnelActionOrder.
* **Contract Version**: API Contract v6.
* **Kemampuan Existing yang Dipakai**: `BaseApiController`, `IAccessActionAuthorizer`, modul manajemen berkas upload internal.
* **Cakupan yang Diharapkan**:
  1. Controller `HrdPersonnelActionOrderController` pada route `api/v1/corporate/human-resource/workforce-core/personnel-action-orders`.
  2. DTO request dan response: `PersonnelActionOrderListItemDto`, `PersonnelActionOrderDetailDto`, `CreatePersonnelActionOrderDto`, `UpdatePersonnelActionOrderDto`, `SubmitPersonnelActionOrderDto`, `ApprovePersonnelActionOrderDto`, `RejectPersonnelActionOrderDto`, `CancelPersonnelActionOrderDto`.
  3. Endpoint unggah berkas `POST .../{id}/attachments` yang menyimpan metadata dan berkas PDF/JPG/PNG max 10MB.
* **Dependency**: `BE-HRD-024`
* **Acceptance Criteria**:
  1. `GET /api/v1/corporate/human-resource/workforce-core/personnel-action-orders` mengembalikan `ApiResponse<PagedResult<PersonnelActionOrderListItemDto>>` dengan filter `actionType`, `status`, `organizationUnitId`, `startDate`, `endDate`.
  2. `GET .../{id}` mengembalikan detail lengkap SK beserta daftar penempatan terdampak dan berkas lampiran fisik.
  3. `POST .../create` membuat record SK berstatus `Draft`.
  4. `POST .../{id}/submit`, `approve`, `reject`, `cancel` menjalankan aksi transisi status sesuai wewenang.
  5. Seluruh endpoint dilindungi atribut authorisasi `[AccessAction("Corporate.HumanResource.WorkforceCore.PersonnelActionOrder.*")]`.
* **Bukti Verifikasi**:
  - Swagger UI memproyeksikan tag grup dan skema DTO secara tepat.
  - Kompilasi project (`dotnet build`) sukses 0 error.
* **Risiko & Pemilik**: Pemilik: Backend Technical Lead. Risiko: sanitasi nama berkas dan MIME-type pada attachment handler.
* **Definition of Done**: Seluruh endpoint lolos audit Swagger, DTO mematuhi API Contract v6, authorisasi RBAC aktif.

### `BE-HRD-026` — Controller & Service Profil Terpadu Pegawai (Employee 360) dengan Dual-Mode Masking Keamanan
* **Slice**: `S-A1` (Workforce Core Target State)
* **Status**: Selesai (✅) — Lihat [Laporan Tracked](../task/report/backend/BE-HRD-026.md)
* **Outcome**: Controller RESTful resmi ber-tag Swagger `[Tags("Corporate / Human Resource / Workforce Core / Employee 360")]` aktif melayani data profil komprehensif pegawai (identitas, penempatan, jabatan, atasan, riwayat pendidikan/pelatihan, dan riwayat mutasi SK) dengan filter otorisasi dan masking keamanan dinamis berbasis relasi aktor login.
* **Traceability**: `HRD-DEC-053`, `FR-HRD-018`, `VR-WFC-06`, `VR-WFC-07`, `contracts/api-contract.md` §Employee360.
* **Contract Version**: API Contract v6, Validation Matrix v6.
* **Kemampuan Existing yang Dipakai**: `IWorkforceProfileService`, `WfpManagerAssignment` (relasi hierarki manajerial langsung).
* **Cakupan yang Diharapkan**:
  1. Controller `HrdEmployee360Controller` pada route `api/v1/corporate/human-resource/workforce-core/employee-360/{workforceProfileId}`.
  2. Service `IHrdEmployee360Service` yang mengagregasikan sub-domain data profil dari database.
  3. Mekanisme evaluasi peran: mendeteksi apakah pemanggil adalah HR Officer berwenang, Atasan Langsung aktif pegawai bersangkutan, atau pihak tidak berhak.
  4. Logika sanitasi/masking data personal sensitif dan kompensasi gaji.
* **Dependency**: `BE-HRD-024`
* **Acceptance Criteria**:
  1. Jika pemanggil memiliki hak HR (`Corporate.HumanResource.WorkforceCore.Employee360.ViewAll`), kembalikan seluruh data termasuk kompensasi, rekening, NIK, keluarga, dan catatan disiplin (`VR-WFC-06`).
  2. Jika pemanggil adalah Atasan Langsung (`DirectManager`), validasi bahwa target adalah bawahan langsung aktif (`VR-WFC-07`).
  3. Untuk Atasan Langsung, respons wajib secara mutlak menyembunyikan gaji pokok, tunjangan, nomor rekening bank, NIK, susunan keluarga, dan sanksi disiplin (`HRD-DEC-053`).
  4. Jika pemanggil bukan HR dan bukan atasan langsung dari target profil, tolak permintaan dengan HTTP 403 Forbidden.
* **Bukti Verifikasi**:
  - Verifikasi payload keluaran untuk simulasi user HR vs user Direct Manager.
  - Kompilasi project (`dotnet build`) sukses 0 error.
* **Risiko & Pemilik**: Pemilik: Backend Security & Domain Lead. Risiko: kebocoran data gaji pada manajer diatasi dengan DTO terpisah atau masking ketat sebelum serialisasi.
* **Definition of Done**: Endpoint Employee 360 aktif dan mematuhi batas privasi klinis/keuangan HRD-DEC-053.

### `BE-HRD-027` — Service & Controller Inisialisasi Penempatan Baseline Pegawai Eksisting (55 Staf)
* **Slice**: `S-A1` (Workforce Core Target State)
* **Status**: Selesai (✅) — Lihat [Laporan Tracked](../task/report/backend/BE-HRD-027.md)
* **Outcome**: Modul API dan service batch satu kali untuk memasukkan dan memvalidasi penempatan organisasi, jabatan, atasan, dan gaji bagi 55 staf eksisting agar memiliki rekam data penempatan resmi yang terstruktur tanpa merusak integritas historis.
* **Traceability**: `HRD-DEC-054`, `FR-HRD-019`, `VR-WFC-08`, `VR-WFC-09`, `contracts/api-contract.md` §InitialPlacementBatch.
* **Contract Version**: API Contract v6, Data Dictionary v6.
* **Kemampuan Existing yang Dipakai**: `MstWorkforceProfile`, `WfpOrganizationAssignment`, `WfpPositionAssignment`, `WfpManagerAssignment`, `WfpSalaryAssignment`.
* **Cakupan yang Diharapkan**:
  1. Controller `HrdInitialPlacementBatchController` pada route `api/v1/corporate/human-resource/workforce-core/initial-placement-batches`.
  2. Engine pemrosesan batch yang membaca daftar baris penempatan awal, memverifikasi ketiadaan duplikasi, dan mengeksekusi penulisan dalam satu transaksi database.
* **Dependency**: `BE-HRD-024`
* **Acceptance Criteria**:
  1. Menyediakan endpoint pembuatan draf batch `POST .../initial-placement-batches` dan query detail status batch `GET .../{id}`.
  2. Menyediakan endpoint eksekusi batch `POST .../{id}/execute`.
  3. Validasi batch memastikan tidak ada profil ganda dalam satu batch dan setiap profil belum memiliki penempatan aktif yang bertentangan (`VR-WFC-08`, `VR-WFC-09`).
  4. Eksekusi batch bersifat atomik: jika 1 baris gagal validasi, seluruh batch di-rollback dengan status `Failed` beserta daftar detail baris error (`HRD-DEC-054`).
  5. Setelah sukses dieksekusi, status batch berubah menjadi `Executed` dan tidak dapat dijalankan ulang.
* **Bukti Verifikasi**:
  - Pemeriksaan transaksi rollback saat baris invalid disimulasikan.
  - Kompilasi project (`dotnet build`) sukses 0 error.
* **Risiko & Pemilik**: Pemilik: Database & Backend Lead. Risiko: duplikasi penempatan awal dicegah dengan flag status executed dan cek penempatan eksisting.
* **Definition of Done**: Service batch teruji, endpoint lolos validasi authorisasi dan kontrak API.

### `BE-HRD-028` — Endpoint Kuasa HR (On-Behalf) Perubahan Profil Pegawai & Penguncian Form Master Data
* **Slice**: `S-A1` (Workforce Core Target State)
* **Status**: Selesai (✅) — Lihat [Laporan Tracked](../task/report/backend/BE-HRD-028.md)
* **Outcome**: Jalur pengajuan perubahan data profil pegawai atas nama pegawai oleh petugas HR (on-behalf) aktif dengan audit trail lengkap dan segregasi tugas (pembuat on-behalf dilarang memverifikasi permohonannya sendiri), serta penyesuaian backend master data pegawai untuk menolak pemindahan unit kerja dan jabatan secara langsung.
* **Traceability**: `HRD-DEC-047`, `HRD-DEC-052`, `FR-HRD-016`, `FR-HRD-017`, `VR-WFC-10`, `contracts/api-contract.md` §EmployeeProfileChange.
* **Contract Version**: API Contract v6, Validation Matrix v6.
* **Kemampuan Existing yang Dipakai**: `EmployeeProfileChangeController`, `WorkforceProfileController`.
* **Cakupan yang Diharapkan**:
  1. Penambahan action `POST /api/v1/corporate/human-resource/workforce-core/employee-profile-changes/on-behalf` pada `EmployeeProfileChangeController`.
  2. Pencatatan flag `IsSubmittedOnBehalf = true`, `SubmittedByWorkforceProfileId = actorId`, dan field alasan kuasa `OnBehalfReason`.
  3. Penegakan aturan: verifikator persetujuan wajib merupakan petugas HR yang berbeda dari aktor yang mengajukan kuasa (`VR-WFC-10`, `HRD-DEC-052`).
  4. Modifikasi `WorkforceProfileController` pada method Update agar mengabaikan atau menolak perubahan langsung pada kolom organisasi dan jabatan, mengharuskan pemakai menggunakan alur SK Kepegawaian (`HRD-DEC-047`).
* **Dependency**: `BE-HRD-024`
* **Acceptance Criteria**:
  1. Endpoint `POST .../employee-profile-changes/on-behalf` menerima payload perubahan data mandiri dan mencatat identitas petugas pembuat kuasa.
  2. Sistem menolak persetujuan permohonan on-behalf jika verifikator memiliki ID yang sama dengan pembuat kuasa (`VR-WFC-10`).
  3. Request `PUT /api/v1/corporate/human-resource/master-data/employees/{id}` menolak perubahan unit organisasi atau jabatan dengan pesan penolakan yang mengarahkan ke menu SK Kepegawaian.
* **Bukti Verifikasi**:
  - Verifikasi penolakan persetujuan on-behalf oleh pembuatnya sendiri.
  - Verifikasi proteksi field unit/jabatan pada endpoint master data pegawai.
  - Kompilasi project (`dotnet build`) sukses 0 error.
* **Risiko & Pemilik**: Pemilik: Backend HR Domain Lead. Risiko: kompatibilitas seeder master data existing diantisipasi dengan bypass khusus flag initialization.
* **Definition of Done**: Endpoint on-behalf aktif, guard clause segregasi tugas terpasang, form master data terlindungi.
