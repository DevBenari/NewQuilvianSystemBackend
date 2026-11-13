# Laporan Perubahan Frontend — `PAY-FE-16`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-FE-16` |
| Judul | Tunjangan tetap pada Struktur Gaji |
| Slice | `MVP-1` — `EPIC PGJ-01` dan `PGJ-02`, layar `FE-PGJ-01` |
| Roadmap | `docs/module-blueprints/payroll-management/payroll-gaji/roadmap/frontend-roadmap.md`, kartu `PAY-FE-16` |
| Trace | FR-PGJ-001, 002, 003, 010, 011; PRL-DEC-02, 12, 13; `../../../03-frontend-architecture.md` bagian 3.1 dan 6; `../../../contracts/api-contract.md` grup Salary Structure Allowance; `../../../contracts/validation-matrix.md` bagian 1 (`SA-01`–`SA-07`) dan bagian 2 (`SR-01`–`SR-04`) |
| Contract version | `PGJ-C1` — `approved` (Yasmin, 2026-11-12) |
| Wewenang UI | Isi layar, sumber data, hak akses per tombol, serta kalimat kosong dan gagal dikunci `03-frontend-architecture.md` bagian 3.1 dan 6. Bentuk form (modal), tata letak, dan komponen `DEV_DISCRETION` lewat gerbang keputusan base component |
| Dependency | `PAY-BE-15` 🟡 dan `PAY-BE-16` 🟡 — source backend sudah ada dan dibaca langsung dari working tree backend, tetapi `dotnet build` dan uji API belum dijalankan pemilik. Aturan roadmap meminta prasyarat `[BE]` berstatus ✅ sebelum task dimulai; task ini dikerjakan atas instruksi pemilik pada 2026-11-13 setelah kondisi itu disampaikan |
| Klasifikasi | `MEDIUM` — satu bagian layar (daftar, modal form, hapus) pada detail yang sudah ada, ditambah dua perubahan kecil pada editor struktur dan grade |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` branch `yasmina`; laporan dan penandaan status di repository backend |
| Model | Claude Sonnet 5.5 |
| Commit frontend saat dikerjakan | `ff46d5db1` (branch `yasmina`); perubahan task ini belum di-commit |
| Commit backend yang dijadikan rujukan | `726234df` (branch `Yasmina`), working tree bersih |
| Tanggal | 2026-11-13 |
| Status | ✅ — source selesai dan 5 dari 5 kriteria terpetakan ke source; ESLint 0 galat; 27 uji unit baru lulus; suite penuh tanpa kegagalan baru; agen tidak membuka layar (`VISUAL CHECK: NOT FEASIBLE`, `MANUAL TEST: NOT FEASIBLE` oleh agen); pada 2026-11-13 pemilik melaporkan `npm run dev` berhasil dan seluruh uji lulus (laporan lisan, tanpa rincian per langkah, tanpa screenshot tersimpan; bukan bukti agen) lalu menginstruksikan status naik ke ✅; **`npm run build` tetap `NOT RUN`**; backend `PAY-BE-15` dan `PAY-BE-16` belum terbukti build dan uji API (masih 🟡) |

---

## 1. Keadaan yang ditemukan di awal

- **Backend** sudah menyediakan empat endpoint tunjangan tetap per struktur
  (`PayrollSalaryStructureAllowancesController`, `GET`, `POST`, `PUT /{id}`, `DELETE /{id}`) dengan
  aturan `SA-01`–`SA-07`, dan menolak batas struktur di luar rentang grade (`SR-01`–`SR-04`) pada
  `SalaryStructureController` dan `SalaryGradeController` lewat `PayrollSalaryRangeRuleService`.
- **Frontend** belum punya layarnya. Detail Struktur Gaji (`/payroll/master-data/salary-structure/[slug]`)
  hanya menampilkan kartu informasi (`BaseDetailView`) dengan tombol Kembali, Perbarui, dan Hapus.
  Tidak ada daftar tunjangan, dan pesan `SR-01`–`SR-04` hanya muncul sebagai toast "Gagal Menyimpan Data"
  tanpa menunjuk isian yang bermasalah.
- **Perilaku backend yang menentukan rancangan:**
  - Pesan penolakan dikirim sebagai teks bebas pada `message` (`ApiResponse.Fail`), tanpa kode aturan
    dan tanpa nama isian. Pemasangan pesan ke isian karena itu memakai awal kalimat pesan.
  - `PUT` membawa `expectedRowVersion` di body, `DELETE` membawanya sebagai query string. Token versi
    tidak cocok menghasilkan 409 (`SA-07`); jenis ganda juga 409 (`SA-04`), sehingga kedua kasus tidak
    bisa dibedakan dari kode status saja.
  - Daftar tidak berhalaman (`List<SalaryStructureAllowanceResponse>` urut nama jenis).
  - Respons detail struktur **tidak** memuat rentang grade (`SalaryGradeResponse.MinimumSalary` dan
    `MaximumSalary`), padahal skema `FE-PGJ-01` menyebut "rentang grade" pada wilayah Kepala.

## 2. Proses bisnis dari sisi pengguna

**Pengguna.** Petugas Payroll yang memegang hak `SalaryStructureAllowance` di Akses Role. Petugas HRD
yang hanya memegang `Read` dapat melihat daftar tanpa tombol tulis.

**Langkah:**

1. Petugas membuka Payroll → Data Induk → Struktur Gaji, lalu membuka detail sebuah struktur.
2. Di bawah kartu informasi muncul bagian **Tunjangan Tetap**, hanya bagi pemegang
   `SalaryStructureAllowance : Read`. Daftarnya memuat jenis tunjangan (beserta kode dan keterangan),
   cara hitung, nilai, batas maksimum, pajak, prorata, dan status.
3. Petugas menekan **+ Tambah Tunjangan** (hak `Create`), memilih **Jenis Tunjangan** dari daftar
   bercari, memilih **Cara Hitung**, lalu mengisi **Nominal per Bulan** (nominal tetap) atau
   **Persentase Gaji Pokok (%)** (persentase), dan keterangan bila perlu.
4. Bila isian tidak memenuhi `SA-01`–`SA-03`, pesan tampil di isiannya sebelum permintaan dikirim.
5. Petugas menyimpan. Modal tertutup, toast hijau muncul, dan daftar dimuat ulang. Batas maksimum,
   pajak, dan prorata ikut dari master Jenis Tunjangan dan tidak diisi di sini.
6. Baris dapat **Diubah** (hak `Update`, termasuk mematikan Status Aktif) atau **Dihapus** (hak
   `Delete`, dengan konfirmasi yang menyebut nama jenisnya).
7. Di form Struktur Gaji, batas bawah, batas atas, dan gaji dasar default yang melanggar rentang grade
   ditolak server; pesannya kini menempel di bawah isian yang bermasalah (selain toast yang sudah ada).
   Di form Grade Gaji, penolakan `SR-04` menempel di bawah isian Gaji Maksimum.

**Contoh** (data samaran): struktur "GR-05 Perawat Pelaksana". Petugas menambah Tunjangan Jabatan
nominal tetap Rp 750.000, lalu Tunjangan Keahlian sebesar 5% gaji pokok. Daftar menampilkan dua baris:
"Nominal tetap, Rp 750.000" dan "% gaji pokok, 5%". Bila petugas mengetik 150 pada persentase, isian
menampilkan "Persentase tunjangan harus di atas 0 dan paling tinggi 100." tanpa memanggil server.

**Jalur tidak normal:**

| Keadaan | Yang terjadi |
| --- | --- |
| Struktur belum punya tunjangan | Tabel menampilkan persis: "Struktur ini belum memuat tunjangan tetap." dan "Selama kosong, run memakai angka tunjangan di profil pegawai." |
| Daftar gagal dimuat | Kotak galat berisi "Tunjangan gagal dimuat." (ditambah sebab dari server bila ada) dan tombol **Coba lagi** |
| Server menjawab 403 saat memuat | Kotak galat "Anda tidak punya hak untuk tindakan ini." tanpa tombol coba lagi |
| Tanpa `SalaryStructureAllowance : Read` | Bagian tidak ditampilkan |
| Hanya `Read` | Daftar tampil tanpa Tambah, Ubah, Hapus |
| Jenis sudah ada di struktur (`SA-04`, 409) | Pesan server di bawah isian Jenis Tunjangan; modal tetap terbuka; daftar dimuat ulang sehingga baris yang sudah ada terlihat |
| Jenis dihitung dari aktivitas kerja (`SA-06`, 400) | Pesan server di bawah isian Jenis Tunjangan |
| Struktur atau jenis tidak aktif (`SA-05`, 400) | Pesan server sebagai galat umum di dalam modal (bisa menyangkut struktur, jadi tidak ditempel ke satu isian) |
| Baris diubah petugas lain (`SA-07`, 409) | Daftar dimuat ulang; bila baris itu berubah atau hilang, modal ditutup dan toast merah memuat pesan server |
| Hapus baris yang sudah diubah atau dihapus petugas lain (409/404) | Konfirmasi ditutup, toast merah, daftar dimuat ulang |
| Klik simpan dua kali | Kunci kirim dan tombol nonaktif sampai respons diterima |
| Peran tidak punya `AllowanceType : Read` | Daftar jenis tunjangan tidak termuat; modal menampilkan peringatan agar peran diberi hak membaca Jenis Tunjangan |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- **Governance:** `AGENTS.md` frontend; `rules/frontend/frontend-architecture.md`, katalog dan gerbang
  keputusan base component, pola komposisi halaman, checklist UI, standar fitur master data, template
  laporan; `rules/rule-output/status-task-roadmap.md`.
- **Blueprint:** `03-frontend-architecture.md`, `contracts/api-contract.md`,
  `contracts/validation-matrix.md`, `contracts/state-transition-matrix.md`,
  `contracts/permission-audit-matrix.md`, `roadmap/frontend-roadmap.md`, `roadmap/backend-roadmap.md`,
  `roadmap/requirement-traceability.md`, laporan `PAY-BE-16` dan `PAY-FE-15`.
- **Backend (baca-saja):** `PayrollSalaryStructureAllowancesController.cs`,
  `PayrollSalaryStructureAllowanceService.cs`, `PayrollSalaryStructureAllowanceDtos.cs`,
  `PayrollSalaryRangeRuleService.cs`, `PayrollControllerSupport.cs`, `SalaryStructureController.cs`,
  `SalaryGradeController.cs`, `SalaryStructureDtos.cs`, `AllowanceTypeController.cs`,
  `AllowanceTypeDtos.cs`, `MstSalaryStructureAllowance.cs`.
- **Rujukan frontend:** `salary-structure-detail-view.jsx`, `salary-structure-constants.jsx`,
  `use-master-data-salary-structure-detail.jsx`, `use-master-data-salary-structure-editor.jsx`,
  `use-master-data-salary-grade-editor.jsx`, `payroll-compensation-detail-view.jsx`,
  `payroll-transport-allowance-*` (hook, view, modal, kolom, service, konstanta, utils),
  `payroll-section.jsx`, `payroll-run.module.css`, `base-detail-view.jsx`, `base-form-control.jsx`,
  `base-editor-field.jsx`, `use-select-resource.jsx`, `hr-select-resources.js`, `data-table.jsx`,
  `input-normalizer-utils.jsx`, `field-normalizer-config.jsx`, `use-permission.jsx`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/payroll/payroll-salary-structure-allowance-constants.js` | Baru. Endpoint, hak akses, nilai cara hitung, batas isian, resource pilihan Jenis Tunjangan, dan seluruh salinan teks termasuk kalimat kosong dan gagal dari skema |
