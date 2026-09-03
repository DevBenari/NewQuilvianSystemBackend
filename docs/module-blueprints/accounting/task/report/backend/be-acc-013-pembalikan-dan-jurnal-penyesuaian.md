# `BE-ACC-013` — Pembalikan penuh dan jurnal penyesuaian

| Field | Isi |
|---|---|
| Task ID | `BE-ACC-013` |
| Blueprint | `ACC-BP-001` revisi `9`, `decision_revision` `1.6` |
| Status | **`IMPLEMENTED — menunggu verifikasi manual owner`** |
| Tanggal | 3 September 2026 |
| Branch | `rizkiG` |
| Kontrak | `ACC-API-0.2` endpoint `reverse`; `ACC-VALIDATION-0.2` bagian 5; `ACC-STATE-0.1` bagian 1.1 baris terakhir |
| Migration | **Nol** |

## 1. Backend Governance Preflight

| Field | Isi |
|---|---|
| Area / Module / Submodule | `Corporate` / `AccountingManagement` / `JournalManagement` |
| Keberlakuan | `NEW CODE` |
| QBE yang berlaku | `QBE-API-001`, `QBE-CODE-003` (alokasi nomor jurnal koreksi) |
| QBE yang **tidak** berlaku | `QBE-MOD-002`/`003` — nol entity persisted baru |

## 2. Berkas

| Berkas | Keadaan |
|---|---|
| `JournalManagement/Services/AccJournalService.cs` | diubah — `ReverseAsync`, helper `NamaPeriode` |
| `JournalManagement/Controllers/JournalController.cs` | diubah — endpoint `POST /{id}/reverse` |
| `JournalManagement/DTOs/JournalDtos.cs` | diubah — `ReverseJournalRequest` |

Endpoint: `POST /api/v1/corporate/accounting/journals/{id}/reverse`, permission
`Journal : Reverse`. **Nol delta kontrak.**

Dengan ini seluruh **sepuluh** endpoint grup Journal pada `ACC-API-0.2` sudah berdiri.

## 3. Keputusan implementasi

### Jurnal asal tidak disentuh — dan itu terlihat dari kodenya

Inti `ACC-DEC-006`. `ReverseAsync` memuat **nol** baris yang menulis ke jurnal asal. Jurnal asal
bahkan dimuat dengan `AsNoTracking`, sehingga EF tidak punya jalan untuk menyimpan perubahan
apa pun atasnya walau ada kekeliruan di kemudian hari. Satu-satunya penautan adalah
`ReversalOfJournalId` pada jurnal **baru** yang menunjuk kepadanya.

### Jenis jurnal diambil dari master menurut kode, bukan Guid di kode

`JB` untuk pembalikan penuh (`ACC-DEC-029`), `JP` untuk penyesuaian (`ACC-DEC-017`). Pencariannya
memakai ulang **`AccJournalTypeService.CariMenurutKodeAsync`** — inilah tempat method itu benar-
benar dibutuhkan, dan sebabnya ia tidak dipakai pada `BE-ACC-010` (di sana kontrak mengirim
`JournalTypeId` bertipe Guid).

Bila `JB` atau `JP` tidak ada di master, jawabannya `422` dengan pesan yang menyuruh mengisi
master lebih dahulu — bukan `500`, dan bukan diam-diam membuat jenis jurnal sendiri.

### Nomor jurnal koreksi memakai alokator yang sama

`AlokasikanNomorJurnalAsync` dipakai apa adanya, sehingga jurnal pembalik memperoleh nomor
berawalan `JB` atau `JP` lewat `pg_advisory_xact_lock` yang sama. Itulah sebabnya method itu
dibuat `public static` pada `BE-ACC-010`. Seluruh pembuatan berada dalam **satu transaction**.

### Baris pembalikan mempertahankan nomor baris, akun, dan unit biaya

Hanya sisinya yang ditukar: debit menjadi kredit dan sebaliknya. Nomor baris dipertahankan supaya
pembalikan dapat dibandingkan **baris demi baris** dengan jurnal asalnya saat audit.

### Penyesuaian wajib seimbang sejak dibuat

Berbeda dari jurnal manual biasa yang boleh disimpan timpang (`ACC-DEC-025`). Koreksi yang timpang
berarti penyusunnya belum tahu apa yang hendak dikoreksi, dan membiarkannya tersimpan hanya
memindahkan kebingungan ke penyetuju.

### Riwayat ditulis pada **dua** jurnal

| Jurnal | Tindakan | Alasan |
|---|---|---|
| Jurnal **asal** | `Reversed` | Di situlah pertanyaan audit *"kapan jurnal ini dibalik dan oleh siapa"* akan dicari |
| Jurnal **koreksi** | `Submitted` | Ia memang lahir dalam keadaan diajukan |

Menulis hanya pada jurnal koreksi akan membuat jurnal asal tampak tidak pernah tersentuh apa pun,
padahal ada peristiwa penting yang mengenainya.

### Lahir menunggu persetujuan

