# Accounting — Register Utang Teknis

Berkas ini mencatat **apa yang sengaja dilewati agar modul dapat maju**, beserta akibatnya bila
tidak pernah ditutup. Ia dibuat 2 September 2026 atas instruksi owner: *"hiraukan blocking atau
acc lead sekalipun agar project ini bisa selesai, tinggal nanti catat kekurangannya saja."*

Register ini **bukan** daftar keluhan dan bukan pengganti keputusan. Ia satu-satunya tempat yang
menjawab pertanyaan *"apa saja yang belum beres di Accounting"* tanpa harus membaca ulang tujuh
belas artefak.

| Field | Isi |
|---|---|
| Dibuat | 2 September 2026 |
| Pemilik register | Rizki (owner modul) |
| Aturan | Satu butir ditutup hanya dengan bukti, bukan dengan pernyataan. Butir yang ditutup **tidak dihapus** — ditandai `CLOSED` beserta buktinya |

## Ringkasan

| ID | Ringkas | Pemilik | Berat | Status |
|---|---|---|:---:|:---:|
| `ACC-TD-001` | Check constraint mustahil dipenuhi di SQLite | Owner modul | Rendah | `OPEN` |
| `ACC-TD-002` | Penyaringan badan hukum per pengguna tidak ada | Security/Platform | **Tinggi** | `OPEN` |
| `ACC-TD-003` | Gerbang QBE akan menolak saat merge | Lead | Sedang | `OPEN` |
| ~~`ACC-TD-004`~~ | ~~Seeder jenis jurnal belum punya call site~~ | — | — | **`CLOSED`** 2 Sep 2026 |
| `ACC-TD-005` | `UAT-15` tidak dapat dijalankan | Owner modul | Rendah | `OPEN` |
| `ACC-TD-006` | Aturan koordinasi migration belum canonical | Lead | Rendah | `OPEN` |
| `ACC-TD-007` | Satu test Billing merah sejak merge integration | Owner Billing | Rendah | `OPEN` |
| `ACC-TD-008` | 52 test Billing tidak dapat berjalan | Owner Billing | Rendah | `OPEN` |
| `ACC-TD-009` | Dua keputusan UI menahan seluruh frontend | **Rizki** | **Tinggi** | `OPEN` |
| `ACC-TD-010` | Dua badan hukum kosong di master | Owner modul lain | Rendah | `OPEN` |
| `ACC-TD-011` | `POST /seed` belum pernah dipanggil | **Rizki** | Sedang | `OPEN` |
| `ACC-TD-012` | Roadmap `BE-ACC-008` bertentangan dengan kontrak engineering | Owner modul | Rendah | `OPEN` |
| `ACC-TD-013` | `POST /seed` belum masuk `ACC-API` | Owner modul | Rendah | `OPEN` |

---

## `ACC-TD-001` — Check constraint mustahil dipenuhi di SQLite

**Ditemukan:** `BE-ACC-007`, 2 September 2026, saat test pertama kali menyisipkan `AccJournalLine`.

Di PostgreSQL, `DebitAmount` dan `CreditAmount` bertipe `numeric(18,2)`, sehingga
`CK_AccJournalLine_TepatSatuSisiTerisi` membandingkan angka dengan angka dan berperilaku benar.
Di SQLite — yang dipakai `TestDatabase` — EF Core menyimpan `decimal` sebagai **TEXT**. SQLite
membandingkan lintas tipe menurut urutan tipe, dan nilai TEXT apa pun selalu lebih besar daripada
angka apa pun. Akibatnya `"CreditAmount" = 0` **selalu salah**, dan constraint itu tidak dapat
dipenuhi berapa pun nilainya.

