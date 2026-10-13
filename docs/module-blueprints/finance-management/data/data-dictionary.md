# Kamus Data — Finance Management

| Field | Nilai |
|---|---|
| Blueprint | `FIN-BP-001` Finance Management, bentuk `SINGLE` |
| Status | `approved` — disetujui pemilik (Yasmin, 5 Oktober 2026) |
| Owner | Yasmin (Product/Domain Owner Finance) |
| `approved_by` / `approved_at` | Yasmin / 2026-10-05 |
| Berkas lahir pada | Revisi 17, 5 Oktober 2026 — amandemen `EPIC FIN-04` sisi Finance |
| Input revision | `00-interview-decisions.md` `FIN-DEC-162`..`179`; `02-backend-architecture.md` bagian O (`FIN-DES-099`..`104`) |
| Backend SHA | `46fa2a91` |
| Traceability | `FIN-DEC-165`, `166`, `168`, `170`, `176`, `177`, `178`, `179`; gerbang `evidence/24` |

> **Kamus tabel Finance yang lahir sebelum revisi 17 masih tinggal di `erd/data-dictionary.md`.**
> Folder `erd/` sudah dinyatakan **`RETIRED`** oleh `AGENTS.md` backend dan lokasi canonical-nya adalah
> berkas ini. Memindahkan isi kamus lama ke sini menyentuh tabel milik seluruh rumpun Finance, jadi ia
> **MUST** menjadi task tersendiri dengan approval pemilik — bukan efek samping amandemen `EPIC FIN-04`.
> Sampai task itu dikerjakan, dua berkas ini dibaca berdampingan: tabel revisi 17 di sini, tabel
> sebelumnya di `erd/`.

## Ketentuan yang berlaku untuk seluruh tabel

**Seluruh tabel mewarisi `IdentityModel`**, yang menyediakan sepuluh kolom audit:
`CreateDateTime`, `CreateBy`, `UpdateDateTime`, `UpdateBy`, `DeleteDateTime`, `DeleteBy`,
`CancelDateTime`, `CancelBy`, `IsCancel`, dan `IsDelete`. Kesepuluh kolom itu **tidak** diulang pada
tabel mana pun di bawah.

Konsekuensi yang **MUST** dipegang: penghapusan bersifat penandaan (`IsDelete`), bukan penghapusan
sungguhan. Desain **MUST NOT** mengandalkan baris benar-benar hilang dari tabel.

Kolom bertanda **Sensitif = Ya** **MUST NOT** masuk custom logger dan **MUST NOT** dipakai sebagai
contoh berisi data asli.

Seluruh tabel memakai schema `public`, nama tabel tunggal PascalCase, dan `DbSet` jamak.

---

## 1. `FinReceivableInstallmentPlan`

| Field | Nilai |
|---|---|
| Status | **`Baru`** |
| Modul pemilik | Finance / Receivable (prefix `Fin`) |
| Model | `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivableInstallmentPlan.cs` |
| Arti satu baris | Satu perjanjian pembayaran bertahap atas satu kartu piutang |

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `PlanNumber` | `string(50)` | Ya | — | **Unique** | — | — | Tidak | Nomor perjanjian. Dialokasikan service, pola `ANG-yyyyMMdd-<guid>` mengikuti yang berjalan di modul ini |
| `ReceivableId` | `Guid` | Ya | — | Index; **unique terfilter** untuk `Status IN ('MENUNGGU','DISETUJUI')` | FK → `FinReceivable.Id` | `Restrict` | Tidak | Kartu piutang yang diangsur. Satu piutang hanya boleh punya satu perjanjian aktif |
| `InstallmentCount` | `int` | Ya | — | — | — | — | Tidak | Jumlah angsuran. **MUST** lebih besar dari nol |
| `InstallmentAmount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Nominal tiap angsuran. **MUST** lebih besar dari nol |
| `TotalAgreedAmount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Total yang disepakati. **MUST** sama dengan `InstallmentCount × InstallmentAmount`, dan **MUST** sama dengan sisa piutang saat disetujui |
| `FirstDeductionPeriod` | `string(7)` | Ya | — | — | — | — | Tidak | Periode gaji pertama, bentuk `YYYY-MM`. Contoh `2026-11` |
| `Status` | `string(30)` | Ya | `MENUNGGU` | Index `(Status, RequestedAt)` | — | — | Tidak | `MENUNGGU`, `DISETUJUI`, `DITOLAK`, `SELESAI`, `DIBATALKAN`. Dibatasi `CK_FinReceivableInstallmentPlan_Status` |
| `AgreementDocumentPath` | `string(512)` | Tidak | — | — | — | — | **Ya** | Jalur berkas perjanjian yang ditandatangani. Memuat identitas pegawai |
| `RequestedBy` | `Guid` | Ya | — | — | Pengguna | — | Tidak | Staf Finance yang mengajukan |
| `RequestedAt` | `DateTimeOffset` | Ya | — | — | — | — | Tidak | `timestamp with time zone` |
| `ApprovedBy` | `Guid?` | Tidak | — | — | Pengguna | — | Tidak | **MUST NOT** sama dengan `RequestedBy` — `CK_FinReceivableInstallmentPlan_MakerChecker` |
| `ApprovedAt` | `DateTimeOffset?` | Tidak | — | — | — | — | Tidak | — |
| `RejectedBy` | `Guid?` | Tidak | — | — | Pengguna | — | Tidak | — |
| `RejectedAt` | `DateTimeOffset?` | Tidak | — | — | — | — | Tidak | — |
| `RejectionReason` | `string(500)` | Tidak | — | — | — | — | **Ya** | Wajib terisi bila ditolak. **MUST NOT** memuat keterangan medis |
| `CancelReason` | `string(500)` | Tidak | — | — | — | — | **Ya** | Wajib terisi bila dibatalkan |
| `Notes` | `string(500)` | Tidak | — | — | — | — | **Ya** | **MUST NOT** memuat keterangan medis |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | Penanda versi; dua petugas yang menyunting hampir bersamaan terdeteksi |

