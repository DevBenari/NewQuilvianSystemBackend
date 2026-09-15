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

Input: **`BKC-DEC-080`** beserta `BKC-DEC-036`; keputusan arsitektur `BKC-DES-021`–`025`. Status ~~draft~~ **approved** — `BKC-DES-021`–`025` disetujui `BKC-DEC-088` (Product/Domain Owner + Finance/AR, "saya approve"), 4 September 2026. **Koreksi 5 September 2026**: baris status ini tertinggal `draft` pada penguncian kontrak sebelumnya ("saya kunci dokumen kontrak" / "kunci semua dokumennya") — enam dokumen kontrak lain sudah `approved` sejak saat itu; ditemukan dan diperbaiki di sini, bukan keputusan baru.

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
| `UnresolvedCoverageAmount` | `decimal(18,2)` | Ya | `0` | — | **Ya** | **Tetap ada, tetap diisi pada versi lama.** Revisi `0.8`: menyempit menyisakan jalur `NotCovered` + `IsAllowExcessPaymentByPatient=false`. **Revisi `0.9`: menyempit lagi — menyisakan NOL jalur**, sehingga bernilai selalu `0` pada setiap versi kalkulasi baru (`BKC-DES-026`/`027`). Kolomnya tidak diganti nama dan tidak di-`DROP` — versi kalkulasi lama tetap memuat angka lamanya sebagai bukti perhitungan yang sudah terjadi |
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

---

## Amendment lanjutan 4 September 2026 — Perluasan perutean write-off ke jalur `NotCovered` (revisi `0.9`)

Input: **`BKC-DEC-089`** menutup `BKC-OQ-093`; keputusan arsitektur `BKC-DES-026`–`027`. Status **approved**, 5 September 2026.

**Nol perubahan skema.** Tidak ada tabel baru, tidak ada kolom baru, tidak ada index baru, dan tidak ada migration tambahan — migration `BE-BKC-027` yang sudah dibuat revisi `0.8` sudah cukup untuk kedua jalur. Yang berubah murni **keterangan peran** kolom `UnresolvedCoverageAmount` di atas (lihat baris yang diperbarui pada tabel `BilCalculationVersion`) — bukan bentuk, tipe, maupun nilai bawaannya.

Trace **`BKC-DEC-089`**, `BKC-DEC-080`, `BKC-DES-026`–`027`. Tests `BIL-AT-062`–`063`.

---

## Amendment 7 September 2026 — Rumpun baru: Petty Cash (Voucher Kas Kecil)

Input: **`PC-DEC-001`–`PC-DEC-013`** (`approved` 7 September 2026); keputusan arsitektur `PC-DES-001`–`PC-DES-014` pada [`../02-backend-architecture.md`](../02-backend-architecture.md). Status **draft**.

Berbeda dari seluruh amendment sebelumnya, amendment ini **membuat tabel baru**. Kelimanya lahir kosong dan tidak menyentuh satu pun tabel yang sudah ada.

### Ringkasan perubahan skema

| Kelompok | Jumlah |
| --- | ---: |
| Tabel baru | 5 |
| Tabel yang berubah skemanya | 0 |
| Kolom baru pada tabel yang sudah ada | 0 |
| Kolom yang dihapus atau diganti nama | 0 |
| Index baru | 10 |
| Foreign key baru | 4 |
| Migration yang dibutuhkan | 1 |
| Baris yang perlu di-backfill | 0 |

### Tabel yang tersentuh — status dan kepemilikan

| Tabel | Status | Modul pemilik | Cara modul ini memakainya |
| --- | --- | --- | --- |
| `BilPettyCashVoucher` | **`Baru`** | Billing dan Kasir | Ditulis dan dibaca |
| `BilPettyCashVoucherCommand` | **`Baru`** | Billing dan Kasir | Ditulis append-only |
| `BilPettyCashBudget` | **`Baru`** | Billing dan Kasir | Ditulis dan dibaca; tepat satu baris pada MVP |
| `BilPettyCashBudgetMovement` | **`Baru`** | Billing dan Kasir | Ditulis append-only |
| `MstPettyCashCategory` | **`Baru`** | Billing Master Data | Dikelola Finance lewat CRUD; dibaca sebagai dropdown |
| `BilNumberSeries` | `Sudah ada` | Billing dan Kasir | **Skemanya tidak berubah.** Bertambah baris data ber-`SequenceKey = 'BILLING_PETTY_CASH_VOUCHER'` |
| `BilCashierShift` | `Sudah ada` | Cashier Operations | **Tidak dibaca dan tidak ditulis sama sekali** (`PC-DEC-001`) |
| `AspNetUsers` (`ApplicationUser`) | `Sudah ada` | Administrator / Identity | Dirujuk lewat `Guid` tanpa foreign key, mengikuti pola `BilCashierShiftCommand.ActorUserId` |

### `BilPettyCashVoucher` — `Areas/HealthServices/BillingManagement/PettyCash/Models/BilPettyCashVoucher.cs`

