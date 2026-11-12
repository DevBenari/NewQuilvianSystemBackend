# Payroll Management — Peta Modul

| Field | Nilai |
|---|---|
| Blueprint ID | `PRL-BP-001` |
| Revision | `1` |
| Status | `draft` |
| Dasar | `00-interview-decisions.md` revision 1 (PRL-DEC-21), `01-existing-capability-map.md` revision 1 |
| Backend / Frontend SHA | `89b9fd69` / `bfa3bae69` |

Berkas ini memegang empat hal yang tidak boleh punya salinan kedua di dalam modul: daftar
sub-modul, kepemilikan data, peta butir menu beserta urutan migration, dan pemetaan setiap
kemampuan ke sub-modul pemiliknya.

---

## 1. Registry sub-modul

| Slug | Rumpun kemampuan | Prefix task | Pemilik | Status | Alasan |
|---|---|---|---|---|---|
| `payroll-gaji` | Penggajian bulanan: master gaji dan tunjangan, run, input variabel, pembayaran, slip, laporan | `PAY` (melanjutkan `PAY-BE-*`, `PAY-FE-*`) | Yasmin | `approved` (Yasmin, 2026-11-12) | Slice siap didesain sudah dirancang; slice terblokir tercatat sebagai epic `OPEN DECISION` |
| `remunerasi-jasa` | Skema remunerasi, pool dan distribusi jasa tidak langsung, insentif kinerja, persetujuan remunerasi | `REM` | Yasmin | `BLOCKED` | DEC-PRL-011 (amandemen Medical Fee, kontrak unit/cost center), DEC-PRL-012 (rumus alokasi, indeks, bobot), DEC-PRL-013 (pejabat penyetuju), DEC-PRL-014 (registry master baru) |

Status modul diturunkan: `payroll-gaji` `approved` dan `remunerasi-jasa` `BLOCKED`, sehingga status modul `partial`.

## 2. Tabel kepemilikan data seluruh modul

| Kelompok data | Pemilik | Dipakai sub-modul | Dibuat ulang |
|---|---|---|---|
| Profil pegawai, unit organisasi, Cost Center (`MstWorkforceProfile`, `MstCostCenter`) | Human Resource (modul lain) | Keduanya | Tidak |
| Penetapan gaji pokok (`WfpSalaryAssignment`) | HRD (modul lain) | `payroll-gaji` | Tidak — dibaca, hanya yang disetujui |
| Penilaian kinerja (`WfpPerformanceReview`), katalog KPI (`MstKpiCatalog`) | Human Resource (modul lain) | `remunerasi-jasa` | Tidak |
| Kehadiran dan lembur terverifikasi, tarif lembur (`MstOvertimeRate`) | Human Resource (modul lain) | `payroll-gaji` | Tidak — snapshot impor ke run, tarif dibaca-saja |
| Jadwal, penugasan siaga | Scheduling (modul lain) | `payroll-gaji` | Tidak — menunggu kontrak DEC-PRL-005 |
| Jasa kotor per penerima (`MdfServiceFee`) | Medical Fee (modul lain) | Keduanya | Tidak |
| Pembayaran, piutang pegawai, pembukuan | Finance dan Accounting (modul lain) | `payroll-gaji` | Tidak — lewat handoff |
| Master gaji: Grade, Struktur, Komponen, Kategori, Jenis Tunjangan, Jenis Potongan, Periode, kebijakan Shift/Siaga/Risiko (`Mst*`, berkas di `Areas/Corporate/HumanResource/MasterData/PayrollAndBenefit`) | `payroll-gaji` | Keduanya | Tidak |
| Tunjangan per struktur gaji (`MstSalaryStructureAllowance`) | `payroll-gaji` | `payroll-gaji` | **Baru** |
| Tunjangan Transportasi: kebijakan, per pegawai, transaksi (`PrlTransportAllowance*`) | `payroll-gaji` | `payroll-gaji` | Tidak |
| Run, pegawai dalam run, komponen hasil hitung, input variabel, persetujuan, pembayaran, slip (`PrlPayroll*`) | `payroll-gaji` | Keduanya (`remunerasi-jasa` membaca) | Tidak |
| Kebijakan THR | `payroll-gaji` | `payroll-gaji` | **Belum diputuskan** — prefix dan registry (DEC-PRL-014) |
| Kebijakan tunjangan jabatan, profesi, jenjang | `payroll-gaji` | `payroll-gaji` | **Belum diputuskan** — DEC-PRL-003, 014 |
| Sumber Dana | **Belum diputuskan** — `payroll-gaji` bersama Finance (DEC-PRL-008) | `payroll-gaji` | `BLOCKED` |
| Skema remunerasi, versi kebijakan, pool, distribusi | `remunerasi-jasa` | `remunerasi-jasa` | `BLOCKED` — DEC-PRL-011, 012, 014 |

