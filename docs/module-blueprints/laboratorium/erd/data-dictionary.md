# Kamus Data — Modul Laboratorium

| Field | Value |
|---|---|
| Blueprint ID | `LAB-BP-001` |
| Revision | `2` |
| Status | `draft` |
| Scope | Slice `S1a`, `S2`, `S3`, `S7`, `S10`, `S11`, `S13a`, `S13b`, `S14`, `S15` |
| Backend SHA | `c87d9c0` |

Seluruh tabel mewarisi `IdentityModel`, sehingga memiliki kolom audit `CreateDateTime`,
`CreateBy`, `UpdateDateTime`, `UpdateBy`, `DeleteDateTime`, `DeleteBy`, `CancelDateTime`,
`CancelBy`, `IsCancel`, dan `IsDelete`. Kolom-kolom itu **tidak** diulang pada tabel di bawah.

Penghapusan bersifat **penandaan** melalui `IsDelete`, bukan penghapusan baris. Desain apa pun
tidak boleh mengandalkan baris benar-benar hilang dari tabel.

Kedalaman dokumentasi mengikuti status tabel: tabel `Baru` dan `Diperbarui` ditulis seluruh
kolomnya; tabel `Sudah ada` cukup kolom kuncinya ditambah rujukan ke berkas model.

---

## 1. `LabOrder` — `Diperbarui`

Berkas model: `Areas/HealthServices/LaboratoryManagement/Models/LabOrder.cs`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `EncounterId` | `Guid` | Ya | — | Index | FK ke `TrxPatientEncounter` | `Restrict` | Tidak | Kunjungan pasien tempat pesanan dibuat |
| `ProcedureId` | `Guid` | Ya | — | Index | FK ke `MstProcedure` | `Restrict` | Tidak | Pemeriksaan yang dipesan pertama; bukan lagi satu-satunya komponen |
| `OrderStatus` | `LabOrderStatus` | Ya | `Requested` | Index | — | — | Tidak | Enum disimpan `int` |
| `StatusBeforeHold` | `LabOrderStatus?` | Tidak | — | — | — | — | Tidak | Status sebelum ditahan, agar dapat dilanjutkan tanpa menebak |
| **`Discipline`** | `LabDiscipline` | Ya | — | Index | — | — | Tidak | **Baru.** Patologi Klinik, Patologi Anatomi, atau Mikrobiologi (`LAB-DEC-025`) |
| `RequestedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Waktu pesanan dibuat |
| `RequestedByUserId` | `Guid?` | Tidak | — | — | — | — | Tidak | Dokter pemesan |
| `CompletedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Waktu pesanan diselesaikan |
| `Version` | `int` | Ya | `0` | — | — | — | Tidak | Token konkurensi |

---

## 2. `TrxLabSpecimen` — `Diperbarui`

Berkas model: `Areas/HealthServices/LaboratoryManagement/Models/TrxLabSpecimen.cs`

Setelah `LAB-DEC-024`, tabel ini mewakili **wadah fisik**, bukan pemeriksaan.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `LabOrderId` | `Guid` | Ya | — | Index | FK ke `LabOrder` | `Restrict` | Tidak | Pesanan induk |
| `SpecimenBarcode` | `string(64)` | Ya | dibangkitkan server | **Unique** | — | — | Tidak | Label wadah. Tidak memuat identitas pasien |
| `SpecimenSequence` | `int` | Ya | — | Index bersama `LabOrderId` | — | — | Tidak | Nomor urut wadah dalam satu pesanan |
| `SpecimenDescription` | `string(200)?` | Tidak | — | — | — | — | Tidak | Keterangan wadah, misalnya jenis tabung |
| `SpecimenStatus` | `LabSpecimenStatus` | Ya | `Planned` | Index | — | — | Tidak | Enum disimpan `int` |
| `StatusBeforeHold` | `LabSpecimenStatus?` | Tidak | — | — | — | — | Tidak | Status sebelum ditahan |
| `CollectedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Waktu pengambilan |
| `CollectedByUserId` | `Guid?` | Tidak | — | — | — | — | Tidak | Petugas pengambil |
| `ReceivedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Waktu tiba di laboratorium |
| `ReceivedByUserId` | `Guid?` | Tidak | — | — | — | — | Tidak | Petugas penerima |
| `DecidedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Waktu keputusan layak atau tolak |
| `DecidedByUserId` | `Guid?` | Tidak | — | — | — | — | Tidak | Petugas pemutus |
| `RejectionReasonId` | `Guid?` | Tidak | — | Index | FK ke `MstLabRejectionReason` | `Restrict` | Tidak | Alasan penolakan terkendali |
| `RejectionReasonCode` | `string(50)?` | Tidak | — | — | — | — | Tidak | Salinan kode alasan saat kejadian |
| `RejectionNote` | `string(1000)?` | Tidak | — | — | — | — | **Ya** | Catatan penolakan; dapat memuat keterangan kondisi pasien |
| `SupersededSpecimenId` | `Guid?` | Tidak | — | Index | FK ke `TrxLabSpecimen` | `Restrict` | Tidak | Wadah yang digantikan saat ambil ulang |
| `RecollectionCause` | `LabRecollectionCause?` | Tidak | — | — | — | — | Tidak | Sebab ambil ulang; menentukan siapa menanggung biaya |
| `RecollectionReason` | `string(1000)?` | Tidak | — | — | — | — | **Ya** | Alasan ambil ulang |
| `RecollectionAuthorizedByUserId` | `Guid?` | Tidak | — | — | — | — | Tidak | Pemberi otorisasi ambil ulang |
| `RecollectionAuthorizedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Waktu otorisasi |
| `Version` | `int` | Ya | `0` | — | — | — | Tidak | Token konkurensi |

