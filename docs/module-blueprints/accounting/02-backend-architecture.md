# Accounting — Backend Architecture

| Field | Value |
|---|---|
| Blueprint ID | `ACC-BP-001` |
| Revision | `3` |
| Status | `draft` — approval adalah tindakan manusia, belum diberikan |
| Cakupan | MVP tulang punggung akuntansi (`ACC-DEC-009`) |
| Backend SHA | `aa837d784ff51cb2b889cf975ada3a204018f1f5` (branch `rizkiG`) |
| Frontend SHA | `fc49cc7714baa9a2c37ed6519fbaba5dffcbda99` (branch `RizkiV2`) — baseline **saat dokumen ini disusun**. Baseline blueprint kini `31a82c8` (`QuilvianIntegrationFrontend`); kutipan di bawah tetap berlaku, lihat `evidence/02-frontend-rebaseline-impact-scan.md` |
| Masukan | `00-interview-decisions.md@3`, `01-existing-capability-map.md@2` |
| Decision revision | `1.1` — `ACC-DEC-001` sampai `ACC-DEC-037` |
| Sumber konvensi | `AGENTS.md@aa837d7`, `backend-structure-rules.md` |

## Peringatan sebelum membaca

Dokumen ini **tidak memberi wewenang menulis kode**. Ia menetapkan bentuk target. Pembuatan
entity dan migration masih terblokir oleh `ACC-DEP-001` dan `ACC-DEP-002`; lihat
[05-prerequisite-readiness.md](05-prerequisite-readiness.md).

Seluruh nama kelas berawalan `Acc` di dokumen ini adalah **nama sementara**. Prefix penamaan
resmi belum terdaftar di registry kepemilikan modul. Yang sudah pasti adalah **bentuknya** —
kolom, kunci, relasi, dan aturannya. Bila lead mendaftarkan prefix lain, hanya namanya yang
berubah.

---

## 1. Keputusan gerbang: Accounting MVP diperlakukan sebagai kemampuan non-rumah-sakit

Skill penyusun blueprint mewajibkan bukti `requirement-completeness-gate` dan handoff
`hospital-domain-architect` **untuk kemampuan bisnis rumah sakit**. Keduanya belum dijalankan.
Karena itu klasifikasinya dinyatakan terbuka di sini, bukan dilewati diam-diam.

**Penilaian:** Accounting MVP diperlakukan sebagai kemampuan **korporat/keuangan umum**, bukan
kemampuan bisnis rumah sakit. Dasarnya:

| Uji | Hasil pada MVP |
|---|---|
| Memuat data pasien? | Tidak ada satu pun kolom pasien |
| Memuat isi klinis atau menyentuh keselamatan pasien? | Tidak |
| Melintasi bounded context lain? | Tidak — seluruh integrasi ditunda ke Phase 2 oleh `ACC-DEC-009` |
| Berdampak pada billing? | Tidak pada MVP; jalur Billing ada di Phase 2 |
| Apakah aturannya khas rumah sakit? | Tidak. Pembukuan berpasangan berlaku sama di industri mana pun |

Karena itu bounded context, batas aggregate, dan ownership pada dokumen ini ditetapkan di sini
dari bukti yang sudah disetujui — 37 keputusan `ACC-DEC-*` — sebagaimana diizinkan untuk
kemampuan non-rumah-sakit.

**Syarat yang mengikat penilaian ini:**

> Phase 2 Accounting **bukan** kemampuan non-rumah-sakit. Begitu modul ini menerima kejadian
> keuangan yang berasal dari tagihan pasien, ia melintasi bounded context Billing dan menyentuh
> data yang terikat pada kunjungan pasien. Sebelum Phase 2 dirancang,
> `requirement-completeness-gate` dan `hospital-domain-architect` **wajib** dijalankan lebih
> dahulu.

Satu hal lagi yang perlu dicatat jujur: folder tata kelola `docs/engineering/` dan `.codex/`
tidak ada di repository ini. `AGENTS.md`, yang menyatakan dirinya otoritatif untuk governance
level-repository, dapat dibaca dan diikuti. Yang tidak dapat dibaca hanyalah kontrak penamaan
QBE, dan konsekuensinya sudah ditangani sebagai `ACC-DEP-002` — penamaan memang sengaja tidak
dikunci di sini.

---

## 2. Bounded context dan ownership

Accounting adalah satu bounded context dengan tiga area di dalamnya. Pembagian mengikuti batas
aggregate, bukan sekadar pengelompokan menu.

| Area | Aggregate root | Invariant yang dijaga |
|---|---|---|
| Master Data Akuntansi | `AccChartOfAccount`, `AccJournalType` | Akun induk tidak menerima transaksi; kode akun unik per badan hukum; kode tidak berubah setelah dipakai |
| Journal Management | `AccJournal` | Total debit sama dengan total kredit; jurnal yang sudah disahkan tidak dapat diubah; pembuat bukan penyetuju |
| Accounting Period | `AccAccountingPeriod` | Pencatatan hanya masuk periode yang menerimanya; penutupan dan pembukaan kembali tercatat alasannya |

`AccJournal` adalah aggregate root yang membawahi `AccJournalLine` dan `AccJournalApproval`.
Barisnya **tidak** boleh diubah lewat endpoint tersendiri — selalu melalui jurnalnya, agar
keseimbangan tidak pernah dinilai setengah jalan.

### Batas transaksi database

| Operasi | Cakupan transaksi | Bila gagal di tengah |
|---|---|---|
| Simpan jurnal beserta barisnya | Satu transaksi mencakup header dan seluruh baris | Seluruhnya dibatalkan; tidak ada jurnal tanpa baris |
| Sahkan jurnal | Satu transaksi: ubah status, isi `PostedBy`/`PostedAt`, tulis riwayat persetujuan | Status tidak berubah sama sekali |
| Balik jurnal | Satu transaksi: buat jurnal pembalik beserta barisnya, tautkan ke jurnal asal | Tidak ada jurnal pembalik separuh jadi |
| Tutup periode | Satu transaksi: ubah status periode, catat alasan | Periode tetap pada status semula |

