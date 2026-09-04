# Accounting — API Contract

| Field | Value |
|---|---|
| `contract_version` | `ACC-API-0.4` |
| Status | `draft` — approval adalah tindakan manusia |
| Owner | Rizki (Product/Domain Owner), owner API backend |
| `approved_by` / `approved_at` | Belum ada |
| `input_revision` | `00-interview-decisions.md@3`, `02-backend-architecture.md@3` |
| Traceability | `ACC-DEC-009` sampai `ACC-DEC-037` |
| Dampak kompatibilitas | Seluruhnya endpoint baru. Tidak ada endpoint existing yang berubah maupun rusak |

**Seluruh 31 endpoint pada dokumen ini sudah BERDIRI** pada `822d48a`, terbukti audit kesiapan
4 September 2026: path, method, dan hak akses cocok 31/31. Kalimat sebelumnya menyatakan semuanya
masih `Rencana (belum tersedia)` — benar pada `aa837d7`, dan sudah tidak benar sejak `BE-ACC-007`
sampai `BE-ACC-014` selesai. Diperbaiki bersama ratifikasi `ACC-GAP-004`.

Amplop respons memakai `ApiResponse<T>` dan daftar berhalaman memakai `PagedResult<T>`, mengikuti
`Responses/ApiResponse.cs` dan `Responses/PagedResult.cs` yang sudah ada.

Seluruh permintaan wajib menyertakan badan hukum yang dituju, karena pembukuan dipisah per badan
hukum (`ACC-DEC-037`). Pada endpoint daftar, badan hukum dikirim sebagai penyaring; pada endpoint
pembuatan, ia bagian dari isi permintaan.

---

## Corporate / Accounting / Master Data / Chart of Account

Base URL: `api/v1/corporate/accounting/master-data/chart-of-accounts`
Contract version: `ACC-API-0.3` — status `approved`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/` | Menampilkan daftar akun berhalaman, dapat disaring per badan hukum, jenis akun, dan status | `ChartOfAccount : Read` | `ChartOfAccountPagedQuery` | `ApiResponse<PagedResult<ChartOfAccountListDto>>` |
| `GET` | `/{id}` | Menampilkan rincian satu akun | `ChartOfAccount : Read` | — | `ApiResponse<ChartOfAccountDetailDto>` |
| `GET` | `/tree` | Menampilkan akun sebagai susunan induk dan anak untuk satu badan hukum | `ChartOfAccount : Read` | `legalEntityId` pada query | `ApiResponse<List<ChartOfAccountTreeDto>>` |
| `GET` | `/options` | Daftar ringkas akun yang menerima transaksi dan aktif, untuk isian pilihan pada form jurnal | `ChartOfAccount : Read` | `legalEntityId`, `search` pada query | `ApiResponse<List<ChartOfAccountOptionDto>>` |
| `POST` | `/` | Menambah akun baru | `ChartOfAccount : Create` | `CreateChartOfAccountDto` | `ApiResponse<ChartOfAccountDetailDto>` |
| `PUT` | `/{id}` | Mengubah kode, nama, induk, tingkat, atau keterangan akun. **Kode hanya dapat diubah selama akun belum dipakai jurnal yang disahkan** (`ACC-DEC-042`) | `ChartOfAccount : Update` | `UpdateChartOfAccountDto` | `ApiResponse<ChartOfAccountDetailDto>` |
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

### `ACC-DEC-042` — kode akun dapat diubah selama belum dipakai

Versi `0.1` menulis `PUT` hanya mengubah "nama, induk, atau keterangan", sehingga tampak seolah
`AccountCode` tidak pernah dapat diubah. Itu bertentangan dengan dua artefak lain yang sama-sama
canonical:

| Artefak | Yang dikatakan |
|---|---|
| `contracts/validation-matrix.md` bagian 1 | "Kode tidak berubah setelah dipakai — **Ubah** — `AccountCode` diubah padahal sudah ada baris jurnal disahkan" ⇒ `409` |
| `roadmap/backend-roadmap.md` acceptance `BE-ACC-007` (4) | "Kode akun bertransaksi **gagal diubah**" |

Keduanya hanya punya isi bila kode **dapat** diubah pada keadaan lain. Aturan validasi yang
melarang sesuatu yang memang tidak pernah mungkin adalah aturan kosong, dan acceptance yang
mengujinya akan selalu lulus tanpa membuktikan apa pun.

Owner memutuskan mengikuti bacaan validation matrix: **`AccountCode` dapat diubah selama akun
belum dipakai baris jurnal yang disahkan.** Deskripsi `PUT` di atas diperbaiki mengikuti keputusan
itu, dan `UpdateChartOfAccountDto` memuat `AccountCode`.

Jurnal `Draft` **tidak** mengunci kode — ia belum menjadi transaksi. Dibuktikan
`ChartOfAccountServiceTests.JurnalDraft_TidakMenguncikanAkun`.

Endpoint `/options` sengaja hanya mengembalikan akun yang **menerima transaksi dan aktif**.
Dengan begitu petugas tidak pernah melihat akun induk pada daftar pilihan, dan `ACC-DEC-022`
terjaga sejak di layar, bukan hanya saat penyimpanan.

---

## Corporate / Accounting / Master Data / Journal Type

Base URL: `api/v1/corporate/accounting/master-data/journal-types`
Contract version: `ACC-API-0.3` — status `approved`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/` | Menampilkan daftar jenis jurnal | `JournalType : Read` | `JournalTypePagedQuery` | `ApiResponse<PagedResult<JournalTypeDto>>` |
| `GET` | `/options` | Daftar ringkas jenis jurnal aktif untuk isian pilihan | `JournalType : Read` | — | `ApiResponse<List<JournalTypeOptionDto>>` |
| `POST` | `/` | Menambah jenis jurnal | `JournalType : Create` | `CreateJournalTypeDto` | `ApiResponse<JournalTypeDto>` |
| `PUT` | `/{id}` | Mengubah jenis jurnal | `JournalType : Update` | `UpdateJournalTypeDto` | `ApiResponse<JournalTypeDto>` |
| `POST` | `/seed` | Mengisi empat jenis jurnal bawaan sistem — `JU`, `JP`, `JB`, `SA`. Aman diulang: pemanggilan kedua menyisipkan nol baris | `JournalType : Create` | — | `ApiResponse<AccountingMasterDataSeedResult>` |

