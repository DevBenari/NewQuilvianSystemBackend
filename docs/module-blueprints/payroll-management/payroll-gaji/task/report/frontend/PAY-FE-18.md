# Laporan Perubahan Frontend — `PAY-FE-18`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-FE-18` |
| Judul | Simulasi Penggajian |
| Slice | `MVP-1` — `EPIC PGJ-05`, layar `FE-PGJ-04` |
| Roadmap | `docs/module-blueprints/payroll-management/payroll-gaji/roadmap/frontend-roadmap.md`, kartu `PAY-FE-18` |
| Trace | FR-PGJ-040–042; PRL-DEC-32; `../../../03-frontend-architecture.md` bagian 2 (peta butir menu), 3.3, 6; `../../../contracts/api-contract.md` grup Payroll Run (tambahan, `POST /simulations`); `../../../contracts/validation-matrix.md` bagian 5 (`SIM-01`–`SIM-03`) |
| Contract version | `PGJ-C1` — `approved` (Yasmin, 2026-11-12). Catatan: `PAY-BE-21` menambah `notes` dan `employeeNumber` pada respons dan mempersempit cakupan simulasi (tanpa impor HR) atas keputusan pemilik 2026-11-12; amandemen kontrak masih menunggu, dan layar ini mengikuti source backend |
| Wewenang UI | Isi layar, sumber data, hak akses butir menu, serta kalimat tetap dikunci `03-frontend-architecture.md` bagian 3.3. Bentuk layar dan komponen `DEV_DISCRETION` lewat gerbang keputusan base component. Sumber daftar pegawai tidak ditetapkan skema (dipilih Profil Penggajian, bagian 8) |
| Dependency | `PAY-BE-21` 🟡 — source backend sudah ada dan dibaca langsung dari working tree backend, tetapi `dotnet build` dan uji runtime belum dijalankan pemilik. Aturan roadmap meminta prasyarat `[BE]` berstatus ✅ sebelum task dimulai; task ini dikerjakan atas instruksi pemilik pada 2026-11-13, sama seperti `PAY-FE-16` dan `PAY-FE-17` |
| Klasifikasi | `MEDIUM` — satu layar baru (pilihan, hasil, rincian), satu endpoint tulis, satu butir menu, sembilan berkas source baru dan dua yang sudah ada diubah |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` branch `yasmina`; laporan dan penandaan status di repository backend |
| Model | Claude Sonnet 5.5 |
| Commit frontend saat dikerjakan | `ff46d5db1` (branch `yasmina`); perubahan task ini, `PAY-FE-16`, dan `PAY-FE-17` belum di-commit |
| Commit backend yang dijadikan rujukan | `726234df` (branch `Yasmina`). `PayrollSimulationService.cs` di working tree backend berbeda dari commit itu oleh satu baris `using` yang belum di-commit (pihak lain, tidak mengubah perilaku); source lain yang dibaca tidak berubah |
| Tanggal | 2026-11-13 |
| Status | ✅ — source selesai dan 4 dari 4 kriteria terpetakan ke source; ESLint 0 galat; 13 uji unit baru lulus; suite penuh tanpa kegagalan baru; agen tidak membuka layar (`VISUAL CHECK: NOT FEASIBLE`, `MANUAL TEST: NOT FEASIBLE` oleh agen); pada 2026-11-13 pemilik melaporkan `npm run dev` berhasil dan seluruh uji lulus (laporan lisan, tanpa rincian per langkah, tanpa screenshot tersimpan; bukan bukti agen) lalu menginstruksikan status naik ke ✅; **`npm run build` tetap `NOT RUN`**; backend `PAY-BE-21` belum terbukti build dan uji runtime (masih 🟡) |

---

## 1. Keadaan yang ditemukan di awal

- **Backend** sudah menyediakan `POST /v1/corporate/payroll-management/payroll-runs/simulations` (hak
  `PayrollRun : Simulate`): permintaan berisi periode dan, opsional, daftar `workforceProfileIds`; respons berisi kode
  periode, versi kebijakan, jumlah dan total (hanya pegawai yang berhasil dihitung), catatan `notes`, dan per pegawai
  angka kotor, potongan, bersih, komponen, peringatan, dan galat. Transaksi simulasi selalu dibatalkan, tidak ada run,
  input, nomor, atau perubahan periode yang tersisa.
- **Frontend** belum punya layarnya, dan tidak ada butir menu "Simulasi Penggajian" di grup Proses Penggajian.
- **Perilaku backend yang menentukan rancangan:**
  - Penolakan `SIM-01` (404, periode tidak ditemukan), `SIM-02` (400, pegawai melebihi batas), dan `SIM-03` (400, tidak ada
    pegawai layak) datang sebagai `message`; batas `SimulationMaxEmployees` adalah opsi kebijakan (bawaan 50) dan tidak
    dikirim ke klien.
  - Pegawai kosong berarti seluruh pegawai layak hitung, dipangkas ke batas dengan catatan di `notes`.
  - Simulasi tidak mengimpor kehadiran, lembur, dan cuti dari HR, tidak memuat penghasilan variabel dan cicilan piutang;
    `notes` menyatakannya. Potongan pada hasil adalah potongan sebelum cicilan.
  - Pegawai bergalat tetap muncul pada `employees` dengan `errors`, tetapi tidak ikut total.
  - Opsi periode memakai endpoint yang sudah ada `GET /payroll-runs/period-options` (`PayrollRun : ReadPeriods`).
  - Tidak ada endpoint pencarian pegawai khusus simulasi; `workforceProfileId` tersedia pada daftar Profil Penggajian
    (`PayrollCompensation : Read`), yang memuat karyawan dan dokter yang punya profil payroll.

## 2. Proses bisnis dari sisi pengguna

**Pengguna.** Petugas atau Kepala Payroll yang memegang hak `PayrollRun : Simulate` di Akses Role, ditambah
`PayrollRun : ReadPeriods` agar pilihan periode terbaca.

**Langkah:**

1. Petugas membuka Payroll → Proses Penggajian → **Simulasi Penggajian**. Butir menu hanya tampil bagi pemegang
   `PayrollRun : Simulate`.
2. Di bagian atas layar selalu tampil: "Hasil simulasi tidak tersimpan dan tidak mempengaruhi run resmi."
3. Petugas memilih **Periode**. Bila perlu, ia mencari pegawai (nama atau nomor), memilihnya satu per satu, dan melihatnya
   sebagai daftar terpilih yang dapat dihapus satu per satu atau dikosongkan. Pegawai dikosongkan berarti seluruh pegawai
   layak hitung (label: "Pegawai (opsional, maks 50)").
4. Petugas menekan **Jalankan**. Tombol berubah "Menghitung..." dan seluruh pilihan dinonaktifkan sampai hasil datang.
5. Layar menampilkan: **Yang tidak ikut disimulasikan** (catatan dari server), **Ringkasan** (jumlah pegawai, total
   kotor, potongan, bersih, beserta periode, versi kebijakan, dan jumlah pegawai bergalat dan berperingatan), dan tabel
   **Per pegawai** (kotor, potongan, bersih, catatan hitung).
6. **Rincian** pada satu baris membuka komponen penghasilan dan potongan pegawai itu beserta galat dan peringatannya.

**Contoh** (data samaran, `UAT-16`): periode Mei, 48 pegawai, tanpa memilih pegawai. Hasil menampilkan 48 baris. Satu
pegawai bergalat "Penetapan gaji belum disetujui." dengan lencana "1 galat"; ringkasan hanya menjumlahkan 47 pegawai
lainnya, dan keterangan ringkasan menyatakannya. Setelah itu tidak ada run, input, atau perubahan periode baru di sistem.

**Jalur tidak normal:**

| Keadaan | Yang terjadi |
| --- | --- |
| Belum dijalankan | Tampil persis: "Pilih periode lalu jalankan simulasi." |
| Sedang berjalan | "Menghitung..." dengan penanda putar; tombol Jalankan memuat dan nonaktif, seluruh pilihan nonaktif |
| Server menolak (`SIM-01`, `SIM-02`, `SIM-03`) | Pesan server tampil apa adanya di kotak merah, dengan tombol **Coba lagi** yang mengulang permintaan yang sama |
| Server menjawab 403 | Kotak merah "Anda tidak punya hak untuk tindakan ini." tanpa tombol coba lagi |
| Jaringan atau server bermasalah | Pesan galat dan tombol Coba lagi |
| Pilihan periode gagal dimuat | Kotak merah dengan tombol Coba lagi; bila 403: "Pilihan periode tidak dapat dimuat. Peran Anda perlu hak membaca periode payroll." |
| Peran tanpa `PayrollCompensation : Read` | Pencarian pegawai kosong dengan peringatan; simulasi seluruh pegawai tetap dapat dijalankan |
| Lebih dari batas pegawai dipilih | Tidak diblokir di layar; server menolak dengan `SIM-02` dan pesannya tampil apa adanya |
| Pegawai bergalat | Tetap tampil dengan lencana galat; tidak ikut jumlah ringkasan |
| Layar ditinggalkan saat simulasi berjalan | Permintaan dibatalkan |
| Periode atau pegawai diganti setelah hasil tampil | Hasil lama tetap tampil dengan kode periodenya; dibuang hanya saat simulasi baru dijalankan |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- **Governance:** `AGENTS.md` frontend; `rules/frontend/*` (arsitektur, katalog dan gerbang keputusan base component,
  pola komposisi halaman, checklist UI, template laporan); `rules/rule-output/status-task-roadmap.md`.
- **Blueprint:** `03-frontend-architecture.md`, `contracts/api-contract.md`, `contracts/validation-matrix.md`,
  `contracts/permission-audit-matrix.md`, `testing/acceptance-test-matrix.md` (`UAT-16`, `UAT-17`),
  `roadmap/frontend-roadmap.md`, `roadmap/requirement-traceability.md`, laporan `PAY-BE-21`, `PAY-FE-16`, `PAY-FE-17`.
- **Backend (baca-saja):** `PayrollRunsController.cs`, `PayrollSimulationService.cs`, `PayrollSimulationDtos.cs`,
  `PayrollPolicyOptions.cs`, `PayrollCompensationsController.cs`, `PayrollCompensationDtos.cs`,
  `PayrollCompensationService.cs`, `PayrollControllerSupport.cs`.
- **Rujukan frontend:** `payroll-run-preparation-view.jsx` (pemilihan periode), `payroll-run-employee-detail-modal.jsx`
  (rincian komponen), `use-payroll-run-period.js`, `payroll-run-service.js`, `payroll-run-utils.js`,
  `payroll-compensation-service.js` dan utils-nya, `payroll-constants.js`, `payroll-section.jsx`,
  `payroll-run.module.css`, `use-select-resource.jsx`, `base-form-control.jsx`,
  `use-hemodialysis-schedule-dialog.jsx` (`NURSE_SELECT_RESOURCE`), `menu-items.jsx`, `payroll-module-contract.test.mjs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/payroll/payroll-simulation-constants.js` | Baru. Endpoint, hak akses, konfigurasi pilihan pegawai, seluruh salinan teks termasuk kalimat tetap dari skema |
| `src/utils/payroll/payroll-simulation-utils.js` | Baru. Normalisasi respons, penyusun permintaan, pilihan periode, pilihan pegawai (tanpa duplikat), ringkasan catatan hitung |
| `src/lib/services/payroll/payroll-simulation-service.js` | Baru. Satu panggilan Axios lewat `InstanceAxios` dengan sinyal pembatalan |
| `src/lib/hooks/payroll/use-payroll-simulation.js` | Baru. Pilihan periode, pilihan pegawai, jalankan dengan kunci tunggal dan pembatalan, hasil, galat, rincian |
| `src/components/view/payroll/simulation/payroll-simulation-view.jsx` | Baru. Halaman |
| `src/components/view/payroll/simulation/payroll-simulation-employee-picker.jsx` | Baru. Pencarian pegawai dan daftar terpilih |
| `src/components/view/payroll/simulation/payroll-simulation-table-columns.jsx` | Baru. Kolom per pegawai dan kolom komponen |
| `src/components/view/payroll/simulation/payroll-simulation-detail-modal.jsx` | Baru. Rincian satu pegawai |
| `src/app/payroll/simulations/page.jsx` | Baru. Route tipis |
| `src/lib/constants/payroll/payroll-constants.js` | Diubah (1 baris). `PAYROLL_ROUTES.simulations` |
| `src/utils/menu-sidebar/menu-items.jsx` | Diubah. Butir "Simulasi Penggajian" di "Proses Penggajian", dijaga `PayrollRun : Simulate` |
| `tests/unit/payroll-simulation.test.mjs` | Baru. 13 uji, termasuk pencocokan dengan source backend |

### 3.3 Kepatuhan arsitektur frontend

- **Pola:** layar operasional Payroll V2 (service → hook dengan state lokal → view), sama dengan `PAY-FE-15`–`17`. Hasil
  simulasi hanya di state hook layar ini: tidak masuk Redux, tidak masuk penyimpanan peramban, tidak ada nominal di log
  (skema bagian 6, "Privasi").
- **Alur dependensi:** route → view → hook → service/utils/constants. View tidak memanggil Axios; utils murni.
- **Hak akses:** butir menu `PayrollRun : Simulate`; tidak ada nama peran di kode. Opsi periode memakai service dan
  normalizer yang sudah ada, bukan salinan.
- **Tanpa abstraksi baru:** tidak ada komponen shared, instance Axios, registry pilihan bersama, atau CSS Module baru.
  Pilihan pegawai memakai konfigurasi pilihan lepas (pola `NURSE_SELECT_RESOURCE`).
- **Tes kontrak Payroll:** `PAYROLL_ROUTES.simulations` ditambahkan sehingga `payroll-module-contract.test.mjs` ikut
  menuntut halaman `/payroll/simulations` ada; tes itu tetap lulus.

### 3.4 Gerbang tampilan

**Modul referensi visual:** `payroll-run-preparation-view.jsx` (pemilihan periode), `payroll-run-employee-detail-modal.jsx`
(rincian komponen per penerima), layar daftar `PAY-FE-15` dan `PAY-FE-17`.

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Header halaman | `Hero` | `REUSE` | — |
| Kalimat "tidak tersimpan", selalu tampil | `InformationAlert` (`info`, tidak dapat ditutup) | `REUSE` | Dipasang tanpa kondisi, sejajar dengan `Hero` |
| Panel pilihan (periode, pegawai, tombol) | `PayrollSection`, `BaseSelectField`, `BaseButton` (`loading`) — `payroll-run.module.css` | `REUSE` | Pola Payroll yang sama |
| Pilihan pegawai opsional | `BaseSelectField` + `useSelectResource` dengan konfigurasi lepas | `COMPOSE` | Tidak ada resource registry untuk Profil Penggajian; konfigurasi lepas di atas daftar `workforce-compensations`, pola `NURSE_SELECT_RESOURCE`. Cari dan muat bertahap dari server |
| Pegawai terpilih | `BaseButton variant="subtle"` per pegawai (klik = hapus) dan `BaseButton variant="secondary"` (kosongkan) — `styles.linkRow` | `COMPOSE` | Tidak ada base component multi-pilih; dirangkai dari select pencarian dan tombol |
| Belum jalan, menghitung | `styles.state`, `Spinner` (react-bootstrap) | `REUSE` | Pola detail run |
| Galat dan Coba lagi | `InformationAlert`, `BaseButton`, `styles.errorActions` | `REUSE` | Pola detail run |
| Catatan yang tidak ikut disimulasikan | `PayrollNoticeList` | `REUSE` | Pola detail run |
| Ringkasan | `PayrollSection`, `SummaryCards`, `PayrollInfoGrid` | `REUSE` | Pola detail run |
| Tabel per pegawai dan komponen | `DataTable` (`pagination={false}`), `StatusBadge` | `REUSE` | Pola detail run |
| Rincian pegawai | `ConfirmModal size="xl"` + `PayrollInfoGrid` + `PayrollNoticeList` + `DataTable` | `REUSE` | `payroll-run-employee-detail-modal.jsx` |
| Butir menu | `menu-items.jsx` (`RiFlaskLine`) | `REUSE` | Pola butir menu Payroll |

**Pilihan yang dicatat (`COMPOSE`, bukan `NEW`):** pilihan pegawai — A (dipakai) pilih satu per satu lewat daftar bercari
lalu tampil sebagai daftar terpilih yang dapat dihapus; B komponen multi-pilih baru, paling nyaman tetapi membutuhkan
persetujuan dan berlebihan untuk satu layar.

`UI GATE: PASS — 10 elemen: REUSE 8, EXTEND 0, COMPOSE 2, WRAP 0, NEW 0`

Hasil pemeriksa dan grep anti-regresi, ditempel apa adanya:

```text
$ node ../QuilvianEngineeringSkills/Claude/.claude/rules/frontend/tools/check-css-usage.mjs \
    .../simulation/payroll-simulation-view.jsx \
    .../simulation/payroll-simulation-table-columns.jsx \
    .../simulation/payroll-simulation-detail-modal.jsx \
    .../simulation/payroll-simulation-employee-picker.jsx
OK — 4 berkas diperiksa, 0 temuan.

$ grep -nE "<button|className=\"btn|btn-primary|btn-secondary" <4 berkas baru>           (kosong)
$ grep -n "<table" <4 berkas baru>                                                       (kosong)
$ grep -nE "fw-(bold|semibold|light)|fs-[0-9]" <4 berkas baru>                           (kosong)
$ grep -nE "<pola #7 class Tailwind>" <4 berkas baru>                                    (kosong)
$ grep -nE "fixed inset-0|position: ?fixed" <4 berkas baru>                              (kosong)
$ grep -nE "<(input|select|textarea)\b|UUID|GUID" <4 berkas baru>                        (kosong)
$ grep -nE "variant=\"(outline|danger-outline|default|link|light|dark)\"|<StatusBadge[^>]*(status=\{[^}]*label|variant=)" <4 berkas baru>   (kosong)
$ grep -n "style={{" <4 berkas baru>                                                     (kosong)
$ grep -nEi "#[0-9a-f]{3,8}\b|rgba?\(" <4 berkas baru>                                   (kosong)
```

Tidak ada stylesheet baru, sehingga grep warna literal, typography, dan `!important` pada berkas gaya tidak berlaku
(`NOT APPLICABLE`).

| Layar | Screenshot | Catatan perbandingan dengan referensi |
| --- | --- | --- |
| `/payroll/simulations` | `VISUAL CHECK: NOT FEASIBLE` | Dev server, build, dan skrip peramban tidak dijalankan: pemilik meminta agen tidak menyalakannya otomatis, dan agen tidak memegang sesi login. Susunan mengikuti layar Persiapan Penggajian dan detail run, tetapi **tampilannya belum pernah dilihat**. Yang paling perlu dilihat: daftar pegawai terpilih (tombol `subtle`), tabel di dalam bungkus `regionTheme`, dan modal rincian |

Catatan 2026-11-13: pemilik menguji layar di peramban lewat `npm run dev` dan melaporkan lulus; tidak ada screenshot yang disimpan, sehingga `VISUAL CHECK: NOT FEASIBLE` di atas tetap berlaku bagi agen.

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Belum dijalankan | "Pilih periode lalu jalankan simulasi." |
| Memuat (menghitung) | "Menghitung..."; tombol Jalankan memuat dan nonaktif; pilihan nonaktif |
| Kosong | Hasil tanpa pegawai: "Tidak ada pegawai pada hasil simulasi." (server biasanya menolak lebih dulu dengan `SIM-03`) |
| Gagal | Pesan server apa adanya (`SIM-01`–`SIM-03`) atau pesan galat, dengan tombol **Coba lagi**; bila 403: "Anda tidak punya hak untuk tindakan ini." |
| Tanpa hak akses | Butir menu tidak tampil tanpa `PayrollRun : Simulate`; penolakan server pada panggilan langsung tampil sebagai kotak merah |
| Berisi | Catatan, ringkasan (4 kartu dan periode, versi kebijakan, jumlah bergalat dan berperingatan), tabel per pegawai, rincian |

## 5. Endpoint yang dikonsumsi

#### Corporate / Payroll Management / Payroll Run

Base URL: `/v1/corporate/payroll-management/payroll-runs`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/simulations` | Menjalankan simulasi; tidak ada data yang disimpan | `PayrollRun : Simulate` |
| `GET` | `/period-options` | Pilihan periode (endpoint yang sudah ada) | `PayrollRun : ReadPeriods` |

#### Corporate / Payroll Management / Payroll Compensation

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/payroll-management/workforce-compensations?payrollStatus=Active&search=...` | Pencarian pegawai opsional; nilai pilihan `workforceProfileId` | `PayrollCompensation : Read` |

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` pada 11 berkas source baru atau berubah dan berkas test | 0 galat; 1 peringatan `react-hooks/set-state-in-effect` pada hook baru (`setPeriodsLoading` di efek), pola yang sama dengan hook Payroll lain | `PASS` (peringatan: pola yang sama) | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/payroll-simulation.test.mjs` | 13 uji, 13 lulus, 0 dilewati (source backend ada, jadi pencocokan ke backend ikut berjalan) | `PASS` | Keluaran perintah |
| Pencocokan alamat `POST simulations` dengan `[Route]` controller backend; hak `Simulate` dan `ReadPeriods` dengan `[AccessPermission]` | Cocok | `PASS` | Uji "alamat dan hak akses" |
| Nama properti permintaan dan respons (respons, pegawai, komponen) dengan DTO backend: nilai khas per properti harus sampai ke hasil normalisasi | Seluruh properti terbaca dengan nilai yang sama | `PASS` | Uji "nama properti permintaan dan respons" |
| Kalimat `SIM-01`–`SIM-03` ada di service backend; tidak disalin di frontend | Cocok | `PASS` | Uji "penolakan SIM-01..SIM-03" |
| Petunjuk "maks 50" sama dengan batas bawaan `SimulationMaxEmployees`; hook tidak membatasi jumlah pilihan | Cocok | `PASS` | Uji "petunjuk maks 50" |
| Sumber pegawai: hak `PayrollCompensation : Read`, saringan `payrollStatus`, `search`, `pageNumber`, `pageSize` ada di query backend | Cocok | `PASS` | Uji "sumber pegawai" |
| Butir menu: label, rute, hak `PayrollRun : Simulate`, halaman nyata, posisi di grup Proses Penggajian | Cocok | `PASS` | Uji "butir menu"; `payroll-module-contract.test.mjs` tetap lulus |
| Kalimat baku (tetap, belum dijalankan, menghitung); kalimat tetap sejajar dengan `Hero` tanpa kondisi | Cocok | `PASS` | Dua uji kalimat |
| Permintaan, normalisasi (camelCase dan PascalCase), jumlah bergalat dan berperingatan, ringkasan catatan, pilihan periode dan pegawai | Sesuai | `PASS` | Lima uji murni |
| Pemeriksa statis sekali pakai (di luar repo): setiap simbol impor di 9 berkas terpetakan ke ekspor modul tujuannya | `OK — 9 berkas, semua impor terpetakan.` | `PASS` | Keluaran skrip |
| Suite unit penuh (`node --import ./tests/helpers/register.mjs --test tests/unit/*.test.mjs`) | 4.874 uji, 4.823 lulus, 51 gagal pada 35 berkas | `EXISTING / ENVIRONMENT ISSUE` (51), `PASS` (13 uji baru) | Keluaran perintah |
| Pembanding: himpunan (berkas, nama uji) yang gagal pada suite sebelum task ini (HEAD sama, 4.861 uji) | Identik: 0 kegagalan baru, 0 yang tidak lagi gagal; seluruhnya juga gagal pada `HEAD` murni (dibuktikan pada `PAY-FE-16`) | `PASS` (tanpa kegagalan baru) | Selisih himpunan nama uji |
| Grep anti-regresi UI dan `check-css-usage.mjs` | 0 temuan | `PASS` | Bagian 3.4 |
| `npm run build` | Tidak dijalankan | `NOT RUN` | — |
| `npm run dev` dan uji manual oleh pemilik (2026-11-13) | Dilaporkan berhasil dan seluruh uji lulus. Tidak dirinci: layar dan akun yang dipakai, hasil per langkah uji; tidak ada screenshot tersimpan | `OWNER-REPORTED` — tidak diverifikasi agen; bukan pengganti `npm run build` | Pesan pemilik |
| Pembukaan layar di peramban dan screenshot | Tidak dilakukan | `NOT FEASIBLE` | Bagian 3.4 |

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/payroll-simulation.test.mjs — PASS (13/13)`

Uji manual: `NOT FEASIBLE` — agen tidak menyalakan `npm run dev` atau skrip peramban, dan tidak memegang sesi login.

**Langkah uji untuk pemilik** (setelah `npm run dev`; akun dengan `PayrollRun : Simulate`, `PayrollRun : ReadPeriods`,
`PayrollCompensation : Read`; periode dengan beberapa pegawai berpenetapan gaji yang berlaku):

1. Buka sidebar Payroll → Proses Penggajian. Harapan: butir **Simulasi Penggajian** tampil. Dengan akun tanpa
   `PayrollRun : Simulate`, butir tidak tampil.
2. Buka layar. Harapan: kalimat "Hasil simulasi tidak tersimpan dan tidak mempengaruhi run resmi." tampil di atas, dan
   "Pilih periode lalu jalankan simulasi." tampil di area hasil; tombol Jalankan nonaktif sampai periode dipilih.
3. Pilih periode, biarkan pegawai kosong, tekan **Jalankan**. Harapan: "Menghitung..." dengan tombol memuat, lalu catatan,
   ringkasan, dan tabel per pegawai (`UAT-16`). Setelahnya periksa bahwa tidak ada payroll run baru di Persiapan Penggajian
   dan tidak ada nomor input yang terbit.
4. Tekan **Rincian** pada satu pegawai. Harapan: komponen penghasilan dan potongan beserta galat dan peringatan.
5. Pada pegawai bergalat (penetapan gaji belum disetujui): lencana galat tampil, pegawai itu tidak ikut total.
6. Cari pegawai (nama atau nomor), pilih dua orang. Harapan: dua tombol terpilih; klik satu untuk menghapusnya; pilih orang
   yang sama dua kali tidak menggandakan. Jalankan: hanya dua pegawai pada hasil, dan catatan menyebut pegawai yang tidak punya
   profil aktif bila ada.
7. Pilih lebih dari 50 pegawai (atau turunkan batas kebijakan di lingkungan uji). Harapan: server menolak `SIM-02`; pesan
   tampil apa adanya dan **Coba lagi** mengulang permintaan (`UAT-17`).
8. Pilih pegawai yang tidak punya profil aktif di periode itu saja. Harapan: `SIM-03` tampil apa adanya.
9. Akun tanpa `PayrollCompensation : Read`. Harapan: pencarian pegawai menampilkan peringatan; simulasi seluruh pegawai tetap
   jalan.
10. Matikan backend lalu jalankan. Harapan: pesan galat dan tombol Coba lagi.

Uji manual oleh pemilik (2026-11-13): `OWNER-REPORTED`, lulus. Pemilik menjalankan `npm run dev` dan melaporkan seluruh uji lulus; hasil per langkah tidak dirinci dan tidak diverifikasi agen. `npm run build` tetap `NOT RUN`.

**Tidak dijalankan oleh agen:** `npm run build`, `npm run dev`, uji e2e, uji manual dan pemeriksaan visual di peramban, `dotnet build`
backend.

## 7. Acceptance criteria dan Definition of Done

Ringkasan: **4 dari 4** kriteria terpetakan ke source dan bukti statis; **4 dari 4** dinyatakan lulus oleh pemilik di peramban (2026-11-13, tanpa rincian per langkah; bukan bukti agen).

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Kalimat "Hasil simulasi tidak tersimpan dan tidak mempengaruhi run resmi." selalu tampil | Terpenuhi di source; pengujian pemilik: lulus (2026-11-13, tanpa rincian) | `InformationAlert` tanpa kondisi, sejajar dengan `Hero`; dua uji (bunyi persis dan tanpa kondisi) |
| 2. Ringkasan dan rincian per pegawai termasuk peringatan dan galat | Terpenuhi di source; pengujian pemilik: lulus (2026-11-13, tanpa rincian) | Kartu ringkasan, tabel dengan lencana galat dan peringatan, modal Rincian dengan `PayrollNoticeList` galat dan peringatan; uji normalisasi seluruh properti DTO |
| 3. Penolakan `SIM-01`–`SIM-03` tampil apa adanya | Terpenuhi di source | Pesan server diteruskan tanpa penyalinan atau penerjemahan; uji menjamin kalimat itu tidak disalin di frontend dan ada di backend |
| 4. Butir menu dijaga `PayrollRun : Simulate` | Terpenuhi di source | `menu-items.jsx`; uji butir menu; `payroll-module-contract.test.mjs` lulus |
| DoD: kriteria terbukti | Terpenuhi menurut pernyataan pemilik (2026-11-13); build produksi belum (`NOT RUN`) | `OWNER-REPORTED`; bagian 6 |
| DoD: butir menu terdaftar | Terpenuhi | `corporatePayrollSimulation` di grup Proses Penggajian |
| DoD: laporan ditulis | Terpenuhi | Berkas ini |
| Verifikasi roadmap: lint | Terpenuhi (0 galat) | Bagian 6 |
| Verifikasi roadmap: build | **Belum** | `NOT RUN` |
| Verifikasi roadmap: uji manual `UAT-16`, `UAT-17` | Terpenuhi menurut pernyataan pemilik (2026-11-13), tanpa rincian per langkah | `OWNER-REPORTED` |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 1 peringatan ESLint `react-hooks/set-state-in-effect` pada hook baru, pola yang sama dengan hook Payroll V2 lain. Tidak diperbaiki di luar cakupan |
| Sumber daftar pegawai dipilih agen | Skema menyebut "Pilih pegawai (opsional, maks 50)" tetapi hanya menunjuk opsi periode sebagai sumber data. Dipilih daftar Profil Penggajian (`PayrollCompensation : Read`) karena memuat karyawan dan dokter yang punya profil payroll, sama dengan populasi yang dihitung simulasi, dan memberi `workforceProfileId`. Bukan dari modul HR. Hak itu tidak ada pada usulan peran di `permission-audit-matrix.md` bagian 3 untuk Staf Payroll (usulan memuat `PayrollRun : Simulate` dan `ReadPeriods`); tanpanya pencarian pegawai gagal dengan peringatan dan simulasi seluruh pegawai tetap jalan. Perlu ditambahkan pada usulan peran bila pencarian pegawai diinginkan |
| Batas 50 pegawai | Label "maks 50" mengikuti skema dan sama dengan batas bawaan backend (diuji). Batas itu adalah opsi kebijakan yang bisa berbeda per lingkungan dan tidak dikirim ke klien, sehingga layar **tidak memblokir** pilihan lebih dari 50 dan membiarkan server menolak dengan `SIM-02` (skema: tampil apa adanya). Bila Backend kelak mengirim batasnya, layar dapat menampilkannya |
| Cakupan simulasi lebih sempit dari skema | Atas keputusan pemilik 2026-11-12 (`PAY-BE-21`), simulasi tidak memuat impor HR (kehadiran, lembur, cuti), penghasilan variabel, dan cicilan piutang; potongan adalah potongan sebelum cicilan. Layar menampilkan catatan itu dari server di "Yang tidak ikut disimulasikan" dan keterangan ringkasan. Angka simulasi karena itu **tidak sama** dengan hasil run resmi untuk pegawai yang punya sumber-sumber itu; amandemen kontrak `PGJ-C1` masih menunggu |
| Pegawai bergalat | Respons mengikutkan pegawai bergalat tetapi tidak menjumlahkannya. Layar menandai jumlahnya dan menyatakannya di keterangan ringkasan; angka kotor, potongan, dan bersih baris bergalat tampil apa adanya dari server |
| Hasil lama setelah pilihan berubah | Hasil dibuang hanya saat simulasi baru dijalankan; selama itu hasil lama tetap tampil dengan kode periodenya, supaya petugas tidak kehilangan hasil karena salah klik. Bila diinginkan dibuang saat pilihan berubah, ubahan kecil di hook |
| Masalah yang diketahui | (1) Pencarian pegawai memuat 25 pegawai pertama saat layar dibuka (satu permintaan tambahan), walau tidak dipakai. (2) Daftar terpilih memakai tombol `subtle` per pegawai; bentuknya belum dilihat agen (pemilik menguji 2026-11-13, lulus, tanpa rincian). (3) Backend memakai koneksi database selama transaksi simulasi terbuka (`PAY-BE-21` bagian 7); simulasi berulang di jam sibuk perlu dipantau. (4) Layar belum pernah dilihat di peramban |
| Dependency backend | `PAY-BE-21` 🟡: source dibaca dan dicocokkan, tetapi `dotnet build` dan uji runtime belum dijalankan pemilik. Seluruh perilaku layar bergantung pada backend itu berjalan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Frontend, task ini: ` M` `payroll-constants.js`, ` M` `menu-items.jsx`; `??` route `simulations/`, folder `view/payroll/simulation/` (4 berkas), konstanta, hook, service, utils, dan `tests/unit/payroll-simulation.test.mjs`. Perubahan `PAY-FE-16` dan `PAY-FE-17` yang belum di-commit masih ada. Tidak ada yang di-stage atau di-commit. Backend, dari task ini: `??` laporan ini; ` M` `frontend-roadmap.md` dan `requirement-traceability.md` (tanda status). ` M` `PayrollSimulationService.cs` bukan dari task ini |
| Langkah berikutnya | Status ✅ atas instruksi pemilik (2026-11-13). Sisa: `npm run build` produksi (`NOT RUN`) dan pembuktian build serta uji runtime backend `PAY-BE-21` (masih 🟡). |
