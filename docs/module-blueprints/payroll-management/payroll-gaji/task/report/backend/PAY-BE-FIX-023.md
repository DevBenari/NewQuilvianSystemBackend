# PAY-BE-FIX-023 — Kategori Komponen sebagai pusat klasifikasi (backend)

Tanggal: 23 November 2026. Repository: NewQuilvianSystemBackend, branch `Yasmina`, HEAD `ab340bc7` (dikonfirmasi; working tree memuat pekerjaan sebelumnya yang tidak disentuh). Pasangan: PAY-FE-FIX-023.

## 0. Status ringkas

| Kategori | Isi |
| --- | --- |
| IMPLEMENTED (source, belum dibuild) | Katalog lima kategori; `ComponentGroup` diturunkan backend dan tidak lagi dikirim pengguna; validasi `ComponentType` selaras kategori; opsi kategori hanya operasional; summary dan metadata tanpa pengelompokan lama; nama kategori inti dikunci |
| PROPOSED | Pemetaan akhir Jenis Tunjangan/Potongan ke Komponen Penggajian; kunci kategori permanen; keputusan nilai awal checkbox dampak |
| NOT VERIFIED | `dotnet build`; perilaku runtime; isi tabel kategori di database pengguna; seluruh referensi komponen ke data payroll di luar Jenis Tunjangan/Potongan |
| BLOCKED_BY_SCHEMA | Lihat bagian 9 |
| NEEDS CONFIRMATION | Lihat bagian 10 |

`AUTOMATED TEST: NOT APPLICABLE` untuk backend (TEST_POLICY). Tes teks sisi frontend yang membaca source backend: 11 lulus (`payroll-component-category-refactor.test.mjs`). `dotnet build`, migration, database, dan runtime: NOT RUN.

## 1. Backend Governance Preflight

Area Corporate / HumanResource / MasterData / PayrollAndBenefit (controller berrute `payroll-management/master-data/...`). Prefix entity `Mst` (existing, tidak ada entity baru). Keberlakuan: TOUCHED LEGACY untuk dua controller dan DTO; NEW CODE untuk `PayrollComponentCategoryCatalog` (helper statis tanpa persistensi). QBE yang berlaku: QBE-API-001, QBE-PERM-001 (atribut Access tidak berubah), QBE-VAL-001, QBE-DTO-001, QBE-CFG-002 (konfigurasi tidak diubah). QBE-SVC-001 tidak diperluas: controller lama tetap mengakses context langsung (temuan legacy, tidak dipindah dalam tugas ini). Tidak ada migration, DDL, snapshot, atau perubahan data.

## 2. Diagnosis

- `MstPayrollComponentCategory` memuat `ComponentGroup` wajib dan tiga flag dampak. `ComponentGroup` dan tiga flag itu **hanya** dibaca controller, model, konfigurasi, dan DTO kategori. `PayrollCalculationService`, `PayrollVariableInputService`, dan `PayrollReportService` tidak membacanya (diuji di tes). Mengubah tampilan atau nilai checkbox tidak mengubah Gross, Taxable, atau Net.
- Consumer yang bergantung pada `MstPayrollComponent.ComponentType` (bukan kategori): `PayrollCalculationService`, `PayrollVariableInputService` (wajib Earning), `PayrollMedicalFeeIntakeService` (Earning), `PayrollReportService`, `FinanceReceivablePayrollSyncService` (wajib Deduction), `LeavePayrollIntegrationService`, `AllowanceTypeController` (Earning/EmployerContribution), `DeductionTypeController` (Deduction). Karena `ComponentType` baru = `ComponentGroup` kategori, nilainya tetap Earning atau Deduction untuk komponen baru.
- **Tabel kategori tidak punya kunci stabil.** Kode `PCC-RSMMC-nnnnn` berurutan dan nama adalah teks bebas. Pemetaan dilakukan lewat nama kanonik yang dinormalkan, dengan penguncian nama (lihat bagian 4).
- Jenis Tunjangan (`MstAllowanceType`) dipakai: `PayrollSalaryStructureAllowanceService`, `PayrollWorkforceAllowanceService` (+ `.Policy`), kebijakan Shift/Siaga/Risiko, `PrlAllowancePolicy`, `MstSalaryStructureAllowance`, `PrlWorkforceAllowance`; membawa `AllowanceCategory`, `MaximumAmount`, `RequiresApproval`, dll. yang tidak ada di komponen.
- Jenis Potongan (`MstDeductionType`) dipakai `PayrollInstallmentDeductionService` (baris 115-194): batas potongan cicilan dari `MaxNetSalaryPercentage` dan `MaximumAmount`; kolom itu tidak ada di komponen.

