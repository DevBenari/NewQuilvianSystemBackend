# Roadmap Backend — Sub-modul Payroll Gaji

| Field | Nilai |
|---|---|
| Blueprint | `PRL-BP-001` / `payroll-gaji`, revision `1` |
| Status roadmap | `draft` — task baru boleh dikerjakan builder setelah pemilik menyetujui roadmap ini |
| Contract version | `PGJ-C1` (`approved`, Yasmin, 2026-11-12) |
| Baseline SHA | Backend `89b9fd69`, frontend `bfa3bae69` |
| Masukan dan hash | `blueprint-manifest.md` sub-modul (hash artefak 12 November 2026 pasca-approval), `../02-module-map.md` (`2877e0320fd1`), `../00-interview-decisions.md` (`87f302b04484`) |
| Repository dan branch | `NewQuilvianSystemBackend`, branch `Yasmina` |
| Laporan task | `payroll-gaji/task/report/backend/<TASK-ID>.md`, bersebelahan dengan roadmap ini |
| Ditulis | 2026-11-12 oleh `plan-module-delivery` |

**Handoff untuk setiap task:** QBE preflight dan kesesuaian engineering diselesaikan pada saat
eksekusi dari `AGENTS.md` backend dan dokumen engineering canonical (`docs/engineering/`), bukan
dari roadmap ini. Pembuatan berkas migration diizinkan preferensi pemilik 2026-10-05; menjalankan
migration ke database tetap tindakan pemilik.

**Kebijakan test:** backend tidak memelihara project test otomatis
(`AUTOMATED TEST: NOT APPLICABLE`). Bukti verifikasi setiap task: QBE preflight dan conformance,
review diff dan cakupan, `dotnet restore` dan `dotnet build`, verifikasi kontrak API, verifikasi
proses bisnis, dan uji manual atau runtime bila relevan.

## Legenda tanda status

| Tanda | Arti |
| :---: | --- |
| ✅ | Acceptance criteria dan DoD sudah terbukti |
| 🟡 | Source sudah ada, kriteria belum terbukti penuh |
| ⛔ | Tertahan keputusan atau gerbang |
| tanpa tanda | Belum disentuh |

## Grafik Urutan Dependency

```text
PAY-BE-14 🟡 ─┬─> PAY-BE-16 🟡 ─> PAY-BE-17 🟡 ─> PAY-BE-18 🟡 ─┬─> PAY-BE-20 🟡 ─> PAY-BE-21 🟡
              │                                                 │
              └─> PAY-BE-19 🟡 ─────────────────────────────────┘

PAY-BE-15 🟡

PAY-BE-22 🟡

PAY-BE-23 🟡
```

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | — | `PAY-BE-14` 🟡, `PAY-BE-15` 🟡, `PAY-BE-22` 🟡, `PAY-BE-23` 🟡 — boleh paralel |
| 2 | `PAY-BE-14` | `PAY-BE-16` 🟡, `PAY-BE-19` 🟡 — boleh paralel |
| 3 | `PAY-BE-16` | `PAY-BE-17` 🟡 |
| 4 | `PAY-BE-17` | `PAY-BE-18` 🟡 |
| 5 | `PAY-BE-18`, `PAY-BE-19` | `PAY-BE-20` 🟡 |
| 6 | `PAY-BE-20` | `PAY-BE-21` 🟡 |

Jumlah pasangan prasyarat→task: 7 (`14→16`, `16→17`, `17→18`, `14→19`, `18→20`, `19→20`,
`20→21`), sama dengan isi kolom `Dependency`.

Rantai `PAY-BE-17 → 18 → 20` sengaja berurutan karena ketiganya mengubah
`PayrollCalculationService.cs`; memparalelkannya membuat benturan di berkas yang sama.
`PAY-BE-23` dan `PAY-BE-21` sama-sama menyentuh `PayrollRunsController.cs`, dan `PAY-BE-23`
dengan `PAY-BE-20` sama-sama menyentuh `PayrollRunService.cs`. Ketiganya tidak saling
membutuhkan sehingga bukan dependency, tetapi sebaiknya tidak dikerjakan bersamaan oleh dua
orang.

