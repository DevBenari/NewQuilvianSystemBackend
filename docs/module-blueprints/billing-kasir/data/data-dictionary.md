# Billing dan Kasir — Kamus Data

> Revision `0.7`, status **draft**. Berkas ini adalah lokasi kamus data menurut struktur keluaran blueprint yang berlaku (`data/data-dictionary.md`). Modul ini dibangun sebelum struktur itu ditetapkan, sehingga **kamus data baseline masih tinggal di [`../erd/data-dictionary.md`](../erd/data-dictionary.md)** beserta lima berkas ERD di sebelahnya.

## Cara membaca dua berkas ini

| Yang dicari | Dibaca di |
| --- | --- |
| Seluruh kolom tabel `Bil*` dan empat master policy, beserta kunci, index, dan skema DDL | [`../erd/data-dictionary.md`](../erd/data-dictionary.md) |
| Kolom milik modul lain yang dibaca modul ini (`MstInsuranceProvider`, `TrxPatientEncounterGuarantor`) | [`../erd/data-dictionary.md`](../erd/data-dictionary.md) § Amendment 3 September 2026 |
| Perubahan kolom dan kontrak data amendment 4 September 2026 | **Berkas ini** |

Pemindahan isi baseline dari `erd/` ke `data/` adalah perubahan struktur yang menyentuh rujukan pada belasan berkas lain. Ia **MUST** dikerjakan sebagai revisi tersendiri oleh `/manage-module-blueprint`, bukan sebagai efek samping pass desain — lihat `BKC-OQ-089`. Sampai itu terjadi, kedua berkas berlaku bersama dan tidak boleh saling menyalin isi.

## Warisan `IdentityModel` — ditulis sekali, tidak diulang per tabel

Seluruh tabel operasional pada modul ini mewarisi `QuilvianSystemBackend.Models.IdentityModel`, yang membawa sepuluh kolom: `CreateDateTime`, `CreateBy`, `UpdateDateTime`, `UpdateBy`, `DeleteDateTime`, `DeleteBy`, `CancelDateTime`, `CancelBy`, `IsCancel`, dan `IsDelete`. Kesepuluhnya **tidak** diulang pada tabel mana pun di bawah, dan **tidak** muncul pada skema DDL dokumentasi.

Konsekuensi yang mengikat desain: penghapusan bersifat **penandaan** (`IsDelete`), bukan penghapusan sungguhan. Tidak ada aturan pada modul ini yang boleh mengandalkan baris benar-benar hilang dari tabel.

---

# Amendment 4 September 2026 — Anomali data penjamin dan gerbang PPN

Input: `BKC-DEC-070`–`079` (approved 4 September 2026), keputusan arsitektur `BKC-DES-010`–`020` pada [`../02-backend-architecture.md`](../02-backend-architecture.md).

## Ringkasan: tidak ada tabel dan tidak ada kolom yang berubah

| Kelompok | Jumlah |
| --- | ---: |
| Tabel baru | 0 |
| Tabel yang berubah skemanya | 0 |
| Kolom baru | 0 |
| Kolom yang berubah tipe, kunci, atau nullability | 0 |
| Migration yang dibutuhkan | 0 |

Seluruh field baru amendment ini hidup di **record kontrak antar-service** dan di **DTO response** yang ikut terserialisasi sebagai JSON ke dalam kolom `BilCalculationVersion.BreakdownSnapshot` yang sudah ada. Kolom itu bertipe `string`; bentuk kolomnya tidak berubah, isinya saja yang menjadi lebih kaya (`BKC-DES-020`).

## Tabel yang tersentuh — status dan kepemilikan

