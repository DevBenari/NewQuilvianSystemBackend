# ISSUE-002 — Lima layar baru Administrasi Kepegawaian tampil tanpa gaya karena ditulis dengan class Tailwind yang tidak terpasang

```yaml
issue_id: ISSUE-HRD-002
module_id: human-resource
submodule: null   # blueprint berbentuk SINGLE (HRD-DEC-003); blueprint root = human-resource/
layar: "Administrasi Kepegawaian — Dokumen SK (FE-HRD-08), Buat SK (FE-HRD-09), Detail SK (FE-HRD-09a), Employee 360 (FE-HRD-09b), Batch Data Awal (FE-HRD-09c), Pengajuan Atas Nama (FE-HRD-09d)"
sumber_laporan: "Laporan pemilik 03-11-2026: 1 keluhan + 1 pertanyaan, 4 lampiran screenshot; ditambah butir tampilan pada laporan live testing 03-11-2026 (LBT-WFC-19, -20, -22, -23, -24, -25, -26)"
tanggal_issue: "2026-11-03"
status: DALAM_PERBAIKAN
keparahan_tertinggi: High
source_sha_backend: "26066c76e69b021f68b63b31bde2258cd9d1bb74 (MHamzah)"
source_sha_frontend: "66b7e5600e1707e4153ca07981ddef054856c8c9 (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-002-tampilan-workforce-core-tanpa-gaya.md
```

## 1. Ringkasan

Pelapor membuka layar **Batch Inisialisasi Penempatan Awal**, **Buat SK Kepegawaian**, dan
**Rincian SK Kepegawaian**. Ketiganya tampak seperti halaman yang belum selesai: label menempel pada
kotak isian, kotak isian berukuran acak, tidak ada kartu, lencana status hampir tidak terbaca, dan
kotak dialog "Buat Batch" muncul di tengah halaman, bukan sebagai jendela dialog.

Penyebabnya satu: kelima layar baru Administrasi Kepegawaian (ditambah daftar SK) ditulis memakai
nama class **Tailwind CSS**, misalnya `grid grid-cols-2`, `border-slate-300`, dan `w-full`.
Tailwind **tidak terpasang** di aplikasi Quilvian, sehingga semua class itu tidak berarti apa-apa
bagi peramban. Hasilnya, halaman tampil dengan gaya bawaan peramban yang paling polos.

Butir yang paling berbahaya bukan soal tampilan. Formulir SK dan formulir pengajuan atas nama
meminta petugas HR **mengetik kode UUID** pegawai, unit, departemen, jabatan, dan atasan.
Akibatnya, formulir itu praktis tidak dapat dipakai tanpa bantuan programmer.

Tidak ada butir yang sesuai desain. Semuanya menyimpang dari aturan tampilan Quilvian
(`rules/frontend/`) dan dari skema layar pada `03-frontend-architecture.md` bagian 4.2a dan 4.2b.

## 2. Laporan asli

| No. laporan | Keluhan (kata-kata pelapor, diringkas) | Lampiran |
| --- | --- | --- |
| 1 | "Banyak tampilan yang belum ada CSS." | Lampiran 1: `/hr/workforce-core/initial-batches`, 03-11-2026 10.02. Lampiran 2: `/hr/workforce-core/personnel-actions/{id}` (SK berstatus Draf, pesan tolak pengajuan), 10.08. Lampiran 3: `/hr/workforce-core/personnel-actions/create`, 10.13. Lampiran 4: `/hr/workforce-core/personnel-actions/{id}` (SK berisi gaji pokok negatif), 10.14 |
| 2 | "Kenapa bisa terjadi masalah seperti ini, apakah agent yang mengerjakan CSS-nya?" | — (pertanyaan) |
| LBT-WFC-19 | Lima layar baru tampil polos/berantakan karena class Tailwind | Laporan live testing bagian 6 P4 |
| LBT-WFC-20 | Lencana "Diterapkan" di detail SK hampir tidak terlihat | Laporan live testing bagian 6 P4 |
| LBT-WFC-22 | Lencana "Dibatalkan" di daftar SK terpotong dua baris | Laporan live testing bagian 6 P4 |
| LBT-WFC-23 | Pesan teknis mentah "Request failed with status code 400/404/415" | Laporan live testing bagian 6 P5; terlihat pada lampiran 1 |
| LBT-WFC-24 | Setelah satu scan terunggah, scan yang salah tidak bisa diganti | Laporan live testing bagian 6 P5 |
| LBT-WFC-25 | Menu SK, Employee 360, dan Batch tidak ada di sidebar | Laporan live testing bagian 6 P5 |
| LBT-WFC-26 | Form SK meminta UUID diketik manual | Laporan live testing bagian 6 P5; terlihat pada lampiran 3 |

