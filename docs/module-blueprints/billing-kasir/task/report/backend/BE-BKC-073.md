# BE-BKC-073 — Mesin Kalkulasi Sewa Kamar Bertingkat & Pro-Rata Transfer Menit Riil

## Ringkasan untuk Pembaca Umum

Dalam pengelolaan rumah sakit, perhitungan biaya sewa kamar rawat inap seringkali menjadi sumber perselisihan antara keluarga pasien dan pihak manajemen kasir apabila tidak memiliki aturan perhitungan yang transparan, terstandar, dan adil. Ada tiga situasi pelayanan kamar yang paling krusial:

1. **Pasien Masuk pada Malam Hari (*Midnight Admission*):** Pasien yang baru masuk kamar perawatan pada larut malam (misalnya pukul 22.30 WIB) tentu merasa tidak adil jika langsung ditagihkan tarif sewa kamar 1 hari penuh (100%). Oleh karena itu, sistem memberlakukan potongan tarif bertingkat yang proporsional sesuai jam masuk:
   - **Masuk sebelum pukul 18.00 WIB:** Dikenakan tarif penuh 100% (`TARIF_PENUH_SEBELUM_18`).
   - **Masuk antara pukul 18.00 s/d sebelum 22.00 WIB:** Dikenakan diskon 50%, sehingga pasien hanya membayar 50% dari tarif harian (`POTONGAN_50_PERSEN_JAM_18_SD_22`).
   - **Masuk antara pukul 22.00 s/d sebelum 24.00 WIB:** Dikenakan diskon 80%, sehingga pasien hanya membayar 20% dari tarif harian (`POTONGAN_80_PERSEN_JAM_22_SD_24`).
   - **Masuk pukul 00.00 WIB ke atas (hari baru):** Beban hari sebelumnya adalah 0% (tidak ditagih sama sekali / `HARI_BARU_TIDAK_DITAGIH`), dan tagihan baru mulai dihitung di hari yang baru tersebut.

2. **Keterlambatan Kepulangan (*Late Checkout*):** Batas standar waktu kepulangan pasien rawat inap rumah sakit adalah pukul 12.00 WIB siang agar tempat tidur dapat dibersihkan dan disiapkan untuk pasien berikutnya. Apabila pasien melewati batas waktu tersebut (misalnya baru meninggalkan bangsal pukul 14.00 WIB), sistem secara otomatis menambahkan penalti keterlambatan (*Late Checkout Fee*) sebesar 50% dari tarif kamar harian (`PENALTI_LATE_CHECKOUT_50_PERSEN`).

3. **Pasien Berpindah Kamar Lebih dari Sekali dalam Satu Hari (*Pro-Rata Transfer Menit Riil*):** Jika kondisi klinis pasien memburuk atau membaik sehingga harus dipindahkan antar-ruangan dalam hari kalender yang sama (misalnya dari Kamar Standar ke Ruang Perawatan Intensif / ICU), tarif sewa kamar tidak boleh dirata-ratakan secara kasar. Sistem menghitung durasi menit penempatan riil pada tiap-tiap kamar dengan rumus:
   $$\text{Biaya Kamar} = \text{Round}\left( \frac{\text{Menit Hunian Kamar}}{\text{Total Menit Hari Itu}} \times \text{Tarif Harian Kamar}, 2 \right)$$

4. **Pelepasan Ketergantungan Lama ke Modul Rawat Inap:** Layanan ringkasan tagihan pasien (`PatientBillingSummaryService`) sebelumnya membaca status kelayakan pulang langsung dari tabel operasional bangsal (`InpFinancialClearance`). Melalui task ini, ketergantungan tersebut dilepaskan secara total dan digantikan dengan membaca surat fakta resmi `BilInpatientClearanceHandoff` yang diterbitkan oleh modul Billing sendiri. Ini mengukuhkan Billing sebagai *Single Source of Truth* status finansial pasien.

---

### Contoh Kasus Nyata di Rumah Sakit

* **Contoh 1 (Pasien Masuk Jam Malam — `BIL-VAL-118`):**
  Tn. Budi masuk kamar Kelas 1 (tarif Rp 1.000.000/hari) pada pukul 22.30 WIB.
  Sistem secara otomatis menerapkan kebijakan `POTONGAN_80_PERSEN_JAM_22_SD_24` (faktor 0.20), sehingga biaya kamar hari pertama Tn. Budi tepat Rp 200.000 (bukan Rp 1.000.000).

