# PRD → MVP — Sub-modul Payroll Gaji

## 1. Identitas dokumen

| Field | Nilai |
|---|---|
| Produk | Quilvian V2 — Payroll Management |
| Sub-modul | `payroll-gaji` (blueprint `PRL-BP-001`, bentuk `COMPOSITE`) |
| Status | `approved` — `approved_by`: Yasmin; `approved_at`: 2026-11-12 |
| `contract_version` | `PGJ-C1` |
| Repository target | `NewQuilvianSystemBackend` (branch `Yasmina`), `QuilvianSystemFrontendDev` (branch `yasmina`) |
| Baseline SHA | Backend `89b9fd69`, frontend `bfa3bae69` |
| `input_revision` | `02-backend-architecture.md` r1, `data/data-dictionary.md` r1, seluruh `contracts/` r1, `../00-interview-decisions.md` r1, `../01-existing-capability-map.md` r1 |
| Kode dokumen | `PGJ` |
| Ringkasan cakupan | Tunjangan tetap dari Struktur Gaji, batas struktur di dalam grade, tunjangan transport bulanan, penghasilan variabel manual yang disetujui, simulasi, laporan beban gaji dan PPh/BPJS, rekap dan tarif lembur baca-saja |

## 2. Ringkasan eksekutif

Gaji bulanan rumah sakit saat ini sudah dapat dihitung, disetujui, dibayar lewat Finance, dan
dicetak sebagai slip. Tetapi tunjangan tetap masih satu angka yang diketik di profil pegawai,
insentif dan honor tidak punya tempat input, tunjangan transport tidak pernah masuk gaji, dan
petugas tidak bisa mencoba hitungan sebelum run resmi.

MVP ini menjadikan Struktur Gaji sumber tunjangan tetap yang terinci per jenis, memberi petugas
tempat memasukkan dan menyetujui insentif, bonus, dan honor tugas tambahan, menghitung tunjangan
transport bulanan, serta menyediakan simulasi dan laporan beban gaji per cost center untuk
Finance. Hasil bisnisnya: slip yang terinci, gaji yang dapat dijelaskan per komponen, dan tidak
ada tunjangan yang dibayar dua kali.

## 3. Masalah produk

| Kondisi sekarang | Bukti @ `89b9fd69` |
|---|---|
| Tunjangan tetap = satu angka profil, satu baris slip, selalu kena pajak | `PayrollCalculationService.cs` baris 276 dan 344 |
| Master Jenis Tunjangan dan Struktur Gaji ada tetapi tidak dibaca perhitungan | `01-existing-capability-map.md` P-04, P-07 |
| Batas gaji struktur tidak dijaga terhadap grade | `SalaryStructureController.cs` baris 282–351, tanpa pemeriksaan grade |
| Tidak ada tempat input insentif, bonus, honor; input `Submitted` ikut dihitung | P-17, F-12 |
| Tunjangan transport punya backend tetapi tidak dihitung dan tidak punya layar | P-12, F-13 |
| Tidak ada simulasi, laporan beban per komponen, laporan PPh/BPJS, layar rekap lembur | P-15, P-25, P-18 |

Yang sudah ada dan dipakai apa adanya: siklus run, persetujuan run, potongan cicilan, handoff
Finance, slip, rekap dan rekonsiliasi.

## 4. Visi produk

1. Petugas Payroll mengatur tunjangan tetap per jenis di Struktur Gaji.
2. HRD menetapkan gaji pokok pegawai pada struktur dan grade yang sah.
3. Run menghitung gaji pokok dan setiap tunjangan dari struktur penetapan, ditambah transport.
4. Petugas memasukkan insentif, bonus, dan honor yang disetujui ke run yang sama.
5. Pengesah mengajukan dan mengesahkan run yang hasilnya sesuai input terbaru.
6. Finance menerima instruksi pembayaran dan laporan beban gaji per cost center.
7. Pegawai menerima slip yang merinci setiap tunjangan dan penghasilan.

## 5. Batas MVP

**Titik mulai:**

1. Struktur Gaji aktif dan grade-nya sudah ada.
2. Pegawai sudah punya penetapan gaji yang disetujui HRD.
3. Periode penggajian terbuka.