Keempat lampiran dirujuk butir 1. Lampiran 4 juga memperlihatkan gaji pokok negatif yang lolos;
itu temuan LBT-WFC-11 dari laporan testing dan tidak dibahas ulang di sini.

## 3. Ringkasan temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `ISS-HRD-002-01` | 1, LBT-WFC-19 | Lima layar ditulis dengan class Tailwind yang tidak terpasang | `RULE_VIOLATION` | Frontend | Medium | SUDAH-VERIFIKASI | `FIX-HRD-002-01` s.d. `-07` |
| `ISS-HRD-002-02` | LBT-WFC-20, LBT-WFC-22 | Lencana status salah pakai sehingga tidak terbaca | `BUG` | Frontend | Low | SUDAH-VERIFIKASI | `FIX-HRD-002-02`, `-04`, `-05`, `-06` |
| `ISS-HRD-002-03` | 1 (lampiran 2–4) | Tombol "Kembali", "Batal", dan "Lihat Employee 360" tampil sama dengan tombol utama | `BUG` | Frontend | Low | SUDAH-VERIFIKASI | `FIX-HRD-002-02` s.d. `-07` |
| `ISS-HRD-002-04` | LBT-WFC-26, lampiran 3 | Formulir meminta petugas mengetik UUID | `RULE_VIOLATION` | Frontend | High | SUDAH-VERIFIKASI | `FIX-HRD-002-03`, `-07` |
| `ISS-HRD-002-05` | 1 (lampiran 1) | Kotak dialog Buat Batch, Tolak SK, dan Batalkan SK buatan sendiri dan tidak tampil sebagai dialog | `RULE_VIOLATION` | Frontend | Medium | SUDAH-VERIFIKASI | `FIX-HRD-002-04`, `-06` |
| `ISS-HRD-002-06` | LBT-WFC-23, lampiran 1 | Pesan galat teknis berbahasa Inggris | `RULE_VIOLATION` | Frontend | Medium | SUDAH-VERIFIKASI | `FIX-HRD-002-02` s.d. `-07` |
| `ISS-HRD-002-07` | LBT-WFC-24 | Scan SK yang salah tidak dapat diganti dan tidak dapat diunduh | `BUG` | Frontend | Medium | SUDAH-VERIFIKASI | `FIX-HRD-002-04` |
| `ISS-HRD-002-08` | LBT-WFC-25 | Menu Dokumen SK dan Batch Data Awal tidak ada di sidebar | `BUG` | Frontend | Medium | SUDAH-VERIFIKASI | `FIX-HRD-002-08` |

## 4. Rincian per temuan

### ISS-HRD-002-01 — Lima layar ditulis dengan class Tailwind yang tidak terpasang

| | |
| --- | --- |
| **No. laporan** | 1, LBT-WFC-19 |
| **Jenis** | `RULE_VIOLATION` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di source dan pada lampiran 1–4 |
| **Perbaikan** | `FIX-HRD-002-01` s.d. `FIX-HRD-002-07` |

**Apa yang terjadi.** Halaman Buat SK, Rincian SK, Batch Data Awal, Employee 360, dan Pengajuan Atas
Nama tampil tanpa tata letak. Label menempel ke kotak isian, kotak isian tidak sejajar, kartu
informasi berubah menjadi teks polos bertumpuk, dan tab Employee 360 tampil sebagai tombol
abu-abu biasa. Petugas HR sulit membedakan bagian satu dengan bagian lain.

**Kenapa terjadi.**

```text
halaman tampil polos
  ← sebab langsung: elemen diberi class seperti "grid grid-cols-1 md:grid-cols-2 gap-6",
    "border-slate-300", "w-full", "text-slate-700" yang tidak didefinisikan di CSS mana pun
    ← akar: view ditulis dengan kosakata Tailwind CSS, padahal Tailwind tidak terpasang;
      gerbang mutu yang seharusnya menangkapnya (keputusan base component, grep anti-regresi,
      uji visual) tidak dijalankan, dan grep yang ada pun tidak mengenali class Tailwind
```

Bukti, seluruhnya `SUDAH-VERIFIKASI` pada SHA frontend di metadata:

- `package.json` tidak memuat `tailwindcss`, dan tidak ada `tailwind.config.*` maupun
  `postcss.config.*`. `src/app/layout.js:1-8` hanya memuat Bootstrap 5, `style.css`, dan
  `globals.css`.
- Pemeriksaan pada seluruh `src/style/**` dan `src/app/*.css`: `text-slate-700`, `border-slate-300`,
  `rounded-md`, `grid-cols-2`, `flex-col`, `space-y-4`, `w-full`, `items-center`, dan `justify-end`
  **tidak didefinisikan di berkas CSS mana pun**.
