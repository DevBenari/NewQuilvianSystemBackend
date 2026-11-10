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
| `DebtorReferenceId` | `Guid` | Ya | — | bagian unique di atas | Master penjamin perusahaan milik **Administrator** (`MstCompanyGuarantor`) bertanda `IsEmployeeBenefitGuarantor = true` | — | Tidak | Penjamin internal yang ditutup, dipilih operator (`FIN-DES-104`). Diratifikasi oleh `FIN-DEC-227` (menutup `FIN-OQ-102`) |
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
| `MstCompanyGuarantor` | Sekurang-kurangnya satu penjamin perusahaan bertanda `IsEmployeeBenefitGuarantor = true` | `FIN-DEC-227` (Administrator) |

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

---

## 11. Perubahan Skema Revisi 21 — Penanda Penjamin Internal dan Tautan Pegawai HR

```yaml
last_changed_in: Revisi 21 (13 Oktober 2026)
status: approved
input_revision: 00-interview-decisions.md (FIN-DEC-227..FIN-DEC-230)
input_hash: 7d14ac2b8911f930
```

### 11.1 `MstCompanyGuarantor` (Diperbarui)

Model: `Areas/Administrator/MasterData/Models/MstCompanyGuarantor.cs`

| Kolom | Tipe | Wajib | Bawaan | Index | Sensitif | Keterangan |
|---|---|:---:|---|---|:---:|---|
| `IsEmployeeBenefitGuarantor` | `bool` | Ya | `false` | Index terfilter `WHERE "IsEmployeeBenefitGuarantor" = true AND "IsDelete" = false` | Tidak | Penanda penjamin internal benefit karyawan RS (`FIN-DEC-227`). Menutup `FIN-OQ-102` |

### 11.2 `MstPatientCompanyGuarantor` (Diperbarui)

Model: `Areas/HealthServices/PatientManagement/MasterData/Models/MstPatientCompanyGuarantor.cs`

| Kolom | Tipe | Wajib | Bawaan | Relasi | Sensitif | Keterangan |
|---|---|:---:|---|---|:---:|---|
| `EmployeeId` | `Guid?` | Bersyarat | `NULL` | FK → `MstEmployee.Id` | Tidak | Wajib terisi jika penjamin bertanda internal (`FIN-VAL-259`). NIK pasien wajib cocok dengan NIK pegawai untuk hubungan `SELF` (`FIN-DEC-228`) |
| `FamilyMemberId` | `Guid?` | Bersyarat | `NULL` | FK → `WfpFamilyMember.Id` | Tidak | Diisi bila pasien adalah anggota keluarga tanggungan. NIK wajib cocok bila ada; bila belum ada (anak/bayi baru lahir), nama lengkap dan tanggal lahir wajib cocok dengan HR (`FIN-DEC-228`) |
| `BenefitRelationship` | `string(50)?` | Bersyarat | `NULL` | — | Tidak | Hubungan benefit hasil penentuan sistem (`SELF`, `SPOUSE`, `CHILD`, `PARENT`, `OTHER`). Klien dilarang mengirim manual (`FIN-VAL-260`) |

### 11.3 `RegPatientEncounterGuarantor` (Diperbarui)

Model: `Areas/HealthServices/RegistrationManagement/Models/RegPatientEncounterGuarantor.cs`
(Didukung migrasi `20261013055825_AddEmployeeLinkToPatientCompanyGuarantor`)

| Kolom | Tipe | Wajib | Bawaan | Sensitif | Keterangan |
|---|---|:---:|---|:---:|---|
| `EmployeeIdSnapshot` | `Guid?` | Tidak | `NULL` | Tidak | Snapshot ID pegawai HR saat penjamin ditautkan ke kunjungan |
| `FamilyMemberIdSnapshot` | `Guid?` | Tidak | `NULL` | Tidak | Snapshot ID anggota keluarga HR saat kunjungan didaftarkan |
| `BenefitRelationshipSnapshot` | `string(50)?` | Tidak | `NULL` | Tidak | Snapshot hubungan benefit saat kunjungan didaftarkan |


## 12. Perubahan Skema Revisi 22 — Catatan Hasil Sinkronisasi Kartu Benefit RS

```yaml
last_changed_in: Revisi 22 (3 November 2026)
status: approved   # 2026-11-03, pemilik (pengguna sesi)
input_revision: 00-interview-decisions.md (FIN-DEC-231..257); 02-backend-architecture.md bagian V
input_hash: lihat blueprint-manifest.md (artifact_hashes_revision_22)
```

Seluruh tabel mewarisi `IdentityModel` (sepuluh kolom audit dan penandaan hapus/batal); kolom itu tidak diulang di bawah.

### 12.1 Status tabel dan pemilik

| Tabel | Status | Modul pemilik | Model |
|---|---|---|---|
| `public."PatEmployeeBenefitCardSync"` | **Baru** | Patient Management (`Pat`) | `Areas/HealthServices/PatientManagement/MasterData/Models/PatEmployeeBenefitCardSync.cs` |
| `public."TrxEmployeeBenefitEnrollment"` | Sudah ada | HR Benefit Management | `Areas/Corporate/HumanResource/BenefitManagement/Models/TrxEmployeeBenefitEnrollment.cs` |
| `public."MstPatientCompanyGuarantor"` | Sudah ada | Patient Management | `Areas/HealthServices/PatientManagement/MasterData/Models/MstPatientCompanyGuarantor.cs` |
| `public."MstPatient"` | Sudah ada | Patient Management | `Areas/HealthServices/PatientManagement/MasterData/Models/MstPatient.cs` |
| `public."MstCompanyGuarantor"` | Sudah ada | Administrator Master Data | `Areas/Administrator/MasterData/Models/MstCompanyGuarantor.cs` |
| `public."MstWorkforceProfile"`, `"MstEmployee"`, `"MstDoctor"` | Sudah ada | HR Workforce | `Areas/Corporate/HumanResource/MasterData/Workforce/Models/` |