Status: **`Baru`**, seluruh kolom didokumentasikan.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `VoucherNumber` | `string(40)` | Ya | — | **Unique** | — | — | Tidak | Nomor voucher, contoh `PTC-20260907-0001`. Dialokasikan `BillingNumberSeriesService`; **MUST NOT** diisi frontend |
| `RecipientName` | `string(150)` | Ya | — | Index | — | — | **Ya** | Nama penerima uang. Teks bebas; **MUST NOT** diberi foreign key (`PC-DEC-011`) |
| `CategoryId` | `Guid` | Ya | — | Index | FK ke `MstPettyCashCategory` | `Restrict` | Tidak | Kategori pengeluaran |
| `Amount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Nominal voucher. **MUST** lebih besar dari nol |
| `Purpose` | `string(500)` | Ya | — | — | — | — | **Ya** | "Tujuan" — keperluan pengeluaran, kalimat bebas |
| `Status` | `string(30)` | Ya | `'WAITING_APPROVAL'` | Index | — | — | Tidak | `WAITING_APPROVAL`, `APPROVED`, `CASH_RECEIVED`, `COMPLETED`, `REJECTED`. Label tampilannya dikunci `PC-DEC-013` |
| `RequestedBy` | `Guid` | Ya | — | Index | `ApplicationUser` (tanpa FK) | — | Tidak | Kasir/petugas administrasi yang membuat voucher |
| `SubmittedAt` | `DateTimeOffset` | Ya | — | Index | — | — | Tidak | "Tanggal Pengajuan" pada layar |
| `DecidedBy` | `Guid?` | Tidak | `null` | — | `ApplicationUser` (tanpa FK) | — | Tidak | Kepala Kasir/Finance Operations yang menyetujui atau menolak |
| `DecidedAt` | `DateTimeOffset?` | Tidak | `null` | — | — | — | Tidak | Waktu keputusan |
| `RejectionReason` | `string(500)?` | Tidak | `null` | — | — | — | **Ya** | **Wajib terisi** ketika `Status = REJECTED`; **MUST** kosong pada status lain |
| `DisbursedBy` | `Guid?` | Tidak | `null` | — | `ApplicationUser` (tanpa FK) | — | Tidak | Kasir yang menekan "Uang Diberikan" |
| `DisbursedAt` | `DateTimeOffset?` | Tidak | `null` | — | — | — | Tidak | Waktu penyerahan uang. Inilah saat saldo berkurang (`PC-DEC-009`) |
| `ProofReferenceNumber` | `string(60)?` | Tidak | `null` | — | — | — | Tidak | "No Nota / Kwitansi" yang diisi lewat modal Bukti Nota |
| `ProofSubmittedBy` | `Guid?` | Tidak | `null` | — | `ApplicationUser` (tanpa FK) | — | Tidak | Petugas yang memasukkan nomor nota |
| `ProofSubmittedAt` | `DateTimeOffset?` | Tidak | `null` | — | — | — | Tidak | Waktu nota dimasukkan |
| `CompletedAt` | `DateTimeOffset?` | Tidak | `null` | — | — | — | Tidak | Sama dengan `ProofSubmittedAt` pada MVP; dipisah agar penambahan syarat penyelesaian kelak tidak menuntut kolom baru |
| `IdempotencyKey` | `Guid?` | Tidak | `null` | Unique parsial | — | — | Tidak | Kunci permintaan pembuatan voucher |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | Penjaga perubahan bersamaan, mengikuti `BilCashierShift.RowVersion` |

**Index:**

| Nama | Kolom | Unique | Filter | Kegunaan |
| --- | --- | :---: | --- | --- |
| `IX_BilPettyCashVoucher_VoucherNumber` | `VoucherNumber` | **Ya** | `IsDelete = false` | Menjamin nomor voucher tidak kembar |
| `IX_BilPettyCashVoucher_Status_SubmittedAt` | `Status`, `SubmittedAt` | Tidak | — | Daftar voucher yang disaring status dan diurutkan tanggal — jalur baca utama layar monitoring |
| `IX_BilPettyCashVoucher_CategoryId` | `CategoryId` | Tidak | — | Penyaringan per kategori dan penjaga penghapusan kategori |
| `IX_BilPettyCashVoucher_RecipientName` | `RecipientName` | Tidak | — | Pencarian nama penerima pada kotak cari |
| `IX_BilPettyCashVoucher_IdempotencyKey` | `IdempotencyKey` | **Ya** | `IdempotencyKey IS NOT NULL` | Pembuatan voucher yang dikirim dua kali tidak menghasilkan dua voucher |

**Perilaku hapus:** soft-delete `IsDelete` warisan `IdentityModel`, tanpa cascade. **Pembatalan** memakai `IsCancel`/`CancelDateTime`/`CancelBy` dari warisan yang sama (`PC-DES-007`), bukan status keenam.

### `BilPettyCashVoucherCommand` — `.../PettyCash/Models/BilPettyCashVoucherCommand.cs`

Status: **`Baru`**, seluruh kolom didokumentasikan. Bentuknya meniru `BilCashierShiftCommand` yang sudah ada.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `VoucherId` | `Guid` | Ya | — | Index | FK ke `BilPettyCashVoucher` | `Restrict` | Tidak | Voucher yang dikenai perintah |
| `CommandType` | `string(40)` | Ya | — | — | — | — | Tidak | `SUBMIT`, `APPROVE`, `REJECT`, `CANCEL`, `DISBURSE`, `ATTACH_PROOF`, `PROOF_CORRECTED` |
| `ActorUserId` | `Guid` | Ya | — | Index | `ApplicationUser` (tanpa FK) | — | Tidak | Siapa yang menjalankan perintah |
| `ActorRole` | `string(150)` | Ya | — | — | — | — | Tidak | Peran aktor saat perintah dijalankan, disimpan sebagai teks |
| `EntityVersion` | `Guid` | Ya | — | — | — | — | Tidak | Nilai `RowVersion` voucher sebelum perintah |
| `StatusBefore` | `string(30)?` | Tidak | `null` | — | — | — | Tidak | Kosong hanya untuk `SUBMIT` |
| `StatusAfter` | `string(30)` | Ya | — | — | — | — | Tidak | Status sesudah perintah |
| `Amount` | `decimal(18,2)?` | Tidak | `null` | — | — | — | Tidak | Nominal yang relevan bagi perintah itu |
| `Reason` | `string(500)?` | Tidak | `null` | — | — | — | **Ya** | **Wajib** untuk `REJECT` dan `CANCEL` |
| `IdempotencyKey` | `Guid?` | Tidak | `null` | Unique parsial | — | — | Tidak | Kunci permintaan |
| `PayloadHash` | `string(64)` | Ya | — | — | — | — | Tidak | Sidik isi permintaan; permintaan berulang berisi berbeda ditolak |
| `CorrelationId` | `Guid` | Ya | — | Index | — | — | Tidak | Penelusuran lintas permintaan |
| `CausationId` | `Guid` | Ya | — | — | — | — | Tidak | Perintah yang menyebabkannya |
| `OccurredAt` | `DateTimeOffset` | Ya | — | — | — | — | Tidak | Waktu perintah, zona Asia/Jakarta saat ditampilkan |
| `ResponseJson` | `string` | Ya | `"{}"` | — | — | — | **Ya** | Hasil yang dikembalikan; dipakai memutar ulang permintaan ber-`Idempotency-Key` sama |

**Index:** `IX_BilPettyCashVoucherCommand_VoucherId_OccurredAt` (`VoucherId`, `OccurredAt`) untuk layar riwayat; `IX_BilPettyCashVoucherCommand_IdempotencyKey` unique parsial `WHERE IdempotencyKey IS NOT NULL`.

**Perilaku hapus:** baris **MUST NOT** dihapus maupun ditandai hapus. Ini catatan audit permanen yang menjadi dasar `PC-DEC-003`.

### `BilPettyCashBudget` — `.../PettyCash/Models/BilPettyCashBudget.cs`

Status: **`Baru`**, seluruh kolom didokumentasikan.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `PoolCode` | `string(30)` | Ya | — | **Unique** | — | — | Tidak | `HOSPITAL_MAIN` pada MVP. Disiapkan agar multi-kolam kelak cukup menambah baris (`PC-DES-014`) |
| `PoolName` | `string(100)` | Ya | — | — | — | — | Tidak | Nama yang dibaca manusia, contoh "Kas Kecil Rumah Sakit" |
| `CurrentBalance` | `decimal(18,2)` | Ya | `0` | — | — | — | Tidak | **Saldo berjalan.** Inilah angka kartu "TOTAL PETTY CASH". **MUST NOT** negatif dan **MUST NOT** ditulis di luar `PettyCashBudgetService` |
| `TotalTopUpAmount` | `decimal(18,2)` | Ya | `0` | — | — | — | Tidak | Akumulasi seluruh penambahan anggaran, untuk pelaporan |
| `TotalDisbursedAmount` | `decimal(18,2)` | Ya | `0` | — | — | — | Tidak | Akumulasi seluruh pencairan, untuk pelaporan |
| `Status` | `string(30)` | Ya | `'ACTIVE'` | Index | — | — | Tidak | `ACTIVE` atau `INACTIVE` |
| `LastMovementAt` | `DateTimeOffset?` | Tidak | `null` | — | — | — | Tidak | Waktu pergerakan terakhir |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | Penjaga perubahan bersamaan |

**Index:** `IX_BilPettyCashBudget_PoolCode` unique dengan filter `IsDelete = false`; `IX_BilPettyCashBudget_ActiveSingleton` unique dengan filter `Status = 'ACTIVE' AND IsDelete = false` **pada ekspresi konstan** — inilah yang menegakkan "tepat satu kolam aktif" selama `PC-DEC-010` masih berlaku.

**Catatan konsistensi:** `CurrentBalance` **MUST** selalu sama dengan `TotalTopUpAmount − TotalDisbursedAmount ± penyesuaian`, dan sama dengan `BalanceAfter` baris ledger terakhir. Ketiganya diperbarui pada transaction yang sama; selisih di antaranya berarti ada penulis saldo di luar `PettyCashBudgetService`.

### `BilPettyCashBudgetMovement` — `.../PettyCash/Models/BilPettyCashBudgetMovement.cs`

Status: **`Baru`**, seluruh kolom didokumentasikan.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `BudgetId` | `Guid` | Ya | — | Index | FK ke `BilPettyCashBudget` | `Restrict` | Tidak | Kolam yang bergerak |
| `MovementType` | `string(30)` | Ya | — | Index | — | — | Tidak | `TOP_UP`, `DISBURSEMENT`, `ADJUSTMENT` |
| `Amount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Selalu **positif**; arahnya ditentukan `MovementType`. `ADJUSTMENT` yang mengurangi ditulis sebagai `ADJUSTMENT` bernominal positif dengan `BalanceAfter < BalanceBefore` |
| `BalanceBefore` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Saldo sebelum pergerakan |
| `BalanceAfter` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Saldo sesudah pergerakan. **MUST NOT** negatif |
| `VoucherId` | `Guid?` | Tidak | `null` | Unique parsial | FK ke `BilPettyCashVoucher` | `Restrict` | Tidak | Terisi **hanya** untuk `DISBURSEMENT` |
| `Reason` | `string(500)?` | Tidak | `null` | — | — | — | **Ya** | **Wajib** untuk `TOP_UP` dan `ADJUSTMENT`; kosong untuk `DISBURSEMENT` karena tujuannya sudah ada di voucher |
| `ActorUserId` | `Guid` | Ya | — | Index | `ApplicationUser` (tanpa FK) | — | Tidak | Siapa yang memicu pergerakan |
| `IdempotencyKey` | `Guid?` | Tidak | `null` | Unique parsial | — | — | Tidak | Kunci permintaan penambahan/penyesuaian |
| `CorrelationId` | `Guid` | Ya | — | — | — | — | Tidak | Penelusuran |
| `OccurredAt` | `DateTimeOffset` | Ya | — | Index | — | — | Tidak | Waktu pergerakan |

