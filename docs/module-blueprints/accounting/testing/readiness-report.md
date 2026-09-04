# Accounting — Laporan Kesiapan End-to-End Backend MVP

| Field | Nilai |
|---|---|
| `blueprint_id` | `ACC-BP-001` |
| Revision manifest | `9` — **`MODULE-STATUS.md` menulis `10`**, lihat `ACC-GAP-003` |
| Status blueprint | `approved` |
| `current_phase` | `ACC-PH-005` |
| `decision_revision` | `1.6` |
| Contract version berlaku | `ACC-API-0.3`, `ACC-STATE-0.1`, `ACC-VALIDATION-0.3`, `ACC-PERMISSION-0.3`, `ACC-INTEGRATION-0.2`, `ACC-TEST-0.1`, `ACC-XMOD-0.1` |
| Backend source SHA — audit ini | `822d48a7268bdf69c37b39d32fb5c023af74f763`, branch `rizkiG`, working tree **bersih** |
| Backend source SHA — verification manifest | `f879944` — **tertinggal 4 commit**, lihat `ACC-GAP-006` |
| Frontend source SHA — audit ini | `1a86d9322`, branch `QuilvianIntegrationFrontend` |
| Baseline canonical integration | `f90bcbe` (termuat); tip `origin/QuilvianIntegrationBackend` = `f103fff`; merge base = `8c45762` |
| Sifat audit | **Read-only.** Nol source diubah, nol database disentuh, nol commit, nol push |
| Tanggal | 4 September 2026 |
| Penulis | `/qv-verify` atas permintaan Rizki |

Seluruh **17 dari 17** hash artefak canonical pada `blueprint-manifest.md` **cocok** saat
diverifikasi ulang. Tidak ada artefak yang isinya bergeser diam-diam.

---

## 1. Verdict

# `NOT_READY`

**Bukan karena kodenya kurang.** Backend MVP Accounting sudah ditulis lengkap dan setia pada
kontrak: 31 endpoint berdiri, seluruh aturan status dan validasi terwujud di source, hak akses
terpasang di setiap endpoint, dan solusinya kompilasi bersih. Pekerjaan menulis kode memang
selesai.

Yang membuat modul ini belum siap adalah tiga hal yang tidak dapat ditutup dengan menulis kode
lagi:

| # | Sebab | Kalimat singkatnya |
|---|---|---|
| 1 | **Buktinya tidak dapat diperiksa ulang siapa pun** | Bukti acceptance `BE-ACC-010`..`014` bersandar pada satu berkas test yang **tidak pernah masuk git** dan kini tidak ada di mana pun |
| 2 | ~~**Modul belum pernah dijalankan sekali pun sebagai proses bisnis**~~ | **SEBAGIAN BESAR TERSELESAIKAN 4 September 2026.** Owner mengisi COA dan membangkitkan periode 2026 (12 baris) lewat layar, lalu **menyusun dan mengajukan jurnal pertama modul ini lewat layar juga**: `JB/2026/09/00001`, Rp 1.000.000 seimbang, 2 baris, 1 baris riwayat persetujuan, status **Menunggu Persetujuan**. Penomoran `BE-ACC-010` terbukti pada data sungguhan. **Sisa yang belum terbukti: setujui dan sahkan** — bukan cacat, melainkan karena layar aksinya (`FE-ACC-007`) belum dibangun. `UAT-01` karena itu baru tuntas separuh |
| 3 | **Pintu merge masih tertutup** | Gerbang QBE akan menolak `QBE-MOD-002` atas tujuh entity `Acc*` — sudah tercatat `ACC-TD-003`/`ACC-TD-015`, pemiliknya lead |

Frontend `NOT_STARTED` **tidak** dihitung sebagai penyebab. Sesuai keterangan owner, itu keputusan
sadar dan berada di luar ruang lingkup audit ini.

### Kenapa bukan `READY_WITH_CONDITIONS`

`READY_WITH_CONDITIONS` dipakai ketika risikonya terbatas, pemiliknya jelas, dan mitigasinya sudah
berjalan. Di sini risikonya tidak terbatas: **invariant akuntansi tidak dijaga apa pun**, dan
pernyataan bahwa invariant itu pernah lulus tidak dapat dibuktikan ulang oleh siapa pun selain
owner. Pada modul keuangan, kerusakan jenis ini tidak memunculkan pesan error — sistem tetap
berjalan dan menghasilkan angka yang keliru. Itu melampaui "risiko terbatas".

Jaraknya ke `READY` **pendek dan konkret**: empat langkah pada bagian 8, dan dua di antaranya
bersifat mekanis.

---

## 2. Papan skor kesiapan

Kemajuan fondasi sengaja dipisahkan dari kesiapan sesungguhnya. Build yang sukses **bukan** bukti
kesiapan.

