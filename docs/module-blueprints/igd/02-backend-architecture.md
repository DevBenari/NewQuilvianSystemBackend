# Arsitektur Backend — Modul IGD

| Field | Nilai |
| --- | --- |
| Blueprint | `IGD-BP-001` revision `5`; **bagian 13 ditambahkan 22 September 2026** (encounter-first, revisi `6` `draft`) |
| Status | `draft` — **belum disetujui siapa pun**. Diturunkan dari `IGD-DEC-067` sampai `IGD-DEC-088` yang seluruhnya masih `draft` |
| Commit diaudit | backend `f69e9e483052845d11c91d8b7bbdce33c4acc8d8`, frontend `96a9120111f6acc6b7c0f37973ea0c717ba41f17` |
| Masukan | `00-interview-decisions.md` (88 keputusan), `01-existing-capability-map.md` revision `3` |
| Keputusan yang mengikat | `IGD-DEC-067` sampai `IGD-DEC-088`; keputusan lama `IGD-DEC-001` sampai `IGD-DEC-066` tetap berlaku kecuali dinyatakan `superseded` |
| Gerbang kemampuan rumah sakit | **BELUM TERPENUHI** — lihat bagian 0 |
| Diselaraskan | **16 September 2026 (ketiga)** — `EmgDoctorAssignment` diperiksa ulang terhadap `IGD-DEC-130`: class diagram baris 131 dan configuration baris 456 **sudah** benar, yaitu tanpa `IsActive` dan bersandar pada `EffectiveTo IS NULL`. Nol perubahan dibutuhkan pada dokumen ini; yang diselaraskan adalah kamus data §4. Sebelumnya 15 September 2026 — model `TrxEmergencyDoctorAssignment` beserta configuration-nya menjadi `EmgDoctorAssignment` (`IGD-DEC-116`). Nama service, controller, DTO, dan kolom tidak berubah. Nama entity IGD lain pada dokumen ini masih nama rancangan sebelum prefix `Emg` 27 Agustus 2026 |

Modul IGD menyimpan proses yang benar-benar khusus kegawatdaruratan. Data klinis yang dipakai
lintas pelayanan tetap dimiliki modul pusat agar tidak terjadi duplikasi antara Rawat Jalan,
IGD, dan Rawat Inap.

Seluruh tabel mewarisi `IdentityModel`, sehingga memiliki sepuluh kolom audit yang tidak
diulang pada dokumen ini: `CreateDateTime`, `CreateBy`, `UpdateDateTime`, `UpdateBy`,
`DeleteDateTime`, `DeleteBy`, `CancelDateTime`, `CancelBy`, `IsCancel`, dan `IsDelete`.
Penghapusan bersifat penandaan, bukan penghapusan baris.

---

## 0. Gerbang yang belum terpenuhi

Dokumen ini disusun walaupun dua artefak hulu yang diwajibkan untuk kemampuan bisnis rumah
sakit **tidak ada** pada blueprint IGD:

| Artefak hulu yang diwajibkan | Keadaan pada blueprint IGD | Keadaan pada blueprint Rawat Inap sebagai pembanding |
| --- | --- | --- |
| `evidence/02-requirement-completeness-gate.md` | **Tidak ada** | Ada, 728 baris |
| `evidence/03-hospital-domain-architecture.md` | **Tidak ada** | Ada, 764 baris |

**Alasan tetap dilanjutkan.** Substansi yang biasanya dihasilkan kedua skill itu — bounded
context, ownership konsep, invariant, lifecycle, batas billing, dan batas keselamatan klinis —
memang sudah tercatat, tetapi tersebar pada 88 keputusan di decision log dan pada revision `4`
dokumen ini, bukan pada berkas hulu tersendiri.

**Akibat yang harus diketahui pembaca.** Modul IGD tidak memiliki klasifikasi kesiapan
requirement per slice (`READY_FOR_DOMAIN_DESIGN`, `BUSINESS_DECISION_REQUIRED`, dan
sejenisnya) sebagaimana dimiliki Rawat Inap. Karena itu tidak ada cara ringkas menyatakan
slice mana yang secara formal siap dan slice mana yang belum. Yang tersedia hanyalah status
per keputusan.

Gerbang ini **tidak** ditandai terpenuhi. Bila Product/Domain Owner menghendaki kesetaraan
dengan Rawat Inap, `/requirement-completeness-gate` dan `hospital-domain-architect` perlu
dijalankan untuk IGD, dan dokumen ini ditinjau ulang terhadap hasilnya.

---

## 1. Kepemilikan data

Tabel ini adalah pertahanan paling langsung terhadap duplikasi entitas. Baris bertanda
**baru** ditambahkan pada revision `5`.

| Kelompok data | Modul pemilik | Dipakai IGD | Dibuat ulang di IGD |
| --- | --- | :---: | --- |
| Pasien dan identitas | Patient Management | Ya | Tidak |
| Encounter atau episode pelayanan | Registration Management | Ya | Tidak |
| Assessment, SOAP, diagnosis, tindakan, CPPT, tanda vital | Clinical Management | Ya | Tidak |
| Resep dan obat | Pharmacy Management | Ya | Tidak |
| **Pemesanan laboratorium** *(baru)* | **Laboratory Management** | Ya | **Tidak** — `IGD-DEC-087` |
| Unit pelayanan, ruangan, dan bed | Master Data | Ya | Tidak |
| **Kelas pasien** *(baru)* | **Master Data** | Ya | **Tidak** — `IGD-DEC-076` memakai penanda `IsForEmergency` yang sudah ada |
| **Episode rawat inap dan penempatan bed** *(baru)* | **Inpatient Management** (`RWI-BP-001`, belum diimplementasikan) | Ya | **Tidak** — `IGD-DEC-069` |
| **Penugasan pegawai ke simpul organisasi** *(baru)* | **Corporate/HR** | Ya | **Tidak** — `IGD-DEC-086` memakai `WfpOrganizationAssignment` yang sudah ada |
| Billing dan penjaminan | Billing Management | Ya | Tidak |
| Triage, resusitasi, observasi, disposition, kepergian pasien | Emergency Installation | Ya | **Ya, karena khusus IGD** |
| **Riwayat penugasan dokter pemeriksa IGD** *(baru)* | **Emergency Installation** | Ya | **Ya, karena khusus IGD** — `IGD-DEC-082` |
| Approval bertingkat, maker-checker, delegasi | Workflow Management | Ya | **Tidak** — engine generik lewat `ReferenceType`/`ReferenceId` |

Penghubung ke seluruh data klinis tetap `EncounterId`. Konteks IGD dibedakan melalui
`EncounterType.Emergency` sesuai `IGD-DEC-074`, bukan melalui penyalinan entitas.

### 1.1 Tiga perubahan pada tabel milik modul lain

Revision `5` mengusulkan perubahan pada tiga tabel yang **bukan milik IGD**. Ketiganya wajib
disetujui pemiliknya sebelum dikerjakan.

| Tabel | Pemilik | Perubahan | Keputusan | Mengapa tidak dapat dihindari |
| --- | --- | --- | --- | --- |
| `TrxPatientEncounter` | Registration Management | Satu kolom `OriginEncounterId` yang boleh kosong | `IGD-DEC-075` | `RWI-RULE-029` aturan 2 mewajibkan kedua kunjungan terhubung. Tidak ada tempat lain yang dapat menampung hubungan antar-kunjungan |
| `MstServiceUnit` | Master Data | Satu kolom `OrganizationUnitId` yang boleh kosong | `IGD-DEC-086` | Satu-satunya mata rantai yang putus antara pengguna dan unit pelayanan |
| `TrxPatientAssessment`, `TrxDoctorConsultation`, `TrxPatientDiagnosis`, `TrxPatientProcedure`, `TrxPrescription` | Clinical Management dan Pharmacy Management | Pelonggaran kewajiban `QueueId` dan `ConsultationId` untuk kunjungan bertipe `Emergency`; penambahan kolom penanda versi untuk koreksi append-only | `IGD-DEC-068`, `IGD-DEC-080` | Tanpa ini pengkajian, diagnosis, tindakan, dan resep pasien IGD **tidak dapat disimpan sama sekali** |

Baris ketiga adalah **satu-satunya dependency eksternal yang menahan rilis pertama**. Dua
lainnya tidak.

---

## 2. Class diagram

Diagram dipecah per konteks agar setiap gambar muat dibaca dalam satu layar. Penanda:
`«new»` entity baru, `«upd»` entity diperbarui, tanpa penanda berarti sudah ada dan tidak
berubah.

### 2.1 Kunjungan, triase, dan penetapan dokter

```mermaid
classDiagram
    class TrxPatientEncounter {
        +Guid Id
        +Guid PatientId
        +Guid ServiceUnitId
        +Guid? PatientClassId
        +EncounterType EncounterType
        +Guid? OriginEncounterId «upd»
    }
    class TrxEmergencyVisit {
        +Guid Id
        +string EmergencyVisitNumber
        +Guid? EncounterId
        +EmergencyRegistrationStatus RegistrationStatus
        +EmergencyVisitStatus VisitStatus
        +DateTime? VisitCompletedAt
    }
    class TrxEmergencyTriage {
        +Guid Id
        +Guid EmergencyVisitId
        +Guid TriageLevelId
        +int Sequence
        +EmergencyTriageStatus TriageStatus
        +Guid? PreviousTriageId
        +bool IsSlaBreached
    }
    class TrxEmergencyTriageDetail {
        +Guid Id
        +Guid EmergencyTriageId
        +string IndicatorCodeSnapshot
    }
    class EmgDoctorAssignment {
        +Guid Id «new»
        +Guid EmergencyVisitId
        +Guid DoctorId
        +DateTime EffectiveFrom
        +DateTime? EffectiveTo
        +Guid AssignedByUserId
        +string? AssignmentReason
    }
    class MstEmergencyTriageLevel {
        +Guid Id
        +int Level
        +int? MaxWaitingMinutes
        +bool AllowsTreatmentBeforeRegistration
    }

    TrxPatientEncounter "1" --> "0..1" TrxEmergencyVisit
    TrxPatientEncounter "0..1" --> "0..1" TrxPatientEncounter : OriginEncounterId
    TrxEmergencyVisit "1" --> "0..*" TrxEmergencyTriage
    TrxEmergencyTriage "1" --> "0..*" TrxEmergencyTriageDetail
    TrxEmergencyTriage "0..1" --> "0..1" TrxEmergencyTriage : PreviousTriageId
    TrxEmergencyTriage "*" --> "1" MstEmergencyTriageLevel
    TrxEmergencyVisit "1" --> "0..*" EmgDoctorAssignment
```

### 2.2 Resusitasi, observasi, dan tindakan

Tidak berubah pada revision `5`. Ketiganya tetap milik IGD dan tetap menempel pada
`EmergencyVisitId`.

```mermaid
classDiagram
    class TrxEmergencyVisit
    class TrxEmergencyResuscitation {
        +Guid Id
        +Guid EmergencyVisitId
        +EmergencyResuscitationStatus ResuscitationStatus
    }
    class TrxEmergencyObservation {
        +Guid Id
        +Guid EmergencyVisitId
        +EmergencyObservationStatus ObservationStatus
    }
    class TrxEmergencyObservationDetail {
        +Guid Id
        +Guid EmergencyObservationId
        +Guid? PatientVitalSignId
    }
    class TrxEmergencyProcedureDetail {
        +Guid Id
        +Guid EmergencyVisitId
        +Guid PatientProcedureId
    }

    TrxEmergencyVisit "1" --> "0..*" TrxEmergencyResuscitation
    TrxEmergencyVisit "1" --> "0..*" TrxEmergencyObservation
    TrxEmergencyObservation "1" --> "0..*" TrxEmergencyObservationDetail
    TrxEmergencyVisit "1" --> "0..*" TrxEmergencyProcedureDetail
```

### 2.3 Tindak lanjut dan kepergian pasien

```mermaid
classDiagram
    class TrxEmergencyVisit
    class TrxEmergencyDisposition {
        +Guid Id
        +Guid EmergencyVisitId
        +Guid DispositionTypeId
        +EmergencyDispositionStatus DispositionStatus
        +DateTime? ExecutedAt
    }
    class MstEmergencyDispositionType {
        +Guid Id
        +string Code
        +bool ClosesEmergencyVisit
        +bool RequiresDestinationServiceUnit
    }
    class TrxEmergencyDeparture {
        +Guid Id «upd»
        +Guid EmergencyVisitId
        +string DepartureNumber
        +Guid ToServiceUnitId
        +EmergencyPhysicalStatus PhysicalStatus «new»
        +EmergencyHandoverStatus HandoverStatus «new»
        +string? SituationSummary «new»
        +string? BackgroundSummary «new»
        +string? AssessmentSummary «new»
        +string? RecommendationSummary «new»
    }
    class TrxEmergencyDepartureEvent {
        +Guid Id «new»
        +Guid EmergencyDepartureId
        +EmergencyDepartureEventType EventType
        +DateTime OccurredAt
        +DateTime RecordedAt
        +Guid RecordedByUserId
        +bool IsEffective
        +Guid? SupersedesEventId
        +Guid? ApprovedByUserId
    }
    class TrxEmergencyHandoverOrderItem {
        +Guid Id «new»
        +Guid EmergencyDepartureId
        +EmergencyOrderKind OrderKind
        +EmergencyOrderSource OrderSource «rev6»
        +Guid? OrderReferenceId «rev6»
        +string? ExternalReference «rev6»
        +string OrderDescription «rev6»
        +EmergencyOrderAction Action
        +string? ActionReason
        +Guid ActionByUserId «rev6»
        +DateTime ActionAt «rev6»
        +EmergencyOrderAcceptanceStatus AcceptanceStatus «rev6»
        +Guid? AcceptedByUserId «rev6»
        +DateTime? AcceptedAt «rev6»
        +string? RejectionReason «rev6»
        +bool IsEffective «rev6»
        +Guid? SupersedesOrderItemId «rev6»
    }

    TrxEmergencyVisit "1" --> "0..*" TrxEmergencyDisposition
    TrxEmergencyDisposition "*" --> "1" MstEmergencyDispositionType
    TrxEmergencyVisit "1" --> "0..*" TrxEmergencyDeparture
    TrxEmergencyDeparture "1" --> "1..*" TrxEmergencyDepartureEvent
    TrxEmergencyDeparture "1" --> "0..*" TrxEmergencyHandoverOrderItem
    TrxEmergencyDepartureEvent "0..1" --> "0..1" TrxEmergencyDepartureEvent : SupersedesEventId
    TrxEmergencyHandoverOrderItem "0..1" --> "0..1" TrxEmergencyHandoverOrderItem : SupersedesOrderItemId
```