**Index yang paling menentukan:**

| Nama | Kolom | Unique | Filter | Yang dijaganya |
| --- | --- | :---: | --- | --- |
| `IX_BilPettyCashBudgetMovement_Voucher_Disbursement` | `VoucherId` | **Ya** | `MovementType = 'DISBURSEMENT' AND IsDelete = false` | **Satu voucher paling banyak satu pengurangan saldo.** Ini jaring pengaman terakhir terhadap klik ganda, dan bekerja walaupun kunci penasihat maupun `RowVersion` gagal |
| `IX_BilPettyCashBudgetMovement_Budget_OccurredAt` | `BudgetId`, `OccurredAt` | Tidak | — | Layar riwayat pergerakan anggaran |
| `IX_BilPettyCashBudgetMovement_IdempotencyKey` | `IdempotencyKey` | **Ya** | `IdempotencyKey IS NOT NULL` | Penambahan anggaran yang dikirim dua kali tidak menambah saldo dua kali |

**Perilaku hapus:** baris **MUST NOT** dihapus. Koreksi memakai baris `ADJUSTMENT` baru — ledger tetap append-only.

### `MstPettyCashCategory` — `Areas/HealthServices/BillingManagement/MasterData/Models/MstPettyCashCategory.cs`

Status: **`Baru`**, seluruh kolom didokumentasikan. Bentuknya meniru `MstTaxRule`.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `CategoryCode` | `string(30)` | Ya | — | **Unique** | — | — | Tidak | Contoh `TRANSPORT`, `ATK`. Diisi Finance, mengikuti `MstTaxRule.Code` yang juga diisi pengguna |
| `CategoryName` | `string(100)` | Ya | — | Index | — | — | Tidak | Nama yang tampil pada badge Kategori, contoh "Transport" |
| `Description` | `string(300)?` | Tidak | `null` | — | — | — | Tidak | Penjelasan singkat bagi Finance |
| `IsActive` | `bool` | Ya | `true` | Index | — | — | Tidak | Kategori nonaktif tidak lagi muncul di dropdown, tetapi voucher lama tetap menampilkannya |

**Index:** `IX_MstPettyCashCategory_CategoryCode` unique dengan filter `IsDelete = false`.

**Perilaku hapus:** soft-delete `IsDelete`. Penghapusan **ditolak** bila masih ada `BilPettyCashVoucher` yang menunjuk kategori itu (`BIL-VAL-053`), mengikuti pola penghapusan master data modul ini.

### Skema dalam bentuk DDL

> **Peringatan.** Basis data project ini dibentuk EF Core Migrations, bukan skrip SQL manual. DDL di bawah adalah **dokumentasi bentuk tabel**, bukan skrip yang dijalankan. Menjalankannya akan berbenturan dengan migration. Kolom audit warisan `IdentityModel` tidak ditulis ulang di sini.

