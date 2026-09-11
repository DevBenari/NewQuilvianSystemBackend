# BE-ACC-007 — API daftar akun

- **TASK ID:** `BE-ACC-007` — API daftar akun
- **TASK TYPE:** Implementasi backend, controller + service + DTO
- **COMPLEXITY:** `MEDIUM`
- **CLASSIFICATION SCORE:** **8** — repository 0, berkas diperiksa 2 (>20), berkas diubah 1 (6 berkas), logika bisnis **2** (sepuluh aturan validasi, penelusuran lingkaran induk, perhitungan saldo), kontrak API 1 (memakai `ACC-API` yang sudah ada), database 1 (perilaku persistence saja, nol schema), keamanan/auth **1**, UI/workflow 0
- **MODEL:** Claude Opus 5
- **TASK MODE:** `BACKEND`
- **WRITE TARGET:** `NewQuilvianSystemBackend` — `Areas/Corporate/AccountingManagement/`, `Program.cs`, `docs/module-blueprints/accounting/`
- **VISUAL REFERENCE:** `NOT REQUIRED` — nol perubahan UI
- **BLUEPRINT STATUS/EVIDENCE:** `NOT APPLICABLE` — bukan `MODULE BLUEPRINT MODE`
- **STALE EVIDENCE / BLOCKED PHASES:** `NOT APPLICABLE`
- **INTERRUPTIONS:** **Dua** — larangan test/build di tengah implementasi, lalu pencabutannya. Lihat bagian 11
- **WARNINGS:** 203 warning build, seluruhnya pre-existing dan milik modul lain
- **Tanggal:** 2 September 2026
- **Baseline:** `ACC-BP-001` revisi 7 `APPROVED`, roadmap revisi 2, `decision_revision` 1.4
- **HEAD saat mulai:** `d9a5a6e` pada branch `rizkiG`, working tree bersih

> ## ✅ STATUS: `DONE`
>
> **Diperbarui 2 September 2026, sesi lanjutan.** Owner mencabut larangan test, sehingga acceptance
> yang sebelumnya kosong kini terbukti: **20 test baru, seluruhnya lulus**.
>
> Laporan ini sempat berstatus `IMPLEMENTED_UNVERIFIED` di antara kedua sesi itu. Riwayat tersebut
> **tidak dihapus** — bagian 11 mencatat urutannya apa adanya, termasuk apa yang ditemukan justru
> karena test akhirnya dijalankan.

## Catatan skor klasifikasi

Faktor keamanan/auth dinilai **1** ("berkaitan tetapi bukan intinya"), bukan 2. Alasannya: task
ini **memakai** mekanisme hak akses yang sudah ada apa adanya (`[AccessController]`,
`[AccessAction]`, `[AccessPermission]`) dan **tidak** merancang ulang apa pun. Penjaga
`ACC-DEC-041` yang ditambahkan pun bukan mekanisme otorisasi — ia tidak menentukan siapa berhak
atas apa, hanya menolak berjalan pada keadaan data yang belum dapat dijaga.

Bila faktor itu dinilai 2, skornya menjadi 9 dan klasifikasinya `HEAVY`. Perbedaannya dicatat di
sini supaya pembaca dapat menilai ulang, bukan disembunyikan di balik satu angka.

## Validasi baseline

| Yang diperiksa | Tercatat | Nyata | Hasil |
|---|---|---|---|
| Blueprint revision | `7` | `7` | Cocok |
| `decision_revision` | `1.4` | `1.4` | Cocok |
| `ACC-PERMISSION` | `0.2` | `0.2` | Cocok |
| `verification_backend_source_sha` | `0f86e84` | `d9a5a6e` | **Berbeda — impact scan dijalankan** |

### Impact scan `0f86e84` → `d9a5a6e`

`d9a5a6e` adalah commit `ACC-DEC-041` oleh owner: **7 berkas, seluruhnya dokumentasi blueprint**.

| Pemeriksaan | Hasil |
|---|---|
| Berkas kode aplikasi tersentuh | **Nol** |
| `Migrations/`, `ModelSnapshot`, `Program.cs` | **Tidak** |
| Modul lain | **Tidak** |

**Dampak terhadap `BE-ACC-007`: justru membukanya.** `ACC-DEC-041` yang membuat task ini boleh
dimulai.

