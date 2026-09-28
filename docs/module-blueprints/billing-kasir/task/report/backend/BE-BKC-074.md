# BE-BKC-074 — Layanan Biaya Administrasi Ranap (7% Cap Rp6jt) & Penggantian Admin Rajal

## Ringkasan untuk Pembaca Umum

Dalam tata kelola keuangan rumah sakit, biaya administrasi pelayanan rawat inap kerap menimbulkan pertanyaan dan potensi sengketa dengan keluarga pasien atau pihak penjamin (asuransi/BPJS) apabila aturannya tidak transparan, kaku, atau memicu penagihan ganda (*double charging*). Dua persoalan mendasar yang diselesaikan melalui implementasi task **BE-BKC-074** ini adalah:

1. **Perhitungan Biaya Administrasi Rawat Inap Berkeadilan (7% dengan Pagu Maksimal Rp 6.000.000):**
   - Biaya administrasi rawat inap dihitung secara deklaratif dari kebijakan master data rumah sakit (`MstAdministrationFeePolicy`), bukan melalui angka statis (*hardcoded*) di dalam kode program.
   - Kebijakan menetapkan persentase sebesar **7%** dari dasar tagihan yang memenuhi syarat (*eligible base amount* — meliputi biaya sewa kamar, visite dokter, tindakan medis/keperawatan, laboratorium, dan radiologi, dengan mengecualikan obat/farmasi).
   - Untuk melindungi pasien dari tagihan yang membengkak tanpa batas pada tindakan medis besar (misalnya operasi jantung atau rawat intensif berbiaya ratusan juta rupiah), sistem memberlakukan pagu maksimal (*cap*) sebesar **Rp 6.000.000**. Berapapun total tagihan pasien di atas batas tersebut, biaya administrasi tidak akan melebihi Rp 6.000.000.
   - **Evaluasi Tanggal Kepulangan (*Discharge Date*):** Kebijakan tarif administrasi baru ini dievaluasi secara adil berdasarkan tanggal pasien keluar/pulang (*Discharge Date*), bukan tanggal pertama kali pasien mendaftar di rumah sakit.
   - **Pengecualian Pasien Penjamin Sistem Paket (BPJS Kesehatan / INA-CBGs):** Pada pasien dengan penjamin sistem paket, biaya administrasi tetap dicatat secara kotor untuk keperluan transparansi akuntansi rumah sakit, namun **porsi tanggung jawab pasien disetel tepat Rp 0**. Pasien BPJS tidak boleh dibebani biaya administrasi sepeser pun secara langsung.

2. **Perlindungan Anti-Double Charging Alihan Rawat Jalan ke Rawat Inap:**
   - Ketika pasien berobat ke Poliklinik (Rawat Jalan) lalu dokter memutuskan pasien harus segera opname (Rawat Inap), biaya administrasi rawat jalan yang sebelumnya timbul tidak boleh menjadi beban ganda bagi pasien.
   - **Kondisi A (Biaya Admin Rajal Belum Dibayar):** Baris tagihan biaya administrasi rawat jalan otomatis dibatalkan (*void*) dengan alasan resmi `"SUPERSEDED_BY_INPATIENT_ADMISSION"`.
   - **Kondisi B (Biaya Admin Rajal Sudah Terlanjur Dibayar di Poli):** Dana yang sudah dibayarkan pasien di kasir rawat jalan (misalnya Rp 50.000) tidak hangus dan tidak perlu melalui birokrasi pengembalian uang manual. Sistem secara otomatis mengonversinya menjadi kredit pembayaran (*Refundable Credit* bertipe `"REFERRED_OUTPATIENT_ADMIN"`) pada invoice rawat inap, sehingga langsung memotong sisa saldo yang harus dibayar pasien saat kepulangan rawat inap.

---

### Contoh Kasus Nyata di Rumah Sakit

