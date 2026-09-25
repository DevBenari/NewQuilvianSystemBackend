# BE-BKC-075 — Service Evaluasi Kelayakan Finansial Rawat Inap & Auto-Reblock

## Ringkasan untuk Pembaca Umum

Dalam tata kelola operasional rumah sakit modern, proses pemulangan pasien rawat inap seringkali menjadi titik rawan terjadinya kebocoran pendapatan (*revenue leakage*) atau kesalahpahaman antara petugas bangsal rawat inap dan bagian kasir. Dua risiko finansial terbesar yang sering dihadapi adalah:

1. **Pasien Pulang Sebelum Menyelesaikan Kewajiban Finansial:**
   - Dokter telah memberikan izin pulang medis (*discharge order*), namun pasien langsung meninggalkan ruangan tanpa melunasi sisa tagihan di kasir karena perawat di bangsal tidak memiliki informasi akurat mengenai status pembayaran pasien.
   - Sebelumnya, modul Rawat Inap menentukan status kelayakan sendiri dari tabel lokal bangsal (`InpFinancialClearance`). Hal ini menciptakan risiko *split-brain* finansial dan inkonsistensi data.
   - **Solusi Single Source of Truth (`BKC-DEC-115`, `BKC-DES-045`):** Melalui implementasi **`BE-BKC-075`**, modul Billing ditetapkan sebagai satu-satunya otoritas mutlak (*Single Source of Truth*) yang berhak menerbitkan surat fakta kelayakan kepulangan pasien (`BilInpatientClearanceHandoff`). Bangsal rawat inap hanya bertindak sebagai pembaca/konsumen fakta resmi ini. Pasien hanya dapat diizinkan meninggalkan ranjang jika status clearance adalah **`CLEARED`** (sisa tagihan mandiri telah Rp 0).

2. **Tagihan Susulan Pasca-Izin Pulang Terbit (*Auto-Reblock Mechanism* — `BKC-DEC-116`, `BKC-DES-046`):**
   - Sering terjadi setelah keluarga pasien menyelesaikan administrasi di loket kasir dan surat izin pulang terbit (`CLEARED`), petugas ruangan atau farmasi baru menginput tindakan medis atau obat yang terlambat dicatat (*late charge*).
   - Tanpa mekanisme penguncian otomatis, tagihan susulan ini akan tertinggal dan menjadi piutang tak tertagih karena pasien sudah terlanjur pulang.
   - **Solusi Auto-Reblock:** Begitu ada tindakan atau obat baru yang dicatatkan pada invoice rawat inap berstatus `OPEN`, sistem Billing secara seketika dan atomik mencabut surat izin pulang sebelumnya menjadi **`REVOKED`** dengan alasan resmi `"LATE_CHARGE_POSTED"`, menaikkan nomor versi finansial (`FinancialVersion`), dan mengirim sinyal pemblokiran ulang ke bangsal. Layar pemulangan bangsal otomatis berubah merah (**`BLOCKED`**) sehingga perawat menahan kepulangan pasien sampai selisih biaya diselesaikan di loket kasir.
   - **Batas Kunci CLOSED (`BKC-DEC-120`, `BIL-VAL-127`):** Apabila invoice rawat inap telah resmi ditutup kasir (**`CLOSED`**), sistem secara mutlak menolak setiap pemasukan tagihan susulan baru demi menjaga integritas pembukuan, kecuali ada pembukaan kunci khusus (*reopen*) oleh Supervisor Kasir.

3. **Perlindungan Pasien Tindakan Bedah/Operasi Besar (Deposit 100% Ekses — `BKC-DEC-114`, `BKC-DES-047`):**
   - Untuk tindakan medis besar atau operasi terencana, rumah sakit mewajibkan setoran uang muka (*deposit*).
   - Sistem secara adil menghitung kewajiban deposit **hanya dari porsi tanggung jawab pasien (ekses / *patient excess*)**, bukan dari total biaya kotor tindakan. Pasien yang memiliki penjamin asuransi/perusahaan tidak dipaksa membayar deposit atas porsi biaya yang sudah dijamin pihak ketiga (`BIL-VAL-121`).

---

### Contoh Kasus Nyata di Rumah Sakit

* **Contoh 1 (Pasien Rawat Inap Lunas Standar — `BIL-VAL-122`):**
  Tn. Hendra selesai dirawat 3 hari di Kamar Kelas 2. Total tagihan pribadi pasien adalah Rp 3.500.000.
  - Keluarga pasien membayar lunas Rp 3.500.000 via kartu debit di kasir rawat inap.
  - Sistem Billing langsung menerbitkan `BilInpatientClearanceHandoff` dengan status **`CLEARED`**, `FinancialOutcome = "FULLY_PAID"`, `OutstandingBalance = 0.00`, dan `FinancialVersion = 1`.
  - Layar monitor perawat di bangsal otomatis menampilkan status hijau "Layak Pulang", dan perawat dengan aman menyerahkan surat kontrol serta mengizinkan Tn. Hendra pulang.

