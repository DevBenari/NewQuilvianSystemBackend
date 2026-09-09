# Laporan Perubahan Backend — `BE-ACC-P2-013`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-ACC-P2-013` |
| Judul | Saldo control account dari buku besar |
| Slice | Gelombang `P2-RECON` — rekonsiliasi control account |
| Roadmap | [`roadmap/backend-roadmap-phase2.md`](../../../roadmap/backend-roadmap-phase2.md), kartu `BE-ACC-P2-013` |
| Trace | `ACC-DEC-066`; turunan `ACC-DEC-064`, `ACC-DEC-062`; `FR-P2-039` |
| Contract version | `ACC-API-0.8` grup Reconciliation — **endpoint baru, delta kontrak** |
| Dependency | `BE-ACC-P2-011` ✅ — kolom `IsControlAccount` sudah ada di database sejak `004` ✅ |
| Klasifikasi | `MEDIUM` — skor 5: repository 0, berkas diperiksa 1, berkas diubah 1 (5 berkas), logika bisnis 1, kontrak API 1, database 1, keamanan 0, UI 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi, project test, `docs/module-blueprints/accounting/**` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `1812446` |
| Tanggal | 9 September 2026 |
| Status | **🟡 `SEBAGIAN`** — 4 dari 4 acceptance terbukti, tetapi verifikasi PostgreSQL yang diminta roadmap tidak dapat dijalankan. Lihat bagian 5 |

---

## 1. Masalah yang diperbaiki

`BE-ACC-P2-011` sudah memberi tanda pada akun yang saldonya wajib sama dengan catatan rincinya —
Kas Kasir dengan setoran kasir, Piutang dengan rincian piutang pasien. Tetapi **belum ada cara
melihat berapa saldo buku besar akun-akun itu**.

Tanpa angka itu, rekonsiliasi hanya bisa dikerjakan manual: membuka buku besar satu per satu,
menjumlahkan sendiri, lalu membandingkan dengan laporan kasir. Pekerjaan yang memakan waktu, dan
justru karena memakan waktu ia jarang dikerjakan — sampai selisihnya menumpuk berbulan-bulan.

Task ini menyediakan **sisi buku besarnya**. Pembandingnya dengan saldo subledger adalah
`BE-ACC-P2-014`, yang masih terblokir `DEC-ACC-P2-011`.

### Kenapa hanya `Posted` yang boleh dihitung

Ini acceptance (2), dan yang paling berbahaya bila keliru.

Jurnal `Draft`, `PendingApproval`, dan `Approved` **belum menjadi transaksi**. Ikut menghitungnya
menghasilkan saldo buku besar yang **tidak akan pernah cocok** dengan subledger — dan selisihnya
akan disalahartikan sebagai cacat data kasir. Orang akan mencari kesalahan di tempat yang tidak
ada kesalahannya, sementara sebabnya ada di rumus laporan ini.

---

## 2. Proses bisnis

### 2.1 Alur normal

| Langkah | Pelaku | Yang terjadi |
| ---: | --- | --- |
| 1 | Pemilik proses akuntansi | Membuka layar rekonsiliasi, memilih badan hukum dan tanggal |
| 2 | Sistem | Mengambil seluruh akun bertanda control account milik badan hukum itu |
| 3 | Sistem | Menjumlahkan debit dan kredit dari baris jurnal **`Posted`** sampai tanggal itu |
| 4 | Sistem | Menampilkan saldo tiap akun beserta jumlah baris pembentuknya |
| 5 | Pemilik proses akuntansi | *(menyusul `014`)* membandingkan dengan saldo subledger |

### 2.2 Contoh berangka

Kas Kasir menerima empat jurnal pada September 2026:

| Jurnal | Status | Nominal | Dihitung? |
| --- | --- | ---: | :---: |
| `JU-001` | `Posted` | Rp 12.500.000 debit | **Ya** |
| `JU-002` | `Posted` | Rp 2.000.000 debit | **Ya** |
| `JU-003` | `Draft` | Rp 900.000.000 debit | Tidak |
| `JU-004` | `Approved` | Rp 700.000.000 debit | Tidak |

Saldo yang benar: **Rp 14.500.000**, dari 2 baris.

Bila status selain `Posted` ikut terhitung, saldonya menjadi **Rp 1.614.500.000** — selisih
Rp 1,6 miliar dari setoran kasir yang sebenarnya, **tanpa satu pun error muncul**. Inilah yang
dijaga uji `SaldoDihitungHanyaDariBarisPosted`.

### 2.3 Saldo negatif yang sebenarnya wajar

Akun `Hutang Pemasok` bersaldo normal **kredit**. Setelah menerima kredit Rp 4.000.000, rumus
debit dikurangi kredit menghasilkan **−4.000.000** — padahal akunnya justru sehat.