**Kolom yang dihapus dari tabel ini** dan pindah ke `LabExamination`: `ProcedureId`,
`ProcedureCodeSnapshot`, `ProcedureNameSnapshot`, `TariffId`, `TariffCodeSnapshot`,
`UnitPriceSnapshot`.

---

## 3. `LabExamination` — `Baru`

Berkas model: `Areas/HealthServices/LaboratoryManagement/Models/LabExamination.cs`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama. Pada pemindahan data lama, **memakai kembali** identitas sampel lama agar tautan tagihan tidak putus |
| `LabOrderId` | `Guid` | Ya | — | Index | FK ke `LabOrder` | `Restrict` | Tidak | Pesanan induk |
| `SpecimenId` | `Guid` | Ya | — | Index | FK ke `TrxLabSpecimen` | `Restrict` | Tidak | Wadah yang menopang pemeriksaan ini |
| `ProcedureId` | `Guid` | Ya | — | Index | FK ke `MstProcedure` | `Restrict` | Tidak | Jenis pemeriksaan. Wajib berpenanda `IsLaboratory` |
| `ProcedureCodeSnapshot` | `string(50)?` | Tidak | — | — | — | — | Tidak | Salinan kode saat kejadian |
| `ProcedureNameSnapshot` | `string(200)?` | Tidak | — | — | — | — | Tidak | Salinan nama saat kejadian |
| `TariffId` | `Guid?` | Tidak | — | Index | FK ke tarif Master Data | `Restrict` | Tidak | Tarif yang berlaku saat kejadian |
| `TariffCodeSnapshot` | `string(50)?` | Tidak | — | — | — | — | Tidak | Salinan kode tarif |
| `UnitPriceSnapshot` | `decimal(18,2)?` | Tidak | — | — | — | — | Tidak | Salinan harga. **Bukan** tagihan; Billing yang memutuskan |
| `ExaminationStatus` | `LabExaminationStatus` | Ya | `Ordered` | Index | — | — | Tidak | Enum disimpan `int` |
| `ChargeEligibleAt` | `DateTime?` | Tidak | — | Index | — | — | Tidak | Waktu pemeriksaan menjadi sah ditagihkan |
| **`Urgency`** | `LabExaminationUrgency` | Ya | `Routine` | Index | — | — | Tidak | **Dipindahkan dari `LabOrder`** oleh `LAB-DEC-026`. Biasa atau cito, **per pemeriksaan** |
| **`UrgencyMarkedAt`** | `DateTime?` | Tidak | — | — | — | — | Tidak | Kapan ditandai cito |
| **`UrgencyMarkedByUserId`** | `Guid?` | Tidak | — | — | — | — | Tidak | Dokter yang menandai |
| **`IsDuplo`** | `bool` | Ya | `false` | — | — | — | Tidak | **Baru** (`LAB-DEC-026`). Pemeriksaan dikerjakan ganda |
| `Version` | `int` | Ya | `0` | — | — | — | Tidak | Token konkurensi |

**Unik:** kombinasi `SpecimenId` + `ProcedureId` tidak boleh berulang. Satu wadah tidak boleh
menopang jenis pemeriksaan yang sama dua kali.

---

## 4. `TrxLabTransitionHistory` — `Diperbarui`

