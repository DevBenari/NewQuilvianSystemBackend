# Roadmap Frontend — Sub-modul Payroll Gaji

| Field | Nilai |
|---|---|
| Blueprint | `PRL-BP-001` / `payroll-gaji`, revision `1` |
| Status roadmap | `draft` — task baru boleh dikerjakan builder setelah pemilik menyetujui roadmap ini |
| Contract version | `PGJ-C1` (`approved`, Yasmin, 2026-11-12) |
| Baseline SHA | Backend `89b9fd69`, frontend `bfa3bae69` |
| Masukan | `../03-frontend-architecture.md`, `../contracts/api-contract.md`, `../02-module-map.md` bagian 3.1 |
| Repository dan branch | `QuilvianSystemFrontendDev`, branch `yasmina` |
| Laporan task | `payroll-gaji/task/report/frontend/<TASK-ID>.md` di repository backend |
| Ditulis | 2026-11-12 oleh `plan-module-delivery` |

**Aturan untuk setiap task:** gerbang keputusan base component dijalankan sebelum JSX pertama
ditulis. Bentuk form (halaman, modal, drawer), tata letak, nama butir menu, dan ikon tetap
`DEV_DISCRETION`. Isi layar, sumber data, hak akses per tombol, serta kalimat kosong dan gagal
dikunci `../03-frontend-architecture.md`. Pendaftaran butir menu di
`src/utils/menu-sidebar/menu-items.jsx` adalah acceptance criteria task layarnya.

**Kebijakan test:** test otomatis frontend opsional sesuai `rules/frontend/test-policy.md`,
tanpa Jest. Bukti: lint, build, checklist konsistensi UI, dan langkah uji manual.

## Legenda tanda status

| Tanda | Arti |
| :---: | --- |
| ✅ | Acceptance criteria dan DoD sudah terbukti |
| 🟡 | Source sudah ada, kriteria belum terbukti penuh |
| ⛔ | Tertahan keputusan atau gerbang |
| tanpa tanda | Belum disentuh |

## Grafik Urutan Dependency

```text
PAY-FE-14

PAY-FE-15

PAY-BE-15 [BE] ─┬─> PAY-FE-16
                │
PAY-BE-16 [BE] ─┘

PAY-BE-19 [BE] ─┬─> PAY-FE-17 ─┬─> PAY-FE-19
                │              │
PAY-BE-20 [BE] ─┘              │
                               │
PAY-BE-23 [BE] ────────────────┘

PAY-BE-21 [BE] ─> PAY-FE-18

PAY-BE-22 [BE] ─> PAY-FE-20
```

`[BE]` = task backend pada `backend-roadmap.md`, cermin baca-saja; tandanya disalin dari sana.

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | — (endpoint transport sudah ada) | `PAY-FE-14`, `PAY-FE-15` — boleh paralel, bisa mulai sekarang |
| 1 | `PAY-BE-15` dan `PAY-BE-16` | `PAY-FE-16` — menunggu kedua task backend |
| 1 | `PAY-BE-19` dan `PAY-BE-20` | `PAY-FE-17` — menunggu kedua task backend |
| 1 | `PAY-BE-21` | `PAY-FE-18` — menunggu task backend |
| 1 | `PAY-BE-22` | `PAY-FE-20` — menunggu task backend |
| 2 | `PAY-FE-17`, `PAY-BE-23` | `PAY-FE-19` |

Gelombang dihitung dari prasyarat di dalam roadmap ini; prasyarat `[BE]` disebut di kolom kedua
dan harus ✅ sebelum task dimulai.

Jumlah pasangan prasyarat→task: 8 (`BE-15→FE-16`, `BE-16→FE-16`, `BE-19→FE-17`, `BE-20→FE-17`,
`FE-17→FE-19`, `BE-23→FE-19`, `BE-21→FE-18`, `BE-22→FE-20`), sama dengan isi kolom `Dependency`.

`PAY-FE-19` menunggu `PAY-FE-17` karena keduanya menambah tombol jalan masuk di layar detail
run yang sama.

