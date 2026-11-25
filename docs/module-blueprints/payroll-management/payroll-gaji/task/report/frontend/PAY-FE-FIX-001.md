# Laporan Perubahan Frontend — `PAY-FE-FIX-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-FE-FIX-001` (ad-hoc, bukan kartu roadmap) |
| Judul | Layar Profil Penggajian menampilkan populasi HRD, termasuk penerima tanpa profil payroll |
| Roadmap | **NOT APPLICABLE.** Task ad-hoc atas perintah pemilik; roadmap dan traceability tidak disentuh |
| Trace | Perintah pemilik 2026-11-17 (master prompt "Perbaikan Data Profil Penggajian Kosong") |
| Contract version | Mengonsumsi kontrak additive `PAY-BE-FIX-001` (`PayrollCompensationListItemResponse` + `hasPayrollProfile`, `payrollProfileStatus`, `hrdComplete`, `hrdDataStatus`, `taxInsuranceStatus`, `notes`; `payrollStatus` nullable; `GET …/summary`). Kontrak itu **belum dikompilasi dan belum diterapkan di lingkungan mana pun** |
| Dependency backend | `PAY-BE-FIX-001`. Tanpa backend baru, daftar tetap kosong seperti semula |
| Wewenang UI | Perintah pemilik; tanpa komponen global baru, tanpa kelas CSS baru, tanpa halaman baru |
| Task mode | `FRONTEND` |
| Model | Claude Sonnet 5.5 |
| Tanggal | 2026-11-17 |
| Status | 🟡 Source selesai dan lulus lint serta uji unit; **`npm run build` dan `npm run dev` NOT RUN** (dilarang); `VISUAL CHECK: NOT FEASIBLE`; `MANUAL TEST: NOT FEASIBLE`; bergantung pada backend yang belum dikompilasi |

---

## 1. Masalah dan akar penyebab

Layar menampilkan "Total Penerima Payroll: 0" dan "Data profil penggajian tidak ditemukan." karena API-nya berangkat dari `PrlPayroll`. Mengubah teks kosong di frontend tidak akan memperbaikinya; akar masalah ada di backend (`PAY-BE-FIX-001`). Frontend diperbaiki agar:

1. menampilkan orang yang profil payroll-nya belum ada tanpa menganggapnya aktif;
2. menamai angka ringkasan dengan benar (tiga definisi berbeda);
3. tidak menyamakan galat API dengan hasil pencarian kosong;
4. membuka detail walaupun `payrollProfileId` null.

