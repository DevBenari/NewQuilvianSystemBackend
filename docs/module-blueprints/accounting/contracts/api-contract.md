# Accounting — API Contract

| Field | Value |
|---|---|
| `contract_version` | `ACC-API-0.5` |
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
| `PUT` | `/{id}` | Mengubah kode, nama, induk, tingkat, keterangan, atau **penanda control account** akun. **Kode hanya dapat diubah selama akun belum dipakai jurnal yang disahkan** (`ACC-DEC-042`) | `ChartOfAccount : Update` | `UpdateChartOfAccountDto` | `ApiResponse<ChartOfAccountDetailDto>` |
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

### `ACC-DEC-064` — penanda control account, 9 September 2026

`CreateChartOfAccountDto` dan `UpdateChartOfAccountDto` bertambah satu bidang:

| Bidang | Tipe | Wajib | Bawaan | Keterangan |
|---|---|:---:|---|---|
| `IsControlAccount` | `bool` | Tidak | `false` | Akun bertanda ini **menolak baris jurnal manual**; pencatatannya hanya sah lewat kejadian akuntansi atau subledger |

`ChartOfAccountDetailDto` dan `ChartOfAccountListDto` ikut memuatnya supaya layar COA dapat
menampilkan penandanya, dan supaya Form Jurnal dapat menyaring akun control dari pemilihnya.

**Ini perubahan pada kontrak Phase 1**, dikerjakan sebagai bagian Phase 2 lewat `BE-ACC-P2-011`.
Nol endpoint bertambah; hanya bidang pada DTO yang sudah ada.

Jurnal `Draft` **tidak** mengunci kode — ia belum menjadi transaksi. Dibuktikan
`ChartOfAccountServiceTests.JurnalDraft_TidakMenguncikanAkun`.

Endpoint `/options` sengaja hanya mengembalikan akun yang **menerima transaksi dan aktif**.
Dengan begitu petugas tidak pernah melihat akun induk pada daftar pilihan, dan `ACC-DEC-022`
terjaga sejak di layar, bukan hanya saat penyimpanan.

### `IsControlAccount` pada `/options` dan `bool?` pada Ubah — amandemen menunggu ratifikasi

> **`ACC-API-0.11` (usulan), 11 September 2026.** Dua perubahan pada grup ini. Status `approved`
> grup tidak diubah.

**(1) `ChartOfAccountOptionResponse` bertambah `IsControlAccount`.** Bagian `ACC-DEC-064` di atas
menyebut Detail dan List, tetapi tidak menyebut Option — padahal Form Jurnal mengisi pemilih
akunnya dari `/options`, bukan dari daftar akun. Tanpa bidang ini acceptance (3) `FE-ACC-P2-007`,
yaitu menyembunyikan atau mematikan akun control di Form Jurnal, mustahil dipenuhi.
`BE-ACC-P2-011` sengaja menunda bidang ini ke `BE-ACC-P2-012`.

**`/options` TIDAK menyaring akun control.** Ia tetap mengembalikan seluruh akun yang menerima
transaksi dan aktif, lengkap dengan penandanya. Enam layar memakai endpoint ini — Form Jurnal,
pembalikan jurnal, Form Jurnal Berulang, Buku Besar, Pengaturan Akuntansi, dan Form Akun — dan
**Buku Besar justru harus tetap dapat memilih Kas Kasir**. Penyaringan dilakukan tiap layar menurut
kebutuhannya, bukan di backend.

**(2) `UpdateChartOfAccountRequest.IsControlAccount` bertipe `bool?`, bukan `bool`.** Tabel
`ACC-DEC-064` di atas menulis `bool` berbawaan `false` tanpa membedakan Create dan Update. Untuk
Update itu tidak lagi benar sejak 9 September 2026: **kosong berarti nilai tersimpan
dipertahankan**, dan melepas penanda menuntut `false` yang tegas. Sebabnya, permintaan lama yang
tidak mengenal bidang ini akan mengirim `false` diam-diam dan membuka kembali akun kas ke jurnal
manual hanya karena seseorang mengubah nama akunnya. Tercatat pada kartu `BE-ACC-P2-011` baris
*Peringatan DITUTUP*, tetapi belum pernah menyeberang ke kontrak. **Yang benar adalah kode.**

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

> **Usulan `ACC-API-0.11` — 11 September 2026.** `422` juga muncul bila baris jurnal manual
> menunjuk control account saat Simpan, Ubah, atau Ajukan (`ACC-DEC-064`), dan bila **baris
> penyesuaian** `JP` menunjuk control account (`ACC-DEC-072`). Pembalikan penuh `JB` tidak terkena.
> Pesannya menyebut akunnya. Rincian jalur yang terkena dan yang tidak ada di
> `validation-matrix.md` bagian 3b Phase 2.

