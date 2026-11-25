# PAY-BE-FIX-025 — Nominal Default dan Persentase Default saling eksklusif (backend)

Tanggal: 24 November 2026. Repository: NewQuilvianSystemBackend, branch `Yasmina`, HEAD `261bb431` (diverifikasi; working tree bersih sebelum pekerjaan ini). Pasangan: PAY-FE-FIX-025. Lanjutan PAY-BE-FIX-023/024 (sudah ada di HEAD).

`AUTOMATED TEST: NOT APPLICABLE` (backend). Tes teks/perilaku di sisi frontend yang membaca source backend: 14 lulus (`payroll-component-default-values.test.mjs`). `dotnet build`, migration, database, runtime: NOT RUN.

## Preflight
Area Corporate / HumanResource / MasterData / PayrollAndBenefit; prefix `Mst` (existing); TOUCHED LEGACY. Tanpa entity/DDL/snapshot baru. QBE: QBE-API-001, QBE-PERM-001 (atribut Access tidak diubah), QBE-VAL-001, QBE-DTO-001.

## Audit (Tahap 1-2)

| Pertanyaan | Bukti |
| --- | --- |
| Siapa membaca `MstPayrollComponent.DefaultAmount`/`DefaultPercentage`? | **Tidak ada service payroll.** Yang dibaca `PayrollWorkforceAllowanceService` adalah milik `MstAllowanceType`. `PayrollCalculationService` hanya membaca `ComponentType`, `IsTaxable`, nama komponen |
| Siapa membaca `CalculationMethod` komponen? | `OvertimePayrollHandoffService`: komponen valid bila `IsOvertimeBased` **atau** metode `Overtime` |
| Siapa membaca `FormulaExpression`? | **Tidak ada** pengevaluasi di mana pun; formula hanya teks tersimpan. Validasi = tidak kosong dan panjang; tidak ada klaim formula aman dieksekusi |
| Siapa membaca `ComponentType`? | Kalkulasi, Penghasilan Variabel (Earning), Medical Fee (Earning), Laporan, Sinkron Piutang (Deduction), Jenis Tunjangan/Potongan; tidak diubah |
| Tabel yang mereferensikan komponen | 14 tabel (Jenis Tunjangan/Potongan, kebijakan, baris Payroll Run, input variabel, penyesuaian, pinjaman, lembur, dst.); daftar di skrip audit |
| Akar masalah | Validasi lama hanya "tidak negatif" dan "persentase <= 100"; kedua nilai disimpan dari request apa adanya |

Kepemilikan parameter permintaan: Komponen Dasar dan Tipe Komponen = `MstPayrollComponent` (sudah selesai di FIX-023/024). Kategori Kepegawaian (`EmployeeCategoryId` dan `EmploymentTypeId`), Gaji Dasar Default/Minimum/Maksimum, Unit Organisasi, Fasilitas = **`MstSalaryStructure`**, bukan komponen; tidak ada pada form/DTO/controller komponen, jadi tidak ada yang diubah. `DefaultBaseSalary` masih dipakai `WfpSalaryAssignmentController` dan `PayrollSalaryRangeRuleService`; tidak dihapus.

## Perubahan

`Controllers/PayrollComponentController.cs`: `ValidateRequestAsync` memanggil `ValidateDefaultValues(metode, nominal, persentase, formula)` (satu jalur untuk Create dan Update); `NormalizeDefaultAmount` (2 desimal) dan `NormalizeDefaultPercentage` (4 desimal) sesuai presisi kolom numeric(18,2) dan (9,4). `DTOs/PayrollComponentDtos.cs`: komentar kontrak.

Aturan (urutan evaluasi) dan pesan 400 (format ApiResponse existing):
1. nominal < 0 -> "Nominal Default tidak boleh negatif."
2. persentase < 0 atau > 100 -> "Persentase Default harus berada dalam rentang yang diperbolehkan."
3. keduanya > 0 -> "Nominal Default dan Persentase Default tidak dapat digunakan bersamaan."
4. Fixed dengan persentase > 0 -> "Metode Nominal Tetap tidak boleh menggunakan Persentase Default."
5. Percentage dengan nominal > 0 -> "Metode Persentase tidak boleh menggunakan Nominal Default."
6. Formula dengan nilai default > 0 -> "Metode Formula tidak boleh menggunakan Nominal Default maupun Persentase Default."; Formula tanpa ekspresi -> pesan lama dipertahankan.
7. Formula terisi pada metode selain Formula -> "Formula Perhitungan hanya dapat digunakan pada metode Formula."
8. **Metode lain** (ManualInput, Attendance, Overtime, Benefit): hanya aturan 1-3 dan 7. Kontrak lama dipertahankan karena tidak ada pembaca nilai default yang terbukti dan keputusan bisnis belum ada.

Dipertahankan: tanggal efektif, kategori (ditemukan/aktif; operasional bila diganti), duplikasi nama, penurunan `ComponentType`, `BaseComponentId` legacy, izin, audit log.

## Kompatibilitas dan data historis
- Respons komponen tidak berubah. Request lama yang mengirim kombinasi bertentangan kini mendapat 400 yang jelas.
- Tidak ada perubahan data. Baris legacy yang konflik tetap tersimpan sampai pengguna berwenang menyimpannya ulang dengan nilai yang benar; backend menolak penyimpanan ulang yang masih konflik.
- Perubahan nilai default tidak memengaruhi Payroll Run final (tidak ada pembaca); snapshot `PrlPayrollEmployeeComponent` tidak disentuh.

## Risiko residual / NEEDS CONFIRMATION
1. Mengubah `CalculationMethod` dari `Overtime` pada komponen yang dipakai handoff lembur dapat membuat handoff berikutnya ditolak (`PAYROLL_COMPONENT_INVALID`) bila `IsOvertimeBased` false. Tidak ada guard baru (aturan perubahan metode untuk komponen terpakai belum diputuskan).
2. Nominal Fixed = 0 / Persentase = 0 tetap diterima sebagai templat.
3. Metode lain tidak dipaksa 0: nilai lama bisa tersimpan; bila keduanya > 0 ditolak.
4. Versi efektif/approval untuk komponen terpakai belum ada; tidak dibuat.
5. `dotnet build` belum dijalankan.

## Lampiran
`sql/PAY-BE-FIX-025-audit-nilai-default-komponen.sql` (baca-saja): pengelompokan 1-5, formula pada metode non-Formula, daftar konflik beserta pemakaiannya (6-8), pemakaian seluruh komponen.
