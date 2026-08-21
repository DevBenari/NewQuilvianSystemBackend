# ERD — Episode Perawatan Rawat Inap (`CTX-INP-CARE`)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Revision | `0.3` |
| Status | `draft` |
| Backend SHA | `5afb54b` |

Kolom audit warisan `IdentityModel` tidak digambar pada ERD mana pun di dokumen ini. Rinciannya
ada di kepala [`data-dictionary.md`](./data-dictionary.md).

Diagram dipecah menjadi tiga supaya masing-masing muat dibaca dalam satu layar.

---

## 1. Inti episode dan penanggung jawab

```mermaid
erDiagram
    InpEpisode {
        uuid Id PK
        varchar EpisodeNumber UK
        uuid EncounterId FK "UK, milik Registration"
        uuid PatientId FK "salinan dari kunjungan"
        uuid ServiceUnitId FK
        uuid PatientClassId FK "kelas saat admisi dibuka"
        int EpisodeStatus "enum disimpan sebagai int"
        timestamp AdmittedAt "kosong selama Draft"
        timestamp DischargeDecidedAt
        timestamp PhysicallyLeftAt "kosong berarti pasien masih di ruangan"
        uuid PhysicallyLeftByUserId FK
        timestamp ClosedAt
        int DischargeType "enum, kosong sampai keputusan pulang"
        uuid MotherEpisodeId FK "episode ibu, hanya untuk bayi rawat gabung"
        boolean RequiresIsolation "dipakai Kelayakan Penempatan"
        int IsolationSource "1 catatan awal admisi, 2 keputusan klinis DPJP"
        uuid IsolationSetByDoctorId FK "diisi bila keputusan klinis"
        boolean IsClosedWithoutFinancialClearance
        varchar CancelReason
    }
    InpDoctorAssignment {
        uuid Id PK
        uuid EpisodeId FK
        uuid DoctorId FK
        timestamp StartDateTime
        timestamp EndDateTime "kosong berarti masih aktif"
        uuid AssignedByUserId FK
        varchar HandoverReason
    }
    InpNurseAssignment {
        uuid Id PK
        uuid EpisodeId FK
        uuid EmployeeId FK
        timestamp StartDateTime
        timestamp EndDateTime "kosong berarti masih aktif"
        uuid AssignedByUserId FK
    }
    InpStatusHistory {
        uuid Id PK
        uuid EpisodeId FK
        int SequenceNumber "unik bersama EpisodeId"
        int FromStatus "kosong pada baris pertama"
        int ToStatus
        varchar ActionType
        int ActorType "1 orang, 2 sistem"
        uuid ChangedByUserId FK "kosong bila dilakukan sistem"
        timestamp ChangedAt
        varchar Reason
    }
    TrxPatientEncounter {
        uuid Id PK
        varchar EncounterNumber UK
        uuid PatientId FK
        int EncounterType "3 berarti Inpatient"
    }
    InpEpisode |o--o| InpEpisode : "0:1 — Baru, episode ibu"
    TrxPatientEncounter ||--|| InpEpisode : "1:1 — Sudah ada"
    InpEpisode ||--o{ InpDoctorAssignment : "1:N — Baru"
    InpEpisode ||--o{ InpNurseAssignment : "1:N — Baru"
    InpEpisode ||--o{ InpStatusHistory : "1:N — Baru"
```

---

## 2. Penghunian tempat tidur

```mermaid
erDiagram
    InpEpisode {
        uuid Id PK
        int EpisodeStatus
    }
    InpBedReservation {
        uuid Id PK
        uuid EpisodeId FK
        uuid BedId FK "UK bila ReservationStatus aktif"
        timestamp ReservedAt
        timestamp ExpiresAt "disalin dari pengaturan saat dibuat"
        int ReservationStatus "enum"
        uuid ReservedByUserId FK
    }
    InpBedPlacement {
        uuid Id PK
        uuid EpisodeId FK
        uuid BedId FK "UK bila EndDateTime kosong"
        uuid RoomId FK "salinan saat penempatan dibuat"
        uuid ServiceUnitId FK "salinan saat penempatan dibuat"
        uuid PatientClassId FK "salinan saat penempatan dibuat"
        timestamp StartDateTime
        timestamp EndDateTime "kosong berarti masih ditempati"
        int EndReason "enum: Transfer, EpisodeClosed, AdmissionCancelled, PatientDeparted"
        uuid EndedByUserId FK "siapa yang mengakhiri penempatan"
        varchar TransferReason
        uuid PlacedByUserId FK
    }
    MstBed {
        uuid Id PK
        varchar BedCode UK
        uuid RoomId FK
        int BedStatus "salinan, bukan sumber kebenaran"
        boolean IsReservable
        boolean IsForNewborn
        boolean IsActive
    }
    MstRoom {
        uuid Id PK
        uuid ServiceUnitId FK
        uuid PatientClassId FK
        int RoomType "2 berarti InpatientRoom"
    }
    InpEpisode ||--o{ InpBedReservation : "1:N — Baru"
    InpEpisode ||--o{ InpBedPlacement : "1:N — Baru"
    MstBed ||--o{ InpBedReservation : "1:N — Sudah ada"
    MstBed ||--o{ InpBedPlacement : "1:N — Sudah ada"
    MstRoom ||--o{ MstBed : "1:N — Sudah ada"
```