---

## 2. `FinReceivableInstallment`

| Field | Nilai |
|---|---|
| Status | **`Baru`** |
| Modul pemilik | Finance / Receivable |
| Model | `.../Receivable/Models/FinReceivableInstallment.cs` |
| Arti satu baris | Satu angsuran pada satu periode gaji |

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `PlanId` | `Guid` | Ya | — | Index; **unique** `(PlanId, InstallmentNumber)` | FK → `FinReceivableInstallmentPlan.Id` | `Restrict` | Tidak | Perjanjian pemiliknya |
| `InstallmentNumber` | `int` | Ya | — | bagian unique di atas | — | — | Tidak | Urutan angsuran, mulai 1 |
| `DeductionPeriod` | `string(7)` | Ya | — | Index `(DeductionPeriod, Status)` | — | — | Tidak | Periode gaji, bentuk `YYYY-MM` |
| `ScheduledAmount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Nominal yang dijadwalkan periode ini. **MUST** lebih besar dari nol |
| `CarriedOverAmount` | `decimal(18,2)` | Ya | `0` | — | — | — | Tidak | Tunggakan dari periode sebelumnya yang ikut dipotong (`FIN-DEC-168`). **MUST** nol atau lebih |
| `PaidAmount` | `decimal(18,2)` | Ya | `0` | — | — | — | Tidak | Yang benar-benar terpotong. Bertambah bertahap bila potongan **sebagian** (`FIN-DEC-179`) |
| `OutstandingAmount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | **MUST** sama dengan `ScheduledAmount + CarriedOverAmount − PaidAmount` — `CK_FinReceivableInstallment_Balance`. **MUST** nol atau lebih |
| `Status` | `string(30)` | Ya | `DIJADWALKAN` | bagian index di atas | — | — | Tidak | `DIJADWALKAN`, `TERBAYAR_SEBAGIAN`, `TERBAYAR`, `TERTUNGGAK`, `DIBATALKAN`. Dibatasi `CK_FinReceivableInstallment_Status` |
| `LastResultAt` | `DateTimeOffset?` | Tidak | — | — | — | — | Tidak | Kapan hasil potongan terakhir diterima |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | — |

---

## 3. `FinBenefitSettlement`

| Field | Nilai |
|---|---|
| Status | **`Baru`** |
| Modul pemilik | Finance / Receivable |
| Model | `.../Receivable/Models/FinBenefitSettlement.cs` |
| Arti satu baris | Satu penutupan berkala atas piutang porsi manfaat yang ditanggung rumah sakit, untuk satu periode dan satu penjamin internal |

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `SettlementNumber` | `string(50)` | Ya | — | **Unique** | — | — | Tidak | Pola `PLB-yyyyMMdd-<guid>`, dialokasikan service |
| `AccountingPeriodCode` | `string(7)` | Ya | — | **Unique terfilter** `(AccountingPeriodCode, DebtorReferenceId)` untuk `Status <> 'DIBATALKAN'` | — | — | Tidak | Periode yang ditutup, bentuk `YYYY-MM` |
| `DebtorReferenceId` | `Guid` | Ya | — | bagian unique di atas | Master penjamin milik **Administrator** — asuransi atau perusahaan | — | Tidak | Penjamin internal yang ditutup, dipilih operator (`FIN-DES-104`). Master mana yang dipakai dilacak `FIN-OQ-102` |
| `TotalAmount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Jumlah seluruh baris anaknya. **MUST** lebih besar dari nol |
| `ItemCount` | `int` | Ya | — | — | — | — | Tidak | Jumlah kartu piutang yang ditutup. **MUST** lebih besar dari nol |
| `Status` | `string(30)` | Ya | `DRAF` | Index `(Status, AccountingPeriodCode)` | — | — | Tidak | `DRAF`, `DITERBITKAN`, `DIBATALKAN`. Dibatasi `CK_FinBenefitSettlement_Status` |
| `PostedBy` | `Guid?` | Tidak | — | — | Pengguna | — | Tidak | Wajib terisi bila `DITERBITKAN` — `CK_FinBenefitSettlement_Posted` |
| `PostedAt` | `DateTimeOffset?` | Tidak | — | — | — | — | Tidak | Wajib terisi bila `DITERBITKAN` |
| `CancelReason` | `string(500)` | Tidak | — | — | — | — | Tidak | Wajib terisi bila dibatalkan |
| `AccountingEventId` | `Guid?` | Tidak | — | Index | FK → `FinAccountingEventOutbox.Id` | `Restrict` | Tidak | Kejadian yang dititipkan ke Accounting. **Tetap kosong** sampai `FIN-OQ-103` terjawab, dan itu bukan cacat |
| `Notes` | `string(500)` | Tidak | — | — | — | — | Tidak | — |
| `RowVersion` | `Guid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | — |

---

## 4. `FinBenefitSettlementItem`

| Field | Nilai |
|---|---|
| Status | **`Baru`** |
| Modul pemilik | Finance / Receivable |
| Model | `.../Receivable/Models/FinBenefitSettlementItem.cs` |
| Arti satu baris | Satu kartu piutang yang ditutup oleh sebuah pelunasan internal |

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `Guid.NewGuid()` | PK | — | — | Tidak | — |
| `SettlementId` | `Guid` | Ya | — | Index; **unique** `(SettlementId, ReceivableId)` | FK → `FinBenefitSettlement.Id` | `Restrict` | Tidak | Pelunasan pemiliknya |
| `ReceivableId` | `Guid` | Ya | — | **Unique terfilter** untuk induk yang `Status <> 'DIBATALKAN'` | FK → `FinReceivable.Id` | `Restrict` | Tidak | Satu kartu piutang **MUST NOT** ditutup dua kali |
| `Amount` | `decimal(18,2)` | Ya | — | — | — | — | Tidak | Nominal yang ditutup untuk kartu itu. **MUST** lebih besar dari nol — `CK_FinBenefitSettlementItem_Amount` |

---

## 5. Tabel yang diperbarui