Berkas model: `Areas/HealthServices/LaboratoryManagement/Models/TrxLabTransitionHistory.cs`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `LabOrderId` | `Guid` | Ya | — | Index bersama `OccurredAt` | FK ke `LabOrder` | `Restrict` | Tidak | Pesanan yang bersangkutan |
| `LabSpecimenId` | `Guid?` | Tidak | — | Index | FK ke `TrxLabSpecimen` | `Restrict` | Tidak | Terisi bila yang berpindah adalah wadah |
| **`LabExaminationId`** | `Guid?` | Tidak | — | Index | FK ke `LabExamination` | `Restrict` | Tidak | **Baru.** Terisi bila yang berpindah adalah pemeriksaan |
| `EncounterId` | `Guid` | Ya | — | Index | FK ke `TrxPatientEncounter` | `Restrict` | Tidak | Kunjungan pasien |
| `Scope` | `LabTransitionScope` | Ya | — | — | — | — | Tidak | Objek yang berpindah. Nilai baru: `LabExamination` |
| `Action` | `string(100)` | Ya | — | — | — | — | Tidak | Nama tindakan, misalnya `Specimen.Accept` |
| `FromStatus` | `string(50)?` | Tidak | — | — | — | — | Tidak | Status asal |
| `ToStatus` | `string(50)` | Ya | — | — | — | — | Tidak | Status tujuan |
| `ReasonCode` | `string(50)?` | Tidak | — | — | — | — | Tidak | Kode alasan terkendali |
| `ReasonNote` | `string(1000)?` | Tidak | — | — | — | — | **Ya** | Catatan bebas; dapat memuat keterangan kondisi pasien |
| `ActorUserId` | `Guid` | Ya | — | — | — | — | Tidak | Pelaku tindakan |
| `OccurredAt` | `DateTime` | Ya | — | Index bersama `LabOrderId` | — | — | Tidak | Waktu kejadian |
| `CorrelationId` | `Guid?` | Tidak | — | — | — | — | Tidak | Penghubung satu rangkaian tindakan |

---

## 5. `LabValueBound` — `Baru`

Berkas model: `Areas/HealthServices/LaboratoryManagement/Models/LabValueBound.cs`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `ProcedureId` | `Guid` | Ya | — | Unique bersama `GenderScope` dan `AgeCategoryId` | FK ke `MstProcedure` | `Restrict` | Tidak | Jenis pemeriksaan |
| `ResultForm` | `LabResultForm` | Ya | `Numeric` | — | — | — | Tidak | Angka atau pilihan terbatas |
| `Unit` | `string(20)?` | Tidak | — | — | — | — | Tidak | Satuan hasil. **Wajib** bila bentuk angka |
| `NormalLow` | `decimal(18,4)?` | Tidak | — | — | — | — | Tidak | Batas normal bawah |
| `NormalHigh` | `decimal(18,4)?` | Tidak | — | — | — | — | Tidak | Batas normal atas |
| `CriticalLow` | `decimal(18,4)?` | Tidak | — | — | — | — | Tidak | Batas kritis bawah. Perubahannya memerlukan persetujuan klinis |
| `CriticalHigh` | `decimal(18,4)?` | Tidak | — | — | — | — | Tidak | Batas kritis atas. Perubahannya memerlukan persetujuan klinis |
| `GenderScope` | `LabGenderScope` | Ya | `All` | Unique bersama | — | — | Tidak | Semua, pria, atau wanita |
| `AgeCategoryId` | `Guid?` | Tidak | — | Unique bersama | FK ke `MstAgeCategory` | `Restrict` | Tidak | Kosong berarti berlaku untuk semua umur |
| `CitoTurnaroundMinutes` | `int?` | Tidak | — | — | — | — | Tidak | Batas waktu penyelesaian cito, dihitung dari wadah dinyatakan layak |
| `IsActive` | `bool` | Ya | `true` | — | — | — | Tidak | Penanda aktif |
| `SortOrder` | `int` | Ya | `0` | — | — | — | Tidak | Urutan tampil |

---

## 6. `LabValueOption` — `Baru`

Berkas model: `Areas/HealthServices/LaboratoryManagement/Models/LabValueOption.cs`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `ValueBoundId` | `Guid` | Ya | — | Unique bersama `OptionCode` | FK ke `LabValueBound` | `Cascade` | Tidak | Batas nilai induk |
| `OptionCode` | `string(20)` | Ya | — | Unique bersama | — | — | Tidak | Kode pilihan, misalnya `P3` |
| `OptionName` | `string(100)` | Ya | — | — | — | — | Tidak | Nama pilihan, misalnya `+3` |
| `IsOutOfReference` | `bool` | Ya | `false` | — | — | — | Tidak | Pilihan ini di luar nilai rujukan |
| `IsCritical` | `bool` | Ya | `false` | — | — | — | Tidak | Pilihan ini kritis. Perubahannya memerlukan persetujuan klinis |
| `SortOrder` | `int` | Ya | `0` | — | — | — | Tidak | Urutan tampil |