Aturan: `remunerasi-jasa` **tidak boleh** membuat tabel run, komponen, atau slip sendiri. Bila
nanti distribusi dibayar lewat Payroll, ia menulis lewat input variabel milik `payroll-gaji`
(kontrak lintas sub-modul yang dideklarasikan, sama seperti lintas modul).

## 3. Peta butir menu dan urutan migration

### 3.1 Peta butir menu

Berkas yang disunting saat implementasi: frontend `src/utils/menu-sidebar/menu-items.jsx`.
Sidebar Payroll saat ini disusun per kelompok kerja, bukan mengikuti 13 menu pemilik. Penataan
ulang ke 13 menu adalah pertanyaan terbuka DEC-PRL-017. Pada MVP, butir baru ditambahkan ke
kelompok yang sudah ada. Nama butir dan urutannya `DEV_DISCRETION`.

```text
Payroll                                        <- tingkat 0 (sudah ada)
├── Data Induk                                 (sudah ada)
│   ├── Grade dan Struktur Gaji               (sudah ada)
│   │   ├── Grade Gaji                         -> /payroll/master-data/salary-grade
│   │   └── Struktur Gaji                      -> /payroll/master-data/salary-structure   [FE-PGJ-01 diperluas]
│   ├── Komponen Penggajian                    (sudah ada, 4 butir)
│   ├── Periode Penggajian                     -> /payroll/master-data/payroll-period
│   └── Kebijakan Tunjangan                    (sudah ada: Shift, Siaga, Risiko)
│       └── Tunjangan Transportasi             -> /payroll/master-data/transport-allowance-policy   [FE-PGJ-09 BARU]
├── Penghasilan Pegawai
│   ├── Profil Penggajian                      -> /payroll/workforce-compensations
│   │   └── (anak) Tunjangan Transportasi      -> /payroll/workforce-compensations/[id]/transport-allowance   [FE-PGJ-10 BARU]
│   └── Penetapan Gaji                         -> /payroll/salary-assignments
├── Proses Penggajian
│   ├── Persiapan Penggajian                   -> /payroll/payroll-runs
│   │   └── (anak) Detail Run                  -> /payroll/payroll-runs/[id]
│   │       ├── (anak) Penghasilan Variabel    -> /payroll/payroll-runs/[id]/variable-inputs   [FE-PGJ-02 BARU]
│   │       └── (anak) Rekap Lembur            -> /payroll/payroll-runs/[id]/overtime          [FE-PGJ-05 BARU]
│   ├── Simulasi Penggajian                    -> /payroll/simulations                         [FE-PGJ-04 BARU]
│   ├── Perhitungan Gaji                       -> /payroll/payroll-calculations
│   └── Persetujuan Penggajian                 -> /payroll/payroll-approvals
├── Lembur                                     <- grup BARU
│   └── Tarif Lembur                           -> /payroll/overtime-rates                      [FE-PGJ-06 BARU]
├── Potongan dan Penyesuaian                   (sudah ada)
├── Remunerasi Dokter                          (sudah ada: Integrasi Jasa Medis)
├── Pembayaran Penggajian                      (sudah ada)
├── Slip Gaji                                  (sudah ada)
└── Laporan
    ├── Rekap Penggajian                       (sudah ada)
    ├── Rekonsiliasi Keuangan                  (sudah ada)
    ├── Beban Gaji per Cost Center             -> /payroll/reports/expense-summary             [FE-PGJ-07 BARU]
    └── PPh dan BPJS                           -> /payroll/reports/tax-contribution            [FE-PGJ-08 BARU]
```

