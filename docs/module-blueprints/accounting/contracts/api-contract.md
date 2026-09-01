# Accounting — API Contract

| Field | Value |
|---|---|
| `contract_version` | `ACC-API-0.1` |
| Status | `draft` — approval adalah tindakan manusia |
| Owner | Rizki (Product/Domain Owner), owner API backend |
| `approved_by` / `approved_at` | Belum ada |
| `input_revision` | `00-interview-decisions.md@3`, `02-backend-architecture.md@3` |
| Traceability | `ACC-DEC-009` sampai `ACC-DEC-037` |
| Dampak kompatibilitas | Seluruhnya endpoint baru. Tidak ada endpoint existing yang berubah maupun rusak |

**Seluruh endpoint pada dokumen ini berstatus `Rencana (belum tersedia)`.** Tidak satu pun sudah
ada di `aa837d7`. Label itu tidak diulang pada setiap baris agar tabel tetap terbaca; ia berlaku
untuk semua.

Amplop respons memakai `ApiResponse<T>` dan daftar berhalaman memakai `PagedResult<T>`, mengikuti
`Responses/ApiResponse.cs` dan `Responses/PagedResult.cs` yang sudah ada.

Seluruh permintaan wajib menyertakan badan hukum yang dituju, karena pembukuan dipisah per badan
hukum (`ACC-DEC-037`). Pada endpoint daftar, badan hukum dikirim sebagai penyaring; pada endpoint
pembuatan, ia bagian dari isi permintaan.

---

## Corporate / Accounting / Master Data / Chart of Account

Base URL: `api/v1/corporate/accounting/master-data/chart-of-accounts`
Contract version: `ACC-API-0.1` — status `draft`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/` | Menampilkan daftar akun berhalaman, dapat disaring per badan hukum, jenis akun, dan status | `ChartOfAccount : Read` | `ChartOfAccountPagedQuery` | `ApiResponse<PagedResult<ChartOfAccountListDto>>` |
| `GET` | `/{id}` | Menampilkan rincian satu akun | `ChartOfAccount : Read` | — | `ApiResponse<ChartOfAccountDetailDto>` |
| `GET` | `/tree` | Menampilkan akun sebagai susunan induk dan anak untuk satu badan hukum | `ChartOfAccount : Read` | `legalEntityId` pada query | `ApiResponse<List<ChartOfAccountTreeDto>>` |
| `GET` | `/options` | Daftar ringkas akun yang menerima transaksi dan aktif, untuk isian pilihan pada form jurnal | `ChartOfAccount : Read` | `legalEntityId`, `search` pada query | `ApiResponse<List<ChartOfAccountOptionDto>>` |
| `POST` | `/` | Menambah akun baru | `ChartOfAccount : Create` | `CreateChartOfAccountDto` | `ApiResponse<ChartOfAccountDetailDto>` |
| `PUT` | `/{id}` | Mengubah nama, induk, atau keterangan akun | `ChartOfAccount : Update` | `UpdateChartOfAccountDto` | `ApiResponse<ChartOfAccountDetailDto>` |
| `PATCH` | `/{id}/deactivate` | Menonaktifkan akun | `ChartOfAccount : Update` | `DeactivateChartOfAccountDto` | `ApiResponse<ChartOfAccountDetailDto>` |
| `PATCH` | `/{id}/activate` | Mengaktifkan kembali akun | `ChartOfAccount : Update` | — | `ApiResponse<ChartOfAccountDetailDto>` |

Arti kode status bagi pengguna:

- `200` — permintaan berhasil.
- `400` — isian tidak lengkap atau tidak masuk akal, misalnya kode akun kosong atau tingkat akun
  di luar 1 sampai 5.
- `403` — pengguna tidak punya hak akses untuk tindakan ini, atau badan hukum yang dituju bukan
  haknya.
- `404` — akun yang dicari tidak ditemukan.
- `409` — bentrok dengan aturan bisnis. Ada empat kemungkinan: kode akun sudah dipakai badan hukum
  yang sama; akun hendak dijadikan penerima transaksi padahal punya anak; kode hendak diubah
  padahal sudah dipakai jurnal yang disahkan; akun hendak dinonaktifkan padahal saldonya belum
  nol.

Endpoint `/options` sengaja hanya mengembalikan akun yang **menerima transaksi dan aktif**.
Dengan begitu petugas tidak pernah melihat akun induk pada daftar pilihan, dan `ACC-DEC-022`
terjaga sejak di layar, bukan hanya saat penyimpanan.

---

## Corporate / Accounting / Master Data / Journal Type

Base URL: `api/v1/corporate/accounting/master-data/journal-types`
Contract version: `ACC-API-0.1` — status `draft`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/` | Menampilkan daftar jenis jurnal | `JournalType : Read` | `JournalTypePagedQuery` | `ApiResponse<PagedResult<JournalTypeDto>>` |
| `GET` | `/options` | Daftar ringkas jenis jurnal aktif untuk isian pilihan | `JournalType : Read` | — | `ApiResponse<List<JournalTypeOptionDto>>` |
| `POST` | `/` | Menambah jenis jurnal | `JournalType : Create` | `CreateJournalTypeDto` | `ApiResponse<JournalTypeDto>` |
| `PUT` | `/{id}` | Mengubah jenis jurnal | `JournalType : Update` | `UpdateJournalTypeDto` | `ApiResponse<JournalTypeDto>` |