## Tabel task

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
|---|---|---|---|---|---|---|---|---|---|---|
| `PAY-BE-14` 🟡 | Skema dan konstanta MVP siap | FR-PGJ-001, 030, 035; PRL-DEC-02, 04, 07 | `PGJ-C1` | `IdentityModel`, pola configuration Payroll | Entity baru, 5 kolom, konstanta, opsi, 2 migration | — | Kartu | Build, QBE, review migration | Migration tak sinkron snapshot / Yasmin | Kartu |
| `PAY-BE-15` 🟡 | Struktur di luar rentang grade ditolak | FR-PGJ-010, 011; PRL-DEC-12 | `PGJ-C1` | Controller struktur dan grade | Service aturan, validasi dua controller | — | Kartu | Build, QBE, uji API | Data lama melanggar / Yasmin | Kartu |
| `PAY-BE-16` 🟡 | Baris tunjangan struktur dapat dikelola | FR-PGJ-001–003; PRL-DEC-02, 13 | `PGJ-C1` | `PayrollVersionToken` | Service, controller, DTO baru | `PAY-BE-14` | Kartu | Build, QBE, uji API | Hak akses baru belum diberikan admin / Yasmin | Kartu |
| `PAY-BE-17` 🟡 | Run menghitung tunjangan per jenis dan masa peralihan | FR-PGJ-004–006; PRL-DEC-02, 04 | `PGJ-C1` | Segmen penetapan gaji yang ada | `PayrollCalculationService` | `PAY-BE-16` | Kartu | Build, QBE, uji proses | Hitung ganda dengan profil / Yasmin | Kartu |
| `PAY-BE-18` 🟡 | Run menghitung transport bulanan tetap | FR-PGJ-022–024; PRL-DEC-46 | `PGJ-C1` | `PrlTransportAllowance*` | `PayrollCalculationService` | `PAY-BE-17` | Kartu | Build, QBE, uji proses | Data transport lama ganda / Yasmin | Kartu |
| `PAY-BE-19` 🟡 | Input variabel manual dapat dibuat, diimpor, dan diputuskan | FR-PGJ-030–032, 034, 035; PRL-DEC-07, 31, 38, 43 | `PGJ-C1` | `NumberSeriesAllocator`, `PayrollVersionToken` | Service, controller, DTO baru | `PAY-BE-14` | Kartu | Build, QBE, uji API | Seri nomor belum siap / Yasmin | Kartu |
| `PAY-BE-20` 🟡 | Hanya input disetujui yang dihitung; run dijaga | FR-PGJ-033, 036, 037; PRL-DEC-07, 10 | `PGJ-C1` | Siklus run yang ada | `PayrollCalculationService`, `PayrollRunService` | `PAY-BE-18`, `PAY-BE-19` | Kartu | Build, QBE, uji proses | Regresi sumber input otomatis / Yasmin | Kartu |
| `PAY-BE-21` 🟡 | Simulasi tanpa jejak | FR-PGJ-040–042; PRL-DEC-32 | `PGJ-C1` | `PayrollHrdImportService`, mesin hitung | Service baru, satu endpoint | `PAY-BE-20` | Kartu | Build, QBE, uji runtime | Efek samping `DbContext` terpisah / Yasmin | Kartu |
| `PAY-BE-22` 🟡 | Laporan beban gaji dan PPh/BPJS | FR-PGJ-050–052; PRL-DEC-27, 33, 41 | `PGJ-C1` | `PayrollReportService` | Dua endpoint laporan | — | Kartu | Build, QBE, uji API | Angka tidak cocok rekap / Yasmin | Kartu |
| `PAY-BE-23` 🟡 | Rekap lembur per run | FR-PGJ-060; PRL-DEC-42 | `PGJ-C1` | `PrlPayrollOvertimeInput` | Satu endpoint | — | Kartu | Build, QBE, uji API | — / Yasmin | Kartu |

