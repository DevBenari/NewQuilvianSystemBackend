# Laporan Perubahan Backend — `PAY-BE-FIX-022`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-BE-FIX-022` (ad-hoc lanjutan seri `PAY-BE-FIX`; lanjutan `PAY-BE-FIX-021`) |
| Judul | Transportasi lewat alur umum Tambah Tunjangan: mesin payroll membayar penetapan Transport bersumber kebijakan, satu sumber pembayaran, penetapan berbasis kebijakan terkunci dari ubah nominal |
| Backend Governance Preflight | Area `Corporate`, modul `PayrollManagement`, prefix `Prl` (ACTIVE), `TOUCHED LEGACY` (QBE: tanpa entity operasional baru, tanpa `Trx*`); penulisan lewat service + `PayrollTransactionRunner` yang sama dengan `PAY-BE-FIX-021` |
| Branch / HEAD | BE `Yasmina` `ab340bc7de4e9a041ca3a8e0289dc910f417ec82` (dikonfirmasi pemilik); working tree berisi pekerjaan `PAY-BE-FIX-015…021` yang belum di-commit |
| Tanggal | 23 November 2026 |
| Status | 🟡 Source selesai; **`dotnet build` NOT RUN**, runtime NOT RUN. Tidak ada perubahan skema (tanpa DDL baru) |

## 1. Hasil

1. **Mesin payroll** (`PayrollCalculationService`): penetapan `PrlWorkforceAllowance` kategori **Transport** kini dibayar bila `AssignmentSource = Policy`; sumber lain tetap dilewati dengan peringatan lama. Rincian baris komponen memuat **provenance**: `assignmentSource`, `allowancePolicyId`, `allowancePolicyGradeRateId`, `salaryGradeId`.
2. **Pagar pembayaran ganda dua arah** (`TransportAllowanceSourceGuard`, baru):
   - `POST from-policy` untuk jenis Transport ditolak (409) bila pegawai masih punya transport bulanan lama (`PrlTransportAllowance` aktif, kebijakan aktif `IsIncludedInPayroll`, `FixedMonthly`) yang masa berlakunya beririsan.
   - `POST/PUT` transportasi lama ditolak (409) bila pegawai punya penetapan Transport berbasis kebijakan (Draft, menunggu persetujuan, atau disetujui) yang beririsan. Menonaktifkan penetapan lama **tidak** diblokir.
   - Jaring pengaman: bila data tetap beririsan, perhitungan pegawai **digagalkan** dengan pesan jelas; tidak ada sumber yang dipilih diam-diam.
3. **`from-policy` menerima Transport** (kebijakan umum, metode `FixedAmount`/`PercentageOfBaseSalary`). Daftar kebijakan menampilkan kebijakan umum yang dapat dipilih; kebijakan transportasi lama ditampilkan **hanya baca** dengan alasan (belum dipetakan HRD).
4. **Pembuatan penetapan transportasi lama dinonaktifkan** (`POST .../transport-allowance` menjawab 409 dengan arahan ke Tambah Tunjangan). `PUT` dan `PATCH .../status` tetap ada untuk mengelola/menonaktifkan penetapan lama; tidak ada penghapusan data. Endpoint dan izin `Create` dipertahankan agar pemetaan Akses Role tidak berubah.
5. **Penetapan berbasis kebijakan tidak dapat diubah nilainya** (`PUT` ditolak, aksi `Update` tidak ditawarkan, `LockReason` jelas, label sumber "Dari Kebijakan"). Satu-satunya jalan: nonaktifkan lalu tetapkan ulang dari kebijakan.

## 2. Berkas

Baru: `Services/TransportAllowanceSourceGuard.cs`.
Diubah: `Services/PayrollCalculationService.cs`, `Services/PayrollWorkforceAllowanceService.Policy.cs`, `Services/PayrollWorkforceAllowanceService.cs` (`IndividualRow` + `AssignmentSource`, `editable`, `LockReason`, guard `UpdateAsync`), `Controllers/WfpTransportAllowanceController.cs` (`CheckUnifiedTransportConflictAsync` pada Create/Update), `DTOs/PayrollAllowancePolicyAssignmentDtos.cs` (komentar).
Tes (repo FE, hanya mencocokkan teks source): `tests/unit/payroll-allowance-policy-assignment.test.mjs` (6 lulus), `payroll-allowance-policy-grade-sync.test.mjs`.

## 3. Skema / database

**Tidak ada perubahan skema, migration, atau `ModelSnapshot` pada tugas ini.** Kolom provenance sudah ada dari `PAY-BE-FIX-021` (DDL/migration `AddPrlAllowancePolicyAndWorkforceAllowanceProvenance` harus sudah diterapkan sebelum backend dijalankan).

## 4. Status