#### Penjelasan field baru revisi 6 pada `TrxEmergencyHandoverOrderItem`

| Field | Arti | Keputusan |
| --- | --- | --- |
| `OrderSource` | `Internal` bila pesanan punya baris di sistem; `External` bila dibuat di luar sistem | `IGD-DEC-103` |
| `OrderReferenceId` | **Boleh kosong.** Terisi hanya untuk `Internal` | `IGD-DEC-103` |
| `ExternalReference` | **Wajib** bila `OrderSource` = `External`. Identitas atau nomor rujukan dari sistem luar | `IGD-DEC-103` |
| `OrderDescription` | **Wajib selalu.** Deskripsi yang dapat diaudit — tanpa ini pesanan luar sistem tidak dapat ditelusuri sama sekali | `IGD-DEC-103` |
| `ActionByUserId`, `ActionAt` | Pelaku dan waktu penetapan sikap. **Wajib**, termasuk untuk pesanan laboratorium yang sikapnya ditetapkan manual | `IGD-DEC-101` |
| `ActionReason` | **Wajib** bila `Action` = `Cancel`; opsional selain itu | `IGD-DEC-100` butir (c) |
| `AcceptanceStatus` | Penerimaan **per pesanan**, terpisah dari `EmergencyHandoverStatus` milik dokumen serah terima | `IGD-DEC-102` |
| `AcceptedByUserId`, `AcceptedAt` | Terisi saat unit penerima menerima pesanan | `IGD-DEC-102` |
| `RejectionReason` | **Wajib** bila `AcceptanceStatus` = `Rejected` | `IGD-DEC-102` |
| `IsEffective`, `SupersedesOrderItemId` | Sikap pengganti setelah penolakan ditulis sebagai **baris baru** yang menunjuk baris lama; baris lama ditandai tidak berlaku dan **tidak dihapus** | `IGD-DEC-102` butir (c), mengikuti pola tambah-saja `IGD-DEC-090` |

> **Mengapa penerimaan tidak menumpang `EmergencyHandoverStatus`.** `IGD-DEC-102` menyatakan
> penerimaan pasien, dokumen serah terima, dan setiap pesanan adalah **tiga fakta terpisah**.
> Menumpangkannya berarti satu pesanan yang ditolak akan menggagalkan penerimaan pasien —
> akibat yang secara tegas dilarang butir (d).

> **Penamaan.** `TrxEmergencyTransfer` diganti nama menjadi `TrxEmergencyDeparture` mengikuti
> `IGD-DEC-069` yang mengubah artinya dari "perpindahan beserta tempat tidur" menjadi "catatan
> kepergian pasien dari IGD". Nama lama menyesatkan setelah urusan tempat tidur pindah ke
> Rawat Inap. Penggantian nama tabel dibahas pada bagian 6 dan 7.

### 2.4 Satu penafsiran desain yang perlu dikonfirmasi

`IGD-DEC-070` memilih **dua kolom status** pada satu baris dan menolak bentuk daftar kejadian.
Namun `IGD-DEC-065`, `IGD-DEC-066`, `IGD-DEC-080`, dan `IGD-DEC-085` menuntut hal-hal yang
tidak dapat disimpan pada kolom status: waktu kejadian sebenarnya yang berbeda dari waktu
pencatatan, koreksi yang tidak menimpa, pembalikan yang butuh persetujuan orang kedua, dan
pemberitahuan ke catatan turunan.

Desain ini menyatukan keduanya:

| Kebutuhan | Diwadahi oleh |
| --- | --- |
| Membaca keadaan sekarang dengan cepat, dan menyaring daftar pantau | Dua kolom status pada `TrxEmergencyDeparture` — sesuai `IGD-DEC-070` |
| Menyimpan setiap perubahan beserta pelaku, waktu server, waktu sebenarnya, alasan, koreksi, dan pembalikan | `TrxEmergencyDepartureEvent`, bersifat tambah-saja — sesuai `IGD-DEC-065`, `066`, `080`, `085` |

Kolom status adalah **turunan** dari kejadian terakhir yang berlaku, bukan sumber kebenaran
tandingan. Setiap penulisan kejadian memperbarui kolom status dalam transaksi yang sama.

Penafsiran ini **belum dikonfirmasi owner**. Bila Product/Domain Owner menganggap
`IGD-DEC-070` melarang tabel kejadian sama sekali, maka `IGD-DEC-065`, `066`, `085` tidak
dapat dijalankan dan ketiganya harus ditinjau ulang. Dicatat sebagai `IGD-OQ-068`.

---

## 3. Penjelasan class

### 3.1 Model transaksi milik IGD

| Class | Status | Lokasi file | Kegunaan |
| --- | --- | --- | --- |
| `TrxEmergencyVisit` | Sudah ada | `Areas/HealthServices/EmergencyInstallationManagement/Models/TrxEmergencyVisit.cs` | Kunjungan IGD sebagai perluasan kunjungan pasien |
| `TrxEmergencyTriage` | Sudah ada | `.../Models/TrxEmergencyTriage.cs` | Penilaian dan penilaian ulang triase |
| `TrxEmergencyTriageDetail` | Sudah ada | `.../Models/TrxEmergencyTriageDetail.cs` | Indikator yang diamati beserta salinan nilai masternya |
| `TrxEmergencyResuscitation` | Sudah ada | `.../Models/TrxEmergencyResuscitation.cs` | Catatan resusitasi |
| `TrxEmergencyObservation` | Sudah ada | `.../Models/TrxEmergencyObservation.cs` | Periode observasi |
| `TrxEmergencyObservationDetail` | Sudah ada | `.../Models/TrxEmergencyObservationDetail.cs` | Pengamatan berkala dalam satu periode observasi |
| `TrxEmergencyProcedureDetail` | Sudah ada | `.../Models/TrxEmergencyProcedureDetail.cs` | Rincian khas IGD atas tindakan milik Clinical Management |
| `TrxEmergencyDisposition` | Sudah ada | `.../Models/TrxEmergencyDisposition.cs` | Keputusan tindak lanjut |
| `TrxEmergencyDeparture` | **Diperbarui** | `.../Models/TrxEmergencyDeparture.cs` | Catatan kepergian pasien dari IGD. Berganti nama dari `TrxEmergencyTransfer` |
| `TrxEmergencyDepartureEvent` | **Baru** | `.../Models/TrxEmergencyDepartureEvent.cs` | Riwayat kejadian kepergian, bersifat tambah-saja |
| `TrxEmergencyHandoverOrderItem` | **Baru** | `.../Models/TrxEmergencyHandoverOrderItem.cs` | Sikap atas setiap pesanan yang belum selesai saat pasien pergi |
| `EmgDoctorAssignment` | **Baru** | `.../Models/EmgDoctorAssignment.cs` | Riwayat penugasan dokter pemeriksa pada satu kunjungan IGD. `[Table("EmgDoctorAssignment")]` (`IGD-DEC-116`) |

### 3.2 Model master

| Class | Status | Lokasi file | Perubahan |
| --- | --- | --- | --- |
| `MstEmergencyTriageLevel` | Sudah ada | `Areas/HealthServices/EmergencyInstallationManagement/MasterData/Models/MstEmergencyTriageLevel.cs` | Tidak ada |
| `MstEmergencyTriageIndicator` | Sudah ada | `.../EmergencyInstallationManagement/MasterData/Models/MstEmergencyTriageIndicator.cs` | Tidak ada |
| `MstEmergencyArrivalMode` | Sudah ada | `.../EmergencyInstallationManagement/MasterData/Models/MstEmergencyArrivalMode.cs` | Tidak ada |
| `MstEmergencyCaseType` | Sudah ada | `.../EmergencyInstallationManagement/MasterData/Models/MstEmergencyCaseType.cs` | Tidak ada |
| `MstEmergencyDispositionType` | Sudah ada | `.../EmergencyInstallationManagement/MasterData/Models/MstEmergencyDispositionType.cs` | Tidak ada struktur; `ClosesEmergencyVisit` mulai dibaca |
| `MstEmergencySetting` | **Diperbarui** | `.../EmergencyInstallationManagement/MasterData/Models/MstEmergencySetting.cs` | Empat kolom mati diberi arti atau dicabut — lihat bagian 5 |
| `MstServiceUnit` | **Diperbarui** | `.../MasterData/Models/MstServiceUnit.cs` | Tambah `OrganizationUnitId` — **milik Master Data, bukan IGD** |
| `MstPatientClass` | Sudah ada | `.../MasterData/Models/MstPatientClass.cs` | Tidak ada. `IsForEmergency` sudah tersedia dan mulai dipakai |

### 3.3 Model milik modul lain yang berubah

| Class | Pemilik | Status | Lokasi file | Perubahan |
| --- | --- | --- | --- | --- |
| `TrxPatientEncounter` | Registration Management | **Diperbarui** | `Areas/HealthServices/RegistrationManagement/Models/TrxPatientEncounter.cs` | Tambah `OriginEncounterId` (`Guid?`) |
| `TrxPatientAssessment` | Clinical Management | **Diperbarui** | `Areas/HealthServices/ClinicalManagement/Models/TrxPatientAssessment.cs` | `QueueId` menjadi `Guid?`; tambah penanda versi `IsEffective`, `AmendsAssessmentId`, `AmendmentReason` |
| `TrxDoctorConsultation` | Clinical Management | **Diperbarui** | `.../ClinicalManagement/Models/TrxDoctorConsultation.cs` | `QueueId` menjadi `Guid?` |
| `TrxPatientVitalSign` | Clinical Management | **Diperbarui** | `.../ClinicalManagement/Models/TrxPatientVitalSign.cs` | Tambah penanda versi `IsEffective`, `AmendsVitalSignId`, `AmendmentReason` |
| `TrxPatientIntegratedProgressNote` | Clinical Management | **Diperbarui** | `.../ClinicalManagement/Models/TrxPatientIntegratedProgressNote.cs` | Tambah penanda versi yang sama |
| `TrxPatientDiagnosis`, `TrxPatientProcedure` | Clinical Management | **Diperbarui** | `.../ClinicalManagement/Models/` | `ConsultationId` menjadi `Guid?` |
| `TrxPrescription` | Pharmacy Management | **Diperbarui** | `Areas/HealthServices/PharmacyManagement/Models/TrxPrescription.cs` | `ConsultationId` menjadi `Guid?` |

> Seluruh baris pada tabel 3.3 **tidak boleh dikerjakan** sebelum pemilik modulnya ditunjuk
> dan menyetujui. Lihat bagian 1.1 dan bagian 9.

### 3.4 Enum

| Enum | Status | Lokasi file | Nilai |
| --- | --- | --- | --- |
| `EmergencyVisitStatus` | Sudah ada | `.../EmergencyInstallationManagement/Enums/EmergencyVisitStatus.cs` | `Arrived`=1 … `Completed`=9 |
| `EmergencyTriageStatus` | Sudah ada | `.../Enums/EmergencyTriageStatus.cs` | `Draft`=1 … `Cancelled`=5 |
| `EmergencyRegistrationStatus` | Sudah ada | `.../Enums/EmergencyRegistrationStatus.cs` | `Pending`=1 … `Cancelled`=5 |
| `EmergencyDispositionStatus` | Sudah ada | `.../Enums/EmergencyDispositionStatus.cs` | `Draft`=1 … `Cancelled`=4 |
| `EmergencyObservationStatus`, `EmergencyResuscitationStatus`, `EmergencyProcedureDetailType`, `EmergencyTriageSystem` | Sudah ada | `.../Enums/` | Tidak berubah |
| `EmergencyTransferStatus` | **Digantikan** | `.../Enums/EmergencyTransferStatus.cs` | Dipecah menjadi dua enum di bawah |
| `EmergencyPhysicalStatus` | **Baru** | `.../Enums/EmergencyPhysicalStatus.cs` | `Prepared`=1, `Departed`=2, `Arrived`=3, `Cancelled`=9 |
| `EmergencyHandoverStatus` | **Baru** | `.../Enums/EmergencyHandoverStatus.cs` | `Submitted`=1, `Pending`=2, `Accepted`=3, `Rejected`=4, `Cancelled`=9 |
| `EmergencyDepartureEventType` | **Baru** | `.../Enums/EmergencyDepartureEventType.cs` | `Prepared`=1, `Departed`=2, `Arrived`=3, `HandoverSubmitted`=4, `HandoverAccepted`=5, `HandoverRejected`=6, `Cancelled`=9, `Amended`=10, `Reversed`=11 |
| `EmergencyOrderKind` | **Baru** | `.../Enums/EmergencyOrderKind.cs` | `Medication`=1, `Procedure`=2, `LaboratoryOrder`=3, **`RadiologyOrder`=4** |
| `EmergencyOrderSource` | **Baru — revisi 6** | `.../Enums/EmergencyOrderSource.cs` | `Internal`=1, `External`=2 |
| `EmergencyOrderAction` | **Baru — nilainya berubah pada revisi 6** | `.../Enums/EmergencyOrderAction.cs` | **`Continue`=1, `Handover`=2, `Cancel`=9** |
| `EmergencyOrderAcceptanceStatus` | **Baru — revisi 6** | `.../Enums/EmergencyOrderAcceptanceStatus.cs` | `NotRequired`=1, `Pending`=2, `Accepted`=3, `Rejected`=4 |

Nilai `Cancelled` sengaja diberi angka `9` pada enum baru agar penambahan nilai antara tidak
menggeser nilai terminal.

`EmergencyOrderKind.LaboratoryOrder` **sudah didefinisikan tetapi belum dipakai** pada rilis
pertama, sesuai `IGD-DEC-087`. Ia ada supaya penambahannya kelak tidak menggeser nilai lain.

#### 3.4.1 Empat koreksi revisi 6

