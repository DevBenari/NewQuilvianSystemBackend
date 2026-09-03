# `BE-ACC-010` — Jurnal draft: simpan, ubah, hapus, dan penomoran

| Field | Isi |
|---|---|
| Task ID | `BE-ACC-010` |
| Blueprint | `ACC-BP-001` revisi `9`, `decision_revision` `1.6` |
| Status | **`DONE`** — kedelapan acceptance terbukti, `GAP-ACC-004` **TERTUTUP** |
| Tanggal | 3 September 2026 |
| Branch | `rizkiG` |
| Source SHA saat mulai | `591882828a7bf46e80a863ec741fedf40132e97b` (`5918828`), working tree bersih |
| Kontrak | `ACC-API-0.2` grup Journal (5 endpoint pertama), `ACC-VALIDATION-0.2` bagian 3, `ACC-PERMISSION-0.3`, `ACC-STATE-0.1` |
| Migration | **Nol.** Tidak ada migration dibuat, tidak ada `dotnet ef` dijalankan, snapshot tidak disentuh |
| Commit | **Belum di-commit** — di luar wewenang task ini |

## 1. Backend Governance Preflight

| Field | Isi |
|---|---|
| Area | `Corporate` |
| Module | `AccountingManagement` |
| Submodule | `JournalManagement` |
| Pemilik / prefix | Rizki — prefix `Acc`, lifecycle `ACTIVE` (`ACC-DEC-038`) |
| Keberlakuan | **`NEW CODE`** |
| Status registry | `ACTIVE` pada registry canonical suite skill; **`ABSENT`** pada salinan `docs/engineering/` repository backend — lihat bagian 8 |
| QBE ID yang berlaku | `QBE-API-001`, `QBE-CODE-003`, `QBE-CODE-006` |
| QBE ID yang **tidak** berlaku | `QBE-MOD-002`, `QBE-MOD-003`, `QBE-NAM-004` — task ini **tidak membuat satu pun entity persisted**; ketujuh entity `Acc*` sudah ada sejak `BE-ACC-001`..`005` dan sudah bermigrasi pada `BE-ACC-006`. Gerbang registry mengunci pembuatan entity operasional, bukan penulisan DTO/service/controller |
| Sumber governance | `AGENTS.md` backend; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |

`AGENTS.md` bagian *Lapisan Operasional Tata Kelola* menyatakan lapisan aturan operasional
**tidak lagi tinggal di dalam repository backend**. Karena itu ketiadaan folder `.codex` di
`NewQuilvianSystemBackend` **bukan** `BLOCKED — canonical governance unavailable`: itu memang
keadaan yang dikehendaki. Akar `rules/` terpasang untuk Claude Code ada di
`${CLAUDE_PLUGIN_ROOT}/.claude/rules/` dan terbaca.

**Satu selisih dilaporkan, tidak ditambal.** Akar terpasang itu memuat `backend/API_RULES.md`,
`DATABASE_RULES.md`, `TASK_RULES.md`, dan seterusnya, tetapi **tidak memuat**
`backend/engineering/`, sehingga `BACKEND_ENGINEERING_CONTRACT.md` dan
`MODULE_OWNERSHIP_PREFIX_REGISTRY.md` tidak ada di sana. Keduanya dibaca dari
`docs/engineering/` repository backend, persis seperti yang diperintahkan skill
`build-module-backend`. Dicatat sebagai `ACC-TD-015`.

## 2. Berkas

| Berkas | Keadaan | Baris |
|---|---|---|
| `Areas/Corporate/AccountingManagement/JournalManagement/DTOs/JournalDtos.cs` | **baru** | 235 |
| `Areas/Corporate/AccountingManagement/JournalManagement/Services/AccJournalService.cs` | **baru** | 843 |
| `Areas/Corporate/AccountingManagement/JournalManagement/Controllers/JournalController.cs` | **baru** | 156 |
| `Program.cs` | diubah | +2 (satu `using`, satu `AddScoped<AccJournalService>()`) |

Nol berkas modul lain disentuh. Nol perubahan pada `Migrations/`, `ApplicationDbContextModelSnapshot.cs`,
entity, configuration EF, `tooling/`, `agents/`, dan `.github/`.

### Endpoint

Lima endpoint pertama grup Journal `ACC-API-0.2`, base URL `api/v1/corporate/accounting/journals`:

| Method | Path | Permission |
|---|---|---|
| `GET` | `/` | `Journal : Read` |
| `GET` | `/{id}` | `Journal : Read` |
| `POST` | `/` | `Journal : Create` |
| `PUT` | `/{id}` | `Journal : Update` |
| `DELETE` | `/{id}` | `Journal : Delete` |

**Nol delta kontrak.** Kelimanya persis seperti yang tertulis di `contracts/api-contract.md`;
tidak ada endpoint yang ditambah di luar kontrak, berbeda dari `BE-ACC-008` yang menambah
`POST /seed` (`ACC-TD-013`).

### Pemakaian ulang, bukan tandingan

| Yang dipakai ulang | Dipakai di mana |
|---|---|
| `AccountingLegalEntityGuard.PeriksaAsync<T>` | Pembuka setiap method publik `AccJournalService` |
| `AccJournalTypeService.CariMenurutKodeAsync` | **Tidak dipakai** — lihat bagian 8, `AccJournalService` menerima `JournalTypeId` (Guid) dari kontrak, bukan kode |
| `AccAccountingPeriodService.AlasanPenolakanJenisJurnalAsync` | `SiapkanAsync`, pemeriksaan status periode |
| `AccountingServiceResult<T>` | Seluruh nilai balik service |
| `MstCostCenter`, `AccChartOfAccount` | Pemeriksaan baris jurnal |
| `ApiResponse<T>`, `PagedResult<T>` | Envelope controller |
| Pola `BillingNumberSeriesService` | Alokator nomor |

`AccChartOfAccountService.HitungSaldoAsync` **tidak dipakai** pada task ini: penyimpanan draft
tidak pernah perlu tahu saldo akun. Ia relevan untuk `BE-ACC-012` (buku besar). Disebutkan agar
tidak terlihat terlewat.

## 3. Mekanisme penomoran

Bentuk nomor: **`{prefix}/{yyyy}/{MM}/{00001}`**, contoh `JU/2026/09/00001`.

| Unsur | Asal |
|---|---|
| `prefix` | `AccJournalType.NumberPrefix` — **dari master**, tidak pernah dituliskan di kode |
| `yyyy`, `MM` | `AccJournal.AccountingDate`, **bukan** waktu permintaan |
| `00001` | `AccNumberSeries.CurrentValue`, lima angka |

| Kunci | Isi | Alasan |
|---|---|---|
| `SequenceKey` | `ACC_JOURNAL_{prefix}` | Memisahkan deret per jenis jurnal, sehingga `JU/2026/09/00001` dan `JP/2026/09/00001` dapat hidup berdampingan |
| `ScopeKey` | `{legalEntityId:N}_{yyyyMM}` | Memisahkan per badan hukum dan mereset per bulan akuntansi |
| `ResetPolicy` | `MONTHLY` | Bulan sudah termuat di dalam nomornya |

Alur alokasi, seluruhnya di dalam satu transaction:

1. `pg_advisory_xact_lock(hashtext('ACC_NUMBER_{SequenceKey}_{ScopeKey}'))`
2. Baca baris `AccNumberSeries` menurut `(SequenceKey, ScopeKey)`
3. Tambah `CurrentValue`, atau sisipkan baris baru bernilai 1 bila deret belum ada
4. Rangkai nomor, sisipkan jurnal beserta barisnya, `SaveChanges` sekali
5. Commit — advisory lock lepas sendiri

**Yang dilarang dan tidak dipakai:** `Count+1`, `Max+1`, counter statis, dan application-level
lock. Kepatuhan pada `QBE-CODE-003` bukan sekadar pernyataan — `AlokasikanNomorJurnalAsync`
melempar `InvalidOperationException` bila dipanggil di luar transaction, dan itu diuji.

Lock dipegang **database** dan ber-scope pada kunci nomor, sehingga tetap benar walau aplikasi
berjalan lebih dari satu instance. Pada penyedia non-PostgreSQL langkah 1 dilewati; itulah sebab
acceptance (3) menuntut database sungguhan.

