# `BE-ACC-014` — Saldo awal

| Field | Isi |
|---|---|
| Task ID | `BE-ACC-014` |
| Blueprint | `ACC-BP-001` revisi `9`, `decision_revision` `1.6` |
| Status | **`IMPLEMENTED — menunggu verifikasi manual owner`** |
| Tanggal | 3 September 2026 |
| Branch | `rizkiG` |
| Kontrak | `ACC-API-0.2` grup Journal; jenis jurnal `SA` |
| Migration | **Nol** |
| **Perubahan kode** | **NOL BARIS.** Lihat bagian 2 |

## 1. Backend Governance Preflight

| Field | Isi |
|---|---|
| Area / Module | `Corporate` / `AccountingManagement` |
| Keberlakuan | Tidak berlaku — nol source berubah |
| QBE yang berlaku | Tidak ada yang menyala; task ini verifikasi, bukan penulisan kode |

## 2. Kenapa nol baris kode

Roadmap `BE-ACC-014` menyatakan sendiri: *Reuse: seluruh jalur jurnal yang sudah ada.* **Tidak ada
endpoint baru.** Cakupannya tiga hal, dan ketiganya **sudah berdiri** sebelum task ini dimulai.

Menambah kode untuk membenarkan keberadaan task ini justru akan melanggar catatan risiko roadmap
sendiri, yang melarang membangun alur persetujuan kedua di dalam sistem.

Yang dikerjakan task ini adalah **memverifikasi** ketiganya dan melaporkan apa yang ditemukan.

### 2.1 Jenis `SA` bertanda sistem dan menuntut persetujuan — **terverifikasi**

`AccountingMasterDataSeeder.cs` baris 96:

```csharp
new JournalTypeDefinition("SA", "Saldo Awal", "SA", true, true)
//                                     RequiresApproval ┘     └ IsSystemType
```

Penjaganya ada di `AccJournalTypeService.UpdateAsync` baris 209: jenis bertanda sistem menolak
`409` bila kode atau awalan nomornya hendak diubah. Dibangun pada `BE-ACC-008`, bukan sekarang.

`AccJournalTypeService.CreateAsync` baris 160 memaksa `IsSystemType = false` untuk jenis baru,
sehingga tanda sistem tidak pernah datang dari permintaan pengguna.

### 2.2 Jurnal `SA` ikut terhitung buku besar dan neraca saldo — **terverifikasi**

Diperiksa langsung: `AccGeneralLedgerService` memuat **nol** rujukan kepada `JournalType`. Satu-
satunya penyaringnya adalah `BarisDisahkan()`, yang menyaring menurut **status** (`Posted`) dan
badan hukum — **tidak pernah menurut jenis jurnal**.

Akibatnya jurnal `SA` ikut terhitung secara otomatis, dan itu memang yang dikehendaki: saldo awal
adalah transaksi pembuka, bukan kategori terpisah yang perlu diistimewakan.

Pencarian `"SA"` di seluruh modul Accounting hanya menemukan **dua** tempat, keduanya sah:

| Tempat | Isi |
|---|---|
| `AccAccountingPeriodService` baris 371 | Periode `Open` menerima `JU`, `JP`, `JB`, `SA` |
| `AccountingMasterDataSeeder` baris 96 | Definisi jenisnya |

Nol perlakuan khusus di jalur jurnal, buku besar, maupun neraca saldo.

### 2.3 Hanya pemegang `Journal : Post` yang dapat mengesahkan — **terverifikasi**

`JournalController.Post` memakai `[AccessPermission("Journal", "Post")]`. Menurut
`ACC-PERMISSION-0.3`, `Journal : Post` hanya dimiliki **Manager**. Tidak ada jalur pengesahan lain.

## 3. Satu batas yang perlu Anda ketahui sebelum menguji

**Jurnal `SA` hanya dapat disahkan ke periode berstatus `Open`.**

`AccAccountingPeriodService.JenisJurnalYangDiterima` menyatakan periode `SoftClosed` hanya
menerima `JP` dan `JB`. Jadi begitu periode pertama ditutup sementara, saldo awal **tidak dapat
lagi** dimasukkan ke sana — jawabannya `422`.

Ini perilaku yang benar menurut `ACC-DEC-012`, tetapi berakibat praktis: **masukkan saldo awal
sebelum menutup periode pertama.** Bila terlanjur tertutup, periode harus dibuka kembali lewat
`POST /periods/{id}/reopen` yang mewajibkan alasan tertulis.

## 4. Acceptance — keadaan implementasi

| # | Acceptance | Ditegakkan di mana | Keadaan |
|---|---|---|---|
| (1) | Jurnal `SA` tersimpan, disetujui, dan disahkan lewat jalur jurnal biasa | `AccJournalService` `BE-ACC-010`/`011` — tanpa cabang khusus | **Terimplementasi** |
| (2) | Neraca saldo periode pertama menampilkan saldo pembuka dan tetap seimbang | `AccGeneralLedgerService.GetTrialBalanceAsync` | **Terimplementasi** |
| (3) | Hanya pemegang `Journal : Post` yang dapat mengesahkannya | `[AccessPermission("Journal", "Post")]` | **Terimplementasi** |