Menyodorkan angka negatif ke layar rekonsiliasi membuat pembacanya mengira ada yang salah. Karena
itu respons membawa **dua** bidang:

| Bidang | Nilai pada contoh | Kegunaan |
| --- | ---: | --- |
| `Balance` | −4.000.000 | Bentuk mentah, sama persis dengan `HitungSaldoAsync` |
| `BalanceInNormalBalance` | 4.000.000 | Untuk ditampilkan; sudah dibalik sesuai saldo normal |

### 2.4 Jalur tidak normal

| Keadaan | Yang terjadi |
| --- | --- |
| Belum ada akun bertanda control account | `200` dengan daftar kosong dan pesan yang menjelaskan |
| Badan hukum tidak disebutkan | `400` — saldo dua badan hukum tidak boleh tercampur |
| Control account milik badan hukum lain | Tidak ikut terhitung |
| Control account tanpa satu pun jurnal `Posted` | Muncul dengan saldo nol, bukan disembunyikan |

Baris terakhir disengaja: akun yang bersaldo nol tetap perlu terlihat, karena "nol" adalah
informasi rekonsiliasi yang sah — berbeda dari "akunnya tidak ada".

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu roadmap `013` dan `014`; `ACC-DEC-066`, `ACC-DEC-064`, `ACC-DEC-061`;
`AccChartOfAccountService.HitungSaldoAsync`; `AccJournalLine.cs` beserta configuration-nya;
`AccJournal.cs`; `GeneralLedgerController.cs` sebagai pola grup; `AccountingLegalEntityGuard.cs`;
`TestDatabase.cs`.

### 3.2 Berkas yang berubah

Lima berkas — empat baru, satu diperbarui.

| Berkas | Perubahan |
| --- | --- |
| `.../AccountingManagement/Reconciliation/Controllers/ReconciliationController.cs` | **Baru.** Satu endpoint |
| `.../AccountingManagement/Reconciliation/Services/AccControlAccountReconciliationService.cs` | **Baru.** Perhitungan saldo |
| `.../AccountingManagement/Reconciliation/DTOs/ControlAccountReconciliationDtos.cs` | **Baru.** Query dan dua response |
| `Tests/.../AccountingManagement/AccControlAccountReconciliationTests.cs` | **Baru.** 9 uji acceptance |
| `Program.cs` | **Satu baris** `AddScoped<AccControlAccountReconciliationService>()` dan satu `using` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Satu endpoint baru** pada grup Reconciliation yang belum ada di `ACC-API-0.8`. Delta kontrak, sudah diantisipasi kartu roadmap |
| Database | **Nol.** Hanya membaca. Nol entity, nol configuration, nol migration |
| Keamanan/Auth | Controller baru `ControllerName = "AccountingReconciliation"`, satu action `Read`. Nol pelonggaran |

### 3.4 Keputusan yang perlu dijelaskan

**Satu query pengelompokan, bukan `HitungSaldoAsync` per akun — dan kesamaannya diuji.**

Kartu roadmap menyebut `AccChartOfAccountService.HitungSaldoAsync` dipakai ulang **apa adanya**.
Dua hal membuat pemakaian harfiahnya tidak tepat di sini:

1. Fungsi itu menghitung **satu akun per panggilan**. Laporan ini justru dibuat untuk membaca
   banyak akun sekaligus, sehingga memanggilnya per akun berarti satu query per control account —
   perilaku N+1 yang dilarang aturan database.
2. Fungsi itu **tidak menerima batas tanggal**, sedangkan acceptance (3) menuntutnya.

Karena itu perhitungannya ditulis sebagai satu query pengelompokan dengan **rumus yang sama**, dan
kesamaan angkanya **dibuktikan uji** `AngkanyaSamaDenganHitungSaldoAsync` — bukan diasumsikan.
Kalau keduanya berselisih, layar rekonsiliasi dan layar akun akan menampilkan saldo berbeda untuk
akun yang sama. **`HitungSaldoAsync` tidak diubah sedikit pun.**

**Batas tanggal memuat tanggalnya sendiri.** "Saldo per 30 September" memuat jurnal 30 September.
Diuji tersendiri, karena batas yang meleset satu hari menghasilkan selisih yang tampak seperti
cacat data.

**Nol hasil disimpan.** `EvaluatedAt` menegaskan angkanya dihitung saat diminta. Rekonsiliasi yang
membaca angka simpanan akan melaporkan cocok padahal sudah tidak.

---

## 4. Dokumentasi endpoint

