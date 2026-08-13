# ERD Bounded Context — Emergency Episode

| Field | Nilai |
| --- | --- |
| Blueprint | `IGD-BP-001` revision `4` |
| Bounded context | Emergency Installation |
| Commit diaudit | backend `e5331a0` |
| Basis data | PostgreSQL, schema `public` |

Diagram dipecah menjadi tiga agar setiap gambar muat dibaca dalam satu layar.

Seluruh tabel mewarisi `IdentityModel`, sehingga memiliki sepuluh kolom audit
(`CreateDateTime`, `CreateBy`, `UpdateDateTime`, `UpdateBy`, `DeleteDateTime`, `DeleteBy`,
`CancelDateTime`, `CancelBy`, `IsCancel`, `IsDelete`). Kolom-kolom itu **tidak digambar**
karena sama di setiap tabel dan akan membuat diagram tidak terbaca.

Seluruh enum disimpan sebagai `integer` melalui `HasConversion<int>` pada EF Core.

## 1. Kunjungan, triage, dan master triage

```mermaid
erDiagram
    TrxEmergencyVisit {
        uuid Id PK
        varchar EmergencyVisitNumber UK
        uuid EncounterId FK "milik Registration, unique"
        uuid PatientId FK "kosong bila belum dikenal"
        uuid ServiceUnitId FK "menentukan konteks IGD"
        uuid ArrivalModeId FK
        uuid CaseTypeId FK
        boolean IsUnknownPatient
        boolean IsImmediateCareAllowed
        int RegistrationStatus "enum"
        int VisitStatus "enum, tambah Completed"
        timestamp VisitCompletedAt
        varchar ChiefComplaint "SENSITIF"
    }
    TrxEmergencyTriage {
        uuid Id PK
        uuid EmergencyVisitId FK
        uuid TriageLevelId FK
        uuid PatientVitalSignId FK "milik Clinical"
        int Sequence "unik bersama EmergencyVisitId"
        boolean IsRetriage
        uuid PreviousTriageId FK "penilaian yang digantikan"
        int TriageSystem "enum ATS atau ESI"
        int TriageStatus "enum"
        timestamp StartedAt
        timestamp CompletedAt
        int MaxWaitingMinutesSnapshot
        timestamp ResponseDueAt "dihitung server"
        boolean ImmediateCareAllowed
        varchar RingkasanKlinis "8 kolom SENSITIF, maks 1000"
        uuid PerformedByUserId FK
        boolean IsSlaBreached "BARU"
        timestamp SlaBreachedAt "BARU"
    }
    TrxEmergencyTriageDetail {
        uuid Id PK
        uuid EmergencyTriageId FK
        uuid TriageIndicatorId FK
        boolean IsMatched
        int Sequence
        varchar SnapshotMaster "kode dan nama indikator"
    }
    MstEmergencyTriageLevel {
        uuid Id PK
        int Level "1 sampai 5"
        varchar Code UK
        varchar Name
        varchar ColorName "Merah, Kuning, Hijau, Hitam"
        varchar ColorHex
        int MaxWaitingMinutes
        boolean IsActive
    }
    MstEmergencyTriageIndicator {
        uuid Id PK
        uuid TriageLevelId FK
        varchar Code UK
        varchar Name
        varchar IndicatorGroup "ABCDE"
    }
    MstEmergencyArrivalMode {
        uuid Id PK
        varchar Code UK
        varchar Name
        boolean IsAmbulance
        boolean IsReferral
    }
    MstEmergencyCaseType {
        uuid Id PK
        varchar Code UK
        varchar Name
    }
    TrxEmergencyVisit ||--o{ TrxEmergencyTriage : "1:N — Sudah ada"
    TrxEmergencyTriage ||--o{ TrxEmergencyTriageDetail : "1:N — Sudah ada"
    TrxEmergencyTriage |o--o| TrxEmergencyTriage : "0:1 — Sudah ada, PreviousTriageId"
    MstEmergencyTriageLevel ||--o{ TrxEmergencyTriage : "1:N — Sudah ada"
    MstEmergencyTriageLevel ||--o{ MstEmergencyTriageIndicator : "1:N — Sudah ada"
    MstEmergencyTriageIndicator |o--o{ TrxEmergencyTriageDetail : "0..1:N — Sudah ada"
    MstEmergencyArrivalMode |o--o{ TrxEmergencyVisit : "0..1:N — Sudah ada"
    MstEmergencyCaseType |o--o{ TrxEmergencyVisit : "0..1:N — Sudah ada"
```