* **Contoh 1 (Pasien Rawat Inap Standar — `BKC-DEC-113`, `BIL-VAL-120`):**
  Tn. Rahman dirawat inap selama 4 hari dengan total tagihan tindakan dan kamar yang memenuhi syarat sebesar **Rp 10.000.000**.
  - Perhitungan 7%: $7\% \times \text{Rp } 10.000.000 = \text{Rp } 700.000$.
  - Karena Rp 700.000 masih di bawah pagu Rp 6.000.000, maka biaya administrasi yang dibebankan pada invoice Tn. Rahman adalah tepat **Rp 700.000**.

* **Contoh 2 (Pasien Tindakan Bedah Besar Mencapai Pagu — `BKC-DEC-113`, `BIL-VAL-120`):**
  Ibu Maria menjalani operasi besar dengan total tagihan ranap yang memenuhi syarat sebesar **Rp 100.000.000**.
  - Perhitungan persentase murni: $7\% \times \text{Rp } 100.000.000 = \text{Rp } 7.000.000$.
  - Sistem mendeteksi bahwa hasil perhitungan melampaui pagu batas atas (*Cap Amount*) sebesar Rp 6.000.000.
  - Sistem mengunci biaya administrasi tepat pada pagu: **Rp 6.000.000** (hemat Rp 1.000.000 untuk pasien). Penanda `IsCapApplied = true`.

* **Contoh 3 (Pasien Peserta BPJS Kesehatan / Paket INA-CBGs — `BKC-DEC-121`, `BKC-AC-089`):**
  Tn. Slamet dirawat inap menggunakan penjamin BPJS Kesehatan dengan total dasar tagihan eligible Rp 20.000.000.
  - Perhitungan bruto: $7\% \times \text{Rp } 20.000.000 = \text{Rp } 1.400.000$.
  - Karena penjamin adalah BPJS Kesehatan (tipe paket), sistem secara otomatis mengalokasikan nilai beban pasien sebesar **Rp 0** (`PatientResponsibility = 0` dan `NonBillableResidualAmount = Rp 1.400.000`). Pasien tidak diminta membayar uang sepeser pun.

* **Contoh 4 (Pasien Alihan Poli ke Ranap — Admin Rajal Sudah Dibayar — `BKC-DEC-119`, `BIL-VAL-126`):**
  Ananda Farhan pagi hari memeriksakan diri di Poli Anak dan membayar biaya karcis/admin rajal sebesar **Rp 50.000** di kasir rawat jalan. Siang harinya dokter anak menginstruksikan opname karena demam tinggi berdarah.
  - Saat invoice rawat inap diproses, sistem menelusuri kunjungan rujukan sebelumnya.
  - Sistem menemukan adanya biaya admin rajal Rp 50.000 yang sudah lunas.
  - Sistem menerbitkan `BilRefundableCredit` bertipe `"REFERRED_OUTPATIENT_ADMIN"` senilai **Rp 50.000** yang langsung memotong sisa tagihan ranap Farhan. Pasien tidak dirugikan dan tidak terjadi penagihan ganda.

* **Contoh 5 (Pasien Alihan Poli ke Ranap — Admin Rajal Belum Dibayar — `BKC-DEC-119`, `BIL-VAL-126`):**
  Pasien langsung diantar dari IGD/Poli ke bangsal ranap sebelum sempat membayar biaya karcis poli Rp 30.000.
  - Sistem secara idempoten menandai baris tagihan admin rajal tersebut sebagai `IsVoid = true` dengan catatan resmi `"SUPERSEDED_BY_INPATIENT_ADMISSION"`.
  - Pasien hanya membayar biaya administrasi rawat inap satu kali saja.

---

## Lembar Metadata Task