**Titik akhir:**

1. Run bulanan disahkan dengan tunjangan per jenis, transport bulanan, dan penghasilan variabel
   yang disetujui.
2. Slip menampilkan baris-baris itu.
3. Laporan beban gaji per cost center dan laporan PPh/BPJS tersedia untuk run tersebut.

## 6. Pelaku sasaran

| Pelaku | Tanggung jawab di MVP |
|---|---|
| Petugas Payroll | Mengatur tunjangan struktur dan transport; memasukkan, mengajukan, menyetujui input variabel; menjalankan simulasi; menghitung run |
| Pengesah run | Mengajukan dan mengesahkan run |
| Petugas HRD | Mengubah rentang grade; menetapkan gaji pokok (di modul HRD) |
| Kepala Payroll, Finance | Membaca laporan beban gaji dan PPh/BPJS |
| Pegawai | Membaca slip |

## 7. Pemilihan kemampuan MVP

| Kemampuan | ID kemampuan asal | Keputusan MVP |
|---|---|---|
| Tunjangan tetap per jenis dari Struktur Gaji, dengan masa peralihan | P-07, P-09 | Wajib; tanpa ini tunjangan tetap tetap angka tunggal tanpa dasar |
| Batas gaji struktur di dalam grade | P-06, P-07 | Wajib; struktur tanpa batas yang sah membuat penetapan gaji keluar plafon tanpa terlihat |
| Tunjangan transport bulanan tetap beserta layar pengaturannya | P-12 | Wajib; data transport sudah ada tetapi tidak pernah dibayar |
| Penghasilan variabel manual yang disetujui | P-17 | Wajib; insentif, bonus, dan honor saat ini tidak punya jalur |
| Simulasi | P-15 | Wajib; petugas butuh memeriksa dampak tunjangan baru sebelum run resmi pertama |
| Laporan beban gaji per cost center (pengganti Jurnal) dan laporan PPh/BPJS | P-25, P-20 | Wajib; Finance butuh dasar pembukuan karena Payroll tidak membuat jurnal |
| Rekap lembur per run dan tarif lembur baca-saja | P-18 | Wajib; petugas tidak dapat memeriksa lembur yang dibayar tanpa membuka modul HR |

## 8. Kemampuan yang ditunda

| Kemampuan | ID kemampuan asal | Alasan ditunda | Pengganti selama MVP |
|---|---|---|---|
| Penyimpangan tunjangan per pegawai | P-09 | Butuh persetujuan HRD atas alur pengecualian (DEC-PRL-002) dan pejabat penyetuju (DEC-PRL-013) | Struktur Gaji khusus untuk kelompok kecil, atau penghasilan variabel per bulan |
| Pengecualian penetapan gaji di luar rentang grade | P-08 | Mengubah layar milik HRD (DEC-PRL-002) | Persetujuan penetapan gaji yang sudah ada |
| Kebijakan tunjangan jabatan, profesi, jenjang | P-10 | Data jabatan, profesi, jenjang pegawai belum terbukti (DEC-PRL-003); prefix belum terdaftar (DEC-PRL-014) | Baris jenis tunjangan di Struktur Gaji, atau angka profil |
| Tunjangan Shift, Siaga, Risiko | P-11 | Rekap aktivitas dari HR/Scheduling belum punya kontrak (DEC-PRL-005) | Penghasilan variabel per pegawai |
| Transport per kehadiran, per hari, penggantian | P-12 | Snapshot kehadiran tidak memuat jumlah hari hadir (DEC-PRL-005) | Penghasilan variabel per pegawai |
| Pensiun mesin Modern | X-02 | Menunggu persetujuan MHamzah (DEC-PRL-001) | Mesin Modern dibekukan; layar Payroll hanya memakai mesin V2 |
| THR dan bonus tahunan sebagai run sendiri | P-14 | Ketentuan pegawai keluar dan cuti (DEC-PRL-006) dan PPh gabungan (DEC-PRL-007) belum disahkan | Dibayar di luar sistem seperti sekarang |
| Sumber Dana | P-05 | Definisi dan tingkat penautan menunggu HRD dan Finance (DEC-PRL-008) | Laporan per cost center |
| Dashboard | P-01 | Empat metrik belum didefinisikan (DEC-PRL-009) | Rekap Penggajian yang sudah ada |
| Aturan Komponen | P-02 | Dimensi kelayakan belum ditetapkan (DEC-PRL-010) | Penanda pajak dan prorata pada master |
| Pengaturan menu PPh/BPJS | P-20 | Isinya belum ditetapkan (DEC-PRL-016) | Tetapan profil pegawai |
| Seluruh Remunerasi dan Jasa | R-01–R-08 | Sub-modul `remunerasi-jasa` `BLOCKED` | Integrasi Jasa Medis yang sudah ada |

