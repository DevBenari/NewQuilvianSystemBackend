# Laporan Perubahan Backend — `PAY-BE-FIX-019`

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-BE-FIX-019` (ad-hoc lanjutan `PAY-BE-FIX-018`); pasangan `PAY-FE-FIX-019` |
| Judul | Filter daftar berdasarkan Grade Gaji dan Struktur Gaji pada kebijakan tunjangan Shift, Siaga (On Call), dan Risiko |
| Klasifikasi | `LIGHT` — 3 controller, perubahan aditif pada parameter query `GET /`; tanpa skema baru |
| Tanggal | 23 November 2026 |
| Status | 🟡 Source selesai; **belum dibuild**. Bergantung pada DDL `PAY-BE-FIX-018` (kolom `SalaryGradeId`, `SalaryStructureId`) |

## Perubahan

`GET .../{shift,on-call,hazard}-allowance-policies` menerima `salaryGradeId` dan `salaryStructureId` (opsional, `Guid?`). Filter memakai kesamaan pada cakupan TERSIMPAN; kebijakan lama tanpa cakupan tidak ikut bila filter dipakai. Pasangan filter terhadap kebijakan transportasi (PAY-BE-FIX-014). Respons daftar sudah memuat id dan nama (PAY-BE-FIX-018), tidak ada perubahan DTO.

Berkas: `Areas/Corporate/HumanResource/MasterData/PayrollAndBenefit/Controllers/{Shift,OnCall,Hazard}AllowancePolicyController.cs`.

## Verifikasi

| Pemeriksaan | Hasil |
| --- | --- |
| `dotnet build` | `NOT RUN` (kompilasi belum dibuktikan) |
| Tes teks FE `payroll-allowance-policy-scope.test.mjs` (mencocokkan teks parameter dan klausa `Where`) | Lulus; bukan eksekusi C# |
| Uji runtime | Belum |
