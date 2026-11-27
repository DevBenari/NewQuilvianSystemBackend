# PAY-BE-FIX-035 — Saldo Awal PPh: opsi Pegawai / Dokter dan Dokumen Sumber bukan syarat (backend)

Tanggal: 25 November 2026. Repository: NewQuilvianSystemBackend, branch `Yasmina`, HEAD `c073f8c1` (working tree belum di-commit). Frontend: PAY-FE-FIX-035.

`dotnet build`: **NOT RUN** (dilarang). `MIGRATION`: tidak ada (kolom `SourceReference` dan tabel tidak berubah). `AUTOMATED TEST`: NOT APPLICABLE untuk backend; kontrak diperiksa lewat tes statis `payroll-opening-tax-balance-workforce.test.mjs` dan `payroll-opening-tax-balance.test.mjs`. Preflight: Area Corporate/PayrollManagement, `Prl`, `TOUCHED LEGACY`, tanpa entity baru.

## Akar masalah (dokter tidak muncul)
Sumber dropdown ada di frontend: resource `workforceProfiles` memanggil `human-resource/master-data/employees/options` (hanya `MstEmployee`; komentar source menyebut dokter belum termasuk) dan memerlukan izin `Employee:Read` HR. Backend saldo awal sendiri sudah menerima profil dokter: `ValidateAsync` hanya memeriksa `MstWorkforceProfiles` (kontrak penyimpanan = `WorkforceProfileId`).

## Perubahan
`Controllers/PayrollOpeningTaxBalancesController.cs`: `GET .../opening-tax-balances/workforce/options` (read-only; `[AccessAction("Create")]` + `[AccessPermission("PayrollOpeningTaxBalance","Create")]`, tanpa AccessAction baru); deskripsi izin Submit tidak lagi menyebut dokumen sumber.

`DTOs/PayrollOpeningTaxBalanceDtos.cs`: `PayrollOpeningTaxBalanceWorkforceQuery` (search, pageNumber, pageSize) dan `PayrollOpeningTaxBalanceWorkforceOptionResponse` (workforceProfileId, name, workforceType Employee|Doctor, workforceTypeLabel, number, numberLabel, label). `SourceReference` tetap nullable, tanpa `[Required]`.

`Services/PayrollOpeningTaxBalanceService.cs`:
- `GetWorkforceOptionsAsync`: satu query atas `MstWorkforceProfiles` (profil aktif tidak dihapus dengan pegawai ATAU dokter aktif tidak dihapus), pencarian nama/kode/NIP/nomor dokter, urut nama, paginasi (maks 100), satu baris per profil (tidak ada penggabungan nama, tidak ada N+1), tanpa data pribadi/nominal. Profil yang punya data pegawai aktif berjenis Pegawai; selain itu Dokter.
- `SubmitAsync`/`ApproveAsync`: pemeriksaan `SourceReference` kosong DIHAPUS. Tidak diubah: status, maker-checker (pengaju ≠ penyetuju), `RequireDecisionPermission` (ViewAmount), irisan payroll run final, irisan saldo APPROVED lain, penggantian versi lama, indeks unik, audit.
- `ValidateAsync`: pesan "Pegawai atau dokter wajib dipilih." dan "Data pegawai atau dokter tidak ditemukan."; tidak mewajibkan `MstEmployee`.
- `Apply`: `SourceReference` hanya ditulis bila klien mengirim field itu (`SourceReference is not null`); update dari form baru tidak menimpa data lama dengan null. Tidak ada teks buatan. `ReviseAsync` tetap menyalin nilai lama.
- `ToResponse`: nama dari profil (cadangan: `ProfileCode`).

## Rekonsiliasi PPh (verifikasi statis)
`PayrollOpeningTaxBalanceStoreSource.LoadApprovedAsync` memuat saldo APPROVED per `WorkforceProfileId`; `PayrollCalculationService` memanggilnya dengan `workforceIds` run dan hanya memakai `SAME_EMPLOYER_PRE_CUTOVER` pada `BuildYtd`. Jalur ini tidak membedakan pegawai/dokter. `SourceReference` tidak dibaca mesin hitung (hanya diteruskan sebagai fakta). Tidak ada perubahan kategori pajak, metode, atau perhitungan.

## NEEDS CONFIRMATION
- Apakah dokter pada kebijakan pajak rumah sakit memakai jalur PPh 21 yang membaca saldo awal (kategori penerima/profil pajak dokter): tidak diverifikasi dan tidak diubah.
- Bukti perpajakan: kontrol wajib dokumen sumber dihapus atas permintaan; bila kebijakan rumah sakit mensyaratkan bukti, itu keputusan owner (field tidak dikembalikan otomatis).
- Temuan lama (tidak diubah): `ValidateAsync` menerima profil apa pun yang tidak dihapus (termasuk eksternal); hanya opsi UI yang dibatasi pegawai/dokter.

## Risiko
Belum dibuild; kompilasi dan terjemahan LINQ (`p.Employee != null && ...`) perlu build; tanpa UAT runtime.