### Buku besar tidak disimpan sebagai tabel

Buku besar **dihitung** dari baris jurnal berstatus `Posted`, bukan disimpan sebagai tabel
tersendiri. Alasannya:

1. Tidak mungkin ada selisih antara jurnal dan buku besar, karena keduanya satu sumber.
2. `ACC-DEC-018` memulai sistem dari saldo awal saja, tanpa memindahkan riwayat lama, sehingga
   jumlah baris pada tahun pertama kecil.

**Contoh perhitungannya.** Saldo akhir akun `5-1001 Beban Obat` untuk badan hukum A pada periode
`2026-09` adalah jumlah seluruh `DebitAmount` dikurangi jumlah seluruh `CreditAmount` pada baris
jurnal berstatus `Posted` yang tanggal akuntansinya sampai 30 September 2026. Bila ada tiga
jurnal yang masing-masing mendebit Rp 3.000.000, mendebit Rp 1.500.000, dan mengkredit
Rp 500.000, maka saldonya Rp 4.000.000.

Bila kelak pengukuran nyata menunjukkan laporan melambat, tabel ringkasan saldo per akun per
periode ditambahkan sebagai optimasi — dan itu keputusan teknis, bukan keputusan bisnis.

### Saldo awal adalah jurnal, bukan tabel tersendiri

`ACC-DEC-018` menetapkan sistem dimulai dari saldo awal. Saldo awal diwujudkan sebagai **satu
jurnal biasa** berjenis `SA` (Saldo Awal), bukan tabel terpisah. Keuntungannya: saldo awal
otomatis tunduk pada aturan keseimbangan, otomatis masuk jejak audit, dan otomatis tampil di
buku besar tanpa kode khusus.

`ACC-DEC-033` menuntut pengesahan oleh Accounting Manager dengan persetujuan pimpinan keuangan.
Itu diwujudkan lewat `AccJournalType.RequiresApproval` bernilai benar pada jenis `SA`, ditambah
hak akses `Journal : Post` yang memang hanya dimiliki Manager.

---

## 3. Tabel kepemilikan data

Ini pertahanan paling langsung terhadap duplikasi entity.

| Kelompok data | Modul pemilik | Dipakai modul ini | Dibuat ulang di modul ini |
|---|---|:---:|---|
| Daftar akun (COA) | **Accounting** | Ya | Ya — memang milik modul ini (`ACC-DEC-002`) |
| Jurnal dan baris jurnal | **Accounting** | Ya | Ya — memang milik modul ini |
| Riwayat persetujuan jurnal | **Accounting** | Ya | Ya — data bisnis, bukan sekadar log |
| Periode akuntansi | **Accounting** | Ya | Ya — memang milik modul ini |
| Jenis jurnal | **Accounting** | Ya | Ya — master khusus akuntansi |
| Buku besar | **Accounting** | Ya | **Tidak** — dihitung dari baris jurnal |
| Cost Center | Corporate / Human Resource / Master Data / Organization | Ya | **Tidak** — dirujuk lewat `CostCenterId` |
| Badan hukum (`MstLegalEntity`) | Corporate / Master Data | Ya | **Tidak** — dirujuk lewat `LegalEntityId` |
| Lokasi rumah sakit (`MstHospitalSite`) | Corporate / Master Data | Tidak pada MVP | **Tidak** |
| Departemen dan unit organisasi | Corporate / Human Resource | Tidak langsung | **Tidak** — dicapai lewat `MstCostCenter` |
| Pengguna dan hak akses | Platform | Ya | **Tidak** — memakai mekanisme yang ada |
| Jejak audit teknis | Platform (`LoggerService`) | Ya | **Tidak** — memakai layanan yang ada |
| Faktur, item tagihan, pembayaran pasien | Billing dan Kasir | Tidak pada MVP | **Tidak** — dilarang `ACC-DEC-004` |
| Piutang dan utang operasional | Finance | Tidak pada MVP | **Tidak** — dilarang `ACC-DEC-003` |

---

## 4. Class diagram

Dipecah per area agar satu diagram muat dibaca dalam satu layar.

### 4.1 Master Data Akuntansi

```mermaid
classDiagram
    class AccChartOfAccount {
        +Guid Id
        +Guid LegalEntityId
        +string AccountCode
        +string AccountName
        +Guid ParentAccountId
        +int AccountLevel
        +AccountType AccountType
        +NormalBalance NormalBalance
        +bool IsPostable
        +bool IsActive
    }
    class AccJournalType {
        +Guid Id
        +string JournalTypeCode
        +string JournalTypeName
        +string NumberPrefix
        +bool RequiresApproval
        +bool IsSystemType
        +bool IsActive
    }
    class MstLegalEntity {
        +Guid Id
    }
    MstLegalEntity "1" --> "0..*" AccChartOfAccount : membatasi buku
    AccChartOfAccount "0..1" --> "0..*" AccChartOfAccount : induk-anak
```

### 4.2 Journal Management

