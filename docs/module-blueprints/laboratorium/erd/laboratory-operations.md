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
    TrxLabSpecimen {
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
    TrxLabTransitionHistory {
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

    LabOrder ||--o{ TrxLabSpecimen : "1:N — Diperbarui"
    LabOrder ||--o{ LabExamination : "1:N — Baru"
    TrxLabSpecimen ||--o{ LabExamination : "1:N — Baru, satu wadah menopang banyak pemeriksaan"
    TrxLabSpecimen |o--o| TrxLabSpecimen : "0:1 — Sudah ada, ambil ulang"
    MstLabRejectionReason ||--o{ TrxLabSpecimen : "1:N — Sudah ada"
    LabOrder ||--o{ TrxLabTransitionHistory : "1:N — Diperbarui"
```

### Perubahan makna yang harus dibaca dengan teliti

| Sebelum `LAB-DEC-024` | Sesudah |
|---|---|
| `TrxLabSpecimen` membawa `ProcedureId` dan salinan tarif | Keduanya pindah ke `LabExamination` |
| Satu barcode = satu pemeriksaan | Satu barcode = satu wadah nyata, yang dapat menopang beberapa pemeriksaan |
| Menolak satu baris = menolak satu pemeriksaan | Menolak satu wadah = menggugurkan seluruh pemeriksaan di atasnya |
| Baris tagihan menunjuk identitas sampel | Baris tagihan menunjuk identitas pemeriksaan |

---

## 2. Batas nilai dan persetujuan klinis

```mermaid
erDiagram
    MstLabValueBound {
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
    MstLabValueOption {
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

    MstLabValueBound ||--o{ MstLabValueOption : "1:N — Baru"
    MstLabValueBound ||--o{ LabValueBoundChangeRequest : "1:N — Baru"
    MstLabValueBound ||--o{ LabValueBoundHistory : "1:N — Baru"
```

### Aturan kunci pada kelompok ini

| Aturan | Wujudnya |
|---|---|
| Satu pemeriksaan boleh punya beberapa baris batas | Unik pada `ProcedureId` + `GenderScope` + `AgeCategoryId` |
| Satu baris punya tepat satu bentuk hasil | `ResultForm` menentukan kolom mana yang wajib terisi |
| Bentuk angka wajib bersatuan | `Unit` wajib bila `ResultForm` bernilai angka |
| Bentuk pilihan wajib punya daftar pilihan | Sekurang-kurangnya satu `MstLabValueOption` |
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