```sql
-- Bentuk tabel sebagaimana dihasilkan EF Core. Bukan skrip untuk dijalankan.

CREATE TABLE public."MstPettyCashCategory" (
    "Id"            uuid          NOT NULL,
    "CategoryCode"  varchar(30)   NOT NULL,
    "CategoryName"  varchar(100)  NOT NULL,
    "Description"   varchar(300),
    "IsActive"      boolean       NOT NULL DEFAULT true,
    -- kolom audit IdentityModel tidak ditulis ulang di sini
    CONSTRAINT "PK_MstPettyCashCategory" PRIMARY KEY ("Id")
);
CREATE UNIQUE INDEX "IX_MstPettyCashCategory_CategoryCode"
    ON public."MstPettyCashCategory" ("CategoryCode") WHERE "IsDelete" = false;

CREATE TABLE public."BilPettyCashBudget" (
    "Id"                    uuid           NOT NULL,
    "PoolCode"              varchar(30)    NOT NULL,
    "PoolName"              varchar(100)   NOT NULL,
    "CurrentBalance"        numeric(18,2)  NOT NULL DEFAULT 0,
    "TotalTopUpAmount"      numeric(18,2)  NOT NULL DEFAULT 0,
    "TotalDisbursedAmount"  numeric(18,2)  NOT NULL DEFAULT 0,
    "Status"                varchar(30)    NOT NULL DEFAULT 'ACTIVE',
    "LastMovementAt"        timestamptz,
    "RowVersion"            uuid           NOT NULL,
    CONSTRAINT "PK_BilPettyCashBudget" PRIMARY KEY ("Id")
);
CREATE UNIQUE INDEX "IX_BilPettyCashBudget_PoolCode"
    ON public."BilPettyCashBudget" ("PoolCode") WHERE "IsDelete" = false;

CREATE TABLE public."BilPettyCashVoucher" (
    "Id"                    uuid           NOT NULL,
    "VoucherNumber"         varchar(40)    NOT NULL,
    "RecipientName"         varchar(150)   NOT NULL,          -- SENSITIF
    "CategoryId"            uuid           NOT NULL,
    "Amount"                numeric(18,2)  NOT NULL,
    "Purpose"               varchar(500)   NOT NULL,          -- SENSITIF
    "Status"                varchar(30)    NOT NULL DEFAULT 'WAITING_APPROVAL',
    "RequestedBy"           uuid           NOT NULL,
    "SubmittedAt"           timestamptz    NOT NULL,
    "DecidedBy"             uuid,
    "DecidedAt"             timestamptz,
    "RejectionReason"       varchar(500),                     -- SENSITIF
    "DisbursedBy"           uuid,
    "DisbursedAt"           timestamptz,
    "ProofReferenceNumber"  varchar(60),
    "ProofSubmittedBy"      uuid,
    "ProofSubmittedAt"      timestamptz,
    "CompletedAt"           timestamptz,
    "IdempotencyKey"        uuid,
    "RowVersion"            uuid           NOT NULL,
    CONSTRAINT "PK_BilPettyCashVoucher" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_BilPettyCashVoucher_MstPettyCashCategory_CategoryId"
        FOREIGN KEY ("CategoryId")
        REFERENCES public."MstPettyCashCategory" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_BilPettyCashVoucher_VoucherNumber"
    ON public."BilPettyCashVoucher" ("VoucherNumber") WHERE "IsDelete" = false;
CREATE INDEX "IX_BilPettyCashVoucher_Status_SubmittedAt"
    ON public."BilPettyCashVoucher" ("Status", "SubmittedAt");

CREATE TABLE public."BilPettyCashVoucherCommand" (
    "Id"              uuid           NOT NULL,
    "VoucherId"       uuid           NOT NULL,
    "CommandType"     varchar(40)    NOT NULL,
    "ActorUserId"     uuid           NOT NULL,
    "ActorRole"       varchar(150)   NOT NULL,
    "EntityVersion"   uuid           NOT NULL,
    "StatusBefore"    varchar(30),
    "StatusAfter"     varchar(30)    NOT NULL,
    "Amount"          numeric(18,2),
    "Reason"          varchar(500),                           -- SENSITIF
    "IdempotencyKey"  uuid,
    "PayloadHash"     varchar(64)    NOT NULL,
    "CorrelationId"   uuid           NOT NULL,
    "CausationId"     uuid           NOT NULL,
    "OccurredAt"      timestamptz    NOT NULL,
    "ResponseJson"    text           NOT NULL DEFAULT '{}',   -- SENSITIF
    CONSTRAINT "PK_BilPettyCashVoucherCommand" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_BilPettyCashVoucherCommand_BilPettyCashVoucher_VoucherId"
        FOREIGN KEY ("VoucherId")
        REFERENCES public."BilPettyCashVoucher" ("Id") ON DELETE RESTRICT
);

CREATE TABLE public."BilPettyCashBudgetMovement" (
    "Id"              uuid           NOT NULL,
    "BudgetId"        uuid           NOT NULL,
    "MovementType"    varchar(30)    NOT NULL,
    "Amount"          numeric(18,2)  NOT NULL,
    "BalanceBefore"   numeric(18,2)  NOT NULL,
    "BalanceAfter"    numeric(18,2)  NOT NULL,
    "VoucherId"       uuid,
    "Reason"          varchar(500),                           -- SENSITIF
    "ActorUserId"     uuid           NOT NULL,
    "IdempotencyKey"  uuid,
    "CorrelationId"   uuid           NOT NULL,
    "OccurredAt"      timestamptz    NOT NULL,
    CONSTRAINT "PK_BilPettyCashBudgetMovement" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_BilPettyCashBudgetMovement_BilPettyCashBudget_BudgetId"
        FOREIGN KEY ("BudgetId")
        REFERENCES public."BilPettyCashBudget" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_BilPettyCashBudgetMovement_BilPettyCashVoucher_VoucherId"
        FOREIGN KEY ("VoucherId")
        REFERENCES public."BilPettyCashVoucher" ("Id") ON DELETE RESTRICT
);
CREATE UNIQUE INDEX "IX_BilPettyCashBudgetMovement_Voucher_Disbursement"
    ON public."BilPettyCashBudgetMovement" ("VoucherId")
    WHERE "MovementType" = 'DISBURSEMENT' AND "IsDelete" = false;
```

### Kolom sensitif — aturan yang berlaku untuk amendment ini