```mermaid
classDiagram
    class AccJournal {
        +Guid Id
        +Guid LegalEntityId
        +string JournalNumber
        +Guid JournalTypeId
        +Guid AccountingPeriodId
        +DateTime AccountingDate
        +JournalStatus JournalStatus
        +decimal TotalDebit
        +decimal TotalCredit
        +Guid ReversalOfJournalId
        +JournalCorrectionType CorrectionType
    }
    class AccJournalLine {
        +Guid Id
        +Guid JournalId
        +int LineNumber
        +Guid AccountId
        +Guid CostCenterId
        +decimal DebitAmount
        +decimal CreditAmount
    }
    class AccJournalApproval {
        +Guid Id
        +Guid JournalId
        +JournalApprovalAction ApprovalAction
        +Guid ActionBy
        +DateTime ActionAt
        +string Reason
    }
    AccJournal "1" --> "2..*" AccJournalLine : memiliki
    AccJournal "1" --> "0..*" AccJournalApproval : riwayat
    AccJournal "0..1" --> "0..1" AccJournal : membalik
```

### 4.3 Accounting Period

```mermaid
classDiagram
    class AccAccountingPeriod {
        +Guid Id
        +Guid LegalEntityId
        +string PeriodCode
        +int FiscalYear
        +int PeriodMonth
        +DateTime StartDate
        +DateTime EndDate
        +AccountingPeriodStatus PeriodStatus
        +string LastReasonNote
    }
    class AccJournal {
        +Guid AccountingPeriodId
    }
    AccAccountingPeriod "1" --> "0..*" AccJournal : menampung
```

### 4.4 Layanan dan controller

```mermaid
classDiagram
    class ChartOfAccountController
    class JournalController
    class AccountingPeriodController
    class GeneralLedgerController
    class AccChartOfAccountService
    class AccJournalService
    class AccAccountingPeriodService
    class AccGeneralLedgerService
    class LoggerService

    ChartOfAccountController --> AccChartOfAccountService
    JournalController --> AccJournalService
    JournalController --> AccAccountingPeriodService
    AccountingPeriodController --> AccAccountingPeriodService
    GeneralLedgerController --> AccGeneralLedgerService
    AccJournalService --> LoggerService
    AccAccountingPeriodService --> LoggerService
```

---

## 5. Penjelasan setiap class

### 5.1 `AccChartOfAccount`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Baru` |
| **Lokasi file** | `Areas/Corporate/AccountingManagement/MasterData/ChartOfAccount/Models/AccChartOfAccount.cs` |
| Kategori | Master akuntansi |
| Tanggung jawab utama | Menyimpan satu akun pada daftar akun. Setiap akun adalah "laci" tempat rupiah digolongkan. Akun tersusun bertingkat: akun induk hanya menjumlahkan, akun paling bawah yang menerima transaksi |
| Field penting | `LegalEntityId`, `AccountCode`, `AccountName`, `ParentAccountId`, `AccountLevel`, `AccountType`, `NormalBalance`, `IsPostable`, `IsActive` |
| Navigation property dan relasi | Menunjuk `MstLegalEntity`; menunjuk dirinya sendiri lewat `ParentAccountId`; dirujuk banyak `AccJournalLine` |
| Pemakaian dalam alur bisnis | Dipakai saat petugas memilih akun pada baris jurnal, dan saat buku besar dikelompokkan |
| Catatan desain | `IsPostable` **tidak** boleh bernilai benar bila akun punya anak (`ACC-DEC-022`). `AccountCode` tidak boleh diubah setelah akun punya baris jurnal berstatus `Posted` (`ACC-DEC-023`). Akun tidak boleh dinonaktifkan bila saldonya belum nol (`ACC-DEC-024`). Kewajiban Cost Center **diturunkan** dari `AccountType == Expense`, tidak disimpan sebagai kolom tersendiri, agar tidak ada dua sumber kebenaran |
| Ekuivalen model lama | — |

### 5.2 `AccJournalType`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Baru` |
| **Lokasi file** | `Areas/Corporate/AccountingManagement/MasterData/JournalType/Models/AccJournalType.cs` |
| Kategori | Master akuntansi |
| Tanggung jawab utama | Menyimpan jenis jurnal beserta aturan alurnya. Inilah yang mewujudkan `ACC-DEC-010`, yaitu alur berbeda menurut jenis jurnal |
| Field penting | `JournalTypeCode`, `JournalTypeName`, `NumberPrefix`, `RequiresApproval`, `IsSystemType`, `IsActive` |
| Navigation property dan relasi | Dirujuk banyak `AccJournal` |
| Pemakaian dalam alur bisnis | Dipilih petugas saat membuat jurnal; menentukan awalan nomor dan apakah jurnal perlu disetujui |
| Catatan desain | Berlaku lintas badan hukum, jadi **tidak** punya `LegalEntityId` — jenis jurnal bersifat struktural. `IsSystemType` menandai jenis yang tidak boleh dihapus pengguna, yaitu Jurnal Pembalik dan Saldo Awal |
| Ekuivalen model lama | — |

### 5.3 `AccAccountingPeriod`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Baru` |
| **Lokasi file** | `Areas/Corporate/AccountingManagement/AccountingPeriod/Models/AccAccountingPeriod.cs` |
| Kategori | Transaksi akuntansi |
| Tanggung jawab utama | Menyimpan satu periode akuntansi beserta statusnya. Periode inilah yang mengunci pembukuan agar angka yang sudah dilaporkan tidak berubah diam-diam |
| Field penting | `LegalEntityId`, `PeriodCode`, `FiscalYear`, `PeriodMonth`, `StartDate`, `EndDate`, `PeriodStatus`, `ClosedBy`, `ClosedAt`, `ReopenedBy`, `ReopenedAt`, `LastReasonNote` |
| Navigation property dan relasi | Menunjuk `MstLegalEntity`; menampung banyak `AccJournal` |
| Pemakaian dalam alur bisnis | Diperiksa setiap kali jurnal disahkan; diubah statusnya saat tutup buku |
| Catatan desain | Periode bulanan mengikuti tahun kalender (`ACC-DEC-013`), sehingga `PeriodCode` berbentuk `2026-09`. Tiga status sesuai `ACC-DEC-012`. Riwayat penuh penutupan dan pembukaan kembali disimpan `LoggerService`; kolom pada tabel ini hanya menyimpan keadaan terakhir, agar tidak menduplikasi jejak audit |
| Ekuivalen model lama | — |