**Tidak ada kolom baru pada tabel yang sudah ada.**

### 12.2 `PatEmployeeBenefitCardSync` (Baru)

Satu baris per enrollment benefit. Menyimpan hasil terakhir pemasangan kartu "Benefit Karyawan RS" ke pasien yang
terbukti sama dengan pemilik benefit (`FIN-DEC-237`, `239`, `250`, `251`, `253`).

| Kolom | Tipe | Wajib | Bawaan | Panjang | Index / unik | Relasi dan perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `uuid` | Ya | `Guid.NewGuid()` | — | PK | — | Tidak | |
| `EmployeeBenefitEnrollmentId` | `uuid` | Ya | — | — | **Unik** terfilter `"IsDelete" = false` | FK → `TrxEmployeeBenefitEnrollment.Id`, `Restrict` | Tidak | Satu catatan per enrollment; dasar idempotensi |
| `WorkforceProfileId` | `uuid` | Ya | — | — | Index | FK → `MstWorkforceProfile.Id`, `Restrict` | Tidak | Pemilik benefit, disalin dari enrollment saat catatan dibuat |
| `SyncStatus` | `varchar` | Ya | — | 30 | Index gabungan (`SyncStatus`, `NextAttemptAt`) | — | Tidak | `SYNCED`, `WAITING_PATIENT`, `ON_HOLD`, `FAILED_RETRYABLE`, `REVOKED` (check constraint) |
| `ReasonCode` | `varchar` | Bersyarat | `NULL` | 50 | — | — | Tidak | Wajib untuk `WAITING_PATIENT`, `ON_HOLD`, `FAILED_RETRYABLE`, `REVOKED`; kosong untuk `SYNCED`. Hanya kode, tidak pernah teks berisi data pribadi |
| `AttemptCount` | `integer` | Ya | `0` | — | — | — | Tidak | ≥ 0 (check constraint). Naik pada setiap percobaan |
| `LastAttemptAt` | `timestamptz` | Tidak | `NULL` | — | — | — | Tidak | UTC |
| `NextAttemptAt` | `timestamptz` | Tidak | `NULL` | — | Lihat index `SyncStatus` | — | Tidak | Diisi hanya untuk `WAITING_PATIENT` dan `FAILED_RETRYABLE` |
| `PatientId` | `uuid` | Bersyarat | `NULL` | — | Index terfilter `IS NOT NULL` | FK → `MstPatient.Id`, `Restrict` | **Ya** | Pasien yang kartunya dipasang. Wajib untuk `SYNCED` dan `REVOKED`. Bersama `WorkforceProfileId` menyatakan bahwa seorang pegawai adalah pasien RS; **tidak boleh** masuk log |
| `PatientCompanyGuarantorId` | `uuid` | Bersyarat | `NULL` | — | **Unik** terfilter `IS NOT NULL AND "IsDelete" = false` | FK → `MstPatientCompanyGuarantor.Id`, `Restrict` | **Ya** | Kartu yang dipasang atau dipakai ulang. Wajib untuk `SYNCED` dan `REVOKED`. Dipakai validasi kunjungan untuk mengenali kartu otomatis (`FIN-DEC-256`) |
| `SyncedAt` | `timestamptz` | Tidak | `NULL` | — | — | — | Tidak | Terakhir kali berhasil |
| `RevokedAt` | `timestamptz` | Tidak | `NULL` | — | — | — | Tidak | Saat pencabutan terdeteksi |
| `LastTriggerSource` | `varchar` | Ya | — | 30 | — | — | Tidak | `ACTIVATION`, `SCHEDULER`, `MANUAL_RESYNC`, `CANCEL` (check constraint) |
| `LastTriggeredByUserId` | `uuid` | Tidak | `NULL` | — | — | Tanpa FK (pola kolom aktor `CreateBy`) | Tidak | Aktor manusia untuk `MANUAL_RESYNC`, akun layanan untuk `SCHEDULER` |
| `RowVersion` | `uuid` | Ya | `Guid.NewGuid()` | — | — | — | Tidak | Token konkurensi optimistik, diganti layanan pada setiap perubahan (pola `TrxEmployeeBenefitEnrollment.RowVersion`) |

Check constraint:

| Nama | Aturan |
|---|---|
| `CK_PatEmployeeBenefitCardSync_Status` | `SyncStatus` ∈ lima nilai di atas |
| `CK_PatEmployeeBenefitCardSync_TriggerSource` | `LastTriggerSource` ∈ empat nilai di atas |
| `CK_PatEmployeeBenefitCardSync_CardLink` | `SyncStatus` ∈ (`SYNCED`, `REVOKED`) ⇔ `PatientCompanyGuarantorId IS NOT NULL AND PatientId IS NOT NULL` |
| `CK_PatEmployeeBenefitCardSync_Reason` | `SyncStatus = 'SYNCED'` ⇔ `ReasonCode IS NULL` |
| `CK_PatEmployeeBenefitCardSync_AttemptCount` | `AttemptCount >= 0` |

Contoh isi (data fiktif, bukan data asli):

| Kolom | Rendy, berhasil | Pegawai X, ditahan |
|---|---|---|
| `SyncStatus` | `SYNCED` | `ON_HOLD` |
| `ReasonCode` | `NULL` | `MULTIPLE_MATCH` |
| `AttemptCount` | `1` | `2` |
| `PatientCompanyGuarantorId` | `5c1e…` | `NULL` |
| `LastTriggerSource` | `ACTIVATION` | `MANUAL_RESYNC` |

### 12.3 Kolom kunci tabel yang sudah ada dan dipakai aturan bisnis amandemen ini