Relasi `TrxEmergencyTriage` ke dirinya sendiri adalah rantai retriage. Penilaian ulang membuat
baris baru yang menunjuk baris sebelumnya, sehingga riwayat perubahan kondisi pasien utuh dan
dapat diaudit.

Delapan kolom ringkasan klinis (`TriageReason`, `AirwaySummary`, `BreathingSummary`,
`CirculationSummary`, `DisabilitySummary`, `ExposureSummary`, `RedFlagSummary`, `Notes`)
diringkas menjadi satu baris pada diagram. Rinciannya ada di
[data-dictionary.md](data-dictionary.md).

## 2. Resusitasi, observasi, dan tindakan

```mermaid
erDiagram
    TrxEmergencyResuscitation {
        uuid Id PK
        uuid EmergencyVisitId FK
        varchar ResuscitationNumber UK
        int ResuscitationStatus "enum"
        uuid TeamLeaderDoctorId FK
        uuid RecordedByUserId FK
        timestamp StartedAt
        timestamp EndedAt
        varchar Pemicu "SENSITIF"
        varchar Outcome "SENSITIF"
    }
    TrxEmergencyObservation {
        uuid Id PK
        uuid EmergencyVisitId FK
        varchar ObservationNumber UK
        int ObservationStatus "enum"
        uuid ResponsibleDoctorId FK
        uuid ResponsibleNurseUserId FK
        timestamp StartedAt
        timestamp EndedAt
        varchar EscalationReason
    }
    TrxEmergencyObservationDetail {
        uuid Id PK
        uuid EmergencyObservationId FK
        uuid PatientVitalSignId FK "milik Clinical"
        uuid ProgressNoteId FK "CPPT, milik Clinical"
        uuid RecordedByUserId FK
        timestamp RecordedAt
        varchar KondisiKlinis "SENSITIF"
        varchar Intervensi "SENSITIF"
    }
    TrxEmergencyProcedureDetail {
        uuid Id PK
        uuid EmergencyVisitId FK
        uuid PatientProcedureId FK "unique, milik Clinical"
        uuid EmergencyResuscitationId FK "konteks opsional"
        uuid EmergencyObservationId FK "konteks opsional"
        int DetailType "enum 7 nilai"
        varchar HasilKhusus "skin test, tetanus"
    }
    TrxEmergencyVisit ||--o{ TrxEmergencyResuscitation : "1:N — Sudah ada"
    TrxEmergencyVisit ||--o{ TrxEmergencyObservation : "1:N — Sudah ada"
    TrxEmergencyVisit ||--o{ TrxEmergencyProcedureDetail : "1:N — Sudah ada"
    TrxEmergencyObservation ||--o{ TrxEmergencyObservationDetail : "1:N — Sudah ada"
    TrxEmergencyResuscitation |o--o{ TrxEmergencyProcedureDetail : "0..1:N — Sudah ada"
    TrxEmergencyObservation |o--o{ TrxEmergencyProcedureDetail : "0..1:N — Sudah ada"
```

`TrxEmergencyProcedureDetail` hanya menyimpan atribut tambahan khas IGD. Tindakan medis
sebenarnya tetap satu sumber di `TrxPatientProcedure` milik Clinical Management, dan relasinya
unique satu banding satu.

## 3. Disposition dan transfer

