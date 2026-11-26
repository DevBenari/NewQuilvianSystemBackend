# PAY-BE-FIX-026 — Perlindungan komponen yang sudah dipakai payroll (backend)

Tanggal: 24 November 2026. Repository: NewQuilvianSystemBackend, branch `Yasmina`, HEAD `261bb431` (working tree memuat PAY-BE-FIX-025 yang belum di-commit). Lanjutan PAY-BE-FIX-025; aturan disetujui pemilik pada 24 November 2026.

`AUTOMATED TEST: NOT APPLICABLE` (backend). Tes teks sisi frontend yang membaca source backend: 32 lulus pada dua berkas terkait. `dotnet build`, migration, database, runtime: NOT RUN.

## Preflight
Area Corporate / HumanResource / MasterData / PayrollAndBenefit; prefix `Mst` (existing); TOUCHED LEGACY; tanpa entity/DDL/snapshot. QBE: QBE-API-001, QBE-PERM-001 (atribut Access tidak diubah), QBE-VAL-001, QBE-DTO-001.

## Aturan yang disetujui
Komponen yang sudah dipakai payroll **tidak boleh mengganti Metode Perhitungan atau pindah ke kategori bertipe berbeda**. Nilai default dan nama tetap boleh diubah (tidak dibaca mesin payroll, dibuktikan di PAY-BE-FIX-025). Komponen dengan tipe tidak berubah, atau yang belum terpakai, tidak terpengaruh.

"Dipakai" = ada baris tidak terhapus pada `PrlPayrollEmployeeComponent` (baris Payroll Run), `PrlPayrollVariableInput`, `TrxOvertimeRealization`, atau `WfpOvertimeRequest` yang menunjuk komponen. Keterhubungan ke Jenis Tunjangan/Potongan **bukan** kriteria pemakaian (hampir semua komponen terhubung); penjaga lama untuk pergantian tipe pada tautan itu tetap ada.

## Perubahan
- `Controllers/PayrollComponentController.cs`: helper `IsUsedByPayrollAsync` (satu sumber kebenaran); guard di `ValidateRequestAsync` untuk Update bila `methodChanged || typeChanged` -> 400 "Komponen ini sudah dipakai oleh data payroll, sehingga Metode Perhitungan dan kategori bertipe berbeda tidak dapat diubah. Buat komponen baru, lalu nonaktifkan komponen ini."; `GET {id}` mengisi `IsUsedByPayroll` dari helper yang sama.
- `DTOs/PayrollComponentDtos.cs`: `PayrollComponentDetailResponse.IsUsedByPayroll` (additif; respons daftar dan options tidak berubah).

## Dampak
Snapshot `PrlPayrollEmployeeComponent` (kode, nama, tipe, metode, formula, Amount, Percentage) tidak disentuh, sehingga hasil yang sudah disahkan tidak berubah oleh perubahan master. Menutup risiko yang dilaporkan di PAY-BE-FIX-025: mengganti metode `Overtime` pada komponen yang dipakai handoff lembur kini ditolak.

## Risiko residual / terbuka
- Guard hanya mencakup metode dan tipe. Perubahan flag lain (`IsOvertimeBased`, `IsTaxable`, `IsEmployerContribution`) pada komponen terpakai belum dijaga (tidak diminta).
- Tidak ada persetujuan bertingkat atau versi efektif (butuh skema; keputusan belum ada).
- Komponen terpakai yang sudah salah klasifikasi tidak bisa diperbaiki lewat edit; jalurnya komponen baru + nonaktifkan yang lama.
- Pemeriksaan tambahan 4 subquery pada `GET {id}` dan pada Update; tabel kecil/terindeks FK, tetapi performa belum diukur.
- `dotnet build` belum dijalankan.