Revisi 5 menetapkan `EmergencyOrderAction` bernilai `Completed`/`Cancelled`/`HandedOver`.
`IGD-DEC-100` membuktikan nilai itu **salah**, bukan sekadar salah nama.

| Nilai revisi 5 | Revisi 6 | Sebab |
| --- | --- | --- |
| `Completed`=1 | **`Continue`=1** | Daftar sikap **hanya memuat pesanan yang belum selesai**. Pesanan yang sudah tuntas tidak pernah muncul di sana, sehingga `Completed` adalah nilai yang tidak pernah terpakai. Yang justru tidak punya nilai adalah keadaan sebenarnya: pesanan yang **masih berjalan** dan akan diproses sampai hasil final meski pasien sudah pergi — `IGD-DEC-100` butir (a) |
| `HandedOver`=3 | **`Handover`=2** | Arti sama. Kini menuntut **penerimaan eksplisit** per pesanan — `IGD-DEC-102` |
| `Cancelled`=2 | **`Cancel`=9** | Arti sama. Dipindah ke `9` mengikuti aturan modul ini: nilai terminal diberi `9` agar penambahan nilai antara tidak menggesernya |

Tiga koreksi lain:

| # | Koreksi | Keputusan |
| ---: | --- | --- |
| 2 | `EmergencyOrderKind` menambah `RadiologyOrder`=4 | `IGD-DEC-099` menetapkan pemesanan radiologi sebagai kebutuhan klinis IGD |
| 3 | `EmergencyOrderSource` memisahkan pesanan **internal** dari **luar sistem** | `IGD-DEC-103`. Selama modul radiologi belum ada, pesanannya dibuat di luar sistem dan tidak punya baris untuk ditunjuk |
| 4 | `EmergencyOrderAcceptanceStatus` — penerimaan **per pesanan** | `IGD-DEC-102`. Penerimaan pasien, dokumen serah terima, dan tiap pesanan adalah tiga fakta terpisah |

**`EmergencyOrderSource` sengaja dibuat terpisah, bukan ditumpangkan pada `EmergencyOrderKind`.**
Menambahkan nilai seperti `ExternalRadiologyOrder` akan menggandakan setiap jenis pesanan
begitu ada jenis kedua yang dipesan di luar sistem, dan membuat "jenis pemeriksaan" bercampur
dengan "asal pesanan" — dua hal yang berubah karena sebab berbeda.

`EmergencyOrderAcceptanceStatus.NotRequired` berlaku untuk `Continue` dan `Cancel`: keduanya
tidak melibatkan unit penerima, sehingga tidak ada yang perlu diterima.

### 3.5 Service

| Service | Status | Fungsi utama | Dipanggil | Membuka transaksi |
| --- | --- | --- | --- | :---: |
| `EmergencyVisitService` | **Diperbarui** | Validasi kunjungan, transisi status, nomor kunjungan. Ditambah: wajib `EncounterType.Emergency`, tolak episode ganda | `EmergencyVisitController` | Tidak |
| `EmergencyTriageService` | **Diperbarui** | Validasi triase, penilaian ulang, penanda pelampauan batas. Ditambah: transisi status kunjungan wajib lewat `CanTransition` | `EmergencyTriageController` | Ya, pada `RetriageAsync` |
| `EmergencyDispositionService` | **Diperbarui** | Validasi tindak lanjut, gerbang penutupan. Ditambah: membaca `ClosesEmergencyVisit`, memeriksa sikap pesanan | `EmergencyDispositionController`, `EmergencyVisitController` | Tidak |
| `EmergencyDepartureService` | **Diperbarui** | Menggantikan `EmergencyTransferService`. Mengelola dua rangkaian status, menulis kejadian, koreksi, dan pembalikan | `EmergencyDepartureController` | Ya |
| `EmergencyObservationService` | Sudah ada | Validasi observasi | `EmergencyObservationController` | Tidak |
| `EmergencyResuscitationService` | Sudah ada | Validasi resusitasi | `EmergencyResuscitationController` | Tidak |
| `EmergencyDocumentNumberService` | Sudah ada | Pembentukan nomor dokumen | Beberapa service | Tidak |
| `EmergencyUnitAuthorityService` | **Baru** | Menjawab "apakah pengguna ini berwenang atas unit pelayanan ini" dengan menelusuri profil pegawai dan penugasan organisasi yang sedang berlaku | Seluruh controller IGD yang menuntut kewenangan unit | Tidak |
| `EmergencyDoctorAssignmentService` | **Baru** | Menetapkan, mengalihkan, dan membaca dokter penanggung jawab yang sedang aktif | `EmergencyDoctorAssignmentController` | Ya |
| `EmergencyHandoverOrderService` | **Baru** | Menyusun daftar pesanan yang belum selesai dan menyimpan sikapnya | `EmergencyDepartureController` | Ya |
| `EmergencyReassessmentMonitorHostedService` | **Baru** | Menandai pengkajian ulang yang tertunggak, meniru `EmergencyTriageSlaMonitorHostedService` | Dijalankan latar belakang | Ya |
| `EmergencyTriageSlaMonitorHostedService` | Sudah ada | Menandai pelampauan batas waktu triase | Dijalankan latar belakang | Ya |

Seluruh service baru diletakkan di
`Areas/HealthServices/EmergencyInstallationManagement/Services/` dan didaftarkan
`AddScoped` di `Program.cs` berdampingan dengan delapan service IGD yang sudah terdaftar pada
baris 291–298.

### 3.6 Controller

| Controller | Status | Lokasi file | Service yang dipakai |
| --- | --- | --- | --- |
| `EmergencyVisitController` | **Diperbarui** | `.../EmergencyInstallationManagement/Controllers/EmergencyVisitController.cs` | `EmergencyVisitService`, `EmergencyDispositionService`, `EmergencyUnitAuthorityService` |
| `EmergencyTriageController` | **Diperbarui** | `.../Controllers/EmergencyTriageController.cs` | `EmergencyTriageService`, `EmergencyVisitService` |
| `EmergencyTriageDetailController` | Sudah ada | `.../Controllers/EmergencyTriageDetailController.cs` | — |
| `EmergencyObservationController` | Sudah ada | `.../Controllers/EmergencyObservationController.cs` | `EmergencyObservationService` |
| `EmergencyObservationDetailController` | Sudah ada | `.../Controllers/EmergencyObservationDetailController.cs` | — |
| `EmergencyResuscitationController` | Sudah ada | `.../Controllers/EmergencyResuscitationController.cs` | `EmergencyResuscitationService` |
| `EmergencyProcedureDetailController` | Sudah ada | `.../Controllers/EmergencyProcedureDetailController.cs` | — |
| `EmergencyDispositionController` | **Diperbarui** | `.../Controllers/EmergencyDispositionController.cs` | `EmergencyDispositionService` |
| `EmergencyDepartureController` | **Diperbarui** | `.../Controllers/EmergencyDepartureController.cs` | `EmergencyDepartureService`, `EmergencyHandoverOrderService`, `EmergencyUnitAuthorityService` |
| `EmergencyDoctorAssignmentController` | **Baru** | `.../Controllers/EmergencyDoctorAssignmentController.cs` | `EmergencyDoctorAssignmentService` |
| `EmergencyReassessmentWatchlistController` | **Baru** | `.../Controllers/EmergencyReassessmentWatchlistController.cs` | `EmergencyReassessmentMonitorHostedService` lewat service pembacanya |

### 3.7 EF Core Configuration

Configuration **tidak** berada di dalam `Areas/`. Seluruhnya di bawah
`Repositories/Configurations/HealthServices/EmergencyInstallationManagement/`.

| Configuration | Status | Relasi dan index yang diatur |
| --- | --- | --- |
| `TrxEmergencyDepartureConfiguration` | **Diperbarui** | Nama tabel berubah; empat index tempat tidur dan ruangan dihapus; index baru `(EmergencyVisitId, PhysicalStatus)` dan `(ToServiceUnitId, HandoverStatus)` |
| `TrxEmergencyDepartureEventConfiguration` | **Baru** | `HasOne(EmergencyDeparture)` `DeleteBehavior.Restrict`; index `(EmergencyDepartureId, OccurredAt)`; index `(EmergencyDepartureId, IsEffective)` |
| `TrxEmergencyHandoverOrderItemConfiguration` | **Baru** | `HasOne(EmergencyDeparture)` `DeleteBehavior.Restrict`; unique `(EmergencyDepartureId, OrderKind, OrderReferenceId)` |
| `EmgDoctorAssignmentConfiguration` | **Baru** | `HasOne(EmergencyVisit)` `DeleteBehavior.Restrict`; index `(EmergencyVisitId, EffectiveFrom)`; **unique filtered** `(EmergencyVisitId)` untuk baris dengan `EffectiveTo IS NULL` agar tidak pernah ada dua dokter aktif |
| Tujuh configuration IGD lain | Sudah ada | Tidak berubah |

`DeleteBehavior.Restrict` dipilih di seluruh relasi baru karena tidak satu pun catatan klinis
boleh ikut terhapus mengikuti induknya.

---

## 4. Arsitektur folder

```text
Areas/HealthServices/EmergencyInstallationManagement/
├── Controllers/
│   ├── EmergencyVisitController.cs                    Diperbarui
│   ├── EmergencyTriageController.cs                   Diperbarui
│   ├── EmergencyTriageDetailController.cs             Sudah ada
│   ├── EmergencyObservationController.cs              Sudah ada
│   ├── EmergencyObservationDetailController.cs        Sudah ada
│   ├── EmergencyResuscitationController.cs            Sudah ada
│   ├── EmergencyProcedureDetailController.cs          Sudah ada
│   ├── EmergencyDispositionController.cs              Diperbarui
│   ├── EmergencyDepartureController.cs                Diperbarui (dari EmergencyTransferController)
│   ├── EmergencyDoctorAssignmentController.cs         Baru
│   └── EmergencyReassessmentWatchlistController.cs    Baru
├── DTOs/
│   ├── EmergencyVisitDtos.cs                          Diperbarui
│   ├── EmergencyTriageDtos.cs                         Sudah ada
│   ├── EmergencyTriageDetailDtos.cs                   Sudah ada
│   ├── EmergencyObservationDtos.cs                    Sudah ada
│   ├── EmergencyObservationDetailDtos.cs              Sudah ada
│   ├── EmergencyResuscitationDtos.cs                  Sudah ada
│   ├── EmergencyProcedureDetailDtos.cs                Sudah ada
│   ├── EmergencyDispositionDtos.cs                    Sudah ada
│   ├── EmergencyDepartureDtos.cs                      Diperbarui (dari EmergencyTransferDtos)
│   ├── EmergencyDoctorAssignmentDtos.cs               Baru
│   └── EmergencyReassessmentWatchlistDtos.cs          Baru
├── Enums/
│   ├── EmergencyVisitStatus.cs                        Sudah ada
│   ├── EmergencyRegistrationStatus.cs                 Sudah ada
│   ├── EmergencyTriageStatus.cs                       Sudah ada
│   ├── EmergencyTriageSystem.cs                       Sudah ada
│   ├── EmergencyObservationStatus.cs                  Sudah ada
│   ├── EmergencyResuscitationStatus.cs                Sudah ada
│   ├── EmergencyProcedureDetailType.cs                Sudah ada
│   ├── EmergencyDispositionStatus.cs                  Sudah ada
│   ├── EmergencyTransferStatus.cs                     Dihapus setelah migrasi data selesai
│   ├── EmergencyPhysicalStatus.cs                     Baru
│   ├── EmergencyHandoverStatus.cs                     Baru
│   ├── EmergencyDepartureEventType.cs                 Baru
│   ├── EmergencyOrderKind.cs                          Baru
│   └── EmergencyOrderAction.cs                        Baru
├── Models/
│   ├── TrxEmergencyVisit.cs                           Sudah ada
│   ├── TrxEmergencyTriage.cs                          Sudah ada
│   ├── TrxEmergencyTriageDetail.cs                    Sudah ada
│   ├── TrxEmergencyResuscitation.cs                   Sudah ada
│   ├── TrxEmergencyObservation.cs                     Sudah ada
│   ├── TrxEmergencyObservationDetail.cs               Sudah ada
│   ├── TrxEmergencyProcedureDetail.cs                 Sudah ada
│   ├── TrxEmergencyDisposition.cs                     Sudah ada
│   ├── TrxEmergencyDeparture.cs                       Diperbarui (dari TrxEmergencyTransfer)
│   ├── TrxEmergencyDepartureEvent.cs                  Baru
│   ├── TrxEmergencyHandoverOrderItem.cs               Baru
│   └── EmgDoctorAssignment.cs                         Baru
└── Services/
    ├── EmergencyDocumentNumberService.cs              Sudah ada
    ├── EmergencyVisitService.cs                       Diperbarui
    ├── EmergencyTriageService.cs                      Diperbarui
    ├── EmergencyResuscitationService.cs               Sudah ada
    ├── EmergencyObservationService.cs                 Sudah ada
    ├── EmergencyDispositionService.cs                 Diperbarui
    ├── EmergencyDepartureService.cs                   Diperbarui (dari EmergencyTransferService)
    ├── EmergencyUnitAuthorityService.cs               Baru
    ├── EmergencyDoctorAssignmentService.cs            Baru
    ├── EmergencyHandoverOrderService.cs               Baru
    ├── EmergencyTriageSlaMonitorHostedService.cs      Sudah ada
    ├── EmergencyTriageSlaMonitorOptions.cs            Sudah ada
    ├── EmergencyReassessmentMonitorHostedService.cs   Baru
    └── EmergencyReassessmentMonitorOptions.cs         Baru

Repositories/Configurations/HealthServices/EmergencyInstallationManagement/
├── TrxEmergencyVisitConfiguration.cs                  Sudah ada
├── TrxEmergencyTriageConfiguration.cs                 Sudah ada
├── TrxEmergencyTriageDetailConfiguration.cs           Sudah ada
├── TrxEmergencyResuscitationConfiguration.cs          Sudah ada
├── TrxEmergencyObservationConfiguration.cs            Sudah ada
├── TrxEmergencyObservationDetailConfiguration.cs      Sudah ada
├── TrxEmergencyProcedureDetailConfiguration.cs        Sudah ada
├── TrxEmergencyDispositionConfiguration.cs            Sudah ada
├── TrxEmergencyDepartureConfiguration.cs              Diperbarui
├── TrxEmergencyDepartureEventConfiguration.cs         Baru
├── TrxEmergencyHandoverOrderItemConfiguration.cs      Baru
└── EmgDoctorAssignmentConfiguration.cs                Baru
```