| Tabel | Status | Modul pemilik | Cara modul ini memakainya |
| --- | --- | --- | --- |
| `BilInvoice` | `Sudah ada` | Billing dan Kasir | Dibaca. Kolom `ServiceType` menjadi sumber gerbang PPN (`BKC-DES-018`) |
| `BilInvoiceItem` | `Sudah ada` | Billing dan Kasir | Dibaca. Kolom `CategoryId` menentukan `IsPharmacy` lewat `MstTariffCategory` |
| `BilCalculationVersion` | `Sudah ada` | Billing dan Kasir | Ditulis. Kolom `BreakdownSnapshot` menampung field baru |
| `MstTariffCategory` | `Sudah ada` | Health Services Master Data | Dibaca. Kolom `IsPharmacy` menentukan basis pajak |
| `MstTaxRule` | `Sudah ada` | Billing Master Data | Dibaca. Kolom `AllocationRule` menentukan pembagian PPN antar-payer |
| `MstInsuranceCoverageRule` | `Sudah ada` | Health Services Master Data | Dibaca lewat adapter. Empat kolom **berhenti dibaca**, satu kolom tetap dibaca |
| `TrxPatientEncounter` | `Sudah ada` | Registration Management | Dibaca. Kolom `EncounterType` **tidak** dibaca langsung sebagai gerbang PPN — hanya lewat snapshot `BilInvoice.ServiceType` |
| `TrxPatientEncounterGuarantor` | `Sudah ada` | Registration Management | Dibaca. Tiga kolom menentukan ada tidaknya anomali data |

## Kolom kunci pada tabel `Sudah ada` yang dipakai aturan bisnis amendment ini

Hanya kolom yang benar-benar dipakai aturan modul ini yang didaftar; kolom lain ada di berkas model masing-masing.

### `BilInvoice` — `Areas/HealthServices/BillingManagement/Billing/Models/BilInvoice.cs`

| Kolom | Tipe | Wajib | Kunci/index | Sensitif | Peran pada amendment ini |
| --- | --- | :---: | --- | :---: | --- |
| `Id` | `Guid` | Ya | PK | Tidak | — |
| `EncounterId` | `Guid` | Ya | FK, UK | **Ya** | Menghubungkan ke kunjungan dan penjaminnya |
| `ServiceType` | `string` | Ya | — | Tidak | **Sumber gerbang PPN.** Nilainya `RAJAL`, `IGD`, `RANAP`, `MCU`, atau `TELEMEDICINE`, diisi `MapServiceType(encounter.EncounterType)` saat invoice dibuka. Hanya `RANAP` yang membebaskan PPN (`BKC-DES-019`) |
| `Status` | `string` | Ya | index | Tidak | Hanya invoice `OPEN` yang dapat dihitung ulang |
| `CurrentCalculationVersion` | `int` | Ya | — | Tidak | Menunjuk versi kalkulasi yang berlaku |

### `BilCalculationVersion` — `Areas/HealthServices/BillingManagement/Billing/Models/BilCalculationVersion.cs`

| Kolom | Tipe | Wajib | Kunci/index | Sensitif | Peran pada amendment ini |
| --- | --- | :---: | --- | :---: | --- |
| `InvoiceId` + `VersionNo` | `Guid` + `int` | Ya | UK gabungan | Tidak | Satu versi kalkulasi per nomor |
| `PrimaryAmount` | `decimal(18,2)` | Ya | — | **Ya** | Total tanggungan penjamin. **Tidak berubah bentuknya**; nilainya bergeser naik karena gerbang approval dan limit bulanan dicabut (`BKC-DEC-071`) |
| `TaxAmount` | `decimal(18,2)` | Ya | — | **Ya** | Nilainya menjadi **nol** untuk seluruh invoice `RANAP` (`BKC-DEC-078`) |
| `PatientAmount` | `decimal(18,2)` | Ya | — | **Ya** | Nilainya bergeser turun untuk tagihan yang gerbangnya dicabut, dan bergeser naik untuk tagihan beranomali data |
| `BreakdownSnapshot` | `string` (JSON) | Tidak | — | **Ya** | **Menampung seluruh field baru amendment ini.** Immutable setelah dikunci; tidak di-backfill |
| `IsLocked` | `bool` | Ya | — | Tidak | Versi terkunci tidak pernah dihitung ulang |

### `MstInsuranceCoverageRule` — `Areas/HealthServices/MasterData/Models/MstInsuranceCoverageRule.cs`

