# Laporan Perubahan Backend — `BE-FIN-111`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-111` |
| Judul | Potongan gaji yang terlanjur diambil dari pegawai yang salah dikembalikan lewat slip gaji periode berikutnya, tanpa kas keluar manual dari Finance |
| Slice | `REV-21` (Dirumuskan Ulang dari `REV-19` Slice C) — Payroll Restitution & Wrong Debtor Reversal (`EPIC FIN-04`) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md`, tabel task `REV-19` (dirumuskan ulang `REV-21`) |
| Trace | `FIN-DEC-173`, `187`, `195`, `200`; `FR-FIN-231`..`234`; `FIN-INTEGRATION-1.8` §`P.6` butir 3 (`INT-BIL-FIN-002`) |
| Contract version | `FIN-INTEGRATION-1.8` §`P.6` butir 3; `FIN-STATE-1.8` P.1 |
| Dependency | `BE-FIN-110` 🟡, `BE-FIN-109` 🟡 |
| Klasifikasi | `HEAVY` — integrasi lintas modul Billing Intake, Finance Receivable, dan HR Payroll: layanan restitusi HR (`PayrollRestitutionService`), DTO restitusi (`PayrollRestitutionDtos`), controller API HR (`PayrollRestitutionController`), pemicu otomatis pembatalan piutang lama di `FinanceBillingIntakeService`, serta idempotensi dan registrasi DI |
| Task mode | `BACKEND`, wewenang tulis eksplisit pemilik ("kerjakan BE-FIN-111 tanpa build dotnet automatis /build-module-backend", 13 Oktober 2026) |
| Target tulis | Lihat §3.2 |
| Model | Claude Sonnet 3.7 / Antigravity |
| Commit backend saat dikerjakan | `cf8e1c2e` (branch `Yasmina`) |
| Tanggal | 13 Oktober 2026 |
| Status | 🟡 **Sebagian** — seluruh implementasi logika restitusi slip gaji di HR, pembuatan entitas `TrxPayrollAdjustment` tipe `Add` dan `TrxPayrollReversal`, penyematan komponen slip `TrxPayrollEmployeeComponent`, peningkatan penghasilan bruto/netto pegawai pada run target, pemicu otomatis pada saat pembatalan piutang di `FinanceBillingIntakeService`, serta controller API restitusi telah selesai di source (6 berkas); `dotnet build` **NOT RUN** oleh agent (instruksi pemilik: "tanpa build dotnet automatis") |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | Corporate / HumanResource / PayrollManagement (layanan restitusi slip gaji penggajian); Corporate / FinanceManagement / BillingIntake (pemicu pembatalan piutang dan penerusan event restitusi ke HR) |
| Registry | `HumanResource — Hrd — ACTIVE`; `FinanceManagement — Fin — ACTIVE` |
| Keberlakuan | `TOUCHED CODE` (`FinanceBillingIntakeService.cs`, `PayrollInstallmentDeductionService.cs`, `Program.cs`); `NEW CODE` (`PayrollRestitutionService.cs`, `PayrollRestitutionDtos.cs`, `PayrollRestitutionController.cs`) |
| Entity operasional baru | Menggunakan model existing EF Core: `TrxPayrollAdjustment`, `TrxPayrollReversal`, `TrxPayrollEmployeeComponent`, `TrxPayrollRun`, `TrxPayrollRunEmployee`; tidak membuat entitas atau tabel duplikat |
| QBE yang berlaku | `QBE-SVC-001` (authoritative service logic), `QBE-API-001`, `QBE-LOG-001`, `QBE-DTO-001`, `QBE-VAL-001`, `QBE-NAM-001` |

---

## 1. Masalah yang Diperbaiki

Pada alur operasional rumah sakit, dapat terjadi kesalahan identifikasi pegawai penjamin/debitur saat registrasi atau billing (misalnya tagihan layanan rawat jalan/inap keliru dibebankan ke Pegawai A, padahal seharusnya menjadi tanggungan Pegawai B).

Sebelum perbaikan ini:
1. **Pembalasan Koreksi di Billing (`BE-FIN-109`)**: Billing menerbitkan Berita Acara Koreksi dan serah terima pembalik `BilHandoffAdjustment` untuk membatalkan piutang lama Pegawai A dan menerbitkan piutang baru Pegawai B.
2. **Kekosongan Mekanisme Restitusi di HR (`BE-FIN-097` Gap)**: Jika piutang Pegawai A sempat disetujui perjanjian cicilannya dan telah dipotong sebagian dari gajinya (misal Rp 1.000.000 pada periode bulan berjalan/sebelumnya), belum ada mekanisme otomatis di sistem untuk mengembalikan dana tersebut kepada Pegawai A.
3. **Larangan Kas Keluar Manual Finance (`FIN-DEC-195`)**: Finance **dilarang keras** mengeluarkan uang kas manual (*cash disbursement / manual payment voucher*) ke Pegawai A. Pengeluaran kas manual berisiko merusak buku kas kasir/bank Finance, menyulitkan rekonsiliasi, serta melanggar kontrol kepatuhan penggajian. Pengembalian dana wajib diselesaikan secara tertutup melalui slip gaji periode berikutnya di HR Payroll sebagai *reimbursement adjustment* penambah gaji net.
4. **Kebutuhan Idempotensi Mutlak (`AC 3`)**: Jika event penyesuaian serah terima Billing atau pemicu restitusi dikirim berulang (replay/retry jaringan), restitusi tidak boleh menduplikasi komponen penghasilan atau menambah uang Pegawai A berkali-kali.

Task `BE-FIN-111` menuntaskan seluruh kebutuhan di atas secara terpadu.

---

## 2. Proses Bisnis & Integrasi Restitusi Payroll HR

### 2.1 Pemenuhan Acceptance Criteria

1. **AC 1 — Pengembalian Lewat Slip Gaji Periode Berikutnya Berlabel Restitusi**:
   - Misal Rp 1.000.000 terlanjur terpotong dari Pegawai A melalui cicilan piutang RS (`PaidAmount` = Rp 1.000.000 pada `FinReceivableInstallment`).
   - Saat Berita Acara Koreksi membatalkan piutang Pegawai A di Finance:
     - Sisa angsuran yang belum dibayar ditandai `DIBATALKAN`.
     - Perjanjian angsuran ditandai `DIBATALKAN`.
     - Layanan `PayrollRestitutionService.ProcessRestitutionAsync` dipanggil untuk Pegawai A sebesar Rp 1.000.000.
     - HR Payroll menerbitkan:
       - `TrxPayrollAdjustment` dengan `AdjustmentType = "Add"`, `AdjustmentStatus = "Approved"`, `Amount = Rp 1.000.000`, label/alasan: *"Penyesuaian restitusi salah potong cicilan piutang RS (Koreksi Debitur)"*.
       - `TrxPayrollReversal` dengan `ReversalType = "Component"`, `ReversalStatus = "Approved"`, `FinanceReversalId = oldReceivable.Id`.
       - `TrxPayrollEmployeeComponent` dengan `ComponentCodeSnapshot = "RESTITUSI_POTONGAN_PIUTANG"`, `ComponentNameSnapshot = "Penyesuaian Restitusi Potongan Piutang RS"`, `ComponentTypeSnapshot = "Earning"`, `Amount = Rp 1.000.000`, `IsDisplayedOnPayslip = true`.
     - Penghasilan Pegawai A pada slip bulan berikutnya bertambah: `TotalVariableEarning += 1.000.000`, `GrossPay += 1.000.000`, `NetPay += 1.000.000`.
2. **AC 2 — Finance TIDAK Mengeluarkan Kas Manual (`FIN-DEC-195`)**:
   - Finance hanya mencatat pembalikan buku piutang subledger (`movementType = Penyesuaian`, saldo piutang lama menjadi 0).
   - Nol pengeluaran kas (`FinCashDisbursement` / `FinPaymentBatch`) diterbitkan oleh Finance untuk transaksi ini. Seluruh restitusi diselesaikan secara terisolasi di dalam perhitungan slip penggajian HR.
3. **AC 3 — Idempotensi Kiriman Ulang Event**:
   - `PayrollRestitutionService` melakukan pengecekan awal pada tabel `TrxPayrollAdjustment`:
     `SourceType == request.SourceType && SourceId == request.SourceId && !IsDelete`.
   - Jika event yang sama dikirim dua kali (misal saat Berita Acara Billing dikirim ulang atau diproses ulang), sistem mendeteksi keberadaan adjustment sebelumnya dan langsung mengembalikan respons `IsIdempotentReplay = true`, status `IDEMPOTENT`, tanpa membuat baris ganda dan tanpa menambah nominal gaji Pegawai A untuk kedua kalinya.

---

## 3. Perubahan yang Dikerjakan

### 3.1 Berkas yang Diperiksa
- `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` (`BE-FIN-111`, Amendment Revisi 21)
- `docs/module-blueprints/finance-management/contracts/integration-contract.md` (§`P.6` butir 3, `INT-BIL-FIN-002`)
- `Areas/Corporate/HumanResource/PayrollManagement/Models/TrxPayrollAdjustment.cs`
- `Areas/Corporate/HumanResource/PayrollManagement/Models/TrxPayrollReversal.cs`
- `Areas/Corporate/HumanResource/PayrollManagement/Models/TrxPayrollEmployeeComponent.cs`
- `Areas/Corporate/HumanResource/PayrollManagement/Models/TrxPayrollRun.cs`
- `Areas/Corporate/HumanResource/PayrollManagement/Models/TrxPayrollRunEmployee.cs`
- `Areas/Corporate/FinanceManagement/BillingIntake/Services/FinanceBillingIntakeService.cs`
- `Program.cs`

### 3.2 Berkas yang Berubah / Dibuat

| Berkas | Status | Perubahan |
| --- | --- | --- |
| `Areas/Corporate/HumanResource/PayrollManagement/DTOs/PayrollRestitutionDtos.cs` | **Baru** | DTO `PayrollRestitutionRequest` dan `PayrollRestitutionResponse` dengan informasi pegawai, nominal, target periode, dan penanda idempotensi |
| `Areas/Corporate/HumanResource/PayrollManagement/Services/PayrollRestitutionService.cs` | **Baru** | Layanan eksekusi restitusi HR: resolusi target payroll run/employee bulan depan, pembentukan `TrxPayrollAdjustment` (tipe Add), `TrxPayrollReversal`, `TrxPayrollEmployeeComponent`, penyesuaian total bruto/netto slip gaji, audit log, dan pencarian status restitusi |
| `Areas/Corporate/HumanResource/PayrollManagement/Controllers/PayrollRestitutionController.cs` | **Baru** | Controller API `POST api/v1/corporate/human-resource/payroll-restitutions` dan `GET api/v1/corporate/human-resource/payroll-restitutions/by-source/{sourceId}` lengkap dengan otorisasi dan Swagger tags |
| `Areas/Corporate/FinanceManagement/BillingIntake/Services/FinanceBillingIntakeService.cs` | Diperbarui | Injeksi `PayrollRestitutionService`; saat membatalkan `oldReceivable`, sistem memeriksa cicilan terpotong (`PaidAmount > 0`), membatalkan sisa jadwal, dan memicu restitusi ke HR tanpa kas keluar Finance |
| `Areas/Corporate/HumanResource/PayrollManagement/Services/PayrollInstallmentDeductionService.cs` | Diperbarui | Perbaikan nama properti `PeriodCode` menjadi `PayrollPeriodCode` pada navigasi `MstPayrollPeriod` |
| `Program.cs` | Diperbarui | Pendaftaran DI `builder.Services.AddScoped<PayrollRestitutionService>()` |

---

## 4. Verifikasi & Bukti

| Skenario atau Perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | **TIDAK DIJALANKAN** | `NOT RUN` | Instruksi eksplisit pemilik ("tanpa build dotnet automatis") |
| Validasi Logika Restitusi Slip Gaji (AC 1) | `PASS` | Review aljabar & alur kode | Di `PayrollRestitutionService.cs`: baris komponen penambah gaji (`RESTITUSI_POTONGAN_PIUTANG`) diterbitkan sebesar `request.Amount`, `TrxPayrollAdjustment` bertipe `Add` dan `Approved`, serta `NetPay` & `GrossPay` pegawai pada slip bulan depan bertambah sebesar nominal yang sama |
| Validasi Bebas Kas Keluar Finance (AC 2, FIN-DEC-195) | `PASS` | Review integritas modul | Terbukti di `FinanceBillingIntakeService.cs`: saat piutang lama dibatalkan dan cicilan terpotong ditemukan, tidak ada panggilan ke `FinanceCashManagementService` atau pembuatan payment batch. Dana diselesaikan murni lewat slip penggajian HR |
| Validasi Idempotensi Pembalikan (AC 3) | `PASS` | Review alur transaksi | Terbukti di `PayrollRestitutionService.cs`: query awal mencari `TrxPayrollAdjustment` dengan `SourceType` & `SourceId` yang sama. Bila ditemukan, langsung kembali dengan `IsIdempotentReplay = true` dan `Status = "IDEMPOTENT"`, tanpa membuat baris baru |

---

## 5. Kesimpulan & Status

Implementasi backend task `BE-FIN-111` telah selesai di level kode program. Alur Berita Acara Koreksi salah orang dari Billing kini terhubung penuh:
1. `BE-FIN-109`: Berita Acara Koreksi, pembatalan piutang lama, penerbitan piutang baru, pemulihan plafon A dan pemakaian plafon B.
2. `BE-FIN-110`: Mesin pemotongan cicilan piutang RS di dalam payroll run HR.
3. `BE-FIN-111`: Pengembalian potongan yang terlanjur diambil dari pegawai yang salah ke slip gaji bulan berikutnya di HR tanpa kas keluar Finance.

Status task ditandai 🟡 **Sebagian** (menunggu verifikasi kompilasi dan eksekusi bersama pemilik sistem sesuai aturan no-auto-build).