### 5.1 `FinReceivableMovement`

| Field | Nilai |
|---|---|
| Status | **`Diperbarui`** |
| Modul pemilik | Finance / Receivable |
| Model | `.../Receivable/Models/FinReceivableMovement.cs` |
| Kolom yang berubah | **Nol kolom** |

Yang berubah hanya daftar nilai yang sah pada static class `FinReceivableMovementTypes`:

| Nilai | Status | Arti |
|---|---|---|
| `POTONGAN-GAJI` | **Baru** | Saldo piutang berkurang karena potongan gaji pegawai, penuh maupun sebagian |
| `PELUNASAN-INTERNAL` | **Baru** | Saldo piutang berkurang karena pelunasan internal porsi manfaat yang ditanggung rumah sakit |
| `PENGAKUAN`, `PEMBUKAAN-MIGRASI`, `ALOKASI-PENERIMAAN`, `PEMBALIKAN-ALOKASI`, `POTONGAN`, `PEMBALIKAN-POTONGAN`, `PENYESUAIAN`, `PENGHAPUSAN`, `PEMBAYARAN-LANGSUNG` | Sudah ada | Tidak disentuh |

**Nol migration untuk perubahan ini.** Diverifikasi pada source `46fa2a91`:
`FinReceivableMovementConfiguration` hanya memasang check constraint untuk `Balance` dan
`FundingSource`, **tidak** untuk `MovementType`. Bila kelak check constraint untuk `MovementType`
ditambahkan, kedua nilai baru ini **MUST** ikut didaftarkan.

Jangan tertukar: `POTONGAN` yang sudah ada berarti potongan **atas penerimaan uang**
(`FinReceiptDeduction`, misalnya PPh atau biaya bank), bukan potongan gaji.

---

## 6. Tabel yang dipakai tetapi tidak diubah

Didokumentasikan kolom kuncinya saja, sesuai kedalaman untuk status `Sudah ada`.

### 6.1 `FinReceivable`

Model: `.../Receivable/Models/FinReceivable.cs`. Modul pemilik: Finance / Receivable.

| Kolom kunci | Tipe | Dipakai amandemen ini untuk | Sensitif |
|---|---|---|:---:|
| `Id` | `Guid` | Induk perjanjian angsuran dan sasaran pelunasan internal | Tidak |
| `DebtorType` | `string(30)` | Membatasi perjanjian angsuran hanya untuk `EMPLOYEE_BENEFIT`, dan memilih porsi benefit lewat `PAYER` | Tidak |
| `DebtorReferenceId` | `Guid?` | Menyaring piutang porsi benefit menurut penjamin internal yang dipilih | Tidak |
| `BenefitOwnerId` | `Guid?` | Menghitung status bebas tanggungan per pegawai | **Ya** |
| `BenefitRelationship` | `string(30)?` | Ditampilkan pada layar; tidak dipakai aturan bisnis amandemen ini | **Ya** |
| `OutstandingAmount` | `decimal(18,2)` | Memvalidasi total perjanjian, dan dikurangi saat potongan maupun pelunasan internal | Tidak |
| `Status` | `string(30)` | `OUTSTANDING`, `PARTIAL`, `SETTLED`, `WRITTEN_OFF`, `CANCELLED` — menentukan kelayakan diangsur dan ditutup | Tidak |

### 6.2 Tabel lain yang dibaca atau ditulis tanpa perubahan bentuk

| Tabel | Dipakai untuk | Perubahan |
|---|---|---|
| `FinReceivableMovement` | Mencatat setiap perubahan saldo | Dua nilai jenis mutasi, nol kolom — bagian 5.1 |
| `FinReceivableWriteOff`, `FinReceivableAdjustment` | Penghapusan buku dan penyesuaian piutang karyawan (`S8`) | **Nol** — dipakai ulang apa adanya |
| `FinAccountingEventOutbox` | Menitipkan kejadian pelunasan internal ke Accounting | **Nol** pada amandemen ini; bentuk kejadiannya tertahan `FIN-OQ-103` |

---

## 7. Tabel milik modul lain yang dirujuk

Dirujuk lewat Id saja. **Tidak ada salinan master yang dibuat di Finance.**

| Tabel | Modul pemilik | Dirujuk oleh | Catatan |
|---|---|---|---|
| Profil pegawai (`MstEmployee`) | **HR** | `FinReceivable.BenefitOwnerId` | Memetakan NIP ke `BenefitOwnerId` saat impor dan validasi (`FIN-DEC-196`, `FIN-DEC-199`) |
| Enrollment benefit (`TrxEmployeeBenefitEnrollment`) | **HR** | Billing (Adapter) | Dibaca oleh Billing saat kalkulasi penjaminan invoice (`FIN-DEC-189`) |
| Input variabel penggajian (`TrxPayrollVariableInput`) | **HR** | Finance (`INT-FIN-HR-001`) | Ditulis Finance saat perjanjian cicilan disetujui (`FIN-DEC-190`) |
| Form bebas tanggungan (`TrxExitClearance`) | **HR** | Finance (`INT-HR-FIN-002`) | Membaca status `IsFinanceCleared` via API clearance Finance (`FIN-DEC-193`) |
| Penjamin Perusahaan (`MstCompanyGuarantor`) | **Patient Registration / Master** | `BilArHandoff.DebtorReferenceId` | Rumah Sakit terdaftar sebagai penjamin internal `CompanyGuarantor` ("RS Benefit") (`FIN-DEC-197`) |
| Kartu Pasien Penjamin (`MstPatientCompanyGuarantor`) | **Patient Registration** | `RegPatientEncounterGuarantor` | Menyimpan relasi keluarga dan tautan ke NIP/Pegawai HR (`FIN-DEC-198`) |

---

## 8. Perubahan Skema Antarmodul Terkait Amandemen Revisi 18

### 8.1 `BilArHandoff` (Billing Management)

Model: `Areas/HealthServices/BillingManagement/Billing/Models/BilArHandoff.cs`.