Butir milik `remunerasi-jasa` belum dirancang karena sub-modul itu `BLOCKED`.

| Butir menu | Tingkat | Induk | `pathname` | Layar | Butir hak akses | Status |
|---|:---:|---|---|---|---|---|
| Tunjangan Transportasi | 3 | Kebijakan Tunjangan | `/payroll/master-data/transport-allowance-policy` | `FE-PGJ-09` | `TransportAllowancePolicy : Read` | Baru |
| Simulasi Penggajian | 2 | Proses Penggajian | `/payroll/simulations` | `FE-PGJ-04` | `PayrollRun : Simulate` | Baru |
| Lembur | 1 | Payroll | — | — | — | Baru (grup) |
| Tarif Lembur | 2 | Lembur | `/payroll/overtime-rates` | `FE-PGJ-06` | `OvertimeRate : Read` | Baru |
| Beban Gaji per Cost Center | 2 | Laporan | `/payroll/reports/expense-summary` | `FE-PGJ-07` | `PayrollReport : ViewAmount` | Baru |
| PPh dan BPJS | 2 | Laporan | `/payroll/reports/tax-contribution` | `FE-PGJ-08` | `PayrollReport : ViewAmount` | Baru |

Layar anak tanpa butir menu, beserta jalan masuknya:

| Layar | Layar induk | Jalan masuk |
|---|---|---|
| `FE-PGJ-01` bagian Tunjangan | Detail Struktur Gaji | Bagian di dalam detail struktur |
| `FE-PGJ-02` Penghasilan Variabel | Detail Run `/payroll/payroll-runs/[id]` | Tombol "Penghasilan Variabel" |
| `FE-PGJ-05` Rekap Lembur | Detail Run | Tombol "Rekap Lembur" |
| `FE-PGJ-10` Tunjangan Transportasi pegawai | Detail Profil Penggajian `/payroll/workforce-compensations/[id]` | Tombol "Tunjangan Transportasi" |

### 3.2 Urutan migration lintas sub-modul

| Urutan | Migration | Sub-modul | Prasyarat | Tanpa downtime |
|---:|---|---|---|---|
| 0 | `20261104071700_AddPayrollIntegrityConstraints` dan `20261111100000_RenamePayrollTrxAndWfpTablesToPrlPrefix` | `payroll-gaji` (sudah ada, belum dijalankan di DB uji) | Skrip pra-cek pada `PAYROLL_V2_05` dan `task/report/backend/PAY-MIG-02.md` | Lihat dokumen asalnya |
| 1 | `AddSalaryStructureAllowance` | `payroll-gaji` | Urutan 0 sudah jalan | Ya — tabel baru |
| 2 | `AddPayrollVariableInputDecisionColumns` | `payroll-gaji` | Urutan 0 sudah jalan | Ya — kolom baru boleh kosong |
| — | Migration `remunerasi-jasa` | `remunerasi-jasa` | `BLOCKED` | — |

Urutan 1 dan 2 saling bebas dan boleh digabung dalam satu migration.

## 4. Pemetaan kemampuan ke sub-modul

Status: `MVP` = dirancang dan masuk gelombang pengiriman; `SUDAH ADA` = dipakai apa adanya;
`DEFERRED` = punya pemilik, ditunda dengan alasan; `BLOCKED` = menunggu keputusan.