## 2. Perubahan

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/payroll/payroll-constants.js` | Opsi filter virtual **Belum Disiapkan** (`NotPrepared`); label kesiapan "Siap diproses"; kunci badge baru (`belum disiapkan`, `tidak eligible`, `profil perlu perhatian`, `belum diketahui`, `lengkap`, `belum lengkap`, `tidak aktif di hrd`); objek `COMPENSATION_TEXT` (hero, catatan sumber data, tiga teks keadaan kosong, pesan galat) |
| `src/utils/payroll/payroll-compensation-utils.js` | `normalizeCompensationItem` aman null: `hasPayrollProfile` (diturunkan dari `payrollProfileId` bila backend lama), `payrollStatus` null bukan string kosong, field baru. Fungsi murni baru: `getCompensationProfileBadgeKey`, `normalizeCompensationSummary`, `buildCompensationSummaryCards`, `buildCompensationNotices`, `resolveCompensationEmptyState`, `describeUnavailableAmount` |
| `src/lib/services/payroll/payroll-compensation-service.js` | `summary(signal)` → `GET …/workforce-compensations/summary` |
| `src/lib/hooks/payroll/use-payroll-compensation-list.js` | Ringkasan dimuat terpisah dari daftar; Refresh memuat ulang keduanya; kegagalan ringkasan tidak menutup daftar; teks keadaan kosong dibedakan menurut penyebab |
| `src/components/view/payroll/compensation/payroll-compensation-view.jsx` | Tiga kartu (Total SDM HRD, Profil Payroll Aktif, Siap Diproses), catatan sumber data, peringatan ringkasan gagal, catatan jumlah belum disiapkan, tombol Refresh, keadaan kosong |
| `src/components/view/payroll/compensation/payroll-compensation-table-columns.jsx` | Kolom "Status Penggajian" memakai badge dari `getCompensationProfileBadgeKey` (Belum Disiapkan / Aktif / Ditahan / Ditangguhkan / Berhenti / Tidak Eligible); nominal tidak tersedia dibedakan ("Tidak ditampilkan" vs "Belum tersedia"); "Pajak & Iuran" menandai tetapan yang belum ditetapkan. **Semua kolom lama dipertahankan** |
| `src/components/view/payroll/compensation/payroll-compensation-detail-view.jsx` | Detail menampilkan status profil, persiapan profil, kelengkapan data HRD, tetapan PPh dan iuran; catatan bila profil belum ada; daftar penghalang dan catatan. Tidak menampilkan UUID profil |
| `tests/unit/payroll-compensation-profile.test.mjs` | **Baru**, 17 uji (lihat bagian 5) |
| `tests/unit/payroll-data-readiness.test.mjs` | Dua uji kontrak lama disesuaikan: kosakata dan aturan profil kini dibaca dari `PayrollReadinessEvaluator` (sumber bersama), bukan dari `PayrollDataReadinessService`. Maksud uji dipertahankan; ditambah pemeriksaan bahwa service tidak mendefinisikan ulang kosakata dan bahwa evaluator murni |

Tidak diubah: `payroll-compensation-detail` hook (`use-payroll-compensation-detail.js`; detail tetap memakai `WorkforceProfileId`), halaman `page.jsx`, CSS.

## 3. Keputusan komponen dasar (gerbang REUSE > EXTEND > COMPOSE > WRAP > NEW)

| Elemen | Status | Bukti |
| --- | --- | --- |
| Judul halaman | `REUSE` | `Hero` |
| Kartu ringkasan | `REUSE` | `SummaryCards` (`summary-grid`), field `subtitle` yang sudah didukung |
| Filter, pencarian, reset | `REUSE` | `DataFilter`, `FilterSelect` |
| Tombol Refresh | `REUSE` | `BaseButton variant="secondary"` pada slot `actions` `DataFilter` (pola sama dengan banyak layar master data) |
| Tabel dan keadaan kosong | `REUSE` | `DataTable` (`emptyTitle`, `emptyDescription`) |
| Badge status | `REUSE` | `StatusBadge` dengan `PAYROLL_COMPENSATION_BADGE_CONFIG` (kelas `region-status-pending/warning/inactive` sudah ada di `status-badge.css`) |
| Catatan dan peringatan | `REUSE` | `InformationAlert`, `PayrollNoticeList` |
| Detail | `REUSE` | `BaseDetailView`, `PayrollSection` |
| CSS | Sumber (a) gaya bawaan base component | Tidak ada kelas CSS baru; tidak ada Tailwind; tidak ada warna literal |

`UI GATE`: tidak ada elemen `EXTEND`, `COMPOSE`, `WRAP`, atau `NEW`; tidak ada keputusan yang menunggu user.

## 4. Perilaku layar

| Keadaan | Tampilan |
| --- | --- |
| Orang HRD tanpa profil payroll | Baris tetap tampil; badge "Belum Disiapkan"; teks pendukung dari backend (mis. "Akan disiapkan otomatis"); penghalang "Profil Payroll belum disiapkan secara otomatis."; kesiapan "Perlu Diperbaiki" |
| Profil Aktif + eligible | Badge "Aktif" |
| Profil `OnHold` / `Suspended` / `Terminated` | "Ditahan" / "Ditangguhkan" / "Berhenti" |
| Aktif tetapi tidak eligible | "Tidak Eligible" |
| Status profil kosong/tidak dikenal | "Belum Diketahui" — tidak pernah "Aktif" |
| Nominal tanpa izin `ViewAmount` | "Tidak ditampilkan" |
| Nominal tersedia hak tetapi data belum ada | "Belum tersedia" (bukan Rp0) |
| API gagal | Galat ditampilkan; keadaan kosong memakai "Gagal memuat data profil penggajian. Silakan coba lagi." |
| Filter tidak menemukan hasil | "Tidak ditemukan penerima yang sesuai dengan filter." |
| Tidak ada data HRD relevan | "Belum ada pegawai atau dokter yang memenuhi kriteria." |
| Ringkasan gagal dimuat | Kartu kosong + peringatan; daftar tetap bisa dipakai; tidak ada angka karangan |
| Detail tanpa `payrollProfileId` | Terbuka; catatan "Profil payroll penerima ini belum dibuat…"; tanpa halaman galat |

Layar tetap baca-saja: tidak ada impor profil, unggah berkas, atau tombol yang mengubah data HRD atau Payroll; keduanya diperiksa lewat uji teks.

## 5. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/payroll-compensation-profile.test.mjs` | 17 uji, 17 lulus, 0 dilewati | `AUTOMATED TEST: PASS` |
| Seluruh uji Payroll (`payroll-*.test.mjs`, `workforce-payroll-*.test.mjs`) | 274 uji, 274 lulus, 0 dilewati (uji kontrak yang membaca source backend ikut berjalan) | `AUTOMATED TEST: PASS` |
| Uji kecocokan dengan backend | Alamat `summary`, izin `Read`, nama properti DTO frontend vs `PayrollCompensationDtos.cs`, dan bahwa service backend tidak menulis; dicocokkan dengan `[Route]` controller backend, bukan dengan service frontend | `PASS` (uji teks, bukan runtime) |
| `npx eslint` pada berkas yang diubah | 0 error; 2 warning `react-hooks/set-state-in-effect`: satu di `use-payroll-compensation-list.js` (sudah ada di HEAD, jumlahnya tidak bertambah), satu di `use-payroll-compensation-detail.js` (tidak diubah) | `PASS` dengan peringatan lama |
| `node …/rules/frontend/tools/check-css-usage.mjs` pada tiga berkas JSX | `OK — 3 berkas diperiksa, 0 temuan` | `PASS` |
| Grep anti-regresi (kelas Tailwind, warna literal) | Kosong | `PASS` |
| `npm run build`, `npm run dev` | Tidak dijalankan | `NOT RUN` (larangan eksplisit) |
| Tampilan di peramban, uji klik, uji filter hidup | Tidak dilakukan | `VISUAL CHECK: NOT FEASIBLE` / `MANUAL TEST: NOT FEASIBLE` — dev server dilarang dan backend baru belum dikompilasi; tidak ada screenshot |