## 3. Perubahan (file)

| File | Perubahan |
| --- | --- |
| `Services/PayrollComponentCategoryCatalog.cs` (baru) | Lima entri: Gaji Pokok, Tunjangan, Insentif, Bonus -> Earning; Potongan -> Deduction. `FindByName`, `NamesMatch`, `IsOperational` |
| `Controllers/PayrollComponentCategoryController.cs` | Create: nama harus katalog, kelompok dari katalog, duplikat ditolak. Update: nama inti dikunci, kategori lama tidak boleh diganti nama menjadi nama katalog, `ComponentGroup` tidak pernah ditimpa. Options: `operationalOnly` (bawaan true) dan `includeCategoryId`. Metadata: `CoreCategoryOptions` (Missing/Ready/Inactive/GroupMismatch). Summary dari data nyata. Filter/sortir/pencarian kelompok dihapus |
| `DTOs/PayrollComponentCategoryDtos.cs` | `ComponentGroup` pada request jadi opsional (ditolak bila bertentangan). Respons: `CategoryKey`, `IsNameLocked`, `IsOperational`. Summary: `CoreCategory`, `LegacyCategory`, `MissingCoreCategory` (menggantikan hitungan per kelompok) |
| `Controllers/PayrollComponentController.cs` | `ComponentType` diturunkan dari kategori; kategori non-operasional hanya boleh bila tidak diganti; kombinasi legacy tidak selaras dipertahankan; guard reklasifikasi; keselarasan tipe komponen dasar |
| `DTOs/PayrollComponentDtos.cs` | `ComponentType` pada request opsional |

## 4. Mapping lima kategori dan identitas

| Kunci | Nama kanonik | Kelompok teknis | Nilai awal dampak (Bruto/Pajak/THP) |
| --- | --- | --- | --- |
| BASIC_SALARY | Gaji Pokok | Earning | ya/ya/ya |
| ALLOWANCE | Tunjangan | Earning | ya/ya/ya |
| INCENTIVE | Insentif | Earning | ya/ya/ya |
| DEDUCTION | Potongan | Deduction | tidak/tidak/ya |
| BONUS | Bonus | Earning | ya/ya/ya |

Nilai awal dampak hanya default formulir dan dapat diubah; **belum divalidasi bisnis** (NEEDS CONFIRMATION). Kategori operasional = nama sama dengan katalog **dan** kelompok tersimpan sama dengan katalog. Bernama katalog tetapi berkelompok lain (`GroupMismatch`) dilaporkan, tidak diubah, tidak ditawarkan. Kategori lain (legacy, termasuk EmployerContribution/Information) tetap dibaca dan tetap dipakai komponen lamanya.

## 5. Penghapusan Kelompok

Tidak ada lagi input, filter, sortir, kolom, atau pencarian kelompok. Kolom database dan properti respons dipertahankan (kompatibilitas). Pada Create, kelompok = kelompok katalog. Pada Update, kelompok tersimpan tidak disentuh. Klien lama yang mengirim `ComponentGroup` ditolak hanya bila nilainya bertentangan.

## 6. Perubahan kontrak API

- `POST` dan `PUT` kategori: `componentGroup` opsional; `POST` hanya menerima lima nama katalog (**perubahan perilaku**: kategori bebas tidak lagi dapat dibuat).
- `GET .../options`: parameter `componentGroup` dihapus; ditambah `operationalOnly=true` dan `includeCategoryId`. Bawaan sekarang hanya lima kategori operasional.
- `GET .../summary`: hitungan per kelompok dihapus; ditambah `coreCategory`, `legacyCategory`, `missingCoreCategory`.
- `GET .../filters/metadata`: `componentGroupOptions` dihapus; ditambah `coreCategoryOptions`.
- `POST`/`PUT` komponen: `componentType` opsional; bila terisi dan bertentangan dengan hasil turunan -> 400.
- Hak akses: tidak berubah.

## 7. Dampak Payroll

Tidak ada file mesin payroll yang diubah. Dampak hanya pada komponen **baru/diubah kategorinya**: tipe selalu mengikuti kategori. Komponen lama tidak disentuh. Penjaga tambahan hanya menolak (tidak mengubah data): pergantian kategori bertipe berbeda ditolak bila komponen dipakai Jenis Tunjangan/Potongan atau punya turunan bertipe lain.

## 8. Strategi penyatuan Jenis Tunjangan dan Potongan

Tidak dihapus dan tidak dipindah. Keduanya tetap master tersendiri karena atributnya dipakai payroll dan belum ada tempat di komponen. Kategori Komponen menjadi pusat klasifikasi untuk komponen baru; Jenis Tunjangan/Potongan tetap menaut ke komponen lewat `PayrollComponentId` (relasi dan FK tidak diubah).