Epic `OPEN DECISION` (`PGJ-08` s.d. `PGJ-17`) tidak diberi task dan tidak masuk grafik. Daftar
beserta keputusan penahannya ada di `../04-prd-to-mvp.md` bagian 10. Task lama `PAY-BE-13`
(penomoran transport ke NumberSeriesManagement, `PLANNED`) tetap tercatat di
`../../PAYROLL_V2_04_KONTRAK_DAN_TASK_IMPLEMENTASI.md` dan tidak bergantung pada roadmap ini.

## Kartu task

### `PAY-BE-14` 🟡 — Fondasi skema dan konstanta

| Field | Isi |
|---|---|
| Status | 🟡 2026-11-12 — source, migration `20261112090000_AddSalaryStructureAllowanceAndVariableInputDecision`, Designer, dan snapshot selesai; pemeriksaan statis `PASS` (18/18 kolom entity baru cocok model–snapshot–migration, 5/5 kolom input variabel, snapshot +140/−0, resolver `QBE-MOD-002` → `Mst`). Database update diterapkan pemilik 2026-11-12 (`Done.`, `--no-build` Release — assembly memuat migration ini). Belum terpenuhi: keluaran `dotnet build` dan `has-pending-model-changes`, checker QBE resmi (`NOT RUN`, tanpa `pwsh`). Laporan: [`PAY-BE-14.md`](../task/report/backend/PAY-BE-14.md) |
| Outcome | Tabel `MstSalaryStructureAllowance`, lima kolom keputusan di `PrlPayrollVariableInput`, konstanta dan opsi kebijakan tersedia untuk task lain |
| Requirement/decision | FR-PGJ-001, 030, 035; PRL-DEC-02, 04, 07; `02-backend-architecture.md` bagian 4.1, 4.4, 7, 8 |
| Reuse | `IdentityModel`; pola configuration di `Repositories/Configurations/Corporate/...`; registry `Mst` ACTIVE |
| Cakupan | `MstSalaryStructureAllowance.cs` dan configuration-nya (Baru); `PrlPayrollVariableInput.cs` dan configuration-nya (Diperbarui); `ApplicationDbContext.cs` (DbSet); `PayrollStatusCatalog.cs` (`PAY_STRUCTURE_ALLOWANCE`, `PAY_TRANSPORT_ALLOWANCE`, sumber `PayrollManualInput`); `PayrollManualInputTypes.cs` (Baru); `PayrollPolicyOptions.cs` (`AllowProfileAllowanceFallback` bawaan `true`, `SimulationMaxEmployees` bawaan 50); migration `AddSalaryStructureAllowance` dan `AddPayrollVariableInputDecisionColumns` beserta Designer dan snapshot (boleh satu berkas) |
| Dependency | — |
| Acceptance criteria | 1. Kolom, panjang, presisi, index unik parsial, dan `Restrict`/`SetNull` sesuai `data/data-dictionary.md`. 2. Migration hanya menambah, tanpa `DROP` tabel atau kolom lama. 3. Tidak ada perubahan perilaku runtime (konstanta dan opsi belum dipakai). |
| Verifikasi | QBE preflight (`Mst` prefix registry, tanpa `Trx*`); `dotnet build`; `has-pending-model-changes` bersih setelah migration; review SQL hasil migration |
| Risiko/pemilik | Migration bertabrakan dengan dua migration Payroll yang belum dijalankan; urutan timestamp diperiksa. Pemilik: Yasmin |
| DoD | Build hijau, QBE tanpa pelanggaran memblokir, berkas migration ada, laporan `PAY-BE-14.md` ditulis; database update dijalankan pemilik |

### `PAY-BE-15` 🟡 — Batas struktur di dalam grade