| Menu / kemampuan | ID kemampuan | Sub-modul pemilik | Status | Keputusan |
|---|---|---|---|---|
| 1.1–1.4 Dashboard | P-01 | `payroll-gaji` | `BLOCKED` | DEC-PRL-009 |
| 2.1 Jenis Penghasilan | P-02 | `payroll-gaji` | `SUDAH ADA` | — |
| 2.2 Jenis Potongan | P-03 | `payroll-gaji` | `SUDAH ADA` | — |
| 2.3 Sumber Dana | P-05 | `payroll-gaji` | `BLOCKED` | DEC-PRL-008 |
| 2.4 Aturan Komponen | P-02 | `payroll-gaji` | `BLOCKED` | DEC-PRL-010 |
| 3.1 Grade Gaji | P-06 | `payroll-gaji` | `MVP` (batas struktur) | PRL-DEC-12 |
| 3.2 Gaji Pokok | P-08 | `payroll-gaji` | `SUDAH ADA` (baca-saja); pengecualian `BLOCKED` | PRL-DEC-36; DEC-PRL-002 |
| 3.3 Tunjangan Jabatan, 3.4 Tunjangan Profesi | P-10 | `payroll-gaji` | `BLOCKED` | DEC-PRL-003, 004, 014 |
| 3.5 Tunjangan Tetap | P-07, P-09 | `payroll-gaji` | `MVP` | PRL-DEC-02, 04 |
| Penyimpangan tunjangan per pegawai | P-09 | `payroll-gaji` | `BLOCKED` | DEC-PRL-002, 013 |
| Tunjangan Shift, Siaga, Risiko | P-11 | `payroll-gaji` | `BLOCKED` | DEC-PRL-005 |
| Tunjangan Transportasi bulanan tetap | P-12 | `payroll-gaji` | `MVP` | PRL-DEC-46 |
| Tunjangan Transportasi per kehadiran, per hari, penggantian | P-12 | `payroll-gaji` | `DEFERRED` | Butuh data kehadiran harian (DEC-PRL-005) |
| 4.1 Periode, 4.2 Data Penggajian, 4.4 Perhitungan | P-13 | `payroll-gaji` | `SUDAH ADA` | — |
| 4.3 Simulasi | P-15 | `payroll-gaji` | `MVP` | PRL-DEC-32 |
| 4.5 Koreksi, 11.2 Koreksi | P-16 | `payroll-gaji` | `SUDAH ADA` (reversal, restitusi) | — |
| 5.1 Rekap Lembur Disetujui | P-18 | `payroll-gaji` | `MVP` | PRL-DEC-42 |
| 5.2 Tarif Lembur | P-18 | `payroll-gaji` | `MVP` (baca-saja dari HR) | PRL-DEC-42 |
| 5.3 Honorium Tugas Tambahan | P-17 | `payroll-gaji` | `MVP` | PRL-DEC-31 |
| Insentif bulanan, bonus dalam run bulanan | P-17 | `payroll-gaji` | `MVP` | PRL-DEC-07, 38, 43 |
| 6.1–6.3 PPH21, BPJS — laporan | P-20 | `payroll-gaji` | `MVP` | PRL-DEC-33 |
| 6.1–6.3 PPH21, BPJS — pengaturan | P-20 | `payroll-gaji` | `BLOCKED` | DEC-PRL-016 |
| 6.4 Potongan Lainnya | P-19 | `payroll-gaji` | `SUDAH ADA` | — |
| THR dan bonus tahunan (run sendiri) | P-14 | `payroll-gaji` | `BLOCKED` | DEC-PRL-006, 007 |
| Pensiun mesin Modern | X-02 | `payroll-gaji` | `BLOCKED` | DEC-PRL-001 |
| 7.1–7.5 Skema Remunerasi | R-01 | `remunerasi-jasa` | `BLOCKED` | DEC-PRL-012, 014 |
| 8.1–8.3 Jasa Pelayanan Medis | R-04 | `remunerasi-jasa` (tampilan); hitungnya Medical Fee | `BLOCKED` | DEC-PRL-011 |
| 8.4 Verifikasi Kontribusi Pelayanan | R-05 | `remunerasi-jasa` | `BLOCKED` | DEC-PRL-011 |
| 9.1–9.4 Distribusi Jasa Pelayanan | R-06 | `remunerasi-jasa` | `BLOCKED` | DEC-PRL-011, 012 |
| 10.1 Indikator Kinerja | R-03 | `remunerasi-jasa` | `BLOCKED` (master HR siap dipakai) | DEC-PRL-012 |
| 10.2–10.4 Insentif Jabatan, Bonus Prestasi, Penyesuaian | R-07 | `remunerasi-jasa` | `BLOCKED` | DEC-PRL-012, 013 |
| 11.1 Validasi Hasil, 11.3 Persetujuan Payroll | P-13, P-21 | `payroll-gaji` | `SUDAH ADA` | — |
| 11.4 Persetujuan Remunerasi | R-08 | `remunerasi-jasa` | `BLOCKED` | DEC-PRL-013 |
| 12.1–12.3 Pembayaran, Instruksi Transfer, Status | P-22 | `payroll-gaji` | `SUDAH ADA` | PRL-DEC-39 |
| 12.4 Slip Gaji | P-23 | `payroll-gaji` | `SUDAH ADA` | — |
| 12.5 Rincian Remunerasi | P-24 | `remunerasi-jasa` | `BLOCKED` | DEC-PRL-011, 012 |
| 13.1 Laporan Payroll | P-25 | `payroll-gaji` | `SUDAH ADA` | — |
| 13.2 Distribusi Jasa | R-06 | `remunerasi-jasa` | `BLOCKED` | DEC-PRL-011 |
| 13.3 Biaya Per-Unit — bagian gaji per Cost Center | P-25 | `payroll-gaji` | `MVP` (laporan beban gaji) | PRL-DEC-27, 41 |
| 13.3 Biaya Per-Unit — bagian remunerasi dan Sumber Dana | P-25 | `remunerasi-jasa` | `BLOCKED` | DEC-PRL-008, 011 |
| 13.4 Jurnal | P-26 | `payroll-gaji` | Diganti laporan beban gaji (`MVP`) | PRL-DEC-27 |
| 13.5 History Pembayaran | P-27 | `payroll-gaji` | `SUDAH ADA` | — |