> **Usulan `ACC-API-0.11` — penegasan kode status `reverse`, 11 September 2026.**
> `POST /{id}/reverse` yang berhasil menjawab **`200`**, bukan `201`, walaupun ia membuat jurnal
> pembalik atau penyesuaian baru. Itu perilaku sejak `BE-ACC-013` dan sudah tercatat di laporannya;
> bagian ini hanya menuliskannya eksplisit. Diputuskan Rizki atas temuan Skenario 6
> `BE-ACC-P2-012`. **Kode tidak diubah.**

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
| `ChartOfAccountListResponse` | Response | `Id`, `LegalEntityId`, `AccountCode`, `AccountName`, `AccountType`, `NormalBalance`, `AccountLevel`, `ParentAccountId?`, `ParentAccountCode?`, `IsPostable`, `IsActive`, `IsControlAccount` |
| `ChartOfAccountDetailResponse` | Response | **Mewarisi `ChartOfAccountListResponse`**, ditambah `ParentAccountName?`, `Description?`, `EffectiveStartDate?`, `HasChildAccounts`, `HasPostedJournalLines`, `RequiresCostCenter`, `CreateDateTime` |
| `ChartOfAccountTreeResponse` | Response | `Id`, `AccountCode`, `AccountName`, `AccountType`, `NormalBalance`, `AccountLevel`, `IsPostable`, `IsActive`, `Children` |
| `ChartOfAccountOptionResponse` | Option | `Id`, `AccountCode`, `AccountName`, `AccountType`, `NormalBalance`, `RequiresCostCenter`, **`IsControlAccount`** *(usulan `ACC-API-0.11`)* |
| `CreateChartOfAccountRequest` | Create | `LegalEntityId`, `AccountCode`, `AccountName`, `AccountType`, `NormalBalance`, `ParentAccountId?`, `AccountLevel`, `IsPostable`, `IsControlAccount`, `Description?`, `EffectiveStartDate?` |
| `UpdateChartOfAccountRequest` | Update | `AccountCode`, `AccountName`, `ParentAccountId?`, `AccountLevel`, `IsPostable`, `IsControlAccount?`, `Description?`, `EffectiveStartDate?` |
| `DeactivateChartOfAccountRequest` | Status | `Reason?` |

> `UpdateChartOfAccountRequest` **tidak memuat `NormalBalance`.** Saldo normal tidak dapat diubah
> sesudah akun dibuat. Daftar sebelumnya mencantumkannya — itu keliru.
>
> `IsControlAccount` pada List, Create, dan Update ditetapkan `ACC-DEC-064` (grup Chart of Account
> di atas) tetapi belum pernah dimasukkan ke tabel ini — disinkronkan 11 September 2026, tanpa
> mengubah isi keputusannya. Pada Update bertipe `bool?`; pada Option, bidangnya **usulan
> `ACC-API-0.11`**.

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
| `JournalDetailResponse` | Response | `Id`, `LegalEntityId`, `JournalNumber`, `JournalTypeId`, `JournalTypeCode`, `JournalTypeName`, `AccountingPeriodId`, `PeriodCode`, `DocumentNumber?`, `DocumentDate?`, `AccountingDate`, `Description`, `JournalStatus`, `TotalDebit`, `TotalCredit`, `IsBalanced`, `SubmittedBy?`, **`SubmittedByName?`**, `SubmittedAt?`, `ApprovedBy?`, **`ApprovedByName?`**, `ApprovedAt?`, `PostedBy?`, **`PostedByName?`**, `PostedAt?`, `RejectionReason?`, `ReversalOfJournalId?`, `ReversalOfJournalNumber?`, `CorrectionType?`, `CreateDateTime`, `CreateBy`, `Lines`, `Approvals`, `AvailableActions` |
| `JournalLineResponse` | Response | `Id`, `LineNumber`, `AccountId`, `AccountCode`, `AccountName`, `CostCenterId?`, `CostCenterName?`, `Description?`, `DebitAmount`, `CreditAmount` |
| `JournalApprovalResponse` | Response | `ApprovalAction`, `ActionBy`, **`ActionByName?`**, `ActionAt`, `Reason?` |
| `CreateJournalRequest` | Create | `LegalEntityId`, `JournalTypeId`, `DocumentNumber?`, `DocumentDate?`, `AccountingDate`, `Description`, `Lines` |
| `UpdateJournalRequest` | Update | Sama dengan `CreateJournalRequest` **tanpa** `LegalEntityId` |
| `CreateJournalLineRequest` | Create | `LineNumber`, `AccountId`, `CostCenterId?`, `Description?`, `DebitAmount`, `CreditAmount` |
| `RejectJournalRequest` | Status | `Reason` |
| `ReverseJournalRequest` | Status | `CorrectionType?`, `Reason`, `AccountingDate?`, `AdjustmentLines` |

> **`ActionByName` dan ketiga `…ByName` ditambahkan `ACC-API-0.5`** (`BE-ACC-015`, menutup
> `ACC-GAP-011`). Sebelumnya hanya `Guid` yang tersedia, sehingga layar rincian tidak dapat
> menjawab *"siapa menyetujui"* — padahal itu alasan riwayat persetujuan ada.
>
> Keempatnya **tidak dipersistensi**: nama diambil dari `AspNetUsers` saat baca, dalam satu
> kueri batch per jurnal. Menyimpannya pada baris riwayat melanggar `QBE-ENT-003` dan akan basi
> begitu pengguna berganti nama. Nilainya `null` bila aktornya belum ada — misalnya
> `ApprovedByName` pada jurnal yang belum disetujui.

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


---

# PHASE 2 (`ACC-PH-006`) — Rencana, belum tersedia