```mermaid
erDiagram
    TrxEmergencyDisposition {
        uuid Id PK
        uuid EmergencyVisitId FK
        uuid DispositionTypeId FK
        int DispositionStatus "enum"
        uuid DecidedByDoctorId FK
        uuid ConfirmedByUserId FK
        uuid DestinationServiceUnitId FK
        varchar ReferralNumber
        boolean IsPatientDeceased
        boolean IsVisumRequested
        timestamp DecisionAt
        varchar KondisiSaatKeluar "SENSITIF"
    }
    TrxEmergencyTransfer {
        uuid Id PK
        uuid EmergencyVisitId FK
        varchar TransferNumber UK
        uuid FromServiceUnitId FK
        uuid ToServiceUnitId FK
        uuid FromRoomId FK "menunggu entity final"
        uuid ToRoomId FK "menunggu entity final"
        uuid FromBedId FK "menunggu entity final"
        uuid ToBedId FK "menunggu entity final"
        int TransferStatus "enum 6 nilai"
        uuid RequestedByUserId FK
        uuid AcceptedByUserId FK
        timestamp RequestedAt
        timestamp ArrivedAt
        varchar HandoverSummary
    }
    MstEmergencyDispositionType {
        uuid Id PK
        varchar Code UK
        varchar Name
        boolean RequiresDestinationServiceUnit
        boolean RequiresReferralFacility
        boolean ClosesEmergencyVisit
    }
    TrxEmergencyVisit ||--o{ TrxEmergencyDisposition : "1:N — Sudah ada"
    TrxEmergencyVisit ||--o{ TrxEmergencyTransfer : "1:N — Sudah ada"
    MstEmergencyDispositionType ||--o{ TrxEmergencyDisposition : "1:N — Sudah ada"
```

Transfer terjadi setelah disposition rawat inap, ICU, atau kamar operasi, dan juga untuk
perpindahan internal. Kewenangan pengaju dan penerima wajib dipisahkan.

## Status entity

| Entity | Status | Owner | Catatan |
| --- | --- | --- | --- |
| `TrxEmergencyVisit` | Sudah ada | Emergency Installation | Enum status bertambah nilai `Completed` |
| `TrxEmergencyTriage` | **Diperbarui** | Emergency Installation | Tambah `IsSlaBreached` dan `SlaBreachedAt` |
| `TrxEmergencyTriageDetail` | Sudah ada | Emergency Installation | Menyimpan snapshot master indikator |
| `TrxEmergencyResuscitation` | Sudah ada | Emergency Installation | — |
| `TrxEmergencyObservation` | Sudah ada | Emergency Installation | — |
| `TrxEmergencyObservationDetail` | Sudah ada | Emergency Installation | Menunjuk tanda vital dan CPPT, tidak menyalinnya |
| `TrxEmergencyProcedureDetail` | Sudah ada | Emergency Installation | Satu banding satu terhadap `TrxPatientProcedure` |
| `TrxEmergencyDisposition` | Sudah ada | Emergency Installation | — |
| `TrxEmergencyTransfer` | Sudah ada | Emergency Installation | Relasi ruangan dan bed menunggu entity final |
| `MstEmergencyTriageLevel` | Sudah ada | Emergency Installation | Membutuhkan data awal |
| `MstEmergencyTriageIndicator` | Sudah ada | Emergency Installation | Membutuhkan data awal |
| `MstEmergencyArrivalMode` | Sudah ada | Emergency Installation | Membutuhkan data awal |
| `MstEmergencyCaseType` | Sudah ada | Emergency Installation | Membutuhkan data awal |
| `MstEmergencyDispositionType` | Sudah ada | Emergency Installation | Membutuhkan data awal |
| `MstEmergencySetting` | Sudah ada | Emergency Installation | Hanya satu baris default |
| `TrxPatientEncounter` | Sudah ada | Registration Management | Direferensikan, **tidak** disalin |
| `TrxPatientProcedure` | Sudah ada | Clinical Management | Direferensikan, **tidak** disalin |
| `TrxPatientVitalSign` | Sudah ada | Clinical Management | Direferensikan, **tidak** disalin |
| `TrxPatientIntegratedProgressNote` | Sudah ada | Clinical Management | Direferensikan, **tidak** disalin |

## Perilaku hapus

Seluruh relasi klinis memakai `DeleteBehavior.Restrict`, sehingga penghapusan master, pasien,
atau encounter tidak menghapus riwayat transaksi secara berantai. Penghapusan tetap berupa
penandaan `IsDelete`, bukan penghapusan baris.

Skema tabel dalam bentuk DDL ada di [data-dictionary.md](data-dictionary.md) bagian 6.