| Kolom | Tipe | Wajib | Bawaan | Index | Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `BenefitOwnerId` | `Guid?` | Tidak | `NULL` | Index `(BenefitOwnerId, DebtorType)` | FK → `MstEmployee.Id` (logis) | `SetNull` | **Ya** | ID profil pegawai pemilik benefit HR (`FIN-DEC-184`, `FIN-DEC-199`). Wajib terisi bila `DebtorType = "EMPLOYEE_BENEFIT"` |
| `BenefitRelationship` | `string(50)?` | Tidak | `NULL` | — | — | — | **Ya** | Hubungan keluarga: `"SELF"`, `"SPOUSE"`, `"CHILD"`, `"PARENT"`, `"OTHER"` (`FIN-DEC-184`, `FIN-DEC-198`) |

### 8.2 Struktur Berkas Impor Migrasi Saldo Lama Piutang Karyawan (`CSV/XLSX`)

| Kolom Berkas | Tipe | Wajib | Contoh Nilai | Validasi / Keterangan |
|---|---|:---:|---|---|
| `NomorKartuLama` | `string(50)` | Ya | `AR-OLD-2025-001` | Nomor rujukan kartu piutang lama |
| `NIP` | `string(50)` | Ya | `PEG-2021-0045` | **Wajib valid & aktif** di `MstEmployee.EmployeeNumber` (`FIN-VAL-246`, `FIN-DEC-196`) |
| `TanggalTransaksi` | `date` | Ya | `2025-12-15` | Tanggal timbulnya piutang di sistem lama |
| `NomorInvoiceLama` | `string(50)` | Tidak | `INV-2025-8891` | Nomor invoice layanan RS lama bila ada |
| `NomorKunjungan` | `string(50)` | Tidak | `ENC-2025-1102` | Nomor encounter lama bila ada |
| `OriginalAmount` | `decimal(18,2)` | Ya | `3500000.00` | Nilai awal tagihan tanggungan pegawai |
| `OutstandingAmount` | `decimal(18,2)` | Ya | `1500000.00` | Sisa saldo yang belum lunas per tanggal cutover |
| `HubunganKeluarga` | `string(30)` | Ya | `SPOUSE` | `"SELF"`, `"SPOUSE"`, `"CHILD"`, `"PARENT"`, `"OTHER"` |
| `Catatan` | `string(500)` | Tidak | `Sisa rawat inap anak` | Catatan penjelas historis |



---

## 9. Perubahan Skema Revisi 19 — Plafon Benefit per Orang, Tautan Kartu Pasien, dan Pilihan Pegawai

```yaml
last_changed_in: Revisi 19 (13 Oktober 2026)
status: draft
input_revision: 00-interview-decisions.md (FIN-DEC-204..FIN-DEC-210)
input_hash: 546cff956e88536f   # SHA-256 (16 karakter pertama) berkas ini sebelum amandemen Revisi 19
```

Seluruh tabel mewarisi `IdentityModel` (sepuluh kolom audit; lihat kepala dokumen). Tidak diulang di bawah.
Bagian DDL adalah **dokumentasi bentuk, bukan skrip yang dijalankan**; sumber kebenarannya file configuration EF Core.

### 9.1 Status tabel

| Tabel | Status | Modul pemilik | Kolom/aturan yang berubah |
|---|---|---|---|
| `HrdBenefitBalanceMovement` | **Baru** | HR / BenefitManagement | Seluruh kolom (9.2) |
| `TrxEmployeeBenefitEnrollment` | **Diperbarui** | HR / BenefitManagement | Nol kolom baru; dua check constraint dan satu indeks unik terfilter (9.3) |
| `TrxEmployeeBenefitDependent` | **Diperbarui** | HR / BenefitManagement | Nol kolom baru; dua check constraint dan satu indeks unik terfilter (9.3) |
| `MstPatientCompanyGuarantor` | **Diperbarui** | Patient Management / Master Data | `EmployeeId`, `FamilyMemberId`, `BenefitRelationship`; satu check constraint; satu indeks (9.4) |
| `RegPatientEncounterGuarantor` | **Diperbarui** | Registration | `EmployeeIdSnapshot`, `FamilyMemberIdSnapshot`, `BenefitRelationshipSnapshot` (9.5) |
| `BilArHandoff` | **Diperbarui** | Billing | `DeferredPatientAmount`; satu check constraint (9.6) |
| `FinReceivable` | **Diperbarui** | Finance / Receivable | `BenefitRelationship` diperlebar dari 30 ke 50 karakter (9.7) |
| `MstBenefitType`, `MstBenefitPlan`, `WfpFamilyMember`, `WfpDependent`, `MstEmployee` | Sudah ada | HR | Dipakai tanpa perubahan (9.8) |

### 9.2 `HrdBenefitBalanceMovement` (Baru)

Buku mutasi plafon. **Hanya menambah**; baris tidak pernah diubah atau dihapus (penghapusan penandaan `IsDelete` tidak dipakai).
Model: `Areas/Corporate/HumanResource/BenefitManagement/Models/HrdBenefitBalanceMovement.cs`.
Configuration: `Repositories/Configurations/Corporate/HumanResource/BenefitManagement/HrdBenefitBalanceMovementConfiguration.cs`.
Prefix `Hrd` = *Human Resource* (baris registry `Corporate / SelfServices — Human Resource — Hrd — ACTIVE / LEGACY`);
penerapannya pada folder `BenefitManagement` **diperiksa saat QBE preflight** (`QBE-MOD-002`, `QBE-MOD-003`).