## 9. Alur bisnis target

`FLOW-PGJ-MVP-001` — penggajian bulanan:

1. Petugas Payroll mengisi baris tunjangan pada Struktur Gaji.
2. Petugas mengatur tunjangan transport pegawai yang berhak.
3. Petugas membuat run periode dan mengimpor data HR.
4. Petugas memasukkan atau mengimpor insentif, bonus, honor; mengajukan dan menyetujuinya.
5. Petugas menjalankan simulasi bila perlu.
6. Petugas menghitung run.
7. Pengesah mengajukan run; sistem menolak bila ada input menunggu keputusan atau berubah setelah
   dihitung.
8. Pengesah mengesahkan run; input manual menjadi `Applied`.
9. Petugas mengirim pembayaran ke Finance dan menerbitkan slip.
10. Kepala Payroll dan Finance membaca laporan beban gaji dan PPh/BPJS.

Diagram dan tabel langkah ada di `flowcharts/00-alur-utama.md`.

## 10. Epic dan functional requirement

### `EPIC PGJ-01` — Tunjangan tetap dari Struktur Gaji — `MUST HAVE` — `EXTEND`

| FR | Requirement |
|---|---|
| FR-PGJ-001 | Petugas dapat menambah, mengubah, dan menghapus baris tunjangan struktur dengan cara hitung nominal tetap atau persentase gaji pokok. **Contoh:** Tunjangan Keahlian 5% pada struktur GR-05. |
| FR-PGJ-002 | Sistem menolak jenis tunjangan yang sama dua kali di satu struktur (`SA-04`, kode 409). |
| FR-PGJ-003 | Sistem menolak jenis tunjangan yang dipakai kebijakan Shift, Siaga, atau Risiko aktif (`SA-06`). |
| FR-PGJ-004 | Run menghitung satu baris per jenis tunjangan, dengan batas maksimum dan prorata. **Contoh:** gaji pokok Rp 9.000.000, keahlian 5% = Rp 450.000 dipotong ke batas Rp 400.000. |
| FR-PGJ-005 | Struktur kosong dan profil berisi Rp 1.200.000: run memakai Rp 1.200.000 dengan peringatan `CALC-03`, selama saklar `AllowProfileAllowanceFallback` menyala. |
| FR-PGJ-006 | Struktur berisi dan profil berisi: angka profil diabaikan dengan peringatan `CALC-04`; tidak ada penjumlahan ganda. |

### `EPIC PGJ-02` — Batas struktur di dalam grade — `MUST HAVE` — `EXTEND`

| FR | Requirement |
|---|---|
| FR-PGJ-010 | Simpan struktur ditolak bila batas bawah, batas atas, atau gaji bawaan berada di luar rentang grade. **Contoh:** grade Rp 6.000.000–Rp 9.000.000, batas atas struktur Rp 10.000.000 ditolak (`SR-02`). |
| FR-PGJ-011 | Ubah grade ditolak bila rentang baru membuat struktur aktif keluar batas, dengan daftar kode struktur (`SR-04`, kode 409). |

### `EPIC PGJ-03` — Tunjangan transport bulanan tetap — `MUST HAVE` — `EXTEND`

| FR | Requirement |
|---|---|
| FR-PGJ-020 | Layar Kebijakan Tunjangan Transportasi memakai sembilan endpoint yang sudah ada. |
| FR-PGJ-021 | Layar Tunjangan Transportasi per pegawai memakai endpoint yang sudah ada. |
| FR-PGJ-022 | Run menghitung transport bulanan tetap dengan batas bulanan dan prorata. **Contoh:** Rp 400.000, berlaku 20 dari 30 hari = Rp 266.667. |
| FR-PGJ-023 | Dua tunjangan transport bulanan aktif yang beririsan membuat pegawai gagal dihitung (`CALC-01`); pegawai lain tetap terhitung. |
| FR-PGJ-024 | Metode selain bulanan tetap menghasilkan peringatan `CALC-02` dan tidak dihitung. |