| Field | Isi |
|---|---|
| Status | 🟡 2026-11-12 — `PayrollSalaryRangeRuleService` baru, dipanggil `SalaryStructureController` (`POST`, `PUT`) dan `SalaryGradeController` (`PUT`); pemeriksaan statis `PASS` (0 baris baru controller cocok pola `QBE-SVC-001`/`CODE-002/003`/`NAM-001`, 0 pembuatan controller manual, kurung seimbang 4 berkas). Belum terpenuhi: `dotnet build` (`NOT RUN` atas instruksi pemilik), uji API enam langkah, checker QBE resmi. Laporan: [`PAY-BE-15.md`](../task/report/backend/PAY-BE-15.md) |
| Outcome | Struktur dengan batas di luar rentang grade, dan perubahan grade yang membuat struktur aktif keluar batas, ditolak |
| Requirement/decision | FR-PGJ-010, 011; PRL-DEC-12; `validation-matrix.md` `SR-01`–`SR-04` |
| Reuse | `SalaryStructureController`, `SalaryGradeController` (legacy, `TOUCHED LEGACY`) |
| Cakupan | `PayrollSalaryRangeRuleService.cs` (Baru); panggilan service pada simpan dan ubah struktur serta ubah grade; `Program.cs` (`AddScoped`) |
| Dependency | — |
| Acceptance criteria | 1. Grade Rp 6.000.000–9.000.000, struktur batas atas Rp 10.000.000 ditolak 400 `SR-02`. 2. Grade dipersempit ke Rp 8.500.000 saat ada struktur aktif berbatas Rp 9.000.000 ditolak 409 `SR-04` dengan kode struktur. 3. Simpan di dalam rentang tetap berhasil. 4. Tidak ada baris akses `ApplicationDbContext` baru di controller (`QBE-SVC-001`). |
| Verifikasi | QBE; build; uji API manual lewat Swagger; pemilik menjalankan pra-cek SQL di `02-backend-architecture.md` bagian 8 sebelum rilis |
| Risiko/pemilik | Struktur lama yang sudah melanggar baru ketahuan saat diubah. Pemilik: Yasmin |
| DoD | Build hijau, QBE bersih, empat kriteria terbukti, laporan ditulis |

### `PAY-BE-16` 🟡 — Kelola baris tunjangan struktur

| Field | Isi |
|---|---|
| Status | 🟡 2026-11-12 — service, controller (`SalaryStructureAllowance`, 4 endpoint), dan DTO baru; pemeriksaan statis `PASS` (4/4 endpoint atribut akses konsisten, 0 baris controller cocok pola QBE, 0 bentrok rute, kurung seimbang 4 berkas). Belum terpenuhi: `dotnet build` (`NOT RUN` atas instruksi pemilik), uji API sembilan langkah, checker QBE resmi; pesan `SA-07` berselisih bunyi dengan kontrak. Laporan: [`PAY-BE-16.md`](../task/report/backend/PAY-BE-16.md) |
| Outcome | Petugas dapat melihat, menambah, mengubah, menghapus tunjangan tetap per struktur |
| Requirement/decision | FR-PGJ-001–003; PRL-DEC-02, 13; `api-contract.md` grup Salary Structure Allowance |
| Reuse | `PayrollVersionToken`; pola controller Payroll V2 (`[AccessController]`, `[AccessAction]`, `[AccessPermission]`) |
| Cakupan | `PayrollSalaryStructureAllowanceService.cs`, `PayrollSalaryStructureAllowancesController.cs`, `PayrollSalaryStructureAllowanceDtos.cs` (Baru); `Program.cs` |
| Dependency | `PAY-BE-14` |
| Acceptance criteria | 1. Empat endpoint sesuai kontrak dengan `ControllerName = "SalaryStructureAllowance"`. 2. `SA-01`–`SA-07` berlaku, termasuk tolak jenis ganda (409) dan jenis yang dipakai kebijakan Shift/Siaga/Risiko aktif (400). 3. Hapus hanya menandai `IsDelete`. 4. Log aksi tulis tanpa nominal. |
| Verifikasi | QBE; build; uji API manual per kode status |
| Risiko/pemilik | Admin perlu memberi hak `SalaryStructureAllowance` di Akses Role. Pemilik: Yasmin |
| DoD | Build hijau, QBE bersih, kriteria terbukti, laporan ditulis |