* **Contoh 2 (Pasien Masih Memiliki Sisa Tagihan Mandiri — `BIL-VAL-122`):**
  Ibu Ratna memiliki total biaya rawat inap Rp 15.000.000. Asuransi menanggung Rp 14.000.000, sehingga porsi ekses pasien adalah Rp 1.000.000.
  - Perawat mengirim permintaan evaluasi kepulangan sebelum keluarga ke loket kasir.
  - Sistem Billing menerbitkan status **`BLOCKED`** dengan rincian kendala (*blocker reason*): `"Pasien masih memiliki sisa tanggung jawab mandiri (patient excess) sebesar Rp 1.000.000 yang belum dilunasi di kasir utama"`.
  - Pasien tertahan di bangsal dan diarahkan menyelesaikan selisih Rp 1.000.000 terlebih dahulu di kasir.

* **Contoh 3 (Pencegahan Kebocoran Biaya Lewat Auto-Reblock — `BIL-VAL-123`):**
  Ny. Siti telah lunas di kasir pada pukul 10.00 WIB dan memperoleh status **`CLEARED`** (versi finansial 1).
  - Pukul 10.15 WIB, sebelum pasien sempat keluar kamar, perawat bangsal baru teringat memasukkan biaya pemakaian tabung oksigen tambahan sebesar Rp 150.000 pada invoice berjalan yang masih `OPEN`.
  - Sistem Billing seketika menjalankan **Auto-Reblock**:
    - Surat kelayakan versi 1 dicabut.
    - Terbit surat kelayakan versi 2 berstatus **`REVOKED`** dengan alasan `"LATE_CHARGE_POSTED"`.
    - Indikator layar bangsal seketika berubah merah "Tertahan / Dicabut".
  - Perawat segera menginformasikan keluarga bahwa ada tagihan oksigen susulan sebesar Rp 150.000 yang perlu dilunasi di kasir. Rumah sakit terhindar dari kerugian finansial.

* **Contoh 4 (Penolakan Tagihan Susulan Pasca-Invoice Ditutup — `BIL-VAL-127`):**
  Pasien Tn. Wahyu telah lunas dan kasir telah resmi menutup invoice (**`CLOSED`**) pada pukul 12.00 WIB.
  - Pukul 14.00 WIB, petugas farmasi mencoba menginput obat susulan ke invoice Tn. Wahyu.
  - Sistem menolak transaksi secara otomatis dengan pesan galat: `"Tagihan susulan ditolak karena invoice telah ditutup. Pembukaan kembali memerlukan persetujuan Supervisor Kasir."`

* **Contoh 5 (Deposit Tindakan Operasi Besar Berdasarkan Ekses Pasien — `BIL-VAL-121`):**
  Ananda Rizky dijadwalkan menjalani operasi bedah dengan estimasi biaya Rp 40.000.000.
  - Penjamin asuransi menanggung 75% (Rp 30.000.000), sehingga porsi tanggung jawab orang tua pasien adalah 25% (Rp 10.000.000).
  - Jika orang tua baru menyetor deposit sebesar Rp 6.000.000, sistem menolak izin tindakan dengan pesan validasi `BIL-VAL-121`: `"Saldo deposit belum memenuhi 100% porsi tanggung jawab pasien untuk tindakan besar. Pasien/keluarga wajib menyetor kekurangan deposit sebesar Rp 4.000.000"`.
  - Orang tua tidak dipaksa menyetor deposit Rp 40.000.000 (biaya kotor), melainkan hanya sebesar ekses riil Rp 10.000.000.

---

## Lembar Metadata Task