### 4.1 Utang teknis

| Utang | Keadaan pada revision 5 |
| --- | --- |
| Folder controller IGD pernah bernama `Controller/` tunggal | **Sudah diperbaiki** oleh `BE-IGD-013`. Sekarang `Controllers/` jamak |
| Nama domain pada `Repositories/Configurations/` pernah `HealthService/` tunggal | **Sudah diperbaiki**. Sekarang `HealthServices/` jamak |
| `LabOrder` tidak memakai awalan `Trx` | **Dibiarkan.** Milik Laboratory Management; merapikannya berarti menyentuh modul orang lain tanpa alasan fungsional |
| `LabOrderConfiguration` berada langsung di `Repositories/Configurations/HealthServices/`, tanpa folder submodul | **Dibiarkan.** Alasan sama |

Utang yang dibiarkan **jangan ditiru** untuk berkas baru.

---

## 5. Status model

| Model atau berkas | Status | Kolom yang berubah | Dampak migration |
| --- | --- | --- | --- |
| `TrxEmergencyVisit` | Sudah ada | Tidak ada | Tidak ada |
| `TrxEmergencyTriage` | Sudah ada | Tidak ada | Tidak ada |
| `TrxEmergencyDeparture` | **Diperbarui** | **Dihapus:** `FromRoomId`, `ToRoomId`, `FromBedId`, `ToBedId`, `TransferStatus`, `AcceptedAt`, `AcceptedByUserId`, `RejectionReason`. **Ditambah:** `PhysicalStatus` (`EmergencyPhysicalStatus`, bawaan `Prepared`), `HandoverStatus` (`EmergencyHandoverStatus`, bawaan `Submitted`), `SituationSummary` (`string?`, 2000), `BackgroundSummary` (`string?`, 2000), `AssessmentSummary` (`string?`, 2000), `RecommendationSummary` (`string?`, 2000), `AllergySnapshot` (`string?`, 1000), `LastVitalSignId` (`Guid?`), `TriageLevelSnapshot` (`string?`, 150). **Diganti nama:** tabel dan kelas dari `TrxEmergencyTransfer` | Ganti nama tabel, hapus delapan kolom, tambah sembilan kolom, ganti dua index. **Tidak dapat dijalankan tanpa memeriksa data lama** — lihat bagian 6 |
| `TrxEmergencyDepartureEvent` | **Baru** | Seluruh kolom baru | Tabel baru |
| `TrxEmergencyHandoverOrderItem` | **Baru** | Seluruh kolom baru | Tabel baru |
| `EmgDoctorAssignment` | **Baru** | Seluruh kolom baru | Tabel baru beserta unique index bersyarat |
| `MstEmergencySetting` | **Diperbarui** | **Dihapus:** `AutoCreateProvisionalEncounter`, `RequireTriageBeforeStandardRegistration`. **Dipertahankan dan mulai dibaca:** `ImmediateCareLevelThreshold`, `RequireRegistrationBeforeTreatmentFromLevel` | Hapus dua kolom. Lihat catatan di bawah |
| `MstServiceUnit` | **Diperbarui** | Tambah `OrganizationUnitId` (`Guid?`) beserta index | Satu kolom, boleh kosong. **Milik Master Data** |
| `TrxPatientEncounter` | **Diperbarui** | Tambah `OriginEncounterId` (`Guid?`) beserta index | Satu kolom, boleh kosong. **Milik Registration Management** |
| `TrxPatientAssessment` | **Diperbarui** | `QueueId` `Guid` → `Guid?`; tambah `IsEffective` (`bool`, bawaan `true`), `AmendsAssessmentId` (`Guid?`), `AmendmentReason` (`string?`, 500) | **Milik Clinical Management** |
| `TrxDoctorConsultation` | **Diperbarui** | `QueueId` `Guid` → `Guid?` | **Milik Clinical Management** |
| `TrxPatientDiagnosis`, `TrxPatientProcedure` | **Diperbarui** | `ConsultationId` `Guid` → `Guid?` | **Milik Clinical Management** |
| `TrxPatientVitalSign`, `TrxPatientIntegratedProgressNote` | **Diperbarui** | Tambah `IsEffective`, `Amends…Id`, `AmendmentReason` | **Milik Clinical Management** |
| `TrxPrescription` | **Diperbarui** | `ConsultationId` `Guid` → `Guid?` | **Milik Pharmacy Management** |
| `EmergencyTransferStatus` | **Dihapus** | Seluruh enum | Setelah data lama dipetakan ke dua enum baru |

### 5.1 Dua kolom pengaturan yang dicabut

`IGD-GAP-031` mencatat empat kolom `MstEmergencySetting` yang tersimpan tetapi tidak
menjalankan apa pun. Revision `5` menyelesaikannya begini:

| Kolom | Perlakuan | Alasan |
| --- | --- | --- |
| `ImmediateCareLevelThreshold` | **Dipertahankan dan mulai dibaca** | Menjadi dasar penentuan `ImmediateCareAllowed` bersama `AllowsTreatmentBeforeRegistration` pada master level |
| `RequireRegistrationBeforeTreatmentFromLevel` | **Dipertahankan dan mulai dibaca** | Menegakkan `IGD-DEC-002`: pasien gawat boleh ditangani sebelum administrasi selesai |
| `AutoCreateProvisionalEncounter` | **Dicabut** | Pembuatan kunjungan selalu dilakukan layar pendaftaran secara eksplisit. Kolom ini tidak pernah mengubah perilaku apa pun dan mempertahankannya hanya mengundang salah paham |
| `RequireTriageBeforeStandardRegistration` | **Dicabut** | Bertentangan dengan `IGD-DEC-002`. Urutan triase dan pendaftaran ditentukan kegawatan pasien, bukan oleh sakelar pengaturan |

Pencabutan dua kolom ini **belum diputuskan owner**. Dicatat sebagai `IGD-OQ-069`.

---

## 6. Rencana migration

Seluruh migration di bawah **belum boleh dijalankan**. Basis data pengembangan dipakai
bersama satu tim dan berisi data pasien.

| Urutan | Migration | Tanpa mematikan layanan | Pengisian data lama | Cara mundur |
| ---: | --- | :---: | --- | --- |
| 1 | `AddOriginEncounterToPatientEncounter` | Ya | Tidak ada. Kolom boleh kosong, seluruh baris lama tetap sah | Hapus kolom dan index. Aman selama belum terisi |
| 2 | `AddOrganizationUnitToServiceUnit` | Ya | **Wajib diisi manual** oleh Master Data bersama Corporate/HR sebelum penjagaan kewenangan dinyalakan. Lihat `IGD-UNK-07` | Hapus kolom dan index |
| 3 | `AddEmergencyPatientClassSeed` | Ya | Menambah satu baris `MstPatientClass` bertanda `IsForEmergency` dan `IsDefault`. Tidak mengubah baris yang sudah ada | Hapus baris yang ditambahkan, dikenali dari kodenya |
| 4 | `ChangeEmergencyEncounterTypeToEmergency` | **Tidak** | `UPDATE TrxPatientEncounter SET EncounterType = 2 WHERE Id IN (SELECT EncounterId FROM TrxEmergencyVisit WHERE EncounterId IS NOT NULL)`. Jumlah baris yang berubah **wajib** sama dengan jumlah baris `TrxEmergencyVisit` yang `EncounterId`-nya terisi | `UPDATE ... SET EncounterType = 1` untuk himpunan yang sama. **Aman** karena nilai `2` tidak pernah dipakai sebelumnya |
| 5 | `AddEmergencyDoctorAssignment` | Ya | Untuk setiap kunjungan IGD yang `TrxPatientEncounter.DoctorId`-nya terisi, dibuat satu baris riwayat dengan `EffectiveFrom` diambil dari `UpdateDateTime` encounter dan `EffectiveTo` kosong. Bila `UpdateDateTime` kosong, dipakai `CreateDateTime` | Hapus tabel |
| 6 | `RenameEmergencyTransferToDeparture` | **Tidak** | Ganti nama tabel dan pemetaan `TransferStatus` lama ke dua kolom baru — lihat tabel pemetaan di bawah | Ganti nama kembali dan pulihkan kolom. **Kolom tempat tidur tidak dapat dipulihkan** bila datanya sudah dibuang |
| 7 | `AddEmergencyDepartureEventAndOrderItem` | Ya | Untuk setiap baris kepergian yang sudah ada, dibuat kejadian awal yang mencerminkan status hasil pemetaan langkah 6 | Hapus dua tabel |
| 8 | `RelaxQueueAndConsultationForEmergency` | Ya | Tidak ada. Melepas kewajiban terisi tidak mengubah nilai apa pun | **Mengembalikan kewajiban terisi akan gagal** bila sudah ada baris IGD yang kolomnya kosong. Lihat peringatan |
| 9 | `AddClinicalRecordVersionMarkers` | Ya | `IsEffective` diisi `true` untuk seluruh baris lama | Hapus tiga kolom pada tiga tabel |
| 10 | `DropUnusedEmergencySettingColumns` | Ya | Tidak ada | Tambah kembali dua kolom dengan nilai bawaannya |

### 6.1 Pemetaan status lama ke dua rangkaian baru

Langkah 6 memetakan enam nilai `EmergencyTransferStatus` menjadi dua kolom:

| `TransferStatus` lama | `PhysicalStatus` baru | `HandoverStatus` baru | Catatan |
| --- | --- | --- | --- |
| `Requested` = 1 | `Prepared` | `Submitted` | Belum berangkat, dokumen sudah diajukan |
| `Accepted` = 2 | `Prepared` | `Accepted` | **Ambigu di data lama.** Lihat peringatan |
| `InTransit` = 3 | `Departed` | `Submitted` | |
| `Completed` = 4 | `Arrived` | `Accepted` | |
| `Rejected` = 5 | `Prepared` | `Rejected` | |
| `Cancelled` = 6 | `Cancelled` | `Cancelled` | |

> **Peringatan pemetaan `Accepted`.** Nilai lama `Accepted` tidak dapat dibedakan artinya:
> ia bisa berarti "unit tujuan setuju menerima" atau "pasien sudah diterima secara fisik".
> Pemetaan di atas memilih arti pertama, karena `ArrivedAt` pada baris lama **selalu kosong** —
> tidak ada satu pun endpoint yang pernah mengisinya. Pilihan ini **wajib diperiksa terhadap
> data nyata** sebelum dijalankan, dan hasilnya dicatat sebagai bukti.

### 6.2 Peringatan cara mundur

> **Langkah 4.** Membatalkannya mengembalikan seluruh kunjungan IGD menjadi `Outpatient`, dan
> laporan rawat jalan kembali memuat pasien IGD. Ini memulihkan keadaan sebelumnya dengan
> setia, tetapi angka laporan akan berubah dua kali.

> **Langkah 6.** Empat kolom tempat tidur dan ruangan dihapus. Bila sudah ada baris yang
> mengisinya, nilainya **hilang permanen** kecuali diarsipkan lebih dulu. Jumlah baris
> terdampak belum diketahui — `IGD-UNK-03`. Langkah 6 **tidak boleh dijalankan** sebelum angka
> itu diketahui dan keputusan pengarsipannya diambil.

> **Langkah 8.** Mengembalikan kewajiban `QueueId` dan `ConsultationId` akan **gagal** begitu
> ada satu saja pengkajian atau resep IGD yang tersimpan tanpa keduanya. Setelah modul IGD
> dipakai, langkah ini praktis tidak dapat dibatalkan.

### 6.3 Urutan yang tidak boleh ditukar

```text
1 ─┐
2 ─┼─► boleh paralel, tidak saling bergantung
3 ─┘
       │
       ▼
4  ChangeEmergencyEncounterTypeToEmergency
       │  ← langkah 3 WAJIB selesai lebih dulu,
       │    kalau tidak pendaftaran IGD berhenti total
       ▼
8  RelaxQueueAndConsultationForEmergency
       │  ← menyaring berdasarkan EncounterType,
       │    jadi wajib sesudah langkah 4
       ▼
9  AddClinicalRecordVersionMarkers

5, 6, 7  ─► bebas urutannya terhadap jalur di atas
10       ─► paling akhir, setelah dipastikan tidak ada yang membaca dua kolom itu
```

Langkah 3 sebelum langkah 4 adalah keharusan mutlak. Menukarnya membuat kunjungan IGD berubah
tipe sementara master kelas pasiennya belum ada, sehingga `PatientClassId` kosong dan konteks
tarif hilang tanpa satu pun pesan galat.

---

## 7. Rencana data master awal

Tanpa data ini modul tidak dapat dipakai sama sekali.

| No | Master | Isi minimum | Keadaan |
| ---: | --- | --- | --- |
| 1 | `MstEmergencyTriageLevel` | Lima level beserta warna; `MaxWaitingMinutes` **dibiarkan kosong** untuk level yang SOP-nya belum disahkan | Seeder tersedia |
| 2 | `MstEmergencyTriageIndicator` | Indikator per level | Seeder tersedia |
| 3 | `MstEmergencyArrivalMode` | Cara kedatangan: datang sendiri, ambulans, rujukan, polisi | Seeder tersedia |
| 4 | `MstEmergencyCaseType` | Jenis kasus: trauma, non-trauma, kebidanan, anak | Seeder tersedia |
| 5 | `MstEmergencyDispositionType` | Tujuh jenis; `ClosesEmergencyVisit` kini **menentukan perilaku**, jadi nilainya wajib ditinjau per jenis | Seeder tersedia, **nilai perlu ditinjau ulang** |
| 6 | `MstEmergencySetting` | Satu baris bawaan menunjuk unit IGD | Seeder tersedia |
| 7 | `MstPatientClass` bertanda `IsForEmergency` + `IsDefault` | **Tepat satu baris** | **Belum ada seeder** — `IGD-DEC-076` |
| 8 | `MstServiceUnit.OrganizationUnitId` untuk unit IGD dan unit tujuan | Pemetaan ke simpul organisasi | **Belum ada** — `IGD-DEC-086` |