### 5.4 `AccJournal`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Baru` |
| **Lokasi file** | `Areas/Corporate/AccountingManagement/JournalManagement/Models/AccJournal.cs` |
| Kategori | Transaksi akuntansi — aggregate root |
| Tanggung jawab utama | Menyimpan kepala satu catatan transaksi akuntansi: nomor, tanggal, jenis, status, dan siapa yang mengerjakan setiap tahapnya |
| Field penting | `LegalEntityId`, `JournalNumber`, `JournalTypeId`, `AccountingPeriodId`, `DocumentNumber`, `DocumentDate`, `AccountingDate`, `Description`, `JournalStatus`, `TotalDebit`, `TotalCredit`, `SubmittedBy`, `SubmittedAt`, `ApprovedBy`, `ApprovedAt`, `PostedBy`, `PostedAt`, `RejectionReason`, `ReversalOfJournalId`, `CorrectionType` |
| Navigation property dan relasi | Menunjuk `MstLegalEntity`, `AccJournalType`, `AccAccountingPeriod`; memiliki banyak `AccJournalLine` dan `AccJournalApproval`; menunjuk dirinya sendiri lewat `ReversalOfJournalId` |
| Pemakaian dalam alur bisnis | Dibuat petugas akuntansi, diajukan, disetujui, lalu disahkan. Setelah disahkan tidak dapat diubah |
| Catatan desain | `TotalDebit` dan `TotalCredit` adalah **salinan untuk mempercepat tampilan daftar**, bukan sumber kebenaran. Keduanya dihitung ulang dari baris setiap kali baris berubah, dan dihitung ulang **sekali lagi saat pengesahan**. Nilai yang dipakai memutuskan boleh atau tidaknya pengesahan selalu hasil hitungan dari baris, bukan isi kolom ini. Satu jurnal **tidak boleh** mencampur dua badan hukum (`ACC-DEC-037`): seluruh barisnya harus menunjuk akun milik `LegalEntityId` yang sama dengan jurnalnya |
| Ekuivalen model lama | — |

### 5.5 `AccJournalLine`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Baru` |
| **Lokasi file** | `Areas/Corporate/AccountingManagement/JournalManagement/Models/AccJournalLine.cs` |
| Kategori | Transaksi akuntansi |
| Tanggung jawab utama | Menyimpan satu baris jurnal: akun mana, sisi debit atau kredit, berapa nilainya, dan unit biaya mana yang menanggung |
| Field penting | `JournalId`, `LineNumber`, `AccountId`, `CostCenterId`, `Description`, `DebitAmount`, `CreditAmount` |
| Navigation property dan relasi | Milik `AccJournal`; menunjuk `AccChartOfAccount`; menunjuk `MstCostCenter` |
| Pemakaian dalam alur bisnis | Diisi petugas saat menyusun jurnal; dijumlahkan untuk memeriksa keseimbangan; menjadi sumber tunggal buku besar |
| Catatan desain | Tepat satu dari `DebitAmount` atau `CreditAmount` harus lebih besar dari nol dan yang lain nol — satu baris tidak boleh mengisi keduanya. `CostCenterId` wajib bila akunnya berjenis beban (`ACC-DEC-019`), dan **tidak boleh** dibuatkan master sendiri karena `MstCostCenter` sudah ada. Tidak ada kolom mata uang maupun kurs (`ACC-DEC-020`) |
| Ekuivalen model lama | — |

### 5.6 `AccJournalApproval`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Baru` |
| **Lokasi file** | `Areas/Corporate/AccountingManagement/JournalManagement/Models/AccJournalApproval.cs` |
| Kategori | Transaksi akuntansi |
| Tanggung jawab utama | Menyimpan riwayat setiap tindakan pada sebuah jurnal: diajukan, disetujui, ditolak, disahkan, dibalik. Berbeda dari log teknis, riwayat ini ditampilkan kepada pengguna di layar rincian jurnal |
| Field penting | `JournalId`, `ApprovalAction`, `ActionBy`, `ActionAt`, `Reason` |
| Navigation property dan relasi | Milik `AccJournal` |
| Pemakaian dalam alur bisnis | Ditulis otomatis setiap kali status jurnal berubah; dibaca auditor dan penyetuju |
| Catatan desain | Baris pada tabel ini **tidak pernah** diubah atau dihapus. `Reason` wajib diisi untuk tindakan penolakan dan pembalikan. Tabel ini menjawab pertanyaan audit "siapa menyetujui apa" tanpa harus membaca log teknis |
| Ekuivalen model lama | — |

### 5.7 `AccChartOfAccountService`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Baru` |
| **Lokasi file** | `Areas/Corporate/AccountingManagement/MasterData/ChartOfAccount/Services/AccChartOfAccountService.cs` |
| Kategori | Service |
| Tanggung jawab utama | Menjaga aturan daftar akun: akun induk tidak menerima transaksi, kode tidak berubah setelah dipakai, akun bersaldo tidak boleh dinonaktifkan |
| Dipanggil oleh | `ChartOfAccountController` |
| Membuka transaksi database | Ya, saat menambah atau mengubah akun yang mengubah susunan induk-anak |
| Catatan desain | Tanpa interface, didaftarkan `AddScoped<AccChartOfAccountService>()`, di-inject langsung ke constructor controller — mengikuti pola yang berlaku di repository |