Arti kode status bagi pengguna:

- `400` — isian tidak lengkap, misalnya awalan nomor kosong.
- `409` — kode jenis jurnal sudah dipakai, atau jenis jurnal sistem hendak diubah kode maupun
  awalan nomornya.

`POST /seed` diratifikasi owner 3 September 2026 (`ACC-TD-013`). Ia dibangun `BE-ACC-008` sebagai
call site seeder `BE-ACC-006`, dan sengaja tidak diletakkan di `Program.cs` — `02-backend-architecture.md`
bagian 6 melarangnya — maupun disembunyikan di jalur `GET`, yang akan mengaburkan siapa mengisi
master dan kapan.

Jenis jurnal `JB` dan `SA` bertanda sistem. Keduanya tidak dapat dihapus, dan kode maupun awalan
nomornya tidak dapat diubah, karena dipakai langsung oleh proses pembalikan dan saldo awal.

---

## Corporate / Accounting / Journal Management / Journal

Base URL: `api/v1/corporate/accounting/journals`
Contract version: `ACC-API-0.3` — status `approved`

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
Contract version: `ACC-API-0.3` — status `approved`

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
Contract version: `ACC-API-0.3` — status `approved`

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

**Diratifikasi 4 September 2026 terhadap source `822d48a`** — `ACC-GAP-004`. Sebelum ini, daftar
di bawah memakai akhiran `Dto` dan sejumlah field tidak cocok dengan kode; frontend yang menyusun
klien dari kontrak ini akan salah. Nama dan field berikut kini **disalin dari source**, bukan
sebaliknya: source adalah bukti otoritatif atas perilaku runtime. Nol baris source diubah.

Konvensi penamaan yang berlaku: masukan bernama `...Request`, keluaran bernama `...Response`,
parameter daftar bernama `...Query`.

### Master Data — Chart of Account

