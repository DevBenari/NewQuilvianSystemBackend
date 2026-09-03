# `BE-ACC-012` — Buku besar, saldo per akun, dan neraca saldo

| Field | Isi |
|---|---|
| Task ID | `BE-ACC-012` |
| Blueprint | `ACC-BP-001` revisi `9`, `decision_revision` `1.6` |
| Status | **`IMPLEMENTED — menunggu verifikasi manual owner`** |
| Tanggal | 3 September 2026 |
| Branch | `rizkiG` |
| Kontrak | `ACC-API-0.2` grup General Ledger (3 endpoint); `ACC-PERMISSION-0.3` |
| Migration | **Nol.** Tidak ada tabel buku besar — seluruhnya dihitung dari `AccJournalLine` |

DoD roadmap menuntut *"acceptance terbukti test"* dan *"hasil verifikasi performa tercatat"*.
Keduanya belum terpenuhi; alasannya di bagian 6 dan 7.

## 1. Backend Governance Preflight

| Field | Isi |
|---|---|
| Area / Module / Submodule | `Corporate` / `AccountingManagement` / `GeneralLedger` |
| Pemilik / prefix | Rizki — `Acc`, lifecycle `ACTIVE` |
| Keberlakuan | `NEW CODE` |
| QBE yang berlaku | `QBE-API-001` |
| QBE yang **tidak** berlaku | `QBE-MOD-002`/`003` — nol entity persisted; folder `GeneralLedger/` hanya memuat DTO, service, dan controller |

`QBE-MOD-003` mewajibkan pendaftaran folder yang akan memuat **model persisted**. Folder
`GeneralLedger/` sengaja tidak memuat satu pun, sehingga kewajiban itu tidak menyala.

## 2. Berkas

| Berkas | Keadaan | Baris |
|---|---|---|
| `GeneralLedger/DTOs/GeneralLedgerDtos.cs` | baru | 108 |
| `GeneralLedger/Services/AccGeneralLedgerService.cs` | baru | 403 |
| `GeneralLedger/Controllers/GeneralLedgerController.cs` | baru | 112 |
| `Program.cs` | diubah | +2 |

### Endpoint

| Method | Path | Permission | Dicatat logger |
|---|---|---|---|
| `GET` | `/movements` | `GeneralLedger : Read` | Tidak |
| `GET` | `/trial-balance` | `GeneralLedger : Read` | **Ya** |
| `GET` | `/account-balance/{accountId}` | `GeneralLedger : Read` | Tidak |

**Nol delta kontrak.** Tiga tambahan field pada response yang tidak mengubah kontrak: `LineNumber`
pada mutasi, `AccountId` pada baris neraca saldo, dan `PeriodName` — ketiganya penambahan, bukan
perubahan, dan ada supaya frontend tidak perlu menebak baris mana yang dimaksud atau memformat
nama periode sendiri.

## 3. Keputusan implementasi

### Satu pintu masuk data, bukan penyaring yang diulang

Acceptance (2) — hanya jurnal `Posted` yang terhitung — adalah butir yang paling mudah terlewat,
dan akibatnya laporan salah **tanpa terlihat salah**: angkanya tetap masuk akal. Karena itu
penyaringnya tidak ditulis ulang di tiap query, melainkan dipusatkan pada satu method
`BarisDisahkan(legalEntityId)` yang menjadi satu-satunya pintu masuk data bagi ketiga endpoint.

Method itu sekaligus menegakkan acceptance (5): badan hukum disaring di tempat yang sama, sekali.

### Urutan adalah bagian dari kebenaran

Saldo berjalan hanya bermakna bila urutannya tidak pernah berubah. Urutannya dikunci pada
**`AccountingDate`, lalu `JournalNumber`, lalu `LineNumber`** — acceptance (3) dan (4). Ketiganya
sudah ada pada rancangan MVP dan bersama-sama unik, karena `(LegalEntityId, JournalNumber)` unik
dan `(JournalId, LineNumber)` unik. **Nol field baru dikarang untuk pengurutan**, sesuai larangan
roadmap dan larangan `SortOrder` presentasi yang dipersistensi.

