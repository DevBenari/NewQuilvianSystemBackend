# ERD — Operasional Laboratorium (`BC-LAB`)

| Field | Value |
|---|---|
| Blueprint ID | `LAB-BP-001` |
| Bounded context | `BC-LAB` Operasional Laboratorium |
| Revision | `2` |
| Status | `draft` |
| Backend SHA | `c87d9c0` |

Kolom audit warisan `IdentityModel` tidak digambar pada ERD ini. Penjelasannya ada di
`data-dictionary.md`.

---

## 1. Pesanan, wadah, dan pemeriksaan

```mermaid
erDiagram
    LabOrder {
        uuid Id PK
        uuid EncounterId FK "milik Registration"
        uuid ProcedureId FK "pemeriksaan yang dipesan pertama"
        int OrderStatus "enum int"
        int StatusBeforeHold "enum int, saat ditahan"
        int Discipline "enum int, 1 Pat Klinik 2 Pat Anatomi 3 Mikrobiologi"
        timestamp RequestedAt
        timestamp CompletedAt
        int Version "concurrency token"
    }
    LabSpecimen {
        uuid Id PK
        uuid LabOrderId FK
        varchar SpecimenBarcode UK "64, unik"
        int SpecimenSequence "unik bersama LabOrderId"
        varchar SpecimenDescription "200"
        int SpecimenStatus "enum int"
        int StatusBeforeHold "enum int"
        timestamp CollectedAt
        uuid CollectedByUserId
        timestamp ReceivedAt
        uuid ReceivedByUserId
        timestamp DecidedAt
        uuid DecidedByUserId
        uuid RejectionReasonId FK
        varchar RejectionNote "1000"
        uuid SupersededSpecimenId FK "wadah yang digantikan"
        int RecollectionCause "enum int"
        int Version "concurrency token"
    }
    LabExamination {
        uuid Id PK
        uuid LabOrderId FK
        uuid SpecimenId FK "wadah penopang"
        uuid ProcedureId FK "milik Master Data"
        varchar ProcedureCodeSnapshot "50"
        varchar ProcedureNameSnapshot "200"
        uuid TariffId FK "milik Master Data"
        varchar TariffCodeSnapshot "50"
        numeric UnitPriceSnapshot "salinan harga saat kejadian"
        int ExaminationStatus "enum int"
        timestamp ChargeEligibleAt
        int Urgency "enum int, cito per pemeriksaan"
        boolean IsDuplo
        int Version "concurrency token"
    }
    LabTransitionHistory {
        uuid Id PK
        uuid LabOrderId FK
        uuid LabSpecimenId FK "boleh kosong"
        uuid LabExaminationId FK "boleh kosong"
        uuid EncounterId FK
        int Scope "enum int"
        varchar Action "100"
        varchar FromStatus "50"
        varchar ToStatus "50"
        varchar ReasonCode "50"
        varchar ReasonNote "1000"
        uuid ActorUserId
        timestamp OccurredAt
        uuid CorrelationId
    }
    MstLabRejectionReason {
        uuid Id PK
        varchar ReasonCode UK "50, unik saat belum dihapus"
        varchar ReasonName "200"
        varchar Description "500"
        boolean IsInternalHospitalError "terkunci admin"
        boolean RequiresNote "terkunci admin"
        boolean IsActive
        int SortOrder
    }

    LabOrder ||--o{ LabSpecimen : "1:N — Diperbarui"
    LabOrder ||--o{ LabExamination : "1:N — Baru"
    LabSpecimen ||--o{ LabExamination : "1:N — Baru, satu wadah menopang banyak pemeriksaan"
    LabSpecimen |o--o| LabSpecimen : "0:1 — Sudah ada, ambil ulang"
    MstLabRejectionReason ||--o{ LabSpecimen : "1:N — Sudah ada"
    LabOrder ||--o{ LabTransitionHistory : "1:N — Diperbarui"
```

### Perubahan makna yang harus dibaca dengan teliti

| Sebelum `LAB-DEC-024` | Sesudah |
|---|---|
| `LabSpecimen` membawa `ProcedureId` dan salinan tarif | Keduanya pindah ke `LabExamination` |
| Satu barcode = satu pemeriksaan | Satu barcode = satu wadah nyata, yang dapat menopang beberapa pemeriksaan |
| Menolak satu baris = menolak satu pemeriksaan | Menolak satu wadah = menggugurkan seluruh pemeriksaan di atasnya |
| Baris tagihan menunjuk identitas sampel | Baris tagihan menunjuk identitas pemeriksaan |

---

## 2. Batas nilai dan persetujuan klinis

```mermaid
erDiagram
    LabValueBound {
        uuid Id PK
        uuid ProcedureId FK "milik Master Data"
        int ResultForm "enum int, 1 angka 2 pilihan"
        varchar Unit "20, wajib bila bentuk angka"
        numeric NormalLow
        numeric NormalHigh
        numeric CriticalLow
        numeric CriticalHigh
        int GenderScope "enum int, 1 semua 2 pria 3 wanita"
        uuid AgeCategoryId FK "milik Master Data, kosong berarti semua umur"
        int CitoTurnaroundMinutes "batas waktu cito"
        boolean IsActive
    }
    LabValueOption {
        uuid Id PK
        uuid ValueBoundId FK
        varchar OptionCode UK "20, unik bersama ValueBoundId"
        varchar OptionName "100"
        boolean IsOutOfReference
        boolean IsCritical
        int SortOrder
    }
    LabValueBoundChangeRequest {
        uuid Id PK
        uuid ValueBoundId FK
        int RequestStatus "enum int"
        numeric ProposedCriticalLow
        numeric ProposedCriticalHigh
        varchar ProposedCriticalOptionCodes "500, dipisah koma"
        varchar RequestReason "1000"
        uuid RequestedByUserId
        timestamp RequestedAt
        uuid DecidedByUserId
        timestamp DecidedAt
        varchar DecisionNote "1000"
    }
    LabValueBoundHistory {
        uuid Id PK
        uuid ValueBoundId FK
        varchar ChangedField "100"
        varchar OldValue "200"
        varchar NewValue "200"
        uuid ActorUserId
        uuid ApprovedByUserId "terisi bila perubahan batas kritis"
        varchar ChangeReason "1000"
        timestamp OccurredAt
    }

    LabValueBound ||--o{ LabValueOption : "1:N — Baru"
    LabValueBound ||--o{ LabValueBoundChangeRequest : "1:N — Baru"
    LabValueBound ||--o{ LabValueBoundHistory : "1:N — Baru"
```