- Jumlah `className` literal per view: Buat SK 34, Detail SK 81, Batch 43, Employee 360 115,
  Pengajuan Atas Nama 26. Tidak satu pun view memakai CSS Module domain.
- Dua kunci CSS Module yang dipakai di keenam view tidak ada di modulnya:
  `baseStyles.container` dan `themeStyles.themeContainer`. Daftar SK juga memakai
  `tableStyles.tableWrapper` yang tidak ada. Ketiganya bernilai `undefined` saat berjalan.

`src/components/view/hr/workforce-core/personnel-action-order-create-view.jsx:126-141`

```jsx
<form onSubmit={handleSubmit} className="mt-6 bg-white p-6 rounded-lg border border-slate-200 shadow-sm">
  <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
    <div>
      <label className="block text-sm font-medium text-slate-700 mb-1">
        ID Pegawai (Workforce Profile ID) <span className="text-red-500">*</span>
      </label>
      <input
        type="text"
        name="workforceProfileId"
        ...
        className="w-full px-3 py-2 border border-slate-300 rounded-md focus:outline-none focus:ring-1 focus:ring-primary text-sm font-mono"
```

Kelima view masuk ke repository dalam satu commit `66b7e5600` (2 November 2026) yang berjudul
"feat(tests): add unit tests for inpatient discharge follow-up, EWS display, monitoring alerts, and
workforce core services". Judul itu tidak menyebut layar baru, sehingga peninjau commit tidak
punya alasan untuk membuka tampilannya.

**Apakah ini menyimpang dari desain?** Ya, `RULE_VIOLATION`. Aturan global
`rules/frontend/frontend-architecture.md` bagian `src/style` mewajibkan CSS Module untuk style
baru, dan `rules/frontend/design-tokens.md` mewajibkan nilai visual diambil dari token `var(--…)`.
Skema layar `03-frontend-architecture.md` 4.2a dan 4.2b menggambarkan kartu, tab, dan tabel
standar.

**Dampak nyata.** Petugas HR yang membuka Rincian SK "SK-RSMMC-…-0001" untuk perawat A harus
membaca daftar label dan nilai yang bertumpuk tanpa batas kartu. Tombol "Ajukan Persetujuan" dan
"Batalkan SK" tampil sama besar dan sama warna dengan tombol "Kembali", sehingga risiko salah tekan
naik.

**Rekomendasi.** Bangun ulang tampilan kelima layar dengan base component Quilvian dan satu CSS
Module domain berbasis token, mengikuti layar Exit Clearance sebagai rujukan visual. Rinciannya
ada di plan repair `FIX-HRD-002-01` s.d. `-07`.

### ISS-HRD-002-02 — Lencana status salah pakai sehingga tidak terbaca

| | |
| --- | --- |
| **No. laporan** | LBT-WFC-20, LBT-WFC-22 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI di source; terlihat pada lampiran 2 dan 4 |
| **Perbaikan** | `FIX-HRD-002-02`, `-04`, `-05`, `-06` |

**Apa yang terjadi.** Lencana "Draf" pada lampiran 2 dan 4 tampil sebagai lingkaran biru dengan
tulisan putih yang nyaris tidak terbaca. Lencana "Diterapkan" bahkan putih di atas putih.

**Kenapa terjadi.** `StatusBadge` menerima `status` berupa salah satu kunci `active`, `inactive`,
`pending`, `warning`, `info`, atau `danger`, lalu memasang class `region-status-<kunci>`
(`src/components/features/base-features/status-badge.jsx:52-53` dan `:107-113`). View mengirim
**label Bahasa Indonesia** sebagai `status`, dan nilai `variant` diteruskan ke Bootstrap sebagai
`bg-<variant>`:

`src/components/view/hr/workforce-core/personnel-action-order-detail-view.jsx:227`

```jsx
<StatusBadge status={statusCfg.label} variant={statusCfg.variant} />
```

Hasilnya class `region-status-diterapkan` dan `bg-active`, dan keduanya tidak ada. Pola yang sama
dipakai di daftar SK (`personnel-action-order-list-view.jsx:198`), Batch (`:272-275`), dan Employee
360 (`:119-122`).

**Apakah ini menyimpang dari desain?** Ya, `BUG`. `rules/frontend/base-component-catalog.md`
bagian StatusBadge menyebut kunci status yang sah dan contoh `label` terpisah.

**Dampak nyata.** HR Manager tidak dapat membedakan SK yang sudah berlaku dari SK yang masih draf
hanya dengan melihat lencananya.