| `src/utils/payroll/payroll-salary-structure-allowance-utils.js` | Baru. Normalisasi respons, form, validasi yang mencerminkan `SA-01`–`SA-03`, payload (`expectedRowVersion` saat ubah), penormal isian persentase, deteksi data basi, pemetaan pesan `SA-01`–`SA-06` ke isian |
| `src/utils/payroll/payroll-salary-range-rejection-utils.js` | Baru. Pemetaan pesan `SR-01`–`SR-03` (form struktur) dan `SR-04` (form grade) ke isian batas |
| `src/lib/services/payroll/payroll-salary-structure-allowance-service.js` | Baru. Panggilan Axios lewat `InstanceAxios`: daftar, tambah, ubah, hapus (token versi sebagai query) |
| `src/lib/hooks/payroll/use-payroll-salary-structure-allowance.js` | Baru. Daftar, hak `Read`/`Create`/`Update`/`Delete`, modal form dengan kunci kirim ganda, hapus dengan kunci, penanganan 409 dan 404, toast; setiap permintaan daftar dibatalkan saat cleanup |
| `src/components/view/payroll/master-data/salary-structure/detail/salary-structure-allowance-section.jsx` | Baru. Bagian "Tunjangan Tetap": tabel, keadaan memuat, kosong, gagal, konfirmasi hapus, toast |
| `src/components/view/payroll/master-data/salary-structure/detail/salary-structure-allowance-form-modal.jsx` | Baru. Modal tambah/ubah |
| `src/components/view/payroll/master-data/salary-structure/detail/salary-structure-allowance-table-columns.jsx` | Baru. Kolom tabel dan tombol Ubah/Hapus yang dijaga hak |
| `src/components/view/payroll/master-data/salary-structure/detail/salary-structure-detail-view.jsx` | Diubah. Memasang bagian baru sebagai `children` `BaseDetailView` |
| `src/lib/hooks/payroll/master-data/salary-structure/use-master-data-salary-structure-detail.jsx` | Diubah (2 baris). Mengembalikan `resolvedId` yang sudah dihitung hook |
| `src/lib/hooks/payroll/master-data/salary-structure/use-master-data-salary-structure-editor.jsx` | Diubah. Saat simpan ditolak, pesan `SR-01`–`SR-03` dipasang sebagai galat isian batas; toast tetap |
| `src/lib/hooks/payroll/master-data/salary-grade/use-master-data-salary-grade-editor.jsx` | Diubah. Saat simpan ditolak, pesan `SR-04` dipasang sebagai galat isian Gaji Maksimum; toast tetap |
| `tests/unit/payroll-salary-structure-allowance.test.mjs` | Baru. 27 uji, termasuk pencocokan dengan source backend |