`Cascade` dipakai di sini — dan **hanya** di sini — karena pilihan tidak punya makna tanpa
batas nilai induknya, dan keduanya bukan data klinis transaksional.

---

## 7. `LabValueBoundChangeRequest` — `Baru`

Berkas model: `Areas/HealthServices/LaboratoryManagement/Models/LabValueBoundChangeRequest.cs`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `ValueBoundId` | `Guid` | Ya | — | Index | FK ke `LabValueBound` | `Restrict` | Tidak | Batas nilai yang diusulkan berubah |
| `RequestStatus` | `LabBoundChangeStatus` | Ya | `Submitted` | Index | — | — | Tidak | Diajukan, berlaku, ditolak, atau ditarik |
| `ProposedCriticalLow` | `decimal(18,4)?` | Tidak | — | — | — | — | Tidak | Usulan batas kritis bawah |
| `ProposedCriticalHigh` | `decimal(18,4)?` | Tidak | — | — | — | — | Tidak | Usulan batas kritis atas |
| `ProposedCriticalOptionCodes` | `string(500)?` | Tidak | — | — | — | — | Tidak | Usulan daftar pilihan kritis, dipisah koma |
| `RequestReason` | `string(1000)` | Ya | — | — | — | — | Tidak | Alasan pengajuan |
| `RequestedByUserId` | `Guid` | Ya | — | — | — | — | Tidak | Pengaju |
| `RequestedAt` | `DateTime` | Ya | — | — | — | — | Tidak | Waktu pengajuan |
| `DecidedByUserId` | `Guid?` | Tidak | — | — | — | — | Tidak | Pemutus dari pihak klinis |
| `DecidedAt` | `DateTime?` | Tidak | — | — | — | — | Tidak | Waktu keputusan |
| `DecisionNote` | `string(1000)?` | Tidak | — | — | — | — | Tidak | Catatan keputusan |

---

## 8. `LabValueBoundHistory` — `Baru`

Berkas model: `Areas/HealthServices/LaboratoryManagement/Models/LabValueBoundHistory.cs`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | Kunci utama |
| `ValueBoundId` | `Guid` | Ya | — | Index bersama `OccurredAt` | FK ke `LabValueBound` | `Restrict` | Tidak | Batas nilai yang berubah |
| `ChangedField` | `string(100)` | Ya | — | — | — | — | Tidak | Nama kolom yang berubah |
| `OldValue` | `string(200)?` | Tidak | — | — | — | — | Tidak | Nilai lama |
| `NewValue` | `string(200)?` | Tidak | — | — | — | — | Tidak | Nilai baru |
| `ActorUserId` | `Guid` | Ya | — | — | — | — | Tidak | Pelaku perubahan |
| `ApprovedByUserId` | `Guid?` | Tidak | — | — | — | — | Tidak | Penyetuju, terisi bila yang berubah batas kritis |
| `ChangeReason` | `string(1000)?` | Tidak | — | — | — | — | Tidak | Alasan perubahan |
| `OccurredAt` | `DateTime` | Ya | — | Index bersama | — | — | Tidak | Waktu perubahan |

---

## 9. `MstLabRejectionReason` — `Sudah ada`

Berkas model: `Areas/HealthServices/LaboratoryManagement/Models/MstLabRejectionReason.cs`

Hanya kolom kunci yang ditulis. Sumber lengkapnya adalah berkas model di atas.

| Kolom | Tipe | Wajib | Index | Sensitif | Keterangan |
|---|---|:---:|---|:---:|---|
| `Id` | `Guid` | Ya | PK | Tidak | Kunci utama |
| `ReasonCode` | `string(50)` | Ya | **Unique** dengan filter `IsDelete = false` | Tidak | Kode alasan |
| `IsInternalHospitalError` | `bool` | Ya | — | Tidak | Menentukan siapa menanggung biaya ambil ulang. **Terkunci** bagi pengelola Laboratorium |
| `RequiresNote` | `bool` | Ya | — | Tidak | Mewajibkan catatan saat dipakai. **Terkunci** bagi pengelola Laboratorium |
| `IsActive` | `bool` | Ya | — | Tidak | Penanda aktif |

---

## 9b. Tabel Milik Modul Lain yang Berubah

Keempat tabel di bawah **bukan milik Laboratorium**. Perubahannya dikerjakan modul pemiliknya,
disetujui `andryzainhome` dan `sukmagp` pada 2026-09-01 lewat `LAB-REQ-001`.

Yang didokumentasikan di sini **hanya kolom yang dipakai Laboratorium**. Bentuk lengkapnya
tetap kewenangan modul pemiliknya, dan boleh lebih kaya daripada ini.

