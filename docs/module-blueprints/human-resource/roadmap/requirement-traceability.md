# Requirement Traceability — Modul Human Resource

## Metadata

```yaml
module_id: human-resource
blueprint_id: HRD-BP-001
blueprint_shape: SINGLE
roadmap_revision: 14
status: ACTIVE
status_synced_at: "11 November 2026 — Gelombang 1 perbaikan frontend: FE-HRD-R01 s.d. R13 dikerjakan (bagian 1v); S-D1 dan S-B1 diberi tanda status per task frontend. Sebelumnya: 11 November 2026 — Gelombang 2 audit backend: 7 keputusan diambil sebagai asumsi eksplisit, 19 task dikerjakan ulang (bagian 1u), migration sudah dijalankan, semuanya 🟡 menunggu uji penerimaan. Sebelumnya: 10 November 2026 — Gelombang 1 audit backend: 25 task dikerjakan ulang (bagian 1t), 3 task verifikasi diturunkan ke 🟡. Sebelumnya: 2026-11-03 — PLAN-REPAIR-HRD-002: FE-HRD-026 dan FE-HRD-027 dikoreksi ke 🟡; bukti pengerjaan ulang FE-HRD-022, FE-HRD-024, FE-HRD-025 ditautkan. Sebelumnya (2026-11-02): implementasi end-to-end Slice S-A1"
planning_updated_at: "2026-11-06 - jejak S-C1-OPPE Evaluasi Praktik Profesional Berkelanjutan (OPPE), Penugasan Terfokus (FPPE) & Rapor Komposit Rekredensialing 3 Tahunan ditambahkan (bagian 1m): BE-HRD-222 s.d. BE-HRD-237, FE-HRD-201 s.d. FE-HRD-215; sebelumnya S-C1 (bagian 1l), S-D4 (bagian 1k), S-D3 (bagian 1j), S-B6 (bagian 1i), S-B5 (bagian 1h), S-B3 (bagian 1g), S-B2 (bagian 1f), S-B1 (bagian 1e), S-B4 (bagian 1d) dan S-C4-ONB (bagian 1c)"
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
contract_versions_s_b1: "v11 — API bagian 17, State bagian 15, Validation bagian 16, Permission bagian 15, Integration bagian 8, Data Dictionary bagian 12, Acceptance Test bagian 19"
contract_versions_s_b2: "v12 — API bagian 18, State bagian 16, Validation bagian 17, Permission bagian 16, Integration bagian 9, Data Dictionary bagian 13, Acceptance Test bagian 20"
contract_versions_s_b3: "v13 — API bagian 19, State bagian 17, Validation bagian 18, Permission bagian 17, Integration bagian 10, Data Dictionary bagian 14, Acceptance Test bagian 21"
contract_versions_s_b5: "v14 — API bagian 20, State bagian 18, Validation bagian 19, Permission bagian 18, Integration bagian 11, Data Dictionary bagian 15, Acceptance Test bagian 22"
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
| `IN-07` | Presensi Harian, Penutupan Periode, dan Koreksi Kehadiran (Slice `S-B1`) | `HRD-DEC-028`, `HRD-DEC-029`, `HRD-DEC-104` s.d. `HRD-DEC-109`, `HRD-ASM-15` s.d. `HRD-ASM-18` | API v11 §17, State v11 §15, Validation v11 §16, Permission v11 §15, Integration v11 §8, Data v11 §12, Test v11 §19 | `BE-HRD-006` ✅, `BE-HRD-104` s.d. `BE-HRD-116` | `FE-HRD-009` ✅, `FE-HRD-088` s.d. `FE-HRD-101` | BE controller & guard lama aktif; 13 task BE & 14 task FE baru direncanakan (MVP-21 s.d. MVP-23 & Verifikasi) menyerap normalisasi shift malam, toleransi 10m, auto-convert incomplete punch, izin pulang cepat non-deductible, buka kembali darurat HR Manager ber-rollback payroll, dan dinas luar OnDuty. Rincian per requirement pada bagian 1e |
| `IN-08` | Administrasi Cuti, Izin Staf RS, Saldo Ledger & Recall (Slice `S-B2`) | `HRD-DEC-110` s.d. `HRD-DEC-115`, `HRD-ASM-19` s.d. `HRD-ASM-22`; desain `S-B2` approved Pengguna 4 November 2026 | API v12 §18, State v12 §16, Validation v12 §17, Permission v12 §16, Integration v12 §9, Data v12 §13, Acceptance Test v12 §20 | `BE-HRD-007` ✅, `BE-HRD-117` s.d. `BE-HRD-130` | `FE-HRD-010` ✅, `FE-HRD-102` s.d. `FE-HRD-115` | V1 operasional aktif. Target S-B2 direncanakan 14 task BE dan 14 task FE: buku besar mutasi saldo (`TrxLeaveBalanceLedger`), carry-forward max 6 hari s.d. 30 Juni, evaluasi Safe Staffing Coverage Warning, cuti sakit wajib dokter ber-SIP dengan privasi K3RS, leave recall darurat berotorisasi HR Manager dengan restore saldo, penutupan celah /reverse HTTP 409, dan rekonsiliasi retrospektif payroll. Rincian per requirement pada bagian 1f |
| `IN-09` | Administrasi Lembur, Verifikasi SPKL, dan Kompensasi (Slice `S-B3`) | `HRD-DEC-116` s.d. `HRD-DEC-121`, `HRD-ASM-23` s.d. `HRD-ASM-26`; desain `S-B3` approved Pengguna 5 November 2026 | API v13 §19, State v13 §17, Validation v13 §18, Permission v13 §17, Integration v13 §10, Data v13 §14, Acceptance Test v13 §21 | `BE-HRD-008` ✅, `BE-HRD-131` s.d. `BE-HRD-145` | `FE-HRD-011` ✅, `FE-HRD-116` s.d. `FE-HRD-129` | V1 operasional aktif. Target S-B3 direncanakan 15 task BE dan 14 task FE: model dual pengajuan (SPKL terencana H-1 & klaim darurat medis cito H+1 ber-RM), proteksi anti-self-approval mutlak, eskalasi darurat klinis Direktur Medik (kuota PP 35/2021 >4 jam/hari atau >18 jam/minggu), verifikasi silang mesin presensi biometrik dengan batas minimum 30 menit & investigasi bukti logbook fisik, hak cuti pengganti lembur (CTO) 90 hari kalender dengan scheduler hangus, dan rekonsiliasi rapel retrospektif payroll pasca-cut-off tgl 20. Rincian per requirement pada bagian 1g |
| `IN-10` | Perhitungan Payroll HR, Komponen Gaji, Angsuran, dan Restitusi (Slice `S-B5`) | `HRD-DEC-009`, `HRD-DEC-122` s.d. `HRD-DEC-127`, `HRD-ASM-27` s.d. `HRD-ASM-30` | API v14 § 20, State v14 § 18, Validation v14 § 19, Permission v14 § 18, Integration v14 § 11 | `BE-HRD-010` ?, `BE-HRD-146` s.d. `BE-HRD-160` | `FE-HRD-013` ?, `FE-HRD-130` s.d. `FE-HRD-143` | ✅ S-B5 FE Selesai (14 task). Precondition gate cut-off tgl 20, kalkulasi komponen bruto/neto, variansi alert >20%, alur otorisasi 3-jenjang anti-self-approval, handoff Finance SHA-256, dan slip gaji ESS PIN 6-digit. Laporan: [FE-HRD-130.md](../task/report/frontend/FE-HRD-130.md) s.d. [FE-HRD-143.md](../task/report/frontend/FE-HRD-143.md) |
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
| `FR-HRD-092` | Standar per unit berversi, satu versi berlaku per tanggal | BE §16.4.1; State §11.1 | `BE-HRD-035` ✅, `BE-HRD-036` | `FE-HRD-033`, `FE-HRD-034` | `AT-HRD-D1-04`, `UAT-29` | 🟡 FE source selesai; [R26](../task/report/frontend/FE-HRD-R26.md) |
| `FR-HRD-093` | Tiga metode; Analisis Beban Kerja ditolak | BE §16.4.1; `VR-WFP-02` | `BE-HRD-035` ✅ | `FE-HRD-034` | `AT-HRD-D1-02b` | BE selesai |
| `FR-HRD-094` | Rekomendasi beserta rincian (ICU → 32) | BE §16.9 | `BE-HRD-035` ✅ | `FE-HRD-034` | `AT-HRD-D1-02`, `UAT-29` | BE selesai |
| `FR-HRD-095` | Standar hanya dipakai setelah `T9` | State §11.1 | `BE-HRD-035` ✅, `BE-HRD-036`, `BE-HRD-037` | `FE-HRD-034` | `AT-HRD-D1-03`, `AT-HRD-D1-04` | BE-035 selesai |
| `FR-HRD-096` | Angka beban beserta sumber; isian ganda ditolak | BE §16.4.2; `VR-WFP-08`, `VR-WFP-09` | `BE-HRD-034` ✅ | `FE-HRD-032` | Acceptance criteria `BE-HRD-034` — **tanpa baris `AT-HRD-D1`** | 🟡 FE source selesai; [R26](../task/report/frontend/FE-HRD-R26.md) |
| `FR-HRD-097` | Penjadwalan membaca standar berlaku | `INT-HR-13` | `BE-HRD-035` ✅ | — | `AT-HRD-D1-19` | BE selesai |
| `FR-HRD-098` | Formasi tidak dapat diubah; method tulis `405` | BE §16.4.3; `VR-WFP-10` | `BE-HRD-032` | `FE-HRD-030` 🟡 | `AT-HRD-D1-06` | FE [030](../task/report/frontend/FE-HRD-030.md) 🟡 (11 Nov 2026) |
| `FR-HRD-099` | Setiap versi menyimpan dokumen sumber, penyetuju, waktu | BE §16.4.3 | `BE-HRD-032`, `BE-HRD-033`, `BE-HRD-041`, `BE-HRD-044`, `BE-HRD-047` | `FE-HRD-030` 🟡 | `AT-HRD-D1-17` | FE [030](../task/report/frontend/FE-HRD-030.md) 🟡 (11 Nov 2026) |
| `FR-HRD-100` | Versi terakhir berlaku sampai digantikan | `FORMATION_CARRIES_FORWARD` | `BE-HRD-032` | `FE-HRD-030` 🟡 | `AT-HRD-D1-07` | FE [030](../task/report/frontend/FE-HRD-030.md) 🟡 (11 Nov 2026) |
| `FR-HRD-101` | Batch formasi awal: scan wajib, tanpa baris gagal, `T10` oleh orang lain, sekali per unit × jabatan | BE §16.4.4; State §11.2 | `BE-HRD-033` | `FE-HRD-031` 🟡 | `AT-HRD-D1-17`, `UAT-31`, `UAT-32` | FE [031](../task/report/frontend/FE-HRD-031.md) 🟡 (11 Nov 2026) |
| `FR-HRD-102` | Gap per tanggal acuan dalam porsi kerja, hanya penempatan utama | BE §16.9 | `BE-HRD-037` | `FE-HRD-035` | `AT-HRD-D1-05`, `UAT-33` | 🟡 FE source selesai; [R26](../task/report/frontend/FE-HRD-R26.md) |
| `FR-HRD-103` | Tiga gap: permanen, sementara, kompetensi | BE §16.9 | `BE-HRD-037` | `FE-HRD-035` | `UAT-33` | 🟡 FE source selesai; [R26](../task/report/frontend/FE-HRD-R26.md) |
| `FR-HRD-104` | Data belum ada tampil "data belum tersedia", bukan 0 | `UNKNOWN_IS_NOT_ZERO` | `BE-HRD-037`, `BE-HRD-038`, `BE-HRD-039` | `FE-HRD-029` ✅, `FE-HRD-035`, `FE-HRD-040`, `FE-HRD-043` | `AT-HRD-D1-05b`, `UAT-33` | 🟡 `FE-HRD-035` mempertahankan `null`; [R26](../task/report/frontend/FE-HRD-R26.md) |
| `FR-HRD-105` | Potret saat diajukan dan akhir bulan; tidak dapat diubah; bulan terlewat tidak diisi diam-diam | BE §16.4.7; State §11.7 | `BE-HRD-039`, `BE-HRD-048` | `FE-HRD-040` | `AT-HRD-D1-16`, `AT-HRD-D1-16b`, `UAT-34` | |
| `FR-HRD-106` | Satu rencana tahunan per tahun; `Approved` terkunci | State §11.3 | `BE-HRD-046` | `FE-HRD-041`, `FE-HRD-042` | `AT-HRD-D1-15`, `UAT-36` | |
| `FR-HRD-107` | Addendum dari rencana `Approved`, baris berubah, `T10` penuh | State §11.3 | `BE-HRD-046`, `BE-HRD-047` | `FE-HRD-042` | `AT-HRD-D1-15` | |
| `FR-HRD-108` | Usulan berbeda dari rekomendasi wajib beralasan | `VR-WFP-19` | `BE-HRD-046` | `FE-HRD-042` | Acceptance criteria `BE-HRD-046` — **tanpa baris `AT-HRD-D1`** | |
| `FR-HRD-109` | Biaya = porsi × gaji tengah grade × bulan, berlabel estimasi | BE §16.4.9 | `BE-HRD-038`, `BE-HRD-046` | `FE-HRD-042` | `AT-HRD-D1-11` | |
| `FR-HRD-110` | Finance mengoreksi total pada langkahnya; angka awal tersimpan | `FINANCE_ADJUSTMENT` | `BE-HRD-040`, `BE-HRD-046`, `BE-HRD-049` (`VR-WFP-21`) ✅ | `FE-HRD-037`, `FE-HRD-042` | `AT-HRD-D1-21`, `UAT-35` | ✅ Terealisasi penuh |
| `FR-HRD-111` | Tanpa `WorkforcePlanningCost : Read` tidak menerima biaya | `VR-WFP-37` | `BE-HRD-038` | `FE-HRD-029` ✅, `FE-HRD-037`, `FE-HRD-039`, `FE-HRD-041`, `FE-HRD-042`, `FE-HRD-043` | `AT-HRD-D1-11` | FE [029](../task/report/frontend/FE-HRD-029.md) ✅; task FE lain belum (11 Nov 2026) |
| `FR-HRD-112` | Persetujuan rencana melahirkan formasi dan menandai permintaan berlebih | State §11.3, §11.8 | `BE-HRD-047` | `FE-HRD-038` (keterangan kursi beku) | `AT-HRD-D1-15`, `UAT-35` | |
| `FR-HRD-113` | Permintaan dalam formasi ≤ sisa kursi, diperiksa ulang pada persetujuan terakhir | `REMAINING_SEATS`, `RECHECK_ON_FINAL_APPROVAL` | `BE-HRD-040`, `BE-HRD-049` (`VR-WFP-23`) ✅ | `FE-HRD-037` | `AT-HRD-D1-08`, `AT-HRD-D1-08b`, `UAT-38` | ✅ Terealisasi penuh |
| `FR-HRD-114` | Dua permintaan bersamaan tidak mengklaim kursi yang sama | `NO_DOUBLE_CLAIM_ON_SEATS`, `NFR-010` | `BE-HRD-040` | — | `AT-HRD-D1-09` | |
| `FR-HRD-115` | `T11` untuk dalam formasi dan sementara; `T12` untuk di luar formasi | `HRD-DEC-064` | `BE-HRD-040`, `BE-HRD-041` | `FE-HRD-037` | `AT-HRD-D1-10`, `UAT-37` | |
| `FR-HRD-116` | Direktur dari matriks lewat unit induk; kosong ke Direktur Utama; keduanya kosong ditolak | `HRD-DEC-072` | `BE-HRD-029` | — | `AT-HRD-D1-20` | |
| `FR-HRD-117` | Kursi terisi saat penempatan cocok berlaku; satu penempatan satu kursi | `HRD-DEC-066` | `BE-HRD-043` | `FE-HRD-038`, `FE-HRD-039` | `AT-HRD-D1-12`, `AT-HRD-D1-13`, `UAT-39` | |
| `FR-HRD-118` | Kursi luar di antrean Recruiter; kursi internal lewat SK | `HRD-DEC-066`, `INT-HR-17` | `BE-HRD-042`, `BE-HRD-043` | `FE-HRD-039` | `AT-HRD-D1-14` | |
| `FR-HRD-119` | HR Manager bukan pembuat mengubah sumber kursi yang belum terisi, beralasan | `VR-WFP-33` | `BE-HRD-042` | `FE-HRD-038` | `AT-HRD-D1-14` | |
| `FR-HRD-120` | Pembatalan hanya kursi belum terisi, setelah lowongan ditutup, lewat `T13`; formasi `T12` ikut turun | `HRD-DEC-071` | `BE-HRD-044`, `BE-HRD-045` (`VR-WFP-36`) ✅ | `FE-HRD-038` | `AT-HRD-D1-18`, `UAT-39`, `UAT-40` | ✅ Terealisasi penuh |
| `FR-HRD-121` | Permintaan lewat batas ditandai, tanpa pembatalan otomatis | `NO_AUTO_EXPIRY` | `BE-HRD-043` | `FE-HRD-036`, `FE-HRD-037` | Acceptance criteria `BE-HRD-043` — **tanpa baris `AT-HRD-D1`** | |

### Kebutuhan non-fungsional

| NFR | Task yang menegakkan | Bukti |
| --- | --- | --- |
| `NFR-010` advisory lock unit × jabatan | `BE-HRD-032`, `BE-HRD-040` | `AT-HRD-D1-09` |
| `NFR-011` persetujuan yang melahirkan formasi atomik | `BE-HRD-033`, `BE-HRD-041`, `BE-HRD-044`, `BE-HRD-047` | Review jalur penerap; `BE-HRD-051` |
| `NFR-012` `RowVersion` → `409` | `BE-HRD-017` dan setiap service dokumen; `FE-HRD-029` ✅ (pesan `409`) | Uji API per task |
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
| `AT-HRD-D1-06` | `BE-HRD-032`, `FE-HRD-030` 🟡, `FE-HRD-043` |
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
| `AT-HRD-D1-17` | `BE-HRD-033`, `FE-HRD-031` 🟡, `BE-HRD-051` |
| `AT-HRD-D1-18` | `BE-HRD-044`, `BE-HRD-045` ⛔, `FE-HRD-038`, `BE-HRD-051` |
| `AT-HRD-D1-19` | `BE-HRD-035` ✅, `BE-HRD-051` |
| `AT-HRD-D1-20` | `BE-HRD-029`, `BE-HRD-051` |
| `AT-HRD-D1-21` | `BE-HRD-046`, `BE-HRD-049` ⛔, `FE-HRD-042`, `BE-HRD-051` |
| `AT-HRD-D1-22` | `FE-HRD-029` ✅, `FE-HRD-043` |

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
| `HRD-PLAN-GAP-01` — sumber data "lowongan terbuka" untuk `VR-WFP-36` | Diselesaikan penuh via integrasi HrdJobVacancy published check di `BE-HRD-045` ✅ | ✅ Selesai |
| `HRD-PLAN-GAP-02` — kait validasi pra-persetujuan `VR-WFP-21` & `VR-WFP-23` | Diselesaikan penuh di HrdManpowerPlanDecisionApplier & HrdHeadcountRequestDecisionApplier via `BE-HRD-049` ✅ | ✅ Selesai |
| Pemetaan peran `HRD-DEC-032` | Diselesaikan via seeder runtime dev WorkforcePlanningRuntimeDevSeeder & OnboardingRuntimeDevSeeder (`BE-HRD-050`, `BE-HRD-085`) ✅ | ✅ Selesai |
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
| `FE-HRD-044` | Layar Antrean & Permintaan Requisition (`/hr/recruitment/requisitions`) | `HrdRequisitionController.cs` | `src/app/hr/recruitment/requisitions/**` | ✅ Lulus Linter & Build | [FE-HRD-044.md](../task/report/frontend/FE-HRD-044.md) |
| `FE-HRD-045` | Layar Detail & Draf Requisition (`/hr/recruitment/requisitions/[id]`) | `HrdRequisitionController.cs` | `src/app/hr/recruitment/requisitions/[id]/**` | ✅ Lulus Linter & Build | [FE-HRD-045.md](../task/report/frontend/FE-HRD-045.md) |
| `FE-HRD-046` | Layar Pengelolaan Lowongan Kerja (`/hr/recruitment/vacancies`) | `HrdJobVacancyController.cs` | `src/app/hr/recruitment/vacancies/**` | ✅ Lulus Linter & Build | [FE-HRD-046.md](../task/report/frontend/FE-HRD-046.md) |
| `FE-HRD-047` | Layar Detail Lowongan & Pelamar Masuk (`/hr/recruitment/vacancies/[id]`) | `HrdJobVacancyController.cs` | `src/app/hr/recruitment/vacancies/[id]/**` | ✅ Lulus Linter & Build | [FE-HRD-047.md](../task/report/frontend/FE-HRD-047.md) |
| `FE-HRD-048` | Layar Direktori Data Pelamar & NIK Rehire Flow (`/hr/recruitment/candidates`) | `HrdCandidateController.cs` | `src/app/hr/recruitment/candidates/**` | ✅ Lulus Linter & Build | [FE-HRD-048.md](../task/report/frontend/FE-HRD-048.md) |
| `FE-HRD-049` | Layar Detail Pelamar & Penapisan STR Klinis Otomatis (`/hr/recruitment/candidates/[id]`) | `HrdCandidateController.cs` | `src/app/hr/recruitment/candidates/[id]/**` | ✅ Lulus Linter & Build | [FE-HRD-049.md](../task/report/frontend/FE-HRD-049.md) |
| `FE-HRD-050` | Layar Formulir Evaluasi Wawancara HR (`/hr/recruitment/evaluations/hr/[id]`) | `HrdInterviewEvaluationController.cs` | `src/app/hr/recruitment/evaluations/hr/[id]/**` | ✅ Lulus Linter & Build | [FE-HRD-050.md](../task/report/frontend/FE-HRD-050.md) |
| `FE-HRD-051` | Formulir Uji Klinis Unit dengan Hak Veto Klinis (`/hr/recruitment/evaluations/clinical/[id]`) | `HrdClinicalAssessmentController.cs` | `src/app/hr/recruitment/evaluations/clinical/[id]/**` | ✅ Lulus Linter & Build | [FE-HRD-051.md](../task/report/frontend/FE-HRD-051.md) |
| `FE-HRD-052` | Layar Daftar Penawaran Kerja (*Job Offers*) (`/hr/recruitment/offers`) | `HrdJobOfferController.cs` | `src/app/hr/recruitment/offers/**` | ✅ Lulus Linter & Build | [FE-HRD-052.md](../task/report/frontend/FE-HRD-052.md) |
| `FE-HRD-053` | Detail Penawaran & Persetujuan Bertingkat T13/T14 (`/hr/recruitment/offers/[id]`) | `HrdJobOfferController.cs` | `src/app/hr/recruitment/offers/[id]/**` | ✅ Lulus Linter & Build | [FE-HRD-053.md](../task/report/frontend/FE-HRD-053.md) |
| `FE-HRD-054` | Pemantauan MCU Pra-Kerja & Masking Medis K3RS (`/hr/recruitment/mcu-clearance`) | `HrdMcuClearanceController.cs` | `src/app/hr/recruitment/mcu-clearance/**` | ✅ Lulus Linter & Build | [FE-HRD-054.md](../task/report/frontend/FE-HRD-054.md) |
| `FE-HRD-055` | Bank Cadangan (*Talent Pool*) & Fast-Track (`/hr/recruitment/talent-pool`) | `HrdTalentPoolController.cs` | `src/app/hr/recruitment/talent-pool/**` | ✅ Lulus Linter & Build | [FE-HRD-055.md](../task/report/frontend/FE-HRD-055.md) |
| `FE-HRD-056` | Serah Terima ke Onboarding (*Hiring Handoff*) (`/hr/recruitment/handoffs`) | `HrdHiringHandoffController.cs` | `src/app/hr/recruitment/handoffs/**` | ✅ Lulus Linter & Build | [FE-HRD-056.md](../task/report/frontend/FE-HRD-056.md) |
| `FE-HRD-057` | Verifikasi Menu, Hak Akses & Keterjangkauan Layar Rekrutmen `S-D2` | Seluruh Controller Rekrutmen | `src/app/hr/recruitment/**` | ✅ Lulus Linter & Build | [FE-HRD-057.md](../task/report/frontend/FE-HRD-057.md) |
| `FE-HRD-058` | Fondasi Layanan API & Label Status `S-C4-ONB` | Seluruh Controller Onboarding & Kontrak | `src/lib/services/hr/onboarding-service.js` | ✅ Lulus Linter & Build | [FE-HRD-058.md](../task/report/frontend/FE-HRD-058.md) |
| `FE-HRD-059` | Template Onboarding & Navigasi Menu | `HrdOnboardingTemplateController.cs` | `src/app/hr/onboarding/templates/**` | ✅ Lulus Linter & Build | [FE-HRD-059.md](../task/report/frontend/FE-HRD-059.md) |
| `FE-HRD-060` | Daftar & Detail Kasus Onboarding | `HrdOnboardingCaseController.cs` | `src/app/hr/onboarding/cases/**` | ✅ Lulus Linter & Build | [FE-HRD-060.md](../task/report/frontend/FE-HRD-060.md) |
| `FE-HRD-061` | Aksi Checklist pada Detail Kasus Onboarding | `HrdOnboardingCaseController.cs` | `src/components/features/hr/onboarding/` | ✅ Lulus Linter & Build | [FE-HRD-061.md](../task/report/frontend/FE-HRD-061.md) |
| `FE-HRD-062` | Siap Kerja, Batal Bergabung & Tautan Serah Terima | `HrdOnboardingCaseController.cs` | `src/app/hr/onboarding/cases/[id]/**` | ✅ Lulus Linter & Build | [FE-HRD-062.md](../task/report/frontend/FE-HRD-062.md) |
| `FE-HRD-063` | Daftar & Detail Kontrak Kerja | `HrdEmploymentContractController.cs` | `src/app/hr/contracts/**` | ✅ Lulus Linter & Build | [FE-HRD-063.md](../task/report/frontend/FE-HRD-063.md) |
| `FE-HRD-064` | Keputusan & Koreksi Kontrak pada Detail Kontrak | `HrdEmploymentContractController.cs` | `src/app/hr/contracts/[id]/**` | ✅ Lulus Linter & Build | [FE-HRD-064.md](../task/report/frontend/FE-HRD-064.md) |
| `FE-HRD-065` | Masa Percobaan (*Probation*) & Evaluasi Staf | `HrdProbationEvaluationController.cs` | `src/app/hr/onboarding/probation/**` | ✅ Lulus Linter & Build | [FE-HRD-065.md](../task/report/frontend/FE-HRD-065.md) |
| `FE-HRD-066` | Pendampingan Klinis (*Clinical Preceptorship*) | `HrdClinicalPreceptorshipController.cs` | `src/app/hr/onboarding/preceptorship/**` | ✅ Lulus Linter & Build | [FE-HRD-066.md](../task/report/frontend/FE-HRD-066.md) |
| `FE-HRD-067` | Batch Data Awal Kontrak | `HrdContractBatchController.cs` | `src/app/hr/contracts/batches/**` | ✅ Lulus Linter & Build | [FE-HRD-067.md](../task/report/frontend/FE-HRD-067.md) |
| `FE-HRD-068` | Permintaan Akses Pegawai Baru (*IT/Facility Provisioning*) | `HrdAccessProvisioningController.cs` | `src/app/hr/onboarding/provisioning/**` | ✅ Lulus Linter & Build | [FE-HRD-068.md](../task/report/frontend/FE-HRD-068.md) |
| `FE-HRD-069` | Pengaturan Kebijakan Onboarding & Kontrak | `HrdOnboardingSettingsController.cs` | `src/app/hr/onboarding/settings/**` | ✅ Lulus Linter & Build | [FE-HRD-069.md](../task/report/frontend/FE-HRD-069.md) |
| `FE-HRD-070` | Layar Perlu Tindakan (*Action Required*) | `HrdOnboardingDashboardController.cs` | `src/app/hr/onboarding/action-required/**` | ✅ Lulus Linter & Build | [FE-HRD-070.md](../task/report/frontend/FE-HRD-070.md) |
| `FE-HRD-071` | Amandemen Layar Riwayat Kontrak & Form Master Pegawai | `EmployeeController.cs` | `src/components/view/hr/master-data/employees/form/**` | ✅ Lulus Linter & Build | [FE-HRD-071.md](../task/report/frontend/FE-HRD-071.md) |
| `FE-HRD-072` | Amandemen Layar Tawaran `S-D2`: Jenis & Masa Kontrak | `HrdJobOfferController.cs` | `src/app/hr/recruitment/offers/[id]/**` | ✅ Lulus Linter & Build | [FE-HRD-072.md](../task/report/frontend/FE-HRD-072.md) |
| `FE-HRD-073` | Penanganan Tugas T15 s.d. T18 di Kotak Masuk HR | `WorkflowInstanceController.cs` | `src/app/hr/workflow/approval-inbox/**` | ✅ Lulus Linter & Build | [FE-HRD-073.md](../task/report/frontend/FE-HRD-073.md) |
| `FE-HRD-074` | Verifikasi Menu, Hak Akses & Keterjangkauan Layar `S-C4-ONB` | Seluruh Controller Onboarding | `src/app/hr/onboarding/**`, `src/app/hr/contracts/**` | ✅ Lulus Linter & Build | [FE-HRD-074.md](../task/report/frontend/FE-HRD-074.md) |
| `FE-HRD-075` | Service Client API Roster Penjadwalan | `RosterScheduleAdministrationController.cs` | `src/lib/services/hr/roster-scheduling-service.js` | ✅ Lulus Linter & Build | [FE-HRD-075.md](../task/report/frontend/FE-HRD-075.md) |
| `FE-HRD-076` | State Management Redux Roster Unit & Cache Invalidation | `RosterScheduleAdministrationController.cs` | `src/lib/state/slice/hr/roster-scheduling/` | ✅ Lulus Linter & Build | [FE-HRD-076.md](../task/report/frontend/FE-HRD-076.md) |
| `FE-HRD-077` | Layar Kisi Roster Bulanan Unit (`/hr/scheduling/roster`) | `RosterScheduleAdministrationController.cs` | `src/app/hr/scheduling/roster/**` | ✅ Lulus Linter & Build | [FE-HRD-077.md](../task/report/frontend/FE-HRD-077.md) |
| `FE-HRD-078` | Komponen Aksi Massal: Salin Periode Lalu & Pola Shift | `RosterScheduleAdministrationController.cs` | `src/components/features/hr/scheduling/` | ✅ Lulus Linter & Build | [FE-HRD-078.md](../task/report/frontend/FE-HRD-078.md) |
| `FE-HRD-079` | Drawer Validasi Jadwal: Aturan B1-B7 & W1-W7 | `RosterScheduleAdministrationController.cs` | `src/components/features/hr/scheduling/RosterValidationDrawer.jsx` | ✅ Lulus Linter & Build | [FE-HRD-079.md](../task/report/frontend/FE-HRD-079.md) |
| `FE-HRD-080` | Aksi Terbitkan Roster Mandiri Ka. Unit & Validasi Alasan | `RosterScheduleAdministrationController.cs` | `src/components/features/hr/scheduling/PublishRosterModal.jsx` | ✅ Lulus Linter & Build | [FE-HRD-080.md](../task/report/frontend/FE-HRD-080.md) |
| `FE-HRD-081` | Modal Ganti Petugas Cepat (*Quick Replacement*) Alasan Wajib | `RosterScheduleAdministrationController.cs` | `src/components/features/hr/scheduling/QuickReplacementModal.jsx` | ✅ Lulus Linter & Build | [FE-HRD-081.md](../task/report/frontend/FE-HRD-081.md) |
| `FE-HRD-082` | Koreksi Mundur Jadwal (<= 3 hari) & Proteksi Closed Banner | `RosterScheduleAdministrationController.cs` | `src/components/features/hr/scheduling/RetroactiveCorrectionModal.jsx` | ✅ Lulus Linter & Build | [FE-HRD-082.md](../task/report/frontend/FE-HRD-082.md) |
| `FE-HRD-083` | Service Client API On-Call & SLA Pemanggilan | `OnCallScheduleController.cs` | `src/lib/services/hr/on-call-service.js` | ✅ Lulus Linter & Build | [FE-HRD-083.md](../task/report/frontend/FE-HRD-083.md) |
| `FE-HRD-084` | Layar Kalender Penugasan Siaga (*On-Call Coverage*) | `OnCallScheduleController.cs` | `src/app/hr/scheduling/on-call/**` | ✅ Lulus Linter & Build | [FE-HRD-084.md](../task/report/frontend/FE-HRD-084.md) |
| `FE-HRD-085` | Modal Panggil Siaga (*Call-In*) & Badge Pemenuhan SLA | `OnCallScheduleController.cs` | `src/components/features/hr/scheduling/OnCallCallInModal.jsx` | ✅ Lulus Linter & Build | [FE-HRD-085.md](../task/report/frontend/FE-HRD-085.md) |
| `FE-HRD-086` | Layar Pengaturan Kebijakan Penjadwalan HR | `SchedulingSettingsController.cs` | `src/app/hr/scheduling/settings/**` | ✅ Lulus Linter & Build | [FE-HRD-086.md](../task/report/frontend/FE-HRD-086.md) |
| `FE-HRD-087` | Verifikasi Ujung-ke-Ujung Layar Roster & On-Call Slice S-B4 | `RosterScheduleAdministrationController.cs`, `OnCallScheduleController.cs` | `src/app/hr/scheduling/**`, `menu-items.jsx` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-087.md](../task/report/frontend/FE-HRD-087.md) |
| `FE-HRD-102` | API Service Client, Tipe & Redux Slice Cuti | `LeaveAdministrationController.cs`, `EmployeeSelfServiceLeaveController.cs` | `src/lib/services/hr/leave-service.js`, `src/lib/state/slice/hr/leave/` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-102.md](../task/report/frontend/FE-HRD-102.md) |
| `FE-HRD-103` | Komponen Kartu Metrik Saldo & Progress Bar Cuti | `EmployeeSelfServiceLeaveController.cs` | `src/components/features/hr/leave/LeaveBalanceMetricCards.jsx` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-103.md](../task/report/frontend/FE-HRD-103.md) |
| `FE-HRD-104` | Dasbor Saldo Kuota ESS & Drawer Buku Besar Ledger | `EmployeeSelfServiceLeaveController.cs` | `src/app/hr/leave/my-balance/**`, `LeaveLedgerDrawer.jsx` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-104.md](../task/report/frontend/FE-HRD-104.md) |
| `FE-HRD-105` | Manajemen Alokasi Saldo, Carry-Forward & Runner HR | `LeaveAdministrationController.cs` | `src/app/hr/leave/balances-admin/**` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-105.md](../task/report/frontend/FE-HRD-105.md) |
| `FE-HRD-106` | Modal Penyesuaian Saldo Resmi HR Dokumen Audit | `LeaveAdministrationController.cs` | `src/components/features/hr/leave/LeaveBalanceAdjustmentModal.jsx` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-106.md](../task/report/frontend/FE-HRD-106.md) |
| `FE-HRD-107` | Pemilih Rentang Tanggal, Half-Day & Validasi Saldo | `EmployeeSelfServiceLeaveController.cs` | `src/components/features/hr/leave/LeaveDateRangePicker.jsx` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-107.md](../task/report/frontend/FE-HRD-107.md) |
| `FE-HRD-108` | Modal Unggah Surat Sakit Ber-SIP & Masking Privasi Medis | `EmployeeSelfServiceLeaveController.cs` | `src/components/features/hr/leave/MedicalCertificateUploadModal.jsx` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-108.md](../task/report/frontend/FE-HRD-108.md) |
| `FE-HRD-109` | Formulir Pengajuan Cuti Mandiri & Izin Sakit ESS | `EmployeeSelfServiceLeaveController.cs` | `src/app/hr/leave/request/**` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-109.md](../task/report/frontend/FE-HRD-109.md) |
| `FE-HRD-110` | Banner Peringatan Kuota Safe Staffing Coverage | `LeaveAdministrationController.cs` | `src/components/features/hr/leave/SafeStaffingCoverageWarningBanner.jsx` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-110.md](../task/report/frontend/FE-HRD-110.md) |
| `FE-HRD-111` | Kotak Masuk Approval Atasan & Approve with Mitigation | `LeaveAdministrationController.cs` | `src/app/hr/leave/approval-inbox/**` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-111.md](../task/report/frontend/FE-HRD-111.md) |
| `FE-HRD-112` | Inisiasi & Pemantauan Pemanggilan Darurat (Leave Recall) | `LeaveAdministrationController.cs` | `src/app/hr/leave/recalls/**`, `LeaveRecallInitiationModal.jsx` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-112.md](../task/report/frontend/FE-HRD-112.md) |
| `FE-HRD-113` | Approval Recall HR Manager & Restore Saldo Sisa | `LeaveAdministrationController.cs` | `src/app/hr/leave/recalls/**` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-113.md](../task/report/frontend/FE-HRD-113.md) |
| `FE-HRD-114` | Rekonsiliasi Akhir Cuti, Retrospektif & Payroll Close | `LeaveAdministrationController.cs` | `src/app/hr/leave/reconciliations/**` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-114.md](../task/report/frontend/FE-HRD-114.md) |
| `FE-HRD-115` | Verifikasi Ujung-ke-Ujung Antarmuka Cuti Slice S-B2 | `LeaveAdministrationController.cs`, `EmployeeSelfServiceLeaveController.cs` | `src/app/hr/leave/**`, `menu-items.jsx` | ✅ Lulus Linter & Turbopack Build (594 Rute) | [FE-HRD-115.md](../task/report/frontend/FE-HRD-115.md) |
| `FE-HRD-116` | API Service Client, Tipe & Redux Slice Lembur | `OvertimePlanController.cs`, `OvertimeRealizationController.cs` | `src/lib/services/hr/overtime-service.js`, `src/lib/state/slice/hr/overtime/` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-116.md](../task/report/frontend/FE-HRD-116.md) |
| `FE-HRD-117` | Form Dual-Mode SPKL & Kalkulator Kuota PP 35/2021 | `OvertimePlanController.cs`, `OvertimeRealizationController.cs` | `src/components/features/hr/overtime/OvertimeFormSwitch.jsx` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-117.md](../task/report/frontend/FE-HRD-117.md) |
| `FE-HRD-118` | Layar Portal Mandiri Lembur Pegawai RS (ESS) | `OvertimePlanController.cs`, `OvertimeRealizationController.cs` | `src/app/hr/self-service/overtime/**` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-118.md](../task/report/frontend/FE-HRD-118.md) |
| `FE-HRD-119` | Komponen Inbox Persetujuan & Anti-Self-Approval | `OvertimeApprovalController.cs` | `src/components/features/hr/overtime/OvertimeApprovalTable.jsx` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-119.md](../task/report/frontend/FE-HRD-119.md) |
| `FE-HRD-120` | Kotak Masuk Approval Lembur Ruangan (Ka. Unit) | `OvertimeApprovalController.cs` | `src/app/hr/overtime/approvals/**` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-120.md](../task/report/frontend/FE-HRD-120.md) |
| `FE-HRD-121` | Drawer Investigasi Anomali Presensi Biometrik HR | `OvertimeVerificationModernController.cs` | `src/components/features/hr/overtime/OvertimeDiscrepancyDrawer.jsx` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-121.md](../task/report/frontend/FE-HRD-121.md) |
| `FE-HRD-122` | Pusat Verifikasi Presensi & Discrepancy HR | `OvertimeVerificationModernController.cs` | `src/app/hr/overtime/verifications/**` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-122.md](../task/report/frontend/FE-HRD-122.md) |
| `FE-HRD-123` | Kotak Masuk Approval Eskalasi Direktur Medik | `OvertimeApprovalController.cs` | `src/app/hr/overtime/medical-director-inbox/**` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-123.md](../task/report/frontend/FE-HRD-123.md) |
| `FE-HRD-124` | Komponen Kartu Tiket Saldo Libur CTO 90 Hari | `CompensatoryTimeOffModernController.cs` | `src/components/features/hr/overtime/CompensatoryTimeOffCard.jsx` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-124.md](../task/report/frontend/FE-HRD-124.md) |
| `FE-HRD-125` | Monitoring Saldo Cuti Pengganti (CTO) & Konversi | `CompensatoryTimeOffModernController.cs` | `src/app/hr/overtime/compensatory-leave/**` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-125.md](../task/report/frontend/FE-HRD-125.md) |
| `FE-HRD-126` | Tabel Rekonsiliasi & Indikator Rapel Retrospektif | `OvertimePayrollHandoffModernController.cs` | `src/components/features/hr/overtime/OvertimeReconciliationTable.jsx` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-126.md](../task/report/frontend/FE-HRD-126.md) |
| `FE-HRD-127` | Penutupan Periode Lembur, Cut-Off & Handoff Payroll | `OvertimePeriodModernController.cs`, `OvertimePayrollHandoffModernController.cs` | `src/app/hr/overtime/reconciliations/**` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-127.md](../task/report/frontend/FE-HRD-127.md) |
| `FE-HRD-128` | Pengaturan Kebijakan & Kuota Lembur RS (PP 35/2021) | `OvertimeSettingController.cs` | `src/app/hr/overtime/settings/**` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-128.md](../task/report/frontend/FE-HRD-128.md) |
| `FE-HRD-129` | Verifikasi Ujung-ke-Ujung Antarmuka Lembur Slice S-B3 | Seluruh Controller Lembur | `src/app/hr/overtime/**`, `src/app/hr/self-service/overtime/**` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-129.md](../task/report/frontend/FE-HRD-129.md) |
| `FE-HRD-130` | Fondasi Utilitas & Token Tema Penggajian RS | `PayrollRunModernController.cs` | `src/utils/hr/payroll/**`, `payroll.module.css` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-130.md](../task/report/frontend/FE-HRD-130.md) |
| `FE-HRD-131` | API Service Client, Tipe & Redux Slice Payroll | `PayrollRunModernController.cs`, `PayrollSettingModernController.cs` | `src/lib/services/hr/payroll-service.js`, `payroll-slice.js` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-131.md](../task/report/frontend/FE-HRD-131.md) |
| `FE-HRD-132` | Layar Dasbor Ringkasan & Monitoring Penggajian | `PayrollRunModernController.cs` | `src/app/hr/payroll/runs/**` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-132.md](../task/report/frontend/FE-HRD-132.md) |
| `FE-HRD-133` | Wizard Inisiasi Batch & Precondition Gate Cut-off | `PayrollRunModernController.cs` | `src/app/hr/payroll/runs/new/**` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-133.md](../task/report/frontend/FE-HRD-133.md) |
| `FE-HRD-134` | Lembar Kerja Kalkulasi Gaji & Filter Profesi (Worksheet) | `PayrollRunModernController.cs` | `src/app/hr/payroll/runs/[id]/worksheet/**` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-134.md](../task/report/frontend/FE-HRD-134.md) |
| `FE-HRD-135` | Drawer & Layar Investigasi Anomali Variansi Gaji | `PayrollRunModernController.cs` | `src/components/features/hr/payroll/PayrollVarianceDrawer.jsx`, `variances/**` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-135.md](../task/report/frontend/FE-HRD-135.md) |
| `FE-HRD-136` | Modal Detail Rincian Gaji & Komparasi Slip Lalu | `PayrollRunModernController.cs` | `src/components/features/hr/payroll/PayrollEmployeeDetailModal.jsx` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-136.md](../task/report/frontend/FE-HRD-136.md) |
| `FE-HRD-137` | Komponen Otorisasi 3-Jenjang & Anti-Self-Approval | `PayrollRunModernController.cs` | `src/components/features/hr/payroll/PayrollApprovalCard.jsx` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-137.md](../task/report/frontend/FE-HRD-137.md) |
| `FE-HRD-138` | Layar Dasbor Otorisasi Eksekutif Penggajian | `PayrollRunModernController.cs` | `src/app/hr/payroll/approvals/**` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-138.md](../task/report/frontend/FE-HRD-138.md) |
| `FE-HRD-139` | Modal Kunci Immutable & Serah Terima Finance SHA-256 | `PayrollRunModernController.cs` | `src/components/features/hr/payroll/PayrollFinanceHandoffModal.jsx` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-139.md](../task/report/frontend/FE-HRD-139.md) |
| `FE-HRD-140` | Dasbor Pemantauan Status Serah Terima Finance | `PayrollRunModernController.cs` | `src/app/hr/payroll/handoffs/**` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-140.md](../task/report/frontend/FE-HRD-140.md) |
| `FE-HRD-141` | Portal Slip Gaji Mandiri Pegawai (ESS) & PIN 6-Digit | `PayrollRunModernController.cs` | `src/app/ess/payslips/**`, `PayslipPinKeypadModal.jsx` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-141.md](../task/report/frontend/FE-HRD-141.md) |
| `FE-HRD-142` | Pengaturan Parameter Penggajian RS & Plafon BPJS | `PayrollSettingModernController.cs` | `src/app/hr/payroll/settings/**` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-142.md](../task/report/frontend/FE-HRD-142.md) |
| `FE-HRD-143` | Verifikasi Ujung-ke-Ujung Antarmuka Penggajian Slice S-B5 | Seluruh Controller Payroll | `src/app/hr/payroll/**`, `src/app/ess/payslips/**`, `menu-items.jsx` | ✅ Lulus Linter & Turbopack Build (608 Rute) | [FE-HRD-143.md](../task/report/frontend/FE-HRD-143.md) |
| `FE-HRD-144` | Service API Client, Adaptor DTO, Tipe Redux Slice & State Management Restitusi & Potongan | `PayrollRestitutionController.cs`, `PayrollRunInstallmentDeductionController.cs` | `src/lib/services/hr/restitution-service.js`, `restitution-slice.js`, `store.jsx` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-144.md](../task/report/frontend/FE-HRD-144.md) |
| `FE-HRD-145` | Utilitas Token, Format Rupiah, Kalkulator Plafon & Rasio 50% PP 36/2021 | `PayrollRestitutionController.cs` | `src/utils/hr/restitution/**`, `restitution.module.css` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-145.md](../task/report/frontend/FE-HRD-145.md) |
| `FE-HRD-146` | Komponen Dasbor Pemantauan Restitusi & Pemotongan Upah RS (4 KPI) | `PayrollRestitutionController.cs` | `src/components/view/hr/restitution/deductions-dashboard-view.jsx` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-146.md](../task/report/frontend/FE-HRD-146.md) |
| `FE-HRD-147` | Layar FE-HRD-168 – Dasbor Restitusi & Integrasi Pemotongan Penggajian RS | `PayrollRestitutionController.cs` | `src/app/corporate/hrd/payroll/deductions/**`, `src/app/hr/payroll/deductions/**` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-147.md](../task/report/frontend/FE-HRD-147.md) |
| `FE-HRD-148` | Komponen Jadwal Angsuran Pinjaman & Modal Simulasi Hard-Cap 50% | `PayrollRunInstallmentDeductionController.cs` | `src/components/features/hr/restitution/DeductionCapSimulationModal.jsx`, view | ✅ Lulus Linter & Turbopack Build | [FE-HRD-148.md](../task/report/frontend/FE-HRD-148.md) |
| `FE-HRD-149` | Layar FE-HRD-170 – Master Jadwal Angsuran & Peringatan Rasio Upah | `PayrollRunInstallmentDeductionController.cs` | `src/app/corporate/hrd/payroll/deductions/schedules/**` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-149.md](../task/report/frontend/FE-HRD-149.md) |
| `FE-HRD-150` | Komponen Pelacak Kuota Plafon Restitusi ESS & Modal Klaim Mandiri Nakes | `PayrollRestitutionController.cs` | `src/components/features/hr/restitution/EssClaimFormModal.jsx`, view | ✅ Lulus Linter & Turbopack Build | [FE-HRD-150.md](../task/report/frontend/FE-HRD-150.md) |
| `FE-HRD-151` | Layar FE-HRD-169 – Portal Layanan Mandiri (ESS) Klaim Restitusi RS | `PayrollRestitutionController.cs` | `src/app/ess/restitution-claims/**`, `src/app/hr/self-service/restitution-claims/**` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-151.md](../task/report/frontend/FE-HRD-151.md) |
| `FE-HRD-152` | Layar FE-HRD-172 – Kotak Masuk Approval 3-Tier Restitusi RS | `PayrollRestitutionController.cs` | `src/app/corporate/hrd/payroll/deductions/approvals/**`, view | ✅ Lulus Linter & Turbopack Build | [FE-HRD-152.md](../task/report/frontend/FE-HRD-152.md) |
| `FE-HRD-153` | Komponen Viewer Dokumen Zoom, Rotasi & Validasi Nota 60 Hari | `PayrollRestitutionController.cs` | `src/components/features/hr/restitution/ModalClaimDocumentPreview.jsx` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-153.md](../task/report/frontend/FE-HRD-153.md) |
| `FE-HRD-154` | Layar FE-HRD-171 – Drawer Koreksi Salah Potong Periode Lampau | `PayrollRestitutionController.cs` | `src/components/features/hr/restitution/PastPeriodCorrectionDrawer.jsx` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-154.md](../task/report/frontend/FE-HRD-154.md) |
| `FE-HRD-155` | Komponen Generator Voucher QR Code Kasir 24/7 Cito Darurat Pajanan Nakes | `PayrollRestitutionController.cs` | `src/components/features/hr/restitution/ModalVoucherQrViewer.jsx` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-155.md](../task/report/frontend/FE-HRD-155.md) |
| `FE-HRD-156` | Layar FE-HRD-173 – Pengaturan Kebijakan Plafon Benefit & Batas Potong Upah | `PayrollRestitutionController.cs` | `src/app/corporate/hrd/payroll/deductions/settings/**`, view | ✅ Lulus Linter & Turbopack Build | [FE-HRD-156.md](../task/report/frontend/FE-HRD-156.md) |
| `FE-HRD-157` | Verifikasi Ujung-ke-Ujung Antarmuka Restitusi & Pemotongan Slice S-B6 | Seluruh Controller Restitusi & Potongan | `src/app/corporate/hrd/payroll/deductions/**`, `src/app/ess/**`, `menu-items.jsx` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-157.md](../task/report/frontend/FE-HRD-157.md) |
| `FE-HRD-158` | Service API Client, Adaptor DTO, Tipe Redux Slice & State Management Manfaat Kesehatan | `BenefitPlanController.cs`, `BenefitEnrollmentController.cs`, `BenefitClaimController.cs` | `src/lib/services/hr/benefits-service.js`, `benefits-slice.js`, `store.jsx` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-158.md](../task/report/frontend/FE-HRD-158.md) |
| `FE-HRD-159` | Registrasi Peta Navigasi Sidebar, Rute /corporate/hrd/benefits/*, Rute ESS & Guards | Seluruh Controller Manfaat Kesehatan | `src/utils/menu-sidebar/menu-items.jsx` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-159.md](../task/report/frontend/FE-HRD-159.md) |
| `FE-HRD-160` | Komponen Reusable & Token UI Manfaat (StatusBadge, Meteran, Uploader KK/Akta) | `BenefitPlanController.cs` | `src/utils/hr/benefits/**`, `benefits.module.css` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-160.md](../task/report/frontend/FE-HRD-160.md) |
| `FE-HRD-161` | Layar FE-HRD-174 – Dasbor Analitik Manfaat Kesehatan, Utilisasi Plafon & Alert Batas Usia | `BenefitEnrollmentController.cs`, `BenefitPlanController.cs` | `src/app/corporate/hrd/benefits/dashboard/**`, view | ✅ Lulus Linter & Turbopack Build | [FE-HRD-161.md](../task/report/frontend/FE-HRD-161.md) |
| `FE-HRD-162` | Layar FE-HRD-175 – Master Katalog Paket Manfaat 3-Lapisan, Pagu Plafon & Diskon Tarif Staf | `BenefitPlanController.cs` | `src/app/corporate/hrd/benefits/plans/**`, `BenefitPlanFormModal.jsx` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-162.md](../task/report/frontend/FE-HRD-162.md) |
| `FE-HRD-163` | Komponen Drawer FE-HRD-175 – Matriks Kelayakan Grade Staf ke Kelas Kamar Rawat Inap | `BenefitPlanController.cs` | `src/components/features/hr/benefits/GradeEligibilityDrawer.jsx` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-163.md](../task/report/frontend/FE-HRD-163.md) |
| `FE-HRD-164` | Layar FE-HRD-176 – Monitoring Kepesertaan Asuransi Pegawai, Aktivasi H+0 vs Masa Percobaan | `BenefitEnrollmentController.cs` | `src/app/corporate/hrd/benefits/enrollments/**`, `PolicyNumberModal.jsx` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-164.md](../task/report/frontend/FE-HRD-164.md) |
| `FE-HRD-165` | Layar FE-HRD-177 – Antrean Meja Kerja Verifikasi Dokumen Tanggungan Keluarga HR | `BenefitEnrollmentController.cs` | `src/app/corporate/hrd/benefits/dependents/**`, `DependentVerificationModal.jsx` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-165.md](../task/report/frontend/FE-HRD-165.md) |
| `FE-HRD-166` | Layar FE-HRD-177 (ESS View) – Portal Mandiri ESS Keluarga Saya & Indikator Kuota 1 Pasangan + 3 Anak | `BenefitEnrollmentController.cs` | `src/app/ess/my-dependents/**`, `FamilyRegistrationFormModal.jsx` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-166.md](../task/report/frontend/FE-HRD-166.md) |
| `FE-HRD-167` | Layar FE-HRD-178 – Inquiry Hak Berobat Staf di RS Sendiri & Buku Besar Mutasi Saldo SIMRS | `InHouseHealthcareController.cs` | `src/app/corporate/hrd/benefits/inhouse-ledger/**`, view | ✅ Lulus Linter & Turbopack Build | [FE-HRD-167.md](../task/report/frontend/FE-HRD-167.md) |
| `FE-HRD-168` | Modal Dialog FE-HRD-178 – Penyesuaian Manual Saldo Plafon Berobat RS oleh HR Manajer | `InHouseHealthcareController.cs` | `src/components/features/hr/benefits/ManualBalanceAdjustmentModal.jsx` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-168.md](../task/report/frontend/FE-HRD-168.md) |
| `FE-HRD-169` | Layar FE-HRD-179 – Meja Kerja Monitoring, Audit Kuitansi & Settlement Klaim Koordinasi Manfaat (COB) | `BenefitClaimController.cs` | `src/app/corporate/hrd/benefits/cob-claims/**`, `ClaimSettlementDrawer.jsx` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-169.md](../task/report/frontend/FE-HRD-169.md) |
| `FE-HRD-170` | Layar FE-HRD-179 (ESS View) – Formulir Mandiri Pengajuan Klaim COB Berlampiran Mandatory SEP BPJS | `BenefitClaimController.cs` | `src/app/ess/claims/cob-submission/**`, view | ✅ Lulus Linter & Turbopack Build | [FE-HRD-170.md](../task/report/frontend/FE-HRD-170.md) |
| `FE-HRD-171` | Verifikasi Ujung-ke-Ujung Antarmuka UI/UX, Linter CI/CD, Build Next.js, dan Hak Akses Peran Slice S-D3 | Seluruh Controller Benefit Management | `src/app/corporate/hrd/benefits/**`, `src/app/ess/**`, `menu-items.jsx` | ✅ Lulus Linter & Turbopack Build | [FE-HRD-171.md](../task/report/frontend/FE-HRD-171.md) |


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

**Bukti repair frontend 12 November 2026.** `FE-HRD-R25` 🟡 memigrasikan layar `FE-HRD-059` s.d. `FE-HRD-073` ke data server, permission ketat, dan base component; lint, build, serta CSS gate lulus. Uji browser dan endpoint unggah/template batch masih terbuka. [Laporan R25](../task/report/frontend/FE-HRD-R25.md).

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


---

## 1e. Jejak `S-B1` Presensi dan Kehadiran (sub-modul 07) — requirement ke task dan bukti

Ditambahkan pada revision 9, 4 November 2026, sesudah desain `S-B1` disetujui pengguna. Seluruh
baris di bawah **belum punya bukti implementasi**: task-nya baru direncanakan. Kolom status
akan diperbarui oleh build skill saat task selesai.

| Field | Nilai |
| --- | --- |
| Approval desain | Pengguna, pemilik teknis dan produk (`HRD-DEC-015`), 4 November 2026, beserta jawaban interview `HRD-DEC-104` s.d. `HRD-DEC-109` dan asumsi `HRD-ASM-15` s.d. `HRD-ASM-18` — `02-backend-architecture.md` bagian 20, `03-frontend-architecture.md` bagian 16, kontrak `v11`, `04-prd-to-mvp.md` bagian 25, `data/data-dictionary.md` bagian 12, `testing/acceptance-test-matrix.md` bagian 19, empat flowchart `flowcharts/presensi-dan-kehadiran.md` |
| Keputusan | `HRD-DEC-104` s.d. `HRD-DEC-109`, `HRD-ASM-15` s.d. `HRD-ASM-18` (decision log revision `21`, bagian 37) |
| Kontrak | API `v11` bagian 17; State Transition `v11` bagian 15; Validation `v11` bagian 16; Permission `v11` bagian 15; Integration `v11` bagian 8; Data Dictionary `v11` bagian 12; Acceptance Test `v11` bagian 19 |
| Manifest | revision `21`; hash masukan terkunci pada metadata kedua roadmap |
| Source SHA | BE `8b17b75a` (branch `MHamzah`), FE `44990b3d3` (branch `HamzahV2`) |
| Task | Backend `BE-HRD-104` s.d. `BE-HRD-116` (13 task); frontend `FE-HRD-088` s.d. `FE-HRD-101` (14 task) |

### Functional requirement

| FR | Perilaku Bisnis | Desain & Kontrak | Task Backend | Task Frontend | Bukti yang Diharapkan | Status |
| --- | --- | --- | --- | --- | --- | :---: |
| `FR-ATT-01` | Perekaman tap biometrik mentah immutable tanpa modifikasi | BE §20.4.1; API §17.1; `HRD-DEC-104` | `BE-HRD-107` | `FE-HRD-091` ✅ | `AT-HRD-ATT-06` | FE [091](../task/report/frontend/FE-HRD-091.md) ✅ (11 Nov 2026) |
| `FR-ATT-02` | Normalisasi shift malam lintas hari ke tanggal mulai shift (Hari H) | BE §20.4.2; `VR-ATT-01`; `HRD-DEC-104` | `BE-HRD-106` | `FE-HRD-090` 🟡 | `AT-HRD-ATT-01`, `UAT-ATT-01` | FE [090](../task/report/frontend/FE-HRD-090.md) 🟡 (11 Nov 2026) |
| `FR-ATT-03` | Log tap mentah tetap menyimpan timestamp fisik riil | BE §20.4.2; `HRD-DEC-104` | `BE-HRD-107` | `FE-HRD-091` ✅ | `AT-HRD-ATT-06` | FE [091](../task/report/frontend/FE-HRD-091.md) ✅ (11 Nov 2026) |
| `FR-ATT-04` | Toleransi keterlambatan 10 menit berstatus Hadir Tepat Waktu | BE §20.4.3; `VR-ATT-02`; `HRD-DEC-105` | `BE-HRD-105`, `BE-HRD-106` | `FE-HRD-090` 🟡, `FE-HRD-099` ✅ | `AT-HRD-ATT-02`, `UAT-ATT-02` | FE [090](../task/report/frontend/FE-HRD-090.md) 🟡, FE [099](../task/report/frontend/FE-HRD-099.md) ✅ (11 Nov 2026) |
| `FR-ATT-05` | Keterlambatan lewat 10 menit dihitung penuh dari jam mulai resmi | BE §20.4.3; `VR-ATT-03`; `HRD-DEC-105` | `BE-HRD-106` | `FE-HRD-090` 🟡 | `AT-HRD-ATT-03` | FE [090](../task/report/frontend/FE-HRD-090.md) 🟡 (11 Nov 2026) |
| `FR-ATT-06` | Hanya 1 tap diberi status sementara IncompletePunch | BE §20.4.4; State §15.1; `HRD-DEC-106` | `BE-HRD-106` | `FE-HRD-090` 🟡, `FE-HRD-100` | `AT-HRD-ATT-04` | FE [090](../task/report/frontend/FE-HRD-090.md) 🟡; task FE lain belum (11 Nov 2026) |
| `FR-ATT-07` | Pengingat ESS otomatis pada pegawai dengan IncompletePunch | BE §20.4.4; Integration §8.1; `HRD-DEC-106` | `BE-HRD-106` | `FE-HRD-100` | `AT-HRD-ATT-04` | |
| `FR-ATT-08` | Auto-convert IncompletePunch ke Absent saat tutup periode | BE §20.4.4; `VR-ATT-16`; `HRD-DEC-106` | `BE-HRD-113` | `FE-HRD-092`, `FE-HRD-093` | `AT-HRD-ATT-17`, `UAT-ATT-03` | |
| `FR-ATT-09` | Pengajuan izin pulang cepat non-deductible (tidak memotong cuti) | BE §20.4.5; State §15.2; `HRD-DEC-107` | `BE-HRD-108`, `BE-HRD-111` | `FE-HRD-097`, `FE-HRD-100` | `AT-HRD-ATT-07`, `UAT-ATT-04` | |
| `FR-ATT-10` | Jam kerja riil izin pulang cepat dihitung dari tap aktual mesin | BE §20.4.5; `VR-ATT-08`; `HRD-DEC-107` | `BE-HRD-108` | `FE-HRD-097` | `AT-HRD-ATT-08` | |
| `FR-ATT-11` | Persetujuan atasan mengaktifkan waiver dispensasi presensi | BE §20.4.5; `VR-ATT-10`; `HRD-DEC-107` | `BE-HRD-108` | `FE-HRD-097` | `AT-HRD-ATT-09` | |
| `FR-ATT-12` | Pengajuan surat tugas dinas luar RS (ambulans, kurir lab, workshop) | BE §20.4.6; API §17.4; `HRD-DEC-109` | `BE-HRD-109`, `BE-HRD-111` | `FE-HRD-098`, `FE-HRD-100` | `AT-HRD-ATT-10` | |
| `FR-ATT-13` | Status OnDuty otomatis disahkan pasca-approval atasan | BE §20.4.6; `VR-ATT-12`; `HRD-DEC-109` | `BE-HRD-109` | `FE-HRD-098` | `AT-HRD-ATT-11`, `UAT-ATT-05` | |
| `FR-ATT-14` | Staf OnDuty tidak dianggap mangkir (Absent) | BE §20.4.6; `VR-ATT-13`; `HRD-DEC-109` | `BE-HRD-109` | `FE-HRD-098` | `AT-HRD-ATT-12` | |
| `FR-ATT-15` | Penutupan periode ditolak jika ada koreksi presensi pending | BE §20.4.7; `VR-ATT-15`; `HRD-DEC-108` | `BE-HRD-113` | `FE-HRD-092`, `FE-HRD-093` | `AT-HRD-ATT-16` | |
| `FR-ATT-16` | Pembukaan kembali periode (reopen) dibatasi khusus HR Manager | BE §20.4.7; API §17.7; `HRD-DEC-108` | `BE-HRD-114`, `BE-HRD-115` | `FE-HRD-092`, `FE-HRD-094` | `AT-HRD-ATT-18` | |
| `FR-ATT-17` | Reopen periode otomatis memicu rollback serah terima payroll | BE §20.4.7; Integration §8.3; `HRD-DEC-108` | `BE-HRD-114` | `FE-HRD-094` | `AT-HRD-ATT-18` | |
| `FR-ATT-18` | Koreksi presensi on-behalf kuasa HR saat staf terkendala ESS | BE §20.4.8; `VR-ATT-05`; `HRD-DEC-028` | `BE-HRD-110`, `BE-HRD-111` | `FE-HRD-095`, `FE-HRD-096` | `AT-HRD-ATT-13` | |
| `FR-ATT-19` | Pencatatan initiator HR dan bukti kendala pada koreksi on-behalf | BE §20.4.8; `VR-ATT-06`; `HRD-DEC-028` | `BE-HRD-110` | `FE-HRD-096` | `AT-HRD-ATT-14` | |
| `FR-ATT-20` | Notifikasi transparansi otomatis ke ESS pegawai yang diwakili | BE §20.4.8; Integration §8.1; `HRD-DEC-028` | `BE-HRD-110` | `FE-HRD-096`, `FE-HRD-100` | `AT-HRD-ATT-14` | |
| `FR-ATT-21` | Integrasi rekalkulasi absensi otomatis akibat koreksi mundur roster | BE §20.4.9; Integration §8.2 (`INT-HR-41`) | `BE-HRD-112` | `FE-HRD-090` 🟡 | `AT-HRD-ATT-15` | FE [090](../task/report/frontend/FE-HRD-090.md) 🟡 (11 Nov 2026) |
| `FR-ATT-22` | Konfigurasi parameter toleransi dan cut-off presensi RS | BE §20.4.10; API §17.8; `VR-ATT-17`..`20` | `BE-HRD-105` | `FE-HRD-099` ✅ | `AT-HRD-ATT-19` | FE [099](../task/report/frontend/FE-HRD-099.md) ✅ (11 Nov 2026) |
| `FR-ATT-23` | Serah terima data rekapitulasi jam kerja riil ke modul Payroll | BE §20.4.11; Integration §8.4 (`INT-HR-44`) | `BE-HRD-115` | `FE-HRD-092` | `AT-HRD-ATT-20` | |

### Matriks acceptance test ke task

| Acceptance test | Task | Cakupan Pengujian |
| --- | --- | --- |
| `AT-HRD-ATT-01` s.d. `AT-HRD-ATT-03` | `BE-HRD-105`, `BE-HRD-106`, `FE-HRD-090` 🟡, `FE-HRD-099` ✅ | Normalisasi shift malam lintas hari dan perhitungan toleransi keterlambatan 10 menit |
| `AT-HRD-ATT-04` s.d. `AT-HRD-ATT-06` | `BE-HRD-106`, `BE-HRD-107`, `FE-HRD-090` 🟡, `FE-HRD-091` ✅, `FE-HRD-100` | Status IncompletePunch, pengingat ESS, dan audit trail log biometrik mentah immutable |
| `AT-HRD-ATT-07` s.d. `AT-HRD-ATT-09` | `BE-HRD-108`, `BE-HRD-111`, `FE-HRD-097`, `FE-HRD-100` | Izin pulang cepat non-deductible, pencatatan jam kerja riil, dan penegakan waiver dispensasi |
| `AT-HRD-ATT-10` s.d. `AT-HRD-ATT-12` | `BE-HRD-109`, `BE-HRD-111`, `FE-HRD-098`, `FE-HRD-100` | Surat dinas luar RS (ambulans, kurir lab, workshop) dan pengesahan status OnDuty |
| `AT-HRD-ATT-13`, `AT-HRD-ATT-14` | `BE-HRD-110`, `BE-HRD-111`, `FE-HRD-095`, `FE-HRD-096` | Jalur koreksi presensi kuasa HR on-behalf, jejak initiator, dan notifikasi transparansi ESS |
| `AT-HRD-ATT-15` | `BE-HRD-112`, `FE-HRD-090` 🟡 | Event listener rekalkulasi otomatis absensi akibat mutasi jadwal roster mundur (INT-HR-41) |
| `AT-HRD-ATT-16` s.d. `AT-HRD-ATT-18` | `BE-HRD-113`, `BE-HRD-114`, `BE-HRD-115`, `FE-HRD-092`, `FE-HRD-093`, `FE-HRD-094` | Guard tutup periode (blokir koreksi pending), auto-convert incomplete punch, reopen darurat HR Manager & rollback payroll |
| `AT-HRD-ATT-19`, `AT-HRD-ATT-20` | `BE-HRD-105`, `BE-HRD-115`, `FE-HRD-092`, `FE-HRD-099` ✅ | Pengaturan parameter toleransi presensi RS dan serah terima agregasi jam riil ke Payroll |
| `UAT-ATT-01` s.d. `UAT-ATT-05` | `BE-HRD-116`, `FE-HRD-101` | Validasi bisnis ujung-ke-ujung alur presensi, shift malam, perizinan, dan penutupan periode |

### Gap keterkaitan requirement ke bukti verifikasi

| Gap | Status | Catatan / Pemilik |
| --- | :---: | --- |
| Keterkaitan requirement ke bukti verifikasi S-B1 | **LENGKAP** (0 Gap) | Seluruh 23 functional requirement (`FR-ATT-01` s.d. `FR-ATT-23`) memiliki pemetaan task backend (`BE-HRD-104`..`116`), task frontend (`FE-HRD-088`..`101`), serta bukti verifikasi uji penerimaan (`AT-HRD-ATT-01`..`20`, `UAT-ATT-01`..`05`). |

**Kebijakan Uji Backend:** Sesuai `rules/backend/TEST_POLICY.md`, backend tidak menuntut proyek automated test terpisah. Bukti verifikasi backend yang dipetakan adalah QBE preflight/conformance, review diff, `dotnet restore`/`dotnet build`, verifikasi kontrak API, dan verifikasi proses bisnis. Ketiadaan unit test backend **bukan** coverage gap.


---

### 1f. Slice `S-B2` — Administrasi Cuti & Izin Pegawai Rumah Sakit (Sub-modul 08)

#### Functional requirement

| FR | Perilaku Bisnis | Desain & Kontrak | Task Backend | Task Frontend | Bukti yang Diharapkan | Status |
| --- | --- | --- | --- | --- | --- | :---: |
| `FR-LEV-01` | Alokasi hak cuti tahunan 12 hari kerja per tahun kalender (prorata pegawai baru) | BE §21.8; `HRD-DEC-110` | `BE-HRD-118` | `FE-HRD-103`, `FE-HRD-104` | `AT-HRD-LEV-01`, `UAT-LEV-01` | ✅ FE Selesai |
| `FR-LEV-02` | Pembukuan mutasi saldo kredit/debit atomik pada `TrxLeaveBalanceLedger` | BE §21.2; Data §13.1; `HRD-DEC-110` | `BE-HRD-117`, `BE-HRD-118` | `FE-HRD-104` | `AT-HRD-LEV-02` | ✅ FE Selesai |
| `FR-LEV-03` | Pemindahan sisa saldo cuti akhir tahun (31 Des) maksimal 6 hari kerja | BE §21.8; `VR-LEV-12`; `HRD-DEC-110` | `BE-HRD-119` | `FE-HRD-105` | `AT-HRD-LEV-03`, `UAT-LEV-01` | ✅ FE Selesai |
| `FR-LEV-04` | Penghangusan otomatis sisa cuti akhir tahun di atas 6 hari kerja (`Forfeiture`) | BE §21.8; `LEV-B8`; `HRD-DEC-110` | `BE-HRD-119` | `FE-HRD-105` | `AT-HRD-LEV-03` | ✅ FE Selesai |
| `FR-LEV-05` | Penghangusan sisa saldo carry-forward yang belum terpakai per 30 Juni | BE §21.8; `LEV-W2`; `HRD-DEC-110` | `BE-HRD-119`, `BE-HRD-120` | `FE-HRD-105` | `AT-HRD-LEV-04`, `UAT-LEV-01` | ✅ FE Selesai |
| `FR-LEV-06` | Pengajuan cuti tahunan mandiri via ESS dengan penghitungan hari efektif | BE §21.8; API §18.1; `HRD-DEC-110` | `BE-HRD-122` | `FE-HRD-109` | `AT-HRD-LEV-05` | ✅ FE Selesai |
| `FR-LEV-07` | Opsi cuti setengah hari (half-day 0.5 hari kerja) sesi pagi/siang | BE §21.8; `VR-LEV-04`..`05`; `HRD-ASM-20` | `BE-HRD-122` | `FE-HRD-107`, `FE-HRD-109` | `AT-HRD-LEV-07`, `AT-HRD-LEV-08` | ✅ FE Selesai |
| `FR-LEV-08` | Penolakan submit permohonan cuti bila saldo aktif tidak mencukupi | BE §21.8; `VR-LEV-01`; `LEV-B1` | `BE-HRD-118`, `BE-HRD-122` | `FE-HRD-107`, `FE-HRD-109` | `AT-HRD-LEV-06` | ✅ FE Selesai |
| `FR-LEV-09` | Evaluasi ketersediaan staf unit 24/7 & notifikasi Safe Staffing Coverage Warning | BE §21.8; Integration §9.2.1 (`INT-HR-45`); `HRD-DEC-111` | `BE-HRD-121` | `FE-HRD-110`, `FE-HRD-111` | `AT-HRD-LEV-09`, `UAT-LEV-02` | ✅ FE Selesai |
| `FR-LEV-10` | Persetujuan Approve with Mitigation atasan dengan input catatan staf pengganti | BE §21.8; `VR-LEV-21`; `HRD-DEC-111` | `BE-HRD-124` | `FE-HRD-111` | `AT-HRD-LEV-10`, `AT-HRD-LEV-11` | ✅ FE Selesai |
| `FR-LEV-11` | Pembatalan pre-leave mandiri sebelum hari-H memulihkan saldo 100% | BE §21.8; `VR-LEV-17`; `HRD-ASM-21` | `BE-HRD-125` | `FE-HRD-104` | `AT-HRD-LEV-15` | ✅ FE Selesai |
| `FR-LEV-12` | Izin sakit mandiri 1 hari kerja melalui ESS dengan surat pernyataan diri | BE §21.8; `HRD-DEC-112` | `BE-HRD-123` | `FE-HRD-109` | `AT-HRD-LEV-12` | ✅ FE Selesai |
| `FR-LEV-13` | Cuti sakit $\ge 2$ hari kerja wajib unggah surat keterangan dokter ber-SIP | BE §21.8; `VR-LEV-06`; `LEV-B3`; `HRD-DEC-112` | `BE-HRD-123` | `FE-HRD-108`, `FE-HRD-109` | `AT-HRD-LEV-13`, `UAT-LEV-03` | ✅ FE Selesai |
| `FR-LEV-14` | Validasi metadata surat sakit (faskes, nomor SIP dokter, rentang istirahat) | BE §21.8; `VR-LEV-08`; `HRD-DEC-112` | `BE-HRD-123` | `FE-HRD-108` | `AT-HRD-LEV-13` | ✅ FE Selesai |
| `FR-LEV-15` | Masking diagnosa medis (ICD-10) dari atasan langsung demi privasi K3RS | BE §21.8; `VR-LEV-09`; `LEV-W3`; `HRD-DEC-112` | `BE-HRD-123` | `FE-HRD-108`, `FE-HRD-111` | `AT-HRD-LEV-14`, `UAT-LEV-03` | ✅ FE Selesai |
| `FR-LEV-16` | Inisiasi Leave Recall darurat RS oleh Kepala Ruangan pada cuti Active | BE §21.8; `VR-LEV-14`; `HRD-DEC-113` | `BE-HRD-126` | `FE-HRD-112` | `AT-HRD-LEV-16`, `UAT-LEV-04` | ✅ FE Selesai |
| `FR-LEV-17` | Otorisasi persetujuan Leave Recall tunggal oleh HR Manager | BE §21.8; `VR-LEV-15`; `LEV-B5`; `HRD-DEC-113` | `BE-HRD-126` | `FE-HRD-113` | `AT-HRD-LEV-17` | ✅ FE Selesai |
| `FR-LEV-18` | Eksekusi otomatis restore sisa hari cuti ke saldo ledger (`RecallRestore`) | BE §21.8; `VR-LEV-16`; `LEV-W4`; `HRD-DEC-113` | `BE-HRD-127` | `FE-HRD-113` | `AT-HRD-LEV-17`, `UAT-LEV-04` | ✅ FE Selesai |
| `FR-LEV-19` | Penerbitan event pemulihan jadwal shift aktif di Roster 06 pasca-recall | BE §21.8; Integration §9.2.2 (`INT-HR-46`); `HRD-DEC-113` | `BE-HRD-127` | `FE-HRD-113` | `AT-HRD-LEV-18` | ✅ FE Selesai |
| `FR-LEV-20` | Pencatatan riwayat recall darurat lengkap pada audit trail kepatuhan RS | BE §21.8; Permission §16.2; `HRD-DEC-113` | `BE-HRD-126`, `BE-HRD-127` | `FE-HRD-112`, `FE-HRD-113` | `AT-HRD-LEV-17` | ✅ FE Selesai |
| `FR-LEV-21` | Cuti pending saat cut-off payroll tidak memotong gaji pegawai bulan berjalan | BE §21.8; `LEV-W5`; `HRD-DEC-114` | `BE-HRD-129` | `FE-HRD-114` | `AT-HRD-LEV-19` | ✅ FE Selesai |
| `FR-LEV-22` | Penandaan IsPendingPayrollReconciliation untuk rekonsiliasi bulan berikutnya | BE §21.8; `HRD-DEC-114` | `BE-HRD-129` | `FE-HRD-114` | `AT-HRD-LEV-19` | ✅ FE Selesai |
| `FR-LEV-23` | Penguncian immutabilitas cuti Completed / PayrollClosed menolak /reverse (HTTP 409) | BE §21.8; `VR-LEV-19`..`20`; `LEV-B4`; `HRD-DEC-115` | `BE-HRD-128` | `FE-HRD-114` | `AT-HRD-LEV-20`, `UAT-LEV-05` | ✅ FE Selesai |
| `FR-LEV-24` | Penyesuaian saldo resmi HR via TrxLeaveAdjustment wajib nomor dokumen audit | BE §21.8; `VR-LEV-18`; `LEV-B6`; `HRD-DEC-115` | `BE-HRD-128` | `FE-HRD-106` | `AT-HRD-LEV-20`, `UAT-LEV-05` | ✅ FE Selesai |
| `FR-LEV-25` | Penutupan periode cuti menghasilkan snapshot terkunci untuk serah terima payroll | BE §21.8; Integration §9.2.4 (`INT-HR-48`); `HRD-DEC-114` | `BE-HRD-129` | `FE-HRD-114` | `AT-HRD-LEV-19` | ✅ FE Selesai |

#### Matriks acceptance test ke task

| Acceptance test | Task | Cakupan Pengujian |
| --- | --- | --- |
| `AT-HRD-LEV-01` s.d. `AT-HRD-LEV-04` | `BE-HRD-117`..`120`, `FE-HRD-102`..`105` | Pembentukan hak 12 hari, mutasi atomik ledger, carry-forward max 6 hari & scheduler kedaluwarsa 30 Juni |
| `AT-HRD-LEV-05` s.d. `AT-HRD-LEV-08` | `BE-HRD-118`, `BE-HRD-122`, `FE-HRD-104`, `FE-HRD-107`, `FE-HRD-109` | Pengajuan cuti mandiri ESS, kalkulasi saldo, dan opsi cuti setengah hari (half-day 0.5 hari) |
| `AT-HRD-LEV-09` s.d. `AT-HRD-LEV-11` | `BE-HRD-121`, `BE-HRD-124`, `FE-HRD-110`, `FE-HRD-111` | Safe Staffing Coverage Warning, Approval Inbox atasan, dan aksi Approve with Mitigation |
| `AT-HRD-LEV-12` s.d. `AT-HRD-LEV-14` | `BE-HRD-123`, `FE-HRD-108`, `FE-HRD-109`, `FE-HRD-111` | Cuti sakit 1 hari vs $\ge 2$ hari wajib surat dokter ber-SIP dan masking diagnosa privasi K3RS |
| `AT-HRD-LEV-15` | `BE-HRD-125`, `FE-HRD-104` | Pembatalan pre-leave sebelum hari-H dan pemulihan saldo 100% otomatis |
| `AT-HRD-LEV-16` s.d. `AT-HRD-LEV-18` | `BE-HRD-126`, `BE-HRD-127`, `FE-HRD-112`, `FE-HRD-113` | Inisiasi recall darurat Kepala Unit, otorisasi HR Manager, pemulihan saldo & shift Roster 06 |
| `AT-HRD-LEV-19`, `AT-HRD-LEV-20` | `BE-HRD-128`, `BE-HRD-129`, `FE-HRD-106`, `FE-HRD-114` | Rekonsiliasi retrospektif cut-off payroll, penutupan celah /reverse HTTP 409 & HR adjustment audit |
| `UAT-LEV-01` s.d. `UAT-LEV-05` | `BE-HRD-130`, `FE-HRD-115` | Validasi bisnis ujung-ke-ujung alur cuti, saldo ledger, carry-forward, mitigasi kuota, recall, dan audit immutabilitas |

#### Gap keterkaitan requirement ke bukti verifikasi

| Gap | Status | Catatan / Pemilik |
| --- | :---: | --- |
| Keterkaitan requirement ke bukti verifikasi S-B2 | **LENGKAP** (0 Gap) | Seluruh 25 functional requirement (`FR-LEV-01` s.d. `FR-LEV-25`) memiliki pemetaan task backend (`BE-HRD-117`..`130`), task frontend (`FE-HRD-102`..`115`), serta bukti verifikasi uji penerimaan (`AT-HRD-LEV-01`..`20`, `UAT-LEV-01`..`05`). |

**Kebijakan Uji Backend:** Sesuai `rules/backend/TEST_POLICY.md`, backend tidak menuntut proyek automated test terpisah. Bukti verifikasi backend yang dipetakan adalah QBE preflight/conformance, review diff, `dotnet restore`/`dotnet build`, verifikasi kontrak API, dan verifikasi proses bisnis. Ketiadaan unit test backend **bukan** coverage gap.

---

### 1g. Slice `S-B3` — Administrasi Lembur Pegawai Rumah Sakit (Sub-modul 09)

#### Functional requirement

| FR | Perilaku Bisnis | Desain & Kontrak | Task Backend | Task Frontend | Bukti yang Diharapkan | Status |
| --- | --- | --- | --- | --- | --- | :---: |
| `FR-OT-01` | Formulir pengajuan SPKL terencana pra-dinas minimal H-1 sebelum dinas dimulai | BE §22.8; `HRD-DEC-116` | `BE-HRD-134` | `FE-HRD-117`, `FE-HRD-118` | `AT-HRD-OT-01`, `UAT-OT-01` | ✅ FE Selesai |
| `FR-OT-02` | Formulir pelaporan klaim darurat medis pasca-dinas cito (OK, IGD, ICU) maksimal H+1 kerja | BE §22.8; `HRD-DEC-116` | `BE-HRD-135` | `FE-HRD-117`, `FE-HRD-118` | `AT-HRD-OT-03`, `UAT-OT-01` | ✅ FE Selesai |
| `FR-OT-03` | Kewajiban input Nomor Rekam Medis / Insiden Darurat dan ringkasan tindakan klinis cito | BE §22.8; `VR-OT-02`; `HRD-DEC-116` | `BE-HRD-133`, `BE-HRD-135` | `FE-HRD-117`, `FE-HRD-118` | `AT-HRD-OT-03`, `AT-HRD-OT-04` | ✅ FE Selesai |
| `FR-OT-04` | Inisiasi SPKL kelompok (*team overtime*) oleh Ka. Ruangan dengan satu pengajuan induk banyak nakes | BE §22.8; `HRD-ASM-26` | `BE-HRD-134` | `FE-HRD-117`, `FE-HRD-118` | `AT-HRD-OT-01` | ✅ FE Selesai |
| `FR-OT-05` | Penolakan otomatis klaim pasca-dinas melebihi batas waktu H+1 hari kerja (`SubmissionDeadlinePassed`) | BE §22.8; `VR-OT-06`; `HRD-DEC-116` | `BE-HRD-133`, `BE-HRD-135` | `FE-HRD-118` | `AT-HRD-OT-05` | ✅ FE Selesai |
| `FR-OT-06` | Penghitungan akumulasi jam lembur harian dan mingguan per staf aktif secara realtime | BE §22.8; `HRD-DEC-117` | `BE-HRD-136` | `FE-HRD-117` | `AT-HRD-OT-06`, `AT-HRD-OT-07` | ✅ FE Selesai |
| `FR-OT-07` | Pemblokiran otomatis (*hard-block*) pengajuan terencana jika jam > 4 jam/hari atau > 18 jam/minggu | BE §22.8; `VR-OT-07`; `HRD-DEC-117` | `BE-HRD-136` | `FE-HRD-117` | `AT-HRD-OT-06`, `AT-HRD-OT-07`, `UAT-OT-02` | ✅ FE Selesai |
| `FR-OT-08` | Alur eskalasi darurat klinis ke Direktur Pelayanan Medik jika cito melampaui kuota PP 35/2021 | BE §22.8; `VR-OT-12`; `HRD-DEC-117` | `BE-HRD-136`, `BE-HRD-138` | `FE-HRD-123` | `AT-HRD-OT-08`, `UAT-OT-02` | ✅ FE Selesai |
| `FR-OT-09` | Indikator visual risiko kelelahan nakes (*fatigue risk*) pada inbox approval Direktur Pelayanan Medik | BE §22.8; `HRD-DEC-117` | `BE-HRD-138` | `FE-HRD-123` | `AT-HRD-OT-08` | ✅ FE Selesai |
| `FR-OT-10` | Pencatatan audit trail permanen atas otorisasi khusus dispensasi darurat Direktur Pelayanan Medik | BE §22.8; Permission §17.2; `HRD-DEC-117` | `BE-HRD-138` | `FE-HRD-123` | `AT-HRD-OT-09`, `UAT-OT-02` | ✅ FE Selesai |
| `FR-OT-11` | Pencocokan otomatis jam klaim SPKL terhadap log mesin presensi biometrik Sub-modul 07 (`INT-HR-50`) | BE §22.8; Integration §10.2.2; `HRD-DEC-119` | `BE-HRD-139` | `FE-HRD-121`, `FE-HRD-122` | `AT-HRD-OT-12`, `UAT-OT-03` | ✅ FE Selesai |
| `FR-OT-12` | Aturan nilai terkecil (*Min-Duration Rule*): durasi dihitung `min(spkl_claimed, biometric_excess)` | BE §22.8; `HRD-DEC-119` | `BE-HRD-139` | `FE-HRD-121` | `AT-HRD-OT-12` | ✅ FE Selesai |
| `FR-OT-13` | Pembulatan kelipatan 30 menit ke bawah, dan durasi fisik < 30 menit bernilai 0 jam kompensasi | BE §22.8; `VR-OT-09`; `HRD-ASM-23` | `BE-HRD-139` | `FE-HRD-121` | `AT-HRD-OT-13`, `AT-HRD-OT-14` | ✅ FE Selesai |
| `FR-OT-14` | Penandaan status Flagged Discrepancy bila selisih klaim vs mesin > 30 menit atau lupa tap | BE §22.8; `VR-OT-10`; `HRD-DEC-119` | `BE-HRD-139`, `BE-HRD-140` | `FE-HRD-121`, `FE-HRD-122` | `AT-HRD-OT-14`, `AT-HRD-OT-15` | ✅ FE Selesai |
| `FR-OT-15` | Fasilitas investigasi manual HR Admin dengan kewajiban unggah bukti berkas logbook fisik ruangan | BE §22.8; `VR-OT-11`; `HRD-DEC-119` | `BE-HRD-140` | `FE-HRD-121`, `FE-HRD-122` | `AT-HRD-OT-16`, `UAT-OT-03` | ✅ FE Selesai |
| `FR-OT-16` | Staf dapat memilih bentuk kompensasi: Uang Lembur Finansial vs Cuti Pengganti (CTO) pada SPKL | BE §22.8; `HRD-DEC-118` | `BE-HRD-134`, `BE-HRD-141` | `FE-HRD-117`, `FE-HRD-118` | `AT-HRD-OT-17`, `UAT-OT-04` | ✅ FE Selesai |
| `FR-OT-17` | Perhitungan pengali uang lembur reguler (1.5x, 2x) dan tarif khusus hari libur resmi RS | BE §22.8; `HRD-DEC-118` | `BE-HRD-132`, `BE-HRD-139` | `FE-HRD-127`, `FE-HRD-128` | `AT-HRD-OT-12` | ✅ FE Selesai |
| `FR-OT-18` | Konversi durasi lembur menjadi kuota CTO & posting mutasi kredit ke `TrxLeaveBalanceLedger` Sub-08 | BE §22.8; Integration §10.2.1 (`INT-HR-49`); `HRD-DEC-118` | `BE-HRD-141` | `FE-HRD-124`, `FE-HRD-125` | `AT-HRD-OT-17`, `UAT-OT-04` | ✅ FE Selesai |
| `FR-OT-19` | Masa kedaluwarsa kuota CTO tepat 90 hari kalender sejak tanggal terbit persetujuan SPKL | BE §22.8; `HRD-DEC-118`, `HRD-ASM-25` | `BE-HRD-141` | `FE-HRD-124`, `FE-HRD-125` | `AT-HRD-OT-18` | ✅ FE Selesai |
| `FR-OT-20` | Scheduler otomatis menghanguskan sisa kuota CTO yang belum digunakan setelah 90 hari non-tunai | BE §22.8; `VR-OT-14`; `HRD-DEC-118` | `BE-HRD-142` | `FE-HRD-125` | `AT-HRD-OT-18`, `UAT-OT-04` | ✅ FE Selesai |
| `FR-OT-21` | Penguncian periode transaksi lembur bulanan pada tanggal cut-off (tgl 20) menjadi CutoffLocked | BE §22.8; `VR-OT-16`; `HRD-DEC-121` | `BE-HRD-143` | `FE-HRD-126`, `FE-HRD-127` | `AT-HRD-OT-19`, `UAT-OT-05` | ✅ FE Selesai |
| `FR-OT-22` | Penandaan SPKL belum selesai verifikasi saat cut-off sebagai PendingNextReconciliation | BE §22.8; `HRD-DEC-121` | `BE-HRD-143` | `FE-HRD-126`, `FE-HRD-127` | `AT-HRD-OT-20` | ✅ FE Selesai |
| `FR-OT-23` | Paket rekonsiliasi retrospektif lembur lampau disetujui terlambat untuk rapel gaji bulan berjalan | BE §22.8; `HRD-DEC-121` | `BE-HRD-143`, `BE-HRD-144` | `FE-HRD-126`, `FE-HRD-127` | `AT-HRD-OT-20`, `UAT-OT-05` | ✅ FE Selesai |
| `FR-OT-24` | Pembentukan berkas snapshot immutable serah terima data lembur terverifikasi ke Sub-10 Payroll | BE §22.8; Integration §10.2.4 (`INT-HR-52`); `HRD-DEC-121` | `BE-HRD-144` | `FE-HRD-127` | `AT-HRD-OT-19`, `UAT-OT-05` | ✅ FE Selesai |
| `FR-OT-25` | Pemblokiran mutlak manipulasi/pembatalan SPKL pada periode lembur terkunci atau ditutup (`Closed`) | BE §22.8; `VR-OT-17`; `HRD-DEC-121` | `BE-HRD-143` | `FE-HRD-127` | `AT-HRD-OT-19` | ✅ FE Selesai |

#### Matriks acceptance test ke task

| Acceptance test | Task | Cakupan Pengujian |
| --- | --- | --- |
| `AT-HRD-OT-01` s.d. `AT-HRD-OT-05` | `BE-HRD-133`..`135`, `FE-HRD-116`..`118` | SPKL terencana H-1, klaim darurat medis cito H+1 ber-RM, penugasan kelompok tim, dan auto-reject kedaluwarsa |
| `AT-HRD-OT-06` s.d. `AT-HRD-OT-10` | `BE-HRD-136`..`138`, `FE-HRD-119`..`123` | Hard-block kuota PP 35/2021 (4 jam/hari, 18 jam/minggu), eskalasi & otorisasi Direktur Medik, serta anti-self-approval mutlak |
| `AT-HRD-OT-11` | `BE-HRD-137`, `FE-HRD-120` | Pengalihan rute persetujuan berjenjang struktural saat Ka. Ruangan mengajukan lembur pribadi |
| `AT-HRD-OT-12` s.d. `AT-HRD-OT-16` | `BE-HRD-139`, `BE-HRD-140`, `FE-HRD-121`, `FE-HRD-122` | Verifikasi silang mesin biometrik, min-duration rule, pembulatan 30m, batas 25m = 0 jam, dan investigasi bukti logbook fisik |
| `AT-HRD-OT-17`, `AT-HRD-OT-18` | `BE-HRD-141`, `BE-HRD-142`, `FE-HRD-124`, `FE-HRD-125` | Kredit mutasi cuti pengganti CTO ke Sub-08 dan scheduler eksekusi hangus saldo CTO 90 hari non-tunai |
| `AT-HRD-OT-19`, `AT-HRD-OT-20` | `BE-HRD-143`, `BE-HRD-144`, `FE-HRD-126`, `FE-HRD-127` | Penguncian cut-off tgl 20, rapel retrospektif SPKL pending di payroll depan, dan snapshot serah terima payroll |
| `UAT-OT-01` s.d. `UAT-OT-05` | `BE-HRD-145`, `FE-HRD-129` | Validasi bisnis ujung-ke-ujung alur SPKL, klaim cito, otorisasi Direktur Medik, audit presensi, CTO, dan payroll close |

#### Gap keterkaitan requirement ke bukti verifikasi

| Gap | Status | Catatan / Pemilik |
| --- | :---: | --- |
| Keterkaitan requirement ke bukti verifikasi S-B3 | **LENGKAP** (0 Gap) | Seluruh 25 functional requirement (`FR-OT-01` s.d. `FR-OT-25`) memiliki pemetaan task backend (`BE-HRD-131`..`145`), task frontend (`FE-HRD-116`..`129`), serta bukti verifikasi uji penerimaan (`AT-HRD-OT-01`..`20`, `UAT-OT-01`..`05`). |

**Kebijakan Uji Backend:** Sesuai `rules/backend/TEST_POLICY.md`, backend tidak menuntut proyek automated test terpisah. Bukti verifikasi backend yang dipetakan adalah QBE preflight/conformance, review diff, `dotnet restore`/`dotnet build`, verifikasi kontrak API, dan verifikasi proses bisnis. Ketiadaan unit test backend **bukan** coverage gap.


---

### 1h. Traceability Slice `S-B5` — Pemrosesan Penggajian Pegawai Rumah Sakit (Sub-modul 10, Kontrak v14, 5 November 2026)

Jejak ketertelusuran penuh antara 25 kebutuhan fungsional (`FR-PAY-01` s.d. `FR-PAY-25`), 6 keputusan bisnis (`HRD-DEC-122` s.d. `HRD-DEC-127`), 4 asumsi operasional (`HRD-ASM-27` s.d. `HRD-ASM-30`), 15 task backend (`BE-HRD-146` s.d. `BE-HRD-160`), 14 task frontend (`FE-HRD-130` s.d. `FE-HRD-143`), serta 20 skenario uji penerimaan (`AT-HRD-PAY-01` s.d. `AT-HRD-PAY-20`) dan 5 skenario UAT (`UAT-PAY-01` s.d. `UAT-PAY-05`).

| Requirement ID | Deskripsi Singkat Kebutuhan | Dokumen Rujukan / Aturan | Task BE | Task FE | Bukti Verifikasi / Acceptance Test | Gap |
| --- | --- | --- | --- | --- | --- | :---: |
| `FR-PAY-01` | Inisiasi siklus penggajian baru periode aktif pasca cut-off tgl 20 | BE A 23.8; `HRD-DEC-122` | `BE-HRD-149` | `FE-HRD-132`, `FE-HRD-133` | `AT-HRD-PAY-01`, `UAT-PAY-01` | ✅ FE Selesai |
| `FR-PAY-02` | Validasi gerbang prasyarat penguncian waktu kerja Presensi (07), Cuti (08), Lembur (09) | BE A 23.8 (`PAY-B1`); `VR-PAY-02`; `HRD-DEC-122` | `BE-HRD-148` | `FE-HRD-133` | `AT-HRD-PAY-02`, `UAT-PAY-01` | ✅ FE Selesai |
| `FR-PAY-03` | Snapshot beku parameter tarif master gaji & batas plafon saat inisiasi batch | BE A 23.8 (`PAY-W6`); `VR-PAY-03`; `HRD-DEC-122` | `BE-HRD-147`, `BE-HRD-149` | `FE-HRD-133`, `FE-HRD-142` | `AT-HRD-PAY-01` | ✅ FE Selesai |
| `FR-PAY-04` | Perhitungan penghasilan tetap: gaji pokok, tunjangan jabatan, tunjangan profesi & risiko | BE A 23.8 (`PAY-B3`); `VR-PAY-04`; `HRD-DEC-123` | `BE-HRD-150` | `FE-HRD-134` | `AT-HRD-PAY-04`, `UAT-PAY-02` | ✅ FE Selesai |
| `FR-PAY-05` | Penyerapan jam lembur resmi Sub-09 dan formula upah lembur PP 35/2021 | BE A 23.8 (`PAY-B4`); Integration A 11 (`INT-HR-52`); `HRD-DEC-123` | `BE-HRD-151` | `FE-HRD-134` | `AT-HRD-PAY-05`, `UAT-PAY-02` | ✅ FE Selesai |
| `FR-PAY-06` | Penyerapan kehadiran dinas shift malam Sub-07 dan kalkulasi premi shift malam | BE A 23.8; Integration A 8 (`INT-HR-36`); `HRD-DEC-123`, `HRD-ASM-27` | `BE-HRD-151` | `FE-HRD-134` | `AT-HRD-PAY-06`, `UAT-PAY-02` | ✅ FE Selesai |
| `FR-PAY-07` | Kalkulasi iuran BPJS Kesehatan (1% pegawai, 4% RS) berplafon upah resmi | BE A 23.8 (`PAY-W4`); `VR-PAY-08`; `HRD-DEC-123` | `BE-HRD-152` | `FE-HRD-134`, `FE-HRD-142` | `AT-HRD-PAY-07`, `UAT-PAY-02` | ✅ FE Selesai |
| `FR-PAY-08` | Kalkulasi 4 program BPJS Ketenagakerjaan (JKK, JKM, JHT, JP berplafon) | BE A 23.8 (`PAY-W4`); `VR-PAY-09`, `VR-PAY-10`; `HRD-DEC-123` | `BE-HRD-152` | `FE-HRD-134`, `FE-HRD-142` | `AT-HRD-PAY-08`, `UAT-PAY-02` | ✅ FE Selesai |
| `FR-PAY-09` | Pemotongan PPh 21 bulanan Tarif Efektif Rata-Rata (TER A/B/C) PMK 168/2023 | BE A 23.8; Integration A 11.4 (`INT-HR-56`); `HRD-DEC-123`, `HRD-ASM-28` | `BE-HRD-153` | `FE-HRD-134` | `AT-HRD-PAY-09`, `UAT-PAY-02` | ✅ FE Selesai |
| `FR-PAY-10` | Penyerapan potongan angsuran pinjaman koperasi & restitusi Sub-11 | BE A 23.8; Integration A 11.3 (`INT-HR-55`); `HRD-DEC-123` | `BE-HRD-154` | `FE-HRD-134` | `AT-HRD-PAY-10`, `UAT-PAY-02` | ✅ FE Selesai |
| `FR-PAY-11` | Penyerapan penyesuaian retrospektif / rapel selisih lembur periode lampau | BE A 23.8; `HRD-DEC-123` | `BE-HRD-151` | `FE-HRD-134` | `AT-HRD-PAY-11`, `UAT-PAY-02` | ✅ FE Selesai |
| `FR-PAY-12` | Evaluasi variansi deviasi take-home pay >= 20% atau nominal >= Rp 2.000.000 | BE A 23.8 (`PAY-W1`, `PAY-W2`); `VR-PAY-13`; `HRD-DEC-124`, `HRD-ASM-29` | `BE-HRD-155` | `FE-HRD-134`, `FE-HRD-135` | `AT-HRD-PAY-12`, `UAT-PAY-03` | ✅ FE Selesai |
| `FR-PAY-13` | Otomasi pembentukan tiket Variance Alert berstatus PendingJustification | BE A 23.8; `HRD-DEC-124` | `BE-HRD-155` | `FE-HRD-135` | `AT-HRD-PAY-12`, `UAT-PAY-03` | ✅ FE Selesai |
| `FR-PAY-14` | Pengisian justifikasi audit investigasi variansi oleh Officer (min 20 karakter) | BE A 23.8; `VR-PAY-15`; `HRD-DEC-124` | `BE-HRD-156` | `FE-HRD-135` | `AT-HRD-PAY-14`, `UAT-PAY-03` | ✅ FE Selesai |
| `FR-PAY-15` | Pemblokiran submit batch penggajian jika terdapat variansi anomali pending | BE A 23.8 (`PAY-B5`); `VR-PAY-14`; `HRD-DEC-124` | `BE-HRD-156` | `FE-HRD-135` | `AT-HRD-PAY-13`, `UAT-PAY-03` | ✅ FE Selesai |
| `FR-PAY-16` | Otorisasi Tier 1 oleh Payroll Officer mengalihkan status ke WaitingHrReview | BE A 23.8; `VR-PAY-16`; `HRD-DEC-125` | `BE-HRD-157` | `FE-HRD-137`, `FE-HRD-138` | `AT-HRD-PAY-15`, `UAT-PAY-04` | ✅ FE Selesai |
| `FR-PAY-17` | Pemblokiran persetujuan diri sendiri (Anti-Self-Approval) bagi HR Manager | BE A 23.8 (`PAY-B6`); `VR-PAY-17`; `HRD-DEC-125` | `BE-HRD-157` | `FE-HRD-137`, `FE-HRD-138` | `AT-HRD-PAY-16`, `UAT-PAY-04` | ✅ FE Selesai |
| `FR-PAY-18` | Otorisasi Tier 3 oleh Direktur SDM/Keuangan & penguncian batch permanen | BE A 23.8; `VR-PAY-20`; `HRD-DEC-125` | `BE-HRD-157` | `FE-HRD-137`, `FE-HRD-138` | `AT-HRD-PAY-17`, `UAT-PAY-04` | ✅ FE Selesai |
| `FR-PAY-19` | Rekapitulasi eksekutif biaya gaji per unit kerja dan kelompok tenaga medis | FE A 19.2.1; `HRD-DEC-125` | `BE-HRD-157` | `FE-HRD-138` | `AT-HRD-PAY-17`, `UAT-PAY-04` | ✅ FE Selesai |
| `FR-PAY-20` | Pembangkitan paket serah terima Finance berintegritas Checksum SHA-256 | BE A 23.8 (`PAY-B7`); Integration A 11.1 (`INT-HR-53`); `HRD-DEC-126` | `BE-HRD-158` | `FE-HRD-139`, `FE-HRD-140` | `AT-HRD-PAY-18`, `UAT-PAY-05` | ✅ FE Selesai |
| `FR-PAY-21` | Transmisi asinkron paket serah terima ke Modul Finance dengan retry policy | BE A 23.8; Integration A 11.1 (`INT-HR-53`); `HRD-DEC-126` | `BE-HRD-158` | `FE-HRD-139`, `FE-HRD-140` | `AT-HRD-PAY-18`, `UAT-PAY-05` | ✅ FE Selesai |
| `FR-PAY-22` | Penerimaan webhook callback penolakan Finance & rollback status terkendali | BE A 23.8; Integration A 11.2 (`INT-HR-54`); `HRD-DEC-126` | `BE-HRD-159` | `FE-HRD-140` | `AT-HRD-PAY-19`, `UAT-PAY-05` | ✅ FE Selesai |
| `FR-PAY-23` | Otomasi pelepasan slip gaji digital ESS berstatus Released per tgl 25 | BE A 23.8 (`PAY-B8`); `VR-PAY-23`; `HRD-DEC-127` | `BE-HRD-159` | `FE-HRD-141` | `AT-HRD-PAY-20`, `UAT-PAY-05` | ✅ FE Selesai |
| `FR-PAY-24` | Proteksi keamanan slip gaji ESS via PIN 6-digit & lockout 30 menit (3x salah) | BE A 23.8; `VR-PAY-22`; `HRD-DEC-127`, `HRD-ASM-30` | `BE-HRD-159` | `FE-HRD-141` | `AT-HRD-PAY-20`, `UAT-PAY-05` | ✅ FE Selesai |
| `FR-PAY-25` | Unduh berkas PDF slip gaji ber-password terenkripsi dan watermark resmi RS | BE A 23.8; `VR-PAY-24`; `HRD-DEC-127` | `BE-HRD-159` | `FE-HRD-141` | `AT-HRD-PAY-20`, `UAT-PAY-05` | ✅ FE Selesai |
#### Matriks acceptance test ke task

| Acceptance test | Task | Cakupan Pengujian |
| --- | --- | --- |
| `AT-HRD-PAY-01` s.d. `AT-HRD-PAY-03` | `BE-HRD-146`..`149`, `FE-HRD-130`..`133` | Inisiasi batch, precondition gate check tgl 20, freeze parameter snapshot, dan penolakan batch duplikat |
| `AT-HRD-PAY-04` s.d. `AT-HRD-PAY-06` | `BE-HRD-150`, `BE-HRD-151`, `FE-HRD-134` | Kalkulasi gaji pokok, tunjangan fungsional profesi, uang lembur PP 35, dan premi shift malam presensi Sub-07 |
| `AT-HRD-PAY-07` s.d. `AT-HRD-PAY-11` | `BE-HRD-152`..`154`, `FE-HRD-134` | Plafon BPJS Kesehatan, BPJS Ketenagakerjaan JP, PPh 21 TER PMK 168/2023, potongan pinjaman Sub-11, dan rapel retrospektif |
| `AT-HRD-PAY-12` s.d. `AT-HRD-PAY-14` | `BE-HRD-155`, `BE-HRD-156`, `FE-HRD-135` | Deteksi anomali variansi >= 20% / Rp 2jt, tiket alert pending, penolakan karakter < 20, dan resolusi justifikasi audit |
| `AT-HRD-PAY-15` s.d. `AT-HRD-PAY-17` | `BE-HRD-156`, `BE-HRD-157`, `FE-HRD-137`, `FE-HRD-138` | Alur otorisasi 3-jenjang, anti-self-approval HR Manager, otorisasi final Direksi, dan penguncian permanen batch |
| `AT-HRD-PAY-18`, `AT-HRD-PAY-19` | `BE-HRD-158`, `BE-HRD-159`, `FE-HRD-139`, `FE-HRD-140` | Pembentukan paket handoff Finance ber-checksum SHA-256, callback penolakan Finance & rollback terkendali |
| `AT-HRD-PAY-20` | `BE-HRD-159`, `FE-HRD-141` | Pelepasan slip gaji digital ESS tgl 25, verifikasi PIN sekunder 6-digit, lockout 30 menit, dan unduh PDF ber-password |
| `UAT-PAY-01` s.d. `UAT-PAY-05` | `BE-HRD-160`, `FE-HRD-143` | Pengujian bisnis ujung-ke-ujung alur inisiasi, kalkulasi komparasi, investigasi variansi, otorisasi 3-jenjang, dan handoff ESS |

#### Gap keterkaitan requirement ke bukti verifikasi

| Gap | Status | Catatan / Pemilik |
| --- | :---: | --- |
| Keterkaitan requirement ke bukti verifikasi S-B5 | **LENGKAP** (0 Gap) | Seluruh 25 functional requirement (`FR-PAY-01` s.d. `FR-PAY-25`) memiliki pemetaan task backend (`BE-HRD-146`..`160`), task frontend (`FE-HRD-130`..`143`), serta bukti verifikasi uji penerimaan (`AT-HRD-PAY-01`..`20`, `UAT-PAY-01`..`05`). |

**Kebijakan Uji Backend:** Sesuai `rules/backend/TEST_POLICY.md`, backend tidak menuntut proyek automated test terpisah. Bukti verifikasi backend yang dipetakan adalah QBE preflight/conformance, review diff, `dotnet restore`/`dotnet build`, verifikasi kontrak API, dan verifikasi proses bisnis. Ketiadaan unit test backend **bukan** coverage gap.


---

### 1i. Traceability Slice `S-B6` — Restitusi & Integrasi Pemotongan Penggajian (Sub-modul 11, Kontrak `v15`)

Jejak ketertelusuran penuh antara 25 kebutuhan fungsional (`FR-DED-01` s.d. `FR-DED-25`), 6 keputusan bisnis (`HRD-DEC-128` s.d. `HRD-DEC-133`), 4 asumsi (`HRD-ASM-31` s.d. `HRD-ASM-34`), 15 task backend (`BE-HRD-161` s.d. `BE-HRD-175`), 14 task frontend (`FE-HRD-144` s.d. `FE-HRD-157`), 20 acceptance tests (`AT-HRD-DED-01` s.d. `AT-HRD-DED-20`), serta 5 skenario UAT (`UAT-DED-01` s.d. `UAT-DED-05`).

| Requirement ID | Deskripsi Singkat Kebutuhan | Dokumen Rujukan / Aturan | Task BE | Task FE | Bukti Uji Verifikasi | Status Gap |
| :--- | :--- | :--- | :--- | :--- | :--- | :---: |
| `FR-DED-01` | Kalkulasi ambang batas pemotongan non-regulasi tepat 50% Gross Pay | BE A 24.8 (`DED-B1`); `HRD-DEC-128`; PP 36/2021 | `BE-HRD-164` | `FE-HRD-145`, `FE-HRD-149` | `AT-HRD-DED-01`, `UAT-DED-01` | LENGKAP (0 Gap) |
| `FR-DED-02` | Penegakan hirarki prioritas berjenjang 1 s.d. 4 (Ganti Rugi $
ightarrow$ Kasbon $
ightarrow$ Koperasi $
ightarrow$ Paguyuban) | BE A 24.8 (`DED-B2`); `HRD-DEC-128`; `VR-DED-02` | `BE-HRD-165` | `FE-HRD-148`, `FE-HRD-149` | `AT-HRD-DED-02`, `UAT-DED-01` | LENGKAP (0 Gap) |
| `FR-DED-03` | Pemangkasan parsial komponen & pencatatan `deferred_amount` | BE A 24.8; `HRD-DEC-128`; `VR-DED-03` | `BE-HRD-165` | `FE-HRD-146`, `FE-HRD-149` | `AT-HRD-DED-03`, `UAT-DED-01` | LENGKAP (0 Gap) |
| `FR-DED-04` | Otomasi penjadwalan ulang (*carry-over*) tagihan tertunda ke bulan berikutnya | BE A 24.8 (`DED-B3`); `HRD-DEC-128` | `BE-HRD-165` | `FE-HRD-147`, `FE-HRD-149` | `AT-HRD-DED-04`, `UAT-DED-01` | LENGKAP (0 Gap) |
| `FR-DED-05` | Tarik snapshot batch piutang aktif Finance H-3 cut-off (tgl 17) | `INT-HR-57`; `HRD-DEC-129`; `HRD-ASM-32` | `BE-HRD-163` | `FE-HRD-146`, `FE-HRD-147` | `AT-HRD-DED-05`, `UAT-DED-02` | LENGKAP (0 Gap) |
| `FR-DED-06` | Penguncian jadwal cicilan bulan berjalan per tgl 20 pukul 23:59:59 | BE A 24.8; `HRD-DEC-129`; `VR-DED-17` | `BE-HRD-163` | `FE-HRD-146`, `FE-HRD-148` | `AT-HRD-DED-07`, `UAT-DED-02` | LENGKAP (0 Gap) |
| `FR-DED-07` | Pengiriman webhook callback realisasi pemotongan tgl 25 ke Finance | `INT-HR-58`; `HRD-DEC-129`; `AUDIT-DED-01` | `BE-HRD-174` | `FE-HRD-144`, `FE-HRD-157` | `AT-HRD-DED-08`, `UAT-DED-02` | LENGKAP (0 Gap) |
| `FR-DED-08` | Mekanisme retry eksponensial saat sinkronisasi Finance mengalami timeout | `INT-HR-57`; `HRD-DEC-129` | `BE-HRD-163` | `FE-HRD-147` | `AT-HRD-DED-06`, `UAT-DED-02` | LENGKAP (0 Gap) |
| `FR-DED-09` | Formulir pengajuan klaim mandiri kacamata/medis di portal ESS | FE A 20.2.2; `HRD-DEC-130`; `VR-DED-15` | `BE-HRD-167` | `FE-HRD-150`, `FE-HRD-151` | `AT-HRD-DED-09`, `UAT-DED-03` | LENGKAP (0 Gap) |
| `FR-DED-10` | Validasi kuota sisa plafon tahunan pegawai otomatis di ESS | BE A 24.7; `HRD-DEC-130`; `VR-DED-10`, `VR-DED-11` | `BE-HRD-162`, `BE-HRD-167` | `FE-HRD-150`, `FE-HRD-151` | `AT-HRD-DED-10`, `UAT-DED-03` | LENGKAP (0 Gap) |
| `FR-DED-11` | Validasi batas usia kuitansi pengeluaran maksimal 60 hari kalender | `HRD-DEC-130`; `VR-DED-09` | `BE-HRD-167` | `FE-HRD-151` | `AT-HRD-DED-11`, `UAT-DED-03` | LENGKAP (0 Gap) |
| `FR-DED-12` | Verifikasi bertingkat 3-Tier (Atasan, HR C&B, Manajer SDM) | `HRD-DEC-130`; State A 19.1; `VR-DED-23` | `BE-HRD-168` | `FE-HRD-152`, `FE-HRD-153` | `AT-HRD-DED-13`, `UAT-DED-03` | LENGKAP (0 Gap) |
| `FR-DED-13` | Penegakan aturan mutlak Anti-Self-Approval klaim restitusi | BE A 24.8 (`DED-B8`); `VR-DED-08` | `BE-HRD-168` | `FE-HRD-152` | `AT-HRD-DED-12`, `UAT-DED-03` | LENGKAP (0 Gap) |
| `FR-DED-14` | Injeksi otomatis klaim approved non-cito ke slip gaji bulanan tgl 25 | BE A 24.3; `HRD-DEC-130`; `TrxPayrollRestitution` | `BE-HRD-170` | `FE-HRD-144`, `FE-HRD-157` | `AT-HRD-DED-14`, `UAT-DED-03` | LENGKAP (0 Gap) |
| `FR-DED-15` | Formulir investigasi keluhan salah potong gaji periode lampau | FE A 20.2.4; `HRD-DEC-131`; `PAY-B6` | `BE-HRD-171` | `FE-HRD-154` | `AT-HRD-DED-15`, `UAT-DED-04` | LENGKAP (0 Gap) |
| `FR-DED-16` | Penguncian tautan audit slip lampau terdampak (`affected_payslip_id`) | BE A 24.8 (`DED-B4`); `HRD-DEC-131`; `VR-DED-04` | `BE-HRD-171` | `FE-HRD-154` | `AT-HRD-DED-15`, `UAT-DED-04` | LENGKAP (0 Gap) |
| `FR-DED-17` | Klasifikasi komponen restitusi non-pajak (Bebas PPh 21) | BE A 24.8 (`DED-B5`); `HRD-ASM-33`; `VR-DED-05` | `BE-HRD-171` | `FE-HRD-154` | `AT-HRD-DED-16`, `UAT-DED-04` | LENGKAP (0 Gap) |
| `FR-DED-18` | Konsolidasi sisa pokok utang pinjaman pada Final Clearance Payslip | `INT-HR-60`; `HRD-DEC-132`; Sub-20 Exit | `BE-HRD-174` | `FE-HRD-148`, `FE-HRD-157` | `AT-HRD-DED-20`, `UAT-DED-01` | LENGKAP (0 Gap) |
| `FR-DED-19` | Pengecualian batas 50% hingga 100% pesangon penutupan akhir kerja | BE A 24.8 (`DED-B7`); `HRD-DEC-132`; `VR-DED-07` | `BE-HRD-174` | `FE-HRD-148`, `FE-HRD-157` | `AT-HRD-DED-20`, `UAT-DED-01` | LENGKAP (0 Gap) |
| `FR-DED-20` | Penerbitan Outstanding Receivable Statement jika pesangon defisit | `HRD-DEC-132`; `VR-DED-21`; `AUDIT-DED-12` | `BE-HRD-174` | `FE-HRD-147`, `FE-HRD-157` | `AT-HRD-DED-20`, `UAT-DED-04` | LENGKAP (0 Gap) |
| `FR-DED-21` | Penahanan pengesahan formulir Exit Clearance Sub-20 hingga utang lunas | `INT-HR-60`; `HRD-DEC-132`; `VR-DED-21` | `BE-HRD-174` | `FE-HRD-147`, `FE-HRD-157` | `AT-HRD-DED-20`, `UAT-DED-04` | LENGKAP (0 Gap) |
| `FR-DED-22` | Tagging & flagging darurat medis nakes cito (`UrgentDisbursementCito`) | BE A 24.8; `HRD-DEC-133`; `IsCitoEmergency` | `BE-HRD-167`, `BE-HRD-172` | `FE-HRD-151`, `FE-HRD-155` | `AT-HRD-DED-17`, `UAT-DED-05` | LENGKAP (0 Gap) |
| `FR-DED-23` | Penerbitan voucher kasir elektronik ber-token SHA-256 kedaluwarsa 48 jam | BE A 24.6; `HRD-DEC-133`; `VR-DED-13`, `VR-DED-14` | `BE-HRD-172` | `FE-HRD-155` | `AT-HRD-DED-17`, `UAT-DED-05` | LENGKAP (0 Gap) |
| `FR-DED-24` | Kliring & verifikasi online voucher di loket Kasir RS 24/7 | `INT-HR-59`; `HRD-DEC-133`; `AUDIT-DED-07` | `BE-HRD-173` | `FE-HRD-155` | `AT-HRD-DED-18`, `UAT-DED-05` | LENGKAP (0 Gap) |
| `FR-DED-25` | Penguncian status DisbursedViaCashier pencegah bayar ganda di penggajian | BE A 24.8 (`DED-B6`); `HRD-DEC-133`; `VR-DED-06` | `BE-HRD-173`, `BE-HRD-170` | `FE-HRD-155` | `AT-HRD-DED-19`, `UAT-DED-05` | LENGKAP (0 Gap) |

#### Matriks acceptance test ke task

| Acceptance test | Task | Cakupan Pengujian |
| --- | --- | --- |
| `AT-HRD-DED-01` s.d. `AT-HRD-DED-04` | `BE-HRD-164`, `BE-HRD-165`, `FE-HRD-145`, `FE-HRD-149` | Penegakan batas hard-cap 50% PP 36/2021, hirarki prioritas 1-4, pemotongan parsial, penundaan otomatis (*carry-over*) |
| `AT-HRD-DED-05` s.d. `AT-HRD-DED-08` | `BE-HRD-163`, `BE-HRD-174`, `FE-HRD-146`, `FE-HRD-147` | Sinkronisasi batch Finance H-3 tgl 17, retry timeout, penguncian tgl 20, callback webhook realisasi tgl 25 |
| `AT-HRD-DED-09` s.d. `AT-HRD-DED-11` | `BE-HRD-167`, `FE-HRD-150`, `FE-HRD-151` | Pengajuan klaim mandiri ESS, pelacak kuota plafon tahunan, penolakan over-quota & kuitansi >60 hari |
| `AT-HRD-DED-12`, `AT-HRD-DED-13` | `BE-HRD-168`, `FE-HRD-152`, `FE-HRD-153` | Penegakan anti-self-approval atasan, alur verifikasi berkas 3-tier, penyesuaian nominal disetujui |
| `AT-HRD-DED-14` | `BE-HRD-170`, `FE-HRD-144`, `FE-HRD-157` | Injeksi otomatis klaim approved non-cito ke slip gaji tgl 25 sebagai tunjangan non-pajak |
| `AT-HRD-DED-15`, `AT-HRD-DED-16` | `BE-HRD-171`, `FE-HRD-154` | Restitusi koreksi salah potong masa lalu tanpa revisi slip lama, penautan audit link, bebas pajak PPh 21 |
| `AT-HRD-DED-17` s.d. `AT-HRD-DED-19` | `BE-HRD-172`, `BE-HRD-173`, `FE-HRD-155` | Penerbitan voucher QR kasir cito 24/7, kliring loket kasir, garansi nol risiko bayar ganda di penggajian |
| `AT-HRD-DED-20` | `BE-HRD-174`, `FE-HRD-148`, `FE-HRD-157` | Pelunasan pinjaman PHK/Resign pada pesangon penutupan Exit Clearance Sub-20 & tagihan piutang residu |
| `UAT-DED-01` s.d. `UAT-DED-05` | `BE-HRD-175`, `FE-HRD-157` | Validasi bisnis ujung-ke-ujung seluruh alur restitusi, potongan upah 50%, integrasi Keuangan, kasir cito, dan Exit Clearance |

#### Gap keterkaitan requirement ke bukti verifikasi

| Gap | Status | Catatan / Pemilik |
| --- | :---: | --- |
| Keterkaitan requirement ke bukti verifikasi S-B6 | **LENGKAP** (0 Gap) | Seluruh 25 functional requirement `FR-DED-01` s.d. `FR-DED-25` terpetakan penuh ke 15 task backend (`BE-HRD-161` s.d. `BE-HRD-175`), 14 task frontend (`FE-HRD-144` s.d. `FE-HRD-157`), 20 acceptance tests (`AT-HRD-DED-01` s.d. `AT-HRD-DED-20`), serta 5 skenario UAT (`UAT-DED-01` s.d. `UAT-DED-05`). |

**Kebijakan Uji Backend:** Sesuai `rules/backend/TEST_POLICY.md`, backend tidak menuntut proyek automated test terpisah dan ketiadaan automated test bukan coverage gap. Bukti verifikasi backend dipenuhi lewat verifikasi build `dotnet build`, conformance QBE preflight, review scope/diff, verifikasi kontrak API OpenAPI, dan verifikasi proses bisnis.


### Bagian 1j: Pemetaan Jejak Requirement Bisnis Sub-Modul 12 (Slice S-D3 Benefit Management)

Bagian ini memetakan seluruh rantai keterlacakan (*end-to-end requirement traceability*) untuk **Sub-Modul 12 (Benefit Management — Slice `S-D3`)**, menghubungkan keputusan bisnis (`HRD-DEC-134` s.d. `HRD-DEC-139`), functional requirements (`FR-BEN-01` s.d. `FR-BEN-25`), task backend (`BE-HRD-176` s.d. `BE-HRD-190`), task frontend (`FE-HRD-158` s.d. `FE-HRD-171`), acceptance tests (`AT-HRD-BEN-01` s.d. `AT-HRD-BEN-20`), serta skenario UAT (`UAT-HRD-BEN-01` s.d. `05`).

#### Matriks jejak requirement ke task

| ID FR | Deskripsi Kebutuhan Fungsional | Rujukan Desain / Keputusan | Task Backend | Task Frontend | Bukti Acceptance / UAT | Status Ketercakupan |
| :--- | :--- | :--- | :--- | :--- | :--- | :---: |
| `FR-BEN-01` | Katalog paket manfaat berjenjang 3-lapisan & pagu tahunan | BE A 25.2; `HRD-DEC-134`; PRD 30.3 | `BE-HRD-176`, `BE-HRD-179` | `FE-HRD-158`, `FE-HRD-162` | `AT-HRD-BEN-01`, `UAT-HRD-BEN-01` | LENGKAP (0 Gap) |
| `FR-BEN-02` | Konfigurasi diskon tarif pelayanan RS bagi nakes & keluarga | BE A 25.5; `HRD-DEC-134`; `HRD-ASM-35` | `BE-HRD-177`, `BE-HRD-179` | `FE-HRD-162`, `FE-HRD-167` | `AT-HRD-BEN-01`, `UAT-HRD-BEN-02` | LENGKAP (0 Gap) |
| `FR-BEN-03` | Pemetaan matriks kelayakan paket benefit berdasarkan grade staf | BE A 25.2; `HRD-DEC-134`; `HrdBenefitGradeEligibility` | `BE-HRD-176`, `BE-HRD-179` | `FE-HRD-162`, `FE-HRD-163` | `AT-HRD-BEN-02`, `UAT-HRD-BEN-01` | LENGKAP (0 Gap) |
| `FR-BEN-04` | Penetapan hak kelas kamar rawat inap (Kelas 2, 1, VIP, VVIP) | BE A 25.5; `HRD-DEC-134`; InpatientRoomClass | `BE-HRD-178`, `BE-HRD-180` | `FE-HRD-163`, `FE-HRD-167` | `AT-HRD-BEN-02`, `UAT-HRD-BEN-03` | LENGKAP (0 Gap) |
| `FR-BEN-05` | Penguncian mandatory COB BPJS Kesehatan untuk asuransi swasta | BE A 25.6 (`BEN-B5`); `HRD-DEC-137`; `VR-BEN-16` | `BE-HRD-178`, `BE-HRD-180` | `FE-HRD-162`, `FE-HRD-170` | `AT-HRD-BEN-01`, `UAT-HRD-BEN-03` | LENGKAP (0 Gap) |
| `FR-BEN-06` | Pencatatan nomor kepesertaan BPJS Kes (13 dig), TK (11 dig) & polis swasta | BE A 25.5; `HRD-DEC-138`; `VR-BEN-09`..`10` | `BE-HRD-176`, `BE-HRD-182` | `FE-HRD-158`, `FE-HRD-164` | `AT-HRD-BEN-03`, `UAT-HRD-BEN-04` | LENGKAP (0 Gap) |
| `FR-BEN-07` | Pendaftaran mandiri tanggungan keluarga ESS berlampiran KK/Akta | BE A 25.3; `HRD-DEC-135`; `TrxEmployeeBenefitDependent`| `BE-HRD-181`, `BE-HRD-183` | `FE-HRD-160`, `FE-HRD-166` | `AT-HRD-BEN-07`, `UAT-HRD-BEN-01` | LENGKAP (0 Gap) |
| `FR-BEN-08` | Penegakan batas kuota maksimal 1 pasangan sah dan 3 anak sah | BE A 25.6 (`BEN-B2`); `HRD-DEC-135`; `VR-BEN-01`..`02` | `BE-HRD-181`, `BE-HRD-183` | `FE-HRD-160`, `FE-HRD-166` | `AT-HRD-BEN-04`..`05`, `UAT-HRD-BEN-01` | LENGKAP (0 Gap) |
| `FR-BEN-09` | Batas usia anak <= 21 tahun (non-mahasiswa) & <= 25 tahun (mahasiswa aktif) | BE A 25.6 (`BEN-B3`); `HRD-DEC-135`; `VR-BEN-05`..`08` | `BE-HRD-181`, `BE-HRD-183` | `FE-HRD-160`, `FE-HRD-166` | `AT-HRD-BEN-06`..`07`, `UAT-HRD-BEN-01` | LENGKAP (0 Gap) |
| `FR-BEN-10` | Antrean verifikasi berkas kependudukan fisik & persetujuan HR | BE A 25.3; `HRD-DEC-135`; `VR-BEN-25`; `AUDIT-BEN-05` | `BE-HRD-183`, `BE-HRD-184` | `FE-HRD-165`, `FE-HRD-171` | `AT-HRD-BEN-08`..`09`, `UAT-HRD-BEN-01` | LENGKAP (0 Gap) |
| `FR-BEN-11` | API inquiry eligibilitas online real-time untuk loket admisi SIMRS | `INT-HR-61`; `HRD-DEC-136`; InHouseHealthcareController | `BE-HRD-185`, `BE-HRD-186` | `FE-HRD-158`, `FE-HRD-167` | `AT-HRD-BEN-11`, `UAT-HRD-BEN-02` | LENGKAP (0 Gap) |
| `FR-BEN-12` | Kalkulasi otomatis sisa saldo plafon tahunan & diskon tarif staf RS | BE A 25.3; `HRD-DEC-136`; InHouseBenefitLedgerService | `BE-HRD-185`, `BE-HRD-186` | `FE-HRD-167`, `FE-HRD-171` | `AT-HRD-BEN-11`, `UAT-HRD-BEN-02` | LENGKAP (0 Gap) |
| `FR-BEN-13` | API pemotongan saldo plafon real-time loket kasir SIMRS | `INT-HR-61`; `HRD-DEC-136`; `BEN-W6` row-locking | `BE-HRD-186`, `BE-HRD-187` | `FE-HRD-167`, `FE-HRD-171` | `AT-HRD-BEN-12`, `UAT-HRD-BEN-02` | LENGKAP (0 Gap) |
| `FR-BEN-14` | Pencatatan mutasi buku besar saldo plafon non-negatif (`chk_non_negative`) | BE A 25.6 (`BEN-B4`); `HRD-DEC-136`; `VR-BEN-14`..`15` | `BE-HRD-176`, `BE-HRD-186` | `FE-HRD-167`, `FE-HRD-168` | `AT-HRD-BEN-12`..`13`, `UAT-HRD-BEN-02` | LENGKAP (0 Gap) |
| `FR-BEN-15` | Pendelegasian ekses tagihan medis ke pemotongan payroll Sub-11 | `INT-HR-63`; `HRD-DEC-136`; OnHospitalCareExcessGenerated | `BE-HRD-186`, `BE-HRD-189` | `FE-HRD-167`, `FE-HRD-169` | `AT-HRD-BEN-14`, `UAT-HRD-BEN-02` | LENGKAP (0 Gap) |
| `FR-BEN-16` | Formulir pengajuan klaim biaya kesehatan COB di portal mandiri ESS | BE A 25.3; `HRD-DEC-137`; `TrxBenefitClaim` | `BE-HRD-188`, `BE-HRD-190` | `FE-HRD-160`, `FE-HRD-170` | `AT-HRD-BEN-15`, `UAT-HRD-BEN-03` | LENGKAP (0 Gap) |
| `FR-BEN-17` | Penegakan mandatory nomor SEP BPJS Kesehatan pada klaim COB sekunder | BE A 25.6 (`BEN-B5`); `HRD-DEC-137`; `VR-BEN-16`..`17` | `BE-HRD-188`, `BE-HRD-189` | `FE-HRD-170`, `FE-HRD-171` | `AT-HRD-BEN-15`..`16`, `UAT-HRD-BEN-03` | LENGKAP (0 Gap) |
| `FR-BEN-18` | Validasi konsistensi rincian biaya: tagihan = primer + sekunder + ekses | `contracts/validation-matrix.md` Bagian 21; `VR-BEN-18` | `BE-HRD-188`, `BE-HRD-189` | `FE-HRD-169`, `FE-HRD-170` | `AT-HRD-BEN-15`, `UAT-HRD-BEN-03` | LENGKAP (0 Gap) |
| `FR-BEN-19` | Meja kerja verifikasi klaim sekunder & audit kuitansi naik kelas | BE A 25.3; `HRD-DEC-137`; BenefitClaimVerificationService | `BE-HRD-189`, `BE-HRD-190` | `FE-HRD-169`, `FE-HRD-171` | `AT-HRD-BEN-17`, `UAT-HRD-BEN-03` | LENGKAP (0 Gap) |
| `FR-BEN-20` | Konfirmasi settlement pelunasan klaim sekunder & nomor referensi transfer | BE A 25.5; `HRD-DEC-137`; `AUDIT-BEN-10` | `BE-HRD-189`, `BE-HRD-190` | `FE-HRD-169`, `FE-HRD-171` | `AT-HRD-BEN-17`, `UAT-HRD-BEN-03` | LENGKAP (0 Gap) |
| `FR-BEN-21` | Aktivasi cepat kepesertaan BPJS TK (JKK & JKM) pada H+0 Onboarding | BE A 25.6 (`BEN-B1`); `HRD-DEC-138`; `VR-BEN-11` | `BE-HRD-182`, `BE-HRD-184` | `FE-HRD-164`, `FE-HRD-171` | `AT-HRD-BEN-03`, `UAT-HRD-BEN-04` | LENGKAP (0 Gap) |
| `FR-BEN-22` | Penahanan paket kesehatan selama masa evaluasi percobaan 3 bulan | BE A 25.6 (`BEN-B1`); `HRD-DEC-138`; `VR-BEN-12` | `BE-HRD-182`, `BE-HRD-184` | `FE-HRD-164`, `FE-HRD-171` | `AT-HRD-BEN-03`, `UAT-HRD-BEN-04` | LENGKAP (0 Gap) |
| `FR-BEN-23` | Aktivasi paket lengkap pasca-lulus masa percobaan (`ProbationPassed`) | BE A 25.6 (`BEN-B1`); `HRD-DEC-138`; `VR-BEN-13` | `BE-HRD-182`, `BE-HRD-184` | `FE-HRD-164`, `FE-HRD-171` | `AT-HRD-BEN-10`, `UAT-HRD-BEN-04` | LENGKAP (0 Gap) |
| `FR-BEN-24` | Cron job tahunan reset hangus sisa plafon per 1 Januari (`YearEndExpiryReset`)| BE A 25.6 (`BEN-B6`); `HRD-DEC-134`; `VR-BEN-22` | `BE-HRD-186`, `BE-HRD-190` | `FE-HRD-161`, `FE-HRD-171` | `AT-HRD-BEN-18`, `UAT-HRD-BEN-02` | LENGKAP (0 Gap) |
| `FR-BEN-25` | Terminasi benefit saat keluar & pemblokiran Exit Clearance bila ada tunggakan | `INT-HR-64`; `HRD-DEC-139`; `VR-BEN-23`..`24` | `BE-HRD-189`, `BE-HRD-190` | `FE-HRD-164`, `FE-HRD-171` | `AT-HRD-BEN-19`..`20`, `UAT-HRD-BEN-05` | LENGKAP (0 Gap) |

#### Matriks acceptance test ke task

| Acceptance test | Task | Cakupan Pengujian |
| --- | --- | --- |
| `AT-HRD-BEN-01`, `AT-HRD-BEN-02` | `BE-HRD-176` s.d. `BE-HRD-180`, `FE-HRD-162`, `FE-HRD-163` | Katalog paket manfaat 3-lapisan, pagu limit tahunan, tier kamar rawat inap, dan matriks kelayakan grade staf |
| `AT-HRD-BEN-03`, `AT-HRD-BEN-10` | `BE-HRD-182`, `FE-HRD-164` | Aktivasi cepat BPJS TK H+0 Onboarding, penahanan paket kesehatan probation, dan aktivasi penuh pasca-kelulusan |
| `AT-HRD-BEN-04` s.d. `AT-HRD-BEN-07` | `BE-HRD-181`, `BE-HRD-183`, `FE-HRD-166` | Penegakan batas kuota keluarga (1 pasangan + 3 anak), batas usia 21/25 tahun mahasiswa aktif & mandatory surat dekanat |
| `AT-HRD-BEN-08`, `AT-HRD-BEN-09` | `BE-HRD-183`, `BE-HRD-184`, `FE-HRD-165` | Meja kerja verifikasi berkas scan KK/Akta fisik oleh HR Benefit Officer, approval, dan penolakan ber-catatan |
| `AT-HRD-BEN-11` s.d. `AT-HRD-BEN-14` | `BE-HRD-185`, `BE-HRD-186`, `BE-HRD-187`, `FE-HRD-167`, `FE-HRD-168` | Layanan berobat nakes di RS sendiri via SIMRS `INT-HR-61`, kalkulasi diskon staf, mutasi buku besar plafon non-negatif & ekses tagihan |
| `AT-HRD-BEN-15` s.d. `AT-HRD-BEN-17` | `BE-HRD-188`, `BE-HRD-189`, `BE-HRD-190`, `FE-HRD-169`, `FE-HRD-170` | Pengajuan klaim koordinasi manfaat (COB) di ESS berlampiran mandatory SEP BPJS Kesehatan & settlement pelunasan santunan |
| `AT-HRD-BEN-18` | `BE-HRD-186`, `BE-HRD-190`, `FE-HRD-161` | Eksekusi cron job 1 Januari untuk me-reset sisa saldo plafon tahun lalu menjadi hangus dan alokasi pagu tahun baru |
| `AT-HRD-BEN-19`, `AT-HRD-BEN-20` | `BE-HRD-189`, `BE-HRD-190`, `FE-HRD-164`, `FE-HRD-171` | Penonaktifan kepesertaan pada tanggal efektif keluar, penerbitan surat mutasi BPJS Mandiri & blokir Exit Clearance Sub-20 bila ada tunggakan |
| `UAT-HRD-BEN-01` s.d. `UAT-HRD-BEN-05` | `BE-HRD-190`, `FE-HRD-171` | Pengujian penerimaan pengguna menyeluruh untuk pendaftaran tanggungan, berobat di RS sendiri, klaim COB, aktivasi probation & Exit Clearance |

#### Gap keterkaitan requirement ke bukti verifikasi

| Gap | Status | Catatan / Pemilik |
| --- | :---: | --- |
| Keterkaitan requirement ke bukti verifikasi S-D3 | **LENGKAP** (0 Gap) | Seluruh 25 functional requirement `FR-BEN-01` s.d. `FR-BEN-25` terpetakan penuh ke 15 task backend (`BE-HRD-176` s.d. `BE-HRD-190`), 14 task frontend (`FE-HRD-158` s.d. `FE-HRD-171`), 20 acceptance tests (`AT-HRD-BEN-01` s.d. `AT-HRD-BEN-20`), serta 5 skenario UAT (`UAT-HRD-BEN-01` s.d. `05`). |

**Kebijakan Uji Backend:** Sesuai `rules/backend/TEST_POLICY.md`, backend tidak menuntut proyek automated test terpisah dan ketiadaan automated test bukan coverage gap. Bukti verifikasi backend dipenuhi lewat verifikasi build `dotnet build`, conformance QBE preflight, review scope/diff, verifikasi kontrak API OpenAPI, dan verifikasi proses bisnis.


---

## 1k. Traceability Slice `S-D4` — HR Service Management & Request / Employee Document Service (Sub-modul 13, Kontrak `v17`)

Jejak ketertelusuran penuh antara 25 kebutuhan fungsional (`FR-HRS-01` s.d. `FR-HRS-25`), 6 keputusan bisnis (`HRD-DEC-140` s.d. `HRD-DEC-145`), 4 asumsi (`HRD-ASM-39` s.d. `HRD-ASM-42`), 15 task backend (`BE-HRD-191` s.d. `BE-HRD-205`), 14 task frontend (`FE-HRD-172` s.d. `FE-HRD-185`), 20 acceptance tests (`AT-HRD-HRS-01` s.d. `AT-HRD-HRS-20`), serta 5 skenario UAT (`UAT-HRD-HRS-01` s.d. `UAT-HRD-HRS-05`).

#### Matriks jejak requirement ke task

| ID FR | Deskripsi Kebutuhan Fungsional | Rujukan Desain / Keputusan | Task Backend | Task Frontend | Bukti Acceptance / UAT | Status Ketercakupan |
| :--- | :--- | :--- | :--- | :--- | :--- | :---: |
| `FR-HRS-01` | Master kategori & jenis layanan SDM berparameter SLA jam operasional RS | BE A 26.3; `HRD-DEC-141`; `VR-HRS-03` | `BE-HRD-191`, `BE-HRD-193` | `FE-HRD-172`, `FE-HRD-173` | `AT-HRD-HRS-01`, `UAT-HRD-HRS-01` | LENGKAP (0 Gap) |
| `FR-HRS-02` | Template naskah dinas resmi berbasis variabel token dinamis Liquid/HTML | BE A 26.3; `HRD-DEC-144`; `VR-HRS-16` | `BE-HRD-191`, `BE-HRD-193` | `FE-HRD-173`, `FE-HRD-174` | `AT-HRD-HRS-01`, `UAT-HRD-HRS-03` | LENGKAP (0 Gap) |
| `FR-HRS-03` | Mesin penomoran surat resmi terpusat gapless berbasis locking atomik | BE A 26.6 (`HRS-B3`); `HRD-DEC-144`; `VR-HRS-09`..`10` | `BE-HRD-194`, `BE-HRD-202` | `FE-HRD-174`, `FE-HRD-182` | `AT-HRD-HRS-13`, `UAT-HRD-HRS-03` | LENGKAP (0 Gap) |
| `FR-HRS-04` | Penandaan naskah dinas sensitif finansial (`is_financial_sensitive`) | BE A 26.3; `HRD-DEC-140`; `VR-HRS-05` | `BE-HRD-191`, `BE-HRD-193` | `FE-HRD-174`, `FE-HRD-182` | `AT-HRD-HRS-08`, `UAT-HRD-HRS-03` | LENGKAP (0 Gap) |
| `FR-HRS-05` | Masa berlaku standar surat resmi kepegawaian (default 90 hari) | BE A 26.3; `HRD-ASM-40`; `VR-HRS-13` | `BE-HRD-191`, `BE-HRD-202` | `FE-HRD-174`, `FE-HRD-184` | `AT-HRD-HRS-17`, `UAT-HRD-HRS-04` | LENGKAP (0 Gap) |
| `FR-HRS-06` | Agregat tiket helpdesk SDM berpenomoran unik `TKT-YYYYMM-XXXXX` | BE A 26.5; `FR-HRS-06`; `VR-HRS-04` | `BE-HRD-191`, `BE-HRD-195` | `FE-HRD-172`, `FE-HRD-176` | `AT-HRD-HRS-01`, `UAT-HRD-HRS-01` | LENGKAP (0 Gap) |
| `FR-HRS-07` | Penghitungan deadline jam kerja operasional RS (melompati libur/weekend) | BE A 26.6 (`HRS-B2`); `HRD-DEC-141` | `BE-HRD-195`, `BE-HRD-197` | `FE-HRD-175`, `FE-HRD-176` | `AT-HRD-HRS-03`, `UAT-HRD-HRS-01` | LENGKAP (0 Gap) |
| `FR-HRS-08` | Penugasan antrean analis SDM (*claim to me* / disposisi) & first response | BE A 26.6 (`HRS-W1`); `03-FE` `FE-HRD-180` | `BE-HRD-195`, `BE-HRD-197` | `FE-HRD-176`, `FE-HRD-177` | `AT-HRD-HRS-01`, `UAT-HRD-HRS-02` | LENGKAP (0 Gap) |
| `FR-HRS-09` | Penangguhan (*pause*) & pelanjutan (*resume*) timer SLA saat klarifikasi | BE A 26.6 (`HRS-W2`); State A 21.1 | `BE-HRD-196`, `BE-HRD-197` | `FE-HRD-176`, `FE-HRD-180` | `AT-HRD-HRS-04`..`05`, `UAT-HRD-HRS-01` | LENGKAP (0 Gap) |
| `FR-HRS-10` | Worker pemantau SLA berkala: alert kuning 75% & auto-eskalasi 100% | BE A 26.6 (`HRS-W3`); `INT-HR-68`; `HRD-DEC-141` | `BE-HRD-197`, `BE-HRD-198` | `FE-HRD-175`, `FE-HRD-176` | `AT-HRD-HRS-06`..`07`, `UAT-HRD-HRS-02` | LENGKAP (0 Gap) |
| `FR-HRS-11` | Katalog kartu layanan interaktif (*service tiles*) di portal ESS | FE A 22.2 (`FE-HRD-182`); `FR-HRS-11` | `BE-HRD-193`, `BE-HRD-195` | `FE-HRD-172`, `FE-HRD-178` | `AT-HRD-HRS-01`, `UAT-HRD-HRS-01` | LENGKAP (0 Gap) |
| `FR-HRS-12` | Upload lampiran berkas persyaratan digital terenkripsi (PDF/JPG maks 5MB) | BE A 26.5; `VR-HRS-01`, `VR-HRS-15` | `BE-HRD-195`, `BE-HRD-196` | `FE-HRD-178`, `FE-HRD-179` | `AT-HRD-HRS-01`, `UAT-HRD-HRS-01` | LENGKAP (0 Gap) |
| `FR-HRS-13` | Penguncian mandatory surat disposisi klinis untuk tiket prioritas Cito | BE A 26.6 (`HRS-B1`); `HRD-DEC-141`; `VR-HRS-02` | `BE-HRD-195`, `BE-HRD-196` | `FE-HRD-178`, `FE-HRD-179` | `AT-HRD-HRS-02`, `UAT-HRD-HRS-01` | LENGKAP (0 Gap) |
| `FR-HRS-14` | Utas komunikasi klarifikasi 2-arah staf pemohon & catatan internal HR | BE A 26.5; `FR-HRS-14`; `FE-HRD-183` | `BE-HRD-196`, `BE-HRD-197` | `FE-HRD-179`, `FE-HRD-180` | `AT-HRD-HRS-04`, `UAT-HRD-HRS-01` | LENGKAP (0 Gap) |
| `FR-HRS-15` | Pemutakhiran otomatis data master profil Sub-02 pasca-verifikasi dokumen | `INT-HR-65`; `HRD-DEC-143`; `HRS-B6`; `VR-HRS-25` | `BE-HRD-195`, `BE-HRD-199` | `FE-HRD-180`, `FE-HRD-181` | `AT-HRD-HRS-14`..`15`, `UAT-HRD-HRS-05` | LENGKAP (0 Gap) |
| `FR-HRS-16` | Penarikan otomatis gaji final Sub-10 pada surat gaji & anti-manual input | `INT-HR-56`; `HRD-DEC-142`; `HRS-B4`; `VR-HRS-05`..`06` | `BE-HRD-200`, `BE-HRD-202` | `FE-HRD-180`, `FE-HRD-182` | `AT-HRD-HRS-08`..`10`, `UAT-HRD-HRS-03` | LENGKAP (0 Gap) |
| `FR-HRS-17` | Alur approval 2-tier surat gaji: Reviewer C&B -> Otorisasi Manajer SDM | BE A 26.6 (`HRS-W4`); `HRD-DEC-142`; `VR-HRS-07` | `BE-HRD-200`, `BE-HRD-201` | `FE-HRD-180`, `FE-HRD-182` | `AT-HRD-HRS-11`, `UAT-HRD-HRS-03` | LENGKAP (0 Gap) |
| `FR-HRS-18` | Penegakan aturan mutlak Anti-Self-Approval otorisasi surat resmi | `HRD-DEC-142`; `VR-HRS-08`; State A 21.3 | `BE-HRD-201`, `BE-HRD-202` | `FE-HRD-182` | `AT-HRD-HRS-12`, `UAT-HRD-HRS-03` | LENGKAP (0 Gap) |
| `FR-HRS-19` | Penerbitan naskah dinas resmi PDF, nomor seri gapless & QR Code unik | `HRD-DEC-144`; `VR-HRS-24`; `FE-HRD-184` | `BE-HRD-194`, `BE-HRD-202`, `BE-HRD-203` | `FE-HRD-182` | `AT-HRD-HRS-13`, `UAT-HRD-HRS-03` | LENGKAP (0 Gap) |
| `FR-HRS-20` | Generator 4-digit PIN acak terenkripsi BCrypt untuk proteksi data gaji | `HRD-DEC-140`; `VR-HRS-11`; `HRD-ASM-41` | `BE-HRD-202`, `BE-HRD-203` | `FE-HRD-182`, `FE-HRD-185` | `AT-HRD-HRS-18`, `UAT-HRD-HRS-03` | LENGKAP (0 Gap) |
| `FR-HRS-21` | Pengarsipan otomatis salinan surat resmi terbit ke E-Dossier Sub-02 | `INT-HR-66`; `HRD-DEC-145`; `HRS-B8` | `BE-HRD-202`, `BE-HRD-205` | `FE-HRD-182`, `FE-HRD-185` | `AT-HRD-HRS-16`, `UAT-HRD-HRS-03` | LENGKAP (0 Gap) |
| `FR-HRS-22` | Portal publik verifikasi QR `/verify/document/{hash}` tanpa sesi login | `HRD-DEC-140`; `FE-HRD-185`; `VR-HRS-24` | `BE-HRD-203`, `BE-HRD-204` | `FE-HRD-172`, `FE-HRD-184` | `AT-HRD-HRS-17`, `UAT-HRD-HRS-04` | LENGKAP (0 Gap) |
| `FR-HRS-23` | Masking nominal gaji di portal publik & pembukaan rincian via PIN 4-digit | `HRD-DEC-140`; `HRS-B5`; PRD 31.3 | `BE-HRD-203`, `BE-HRD-204` | `FE-HRD-184` | `AT-HRD-HRS-17`..`18`, `UAT-HRD-HRS-04` | LENGKAP (0 Gap) |
| `FR-HRS-24` | Rate limiting salah input PIN 3 kali per jam per IP (kunci 30 menit) | `VR-HRS-12`; `02-BE` 26.6 | `BE-HRD-204` | `FE-HRD-184` | `AT-HRD-HRS-19`, `UAT-HRD-HRS-04` | LENGKAP (0 Gap) |
| `FR-HRS-25` | Fitur pencabutan keabsahan dokumen (revokasi) ber-BAP & tanda merah portal | `HRD-DEC-145`; `HRS-B7`; `HRS-W5`; `VR-HRS-14` | `BE-HRD-204`, `BE-HRD-205` | `FE-HRD-183`, `FE-HRD-184`, `FE-HRD-185` | `AT-HRD-HRS-20`, `UAT-HRD-HRS-05` | LENGKAP (0 Gap) |

#### Matriks acceptance test ke task

| Acceptance test | Task | Cakupan Pengujian |
| --- | --- | --- |
| `AT-HRD-HRS-01`, `AT-HRD-HRS-02` | `BE-HRD-195`, `BE-HRD-196`, `FE-HRD-178`, `FE-HRD-179` | Pengajuan tiket mandiri ESS, upload lampiran persyaratan, dan penolakan prioritas Cito tanpa disposisi pimpinan |
| `AT-HRD-HRS-03` s.d. `AT-HRD-HRS-05` | `BE-HRD-197`, `FE-HRD-176`, `FE-HRD-180` | Inisiasi timer jam kerja RS, penangguhan (*pause*) saat minta klarifikasi berkas, dan pelanjutan (*resume*) saat staf membalas |
| `AT-HRD-HRS-06`, `AT-HRD-HRS-07` | `BE-HRD-198`, `FE-HRD-175`, `FE-HRD-176` | Background worker berkala tiap 15 menit mendeteksi sisa waktu, memicu alert kuning 75%, dan auto-eskalasi 100% keterlambatan |
| `AT-HRD-HRS-08` s.d. `AT-HRD-HRS-10` | `BE-HRD-200`, `BE-HRD-202`, `FE-HRD-182` | Penarikan nominal gaji otomatis dari slip gaji final Sub-10, penolakan jika belum finalized, dan proteksi anti-manual input |
| `AT-HRD-HRS-11`, `AT-HRD-HRS-12` | `BE-HRD-201`, `FE-HRD-182` | Penegakan gerbang persetujuan 2-tier Supervisor C&B serta penolakan otorisasi surat mandiri oleh pejabat (Anti-Self-Approval) |
| `AT-HRD-HRS-13` | `BE-HRD-194`, `BE-HRD-202`, `FE-HRD-174`, `FE-HRD-182` | Integritas mesin penomoran surat seri resmi gapless sequence saat multi-user konkurensi paralel |
| `AT-HRD-HRS-14`, `AT-HRD-HRS-15` | `BE-HRD-199`, `FE-HRD-181` | Pemutakhiran otomatis data rekening payroll/master kepegawaian Sub-02 pasca-approval tiket sah dan rekam log audit snapshot |
| `AT-HRD-HRS-16` | `BE-HRD-205`, `FE-HRD-185` | Pengarsipan otomatis salinan berkas PDF resmi terbit ke repositori digital *Employee E-Dossier* Sub-02 dan unduh mandiri ESS |
| `AT-HRD-HRS-17`, `AT-HRD-HRS-18` | `BE-HRD-203`, `BE-HRD-204`, `FE-HRD-184` | Pemindaian QR Code publik menampilkan status sah dengan nominal gaji tersamar, dan pembukaan rincian nominal via 4-digit PIN |
| `AT-HRD-HRS-19` | `BE-HRD-204`, `FE-HRD-184` | Pembatasan percobaan input PIN salah di portal publik maksimal 3 kali per jam per IP sebelum penguncian 30 menit (anti-brute force) |
| `AT-HRD-HRS-20` | `BE-HRD-205`, `FE-HRD-183`, `FE-HRD-184`, `FE-HRD-185` | Protokol pencabutan surat resmi terbit ber-BAP, penyematan watermark revoked di e-Dossier, dan banner merah penolakan di portal publik |
| `UAT-HRD-HRS-01` s.d. `UAT-HRD-HRS-05` | `BE-HRD-205`, `FE-HRD-185` | Validasi bisnis ujung-ke-ujung seluruh alur tiket helpdesk, SLA jam kerja RS, surat gaji KPR, verifikasi QR publik & revokasi |

#### Gap keterkaitan requirement ke bukti verifikasi

| Gap | Status | Catatan / Pemilik |
| --- | :--- :--- | :--- |
| Keterkaitan requirement ke bukti verifikasi S-D4 | **LENGKAP** (0 Gap) | Seluruh 25 functional requirement `FR-HRS-01` s.d. `FR-HRS-25` terpetakan penuh ke 15 task backend (`BE-HRD-191` s.d. `BE-HRD-205`), 14 task frontend (`FE-HRD-172` s.d. `FE-HRD-185`), 20 acceptance tests (`AT-HRD-HRS-01` s.d. `AT-HRD-HRS-20`), serta 5 skenario UAT (`UAT-HRD-HRS-01` s.d. `05`). |

**Kebijakan Uji Backend:** Sesuai `rules/backend/TEST_POLICY.md`, backend tidak menuntut proyek automated test terpisah dan ketiadaan automated test bukan coverage gap. Bukti verifikasi backend dipenuhi lewat verifikasi build `dotnet build`, conformance QBE preflight, review scope/diff, verifikasi kontrak API OpenAPI, dan verifikasi proses bisnis.


---

## 1l. Traceability Slice `S-C1` — Credentialing, License & Clinical Privilege (Sub-modul 14, Kontrak `v18`)

Jejak ketertelusuran penuh antara 25 kebutuhan fungsional (`FR-CRD-01` s.d. `FR-CRD-25`), 6 keputusan bisnis (`HRD-DEC-146` s.d. `HRD-DEC-151`), penuntasan posisi `HRD-DEC-005`, 4 asumsi (`HRD-ASM-43` s.d. `HRD-ASM-46`), 16 task backend (`BE-HRD-206` s.d. `BE-HRD-221`), 15 task frontend (`FE-HRD-186` s.d. `FE-HRD-200`), 22 acceptance tests (`AT-HRD-CRD-01` s.d. `AT-HRD-CRD-22`), serta 5 skenario UAT (`UAT-CRD-01` s.d. `UAT-CRD-05`).

#### Matriks jejak requirement ke task

| ID FR | Deskripsi Kebutuhan Fungsional | Rujukan Desain / Keputusan | Task Backend | Task Frontend | Bukti Acceptance / UAT | Status Ketercakupan |
| :--- | :--- | :--- | :--- | :--- | :--- | :---: |
| `FR-CRD-01` | Pendaftaran dokumen STR seumur hidup & SIP 5 tahunan faskes RS via ESS | BE § 27.5; `HRD-DEC-146`; `VR-CRD-01` | `BE-HRD-206`, `BE-HRD-210` | `FE-HRD-186`, `FE-HRD-187` | `AT-HRD-CRD-01`, `UAT-CRD-01` | **TERCAKUP PENUH** |
| `FR-CRD-02` | Audit verifikasi sumber primer (PSV) ke portal KKI / SatuSehat SDMK Kemenkes | BE § 27.3; `HRD-DEC-146`; `VR-CRD-02` | `BE-HRD-208`, `BE-HRD-210` | `FE-HRD-186`, `FE-HRD-188` | `AT-HRD-CRD-01`, `AT-HRD-CRD-02`, `UAT-CRD-01` | **TERCAKUP PENUH** |
| `FR-CRD-03` | Validasi masa berlaku SIP maksimal 5 tahun sejak tanggal terbit izin Dinkes | BE § 27.6; `HRD-DEC-146`; `VR-CRD-03` | `BE-HRD-207`, `BE-HRD-210` | `FE-HRD-187`, `FE-HRD-189` | `AT-HRD-CRD-03`, `AT-HRD-CRD-04` | **TERCAKUP PENUH** |
| `FR-CRD-04` | Background worker radar pemantau kedaluwarsa SIP H-90, H-60, H-30, H-7 | BE § 27.6; `HRD-DEC-146`; `VR-CRD-20` | `BE-HRD-209`, `BE-HRD-210` | `FE-HRD-188`, `FE-HRD-189` | `AT-HRD-CRD-05`, `UAT-CRD-01` | **TERCAKUP PENUH** |
| `FR-CRD-05` | Fail-closed H-0: Nonaktif akun order SIMRS (`INT-HR-69`) & blokir jadwal Sub-06 (`INT-HR-71`) | BE § 27.6; `HRD-DEC-146`; `CRD-B2` | `BE-HRD-209`, `BE-HRD-216` | `FE-HRD-189`, `FE-HRD-196` | `AT-HRD-CRD-06`, `UAT-CRD-01` | **TERCAKUP PENUH** |
| `FR-CRD-06` | Pemisahan alur kredensialing 3 komite mandiri (Medik, Keperawatan, Nakes Lain) | BE § 27.1; `HRD-DEC-147`; `VR-CRD-10` | `BE-HRD-206`, `BE-HRD-211` | `FE-HRD-187`, `FE-HRD-190` | `AT-HRD-CRD-07`, `UAT-CRD-02` | **TERCAKUP PENUH** |
| `FR-CRD-07` | Katalog whitebook kompetensi klinis terisolasi per komite profesi | BE § 27.3; `HRD-DEC-147` | `BE-HRD-207`, `BE-HRD-211` | `FE-HRD-187`, `FE-HRD-190` | `AT-HRD-CRD-07`, `UAT-CRD-02` | **TERCAKUP PENUH** |
| `FR-CRD-08` | Penunjukan panel Mitra Bestari (minimal 2 reviewer sejawat sebidang) | BE § 27.3; `HRD-DEC-147`; `VR-CRD-11` | `BE-HRD-212`, `BE-HRD-215` | `FE-HRD-190`, `FE-HRD-191` | `AT-HRD-CRD-08`, `UAT-CRD-02` | **TERCAKUP PENUH** |
| `FR-CRD-09` | Penegakan aturan anti-self-review panel Mitra Bestari | BE § 27.6; `HRD-DEC-147`; `VR-CRD-15` | `BE-HRD-212` | `FE-HRD-191` | `AT-HRD-CRD-08` | **TERCAKUP PENUH** |
| `FR-CRD-10` | Rekomendasi derajat wewenang panel (4 Mandiri, 3 Supervisi, 1-2 Ditolak) | BE § 27.6; `HRD-DEC-147` | `BE-HRD-213`, `BE-HRD-215` | `FE-HRD-191`, `FE-HRD-192` | `AT-HRD-CRD-09`, `UAT-CRD-02` | **TERCAKUP PENUH** |
| `FR-CRD-11` | Sidang pleno komite konsolidasi penilaian & penerbitan usulan Ketua Komite | BE § 27.3; `HRD-DEC-147` | `BE-HRD-213`, `BE-HRD-215` | `FE-HRD-192` | `AT-HRD-CRD-10`, `UAT-CRD-02` | **TERCAKUP PENUH** |
| `FR-CRD-12` | Draf SPK wajib menetapkan nama dokter supervisor untuk tindakan Level 3 | BE § 27.6; `HRD-DEC-148`; `VR-CRD-05` | `BE-HRD-214`, `BE-HRD-215` | `FE-HRD-192`, `FE-HRD-193` | `AT-HRD-CRD-11`, `UAT-CRD-03` | **TERCAKUP PENUH** |
| `FR-CRD-13` | Paraf telaah kesesuaian operasional layanan oleh Direktur Pelayanan Medik | BE § 27.3; `HRD-DEC-148` | `BE-HRD-214`, `BE-HRD-215` | `FE-HRD-193` | `AT-HRD-CRD-12`, `UAT-CRD-03` | **TERCAKUP PENUH** |
| `FR-CRD-14` | Pengesahan tunggal dan tanda tangan elektronik (e-Sign) oleh Direktur Utama RS | BE § 27.3; `HRD-DEC-148`; `VR-CRD-12` | `BE-HRD-214`, `BE-HRD-215` | `FE-HRD-193` | `AT-HRD-CRD-13`, `AT-HRD-CRD-14`, `UAT-CRD-03` | **TERCAKUP PENUH** |
| `FR-CRD-15` | Generate nomor register gapless, PDF ber-QR Code & auto-arsip e-Dossier Sub-02 | BE § 27.3; `HRD-DEC-148`; `INT-HR-72` | `BE-HRD-214`, `BE-HRD-215` | `FE-HRD-193`, `FE-HRD-194` | `AT-HRD-CRD-15`, `AT-HRD-CRD-16`, `UAT-CRD-03` | **TERCAKUP PENUH** |
| `FR-CRD-16` | Layanan gerbang real-time SIMRS (`INT-HR-69`) ber-cache Redis latensi < 50ms | BE § 27.3; `HRD-DEC-149`; `HRD-ASM-45` | `BE-HRD-216` | `FE-HRD-186`, `FE-HRD-195` | `AT-HRD-CRD-17` | **TERCAKUP PENUH** |
| `FR-CRD-17` | SIMRS mengizinkan langsung order tindakan mandiri (Level 4) | BE § 27.6; `HRD-DEC-149` | `BE-HRD-216` | `FE-HRD-195` | `AT-HRD-CRD-17`, `UAT-CRD-04` | **TERCAKUP PENUH** |
| `FR-CRD-18` | Penahanan order tindakan Level 3 supervisi & alur co-signature supervisor 24 jam | BE § 27.6; `HRD-DEC-149`; `VR-CRD-07` | `BE-HRD-217` | `FE-HRD-197` | `AT-HRD-CRD-18`, `UAT-CRD-04` | **TERCAKUP PENUH** |
| `FR-CRD-19` | Penolakan keras (Hard-Stop) order tindakan tidak berwenang / lisensi kedaluwarsa | BE § 27.6; Resolusi `HRD-DEC-005` & `HRD-DEC-149` | `BE-HRD-216` | `FE-HRD-196` | `AT-HRD-CRD-19`, `UAT-CRD-04` | **TERCAKUP PENUH** |
| `FR-CRD-20` | Tombol bypass darurat penyelamatan nyawa (Life-Saving Override) & alert audit KMKP | BE § 27.5; `HRD-DEC-149`; `VR-CRD-08` | `BE-HRD-218` | `FE-HRD-198` | `AT-HRD-CRD-20`, `UAT-CRD-05` | **TERCAKUP PENUH** |
| `FR-CRD-21` | Otorisasi kewenangan sementara (*Temporary Privilege*) dokter tamu maks 3 bulan | BE § 27.6; `HRD-DEC-150`; `VR-CRD-13` | `BE-HRD-219` | `FE-HRD-199` | `AT-HRD-CRD-21` | **TERCAKUP PENUH** |
| `FR-CRD-22` | Otorisasi kewenangan darurat (*Emergency Privilege*) bencana cito maks 72 jam | BE § 27.6; `HRD-DEC-150`; `VR-CRD-09` | `BE-HRD-219` | `FE-HRD-199` | `AT-HRD-CRD-22` | **TERCAKUP PENUH** |
| `FR-CRD-23` | Penguncian masa berlaku SPK/RKK standar selama 3 tahun sejak pengesahan Dirut | BE § 27.6; `HRD-DEC-151`; `VR-CRD-14` | `BE-HRD-214`, `BE-HRD-220` | `FE-HRD-193`, `FE-HRD-199` | `AT-HRD-CRD-13`, `UAT-CRD-05` | **TERCAKUP PENUH** |
| `FR-CRD-24` | Radar rekredensialing H-90, inisiasi draf H-60 & integrasi nilai OPPE Sub-15 | BE § 27.6; `HRD-DEC-151`; `INT-HR-70` | `BE-HRD-220` | `FE-HRD-199` | `UAT-CRD-05` | **TERCAKUP PENUH** |
| `FR-CRD-25` | Eksekusi auto-suspended kewenangan nakes tepat H-0 jika rekredensialing belum sah | BE § 27.6; `HRD-DEC-151`; `VR-CRD-16` | `BE-HRD-220` | `FE-HRD-199` | `UAT-CRD-05` | **TERCAKUP PENUH** |

---

#### Matriks acceptance test ke task

| ID Skenario Uji | Deskripsi Skenario Uji | Task Backend Terkait | Task Frontend Terkait | Status Rencana |
| :--- | :--- | :--- | :--- | :---: |
| `AT-HRD-CRD-01` | Pendaftaran STR seumur hidup & verifikasi PSV Kemenkes | `BE-HRD-208`, `BE-HRD-210` | `FE-HRD-186`, `FE-HRD-188` | ✅ PASS |
| `AT-HRD-CRD-02` | Penolakan disposisi berkas jika STR belum terverifikasi PSV | `BE-HRD-208`, `BE-HRD-210` | `FE-HRD-188` | ✅ PASS |
| `AT-HRD-CRD-03` | Registrasi lisensi SIP 5 tahunan faskes rumah sakit | `BE-HRD-206`, `BE-HRD-210` | `FE-HRD-187`, `FE-HRD-189` | ✅ PASS |
| `AT-HRD-CRD-04` | Penolakan masa berlaku SIP melebihi batas 5 tahun | `BE-HRD-207`, `BE-HRD-210` | `FE-HRD-187`, `FE-HRD-189` | ✅ PASS |
| `AT-HRD-CRD-05` | Eksekusi radar peringatan dini SIP H-90, H-60, H-30, H-7 | `BE-HRD-209` | `FE-HRD-188`, `FE-HRD-189` | ✅ PASS |
| `AT-HRD-CRD-06` | Eksekusi fail-closed H-0: SIP kedaluwarsa | `BE-HRD-209`, `BE-HRD-216` | `FE-HRD-189`, `FE-HRD-196` | ✅ PASS |
| `AT-HRD-CRD-07` | Pengajuan berkas kredensial dengan pemisahan 3 komite | `BE-HRD-211`, `BE-HRD-212` | `FE-HRD-187`, `FE-HRD-190` | ✅ PASS |
| `AT-HRD-CRD-08` | Penugasan panel Mitra Bestari & blokir anti-self-review | `BE-HRD-212` | `FE-HRD-190`, `FE-HRD-191` | ✅ PASS |
| `AT-HRD-CRD-09` | Asesmen portofolio oleh panel Mitra Bestari | `BE-HRD-213` | `FE-HRD-191`, `FE-HRD-192` | ✅ PASS |
| `AT-HRD-CRD-10` | Sidang pleno komite menyetujui rekomendasi kewenangan | `BE-HRD-213`, `BE-HRD-215` | `FE-HRD-192` | ✅ PASS |
| `AT-HRD-CRD-11` | Keharusan penetapan dokter supervisor untuk Level 3 | `BE-HRD-214` | `FE-HRD-192`, `FE-HRD-193` | ✅ PASS |
| `AT-HRD-CRD-12` | Paraf manajerial Direktur Pelayanan Medik / Keperawatan | `BE-HRD-214`, `BE-HRD-215` | `FE-HRD-193` | ✅ PASS |
| `AT-HRD-CRD-13` | Pengesahan tunggal SPK & RKK oleh Direktur Utama RS | `BE-HRD-214`, `BE-HRD-215` | `FE-HRD-193` | ✅ PASS |
| `AT-HRD-CRD-14` | Penolakan pengesahan SPK oleh selain Direktur Utama | `BE-HRD-214` | `FE-HRD-193` | ✅ PASS |
| `AT-HRD-CRD-15` | Auto-arsip dokumen sah SPK ke e-Dossier Sub-02 | `BE-HRD-214` | `FE-HRD-193` | ✅ PASS |
| `AT-HRD-CRD-16` | Verifikasi keabsahan dokumen via pemindaian QR Code | `BE-HRD-215` | `FE-HRD-194` | ✅ PASS |
| `AT-HRD-CRD-17` | Evaluasi wewenang mandiri Level 4 di SIMRS EMR | `BE-HRD-216` | `FE-HRD-186`, `FE-HRD-195` | ✅ PASS |
| `AT-HRD-CRD-18` | Evaluasi wewenang supervisi Level 3 & alur co-signature | `BE-HRD-217` | `FE-HRD-197` | ✅ PASS |
| `AT-HRD-CRD-19` | Blokir keras order tindakan klinis tidak berwenang | `BE-HRD-216` | `FE-HRD-196` | ✅ PASS |
| `AT-HRD-CRD-20` | Bypass darurat cito penyelamatan nyawa (Life-Saving Override) | `BE-HRD-218` | `FE-HRD-198` | ✅ PASS |
| `AT-HRD-CRD-21` | Penerbitan kewenangan klinis sementara dokter konsultan tamu | `BE-HRD-219` | `FE-HRD-199` | ✅ PASS |
| `AT-HRD-CRD-22` | Penerbitan kewenangan darurat cito bencana massal | `BE-HRD-219` | `FE-HRD-199` | ✅ PASS |

---

#### Gap keterkaitan requirement ke bukti verifikasi

- **Total Kebutuhan Fungsional (`FR-CRD-01` s.d. `FR-CRD-25`):** 25 Kebutuhan.
- **Kebutuhan yang Tercakup Penuh oleh Task BE & FE:** 25 Kebutuhan (100%).
- **Kebutuhan yang Belum Tercakup (Gap):** **0 KEBUTUHAN (0 GAP)**.
- **Kesimpulan Status Traceability:** Seluruh 25 kebutuhan bisnis rumah sakit, 6 keputusan otoritas klinis, 22 skenario acceptance tests, dan 5 skenario UAT telah terpetakan secara lengkap dan deterministik ke dalam 16 task backend (`BE-HRD-206` s.d. `BE-HRD-221`) dan 15 task frontend (`FE-HRD-186` s.d. `FE-HRD-200`).

---

### Bagian 1m: Pemetaan Jejak Requirement Bisnis Sub-Modul 15 (Slice `S-C1-OPPE` OPPE, FPPE & Recredentialing)

Bagian ini memetakan seluruh rantai keterlacakan (*end-to-end requirement traceability*) untuk **Sub-Modul 15 (OPPE, FPPE & Recredentialing - Slice `S-C1-OPPE`)**, menghubungkan keputusan bisnis (`HRD-DEC-152` s.d. `HRD-DEC-157`), functional requirements (`FR-OPP-01` s.d. `FR-OPP-25`), task backend (`BE-HRD-222` s.d. `BE-HRD-237`), task frontend (`FE-HRD-201` s.d. `FE-HRD-215`), acceptance tests (`AT-HRD-OPP-01` s.d. `AT-HRD-OPP-22`), serta skenario UAT (`UAT-OPP-01` s.d. `05`).

#### Matriks jejak requirement ke task

| ID Kebutuhan | Deskripsi Kebutuhan Fungsional | Dasar Desain & Keputusan | Task Backend Terkait | Task Frontend Terkait | Skenario UAT | Status Traceability |
| :--- | :--- | :--- | :--- | :--- | :---: | :---: |
| `FR-OPP-01` | Master katalog indikator OPPE 6 pilar kompetensi standar ACGME/KARS | BE A § 28.1; `HRD-DEC-152`; `VR-OPP-01` | `BE-HRD-222`, `BE-HRD-223` | `FE-HRD-201`, `FE-HRD-203` | `UAT-OPP-01` | **TERCAKUP PENUH** |
| `FR-OPP-02` | Konfigurasi target ambang batas numerik dan pembobotan per KSM | BE A § 28.1; `HRD-DEC-152`; `VR-OPP-01` | `BE-HRD-223` | `FE-HRD-203`, `FE-HRD-204` | `UAT-OPP-01` | **TERCAKUP PENUH** |
| `FR-OPP-03` | Pembukaan periode siklus evaluasi OPPE tahunan atau semesteran | BE A § 28.2; `HRD-DEC-152`; `VR-OPP-24` | `BE-HRD-224` | `FE-HRD-203` | `UAT-OPP-01` | **TERCAKUP PENUH** |
| `FR-OPP-04` | Validasi komposisi total bobot evaluasi kuantitatif + kualitatif tepat 100% | BE A § 28.2; `HRD-DEC-152`; `VR-OPP-01` | `BE-HRD-224` | `FE-HRD-203` | `UAT-OPP-01` | **TERCAKUP PENUH** |
| `FR-OPP-05` | Inisiasi otomatis berkas rapor evaluasi individu bagi dokter aktif di KSM | BE A § 28.3; `HRD-DEC-152`; `VR-OPP-01` | `BE-HRD-224` | `FE-HRD-202`, `FE-HRD-203` | `UAT-OPP-01` | **TERCAKUP PENUH** |
| `FR-OPP-06` | Adapter ekstraksi data indikator kuantitatif dari database SIMRS & KMKP | BE A § 28.4; `HRD-DEC-152`; `INT-HR-73` | `BE-HRD-225` | `FE-HRD-202` | `UAT-OPP-01` | **TERCAKUP PENUH** |
| `FR-OPP-07` | Kalkulasi rasio kepatuhan pengisian resume medis EMR 1x24 jam pasca-pulang | BE A § 28.4; `HRD-DEC-152`; `VR-OPP-07` | `BE-HRD-225` | `FE-HRD-202` | `UAT-OPP-01` | **TERCAKUP PENUH** |
| `FR-OPP-08` | Evaluasi kepatuhan Clinical Pathway dan peresepan formularium RS | BE A § 28.4; `HRD-DEC-152`; `INT-HR-73` | `BE-HRD-225` | `FE-HRD-202` | `UAT-OPP-01` | **TERCAKUP PENUH** |
| `FR-OPP-09` | Agregasi bobot kuantitatif SIMRS baku 60% terhadap nilai komposit OPPE | BE A § 28.3; `HRD-DEC-152`; `VR-OPP-01` | `BE-HRD-225`, `BE-HRD-228` | `FE-HRD-202` | `UAT-OPP-01` | **TERCAKUP PENUH** |
| `FR-OPP-10` | Early warning alert visual jika indikator klinis jatuh di bawah standar KSM | BE A § 28.3; `HRD-DEC-152`; `VR-OPP-06` | `BE-HRD-228`, `BE-HRD-236` | `FE-HRD-202` | `UAT-OPP-01` | **TERCAKUP PENUH** |
| `FR-OPP-11` | Penugasan instrumen telaah sejawat (peer review) dalam kelompok KSM sebidang | BE A § 28.4; `HRD-DEC-152`; `VR-OPP-02` | `BE-HRD-226` | `FE-HRD-205` | `UAT-OPP-01` | **TERCAKUP PENUH** |
| `FR-OPP-12` | Blokir mutlak anti-self-review pada pengisian telaah sejawat | BE A § 28.4; `HRD-DEC-152`; `VR-OPP-03` | `BE-HRD-226` | `FE-HRD-205` | `UAT-OPP-01` | **TERCAKUP PENUH** |
| `FR-OPP-13` | Penilaian skala Likert 1-5 etika profesi, komunikasi & partisipasi audit M&M | BE A § 28.4; `HRD-DEC-152`; `VR-OPP-04` | `BE-HRD-226` | `FE-HRD-205` | `UAT-OPP-01` | **TERCAKUP PENUH** |
| `FR-OPP-14` | Kewajiban pengisian catatan naratif klinis jika skor Likert <= 2 | BE A § 28.4; `HRD-DEC-152`; `VR-OPP-04` | `BE-HRD-226` | `FE-HRD-205` | `UAT-OPP-01` | **TERCAKUP PENUH** |
| `FR-OPP-15` | Penegakan Peer Review Privilege & perekaman read-audit logging tamper-proof | BE A § 28.8; `HRD-DEC-156`; `VR-OPP-23` | `BE-HRD-227` | `FE-HRD-206` | `UAT-OPP-02` | **TERCAKUP PENUH** |
| `FR-OPP-16` | Deteksi 1 kasus insiden sentinel otomatis memicu penugasan investigasi FPPE | BE A § 28.5; `HRD-DEC-153`; `VR-OPP-05` | `BE-HRD-229`, `BE-HRD-230` | `FE-HRD-207`, `FE-HRD-208` | `UAT-OPP-03` | **TERCAKUP PENUH** |
| `FR-OPP-17` | Deteksi komplikasi bedah > 2 SD & penundaan resume EMR kronis pemicu FPPE | BE A § 28.5; `HRD-DEC-153`; `VR-OPP-06` | `BE-HRD-229`, `BE-HRD-236` | `FE-HRD-207` | `UAT-OPP-03` | **TERCAKUP PENUH** |
| `FR-OPP-18` | Penugasan proctoring dokter baru (3 bln / 10 kasus) & prosedur baru (6 bln / 5 kasus) | BE A § 28.5; `HRD-DEC-154`; `VR-OPP-08`..`10` | `BE-HRD-230` | `FE-HRD-208`, `FE-HRD-209` | `UAT-OPP-04` | **TERCAKUP PENUH** |
| `FR-OPP-19` | Buku catatan digital (logbook) kasus proctoring dengan pseudonimisasi pasien | BE A § 28.6; `HRD-DEC-154`; `VR-OPP-11` | `BE-HRD-231` | `FE-HRD-210` | `UAT-OPP-04` | **TERCAKUP PENUH** |
| `FR-OPP-20` | Antarmuka verifikasi pengesahan kemandirian tindakan medis oleh Dokter Proctor | BE A § 28.6; `HRD-DEC-154`; `VR-OPP-12` | `BE-HRD-231` | `FE-HRD-210` | `UAT-OPP-04` | **TERCAKUP PENUH** |
| `FR-OPP-21` | Sidang pleno Subkomite Mutu Profesi menetapkan vonis kesimpulan & rencana pembinaan | BE A § 28.7; `HRD-DEC-154`; `VR-OPP-13`..`14` | `BE-HRD-232` | `FE-HRD-211` | `UAT-OPP-04` | **TERCAKUP PENUH** |
| `FR-OPP-22` | Kalkulator komposit 3 tahun rekredensialing dengan rasio baku 25%-35%-40% | BE A § 28.7; `HRD-DEC-155`; `VR-OPP-15` | `BE-HRD-233` | `FE-HRD-212` | `UAT-OPP-05` | **TERCAKUP PENUH** |
| `FR-OPP-23` | Klasifikasi eligibilitas otomatis ke dalam Tier Hijau, Tier Kuning, atau Tier Merah | BE A § 28.7; `HRD-DEC-155`; `VR-OPP-16`..`18` | `BE-HRD-233`, `BE-HRD-234` | `FE-HRD-212`, `FE-HRD-213` | `UAT-OPP-05` | **TERCAKUP PENUH** |
| `FR-OPP-24` | Penetapan 4 putusan sidang rekredensialing (Perpanjang, Upgrade, Restricted, Revoked) | BE A § 28.7; `HRD-DEC-157`; `VR-OPP-20` | `BE-HRD-235` | `FE-HRD-214` | `UAT-OPP-05` | **TERCAKUP PENUH** |
| `FR-OPP-25` | Dispatch sinkronisasi wewenang ke SIMRS EMR & penegakan Hard-Stop lockout | BE A § 28.7; `HRD-DEC-157`; `VR-OPP-21`..`22` | `BE-HRD-235` | `FE-HRD-214` | `UAT-OPP-05` | **TERCAKUP PENUH** |

---

#### Matriks acceptance test ke task

| ID Skenario Uji | Deskripsi Skenario Uji | Task Backend Terkait | Task Frontend Terkait | Status Rencana |
| :--- | :--- | :--- | :--- | :---: |
| `AT-HRD-OPP-01` | Pembukaan siklus periode OPPE baru dengan rasio 60:40 | `BE-HRD-223`, `BE-HRD-224` | `FE-HRD-201`, `FE-HRD-203` | ✅ PASS |
| `AT-HRD-OPP-02` | Penolakan konfigurasi bobot OPPE jika total tidak 100% | `BE-HRD-224` | `FE-HRD-203` | ✅ PASS |
| `AT-HRD-OPP-03` | Agregasi otomatis indikator kuantitatif dari SIMRS/KMKP | `BE-HRD-225` | `FE-HRD-202` | 🟡 `NOT RUN` 11 November 2026 — konstanta dihapus; 4 tipe indikator ditarik dari tabel pelayanan aplikasi (`OppeSimrsMetricExtractor`), KMKP/clinical pathway dilaporkan tidak tersedia |
| `AT-HRD-OPP-04` | Penugasan formulir telaah sejawat ke dokter rekan KSM | `BE-HRD-226` | `FE-HRD-205` | ✅ PASS |
| `AT-HRD-OPP-05` | Penolakan penilaian sejawat mandiri (anti-self-review) | `BE-HRD-226` | `FE-HRD-205` | ✅ PASS |
| `AT-HRD-OPP-06` | Keharusan narasi klinis jika skor telaah sejawat <= 2 | `BE-HRD-226` | `FE-HRD-205` | ✅ PASS |
| `AT-HRD-OPP-07` | Penegakan Peer Review Privilege & perekaman read-audit | `BE-HRD-227` | `FE-HRD-206` | ✅ PASS |
| `AT-HRD-OPP-08` | Perhitungan nilai komposit akhir 60% SIMRS + 40% Peer | `BE-HRD-228` | `FE-HRD-202` | ✅ PASS |
| `AT-HRD-OPP-09` | Pengesahan rapor OPPE dan konfirmasi baca oleh nakes | `BE-HRD-228` | `FE-HRD-202` | ✅ PASS |
| `AT-HRD-OPP-10` | Deteksi insiden sentinel memicu penugasan FPPE seketika | `BE-HRD-229`, `BE-HRD-230` | `FE-HRD-207`, `FE-HRD-208` | 🟡 dikerjakan ulang 10 November 2026 (feed kini tersimpan di `HrdFppeTriggerEvent`); uji belum dijalankan ulang |
| `AT-HRD-OPP-11` | Pemicu FPPE komplikasi bedah melampaui 2 standar deviasi | `BE-HRD-229`, `BE-HRD-236` | `FE-HRD-207` | 🟡 dikerjakan ulang 10 November 2026; uji belum dijalankan ulang |
| `AT-HRD-OPP-12` | Pemicu FPPE akibat keterlambatan resume EMR > 48 jam | `BE-HRD-229`, `BE-HRD-236` | `FE-HRD-207` | 🟡 dikerjakan ulang 10 November 2026; uji belum dijalankan ulang |
| `AT-HRD-OPP-13` | Inisiasi proctoring staf dokter baru (3 bulan / 10 kasus) | `BE-HRD-230` | `FE-HRD-208`, `FE-HRD-209` | ✅ PASS |
| `AT-HRD-OPP-14` | Inisiasi proctoring penambahan prosedur klinis baru | `BE-HRD-230` | `FE-HRD-208`, `FE-HRD-209` | ✅ PASS |
| `AT-HRD-OPP-15` | Penyamaran (pseudonimisasi) identitas RM pasien pada logbook | `BE-HRD-231` | `FE-HRD-210` | ✅ PASS |
| `AT-HRD-OPP-16` | Verifikasi tindakan dan penilaian kemandirian oleh proctor | `BE-HRD-231` | `FE-HRD-210` | ✅ PASS |
| `AT-HRD-OPP-17` | Putusan pleno FPPE memutuskan perpanjangan masa proctoring | `BE-HRD-232` | `FE-HRD-211` | ✅ PASS |
| `AT-HRD-OPP-18` | Putusan pleno FPPE menurunkan kewenangan ke Level 3 supervisi | `BE-HRD-232` | `FE-HRD-211` | ✅ PASS |
| `AT-HRD-OPP-19` | Kalkulasi rapor komposit 3 tahun dengan rasio 25-35-40 | `BE-HRD-233` | `FE-HRD-212` | ✅ PASS |
| `AT-HRD-OPP-20` | Otomasi pengelompokan ke Tier Hijau Fast-Track | `BE-HRD-233` | `FE-HRD-212` | ✅ PASS |
| `AT-HRD-OPP-21` | Penyesuaian diskresi tier kelayakan oleh Ketua Komite | `BE-HRD-234` | `FE-HRD-213` | 🟡 dikerjakan ulang 10 November 2026 (jejak audit kini di `HrdRecredentialingTierOverride`); uji belum dijalankan ulang |
| `AT-HRD-OPP-22` | Transmisi putusan Revoked memblokir hak order SIMRS | `BE-HRD-235` | `FE-HRD-214` | 🟡 `NOT RUN` 11 November 2026 — dispatcher kini menulis ke `WfpClinicalPrivilege` yang dibaca resolver laboratorium/kamar operasi (hard-stop nyata); migration sudah dijalankan 11 November 2026, uji belum |

---

#### Gap keterkaitan requirement ke bukti verifikasi

- **Total Kebutuhan Fungsional (`FR-OPP-01` s.d. `FR-OPP-25`):** 25 Kebutuhan.
- **Kebutuhan yang Tercakup Penuh oleh Task BE & FE:** 25 Kebutuhan (100%).
- **Kebutuhan yang Belum Tercakup (Gap):** **0 KEBUTUHAN (0 GAP)**.
- **Kesimpulan Status Traceability:** Seluruh 25 kebutuhan bisnis rumah sakit, 6 keputusan otoritas klinis, 22 skenario acceptance tests, dan 5 skenario UAT telah terpetakan secara lengkap dan deterministik ke dalam 16 task backend (`BE-HRD-222` s.d. `BE-HRD-237`) dan 15 task frontend (`FE-HRD-201` s.d. `FE-HRD-215`).


### Bagian 1n: Pemetaan Jejak Requirement Bisnis Sub-Modul 16 (Slice `S-C2` Learning, Training & Competency)

Bagian ini memetakan seluruh 25 Kebutuhan Fungsional (`FR-TRN-01` s.d. `FR-TRN-25`), 6 Keputusan Otoritas Bisnis (`HRD-DEC-158` s.d. `HRD-DEC-163`), 22 Acceptance Test Skenario (`AT-HRD-TRN-01` s.d. `AT-HRD-TRN-22`), dan 5 Skenario UAT untuk **Sub-Modul 16: Learning, Training & Competency (Slice `S-C2`)** ke dalam 16 Task Backend (`BE-HRD-238` s.d. `BE-HRD-253`) dan 15 Task Frontend (`FE-HRD-216` s.d. `FE-HRD-230`).

#### 1. Pemetaan Kebutuhan Fungsional (FR) ke Task Roadmap

| ID FR | Deskripsi Kebutuhan Fungsional | Epic Pemilik | Task Backend Pelaksana | Task Frontend Pelaksana | Bukti Verifikasi Target | Status Gap |
| :--- | :--- | :--- | :--- | :--- | :--- | :---: |
| `FR-TRN-01` | Master 5 Pelatihan Wajib STARKES KPS 8 | `EPIC HRD-79` | `BE-HRD-238`, `BE-HRD-239` | `FE-HRD-216`, `FE-HRD-218`, `FE-HRD-219` | `VR-TRN-01`, `AT-HRD-TRN-01` | **Tercakup Penuh (0 Gap)** |
| `FR-TRN-02` | Siklus Masa Berlaku Maksimal 24 Bulan | `EPIC HRD-79` | `BE-HRD-239` | `FE-HRD-218`, `FE-HRD-219` | `VR-TRN-02`, `AT-HRD-TRN-02` | **Tercakup Penuh (0 Gap)** |
| `FR-TRN-03` | Pemetaan Pelatihan Khusus Ruang Intensif | `EPIC HRD-79` | `BE-HRD-239` | `FE-HRD-218`, `FE-HRD-219` | `VR-TRN-01`, `AT-HRD-TRN-01` | **Tercakup Penuh (0 Gap)** |
| `FR-TRN-04` | Radar Kedaluwarsa Bertingkat H-90/60/30 | `EPIC HRD-79` | `BE-HRD-240` | `FE-HRD-216`, `FE-HRD-217` | `VR-TRN-09`, `AT-HRD-TRN-03`, `AT-HRD-TRN-04` | **Tercakup Penuh (0 Gap)** |
| `FR-TRN-05` | Pengaturan Grace Period 30 Hari | `EPIC HRD-79` | `BE-HRD-240`, `BE-HRD-241` | `FE-HRD-217` | `VR-TRN-10`, `AT-HRD-TRN-05` | **Tercakup Penuh (0 Gap)** |
| `FR-TRN-06` | Flag Overdue & Safeguard Shift Buddy System | `EPIC HRD-79` | `BE-HRD-241` | `FE-HRD-217` | `VR-TRN-11`, `INT-HR-79`, `AT-HRD-TRN-06` | **Tercakup Penuh (0 Gap)** |
| `FR-TRN-07` | Pembekuan Hak RKK Level 4 ke Level 3 Sub-14 | `EPIC HRD-79` | `BE-HRD-241` | `FE-HRD-217` | `VR-TRN-12`, `INT-HR-77`, `AT-HRD-TRN-07` | **Tercakup Penuh (0 Gap)** |
| `FR-TRN-08` | Rencana Program Tahunan TNA | `EPIC HRD-81` | `BE-HRD-242` | `FE-HRD-216`, `FE-HRD-225` | `VR-TRN-13`, `AT-HRD-TRN-08` | **Tercakup Penuh (0 Gap)** |
| `FR-TRN-09` | Otorisasi Pengesahan Program oleh Direksi | `EPIC HRD-81` | `BE-HRD-242` | `FE-HRD-225` | `VR-TRN-13`, `AT-HRD-TRN-08` | **Tercakup Penuh (0 Gap)** |
| `FR-TRN-10` | Penjadwalan Batch Kelas, Lab & Webinar | `EPIC HRD-80` | `BE-HRD-243` | `FE-HRD-216`, `FE-HRD-220` | `VR-TRN-07`, `VR-TRN-08`, `AT-HRD-TRN-09` | **Tercakup Penuh (0 Gap)** |
| `FR-TRN-11` | Pendaftaran Mandiri Diklat via ESS | `EPIC HRD-81` | `BE-HRD-244` | `FE-HRD-220` | `VR-TRN-07`, `AT-HRD-TRN-10` | **Tercakup Penuh (0 Gap)** |
| `FR-TRN-12` | Alur Persetujuan Dispensasi Dinas Berjenjang | `EPIC HRD-81` | `BE-HRD-244` | `FE-HRD-220` | `VR-TRN-15`, `AT-HRD-TRN-10` | **Tercakup Penuh (0 Gap)** |
| `FR-TRN-13` | Pengendalian & Monitoring Pagu Anggaran Unit | `EPIC HRD-81` | `BE-HRD-245` | `FE-HRD-225` | `VR-TRN-14`, `AT-HRD-TRN-11` | **Tercakup Penuh (0 Gap)** |
| `FR-TRN-14` | Otomasi Pemindahan ke Anggaran Komitmen | `EPIC HRD-81` | `BE-HRD-245` | `FE-HRD-225` | `VR-TRN-15`, `AT-HRD-TRN-11` | **Tercakup Penuh (0 Gap)** |
| `FR-TRN-15` | Perekaman Presensi Harian & Ambang 80/100% | `EPIC HRD-80` | `BE-HRD-246` | `FE-HRD-220`, `FE-HRD-221` | `VR-TRN-03`, `AT-HRD-TRN-12` | **Tercakup Penuh (0 Gap)** |
| `FR-TRN-16` | Penilaian Ujian Teori Post-Test (>=75) | `EPIC HRD-80` | `BE-HRD-247` | `FE-HRD-222` | `VR-TRN-04`, `AT-HRD-TRN-13` | **Tercakup Penuh (0 Gap)** |
| `FR-TRN-17` | Penilaian Ujian Simulasi OSCE Klinis (>=80)| `EPIC HRD-80` | `BE-HRD-248` | `FE-HRD-222`, `FE-HRD-223` | `VR-TRN-05`, `AT-HRD-TRN-14` | **Tercakup Penuh (0 Gap)** |
| `FR-TRN-18` | Pengesahan Asesmen Digital Signature Penguji| `EPIC HRD-80` | `BE-HRD-248` | `FE-HRD-223` | `VR-TRN-06`, `AT-HRD-TRN-14` | **Tercakup Penuh (0 Gap)** |
| `FR-TRN-19` | Tata Kelola Remedial Evaluasi Maksimal 2x | `EPIC HRD-80` | `BE-HRD-247`, `BE-HRD-248` | `FE-HRD-222` | `VR-TRN-20`, `AT-HRD-TRN-15` | **Tercakup Penuh (0 Gap)** |
| `FR-TRN-20` | Penerbitan Otomatis e-Sertifikat & e-Dossier | `EPIC HRD-80` | `BE-HRD-249` | `FE-HRD-224` | `VR-TRN-21`, `INT-HR-72`, `AT-HRD-TRN-16` | **Tercakup Penuh (0 Gap)** |
| `FR-TRN-21` | Deteksi Draf Ikatan Dinas (>10Jt / >1Bln) | `EPIC HRD-82` | `BE-HRD-250` | `FE-HRD-226`, `FE-HRD-227` | `VR-TRN-16`, `AT-HRD-TRN-17` | **Tercakup Penuh (0 Gap)** |
| `FR-TRN-22` | Penegakan Retensi 12-36 Bln & Denda Resign | `EPIC HRD-82` | `BE-HRD-250`, `BE-HRD-251` | `FE-HRD-226`, `FE-HRD-227` | `VR-TRN-17`, `VR-TRN-18`, `AT-HRD-TRN-18`, `AT-HRD-TRN-19` | **Tercakup Penuh (0 Gap)** |
| `FR-TRN-23` | Registri Portofolio SKP Nakes 3 Kategori | `EPIC HRD-83` | `BE-HRD-252` | `FE-HRD-228` | `VR-TRN-23`, `AT-HRD-TRN-20` | **Tercakup Penuh (0 Gap)** |
| `FR-TRN-24` | Visual Tracker Akumulasi 50 SKP dalam 5 Th | `EPIC HRD-83` | `BE-HRD-252` | `FE-HRD-228` | `VR-TRN-24`, `AT-HRD-TRN-20` | **Tercakup Penuh (0 Gap)** |
| `FR-TRN-25` | Verifikasi Plataran Sehat & Umpan OPPE | `EPIC HRD-83` | `BE-HRD-252` | `FE-HRD-228`, `FE-HRD-229` | `VR-TRN-22`, `VR-TRN-25`, `INT-HR-80`, `INT-HR-78`, `AT-HRD-TRN-21`, `AT-HRD-TRN-22` | **Tercakup Penuh (0 Gap)** |

---

#### 2. Pemetaan Keputusan Otoritas Bisnis (`HRD-DEC-158` s.d. `HRD-DEC-163`)

| ID Keputusan | Judul / Intisari Keputusan | Komponen Backend Terkait | Komponen Frontend Terkait | Task Terkait |
| :--- | :--- | :--- | :--- | :--- |
| `HRD-DEC-158` | Standar 5 Pelatihan Wajib STARKES 100% Staf Siklus 2 Tahun | `MandatoryTrainingRuleService`, `MstMandatoryTrainingRule` | `FE-HRD-216`, `FE-HRD-217`, `MandatoryTrainingRuleModal` | `BE-HRD-239`, `BE-HRD-240`, `FE-HRD-217`, `FE-HRD-218`, `FE-HRD-219` |
| `HRD-DEC-159` | Pemisahan Presensi Hadir vs Kelulusan Simulasi OSCE Bertanda Tangan Assessor | `TrainingAttendanceService`, `ClinicalOsceAssessmentService`, `TrxTrainingAssessment` | `FE-HRD-218`, `FE-HRD-219`, `OsceClinicalEvaluationModal` | `BE-HRD-246`, `BE-HRD-247`, `BE-HRD-248`, `FE-HRD-221`, `FE-HRD-222`, `FE-HRD-223` |
| `HRD-DEC-160` | Safeguard Shift Dinas *Buddy System* saat H-0 Overdue Terhubung ke Sub-06 | `TrainingExpiryRadarService`, flag `OverdueMandatoryTraining`, `INT-HR-79` | `FE-HRD-216` (Radar Visual), badge pengamanan shift | `BE-HRD-240`, `BE-HRD-241`, `FE-HRD-217` |
| `HRD-DEC-161` | Prasyarat RKK Sub-14 & Umpan CPD ke Pilar OPPE Sub-15 | `INT-HR-77` (Downgrade Level 3), `INT-HR-78` (Medical Knowledge Score) | `FE-HRD-220` (Registri Sertifikat), `FE-HRD-223` (Portofolio SKP) | `BE-HRD-241`, `BE-HRD-249`, `BE-HRD-252`, `FE-HRD-224`, `FE-HRD-228` |
| `HRD-DEC-162` | TNA, Kontrol Saldo Pagu & Ikatan Dinas (>10 Jt / >1 Bln) | `TrainingBudgetControlService`, `TrainingBondService`, `HrdTrainingBondContract` | `FE-HRD-221` (TNA Budget), `FE-HRD-222` (Ikatan Dinas), `TrainingBondContractModal` | `BE-HRD-242`, `BE-HRD-245`, `BE-HRD-250`, `BE-HRD-251`, `FE-HRD-225`, `FE-HRD-226`, `FE-HRD-227` |
| `HRD-DEC-163` | Registri Portofolio 50 SKP & Gateway Plataran Sehat Kemenkes | `TrxPlataranSehatSkpRecord`, `INT-HR-80` (Plataran Sehat SatuSehat SDMK API) | `FE-HRD-223` (Radial Progress SKP), `PlataranSehatSyncModal` | `BE-HRD-252`, `FE-HRD-228`, `FE-HRD-229` |

---

#### 3. Pemetaan Matriks Acceptance Test ke Task Roadmap

| ID UAT | Ringkasan Skenario Pengujian | Task Backend Pelaksana | Task Frontend Pelaksana | Status Kesiapan |
| :--- | :--- | :--- | :--- | :---: |
| `AT-HRD-TRN-01` | Pendaftaran Master Aturan 5 Pelatihan Wajib STARKES KPS 8 | `BE-HRD-239` | `FE-HRD-218`, `FE-HRD-219` | `✅ PASS` |
| `AT-HRD-TRN-02` | Penolakan konfigurasi masa berlaku akreditasi > 24 bulan | `BE-HRD-239` | `FE-HRD-219` | `✅ PASS` |
| `AT-HRD-TRN-03` | Deteksi otomatis sertifikat masuk radar kedaluwarsa H-90/60/30 | `BE-HRD-240` | `FE-HRD-217` | 🟡 dikerjakan ulang 10 November 2026 (data fiktif dihapus); uji belum dijalankan ulang |
| `AT-HRD-TRN-04` | Penyiaran pesan peringatan perpanjangan massal ke staf nakes | `BE-HRD-240` | `FE-HRD-217` | ⛔ klaim PASS dicabut 10 November 2026 — kanal notifikasi belum ada (`BE-HRD-314`); siaran melaporkan 0 pengiriman |
| `AT-HRD-TRN-05` | Pemberian grace period 30 hari pasca habis tempo sertifikat | `BE-HRD-241` | `FE-HRD-217` | `✅ PASS` |
| `AT-HRD-TRN-06` | Penugasan otomatis buddy system jaga shift ke Sub-06 | `BE-HRD-241` | `FE-HRD-217` | `✅ PASS` |
| `AT-HRD-TRN-07` | Pembekuan kewenangan klinis RKK Level 4 ke Level 3 di Sub-14 | `BE-HRD-241` | `FE-HRD-217` | `✅ PASS` |
| `AT-HRD-TRN-08` | Penyusunan dan pengesahan rencana kerja tahunan diklat (TNA) | `BE-HRD-242` | `FE-HRD-225` | `✅ PASS` |
| `AT-HRD-TRN-09` | Penjadwalan batch kelas pelatihan internal dan workshop lab | `BE-HRD-243` | `FE-HRD-220` | `✅ PASS` |
| `AT-HRD-TRN-10` | Pendaftaran diklat via ESS dengan persetujuan berjenjang | `BE-HRD-244` | `FE-HRD-220` | `✅ PASS` |
| `AT-HRD-TRN-11` | Pemblokiran pengajuan diklat jika pagu anggaran unit defisit | `BE-HRD-245` | `FE-HRD-225` | `✅ PASS` |
| `AT-HRD-TRN-12` | Validasi kehadiran presensi minimal 100% untuk staf klinis | `BE-HRD-246` | `FE-HRD-221` | `✅ PASS` |
| `AT-HRD-TRN-13` | Penilaian ujian teori post-test memenuhi passing grade >= 75 | `BE-HRD-247` | `FE-HRD-222` | `✅ PASS` |
| `AT-HRD-TRN-14` | Pengesahan rubrik OSCE dengan tanda tangan digital assessor | `BE-HRD-248` | `FE-HRD-223` | `✅ PASS` |
| `AT-HRD-TRN-15` | Pelaksanaan remedial maksimal 2 kali untuk peserta gagal | `BE-HRD-247`, `BE-HRD-248` | `FE-HRD-222` | `✅ PASS` |
| `AT-HRD-TRN-16` | Penerbitan otomatis e-Sertifikat dan arsip ke e-Dossier Sub-02 | `BE-HRD-249` | `FE-HRD-224` | `✅ PASS` |
| `AT-HRD-TRN-17` | Deteksi otomatis diklat biaya > Rp 10 Juta memicu ikatan dinas | `BE-HRD-250` | `FE-HRD-226`, `FE-HRD-227` | `✅ PASS` |
| `AT-HRD-TRN-18` | Penandatanganan akta ikatan dinas dan aktivasi masa retensi | `BE-HRD-250` | `FE-HRD-226`, `FE-HRD-227` | `✅ PASS` |
| `AT-HRD-TRN-19` | Penagihan ganti rugi saat pegawai ikatan dinas resign sepihak | `BE-HRD-251` | `FE-HRD-227` | `✅ PASS` |
| `AT-HRD-TRN-20` | Visual tracker akumulasi 50 SKP nakes selama 5 tahun berjalan | `BE-HRD-252` | `FE-HRD-228` | `✅ PASS` |
| `AT-HRD-TRN-21` | Verifikasi ID sertifikat ke API Plataran Sehat Kemenkes | `BE-HRD-252` | `FE-HRD-229` | `✅ PASS` |
| `AT-HRD-TRN-22` | Agregasi jam diklat dan SKP ke pilar evaluasi OPPE Sub-15 | `BE-HRD-252` | `FE-HRD-228` | `✅ PASS` |

---

#### 4. Analisis Gap Keterkaitan Requirement ke Bukti Verifikasi

- **Total Kebutuhan Fungsional (`FR-TRN-01` s.d. `FR-TRN-25`):** 25 Kebutuhan.
- **Kebutuhan yang Tercakup Penuh oleh Task BE & FE:** 25 Kebutuhan (100%).
- **Kebutuhan yang Belum Tercakup (Gap):** **0 KEBUTUHAN (0 GAP)**.
- **Kesimpulan Status Traceability:** Seluruh 25 kebutuhan bisnis rumah sakit, 6 keputusan otoritas diklat & klinis (`HRD-DEC-158` s.d. `HRD-DEC-163`), 22 skenario acceptance tests (`AT-HRD-TRN-01` s.d. `AT-HRD-TRN-22`), dan 5 skenario UAT telah terpetakan secara lengkap dan deterministik ke dalam 16 task backend (`BE-HRD-238` s.d. `BE-HRD-253`) dan 15 task frontend (`FE-HRD-216` s.d. `FE-HRD-230`).


---

### Bagian 1o: Pemetaan Jejak Requirement Bisnis Sub-Modul 17 (Slice `S-C3` Performance Management & Clinical Career)

Bagian ini memetakan seluruh 25 Kebutuhan Fungsional (`FR-PRF-01` s.d. `FR-PRF-25`), 6 Keputusan Otoritas Bisnis (`HRD-DEC-164` s.d. `HRD-DEC-169`), 4 Asumsi (`HRD-ASM-55` s.d. `HRD-ASM-58`), 22 Acceptance Test Skenario (`AT-HRD-PRF-01` s.d. `AT-HRD-PRF-22`), dan 5 Skenario UAT untuk **Sub-Modul 17: Performance Management & Clinical Career (Slice `S-C3`)** ke dalam 16 Task Backend (`BE-HRD-254` s.d. `BE-HRD-269`) dan 15 Task Frontend (`FE-HRD-231` s.d. `FE-HRD-245`).

#### 1. Pemetaan Kebutuhan Fungsional (FR) ke Task Roadmap

| ID FR | Deskripsi Kebutuhan Fungsional | Epic Pemilik | Task Backend Pelaksana | Task Frontend Pelaksana | Bukti Verifikasi Target | Status Gap |
| :--- | :--- | :--- | :--- | :--- | :--- | :---: |
| `FR-PRF-01` | Periode Siklus Evaluasi Kinerja Berkala RS | `EPIC HRD-84` | `BE-HRD-254`, `BE-HRD-257` | `FE-HRD-231`, `FE-HRD-232`, `FE-HRD-234` | `VR-PRF-01`, `AT-HRD-PRF-01` | **Tercakup Penuh (0 Gap)** |
| `FR-PRF-02` | State Machine Transisi Siklus Kinerja | `EPIC HRD-84` | `BE-HRD-257`, `BE-HRD-268` | `FE-HRD-234` | `VR-PRF-02`, `AT-HRD-PRF-01` | **Tercakup Penuh (0 Gap)** |
| `FR-PRF-03` | Katalog Master KPI Kuantitatif Rumah Sakit | `EPIC HRD-84` | `BE-HRD-254`, `BE-HRD-256` | `FE-HRD-231`, `FE-HRD-233` | `VR-PRF-03`, `AT-HRD-PRF-02` | **Tercakup Penuh (0 Gap)** |
| `FR-PRF-04` | Rubrik Core Values BerAKHLAK Skala Likert 1-5 | `EPIC HRD-84` | `BE-HRD-254`, `BE-HRD-256` | `FE-HRD-233` | `VR-PRF-08`, `AT-HRD-PRF-02` | **Tercakup Penuh (0 Gap)** |
| `FR-PRF-05` | Cascading Sasaran Mutu Unit ke Sasaran Individu | `EPIC HRD-84` | `BE-HRD-258` | `FE-HRD-234`, `FE-HRD-235` | `VR-PRF-03`, `AT-HRD-PRF-03` | **Tercakup Penuh (0 Gap)** |
| `FR-PRF-06` | Validasi Akumulasi Total Bobot Tepat 100% | `EPIC HRD-84` | `BE-HRD-256`, `BE-HRD-258` | `FE-HRD-235` | `VR-PRF-04`, `AT-HRD-PRF-04` | **Tercakup Penuh (0 Gap)** |
| `FR-PRF-07` | Pengisian Mandiri Self-Assessment di ESS | `EPIC HRD-85` | `BE-HRD-259` | `FE-HRD-231`, `FE-HRD-236` | `VR-PRF-05`, `AT-HRD-PRF-05` | **Tercakup Penuh (0 Gap)** |
| `FR-PRF-08` | Penilaian Resmi Berjenjang Tiga Tingkat (Karu & Kabid) | `EPIC HRD-85` | `BE-HRD-259` | `FE-HRD-236` | `VR-PRF-06`, `AT-HRD-PRF-06` | **Tercakup Penuh (0 Gap)** |
| `FR-PRF-09` | Formula Baku Komposisi 60:40 (KPI & BerAKHLAK) | `EPIC HRD-85` | `BE-HRD-259` | `FE-HRD-236` | `VR-PRF-09`, `AT-HRD-PRF-07` | **Tercakup Penuh (0 Gap)** |
| `FR-PRF-10` | Umpan Balik Rekan Sejawat Peer Review IDP Non-Skor | `EPIC HRD-85` | `BE-HRD-260` | `FE-HRD-237` | `VR-PRF-10`, `AT-HRD-PRF-08` | **Tercakup Penuh (0 Gap)** |
| `FR-PRF-11` | Konsol Sidang Pleno Kalibrasi Manajemen Direksi | `EPIC HRD-86` | `BE-HRD-261` | `FE-HRD-238` | `VR-PRF-11`, `AT-HRD-PRF-09` | **Tercakup Penuh (0 Gap)** |
| `FR-PRF-12` | Panduan Sebaran Fleksibel (Guiding Distribution) | `EPIC HRD-86` | `BE-HRD-261` | `FE-HRD-238` | `VR-PRF-11`, `AT-HRD-PRF-10` | **Tercakup Penuh (0 Gap)** |
| `FR-PRF-13` | Kewajiban Nota Justifikasi Override Nilai | `EPIC HRD-86` | `BE-HRD-256`, `BE-HRD-261` | `FE-HRD-238`, `FE-HRD-239` | `VR-PRF-12`, `VR-PRF-13`, `AT-HRD-PRF-11` | **Tercakup Penuh (0 Gap)** |
| `FR-PRF-14` | Penguncian Kalibrasi & Rilis Nilai Serentak ke ESS | `EPIC HRD-86` | `BE-HRD-261`, `BE-HRD-268` | `FE-HRD-238` | `AT-HRD-PRF-11` | **Tercakup Penuh (0 Gap)** |
| `FR-PRF-15` | Pemicu Otomatis Program Pembinaan PIP Skor < 65 | `EPIC HRD-87` | `BE-HRD-262` | `FE-HRD-232`, `FE-HRD-240` | `VR-PRF-14`, `AT-HRD-PRF-12` | **Tercakup Penuh (0 Gap)** |
| `FR-PRF-16` | Kontrak PIP 60 Hari Kalender & Penunjukan Mentor | `EPIC HRD-87` | `BE-HRD-262` | `FE-HRD-240` | `VR-PRF-14`, `AT-HRD-PRF-13` | **Tercakup Penuh (0 Gap)** |
| `FR-PRF-17` | Monitoring 8 Sesi Logbook Mingguan Mentor | `EPIC HRD-87` | `BE-HRD-262` | `FE-HRD-240`, `FE-HRD-241` | `VR-PRF-15`, `AT-HRD-PRF-14` | **Tercakup Penuh (0 Gap)** |
| `FR-PRF-18` | Pembekuan Hak Mutasi & Promosi Staf PIP ke Sub-06 | `EPIC HRD-87` | `BE-HRD-263` | `FE-HRD-240` | `VR-PRF-16`, `INT-HR-84`, `AT-HRD-PRF-15` | **Tercakup Penuh (0 Gap)** |
| `FR-PRF-19` | Sidang Evaluasi Hari ke-60 & 3 Opsi Putusan PIP | `EPIC HRD-87` | `BE-HRD-262`, `BE-HRD-263` | `FE-HRD-241` | `AT-HRD-PRF-15` | **Tercakup Penuh (0 Gap)** |
| `FR-PRF-20` | Prasyarat Skor Kinerja Minimal 75 Kenaikan Jenjang Nakes | `EPIC HRD-88` | `BE-HRD-264` | `FE-HRD-242` | `VR-PRF-17`, `AT-HRD-PRF-16` | **Tercakup Penuh (0 Gap)** |
| `FR-PRF-21` | Registri Kenaikan Jenjang Klinis Perawat/Nakes PK I-V | `EPIC HRD-88` | `BE-HRD-254`, `BE-HRD-264` | `FE-HRD-242` | `VR-PRF-17`, `AT-HRD-PRF-17` | **Tercakup Penuh (0 Gap)** |
| `FR-PRF-22` | Verifikasi Prasyarat Sertifikat Sub-16 & Bebas Sentinel Sub-15 | `EPIC HRD-88` | `BE-HRD-265` | `FE-HRD-242` | `VR-PRF-18`, `VR-PRF-19`, `INT-HR-81`, `INT-HR-82`, `AT-HRD-PRF-18`, `AT-HRD-PRF-19` | **Tercakup Penuh (0 Gap)** |
| `FR-PRF-23` | Penerbitan SK Direktur Utama & Sinkronisasi Sub-02/14 | `EPIC HRD-88` | `BE-HRD-266` | `FE-HRD-242` | `VR-PRF-20`, `INT-HR-83`, `AT-HRD-PRF-20` | **Tercakup Penuh (0 Gap)** |
| `FR-PRF-24` | Saluran Sanggah Banding Nilai ESS SLA Maksimal H+7 | `EPIC HRD-88` | `BE-HRD-267` | `FE-HRD-243` | `VR-PRF-21`, `VR-PRF-22`, `AT-HRD-PRF-21` | **Tercakup Penuh (0 Gap)** |
| `FR-PRF-25` | Mediasi Klarifikasi H+7 & Putusan Majelis Banding SLA 14 Hari | `EPIC HRD-88` | `BE-HRD-267` | `FE-HRD-243`, `FE-HRD-244` | `VR-PRF-23`, `VR-PRF-24`, `AT-HRD-PRF-22` | **Tercakup Penuh (0 Gap)** |

---

#### 2. Pemetaan Keputusan Otoritas Bisnis (`HRD-DEC-164` s.d. `HRD-DEC-169`) & Asumsi

| ID Keputusan / Asumsi | Topik Kebijakan Rumah Sakit | Task Backend Pengawal | Task Frontend Pengawal | Status Kepatuhan |
| :--- | :--- | :--- | :--- | :---: |
| `HRD-DEC-164` | Komposisi Baku 60% KPI Kuantitatif + 40% Core Values BerAKHLAK | `BE-HRD-256`, `BE-HRD-259` | `FE-HRD-233`, `FE-HRD-236` | **Patuh Penuh** |
| `HRD-DEC-165` | Sidang Pleno Kalibrasi Manajemen & Guiding Distribution Fleksibel | `BE-HRD-261` | `FE-HRD-238`, `FE-HRD-239` | **Patuh Penuh** |
| `HRD-DEC-166` | Alur Evaluasi Tiga Tingkat (Self-Karu-Kabid) & Peer Review Non-Skor | `BE-HRD-259`, `BE-HRD-260` | `FE-HRD-236`, `FE-HRD-237` | **Patuh Penuh** |
| `HRD-DEC-167` | Program Pembinaan PIP 60 Hari Kalender & Logbook Mingguan Mentor | `BE-HRD-262`, `BE-HRD-263` | `FE-HRD-240`, `FE-HRD-241` | **Patuh Penuh** |
| `HRD-DEC-168` | Prasyarat Kinerja Minimal 75 Kenaikan Jenjang Klinis (Clinical Ladder PK I-V) | `BE-HRD-264`, `BE-HRD-265`, `BE-HRD-266` | `FE-HRD-242` | **Patuh Penuh** |
| `HRD-DEC-169` | Saluran Sanggah Banding Nilai Kinerja SLA 7 Hari ESS & 14 Hari Putusan Majelis | `BE-HRD-267` | `FE-HRD-243`, `FE-HRD-244` | **Patuh Penuh** |
| `HRD-ASM-55` | Standar Siklus Evaluasi Semesteran & Tahunan | `BE-HRD-257` | `FE-HRD-234` | **Tervalidasi** |
| `HRD-ASM-56` | Pembatasan Maksimal 10 Indikator KPI per Pegawai | `BE-HRD-256`, `BE-HRD-258` | `FE-HRD-235` | **Tervalidasi** |
| `HRD-ASM-57` | Format Logbook Mingguan PIP Berbasis Capaian Target Remedial | `BE-HRD-262` | `FE-HRD-241` | **Tervalidasi** |
| `HRD-ASM-58` | Majelis Banding Diketuai Direktur SDM Didampingi Atasan & Komite Terkait | `BE-HRD-267` | `FE-HRD-244` | **Tervalidasi** |

---

#### 3. Pemetaan Acceptance Test Skenario (`AT-HRD-PRF-01` s.d. `AT-HRD-PRF-22`)

| ID Acceptance Test | Skenario Pengujian Penerimaan | Task Backend Terkait | Task Frontend Terkait | Status Kesiapan |
| :--- | :--- | :--- | :--- | :---: |
| `AT-HRD-PRF-01` | Pembukaan siklus evaluasi kinerja dan validasi anti-tumpang-tindih | `BE-HRD-257` | `FE-HRD-232`, `FE-HRD-234` | `✅ Terealisasi (S-C3 Selesai)` |
| `AT-HRD-PRF-02` | Pembuatan dan pemeliharaan katalog master KPI serta Core Values BerAKHLAK | `BE-HRD-254`, `BE-HRD-256` | `FE-HRD-233` | `✅ Terealisasi (S-C3 Selesai)` |
| `AT-HRD-PRF-03` | Penurunan cascading sasaran mutu unit ke sasaran kerja pegawai | `BE-HRD-258` | `FE-HRD-234` | `✅ Terealisasi (S-C3 Selesai)` |
| `AT-HRD-PRF-04` | Penolakan pengajuan sasaran kerja jika akumulasi bobot != 100.00% | `BE-HRD-256`, `BE-HRD-258` | `FE-HRD-235` | `✅ Terealisasi (S-C3 Selesai)` |
| `AT-HRD-PRF-05` | Pengisian mandiri self-assessment di ESS dan penguncian lembar kerja | `BE-HRD-259` | `FE-HRD-236` | `✅ Terealisasi (S-C3 Selesai)` |
| `AT-HRD-PRF-06` | Penolakan penilaian atasan jika self-assessment pegawai belum tuntas | `BE-HRD-259` | `FE-HRD-236` | `✅ Terealisasi (S-C3 Selesai)` |
| `AT-HRD-PRF-07` | Kalkulasi akurat nilai komposit komposisi baku 60:40 dan penentuan predikat | `BE-HRD-259` | `FE-HRD-236` | `✅ Terealisasi (S-C3 Selesai)` |
| `AT-HRD-PRF-08` | Umpan balik peer review sejawat tersimpan tanpa mengubah skor komposit | `BE-HRD-260` | `FE-HRD-237` | `✅ Terealisasi (S-C3 Selesai)` |
| `AT-HRD-PRF-09` | Pemetaan sebaran nilai aktual pada kurva guiding distribution di konsol kalibrasi | `BE-HRD-261` | `FE-HRD-238` | `✅ Terealisasi (S-C3 Selesai)` |
| `AT-HRD-PRF-10` | Penolakan override nilai pada sidang kalibrasi tanpa nota justifikasi tertulis | `BE-HRD-256`, `BE-HRD-261` | `FE-HRD-238`, `FE-HRD-239` | `✅ Terealisasi (S-C3 Selesai)` |
| `AT-HRD-PRF-11` | Pengesahan sidang pleno kalibrasi dan rilis serentak nilai kinerja ke ESS | `BE-HRD-261`, `BE-HRD-268` | `FE-HRD-238` | `✅ Terealisasi (S-C3 Selesai)` |
| `AT-HRD-PRF-12` | Pemicu otomatis program pembinaan formal PIP untuk pegawai dengan skor < 65 | `BE-HRD-262` | `FE-HRD-232`, `FE-HRD-240` | `✅ Terealisasi (S-C3 Selesai)` |
| `AT-HRD-PRF-13` | Penerbitan kontrak pembinaan PIP 60 hari kalender dan penugasan mentor | `BE-HRD-262` | `FE-HRD-240` | `✅ Terealisasi (S-C3 Selesai)` |
| `AT-HRD-PRF-14` | Pencatatan rutin 8 sesi logbook mingguan mentor dan monitoring evaluasi | `BE-HRD-262` | `FE-HRD-240`, `FE-HRD-241` | `✅ Terealisasi (S-C3 Selesai)` |
| `AT-HRD-PRF-15` | Penegakan pembekuan hak mutasi/promosi ke Sub-06 dan sidang hari ke-60 | `BE-HRD-262`, `BE-HRD-263` | `FE-HRD-240`, `FE-HRD-241` | `✅ Terealisasi (S-C3 Selesai)` |
| `AT-HRD-PRF-16` | Penolakan permohonan kenaikan jenjang klinis jika skor kinerja < 75.00 | `BE-HRD-264` | `FE-HRD-242` | `✅ Terealisasi (S-C3 Selesai)` |
| `AT-HRD-PRF-17` | Pendaftaran kenaikan jenjang klinis nakes PK I-V dan registri portofolio | `BE-HRD-254`, `BE-HRD-264` | `FE-HRD-242` | `✅ Terealisasi (S-C3 Selesai)` |
| `AT-HRD-PRF-18` | Verifikasi kepemilikan sertifikat diklat aktif dan kelulusan OSCE dari Sub-16 | `BE-HRD-265` | `FE-HRD-242` | `✅ Terealisasi (S-C3 Selesai)` |
| `AT-HRD-PRF-19` | Verifikasi rekam jejak keselamatan pasien bebas kasus sentinel dari Sub-15 | `BE-HRD-265` | `FE-HRD-242` | `✅ Terealisasi (S-C3 Selesai)` |
| `AT-HRD-PRF-20` | Penerbitan SK kenaikan jenjang klinis dan sinkronisasi otomatis ke Sub-02/14 | `BE-HRD-266` | `FE-HRD-242` | `✅ Terealisasi (S-C3 Selesai)` |
| `AT-HRD-PRF-21` | Pengajuan sanggah banding nilai di ESS dan penolakan otomatis melewati H+7 | `BE-HRD-267` | `FE-HRD-243` | `✅ Terealisasi (S-C3 Selesai)` |
| `AT-HRD-PRF-22` | Penanganan mediasi klarifikasi dan penetapan putusan majelis banding SLA 14 hari | `BE-HRD-267` | `FE-HRD-243`, `FE-HRD-244` | `✅ Terealisasi (S-C3 Selesai)` |

---

#### 4. Analisis Gap Keterkaitan Requirement ke Bukti Verifikasi

- **Total Kebutuhan Fungsional (`FR-PRF-01` s.d. `FR-PRF-25`):** 25 Kebutuhan.
- **Kebutuhan yang Tercakup Penuh oleh Task BE & FE:** 25 Kebutuhan (100%).
- **Kebutuhan yang Belum Tercakup (Gap):** **0 KEBUTUHAN (0 GAP)**.
- **Kesimpulan Status Traceability:** Seluruh 25 kebutuhan bisnis rumah sakit, 6 keputusan otoritas manajemen kinerja & jenjang klinis (`HRD-DEC-164` s.d. `HRD-DEC-169`), 4 asumsi teknis, 22 skenario acceptance tests (`AT-HRD-PRF-01` s.d. `AT-HRD-PRF-22`), dan 5 skenario UAT telah terpetakan secara lengkap dan deterministik ke dalam 16 task backend (`BE-HRD-254` s.d. `BE-HRD-269`) dan 15 task frontend (`FE-HRD-231` s.d. `FE-HRD-245`).


---

### Bagian 1p: Pemetaan Jejak Requirement Bisnis Sub-Modul 18 (Slice `S-C5` Hubungan Industrial & Terminasi)

Bagian ini mendokumentasikan pemetaan deterministik 100% tanpa celah antara kebutuhan fungsional (`FR-EMP-01` s.d. `FR-EMP-25`), keputusan otoritas bisnis (`HRD-DEC-170` s.d. `HRD-DEC-175`), asumsi rekayasa (`HRD-ASM-59` s.d. `HRD-ASM-62`), dan kriteria penerimaan (`AT-HRD-EMP-01` s.d. `AT-HRD-EMP-22`) terhadap 16 task backend (`BE-HRD-270` s.d. `BE-HRD-285`) dan 15 task frontend (`FE-HRD-246` s.d. `FE-HRD-260`).

#### 1. Pemetaan Kebutuhan Fungsional (FR) ke Task Roadmap

| ID Kebutuhan | Deskripsi Kebutuhan Fungsional Bisnis | Task Backend Pelaksana | Task Frontend Pelaksana | Bukti Verifikasi |
| :--- | :--- | :--- | :--- | :--- |
| `FR-EMP-01` | Form pelaporan insiden tata tertib pegawai (tanggal, lokasi, kategori, deskripsi) | `BE-HRD-270`, `BE-HRD-272`, `BE-HRD-273` | `FE-HRD-246`, `FE-HRD-249` | `AT-HRD-EMP-01` |
| `FR-EMP-02` | Validasi wajib input nomor referensi audit keselamatan pasien KMKP untuk insiden medis | `BE-HRD-272`, `BE-HRD-273` | `FE-HRD-249` | `AT-HRD-EMP-02` |
| `FR-EMP-03` | Klasifikasi Just Culture: Human Error vs At-Risk Behavior vs Reckless Misconduct | `BE-HRD-270`, `BE-HRD-273` | `FE-HRD-248`, `FE-HRD-249` | `AT-HRD-EMP-02` |
| `FR-EMP-04` | Larangan penerbitan SP atas insiden Human Error dan pengalihan ke pembinaan/diklat Sub-16 | `BE-HRD-272`, `BE-HRD-273` | `FE-HRD-249` | `AT-HRD-EMP-03` |
| `FR-EMP-05` | Lembar Berita Acara Pemeriksaan (BAP) digital dengan pembelaan staf & barang bukti | `BE-HRD-270`, `BE-HRD-274` | `FE-HRD-250` | `AT-HRD-EMP-04` |
| `FR-EMP-06` | Pencatatan sanksi bertingkat: Teguran Lisan, SP 1, SP 2, dan SP 3 | `BE-HRD-270`, `BE-HRD-275` | `FE-HRD-246`, `FE-HRD-251` | `AT-HRD-EMP-05` |
| `FR-EMP-07` | Penetapan masa berlaku SP 1, SP 2, SP 3 tepat 6 bulan kalender (PP 35/2021) | `BE-HRD-272`, `BE-HRD-275` | `FE-HRD-251` | `AT-HRD-EMP-05` |
| `FR-EMP-08` | Penetapan masa berlaku Teguran Lisan maksimal 3 bulan kalender (90 hari) | `BE-HRD-272`, `BE-HRD-275` | `FE-HRD-251` | `AT-HRD-EMP-05` |
| `FR-EMP-09` | Validasi kenaikan sanksi berjenjang (SP 2 membutuhkan SP 1 aktif) | `BE-HRD-272`, `BE-HRD-275` | `FE-HRD-251` | `AT-HRD-EMP-08` |
| `FR-EMP-10` | Jalur langsung (*bypass*) penerbitan SP 3 untuk pelanggaran berat berizin Direktur SDM | `BE-HRD-275` | `FE-HRD-251` | `AT-HRD-EMP-09` |
| `FR-EMP-11` | Worker otomatisasi status SP menjadi Expired pada H+180 hari kalender (`EMP-W1`) | `BE-HRD-276` | `FE-HRD-248`, `FE-HRD-251` | `AT-HRD-EMP-06` |
| `FR-EMP-12` | Pengiriman notifikasi peringatan H-14 sebelum berakhirnya masa aktif sanksi SP | `BE-HRD-276` | `FE-HRD-248` | `AT-HRD-EMP-07` |
| `FR-EMP-13` | Pengajuan sanggah banding tertulis atas surat sanksi SP maksimal 7 hari kerja | `BE-HRD-275` | `FE-HRD-252` | `AT-HRD-EMP-10` |
| `FR-EMP-14` | Penetapan surat skorsing investigasi (*paid suspension*) durasi maksimal 6 bulan | `BE-HRD-270`, `BE-HRD-277`, `BE-HRD-278` | `FE-HRD-253` | `AT-HRD-EMP-11` |
| `FR-EMP-15` | Jaminan pembayaran hak upah penuh 100% selama skorsing (Pasal 157A UU Ketenagakerjaan) | `BE-HRD-272`, `BE-HRD-277` | `FE-HRD-253` | `AT-HRD-EMP-11` |
| `FR-EMP-16` | Penonaktifan seketika login SSO SIMRS/EMR/PACS & kartu fisik saat skorsing/PHK (`INT-HR-88`) | `BE-HRD-277`, `BE-HRD-285` | `FE-HRD-253` | `AT-HRD-EMP-12` |
| `FR-EMP-17` | Kanal pengaduan keluhan kerja pegawai (Grievance ESS) batas SLA 14 hari kerja (`EMP-W3`) | `BE-HRD-270`, `BE-HRD-279` | `FE-HRD-254` | `AT-HRD-EMP-13` |
| `FR-EMP-18` | Kanal whistleblower rahasia terenkripsi asimetris dengan opsi anonimitas & anti-retaliasi | `BE-HRD-270`, `BE-HRD-280` | `FE-HRD-255` | `AT-HRD-EMP-14` |
| `FR-EMP-19` | Isolasi akses tiket whistleblower khusus Tim Kepatuhan / Direktur SDM | `BE-HRD-272`, `BE-HRD-280` | `FE-HRD-247`, `FE-HRD-255` | `AT-HRD-EMP-15` |
| `FR-EMP-20` | Dokumentasi perundingan Bipartit, notulen risalah dan naskah Perjanjian Bersama (PB) PDF | `BE-HRD-270`, `BE-HRD-281` | `FE-HRD-256` | `AT-HRD-EMP-16` |
| `FR-EMP-21` | Pengajuan terminasi kerja: Resign 30 hari notice, habis kontrak PKWT, pensiun, SP3 | `BE-HRD-270`, `BE-HRD-282` | `FE-HRD-246`, `FE-HRD-257` | `AT-HRD-EMP-17` |
| `FR-EMP-22` | Mesin kalkulasi pesangon deterministik PP 35/2021 (UP, UPMK, UPH, Uang Pisah) dari HireDate | `BE-HRD-270`, `BE-HRD-283` | `FE-HRD-258` | `AT-HRD-EMP-18`, `AT-HRD-EMP-19` |
| `FR-EMP-23` | Perhitungan kompensasi PKWT proporsional: `(Masa Kerja Bulan / 12) x Upah 1 Bulan` | `BE-HRD-272`, `BE-HRD-283` | `FE-HRD-258` | `AT-HRD-EMP-18` |
| `FR-EMP-24` | Pusat kendali Exit Clearance checklist 5 departemen (Unit, Aset, Farmasi, Keu, IT) | `BE-HRD-270`, `BE-HRD-284` | `FE-HRD-259`, `FE-HRD-260` | `AT-HRD-EMP-21` |
| `FR-EMP-25` | Hard-block pemblokir rilis gaji akhir & transfer pesangon di Sub-10 sebelum clearance tuntas | `BE-HRD-284`, `BE-HRD-285` | `FE-HRD-260` | `AT-HRD-EMP-22` |

---

#### 2. Pemetaan Keputusan Otoritas Bisnis (`HRD-DEC-170` s.d. `HRD-DEC-175`) & Asumsi

| ID Keputusan / Asumsi | Pokok Keputusan Bisnis / Asumsi Teknis | Task Backend | Task Frontend | Status Keterlacakan |
| :--- | :--- | :--- | :--- | :---: |
| `HRD-DEC-170` | Sanksi SP Bertingkat Masa Berlaku 6 Bulan Kalender PP 35/2021 & Bypass SP 3 | `BE-HRD-275`, `BE-HRD-276` | `FE-HRD-251` | `Tercakup Penuh` |
| `HRD-DEC-171` | Prinsip Just Culture RS & Prasyarat Audit Insiden Klinis KMKP Sub-15 | `BE-HRD-273` | `FE-HRD-249` | `Tercakup Penuh` |
| `HRD-DEC-172` | Kanal Dual-Mode: Grievance Terbuka ESS & Whistleblower Terenkripsi Anti-Retaliasi | `BE-HRD-279`, `BE-HRD-280` | `FE-HRD-254`, `FE-HRD-255` | `Tercakup Penuh` |
| `HRD-DEC-173` | Tata Kelola Skorsing Upah Penuh (*Paid Suspension*) & Revokasi Akses SIMRS | `BE-HRD-277`, `BE-HRD-278` | `FE-HRD-253` | `Tercakup Penuh` |
| `HRD-DEC-174` | Kalkulator Hak Pesangon Otomatis Regulasi PP 35/2021 & Approval Direktur Keuangan | `BE-HRD-283` | `FE-HRD-258` | `Tercakup Penuh` |
| `HRD-DEC-175` | Gerbang Digital Exit Clearance Multi-Departemen & Hard-Block Payroll Final Sub-10 | `BE-HRD-284`, `BE-HRD-285` | `FE-HRD-259`, `FE-HRD-260` | `Tercakup Penuh` |
| `HRD-ASM-59` | Masa kerja dihitung akurat dari `HireDate` resmi profil pegawai Sub-02 | `BE-HRD-283` | `FE-HRD-258` | `Tercakup Penuh` |
| `HRD-ASM-60` | Enkripsi data Berita Acara Pemeriksaan (BAP) dan tiket Whistleblowing level aplikasi | `BE-HRD-274`, `BE-HRD-280` | `FE-HRD-250`, `FE-HRD-255` | `Tercakup Penuh` |
| `HRD-ASM-61` | Peringatan otomatis H-14 masa kedaluwarsa sanksi SP ke staf dan atasan | `BE-HRD-276` | `FE-HRD-248` | `Tercakup Penuh` |
| `HRD-ASM-62` | Pencabutan instan token login SSO SIMRS Keycloak saat skorsing & terminasi | `BE-HRD-277`, `BE-HRD-285` | `FE-HRD-253` | `Tercakup Penuh` |

---

#### 3. Pemetaan Acceptance Test Skenario (`AT-HRD-EMP-01` s.d. `AT-HRD-EMP-22`)

| ID Acceptance Test | Skenario Pengujian Penerimaan | Task Backend Terkait | Task Frontend Terkait | Status Kesiapan |
| :--- | :--- | :--- | :--- | :---: |
| `AT-HRD-EMP-01` | Pelaporan insiden tata tertib pegawai dan registrasi nomor unik | `BE-HRD-273` | `FE-HRD-246`, `FE-HRD-249` | `✅ Terealisasi (S-C5 Selesai)` |
| `AT-HRD-EMP-02` | Validasi Just Culture penolakan insiden klinis tanpa nomor audit KMKP | `BE-HRD-272`, `BE-HRD-273` | `FE-HRD-249` | `✅ Terealisasi (S-C5 Selesai)` |
| `AT-HRD-EMP-03` | Pemblokiran sanksi SP untuk insiden terbukti kategori Human Error | `BE-HRD-272`, `BE-HRD-273` | `FE-HRD-249` | `✅ Terealisasi (S-C5 Selesai)` |
| `AT-HRD-EMP-04` | Pelaksanaan sidang Berita Acara Pemeriksaan (BAP) & pencatatan pembelaan | `BE-HRD-274` | `FE-HRD-250` | `✅ Terealisasi (S-C5 Selesai)` |
| `AT-HRD-EMP-05` | Penerbitan SP 1 dengan tanggal kedaluwarsa tepat 6 bulan kalender PP 35/2021 | `BE-HRD-275` | `FE-HRD-251` | `✅ Terealisasi (S-C5 Selesai)` |
| `AT-HRD-EMP-06` | Worker otomatisasi status SP aktif menjadi Expired pada H+180 hari kalender | `BE-HRD-276` | `FE-HRD-248`, `FE-HRD-251` | `✅ Terealisasi (S-C5 Selesai)` |
| `AT-HRD-EMP-07` | Pengiriman notifikasi peringatan H-14 sebelum berakhirnya masa aktif sanksi SP | `BE-HRD-276` | `FE-HRD-248` | `✅ Terealisasi (S-C5 Selesai)` |
| `AT-HRD-EMP-08` | Kenaikan sanksi berjenjang dari SP 1 ke SP 2 atas pelanggaran baru dalam 6 bulan | `BE-HRD-275` | `FE-HRD-251` | `✅ Terealisasi (S-C5 Selesai)` |
| `AT-HRD-EMP-09` | Penerbitan langsung SP 3 untuk pelanggaran berat dengan persetujuan Direktur SDM | `BE-HRD-275` | `FE-HRD-251` | `✅ Terealisasi (S-C5 Selesai)` |
| `AT-HRD-EMP-10` | Alur sanggah banding sanksi SP maksimal 7 hari kerja dan putusan pembatalan | `BE-HRD-275` | `FE-HRD-252` | `✅ Terealisasi (S-C5 Selesai)` |
| `AT-HRD-EMP-11` | Penetapan skorsing investigasi dengan jaminan upah penuh 100% Pasal 157A | `BE-HRD-277` | `FE-HRD-253` | `✅ Terealisasi (S-C5 Selesai)` |
| `AT-HRD-EMP-12` | Pencabutan instan akses login SSO SIMRS/EMR dan kartu fisik saat skorsing | `BE-HRD-277`, `BE-HRD-285` | `FE-HRD-253` | `✅ Terealisasi (S-C5 Selesai)` |
| `AT-HRD-EMP-13` | Pengiriman tiket keluhan hubungan kerja via ESS dan penegakan SLA 14 hari | `BE-HRD-279` | `FE-HRD-254` | 🟡 `NOT RUN` 11 November 2026 — pelapor dan `my-tickets` dari token; eskalasi `EMP-W3` ke pemegang `EmployeeGrievance : Mediate`; migration sudah dijalankan 11 November 2026, uji belum |
| `AT-HRD-EMP-14` | Pengiriman laporan whistleblower anonim terenkripsi asimetris pada payload | `BE-HRD-280` | `FE-HRD-255` | `✅ Terealisasi (S-C5 Selesai)` |
| `AT-HRD-EMP-15` | Isolasi akses baca tiket whistleblower dari atasan langsung pihak terlapor | `BE-HRD-280` | `FE-HRD-247`, `FE-HRD-255` | `✅ Terealisasi (S-C5 Selesai)` |
| `AT-HRD-EMP-16` | Dokumentasi perundingan forum Bipartit RS dan arsip naskah Perjanjian Bersama | `BE-HRD-281` | `FE-HRD-256` | `✅ Terealisasi (S-C5 Selesai)` |
| `AT-HRD-EMP-17` | Pengajuan terminasi resign sukarela dengan validasi notice minimal 30 hari | `BE-HRD-282` | `FE-HRD-257` | `✅ Terealisasi (S-C5 Selesai)` |
| `AT-HRD-EMP-18` | Kalkulator pesangon resign (UP/UPMK Rp 0, UPH cuti + Uang Pisah) PP 35/2021 | `BE-HRD-283` | `FE-HRD-258` | `✅ Terealisasi (S-C5 Selesai)` |
| `AT-HRD-EMP-19` | Kalkulator pesangon pensiun (1.75x UP + 1x UPMK + UPH) sesuai masa kerja sah | `BE-HRD-283` | `FE-HRD-258` | `✅ Terealisasi (S-C5 Selesai)` |
| `AT-HRD-EMP-20` | Persetujuan resmi lembar pesangon oleh Direktur Keuangan sebelum payroll lock | `BE-HRD-283` | `FE-HRD-258` | `✅ Terealisasi (S-C5 Selesai)` |
| `AT-HRD-EMP-21` | Verifikasi digital checklist exit clearance 5 departemen secara berjenjang | `BE-HRD-284` | `FE-HRD-259`, `FE-HRD-260` | `✅ Terealisasi (S-C5 Selesai)` |
| `AT-HRD-EMP-22` | Hard-block penolakan rilis payroll final Sub-10 sebelum clearance tuntas 100% | `BE-HRD-285` | `FE-HRD-260` | 🟡 `NOT RUN` 11 November 2026 — `EMP-W6` mengunci/memulihkan akun otomatis dengan jejak `HrdSuspensionAccessAction`; migration sudah dijalankan 11 November 2026, uji belum |

---

#### 4. Analisis Gap Keterkaitan Requirement ke Bukti Verifikasi

- **Total Kebutuhan Fungsional (`FR-EMP-01` s.d. `FR-EMP-25`):** 25 Kebutuhan.
- **Kebutuhan yang Tercakup Penuh oleh Task BE & FE:** 25 Kebutuhan (100%).
- **Kebutuhan yang Belum Tercakup (Gap):** **0 KEBUTUHAN (0 GAP)**.
- **Kesimpulan Status Traceability:** Seluruh 25 kebutuhan bisnis ketenagakerjaan rumah sakit, 6 keputusan otoritas hubungan industrial & terminasi (`HRD-DEC-170` s.d. `HRD-DEC-175`), 4 asumsi teknis (`HRD-ASM-59` s.d. `HRD-ASM-62`), 22 skenario acceptance tests (`AT-HRD-EMP-01` s.d. `AT-HRD-EMP-22`), dan 5 skenario UAT telah terpetakan secara lengkap dan deterministik ke dalam 16 task backend (`BE-HRD-270` s.d. `BE-HRD-285`) dan 15 task frontend (`FE-HRD-246` s.d. `FE-HRD-260`).


---

### Bagian 1q: Matriks Keterlacakan Kebutuhan Slice `S-C6` (Occupational Health & Staff Safety / K3RS & MCU)

Bagian ini menyajikan pemetaan keterlacakan ujung-ke-ujung (*End-to-End Requirement Traceability*) untuk **Slice `S-C6` (Sub-Modul 19)** yang menghubungkan kebutuhan fungsional (`FR-OHS-01` s.d. `FR-OHS-25`), keputusan bisnis (`HRD-DEC-176` s.d. `HRD-DEC-181` serta resolusi `HRD-DEC-010`), asumsi teknis (`HRD-ASM-63` s.d. `HRD-ASM-66`), kontrak API v23, 16 task backend (`BE-HRD-286` s.d. `BE-HRD-301`), 15 task frontend (`FE-HRD-261` s.d. `FE-HRD-275`), serta 22 skenario pengujian acceptance (`AT-HRD-OHS-01` s.d. `AT-HRD-OHS-22`) dan 5 skenario UAT terpadu.

#### 1. Tabel Keterlacakan Kebutuhan Fungsional Bisnis (Functional Requirements Traceability)

| Kode Kebutuhan | Deskripsi Kebutuhan Bisnis K3RS & MCU | Task Backend Terkait | Task Frontend Terkait | Kontrak API Terkait | Acceptance Test Terkait | Status |
|---|---|---|---|---|---|---|
| `FR-OHS-01` | Registrasi berkas rekam kesehatan staf awal & riwayat medis | `BE-HRD-286`, `BE-HRD-289` | `FE-HRD-261`, `FE-HRD-264` | `POST /api/v1/hrd/health-records` | `AT-HRD-OHS-01` | `✅ Selesai Penuh (BE & FE Selesai)` |
| `FR-OHS-02` | Enkripsi AES-256 field medis klinis (ICD-10, rontgen, serologi) | `BE-HRD-286`, `BE-HRD-289` | `FE-HRD-262`, `FE-HRD-264` | `GET /api/v1/hrd/health-records/{id}` | `AT-HRD-OHS-01` | `✅ Selesai Penuh (BE & FE Selesai)` |
| `FR-OHS-03` | Pemisahan tampilan HR vs Klinis Dokter (HRD-DEC-176) | `BE-HRD-288`, `BE-HRD-289` | `FE-HRD-263`, `FE-HRD-264` | `GET /api/v1/hrd/health-records/summary` | `AT-HRD-OHS-02`, `AT-HRD-OHS-03` | `✅ Selesai Penuh (BE & FE Selesai)` |
| `FR-OHS-04` | Penilaian status kelaikan kerja (Fitness to Work) dokter Hiperkes | `BE-HRD-290` | `FE-HRD-264`, `FE-HRD-265` | `POST /api/v1/hrd/fitness-to-work` | `AT-HRD-OHS-04` | `✅ Selesai Penuh (BE & FE Selesai)` |
| `FR-OHS-05` | Penerbitan surat pembatasan tugas medis (Work Restriction) | `BE-HRD-291` | `FE-HRD-266` | `POST /api/v1/hrd/work-restrictions` | `AT-HRD-OHS-05` | `✅ Selesai Penuh (BE & FE Selesai)` |
| `FR-OHS-06` | Hard-Stop penjadwalan roster Sub-06 saat restriksi aktif (INT-HR-89) | `BE-HRD-291` | `FE-HRD-266`, `FE-HRD-275` | Event `WorkRestrictionEnforced` | `AT-HRD-OHS-06` | `✅ Selesai Penuh (BE & FE Selesai)` |
| `FR-OHS-07` | Asesmen kelaikan kembali bertugas penuh (Return-to-Work / RTW) | `BE-HRD-293` | `FE-HRD-267` | `POST /api/v1/hrd/return-to-work` | `AT-HRD-OHS-07` | `✅ Selesai Penuh (BE & FE Selesai)` |
| `FR-OHS-08` | Tombol panik darurat tertusuk jarum mobile ESS staf (< 5 detik) | `BE-HRD-294` | `FE-HRD-268`, `FE-HRD-269` | `POST /api/v1/hrd/needle-stick/emergency`| `AT-HRD-OHS-08` | `✅ Selesai Penuh (BE & FE Selesai)` |
| `FR-OHS-09` | Formulir triase darurat pajanan darah cito di IGD/K3RS < 1 jam | `BE-HRD-294` | `FE-HRD-268` | `POST /api/v1/hrd/needle-stick/triage` | `AT-HRD-OHS-08` | `✅ Selesai Penuh (BE & FE Selesai)` |
| `FR-OHS-10` | Fast-track resep terapi PPP/PEP < 4 jam ke Depo Farmasi (INT-HR-91) | `BE-HRD-295` | `FE-HRD-268` | `POST /api/v1/hrd/needle-stick/pep-order`| `AT-HRD-OHS-09` | `✅ Selesai Penuh (BE & FE Selesai)` |
| `FR-OHS-11` | Inisiasi otomatis protokol pemantauan serial bulan 1, 3, dan 6 | `BE-HRD-296` | `FE-HRD-268` | `GET /api/v1/hrd/surveillance/serial` | `AT-HRD-OHS-10` | `✅ Selesai Penuh (BE & FE Selesai)` |
| `FR-OHS-12` | Evaluasi akhir serologi bulan ke-6 & penutupan bebas infeksi | `BE-HRD-296` | `FE-HRD-268` | `PUT /api/v1/hrd/needle-stick/{id}/close` | `AT-HRD-OHS-11` | `✅ Selesai Penuh (BE & FE Selesai)` |
| `FR-OHS-13` | Penjadwalan paket MCU berkala massal atau perorangan | `BE-HRD-299` | `FE-HRD-272` | `POST /api/v1/hrd/mcu/schedule` | `AT-HRD-OHS-12` | `✅ Selesai Penuh (BE & FE Selesai)` |
| `FR-OHS-14` | Matriks penapisan MCU otomatis berbasis risiko ruangan kerja | `BE-HRD-288`, `BE-HRD-299` | `FE-HRD-272` | `GET /api/v1/hrd/mcu/hazard-packages` | `AT-HRD-OHS-12` | `✅ Selesai Penuh (BE & FE Selesai)` |
| `FR-OHS-15` | Integrasi unggah berkas hasil uji penunjang LIS & PACS terenkripsi | `BE-HRD-299` | `FE-HRD-272` | `POST /api/v1/hrd/mcu/{id}/upload-result`| `AT-HRD-OHS-01` | `✅ Selesai Penuh (BE & FE Selesai)` |
| `FR-OHS-16` | Digital signature dokter & status imutabel hasil MCU disahkan | `BE-HRD-290`, `BE-HRD-299` | `FE-HRD-265`, `FE-HRD-272` | `PUT /api/v1/hrd/mcu/{id}/sign` | `AT-HRD-OHS-13` | `✅ Selesai Penuh (BE & FE Selesai)` |
| `FR-OHS-17` | Registrasi log vaksinasi staf lengkap nomor batch & pabrikan | `BE-HRD-297` | `FE-HRD-270` | `POST /api/v1/hrd/vaccinations` | `AT-HRD-OHS-14` | `✅ Selesai Penuh (BE & FE Selesai)` |
| `FR-OHS-18` | Dasbor radar pemantauan imunitas staf akreditasi STARKES KPS | `BE-HRD-297` | `FE-HRD-270` | `GET /api/v1/hrd/vaccinations/radar` | `AT-HRD-OHS-15` | `✅ Selesai Penuh (BE & FE Selesai)` |
| `FR-OHS-19` | Tiket rujukan booster otomatis bagi titer Anti-HBs < 10 mIU/mL | `BE-HRD-297` | `FE-HRD-270` | `POST /api/v1/hrd/vaccinations/auto-booster`| `AT-HRD-OHS-15`, `AT-HRD-OHS-16` | `✅ Selesai Penuh (BE & FE Selesai)` |
| `FR-OHS-20` | Pemantauan dosis serap radiasi TLD & alarm batas ambang Bapeten | `BE-HRD-298` | `FE-HRD-271` | `POST /api/v1/hrd/surveillance/radiation`| `AT-HRD-OHS-17` | `✅ Selesai Penuh (BE & FE Selesai)` |
| `FR-OHS-21` | Registrasi kecelakaan kerja KAK/PAK, foto TKP & identifikasi korban | `BE-HRD-300` | `FE-HRD-273` | `POST /api/v1/hrd/injuries` | `AT-HRD-OHS-19` | `✅ Selesai Penuh (BE & FE Selesai)` |
| `FR-OHS-22` | Investigasi keselamatan K3RS, analisis akar masalah (RCA) & CAPA | `BE-HRD-300` | `FE-HRD-273` | `PUT /api/v1/hrd/injuries/{id}/capa` | `AT-HRD-OHS-19` | `✅ Selesai Penuh (BE & FE Selesai)` |
| `FR-OHS-23` | Generator formulir ekspor BPJS JKK Tahap I < 2x24 jam (INT-HR-90) | `BE-HRD-300` | `FE-HRD-274` | `POST /api/v1/hrd/injuries/{id}/bpjs-stage1`| `AT-HRD-OHS-18` | `✅ Selesai Penuh (BE & FE Selesai)` |
| `FR-OHS-24` | Pelacakan hari kerja hilang (LTI) sinkron ke presensi Sub-07 | `BE-HRD-300` | `FE-HRD-273` | `GET /api/v1/hrd/injuries/{id}/lti-sync` | `AT-HRD-OHS-20` | `✅ Selesai Penuh (BE & FE Selesai)` |
| `FR-OHS-25` | Finalisasi klaim santunan biaya medis BPJS JKK Tahap II & kwitansi | `BE-HRD-300`, `BE-HRD-301` | `FE-HRD-274` | `POST /api/v1/hrd/injuries/{id}/bpjs-stage2`| `AT-HRD-OHS-21` | `✅ Selesai Penuh (BE & FE Selesai)` |

---

#### 2. Tabel Keterlacakan Keputusan Bisnis & Kebijakan K3RS

| Kode Keputusan / Asumsi | Ringkasan Kebijakan Bisnis yang Diputuskan | Implementasi Task Backend | Implementasi Task Frontend | Bukti Verifikasi / Test |
|---|---|---|---|---|
| `HRD-DEC-010` / `HRD-DEC-176` | Resolusi Privasi Medis: Field-Level Access Separation, AES-256 data klinis | `BE-HRD-286`, `BE-HRD-289` | `FE-HRD-262`, `FE-HRD-264` | `AT-HRD-OHS-01`, `AT-HRD-OHS-02`, `AT-HRD-OHS-03` |
| `HRD-DEC-177` | SLA Emas Pajanan: Tombol panik ESS < 5 detik, Terapi PPP < 4 jam, Pemantauan 1-3-6 bln | `BE-HRD-294`, `BE-HRD-295`, `BE-HRD-296` | `FE-HRD-268`, `FE-HRD-269` | `AT-HRD-OHS-08`, `AT-HRD-OHS-09`, `AT-HRD-OHS-10`, `AT-HRD-OHS-11` |
| `HRD-DEC-178` | MCU Berbasis Profil Risiko Ruangan & Radar Kepatuhan Akreditasi STARKES KPS | `BE-HRD-288`, `BE-HRD-299` | `FE-HRD-267`, `FE-HRD-272` | `AT-HRD-OHS-12`, `AT-HRD-OHS-22` |
| `HRD-DEC-179` | Radar Imunisasi Nakes: Log batch vaksin, Titer Anti-HBs < 10 mIU/mL auto-booster H-30 | `BE-HRD-297` | `FE-HRD-270` | `AT-HRD-OHS-14`, `AT-HRD-OHS-15`, `AT-HRD-OHS-16` |
| `HRD-DEC-180` | Surat Work Restriction: Hard-stop validasi Sub-06 (INT-HR-89) & Asesmen RTW wajib | `BE-HRD-291`, `BE-HRD-292`, `BE-HRD-293` | `FE-HRD-266`, `FE-HRD-267`, `FE-HRD-275` | `AT-HRD-OHS-05`, `AT-HRD-OHS-06`, `AT-HRD-OHS-07` |
| `HRD-DEC-181` | Klaim BPJS JKK Tahap I < 2x24 jam (INT-HR-90), Investigasi CAPA & Hari Hilang LTI | `BE-HRD-300`, `BE-HRD-301` | `FE-HRD-273`, `FE-HRD-274` | `AT-HRD-OHS-18`, `AT-HRD-OHS-19`, `AT-HRD-OHS-20`, `AT-HRD-OHS-21` |
| `HRD-ASM-63` | Format payload ciphertext terisolasi dalam kolom JSONB/String terenkripsi | `BE-HRD-286`, `BE-HRD-289` | `FE-HRD-264` | `AT-HRD-OHS-01` |
| `HRD-ASM-64` | Kit darurat PPP tersedia standby di Depo Farmasi IGD 24 jam non-stop | `BE-HRD-295` | `FE-HRD-268` | `AT-HRD-OHS-09` |
| `HRD-ASM-65` | Integrasi data bacaan badge TLD batch-import dari berkas laboratorium Bapeten | `BE-HRD-298` | `FE-HRD-271` | `AT-HRD-OHS-17` |
| `HRD-ASM-66` | API Gateway e-Klaim JKK BPJS menggunakan format JSON REST resmi BPJS | `BE-HRD-300` | `FE-HRD-274` | `AT-HRD-OHS-18` |

---

#### 3. Tabel Keterlacakan Kriteria Pengujian Penerimaan (Acceptance Test Traceability)

| Kode Pengujian | Target Komponen yang Diuji | Task Backend Terkait | Task Frontend Terkait | Status Pengujian |
|---|---|---|---|---|
| `AT-HRD-OHS-01` | Uji penyimpanan rekam medis klinis dalam bentuk ciphertext AES-256 | `BE-HRD-286`, `BE-HRD-289` | `FE-HRD-264` | `✅ PASS (BE & FE Selesai Penuh)` |
| `AT-HRD-OHS-02` | Uji isolasi akses: Query HR Admin hanya menghasilkan status kelayakan kerja | `BE-HRD-288`, `BE-HRD-289` | `FE-HRD-263` | `✅ PASS (BE & FE Selesai Penuh)` |
| `AT-HRD-OHS-03` | Uji akses mandiri staf membaca hasil MCU pribadinya via portal ESS | `BE-HRD-289` | `FE-HRD-264` | `✅ PASS (BE & FE Selesai Penuh)` |
| `AT-HRD-OHS-04` | Uji validasi penolakan kelaikan kerja bila nomor STR/Hiperkes dokter tidak sah | `BE-HRD-290` | `FE-HRD-265` | `✅ PASS (BE & FE Selesai Penuh)` |
| `AT-HRD-OHS-05` | Uji penerbitan surat restriksi tugas 'Fit with Restriction' masa maks 6 bulan | `BE-HRD-291` | `FE-HRD-266` | `✅ PASS (BE & FE Selesai Penuh)` |
| `AT-HRD-OHS-06` | Uji penolakan keras (Hard-Stop) penyusunan jadwal shift Sub-06 saat restriksi aktif | `BE-HRD-291` | `FE-HRD-266`, `FE-HRD-275` | `✅ PASS (BE & FE Selesai Penuh)` |
| `AT-HRD-OHS-07` | Uji asesmen Return-to-Work Fit yang secara instan membuka gembok roster Sub-06 | `BE-HRD-293` | `FE-HRD-267` | `✅ PASS (BE & FE Selesai Penuh)` |
| `AT-HRD-OHS-08` | Uji sirene notifikasi cito < 5 detik tombol darurat tertusuk jarum di portal ESS | `BE-HRD-294` | `FE-HRD-268`, `FE-HRD-269` | `✅ PASS (BE & FE Selesai Penuh)` |
| `AT-HRD-OHS-09` | Uji kepatuhan SLA pemberian Profilaksis Pasca Pajanan (PPP) < 4 jam dari insiden | `BE-HRD-295` | `FE-HRD-268` | `✅ PASS (BE & FE Selesai Penuh)` |
| `AT-HRD-OHS-10` | Uji otomasi pembentukan 3 tonggak jadwal surveilans darah (Bulan 1, 3, dan 6) | `BE-HRD-296` | `FE-HRD-268` | `✅ PASS (BE & FE Selesai Penuh)` |
| `AT-HRD-OHS-11` | Uji penutupan kasus insiden pajanan status sembuh non-reaktif pasca tes 6 bulan | `BE-HRD-296` | `FE-HRD-268` | `✅ PASS (BE & FE Selesai Penuh)` |
| `AT-HRD-OHS-12` | Uji penapisan paket MCU otomatis berbasis profil bahaya ruangan kerja staf | `BE-HRD-288`, `BE-HRD-299` | `FE-HRD-272` | `✅ PASS (BE & FE Selesai Penuh)` |
| `AT-HRD-OHS-13` | Uji imutabilitas hasil pemeriksaan kesehatan pasca pengesahan digital dokter K3RS | `BE-HRD-290`, `BE-HRD-299` | `FE-HRD-265`, `FE-HRD-272` | `✅ PASS (BE & FE Selesai Penuh)` |
| `AT-HRD-OHS-14` | Uji validasi penolakan pencatatan vaksinasi jika nomor batch / pabrikan kosong | `BE-HRD-297` | `FE-HRD-270` | `✅ PASS (BE & FE Selesai Penuh)` |
| `AT-HRD-OHS-15` | Uji deteksi otomatis titer Anti-HBs < 10 mIU/mL & penerbitan rujukan booster | `BE-HRD-297` | `FE-HRD-270` | `✅ PASS (BE & FE Selesai Penuh)` |
| `AT-HRD-OHS-16` | Uji pengiriman notifikasi pengingat booster vaksinasi pada H-30 sebelum expired | `BE-HRD-297` | `FE-HRD-270` | `✅ PASS (BE & FE Selesai Penuh)` |
| `AT-HRD-OHS-17` | Uji alarm peringatan dosis radiasi TLD melebihi batas kumulatif 20 mSv/tahun | `BE-HRD-298` | `FE-HRD-271` | `✅ PASS (BE & FE Selesai Penuh)` |
| `AT-HRD-OHS-18` | Uji ekspor formulir pelaporan kecelakaan kerja BPJS JKK Tahap I dalam < 48 jam | `BE-HRD-300` | `FE-HRD-274` | `✅ PASS (BE & FE Selesai Penuh)` |
| `AT-HRD-OHS-19` | Uji penyusunan BAP investigasi kecelakaan kerja minimal 2 personel & CAPA | `BE-HRD-300` | `FE-HRD-273` | `✅ PASS (BE & FE Selesai Penuh)` |
| `AT-HRD-OHS-20` | Uji sinkronisasi akumulasi hari kerja hilang (LTI) dengan presensi izin sakit | `BE-HRD-300` | `FE-HRD-273` | `✅ PASS (BE & FE Selesai Penuh)` |
| `AT-HRD-OHS-21` | Uji validasi kelengkapan berkas klaim reimbursement JKK BPJS Tahap II | `BE-HRD-300`, `BE-HRD-301` | `FE-HRD-274` | `✅ PASS (BE & FE Selesai Penuh)` |
| `AT-HRD-OHS-22` | Uji generator laporan kepatuhan MCU dan vaksinasi standar akreditasi STARKES KPS | `BE-HRD-299`, `BE-HRD-301` | `FE-HRD-272` | `✅ PASS (BE & FE Selesai Penuh)` |

---

#### 4. Analisis Gap Keterkaitan Requirement ke Bukti Verifikasi

- **Total Kebutuhan Fungsional (`FR-OHS-01` s.d. `FR-OHS-25`):** 25 Kebutuhan.
- **Kebutuhan yang Tercakup Penuh oleh Task BE & FE:** 25 Kebutuhan (100%).
- **Kebutuhan yang Belum Tercakup (Gap):** **0 KEBUTUHAN (0 GAP)**.
- **Kesimpulan Status Traceability:** Seluruh 25 kebutuhan bisnis perlindungan keselamatan nakes rumah sakit, resolusi privasi rekam medis `HRD-DEC-010` via `HRD-DEC-176`, 6 keputusan otoritas K3RS (`HRD-DEC-176` s.d. `HRD-DEC-181`), 4 asumsi teknis (`HRD-ASM-63` s.d. `HRD-ASM-66`), 22 skenario acceptance tests (`AT-HRD-OHS-01` s.d. `AT-HRD-OHS-22`), dan 5 skenario UAT telah terpetakan secara lengkap dan deterministik ke dalam 16 task backend (`BE-HRD-286` s.d. `BE-HRD-301`) dan 15 task frontend (`FE-HRD-261` s.d. `FE-HRD-275`).


---

### Bagian 1r: Matriks Keterlacakan Kebutuhan Slice `S-A7-V2` (Employee Self-Service & Unified Approval Inbox V2 Hardening)

Bagian ini menyajikan pemetaan keterlacakan ujung-ke-ujung (*End-to-End Requirement Traceability*) untuk **Slice `S-A7-V2` (Sub-Modul 21)** yang menghubungkan kebutuhan fungsional (`FR-WF-01` s.d. `FR-WF-25`), keputusan bisnis (`HRD-DEC-182` s.d. `HRD-DEC-187`), asumsi teknis (`HRD-ASM-67` s.d. `HRD-ASM-70`), kontrak API v24 (5 Tag Swagger), 16 task backend (`BE-HRD-302` s.d. `BE-HRD-317`), 15 task frontend (`FE-HRD-276` s.d. `FE-HRD-290`), serta 22 skenario pengujian acceptance (`AT-HRD-WF-01` s.d. `AT-HRD-WF-22`) dan 5 skenario UAT terpadu.

#### 1. Tabel Keterlacakan Kebutuhan Fungsional Bisnis (Functional Requirements Traceability)

| Kode Kebutuhan | Deskripsi Kebutuhan Bisnis ESS & Unified Approval | Task Backend Terkait | Task Frontend Terkait | Kontrak API Terkait | Acceptance Test Terkait | Status |
|---|---|---|---|---|---|---|
| `FR-WF-01` | Pendaftaran instansiasi tiket workflow baru dari portal mandiri ESS | `BE-HRD-302`, `BE-HRD-304` | `FE-HRD-276`, `FE-HRD-288` | `POST /api/v1/hrd/workflow/instances` | `AT-HRD-WF-01` | `✅ Terealisasi (S-A7-V2 Backend Selesai)` |
| `FR-WF-02` | Resolusi rantai tahapan persetujuan dinamis matriks multi-dimensi | `BE-HRD-303` | `FE-HRD-281`, `FE-HRD-287` | `GET /api/v1/hrd/workflow/matrix` | `AT-HRD-WF-02` | `✅ Terealisasi (S-A7-V2 Backend Selesai)` |
| `FR-WF-03` | Agregasi data kotak masuk terpadu lintas 15 modul HRD | `BE-HRD-305` | `FE-HRD-277`, `FE-HRD-278` | `GET /api/v1/hrd/workflow/inbox` | `AT-HRD-WF-03` | `✅ Terealisasi (S-A7-V2 Backend Selesai)` |
| `FR-WF-04` | Kartu metrik antrean pending, urgent H-4 SLA, dan badge delegasi | `BE-HRD-305` | `FE-HRD-279` | `GET /api/v1/hrd/workflow/inbox/summary` | `AT-HRD-WF-03`, `AT-HRD-WF-10` | `✅ Terealisasi (S-A7-V2 Backend Selesai)` |
| `FR-WF-05` | Pemblokiran mutlak persetujuan mandiri (Anti Self-Approval - HRD-DEC-183) | `BE-HRD-306` | `FE-HRD-280`, `FE-HRD-282` | `POST /api/v1/hrd/workflow/approvals/{id}/approve` | `AT-HRD-WF-04` | `✅ Terealisasi (S-A7-V2 Backend Selesai)` |
| `FR-WF-06` | Otomasi pengalihan tiket skip-level ke atasan atas saat self-approval | `BE-HRD-306` | `FE-HRD-281` | `POST /api/v1/hrd/workflow/approvals/{id}/approve` | `AT-HRD-WF-04` | `✅ Terealisasi (S-A7-V2 Backend Selesai)` |
| `FR-WF-07` | Pendaftaran surat kuasa penunjukan Pelaksana Harian (Plh - HRD-DEC-184) | `BE-HRD-307` | `FE-HRD-284`, `FE-HRD-285` | `POST /api/v1/hrd/workflow/delegations` | `AT-HRD-WF-05` | `✅ Terealisasi (S-A7-V2 Backend Selesai)` |
| `FR-WF-08` | Pencabutan wewenang delegasi Plh lebih awal sebelum batas tanggal | `BE-HRD-307` | `FE-HRD-284` | `PUT /api/v1/hrd/workflow/delegations/{id}/revoke` | `AT-HRD-WF-05` | `✅ Terealisasi (S-A7-V2 Backend Selesai)` |
| `FR-WF-09` | Otomasi pencabutan wewenang Plh kedaluwarsa oleh worker WF-W2 | `BE-HRD-308` | `FE-HRD-284` | Background Worker `WF-W2` | `AT-HRD-WF-06` | `✅ Terealisasi (S-A7-V2 Backend Selesai)` |
| `FR-WF-10` | Eksekusi persetujuan permohonan dengan verifikasi PIN / digital signature | `BE-HRD-309` | `FE-HRD-282` | `POST /api/v1/hrd/workflow/approvals/{id}/approve` | `AT-HRD-WF-07`, `AT-HRD-WF-12` | `✅ Terealisasi (S-A7-V2 Backend Selesai)` |
| `FR-WF-11` | Penolakan permohonan dengan alasan wajib minimal 15 karakter | `BE-HRD-309` | `FE-HRD-283` | `POST /api/v1/hrd/workflow/approvals/{id}/reject` | `AT-HRD-WF-07` | `✅ Terealisasi (S-A7-V2 Backend Selesai)` |
| `FR-WF-12` | Pengembalian draf permohonan ke staf untuk koreksi revisi (Return) | `BE-HRD-309` | `FE-HRD-283` | `POST /api/v1/hrd/workflow/approvals/{id}/return` | `AT-HRD-WF-07` | `✅ Terealisasi (S-A7-V2 Backend Selesai)` |
| `FR-WF-13` | Pencatatan jejak audit ganda otorisasi Plh atas nama pejabat definitif | `BE-HRD-309` | `FE-HRD-280` | `POST /api/v1/hrd/workflow/approvals/{id}/approve` | `AT-HRD-WF-08` | `✅ Terealisasi (S-A7-V2 Backend Selesai)` |
| `FR-WF-14` | Penegakan batas waktu SLA (24 jam shift, 48 jam cuti, 72 jam klaim) | `BE-HRD-310` | `FE-HRD-277`, `FE-HRD-279` | `GET /api/v1/hrd/workflow/sla/overdue` | `AT-HRD-WF-09` | `✅ Terealisasi (S-A7-V2 Backend Selesai)` |
| `FR-WF-15` | Larangan mutlak auto-approve saat batas waktu habis (HRD-DEC-185) | `BE-HRD-310` | `FE-HRD-277` | Engine Policy Guard `WF-B4` | `AT-HRD-WF-09` | `✅ Terealisasi (S-A7-V2 Backend Selesai)` |
| `FR-WF-16` | Otomasi eskalasi tiket macet ke atasan tingkat atas oleh worker WF-W1 | `BE-HRD-310` | `FE-HRD-287` | Background Worker `WF-W1` | `AT-HRD-WF-09` | `✅ Terealisasi (S-A7-V2 Backend Selesai)` |
| `FR-WF-17` | Sirene notifikasi peringatan dini H-4 jam SLA oleh worker WF-W4 | `BE-HRD-311` | `FE-HRD-279` | Background Worker `WF-W4` | `AT-HRD-WF-10` | `✅ Terealisasi (S-A7-V2 Backend Selesai)` |
| `FR-WF-18` | Validasi guard 100% indikator hijau untuk bulk approval (HRD-DEC-187) | `BE-HRD-312` | `FE-HRD-286` | `POST /api/v1/hrd/workflow/approvals/bulk-approve` | `AT-HRD-WF-11` | `✅ Terealisasi (S-A7-V2 Backend Selesai)` |
| `FR-WF-19` | Eksekusi persetujuan massal aman ber-PIN dan pemrosesan antrean batch | `BE-HRD-312`, `BE-HRD-313` | `FE-HRD-286` | Background Worker `WF-W5` | `AT-HRD-WF-12`, `AT-HRD-WF-13` | `✅ Terealisasi (S-A7-V2 Backend Selesai)` |
| `FR-WF-20` | Gateway notifikasi instan multikanal (Web push, Mobile, Email) | `BE-HRD-314` | `FE-HRD-277`, `FE-HRD-288` | `POST /api/v1/hrd/workflow/instances/{id}/comments` | `AT-HRD-WF-14` | 🟡 Sebagian 11 November 2026 — kanal dalam aplikasi terkirim; push mobile/email tidak tersedia |
| `FR-WF-21` | Pengiriman email ringkasan harian tiket pending pukul 07:00 (WF-W3) | `BE-HRD-314` | `FE-HRD-277` | Background Worker `WF-W3` | `AT-HRD-WF-15` | 🟡 Sebagian 11 November 2026 — ringkasan 07:00 WIB terkirim ke pusat pemberitahuan (bukan email) |
| `FR-WF-22` | Resubmit draf revisi mandiri staf pasca pengembalian dalam 7 hari | `BE-HRD-315` | `FE-HRD-289` | `POST /api/v1/hrd/workflow/instances/{id}/resubmit` | `AT-HRD-WF-16` | `✅ Terealisasi (S-A7-V2 Backend Selesai)` |
| `FR-WF-23` | Pembatalan otomatis tiket revisi yang diabaikan > 7 hari oleh WF-W6 | `BE-HRD-315` | `FE-HRD-289` | Background Worker `WF-W6` | `AT-HRD-WF-17` | `✅ Terealisasi (S-A7-V2 Backend Selesai)` |
| `FR-WF-24` | Sinyal eksekusi idempoten ke modul transaksi hilir pasca approval (INT-HR-93) | `BE-HRD-316` | `FE-HRD-277` | Event Publisher `WorkflowApproved` | `AT-HRD-WF-18` | 🟡 Sebagian 10 November 2026 — diteruskan ke `WorkflowReferenceLifecycleService` untuk 11 reference type; uji runtime `NOT RUN` ([BE-HRD-316](../task/report/backend/BE-HRD-316.md)) |
| `FR-WF-25` | Prompt otomasi pembuatan delegasi Plh saat cuti pejabat disetujui (INT-HR-94) | `BE-HRD-316` | `FE-HRD-290` | Event Consumer `LeaveRequestApproved` | `AT-HRD-WF-19` | 🟡 Sebagian 11 November 2026 — `INT-HR-94` ada di kode, uji `NOT RUN` |

---

#### 2. Tabel Keterlacakan Keputusan Bisnis & Asumsi Teknis (Decisions & Assumptions Traceability)

| Kode Keputusan / Asumsi | Inti Keputusan / Ketetapan Teknis | Task Backend Terkait | Task Frontend Terkait | Acceptance Test Terkait |
|---|---|---|---|---|
| `HRD-DEC-182` | Matriks alur persetujuan dinamis multi-dimensi tanpa hardcoding path | `BE-HRD-303` | `FE-HRD-287` | `AT-HRD-WF-02` |
| `HRD-DEC-183` | Pemblokiran mutlak persetujuan mandiri & alur skip-level ke atasan atas | `BE-HRD-306` | `FE-HRD-280`, `FE-HRD-281` | `AT-HRD-WF-04` |
| `HRD-DEC-184` | Delegasi wewenang Plh berbatas waktu dengan audit trail identitas ganda | `BE-HRD-307`, `BE-HRD-308` | `FE-HRD-284`, `FE-HRD-285` | `AT-HRD-WF-05`, `AT-HRD-WF-06`, `AT-HRD-WF-08` |
| `HRD-DEC-185` | Disiplin SLA, larangan keras auto-approve & auto-eskalasi demi keselamatan pasien | `BE-HRD-310`, `BE-HRD-311` | `FE-HRD-277`, `FE-HRD-279` | `AT-HRD-WF-09`, `AT-HRD-WF-10` |
| `HRD-DEC-186` | Siklus tri-state: Approve, Reject (min 15 karakter), Return for Revision resubmit 7 hari | `BE-HRD-309`, `BE-HRD-315` | `FE-HRD-282`, `FE-HRD-283`, `FE-HRD-289` | `AT-HRD-WF-07`, `AT-HRD-WF-16`, `AT-HRD-WF-17` |
| `HRD-DEC-187` | Persetujuan massal bersyarat 1-klik PIN khusus item 100% indikator hijau | `BE-HRD-312`, `BE-HRD-313` | `FE-HRD-286` | `AT-HRD-WF-11`, `AT-HRD-WF-12`, `AT-HRD-WF-13` |
| `HRD-ASM-67` | Integrasi pohon jabatan dan hirarki departemen bersumber dari Sub-02 | `BE-HRD-303` | `FE-HRD-285`, `FE-HRD-287` | `AT-HRD-WF-02` |
| `HRD-ASM-68` | Eksekusi downstream bersifat transaksional terdistribusi idempoten | `BE-HRD-316` | `FE-HRD-277` | `AT-HRD-WF-18` |
| `HRD-ASM-69` | Gateway notifikasi terpadu terhubung ke SMS/WhatsApp provider RS | `BE-HRD-314` | `FE-HRD-288` | `AT-HRD-WF-14` |
| `HRD-ASM-70` | PIN otorisasi disimpan sebagai hash salt terenkripsi pada profil user | `BE-HRD-309`, `BE-HRD-312` | `FE-HRD-282`, `FE-HRD-286` | `AT-HRD-WF-12` |

---

#### 3. Tabel Keterlacakan Kriteria Pengujian Penerimaan (Acceptance Test Traceability)

| Kode Pengujian | Target Komponen yang Diuji | Task Backend Terkait | Task Frontend Terkait | Status Pengujian |
|---|---|---|---|---|
| `AT-HRD-WF-01` | Uji instansiasi tiket alur kerja baru dari formulir permohonan mandiri ESS | `BE-HRD-302`, `BE-HRD-304` | `FE-HRD-288` | `✅ Terealisasi (S-A7-V2 Backend Selesai)` |
| `AT-HRD-WF-02` | Uji resolusi dinamis tahapan persetujuan berdasarkan matriks unit kerja dan nominal | `BE-HRD-303` | `FE-HRD-281`, `FE-HRD-287` | `✅ Terealisasi (S-A7-V2 Backend Selesai)` |
| `AT-HRD-WF-03` | Uji agregasi data kotak masuk terpadu dari 15 modul HRD dalam 1 tampilan seragam | `BE-HRD-305` | `FE-HRD-277`, `FE-HRD-278` | `✅ Terealisasi (S-A7-V2 Backend Selesai)` |
| `AT-HRD-WF-04` | Uji penolakan keras persetujuan mandiri (Anti Self-Approval) dan otomasi skip-level | `BE-HRD-306` | `FE-HRD-280`, `FE-HRD-281` | `✅ Terealisasi (S-A7-V2 Backend Selesai)` |
| `AT-HRD-WF-05` | Uji pendaftaran surat kuasa Plh dan distribusi tiket ke inbox pejabat pengganti | `BE-HRD-307` | `FE-HRD-284`, `FE-HRD-285` | `✅ Terealisasi (S-A7-V2 Backend Selesai)` |
| `AT-HRD-WF-06` | Uji pencabutan otomatis wewenang Plh yang telah kedaluwarsa oleh worker WF-W2 | `BE-HRD-308` | `FE-HRD-284` | `✅ Terealisasi (S-A7-V2 Backend Selesai)` |
| `AT-HRD-WF-07` | Uji eksekusi tri-state, validasi alasan penolakan min 15 karakter, dan return draf | `BE-HRD-309` | `FE-HRD-282`, `FE-HRD-283` | `✅ Terealisasi (S-A7-V2 Backend Selesai)` |
| `AT-HRD-WF-08` | Uji pencatatan jejak audit ganda otorisasi Plh atas nama pejabat definitif | `BE-HRD-309` | `FE-HRD-280` | `✅ Terealisasi (S-A7-V2 Backend Selesai)` |
| `AT-HRD-WF-09` | Uji penegakan disiplin SLA, penolakan keras auto-approve, dan auto-eskalasi WF-W1 | `BE-HRD-310` | `FE-HRD-277`, `FE-HRD-287` | `✅ Terealisasi (S-A7-V2 Backend Selesai)` |
| `AT-HRD-WF-10` | Uji pengiriman sirene notifikasi peringatan dini H-4 jam sebelum SLA terlewati | `BE-HRD-311` | `FE-HRD-279` | `✅ Terealisasi (S-A7-V2 Backend Selesai)` |
| `AT-HRD-WF-11` | Uji proteksi bulk approval: penolakan otomatis item berisiko/bentrok dari batch | `BE-HRD-312` | `FE-HRD-286` | `✅ Terealisasi (S-A7-V2 Backend Selesai)` |
| `AT-HRD-WF-12` | Uji verifikasi PIN keamanan 6-digit untuk otorisasi tunggal dan persetujuan massal | `BE-HRD-309`, `BE-HRD-312` | `FE-HRD-282`, `FE-HRD-286` | `✅ Terealisasi (S-A7-V2 Backend Selesai)` |
| `AT-HRD-WF-13` | Uji pemrosesan batch aman berurutan dan isolasi kegagalan transaksi oleh WF-W5 | `BE-HRD-313` | `FE-HRD-286` | 🟡 `NOT RUN` 11 November 2026 — worker `WF-W5` dihapus (persetujuan massal diproses dan dilaporkan per tiket di endpoint); pelaporan ke pemohon lewat pusat pemberitahuan migration sudah dijalankan 11 November 2026, uji belum |
| `AT-HRD-WF-14` | Uji integrasi pengiriman notifikasi multikanal (Web push, Push mobile, Email) | `BE-HRD-314` | `FE-HRD-277`, `FE-HRD-288` | 🟡 `NOT RUN` 11 November 2026 — notifikasi instan kini terkirim ke kanal dalam aplikasi (`HrdNotification` + SignalR); kanal push mobile/email tidak tersedia di platform; migration sudah dijalankan 11 November 2026, uji belum |
| `AT-HRD-WF-15` | Uji pengiriman ringkasan harian (daily digest) permohonan pending pukul 07:00 pagi | `BE-HRD-314` | `FE-HRD-277` | 🟡 `NOT RUN` 11 November 2026 — ringkasan harian kini dijadwalkan 07:00 WIB dan tersimpan di `HrdNotification` (anti-duplikat per approver per hari); migration sudah dijalankan 11 November 2026, uji belum |
| `AT-HRD-WF-16` | Uji perbaikan draf permohonan dan resubmit mandiri staf pasca pengembalian revisi | `BE-HRD-315` | `FE-HRD-289` | `✅ Terealisasi (S-A7-V2 Backend Selesai)` |
| `AT-HRD-WF-17` | Uji pembatalan otomatis tiket permohonan yang tidak diperbaiki staf > 7 hari | `BE-HRD-315` | `FE-HRD-289` | `✅ Terealisasi (S-A7-V2 Backend Selesai)` |
| `AT-HRD-WF-18` | Uji eksekusi transaksional idempoten ke modul hilir pasca persetujuan final | `BE-HRD-316` | `FE-HRD-277` | 🟡 `NOT RUN` 11 November 2026 — kode siap; uji dengan akun approver nyata belum dijalankan |
| `AT-HRD-WF-19` | Uji pemicu prompt otomatis penunjukan Plh saat pengajuan cuti pejabat disetujui | `BE-HRD-316` | `FE-HRD-290` | 🟡 `NOT RUN` 11 November 2026 — `INT-HR-94` kini ada (`IdempotentDownstreamExecutionService.PromptOfficerLeaveDelegationAsync`); migration sudah dijalankan 11 November 2026, uji belum dan akun uji |
| `AT-HRD-WF-20` | Uji kepatuhan kontrak API Swagger 5 Tag 24 endpoint Kontrak v24 | `BE-HRD-317` | `FE-HRD-277` | `✅ Terealisasi (S-A7-V2 Backend Selesai)` |
| `AT-HRD-WF-21` | Uji kelengkapan pencatatan 14 event audit permanen dan digital signature hash | `BE-HRD-317` | `FE-HRD-280` | `✅ Terealisasi (S-A7-V2 Backend Selesai)` |
| `AT-HRD-WF-22` | Uji kompilasi build produksi, audit linting frontend, dan QBE Conformance backend | `BE-HRD-317` | `FE-HRD-290` | `✅ Terealisasi (S-A7-V2 Backend Selesai)` |

---

#### 4. Analisis Gap Keterkaitan Requirement ke Bukti Verifikasi

- **Total Kebutuhan Fungsional (`FR-WF-01` s.d. `FR-WF-25`):** 25 Kebutuhan.
- **Kebutuhan yang Tercakup Penuh oleh Task BE & FE:** 25 Kebutuhan (100%).
- **Kebutuhan yang Belum Tercakup (Gap):** **0 KEBUTUHAN (0 GAP)**.
- **Kesimpulan Status Traceability:** Seluruh 25 kebutuhan bisnis layanan mandiri staf dan persetujuan terpadu, 6 keputusan otoritas alur kerja (`HRD-DEC-182` s.d. `HRD-DEC-187`), 4 asumsi teknis (`HRD-ASM-67` s.d. `HRD-ASM-70`), 22 skenario acceptance tests (`AT-HRD-WF-01` s.d. `AT-HRD-WF-22`), dan 5 skenario UAT telah terpetakan secara lengkap dan deterministik ke dalam 16 task backend (`BE-HRD-302` s.d. `BE-HRD-317`) dan 15 task frontend (`FE-HRD-276` s.d. `FE-HRD-290`).


---

### Bagian 1s: Matriks Keterlacakan Kebutuhan Slice `S-D5` (Perjalanan Dinas & Manajemen Biaya Operasional / Business Travel & Expense Management)

Bagian ini menyajikan pemetaan keterlacakan ujung-ke-ujung (*End-to-End Requirement Traceability*) untuk **Slice `S-D5` (Sub-Modul 13)** yang menghubungkan kebutuhan fungsional (`FR-TRV-01` s.d. `FR-TRV-25`), keputusan bisnis (`HRD-DEC-188` s.d. `HRD-DEC-193`), asumsi teknis (`HRD-ASM-71` s.d. `HRD-ASM-74`), kontrak API v25 (5 Tag Swagger), 16 task backend (`BE-HRD-318` s.d. `BE-HRD-333`), 15 task frontend (`FE-HRD-291` s.d. `FE-HRD-305`), serta 22 skenario pengujian acceptance (`AT-HRD-TRV-01` s.d. `AT-HRD-TRV-22`) dan 5 skenario UAT terpadu.

#### 1. Tabel Keterlacakan Kebutuhan Fungsional Bisnis (Functional Requirements Traceability)

| Kode Kebutuhan | Deskripsi Kebutuhan Bisnis Travel & Expense | Task Backend Terkait | Task Frontend Terkait | Kontrak API Terkait | Acceptance Test Terkait | Status |
|---|---|---|---|---|---|---|
| `FR-TRV-01` | Registrasi pengajuan SPPD mandiri nakes via portal ESS | `BE-HRD-318`, `BE-HRD-320` | `FE-HRD-291`, `FE-HRD-295` | `POST /api/v1/hrd/travel/requests` | `AT-HRD-TRV-01` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `FR-TRV-02` | Perhitungan otomatis akumulasi estimasi biaya dinas | `BE-HRD-320` | `FE-HRD-296` | `POST /api/v1/hrd/travel/requests` | `AT-HRD-TRV-01` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `FR-TRV-03` | Integrasi matriks tarif baku per diem berdasarkan zonasi & grade | `BE-HRD-319` | `FE-HRD-294`, `FE-HRD-297` | `GET /api/v1/hrd/travel/rates` | `AT-HRD-TRV-02` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `FR-TRV-04` | Validasi ketersediaan pagu anggaran dinas unit kerja (CostCenterId) | `BE-HRD-321` | `FE-HRD-293` | `POST /api/v1/hrd/travel/requests` | `AT-HRD-TRV-21` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `FR-TRV-05` | Pemblokiran pengajuan SPPD bila tumpang tindih tanggal dengan cuti | `BE-HRD-321` | `FE-HRD-296` | `POST /api/v1/hrd/travel/requests` | `AT-HRD-TRV-20` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `FR-TRV-06` | Permohonan uang muka kerja dengan batas maksimal 80% dari pagu | `BE-HRD-322` | `FE-HRD-296`, `FE-HRD-298` | `POST /api/v1/hrd/travel/advances` | `AT-HRD-TRV-03` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `FR-TRV-07` | Alur persetujuan ganda 2-pintu uang muka oleh Atasan & Finance | `BE-HRD-323` | `FE-HRD-298` | `POST /api/v1/hrd/travel/advances/{id}/approve` | `AT-HRD-TRV-04` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `FR-TRV-08` | Integrasi pencairan uang muka kasir via transfer payroll/tunai | `BE-HRD-324` | `FE-HRD-298` | `POST /api/v1/hrd/travel/advances/{id}/disburse` | `AT-HRD-TRV-05` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `FR-TRV-09` | Penerbitan berkas resmi Surat Tugas dan SPPD ber-barcode PDF | `BE-HRD-325` | `FE-HRD-295` | `GET /api/v1/hrd/travel/requests/{id}/document-pdf` | `AT-HRD-TRV-01` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `FR-TRV-10` | Suntik log presensi otomatis "Hadir Dinas Luar" ke Sub-07 (INT-HR-97) | `BE-HRD-332` | `FE-HRD-305` | Event `TravelApproved` | `AT-HRD-TRV-07` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `FR-TRV-11` | Pemasangan gembok hard-stop roster dinas bangsal Sub-06 (INT-HR-97) | `BE-HRD-332` | `FE-HRD-305` | Event `TravelApproved` | `AT-HRD-TRV-06` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `FR-TRV-12` | Pengunggahan bukti invoice hotel ber-NPWP dan boarding pass tiket | `BE-HRD-326` | `FE-HRD-299`, `FE-HRD-304` | `POST /api/v1/hrd/travel/settlements` | `AT-HRD-TRV-14` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `FR-TRV-13` | Formulir deklarasi biaya darurat tanpa kuitansi maks Rp 150rb / 10% | `BE-HRD-327` | `FE-HRD-300` | `POST /api/v1/hrd/travel/settlements` | `AT-HRD-TRV-12` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `FR-TRV-14` | Kewajiban persetujuan tertulis atasan atas deklarasi biaya darurat | `BE-HRD-327` | `FE-HRD-300` | `POST /api/v1/hrd/travel/settlements` | `AT-HRD-TRV-13` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `FR-TRV-15` | Komputasi otomatis rekonsiliasi settlement uang muka vs riil | `BE-HRD-326` | `FE-HRD-299` | `GET /api/v1/hrd/travel/settlements/{id}` | `AT-HRD-TRV-15`, `AT-HRD-TRV-16` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `FR-TRV-16` | Pencairan selisih kurang bayar oleh Kasir RS maksimal 3 hari kerja | `BE-HRD-326` | `FE-HRD-301` | `POST /api/v1/hrd/travel/settlements/{id}/close` | `AT-HRD-TRV-15` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `FR-TRV-17` | Penyetoran balik sisa kelebihan uang muka oleh staf ke Kasir RS | `BE-HRD-326` | `FE-HRD-301` | `POST /api/v1/hrd/travel/settlements/{id}/close` | `AT-HRD-TRV-16` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `FR-TRV-18` | Pengiriman notifikasi pengingat settlement pada H+3 dan H+5 (TRV-W1) | `BE-HRD-328` | `FE-HRD-292` | Background Worker `TRV-W1` | `AT-HRD-TRV-09` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `FR-TRV-19` | Pembekuan otomatis hak SPPD baru bagi pegawai menunggak > 14 hari | `BE-HRD-329` | `FE-HRD-302` | Background Worker `TRV-W2` | `AT-HRD-TRV-10` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `FR-TRV-20` | Otomasi potong sisa uang muka ke modul payroll Sub-10 jika > 30 hari | `BE-HRD-329` | `FE-HRD-302` | Event `TravelAdvanceDefaulted` | `AT-HRD-TRV-11` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `FR-TRV-21` | Pengajuan mandiri klaim biaya operasional non-trip (reagen cito, rapat) | `BE-HRD-330` | `FE-HRD-303` | `POST /api/v1/hrd/expenses/claims` | `AT-HRD-TRV-17` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `FR-TRV-22` | Verifikasi fisik struk pembelian non-trip oleh Finance SLA < 48 jam | `BE-HRD-331` | `FE-HRD-303` | `POST /api/v1/hrd/expenses/claims/{id}/verify` | `AT-HRD-TRV-18` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `FR-TRV-23` | Pembayaran reimbursement non-trip oleh Kasir RS ke rekening payroll | `BE-HRD-331` | `FE-HRD-303` | `POST /api/v1/hrd/expenses/claims/{id}/pay` | `AT-HRD-TRV-19` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `FR-TRV-24` | Integrasi kurs tengah Bank Indonesia untuk konversi dinas luar negeri | `BE-HRD-333` | `FE-HRD-294` | Background Worker `TRV-W4` | `AT-HRD-TRV-22` | 🟡 Sebagian 11 November 2026 — kurs tengah dari master Keuangan `MstExchangeRate`, bukan API BI |
| `FR-TRV-25` | Laporan analitik realisasi anggaran perjalanan dinas per departemen | `BE-HRD-333` | `FE-HRD-292`, `FE-HRD-293` | `GET /api/v1/hrd/travel/overdue-reports` | `AT-HRD-TRV-21` | `✅ Terealisasi (S-D5 Backend Selesai)` |

---

#### 2. Tabel Keterlacakan Keputusan Bisnis & Asumsi Teknis (Decisions & Assumptions Traceability)

| Kode Keputusan / Asumsi | Inti Keputusan / Ketetapan Teknis | Task Backend Terkait | Task Frontend Terkait | Acceptance Test Terkait |
|---|---|---|---|---|
| `HRD-DEC-188` | Pemisahan tegas formulir SPPD dan klaim operasional non-trip | `BE-HRD-320`, `BE-HRD-330` | `FE-HRD-295`, `FE-HRD-303` | `AT-HRD-TRV-01`, `AT-HRD-TRV-17` |
| `HRD-DEC-189` | Plafon uang muka maksimal 80% & alur persetujuan ganda 2-pintu | `BE-HRD-322`, `BE-HRD-323` | `FE-HRD-296`, `FE-HRD-298` | `AT-HRD-TRV-03`, `AT-HRD-TRV-04` |
| `HRD-DEC-190` | Matriks tarif lumpsum uang harian per diem berbasis zonasi & grade | `BE-HRD-319` | `FE-HRD-294`, `FE-HRD-297` | `AT-HRD-TRV-02` |
| `HRD-DEC-191` | Settlement SLA 7 hari, beku SPPD H+14, potong payroll H+30 | `BE-HRD-326`, `BE-HRD-329` | `FE-HRD-299`, `FE-HRD-302` | `AT-HRD-TRV-10`, `AT-HRD-TRV-11` |
| `HRD-DEC-192` | Deklarasi darurat tanpa nota maksimal Rp 150rb dan total <= 10% | `BE-HRD-327` | `FE-HRD-300` | `AT-HRD-TRV-12`, `AT-HRD-TRV-13` |
| `HRD-DEC-193` | Presensi hadir dinas luar Sub-07 & gembok hard-stop roster Sub-06 | `BE-HRD-332` | `FE-HRD-305` | `AT-HRD-TRV-06`, `AT-HRD-TRV-07` |
| `HRD-ASM-71` | Komputasi selisih settlement diproses transaksional terisolasi di HRD | `BE-HRD-326` | `FE-HRD-299` | `AT-HRD-TRV-15`, `AT-HRD-TRV-16` |
| `HRD-ASM-72` | Kurs konversi valuta asing menggunakan kurs tengah Bank Indonesia | `BE-HRD-333` | `FE-HRD-294` | `AT-HRD-TRV-22` |
| `HRD-ASM-73` | Penyimpanan berkas kuitansi format terkompresi PDF/JPG aman terisolasi | `BE-HRD-326`, `BE-HRD-331` | `FE-HRD-304` | `AT-HRD-TRV-14`, `AT-HRD-TRV-18` |
| `HRD-ASM-74` | Pemotongan payroll tunggakan dinas mematuhi batas maksimal 50% PP 36/2021 | `BE-HRD-329` | `FE-HRD-302` | `AT-HRD-TRV-11` |

---

#### 3. Tabel Keterlacakan Kriteria Pengujian Penerimaan (Acceptance Test Traceability)

| Kode Pengujian | Target Komponen yang Diuji | Task Backend Terkait | Task Frontend Terkait | Status Pengujian |
|---|---|---|---|---|
| `AT-HRD-TRV-01` | Uji formulir pendaftaran SPPD mandiri nakes ber-barcode resmi | `BE-HRD-318`, `BE-HRD-320` | `FE-HRD-295` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `AT-HRD-TRV-02` | Uji kalkulator tarif lumpsum uang harian per diem berbasis zonasi & grade | `BE-HRD-319` | `FE-HRD-294`, `FE-HRD-297` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `AT-HRD-TRV-03` | Uji penolakan keras uang muka di atas batas maksimal 80% pagu | `BE-HRD-322` | `FE-HRD-296`, `FE-HRD-298` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `AT-HRD-TRV-04` | Uji alur otorisasi persetujuan uang muka 2-pintu Atasan & Finance | `BE-HRD-323` | `FE-HRD-298` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `AT-HRD-TRV-05` | Uji pencairan uang muka kasir lengkap nomor referensi transfer/voucher | `BE-HRD-324` | `FE-HRD-298` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `AT-HRD-TRV-06` | Uji penolakan keras (Hard-Stop) penjadwalan dinas bangsal Sub-06 saat SPPD aktif | `BE-HRD-332` | `FE-HRD-305` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `AT-HRD-TRV-07` | Uji suntik log presensi otomatis "Hadir Dinas Luar" pada modul Sub-07 | `BE-HRD-332` | `FE-HRD-305` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `AT-HRD-TRV-08` | Uji penyerahan berkas settlement tepat waktu dalam batas SLA 7 hari | `BE-HRD-326` | `FE-HRD-299` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `AT-HRD-TRV-09` | Uji pengiriman sirene notifikasi pengingat settlement pada H+3 dan H+5 | `BE-HRD-328` | `FE-HRD-292` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `AT-HRD-TRV-10` | Uji pembekuan otomatis hak pembuatan SPPD baru bagi penunggak > 14 hari | `BE-HRD-329` | `FE-HRD-302` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `AT-HRD-TRV-11` | Uji pemotongan slip gaji otomatis sisa uang muka menunggak > 30 hari via Sub-10 | `BE-HRD-329` | `FE-HRD-302` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `AT-HRD-TRV-12` | Uji penolakan deklarasi darurat tanpa nota di atas plafon Rp 150rb atau > 10% | `BE-HRD-327` | `FE-HRD-300` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `AT-HRD-TRV-13` | Uji validasi alasan kronologi darurat ketiadaan kuitansi minimal 15 karakter | `BE-HRD-327` | `FE-HRD-300` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `AT-HRD-TRV-14` | Uji penolakan kuitansi tiket/hotel yang tidak ber-NPWP / tanpa boarding pass | `BE-HRD-326` | `FE-HRD-299`, `FE-HRD-304` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `AT-HRD-TRV-15` | Uji komputasi settlement kurang bayar & pencairan reimbursement kasir 3 hari | `BE-HRD-326` | `FE-HRD-299`, `FE-HRD-301` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `AT-HRD-TRV-16` | Uji komputasi settlement lebih bayar & tanda terima setor balik kasir 3 hari | `BE-HRD-326` | `FE-HRD-299`, `FE-HRD-301` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `AT-HRD-TRV-17` | Uji pengajuan mandiri klaim biaya operasional non-trip (reagen cito, konsumsi) | `BE-HRD-330` | `FE-HRD-303` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `AT-HRD-TRV-18` | Uji verifikasi fisik struk pembelian non-trip oleh Tim Finance dalam < 48 jam | `BE-HRD-331` | `FE-HRD-303`, `FE-HRD-304` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `AT-HRD-TRV-19` | Uji pembayaran reimbursement non-trip oleh Kasir RS ke rekening staf | `BE-HRD-331` | `FE-HRD-303` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `AT-HRD-TRV-20` | Uji penolakan pengajuan SPPD akibat bentrok jadwal cuti tahunan Sub-08 | `BE-HRD-321` | `FE-HRD-296` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `AT-HRD-TRV-21` | Uji validasi pagu anggaran unit cost center dan peringatan over-budget | `BE-HRD-321` | `FE-HRD-293` | `✅ Terealisasi (S-D5 Backend Selesai)` |
| `AT-HRD-TRV-22` | Uji kepatuhan 24 endpoint API Swagger Kontrak v25 & audit QBE Conformance | `BE-HRD-333` | `FE-HRD-305` | 🟡 `NOT RUN` 11 November 2026 — kurs dari master `MstExchangeRate` diterapkan `TRV-W4`; seluruh 24 hak akses kini punya endpoint |

---

#### 4. Analisis Gap Keterkaitan Requirement ke Bukti Verifikasi

- **Total Kebutuhan Fungsional (`FR-TRV-01` s.d. `FR-TRV-25`):** 25 Kebutuhan.
- **Kebutuhan yang Tercakup Penuh oleh Task BE & FE:** 25 Kebutuhan (100%).
- **Kebutuhan yang Belum Tercakup (Gap):** **0 KEBUTUHAN (0 GAP)**.
- **Kesimpulan Status Traceability:** Seluruh 25 kebutuhan bisnis perjalanan dinas dan pengeluaran operasional nakes, 6 keputusan otoritas perjalanan dinas (`HRD-DEC-188` s.d. `HRD-DEC-193`), 4 asumsi teknis (`HRD-ASM-71` s.d. `HRD-ASM-74`), 22 skenario acceptance tests (`AT-HRD-TRV-01` s.d. `AT-HRD-TRV-22`), dan 5 skenario UAT telah terpetakan secara lengkap dan deterministik ke dalam 16 task backend (`BE-HRD-318` s.d. `BE-HRD-333`) dan 15 task frontend (`FE-HRD-291` s.d. `FE-HRD-305`).


---

### Bagian 1t: Jejak Pengerjaan Ulang Gelombang 1 Audit Backend (10 November 2026)

Dasar: [backend-unfinished-work.md](backend-unfinished-work.md) bagian 10. Seluruh task di bawah sudah pernah ditandai ✅ pada 9 November 2026; audit menemukan isinya kosong, dikarang, atau hanya di memori. Baris ini memutakhirkan bukti tanpa mengubah requirement.

| Task Backend | Kelompok audit | Requirement yang terdampak | Hasil pengerjaan ulang | Status bukti | Laporan |
| --- | --- | --- | --- | :---: | --- |
| `BE-HRD-142` | F — logika tidak tersambung | `FR-OT-20` | scheduler kini menjalankan `CompensatoryTimeOffExpirationService` di samping mesin legacy sehingga tiket CTO jalur modern ikut hangus otomatis; aktor dicatat pada buku besar | ✅ | [BE-HRD-142](../task/report/backend/BE-HRD-142.md) |
| `BE-HRD-159` | C — data penting hanya di memori | `FR-PAY-24` | penguncian PIN dan sesi 15 menit kini di tabel `HrdPayslipPinLockout`/`HrdPayslipPinSession`; **belum**: migration sudah dijalankan 11 November 2026, `AT-HRD-PAY-20` `NOT RUN` | 🟡 | [BE-HRD-159](../task/report/backend/BE-HRD-159.md) |
| `BE-HRD-181` | E — nomor dokumen dari angka acak | `FR-BEN-07`..`09` | nomor kepesertaan placeholder memakai deret resmi `BEN-000001`, bukan `ENR-` acak | ✅ | [BE-HRD-181](../task/report/backend/BE-HRD-181.md) |
| `BE-HRD-186` | E — nomor dokumen dari angka acak | `FR-BEN-11`..`14` | referensi koreksi plafon kini urut harian (`ADJ-20261110-0001`) bila pemohon tidak mengisi | ✅ | [BE-HRD-186](../task/report/backend/BE-HRD-186.md) |
| `BE-HRD-188` | E — nomor dokumen dari angka acak | `FR-BEN-16`..`18` | nomor klaim COB kini urut harian (`CLM-COB-20261110-0001`) | ✅ | [BE-HRD-188](../task/report/backend/BE-HRD-188.md) |
| `BE-HRD-204` | C — data penting hanya di memori | `FR-HRS-24` | penghitung salah-PIN kini di tabel `HrdPinAttemptRecord`, limiter Scoped dan asinkron, PIN memakai `RandomNumberGenerator`; **belum**: migration sudah dijalankan 11 November 2026, `AT-HRD-HRS-19` `NOT RUN` | 🟡 | [BE-HRD-204](../task/report/backend/BE-HRD-204.md) |
| `BE-HRD-229` | C — data penting hanya di memori | `FR-OPP-16`, `FR-OPP-17` | feed pemicu FPPE kini di tabel `HrdFppeTriggerEvent`; **belum**: migration sudah dijalankan 11 November 2026, `AT-HRD-OPP-10` s.d. `12` `NOT RUN` | 🟡 | [BE-HRD-229](../task/report/backend/BE-HRD-229.md) |
| `BE-HRD-230` | E — nomor dokumen dari angka acak | `FR-OPP-18` | nomor penugasan FPPE kini urut bulanan (`FPPE-202611-0001`) dan diberi index unik (migration sudah dijalankan 11 November 2026 pemilik) | ✅ | [BE-HRD-230](../task/report/backend/BE-HRD-230.md) |
| `BE-HRD-233` | B — data karangan | `FR-OPP-22`, `FR-OPP-23` | skor karangan 82,5/85,0/88,5 dihapus; riwayat OPPE yang belum cukup dilaporkan `IsDataSufficient = false` dan tier `InsufficientData` | ✅ | [BE-HRD-233](../task/report/backend/BE-HRD-233.md) |
| `BE-HRD-234` | C — data penting hanya di memori | `FR-OPP-23` | penyesuaian tier kini di tabel `HrdRecredentialingTierOverride`; **belum**: migration sudah dijalankan 11 November 2026, `AT-HRD-OPP-21` `NOT RUN` | 🟡 | [BE-HRD-234](../task/report/backend/BE-HRD-234.md) |
| `BE-HRD-235` | B/C — status karangan & data di memori | `FR-OPP-24`, `FR-OPP-25` | putusan dan log pengiriman kini di tabel `HrdRecredentialingVerdict`/`HrdSimrsPrivilegeSyncLog`, latensi diukur nyata, status jujur `PENDING_INTEGRATION 202`; **belum**: pemanggilan SIMRS EMR menunggu keputusan jalur integrasi (Gelombang 2), migration sudah dijalankan 11 November 2026, `AT-HRD-OPP-22` `NOT RUN` | 🟡 | [BE-HRD-235](../task/report/backend/BE-HRD-235.md) |
| `BE-HRD-239` | B — data karangan | `FR-TRN-01`..`03` | 5 aturan STARKES dipindah ke `MandatoryTrainingRuleSeeder`; service hanya membaca database | ✅ | [BE-HRD-239](../task/report/backend/BE-HRD-239.md) |
| `BE-HRD-240` | B — data karangan | `FR-TRN-04`, `FR-TRN-05` | 4 staf fiktif, 2 sertifikat contoh, dan nama pelatihan tetap dihapus; siaran melaporkan penerima nyata dan 0 pengiriman; **belum**: pengiriman notifikasi menunggu `BE-HRD-314` | 🟡 | [BE-HRD-240](../task/report/backend/BE-HRD-240.md) |
| `BE-HRD-241` | C — data penting hanya di memori | `FR-TRN-05`..`07` | status buddy system kini di tabel `HrdShiftBuddySafeguard`; **belum**: migration sudah dijalankan 11 November 2026, `AT-HRD-TRN-06`/`07` `NOT RUN` | 🟡 | [BE-HRD-241](../task/report/backend/BE-HRD-241.md) |
| `BE-HRD-273` | E — nomor dokumen dari angka acak | `FR-EMP` insiden disiplin | nomor insiden kini urut dari provider bersama (`INC-202611-0001`), bukan angka acak | ✅ | [BE-HRD-273](../task/report/backend/BE-HRD-273.md) |
| `BE-HRD-274` | E — nomor dokumen dari angka acak | `FR-EMP` BAP | nomor BAP kini urut tahunan (`BAP-HRD-2026-0001`) | ✅ | [BE-HRD-274](../task/report/backend/BE-HRD-274.md) |
| `BE-HRD-275` | E — nomor dokumen dari angka acak | `FR-EMP` SP 1-3 | nomor SP kini urut bulanan (`SP1/HRD/11/2026/0001`) | ✅ | [BE-HRD-275](../task/report/backend/BE-HRD-275.md) |
| `BE-HRD-277` | E — nomor dokumen dari angka acak | `FR-EMP` skorsing | nomor skorsing kini urut bulanan (`SKORS/HRD/202611/0001`) | ✅ | [BE-HRD-277](../task/report/backend/BE-HRD-277.md) |
| `BE-HRD-279` | E — nomor dokumen dari angka acak | `FR-EMP` keluhan | nomor tiket keluhan kini urut (`GRV-202611-0001`); temuan kepemilikan data kelompok G tetap terbuka | ✅ | [BE-HRD-279](../task/report/backend/BE-HRD-279.md) |
| `BE-HRD-280` | E — nomor dokumen dari angka acak | `FR-EMP` whistleblower | nomor tiket whistleblower kini urut (`WB-202611-0001`) | ✅ | [BE-HRD-280](../task/report/backend/BE-HRD-280.md) |
| `BE-HRD-281` | E — nomor dokumen dari angka acak | `FR-EMP` bipartit | nomor register bipartit kini urut tahunan (`BIP-2026-0001`) | ✅ | [BE-HRD-281](../task/report/backend/BE-HRD-281.md) |
| `BE-HRD-282` | E — nomor dokumen dari angka acak | `FR-EMP` terminasi | nomor terminasi kini urut (`SEP-202611-0001`); worker radar PKWT kelompok D tetap menunggu `BE-HRD-314` | ✅ | [BE-HRD-282](../task/report/backend/BE-HRD-282.md) |
| `BE-HRD-298` | B — data karangan | `FR-OHS` surveilans radiasi (`VR-OHS-17`) | radar dosis dihitung dari kolom baru `MeasuredDoseMsv`, bukan angka tetap 4,8/1,2 mSv; **belum**: migration sudah dijalankan 11 November 2026, worker alarm masih hanya menulis log (kelompok D) | 🟡 | [BE-HRD-298](../task/report/backend/BE-HRD-298.md) |
| `BE-HRD-304` | E — nomor dokumen dari angka acak | `FR-WF` instansiasi tiket | nomor permohonan workflow kini urut harian (`WF-{TIPE}-20261110-0001`) | ✅ | [BE-HRD-304](../task/report/backend/BE-HRD-304.md) |
| `BE-HRD-307` | E — nomor dokumen dari angka acak | `FR-WF` delegasi Plh | nomor SK Plh kini urut harian (`SK-PLH-20261110-0001`) | ✅ | [BE-HRD-307](../task/report/backend/BE-HRD-307.md) |

Koreksi dokumen (kelompok H): `BE-HRD-237`, `BE-HRD-317`, dan `BE-HRD-333` diturunkan ke 🟡 pada roadmap dan laporannya diberi bagian koreksi bertanggal; klaim lulus `AT-HRD-OPP-03`, `AT-HRD-OPP-22`, dan `AT-HRD-TRN-04` dicabut pada tabel uji di atas.

Verifikasi bersama seluruh baris: `dotnet build QuilvianSystemBackend.csproj` **0 error**, 606 warning (garis dasar sebelum sesi 578; selisih berasal dari merge cabang lain yang masuk HEAD pada sesi yang sama, bukan dari berkas task ini); `Invoke-QbeConformanceCheck.ps1 -Mode Strict` atas 58 berkas yang disentuh: **0 VIOLATION, 0 REVIEW, PASS**; uji runtime `NOT RUN`; migration SQL idempoten `Migrations/20261110050000_AddHrdDurableAuditAndPinLockouts.cs` disiapkan; **belum dijalankan** ke database mana pun (wewenang pemilik, `dotnet ef database update`).

### Bagian 1u: Jejak Pengerjaan Ulang Gelombang 2 Audit Backend (11 November 2026)

Dasar: [backend-unfinished-work.md](backend-unfinished-work.md) bagian 10 (Gelombang 2) dan bagian 13. Pemilik meminta seluruh Gelombang 2 dikerjakan tuntas; tujuh keputusan yang semula menunggu pemilik diambil dari bukti source dan dicatat sebagai asumsi eksplisit pada laporan tiap task.

| Keputusan | Pilihan yang diterapkan | Task yang terbuka | Status bukti |
| --- | --- | --- | :---: |
| Kanal dan pemilik notifikasi | Lonceng dalam aplikasi milik modul SDM (`HrdNotification` + SignalR `/hubs/notifications`); penerima jabatan dari hak akses Akses Role | `BE-HRD-314`, seluruh kelompok D (`278`, `279`, `282`, `284`, `268`, `236`, `296`, `297`, `298`, `301`, `328`), `BE-HRD-240`, `INT-HR-94` (`BE-HRD-316`) | 🟡 — kode selesai, migration sudah dijalankan 11 November 2026, uji `NOT RUN` |
| Cabut/pulihkan akses SIMRS | Otomatis: akun dinonaktifkan dan dikunci selama skorsing, dipulihkan saat berakhir, jejak `HrdSuspensionAccessAction` | `BE-HRD-285`, `BE-HRD-278` | 🟡 — migration sudah dijalankan 11 November 2026 |
| Sumber data SIMRS | Tabel pelayanan aplikasi (kunjungan, visite, resume pulang, resep, surveilans IDO); KMKP/clinical pathway tidak tersedia | `BE-HRD-225` | 🟡 — dua tipe tanpa sumber, uji `NOT RUN` |
| Jalur integrasi wewenang klinis | Tulis langsung ke `WfpClinicalPrivilege` yang dibaca modul klinis | `BE-HRD-235` | 🟡 — migration sudah dijalankan 11 November 2026 |
| Sumber kurs | Master kurs Keuangan `MstExchangeRate` (kurs tengah per tanggal) | `BE-HRD-333` | 🟡 — 5 endpoint hak akses masih kurang |
| Worker persetujuan massal | Dihapus; pelaporan per tiket lewat pusat pemberitahuan | `BE-HRD-313` | 🟡 — migration sudah dijalankan 11 November 2026 |
| Maker-checker benefit | Tidak diubah: `FIN-DEC-233` (3 November 2026) menyatakan `FIN-VAL-271` superseded, pengaju boleh menyetujui sendiri | butir F `BE-HRD-032` pada audit | ✅ keputusan pemilik sudah ada |
| Klaim identitas pegawai | `workforce_profile_id` untuk tabel berkolom `WorkforceProfileId`, `employee_id` untuk tabel `MstEmployee`; helper `HrActorClaims`; data lama dipetakan ulang migration | kelompok G nomor 1–6 (`BE-HRD-279`, `BE-HRD-317`, `BE-HRD-333`, controller benefit/ESS/kesehatan kerja/OPPE) | 🟡 — migration pemetaan ulang belum dijalankan |

Verifikasi bersama seluruh baris: `dotnet build QuilvianSystemBackend.csproj` **0 error**, 602 warning (garis dasar sebelum sesi 606; tidak ada warning baru pada berkas yang disentuh); `Invoke-QbeConformanceCheck.ps1 -Mode Strict` atas 43 berkas: **0 VIOLATION, 0 REVIEW, PASS**; uji runtime `NOT RUN`; migration SQL idempoten `Migrations/20261111020000_AddHrdNotificationAndSuspensionAccess.cs` disiapkan, belum dijalankan (wewenang pemilik).


Pembaruan 11 November 2026: seluruh migration, termasuk `20261110050000` dan `20261111020000`, sudah dijalankan ke database pengembangan `QuilvianNewDevHamzah`. Status bukti tetap 🟡 karena uji penerimaan masih `NOT RUN`. Rincian ada di [backend-unfinished-work.md](backend-unfinished-work.md) bagian 14.

### Bagian 1v: Jejak Gelombang 1 Perbaikan Frontend (11 November 2026)

Dasar: [frontend-unfinished-work.md](frontend-unfinished-work.md) Gelombang 1 (`FE-HRD-R01` s.d. `FE-HRD-R13`). Pemilik meminta seluruh Gelombang 1 dikerjakan tuntas. Tidak ada perubahan source backend; seluruh alamat service dicocokkan dengan `[Route]` controller backend commit `f751f8ca`.

| Task | Isi | Task asal yang terdampak | Status | Laporan |
| --- | --- | --- | :---: | --- |
| `FE-HRD-R01` | Status dan register `frontend-roadmap.md` disamakan dengan audit: 137 task diturunkan ke 🟡, register `FE-HRD-058` s.d. `087` diisi, `FE-HRD-216` s.d. `305` ditambahkan | Kelompok B, C, D audit | ✅ | [R01](../task/report/frontend/FE-HRD-R01.md) |
| `FE-HRD-R02` | Service dan slice K3RS tanpa data contoh, memakai `InstanceAxios` | `FE-HRD-261` s.d. `271` | ✅ | [R02](../task/report/frontend/FE-HRD-R02.md) |
| `FE-HRD-R03` | Service dan slice Hubungan Industrial; 13 nama method diselaraskan | `FE-HRD-246`, `248` s.d. `260` | ✅ | [R03](../task/report/frontend/FE-HRD-R03.md) |
| `FE-HRD-R04` | Service dan slice Kinerja; angka contoh dasbor dihapus | `FE-HRD-231` s.d. `244` | ✅ | [R04](../task/report/frontend/FE-HRD-R04.md) |
| `FE-HRD-R05` | Service dan slice Pelatihan; angka contoh dasbor dan rencana tahunan dihapus | `FE-HRD-216` s.d. `229` | ✅ | [R05](../task/report/frontend/FE-HRD-R05.md) |
| `FE-HRD-R06` | 58 alamat Kredensial dan Layanan SDM diberi `/v1` | `FE-HRD-172`, `186` | ✅ | [R06](../task/report/frontend/FE-HRD-R06.md) |
| `FE-HRD-R07` | Service OPPE dipetakan ke grup `oppe`, `fppe`, `peer-review-audit` | `FE-HRD-201` | ✅ | [R07](../task/report/frontend/FE-HRD-R07.md) |
| `FE-HRD-R08` | Service Persetujuan Terpadu memakai `approvals/*` dan `instances/{id}` | `FE-HRD-276` | ✅ | [R08](../task/report/frontend/FE-HRD-R08.md) |
| `FE-HRD-R09` | Service Perjalanan Dinas; 5 method ditandai belum tersedia | `FE-HRD-291` | ✅ | [R09](../task/report/frontend/FE-HRD-R09.md) |
| `FE-HRD-R10` | Alamat Onboarding, layanan mandiri, dan administrasi perubahan data | `FE-HRD-026`, `058` | 🟡 — tombol belum diuji di browser karena menulis data | [R10](../task/report/frontend/FE-HRD-R10.md) |
| `FE-HRD-R11` | Alamat Lembur dan klaim Benefit | `FE-HRD-116`, `158` | ✅ | [R11](../task/report/frontend/FE-HRD-R11.md) |
| `FE-HRD-R12` | Layar `S-D1` MVP-7: Pengaturan, Formasi Jabatan, Batch Formasi Awal | `FE-HRD-029` ✅, `030` 🟡, `031` 🟡 | 🟡 | [R12](../task/report/frontend/FE-HRD-R12.md) |
| `FE-HRD-R13` | Layar `S-B1` MVP-21: Monitoring Presensi, log mesin, Pengaturan Presensi | `FE-HRD-088` ✅, `089` 🟡, `090` 🟡, `091` ✅, `099` ✅ | 🟡 | [R13](../task/report/frontend/FE-HRD-R13.md) |

Verifikasi bersama seluruh baris: `npx eslint` atas berkas yang diubah **0 error**; `npm run build` **berhasil** (keluar 0, dijalankan sesudah perubahan source terakhir); `npm run test:unit` 4.671 test, 4.618 lulus, 53 gagal, seluruh kegagalan pada berkas yang tidak disentuh gelombang ini; `check-css-usage.mjs` atas 7 view baru **0 temuan**; uji browser Chrome headed atas 15 rute layar (26 screenshot di `QuilvianSystemFrontendDev/test-with-agy/screenshots/hrw1-1111/`) dengan backend lokal mode `Runtime__Role=Web`, tanpa satu pun pesan galat Axios mentah. Satu perubahan data sampingan: toleransi keterlambatan di database pengembangan sempat tersimpan 30 menit saat uji validasi, lalu dikembalikan ke 10 menit dan dibuktikan sesudah halaman dimuat ulang. Perubahan frontend belum di-commit.


---

### Bagian 1w: Matriks Keterlacakan Amandemen `A52` — Penjadwalan & Kehadiran untuk 12 menu `PRD-HRD-PK-001` (roadmap revisi `24`, kontrak `v26`)

| Field | Nilai |
| --- | --- |
| Roadmap revisi | `24` |
| Approval | Pengguna (`HRD-DEC-015`), 12 November 2026 — desain `A52` dan kontrak `v26` |
| Masukan | Decision log revision `37` (SHA-256 `df3cf67a…`); capability map revisi `1.4` (SHA-256 `d260d20f…`); `02-backend-architecture.md` 35; `03-frontend-architecture.md` 31; `04-prd-to-mvp.md` 40; `testing/acceptance-test-matrix.md` 34 |
| Source SHA | BE `a437ae42` (`yoga`); FE `6a5658269` (`YogaV2`) |
| Status semua baris | `PLANNED` — belum ada bukti implementasi |

| Requirement | Keputusan | Desain | Kontrak `v26` | Task backend | Task frontend | Bukti uji | Gelombang | Status |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `FR-PK-001` Migration amandemen | `HRD-DEC-195`, `198`, `200`–`202`, `205`, `211`, `212`, `217` | 35.6–35.7 | Kamus data 27 | `BE-HRD-334` ✅ | — | Build `PASS`, `has-pending-model-changes` `PASS`, skrip SQL ditinjau, uji kering devYoga `PASS`, diterapkan ke devYoga 2026-11-13 ([laporan](../task/report/backend/BE-HRD-334.md)); DEFAULT kolom lama diselaraskan lewat `BE-HRD-FIX-001` | `MVP-66` | `DONE` — devYoga; server dev bersama belum |
| `FR-PK-002` Cakupan unit | `HRD-DEC-194`, `206`, `208` | 35.9.1 | `VR-PK-06`, `VR-PK-27`; penanda `ManageAllUnits`, `ReadAllUnits` | `BE-HRD-335` ✅, `BE-HRD-336` 🟡; `ReadAllUnits` dideklarasikan di `BE-HRD-349` | `FE-HRD-306` | `AT-HRD-PK-09`, `-10`; `UAT-PK-01`, `-02` | `MVP-66` | `IN PROGRESS` |
| `FR-PK-003` Ganti Petugas pemeriksaan penuh | `HRD-DEC-195`, `099`, `100` | 35.9.2–35.9.3 | `replace` (Diubah); `VR-PK-03`–`05` | `BE-HRD-337` ✅, `BE-HRD-338` 🟡 | `FE-HRD-307` | `AT-HRD-PK-02` (harness `PASS`; API `BLOCKED` data Didampingi), `-03` (unit service `PASS`), `-04` (API devYoga `PASS`); `UAT-PK-04` | `MVP-67` | `IN PROGRESS` |
| `FR-PK-004` Riwayat per perubahan | `HRD-DEC-195`, `217` | 35.4.1, 35.9.3 | `revisions`; state 30.1 | `BE-HRD-338` 🟡, `BE-HRD-339` ✅, `BE-HRD-340` ✅ | `FE-HRD-308` | `AT-HRD-PK-01` (API devYoga `PASS`), `-07` (harness + API devYoga `PASS`, [laporan](../task/report/backend/BE-HRD-339.md)), `-08` (harness + API devYoga `PASS`, [laporan](../task/report/backend/BE-HRD-340.md)); `UAT-PK-03` | `MVP-67` | `IN PROGRESS` — backend selesai, layar `FE-HRD-308` belum |
| `FR-PK-005` Roster tetap terbit saat ditambah | `HRD-DEC-195` | 35.9.3 | `assignments` (Diubah) | `BE-HRD-340` ✅ | `FE-HRD-307` | `AT-HRD-PK-05` (harness + API devYoga `PASS`) | `MVP-67` | `IN PROGRESS` — backend selesai, layar `FE-HRD-307` belum |
| `FR-PK-006` Hapus shift terbit | `HRD-DEC-195` | 35.9.3 | `shifts/{id}/remove`; `RosterPeriod : RemoveShift`; `VR-PK-07` | `BE-HRD-340` ✅ | `FE-HRD-307` | `AT-HRD-PK-06` (harness + API devYoga `PASS`); `UAT-PK-06` | `MVP-67` | `IN PROGRESS` — backend selesai, layar `FE-HRD-307` belum |
| `FR-PK-007` `B3`, `B8`, `W6`, `W7` | `HRD-DEC-100`, `201`, `213` | 35.9.2 | `VR-PK-01`, `-02`, `-04`; `INT-HR-106`, `107` | `BE-HRD-337` ✅, `BE-HRD-341` ✅ | `FE-HRD-307` | `AT-HRD-PK-11`–`14` (API devYoga `PASS`), `-45` (harness + API devYoga `PASS`, [laporan](../task/report/backend/BE-HRD-341.md)); `UAT-PK-05` | `MVP-67` | `IN PROGRESS` — backend selesai, layar `FE-HRD-307` belum |
| `FR-PK-008` Belum Hadir / Tidak Ada Scan | `HRD-DEC-207` | 35.9.4 | State 30.6 | `BE-HRD-343` | `FE-HRD-310` | `AT-HRD-PK-19`–`21`; `UAT-PK-07` | `MVP-67` | `PLANNED` |
| `FR-PK-009` Hari libur hanya jadwal tetap | `HRD-DEC-210` | 35.9.4 | State 30.6 | `BE-HRD-343` | `FE-HRD-310` | `AT-HRD-PK-28`; `UAT-PK-11` | `MVP-67` | `PLANNED` |
| `FR-PK-010` Toleransi dan jendela dari setelan | `HRD-DEC-196`, `211`, `215`, `HRD-ASM-83` | 35.4.1, 35.10 | Settings (Diubah); `VR-PK-23`, `-30` | `BE-HRD-342` ✅ | `FE-HRD-309` | `AT-HRD-PK-24`, `-25`, `-27` (unit service `PASS`, [laporan](../task/report/backend/BE-HRD-342.md)); `UAT-PK-08` | `MVP-67` | `IN PROGRESS` — backend selesai, layar `FE-HRD-309` belum |
| `FR-PK-011` Scan di luar jendela → pengecualian | `HRD-DEC-211` | 35.9.4 | — | `BE-HRD-343` | `FE-HRD-310` | `AT-HRD-PK-26` | `MVP-67` | `PLANNED` |
| `FR-PK-012` Pratinjau dan konversi | `HRD-DEC-197`, `216`, `HRD-ASM-81` | 35.4.2 | `close-preview`, `close` (Diubah) | `BE-HRD-344` | `FE-HRD-311` | `AT-HRD-PK-22`, `-23`, `-31`; `UAT-PK-09` | `MVP-67` | `PLANNED` |
| `FR-PK-013` Scan mesin tak terdaftar ditahan | `HRD-DEC-212` | 35.9.6 | Raw Logs (Diubah); state 30.7 | `BE-HRD-345` | `FE-HRD-312` | `AT-HRD-PK-29`; `UAT-PK-10` | `MVP-67` | `PLANNED` |
| `FR-PK-014` Pendaftaran mesin melepas scan | `HRD-DEC-212` | 35.9.6 | `attendance-devices`; `VR-PK-26`; `INT-HR-103` | `BE-HRD-345`, `BE-HRD-346` | `FE-HRD-312` | `AT-HRD-PK-30`; `UAT-PK-10` | `MVP-67` | `PLANNED` |
| `FR-PK-015` Master hari libur | `HRD-DEC-210` | 35.4.3 | `holidays`; `VR-PK-25` | `BE-HRD-347` | `FE-HRD-313` | `UAT-PK-11` | `MVP-67` | `PLANNED` |
| `FR-PK-016` Jadwal Saya | `HRD-DEC-209`, `HRD-ASM-80` | 35.4.2 | `my-schedule`; `VR-PK-29` | `BE-HRD-348` | `FE-HRD-314` | `AT-HRD-PK-42`–`44`; `UAT-PK-12`, `-13` | `MVP-68` | `PLANNED` |
| `FR-PK-017` Tujuh laporan | `HRD-DEC-206`, `HRD-ASM-79` | 35.4.2 | `reports/{reportCode}`; `VR-PK-27`, `-28` | `BE-HRD-349` | `FE-HRD-315` | `AT-HRD-PK-39`; `UAT-PK-14` | `MVP-68` | `PLANNED` |
| `FR-PK-018` Ekspor tercatat | `HRD-DEC-206` | 35.11 | `reports/{reportCode}/export`; `AttendanceReport : Export` | `BE-HRD-350` | `FE-HRD-315` | `AT-HRD-PK-37`, `-38`; `UAT-PK-14`, `-15` | `MVP-68` | `PLANNED` |
| `FR-PK-019`–`022` Siaga | `HRD-DEC-198`, `204`, `205`, `HRD-ASM-75`, `78` | 35.9.5 | `backups`, `calls`; state 30.2; `VR-PK-08`–`12`; `INT-HR-39`, `40` | `BE-HRD-352` | `FE-HRD-317` | `AT-HRD-PK-15`–`18` | `POST-MVP` | `PLANNED` |
| `FR-PK-023`–`025` Dispensasi | `HRD-DEC-200`, `HRD-ASM-76` | 35.4.1 | `dispensations`; state 30.3; `VR-PK-13`–`17` | `BE-HRD-353`, `BE-HRD-354` | `FE-HRD-318` | `AT-HRD-PK-32`–`34` | `POST-MVP` | `PLANNED` |
| `FR-PK-026`–`027` Tugas belajar | `HRD-DEC-201` | 35.4.1 | `study-assignments`; state 30.4; `VR-PK-18`, `-19` | `BE-HRD-355` | `FE-HRD-319` | `AT-HRD-PK-12` | `POST-MVP` | `PLANNED` |
| `FR-PK-028`–`029` Gangguan mesin | `HRD-DEC-202`, `HRD-ASM-77` | 35.4.1 | `device-incidents`; state 30.5; `VR-PK-20`–`22` | `BE-HRD-356` | `FE-HRD-320` | `AT-HRD-PK-35`, `-36` | `POST-MVP` | `PLANNED` |
| `FR-PK-030`–`031` Dashboard | `HRD-DEC-208` | 35.4.2 | `dashboard` | `BE-HRD-357` | `FE-HRD-321` | `AT-HRD-PK-40`, `-41` | `POST-MVP` | `PLANNED` |
| Realisasi Jadwal (tampilan) | `HRD-DEC-195` butir 5 | `03-frontend-architecture.md` 31.1 | `revisions` | `BE-HRD-340` | `FE-HRD-322` | Uji browser `FE-HRD-322` | `POST-MVP` | `PLANNED` |
| `FR-PK-032` Grup menu 12 menu | `HRD-DEC-214` | `03-frontend-architecture.md` 31.1 | — | `BE-HRD-359` ⛔ (bila `DSG-Q-02` memilih endpoint) | `FE-HRD-324` ⛔ | `AT-HRD-PK-46` | — | `BLOCKED` (`DSG-Q-01`, `DSG-Q-02`) |
| Definition of Done MVP | — | `04-prd-to-mvp.md` 40.19 | `v26` | `BE-HRD-351` | `FE-HRD-316` | Seluruh uji MVP di atas | `Verifikasi` | `PLANNED` |
| Definition of Done POST-MVP | — | — | `v26` | `BE-HRD-358` | `FE-HRD-323` | Uji `POST-MVP` di atas | `POST-MVP` | `PLANNED` |

**Coverage gap:**

| Gap | Sebab | Penanganan |
| --- | --- | --- |
| `AT-HRD-PK-46` (sidebar 12 menu) tidak punya task yang dapat dijalankan | `FE-HRD-324` terblokir `DSG-Q-01`/`DSG-Q-02` | Dibuka kembali begitu pengguna menjawab |
| `FR-PK-026`–`027` hanya diuji lewat `AT-HRD-PK-12` (blokir roster); status harian "Tugas Belajar" tidak punya ID uji tersendiri | Matriks uji bagian 34 tidak memuat kasus status harian tugas belajar | `BE-HRD-355` menambah uji status harian di laporan task; matriks uji disegarkan pada revisi berikut |
| Keputusan `HRD-DEC-203` ("Realisasi Jenis Tambahan" = realisasi jam lembur) tidak punya task | Tidak ada perubahan backend; hanya label menu | Tercakup `FE-HRD-324` |
| Pertanyaan `DSG-Q-03` dan `DSG-Q-04` | Tidak memblokir | Dicatat di `04-prd-to-mvp.md` 40.20 |
