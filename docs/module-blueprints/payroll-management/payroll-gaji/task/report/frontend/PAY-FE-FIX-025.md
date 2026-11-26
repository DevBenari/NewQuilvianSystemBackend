# PAY-FE-FIX-025 — Form Komponen Penggajian digerakkan Metode Perhitungan (frontend)

Tanggal: 24 November 2026. Repository: QuilvianSystemFrontendDev, branch `yasmina`, HEAD `b05275ec` (diverifikasi; working tree bersih sebelum pekerjaan ini). Pasangan: PAY-BE-FIX-025 (terapkan bersamaan).

`VISUAL CHECK: NOT FEASIBLE` dan `MANUAL TEST: NOT FEASIBLE` (`npm run dev`/`build` dilarang). Status tugas UI: paling tinggi sebagian.

## File
| File | Perubahan |
| --- | --- |
| `src/utils/payroll/master-data/payroll-component/payroll-component-default-value-rules.js` (baru) | Aturan murni: field aktif per metode, reset saat ganti metode, netralisasi payload (angka), deteksi konflik data lama |
| `src/lib/constants/payroll/master-data/payroll-component/payroll-component-constants.jsx` | `transformFormChange` (reset), `validateFormExtra` sadar-metode, `transformPayload` memakai `applyMethodToPayload`; deskripsi field; "Komponen Turunan" dihapus dari detail |
| `src/utils/payroll/master-data/payroll-component/payroll-component-utils.jsx` | `buildDetailRows` hanya nilai relevan + baris "Pemeriksaan Data"; `buildLegacyWarning` |
| `src/lib/hooks/payroll/master-data/payroll-component/use-master-data-payroll-component-editor.jsx` | `fields` menyembunyikan field nilai lewat `hidden`; ganti metode membersihkan error field nilai saja; `legacyWarning` |
| `.../add/payroll-component-form-view.jsx` | `InformationAlert` (warning) lewat slot `afterHero` |
| `tests/unit/payroll-component-default-values.test.mjs` (baru) | 14 tes |

`BaseEditorForm`/`BaseEditorField` tidak diubah (`field.hidden` sudah didukung). Struktur Gaji dan modul lain tidak disentuh.

## Perilaku
- Fixed: Nominal Default. Percentage: Persentase Default. Formula: Formula Perhitungan. Metode lain: tanpa input nilai (nilai lama yang tersimpan dibawa apa adanya selama metode tidak diganti).
- Ganti metode mereset nilai yang tidak relevan (contoh 1-3 pada prompt diuji) dan membersihkan hanya error field nilai.
- Payload hanya membawa nilai sesuai metode, dikirim sebagai **angka** (sebelumnya string "500000"/"0"), dibulatkan 2 desimal (nominal) dan 4 desimal (persentase) sesuai kolom.
- Field tersembunyi tidak pernah memblokir penyimpanan (errornya dibuang).
- Update memuat metode tersimpan; data legacy konflik memunculkan peringatan kuning dan **tidak diubah otomatis**: nilai yang tidak sesuai metode baru terbuang saat pengguna menyimpan (tindakan eksplisit setelah peringatan).
- Detail: hanya nilai default relevan; nilai 0 dari parameter yang tidak dipakai disembunyikan; konflik legacy ditandai "Perlu pemeriksaan"; nilai lama pada metode lain tetap terlihat bila > 0.
- Tabel daftar tidak berubah.

## UI GATE
Semua REUSE: `BaseEditorView.afterHero`, `InformationAlert variant="warning"`, `field.hidden`, normalizer `currencyIdr`/`percentage`. Tanpa CSS baru.

## Validasi
`AUTOMATED TEST`: 14 tes berkas ini lulus; `payroll-*.test.mjs` dari root workspace: 521 tes, 518 lulus, 3 gagal lama (module-contract x2, run-screens x1; tidak dibandingkan dengan HEAD bersih). ESLint: 0 error, 1 peringatan lama. `check-css-usage.mjs`: 0 temuan.

## UAT (rencana; status statis)
| UAT | Status |
| --- | --- |
| 01-03 (tanpa Komponen Dasar/Tipe; tipe diturunkan BE) | VERIFIED STATIC (sudah dari FIX-023/024), NEEDS RUNTIME UAT |
| 04 (tanpa field Struktur Gaji) | VERIFIED STATIC (tes) |
| 05-07 (Fixed/Percentage/Formula) | VERIFIED STATIC (hook + `field.hidden`), NEEDS RUNTIME UAT |
| 08-10 (reset, tanpa nilai basi) | VERIFIED STATIC (tes perilaku) |
| 11-13 (API menolak) | VERIFIED STATIC (BE), NEEDS RUNTIME UAT |
| 14 (Update sesuai metode) | VERIFIED STATIC |
| 15 (legacy tidak dikoreksi diam-diam) | VERIFIED STATIC: peringatan + backend menolak simpan konflik |
| 16-18, 20 (Jenis Tunjangan/Potongan, variabel, tanpa ganda) | VERIFIED STATIC: tidak ada pembaca nilai default komponen |
| 19 (Payroll final tidak berubah) | VERIFIED STATIC: tidak ada penulisan ke tabel payroll |
| 21 (DefaultBaseSalary utuh) | VERIFIED STATIC (tes) |
| 22 (izin) | VERIFIED STATIC: atribut tidak diubah; RBAC runtime NEEDS UAT |
| 23-24 (loading/error/toast; field tersembunyi tidak menggagalkan) | VERIFIED STATIC, NEEDS RUNTIME UAT |
| 25 (metode lain) | VERIFIED STATIC dengan keputusan terbuka |
| 26 (list, detail, create, update, payload konsisten) | VERIFIED STATIC, NEEDS RUNTIME UAT |