Kolom bertanda **Sensitif** — `RecipientName`, `Purpose`, `RejectionReason`, `Reason`, dan `ResponseJson` — **MUST NOT** masuk custom logger, **MUST NOT** muncul pada payload galat, dan **MUST NOT** dipakai sebagai contoh berisi data asli. Yang **boleh** masuk log adalah `VoucherId`, `VoucherNumber`, `CategoryId`, `Amount`, `Status` sebelum dan sesudah, serta `ActorUserId`.

Perlu dicatat bahwa amendment ini adalah satu-satunya bagian modul ini yang **tidak menyentuh data pasien sama sekali** — tidak ada `EncounterId`, tidak ada nomor rekam medis, tidak ada nomor polis, dan tidak ada diagnosis. Yang sensitif di sini adalah identitas penerima uang dan keperluan internal rumah sakit, bukan data medis.

Seluruh contoh berangka pada dokumentasi ini memakai data samaran.

Trace **`PC-DEC-001`–`013`**, `PC-DES-001`–`014`. Tests `BIL-AT-064`–`080`.

---

# Amendment 11 September 2026 — Rumpun Edit Tagihan & Multi-Payer Coverage

> `last_changed_in`: `BIL-CALCULATION-0.9` / revisi blueprint `1.1`, status **draft**. Masukan: `MPY-DEC-001`–`010` (`approved`), `MPY-DES-001`–`017` (`draft`), `01-existing-capability-map.md` § 19.
>
> Seluruh tabel mewarisi `IdentityModel`, sehingga memiliki kolom audit `CreateDateTime`, `CreateBy`, `UpdateDateTime`, `UpdateBy`, `DeleteDateTime`, `DeleteBy`, `CancelDateTime`, `CancelBy`, `IsCancel`, dan `IsDelete`. Kolom-kolom itu **tidak diulang** pada tabel mana pun di bawah, dan tidak ditulis ulang pada bagian DDL. Penghapusan bersifat penandaan lewat `IsDelete`, bukan penghapusan baris.

## Status dan kepemilikan tabel

| Entity | Status | Pemilik | Catatan |
| --- | --- | --- | --- |
| `MstCompanyGuarantorReimbursementRoute` | **Baru** | Administrator / Master Data | Menerangkan hubungan kontraktual dua master pihak (`MPY-DEC-008`) |
| `MstCompanyGuarantorCoverageRule` | **Baru** | Health Services / Master Data | Cetakan 1:1 dari `MstInsuranceCoverageRule` (`CAP-36`) |
| `BilInvoiceItemPayerAssignment` | **Baru** | Billing dan Kasir | Menutup `CAP-37` |
| `BilInvoiceItemBillingDisposition` | **Baru** | Billing dan Kasir | Menutup `CAP-37`, terpisah dari penanggung (`MPY-DES-010`) |
| `BilInvoicePayerChangeCommand` | **Baru** | Billing dan Kasir | Jejak perubahan payer (`MPY-DES-003`) |
| `RegPatientEncounterGuarantor` | Sudah ada | Registration Management | Dirujuk dan diperbarui lewat service pemiliknya; **nol kolom berubah**, **MUST NOT** disalin |
| `MstCompanyGuarantor`, `MstInsuranceProvider` | Sudah ada | Administrator / Master Data | Dirujuk saja |
| `MstPatientCompanyGuarantor`, `MstPatientInsurance` | Sudah ada | Patient Management | Dirujuk untuk memvalidasi kandidat payer |
| `BilInvoice`, `BilInvoiceItem`, `BilCalculationVersion` | Sudah ada | Billing dan Kasir | **Nol kolom berubah** |
| `PhmDrugUsage`, `PhmDrugUsageItem` | Sudah ada | Pharmacy Management | Dibaca saja; **MUST NOT** ditulis (`MPY-DEC-009`) |

### Kolom kunci tabel `Sudah ada` yang dipakai aturan bisnis rumpun ini

| Tabel | Kolom kunci yang dipakai | Sumber lengkap |
| --- | --- | --- |
| `RegPatientEncounterGuarantor` | `EncounterId` (unique, tidak difilter), `PaymentType`, `PatientInsuranceId`, `InsuranceProviderId`, `PatientCompanyGuarantorId`, `CompanyGuarantorId`, `IsEligible`, `IsPolicyActive`, seluruh kolom `*Snapshot` | `Areas/HealthServices/RegistrationManagement/Models/RegPatientEncounterGuarantor.cs` |
| `BilInvoice` | `Id`, `EncounterId`, `Status`, `ServiceType` (`RAJAL`/`IGD`/`OTC`/`RANAP`), `CurrentCalculationVersion`, `RowVersion` | `.../Billing/Models/BilInvoice.cs` |
| `BilInvoiceItem` | `Id`, `InvoiceId`, `CategoryId`, `SourceDomain`, `Status` (`ACTIVE`/`VOIDED`), `Quantity`, `UnitPrice` | `.../Billing/Models/BilInvoiceItem.cs` |
| `MstPatientCompanyGuarantor` | `PatientId`, `CompanyGuarantorId`, `GradeLevel`, `BenefitPlanCode`, `EffectiveStartDate`, `EffectiveEndDate`, `IsEligible`, `IsActive` | `Areas/HealthServices/PatientManagement/MasterData/Models/MstPatientCompanyGuarantor.cs` |

## `MstCompanyGuarantorReimbursementRoute` — Baru

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `CompanyGuarantorId` | `Guid` | Ya | — | Index | FK ke `MstCompanyGuarantor` | `Restrict` | Tidak | Perusahaan penjamin pemilik rute |
| `RouteType` | `string(30)` | Ya | `SELF` | Index | — | — | Tidak | `SELF` atau `INSURANCE_PROVIDER` |
| `InsuranceProviderId` | `Guid?` | Tidak | — | Index | FK ke `MstInsuranceProvider` | `Restrict` | Tidak | Wajib diisi hanya bila `RouteType = INSURANCE_PROVIDER`; **MUST** kosong bila `SELF` |
| `Priority` | `int` | Ya | `1` | — | — | — | Tidak | Urutan bila satu perusahaan punya beberapa rute |
| `IsDefault` | `bool` | Ya | `false` | Unique tersaring | — | — | Tidak | Maksimal satu rute bawaan aktif per perusahaan |
| `EffectiveStartDate` | `DateTime?` | Tidak | — | — | — | — | Tidak | Awal masa berlaku kontrak |
| `EffectiveEndDate` | `DateTime?` | Tidak | — | — | — | — | Tidak | Akhir masa berlaku kontrak |
| `Description` | `string(500)?` | Tidak | — | — | — | — | Tidak | Catatan operasional |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | Penanda aktif |