| Dimensi | Bobot | Skor | Bukti | Gap / blocker |
|---|---:|---:|---|---|
| **Fondasi** | 15% | **10 / 10** | 7 entity `Acc*` dan 6 configuration di `Areas/Corporate/AccountingManagement/` serta `Repositories/Configurations/.../AccountingManagement/@822d48a`; migration `20260902081432_AddAccountingFoundation` sudah diterapkan; snapshot 545 tabel, bertambah 751 baris, **0 deletion** | — |
| **Backend — kelengkapan** | 25% | **10 / 10** | **31 dari 31** endpoint kontrak berdiri; 5 controller, 5 service; `dotnet build QuilvianSystemBackend.sln` → **0 error**, 23 warning, dijalankan 4 Sep 2026 pada `822d48a` | — |
| **Backend — kesetiaan kontrak** | 20% | **9 / 10** | `ACC-API` 31/31 rute cocok; `ACC-STATE` seluruh transisi sah dan tidak sah terwujud; `ACC-VALIDATION` bagian 3–6 terwujud beserta kode `400`/`409`/`422`; `ACC-PERMISSION` 31/31 endpoint bertanda `[AccessPermission]` sesuai matriks | Daftar DTO `ACC-API` sudah tidak cocok dengan source (`ACC-GAP-004`); satu transisi tidak terdaftar (`ACC-GAP-008`) |
| **Keamanan / otorisasi** | 10% | **6 / 10** | `AccessPermissionFilter` benar-benar menegakkan izin; penjaga `AccountingLegalEntityGuard` dipanggil pada **31 dari 31** method service; pencatatan logger persis satu `GET`, yaitu `/trial-balance`, sesuai `ACC-DEC-032` | `ACC-TD-002` — penyaringan badan hukum per pengguna **tidak ada**; `LegalEntityId` masih datang dari pengirim permintaan |
| **Data / konfigurasi runtime** | 10% | **3 / 10** | `AccJournalType` terisi 4 baris — `JB`, `JP`, `JU`, `SA` — lewat `POST /seed`, idempotensinya terbukti pada database sungguhan (`ACC-TD-011` `CLOSED`) | **Daftar akun kosong**, **periode belum dibangkitkan**, sehingga modul tidak dapat dipakai. Bukan cacat kode, tetapi menahan seluruh UAT |
| **Verifikasi** | 15% | **2 / 10** | `BE-ACC-001`..`009`: **83 method test** masih dapat dipulihkan dari git pada `afa91f0^`. Suite yang berjalan sekarang: **1000 lulus / 1 gagal / 1001 total** pada `UnitTests.InMemory` | **Nol** berkas test Accounting di `822d48a`. Bukti `BE-ACC-010`..`014` **tidak dapat dipulihkan**. **0 dari 19** UAT punya bukti eksekusi |
| **Integrasi / merge** | 5% | **0 / 10** | Checker QBE hidup di `tooling/qbe/Invoke-QbeConformanceCheck.ps1`; workflow berjalan `Strict` untuk PR ke `QuilvianIntegrationBackend` | Registry backend **97 baris, nol baris `Acc`**, sehingga `QBE-MOD-002 VIOLATION` atas 7 entity |
| **Frontend** | — | **`NOT_STARTED`** | Nol berkas Accounting di `QuilvianSystemFrontendDev@1a86d933` | **Di luar ruang lingkup audit ini** atas keterangan owner. Tertahan `ACC-FE-001` dan `ACC-FE-003` (`ACC-TD-009`) |

**Skor tertimbang backend MVP: 6,4 dari 10.** Angka ini turun bukan karena fiturnya kurang,
melainkan karena dimensi verifikasi, data runtime, dan integrasi.

---

## 3. Jawaban atas empat pertanyaan owner

### Pertanyaan 1 — Apakah 14 task benar-benar memenuhi acceptance-nya?

**Tidak ada satu pun task yang ditandai `DONE` tanpa bukti apa pun.** Tetapi kualitas buktinya
terbelah menjadi tiga tingkat, dan perbedaannya penting.

| Tingkat | Task | Bukti hari ini | Dapat diperiksa ulang orang lain? |
|---|---|---|:---:|
| **A — kuat** | `BE-ACC-001`..`006` | Entity, configuration, migration, dan snapshot ada di source dan dapat dibaca langsung, ditambah test yang masih tersimpan di riwayat git | **Ya** |
| **B — dapat dipulihkan** | `BE-ACC-007`, `008`, `009` | Berkas test-nya **pernah dilacak git** dan dihapus pada `afa91f0`. Isinya masih utuh di `afa91f0^` | **Ya**, lewat `git show` |
| **C — tidak dapat dipulihkan** | `BE-ACC-010`..`014` | Bersandar seluruhnya pada `JournalLifecycleTests.cs`. Berkas itu **tidak pernah ada di git** dan **tidak ada di disk** | **Tidak** |

Rincian tingkat B — jumlah method test yang masih dapat dipulihkan dari `afa91f0^`:

| Berkas di `Tests/QuilvianSystemBackend.Tests/AccountingManagement/` | `[Fact]` / `[Theory]` |
|---|---:|
| `AccountingFoundationTests.cs` | 18 |
| `ChartOfAccountServiceTests.cs` | 20 |
| `AccountingPeriodServiceTests.cs` | 21 |
| `JournalTypeServiceTests.cs` | 18 |
| `AccountingMasterDataSeederTests.cs` | 6 |
| **Jumlah** | **83** |

Diverifikasi: `git log --all -- '*JournalLifecycleTests*'` mengembalikan **kosong**, jadi berkas
itu memang tidak pernah masuk git. `UTANG-TEKNIS.md` sudah mencatat hal ini dengan jujur sebagai
*"belum terlacak"* pada tabel `ACC-TD-016`, sehingga ini **bukan temuan baru**. Yang perlu
ditegaskan adalah akibatnya, yang lebih tajam daripada yang terbaca sekilas: **dua pertiga jaring
pengaman dapat dipulihkan dengan satu perintah git, sepertiga sisanya harus ditulis ulang dari
nol.** Yang sepertiga itu justru yang menjaga invariant paling mahal — penomoran jurnal saat
banyak orang menyimpan bersamaan, kontrol orang kedua, dan ketidakberubahan jurnal yang sudah
disahkan.

Dua catatan tambahan pada tingkat kepatuhan Definition of Done:

| Task | DoD yang tidak terpenuhi | Sudah tercatat? |
|---|---|:---:|
| `BE-ACC-012` | *"hasil verifikasi performa tercatat"* — tidak dapat dipenuhi karena `AccJournal` masih 0 baris | **Ya**, `ACC-TD-018`, dan status roadmap menyebutkannya terbuka |
| `BE-ACC-012` | `GAP-ACC-003` mengikat task ini: *"ditetapkan cara mengujinya, **atau** diterima sebagai pemeriksaan manual yang dicatat"* | **Tidak** — lihat `ACC-GAP-005` |

**Bukti tambahan yang saya kumpulkan sendiri pada `822d48a`**, karena `ACC-TD-019` ditutup dengan
catatan *"belum dikompilasi, verifikasi kompilasi menyusul"*:

| Pemeriksaan | Hasil |
|---|---|
| `dotnet build ./QuilvianSystemBackend.sln` | **0 error**, 23 warning. Dijalankan ke direktori keluaran terpisah, lalu direktorinya dihapus; working tree tetap bersih |
| Pengunci `RequiresApproval` di source | `AccJournalTypeService.CreateAsync` memaksa `RequiresApproval = true`; `UpdateAsync` tidak menyentuhnya sama sekali; kolomnya sudah dicabut dari `CreateJournalTypeRequest` dan `UpdateJournalTypeRequest` |

Jadi **verifikasi kompilasi yang tertunda pada `ACC-TD-019` kini terpenuhi.** Butir itu boleh
dianggap tuntas seluruhnya.

### Pertanyaan 2 — Apakah keempat kontrak benar-benar terwujud di source?

Catatan awal. Baseline yang Anda kirim menyebut `ACC-API-0.2` dan `ACC-VALIDATION-0.2`. Keduanya
sudah naik ke **`0.3`** pada 3 September 2026 lewat ratifikasi `ACC-TD-013` dan `ACC-TD-014`.
Audit ini menilai terhadap `0.3`.

| Kontrak | Terwujud? | Bukti | Delta |
|---|:---:|---|---|
| `ACC-API-0.3` | **Ya, untuk rutenya** | **31 dari 31** endpoint berdiri dengan path, method, dan hak akses persis seperti kontrak | **Daftar DTO-nya tidak lagi cocok** — `ACC-GAP-004`. Header `contract_version` masih tertulis `0.2` — `ACC-GAP-002` |
| `ACC-STATE-0.1` | **Ya** | Bagian 1.1, 1.2, 1.3, 2.1, 2.2, 2.3, dan 3 seluruhnya terwujud di `AccJournalService` dan `AccAccountingPeriodService` | Satu transisi **lebih longgar** daripada kontrak — `ACC-GAP-008` |
| `ACC-VALIDATION-0.3` | **Ya** | Bagian 3 berisi 14 aturan, bagian 4 berisi sembilan syarat, bagian 5 berisi 7 aturan pembalikan, bagian 6 berisi 6 aturan periode — seluruhnya ada beserta pesan dan kode statusnya | Nol delta |
| `ACC-PERMISSION-0.3` | **Sebagian** | Bagian 2 dan 3 terwujud penuh: 31/31 endpoint bertanda `[AccessPermission]` yang cocok; pencatatan logger persis satu `GET` | Bagian 5 aturan kedua **sengaja ditunda** `ACC-DEC-041` dan digantikan penjaga `IsDefault` — sudah tercatat `ACC-TD-002` |

Pembuktian rinci `ACC-VALIDATION-0.3` bagian 4 — kesembilan syarat, seluruhnya berada di
`AccJournalService.PeriksaSembilanSyaratAsync`, dan method itu dipanggil **dua kali**, yaitu oleh
`SubmitAsync` dan `PostAsync`, persis seperti yang dituntut kontrak:

| Syarat | Ada di source? | Kode status |
|---:|:---:|:---:|
| 1 — total debit sama dengan total kredit | Ya | `400` |
| 2 — sekurang-kurangnya dua baris | Ya | `400` |
| 3 — tepat satu sisi terisi dan lebih dari nol | Ya | `400` |
| 4 — seluruh akun aktif | Ya | `400` |
| 5 — seluruh akun menerima transaksi | Ya | `409` |
| 6 — akun milik badan hukum yang sama | Ya | `409` |
| 7 — akun beban wajib menyebut Cost Center | Ya | `400` |
| 8 — Cost Center aktif dan sebadan hukum | Ya | `409` |
| 9 — periode menerima jenis jurnal ini | Ya | `422` |

Penjaga badan hukum `ACC-DEC-043` terpasang **menyeluruh**: seluruh **31** method publik pada
kelima service memanggil `AccountingLegalEntityGuard.PeriksaAsync` sebagai baris pertamanya. Tidak
ada satu pun pintu belakang yang melewatinya.

### Pertanyaan 3 — Apakah `UAT-01` sampai `UAT-17` sudah punya bukti?

**Belum. Nol dari sembilan belas.**

Tiga hal perlu diluruskan lebih dahulu:

1. **Denominatornya 19, bukan 17.** `testing/acceptance-test-matrix.md` memuat `UAT-01` sampai
   **`UAT-19`**. `UAT-18` dan `UAT-19` sengaja ditambahkan pada 1 September 2026 untuk menutup
   `GAP-ACC-001` dan `GAP-ACC-002`, dan keduanya berkategori `MUST HAVE`.
2. **Matriksnya belum punya tempat untuk menaruh bukti.** Berkas itu berstatus `draft`,
   `approved_by` dan `approved_at` **belum ada**, dan tabelnya hanya memuat kolom *"Bukti yang
   diharapkan"*. Tidak ada kolom untuk bukti yang benar-benar terjadi, tidak ada kolom status, dan
   tidak ada catatan eksekusi.
3. **Laporan task tidak mengklaim UAT.** Dari 14 laporan, hanya satu yang menyebut UAT sama
   sekali. Laporan membuktikan **acceptance task**, bukan **skenario UAT**. Keduanya berbeda: UAT
   ditulis sebagai alur pengguna dari ujung ke ujung.

Keadaan tiap skenario:

| UAT | Isi | Penegakan ada di source? | Bukti eksekusi | Penahan hari ini |
|---|---|:---:|:---:|---|
| `UAT-01` | Susun daftar akun, catat jurnal pertama sampai disahkan | Ya | **Tidak ada** | Daftar akun dan periode kosong |
| `UAT-02` | Jurnal belum seimbang tertahan | Ya — syarat 1 | **Tidak ada** | Sama |
| `UAT-03` | Menyetujui jurnal sendiri ditolak `403` | Ya — `PeriksaBukanJurnalSendiri` | **Tidak ada** | Sama, ditambah butuh dua akun pengguna |
| `UAT-04` | Akun beban tanpa unit biaya tertahan | Ya — syarat 7 | **Tidak ada** | Sama |
| `UAT-05` | Dua petugas menyimpan bersamaan | Ya — advisory lock dan unique index | **Tidak ada** | Sama. Test penutup `GAP-ACC-004` sudah dihapus |
| `UAT-06` | Periode tutup sementara menolak `JU` | Ya — syarat 9 | **Tidak ada** | Sama |
| `UAT-07` | Periode tutup sementara menerima `JP` | Ya — syarat 9 | **Tidak ada** | Sama |
| `UAT-08` | Buka kembali tutup permanen menjadi tutup sementara | Ya — `ReopenAsync` | **Tidak ada** | Periode belum dibangkitkan |
| `UAT-09` | Buka kembali tanpa alasan ditolak `400` | Ya — `ReopenAsync` | **Tidak ada** | Sama |
| `UAT-10` | Koreksi lewat pembalikan penuh | Ya — `ReverseAsync`, jenis `JB` | **Tidak ada** | Belum ada jurnal berstatus disahkan |
| `UAT-11` | Koreksi lewat penyesuaian | Ya — `ReverseAsync`, jenis `JP` | **Tidak ada** | Sama |
| `UAT-12` | Membalik dua kali ditolak `409` | Ya — pemeriksaan `ReversalOfJournalId` | **Tidak ada** | Sama |
| `UAT-13` | Jurnal disahkan tidak dapat diubah maupun dihapus | Ya — `PeriksaDapatDisunting` | **Tidak ada** | Sama |
| `UAT-14` | Neraca saldo seimbang dan tidak mencampur status | Ya — `BarisDisahkan` | **Tidak ada** | Sama |
| `UAT-15` | Dua badan hukum tidak tercampur | — | **`DEFERRED`** | Sah, dan sudah tercatat `ACC-TD-005` |
| `UAT-16` | Saldo awal menjadi titik mulai pembukuan | Ya — jalur jurnal `SA` | **Tidak ada** | Daftar akun dan periode kosong |
| `UAT-17` | Akun bersaldo gagal dinonaktifkan | Ya — `HitungSaldoAsync` | **Tidak ada** | Sama |
| `UAT-18` | Mengubah jenis jurnal sistem ditolak | Ya — penguncian `IsSystemType` | **Tidak ada** | **Dapat dijalankan sekarang juga** — master jenis jurnal sudah terisi |
| `UAT-19` | Saldo awal tidak seimbang ditolak | Ya — syarat 1 | **Tidak ada** | Daftar akun dan periode kosong |

**Kabar baiknya: 18 dari 19 skenario sudah punya penegakan di source.** Yang hilang murni bukti
bahwa penegakan itu benar-benar bekerja saat dipakai orang. Dan **satu skenario, `UAT-18`, dapat
dijalankan hari ini juga** lewat Swagger tanpa persiapan apa pun, karena master jenis jurnal sudah
terisi empat baris.

### Pertanyaan 4 — Apakah 13 butir `OPEN` sudah lengkap?

Butirnya benar berjumlah **13**, dan seluruhnya saya periksa terhadap keadaan nyata. Hasilnya:
**sembilan akurat, empat kedaluwarsa sebagian, dan sembilan kekurangan lain sama sekali belum
tercatat.**

Sesuai permintaan Anda, butir yang merupakan keputusan sadar **tidak** saya laporkan ulang sebagai
temuan baru. Yang saya nilai hanyalah apakah risikonya sudah tercatat dengan benar.

#### Penilaian atas 13 butir `OPEN`

| ID | Risikonya tercatat benar? | Catatan pemeriksaan |
|---|:---:|---|
| `ACC-TD-001` | **Akurat** | Betul bahwa ini bukan cacat produksi. Namun *cara menutup*-nya menyebut `QuilvianSystemBackend.BillingTests`, yang **sudah tidak ada** — lihat `ACC-GAP-007` |
| `ACC-TD-002` | **Akurat, dan paling berat** | Diverifikasi ulang: nol klaim badan hukum di JWT, nol `HasQueryFilter`, `LegalEntityId` tetap dari `[FromQuery]`. Penjaga `IsDefault` terpasang di 31/31 method dan benar-benar menahan |
| `ACC-TD-003` | **Akurat** | Diverifikasi: registry backend **97 baris, nol baris `Acc`**; checker hidup di `tooling/qbe/`, membaca registry itu pada baris 200, dan menerbitkan `QBE-MOD-002 VIOLATION` pada baris 345; workflow berjalan `Strict` untuk PR ke `QuilvianIntegrationBackend` |
| `ACC-TD-005` | **Akurat** | `UAT-15` memang tidak dapat dijalankan, dan penegakan penggantinya nyata di source |
| `ACC-TD-006` | **Akurat** | `QBE-MIG-001` dan `QBE-MIG-002` masih `PROPOSED`, dan tidak mengikat task mana pun |
| `ACC-TD-007` | **Akurat isinya, kedaluwarsa lokasinya** | **Saya jalankan sendiri hari ini**: test itu **masih merah**, `Expected "FINAL", Actual "CLOSED"`. Tetap milik Billing. Namanya kini berada di project `UnitTests.InMemory`, bukan `QuilvianSystemBackend.Tests` |
| `ACC-TD-008` | **Kedaluwarsa** | Project `QuilvianSystemBackend.BillingTests` **sudah tidak ada**. Test bergerbang environment variable kini berjumlah **76**, bukan 52, dan berada di `QuilvianSystemBackend.IntegrationTests.Postgres`. Substansinya tetap benar — lihat `ACC-GAP-007` |
| `ACC-TD-009` | **Akurat** | Dua keputusan UI tetap terbuka, dan frontend nol berkas Accounting. Di luar ruang lingkup audit ini |
| `ACC-TD-010` | **Akurat** | Tidak diperiksa ulang ke database sesuai larangan Anda. Catatannya konsisten dengan `ACC-DEC-043` |
| `ACC-TD-012` | **Akurat** | Baris 361 `backend-roadmap.md` masih berbunyi persis seperti yang dicatat. Perbaikan teks, nol dampak kode |
| `ACC-TD-015` | **Akurat, dan terbukti persis** | Diverifikasi dua arah: backend **97 baris**, `Acc` **tidak ada**, `Lab` **`ACTIVE`**; suite skill **100 baris**, `Acc` **`ACTIVE`**, `Lab` **`PLANNED`**. Benar bahwa **tidak ada salinan yang merupakan superset** |
| `ACC-TD-016` | **Akurat, akibatnya lebih tajam** | Tabel *"Yang dihapus"* jujur menandai tiga berkas *"belum terlacak"*. Yang belum terbaca dari sana: **83 method test dapat dipulihkan hanya dengan satu perintah git**, sedangkan `JournalLifecycleTests.cs` harus ditulis ulang dari nol |
| `ACC-TD-018` | **Akurat** | `AccJournal` masih 0 baris, jadi syaratnya memang belum dapat dipenuhi. Alasan menolak index spekulatif juga benar |