| Tabel | Kolom | Dipakai untuk |
|---|---|---|
| `TrxEmployeeBenefitEnrollment` | `Id`, `WorkforceProfileId`, `EnrollmentNumber`, `EnrollmentStatus`, `EffectiveStartDate`, `EffectiveEndDate`, `SubmittedAt`, `SubmittedByUserId`, `ApprovedAt`, `ApprovedByUserId`, `ActivatedAt`, `ActivatedByUserId`, `CoverageLimitAmount`, `UsedAmount`, `RemainingAmount`, `RowVersion` | Ajukan atomik, Setujui+aktif, penjadwal, batas `ActivationCutoffUtc` (dibandingkan dengan `ActivatedAt`) |
| `MstPatientCompanyGuarantor` | `Id`, `PatientId`, `CompanyGuarantorId`, `EmployeeNumber`, `BenefitOwnerWorkforceProfileId`, `BenefitOwnerType`, `FamilyMemberId`, `BenefitRelationship`, `IsActive`, `IsPrimary`, `LastEligibilityReferenceNumber` | Pakai ulang kartu, penahanan, pencabutan, nomor referensi benefit (`LastEligibilityReferenceNumber` = nomor enrollment untuk kartu otomatis) |
| `MstPatient` | `Id`, `IdentityType`, `IdentityNumber` (Sensitif), `BirthDate` (Sensitif), `MergedToPatientId`, `IsActive` | Pencocokan otomatis dan cek ulang kunjungan |
| `MstCompanyGuarantor` | `Id`, `CompanyGuarantorCode`, `IsEmployeeBenefitGuarantor`, `IsActive`, `ContractStartDate`, `ContractEndDate` | Penjamin internal tunggal (`FIN-DEC-236`) |
| `MstWorkforceProfile` | `Id`, `UserType`, `IsActive` | Dokter tamu ditolak (`FIN-DEC-240`) |
| `MstEmployee`, `MstDoctor` | `WorkforceProfileId`, `IdentityNumber` (Sensitif), `BirthDate` (Sensitif), `IsActive`, `ResignDate`, `IsCancel` | Identitas pemilik untuk pencocokan |

**Koreksi catatan lama (bagian 11.2).** Baris `EmployeeId` di bagian 11.2 menyebut kolom itu wajib untuk penjamin
internal. Sejak migration `20261019100000_AddWorkforceBenefitOwnerIdentity`, kartu internal sah dengan
`BenefitOwnerWorkforceProfileId` saja (dokter tetap tidak punya `EmployeeId`), sesuai
`CK_MstPatientCompanyGuarantor_BenefitLink`. Bagian 11.2 tidak disunting; catatan ini yang berlaku.

### 12.4 Skema DDL (dokumentasi bentuk)

> **Peringatan.** Bagian ini dokumentasi bentuk yang diharapkan dari configuration EF Core, **bukan** skrip untuk
> dijalankan. Skema dibuat oleh migration EF yang ditulis tim pengembang.

```sql
CREATE TABLE public."PatEmployeeBenefitCardSync" (
    "Id"                          uuid         NOT NULL PRIMARY KEY,
    "EmployeeBenefitEnrollmentId" uuid         NOT NULL REFERENCES public."TrxEmployeeBenefitEnrollment"("Id") ON DELETE RESTRICT,
    "WorkforceProfileId"          uuid         NOT NULL REFERENCES public."MstWorkforceProfile"("Id") ON DELETE RESTRICT,
    "SyncStatus"                  varchar(30)  NOT NULL,
    "ReasonCode"                  varchar(50)  NULL,
    "AttemptCount"                integer      NOT NULL DEFAULT 0,
    "LastAttemptAt"               timestamptz  NULL,
    "NextAttemptAt"               timestamptz  NULL,
    "PatientId"                   uuid         NULL REFERENCES public."MstPatient"("Id") ON DELETE RESTRICT,
    "PatientCompanyGuarantorId"   uuid         NULL REFERENCES public."MstPatientCompanyGuarantor"("Id") ON DELETE RESTRICT,
    "SyncedAt"                    timestamptz  NULL,
    "RevokedAt"                   timestamptz  NULL,
    "LastTriggerSource"           varchar(30)  NOT NULL,
    "LastTriggeredByUserId"       uuid         NULL,
    "RowVersion"                  uuid         NOT NULL,
    -- sepuluh kolom IdentityModel tidak ditulis ulang
    CONSTRAINT "CK_PatEmployeeBenefitCardSync_Status"
        CHECK ("SyncStatus" IN ('SYNCED','WAITING_PATIENT','ON_HOLD','FAILED_RETRYABLE','REVOKED')),
    CONSTRAINT "CK_PatEmployeeBenefitCardSync_TriggerSource"
        CHECK ("LastTriggerSource" IN ('ACTIVATION','SCHEDULER','MANUAL_RESYNC','CANCEL')),
    CONSTRAINT "CK_PatEmployeeBenefitCardSync_CardLink"
        CHECK (("SyncStatus" IN ('SYNCED','REVOKED')) = ("PatientCompanyGuarantorId" IS NOT NULL AND "PatientId" IS NOT NULL)),
    CONSTRAINT "CK_PatEmployeeBenefitCardSync_Reason"
        CHECK (("SyncStatus" = 'SYNCED') = ("ReasonCode" IS NULL)),
    CONSTRAINT "CK_PatEmployeeBenefitCardSync_AttemptCount" CHECK ("AttemptCount" >= 0)
);

CREATE UNIQUE INDEX "UX_PatEmployeeBenefitCardSync_Enrollment"
    ON public."PatEmployeeBenefitCardSync" ("EmployeeBenefitEnrollmentId") WHERE "IsDelete" = false;
CREATE UNIQUE INDEX "UX_PatEmployeeBenefitCardSync_Card"
    ON public."PatEmployeeBenefitCardSync" ("PatientCompanyGuarantorId")
    WHERE "PatientCompanyGuarantorId" IS NOT NULL AND "IsDelete" = false;
CREATE INDEX "IX_PatEmployeeBenefitCardSync_Due"
    ON public."PatEmployeeBenefitCardSync" ("SyncStatus", "NextAttemptAt");
CREATE INDEX "IX_PatEmployeeBenefitCardSync_WorkforceProfile"
    ON public."PatEmployeeBenefitCardSync" ("WorkforceProfileId");
CREATE INDEX "IX_PatEmployeeBenefitCardSync_Patient"
    ON public."PatEmployeeBenefitCardSync" ("PatientId") WHERE "PatientId" IS NOT NULL;
```