### Aturan kunci pada kelompok ini

| Aturan | Wujudnya |
|---|---|
| Satu pemeriksaan boleh punya beberapa baris batas | Unik pada `ProcedureId` + `GenderScope` + `AgeCategoryId` |
| Satu baris punya tepat satu bentuk hasil | `ResultForm` menentukan kolom mana yang wajib terisi |
| Bentuk angka wajib bersatuan | `Unit` wajib bila `ResultForm` bernilai angka |
| Bentuk pilihan wajib punya daftar pilihan | Sekurang-kurangnya satu `LabValueOption` |
| Batas kritis tidak dapat diubah langsung | Perubahan ditulis ke `LabValueBoundChangeRequest` lebih dulu |
| Seluruh perubahan berriwayat | Satu baris `LabValueBoundHistory` per kolom yang berubah |

---

## 2b. Lapis rujukan — data milik modul lain

Laboratorium **membaca** ketiga kelompok di bawah dan **tidak menulis** ke satu pun.
Digambarkan di sini agar implementer melihat bentuk yang dipakainya.

```mermaid
erDiagram
    MstProcedure {
        uuid Id PK
        varchar ProcedureCode UK
        varchar ProcedureName
        boolean IsLaboratory "penyaring pertama"
        int LabDiscipline "BARU, 1 Patologi Klinik 2 Patologi Anatomi 3 Mikrobiologi"
        boolean IsCoveredByInsuranceDefault
    }
    MstTariff {
        uuid Id PK
        uuid ProcedureId FK
        uuid ServiceUnitId FK "boleh kosong"
        uuid PatientClassId FK "boleh kosong"
        numeric TariffAmount
        timestamp EffectiveStartDate
        timestamp EffectiveEndDate
    }
    MstInsuranceTariff {
        uuid Id PK
        uuid InsuranceProviderId FK
        uuid TariffId FK
        numeric ContractPrice
        boolean IsUsingContractPrice
        varchar BenefitPlanCode
        int Priority
    }
    MstReferralInstitution {
        uuid Id PK
        varchar InstitutionCode UK
        varchar InstitutionName
        varchar Address
        varchar PhoneNumber
        boolean IsActive
    }
    MstReferralDoctor {
        uuid Id PK
        uuid ReferralInstitutionId FK
        varchar DoctorName
        boolean IsActive
    }
    TrxPatientEncounter {
        uuid Id PK
        boolean IsWalkIn
        boolean IsReferral
        varchar ReferralNumber
        uuid ReferralInstitutionId FK "BARU"
        uuid ReferralDoctorId FK "BARU"
    }

    MstProcedure ||--o{ MstTariff : "1:N — Sudah ada, milik Master Data"
    MstTariff ||--o{ MstInsuranceTariff : "1:N — Sudah ada, milik Master Data"
    MstReferralInstitution ||--o{ MstReferralDoctor : "1:N — Baru, milik Master Data"
    MstReferralInstitution ||--o{ TrxPatientEncounter : "1:N — Baru, milik Registration"
    MstReferralDoctor ||--o{ TrxPatientEncounter : "1:N — Baru, milik Registration"
```

### Bagaimana ketiganya dipakai bersama

Saat petugas membuka layar pemesanan, satu baris katalog dirakit dari tiga sumber:

| Yang tampil di layar | Dari |
|---|---|
| Nama pemeriksaan dan disiplinnya | `MstProcedure` |
| Harga satuan | `MstTariff` yang berlaku pada tanggal kejadian, menurut unit dan kelas pasien |
| Tercakup penjamin atau tidak | Ada tidaknya baris `MstInsuranceTariff` untuk penjamin pasien |

**Contoh perakitan:**

> Pasien Andi berpenjamin BPJS, dilayani di unit laboratorium, kelas pasien umum.
>
> | Pemeriksaan | Disiplin | Harga rumah sakit | Kontrak BPJS | Yang tampil |
> |---|---|---|---|---|
> | Hemoglobin | Patologi Klinik | Rp50.000 | Ada, Rp42.000 | Rp50.000, **tercakup** |
> | Kultur darah | Mikrobiologi | Rp350.000 | Tidak ada | Rp350.000, **tidak tercakup** |
>
> Kultur darah **tetap boleh** dipesan. Keterangan tidak tercakup muncul agar pasien tahu
> sebelum pemeriksaan dikerjakan. Keputusan tagihannya tetap milik Billing.

### Batas yang tegas

| Yang boleh | Yang dilarang |
|---|---|
| Membaca ketiganya | Menulis ke salah satunya |
| Menyalin harga saat kejadian ke baris pemeriksaan | Menyimpan keputusan cakupan sebagai kebenaran |
| Menampilkan total sebagai perkiraan biaya | Membentuk tagihan |

---

## 3. Contoh isi yang menjelaskan bentuknya

**Hemoglobin — bentuk angka, tiga baris batas:**