| Hal | Keterangan |
|---|---|
| **Ini cacat produksi?** | **Bukan.** Migration dan configuration keduanya benar untuk PostgreSQL |
| Siasat yang dipakai | `ChartOfAccountServiceTests.SisipkanBarisJurnalLewatSqlAsync` menyisipkan baris lewat SQL mentah dengan literal angka, sehingga SQLite menyimpannya sebagai angka |
| Risikonya | Setiap test berikutnya yang menyisipkan `AccJournalLine` lewat EF akan gagal dengan pesan yang **tidak menyebut** sebabnya — hanya `SQLite Error 19`. Penelusurannya mahal bila sebabnya sudah lupa |
| Cara menutup | Pindahkan test Accounting ke PostgreSQL sungguhan seperti `QuilvianSystemBackend.BillingTests`, atau sediakan pembantu bersama di `Tests/.../Infrastructure/` supaya siasat itu tidak disalin-tempel |
| Mengenai | `BE-ACC-010`, `BE-ACC-011`, `BE-ACC-012` — ketiganya akan banyak menyisipkan baris jurnal |

---

## `ACC-TD-002` — Penyaringan badan hukum per pengguna tidak ada

**Sumber:** `ACC-DEP-008`, ditunda oleh `ACC-DEC-041` pada 2 September 2026.

Ini butir **paling berat** di register ini, dan satu-satunya yang berakibat pada data keuangan
sungguhan.

Endpoint Accounting menerima `LegalEntityId` **dari pengirim permintaan**, bukan dari identitas
pengguna. Diverifikasi 2 September 2026: 17 controller memakai `[FromQuery]`, **0** klaim badan
hukum di JWT, **0** `HasQueryFilter` di seluruh repository.

| Hal | Keterangan |
|---|---|
| Yang menahan celahnya sekarang | `AccountingLegalEntityGuard` (`ACC-DEC-043`) — Accounting berjalan di atas badan hukum bertanda `IsDefault`, dan menolak `409` bila yang bertanda utama bukan tepat satu |
| Keadaan nyata per 2 Sep 2026 | **Tiga** badan hukum aktif, **satu** bertanda utama: `LE-MMC-001` PT Metropolitan Medical Centre. Penjaga lolos, Accounting berjalan |
| **Kapan menjadi berbahaya** | Saat ada yang menandai badan hukum kedua sebagai `IsDefault`, atau mencabut tanda utama dari MMC. Penjaga akan langsung mematikan seluruh modul Accounting — itu memang perilaku yang dikehendaki, tetapi akan terasa seperti kerusakan mendadak bila tidak ada yang tahu sebabnya |
| **Yang tetap terbuka** | Pengguna mana pun masih dapat mengirim `LegalEntityId` milik `LE-MDC-001` atau `LE-MHS-001` pada permintaan, dan sistem tidak menolaknya berdasarkan hak akses. Penjaga hanya memastikan tidak ada **ambiguitas** buku besar utama, bukan menyaring per pengguna |
| Cara menutup | Security/Platform menetapkan model lima lapis pada `05-prerequisite-readiness.md` bagian `ACC-DEP-008` |
| **Jangan** | Membuat penyaringan sendiri di dalam Accounting. Itu menjadi cara kedua yang berbeda dari platform, dan justru mempersulit penutupan yang benar |

**Yang tetap berlaku dan sudah ditegakkan:** pemisahan data. Kode akun tetap unik per badan hukum,
dan satu jurnal tetap tidak boleh mencampur dua badan hukum.

---

## `ACC-TD-003` — Gerbang QBE akan menolak saat merge

**Sumber:** `ACC-DEP-007`. **Diabaikan atas instruksi owner** 2 September 2026 agar pekerjaan
dapat berlanjut.

Registry di `NewQuilvianSystemBackend:docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` berisi
48 baris dan **nol baris `Acc`**, sedangkan registry canonical di suite skill
`QuilvianEngineeringSkills` berisi 52 baris dengan `Acc` berstatus `ACTIVE`.