| Nama class | Jenis | Field |
|---|---|---|
| `ChartOfAccountPagedQuery` | Query | `PageNumber`, `PageSize`, `LegalEntityId?`, `AccountType?`, `IsActive?`, `IsPostable?`, `Search?`, `SortBy?`, `SortDirection?` |
| `ChartOfAccountListResponse` | Response | `Id`, `LegalEntityId`, `AccountCode`, `AccountName`, `AccountType`, `NormalBalance`, `AccountLevel`, `ParentAccountId?`, `ParentAccountCode?`, `IsPostable`, `IsActive` |
| `ChartOfAccountDetailResponse` | Response | **Mewarisi `ChartOfAccountListResponse`**, ditambah `ParentAccountName?`, `Description?`, `EffectiveStartDate?`, `HasChildAccounts`, `HasPostedJournalLines`, `RequiresCostCenter`, `CreateDateTime` |
| `ChartOfAccountTreeResponse` | Response | `Id`, `AccountCode`, `AccountName`, `AccountType`, `NormalBalance`, `AccountLevel`, `IsPostable`, `IsActive`, `Children` |
| `ChartOfAccountOptionResponse` | Option | `Id`, `AccountCode`, `AccountName`, `AccountType`, `NormalBalance`, `RequiresCostCenter` |
| `CreateChartOfAccountRequest` | Create | `LegalEntityId`, `AccountCode`, `AccountName`, `AccountType`, `NormalBalance`, `ParentAccountId?`, `AccountLevel`, `IsPostable`, `Description?`, `EffectiveStartDate?` |
| `UpdateChartOfAccountRequest` | Update | `AccountCode`, `AccountName`, `ParentAccountId?`, `AccountLevel`, `IsPostable`, `Description?`, `EffectiveStartDate?` |
| `DeactivateChartOfAccountRequest` | Status | `Reason?` |

> `UpdateChartOfAccountRequest` **tidak memuat `NormalBalance`.** Saldo normal tidak dapat diubah
> sesudah akun dibuat. Daftar sebelumnya mencantumkannya — itu keliru.

### Master Data — Journal Type

| Nama class | Jenis | Field |
|---|---|---|
| `JournalTypePagedQuery` | Query | `PageNumber`, `PageSize`, `IsActive?`, `IsSystemType?`, `Search?`, `SortBy?`, `SortDirection?` |
| `JournalTypeResponse` | Response | `Id`, `JournalTypeCode`, `JournalTypeName`, `NumberPrefix`, `RequiresApproval`, `IsSystemType`, `IsActive`, `HasJournals`, `CreateDateTime` |
| `JournalTypeOptionResponse` | Option | `Id`, `JournalTypeCode`, `JournalTypeName`, `NumberPrefix`, `RequiresApproval` |
| `CreateJournalTypeRequest` | Create | `JournalTypeCode`, `JournalTypeName`, `NumberPrefix` |
| `UpdateJournalTypeRequest` | Update | `JournalTypeCode`, `JournalTypeName`, `NumberPrefix`, `IsActive` |
| `JournalTypeSeedResponse` | Response | `Inserted`, `Skipped`, `SkippedReason?`, `Items` |

> `RequiresApproval` **tidak dapat dikirim** pada Create maupun Update — dikunci `true` oleh
> `ACC-TD-019`. Ia hanya muncul sebagai keluaran. Begitu pula `IsSystemType`, yang hanya lahir
> dari data master awal.

### Journal Management