#### Kekurangan yang belum tercatat sama sekali

Sembilan butir berikut tidak ada di `UTANG-TEKNIS.md` maupun di artefak lain. Saya beri ID usulan
agar mudah dirujuk; **penomoran resminya tetap wewenang Anda.**

| ID usulan | Kekurangan | Berat | Pemilik |
|---|---|:---:|---|
| `ACC-GAP-001` | **`roadmap/requirement-traceability.md` beku pada keadaan pra-implementasi.** Metadatanya masih `blueprint_revision: 4`, `decision_revision: 1.1`, dan kontrak `ACC-API-0.1`/`ACC-PERMISSION-0.1`. Seluruh barisnya berstatus `Planned`, dengan kalimat *"Belum ada satu pun yang berstatus selesai, karena belum ada implementasi"*. Ringkasan kesiapannya menulis **"Task yang sudah selesai: 0"**, padahal 14 dari 14 sudah `DONE`. Justru berkas inilah yang seharusnya menjawab *"aturan ini diwujudkan di mana, dan dibuktikan apa"* | **Tinggi** | Rizki |
| ~~`ACC-GAP-002`~~ | ~~Header `contract_version` masih `ACC-API-0.2`~~ — **`CLOSED` 4 September 2026** bersama `ACC-GAP-004`. Header kini `ACC-API-0.4`, cocok dengan manifest. Kalimat "seluruh endpoint berstatus Rencana (belum tersedia)" juga diperbaiki — ia benar pada `aa837d7`, keliru sejak `BE-ACC-014` | — | Rizki |
| `ACC-GAP-003` | **`blueprint-manifest.md` menulis `revision: 9`, sedangkan `MODULE-STATUS.md` menulis `Revision 10`.** Bagian *Riwayat verifikasi* pada manifest juga tidak memuat entri `9 → 10`, padahal kenaikan itu disebut menaikkan dua kontrak sekaligus | Sedang | Rizki |
| ~~`ACC-GAP-004`~~ | ~~Daftar DTO `ACC-API` sudah tidak cocok dengan source~~ — **`CLOSED` 4 September 2026.** Bagian *Daftar DTO* pada `contracts/api-contract.md` ditulis ulang dari source `822d48a`. Pemeriksaan penuh menemukan **24 selisih**, bukan lima: penamaan `Dto`→`Request`/`Response`/`Query`, ditambah selisih field pada kelima grup. `ACC-API` naik `0.3` → `0.4`, hash manifest digeser `9f05df37` → `b4a20208`. Nol baris source diubah | — | Rizki |
| `ACC-GAP-005` | **`GAP-ACC-003` tidak pernah diselesaikan.** Roadmap mengikatnya ke `BE-ACC-012` dengan kalimat tegas *"Keputusannya diambil saat task dikerjakan, bukan didiamkan"*. Laporan `BE-ACC-012` tidak menyebut `GAP-ACC-003` sama sekali, dan traceability masih mencatatnya terbuka. Yang ditanyakan: bagaimana pembatasan pencatatan `ACC-DEC-032` diuji | Sedang | Rizki |
| `ACC-GAP-006` | **Baseline verifikasi tertinggal dari HEAD.** Manifest dan `MODULE-STATUS` sama-sama menunjuk `f879944`, sedangkan HEAD `822d48a` berjarak empat commit. Dua di antaranya menyentuh Accounting: `b00e889` mengubah `JournalTypeDtos.cs` dan `AccJournalTypeService.cs`, lalu `afa91f0` menghapus lima berkas test. Ditambah `b091f0e` yang me-merge integration dan **merestrukturisasi seluruh project test**. Artinya **nol bukti verifikasi tercatat pada baseline yang sedang dinilai** | **Tinggi** | Rizki |
| `ACC-GAP-007` | **Empat butir utang menyebut project test yang sudah tidak ada.** `ACC-TD-001`, `007`, `008`, dan `016` merujuk `Tests/QuilvianSystemBackend.Tests` dan `Tests/QuilvianSystemBackend.BillingTests`. Sejak `BE-OPS-001A` masuk lewat merge, project test menjadi tiga: `UnitTests.InMemory` (1001 test), `UnitTests.Sqlite` (169 test), dan `IntegrationTests.Postgres` (76 test). **Sisi baiknya juga belum tercatat**: `IntegrationTests.Postgres/Infrastructure/` kini sudah tersedia, sehingga biaya menutup `ACC-TD-001` dan `ACC-TD-016` jauh lebih murah daripada yang tertulis di register | Sedang | Rizki |
| `ACC-GAP-008` | **Satu transisi status lebih longgar daripada kontrak.** `AccJournalService.SubmitAsync` menerima jurnal berstatus `Rejected` **langsung** menjadi `PendingApproval`. `ACC-STATE-0.1` bagian 1.1 hanya mendaftarkan `Rejected → Draft` lewat penyuntingan, lalu `Draft → PendingApproval`. Tidak merugikan, karena jurnal yang ditolak memang perlu diajukan ulang, tetapi ia perpindahan yang tidak terdaftar | Rendah | Rizki |
| `ACC-GAP-009` | **Angka suite pada `MODULE-STATUS.md` sudah usang.** Di sana tertulis *"suite tersisa 176 lulus, 0 gagal"*. Diukur hari ini pada `822d48a`, project `UnitTests.InMemory` saja sudah memuat **1001 test**. Angka 176 diukur sebelum merge integration | Rendah | Rizki |
| `ACC-GAP-010` | **`AccountingPeriodResponse` tidak memuat `AvailableActions`.** Acceptance (4) `FE-ACC-004` tidak dapat dipenuhi tanpanya. Rinciannya di laporan `fe-acc-004` bagian 3. Terkonfirmasi ulang saat ratifikasi `ACC-GAP-004` | Sedang | Rizki |
| `ACC-GAP-011` | **`JournalApprovalResponse` tidak memuat nama penyetuju.** Kontrak lama menyebut `ActionByName`; pencarian ke seluruh `Areas/Corporate/AccountingManagement/` mengembalikan **nol kemunculan**. Yang ada `ActionBy` bertipe `Guid`. Akibatnya layar rincian jurnal (`FE-ACC-007`) **tidak dapat menampilkan nama penyetuju** dari endpoint ini saja — ia perlu endpoint pengguna terpisah, atau backend menambahkan field namanya. Ditemukan saat ratifikasi `ACC-GAP-004` | Sedang | Rizki |