### `PAY-BE-17` 🟡 — Hitung tunjangan dari struktur dan masa peralihan

| Field | Isi |
|---|---|
| Status | 🟡 2026-11-12 — `PayrollCalculationService` menghitung `PAY_STRUCTURE_ALLOWANCE` per baris struktur dengan cadangan profil (`CALC-03`) dan profil diabaikan (`CALC-04`); simulasi tiga contoh kontrak `PASS` (Rp 1.550.000 / Rp 1.025.000 / Rp 1.650.000), kurung seimbang, peringatan terbukti tidak menahan run. Belum terpenuhi: `dotnet build` (`NOT RUN` atas instruksi pemilik), uji proses di DB uji, checker QBE resmi. Laporan: [`PAY-BE-17.md`](../task/report/backend/PAY-BE-17.md) |
| Outcome | Run menghasilkan satu baris per jenis tunjangan dari struktur, dengan cadangan angka profil selama peralihan |
| Requirement/decision | FR-PGJ-004–006; PRL-DEC-02, 04; `02-backend-architecture.md` bagian 5.1–5.2 |
| Reuse | Loop segmen penetapan gaji dan `UpsertComponent` di `PayrollCalculationService` |
| Cakupan | `PayrollCalculationService.cs` |
| Dependency | `PAY-BE-16` |
| Acceptance criteria | 1. Contoh tabel bagian 5.1 menghasilkan angka persis (Rp 1.550.000 penuh; Rp 1.025.000 masuk tanggal 16; keahlian dipotong ke Rp 400.000). 2. Struktur kosong dan saklar menyala: baris "Tunjangan Tetap" dari profil dan `CALC-03`. 3. Struktur berisi dan profil berisi: profil diabaikan, `CALC-04`, tanpa penjumlahan ganda. 4. Pajak dan prorata mengikuti jenis tunjangan. |
| Verifikasi | QBE; build; uji proses pada DB uji dengan tiga kasus di atas |
| Risiko/pemilik | Total gaji pegawai berubah saat struktur diisi; rekonsiliasi angka lama (DEC-PRL-015). Pemilik: Yasmin |
| DoD | Build hijau, QBE bersih, empat kriteria terbukti di DB uji, laporan ditulis |

### `PAY-BE-18` 🟡 — Hitung transport bulanan tetap

| Field | Isi |
|---|---|
| Status | 🟡 2026-11-12 — `PayrollCalculationService` menghitung `PAY_TRANSPORT_ALLOWANCE` dari tunjangan transport `FixedMonthly` (batas `MonthlyLimitAmount`, prorata hari berlaku, pajak `IsTaxable`), `CALC-01` untuk tunjangan bulanan beririsan, `CALC-02` untuk metode lain; simulasi 4/4 kriteria dan 4/4 kasus tepi `PASS` (kriteria 1 menghasilkan Rp 266.666,67 menurut pembulatan 2 desimal mesin yang ada), kurung seimbang. Belum terpenuhi: `dotnet build` (`NOT RUN` atas instruksi pemilik), uji proses di DB uji, checker QBE resmi. Laporan: [`PAY-BE-18.md`](../task/report/backend/PAY-BE-18.md) |
| Outcome | Run menghasilkan baris transport untuk metode bulanan tetap |
| Requirement/decision | FR-PGJ-022–024; PRL-DEC-46; `02-backend-architecture.md` bagian 5.3 |
| Reuse | `PrlTransportAllowance`, `PrlTransportAllowancePolicy` |
| Cakupan | `PayrollCalculationService.cs` |
| Dependency | `PAY-BE-17` |
| Acceptance criteria | 1. Rp 400.000 diprorata 20 dari 30 hari = Rp 266.667. 2. `MonthlyLimitAmount` membatasi. 3. Dua tunjangan bulanan beririsan: pegawai gagal `CALC-01`, pegawai lain terhitung. 4. Metode lain: peringatan `CALC-02`, tanpa baris. |
| Verifikasi | QBE; build; uji proses di DB uji |
| Risiko/pemilik | Data transport lama yang beririsan menggagalkan pegawai. Pemilik: Yasmin |
| DoD | Build hijau, QBE bersih, kriteria terbukti, laporan ditulis |