### `EPIC PGJ-04` — Penghasilan variabel manual — `MUST HAVE` — `EXTEND`

| FR | Requirement |
|---|---|
| FR-PGJ-030 | Petugas dapat membuat input `Draft` untuk pegawai di run yang menerima input, dengan komponen penghasilan aktif dan nominal > 0. |
| FR-PGJ-031 | Impor banyak baris berlaku semua atau tidak sama sekali (`VI-08`). **Contoh:** 120 baris dengan 2 baris salah: tidak ada yang tersimpan. |
| FR-PGJ-032 | Petugas dapat mengajukan, menyetujui (termasuk input sendiri), menolak dengan alasan, dan membatalkan dengan alasan, satu per satu maupun massal. |
| FR-PGJ-033 | Hanya input manual `Approved` atau `Applied` yang dihitung. Sumber otomatis tidak berubah. |
| FR-PGJ-034 | Honor tugas tambahan wajib menyebut tugasnya (`VI-06`). |
| FR-PGJ-035 | Nomor input dialokasikan seri `PVI` lewat `NumberSeriesAllocator`. |
| FR-PGJ-036 | Pengajuan run ditolak bila ada input manual `Submitted` (`VI-10`) atau input yang berubah setelah perhitungan terakhir (`VI-11`). |
| FR-PGJ-037 | Run disahkan: input manual `Approved` menjadi `Applied`. Run dibatalkan: input manual yang belum `Applied` menjadi `Cancelled` dengan alasan "Run dibatalkan". |

### `EPIC PGJ-05` — Simulasi — `MUST HAVE` — `MISSING / NEW`

| FR | Requirement |
|---|---|
| FR-PGJ-040 | Petugas dapat mensimulasikan satu periode untuk paling banyak `SimulationMaxEmployees` pegawai (bawaan 50); lebih dari itu ditolak (`SIM-02`). |
| FR-PGJ-041 | Setelah simulasi, tidak ada run, input, atau perubahan periode yang tersimpan, dan run resmi tidak berubah. |
| FR-PGJ-042 | Hasil memuat baris komponen, peringatan, dan galat per pegawai, dihitung dengan aturan yang sama dengan run resmi. |

### `EPIC PGJ-06` — Laporan beban gaji dan PPh/BPJS — `MUST HAVE` — `EXTEND`

| FR | Requirement |
|---|---|
| FR-PGJ-050 | Laporan beban gaji per Cost Center per komponen hanya dari run yang sudah disahkan. **Contoh:** Cost Center Rawat Inap, Tunjangan Jabatan, 30 pegawai, Rp 22.500.000. |
| FR-PGJ-051 | Laporan PPh dan BPJS menampilkan nilai yang dipakai run beserta sumber "Tetapan profil". |
| FR-PGJ-052 | Kedua laporan hanya untuk pemegang `PayrollReport : ViewAmount`. |

### `EPIC PGJ-07` — Lembur baca-saja — `MUST HAVE` — `EXTEND`

| FR | Requirement |
|---|---|
| FR-PGJ-060 | Rekap lembur terverifikasi yang diimpor ke run tampil per pegawai per tanggal. |
| FR-PGJ-061 | Tarif lembur tampil baca-saja dari endpoint HR; tidak ada tombol ubah di Payroll. |

### Epic `OPEN DECISION` — tidak masuk gelombang mana pun

