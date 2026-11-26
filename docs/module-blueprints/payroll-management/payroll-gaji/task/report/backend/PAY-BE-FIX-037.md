# PAY-BE-FIX-037 — Saldo Awal PPh: pengaju boleh menyetujui sendiri (backend)

Tanggal: 26 November 2026. Repository: NewQuilvianSystemBackend, branch `Yasmina`, HEAD `10563ac5` (working tree belum di-commit).

`dotnet build`: **NOT RUN** (dilarang). `MIGRATION`: tidak ada. `AUTOMATED TEST`: NOT APPLICABLE untuk backend; kontrak diperiksa lewat tes statis FE (`payroll-opening-tax-balance*.test.mjs`). Preflight: Area Corporate/PayrollManagement, `Prl`, `TOUCHED LEGACY`.

## Perubahan (keputusan pemilik, 26 Nov 2026)
`Services/PayrollOpeningTaxBalanceService.cs` `ApproveAsync`: penolakan "Pengaju tidak boleh menyetujui saldo awal yang diajukannya sendiri." (PayrollForbiddenActionException/403) DIHAPUS. Komentar kelas dan komentar guard diperbarui. `Controllers/PayrollOpeningTaxBalancesController.cs`: komentar dan deskripsi izin Approve diperbarui (deskripsi AccessAction berubah teks saja).

Tetap berlaku: `[AccessPermission("PayrollOpeningTaxBalance","Approve")]`, `RequireDecisionPermission` (ViewAmount), status harus `PENDING_APPROVAL`, irisan payroll run final, irisan saldo APPROVED sumber lain, penggantian versi lama (SUPERSEDED), indeks unik, audit. Aktor dan waktu pengajuan (`SubmittedBy/At`) dan persetujuan (`ApprovedBy/At`) tetap dicatat terpisah, dan `Reject` tidak pernah membandingkan pengguna.

## Dampak
Kewenangan kini sepenuhnya di Role Management: pengguna yang memegang Submit + Approve + ViewAmount dapat mengajukan dan menyetujui saldo yang sama (dua aksi terpisah). Tanpa pemisahan tugas pada saldo awal PPh, kontrol atas kebenaran nominal bergantung pada pemeriksa yang sama; keputusan ini diambil pemilik. Mengembalikan pemisahan = memasukkan kembali satu pemeriksaan di `ApproveAsync`.

Frontend: hanya teks ("pengguna lain" → "pengguna yang berwenang") dan tes statis; tanpa perubahan alur. Belum dibuild; tanpa UAT runtime.