### 9b.1 `MstProcedure` — `Diperbarui` oleh Master Data

Berkas model: `Areas/HealthServices/MasterData/Models/MstProcedure.cs`

| Kolom | Tipe | Wajib | Index | Sensitif | Keterangan |
|---|---|:---:|---|:---:|---|
| `Id` | `Guid` | Ya | PK | Tidak | Kunci utama |
| `ProcedureCode` | `string` | Ya | **Unique** | Tidak | Kode tindakan |
| `IsLaboratory` | `bool` | Ya | — | Tidak | Penyaring pertama katalog laboratorium |
| **`LabDiscipline`** | `LabDiscipline?` | Tidak | Index | Tidak | **Baru.** Patologi Klinik, Patologi Anatomi, atau Mikrobiologi. Hanya bermakna bila `IsLaboratory` bernilai benar |
| `IsCoveredByInsuranceDefault` | `bool` | Ya | — | Tidak | Penanda bawaan tercakup penjamin |

**Satu-satunya kolom yang ditambahkan Laboratorium.** Satuan hasil, batas nilai, dan jenis wadah
**tetap dilarang** masuk ke sini — seluruhnya berada di tabel milik Laboratorium (`LAB-DEC-036`).

### 9b.2 `MstReferralInstitution` — `Baru`, milik Master Data

Berkas model: `Areas/HealthServices/MasterData/Models/MstReferralInstitution.cs`

| Kolom | Tipe | Wajib | Bawaan | Index | Sensitif | Keterangan |
|---|---|:---:|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | Tidak | Kunci utama |
| `InstitutionCode` | `string(50)` | Ya | — | **Unique** | Tidak | Kode instansi perujuk |
| `InstitutionName` | `string(200)` | Ya | — | Index | Tidak | Nama klinik atau rumah sakit perujuk |
| `Address` | `string(500)?` | Tidak | — | — | Tidak | Alamat instansi |
| `PhoneNumber` | `string(50)?` | Tidak | — | — | Tidak | Telepon instansi |
| `IsActive` | `bool` | Ya | `true` | — | Tidak | Penanda aktif |

### 9b.3 `MstReferralDoctor` — `Baru`, milik Master Data

Berkas model: `Areas/HealthServices/MasterData/Models/MstReferralDoctor.cs`

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Sensitif | Keterangan |
|---|---|:---:|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | Tidak | Kunci utama |
| `ReferralInstitutionId` | `Guid` | Ya | — | Index | FK ke `MstReferralInstitution` | Tidak | Instansi tempat dokter berpraktik |
| `DoctorName` | `string(200)` | Ya | — | Index | — | Tidak | Nama dokter perujuk |
| `IsActive` | `bool` | Ya | `true` | — | — | Tidak | Penanda aktif |

**Kenapa dokter perujuk tidak memakai data induk dokter yang sudah ada.** Dokter pada
`master-data` adalah dokter **rumah sakit ini**. Dokter perujuk adalah dokter **di luar** rumah
sakit — ia tidak punya jadwal praktik, tidak menerima jasa medis, dan tidak dapat menjadi DPJP.
Menyatukan keduanya akan membuat daftar dokter internal tercemar nama dari luar.

### 9b.4 `TrxPatientEncounter` — `Diperbarui` oleh Registration Management

Berkas model: `Areas/HealthServices/RegistrationManagement/Models/TrxPatientEncounter.cs`

| Kolom | Tipe | Wajib | Index | Relasi | Sensitif | Keterangan |
|---|---|:---:|---|---|:---:|---|
| `Id` | `Guid` | Ya | PK | — | Tidak | Kunci utama |
| `IsWalkIn` | `bool` | Ya | — | — | Tidak | **Sudah ada.** Penanda pasien datang langsung |
| `RegistrationSource` | `EncounterRegistrationSource` | Ya | — | — | Tidak | **Sudah ada.** Bernilai `WalkIn` untuk pendaftaran dari laboratorium |
| `IsReferral` | `bool` | Ya | — | — | Tidak | **Sudah ada.** Penanda pasien rujukan |
| `ReferralNumber` | `string?` | Tidak | — | — | Tidak | **Sudah ada.** Nomor surat rujukan |
| **`ReferralInstitutionId`** | `Guid?` | Tidak | Index | FK ke `MstReferralInstitution` | Tidak | **Baru.** Instansi perujuk |
| **`ReferralDoctorId`** | `Guid?` | Tidak | Index | FK ke `MstReferralDoctor` | Tidak | **Baru.** Dokter perujuk |