- TASK ID: BE-BKC-074
- TASK TYPE: Domain Service & Financial Business Rules Implementation (`AdministrationFeeCalculationService` & `BillingCalculationService`)
- COMPLEXITY: MEDIUM
- CLASSIFICATION SCORE: 3 (repo 0 + berkas diperiksa ≤8 → 0 + berkas diubah/dibuat 6 → 1 + logika kalkulasi ber-cap deklaratif, proteksi paket penjamin BPJS, rekonsiliasi alihan rajal anti-double-charging → 2; total score 3)
- MODEL: Gemini 3.8 Flash (High)
- TASK MODE: BACKEND
- WRITE TARGET: `NewQuilvianSystemBackend` — `Areas/HealthServices/BillingManagement/Billing/**` (source); `docs/module-blueprints/billing-kasir/task/report/backend/**`, `roadmap/**`, `requirement-traceability.md` (laporan & registry)
- FILES INSPECTED:
  - `docs/module-blueprints/billing-kasir/02-backend-architecture.md` (`BKC-DES-044`, `BKC-DES-049`)
  - `docs/module-blueprints/billing-kasir/contracts/validation-matrix.md` (`BIL-VAL-120`, `BIL-VAL-126`)
  - `docs/module-blueprints/billing-kasir/flowcharts/06-integrasi-rawat-inap.md`
  - `Areas/HealthServices/BillingManagement/MasterData/Models/MstAdministrationFeePolicy.cs`
  - `Areas/HealthServices/BillingManagement/Billing/Models/BilRefundableCredit.cs`
  - `Areas/HealthServices/BillingManagement/Billing/Models/BilInvoice.cs`
  - `Areas/HealthServices/BillingManagement/Billing/Services/BillingCalculationService.cs`
- FILES CHANGED / CREATED:
  - **Diperbarui**: `Areas/HealthServices/BillingManagement/Billing/Models/BilRefundableCredit.cs`
    - Menambahkan tipe sumber kredit baru `BillingRefundableCreditSourceTypes.ReferredOutpatientAdmin = "REFERRED_OUTPATIENT_ADMIN"`.
  - **Dibuat**: `Areas/HealthServices/BillingManagement/Billing/DTOs/AdministrationFeeCalculationDtos.cs`
    - Mendefinisikan DTO kalkulasi administrasi: `AdminFeeCalculationRequest`, `AdminFeeCalculationResult`, `ReconcileReferredOutpatientAdminRequest`, `ReconcileReferredOutpatientAdminResult`.
  - **Dibuat**: `Areas/HealthServices/BillingManagement/Billing/Services/IAdministrationFeeCalculationService.cs`
    - Kontrak antarmuka: `CalculateAdministrationFeeAsync` dan `ReconcileReferredOutpatientAdminFeeAsync`.
  - **Dibuat**: `Areas/HealthServices/BillingManagement/Billing/Services/AdministrationFeeCalculationService.cs`
    - Mesin kalkulasi administrasi ranap deklaratif (evaluasi tanggal discharge vs `EffectiveFrom`, kalkulasi 7% ber-cap Rp 6.000.000, pembebasan beban pasien penjamin paket BPJS).
    - Mesin rekonsiliasi alihan rajal ke ranap: pembatalan *void* jika belum dibayar, penerbitan kredit pemotong tagihan jika sudah dibayar di poli secara idempoten.
  - **Diperbarui**: `Areas/HealthServices/BillingManagement/Billing/Dtos/BillingInvoiceDtos.cs`
    - Memperkaya `AdministrationFeeCalculationResponse` dengan metadata kalkulasi: `CalculationType`, `Percentage`, `CapAmount`, `EligibleBaseAmount`, `RawCalculatedAmount`, `IsCapApplied`, `IsPackageGuaranteed`, `ReferredOutpatientAdminVoided`, `ReferredOutpatientAdminCreditedAmount`.
  - **Diperbarui**: `Areas/HealthServices/BillingManagement/Billing/Services/BillingCalculationService.cs`
    - Mengintegrasikan `IAdministrationFeeCalculationService` ke alur kalkulasi invoice ranap utama: menghitung dasar eligible non-farmasi, mengevaluasi tanggal discharge, dan membebaskan porsi pasien BPJS.
  - **Diperbarui**: `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs`
    - Mendaftarkan `IAdministrationFeeCalculationService` dan `AdministrationFeeCalculationService` ke Dependency Injection container.

---

## Alur Proses Bisnis & Logika Algoritma