## Tabel task

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
|---|---|---|---|---|---|---|---|---|---|---|
| `PAY-FE-14` | Kebijakan transport dapat diatur | FR-PGJ-020; PRL-DEC-46 | `PGJ-C1` | 9 endpoint `TransportAllowancePolicy`; standar master data | `FE-PGJ-09` + butir menu | — | Kartu | Lint, build, uji manual | — / Yasmin | Kartu |
| `PAY-FE-15` | Transport pegawai dapat diatur | FR-PGJ-021 | `PGJ-C1` | Endpoint `WorkforceTransportAllowance` | `FE-PGJ-10` + tombol masuk | — | Kartu | Lint, build, uji manual | — / Yasmin | Kartu |
| `PAY-FE-16` | Tunjangan tetap struktur dapat dikelola | FR-PGJ-001–003, 010, 011 | `PGJ-C1` | Detail struktur yang ada | `FE-PGJ-01` | `PAY-BE-15`, `PAY-BE-16` | Kartu | Lint, build, uji manual | — / Yasmin | Kartu |
| `PAY-FE-17` | Penghasilan variabel dikelola per run | FR-PGJ-030–036 | `PGJ-C1` | Detail run yang ada | `FE-PGJ-02`, `FE-PGJ-03` + tombol masuk | `PAY-BE-19`, `PAY-BE-20` | Kartu | Lint, build, uji manual | Impor besar / Yasmin | Kartu |
| `PAY-FE-18` | Simulasi dapat dijalankan | FR-PGJ-040–042 | `PGJ-C1` | Opsi periode yang ada | `FE-PGJ-04` + butir menu | `PAY-BE-21` | Kartu | Lint, build, uji manual | — / Yasmin | Kartu |
| `PAY-FE-19` | Rekap dan tarif lembur terbaca | FR-PGJ-060, 061 | `PGJ-C1` | Endpoint `OvertimeRate` HR | `FE-PGJ-05`, `FE-PGJ-06` + grup menu Lembur | `PAY-FE-17`, `PAY-BE-23` | Kartu | Lint, build, uji manual | — / Yasmin | Kartu |
| `PAY-FE-20` | Laporan beban gaji dan PPh/BPJS terbaca | FR-PGJ-050–052 | `PGJ-C1` | Pola layar laporan yang ada | `FE-PGJ-07`, `FE-PGJ-08` + dua butir menu | `PAY-BE-22` | Kartu | Lint, build, uji manual | — / Yasmin | Kartu |

## Kartu task

### `PAY-FE-14` — Kebijakan Tunjangan Transportasi

| Field | Isi |
|---|---|
| Status | Belum dikerjakan |
| Outcome | Petugas dapat mengelola kebijakan transport dari menu Payroll |
| Requirement/decision | FR-PGJ-020; PRL-DEC-46; `03-frontend-architecture.md` bagian 3.6 |
| Reuse | Sembilan endpoint `TransportAllowancePolicy` yang sudah ada; standar fitur master data frontend (rujukan `hr/master-data/job-level`) |
| Cakupan | Layar `FE-PGJ-09` di `/payroll/master-data/transport-allowance-policy`; butir menu di bawah "Kebijakan Tunjangan" |
| Dependency | — |
| Acceptance criteria | 1. Daftar, detail, tambah, ubah, status, hapus berjalan. 2. Cara hitung selain "Bulanan tetap" menampilkan keterangan "Metode ini belum dihitung otomatis di run. Gunakan Penghasilan Variabel." 3. Butir menu dijaga `TransportAllowancePolicy : Read`; tombol tulis dijaga `Create`, `Update`, `Delete`. 4. Empat keadaan (memuat, kosong, gagal, berisi) ada. |
| Verifikasi | ESLint pada berkas yang diubah; build; checklist konsistensi UI; uji manual langkah 1–4 |
| Risiko/pemilik | Controller transport memakai pengecualian `QBE-CODE-002` bertenggat (`PAY-BE-13`); tidak mempengaruhi layar. Pemilik: Yasmin |
| DoD | Kriteria terbukti, butir menu terdaftar, laporan ditulis |