| Kolom | Tipe | Bawaan | Sensitif | Peran pada amendment ini |
| --- | --- | --- | :---: | --- |
| `CoverageStatus` | `string` | `"Covered"` | Tidak | `"NotCovered"` tetap menjadi tanggungan pasien (`BKC-DEC-072`). `"NeedApproval"` **berhenti menahan** perhitungan (`BKC-DEC-071`) |
| `CoveragePercent` | `decimal` | — | Tidak | Tetap dipakai `CalculateCoveredAmount`, tidak berubah (`BKC-DEC-070`) |
| `CoPaymentPercent`, `CoPaymentAmount` | `decimal?` | `null` | Tidak | Tetap dipakai, tidak berubah |
| `MaxCoverageAmount` | `decimal?` | `null` | Tidak | Tetap dipakai sebagai batas atas, tidak berubah |
| `MaxQuantityPerVisit`, `MaxAmountPerVisit` | `int?`, `decimal?` | `null` | Tidak | **Tetap berlaku.** Yang dicabut adalah limit bulanan, bukan limit per kunjungan |
| `IsAllowExcessPaymentByPatient` | `bool` | **`true`** | Tidak | **Tetap dipakai runtime** (`BKC-DEC-074`). `true` → residual ditagihkan ke pasien; `false` → residual masuk `UnresolvedAmount` dan tampil sebagai "Selisih Tidak Ditagihkan" |
| `IsNeedApproval` | `bool` | `false` | Tidak | **Berhenti dibaca** `RegistrationBillingCoverageAdapter` (`BKC-DEC-071`). Kolomnya **MUST NOT** dihapus — masih dibaca `InsuranceCoverageService` untuk badge advisory |
| `IsNeedGuaranteeLetter` | `bool` | `false` | Tidak | Sama seperti `IsNeedApproval` |
| `MaxAmountPerMonth` | `decimal?` | `null` | Tidak | **Berhenti dibaca** oleh perhitungan tagihan (`BKC-DEC-071`). Kolomnya tetap ada |
| `MaxQuantityPerMonth` | `int?` | `null` | Tidak | Sama seperti `MaxAmountPerMonth` |
| `RuleCode`, `RuleName`, `ApprovalInstruction`, `BillingInstruction` | `string` | — | Tidak | **MUST NOT** diekspos ke DTO publik — isi kesepakatan komersial RS–asuransi |

### `MstTaxRule` — `Areas/HealthServices/BillingManagement/MasterData/Models/MstTaxRule.cs`

| Kolom | Tipe | Nilai yang sah | Sensitif | Peran pada amendment ini |
| --- | --- | --- | :---: | --- |
| `Code` | `string` | — | Tidak | Disebut dalam pesan galat bila ada lebih dari satu rule aktif |
| `Rate` | `decimal` | — | Tidak | Tarif PPN, contoh `11` |
| `RoundingMode` | `string` | `HALF_UP`, `HALF_EVEN`, `UP`, `DOWN` | Tidak | Tidak berubah |
| `AllocationRule` | `string` | `PROPORTIONAL`, `PATIENT`, `GUARANTOR` | Tidak | **MUST bernilai `PROPORTIONAL`** menurut `BKC-DEC-077`. Nilai lain menghasilkan alokasi PPN yang salah tanpa peringatan apa pun. Ini koreksi **data**, bukan kode (`BKC-DES-020`) |
| `IsActive` + `EffectiveFrom`/`EffectiveTo` | `bool`, `DateTimeOffset` | — | Tidak | **Tepat satu** baris boleh berlaku pada satu waktu; dua baris menghentikan seluruh kalkulasi |

### `TrxPatientEncounterGuarantor` — `Areas/HealthServices/RegistrationManagement/Models/TrxPatientEncounterGuarantor.cs`

| Kolom | Tipe | Sensitif | Peran pada amendment ini |
| --- | --- | :---: | --- |
| `EncounterId` | `Guid` | **Ya** | Kunci pencarian penjamin kunjungan |
| `PaymentType` | enum | Tidak | `Cash` → jalur `SelfPay`, tanpa anomali |
| `IsEligible` | `bool` | Tidak | `false` → anomali `PAYER_NOT_ELIGIBLE` |
| `IsPolicyActive` | `bool` | Tidak | `false` → anomali `POLICY_INACTIVE` |
| `InsuranceProviderId` | `Guid?` | Tidak | Kosong padahal bukan tunai → anomali `INSURANCE_PROVIDER_MISSING` |
| `BenefitPlanCodeSnapshot` | `string?` | Tidak | Penyaring aturan coverage, tidak berubah |
| `PolicyNumberSnapshot`, `MemberNumberSnapshot` | `string?` | **Ya** | Tidak dipakai amendment ini; **MUST NOT** masuk payload log |

