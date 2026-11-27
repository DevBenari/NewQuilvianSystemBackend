# Laporan Perubahan Frontend — `PAY-FE-FIX-011`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-FE-FIX-011` (ad-hoc, bukan kartu roadmap; melanjutkan seri `PAY-FE-FIX-001..010`) |
| Judul | Form Penetapan Gaji: Grade Gaji → Struktur Gaji (bertingkat, filter server-side) → Gaji Dasar (turunan, hanya baca) untuk Karyawan, Dokter, dan External User |
| Roadmap | **NOT APPLICABLE** (brief pemilik 2026-11-20) |
| Dependency | `PAY-BE-FIX-011` (source selesai, **belum dibuild**). **Deploy Backend lebih dulu**: tanpa penentuan nominal di Backend, penyimpanan tanpa `baseSalary` ditolak |
| Task mode | `FRONTEND` (backend read-only sumber kontrak) |
| Branch / HEAD | `yasmina` / `31df72b9d907796eb106c638a2a350405c866efa` (cocok snapshot). Working tree sudah memuat pekerjaan lain (`PAY-FE-FIX-010`: kolom persetujuan Payroll dan lain-lain); file di bagian 7 adalah yang disentuh task ini |
| Model | Claude Sonnet 5.5 |
| Tanggal | 2026-11-20 |
| Status | 🟡 **Source selesai; ESLint 0 error (0 warning baru); tes terkait lulus; `VISUAL CHECK: NOT FEASIBLE`; `npm run build`/`dev` dilarang task.** |

## 1. Akar masalah

| Gejala | Penyebab | Lokasi |
| --- | --- | --- |
| Urutan Struktur Gaji lalu Grade Gaji; dropdown tidak bertingkat | Ketiga konfigurasi menaruh `salaryStructureId` sebelum `salaryGradeId` dan tanpa `dependsOn`/`filterBy`: daftar struktur memuat **semua** struktur | `employee/doctor/external-user-workforce-editor-config.jsx` bagian `salaryAssignments` |
| Arah pemilihan terbalik | `mapSalaryStructureSelection` **mengisi grade dari struktur** (`updates.salaryGradeId = raw.salaryGradeId`): anak mengubah induk | `workforce-editor-utils.jsx` |
| Nominal tertahan dari struktur sebelumnya | `if (raw.defaultBaseSalary && !currentForm?.baseSalary)`: Gaji Dasar hanya diisi bila kosong, jadi mengganti struktur mempertahankan nominal lama | `mapSalaryStructureSelection` |
| "Gaji Pokok" masih dapat diketik | `baseSalary` adalah `numberField` biasa, adapter editor memaksanya `type: "text"` dengan pemformat dan validator nominal, dan payload mengirim `normalizeSalaryAmount(...)` | `employee-workforce-editor-config.jsx`, `workforce-resource-editor.jsx` (`adaptWorkforceResourceConfig`, `normalizeSalaryAssignmentPayload`) |
| Opsi induk lama bisa tampil sementara | `EditorField` memasang `keepPreviousOptions: true` untuk semua select | `workforce-resource-editor.jsx` |

**Jalur pembukaan editor (ditelusuri, bukan diasumsikan).** `payroll-compensation-detail-view.jsx` dan `payroll-salary-assignment-view.jsx` **read-only**. Satu-satunya pemilik formulir "Ubah Penetapan Gaji" adalah `WorkforceResourceEditor` (`src/components/features/workforce/workforce-resource-editor.jsx`), dipakai hanya oleh `workforce-profile-dashboard.jsx` dan dikonfigurasi lewat tiga berkas `*-workforce-editor-config.jsx`. Karena satu editor melayani ketiganya, perilaku diterapkan di satu tempat dan konfigurasi dipakai bersama.

## 2. Keputusan komponen (UI GATE)