## 13. Perubahan Skema Revisi 23 — Penampung Data PO/GR dari BLM

```yaml
last_changed_in: Revisi 23 putaran 2 (9 November 2026)
status: approved   # 2026-11-09, pemilik (pengguna sesi), termasuk FIN-DES-141
input_revision: 00-interview-decisions.md (FIN-DEC-258..278); 02-backend-architecture.md bagian W
input_hash: lihat blueprint-manifest.md (input_hashes_revision_23)
```

Seluruh tabel mewarisi `IdentityModel` (sepuluh kolom audit dan penandaan hapus/batal); kolom itu tidak diulang di bawah.
Semua waktu disimpan UTC (`timestamptz`). Tidak ada kolom berisi data pasien di bagian ini. Kolom kuantitas satuan dasar
memakai `numeric(18,6)` supaya tidak ada pembulatan dini (`FIN-DEC-276`); kolom lama `numeric(18,2)` tidak diubah.

### 13.1 Status tabel dan pemilik

| Tabel | Status | Modul pemilik | Model |
|---|---|---|---|
| `public."FinPurchaseOrder"` | **Diperbarui** | Finance Purchasing | `Areas/Corporate/FinanceManagement/Purchasing/Models/FinPurchaseOrder.cs` |
| `public."FinPurchaseOrderItem"` | **Diperbarui** | Finance Purchasing | `.../Purchasing/Models/FinPurchaseOrderItem.cs` |
| `public."FinGoodsReceipt"` | **Diperbarui** | Finance Purchasing | `.../Purchasing/Models/FinGoodsReceipt.cs` |
| `public."FinGoodsReceiptItem"` | **Diperbarui** | Finance Purchasing | `.../Purchasing/Models/FinGoodsReceiptItem.cs` |
| `public."FinPurchasingInboundMessage"` | **Baru** | Finance Purchasing | `.../Purchasing/Models/FinPurchasingInboundMessage.cs` |
| `public."FinPurchasingSupplierMap"` | **Baru** | Finance Purchasing | `.../Purchasing/Models/FinPurchasingSupplierMap.cs` |
| `public."FinPurchasingSyncConflict"` | **Baru** | Finance Purchasing | `.../Purchasing/Models/FinPurchasingSyncConflict.cs` |
| `public."FinPurchasingExternalCancellation"` | **Baru** | Finance Purchasing | `.../Purchasing/Models/FinPurchasingExternalCancellation.cs` |
| `public."MstSupplier"` | Sudah ada | Administrator Master Data | `Areas/Administrator/MasterData/Models/MstSupplier.cs`, kunci `Id`. **Tidak diubah** |

Tidak ada tabel pemetaan produk (`FIN-DEC-274`) dan tidak ada tabel pemetaan satuan (`FIN-DEC-276`).

### 13.2 `FinPurchaseOrder` (Diperbarui) — kolom baru dan kolom kunci

| Kolom | Tipe | Wajib | Bawaan | Panjang | Index / unik | Relasi dan perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `uuid` | Ya | — | — | PK | — | Tidak | Sudah ada |
| `PONumber` | `varchar` | Ya | — | 50 | Unik global terfilter `"IsDelete" = false` (tetap) | — | Tidak | Sudah ada. PO BLM memakai nomor asli BLM (`FIN-DEC-263`); tabrakan → `NUMBER_COLLISION` |
| `Status` | `varchar` | Ya | `'DRAFT'` | 30 | Index | — | Tidak | Sudah ada. Untuk PO BLM dihitung penghitung penerimaan (`02-backend-architecture.md` W.11) |
| `ApprovalTier` | `varchar` | Ya | — | 10 | — | — | Tidak | Sudah ada. Check diganti `IN ('TIER_1','TIER_2','EXTERNAL')`. Check baru `CK_FinPurchaseOrder_ExternalTier`: `EXTERNAL` **jika dan hanya jika** `SourceSystem = 'BLM'` |
| `SourceSystem` | `varchar` | Ya | `'QUILVIAN'` | 20 | Bagian index unik identitas | — | Tidak | **Baru.** `QUILVIAN` atau `BLM` (`CK_FinPurchaseOrder_SourceSystem`) |
| `ExternalPurchaseOrderId` | `uuid` | Bersyarat | `NULL` | — | Unik (`SourceSystem`, `ExternalPurchaseOrderId`) terfilter `IS NOT NULL AND "IsDelete" = false` | Tanpa FK | Tidak | **Baru.** `PurchaseOrderId` BLM. `CK_FinPurchaseOrder_ExternalIdentity`: terisi ⇔ `BLM` |
| `ExternalUpdatedAt` | `timestamptz` | Tidak | `NULL` | — | — | — | Tidak | **Baru.** Waktu perubahan terakhir menurut BLM; kiriman yang lebih lama ditandai `SUPERSEDED` |
| `LastSyncedAt` | `timestamptz` | Tidak | `NULL` | — | — | — | Tidak | **Baru.** Waktu Quilvian terakhir menerapkan kiriman |

PO BLM: `RequestedByUserId = Guid.Empty`, `ApprovalTier = 'EXTERNAL'`, `ApprovedAt` = waktu persetujuan BLM.

### 13.3 `FinPurchaseOrderItem` (Diperbarui)