| Kolom | Tipe | Wajib | Bawaan | Index / Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `NewGuid()` | PK | — | Tidak | |
| `EmployeeBenefitEnrollmentId` | `Guid` | Ya | — | FK → `TrxEmployeeBenefitEnrollment.Id`; indeks `(EmployeeBenefitEnrollmentId, OccurredAt)` | `Restrict` | Tidak | Enrollment pemilik saldo |
| `EmployeeBenefitDependentId` | `Guid?` | Tidak | `NULL` | FK → `TrxEmployeeBenefitDependent.Id` | `Restrict` | Tidak | Terisi bila orangnya tanggungan |
| `BenefitPersonKind` | `string(20)` | Ya | — | — | — | Tidak | `ENROLLMENT` atau `DEPENDENT` |
| `MovementType` | `string(10)` | Ya | — | — | — | Tidak | `USE` atau `RETURN` |
| `SourceHandoffKey` | `Guid` | Ya | — | **Unik** `(SourceHandoffKey, MovementType)` | — | Tidak | `HandoffKey` baris serah terima `PAYER`; kunci idempotensi |
| `InvoiceId` | `Guid` | Ya | — | Indeks | — | Tidak | Invoice Billing asal; rujukan logis, tanpa FK lintas area |
| `Amount` | `decimal(18,2)` | Ya | — | — | — | Tidak | Lebih dari 0 |
| `BalanceBefore` | `decimal(18,2)` | Ya | — | — | — | Tidak | Saldo sebelum mutasi |
| `BalanceAfter` | `decimal(18,2)` | Ya | — | — | — | Tidak | Saldo sesudah mutasi |
| `OccurredAt` | `DateTimeOffset` | Ya | `UtcNow` | Indeks (lihat atas) | — | Tidak | |
| `ReversesMovementId` | `Guid?` | Tidak | `NULL` | FK → dirinya sendiri; **unik** terfilter `WHERE ReversesMovementId IS NOT NULL` | `Restrict` | Tidak | Wajib pada `RETURN`; menunjuk `USE` yang dikembalikan; satu `USE` hanya sekali |
| `ReasonText` | `string(500)?` | Tidak | `NULL` | — | — | Tidak | Alasan pengembalian |
| `ActorUserId` | `Guid?` | Tidak | `NULL` | — | — | Tidak | Pengguna yang memicu finalisasi atau koreksi |

**Constraint (usulan):**

| Nama | Aturan |
|---|---|
| `CK_HrdBenefitBalanceMovement_Type` | `"MovementType" IN ('USE','RETURN')` |
| `CK_HrdBenefitBalanceMovement_Kind` | `"BenefitPersonKind" IN ('ENROLLMENT','DEPENDENT')` dan `DEPENDENT` ⇔ `EmployeeBenefitDependentId` terisi |
| `CK_HrdBenefitBalanceMovement_Amount` | `"Amount" > 0` |
| `CK_HrdBenefitBalanceMovement_Balance` | `USE`: `BalanceAfter = BalanceBefore − Amount`; `RETURN`: `BalanceAfter = BalanceBefore + Amount`; `BalanceAfter >= 0` |
| `CK_HrdBenefitBalanceMovement_Reverses` | `RETURN` ⇔ `ReversesMovementId` terisi |

```sql
-- DOKUMENTASI BENTUK, BUKAN SKRIP YANG DIJALANKAN
CREATE TABLE public."HrdBenefitBalanceMovement" (
  "Id" uuid PRIMARY KEY,
  "EmployeeBenefitEnrollmentId" uuid NOT NULL REFERENCES public."TrxEmployeeBenefitEnrollment"("Id"),
  "EmployeeBenefitDependentId" uuid NULL REFERENCES public."TrxEmployeeBenefitDependent"("Id"),
  "BenefitPersonKind" varchar(20) NOT NULL,
  "MovementType" varchar(10) NOT NULL,
  "SourceHandoffKey" uuid NOT NULL,
  "InvoiceId" uuid NOT NULL,
  "Amount" numeric(18,2) NOT NULL,
  "BalanceBefore" numeric(18,2) NOT NULL,
  "BalanceAfter" numeric(18,2) NOT NULL,
  "OccurredAt" timestamptz NOT NULL,
  "ReversesMovementId" uuid NULL REFERENCES public."HrdBenefitBalanceMovement"("Id"),
  "ReasonText" varchar(500) NULL,
  "ActorUserId" uuid NULL
  -- + kolom audit IdentityModel (tidak diulang) dan constraint di atas
);
CREATE UNIQUE INDEX "UX_HrdBenefitBalanceMovement_Key" ON public."HrdBenefitBalanceMovement"("SourceHandoffKey","MovementType");
CREATE UNIQUE INDEX "UX_HrdBenefitBalanceMovement_Reverses" ON public."HrdBenefitBalanceMovement"("ReversesMovementId") WHERE "ReversesMovementId" IS NOT NULL;
```

### 9.3 `TrxEmployeeBenefitEnrollment` dan `TrxEmployeeBenefitDependent` (Diperbarui)

Model: `Areas/Corporate/HumanResource/BenefitManagement/Models/`. **Nol kolom baru.** Kolom kunci yang dipakai aturan bisnis:

| Tabel | Kolom kunci | Dipakai untuk |
|---|---|---|
| Enrollment | `WorkforceProfileId`, `EmployeeId`, `BenefitPlanId`, `EnrollmentStatus`, `EffectiveStartDate`, `EffectiveEndDate`, `CoverageLimitAmount`, `UsedAmount`, `RemainingAmount`, `MaximumDependents`, `ActiveDependentCount`, `SubmittedByUserId`, `ApprovedByUserId` | Saldo pegawai sendiri; masa berlaku; maker-checker |
| Tanggungan | `EmployeeBenefitEnrollmentId`, `FamilyMemberId`, `DependentStatus`, `IsEligible`, `IsCovered`, `EffectiveStartDate`, `EffectiveEndDate`, `CoverageLimitAmount`, `UsedAmount`, `RemainingAmount`, `VerifiedAt`, `RelationshipType` | Saldo tanggungan; kelayakan |

