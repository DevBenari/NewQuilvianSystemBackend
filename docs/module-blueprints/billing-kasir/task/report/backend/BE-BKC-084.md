# Laporan Perubahan Backend — `BE-BKC-084`

## Ringkasan untuk Pembaca Umum

Ketika seorang pasien berobat ke Poliklinik Rawat Jalan (Rajal) lalu oleh dokter diputuskan harus segera menjalani Rawat Inap (Ranap) pada hari yang sama, timbul pertanyaan penting mengenai biaya administrasi: *Apakah pasien harus membayar dua kali biaya administrasi (administrasi rawat jalan dan administrasi rawat inap)?*

Kebijakan manajemen rumah sakit (`BKC-DEC-119`) menetapkan prinsip adil: **Pasien tidak boleh dibebani biaya administrasi ganda**. Biaya administrasi poliklinik digantikan oleh biaya administrasi rawat inap. Namun, penerapan aturan ini harus berbasis bukti transfer/rujukan internal yang sah (*tautan aktif*), bukan sekadar menebak tanggal kedatangan.

Sebelum task `BE-BKC-084`:
1. **Pemotongan Semu Berbasis Kedekatan Tanggal Tanpa Bukti Rujukan:**
   Sistem lama secara otomatis memotong biaya administrasi rawat inap sebesar Rp 50.000 jika mendeteksi pasien pernah ke poliklinik dalam rentang ±1 hari, meskipun kunjungan poliklinik tersebut sama sekali tidak berhubungan dengan rawat inapnya (misalnya pasien kemarin kontrol gigi, hari ini masuk rawat inap karena kecelakaan). Ini merugikan pendapatan rumah sakit dan tidak memiliki landasan bukti transfer (`BKC-DEC-161`, regresi F12).
2. **Alur Pembatalan Paksa Item Tindakan Medis yang Keliru:**
   Pada sistem sebelumnya, ada logika rekonsiliasi yang secara sepihak membatalkan (*void*) item tagihan jika mendeteksi transfer, yang sempat menyebabkan tindakan medis riil ikut terhapus dari tagihan. Task `BE-BKC-081` telah menghentikan rekonsiliasi liar tersebut, dan task `BE-BKC-083` telah menyiapkan dua kolom pencatatan kredit pada skema database.
3. **Ketiadaan Logika Penggantian dan Pengkreditan Terstruktur:**
   Sistem belum memiliki evaluasi terpadu berbasis tautan resmi (`BilInvoiceEncounterLink`) untuk menentukan apakah administrasi rawat jalan harus digratiskan (jika tagihan poli masih terbuka) atau diperhitungkan sebagai kredit pengurang tagihan rawat inap (jika tagihan poli terlanjur dibayar/lunas).

---

### Solusi Penyempurnaan `BE-BKC-084`

Melalui implementasi task `BE-BKC-084` (mengacu pada arsitektur `BKC-DES-084`, `BKC-DES-085`, `BKC-DES-087`, dan pembaruan kontrak `BIL-API-2.0` serta `BIL-CALCULATION-1.0`):

1. **Layanan Evaluasi Penggantian Administrasi Berbasis Tautan Aktif (`ResolveReferredAdminReplacementAsync`):**
   - Diimplementasikan pada `AdministrationFeeCalculationService.cs` dengan pendekatan baca-saja (*read-only* murni menggunakan `AsNoTracking`).
   - Tidak membuka transaksi tulis database, aman dipanggil baik dari kalkulasi pratinjau (*preview*) maupun penyimpanan permanen.
2. **Perlakuan untuk Tagihan Poliklinik Terbuka (`OPEN`):**
   - Jika tagihan rawat jalan memiliki tautan transfer resmi ke rawat inap yang tidak dibatalkan, biaya administrasi poliklinik langsung dinolkan (`AppliedAmount = 0m`).
   - Response DTO kalkulasi menandai `IsSupersededByInpatientAdmission = true`, mencatat id invoice rawat inap pada `SupersededByInpatientInvoiceId`, serta menyematkan alasan resmi `SupersededReasonCode = "SUPERSEDED_BY_INPATIENT_ADMISSION"`.
