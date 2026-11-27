# Laboratorium — Context ERD

| Field | Value |
|---|---|
| Blueprint ID | `LAB-BP-001` |
| Revision | `5` — 2026-11-27: `AGG-LAB-04` Dokumen Hasil untuk Pasien dan dua tabel setelan (`S17-1`, `draft`); baca nomor WhatsApp dan tanggal lahir dari data induk pasien. Sebelumnya `4` — 2026-11-25: `BC-MCU` dicatat sebagai hulu baca (tautan komponen order MCU, `LAB-DA-001` A9.3); `LabOrder` diperluas `PlannedDate`, `LabOrderReschedule` baru (`S20-1`, `draft`). Sebelumnya `3` — 2026-10-20: hubungan baca laporan `S16b-1`/`S16b-2` ke Registrasi, Master Data, dan data pasien (A8.3); nol entity baru |
| Status | `draft` |
| Scope | Slice `S1a`, `S2`, `S3`, `S7`, `S10`, `S11`, `S13a`, `S13b`, `S14`, `S15` |
| Backend SHA | `c87d9c0` |

Dokumen ini memetakan **hubungan antar bounded context** dan ke mana arah ketergantungannya.
ERD rinci per konteks ada di `laboratory-operations.md`.

---

## Peta ketergantungan

```mermaid
erDiagram
    TrxPatientEncounter {
        uuid Id PK
        uuid PatientId FK "milik Patient Management"
        int EncounterType "1 Rawat Jalan, 2 IGD, 3 Rawat Inap"
        boolean IsWalkIn "sudah ada"
        boolean IsReferral "sudah ada"
        varchar ReferralNumber "sudah ada"
        uuid ReferralInstitutionId FK "BARU, milik Registration"
        uuid ReferralDoctorId FK "BARU, milik Registration"
    }
    MstProcedure {
        uuid Id PK
        varchar ProcedureCode UK
        boolean IsLaboratory "penanda pemeriksaan lab"
        int LabDiscipline "BARU, hanya bermakna bila IsLaboratory"
    }
    MstReferralInstitution {
        uuid Id PK
        varchar InstitutionCode UK
        varchar InstitutionName
        boolean IsActive
    }
    MstReferralDoctor {
        uuid Id PK
        uuid ReferralInstitutionId FK
        varchar DoctorName
        boolean IsActive
    }
    LabOrder {
        uuid Id PK
        uuid EncounterId FK "milik Registration"
        uuid ProcedureId FK "milik Master Data"
        int OrderStatus
        int Discipline "1 Pat Klinik, 2 Pat Anatomi, 3 Mikrobiologi"
    }
    LabSpecimen {
        uuid Id PK
        uuid LabOrderId FK
        varchar SpecimenBarcode UK
        int SpecimenStatus
    }
    LabExamination {
        uuid Id PK
        uuid LabOrderId FK
        uuid SpecimenId FK
        uuid ProcedureId FK "milik Master Data"
        numeric UnitPriceSnapshot "salinan saat kejadian"
        int Urgency "cito, per pemeriksaan"
        boolean IsDuplo
    }
    LabValueBound {
        uuid Id PK
        uuid ProcedureId FK "milik Master Data"
        int ResultForm "angka atau pilihan"
    }
    CliClinicalMilestoneFact {
        uuid Id PK
        int MilestoneKind "kelayakan tagih atau pembatalan"
        uuid SourceItemId FK "menunjuk pemeriksaan"
    }

    TrxPatientEncounter ||--o{ LabOrder : "1:N — Sudah ada, milik Registration"
    MstReferralInstitution ||--o{ TrxPatientEncounter : "1:N — Baru, milik Master Data"
    MstReferralInstitution ||--o{ MstReferralDoctor : "1:N — Baru, milik Master Data"
    MstReferralDoctor ||--o{ TrxPatientEncounter : "1:N — Baru, milik Master Data"
    MstProcedure ||--o{ LabOrder : "1:N — Sudah ada, milik Master Data"
    MstProcedure ||--o{ LabExamination : "1:N — Baru, milik Master Data"
    MstProcedure ||--o{ LabValueBound : "1:N — Baru, milik Master Data"
    LabOrder ||--o{ LabSpecimen : "1:N — Diperbarui"
    LabOrder ||--o{ LabExamination : "1:N — Baru"
    LabSpecimen ||--o{ LabExamination : "1:N — Baru, satu wadah menopang banyak pemeriksaan"
    LabExamination ||--o{ CliClinicalMilestoneFact : "1:N — Sudah ada, milik Billing Integration"
```