### 7.1 Peninjauan `ClosesEmergencyVisit`

Seeder mengisi ketujuh jenis dengan `true`. Setelah `IGD-DEC-067` menjadikannya penentu
perilaku, nilai itu perlu ditinjau:

| Kode | Nama | Usulan nilai | Alasan |
| --- | --- | :---: | --- |
| `PULANG` | Pulang | `true` | Pasien meninggalkan rumah sakit |
| `RANAP` | Rawat inap | `true` | Menutup kunjungan IGD dan membuka kunjungan rawat inap — `RWI-RULE-029` |
| `INTENSIF` | Pindah ICU atau kamar operasi | `true` | Sama seperti rawat inap untuk tujuan ICU; kamar operasi perlu ditinjau klinis |
| `RUJUK` | Rujuk ke fasilitas lain | `true` | Pasien meninggalkan rumah sakit |
| `MENINGGAL` | Meninggal | `true` | |
| `TOLAK` | Menolak perawatan | `true` | |
| `APS` | Pulang atas permintaan sendiri | `true` | |

Ketujuhnya bernilai `true`, sehingga **tidak ada perubahan data** yang diperlukan. Yang
berubah hanyalah bahwa nilainya sekarang benar-benar dibaca. Peninjauan ini tetap perlu
persetujuan Clinical Governance karena menyangkut kapan pelayanan IGD dianggap berakhir.

---

## 8. Kebutuhan lintas modul

| No | Kebutuhan | Modul pemilik | Menahan rilis IGD | Keputusan |
| ---: | --- | --- | :---: | --- |
| 1 | Pelonggaran kewajiban `QueueId` dan `ConsultationId` untuk kunjungan `Emergency` | Clinical Management, Pharmacy Management | **Ya** | `IGD-DEC-068` |
| 2 | Penanda versi catatan klinis untuk koreksi tambah-saja | Clinical Management | **Ya** untuk pengkajian; tidak untuk yang lain | `IGD-DEC-080` |
| 3 | Kolom `OriginEncounterId` | Registration Management | Tidak | `IGD-DEC-075` |
| 4 | Kolom `OrganizationUnitId` | Master Data | Tidak — penjagaan kewenangan dapat menyusul | `IGD-DEC-086` |
| 5 | `InpBedPlacement` membaca waktu tiba dari catatan kepergian IGD | Inpatient Management | Tidak | `IGD-DEC-071` |
| 6 | Pelengkapan `LabOrder` | Laboratory Management | Tidak | `IGD-DEC-087`, `IGD-DEC-088` |
| 7 | Revisi `RWI-RULE-026` aturan 6 dan `compatibility_impact` manifest | Inpatient Management | **Ya** lewat nomor 1 | `IGD-DEC-068`, `IGD-DEC-075` |

Nomor 1 dan 2 adalah **satu-satunya** yang menahan rilis pertama. Keduanya menunggu pemilik
modul yang belum ditunjuk.

---

## 9. Yang sengaja tidak dibuat

| Yang dipertimbangkan | Ditolak karena |
| --- | --- |
| Tabel pengkajian keperawatan khusus IGD | `IGD-DEC-003` dan `IGD-DEC-068` melarang tabel klinis tandingan. Rekam medis pasien harus satu tempat |
| Tabel antrean semu untuk pasien IGD | `IGD-DEC-068` dan `RWI-RULE-026` aturan 2 melarangnya. Laporan antrean poliklinik tidak boleh tercemar |
| Tabel penugasan pengguna ke unit pelayanan | `IGD-DEC-086` mencabutnya. Rantai pengguna ke organisasi sudah ada; yang kurang hanya satu jembatan |
| Tabel pemesanan laboratorium milik IGD | `IGD-DEC-087`. `LabOrder` sudah ada dan sudah dapat dipakai |
| Tabel alokasi tempat tidur milik IGD | `IGD-DEC-069`. Milik Rawat Inap lewat `InpBedPlacement` |
| Dokumen serah terima terpisah untuk perawat dan dokter | `IGD-DEC-079` memilih satu dokumen untuk rilis pertama |
| Salinan pasien, dokter, atau master apa pun ke dalam IGD | Aturan kepemilikan data bagian 1 |
| Pemesanan radiologi | Modulnya belum ada; membuatnya di IGD berarti mendirikan modul penunjang kedua |
| Catatan pemberian obat milik IGD | Menyentuh Pharmacy Management. Ditunggu sampai pemiliknya ditunjuk |
| Perubahan pada mesin hak akses `SysAccessPolicy` | `IGD-DEC-081` dan `IGD-DEC-086` menegaskan penjaga ditulis di service IGD, bukan di mesin yang menjaga seluruh aplikasi tanpa satu pun test |

---

## 10. Pertanyaan terbuka yang lahir dari desain ini

| ID | Pertanyaan | Memblokir |
| --- | --- | --- |
| `IGD-OQ-068` | `IGD-DEC-070` memilih dua kolom status dan menolak daftar kejadian, tetapi `IGD-DEC-065`, `066`, `085` menuntut penyimpanan yang hanya mungkin sebagai daftar kejadian. Apakah penafsiran bagian 2.4 — kolom status sebagai turunan, daftar kejadian sebagai sumber audit — dapat diterima? | `IMPLEMENTATION` catatan kepergian |
| `IGD-OQ-069` | Apakah `AutoCreateProvisionalEncounter` dan `RequireTriageBeforeStandardRegistration` benar dicabut, atau justru harus diberi arti? | `IMPLEMENTATION` `MstEmergencySetting` |
| `IGD-OQ-070` | Penggantian nama `TrxEmergencyTransfer` menjadi `TrxEmergencyDeparture` mengubah nama tabel dan seluruh route-nya. Apakah penggantian nama diterima, atau nama lama dipertahankan demi kompatibilitas pemakai luar? | `IMPLEMENTATION` |

---

## 11. Correction pass revisi 6 — 26 Agustus 2026

Ditambahkan atas permintaan Product/Domain Owner. **Bukan** perancangan area baru.

### 11.1 Cara baris pesanan internal dibentuk

Revisi 6 memperkenalkan `EmergencyOrderSource`, tetapi **belum menetapkan** dari mana baris
`Internal` datang. Tanpa itu, `TrxEmergencyHandoverOrderItem` hanya dapat diisi manual, dan
`IGD-DEC-100` butir (d) — larangan pembatalan otomatis — kehilangan artinya karena tidak ada
daftar yang dibentuk sistem.

Sumber per jenis, seluruhnya diverifikasi pada `300922c`:

| `OrderKind` | Tabel sumber | `OrderReferenceId` menunjuk | Penanda "belum selesai" | Pemilik tabel |
| --- | --- | --- | --- | --- |
| `Medication` | `TrxPrescription` | `TrxPrescription.Id` | `FulfillmentStatus` belum terminal | `PharmacyManagement` |
| `Procedure` | `TrxPatientProcedure` | `TrxPatientProcedure.Id` | `ProcedureStatus` belum terminal; nilai awalnya `Planned` | `ClinicalManagement` |
| `LaboratoryOrder` | `LabOrder` | `LabOrder.Id` | **Tidak dapat ditentukan sistem** | `LaboratoryManagement` |
| `RadiologyOrder` | — | **Selalu kosong** | Tidak berlaku | Belum ada modulnya |

#### Tiga akibat yang harus diterima secara sadar

**① Dua dari empat jenis bergantung pada tabel milik modul lain yang pemiliknya belum
ditunjuk.** `Medication` dan `Procedure` hanya dapat dibentuk otomatis setelah `MVP-3` membuka
jalur klinis IGD. Sebelum itu keduanya **kosong**, bukan salah.

**② `LaboratoryOrder` tidak akan pernah terbentuk otomatis dengan benar** selama `LabOrder`
tidak punya kolom status. Baris tetap dapat dibentuk — pesanan lab memang ada dan menempel
pada encounter — tetapi **sikapnya** wajib ditetapkan manual, sesuai `IGD-DEC-101`. Sistem
membentuk barisnya, klinisi menentukan sikapnya.

**③ `RadiologyOrder` selalu `External`.** Selama modul radiologi belum ada, tidak ada baris
untuk ditunjuk, sehingga `OrderReferenceId` selalu kosong dan `ExternalReference` selalu wajib
— `IGD-DEC-099`, `IGD-DEC-103`.

#### Kapan daftar dibentuk

Daftar disusun **saat dokumen serah terima diajukan**, bukan saat pesanan dibuat. Alasannya:
pesanan yang selesai sebelum pasien pergi tidak pernah perlu diberi sikap, dan membentuk
barisnya lebih awal hanya menghasilkan baris yang langsung usang.

`EmergencyHandoverOrderService` menyusunnya dengan menanyakan tiap modul sumber, lalu
menyimpan **snapshot** uraian pesanan pada `OrderDescription`. Snapshot dipakai supaya daftar
tetap terbaca meski baris sumbernya kelak berubah — pola yang sama dengan
`OrderLabelSnapshot` yang sudah ada.

### 11.2 Unique constraint yang mendukung tiga keadaan sekaligus

Rancangan revisi 6 menulis *"tepat satu baris `IsEffective = true` per pesanan yang sama"*.
Rumusan itu **tidak dapat ditegakkan** apa adanya: pesanan `External` tidak punya
`OrderReferenceId` untuk dijadikan kunci, sehingga seluruh pesanan luar sistem akan dianggap
"pesanan yang sama" dan saling menolak.

Diperbaiki menjadi **dua index parsial**, masing-masing dengan kuncinya sendiri:

| Index | Kunci | Syarat |
| --- | --- | --- |
| `UX_EmergencyHandoverOrderItem_Internal` | `EmergencyDepartureId`, `OrderKind`, `OrderReferenceId` | `IsEffective` **dan** `OrderSource = Internal` **dan** `NOT IsDelete` |
| `UX_EmergencyHandoverOrderItem_External` | `EmergencyDepartureId`, `OrderKind`, `ExternalReference` | `IsEffective` **dan** `OrderSource = External` **dan** `NOT IsDelete` |

Ditambah satu `CHECK` yang menjaga keduanya tidak pernah kosong bersamaan:

```
(OrderSource = Internal AND OrderReferenceId IS NOT NULL AND ExternalReference IS NULL)
OR
(OrderSource = External AND ExternalReference IS NOT NULL AND OrderReferenceId IS NULL)
```

#### Kenapa koreksi tambah-saja tidak bertabrakan dengan index ini

Baris yang digantikan ditandai `IsEffective = false` **dalam transaksi yang sama** dengan
penulisan baris penggantinya. Karena kedua index hanya berlaku pada baris `IsEffective = true`,
riwayat sepanjang apa pun tidak pernah melanggarnya — dan urutan penulisannya tidak perlu
diatur khusus.

Ini pola yang sama dengan `TrxEmergencyDepartureEvent` (`IGD-DEC-090`) dan dengan unique index
bersyarat pada `EmgDoctorAssignment`. Konsisten, bukan mekanisme baru.

> **Catatan PostgreSQL.** `NULL` tidak dianggap sama dengan `NULL` pada unique index. Tanpa
> syarat `OrderSource` pada tiap index, baris `External` yang `OrderReferenceId`-nya sama-sama
> kosong akan lolos begitu saja — bukan karena aturannya benar, melainkan karena
> perbandingannya tidak pernah terjadi. Syarat `OrderSource` itulah yang membuat index kedua
> benar-benar menjaga.

---

## 12. Pemantauan observasi bertanda vital — 16 September 2026

Bagian ini menurunkan `IGD-DEC-122` sampai `IGD-DEC-126` menjadi desain backend. **Tidak ada
tabel baru, tidak ada kolom baru, dan tidak ada migration.** Yang berubah adalah aturan
penerimaan dan bentuk pembacaan.

### 12.1 Kepemilikan data yang ditegaskan ulang

| Kelompok data | Modul pemilik | Dipakai IGD? | Dibuat ulang di IGD? |
| --- | --- | :-: | :-: |
| Angka tanda vital, GCS, kesadaran, oksigen, nyeri, EWS | `ClinicalManagement` — `TrxPatientVitalSign` | Ya, lewat tautan | **Tidak** |
| Catatan perkembangan | `ClinicalManagement` — `TrxPatientIntegratedProgressNote` | Ya, lewat tautan | **Tidak** |
| Periode observasi dan isi pemantauan | `EmergencyInstallationManagement` — `EmgObservation`, `EmgObservationDetail` | Ya | — |
| Primary survey ABCDE | `EmergencyInstallationManagement` — `EmgTriage` (ringkasan teks) | Dibaca saja | **Tidak** |
| RJP, ROSC, defibrilasi | `EmergencyInstallationManagement` — `EmgResuscitation` | Tidak pada layar ini | **Tidak** |
| Obat dan tindakan | Farmasi dan `EmgProcedureDetail` | Tidak pada layar ini | **Tidak** |

### 12.2 Relasi yang dipakai validasi

```mermaid
classDiagram
    class EmgVisit {
        +Guid Id
        +Guid? EncounterId
        +Guid? PatientId
    }
    class EmgObservation {
        +Guid Id
        +Guid EmergencyVisitId
        +EmergencyObservationStatus ObservationStatus
    }
    class EmgObservationDetail {
        +Guid Id
        +Guid EmergencyObservationId
        +Guid? PatientVitalSignId «tautan»
        +Guid? ProgressNoteId «tautan»
        +Guid RecordedByUserId «dari token»
    }
    class TrxPatientVitalSign {
        +Guid Id
        +Guid PatientId
        +Guid? EncounterId
        +PatientVitalSignStatus VitalSignStatus
        +bool IsActive
    }

    EmgVisit "1" --> "0..*" EmgObservation
    EmgObservation "1" --> "0..*" EmgObservationDetail
    EmgObservationDetail "0..*" --> "0..1" TrxPatientVitalSign : PatientVitalSignId
```

Jalur yang dilalui pemeriksaan lingkup:
`EmgObservationDetail → EmgObservation → EmgVisit → (PatientId, EncounterId)`, lalu dibandingkan
dengan `TrxPatientVitalSign.PatientId` dan `TrxPatientVitalSign.EncounterId`.

### 12.3 Kelas yang berubah