| Hal | Keterangan |
|---|---|
| Akibat | Checker QBE — yang sudah hidup kembali lewat PR #72 `b19c01e` — diperkirakan menolak **`QBE-MOD-002 VIOLATION`** atas tujuh entity `Acc*` saat merge ke `QuilvianIntegrationBackend` |
| Tidak menghalangi | Penulisan kode lokal. Seluruh `BE-ACC-001`..`007` selesai tanpa terhalang ini |
| **Yang menumpuk** | Setiap task yang selesai menambah isi merge yang kelak tertahan. Saat ini sudah tujuh task |
| Cara menutup | Pemilik registry menambahkan satu baris di branch integration. Berkas serah terima: `evidence/03-acc-dep-007-governance-propagation.md` dan `evidence/07-acc-dep-007-ringkasan-untuk-lead.md` |

---

## ~~`ACC-TD-004`~~ — Seeder jenis jurnal belum punya call site — **`CLOSED`**

> **Ditutup 2 September 2026 oleh `BE-ACC-008`.** Call site-nya kini
> `AccJournalTypeService.SeedAsync`, dipanggil endpoint `POST /journal-types/seed`. Bukan lewat
> `Program.cs` (dilarang bagian 6) dan bukan diam-diam dari jalur `GET` (menyembunyikan siapa yang
> mengisi dan kapan). Idempotensinya dibuktikan `JournalTypeServiceTests.Seed_DijalankanDuaKali_TidakMenghasilkanDataGanda`.
>
> **Menyisakan `ACC-TD-011`:** endpoint itu belum pernah dipanggil, jadi tabelnya masih kosong.

**Sumber:** `BE-ACC-006`, keputusan owner 2 September 2026.

`AccountingMasterDataSeeder` sudah ada dan terbukti enam test, tetapi **tidak dipanggil kode
aplikasi mana pun**, sehingga tabel `AccJournalType` di database **masih kosong**.

| Hal | Keterangan |
|---|---|
| Kenapa begitu | `02-backend-architecture.md` bagian 6 melarang pemanggilan seeder di `Program.cs`; dua seeder master lain di repository ini (`EmergencyMasterDataSeeder`, `InpatientMasterDataSeeder`) juga belum punya call site |
| Akibat | `BE-ACC-010` tidak akan menemukan awalan nomor jurnal, sehingga penomoran jurnal gagal |
| **Bukan** blocker | `BE-ACC-007`, `BE-ACC-008`, `BE-ACC-009` |
| Cara menutup | Beri call site di `BE-ACC-008` — endpoint master data jenis jurnal adalah tempat yang wajar |

---

## `ACC-TD-005` — `UAT-15` tidak dapat dijalankan

**Sumber:** `ACC-DEC-041`.

`UAT-15` menguji pembukuan dua badan hukum tidak tercampur. Karena MVP diturunkan menjadi satu
badan hukum, skenarionya tidak dapat dijalankan pada rilis pertama.

Penegakan yang diujinya — kode akun unik per badan hukum, dan jurnal menolak akun milik badan
hukum lain — **tetap dibangun dan tetap diuji**, lewat
`ChartOfAccountServiceTests.KodeAkunSamaPadaBadanHukumBerbeda_Diterima` dan
`IndukDariBadanHukumBerbeda_Ditolak409`, serta nanti `BE-ACC-010` acceptance (7).

`UAT-15` kembali berlaku begitu `ACC-TD-002` ditutup.

---

## `ACC-TD-006` — Aturan koordinasi migration belum canonical

**Sumber:** `ACC-DEP-005`. `QBE-MIG-001` dan `QBE-MIG-002` masih `PROPOSED`, rumah canonical-nya
`docs/engineering/BACKEND_ENGINEERING_CONTRACT.md` di branch integration.

Tidak lagi mengikat task mana pun — `BE-ACC-006` sudah lewat memakai teks usulannya, persis
seperti yang diizinkan roadmap. Tetap dicatat karena modul lain yang membuat migration bersama
tidak punya aturan tertulis yang mengikat.