---

## Arah ketergantungan antar konteks

| Konteks | Peran | Yang dibaca dari luar | Yang diterbitkan ke luar |
|---|---|---|---|
| `BC-LAB` Operasional Laboratorium | **Pemilik** modul ini | Kunjungan, jenis pemeriksaan, tarif, identitas pengguna. **Sejak 2026-10-20 (`LAB-DA-001` A8.3):** jenis kunjungan dan unit layanan, nama unit, No. RM dan nama pasien — **baca saja**, untuk dua laporan `S16b` | Fakta milestone klinis ke Billing |
| `BC-REG` Registrasi | Hulu | — | Kunjungan pasien |
| `BC-MD` Data Induk | Hulu | — | Jenis pemeriksaan, tarif, kelompok umur |
| `BC-BIL` Billing | Hilir | Fakta milestone klinis | Seluruh akibat finansial |
| `BC-PLAT` Platform | Hulu | — | Identitas pengguna dan kewenangan |
| `BC-MCU` Medical Check-Up | Hulu — **baca saja**, dicatat 2026-11-25 (`LAB-DA-001` A9.3) | — | Ada/tidaknya komponen order MCU yang menautkan pesanan Lab (`McuOrderComponent.LabOrderId`) — penjaga jadwal ulang `INV-65`. Laboratorium tidak menulis ke MCU |

**Aturan yang tidak boleh dilanggar.** Tidak ada satu pun tabel Laboratorium yang menyalin
pasien, dokter, kunjungan, jenis pemeriksaan, tarif, atau sumber rujukan sebagai sumber
kebenaran tandingan. Yang disimpan hanyalah **kunci rujukan** dan **salinan sesaat** tarif untuk
keperluan penelusuran harga pada saat kejadian.

### Empat perubahan yang dikerjakan modul lain

Keempatnya disetujui `andryzainhome` dan `sukmagp` pada 2026-09-01 lewat `LAB-REQ-001`.
Laboratorium **tidak mengerjakannya** dan **tidak menulis** ke sana.

| Perubahan | Dikerjakan oleh | Dipakai Laboratorium untuk |
|---|---|---|
| Kolom `LabDiscipline` pada `MstProcedure` | Master Data | Menyaring katalog per disiplin dan menegakkan `INV-22` |
| `MstReferralInstitution` | Master Data | Pilihan instansi perujuk saat mendaftarkan pasien rujukan |
| `MstReferralDoctor` | Master Data | Pilihan dokter perujuk |
| Dua kolom penunjuk perujuk pada `TrxPatientEncounter` | Registration Management | Diisi lewat permintaan `INT-05`, dibaca saat menampilkan pesanan |

---

## Tabel status entity