Tidak ada `NEW`, `WRAP`, atau komponen baru. Sumber gaya: (a) bawaan komponen; **tidak ada CSS baru** (hanya kelas yang sudah ada: `wfp-modern-help`, `wfp-modern-error`).

| Elemen | Status | Bukti |
| --- | --- | --- |
| Dropdown Grade/Struktur Gaji | `REUSE` | `ResourceFilterSelect` + `useSelectResource` (server-side, `search`, paginasi, `requestIdRef` anti respons kedaluwarsa) |
| Dependensi induk-anak | `REUSE` | `dependsOn`, `filterBy`, `clearOnChange` yang sudah ada di editor (dipakai Departemen → Posisi); **tidak ada dropdown bersarang baru** |
| Gaji Dasar hanya baca | `REUSE` | `readOnly` yang sudah didukung `EditorField` (`is-readonly`, `aria-readonly`) |
| Dua pengaya opsi field | `EXTEND` kecil, **aditif, bawaan tidak berubah** | `keepPreviousOptions: field.keepPreviousOptions ?? true` (bawaan tetap `true`) dan `field.dependencyPlaceholder` (hanya bila dipasang); tidak mengubah perilaku field lain |

## 3. Perubahan frontend

### 3.1 Susunan form (ketiga varian identik)

Baris 1: **Grade Gaji \*** (kiri) | **Struktur Gaji \*** (kanan). Baris 2: **Grade Karyawan** (kiri) | **Gaji Dasar \*** (kanan, hanya baca). Field lain tidak berpindah. Label diseragamkan di ketiga varian (sebelumnya Dokter dan External User berhuruf kecil). Label "Gaji Pokok" **hanya** berubah pada form ini; kolom tabel dashboard, layar Payroll, dan teks lain tidak diubah (tidak ada audit terminologi aplikasi, sesuai brief).

### 3.2 Dropdown bertingkat

- Sebelum grade dipilih: Struktur Gaji **disabled**, placeholder "Pilih Grade Gaji terlebih dahulu".
- Grade dipilih: field aktif, memuat `…/salary-structures/options?salaryGradeId=<grade>&onlyActive=true&search=&pageNumber=&pageSize=25` (filter di server; **tidak ada** penyaringan di Frontend). Memuat: "Memuat Struktur Gaji…". Kosong: "Tidak ada Struktur Gaji aktif untuk Grade Gaji ini."
- **Grade berganti** (`clearOnChange` + `mapSalaryGradeSelection` dengan form sebelumnya): struktur, labelnya, batas min/maks struktur, mata uang struktur, nominal master, **Gaji Dasar**, galat grade/struktur/Gaji Dasar/mata uang dikosongkan; struktur dimuat ulang dari grade baru. Memilih ulang grade yang **sama** tidak mengosongkan apa pun.
- **Respons kedaluwarsa**: `useSelectResource` membuang respons yang `requestId`-nya bukan yang terbaru; opsi grade lama dikosongkan seketika (`keepPreviousOptions: false`); detail rentang grade sudah memakai `AbortController` + `gradeFetchRequestIdRef` + pemeriksaan `prev.salaryGradeId`. Pilihan struktur dari daftar yang ternyata milik grade lain **ditolak** dan tidak pernah mengubah grade (`mapSalaryStructureSelection`).
- **Grade Karyawan** tetap atribut terpisah: tidak punya `dependsOn`/`onSelectMap`, tidak disamakan dengan Grade Gaji (diuji).

### 3.3 Gaji Dasar