| `ProcedureId` | `GenderScope` | `AgeCategoryId` | `Unit` | Normal | Kritis |
|---|---|---|---|---|---|
| Hemoglobin | Pria | Dewasa | g/dL | 13,0 – 17,0 | < 7,0 atau > 20,0 |
| Hemoglobin | Wanita | Dewasa | g/dL | 12,0 – 15,0 | < 7,0 atau > 20,0 |
| Hemoglobin | Semua | Anak | g/dL | 11,0 – 14,0 | < 6,0 atau > 18,0 |

**Protein urin — bentuk pilihan, satu baris batas dengan lima pilihan:**

| `OptionCode` | `OptionName` | `IsOutOfReference` | `IsCritical` |
|---|---|:---:|:---:|
| `NEG` | Negatif | Tidak | Tidak |
| `P1` | +1 | Ya | Tidak |
| `P2` | +2 | Ya | Tidak |
| `P3` | +3 | Ya | **Ya** |
| `P4` | +4 | Ya | **Ya** |

**Satu wadah menopang dua pemeriksaan:**

| Wadah | Barcode | Pemeriksaan yang ditopang | Harga |
|---|---|---|---|
| Tabung serum | `LSP-a1b2…` | Fungsi hati | Rp150.000 |
| Tabung serum yang sama | `LSP-a1b2…` | Fungsi ginjal | Rp120.000 |

Satu barcode, satu keputusan kelayakan, dua baris tagihan.

---

## Amandemen 2026-09-18 — Hasil Mikrobiologi dan Patologi Anatomi (`S4b`, `S4c`)

Menurunkan `LAB-DA-001` revision 6 bagian A3, dan `02-backend-architecture.md` bagian 14.

### ERD — hasil Mikrobiologi

```mermaid
erDiagram
    LabExamination ||--o{ LabMicrobiologyIsolate : "menemukan"
    LabMicrobiologyIsolate ||--o{ LabIsolateSusceptibility : "diuji terhadap"
    LabOrganism ||--o{ LabMicrobiologyIsolate : "menamai"
    LabAntibiotic ||--o{ LabIsolateSusceptibility : "menamai"

    LabExamination {
        uuid Id PK
        uuid LabOrderId FK
        int MicrobiologyFinding "nullable"
        timestamp ExaminedAt "nullable"
        timestamp ResultEnteredAt "nullable"
    }
    LabMicrobiologyIsolate {
        uuid Id PK
        uuid LabExaminationId FK
        uuid LabOrganismId FK
        varchar OrganismNameSnapshot
        varchar Note "nullable"
    }
    LabIsolateSusceptibility {
        uuid Id PK
        uuid LabMicrobiologyIsolateId FK
        uuid LabAntibioticId FK
        varchar AntibioticNameSnapshot
        decimal Concentration "nullable"
        int ZoneDiameterMm "nullable"
        int Result
    }
    LabOrganism {
        uuid Id PK
        varchar OrganismCode UK
        varchar OrganismName
        bool IsActive
    }
    LabAntibiotic {
        uuid Id PK
        varchar AntibioticCode UK
        varchar AntibioticName
        bool IsActive
    }
```

### Status dan pemilik setiap entity

| Entity | Status | Pemilik | Catatan |
|---|---|---|---|
| `LabExamination` | **`Extend`** | `BC-LAB` | Bertambah empat kolom |
| `LabMicrobiologyIsolate` | **`New`** | `BC-LAB` | `LAB-DC-036` |
| `LabIsolateSusceptibility` | **`New`** | `BC-LAB` | `LAB-DC-037` |
| `LabOrganism` | **`New`** | `BC-LAB` | `LAB-DC-041`, `LAB-DEC-084` |
| `LabAntibiotic` | **`New`** | `BC-LAB` | `LAB-DC-042`, `LAB-DEC-084` |

## Amandemen 2026-09-21 — `S4b` sesudah amendment pass putaran 9 dan 10

Menurunkan decision log **revision 50** (`LAB-DEC-095`..`LAB-DEC-113`) dan capability map
**revision 4**. Bersifat **aditif** terhadap amandemen 2026-09-18: nol entity yang sudah
digambar berubah bentuk, dan nol kolom yang sudah ada bergeser.

### Yang sudah benar sejak 2026-09-18 dan tidak diubah

Empat keputusan putaran 9 ternyata **menegakkan** bentuk yang sudah digambar, bukan mengubahnya.
Dicatat supaya tidak ada yang mengira gambar lama perlu disentuh:

| Keputusan | Bentuk yang sudah digambar | Hasil |
|---|---|---|
| `LAB-DEC-095` hasil per pemeriksaan | `LabMicrobiologyIsolate.LabExaminationId` | ✅ sudah benar |
| `LAB-DEC-101` MIC dan zona keduanya opsional | `Concentration` dan `ZoneDiameterMm` keduanya `nullable`, `Result` wajib | ✅ sudah benar |
| `LAB-DEC-102` nol subbakteri | `LabOrganism` nol kolom induk | ✅ sudah benar |
| `LAB-DEC-113` status temuan daftar sendiri | `LabExamination.MicrobiologyFinding` terpisah dari `LabPathologyFindingStatus` | ✅ sudah benar |

### ERD — kelengkapan hasil, penanda Definitif, dan aturan kritis

```mermaid
erDiagram
    LabExamination ||--o{ LabMicrobiologyIsolate : "menemukan"
    LabMicrobiologyIsolate ||--o{ LabIsolateSusceptibility : "diuji terhadap"
    LabOrganism ||--o{ LabMicrobiologyIsolate : "menamai"
    LabAntibiotic ||--o{ LabIsolateSusceptibility : "menamai"
    LabOrganism ||--o{ LabMicrobiologyCriticalRule : "dinilai oleh"
    LabAntibiotic ||--o{ LabMicrobiologyCriticalRule : "dinilai oleh"

    LabExamination {
        uuid Id PK
        int MicrobiologyFinding "nullable"
        timestamp FinalizedAt "nullable - BARU"
        uuid FinalizedByUserId "nullable - BARU"
        int ReopenCount "BARU"
        uuid ConsultedByUserId "nullable - BARU"
        varchar ConsultedToName "nullable - BARU"
        timestamp ConsultedAt "nullable - BARU"
    }
    LabMicrobiologyCriticalRule {
        uuid Id PK
        uuid LabOrganismId FK "nullable"
        uuid LabAntibioticId FK "nullable"
        int SusceptibilityResult "nullable"
        varchar RuleNote "nullable"
        bool IsActive
    }
```

