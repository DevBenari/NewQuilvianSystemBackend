# MCU — ERD Layanan MCU

| Field | Nilai |
| --- | --- |
| Blueprint ID | `MCU-BP-001` revision 1, `approved` 2026-10-21 |
| Context | Layanan MCU (`BC-MCU-03`) |
| Keputusan | `MCU-DEC-003`..`011`, `013`..`019`, `021`..`024`, `030`..`040` |

Dipecah tiga diagram agar terbaca dalam satu layar.

## 1. Order, komponen, dan rujukan ke modul lain

```mermaid
erDiagram
    McuOrder {
        uuid Id PK
        varchar OrderNumber UK "MCU-RSMMC-000001"
        uuid PackageId FK
        uuid PatientId FK "kosong selama identitas sementara"
        varchar ParticipantNik "SENSITIF, 16"
        int Source "pintu asal"
        uuid SourceEncounterId FK "kunjungan pemberi rekomendasi"
        uuid CompanyGuarantorId FK
        date ExaminationDate
        uuid DailyScheduleId FK
        int OrderStatus "9 status"
        uuid EncounterId FK "UK bila terisi"
        uuid McuDoctorId FK "dokter MCU berlaku"
        uuid AppliedTariffId FK "tarif dipakai saat check-in"
        uuid PackageInvoiceItemId FK "baris paket di Billing"
        int Version "token konkurensi"
    }
    McuOrderComponent {
        uuid Id PK
        uuid McuOrderId FK
        uuid PackageComponentId FK "kosong = di luar paket"
        int ComponentType
        boolean IsPackageCovered
        int ComponentStatus
        date ScheduledDate "berubah bila dijadwal ulang"
        uuid LabOrderId FK
        uuid RadOrderId FK
        uuid PerformingDoctorId FK "untuk jasa medis"
        uuid RefundCaseId FK
    }
    McuOrderRequirementCheck {
        uuid Id PK
        uuid McuOrderId FK
        uuid McuOrderComponentId FK
        int Outcome
    }
    McuOrderStatusHistory {
        uuid Id PK
        uuid McuOrderId FK
        int FromStatus
        int ToStatus
        uuid ChangedByUserId FK
    }
    McuOrderDoctorAssignment {
        uuid Id PK
        uuid McuOrderId FK
        uuid DoctorId FK
        timestamp EndedAt "kosong = berlaku"
    }
    McuBookingImport {
        uuid Id PK
        varchar ImportNumber UK
        uuid CompanyGuarantorId FK
    }
    LabOrder {
        uuid Id PK
        uuid CoveringMcuOrderId FK "BARU - Diperbarui"
        uuid InstructingDoctorId FK "diisi dokter MCU"
    }
    RadOrder {
        uuid Id PK
        uuid CoveringMcuOrderId FK "BARU - Diperbarui"
        uuid InstructingDoctorId FK "diisi dokter MCU"
    }
    RegPatientEncounter {
        uuid Id PK
        int EncounterType "4 MedicalCheckup"
    }
    MstPatient {
        uuid Id PK
        varchar IdentityNumber "NIK - SENSITIF"
        date BirthDate "SENSITIF"
    }
    McuOrder ||--|{ McuOrderComponent : "1:N — Baru"
    McuOrder ||--o{ McuOrderRequirementCheck : "1:N — Baru"
    McuOrderComponent ||--o{ McuOrderRequirementCheck : "1:N — Baru"
    McuOrder ||--|{ McuOrderStatusHistory : "1:N — Baru"
    McuOrder ||--o{ McuOrderDoctorAssignment : "1:N — Baru"
    McuBookingImport |o--o{ McuOrder : "0..1:N — Baru"
    MstPatient |o--o{ McuOrder : "0..1:N — Sudah ada"
    RegPatientEncounter |o--o| McuOrder : "0..1:0..1 — Sudah ada"
    LabOrder }o--o| McuOrderComponent : "N:0..1 — Diperbarui"
    RadOrder |o--o| McuOrderComponent : "0..1:0..1 — Diperbarui"
    McuOrder |o--o{ LabOrder : "dicakup paket — Diperbarui"
    McuOrder |o--o{ RadOrder : "dicakup paket — Diperbarui"
```

Pencocokan peserta (`MCU-DEC-039`) memakai `MstPatient.IdentityNumber` dan `MstPatient.BirthDate`
(`Areas/HealthServices/PatientManagement/MasterData/Models/MstPatient.cs`).

## 2. Hasil Pemeriksaan MCU