- Diisi dari `defaultBaseSalary` struktur yang **baru dipilih** pada opsi resmi (bukan hanya bila kosong); mengganti struktur mengganti nominal. Mata uang form mengikuti mata uang struktur (Backend menolak yang berbeda).
- Hanya baca, tidak dapat diketik (input `readOnly` dan memakai gaya `:disabled` yang sudah ada, karena `is-readonly` tidak punya gaya); tidak lagi dikirim di payload (`baseSalary` dibuang dari `normalizeSalaryAssignmentPayload`; field dipaksa `readOnly/derived/omitFromPayload` di adapter sehingga varian yang lupa memasang opsi tetap aman). Validasi generik "wajib diisi" dilewati untuk field turunan.
- `DefaultBaseSalary` kosong/0/tidak valid: nominal **tidak** diminta manual; pesan "Gaji Dasar belum dikonfigurasi pada Struktur Gaji. Lengkapi master Struktur Gaji terlebih dahulu." tampil seketika dan tombol **Simpan dinonaktifkan** (`structureDefaultMissing`).
- Rentang gaji (grade ∩ struktur) tetap divalidasi di FE hanya bila nominal terlihat; kesalahan dasar ada di Backend.

### 3.4 Mode Ubah

- Grade dan struktur tersimpan tetap terpilih; tidak ada yang dikosongkan hanya karena opsi belum dimuat.
- **Nominal tersimpan ≠ nilai master**: Gaji Dasar menampilkan **nominal tersimpan** (histori). Bila `baseSalaryDiffersFromStructureDefault`: "Nominal tersimpan pada penetapan ini dipertahankan. Gaji Dasar master Struktur Gaji saat ini Rp …. Untuk memakai nominal master, tutup masa berlaku penetapan ini lalu buat penetapan baru."
- Memilih ulang **struktur yang sama** mengembalikan nominal tersimpan (sama dengan Backend); memilih struktur **lain** memakai nominal master struktur itu.
- Struktur tersimpan yang tidak lagi sah (`masterValidityStatus` ≠ `Valid`): catatan di bawah field Struktur Gaji berisi alasan Backend (mis. "sudah berakhir …") dan "tidak dapat dipilih untuk penetapan baru"; **tidak ada struktur pengganti yang dipilih diam-diam**.
- Mengubah keterangan saja tidak mengirim nominal dan Backend mempertahankan nominal tersimpan.

### 3.5 `ViewAmount`

Nominal Gaji Dasar ditampilkan hanya bila `WfpSalaryAssignment : ViewAmount` (`usePermission`, gagal-tertutup selama izin belum dimuat) **dan** Backend tidak mengirim `amountsVisible=false`. Selain itu field menampilkan **"Terselubung (memerlukan wewenang ViewAmount)"** (bukan 0 dan bukan kosong), rentang gaji disembunyikan, dan validasi nominal dilewati. Berbeda dari sebelumnya, pengguna tanpa `ViewAmount` **tidak lagi wajib mengetik nominal baru** saat mengubah penetapan. Catatan: opsi struktur dari endpoint master tetap mengirim nominal (lihat `PAY-BE-FIX-011` bagian 5).

## 4. Sebelum dan sesudah

| Aspek | Sebelum | Sesudah |
| --- | --- | --- |
| Baris 1 | Struktur Gaji \| Grade Gaji | **Grade Gaji \| Struktur Gaji** |
| Baris 2 | Grade Karyawan \| Gaji Pokok (isian) | Grade Karyawan \| **Gaji Dasar (hanya baca)** |
| Struktur Gaji | Semua struktur; memilih struktur mengubah grade | Hanya struktur grade terpilih (server-side); disabled sebelum grade dipilih; struktur tidak mengubah grade |
| Ganti grade | Struktur dan nominal lama tertinggal | Struktur, nominal, batas, dan galat dikosongkan |
| Sumber nominal | Diketik pengguna (struktur hanya mengisi bila kosong) | `DefaultBaseSalary` struktur terpilih; **Backend yang menentukan** |
| Payload | `baseSalary` dikirim | `baseSalary` tidak dikirim |
| Tanpa `ViewAmount` | Wajib mengetik nominal baru | Nominal terselubung; tidak perlu mengetik |

## 5. Kontrak yang dikonsumsi