---

## `ACC-TD-007` — Satu test Billing merah sejak merge integration

`BillingFinalizationServiceTests.NormalFinalizationRequiresFullySettledOutstandingAndSetsInvoiceDate`
gagal dengan `Expected: "FINAL", Actual: "CLOSED"`.

Dibuktikan **pre-existing** pada 2 September 2026: berkas Accounting dipindahkan keluar, project
di-build ulang, dan test yang sama gagal identik. Penyebabnya semantik status folio Billing yang
bergeser lewat merge integration. **Milik owner Billing, bukan Accounting.**

---

## `ACC-TD-008` — 52 test Billing tidak dapat berjalan

Seluruh 52 kegagalan di `Tests/QuilvianSystemBackend.BillingTests` bersebab satu: environment
variable `QUILVIAN_BILLING_TEST_DB` tidak disetel. Fixture-nya **sengaja** menolak berjalan tanpa
database test tersendiri, dan daftar penanda terlarangnya memuat `dev`, `shared`, dan `prod`.

Nol test logic dijalankan. **Jangan menyetelnya sembarangan** — fixture itu menerapkan migration
ke database yang ditunjuk.

---

## `ACC-TD-009` — Dua keputusan UI menahan seluruh frontend

**Pemilik: Rizki.** Ini satu-satunya butir berat yang **berada di dalam wewenang owner sendiri**,
dan ia menahan sebelas task frontend sekaligus.

| Keputusan | Isi | Pilihan |
|---|---|---|
| `ACC-FE-001` | Letak menu Accounting di navigasi | Tiga usulan di `03-frontend-architecture.md` bagian 7 |
| `ACC-FE-003` | Bentuk layar rincian jurnal | Halaman tersendiri, panel samping, atau modal |

`FE-ACC-001` terhalang `ACC-FE-001` **dan** belum adanya endpoint. Endpoint pertama kini sudah ada
(`BE-ACC-007`), sehingga tinggal keputusan menu yang menahan.

**Bila tujuannya sampai ke frontend, butir inilah yang paling murah dibuka dan paling besar
dampaknya.**


---

## `ACC-TD-010` — Dua badan hukum kosong di master

**Ditemukan:** 2 September 2026, saat memeriksa database sebelum membuat badan hukum pertama.

`MstLegalEntity` memuat tiga baris aktif, tetapi hanya satu yang benar-benar dipakai:

| Kode | Nama | `IsDefault` | Site | Unit | Cost Center | Lokasi |
|---|---|:---:|---:|---:|---:|---:|
| `LE-MMC-001` | PT Metropolitan Medical Centre | **Ya** | 1 | 5 | 5 | 3 |
| `LE-MDC-001` | PT Metropolitan Diagnostic Centre | — | 1 | 0 | 0 | 0 |
| `LE-MHS-001` | PT Metropolitan Healthcare Services | — | 1 | 0 | 0 | 0 |

Owner menyatakan hanya membangun untuk **satu rumah sakit**, sehingga dua baris terakhir tampak
tidak terpakai. Keduanya **tidak disentuh** Accounting: masing-masing punya satu `MstHospitalSite`,
dan modul lain mungkin merujuknya. Menonaktifkannya adalah keputusan pemilik master data
organisasi, bukan Accounting.

| Hal | Keterangan |
|---|---|
| Menghalangi Accounting? | **Tidak** sejak `ACC-DEC-043` — penjaga memakai `IsDefault`, bukan jumlah |
| Risikonya | Pengguna dapat mengirim `LegalEntityId` milik keduanya pada permintaan Accounting. Akun dan jurnal akan tersimpan di bawah badan hukum yang tidak pernah dipakai, dan tidak muncul pada laporan MMC. Tidak ada yang menolaknya sampai `ACC-TD-002` ditutup |
| Cara menutup | Pastikan frontend selalu mengirim badan hukum utama — `AccountingLegalEntityGuard.AmbilBadanHukumUtamaAsync` menyediakannya. Atau pemilik master data menonaktifkan kedua baris kosong itu |