Skenario TC yang relevan bagi frontend (TC-08 `PayrollProfileId` null tanpa error, TC-18 tanpa impor CSV, TC-19 galat API bukan daftar kosong) ditangani dan diuji di tingkat fungsi murni dan teks source; **tidak ada yang diverifikasi di peramban**.

## 6. Risiko dan hal yang belum selesai

| Hal | Isi |
| --- | --- |
| Bergantung pada backend baru | Frontend ini memanggil `GET …/summary`; dengan backend lama jawabannya 404 dan layar menampilkan peringatan ringkasan gagal (daftar tetap bekerja). Deploy bersamaan |
| Backend belum dikompilasi | `PAY-BE-FIX-001` belum di-`dotnet build`; kegagalan kompilasi di sana menghentikan seluruh layar |
| Tidak ada cek visual | Susunan kartu, lebar kolom, dan badge baru belum dilihat. Segera periksa di peramban setelah backend siap |
| Penyebutan lain dari `Siap` | Label badge `siap` berubah dari "Siap Dihitung" menjadi "Siap Diproses" dan pilihan filter kesiapan menjadi "Siap diproses". Tidak ada uji lain yang menunjukkan ketergantungan pada label lama |
| Tidak ada tombol ke menu HRD | Catatan sumber data hanya teks yang menunjuk menu HRD; tidak ada tombol yang mengubah data atau tautan baru, supaya tidak menambah jalur ubah dari layar Payroll |
| Refresh dan ringkasan | Saat Refresh, angka ringkasan lama tetap tampil sampai yang baru tiba |

---

## 7. Addendum 2026-11-17 (2) — Detail Profil Penggajian (`/payroll/workforce-compensations/[id]`)

Master prompt lanjutan untuk detail penerima. Dependency backend: bagian 9 pada [`PAY-BE-FIX-001`](../backend/PAY-BE-FIX-001.md). Tanpa `npm run build`/`dev`; `VISUAL CHECK: NOT FEASIBLE`.

### 7.1 Temuan

Route `[id]/page.jsx` meneruskan `id` sebagai `workforceProfileId`, dan daftar membuka detail dengan `item.workforceProfileId`, jadi identifier list dan detail sudah konsisten. Teks alert pada screenshot berasal dari backend lama, bukan dari frontend; frontend sebelumnya hanya menampilkan pesan server apa adanya dan menyisakan area detail kosong. Tombol Tunjangan Transportasi nonaktif pada screenshot karena `!detail || loading`, bukan karena aturan fitur: backend transportasi tidak bergantung pada `PrlPayroll`.