| Class | Status | Lokasi file | Perubahan |
| --- | --- | --- | --- |
| `EmergencyObservationService` | **Diperbarui** | `Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyObservationService.cs` | Menerima pemeriksaan lingkup pemantauan: periode belum ditutup, tanda vital dan catatan perkembangan satu pasien dan satu encounter, serta tanda vital masih berlaku. Service ini **sudah terdaftar** di `Program.cs`; tidak ada pendaftaran baru |
| `EmergencyObservationDetailController` | **Diperbarui** | `.../Controllers/EmergencyObservationDetailController.cs` | Memanggil pemeriksaan di atas sebelum menyimpan; pelaku pencatat selalu dari token; proyeksi tanda vital dan nama pelaku pada response |
| `EmergencyObservationDetailDtos` | **Diperbarui** | `.../DTOs/EmergencyObservationDetailDtos.cs` | `EmergencyObservationDetailResponse` bertambah `RecordedByName` dan objek `VitalSign`; `CreateEmergencyObservationDetailRequest.RecordedByUserId` ditandai usang dan diabaikan |
| `EmgObservationDetail` | **Sudah ada** | `.../Models/EmgObservationDetail.cs` | **Tidak berubah** |
| `EmgObservationDetailConfiguration` | **Sudah ada** | `Repositories/Configurations/HealthServices/EmergencyInstallationManagement/EmgObservationDetailConfiguration.cs` | **Tidak berubah**; FK, index `PatientVitalSignId`, dan `DeleteBehavior.Restrict` sudah sesuai |

Pemeriksaan ditaruh di service, bukan di controller, mengikuti `QBE-SVC-001` untuk kode yang
disentuh. Pemakaian `ApplicationDbContext` langsung di controller adalah utang lama yang
**tidak** diperluas dan **tidak** dirapikan pada task ini.

### 12.4 Bentuk pembacaan yang menghindari N+1

Proyeksi tanda vital diambil dalam **satu kueri** bersama daftar pemantauan, memakai
`Include`/`Select` pada relasi `PatientVitalSign` yang sudah dikonfigurasi. Nama pelaku diambil
dari relasi `RecordedByUser` yang juga sudah ada.

Yang **dilarang**: memanggil endpoint tanda vital satu kali per baris dari frontend, dan
mengambil seluruh direktori pengguna hanya untuk menerjemahkan satu GUID.

### 12.5 Dampak migration dan data lama

| Hal | Keadaan |
| --- | --- |
| Migration | **Tidak ada** |
| Kolom baru | **Tidak ada** |
| Data lama | Baris pemantauan lama yang `PatientVitalSignId`-nya kosong tetap terbaca; `vitalSign` dikirim `null`. **Tidak ada** pengisian mundur |
| Data master awal | **Tidak ada** yang perlu diisi |
| Rollback | Cukup mengembalikan source; tidak ada perubahan skema yang perlu dibalik |

### 12.6 Yang sengaja tidak dibuat

| Yang ditolak | Alasan |
| --- | --- |
| Tabel tanda vital IGD | `IGD-DEC-122` — angka tetap milik `ClinicalManagement` |
| Kolom GCS, kesadaran, oksigen pada `EmgObservationDetail` | Sama; sudah ada di `TrxPatientVitalSign` |
| Kolom ABCDE pada `EmgObservationDetail` | `IGD-DEC-123`; `IGD-GAP-027` masih ditunda |
| Kolom alat bantu jalan napas | `IGD-DEC-124`; menunggu `IGD-OQ-089` |
| Kolom obat, gambaran EKG, DC Shock | Milik Farmasi, Tindakan, dan Resusitasi |
| Endpoint baru untuk memilih tanda vital | `GET patient-vital-signs?patientId=&encounterId=` milik `ClinicalManagement` sudah cukup |
| Jalur entri susulan setelah periode ditutup | `IGD-OQ-090`, belum dirancang; **dilarang** menumpang `BE-IGD-046` |
| Penambahan nilai enum `OxygenSupportType` | `IGD-DEC-125` — milik `ClinicalManagement` |

### 12.7 Urutan pemanggilan

```mermaid
sequenceDiagram
    actor Perawat
    participant Layar as Layar Pengkajian IGD
    participant VS as Clinical Management API
    participant OBS as Emergency Observation Detail API
    participant DB as Basis data

    Perawat->>Layar: Catat Pemantauan
    alt Tanda vital baru
        Layar->>VS: POST patient-vital-signs (pasien + encounter kunjungan ini)
        VS-->>Layar: id tanda vital
    else Pilih tanda vital yang sudah ada
        Layar->>VS: GET patient-vital-signs?patientId=&encounterId=
        VS-->>Layar: daftar tanda vital kunjungan ini
    end
    Layar->>OBS: POST emergency-observation-details (+ patientVitalSignId)
    OBS->>DB: periode masih terbuka? pasien & encounter cocok? tanda vital berlaku?
    alt Periode sudah ditutup
        OBS-->>Layar: 409 periode sudah ditutup
    else Tautan di luar lingkup
        OBS-->>Layar: 400 beserta sebabnya
    else Lolos
        OBS->>DB: simpan; pelaku dari token
        DB-->>OBS: baris pemantauan
        OBS-->>Layar: 200 + proyeksi vitalSign + recordedByName
        Layar-->>Perawat: riwayat pemantauan bertambah, lengkap dengan angka tanda vital
    end
```

---

## 13. Encounter-first — 22 September 2026

Desain target untuk sub-slice `S1`…`S6` dan `S8` gate
[evidence/02-requirement-completeness-gate.md](evidence/02-requirement-completeness-gate.md), dari
`IGD-DEC-139`, `142`…`148`, `150`…`154`. `S7` (kelayakan dokter jaga) **tidak** dirancang — ditahan
`IGD-OQ-102`/`103`. Kontrak: API `0.11.0` §8, validation `0.8.0` §10, state `0.5.0` §8, integration `0.4.0`
§5, permission/audit `0.5.0` §7. Status seluruh isi bagian ini: `draft`, **Rencana (belum tersedia)**.

Prefix `Emg` sudah terdaftar di `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 19
(`EmergencyInstallationManagement`, `ACTIVE / LEGACY`) — tiga tabel baru **tidak** butuh baris registry
baru. Keberlakuan QBE: `NEW CODE` untuk tabel/berkas baru; `TOUCHED LEGACY` untuk `EmgVisit`,
`EmergencyVisitController`, `EmergencyVisitService`, dan `PatientEncounterController`.

### 13.1 Tabel kepemilikan data

| Kelompok data | Pemilik | Dipakai slice ini | Dibuat ulang? |
| --- | --- | --- | --- |
| Encounter pasien (`RegPatientEncounter`) | Registration Management | **Ya** — dibuat di pintu Registrasi; kolom status akhir ditulis IGD (daftar tertutup, integration §5.2) | **Tidak**. Nol kolom baru (`IGD-DEC-145`) |
| Antrean (`TrxQueue`) | Registration Management | **Tidak** — Emergency tidak membuat antrean (`IGD-DEC-144`) | Tidak |
| Pasien (`MstPatient`) | Patient Management / Master Patient | Ya — rekam pengganti lewat alur pasien baru yang ada (`IGD-DEC-151`) | **Tidak**. Nol tabel pasien IGD |
| Kunjungan IGD (`EmgVisit`) | IGD | Ya — **Diperbarui** tiga kolom waktu tiba | — |
| Catatan override pendaftaran ganda | IGD | Ya | **Baru**: `EmgDuplicateEpisodeOverride` |
| Run dan baris rekonsiliasi | IGD | Ya | **Baru**: `EmgEncounterReconciliationRun`, `EmgEncounterReconciliationItem` |
| Penguncian catatan klinis | Medical Record Management | Ya — `ClinicalDocumentIntegrityService` dipakai ulang | Tidak |
| Triage, penugasan dokter | IGD | Dibaca untuk batas koreksi waktu tiba | Tidak |
| Pengguna (`AspNetUsers`) | Administrator | Pelaku | Tidak |

### 13.2 Class diagram — episode dan pintu encounter

```mermaid
classDiagram
    class RegPatientEncounter {
        <<Sudah ada · Registration>>
        +Guid Id
        +Guid PatientId
        +EncounterType EncounterType
        +EncounterStatus EncounterStatus
        +DateTime RegisteredAt
        +DateTime? CompletedAt
        +DateTime? NoShowAt
        +Guid? NoShowByUserId
        +string? NoShowReason
        +bool IsCancel
        +DateTime? CancelledAt
    }
    class EmgVisit {
        <<Diperbarui · IGD>>
        +Guid Id
        +Guid? EncounterId
        +Guid? PatientId
        +DateTime ArrivalDateTime
        +EmergencyArrivalTimeSource ArrivalTimeSource
        +Guid? ArrivalConfirmedByUserId
        +DateTime? ArrivalConfirmedAt
        +EmergencyVisitStatus VisitStatus
    }
    class EmgDuplicateEpisodeOverride {
        <<Baru · IGD>>
        +Guid Id
        +Guid EncounterId
        +Guid PatientId
        +Guid? OverriddenEncounterId
        +Guid? OverriddenVisitId
        +string Reason
        +Guid OverriddenByUserId
        +DateTime OverriddenAt
    }
    class EmergencyEpisodeRule {
        <<Baru · static · IGD>>
        +IsEncounterEnded(RegPatientEncounter) bool
        +FindOpenEpisodeAsync(db, patientId, exceptEncounterId, ct) OpenEpisode?
        +LockPatientEpisodeAsync(db, patientId, ct)
    }
    RegPatientEncounter "1" --> "0..1" EmgVisit : EncounterId (unique)
    EmgDuplicateEpisodeOverride "0..1" --> "1" RegPatientEncounter : EncounterId (unique)
    EmgDuplicateEpisodeOverride --> "0..1" RegPatientEncounter : OverriddenEncounterId
    EmgDuplicateEpisodeOverride --> "0..1" EmgVisit : OverriddenVisitId
    EmergencyEpisodeRule ..> RegPatientEncounter : membaca
    EmergencyEpisodeRule ..> EmgVisit : membaca
```

### 13.3 Class diagram — rekonsiliasi

```mermaid
classDiagram
    class EmgEncounterReconciliationRun {
        <<Baru · IGD>>
        +Guid Id
        +string RunNumber
        +EmergencyReconciliationRunStatus Status
        +string Reason
        +int CountK1
        +int CountK1Outpatient
        +int CountK2
        +int CountK3
        +int CountK4
        +Guid ExecutedByUserId
        +DateTime ExecutedAt
        +Guid? ReversedByUserId
        +DateTime? ReversedAt
        +string? ReverseReason
    }
    class EmgEncounterReconciliationItem {
        <<Baru · IGD>>
        +Guid Id
        +Guid RunId
        +Guid EncounterId
        +Guid EmergencyVisitId
        +EmergencyReconciliationClass Class
        +EncounterStatus StatusBefore
        +DateTime? CompletedAtBefore
        +EncounterStatus StatusAfter
        +DateTime? CompletedAtAfter
        +bool IsReversed
        +string? ReverseSkipReason
    }
    EmgEncounterReconciliationRun "1" --> "1..*" EmgEncounterReconciliationItem
    EmgEncounterReconciliationItem --> "1" RegPatientEncounter
    EmgEncounterReconciliationItem --> "1" EmgVisit