## Kontrak data di dalam `BreakdownSnapshot`

Bagian ini adalah **dokumentasi bentuk JSON**, bukan skrip yang dijalankan dan bukan skema tabel. Ia tidak pernah menjadi DDL karena isinya tinggal di dalam satu kolom `string`.

### Field baru pada `breakdown.items[]`

| Field | Tipe JSON | Bawaan | Sensitif | Arti |
| --- | --- | --- | :---: | --- |
| `itemDataAnomalyAmount` | angka | `0` | **Ya** | Rupiah pokok yang tidak dapat dinilai penjaminnya karena data pendaftaran bermasalah |
| `taxDataAnomalyAmount` | angka | `0` | **Ya** | Rupiah pajak yang tidak dapat dinilai penjaminnya karena data pendaftaran bermasalah |

### Field baru pada `breakdown.administrationFee` dan `breakdown.roomCharge`

| Field | Tipe JSON | Bawaan | Sensitif | Arti |
| --- | --- | --- | :---: | --- |
| `dataAnomalyAmount` | angka | `0` | **Ya** | Sama seperti di atas, untuk komponen yang bukan `BilInvoiceItem` |

### Field baru pada `breakdown.coverage`

| Field | Tipe JSON | Bawaan | Sensitif | Arti |
| --- | --- | --- | :---: | --- |
| `dataAnomalyAmount` | angka | `0` | **Ya** | Total rupiah beranomali pada tagihan ini |
| `hasDataAnomaly` | boolean | `false` | Tidak | Ada masalah data penjamin, walaupun nominalnya mungkin nol |
| `anomalyCodes` | daftar teks | `[]` | Tidak | Kode program; **MUST NOT** diterjemahkan |
| `anomalyMessages` | daftar teks | `[]` | Tidak | Kalimat siap tampil, sejajar indeksnya dengan `anomalyCodes` |
| `isPerItemAllocationAvailable` | boolean | `false` | Tidak | Rincian per baris pada snapshot ini boleh dipercaya |

**Kompatibilitas snapshot lama.** Seluruh field di atas bertipe nilai non-nullable dengan bawaan `0`, `false`, atau `[]`. Snapshot yang ditulis sebelum amendment ini tidak memuatnya, dan `JsonSerializerDefaults.Web` mengisinya dengan bawaan itu tanpa galat. Penanda `isPerItemAllocationAvailable` yang otomatis bernilai `false` pada snapshot lama adalah **satu-satunya** cara program membedakan "tanggungan penjamin memang nol" dari "rincian per baris belum pernah dihitung" (`BKC-DES-017`).

## Kolom sensitif — aturan yang berlaku untuk seluruh amendment ini

Kolom bertanda **Sensitif** di atas **MUST NOT** masuk custom logger, **MUST NOT** dipakai sebagai contoh berisi data asli, dan **MUST NOT** muncul pada payload galat. Yang **boleh** masuk log adalah `InvoiceId` dan `anomalyCodes`; keduanya tidak mengidentifikasi pasien maupun mengungkap isi polis.

Seluruh contoh berangka pada dokumentasi blueprint ini memakai data samaran.

---

Trace `BKC-DEC-070`–`079`, `BKC-DES-010`–`020`. Tests `BIL-AT-036`–`054`.

---

## Amendment lanjutan 4 September 2026 — Residual non-billable dirutekan ke write-off

Input: **`BKC-DEC-080`** beserta `BKC-DEC-036`; keputusan arsitektur `BKC-DES-021`–`025`. Status **draft**.

Berbeda dari amendment sebelumnya, amendment ini **menyentuh skema**. Sebabnya satu: nominal yang boleh ditulis-off harus dapat dibaca penjaga validasi sebagai **kolom**, bukan diurai dari JSON, dan kasus write-off harus dapat dibedakan sebabnya agar tidak salah mengurangi tagihan pasien (`BKC-DES-024`, `BKC-DES-025`).