3. **Perlakuan untuk Tagihan Poliklinik yang Sudah Difinalisasi / Lunas (`FINAL` / `CLOSED` / `SETTLED_BY_WRITE_OFF`):**
   - Biaya administrasi poliklinik yang sudah dibayarkan oleh pasien (porsi pasien dari snapshot rincian kalkulasi poli) dikreditkan pada tagihan rawat inap.
   - Pada tagihan rawat inap, kredit ini dikurangkan langsung dari porsi yang harus dibayar pasien (*patient responsibility*) setelah potongan promo/diskon (`:314`).
   - Sistem menjamin porsi tagihan pasien tidak pernah menjadi minus (*non-negative cap*). Sisa kredit yang belum terserap dicatat pada `ReferredOutpatientAdminUnappliedCreditAmount` untuk disiapkan menjadi kredit pengembalian (*refundable credit*) pada task berikutnya (`BE-BKC-085`).
4. **Penghapusan Total Potongan Flat Rp 50.000 pada Rawat Inap (`BKC-DES-087`):**
   - Metode `GetPriorAppliedAdminFeeAmountAsync` untuk tipe layanan `RANAP` kini mengembalikan `(0m, int.MinValue)`.
   - Pasien rawat inap yang tidak tertaut rujukan resmi dari poliklinik akan dikenakan biaya administrasi rawat inap secara utuh sesuai ketentuan kelas kamar tanpa potongan liar.
   - Logika administrasi satu kali per hari untuk Rawat Jalan / IGD / OTC tetap dipertahankan dan tidak terganggu.
5. **Penyempurnaan DTO Response (`BIL-API-2.0` Bagian A):**
   - Menghapus field usang `ReferredOutpatientAdminVoided`.
   - Menambahkan 5 field resmi: `ReferredOutpatientAdminUnappliedCreditAmount`, `IsSupersededByInpatientAdmission`, `SupersededByInpatientInvoiceId`, `SupersededReasonCode`, dan rincian daftar invoice asal `ReferredOutpatientAdminOrigins`.
6. **Perlindungan Item Tindakan Medis (`BKC-DEC-167`):**
   - Item tarif tindakan atau pelayanan medis yang memuat kata "Administrasi" (misalnya "Administrasi Rekam Medis" atau "Administrasi Kamar Operasi") diperlakukan murni sebagai item tarif biasa dan **tidak pernah** dianggap sebagai biaya administrasi umum yang dibatalkan (`BIL-AT-230`).

---

## Contoh Kasus Nyata di Rumah Sakit

### Skenario 1: Pasien Tn. Budi (Rujukan Rawat Jalan ke Rawat Inap — Tagihan Poli Masih Belum Dibayar) — `BIL-AT-204`
1. **Pemeriksaan di Poli Penyakit Dalam:**
   - Tn. Budi berobat ke poli spesialis. Dokter mendiagnosis demam berdarah dengan trombosit turun drastis dan menyarankan opname segera.
   - Tagihan poliklinik masih berstatus `OPEN` (belum dibayar oleh keluarga di kasir).
2. **Penerbitan Tautan Transfer Resmi:**
   - Bagian admisi rawat inap memproses rujukan rawat jalan tersebut sehingga terbentuk tautan resmi `BilInvoiceEncounterLink`.
3. **Kalkulasi Tagihan Poli:**
   - Saat kasir membuka tagihan poliklinik, sistem secara otomatis mengevaluasi tautan aktif. Biaya administrasi poli yang awalnya Rp 50.000 menjadi **Rp 0** (`isSupersededByInpatientAdmission = true`, kode `SUPERSEDED_BY_INPATIENT_ADMISSION`).
   - Kasir poliklinik tidak menagihkan biaya administrasi poli kepada keluarga pasien.
4. **Kalkulasi Tagihan Rawat Inap:**
   - Pada tagihan rawat inap Tn. Budi, origin poli terdeteksi berstatus `treatment = "SUPERSEDED"`, dengan kredit Rp 0. Administrasi rawat inap (misal Rp 150.000) ditagihkan di rawat inap. Pasien hanya membayar 1 kali biaya administrasi.