| Field | Nilai |
|---|---|
| `contract_version` | `ACC-API-0.10` |
| `last_changed_in` | `ACC-API-0.10` — 11 September 2026, penyelarasan grup Recurring Journal dengan `RecurringJournalController`: hak akses `activate`/`deactivate` menjadi `RecurringJournal : Activate`, empat bidang ringkasan, dan label ketersediaan. Sebelumnya `0.9` — 10 September 2026, penyelarasan grup Accounting Period dengan `AccountingPeriodController` yang sudah berdiri: base URL `periods` (bukan `accounting-periods`), hak akses `AccountingPeriod` (bukan `Period`), endpoint `GET /{id}/closing-history`, dan empat bidang keadaan daftar periksa. Ditambah `ACC-DEC-070`, peringatan keenam. Sebelumnya `0.8` (`ACC-DEC-060`, `CorrelationId` dan `CausationId` sebagai bidang wajib ke-11 dan ke-12), `0.7` (`ACC-DEC-058`, aturan posting daftar baris), dan `0.6` (33 endpoint Phase 2) |
| Amandemen menunggu ratifikasi | **`ACC-API-0.11` (usulan) — 11 September 2026.** Lima perubahan: (1) grup **Reconciliation** dicatat untuk pertama kali — `GET /reconciliation/gl-balances`; (2) `ChartOfAccountOptionResponse` bertambah `IsControlAccount`, dan `UpdateChartOfAccountRequest.IsControlAccount` dicatat sebagai `bool?`; (3) `422` control account pada grup Journal — Simpan, Ubah, Ajukan, dan penyesuaian `JP` (`ACC-DEC-064`, `ACC-DEC-072`); (4) `422` control account pada grup Recurring Journal — Tambah, Ubah, Aktifkan (`ACC-DEC-073`); (5) penegasan bahwa `POST /journals/{id}/reverse` yang berhasil menjawab **`200`**, bukan `201` — perilaku sejak `BE-ACC-013`, dituliskan eksplisit atas keputusan Rizki 11 September 2026 (Skenario 6 `BE-ACC-P2-012`); kode tidak diubah. **Status `approved` di bawah belum diubah** — menunggu ratifikasi Rizki |
| Status | **`approved`** — Rizki, 11 September 2026 |
| `approved_by` / `approved_at` | Rizki / 11 September 2026 (ratifikasi `ACC-API-0.10`); sebelumnya Rizki / 10 September 2026 (`ACC-API-0.9`) dan Rizki / 8 September 2026 (`ACC-API-0.8`) |
| `input_revision` | `00-interview-decisions.md@6`, `02-backend-architecture.md@4`, `evidence/09` `ACC-DOMAIN-P2-0.1` |
| Traceability | `ACC-DEC-044` sampai `ACC-DEC-057`, ditambah `ACC-DEC-065` dan `ACC-DEC-070` |
| Dampak kompatibilitas | Seluruhnya endpoint **baru**, ditambah **lima endpoint baru** pada grup Accounting Period yang sudah ada. **Nol endpoint existing berubah bentuk maupun rusak** |

**Sebagian besar endpoint di bawah berlabel `Rencana (belum tersedia)`.** Jangan menyangka yang
berlabel demikian sudah bisa dipakai.

**Kecualinya grup Accounting Period.** Kelima endpoint barunya — `closing-checklist`,
`closing-history`, `submit-closing`, `approve-closing`, `reject-closing` — **sudah berdiri** lewat
`BE-ACC-P2-005` dan `BE-ACC-P2-006`, dan labelnya sudah diperbarui pada bagiannya. Kalimat
sebelumnya berbunyi "tidak satu pun sudah berdiri di `02c3219`", benar saat ditulis dan sudah
tidak benar sejak kedua task itu selesai.

Grup Recurring Journal (`ACC-API-0.10`) dan Reconciliation (usulan `ACC-API-0.11`) juga sudah
berdiri; labelnya tercatat pada bagian masing-masing.

Amplop respons tetap `ApiResponse<T>`, daftar berhalaman tetap `PagedResult<T>`, sama seperti MVP.

## Corporate / Accounting / Accounting Event

`[Tags("Corporate - Accounting - Accounting Event")]`
Base URL: `api/v1/corporate/accounting/accounting-events` — **Rencana (belum tersedia)**

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/` | Daftar kejadian berhalaman, dapat disaring per status, jenis kejadian, periode, dan badan hukum | `AccountingEvent : Read` | `AccountingEventPagedQuery` | `ApiResponse<PagedResult<AccountingEventListDto>>` |
| `GET` | `/{id}` | Rincian satu kejadian beserta seluruh riwayat percobaannya dan jurnal yang dihasilkan | `AccountingEvent : Read` | — | `ApiResponse<AccountingEventDetailDto>` |
| `GET` | `/summary` | Jumlah kejadian per status, untuk penanda angka pada menu (`ACC-DEC-057`) | `AccountingEvent : Read` | `legalEntityId` pada query | `ApiResponse<AccountingEventSummaryDto>` |
| `POST` | `/` | **Menerima satu kejadian keuangan dari Finance.** Inilah pintu masuk Phase 2 | `AccountingEvent : Receive` | `ReceiveAccountingEventRequest` | `ApiResponse<AccountingEventReceiptDto>` |
| `POST` | `/{id}/retry` | Mencoba ulang kejadian berstatus Gagal secara manual | `AccountingEvent : Retry` | — | `ApiResponse<AccountingEventDetailDto>` |
| `PATCH` | `/{id}/ignore` | Menandai kejadian Gagal sebagai Diabaikan; **alasan tertulis wajib** | `AccountingEvent : Ignore` | `IgnoreAccountingEventRequest` | `ApiResponse<AccountingEventDetailDto>` |

Arti kode status bagi pengguna:

- `200` — permintaan berhasil.
- `201` — kejadian baru diterima dan dicatat.
- `200` **pada `POST /` untuk kejadian yang sudah pernah diterima** — ini bukan kesalahan.
  Sistem mengembalikan nomor jurnal yang sama tanpa membuat jurnal baru (`ACC-DEC-035`).
- `400` — pesan tidak memuat kedua belas bidang wajib (`ACC-DEC-048` dan `ACC-DEC-060`), atau nilainya tidak masuk akal.
- `403` — pengguna tidak berhak, atau badan hukum yang dituju bukan haknya.
- `404` — kejadian tidak ditemukan.
- `409` — mata uang bukan rupiah (`ACC-DEC-020`), atau kejadian hendak diabaikan padahal statusnya
  bukan Gagal.
- `422` — kejadian sah tetapi jenisnya belum punya aturan posting. Kejadian tercatat berstatus
  **Tertahan**, dan **tidak ada jurnal yang dibuat** (`ACC-DEC-046`).

### Kenapa `POST /` mengembalikan `200` untuk kiriman ulang, bukan `409`

Ini sengaja, dan mudah salah dirancang. Finance yang mengirim ulang karena jaringan putus
**bukan** sedang melakukan kesalahan — ia sedang memastikan pesannya sampai. Menjawab `409` akan
membuat Finance menganggap pengirimannya gagal lalu mencoba lagi tanpa henti. Menjawab `200`
beserta nomor jurnal yang sama membuat Finance tahu pesannya **sudah diterima dan sudah dibukukan**.

**Contohnya.** `EVT-100` dikirim tiga kali. Kiriman pertama menjawab `201` dan nomor jurnal
`JU/2026/09/00042`. Kiriman kedua dan ketiga menjawab `200` dengan nomor jurnal yang sama persis.
Buku besar tetap berisi satu catatan.

## Corporate / Accounting / Master Data / Event Type

`[Tags("Corporate - Accounting - Master Data - Event Type")]`
Base URL: `api/v1/corporate/accounting/event-types` — **Rencana (belum tersedia)**

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/` | Daftar jenis kejadian berhalaman | `EventType : Read` | `EventTypePagedQuery` | `ApiResponse<PagedResult<EventTypeListDto>>` |
| `GET` | `/{id}` | Rincian satu jenis kejadian | `EventType : Read` | — | `ApiResponse<EventTypeDetailDto>` |
| `GET` | `/options` | Daftar ringkas jenis kejadian aktif, untuk isian pilihan pada form aturan posting | `EventType : Read` | `search` pada query | `ApiResponse<List<EventTypeOptionDto>>` |
| `POST` | `/` | Menambah jenis kejadian | `EventType : Create` | `CreateEventTypeRequest` | `ApiResponse<EventTypeDetailDto>` |
| `PUT` | `/{id}` | Mengubah nama dan modul asal | `EventType : Update` | `UpdateEventTypeRequest` | `ApiResponse<EventTypeDetailDto>` |
| `PATCH` | `/{id}/deactivate` | Menonaktifkan jenis kejadian | `EventType : Update` | — | `ApiResponse<EventTypeDetailDto>` |

