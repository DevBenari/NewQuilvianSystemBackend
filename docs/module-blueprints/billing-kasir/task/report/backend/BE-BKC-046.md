# Laporan Perubahan Backend — `BE-BKC-046`

## Metadata

| Field | Nilai |
| --- | --- |
| **Task ID** | `BE-BKC-046` |
| **Judul** | Konteks Payer Eksplisit pada Mesin Tanggungan (*Explicit Payer Context on Coverage Engine*) |
| **Slice / Milestone** | `MVP-17` (Penggantian penanggung kunjungan sebelum pembayaran / Ganti Payer) |
| **Roadmap** | `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` baris 1789 |
| **Trace** | `FR-BKC-070`; `MPY-DES-005`; `CAP-34` |
| **Contract Version** | `BIL-CALCULATION-0.9` / `REGISTRATION-COVERAGE-ADAPTER-2` / internal extension |
| **Dependency** | Tidak ada |
| **Klasifikasi** | `CORE ENGINE EXTENSION / REUSE WITH ADAPTER` (Perluasan internal mesin tanggungan tanpa perubahan skema basis data) |
| **Task Mode** | `TASK MODE: BACKEND` |
| **Target Tulis** | `NewQuilvianSystemBackend`, branch `Yasmina` |
| **Model** | Gemini 3.8 Flash |
| **Tanggal** | 11 September 2026 |
| **Status** | ✅ `Selesai` (Mesin tanggungan asuransi dan penjamin perusahaan dapat mengevaluasi payer kandidat secara murni tanpa menyentuh penjamin kunjungan persistent; 100% kompatibel ke belakang dengan pemanggil existing; nol operasi tulis pada basis data) |

### Backend Governance Preflight

| Parameter | Nilai |
| --- | --- |
| **Area** | `HealthServices` |
| **Module / Submodule** | `BillingManagement / Billing` & `ClinicalManagement` |
| **Owner / Prefix Registry** | Prefix `BIL-` (`HealthServices / BillingManagement / Billing`) & `CLN-` (`HealthServices / ClinicalManagement`) |
| **Keberlakuan** | `TOUCHED LEGACY & NEW ENGINE CAPABILITY` |
| **QBE ID yang Berlaku** | `QBE-MOD-002`, `QBE-CON-001`, `QBE-INT-001` |
| **Pengecualian / Catatan** | Task ini **tidak menambahkan endpoint publik baru**, melainkan memperluas kemampuan internal mesin kalkulasi tanggungan agar dapat menerima parameter konteks payer kandidat opsional (`CandidatePayerContext`). Seluruh pemanggilan existing dijamin berjalan identik tanpa perubahan tanda tangan maupun perilaku. |

---

## 1. Masalah & Latar Belakang Bisnis

### 1.1 Kebutuhan Pratinjau Perbandingan Payer (`FR-BKC-070`, `MPY-DEC-003`)
Dalam modul kasir rumah sakit, fitur **Ganti Payer** mengharuskan kasir dapat membandingkan dampak biaya tagihan antara penanggung aktif saat ini dengan penanggung kandidat (misalnya berganti dari pasien Tunai ke kartu Asuransi Prudential, atau ke Penjamin Perusahaan PT Telkom) **sebelum kasir memutuskan untuk menyimpan perubahan tersebut**:
- **Contoh Skenario Rumah Sakit (Data Samaran):**
  Pasien **Tn. B** didaftarkan sebagai pasien Tunai dengan total tagihan obat dan tindakan sebesar Rp 2.500.000. Sebelum membayar, pasien mengajukan kartu asuransi kesehatannya.
  Kasir membuka layar perbandingan tanggungan. Sistem harus menampilkan simulasi:
  - Berapa porsi yang akan ditanggung asuransi jika kartu tersebut dipakai? (misal: Rp 2.000.000)
  - Berapa sisa yang harus dibayar mandiri oleh pasien sebagai *excess* / *co-payment*? (misal: Rp 500.000)
  - Apakah ada item yang *NotCovered*?
- **Prinsip Utama (`FR-BKC-070`):**
  Proses perbandingan dan pratinjau ini **sama sekali tidak boleh menyimpan apa pun ke basis data**. Data kunjungan pasien di pendaftaran tidak boleh berubah, tidak boleh ada versi kalkulasi resmi yang lahir, dan *row version* tagihan tidak boleh bertambah sampai kasir secara sadar mengeksekusi perintah simpan penggantian penanggung.