### Saldo berjalan benar lintas halaman

Halaman kedua tidak dimulai dari nol. Saldo dibuka dari dua bagian, keduanya dihitung **di
database**:

1. Mutasi sebelum `DateFrom`.
2. Seluruh baris yang dilewati halaman-halaman sebelumnya — `terurut.Take(dilewati).Sum(...)`.

Tanpa bagian kedua, `RunningBalance` pada halaman 2 akan salah dan kesalahannya sulit terlihat
karena bentuk angkanya tetap wajar.

### Konvensi tanda saldo

`RunningBalance`, `OpeningBalance`, dan `ClosingBalance` memakai **debit dikurangi kredit**;
positif berarti condong debit. Ini konvensi yang sama persis dengan
`AccChartOfAccountService.HitungSaldoAsync` yang sudah ada sejak `BE-ACC-007`.

Alternatifnya — membalik tanda menurut `NormalBalance` akun — sengaja **tidak** dipilih: ia
menciptakan pengertian "saldo" kedua yang berbeda di dalam satu modul, dan dua pengertian saldo
adalah cara paling cepat membuat laporan saling bertentangan. Bila owner menghendaki tampilan
per sisi normal, itu keputusan presentasi yang tempatnya di frontend atau di keputusan
arsitektur tersendiri.

### `IsBalanced` dihitung apa adanya

Neraca saldo seimbang bukan karena dipaksakan, melainkan **akibat** dari setiap jurnal yang
disahkan wajib seimbang. `IsBalanced` karena itu dihitung apa adanya dan tidak pernah dibulatkan
agar terlihat seimbang. Bila ia `false`, yang rusak adalah datanya, dan laporan wajib
mengatakannya.

### `account-balance` menurunkan badan hukum dari akunnya

Endpoint itu tidak menerima `LegalEntityId` dari pemanggil — ia membacanya dari akun yang diminta.
Dengan begitu saldo dua badan hukum tidak mungkin tercampur lewat parameter yang keliru.

## 4. Acceptance — keadaan implementasi

| # | Acceptance | Tempat penegakan | Keadaan |
|---|---|---|---|
| (1) | Neraca saldo total debit sama persis dengan total kredit | `GetTrialBalanceAsync`, `IsBalanced` | **Terimplementasi** |
| (2) | Jurnal selain `Posted` tidak ikut terhitung | `BarisDisahkan` — satu pintu masuk | **Terimplementasi** |
| (3) | Saldo berjalan deterministic | Urutan tetap + saldo lintas halaman | **Terimplementasi** |
| (4) | Urutan sekunder stabil pada `AccountingDate` kembar | `OrderBy(AccountingDate).ThenBy(JournalNumber).ThenBy(LineNumber)` | **Terimplementasi** |
| (5) | Saldo dua badan hukum tidak tercampur | `BarisDisahkan(legalEntityId)`; `account-balance` menurunkannya dari akun | **Terimplementasi** |
| (6) | `/trial-balance` dicatat logger, dua lainnya tidak | `GeneralLedgerController` | **Terimplementasi** |

## 5. Skrip uji manual

Prasyarat: minimal **dua jurnal disahkan** pada periode yang sama, plus satu jurnal yang
**tidak** disahkan (biarkan `Draft`), dan satu jurnal disahkan pada periode **sebelumnya**.