**`QBE-CODE-006` — penyimpangan yang sudah disetujui.** Kontrak menghendaki *provider bersama*.
Accounting memakai tabelnya sendiri, `AccNumberSeries`, karena `ACC-DEC-004` melarangnya menulis
tabel Billing. Ini sudah tertulis di roadmap `BE-ACC-010` dan pada XML doc `AccNumberSeries`:
begitu alokator lintas modul yang benar diekstrak, Accounting berpindah ke sana lewat keputusan
arsitektur tersendiri.

## 4. Acceptance — satu per satu

Seluruhnya dibuktikan terhadap **PostgreSQL sungguhan** (`QuilvianNewDevRizki`).

### (1) Jurnal timpang tersimpan sebagai `Draft` — **TERBUKTI**

`Create_JurnalTimpang_TersimpanSebagaiDraft`. Satu baris debit Rp 500.000 tanpa lawan kredit
sama sekali. Hasil `201`, `JournalStatus = Draft`, `IsBalanced = false`, `TotalDebit = 500000`,
`TotalCredit = 0`. Dibaca ulang lewat `ApplicationDbContext` **kedua** untuk membuktikan ia
benar-benar tersimpan, bukan tertahan di memori context pertama. Sesuai `ACC-DEC-025`:
keseimbangan bukan syarat penyimpanan.

### (2) Nomor `{prefix}/{yyyy}/{MM}/{00001}`, awalan dari master — **TERBUKTI**

Dua test:

- `Create_NomorBerbentukPrefixTahunBulanUrutan` — dua jurnal berurutan menghasilkan
  `{tanda}/2099/09/00001` dan `{tanda}/2099/09/00002`. Dicocokkan juga dengan regex
  `^[A-Z0-9]+/2099/09/\d{5}$`, sehingga lebar lima angka ikut terkunci.
- `Create_AwalanNomorMengikutiMasterJenisJurnal` — `NumberPrefix` pada master **diubah di
  tengah test**, lalu jurnal berikutnya terbukti memakai awalan baru dan memulai deret
  tersendiri dari `00001`. Inilah yang membuktikan awalan benar-benar dibaca dari master dan
  bukan konstanta di kode.

### (3) Create paralel menghasilkan `JournalNumber` seluruhnya unik — **TERBUKTI**

`Create_DuaPuluhPermintaanParalel_SeluruhNomorUnik`. **Inilah penutup `GAP-ACC-004`.**

| Hal | Angka |
|---|---|
| Permintaan create paralel | **20** |
| `ApplicationDbContext` dan koneksi PostgreSQL | **20, masing-masing tersendiri** |
| Nomor yang dihasilkan | **20** |
| Nomor **unik** | **20** |
| Nomor kembar | **0** |
| Baris `AccJournal` tersimpan | **20** |
| `AccNumberSeries.CurrentValue` sesudahnya | **20** |

Yang membuat test ini bermakna adalah **koneksi yang terpisah**: satu context yang dipakai
bergantian hanya mengantre di dalam proses dan tidak pernah menguji lock database. Kedua puluh
tugas ditahan pada satu `TaskCompletionSource` dan dilepas serentak, supaya benar-benar berebut.

`CurrentValue` berhenti tepat di 20 membuktikan tidak ada nomor yang terbuang pada jalur yang
seluruhnya berhasil — walaupun nomor terlewat memang diizinkan `ACC-DEC-014`.

### (4) `JournalNumber` kembar adalah pelanggaran invariant — **TERBUKTI**

`NomorKembar_DitolakDatabaseSebagaiPelanggaranInvariant`. Sesudah satu jurnal dibuat normal,
jurnal kedua bernomor **sama persis** disisipkan **melewati service**, langsung lewat
`ApplicationDbContext`. Database menolaknya dengan `DbUpdateException` dari unique index
`(LegalEntityId, JournalNumber)`.

Ini menguji jaring terakhir: bahkan bila alokator kelak diubah keliru, nomor kembar tetap
mustahil masuk.

### (5) Baris berisi debit dan kredit sekaligus ditolak `400` + nomor baris — **TERBUKTI**

Tiga test:

- `Create_BarisDebitDanKreditSekaligus_Ditolak400BesertaNomorBaris` — baris **kedua** mengisi
  debit 500 dan kredit 500. Hasil `400`, pesan memuat **"Baris ke-2"**. Diperiksa pula bahwa
  **nol** jurnal tersimpan, jadi tidak ada penyimpanan sebagian.