| Epic | Keputusan yang ditunggu |
|---|---|
| `EPIC PGJ-08` Penyimpangan tunjangan per pegawai | DEC-PRL-002, 013 |
| `EPIC PGJ-09` Pengecualian penetapan gaji | DEC-PRL-002, 013 |
| `EPIC PGJ-10` Kebijakan tunjangan jabatan, profesi, jenjang | DEC-PRL-003, 004, 014 |
| `EPIC PGJ-11` Tunjangan Shift, Siaga, Risiko, transport per kehadiran | DEC-PRL-005 |
| `EPIC PGJ-12` Pensiun mesin Modern | DEC-PRL-001 |
| `EPIC PGJ-13` Run THR dan bonus tahunan | DEC-PRL-006, 007 |
| `EPIC PGJ-14` Sumber Dana | DEC-PRL-008, 014 |
| `EPIC PGJ-15` Dashboard | DEC-PRL-009 |
| `EPIC PGJ-16` Aturan Komponen | DEC-PRL-010 |
| `EPIC PGJ-17` Pengaturan menu PPh/BPJS | DEC-PRL-016 |

## 11. Model status yang diusulkan

| Status input manual | Invariant |
|---|---|
| `Draft` | Bisa diubah; tidak dihitung |
| `Submitted` | Tidak dihitung; menahan pengajuan run |
| `Approved` | Dihitung; perubahan setelah perhitungan terakhir menahan pengajuan run |
| `Applied` | Final; sudah masuk run yang disahkan |
| `Rejected`, `Cancelled` | Final; tidak dihitung; alasan wajib |

Lengkapnya di `contracts/state-transition-matrix.md`.

## 12. Sasaran arsitektur

| Dipakai ulang | Diperluas | Baru |
|---|---|---|
| Siklus run, persetujuan run, handoff Finance, slip, endpoint transport, endpoint tarif lembur HR, `NumberSeriesAllocator`, `PayrollVersionToken` | `PrlPayrollVariableInput` (lima kolom), `PayrollCalculationService`, `PayrollRunService`, `PayrollReportService`, `PayrollRunsController`, `PayrollReportsController`, validasi `SalaryStructureController` dan `SalaryGradeController` | `MstSalaryStructureAllowance`, empat service, dua controller, sepuluh layar frontend |

Rinciannya di `02-backend-architecture.md` dan `03-frontend-architecture.md`.

## 13. Sasaran kemampuan API

Bagian dari `contracts/api-contract.md`; tidak melebihinya.

### Corporate / Payroll Management / Master Data / Salary Structure Allowance

Base URL: `api/v1/corporate/payroll-management/master-data/salary-structures/{salaryStructureId}/allowances`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
|---|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar tunjangan struktur | `SalaryStructureAllowance : Read` | — | `List<SalaryStructureAllowanceResponse>` | `PGJ-01` | **Rencana (belum tersedia)** |
| `POST` | `/` | Tambah tunjangan | `SalaryStructureAllowance : Create` | `CreateSalaryStructureAllowanceRequest` | `SalaryStructureAllowanceResponse` | `PGJ-01` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Ubah tunjangan | `SalaryStructureAllowance : Update` | `UpdateSalaryStructureAllowanceRequest` | `SalaryStructureAllowanceResponse` | `PGJ-01` | **Rencana (belum tersedia)** |
| `DELETE` | `/{id}` | Hapus tunjangan | `SalaryStructureAllowance : Delete` | query `expectedRowVersion` | `object` | `PGJ-01` | **Rencana (belum tersedia)** |

### Corporate / Payroll Management / Payroll Variable Input