**`LabMicrobiologyCriticalRule` — kenapa ketiga ruas penilainya boleh kosong.** Satu baris
menyatakan *"keadaan seperti ini kritis"*, dan keadaan itu tidak selalu bertumpu pada ketiganya.
Ruas yang kosong berarti **apa saja**.

| Contoh baris | Organisme | Antibiotik | Interpretasi | Artinya |
|---|---|---|---|---|
| 1 | `MRSA` | *(kosong)* | *(kosong)* | Kuman itu selalu kritis, antibiotik apa pun hasilnya |
| 2 | *(kosong)* | `Meropenem` | `R` | Resisten terhadap Meropenem selalu kritis, kuman apa pun |
| 3 | `Escherichia coli` | `Ceftriaxone` | `R` | Hanya kombinasi itu yang kritis |

Baris yang **ketiganya kosong** ditolak — ia berarti seluruh hasil kritis, dan itu mematikan
guna penandanya (`VAL-106`).

### ERD — Spesifik Specimen dan jejak perubahan ruas

```mermaid
erDiagram
    LabSpecimenType ||--o{ LabSpecimenDetailType : "menaungi"
    LabSpecimen ||--o{ LabSpecimenDetail : "dirinci oleh"
    LabSpecimenDetailType ||--o{ LabSpecimenDetail : "menamai"
    LabSpecimen ||--o{ LabFieldChangeLog : "dicatat perubahannya"

    LabSpecimenType {
        uuid Id PK
        varchar SpecimenTypeCode UK
        varchar SpecimenTypeName
        bool IsOtherBucket
    }
    LabSpecimenDetailType {
        uuid Id PK
        uuid LabSpecimenTypeId FK
        varchar DetailTypeCode UK
        varchar DetailTypeNameId "Bahasa Indonesia"
        varchar DetailTypeNameEn "nullable - sinonim pencarian"
        bool IsActive
        int SortOrder
    }
    LabSpecimenDetail {
        uuid Id PK
        uuid LabSpecimenId FK
        uuid LabSpecimenDetailTypeId FK
        varchar DetailNameSnapshot
    }
    LabFieldChangeLog {
        uuid Id PK
        varchar EntityName
        uuid EntityId
        varchar FieldName
        varchar OldValue "nullable"
        varchar NewValue "nullable"
        uuid ChangedByUserId
        timestamp ChangedAt
    }
```

**`LabSpecimenDetail` adalah tabel jembatan, dan ia menyimpan snapshot nama.** Alasannya sama
dengan `OrganismNameSnapshot`: nama yang diperbaiki kepala instalasi enam bulan kemudian tidak
boleh mengubah arti specimen yang sudah tercatat.

**`LabFieldChangeLog` sengaja dibuat umum**, tidak khusus specimen. `EntityName` + `EntityId`
membuatnya dapat dipakai ulang ketika ruas lain kelak perlu dijejaki, tanpa menambah tabel
kelima. Ia **tidak** menggantikan `LabTransitionHistory`; keduanya bersumbu berbeda
(`LAB-DEC-112`).

### Status dan pemilik setiap entity — amandemen ini

| Entity | Status | Pemilik | Keputusan |
|---|---|---|---|
| `LabExamination` | **`Extend`** | `BC-LAB` | Bertambah **enam** kolom: tiga kelengkapan (`LAB-DEC-097`), tiga konsultasi (`LAB-DEC-106`) |
| `LabMicrobiologyCriticalRule` | **`New`** | `BC-LAB` | `LAB-DEC-103` |
| `LabSpecimenDetailType` | **`New`** | `BC-LAB` | `LAB-DEC-098`, `LAB-DEC-099` |
| `LabSpecimenDetail` | **`New`** | `BC-LAB` | `LAB-DEC-098` |
| `LabFieldChangeLog` | **`New`** | `BC-LAB` | `LAB-DEC-112` |
| `LabSpecimenType` | **`Existing`** — tidak disentuh | `BC-LAB` | `LAB-DEC-098` butir 1 |
| `LabTransitionHistory` | **`Existing`** — tidak disentuh | `BC-LAB` | `LAB-DEC-112` |
| `MstMeasurement` | **`Existing`** — hanya **baris baru**, nol kolom | `master-data` | `LAB-DEC-100` |
| `MstDoctor`, `TrxOnCallAssignment` | **`Adapter/View`** — dibaca saja, nol disalin | `master-data` / `human-resource` | `LAB-DEC-111` |

> **Nol salinan pasien maupun dokter dibuat.** `LAB-DEC-111` membaca rantai
> `TrxOnCallAssignment` → `MstDoctor` lewat join pada `ApplicationDbContext` yang sama. Nol
> tabel bayangan, nol sinkronisasi.

### Delete behavior dan index

| Tabel | Delete behavior | Index |
|---|---|---|
| `LabMicrobiologyCriticalRule` | **Nol cascade.** Organisme/antibiotik dinonaktifkan, bukan dihapus (`INV-31`) | Index atas `(LabOrganismId, LabAntibioticId, SusceptibilityResult)` di antara baris `IsActive` |
| `LabSpecimenDetailType` | `Restrict` terhadap `LabSpecimenType` | Unik **parsial** atas `DetailTypeCode` di antara baris yang belum `IsDelete` — mengikuti pola `VAL-91` |
| `LabSpecimenDetail` | `Cascade` dari `LabSpecimen` | Unik parsial atas `(LabSpecimenId, LabSpecimenDetailTypeId)` — satu rincian tidak boleh dicentang dua kali |
| `LabFieldChangeLog` | **Nol cascade dari mana pun.** Jejak tidak ikut mati bersama barisnya | Index atas `(EntityName, EntityId, ChangedAt)` |