---

## 4. Blocker, pemilik, dan mitigasi

Diurutkan menurut dampak, bukan menurut kemudahannya.

### `BLK-ACC-01` — Invariant akuntansi tidak dijaga apa pun

| Hal | Keterangan |
|---|---|
| **Pemilik** | **Rizki** — penghapusannya keputusan owner, jadi pemulihannya juga |
| Bukti | Nol berkas test Accounting di `822d48a`. `JournalLifecycleTests.cs` tidak pernah masuk git dan tidak ada di disk |
| Kenapa berat | Yang tidak lagi terjaga seluruhnya invariant keuangan. Cirinya sama: **bila rusak, aplikasi tidak error.** Ia terus berjalan dan menghasilkan angka yang keliru, dan ketahuan saat audit keuangan, bukan saat deploy |
| Sudah tercatat | Ya, `ACC-TD-016`. Yang belum tercatat adalah asimetri biaya pemulihannya |
| **Mitigasi** | Dua langkah, sengaja dipisah karena biayanya jauh berbeda. **(a)** Pulihkan 83 method test `BE-ACC-001`..`009` dari `afa91f0^` — mekanis, tinggal dipindahkan ke project `UnitTests.Sqlite` yang sekarang memegang `TestDatabase`. **(b)** Tulis ulang test `BE-ACC-010`..`014` dari bagian 4 dan 6 tiap laporan task ke `IntegrationTests.Postgres`, yang memang menuntut PostgreSQL sungguhan seperti yang dibutuhkan `pg_advisory_xact_lock` |

### `BLK-ACC-02` — Modul belum pernah dijalankan sebagai proses bisnis

| Hal | Keterangan |
|---|---|
| **Pemilik** | **Rizki** — ini langkah operasional, bukan pekerjaan kode |
| Bukti | `MODULE-STATUS.md`: *"yang masih kosong adalah daftar akun dan periode, keduanya menunggu owner"*. `ACC-TD-018` menegaskan `AccJournal` masih 0 baris |
| Kenapa berat | Tanpa daftar akun dan periode, **tidak satu pun** dari 19 skenario UAT dapat dijalankan, dan tidak ada satu jurnal pun yang pernah melewati `submit` → `approve` → `post` di luar test |
| Sudah tercatat | Sebagian. `MODULE-STATUS` menyebutnya sebagai langkah berikutnya, tetapi tidak ada butir utang yang mencatat bahwa **inilah yang menahan seluruh UAT** |
| **Mitigasi** | Susun daftar akun awal lewat `POST /chart-of-accounts`. Ini kebijakan akuntansi rumah sakit dan memang **tidak boleh** ada seeder-nya. Lalu jalankan `POST /periods/generate` untuk tahun buku berjalan. Sesudah itu `UAT-01` dapat dijalankan untuk pertama kalinya |

### `BLK-ACC-03` — Gerbang QBE menolak merge ke integration

| Hal | Keterangan |
|---|---|
| **Pemilik** | **Lead / pemilik registry** — bukan owner modul |
| Bukti | Registry backend 97 baris tanpa `Acc`. Checker membacanya dan menerbitkan `QBE-MOD-002 VIOLATION` untuk setiap entity persisted baru yang kepemilikannya tidak dapat diresolusi |
| Sudah tercatat | Ya, `ACC-TD-003` dan `ACC-TD-015`, keduanya akurat |
| **Mitigasi** | Pemilik registry **menggabungkan kedua arah**, bukan menyalin satu arah. Menimpakan salinan suite ke backend akan mencabut `Lab` `ACTIVE` milik Muhammad Hamzah beserta changelog-nya. Berkas serah terimanya sudah siap di `evidence/03-acc-dep-007-governance-propagation.md` dan `evidence/07-acc-dep-007-ringkasan-untuk-lead.md` |
| **Jangan** | Menambahkan baris `Acc` ke salinan backend sendiri. Itu berarti meloloskan gerbang untuk PR sendiri, dan bukan wewenang owner modul |

### `BLK-ACC-04` — Penyaringan badan hukum per pengguna tidak ada

| Hal | Keterangan |
|---|---|
| **Pemilik** | **Security / Platform** |
| Bukti | Nol klaim badan hukum di JWT, nol `HasQueryFilter`, dan `LegalEntityId` selalu datang dari pengirim permintaan |
| Sudah tercatat | Ya, `ACC-TD-002`, dan penilaian risikonya tepat |
| **Mitigasi yang sudah berjalan** | Penjaga `IsDefault` terpasang di 31/31 method service dan terbukti menahan. MVP berjalan di atas satu badan hukum |
| Kapan menggigit | Saat badan hukum kedua didaftarkan, atau saat ada yang memindahkan tanda `IsDefault` |

### `BLK-ACC-05` — Traceability tidak lagi menjawab pertanyaannya sendiri

| Hal | Keterangan |
|---|---|
| **Pemilik** | **Rizki** |
| Bukti | `ACC-GAP-001` |
| Kenapa penting untuk kesiapan | Saat sign-off, pertanyaan pertama auditor adalah *"aturan ini diwujudkan di mana, dan dibuktikan apa"*. Berkas yang seharusnya menjawabnya masih menyatakan implementasi belum dimulai |
| **Mitigasi** | Perbarui metadata dan kolom statusnya, lalu tautkan tiap baris ke laporan task yang sudah ada. Pekerjaan dokumen, nol dampak kode |