### 3.3 Kepatuhan arsitektur frontend

- **Pola:** sub-resource di dalam detail master data, bukan master data mandiri. Karena itu mengikuti
  pola Payroll V2 yang sama dengan `PAY-FE-15` (konstanta → utils → service → hook dengan state lokal →
  view), bukan standar tujuh berkas Redux master data. Tidak ada slice, route, atau butir menu baru:
  layar adalah bagian dari route detail Struktur Gaji yang sudah ada.
- **Alur dependensi:** route (tidak berubah) → view → hook → service/utils/constants. View tidak memanggil
  Axios. Utils murni (tanpa React, request, atau Redux).
- **Hak akses:** `usePermission` dengan resource `SalaryStructureAllowance`; tidak ada nama peran di kode.
- **Tanpa abstraksi baru:** tidak ada komponen shared, instance Axios, atau CSS Module baru.
- **Pola baru yang perlu disebut:** penormal isian persentase lokal (lihat 8, Masalah yang diketahui).

### 3.4 Gerbang tampilan

**Modul referensi visual:** `src/components/view/payroll/compensation/payroll-compensation-detail-view.jsx`
(`BaseDetailView` dengan bagian bertabel di `children`) dan layar `payroll-transport-allowance-*` dari
`PAY-FE-15` (modal form dan tombol per baris).

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Kerangka detail yang ada | `BaseDetailView` (slot `children`) | `REUSE` | Bawaan komponen |
| Bagian "Tunjangan Tetap" dan tombol Tambah di header bagian | `PayrollSection flush` — `payroll-run.module.css` | `REUSE` | Pembungkus Payroll yang sudah ada; padanan detail Profil Penggajian |
| Tabel daftar | `DataTable` (`pagination={false}`) — `tableStyles.regionDataTable` | `REUSE` | Pola detail Profil Penggajian |
| Lencana Aktif/Nonaktif | `StatusBadge isActive` | `REUSE` | Pola baku |
| Tombol Tambah, Ubah, Hapus, Coba lagi | `BaseButton` (`primary`, `secondary`, `danger`) | `REUSE` | `PAY-FE-15` |
| Form tambah/ubah | `ConfirmModal size="lg"` berisi `BaseSelectField`, `BaseTextField`, `BaseTextAreaField`, `BaseSwitchField` | `REUSE` | `PAY-FE-15` |
| Pilihan Jenis Tunjangan | `BaseSelectField` + `useSelectResource("allowanceTypes")` | `COMPOSE` | Resource sudah terdaftar di `hr-select-resources.js`; dirangkai sama seperti `base-editor-field.jsx`. Cari dan muat bertahap dari server |
| Peringatan, galat, galat dalam modal | `InformationAlert`, `PayrollActionError` | `REUSE` | Dipakai di seluruh layar Payroll |
| Konfirmasi hapus, toast | `ConfirmModal`, `ToastStack` | `REUSE` | Pola baku |
| Tema warna tabel dan lencana | `themeStyles.regionTheme` hanya di pembungkus bagian baru | `REUSE` | Tabel dan lencana Region membutuhkan variabel `--region-*`; dipasang di pembungkus bagian, bukan seluruh halaman, supaya kartu detail yang sudah ada tidak berubah |