Base URL: `api/v1/corporate/payroll-management/payroll-runs/{runId}/variable-inputs`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
|---|---|---|---|---|---|---|---|
| `GET` | `/`, `/{id}` | Daftar dan detail input | `PayrollVariableInput : Read` | query | `PagedResult<PayrollVariableInputResponse>` | `PGJ-04` | **Rencana (belum tersedia)** |
| `POST` | `/` | Buat input | `PayrollVariableInput : Create` | `CreatePayrollVariableInputRequest` | `PayrollVariableInputResponse` | `PGJ-04` | **Rencana (belum tersedia)** |
| `POST` | `/import` | Impor | `PayrollVariableInput : Import` | `ImportPayrollVariableInputRequest` | `PayrollVariableInputImportResult` | `PGJ-04` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/content` | Ubah isi `Draft` | `PayrollVariableInput : Update` | `UpdatePayrollVariableInputContentRequest` | `PayrollVariableInputResponse` | `PGJ-04` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/submit`, `/bulk-submit` | Ajukan | `PayrollVariableInput : Submit` | `PayrollVariableInputActionRequest`, `PayrollVariableInputBulkRequest` | respons input atau hasil massal | `PGJ-04` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/approve`, `/bulk-approve` | Setujui | `PayrollVariableInput : Approve` | sama | sama | `PGJ-04` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/reject` | Tolak | `PayrollVariableInput : Reject` | `PayrollVariableInputDecisionRequest` | `PayrollVariableInputResponse` | `PGJ-04` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/cancel` | Batalkan | `PayrollVariableInput : Cancel` | `PayrollVariableInputDecisionRequest` | `PayrollVariableInputResponse` | `PGJ-04` | **Rencana (belum tersedia)** |

### Corporate / Payroll Management / Payroll Run

Base URL: `api/v1/corporate/payroll-management/payroll-runs`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
|---|---|---|---|---|---|---|---|
| `POST` | `/simulations` | Simulasi | `PayrollRun : Simulate` | `PayrollSimulationRequest` | `PayrollSimulationResponse` | `PGJ-05` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/overtime-inputs` | Rekap lembur run | `PayrollRun : Read` | query | `PagedResult<PayrollRunOvertimeInputResponse>` | `PGJ-07` | **Rencana (belum tersedia)** |

### Corporate / Payroll Management / Report

Base URL: `api/v1/corporate/payroll-management/reports`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
|---|---|---|---|---|---|---|---|
| `GET` | `/expense-summary` | Beban gaji per Cost Center | `PayrollReport : ViewAmount` | query | `PayrollExpenseSummaryResponse` | `PGJ-06` | **Rencana (belum tersedia)** |
| `GET` | `/tax-contribution` | PPh dan BPJS | `PayrollReport : ViewAmount` | query | `PagedResult<PayrollTaxContributionRowResponse>` | `PGJ-06` | **Rencana (belum tersedia)** |

Endpoint yang sudah ada dan dipakai layar baru: Transport Allowance Policy, Transport Allowance,
dan Overtime Rate (lihat `contracts/api-contract.md` bagian 2).

## 14. Matriks kewenangan

| Pelaku | Hak akses |
|---|---|
| Petugas Payroll | `SalaryStructureAllowance : Read`, `Create`, `Update`, `Delete`; `PayrollVariableInput : Read`, `Create`, `Import`, `Update`, `Submit`, `Approve`, `Reject`, `Cancel`; `PayrollRun : Simulate`, `ReadPeriods`, `Read`, `Calculate`; `TransportAllowancePolicy : Read`, `Create`, `Update`, `Delete`; `WorkforceTransportAllowance : Read`, `Create`, `Update`, `Delete`; `OvertimeRate : Read` |
| Pengesah run | `PayrollRun : Submit`, `Approve`, `Cancel` |
| Petugas HRD | `SalaryGrade : Update`; `SalaryStructureAllowance : Read` |
| Kepala Payroll, Finance | `PayrollReport : ViewAmount` |

Usulan ini ditetapkan admin di layar Akses Role; kode tidak memeriksa nama peran.

## 15. Batas integrasi dan billing

Sub-modul ini **MUST NOT**: menulis ke tabel HR, Scheduling, Medical Fee, atau Finance; membuat
jurnal; mengirim instruksi ke bank; memanggil mesin Modern; membuat tabel run atau komponen di
luar `Prl*`. Tidak ada dampak billing pasien. Rinciannya di `contracts/integration-contract.md`.

## 16. Guardrail regulasi

| Kewajiban | Penerapan di MVP |
|---|---|
| Kerahasiaan penghasilan pegawai | Nominal tidak masuk log; laporan nominal hanya untuk `PayrollReport : ViewAmount`; slip pegawai mengikuti hak `MyPayslip` yang sudah ada |
| Perpajakan PPh 21 | Tidak ada perhitungan pajak baru; nilai tetapan profil dilaporkan apa adanya (PRL-DEC-33) |
| Jejak audit penggajian | Pengaju, penyetuju, penolak, alasan, dan snapshot hasil hitung tersimpan |

Rekam medis tidak tersentuh.

## 17. Kebutuhan non-fungsional