| Kolom | Tipe | Wajib | Bawaan | Panjang | Index / unik | Relasi dan perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id`, `PurchaseOrderId` | `uuid` | Ya | — | — | PK, index | FK → `FinPurchaseOrder.Id` (tetap) | Tidak | Sudah ada |
| `ProductCategory`, `ProductName` | `varchar(100)`, `varchar(300)` | Ya | — | — | — | — | Tidak | Sudah ada. Untuk PO BLM: **snapshot** kategori dan nama saat PO diterima, tidak berubah walau master BLM berubah (`FIN-DEC-275`) |
| `Unit` | `varchar(30)` | Ya | — | — | — | — | Tidak | Sudah ada. Untuk PO BLM: nama satuan PO dari BLM, snapshot |
| `Quantity` | `numeric(18,2)` | Ya | — | — | — | — | Tidak | Sudah ada. Untuk PO BLM: kuantitas dalam satuan PO, snapshot |
| `ExternalPurchaseOrderItemId` | `uuid` | Bersyarat | `NULL` | — | Unik terfilter `IS NOT NULL AND "IsDelete" = false` | Tanpa FK | Tidak | **Baru.** `PurchaseOrderItemId` BLM; satu-satunya kunci pencocokan baris GR ke item PO (`FIN-DEC-275`) |
| `ExternalProductId` | `uuid` | Bersyarat | `NULL` | — | Index terfilter `IS NOT NULL` | Tanpa FK | Tidak | **Baru.** `ProductId` BLM |
| `ExternalProductCode` | `varchar` | Tidak | `NULL` | 100 | — | — | Tidak | **Baru.** `ProductExtCode` BLM bila ada. Panjang 100 adalah asumsi |
| `SourceUomCode` | `varchar` | Bersyarat | `NULL` | 30 | — | — | Tidak | **Baru.** Kode satuan PO asli dari BLM (contoh `BOX`) |
| `QuantityPerBaseUnit` | `numeric(18,6)` | Bersyarat | `NULL` | — | — | — | Tidak | **Baru.** Isi satu satuan PO dalam satuan dasar produk, snapshot dari `MstProductConversion.QuantityPerUnit` BLM (satuan dasar = 1). Check `> 0` bila terisi. **Tidak pernah diisi 1 sebagai tebakan** |
| `BaseUomCode` | `varchar` | Bersyarat | `NULL` | 30 | — | — | Tidak | **Baru.** Kode satuan dasar produk menurut BLM (`MstProduct.MeasurementId` → `MeasurementCode`, contoh `PCS`) |
| `BaseQuantity` | `numeric(18,6)` | Bersyarat | `NULL` | — | — | — | Tidak | **Baru.** `Quantity × QuantityPerBaseUnit`, dihitung saat PO diterapkan |

Check `CK_FinPurchaseOrderItem_BlmFields`: `ExternalPurchaseOrderItemId`, `ExternalProductId`, `SourceUomCode`,
`QuantityPerBaseUnit`, `BaseUomCode`, dan `BaseQuantity` **semuanya kosong** (item lama) **atau semuanya terisi** (item BLM).
`ExternalProductCode` dikecualikan karena boleh kosong di BLM.

**Contoh:** item BLM 1 BOX produk A → `Quantity = 1`, `Unit = "Box"`, `SourceUomCode = "BOX"`, `QuantityPerBaseUnit = 12`,
`BaseUomCode = "PCS"`, `BaseQuantity = 12`.

### 13.4 `FinGoodsReceipt` (Diperbarui)

| Kolom | Tipe | Wajib | Bawaan | Panjang | Index / unik | Relasi dan perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `uuid` | Ya | — | — | PK | — | Tidak | Sudah ada |
| `GRNumber` | `varchar` | Ya | — | 50 | Unik global terfilter (tetap) | — | Tidak | Sudah ada. Nomor asli BLM untuk GR BLM |
| `PurchaseOrderId` | `uuid` | Ya | — | — | Index | FK → `FinPurchaseOrder.Id` (tetap) | Tidak | Sudah ada. GR BLM hanya menunjuk PO BLM (`FIN-VAL-307`) |
| `Status` | `varchar` | Ya | `'RECEIVED'` | 20 | Index | — | Tidak | Sudah ada (`RECEIVED`, `CANCELLED`). GR BLM menjadi `CANCELLED` hanya lewat pemberitahuan/konfirmasi BLM |
| `SourceSystem` | `varchar` | Ya | `'QUILVIAN'` | 20 | Bagian index unik | — | Tidak | **Baru** |
| `ExternalReceiveOrderId` | `uuid` | Bersyarat | `NULL` | — | Unik (`SourceSystem`, `ExternalReceiveOrderId`) terfilter | Tanpa FK | Tidak | **Baru.** `ReceiveOrderId` BLM; check terisi ⇔ `BLM` |
| `ExternalUpdatedAt` | `timestamptz` | Tidak | `NULL` | — | — | — | Tidak | **Baru** |
| `LastSyncedAt` | `timestamptz` | Tidak | `NULL` | — | — | — | Tidak | **Baru** |

### 13.5 `FinGoodsReceiptItem` (Diperbarui)

| Kolom | Tipe | Wajib | Bawaan | Panjang | Index / unik | Relasi dan perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id`, `GoodsReceiptId`, `PurchaseOrderItemId` | `uuid` | Ya | — | — | PK, index | FK (tetap) | Tidak | Sudah ada. Untuk baris BLM, `PurchaseOrderItemId` ditentukan dari `ExternalPurchaseOrderItemId` kiriman, **bukan** dari nama produk |
| `ReceivedQuantity` | `numeric(18,2)` | Ya | — | — | — | — | Tidak | Sudah ada; arti untuk GR lama **tidak berubah**. Untuk baris BLM: jumlah **sah** dalam satuan PO (`AcceptedBaseQuantity / QuantityPerBaseUnit` item PO), dibulatkan 2 desimal **hanya untuk tampilan dan kompatibilitas** (`FIN-DES-140`) |
| `ExternalReceiveOrderItemId` | `uuid` | Bersyarat | `NULL` | — | Unik terfilter `IS NOT NULL AND "IsDelete" = false` | Tanpa FK | Tidak | **Baru.** `ReceivedOrderItemId` BLM |
| `ConditionCode` | `varchar` | Bersyarat | `NULL` | 30 | — | — | Tidak | **Baru.** `GOOD`, `DAMAGED`, `PENDING_INSPECTION`. Dari `Remarks` BLM: "Good Condition" → `GOOD`, "Damage" → `DAMAGED`, "Pending Inspection" → `PENDING_INSPECTION`; teks lain → kiriman ditolak |
| `SourceQuantity` | `numeric(18,6)` | Bersyarat | `NULL` | — | — | — | Tidak | **Baru.** Kuantitas asli penerimaan (`QtyReceive` BLM) dalam satuan asli |
| `SourceUomCode` | `varchar` | Bersyarat | `NULL` | 30 | — | — | Tidak | **Baru.** Satuan asli penerimaan (`Measure` BLM) |
| `QuantityPerBaseUnit` | `numeric(18,6)` | Bersyarat | `NULL` | — | — | — | Tidak | **Baru.** Faktor konversi yang dipakai: isi satu satuan asli dalam satuan dasar, snapshot dari BLM. Check `> 0` |
| `BaseUomCode` | `varchar` | Bersyarat | `NULL` | 30 | — | — | Tidak | **Baru.** Wajib sama dengan `BaseUomCode` item PO (diperiksa pengolah; bila beda → `CONVERSION_UNRESOLVED`) |
| `BaseQuantity` | `numeric(18,6)` | Bersyarat | `NULL` | — | — | — | Tidak | **Baru.** Hasil normalisasi: `SourceQuantity × QuantityPerBaseUnit` (jumlah fisik dalam satuan dasar) |
| `AcceptedBaseQuantity` | `numeric(18,6)` | Bersyarat | `NULL` | — | — | — | Tidak | **Baru.** Jumlah diterima sah dalam satuan dasar setelah pemeriksaan kondisi |