### Ringkasan perubahan skema

| Kelompok | Jumlah |
| --- | ---: |
| Tabel baru | 0 |
| Tabel yang berubah skemanya | 2 |
| Kolom baru | 2 |
| Index baru | 1 |
| Kolom yang berubah tipe, kunci, atau nullability | 0 |
| Kolom yang dihapus atau diganti nama | 0 |
| Migration yang dibutuhkan | 1 |

### `BilWriteOffCase` — `Areas/HealthServices/BillingManagement/Billing/Models/BilWriteOffCase.cs`

Status tabel: **`Diperbarui`**. Modul pemilik: Billing dan Kasir (modul ini). Ditulis modul ini.

| Kolom | Tipe | Wajib | Bawaan | Kunci/index | Sensitif | Peran |
| --- | --- | :---: | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | Tidak | — |
| `InvoiceId` | `Guid` | Ya | — | FK ke `BilInvoice`, index | **Ya** | Tagihan yang ditulis-off |
| `Amount` | `decimal(18,2)` | Ya | — | — | **Ya** | Nominal yang ditulis-off. Plafonnya bergantung `Category` (`BIL-VAL-040`) |
| `Category` (**baru**) | `varchar(30)` | Ya | `'PATIENT_AR'` | index gabungan | Tidak | **Kolom baru.** `PATIENT_AR` = piutang pasien (seluruh perilaku yang sudah berjalan). `NON_BILLABLE_RESIDUAL` = selisih yang menurut kontrak penjamin tidak dapat ditagihkan kepada siapa pun. Nilainya **MUST NOT** berubah setelah kasus dibuat |
| `IsFullSettlement` | `bool` | Ya | `false` | — | Tidak | Hanya sah bernilai `true` untuk `Category = PATIENT_AR` (`BIL-VAL-041`) |
| `Status` | `varchar(30)` | Ya | `'SUBMITTED'` | index gabungan | Tidak | `SUBMITTED`, `POSTED`, `REJECTED` — **tidak bertambah** |
| `RequestedBy` | `Guid` | Ya | — | — | Tidak | Pengaju. Selalu manusia; mesin kalkulasi **MUST NOT** mengisi kolom ini (`BKC-DES-023`) |
| `ApprovedBy` | `Guid?` | Tidak | `null` | — | Tidak | Penyetuju. **MUST** berbeda dari `RequestedBy` (`BIL-VAL-017`) |
| `Reason` | `varchar(500)` | Ya | — | — | **Ya** | Alasan berbahasa manusia. **MUST NOT** memuat nomor polis, nomor anggota, nama pasien, maupun diagnosis |
| `IdempotencyKey`, `PayloadHash` | `Guid`, `varchar(64)` | Ya | — | — | Tidak | `Category` **MUST** ikut masuk `PayloadHash` |

**Index baru:** `(InvoiceId, Category, Status)` dengan filter `IsDelete = false`. Dipakai dua perhitungan uang yang berjalan pada setiap pengajuan: `writeOffTotal` per kategori pada `CalculateOutstandingAsync`, dan sisa plafon residual pada `CalculateNonBillableResidualRemainingAsync`.

**Perilaku hapus:** tidak berubah — soft-delete `IsDelete` warisan `IdentityModel`, tanpa cascade.

**Backfill:** seluruh baris lama menjadi `PATIENT_AR` lewat `DEFAULT`, dan itu **benar secara bisnis** — setiap write-off yang pernah dibuat sebelum amendment ini memang write-off piutang pasien. Tidak ada baris yang perlu ditinjau manusia.

### `BilCalculationVersion` — kolom yang bertambah

Status tabel: **`Diperbarui`** (pada revisi `0.7` masih `Sudah ada`).

