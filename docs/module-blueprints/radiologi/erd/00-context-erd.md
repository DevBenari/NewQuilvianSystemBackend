# Radiologi — Context ERD

| Field | Value |
|---|---|
| Contract version | `RAD-ERD-CTX-001` |
| Revision | `1` |
| Status | `draft` |
| Backend SHA | `64da911` |
| Input | `RAD-DA-001-r1`, `RAD-ARCH-BE-001` |

Dokumen ini memperlihatkan **peta antar bounded context** beserta arah ketergantungannya. ERD
rinci per konteks ada di berkas terpisah pada folder yang sama.

---

## Peta Antar Konteks

```mermaid
erDiagram
    TrxPatientEncounter {
        uuid Id PK
        int EncounterStatus "milik Registration Management"
    }
    MstProcedure {
        uuid Id PK
        varchar ProcedureName "milik Health Services MasterData"
    }
    MstRadModality {
        uuid Id PK
        varchar ModalityCode UK
        boolean UsesIonisingRadiation
        boolean IsActive
    }
    RadOrder {
        uuid Id PK
        uuid EncounterId FK
        uuid ProcedureId FK
        uuid ModalityId FK
        uuid InpEpisodeId FK "kosong bila rawat jalan"
        int OrderStatus
    }
    RadStudy {
        uuid Id PK
        uuid RadOrderId FK
        varchar StudyNumber UK
        int StudyStatus
        boolean IsUsable "null bila belum dinilai"
    }
    RadReport {
        uuid Id PK
        uuid RadStudyId FK UK
        varchar ReportNumber UK
        int ReportStatus
        int CurrentVersionNumber
    }
    TrxPatientEncounter ||--o{ RadOrder : "1:N — Sudah ada"
    MstProcedure ||--o{ RadOrder : "1:N — Sudah ada"
    MstRadModality ||--o{ RadOrder : "1:N — Sudah ada"
    RadOrder ||--o{ RadStudy : "1:N — Sudah ada"
    RadStudy |o--o| RadReport : "1:0..1 — Baru"
```

---

## Arah Ketergantungan Antar Konteks

| Dari | Ke | Sifat | Yang mengalir |
|---|---|---|---|
| Registration Management | `BC-RAD-01` Ordering | Upstream | Identitas kunjungan pasien |
| Health Services MasterData | `BC-RAD-01`, `BC-RAD-02` | Upstream | Katalog prosedur |
| InPatient Management | `BC-RAD-01` | Upstream, opsional | Konteks perawatan rawat inap |
| `BC-RAD-04` Safety Policy | `BC-RAD-02` Acquisition | Upstream | Aturan keselamatan yang berlaku beserta nomor versinya |
| `BC-RAD-01` Ordering | `BC-RAD-02` Acquisition | Upstream | Pesanan yang harus dikerjakan |
| `BC-RAD-02` Acquisition | `BC-RAD-03` Reporting | Upstream | Study yang citranya dinyatakan layak |
| `BC-RAD-02` Acquisition | Billing Management | Downstream | Fakta kelayakan tagih |
| `BC-RAD-03` Reporting | Clinical Management | Downstream | Hasil bacaan, **dibaca langsung** tanpa disalin |

---

## Tabel Status Entity Lintas Konteks

| Entity | Status | Owner | Catatan |
|---|---|---|---|
| `TrxPatientEncounter` | `Sudah ada` | Registration Management | Direferensikan, **MUST NOT** disalin |
| `MstProcedure` | `Sudah ada` | Health Services MasterData | Direferensikan, **MUST NOT** disalin |
| `MstRadModality` | `Sudah ada` | Radiology Management | Data induk milik modul ini |
| `MstRadSafetyRequirement` | `Sudah ada` | Radiology Management | Data induk milik modul ini |
| `MstRadModalitySafetyRule` | **`Diperbarui`** | Radiology Management | Ditambah siklus pengesahan |
| `RadOrder` | `Sudah ada` | Radiology Management | — |
| `RadStudy` | `Sudah ada` | Radiology Management | — |
| `RadStudySafetyCheck` | `Sudah ada` | Radiology Management | — |
| `RadAcquisitionConsumption` | `Sudah ada` | Radiology Management | — |
| `RadTransitionHistory` | `Sudah ada` | Radiology Management | — |
| `RadReport` | **`Baru`** | Radiology Management | Tabel baru |
| `RadReportVersion` | **`Baru`** | Radiology Management | Tabel baru |

---

## ERD Rinci per Konteks

| Berkas | Konteks yang dibahas |
|---|---|
| [radiology-ordering-acquisition.md](radiology-ordering-acquisition.md) | `BC-RAD-01` Ordering dan `BC-RAD-02` Acquisition |
| [radiology-reporting.md](radiology-reporting.md) | `BC-RAD-03` Reporting |
| [radiology-safety-policy.md](radiology-safety-policy.md) | `BC-RAD-04` Safety Policy |
| [data-dictionary.md](data-dictionary.md) | Kamus data seluruh tabel |