---

## `ACC-TD-011` — `POST /seed` belum pernah dipanggil

**Ditemukan:** `BE-ACC-008`, 2 September 2026.

`ACC-TD-004` menutup soal *call site*, tetapi tabelnya masih kosong sampai endpoint itu
benar-benar dipanggil satu kali terhadap database.

| Hal | Keterangan |
|---|---|
| Diverifikasi | `AccJournalType` **0 baris** pada `QuilvianNewDevRizki` per 2 September 2026 |
| Cara menutup | Panggil `POST /api/v1/corporate/accounting/master-data/journal-types/seed` sekali, dengan pengguna berhak `JournalType : Create` |
| Aman diulang | Ya — pemanggilan kedua menyisipkan nol baris |
| Akibat bila dibiarkan | `BE-ACC-010` gagal menemukan awalan nomor jurnal, sehingga penomoran jurnal tidak berjalan |
| Pemilik | **Rizki** — ini langkah operasional, bukan pekerjaan kode |

---

## `ACC-TD-012` — Roadmap `BE-ACC-008` bertentangan dengan kontrak engineering

**Ditemukan:** `BE-ACC-008`, 2 September 2026, atas permintaan owner untuk memeriksanya lebih
dahulu.

`roadmap/backend-roadmap.md` baris 361 menulis *Reuse:* "`ApplicationDbContext` langsung — CRUD
sederhana, tanpa service, sesuai konvensi", sedangkan
`docs/engineering/BACKEND_ENGINEERING_CONTRACT.md` bagian *Boundary API/service* menetapkan alur
baru adalah **Controller → Module Service → DbContext**.

**Sudah terselesaikan untuk implementasi**, oleh roadmap itu sendiri: baris 72 menyatakan
kesesuaian engineering diselesaikan dari dokumen canonical, **bukan dari roadmap**. `BE-ACC-008`
karena itu memakai service, konsisten dengan `BE-ACC-007`.

| Hal | Keterangan |
|---|---|
| Menghalangi? | **Tidak.** Implementasi sudah berjalan dan terbukti test |
| Yang tersisa | Teks roadmap baris 361 masih menyesatkan pembaca berikutnya |
| Cara menutup | Perbaiki kolom *Reuse* itu agar tidak bertentangan. Perbaikan teks, bukan perubahan target — nol dampak kode |
| Kenapa dicatat | Pembaca yang hanya membaca baris 361 akan menyimpulkan gaya yang salah, dan modul berikutnya bisa jadi tidak konsisten |


---

## `ACC-TD-013` — `POST /seed` belum masuk `ACC-API`

**Ditemukan:** `BE-ACC-008`, 2 September 2026.

`ACC-API-0.2` grup Journal Type mencantumkan **empat** endpoint. Implementasi menambah kelima,
`POST /journal-types/seed`, atas permintaan eksplisit owner agar seeder `BE-ACC-006` punya call
site pada task ini.

**Dilaporkan, tidak diputuskan sepihak.** Kontrak sengaja **belum** diubah, supaya kenaikan
versinya menjadi keputusan owner dan bukan efek samping implementasi.

| Hal | Keterangan |
|---|---|
| Menghalangi? | **Tidak.** Endpoint berjalan dan terbukti test |
| Risikonya | Pembaca `ACC-API-0.2` tidak akan tahu endpoint itu ada. Frontend yang menyusun klien dari kontrak akan melewatkannya |
| Cara menutup | Owner meratifikasi, `ACC-API` naik `0.2` → `0.3`, dan barisnya ditambahkan ke `contracts/api-contract.md` grup Journal Type |
| Rinciannya | Laporan `be-acc-008-api-jenis-jurnal.md` bagian 7 |