> **Kenapa `LabFieldChangeLog` tidak cascade.** Menghapus specimen lalu ikut menghapus jejak
> perubahannya membuat jejak itu hilang tepat pada saat ia paling dibutuhkan. Pola yang sama
> dipakai `LabTransitionHistory`.

### Yang sengaja tidak digambar

| Yang ditolak | Alasan |
|---|---|
| Kolom `IsCritical` tersimpan pada isolat atau baris kepekaan | Penilaian dihitung saat dibaca, bukan disimpan. Aturan kritis dapat berubah, dan nilai tersimpan akan membekukan penilaian lama sebagai kalau-kalau fakta. Sejalan `LAB-DEC-080` — kolom mencatat apa yang terjadi, bukan menyimpulkan |
| Tabel `LabMicrobiologyReport` per order | `LAB-DEC-095` menegaskan hasil melekat pada pemeriksaan. Patologi Anatomi punya tabel per order justru karena `LAB-DEC-085` memutuskan sebaliknya untuk disiplin itu |
| Kolom `HL7Status` | `LAB-DEC-109` — ruasnya tidak dibangun sampai `LAB-COORD-012` dijawab |
| Tabel penugasan jaga milik Laboratorium | `LAB-DEC-111` membaca milik Human Resource. Mendirikan sendiri berarti dua daftar dokter jaga yang dapat berbeda |

---

### ~~Patologi Anatomi — nol entity baru~~ — **DICABUT 2026-09-18 sore**

> Bagian ini menilai laporan Patologi Anatomi sebagai `VALUE_OBJECT` berbentuk tiga kolom pada
> `LabExamination`. **Penilaian itu dicabut** oleh bukti `LAB-EVD-003` dan `LAB-DEC-085`:
> hasil PA melekat pada **pesanan**, ruasnya sampai lima belas, dan ia punya lifecycle sendiri.
> Bentuk yang berlaku ada pada **Amandemen 2026-09-18 sore** di bawah.

---

## Amandemen 2026-09-18 sore — Laporan Patologi Anatomi per pesanan

Menurunkan `LAB-DA-001` revision 7 bagian A4, dan `02-backend-architecture.md` bagian 15.

### ERD — laporan dan konteks klinis

```mermaid
erDiagram
    LabOrder ||--o| LabPathologyOrderContext : "konteks klinis"
    LabOrder ||--o| LabPathologyReport : "menghasilkan"
    LabPathologyReport ||--o{ LabPathologyReportValue : "berisi"
    LabPathologyParameter ||--o{ LabPathologyReportValue : "menamai"

    LabOrder {
        uuid Id PK
        int Discipline
    }
    LabPathologyOrderContext {
        uuid Id PK
        uuid LabOrderId FK "unik parsial"
        text InitialDiagnosis "nullable"
        text RelevantHistory "nullable"
        date LastMenstrualPeriod "nullable"
        text ClinicalNote "nullable"
    }
    LabPathologyReport {
        uuid Id PK
        uuid LabOrderId FK "unik parsial"
        int FindingStatus "nullable"
        uuid AnalystUserId "nullable, tanpa FK"
        timestamp FinalizedAt "nullable"
        uuid FinalizedByUserId "nullable, tanpa FK"
        int ReopenCount
    }
    LabPathologyReportValue {
        uuid Id PK
        uuid LabPathologyReportId FK
        uuid LabPathologyParameterId FK
        varchar ParameterNameSnapshot
        text Value
    }
    LabPathologyParameter {
        uuid Id PK
        varchar ParameterCode UK
        varchar ParameterName
        bool IsActive
    }
```

### ERD — data induk dan pemetaan kategori

```mermaid
erDiagram
    LabPathologyParameter ||--o{ LabPathologyParameterCategory : "berlaku bagi"
    LabPathologyCategory ||--o{ LabPathologyParameterCategory : "memakai"
    LabPathologyCategory ||--o{ LabProcedurePathologyCategory : "menggolongkan"
    MstProcedure ||--o| LabProcedurePathologyCategory : "digolongkan"

    LabPathologyParameter {
        uuid Id PK
        varchar ParameterCode UK
        varchar ParameterName
        int SortOrder
        bool IsActive
    }
    LabPathologyCategory {
        uuid Id PK
        varchar CategoryCode UK
        varchar CategoryName
        bool IsActive
    }
    LabPathologyParameterCategory {
        uuid Id PK
        uuid LabPathologyParameterId FK
        uuid LabPathologyCategoryId FK
        bool IsRequired
    }
    LabProcedurePathologyCategory {
        uuid Id PK
        uuid ProcedureId FK "unik parsial"
        uuid LabPathologyCategoryId FK
    }
    MstProcedure {
        uuid Id PK
        bool IsLaboratory
        int LabDiscipline "nullable"
    }
```

### Status dan pemilik setiap entity

| Entity | Status | Pemilik | Catatan |
|---|---|---|---|
| `LabPathologyReport` | **`New`** | `BC-LAB` | `LAB-DC-043` |
| `LabPathologyReportValue` | **`New`** | `BC-LAB` | `LAB-DC-044` |
| `LabPathologyOrderContext` | **`New`** | `BC-LAB` | `LAB-DC-049` |
| `LabPathologyParameter` | **`New`** | `BC-LAB` | `LAB-DC-045` |
| `LabPathologyCategory` | **`New`** | `BC-LAB` | `LAB-DC-046` |
| `LabPathologyParameterCategory` | **`New`** | `BC-LAB` | `LAB-DC-047` |
| `LabProcedurePathologyCategory` | **`New`** | `BC-LAB` | `LAB-DC-048`. **`MstProcedure` nol disentuh** |
| `LabExamination` | **tidak berubah oleh PA** | `BC-LAB` | Tiga kolom `Pathology*` yang dirancang bagian 14 **dicabut** |