- `Create_BarisKeduaSisiNol_Ditolak400BesertaNomorBaris` — kedua sisi nol pada baris bernomor 3,
  `400`, pesan memuat "Baris ke-3".
- `Create_BarisBernilaiNegatif_DitolakDenganPesanTersendiri` — debit `-1000` menghasilkan pesan
  tersendiri "tidak boleh negatif", bukan pesan satu-sisi. Urutan pemeriksaan sengaja
  mendahulukan negatif, karena hanya pesan itu yang memberi tahu cara memperbaikinya.

### (6) Akun beban tanpa Cost Center ditolak `400` — **TERBUKTI**

- `Create_AkunBebanTanpaCostCenter_Ditolak400` — akun berjenis `Expense` tanpa `CostCenterId`.
  Hasil `400`, pesan memuat nomor baris, teks "wajib menyebutkan unit biaya", **dan kode
  akunnya**.
- `Create_AkunBebanDenganCostCenter_Diterima` — sisi sebaliknya. Akun beban yang **menyertakan**
  Cost Center diterima, dan `CostCenterId` benar tersimpan pada barisnya. Tanpa test ini,
  aturannya bisa saja "menolak semua akun beban" dan tetap lulus.

Kewajiban diturunkan dari `AccountType == Expense` (`ACC-DEC-019`), bukan dari kolom tersimpan.

### (7) Akun milik badan hukum lain ditolak `409` — **TERBUKTI**

`Create_AkunMilikBadanHukumLain_Ditolak409`. Akun uji dibuat di bawah badan hukum **bukan
`IsDefault`** (`LE-MDC-001`/`LE-MHS-001`), lalu dipakai pada jurnal milik `LE-MMC-001`. Hasil
`409`, pesan "Baris ke-1: akun ... bukan milik badan hukum jurnal ini", dan **nol** jurnal
tersimpan.

Inilah penegakan `ACC-DEC-037` yang tetap hidup walau `UAT-15` tidak dapat dijalankan
(`ACC-TD-005`).

Ditambah `Create_AkunIndukTidakMenerimaTransaksi_Ditolak409` — akun `IsPostable = false` ditolak
`409` (`ACC-DEC-022`).

### (8) Jurnal beserta barisnya tersimpan dalam satu transaksi — **TERBUKTI**

Tiga test, dan yang kedua adalah buktinya yang sebenarnya:

- `Create_JurnalDanBarisnyaTersimpanBersama` — jalur berhasil. Satu kepala jurnal dan dua baris
  muncul bersama-sama, dibaca dari context terpisah.
- `Create_BarisDitolakDatabase_KepalaJurnalIkutBatal` — **jalur gagal.** Nomor dialokasikan,
  kepala jurnal disusun, lalu satu baris yang melanggar `CK_AccJournalLine_TepatSatuSisiTerisi`
  disisipkan **melewati service**, sehingga kegagalan terjadi tepat pada `SaveChanges`. Sesudah
  rollback: **nol** kepala jurnal tertinggal, dan `AccNumberSeries` kembali ke nilai semula.
  Bila kepala dan baris tidak berada dalam satu transaksi, yang tertinggal adalah jurnal
  bernomor tanpa baris — dan jurnal semacam itu adalah lubang di buku besar.
- `AlokasiNomor_DiLuarTransaction_Ditolak` — memanggil alokator tanpa transaction melempar
  `InvalidOperationException`. Gagal keras lebih baik daripada diam-diam menghasilkan nomor yang
  tidak terlindungi.

### Cakupan lain yang ikut diuji

| Test | Yang dibuktikan |
|---|---|
| `Create_PeriodeDitentukanSistemDariTanggalAkuntansi` | `AccountingPeriodId` benar terisi walau `CreateJournalRequest` **tidak punya** field itu; `PeriodCode` sesuai |
| `Create_TanggalTanpaPeriode_Ditolak422` | `422` beserta nama bulan yang terbaca — "Maret 2098" |
| `Update_BarisDikirimUtuhDanMenggantikanSeluruhnya` | Dua baris diganti satu baris; sisa **tepat 1**; nomor jurnal **tidak berpindah** |
| `Delete_JurnalDraftDitandaiTerhapusBesertaBarisnya` | `IsDelete` menyala pada jurnal **dan seluruh barisnya** |
| `Update_JurnalBukanDraft_Ditolak409` | Jurnal `Posted` menolak perubahan — pendahulu `BE-ACC-011` acceptance (4) |
| `Create_NomorBarisKembar_Ditolak400` | `LineNumber` kembar ditolak |
| `Create_KeteranganKosong_Ditolak400` | Keterangan wajib |