**Rekomendasi.** Petakan status backend ke kunci `StatusBadge` di satu utilitas bersama, lalu kirim
labelnya lewat prop `label`.

### ISS-HRD-002-03 — Tombol sekunder tampil sama dengan tombol utama

| | |
| --- | --- |
| **No. laporan** | 1 (lampiran 2, 3, 4) |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI di source dan pada lampiran |
| **Perbaikan** | `FIX-HRD-002-02` s.d. `-07` |

**Apa yang terjadi.** "← Kembali", "Lihat Employee 360", "Batal", dan "Lihat Rincian" tampil
dengan gradien hijau-biru yang sama dengan tombol utama.

**Kenapa terjadi.** View memakai `variant="outline"`. `BaseButton` hanya mengenal `primary`,
`secondary`, `danger`, `ghost`, `subtle`, `warning`, dan `success`, lalu diam-diam memakai `primary`
untuk nilai yang tidak dikenal:

`src/components/features/base-features/base-button.jsx:4-12, 36`

```jsx
const VARIANT_CLASS = Object.freeze({
  primary: buttonStyles.primary,
  secondary: buttonStyles.secondary,
  danger: buttonStyles.danger,
  ghost: buttonStyles.ghost,
  subtle: buttonStyles.subtle,
  warning: buttonStyles.warning,
  success: buttonStyles.success,
});
...
const variantClass = VARIANT_CLASS[variant] || VARIANT_CLASS.primary;
```

**Apakah ini menyimpang dari desain?** Ya, `BUG` terhadap katalog base component.

**Dampak nyata.** Tidak ada tombol yang menonjol sebagai aksi utama layar.

**Rekomendasi.** Pakai `secondary` untuk aksi navigasi dan batal, `primary` hanya untuk satu aksi
utama per wilayah.

### ISS-HRD-002-04 — Formulir meminta petugas mengetik UUID

| | |
| --- | --- |
| **No. laporan** | LBT-WFC-26, lampiran 3 |
| **Jenis** | `RULE_VIOLATION` |
| **Area** | Frontend |
| **Keparahan** | High |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-HRD-002-03`, `FIX-HRD-002-07` |

**Apa yang terjadi.** Untuk menerbitkan SK mutasi, petugas HR harus mengetik
"11111111-1111-1111-1111-111111111111" untuk pegawai, lalu UUID unit, departemen, jabatan, dan
atasan. Petugas HR tidak pernah melihat UUID di layar mana pun, sehingga formulir tidak dapat
dipakai tanpa programmer. Formulir Pengajuan Atas Nama punya masalah yang sama untuk pegawai.
Employee 360 juga menampilkan UUID atasan ("ID Atasan") ke pengguna.

**Kenapa terjadi.** Formulir diturunkan langsung dari DTO backend (`CreatePersonnelActionOrderRequest`
berisi `Guid`), dan setiap `Guid` dijadikan `<input type="text">`
(`personnel-action-order-create-view.jsx:133-141, 205-257`;
`employee-profile-change-on-behalf-view.jsx:133-141`; `employee-360-view.jsx:212-217`).

Padahal pilihan siap pakai sudah ada:

- `departments`, `positions`, dan `organizationUnits` terdaftar di
  `src/lib/hooks/select/hr/hr-select-resources.js:76-106, 310-333`.
- Endpoint opsi karyawan sudah mengirim `workforceProfileId`
  (`EmployeeController.cs` action `options`, `EmployeeOptionResponse.WorkforceProfileId`).
- Modul Hemodialisa sudah memakainya sebagai pilihan bernilai `workforceProfileId`
  (`use-hemodialysis-schedule-dialog.jsx:39-46`).

**Apakah ini menyimpang dari desain?** Ya, `RULE_VIOLATION`. Daftar periksa diagnosis Quilvian
melarang pengenal teknis diketik atau ditampilkan ke pengguna, dan skema 4.2a menampilkan nama
pegawai dan nama unit, bukan kode.

**Dampak nyata.** HR tidak dapat menerbitkan SK mutasi perawat ke IGD tanpa membuka database.

**Rekomendasi.** Ganti setiap isian UUID dengan daftar pilihan bercari (`ResourceFilterSelect`) yang
menampilkan nama dan mengirim id. Tampilkan jabatan/departemen atasan, bukan UUID-nya.

### ISS-HRD-002-05 — Kotak dialog buatan sendiri tidak tampil sebagai dialog

| | |
| --- | --- |
| **No. laporan** | 1 (lampiran 1) |
| **Jenis** | `RULE_VIOLATION` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-HRD-002-04`, `FIX-HRD-002-06` |