Check `CK_FinGoodsReceiptItem_BlmFields`: kedelapan kolom baru **semuanya kosong** (baris lama) **atau semuanya terisi**
(baris BLM). Check `CK_FinGoodsReceiptItem_ConditionCode`: nilai dalam daftar bila terisi. Check
`CK_FinGoodsReceiptItem_AcceptedBaseQuantity`: `0 <= AcceptedBaseQuantity <= BaseQuantity`, dan `AcceptedBaseQuantity = 0`
bila `ConditionCode <> 'GOOD'`. Check `CK_FinGoodsReceiptItem_QuantityPerBaseUnit`: `> 0` bila terisi.

**Contoh baris BLM:** GR-1 menerima 6 PCS produk A, kondisi Baik → `SourceQuantity = 6`, `SourceUomCode = "PCS"`,
`QuantityPerBaseUnit = 1`, `BaseUomCode = "PCS"`, `BaseQuantity = 6`, `AcceptedBaseQuantity = 6`, `ReceivedQuantity = 0.50`
(6 ÷ 12, dalam BOX, untuk tampilan). Perhitungan status memakai `6`, bukan `0.50`.

Rumus kemajuan dan status per item PO BLM ada di `02-backend-architecture.md` W.11.

### 13.6 `FinPurchasingInboundMessage` (Baru) — kotak masuk dan riwayat sinkronisasi

Satu baris per kiriman BLM, disimpan apa adanya sebelum diolah (`FIN-DES-132`).