## 5. Hasil eksekusi

| Berkas test | `Tests/QuilvianSystemBackend.Tests/AccountingManagement/JournalServiceTests.cs` |
|---|---|
| Jumlah test `BE-ACC-010` | **22** |
| Hasil | **22 passed, 0 failed, 0 skipped** — durasi 55 detik |
| Suite Accounting seluruhnya | **120 passed, 0 failed** (98 sebelumnya + 22 baru) |
| Project `QuilvianSystemBackend.Tests` seluruhnya | **296 passed, 0 failed, 0 skipped** — durasi 3 menit 43 detik |
| `dotnet build` | **0 error**, 186 warning (seluruhnya `CS1573`/`CS1574`/`CS1587` pre-existing milik modul lain) |

Perintah yang dipakai — direktori keluaran dipisah karena backend sedang berjalan dan mengunci
apphost:

```bash
BaseOutputPath=obj/acc010build/ dotnet build ./QuilvianSystemBackend.csproj
BaseOutputPath=obj/acc010build/ dotnet test ./Tests/QuilvianSystemBackend.Tests/QuilvianSystemBackend.Tests.csproj \
  --filter "FullyQualifiedName~JournalServiceTests"
```

### Satu cacat produksi ditemukan dan diperbaiki oleh test

`Update_BarisDikirimUtuhDanMenggantikanSeluruhnya` **gagal** pada eksekusi pertama dengan
`DbUpdateConcurrencyException: expected to affect 1 row(s), but actually affected 0 row(s)`.

SQL yang tercatat memperlihatkan sebabnya: EF mengirim **`UPDATE`** atas baris jurnal pengganti,
bukan `INSERT`. Versi pertama `UpdateAsync` memanggil `RemoveRange(jurnal.Lines)` lalu
`jurnal.Lines.Clear()` lalu menambahkan baris baru **lewat navigasi yang sedang dilacak**.
Menyunting koleksi terlacak sambil menghapus isinya membuat EF memperlakukan entity yang baru
sebagai `Modified`, sehingga ia meng-`UPDATE` baris yang belum pernah ada.

Perbaikannya: baris pengganti ditambahkan lewat `DbSet.AddRange`, bukan lewat navigasi, dan
penghapusan baris lama di-`SaveChanges` lebih dahulu — keduanya di dalam **satu transaction**.
Dua `SaveChanges` itu disengaja, karena unique index `(JournalId, LineNumber)` disaring
`"IsDelete" = false`: dalam satu batch, EF bebas menyisipkan baris nomor 1 yang baru sebelum
menghapus baris nomor 1 yang lama, dan itu menabrak index.

Cacat ini **hanya tertangkap di PostgreSQL**. Alasannya dicatat di bagian 6.

## 6. Data uji: apa yang dibuat dan apa yang dihapus

Database: **`QuilvianNewDevRizki`** (`160.22.250.77`), atas izin eksplisit owner.

**Nol DDL.** Tidak ada `dotnet ef`, tidak ada migration, tidak ada `CREATE`/`ALTER`/`DROP`.
Hanya `INSERT`, `SELECT`, dan `DELETE` atas data uji sendiri.

### Kenapa database sungguhan, bukan SQLite

Dua hal yang diuji tidak ada di SQLite:

1. `pg_advisory_xact_lock` — inti acceptance (3). Tanpanya konkurensi tidak dapat dibuktikan
   sama sekali, dan `GAP-ACC-004` tidak akan tertutup.
2. `CK_AccJournalLine_TepatSatuSisiTerisi` — di SQLite constraint ini **mustahil dipenuhi**,
   karena EF menyimpan `decimal` sebagai TEXT dan perbandingan lintas tipe selalu salah. Itulah
   `ACC-TD-001`.

Efek sampingnya: menjalankan seluruh berkas ini di atas PostgreSQL **menutup `ACC-TD-001`
untuk `BE-ACC-010`** — siasat SQL mentah `SisipkanBarisJurnalLewatSqlAsync` tidak diperlukan
sama sekali di sini.