## 5. Skrip uji manual

Prasyarat: master jenis jurnal terisi (`SA` ada), periode `2026-01` berstatus **`Open`**, dan
minimal dua akun `IsPostable`.

| # | Langkah | Hasil yang diharapkan |
|---|---|---|
| **(1a)** | `POST /journals` dengan `journalTypeId` = jenis **`SA`**, `accountingDate` = `2026-01-01`, dua baris seimbang (mis. debit Kas 50.000.000 / kredit Ekuitas 50.000.000) | `201`, nomor berawalan **`SA/2026/01/00001`**, status `Draft` |
| **(1b)** | `POST /{id}/submit` sebagai `STAF` | `200`, `PendingApproval` |
| **(1c)** | `POST /{id}/approve` sebagai `APPROVER` (**bukan** pembuatnya) | `200`, `Approved` |
| **(1d)** | `POST /{id}/post` sebagai `MANAJER` | `200`, `Posted`. Seluruhnya lewat jalur jurnal biasa — tanpa endpoint khusus saldo awal |
| **(3)** | Ulangi (1d) dengan pengguna yang **tidak** punya `Journal : Post` | `403` — "Anda tidak memiliki akses ke menu atau fitur ini." |
| **(2a)** | `GET /general-ledger/trial-balance?legalEntityId=...&periodCode=2026-01` | Akun Kas dan Ekuitas muncul. `totalDebit` = `totalCredit` = 50.000.000, `isBalanced` = **`true`** |
| **(2b)** | `GET /general-ledger/trial-balance?...&periodCode=2026-02` | `openingBalance` Kas = **50.000.000**, bukan nol. Inilah bukti saldo awal terbawa ke periode berikutnya |
| **(2c)** | `GET /general-ledger/account-balance/{akun Kas}?periodCode=2026-01` | `openingBalance` 0, `totalDebit` 50.000.000, `closingBalance` 50.000.000 |
| **(2d)** | `GET /general-ledger/movements?...&accountId={Kas}` | Mutasi `SA/2026/01/00001` muncul, `runningBalance` 50.000.000 |
| **batas** | Tutup sementara periode `2026-01`, lalu coba ajukan jurnal `SA` baru ke sana | `422` — "Periode Januari 2026 sudah ditutup sementara. Hanya jurnal penyesuaian dan pembalikan yang masih dapat disahkan." |
| **sistem** | `PUT /master-data/journal-types/{id SA}` mencoba mengubah `journalTypeCode` menjadi `SL` | `409` — "Jenis jurnal SA dipakai sistem dan kode maupun awalan nomornya tidak dapat diubah." |

**Persetujuan pimpinan keuangan berlangsung di luar sistem** (`ACC-DEC-033`). Tidak ada alur
persetujuan kedua di dalam aplikasi, dan itu disengaja — roadmap melarang membangunnya tanpa
keputusan owner tersendiri.

## 6. Temuan: `RequiresApproval` tidak pernah dibaca

Diperiksa di seluruh modul: `AccJournalType.RequiresApproval` **disimpan, ditampilkan, dan dapat
diubah admin**, tetapi **tidak pernah dibaca untuk menentukan alur**. Setiap jurnal — apa pun
jenisnya — wajib melewati `submit` → `approve` → `post`.

| Hal | Keterangan |
|---|---|
| Apakah ini cacat? | **Bukan, untuk saat ini.** `ACC-DEC-010` menetapkan jurnal manual selalu melewati persetujuan tanpa pengecualian jenis. Alur seragam justru **memenuhi** keputusan itu |
| Lalu apa masalahnya | Admin dapat menyetel `RequiresApproval = false` lewat `PUT /journal-types/{id}`, dan sistem akan **tetap** menuntut persetujuan. Layar akan mengatakan satu hal, backend melakukan hal lain |
| Kenapa tidak saya perbaiki | Dua-duanya perubahan perilaku yang butuh keputusan owner: menghormati kolom itu **melanggar** `ACC-DEC-010`; mengunci kolom itu agar selalu `true` mengubah kontrak `BE-ACC-008` |

Dicatat sebagai **`ACC-TD-019`**.

## 7. Blocker

| Hal | Keterangan |
|---|---|
| `ACC-TD-011` | Jenis `SA` datang dari seeder. Skrip bagian 5 tidak dapat dimulai sebelum `POST /seed` dipanggil |
| `ACC-TD-017` | Tanpa test otomatis — acceptance belum punya bukti eksekusi |
| `ACC-TD-019` | **Baru.** `RequiresApproval` menjanjikan sesuatu yang tidak ditegakkan |

## 8. Task berikutnya

**Seluruh 14 task backend Accounting kini terimplementasi.** Yang tersisa bukan pekerjaan
backend:

1. Verifikasi manual keempat laporan `BE-ACC-011`..`014`, lalu naikkan statusnya ke `DONE`.
2. `ACC-FE-001` dan `ACC-FE-003` — dua keputusan UI yang menahan sebelas task frontend.
3. `ACC-TD-015` / `ACC-DEP-007` — registry, milik lead, menahan merge ke integration.