| Kolom | Tipe | Wajib | Bawaan | Panjang | Index / unik | Relasi dan perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `uuid` | Ya | `Guid.NewGuid()` | — | PK | — | Tidak | |
| `SourceSystem` | `varchar` | Ya | `'BLM'` | 20 | Bagian unik (`SourceSystem`, `EventId`) | — | Tidak | Check `IN ('BLM')`; `QUILVIAN` tidak masuk kotak masuk |
| `EventId` | `varchar` | Ya | — | 100 | **Unik** (`SourceSystem`, `EventId`) | — | Tidak | Nomor kejadian dari BLM; dasar idempotensi |
| `EventType` | `varchar` | Ya | — | 50 | — | — | Tidak | Kode jenis kejadian dari BLM (contoh usulan: `PO_UPSERTED`, `GR_UPSERTED`, `PO_CANCELLED`, `GR_CANCELLED`). **Tidak** dibatasi check constraint sampai kontrak BLM pasti (`FIN-OQ-141`) |
| `DocumentType` | `varchar` | Ya | — | 30 | Index gabungan (`DocumentType`, `ExternalDocumentId`, `SourceOccurredAt`) | — | Tidak | `PURCHASE_ORDER` atau `GOODS_RECEIPT` (check) |
| `ExternalDocumentId` | `uuid` | Ya | — | — | Lihat index di atas | Tanpa FK | Tidak | `PurchaseOrderId` atau `ReceiveOrderId` BLM |
| `ExternalDocumentNumber` | `varchar` | Tidak | `NULL` | 50 | — | — | Tidak | Nomor dokumen BLM, untuk pencarian petugas |
| `SourceVersion` | `varchar` | Tidak | `NULL` | 50 | — | — | Tidak | Versi data dari BLM bila dikirim; format menunggu kontrak |
| `SourceOccurredAt` | `timestamptz` | Ya | — | — | Lihat index | — | Tidak | Waktu kejadian menurut BLM |
| `ReceivedAt` | `timestamptz` | Ya | `CURRENT_TIMESTAMP` | — | — | — | Tidak | Waktu Quilvian menerima |
| `PayloadJson` | `text` | Ya | — | — | — | — | **Ya** | Isi kiriman utuh. Dapat memuat identitas dan nama pegawai BLM; **MUST NOT** masuk custom logger dan tidak dipakai sebagai contoh berisi data asli |
| `PayloadHash` | `varchar` | Ya | — | 64 | — | — | Tidak | SHA-256 heksadesimal dari `PayloadJson`. `EventId` sama dengan hash berbeda → konflik `PAYLOAD_MISMATCH` |
| `ProcessingStatus` | `varchar` | Ya | `'RECEIVED'` | 30 | Index gabungan (`ProcessingStatus`, `NextAttemptAt`) | — | Tidak | `RECEIVED`, `APPLIED`, `WAITING_MAPPING`, `CONFLICT`, `REJECTED`, `SUPERSEDED` (check). Transisi: `contracts/state-transition-matrix.md` bagian W |
| `AttemptCount` | `integer` | Ya | `0` | — | — | — | Tidak | ≥ 0 (check) |
| `LastAttemptAt` | `timestamptz` | Tidak | `NULL` | — | — | — | Tidak | |
| `NextAttemptAt` | `timestamptz` | Tidak | `NULL` | — | Lihat index | — | Tidak | Hanya untuk `WAITING_MAPPING` (pemetaan supplier belum ada) |
| `LastErrorCode` | `varchar` | Tidak | `NULL` | 50 | — | — | Tidak | Kode saja (contoh `SUPPLIER_NOT_MAPPED`, `PO_NOT_FOUND`, `CONVERSION_MISSING`, `BASE_UOM_MISMATCH`), tanpa teks berisi data |
| `AppliedPurchaseOrderId` | `uuid` | Tidak | `NULL` | — | Index terfilter `IS NOT NULL` | FK → `FinPurchaseOrder.Id`, `Restrict` | Tidak | Terisi saat `APPLIED` untuk kiriman PO |
| `AppliedGoodsReceiptId` | `uuid` | Tidak | `NULL` | — | Index terfilter `IS NOT NULL` | FK → `FinGoodsReceipt.Id`, `Restrict` | Tidak | Terisi saat `APPLIED` untuk kiriman GR |
| `RowVersion` | `uuid` | Ya | `Guid.NewGuid()` | — | Concurrency token | — | Tidak | Mencegah dua pengolah menerapkan kiriman yang sama bersamaan |

Check `CK_FinPurchasingInboundMessage_Applied`: `ProcessingStatus = 'APPLIED'` ⇒ tepat satu dari `AppliedPurchaseOrderId`
dan `AppliedGoodsReceiptId` terisi, sesuai `DocumentType`.

**Retensi.** Baris kotak masuk tidak dihapus (riwayat audit). Kebijakan pengarsipan menunggu volume nyata.

### 13.7 `FinPurchasingSupplierMap` (Baru)

| Kolom | Tipe | Wajib | Bawaan | Panjang | Index / unik | Relasi dan perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `uuid` | Ya | `Guid.NewGuid()` | — | PK | — | Tidak | |
| `SourceSystem` | `varchar` | Ya | `'BLM'` | 20 | Bagian unik | — | Tidak | Check `IN ('BLM')` |
| `ExternalSupplierId` | `uuid` | Ya | — | — | **Unik** (`SourceSystem`, `ExternalSupplierId`) terfilter `"IsActive" = true AND "IsDelete" = false` | Tanpa FK | Tidak | `SupplierId` BLM |
| `ExternalSupplierName` | `varchar` | Tidak | `NULL` | 300 | — | — | Tidak | Nama di BLM saat dipetakan, untuk membantu petugas memeriksa |
| `SupplierId` | `uuid` | Ya | — | — | Index | FK → `MstSupplier.Id`, `Restrict` | Tidak | Supplier V2. Boleh dirujuk lebih dari satu baris pemetaan |
| `IsActive` | `boolean` | Ya | `true` | — | Bagian filter unik | — | Tidak | Pemetaan lama dinonaktifkan, tidak ditimpa, supaya jejak tetap ada |
| `RowVersion` | `uuid` | Ya | `Guid.NewGuid()` | — | Concurrency token | — | Tidak | |

### 13.8 (Dihapus pada putaran 2)

`FinPurchasingUomMap` tidak dibuat (`FIN-DEC-276`). Nomor bagian dipertahankan agar rujukan lain tidak bergeser.

### 13.9 `FinPurchasingSyncConflict` (Baru)

| Kolom | Tipe | Wajib | Bawaan | Panjang | Index / unik | Relasi dan perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `uuid` | Ya | `Guid.NewGuid()` | — | PK | — | Tidak | |
| `InboundMessageId` | `uuid` | Ya | — | — | **Unik** (`InboundMessageId`, `ConflictType`) terfilter `"Status" = 'OPEN' AND "IsDelete" = false` | FK → `FinPurchasingInboundMessage.Id`, `Restrict` | Tidak | Kiriman yang memicu konflik |
| `ConflictType` | `varchar` | Ya | — | 50 | Index (`Status`, `ConflictType`) | — | Tidak | `GR_CANCELLED_WITH_DEPENDENTS`, `PO_CANCELLED_WITH_DEPENDENTS`, `NUMBER_COLLISION`, `PAYLOAD_MISMATCH`, `CONVERSION_UNRESOLVED` (check). `CONVERSION_UNRESOLVED`: isi per satuan tidak ada, tidak sah (≤ 0), atau satuan dasar GR berbeda dari satuan dasar item PO (`FIN-DEC-276`) |
| `PurchaseOrderId` | `uuid` | Tidak | `NULL` | — | Index terfilter | FK → `FinPurchaseOrder.Id`, `Restrict` | Tidak | |
| `GoodsReceiptId` | `uuid` | Tidak | `NULL` | — | Index terfilter | FK → `FinGoodsReceipt.Id`, `Restrict` | Tidak | |
| `DependentDocumentType` | `varchar` | Tidak | `NULL` | 30 | — | — | Tidak | `INVOICE_EXCHANGE`, `PURCHASING_INVOICE`, `SUPPLIER_PAYABLE`, `PAYMENT` (check bila terisi) |
| `DependentDocumentId` | `uuid` | Bersyarat | `NULL` | — | — | **Tanpa FK** (jenis beragam) | Tidak | Wajib bila `DependentDocumentType` terisi (check) |
| `Status` | `varchar` | Ya | `'OPEN'` | 20 | Lihat index | — | Tidak | `OPEN`, `RESOLVED` (check) |
| `ResolutionCode` | `varchar` | Bersyarat | `NULL` | 30 | — | — | Tidak | `ADJUSTMENT`, `REVERSAL`, `SUPPLIER_RETURN`, `SUPPLIER_CREDIT`, `NO_FINANCIAL_IMPACT` (check). Wajib bila `RESOLVED` |
| `ResolutionNote` | `varchar` | Tidak | `NULL` | 1000 | — | — | Tidak | Catatan petugas Finance; tanpa data pribadi |
| `ResolvedByUserId` | `uuid` | Bersyarat | `NULL` | — | — | — | Tidak | Wajib bila `RESOLVED` |
| `ResolvedAt` | `timestamptz` | Bersyarat | `NULL` | — | — | — | Tidak | Wajib bila `RESOLVED` |
| `RowVersion` | `uuid` | Ya | `Guid.NewGuid()` | — | Concurrency token | — | Tidak | |