### Baris yang dibuat

Setiap test membuat lingkungannya sendiri dengan penanda acak enam karakter — `ZT{4 hex}` untuk
awalan utama dan `ZU{4 hex}` untuk awalan pembanding — yang melekat pada kode jenis jurnal,
awalan nomor, dan kode akun.

Per test:

| Tabel | Baris | Isi |
|---|---|---|
| `AccJournalType` | 1 | kode dan `NumberPrefix` = penanda |
| `AccAccountingPeriod` | 1 | `2099-09`, status `Open`, **tahun 2099 sengaja dipilih** agar tidak pernah bertabrakan dengan periode sungguhan |
| `AccChartOfAccount` | 5 | induk (tidak menerima transaksi), kas, pendapatan, beban, dan satu akun di bawah badan hukum **bukan default** |
| `AccJournal`, `AccJournalLine`, `AccNumberSeries` | bervariasi | sesuai skenario test |

Dijumlahkan atas 22 test — angka ini **diturunkan dari isi test**, sedangkan pembuktian yang
sebenarnya ada pada verifikasi sesudahnya:

| Tabel | Dibuat | Dihapus | Sisa |
|---|---|---|---|
| `AccJournalType` | 22 | 22 | **0** |
| `AccAccountingPeriod` | 22 | 22 | **0** |
| `AccChartOfAccount` | 110 | 110 | **0** |
| `AccJournal` | 32 | 32 | **0** |
| `AccJournalLine` | 34 | 34 | **0** |
| `AccNumberSeries` | 12 | 12 | **0** |
| `AccJournalApproval` | 0 | 0 | **0** |

### Baris yang dibaca saja, tidak pernah diubah

| Tabel | Perlakuan |
|---|---|
| `MstLegalEntity` | `SELECT` saja — satu badan hukum `IsDefault` (`LE-MMC-001`) dan satu yang bukan |
| `MstCostCenter` | `SELECT` saja — satu cost center existing dipakai pada test acceptance (6) |

### Verifikasi kebersihan sesudah seluruh test selesai

Diperiksa langsung ke database sesudah eksekusi terakhir:

```
AccJournalType 0   AccChartOfAccount 0   AccAccountingPeriod 0
AccJournal 0       AccJournalLine 0      AccNumberSeries 0     AccJournalApproval 0
sisa penanda ZT/ZU pada AccJournalType     : 0
sisa penanda ZT/ZU pada AccChartOfAccount  : 0
AccAccountingPeriod tahun >= 2098          : 0
MstLegalEntity IsDefault aktif             : 1   (tidak berubah)
MstCostCenter hidup                        : 5   (tidak berubah)
```

Ketujuh tabel `Acc*` kembali **kosong**, persis seperti keadaan sebelum task ini dijalankan
(diperiksa juga di awal). Master data modul lain tidak berubah sama sekali.

## 7. Penghapusan berkas test

Atas instruksi owner, berkas test dihapus sesudah seluruh acceptance terbukti hijau, dan laporan
ini menjadi **satu-satunya bukti yang tersisa**.

| Hal | Keterangan |
|---|---|
| Berkas dihapus | `Tests/QuilvianSystemBackend.Tests/AccountingManagement/JournalServiceTests.cs` — 914 baris, 22 test, 1 berkas |
| Berkas test task sebelumnya | **Tidak ada yang ikut terhapus.** `AccountingFoundationTests`, `AccountingMasterDataSeederTests`, `ChartOfAccountServiceTests`, `JournalTypeServiceTests`, dan `AccountingPeriodServiceTests` **tetap utuh** |
| `dotnet build` sesudah penghapusan | **0 error** |
| Suite sesudah penghapusan | **274 passed, 0 failed** (296 − 22) |

### Akibatnya pada kemampuan mendeteksi regresi

Ini kehilangan yang nyata dan perlu dinyatakan terang-terangan:

| Yang hilang | Akibat |
|---|---|
| **Deteksi regresi penomoran konkuren** | Paling berat. Bila `AlokasikanNomorJurnalAsync` kelak diubah — advisory lock dilepas, atau alokasi dipindah ke luar transaction — **tidak ada** yang menangkapnya. Yang tersisa hanya unique index, dan ia menolak dengan `500` saat produksi, bukan saat build |
| **Deteksi regresi `UpdateAsync`** | Cacat EF di bagian 5 persis jenis yang kambuh saat kode disentuh lagi. Tidak ada lagi yang menjaganya |
| **Kunci bentuk nomor** | `{prefix}/{yyyy}/{MM}/{00001}` tidak lagi terkunci test. Perubahan bentuk nomor tidak akan ketahuan sampai ada yang melihat nomor jurnalnya |
| **Kunci 13 aturan validasi** | Pesan bernomor baris, kode akun pada pesan, dan pemetaan `400`/`409`/`422` tidak lagi terjaga |
| **Bukti satu transaksi** | Jaminan "kepala dan baris tidak pernah terpisah" kembali menjadi pernyataan, bukan bukti berjalan |

`BE-ACC-011` akan menyentuh `AccJournalService` yang sama untuk daur hidup persetujuan, **tanpa
jaring pengaman apa pun atas jalur CRUD dan penomoran di bawahnya.**

Dicatat sebagai **`ACC-TD-016`**. Cara menutupnya: menulis ulang berkas ini dari bagian 4 dan 6
laporan ini — keduanya sengaja ditulis cukup rinci untuk itu.

## 8. Delta, penyimpangan, dan yang dilaporkan bukan diputuskan

### `ACC-TD-014` — pemeriksaan status periode lebih awal daripada yang didaftar kontrak

Owner meminta `AccAccountingPeriodService.AlasanPenolakanJenisJurnalAsync` dipakai ulang, dan itu
dikerjakan. Tetapi perlu dinyatakan: aturan **"periode menerima jenis jurnal ini"** terdaftar di
`ACC-VALIDATION-0.2` **bagian 4** — *saat diajukan dan saat disahkan* — bukan bagian 3 yang
mengatur penyimpanan draft. Bagian 3 hanya menuntut **periodenya ada**.

Akibat nyata: menyusun draft `JU` ke periode yang sudah `SoftClosed` kini ditolak `422` **sejak
penyimpanan**, padahal menurut bagian 3 ia mestinya tersimpan dan baru ditolak saat pengajuan.

| Hal | Keterangan |
|---|---|
| Merugikan? | Tidak menghilangkan data. Penolakannya lebih awal dan pesannya sama |
| Kenapa tidak diputuskan sendiri | Ini perubahan perilaku yang akan ditemui frontend. Presedennya `ACC-TD-013`: `BE-ACC-008` menambah endpoint atas permintaan owner dan tetap mencatatnya untuk diratifikasi |
| Cara menutup | Owner meratifikasi, lalu barisnya ditambahkan ke `ACC-VALIDATION` bagian 3 dan versinya naik |
| **`BE-ACC-011` tetap wajib** | Memeriksa ulang saat `submit` **dan** `post`. Periode dapat berubah status sesudah draft tersimpan, jadi acceptance (1) dan (5) `BE-ACC-011` tidak berkurang sedikit pun |

### `AccJournalTypeService.CariMenurutKodeAsync` tidak dipakai

Owner mencantumkannya pada daftar pakai-ulang. Ia **tidak** dipakai, dan alasannya bukan
kelalaian: `ACC-API-0.2` menetapkan `CreateJournalDto` membawa **`JournalTypeId`** bertipe Guid,
bukan kode. Service mencarinya menurut Id. Memakai pencarian menurut kode berarti mengubah
kontrak yang sudah disetujui.

Kode jenis jurnal tetap dibaca — dari entity yang sudah ditemukan menurut Id — untuk diteruskan
ke `AlasanPenolakanJenisJurnalAsync`. Jadi tidak ada pencarian tandingan yang ditulis.

### `ACC-TD-015` — dua salinan registry berselisih **dua arah**

Ini menajamkan `ACC-TD-003`, yang selama ini mencatat backend "tertinggal tepat satu
pendaftaran". Diperiksa 3 September 2026, `BACKEND_ENGINEERING_CONTRACT.md` kedua salinan
**identik**, tetapi registry-nya tidak:

| Baris | `NewQuilvianSystemBackend/docs/engineering/` | Suite skill `QuilvianEngineeringSkills` |
|---|---|---|
| `AccountingManagement / Acc` | **tidak ada** | `ACTIVE` |
| `LaboratoryManagement / Lab` | **`ACTIVE`** | `PLANNED` |
| Changelog `Lab` 2026-09-02 | **ada** | tidak ada |
| Jumlah baris | 97 | 100 |

