# Laporan Perubahan Backend — `PAY-BE-11`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PAY-BE-11` |
| Judul | Penanda sumber konfirmasi pembayaran, serta status bayar gaji pada potongan piutang, ringkasan pembayaran, dan rekonsiliasi |
| Slice | Payroll mandiri — pembayaran dan potongan piutang |
| Roadmap | Tidak ada roadmap Payroll terdaftar; register status di `PAYROLL_V2_04_KONTRAK_DAN_TASK_IMPLEMENTASI.md` bagian 2 |
| Trace | `PAYROLL_V2_01` bagian 8.1 butir 2 dan 7 (keputusan pemilik 11 November 2026); `PAYROLL_V2_03` bagian 2.3; permintaan pemilik 11 November 2026 ("boleh kerjakan PAY-BE-11 dan PAY-FE-13") |
| Contract version | Penambahan field yang kompatibel mundur pada DTO yang sudah ada (`PAYROLL_V2_04` bagian 5). Tidak ada endpoint baru dan tidak ada kontrak ke Finance yang berubah |
| Dependency | `PAY-BE-06` (potongan piutang), `PAY-BE-08` (pembayaran), `PAY-BE-10` (rekonsiliasi). `PAY-FE-12` sudah mengirim kunci berawalan `MANUAL-` |
| Klasifikasi | `HEAVY` — 12 berkas, menyentuh pembayaran gaji dan rekonsiliasi keuangan, tetapi tanpa perubahan skema |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` branch `Yasmina`; laporan ini di `docs/module-blueprints/payroll-management/task/report/backend/` |
| Model | Claude Sonnet 5.5 |
| Commit backend saat dikerjakan | `f2dd20a6` ditambah perubahan lokal yang belum di-commit (dokumen BLM dan Payroll) |
| Tanggal | 11 November 2026 |
| Status | 🟡 **Source selesai dan lolos pemeriksaan statis; `dotnet build` menunggu pemilik.** Tanpa build, kompilasi C# dan terjemahan LINQ ke SQL belum terbukti. Tanpa migration |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `Corporate` / `PayrollManagement` |
| Pemilik/prefix registry | Modul belum memiliki baris registry sebagai pemilik (hanya tercatat sebagai submodule HR lama). **Tidak ada entity operasional baru**, sehingga `QBE-MOD-002` dan `QBE-MOD-003` tidak berlaku. Rencana pendaftaran `Prl` ada di `PAYROLL_V2_06` dan tidak disentuh task ini |
| Keberlakuan | `NEW CODE` untuk tiga berkas baru (konstanta dan pembaca status); `TOUCHED LEGACY` untuk sembilan berkas yang diubah |
| QBE ID yang berlaku | `QBE-MOD-001` (penempatan), `QBE-SVC-001` (akses data hanya lewat service), `QBE-API-001` dan `QBE-DTO-001` (DTO, bukan entity), `QBE-VAL-001` (validasi sumber), `QBE-PERM-001` (izin tidak berubah), `QBE-LOG-001` (aktor dan sumber pada audit), `QBE-AUD-001` |
| Migration / database | **Tidak ada migration, tidak ada perubahan skema, tidak ada eksekusi database.** Sumber konfirmasi tidak disimpan sebagai kolom; status bayar gaji diturunkan saat dibaca |
| Penamaan | Tidak ada nama `Trx*`, `Wfp*`, atau awalan baru pada tipe baru (`QBE-NAM-001`, `QBE-NAM-004` tidak terlanggar) |

## 1. Masalah yang diperbaiki

Pemilik memutuskan potongan piutang tetap diakui saat run **disahkan** (pilihan A), dengan satu tambahan: petugas harus bisa melihat apakah gaji penerima itu benar-benar sudah dibayar. Sebelumnya tidak ada cara melihatnya:

- Daftar potongan menampilkan status potongan (Terkonfirmasi, Direncanakan), bukan status pembayaran gaji penerimanya.
- Bila run disahkan lalu pembayaran gagal total, potongan sudah tercatat di Finance tetapi tidak ada tanda apa pun.
- Konfirmasi pembayaran manual (`PAY-FE-12`) dan hasil bank dari Finance memakai endpoint yang sama dan tidak bisa dibedakan kecuali lewat kunci idempotensi.

## 2. Proses bisnis

1. Run disahkan: potongan cicilan dikonfirmasi ke Finance dalam transaksi (tidak berubah).
2. Run dikirim ke Finance: instruksi pembayaran dibuat per penerima berstatus Dijadwalkan.
3. Hasil bayar masuk lewat ACK, sekarang dengan sumber `FINANCE_ACK` (bawaan) atau `MANUAL`.
4. Pada setiap pembacaan, sistem menurunkan **status pembayaran gaji** per penerima dari instruksinya (aturan di bagian 3.4) dan memakainya pada empat tempat: daftar potongan, rekonsiliasi potongan per periode, ringkasan pembayaran run, dan rekonsiliasi keuangan per run.

**Contoh.** Run `PR-2026-11` berisi dua penerima dengan potongan cicilan Rp 500.000 yang sudah dikonfirmasi. Ani sudah Berhasil Dibayar. Budi instruksinya Gagal. Hasilnya: daftar potongan menampilkan `LUNAS` untuk Ani dan `GAGAL` untuk Budi; ringkasan pembayaran menyatakan 1 potongan gagal; rekonsiliasi menambah pemeriksaan `AR_DEDUCTION_PAYMENT` berstatus `SELISIH` dengan baris "Budi: potongan Rp 500.000 sudah dikonfirmasi, tetapi pembayaran gaji gagal." Budi kemudian dikirim ulang dan menjadi `MENUNGGU_BAYAR`, lalu `LUNAS` setelah dikonfirmasi, dan pemeriksaan kembali `OK`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` dan `CLAUDE.md` backend, `BACKEND_ENGINEERING_CONTRACT.md`, registry QBE, `TEST_POLICY.md`; `PayrollPaymentService`, `PayrollRunService`, `PayrollEmployeeDeductionService`, `PayrollReportService`, controller pembayaran, laporan, dan potongan; DTO pembayaran, run, potongan, dan laporan; `PayrollStatusCatalog`, `PayrollExceptions`; model `TrxPayrollPayment`, `TrxPayrollVariableInput`, `TrxPayrollRunEmployee`, `TrxPayrollEmployeeComponent`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Constants/PayrollConfirmationSources.cs` | **Baru** — `FINANCE_ACK` (bawaan), `MANUAL`, dan `Normalize` |
| `Constants/PayrollSalaryPaymentStatuses.cs` | **Baru** — enam kode status bayar gaji dan tiga penentu perhatian |
| `Services/PayrollSalaryPaymentStatusResolver.cs` | **Baru** — aturan tunggal `Resolve`, pembaca `LoadAsync` (satu kueri), dan penghitung `CountConfirmedDeductionAttentionAsync` |
| `DTOs/PayrollPaymentDtos.cs` | `ConfirmationSource` pada permintaan dan respons ACK; komentar handoff diperbarui |
| `DTOs/PayrollRunDtos.cs` | Dua hitungan pada `PayrollRunPaymentSummaryResponse` |
| `DTOs/PayrollEmployeeDeductionDtos.cs` | `SalaryPaymentStatus` dan `SalaryPaymentNumber` pada item daftar; `SalaryPaymentStatus` pada item rekonsiliasi; dua hitungan pada ringkasan rekonsiliasi |
| `Services/PayrollPaymentService.cs` | Validasi dan pencatatan sumber pada `AcknowledgeAsync`; hitungan pada `GetPaymentsAsync`; komentar dan teks catatan Finance diperbarui |
| `Services/PayrollRunService.cs` | Dua hitungan pada ringkasan pembayaran detail run (lima baris) |
| `Services/PayrollEmployeeDeductionService.cs` | Status bayar pada daftar dan rekonsiliasi per periode |
| `Services/PayrollReportService.cs` | Pemeriksaan baru `AR_DEDUCTION_PAYMENT` pada rekonsiliasi per run |
| `Controllers/PayrollPaymentsController.cs` | Hanya komentar kelas yang usang |
| `Constants/PayrollStatusCatalog.cs` | Hanya komentar yang usang |

### 3.3 Dampak kontrak API, database, dan keamanan

**API.** Tanpa endpoint baru. Semua tambahan adalah field baru atau field opsional pada DTO yang sudah ada, jadi klien lama tidak terdampak:

| Endpoint | Tambahan |
| --- | --- |
| `POST .../payments/acknowledge` | Permintaan: `confirmationSource` (opsional, maks. 20, `FINANCE_ACK` atau `MANUAL`, tidak peka huruf besar-kecil). Respons: `confirmationSource` |
| `GET .../payments` dan `GET .../payroll-runs/{id}` | `summary.confirmedDeductionPaymentPendingCount`, `summary.confirmedDeductionPaymentFailedCount` (pada detail run: `paymentSummary.*`) |
| `GET .../employee-receivable-deductions` | Per item: `salaryPaymentStatus`, `salaryPaymentNumber` |
| `GET .../employee-receivable-deductions/summary` dan `POST .../reconcile` | Per item: `salaryPaymentStatus`; ringkasan: `confirmedPaymentPendingCount`, `confirmedPaymentFailedCount` |
| `GET .../reports/reconciliation` | Satu pemeriksaan baru `AR_DEDUCTION_PAYMENT` |

**Database.** Tidak ada. Status bayar gaji dibaca dari `TrxPayrollPayments` yang sudah ada.

**Keamanan.** Izin tidak berubah. Field baru mengikuti izin baca endpoint masing-masing. Pemeriksaan `AR_DEDUCTION_PAYMENT` memuat nominal potongan per penerima pada `Details`, tetapi endpoint rekonsiliasi itu sudah menuntut izin `PayrollReport : ViewAmount` (dibaca di `PayrollReportsController`), jadi tidak ada kebocoran baru. Field baru lainnya hanya berupa kode status dan hitungan, bukan nominal.

### 3.4 Aturan inti

**Sumber konfirmasi.** Kosong atau tidak diisi menjadi `FINANCE_ACK`. Nilai di luar dua yang dikenal ditolak dengan HTTP 400 dan pesan "Sumber konfirmasi tidak dikenal...", **sebelum** data apa pun dibaca atau diubah. Sumber dicatat pada payload audit `PayrollRun.PaymentAcknowledged` (selalu) dan pada catatan persetujuan run `PaymentAck` serta deskripsi audit (hanya bila `MANUAL`, supaya teks lama untuk Finance tidak berubah). Untuk item Paid hasil konfirmasi manual yang diterapkan, `Notes` instruksi diisi "Dikonfirmasi manual (bukan hasil bank)" bila masih kosong; itu tampil pada kolom status layar dan bertahan tanpa kolom baru. Perilaku item Failed dan seluruh perilaku `FINANCE_ACK` tidak berubah.

**Status bayar gaji per penerima** (`PayrollSalaryPaymentStatusResolver.Resolve`), berurutan:

| Urutan | Kondisi pada instruksi penerima | Status |
| ---: | --- | --- |
| 1 | Ada `Paid` atau `PartiallyReversed` | `LUNAS` |
| 2 | Ada `Scheduled` atau `Processing` | `MENUNGGU_BAYAR` |
| 3 | Ada `Reversed` | `DIBALIK` |
| 4 | Ada `Failed` | `GAGAL` |
| 5 | Tidak ada satu pun; nominal pembayaran nol atau kurang | `TIDAK_ADA_PEMBAYARAN` |
| 6 | Tidak ada satu pun; nominal pembayaran lebih dari nol | `BELUM_DIKIRIM` |

`Cancelled`, `Draft`, dan status lain diabaikan. Perhatian: `BELUM_DIKIRIM` dan `MENUNGGU_BAYAR` dihitung **menunggu**; `GAGAL` dan `DIBALIK` dihitung **gagal atau dibalik**.

**Pemeriksaan rekonsiliasi `AR_DEDUCTION_PAYMENT`.** Hanya menimbang potongan berstatus Applied (sudah dikonfirmasi):

| Keadaan | Status pemeriksaan |
| --- | --- |
| Tidak ada potongan terkonfirmasi | `INFO` |
| Ada yang `GAGAL` atau `DIBALIK` | `SELISIH` (menaikkan `MismatchCount` dan membuat `OverallStatus` menjadi `ADA_SELISIH`), dengan baris per penerima |
| Tidak ada yang gagal, ada yang `BELUM_DIKIRIM` atau `MENUNGGU_BAYAR` | `INFO` (menunggu, bukan selisih; pola sama dengan `INSTRUCTION_VS_PAID`) |
| Semua `LUNAS` atau `TIDAK_ADA_PEMBAYARAN` | `OK` |

`Expected` = total potongan terkonfirmasi (dari komponen slip, tidak dihitung ulang); `Actual` = total potongan pada penerima yang lunas atau tanpa pembayaran; `Difference` = `Actual` − `Expected`.

**Yang tidak berubah.** Titik pengakuan potongan (saat run disahkan), perhitungan, kontrak ke Finance, `DiscrepancyCount` dan kode temuan lama pada rekonsiliasi potongan (status bayar bukan selisih jadwal).

### 3.5 Selisih kecil dari rancangan awal

`PAYROLL_V2_01` bagian 8.1 menulis "item ACK diberi penanda sumber". Diimplementasikan pada **tingkat kiriman**: seluruh item dalam satu kiriman berasal dari sumber yang sama, sehingga lebih sederhana dan tidak memungkinkan satu kiriman bercampur. Kalimat di dokumen itu sudah disesuaikan.

## 4. Dokumentasi endpoint

Lihat `PAYROLL_V2_04` bagian 5, grup **Payment**, **Employee Receivable Deduction**, dan **Report** (diperbarui pada task ini).

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Keseimbangan kurung, kurawal, dan siku pada 12 berkas yang disentuh (pemeriksa statis di scratchpad, bukan bagian repo) | Seimbang pada semuanya | `PASS` (bukan kompilasi) | Keluaran skrip |
| Nama tipe baru unik di assembly (`PayrollConfirmationSources`, `PayrollSalaryPaymentStatuses`, `PayrollSalaryPaymentStatusResolver`, `PayrollSalaryPaymentState`) | Masing-masing satu definisi | `PASS` | Pencarian teks |
| Setiap anggota yang dipakai kode baru ada pada model dan `DbSet`-nya (`PaymentAmount`, `InputStatus`, `SourceType`, `SourceId`, `PayrollRunEmployee`, `PaymentNumber`, `Notes`, `Amount`, `ComponentCodeSnapshot`, `IsDelete`, `CreateDateTime`, `TrxPayrollPayments`, `TrxPayrollVariableInputs`, `TrxPayrollEmployeeComponents`) | Semua ada | `PASS` | Pencarian pada berkas model dan `ApplicationDbContext` |
| `using` pada berkas baru dan yang diubah | Lengkap (namespace `Constants`, `DTOs`, `Repositories`, `Microsoft.EntityFrameworkCore` sudah ada pada tiap berkas) | `PASS` (telaah) | Telaah diff |
| Penelusuran skenario fungsi murni `Resolve` lewat port Python (18 skenario) dan keputusan pemeriksaan `AR_DEDUCTION_PAYMENT` (7 skenario) | 25 dari 25 sesuai | `PASS` (penelusuran; **bukan eksekusi C#**) | Tabel di bawah |
| Test FE yang membaca source backend (`payroll-installment-deduction-guards`, `payroll-run-screens`, `payroll-module-contract`) dan test PAY-FE-12 | 57 lulus, 0 gagal | `PASS` | `node --test` |
| `dotnet restore` / `dotnet build` pada project aplikasi | **Tidak dijalankan oleh agen** (aturan pemilik: build dijalankan pemilik). **Menunggu pemilik** | `NOT RUN` | — |
| Perilaku runtime (kueri EF yang baru, terjemahan LINQ, pemanggilan endpoint) | Belum dijalankan | `NOT RUN` | — |
| Tes otomatis backend | Tidak ada project tes otomatis di backend (kebijakan repository); tidak dibuat | `NOT APPLICABLE` | `rules/backend/TEST_POLICY.md` |

**Penelusuran skenario `Resolve`** (nominal pembayaran 6.800.000 kecuali disebut):

| Skenario | Hasil | Perhatian |
| --- | --- | --- |
| Tiada instruksi | `BELUM_DIKIRIM` | menunggu |
| Tiada instruksi, nominal 0 | `TIDAK_ADA_PEMBAYARAN` | — |
| `Scheduled` | `MENUNGGU_BAYAR` | menunggu |
| `Processing` | `MENUNGGU_BAYAR` | menunggu |
| `Paid` | `LUNAS` | — |
| `PartiallyReversed` | `LUNAS` | — |
| `Failed` saja | `GAGAL` | gagal atau dibalik |
| `Failed` lalu `Scheduled` (kirim ulang) | `MENUNGGU_BAYAR` | menunggu |
| `Failed` dan `Paid` bersamaan | `LUNAS` | — |
| `Reversed` | `DIBALIK` | gagal atau dibalik |
| `Reversed` lalu `Scheduled` | `MENUNGGU_BAYAR` | menunggu |
| `Cancelled` saja, nominal lebih dari nol | `BELUM_DIKIRIM` | menunggu |
| `Cancelled` saja, nominal 0 | `TIDAK_ADA_PEMBAYARAN` | — |
| `Draft` saja, huruf kecil dan spasi (` paid `), status kosong, nominal negatif | `BELUM_DIKIRIM`, `LUNAS`, `BELUM_DIKIRIM`, `TIDAK_ADA_PEMBAYARAN` | sesuai aturan |

**Permintaan kepada pemilik.** Mohon jalankan `dotnet build` pada project aplikasi dan tempel keluarannya. Titik yang paling mungkin bermasalah saat kompilasi atau terjemahan LINQ: proyeksi anonim dengan `x.PayrollRunEmployee!.PaymentAmount` pada `CountConfirmedDeductionAttentionAsync`, kamus bertuple `(Name, PaymentAmount)` pada `BuildDeductionPaymentCheckAsync`, dan ekspresi `switch` berkoleksi pada `DeterminingPaymentStatuses`.

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

**Tidak dijalankan oleh agen:** semua perintah `dotnet`, eksekusi database, server dan API.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Sumber konfirmasi `FINANCE_ACK`/`MANUAL` divalidasi sebelum data berubah dan dicatat pada audit dan catatan persetujuan | Terpenuhi (source) | Bagian 3.4; diff `AcknowledgeAsync` |
| Perilaku `FINANCE_ACK` dan item Failed tidak berubah | Terpenuhi (review diff) | Hanya cabang `isManual` yang menambah perilaku; Notes dan komentar persetujuan manual saja |
| Status bayar gaji per penerima dengan aturan tunggal, dipakai empat tempat | Terpenuhi (source) | `PayrollSalaryPaymentStatusResolver` dipanggil dari empat tempat |
| Daftar dan ringkasan potongan membawa status bayar; ringkasan pembayaran run membawa dua hitungan pada kedua pembangunnya | Terpenuhi (source) | Bagian 3.2 dan 3.3 |
| Rekonsiliasi per run menambah satu pemeriksaan dengan aturan yang diminta | Terpenuhi (source, penelusuran) | Bagian 3.4 |
| Tanpa migration, tanpa endpoint baru, tanpa perubahan titik pengakuan potongan atau kontrak Finance | Terpenuhi | Daftar berkas; tidak ada berkas `Migrations/` berubah |
| Tanpa kebocoran nominal | Terpenuhi (review) | Bagian 3.3 |
| DoD: laporan tracked | Terpenuhi | Berkas ini |
| DoD: `dotnet build` | **Belum** | Menunggu pemilik |
| DoD: perilaku runtime | **Belum** | Menunggu build dan pengujian pemilik (potongan terkonfirmasi, pembayaran gagal, kirim ulang) |

Status akhir **🟡**: seluruh kriteria terpenuhi pada tingkat source dan penelusuran, tetapi kompilasi dan runtime belum terbukti.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kompilasi C# dan terjemahan LINQ ke SQL belum terbukti. Pemeriksa statis pernah meloloskan kesalahan yang baru ketahuan saat build (`CS0509` pada fase 1); jangan menganggapnya pengganti build |
| Keputusan implementasi | (1) Satu pembaca status untuk empat tempat supaya aturan tidak pernah berbeda. (2) Pemeriksaan rekonsiliasi memakai `SELISIH` hanya untuk gagal atau dibalik; belum lunas dihitung `INFO` agar run yang baru disahkan tidak langsung tampak bermasalah. (3) Hitungan pada ringkasan pembayaran berupa jumlah potongan, bukan penerima. (4) Pada detail run, hitungan hanya terisi setelah ada instruksi pembayaran (ringkasan itu memang `null` sebelum handoff), jadi sebelum handoff peringatan tidak muncul di sana |
| Risiko tersisa | (1) Kueri baru menambah satu pembacaan `TrxPayrollPayments` per pemanggilan daftar potongan, rekonsiliasi, ringkasan pembayaran, dan detail run; pada run besar (ribuan penerima) pantau waktu responsnya. (2) Konfirmasi manual tidak memeriksa apakah dana benar-benar masuk (keputusan pemilik; pemisahan tugas dijaga lewat Akses Role) |
| Masalah yang diketahui | Teks catatan sisi Finance (`FinanceConsumptionNote`) diperbarui; FE menampilkannya apa adanya pada dialog pengiriman |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | BE: `M` 9 berkas source, `??` 3 berkas baru (konstanta dan pembaca status), ditambah dokumen Payroll dan BLM yang belum di-commit. Tidak ada stage atau commit |
| Langkah berikutnya | Pemilik: `dotnet build`. Pasangan frontend `PAY-FE-13` (penanda dan peringatan) bergantung pada field baru di sini |