Tidak ada endpoint baru. Memakai `GET …/master-data/salary-structures/options` (`salaryGradeId`, `onlyActive`, `search`, `pageNumber`, `pageSize`; respons `id`, `salaryGradeId`, `salaryStructureName`, `currencyCode`, `paymentFrequency`, `defaultBaseSalary`, `minimumBaseSalary`, `maximumBaseSalary`) dan `GET …/master-data/salary-grades/options`, serta field aditif respons penetapan dari `PAY-BE-FIX-011` (`structureDefaultBaseSalary`, `baseSalaryDiffersFromStructureDefault`, `masterValidityStatus`, `masterValidityMessage`; bila Backend lama, semuanya bernilai bawaan dan form tetap berjalan).

## 6. Validasi

| Pemeriksaan | Hasil |
| --- | --- |
| ESLint (5 berkas: editor, utils, 3 konfigurasi) | **0 error, 10 warning**; seluruhnya di `workforce-resource-editor.jsx` dan **identik dengan HEAD (10)** (`set-state-in-effect`, `exhaustive-deps` lama). Tidak ada warning baru |
| `check-css-usage.mjs` pada `workforce-resource-editor.jsx` | 5 temuan, **semuanya sudah ada sebelumnya** (`is-checkbox-field`, `is-placeholder`, `is-readonly`, `wfp-modern-date-picker`, `UUID-DIKETIK` pada form Pengajuan Perubahan Data); edit ini tidak menambah kelas baru → `UNRELATED EXISTING ISSUE`. Target "0 temuan" tidak tercapai untuk berkas legacy itu |
| `workforce-salary-assignment.test.mjs` (dijalankan dari root workspace; 6 tes baru + 4 diperbarui) | **30/30 lulus** |
| Berkas tes terkait lainnya (`payroll-allowance-policy-grade-sync`, `payroll-salary-structure-allowance`, `workforce-payroll-data`, `doctor-workforce-tax-tab`, `payroll-compensation-profile`) | 11 + 29 + 13 + 7 + 26 lulus, 0 gagal |
| Gagal yang **bukan** akibat task ini | `payroll-module-contract` 2 gagal (rute di bawah `/payroll`; `workforce-config-status-header.jsx` komponen mentah); `workforce-core-cross-employee` 3 gagal bila dijalankan dari folder FE (ENOENT: tiga berkas view/service yang memang tidak ada). Keduanya tidak menyentuh berkas task ini → `UNRELATED EXISTING ISSUE` |
| `npm run build`, `npm run dev`, server dev | **NOT RUN** (dilarang task) |
| `VISUAL CHECK` / `MANUAL TEST` | **NOT FEASIBLE** (server dev dilarang): tampilan dua baris, nonaktif/aktif Struktur Gaji, pemuatan ulang saat grade berganti, dan balapan respons **belum diamati di browser** |

Tes baru mengunci: susunan dan label ketiga varian; `dependsOn`/`filterBy`/`clearOnChange`; reset saat grade berganti dan **tidak** reset saat grade sama; Gaji Dasar diganti saat struktur diganti; penolakan struktur milik grade lain; default 0/null/tidak valid memblokir; mode ubah (nominal tersimpan vs master, struktur tidak sah, terselubung); payload tanpa `baseSalary`; simpan diblokir; `ViewAmount`. Tes memeriksa **fungsi murni dan teks source**, bukan perilaku di browser.

### Peta UAT (statis)

