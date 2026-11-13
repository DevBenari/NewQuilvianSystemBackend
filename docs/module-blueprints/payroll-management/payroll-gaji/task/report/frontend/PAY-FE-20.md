# Laporan Perubahan Frontend — `PAY-FE-20`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-FE-20` |
| Judul | Laporan Beban Gaji dan PPh/BPJS |
| Slice | `MVP-1` — `EPIC PGJ-06`, layar `FE-PGJ-07` dan `FE-PGJ-08` |
| Roadmap | `docs/module-blueprints/payroll-management/payroll-gaji/roadmap/frontend-roadmap.md`, kartu `PAY-FE-20` |
| Trace | FR-PGJ-050, 051, 052; PRL-DEC-27, 33, 34, 41; `../../../03-frontend-architecture.md` bagian 1, 2 (peta butir menu), 3.5; `../../../contracts/api-contract.md` (Payroll Report tambahan `GET /expense-summary` dan `GET /tax-contribution`); `UAT-18`, `UAT-19`, `UAT-20` |
| Contract version | `PGJ-C1` — `approved` (Yasmin, 2026-11-12) |
| Wewenang UI | Saringan, kolom, sumber data, hak akses butir menu, dan kalimat kosong dikunci `03-frontend-architecture.md` bagian 3.5. Bentuk layar dan komponen `DEV_DISCRETION` lewat gerbang keputusan base component. Hal yang ditentukan agen dirinci di bagian 8 |
| Dependency | `PAY-BE-22` 🟡 — source backend sudah ada dan dibaca langsung dari working tree backend, tetapi `dotnet build` dan uji API belum dijalankan pemilik. Aturan roadmap meminta prasyarat ✅ sebelum task dimulai; task ini dikerjakan atas instruksi pemilik pada 2026-11-13, sama seperti `PAY-FE-16`–`19` |
| Klasifikasi | `MEDIUM` — dua layar laporan baca-saja, dua endpoint baca, dua butir menu, sebelas berkas source baru, tiga berkas source yang sudah ada diubah, satu tes kontrak diperbarui |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` branch `yasmina`; laporan dan penandaan status di repository backend |
| Model | Claude Sonnet 5.5 |
| Commit frontend saat dikerjakan | `ff46d5db1` (branch `yasmina`); perubahan task ini dan `PAY-FE-16`–`19` belum di-commit |
| Commit backend yang dijadikan rujukan | `726234df` (branch `Yasmina`). Source yang dibaca untuk task ini (controller, service, dan DTO laporan; konstanta status run; query daftar run) tidak berubah dari commit itu |
| Tanggal | 2026-11-13 |
| Status | ✅ — source selesai dan 4 dari 4 kriteria terpetakan ke source; ESLint 0 galat; 18 uji unit baru lulus dan satu tes kontrak diperbarui; suite penuh tanpa kegagalan baru; agen tidak membuka layar (`VISUAL CHECK: NOT FEASIBLE`, `MANUAL TEST: NOT FEASIBLE` oleh agen); pada 2026-11-13 pemilik melaporkan `npm run dev` berhasil dan seluruh uji lulus (laporan lisan, tanpa rincian per langkah, tanpa screenshot tersimpan; bukan bukti agen) lalu menginstruksikan status naik ke ✅; **`npm run build` tetap `NOT RUN`**; backend `PAY-BE-22` belum terbukti build dan uji API (masih 🟡); **keputusan pemilik terbuka: peran Finance menurut usulan matriks (hanya `PayrollReport : ViewAmount`) belum dapat memilih periode, lihat bagian 8** |

---

## 1. Keadaan yang ditemukan di awal

- **Backend** (`PAY-BE-22`), controller `PayrollReportsController`, dua endpoint baca dengan hak `PayrollReport : ViewAmount`
  (hak yang sama dengan layar Rekonsiliasi):
  - `GET /v1/corporate/payroll-management/reports/expense-summary` — query `payrollPeriodId`, `payrollRunId`, `costCenterId`.
    Respons: `periodCode`, `runNumbers`, `rows` (Cost Center, komponen, jenis, jumlah pegawai, nominal), `totalEarning`,
    `totalDeduction`, `note`.
  - `GET /v1/corporate/payroll-management/reports/tax-contribution` — query `payrollPeriodId`, `payrollRunId`, `search`,
    `pageNumber`, `pageSize` (paling banyak 200). Respons berhalaman ditambah `periodCode`, `totalTaxAmount`,
    `totalInsuranceEmployeeAmount`, `note`; tiap baris memuat nomor run, pegawai, Cost Center, PPh, BPJS pegawai, `source`
    (selalu `TETAPAN_PROFIL`), dan `policyVersion`.
  - Salah satu dari periode atau run wajib; bila keduanya kosong, backend menjawab 400 "Pilih periode penggajian atau payroll run."
  - Hanya run yang sudah disahkan dan belum dibalik yang ikut (`Approved`, `SentToFinance`, `PartiallyPaid`, `Paid`,
    `Posted`, `Closed`). **RPT-01** ("Belum ada run yang disahkan pada periode ini.") **bukan galat**: backend menjawab 200
    dengan hasil kosong dan kalimat itu di `note`.
  - Total pada laporan PPh/BPJS dihitung atas seluruh hasil saringan (termasuk pencarian) sebelum dipotong halaman, jadi
    bukan jumlah halaman yang tampil.
- **Frontend**: pola layar laporan yang ada ada di `/payroll/reports/recap` dan `/payroll/reports/reconciliation`. Belum ada
  layar beban gaji maupun PPh/BPJS, dan grup menu "Laporan" baru berisi Rekap Penggajian dan Rekonsiliasi Keuangan.
- **Hal yang menentukan rancangan:**
  - Karena periode atau run wajib, layar tidak boleh memanggil API sebelum salah satunya dipilih; layar butuh keadaan
    "belum memilih".
  - RPT-01 harus tampil sebagai keadaan kosong biasa dengan kalimat dari skema, bukan sebagai kotak galat.
  - Pilihan Payroll Run sebaiknya hanya memuat run yang memang ikut laporan (status yang sama dengan backend), supaya
    petugas tidak memilih run yang pasti kosong.

## 2. Proses bisnis dari sisi pengguna

**Pengguna.** Kepala Payroll dan Finance. Kedua laporan dibuka dengan `PayrollReport : ViewAmount`.

**Langkah — Beban Gaji per Cost Center (`FE-PGJ-07`):**

1. Petugas membuka sidebar Payroll → **Laporan** → **Beban Gaji per Cost Center**. Butir hanya tampil bagi pemegang
   `PayrollReport : ViewAmount`.
2. Layar membuka periode bulan berjalan (atau periode terbaru bila bulan berjalan tidak ada) dan langsung menyusun laporan.
3. Petugas dapat mengganti **Periode Payroll**, memilih satu **Payroll Run** (atau "Semua Run Disahkan pada Periode"), dan
   menyaring **Cost Center**.
4. Di atas tabel tampil kartu: Total Penghasilan, Total Potongan, Jumlah Payroll Run, Jumlah Cost Center; lalu cakupan
   laporan (periode dan nomor run yang ikut). Tabel memuat Cost Center, komponen, jenis, jumlah pegawai, dan nominal.
5. Bila backend menyertakan catatan (misalnya selisih antara jumlah baris dan total pegawai run), catatan itu tampil
   apa adanya di atas tabel.

**Langkah — PPh dan BPJS (`FE-PGJ-08`):**

1. Sidebar Payroll → **Laporan** → **PPh dan BPJS**.
2. Saringan: Periode Payroll, Payroll Run, dan kotak cari (nama atau nomor pegawai).
3. Kartu: Total PPh 21, Total BPJS Pegawai, Jumlah Pegawai (seluruh hasil saringan). Tabel berhalaman server:
   pegawai (nama dan nomor), nomor run, Cost Center, PPh 21, BPJS pegawai, sumber nilai ("Tetapan profil"), dan versi kebijakan.

**Contoh** (data samaran, `UAT-18`/`UAT-20`): periode 2026-05 dengan satu run yang sudah disahkan. Laporan beban gaji
menampilkan IGD dan Farmasi dengan baris Gaji Pokok dan PPh 21; jumlah baris penghasilan sama dengan total bruto run, dan
jumlah baris potongan sama dengan total potongan run. Laporan PPh/BPJS menampilkan PPh 21 dan BPJS pegawai per orang
dengan sumber "Tetapan profil".

**Jalur tidak normal:**

| Keadaan | Yang terjadi |
| --- | --- |
| Belum ada run yang disahkan pada saringan | Tabel menampilkan persis: "Belum ada run yang disahkan pada periode ini." (bukan galat) |
| Run ada, tetapi Cost Center atau kata cari tidak punya baris | "Tidak ada data untuk saringan ini." atau "Tidak ada pegawai untuk saringan ini." |
| Belum ada periode yang dipilih atau tidak ada periode | Pesan "Pilih periode atau payroll run untuk menampilkan laporan." dan laporan tidak diminta |
| Pilihan periode gagal dimuat | Kotak galat dan tombol **Coba lagi** (tanpa tombol bila ditolak karena hak) |
| Laporan gagal dimuat | Kotak galat (pesan server atau "Laporan beban gaji gagal dimuat." / "Laporan PPh dan BPJS gagal dimuat.") dan tombol **Coba lagi** |
| Server menjawab 403, atau pengguna tanpa `PayrollReport : ViewAmount` membuka alamatnya langsung | Halaman diganti pesan akses ditolak standar (`AccessDeniedGate`); laporan tidak diminta (`UAT-19`) |
| Tanpa `PayrollReport : ViewAmount` | Kedua butir menu tidak tampil |
| Pemegang `PayrollReport : ViewAmount` tanpa hak membaca daftar run (`PayrollRun : Read`) | Pilihan run hanya memuat "Semua Run Disahkan pada Periode"; laporan per periode tetap berjalan |
| Pemegang `PayrollReport : ViewAmount` tanpa `PayrollRun : ReadPeriods` (hak pilihan periode) | Kotak galat "Pilihan periode tidak dapat dimuat. Peran Anda perlu hak membaca periode payroll." tanpa tombol Coba lagi; karena backend mewajibkan periode atau run, laporan tidak dapat dibuka. **Ini keadaan peran Finance menurut usulan matriks hak** (bagian 8) |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- **Governance:** `AGENTS.md` frontend; `rules/frontend/*` (arsitektur, katalog dan gerbang keputusan base component,
  pola komposisi halaman, checklist UI, template laporan); `rules/rule-output/status-task-roadmap.md`.
- **Blueprint:** `03-frontend-architecture.md` (bagian 2 dan 3.5), `contracts/api-contract.md`,
  `contracts/permission-audit-matrix.md`, `testing/acceptance-test-matrix.md` (`UAT-18`–`UAT-20`), `roadmap/*`, laporan
  `PAY-BE-22`, `PAY-FE-18`, `PAY-FE-19`.
- **Backend (baca-saja):** `PayrollReportsController.cs`, `PayrollReportService.cs` (`GetExpenseSummaryAsync`,
  `GetTaxContributionAsync`, `LoadApprovedRunsAsync`, konstanta `ApprovedRunStatuses` dan `NoApprovedRunNote`),
  `PayrollReportDtos.cs`, `PayrollRunStatuses.cs`, `PayrollRunDtos.cs` (`PayrollRunQuery`),
  `PayrollBpjsCalculationService.cs` (nilai jenis komponen `EmployerContribution`).
- **Rujukan frontend:** `payroll-recap-view.jsx`, `use-payroll-recap.js`, `payroll-report-table-columns.jsx`,
  `payroll-simulation-view.jsx` (pola galat dan Coba lagi), `payroll-variable-input-view.jsx` (pola tabel berhalaman),
  `data-filter.jsx`, `access-denied-gate.jsx`, `payroll-section.jsx`, `payroll-run-utils.js` (`pickDefaultPeriodId`),
  `payroll-common-utils.js`, `menu-items.jsx`, `payroll-module-contract.test.mjs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/payroll/payroll-expense-report-constants.js` | Baru. Endpoint, hak akses, status run yang disahkan, label jenis komponen dan sumber, saringan bawaan, kalimat RPT-01, seluruh salinan teks |
| `src/utils/payroll/payroll-expense-report-utils.js` | Baru. Normalisasi kedua laporan, penanda RPT-01, parameter permintaan, query pilihan run, teks tampilan |
| `src/lib/hooks/payroll/use-payroll-report-scope.js` | Baru. Pilihan periode (periode awal otomatis) dan run yang disahkan, dipakai kedua laporan |
| `src/lib/hooks/payroll/use-payroll-expense-summary.js` | Baru. Laporan beban gaji: saringan Cost Center, memuat, galat, Coba lagi |
| `src/lib/hooks/payroll/use-payroll-tax-contribution.js` | Baru. Laporan PPh/BPJS: pencarian, halaman, memuat, galat, Coba lagi |
| `src/components/view/payroll/report/payroll-expense-summary-view.jsx` | Baru. Layar Beban Gaji per Cost Center |
| `src/components/view/payroll/report/payroll-tax-contribution-view.jsx` | Baru. Layar PPh dan BPJS |
| `src/components/view/payroll/report/payroll-report-scope-filters.jsx` | Baru. Kontrol Periode dan Payroll Run (larik berkunci untuk `DataFilter`) dan kotak galat pilihan periode |
| `src/components/view/payroll/report/payroll-expense-report-table-columns.jsx` | Baru. Kolom kedua tabel |
| `src/app/payroll/reports/expense-summary/page.jsx` | Baru. Route tipis |
| `src/app/payroll/reports/tax-contribution/page.jsx` | Baru. Route tipis |
| `src/lib/services/payroll/payroll-report-service.js` | Diubah. Menambah `expenseSummary` dan `taxContribution` (GET); `recap` dan `reconciliation` tidak berubah |
| `src/lib/constants/payroll/payroll-constants.js` | Diubah. `PAYROLL_ROUTES.expenseSummary` dan `taxContribution` |
| `src/utils/menu-sidebar/menu-items.jsx` | Diubah. Dua butir di grup "Laporan", dijaga `PayrollReport : ViewAmount` |
| `tests/unit/payroll-module-contract.test.mjs` | Diubah. Daftar butir menu Payroll yang dikunci tes kini memuat "Beban Gaji per Cost Center" dan "PPh dan BPJS", dan daftar rute operasional memuat kedua halaman baru |
| `tests/unit/payroll-expense-report.test.mjs` | Baru. 18 uji, termasuk pencocokan dengan source backend |

### 3.3 Kepatuhan arsitektur frontend

- **Pola:** layar laporan baca-saja Payroll V2 (service → hook dengan state lokal → view), sama dengan `PAY-FE-15`–`19`.
  Tidak ada slice, instance Axios, komponen shared, atau CSS Module baru.
- **Alur dependensi:** route → view → hook → service/utils/constants. View tidak memanggil Axios; utils murni.
- **Laporan tidak diminta tanpa syarat:** hook hanya memanggil service bila `PayrollReport : ViewAmount` dipegang **dan**
  periode atau run terpilih (`enabled = Boolean(canView) && hasScope`); keduanya dikunci uji statis, dan pemeriksaan
  `enabled` diuji mendahului pemanggilan service.
- **Hak akses:** `usePermission` dengan `PayrollReport : ViewAmount`; tidak ada nama peran di kode. Tanpa hak, layar
  menampilkan `AccessDeniedGate`; 403 dari server juga.
- **Nominal tidak disimpan di peramban:** nominal hanya ada di state layar. Tidak ada `localStorage`, `sessionStorage`,
  `indexedDB`, cookie, atau Redux di kesepuluh berkas layar (diuji lewat pemindaian sumber).
- **Kontrol saringan sebagai larik berkunci, bukan komponen pembungkus.** `DataFilter` hanya meratakan Fragment dan larik,
  lalu menghitung jumlah kolom dan menempatkan kotak cari menurut jumlah itu. Versi pertama memakai komponen pembungkus
  untuk dua `FilterSelect`; saat ditinjau ulang, itu dihitung sebagai satu kontrol (kolom keliru, dan pada laporan PPh/BPJS
  kotak cari jatuh sebelum kedua pilihan). Diganti dengan fungsi `buildReportScopeFilterControls(scope)` yang
  mengembalikan larik berkunci, dan dikunci satu uji statis supaya tidak kembali.
- **Menu:** dua butir ditambahkan di grup "Laporan" yang sudah ada (sesuai peta menu blueprint), keduanya
  `PayrollReport : ViewAmount`; tanpa hak butir tidak tampil, dan bila grup tidak punya butir yang diizinkan, grup ikut hilang
  (diuji lewat `filterMenuItemsByPermission`).
- **Tes kontrak Payroll diperbarui, bukan dilonggarkan:** tes mengunci daftar tepat butir menu Payroll; dua butir baru
  ditambahkan ke daftar, dan tes tetap meminta setiap butir punya hak menu dan halaman nyata.

### 3.4 Gerbang tampilan

**Modul referensi visual:** `payroll-recap-view.jsx` (laporan Payroll dengan kartu ringkasan, filter, dan tabel),
`payroll-variable-input-view.jsx` (tabel berhalaman server), `payroll-simulation-view.jsx` (keadaan belum memilih dan galat
dengan Coba lagi).

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Header | `Hero` | `REUSE` | — |
| Periode Payroll dan Payroll Run | `DataFilter`, `FilterSelect` | `REUSE` | Pola saringan baku, sama dengan Rekap Penggajian |
| Cost Center (beban gaji) | `ResourceFilterSelect` + `useSelectResource("costCenters")` | `REUSE` | Pola Rekap Penggajian; pengguna tidak mengetik ID |
| Cari pegawai (PPh/BPJS) | Pencarian bawaan `DataFilter` | `REUSE` | Pola daftar baku |
| Kartu ringkasan, cakupan laporan | `SummaryCards`, `PayrollSection`, `PayrollInfoGrid` | `REUSE` | Pola Payroll yang sudah ada. Kartu hanya dirender saat memuat atau bila ada ringkasan: `SummaryCards` bawaan menampilkan "Ringkasan tidak tersedia." untuk daftar kosong, yang menyesatkan pada keadaan belum memilih dan RPT-01 (berbeda dari Rekap Penggajian, yang menampilkannya) |
| Tabel beban gaji (tanpa halaman) dan PPh/BPJS (berhalaman server) | `DataTable`, `Pagination` — `tableStyles.regionDataTable` | `REUSE` | Pola daftar baku; urutan dipegang server (`sortLatestFirst={false}`) |
| Catatan server, galat, 403, keadaan belum memilih | `InformationAlert`, `AccessDeniedGate`, `BaseButton`, `payroll-run.module.css` (`alertStack`, `errorActions`, `state`) | `REUSE` | Pola galat dan Coba lagi `PAY-FE-18` |
| Teks bertumpuk di sel (nama + kode) | `payroll-run.module.css` (`cellStack`, `amountCell`, `wrapText`) | `REUSE` | CSS Module Payroll yang sudah ada |
| Dua butir menu di grup "Laporan" | `menu-items.jsx` | `REUSE` | Pola menu Payroll |

`UI GATE: PASS — 8 elemen: REUSE 8, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`

Hasil pemeriksa dan grep anti-regresi, ditempel apa adanya (keadaan akhir berkas):

```text
$ node ../QuilvianEngineeringSkills/Claude/.claude/rules/frontend/tools/check-css-usage.mjs \
    .../report/payroll-expense-summary-view.jsx \
    .../report/payroll-tax-contribution-view.jsx \
    .../report/payroll-report-scope-filters.jsx \
    .../report/payroll-expense-report-table-columns.jsx
OK — 4 berkas diperiksa, 0 temuan.

$ grep -nEi "#[0-9a-f]{3,8}\b|rgba?\(" <4 berkas baru>                                   (kosong)
$ grep -nE "font-size|font-weight|line-height" <4 berkas baru>                           (kosong)
$ grep -nE "<button|className=\"btn|btn-primary|btn-secondary" <4 berkas baru>           (kosong)
$ grep -n "<table" <4 berkas baru>                                                       (kosong)
$ grep -nE "fw-(bold|semibold|light)|fs-[0-9]" <4 berkas baru>                           (kosong)
$ grep -n "!important" <4 berkas baru>                                                   (kosong)
$ grep -nE "<pola #7 class Tailwind>" <4 berkas baru>                                    (kosong)
$ grep -nE "fixed inset-0|position: ?fixed" <4 berkas baru>                              (kosong)
$ grep -nE "<(input|select|textarea)\b|UUID|GUID" <4 berkas baru>                        (kosong)
$ grep -nE "variant=\"(outline|danger-outline|default|link|light|dark)\"|<StatusBadge[^>]*(status=\{[^}]*label|variant=)" <4 berkas baru>   (kosong)
$ grep -n "style={{" <4 berkas baru>                                                     (kosong)
```

Tidak ada stylesheet baru, sehingga pemeriksaan pada berkas gaya (warna literal, typography, `!important` di stylesheet)
tidak berlaku (`NOT APPLICABLE`).

| Layar | Screenshot | Catatan perbandingan dengan referensi |
| --- | --- | --- |
| `/payroll/reports/expense-summary` | `VISUAL CHECK: NOT FEASIBLE` | Dev server, build, dan skrip peramban tidak dijalankan: pemilik meminta agen tidak menyalakannya otomatis, dan agen tidak memegang sesi login. Susunan mengikuti Rekap Penggajian, tetapi **tampilannya belum pernah dilihat**. Yang paling perlu dilihat: jumlah kolom grid pada baris saringan (tiga kontrol) dan kartu ringkasan empat buah |
| `/payroll/reports/tax-contribution` | `VISUAL CHECK: NOT FEASIBLE` | Alasan sama. Yang paling perlu dilihat: urutan kontrol pada baris saringan (Periode, kotak cari, Payroll Run karena penempatan `before-last` bawaan `DataFilter`) dan lebar tabel tujuh kolom |
| Sidebar Payroll (grup Laporan) | `VISUAL CHECK: NOT FEASIBLE` | Alasan sama |

Catatan 2026-11-13: pemilik menguji layar di peramban lewat `npm run dev` dan melaporkan lulus; tidak ada screenshot yang disimpan, sehingga `VISUAL CHECK: NOT FEASIBLE` di atas tetap berlaku bagi agen.

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat pilihan periode | Pilihan Periode nonaktif; laporan belum diminta |
| Belum memilih (tanpa periode dan run) | "Pilih periode atau payroll run untuk menampilkan laporan." |
| Memuat laporan | Kartu berupa kerangka dan tabel "Menyusun laporan beban gaji..." atau "Menyusun laporan PPh dan BPJS..." |
| Tanpa ringkasan (belum memilih, RPT-01, galat) | Kartu tidak tampil |
| Kosong, RPT-01 | "Belum ada run yang disahkan pada periode ini." beserta "Laporan hanya memuat run yang sudah disahkan." |
| Kosong karena saringan | "Tidak ada data untuk saringan ini." atau "Tidak ada pegawai untuk saringan ini." |
| Gagal | Kotak galat dengan pesan server atau kalimat baku, dan tombol **Coba lagi** |
| Tanpa hak akses | Butir menu tidak tampil; membuka alamat langsung atau 403 dari server diganti pesan akses ditolak standar |
| Berisi | Kartu, cakupan laporan (beban gaji), dan tabel; catatan server (bila ada) di atas tabel |

## 5. Endpoint yang dikonsumsi

#### Corporate / Payroll Management / Report

Base URL: `/v1/corporate/payroll-management/reports`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/expense-summary?payrollPeriodId=&payrollRunId=&costCenterId=` | Laporan beban gaji per Cost Center | `PayrollReport : ViewAmount` |
| `GET` | `/tax-contribution?payrollPeriodId=&payrollRunId=&search=&pageNumber=&pageSize=` | Laporan PPh dan BPJS | `PayrollReport : ViewAmount` |

#### Endpoint bantu pilihan saringan (sudah ada, tidak diubah task ini)

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/payroll-management/payroll-runs/period-options` | Pilihan periode (`PayrollRunService.periodOptions`) | `PayrollRun : ReadPeriods` |
| `GET` | `/v1/corporate/payroll-management/payroll-runs?payrollPeriodId=&status=Approved,SentToFinance,PartiallyPaid,Paid,Posted,Closed&pageNumber=1&pageSize=100` | Pilihan Payroll Run (hanya run yang disahkan) | `PayrollRun : Read` |
| `GET` | `/v1/corporate/human-resource/master-data/cost-centers/options` (lewat `useSelectResource("costCenters")`) | Pilihan Cost Center (hanya laporan beban gaji) | Hak pilihan data master HR menurut registri pilihan; tidak diperiksa task ini |

Tidak ada endpoint `POST`, `PUT`, `PATCH`, atau `DELETE` yang dipanggil.

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` pada 14 berkas source baru atau berubah dan dua berkas test | 0 galat; 4 peringatan `react-hooks/set-state-in-effect` pada tiga hook baru, pola yang sama dengan hook Payroll lain (`use-payroll-recap.js` sendiri punya 2; hook `PAY-FE-17`–`19` masing-masing 1). Berkas yang kemudian berubah (kedua view, berkas kontrol saringan, berkas uji) diperiksa ulang: 0 galat, 0 peringatan | `PASS` (peringatan: pola yang sama dengan hook yang sudah ada) | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/payroll-expense-report.test.mjs tests/unit/payroll-module-contract.test.mjs` | 30 uji (18 baru + 12 kontrak), 30 lulus, 0 dilewati (source backend ada, jadi pencocokan ke backend ikut berjalan) | `PASS` | Keluaran perintah |
| Alamat kedua laporan dan hak `PayrollReport : ViewAmount` dengan `[Route]`, `ControllerName`, dan `[AccessPermission]` controller | Cocok | `PASS` | Uji "alamat dan hak akses kedua laporan" |
| Parameter permintaan sama dengan properti query backend (3 properti beban gaji, 5 properti PPh/BPJS); saringan bawaan hanya memuat properti itu | Cocok | `PASS` | Uji "parameter permintaan" |
| Query pilihan run memakai properti `PayrollRunQuery`; daftar status run yang disahkan sama dengan `ApprovedRunStatuses` di backend (dibaca dari source: nama konstanta, lalu nilainya) | Cocok | `PASS` | Uji "pilihan run" |
| Kalimat RPT-01 sama dengan `NoApprovedRunNote` backend; nilai sumber `TETAPAN_PROFIL` sama dengan backend; setiap jenis komponen yang disebut `PayrollReportService` punya label | Cocok | `PASS` | Uji "kalimat RPT-01, nilai sumber, dan jenis komponen" |
| Properti yang dibaca layar ada di DTO backend dan nilai khasnya sampai ke hasil normalisasi (7 properti baris beban gaji, 6 properti respons; 8 properti baris PPh/BPJS, 4 properti halaman) | Cocok | `PASS` | Dua uji properti DTO |
| Ketergantungan hak: `period-options` dijaga `PayrollRun : ReadPeriods` dan daftar run `PayrollRun : Read` di controller backend, bukan hak laporan; layar menyatakan penolakan hak periode dengan jelas dan gagal-muat run tidak menutup laporan | Cocok | `PASS` | Uji "ketergantungan hak" |
| RPT-01 sebagai keadaan kosong (beban gaji: tanpa nomor run; PPh/BPJS: daftar kosong dan `note` sama dengan kalimat RPT-01); pencarian tanpa hasil dan Cost Center tanpa baris bukan RPT-01 | Sesuai | `PASS` | Uji "RPT-01" |
| Laporan tidak diminta tanpa periode atau run; parameter kosong tidak dikirim; PascalCase diterima; angka teks menjadi angka; larik dan halaman kosong aman | Sesuai | `PASS` | Tiga uji murni |
| Dua butir menu dijaga `PayrollReport : ViewAmount`; hanya pemegang `ViewAmount` melihat keduanya, pemegang `Read` hanya melihat Rekap, tanpa hak grup hilang | Sesuai | `PASS` | Uji menu (memakai `filterMenuItemsByPermission` pada grup dari source) |
| Nominal tidak disimpan di penyimpanan peramban dan tidak masuk Redux (10 berkas); service hanya `GET`; tanpa kontrol tulis | Sesuai | `PASS` | Dua uji sifat layar |
| Penjaga permintaan di kedua hook (`enabled = Boolean(canView) && hasScope`, pemeriksaan mendahului pemanggilan service) | Sesuai | `PASS` | Uji "laporan tidak diminta sebelum..." |
| Kontrol saringan berupa larik berkunci dan menjadi anak langsung `DataFilter` (asumsi perataan `DataFilter` ikut dikunci) | Sesuai | `PASS` | Uji "kontrol Periode dan Payroll Run..." |
| `payroll-module-contract.test.mjs` | 12 dari 12 lulus setelah daftar menu dan daftar rute diperbarui | `PASS` | Keluaran perintah |
| Pemeriksa statis sekali pakai (di luar repo): setiap simbol impor di 12 berkas terpetakan ke ekspor modul tujuannya; diulang untuk berkas yang kemudian berubah | `OK — 12 berkas, semua impor terpetakan.`, lalu `OK — 3 berkas, ...` dan `OK — 2 berkas, ...` | `PASS` | Keluaran skrip |
| Suite unit penuh (`node --import ./tests/helpers/register.mjs --test tests/unit/*.test.mjs`), dijalankan pada keadaan akhir source | 4.910 uji, 4.859 lulus, 51 gagal pada 35 berkas | `EXISTING / ENVIRONMENT ISSUE` (51), `PASS` (18 uji baru) | Keluaran perintah |
| Pembanding: himpunan (berkas, nama uji) yang gagal pada suite sebelum task ini (`PAY-FE-19`, 4.892 uji) | Identik: 0 kegagalan baru, 0 yang tidak lagi gagal; seluruhnya juga gagal pada `HEAD` murni (dibuktikan pada `PAY-FE-16`) | `PASS` (tanpa kegagalan baru) | Selisih himpunan nama uji |
| Grep anti-regresi UI dan `check-css-usage.mjs` | 0 temuan | `PASS` | Bagian 3.4 |
| `npm run build` | Tidak dijalankan | `NOT RUN` | — |
| Pembukaan layar di peramban dan screenshot | Tidak dilakukan | `NOT FEASIBLE` | Bagian 3.4 |
| `npm run dev` dan uji manual oleh pemilik (2026-11-13) | Dilaporkan berhasil dan seluruh uji lulus. Tidak dirinci: layar dan akun yang dipakai, hasil per langkah uji; tidak ada screenshot tersimpan | `OWNER-REPORTED` — tidak diverifikasi agen; bukan pengganti `npm run build` | Pesan pemilik |

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/payroll-expense-report.test.mjs — PASS (18/18)`

Uji manual: `NOT FEASIBLE` — agen tidak menyalakan `npm run dev` atau skrip peramban, dan tidak memegang sesi login.

**Langkah uji untuk pemilik** (setelah `npm run dev`; akun dengan `PayrollReport : ViewAmount`, `PayrollRun : ReadPeriods`,
`PayrollRun : Read`; periode yang punya run berstatus disahkan, dan satu periode tanpa run yang disahkan):

1. Buka sidebar Payroll → **Laporan**. Harapan: ada **Beban Gaji per Cost Center** dan **PPh dan BPJS** di samping Rekap
   Penggajian dan Rekonsiliasi Keuangan. Dengan akun tanpa `PayrollReport : ViewAmount`, kedua butir tidak tampil
   (`UAT-19`); membuka alamat langsungnya menampilkan pesan akses ditolak.
2. Buka **Beban Gaji per Cost Center**. Harapan: periode bulan berjalan (atau terbaru) terpilih dan laporan langsung
   tersusun; kartu Total Penghasilan, Total Potongan, Jumlah Payroll Run, Jumlah Cost Center; tabel Cost Center, komponen,
   jenis, jumlah pegawai, nominal (`UAT-18`).
3. Bandingkan Total Penghasilan dan Total Potongan dengan total bruto dan total potongan run yang sama di layar Rekap
   Penggajian. Harapan: sama (`UAT-18`: jumlah seluruh baris = total kotor dan potongan run yang disahkan). Bila berbeda,
   catatan selisih dari server tampil di atas tabel.
4. Pilih satu Payroll Run, lalu satu Cost Center. Harapan: hasil menyempit; pilihan run hanya memuat run yang disahkan;
   mengganti periode mengosongkan pilihan run.
5. Pilih periode yang belum punya run yang disahkan. Harapan: persis "Belum ada run yang disahkan pada periode ini."
   (bukan kotak galat merah).
6. Tekan tombol reset (↻) pada kartu saringan. Harapan: kembali ke periode awal, tanpa run, tanpa Cost Center, dan tanpa kata cari.
7. Buka **PPh dan BPJS**. Harapan: kartu Total PPh 21, Total BPJS Pegawai, Jumlah Pegawai; tabel pegawai, nomor run,
   Cost Center, PPh 21, BPJS Pegawai, "Tetapan profil", dan versi kebijakan; nilainya sama dengan tetapan profil penggajian
   pegawai itu (`UAT-20`).
8. Cari satu nama atau nomor pegawai; pindah halaman. Harapan: server yang memfilter dan membagi halaman; kartu total tetap
   untuk seluruh hasil saringan, bukan hanya halaman itu.
9. Amati baris saringan pada lebar layar sempit dan lebar. Harapan: kontrol tertata rapi (belum pernah dilihat agen).
10. Buka Application → Local Storage dan Session Storage di alat pengembang peramban. Harapan: tidak ada nominal laporan yang
    tersimpan.
11. Matikan backend lalu muat ulang kedua layar. Harapan: pesan galat dan tombol Coba lagi.
12. Dengan akun yang hanya memegang `PayrollReport : ViewAmount` (seperti usulan peran Finance), buka kedua layar. Harapan
    saat ini: "Pilihan periode tidak dapat dimuat. Peran Anda perlu hak membaca periode payroll." dan laporan tidak dapat
    dibuka. Ini keadaan yang perlu diputuskan pemilik (bagian 8), bukan kerusakan layar.

Uji manual oleh pemilik (2026-11-13): `OWNER-REPORTED`, lulus. Pemilik menjalankan `npm run dev` dan melaporkan seluruh uji lulus; hasil per langkah tidak dirinci dan tidak diverifikasi agen. `npm run build` tetap `NOT RUN`.

**Tidak dijalankan oleh agen:** `npm run build`, `npm run dev`, uji e2e, uji manual dan pemeriksaan visual di peramban, `dotnet build`
backend.

## 7. Acceptance criteria dan Definition of Done

Ringkasan: **4 dari 4** kriteria terpetakan ke source dan bukti statis; **4 dari 4** dinyatakan lulus oleh pemilik di peramban (2026-11-13, tanpa rincian per langkah; bukan bukti agen).

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Saringan periode atau run, Cost Center, cari pegawai | Terpenuhi di source; pengujian pemilik: lulus (2026-11-13, tanpa rincian) | Skema bagian 3.5 membagi saringan per layar dan backend sama: beban gaji memakai periode, run, Cost Center (`PayrollExpenseSummaryQuery`); PPh/BPJS memakai periode, run, cari pegawai (`PayrollTaxContributionQuery`). Uji "parameter permintaan" mencocokkan keduanya dengan DTO query backend |
| 2. Hasil kosong berbunyi "Belum ada run yang disahkan pada periode ini." | Terpenuhi di source | Uji "RPT-01" dan uji kalimat; kalimat dicocokkan dengan `NoApprovedRunNote` di backend |
| 3. Butir menu dijaga `PayrollReport : ViewAmount`; tanpa hak tidak tampil | Terpenuhi di source | `menu-items.jsx`; uji menu dengan `filterMenuItemsByPermission`; `payroll-module-contract.test.mjs` lulus |
| 4. Nominal tidak disimpan di penyimpanan browser | Terpenuhi di source | Pemindaian sumber kesepuluh berkas layar: tanpa `localStorage`, `sessionStorage`, `indexedDB`, cookie, dan Redux; nominal hanya di state layar |
| DoD: kriteria terbukti | Terpenuhi menurut pernyataan pemilik (2026-11-13); build produksi belum (`NOT RUN`) | `OWNER-REPORTED`; bagian 6 |
| DoD: butir menu terdaftar | Terpenuhi | Dua butir di grup "Laporan" |
| DoD: laporan ditulis | Terpenuhi | Berkas ini |
| Verifikasi roadmap: lint | Terpenuhi (0 galat) | Bagian 6 |
| Verifikasi roadmap: build | **Belum** | `NOT RUN` |
| Verifikasi roadmap: uji manual `UAT-18`–`UAT-20` | Terpenuhi menurut pernyataan pemilik (2026-11-13), tanpa rincian per langkah | `OWNER-REPORTED` |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 4 peringatan ESLint `react-hooks/set-state-in-effect` pada tiga hook baru, pola yang sama dengan hook Payroll V2 lain dan hook `use-payroll-recap.js` yang sudah ada. Tidak diperbaiki di luar cakupan |
| Periode awal dipilih otomatis (ditentukan agen) | Skema hanya menyebut saringan "periode atau run". Agar tidak membuka layar kosong, layar memilih periode bulan berjalan (atau terbaru) dan langsung menyusun laporan, dengan fungsi yang sama seperti Persiapan Penggajian (`pickDefaultPeriodId`). Bila pemilik lebih suka layar kosong sampai petugas memilih, ubahannya satu baris di `use-payroll-report-scope.js` |
| Kolom "Payroll Run" pada laporan PPh/BPJS (tambahan agen) | Skema menyebut kolom: pegawai, Cost Center, PPh, BPJS pegawai, sumber, versi kebijakan. Backend mengirim nomor run di tiap baris, dan pegawai yang sama dapat muncul pada beberapa run dalam satu periode; kolom nomor run ditambahkan supaya baris tidak tampak ganda tanpa penjelasan. Mudah dicabut bila tidak diinginkan |
| **Keputusan pemilik terbuka: peran Finance tidak dapat memilih periode** | Backend mewajibkan periode atau run (400 bila kosong), dan satu-satunya sumber pilihan periode di Payroll (`period-options`) dijaga `PayrollRun : ReadPeriods`, bukan hak laporan (dibuktikan dari controller dan dikunci uji). `permission-audit-matrix.md` bagian 3 mengusulkan peran **Finance = hanya `PayrollReport : ViewAmount`** dan Kepala Payroll = hak Staf Payroll (yang memuat `PayrollRun : ReadPeriods`) ditambah `ViewAmount`. Akibatnya: **Kepala Payroll dapat memakai kedua layar; Finance menurut usulan itu tidak** — ia melihat pesan "Pilihan periode tidak dapat dimuat. Peran Anda perlu hak membaca periode payroll." dan laporan tidak dapat dibuka, padahal Outcome kartu menyebut "Kepala Payroll dan Finance membaca dua laporan baru". Layar sudah menyatakan keadaan ini dengan jelas, tetapi tidak dapat menyelesaikannya dari sisi frontend. Pilihan: **(A, direkomendasikan, tanpa perubahan kode)** tambahkan `PayrollRun : ReadPeriods` (dan `PayrollRun : Read` bila pilihan per run diinginkan) ke peran Finance di matriks hak dan ke data peran; biayanya satu baris matriks; endpoint `period-options` hanya mengembalikan daftar pilihan periode (id, kode, nama, bulan potongan, status kunci menurut normalisasi frontend), tanpa data run atau nominal. **(B)** ubah backend (`PAY-BE-22`) agar kedua laporan, saat periode dan run kosong, memakai periode terbaru yang punya run disahkan, atau sediakan endpoint pilihan periode milik laporan yang dijaga `ViewAmount`; menghindari pelebaran hak tetapi mengubah kontrak `PGJ-C1`, sehingga perlu persetujuan dan task backend. Sampai diputuskan, layar tetap benar untuk peran yang memegang kedua hak |
| Pilihan run memerlukan hak baca run | Pilihan Payroll Run diambil dari daftar run (`PayrollRun : Read`). Peran yang memegang `PayrollReport : ViewAmount` (dan `ReadPeriods`) tetapi tidak `PayrollRun : Read` hanya melihat "Semua Run Disahkan pada Periode"; laporan per periode tetap berjalan (dikunci uji) |
| Pilihan Cost Center | Pilihan diambil dari endpoint pilihan master data HR lewat `useSelectResource("costCenters")`, sama dengan Rekap Penggajian. Hak membacanya tidak diperiksa task ini; bila ditolak, saringan Cost Center kosong dan laporan tetap tampil untuk semua Cost Center |
| Pilihan run dibatasi 100 | Daftar run diminta satu halaman 100 baris, sama dengan Rekap Penggajian. Periode dengan lebih dari 100 run disahkan akan terpotong di pilihan (tidak realistis untuk satu periode) |
| Total kartu PPh/BPJS | Total PPh 21 dan Total BPJS Pegawai berasal dari backend dan mencakup seluruh hasil saringan (dibuktikan dari source `GetTaxContributionAsync`), bukan hanya halaman yang tampil. Layar tidak menjumlahkan apa pun |
| Kalimat RPT-01 dikenali lewat teks | Laporan PPh/BPJS menyampaikan RPT-01 lewat `note`, sehingga layar membandingkan `note` dengan kalimat baku (dan daftar harus kosong). Bila backend mengubah kalimat itu, layar jatuh ke "Tidak ada pegawai untuk saringan ini." dan catatan server tampil sebagai kotak info. Uji mencocokkan kalimat dengan source backend, sehingga perubahan di sana akan membuat uji gagal |
| Hak akses yang harus diberikan admin | `PayrollReport : ViewAmount` (hak yang sama dengan Rekonsiliasi Keuangan, sehingga tidak ada hak laporan baru); dua butir menu tidak tampil tanpa hak itu. Agar laporan **dapat dipakai**, peran juga memerlukan `PayrollRun : ReadPeriods` (wajib) dan `PayrollRun : Read` (pilihan per run); lihat baris keputusan pemilik di atas |
| Masalah yang diketahui | (1) Layar sudah diuji pemilik (2026-11-13, lulus, tanpa rincian), tetapi agen belum pernah melihatnya dan tidak ada screenshot tersimpan. (2) Penempatan kotak cari pada laporan PPh/BPJS mengikuti `before-last` bawaan `DataFilter` (di antara Periode dan Payroll Run), sama dengan layar daftar Payroll lain; bila terasa janggal, dapat diubah menjadi `end` dengan satu atribut. (3) `usePermission` mengembalikan "diizinkan" selama daftar hak belum dimuat, sehingga permintaan pertama dapat terkirim sebelum hak diketahui; backend tetap menolak 403 dan layar menampilkan akses ditolak |
| Dependency backend | `PAY-BE-22` 🟡: source dibaca dan dicocokkan, tetapi `dotnet build` dan uji API belum dijalankan pemilik. Seluruh perilaku kedua layar bergantung pada backend itu berjalan |
| Perubahan sampingan | `NONE` pada source. Satu tes kontrak (`payroll-module-contract.test.mjs`) diperbarui karena butir menu dan rute baru, dijelaskan di 3.3 |
| Interupsi | Sesi sempat terpotong karena batas konteks; pekerjaan dilanjutkan dari berkas yang sudah ditulis (konstanta, utils, service) setelah memeriksa isinya, tanpa penggandaan |
| Status Git | Frontend, task ini: ` M` `payroll-constants.js`, ` M` `payroll-report-service.js`, ` M` `menu-items.jsx`, ` M` `payroll-module-contract.test.mjs`; `??` route `reports/expense-summary/` dan `reports/tax-contribution/`, empat berkas di `view/payroll/report/`, konstanta, tiga hook, utils, dan `tests/unit/payroll-expense-report.test.mjs`. Perubahan `PAY-FE-16`–`19` yang belum di-commit masih ada. Tidak ada yang di-stage atau di-commit. Backend, dari task ini: `??` laporan ini; ` M` `frontend-roadmap.md` dan `requirement-traceability.md` (tanda status). ` M` `PayrollSimulationService.cs` bukan dari task ini |
| Langkah berikutnya | Status ✅ atas instruksi pemilik (2026-11-13). Sisa: `npm run build` produksi (`NOT RUN`), pembuktian build serta uji API backend `PAY-BE-22` (masih 🟡), dan keputusan peran Finance (opsi A atau B pada baris keputusan di atas) yang tetap terbuka: selama belum diputuskan, Finance menurut usulan matriks belum dapat memakai kedua laporan. Ini task FE terakhir pada roadmap `payroll-gaji`. |