## `MstCompanyGuarantorCoverageRule` — Baru

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `CompanyGuarantorId` | `Guid` | Ya | — | Index | FK ke `MstCompanyGuarantor` | `Restrict` | Tidak | Perusahaan pemilik aturan |
| `RuleCode` | `string(50)` | Ya | — | Index | — | — | Tidak | Kode aturan, unik per perusahaan |
| `RuleName` | `string(200)` | Ya | — | — | — | — | Tidak | Nama aturan yang dibaca admin |
| `ItemType` | `string(30)` | Ya | `Tariff` | Index | — | — | Tidak | `Tariff`/`Drug`/`DrugCategory`/`Procedure`/`ServiceCategory` |
| `TariffId` | `Guid?` | Tidak | — | Index | FK ke master tarif | `Restrict` | Tidak | Diisi bila aturan menyasar satu tarif |
| `DrugId` | `Guid?` | Tidak | — | Index | FK ke master obat | `Restrict` | Tidak | Diisi bila aturan menyasar satu obat |
| `DrugCategoryId` | `Guid?` | Tidak | — | Index | FK ke master kategori obat | `Restrict` | Tidak | — |
| `ProcedureId` | `Guid?` | Tidak | — | Index | FK ke master tindakan | `Restrict` | Tidak | — |
| `TariffCategoryId` | `Guid?` | Tidak | — | Index | FK ke master kategori tarif | `Restrict` | Tidak | — |
| `PatientClassId` | `Guid?` | Tidak | — | Index | FK ke master kelas perawatan | `Restrict` | Tidak | Aturan berbeda per kelas |
| `BenefitPlanCode` | `string(100)?` | Tidak | — | Index | — | — | Tidak | Paket manfaat pada kartu karyawan |
| `BenefitPlanName` | `string(150)?` | Tidak | — | — | — | — | Tidak | — |
| `EmployeeGrade` | `string(50)?` | Tidak | — | Index | — | — | **Ya** | Golongan karyawan; satu-satunya dimensi yang tidak ada pada aturan asuransi |
| `CoverageStatus` | `string(30)` | Ya | `Covered` | — | — | — | Tidak | `Covered`/`NotCovered`/`PartialCovered`/`NeedApproval` |
| `CoveragePercent` | `numeric(5,2)` | Ya | `100` | — | — | — | Tidak | Persentase tanggungan, 0–100 |
| `MaxCoverageAmount` | `numeric(18,2)?` | Tidak | — | — | — | — | Tidak | Batas rupiah tanggungan per baris |
| `CoPaymentPercent` | `numeric(5,2)?` | Tidak | — | — | — | — | Tidak | **Diturunkan server** dari `CoveragePercent`; masukan klien diabaikan |
| `CoPaymentAmount` | `numeric(18,2)?` | Tidak | — | — | — | — | Tidak | Urun biaya tetap |
| `IsNeedApproval` | `bool` | Ya | `false` | — | — | — | Tidak | Perlu persetujuan perusahaan lebih dulu |
| `IsNeedGuaranteeLetter` | `bool` | Ya | `false` | — | — | — | Tidak | Perlu surat jaminan |
| `IsAllowExcessPaymentByPatient` | `bool` | Ya | `true` | — | — | — | Tidak | Bila `false`, selisih tidak boleh ditagihkan ke pasien |
| `MaxQuantityPerVisit` | `numeric(18,3)?` | Tidak | — | — | — | — | Tidak | Batas jumlah per kunjungan |
| `MaxQuantityPerMonth` | `numeric(18,3)?` | Tidak | — | — | — | — | Tidak | Batas jumlah per bulan |
| `MaxAmountPerVisit` | `numeric(18,2)?` | Tidak | — | — | — | — | Tidak | Batas rupiah per kunjungan |
| `MaxAmountPerMonth` | `numeric(18,2)?` | Tidak | — | — | — | — | Tidak | Batas rupiah per bulan |
| `EffectiveStartDate` | `DateTime?` | Tidak | — | Index | — | — | Tidak | Awal masa berlaku aturan |
| `EffectiveEndDate` | `DateTime?` | Tidak | — | Index | — | — | Tidak | Akhir masa berlaku aturan |
| `Priority` | `int` | Ya | `0` | Index | — | — | Tidak | Prioritas pemilihan aturan; makin besar makin didahulukan |
| `ApprovalInstruction` | `string(500)?` | Tidak | — | — | — | — | Tidak | Petunjuk bagi petugas |
| `BillingInstruction` | `string(500)?` | Tidak | — | — | — | — | Tidak | Petunjuk penagihan |
| `Description` | `string(500)?` | Tidak | — | — | — | — | Tidak | — |
| `SortOrder` | `int` | Ya | `0` | — | — | — | Tidak | Urutan tampil |
| `IsActive` | `bool` | Ya | `true` | Index | — | — | Tidak | Penanda aktif |

## `BilInvoiceItemPayerAssignment` — Baru

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `InvoiceItemId` | `Guid` | Ya | — | **Unique tersaring** | FK ke `BilInvoiceItem` | `Restrict` | Tidak | Tepat satu baris aktif per item |
| `EncounterGuarantorId` | `Guid?` | Tidak | — | Index | FK ke `RegPatientEncounterGuarantor` | `Restrict` | Tidak | **MUST** kosong untuk `CASH`; berisi baris payer kunjungan untuk `INSURANCE`/`COMPANY_GUARANTOR` |
| `PayerKind` | `string(30)` | Ya | `CASH` | Index | — | — | Tidak | `CASH`/`INSURANCE`/`COMPANY_GUARANTOR` |
| `AssignmentSource` | `string(20)` | Ya | `AUTO` | — | — | — | Tidak | `AUTO` (mengikuti payer kunjungan) atau `MANUAL` (diubah kasir) |
| `Reason` | `string(500)?` | Tidak | — | — | — | — | Tidak | Wajib diisi pada perubahan `MANUAL` dan pada reset otomatis |
| `IsActive` | `bool` | Ya | `true` | Index | — | — | Tidak | Baris lama dinonaktifkan, **MUST NOT** ditimpa |

## `BilInvoiceItemBillingDisposition` — Baru

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `InvoiceItemId` | `Guid` | Ya | — | **Unique tersaring** | FK ke `BilInvoiceItem` | `Restrict` | Tidak | Tepat satu keputusan aktif per item |
| `Disposition` | `string(20)` | Ya | `INCLUDED` | Index | — | — | Tidak | `INCLUDED` atau `EXCLUDED` |
| `DecisionSource` | `string(20)` | Ya | `AUTO` | — | — | — | Tidak | `AUTO` atau `CASHIER` |
| `Reason` | `string(500)?` | Tidak | — | — | — | — | Tidak | Wajib pada keputusan `CASHIER` |
| `IsActive` | `bool` | Ya | `true` | Index | — | — | Tidak | Baris lama dinonaktifkan |

