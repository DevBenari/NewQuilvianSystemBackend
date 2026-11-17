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
PAY-FE-14 ✅

PAY-FE-15 ✅

PAY-BE-15 🟡 [BE] ─┬─> PAY-FE-16 ✅
                   │
PAY-BE-16 🟡 [BE] ─┘

PAY-BE-19 🟡 [BE] ─┬─> PAY-FE-17 ✅ ─┬─> PAY-FE-19 ✅
                   │                 │
PAY-BE-20 🟡 [BE] ─┘                 │
                                     │
PAY-BE-23 🟡 [BE] ───────────────────┘

PAY-BE-21 🟡 [BE] ─> PAY-FE-18 ✅

PAY-BE-22 🟡 [BE] ─> PAY-FE-20 ✅
```

`[BE]` = task backend pada `backend-roadmap.md`, cermin baca-saja; tandanya disalin dari sana.

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | — (endpoint transport sudah ada) | `PAY-FE-14` 🟡, `PAY-FE-15` 🟡 — boleh paralel, bisa mulai sekarang |
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
| `PAY-FE-14` ✅ | Kebijakan transport dapat diatur | FR-PGJ-020; PRL-DEC-46 | `PGJ-C1` | 9 endpoint `TransportAllowancePolicy`; standar master data | `FE-PGJ-09` + butir menu | — | Kartu | Lint, build, uji manual | — / Yasmin | Kartu |
| `PAY-FE-15` ✅ | Transport pegawai dapat diatur | FR-PGJ-021 | `PGJ-C1` | Endpoint `WorkforceTransportAllowance` | `FE-PGJ-10` + tombol masuk | — | Kartu | Lint, build, uji manual | — / Yasmin | Kartu |
| `PAY-FE-16` ✅ | Tunjangan tetap struktur dapat dikelola | FR-PGJ-001–003, 010, 011 | `PGJ-C1` | Detail struktur yang ada | `FE-PGJ-01` | `PAY-BE-15`, `PAY-BE-16` | Kartu | Lint, build, uji manual | — / Yasmin | Kartu |
| `PAY-FE-17` ✅ | Penghasilan variabel dikelola per run | FR-PGJ-030–036 | `PGJ-C1` | Detail run yang ada | `FE-PGJ-02`, `FE-PGJ-03` + tombol masuk | `PAY-BE-19`, `PAY-BE-20` | Kartu | Lint, build, uji manual | Impor besar / Yasmin | Kartu |
| `PAY-FE-18` ✅ | Simulasi dapat dijalankan | FR-PGJ-040–042 | `PGJ-C1` | Opsi periode yang ada | `FE-PGJ-04` + butir menu | `PAY-BE-21` | Kartu | Lint, build, uji manual | — / Yasmin | Kartu |
| `PAY-FE-19` ✅ | Rekap dan tarif lembur terbaca | FR-PGJ-060, 061 | `PGJ-C1` | Endpoint `OvertimeRate` HR | `FE-PGJ-05`, `FE-PGJ-06` + grup menu Lembur | `PAY-FE-17`, `PAY-BE-23` | Kartu | Lint, build, uji manual | — / Yasmin | Kartu |
| `PAY-FE-20` ✅ | Laporan beban gaji dan PPh/BPJS terbaca | FR-PGJ-050–052 | `PGJ-C1` | Pola layar laporan yang ada | `FE-PGJ-07`, `FE-PGJ-08` + dua butir menu | `PAY-BE-22` | Kartu | Lint, build, uji manual | — / Yasmin | Kartu |

## Kartu task

### `PAY-FE-14` ✅ — Kebijakan Tunjangan Transportasi

| Field | Isi |
|---|---|
| Status | ✅ 2026-11-12 — fitur master data `transport-allowance-policy` (7 berkas source + 5 route) ditulis mengikuti standar master data dengan rujukan `hazard-allowance-policy`; reducer dan butir menu "Tunjangan Transportasi" (`TransportAllowancePolicy : Read`) terdaftar; tombol Tambah/Perbarui/Hapus dijaga `Create`/`Update`/`Delete`; keterangan `CALC-02` di form dan detail (`InformationAlert` lewat `afterHero`, `UI GATE` 0 NEW). ESLint 0 galat (2 peringatan warisan rujukan); uji unit Payroll lolos; suite penuh 4.753/4.800 lolos, 47 gagal tanpa yang baru (2 berkas menu dibuktikan gagal juga pada HEAD); skrip pemeriksa utils `PASS`. Belum terpenuhi: `npm run build` (`NOT RUN` atas instruksi pemilik), uji manual langkah 1–8 (`MANUAL TEST: NOT FEASIBLE` oleh agen). **Naik ke ✅ 2026-11-13 atas instruksi pemilik:** pemilik melaporkan `npm run dev` berhasil dan seluruh uji lulus (laporan lisan, tanpa rincian per langkah, tanpa screenshot tersimpan; bukan bukti agen). Pernyataan "belum terpenuhi" dan "belum dibuktikan" di atas berlaku saat ditulis; yang tetap berlaku: `npm run build` `NOT RUN`. Laporan: [`PAY-FE-14.md`](../task/report/frontend/PAY-FE-14.md) |
| Outcome | Petugas dapat mengelola kebijakan transport dari menu Payroll |
| Requirement/decision | FR-PGJ-020; PRL-DEC-46; `03-frontend-architecture.md` bagian 3.6 |
| Reuse | Sembilan endpoint `TransportAllowancePolicy` yang sudah ada; standar fitur master data frontend (rujukan `hr/master-data/job-level`) |
| Cakupan | Layar `FE-PGJ-09` di `/payroll/master-data/transport-allowance-policy`; butir menu di bawah "Kebijakan Tunjangan" |
| Dependency | — |
| Acceptance criteria | 1. Daftar, detail, tambah, ubah, status, hapus berjalan. 2. Cara hitung selain "Bulanan tetap" menampilkan keterangan "Metode ini belum dihitung otomatis di run. Gunakan Penghasilan Variabel." 3. Butir menu dijaga `TransportAllowancePolicy : Read`; tombol tulis dijaga `Create`, `Update`, `Delete`. 4. Empat keadaan (memuat, kosong, gagal, berisi) ada. |
| Verifikasi | ESLint pada berkas yang diubah; build; checklist konsistensi UI; uji manual langkah 1–4 |
| Risiko/pemilik | Controller transport memakai pengecualian `QBE-CODE-002` bertenggat (`PAY-BE-13`); tidak mempengaruhi layar. Pemilik: Yasmin |
| DoD | Kriteria terbukti, butir menu terdaftar, laporan ditulis |

### `PAY-FE-15` ✅ — Tunjangan Transportasi pegawai

| Field | Isi |
|---|---|
| Status | ✅ 2026-11-12 — layar anak `/payroll/workforce-compensations/[id]/transport-allowance` (daftar, ringkasan, modal tambah/ubah, hapus) dan tombol "Tunjangan Transportasi" di detail Profil Penggajian ditulis dengan pola Payroll V2; peringatan `CALC-01` tampil di form sebelum simpan dan di atas daftar untuk data lama yang ganda; tombol dijaga `WorkforceTransportAllowance : Read/Create/Update/Delete`; `UI GATE` seluruhnya `REUSE`. ESLint 0 galat (3 peringatan pola Payroll V2); suite unit 4.753/4.800 lolos, 47 kegagalan identik dengan baseline `PAY-FE-14`; skrip pemeriksa irisan dan payload `PASS`. Selisih: backend menolak tunjangan kedua per pegawai, jadi tombol Tambah dinonaktifkan dengan penjelasan. Belum terpenuhi: `npm run build` (`NOT RUN` atas instruksi pemilik), uji manual langkah 1–8 (`MANUAL TEST: NOT FEASIBLE` oleh agen). **Naik ke ✅ 2026-11-13 atas instruksi pemilik:** pemilik melaporkan `npm run dev` berhasil dan seluruh uji lulus (laporan lisan, tanpa rincian per langkah, tanpa screenshot tersimpan; bukan bukti agen). Pernyataan "belum terpenuhi" dan "belum dibuktikan" di atas berlaku saat ditulis; yang tetap berlaku: `npm run build` `NOT RUN`. Laporan: [`PAY-FE-15.md`](../task/report/frontend/PAY-FE-15.md) |
| Outcome | Petugas dapat menetapkan tunjangan transport per pegawai |
| Requirement/decision | FR-PGJ-021; `03-frontend-architecture.md` bagian 3.6 |
| Reuse | Endpoint `WorkforceTransportAllowance`; detail Profil Penggajian `/payroll/workforce-compensations/[id]` |
| Cakupan | Layar `FE-PGJ-10` di `/payroll/workforce-compensations/[id]/transport-allowance`; tombol "Tunjangan Transportasi" di detail profil |
| Dependency | — |
| Acceptance criteria | 1. Daftar dan form berjalan. 2. Peringatan tampil sebelum menyimpan bila pegawai sudah punya transport bulanan aktif yang beririsan. 3. Tombol dijaga `WorkforceTransportAllowance : *`. |
| Verifikasi | ESLint; build; uji manual |
| Risiko/pemilik | — / Yasmin |
| DoD | Kriteria terbukti, jalan masuk ada, laporan ditulis |

### `PAY-FE-16` ✅ — Tunjangan tetap pada Struktur Gaji

| Field | Isi |
|---|---|
| Status | ✅ 2026-11-13 — bagian "Tunjangan Tetap" (daftar, modal tambah/ubah, hapus, keadaan memuat/kosong/gagal) dipasang di detail Struktur Gaji; pilihan Jenis Tunjangan dari daftar bercari (`UI GATE` 8 REUSE, 1 COMPOSE, 0 NEW); pesan `SA-01`–`SA-04`, `SA-06` menempel di isian, `SR-01`–`SR-03` di form struktur dan `SR-04` di form grade; benturan 409 memuat ulang daftar. 5 dari 5 kriteria terpetakan ke source dan 5 dari 5 dinyatakan lulus oleh pemilik di peramban (2026-11-13, tanpa rincian per langkah). ESLint 0 galat (4 peringatan `set-state-in-effect`, 3 warisan); 27 uji unit baru lulus (termasuk pencocokan ke source backend); suite penuh 4.827 uji, 51 gagal, seluruhnya juga gagal pada `HEAD` murni (0 kegagalan baru); `check-css-usage.mjs` 0 temuan. Belum terpenuhi: `npm run build` (`NOT RUN`), pemeriksaan visual dan uji manual langkah 1–11 (`NOT FEASIBLE` oleh agen); prasyarat `PAY-BE-15` dan `PAY-BE-16` masih 🟡 (dikerjakan atas instruksi pemilik). Delta: rentang grade pada wilayah Kepala belum ada di respons detail struktur. **Naik ke ✅ 2026-11-13 atas instruksi pemilik:** pemilik melaporkan `npm run dev` berhasil dan seluruh uji lulus (laporan lisan, tanpa rincian per langkah, tanpa screenshot tersimpan; bukan bukti agen). Pernyataan "belum terpenuhi" dan "belum dibuktikan" di atas berlaku saat ditulis; yang tetap berlaku: `npm run build` `NOT RUN`; prasyarat backend `PAY-BE-15` dan `PAY-BE-16` masih 🟡 (belum terbukti build dan uji API). Laporan: [`PAY-FE-16.md`](../task/report/frontend/PAY-FE-16.md) |
| Outcome | Petugas mengelola tunjangan per jenis dan melihat penolakan batas grade |
| Requirement/decision | FR-PGJ-001–003, 010, 011; `03-frontend-architecture.md` bagian 3.1 |
| Reuse | Detail Struktur Gaji yang sudah ada di `/payroll/master-data/salary-structure/[slug]` |
| Cakupan | Bagian Tunjangan Tetap; pesan `SR-01`–`SR-04` pada form struktur dan grade |
| Dependency | `PAY-BE-15`, `PAY-BE-16` |
| Acceptance criteria | 1. Daftar tunjangan menampilkan jenis, cara hitung, nilai, batas maksimum, pajak, prorata. 2. Keadaan kosong berbunyi persis sesuai skema. 3. Tombol dijaga `SalaryStructureAllowance : Create/Update/Delete`. 4. Pesan penolakan server tampil di dekat isian. 5. Benturan 409 memuat ulang data. |
| Verifikasi | ESLint; build; uji manual dengan contoh struktur GR-05 |
| Risiko/pemilik | — / Yasmin |
| DoD | Kriteria terbukti, laporan ditulis |

### `PAY-FE-17` ✅ — Penghasilan Variabel per run

| Field | Isi |
|---|---|
| Status | ✅ 2026-11-13 — layar `/payroll/payroll-runs/[id]/variable-inputs` ditulis: daftar berhalaman dengan saringan, form tambah/ubah, impor CSV (semua atau tidak sama sekali, daftar baris gagal), aksi per baris menurut `availableActions` dan hak (ajukan, setujui, tolak dan batalkan dengan alasan), aksi massal dengan hasil per baris, peringatan hitung ulang atas seluruh input run, layar baca-saja pada run yang tidak menerima input; tombol "Penghasilan Variabel" di detail run (`UI GATE` 12 REUSE, 2 COMPOSE, 0 NEW). 6 dari 6 kriteria terpetakan ke source dan 6 dari 6 dinyatakan lulus oleh pemilik di peramban (2026-11-13, tanpa rincian per langkah). ESLint 0 galat (3 peringatan `set-state-in-effect`, 2 warisan); 34 uji unit baru lulus (termasuk pencocokan ke source backend); suite penuh 4.861 uji, 51 gagal, himpunan identik dengan sebelum task (0 kegagalan baru); `check-css-usage.mjs` 0 temuan. Belum terpenuhi: `npm run build` (`NOT RUN`), pemeriksaan visual dan uji manual langkah 1–13 (`NOT FEASIBLE` oleh agen); prasyarat `PAY-BE-19` dan `PAY-BE-20` masih 🟡 (dikerjakan atas instruksi pemilik). Delta: respons input tidak membawa waktu perubahan, sehingga peringatan membaca token versi (dikunci uji terhadap source backend); format berkas impor CSV dipilih agen. **Naik ke ✅ 2026-11-13 atas instruksi pemilik:** pemilik melaporkan `npm run dev` berhasil dan seluruh uji lulus (laporan lisan, tanpa rincian per langkah, tanpa screenshot tersimpan; bukan bukti agen). Pernyataan "belum terpenuhi" dan "belum dibuktikan" di atas berlaku saat ditulis; yang tetap berlaku: `npm run build` `NOT RUN`; prasyarat backend `PAY-BE-19` dan `PAY-BE-20` masih 🟡 (belum terbukti build dan uji API). Laporan: [`PAY-FE-17.md`](../task/report/frontend/PAY-FE-17.md) |
| Outcome | Petugas memasukkan, mengimpor, dan memutuskan insentif, bonus, dan honor dari detail run |
| Requirement/decision | FR-PGJ-030–036; `03-frontend-architecture.md` bagian 3.2; `state-transition-matrix.md` bagian 1 |
| Reuse | Detail run `/payroll/payroll-runs/[id]` |
| Cakupan | `FE-PGJ-02` dan `FE-PGJ-03` di `/payroll/payroll-runs/[id]/variable-inputs`; tombol "Penghasilan Variabel" di detail run |
| Dependency | `PAY-BE-19`, `PAY-BE-20` |
| Acceptance criteria | 1. Tombol aksi mengikuti `availableActions` dan hak akses. 2. Impor dengan baris salah menampilkan daftar baris dan sebabnya; tidak ada baris tersimpan. 3. Aksi massal menampilkan hasil per baris. 4. Peringatan hitung ulang tampil bila input berubah setelah `calculatedAt`. 5. Run yang tidak menerima input menjadikan layar baca-saja. 6. Tombol dinonaktifkan selama permintaan berjalan. |
| Verifikasi | ESLint; build; uji manual skenario `UAT-11`–`UAT-14` |
| Risiko/pemilik | Impor ratusan baris perlu umpan balik progres (`DEV_DISCRETION`). Pemilik: Yasmin |
| DoD | Kriteria terbukti, jalan masuk ada, laporan ditulis |

### `PAY-FE-18` ✅ — Simulasi Penggajian

| Field | Isi |
|---|---|
| Status | ✅ 2026-11-13 — layar `/payroll/simulations` ditulis: pilihan periode (opsi periode yang ada), pilihan pegawai opsional dari daftar Profil Penggajian, tombol Jalankan dengan keadaan menghitung, catatan yang tidak ikut disimulasikan, ringkasan, tabel per pegawai dengan lencana galat dan peringatan, serta rincian komponen; kalimat "Hasil simulasi tidak tersimpan dan tidak mempengaruhi run resmi." tanpa kondisi; penolakan `SIM-01`–`SIM-03` tampil apa adanya; butir menu "Simulasi Penggajian" dijaga `PayrollRun : Simulate` (`UI GATE` 8 REUSE, 2 COMPOSE, 0 NEW). 4 dari 4 kriteria terpetakan ke source dan 4 dari 4 dinyatakan lulus oleh pemilik di peramban (2026-11-13, tanpa rincian per langkah). ESLint 0 galat (1 peringatan `set-state-in-effect`); 13 uji unit baru lulus (termasuk pencocokan ke source backend); suite penuh 4.874 uji, 51 gagal, himpunan identik dengan sebelum task (0 kegagalan baru); `check-css-usage.mjs` 0 temuan. Belum terpenuhi: `npm run build` (`NOT RUN`), pemeriksaan visual dan uji manual langkah 1–10 (`NOT FEASIBLE` oleh agen); prasyarat `PAY-BE-21` masih 🟡 (dikerjakan atas instruksi pemilik). Delta: sumber daftar pegawai dipilih agen (Profil Penggajian, butuh `PayrollCompensation : Read`); batas "maks 50" tidak diblokir di layar karena ditegakkan server (`SIM-02`); hasil simulasi lebih sempit dari skema (tanpa impor HR, penghasilan variabel, dan cicilan) atas keputusan pemilik pada `PAY-BE-21`. **Naik ke ✅ 2026-11-13 atas instruksi pemilik:** pemilik melaporkan `npm run dev` berhasil dan seluruh uji lulus (laporan lisan, tanpa rincian per langkah, tanpa screenshot tersimpan; bukan bukti agen). Pernyataan "belum terpenuhi" dan "belum dibuktikan" di atas berlaku saat ditulis; yang tetap berlaku: `npm run build` `NOT RUN`; prasyarat backend `PAY-BE-21` masih 🟡 (belum terbukti build dan uji runtime). Laporan: [`PAY-FE-18.md`](../task/report/frontend/PAY-FE-18.md) |
| Outcome | Petugas menjalankan simulasi dan membaca hasil per pegawai |
| Requirement/decision | FR-PGJ-040–042; `03-frontend-architecture.md` bagian 3.3 |
| Reuse | `GET /payroll-runs/period-options` (`PayrollRun : ReadPeriods`) |
| Cakupan | `FE-PGJ-04` di `/payroll/simulations`; butir menu di "Proses Penggajian" |
| Dependency | `PAY-BE-21` |
| Acceptance criteria | 1. Kalimat "Hasil simulasi tidak tersimpan dan tidak mempengaruhi run resmi." selalu tampil. 2. Ringkasan dan rincian per pegawai termasuk peringatan dan galat. 3. Penolakan `SIM-01`–`SIM-03` tampil apa adanya. 4. Butir menu dijaga `PayrollRun : Simulate`. |
| Verifikasi | ESLint; build; uji manual `UAT-16`, `UAT-17` |
| Risiko/pemilik | — / Yasmin |
| DoD | Kriteria terbukti, butir menu terdaftar, laporan ditulis |

### `PAY-FE-19` ✅ — Rekap Lembur dan Tarif Lembur

| Field | Isi |
|---|---|
| Status | ✅ 2026-11-13 — dua layar daftar baca-saja ditulis: Rekap Lembur `/payroll/payroll-runs/[id]/overtime` (pegawai, tanggal lembur, menit terverifikasi, pengali, tarif per jam, nominal; pencarian dan halaman dari server) dengan tombol "Rekap Lembur" di detail run dijaga `PayrollRun : Read`, dan Tarif Lembur `/payroll/overtime-rates` yang hanya membaca endpoint HR, tanpa kontrol tulis, dengan keterangan "diubah di modul HR" tanpa kondisi; grup menu "Lembur" dengan butir "Tarif Lembur" dijaga `OvertimeRate : Read`; kalimat kosong persis dari skema (`UI GATE` 8 REUSE, 0 COMPOSE, 0 NEW). 4 dari 4 kriteria terpetakan ke source dan 4 dari 4 dinyatakan lulus oleh pemilik di peramban (2026-11-13, tanpa rincian per langkah). ESLint 0 galat (4 peringatan `set-state-in-effect`, 2 warisan); 18 uji unit baru lulus (termasuk pencocokan ke source backend dan HR); suite penuh 4.892 uji, 51 gagal, himpunan identik dengan sebelum task (0 kegagalan baru); `payroll-module-contract.test.mjs` diperbarui untuk grup menu baru dan lulus 12 dari 12; `check-css-usage.mjs` 0 temuan. Belum terpenuhi: `npm run build` (`NOT RUN`), pemeriksaan visual dan uji manual langkah 1–10 (`NOT FEASIBLE` oleh agen); prasyarat `PAY-BE-23` dan `PAY-FE-17` masih 🟡 (dikerjakan atas instruksi pemilik). Terbuka: keputusan pemilik dari `PAY-BE-23` (hak rekap `Read` vs `ReadEmployeeDetail`); layar mengikuti kontrak (`Read`). **Naik ke ✅ 2026-11-13 atas instruksi pemilik:** pemilik melaporkan `npm run dev` berhasil dan seluruh uji lulus (laporan lisan, tanpa rincian per langkah, tanpa screenshot tersimpan; bukan bukti agen). Pernyataan "belum terpenuhi" dan "belum dibuktikan" di atas berlaku saat ditulis; yang tetap berlaku: `npm run build` `NOT RUN`; prasyarat backend `PAY-BE-23` masih 🟡 (belum terbukti build dan uji API). Laporan: [`PAY-FE-19.md`](../task/report/frontend/PAY-FE-19.md) |
| Outcome | Petugas membaca lembur yang dibayar per run dan tarif lembur HR |
| Requirement/decision | FR-PGJ-060, 061; PRL-DEC-42; `03-frontend-architecture.md` bagian 3.4 |
| Reuse | Endpoint `OvertimeRate` HR (baca-saja) |
| Cakupan | `FE-PGJ-05` di `/payroll/payroll-runs/[id]/overtime` dengan tombol "Rekap Lembur" di detail run; `FE-PGJ-06` di `/payroll/overtime-rates`; grup menu "Lembur" |
| Dependency | `PAY-FE-17`, `PAY-BE-23` |
| Acceptance criteria | 1. Rekap menampilkan kolom sesuai skema. 2. Tarif lembur tanpa tombol ubah. 3. Butir "Tarif Lembur" dijaga `OvertimeRate : Read`. 4. Kalimat kosong sesuai skema. |
| Verifikasi | ESLint; build; uji manual `UAT-21`, `UAT-22` |
| Risiko/pemilik | Peran Payroll perlu hak `OvertimeRate : Read` dari admin. Pemilik: Yasmin |
| DoD | Kriteria terbukti, menu dan jalan masuk ada, laporan ditulis |

### `PAY-FE-20` ✅ — Laporan Beban Gaji dan PPh/BPJS

| Field | Isi |
|---|---|
| Status | ✅ 2026-11-13 — dua layar laporan baca-saja ditulis: Beban Gaji per Cost Center `/payroll/reports/expense-summary` (saringan periode, run, Cost Center; kartu total penghasilan dan potongan; tabel Cost Center, komponen, jenis, jumlah pegawai, nominal) dan PPh dan BPJS `/payroll/reports/tax-contribution` (saringan periode, run, cari pegawai; tabel berhalaman server dengan sumber "Tetapan profil" dan versi kebijakan); dua butir menu di "Laporan" dijaga `PayrollReport : ViewAmount`; laporan tidak diminta sebelum periode atau run dipilih; "Belum ada run yang disahkan pada periode ini." tampil sebagai keadaan kosong, bukan galat; nominal hanya di state layar (`UI GATE` 8 REUSE, 0 COMPOSE, 0 NEW). 4 dari 4 kriteria terpetakan ke source dan 4 dari 4 dinyatakan lulus oleh pemilik di peramban (2026-11-13, tanpa rincian per langkah). ESLint 0 galat (4 peringatan `set-state-in-effect` pada hook baru, pola yang sama dengan hook yang sudah ada); 18 uji unit baru lulus (termasuk pencocokan ke source backend); suite penuh 4.910 uji, 51 gagal, himpunan identik dengan sebelum task (0 kegagalan baru); `payroll-module-contract.test.mjs` diperbarui untuk dua butir menu baru dan lulus 12 dari 12; `check-css-usage.mjs` 0 temuan. Belum terpenuhi: `npm run build` (`NOT RUN`), pemeriksaan visual dan uji manual langkah 1–12 (`NOT FEASIBLE` oleh agen); prasyarat `PAY-BE-22` masih 🟡 (dikerjakan atas instruksi pemilik). **Terbuka, butuh keputusan pemilik:** pilihan periode dijaga `PayrollRun : ReadPeriods`, sedangkan usulan peran Finance di matriks hak hanya `PayrollReport : ViewAmount`, sehingga Finance belum dapat memilih periode (opsi A: tambah hak ke peran Finance; opsi B: ubah backend `PAY-BE-22`). **Naik ke ✅ 2026-11-13 atas instruksi pemilik:** pemilik melaporkan `npm run dev` berhasil dan seluruh uji lulus (laporan lisan, tanpa rincian per langkah, tanpa screenshot tersimpan; bukan bukti agen). Pernyataan "belum terpenuhi" dan "belum dibuktikan" di atas berlaku saat ditulis; yang tetap berlaku: `npm run build` `NOT RUN`; prasyarat backend `PAY-BE-22` masih 🟡 (belum terbukti build dan uji API). Laporan: [`PAY-FE-20.md`](../task/report/frontend/PAY-FE-20.md) |
| Outcome | Kepala Payroll dan Finance membaca dua laporan baru |
| Requirement/decision | FR-PGJ-050–052; `03-frontend-architecture.md` bagian 3.5 |
| Reuse | Pola layar `/payroll/reports/recap` yang ada |
| Cakupan | `FE-PGJ-07` di `/payroll/reports/expense-summary`, `FE-PGJ-08` di `/payroll/reports/tax-contribution`; dua butir menu di "Laporan" |
| Dependency | `PAY-BE-22` |
| Acceptance criteria | 1. Saringan periode atau run, Cost Center, cari pegawai. 2. Hasil kosong berbunyi "Belum ada run yang disahkan pada periode ini." 3. Butir menu dijaga `PayrollReport : ViewAmount`; tanpa hak tidak tampil. 4. Nominal tidak disimpan di penyimpanan browser. |
| Verifikasi | ESLint; build; uji manual `UAT-18`–`UAT-20` |
| Risiko/pemilik | — / Yasmin |
| DoD | Kriteria terbukti, butir menu terdaftar, laporan ditulis |