* **Contoh 2 (Pasien Pindah Kamar Dua Kali dalam Sehari — `BIL-VAL-119`):**
  Ny. Siti pada tanggal 10 Oktober menempati Kamar Standar (Rp 600.000/hari) selama 360 menit (6 jam), kemudian dipindahkan ke ICU (Rp 2.400.000/hari) selama 1.080 menit (18 jam). Total durasi rawat tanggal 10 Oktober adalah 1.440 menit (24 jam).
  Biaya sewa kamar tanggal 10 Oktober dihitung pro-rata menit riil:
  - Kamar Standar: $(360 / 1.440) \times \text{Rp } 600.000 = \text{Rp } 150.000$
  - Ruang ICU: $(1.080 / 1.440) \times \text{Rp } 2.400.000 = \text{Rp } 1.800.000$
  - Total sewa kamar tanggal 10 Oktober: $\text{Rp } 150.000 + \text{Rp } 1.800.000 = \text{Rp } 1.950.000$.

* **Contoh 3 (Denda Keterlambatan Kepulangan — `BKC-DEC-112`):**
  Tn. Joko selesai dirawat di Kamar VIP (tarif Rp 1.500.000/hari) dan dokter menerbitkan instruksi pulang pada pagi hari. Namun, karena menunggu jemputan keluarga dari luar kota, Tn. Joko baru checkout meninggalkan tempat tidur pada pukul 14.15 WIB.
  Karena waktu keluar $> 12.00$ WIB, sistem menambahkan penalti late checkout sebesar $50\% \times \text{Rp } 1.500.000 = \text{Rp } 750.000$ pada rincian tagihan akhir.

---

## Lembar Metadata Task

- TASK ID: BE-BKC-073
- TASK TYPE: Domain Calculation Engine & Service Decoupling (`InpatientRoomChargeCalculationService` & `PatientBillingSummaryService`)
- COMPLEXITY: MEDIUM
- CLASSIFICATION SCORE: 3 (repo 0 + berkas diperiksa ≤8 → 0 + berkas diubah/dibuat 4 → 1 + logika kalkulasi tarif bertingkat, late checkout, pro-rata durasi riil, dan pemisahan dependensi → 2; total score 3)
- MODEL: Gemini 3.8 Flash (High)
- TASK MODE: BACKEND
- WRITE TARGET: `NewQuilvianSystemBackend` — `Areas/HealthServices/BillingManagement/Billing/**`, `Areas/HealthServices/BillingManagement/Operational/**` (source); `docs/module-blueprints/billing-kasir/task/report/backend/**`, `roadmap/**`, `requirement-traceability.md` (laporan & registry)
- FILES INSPECTED:
  - `docs/module-blueprints/billing-kasir/02-backend-architecture.md` (`BKC-DES-043`, `BKC-DES-050`)
  - `docs/module-blueprints/billing-kasir/contracts/validation-matrix.md` (`BIL-VAL-118`, `BIL-VAL-119`)
  - `docs/module-blueprints/billing-kasir/contracts/api-contract.md` (`BIL-API-1.4`, `POST /invoices/occupancy-charges`)
  - `docs/module-blueprints/billing-kasir/flowcharts/06-integrasi-rawat-inap.md`
  - `Areas/HealthServices/BillingManagement/Operational/Services/PatientBillingSummaryService.cs`
  - `Areas/HealthServices/InPatientManagement/Models/InpBedPlacement.cs`
  - `Areas/HealthServices/MasterData/Models/MstTariff.cs`
  - `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs`