## `BilInvoicePayerChangeCommand` — Baru

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
| --- | --- | :---: | --- | --- | --- | --- | :---: | --- |
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `InvoiceId` | `Guid` | Ya | — | Index | FK ke `BilInvoice` | `Restrict` | Tidak | Tagihan tempat perintah dijalankan |
| `EncounterId` | `Guid` | Ya | — | Index | — | — | Tidak | Kunjungan yang payer-nya berubah |
| `PreviousPayerKind` | `string(30)` | Ya | — | — | — | — | Tidak | Jenis payer sebelum perubahan |
| `NewPayerKind` | `string(30)` | Ya | — | — | — | — | Tidak | Jenis payer sesudah perubahan |
| `PreviousPayerNameSnapshot` | `string(250)?` | Tidak | — | — | — | — | Tidak | Nama penjamin sebelumnya, disalin saat perintah dijalankan |
| `NewPayerNameSnapshot` | `string(250)?` | Tidak | — | — | — | — | Tidak | Nama penjamin sesudahnya |
| `PreviousCalculationVersionId` | `Guid?` | Tidak | — | — | FK ke `BilCalculationVersion` | `Restrict` | Tidak | Versi perhitungan sebelum perubahan |
| `NewCalculationVersionId` | `Guid` | Ya | — | Index | FK ke `BilCalculationVersion` | `Restrict` | Tidak | Versi perhitungan hasil perubahan |
| `ResetAssignmentCount` | `int` | Ya | `0` | — | — | — | Tidak | Berapa penanggung item yang ikut direset (`MPY-DES-009`) |
| `Reason` | `string(500)` | Ya | — | — | — | — | Tidak | Alasan yang ditulis kasir; dibaca auditor |
| `IdempotencyKey` | `string(100)` | Ya | — | Unique | — | — | Tidak | Mencegah perintah ganda terhitung dua kali |
| `CorrelationId` | `string(100)?` | Tidak | — | Index | — | — | Tidak | Penelusuran lintas permintaan |
| `CausationId` | `string(100)?` | Tidak | — | — | — | — | Tidak | Perintah pemicu |

Tabel ini **append-only**: barisnya **MUST NOT** diperbarui maupun ditandai hapus dalam keadaan apa pun, karena ia satu-satunya bukti nilai payer sebelum perubahan.

## Skema tabel dalam bentuk DDL

> **Peringatan.** Basis data project ini dibentuk EF Core Migrations, bukan skrip SQL manual. DDL di bawah adalah **dokumentasi bentuk tabel**, bukan skrip untuk dijalankan. Menjalankannya akan berbenturan dengan migration. Kolom audit `IdentityModel` tidak ditulis ulang di sini.