**Pilihan yang dicatat (`COMPOSE`, bukan `NEW`):** (A, dipakai) `BaseSelectField` + `useSelectResource`:
pencarian dan muat bertahap dari server, tidak ada jenis yang terpotong. (B) memuat 100 opsi sekali
seperti `PAY-FE-15`: lebih sederhana, tetapi jenis ke-101 dan seterusnya tidak dapat dipilih tanpa
pemberitahuan.

`UI GATE: PASS — 9 elemen: REUSE 8, EXTEND 0, COMPOSE 1, WRAP 0, NEW 0`

Hasil pemeriksa dan grep anti-regresi, ditempel apa adanya:

```text
$ node ../QuilvianEngineeringSkills/Claude/.claude/rules/frontend/tools/check-css-usage.mjs \
    .../detail/salary-structure-allowance-section.jsx \
    .../detail/salary-structure-allowance-form-modal.jsx \
    .../detail/salary-structure-allowance-table-columns.jsx \
    .../detail/salary-structure-detail-view.jsx
OK — 4 berkas diperiksa, 0 temuan.

$ grep -nE "<button|className=\"btn|btn-primary|btn-secondary" <3 berkas baru>          (kosong)
$ grep -n "<table" <3 berkas baru>                                                      (kosong)
$ grep -nE "fw-(bold|semibold|light)|fs-[0-9]" <3 berkas baru>                          (kosong)
$ grep -nE "<pola #7 class Tailwind>" <3 berkas baru>                                   (kosong)
$ grep -nE "fixed inset-0|position: ?fixed" <3 berkas baru>                             (kosong)
$ grep -nE "<(input|select|textarea)\b|UUID|GUID" <3 berkas baru>                       (kosong)
$ grep -nE "variant=\"(outline|danger-outline|default|link|light|dark)\"|<StatusBadge[^>]*(status=\{[^}]*label|variant=)" <3 berkas baru>   (kosong)
$ grep -n "style={{" <3 berkas baru>                                                    (kosong)
$ grep -nEi "#[0-9a-f]{3,8}\b|rgba?\(" <3 berkas baru>                                  (kosong)
```