### 1.2 Keterbatasan Mesin Tanggungan Sebelumnya (`CAP-34` — Reuse with Adapter)
- Mesin kalkulasi tanggungan (`RegistrationBillingCoverageAdapter`) dan layanan advisory klinis (`InsuranceCoverageService`, `CompanyGuarantorCoverageService`) sebelumnya **hanya dapat membaca payer dari `EncounterId`**.
- Adapter selalu mengambil satu-satunya baris penjamin aktif di `RegPatientEncounterGuarantor` milik kunjungan.
- Tidak tersedia parameter maupun jalur untuk mengevaluasi aturan tanggungan terhadap kartu asuransi pasien (`MstPatientInsurance`) atau kartu penjamin perusahaan (`MstPatientCompanyGuarantor`) yang bukan merupakan penjamin aktif kunjungan tersebut.

---

## 2. Solusi Teknis & Arsitektur yang Diterapkan (`MPY-DES-005`)

Untuk memenuhi kebutuhan pratinjau perbandingan tanpa efek samping, dibangun perluasan bertingkat pada lapisan tanggungan:

```text
[BillingInvoicesController (BE-BKC-047)]
        │
        ▼
[BillingCalculationService.PreviewCandidateCalculationAsync]
        │  (persist: false, AsNoTracking, zero-audit, zero-version-bump)
        ▼
[RegistrationBillingCoverageAdapter.ResolveAsync]
        │
        ├── CandidatePayer == null ──► Jalur Persistent Existing (ResolveInsurance / ResolveCompanyGuarantor)
        │
        └── CandidatePayer != null ──► Jalur Kandidat Eksplisit
                 │
                 ├── Cash ──────────────► SelfPay() (PayerKind: "CASH")
                 │
                 ├── Insurance ─────────► ResolveCandidateInsuranceAsync
                 │                              │
                 │                              ├── EncounterInsuranceService.GetCandidateContextAsync
                 │                              └── CalculateInsuranceCoverageDecision (DRY Shared Logic)
                 │
                 └── CompanyGuarantor ──► ResolveCandidateCompanyGuarantorAsync
                                                │
                                                ├── Validasi MstPatientCompanyGuarantor & MstCompanyGuarantor
                                                └── CompanyGuarantorCoverageService.ResolveCandidateCoverageAsync
```

### 2.1 Perubahan pada `BillingCoverageAdapter.cs`
1. **Definisi Record Baru `CandidatePayerContext`:**
   ```csharp
   public sealed record CandidatePayerContext(
       EncounterPaymentType PaymentType,
       Guid? PaymentMethodId = null,
       Guid? PatientInsuranceId = null,
       Guid? PatientCompanyGuarantorId = null);
   ```
2. **Perluasan `BillingCoverageContext`:**
   Menambahkan properti opsional `CandidatePayerContext? CandidatePayer = null` beserta konstruktor kompatibilitas penuh untuk pemanggil existing:
   ```csharp
   public sealed record BillingCoverageContext(
       Guid InvoiceId,
       Guid EncounterId,
       DateTimeOffset CalculatedAt,
       decimal EligibleAmount,
       IReadOnlyList<BillingCoverageComponent> Components,
       CandidatePayerContext? CandidatePayer = null)
   {
       public BillingCoverageContext(
           Guid invoiceId,
           Guid encounterId,
           DateTimeOffset calculatedAt,
           decimal eligibleAmount,
           IReadOnlyList<BillingCoverageComponent> components)
           : this(invoiceId, encounterId, calculatedAt, eligibleAmount, components, null)
       {
       }
   }
   ```
3. **Injeksi `EncounterInsuranceService` pada `RegistrationBillingCoverageAdapter`:**
   Didaftarkan ke constructor adapter untuk membangun konteks kartu asuransi kandidat secara terisolasi.
4. **Percabangan `ResolveAsync`:**
   Jika `context.CandidatePayer != null`, adapter langsung mengeksekusi `ResolveCandidatePayerAsync` tanpa menyentuh tabel `RegPatientEncounterGuarantor`.