`409` muncul bila kode jenis kejadian sudah dipakai, atau bila jenis hendak dinonaktifkan padahal
masih ada aturan posting aktif yang memakainya.

## Corporate / Accounting / Master Data / Posting Rule

`[Tags("Corporate - Accounting - Master Data - Posting Rule")]`
Base URL: `api/v1/corporate/accounting/posting-rules` — **Rencana (belum tersedia)**

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/` | Daftar aturan posting berhalaman, disaring per badan hukum dan jenis kejadian | `PostingRule : Read` | `PostingRulePagedQuery` | `ApiResponse<PagedResult<PostingRuleListDto>>` |
| `GET` | `/{id}` | Rincian satu aturan posting | `PostingRule : Read` | — | `ApiResponse<PostingRuleDetailDto>` |
| `POST` | `/` | Menambah aturan posting | `PostingRule : Create` | `CreatePostingRuleRequest` | `ApiResponse<PostingRuleDetailDto>` |
| `PUT` | `/{id}` | Mengubah akun debit, akun kredit, atau perlakuannya | `PostingRule : Update` | `UpdatePostingRuleRequest` | `ApiResponse<PostingRuleDetailDto>` |
| `PATCH` | `/{id}/deactivate` | Menonaktifkan aturan posting | `PostingRule : Update` | — | `ApiResponse<PostingRuleDetailDto>` |

`409` muncul bila jenis kejadian itu sudah punya aturan aktif pada badan hukum yang sama, atau
bila ada baris berakun dari badan hukum yang berbeda, atau bila aturannya tidak akan pernah seimbang.
`422` muncul bila akun yang ditunjuk bukan akun yang menerima transaksi (`ACC-DEC-022`).

## Corporate / Accounting / Recurring Journal

`[Tags("Corporate / Accounting / Recurring Journal")]`
Base URL: `api/v1/corporate/accounting/recurring-journals` — kedelapan endpoint **sudah berdiri**
lewat `BE-ACC-P2-007` dan `BE-ACC-P2-008`, terbukti `RecurringJournalController` pada `rizkiG`

> **Diratifikasi Rizki, 11 September 2026 — `ACC-API-0.10`.** Hak akses `activate` dan `deactivate`
> diubah dari `RecurringJournal : Update` menjadi **`RecurringJournal : Activate`**, empat bidang
> ringkasan dicatat, dan label ketersediaan diperbarui. **Untuk ketiganya yang benar adalah kode,
> bukan dokumen ini** — delta hak aksesnya sudah dicatat laporan `BE-ACC-P2-007` sejak
> 10 September, hanya belum pernah menyeberang ke kontrak.

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/` | Daftar template berhalaman | `RecurringJournal : Read` | `RecurringJournalPagedQuery` | `ApiResponse<PagedResult<RecurringJournalListDto>>` |
| `GET` | `/{id}` | Rincian template beserta barisnya | `RecurringJournal : Read` | — | `ApiResponse<RecurringJournalDetailDto>` |
| `GET` | `/{id}/runs` | Riwayat penerbitan template per periode beserta jurnal yang dihasilkan | `RecurringJournal : Read` | — | `ApiResponse<List<RecurringJournalRunDto>>` |
| `POST` | `/` | Menambah template beserta barisnya. Template baru **selalu lahir tidak aktif** | `RecurringJournal : Create` | `CreateRecurringJournalRequest` | `ApiResponse<RecurringJournalDetailDto>` |
| `PUT` | `/{id}` | Mengubah template beserta barisnya | `RecurringJournal : Update` | `UpdateRecurringJournalRequest` | `ApiResponse<RecurringJournalDetailDto>` |
| `PATCH` | `/{id}/activate` | Mengaktifkan template | **`RecurringJournal : Activate`** | — | `ApiResponse<RecurringJournalDetailDto>` |
| `PATCH` | `/{id}/deactivate` | Menonaktifkan template | **`RecurringJournal : Activate`** | — | `ApiResponse<RecurringJournalDetailDto>` |
| `POST` | `/{id}/generate` | Menerbitkan jurnal draft untuk satu periode secara manual, di luar jadwal | `RecurringJournal : Generate` | `GenerateRecurringJournalRequest` | `ApiResponse<RecurringJournalRunDto>` |

