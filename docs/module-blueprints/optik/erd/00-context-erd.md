# Optik — Context ERD

| Field | Nilai |
| --- | --- |
| Blueprint ID | `OPT-BP-001` |
| Revision | `1` — `draft` |
| Sumber konteks | `03-domain-architecture.md` (`OPT-HDA-001`) bagian C |

Diagram ini hanya memperlihatkan **arah ketergantungan** antara tabel milik Optik dan tabel milik
modul lain. Kolom lengkap ada di ERD per konteks dan di [data-dictionary.md](data-dictionary.md).

```mermaid
erDiagram
    MstPatient ||--o{ RegPatientEncounter : "Patient Management → Registrasi (Sudah ada)"
    RegPatientEncounter ||--o{ OptRefractionExam : "Registrasi → Optik (Baru)"
    RegPatientEncounter ||--o{ OptPrescription : "Registrasi → Optik (Baru)"
    RegPatientEncounter ||--o{ OptOrder : "Registrasi → Optik (Baru)"
    RegPatientEncounter ||--o{ OptWarrantyClaim : "kunjungan keluhan (Baru)"
    RegPatientEncounter |o--o{ RegPatientEncounter : "rujukan Poli Mata (Diperbarui)"
    MstWorkforceProfile ||--o{ OptRefractionExam : "HR → Optik, pemeriksa"
    OptRefractionExam |o--o{ OptPrescription : "resep internal"
    OptPrescription ||--o{ OptOrder : "versi resep yang dikerjakan"
    MstDrug ||--o{ OptOrderItem : "Master Data → Optik, barang"
    MstDrug ||--o| OptItemProfile : "atribut Optik (Baru)"
    MstSupplier |o--o{ OptOrder : "lab lensa luar"
    MstServiceUnit ||--o| OptSetting : "pengaturan per unit (Baru)"
    OptOrder ||--o{ OptWarrantyClaim : "keluhan"
```

| Arah | Dari (pemilik) | Ke (pemakai) | Bentuk keterhubungan |
| --- | --- | --- | --- |
| Upstream | Patient Management | Optik | `PatientId` |
| Upstream | Registrasi | Optik | `EncounterId` |
| Upstream | Human Resource | Optik | `WorkforceProfileId` + baca izin |
| Upstream | Master Data | Optik | `DrugId`, `ServiceUnitId`, `SupplierId` |
| Downstream | Optik | Farmasi | Perintah stok lewat `DrugStockService` (kunci `SourceDocumentType = OpticalOrder`, `SourceDocumentId = OptOrder.Id`) |
| Downstream | Optik | Billing | Fakta tagihan sumber `Optical` (`SourceAggregateId` = pemeriksaan/pesanan, `SourceItemId` = item) |
| Downstream | Optik | Rekam Medis | Pembaca `OptMedicalRecordDocumentSource` |
| Downstream | Optik | Medical Fee | Ditarik adapter Medical Fee |

Tidak ada tabel modul lain yang memegang foreign key ke tabel Optik, kecuali tautan logis di
Billing dan Farmasi (kolom sumber berbentuk Guid + jenis, bukan FK fisik).