Check `CK_FinPurchasingSyncConflict_Target`: minimal satu dari `PurchaseOrderId` dan `GoodsReceiptId` terisi.

### 13.10 `FinPurchasingExternalCancellation` (Baru)

| Kolom | Tipe | Wajib | Bawaan | Panjang | Index / unik | Relasi dan perilaku hapus | Sensitif | Keterangan |
|---|---|:---:|---|---|---|---|:---:|---|
| `Id` | `uuid` | Ya | `Guid.NewGuid()` | — | PK | — | Tidak | |
| `SourceSystem` | `varchar` | Ya | `'BLM'` | 20 | Bagian unik | — | Tidak | Check `IN ('BLM')` |
| `Direction` | `varchar` | Ya | — | 20 | — | — | Tidak | `OUTBOUND_REQUEST` (Quilvian meminta) atau `INBOUND_NOTICE` (BLM memberi tahu) (check) |
| `DocumentType` | `varchar` | Ya | — | 30 | Index (`DocumentType`, `ExternalDocumentId`) | — | Tidak | `PURCHASE_ORDER` atau `GOODS_RECEIPT` (check) |
| `ExternalDocumentId` | `uuid` | Ya | — | — | Lihat index | Tanpa FK | Tidak | `PurchaseOrderId` atau `ReceiveOrderId` BLM |
| `PurchaseOrderId` | `uuid` | Bersyarat | `NULL` | — | Index terfilter | FK → `FinPurchaseOrder.Id`, `Restrict` | Tidak | Wajib bila `DocumentType = 'PURCHASE_ORDER'` |
| `GoodsReceiptId` | `uuid` | Bersyarat | `NULL` | — | Index terfilter | FK → `FinGoodsReceipt.Id`, `Restrict` | Tidak | Wajib bila `DocumentType = 'GOODS_RECEIPT'` |
| `EventId` | `varchar` | Ya | — | 100 | **Unik** (`SourceSystem`, `Direction`, `EventId`) | — | Tidak | Untuk `OUTBOUND_REQUEST` dibuat Quilvian; untuk `INBOUND_NOTICE` dari BLM |
| `CategoryCode` | `varchar` | Ya | — | 50 | — | — | Tidak | Sejajar `PchHistoryCancelPurchaseOrder.CategoryCode` BLM. Panjang asumsi |
| `CategoryName` | `varchar` | Ya | — | 100 | — | — | Tidak | Panjang asumsi |
| `OtherCategoryText` | `varchar` | Tidak | `NULL` | 300 | — | — | Tidak | Diisi bila kategori "lainnya" |
| `Reason` | `varchar` | Ya | — | 1000 | — | — | Tidak | Alasan batal; tanpa data pribadi |
| `RequestedByUserId` | `uuid` | Bersyarat | `NULL` | — | — | — | Tidak | Pengguna Quilvian; wajib untuk `OUTBOUND_REQUEST` |
| `CancelledBy` | `varchar` | Tidak | `NULL` | 100 | — | — | **Ya** | Identitas pelaku menurut BLM (`CancelOrderById`). Tidak masuk custom logger |
| `CancelledAt` | `timestamptz` | Bersyarat | `NULL` | — | — | — | Tidak | Wajib bila `CONFIRMED` |
| `Status` | `varchar` | Ya | — | 20 | Index (`Status`, `NextAttemptAt`) | — | Tidak | `REQUESTED`, `SENT`, `CONFIRMED`, `REJECTED`, `FAILED` (check). `INBOUND_NOTICE` langsung `CONFIRMED` |
| `RejectionCode` | `varchar` | Tidak | `NULL` | 50 | — | — | Tidak | Kode penolakan BLM, atau `AP_DEPENDENCY` bila ditolak pemeriksaan Quilvian sendiri |
| `AttemptCount` | `integer` | Ya | `0` | — | — | — | Tidak | ≥ 0 |
| `NextAttemptAt` | `timestamptz` | Tidak | `NULL` | — | Lihat index | — | Tidak | Hanya untuk `REQUESTED`/`FAILED` |
| `InboundMessageId` | `uuid` | Tidak | `NULL` | — | — | FK → `FinPurchasingInboundMessage.Id`, `Restrict` | Tidak | Kiriman BLM yang mengonfirmasi atau memberi tahu pembatalan |
| `RowVersion` | `uuid` | Ya | `Guid.NewGuid()` | — | Concurrency token | — | Tidak | |