Tidak ada kemampuan tanpa sub-modul pemilik.

Catatan pembagian 13.3: PRL-DEC-21 menempatkan Biaya Per-Unit di `remunerasi-jasa`. Bagian
yang sudah dapat dijawab dari data gaji (beban gaji per Cost Center) dikirim lebih dulu oleh
`payroll-gaji` sebagai laporan beban gaji (PRL-DEC-27). Bagian remunerasi dan Sumber Dana tetap
milik `remunerasi-jasa`.

## 5. Artefak lama yang belum dipindahkan

Pada bentuk `COMPOSITE`, artefak berikut semestinya berpindah ke sub-modul `payroll-gaji`.
Pass desain ini **sengaja tidak memindahkannya**:

| Artefak | Alasan belum dipindah |
|---|---|
| `PAYROLL_V2_01` s.d. `PAYROLL_V2_06` | Dirujuk laporan task, memori kerja, dan dokumen lain dengan path absolut; pemindahan perlu satu perubahan tersendiri yang memperbaiki semua rujukan |
| `task/report/backend/`, `task/report/frontend/` | Laporan task yang sudah di-commit; pemindahan mengubah path yang dirujuk roadmap `PAYROLL_V2_04` |
| `evidence/` | Surat lintas pemilik untuk seluruh modul; tetap di tingkat modul |

Usulan: pindahkan pada task dokumentasi tersendiri setelah blueprint ini disetujui, sekaligus
memperbaiki rujukannya. Ini keputusan pemilik dan tercatat di manifest modul.