### 5.8 `AccJournalService`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Baru` |
| **Lokasi file** | `Areas/Corporate/AccountingManagement/JournalManagement/Services/AccJournalService.cs` |
| Kategori | Service |
| Tanggung jawab utama | Mengurus seluruh daur hidup jurnal: menyimpan beserta barisnya, memeriksa keseimbangan, mengajukan, menyetujui, menolak, mengesahkan, membalik, dan membangkitkan nomor jurnal |
| Dipanggil oleh | `JournalController` |
| Membuka transaksi database | Ya, pada setiap perubahan status dan pada penyimpanan jurnal beserta barisnya |
| Catatan desain | Inilah tempat `ACC-DEC-016` ditegakkan: persetujuan ditolak bila `ActionBy` sama dengan `CreateBy` jurnal. Nomor jurnal dibangkitkan **saat penyimpanan**, bukan saat layar dibuka, dan tanpa penguncian antrean nomor sesuai `ACC-DEC-014`. Larangan menghapus jurnal yang sudah disahkan — termasuk lewat penandaan `IsDelete` — ditegakkan di sini, bukan diserahkan pada kebiasaan pemanggil |

### 5.9 `AccAccountingPeriodService`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Baru` |
| **Lokasi file** | `Areas/Corporate/AccountingManagement/AccountingPeriod/Services/AccAccountingPeriodService.cs` |
| Kategori | Service |
| Tanggung jawab utama | Membangkitkan periode satu tahun buku, menutup, dan membuka kembali periode. Menyediakan pemeriksaan "apakah periode ini masih menerima pencatatan" yang dipakai `AccJournalService` |
| Dipanggil oleh | `AccountingPeriodController`, dan `AccJournalService` saat mengesahkan jurnal |
| Membuka transaksi database | Ya, saat menutup dan membuka kembali periode |
| Catatan desain | Pemeriksaan penerimaan pencatatan dibuat `public static` dengan `ApplicationDbContext` sebagai parameter, agar dapat dipakai controller maupun service lain **tanpa menambah baris registrasi baru** — mengikuti pola `EmergencyVisitService.PeriksaJenisEncounter` yang sudah ada di repository |

### 5.10 `AccGeneralLedgerService`

| Aspek | Penjelasan |
|---|---|
| **Status** | `Baru` |
| **Lokasi file** | `Areas/Corporate/AccountingManagement/GeneralLedger/Services/AccGeneralLedgerService.cs` |
| Kategori | Service |
| Tanggung jawab utama | Menghitung mutasi buku besar, saldo berjalan, dan neraca saldo dari baris jurnal berstatus `Posted` |
| Dipanggil oleh | `GeneralLedgerController` |
| Membuka transaksi database | Tidak — hanya membaca, memakai `AsNoTracking` |
| Catatan desain | Seluruh perhitungan menyaring `JournalStatus == Posted` dan `LegalEntityId`. Laporan **tidak boleh** mencampur jurnal yang sudah dan belum disahkan |

### 5.11 Controller

| Controller | Status | Lokasi file | Service yang dipakai | Endpoint yang diurus |
|---|---|---|---|---|
| `ChartOfAccountController` | `Baru` | `Areas/Corporate/AccountingManagement/MasterData/ChartOfAccount/Controllers/ChartOfAccountController.cs` | `AccChartOfAccountService` | Daftar, rincian, susunan pohon, opsi, tambah, ubah, nonaktifkan akun |
| `JournalTypeController` | `Baru` | `Areas/Corporate/AccountingManagement/MasterData/JournalType/Controllers/JournalTypeController.cs` | — | CRUD sederhana, memakai `ApplicationDbContext` langsung sesuai konvensi |
| `JournalController` | `Baru` | `Areas/Corporate/AccountingManagement/JournalManagement/Controllers/JournalController.cs` | `AccJournalService`, `AccAccountingPeriodService` | Daftar, rincian, buat, ubah, hapus draft, ajukan, setujui, tolak, sahkan, balik |
| `AccountingPeriodController` | `Baru` | `Areas/Corporate/AccountingManagement/AccountingPeriod/Controllers/AccountingPeriodController.cs` | `AccAccountingPeriodService` | Daftar periode, periode berjalan, bangkitkan setahun, tutup, buka kembali |
| `GeneralLedgerController` | `Baru` | `Areas/Corporate/AccountingManagement/GeneralLedger/Controllers/GeneralLedgerController.cs` | `AccGeneralLedgerService` | Mutasi buku besar, neraca saldo, saldo per akun |

`JournalTypeController` sengaja tidak memakai service, karena isinya CRUD sederhana tanpa aturan
bisnis lintas tabel. Ini sesuai konvensi yang berlaku di repository.

---

## 5b. Di mana proses bisnis dan aturannya ditulis

Dokumen ini sengaja **tidak** mengulang alur proses, transisi status, maupun aturan validasi.
Ketiganya punya berkas sendiri, supaya tidak ada dua salinan yang lama-lama berbeda.

| Yang dicari | Berkasnya |
|---|---|
| Alur bisnis dari kejadian sampai laporan, langkah demi langkah | [04-prd-to-mvp.md](04-prd-to-mvp.md) bagian 9 |
| Perpindahan status jurnal dan periode, termasuk yang **tidak** sah | [contracts/state-transition-matrix.md](contracts/state-transition-matrix.md) |
| Aturan validasi beserta pesan untuk pengguna | [contracts/validation-matrix.md](contracts/validation-matrix.md) |
| Daftar endpoint bergaya Swagger | [contracts/api-contract.md](contracts/api-contract.md) |
| Hak akses dan apa yang dicatat logger | [contracts/permission-audit-matrix.md](contracts/permission-audit-matrix.md) |
| Kolom, tipe, index, dan bentuk DDL | [erd/data-dictionary.md](erd/data-dictionary.md) |
| Skenario pengujian, termasuk jalur gagal | [testing/acceptance-test-matrix.md](testing/acceptance-test-matrix.md) |

---

## 6. Arsitektur folder