`400` muncul bila total debit dan kredit baris template tidak sama, bila satu baris berisi debit
**dan** kredit sekaligus, atau bila baris berakun beban tidak menyebut unit biaya.
`409` muncul bila template sudah pernah terbit untuk periode yang diminta — inilah penjaga terbit
ganda — dan juga saat **mengaktifkan** template yang barisnya sudah tidak sah lagi (akun
dinonaktifkan, unit biaya dipindah, atau jenis jurnal dimatikan sesudah template tersimpan).
`422` muncul bila periode yang dituju tidak menerima pencatatan.

> **Usulan `ACC-API-0.11` — 11 September 2026 (`ACC-DEC-073`).** `422` juga muncul pada `POST /`,
> `PUT /{id}`, dan `PATCH /{id}/activate` bila ada baris template yang menunjuk control account,
> dengan pesan yang menyebut akunnya. Pada `activate` kodenya **`422`, bukan `409`** seperti baris
> tidak sah lainnya: ini aturan control account yang sama dengan bagian 3b `validation-matrix.md`,
> dan satu aturan dijawab satu kode di setiap jalurnya. `POST /{id}/generate` dan penjadwal
> **tidak** memeriksanya ulang.

### Hak akses `Activate` terpisah dari `Update`

Bagian ini sebelumnya menulis `RecurringJournal : Update` untuk `activate` dan `deactivate`.
Implementasinya memakai **`RecurringJournal : Activate`**, didaftarkan `[AccessAction]` sebagai
aksi tersendiri di layar Akses Role.

Pemisahannya disengaja: mengaktifkan template jauh lebih berat daripada menyuntingnya. Template
aktif mulai **menulis ke buku besar** pada siklus penjadwal berikutnya, sedangkan menyunting
template yang tidak aktif belum berakibat apa pun. Karena itu keduanya boleh dipegang orang yang
berbeda.

Akibatnya bagi pemasangan: hak `Activate` **harus dicentang tersendiri**. Peran yang hanya punya
`Update` dapat menyunting template tetapi mendapat `403` saat mengaktifkannya.

### Empat bidang ringkasan di luar daftar DTO semula

| Bidang | Letak | Arti |
|---|---|---|
| `LineCount` | daftar | Jumlah baris template |
| `TotalAmount` | daftar | Total nilai debit template |
| `RunCount` | daftar | Berapa kali template pernah terbit. Membedakan template yang belum pernah terbit dari yang rutin terbit |
| `IsBalanced` | rincian | Apakah total debit sama dengan total kredit, dihitung backend |

## Corporate / Accounting / Accounting Period — lima endpoint baru

`[Tags("Corporate - Accounting - Accounting Period")]`
Base URL: `api/v1/corporate/accounting/periods` — grup **sudah ada**; kelima baris di bawah
**sudah berdiri**, terbukti `AccountingPeriodController` pada `rizkiG`

