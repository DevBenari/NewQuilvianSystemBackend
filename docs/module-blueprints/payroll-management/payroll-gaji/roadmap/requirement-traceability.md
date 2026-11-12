# Requirement Traceability — Sub-modul Payroll Gaji

| Field | Nilai |
|---|---|
| Blueprint | `PRL-BP-001` / `payroll-gaji`, revision `1` |
| Roadmap revision | `1` (`draft`) |
| Contract version | `PGJ-C1` (`approved`, Yasmin, 2026-11-12) |
| Baseline SHA | Backend `89b9fd69`, frontend `bfa3bae69` |
| Masukan | `../04-prd-to-mvp.md`, `backend-roadmap.md`, `frontend-roadmap.md` |

Bukti verifikasi backend memakai `rules/backend/TEST_POLICY.md`: QBE, build, uji kontrak API,
dan uji proses. Ketiadaan test otomatis backend **bukan** gap.

| Requirement | Decision | Desain | Task backend | Task frontend | Bukti verifikasi | Status |
|---|---|---|---|---|---|---|
| FR-PGJ-001 Kelola tunjangan struktur | PRL-DEC-02, 13 | Arsitektur 4.1; kontrak Salary Structure Allowance | `PAY-BE-14`, `PAY-BE-16` | `PAY-FE-16` | Uji API; `UAT-01` | Belum dikerjakan |
| FR-PGJ-002 Tolak jenis ganda | PRL-DEC-02 | Validasi `SA-04` | `PAY-BE-16` | `PAY-FE-16` | `UAT-02` | Belum dikerjakan |
| FR-PGJ-003 Tolak jenis berbasis aktivitas | PRL-DEC-13 | Validasi `SA-06` | `PAY-BE-16` | `PAY-FE-16` | `UAT-02b` | Belum dikerjakan |
| FR-PGJ-004 Hitung tunjangan per jenis | PRL-DEC-02 | Arsitektur 5.1 | `PAY-BE-17` | — (tampil di detail run dan slip yang ada) | `UAT-01` | Belum dikerjakan |
| FR-PGJ-005 Cadangan angka profil | PRL-DEC-04 | Arsitektur 5.2; `CALC-03` | `PAY-BE-14`, `PAY-BE-17` | — | `UAT-03` | Belum dikerjakan |
| FR-PGJ-006 Profil diabaikan bila struktur berisi | PRL-DEC-04 | Arsitektur 5.2; `CALC-04` | `PAY-BE-17` | — | `UAT-04` | Belum dikerjakan |
| FR-PGJ-010 Batas struktur di dalam grade | PRL-DEC-12 | `SR-01`–`SR-03` | `PAY-BE-15` | `PAY-FE-16` | `UAT-05`, `UAT-06` | Belum dikerjakan |
| FR-PGJ-011 Tolak grade dipersempit | PRL-DEC-12 | `SR-04` | `PAY-BE-15` | `PAY-FE-16` | `UAT-07` | Belum dikerjakan |
| FR-PGJ-020 Layar kebijakan transport | PRL-DEC-46 | Frontend 3.6 | — (endpoint sudah ada) | `PAY-FE-14` | Uji manual | Belum dikerjakan |
| FR-PGJ-021 Layar transport pegawai | PRL-DEC-46 | Frontend 3.6 | — (endpoint sudah ada) | `PAY-FE-15` | Uji manual | Belum dikerjakan |
| FR-PGJ-022 Hitung transport bulanan | PRL-DEC-46 | Arsitektur 5.3 | `PAY-BE-14`, `PAY-BE-18` | — | `UAT-08` | Belum dikerjakan |
| FR-PGJ-023 Transport ganda menggagalkan pegawai | PRL-DEC-46 | `CALC-01` | `PAY-BE-18` | `PAY-FE-15` (peringatan sebelum simpan) | `UAT-09` | Belum dikerjakan |
| FR-PGJ-024 Metode transport lain diberi peringatan | PRL-DEC-46 | `CALC-02` | `PAY-BE-18` | `PAY-FE-14` (keterangan) | `UAT-10` | Belum dikerjakan |
| FR-PGJ-030 Buat input variabel | PRL-DEC-07, 31 | Kontrak Payroll Variable Input | `PAY-BE-14`, `PAY-BE-19` | `PAY-FE-17` | `UAT-11` | Belum dikerjakan |
| FR-PGJ-031 Impor atomik | PRL-DEC-07 | `VI-08` | `PAY-BE-19` | `PAY-FE-17` | `UAT-12` | Belum dikerjakan |
| FR-PGJ-032 Ajukan, setujui, tolak, batalkan | PRL-DEC-10, 43 | Matriks status bagian 1 | `PAY-BE-19` | `PAY-FE-17` | `UAT-11` | Belum dikerjakan |
| FR-PGJ-033 Hanya input disetujui dihitung | PRL-DEC-07 | Arsitektur 5.4 | `PAY-BE-20` | — | `UAT-11`, `UAT-13` | Belum dikerjakan |
| FR-PGJ-034 Honor wajib alasan | PRL-DEC-31 | `VI-06` | `PAY-BE-19` | `PAY-FE-17` | `UAT-15` | Belum dikerjakan |
| FR-PGJ-035 Nomor dari number series | `QBE-CODE-002` | Arsitektur 4.2 | `PAY-BE-14`, `PAY-BE-19` | — | QBE conformance | Belum dikerjakan |
| FR-PGJ-036 Penjaga pengajuan run | PRL-DEC-07 | `VI-10`, `VI-11` | `PAY-BE-20` | `PAY-FE-17` (peringatan hitung ulang) | `UAT-13`, `UAT-14` | Belum dikerjakan |
| FR-PGJ-037 Status input mengikuti run | PRL-DEC-07 | Matriks status bagian 3 | `PAY-BE-20` | — | `UAT-11`, `UAT-15b` | Belum dikerjakan |
| FR-PGJ-040 Simulasi berbatas | PRL-DEC-32 | Arsitektur 5.5; `SIM-02` | `PAY-BE-14`, `PAY-BE-21` | `PAY-FE-18` | `UAT-17` | Belum dikerjakan |
| FR-PGJ-041 Simulasi tanpa jejak | PRL-DEC-32 | Arsitektur 5.5 | `PAY-BE-21` | `PAY-FE-18` | `UAT-16` | Belum dikerjakan |
| FR-PGJ-042 Hasil simulasi rinci | PRL-DEC-32 | Kontrak `PayrollSimulationResponse` | `PAY-BE-21` | `PAY-FE-18` | `UAT-16` | Belum dikerjakan |
| FR-PGJ-050 Beban gaji per Cost Center | PRL-DEC-27, 41 | Kontrak Report | `PAY-BE-22` | `PAY-FE-20` | `UAT-18` | Belum dikerjakan |
| FR-PGJ-051 Laporan PPh dan BPJS | PRL-DEC-33 | Kontrak Report | `PAY-BE-22` | `PAY-FE-20` | `UAT-20` | Belum dikerjakan |
| FR-PGJ-052 Laporan dijaga hak nominal | PRL-DEC-34 | Matriks hak akses | `PAY-BE-22` | `PAY-FE-20` | `UAT-19` | Belum dikerjakan |
| FR-PGJ-060 Rekap lembur per run | PRL-DEC-42 | Kontrak Payroll Run | `PAY-BE-23` | `PAY-FE-19` | `UAT-21` | Belum dikerjakan |
| FR-PGJ-061 Tarif lembur baca-saja | PRL-DEC-42 | Kontrak Overtime Rate (HR) | — (endpoint HR sudah ada) | `PAY-FE-19` | `UAT-22` | Belum dikerjakan |

## Gap

| Gap | Keterangan |
|---|---|
| Tidak ada requirement tanpa task | Ke-29 FR punya task dan bukti verifikasi |
| FR-PGJ-004, 005, 006, 022, 033, 037 tanpa task frontend | Disengaja: hasilnya tampil di detail run dan slip yang sudah membaca baris komponen secara umum; diverifikasi lewat UAT |
| Data master untuk UAT | Nominal tunjangan sah (DEC-PRL-004) belum ditetapkan; UAT boleh memakai data uji samaran, produksi menunggu keputusan |
| Hak akses baru | Admin harus memberi hak `SalaryStructureAllowance`, `PayrollVariableInput`, `PayrollRun : Simulate` sebelum UAT; bukan task kode |
| Epic `OPEN DECISION` | `PGJ-08` s.d. `PGJ-17` tidak punya task sampai keputusannya turun; tercatat di `../04-prd-to-mvp.md` bagian 10 |