```

### 13.4 Penjelasan class

| Class | Jenis | Status | Lokasi file | Tanggung jawab |
| --- | --- | --- | --- | --- |
| `EmgVisit` | Model | **Diperbarui** | `Areas/HealthServices/EmergencyInstallationManagement/Models/EmgVisit.cs` | + `ArrivalTimeSource`, `ArrivalConfirmedByUserId`, `ArrivalConfirmedAt`, navigasi `ArrivalConfirmedByUser` |
| `EmgDuplicateEpisodeOverride` | Model | **Baru** | `…/EmergencyInstallationManagement/Models/EmgDuplicateEpisodeOverride.cs` | Catatan tambah-saja pendaftaran ganda beralasan (`IGD-DEC-145`) |
| `EmgEncounterReconciliationRun` | Model | **Baru** | `…/EmergencyInstallationManagement/Models/EmgEncounterReconciliationRun.cs` | Kepala satu run rekonsiliasi |
| `EmgEncounterReconciliationItem` | Model | **Baru** | `…/EmergencyInstallationManagement/Models/EmgEncounterReconciliationItem.cs` | Satu encounter yang ditulis run, dengan nilai sebelum/sesudah |
| `EmergencyArrivalTimeSource` | Enum | **Baru** | `…/EmergencyInstallationManagement/Enums/EmergencyArrivalTimeSource.cs` | `Unverified = 0` (bawaan), `Fallback = 1`, `Confirmed = 2` |
| `EmergencyVisitStartMode` | Enum | **Baru** | `…/Enums/EmergencyVisitStartMode.cs` | `Triage = 1`, `ImmediateCare = 2` — hanya untuk request |
| `EmergencyReconciliationClass` | Enum | **Baru** | `…/Enums/EmergencyReconciliationClass.cs` | `K1 = 1`, `K1Outpatient = 2`, `K2 = 3`, `K3 = 4`, `K4 = 5` |
| `EmergencyReconciliationRunStatus` | Enum | **Baru** | `…/Enums/EmergencyReconciliationRunStatus.cs` | `Executed = 1`, `Reversed = 2` |
| `EmergencyEpisodeRule` | Static class | **Baru** | `…/EmergencyInstallationManagement/Services/EmergencyEpisodeRule.cs` | **Satu-satunya** rumus "encounter berakhir" dan "episode terbuka", plus kunci per pasien. `public static`, menerima `ApplicationDbContext` — dapat dipanggil controller Registrasi **tanpa DI dan tanpa `Program.cs`** (pola `EmergencyVisitService.PeriksaJenisEncounter`) |
| `EmergencyEncounterReconciliation` | Static class | **Baru** | `…/Services/EmergencyEncounterReconciliation.cs` | Preview, eksekusi, pembalikan run. Static dengan `ApplicationDbContext` sebagai parameter — nol `Program.cs` |
| `EmergencyVisitService` | Service | **Diperbarui** | `…/Services/EmergencyVisitService.cs` | `CariEpisodeAktifAsync` **mendelegasikan** ke `EmergencyEpisodeRule` (pemanggil lama tidak berubah); + `GetTriageQueueAsync`, `StartVisitAsync`, `MarkNoShowAsync`, `ApplyEncounterClosure` (tidak menyimpan sendiri), `ValidateArrivalTimeAsync` |
| `EmergencyVisitController` | Controller | **Diperbarui** | `…/Controllers/EmergencyVisitController.cs` | Action baru `TriageQueue`, `StartTriage`, `NoShow`, `UpdateArrivalTime`; perubahan `Create` (transaksi + kunci), `Update` (kunci ruas), `UpdateVisitStatus` dan `Complete` (penutupan encounter), `ActiveEpisode` (ruas `encounter`). Constructor + `ClinicalDocumentIntegrityService` (sudah terdaftar, `Program.cs:392`) |
| `EmergencyEncounterReconciliationController` | Controller | **Baru** | `…/Controllers/EmergencyEncounterReconciliationController.cs` | Lima action API §8.4; `[AccessController]` resource `EmergencyEncounterReconciliation` |
| `PatientEncounterController` | Controller | **Diperbarui** — **berkas Registrasi**, `IGD-DEC-135` | `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs` | Cabang Emergency di `CreateEncounterCoreAsync` (kunci → rumus → override → tanpa antrean) sebelum alokasi nomor (`:567`); penolakan Emergency di `UpdateEncounterStatus`; syarat "belum punya kunjungan" + kunci di `CancelEncounter` |
| `EmergencyVisitDtos.cs` | DTO | **Diperbarui** | `…/EmergencyInstallationManagement/DTOs/EmergencyVisitDtos.cs` | + `EmergencyTriageQueueQuery` (PagedQuery), `EmergencyTriageQueueRowResponse` (Response), `StartEmergencyVisitRequest` (Create), `MarkEmergencyEncounterNoShowRequest` (Status), `EmergencyEncounterNoShowResponse` (Response), `UpdateEmergencyArrivalTimeRequest` (Update), `EmergencyActiveEncounterSummary` (Response); `EmergencyVisitResponse` + tiga ruas waktu tiba; `EmergencyActiveEpisodeResponse` + `Encounter` |
| `EmergencyEncounterReconciliationDtos.cs` | DTO | **Baru** | `…/DTOs/EmergencyEncounterReconciliationDtos.cs` | `…PreviewResponse`, `ExecuteEmergencyEncounterReconciliationRequest` (Create), `ReverseEmergencyEncounterReconciliationRequest` (Status), `…RunResponse`, `…ItemResponse` |
| `PatientEncounterDtos.cs` | DTO | **Diperbarui** — berkas Registrasi | `Areas/HealthServices/RegistrationManagement/DTOS/PatientEncounterDtos.cs` | `PatientEncounterCreateRequest` + `DuplicateEpisodeOverrideReason` (`string?`, **wajib** `string?` supaya tidak terkena `[Required]` implisit) |
| `EmgVisitConfiguration` | Configuration | **Diperbarui** | `Repositories/Configurations/HealthServices/EmergencyInstallationManagement/EmgVisitConfiguration.cs` | `ArrivalTimeSource` `HasConversion<int>()` + `HasDefaultValue(Unverified)`; FK `ArrivalConfirmedByUserId` → `AspNetUsers`, `Restrict` |
| `EmgDuplicateEpisodeOverrideConfiguration` | Configuration | **Baru** | `Repositories/Configurations/HealthServices/EmergencyInstallationManagement/` | Lihat 13.6 |
| `EmgEncounterReconciliationRunConfiguration`, `…ItemConfiguration` | Configuration | **Baru** | Sama | Lihat 13.6 |
| `ApplicationDbContext` | DbContext | **Diperbarui** | `Repositories/ApplicationDbContext.cs` (DbSet `EmgVisits` di baris 809; `ApplyConfigurationsFromAssembly` di baris 903) | + tiga `DbSet`: `EmgDuplicateEpisodeOverrides`, `EmgEncounterReconciliationRuns`, `EmgEncounterReconciliationItems`. Configuration ditemukan otomatis (`ApplyConfigurationsFromAssembly`) |

**Transaksi.** `StartVisitAsync`, `MarkNoShowAsync`, dan cabang Emergency `CancelEncounter` membuka transaksi
eksplisit karena mengambil kunci per pasien. `Create` (jalur lama) **dibungkus** transaksi eksplisit untuk
alasan yang sama. `ApplyEncounterClosure` **tidak** membuka transaksi dan **tidak** menyimpan — ikut
`SaveChanges` aksi kunjungan. Rekonsiliasi: satu transaksi per run.

### 13.5 Arsitektur folder

```text
NewQuilvianSystemBackend/
├── Areas/HealthServices/EmergencyInstallationManagement/
│   ├── Controllers/
│   │   ├── EmergencyVisitController.cs                       Diperbarui
│   │   └── EmergencyEncounterReconciliationController.cs     Baru
│   ├── DTOs/
│   │   ├── EmergencyVisitDtos.cs                             Diperbarui
│   │   └── EmergencyEncounterReconciliationDtos.cs           Baru
│   ├── Enums/
│   │   ├── EmergencyArrivalTimeSource.cs                     Baru
│   │   ├── EmergencyVisitStartMode.cs                        Baru
│   │   ├── EmergencyReconciliationClass.cs                   Baru
│   │   └── EmergencyReconciliationRunStatus.cs               Baru
│   ├── Models/
│   │   ├── EmgVisit.cs                                       Diperbarui
│   │   ├── EmgDuplicateEpisodeOverride.cs                    Baru
│   │   ├── EmgEncounterReconciliationRun.cs                  Baru
│   │   └── EmgEncounterReconciliationItem.cs                 Baru
│   └── Services/
│       ├── EmergencyVisitService.cs                          Diperbarui
│       ├── EmergencyEpisodeRule.cs                           Baru (static)
│       └── EmergencyEncounterReconciliation.cs               Baru (static)
├── Areas/HealthServices/RegistrationManagement/              (milik Registrasi — IGD-DEC-135)
│   ├── Controllers/PatientEncounterController.cs             Diperbarui
│   └── DTOS/PatientEncounterDtos.cs                          Diperbarui
├── Repositories/
│   ├── ApplicationDbContext.cs                               Diperbarui (3 DbSet)
│   └── Configurations/HealthServices/EmergencyInstallationManagement/
│       ├── EmgVisitConfiguration.cs                          Diperbarui
│       ├── EmgDuplicateEpisodeOverrideConfiguration.cs       Baru
│       ├── EmgEncounterReconciliationRunConfiguration.cs     Baru
│       └── EmgEncounterReconciliationItemConfiguration.cs    Baru
├── Migrations/                                               3 migration — dijalankan Rizki (13.8)
└── Program.cs                                                TIDAK disentuh
```

**Utang teknis yang dicatat, tidak dirapikan.** Folder DTO Registrasi bernama `DTOS` (huruf besar),
berbeda dari `DTOs` modul lain — dipakai apa adanya.

### 13.6 Status model dan kolom

**`EmgVisit` — Diperbarui** (`public."EmgVisit"`)

| Kolom | Tipe | Wajib | Bawaan | Validasi | Sensitif |
| --- | --- | :-: | --- | --- | :-: |
| `ArrivalTimeSource` | `int` (`EmergencyArrivalTimeSource`) | Ya | `0` (`Unverified`) | Nilai enum | Tidak |
| `ArrivalConfirmedByUserId` | `uuid?` | Tidak | `null` | FK `AspNetUsers`, `Restrict`; diisi dari token | Tidak |
| `ArrivalConfirmedAt` | `timestamptz?` | Tidak | `null` | Waktu server | Tidak |

Index: tidak ada yang baru. Perilaku hapus: tidak berubah.

**`EmgDuplicateEpisodeOverride` — Baru** (`public."EmgDuplicateEpisodeOverride"`, mewarisi `IdentityModel`)

| Kolom | Tipe | Wajib | Aturan | Sensitif |
| --- | --- | :-: | --- | :-: |
| `Id` | `uuid` | Ya | PK | Tidak |
| `EncounterId` | `uuid` | Ya | FK `RegPatientEncounter`, `Restrict`; **unique** — satu catatan per encounter baru | Tidak |
| `PatientId` | `uuid` | Ya | FK `MstPatient`, `Restrict`; index | Tidak |
| `OverriddenEncounterId` | `uuid?` | Tidak | FK `RegPatientEncounter`, `Restrict` — episode lama bila berupa encounter | Tidak |
| `OverriddenVisitId` | `uuid?` | Tidak | FK `EmgVisit`, `Restrict` — episode lama bila berupa kunjungan | Tidak |
| `Reason` | `varchar(500)` | Ya | Di-*trim*, 1–500 | **Ya** — dapat memuat keterangan klinis |
| `OverriddenByUserId` | `uuid` | Ya | FK `AspNetUsers`, `Restrict`; dari token | Tidak |
| `OverriddenAt` | `timestamptz` | Ya | Waktu server | Tidak |

Check constraint: `OverriddenEncounterId IS NOT NULL OR OverriddenVisitId IS NOT NULL`. Tambah-saja: tidak
ada endpoint ubah/hapus.

**`EmgEncounterReconciliationRun` — Baru**

| Kolom | Tipe | Wajib | Aturan | Sensitif |
| --- | --- | :-: | --- | :-: |
| `Id` | `uuid` | Ya | PK | Tidak |
| `RunNumber` | `varchar(50)` | Ya | Unique; dibentuk `DocumentNumberService` | Tidak |
| `Status` | `int` | Ya | `EmergencyReconciliationRunStatus` | Tidak |
| `Reason` | `varchar(500)` | Ya | 1–500 | Ya |
| `CountK1`, `CountK1Outpatient`, `CountK2`, `CountK3`, `CountK4` | `int` | Ya | Hasil hitung saat eksekusi | Tidak |
| `ExecutedByUserId` | `uuid` | Ya | FK `AspNetUsers`, `Restrict` | Tidak |
| `ExecutedAt` | `timestamptz` | Ya | Server | Tidak |
| `ReversedByUserId` | `uuid?` | Tidak | FK `AspNetUsers`, `Restrict` | Tidak |
| `ReversedAt` | `timestamptz?` | Tidak | Server | Tidak |
| `ReverseReason` | `varchar(500)?` | Tidak | Wajib saat dibalik | Ya |

**`EmgEncounterReconciliationItem` — Baru**

| Kolom | Tipe | Wajib | Aturan | Sensitif |
| --- | --- | :-: | --- | :-: |
| `Id` | `uuid` | Ya | PK | Tidak |
| `RunId` | `uuid` | Ya | FK run, `Cascade` (baris ikut run; run sendiri tidak pernah dihapus) | Tidak |
| `EncounterId` | `uuid` | Ya | FK `RegPatientEncounter`, `Restrict`; index | Tidak |
| `EmergencyVisitId` | `uuid` | Ya | FK `EmgVisit`, `Restrict` | Tidak |
| `Class` | `int` | Ya | Hanya `K1` atau `K1Outpatient` yang pernah ditulis | Tidak |
| `StatusBefore`, `StatusAfter` | `int` | Ya | Nilai `EncounterStatus` | Tidak |
| `CompletedAtBefore`, `CompletedAtAfter` | `timestamptz?` | Tidak | — | Tidak |
| `IsReversed` | `bool` | Ya | Bawaan `false` | Tidak |
| `ReverseSkipReason` | `varchar(200)?` | Tidak | Diisi bila baris dilewati saat pembalikan | Tidak |

Unique `(RunId, EncounterId)`.

**`RegPatientEncounter` — Sudah ada**, nol kolom baru; kolom yang ditulis IGD: integration §5.2.

### 13.7 Konkurensi

| Risiko | Penjaga |
| --- | --- |
| Dua pendaftaran Emergency serentak | `pg_advisory_xact_lock(hashtext('EMG_EPISODE_' || patientId))` sebelum rumus episode; diambil **sebelum** kunci penomoran (integration §5.3) |
| Mulai Triage / Tangani Segera serentak | Kunci per pasien + unique `EmgVisit.EncounterId` (tangkap `UniqueViolation`) |
| NoShow atau batal Registrasi serentak dengan Mulai Triage | Kunci per pasien + periksa ulang di dalam kunci |
| Rekonsiliasi atas data basi | `expectedCount` + evaluasi ulang tiap baris di dalam transaksi run |
| Pembalikan atas encounter yang sudah berubah | Baris dilewati bila nilai sekarang ≠ `StatusAfter`/`CompletedAtAfter` |

Kunci penomoran encounter yang sudah ada (`PatientEncounterNumberService`) **tidak** dijadikan sandaran
invariant bisnis (capability map S3.2.6).

### 13.8 Rencana migration

Dijalankan **Rizki sendiri**; agent berhenti sebelum `dotnet ef migrations add`. Ketiganya aditif dan
dapat dijalankan tanpa mematikan layanan.

| Urutan | Nama | Isi | Tanpa downtime | Data lama | Cara mundur | Task |
| ---: | --- | --- | :-: | --- | --- | --- |
| 1 | `AddEmergencyArrivalTimeSource` | 3 kolom pada `EmgVisit` + FK | Ya — kolom berbawaan konstan | Seluruh baris lama `Unverified` (tidak mengarang konfirmasi) | `Down()` **berpenjaga**: menolak bila ada baris `ArrivalTimeSource <> 0`, supaya konfirmasi perawat tidak hilang diam-diam; pola `BE-IGD-048` | `BE-IGD-055` |
| 2 | `AddEmergencyDuplicateEpisodeOverride` | Tabel `EmgDuplicateEpisodeOverride` | Ya | Tidak ada | `Down()` berpenjaga: menolak bila tabel berisi (jejak audit) | `BE-IGD-053` |
| 3 | `AddEmergencyEncounterReconciliation` | Dua tabel rekonsiliasi | Ya | Tidak ada | `Down()` berpenjaga: menolak bila ada run | `BE-IGD-052` |

Snapshot EF wajib diperiksa hanya bertambah blok ketiga tabel ini (pelajaran snapshot kehilangan blok modul
lain). Blok `migrationBuilder.Sql` untuk penjaga `Down()` ditulis manual dan **diperiksa** sebelum
`database update`.

### 13.9 Rencana data master awal

**Tidak ada master baru.** Slice ini memakai `EmgSetting.DefaultEmergencyServiceUnitId`, master cara datang,
dan master jenis kasus yang sudah ada. `IsQueueRequired` unit/klinik IGD boleh dirapikan pemilik master,
tetapi tidak lagi menentukan perilaku (`IGD-DEC-144`).

### 13.10 Yang sengaja tidak dibuat

| Tidak dibuat | Sebab |
| --- | --- |
| Kolom override / waktu tiba / status "menunggu triage" pada `RegPatientEncounter` | `IGD-DEC-145`: jangan tambah ruas IGD di tabel global |
| Unique index bersyarat "satu encounter Emergency terbuka per pasien" | Menolak pendaftaran ganda yang sah (`IGD-DEC-146`) |
| Service baru ber-DI untuk aturan episode atau rekonsiliasi | Menuntut baris `Program.cs`; static class cukup dan sudah berpola di modul ini |
| Tabel riwayat koreksi waktu tiba | `IGD-DEC-152` tidak memintanya; penanda menyimpan nilai terakhir |
| Fitur rekam pasien sementara / penggabungan rekam | Milik Master Patient (`IGD-DEC-151`, `IGD-OQ-098`) |
| Endpoint pembatalan NoShow | `IGD-DEC-142`: final |
| Migration data rekonsiliasi | `IGD-DEC-148` memilih endpoint admin |
| Kelayakan dokter jaga | `S7` ditahan `IGD-OQ-102`/`103` |

### 13.11 Pertanyaan desain untuk ditinjau pemilik sebelum development lock

Bukan keputusan bisnis baru — pilihan realisasi yang agent ambil dan perlu dilihat pemilik.

**Dijawab 22 September 2026:** ketiganya disahkan apa adanya — `IGD-OQ-104` → `IGD-DEC-159`, `IGD-OQ-105` →
`IGD-DEC-160`, `IGD-OQ-106` → `IGD-DEC-161`. Rekonsiliasi tanpa layar (`IGD-OQ-107`) → `IGD-DEC-162`.

| ID | Pilihan desain | Alasan | Bila ditolak |
| --- | --- | --- | --- |
| `IGD-OQ-104` | Waktu tiba hanya dapat diubah lewat `PATCH /{id}/arrival-time`; `PUT` menolak perubahannya | Satu jalur yang menegakkan `IGD-DEC-152` dan menulis penanda; nol layar memakai `PUT` | `PUT` menjalankan validasi yang sama dan ikut menandai `Confirmed` |
| `IGD-OQ-105` | Konfirmasi waktu tiba sesudah Tangani Segera diwajibkan **di layar** triage susulan, tetapi backend **tidak** menolak penyimpanan triage bila waktu tiba masih `Fallback` | Backend yang menahan catatan klinis karena urusan waktu tiba berisiko menunda dokumentasi pasien gawat | Backend menolak triage selama `Fallback` |
| `IGD-OQ-106` | Ruas kedatangan non-waktu (cara datang, jenis kasus, keluhan, penanda pasien tanpa identitas) diisi opsional pada Mulai Triage; keluhan diisi awal dari `RegPatientEncounter.ChiefComplaint` | Loket tidak lagi melahirkan kunjungan, sehingga ruas milik kunjungan tidak punya tempat di loket | Loket tetap mengisi, disimpan sementara di tempat lain (butuh keputusan penyimpanan) |

## 14. Penutupan kunjungan lewat disposisi yang dilaksanakan — 23 September 2026

Desain target untuk `IGD-DEC-163`…`169` (amendment pass 23 September 2026). Slice ini **sempit**: satu aturan baru,
satu kolom baru, nol tabel baru, nol endpoint baru. Status seluruh isi bagian ini: `draft`, **Rencana (belum tersedia)**.

Masukan: decision log bagian "Amendment pass 23 September 2026"; capability map suplemen revision 3.3
(`dce1f138`); gerbang requirement `S5` `READY_FOR_DOMAIN_DESIGN`. Keberlakuan QBE: `TOUCHED LEGACY` untuk
`EmgVisit`, `EmergencyVisitService`, dan empat controller pemicu; `NEW CODE` untuk method penutupan susulan.

### 14.1 Tabel kepemilikan data

| Kelompok data | Pemilik | Dipakai slice ini | Dibuat ulang? |
| --- | --- | --- | --- |
| Kunjungan IGD (`EmgVisit`) | IGD | Ya — **Diperbarui**, satu kolom penanda asal penutupan | — |
| Disposisi IGD (`EmgDisposition`) | IGD | Ya — dibaca sebagai pemicu; **nol kolom baru** | Tidak |
| Observasi, kepergian, pesanan serah terima | IGD | Ya — dibaca sebagai penahan; **nol kolom baru** | Tidak |
| Encounter pasien (`RegPatientEncounter`) | Registration Management | Ya — ditutup lewat jalur `BE-IGD-051` yang sudah ada, daftar kolom tertutup integration §5.2 | **Tidak**. Nol kolom baru |
| Order darah (`BbkBloodOrder`), order laboratorium (`LabOrder`) | Bank Darah, Laboratorium | **Tidak disentuh** — hanya menjadi pembaca hilir (`IGD-DEC-169`) | Tidak |

### 14.2 Class diagram — pemicu penutupan

```mermaid
classDiagram
    class EmgVisit {
        <<Diperbarui · IGD>>
        +Guid Id
        +EmergencyVisitStatus VisitStatus
        +DateTime? VisitCompletedAt
        +Guid? ClosedByDispositionId
    }
    class EmgDisposition {
        <<Sudah ada · IGD>>
        +Guid Id
        +Guid EmergencyVisitId
        +EmergencyDispositionStatus Status
        +DateTime? ExecutedAt
    }
    class EmergencyVisitService {
        <<Diperbarui · IGD>>
        +TryApplyVisitStatus(...) bool
        +ApplyEncounterClosureAsync(...) bool
        +TryCloseAfterDispositionAsync(visitId, actor, now, ct) HasilPenutupanSusulan
    }
    class EmergencyDispositionService {
        <<Sudah ada · IGD>>
        +ValidateVisitClosureAsync(visit, ct) string?
    }
    class EmergencyDepartureService {
        <<Sudah ada · IGD>>
        +AmbilPesananPenahanPenutupanAsync(visitId, ct) string[]
    }
    EmergencyVisitService ..> EmergencyDispositionService : memanggil penjaga
    EmergencyDispositionService ..> EmergencyDepartureService : pesanan penahan
    EmergencyVisitService ..> EmgVisit : menutup
    EmergencyVisitService ..> EmgDisposition : membaca pemicu