| # | Langkah | Hasil yang diharapkan |
|---|---|---|
| **(1)** | `GET /trial-balance?legalEntityId=...&periodCode=2026-09` | `totalDebit` sama persis dengan `totalCredit`, dan `isBalanced` = `true` |
| **(2a)** | Catat `totalDebit`. Lalu buat jurnal baru, ajukan, setujui, **jangan disahkan**. Panggil ulang | `totalDebit` **tidak berubah** |
| **(2b)** | Sahkan jurnal itu, panggil ulang | `totalDebit` **bertambah** sebesar nilainya |
| **(2c)** | Tolak sebuah jurnal `PendingApproval`, panggil ulang | Tidak berpengaruh sama sekali |
| **(3)** | `GET /movements?...&pageSize=100` dua kali berturut-turut | Urutan baris **identik**, dan `runningBalance` tiap baris **identik** |
| **(4)** | Buat dua jurnal ber-`accountingDate` **sama persis** menyentuh akun yang sama, sahkan keduanya. Panggil `/movements` berkali-kali | Urutannya selalu sama — yang bernomor jurnal lebih kecil selalu lebih dahulu |
| **(3b)** | `GET /movements?...&pageSize=2&pageNumber=1`, catat `runningBalance` baris terakhir. Lalu `pageNumber=2` | `runningBalance` baris pertama halaman 2 **melanjutkan** dari baris terakhir halaman 1, bukan mengulang dari nol |
| **(5)** | Buat akun dan jurnal di bawah `LE-MDC-001`, sahkan. Panggil `/trial-balance` untuk `LE-MMC-001` | Akun `LE-MDC-001` **tidak muncul**, total tidak berubah |
| **(5b)** | `GET /account-balance/{akun milik LE-MDC-001}?periodCode=...` | Memakai periode milik `LE-MDC-001`; `404` bila periode itu tidak ada di sana |
| **(6)** | Panggil ketiga endpoint, lalu buka `Logs/quilvian-backend-<tanggal>.json` | Hanya `GeneralLedger.TrialBalance` yang muncul. `movements` dan `account-balance` **tidak** tercatat |
| **(6b)** | Periksa isi baris log itu | **Tidak memuat satu pun angka uang** — hanya `LegalEntityId`, `PeriodCode`, `StatusCode`, dan jumlah baris |
| **Saldo pembuka** | `GET /account-balance/{akun}?periodCode=2026-10` sesudah ada jurnal September | `openingBalance` = saldo akhir September, bukan nol |
| **Rentang salah** | `GET /movements?dateFrom=2026-09-30&dateTo=2026-09-01` | `400` — "Tanggal akhir tidak boleh mendahului tanggal mulai." |

## 6. Verifikasi performa — **belum dapat dijalankan**

DoD menuntut *"hasil verifikasi performa tercatat"*. Itu **tidak dapat dipenuhi sekarang**, dan
alasannya bukan kelalaian: roadmap sendiri mensyaratkan pengukuran dilakukan *"pada data yang
menyerupai produksi"*, sedangkan `AccJournal` dan `AccJournalLine` di `QuilvianNewDevRizki`
saat ini **0 baris**.

Mengukur rencana eksekusi pada tabel kosong akan menghasilkan angka yang menyesatkan — PostgreSQL
akan memilih sequential scan untuk tabel kecil apa pun index-nya, sehingga hasilnya tidak
mengatakan apa-apa tentang perilaku sebenarnya.

Kedua kandidat index roadmap **sengaja belum ditambahkan**, persis seperti yang diperintahkan:

| Kandidat | Melayani | Keadaan |
|---|---|---|
| `AccJournalLine (AccountId, JournalId)` | Buku besar per akun | **Kandidat.** Belum dibuat — menunggu rencana eksekusi pada data nyata |
| `AccJournal (LegalEntityId, JournalStatus, AccountingDate)` | Penyaringan `Posted` per periode | **Kandidat.** Belum dibuat |

Menambah index spekulatif memperlambat tulis tanpa bukti bahwa baca menjadi lebih cepat. Dicatat
sebagai **`ACC-TD-018`**.

## 7. Blocker dan catatan

| Hal | Keterangan |
|---|---|
| `ACC-TD-017` | Tanpa test otomatis — acceptance belum punya bukti eksekusi |
| `ACC-TD-018` | **Baru.** Verifikasi performa dan keputusan index tertunda sampai ada data menyerupai produksi |
| `ACC-TD-011` | Master masih kosong; skrip bagian 5 tidak dapat dimulai sebelum data ada |

## 8. Task berikutnya

`BE-ACC-013` — pembalikan penuh dan jurnal penyesuaian.