### Delete behavior dan index

| Relasi | `DeleteBehavior` |
|---|---|
| `LabPathologyReport` → `LabOrder` | **`Restrict`** |
| `LabPathologyOrderContext` → `LabOrder` | **`Restrict`** |
| `LabPathologyReportValue` → `LabPathologyReport` | **`Restrict`** |
| `LabPathologyReportValue` → `LabPathologyParameter` | **`Restrict`** — menonaktifkan parameter nol menghapus isi laporan (`INV-37`) |
| `LabPathologyParameterCategory` → keduanya | **`Restrict`** |
| `LabProcedurePathologyCategory` → keduanya | **`Restrict`** |

| Index | Tabel | Sifat |
|---|---|---|
| `LabOrderId` | `LabPathologyReport` | **Unik parsial** — `INV-32`, satu laporan per pesanan |
| `LabOrderId` | `LabPathologyOrderContext` | **Unik parsial** |
| (`LabPathologyReportId`, `LabPathologyParameterId`) | `LabPathologyReportValue` | **Unik parsial** — satu parameter sekali per laporan |
| `ParameterCode` | `LabPathologyParameter` | **Unik parsial** |
| `CategoryCode` | `LabPathologyCategory` | **Unik parsial** |
| (`ParameterId`, `CategoryId`) | `LabPathologyParameterCategory` | **Unik parsial** |
| `ProcedureId` | `LabProcedurePathologyCategory` | **Unik parsial** — satu pemeriksaan satu kategori |

> ### ⚠ Tujuh index unik, dan ketujuhnya WAJIB PARSIAL
>
> Pembatas `IsDelete = false` bukan kehalusan teknis. Penghapusan di sistem ini bersifat
> **penandaan**, sehingga baris tertandai hapus **tetap menempati kuncinya** — dan kepala instalasi
> yang menghapus satu parameter lalu menambahkannya lagi dengan kode yang sama akan ditolak basis
> data tanpa sebab yang masuk akal baginya.
>
> Modul ini sudah membayar persis kesalahan itu lewat `LAB-CONFLICT-005`. **Tujuh index berarti
> tujuh kesempatan mengulanginya.**

### Yang sengaja tidak digambar

| Yang dicari pembaca | Kenapa tidak ada |
|---|---|
| Kolom `IssuedAt` dan `EffectiveAt` | **Nol disimpan** (`INV-38`). Keduanya diturunkan dari `FinalizedAt` dan `LabSpecimen.CollectedAt` |
| Kolom status `Draft`/`Final` | `INV-36`. Dibaca dari `FinalizedAt` |
| Tabel gambar laporan | `DEC-LAB-016` |
| Kolom kategori pada `MstProcedure` | `LAB-DEC-087` — pemetaannya milik Laboratorium; katalognya nol disentuh |

### Delete behavior, index, dan satu jebakan

| Relasi | `DeleteBehavior` | Alasan |
|---|---|---|
| `LabMicrobiologyIsolate` → `LabExamination` | **`Restrict`** | Histori klinis tidak boleh terhapus berantai |
| `LabMicrobiologyIsolate` → `LabOrganism` | **`Restrict`** | Menonaktifkan data induk tidak boleh menghapus temuan pasien (`INV-31`) |
| `LabIsolateSusceptibility` → `LabMicrobiologyIsolate` | **`Restrict`** | Sama |
| `LabIsolateSusceptibility` → `LabAntibiotic` | **`Restrict`** | Sama |

| Index | Tabel | Sifat |
|---|---|---|
| `OrganismCode` | `LabOrganism` | **Unik** |
| `AntibioticCode` | `LabAntibiotic` | **Unik** |
| `LabExaminationId` | `LabMicrobiologyIsolate` | Biasa |
| `LabMicrobiologyIsolateId` | `LabIsolateSusceptibility` | Biasa |
| (`LabMicrobiologyIsolateId`, `LabAntibioticId`) | `LabIsolateSusceptibility` | **Unik PARSIAL** — dibatasi `IsDelete = false` |

> ### ⚠ Index unik parsial itu bukan kehalusan teknis
>
> Penghapusan di sistem ini bersifat **penandaan** (`IsDelete`), bukan penghapusan sungguhan.
> Index unik biasa akan membuat baris yang sudah ditandai hapus **tetap menempati kuncinya**,
> sehingga analis yang salah memilih antibiotik, menghapusnya, lalu memilih antibiotik yang sama
> lagi akan ditolak sistem tanpa sebab yang masuk akal baginya.
>
> Modul ini **sudah pernah membayar persis kesalahan ini** lewat `LAB-CONFLICT-005` pada index
> `(SpecimenId, ProcedureId)`. Dicatat di sini agar tidak diulang untuk ketiga kalinya.

---

## Amandemen 2026-09-21 (kedua) — `S4b` sesudah bukti cetak `LAB-EVD-005` dan `LAB-EVD-006`

Menurunkan decision log **revision 52** (`LAB-DEC-114`..`LAB-DEC-128`). Bersifat **aditif**
terhadap amandemen pertama 2026-09-21; nol entity yang sudah digambar dibongkar.

> **Peringatan yang dibawa amandemen ini.** `LAB-OPEN-039` masih menyisakan **enam varian
> cetak** yang belum pernah dilihat. `LAB-DEC-116` sudah sekali terkoreksi kurang dari satu jam
> sesudah dicatat, dan sebabnya persis itu: disimpulkan dari satu contoh. Bagian yang paling
> mungkin bergeser lagi ditandai **⚠ rapuh** di bawah.