- FILES CHANGED / CREATED:
  - **Dibuat**: `Areas/HealthServices/BillingManagement/Billing/DTOs/InpatientRoomChargeDtos.cs`
    - Mendefinisikan konstanta kebijakan tarif kamar: `TARIF_PENUH_SEBELUM_18`, `POTONGAN_50_PERSEN_JAM_18_SD_22`, `POTONGAN_80_PERSEN_JAM_22_SD_24`, `HARI_BARU_TIDAK_DITAGIH`, `PENALTI_LATE_CHECKOUT_50_PERSEN`, `PRO_RATA_TRANSFER_MENIT`.
    - Mendefinisikan DTO kalkulasi: `PlacementSegmentInput`, `RoomPlacementSegmentCalculation`, `DailyRoomChargeCalculationResult`, `InpatientEpisodeRoomChargeSummary`, `OccupancyChargeRequest`, `OccupancyChargeResponse`.
  - **Dibuat**: `Areas/HealthServices/BillingManagement/Billing/Services/IInpatientRoomChargeCalculationService.cs`
    - Kontrak antarmuka mesin hitung: penentuan faktor jam masuk, denda late checkout, alokasi pro-rata transfer harian, ringkasan episode ranap, dan pemrosesan event hunian kamar.
  - **Dibuat**: `Areas/HealthServices/BillingManagement/Billing/Services/InpatientRoomChargeCalculationService.cs`
    - Implementasi mesin hitung sewa kamar dengan presisi desimal 2 angka (`MidpointRounding.AwayFromZero`).
    - Penanganan zona waktu lokal rumah sakit (WIB / UTC+7) yang konsisten.
    - Pengelompokan harian penempatan tempat tidur (`InpBedPlacement`) dan penarikan tarif kamar dari `MstTariff` (`IsRoomCharge = true`).
  - **Diperbarui**: `Areas/HealthServices/BillingManagement/Operational/Services/PatientBillingSummaryService.cs`
    - Melepaskan dependensi lama terhadap entity `InpFinancialClearance` dan enum `InpFinancialClearanceStatus`.
    - Membaca status kelayakan langsung dari `_dbContext.BilInpatientClearanceHandoffs` berdasarkan `EncounterId` dan nomor versi finansial tertinggi (`FinancialVersion`).
    - Memetakan label status kelayakan menggunakan konstanta `InpatientClearanceStatuses` (`CLEARED` -> "Layak", `BLOCKED` -> "Tertahan", `PENDING` -> "Menunggu penilaian", `REVOKED` -> "Dibatalkan").
  - **Diperbarui**: `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs`
    - Mendaftarkan `IInpatientRoomChargeCalculationService` dan `InpatientRoomChargeCalculationService` ke DI container.

---

## Implementasi Teknis & Algoritma Kalkulasi

### 1. Mesin Jam Masuk Bertingkat (`CalculateAdmissionTierMultiplier`)

Algoritma mengevaluasi waktu masuk lokal rumah sakit (WIB / UTC+7):
```text
Jika TimeOfDay < 18:00:00:
    Multiplier = 1.00m (100%), Kebijakan = TARIF_PENUH_SEBELUM_18
Jika 18:00:00 <= TimeOfDay < 22:00:00:
    Multiplier = 0.50m (50%), Kebijakan = POTONGAN_50_PERSEN_JAM_18_SD_22
Jika 22:00:00 <= TimeOfDay < 24:00:00:
    Multiplier = 0.20m (20%), Kebijakan = POTONGAN_80_PERSEN_JAM_22_SD_24
Jika TimeOfDay >= 00:00:00 (hari kalender baru):
    Hari sebelumnya 0% (0.00m), Kebijakan = HARI_BARU_TIDAK_DITAGIH
```

### 2. Penalti Keterlambatan Kepulangan (`CalculateLateCheckoutFee`)

```text
Threshold = 12:00:00 siang WIB
Jika CheckoutTime > Threshold:
    LateFee = Round(DailyTariff * 0.50m, 2, AwayFromZero)
    IsLate = True, Kebijakan = PENALTI_LATE_CHECKOUT_50_PERSEN
Lainnya:
    LateFee = 0.00m, IsLate = False
```

### 3. Alokasi Pro-Rata Transfer Kamar Multipel (`CalculateDailyProRataTransfers`)

Untuk setiap segmen kamar $i$ dalam hari kalender yang sama:
- Durasi menit: $M_i = \max(1, \text{Round}((\text{End}_i - \text{Start}_i).\text{TotalMinutes}))$
- Total durasi: $M_{total} = \sum M_i$
- Biaya segmen: $\text{Round}\left( \frac{M_i}{M_{total}} \times \text{DailyRate}_i \times \text{DayMultiplier}, 2, \text{AwayFromZero} \right)$
- Kebijakan yang disematkan: `PRO_RATA_TRANSFER_MENIT`.