- TASK ID: BE-BKC-075
- TASK TYPE: Domain Clearance Service, Handoff Publishing, & Auto-Reblock Engine (`InpatientClearanceService`, `BilConsumerHandoffService`, `BillingSettlementService`, `BillingInvoiceService`)
- COMPLEXITY: HIGH
- CLASSIFICATION SCORE: 4 (repo 0 + berkas diperiksa ≤8 → 0 + berkas diubah/dibuat 6 → 1 + logika advisory lock transaksi, state transition clearance berversi monoton, validasi ekses deposit tindakan besar, dan Auto-Reblock atomik → 3; total score 4)
- MODEL: Gemini 3.8 Flash (High)
- TASK MODE: BACKEND
- WRITE TARGET: `NewQuilvianSystemBackend` — `Areas/HealthServices/BillingManagement/Billing/**` (source); `docs/module-blueprints/billing-kasir/task/report/backend/**`, `roadmap/**`, `requirement-traceability.md` (laporan & registry)
- FILES INSPECTED:
  - `docs/module-blueprints/billing-kasir/02-backend-architecture.md` (`BKC-DES-045`, `BKC-DES-046`, `BKC-DES-047`)
  - `docs/module-blueprints/billing-kasir/contracts/state-transition-matrix.md` (`BIL-STATE-1.3`)
  - `docs/module-blueprints/billing-kasir/contracts/validation-matrix.md` (`BIL-VAL-121`, `BIL-VAL-122`, `BIL-VAL-123`, `BIL-VAL-127`)
  - `docs/module-blueprints/billing-kasir/flowcharts/06-integrasi-rawat-inap.md`
  - `Areas/HealthServices/BillingManagement/Billing/Models/BilInpatientClearanceHandoff.cs`
  - `Areas/HealthServices/BillingManagement/Billing/Services/BilConsumerHandoffService.cs`
  - `Areas/HealthServices/BillingManagement/Billing/Services/BillingSettlementService.cs`
  - `Areas/HealthServices/BillingManagement/Billing/Services/BillingInvoiceService.cs`
- FILES CHANGED / CREATED:
  - **Dibuat**: `Areas/HealthServices/BillingManagement/Billing/Dtos/InpatientClearanceDtos.cs`
    - Mendefinisikan DTO: `EvaluateInpatientClearanceRequest`, `ReevaluateInpatientClearanceRequest`, `InpatientClearanceHandoffResponse`, `MajorProcedureDepositValidationRequest`, `MajorProcedureDepositValidationResult`, `InpatientBillingSummaryResponse`, `AcknowledgeInpatientClearanceRequest`.
  - **Dibuat**: `Areas/HealthServices/BillingManagement/Billing/Services/IInpatientClearanceService.cs`
    - Kontrak antarmuka: `EvaluateClearanceAsync`, `ValidateMajorProcedureDepositAsync`, `TriggerAutoReblockIfApplicableAsync`, `AcknowledgeClearanceHandoffAsync`, `GetLatestClearanceForEncounterAsync`, `GetInpatientBillingSummaryAsync`.
  - **Dibuat**: `Areas/HealthServices/BillingManagement/Billing/Services/InpatientClearanceService.cs`
    - Implementasi Single Source of Truth evaluasi kelayakan finansial rawat inap.
    - Evaluasi saldo sisa tagihan pasien (`outstandingBalance`) dan deposit tindakan besar.
    - Penentuan status kelayakan (`CLEARED`, `BLOCKED`, `REVOKED`, `PENDING`) dan hasil finansial (`FULLY_PAID`, `INSURANCE_GUARANTEED`, `SETTLED_WITH_DEPOSIT`, `DISCHARGED_WITH_AR`).
    - Penjaminan nomor `FinancialVersion` yang naik monoton per encounter.
    - Pengambilan *advisory lock* PostgreSQL (`$"BIL_INPATIENT_CLEARANCE_{encounterId:N}"`) untuk proteksi konkurensi.
  - **Diperbarui**: `Areas/HealthServices/BillingManagement/Billing/Dtos/BillingConsumerHandoffDtos.cs`
    - Mendaftarkan konstanta `Inpatient = "INPATIENT"` pada `BillingHandoffTypes` dan `BillingHandoffTargetModules`.
  - **Diperbarui**: `Areas/HealthServices/BillingManagement/Billing/Services/BilConsumerHandoffService.cs`
    - Mengintegrasikan `IInpatientClearanceService` untuk penerbitan handoff kelayakan ranap (`PublishForInpatientClearanceAsync`).
    - Menambahkan metode penegakan `TriggerInpatientAutoReblockIfApplicableAsync`.
    - Memperluas `GetPendingHandoffsAsync` untuk mendukung penyaringan dan paginasi handoff rawat inap.
    - Memperluas `AcknowledgeHandoffAsync` untuk mendukung pengakuan surat kelayakan ranap secara idempoten.
  - **Diperbarui**: `Areas/HealthServices/BillingManagement/Billing/Services/BillingSettlementService.cs`
    - Menghubungkan hook transaksi pelunasan kasir atau pembalikan pembayaran tender ke `PublishForInpatientClearanceAsync` di dalam transaksi atomik yang sama.
  - **Diperbarui**: `Areas/HealthServices/BillingManagement/Billing/Services/BillingInvoiceService.cs`
    - Menegakkan `BIL-VAL-127`: Penolakan mutlak tagihan susulan ketika status invoice telah `CLOSED`.
    - Menegakkan `BIL-VAL-123` & `BKC-DEC-116`: Pemicuan Auto-Reblock seketika saat tagihan baru atau peningkatan kuantitas/harga masuk pada invoice ranap yang sebelumnya berstatus `CLEARED`.
  - **Diperbarui**: `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs`
    - Mendaftarkan `IInpatientClearanceService` dan `InpatientClearanceService` ke container Dependency Injection.