Tidak ada stylesheet baru, sehingga grep warna literal, typography, dan `!important` pada berkas gaya
tidak berlaku (`NOT APPLICABLE`).

| Layar | Screenshot | Catatan perbandingan dengan referensi |
| --- | --- | --- |
| `/payroll/master-data/salary-structure/[slug]` | `VISUAL CHECK: NOT FEASIBLE` | Dev server, build, dan skrip peramban tidak dijalankan: pemilik meminta agen tidak menyalakannya otomatis, dan agen tidak memegang sesi login. Susunan mengikuti detail Profil Penggajian, tetapi **tampilannya belum pernah dilihat** |

Catatan 2026-11-13: pemilik menguji layar di peramban lewat `npm run dev` dan melaporkan lulus; tidak ada screenshot yang disimpan, sehingga `VISUAL CHECK: NOT FEASIBLE` di atas tetap berlaku bagi agen.

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tabel "Mengambil tunjangan tetap..."; tombol Tambah dan Ubah/Hapus nonaktif; pilihan jenis di modal menampilkan "Memuat jenis tunjangan..." |
| Kosong | "Struktur ini belum memuat tunjangan tetap." dan "Selama kosong, run memakai angka tunjangan di profil pegawai." |
| Gagal | "Tunjangan gagal dimuat." (ditambah sebab server bila ada) dengan tombol **Coba lagi**; bila 403: "Anda tidak punya hak untuk tindakan ini." |
| Tanpa hak akses | Bagian tidak tampil tanpa `Read`; Tambah, Ubah, Hapus tidak tampil tanpa `Create`, `Update`, `Delete`; penolakan server pada aksi tampil sebagai pesan di modal atau toast |
| Berisi | Tabel: jenis (nama, kode, keterangan), cara hitung, nilai, batas maksimum, pajak, prorata, status, aksi. Nominal memakai mata uang struktur |
| Data basi (409) | Daftar dimuat ulang; modal ditutup dengan toast bila baris berubah atau hilang, tetap terbuka dengan pesan di isian bila baris sama |

## 5. Endpoint yang dikonsumsi

#### Corporate / Payroll Management / Master Data / Salary Structure Allowance

Base URL: `/v1/corporate/payroll-management/master-data/salary-structures/{salaryStructureId}/allowances`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Daftar tunjangan satu struktur (tanpa halaman) | `SalaryStructureAllowance : Read` |
| `POST` | `/` | Tambah satu jenis tunjangan | `SalaryStructureAllowance : Create` |
| `PUT` | `/{id}` | Ubah cara hitung, nilai, keterangan, status aktif; membawa `expectedRowVersion` | `SalaryStructureAllowance : Update` |
| `DELETE` | `/{id}?expectedRowVersion=...` | Hapus (ditandai) | `SalaryStructureAllowance : Delete` |

#### Corporate / Payroll Management / Master Data / Allowance Type

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/master-data/allowance-types/options` | Pilihan Jenis Tunjangan di modal (lewat registry `allowanceTypes`; rute HR dan rute Payroll milik controller yang sama) | `AllowanceType : Read` |

#### Corporate / Payroll Management / Master Data / Salary Structure

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST`, `PUT` | `/v1/corporate/payroll-management/master-data/salary-structures` dan `/{id}` | Tidak ada panggilan baru; penolakan `SR-01`–`SR-03` dari respons ini kini ditempel ke isian batas | `SalaryStructure : Create`, `SalaryStructure : Update` |