Jurnal koreksi berstatus `PendingApproval` dengan `SubmittedBy`/`SubmittedAt` terisi — acceptance
(5). Koreksi adalah tindakan yang **paling** perlu diperiksa orang kedua, bukan yang paling boleh
melewatinya. Selanjutnya ia mengikuti jalur `approve` → `post` biasa dari `BE-ACC-011`, termasuk
aturan `ACC-DEC-016`.

## 4. Acceptance — keadaan implementasi

| # | Acceptance | Keadaan |
|---|---|---|
| (1) | Pembalikan penuh menghasilkan jurnal `JB` berisi kebalikan seluruh baris | **Terimplementasi** |
| (2) | Penyesuaian menghasilkan jurnal `JP` berisi baris selisih pengguna | **Terimplementasi** |
| (3) | **Jurnal asal tetap `Posted` dan isinya tidak berubah sama sekali** | **Terimplementasi** — dimuat `AsNoTracking`, nol baris menulis kepadanya |
| (4) | Membalik dua kali ditolak `409` beserta nomor jurnal pembaliknya | **Terimplementasi** |
| (5) | Jurnal pembalik lahir menunggu persetujuan | **Terimplementasi** |
| (6) | Alasan wajib diisi | **Terimplementasi** |

Seluruh aturan `ACC-VALIDATION-0.2` bagian 5 tertutup, termasuk cara koreksi wajib dipilih,
penyesuaian wajib punya baris, penyesuaian wajib seimbang, dan periode tujuan menerima.

## 5. Skrip uji manual

Prasyarat: satu jurnal **sudah disahkan** (`Posted`), misalnya debit Beban Rp 1.000.000 / kredit
Kas Rp 1.000.000. Pengguna butuh `Journal : Reverse`.

| # | Langkah | Hasil yang diharapkan |
|---|---|---|
| **(6)** | `POST /{id}/reverse` dengan `reason` kosong | `400` — "Alasan pembalikan wajib diisi." |
| **cara** | `POST /{id}/reverse` tanpa `correctionType` | `400` — "Pilih cara koreksi: pembalikan penuh atau jurnal penyesuaian." |
| **(1)** | `POST /{id}/reverse` `{ "correctionType": 1, "reason": "salah akun" }` | `200`. Jurnal baru bernomor **`JB/...`**. Barisnya: yang tadinya debit 1.000.000 kini **kredit** 1.000.000, dan sebaliknya. Nomor baris sama |
| **(5)** | Periksa jurnal baru itu | `journalStatus` = **`PendingApproval`**, `submittedBy` terisi, `postedAt` kosong |
| **(3)** | `GET /{id asal}` | Masih **`Posted`**. `totalDebit`, `totalCredit`, seluruh baris, dan `updateDateTime` **tidak berubah** |
| **(3b)** | `GET /general-ledger/trial-balance` untuk periode itu | **Belum berubah** — jurnal pembalik belum disahkan, jadi belum terhitung |
| **(3c)** | Setujui dan sahkan jurnal pembalik, panggil ulang neraca saldo | Saldo akun yang terlibat kembali nol, dan `isBalanced` tetap `true` |
| **(4)** | `POST /{id asal}/reverse` sekali lagi | `409` — "Jurnal ini sudah pernah dibalik dengan jurnal **JB/2026/09/00001**." Nomornya harus benar-benar disebutkan |
| **(2)** | Atas jurnal `Posted` lain, `{ "correctionType": 2, "reason": "nominal kurang", "adjustmentLines": [debit Beban 500.000, kredit Kas 500.000] }` | `200`. Jurnal baru bernomor **`JP/...`** berisi tepat dua baris itu |
| **(2b)** | Penyesuaian dengan `adjustmentLines` kosong | `400` — "Jurnal penyesuaian harus memiliki baris selisih." |
| **(2c)** | Penyesuaian dengan baris **tidak seimbang** (debit 500.000, kredit 300.000) | `400` — "Baris penyesuaian belum seimbang. Selisih Rp 200.000." |
| **status** | Membalik jurnal yang masih `Draft` | `409` — "Hanya jurnal yang sudah disahkan yang dapat dibalik." |
| **periode** | Membalik ke periode yang sudah `Closed` (kirim `accountingDate` di periode itu) | `422` menyebut nama periodenya |
| **AvailableActions** | `GET /{id asal}` sesudah dibalik, sebagai pemegang `Journal : Reverse` | `availableActions` **tidak lagi memuat** `reverse` |
| **riwayat** | `GET /{id asal}` | `approvals` memuat baris `Reversed` beserta alasannya |

## 6. Blocker dan catatan

| Hal | Keterangan |
|---|---|
| `ACC-TD-011` | `JB` dan `JP` datang dari seeder. Selama `POST /seed` belum dipanggil, seluruh pembalikan akan dijawab `422` |
| `ACC-TD-017` | Tanpa test otomatis — acceptance belum punya bukti eksekusi |

## 7. Task berikutnya

`BE-ACC-014` — saldo awal. Roadmap menyatakan **tidak ada endpoint baru**; ia memakai seluruh
jalur jurnal yang sudah ada.
