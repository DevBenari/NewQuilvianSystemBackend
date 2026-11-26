# PAY-BE-FIX-039 — Slip Gaji Saya: pencarian, tahun, dan perlindungan data (backend)

Tanggal: 26 November 2026. Repository: NewQuilvianSystemBackend, branch `Yasmina`, HEAD `dccfa89d` (working tree belum di-commit). Frontend: PAY-FE-FIX-039 (`1eeefd40`).

`dotnet build`: **NOT RUN**. `MIGRATION`: tidak ada. `AUTOMATED TEST`: NOT APPLICABLE untuk backend; kontrak diperiksa lewat `payroll-my-payslips.test.mjs` (root workspace). Preflight: Area Corporate/PayrollManagement, `Prl`, `TOUCHED LEGACY`; tanpa endpoint, tabel, atau permission baru.

## Hasil verifikasi (NO CHANGE REQUIRED)
Pemilik slip diturunkan dari `ApplicationUser.WorkforceProfileId` (bukan EmployeeId/DoctorId/nama) dan dicocokkan ke `PrlPayrollRunEmployee.WorkforceProfileId`; tidak ada parameter identitas dari klien; slip orang lain/belum terbit = 404; izin `MyPayslip` Read/ReadDetail/Download terpisah dari `PayrollPayslip`; unduhan menambah `DownloadCount`/`FirstDownloadedAt`/`LastDownloadedAt` dan audit. Slip hanya dibuat/diterbitkan dari run `Approved` ke atas (`AllowsPayslipGeneration`); run Submitted atau disahkan bersyarat tidak menerbitkan slip; default `PayslipPublishTrigger = PaymentPaid`.

## Perubahan
- `PayrollPayslipService.GetMineAsync`: `Search` kini diterapkan di server (nomor slip, nomor run, kode dan nama periode; ILike dengan karakter pola di-escape) dan filter `Year` (rentang akhir periode); keduanya SETELAH batas kepemilikan + terbit; satu query (Include periode sudah ada), urut periode terbaru, `AsNoTracking`. Akun tanpa `WorkforceProfileId` kini mendapat 422 "Akun Anda belum terhubung dengan profil pegawai/dokter. Hubungi administrator HRD." (sebelumnya daftar kosong diam-diam); tanpa fallback identitas, tanpa membuat/memperbaiki relasi.
- `DTOs/PayrollPayslipDtos.cs` (aditif): `PayrollPayslipQuery.Year`, `PayslipSummaryResponse.PeriodName`.
- `PayrollMyPayslipsController`: header `Cache-Control: private, no-store` dan `Pragma: no-cache` pada daftar, detail, dan unduhan.

## Risiko / belum diverifikasi
Belum dibuild; terjemahan `ILike(..., escape)` pada navigasi `PayrollRun.PayrollPeriod` perlu build; relasi User–WorkforceProfile pada database tidak diperiksa (NEEDS CONFIRMATION); izin MyPayslip pada role belum diverifikasi; tanpa UAT runtime. Perubahan perilaku: daftar kosong → 422 untuk akun tanpa profil (admin tanpa profil yang membuka menu ini akan melihat pesan tersebut).