> **Diratifikasi Rizki, 10 September 2026 — `ACC-API-0.9`.** Empat perbaikan pada grup ini: base
> URL `accounting-periods` menjadi `periods`, nama hak akses `Period` menjadi `AccountingPeriod`,
> penambahan `GET /{id}/closing-history`, dan penambahan empat bidang keadaan pada
> `PeriodClosingChecklistDto`. **Untuk keempatnya yang benar adalah kode, bukan dokumen ini.**
> Rinciannya di bawah tabel.

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/{id}/closing-checklist` | Daftar penghalang dan peringatan penutupan, **dihitung saat diminta** | `AccountingPeriod : Read` | — | `ApiResponse<PeriodClosingChecklistDto>` |
| `GET` | `/{id}/closing-history` | Riwayat tindakan penutupan satu periode — ajukan, setujui, tolak — berurut menaik. Barisnya **tidak pernah diubah maupun dihapus** | `AccountingPeriod : Read` | — | `ApiResponse<List<PeriodClosingApprovalDto>>` |
| `POST` | `/{id}/submit-closing` | Accounting Manager mengajukan penutupan periode | `AccountingPeriod : Close` | `SubmitPeriodClosingRequest` | `ApiResponse<AccountingPeriodDetailDto>` |
| `POST` | `/{id}/approve-closing` | **`Accounting Director`** menyetujui penutupan | `AccountingPeriod : Approve` | `ApprovePeriodClosingRequest` | `ApiResponse<AccountingPeriodDetailDto>` |
| `POST` | `/{id}/reject-closing` | Penyetuju menolak pengajuan; alasan tertulis wajib | `AccountingPeriod : Approve` | `RejectPeriodClosingRequest` | `ApiResponse<AccountingPeriodDetailDto>` |

`409` pada `submit-closing` muncul bila masih ada penghalang `ACC-DEC-051` beserta perluasannya
`ACC-DEC-065`: jurnal belum disahkan, kejadian keuangan gagal, atau shift kasir belum ditutup.
`403` pada `approve-closing` muncul bila penyetujunya adalah orang yang mengajukan — penerapan
ulang prinsip empat mata `ACC-DEC-016`.

### Base URL grup ini adalah `periods`, bukan `accounting-periods`

Baris di atas sebelumnya menulis `api/v1/corporate/accounting/accounting-periods`, sementara
bagian MVP dokumen yang sama menulis `api/v1/corporate/accounting/periods` untuk **grup yang sama
persis**. Satu dokumen, dua base URL — dan yang dipakai `[Route(...)]` pada
`AccountingPeriodController` adalah `periods`.

`periods` **tidak dapat ditawar**: ia sudah dipakai sebelas layar Accounting MVP yang berjalan
lewat `accounting-period-slice.jsx`. Mengubah route agar cocok dengan dokumen akan merusak
kesebelasnya sekaligus.

### Nama hak akses grup ini adalah `AccountingPeriod`, bukan `Period`

Bagian Phase 2 sebelumnya menulis `Period : Read`, `Close`, dan `Approve`. Implementasinya
`AccountingPeriod : ...`, sama seperti bagian MVP.

Ini pun bukan pilihan: argumen pertama `[AccessPermission]` **wajib** sama dengan `ControllerName`
pada controller yang bersangkutan, dan `ControllerName`-nya `"AccountingPeriod"`. Nama `Period`
menghasilkan `403` permanen yang **tidak dapat diperbaiki dari layar Akses Role** — hak yang
dicari tidak pernah ada untuk diberikan.

### Empat bidang keadaan pada `PeriodClosingChecklistDto`

Daftar periksa penutupan tidak hanya membawa jumlah; ia juga membawa **apakah setiap butir sudah
sempat diperiksa**. Sebagiannya belum, karena gelombang `P2-1` belum berdiri.

| Bidang | Letak | Tipe | Arti |
|---|---|---|---|
| `State` | tiap butir | `PeriodChecklistItemState` — `Evaluated` atau `NotYetAvailable` | `Evaluated` berarti butir ini benar-benar dihitung. `NotYetAvailable` berarti **belum dapat diperiksa sama sekali** |
| `UnavailableReason` | tiap butir | `string?` | Diisi **hanya** bila `State` bernilai `NotYetAvailable`: alasannya, beserta gelombang yang akan menyediakannya |
| `IsComplete` | akar respons | `bool` | Benar hanya bila **seluruh** butir sudah dapat diperiksa. Selama `P2-1` belum berdiri, nilainya `false` |
| `NotYetAvailableCount` | akar respons | `int` | Jumlah butir yang belum dapat diperiksa, penghalang maupun peringatan |

**`Count` bernilai `0` pada butir `NotYetAvailable`.** Nol di sana berarti "belum ada yang
memeriksa", bukan "sudah diperiksa dan bersih". Layar yang merender keduanya sama membuat petugas
menutup periode dengan yakin, padahal sebagian penghalangnya tidak pernah dihitung.

Karena itu `CanSubmitClosing` bernilai benar bila tidak ada penghalang **yang sudah diperiksa**
bernilai lebih dari nol — ia **tidak** menjamin seluruh pemeriksaan sudah berjalan. Yang menjamin
itu `IsComplete`.

Bentuk respons **tidak berubah** saat sebuah butir beralih dari `NotYetAvailable` ke `Evaluated`:
`Blockers` selalu berisi **tiga** penghalang (`ACC-DEC-051` + `ACC-DEC-065`) dan `Warnings` selalu
berisi **enam** peringatan (`ACC-DEC-051` + `ACC-DEC-070`), apa pun keadaannya. Layar tidak perlu
diubah saat gelombang berikutnya menyalakan sebuah butir, dan **tidak boleh** menanam kedua angka
itu di kodenya.

## Corporate / Accounting / Year End Closing

`[Tags("Corporate - Accounting - Year End Closing")]`
Base URL: `api/v1/corporate/accounting/year-end-closing` — **Rencana (belum tersedia)**

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/preview` | Pratinjau perhitungan tutup tahun: saldo tiap akun pendapatan dan beban, serta selisih yang akan masuk laba ditahan. **Tidak membuat apa pun** | `YearEndClosing : Read` | `legalEntityId`, `fiscalYear` pada query | `ApiResponse<YearEndClosingPreviewDto>` |
| `POST` | `/generate` | Menyusun jurnal penutup tahun berstatus `Draft` berjenis `JT` | `YearEndClosing : Generate` | `GenerateYearEndClosingRequest` | `ApiResponse<JournalDetailDto>` |

`409` muncul bila masih ada periode tahun itu yang belum tertutup, atau bila jurnal penutup tahun
itu sudah pernah disusun. `422` muncul bila akun laba ditahan belum ditetapkan pada pengaturan
akuntansi.

**Pengesahannya memakai endpoint yang sudah ada**, yaitu `POST /journals/{id}/post`. Tidak ada
jalur pengesahan khusus tutup tahun, karena jurnal penutup adalah jurnal biasa.

## Corporate / Accounting / Master Data / Accounting Configuration

`[Tags("Corporate - Accounting - Master Data - Configuration")]`
Base URL: `api/v1/corporate/accounting/configuration` — **Rencana (belum tersedia)**

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/{legalEntityId}` | Menampilkan pengaturan akuntansi satu badan hukum | `AccountingConfiguration : Read` | — | `ApiResponse<AccountingConfigurationDto>` |
| `PUT` | `/{legalEntityId}` | Menetapkan akun laba ditahan | `AccountingConfiguration : Update` | `UpdateAccountingConfigurationRequest` | `ApiResponse<AccountingConfigurationDto>` |

`422` muncul bila akun yang ditunjuk bukan akun berjenis Ekuitas, atau bukan akun yang menerima
transaksi.

## Corporate / Accounting / Reconciliation

`[Tags("Corporate / Accounting / Reconciliation")]`
Base URL: `api/v1/corporate/accounting/reconciliation` — `gl-balances` **sudah berdiri** lewat
`BE-ACC-P2-013`, terbukti `ReconciliationController` pada `rizkiG`

> **Amandemen menunggu ratifikasi — `ACC-API-0.11`, 11 September 2026.** Grup ini belum pernah
> tercatat di kontrak mana pun, padahal endpoint pertamanya sudah berdiri sejak 9 September 2026.
> Kartu `BE-ACC-P2-013` sudah mengantisipasinya sebagai *"delta kontrak, endpoint baru"*, dan
> laporannya mencatatnya menunggu ratifikasi. **Untuk baris `gl-balances` yang benar adalah kode,
> bukan dokumen ini.**

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/gl-balances` | Saldo buku besar seluruh control account satu badan hukum, dihitung **hanya dari baris jurnal `Posted`**, sampai tanggal batas bila diminta. **Tidak menyimpan apa pun** | `AccountingReconciliation : Read` | `ControlAccountBalanceQuery` — `legalEntityId` wajib, `asOfDate` opsional, keduanya pada query | `ApiResponse<ControlAccountBalanceReportResponse>` |