Arti kode status bagi pengguna:

- `400` — isian tidak lengkap, misalnya awalan nomor kosong.
- `409` — kode jenis jurnal sudah dipakai, atau jenis jurnal sistem hendak diubah kode maupun
  awalan nomornya.

Jenis jurnal `JB` dan `SA` bertanda sistem. Keduanya tidak dapat dihapus, dan kode maupun awalan
nomornya tidak dapat diubah, karena dipakai langsung oleh proses pembalikan dan saldo awal.

---

## Corporate / Accounting / Journal Management / Journal

Base URL: `api/v1/corporate/accounting/journals`
Contract version: `ACC-API-0.1` — status `draft`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/` | Mencari jurnal berdasarkan badan hukum, rentang tanggal, jenis, status, atau nomor | `Journal : Read` | `JournalPagedQuery` | `ApiResponse<PagedResult<JournalListDto>>` |
| `GET` | `/{id}` | Menampilkan satu jurnal beserta seluruh baris dan riwayat persetujuannya | `Journal : Read` | — | `ApiResponse<JournalDetailDto>` |
| `POST` | `/` | Membuat jurnal draft beserta barisnya | `Journal : Create` | `CreateJournalDto` | `ApiResponse<JournalDetailDto>` |
| `PUT` | `/{id}` | Mengubah jurnal draft beserta barisnya. Baris dikirim utuh, bukan sebagian | `Journal : Update` | `UpdateJournalDto` | `ApiResponse<JournalDetailDto>` |
| `DELETE` | `/{id}` | Menghapus jurnal yang masih draft | `Journal : Delete` | — | `ApiResponse<bool>` |
| `POST` | `/{id}/submit` | Mengajukan jurnal untuk disetujui | `Journal : Submit` | — | `ApiResponse<JournalDetailDto>` |
| `POST` | `/{id}/approve` | Menyetujui jurnal | `Journal : Approve` | — | `ApiResponse<JournalDetailDto>` |
| `POST` | `/{id}/reject` | Menolak jurnal beserta alasannya | `Journal : Approve` | `RejectJournalDto` | `ApiResponse<JournalDetailDto>` |
| `POST` | `/{id}/post` | Mengesahkan jurnal ke buku besar | `Journal : Post` | — | `ApiResponse<JournalDetailDto>` |
| `POST` | `/{id}/reverse` | Membuat jurnal pembalik atau jurnal penyesuaian | `Journal : Reverse` | `ReverseJournalDto` | `ApiResponse<JournalDetailDto>` |

Arti kode status bagi pengguna:

- `200` — permintaan berhasil.
- `400` — isian tidak sah. Contoh: jurnal belum seimbang saat diajukan; ada baris yang mengisi
  debit dan kredit sekaligus; alasan penolakan kosong.
- `403` — pengguna tidak berwenang. Contoh paling sering: mencoba menyetujui jurnal buatan
  sendiri, yang dilarang `ACC-DEC-016` tanpa pengecualian.
- `404` — jurnal tidak ditemukan.
- `409` — tindakan tidak sesuai status jurnal saat ini. Contoh: mengesahkan jurnal yang belum
  disetujui, mengubah jurnal yang sudah disahkan, atau membalik jurnal yang sudah pernah dibalik.
- `422` — periode akuntansi tujuan tidak menerima jenis jurnal ini. Pesannya menyebut nama
  periodenya, misalnya "Periode September 2026 sudah ditutup sementara."

Perlu dicatat: **tidak ada endpoint untuk baris jurnal.** Baris selalu dikirim bersama jurnalnya
lewat `POST /` atau `PUT /{id}`, dan dikirim **utuh** — daftar baris yang dikirim menggantikan
seluruh baris sebelumnya. Ini disengaja agar keseimbangan tidak pernah dinilai setengah jalan.

Endpoint `/{id}/reverse` menerima pilihan cara koreksi sesuai `ACC-DEC-017`. Bila yang dipilih
pembalikan penuh, sistem membuat jurnal berjenis `JB` yang membalik seluruh baris. Bila yang
dipilih penyesuaian, sistem membuat jurnal berjenis `JP` berisi baris selisih yang dikirim
pengguna. Keduanya lahir berstatus menunggu persetujuan, bukan langsung disahkan
(`ACC-DEC-029`).

---

## Corporate / Accounting / Accounting Period

Base URL: `api/v1/corporate/accounting/periods`
Contract version: `ACC-API-0.1` — status `draft`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/` | Menampilkan daftar periode beserta statusnya, disaring per badan hukum dan tahun buku | `AccountingPeriod : Read` | `AccountingPeriodPagedQuery` | `ApiResponse<PagedResult<AccountingPeriodDto>>` |
| `GET` | `/current` | Menampilkan periode yang sedang berjalan untuk satu badan hukum | `AccountingPeriod : Read` | `legalEntityId` pada query | `ApiResponse<AccountingPeriodDto>` |
| `POST` | `/generate` | Membangkitkan dua belas periode sekaligus untuk satu tahun buku | `AccountingPeriod : Create` | `GenerateAccountingPeriodDto` | `ApiResponse<List<AccountingPeriodDto>>` |
| `POST` | `/{id}/close` | Menutup periode, sementara atau permanen | `AccountingPeriod : Close` | `ClosePeriodDto` | `ApiResponse<AccountingPeriodDto>` |
| `POST` | `/{id}/reopen` | Membuka kembali periode yang sudah ditutup | `AccountingPeriod : Reopen` | `ReopenPeriodDto` | `ApiResponse<AccountingPeriodDto>` |

