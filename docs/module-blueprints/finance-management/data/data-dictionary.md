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