#### Corporate / Accounting / Reconciliation

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/accounting/reconciliation/gl-balances?legalEntityId=…&asOfDate=…` | Saldo buku besar seluruh control account, **hanya dari baris jurnal `Posted`** | `AccountingReconciliation : Read` |

`400` bila `legalEntityId` tidak disebutkan. `asOfDate` opsional; kosong berarti seluruh riwayat.

### Delta kontrak yang dicatat

| Delta | Alasan |
| --- | --- |
| **Grup Reconciliation belum ada di `ACC-API-0.8`** | Sudah diantisipasi kartu roadmap: *"delta kontrak, endpoint baru"*. Perlu ditambahkan ke kontrak |
| Bidang `BalanceInNormalBalance` | Tanpa ini, akun bersaldo normal kredit tampil negatif saat justru sehat — lihat bagian 2.3 |
| Bidang `PostedLineCount` | Saat saldo tidak cocok, pertanyaan pertama selalu "dari berapa baris angka ini?" |
| `AccountCount`, `TotalDebit`, `TotalCredit` pada tingkat laporan | Ringkasan untuk kepala layar, menghindari layar menjumlahkan sendiri |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build ... -c Release --no-incremental` | `0 Error(s)`, `145 Warning(s)` | `PASS` | Angka sama persis — nol warning baru |
| 9 uji `BE-ACC-P2-013` | `Failed: 0, Passed: 9` | `PASS` | `AccControlAccountReconciliationTests` |
| Seluruh project `UnitTests.Sqlite`, 516 uji | `Failed: 3, Passed: 513` | `EXISTING / ENVIRONMENT ISSUE` | **Nol regresi**; 504 → 513 tepat `+9` |
| QBE checker atas 5 berkas | `VIOLATION: 0`, `PASS` | `PASS` | Keluaran checker |
| **Test integrasi PostgreSQL** | **Tidak dapat dijalankan** | **`NOT RUN`** | `QUILVIAN_BILLING_TEST_DB` tidak diset; fixture `fail-closed` |

### Rincian uji

| Uji | Yang dibuktikan |
| --- | --- |
| `HanyaControlAccount_YangMuncul` | **Acceptance 1** — akun non-control tidak muncul |
| `ControlAccountBadanHukumLain_TidakIkut` | **Acceptance 3** sisi badan hukum |
| `BelumAdaControlAccount_MenjawabBerhasilDenganDaftarKosong` | Daftar kosong bukan kegagalan |
| **`SaldoDihitungHanyaDariBarisPosted`** | **Acceptance 2** — contoh berangka bagian 2.2 |
| **`AngkanyaSamaDenganHitungSaldoAsync`** | Kesamaan dengan fungsi yang kartu minta dipakai ulang |
| `AkunSaldoNormalKredit_DitampilkanPositifMenurutSaldoNormalnya` | `Balance` −4.000.000, `BalanceInNormalBalance` 4.000.000 |
| `DisaringMenurutTanggal_TermasukTanggalBatasnya` | **Acceptance 3** sisi tanggal — 3.000.000 sampai 30 Sep, 8.000.000 tanpa batas |
| `BadanHukumKosong_Ditolak400` | Saldo dua badan hukum tidak tercampur |
| `Endpoint_MembawaHakAksesYangCocokDenganAccessController` | **Acceptance 4** |

### Rintangan `ACC-TD-001` dan cara mengatasinya

Keempat uji yang menyimpan baris jurnal semula **gagal** dengan
`SQLite Error 19: CHECK constraint failed: CK_AccJournalLine_TepatSatuSisiTerisi`.

Sebabnya persis `ACC-TD-001` yang sudah tercatat sejak `BE-ACC-P2-002`: EF menyimpan `decimal`
sebagai TEXT pada SQLite, sehingga bagian constraint yang berbunyi `"CreditAmount" = 0` **mustahil
terpenuhi** — TEXT tidak pernah sama dengan angka. Tidak ada nilai yang dapat lolos.

Diatasi dengan menjalankan `PRAGMA ignore_check_constraints = ON` pada koneksi uji saja.
`TestDatabase` memakai satu koneksi untuk seluruh konteksnya, sehingga pragma itu cukup sekali.
**Nol perubahan pada model maupun configuration aplikasi.**

Yang dikorbankan hanya penegakan constraint itu sendiri — dan constraint itu **bukan** yang diuji
berkas ini; ia dijamin PostgreSQL. Yang diuji adalah perhitungan saldonya, dan perhitungan itu
tidak bergantung pada constraint tersebut.

### Kenapa statusnya 🟡

Kolom Verifikasi kartu berbunyi *"Test integrasi PostgreSQL memakai contoh berangka"*. Contoh
berangkanya **dijalankan** dan hijau, tetapi **di atas SQLite**, bukan PostgreSQL — penyebabnya
sama dengan `005` dan `006`: tidak ada database test yang tersedia, dan fixture-nya menolak
diarahkan ke database dev.