### `PAY-FE-15` — Tunjangan Transportasi pegawai

| Field | Isi |
|---|---|
| Status | Belum dikerjakan |
| Outcome | Petugas dapat menetapkan tunjangan transport per pegawai |
| Requirement/decision | FR-PGJ-021; `03-frontend-architecture.md` bagian 3.6 |
| Reuse | Endpoint `WorkforceTransportAllowance`; detail Profil Penggajian `/payroll/workforce-compensations/[id]` |
| Cakupan | Layar `FE-PGJ-10` di `/payroll/workforce-compensations/[id]/transport-allowance`; tombol "Tunjangan Transportasi" di detail profil |
| Dependency | — |
| Acceptance criteria | 1. Daftar dan form berjalan. 2. Peringatan tampil sebelum menyimpan bila pegawai sudah punya transport bulanan aktif yang beririsan. 3. Tombol dijaga `WorkforceTransportAllowance : *`. |
| Verifikasi | ESLint; build; uji manual |
| Risiko/pemilik | — / Yasmin |
| DoD | Kriteria terbukti, jalan masuk ada, laporan ditulis |

### `PAY-FE-16` — Tunjangan tetap pada Struktur Gaji

| Field | Isi |
|---|---|
| Status | Belum dikerjakan |
| Outcome | Petugas mengelola tunjangan per jenis dan melihat penolakan batas grade |
| Requirement/decision | FR-PGJ-001–003, 010, 011; `03-frontend-architecture.md` bagian 3.1 |
| Reuse | Detail Struktur Gaji yang sudah ada di `/payroll/master-data/salary-structure/[slug]` |
| Cakupan | Bagian Tunjangan Tetap; pesan `SR-01`–`SR-04` pada form struktur dan grade |
| Dependency | `PAY-BE-15`, `PAY-BE-16` |
| Acceptance criteria | 1. Daftar tunjangan menampilkan jenis, cara hitung, nilai, batas maksimum, pajak, prorata. 2. Keadaan kosong berbunyi persis sesuai skema. 3. Tombol dijaga `SalaryStructureAllowance : Create/Update/Delete`. 4. Pesan penolakan server tampil di dekat isian. 5. Benturan 409 memuat ulang data. |
| Verifikasi | ESLint; build; uji manual dengan contoh struktur GR-05 |
| Risiko/pemilik | — / Yasmin |
| DoD | Kriteria terbukti, laporan ditulis |

### `PAY-FE-17` — Penghasilan Variabel per run

| Field | Isi |
|---|---|
| Status | Belum dikerjakan |
| Outcome | Petugas memasukkan, mengimpor, dan memutuskan insentif, bonus, dan honor dari detail run |
| Requirement/decision | FR-PGJ-030–036; `03-frontend-architecture.md` bagian 3.2; `state-transition-matrix.md` bagian 1 |
| Reuse | Detail run `/payroll/payroll-runs/[id]` |
| Cakupan | `FE-PGJ-02` dan `FE-PGJ-03` di `/payroll/payroll-runs/[id]/variable-inputs`; tombol "Penghasilan Variabel" di detail run |
| Dependency | `PAY-BE-19`, `PAY-BE-20` |
| Acceptance criteria | 1. Tombol aksi mengikuti `availableActions` dan hak akses. 2. Impor dengan baris salah menampilkan daftar baris dan sebabnya; tidak ada baris tersimpan. 3. Aksi massal menampilkan hasil per baris. 4. Peringatan hitung ulang tampil bila input berubah setelah `calculatedAt`. 5. Run yang tidak menerima input menjadikan layar baca-saja. 6. Tombol dinonaktifkan selama permintaan berjalan. |
| Verifikasi | ESLint; build; uji manual skenario `UAT-11`–`UAT-14` |
| Risiko/pemilik | Impor ratusan baris perlu umpan balik progres (`DEV_DISCRETION`). Pemilik: Yasmin |
| DoD | Kriteria terbukti, jalan masuk ada, laporan ditulis |

