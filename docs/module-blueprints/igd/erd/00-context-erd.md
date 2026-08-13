# Context ERD — Modul IGD

| Field | Nilai |
| --- | --- |
| Blueprint | `IGD-BP-001` revision `4` |
| Cakupan | Hubungan antar bounded context, bukan detail kolom |
| Commit diaudit | backend `e5331a0` |

Diagram ini memperlihatkan bagaimana IGD terhubung ke modul lain. Aturannya satu: IGD
**menunjuk**, tidak menyalin.

```mermaid
erDiagram
    TrxPatientEncounter ||--o| TrxEmergencyVisit : "1:0..1 — Sudah ada"
    Patient ||--o{ TrxEmergencyVisit : "1:N — Sudah ada, direferensikan"
    MstServiceUnit ||--o{ TrxEmergencyVisit : "1:N — Sudah ada"
    TrxEmergencyVisit ||--o{ TrxEmergencyTriage : "1:N — Sudah ada"
    TrxEmergencyVisit ||--o{ TrxEmergencyResuscitation : "1:N — Sudah ada"
    TrxEmergencyVisit ||--o{ TrxEmergencyObservation : "1:N — Sudah ada"
    TrxEmergencyVisit ||--o{ TrxEmergencyDisposition : "1:N — Sudah ada"
    TrxEmergencyVisit ||--o{ TrxEmergencyTransfer : "1:N — Sudah ada"
    TrxPatientProcedure ||--o| TrxEmergencyProcedureDetail : "1:0..1 — Sudah ada"
    TrxPatientVitalSign ||--o{ TrxEmergencyObservationDetail : "1:N — Sudah ada, direferensikan"
    TrxPatientIntegratedProgressNote ||--o{ TrxEmergencyObservationDetail : "1:N — Sudah ada, direferensikan"
```

## Batas kepemilikan

| Bounded context | Modul pemilik | Cara IGD memakainya |
| --- | --- | --- |
| Encounter | Registration Management | `TrxEmergencyVisit.EncounterId` |
| Pasien | Patient Management | `TrxEmergencyVisit.PatientId` |
| Unit pelayanan, ruangan, bed | Master Data | `ServiceUnitId`, `FromRoomId`, `ToBedId`, dan sejenisnya |
| Tindakan klinis | Clinical Management | `TrxEmergencyProcedureDetail.PatientProcedureId` |
| Tanda vital | Clinical Management | `TrxEmergencyObservationDetail.PatientVitalSignId` |
| CPPT | Clinical Management | `TrxEmergencyObservationDetail.ProgressNoteId` |
| Approval dan delegasi | Workflow Management | `TrxWorkflowInstance.ReferenceType` = `EmergencyVisit`, `ReferenceId` = `TrxEmergencyVisit.Id` |
| Billing | Billing Management | Melalui `EncounterId`; **tidak** menjadi syarat penutupan klinis sesuai `IGD-DEC-021` |

## Arah ketergantungan

IGD bergantung pada modul pusat, tidak sebaliknya. Modul pusat tidak boleh mengetahui adanya
IGD. Konsekuensinya:

- perubahan pada IGD tidak boleh memaksa perubahan pada Clinical Management;
- IGD tidak boleh menambah kolom pada entitas milik modul lain;
- kebutuhan lintas modul diselesaikan lewat relasi atau adapter, bukan penambahan kolom.

## Yang tidak muncul di diagram ini

Modul Laboratory dan Radiology terhubung ke IGD hanya melalui `EncounterId` yang sama, tanpa
relasi langsung ke entitas IGD. Karena itu keduanya tidak digambar sebagai relasi.