---

## 5. Endpoint yang berdiri — gaya Swagger

Seluruh 31 endpoint di bawah ini **sudah ada** pada `822d48a`, dan hak aksesnya sudah terpasang.
Kolom terakhir menyatakan apakah pembacaannya dicatat logger, sesuai `ACC-DEC-032`.

### Grup `Corporate / Accounting / Master Data / Chart of Account`

Base URL `api/v1/corporate/accounting/master-data/chart-of-accounts`

| Method | Path | Hak akses | Dicatat logger |
|---|---|---|:---:|
| `GET` | `/` | `ChartOfAccount : Read` | Tidak |
| `GET` | `/{id}` | `ChartOfAccount : Read` | Tidak |
| `GET` | `/tree` | `ChartOfAccount : Read` | Tidak |
| `GET` | `/options` | `ChartOfAccount : Read` | Tidak |
| `POST` | `/` | `ChartOfAccount : Create` | Ya |
| `PUT` | `/{id}` | `ChartOfAccount : Update` | Ya |
| `PATCH` | `/{id}/deactivate` | `ChartOfAccount : Update` | Ya |
| `PATCH` | `/{id}/activate` | `ChartOfAccount : Update` | Ya |

### Grup `Corporate / Accounting / Master Data / Journal Type`

Base URL `api/v1/corporate/accounting/master-data/journal-types`

| Method | Path | Hak akses | Dicatat logger |
|---|---|---|:---:|
| `GET` | `/` | `JournalType : Read` | Tidak |
| `GET` | `/options` | `JournalType : Read` | Tidak |
| `POST` | `/` | `JournalType : Create` | Ya |
| `PUT` | `/{id}` | `JournalType : Update` | Ya |
| `POST` | `/seed` | `JournalType : Create` | Ya |

### Grup `Corporate / Accounting / Journal Management / Journal`

Base URL `api/v1/corporate/accounting/journals`

| Method | Path | Hak akses | Dicatat logger |
|---|---|---|:---:|
| `GET` | `/` | `Journal : Read` | Tidak |
| `GET` | `/{id}` | `Journal : Read` | Tidak |
| `POST` | `/` | `Journal : Create` | Ya |
| `PUT` | `/{id}` | `Journal : Update` | Ya |
| `DELETE` | `/{id}` | `Journal : Delete` | Ya |
| `POST` | `/{id}/submit` | `Journal : Submit` | Ya |
| `POST` | `/{id}/approve` | `Journal : Approve` | Ya |
| `POST` | `/{id}/reject` | `Journal : Approve` | Ya |
| `POST` | `/{id}/post` | `Journal : Post` | Ya |
| `POST` | `/{id}/reverse` | `Journal : Reverse` | Ya |

### Grup `Corporate / Accounting / Accounting Period`

Base URL `api/v1/corporate/accounting/periods`

| Method | Path | Hak akses | Dicatat logger |
|---|---|---|:---:|
| `GET` | `/` | `AccountingPeriod : Read` | Tidak |
| `GET` | `/current` | `AccountingPeriod : Read` | Tidak |
| `POST` | `/generate` | `AccountingPeriod : Create` | Ya |
| `POST` | `/{id}/close` | `AccountingPeriod : Close` | Ya |
| `POST` | `/{id}/reopen` | `AccountingPeriod : Reopen` | Ya |

### Grup `Corporate / Accounting / General Ledger`

Base URL `api/v1/corporate/accounting/general-ledger`

| Method | Path | Hak akses | Dicatat logger |
|---|---|---|:---:|
| `GET` | `/movements` | `GeneralLedger : Read` | Tidak |
| `GET` | `/trial-balance` | `GeneralLedger : Read` | **Ya** |
| `GET` | `/account-balance/{accountId}` | `GeneralLedger : Read` | Tidak |

`GET /trial-balance` adalah satu-satunya pembacaan yang dicatat, dan muatan log-nya sengaja tidak
memuat satu pun angka rupiah — sesuai larangan `ACC-PERMISSION-0.3` bagian 4. Diverifikasi di
`GeneralLedgerController.cs`.

---

## 6. Alur bisnis yang sudah berdiri dan yang belum pernah dijalani

Supaya jelas bagi pembaca non-teknis, inilah perjalanan satu jurnal dari awal sampai masuk buku
besar, beserta keadaannya hari ini.

| Langkah | Siapa | Apa yang terjadi | Berdiri di source? | Pernah dijalani sungguhan? |
|---:|---|---|:---:|:---:|
| 1 | Administrator | Mengisi master jenis jurnal — `JU`, `JP`, `JB`, `SA` | Ya | **Ya**, 3 Sep 2026, 4 baris masuk |
| 2 | Administrator | Menyusun daftar akun rumah sakit | Ya | **Belum** |
| 3 | Administrator | Membangkitkan 12 periode tahun buku | Ya | **Belum** |
| 4 | Staf akuntansi | Menyusun jurnal sebagai draft, boleh belum seimbang | Ya | Hanya di dalam test yang kini sudah terhapus |
| 5 | Staf akuntansi | Mengajukan jurnal — sembilan syarat diperiksa | Ya | Sama |
| 6 | Supervisor | Menyetujui atau menolak. **Tidak boleh menyetujui jurnal buatannya sendiri** | Ya | Sama |
| 7 | Manajer | Mengesahkan — sembilan syarat **diperiksa ulang** | Ya | Sama |
| 8 | Siapa pun yang berhak | Membaca buku besar dan neraca saldo. Hanya jurnal disahkan yang terhitung | Ya | Sama |
| 9 | Manajer | Membalik jurnal keliru, atau membuat jurnal penyesuaian | Ya | Sama |

**Langkah 2 dan 3 adalah pintu yang masih tertutup.** Selama keduanya belum dilakukan, langkah 4
sampai 9 tidak dapat disentuh pengguna sungguhan, walaupun kodenya sudah lengkap.

---

## 7. Bukti yang saya kumpulkan sendiri