```text
Areas/Corporate/AccountingManagement/                    # Baru — seluruh isi
├── MasterData/
│   ├── ChartOfAccount/
│   │   ├── Controllers/ChartOfAccountController.cs      # Baru
│   │   ├── DTOs/ChartOfAccountDtos.cs                   # Baru
│   │   ├── Enums/AccountType.cs                         # Baru
│   │   ├── Enums/NormalBalance.cs                       # Baru
│   │   ├── Models/AccChartOfAccount.cs                  # Baru
│   │   └── Services/AccChartOfAccountService.cs         # Baru
│   └── JournalType/
│       ├── Controllers/JournalTypeController.cs         # Baru
│       ├── DTOs/JournalTypeDtos.cs                      # Baru
│       └── Models/AccJournalType.cs                     # Baru
├── JournalManagement/
│   ├── Controllers/JournalController.cs                 # Baru
│   ├── DTOs/JournalDtos.cs                              # Baru
│   ├── Enums/JournalStatus.cs                           # Baru
│   ├── Enums/JournalApprovalAction.cs                   # Baru
│   ├── Enums/JournalCorrectionType.cs                   # Baru
│   ├── Models/AccJournal.cs                             # Baru
│   ├── Models/AccJournalLine.cs                         # Baru
│   ├── Models/AccJournalApproval.cs                     # Baru
│   └── Services/AccJournalService.cs                    # Baru
├── AccountingPeriod/
│   ├── Controllers/AccountingPeriodController.cs        # Baru
│   ├── DTOs/AccountingPeriodDtos.cs                     # Baru
│   ├── Enums/AccountingPeriodStatus.cs                  # Baru
│   ├── Models/AccAccountingPeriod.cs                    # Baru
│   └── Services/AccAccountingPeriodService.cs           # Baru
└── GeneralLedger/
    ├── Controllers/GeneralLedgerController.cs           # Baru
    ├── DTOs/GeneralLedgerDtos.cs                        # Baru
    └── Services/AccGeneralLedgerService.cs              # Baru

Repositories/Configurations/Corporate/AccountingManagement/   # Baru
├── MasterData/AccChartOfAccountConfiguration.cs         # Baru
├── MasterData/AccJournalTypeConfiguration.cs            # Baru
├── JournalManagement/AccJournalConfiguration.cs         # Baru
├── JournalManagement/AccJournalLineConfiguration.cs     # Baru
├── JournalManagement/AccJournalApprovalConfiguration.cs # Baru
└── AccountingPeriod/AccAccountingPeriodConfiguration.cs # Baru

Repositories/ApplicationDbContext.cs                     # Diperbarui — 6 baris DbSet
Program.cs                                               # Diperbarui — 4 baris AddScoped
Migrations/                                              # Baru — satu migration, TERBLOKIR
```

Tiga hal yang mudah salah dan sengaja ditegaskan:

1. **Berkas configuration tidak berada di dalam `Areas/`.** Ia terpisah di
   `Repositories/Configurations/Corporate/AccountingManagement/`. Pola ini diambil apa adanya
   dari `Repositories/Configurations/Corporate/HumanResource/MasterData/Organization/MstCostCenterConfiguration.cs@aa837d7`.
2. **Nama domain di folder configuration adalah `Corporate`**, sama seperti di `Areas/`. Ini
   berbeda dari `HealthService` (tunggal) yang merupakan utang teknis modul lain — **jangan
   ditiru**.
3. **Folder controller memakai bentuk jamak `Controllers/`.** Bentuk tunggal `Controller/` yang
   ada di modul IGD adalah utang teknis dan tidak boleh ditiru.

### Tentang menyentuh `Program.cs`

Modul ini menambahkan **empat baris** `AddScoped<TService>()` ke `Program.cs`. Ini memang
diperlukan dan sesuai konvensi — sudah ada 164 baris sejenis di sana.

Yang **tidak** boleh ditambahkan ke `Program.cs`: pemanggilan seeder, logika startup, atau
konfigurasi khusus Accounting. Kebutuhan semacam itu diselesaikan di dalam
`Areas/Corporate/AccountingManagement/`. Bila sebuah logika perlu dipakai controller dan service
sekaligus tanpa menambah registrasi baru, pakai `public static` pada service yang sudah
terdaftar dan oper `ApplicationDbContext` sebagai parameter.

---

## 7. Status model dan dampak migration

| Model | Status | Kolom yang berubah | Dampak migration |
|---|---|---|---|
| `AccChartOfAccount` | `Baru` | Seluruh kolom baru | `CreateTable` + 2 index |
| `AccJournalType` | `Baru` | Seluruh kolom baru | `CreateTable` + 1 unique index |
| `AccAccountingPeriod` | `Baru` | Seluruh kolom baru | `CreateTable` + 1 unique index |
| `AccJournal` | `Baru` | Seluruh kolom baru | `CreateTable` + 4 index |
| `AccJournalLine` | `Baru` | Seluruh kolom baru | `CreateTable` + 3 index |
| `AccJournalApproval` | `Baru` | Seluruh kolom baru | `CreateTable` + 1 index |
| `MstCostCenter` | `Sudah ada` | **Tidak ada perubahan** | Hanya menjadi tujuan foreign key baru |
| `MstLegalEntity` | `Sudah ada` | **Tidak ada perubahan** | Hanya menjadi tujuan foreign key baru |
| `ApplicationDbContext` | `Diperbarui` | Menambah 6 properti `DbSet` | Tidak menghasilkan operasi tabel tersendiri |

Tidak ada satu pun tabel milik modul lain yang diubah. Ini penting: seluruh dampak migration
Accounting seharusnya berupa tujuh `CreateTable` beserta index dan foreign key-nya. **Bila
migration yang dihasilkan memuat operasi di luar itu, hentikan** — artinya `ACC-DEP-001` belum
selesai.