| Entity | Status | Owner | Catatan |
|---|---|---|---|
| `TrxPatientEncounter` | **Diperbarui** | Registration Management | Tambah `ReferralInstitutionId` dan `ReferralDoctorId`. **Dikerjakan Registration**, disetujui `LAB-COORD-004` |
| `MstProcedure` | **Diperbarui** | Health Services Master Data | Tambah kolom klasifikasi `LabDiscipline`. **Dikerjakan Master Data**, disetujui `LAB-COORD-005` |
| `MstReferralInstitution` | **Baru** | Health Services Master Data | Data induk global. **Dikerjakan Master Data**, disetujui `LAB-COORD-004` |
| `MstReferralDoctor` | **Baru** | Health Services Master Data | Data induk global, tertaut ke instansinya. **Dikerjakan Master Data** |
| `MstAgeCategory` | Sudah ada | Health Services Master Data | Direferensikan oleh batas nilai |
| `LabOrder` | Diperbarui | Laboratorium | Tambah kolom `Discipline`. Kolom kesegeraan pindah ke `LabExamination` |
| `LabSpecimen` | Diperbarui | Laboratorium | Berubah makna menjadi wadah fisik; enam kolom pindah keluar |
| `LabExamination` | **Baru** | Laboratorium | Satuan yang ditagihkan dan kelak punya hasil. Membawa penanda cito dan duplo |
| `LabTransitionHistory` | Diperbarui | Laboratorium | Tambah `LabExaminationId` |
| `LabOrder` (2026-11-25, `S20-1`) | **Diperbarui** — usulan | Laboratorium | Tambah `PlannedDate` (`date`, wajib) — data dictionary bagian 24 |
| `LabOrderReschedule` | **Baru** — usulan | Laboratorium | Riwayat jadwal ulang per pesanan — data dictionary bagian 24 |
| `McuOrderComponent` | Sudah ada — **baca** | MCU | Hanya `LabOrderId` dibaca |
| `LabValueBound` | **Baru** | Laboratorium | Batas nilai per pemeriksaan, jenis kelamin, dan kelompok umur |
| `LabValueOption` | **Baru** | Laboratorium | Pilihan sah untuk hasil berbentuk pilihan |
| `LabValueBoundChangeRequest` | **Baru** | Laboratorium | Pengajuan perubahan batas kritis |
| `LabValueBoundHistory` | **Baru** | Laboratorium | Riwayat perubahan batas nilai |
| `MstLabRejectionReason` | Sudah ada | Laboratorium | Tidak berubah |
| `CliClinicalMilestoneFact` | Sudah ada | Clinical Billing Integration | Diterbitkan modul ini, dimiliki integrasi Billing |
| `BilChargeLine` | Sudah ada | Billing dan Kasir | **Tidak** disentuh modul ini |

---

## Entity yang sengaja tidak ada pada ERD ini

| Entity | Alasan |
|---|---|
| Tabel hasil pemeriksaan | Slice hasil terblokir `LAB-SIGN-001` |
| Tabel pemberitahuan | `LAB-DEC-016` menetapkannya milik platform |
| Tabel keutuhan dokumen rekam medis | Slice pendaftaran rekam medis terblokir `LAB-COORD-002` |
| Tabel daftar kerja | Daftar kerja diturunkan, tidak disimpan |

## Amandemen 2026-11-27 — Penyerahan hasil kepada pasien (`S17-1`)

| Perubahan | Isi |
|---|---|
| Aggregate baru | `AGG-LAB-04` Dokumen Hasil untuk Pasien — `LabPatientResultDocument`, `LabPatientResultVersion`, `LabPatientResultApproval`, `LabPatientResultDelivery` (**New**, Laboratorium); merujuk `LabOrder` (baca) |
| Setelan baru | `LabPatientDeliveryActivation`, `LabSensitiveProcedure` (**New**, Laboratorium); yang kedua merujuk `MstProcedure` milik Master Data (baca) |
| Hubungan baca diperluas | Data induk pasien (`MstPatient`): **nomor WhatsApp** dan **tanggal lahir**, lewat kunjungan — nol tulis |
| Tidak berubah | Rekam Medis (`LAB-DC-057`): dokumen pasien **bukan** dokumen RM kedua |

ERD rinci: `data-dictionary.md` bagian 25. Arsitektur domain: `LAB-DA-001` A10.