| ID | Kebutuhan |
|---|---|
| NFR-001 | Impor input variabel atomik: satu transaksi untuk seluruh baris |
| NFR-002 | Aksi tulis memakai token versi; benturan menghasilkan 409 dan tidak menimpa data |
| NFR-003 | Aksi status idempoten |
| NFR-004 | Simulasi tidak meninggalkan data apa pun di basis data utama |
| NFR-005 | Nomor input dari number series atomik, bukan Count/Max+1 |
| NFR-006 | Log aksi tulis tanpa nominal |
| NFR-007 | Tanggal dihitung dengan aturan prorata hari kalender yang sama dengan gaji pokok |

## 18. Skenario UAT

> **UAT-01 — Tunjangan per jenis di slip.** Kondisi awal: struktur GR-05 memuat Jabatan
> Rp 750.000, Makan Rp 500.000 (tidak prorata), Keahlian 5% batas Rp 400.000; pegawai bergaji
> pokok Rp 6.000.000 masuk tanggal 16 pada bulan 30 hari. Langkah: hitung run. Hasil: Jabatan
> Rp 375.000, Makan Rp 500.000, Keahlian Rp 150.000 sebagai tiga baris terpisah.

> **UAT-02 — Gagal: jenis tunjangan ganda.** Kondisi awal: struktur sudah memuat Tunjangan
> Jabatan. Langkah: tambah Tunjangan Jabatan lagi. Hasil: ditolak dengan pesan `SA-04`.

> **UAT-04 — Tidak terhitung ganda.** Kondisi awal: struktur berisi total Rp 1.550.000, profil
> `TotalAllowance` Rp 1.200.000. Langkah: hitung run. Hasil: hanya baris struktur
> (Rp 1.550.000) dan peringatan `CALC-04`.

> **UAT-06 — Gagal: batas struktur melebihi grade.** Kondisi awal: grade GR-05 Rp 6.000.000–
> Rp 9.000.000. Langkah: simpan struktur dengan batas atas Rp 10.000.000. Hasil: ditolak `SR-02`.

> **UAT-07 — Gagal: grade dipersempit.** Kondisi awal: struktur aktif berbatas atas
> Rp 9.000.000. Langkah: ubah batas atas grade menjadi Rp 8.500.000. Hasil: ditolak `SR-04`
> beserta kode struktur.

> **UAT-08 — Transport bulanan diprorata.** Kondisi awal: kebijakan bulanan Rp 400.000,
> diprorata; pegawai berlaku tanggal 11 pada bulan 30 hari. Langkah: hitung run. Hasil: baris
> transport Rp 266.667.

> **UAT-09 — Gagal: transport ganda.** Kondisi awal: dua tunjangan transport bulanan aktif
> beririsan untuk satu pegawai. Langkah: hitung run. Hasil: pegawai itu gagal dihitung (`CALC-01`),
> pegawai lain terhitung.

> **UAT-11 — Insentif disetujui masuk gaji.** Langkah: impor 120 insentif, ajukan massal,
> setujui massal, hitung, ajukan, sahkan run. Hasil: insentif ada di gaji; seluruh input
> `Applied`.

> **UAT-12 — Gagal: impor dengan baris salah.** Langkah: impor 120 baris dengan baris 17 pegawai
> tidak ada dan baris 64 bernominal 0. Hasil: ditolak `VI-08`; tidak ada baris tersimpan.

> **UAT-13 — Gagal: input menunggu keputusan.** Langkah: ajukan run saat 3 input `Submitted`.
> Hasil: ditolak `VI-10`.

> **UAT-14 — Gagal: input berubah setelah dihitung.** Langkah: hitung run pukul 13.00, setujui
> input pukul 14.00, ajukan run. Hasil: ditolak `VI-11`; setelah hitung ulang pengajuan diterima.

> **UAT-16 — Simulasi tidak menyimpan apa pun.** Langkah: simulasikan 48 pegawai. Hasil: hasil
> tampil; jumlah run, input, dan status periode sama dengan sebelum simulasi.

> **UAT-17 — Gagal: simulasi terlalu besar.** Langkah: simulasikan 60 pegawai. Hasil: ditolak
> `SIM-02`.

> **UAT-18 — Laporan beban gaji.** Langkah: buka laporan untuk run Mei yang disahkan. Hasil:
> jumlah baris sama dengan total penghasilan dan potongan run.