5. **Ekstraksi Logika Perhitungan Bersama (`CalculateInsuranceCoverageDecision`):**
   Logika pencocokan rule, batas per kunjungan, akumulasi *non-billable residual*, dan pemetaan outcome komponen diekstraksi ke metode statis privat murni, sehingga jalur penjamin aktif dan jalur kandidat memakai formula kalkulasi yang 100% identik.
6. **Resolusi Kandidat Penjamin Perusahaan (`ResolveCandidateCompanyGuarantorAsync`):**
   Memvalidasi kartu kandidat (`MstPatientCompanyGuarantor`), status aktif, masa berlaku polis, kontrak perusahaan, dan kesesuaian kepemilikan pasien secara *read-only*, lalu meneruskannya ke `CompanyGuarantorCoverageService.ResolveCandidateCoverageAsync`.

### 2.2 Perubahan pada `BillingCalculationService.cs`
1. **Metode Baru `PreviewCandidateCalculationAsync`:**
   ```csharp
   public async Task<CalculationResponse> PreviewCandidateCalculationAsync(
       Guid invoiceId,
       CandidatePayerContext candidatePayer,
       Guid actorUserId,
       CancellationToken cancellationToken)
   ```
   Metode ini memanggil `CalculateAsync` dengan parameter `persist: false` dan meneruskan `candidatePayer`.
2. **Karakteristik Pratinjau Bersih (Zero-Side-Effects):**
   - Menggunakan `AsNoTracking()` pada pembacaan tagihan dan data master.
   - Tidak mengakuisisi *distributed lock*.
   - Tidak menambahkan baris ke `BilCalculationVersions`.
   - Tidak mengubah `invoice.RowVersion` maupun `invoice.UpdateDateTime`.
   - Tidak memanggil `_dbContext.SaveChangesAsync`.
   - Tidak menerbitkan log audit ke `LoggerService`.
   - Mengembalikan nomor versi kalkulasi yang sedang aktif (*running version*).

### 2.3 Perubahan pada `EncounterInsuranceService.cs`
- Menambahkan metode `GetCandidateContextAsync(Guid encounterId, Guid patientInsuranceId, DateTime? serviceDate, CancellationToken cancellationToken)`.
- Mengonstruksi `EncounterInsuranceContext` valid/tidak valid berbasis kartu asuransi kandidat `MstPatientInsurance` tanpa mengubah data relasi kunjungan.

### 2.4 Perubahan pada `InsuranceCoverageService.cs`
- Menambahkan parameter opsional `EncounterInsuranceContext? explicitContext = null` pada metode `ResolveDrugAsync`, `ResolveProcedureAsync`, dan `ResolveTariffAsync`.

### 2.5 Perubahan pada `CompanyGuarantorCoverageService.cs`
- Menambahkan metode `ResolveCandidateCoverageAsync(BillingCoverageContext context, MstPatientCompanyGuarantor candidateCard, RegPatientEncounter encounter, CancellationToken cancellationToken)`.
- Mengekstrak metode `CalculateCoverageDecision(BillingCoverageContext context, List<MstCompanyGuarantorCoverageRule> rules)`.
- Menambahkan parameter opsional `MstPatientCompanyGuarantor? explicitCandidateCard = null` pada metode `ResolveTariffAsync` untuk keperluan advisory per item.

---

## 3. Matriks Penanganan Anomali Data Payer Kandidat

Jika kartu kandidat yang dipilih memiliki masalah integritas data atau masa berlaku, sistem mengembalikan keputusan terstruktur yang membebankan komponen ke pasien sementara dengan anomali yang informatif bagi kasir:

| Kondisi Kartu Kandidat | Kode Anomali | Pesan Penjelas untuk Kasir |
| :--- | :--- | :--- |
| Kartu asuransi belum dipilih | `INSURANCE_PROVIDER_MISSING` | Kartu asuransi pasien kandidat belum dipilih. Seluruh biaya untuk sementara dibebankan ke pasien. |
| Kartu penjamin perusahaan belum dipilih | `COMPANY_GUARANTOR_MISSING` | Kartu penjamin perusahaan kandidat belum dipilih. Seluruh biaya untuk sementara dibebankan ke pasien. |
| Kartu tidak ditemukan pada basis data | `COMPANY_GUARANTOR_MISSING` / `INSURANCE_PROVIDER_MISSING` | Data kartu kandidat tidak ditemukan atau tidak aktif. |
| Polis belum mulai berlaku pada tanggal pelayanan | `POLICY_INACTIVE` | Kartu/polis penjamin kandidat belum mulai berlaku pada tanggal pelayanan. |
| Polis sudah kedaluwarsa pada tanggal pelayanan | `POLICY_INACTIVE` | Kartu/polis penjamin kandidat sudah berakhir pada tanggal pelayanan. |
| Kartu berstatus non-aktif | `POLICY_INACTIVE` | Polis atau kartu penjamin kandidat tercatat tidak aktif. |
| Kartu belum berstatus eligible | `PAYER_NOT_ELIGIBLE` | Penjamin kandidat belum dinyatakan layak (eligible). |
| Kartu bukan milik pasien pada kunjungan | `PAYER_NOT_ELIGIBLE` | Kartu penjamin kandidat bukan milik pasien pada kunjungan ini. |
| Encounter tidak ditemukan | `ENCOUNTER_NOT_FOUND` | Data kunjungan tidak ditemukan saat memeriksa penjamin. |

---

## 4. Berkas yang Diubah & Ditambahkan

| File | Status | Keterangan |
| :--- | :--- | :--- |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingCoverageAdapter.cs` | Diperbarui | Menambahkan `CandidatePayerContext`, field `CandidatePayer` pada context, injeksi `EncounterInsuranceService`, resolver candidate payer per jenis, dan ekstraksi `CalculateInsuranceCoverageDecision`. |
| `Areas/HealthServices/BillingManagement/Billing/Services/BillingCalculationService.cs` | Diperbarui | Menambahkan metode `PreviewCandidateCalculationAsync` dan parameter `candidatePayer` pada `CalculateAsync` untuk diteruskan ke adapter. |
| `Areas/HealthServices/ClinicalManagement/Services/EncounterInsuranceService.cs` | Diperbarui | Menambahkan metode `GetCandidateContextAsync` untuk memvalidasi kartu asuransi kandidat secara read-only. |
| `Areas/HealthServices/ClinicalManagement/Services/InsuranceCoverageService.cs` | Diperbarui | Menambahkan parameter opsional `explicitContext` pada `ResolveDrugAsync`, `ResolveProcedureAsync`, dan `ResolveTariffAsync`. |
| `Areas/HealthServices/ClinicalManagement/Services/CompanyGuarantorCoverageService.cs` | Diperbarui | Menambahkan metode `ResolveCandidateCoverageAsync`, perbaikan nama penjamin, dan parameter opsional `explicitCandidateCard` pada `ResolveTariffAsync`. |

---

## 5. Ringkasan Verifikasi

| Butir Verifikasi | Kriteria Sukses | Status |
| :--- | :--- | :--- |
| **Kompatibilitas Pemanggil Existing** | Seluruh pemanggil `RegistrationBillingCoverageAdapter`, `InsuranceCoverageService`, dan `BillingCalculationService` berjalan normal tanpa perubahan parameter. | ✅ Terpenuhi |
| **Zero Side-Effects pada Pratinjau** | Pemanggilan `PreviewCandidateCalculationAsync` murni membaca data (`AsNoTracking`), tidak ada row `BilCalculationVersion` baru, dan tidak mengubah `RowVersion` invoice. | ✅ Terpenuhi |
| **Pemisahan Jalur Payer Kandidat** | Kartu kandidat Tunai, Asuransi, maupun Penjamin Perusahaan dievaluasi sesuai aturan masing-masing tanpa menyentuh data pendaftaran kunjungan. | ✅ Terpenuhi |
| **Pencegahan Anomali Palsu** | Evaluasi penjamin perusahaan kandidat tidak memicu pengecekan `InsuranceProviderId`, dan sebaliknya. | ✅ Terpenuhi |

---

## 6. Langkah Selanjutnya

- Pengerjaan task `BE-BKC-047`: Konteks layar edit, pratinjau perbandingan, dan perintah ganti payer (`GET /{id}/edit-context`, `POST /{id}/payer-comparison-preview`, dan `PUT /{id}/payment-source`).