**Yang wajib dipegang implementer Laboratorium.** Keempat tabel di atas **tidak boleh ditulis**
dari kode Laboratorium. Kolom perujuk diisi Registrasi lewat permintaan `INT-05`; Laboratorium
hanya mengirim penunjuknya dan membacanya kembali.

### 9b.5 Bentuk DDL — dokumentasi, bukan skrip

> Peringatan yang sama berlaku: ini **dokumentasi bentuk**, bukan skrip untuk dijalankan.
> Migration-nya dibuat modul pemiliknya, bukan Laboratorium.

```sql
-- Dikerjakan Master Data. Bukan skrip untuk dijalankan.
ALTER TABLE public."MstProcedure" ADD COLUMN "LabDiscipline" integer;  -- enum, boleh kosong
CREATE INDEX "IX_MstProcedure_LabDiscipline" ON public."MstProcedure" ("LabDiscipline");

CREATE TABLE public."MstReferralInstitution" (
    "Id"                uuid          NOT NULL,
    "InstitutionCode"   varchar(50)   NOT NULL,
    "InstitutionName"   varchar(200)  NOT NULL,
    "Address"           varchar(500),
    "PhoneNumber"       varchar(50),
    "IsActive"          boolean       NOT NULL DEFAULT true,
    -- kolom audit IdentityModel tidak ditulis ulang di sini
    CONSTRAINT "PK_MstReferralInstitution" PRIMARY KEY ("Id")
);
CREATE UNIQUE INDEX "IX_MstReferralInstitution_InstitutionCode"
    ON public."MstReferralInstitution" ("InstitutionCode") WHERE "IsDelete" = false;

CREATE TABLE public."MstReferralDoctor" (
    "Id"                      uuid          NOT NULL,
    "ReferralInstitutionId"   uuid          NOT NULL,
    "DoctorName"              varchar(200)  NOT NULL,
    "IsActive"                boolean       NOT NULL DEFAULT true,
    -- kolom audit IdentityModel tidak ditulis ulang di sini
    CONSTRAINT "PK_MstReferralDoctor" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_MstReferralDoctor_MstReferralInstitution_ReferralInstitutionId"
        FOREIGN KEY ("ReferralInstitutionId")
        REFERENCES public."MstReferralInstitution" ("Id") ON DELETE RESTRICT
);

-- Dikerjakan Registration Management. Bukan skrip untuk dijalankan.
ALTER TABLE public."TrxPatientEncounter" ADD COLUMN "ReferralInstitutionId" uuid;
ALTER TABLE public."TrxPatientEncounter" ADD COLUMN "ReferralDoctorId" uuid;
```

---

## 10. Kolom Sensitif

Kolom bertanda **Ya** pada tabel di atas:

| Tabel | Kolom |
|---|---|
| `TrxLabSpecimen` | `RejectionNote`, `RecollectionReason` |
| `TrxLabTransitionHistory` | `ReasonNote` |

Aturan yang berlaku bagi ketiganya:

- **Tidak boleh** masuk ke custom logger;
- **Tidak boleh** dipakai sebagai contoh berisi data asli di dokumentasi;
- perlu ditinjau kebutuhan penyamarannya pada DTO response yang dilihat pengguna non-klinis.

---

## 11. Bentuk DDL

> **Peringatan.** Basis data project ini dibentuk EF Core Migrations, bukan skrip SQL manual.
> DDL di bawah adalah **dokumentasi bentuk tabel**, bukan skrip yang dijalankan. Menjalankannya
> akan berbenturan dengan migration. Kolom audit `IdentityModel` tidak ditulis ulang di sini.

### 11.1 `LabExamination` — Baru