> **UAT-19 — Gagal: tanpa hak nominal.** Langkah: pengguna tanpa `PayrollReport : ViewAmount`
> membuka laporan. Hasil: butir menu tidak tampil; panggilan langsung ditolak 403.

> **UAT-21 — Rekap lembur.** Langkah: buka rekap lembur run Mei. Hasil: baris sama dengan
> lembur terverifikasi yang diimpor.

> **UAT-22 — Gagal: mengubah tarif lembur.** Langkah: buka Tarif Lembur di Payroll. Hasil: tidak
> ada tombol ubah.

Matriks lengkap, termasuk UAT-02b, 03, 05, 10, 15, 15b, dan 20, ada di
`testing/acceptance-test-matrix.md`.

## 19. Definition of Done

| Butir | Bukti |
|---|---|
| Tunjangan tetap tampil per jenis dari Struktur Gaji | `UAT-01`, `UAT-04` |
| Tidak ada tunjangan atau transport yang terhitung ganda | `UAT-04`, `UAT-09` |
| Struktur di luar rentang grade tidak dapat disimpan | `UAT-06`, `UAT-07` |
| Input manual hanya dihitung setelah disetujui, dan run tidak dapat diajukan dengan input basi | `UAT-11`, `UAT-13`, `UAT-14` |
| Impor berlaku semua atau tidak sama sekali | `UAT-12` |
| Simulasi tidak meninggalkan data | `UAT-16` |
| Laporan nominal hanya untuk pemegang hak | `UAT-18`, `UAT-19` |
| Enam butir menu baru dan empat jalan masuk layar anak ada | Pemeriksaan menu pada `03-frontend-architecture.md` bagian 2 |
| Dua migration baru diterapkan di DB uji | Laporan task migration |
| Master minimum terisi (jenis tunjangan, komponen insentif/bonus/honor/transport, seri `PVI`) | `02-backend-architecture.md` bagian 9 |
| `dotnet build` berhasil dan QBE conformance strict tanpa pelanggaran yang memblokir | Laporan task backend |

## 20. Urutan pengiriman dan pertanyaan terbuka

| Gelombang | Isi | Syarat mulai |
|---|---|---|
| `MVP-0` | Migration `AddSalaryStructureAllowance` dan `AddPayrollVariableInputDecisionColumns`, konstanta dan opsi, registrasi DI, seri nomor `PVI`; `EPIC PGJ-02` | Blueprint disetujui; dua migration Payroll yang tertunda sudah diterapkan |
| `MVP-1` | `EPIC PGJ-01`, `EPIC PGJ-03` | `MVP-0` selesai |
| `MVP-2` | `EPIC PGJ-04` | `MVP-0` selesai |
| `MVP-3` | `EPIC PGJ-05`, `EPIC PGJ-06`, `EPIC PGJ-07` | `MVP-1` dan `MVP-2` selesai |
| `POST-MVP` | Seluruh kemampuan pada bagian 8 dan epic `OPEN DECISION` | Keputusan masing-masing turun |

| Pertanyaan | Siapa yang menjawab | Dampak bila belum dijawab | Memblokir |
|---|---|---|:---:|
| Kapan saklar cadangan profil dimatikan, dan bagaimana angka lama dipindah ke struktur (DEC-PRL-015) | Pemilik bisnis | Saklar tetap menyala; tidak menghalangi pengembangan | Tidak |
| Apakah nominal tunjangan sah sebagai isi master (DEC-PRL-004) | HRD, Finance, Direksi | Data master untuk UAT dan produksi belum dapat diisi final | Tidak (pengembangan); wajib sebelum produksi |
| Pengesahan HRD atas metode tunjangan (PRL-DEC-13) dan rekap lembur baca-saja (PRL-DEC-42) | HRD | Menunggu pengesahan sebelum produksi | Tidak |
| Sidebar ditata ulang ke 13 menu (DEC-PRL-017) | Pemilik | Butir baru ditambahkan ke kelompok yang ada | Tidak |
| Persetujuan pemilik atas blueprint ini | Yasmin | — | **Terjawab** — disetujui Yasmin 2026-11-12 |