| UAT | Cakupan source | Status |
| --- | --- | --- |
| 01 susunan | Konfigurasi 3 varian | Diuji (teks/objek konfigurasi); **tampilan NOT RUN** |
| 02 struktur disabled | `dependsOn` + `dependencyPlaceholder` | Diuji (konfigurasi); NOT RUN |
| 03 struktur sesuai grade | `filterBy` → `salaryGradeId` server-side | Diuji (konfigurasi); NOT RUN |
| 04 ganti grade | `mapSalaryGradeSelection` | Diuji (fungsi murni); NOT RUN |
| 05 nominal otomatis | `mapSalaryStructureSelection` | Diuji (fungsi murni); NOT RUN |
| 06 tidak bisa diketik | `readOnly` + adapter | Diuji (teks source); NOT RUN |
| 07 manipulasi request | Sisi Backend (`PAY-BE-FIX-011`) | NOT RUN |
| 09 default kosong | Pesan + simpan diblokir | Diuji (fungsi murni/teks source); NOT RUN |
| 10 paginasi + pencarian | Hook yang sudah ada (`pageSize 25`, `search`, muat lagi) | Tidak diubah; NOT RUN |
| 11 respons terlambat | `requestIdRef`, `keepPreviousOptions:false`, penolakan struktur milik grade lain | Sebagian diuji; **balapan nyata NOT RUN** |
| 12 mode ubah | `buildSalaryAssignmentInitialState`, struktur/nominal tersimpan | Diuji (fungsi murni); NOT RUN |
| 17 konsistensi tiga varian | Satu editor + konfigurasi bersama | Diuji |
| 18 `ViewAmount` | `usePermission` + `amountsVisible` + teks terselubung | Diuji (teks source); NOT RUN |
| 20 simpan berulang | Tidak ada perubahan pada hook simpan | Tidak berubah; NOT RUN |

## 7. Daftar berkas (repository frontend, branch `yasmina`)

| Berkas | Perubahan |
| --- | --- |
| `src/components/features/workforce/workforce-resource-editor.jsx` | Gaji Dasar `readOnly/derived/omitFromPayload`; payload tanpa `baseSalary`; fungsi `normalizeSalaryAmount` yang tak terpakai dihapus; `validateForm` melewati field turunan; `validateSalaryAssignmentForm` tanpa validasi isian nominal; `onSelectMap` menerima form sebelumnya; `keepPreviousOptions`/`dependencyPlaceholder` per field; teks bantuan, topeng `ViewAmount`, simpan diblokir |
| `src/utils/hr/master-data/workforce/workforce-editor-utils.jsx` | Pemetaan grade (induk) dan struktur (anak) ditulis ulang; `buildSalaryAssignmentInitialState`, `isOriginalSalaryStructure`, `getSalaryBaseHelp`, `getSalaryStructureNotice`, konstanta bersama |
| `src/lib/constants/hr/master-data/employee/employee-workforce-editor-config.jsx` | Urutan, label, `dependsOn/filterBy/clearOnChange`, Gaji Dasar turunan; pembatas/normalizer nominal yang tak terpakai dihapus |
| `src/lib/constants/hr/master-data/doctor/doctor-workforce-editor-config.jsx` | Sama |
| `src/lib/constants/hr/master-data/external-user/external-user-workforce-editor-config.jsx` | Sama |
| `tests/unit/workforce-salary-assignment.test.mjs` | 4 asersi diperbarui (arah pilihan, panggilan validasi BE, teks bantuan), 6 tes baru |

## 8. Keputusan terbuka dan risiko

1. **Urutan deploy**: Backend dulu (bagian Metadata).
2. **`ViewAmount` pada opsi struktur** dan filter tanggal pada endpoint options belum diputuskan (`PAY-BE-FIX-011` bagian 6); struktur yang berakhir hanya dijelaskan pada penetapan lama, sedangkan untuk transaksi baru Backend yang menolak (dropdown tidak menyaring menurut tanggal).
3. Pengguna tanpa `ViewAmount` tidak melihat Gaji Dasar yang akan tersimpan; mereka tetap dapat menyimpan (Backend yang menentukan).
4. Mata uang kini mengikuti struktur saat struktur dipilih (menimpa pilihan mata uang sebelumnya) supaya selaras dengan validasi Backend; ini penyesuaian di luar daftar brief.
5. Tombol "Setujui penetapan gaji" di UI HRD tetap tidak ada (prasyarat `RequireApprovedSalaryAssignment`, tidak berubah).
6. Tidak ada stage, commit, push, pull, merge, atau deploy.