```

**Nol siklus dependency.** `EmergencyDispositionService` hanya bergantung pada `ApplicationDbContext` dan
`EmergencyDepartureService`; ia **tidak** memanggil `EmergencyVisitService`. Karena itu `EmergencyVisitService`
boleh memanggilnya. Keduanya sudah terdaftar di `Program.cs` (`:515`, `:520`), sehingga slice ini **nol perubahan
`Program.cs`**.

### 14.3 Penjelasan class

| Class | Jenis | Status | Lokasi file | Tanggung jawab |
| --- | --- | --- | --- | --- |
| `EmgVisit` | Model | **Diperbarui** | `Areas/HealthServices/EmergencyInstallationManagement/Models/EmgVisit.cs` | + `ClosedByDispositionId` beserta navigasi `ClosedByDisposition` |
| `EmergencyVisitService` | Service | **Diperbarui** | `…/Services/EmergencyVisitService.cs` | + `TryCloseAfterDispositionAsync` dan record hasilnya; constructor + `EmergencyDispositionService`. **Tidak** menyimpan sendiri — penyimpanan tetap milik pemanggil, sama seperti `ApplyEncounterClosureAsync` |
| `EmergencyDispositionController` | Controller | **Diperbarui** | `…/Controllers/EmergencyDispositionController.cs` | Sesudah disposisi berpindah ke `Executed` (`:300` `UpdateDispositionStatus`), panggil penutupan susulan sebelum `SaveChanges`. Tambah penolakan pembatalan atas kunjungan yang sudah selesai |
| `EmergencyObservationController` | Controller | **Diperbarui** | `…/Controllers/EmergencyObservationController.cs` | Sesudah observasi tidak lagi aktif (`:261` `UpdateObservationStatus`), panggil penutupan susulan |
| `EmergencyDepartureController` | Controller | **Diperbarui** | `…/Controllers/EmergencyDepartureController.cs` | Sesudah serah terima diterima/ditolak/dibatalkan (`:162`, `:172`, `:206`) dan sesudah sikap pesanan ditetapkan (`:118`, `:129`, `:135`), panggil penutupan susulan |
| `EmergencyVisitController` | Controller | **Diperbarui** | `…/Controllers/EmergencyVisitController.cs` | `GET /` bertambah saringan `awaitingClosure`; `EmergencyVisitResponse` bertambah dua ruas penahan |
| `EmergencyVisitDtos.cs` | DTO | **Diperbarui** | `…/DTOs/EmergencyVisitDtos.cs` | + `IsAwaitingClosure`, `AwaitingClosureReason` pada response |
| `EmgVisitConfiguration` | Configuration | **Diperbarui** | `Repositories/Configurations/HealthServices/EmergencyInstallationManagement/EmgVisitConfiguration.cs` | FK `ClosedByDispositionId` → `EmgDisposition`, `Restrict` |

### 14.4 Arsitektur folder

```text
Areas/HealthServices/EmergencyInstallationManagement/
├── Controllers/
│   ├── EmergencyDispositionController.cs     Diperbarui  (pemicu 1 + penolakan pembatalan)
│   ├── EmergencyObservationController.cs     Diperbarui  (pemicu 2)
│   ├── EmergencyDepartureController.cs       Diperbarui  (pemicu 3 dan 4)
│   └── EmergencyVisitController.cs           Diperbarui  (saringan + ruas response)
├── DTOs/EmergencyVisitDtos.cs                Diperbarui
├── Models/EmgVisit.cs                        Diperbarui  (+1 kolom)
└── Services/EmergencyVisitService.cs         Diperbarui  (+1 method)

Repositories/Configurations/…/EmgVisitConfiguration.cs   Diperbarui  (+1 FK)
Migrations/                                              1 migration — dibuat Rizki
Program.cs                                               TIDAK disentuh
```

### 14.5 Status model dan kolom

**`EmgVisit` — Diperbarui** (`public."EmgVisit"`)

| Kolom | Tipe | Wajib | Bawaan | Validasi | Sensitif |
| --- | --- | :-: | --- | --- | :-: |
| `ClosedByDispositionId` | `uuid?` | Tidak | `null` | FK `EmgDisposition`, `Restrict`. Terisi **hanya** bila penutupan berasal dari disposisi yang dilaksanakan; kosong berarti ditutup petugas lewat aksi selesaikan kunjungan | Tidak |

Index: satu index FK bawaan konvensi EF. Perilaku hapus: tidak berubah. Nol kolom baru pada tabel lain.

Kolom ini yang memenuhi `IGD-DEC-165`: riwayat kunjungan dapat menunjukkan bahwa penutupan berasal dari disposisi,
dan disposisi mana. Tanpanya, penutupan otomatis tidak dapat dibedakan dari penutupan manual.

### 14.6 Rencana migration

| Urutan | Nama | Isi | Tanpa downtime | Data lama | Cara mundur | Task |
| ---: | --- | --- | :-: | --- | --- | --- |
| 1 | `AddEmergencyVisitClosureSource` | 1 kolom `ClosedByDispositionId` pada `EmgVisit` + FK + index | Ya — kolom nullable tanpa bawaan | Seluruh baris lama `null` (artinya: ditutup manual atau belum ditutup) | `Down()` **berpenjaga**: menolak bila ada baris `ClosedByDispositionId IS NOT NULL`, supaya jejak asal penutupan tidak hilang diam-diam | Kartu baru R3.14 |

Dibuat **Rizki sendiri**, di atas migration terakhir yang berlaku. Agent berhenti sebelum `dotnet ef migrations add`.

### 14.7 Rencana data master awal

**Tidak ada master baru.** `IGD-DEC-163` sengaja tidak menambah kolom penanda pada `EmgDispositionType`: aturan
berlaku untuk semua jenis disposisi, sehingga jenis yang ditambahkan rumah sakit kelak ikut berlaku tanpa
pengisian master apa pun.

### 14.8 Konkurensi

| Risiko | Penjaga |
| --- | --- |
| Dua petugas membereskan dua penahan terakhir bersamaan, keduanya mencoba menutup | `TryApplyVisitStatus` menolak perpindahan dari `Completed`, sehingga percobaan kedua tidak berbuat apa-apa dan tidak menimpa `VisitCompletedAt` |
| Penutupan susulan berbarengan dengan aksi selesaikan kunjungan manual | Sama — satu-satunya penulis `VisitStatus` tetap penjaga transisi `BE-IGD-018` |
| Penahan dibereskan lalu segera muncul penahan baru | Penutupan dievaluasi pada saat aksi disimpan; bila penahan baru muncul sesudahnya, kunjungan sudah tertutup dan penahan itu ditolak jalur masing-masing |

### 14.9 Yang sengaja tidak dibuat

| Tidak dibuat | Sebab |
| --- | --- |
| Proses latar penutup terjadwal | `IGD-DEC-165` menolak penutupan tanpa pelaku manusia; pelaku selalu petugas yang membereskan penahan terakhir |
| Kolom penanda "menutup kunjungan" pada `EmgDispositionType` | `IGD-DEC-163` berlaku untuk semua jenis; kolom itu justru membuka celah "jenis yang lupa ditandai" |
| Endpoint atau layar khusus daftar "menunggu penutupan" | `IGD-DEC-168` memilih saringan pada daftar kunjungan yang sudah ada |
| Kolom `VisitClosureSource` bertipe enum | Satu kolom FK `ClosedByDispositionId` sudah menjawab dua hal sekaligus: dari mana penutupan berasal, dan disposisi mana |
| Pembukaan kembali kunjungan yang sudah selesai | `IGD-DEC-166`; invariant `Completed` final sudah berlaku di source hari ini |
| Perubahan pada modul Bank Darah dan Laboratorium | `IGD-DEC-169` menerima konsekuensi hilir apa adanya |
