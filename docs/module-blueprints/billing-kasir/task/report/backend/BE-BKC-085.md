# Laporan Perubahan Backend — `BE-BKC-085`

## Ringkasan untuk Pembaca Umum

Ketika seorang pasien berpindah dari Poliklinik Rawat Jalan ke Rawat Inap, biaya administrasi poliklinik yang telah dibayarkan pasien berhak dialihkan menjadi kredit pengurang biaya rawat inap (`BE-BKC-084`). Namun, dalam situasi tertentu—terutama ketika pasien dirawat inap menggunakan jaminan BPJS Kesehatan atau asuransi penuh di mana seluruh biaya kamar dan tindakan medis ditanggung 100% oleh penjamin sehingga tagihan porsi pasien adalah **Rp 0**—timbul pertanyaan penting bagi bagian keuangan rumah sakit:

*Ke mana larinya kelebihan biaya administrasi poliklinik yang sebelumnya sudah disetor pasien jika tidak ada tagihan rawat inap yang bisa dipotong?*

Prinsip keadilan finansial rumah sakit (`BKC-DEC-164`) menegaskan bahwa **hak pasien tidak boleh hilang**. Sisa kredit administrasi yang belum terserap porsi pasien di rawat inap wajib dialihkan menjadi hak pengembalian dana (*refundable credit*) saat tagihan rawat inap difinalisasi. Pasien atau keluarga dapat memproses pengembalian uang tersebut melalui Kasir dan Keuangan (*Finance*) sesuai tata cara refund yang berlaku di rumah sakit (`BKC-OQ-140`).

Sebelum task `BE-BKC-085`:
1. **Sisa Kredit Belum Dikonversi Menjadi Kredit Pengembalian Dana:**
   Sisa kredit administrasi poliklinik yang belum terserap porsi tagihan ranap hanya tersimpan sebagai angka residual di kolom `ReferredOutpatientAdminUnappliedCreditAmount` tabel `BilCalculationVersion`, tanpa pernah melahirkan entitas kredit pengembalian resmi (`BilRefundableCredit`) pada saat penagihan difinalisasi.
2. **Potensi Perhitungan Ganda pada Surat Kelayakan Pemulangan Pasien (*Inpatient Clearance*):**
   Pada pemeriksaan kelayakan pulang pasien rawat inap (`InpatientClearanceService.cs`), rumus evaluasi menghitung seluruh `BilRefundableCredit` sebagai pengurang sisa tagihan pasien (`outstanding`). Padahal, kredit administrasi rawat jalan telah diperhitungkan secara langsung sebagai pengurang kewajiban pasien (`PatientAmount`) pada versi kalkulasi. Bila kredit refund administrasi ini dihitung kembali pada rumus kelayakan, terjadi pengurangan dua kali (*double deduction*), yang dapat menyebabkan kalkulasi pelunasan pasien menjadi tidak akurat (`BIL-AT-210`).

---

### Solusi Penyempurnaan `BE-BKC-085`

Melalui implementasi task `BE-BKC-085` (mengacu pada arsitektur `BKC-DES-086`, keputusan `BKC-DEC-164`, dan matriks transisi `BIL-STATE-2.0` Bagian 2):