**Empat unique index parsial yang menjaga invariant antar-baris:**

| Index | Berlaku pada baris | Menjaga |
| --- | --- | --- |
| `IX_InpBedPlacement_BedId_Active` unik atas `BedId` | `EndDateTime IS NULL` | `INV-INP-02` — satu tempat tidur satu penempatan aktif |
| `IX_InpBedReservation_BedId_Active` unik atas `BedId` | `ReservationStatus = 1` | `INV-INP-02` — satu tempat tidur satu pemesanan aktif |
| `IX_InpDoctorAssignment_EpisodeId_Active` unik atas `EpisodeId` | `EndDateTime IS NULL` | `INV-INP-03` — satu episode satu DPJP aktif |
| `IX_InpEpisode_PatientId_Present` unik atas `PatientId` | `EpisodeStatus = 1` **atau** (`EpisodeStatus = 2` dan `PhysicallyLeftAt IS NULL`) | `INV-INP-10` — satu pasien satu episode yang benar-benar hadir |

---

## 3. Pemulangan, kelayakan, dan koreksi

```mermaid
erDiagram
    InpEpisode {
        uuid Id PK
        int EpisodeStatus
        int DischargeType
    }
    InpDischargeSummary {
        uuid Id PK
        uuid EpisodeId FK "UK — satu per episode"
        text PrimaryDiagnosisText "SENSITIF"
        text SecondaryDiagnosisText "SENSITIF"
        text ProcedureSummary "SENSITIF"
        text DischargeMedicationNote "SENSITIF"
        text FollowUpInstruction "SENSITIF"
        varchar ReferralDestination "wajib bila cara pulang dirujuk"
        timestamp SignedAt "kosong berarti belum ditandatangani"
        uuid SignedByDoctorId FK
    }
    InpClearanceMark {
        uuid Id PK
        uuid EpisodeId FK "UK bersama ClearanceItemId"
        uuid ClearanceItemId FK
        timestamp MarkedAt
        uuid MarkedByUserId FK
        varchar Note
    }
    InpFinancialClearance {
        uuid Id PK
        uuid EpisodeId FK
        int SequenceNumber "unik bersama EpisodeId"
        int ClearanceStatus "enum"
        timestamp MarkedAt
        uuid MarkedByUserId FK
        varchar Note
        boolean IsManualMarking "true selama MVP"
    }
    InpCorrectionSession {
        uuid Id PK
        uuid EpisodeId FK
        int SequenceNumber "unik bersama EpisodeId"
        timestamp OpenedAt
        uuid OpenedByUserId FK
        varchar OpenReason
        timestamp ClosedAt "kosong berarti masih terbuka"
        uuid ClosedByUserId FK
        text ChangedFieldSummary
    }
    MstInpatientClearanceItem {
        uuid Id PK
        varchar ItemCode UK
        varchar ItemName
        boolean IsMandatory
        boolean IsActive
    }
    InpDischargeSummaryRevision {
        uuid Id PK
        uuid DischargeSummaryId FK
        int RevisionNumber "unik bersama DischargeSummaryId"
        uuid CorrectionSessionId FK "sesi yang menyebabkan penggantian"
        timestamp PreviousSignedAt
        uuid PreviousSignedByDoctorId FK
        timestamp SupersededAt
        uuid SupersededByUserId FK
    }
    InpDischargeSummary ||--o{ InpDischargeSummaryRevision : "1:N — Baru"
    InpCorrectionSession ||--o{ InpDischargeSummaryRevision : "1:N — Baru"
    InpEpisode ||--o| InpDischargeSummary : "1:0..1 — Baru"
    InpEpisode ||--o{ InpClearanceMark : "1:N — Baru"
    InpEpisode ||--o{ InpFinancialClearance : "1:N — Baru"
    InpEpisode ||--o{ InpCorrectionSession : "1:N — Baru"
    MstInpatientClearanceItem ||--o{ InpClearanceMark : "1:N — Baru"
```