```sql
-- Bentuk tabel sebagaimana dihasilkan EF Core. Bukan skrip untuk dijalankan.
CREATE TABLE public."LabExamination" (
    "Id"                      uuid           NOT NULL,
    "LabOrderId"              uuid           NOT NULL,
    "SpecimenId"              uuid           NOT NULL,
    "ProcedureId"             uuid           NOT NULL,
    "ProcedureCodeSnapshot"   varchar(50),
    "ProcedureNameSnapshot"   varchar(200),
    "TariffId"                uuid,
    "TariffCodeSnapshot"      varchar(50),
    "UnitPriceSnapshot"       numeric(18,2),
    "ExaminationStatus"       integer        NOT NULL,  -- enum, HasConversion<int>
    "ChargeEligibleAt"        timestamp,
    "Urgency"                 integer        NOT NULL,  -- enum, LAB-DEC-026
    "UrgencyMarkedAt"         timestamp,
    "UrgencyMarkedByUserId"   uuid,
    "IsDuplo"                 boolean        NOT NULL DEFAULT false,
    "Version"                 integer        NOT NULL,
    -- kolom audit IdentityModel tidak ditulis ulang di sini

    CONSTRAINT "PK_LabExamination" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_LabExamination_LabOrder_LabOrderId"
        FOREIGN KEY ("LabOrderId") REFERENCES public."LabOrder" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_LabExamination_TrxLabSpecimen_SpecimenId"
        FOREIGN KEY ("SpecimenId") REFERENCES public."TrxLabSpecimen" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_LabExamination_MstProcedure_ProcedureId"
        FOREIGN KEY ("ProcedureId") REFERENCES public."MstProcedure" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_LabExamination_LabOrderId" ON public."LabExamination" ("LabOrderId");
CREATE INDEX "IX_LabExamination_ExaminationStatus" ON public."LabExamination" ("ExaminationStatus");
CREATE INDEX "IX_LabExamination_ChargeEligibleAt" ON public."LabExamination" ("ChargeEligibleAt");
CREATE INDEX "IX_LabExamination_Urgency" ON public."LabExamination" ("Urgency");
CREATE UNIQUE INDEX "IX_LabExamination_SpecimenId_ProcedureId"
    ON public."LabExamination" ("SpecimenId", "ProcedureId")
    WHERE "IsDelete" = false;
```

### 11.2 `LabValueBound` — Baru

```sql
-- Bentuk tabel sebagaimana dihasilkan EF Core. Bukan skrip untuk dijalankan.
CREATE TABLE public."LabValueBound" (
    "Id"                      uuid           NOT NULL,
    "ProcedureId"             uuid           NOT NULL,
    "ResultForm"              integer        NOT NULL,  -- enum, HasConversion<int>
    "Unit"                    varchar(20),
    "NormalLow"               numeric(18,4),
    "NormalHigh"              numeric(18,4),
    "CriticalLow"             numeric(18,4),
    "CriticalHigh"            numeric(18,4),
    "GenderScope"             integer        NOT NULL,  -- enum, HasConversion<int>
    "AgeCategoryId"           uuid,
    "CitoTurnaroundMinutes"   integer,
    "IsActive"                boolean        NOT NULL DEFAULT true,
    "SortOrder"               integer        NOT NULL DEFAULT 0,
    -- kolom audit IdentityModel tidak ditulis ulang di sini

    CONSTRAINT "PK_LabValueBound" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_LabValueBound_MstProcedure_ProcedureId"
        FOREIGN KEY ("ProcedureId") REFERENCES public."MstProcedure" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_LabValueBound_MstAgeCategory_AgeCategoryId"
        FOREIGN KEY ("AgeCategoryId") REFERENCES public."MstAgeCategory" ("Id") ON DELETE RESTRICT
);

CREATE UNIQUE INDEX "IX_LabValueBound_Procedure_Gender_AgeCategory"
    ON public."LabValueBound" ("ProcedureId", "GenderScope", "AgeCategoryId")
    WHERE "IsDelete" = false;
```

### 11.3 `LabValueOption` — Baru

```sql
-- Bentuk tabel sebagaimana dihasilkan EF Core. Bukan skrip untuk dijalankan.
CREATE TABLE public."LabValueOption" (
    "Id"                 uuid          NOT NULL,
    "ValueBoundId"       uuid          NOT NULL,
    "OptionCode"         varchar(20)   NOT NULL,
    "OptionName"         varchar(100)  NOT NULL,
    "IsOutOfReference"   boolean       NOT NULL DEFAULT false,
    "IsCritical"         boolean       NOT NULL DEFAULT false,
    "SortOrder"          integer       NOT NULL DEFAULT 0,
    -- kolom audit IdentityModel tidak ditulis ulang di sini

    CONSTRAINT "PK_LabValueOption" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_LabValueOption_LabValueBound_ValueBoundId"
        FOREIGN KEY ("ValueBoundId") REFERENCES public."LabValueBound" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX "IX_LabValueOption_ValueBoundId_OptionCode"
    ON public."LabValueOption" ("ValueBoundId", "OptionCode")
    WHERE "IsDelete" = false;
```

### 11.4 `LabValueBoundChangeRequest` — Baru