```
                              [ Mulai Kalkulasi Invoice Ranap ]
                                              │
                                              ▼
                        [ Ambil Seluruh Item Tagihan Aktif ]
                                              │
                                              ▼
               [ Filter Item Non-Farmasi: Kamar, Tindakan, Visite, Lab, Rad ]
                                              │
                                              ▼
                           [ Hitung Eligible Base Amount (EBA) ]
                                              │
                                              ▼
                      [ Cari Kebijakan Aktif MstAdministrationFeePolicy ]
               [ Syarat: PolicyType == 'INPATIENT' & DischargeDate >= EffectiveFrom ]
                                              │
                      ┌───────────────────────┴───────────────────────┐
                      ▼                                               ▼
             [ Kebijakan Ditemukan ]                        [ Kebijakan Tidak Ditemukan ]
                      │                                               │
                      ▼                                               ▼
         [ Evaluasi CalculationType ]                     [ Gunakan Nilai Default / 0 ]
                      │
       ┌──────────────┴──────────────┐
       ▼                             ▼
[ PERCENTAGE_WITH_CAP ]       [ FLAT / PERCENTAGE ]
       │                             │
       ▼                             ▼
[ Raw = EBA * (Pct / 100) ]   [ Hitung Flat / Persen Biasa ]
[ Cap = CapAmount ]
[ Fee = Min(Raw, Cap) ]
[ IsCapApplied = Raw > Cap ]
       │
       └─────────────────────────────┬───────────────────────────────┘
                                     │
                                     ▼
                      [ Evaluasi Penjamin Pasien (Guarantor) ]
                                     │
                      ┌──────────────┴──────────────┐
                      ▼                             ▼
             [ Penjamin Sistem Paket ]       [ Penjamin Mandiri / Asuransi ]
              (BPJS / JKN / INA-CBG)                        │
                      │                                     ▼
                      ▼                       [ Porsi Pasien = Fee ]
         [ Porsi Pasien = Rp 0 ]
  [ NonBillableResidualAmount = Fee ]
                      │
                      └──────────────┬───────────────────────────────┘
                                     │
                                     ▼
                 [ Deteksi Kunjungan Rujukan Rawat Jalan Asal ]
                                     │
                      ┌──────────────┴──────────────┐
                      ▼                             ▼
             [ Ada Rujukan Rajal ]           [ Bukan Pasien Alihan ]
                      │                                     │
                      ▼                                     ▼
         [ Cari Biaya Admin Rajal ]                 [ Selesai Kalkulasi ]
                      │
       ┌──────────────┴──────────────┐
       ▼                             ▼
[ Belum Dibayar ]             [ Sudah Dibayar di Poli ]
       │                             │
       ▼                             ▼
[ Tandai Void Item Rajal: ]   [ Terbitkan BilRefundableCredit: ]
[ "SUPERSEDED_BY_INPATIENT" ] [ Source: REFERRED_OUTPATIENT_ADMIN ]
                              [ Memotong Saldo Invoice Ranap ]
                                     │
                                     ▼
                                 [ Selesai ]
```

---

## Spesifikasi Kontrak DTO & Respon Integrasi

### Model Respon Kalkulasi Administrasi (`AdministrationFeeCalculationResponse`)
* **Penggunaan:** Dikembalikan pada proses kalkulasi invoice maupun inquiry rincian tagihan ranap.

| Field | Tipe | Deskripsi |
| :--- | :--- | :--- |
| `policyId` | `Guid` | ID entitas kebijakan `MstAdministrationFeePolicy` yang aktif |
| `policyCode` | `string` | Kode kebijakan (contoh: `"ADM-RANAP-01"`) |
| `policyName` | `string` | Nama kebijakan resmi rumah sakit |
| `calculationType` | `string` | Formula hitung: `"PERCENTAGE_WITH_CAP"`, `"PERCENTAGE"`, atau `"FLAT"` |
| `percentage` | `decimal?` | Nilai persentase (contoh: `7.00`) |
| `capAmount` | `decimal?` | Pagu batas atas maksimal biaya (contoh: `6000000.00`) |
| `eligibleBaseAmount` | `decimal` | Dasar tagihan non-farmasi yang dikenakan persentase admin |
| `rawCalculatedAmount` | `decimal` | Hasil perkalian persentase sebelum dipotong pagu (contoh: Rp 7.000.000) |
| `appliedAmount` | `decimal` | Biaya akhir yang disematkan pada invoice (contoh: Rp 6.000.000) |
| `isCapApplied` | `bool` | `true` jika tagihan terkena pagu batas atas maksimal |
| `isPackageGuaranteed` | `bool` | `true` jika pasien menggunakan penjamin paket (BPJS Kesehatan) |
| `referredOutpatientAdminVoided` | `bool` | `true` jika biaya admin rajal sebelumnya berhasil dibatalkan otomatis |
| `referredOutpatientAdminCreditedAmount` | `decimal` | Jumlah nominal rupiah admin rajal yang dialihkan sebagai kredit pemotong ranap |