### 7.2 Perubahan

| Berkas | Perubahan |
| --- | --- |
| `src/lib/hooks/payroll/use-payroll-compensation-detail.js` | State berkunci per ID dan nomor percobaan (`settled.key === requestKey`): data penerima lama tidak tampil saat ID berubah dan response lambat dari permintaan lama tidak menimpa yang baru; guard `controller.signal.aborted`; tanpa `setState` sinkron di efek (warning ESLint hook ini hilang); `reload` memanggil ulang endpoint yang sama |
| `src/utils/payroll/payroll-compensation-utils.js` | Normalisasi aman null untuk `operationalProfileReady` (diturunkan dari status profil bila backend lama), `salaryStatus` (tidak ditebak), dan pada detail `payrollNumber`, `paymentFrequency`, masa berlaku profil, `organizationUnitName`, `departmentName`, `positionName`; `getCompensationProfileBadgeKey` memakai `operationalProfileReady`; fungsi murni baru `resolveCompensationDetailError` dan `buildCompensationDetailSubtitle` |
| `src/lib/constants/payroll/payroll-constants.js` | Teks detail (judul, 404, galat API, Coba Lagi, siap); kunci badge status penetapan gaji (Belum Ada Penetapan Gaji, Menunggu Persetujuan HRD, Disetujui, Belum Berlaku, Sudah Berakhir, Konflik Penetapan Gaji); kunci `profil perlu perhatian` menggantikan `profil tidak aktif` |
| `src/components/view/payroll/compensation/payroll-compensation-detail-view.jsx` | **Ditulis ulang** (lihat 7.3 dan 7.4) |
| `tests/unit/payroll-compensation-profile.test.mjs` | Dari 17 menjadi 26 uji (galat detail, subjudul, hook berkunci, urutan baca backend, kecocokan label status gaji dengan backend, pemilihan profil bersama) |

### 7.3 Keadaan yang dibedakan

| Keadaan | Tampilan |
| --- | --- |
| Identitas HRD ada, profil payroll belum ada | Detail terbuka; badge "Belum Disiapkan"; catatan "Profil payroll penerima ini belum dibuat…"; eligible "-"; semua bagian tetap tampil |
| Identitas HRD tidak ada (404) | "Data pegawai atau dokter tidak ditemukan di HRD." + tombol Kembali; tanpa Coba Lagi |
| Tanpa izin (403) | `AccessDeniedGate` |
| API atau jaringan gagal (network/server/request) | "Gagal memuat detail profil penggajian." + tombol **Coba Lagi** yang memanggil ulang endpoint; pesan mentah server tidak ditampilkan |
| `salaryHistory`, `bankAccounts`, `issues` kosong | Array kosong; bagian tetap tampil dengan keadaan kosong, bukan galat |
| Nominal tanpa `ViewAmount` / belum ada | "Tidak ditampilkan" / "Belum tersedia"; tidak pernah Rp0 |

### 7.4 Susunan layar

Header: eyebrow `Payroll > Penghasilan Pegawai > Profil Penggajian`, judul "Detail Profil Penggajian", subjudul "Nama · Nomor induk · Jenis penerima"; tombol Kembali, Coba Lagi (hanya galat API), Tunjangan Transportasi (sesuai izin; nonaktif hanya saat memuat atau tanpa detail).

Urutan: kartu Informasi Penerima (`BaseDetailCard`: nama, nomor induk, jenis, unit, departemen, jabatan, kelengkapan data HRD) → pita Status Profil (badge profil, penetapan gaji, rekening, kesiapan) → Status Penggajian → Penetapan Gaji → Riwayat Penetapan Gaji → Rekening Payroll → Pajak dan Iuran → Kesiapan Data (daftar masalah dari backend atau pernyataan siap; catatan).

**Selisih dari mockup:** mockup menempatkan Informasi Penerima dan Status Penggajian berdampingan dua kolom. Itu tidak dibuat: `payroll-run.module.css` hanya memiliki grid 4 kolom yang menyusut menjadi 2 dan 1 kolom, sedangkan dua kolom berdampingan memerlukan CSS baru dan grid 4 kolom di setengah lebar akan sempit. Section dibuat bertumpuk memakai `PayrollSection` + `PayrollInfoGrid` yang sudah responsif (desktop 4 kolom, tablet 2, ponsel 1) tanpa overflow horizontal. Bila dua kolom diinginkan, itu butuh keputusan CSS baru.