---

## 8. Rencana migration

**Status: TERBLOKIR oleh `ACC-DEP-002` saja.** `ACC-DEP-001` sudah selesai pada 30 Agustus 2026 dan
diverifikasi 1 September 2026 — snapshot `aa837d7` berisi 530 blok dengan 28 `Bil`, identik dengan
`origin/QuilvianIntegrationBackend@c081939`.

| Langkah | Isi | Tanpa mematikan layanan? | Cara mundur |
|---|---|:---:|---|
| 1 | ~~Pemulihan snapshot model EF bersama~~ | — | **Sudah selesai** 30 Agustus 2026 |
| 2 | Pendaftaran prefix `Acc` di `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` | — | Prasyarat, bukan migration. **Satu-satunya yang tersisa** |
| 3 | Buat tujuh tabel Accounting beserta index dan foreign key | Ya — seluruhnya tabel baru, tidak ada tabel berjalan yang disentuh | `DROP TABLE` ketujuh tabel; aman karena belum ada modul lain yang bergantung |
| 4 | Isi data master awal (lihat bagian 9) | Ya | Hapus baris master yang baru diisi |
| 5 | Buat jurnal Saldo Awal lewat aplikasi, bukan lewat skrip | Ya | Batalkan lewat jurnal pembalik, sesuai `ACC-DEC-006` |

Pengisian data lama tidak diperlukan, karena `ACC-DEC-018` memulai sistem dari saldo awal saja
tanpa memindahkan riwayat jurnal lama.

**Pemeriksaan wajib sebelum migration diterima.** Setelah `dotnet ef migrations add` dijalankan,
buka berkas migration yang dihasilkan lalu hitung operasinya. Yang benar hanya tujuh `CreateTable`
bernama `Acc*` beserta index dan foreign key-nya. Bila muncul operasi bernama `Bil*`, `Opr*`,
atau `Mst*`, berarti snapshot masih rusak — buang migration itu dan laporkan ke lead.

Pembuatan maupun penjalanan migration dilakukan sendiri oleh owner modul dengan wewenang
terpisah. Dokumen ini tidak memberi wewenang itu.

---

## 9. Rencana data master awal

Modul dengan tabel master kosong tidak dapat dipakai sama sekali. Berikut isi minimumnya.

### 9.1 `AccJournalType` — empat jenis

| Kode | Nama | Awalan nomor | Perlu persetujuan | Jenis sistem | Sumber nilai |
|---|---|---|:---:|:---:|---|
| `JU` | Jurnal Umum | `JU` | Ya | Tidak | Kebijakan akuntansi, `ACC-DEC-010` |
| `JP` | Jurnal Penyesuaian | `JP` | Ya | Tidak | Kebijakan akuntansi, `ACC-DEC-017` |
| `JB` | Jurnal Pembalik | `JB` | Ya | **Ya** | Dibuat sistem saat pembalikan, `ACC-DEC-029` |
| `SA` | Saldo Awal | `SA` | Ya | **Ya** | `ACC-DEC-018`, `ACC-DEC-033` |

Awalan nomor **wajib** berasal dari master ini, dan tidak boleh ditulis langsung di dalam
controller maupun frontend.

### 9.2 `AccAccountingPeriod` — dua belas periode per tahun buku

Dibangkitkan sekaligus lewat endpoint `POST /generate`, bukan diisi satu per satu. Contoh untuk
tahun buku 2027 pada satu badan hukum:

| `PeriodCode` | `FiscalYear` | `PeriodMonth` | `StartDate` | `EndDate` | Status awal |
|---|---:|---:|---|---|---|
| `2027-01` | 2027 | 1 | 1 Januari 2027 | 31 Januari 2027 | `Open` |
| `2027-02` | 2027 | 2 | 1 Februari 2027 | 28 Februari 2027 | `Open` |
| … | … | … | … | … | `Open` |
| `2027-12` | 2027 | 12 | 1 Desember 2027 | 31 Desember 2027 | `Open` |

Tahun kabisat ditangani perhitungan tanggal, bukan didaftar manual.

### 9.3 `AccChartOfAccount` — kerangka minimum lima kelompok

Daftar akun lengkap adalah kebijakan akuntansi rumah sakit dan **wajib disusun pemilik proses**,
bukan dikarang di sini. Yang dapat dipastikan hanya kerangka tingkat pertamanya, karena ia
mengikuti klasifikasi laporan keuangan yang baku:

| Kode | Nama | Jenis | Saldo normal | Menerima transaksi |
|---|---|---|---|:---:|
| `1` | Aset | `Asset` | Debit | Tidak |
| `2` | Liabilitas | `Liability` | Kredit | Tidak |
| `3` | Ekuitas | `Equity` | Kredit | Tidak |
| `4` | Pendapatan | `Revenue` | Kredit | Tidak |
| `5` | Beban | `Expense` | Debit | Tidak |

Kelimanya berstatus tidak menerima transaksi, sesuai `ACC-DEC-022`. Akun turunannya diisi
pemilik proses sebelum modul dipakai, dan wajib dibuat per badan hukum sesuai `ACC-DEC-037`.

**Contoh turunan yang wajar**, sebagai gambaran saja dan bukan keputusan: `1-1001 Kas Besar`,
`1-1201 Piutang Penjamin`, `4-1001 Pendapatan Rawat Inap`, `5-1001 Beban Obat`. Yang berjenis
`Expense` akan mewajibkan Cost Center pada setiap baris jurnalnya.

---

## 10. Yang sengaja tidak dibuat

Bagian ini mencegah orang berikutnya mengusulkan ulang hal yang sama.