| Perubahan | Rincian |
|---|---|
| Check constraint saldo (kedua tabel) | `"UsedAmount" >= 0 AND "RemainingAmount" >= 0`. **Peringatan:** bila tabel sudah berisi baris yang melanggarnya, migration gagal. Periksa dulu lewat kueri sebelum diterapkan |
| Indeks unik terfilter enrollment | `(WorkforceProfileId, BenefitPlanId)` `WHERE "EnrollmentStatus" IN ('Submitted','Approved','Active') AND "IsDelete" = false` — satu enrollment terbuka per pegawai per plan |
| Indeks unik terfilter tanggungan | `(EmployeeBenefitEnrollmentId, FamilyMemberId)` `WHERE "DependentStatus" IN ('Draft','Requested','Verified') AND "FamilyMemberId" IS NOT NULL AND "IsDelete" = false` — satu tanggungan terbuka per orang |
| Catatan | Aturan "satu enrollment **layanan RS** aktif per pegawai pada periode berimpit" lintas plan **tidak** dapat berupa indeks (butuh `MstBenefitType`); ditegakkan service (`FIN-VAL-249`) |

### 9.4 `MstPatientCompanyGuarantor` (Diperbarui)

Model: `Areas/HealthServices/PatientManagement/MasterData/Models/MstPatientCompanyGuarantor.cs`.

| Kolom | Tipe | Wajib | Bawaan | Index / Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|:---:|---|
| `EmployeeId` | `Guid?` | Tidak | `NULL` | Indeks; FK → `MstEmployee.Id` | `Restrict` | **Ya** | Terisi untuk kartu penjamin internal (`FIN-DEC-199`) |
| `FamilyMemberId` | `Guid?` | Tidak | `NULL` | FK → `WfpFamilyMember.Id` | `Restrict` | **Ya** | Terisi bila bukan pegawai sendiri |
| `BenefitRelationship` | `string(50)?` | Tidak | `NULL` | — | — | **Ya** | `SELF`/`SPOUSE`/`CHILD`/`PARENT`/`OTHER`; **diisi sistem** dari hasil kelayakan (`FIN-VAL-260`) |

`CK_MstPatientCompanyGuarantor_BenefitLink`: ketiga kolom kosong semua, **atau** `EmployeeId` dan `BenefitRelationship`
terisi dengan `SELF` ⇔ `FamilyMemberId` kosong. Kolom `EmployeeNumber` dan `EmployeeName` (teks) **tetap** sebagai label tampilan
dan diisi dari data HR untuk kartu internal. Bila preflight QBE menolak FK lintas area, ketiga kolom dipertahankan
sebagai `Guid` tanpa FK dan keabsahannya dijaga layanan kelayakan. Kartu lama tetap sah tanpa tautan; **tanpa backfill**
(selaras `FIN-DEC-186`).

### 9.5 `RegPatientEncounterGuarantor` (Diperbarui)

Model: `Areas/HealthServices/RegistrationManagement/Models/RegPatientEncounterGuarantor.cs`.
Salinan historis saat registrasi; **tanpa FK** (menjaga riwayat bila data HR berubah).

| Kolom | Tipe | Wajib | Bawaan | Index | Sensitif | Keterangan |
|---|---|:---:|---|---|:---:|---|
| `EmployeeIdSnapshot` | `Guid?` | Tidak | `NULL` | Indeks | **Ya** | Disalin dari kartu oleh `EncounterPaymentSourceService` |
| `FamilyMemberIdSnapshot` | `Guid?` | Tidak | `NULL` | — | **Ya** | Sama |
| `BenefitRelationshipSnapshot` | `string(50)?` | Tidak | `NULL` | — | **Ya** | Sama (`FIN-DEC-198`) |

### 9.6 `BilArHandoff` (Diperbarui)

Model: `Areas/HealthServices/BillingManagement/Billing/Models/BilArHandoff.cs`.

| Kolom | Tipe | Wajib | Bawaan | Index | Sensitif | Keterangan |
|---|---|:---:|---|---|:---:|---|
| `DeferredPatientAmount` | `decimal(18,2)` | Ya | `0` | — | Tidak | Bagian dari `Amount` yang berasal dari pilihan pegawai (`D`); hanya bermakna pada baris `EMPLOYEE_BENEFIT` |

`CK_BilArHandoff_Deferred`: `"DeferredPatientAmount" >= 0 AND "DeferredPatientAmount" <= "Amount" AND ("DebtorType" = 'EMPLOYEE_BENEFIT' OR "DeferredPatientAmount" = 0)`.
Migration ini **menyusul** migration `20261010022402_AddEmployeeBenefitColumnsToBilArHandoff` yang belum diterapkan.
Finance **tidak** membaca kolom ini; Finance hanya menyalin `Amount`.

### 9.7 `FinReceivable` (Diperbarui)

| Kolom | Perubahan |
|---|---|
| `BenefitRelationship` | `string(30)?` → `string(50)?` (diperlebar; nol kehilangan data). Menyamakan dengan `BilArHandoff.BenefitRelationship` (`FIN-INTEGRATION-1.9` `R.4.1` butir 4) |

### 9.8 Tabel yang dipakai tanpa perubahan

| Tabel | Modul pemilik | Kolom kunci yang dibaca | Dipakai untuk |
|---|---|---|---|
| `MstEmployee` | HR | `Id`, `EmployeeNumber`, `FullName`, `WorkforceProfileId`, `IsActive`, `IdentityNumber` | Pencarian pegawai; `BenefitOwnerId` |
| `WfpFamilyMember` | HR | `Id`, `WorkforceProfileId`, `Relationship`, `FullName`, `BirthDate`, `IdentityNumber`, `IsActive` | Identitas anggota keluarga; pemetaan hubungan |
| `WfpDependent` | HR | `FamilyMemberId`, `BenefitPlanId`, `DependentStatus`, `IsBenefitEligible` | **Tidak dipakai** sebagai sumber kelayakan; sumber kelayakan adalah `TrxEmployeeBenefitDependent` (`FIN-DEC-204`) |
| `MstBenefitPlan` | HR | `CoverageType`, `CoverageLimitAmount`, `MaximumDependents`, `BenefitTypeId` | Batas plafon dan hubungan yang boleh |
| `MstBenefitType` | HR | `BenefitCategory`, `IsClaimBased` | Penanda enrollment layanan RS (`FIN-OQ-109`) |

### 9.9 Rencana data master awal