**Apa yang terjadi.** Pada lampiran 1, kotak "Buat Batch Inisialisasi Baru" tampil menempel di
bawah tombol, tanpa latar gelap, tanpa tombol tutup, dan tidak menghalangi halaman di belakangnya.
Dialog Tolak SK dan Batalkan SK di Rincian SK berperilaku sama.

**Kenapa terjadi.** Ketiganya dibuat sendiri dengan class Tailwind
`fixed inset-0 bg-slate-900/50 flex items-center justify-center z-50`
(`initial-placement-batch-view.jsx:297`, `personnel-action-order-detail-view.jsx:456, 483`). Tanpa
Tailwind, class itu tidak memberi posisi tetap maupun latar, sehingga "dialog" menjadi blok biasa
di aliran halaman. Base component `ConfirmModal` sudah menyediakan dialog dengan alasan wajib
(`requireReason`), tetapi tidak dipakai.

**Apakah ini menyimpang dari desain?** Ya. Katalog base component menetapkan `ConfirmModal` untuk
konfirmasi dan alasan.

**Rekomendasi.** Pakai `ConfirmModal`. Untuk Tolak dan Batal, aktifkan `requireReason`.

### ISS-HRD-002-06 — Pesan galat teknis berbahasa Inggris

| | |
| --- | --- |
| **No. laporan** | LBT-WFC-23, lampiran 1 |
| **Jenis** | `RULE_VIOLATION` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-HRD-002-02` s.d. `-07` |

**Apa yang terjadi.** Lampiran 1 menampilkan "Request failed with status code 404". Petugas tidak
tahu artinya maupun apa yang harus dilakukan.

**Kenapa terjadi.** Setiap view menyusun pesan dengan
`err?.response?.data?.message || err?.message || "<fallback>"`. Ketika backend menjawab 404 tanpa
isi, `err.message` milik Axios yang berbahasa Inggris dipakai lebih dulu daripada fallback Bahasa
Indonesia (misalnya `initial-placement-batch-view.jsx:54-58`).

**Rekomendasi.** Satu utilitas pesan galat yang memprioritaskan pesan backend, lalu memetakan kode
HTTP ke kalimat Bahasa Indonesia, dan tidak pernah menampilkan `err.message` Axios.

### ISS-HRD-002-07 — Scan SK yang salah tidak dapat diganti dan tidak dapat diunduh

| | |
| --- | --- |
| **No. laporan** | LBT-WFC-24 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-HRD-002-04` |

**Apa yang terjadi.** Setelah satu berkas terunggah, formulir unggah hilang dan berkas tidak dapat
dibuka. Bila petugas salah memilih berkas, misalnya SK pegawai lain, SK itu tidak bisa dibetulkan
sebelum diajukan.

**Kenapa terjadi.** Formulir unggah hanya dirender bila `signedDocumentPath` kosong
(`personnel-action-order-detail-view.jsx:371-404`), dan view tidak memakai
`GET .../personnel-action-orders/{id}/attachments/{attachmentId}`. Backend menerima unggahan
berikutnya dan memakai berkas terakhir sebagai dokumen bertanda tangan
(`HrdPersonnelActionOrderService.cs:272-273`).

**Apakah ini menyimpang dari desain?** Ya, `BUG`. Skema 4.2a FE-HRD-09a memuat "Pratinjau PDF SK"
dan tombol "[Unduh Berkas]".

**Rekomendasi.** Tampilkan daftar lampiran dengan tombol Unduh, dan sediakan "Ganti Berkas" selama
SK berstatus Draf.

### ISS-HRD-002-08 — Menu Dokumen SK dan Batch Data Awal tidak ada di sidebar