| Yang ditolak | Alasan |
|---|---|
| Tabel Cost Center milik Accounting | `MstCostCenter` sudah ada di `Areas/Corporate/HumanResource/MasterData/Organization/Models/`, lengkap dengan `LegalEntityId` dan bahkan kolom `AccountingCode`. Dirujuk lewat `CostCenterId` |
| Tabel badan hukum milik Accounting | `MstLegalEntity` sudah ada dan dipakai 83 berkas di domain Corporate |
| Tabel buku besar tersendiri | Buku besar dihitung dari baris jurnal berstatus `Posted`. Tabel terpisah menambah risiko selisih tanpa manfaat pada volume MVP |
| Tabel saldo awal tersendiri | Saldo awal diwujudkan sebagai jurnal berjenis `SA`, sehingga otomatis tunduk pada aturan keseimbangan dan jejak audit |
| Tabel jejak audit milik Accounting | `Services/Logging/LoggerService.cs` sudah ada dan dipakai seluruh modul. `AccJournalApproval` bukan penggantinya — ia data bisnis yang ditampilkan ke pengguna, bukan log teknis |
| `DbContext` khusus Accounting | `AGENTS.md` menetapkan satu `ApplicationDbContext` untuk seluruh aplikasi |
| Lapisan repository atau interface service | Repository ini memakai `ApplicationDbContext` langsung, dan service tanpa interface. Menambah abstraksi baru melanggar konvensi yang berlaku |
| Kolom `RequiresCostCenter` pada `AccChartOfAccount` | Kewajiban Cost Center diturunkan dari `AccountType == Expense` sesuai `ACC-DEC-019`. Kolom tersendiri menciptakan sumber kebenaran kedua yang bisa bertentangan |
| Kolom mata uang, kurs, dan selisih kurs | Dilarang `ACC-DEC-020`; rilis pertama hanya rupiah |
| Kolom `SourceDomain` dan `SourceTransactionId` pada `AccJournal` | Milik jalur jurnal otomatis yang ada di Phase 2. Menambahkannya sekarang berarti menebak bentuk kontrak yang `ACC-XM-001`-nya belum diputuskan. Ditambahkan nanti sebagai kolom baru yang boleh kosong |
| Tabel kotak masuk kejadian dan pemetaan posting | Phase 2 (`ACC-DEC-009`, `ACC-DEC-036`) |
| Endpoint ubah dan hapus baris jurnal tersendiri | Baris selalu diubah lewat jurnalnya, agar keseimbangan tidak pernah dinilai setengah jalan |
| Penomoran jurnal tanpa celah | Ditolak `ACC-DEC-014`, karena menuntut penguncian antrean nomor yang memperlambat penyimpanan bersamaan |

---

## 11. Keamanan, privasi, dan pencatatan

| Aspek | Ketentuan |
|---|---|
| Autentikasi | `[Authorize]` pada seluruh controller, mengikuti pola yang berlaku |
| Hak akses | `[AccessController]` di kelas, `[AccessAction]` dan `[AccessPermission("Resource","Action")]` di setiap endpoint. Daftar lengkapnya di [contracts/permission-audit-matrix.md](contracts/permission-audit-matrix.md) |
| Data pribadi | **Tidak ada.** MVP tidak menyimpan satu pun kolom pasien maupun pegawai. Nilai uang bersifat rahasia bisnis, bukan data pribadi |
| Pencatatan | `LoggerService` mencatat tindakan Create, Update, perubahan status, dan Delete. Permintaan `GET` tidak dicatat, kecuali dua pengecualian pada `ACC-DEC-032` |
| Isi catatan | Hanya `EntityId`, controller, action, dan status. **Tidak boleh** memuat nilai uang maupun keterangan jurnal |
| Penyimpanan | Jurnal berstatus `Posted` tidak pernah dihapus, termasuk lewat penandaan `IsDelete` |

Perlu dicatat: seluruh model mewarisi `IdentityModel`, yang menyediakan penghapusan berupa
penandaan `IsDelete`. Untuk jurnal yang sudah disahkan, penandaan itu **tetap dilarang** oleh
`ACC-DEC-006`, dan larangannya ditegakkan di `AccJournalService`.

---

## 12. Strategi pengujian

| Lapis | Yang diuji | Catatan |
|---|---|---|
| Unit | Perhitungan keseimbangan, pembangkitan nomor jurnal, penurunan kewajiban Cost Center, penentuan periode dari tanggal akuntansi | Proyek `QuilvianSystemBackend.Tests` berjalan tanpa database |
| Integrasi | Daur hidup jurnal ujung ke ujung, penolakan saat periode tertutup, penolakan menyetujui jurnal sendiri | Menuntut database khusus test yang namanya mengandung `test`. **Jangan** memakai database pengembangan bersama |
| Acceptance | Skenario pada [testing/acceptance-test-matrix.md](testing/acceptance-test-matrix.md) | Termasuk jalur gagal, bukan hanya jalur berhasil |

---

## 13. Ketergantungan yang tersisa

| Butir | Status | Memblokir apa |
|---|---|---|
| `ACC-DEP-001` snapshot model EF bersama | **`RESOLVED`** 30 Agustus 2026 | Tidak lagi memblokir apa pun |
| `ACC-DEP-002` prefix penamaan entity | `MISSING`, milik lead | Penamaan kelas dan tabel. **Tidak** memblokir bentuk kolom dan relasi |
| `ACC-XM-001` penerbit kejadian keuangan | Terbuka, lintas modul | Phase 2 saja |
| `requirement-completeness-gate` dan `hospital-domain-architect` | Belum dijalankan | Phase 2 saja, sesuai penilaian gerbang di bagian 1 |
| Daftar akun lengkap per badan hukum | Milik pemilik proses akuntansi | Pemakaian nyata, bukan pembangunan. Modul dapat dibangun dan diuji dengan kerangka lima kelompok |