Yang belum terbukti karenanya: perilaku agregasi `SUM`/`GROUP BY` dan pembandingan tanggal pada
PostgreSQL sungguhan — termasuk apakah `decimal` PostgreSQL memberi hasil yang sama dengan TEXT
SQLite pada penjumlahan besar. Untuk laporan uang, itu bukan risiko yang pantas diabaikan.

**Tidak dijalankan:** migration apa pun (nol dampak schema); pemanggilan endpoint terhadap
database sungguhan.

Uji manual: `NOT FEASIBLE` — layarnya `FE-ACC-P2-008`, belum dibuat.

---

## 6. Acceptance criteria dan Definition of Done

| # | Kriteria persis seperti roadmap | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Hanya akun ber-`IsControlAccount = true` yang muncul | **Terpenuhi** | `HanyaControlAccount_YangMuncul` |
| 2 | Saldo dihitung **hanya dari baris berstatus `Posted`** | **Terpenuhi** | `SaldoDihitungHanyaDariBarisPosted` — `Draft` dan `Approved` diabaikan |
| 3 | Disaring per badan hukum dan per tanggal | **Terpenuhi** | `ControlAccountBadanHukumLain_TidakIkut` dan `DisaringMenurutTanggal_TermasukTanggalBatasnya` |
| 4 | `[AccessPermission]` terpasang | **Terpenuhi** | `Endpoint_MembawaHakAksesYangCocokDenganAccessController` |

**Empat dari empat terpenuhi**, seluruhnya dengan bukti SQLite.

### Definition of Done

| Butir | Hasil |
| --- | --- |
| Endpoint berjalan | **Ya** |
| Test hijau | **Sebagian** — 9 uji SQLite hijau; **test integrasi PostgreSQL belum dijalankan** |
| Laporan task tertulis | **Ya** — berkas ini |
| Roadmap ditandai | **Ya** — `🟡` |

**Butir DoD yang belum terpenuhi: test integrasi PostgreSQL.**

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 145 warning solution, seluruhnya pre-existing. Nol dari task ini |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Temuan yang perlu diperbaiki | **`NONE`** — nol cacat ditemukan pada kode yang disentuh |

### Masalah yang diketahui

| # | Isu | Pemilik |
| ---: | --- | --- |
| 1 | **Belum ada satu pun akun bertanda control account di database.** Endpoint ini akan menjawab daftar kosong sampai Kas Kasir, Kas Kecil, Piutang, dan Hutang ditandai — dan keempatnya belum ada di daftar akun, yang saat ini berisi 3 akun | Pemilik proses akuntansi |
| 2 | Grup Reconciliation belum ada di kontrak `ACC-API-0.8` | Rizki — ratifikasi |
| 3 | Test integrasi PostgreSQL belum tersedia | Owner Backend |
| 4 | `BE-ACC-P2-014` tetap **⛔ terblokir** `DEC-ACC-P2-011`; laporan ini hanya separuh rekonsiliasi | Rizki bersama owner Finance |
| 5 | `SwaggerDocumentationTests` tidak dapat lulus pada Release | Owner Backend |

### Risiko tersisa

| Risiko | Penjelasan |
| --- | --- |
| **Laporan yang tampak bersih karena kosong** | Selama nol akun ditandai control account, laporan ini selalu kosong — dan kosong dapat disalahartikan sebagai "tidak ada selisih". Pesannya sudah membedakan keduanya, tetapi layar wajib menampilkan pesan itu |
| Perilaku PostgreSQL belum terbukti | Lihat bagian 5. Untuk laporan uang, agregasi `decimal` pantas dibuktikan di provider sungguhan |
| Separuh rekonsiliasi | Tanpa `014`, angka ini masih harus dibandingkan manual dengan subledger |

### Status Git

```text
 M Program.cs
?? Areas/Corporate/AccountingManagement/Reconciliation/
?? Tests/.../AccountingManagement/AccControlAccountReconciliationTests.cs
```

Ditambah berkas `005`, `006`, `009`, dan dua perbaikan yang juga belum di-commit.
**Nol commit, push, stage, merge, atau rebase dilakukan.**

### Langkah berikutnya

1. **Susun daftar akun** — Kas Kasir, Kas Kecil, Piutang, Hutang — lalu tandai keempatnya sebagai
   control account lewat `POST`/`PUT` daftar akun. Tanpa itu, `013` maupun `012` tidak punya data
   untuk bekerja.
2. **`BE-ACC-P2-012`** — penolakan jurnal manual ke control account. Menunggu keputusan owner
   soal pembeda jurnal manual dan otomatis: parameter service, atau kolom `JournalSource` yang
   menuntut migration baru.
3. **`BE-ACC-P2-010`** — tutup tahun. Kedua prasyarat datanya kini sudah terpenuhi.