`400` muncul bila `legalEntityId` tidak disebutkan — saldo dua badan hukum tidak boleh tercampur.
`409` muncul dari penjaga badan hukum utama (`ACC-DEC-043`), sama seperti seluruh endpoint
Accounting. Badan hukum tanpa satu pun control account dijawab **`200` berdaftar kosong** beserta
pesan bahwa belum ada akun yang ditandai — bukan `404`, karena itu keadaan wajar sebelum daftar
akun dilengkapi.

Hak aksesnya bernama **`AccountingReconciliation`**, bukan `Reconciliation`: argumen pertama
`[AccessPermission]` wajib sama dengan `ControllerName`, dan `ControllerName`-nya
`"AccountingReconciliation"` — alasan yang sama dengan `AccountingPeriod` di atas.

### Aturan perhitungan yang mengikat

| Aturan | Isi |
|---|---|
| Hanya `Posted` | Baris dari jurnal `Draft`, `PendingApproval`, dan `Approved` **tidak** dihitung. Menghitungnya menghasilkan saldo yang tidak pernah cocok dengan subledger, dan selisihnya akan disalahartikan sebagai cacat data |
| Batas tanggal inklusif | `asOfDate = 2026-09-30` memuat jurnal bertanggal akuntansi 30 September. Kosong berarti seluruh riwayat. Hanya bagian tanggalnya yang dipakai |
| Akun yang ikut | Seluruh akun ber-`IsControlAccount = true` milik badan hukum itu yang belum dihapus, **termasuk yang nonaktif**. Akun hanya dapat dinonaktifkan saat saldonya nol (`ACC-DEC-024`) |
| Rumus | Sama persis dengan `AccChartOfAccountService.HitungSaldoAsync`; kesamaannya diuji `AngkanyaSamaDenganHitungSaldoAsync` |
| Tanpa cache | Dihitung saat diminta; `EvaluatedAt` mencatat kapan |

### Bidang respons

| Bidang | Letak | Tipe | Arti |
|---|---|---|---|
| `LegalEntityId` | akar | `Guid` | Badan hukum yang dibaca |
| `AsOfDate` | akar | `DateTime?` | Tanggal batas yang dipakai; kosong berarti seluruh riwayat |
| `EvaluatedAt` | akar | `DateTime` (UTC) | Waktu perhitungan. Angkanya tidak pernah disimpan |
| `AccountCount` | akar | `int` | Jumlah control account |
| `TotalDebit`, `TotalCredit` | akar | `decimal` | Jumlah seluruh akun |
| `Accounts` | akar | daftar | Satu butir per control account, berurut kode akun |
| `AccountId`, `AccountCode`, `AccountName`, `AccountType`, `NormalBalance` | tiap akun | — | Identitas akun |
| `TotalDebit`, `TotalCredit` | tiap akun | `decimal` | Jumlah baris `Posted` |
| `Balance` | tiap akun | `decimal` | Debit dikurangi kredit — bentuk mentah |
| **`BalanceInNormalBalance`** | tiap akun | `decimal` | Saldo menurut saldo normal akunnya. Akun bersaldo normal kredit — Utang, misalnya — tampil **positif** saat sehat. Bidang inilah yang ditampilkan di layar |
| **`PostedLineCount`** | tiap akun | `int` | Jumlah baris `Posted` yang membentuk saldo |

`BalanceInNormalBalance` dan `PostedLineCount` adalah dua bidang di luar kartu `BE-ACC-P2-013`,
dicatat laporannya sebagai delta kontrak.

### Sisi subledger belum dirancang endpointnya

`ACC-DEC-066` meminta tiga hal: saldo buku besar, saldo subledger, dan laporan selisih. Grup ini
baru memuat yang pertama. Dua sisanya adalah `BE-ACC-P2-014`, yang menurut `ACC-DEC-071` membaca
saldo subledger **dari kejadian yang diterbitkan Finance per periode** — dan kotak masuknya menunggu
gelombang `P2-1`. Path, request, dan bentuk responsnya **sengaja belum ditulis di sini**; menulisnya
sekarang berarti mengunci bentuk yang belum pernah dirancang. Ditambahkan saat `BE-ACC-P2-014`
direncanakan.

Sampai saat itu layar rekonsiliasi (`FE-ACC-P2-008`) menampilkan kolom subledger dan selisih
sebagai **"belum tersedia"**, bukan menyembunyikannya.

## Daftar DTO Phase 2

Penamaan mengikuti koreksi `ACC-GAP-004`: masukan bernama `Request`, keluaran bernama `Dto`.