| | |
| --- | --- |
| **No. laporan** | LBT-WFC-25 |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-HRD-002-08` |

**Apa yang terjadi.** Petugas harus mengetik alamat halaman untuk membuka Dokumen SK maupun Batch
Data Awal.

**Kenapa terjadi.** `src/utils/menu-sidebar/menu-items.jsx:609-650` hanya memuat enam layar rekap.

**Apakah ini menyimpang dari desain?** Ya, `BUG`. Tabel menu `03-frontend-architecture.md` (baris
"Dokumen SK Kepegawaian" dan "Batch Data Awal") menetapkan keduanya sebagai menu tingkat 2 di
bawah Administrasi Kepegawaian. Employee 360 dan Pengajuan Atas Nama **tidak** dirancang sebagai
menu sidebar: Employee 360 dibuka dari Rincian SK, dan Pengajuan Atas Nama dari tombol pada layar
Perubahan Data Karyawan.

**Rekomendasi.** Tambahkan dua menu sesuai tabel desain.

## 5. Tanya-jawab pelapor

> **T:** Kenapa bisa terjadi masalah seperti ini, apakah agent yang mengerjakan CSS-nya?
>
> **J:** Ya. Layar-layar ini dikerjakan oleh AI agent, dan agent menulis class Tailwind CSS karena
> terbiasa memakai Tailwind dari proyek lain. Quilvian tidak memakai Tailwind, sehingga class itu
> tidak berpengaruh sama sekali. Kesalahan ini lolos karena empat lapis pengaman yang semestinya
> menangkapnya tidak bekerja:
>
> 1. **Gerbang keputusan base component dilewati.** Skill `build-module-frontend` langkah 3
>    mewajibkan tabel keputusan `REUSE`/`EXTEND`/`NEW` sebelum JSX pertama ditulis. Laporan task
>    `FE-HRD-024` s.d. `FE-HRD-028` tidak memuat tabel itu, baris `UI GATE`, hasil checklist UI,
>    maupun uji manual. Isinya hanya satu baris "uji unit".
> 2. **"Uji unit"-nya tidak menguji tampilan.** `tests/unit/workforce-core-target-state.test.mjs`
>    hanya memeriksa bahwa berkas ada dan berisi teks tertentu. Untuk Employee 360 dan Batch, test
>    itu bahkan **mengunci alamat API yang salah** (`:122-124`, `:154-155`).
> 3. **Grep anti-regresi tidak mengenal Tailwind.** Checklist `ui-consistency-checklist.md` bagian G
>    hanya mencari warna literal, `font-size`, tombol mentah, tabel tanpa `data-flat-table`, class
>    tipografi Bootstrap, dan `!important`. Tidak ada pemeriksaan class yang tidak didefinisikan, nilai
>    prop base component yang tidak sah (`variant="outline"`, `StatusBadge status="Draf"`), atau kunci
>    CSS Module yang tidak ada. Grep #3 dan #4 sebenarnya akan menangkap tombol dan tabel buatan
>    sendiri di layar Batch dan Employee 360, tetapi grep itu tidak dijalankan.
> 4. **Build dan lint tidak bisa menangkapnya.** Bagi JavaScript, `className="grid grid-cols-2"`
>    hanya teks, sehingga `next build` dan ESLint lulus. Yang menangkapnya hanyalah mata manusia
>    atau screenshot, dan langkah itu tidak dikerjakan.
>
> Perbaikan dilakukan di dua tempat: tampilannya dibangun ulang (plan repair `FIX-HRD-002-01` s.d.
> `-08`), dan skill beserta aturannya diperkuat agar kesalahan yang sama tertangkap sebelum task
> dinyatakan selesai (`FIX-HRD-002-10`).

## 6. Temuan tambahan

### ISS-HRD-002-T1 — Saringan Status dan Kategori di Daftar SK tidak pernah tampil

| | |
| --- | --- |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-HRD-002-02` |
| **Kenapa dicantumkan** | Wilayah layar sama (Daftar SK); terlihat pada screenshot laporan testing `09-daftar-sk-akhir.png` |

**Apa yang terjadi.** Daftar SK hanya punya kotak cari. Saringan Status dan Kategori yang ada di
kode tidak muncul, dan tombol atur ulang tidak mengosongkan kotak cari.

**Kenapa terjadi.** `personnel-action-order-list-view.jsx:254, 260` mengirim `searchTerm` dan
`filterControls`, padahal `DataFilter` hanya mengenal `searchKeyword` dan `children`
(`data-filter.jsx:283, 290`).

**Rekomendasi.** Kirim saringan sebagai `children` dan nilai cari sebagai `searchKeyword`, seperti
layar rekap `workforce-core-cross-employee-list-view.jsx`.

### ISS-HRD-002-T2 — Kartu ringkasan Daftar SK hanya menghitung halaman yang sedang tampil

| | |
| --- | --- |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI |
| **Perbaikan** | `FIX-HRD-002-02` |
| **Kenapa dicantumkan** | Wilayah layar sama; bertentangan dengan skema 4.2a ("Ringkasan → GET …/summary") |

**Apa yang terjadi.** Bila ada 30 SK menunggu persetujuan dan yang tampil hanya halaman 1 berisi 25
baris, kartu "Menunggu Persetujuan" menghitung sebagian saja. Kartu "Dijadwalkan" yang ada di desain
tidak ada.

**Kenapa terjadi.** `personnel-action-order-list-view.jsx:129-140` menghitung dari `items` halaman
aktif. Endpoint `GET .../personnel-action-orders/summary` sudah tersedia di backend tetapi tidak
dipanggil.

**Rekomendasi.** Ambil angka dari `/summary`.

### ISS-HRD-002-T3 — Tombol Tolak dan Batalkan SK di layar selalu gagal HTTP 500

