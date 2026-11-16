# Laporan Perubahan Frontend — `PAY-FE-19`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-FE-19` |
| Judul | Rekap Lembur dan Tarif Lembur |
| Slice | `MVP-1` — `EPIC PGJ-07`, layar `FE-PGJ-05` dan `FE-PGJ-06` |
| Roadmap | `docs/module-blueprints/payroll-management/payroll-gaji/roadmap/frontend-roadmap.md`, kartu `PAY-FE-19` |
| Trace | FR-PGJ-060, 061; PRL-DEC-42; `../../../03-frontend-architecture.md` bagian 1, 2 (peta butir menu), 3.4, 6; `../../../contracts/api-contract.md` (Payroll Run tambahan `GET /{id}/overtime-inputs`; Overtime Rate HR) |
| Contract version | `PGJ-C1` — `approved` (Yasmin, 2026-11-12). Endpoint tarif milik HR dan sudah ada sebelum `PGJ-C1` |
| Wewenang UI | Isi layar, kolom, sumber data, hak akses butir menu, dan kalimat kosong dikunci `03-frontend-architecture.md` bagian 3.4. Bentuk layar dan komponen `DEV_DISCRETION` lewat gerbang keputusan base component. Kolom tarif "sesuai endpoint HR" ditentukan agen dari DTO HR (bagian 8) |
| Dependency | `PAY-FE-17` 🟡 (tombol jalan masuk di detail run) dan `PAY-BE-23` 🟡 — source backend sudah ada dan dibaca langsung dari working tree backend, tetapi `dotnet build` dan uji API belum dijalankan pemilik. Aturan roadmap meminta prasyarat ✅ sebelum task dimulai; task ini dikerjakan atas instruksi pemilik pada 2026-11-13, sama seperti `PAY-FE-16`–`18` |
| Klasifikasi | `MEDIUM` — dua layar daftar baca-saja, dua endpoint baca, satu grup dan butir menu, satu tombol masuk, sepuluh berkas source baru, empat berkas source yang sudah ada diubah, dan satu tes kontrak diperbarui |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` branch `yasmina`; laporan dan penandaan status di repository backend |
| Model | Claude Sonnet 5.5 |
| Commit frontend saat dikerjakan | `ff46d5db1` (branch `yasmina`); perubahan task ini dan `PAY-FE-16`–`18` belum di-commit |
| Commit backend yang dijadikan rujukan | `726234df` (branch `Yasmina`). Source yang dibaca untuk task ini (controller, service, DTO rekap lembur; controller, DTO, dan konstanta tarif lembur HR) tidak berubah dari commit itu |
| Tanggal | 2026-11-13 |
| Status | ✅ — source selesai dan 4 dari 4 kriteria terpetakan ke source; ESLint 0 galat; 18 uji unit baru lulus dan satu tes kontrak diperbarui; suite penuh tanpa kegagalan baru; agen tidak membuka layar (`VISUAL CHECK: NOT FEASIBLE`, `MANUAL TEST: NOT FEASIBLE` oleh agen); pada 2026-11-13 pemilik melaporkan `npm run dev` berhasil dan seluruh uji lulus (laporan lisan, tanpa rincian per langkah, tanpa screenshot tersimpan; bukan bukti agen) lalu menginstruksikan status naik ke ✅; **`npm run build` tetap `NOT RUN`**; backend `PAY-BE-23` belum terbukti build dan uji API (masih 🟡) |

---

## 1. Keadaan yang ditemukan di awal

- **Backend rekap lembur** (`PAY-BE-23`): `GET /v1/corporate/payroll-management/payroll-runs/{id}/overtime-inputs` (hak
  `PayrollRun : Read`), dengan query `search`, `pageNumber`, `pageSize` (paling banyak 200). Respons berhalaman berisi
  per baris: pegawai, tanggal lembur, menit terverifikasi, pengali, tarif per jam, dan nominal. Saringannya sengaja sama
  dengan mesin hitung (hanya baris aktif), sehingga rekap cocok dengan angka "Lembur" pada slip.
- **Backend tarif lembur** (milik HR, sudah ada): `GET /v1/corporate/human-resource/master-data/overtime-rates` (hak
  `OvertimeRate : Read`) dengan saringan `search`, `dayType`, `isActive`, `sortBy`, `sortDirection`, dan halaman.
  Respons memuat kode, nama, kebijakan, jenis hari, rentang waktu, metode hitung, pengali, nominal tetap, rentang menit
  atau jam, syarat menit, prioritas, masa berlaku, dan status. Urutan bawaan: prioritas naik.
- **Frontend**: layar tarif lembur hanya ada di modul HR (`/hr/master-data/overtime-rate`, master data penuh dengan
  tambah, ubah, hapus berbasis Redux). Payroll belum punya layar rekap maupun tarif, belum punya grup menu "Lembur", dan
  detail Payroll Run belum punya jalan masuk ke rekap.
- **Hal yang menentukan rancangan:**
  - Resource hak akses tarif (`OvertimeRate`) sama dengan resource layar HR. Layar Payroll tidak boleh bisa mengubah
    tarif, walau penggunanya mungkin juga memegang `OvertimeRate : Update` di HR.
  - Backend rekap tidak memuat total; total hanya dapat dihitung dari semua halaman, jadi layar tidak membuat total.
  - `PAY-BE-23` mencatat satu keputusan pemilik yang belum diambil: hak rekap (`PayrollRun : Read`, sesuai kontrak) lebih
    longgar daripada detail pegawai run (`ReadEmployeeDetail`).

## 2. Proses bisnis dari sisi pengguna

**Pengguna.** Petugas atau Kepala Payroll. Rekap dibuka dengan `PayrollRun : Read`; tarif lembur dengan
`OvertimeRate : Read` (hak ini perlu diberikan admin kepada peran Payroll).

**Langkah — Rekap Lembur:**

1. Petugas membuka detail Payroll Run. Pemegang `PayrollRun : Read` melihat tombol **Rekap Lembur** di header, di samping
   **Penghasilan Variabel** dan **Kembali**.
2. Layar menampilkan nomor run, kotak cari (nama atau nomor pegawai), pilihan jumlah baris, dan tabel: pegawai (nama dan
   nomor), tanggal lembur, menit terverifikasi (dengan jam di bawahnya bila satu jam atau lebih), pengali, tarif per jam,
   dan nominal. Urutan: nama pegawai lalu tanggal.
3. Tidak ada tombol tulis. Lembur diverifikasi di modul HR dan masuk ke run lewat impor data HRD.

**Langkah — Tarif Lembur:**

1. Petugas membuka sidebar Payroll → **Lembur** → **Tarif Lembur**. Butir hanya tampil bagi pemegang
   `OvertimeRate : Read`.
2. Di atas daftar selalu tampil: "Tarif lembur diubah di modul HR, bukan di sini. Payroll hanya membacanya."
3. Daftar menampilkan tarif menurut prioritas: nama dan kode, kebijakan lembur, jenis hari, rentang waktu (dengan jam atau
   menit dan syarat menit), perhitungan (metode dan nilainya), masa berlaku, prioritas, dan status. Saringan: cari, Jenis
   Hari, Status, jumlah baris.
4. Tidak ada tombol tambah, ubah, atau hapus.

**Contoh** (data samaran, `UAT-21`): run Mei memuat lembur Sari Wulandari pada 12 Mei, 90 menit (1 jam 30 menit),
pengali 1,5×, tarif per jam Rp 40.000, nominal Rp 90.000. Baris yang sama menjadi dasar angka "Lembur" pada slip Sari.

**Jalur tidak normal:**

| Keadaan | Yang terjadi |
| --- | --- |
| Run belum punya lembur terverifikasi | Tabel menampilkan persis: "Belum ada lembur terverifikasi yang diimpor ke run ini." |
| Belum ada tarif | Tabel menampilkan persis: "Belum ada tarif lembur." |
| Daftar gagal dimuat | Kotak galat (pesan server atau "Rekap lembur gagal dimuat." / "Tarif lembur gagal dimuat.") dan tombol **Coba lagi** |
| Server menjawab 403 | Halaman diganti pesan akses ditolak standar (`AccessDeniedGate`) |
| Tanpa `PayrollRun : Read` | Tombol **Rekap Lembur** dan layar rekap tidak tampil |
| Tanpa `OvertimeRate : Read` | Butir menu **Tarif Lembur** tidak tampil; layar tidak ditampilkan bila dibuka langsung (`UAT-22`) |
| Peran tanpa `PayrollRun : ReadDetail` | Nomor run tidak tampil di judul kecil; rekap tetap berjalan |
| Pencarian tanpa hasil | Keadaan kosong yang sama |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- **Governance:** `AGENTS.md` frontend; `rules/frontend/*` (arsitektur, katalog dan gerbang keputusan base component,
  pola komposisi halaman, checklist UI, template laporan); `rules/rule-output/status-task-roadmap.md`.
- **Blueprint:** `03-frontend-architecture.md` (bagian 2 dan 3.4), `../../02-module-map.md` (peta menu),
  `contracts/api-contract.md`, `contracts/permission-audit-matrix.md`, `testing/acceptance-test-matrix.md` (`UAT-21`,
  `UAT-22`), `roadmap/*`, laporan `PAY-BE-23`, `PAY-FE-17`, `PAY-FE-18`.
- **Backend (baca-saja):** `PayrollRunsController.cs`, `PayrollRunService.cs` (`GetOvertimeInputsAsync`),
  `PayrollRunDtos.cs`, `OvertimeRateController.cs`, `OvertimeRateDtos.cs`, `OvertimeValueConstants.cs`.
- **Rujukan frontend:** `overtime-rate-constants.jsx` (HR; label dan kolom), `master-data-overtime-rate-view.jsx`,
  `payroll-transport-allowance-view.jsx`, `payroll-variable-input-view.jsx`, `payroll-run-detail-view.jsx`,
  `use-payroll-run-detail.js`, `payroll-run-service.js`, `payroll-common-utils.js`, `payroll-constants.js`,
  `menu-items.jsx`, `payroll-module-contract.test.mjs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/payroll/payroll-overtime-constants.js` | Baru. Endpoint, rute, hak akses, label (dari konstanta HR), saringan, seluruh salinan teks termasuk kalimat kosong dari skema |
| `src/utils/payroll/payroll-overtime-utils.js` | Baru. Normalisasi rekap dan tarif, teks menit, pengali, nilai tarif menurut metode, rentang, syarat menit, masa berlaku |
| `src/lib/services/payroll/payroll-overtime-service.js` | Baru. Dua panggilan baca lewat `InstanceAxios`; tidak ada panggilan tulis |
| `src/lib/hooks/payroll/use-payroll-run-overtime.js` | Baru. Rekap: daftar, pencarian, halaman, nomor run, kembali |
| `src/lib/hooks/payroll/use-payroll-overtime-rates.js` | Baru. Tarif: daftar, saringan, halaman |
| `src/components/view/payroll/overtime/payroll-run-overtime-view.jsx` | Baru. Layar Rekap Lembur |
| `src/components/view/payroll/overtime/payroll-overtime-rate-view.jsx` | Baru. Layar Tarif Lembur |
| `src/components/view/payroll/overtime/payroll-overtime-table-columns.jsx` | Baru. Kolom rekap dan kolom tarif |
| `src/app/payroll/payroll-runs/[id]/overtime/page.jsx` | Baru. Route tipis |
| `src/app/payroll/overtime-rates/page.jsx` | Baru. Route tipis |
| `src/lib/constants/payroll/payroll-constants.js` | Diubah (1 baris). `PAYROLL_ROUTES.overtimeRates` |
| `src/utils/menu-sidebar/menu-items.jsx` | Diubah. Grup "Lembur" dan butir "Tarif Lembur" dijaga `OvertimeRate : Read` |
| `src/lib/hooks/payroll/use-payroll-run-detail.js` | Diubah. `canOpenOvertime` (`PayrollRun : Read`) |
| `src/components/view/payroll/run/payroll-run-detail-view.jsx` | Diubah. Tombol **Rekap Lembur** di header |
| `tests/unit/payroll-module-contract.test.mjs` | Diubah. Daftar menu Payroll yang dikunci tes kini memuat grup "Lembur" (di antara Proses Penggajian dan Potongan dan Penyesuaian) serta butir "Simulasi Penggajian" dan "Tarif Lembur" |
| `tests/unit/payroll-overtime.test.mjs` | Baru. 18 uji, termasuk pencocokan dengan source backend |

### 3.3 Kepatuhan arsitektur frontend

- **Pola:** layar daftar baca-saja Payroll V2 (service → hook dengan state lokal → view), sama dengan `PAY-FE-15`–`18`. Tidak
  ada slice, instance Axios, komponen shared, atau CSS Module baru.
- **Alur dependensi:** route → view → hook → service/utils/constants. View tidak memanggil Axios; utils murni.
- **Layar tarif berdiri sendiri, tidak menyentuh HR:** layar HR yang sudah ada (master data penuh berbasis Redux) tidak
  diubah. Payroll membaca endpoint HR lewat service sendiri; label jenis hari, rentang waktu, dan metode hitung dipakai
  ulang dari konstanta HR supaya istilahnya sama. Tidak ada panggilan tulis ke endpoint HR (diuji).
- **Hak akses:** `usePermission` dengan `PayrollRun : Read` (rekap, tombol masuk) dan `OvertimeRate : Read` (tarif, butir
  menu); tidak ada nama peran di kode.
- **Menu:** sesuai peta menu bagian 3.1 blueprint: grup tingkat 1 "Lembur" dengan satu butir "Tarif Lembur". Rekap Lembur
  tanpa butir menu (layar anak dari detail run).
- **Tes kontrak Payroll diperbarui, bukan dilonggarkan:** tes mengunci daftar tepat menu Payroll. Penambahan grup "Lembur"
  adalah perubahan yang disengaja menurut blueprint; tes kini mengunci urutan baru dan tetap meminta setiap butir punya hak
  menu dan halaman nyata.

### 3.4 Gerbang tampilan

**Modul referensi visual:** `payroll-transport-allowance-view.jsx` dan `payroll-variable-input-view.jsx` (daftar Payroll
tanpa tombol tulis), `master-data-overtime-rate-view.jsx` (isi kolom tarif).

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Header, tombol Kembali ke Detail Run | `Hero`, `BaseButton` | `REUSE` | — |
| Pencarian, saringan Jenis Hari dan Status, jumlah baris | `DataFilter`, `FilterSelect` | `REUSE` | Pola daftar baku |
| Tabel berhalaman server (rekap dan tarif) | `DataTable`, `Pagination` — `tableStyles.regionDataTable` | `REUSE` | Pola daftar baku; urutan dipegang server (`sortLatestFirst={false}`) |
| Lencana status tarif | `StatusBadge` | `REUSE` | Pola baku |
| Keterangan "diubah di modul HR", galat, 403 | `InformationAlert`, `AccessDeniedGate`, `BaseButton` | `REUSE` | Pola daftar baku |
| Teks bertumpuk di sel (nama + nomor, nilai + pendamping) | `payroll-run.module.css` (`cellStack`, `amountCell`, `wrapText`) | `REUSE` | CSS Module Payroll yang sudah ada |
| Tombol **Rekap Lembur** di detail run | `BaseButton` pada `actions` `Hero` | `REUSE` | Pola tombol masuk `PAY-FE-15`, `PAY-FE-17` |
| Grup menu "Lembur" dan butir "Tarif Lembur" | `menu-items.jsx` (`RiTimeLine`) | `REUSE` | Pola menu Payroll |

**Pilihan yang dicatat (`REUSE`, bukan `NEW`):** sumber layar tarif — A (dipakai) layar Payroll baru yang hanya membaca
endpoint HR dengan hook dan service Payroll; B bendera baca-saja pada layar master data HR, yang menyentuh view dan hook HR
yang dipakai banyak pihak.

`UI GATE: PASS — 8 elemen: REUSE 8, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`

Hasil pemeriksa dan grep anti-regresi, ditempel apa adanya:

```text
$ node ../QuilvianEngineeringSkills/Claude/.claude/rules/frontend/tools/check-css-usage.mjs \
    .../overtime/payroll-run-overtime-view.jsx \
    .../overtime/payroll-overtime-rate-view.jsx \
    .../overtime/payroll-overtime-table-columns.jsx \
    .../run/payroll-run-detail-view.jsx
OK — 4 berkas diperiksa, 0 temuan.

$ grep -nE "<button|className=\"btn|btn-primary|btn-secondary" <3 berkas baru>           (kosong)
$ grep -n "<table" <3 berkas baru>                                                       (kosong)
$ grep -nE "fw-(bold|semibold|light)|fs-[0-9]" <3 berkas baru>                           (kosong)
$ grep -nE "<pola #7 class Tailwind>" <3 berkas baru>                                    (kosong)
$ grep -nE "fixed inset-0|position: ?fixed" <3 berkas baru>                              (kosong)
$ grep -nE "<(input|select|textarea)\b|UUID|GUID" <3 berkas baru>                        (kosong)
$ grep -nE "variant=\"(outline|danger-outline|default|link|light|dark)\"|<StatusBadge[^>]*(status=\{[^}]*label|variant=)" <3 berkas baru>   (kosong)
$ grep -n "style={{" <3 berkas baru>                                                     (kosong)
$ grep -nEi "#[0-9a-f]{3,8}\b|rgba?\(" <3 berkas baru>                                   (kosong)
```

Tidak ada stylesheet baru, sehingga grep warna literal, typography, dan `!important` pada berkas gaya tidak berlaku
(`NOT APPLICABLE`).

| Layar | Screenshot | Catatan perbandingan dengan referensi |
| --- | --- | --- |
| `/payroll/payroll-runs/[id]/overtime` | `VISUAL CHECK: NOT FEASIBLE` | Dev server, build, dan skrip peramban tidak dijalankan: pemilik meminta agen tidak menyalakannya otomatis, dan agen tidak memegang sesi login. Susunan mengikuti layar daftar `PAY-FE-15`/`17`, tetapi **tampilannya belum pernah dilihat** |
| `/payroll/overtime-rates` | `VISUAL CHECK: NOT FEASIBLE` | Alasan sama. Yang paling perlu dilihat: kolom Rentang Waktu dan Perhitungan (dua baris teks per sel) |
| `/payroll/payroll-runs/[id]` (tombol masuk) | `VISUAL CHECK: NOT FEASIBLE` | Alasan sama; tiga tombol di header (dua sebelumnya dari `PAY-FE-17`) perlu dilihat pada lebar sempit |
| Sidebar Payroll (grup Lembur) | `VISUAL CHECK: NOT FEASIBLE` | Alasan sama |

Catatan 2026-11-13: pemilik menguji layar di peramban lewat `npm run dev` dan melaporkan lulus; tidak ada screenshot yang disimpan, sehingga `VISUAL CHECK: NOT FEASIBLE` di atas tetap berlaku bagi agen.

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tabel "Mengambil rekap lembur..." atau "Mengambil tarif lembur..." |
| Kosong | "Belum ada lembur terverifikasi yang diimpor ke run ini." atau "Belum ada tarif lembur." beserta kalimat pendamping |
| Gagal | Kotak galat dengan pesan server atau kalimat baku, dan tombol **Coba lagi** |
| Tanpa hak akses | Tombol masuk dan butir menu tidak tampil tanpa hak; 403 dari server diganti pesan akses ditolak standar |
| Berisi | Tabel berhalaman; pada tarif, keterangan "diubah di modul HR" selalu tampil di atas daftar |

## 5. Endpoint yang dikonsumsi

#### Corporate / Payroll Management / Payroll Run

Base URL: `/v1/corporate/payroll-management/payroll-runs`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/{id}/overtime-inputs?search=&pageNumber=&pageSize=` | Rekap lembur terverifikasi di run | `PayrollRun : Read` |
| `GET` | `/{id}` | Nomor run di judul (opsional) | `PayrollRun : ReadDetail` |

#### Corporate / Human Resource / Master Data / Leave and Overtime / Overtime Rate

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/human-resource/master-data/overtime-rates?search=&dayType=&isActive=&sortBy=priority&sortDirection=asc&pageNumber=&pageSize=` | Daftar tarif lembur baca-saja | `OvertimeRate : Read` |

Tidak ada endpoint `POST`, `PUT`, `PATCH`, atau `DELETE` yang dipanggil.

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` pada 14 berkas source baru atau berubah dan dua berkas test | 0 galat; 4 peringatan `react-hooks/set-state-in-effect`: 2 pada baris warisan `use-payroll-run-detail.js` (88 dan 110), 2 pada hook baru dengan pola yang sama seperti hook Payroll lain | `PASS` (peringatan: `EXISTING WARNING` 2, pola yang sama 2) | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/payroll-overtime.test.mjs` | 18 uji, 18 lulus, 0 dilewati (source backend ada, jadi pencocokan ke backend ikut berjalan) | `PASS` | Keluaran perintah |
| Alamat rekap dan hak `PayrollRun : Read` (dan `ReadDetail` untuk nomor run) dengan `[Route]` dan `[AccessPermission]` controller | Cocok | `PASS` | Uji "rekap lembur: alamat dan hak akses" |
| Saringan rekap sama dengan query backend; ukuran halaman tidak melebihi batas backend 200 | Cocok | `PASS` | Uji "saringan dan batas halaman" |
| Properti rekap dan tarif yang dibaca layar ada di DTO backend dan nilai khasnya sampai ke hasil normalisasi (9 properti rekap, 21 properti tarif) | Cocok | `PASS` | Dua uji properti DTO |
| Alamat, hak `OvertimeRate : Read`, dan parameter saringan tarif dengan controller HR; service tidak punya panggilan tulis | Cocok | `PASS` | Uji "tarif lembur: alamat, hak akses, dan saringan" |
| Nilai jenis hari, rentang waktu, dan metode hitung sama dengan konstanta backend | Cocok | `PASS` | Uji "label ... konstanta backend" |
| Grup menu Lembur: posisi, satu butir, hak `OvertimeRate : Read`, halaman nyata; tombol masuk di detail run dijaga `PayrollRun : Read` | Cocok | `PASS` | Dua uji menu dan jalan masuk |
| Kedua layar tanpa kontrol tulis (tambah, ubah, hapus, kolom aksi); keterangan HR tanpa kondisi; kalimat baku dari skema | Cocok | `PASS` | Tiga uji sifat baca-saja dan kalimat |
| Normalisasi, menit menjadi jam, pengali (koma Indonesia), nilai tarif per metode, rentang, syarat menit, masa berlaku | Sesuai | `PASS` | Tujuh uji murni |
| `payroll-module-contract.test.mjs` | 12 dari 12 lulus setelah daftar menu diperbarui (sebelum diperbarui, 1 gagal tepat pada daftar 8 menu karena grup "Lembur" baru) | `PASS` | Keluaran perintah |
| Pemeriksa statis sekali pakai (di luar repo): setiap simbol impor di 12 berkas terpetakan ke ekspor modul tujuannya | `OK — 12 berkas, semua impor terpetakan.` | `PASS` | Keluaran skrip |
| Suite unit penuh (`node --import ./tests/helpers/register.mjs --test tests/unit/*.test.mjs`) | 4.892 uji, 4.841 lulus, 51 gagal pada 35 berkas | `EXISTING / ENVIRONMENT ISSUE` (51), `PASS` (18 uji baru) | Keluaran perintah |
| Pembanding: himpunan (berkas, nama uji) yang gagal pada suite sebelum task ini (HEAD sama, 4.874 uji) | Identik: 0 kegagalan baru, 0 yang tidak lagi gagal; seluruhnya juga gagal pada `HEAD` murni (dibuktikan pada `PAY-FE-16`) | `PASS` (tanpa kegagalan baru) | Selisih himpunan nama uji |
| Grep anti-regresi UI dan `check-css-usage.mjs` | 0 temuan | `PASS` | Bagian 3.4 |
| `npm run build` | Tidak dijalankan | `NOT RUN` | — |
| `npm run dev` dan uji manual oleh pemilik (2026-11-13) | Dilaporkan berhasil dan seluruh uji lulus. Tidak dirinci: layar dan akun yang dipakai, hasil per langkah uji; tidak ada screenshot tersimpan | `OWNER-REPORTED` — tidak diverifikasi agen; bukan pengganti `npm run build` | Pesan pemilik |
| Pembukaan layar di peramban dan screenshot | Tidak dilakukan | `NOT FEASIBLE` | Bagian 3.4 |

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/payroll-overtime.test.mjs — PASS (18/18)`

Uji manual: `NOT FEASIBLE` — agen tidak menyalakan `npm run dev` atau skrip peramban, dan tidak memegang sesi login.

**Langkah uji untuk pemilik** (setelah `npm run dev`; akun dengan `PayrollRun : Read`, `PayrollRun : ReadDetail`,
`OvertimeRate : Read`; run yang sudah mengimpor data HRD dengan lembur):

1. Buka detail Payroll Run. Harapan: tombol **Rekap Lembur** muncul di header. Dengan akun tanpa `PayrollRun : Read`,
   tombol tidak tampil.
2. Tekan **Rekap Lembur**. Harapan: tabel menampilkan pegawai, tanggal lembur, menit terverifikasi (dengan jam bila ≥ 60),
   pengali, tarif per jam, dan nominal; urut nama lalu tanggal; tanpa tombol tulis (`UAT-21`).
3. Bandingkan beberapa baris dengan angka "Lembur" pada slip atau rincian pegawai di run. Harapan: sama (saringan rekap
   disamakan dengan mesin hitung).
4. Cari nama atau nomor pegawai; ubah jumlah baris; pindah halaman. Harapan: server yang memfilter dan mengurutkan.
5. Pada run tanpa lembur terverifikasi. Harapan: "Belum ada lembur terverifikasi yang diimpor ke run ini."
6. Buka sidebar Payroll. Harapan: grup **Lembur** dengan butir **Tarif Lembur**. Dengan akun tanpa
   `OvertimeRate : Read`, grup dan butir tidak tampil (`UAT-22`).
7. Buka **Tarif Lembur**. Harapan: keterangan "Tarif lembur diubah di modul HR, bukan di sini. Payroll hanya membacanya."
   tampil; daftar menurut prioritas; tidak ada tombol tambah, ubah, atau hapus, walau akun juga memegang
   `OvertimeRate : Update` di HR.
8. Saring Jenis Hari dan Status. Harapan: hasil sesuai; baris Nonaktif bertanda Nonaktif.
9. Bandingkan satu tarif dengan layar HR (`/hr/master-data/overtime-rate`): nama, kode, kebijakan, jenis hari, pengali atau
   nominal, masa berlaku, dan status sama.
10. Matikan backend lalu muat ulang kedua layar. Harapan: pesan galat dan tombol Coba lagi.

Uji manual oleh pemilik (2026-11-13): `OWNER-REPORTED`, lulus. Pemilik menjalankan `npm run dev` dan melaporkan seluruh uji lulus; hasil per langkah tidak dirinci dan tidak diverifikasi agen. `npm run build` tetap `NOT RUN`.

**Tidak dijalankan oleh agen:** `npm run build`, `npm run dev`, uji e2e, uji manual dan pemeriksaan visual di peramban, `dotnet build`
backend.

## 7. Acceptance criteria dan Definition of Done

Ringkasan: **4 dari 4** kriteria terpetakan ke source dan bukti statis; **4 dari 4** dinyatakan lulus oleh pemilik di peramban (2026-11-13, tanpa rincian per langkah; bukan bukti agen).

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Rekap menampilkan kolom sesuai skema | Terpenuhi di source; pengujian pemilik: lulus (2026-11-13, tanpa rincian) | `buildRunOvertimeColumns`: Pegawai, Tanggal Lembur, Menit Terverifikasi, Pengali, Tarif per Jam, Nominal; uji judul kolom persis dan uji properti DTO |
| 2. Tarif lembur tanpa tombol ubah | Terpenuhi di source | Tidak ada kontrol tulis atau kolom aksi di view, kolom, dan hook; service tanpa panggilan tulis; dua uji |
| 3. Butir "Tarif Lembur" dijaga `OvertimeRate : Read` | Terpenuhi di source | `menu-items.jsx`; uji butir menu; `payroll-module-contract.test.mjs` lulus (setiap butir punya hak menu dan halaman nyata) |
| 4. Kalimat kosong sesuai skema | Terpenuhi di source | Uji kalimat baku: "Belum ada lembur terverifikasi yang diimpor ke run ini." dan "Belum ada tarif lembur."; keterangan HR persis dari skema |
| DoD: kriteria terbukti | Terpenuhi menurut pernyataan pemilik (2026-11-13); build produksi belum (`NOT RUN`) | `OWNER-REPORTED`; bagian 6 |
| DoD: menu dan jalan masuk ada | Terpenuhi | Grup Lembur, butir Tarif Lembur, tombol Rekap Lembur di detail run |
| DoD: laporan ditulis | Terpenuhi | Berkas ini |
| Verifikasi roadmap: lint | Terpenuhi (0 galat) | Bagian 6 |
| Verifikasi roadmap: build | **Belum** | `NOT RUN` |
| Verifikasi roadmap: uji manual `UAT-21`, `UAT-22` | Terpenuhi menurut pernyataan pemilik (2026-11-13), tanpa rincian per langkah | `OWNER-REPORTED` |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 2 peringatan ESLint `react-hooks/set-state-in-effect` pada hook baru, pola yang sama dengan hook Payroll V2 lain, dan 2 pada baris warisan `use-payroll-run-detail.js`. Tidak diperbaiki di luar cakupan |
| Keputusan pemilik yang masih terbuka (dari `PAY-BE-23`) | Hak rekap mengikuti kontrak (`PayrollRun : Read`), lebih longgar daripada detail pegawai run (`ReadEmployeeDetail`). Layar mengikuti kontrak: tombol dan layar dijaga `PayrollRun : Read`. Bila pemilik memilih mengetatkan ke `ReadEmployeeDetail`, `PAYROLL_OVERTIME_PERMISSIONS.run.read` dan satu uji perlu disesuaikan (ubahan kecil). Sampai itu diputuskan, peran yang tidak boleh melihat nominal lembur sebaiknya tidak diberi `PayrollRun : Read` |
| Hak akses yang harus diberikan admin | `OvertimeRate : Read` untuk peran Payroll (sudah ada pada usulan peran Staf Payroll, `permission-audit-matrix.md` bagian 3) dan `PayrollRun : Read` untuk rekap. Tanpa `OvertimeRate : Read`, butir menu dan layar tarif tidak tampil |
| Kolom tarif ditentukan agen | Skema hanya menyebut "tarif lembur sesuai endpoint HR". Kolom dipilih dari `OvertimeRateResponse`: tarif (nama, kode), kebijakan, jenis hari, rentang waktu (dengan jam atau menit dan syarat menit), perhitungan (metode dan nilai), masa berlaku, prioritas, status. Kolom pembuat dan tanggal buat dari layar HR sengaja tidak dibawa karena tidak berguna bagi pembaca Payroll. Layar tidak mengurutkan di klien dan tidak menyediakan pengurutan |
| Layar rekap tanpa total | Backend tidak mengirim total, dan menjumlahkan halaman yang tampil akan menyesatkan. Total per pegawai ada di rincian pegawai pada detail run |
| Layar tarif tidak menyentuh HR | Tarif dibaca lewat endpoint HR dengan service Payroll; layar HR yang sudah ada tidak diubah dan tidak diimpor. Label jenis hari, rentang waktu, dan metode hitung diimpor dari konstanta HR (`overtime-rate-constants.jsx`); bila berkas itu kelak dipindah atau diubah namanya, impor di konstanta lembur Payroll ikut diperbarui (diuji lewat nilai backend) |
| Tombol di header detail run | Detail run kini punya tiga tombol (Penghasilan Variabel, Rekap Lembur, Kembali). Belum dilihat pada lebar sempit; bila terasa padat, dapat dipindah ke bagian bawah header |
| Masalah yang diketahui | (1) Layar sudah diuji pemilik (2026-11-13, lulus, tanpa rincian), tetapi agen belum pernah melihatnya dan tidak ada screenshot tersimpan. (2) Nomor run di judul membutuhkan `PayrollRun : ReadDetail` dan memuat seluruh detail run (termasuk pegawai) hanya untuk satu nomor; dapat diganti bila backend kelak mengirim nomor run di respons rekap. (3) Kolom Rentang Waktu dan Perhitungan memuat dua sampai tiga baris teks per sel |
| Dependency backend | `PAY-BE-23` 🟡: source dibaca dan dicocokkan, tetapi `dotnet build` dan uji API belum dijalankan pemilik. Seluruh perilaku layar rekap bergantung pada backend itu berjalan; layar tarif memakai endpoint HR yang sudah ada |
| Perubahan sampingan | `NONE` pada source. Satu tes kontrak (`payroll-module-contract.test.mjs`) diperbarui karena grup menu baru, dijelaskan di 3.3 |
| Interupsi | `NONE` |
| Status Git | Frontend, task ini: ` M` `payroll-constants.js`, ` M` `menu-items.jsx`, ` M` `use-payroll-run-detail.js`, ` M` `payroll-run-detail-view.jsx`, ` M` `payroll-module-contract.test.mjs`; `??` route `payroll-runs/[id]/overtime/` dan `overtime-rates/`, folder `view/payroll/overtime/` (3 berkas), konstanta, 2 hook, service, utils, dan `tests/unit/payroll-overtime.test.mjs`. Perubahan `PAY-FE-16`–`18` yang belum di-commit masih ada. Tidak ada yang di-stage atau di-commit. Backend, dari task ini: `??` laporan ini; ` M` `frontend-roadmap.md` dan `requirement-traceability.md` (tanda status). ` M` `PayrollSimulationService.cs` bukan dari task ini |
| Langkah berikutnya | Status ✅ atas instruksi pemilik (2026-11-13). Sisa: `npm run build` produksi (`NOT RUN`), pembuktian build serta uji API backend `PAY-BE-23` (masih 🟡), dan keputusan pemilik tentang hak rekap (`Read` vs `ReadEmployeeDetail`, baris di atas) yang tetap terbuka. |