```sql
-- Bentuk tabel sebagaimana dihasilkan EF Core. Bukan skrip untuk dijalankan.
CREATE TABLE public."LabValueBoundChangeRequest" (
    "Id"                            uuid           NOT NULL,
    "ValueBoundId"                  uuid           NOT NULL,
    "RequestStatus"                 integer        NOT NULL,  -- enum, HasConversion<int>
    "ProposedCriticalLow"           numeric(18,4),
    "ProposedCriticalHigh"          numeric(18,4),
    "ProposedCriticalOptionCodes"   varchar(500),
    "RequestReason"                 varchar(1000)  NOT NULL,
    "RequestedByUserId"             uuid           NOT NULL,
    "RequestedAt"                   timestamp      NOT NULL,
    "DecidedByUserId"               uuid,
    "DecidedAt"                     timestamp,
    "DecisionNote"                  varchar(1000),
    -- kolom audit IdentityModel tidak ditulis ulang di sini

    CONSTRAINT "PK_LabValueBoundChangeRequest" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_LabValueBoundChangeRequest_LabValueBound_ValueBoundId"
        FOREIGN KEY ("ValueBoundId") REFERENCES public."LabValueBound" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_LabValueBoundChangeRequest_ValueBoundId" ON public."LabValueBoundChangeRequest" ("ValueBoundId");
CREATE INDEX "IX_LabValueBoundChangeRequest_RequestStatus" ON public."LabValueBoundChangeRequest" ("RequestStatus");
```

### 11.5 `LabValueBoundHistory` — Baru

```sql
-- Bentuk tabel sebagaimana dihasilkan EF Core. Bukan skrip untuk dijalankan.
CREATE TABLE public."LabValueBoundHistory" (
    "Id"                  uuid           NOT NULL,
    "ValueBoundId"        uuid           NOT NULL,
    "ChangedField"        varchar(100)   NOT NULL,
    "OldValue"            varchar(200),
    "NewValue"            varchar(200),
    "ActorUserId"         uuid           NOT NULL,
    "ApprovedByUserId"    uuid,
    "ChangeReason"        varchar(1000),
    "OccurredAt"          timestamp      NOT NULL,
    -- kolom audit IdentityModel tidak ditulis ulang di sini

    CONSTRAINT "PK_LabValueBoundHistory" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_LabValueBoundHistory_LabValueBound_ValueBoundId"
        FOREIGN KEY ("ValueBoundId") REFERENCES public."LabValueBound" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_LabValueBoundHistory_ValueBoundId_OccurredAt"
    ON public."LabValueBoundHistory" ("ValueBoundId", "OccurredAt");
```

### 11.6 `LabOrder` — Diperbarui

```sql
-- Hanya kolom yang ditambahkan. Bukan skrip untuk dijalankan.
-- LAB-DEC-026: kolom kesegeraan TIDAK lagi di sini, melainkan di LabExamination.
ALTER TABLE public."LabOrder" ADD COLUMN "Discipline" integer NOT NULL DEFAULT 1;  -- enum

CREATE INDEX "IX_LabOrder_Discipline" ON public."LabOrder" ("Discipline");
```

### 11.7 `TrxLabSpecimen` — Diperbarui

```sql
-- Perubahan struktur. Bukan skrip untuk dijalankan.
-- Kolom di bawah dihapus SETELAH datanya dipindahkan ke LabExamination.
ALTER TABLE public."TrxLabSpecimen" DROP COLUMN "ProcedureId";
ALTER TABLE public."TrxLabSpecimen" DROP COLUMN "ProcedureCodeSnapshot";
ALTER TABLE public."TrxLabSpecimen" DROP COLUMN "ProcedureNameSnapshot";
ALTER TABLE public."TrxLabSpecimen" DROP COLUMN "TariffId";
ALTER TABLE public."TrxLabSpecimen" DROP COLUMN "TariffCodeSnapshot";
ALTER TABLE public."TrxLabSpecimen" DROP COLUMN "UnitPriceSnapshot";
```

### 11.8 `TrxLabTransitionHistory` — Diperbarui

```sql
-- Hanya kolom yang ditambahkan. Bukan skrip untuk dijalankan.
ALTER TABLE public."TrxLabTransitionHistory" ADD COLUMN "LabExaminationId" uuid;

ALTER TABLE public."TrxLabTransitionHistory"
    ADD CONSTRAINT "FK_TrxLabTransitionHistory_LabExamination_LabExaminationId"
    FOREIGN KEY ("LabExaminationId") REFERENCES public."LabExamination" ("Id") ON DELETE RESTRICT;

CREATE INDEX "IX_TrxLabTransitionHistory_LabExaminationId"
    ON public."TrxLabTransitionHistory" ("LabExaminationId");
```

### 11.9 Tabel `Sudah ada` yang tidak berubah

| Tabel | Berkas configuration |
|---|---|
| `MstLabRejectionReason` | `Areas/HealthServices/LaboratoryManagement/Configurations/LaboratoryManagementConfigurations.cs` — utang teknis lokasi, lihat `02-backend-architecture.md` bagian 5 |