---

## 4. Tabel status entity

| Entity | Status | Owner | Catatan |
| --- | --- | --- | --- |
| `InpEpisode` | `Baru` | InPatient Management | Aggregate root |
| `InpDoctorAssignment` | `Baru` | InPatient Management | Riwayat berperiode |
| `InpNurseAssignment` | `Baru` | InPatient Management | Riwayat berperiode, boleh kosong |
| `InpBedReservation` | `Baru` | InPatient Management | Kedaluwarsa dihitung saat dibaca |
| `InpBedPlacement` | `Baru` | InPatient Management | Sumber kebenaran penghunian |
| `InpDischargeSummary` | `Baru` | InPatient Management | Seluruh kolom isi bertanda sensitif. Menyimpan versi yang **berlaku** |
| `InpDischargeSummaryRevision` | `Baru` pada revision `0.2` | InPatient Management | Salinan versi resume yang sudah tidak berlaku. Tidak dapat diubah |
| `InpClearanceMark` | `Baru` | InPatient Management | — |
| `InpFinancialClearance` | `Baru` | InPatient Management | Sementara sampai Billing operasional |
| `InpStatusHistory` | `Baru` | InPatient Management | Tidak dapat diubah dan tidak dapat dihapus |
| `InpCorrectionSession` | `Baru` | InPatient Management | Menggantikan status episode keenam |
| `MstInpatientClearanceItem` | `Baru` | Master Data HealthServices | Lihat `02-inpatient-configuration.md` |
| `TrxPatientEncounter` | `Sudah ada` | Registration Management | Direferensikan, **MUST NOT** disalin |
| `MstBed` | `Sudah ada` | Master Data HealthServices | Direferensikan. Kolom `BedStatus` **ditulis** modul ini sebagai salinan |
| `MstRoom` | `Sudah ada` | Master Data HealthServices | Direferensikan |
| `MstServiceUnit` | `Sudah ada` | Master Data HealthServices | Direferensikan |
| `MstPatientClass` | `Sudah ada` | Master Data HealthServices | Direferensikan |
| `MstDoctor` | `Sudah ada` | Corporate HR Workforce | Direferensikan |
| `MstEmployee` | `Sudah ada` | Corporate HR Workforce | Direferensikan |
| `MstPatient` | `Sudah ada` | Patient Management | Direferensikan lewat kunjungan |

---

## 5. Perilaku hapus

Seluruh relasi ke tabel milik modul lain memakai `DeleteBehavior.Restrict`, mengikuti konvensi
project untuk relasi klinis. Alasannya: histori transaksi tidak boleh ikut terhapus berantai
ketika sebuah master dihapus.

Relasi di dalam aggregate `InpEpisode` juga memakai `Restrict`, bukan `Cascade`. Penghapusan
memang tidak pernah terjadi secara fisik — seluruh tabel memakai penandaan `IsDelete`.

**Dua pengecualian yang perlu ditegaskan:** `InpStatusHistory` dan `InpDischargeSummaryRevision`
**tidak** boleh ditandai `IsDelete` lewat endpoint mana pun. Tidak ada endpoint update maupun delete
yang disediakan untuk keduanya.

## 6. Perubahan pada revision `0.2`

| Yang berubah | Dasar |
| --- | --- |
| `InpEpisode` bertambah `PhysicallyLeftAt`, `PhysicallyLeftByUserId`, dan `MotherEpisodeId` | `RWI-DEC-055`, `RWI-DEC-056` |
| Pada revision `0.3`: `InpEpisode` bertambah enam kolom kebutuhan isolasi | `RWI-DEC-065` |
| Relasi baru `InpEpisode` ke dirinya sendiri untuk hubungan bayi dan ibu | `RWI-DEC-056` |
| `InpBedPlacementEndReason` bertambah nilai `PatientDeparted` | `RWI-DEC-055` |
| Unique index parsial keempat menjaga `INV-INP-10` | `RWI-DEC-054` |
| Entity baru `InpDischargeSummaryRevision` | `RWI-DEC-057` |