Arti kode status bagi pengguna:

- `400` — alasan pembukaan kembali kosong, padahal wajib diisi (`ACC-DEC-027`).
- `403` — hanya Manajer Akuntansi yang dapat menutup dan membuka kembali periode
  (`ACC-DEC-026`).
- `409` — periode tahun buku itu sudah pernah dibangkitkan, atau perpindahan status tidak sah.

`ClosePeriodDto` memuat pilihan jenis penutupan, yaitu tutup sementara atau tutup permanen.
`ReopenPeriodDto` memuat alasan tertulis yang wajib diisi, dan alasan itu ikut tercatat di jejak
audit.

Perlu ditegaskan: membuka kembali periode yang sudah tutup permanen menghasilkan status **tutup
sementara**, bukan terbuka. Ini yang mewujudkan `ACC-DEC-028` — setelah dibuka kembali, hanya
penyesuaian dan pembalikan yang boleh masuk.

---

## Corporate / Accounting / General Ledger

Base URL: `api/v1/corporate/accounting/general-ledger`
Contract version: `ACC-API-0.1` — status `draft`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/movements` | Menampilkan mutasi buku besar per akun dan rentang tanggal, beserta saldo berjalan | `GeneralLedger : Read` | `LedgerMovementQuery` | `ApiResponse<PagedResult<LedgerMovementDto>>` |
| `GET` | `/trial-balance` | Menampilkan neraca saldo satu periode untuk satu badan hukum | `GeneralLedger : Read` | `TrialBalanceQuery` | `ApiResponse<TrialBalanceDto>` |
| `GET` | `/account-balance/{accountId}` | Menampilkan saldo awal, mutasi, dan saldo akhir satu akun | `GeneralLedger : Read` | `periodCode` pada query | `ApiResponse<AccountBalanceDto>` |

Arti kode status bagi pengguna:

- `400` — rentang tanggal tidak masuk akal, misalnya tanggal akhir mendahului tanggal mulai.
- `403` — badan hukum yang diminta bukan hak pengguna.
- `404` — akun atau periode tidak ditemukan.

Seluruh endpoint pada grup ini **hanya menghitung jurnal berstatus disahkan**. Jurnal draft,
menunggu persetujuan, dan disetujui tidak pernah ikut. Laporan tidak boleh mencampur yang sudah
dan belum disahkan.

`GET /trial-balance` adalah satu-satunya endpoint pembacaan yang pembacaannya dicatat logger,
sesuai `ACC-DEC-032`. Rinciannya di [permission-audit-matrix.md](permission-audit-matrix.md).

---

## Daftar DTO

| Nama class | Jenis | Field utama |
|---|---|---|
| `ChartOfAccountPagedQuery` | PagedQuery | `LegalEntityId`, `Search`, `AccountType`, `IsActive`, `PageNumber`, `PageSize` |
| `ChartOfAccountListDto` | Response | `Id`, `AccountCode`, `AccountName`, `AccountType`, `AccountLevel`, `IsPostable`, `IsActive` |
| `ChartOfAccountDetailDto` | Response | Seluruh field daftar ditambah `ParentAccountId`, `ParentAccountName`, `NormalBalance`, `Description`, `RequiresCostCenter` |
| `ChartOfAccountTreeDto` | Response | `Id`, `AccountCode`, `AccountName`, `IsPostable`, `Children` |
| `ChartOfAccountOptionDto` | Option | `Id`, `AccountCode`, `AccountName`, `AccountType`, `RequiresCostCenter` |
| `CreateChartOfAccountDto` | Create | `LegalEntityId`, `AccountCode`, `AccountName`, `ParentAccountId`, `AccountType`, `NormalBalance`, `IsPostable`, `EffectiveStartDate`, `Description` |
| `UpdateChartOfAccountDto` | Update | `AccountName`, `ParentAccountId`, `AccountCode`, `NormalBalance`, `IsPostable`, `Description` |
| `DeactivateChartOfAccountDto` | Status | `Reason` |
| `JournalTypeDto` | Response | `Id`, `JournalTypeCode`, `JournalTypeName`, `NumberPrefix`, `RequiresApproval`, `IsSystemType`, `IsActive` |
| `JournalTypeOptionDto` | Option | `Id`, `JournalTypeCode`, `JournalTypeName` |
| `CreateJournalTypeDto` / `UpdateJournalTypeDto` | Create/Update | `JournalTypeCode`, `JournalTypeName`, `NumberPrefix`, `RequiresApproval` |
| `JournalPagedQuery` | PagedQuery | `LegalEntityId`, `DateFrom`, `DateTo`, `JournalTypeId`, `JournalStatus`, `Search`, `PageNumber`, `PageSize` |
| `JournalListDto` | Response | `Id`, `JournalNumber`, `AccountingDate`, `JournalTypeName`, `Description`, `JournalStatus`, `TotalDebit`, `TotalCredit` |
| `JournalDetailDto` | Response | Seluruh field daftar ditambah `Lines`, `Approvals`, `PeriodCode`, `ReversalOfJournalNumber`, `CorrectionType`, `AvailableActions` |
| `JournalLineDto` | Response | `LineNumber`, `AccountId`, `AccountCode`, `AccountName`, `CostCenterId`, `CostCenterName`, `Description`, `DebitAmount`, `CreditAmount` |
| `JournalApprovalDto` | Response | `ApprovalAction`, `ActionByName`, `ActionAt`, `Reason` |
| `CreateJournalDto` | Create | `LegalEntityId`, `JournalTypeId`, `DocumentNumber`, `DocumentDate`, `AccountingDate`, `Description`, `Lines` |
| `CreateJournalLineDto` | Create | `LineNumber`, `AccountId`, `CostCenterId`, `Description`, `DebitAmount`, `CreditAmount` |
| `UpdateJournalDto` | Update | Sama dengan `CreateJournalDto` tanpa `LegalEntityId` |
| `RejectJournalDto` | Status | `Reason` |
| `ReverseJournalDto` | Status | `CorrectionType`, `Reason`, `AccountingDate`, `AdjustmentLines` |
| `AccountingPeriodPagedQuery` | PagedQuery | `LegalEntityId`, `FiscalYear`, `PeriodStatus`, `PageNumber`, `PageSize` |
| `AccountingPeriodDto` | Response | `Id`, `PeriodCode`, `FiscalYear`, `PeriodMonth`, `StartDate`, `EndDate`, `PeriodStatus`, `ClosedAt`, `ReopenedAt`, `LastReasonNote` |
| `GenerateAccountingPeriodDto` | Create | `LegalEntityId`, `FiscalYear` |
| `ClosePeriodDto` | Status | `CloseType`, `Reason` |
| `ReopenPeriodDto` | Status | `Reason` |
| `LedgerMovementQuery` | PagedQuery | `LegalEntityId`, `AccountId`, `DateFrom`, `DateTo`, `PageNumber`, `PageSize` |
| `LedgerMovementDto` | Response | `AccountingDate`, `JournalNumber`, `Description`, `DebitAmount`, `CreditAmount`, `RunningBalance` |
| `TrialBalanceQuery` | Query | `LegalEntityId`, `PeriodCode` |
| `TrialBalanceDto` | Response | `PeriodCode`, `Rows`, `TotalDebit`, `TotalCredit`, `IsBalanced` |
| `TrialBalanceRowDto` | Response | `AccountCode`, `AccountName`, `OpeningBalance`, `TotalDebit`, `TotalCredit`, `ClosingBalance` |
| `AccountBalanceDto` | Response | `AccountCode`, `AccountName`, `PeriodCode`, `OpeningBalance`, `TotalDebit`, `TotalCredit`, `ClosingBalance` |

`RequiresCostCenter` pada DTO akun **diturunkan** dari `AccountType == Expense`, bukan dibaca
dari kolom tabel. Ia disertakan agar frontend tahu kapan harus mewajibkan pengisian Cost Center
tanpa perlu menghafal aturannya sendiri.

`AvailableActions` pada `JournalDetailDto` berisi daftar tindakan yang boleh dilakukan pengguna
saat itu, dihitung backend dari status jurnal, hak akses, dan aturan `ACC-DEC-016`. Frontend
menampilkan tombol berdasarkan daftar ini — bukan menghitung sendiri. Backend tetap memeriksa
ulang saat tindakannya benar-benar dijalankan.
