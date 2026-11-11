# Laporan Penyelesaian Backend Task: BE-HRD-159

## 1. Identitas Task
* **ID Task**: `BE-HRD-159`
* **Nama Task**: Webhook reconciler callback pembayaran/penolakan Finance (`INT-HR-54`) & distribusi slip gaji digital ESS ber-PIN sekunder 6-digit (`VR-PAY-22` s.d. `VR-PAY-24`, `PAY-B5`, `PAY-B6`, `PAY-B7`, `PAY-B8`, `HRD-ASM-30`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 10 — Pemrosesan Penggajian Pegawai Rumah Sakit (Payroll V2)
* **Slice / Milestone**: `Slice S-B5` · `Fase MVP-32: Serah Terima Finance SHA-256, Webhook Reconciler, & Slip Gaji Digital ESS`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-06
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Setelah berkas serah terima diserahkan ke Finance, sistem HRD memerlukan mekanisme umpan balik otomatis (*Webhook Callback*) untuk mengetahui status akhir transfer bank serta mengamankan hak akses slip gaji pegawai mandiri:

1. **Webhook Callback Konfirmasi Pembayaran (`INT-HR-54`, `HRD-DEC-126`)**:
   - Jika proses kliring bank sukses (*PaymentConfirmed*):
     * Status `TrxPayrollRun` beralih ke terminal `Completed`.
     * Status `TrxPayrollHandoffBatch` menjadi `PaymentConfirmed` dengan nomor referensi transfer bank kasir.
     * Memicu pelepasan (*release*) slip gaji digital `TrxPayrollPayslip` ke portal ESS pegawai per tanggal 25 (`VR-PAY-23`).
2. **Webhook Callback Penolakan & Rollback Terkendali (`PAY-B7`, `VR-PAY-16`)**:
   - Jika Finance mendeteksi kendala rekening bank atau likuiditas (*PaymentRejected*):
     * Finance **wajib menyertakan alasan penolakan minimal 15 karakter** formal.
     * Status `TrxPayrollRun` dikembalikan secara terkendali ke `NeedCorrection`.
     * Gembok batch dibuka kembali (`IsLocked = false`) agar petugas payroll dapat mengoreksi rekening tujuan yang bermasalah.
3. **Distribusi Slip Gaji Mandiri ESS & Proteksi PIN 6-Digit (`VR-PAY-22`, `VR-PAY-24`, `PAY-B6`, `HRD-ASM-30`)**:
   - Seluruh pegawai rumah sakit dapat melihat riwayat slip gaji miliknya pada modul ESS.
   - Untuk membuka lembar rincian gaji atau mengunduh berkas PDF, pegawai **wajib memasukkan PIN sekunder 6-digit** (standar bawaan kombinasi tanggal lahir `DDMMYY`).
   - **Penalti Keras Lockout 30 Menit (`VR-PAY-22`, `PAY-B6`)**: Jika pegawai salah memasukkan PIN sebanyak 3 kali berturut-turut, akses menu slip gaji dikunci selama 30 menit (HTTP 429 Too Many Requests).
   - Berkas PDF slip gaji dilengkapi watermark resmi rumah sakit (*"DOKUMEN RAHASIA - DILINDUNGI HUKUM KETENAGAKERJAAN RS QUILVIAN"*) dan jejak audit pengunduhan (`DownloadCount`, `LastDownloadedAt`).

---

## 3. Komponen yang Diimplementasikan

### A. Data Transfer Objects (DTO)
* Berkas: `Areas/Corporate/HumanResource/PayrollManagement/DTOs/PayrollReconciliationAndPayslipDtos.cs`
* Komponen:
  - `FinancePaymentConfirmationCommand`: Payload callback konfirmasi pembayaran kasir/bank.
  - `FinanceRejectionCallbackCommand`: Payload callback penolakan ber-alasan minimal 15 karakter (`[MinLength(15)]`).
  - `FinanceConfirmationResultDto`: Hasil konfirmasi pencairan bank & jumlah slip gaji yang dirilis.
  - `FinanceRejectionResultDto`: Hasil penolakan Finance & status rollback terkendali.
  - `SelfServicePayslipSummaryDto`: Ringkasan slip gaji pada tabel riwayat ESS.
  - `SelfServicePayslipDetailDto`: Rincian penerimaan dan potongan gaji pasca verifikasi PIN.
  - `VerifyPayslipPinRequest`: Permintaan verifikasi PIN 6-digit numerik (`Regex: ^\d{6}$`).
  - `PayslipPinVerificationResultDto`: Status verifikasi PIN, token sesi 15 menit, dan sisa percobaan.

### B. Antarmuka & Layanan Bisnis
* Berkas:
  - `Areas/Corporate/HumanResource/PayrollManagement/Services/IPayrollDisbursementReconciliationService.cs`
  - `Areas/Corporate/HumanResource/PayrollManagement/Services/PayrollDisbursementReconciliationService.cs`
  - `Areas/Corporate/HumanResource/PayrollManagement/Services/IPayrollPayslipDistributionService.cs`
  - `Areas/Corporate/HumanResource/PayrollManagement/Services/PayrollPayslipDistributionService.cs`
* Logika Bisnis:
  1. `ConfirmPaymentAsync`:
     - Mengubah status batch ke `Completed` dan merilis entitas `TrxPayrollPayslip` bagi seluruh pegawai aktif.
  2. `RejectPaymentAsync`:
     - Memvalidasi alasan penolakan $\ge 15$ karakter.
     - Rollback status run ke `NeedCorrection` dan membuka gembok (`IsLocked = false`).
  3. `VerifyPinAsync`:
     - Evaluasi batas lockout 30 menit jika kesalahan mencapai 3 kali berturut-turut.
     - Verifikasi PIN terhadap tanggal lahir `BirthDate` di tabel `MstEmployee`.
     - Penerbitan token sesi `PinToken` berdurasi 15 menit.
  4. `GetPayslipDetailAsync` & `DownloadPayslipPdfAsync`:
     - Memvalidasi keabsahan token sesi PIN.
     - Menyusun rincian komponen penerimaan (Gaji Pokok, Tunjangan, Lembur PP 35, Premi Malam) dan potongan (BPJS, PPh 21 TER, Pinjaman).
     - Menghasilkan dokumen biner PDF resmi dengan format standar ISO dan stempel watermark.

### C. Controller REST API Bergaya Swagger
* Tag Swagger: `[Tags("HR Payroll - Finance Handoff")]` & `[Tags("HR Payroll - Self-Service Payslips")]`
* Endpoint:
  - `POST /api/payroll/runs/{id}/finance-payment-confirmed` — Webhook konfirmasi transfer bank sukses.
  - `POST /api/payroll/runs/{id}/finance-rejection` — Webhook penolakan Finance & rollback terkendali.
  - `GET /api/payroll/runs/self-service/my-payslips` — Riwayat daftar slip gaji pegawai ESS.
  - `POST /api/payroll/runs/self-service/verify-pin` — Verifikasi autentikasi PIN sekunder 6-digit.
  - `GET /api/payroll/runs/self-service/payslips/{payslipId}/detail` — Rincian lembar slip gaji.
  - `GET /api/payroll/runs/self-service/payslips/{payslipId}/download-pdf` — Unduh dokumen PDF slip gaji resmi.

---

## 4. Contoh Skenario Rumah Sakit
1. **Skenario Konfirmasi Pembayaran & Pelepasan Slip Gaji**:
   - Finance mengirimkan callback konfirmasi pembayaran dengan referensi bank `TRX-MANDIRI-CORP-20261125-9988`.
   - Batch beralih ke status `Completed`. Sebanyak 320 slip gaji digital berstatus `Published` dan dapat diakses langsung oleh pegawai pada portal ESS.
2. **Skenario Penalti Lockout PIN 30 Menit**:
   - Perawat Ani mencoba membuka slip gajinya di ponsel, namun salah memasukkan PIN 3 kali berturut-turut.
   - Sistem merespons dengan HTTP 429 Too Many Requests: *"Akses menu slip gaji terkunci selama 30 menit karena 3 kali kesalahan PIN berturut-turut (VR-PAY-22, PAY-B6)."*
   - Setelah 30 menit berlalu, Ani memasukkan PIN tanggal lahir yang benar (`140595`), sistem menerbitkan token sesi, dan Ani dapat mengunduh berkas PDF slip gajinya.

---

## 5. Bukti Verifikasi Kompilasi
* Solusi: `NewQuilvianSystemBackend\QuilvianSystemBackend.csproj`
* Status Build: `Build succeeded. 0 Error(s)`


---

## Pengerjaan Ulang 10 November 2026 — Remediasi Gelombang 1 Audit Backend HR

| Field | Nilai |
| --- | --- |
| Dasar | [backend-unfinished-work.md](../../../roadmap/backend-unfinished-work.md) bagian 10 Gelombang 1, kelompok C — data penting hanya di memori |
| Task mode | `BACKEND` |
| Model | Claude Fable 5.1 |
| Commit backend saat dikerjakan | mulai `edfd843bac1c2860da7ae9a5ddcb68a75cf42666`; pemilik melakukan commit dan merge selama sesi (HEAD akhir `05325918`, branch `MHamzah`) |
| Status sesudah pengerjaan ulang | 🟡 **SEBAGIAN 10 November 2026** (dikerjakan ulang; klaim ✅ 9 November 2026 diturunkan) |

### Masalah yang ditemukan audit

Penghitung salah-PIN slip gaji, penguncian 30 menit, dan sesi gaji sensitif 15 menit disimpan di `ConcurrentDictionary` statis. Penyerang yang terkunci cukup menunggu aplikasi di-restart (atau permintaannya jatuh ke instance lain) untuk mencoba lagi; sesi pun hilang saat restart.

### Perubahan yang dikerjakan

- Tabel baru `HrdPayslipPinLockout` (satu baris per pegawai) dan `HrdPayslipPinSession` (hanya hash SHA-256 token yang disimpan; token mentah 32 byte acak kriptografis).
- Verifikasi PIN, penghitungan kegagalan, penguncian, penerbitan sesi, dan validasi sesi kini membaca/menulis database.

### Berkas yang berubah

- `Areas/Corporate/PayrollManagement/Services/PayrollPayslipDistributionService.cs` (folder dipindah oleh merge cabang lain pada sesi yang sama; namespace tidak berubah)
- `Areas/Corporate/HumanResource/PayrollManagement/Models/HrdPayslipPinLockout.cs`, `HrdPayslipPinSession.cs` (baru)
- `Repositories/Configurations/Corporate/HumanResource/PayrollManagement/HrdPayslipPinLockoutConfiguration.cs`, `HrdPayslipPinSessionConfiguration.cs` (baru)
- `Repositories/ApplicationDbContext.cs`
- `Migrations/20261110050000_AddHrdDurableAuditAndPinLockouts.cs`

### Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` **0 error**, 606 warning (garis dasar sebelum sesi 578; selisih berasal dari merge cabang lain yang masuk HEAD pada sesi yang sama, bukan dari berkas task ini) | 0 error | `PASS` |
| `Invoke-QbeConformanceCheck.ps1 -Mode Strict` atas 58 berkas yang disentuh: **0 VIOLATION, 0 REVIEW, PASS** | 0 temuan | `PASS` |
| Review diff dan scope | hanya berkas di atas yang disentuh untuk task ini | `PASS` |
| Uji runtime / API | tidak dijalankan pada sesi ini | `NOT RUN` |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

### Acceptance criteria yang disentuh pengerjaan ulang

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Penguncian 3x salah PIN selama 30 menit bertahan lintas restart | Terpenuhi di source | tabel `HrdPayslipPinLockout` |
| Sesi 15 menit bertahan lintas restart | Terpenuhi di source | tabel `HrdPayslipPinSession` |

### Yang belum terpenuhi

- Migration belum dijalankan ke database, sehingga perilaku runtime belum terbukti (`AT-HRD-PAY-20` `NOT RUN`).

### Database dan migration

Migration SQL idempoten `Migrations/20261110050000_AddHrdDurableAuditAndPinLockouts.cs` disiapkan; **belum dijalankan** ke database mana pun (wewenang pemilik, `dotnet ef database update`). Snapshot model EF tidak dipakai sebagai dasar migration ini karena sudah lama tidak sinkron dengan migration SQL manual slice sebelumnya (lihat catatan risiko).

### Catatan penutup

- Temuan lama yang tidak disentuh: PIN cadangan tetap `123456`, `000000`, `112233`, `654321` (komentar "fallback operasional/testing dev"). Ini kelemahan keamanan yang perlu keputusan pemilik.
- Perubahan sampingan: migration hasil scaffold otomatis `20261110044224_AddHrdDurableAuditAndPinLockouts` (14.620 baris, 79 CreateTable/26 DropTable di luar lingkup) **dibuang** dan diganti migration SQL manual; snapshot model yang ikut ter-commit pemilik dibiarkan karena sudah bercampur dengan merge cabang lain.
- Status Git akhir sesi: berkas task sudah masuk HEAD lewat commit pemilik; sisa di working tree adalah penghapusan dua berkas scaffold dan satu berkas migration manual baru (lihat `git status --short` pada ringkasan sesi).


## Pembaruan 11 November 2026 — Status Migration

Migration task ini sudah dijalankan ke database pengembangan `QuilvianNewDevHamzah` (`dotnet ef database update`, 0 migration tertunda). Pernyataan "migration belum dijalankan" pada bagian di atas sudah tidak berlaku. Uji penerimaan dengan akun sungguhan tetap `NOT RUN`, jadi statusnya tetap 🟡 bila bagian di atas menyatakan demikian. Rincian ada di `roadmap/backend-unfinished-work.md` bagian 14.