#### Corporate / Payroll Management / Master Data / Salary Grade

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `PUT` | `/v1/corporate/payroll-management/master-data/salary-grades/{id}` | Tidak ada panggilan baru; penolakan `SR-04` (409) kini ditempel ke isian Gaji Maksimum | `SalaryGrade : Update` |

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` pada 12 berkas source yang baru atau berubah | 0 galat, 4 peringatan `react-hooks/set-state-in-effect`: 3 pada baris warisan (hook detail baris 81, editor struktur baris 108, editor grade baris 110, semuanya `setResolvedId` di efek), 1 pada hook baru dengan pola yang sama seperti hook Payroll lain | `PASS` (peringatan: `EXISTING WARNING` 3, pola yang sama 1) | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/payroll-salary-structure-allowance.test.mjs` | 27 uji, 27 lulus, 0 dilewati (source backend ada, jadi pencocokan ke backend ikut berjalan) | `PASS` | Keluaran perintah |
| Pencocokan alamat dengan `[Route]` controller backend | Cocok | `PASS` | Uji "alamat endpoint cocok dengan [Route] controller backend" |
| Pencocokan hak akses dengan `ControllerName` dan `[AccessPermission]`, nilai cara hitung dengan `SalaryStructureAllowanceCalculationMethods`, awal kalimat pesan `SA` dan `SR` dengan source service backend | Cocok | `PASS` | Empat uji pada berkas yang sama |
| Validasi `SA-01`–`SA-03` dan payload (kolom tak terpakai 0, token versi saat ubah) | Sesuai | `PASS` | Uji "SA-02", "SA-03", "payload tambah", "payload ubah" |
| Mengetik persentase karakter demi karakter (simulasi `BaseTextField`) | "2" "," "5" menjadi 2,5; 150 tidak dipotong dan ditolak `SA-03` | `PASS` | Dua uji penormal persentase |
| Deteksi data basi (`SA-07`) | Baris berubah atau hilang dianggap basi; baris sama tidak | `PASS` | Uji "data basi" |
| Pemetaan pesan penolakan ke isian | `SA-01`–`SA-04`, `SA-06`, `SR-01`–`SR-04` terpasang ke isian yang benar; `SA-05`, `SA-07`, dan pesan lain tidak ditebak | `PASS` | Uji "SA-01..SA-06", "SR-01..SR-03" |
| Pemeriksa statis sekali pakai (di luar repo): setiap simbol impor di 12 berkas source terpetakan ke ekspor modul tujuannya | Seluruh berkas baru bersih. 9 temuan semu pada baris impor warisan (ekspor hasil destructuring `slice.actions`) | `PASS` | Keluaran skrip |
| Suite unit penuh (`node --import ./tests/helpers/register.mjs --test tests/unit/*.test.mjs`) | 4.827 uji, 4.776 lulus, 51 gagal pada 35 berkas; tidak satu pun menyebut Struktur Gaji, Grade, atau tunjangan | `EXISTING / ENVIRONMENT ISSUE` (51), `PASS` (27 uji baru) | Keluaran perintah |
| Pembanding: suite yang sama pada ekspor `HEAD` murni (`git archive`, di luar repo, sudah dihapus) | Seluruh 51 kegagalan di working tree **juga gagal** pada `HEAD` murni; 0 kegagalan hanya di working tree. Pada salinan itu 4 uji tambahan gagal; diklasifikasikan artefak salinan, bukan masalah kode: 3 membaca berkas akar (`version.json`, `next.config.js`) yang tidak ikut diarsipkan, dan 1 membaca berkas sumber yang ada dan terlacak di `HEAD` (kemungkinan tidak terekstrak karena path scratch yang panjang di Windows; tidak diselidiki lebih jauh). Tidak ada yang menyangkut Payroll | `PASS` (tanpa kegagalan baru) | Selisih himpunan nama uji |
| Grep anti-regresi UI dan `check-css-usage.mjs` | 0 temuan | `PASS` | Bagian 3.4 |
| `npm run build` | Tidak dijalankan | `NOT RUN` | — |
| `npm run dev` dan uji manual oleh pemilik (2026-11-13) | Dilaporkan berhasil dan seluruh uji lulus. Tidak dirinci: layar dan akun yang dipakai, hasil per langkah uji; tidak ada screenshot tersimpan | `OWNER-REPORTED` — tidak diverifikasi agen; bukan pengganti `npm run build` | Pesan pemilik |
| Pembukaan layar di peramban dan screenshot | Tidak dilakukan | `NOT FEASIBLE` | Bagian 3.4 |

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/payroll-salary-structure-allowance.test.mjs — PASS (27/27)`

Uji manual: `NOT FEASIBLE` — agen tidak menyalakan `npm run dev` atau skrip peramban, dan tidak memegang
sesi login.

**Langkah uji untuk pemilik** (setelah `npm run dev`; akun dengan `SalaryStructureAllowance : *`,
`AllowanceType : Read`, `SalaryStructure : Read/Update`, `SalaryGrade : Update`):

1. Buka detail Struktur Gaji (misalnya "GR-05 Perawat Pelaksana"). Harapan: bagian **Tunjangan Tetap**
   muncul di bawah kartu. Pada struktur tanpa tunjangan, tabel menampilkan kalimat kosong persis.
2. Tambah: pilih Tunjangan Jabatan, Nominal tetap, Rp 750.000. Harapan: tersimpan, toast hijau, baris
   tampil dengan Pajak dan Prorata dari master.
3. Tambah Tunjangan Keahlian, % gaji pokok, ketik 150. Harapan: "Persentase tunjangan harus di atas 0 dan
   paling tinggi 100." di isian, tanpa permintaan ke server. Ketik 2,5 (harus bisa diketik), lalu 5, simpan.
   Harapan: nilai tampil "5%"; batas maksimum dari master.
4. Tambah jenis yang sama lagi. Harapan: pesan `SA-04` di bawah isian Jenis Tunjangan; modal tetap
   terbuka.
5. Pilih jenis yang dipakai kebijakan Shift, Siaga, atau Risiko aktif. Harapan: pesan `SA-06` di bawah
   isian Jenis Tunjangan.
6. Buka struktur yang sama di dua tab. Ubah baris yang sama di tab A dan simpan, lalu ubah dan simpan di
   tab B. Harapan: tab B menampilkan toast "Data ini baru saja diubah petugas lain. Muat ulang lalu coba
   lagi.", modal tertutup, daftar menampilkan nilai terbaru.
7. Hapus baris. Harapan: konfirmasi menyebut nama jenis; setelah dikonfirmasi baris hilang. Hapus baris
   yang sudah dihapus di tab lain: toast dan daftar dimuat ulang.
8. Akun yang hanya punya `Read`: daftar tampil tanpa Tambah, Ubah, Hapus. Akun tanpa `Read`: bagian tidak
   tampil.
9. Matikan backend lalu muat ulang detail. Harapan: "Tunjangan gagal dimuat." dengan tombol Coba lagi.
10. Form Perbarui Struktur Gaji: isi Gaji Dasar Maksimum melebihi batas atas grade. Harapan: pesan `SR-02`
    di bawah isian itu (selain toast). Ulangi untuk Gaji Dasar Minimum (`SR-01`) dan Gaji Dasar Default
    (`SR-03`).
11. Form Perbarui Grade Gaji: persempit rentang sehingga ada struktur aktif di luar batas. Harapan: pesan
    `SR-04` di bawah isian Gaji Maksimum.

Uji manual oleh pemilik (2026-11-13): `OWNER-REPORTED`, lulus. Pemilik menjalankan `npm run dev` dan melaporkan seluruh uji lulus; hasil per langkah tidak dirinci dan tidak diverifikasi agen. `npm run build` tetap `NOT RUN`.

**Tidak dijalankan oleh agen:** `npm run build`, `npm run dev`, uji e2e, uji manual dan pemeriksaan visual di
peramban, `dotnet build` backend.

## 7. Acceptance criteria dan Definition of Done

Ringkasan: **5 dari 5** kriteria terpetakan ke source dan bukti statis; **5 dari 5** dinyatakan lulus oleh pemilik di peramban (2026-11-13, tanpa rincian per langkah; bukan bukti agen).

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Daftar tunjangan menampilkan jenis, cara hitung, nilai, batas maksimum, pajak, prorata | Terpenuhi di source; pengujian pemilik: lulus (2026-11-13, tanpa rincian) | `salary-structure-allowance-table-columns.jsx` (kolom Jenis Tunjangan, Cara Hitung, Nilai, Batas Maksimum, Pajak, Prorata; ditambah Status dan Aksi) |
| 2. Keadaan kosong berbunyi persis sesuai skema | Terpenuhi di source | Uji "kalimat kosong dan gagal sama persis dengan skema FE-PGJ-01"; `emptyTitle` dan `emptyDescription` pada `DataTable` |
| 3. Tombol dijaga `SalaryStructureAllowance : Create/Update/Delete` | Terpenuhi di source | Hook: `Create` untuk Tambah, `Update` untuk Ubah, `Delete` untuk Hapus; uji kecocokan nama hak dengan controller |
| 4. Pesan penolakan server tampil di dekat isian | Terpenuhi di source untuk `SA-01`–`SA-04`, `SA-06`, `SR-01`–`SR-04`; `SA-05` dan `SA-07` sengaja tidak ditempel ke satu isian (lihat bagian 2) | Uji pemetaan; `presentFailure` pada hook; perubahan pada editor struktur dan grade |
| 5. Benturan 409 memuat ulang data | Terpenuhi di source; pengujian pemilik: lulus (2026-11-13, tanpa rincian) | `recoverFromConflict` dan `confirmDelete` pada hook; uji deteksi data basi |
| DoD: kriteria terbukti | Terpenuhi menurut pernyataan pemilik (2026-11-13); build produksi belum (`NOT RUN`) | `OWNER-REPORTED`; bagian 6 |
| DoD: laporan ditulis | Terpenuhi | Berkas ini |
| Verifikasi roadmap: lint | Terpenuhi (0 galat) | Bagian 6 |
| Verifikasi roadmap: build | **Belum** | `NOT RUN` |
| Verifikasi roadmap: uji manual dengan contoh struktur GR-05 | Terpenuhi menurut pernyataan pemilik (2026-11-13), tanpa rincian per langkah | `OWNER-REPORTED` |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 4 peringatan ESLint `react-hooks/set-state-in-effect`; 3 pada baris warisan, 1 pada hook baru dengan pola yang sama seperti hook Payroll V2 lain. Tidak diperbaiki di luar cakupan |
| Selisih desain dengan backend | Skema `FE-PGJ-01` menyebut "rentang grade" pada wilayah Kepala, tetapi respons detail struktur tidak memuatnya. Kepala tidak diubah (di luar cakupan kartu `PAY-FE-16`: "Bagian Tunjangan Tetap; pesan `SR-01`–`SR-04`"). Pilihan perbaikan: tambah dua kolom rentang grade pada `SalaryStructureDetailResponse` (backend), atau frontend memanggil detail grade (perlu `SalaryGrade : Read`). Perlu keputusan pemilik |
| Pesan penolakan dipasangkan lewat awal kalimat | Backend mengirim `SA-xx` dan `SR-xx` sebagai teks bebas tanpa kode. Pemetaan ke isian memakai awal kalimat, dan uji unit mengunci awal kalimat itu terhadap source backend. Bila backend mengubah kalimatnya, pesan tetap tampil lewat toast dan galat umum; yang hilang hanya penempelan ke isian. Solusi tahan lama: kode aturan pada respons galat (backend) |
| Hak akses yang harus diberikan admin | Usulan peran Staf Payroll di `permission-audit-matrix.md` bagian 3 **tidak memuat** `AllowanceType : Read` dan `SalaryStructure : Read`, padahal modal membutuhkan yang pertama untuk memilih jenis dan layar detail membutuhkan yang kedua. Tanpa `AllowanceType : Read`, modal menampilkan peringatan dan daftar jenis kosong. Perlu ditambahkan pada usulan peran |
| Masalah yang diketahui | (1) `FIELD_NORMALIZER.decimal` bersama menelan tanda pemisah saat mengetik ("2", ",", "5" menjadi "25"), sehingga pecahan seperti 2,5% tidak bisa diketik. Task ini memakai penormal lokal (`normalizeAllowancePercentageInput`) dan tidak mengubah penormal bersama yang dipakai banyak layar; cacat itu tercatat sebagai utang teknis. (2) Kolom **Status** dan keterangan di bawah nama jenis ditambahkan melebihi kolom pada skema, karena kontrak membawa `isActive` dan `notes`. (3) Toast bagian ini berdiri sendiri, terpisah dari toast halaman detail; bila keduanya muncul bersamaan dapat bertumpuk. (4) Detail memanggil `router.replace` untuk mengganti id dengan token pada tautan; bila itu membuat halaman dipasang ulang, daftar tunjangan dimuat dua kali. Tidak dibuktikan di peramban. (5) Pilihan jenis tidak menyingkirkan jenis yang sudah ada di struktur; `SA-04` yang menolaknya |
| Dependency backend | `PAY-BE-15` dan `PAY-BE-16` 🟡: source dibaca dan dicocokkan, tetapi `dotnet build` dan uji API belum dijalankan pemilik. Seluruh perilaku layar bergantung pada backend itu berjalan |
| Perubahan sampingan | `NONE`. Salinan `HEAD` untuk pembanding suite dibuat di folder scratch di luar repo dan sudah dihapus |
| Interupsi | `NONE` |
| Status Git | Frontend: ` M` `salary-structure-detail-view.jsx`, ` M` `use-master-data-salary-structure-detail.jsx`, ` M` `use-master-data-salary-structure-editor.jsx`, ` M` `use-master-data-salary-grade-editor.jsx`; `??` 3 berkas view bagian tunjangan, konstanta, hook, service, 2 utils, dan `tests/unit/payroll-salary-structure-allowance.test.mjs`. Tidak ada yang di-stage atau di-commit. Backend: hanya laporan ini serta tanda status pada roadmap dan `requirement-traceability.md` |
| Langkah berikutnya | Status ✅ atas instruksi pemilik (2026-11-13). Sisa: `npm run build` produksi (`NOT RUN`) dan pembuktian build serta uji API backend `PAY-BE-15` dan `PAY-BE-16` (masih 🟡). |