### ERD — antibiogram sesudah bukti cetak

```mermaid
erDiagram
    LabExamination ||--o{ LabMicrobiologyIsolate : "menemukan"
    LabMicrobiologyIsolate ||--o{ LabIsolateSusceptibility : "diuji terhadap"
    LabOrganism ||--o{ LabSusceptibilityBreakpoint : "membatasi"
    LabAntibiotic ||--o{ LabSusceptibilityBreakpoint : "membatasi"
    MstProcedure ||--o| LabProcedureMicrobiologyProfile : "diprofilkan"

    LabExamination {
        uuid Id PK
        int MicrobiologyFinding "nullable"
        int ResultQualifier "nullable - BARU, Definitif/Sementara"
        int CultureType "nullable - BARU, Bakteri/Jamur"
        int SusceptibilityMethod "nullable - BARU, Difusi/Dilusi"
    }
    LabMicrobiologyIsolate {
        uuid Id PK
        uuid LabExaminationId FK
        uuid LabOrganismId FK
        varchar OrganismNameSnapshot
        bool IsSusceptibilityTested "BARU"
        varchar Note "nullable"
    }
    LabIsolateSusceptibility {
        uuid Id PK
        uuid LabMicrobiologyIsolateId FK
        uuid LabAntibioticId FK
        varchar AntibioticNameSnapshot
        decimal Concentration "nullable - MIC, dilusi"
        uuid ConcentrationUnitId FK "nullable - BARU"
        int DiscContentUgSnapshot "nullable - BARU, difusi"
        int BreakpointLowerMmSnapshot "nullable - BARU"
        int BreakpointUpperMmSnapshot "nullable - BARU"
        int ZoneDiameterMm "nullable - 0 sah, kosong berbeda"
        int ComputedResult "nullable - BARU"
        int Result
        bool IsResultOverridden "BARU"
        varchar ResultOverrideReason "nullable - BARU"
    }
    LabSusceptibilityBreakpoint {
        uuid Id PK
        uuid LabOrganismId FK
        uuid LabAntibioticId FK
        int LowerMm
        int UpperMm
        varchar GuidelineVersion "nullable"
        bool IsActive
    }
    LabProcedureMicrobiologyProfile {
        uuid Id PK
        uuid ProcedureId FK
        bool UsesSusceptibilitySet
        int DefaultCultureType "nullable"
        int DefaultSusceptibilityMethod "nullable"
        bool IsActive
    }
```

### Kenapa `ComputedResult` dan `Result` disimpan berdampingan

`LAB-DEC-123` menjadikan interpretasi **terhitung**, tetapi membuka penimpaan beralasan.

| Kolom | Isinya |
|---|---|
| `ComputedResult` | Hasil hitungan sistem dari zona terhadap breakpoint |
| `Result` | Nilai yang **berlaku** dan yang dicetak |
| `IsResultOverridden` | Benar ketika keduanya berbeda |
| `ResultOverrideReason` | **Wajib** ketika ditimpa |

> **Kenapa hitungannya ikut disimpan, bukan dihitung ulang saat dibaca.** Ia berbeda dari
> `IsCritical` yang sengaja **tidak** disimpan. Alasannya: penanda kritis menilai hasil
> terhadap aturan yang boleh berubah, sedangkan `ComputedResult` adalah **fakta apa yang
> sistem katakan pada saat analis memutuskan menimpanya**. Tanpa menyimpannya, pertanyaan
> *"analis menimpa dari apa"* kehilangan jawabannya begitu breakpoint diperbarui.

### Kenapa breakpoint di-snapshot ke baris hasil

`BreakpointLowerMmSnapshot` dan `BreakpointUpperMmSnapshot` menyalin rentang yang **berlaku
saat hasil diisi**. Ketika versi CLSI berganti dan rentangnya bergeser, hasil tahun lalu tetap
dapat dibaca dengan rentang yang dipakai ketika ia dibuat — dan cetakan ulang menghasilkan
lembar yang sama persis. Pola yang sama sudah dipakai `OrganismNameSnapshot`.

### ERD — pengaturan per disiplin

```mermaid
erDiagram
    LabDisciplineSetting {
        uuid Id PK
        int Discipline UK
        varchar ConsultantLabel
        varchar ConsultantName "nullable"
        varchar StandingNote "nullable"
        varchar ReportNumberPrefix "nullable"
        bool IsActive
    }
    LabOrder {
        uuid Id PK
        varchar OrderNumber "sudah ada - LAB-RSMMC-000000123"
        varchar LabReportNumber "nullable - BARU, 26-1246"
    }
```

`LabDisciplineSetting` memuat tiga hal yang seluruhnya **berbeda per disiplin** dan seluruhnya
terbukti dari bukti cetak:

| Ruas | Mikrobiologi | Patologi Anatomi | Patologi Klinik |
|---|---|---|---|
| `ConsultantLabel` | `Konsultan Mikrobiologi Klinik` | `Spesialis Patologi Anatomi` | `Konsultan` |
| `ConsultantName` | `Usman Chatib Warsa, PhD, SpMK-K, Prof. dr.` | `Ening Krisnuhoni, SpPA-K, dr.` | `Prof.Dr.Riadi Wirawan SpPK(K)` |
| `StandingNote` | `LEBAR ZONA ANTIBIOTIK TIDAK MEMPENGARUHI...` | *(kosong)* | *(catatan penafsiran)* |

> **Satu tabel, bukan tiga pengaturan terpisah.** Ketiganya menjawab pertanyaan yang sama —
> *"apa yang tercetak di footer disiplin ini"* — dan memisahkannya berarti tiga tempat yang
> harus diingat bersamaan ketika kop rumah sakit berubah.

### Status dan pemilik setiap entity — amandemen ini