1. **Konversi Otomatis Sisa Kredit Menjadi Kredit Refund Saat Finalisasi (`RecognizeUnappliedReferredAdminCreditAsync`):**
   - Diimplementasikan di dalam alur inti finalisasi tagihan [`BillingFinalizationService.ExecuteInternalFinalizationAndClosureAsync`](file:///Users/yasminap/QuilvianV2/NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Billing/Services/BillingFinalizationService.cs#L718-L724).
   - Berjalan secara atomik di dalam transaksi pemanggil (*caller transaction*) tepat setelah status tagihan ditetapkan menjadi `FINAL` dan sebelum `SaveChangesAsync`.
   - Jika `ReferredOutpatientAdminUnappliedCreditAmount` pada versi kalkulasi yang difinalisasi lebih dari 0 (`> 0m`) dan belum ada baris kredit bertipe `REFERRED_OUTPATIENT_ADMIN` dengan `SourceId = invoice.Id`, sistem menerbitkan **tepat satu** entitas `BilRefundableCredit` baru berstatus `AVAILABLE`.
   - Mengeluarkan catatan audit resmi `BillingRefundableCredit.RecognizeReferredAdmin`.
2. **Perlindungan Idempotensi & Deteksi Perubahan Nilai (*Drift Audit*):**
   - Jika tagihan rawat inap sempat dibuka kembali (*reopened*) lalu difinalisasi ulang, sistem mendeteksi keberadaan kredit yang sudah ada.
   - Sesuai prinsip ketetapan finansial, kredit lama **tidak diubah** (`OriginalAmount` tetap abadi).
   - Apabila nominal sisa pada versi kalkulasi baru berbeda dengan kredit lama, sistem tidak mengubah saldo kredit melainkan mencatat peristiwa audit `BillingRefundableCredit.ReferredAdminDrift` agar dapat ditinjau oleh Bagian Keuangan (*Finance*) (`BIL-AT-209`).
3. **Pengecualian Kredit Administrasi pada Surat Kelayakan Pemulangan Pasien (`InpatientClearanceService.cs`):**
   - Pada [`InpatientClearanceService.cs`](file:///Users/yasminap/QuilvianV2/NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Billing/Services/InpatientClearanceService.cs#L102-L108), query penarikan `BilRefundableCredits` difilter secara eksplisit mengecualikan sumber `BillingRefundableCreditSourceTypes.ReferredOutpatientAdmin`.
   - Dengan pengecualian ini, sisa tagihan pasien (*outstanding balance*) pada evaluasi kelayakan pulang dihitung secara akurat tanpa pengurangan ganda (`BIL-AT-210`).

---

## Contoh Kasus Nyata di Rumah Sakit

### Skenario 1: Pasien BPJS Ny. Ratna (Kredit Administrasi Menjadi Refund Utuh) — `BIL-AT-209`
1. **Pemeriksaan Poliklinik Berbayar:**
   - Ny. Ratna awalnya mendaftar sebagai pasien umum di Poliklinik Penyakit Dalam dan membayar biaya pendaftaran/administrasi poliklinik sebesar Rp 50.000 di kasir (tagihan poli berstatus `CLOSED`).
2. **Masuk Rawat Inap dengan Jaminan BPJS Penuh:**
   - Dokter menyarankan Ny. Ratna untuk dirawat inap. Di admisi, keluarga mengurus kepesertaan BPJS Kesehatan sehingga seluruh biaya rawat inap dijamin 100% oleh BPJS (porsi pasien rawat inap = Rp 0).
3. **Kalkulasi Tagihan Rawat Inap:**
   - Sesuai aturan `BE-BKC-084`, porsi pasien ranap adalah Rp 0, sehingga kredit administrasi poli Rp 50.000 tidak terserap (`ReferredOutpatientAdminCreditAmount = Rp 0`).
   - Seluruh Rp 50.000 masuk ke sisa kredit: `ReferredOutpatientAdminUnappliedCreditAmount = Rp 50.000`. Porsi pasien tetap Rp 0 dan tidak menjadi negatif.
4. **Finalisasi Tagihan Rawat Inap (`FINAL`):**
   - Petugas Billing memfinalisasi tagihan rawat inap Ny. Ratna saat pasien bersiap pulang.
   - Sistem mendeteksi sisa kredit Rp 50.000 dan menerbitkan tepat satu `BilRefundableCredit` sebesar Rp 50.000 dengan tipe `REFERRED_OUTPATIENT_ADMIN` dan status `AVAILABLE`.
   - Sistem mencatat audit `BillingRefundableCredit.RecognizeReferredAdmin`.
   - Keluarga pasien dapat mendatangi kasir/Finance untuk mencairkan pengembalian uang Rp 50.000 tersebut sesuai alur refund resmi rumah sakit.
5. **Idempotensi Finalisasi Ulang:**
   - Jika tagihan rawat inap sempat dibuka kembali karena penyesuaian rekam medis lalu difinalisasi ulang, sistem tidak membuat baris kredit kedua (tetap tepat 1 baris kredit refund).

### Skenario 2: Evaluasi Surat Kelayakan Pemulangan Pasien (*Clearance*) Ny. Dewi — `BIL-AT-210`
1. **Kondisi Tagihan:**
   - Pasien Ny. Dewi memiliki tagihan rawat inap dengan porsi pasien Rp 1.000.000 setelah memperhitungkan kredit administrasi poli.
   - Ny. Dewi telah melakukan pembayaran tunai di kasir ranap sebesar Rp 1.000.000 (lunas).
   - Di sisi lain, Ny. Dewi juga memiliki sisa kredit refund administrasi poli `REFERRED_OUTPATIENT_ADMIN` sebesar Rp 50.000 yang berstatus `AVAILABLE`.
2. **Evaluasi Kelayakan Pulang:**
   - Tim rawat inap mencetak Surat Kelayakan Pemulangan Pasien (*Inpatient Clearance*).
   - Sistem menghitung:
     $$\text{Outstanding} = \text{Total Tanggung Jawab Pasien (Rp 1.000.000)} - \text{Pembayaran (Rp 1.000.000)} = \text{Rp 0}$$
   - Karena kredit `REFERRED_OUTPATIENT_ADMIN` telah dikecualikan dari `creditTotal` kelayakan, sistem tidak mengurangkan Rp 50.000 untuk kedua kalinya.
   - Status kelayakan terbit sebagai `CLEARED` secara sah, dan hak pengembalian dana Rp 50.000 milik pasien tetap aman di modul kasir/refund tanpa membingungkan petugas ruangan.

---

## Lembar Metadata Task

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-085` |
| Judul | Sisa kredit menjadi kredit refund saat finalisasi |
| Slice / Gelombang | Gelombang Eksekusi 1 (Revisi Blueprint 2.2 / 2.3) |
| Roadmap | `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` § `BE-BKC-085` |
| Trace | `FR-BKC-281`; `BKC-DEC-164`; `BKC-DES-086` |
| Kontrak | `BIL-STATE-2.0` Bagian 2; `BIL-PERMISSION-1.6` (audit `RecognizeReferredAdmin`, `ReferredAdminDrift`) |
| Acceptance Criteria | `BIL-AT-209` (porsi pasien Rp 0 → satu kredit refund Rp 150.000; finalisasi ulang tidak membuat baris kedua; drift tercatat audit); `BIL-AT-210` (kelayakan tidak menghitung dua kali) |
| Dependency | `BE-BKC-084` (selesai) |
| Klasifikasi | Sedang / Logika Finansial Pengembalian Dana & Kelayakan Pulang Rawat Inap |
| Task mode | `BACKEND` |
| Target berkas | `Areas/HealthServices/BillingManagement/Billing/Services/BillingFinalizationService.cs`<br/>`Areas/HealthServices/BillingManagement/Billing/Services/InpatientClearanceService.cs` |
| Tanggal Implementasi | 17 Oktober 2026 |
| Status | ✅ **SELESAI (Implementasi kode selesai & terverifikasi)** |

---

## Backend Governance Preflight

| Field Preflight | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `BillingManagement / Billing` |
| Submodule | `Billing` |
| Service Terkait | `BillingFinalizationService`, `InpatientClearanceService` |
| Prefix Registry | `Bil` — status `ACTIVE` pada registry arsitektur Quilvian |
| Applicability | `TOUCHED LEGACY` (Penyempurnaan alur finalisasi tagihan ranap dan evaluasi kelayakan pulang) |
| QBE Rules Berlaku | `QBE-CAL-001` (presisi nominal uang 18,2, batasan non-negatif), `QBE-SEC-001` (audit trail peristiwa finansial `RecognizeReferredAdmin` dan `ReferredAdminDrift`), `QBE-API-001` (idempotensi transaksi finalisasi) |
| Database Migration | **TIDAK ADA MIGRATION BARU** (Check constraint `CK_BilRefundableCredit_SourceType` yang menerima `'REFERRED_OUTPATIENT_ADMIN'` telah disiapkan pada `BE-BKC-083`). Tidak ada eksekusi database otomatis. |

---

## 1. Rincian Perubahan Source Code

### 1.1 Berkas yang Diubah

#### 1. [`BillingFinalizationService.cs`](file:///Users/yasminap/QuilvianV2/NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Billing/Services/BillingFinalizationService.cs)
- Pada metode inti finalisasi `ExecuteInternalFinalizationAndClosureAsync` (baris 720):
  Memanggil `RecognizeUnappliedReferredAdminCreditAsync` tepat setelah status invoice menjadi `FINAL` dan sebelum penyimpanan basis data.
- Menambahkan metode privat `RecognizeUnappliedReferredAdminCreditAsync`:
  ```csharp
  private async Task RecognizeUnappliedReferredAdminCreditAsync(
      BilInvoice invoice,
      BilCalculationVersion calculation,
      int calculationVersion,
      Guid actorUserId,
      DateTimeOffset now,
      CancellationToken cancellationToken)
  {
      if (calculation.ReferredOutpatientAdminUnappliedCreditAmount <= 0m)
          return;

      var existingCredit = await _dbContext.BilRefundableCredits
          .Where(c => c.SourceType == BillingRefundableCreditSourceTypes.ReferredOutpatientAdmin
                   && c.SourceId == invoice.Id
                   && !c.IsDelete)
          .FirstOrDefaultAsync(cancellationToken);

      if (existingCredit is null)
      {
          var credit = new BilRefundableCredit
          {
              Id = Guid.NewGuid(),
              InvoiceId = invoice.Id,
              SourceType = BillingRefundableCreditSourceTypes.ReferredOutpatientAdmin,
              SourceId = invoice.Id,
              OriginalAmount = calculation.ReferredOutpatientAdminUnappliedCreditAmount,
              AvailableAmount = calculation.ReferredOutpatientAdminUnappliedCreditAmount,
              Status = BillingRefundableCreditStatuses.Available,
              RecognizedAt = now,
              CreateDateTime = DateTime.UtcNow,
              CreateBy = actorUserId
          };

          _dbContext.BilRefundableCredits.Add(credit);

          await _loggerService.AuditAsync(
              LogCategory,
              "BillingRefundableCredit.RecognizeReferredAdmin",
              "Kredit refund admin Rawat Jalan lahir saat finalisasi.",
              new
              {
                  CreditId = credit.Id,
                  InpatientInvoiceId = invoice.Id,
                  Amount = credit.OriginalAmount,
                  CalculationVersionNo = calculationVersion,
                  ActorUserId = actorUserId
              });
      }
      else if (existingCredit.OriginalAmount != calculation.ReferredOutpatientAdminUnappliedCreditAmount)
      {
          await _loggerService.AuditAsync(
              LogCategory,
              "BillingRefundableCredit.ReferredAdminDrift",
              "Finalisasi ulang menghasilkan sisa kredit berbeda dari kredit yang sudah ada.",
              new
              {
                  CreditId = existingCredit.Id,
                  InpatientInvoiceId = invoice.Id,
                  ExistingAmount = existingCredit.OriginalAmount,
                  NewUnappliedAmount = calculation.ReferredOutpatientAdminUnappliedCreditAmount,
                  ActorUserId = actorUserId
              });
      }
  }
  ```

#### 2. [`InpatientClearanceService.cs`](file:///Users/yasminap/QuilvianV2/NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Billing/Services/InpatientClearanceService.cs)
- Pada baris 102–108 di metode `EvaluateClearanceAsync`:
  Menambahkan filter penyaringan `&& x.SourceType != BillingRefundableCreditSourceTypes.ReferredOutpatientAdmin` pada penarikan kredit refund.
  Hal ini mencegah kredit refund administrasi poli mengurangi tagihan pasien untuk kedua kalinya pada evaluasi surat kelayakan pemulangan.

---

## 2. Event Audit Finansial (`[Tags("Billing Audit")]`)

Task ini mendaftarkan dan menegakkan dua payload audit resmi pada logger sistem:

### 1. `BillingRefundableCredit.RecognizeReferredAdmin`
- **Pemicu:** Lahirnya kredit pengembalian dana biaya administrasi rawat jalan saat tagihan rawat inap pertama kali difinalisasi.
- **Struktur Payload:**
  ```json
  {
    "CreditId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "InpatientInvoiceId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "Amount": 150000.00,
    "CalculationVersionNo": 1,
    "ActorUserId": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d"
  }
  ```

### 2. `BillingRefundableCredit.ReferredAdminDrift`
- **Pemicu:** Finalisasi ulang pada invoice rawat inap menghasilkan nominal sisa kredit yang berbeda dari kredit yang sudah terlanjur dicatat sebelumnya.
- **Struktur Payload:**
  ```json
  {
    "CreditId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "InpatientInvoiceId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "ExistingAmount": 150000.00,
    "NewUnappliedAmount": 100000.00,
    "ActorUserId": "9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d"
  }
  ```

---

## 3. Matriks Pemenuhan Acceptance Criteria

| ID Pengujian | Deskripsi Skenario | Bukti Pemenuhan pada Kode | Status |
| :--- | :--- | :--- | :---: |
| `BIL-AT-209` | Rawat inap ditanggung penuh BPJS (porsi pasien Rp 0), lalu difinalisasi dua kali (finalisasi, buka kembali, finalisasi) | Finalisasi pertama: `ReferredOutpatientAdminUnappliedCreditAmount = 150.000` melahirkan **tepat satu** kredit `REFERRED_OUTPATIENT_ADMIN` Rp 150.000 dengan `SourceId = id invoice rawat inap`, status `AVAILABLE`, dan audit `RecognizeReferredAdmin`. Finalisasi kedua: mendeteksi kredit yang ada, tidak membuat baris kedua; bila nominal berbeda, mencatat audit `ReferredAdminDrift` tanpa mengubah kredit lama. | ✅ Terpenuhi |
| `BIL-AT-210` | Kredit `REFERRED_OUTPATIENT_ADMIN` sudah ada; kelayakan pulang dinilai | `InpatientClearanceService.cs:103-107` menyaring `x.SourceType != BillingRefundableCreditSourceTypes.ReferredOutpatientAdmin`. Kredit tidak ikut mengurangi `outstanding` untuk kedua kalinya (`outstanding = porsi pasien - pembayaran`). | ✅ Terpenuhi |

---

## 4. Catatan Koordinasi dengan Bagian Keuangan (*Finance*)

- **Pencairan Dana Refund:** Penerbitan entitas `BilRefundableCredit` bertipe `REFERRED_OUTPATIENT_ADMIN` **tidak** secara otomatis mentransfer uang ke rekening pasien. Kredit ini berstatus `AVAILABLE` dan siap diproses melalui pengajuan kasus pengembalian dana (*refund case*) oleh Kasir/Finance menggunakan alur persetujuan (*maker-checker*) yang sudah berjalan di modul Keuangan (`BKC-OQ-140`).
- **Penanganan Drift:** Jika tim Finance menemukan catatan audit `BillingRefundableCredit.ReferredAdminDrift`, hal tersebut menandakan bahwa tagihan rawat inap sempat dibuka kembali dan dilakukan penyesuaian perhitungan setelah kredit refund diterbitkan. Tim Finance dapat melakukan rekonsiliasi manual sebelum menyetujui pengembalian dana.
