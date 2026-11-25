# PAY-FE-FIX-023 — Kategori Komponen dan Komponen Penggajian (frontend)

Tanggal: 23 November 2026. Repository: QuilvianSystemFrontendDev, branch `yasmina`, HEAD `ab5843be` (working tree memuat pekerjaan sebelumnya yang tidak disentuh). Pasangan: PAY-BE-FIX-023 (backend harus dibuild lebih dulu; kontrak API berubah).

## Status

IMPLEMENTED (source): bagian 1. NOT VERIFIED: tampilan, interaksi, dan runtime. `VISUAL CHECK: NOT FEASIBLE` (`npm run dev` dan `npm run build` dilarang pada tugas ini; screenshot tidak dapat diambil). `MANUAL TEST: NOT FEASIBLE` dengan alasan yang sama. Status tugas UI: paling tinggi sebagian.

## 1. File

| File | Perubahan |
| --- | --- |
| `src/components/view/payroll/master-data/shared/payroll-master-data-table-cells.jsx` (baru) | `CreationInfoCell`, `ReadOnlyCheckCell` |
| `src/style/payroll/payroll-master-data-cells.module.css` (baru) | Tata letak sel; seluruh nilai token |
| `src/lib/constants/payroll/master-data/payroll-component-category/payroll-component-category-constants.jsx` | Kelompok dihapus dari form/filter/kolom/detail/sortir/summary/metadata; kolom: Kode, Nama, Gaji Bruto, Penghasilan Pajak, Take Home Pay (checklist), Informasi Pembuatan, Status; Nama = pilihan katalog Backend saat Tambah, teks saat Ubah; checkbox |
| `.../payroll-component/payroll-component-constants.jsx` | "Tipe Komponen" dihapus dari form; Tanggal Dibuat + Dibuat Oleh -> Informasi Pembuatan |
| `src/components/view/payroll/master-data/payroll-component-category/master-data-payroll-component-category-view.jsx` | Renderer `checkbox` dan `creationInfo` |
| `src/components/view/payroll/master-data/payroll-component/master-data-payroll-component-view.jsx` | Renderer `creationInfo` |
| `src/lib/hooks/payroll/master-data/payroll-component-category/use-master-data-payroll-component-category-editor.jsx` | Pilihan nama hanya kategori yang belum dibuat; nilai awal checkbox dari katalog Backend; nama dikunci bila `isNameLocked` |
| `src/lib/hooks/payroll/master-data/payroll-component/use-master-data-payroll-component-editor.jsx` | Mode ubah memuat kategori dengan `includeCategoryId` agar kategori legacy komponen tetap tampil |
| `src/utils/menu-sidebar/menu-items.jsx` | Subkelompok "Komponen Penggajian" dibongkar; Komponen Penggajian dan Kategori Komponen menjadi menu langsung |
| `tests/unit/payroll-component-category-refactor.test.mjs` (baru) | 11 tes |
| `tests/unit/payroll-module-contract.test.mjs` | Kontrak submenu Data Induk diperbarui sesuai hierarki baru |

## 2. Tabel keputusan base component (UI GATE)

| Elemen | Status | Sumber gaya / alasan |
| --- | --- | --- |
| Tabel, filter, hero, summary, badge, paginasi | REUSE | Tidak berubah |
| Checkbox form | REUSE | `BaseCheckboxField` (kartu centang) sudah dipakai; tipe diubah ke `checkbox` |
| Pilihan Nama Kategori | REUSE | `BaseSelectField` lewat `optionKey` |
| Informasi Pembuatan | COMPOSE | Teks memakai peran `dateCell` dan `supportingCell` dari `administrator-region-table.module.css`; wadah dua baris di CSS Module domain |
| Checklist read-only tabel | NEW (kecil) | Tidak ada base component atau pola sel yang dapat dipakai. Opsi: (1) sel domain berisi `<input type="checkbox">` read-only [**dipilih, direkomendasikan**: tanpa mengubah DataTable, risiko regresi nol, biaya kecil]; (2) memperluas `DataTable` dengan tipe kolom boolean [lebih reusable, tetapi mengubah komponen shared, regresi lintas modul]. Menunggu persetujuan pemilik bila ingin opsi 2 |

## 3. Perilaku

- Checklist: `checked` dari boolean Backend; `onClick` di-`preventDefault`, `tabIndex=-1`, `pointer-events: none`, `aria-label` "Gaji Bruto: dicentang". Tidak mengubah data. Tercentang berarti memengaruhi aspek itu, bukan nominal positif.
- Informasi Pembuatan: tanggal `CreateDateTime` (format `formatDateTime` yang sama dengan sebelumnya, zona waktu peramban), nama `CreateByName`; "-" bila kosong; tidak pernah menampilkan ID pengguna.
- Take Home Pay pada tabel = apakah kategori memengaruhi, bukan nominal Net Pay pegawai.
- Tidak ada kolom **Aksi** pada tabel saat ini (detail dibuka dengan klik dua kali). Kolom itu **tidak ditambahkan** karena tidak ada di source dan menambah perilaku baru (NEEDS CONFIRMATION).
- Sidebar: Jenis Tunjangan dan Jenis Potongan **tetap ada** sebagai menu langsung (tidak di subkelompok). Alasan: dipakai mesin Payroll dan atributnya belum punya pengganti (lihat PAY-BE-FIX-023 bagian 8-9). Menghapusnya = menghapus dua entri di `menu-items.jsx`.

## 4. Validasi

- `AUTOMATED TEST`: `node --import ./QuilvianSystemFrontendDev/tests/helpers/register.mjs --test QuilvianSystemFrontendDev/tests/unit/payroll-*.test.mjs` (dari root workspace) — 502 tes: 499 lulus, 3 gagal. Ketiganya gagal sebelum pekerjaan ini: `payroll-module-contract` (rute menu di luar `/payroll/`; komponen mentah di `workforce-config-status-header.jsx`) dan `payroll-run-screens` (menu Payroll Run). Tidak dibandingkan dengan HEAD bersih.
- ESLint pada 9 file: 0 error, 2 peringatan lama (`set-state-in-effect` pada efek existing).
- `check-css-usage.mjs` pada 3 JSX: `OK — 3 berkas diperiksa, 0 temuan.`
- `ui-consistency-checklist` grep anti-regresi: tidak ada class Tailwind, tidak ada warna literal pada CSS baru (diuji).

## 5. Risiko dan ketergantungan

- Backend PAY-BE-FIX-023 wajib dibuild dan diterapkan bersama frontend ini: tanpa itu dropdown Nama Kategori kosong (tidak ada `coreCategoryOptions`) dan Tambah Kategori ditolak.
- Dropdown Nama Kategori hanya menampilkan kategori yang belum ada; bila semuanya sudah ada, tidak ada pilihan (kategori ditambah ulang tidak diperlukan).
- Tidak ada commit, stage, atau push.