| Entity | Status | Keputusan | Rapuh? |
|---|---|---|---|
| `LabExamination` | **`Extend`** — 3 kolom lagi | `LAB-DEC-114`, `124` | — |
| `LabMicrobiologyIsolate` | **`Extend`** — 1 kolom | `LAB-DEC-126` | — |
| `LabIsolateSusceptibility` | **`Extend`** — 7 kolom | `LAB-DEC-115`, `122`, `123` | — |
| `LabSusceptibilityBreakpoint` | **`New`** | `LAB-DEC-122` | — |
| `LabProcedureMicrobiologyProfile` | **`New`** | `LAB-DEC-125` | — |
| `LabDisciplineSetting` | **`New`** | `LAB-DEC-119`, `127` | ⚠ **rapuh** — footer tiga disiplin baru terlihat masing-masing satu contoh |
| `LabOrder` | **`Extend`** — 1 kolom | `LAB-DEC-117` | — |
| `MstProcedure` | **`Existing`** — nol disentuh | `LAB-DEC-125` | — |
| `MstMeasurement` | **`Existing`** — baris baru `ug/mL`, `mg/L` | `LAB-DEC-115` | — |

> **`MstProcedure` sengaja nol disentuh.** `LabProcedureMicrobiologyProfile` menunjuk kepadanya
> dari sisi Laboratorium, persis pola `LabProcedurePathologyCategory`. Menambah kolom pada
> tabel milik `master-data` sudah dua kali menahan modul ini lewat `LAB-COORD-006` dan
> `MST-POS-WRITE`.

### Delete behavior dan index

| Tabel | Delete behavior | Index |
|---|---|---|
| `LabSusceptibilityBreakpoint` | `Restrict` terhadap organisme dan antibiotik | Unik **parsial** atas `(LabOrganismId, LabAntibioticId)` di antara baris `IsActive` |
| `LabProcedureMicrobiologyProfile` | `Restrict` terhadap `MstProcedure` | Unik **parsial** atas `ProcedureId` |
| `LabDisciplineSetting` | Nol cascade | Unik **parsial** atas `Discipline` |
| `LabOrder.LabReportNumber` | — | Unik **parsial** atas `(Discipline, Tahun, Nomor)` |

### ⚠ Bagian yang paling mungkin bergeser lagi

| Bagian | Kenapa rapuh | Varian yang akan memastikannya |
|---|---|---|
| Susunan isolat pada cetakan | Contoh yang ada hanya punya **satu** tabel `IDENTITAS` | Cetakan **dua isolat berantibiogram** |
| Bentuk hasil nol pertumbuhan | Belum pernah terlihat | Cetakan **kultur steril** |
| Pengulangan kop per lembar | Ketiga contoh satu halaman | Cetakan **halaman kedua** |
| `LabDisciplineSetting` | Footer tiap disiplin baru terlihat satu contoh | Kategori PA lain |

**Ketiga ruas berikut sengaja dibuat nullable** justru karena keempat varian di atas belum
terlihat: `ResultQualifier`, `CultureType`, dan `SusceptibilityMethod`. Bila ternyata ada
bentuk kelima, menambah nilai enum lebih murah daripada membongkar kolom wajib.

---

## Amandemen 2026-09-21 (ketiga) — Data induk specimen dari `LAB-EVD-007`

Menurunkan decision log **revision 53** (`LAB-DEC-129`..`132`).

```mermaid
erDiagram
    LabSpecimenType ||--o{ LabSpecimenDetailType : "menaungi"
    LabSpecimenType {
        uuid Id PK
        varchar SpecimenTypeCode UK
        varchar SpecimenTypeName "isi bertambah 7 -> 31, struktur nol berubah"
        bool IsOtherBucket
    }
    LabSpecimenDetailType {
        uuid Id PK
        uuid LabSpecimenTypeId FK
        varchar DetailTypeCode UK
        varchar DetailTypeNameId "nullable - BERUBAH, boleh kosong"
        varchar DetailTypeNameEn "wajib - nama SNOMED CT"
        varchar SubTypeName "nullable - BARU, atribut pengelompokan"
        varchar SnomedCode "nullable - BARU, kosong untuk baris Lainnya"
        int SortOrder
        bool IsActive "166 baris konfidensi Rendah ter-seed bernilai salah"
    }
```

### Yang berubah dan tidak berubah

| Hal | Keadaan |
|---|---|
| `LabSpecimenType` struktur | **Nol berubah** — GUID ketujuh baris ter-seed dipertahankan |
| `LabSpecimenType` isi | 7 → **31**, ditangani **seeder**, bukan migration |
| `LabSpecimen.SpecimenTypeId` | **Nol perlu dipetakan ulang** |
| `DetailTypeNameId` | Wajib → **nullable** |

> **`SubTypeName` disimpan sebagai teks, bukan sebagai penunjuk tabel.** Ia **bukan tingkat
> pilihan** (`LAB-DEC-129`), dan mendirikan tabel untuk sesuatu yang nol pernah dipilih berarti
> tiga tabel untuk dua tingkat. Sebagai teks ia tetap dapat dikelompokkan pada pelaporan, yang
> memang satu-satunya kegunaannya.

### Index

| Kolom | Index |
|---|---|
| `SnomedCode` | Unik **parsial** di antara baris yang belum `IsDelete` **dan** `SnomedCode` tidak kosong |
| `SubTypeName` | Index biasa, untuk pengelompokan pelaporan |
| `DetailTypeNameId`, `DetailTypeNameEn` | Index untuk pencarian dua bahasa (`RULE-007`) |

> **Unik parsial pada `SnomedCode` wajib mengecualikan yang kosong.** Baris yang ditambahkan
> lewat `Lainnya` seluruhnya berkode SNOMED kosong; index unik penuh akan menolak baris lokal
> kedua.