---

## Alur Transisi Status & Arsitektur Auto-Reblock

```
               [ Permintaan Evaluasi / Admisi Baru ]
                                 │
                                 ▼
                     [ Apakah Tagihan Ada? ]
                                 │
                ┌────────────────┴────────────────┐
                ▼                                 ▼
             [ Tidak ]                          [ Ya ]
                │                                 │
                ▼                                 ▼
       [ Status: PENDING ]           [ Hitung Sisa Tagihan ]
                                                  │
                                 ┌────────────────┴────────────────┐
                                 ▼                                 ▼
                       [ Sisa Tagihan > 0 ]              [ Sisa Tagihan <= 0 ]
                                 │                                 │
                                 ▼                                 ▼
                        [ Status: BLOCKED ]               [ Status: CLEARED ]
                                 │                                 │
                                 │                ┌────────────────┴────────────────┐
                                 │                ▼                                 ▼
                                 │       [ Tagihan Baru Masuk ]            [ Pasien Pulang ]
                                 │          (Invoice OPEN)                         │
                                 │                │                                ▼
                                 │                ▼                            [ SELESAI ]
                                 │        [ AUTO-REBLOCK ]
                                 │      [ Status: REVOKED ]
                                 │    [ LATE_CHARGE_POSTED ]
                                 │                │
                                 └────────────────┘
                                          │
                                          ▼
                               [ Pelunasan di Kasir ]
                                          │
                                          ▼
                          [ Status Kembali: CLEARED ]
                         [ FinancialVersion Naik +1 ]
```

---

## Verifikasi & Kepatuhan Arsitektur

| Pemeriksaan / Uji | Hasil | Status | Catatan Bukti |
| :--- | :--- | :--- | :--- |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1` | **PASS** | VERIFIED | Mode Strict, 25 berkas working tree dievaluasi, 0 pelanggaran (`VIOLATION: 0`, `REVIEW: 0`, `INFO: 0`) |
| `dotnet build` | **Menunggu eksekusi mandiri pengguna** | NOT VERIFIED | Sesuai instruksi eksplisit pengguna: *"Akan tetapi, jangan jalankan build secara automatis"* |
| Billing Single Source of Truth (`BKC-DEC-115`) | Selesai | VERIFIED (Inspeksi Kode) | `InpatientClearanceService.cs` bertindak sebagai penerbit tunggal status `BilInpatientClearanceHandoff` dengan versi finansial monoton naik |
| Validasi Deposit Tindakan Besar 100% Ekses (`BIL-VAL-121`) | Selesai | VERIFIED (Inspeksi Kode) | `ValidateMajorProcedureDepositAsync` menghitung ekses pasien terhadap jaminan asuransi dan membandingkan saldo deposit akun ranap |
| Evaluasi Sisa Tagihan (`BIL-VAL-122`) | Selesai | VERIFIED (Inspeksi Kode) | Tagihan bersisa menghasilkan `BLOCKED`, tagihan lunas menghasilkan `CLEARED` dengan penentuan `FinancialOutcome` yang tepat |
| Penegakan Auto-Reblock Tagihan Susulan (`BIL-VAL-123`) | Selesai | VERIFIED (Inspeksi Kode) | Pemasukan charge baru pada invoice ranap `OPEN` yang telah `CLEARED` otomatis mencabut status menjadi `REVOKED` (`LATE_CHARGE_POSTED`) |
| Penolakan Tagihan Pasca-CLOSED (`BIL-VAL-127`) | Selesai | VERIFIED (Inspeksi Kode) | `BillingInvoiceService.UpsertChargeAsync` melempar galat penolakan mutlak saat invoice `CLOSED` |
| Idempotensi Pengakuan Handoff (`BIL-VAL-116`, `BIL-VAL-125`) | Selesai | VERIFIED (Inspeksi Kode) | Pengakuan surat berstatus `ACKNOWLEDGED` mengembalikan data tanpa modifikasi ganda |

---

## Status Task Selanjutnya

- `BE-BKC-076` (API Controller Integrasi Ranap untuk Inquiry, Kalkulasi Kamar, Reevaluasi, & Acknowledge)
