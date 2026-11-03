# Requirement Traceability — Modul Human Resource

## Metadata

```yaml
module_id: human-resource
blueprint_id: HRD-BP-001
blueprint_shape: SINGLE
roadmap_revision: 6
status: ACTIVE
status_synced_at: "2026-11-03 — PLAN-REPAIR-HRD-002: FE-HRD-026 dan FE-HRD-027 dikoreksi ke 🟡; bukti pengerjaan ulang FE-HRD-022, FE-HRD-024, FE-HRD-025 ditautkan. Sebelumnya (2026-11-02): implementasi end-to-end Slice S-A1"
planning_updated_at: "2026-11-03 — jejak S-D2 Rekrutmen & Hiring ditambahkan (bagian 1b): BE-HRD-018, BE-HRD-052 s.d. BE-HRD-062, FE-HRD-044 s.d. FE-HRD-057"
generated_at: "2026-10-26"
owners:
  - "Module Lead & Quality Assurance"
source_commits:
  backend: "26066c76e69b021f68b63b31bde2258cd9d1bb74 (S-D1); 16f6b2ad28e0e9cbd374354abe240ff3b163ace0 (task lama)"
  frontend: "66b7e5600e1707e4153ca07981ddef054856c8c9 (S-D1); HamzahV2 (task lama)"
contract_versions_s_d1: "v7 — API bagian 13, State bagian 11, Validation bagian 12, Permission bagian 11, Integration bagian 11, Data Dictionary bagian 8"
contract_versions_s_d2: "v8 — API bagian 14, State bagian 12, Validation bagian 13, Permission bagian 12, Integration bagian 12, Data Dictionary bagian 9"
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
| `IN-03` | Perencanaan Tenaga Kerja (Workforce Planning) | `HRD-DEC-058` s.d. `HRD-DEC-072`; desain `S-D1` approved Pengguna 3 November 2026 | API v7 §13, State v7 §11, Validation v7 §12, Permission v7 §11, Integration v7 §11, Data v7 §8 | `BE-HRD-017` ✅, `BE-HRD-029` ✅, `BE-HRD-030` ✅, `BE-HRD-031` ✅, `BE-HRD-032` ✅, `BE-HRD-033` ✅, `BE-HRD-034` s.d. `BE-HRD-051` (`BE-HRD-045`, `BE-HRD-049`, `BE-HRD-050`, `BE-HRD-051` ⛔) | `FE-HRD-029` s.d. `FE-HRD-043` | Fondasi data BE-HRD-017 selesai; penelusuran Direktur BE-HRD-029 selesai; integrasi workflow BE-HRD-030 selesai; pengaturan rumah sakit BE-HRD-031 selesai; buku formasi berversi BE-HRD-032 selesai; batch formasi awal BE-HRD-033 selesai. Rincian per requirement pada bagian 1a |
| `IN-04` | Rekrutmen, Seleksi Pelamar, dan Hiring | `HRD-DEC-073` s.d. `HRD-DEC-080`; desain `S-D2` approved Pengguna 3 November 2026 | API v8 §14, State v8 §12, Validation v8 §13, Permission v8 §12, Integration v8 §12, Data v8 §9 | `BE-HRD-018`, `BE-HRD-052` s.d. `BE-HRD-062` | `FE-HRD-044` s.d. `FE-HRD-057` | Fondasi skema & 12 model Hrd siap; endpoint Swagger 9 grup siap; 14 task FE siap dikerjakan. Rincian per requirement pada bagian 1b |
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
| `IN-02-SK` | Dokumen SK Kepegawaian & Alur Persetujuan Mutasi/Rotasi/Demosi | `HRD-DEC-048`, `050`, `056`, `057` | API §PersonnelActionOrder, State §Order, Validation §SK | `BE-HRD-023` ✅, `024` ✅, `025` ✅ | `FE-HRD-022` ✅, `023` ✅, `024` ✅ | ✅ Selesai Penuh (BE & FE SK Kepegawaian aktif, Laporan [FE-022](../task/report/frontend/FE-HRD-022.md) s.d. [FE-024](../task/report/frontend/FE-HRD-024.md)) |
| `IN-02-360` | Profil Terpadu Pegawai (Employee 360) Dual-View HR vs Manager | `HRD-DEC-053`, `FR-HRD-018` | API §Employee360, Validation §360 | `BE-HRD-026` ✅ | `FE-HRD-025` ✅ | ✅ Selesai Penuh (BE & FE Employee 360 Dual-View aktif, Laporan [FE-025](../task/report/frontend/FE-HRD-025.md)) |
| `IN-02-BATCH` | Inisialisasi Batch Penempatan Awal Staf Eksisting (55 Staf) | `HRD-DEC-054`, `FR-HRD-019` | API §InitialPlacementBatch, Data §Batch | `BE-HRD-027` ✅ | `FE-HRD-026` 🟡 | 🟡 Sebagian (dikoreksi 03-11-2026): BE aktif; layar FE belum tersambung ke kontrak backend — menunggu `FIX-HRD-002-09` ([Laporan FE-026](../task/report/frontend/FE-HRD-026.md)) |
| `IN-02-OB` | Permohonan Perubahan Profil Mandiri & Kuasa HR (On-Behalf) | `HRD-DEC-047`, `052`, `FR-HRD-017` | API §EmployeeProfileChange, Validation §OnBehalf | `BE-HRD-028` ✅ | `FE-HRD-027` 🟡, `FE-HRD-028` ✅ | 🟡 Sebagian (dikoreksi 03-11-2026): kunci form Master Data aktif; form Kuasa HR belum dapat mengirim (415) — menunggu `FIX-HRD-002-09` ([Laporan FE-027](../task/report/frontend/FE-HRD-027.md), [FE-028](../task/report/frontend/FE-HRD-028.md)) |


---

## 1a. Jejak `S-D1` Perencanaan Tenaga Kerja — requirement ke task dan bukti

Ditambahkan pada revision 5, 3 November 2026, sesudah desain `S-D1` disetujui pemilik. Seluruh
baris di bawah **belum punya bukti implementasi**: task-nya baru direncanakan. Kolom status akan
diperbarui build skill saat task selesai.

| Field | Nilai |
| --- | --- |
| Approval desain | Pengguna, pemilik teknis dan produk (`HRD-DEC-015`), 3 November 2026 — `02-backend-architecture.md` bagian 16, `03-frontend-architecture.md` bagian 12, kontrak `v7`, `04-prd-to-mvp.md` bagian 21, `data/data-dictionary.md` bagian 8, `testing/acceptance-test-matrix.md` bagian 15, tiga flowchart `S-D1` |
| Keputusan | `HRD-DEC-058` s.d. `HRD-DEC-072` (decision log revision `17`, bagian 32) |
| Kontrak | API v7 bagian 13; State Transition v7 bagian 11; Validation v7 bagian 12; Permission v7 bagian 11; Integration v7 bagian 11; Data Dictionary v7 bagian 8 |
| Manifest | revision `14`, hash artefak pada `blueprint-manifest.md` dan metadata `backend-roadmap.md` |
| Source SHA | BE `26066c76e69b021f68b63b31bde2258cd9d1bb74`, FE `66b7e5600e1707e4153ca07981ddef054856c8c9` |

### Functional requirement

| FR | Perilaku | Desain | Task backend | Task frontend | Bukti yang diharapkan | Status |
| --- | --- | --- | --- | --- | --- | :---: |
| `FR-HRD-092` | Standar per unit berversi, satu versi berlaku per tanggal | BE §16.4.1; State §11.1 | `BE-HRD-035`, `BE-HRD-036` | `FE-HRD-033`, `FE-HRD-034` | `AT-HRD-D1-04`, `UAT-29` | |
| `FR-HRD-093` | Tiga metode; Analisis Beban Kerja ditolak | BE §16.4.1; `VR-WFP-02` | `BE-HRD-035` | `FE-HRD-034` | `AT-HRD-D1-02b` | |
| `FR-HRD-094` | Rekomendasi beserta rincian (ICU → 32) | BE §16.9 | `BE-HRD-035` | `FE-HRD-034` | `AT-HRD-D1-02`, `UAT-29` | |
| `FR-HRD-095` | Standar hanya dipakai setelah `T9` | State §11.1 | `BE-HRD-035`, `BE-HRD-036`, `BE-HRD-037` | `FE-HRD-034` | `AT-HRD-D1-03`, `AT-HRD-D1-04` | |
| `FR-HRD-096` | Angka beban beserta sumber; isian ganda ditolak | BE §16.4.2; `VR-WFP-08`, `VR-WFP-09` | `BE-HRD-034` | `FE-HRD-032` | Acceptance criteria `BE-HRD-034` — **tanpa baris `AT-HRD-D1`** | |
| `FR-HRD-097` | Penjadwalan membaca standar berlaku | `INT-HR-13` | `BE-HRD-035` | — | `AT-HRD-D1-19` | |
| `FR-HRD-098` | Formasi tidak dapat diubah; method tulis `405` | BE §16.4.3; `VR-WFP-10` | `BE-HRD-032` | `FE-HRD-030` | `AT-HRD-D1-06` | |
| `FR-HRD-099` | Setiap versi menyimpan dokumen sumber, penyetuju, waktu | BE §16.4.3 | `BE-HRD-032`, `BE-HRD-033`, `BE-HRD-041`, `BE-HRD-044`, `BE-HRD-047` | `FE-HRD-030` | `AT-HRD-D1-17` | |
| `FR-HRD-100` | Versi terakhir berlaku sampai digantikan | `FORMATION_CARRIES_FORWARD` | `BE-HRD-032` | `FE-HRD-030` | `AT-HRD-D1-07` | |
| `FR-HRD-101` | Batch formasi awal: scan wajib, tanpa baris gagal, `T10` oleh orang lain, sekali per unit × jabatan | BE §16.4.4; State §11.2 | `BE-HRD-033` | `FE-HRD-031` | `AT-HRD-D1-17`, `UAT-31`, `UAT-32` | |
| `FR-HRD-102` | Gap per tanggal acuan dalam porsi kerja, hanya penempatan utama | BE §16.9 | `BE-HRD-037` | `FE-HRD-035` | `AT-HRD-D1-05`, `UAT-33` | |
| `FR-HRD-103` | Tiga gap: permanen, sementara, kompetensi | BE §16.9 | `BE-HRD-037` | `FE-HRD-035` | `UAT-33` | |
| `FR-HRD-104` | Data belum ada tampil "data belum tersedia", bukan 0 | `UNKNOWN_IS_NOT_ZERO` | `BE-HRD-037`, `BE-HRD-038`, `BE-HRD-039` | `FE-HRD-029`, `FE-HRD-035`, `FE-HRD-040`, `FE-HRD-043` | `AT-HRD-D1-05b`, `UAT-33` | |
| `FR-HRD-105` | Potret saat diajukan dan akhir bulan; tidak dapat diubah; bulan terlewat tidak diisi diam-diam | BE §16.4.7; State §11.7 | `BE-HRD-039`, `BE-HRD-048` | `FE-HRD-040` | `AT-HRD-D1-16`, `AT-HRD-D1-16b`, `UAT-34` | |
| `FR-HRD-106` | Satu rencana tahunan per tahun; `Approved` terkunci | State §11.3 | `BE-HRD-046` | `FE-HRD-041`, `FE-HRD-042` | `AT-HRD-D1-15`, `UAT-36` | |
| `FR-HRD-107` | Addendum dari rencana `Approved`, baris berubah, `T10` penuh | State §11.3 | `BE-HRD-046`, `BE-HRD-047` | `FE-HRD-042` | `AT-HRD-D1-15` | |
| `FR-HRD-108` | Usulan berbeda dari rekomendasi wajib beralasan | `VR-WFP-19` | `BE-HRD-046` | `FE-HRD-042` | Acceptance criteria `BE-HRD-046` — **tanpa baris `AT-HRD-D1`** | |
| `FR-HRD-109` | Biaya = porsi × gaji tengah grade × bulan, berlabel estimasi | BE §16.4.9 | `BE-HRD-038`, `BE-HRD-046` | `FE-HRD-042` | `AT-HRD-D1-11` | |
| `FR-HRD-110` | Finance mengoreksi total pada langkahnya; angka awal tersimpan | `FINANCE_ADJUSTMENT` | `BE-HRD-040`, `BE-HRD-046`; kaki `VR-WFP-21` di `BE-HRD-049` ⛔ | `FE-HRD-037`, `FE-HRD-042` | `AT-HRD-D1-21`, `UAT-35` | Sebagian tertahan `HRD-PLAN-GAP-02` |
| `FR-HRD-111` | Tanpa `WorkforcePlanningCost : Read` tidak menerima biaya | `VR-WFP-37` | `BE-HRD-038` | `FE-HRD-029`, `FE-HRD-037`, `FE-HRD-039`, `FE-HRD-041`, `FE-HRD-042`, `FE-HRD-043` | `AT-HRD-D1-11` | |
| `FR-HRD-112` | Persetujuan rencana melahirkan formasi dan menandai permintaan berlebih | State §11.3, §11.8 | `BE-HRD-047` | `FE-HRD-038` (keterangan kursi beku) | `AT-HRD-D1-15`, `UAT-35` | |
| `FR-HRD-113` | Permintaan dalam formasi ≤ sisa kursi, diperiksa ulang pada persetujuan terakhir | `REMAINING_SEATS`, `RECHECK_ON_FINAL_APPROVAL` | `BE-HRD-040`; pemeriksaan ulang di `BE-HRD-049` ⛔ | `FE-HRD-037` | `AT-HRD-D1-08`, `AT-HRD-D1-08b`, `UAT-38` | Sebagian tertahan `HRD-PLAN-GAP-02` |
| `FR-HRD-114` | Dua permintaan bersamaan tidak mengklaim kursi yang sama | `NO_DOUBLE_CLAIM_ON_SEATS`, `NFR-010` | `BE-HRD-040` | — | `AT-HRD-D1-09` | |
| `FR-HRD-115` | `T11` untuk dalam formasi dan sementara; `T12` untuk di luar formasi | `HRD-DEC-064` | `BE-HRD-040`, `BE-HRD-041` | `FE-HRD-037` | `AT-HRD-D1-10`, `UAT-37` | |
| `FR-HRD-116` | Direktur dari matriks lewat unit induk; kosong ke Direktur Utama; keduanya kosong ditolak | `HRD-DEC-072` | `BE-HRD-029` | — | `AT-HRD-D1-20` | |
| `FR-HRD-117` | Kursi terisi saat penempatan cocok berlaku; satu penempatan satu kursi | `HRD-DEC-066` | `BE-HRD-043` | `FE-HRD-038`, `FE-HRD-039` | `AT-HRD-D1-12`, `AT-HRD-D1-13`, `UAT-39` | |
| `FR-HRD-118` | Kursi luar di antrean Recruiter; kursi internal lewat SK | `HRD-DEC-066`, `INT-HR-17` | `BE-HRD-042`, `BE-HRD-043` | `FE-HRD-039` | `AT-HRD-D1-14` | |
| `FR-HRD-119` | HR Manager bukan pembuat mengubah sumber kursi yang belum terisi, beralasan | `VR-WFP-33` | `BE-HRD-042` | `FE-HRD-038` | `AT-HRD-D1-14` | |
| `FR-HRD-120` | Pembatalan hanya kursi belum terisi, setelah lowongan ditutup, lewat `T13`; formasi `T12` ikut turun | `HRD-DEC-071` | `BE-HRD-044`; syarat lowongan di `BE-HRD-045` ⛔ | `FE-HRD-038` | `AT-HRD-D1-18`, `UAT-39`, `UAT-40` | Sebagian tertahan `HRD-PLAN-GAP-01` |
| `FR-HRD-121` | Permintaan lewat batas ditandai, tanpa pembatalan otomatis | `NO_AUTO_EXPIRY` | `BE-HRD-043` | `FE-HRD-036`, `FE-HRD-037` | Acceptance criteria `BE-HRD-043` — **tanpa baris `AT-HRD-D1`** | |

### Kebutuhan non-fungsional

| NFR | Task yang menegakkan | Bukti |
| --- | --- | --- |
| `NFR-010` advisory lock unit × jabatan | `BE-HRD-032`, `BE-HRD-040` | `AT-HRD-D1-09` |
| `NFR-011` persetujuan yang melahirkan formasi atomik | `BE-HRD-033`, `BE-HRD-041`, `BE-HRD-044`, `BE-HRD-047` | Review jalur penerap; `BE-HRD-051` |
| `NFR-012` `RowVersion` → `409` | `BE-HRD-017` dan setiap service dokumen; `FE-HRD-029` (pesan `409`) | Uji API per task |
| `NFR-013` keputusan dan scheduler idempoten | `BE-HRD-030`, `BE-HRD-033`, `BE-HRD-036`, `BE-HRD-041`, `BE-HRD-043`, `BE-HRD-044`, `BE-HRD-047`, `BE-HRD-048` | Uji jalan ganda per task |
| `NFR-014` biaya dan alasan tidak masuk logger maupun penyimpanan klien | Seluruh task backend `S-D1`; `BE-HRD-038`; `FE-HRD-043` | Review diff; pemeriksaan penyimpanan peramban |
| `NFR-015` tanggal operasional rumah sakit | `BE-HRD-036`, `BE-HRD-048`, dan seluruh perhitungan tanggal acuan | Uji runtime scheduler |

### Matriks acceptance test ke task

| Acceptance test | Task |
| --- | --- |
| `AT-HRD-D1-01`, `AT-HRD-D1-01b` | **Tidak ada task** — migration pembersihan kandidat tidak dijadwalkan pada `S-D1` (`02-backend-architecture.md` bagian 16.7) |
| `AT-HRD-D1-02`, `AT-HRD-D1-02b` | `BE-HRD-035`, `FE-HRD-034` |
| `AT-HRD-D1-03` | `BE-HRD-035`, `BE-HRD-037` |
| `AT-HRD-D1-04` | `BE-HRD-035`, `BE-HRD-036`, `BE-HRD-051` |
| `AT-HRD-D1-05` | `BE-HRD-037` |
| `AT-HRD-D1-05b` | `BE-HRD-037`, `FE-HRD-035`, `FE-HRD-043` |
| `AT-HRD-D1-06` | `BE-HRD-032`, `FE-HRD-030`, `FE-HRD-043` |
| `AT-HRD-D1-07` | `BE-HRD-032` |
| `AT-HRD-D1-08`, `AT-HRD-D1-08b`, `AT-HRD-D1-09` | `BE-HRD-040`, `FE-HRD-037` |
| `AT-HRD-D1-10` | `BE-HRD-040`, `BE-HRD-051` |
| `AT-HRD-D1-11` | `BE-HRD-038`, `BE-HRD-040`, `BE-HRD-042`, `FE-HRD-039`, `FE-HRD-043` |
| `AT-HRD-D1-12` | `BE-HRD-043`, `BE-HRD-051` |
| `AT-HRD-D1-13` | `BE-HRD-043` |
| `AT-HRD-D1-14` | `BE-HRD-042`, `BE-HRD-051` |
| `AT-HRD-D1-15` | `BE-HRD-046`, `BE-HRD-047`, `BE-HRD-051` |
| `AT-HRD-D1-16` | `BE-HRD-039`, `FE-HRD-037`, `FE-HRD-042`, `FE-HRD-043` |
| `AT-HRD-D1-16b` | `BE-HRD-048`, `FE-HRD-040` |
| `AT-HRD-D1-17` | `BE-HRD-033`, `FE-HRD-031`, `BE-HRD-051` |
| `AT-HRD-D1-18` | `BE-HRD-044`, `BE-HRD-045` ⛔, `FE-HRD-038`, `BE-HRD-051` |
| `AT-HRD-D1-19` | `BE-HRD-035`, `BE-HRD-051` |
| `AT-HRD-D1-20` | `BE-HRD-029`, `BE-HRD-051` |
| `AT-HRD-D1-21` | `BE-HRD-046`, `BE-HRD-049` ⛔, `FE-HRD-042`, `BE-HRD-051` |
| `AT-HRD-D1-22` | `FE-HRD-029`, `FE-HRD-043` |

### Gap keterkaitan requirement ke bukti verifikasi

Sesuai `rules/backend/TEST_POLICY.md`, tidak adanya automated test backend **bukan** gap. Yang
tercatat di bawah adalah requirement yang belum punya jalan pembuktian penuh.

| Gap | Akibat | Pemilik |
| --- | --- | --- |
| `HRD-PLAN-GAP-01` — sumber data "lowongan terbuka" untuk `VR-WFP-36` belum dirancang | `FR-HRD-120` dan `UAT-40` tidak dapat dibuktikan penuh; `BE-HRD-045` ⛔ | Pengguna, lewat revisi desain `S-D1` |
| `HRD-PLAN-GAP-02` — mesin workflow belum punya kait validasi sebelum persetujuan per langkah | Kaki `VR-WFP-21` (`FR-HRD-110`, `AT-HRD-D1-21`) dan `VR-WFP-23` (`FR-HRD-113`) tidak dapat dibuktikan; `BE-HRD-049` ⛔ | Pemilik arsitektur backend (Pengguna), lewat revisi bagian 16.4.9 |
| Pemetaan peran `HRD-DEC-032` | Alur persetujuan sungguhan `T9` s.d. `T13` tidak dapat diuji; `BE-HRD-050` dan `BE-HRD-051` ⛔ | Administrator |
| `FR-HRD-096`, `FR-HRD-108`, `FR-HRD-121` tidak punya baris di `testing/acceptance-test-matrix.md` bagian 15 | Pembuktiannya hanya lewat acceptance criteria task (`BE-HRD-034`, `BE-HRD-046`, `BE-HRD-043`) | Pemilik blueprint — dapat ditambahkan pada revisi matriks berikutnya; tidak menahan task |
| `AT-HRD-D1-01`, `AT-HRD-D1-01b` tanpa task | Pengaman tabel kosong baru terbukti saat migration pembersihan dijadwalkan | Setelah desain `S-D2` dan Penjadwalan |

---

## 2. Penelusuran Terbalik (Task Implementation ke Dokumen Bukti)

| Task ID | Nama Modul / Fitur | Berkas Source Backend Terkait | Berkas Antarmuka Frontend Terkait | Status Validasi | Laporan Tracked |
| --- | --- | --- | --- | :---: | --- |
| `FE-HRD-001` | Rute Kebab-Case Master Data | 8 Master Data Controller HR | `src/lib/constants/hr/master-data/**`, slice | ✅ Lulus Linter | [FE-HRD-001.md](../task/report/frontend/FE-HRD-001.md) |
| `FE-HRD-002` | Workforce Core Cross-Employee | `WorkforceCoreCrossEmployeeController.cs`, `EmployeeProfileChangeController.cs` | `src/app/hr/workforce-core/**`, view, service | ✅ Lulus Linter & Test Unit | [FE-HRD-002.md](../task/report/frontend/FE-HRD-002.md) |
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
| `BE-HRD-023` | Skema & Migration SK Kepegawaian | `HrdPersonnelActionOrder.cs`, `HrdInitialPlacementBatch.cs`, Migrations | — | ✅ Selesai | [BE-HRD-023.md](../task/report/backend/BE-HRD-023.md) |
| `BE-HRD-024` | Service Mutasi & Persetujuan SK | `HrdPersonnelActionOrderService.cs` | — | ✅ Selesai | [BE-HRD-024.md](../task/report/backend/BE-HRD-024.md) |
| `BE-HRD-025` | Controller API SK Kepegawaian | `HrdPersonnelActionOrderController.cs` | — | ✅ Selesai | [BE-HRD-025.md](../task/report/backend/BE-HRD-025.md) |
| `BE-HRD-026` | API Employee 360 Dual-View | `HrdEmployee360Controller.cs`, `HrdEmployee360Service.cs` | — | ✅ Selesai | [BE-HRD-026.md](../task/report/backend/BE-HRD-026.md) |
| `BE-HRD-027` | API Batch Penempatan Awal | `HrdInitialPlacementBatchController.cs`, `HrdInitialPlacementBatchService.cs` | — | ✅ Selesai | [BE-HRD-027.md](../task/report/backend/BE-HRD-027.md) |
| `BE-HRD-028` | API Kuasa HR & Lock Master Data | `EmployeeProfileChangeController.cs`, `EmployeeController.cs` | — | ✅ Selesai | [BE-HRD-028.md](../task/report/backend/BE-HRD-028.md) |
| `FE-HRD-022` | Layar Daftar Dokumen SK | `HrdPersonnelActionOrderController.cs` | `src/app/hr/workforce-core/personnel-actions/**`, service, slice | ✅ Dikerjakan ulang 03-11-2026 (PLAN-REPAIR-HRD-002): build lulus, check-css-usage 0 temuan, screenshot terlampir | [FE-HRD-022.md](../task/report/frontend/FE-HRD-022.md) |
| `FE-HRD-023` | Rute Halaman & Daftar SK | `HrdPersonnelActionOrderController.cs` | `src/app/hr/personnel-action-orders/**`, view | ✅ Lulus Linter & Test Unit | [FE-HRD-023.md](../task/report/frontend/FE-HRD-023.md) |
| `FE-HRD-024` | Form Penerbitan & Detail SK | `HrdPersonnelActionOrderController.cs` | `src/app/hr/workforce-core/personnel-actions/create/**`, `[id]/**` | ✅ Dikerjakan ulang 03-11-2026 (PLAN-REPAIR-HRD-002): build lulus, check-css-usage 0 temuan, screenshot terlampir | [FE-HRD-024.md](../task/report/frontend/FE-HRD-024.md) |
| `FE-HRD-025` | Antarmuka Employee 360 Dual-View | `HrdEmployee360Controller.cs` | `src/app/hr/workforce-core/employee-360/[id]/**`, service | ✅ Dikerjakan ulang 03-11-2026 (PLAN-REPAIR-HRD-002): build lulus, check-css-usage 0 temuan, screenshot terlampir | [FE-HRD-025.md](../task/report/frontend/FE-HRD-025.md) |
| `FE-HRD-026` | Layar Batch Penempatan Awal | `HrdInitialPlacementBatchController.cs` | `src/app/hr/workforce-core/initial-batches/**`, service | 🟡 Sebagian — tampilan dibangun ulang 03-11-2026; kontrak backend menunggu FIX-HRD-002-09 | [FE-HRD-026.md](../task/report/frontend/FE-HRD-026.md) |
| `FE-HRD-027` | Form Permohonan Kuasa HR | `EmployeeProfileChangeController.cs` | `src/app/hr/workforce-core/employee-profile-changes/create-on-behalf/**` | 🟡 Sebagian — tampilan dibangun ulang 03-11-2026; kontrak backend menunggu FIX-HRD-002-09 | [FE-HRD-027.md](../task/report/frontend/FE-HRD-027.md) |
| `FE-HRD-028` | Penguncian Form Master Data | `EmployeeController.cs` | `src/components/view/hr/master-data/employees/form/**` | ✅ Lulus Linter & Test Unit | [FE-HRD-028.md](../task/report/frontend/FE-HRD-028.md) |


## 1b. Jejak `S-D2` Rekrutmen dan Hiring — requirement ke task dan bukti

Ditambahkan pada revision 6, 3 November 2026, sesudah desain `S-D2` disetujui pemilik. Seluruh
baris di bawah **belum punya bukti implementasi**: task-nya baru direncanakan. Kolom status akan
diperbarui build skill saat task selesai.

| Field | Nilai |
| --- | --- |
| Approval desain | Pengguna, pemilik teknis dan produk (`HRD-DEC-015`), 3 November 2026 — `02-backend-architecture.md` bagian 17, `03-frontend-architecture.md` bagian 13, kontrak `v8`, `04-prd-to-mvp.md` bagian 22, `data/data-dictionary.md` bagian 9, `testing/acceptance-test-matrix.md` bagian 16, flowchart `flowcharts/rekrutmen-dan-seleksi.md` |
| Keputusan | `HRD-DEC-073` s.d. `HRD-DEC-080` (decision log revision `18`, bagian 33) |
| Kontrak | API v8 bagian 14; State Transition v8 bagian 12; Validation v8 bagian 13; Permission v8 bagian 12; Integration v8 bagian 12; Data Dictionary v8 bagian 9 |
| Manifest | revision `15`, hash artefak pada `blueprint-manifest.md` dan metadata roadmap |
| Source SHA | BE `e8d47c77c1a40035bd83dfc6ee60aad7f6092f2f`, FE `cfa82fbb3bbaed45cda215582d3ef710811679cf` |

### Functional requirement

| FR | Perilaku Bisnis | Desain & Kontrak | Task Backend | Task Frontend | Bukti yang Diharapkan | Status |
| --- | --- | --- | --- | --- | --- | :---: |
| `FR-REC-01` | Requisition wajib menarik kursi yang tersedia dari `HeadcountRequestId` aktif | BE §17.4.1; API §14.1; `VR-REC-01`, `VR-REC-02` | `BE-HRD-052` | `FE-HRD-044`, `FE-HRD-045` | `AT-HRD-D2-01`, `AT-HRD-D2-02` | |
| `FR-REC-02` | Pembuatan lowongan kerja dengan kuota kursi & penanda peran klinis `IsClinical` | BE §17.4.2; API §14.2; `VR-REC-04` | `BE-HRD-053` | `FE-HRD-046`, `FE-HRD-047` | `AT-HRD-D2-03`, `UAT-REC-01` | |
| `FR-REC-03` | Pendaftaran pelamar dengan validasi unik NIK 16 digit & deteksi mantan pegawai rehire | BE §17.4.3; API §14.3; `VR-REC-05`, `VR-REC-06` | `BE-HRD-054` | `FE-HRD-048` | `AT-HRD-D2-04`, `UAT-REC-03` | |
| `FR-REC-04` | Penapisan berkas administrasi & auto-reject STR klinis kedaluwarsa | BE §17.4.4; API §14.4; `VR-REC-07`, `VR-REC-08` | `BE-HRD-055` | `FE-HRD-049` | `AT-HRD-D2-05`, `UAT-REC-02` | |
| `FR-REC-05` | Penjadwalan wawancara independen dan formulir evaluasi HR | BE §17.4.5; API §14.5; `VR-REC-10` | `BE-HRD-056` | `FE-HRD-050` | `AT-HRD-D2-06` | |
| `FR-REC-06` | Formulir uji klinis unit dengan penegakan hak veto mutlak asesor ruang | BE §17.4.5; API §14.5; `VR-REC-11`, `VR-REC-12` | `BE-HRD-056` | `FE-HRD-051` | `AT-HRD-D2-06`, `UAT-REC-04` | |
| `FR-REC-07` | Penerbitan Surat Penawaran Kerja Bersyarat (*Conditional Offer*) sebelum MCU | BE §17.4.6; API §14.6; `VR-REC-13` | `BE-HRD-057` | `FE-HRD-052` | `AT-HRD-D2-07` | |
| `FR-REC-08` | Integrasi kelaikan MCU pra-kerja dengan masking diagnosa medis K3RS | BE §17.4.7; API §14.7; `VR-REC-17` | `BE-HRD-059` | `FE-HRD-054` | `AT-HRD-D2-09`, `UAT-REC-05` | |
| `FR-REC-09` | Evaluasi penawaran upah terhadap grade: jalur standar T13 vs eskalasi T14 | BE §17.4.6; API §14.6; `VR-REC-14`, `VR-REC-15` | `BE-HRD-057`, `BE-HRD-058` | `FE-HRD-053` | `AT-HRD-D2-08`, `UAT-REC-06` | |
| `FR-REC-10` | Konversi kandidat final menjadi draf `WorkforceProfile` berstatus `Pending Onboarding` | BE §17.4.9; API §14.9; `VR-REC-20`, `INT-HR-20` | `BE-HRD-061` | `FE-HRD-056` | `AT-HRD-D2-11` | |
| `FR-REC-11` | Penampungan otomatis kandidat cadangan ke dalam Talent Pool selama 6 bulan | BE §17.4.8; API §14.8; `VR-REC-18` | `BE-HRD-060` | `FE-HRD-055` | `AT-HRD-D2-10` | |
| `FR-REC-12` | Promosi cepat (*Fast-Track*) kandidat cadangan tertinggi saat tawaran utama batal | BE §17.4.8; API §14.8; `VR-REC-19` | `BE-HRD-060` | `FE-HRD-055` | `AT-HRD-D2-10`, `UAT-REC-07` | |

### Keputusan bisnis

| Keputusan | Kebijakan yang Ditegakkan | Task Backend | Task Frontend | Bukti Verifikasi |
| --- | --- | --- | --- | --- |
| `HRD-DEC-073` | Lowongan wajib terikat formasi WFP disetujui Direktur | `BE-HRD-052` | `FE-HRD-044`, `FE-HRD-045` | `AT-HRD-D2-01`, `AT-HRD-D2-02` |
| `HRD-DEC-074` | 20 tabel legacy diasumsikan kosong & `EMPTY_TABLE_GUARD` wajib | `BE-HRD-018` | — | Script migration & `dotnet build` 0 error |
| `HRD-DEC-075` | Isolasi data pelamar, NIK rehire & deferred profile creation | `BE-HRD-054`, `BE-HRD-061` | `FE-HRD-048`, `FE-HRD-056` | `AT-HRD-D2-04`, `AT-HRD-D2-11`, `UAT-REC-03` |
| `HRD-DEC-076` | Penapisan wajib STR klinis aktif & auto-reject jika kedaluwarsa | `BE-HRD-055` | `FE-HRD-049` | `AT-HRD-D2-05`, `UAT-REC-02` |
| `HRD-DEC-077` | Wawancara independen & hak veto mutlak asesor unit klinis | `BE-HRD-056` | `FE-HRD-050`, `FE-HRD-051` | `AT-HRD-D2-06`, `UAT-REC-04` |
| `HRD-DEC-078` | Conditional offer sebelum MCU & masking rekam medis K3RS | `BE-HRD-057`, `BE-HRD-059` | `FE-HRD-052`, `FE-HRD-054` | `AT-HRD-D2-07`, `AT-HRD-D2-09`, `UAT-REC-05` |
| `HRD-DEC-079` | Rantai persetujuan bertingkat plafon upah (T13 standard vs T14 eskalasi) | `BE-HRD-057`, `BE-HRD-058` | `FE-HRD-053` | `AT-HRD-D2-08`, `UAT-REC-06` |
| `HRD-DEC-080` | Talent Pool retensi 6 bulan & promosi cepat kandidat cadangan | `BE-HRD-060` | `FE-HRD-055` | `AT-HRD-D2-10`, `UAT-REC-07` |

### Acceptance test dan skenario UAT

| Ref Uji | Cakupan Skenario | Task Terkait | Dokumen Bukti yang Diharapkan |
| --- | --- | --- | --- |
| `AT-HRD-D2-01` | Validasi Requisition terhadap Headcount WFP | `BE-HRD-052`, `FE-HRD-044` | Respons penolakan 422 saat kursi melebihi formasi |
| `AT-HRD-D2-02` | Pembatalan Requisition berstatus draf | `BE-HRD-052`, `FE-HRD-045` | Requisition berstatus Cancelled di dev DB |
| `AT-HRD-D2-03` | Publikasi lowongan kerja klinis & non-klinis | `BE-HRD-053`, `FE-HRD-046` | Lowongan terbit dengan penanda IsClinical akurat |
| `AT-HRD-D2-04` | Deteksi mantan karyawan via NIK (rehire flow) | `BE-HRD-054`, `FE-HRD-048` | Badge rehire aktif dan profil lama terhubung |
| `AT-HRD-D2-05` | Auto-reject pelamar klinis dengan STR kedaluwarsa | `BE-HRD-055`, `FE-HRD-049` | Status screening gugur otomatis tanpa intervensi manual |
| `AT-HRD-D2-06` | Penegakan hak veto klinis asesor ruang ICU | `BE-HRD-056`, `FE-HRD-051` | Status kandidat gagal asesmen; penawaran terkunci permanen |
| `AT-HRD-D2-07` | Penerbitan penawaran kerja bersyarat (conditional offer) | `BE-HRD-057`, `FE-HRD-052` | Dokumen conditional offer terbit sebelum MCU pra-kerja |
| `AT-HRD-D2-08` | Persetujuan bertingkat penawaran upah di atas plafon grade | `BE-HRD-058`, `FE-HRD-053` | Alur T14 mewajibkan persetujuan Finance dan Dirut |
| `AT-HRD-D2-09` | Perlindungan privasi diagnosa medis MCU pra-kerja | `BE-HRD-059`, `FE-HRD-054` | Akun HR non-medis hanya melihat kesimpulan Fit/Unfit |
| `AT-HRD-D2-10` | Promosi cepat kandidat cadangan dari Talent Pool | `BE-HRD-060`, `FE-HRD-055` | Surat penawaran baru terbit instan untuk kandidat cadangan #1 |
| `AT-HRD-D2-11` | Serah terima rekrutmen ke Onboarding Sub-modul 05 | `BE-HRD-061`, `FE-HRD-056` | Draf WorkforceProfile terbentuk berstatus Pending Onboarding |
| `AT-HRD-D2-12` | Keterjangkauan menu rekrutmen dan RBAC | `FE-HRD-057` | Tangkapan layar sidebar per peran dan npm run build sukses |
| `UAT-REC-01` s.d. `UAT-REC-07` | Validasi bisnis ujung-ke-ujung proses rekrutmen | `BE-HRD-062`, `FE-HRD-057` | Laporan uji komprehensif dev runtime S-D2 |