| Kolom | Tipe | Wajib | Bawaan | Kunci/index | Sensitif | Peran |
| --- | --- | :---: | --- | --- | :---: | --- |
| `NonBillableResidualAmount` (**baru**) | `decimal(18,2)` | Ya | `0` | — | **Ya** | **Kolom baru.** Total selisih perhitungan tanggungan yang menurut kontrak penjamin tidak boleh ditagihkan ke pasien, pada versi kalkulasi ini. Menjadi **plafon** write-off kategori `NON_BILLABLE_RESIDUAL` (`BKC-DES-025`) |
| `UnresolvedCoverageAmount` | `decimal(18,2)` | Ya | `0` | — | **Ya** | **Tetap ada dan tetap diisi.** Maknanya menyempit: menyisakan jalur aturan `NotCovered` dengan `IsAllowExcessPaymentByPatient = false` (`BKC-DES-021`). Kolomnya tidak diganti nama — penggantian nama merusak konsumen tanpa menambah kemampuan |
| `PatientAmount` | `decimal(18,2)` | Ya | — | — | **Ya** | **Nilainya tidak bergeser** oleh amendment ini. Nominal yang berpindah ember sudah dikeluarkan dari porsi pasien sejak revisi `0.7` |

**Backfill:** versi kalkulasi lama tetap `0`, dan itu benar. Sebelum amendment ini nominalnya tercatat pada `UnresolvedCoverageAmount`; menulis ulang versi lama berarti mengubah bukti perhitungan yang kolom itu ada untuk melindunginya.

### `BilAdjustment` — tidak ada kolom baru

Status tabel: **`Sudah ada`**. Kategori sebuah adjustment reversal dibaca lewat relasi `ReversesWriteOffCaseId` → `BilWriteOffCase.Category`, bukan lewat kolom sendiri.

> **Ketidaksesuaian dokumen yang dicatat apa adanya.** Diagram pada `erd/03-financial-exception-adjustment.md` mencantumkan `string AdjustmentType` pada `BilAdjustment`. Kolom itu **tidak ada** di `BilAdjustment.cs`; berkas modelnya hanya mengenal `BillingAdjustmentDirections` dan `BillingAdjustmentStatuses`. Perbedaan ini ditemukan saat membaca source untuk amendment ini, dilaporkan, dan **tidak** dirapikan di sini — perapiannya milik revisi tersendiri agar tidak bercampur dengan perubahan berbasis `BKC-DEC-080`.

### Field baru pada `breakdown.coverage`, `breakdown.items[]`, `administrationFee`, dan `roomCharge`

| Field | Tempat | Tipe JSON | Bawaan | Sensitif | Arti |
| --- | --- | --- | --- | :---: | --- |
| `nonBillableResidualAmount` | `coverage` | angka | `0` | **Ya** | Total selisih yang tidak dapat ditagihkan pada tagihan ini |
| `hasNonBillableResidual` | `coverage` | boolean | `false` | Tidak | Tagihan ini memuat selisih semacam itu pada versi kalkulasi terkini |
| `itemNonBillableResidualAmount` | `items[]` | angka | `0` | **Ya** | Porsi selisih milik baris itu |
| `taxNonBillableResidualAmount` | `items[]` | angka | `0` | **Ya** | Porsi selisih milik pajak baris itu |
| `nonBillableResidualAmount` | `administrationFee`, `roomCharge` | angka | `0` | **Ya** | Porsi selisih milik komponen yang bukan `BilInvoiceItem` |

**Kompatibilitas snapshot lama** tetap dijaga dengan cara yang sama: seluruhnya bertipe nilai non-nullable berbawaan `0`/`false`, sehingga snapshot lama dideserialisasi tanpa galat, dan kebenarannya tetap dijaga `isPerItemAllocationAvailable` (`BKC-DES-017`) — bukan oleh `null`.

Kolom bertanda **Sensitif** di atas mengikuti aturan yang sama seperti seluruh berkas ini: **MUST NOT** masuk custom logger, **MUST NOT** muncul pada payload galat, dan **MUST NOT** dipakai sebagai contoh berisi data asli. Yang boleh masuk audit write-off adalah `InvoiceId`, `WriteOffCaseId`, `Category`, dan perubahan nominal outstanding.

Trace **`BKC-DEC-080`**, `BKC-DEC-036`, `BKC-DES-021`–`025`. Tests `BIL-AT-055`–`061`.
