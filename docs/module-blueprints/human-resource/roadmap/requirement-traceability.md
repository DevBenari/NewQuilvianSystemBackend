# Requirement Traceability — Modul Human Resource

## Metadata

```yaml
module_id: human-resource
blueprint_id: HRD-BP-001
blueprint_shape: SINGLE
roadmap_revision: 8
status: ACTIVE
status_synced_at: "2026-11-03 — PLAN-REPAIR-HRD-002: FE-HRD-026 dan FE-HRD-027 dikoreksi ke 🟡; bukti pengerjaan ulang FE-HRD-022, FE-HRD-024, FE-HRD-025 ditautkan. Sebelumnya (2026-11-02): implementasi end-to-end Slice S-A1"
planning_updated_at: "2026-11-04 — jejak S-B4 Penjadwalan, Roster, dan On-Call ditambahkan (bagian 1d): BE-HRD-087 s.d. BE-HRD-103, FE-HRD-075 s.d. FE-HRD-087; sebelumnya S-C4-ONB (bagian 1c)"
generated_at: "2026-10-26"
owners:
  - "Module Lead & Quality Assurance"
source_commits:
  backend: "8b17b75a (MHamzah); 26066c76e69b021f68b63b31bde2258cd9d1bb74 (S-D1); 16f6b2ad28e0e9cbd374354abe240ff3b163ace0 (task lama)"
  frontend: "44990b3d3 (HamzahV2); 66b7e5600e1707e4153ca07981ddef054856c8c9 (S-D1)"
contract_versions_s_d1: "v7 — API bagian 13, State bagian 11, Validation bagian 12, Permission bagian 11, Integration bagian 11, Data Dictionary bagian 8"
contract_versions_s_d2: "v8 — API bagian 14, State bagian 12, Validation bagian 13, Permission bagian 12, Integration bagian 12, Data Dictionary bagian 9"
contract_versions_s_c4_onb: "v9 — API bagian 15, State bagian 13, Validation bagian 14, Permission bagian 13, Integration bagian 13, Data Dictionary bagian 10"
contract_versions_s_b4: "v10 — API bagian 16, State bagian 14, Validation bagian 15, Permission bagian 14, Integration bagian 7, Data Dictionary bagian 11, Acceptance Test bagian 18"
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
| `IN-03` | Perencanaan Tenaga Kerja (Workforce Planning) | `HRD-DEC-058` s.d. `HRD-DEC-072`; desain `S-D1` approved Pengguna 3 November 2026 | API v7 §13, State v7 §11, Validation v7 §12, Permission v7 §11, Integration v7 §11, Data v7 §8 | `BE-HRD-017` ✅, `BE-HRD-029` ✅, `BE-HRD-030` ✅, `BE-HRD-031` ✅, `BE-HRD-032` ✅, `BE-HRD-033` ✅, `BE-HRD-034` s.d. `BE-HRD-051` (`BE-HRD-045`, `BE-HRD-049`, `BE-HRD-050`, `BE-HRD-051` ⛔) | `FE-HRD-029` s.d. `FE-HRD-043` | Fondasi data BE-HRD-017 selesai; penelusuran Direktur BE-HRD-029 selesai; integrasi workflow BE-HRD-030 selesai; pengaturan rumah sakit BE-HRD-031 selesai; buku formasi berversi BE-HRD-032 selesai; batch formasi awal BE-HRD-033 selesai; isian angka beban manual BE-HRD-034 selesai. Rincian per requirement pada bagian 1a |
| `IN-04` | Rekrutmen, Seleksi Pelamar, dan Hiring | `HRD-DEC-073` s.d. `HRD-DEC-080`; desain `S-D2` approved Pengguna 3 November 2026 | API v8 §14, State v8 §12, Validation v8 §13, Permission v8 §12, Integration v8 §12, Data v8 §9 | `BE-HRD-018`, `BE-HRD-052` s.d. `BE-HRD-062` | `FE-HRD-044` s.d. `FE-HRD-057` | Fondasi skema & 12 model Hrd siap; endpoint Swagger 9 grup siap; 14 task FE siap dikerjakan. Rincian per requirement pada bagian 1b |
| `IN-05` | Onboarding, Evaluasi Probation, dan Exit Clearance Karyawan | `HRD-DEC-003`, `HRD-DEC-019` | API §Lifecycle, State §Separation | `BE-HRD-014` ✅ | `FE-HRD-017` ✅ | ✅ BE controller exit clearance & gerbang kliring Finance aktif; FE aktif ([Laporan FE](../task/report/frontend/FE-HRD-017.md)) |
| `IN-06` | Penjadwalan Shift Kerja 24/7, On-Call, dan Tukar Shift (Slice `S-B4`) | `HRD-DEC-006`, `HRD-DEC-026`, `HRD-DEC-097` s.d. `HRD-DEC-103`, `HRD-ASM-05` s.d. `HRD-ASM-14` | API v10 §16, State v10 §14, Validation v10 §15, Permission v10 §14, Integration v10 §7, Data v10 §11, Test v10 §18 | `BE-HRD-009` ✅, `BE-HRD-087` s.d. `BE-HRD-103` | `FE-HRD-012` ✅, `FE-HRD-075` s.d. `FE-HRD-087` | BE engine roster awal selesai; 17 task BE & 13 task FE baru direncanakan (MVP-18 s.d. MVP-20 & Verifikasi) menyerap validasi B1–B7/W1–W7, penerbitan mandiri, override langsung efektif, toleransi mundur 3 hari, dan keselamatan kontrak Lab. Rincian per requirement pada bagian 1d |
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
| `IN-02-OB` | Permohonan Perubahan Profil Mandiri & Kuasa HR (On-Behalf) | `HRD-DEC-047`, `052`, `091..094`, `FR-HRD-017` | API §EmployeeProfileChange, Validation §OnBehalf | `BE-HRD-028` ✅ | `FE-HRD-027` ✅, `FE-HRD-028` ✅ | ✅ Lengkap (04-11-2026 via PLAN-REPAIR-HRD-003): Layanan Mandiri selesai penuh (`FE-HRD-26`/`S-A5`), form Kuasa HR bebas 415 via perbaikan multipart `InstanceAxios.jsx` (`FIX-HRD-003-06`), Anti-Self-Approval aktif ([Laporan FIX-003](../docs/plan-repair/plan-repair-003-employee-self-service-profile-change.md)) |


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
| `FR-HRD-092` | Standar per unit berversi, satu versi berlaku per tanggal | BE §16.4.1; State §11.1 | `BE-HRD-035` ✅, `BE-HRD-036` | `FE-HRD-033`, `FE-HRD-034` | `AT-HRD-D1-04`, `UAT-29` | BE-035 selesai |
| `FR-HRD-093` | Tiga metode; Analisis Beban Kerja ditolak | BE §16.4.1; `VR-WFP-02` | `BE-HRD-035` ✅ | `FE-HRD-034` | `AT-HRD-D1-02b` | BE selesai |
| `FR-HRD-094` | Rekomendasi beserta rincian (ICU → 32) | BE §16.9 | `BE-HRD-035` ✅ | `FE-HRD-034` | `AT-HRD-D1-02`, `UAT-29` | BE selesai |
| `FR-HRD-095` | Standar hanya dipakai setelah `T9` | State §11.1 | `BE-HRD-035` ✅, `BE-HRD-036`, `BE-HRD-037` | `FE-HRD-034` | `AT-HRD-D1-03`, `AT-HRD-D1-04` | BE-035 selesai |
| `FR-HRD-096` | Angka beban beserta sumber; isian ganda ditolak | BE §16.4.2; `VR-WFP-08`, `VR-WFP-09` | `BE-HRD-034` ✅ | `FE-HRD-032` | Acceptance criteria `BE-HRD-034` — **tanpa baris `AT-HRD-D1`** | BE selesai |
| `FR-HRD-097` | Penjadwalan membaca standar berlaku | `INT-HR-13` | `BE-HRD-035` ✅ | — | `AT-HRD-D1-19` | BE selesai |
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
| `AT-HRD-D1-02`, `AT-HRD-D1-02b` | `BE-HRD-035` ✅, `FE-HRD-034` |
| `AT-HRD-D1-03` | `BE-HRD-035` ✅, `BE-HRD-037` |
| `AT-HRD-D1-04` | `BE-HRD-035` ✅, `BE-HRD-036`, `BE-HRD-051` |
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
| `AT-HRD-D1-19` | `BE-HRD-035` ✅, `BE-HRD-051` |
| `AT-HRD-D1-20` | `BE-HRD-029`, `BE-HRD-051` |
| `AT-HRD-D1-21` | `BE-HRD-046`, `BE-HRD-049` ⛔, `FE-HRD-042`, `BE-HRD-051` |
| `AT-HRD-D1-22` | `FE-HRD-029`, `FE-HRD-043` |

## 1d. Jejak `S-B4` Penjadwalan, Roster, dan On-Call (sub-modul 06) — requirement ke task dan bukti

Ditambahkan pada revision 8, 4 November 2026, sesudah desain `S-B4` disetujui pengguna. Seluruh
baris di bawah **belum punya bukti implementasi**: task-nya baru direncanakan. Kolom status
akan diperbarui oleh build skill saat task selesai.

| Field | Nilai |
| --- | --- |
| Approval desain | Pengguna, pemilik teknis dan produk (`HRD-DEC-015`), 4 November 2026, beserta jawaban interview `HRD-DEC-097` s.d. `HRD-DEC-103` dan asumsi `HRD-ASM-05` s.d. `HRD-ASM-14` — `02-backend-architecture.md` bagian 19, `03-frontend-architecture.md` bagian 15, kontrak `v10`, `04-prd-to-mvp.md` bagian 24, `data/data-dictionary.md` bagian 11, `testing/acceptance-test-matrix.md` bagian 18, empat flowchart `flowcharts/penjadwalan-kerja.md` |
| Keputusan | `HRD-DEC-097` s.d. `HRD-DEC-103`, `HRD-ASM-05` s.d. `HRD-ASM-14` (decision log revision `20`, bagian 36) |
| Kontrak | API `v10` bagian 16; State Transition `v10` bagian 14; Validation `v10` bagian 15; Permission `v10` bagian 14; Integration `v10` bagian 7; Data Dictionary `v10` bagian 11; Acceptance Test `v10` bagian 18 |
| Manifest | revision `19`; hash masukan terkunci pada metadata kedua roadmap |
| Source SHA | BE `8b17b75a` (branch `MHamzah`), FE `44990b3d3` (branch `HamzahV2`) |
| Task | Backend `BE-HRD-087` s.d. `BE-HRD-103` (17 task); frontend `FE-HRD-075` s.d. `FE-HRD-087` (13 task) |

### Functional requirement

| FR | Perilaku Bisnis | Desain & Kontrak | Task Backend | Task Frontend | Bukti yang Diharapkan | Status |
| --- | --- | --- | --- | --- | --- | :---: |
| `FR-SCH-01` | Daftar staf eligible unit terisi otomatis dari penempatan aktif di WorkforceCore | BE §19.4.1; API §16.1; `VR-SCH-01`; `INT-HR-36` | `BE-HRD-092`, `BE-HRD-094` | `FE-HRD-080`, `FE-HRD-081` | `AT-HRD-SCH-01` | |
| `FR-SCH-02` | Aksi Salin dari Periode Lalu menduplikasi shift bulan lalu ke draft baru | BE §19.4.2; API §16.2; `VR-SCH-02` | `BE-HRD-093` | `FE-HRD-078`, `FE-HRD-081` | `AT-HRD-SCH-02` | |
| `FR-SCH-03` | Aksi Terapkan Pola Shift massal (Pagi-Siang-Malam-Libur) per pegawai/grup | BE §19.4.3; API §16.3; `VR-SCH-03` | `BE-HRD-093` | `FE-HRD-078`, `FE-HRD-081` | `AT-HRD-SCH-03` | |
| `FR-SCH-04` | Kepala Unit menerbitkan roster mandiri tanpa approval sebelum terbit | BE §19.4.4; API §16.4; `HRD-DEC-098` | `BE-HRD-092` | `FE-HRD-081` | `AT-HRD-SCH-04`, `UAT-SCH-01` | |
| `FR-SCH-05` | Tombol Terbitkan wajib memicu validasi ulang menyeluruh sebelum status berubah Published | BE §19.4.4; API §16.4; `VR-SCH-04` | `BE-HRD-089`, `BE-HRD-092` | `FE-HRD-077`, `FE-HRD-081` | `AT-HRD-SCH-04`, `AT-HRD-SCH-05` | |
| `FR-SCH-06` | Kepala Bidang otomatis menerima notifikasi terbit dan hak pantau read-only | BE §19.4.5; API §16.5; `HRD-DEC-098` | `BE-HRD-094` | `FE-HRD-082` | `AT-HRD-SCH-06` | |
| `FR-SCH-07` | Blokir penerbitan B1–B6 (bentrok, cuti disetujui, beda unit, belum siap kerja, kontrak berakhir, status gagal baca) | BE §19.5; API §16.4; `VR-SCH-05` s.d. `VR-SCH-10`; `INT-HR-31` | `BE-HRD-089`, `BE-HRD-090` | `FE-HRD-077`, `FE-HRD-081` | `AT-HRD-SCH-07`, `AT-HRD-SCH-08` | |
| `FR-SCH-08` | Blokir penerbitan B7 bila staf Didampingi berdinas tanpa perawat Mandiri di ICU/IGD | BE §19.5; API §16.4; `VR-SCH-11`; `INT-HR-37`; `HRD-DEC-100` | `BE-HRD-089`, `BE-HRD-091` | `FE-HRD-077`, `FE-HRD-081` | `AT-HRD-SCH-09`, `UAT-SCH-02` | |
| `FR-SCH-09` | Peringatan W1–W3 (jeda < 11 jam, kerja > 6 hari, malam > 2) berdasarkan HrdSchedulingSetting | BE §19.3, §19.5; API §16.7; `VR-SCH-12` s.d. `VR-SCH-14` | `BE-HRD-088`, `BE-HRD-089` | `FE-HRD-077`, `FE-HRD-081` | `AT-HRD-SCH-10` | |
| `FR-SCH-10` | Peringatan W4–W5 (cuti pending & kredensial STR kedaluwarsa) | BE §19.5; API §16.4; `VR-SCH-15`, `VR-SCH-16`; `HRD-DEC-005` | `BE-HRD-089`, `BE-HRD-090` | `FE-HRD-077`, `FE-HRD-081` | `AT-HRD-SCH-11` | |
| `FR-SCH-11` | Peringatan W6–W7 (kuota tenaga & kualifikasi klinis) dengan pilihan alasan wajib sebelum terbit | BE §19.5; API §16.4; `VR-SCH-17`, `VR-SCH-18`; `HRD-DEC-099` | `BE-HRD-089`, `BE-HRD-091` | `FE-HRD-077`, `FE-HRD-081` | `AT-HRD-SCH-12`, `UAT-SCH-01` | |
| `FR-SCH-12` | Ganti Petugas pada shift terbit untuk menunjuk staf pengganti dengan alasan wajib | BE §19.6; API §16.6; `VR-SCH-19`; `HRD-DEC-101` | `BE-HRD-096` | `FE-HRD-079`, `FE-HRD-081` | `AT-HRD-SCH-13`, `UAT-SCH-03` | |
| `FR-SCH-13` | Penggantian shift langsung berstatus efektif di sistem tanpa approval Kepala Bidang | BE §19.6; API §16.6; `HRD-DEC-101` | `BE-HRD-096` | `FE-HRD-079`, `FE-HRD-081` | `AT-HRD-SCH-13` | |
| `FR-SCH-14` | Penggantian staf wajib lolos validasi keselamatan B1–B7 | BE §19.6; API §16.6; `VR-SCH-05` s.d. `VR-SCH-11` | `BE-HRD-089`, `BE-HRD-096` | `FE-HRD-079`, `FE-HRD-081` | `AT-HRD-SCH-14` | |
| `FR-SCH-15` | Roster periode tetap Published, terekam audit lengkap, dan memicu notifikasi mutasi | BE §19.6; API §16.6; State §14.2 | `BE-HRD-096` | `FE-HRD-079`, `FE-HRD-081` | `AT-HRD-SCH-13` | |
| `FR-SCH-16` | Koreksi mundur maksimal 3 hari ke belakang dengan alasan wajib | BE §19.7; API §16.8; `VR-SCH-20`; `HRD-DEC-102` | `BE-HRD-088`, `BE-HRD-097` | `FE-HRD-076`, `FE-HRD-081` | `AT-HRD-SCH-15`, `UAT-SCH-05` | |
| `FR-SCH-17` | Koreksi mundur otomatis memicu rekalkulasi kehadiran harian (modul 07) | BE §19.7; Integration §7.3 (`INT-HR-38`) | `BE-HRD-097` | `FE-HRD-081` | `AT-HRD-SCH-16` | |
| `FR-SCH-18` | Penguncian periode Closed saat cut-off payroll mengunci jadwal secara mutlak | BE §19.8; API §16.9; State §14.1; `VR-SCH-21` | `BE-HRD-098` | `FE-HRD-080`, `FE-HRD-081` | `AT-HRD-SCH-17` | |
| `FR-SCH-19` | Pemisahan wewenang penyusunan siaga staf (Kepala Ruangan) vs spesialis (Yanmed/KSM) | BE §19.9; API §16.10; Permission §14; `HRD-DEC-103` | `BE-HRD-099` | `FE-HRD-083`, `FE-HRD-085` | `AT-HRD-SCH-18` | |
| `FR-SCH-20` | Status siaga tetap Active saat dipanggil untuk melindungi kontrak LabConfirmingDoctorResolver | BE §19.9; State §14.3; Integration §7.4 (`INT-HR-39`) | `BE-HRD-099`, `BE-HRD-101` | `FE-HRD-085` | `AT-HRD-SCH-18` | |
| `FR-SCH-21` | Pencatatan pemanggilan (call-in) staf/dokter siaga beserta waktu panggil dan kedatangan | BE §19.10; API §16.11; `VR-SCH-21` | `BE-HRD-100` | `FE-HRD-084`, `FE-HRD-085` | `AT-HRD-SCH-19`, `UAT-SCH-04` | |
| `FR-SCH-22` | Kalkulasi selisih waktu respon dan kepatuhan terhadap target SLA menit | BE §19.10; API §16.11; `HRD-DEC-103` | `BE-HRD-100` | `FE-HRD-084`, `FE-HRD-085` | `AT-HRD-SCH-19` | |
| `FR-SCH-23` | Integrasi jam standby on-call ke Payroll dan transaksi panggilan cito ke Lembur & Payroll | BE §19.11; Integration §7.5 (`INT-HR-40`) | `BE-HRD-102` | `FE-HRD-085` | `AT-HRD-SCH-20` | |

### Matriks acceptance test ke task

| Acceptance test | Task | Cakupan Pengujian |
| --- | --- | --- |
| `AT-HRD-SCH-01` s.d. `AT-HRD-SCH-03` | `BE-HRD-092`, `BE-HRD-093`, `BE-HRD-094`, `FE-HRD-078`, `FE-HRD-080`, `FE-HRD-081` | Daftar eligible, salin periode lalu, dan pola shift massal |
| `AT-HRD-SCH-04` s.d. `AT-HRD-SCH-06` | `BE-HRD-089`, `BE-HRD-092`, `BE-HRD-094`, `FE-HRD-077`, `FE-HRD-081`, `FE-HRD-082` | Penerbitan mandiri, validasi pra-terbit, dan hak pantau read-only Kabid |
| `AT-HRD-SCH-07` s.d. `AT-HRD-SCH-09` | `BE-HRD-089`, `BE-HRD-090`, `BE-HRD-091`, `FE-HRD-077`, `FE-HRD-081` | Pemblokir fisik & hukum B1–B7 (bentrok, cuti, kesiapan, pendampingan) |
| `AT-HRD-SCH-10` s.d. `AT-HRD-SCH-12` | `BE-HRD-088`, `BE-HRD-089`, `BE-HRD-090`, `BE-HRD-091`, `FE-HRD-077`, `FE-HRD-081` | Peringatan beralasan W1–W7 (waktu kerja, STR habis, kuota/komposisi) |
| `AT-HRD-SCH-13` s.d. `AT-HRD-SCH-15` | `BE-HRD-088`, `BE-HRD-089`, `BE-HRD-096`, `BE-HRD-097`, `FE-HRD-076`, `FE-HRD-079`, `FE-HRD-081` | Ganti petugas langsung aktif (override), audit mutasi, dan koreksi mundur 3 hari |
| `AT-HRD-SCH-16`, `AT-HRD-SCH-17` | `BE-HRD-097`, `BE-HRD-098`, `FE-HRD-080`, `FE-HRD-081` | Pemicu rekalkulasi kehadiran dan penguncian mutlak periode Closed |
| `AT-HRD-SCH-18` s.d. `AT-HRD-SCH-20` | `BE-HRD-099`, `BE-HRD-100`, `BE-HRD-101`, `BE-HRD-102`, `FE-HRD-083`, `FE-HRD-084`, `FE-HRD-085` | Tata kelola siaga on-call, proteksi kontrak Lab, pencatatan call-in, dan kalkulasi SLA |
| `UAT-SCH-01` s.d. `UAT-SCH-05` | `BE-HRD-103`, `FE-HRD-087` | Validasi bisnis ujung-ke-ujung alur penjadwalan, roster, dan siaga |

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
| `FE-HRD-027` | Form Permohonan Kuasa HR | `EmployeeProfileChangeController.cs` | `src/app/hr/workforce-core/employee-profile-changes/create-on-behalf/**` | ✅ Selesai — kendala multipart 415 diselesaikan via perbaikan interceptor InstanceAxios (`FIX-HRD-003-06`) | [FE-HRD-027.md](../task/report/frontend/FE-HRD-027.md) |
| `FE-HRD-026-ESS` | Permohonan Perubahan Data Mandiri (Kelola Data Saya) | `EmployeeProfileChangeSelfServiceController.cs` | `src/app/self-services/human-resource/employee/profile-changes/**` | ✅ Selesai — diimplementasikan penuh (live diff, bukti sensitif, aksi tunggal, riwayat pengajuan) via PLAN-REPAIR-HRD-003 | [PLAN-REPAIR-003](../docs/plan-repair/plan-repair-003-employee-self-service-profile-change.md) |
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


---

## 1c. Jejak `S-C4-ONB` Onboarding dan Kontrak Kerja (sub-modul 05) — requirement ke task dan bukti

Ditambahkan pada revision 7, 4 November 2026, sesudah desain `S-C4-ONB` disetujui pemilik. Seluruh
baris di bawah **belum punya bukti implementasi**: task-nya baru direncanakan. Kolom status
diperbarui build skill saat task selesai.

| Field | Nilai |
| --- | --- |
| Approval desain | Pengguna, pemilik teknis dan produk (`HRD-DEC-015`), 4 November 2026, beserta lima pilihan desain (`HRD-DEC-096`) — `02-backend-architecture.md` bagian 18, `03-frontend-architecture.md` bagian 14, kontrak `v9`, `04-prd-to-mvp.md` bagian 23, `data/data-dictionary.md` bagian 10, `testing/acceptance-test-matrix.md` bagian 17, lima flowchart `S-C4-ONB` |
| Keputusan | `HRD-DEC-081` s.d. `HRD-DEC-090`, `HRD-DEC-095`, `HRD-DEC-096` (decision log revision `19`, bagian 34) |
| Kontrak | API `v9` bagian 15; State Transition `v9` bagian 13; Validation `v9` bagian 14; Permission `v9` bagian 13; Integration `v9` bagian 13; Data Dictionary `v9` bagian 10 |
| Manifest | revision `18`; hash masukan terkunci pada metadata kedua roadmap (`locked_input_hashes_s_c4_onb`) |
| Source SHA | BE `8b17b75a` (branch `MHamzah`), FE `44990b3d3` (branch `HamzahV2`) |
| Task | Backend `BE-HRD-063` s.d. `BE-HRD-086`; frontend `FE-HRD-058` s.d. `FE-HRD-074` |

### Functional requirement

| FR | Perilaku Bisnis | Desain & Kontrak | Task Backend | Task Frontend | Bukti yang Diharapkan | Status |
| --- | --- | --- | --- | --- | --- | :---: |
| `FR-ONB-001` | Serah terima membuat tepat satu kasus onboarding | BE §18.4.1; API §15.1; `VR-ONB-01`; `INT-HR-25` | `BE-HRD-074` | `FE-HRD-060` | `AT-HRD-ONB-01`, `AT-HRD-ONB-02`, `AT-HRD-ONB-03` | |
| `FR-ONB-002` | Checklist tiga lapis sesuai jenis tenaga dan unit | BE §18.4.2, §18.4.3; API §15.1, §15.2 | `BE-HRD-070`, `BE-HRD-074` | `FE-HRD-059`, `FE-HRD-060` | `AT-HRD-ONB-16` | |
| `FR-ONB-003` | Butir otomatis tidak dicentang tangan; butir wajib bukti menolak tanpa berkas | API §15.1; `VR-ONB-05`, `VR-ONB-06` | `BE-HRD-075` | `FE-HRD-061` | `AT-HRD-ONB-17` | |
| `FR-ONB-004` | Penanggung jawab butir dan cakupan unit | API §15.1; `VR-ONB-03`, `VR-ONB-04` | `BE-HRD-075` | `FE-HRD-061` | `AT-HRD-ONB-17`, `AT-HRD-ONB-18` | |
| `FR-ONB-005` | Batal bergabung hanya sebelum hadir hari pertama; kursi kembali terbuka | API §15.1; `VR-ONB-13`, `VR-ONB-14` | `BE-HRD-076` | `FE-HRD-062` | `AT-HRD-ONB-04`, `AT-HRD-ONB-05`, `UAT-ONB-02`, `UAT-ONB-03` | |
| `FR-ONB-006` | Perubahan template tidak mengubah kasus berjalan | BE §18.4.2 | `BE-HRD-070`, `BE-HRD-074` | `FE-HRD-059` | `AT-HRD-ONB-20` | |
| `FR-ONB-010` | Tawaran wajib memuat jenis dan lama kontrak | API §15.12; `VR-ONB-50`, `VR-ONB-51`; `INT-HR-35` | `BE-HRD-064` | `FE-HRD-072` | `AT-HRD-ONB-38`, `UAT-ONB-06` | |
| `FR-ONB-011` | Draf kontrak pertama menyalin isi tawaran | BE §18.9.1; API §15.3 | `BE-HRD-072` | `FE-HRD-063` | `AT-HRD-ONB-01`, `AT-HRD-ONB-07` | |
| `FR-ONB-012` | Kontrak sah hanya dengan scan bertanda tangan | API §15.3; `VR-ONB-30` | `BE-HRD-072` | `FE-HRD-063` | `AT-HRD-ONB-06`, `UAT-ONB-04` | |
| `FR-ONB-013` | Penandatanganan membuat penempatan dan gaji awal dalam satu transaksi | BE §18.9.6; `INT-HR-27` | `BE-HRD-067`, `BE-HRD-072` | `FE-HRD-063` | `AT-HRD-ONB-07`, `UAT-ONB-05` | |
| `FR-ONB-014` | Isi draf kontrak tidak dapat diubah langsung | API §15.3; `VR-ONB-31` | `BE-HRD-072` | `FE-HRD-063` | `AT-HRD-ONB-08` | |
| `FR-ONB-015` | Endpoint tulis riwayat kontrak 02 menjawab `409` | API §15.11; `VR-ONB-32` | `BE-HRD-065` | `FE-HRD-071` | `AT-HRD-ONB-39` | |
| `FR-ONB-020` | Siap kerja ditolak bila butir wajib belum selesai atau penyata sama dengan pemilih atasan | API §15.1; `VR-ONB-11`, `VR-ONB-12` | `BE-HRD-076` | `FE-HRD-062` | `AT-HRD-ONB-21`, `AT-HRD-ONB-22`, `UAT-ONB-07` | |
| `FR-ONB-021` | Siap kerja: status Aktif, relasi atasan, akses kerja diminta | State §13.1, §13.9 | `BE-HRD-067`, `BE-HRD-076` | `FE-HRD-062` | `AT-HRD-ONB-23`, `UAT-ONB-08` | |
| `FR-ONB-022` | Pegawai `PENDING_ONBOARDING` tidak dapat dijadwalkan | BE §18.9.7; `VR-ONB-47`; `INT-HR-31` | `BE-HRD-084` | — (layar 06) | `AT-HRD-ONB-24` | |
| `FR-ONB-023` | Akun dibuat saat kasus terbentuk, aktif untuk layanan mandiri sejak hadir | API §15.1, §15.7; `VR-ONB-75` | `BE-HRD-066`, `BE-HRD-074`, `BE-HRD-075`, `BE-HRD-077` | `FE-HRD-068` | `AT-HRD-ONB-25`, `AT-HRD-ONB-26`, `UAT-ONB-09` | |
| `FR-ONB-024` | Gaji periode pertama sejak tanggal mulai kontrak | BE §18.9.6; `INT-HR-33` | `BE-HRD-067`, `BE-HRD-072` | — | `AT-HRD-ONB-07` (penetapan gaji berlaku sejak tanggal mulai kontrak) | |
| `FR-ONB-030` | Masa percobaan hanya untuk PKWTT sejak bergabung | BE §18.9.3; State §13.4 | `BE-HRD-071` | `FE-HRD-065` | `AT-HRD-ONB-09`, `AT-HRD-ONB-10`, `AT-HRD-ONB-15`, `UAT-ONB-12` | |
| `FR-ONB-031` | Masa percobaan tepat 3 bulan tanpa perpanjangan | BE §18.9.3; `VR-ONB-59` | `BE-HRD-071`, `BE-HRD-079` | `FE-HRD-065` | `AT-HRD-ONB-10` | |
| `FR-ONB-032` | Tidak Lulus wajib lampiran | API §15.4; `VR-ONB-57` | `BE-HRD-079` | `FE-HRD-065` | `AT-HRD-ONB-12`, `UAT-ONB-10` | |
| `FR-ONB-033` | Lulus otomatis sehari sesudah tanggal akhir | State §13.4 | `BE-HRD-079`, `BE-HRD-083` | `FE-HRD-065` | `AT-HRD-ONB-13`, `UAT-ONB-11` | |
| `FR-ONB-034` | Tidak Lulus diserahkan ke 20 | `INT-HR-29` | `BE-HRD-078`, `BE-HRD-079` | — | `AT-HRD-ONB-14` | |
| `FR-ONB-040` | Tenaga klinis tidak siap kerja sebelum preceptor dipilih | BE §18.4.2 (butir sistem) | `BE-HRD-075`, `BE-HRD-076` | `FE-HRD-061` | `AT-HRD-ONB-21` | |
| `FR-ONB-041` | Pegawai Didampingi tetap dapat dijadwalkan dan berpenanda | BE §18.9.7; `INT-HR-31` | `BE-HRD-084` | — (layar 06) | `AT-HRD-ONB-28` | |
| `FR-ONB-042` | Siap Mandiri wajib lampiran tanpa persetujuan HRD | API §15.5; `VR-ONB-60`, `VR-ONB-61` | `BE-HRD-073` | `FE-HRD-066` | `AT-HRD-ONB-27`, `AT-HRD-ONB-28`, `UAT-ONB-13`, `UAT-ONB-14` | |
| `FR-ONB-043` | Peringatan kredensial tampil tetapi tidak menghalangi | BE §18.9.7 | `BE-HRD-074`, `BE-HRD-084` | `FE-HRD-060` | **Gap** — lihat bagian gap | ⛔ |
| `FR-ONB-050` | Pengingat 60 hari dan eskalasi 14 hari di Perlu Tindakan | BE §18.9.5; API §15.8 | `BE-HRD-084` | `FE-HRD-070` | `AT-HRD-ONB-36` (pengingat); eskalasi **gap** | |
| `FR-ONB-051` | Perpanjangan melewati 5 tahun ditolak | BE §18.9.2; `VR-ONB-36` | `BE-HRD-080` | `FE-HRD-064` | `AT-HRD-ONB-29`, `UAT-ONB-15` | |
| `FR-ONB-052` | Tidak Diperpanjang diserahkan ke 20 | `INT-HR-29` | `BE-HRD-078`, `BE-HRD-080` | `FE-HRD-064` | `AT-HRD-ONB-31` | |
| `FR-ONB-053` | PKWT lewat tanpa lanjutan bertanda tangan menahan jadwal | BE §18.9.4, §18.9.7; `VR-ONB-46` | `BE-HRD-083`, `BE-HRD-084` | — (layar 06) | `AT-HRD-ONB-32`, `UAT-ONB-16` | |
| `FR-ONB-054` | Koreksi kontrak oleh petugas HRD lain; versi lama tersimpan | API §15.3; `VR-ONB-41` s.d. `VR-ONB-44` | `BE-HRD-081` | `FE-HRD-064` | `AT-HRD-ONB-33`, `AT-HRD-ONB-34`, `UAT-ONB-18` | |
| `FR-ONB-060` | Batch dengan baris tidak sah tidak dapat diajukan; sekali per pegawai | API §15.6; `VR-ONB-65` s.d. `VR-ONB-70` | `BE-HRD-082` | `FE-HRD-067` | `AT-HRD-ONB-35`, `UAT-ONB-19` | |
| `FR-ONB-061` | Batch yang disetujui membuat kontrak data awal tanpa masa percobaan | State §13.8 | `BE-HRD-082` | `FE-HRD-067` | `AT-HRD-ONB-36`, `UAT-ONB-20` | |
| `FR-ONB-062` | Scan kontrak lama boleh menyusul | API §15.3; `VR-ONB-45` | `BE-HRD-082` | `FE-HRD-063` | `AT-HRD-ONB-36` | |
| `FR-ONB-063` | Migration pembersihan berhenti bila tabel kandidat berisi data | BE §18.7 | `BE-HRD-063` | — | `AT-HRD-ONB-37` | |

### Kebutuhan non-fungsional

| NFR | Kebutuhan | Task Backend | Bukti verifikasi |
| --- | --- | --- | --- |
| `NFR-ONB-001` | Transaksi tunggal untuk serah terima, tanda tangan, siap kerja, batal bergabung, penerapan keputusan | `BE-HRD-072`, `BE-HRD-074`, `BE-HRD-076`, `BE-HRD-079`, `BE-HRD-080` | Review diff batas transaksi; `AT-HRD-ONB-03` |
| `NFR-ONB-002` | Kunci advisory per pegawai | `BE-HRD-072`, `BE-HRD-083` | Review diff |
| `NFR-ONB-003` | `RowVersion` pada setiap dokumen | `BE-HRD-063` | Review configuration; `VR-ONB-15` lewat uji API |
| `NFR-ONB-004` | Scheduler idempoten | `BE-HRD-083` | Uji dua kali jalan pada tanggal yang sama |
| `NFR-ONB-005` | Kolom sensitif tidak masuk logger; berkas lewat endpoint terautentikasi | `BE-HRD-069`, `BE-HRD-072` | Review payload logger; `AT-HRD-ONB-40` |
| `NFR-ONB-006` | Pergantian hari menurut zona waktu rumah sakit | `BE-HRD-079`, `BE-HRD-083` | Uji `AT-HRD-ONB-13`, `AT-HRD-ONB-32` dengan tanggal uji |

### Keputusan bisnis

| Keputusan | Kebijakan yang Ditegakkan | Task Backend | Task Frontend | Bukti Verifikasi |
| --- | --- | --- | --- | --- |
| `HRD-DEC-081` | Satu pengaju, HRD memutuskan; satu langkah persetujuan | `BE-HRD-068` | `FE-HRD-073` | `AT-HRD-ONB-22`, `AT-HRD-ONB-33` |
| `HRD-DEC-082` | Keputusan kontrak dan masa percobaan di 05; proses keluar di 20 | `BE-HRD-078`, `BE-HRD-079`, `BE-HRD-080` | `FE-HRD-064`, `FE-HRD-065` | `AT-HRD-ONB-14`, `AT-HRD-ONB-31` |
| `HRD-DEC-083` | Kasus otomatis; batal bergabung tanpa persetujuan | `BE-HRD-074`, `BE-HRD-076` | `FE-HRD-060`, `FE-HRD-062` | `AT-HRD-ONB-01` s.d. `AT-HRD-ONB-05` |
| `HRD-DEC-084` | Kontrak pertama dari tawaran, sah dengan scan | `BE-HRD-072` | `FE-HRD-063` | `AT-HRD-ONB-06` s.d. `AT-HRD-ONB-08` |
| `HRD-DEC-085` | Masa percobaan PKWTT 3 bulan, Lulus/Tidak Lulus, lulus otomatis | `BE-HRD-071`, `BE-HRD-079` | `FE-HRD-065`, `FE-HRD-071` | `AT-HRD-ONB-09` s.d. `AT-HRD-ONB-15` |
| `HRD-DEC-086` | Checklist tiga lapis, tiga jenis butir | `BE-HRD-070`, `BE-HRD-074`, `BE-HRD-075` | `FE-HRD-059`, `FE-HRD-061` | `AT-HRD-ONB-16` s.d. `AT-HRD-ONB-20` |
| `HRD-DEC-087` | Pendampingan sesudah siap kerja; siap mandiri oleh Kepala Unit; kredensial peringatan | `BE-HRD-073`, `BE-HRD-084` | `FE-HRD-066` | `AT-HRD-ONB-27`, `AT-HRD-ONB-28` |
| `HRD-DEC-088` | Siap kerja oleh HRD; akun bertahap; gaji sejak mulai kontrak | `BE-HRD-066`, `BE-HRD-076`, `BE-HRD-077` | `FE-HRD-062`, `FE-HRD-068` | `AT-HRD-ONB-21` s.d. `AT-HRD-ONB-26` |
| `HRD-DEC-089` | Siklus kontrak, batas 5 tahun, penahan jadwal, koreksi | `BE-HRD-080`, `BE-HRD-081`, `BE-HRD-083`, `BE-HRD-084` | `FE-HRD-064`, `FE-HRD-070` | `AT-HRD-ONB-29` s.d. `AT-HRD-ONB-34` |
| `HRD-DEC-090` | Batch data awal; pengaman tabel kosong | `BE-HRD-063`, `BE-HRD-082` | `FE-HRD-067` | `AT-HRD-ONB-35` s.d. `AT-HRD-ONB-37` |
| `HRD-DEC-095` | Tawaran memuat jenis dan lama kontrak | `BE-HRD-064` | `FE-HRD-072` | `AT-HRD-ONB-38` |
| `HRD-DEC-096` | Lima pilihan desain turunan | `BE-HRD-065`, `BE-HRD-067`, `BE-HRD-075`, `BE-HRD-084` | `FE-HRD-070`, `FE-HRD-071` | `AT-HRD-ONB-07`, `AT-HRD-ONB-23`, `AT-HRD-ONB-39` |

### Acceptance test dan skenario UAT

| Ref Uji | Cakupan Skenario | Task Terkait | Dokumen Bukti yang Diharapkan |
| --- | --- | --- | --- |
| `AT-HRD-ONB-01` s.d. `AT-HRD-ONB-05` | Kasus otomatis, idempotensi, transaksi, batal bergabung | `BE-HRD-074`, `BE-HRD-076`, `FE-HRD-060`, `FE-HRD-062` | Respons API dan isi tabel kasus, kontrak, serah terima |
| `AT-HRD-ONB-06` s.d. `AT-HRD-ONB-08` | Kontrak pertama dan tanda tangan | `BE-HRD-072`, `FE-HRD-063` | Kontrak `Active`, penempatan dan gaji awal di dev |
| `AT-HRD-ONB-09` s.d. `AT-HRD-ONB-15` | Masa percobaan | `BE-HRD-065`, `BE-HRD-071`, `BE-HRD-079`, `BE-HRD-080`, `FE-HRD-065` | Status masa percobaan, pemisahan `ProbationFailed` |
| `AT-HRD-ONB-16` s.d. `AT-HRD-ONB-20` | Checklist dan template | `BE-HRD-070`, `BE-HRD-074`, `BE-HRD-075`, `FE-HRD-059`, `FE-HRD-061` | Butir per lapis; penolakan `VR-ONB` |
| `AT-HRD-ONB-21` s.d. `AT-HRD-ONB-26` | Siap kerja, kesiapan jadwal, akun | `BE-HRD-066`, `BE-HRD-076`, `BE-HRD-077`, `BE-HRD-084`, `FE-HRD-062`, `FE-HRD-068` | Status kepegawaian, akun, kesiapan |
| `AT-HRD-ONB-27`, `AT-HRD-ONB-28` | Pendampingan klinis | `BE-HRD-073`, `FE-HRD-066` | Status pendampingan; kotak masuk HRD kosong |
| `AT-HRD-ONB-29` s.d. `AT-HRD-ONB-34` | Keputusan, penahan jadwal, koreksi | `BE-HRD-080`, `BE-HRD-081`, `BE-HRD-083`, `BE-HRD-084`, `FE-HRD-064` | Kontrak lanjutan, pemisahan, status `LapsedWithoutDecision` |
| `AT-HRD-ONB-35` s.d. `AT-HRD-ONB-37` | Batch data awal dan pengaman migration | `BE-HRD-063`, `BE-HRD-082`, `FE-HRD-067` | Kontrak `InitialData`; migration berhenti |
| `AT-HRD-ONB-38` | Tawaran `S-D2` | `BE-HRD-064`, `FE-HRD-072` | Penolakan `VR-ONB-50`, `VR-ONB-51` |
| `AT-HRD-ONB-39` | Riwayat kontrak 02 baca saja | `BE-HRD-065`, `FE-HRD-071` | `409` beserta pesan |
| `AT-HRD-ONB-40` | Privasi gaji pada data cetak | `BE-HRD-072`, `FE-HRD-063` | `amountVisible = false` bagi Kepala Unit; audit pembacaan |
| `UAT-ONB-01` s.d. `UAT-ONB-20` | Validasi bisnis ujung-ke-ujung | `BE-HRD-086`, `FE-HRD-074` | Laporan uji dev runtime `S-C4-ONB` |

## 1d. Jejak `S-B4` Penjadwalan, Roster, dan On-Call (sub-modul 06) — requirement ke task dan bukti

Ditambahkan pada revision 8, 4 November 2026, sesudah desain `S-B4` disetujui pengguna. Seluruh
baris di bawah **belum punya bukti implementasi**: task-nya baru direncanakan. Kolom status
akan diperbarui oleh build skill saat task selesai.

| Field | Nilai |
| --- | --- |
| Approval desain | Pengguna, pemilik teknis dan produk (`HRD-DEC-015`), 4 November 2026, beserta jawaban interview `HRD-DEC-097` s.d. `HRD-DEC-103` dan asumsi `HRD-ASM-05` s.d. `HRD-ASM-14` — `02-backend-architecture.md` bagian 19, `03-frontend-architecture.md` bagian 15, kontrak `v10`, `04-prd-to-mvp.md` bagian 24, `data/data-dictionary.md` bagian 11, `testing/acceptance-test-matrix.md` bagian 18, empat flowchart `flowcharts/penjadwalan-kerja.md` |
| Keputusan | `HRD-DEC-097` s.d. `HRD-DEC-103`, `HRD-ASM-05` s.d. `HRD-ASM-14` (decision log revision `20`, bagian 36) |
| Kontrak | API `v10` bagian 16; State Transition `v10` bagian 14; Validation `v10` bagian 15; Permission `v10` bagian 14; Integration `v10` bagian 7; Data Dictionary `v10` bagian 11; Acceptance Test `v10` bagian 18 |
| Manifest | revision `19`; hash masukan terkunci pada metadata kedua roadmap |
| Source SHA | BE `8b17b75a` (branch `MHamzah`), FE `44990b3d3` (branch `HamzahV2`) |
| Task | Backend `BE-HRD-087` s.d. `BE-HRD-103` (17 task); frontend `FE-HRD-075` s.d. `FE-HRD-087` (13 task) |

### Functional requirement

| FR | Perilaku Bisnis | Desain & Kontrak | Task Backend | Task Frontend | Bukti yang Diharapkan | Status |
| --- | --- | --- | --- | --- | --- | :---: |
| `FR-SCH-01` | Daftar staf eligible unit terisi otomatis dari penempatan aktif di WorkforceCore | BE §19.4.1; API §16.1; `VR-SCH-01`; `INT-HR-36` | `BE-HRD-092`, `BE-HRD-094` | `FE-HRD-080`, `FE-HRD-081` | `AT-HRD-SCH-01` | |
| `FR-SCH-02` | Aksi Salin dari Periode Lalu menduplikasi shift bulan lalu ke draft baru | BE §19.4.2; API §16.2; `VR-SCH-02` | `BE-HRD-093` | `FE-HRD-078`, `FE-HRD-081` | `AT-HRD-SCH-02` | |
| `FR-SCH-03` | Aksi Terapkan Pola Shift massal (Pagi-Siang-Malam-Libur) per pegawai/grup | BE §19.4.3; API §16.3; `VR-SCH-03` | `BE-HRD-093` | `FE-HRD-078`, `FE-HRD-081` | `AT-HRD-SCH-03` | |
| `FR-SCH-04` | Kepala Unit menerbitkan roster mandiri tanpa approval sebelum terbit | BE §19.4.4; API §16.4; `HRD-DEC-098` | `BE-HRD-092` | `FE-HRD-081` | `AT-HRD-SCH-04`, `UAT-SCH-01` | |
| `FR-SCH-05` | Tombol Terbitkan wajib memicu validasi ulang menyeluruh sebelum status berubah Published | BE §19.4.4; API §16.4; `VR-SCH-04` | `BE-HRD-089`, `BE-HRD-092` | `FE-HRD-077`, `FE-HRD-081` | `AT-HRD-SCH-04`, `AT-HRD-SCH-05` | |
| `FR-SCH-06` | Kepala Bidang otomatis menerima notifikasi terbit dan hak pantau read-only | BE §19.4.5; API §16.5; `HRD-DEC-098` | `BE-HRD-094` | `FE-HRD-082` | `AT-HRD-SCH-06` | |
| `FR-SCH-07` | Blokir penerbitan B1–B6 (bentrok, cuti disetujui, beda unit, belum siap kerja, kontrak berakhir, status gagal baca) | BE §19.5; API §16.4; `VR-SCH-05` s.d. `VR-SCH-10`; `INT-HR-31` | `BE-HRD-089`, `BE-HRD-090` | `FE-HRD-077`, `FE-HRD-081` | `AT-HRD-SCH-07`, `AT-HRD-SCH-08` | |
| `FR-SCH-08` | Blokir penerbitan B7 bila staf Didampingi berdinas tanpa perawat Mandiri di ICU/IGD | BE §19.5; API §16.4; `VR-SCH-11`; `INT-HR-37`; `HRD-DEC-100` | `BE-HRD-089`, `BE-HRD-091` | `FE-HRD-077`, `FE-HRD-081` | `AT-HRD-SCH-09`, `UAT-SCH-02` | |
| `FR-SCH-09` | Peringatan W1–W3 (jeda < 11 jam, kerja > 6 hari, malam > 2) berdasarkan HrdSchedulingSetting | BE §19.3, §19.5; API §16.7; `VR-SCH-12` s.d. `VR-SCH-14` | `BE-HRD-088`, `BE-HRD-089` | `FE-HRD-077`, `FE-HRD-081` | `AT-HRD-SCH-10` | |
| `FR-SCH-10` | Peringatan W4–W5 (cuti pending & kredensial STR kedaluwarsa) | BE §19.5; API §16.4; `VR-SCH-15`, `VR-SCH-16`; `HRD-DEC-005` | `BE-HRD-089`, `BE-HRD-090` | `FE-HRD-077`, `FE-HRD-081` | `AT-HRD-SCH-11` | |
| `FR-SCH-11` | Peringatan W6–W7 (kuota tenaga & kualifikasi klinis) dengan pilihan alasan wajib sebelum terbit | BE §19.5; API §16.4; `VR-SCH-17`, `VR-SCH-18`; `HRD-DEC-099` | `BE-HRD-089`, `BE-HRD-091` | `FE-HRD-077`, `FE-HRD-081` | `AT-HRD-SCH-12`, `UAT-SCH-01` | |
| `FR-SCH-12` | Ganti Petugas pada shift terbit untuk menunjuk staf pengganti dengan alasan wajib | BE §19.6; API §16.6; `VR-SCH-19`; `HRD-DEC-101` | `BE-HRD-096` | `FE-HRD-079`, `FE-HRD-081` | `AT-HRD-SCH-13`, `UAT-SCH-03` | |
| `FR-SCH-13` | Penggantian shift langsung berstatus efektif di sistem tanpa approval Kepala Bidang | BE §19.6; API §16.6; `HRD-DEC-101` | `BE-HRD-096` | `FE-HRD-079`, `FE-HRD-081` | `AT-HRD-SCH-13` | |
| `FR-SCH-14` | Penggantian staf wajib lolos validasi keselamatan B1–B7 | BE §19.6; API §16.6; `VR-SCH-05` s.d. `VR-SCH-11` | `BE-HRD-089`, `BE-HRD-096` | `FE-HRD-079`, `FE-HRD-081` | `AT-HRD-SCH-14` | |
| `FR-SCH-15` | Roster periode tetap Published, terekam audit lengkap, dan memicu notifikasi mutasi | BE §19.6; API §16.6; State §14.2 | `BE-HRD-096` | `FE-HRD-079`, `FE-HRD-081` | `AT-HRD-SCH-13` | |
| `FR-SCH-16` | Koreksi mundur maksimal 3 hari ke belakang dengan alasan wajib | BE §19.7; API §16.8; `VR-SCH-20`; `HRD-DEC-102` | `BE-HRD-088`, `BE-HRD-097` | `FE-HRD-076`, `FE-HRD-081` | `AT-HRD-SCH-15`, `UAT-SCH-05` | |
| `FR-SCH-17` | Koreksi mundur otomatis memicu rekalkulasi kehadiran harian (modul 07) | BE §19.7; Integration §7.3 (`INT-HR-38`) | `BE-HRD-097` | `FE-HRD-081` | `AT-HRD-SCH-16` | |
| `FR-SCH-18` | Penguncian periode Closed saat cut-off payroll mengunci jadwal secara mutlak | BE §19.8; API §16.9; State §14.1; `VR-SCH-21` | `BE-HRD-098` | `FE-HRD-080`, `FE-HRD-081` | `AT-HRD-SCH-17` | |
| `FR-SCH-19` | Pemisahan wewenang penyusunan siaga staf (Kepala Ruangan) vs spesialis (Yanmed/KSM) | BE §19.9; API §16.10; Permission §14; `HRD-DEC-103` | `BE-HRD-099` | `FE-HRD-083`, `FE-HRD-085` | `AT-HRD-SCH-18` | |
| `FR-SCH-20` | Status siaga tetap Active saat dipanggil untuk melindungi kontrak LabConfirmingDoctorResolver | BE §19.9; State §14.3; Integration §7.4 (`INT-HR-39`) | `BE-HRD-099`, `BE-HRD-101` | `FE-HRD-085` | `AT-HRD-SCH-18` | |
| `FR-SCH-21` | Pencatatan pemanggilan (call-in) staf/dokter siaga beserta waktu panggil dan kedatangan | BE §19.10; API §16.11; `VR-SCH-21` | `BE-HRD-100` | `FE-HRD-084`, `FE-HRD-085` | `AT-HRD-SCH-19`, `UAT-SCH-04` | |
| `FR-SCH-22` | Kalkulasi selisih waktu respon dan kepatuhan terhadap target SLA menit | BE §19.10; API §16.11; `HRD-DEC-103` | `BE-HRD-100` | `FE-HRD-084`, `FE-HRD-085` | `AT-HRD-SCH-19` | |
| `FR-SCH-23` | Integrasi jam standby on-call ke Payroll dan transaksi panggilan cito ke Lembur & Payroll | BE §19.11; Integration §7.5 (`INT-HR-40`) | `BE-HRD-102` | `FE-HRD-085` | `AT-HRD-SCH-20` | |

### Matriks acceptance test ke task

| Acceptance test | Task | Cakupan Pengujian |
| --- | --- | --- |
| `AT-HRD-SCH-01` s.d. `AT-HRD-SCH-03` | `BE-HRD-092`, `BE-HRD-093`, `BE-HRD-094`, `FE-HRD-078`, `FE-HRD-080`, `FE-HRD-081` | Daftar eligible, salin periode lalu, dan pola shift massal |
| `AT-HRD-SCH-04` s.d. `AT-HRD-SCH-06` | `BE-HRD-089`, `BE-HRD-092`, `BE-HRD-094`, `FE-HRD-077`, `FE-HRD-081`, `FE-HRD-082` | Penerbitan mandiri, validasi pra-terbit, dan hak pantau read-only Kabid |
| `AT-HRD-SCH-07` s.d. `AT-HRD-SCH-09` | `BE-HRD-089`, `BE-HRD-090`, `BE-HRD-091`, `FE-HRD-077`, `FE-HRD-081` | Pemblokir fisik & hukum B1–B7 (bentrok, cuti, kesiapan, pendampingan) |
| `AT-HRD-SCH-10` s.d. `AT-HRD-SCH-12` | `BE-HRD-088`, `BE-HRD-089`, `BE-HRD-090`, `BE-HRD-091`, `FE-HRD-077`, `FE-HRD-081` | Peringatan beralasan W1–W7 (waktu kerja, STR habis, kuota/komposisi) |
| `AT-HRD-SCH-13` s.d. `AT-HRD-SCH-15` | `BE-HRD-088`, `BE-HRD-089`, `BE-HRD-096`, `BE-HRD-097`, `FE-HRD-076`, `FE-HRD-079`, `FE-HRD-081` | Ganti petugas langsung aktif (override), audit mutasi, dan koreksi mundur 3 hari |
| `AT-HRD-SCH-16`, `AT-HRD-SCH-17` | `BE-HRD-097`, `BE-HRD-098`, `FE-HRD-080`, `FE-HRD-081` | Pemicu rekalkulasi kehadiran dan penguncian mutlak periode Closed |
| `AT-HRD-SCH-18` s.d. `AT-HRD-SCH-20` | `BE-HRD-099`, `BE-HRD-100`, `BE-HRD-101`, `BE-HRD-102`, `FE-HRD-083`, `FE-HRD-084`, `FE-HRD-085` | Tata kelola siaga on-call, proteksi kontrak Lab, pencatatan call-in, dan kalkulasi SLA |
| `UAT-SCH-01` s.d. `UAT-SCH-05` | `BE-HRD-103`, `FE-HRD-087` | Validasi bisnis ujung-ke-ujung alur penjadwalan, roster, dan siaga |

### Gap keterkaitan requirement ke bukti verifikasi

| Gap | Apa yang belum ada | Pemilik | Yang tetap bisa jalan |
| --- | --- | --- | --- |
| `FR-ONB-043` tanpa baris acceptance test | `testing/acceptance-test-matrix.md` bagian 17 tidak punya baris yang membuktikan peringatan kredensial tampil **dan** tidak menahan siap kerja maupun jadwal. Kriteria tercakup acceptance criteria `BE-HRD-084` butir 2 dan `FE-HRD-060` butir 2, tetapi belum punya ID uji | `design-business-module` — tambah satu baris `AT-HRD-ONB` pada revisi matriks uji berikutnya | Seluruh task |
| Kategori eskalasi `FR-ONB-050` tanpa baris acceptance test | Pengingat 60 hari terbukti lewat `AT-HRD-ONB-36`; eskalasi 14 hari kepada HR Manager belum punya ID uji | Sama dengan di atas | Seluruh task |
| Penahan jadwal di Penjadwalan 06 (`INT-HR-31`) | Ditutup pada perencanaan `S-B4` (roadmap revision 8): `BE-HRD-090` mengintegrasikan pemanggilan `HrdEmploymentReadinessReadService` dan diuji via `AT-HRD-SCH-08` | Desain & Task `S-B4` selesai | `BE-HRD-084` dan `BE-HRD-090` siap diintegrasikan |
| `HRD-DEC-005` dan `HRD-DEC-010` | Aturan blokir kredensial dan pengesahan tampilan catatan pembatasan kerja | Komite Medik/Keperawatan; K3RS | Peringatan kredensial dan kesimpulan kelaikan tanpa diagnosis |
| Pemetaan peran `HRD-DEC-032` | Rantai `T15` s.d. `T18` tidak menemukan penyetuju sampai peran dipetakan | Administrator | Seluruh task build; menahan `BE-HRD-085` dan `BE-HRD-086` |

**Yang bukan gap.** Backend tidak memelihara proyek automated test (`rules/backend/TEST_POLICY.md`);
ketiadaan test unit atau integration backend untuk task `S-C4-ONB` **bukan** gap. Bukti backend adalah
QBE preflight, review diff, `dotnet build`, verifikasi kontrak API, dan verifikasi proses bisnis pada
baris di atas. Untuk frontend, test unit baru opsional (`rules/frontend/test-policy.md`).