### `PAY-BE-19` 🟡 — Input variabel manual

| Field | Isi |
|---|---|
| Status | 🟡 2026-11-12 — `PayrollVariableInputService`, `PayrollVariableInputsController` (`PayrollVariableInput`, 11 endpoint, 8 action), dan DTO baru; pemeriksaan statis `PASS` (11/11 endpoint atribut akses konsisten, matriks transisi 6 status × 4 aksi 0 selisih, contoh impor 120 baris → 400 `VI-08` tanpa baris tersimpan, kurung seimbang 4 berkas, 0 akses context di controller). Belum terpenuhi: `dotnet build` (`NOT RUN` atas instruksi pemilik), uji API sepuluh langkah, checker QBE resmi. Delta: `VI-01` memakai 400 (kontrak transisi menyebut 409). Risiko sela (input `Submitted` ikut dihitung) ditutup di source oleh `PAY-BE-20` 🟡 pada 2026-11-12. Laporan: [`PAY-BE-19.md`](../task/report/backend/PAY-BE-19.md) |
| Outcome | Petugas dapat membuat, mengimpor, mengubah, mengajukan, menyetujui, menolak, dan membatalkan insentif, bonus, dan honor tugas tambahan |
| Requirement/decision | FR-PGJ-030–032, 034, 035; PRL-DEC-07, 31, 38, 43; `api-contract.md` grup Payroll Variable Input; `state-transition-matrix.md` bagian 1 |
| Reuse | `NumberSeriesAllocator.AllocateAsync` (pola `FinancePayrollDeductionBatchService`); `PayrollVersionToken`; `PayrollRunStatuses.AcceptsInputs` |
| Cakupan | `PayrollVariableInputService.cs`, `PayrollVariableInputsController.cs`, `PayrollVariableInputDtos.cs` (Baru); `Program.cs` |
| Dependency | `PAY-BE-14` |
| Acceptance criteria | 1. Sebelas endpoint sesuai kontrak dengan `ControllerName = "PayrollVariableInput"`. 2. Transisi sah dan tidak sah sesuai matriks; aksi ke status yang sama idempoten. 3. Impor 120 baris dengan 2 baris salah: 400 `VI-08`, tidak ada baris tersimpan. 4. Nomor `PVI-...` dari number series, bukan Count/Max+1. 5. Pengaju boleh menyetujui inputnya sendiri. 6. `SourceType = PayrollManualInput`. |
| Verifikasi | QBE (`QBE-CODE-002`, `QBE-SVC-001`); build; uji API manual |
| Risiko/pemilik | Apakah seri nomor perlu didaftarkan lebih dulu di NumberSeriesManagement diperiksa saat eksekusi. Pemilik: Yasmin |
| DoD | Build hijau, QBE bersih, kriteria terbukti, laporan ditulis |

### `PAY-BE-20` 🟡 — Aturan hitung input manual dan penjaga run