```sql
-- Bentuk tabel sebagaimana dihasilkan EF Core. Bukan skrip untuk dijalankan.

CREATE TABLE public."MstCompanyGuarantorReimbursementRoute" (
    "Id"                    uuid          NOT NULL,
    "CompanyGuarantorId"    uuid          NOT NULL,
    "RouteType"             varchar(30)   NOT NULL DEFAULT 'SELF',
    "InsuranceProviderId"   uuid,
    "Priority"              integer       NOT NULL DEFAULT 1,
    "IsDefault"             boolean       NOT NULL DEFAULT false,
    "EffectiveStartDate"    timestamp with time zone,
    "EffectiveEndDate"      timestamp with time zone,
    "Description"           varchar(500),
    "IsActive"              boolean       NOT NULL DEFAULT true,
    CONSTRAINT "PK_MstCompanyGuarantorReimbursementRoute" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_MstCompanyGuarantorReimbursementRoute_MstCompanyGuarantor"
        FOREIGN KEY ("CompanyGuarantorId")
        REFERENCES public."MstCompanyGuarantor" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_MstCompanyGuarantorReimbursementRoute_MstInsuranceProvider"
        FOREIGN KEY ("InsuranceProviderId")
        REFERENCES public."MstInsuranceProvider" ("Id") ON DELETE RESTRICT
);

-- Maksimal satu rute bawaan aktif per perusahaan penjamin.
CREATE UNIQUE INDEX "IX_MstCompanyGuarantorReimbursementRoute_Default"
    ON public."MstCompanyGuarantorReimbursementRoute" ("CompanyGuarantorId")
    WHERE "IsDefault" = true AND "IsActive" = true AND "IsDelete" = false;

CREATE TABLE public."MstCompanyGuarantorCoverageRule" (
    "Id"                             uuid          NOT NULL,
    "CompanyGuarantorId"             uuid          NOT NULL,
    "RuleCode"                       varchar(50)   NOT NULL,
    "RuleName"                       varchar(200)  NOT NULL,
    "ItemType"                       varchar(30)   NOT NULL DEFAULT 'Tariff',
    "TariffId"                       uuid,
    "DrugId"                         uuid,
    "DrugCategoryId"                 uuid,
    "ProcedureId"                    uuid,
    "TariffCategoryId"               uuid,
    "PatientClassId"                 uuid,
    "BenefitPlanCode"                varchar(100),
    "BenefitPlanName"                varchar(150),
    "EmployeeGrade"                  varchar(50),            -- SENSITIF
    "CoverageStatus"                 varchar(30)   NOT NULL DEFAULT 'Covered',
    "CoveragePercent"                numeric(5,2)  NOT NULL DEFAULT 100,
    "MaxCoverageAmount"              numeric(18,2),
    "CoPaymentPercent"               numeric(5,2),
    "CoPaymentAmount"                numeric(18,2),
    "IsNeedApproval"                 boolean       NOT NULL DEFAULT false,
    "IsNeedGuaranteeLetter"          boolean       NOT NULL DEFAULT false,
    "IsAllowExcessPaymentByPatient"  boolean       NOT NULL DEFAULT true,
    "MaxQuantityPerVisit"            numeric(18,3),
    "MaxQuantityPerMonth"            numeric(18,3),
    "MaxAmountPerVisit"              numeric(18,2),
    "MaxAmountPerMonth"              numeric(18,2),
    "EffectiveStartDate"             timestamp with time zone,
    "EffectiveEndDate"               timestamp with time zone,
    "Priority"                       integer       NOT NULL DEFAULT 0,
    "ApprovalInstruction"            varchar(500),
    "BillingInstruction"             varchar(500),
    "Description"                    varchar(500),
    "SortOrder"                      integer       NOT NULL DEFAULT 0,
    "IsActive"                       boolean       NOT NULL DEFAULT true,
    CONSTRAINT "PK_MstCompanyGuarantorCoverageRule" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_MstCompanyGuarantorCoverageRule_MstCompanyGuarantor"
        FOREIGN KEY ("CompanyGuarantorId")
        REFERENCES public."MstCompanyGuarantor" ("Id") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX "IX_MstCompanyGuarantorCoverageRule_Company_RuleCode"
    ON public."MstCompanyGuarantorCoverageRule" ("CompanyGuarantorId", "RuleCode")
    WHERE "IsDelete" = false;

CREATE TABLE public."BilInvoiceItemPayerAssignment" (
    "Id"                    uuid          NOT NULL,
    "InvoiceItemId"         uuid          NOT NULL,
    "EncounterGuarantorId"  uuid,
    "PayerKind"             varchar(30)   NOT NULL DEFAULT 'CASH',
    "AssignmentSource"      varchar(20)   NOT NULL DEFAULT 'AUTO',
    "Reason"                varchar(500),
    "IsActive"              boolean       NOT NULL DEFAULT true,
    CONSTRAINT "PK_BilInvoiceItemPayerAssignment" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_BilInvoiceItemPayerAssignment_BilInvoiceItem"
        FOREIGN KEY ("InvoiceItemId")
        REFERENCES public."BilInvoiceItem" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_BilInvoiceItemPayerAssignment_RegPatientEncounterGuarantor"
        FOREIGN KEY ("EncounterGuarantorId")
        REFERENCES public."RegPatientEncounterGuarantor" ("Id") ON DELETE RESTRICT
);

-- Tepat satu penanggung aktif per baris biaya.
CREATE UNIQUE INDEX "IX_BilInvoiceItemPayerAssignment_ActiveItem"
    ON public."BilInvoiceItemPayerAssignment" ("InvoiceItemId")
    WHERE "IsActive" = true AND "IsDelete" = false;

CREATE TABLE public."BilInvoiceItemBillingDisposition" (
    "Id"              uuid          NOT NULL,
    "InvoiceItemId"   uuid          NOT NULL,
    "Disposition"     varchar(20)   NOT NULL DEFAULT 'INCLUDED',
    "DecisionSource"  varchar(20)   NOT NULL DEFAULT 'AUTO',
    "Reason"          varchar(500),
    "IsActive"        boolean       NOT NULL DEFAULT true,
    CONSTRAINT "PK_BilInvoiceItemBillingDisposition" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_BilInvoiceItemBillingDisposition_BilInvoiceItem"
        FOREIGN KEY ("InvoiceItemId")
        REFERENCES public."BilInvoiceItem" ("Id") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX "IX_BilInvoiceItemBillingDisposition_ActiveItem"
    ON public."BilInvoiceItemBillingDisposition" ("InvoiceItemId")
    WHERE "IsActive" = true AND "IsDelete" = false;

CREATE TABLE public."BilInvoicePayerChangeCommand" (
    "Id"                            uuid          NOT NULL,
    "InvoiceId"                     uuid          NOT NULL,
    "EncounterId"                   uuid          NOT NULL,
    "PreviousPayerKind"             varchar(30)   NOT NULL,
    "NewPayerKind"                  varchar(30)   NOT NULL,
    "PreviousPayerNameSnapshot"     varchar(250),
    "NewPayerNameSnapshot"          varchar(250),
    "PreviousCalculationVersionId"  uuid,
    "NewCalculationVersionId"       uuid          NOT NULL,
    "ResetAssignmentCount"          integer       NOT NULL DEFAULT 0,
    "Reason"                        varchar(500)  NOT NULL,
    "IdempotencyKey"                varchar(100)  NOT NULL,
    "CorrelationId"                 varchar(100),
    "CausationId"                   varchar(100),
    CONSTRAINT "PK_BilInvoicePayerChangeCommand" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_BilInvoicePayerChangeCommand_BilInvoice"
        FOREIGN KEY ("InvoiceId")
        REFERENCES public."BilInvoice" ("Id") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX "IX_BilInvoicePayerChangeCommand_IdempotencyKey"
    ON public."BilInvoicePayerChangeCommand" ("IdempotencyKey");
```

## Contoh berangka — bagaimana ketiga tabel operasional bekerja bersama

Kunjungan rawat jalan Ny. S (data samaran) berpenjamin perusahaan PT Sejahtera, aturan tanggungan 80% untuk tindakan dan 100% untuk obat generik.

| Baris biaya | Harga | Penanggung item | Disposisi | Hasil perhitungan |
| --- | ---: | --- | --- | --- |
| Konsultasi dokter | Rp 150.000 | `COMPANY_GUARANTOR` (`AUTO`) | `INCLUDED` (`AUTO`) | Penjamin Rp 120.000, pasien Rp 30.000 |
| Obat generik A | Rp 40.000 | `COMPANY_GUARANTOR` (`AUTO`) | `INCLUDED` (`AUTO`) | Penjamin Rp 40.000, pasien Rp 0 |
| Obat generik B | Rp 25.000 | `COMPANY_GUARANTOR` (`AUTO`) | **`EXCLUDED`** (`CASHIER`) | **Tidak masuk tagihan sama sekali** — pasien tidak menebusnya |
| Vitamin tambahan | Rp 80.000 | **`CASH`** (`MANUAL`) | `INCLUDED` (`AUTO`) | Pasien Rp 80.000 — diminta dibayar sendiri |

Total tagihan Rp 270.000 (Rp 295.000 dikurangi obat B yang tidak ditebus). Porsi penjamin Rp 160.000, porsi pasien Rp 110.000. Perhatikan bahwa obat B **tidak** muncul sebagai porsi pasien maupun porsi penjamin — ia keluar dari nominal yang layak dihitung sebelum mesin tanggungan dipanggil.

Bila kemudian kasir mengganti payer kunjungan menjadi tunai, baris pertama sampai ketiga yang bertanda `COMPANY_GUARANTOR` **direset otomatis** menjadi `CASH`/`AUTO` beserta alasan bawaan, `ResetAssignmentCount` bernilai `3`, dan response memberi tahu kasir bahwa tiga penanggung item ikut berubah (`MPY-DES-009`).

Seluruh contoh berangka memakai data samaran.

Trace **`MPY-DEC-001`–`010`**, `MPY-DES-001`–`017`. Tests `BIL-AT-081`–`100`.