## 9. BLOCKED_BY_SCHEMA

1. **Kunci kategori permanen** (kolom mis. `CategoryKey` bernilai unik): identitas saat ini = nama kanonik dengan nama terkunci.
2. **Penyatuan penuh Jenis Potongan**: `MaxNetSalaryPercentage`, `MaximumAmount`, `Priority`, `AllowPartialDeduction`, `IsStatutory`, `IsPreTax` belum punya padanan di komponen; `PayrollInstallmentDeductionService` bergantung padanya.
3. **Penyatuan penuh Jenis Tunjangan**: `AllowanceCategory`, `MaximumAmount`, `RequiresApproval`, aturan kelayakan belum punya padanan di komponen.
4. Kolom `AffectsTakeHomePay` pada komponen: tidak dibuat (kategori sudah membawanya).

## 10. NEEDS CONFIRMATION

1. Menu Jenis Tunjangan dan Jenis Potongan **tidak dihapus** (bagian 8); pemilik memutuskan apakah atribut tersebut dipindahkan (butuh skema) sebelum menu dicabut.
2. Nilai awal dampak (bagian 4), terutama Potongan: Pajak = tidak.
3. Komponen dasar harus bertipe sama dengan turunannya (mencegah klasifikasi bertentangan). Bila potongan berbasis persentase atas komponen penghasilan memang sah, aturan ini perlu dilonggarkan. `BaseComponentId` tidak dibaca mesin payroll.
4. Kategori EmployerContribution/Information tidak lagi dapat dibuat atau dipilih untuk komponen baru. Komponen kontribusi pemberi kerja baru perlu keputusan kategori.
5. Kategori legacy yang bernama katalog tetapi berkelompok salah (jalankan audit [1]).

## 11. Tinjauan statis 45 skenario

Status: **P** = Static Review Passed, **N** = Not Verified, **B** = BLOCKED_BY_SCHEMA / tidak dipenuhi karena diblokir.

| No | Skenario | Status | Bukti |
| --- | --- | --- | --- |
| 01-03 | Menu langsung, tanpa subkelompok, Kategori dapat diakses | P | `menu-items.jsx`; tes `payroll-component-category-refactor` |
| 04-05 | Menu Jenis Tunjangan/Potongan tidak tampil | B | Menu dipertahankan karena consumer (bagian 2, 8) |
| 06-07 | Rute dan RBAC menu | P | Rute dan `requiredPermission` tidak berubah |
| 08-09 | Form tanpa Kelompok; backend tanpa input kelompok | P | Konstanta, DTO, controller |
| 10-14 | Kelima kategori tersedia | N | Bergantung isi database; audit [1]; formulir Tambah membuatnya |
| 15 | Tanpa kategori duplikat | P (kode) / N (data) | `NameExistsAsync`; audit [2] |
| 16 | Kategori legacy aman | P | Tidak ada konversi; dibaca apa adanya |
| 17-19 | Tipe selaras kategori; create/update komponen | P | `ValidateRequestAsync` |
| 20-26 | Checkbox tabel/form | P | Konstanta, sel, tes |
| 27-29 | Informasi Pembuatan satu kolom | P | `creationInfo`; tes |
| 30-31 | Sortir, pencarian, paginasi | P | Data diurut sumber; paginasi tidak diubah |
| 32 | Summary akurat | P | Dihitung dari baris tersimpan |
| 33 | Komponen legacy terbaca | P | Respons tidak berubah |
| 34-36 | Tunjangan, Potongan, Penghasilan Variabel | P | Consumer tidak disentuh; tipe Earning dipertahankan |
| 37-40 | Perhitungan, PPh, THP aktual, Payroll Run final | P | Mesin tidak membaca flag/kelompok kategori; tidak ada penulisan data |
| 41 | Tanpa broken import FE | P | ESLint 0 error; 502 tes payroll: 499 lulus (3 gagal lama) |
| 42 | Tanpa dependency orphan BE | N | Tidak dikompilasi |
| 43 | Hak akses aman | P | Atribut Access tidak diubah |
| 44-45 | Tanpa perubahan skema/migration | P | Tidak ada migration/DDL |

## 12. Lampiran dan langkah pemilik

- `sql/PAY-BE-FIX-023-audit-kategori-komponen.sql` (baca-saja): status lima kategori, duplikat, kategori legacy, komponen dengan tipe tidak selaras, turunan bertipe beda.
- Urutan: jalankan audit -> `dotnet build` -> buka Kategori Komponen > Tambah dan buat kategori yang berstatus Missing -> uji formulir Komponen.
- Tidak ada stage, commit, push.
