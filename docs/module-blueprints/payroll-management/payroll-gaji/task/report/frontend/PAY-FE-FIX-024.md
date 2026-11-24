# PAY-FE-FIX-024 — Komponen Penggajian tanpa Komponen Dasar dan Tipe Komponen (frontend)

Tanggal: 23 November 2026. Repository: QuilvianSystemFrontendDev, branch `yasmina`, HEAD `ab5843be` (tidak berubah; working tree memuat pekerjaan sebelumnya yang tidak disentuh). Lanjutan PAY-FE-FIX-023.

`VISUAL CHECK: NOT FEASIBLE` dan `MANUAL TEST: NOT FEASIBLE` (`npm run dev`/`build` dilarang). Status tugas UI: paling tinggi sebagian.

## File
| File | Perubahan |
| --- | --- |
| `src/lib/constants/payroll/master-data/payroll-component/payroll-component-constants.jsx` | Hapus field `baseComponentId`; `baseComponentName`+`componentType` dari detail; kolom Tipe; filter/key `componentType`; `optionMap.baseComponents`; `PAYROLL_COMPONENT_TYPE_OPTIONS`; transformasi `baseComponentId`. Summary: kartu Pendapatan/Potongan/Kontribusi diganti Nonaktif/Berulang/Kena Pajak (field summary Backend sudah ada). Helper text: hanya kategori dan metode perhitungan |
| `src/lib/hooks/payroll/master-data/payroll-component/use-master-data-payroll-component-editor.jsx` | Tidak lagi memuat opsi Komponen Dasar (`getPayrollComponentOptions`, selector, `mapBaseComponentOptions`); `optionsLoading` hanya menunggu metadata dan kategori |
| `tests/unit/payroll-component-category-refactor.test.mjs` | +5 tes (total 16) |

Tidak diubah (dengan alasan): slice `getPayrollComponentOptions` dan endpoint options (dipakai editor/list Jenis Tunjangan, Jenis Potongan, Kebijakan Transportasi, dan Penghasilan Variabel); view, utils, hook list/detail (tidak mereferensikan field yang dihapus); route dan izin.

## Payload
Tambah: `{ payrollComponentCategoryId, payrollComponentName, calculationMethod, formulaExpression, defaultAmount, defaultPercentage, isRecurring, isTaxable, isProrated, isAttendanceBased, isOvertimeBased, isBenefitBased, isEmployerContribution, isEmployeeContribution, isDisplayedOnPayslip, isEditableDuringPayroll, effectiveStartDate, effectiveEndDate, description, sortOrder }`. Ubah: sama ditambah `isActive`. Tanpa `baseComponentId` dan `componentType` (diuji dengan membangun payload nyata dari utils). FE tidak mengisi `Earning` di mana pun (diuji).

## Matriks TC (statis)
| TC | Status | Bukti |
| --- | --- | --- |
| 01-02 | VERIFIED STATIC / NEEDS RUNTIME UAT | `createFields`/`updateFields` tanpa kedua field; tes |
| 03 | VERIFIED STATIC / NEEDS RUNTIME UAT | `detailFields` tanpa Komponen Dasar dan Tipe Komponen (Komponen Turunan tetap tampil sebagai hitungan legacy) |
| 04 | VERIFIED STATIC / NEEDS RUNTIME UAT | Tanpa kolom Tipe; `filterDefaults`/`supportedFilterKeys` tanpa `componentType`; metadata Backend tanpa opsi dan sortir tipe; summary tanpa kartu per tipe |
| 05 | VERIFIED STATIC (FE) / NEEDS RUNTIME UAT | Payload nyata diuji |
| 10 | VERIFIED STATIC | Paginasi, pencarian, sortir, status tidak diubah |
| 12 | VERIFIED STATIC | Modul Jenis Tunjangan/Potongan tetap mengimpor `getPayrollComponentOptions` (diuji) |
| 13 | VERIFIED STATIC | Tidak ada validasi yang mewajibkan kedua field; `validateFormExtra` hanya Formula dan tanggal |
| 15 | VERIFIED STATIC | Route, izin tidak disentuh |

## Validasi
- `AUTOMATED TEST`: 16 tes berkas ini lulus; rangkaian `payroll-*.test.mjs` dari root workspace: 507 tes, 504 lulus, 3 gagal lama (module-contract x2, run-screens x1), tidak dibandingkan dengan HEAD bersih.
- ESLint 3 berkas: 0 error, 1 peringatan lama. `check-css-usage.mjs`: 0 temuan.
- UI GATE: tidak ada elemen UI baru (hanya penghapusan field/kolom dan penggantian kartu summary memakai `SummaryCards` yang sama): REUSE.

## Ketergantungan
Backend PAY-BE-FIX-024 harus diterapkan bersama: tanpa itu metadata lama masih mengirim `componentTypeOptions` (diabaikan FE, tidak berbahaya) dan, bila backend lama dipakai, `ComponentType` wajib akan membuat tambah komponen gagal.