### Skenario 2: Pasien Ny. Aminah (Rujukan Rawat Jalan ke Rawat Inap — Tagihan Poli Sudah Lunas) — `BIL-AT-205`
1. **Konsultasi Poliklinik Sudah Dibayar:**
   - Ny. Aminah kontrol ke Poli Jantung, membayar biaya konsultasi dokter Rp 200.000 dan biaya administrasi poli Rp 50.000 di kasir (tagihan berstatus `CLOSED`).
2. **Kondisi Menurun & Masuk Rawat Inap:**
   - Beberapa jam kemudian di ruang observasi, dokter memutuskan pasien harus dirawat inap di ICCU. Admisi membuat tautan transfer resmi dari encounter poli ke ranap.
3. **Kredit pada Tagihan Rawat Inap:**
   - Biaya administrasi rawat inap ICCU adalah Rp 150.000.
   - Sistem mendeteksi bahwa tagihan poli telah berstatus `CLOSED` dan pasien telah melunasi porsi administrasi Rp 50.000.
   - Sistem mencatat kredit administrasi `ReferredOutpatientAdminCreditAmount = Rp 50.000`.
   - Porsi pasien pada tagihan rawat inap otomatis berkurang sebesar Rp 50.000. Keluarga pasien hanya membayar sisa selisih biaya administrasi rawat inap sebesar Rp 100.000.

### Skenario 3: Pasien Tn. Hendra (Kunjungan Poliklinik Terpisah Tanpa Rujukan) — `BIL-AT-208`
1. **Riwayat Berobat Kemarin:**
   - Kemarin sore Tn. Hendra berobat ke Poli Gigi untuk tambal gigi dan sudah lunas.
2. **Masuk Rawat Inap Hari Ini:**
   - Hari ini Tn. Hendra mengalami cedera kaki saat berolahraga dan masuk rawat inap melalui IGD.
   - Tidak ada tautan transfer poli ke rawat inap karena kedua kasus medis ini sama sekali tidak berhubungan.
3. **Penetapan Administrasi Rawat Inap:**
   - Pada sistem lama, sistem memotong Rp 50.000 secara keliru hanya karena tanggal kunjungan berdekatan (kemarin dan hari ini).
   - Melalui penyempurnaan `BE-BKC-084`, sistem membaca bahwa tidak ada tautan rujukan aktif. Biaya administrasi rawat inap dikenakan **penuh sesuai tarif (Rp 150.000)** tanpa potongan semu Rp 50.000. Pendapatan rumah sakit terlindungi secara akuntabel.

### Skenario 4: Pasien Batal Rawat Inap — `BIL-AT-212`
1. **Pembatalan Rawat Inap:**
   - Setelah tagihan poli digratiskan administrasinya (Rp 0), keluarga pasien memutuskan batal opname dan memilih pulang atas permintaan sendiri. Invoice rawat inap dibatalkan (`Cancelled`).
2. **Pemulihan Otomatis Administrasi Poli:**
   - Saat kasir poli membuka kembali tagihan rawat jalan untuk proses penyelesaian pulang, sistem mendeteksi invoice rawat inap tujuan telah berstatus `Cancelled`.
   - Biaya administrasi poliklinik secara otomatis kembali muncul sebesar Rp 50.000 (`isSupersededByInpatientAdmission = false`). Rumah sakit tidak kehilangan tagihan haknya.

---