| Kelompok | DTO |
|---|---|
| Accounting Event | `AccountingEventPagedQuery`, `AccountingEventListDto`, `AccountingEventDetailDto`, `AccountingEventAttemptDto`, `AccountingEventSummaryDto`, `AccountingEventReceiptDto`, `ReceiveAccountingEventRequest`, `IgnoreAccountingEventRequest` |
| Event Type | `EventTypePagedQuery`, `EventTypeListDto`, `EventTypeDetailDto`, `EventTypeOptionDto`, `CreateEventTypeRequest`, `UpdateEventTypeRequest` |
| Posting Rule | `PostingRulePagedQuery`, `PostingRuleListDto`, `PostingRuleDetailDto`, **`PostingRuleLineDto`**, `CreatePostingRuleRequest`, `UpdatePostingRuleRequest` — keduanya memuat daftar baris |
| Recurring Journal | `RecurringJournalPagedQuery`, `RecurringJournalListDto`, `RecurringJournalDetailDto`, `RecurringJournalLineDto`, `RecurringJournalRunDto`, `CreateRecurringJournalRequest`, `UpdateRecurringJournalRequest`, `GenerateRecurringJournalRequest` |
| Period Closing | `PeriodClosingChecklistDto` (+ `IsComplete`, `NotYetAvailableCount`), `PeriodClosingBlockerDto` (+ `State`, `UnavailableReason`), `PeriodClosingApprovalDto`, `SubmitPeriodClosingRequest`, `ApprovePeriodClosingRequest`, `RejectPeriodClosingRequest` |
| Year End Closing | `YearEndClosingPreviewDto`, `YearEndClosingPreviewLineDto`, `GenerateYearEndClosingRequest` |
| Configuration | `AccountingConfigurationDto`, `UpdateAccountingConfigurationRequest` |
| Reconciliation *(usulan `ACC-API-0.11`)* | `ControlAccountBalanceQuery`, `ControlAccountBalanceReportResponse`, `ControlAccountBalanceResponse` — **disalin dari source**, berakhiran `Query`/`Response` seperti kodenya |

### Isi `ReceiveAccountingEventRequest` — dua belas bidang wajib

Mewujudkan `ACC-DEC-048` (sepuluh bidang pertama) dan `ACC-DEC-060` (dua bidang penelusuran).
Pesan yang kehilangan satu pun bidang ini ditolak `400`.

| Bidang | Tipe | Wajib | Contoh |
|---|---|:---:|---|
| `EventNumber` | `string` | Ya | `EVT-100` |
| `EventTypeCode` | `string` | Ya | `PENGAKUAN-PIUTANG` |
| `SourceModule` | `string` | Ya | `Finance` |
| `SourceTransactionId` | `string` | Ya | `AR-2026-09-00871` |
| `SourceVersion` | `string` | Ya | `1` |
| `EventOccurredAt` | `timestamptz` | Ya | `2026-09-08T10:15:00+07:00` |
| `AccountingDate` | `date` | Ya | `2026-09-08` |
| `Amount` | `decimal(18,2)` | Ya | `10000000.00` |
| `CurrencyCode` | `string` | Ya | `IDR` — nilai lain ditolak `409` |
| `LegalEntityId` | `Guid` | Ya | Badan hukum yang bukunya disentuh |
| `CorrelationId` | `Guid` | **Ya** | Penelusuran ke kejadian asal di Billing (`ACC-DEC-060`) |
| `CausationId` | `Guid` | **Ya** | Penelusuran ke tindakan yang menyebabkannya (`ACC-DEC-060`) |

**Tidak ada bidang pengenal pasien**, dan itu disengaja (`ACC-DEC-056`).

### Kenapa `CorrelationId` dan `CausationId` wajib, bukan opsional

Owner Billing menyatakan 9 September 2026 bahwa kejadian yang diterbitkan Finance **tetap membawa
correlation dan causation ke source Billing**. Tanpa keduanya disimpan Accounting,
`SourceTransactionId` hanya berisi nomor pencatatan AR milik **Finance** — bukan nomor faktur
Billing.

Akibatnya pertanyaan *"jurnal ini berasal dari tagihan pasien yang mana"* menuntut membuka modul
Finance lebih dahulu untuk mencari nomor fakturnya. Padahal penghubungnya sudah dibawakan
Finance, dan hanya perlu disimpan.

Dijadikan **wajib**, bukan opsional, karena bidang penelusuran yang boleh kosong cenderung
berakhir kosong — dan baru ketahuan saat benar-benar dibutuhkan, yaitu ketika ada selisih angka
yang harus ditelusuri.

### Bidang kesebelas yang bersifat opsional: `Components`

Ditambahkan `ACC-DEC-058`. Bukan bagian dari kedua belas bidang wajib, sehingga pesan tanpa `Components`
tetap sah — ia berarti seluruh nilai memakai komponen `TOTAL`.

| Bidang | Tipe | Wajib | Keterangan |
|---|---|:---:|---|
| `Components` | daftar | Tidak | Rincian nilai. Setiap butir berisi `ComponentCode` dan `Amount` |

**Contoh pendapatan rawat jalan dengan jasa medis dokter:**

```json
{
  "EventNumber": "EVT-100",
  "EventTypeCode": "PENGAKUAN-PIUTANG",
  "SourceModule": "Finance",
  "SourceTransactionId": "AR-2026-09-00871",
  "SourceVersion": "1",
  "EventOccurredAt": "2026-09-08T10:15:00+07:00",
  "AccountingDate": "2026-09-08",
  "Amount": 10000000.00,
  "CurrencyCode": "IDR",
  "LegalEntityId": "...",
  "Components": [
    { "ComponentCode": "JASA_MEDIS", "Amount": 3000000.00 }
  ]
}
```

Aturan posting untuk jenis itu memuat empat baris — dua memakai `TOTAL`, dua memakai `JASA_MEDIS`
— sehingga jurnalnya berisi empat baris: debit Rp 13.000.000 lawan kredit Rp 13.000.000.

`422` muncul bila kejadian membawa komponen yang **tidak dipakai** satu pun baris aturan. Kejadian
berstatus **Tertahan**, karena mengabaikan komponen yang tidak dikenal berarti membuang angka
diam-diam.