| Field | Isi |
|---|---|
| Status | 🟡 2026-11-12 — `PayrollCalculationService` menghitung input manual hanya bila `Approved`/`Applied` (sumber otomatis tidak berubah); `PayrollRunService` menolak pengajuan run dengan 409 `VI-10`/`VI-11`, memindah input `Approved` → `Applied` saat disahkan dan belum `Applied` → `Cancelled` ("Run dibatalkan") saat dibatalkan, di transaksi yang sama; simulasi 5/5 kriteria `PASS` (11 kombinasi sumber×status hitung, 2 penjaga, 2 perpindahan), kurung seimbang 2 berkas. Belum terpenuhi: `dotnet build` (`NOT RUN` atas instruksi pemilik), uji proses di DB uji termasuk regresi input otomatis, checker QBE resmi. Laporan: [`PAY-BE-20.md`](../task/report/backend/PAY-BE-20.md) |
| Outcome | Hanya input manual yang disetujui yang dihitung; run tidak dapat diajukan dengan input menunggu atau basi; status input mengikuti run |
| Requirement/decision | FR-PGJ-033, 036, 037; PRL-DEC-07, 10; `state-transition-matrix.md` bagian 3 |
| Reuse | `CountableInputStatuses`, aksi submit/approve/cancel run yang ada |
| Cakupan | `PayrollCalculationService.cs`, `PayrollRunService.cs` |
| Dependency | `PAY-BE-18`, `PAY-BE-19` |
| Acceptance criteria | 1. Input manual `Submitted` tidak dihitung; input jasa medis, cicilan, cuti, AR tetap dihitung seperti sebelumnya. 2. Ajukan run dengan input `Submitted`: 409 `VI-10`. 3. Input berubah setelah `CalculatedAt`: 409 `VI-11`. 4. Run disahkan: input `Approved` → `Applied` dalam transaksi yang sama. 5. Run dibatalkan: input belum `Applied` → `Cancelled` beralasan "Run dibatalkan". |
| Verifikasi | QBE; build; uji proses termasuk satu run berisi input otomatis (regresi) |
| Risiko/pemilik | Regresi pada sumber input otomatis. Pemilik: Yasmin |
| DoD | Build hijau, QBE bersih, lima kriteria terbukti, laporan ditulis |

### `PAY-BE-21` 🟡 — Simulasi

| Field | Isi |
|---|---|
| Status | 🟡 2026-11-12 — `PayrollSimulationService` dan `POST /simulations` (`PayrollRun : Simulate`) ditulis; run sementara `Simulation` di transaksi yang selalu di-rollback; `SIM-01`–`SIM-03` dan batas `SimulationMaxEmployees`. **Cakupan dipersempit atas keputusan pemilik 2026-11-12:** tanpa impor HR (ketiga service HR membuka transaksi sendiri dan mengunci absensi), tanpa input variabel dan cicilan; kriteria 1 hanya untuk komponen di luar sumber itu. Pemeriksaan statis `PASS` (0 rujukan number series/Finance/impor HR di jalur hitung, 21/21 atribut akses controller run konsisten, kurung seimbang 4 berkas). Belum terpenuhi: `dotnet build` (`NOT RUN` atas instruksi pemilik), uji runtime hitung baris sebelum/sesudah, checker QBE resmi, revisi bagian 5.5 dan kontrak respons (delta `notes`, `employeeNumber`). Laporan: [`PAY-BE-21.md`](../task/report/backend/PAY-BE-21.md) |
| Outcome | Petugas melihat hasil hitung percobaan tanpa ada data tersisa |
| Requirement/decision | FR-PGJ-040–042; PRL-DEC-32; `02-backend-architecture.md` bagian 5.5 |
| Reuse | `PayrollHrdImportService`, `PayrollCalculationService` |
| Cakupan | `PayrollSimulationService.cs`, `PayrollSimulationDtos.cs` (Baru); `PayrollRunsController.cs` (`POST /simulations`, action `Simulate`); `Program.cs` |
| Dependency | `PAY-BE-20` |
| Acceptance criteria | 1. Hasil per pegawai sama dengan hasil run resmi untuk data yang sama. 2. Jumlah baris `PrlPayrollRun`, `PrlPayrollVariableInput`, dan status periode sama sebelum dan sesudah simulasi. 3. Lebih dari batas pegawai: 400 `SIM-02`. 4. Tidak ada pemanggilan number series atau sinkronisasi Finance pada jalur simulasi. |
| Verifikasi | QBE; build; uji runtime di DB uji dengan hitung baris sebelum dan sesudah |
| Risiko/pemilik | Service dengan `DbContext` terpisah di jalur impor meninggalkan data; wajib ditelusuri saat eksekusi. Pemilik: Yasmin |
| DoD | Build hijau, QBE bersih, empat kriteria terbukti, laporan ditulis |