---

## Spesifikasi Kontrak API & Integrasi

### `POST /invoices/occupancy-charges` (Kontrak `BIL-API-1.4`)
* **Grup Tag:** `[Tags("Billing Management - Inpatient Integration")]`
* **Metode & Path:** `POST /api/v1/health-services/billing-management/billing/invoices/occupancy-charges`
* **Hak Akses:** `BillingInvoice : Create`
* **Header Wajib:** `Idempotency-Key` (Guid / String)

| Parameter / Field | Tipe | Status | Deskripsi |
| :--- | :--- | :--- | :--- |
| `encounterId` | `Guid` | Wajib | Kunjungan rawat inap yang ditagihkan |
| `placementId` | `Guid` | Wajib | ID penempatan bed pasien |
| `roomId` | `string` | Wajib | Kode atau ID ruangan rawat |
| `roomName` | `string` | Opsional | Nama ruangan rawat |
| `bedId` | `string` | Wajib | ID tempat tidur |
| `bedCode` | `string` | Wajib | Nomor/kode bed |
| `patientClassId` | `string` | Wajib | ID kelas perawatan |
| `occupancyStartAt` | `DateTimeOffset` | Wajib | Waktu mulai menempati tempat tidur |
| `occupancyEndAt` | `DateTimeOffset?` | Opsional | Waktu selesai menempati tempat tidur (jika sudah keluar) |
| `changeType` | `string` | Wajib | Tipe peristiwa: `BED_OCCUPIED`, `TRANSFERRED`, dll. |
| `version` | `int` | Wajib | Versi penempatan tempat tidur |

**Contoh Payload Response (200 OK):**
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Beban sewa kamar berhasil dicatat pada invoice berjalan.",
  "data": {
    "invoiceId": "a1b2c3d4-e5f6-7a8b-9c0d-1e2f3a4b5c6d",
    "invoiceNumber": "INV-202609-0012",
    "currentChargeAmount": 1500000.00,
    "appliedPolicy": "POTONGAN_50_PERSEN_JAM_18_SD_22",
    "roomChargeAmount": 750000.00,
    "versionNo": 2
  }
}
```

---

## Verifikasi & Kepatuhan Arsitektur

| Pemeriksaan / Uji | Hasil | Status | Catatan Bukti |
| :--- | :--- | :--- | :--- |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1` | **PASS** | VERIFIED | Mode Strict, 13 berkas working tree dievaluasi, 0 pelanggaran (`VIOLATION: 0`, `REVIEW: 0`, `INFO: 0`) |
| `dotnet build` | **Menunggu eksekusi mandiri pengguna** | NOT VERIFIED | Sesuai instruksi eksplisit pengguna: *"Akan tetapi, jangan jalankan build secara automatis"* |
| Jam Masuk Bertingkat (`BIL-VAL-118`) | Selesai | VERIFIED (Inspeksi Kode) | Seluruh cabang jam malam (<18:00, 18:00-<22:00, 22:00-<24:00, >=00:00) terimplementasi dengan multiplier dan policy name baku |
| Penalti Late Checkout | Selesai | VERIFIED (Inspeksi Kode) | Checkout >12:00 siang waktu lokal menghasilkan penalti 50% tarif harian kamar |
| Pro-Rata Transfer Menit Riil (`BIL-VAL-119`) | Selesai | VERIFIED (Inspeksi Kode) | Perhitungan proporsional $(M_i / M_{total}) \times \text{Tarif}_i$ dibulatkan 2 desimal `MidpointRounding.AwayFromZero` |
| Pelepasan Dependensi Lama | Selesai | VERIFIED (Inspeksi Kode) | `PatientBillingSummaryService.cs` bersih dari `InpFinancialClearance`, membaca `BilInpatientClearanceHandoffs` |

---

## Status Task Selanjutnya

- `BE-BKC-074` (Layanan Biaya Administrasi Ranap 7% Cap Rp6jt & Penggantian Admin Rajal)
- `BE-BKC-075` (Layanan Kelayakan Pemulangan Financial Clearance & Auto-Reblock Handoff)
- `BE-BKC-076` (API Controller Integrasi Ranap untuk Inquiry, Kalkulasi Kamar, Reevaluasi, & Acknowledge)