Seluruhnya read-only terhadap source, dan **nol sentuhan database**.

| Pemeriksaan | Cara | Hasil |
|---|---|---|
| Keutuhan artefak | `sha256sum` 17 artefak dibandingkan `artifact_hashes` manifest | **17 dari 17 cocok** |
| Keadaan branch | `git status`, `git rev-parse` | `rizkiG`, `822d48a`, working tree **bersih**, dan tetap bersih sesudah audit |
| Kompilasi | `dotnet build ./QuilvianSystemBackend.sln` ke `BaseOutputPath` terpisah, direktorinya lalu dihapus | **0 error**, 23 warning |
| Suite yang berjalan | `dotnet test Tests/QuilvianSystemBackend.UnitTests.InMemory` | **Failed: 1, Passed: 1000, Total: 1001** |
| Identitas satu kegagalan | Keluaran test | `BillingFinalizationServiceTests.NormalFinalizationRequiresFullySettledOutstandingAndSetsInvoiceDate` — persis `ACC-TD-007`, milik Billing, pre-existing |
| Test Accounting tersisa | `find` dan `grep` pada `Tests/` | **Nol berkas** |
| Riwayat test | `git log --all -- '*JournalLifecycleTests*'` | **Kosong** — tidak pernah masuk git |
| Test yang dapat dipulihkan | `git ls-tree afa91f0^`, lalu menghitung `[Fact]`/`[Theory]` | **83 method** pada 5 berkas |
| Permukaan API | Enumerasi atribut `[Http*]` pada 5 controller | **31 endpoint**, cocok 31/31 dengan `ACC-API-0.3` |
| Hak akses | Enumerasi `[AccessPermission]` | **31 dari 31** endpoint bertanda, cocok dengan `ACC-PERMISSION-0.3` bagian 3 |
| Penegakan hak akses | `Filters/AccessPermissionFilter.cs` | Benar-benar menegakkan: `401` bila belum login, `403` bila tidak berhak, disertai log `Security/AccessDenied` |
| Penjaga badan hukum | Pemindaian tiap method publik pada kelima service | **31 dari 31** memanggil `AccountingLegalEntityGuard.PeriksaAsync` sebagai baris pertama |
| Registry QBE | Membandingkan salinan backend dan salinan suite skill | Backend **97 baris**, `Acc` tidak ada, `Lab` `ACTIVE`. Suite **100 baris**, `Acc` `ACTIVE`, `Lab` `PLANNED` |
| Checker QBE | `tooling/qbe/Invoke-QbeConformanceCheck.ps1` | Hidup; membaca registry backend; menerbitkan `QBE-MOD-002 VIOLATION` bila kepemilikan tidak dapat diresolusi |
| Frontend | `find` pada `QuilvianSystemFrontendDev@1a86d933` | **Nol berkas Accounting** — sesuai keterangan owner |

Catatan kejujuran. Saya juga mencoba menjalankan checker QBE atas rentang merge penuh
`8c45762..822d48a` untuk mendapatkan angka pelanggarannya secara langsung. Prosesnya **tidak
selesai** dalam batas waktu yang wajar, dan saya hentikan. Kesimpulan `BLK-ACC-03` karena itu
bersandar pada pembacaan kode checker dan isi registry, **bukan** pada eksekusi checker yang
selesai. Kesimpulannya tetap kuat karena mekanismenya lugas, tetapi bedanya perlu Anda ketahui.

---

## 8. Jalan menuju `READY`

Empat langkah. Dua mekanis, satu operasional, dan satu di luar wewenang Anda.

| # | Langkah | Pemilik | Sifat | Menutup |
|---:|---|---|---|---|
| 1 | **Pulihkan 83 method test** `BE-ACC-001`..`009` dari `afa91f0^` ke project `UnitTests.Sqlite` | Rizki | Mekanis — `git show`, lalu sesuaikan namespace | Dua pertiga `BLK-ACC-01` |
| 2 | **Susun daftar akun awal dan bangkitkan periode**, lalu jalankan `UAT-01` sekali dari ujung ke ujung | Rizki | Operasional | `BLK-ACC-02`, sekaligus membuka 18 UAT |
| 3 | **Tulis ulang test** `BE-ACC-010`..`014` dari bagian 4 dan 6 laporan task, ke `IntegrationTests.Postgres` | Rizki | Menulis kode test | Sepertiga sisa `BLK-ACC-01` |
| 4 | **Teruskan penggabungan registry dua arah ke lead** | Lead / pemilik registry | Governance | `BLK-ACC-03` |

Di luar keempatnya, sembilan butir `ACC-GAP-001` sampai `ACC-GAP-009` sebaiknya dimasukkan ke
`UTANG-TEKNIS.md`, supaya register itu tetap menjadi **satu-satunya tempat** yang menjawab *"apa
saja yang belum beres di Accounting"* — persis seperti yang dijanjikan berkas itu pada paragraf
pembukanya. Yang paling mendesak dari sembilan itu adalah `ACC-GAP-004` dan `ACC-GAP-001`, karena
keduanya akan menyesatkan orang yang menyusun frontend dari kontrak.

`BLK-ACC-04` **tidak** perlu tertutup untuk rilis pertama. Ia sudah dimitigasi penjaga `IsDefault`
yang terbukti terpasang menyeluruh, dan menjadi prasyarat hanya sebelum badan hukum kedua
didaftarkan.

---

## 9. Yang sengaja tidak saya kerjakan

| Hal | Alasan |
|---|---|
| Memperbaiki source | Dilarang. Audit ini read-only |
| Menyentuh database | Dilarang. Seluruh pernyataan tentang isi database diambil dari dokumen, bukan dari `SELECT` |
| Menambah baris `Acc` ke registry backend | Bukan wewenang owner modul, dan itu berarti meloloskan gerbang untuk PR sendiri |
| Menandai frontend sebagai blocker | Keputusan sadar owner, dinyatakan di awal permintaan |
| Melaporkan ulang butir `OPEN` sebagai temuan baru | Sesuai permintaan. Ketiga belas butir hanya saya nilai ketepatan pencatatannya |
| Commit atau push | Dilarang |