| | |
| --- | --- |
| **Jenis** | `CONTRACT_MISMATCH` |
| **Area** | Frontend |
| **Keparahan** | Blocker |
| **Status bukti** | SUDAH-VERIFIKASI (laporan live testing LBT-WFC-01) |
| **Perbaikan** | `FIX-HRD-002-04` |
| **Kenapa dicantumkan** | Ditulis di handler yang sama dengan dialog Tolak/Batal yang dibangun ulang pada `FIX-HRD-002-04` |

**Kenapa terjadi.** `personnel-action-order-detail-view.jsx:137, 158` mengirim `rejectionReason`
dan `cancellationReason`. `RejectOrderRequest` dan `CancelOrderRequest` di backend membaca `reason`
(`PersonnelActionOrderDtos.cs:119-128`).

**Rekomendasi.** Kirim `reason` dari isian alasan `ConfirmModal`.

### ISS-HRD-002-T4 — Employee 360 memanggil alamat API yang salah, dan test mengunci alamat salah itu

| | |
| --- | --- |
| **Jenis** | `CONTRACT_MISMATCH` |
| **Area** | Frontend |
| **Keparahan** | Blocker |
| **Status bukti** | SUDAH-VERIFIKASI (laporan live testing LBT-WFC-04) |
| **Perbaikan** | `FIX-HRD-002-05` |
| **Kenapa dicantumkan** | Tanpa perbaikan ini tampilan Employee 360 yang dibangun ulang tidak dapat diverifikasi |

**Kenapa terjadi.** `employee-360-service.js` memanggil `/v1/corporate/human-resource/employee-360/{id}`.
Controller backend memakai
`/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/employee-360`.
`tests/unit/workforce-core-target-state.test.mjs:122-124` menuntut alamat yang salah.

**Rekomendasi.** Perbaiki service dan test-nya bersama.

### ISS-HRD-002-T5 — Alur Batch dan Pengajuan Atas Nama tidak cocok dengan kontrak backend

| | |
| --- | --- |
| **Jenis** | `CONTRACT_MISMATCH` |
| **Area** | Frontend (+ backend) |
| **Keparahan** | Blocker |
| **Status bukti** | SUDAH-VERIFIKASI (laporan live testing LBT-WFC-03, -05) |
| **Perbaikan** | `FIX-HRD-002-09` |
| **Kenapa dicantumkan** | Layar yang sama; perbaikan tampilan tidak membuat keduanya berfungsi |

**Kenapa terjadi.** Service Batch memanggil `/initial-placement-batches` dan aksi `/{id}/validate`
serta `/{id}/apply` yang tidak ada. Form Atas Nama mengirim JSON, padahal backend menuntut
`multipart/form-data`.

**Rekomendasi.** Tangani sebagai perbaikan terpisah setelah backend menutup celah validasi batch
(LBT-WFC-09, -10) dan kebuntuan draf atas nama (KI-05). Menyambungkan layar lebih dulu justru membuka
jalan bagi petugas untuk mengeksekusi batch tidak valid atau mengunci pengajuan mandiri pegawai.

### ISS-HRD-002-T6 — Pola yang sama di modul lain

| | |
| --- | --- |
| **Jenis** | `RULE_VIOLATION` |
| **Area** | Frontend lintas modul |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI (grep pada SHA frontend) |
| **Perbaikan** | Tidak dimasukkan ke rencana ini |
| **Kenapa dicantumkan** | Akar masalahnya sama dengan `ISS-HRD-002-01` |

Pencarian class khas Tailwind (`text-slate-*`, `border-slate-*`, `grid-cols-*`, `flex-col`,
`space-y-*`, `w-full`, `items-center`, `font-semibold`, dan sejenisnya) menemukan **198 berkas JSX**.
Yang terbanyak ada di modul Instrumen Klinis (7), Dashboard Kunjungan (5), Rawat Inap (5 + 4), dan
Pembelian Finance (5). Tidak semua pemakaian merusak tampilan, karena sebagian berdampingan dengan CSS
Module, tetapi setiap class itu tidak berfungsi. Pemilik modul masing-masing perlu memeriksanya
dengan grep yang ditambahkan pada `FIX-HRD-002-10`.

### ISS-HRD-002-T7 — Pilihan pegawai hanya memuat pegawai yang punya data karyawan

| | |
| --- | --- |
| **Jenis** | `CONTRACT_MISMATCH` (kemampuan backend belum ada) |
| **Area** | Backend + Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI (query baca-saja `QuilvianNewDevHamzah`, 03-11-2026) |
| **Perbaikan** | `FIX-HRD-002-11` |
| **Kenapa dicantumkan** | Ditemukan saat mengerjakan `FIX-HRD-002-03`: daftar pilihan pengganti isian UUID tidak mencakup seluruh profil |