**Tidak ada yang merupakan superset dari yang lain.** Masing-masing memuat yang tidak dimiliki
yang lain, jadi menyalin satu arah akan menghapus pekerjaan orang lain — `Lab` `ACTIVE` milik
Muhammad Hamzah akan hilang bila salinan suite ditimpakan ke backend.

**Tidak ditambal.** Menambahkan baris `Acc` ke salinan backend akan meloloskan gerbang QBE untuk
PR sendiri, dan itu persis yang tidak boleh dilakukan. Ini pekerjaan pemilik registry.

### `ACC-TD-011` masih terbuka, dan sekarang lebih terasa

`AccJournalType` **masih 0 baris** di `QuilvianNewDevRizki` — diperiksa 3 September 2026.
`POST /journal-types/seed` belum pernah dipanggil.

Akibatnya untuk task ini: **nol jurnal dapat dibuat lewat API di lingkungan itu**, karena tidak
ada jenis jurnal yang dapat dipilih, sehingga tidak ada awalan nomor. Test membuat jenis
jurnalnya sendiri dan menghapusnya kembali, jadi acceptance tetap terbukti — tetapi
**pemakaian sungguhan masih tertahan** sampai owner memanggil endpoint itu satu kali.

Ini langkah operasional milik owner, bukan pekerjaan kode.

## 9. Definition of Done

| Syarat DoD | Keadaan |
|---|---|
| Acceptance terbukti test | **Terpenuhi** — kedelapan butir, 22 test hijau di PostgreSQL sungguhan |
| `GAP-ACC-004` tertutup | **TERTUTUP** — 20 create paralel, 20 nomor unik, 0 kembar |
| Laporan task tersedia | **Terpenuhi** — dokumen ini |

`BE-ACC-010` **`DONE`**.

## 10. Blocker yang tersisa

| ID | Ringkas | Pemilik | Memblokir `BE-ACC-011`? |
|---|---|---|---|
| `ACC-TD-011` | `POST /seed` belum pernah dipanggil; `AccJournalType` masih kosong | **Rizki** | Tidak memblokir penulisan kode. Memblokir pemakaian sungguhan |
| `ACC-TD-016` | Berkas test `BE-ACC-010` dihapus; nol jaring regresi atas CRUD dan penomoran | **Rizki** | Tidak memblokir, tetapi menaikkan risikonya |
| `ACC-TD-014` | Pemeriksaan periode lebih awal daripada kontrak, menunggu ratifikasi | Owner modul | Tidak |
| `ACC-TD-015` | Dua salinan registry berselisih dua arah | Lead / pemilik registry | Tidak. Memblokir merge ke integration |
| `ACC-TD-003` / `ACC-DEP-007` | Gerbang QBE menolak saat merge | Lead | Tidak. Memblokir merge |
| `ACC-TD-002` / `ACC-DEP-008` | Penyaringan badan hukum per pengguna tidak ada | Security/Platform | Tidak — `NON-BLOCKING` sejak `ACC-DEC-041` |
| `ACC-TD-009` | `ACC-FE-001`, `ACC-FE-003` menahan seluruh frontend | **Rizki** | Tidak. Memblokir seluruh task frontend |

## 11. Task berikutnya

**`BE-ACC-011`** — jurnal: pengajuan, persetujuan, penolakan, pengesahan. Dependency-nya
(`BE-ACC-010`) kini `DONE`.

Roadmap menandainya **risiko tertinggi pada modul ini**: acceptance (1) dan (4) adalah invariant
akuntansi, dan kegagalan di sana merusak seluruh laporan. Dikerjakan hanya atas instruksi
eksplisit owner.

Dua hal yang sudah siap dipakai `BE-ACC-011`:

- `AccJournalService.AlokasikanNomorJurnalAsync` bersifat `public static`, sehingga jurnal
  pembalik pada `BE-ACC-013` memakai alokator yang sama tanpa registrasi DI baru.
- `JournalDetailResponse.AvailableActions` sudah ada tetapi baru diisi menurut **status saja**.
  Penyaringan menurut hak akses dan aturan pembuat-bukan-penyetuju (`ACC-DEC-016`) adalah
  acceptance (6) `BE-ACC-011`.