| Nama class | Jenis | Field |
|---|---|---|
| `JournalPagedQuery` | Query | `PageNumber`, `PageSize`, `LegalEntityId?`, `DateFrom?`, `DateTo?`, `JournalTypeId?`, `JournalStatus?`, `Search?`, `SortBy?`, `SortDirection?` |
| `JournalListResponse` | Response | `Id`, `JournalNumber`, `AccountingDate`, `JournalTypeId`, `JournalTypeName`, `Description`, `JournalStatus`, `TotalDebit`, `TotalCredit` |
| `JournalDetailResponse` | Response | `Id`, `LegalEntityId`, `JournalNumber`, `JournalTypeId`, `JournalTypeCode`, `JournalTypeName`, `AccountingPeriodId`, `PeriodCode`, `DocumentNumber?`, `DocumentDate?`, `AccountingDate`, `Description`, `JournalStatus`, `TotalDebit`, `TotalCredit`, `IsBalanced`, `SubmittedBy?`, `SubmittedAt?`, `ApprovedBy?`, `ApprovedAt?`, `PostedBy?`, `PostedAt?`, `RejectionReason?`, `ReversalOfJournalId?`, `ReversalOfJournalNumber?`, `CorrectionType?`, `CreateDateTime`, `CreateBy`, `Lines`, `Approvals`, `AvailableActions` |
| `JournalLineResponse` | Response | `Id`, `LineNumber`, `AccountId`, `AccountCode`, `AccountName`, `CostCenterId?`, `CostCenterName?`, `Description?`, `DebitAmount`, `CreditAmount` |
| `JournalApprovalResponse` | Response | `ApprovalAction`, `ActionBy`, `ActionAt`, `Reason?` |
| `CreateJournalRequest` | Create | `LegalEntityId`, `JournalTypeId`, `DocumentNumber?`, `DocumentDate?`, `AccountingDate`, `Description`, `Lines` |
| `UpdateJournalRequest` | Update | Sama dengan `CreateJournalRequest` **tanpa** `LegalEntityId` |
| `CreateJournalLineRequest` | Create | `LineNumber`, `AccountId`, `CostCenterId?`, `Description?`, `DebitAmount`, `CreditAmount` |
| `RejectJournalRequest` | Status | `Reason` |
| `ReverseJournalRequest` | Status | `CorrectionType?`, `Reason`, `AccountingDate?`, `AdjustmentLines` |

> **`JournalApprovalResponse.ActionBy` adalah `Guid`, bukan nama.** Daftar sebelumnya menyebut
> `ActionByName`, dan **field itu tidak ada di source mana pun** — dicari di seluruh
> `Areas/Corporate/AccountingManagement/`, nol kemunculan. Layar rincian jurnal (`FE-ACC-007`)
> karena itu **tidak dapat menampilkan nama penyetuju** dari endpoint ini saja. Dicatat sebagai
> `ACC-GAP-011`.

### Accounting Period

| Nama class | Jenis | Field |
|---|---|---|
| `AccountingPeriodPagedQuery` | Query | `PageNumber`, `PageSize`, `LegalEntityId?`, `FiscalYear?`, `PeriodStatus?`, `SortDirection?` |
| `AccountingPeriodResponse` | Response | `Id`, `LegalEntityId`, `PeriodCode`, `FiscalYear`, `PeriodMonth`, `StartDate`, `EndDate`, `PeriodStatus`, `PeriodName`, `ClosedBy?`, `ClosedAt?`, `ReopenedBy?`, `ReopenedAt?`, `LastReasonNote?`, `AcceptedJournalTypeCodes` |
| `GenerateAccountingPeriodRequest` | Create | `LegalEntityId`, `FiscalYear` |
| `ClosePeriodRequest` | Status | `Permanent`, `Reason?` |
| `ReopenPeriodRequest` | Status | `Reason` |

> `ClosePeriodRequest` memakai **`Permanent` bertipe `bool`**, bukan `CloseType`. Daftar
> sebelumnya keliru.
>
> `AccountingPeriodResponse` **tidak memuat `AvailableActions`** — inilah `ACC-GAP-010` yang
> menahan acceptance (4) `FE-ACC-004`. Belum diputuskan owner, dan **tidak** diubah di sini.

### General Ledger

| Nama class | Jenis | Field |
|---|---|---|
| `LedgerMovementQuery` | Query | `PageNumber`, `PageSize`, `LegalEntityId`, `AccountId`, `DateFrom?`, `DateTo?` |
| `LedgerMovementResponse` | Response | `AccountingDate`, `JournalNumber`, `LineNumber`, `Description`, `DebitAmount`, `CreditAmount`, `RunningBalance` |
| `TrialBalanceQuery` | Query | `LegalEntityId`, `PeriodCode` |
| `TrialBalanceResponse` | Response | `PeriodCode`, `PeriodName`, `Rows`, `TotalDebit`, `TotalCredit`, `IsBalanced` |
| `TrialBalanceRowResponse` | Response | `AccountId`, `AccountCode`, `AccountName`, `OpeningBalance`, `TotalDebit`, `TotalCredit`, `ClosingBalance` |
| `AccountBalanceResponse` | Response | `AccountId`, `AccountCode`, `AccountName`, `PeriodCode`, `PeriodName`, `OpeningBalance`, `TotalDebit`, `TotalCredit`, `ClosingBalance` |