**Apa yang terjadi.** Daftar pilihan "Pegawai" dan "Atasan Langsung" di form SK dan form Atas Nama hanya
memuat pegawai yang punya data karyawan: 27 dari 55 profil tenaga kerja. Ke-13 dokter (Dokter Tetap) dan
14 profil tenaga lain tanpa data karyawan tidak dapat dipilih, sehingga SK untuk seorang dokter belum
dapat diterbitkan dari layar.

**Kenapa terjadi.** Backend tidak punya endpoint opsi profil tenaga kerja. Pilihan memakai
`GET …/master-data/employees/options` yang mengirim `workforceProfileId` (`EmployeeOptionResponse`), tetapi
hanya untuk baris `MstEmployee`. Sebelum perbaikan, keterbatasan ini tersembunyi karena petugas harus
mengetik UUID.

**Rekomendasi.** Tambahkan endpoint opsi profil tenaga kerja di backend (karyawan dan dokter, bernilai
`workforceProfileId`), lalu ganti endpoint resource `workforceProfiles` di frontend. Tidak ada perubahan
lain di layar.

### ISS-HRD-002-T8 — Layar Exit Clearance memakai nilai prop yang tidak dikenal

| | |
| --- | --- |
| **Jenis** | `BUG` |
| **Area** | Frontend (Exit Clearance, `FE-HRD-017`) |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI (pemeriksa `check-css-usage.mjs`) |
| **Perbaikan** | Tidak dimasukkan ke rencana ini |
| **Kenapa dicantumkan** | Ditemukan oleh pemeriksa baru saat dicoba pada layar rujukan visual |

`exit-clearance-detail-view.jsx:162, 172, 205, 350` memakai `BaseButton variant="outline"` dan
`"danger-outline"` (jatuh ke `primary`), dan `:266, 381` mengirim `variant` ke `StatusBadge`. Tata letaknya
tetap dijadikan rujukan, tetapi nilai prop ini tidak ditiru. Pemilik layar Exit Clearance perlu
menjalankan pemeriksa yang sama.

## 7. Pertanyaan dan keputusan yang dibutuhkan

**Untuk pelapor:** Tidak ada. Butir 1 dan 2 jelas dari lampiran.

**Untuk pemilik:**

| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| K-01 | Perintah pemilik 03-11-2026 ("Perbaiki seluruh tampilan tersebut, sesuai standar UI, mengikuti base component") | — | **Sudah diambil** pada laporan; berlaku untuk `FIX-HRD-002-01` s.d. `-08` dan `-10` | — |
| K-02 | Kapan alur Batch dan Pengajuan Atas Nama disambungkan ke backend | Setelah backend diperbaiki / sekarang dengan risiko | Setelah backend menutup LBT-WFC-09, -10 dan KI-05 | `FIX-HRD-002-09` |

## 8. Catatan pola

- **Pola berulang di HR.** ISSUE-HRD-001 juga berawal dari layar yang "selesai" tanpa pernah dibuka
  di peramban: alamat API salah, tabel kosong, dan laporan task tetap berstatus ✅. Kedua issue
  menunjukkan pola yang sama. **Laporan task frontend dapat berstatus selesai tanpa bukti visual.**
- **Usulan pencegahan** (dikerjakan pada `FIX-HRD-002-10`):
  1. Aturan eksplisit: Tailwind tidak terpasang dan dilarang; `className` literal hanya boleh memakai
     class global yang terbukti ada.
  2. Grep anti-regresi baru untuk class Tailwind, dialog buatan sendiri, dan nilai prop base component
     yang tidak sah, ditambah pemeriksa kunci CSS Module yang tidak ada.
  3. Bukti visual (screenshot) wajib untuk setiap task yang menyentuh tampilan. Laporan tanpa tabel
     keputusan base component, baris `UI GATE`, dan hasil grep tidak boleh berstatus ✅.
  4. Test unit dilarang mengunci alamat API tanpa mencocokkannya dengan `[Route]` controller backend.

## 9. Riwayat dokumen

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-11-03 | Issue dibuat dari laporan pemilik (1 keluhan, 1 pertanyaan, 4 lampiran) dan 7 butir tampilan laporan live testing | `diagnose-module-issue` |
| 2026-11-03 | Status `DALAM_PERBAIKAN`: `FIX-HRD-002-01` s.d. `-08` dan `-10` dikerjakan atas perintah pemilik. Temuan tambahan `T7` (cakupan pilihan pegawai) dan `T8` (prop Exit Clearance) ditambahkan dari hasil pengerjaan | Pengerjaan perbaikan |