| Master | Isi minimum | Sumber nilai |
|---|---|---|
| `MstBenefitType` | Sekurang-kurangnya satu jenis dengan `BenefitCategory = 'Medical'` dan `IsClaimBased = false` | Kebijakan benefit rumah sakit (pemilik HR) |
| `MstBenefitPlan` | Sekurang-kurangnya satu plan atas jenis itu dengan `CoverageType`, `CoverageLimitAmount` > 0, dan `MaximumDependents` | Kebijakan benefit rumah sakit (pemilik HR) |
| `MstCompanyGuarantor` | Satu penjamin internal "RS Benefit" | `FIN-OQ-102` (Administrator) |

Tanpa ketiganya, tidak ada pegawai yang dapat di-enroll dan seluruh kunjungan pegawai jatuh ke Pasien Umum.
Nilai batas plafon **MUST** berasal dari master plan, **MUST NOT** di-hardcode di controller maupun frontend.


---

## 10. Perubahan Skema Revisi 20 — Jembatan Manual Potong Gaji dan Persetujuan Pegawai

```yaml
last_changed_in: Revisi 20 (13 Oktober 2026)
status: draft
input_revision: 00-interview-decisions.md (FIN-DEC-215..FIN-DEC-222)
input_hash: e291f6c9218e8762   # SHA-256 (16 karakter pertama) berkas ini sebelum amandemen Revisi 20
```

Seluruh tabel mewarisi `IdentityModel` (tidak diulang). DDL di bawah adalah **dokumentasi bentuk, bukan skrip yang dijalankan**.

### 10.1 Status tabel

| Tabel | Status | Modul pemilik | Yang berubah |
|---|---|---|---|
| `FinPayrollDeductionBatch` | **Baru** | Finance / Receivable (`Fin`) | Seluruh kolom (10.2) |
| `FinPayrollDeductionBatchItem` | **Baru** | Finance / Receivable (`Fin`) | Seluruh kolom (10.3) |
| `MstPayrollDeductionCutover` | **Baru** | Finance / Master Data (`Mst`) | Seluruh kolom (10.4) |
| `BilArHandoff` | **Diperbarui** | Billing | `DeferredConsentGivenBy`; satu check constraint (10.5) |
| `FinReceivableMovement` | Sudah ada | Finance | Nol kolom; satu nilai jenis mutasi baru `PEMBALIKAN-POTONGAN-GAJI` (tanpa check constraint pada `MovementType`) |
| `FinTransactionProof` | Sudah ada | Finance / Collection | Nol kolom; satu nilai `ProofType` baru `LAPORAN-PENGGAJIAN` (kolom tanpa check constraint) |
| `FinReceivableInstallment` | Sudah ada | Finance | Nol kolom. Kolom kunci: `DeductionPeriod`, `ScheduledAmount`, `CarriedOverAmount`, `PaidAmount`, `OutstandingAmount`, `Status`, `LastPayrollPeriodId` (kunci idempotensi), `RowVersion` |
| `MstPayrollPeriod` | Sudah ada | HR | Dibaca: `Id`, `StartDate`, `PayrollPeriodStatus`, `IsActive` |

### 10.2 `FinPayrollDeductionBatch` (Baru)

Model: `Areas/Corporate/FinanceManagement/Receivable/Models/FinPayrollDeductionBatch.cs`.
Configuration: `Repositories/Configurations/Corporate/FinanceManagement/Receivable/FinPayrollDeductionBatchConfiguration.cs`.

| Kolom | Tipe | Wajib | Bawaan | Index / Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `NewGuid()` | PK | — | Tidak | |
| `BatchNumber` | `string(50)` | Ya | — | **Unik** | — | Tidak | Dialokasikan service lewat number-series atomik (`QBE-CODE-002`), bukan Count+1 |
| `DeductionPeriod` | `string(7)` | Ya | — | Indeks | — | Tidak | `YYYY-MM` |
| `PayrollPeriodId` | `Guid` | Ya | — | Rujukan logis ke `MstPayrollPeriod.Id` (lintas area, tanpa FK) | — | Tidak | Kunci idempotensi inti pencatatan |
| `BatchType` | `string(10)` | Ya | — | — | — | Tidak | `REGULER` atau `KOREKSI` |
| `Status` | `string(20)` | Ya | `DRAFT` | Indeks `(DeductionPeriod, Status)` | — | Tidak | `DRAFT`, `DIAJUKAN`, `DISAHKAN`, `DITOLAK`, `DIBATALKAN` |
| `ControlTotalAmount` | `decimal(18,2)` | Ya | — | — | — | Tidak | Total menurut laporan penggajian |
| `TotalDeductedAmount` | `decimal(18,2)` | Ya | 0 | — | — | Tidak | Jumlah nominal terpotong pada baris |
| `TotalRows`, `InvalidRows` | `int` | Ya | 0 | — | — | Tidak | |
| `PayrollReportNumber` | `string(100)?` | Saat diajukan | `NULL` | — | — | Tidak | |
| `ProofId` | `Guid?` | Saat diajukan | `NULL` | FK → `FinTransactionProof.Id` | `Restrict` | Tidak | Bukti `LAPORAN-PENGGAJIAN` |
| `CorrectionReason` | `string(500)?` | Bila `KOREKSI` | `NULL` | — | — | Tidak | |
| `SourceFileName` | `string(255)` | Ya | — | — | — | Tidak | Nama berkas unggahan |
| `SubmittedBy`, `SubmittedAt` | `Guid?`, `DateTimeOffset?` | — | `NULL` | — | — | Tidak | |
| `ApprovedBy`, `ApprovedAt` | `Guid?`, `DateTimeOffset?` | — | `NULL` | — | — | Tidak | |
| `RejectedBy`, `RejectedAt`, `RejectionReason` | `Guid?`, `DateTimeOffset?`, `string(500)?` | — | `NULL` | — | — | Tidak | |
| `RowVersion` | `Guid` | Ya | `NewGuid()` | Konkurensi | — | Tidak | |