### 7.5 Keputusan komponen dasar

Semua `REUSE`/komponen payroll yang sudah ada: `BaseDetailView`, `BaseDetailCard` (lewat `BaseDetailView`), `PayrollSection`, `PayrollInfoGrid`, `PayrollNoticeList`, `DataTable`, `StatusBadge`, `InformationAlert`, `BaseButton`. Gaya dari `payroll-run.module.css` yang sudah ada (`statusRow`, `statusItem`, `infoLabel`). Tidak ada komponen atau kelas CSS baru. `UI GATE`: tidak ada elemen `EXTEND`/`COMPOSE`/`NEW`; tidak ada keputusan yang menunggu user.

### 7.6 Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `payroll-compensation-profile.test.mjs` | 26 uji, 26 lulus, 0 dilewati | `AUTOMATED TEST: PASS` |
| Seluruh uji Payroll (`payroll-*`, `workforce-payroll-*`) | 283 uji, 283 lulus, 0 dilewati (uji kontrak yang membaca source backend ikut berjalan) | `AUTOMATED TEST: PASS` |
| `npx eslint` pada berkas yang diubah | 0 error; 1 warning `react-hooks/set-state-in-effect` pada `use-payroll-compensation-list.js` (sudah ada di HEAD) | `PASS` dengan peringatan lama |
| `check-css-usage.mjs` pada tiga berkas JSX | `OK — 3 berkas diperiksa, 0 temuan` | `PASS` |
| Grep anti-regresi (kelas Tailwind, warna literal) | Kosong | `PASS` |
| `npm run build`, `npm run dev`, tampilan, uji klik, uji Coba Lagi hidup | Tidak dijalankan | `NOT RUN` / `VISUAL CHECK: NOT FEASIBLE` / `MANUAL TEST: NOT FEASIBLE` |

TC-10, TC-11, TC-12, TC-13, TC-14, TC-19, TC-20 ditangani dan diuji pada fungsi murni dan teks source; **tidak ada yang diverifikasi di peramban**.

### 7.7 Risiko

| Hal | Isi |
| --- | --- |
| Screenshot yang sama akan tetap muncul sampai backend baru dibuild dan dijalankan | Penyebabnya ada di backend lama, bukan di frontend |
| Frontend ini dengan backend lama | `salaryStatus` null: badge penetapan gaji tidak dirender dan baris status tampil "-" (tidak ditebak) |
| Tidak ada cek visual | Susunan section, badge pita status, dan tombol baru belum dilihat |
| Tidak ada dua kolom | Lihat 7.4 |

---

## 8. Addendum 2026-11-17 (3) — uji kontrak untuk perbaikan 400 `POST payroll-runs`

Tidak ada source frontend yang diubah. Penyebab 400 "Cannot write DateTime with Kind=Unspecified …" ada di backend (tanggal periode berkolom `date` ditulis mentah ke snapshot `timestamptz`; lihat `PAY-BE-FIX-001` bagian 10). Satu berkas uji ditambahkan di repo frontend karena backend tidak memelihara project uji otomatis (`TEST_POLICY`), dan pola uji kontrak yang membaca source backend sudah dipakai di sini:

| Berkas | Isi | Hasil |
| --- | --- | --- |
| `tests/unit/payroll-run-utc-dates.test.mjs` (baru) | 11 uji yang membaca source backend: helper `PayrollDateNormalizer` mencap UTC dan tidak mengonversi zona; tidak ada snapshot tanggal periode mentah; kelima snapshot `CreateAsync` dinormalkan; tidak ada pembandingan `Effective*Date` mentah pada tiga layanan; `EnsureProfilesAsync` menormalkan; alias `using` pada layanan lama | **11 dari 11 lulus** |
| Seluruh uji Payroll (`payroll-*`, `workforce-payroll-*`) | 294 uji | **294 lulus, 0 gagal, 0 dilewati** (sebelumnya 283) |

Uji ini membaca teks source; ia **bukan** bukti bahwa 400 hilang di runtime. Bukti itu baru ada setelah backend dibuild, dijalankan ulang, dan run benar-benar dibuka.