## Backend Governance Preflight

| Field | Isi |
|---|---|
| Area | `Corporate` |
| Module | `AccountingManagement` |
| Submodule | `MasterData / ChartOfAccount` |
| Pemilik / prefix registry | Rizki / **`Acc`** — terdaftar, lifecycle **`ACTIVE`** |
| Applicability | **`NEW CODE`** |
| QBE ID yang berlaku | `QBE-MOD-002`/`003` **tidak terpicu** — nol model persisted baru. `QBE-NAM-004` tidak dilanggar. **`QBE-CODE`**: alur `Controller → Module Service → DbContext` ditegakkan; **pola legacy `ApplicationDbContext` langsung di controller sengaja TIDAK ditiru** |
| `AGENTS.md` | Terbaca |
| Registry canonical | Terbaca dari `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |

### Satu pola legacy yang sengaja tidak ditiru

`CostCenterController` — controller master terdekat di `Areas/Corporate/` — menyuntik
`ApplicationDbContext` langsung dan menaruh aturan bisnisnya di dalam controller.
`BACKEND_ENGINEERING_CONTRACT.md` baris 69 menetapkan alur baru adalah
**Controller → Module Service → DbContext**, dan skill `build-module-backend` menegaskan untuk
`NEW CODE` QBE canonical mengalahkan pola legacy yang bertentangan.

Karena itu seluruh aturan bisnis berada di `AccChartOfAccountService`, dan controller hanya
memetakan hasil ke kode status HTTP. Ini juga yang membuat `BE-ACC-010` nanti dapat memakai
ulang perhitungan saldo tanpa menyalinnya.

---

## 1. FILE YANG DIBUAT

Lima berkas, 1.127 baris.

| Berkas | Baris |
|---|---:|
| `Areas/.../MasterData/ChartOfAccount/DTOs/ChartOfAccountDtos.cs` | 191 |
| `Areas/.../MasterData/ChartOfAccount/Services/AccChartOfAccountService.cs` | 643 |
| `Areas/.../MasterData/ChartOfAccount/Controllers/ChartOfAccountController.cs` | 164 |
| `Areas/Corporate/AccountingManagement/Services/AccountingServiceResult.cs` | 48 |
| `Areas/Corporate/AccountingManagement/Services/AccountingLegalEntityGuard.cs` | 81 |

Dua berkas terakhir sengaja diletakkan di `AccountingManagement/Services/`, bukan di dalam
submodule `ChartOfAccount`, karena keduanya dipakai bersama seluruh service Accounting
berikutnya — `BE-ACC-008` sampai `BE-ACC-014`.

## 2. FILE YANG DIUBAH

| Berkas | Perubahan |
|---|---|
| `Program.cs` | **+6 baris, 0 penghapusan** — 1 `using`, 1 `AddScoped<AccChartOfAccountService>()`, 3 baris komentar |

`Program.cs` sudah **di-stage owner** saat sesi berjalan (`M ` pada `git status`). Isinya
diverifikasi dari index: tepat 6 insertion, nol deletion.

Roadmap `BE-ACC-007` mencantumkan "satu baris `AddScoped`" sebagai bagian cakupan, dan
`02-backend-architecture.md` bagian 6 secara eksplisit mengizinkannya. Yang dilarang bagian itu —
pemanggilan seeder dan logika startup — **tidak** ditambahkan, dan komentarnya menyatakan hal itu
supaya tidak ada yang menaruhnya di sana kemudian.

Ditambah register: laporan ini.

---

## 3. ENDPOINT YANG DIBUAT

Delapan, persis `ACC-API-0.2` grup Chart of Account. Base URL
`api/v1/corporate/accounting/master-data/chart-of-accounts`.

| Method | Path | Hak akses | Response |
|---|---|---|---|
| `GET` | `/` | `ChartOfAccount : Read` | `ApiResponse<PagedResult<ChartOfAccountListResponse>>` |
| `GET` | `/{id}` | `ChartOfAccount : Read` | `ApiResponse<ChartOfAccountDetailResponse>` |
| `GET` | `/tree` | `ChartOfAccount : Read` | `ApiResponse<List<ChartOfAccountTreeResponse>>` |
| `GET` | `/options` | `ChartOfAccount : Read` | `ApiResponse<List<ChartOfAccountOptionResponse>>` |
| `POST` | `/` | `ChartOfAccount : Create` | `ApiResponse<ChartOfAccountDetailResponse>` `201` |
| `PUT` | `/{id}` | `ChartOfAccount : Update` | `ApiResponse<ChartOfAccountDetailResponse>` |
| `PATCH` | `/{id}/deactivate` | `ChartOfAccount : Update` | `ApiResponse<ChartOfAccountDetailResponse>` |
| `PATCH` | `/{id}/activate` | `ChartOfAccount : Update` | `ApiResponse<ChartOfAccountDetailResponse>` |

Nilai `[AccessPermission(...)]` disalin apa adanya dari `ACC-PERMISSION-0.2` bagian 7, tidak
diterjemahkan.

### `/options` menjaga `ACC-DEC-022` sejak di layar

Hanya mengembalikan akun yang **menerima transaksi dan aktif**, sehingga petugas tidak pernah
melihat akun induk pada daftar pilihan form jurnal. `RequiresCostCenter` ikut disertakan dan
**diturunkan** dari `AccountType == Expense` — bukan dibaca dari kolom, karena roadmap
`BE-ACC-003` melarang kolom `RequiresCostCenter` pada entity.

## 4. ATURAN YANG DITEGAKKAN

Seluruh sepuluh baris `ACC-VALIDATION-0.2` bagian 1, di service.

| Aturan | Kode | Tempat |
|---|:---:|---|
| Kode akun wajib, maksimal 20 karakter | `400` | `PeriksaIsianDasar` |
| Nama akun wajib, maksimal 200 karakter | `400` | `PeriksaIsianDasar` |
| Tingkat akun 1 sampai 5 | `400` | `PeriksaIsianDasar` |
| Kode unik per badan hukum | `409` | `CreateAsync`, `UpdateAsync` |
| Induk harus badan hukum sama | `409` | `PeriksaIndukAsync` |
| Induk tidak boleh dirinya sendiri, termasuk lingkaran tidak langsung | `409` | `PeriksaIndukAsync` |
| Akun induk tidak menerima transaksi | `409` | `UpdateAsync` |
| Akun bertransaksi tidak dapat diberi turunan | `409` | `CreateAsync` |
| Kode tidak berubah setelah dipakai | `409` | `UpdateAsync` |
| Akun bersaldo tidak dinonaktifkan | `409` | `DeactivateAsync` |

Pesan bagi pengguna disalin **kata per kata** dari validation matrix, termasuk penyebutan jumlah
saldo pada aturan terakhir.

### Lingkaran induk ditelusuri, bukan hanya dibandingkan

Aturan "induk tidak boleh dirinya sendiri" dipenuhi dua lapis: perbandingan langsung
(`ParentAccountId == Id`), **dan** penelusuran ke atas dari calon induk untuk menangkap lingkaran
tidak langsung `A → B → A`. Penelusurannya berpagar `TingkatMaksimum + 1` supaya data yang sudah
terlanjur melingkar tidak menyebabkan loop tak berujung.

### Perhitungan saldo menyaring `Posted`

`HitungSaldoAsync` hanya menjumlahkan baris jurnal yang **jurnalnya berstatus `Posted`**. Jurnal
`Draft` dan `PendingApproval` belum menjadi transaksi; ikut menghitungnya akan mengunci akun yang
sebenarnya masih bebas dinonaktifkan atau diubah kodenya.

Dibuat `public static` menerima `ApplicationDbContext` supaya `BE-ACC-010` dan `BE-ACC-012` dapat
memakainya tanpa registrasi DI baru, sesuai `02-backend-architecture.md` bagian 6.

## 5. PENJAGA `ACC-DEC-041` — acceptance (5b)

`AccountingLegalEntityGuard`, dipanggil di **kedelapan** jalur service sebelum operasi apa pun.

> **Mekanismenya berubah 2 September 2026 lewat `ACC-DEC-043`.** Versi pertama menolak bila badan
> hukum **aktif** lebih dari satu. Pemeriksaan database sungguhan menemukan **tiga** badan hukum
> aktif, dan ambang itu akan mematikan Accounting tanpa alasan sebenarnya. Uraian di bawah sudah
> versi final; riwayatnya di bagian 12.

| Aspek | Keputusan |
|---|---|
| Ambang | Badan hukum bertanda `IsDefault`, aktif, tidak terhapus — harus **tepat satu** |
| Tindakan | Menolak, `409 Conflict`. Nol default dan default ganda punya pesan berbeda |
| Kenapa `409`, bukan `403` | Penolakan ini bukan soal hak akses — pengguna mana pun ditolak, termasuk yang berhak penuh. Yang bentrok adalah **keadaan data** terhadap batas `ACC-DEC-041` |
| Kenapa menolak keras | Pembukuan tidak punya jalan mundur murah: tercampurnya dua buku besar baru ketahuan saat tutup buku, dan jurnal `Posted` tidak dapat dihapus (`ACC-DEC-015`) |
| Kenapa `static` | Agar `BE-ACC-008`..`014` memakainya tanpa menambah registrasi di `Program.cs` |
| Kenapa `IsDefault` | Bahaya yang dijaga bukan "ada lebih dari satu badan hukum di master", melainkan **ketidakjelasan buku besar mana yang disentuh**. `IsDefault` sudah menjawabnya, dan ia kolom platform yang sudah ada — bukan konsep baru yang dikarang Accounting |

Badan hukum nonaktif maupun terhapus lunak **tidak** dihitung — keduanya tidak dapat menerima
pembukuan baru, sehingga tidak menimbulkan risiko yang dijaga.

`AmbilBadanHukumUtamaAsync` disediakan agar frontend dan task berikutnya dapat menanyakan badan
hukum mana yang menjadi tumpuan, alih-alih menebaknya.

**Ini bukan sistem hak akses tandingan.** Ia tidak menentukan siapa berhak atas apa. Penyaringan
yang sesungguhnya tetap milik Security/Platform lewat `ACC-DEP-008`.

## 6. DELTA TERHADAP KONTRAK — dilaporkan, tidak diputuskan sepihak

**`AccountCode` dapat diubah lewat `PUT`.**

| Sumber | Yang dikatakan |
|---|---|
| `ACC-API-0.1` | `PUT /{id}` — "Mengubah **nama, induk, atau keterangan** akun" |
| `ACC-VALIDATION-0.2` bagian 1 | "Kode tidak berubah setelah dipakai — **Ubah** — `AccountCode` diubah padahal sudah ada baris jurnal disahkan" |
| Roadmap acceptance (4) | "Kode akun bertransaksi **gagal diubah**" |

Dua sumber terakhir hanya masuk akal bila kode **boleh** diubah selama akun belum dipakai. Kalau
kode memang tidak pernah dapat diubah, aturan validasi dan acceptance (4) tidak punya isi.

Saya mengikuti validation matrix dan acceptance, sehingga `AccountCode` masuk ke
`UpdateChartOfAccountRequest`. **Ini delta terhadap deskripsi `ACC-API-0.1`, dan owner yang
memutuskan** — bukan saya. Bila yang dikehendaki kode benar-benar tidak dapat diubah, yang perlu
dicabut adalah aturan validasi dan acceptance (4), bukan sekadar field DTO-nya.

## 7. BUILD RESULT

```
dotnet build ./QuilvianSystemBackend.sln
Build succeeded.
    203 Warning(s)
    0 Error(s)