## Lembar Metadata Task

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-084` |
| Judul | Model penggantian admin berbasis tautan aktif |
| Slice / Gelombang | Gelombang Eksekusi 1 (Revisi Blueprint 2.2 / 2.3) |
| Roadmap | `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` § `BE-BKC-084` |
| Trace | `FR-BKC-279`, `FR-BKC-280`, `FR-BKC-282`; `BKC-DEC-119`, `BKC-DEC-161`, `BKC-DEC-167`; `BKC-DES-084`, `BKC-DES-085`, `BKC-DES-087` |
| Kontrak | `BIL-API-2.0` Bagian A; `BIL-VALIDATION-2.0` `BIL-VAL-126`, `BIL-VAL-160`; `BIL-CALCULATION-1.0` |
| Acceptance Criteria | `BIL-AT-204`–`208`, `211`–`213`, `230` |
| Dependency | `BE-BKC-081` (selesai), `BE-BKC-083` (selesai) |
| Klasifikasi | Tinggi / Perhitungan Angka Finansial Tagihan Pasien |
| Task mode | `BACKEND` |
| Target berkas | `Areas/HealthServices/BillingManagement/Billing/Dtos/AdministrationFeeCalculationDtos.cs`<br/>`Areas/HealthServices/BillingManagement/Billing/Dtos/BillingInvoiceDtos.cs`<br/>`Areas/HealthServices/BillingManagement/Billing/Services/IAdministrationFeeCalculationService.cs`<br/>`Areas/HealthServices/BillingManagement/Billing/Services/AdministrationFeeCalculationService.cs`<br/>`Areas/HealthServices/BillingManagement/Billing/Services/BillingCalculationService.cs` |
| Tanggal Implementasi | 17 Oktober 2026 |
| Status | ✅ **SELESAI (Implementasi kode selesai & terverifikasi)** |

---

## Backend Governance Preflight

| Field Preflight | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `BillingManagement / Billing` |
| Submodule | `Billing` |
| Service Terkait | `AdministrationFeeCalculationService`, `BillingCalculationService` |
| Prefix Registry | `Bil` — status `ACTIVE` pada registry arsitektur Quilvian |
| Applicability | `TOUCHED LEGACY` (Penyempurnaan kalkulasi administrasi rawat jalan dan rawat inap) |
| QBE Rules Berlaku | `QBE-CAL-001` (presisi hitung nominal uang 18,2, pembatasan non-negatif), `QBE-SEC-001` (audit trail rincian origin), `QBE-API-001` (konsistensi kontrak DTO API publik) |
| Database Migration | **TIDAK ADA MIGRATION BARU** (Skema kolom telah disiapkan pada task `BE-BKC-083`). Tidak ada eksekusi database otomatis. |

---

## 1. Rincian Perubahan Source Code

### 1.1 Berkas yang Diubah

#### 1. [`AdministrationFeeCalculationDtos.cs`](file:///Users/yasminap/QuilvianV2/NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Billing/Dtos/AdministrationFeeCalculationDtos.cs)
- Menghapus properti usang `ReferredOutpatientAdminVoided` dari `AdminFeeCalculationResult`.
- Menambahkan model DTO rincian invoice asal:
  ```csharp
  public class ReferredAdminOriginResult
  {
      public Guid OriginInvoiceId { get; set; }
      public string OriginInvoiceNumber { get; set; } = string.Empty;
      public Guid OriginEncounterId { get; set; }
      public string OriginInvoiceStatus { get; set; } = string.Empty;
      public string Treatment { get; set; } = string.Empty; // SUPERSEDED, CREDITED, NO_ADMIN
      public decimal AdminPatientAmount { get; set; }
      public decimal CreditAmount { get; set; }
  }
  ```
- Menambahkan model DTO hasil evaluasi penggantian:
  ```csharp
  public class ReferredAdminReplacementResult
  {
      public bool IsSupersededByInpatientAdmission { get; set; }
      public Guid? SupersededByInpatientInvoiceId { get; set; }
      public string? SupersededReasonCode { get; set; }
      public decimal ReferredOutpatientAdminCreditedAmount { get; set; }
      public decimal ReferredOutpatientAdminUnappliedCreditAmount { get; set; }
      public IReadOnlyList<ReferredAdminOriginResult> ReferredOutpatientAdminOrigins { get; set; } = [];
  }
  ```

#### 2. [`BillingInvoiceDtos.cs`](file:///Users/yasminap/QuilvianV2/NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Billing/Dtos/BillingInvoiceDtos.cs)
- Memperbarui `AdministrationFeeCalculationResponse`:
  - Menghapus `ReferredOutpatientAdminVoided`.
  - Menambahkan `ReferredOutpatientAdminUnappliedCreditAmount`.
  - Menambahkan `IsSupersededByInpatientAdmission`.
  - Menambahkan `SupersededByInpatientInvoiceId`.
  - Menambahkan `SupersededReasonCode`.
  - Menambahkan `IReadOnlyList<ReferredAdminOriginResult> ReferredOutpatientAdminOrigins`.

#### 3. [`IAdministrationFeeCalculationService.cs`](file:///Users/yasminap/QuilvianV2/NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Billing/Services/IAdministrationFeeCalculationService.cs)
- Menambahkan deklarasi metode evaluasi baca-saja:
  ```csharp
  Task<ReferredAdminReplacementResult> ResolveReferredAdminReplacementAsync(
      BilInvoice invoice,
      RegPatientEncounter encounter,
      CancellationToken cancellationToken = default);
  ```

#### 4. [`AdministrationFeeCalculationService.cs`](file:///Users/yasminap/QuilvianV2/NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Billing/Services/AdministrationFeeCalculationService.cs)
- Menghentikan potongan flat Rp 50.000 pada `GetPriorAppliedAdminFeeAmountAsync` khusus untuk tipe layanan `RANAP`:
  ```csharp
  if (string.Equals(request.ServiceType, AdministrationFeeServiceTypes.Ranap, StringComparison.OrdinalIgnoreCase))
  {
      return (0m, int.MinValue);
  }
  ```
- Mengimplementasikan `ResolveReferredAdminReplacementAsync`:
  - **Untuk Rawat Jalan (`RAJAL`):** Mencari tautan aktif ke invoice rawat inap yang tidak dibatalkan. Jika ditemukan, menetapkan `IsSupersededByInpatientAdmission = true`, mencatat id invoice rawat inap, dan kode `SUPERSEDED_BY_INPATIENT_ADMISSION`.
  - **Untuk Rawat Inap (`RANAP`):** Mencari seluruh tagihan rawat jalan asal yang tertaut aktif. Membaca porsi administrasi yang dibayar pasien dari snapshot rincian kalkulasi versi terakhir (`AppliedAmount - PrimaryAmount - UnresolvedAmount - NonBillableResidualAmount`).
  - Menerapkan perlakuan status invoice asal:
    - `OPEN`: perlakuan `SUPERSEDED`, kredit Rp 0.
    - `FINAL` / `CLOSED`: perlakuan `CREDITED`, kredit sebesar porsi administrasi pasien.
    - `SETTLED_BY_WRITE_OFF`: perlakuan `CREDITED`, kredit sebesar nilai terkecil antara porsi administrasi pasien dan alokasi pembayaran bersih yang masuk.

#### 5. [`BillingCalculationService.cs`](file:///Users/yasminap/QuilvianV2/NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Billing/Services/BillingCalculationService.cs)
- Menyesuaikan kondisi evaluasi kalkulasi administrasi pada line 203 agar selalu mengevaluasi tautan aktif pada tagihan `Ranap` dan `Rajal` meskipun item tindakan belum ada.
- Menghubungkan pemanggilan `ResolveReferredAdminReplacementAsync` di dalam `CalculateAdministrationFeeAsync`.
- Menetapkan `AppliedAmount = 0m` dan `NonBillableResidualAmount = 0m` jika biaya administrasi rajal digantikan oleh rawat inap (`IsSupersededByInpatientAdmission == true`).
- Pada `CalculateAsync`, mengurangkan total kredit administrasi dari porsi pasien setelah promo/diskon (`:314`), dengan batasan bahwa porsi pasien tidak boleh menjadi negatif.
- Menyimpan nilai kredit terpakai (`ReferredOutpatientAdminCreditAmount`) dan sisa kredit belum terserap (`ReferredOutpatientAdminUnappliedCreditAmount`) pada entitas `BilCalculationVersion` saat kalkulasi disimpan permanen.

---

## 2. Spesifikasi Endpoint & Kontrak Response DTO (`[Tags("Billing Calculation")]`)

Perubahan kontrak kalkulasi ini berlaku pada endpoint kalkulasi pratinjau dan kalkulasi permanen:
- `POST /api/health-services/billing/invoices/{invoiceId}/preview-calculation`
- `POST /api/health-services/billing/invoices/{invoiceId}/recalculate`

### Struktur Response DTO Komponen Biaya Administrasi (`AdministrationFeeCalculationResponse`)

| Field | Tipe Data | Deskripsi | Contoh Nilai |
| --- | --- | --- | --- |
| `policyCode` | `string` | Kode kebijakan tarif administrasi yang berlaku | `"POL-ADM-RANAP-01"` |
| `policyAmount` | `decimal` | Nilai tarif administrasi standar menurut kebijakan | `150000.00` |
| `priorAppliedAmount` | `decimal` | Nilai administrasi terdahulu yang diperhitungkan (selalu `0` untuk Ranap) | `0.00` |
| `appliedAmount` | `decimal` | Nilai biaya administrasi yang dikenakan pada tagihan ini (`0` jika digantikan) | `0.00` atau `150000.00` |
| `isSupersededByInpatientAdmission` | `boolean` | `true` jika administrasi tagihan poli ini digantikan oleh rawat inap | `true` |
| `supersededByInpatientInvoiceId` | `UUID?` | ID tagihan rawat inap yang menggantikan administrasi poli ini | `"b6f2b842-..."` |
| `supersededReasonCode` | `string?` | Kode alasan penggantian resmi | `"SUPERSEDED_BY_INPATIENT_ADMISSION"` |
| `referredOutpatientAdminCreditedAmount` | `decimal` | Total kredit administrasi poli yang terserap pada tagihan ranap | `50000.00` |
| `referredOutpatientAdminUnappliedCreditAmount` | `decimal` | Sisa kredit administrasi poli yang belum terserap porsi pasien ranap | `0.00` |
| `referredOutpatientAdminOrigins` | `Array` | Rincian seluruh tagihan rawat jalan asal yang tertaut | *Lihat tabel origin di bawah* |

#### Rincian Sub-Elemen `referredOutpatientAdminOrigins[]`

| Field | Tipe Data | Deskripsi | Contoh Nilai |
| --- | --- | --- | --- |
| `originInvoiceId` | `UUID` | ID tagihan rawat jalan asal | `"a1b2c3d4-..."` |
| `originInvoiceNumber` | `string` | Nomor tagihan rawat jalan asal | `"INV-RJ-202610-0012"` |
| `originEncounterId` | `UUID` | ID kunjungan rawat jalan asal | `"e5f6a7b8-..."` |
| `originInvoiceStatus` | `string` | Status tagihan rawat jalan asal (`OPEN`, `CLOSED`, dll.) | `"CLOSED"` |
| `treatment` | `string` | Perlakuan administrasi (`SUPERSEDED`, `CREDITED`, `NO_ADMIN`) | `"CREDITED"` |
| `adminPatientAmount` | `decimal` | Porsi biaya administrasi yang dibayar pasien pada tagihan poli | `50000.00` |
| `creditAmount` | `decimal` | Nominal kredit yang disumbangkan ke rawat inap | `50000.00` |

---

## 3. Matriks Pemenuhan Acceptance Criteria

| ID Pengujian | Deskripsi Skenario | Bukti Pemenuhan pada Kode | Status |
| :--- | :--- | :--- | :---: |
| `BIL-AT-204` | Pasien poli ditransfer resmi; tagihan poli masih `OPEN`, lalu dihitung | `ResolveReferredAdminReplacementAsync` mendeteksi tautan aktif ke invoice ranap non-batal; menetapkan `AppliedAmount = 0`, `isSupersededByInpatientAdmission = true`, kode `SUPERSEDED_BY_INPATIENT_ADMISSION`. Di ranap: `treatment = SUPERSEDED`, kredit Rp 0. | ✅ Terpenuhi |
| `BIL-AT-205` | Pasien poli ditransfer resmi; tagihan poli sudah `CLOSED` dengan admin Rp 50.000 | Sistem membaca porsi admin pasien poli dari snapshot rincian kalkulasi; memberikan kredit Rp 50.000 ke tagihan ranap; mengurangi porsi pasien ranap; menyimpan ke `ReferredOutpatientAdminCreditAmount`. | ✅ Terpenuhi |
| `BIL-AT-206` | Tagihan poli `FINAL` dengan piutang belum dibayar | Memberikan kredit penuh di ranap; sisa piutang poli tetap ada dan tidak diubah secara sepihak. | ✅ Terpenuhi |
| `BIL-AT-207` | Tagihan poli `SETTLED_BY_WRITE_OFF` dengan alokasi bersih Rp 60.000 | Mengambil nilai terkecil antara porsi administrasi dan alokasi pembayaran bersih yang tercatat (`Math.Min(adminPatientAmount, paidAmount)`). | ✅ Terpenuhi |
| `BIL-AT-208` | Pasien berobat kemarin di poli untuk kasus terpisah (tidak ada tautan rujukan aktif), lalu hari ini masuk ranap | `GetPriorAppliedAdminFeeAmountAsync` untuk `RANAP` mengembalikan `(0m, int.MinValue)`; biaya administrasi ranap dikenakan utuh tanpa potongan Rp 50.000 semu. Regresi F12 tuntas. | ✅ Terpenuhi |
| `BIL-AT-211` | Tagihan poli dihitung ulang sesudah tautan lahir (admin Rp 0) lalu dibayar; ranap dihitung | Porsi administrasi pasien pada snapshot versi kalkulasi terkini poli bernilai 0, sehingga ranap menerima kredit Rp 0 (tidak dobel klaim kredit). | ✅ Terpenuhi |
| `BIL-AT-212` | Tagihan ranap dibatalkan; tagihan poli dihitung ulang | Tautan aktif mengecualikan invoice ranap dengan `Status == Cancelled` atau `IsCancel == true`. Admin poli kembali normal dan `isSupersededByInpatientAdmission = false`. | ✅ Terpenuhi |
| `BIL-AT-213` | Dua tagihan poli pada hari yang sama tanpa rawat inap | Perhitungan administrasi poli harian (`RAJAL`/`IGD`/`OTC`) tetap berjalan as-is tanpa perubahan. | ✅ Terpenuhi |
| `BIL-AT-230` | Tagihan memuat item tindakan medis dengan kata "Administrasi" (misal "Administrasi Kamar Operasi") | Item tindakan medis tetap dihitung penuh sebagai revenue tindakan. Adendum `BKC-DES-092` telah dicabut (`BKC-DEC-167`), kata pada deskripsi tidak mempengaruhi kalkulasi biaya administrasi. | ✅ Terpenuhi |

---

## 4. Catatan Komunikasi untuk Petugas Kasir & Billing

Sebelum pembaruan backend ini dirilis ke lingkungan produksi (*production*), tiga perubahan perilaku operasional berikut **WAJIB** disosialisasikan kepada Supervisor Kasir, Staf Kasir Rawat Jalan, dan Staf Kasir Rawat Inap:

1. **Biaya Administrasi Rawat Inap Utuh Tanpa Potongan Rp 50.000 Otomatis:**
   - Kasir ranap tidak akan lagi melihat potongan otomatis Rp 50.000 hanya karena pasien pernah berkunjung ke poliklinik pada hari kemarin atau hari ini, **kecuali** jika pasien tersebut memang dirujuk resmi melalui alur transfer rawat jalan ke rawat inap.
2. **Biaya Administrasi Poliklinik Menjadi Rp 0 untuk Pasien yang Dirujuk ke Rawat Inap:**
   - Ketika dokter merujuk pasien dari poliklinik ke rawat inap dan admisi menerbitkan transfer resmi, tagihan rawat jalan yang belum dibayar akan otomatis menghapus biaya administrasinya (menjadi Rp 0). Kasir poliklinik cukup menagihkan tindakan medis atau obat yang telah diberikan.
3. **Kredit Pengurang Otomatis pada Tagihan Rawat Inap Jika Pasien Sudah Terlanjur Membayar Poli:**
   - Jika keluarga pasien sudah melunasi tagihan poliklinik beserta administrasinya sebelum diputuskan opname, biaya administrasi poliklinik yang telah dibayarkan tersebut akan langsung muncul sebagai kredit pengurang (*Referred Outpatient Admin Credit*) pada rincian tagihan rawat inap pasien. Kasir rawat inap dapat menjelaskan kepada keluarga pasien bahwa uang administrasi poliklinik tidak hilang melainkan telah memotong tagihan rawat inapnya.