```mermaid
erDiagram
    McuOrderComponent {
        uuid Id PK
        int ComponentType "3 = Pemeriksaan MCU"
    }
    McuExaminationResult {
        uuid Id PK
        uuid McuOrderComponentId FK "UK"
        varchar ConclusionText "SENSITIF, 2000"
        uuid FilledByUserId FK
        timestamp FilledAt
    }
    McuExaminationResultValue {
        uuid Id PK
        uuid ExaminationResultId FK
        uuid ExaminationFieldId FK "UK bersama ExaminationResultId"
        varchar ValueText "SENSITIF, 500"
    }
    McuExaminationResultValueRevision {
        uuid Id PK
        uuid ResultValueId FK
        varchar PreviousValueText "SENSITIF"
        int ResumeVersionNumber
        uuid ChangedByUserId FK
    }
    McuExaminationField {
        uuid Id PK
        varchar FieldCode
    }
    McuOrderComponent ||--o| McuExaminationResult : "1:0..1 — Baru"
    McuExaminationResult ||--o{ McuExaminationResultValue : "1:N — Baru"
    McuExaminationField ||--o{ McuExaminationResultValue : "1:N — Baru"
    McuExaminationResultValue ||--o{ McuExaminationResultValueRevision : "1:N — Baru"
```

## 3. Resume, persetujuan, penyerahan

```mermaid
erDiagram
    McuOrder {
        uuid Id PK
        int OrderStatus
    }
    McuResume {
        uuid Id PK
        uuid McuOrderId FK
        int VersionNumber "UK bersama McuOrderId"
        int ResumeStatus "1 Draf, 2 Disetujui, 3 Terverifikasi, 4 Dikoreksi"
        boolean IsCurrent "UK filtered per order"
        varchar GeneralConditionText "SENSITIF"
        varchar ImportantFindingsText "SENSITIF"
        varchar RecommendationText "SENSITIF"
        uuid FitnessCategoryId FK
        varchar WorkRecommendationText "SENSITIF, boleh ke perusahaan"
        uuid ApprovedByDoctorId FK
        uuid VerifiedByUserId FK
    }
    McuReleaseConsent {
        uuid Id PK
        uuid McuOrderId FK
        uuid CompanyGuarantorId FK
        int EvidenceType
        timestamp RevokedAt "kosong = berlaku"
    }
    McuResultDelivery {
        uuid Id PK
        uuid McuOrderId FK
        uuid McuResumeId FK
        int RecipientType "1 Peserta, 2 Perusahaan"
        int ContentType "1 Lengkap, 2 Ringkasan kelayakan"
        int Channel "1 Ambil langsung, 2 WhatsApp, 3 Email, 4 Telepon"
        varchar DestinationText "SENSITIF"
        boolean RequiresCorrectionNotice
    }
    McuFitnessCategory {
        uuid Id PK
        varchar FitnessCategoryCode
    }
    McuOrder ||--o{ McuResume : "1:N — Baru"
    McuFitnessCategory |o--o{ McuResume : "0..1:N — Baru"
    McuOrder ||--o{ McuReleaseConsent : "1:N — Baru"
    McuOrder ||--o{ McuResultDelivery : "1:N — Baru"
    McuResume ||--o{ McuResultDelivery : "1:N — Baru"
```

## 4. Tabel status entity

| Entity | Status | Owner | Catatan |
| --- | --- | --- | --- |
| `McuOrder` | Baru | MCU | Aggregate root |
| `McuOrderComponent` | Baru | MCU | Termasuk komponen di luar paket |
| `McuOrderRequirementCheck` | Baru | MCU | — |
| `McuOrderStatusHistory` | Baru | MCU | Ditulis setiap perpindahan status |
| `McuOrderDoctorAssignment` | Baru | MCU | Penetapan + penggantian dokter MCU |
| `McuBookingImport` | Baru | MCU | Berkas tidak disimpan |
| `McuExaminationResult`, `McuExaminationResultValue`, `McuExaminationResultValueRevision` | Baru | MCU | — |
| `McuResume` | Baru | MCU | Berversi |
| `McuReleaseConsent` | Baru | MCU | — |
| `McuResultDelivery` | Baru | MCU | — |
| `LabOrder` | **Diperbarui** | Laboratorium | Tambah `CoveringMcuOrderId` |
| `RadOrder` | **Diperbarui** | Radiologi | Tambah `CoveringMcuOrderId` |
| `RegPatientEncounter`, `MstPatient`, `BilInvoiceItem`, `BilRefundCase`, `MstDoctor` | Sudah ada | Modul masing-masing | Dirujuk, **tidak** disalin |