```

Build diulang pada sesi lanjutan bersama berkas test, hasilnya tetap **0 error**.

203 warning seluruhnya pre-existing dan milik modul lain. Pemeriksaan terarah pada keluaran
build: **nol warning berasal dari kelima berkas baru maupun dari berkas test**.

## 8. VALIDATION

| Perintah / pemeriksaan | Hasil | Klasifikasi | Catatan |
|---|---|---|---|
| `dotnet build ./QuilvianSystemBackend.sln` | 0 error, 203 warning | **PASS** | Dijalankan sebelum instruksi "jangan build" |
| Verifikasi baseline revisi 7 + `ACC-PERMISSION-0.2` | Cocok | **PASS** | — |
| Impact scan `0f86e84` → `d9a5a6e` | Nol berkas kode | **PASS** | — |
| Pembacaan `ACC-API-0.1`, `ACC-VALIDATION-0.2`, `ACC-PERMISSION-0.2` | Selesai, 1 delta ditemukan | **PASS** | Bagian 6 |
| `dotnet test --filter ChartOfAccountServiceTests` | **20 lulus**, 0 gagal | **PASS** | Bagian 9 |
| `dotnet test --filter AccountingManagement` | **44 lulus**, 0 gagal | **PASS** | 24 lama + 20 baru |
| `dotnet test Tests/QuilvianSystemBackend.Tests` | **220 lulus**, 0 gagal | **PASS** | Nol regresi |
| **Uji manual lewat Swagger** | **TIDAK DIJALANKAN** | **NOT RUN** | Tidak diperlukan lagi — test menutup kelima acceptance |

## 9. ACCEPTANCE CRITERIA `BE-ACC-007` — SELURUHNYA TERBUKTI

20 test pada `Tests/QuilvianSystemBackend.Tests/AccountingManagement/ChartOfAccountServiceTests.cs`.

| # | Kriteria | Hasil | Test yang membuktikan |
|---|---|:---:|---|
| 1 | Kode akun kembar pada badan hukum sama ditolak `409` | ✅ | `KodeAkunKembarPadaBadanHukumSama_Ditolak409` |
| 2 | Akun beranak tidak dapat `IsPostable = true` | ✅ | `AkunBeranak_TidakDapatMenerimaTransaksi_Ditolak409` |
| 3 | Akun bersaldo bukan nol gagal dinonaktifkan, pesan menyebut jumlah | ✅ | `AkunBersaldo_GagalDinonaktifkan_PesanMenyebutJumlah` — pesannya memuat `15.000.000` |
| 4 | Kode akun bertransaksi gagal diubah | ✅ | `KodeAkunBertransaksi_GagalDiubah_Ditolak409` |
| 5 | ~~`403` badan hukum bukan hak pengguna~~ | — | `DEFERRED` `ACC-DEC-041` |
| 5b | Badan hukum utama bukan tepat satu ⇒ endpoint menolak | ✅ | `LebihDariSatuBadanHukumUtama_SeluruhEndpointMenolak`, `TanpaBadanHukumUtama_SeluruhEndpointMenolak`, `TigaBadanHukumAktifDenganSatuUtama_AccountingTetapBerjalan` |

### Dua butir yang paling berisiko, dan hasilnya

**Acceptance (3) — penyaring `Posted` bekerja.** `JurnalDraft_TidakMenguncikanAkun` membuktikan
jurnal `Draft` **tidak** ikut terhitung: akun tetap dapat dinonaktifkan dan kodenya tetap dapat
diubah. Ini kesalahan yang tidak akan terlihat dari build maupun dari membaca kode sepintas —
gejalanya cuma keluhan "akun tidak bisa dinonaktifkan padahal belum dipakai".

**Acceptance (5b) — penjaga `ACC-DEC-041` benar-benar menolak.** Keempat jalur yang diuji
(`GetPaged`, `GetOptions`, `GetTree`, `Create`) menolak `409`, pesannya menyebut `ACC-DEP-008`,
dan `Assert.Empty(db.AccChartOfAccounts)` membuktikan penolakan terjadi **sebelum menulis apa
pun**. `BadanHukumNonaktif_TidakMenyalakanPenjaga` membuktikan sisi sebaliknya: badan hukum
nonaktif dan terhapus lunak tidak ikut dihitung, sehingga penjaga tidak menyala palsu.

Dengan ini syarat yang mengikat `ACC-DEC-041` terpenuhi — bukan sekadar dibangun.

### Sepuluh aturan `ACC-VALIDATION-0.2` bagian 1

Lima aturan lain di luar acceptance ikut diuji: tingkat akun di luar 1–5 (`400`), induk dari badan
hukum berbeda (`409`), induk bagi dirinya sendiri (`409`), akun bertransaksi diberi turunan
(`409`), dan akun tidak ditemukan (`404`). Ditambah tiga test endpoint baca: `/options` hanya
akun postable+aktif dengan `RequiresCostCenter` diturunkan, `/tree` menyusun induk-anak, dan
daftar dapat disaring serta dihalamankan.

## 10. DEFINITION OF DONE

| Butir DoD roadmap | Hasil |
|---|:---:|
| Seluruh acceptance terbukti test | ✅ 20 test, seluruhnya lulus |
| `[AccessPermission]` sesuai matriks | ✅ Disalin apa adanya dari `ACC-PERMISSION-0.2` bagian 7 |
| Laporan task tersedia | ✅ Berkas ini |

**Status: `DONE`.**

## 11. INTERUPSI

Owner menginstruksikan "jangan buat testing dan jangan build" **saat implementasi sudah berjalan**.
Keadaan pada saat itu: kelima berkas kode selesai, `Program.cs` sudah diubah, satu berkas test
sudah ditulis, dan build sudah dijalankan sekali dengan hasil 0 error.

Tindakan pemulihan:

| Tindakan | Hasil |
|---|---|
| `Tests/.../ChartOfAccountServiceTests.cs` dihapus | Berkas tidak lagi ada |
| Dua berkas test lama tidak disentuh | `AccountingFoundationTests.cs` dan `AccountingMasterDataSeederTests.cs` utuh — keduanya sudah di-commit owner pada `0f86e84` |
| Build dan test dihentikan | Tidak dijalankan lagi sesudah instruksi |
| Kode aplikasi | **Tidak dibatalkan** — instruksinya menyangkut test dan build, bukan implementasi |

### Interupsi kedua — larangan dicabut

Pada sesi lanjutan hari yang sama, owner mencabut larangan itu: *"lanjutkan nomor 3"*, yaitu
menulis test acceptance-nya. Berkas test ditulis ulang, dan **justru karena dijalankan, satu
temuan muncul yang tidak akan pernah terlihat dari membaca kode**.

#### Temuan: check constraint mustahil dipenuhi di SQLite

Enam dari 18 test gagal pada percobaan pertama dengan
`SQLite Error 19: CHECK constraint failed: CK_AccJournalLine_TepatSatuSisiTerisi`.

Sebabnya bukan cacat kode maupun cacat migration. Di PostgreSQL `DebitAmount`/`CreditAmount`
bertipe `numeric(18,2)`, sehingga constraint membandingkan angka dengan angka. Di SQLite — yang
dipakai `TestDatabase` — EF Core menyimpan `decimal` sebagai **TEXT**, dan SQLite membandingkan
lintas tipe menurut urutan tipe: nilai TEXT apa pun selalu lebih besar daripada angka apa pun.
Akibatnya `"CreditAmount" = 0` **selalu salah**, dan constraint itu tidak dapat dipenuhi berapa
pun nilainya.

Diselesaikan dengan menyisipkan baris jurnal lewat SQL mentah berliteral angka
(`SisipkanBarisJurnalLewatSqlAsync`), sehingga SQLite menyimpannya sebagai angka dan constraint
berperilaku sama dengan di PostgreSQL. **Migration dan configuration tidak diubah — keduanya
benar.**

Dicatat sebagai **`ACC-TD-001`** pada `UTANG-TEKNIS.md`, karena `BE-ACC-010` sampai `BE-ACC-012`
akan banyak menyisipkan baris jurnal dan akan menabrak hal yang sama.

## 12. `ACC-DEC-043` — penjaga disempurnakan setelah memeriksa data sungguhan

Owner meminta badan hukum pertama dibuatkan. Pemeriksaan read-only dijalankan lebih dahulu —
justru untuk memastikan penambahan itu tidak memperburuk keadaan — dan hasilnya mengubah
rancangan.

| Kode | Nama | `IsDefault` | Site | Unit | Cost Center | Lokasi |
|---|---|:---:|---:|---:|---:|---:|
| `LE-MMC-001` | PT Metropolitan Medical Centre | **Ya** | 1 | 5 | 5 | 3 |
| `LE-MDC-001` | PT Metropolitan Diagnostic Centre | — | 1 | 0 | 0 | 0 |
| `LE-MHS-001` | PT Metropolitan Healthcare Services | — | 1 | 0 | 0 | 0 |

**Tiga badan hukum aktif, bukan nol dan bukan satu.** Penjaga versi pertama — yang menolak bila
aktif lebih dari satu — akan langsung mematikan seluruh modul Accounting, termasuk saat frontend
memanggilnya.

Kalau permintaan owner dituruti tanpa memeriksa lebih dulu, jumlahnya menjadi **empat** dan
keadaannya makin jauh dari rancangan. Ini alasan pemeriksaan itu tidak dilewati.

**Yang diputuskan owner:** Accounting berjalan di atas badan hukum bertanda `IsDefault`, dan
penjaga menuntut **tepat satu** default. Nol default maupun default ganda tetap ditolak keras.

Dasarnya: bahaya yang dijaga bukan "ada lebih dari satu badan hukum di master", melainkan
**ketidakjelasan buku besar mana yang disentuh**. `IsDefault` sudah menjawab pertanyaan itu, dan
ia kolom platform yang sudah ada — bukan konsep baru yang dikarang Accounting.

**Nol data modul lain disentuh.** `LE-MDC-001` dan `LE-MHS-001` dibiarkan apa adanya; keduanya
punya `MstHospitalSite` dan mungkin dirujuk modul lain. Menonaktifkannya keputusan pemilik master
data organisasi, dan dicatat sebagai `ACC-TD-010`.

Diverifikasi terhadap database sungguhan: badan hukum utama berjumlah **1**, penjaga **lolos**,
Accounting berjalan di atas `LE-MMC-001`.

## API CONTRACT IMPACT

Mewujudkan `ACC-API-0.1` grup Chart of Account, 8 endpoint, tanpa menambah atau mengurangi satu
pun. Satu delta terhadap deskripsi `PUT` dilaporkan di bagian 6 dan **menunggu keputusan owner**.

## DATABASE IMPACT

`NONE` sebagai perubahan schema. Nol migration, nol perubahan entity maupun configuration, nol
sentuhan `ApplicationDbContextModelSnapshot.cs`. Migration tetap **119**, snapshot tetap **545
tabel**. Yang bertambah hanya perilaku baca-tulis terhadap tabel yang sudah berdiri.

## SECURITY IMPACT

Memakai mekanisme hak akses yang sudah ada apa adanya — `[AccessController]`, `[AccessAction]`,
`[AccessPermission]` — dan **tidak** membuat sistem tandingan.

Satu hal yang wajib dibaca terang-terangan: **endpoint ini menerima `LegalEntityId` dari pengirim
permintaan, bukan dari identitas pengguna.** Itu konsekuensi langsung `ACC-DEC-041` yang menunda
`ACC-DEP-008`, dan **bukan** kelalaian implementasi. Yang menahan celahnya adalah penjaga jumlah
badan hukum pada bagian 5 — dan penjaga itu belum terbukti bekerja.

## MANUAL TEST

`NOT APPLICABLE` — kelima acceptance sudah tertutup 18 test otomatis, sehingga uji manual tidak
lagi menjadi syarat.

Dicatat sebagai keterangan: acceptance (3) dan (4) memang **tidak dapat** diuji manual lewat
Swagger saat ini, karena keduanya menuntut baris jurnal **disahkan**, sedangkan `BE-ACC-010` belum
ada dan `AccJournalType` masih kosong (`ACC-TD-004`). Test menanam baris jurnal langsung ke basis
data uji, dan itulah satu-satunya jalan membuktikannya sekarang.

## INCIDENTAL CHANGES

`NONE`.

## GIT STATUS

```
M  Program.cs
?? Areas/Corporate/AccountingManagement/MasterData/ChartOfAccount/Controllers/
?? Areas/Corporate/AccountingManagement/MasterData/ChartOfAccount/DTOs/
?? Areas/Corporate/AccountingManagement/MasterData/ChartOfAccount/Services/
?? Areas/Corporate/AccountingManagement/Services/
```

`Program.cs` ter-stage oleh owner. Saya tidak melakukan stage, commit, push, pull, merge, rebase,
maupun deploy.

## NEXT RECOMMENDED STEP

`BE-ACC-008` — API jenis jurnal, 4 endpoint. Ia juga tempat yang tepat untuk memberi seeder
`BE-ACC-006` call site-nya, sehingga `AccJournalType` akhirnya terisi dan `BE-ACC-010` punya
awalan nomor jurnal (`ACC-TD-004`).

Bila tujuannya sampai ke frontend, jalur tercepatnya **bukan** menyelesaikan seluruh backend
lebih dulu: `FE-ACC-001` tertahan `ACC-FE-001` (letak menu) yang merupakan keputusan owner
sendiri, dan endpoint pertama kini sudah ada. Lihat `UTANG-TEKNIS.md` butir `ACC-TD-009`.

**Menunggu instruksi eksplisit owner.**