### Catatan yang tetap berlaku

`RequiresCostCenter` pada DTO akun **diturunkan** dari `AccountType == Expense`, bukan dibaca
dari kolom tabel. Ia disertakan agar frontend tahu kapan harus mewajibkan pengisian Cost Center
tanpa perlu menghafal aturannya sendiri.

`AvailableActions` pada `JournalDetailResponse` berisi daftar tindakan yang boleh dilakukan
pengguna saat itu, dihitung backend dari status jurnal, hak akses, dan aturan `ACC-DEC-016`.
Frontend menampilkan tombol berdasarkan daftar ini — bukan menghitung sendiri. Backend tetap
memeriksa ulang saat tindakannya benar-benar dijalankan.

### Ringkasan selisih yang diperbaiki ratifikasi ini

| # | Selisih | Kelompok |
|---:|---|---|
| 1 | Seluruh nama berakhiran `Dto` → `Request`/`Response`/`Query` | Semua |
| 2 | `RequiresApproval` dicabut dari Create/Update | Journal Type |
| 3 | `UpdateJournalTypeRequest.IsActive` ditambahkan | Journal Type |
| 4 | `HasJournals`, `CreateDateTime` ditambahkan | Journal Type |
| 5 | `JournalTypeOptionResponse` + `NumberPrefix`, `RequiresApproval` | Journal Type |
| 6 | `JournalTypeSeedResponse` sebelumnya tidak terdaftar sama sekali | Journal Type |
| 7 | `ChartOfAccountListResponse` + `LegalEntityId`, `NormalBalance`, `ParentAccountId`, `ParentAccountCode` | COA |
| 8 | `ChartOfAccountDetailResponse` mewarisi List + `EffectiveStartDate`, `HasChildAccounts`, `HasPostedJournalLines`, `CreateDateTime` | COA |
| 9 | `ChartOfAccountTreeResponse` + `AccountType`, `NormalBalance`, `AccountLevel`, `IsActive` | COA |
| 10 | `ChartOfAccountOptionResponse` + `NormalBalance` | COA |
| 11 | `AccountLevel` ditambahkan pada Create dan Update | COA |
| 12 | `NormalBalance` **dihapus** dari Update — tidak dapat diubah | COA |
| 13 | `EffectiveStartDate` ditambahkan pada Update | COA |
| 14 | `JournalPagedQuery` + `SortBy`, `SortDirection` | Journal |
| 15 | `JournalListResponse` + `JournalTypeId` | Journal |
| 16 | `JournalDetailResponse` dijabarkan penuh — 15 field sebelumnya tidak tercantum | Journal |
| 17 | `JournalLineResponse` + `Id` | Journal |
| 18 | `ActionByName` **tidak ada**; yang ada `ActionBy` bertipe `Guid` — `ACC-GAP-011` | Journal |
| 19 | `AccountingPeriodPagedQuery` + `SortDirection` | Period |
| 20 | `AccountingPeriodResponse` + `LegalEntityId`, `PeriodName`, `ClosedBy`, `ReopenedBy`, `AcceptedJournalTypeCodes` | Period |
| 21 | `ClosePeriodRequest` memakai `Permanent: bool`, bukan `CloseType` | Period |
| 22 | `LedgerMovementResponse` + `LineNumber` | GL |
| 23 | `TrialBalanceResponse` + `PeriodName`; `TrialBalanceRowResponse` + `AccountId` | GL |
| 24 | `AccountBalanceResponse` + `AccountId`, `PeriodName` | GL |

`ACC-GAP-004` mencatat lima selisih. Pemeriksaan penuh terhadap source menemukan **24**.