**Contoh Payload Hasil Perhitungan (JSON):**
```json
{
  "policyId": "d1c2b3a4-5678-90ab-cdef-1234567890ab",
  "policyCode": "ADM-RANAP-01",
  "policyName": "Biaya Administrasi Rawat Inap 7% Cap Rp6jt",
  "calculationType": "PERCENTAGE_WITH_CAP",
  "percentage": 7.00,
  "capAmount": 6000000.00,
  "eligibleBaseAmount": 100000000.00,
  "rawCalculatedAmount": 7000000.00,
  "appliedAmount": 6000000.00,
  "isCapApplied": true,
  "isPackageGuaranteed": false,
  "referredOutpatientAdminVoided": false,
  "referredOutpatientAdminCreditedAmount": 50000.00
}
```

---

## Verifikasi & Kepatuhan Arsitektur

| Pemeriksaan / Uji | Hasil | Status | Catatan Bukti |
| :--- | :--- | :--- | :--- |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1` | **PASS** | VERIFIED | Mode Strict, 19 berkas working tree dievaluasi, 0 pelanggaran (`VIOLATION: 0`, `REVIEW: 0`, `INFO: 0`) |
| `dotnet build` | **Menunggu eksekusi mandiri pengguna** | NOT VERIFIED | Sesuai instruksi eksplisit pengguna: *"Akan tetapi, jangan jalankan build secara automatis"* |
| Persentase 7% & Pagu Rp 6.000.000 (`BIL-VAL-120`) | Selesai | VERIFIED (Inspeksi Kode) | Logika `PERCENTAGE_WITH_CAP` dengan `Math.Min(rawAmount, policy.CapAmount.Value)` dan `MidpointRounding.AwayFromZero` terimplementasi di `AdministrationFeeCalculationService.cs` |
| Evaluasi Tanggal Discharge (`BKC-DEC-122`) | Selesai | VERIFIED (Inspeksi Kode) | Waktu evaluasi menggunakan `request.DischargeDate` atau `DateTimeOffset.UtcNow` terhadap `policy.EffectiveFrom` |
| Pengecualian Beban Pasien BPJS (`BKC-DEC-121`, `BKC-AC-089`) | Selesai | VERIFIED (Inspeksi Kode) | Deteksi `IsPackageSystemGuarantor` mengatur `patientResponsibility = 0m` dan `NonBillableResidualAmount = AppliedAmount` pada invoice |
| Void Otomatis Admin Rajal Belum Bayar (`BIL-VAL-126`) | Selesai | VERIFIED (Inspeksi Kode) | Eksekusi `ReconcileReferredOutpatientAdminFeeAsync` menandai `IsVoid = true` dengan catatan `SUPERSEDED_BY_INPATIENT_ADMISSION` secara idempoten |
| Pengalihan Kredit Admin Rajal Terbayar (`BKC-DEC-119`) | Selesai | VERIFIED (Inspeksi Kode) | Penerbitan entitas `BilRefundableCredit` dengan `SourceType = "REFERRED_OUTPATIENT_ADMIN"` memotong tagihan invoice ranap |

---

## Status Task Selanjutnya

- `BE-BKC-075` (Layanan Kelayakan Pemulangan Financial Clearance & Auto-Reblock Handoff)
- `BE-BKC-076` (API Controller Integrasi Ranap untuk Inquiry, Kalkulasi Kamar, Reevaluasi, & Acknowledge)