### `PAY-BE-22` 🟡 — Laporan beban gaji dan PPh/BPJS

| Field | Isi |
|---|---|
| Status | 🟡 2026-11-12 — `GET /reports/expense-summary` dan `GET /reports/tax-contribution` (`PayrollReport : ViewAmount`) ditulis di `PayrollReportService`; hanya run disahkan (`Approved` s.d. `Closed`, tanpa `Reversed`); `RPT-01` pada keduanya. Pemeriksaan `PASS`: simulasi 3 pegawai/2 Cost Center total penghasilan Rp 19.200.000 dan potongan Rp 1.500.000 sama dengan total pegawai run, 4/4 atribut akses controller laporan konsisten, kurung seimbang 3 berkas. Belum terpenuhi: `dotnet build` (`NOT RUN` atas instruksi pemilik), uji API dibandingkan rekap, checker QBE resmi. Delta: respons PPh/BPJS memakai `PagedResult` + `note`/total, baris menambah `runNumber`. Laporan: [`PAY-BE-22.md`](../task/report/backend/PAY-BE-22.md) |
| Outcome | Finance mendapat beban gaji per Cost Center per komponen dan laporan PPh/BPJS tetapan profil |
| Requirement/decision | FR-PGJ-050–052; PRL-DEC-27, 33, 41 |
| Reuse | `PayrollReportService` (pengelompokan cost center yang ada) |
| Cakupan | `PayrollReportService.cs`, `PayrollReportsController.cs`, `PayrollReportDtos.cs` |
| Dependency | — |
| Acceptance criteria | 1. `GET /expense-summary` hanya dari run yang disahkan; jumlah seluruh baris = total penghasilan dan potongan run. 2. `GET /tax-contribution` menampilkan `PAY_TAX` dan `PAY_INSURANCE_EMPLOYEE` dengan sumber `TETAPAN_PROFIL`. 3. Keduanya `PayrollReport : ViewAmount`. 4. Tanpa run disahkan: hasil kosong dengan keterangan `RPT-01`. |
| Verifikasi | QBE; build; uji API dibandingkan rekap yang ada |
| Risiko/pemilik | — / Yasmin |
| DoD | Build hijau, QBE bersih, kriteria terbukti, laporan ditulis |

### `PAY-BE-23` 🟡 — Rekap lembur per run

| Field | Isi |
|---|---|
| Status | 🟡 2026-11-12 — `GET /payroll-runs/{id}/overtime-inputs` (`PayrollRun : Read`) dan `GetOvertimeInputsAsync` ditulis; saringan sama dengan mesin hitung, pencarian nama/nomor, paginasi maks 200, run tanpa lembur → daftar kosong. Pemeriksaan statis `PASS` (22/22 atribut akses controller run konsisten, satu rute `overtime-inputs`, kurung seimbang 3 berkas). Belum terpenuhi: `dotnet build` (`NOT RUN` atas instruksi pemilik), uji API, checker QBE resmi. Temuan: `Read` lebih longgar daripada `ReadEmployeeDetail` pada detail pegawai; menunggu keputusan pemilik. Laporan: [`PAY-BE-23.md`](../task/report/backend/PAY-BE-23.md) |
| Outcome | Petugas melihat lembur terverifikasi yang diimpor ke run |
| Requirement/decision | FR-PGJ-060; PRL-DEC-42 |
| Reuse | `PrlPayrollOvertimeInput` |
| Cakupan | `PayrollRunService.cs` (query), `PayrollRunsController.cs` (`GET /{id}/overtime-inputs`), `PayrollRunDtos.cs` |
| Dependency | — |
| Acceptance criteria | 1. Baris sama dengan lembur yang diimpor, dengan paginasi dan pencarian. 2. Hak `PayrollRun : Read`. 3. Run tanpa lembur: daftar kosong. |
| Verifikasi | QBE; build; uji API |
| Risiko/pemilik | — / Yasmin |
| DoD | Build hijau, QBE bersih, kriteria terbukti, laporan ditulis |