| Butir | Status |
| --- | --- |
| Pembayaran Transport bersumber kebijakan, provenance pada rincian, pagar dua arah, kunci ubah | **IMPLEMENTED** (source) |
| Kompilasi C#, perilaku runtime, hasil payroll run | **NOT VERIFIED** (`dotnet build` dan runtime tidak dijalankan) |
| Kebijakan umum Transportasi per Grade Gaji belum berisi data | **BLOCKED** sampai HRD mengisi lewat template SQL `PAY-BE-FIX-021` §5.1 (belum ada layar pengelola) |
| Pemetaan kebijakan transportasi lama (`EmployeeGradeId`/`SalaryGradeId`, metode `PerAttendance`, dll.) ke kebijakan umum dan pemindahan penetapan lama | **NEEDS CONFIRMATION** (keputusan HRD; tidak diubah) |
| Transportasi `PerAttendance`/`PerDay`/`Reimbursement` lewat alur umum | **PROPOSED** (mesin payroll tidak menghitungnya). **Dampak keputusan pemilik:** karena pembuatan di jalur lama dinonaktifkan, penetapan baru bermetode ini tidak dapat dibuat lagi di sistem sampai alur umum mendukungnya — **NEEDS CONFIRMATION** |
| Shift / Siaga / Risiko lewat alur umum | **PROPOSED** (belum ada consumer aktivitas aktual; sengaja ditolak) |
| Kolom provenance khusus pada baris payroll (di luar JSON rincian) | **BLOCKED_BY_SCHEMA** (butuh DDL; pemilik) |

## 5. Tinjauan statis (subset relevan dari 35 skenario; bukan eksekusi)

| # | Skenario | Hasil tinjauan |
| --- | --- | --- |
| 20 | Penetapan ganda | Idempoten + `EnsureNoOverlapAsync` + advisory lock (021); lintas sumber oleh `TransportAllowanceSourceGuard` |
| 21 | Struktur Gaji tidak terhitung ganda | `EnsureNotInSalaryStructureAsync` + mesin melewati jenis yang sudah di struktur (tidak diubah) |
| 22 | Transport lama tidak ganda | Guard create dua arah + galat mesin; lulus tinjauan statis |
| 23 | Perubahan Grade Gaji | Masa berlaku penetapan dibatasi akhir penetapan gaji; grade baru = penetapan baru (otomatis tidak ada) |
| 24 | Payroll final utuh | Mesin tidak menulis ke run final; snapshot tidak diubah |
| 25–27 | Bonus/insentif/lembur | Variabel/aktivitas tetap ditolak oleh `PolicyFlowBlockReason`; lembur tidak disentuh |
| 28 | Shift/On-call/Risiko | Ditolak (bukan Payable) |
| 29–30 | RBAC/ViewAmount | Izin `WorkforceAllowanceAssignment` Read/Create tetap; nominal preview null tanpa `ViewAmount` |
| 35 | Tanpa migration/skema | Terpenuhi pada tugas ini |
| — | Perhitungan Gross/Taxable/PPh/Net | **NOT VERIFIED** (tanpa eksekusi); komponen memakai `PAY_WORKFORCE_ALLOWANCE` dan `IsTaxable` jenis tunjangan seperti biasa |

## 6. Risiko

- Penetapan Transport lama yang masih aktif memblokir penetapan baru (sengaja). Pemindahan data lama perlu keputusan HRD.
- Bila dua sumber terlanjur aktif bersamaan (mis. data diisi lewat SQL), payroll run pegawai itu gagal dengan pesan jelas sampai salah satunya dinonaktifkan.
- Pemakaian `ToLower()` pada kategori di kueri guard bergantung pada terjemahan Npgsql; belum dijalankan.

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.

## Addendum 23 Nov 2026 — Transport "tidak dapat dipilih" padahal kebijakan sudah ada

**Penyebab:** "Daftar Kebijakan Tunjangan Transportasi" (halaman lama, tabel `PrlTransportAllowancePolicy`) bukan sumber dropdown "Tambah Tunjangan". Dropdown membaca Kebijakan Tunjangan umum (`PrlAllowancePolicy` + tarif per Grade Gaji) yang masih kosong, sehingga jenis Transport ditolak dengan "Belum tersedia kebijakan aktif...".

**IMPLEMENTED (source, belum dibuild):** `PayrollWorkforceAllowanceService.Policy.cs` — pesan khusus bila kebijakan lama ada untuk Grade Gaji pegawai tetapi belum dipindahkan; kebijakan lama yang sudah dipindahkan (`MigratedPolicyCode`, awalan `MIG-`) tidak tampil dobel. Lampiran SQL `sql/PAY-BE-FIX-022-pindah-kebijakan-transport-lama.sql` (aman diulang, berakhir ROLLBACK bawaan, tidak dijalankan oleh saya).

**NEEDS CONFIRMATION:** kebijakan umum tidak mengenal Struktur Gaji (lama: "Per Struktur Gaji") — bila satu grade punya >1 struktur aktif, kebijakan berlaku untuk semuanya; PerAttendance/PerDay/Reimbursement tidak dipindahkan.