### `PAY-FE-18` — Simulasi Penggajian

| Field | Isi |
|---|---|
| Status | Belum dikerjakan |
| Outcome | Petugas menjalankan simulasi dan membaca hasil per pegawai |
| Requirement/decision | FR-PGJ-040–042; `03-frontend-architecture.md` bagian 3.3 |
| Reuse | `GET /payroll-runs/period-options` (`PayrollRun : ReadPeriods`) |
| Cakupan | `FE-PGJ-04` di `/payroll/simulations`; butir menu di "Proses Penggajian" |
| Dependency | `PAY-BE-21` |
| Acceptance criteria | 1. Kalimat "Hasil simulasi tidak tersimpan dan tidak mempengaruhi run resmi." selalu tampil. 2. Ringkasan dan rincian per pegawai termasuk peringatan dan galat. 3. Penolakan `SIM-01`–`SIM-03` tampil apa adanya. 4. Butir menu dijaga `PayrollRun : Simulate`. |
| Verifikasi | ESLint; build; uji manual `UAT-16`, `UAT-17` |
| Risiko/pemilik | — / Yasmin |
| DoD | Kriteria terbukti, butir menu terdaftar, laporan ditulis |

### `PAY-FE-19` — Rekap Lembur dan Tarif Lembur

| Field | Isi |
|---|---|
| Status | Belum dikerjakan |
| Outcome | Petugas membaca lembur yang dibayar per run dan tarif lembur HR |
| Requirement/decision | FR-PGJ-060, 061; PRL-DEC-42; `03-frontend-architecture.md` bagian 3.4 |
| Reuse | Endpoint `OvertimeRate` HR (baca-saja) |
| Cakupan | `FE-PGJ-05` di `/payroll/payroll-runs/[id]/overtime` dengan tombol "Rekap Lembur" di detail run; `FE-PGJ-06` di `/payroll/overtime-rates`; grup menu "Lembur" |
| Dependency | `PAY-FE-17`, `PAY-BE-23` |
| Acceptance criteria | 1. Rekap menampilkan kolom sesuai skema. 2. Tarif lembur tanpa tombol ubah. 3. Butir "Tarif Lembur" dijaga `OvertimeRate : Read`. 4. Kalimat kosong sesuai skema. |
| Verifikasi | ESLint; build; uji manual `UAT-21`, `UAT-22` |
| Risiko/pemilik | Peran Payroll perlu hak `OvertimeRate : Read` dari admin. Pemilik: Yasmin |
| DoD | Kriteria terbukti, menu dan jalan masuk ada, laporan ditulis |

### `PAY-FE-20` — Laporan Beban Gaji dan PPh/BPJS

| Field | Isi |
|---|---|
| Status | Belum dikerjakan |
| Outcome | Kepala Payroll dan Finance membaca dua laporan baru |
| Requirement/decision | FR-PGJ-050–052; `03-frontend-architecture.md` bagian 3.5 |
| Reuse | Pola layar `/payroll/reports/recap` yang ada |
| Cakupan | `FE-PGJ-07` di `/payroll/reports/expense-summary`, `FE-PGJ-08` di `/payroll/reports/tax-contribution`; dua butir menu di "Laporan" |
| Dependency | `PAY-BE-22` |
| Acceptance criteria | 1. Saringan periode atau run, Cost Center, cari pegawai. 2. Hasil kosong berbunyi "Belum ada run yang disahkan pada periode ini." 3. Butir menu dijaga `PayrollReport : ViewAmount`; tanpa hak tidak tampil. 4. Nominal tidak disimpan di penyimpanan browser. |
| Verifikasi | ESLint; build; uji manual `UAT-18`–`UAT-20` |
| Risiko/pemilik | — / Yasmin |
| DoD | Kriteria terbukti, butir menu terdaftar, laporan ditulis |