| Constraint | Aturan |
|---|---|
| `CK_FinPayrollDeductionBatch_Type` | `"BatchType" IN ('REGULER','KOREKSI')` |
| `CK_FinPayrollDeductionBatch_Status` | `"Status" IN ('DRAFT','DIAJUKAN','DISAHKAN','DITOLAK','DIBATALKAN')` |
| `CK_FinPayrollDeductionBatch_MakerChecker` | `"ApprovedBy" IS NULL OR "ApprovedBy" <> "SubmittedBy"` |
| `CK_FinPayrollDeductionBatch_Correction` | `"BatchType" <> 'KOREKSI' OR "CorrectionReason" IS NOT NULL` |
| `CK_FinPayrollDeductionBatch_Period` | `"DeductionPeriod" ~ '^[0-9]{4}-(0[1-9]\|1[0-2])$'` (tanda `\|` hanya pelolosan tabel; di SQL ditulis `|`) |
| `UX_FinPayrollDeductionBatch_RegularPeriod` | Unik `(DeductionPeriod)` `WHERE "BatchType" = 'REGULER' AND "Status" IN ('DRAFT','DIAJUKAN','DISAHKAN') AND "IsDelete" = false` |

### 10.3 `FinPayrollDeductionBatchItem` (Baru)

Model: `.../Receivable/Models/FinPayrollDeductionBatchItem.cs`. Configuration sejajar 10.2.

| Kolom | Tipe | Wajib | Bawaan | Index / Relasi | Perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|:---:|---|
| `Id` | `Guid` | Ya | `NewGuid()` | PK | — | Tidak | |
| `BatchId` | `Guid` | Ya | — | FK → `FinPayrollDeductionBatch.Id`; unik `(BatchId, RowNumber)` | `Restrict` | Tidak | |
| `RowNumber` | `int` | Ya | — | — | — | Tidak | Nomor baris pada berkas |
| `InstallmentId` | `Guid?` | Tidak | `NULL` | FK → `FinReceivableInstallment.Id`; unik `(BatchId, InstallmentId)` `WHERE "InstallmentId" IS NOT NULL` | `Restrict` | Tidak | Kosong bila kode angsuran tidak ditemukan |
| `InstallmentCode` | `string(60)` | Ya | — | — | — | Tidak | Seperti tertulis di berkas |
| `EmployeeNumber` | `string(50)` | Ya | — | — | — | **Ya** | NIP seperti tertulis di berkas |
| `ResultStatus` | `string(10)` | Ya | — | — | — | Tidak | `BERHASIL`, `SEBAGIAN`, `GAGAL` |
| `DeductedAmount` | `decimal(18,2)` | Ya | — | — | — | Tidak | ≥ 0. Pada `KOREKSI`: nominal yang **benar** |
| `DueAmount` | `decimal(18,2)` | Ya | 0 | — | — | Tidak | Terutang saat validasi |
| `PreviousDeductedAmount` | `decimal(18,2)?` | Tidak | `NULL` | — | — | Tidak | Hanya `KOREKSI`: nominal yang tercatat sebelumnya |
| `DeltaAmount` | `decimal(18,2)?` | Tidak | `NULL` | — | — | Tidak | Hanya `KOREKSI`: benar − tercatat |
| `ValidationStatus` | `string(10)` | Ya | — | — | — | Tidak | `VALID` atau `INVALID` |
| `ValidationMessage` | `string(500)?` | Tidak | `NULL` | — | — | Tidak | Pesan baris tidak valid |
| `AppliedAt` | `DateTimeOffset?` | Tidak | `NULL` | — | — | Tidak | Waktu penerapan saat batch disahkan |

`CK_FinPayrollDeductionBatchItem_Result`: `"ResultStatus" IN ('BERHASIL','SEBAGIAN','GAGAL') AND "DeductedAmount" >= 0 AND ("ResultStatus" <> 'GAGAL' OR "DeductedAmount" = 0)`.

### 10.4 `MstPayrollDeductionCutover` (Baru)

Model: `Areas/Corporate/FinanceManagement/MasterData/Models/MstPayrollDeductionCutover.cs` (pola `MstDirectPaymentThreshold`).

| Kolom | Tipe | Wajib | Bawaan | Index | Sensitif | Keterangan |
|---|---|:---:|---|---|:---:|---|
| `Id` | `Guid` | Ya | `NewGuid()` | PK | Tidak | |
| `AutomaticFromPeriod` | `string(7)` | Ya | — | — | Tidak | `YYYY-MM`; periode pertama yang ditangani mesin |
| `ChangeReason` | `string(500)` | Ya | — | — | Tidak | |
| `IsActive` | `bool` | Ya | `true` | **Unik** terfilter `WHERE "IsActive" = true AND "IsDelete" = false` | Tidak | Hanya satu nilai aktif |
| `RowVersion` | `Guid` | Ya | `NewGuid()` | Konkurensi | Tidak | |

**Data master awal:** **kosong**. Tabel kosong berarti seluruh periode memakai jalur manual — itu keadaan yang benar sampai mesin payroll siap.

### 10.5 `BilArHandoff` (Diperbarui)

| Kolom | Tipe | Wajib | Bawaan | Sensitif | Keterangan |
|---|---|:---:|---|:---:|---|
| `DeferredConsentGivenBy` | `string(200)?` | Bersyarat | `NULL` | **Ya** | Nama pemberi persetujuan utang (`FIN-DEC-218`) |

`CK_BilArHandoff_Consent`: `"DeferredPatientAmount" = 0 OR "BenefitRelationship" = 'SELF' OR "DeferredConsentGivenBy" IS NOT NULL`.

### 10.6 Rencana data master awal

| Master | Isi minimum | Sumber nilai |
|---|---|---|
| `MstPayrollPeriod` (HR, sudah ada) | Satu baris per bulan yang akan dipotong, aktif | Kalender penggajian (pemilik HR), lewat `PayrollPeriodController` yang sudah ada |
| `MstPayrollDeductionCutover` | Kosong | Diisi pejabat Finance saat mesin payroll siap |
