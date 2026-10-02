# Laporan Perubahan Backend — `PLT-BE-004`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `PLT-BE-004` |
| Judul | Durabilitas dan antrean dibuktikan di PostgreSQL sungguhan |
| Slice | `PLT-SLICE-01` — gelombang `MVP-1` |
| Roadmap | `docs/module-blueprints/platform/roadmap/backend-roadmap.md` §5 |
| Trace | `AC-PLT-003`, `AC-PLT-004`, `AC-PLT-005`, `AC-PLT-012` · `DEC-PLT-003`, `DEC-PLT-008` · `INV-PLT-001`, `INV-PLT-003` · `CONF-PLT-002`, `NOTE-PLT-001` · **`RJ-BIL-DEC-019`** — opt-in database pengembangan personal |
| Contract version | `v1` — ✅ **`approved`** |
| Dependency | `PLT-BE-003` ✅ · PostgreSQL sungguhan ✅ — `QuilvianNewDevSukma`, dibuka `RJ-BIL-DEC-019` pada 10 September 2026 |
| Klasifikasi | `MEDIUM` — skor 6: berkas diperiksa lebih dari 20 (2), berkas diubah 6 (1), logika sedang (1), database memakai perilaku persistence yang sudah ada (1), keamanan berkaitan tetapi bukan inti — pengaman target database (1). Satu repository, nol kontrak API, nol UI |
| Task mode | `BACKEND` — dinyatakan eksplisit oleh pemilik pekerjaan pada 10 September 2026 |
| Target tulis | `NewQuilvianSystemBackend` — `Tests/QuilvianSystemBackend.IntegrationTests.Postgres/**`. Ditambah satu baris register keputusan `docs/module-blueprints/rawat-jalan/00-interview-decisions.md` atas pilihan eksplisit pemilik, serta laporan, roadmap, dan traceability Platform |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `f0d6855` saat ditulis 9 September 2026 · `843430b` percobaan kedua · **`c606baf`** percobaan ketiga 10 September 2026 — seluruhnya cabang `sukmagp` |
| Tanggal | `2026-09-09` ditulis · `2026-09-10` diperbarui dua kali |
| Status | ✅ **SELESAI 10 September 2026.** Keenam uji **lulus di PostgreSQL sungguhan** terhadap database pengembangan personal `QuilvianNewDevSukma`, dan keempat acceptance criteria terbukti. **Riwayat:** 🟡 sebagian pada 9 September 2026 karena tidak ada database test, lalu tetap 🟡 pada percobaan kedua 10 September 2026 karena role database ditolak `42501: permission denied to create database` |

---

## 1. Masalah yang diperbaiki

`PLT-BE-003` membangun alokator yang **mengaku** durabel: pencacahnya ditulis pada transaksi dan
koneksi tersendiri, sehingga nomor seharusnya hangus — bukan dipakai ulang — ketika pekerjaan
bisnis pemanggil dibatalkan.

**Klaim itu belum pernah dibuktikan.** Seluruh pengujian `PLT-BE-003` berjalan di SQLite, yang
tidak punya `pg_advisory_xact_lock` dan yang lingkungan ujinya berbagi satu koneksi. Dua sifat
yang justru paling menentukan modul ini karena itu belum diuji sama sekali.

**Kenapa itu berbahaya bila dibiarkan.** Sembilan task backend Bank Darah menunggu di belakang
alokator ini. Membangunnya di atas mesin yang durabilitasnya belum terbukti berarti, bila
`AC-PLT-003` ternyata gagal, perbaikannya menyentuh mesin yang sudah dipakai order darah.

### 1.1 Pembaruan 10 September 2026 — kenapa baru dapat dibuktikan sekarang

Uji-nya sudah ada sejak 9 September 2026, tetapi tidak ada database yang boleh dipakai:

| Jalan | Kenapa buntu |
| --- | --- |
| Database test baru `QuilvianNumberSeriesTest` | Server menolak `CREATE DATABASE` — role-nya tidak punya hak `CREATEDB` |
| Database pengembangan personal `QuilvianNewDevSukma` | Pengaman fixture menolak nama yang mengandung `dev`, dan `RJ-BIL-DEC-017` menyatakan database itu tidak akan pernah dapat menjadi sasaran integration test |

Pemilik pekerjaan memutuskan jalan kedua dan **menolak membuat database test baru**. Dasarnya:
setiap developer memakai database pengembangan personal masing-masing yang dapat di-restore, dan
perubahan baru disatukan ke database master lewat migration terpisah. Kesalahan pada test karena
itu hanya berdampak ke database milik pemiliknya sendiri.

Keputusan itu dicatat sebagai **`RJ-BIL-DEC-019`**, yang membalik bagian `RJ-BIL-DEC-017`, lalu
diwujudkan sebagai opt-in eksplisit pada fixture — bukan dengan menghapus pengamannya.

---

## 2. Proses bisnis

Task ini **tidak** menambah atau mengubah proses bisnis rumah sakit. Ia menghasilkan **bukti**
atas perilaku yang sudah dibangun `PLT-BE-003`, ditambah satu tata cara bagi developer.

### 2.1 Yang dibuktikan enam uji

| Uji | Yang dibuktikan | Contoh nyata dari hasil uji |
| --- | --- | --- |
| Pekerjaan pemanggil dibatalkan | Pencacah **tetap naik**; nomor hangus, tidak diterbitkan lagi | `DUR-00000001` terbit, pekerjaannya dibatalkan, lalu permintaan berikutnya mendapat `DUR-00000002` — bukan `DUR-00000001` lagi |
| Pekerjaan pemanggil berhasil | Nomor tetap berurutan — jalur normal tidak rusak oleh perbaikan durabilitas | `NRM-00000001` lalu `NRM-00000002` |
| Dua puluh alokasi bersamaan, deret sama | Dua puluh nomor **berbeda**, nol kegagalan | 20 permintaan serentak → 20 nomor unik, pencacah berhenti di 20 |
| Kunci deret A ditahan, alokasi deret B | Deret B **tetap selesai** — kunci per deret, bukan global | Deret A dikunci dan sengaja tidak dilepas; deret B tetap mendapat `KNB-00000001` |
| Deret Billing dibatalkan | Nomor **dipakai ulang** — perilaku lama **sengaja dipertahankan** | Dua kali alokasi invoice yang sama-sama dibatalkan menghasilkan nomor yang sama |
| Tabel Billing dan Platform | Terpisah; nol baris saling menyeberang | Deret Platform ada di `NumNumberSeries` dan tidak ada di `BilNumberSeries` |

### 2.2 Tata cara menjalankan uji terhadap database personal

| Unsur | Isi |
| --- | --- |
| Tujuan | Membuktikan perilaku yang hanya dapat diuji di PostgreSQL, tanpa membuat database test baru |
| Pelaku | Developer pemilik database pengembangan personal |
| Pemicu | Task yang acceptance criteria-nya menuntut PostgreSQL sungguhan |
| Prasyarat | Database pengembangan milik developer itu sendiri, yang boleh menerima migration tertunda dan baris uji |

Langkahnya:

1. Developer mengisi `QUILVIAN_BILLING_TEST_DB` dengan connection string database personalnya.
2. Developer mengisi `QUILVIAN_BILLING_TEST_DB_ALLOW_PERSONAL` dengan nama database yang **sama
   persis**, misalnya `QuilvianNewDevSukma`.
3. Fixture memeriksa target sebelum membuka koneksi apa pun. Urutannya: variable kosong, nilai tidak
   sah, nama database kosong, nama terlarang, kecocokan opt-in, penanda terlarang, lalu penanda
   `test`.
4. Bila lolos, fixture mencetak peringatan `[BILLING-TEST]` bahwa opt-in personal aktif, lalu
   menjalankan `Database.Migrate()`.
5. Uji berjalan dan menulis barisnya ke database itu.

Aturan yang berlaku:

- Opt-in hanya melepas **dua** pemeriksaan: penanda `dev` dan tuntutan penanda `test`.
- `QuilvianNewDevTim01` beserta penanda `prod`, `production`, `live`, `staging`, `stage`, `uat`, dan
  `shared` tetap ditolak mutlak.
- Tanpa opt-in, perilaku fixture **tidak berubah** bagi developer lain.

Jalur tidak normal — seluruhnya berhenti **sebelum** koneksi dibuka, dan seluruhnya dibuktikan pada
bagian 5.3:

| Contoh isian | Hasil | Arti bagi developer |
| --- | --- | --- |
| `QuilvianNewDevSukma` tanpa opt-in | Ditolak penanda `dev`, disertai petunjuk nama variable opt-in | Perilaku lama tetap berlaku |
| `QuilvianNewDevSukma` dengan opt-in `QuilvianNewDevsukma` — huruf `s` kecil | Ditolak, nilai opt-in tidak sama persis | Salah ketik berakhir sebagai penolakan, bukan salah sasaran |
| `QuilvianNewDevTim01` dengan opt-in `QuilvianNewDevTim01` | Ditolak, database bersama tim | Opt-in tidak dapat membuka database bersama |
| `QuilvianSharedDevSukma` dengan opt-in yang sama | Ditolak penanda `shared` | Penanda lain tidak mengenal override |
| `QuilvianProdDevSukma` dengan opt-in yang sama | Ditolak penanda `prod` | Sama |

Hasil akhirnya: uji berjalan hanya terhadap database yang disebut namanya persis oleh pemiliknya,
dan semua target lain tetap tertutup.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Kelompok | Berkas |
| --- | --- |
| Tata kelola | `AGENTS.md` · `rules/GLOBAL_RULES.md` · `rules/backend/TASK_RULES.md`, `TASK_CLASSIFICATION.md`, `DATABASE_RULES.md`, `REVIEW_RULES.md`, `REPORT_TEMPLATE.md` · `rules/backend/engineering/BACKEND_ENGINEERING_CONTRACT.md` bagian keberlakuan · `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris `Bil` dan `Num` · `rules/rule-output/status-task-roadmap.md`, `aturan-output-dokumentasi.md` |
| Keputusan pengaman fixture | `rawat-jalan/00-interview-decisions.md` — `RJ-BIL-DEC-009`, `RJ-BIL-DEC-017` · `rawat-jalan/execution-evidence-RJ-BIL-BE-002.md` · `rawat-jalan/execution-evidence-RJ-BIL-BE-003.md` |
| Kontrak modul | `platform/testing/acceptance-test-matrix.md` · `platform/roadmap/backend-roadmap.md` · `platform/roadmap/requirement-traceability.md` |
| Source | `Infrastructure/BillingTestDatabaseFixture.cs` · `Platform/NumberSeriesDurabilityTests.cs` · `BillingNumberSeriesService.cs` · README project uji · lima kelas uji pemakai fixture yang sama |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Tests/QuilvianSystemBackend.IntegrationTests.Postgres/Infrastructure/BillingTestDatabaseFixture.cs` | **Diubah 10 September 2026.** Konstanta `PersonalDatabaseOptInVariable` dan `PersonalOptInWaivableMarker`, method `ResolvePersonalDatabaseOptIn`, pelepasan penanda `dev` dan penanda `test` hanya saat opt-in cocok, petunjuk opt-in pada pesan penolakan `dev`, dan peringatan `[BILLING-TEST]` saat opt-in aktif. Komentar yang menyatakan "tidak disediakan override apa pun" diperbarui |
| `Tests/QuilvianSystemBackend.IntegrationTests.Postgres/README.md` | **Diubah 10 September 2026.** Bagian *Database yang dipakai* ditulis ulang. Isi lamanya **usang**: masih menyebut fallback ke `appsettings.Development.json` yang sudah dihapus sejak `RJ-BIL-BE-003`, dan masih menyebut `QuilvianNewDevTim01` boleh berjalan |
| `docs/module-blueprints/rawat-jalan/00-interview-decisions.md` | **Satu baris ditambah:** `RJ-BIL-DEC-019`. Baris `RJ-BIL-DEC-017` **tidak disunting**; ia tetap sebagai riwayat |
| `Tests/QuilvianSystemBackend.IntegrationTests.Postgres/Platform/NumberSeriesDurabilityTests.cs` | Ditulis 9 September 2026 dan sudah ter-commit pada `843430b`. **Tidak diubah** pada pembaruan ini |
| Laporan ini, `platform/roadmap/backend-roadmap.md`, `platform/roadmap/requirement-traceability.md` | Status dan bukti `PLT-BE-004` |

**Nol source aplikasi disentuh.** `NumberSeriesAllocator`, `NumNumberSeries`, `Program.cs`, dan
seluruh berkas Billing tidak berubah satu baris pun.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — nol endpoint |
| Database — schema | **Nol perubahan schema** dan nol migration baru |
| Database — eksekusi | Uji dijalankan terhadap `QuilvianNewDevSukma` atas wewenang eksplisit pemilik. `Database.Migrate()` ikut berjalan. Source memuat 137 migration dan database itu tercatat `137/137` diterapkan pada 10 September 2026, sehingga **nol migration tertunda diharapkan** — ini **tidak** diverifikasi dengan query tersendiri |
| Database — baris | Menurut isi uji, sekitar **lima baris** deret berawalan `PLT_UJI_` tertinggal permanen di `NumNumberSeries`, karena pencacah tidak boleh dihapus (`INV-PLT-001`). Alokasi Billing pada uji `AC-PLT-012` di-rollback, sehingga nol baris Billing tertinggal. Jumlah ini tidak dihitung dengan query |
| Keamanan | Pengaman target database dilonggarkan **hanya** untuk penanda `dev`, **hanya** lewat opt-in bernama persis. Perilaku bawaan tidak berubah, dan kelima skenario penolakan terbukti pada bagian 5.3 |
| Kredensial | Connection string dibaca di dalam proses dari key `ConnectionStrings:DefaultConnection`. Nilainya tidak dicetak dan tidak disimpan ke berkas maupun environment user atau machine, lalu variable-nya dikosongkan setelah uji selesai |

### 3.4 Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module task | `Platform` / `NumberSeriesManagement` — prefix `Num`, status ✅ **`ACTIVE`** |
| Pemilik berkas yang disentuh | Infrastruktur uji bersama `QuilvianSystemBackend.BillingTests.Infrastructure`, asal `BillingManagement / Billing` — prefix `Bil`, status ✅ **`ACTIVE`**. Keputusan pengamannya milik blueprint `RJ-BIL-BP-001`, yang pemiliknya adalah pemilik pekerjaan ini sendiri |
| Keberlakuan | **`TOUCHED LEGACY`** — berkas uji yang sudah ada disentuh; bukan kode aplikasi |
| QBE ID yang berlaku | **Nol QBE aplikasi berlaku.** Task ini tidak membuat entity, configuration, controller, service, maupun alokasi nomor. `QBE-MOD-001` tidak berlaku karena tidak ada capability aplikasi yang ditempatkan |
| Branch | `sukmagp` ↔ `origin/sukmagp`, sejajar `0/0` saat mulai; working tree bersih |

### 3.5 Delta kontrak dan keputusan

| Delta | Isi | Alasan |
| --- | --- | --- |
| Kontrak Platform | **Nol delta.** `acceptance-test-matrix.md` hanya menuntut uji lulus *"di PostgreSQL sungguhan — bukan InMemory"* | Tuntutan "database test tersendiri" berasal dari pengaman fixture Billing, bukan dari kontrak Platform |
| Keputusan | **`RJ-BIL-DEC-019`** membalik bagian `RJ-BIL-DEC-017` | Diputuskan pemilik pekerjaan pada 10 September 2026 |
| Tambahan uji | Dua uji di luar daftar `AC`: jalur normal tetap berurutan, dan tabel Billing/Platform terpisah | Keduanya penjaga regresi, bukan aturan bisnis baru |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak membuat maupun menyentuh endpoint.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| **9 Sep 2026** — `dotnet build QuilvianSystemBackend.sln --no-incremental` | `0 Error(s)`, `210 Warning(s)` | `PASS` | Sama persis baseline |
| **9 Sep 2026** — keempat `AC` | Tidak dijalankan | `NOT RUN` | Fixture berhenti fail-closed — bagian 5.1 |
| **10 Sep 2026, percobaan kedua** — `dotnet test` filter `~Platform` (Postgres) | 0 lulus, 6 gagal dalam 13 ms | `NOT RUN` | `42501: permission denied to create database` — bagian 5.2 |
| **10 Sep 2026, percobaan ketiga** — sebelum fixture diubah, target `QuilvianNewDevSukma` | 0 lulus, 6 gagal dalam 4 ms | `NOT RUN` | Ditolak penanda `dev`; nol koneksi dibuka |
| **Percobaan ketiga** — `dotnet build QuilvianSystemBackend.sln --no-incremental` | `0 Error(s)`, **`210 Warning(s)`** | `PASS` | Sama persis baseline — nol peringatan baru |
| **Percobaan ketiga** — `dotnet build` project integrasi Postgres | `0 Error(s)` | `PASS` | Peringatannya `CS8603` pada `Laboratory/LaboratoryAuthorityTests.cs:263` dan `MSB3277` konflik versi `Microsoft.Extensions.DependencyModel` — **`EXISTING / ENVIRONMENT ISSUE`**, nol dari `BillingTestDatabaseFixture.cs` |
| **Percobaan ketiga** — `dotnet test` `UnitTests.Sqlite` filter `~Platform` | **54 lulus, 0 gagal** | `PASS` | Pembanding bahwa ✅ lain masih sahih |
| **Percobaan ketiga** — lima skenario penolakan pengaman | Kelimanya ditolak dalam 4–5 ms | `PASS` | Bagian 5.3 |
| **`AC-PLT-003`** durabilitas | Lulus, 243 ms | `PASS` | `PekerjaanPemanggilDibatalkan_PencacahTetapNaik_NomorHangus` |
| **`AC-PLT-004`** antrean deret sama | Lulus, 902 ms | `PASS` | `DuaPuluhAlokasiBersamaan_DeretSama_MenghasilkanNomorBerbedaSemua` |
| **`AC-PLT-005`** deret berbeda tidak menunggu | Lulus, 65 ms | `PASS` | `DeretBerbeda_TidakSalingMenunggu` |
| **`AC-PLT-012`** deret Billing tidak berubah | Lulus, 83 ms | `PASS` | `DeretBilling_TetapMemakaiUlangNomorSaatTransaksiDibatalkan` |
| Penjaga regresi | Keduanya lulus — 118 ms dan 1 s | `PASS` | `PekerjaanPemanggilBerhasil_NomorTetapBerurutan`, `TabelBillingDanPlatform_Terpisah` |

Uji manual: `NOT APPLICABLE` — nol endpoint dan nol layar.

**Tidak dijalankan:**

- Uji Postgres milik modul lain yang memakai fixture yang sama — Billing, Laboratory, Radiology, dan
  ClinicalIntegration — terhadap `QuilvianNewDevSukma`. Wewenang eksekusi hanya diberikan untuk
  keenam uji task ini.
- Query verifikasi jumlah migration dan jumlah baris di `QuilvianNewDevSukma`. Query itu tidak
  termasuk wewenang yang diberikan.

### 5.1 Riwayat 9 September 2026 — kenapa `NOT RUN`, bukan `FAIL`

Percobaan menjalankan uji ini berakhir dengan **6 kegagalan yang seluruhnya berupa
configuration error**, bukan kegagalan domain:

```text
Failed!  - Failed: 6, Passed: 0, Total: 6, Duration: 4 ms

System.InvalidOperationException : BLOCKED_BY_TEST_DB_CONFIGURATION:
environment variable QUILVIAN_BILLING_TEST_DB belum diisi.
```

**Nol perintah dikirim ke database mana pun** — durasinya 4 milidetik, dan fixture berhenti
sebelum membuka koneksi. Itu perilaku yang **benar** dan disengaja: `BillingTestDatabaseFixture`
bersifat *fail-closed* sejak temuan `RJ-BIL-BE-002`, ketika fallback ke
`appsettings.Development.json` membuat `dotnet test` menerapkan migration ke database dev bersama
`QuilvianNewDevTim01` tanpa ada yang memerintahkannya.

Keadaan lingkungan yang diperiksa pada 9 September 2026 — sebagian di antaranya **dikoreksi**
10 September 2026 pada bagian 5.2:

| Pemeriksaan | Hasil |
| --- | --- |
| `QUILVIAN_BILLING_TEST_DB` (process / user / machine) | **Kosong pada ketiganya** |
| Service bernama `*postgres*` | **Nol** |
| Port `5432` LISTEN | **Tidak** |
| `docker` | **Tidak terpasang** |

### 5.2 Riwayat percobaan kedua 10 September 2026 — blocker dipersempit menjadi satu hak akses

Percobaan kedua dijalankan pada `843430b` atas wewenang eksplisit pemilik pekerjaan, dengan target
database yang disebut namanya: `QuilvianNumberSeriesTest`.

**Tiga catatan 9 September 2026 dikoreksi:**

| Yang dicatat 9 September 2026 | Keadaan sebenarnya 10 September 2026 |
| --- | --- |
| "Nol service `*postgres*`, port `5432` tidak LISTEN" | **Benar untuk mesin lokal, tetapi menyesatkan.** Server PostgreSQL yang dipakai project ini **remote** dan **terjangkau** pada port `5432` |
| "Tidak ada PostgreSQL yang dapat dipakai di lingkungan ini" | **Tidak akurat.** Servernya ada; yang belum ada adalah **database test tersendiri** beserta **hak membuatnya** |
| Blocker = "sediakan PostgreSQL" | Blocker sebenarnya = **satu grant privilege** |

Ketiga penjagaan nama fixture lolos, lalu **server** yang menolak:

```text
Failed!  - Failed: 6, Passed: 0, Total: 6, Duration: 13 ms

Npgsql.PostgresException : 42501: permission denied to create database
   at Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.NpgsqlDatabaseCreator.Create()
   at Microsoft.EntityFrameworkCore.Migrations.Internal.Migrator.Migrate(String targetMigration)
```

**Nol efek samping.** Penolakan terjadi pada perintah `CREATE DATABASE`, sebelum satu baris pun
ditulis: nol database dibuat, nol schema berubah, dan nol perintah tulis dikirim ke
`QuilvianNewDevTim01`.

### 5.3 Percobaan ketiga 10 September 2026 — terbukti di `QuilvianNewDevSukma`

**Pemicunya:** pemilik pekerjaan menolak membuat database test baru dan memutuskan
`RJ-BIL-DEC-019`. Percobaan ini dijalankan pada `c606baf` ditambah perubahan fixture di atas.

**Langkah pertama — bukti bahwa perubahan fixture memang diperlukan.** Sebelum fixture diubah,
target `QuilvianNewDevSukma` ditolak:

```text
Failed!  - Failed: 6, Passed: 0, Total: 6, Duration: 4 ms
BLOCKED_BY_TEST_DB_CONFIGURATION: nama database 'QuilvianNewDevSukma' mengandung penanda 'dev', ...
```

**Langkah kedua — pengaman dibuktikan tetap menolak.** Seluruh skenario berikut berhenti sebelum
koneksi dibuka. Tiga skenario terakhir memakai connection string palsu ke `localhost`, sehingga
tidak ada database sungguhan yang tersentuh walau pengamannya keliru:

| Skenario | Durasi | Pesan penolakan |
| --- | ---: | --- |
| 4A — `QuilvianNewDevSukma` tanpa opt-in | 5 ms | Penanda `dev`, disertai petunjuk `QUILVIAN_BILLING_TEST_DB_ALLOW_PERSONAL` (`RJ-BIL-DEC-019`) |
| 4B — opt-in `QuilvianNewDevsukma`, beda satu huruf | 4 ms | Nilai opt-in tidak sama persis dengan database `QuilvianNewDevSukma` |
| 4C — `QuilvianNewDevTim01` dengan opt-in yang sama | 4 ms | Termasuk daftar terlarang, database bersama tim |
| 4D — `QuilvianSharedDevSukma` dengan opt-in yang sama | 4 ms | Penanda `shared` |
| 4E — `QuilvianProdDevSukma` dengan opt-in yang sama | 4 ms | Penanda `prod` |

**Langkah ketiga — uji sungguhan.** Dengan opt-in `QuilvianNewDevSukma` yang cocok, fixture
mencetak peringatan bahwa opt-in personal aktif, lalu keenam uji berjalan:

```text
Passed  TabelBillingDanPlatform_Terpisah                                    [1 s]
Passed  PekerjaanPemanggilDibatalkan_PencacahTetapNaik_NomorHangus          [243 ms]
Passed  PekerjaanPemanggilBerhasil_NomorTetapBerurutan                      [118 ms]
Passed  DuaPuluhAlokasiBersamaan_DeretSama_MenghasilkanNomorBerbedaSemua    [902 ms]
Passed  DeretBilling_TetapMemakaiUlangNomorSaatTransaksiDibatalkan          [83 ms]
Passed  DeretBerbeda_TidakSalingMenunggu                                    [65 ms]
Total tests: 6
```

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-PLT-003` pencacah tetap naik setelah pemanggil batal | **Terpenuhi** 10 September 2026 | Uji `PekerjaanPemanggilDibatalkan_PencacahTetapNaik_NomorHangus` lulus di PostgreSQL. Nomor kedua `DUR-00000002`, dan pencacah tersimpan bernilai 2 |
| `AC-PLT-004` dua puluh alokasi bersamaan, nomor berbeda semua | **Terpenuhi** 10 September 2026 | `DuaPuluhAlokasiBersamaan_DeretSama_MenghasilkanNomorBerbedaSemua` — 20 nomor unik, pencacah 20 |
| `AC-PLT-005` deret berbeda tidak saling menunggu | **Terpenuhi** 10 September 2026 | `DeretBerbeda_TidakSalingMenunggu` selesai dalam 65 ms, jauh di bawah batas 15 detik, sementara kunci deret A ditahan |
| `AC-PLT-012` empat deret Billing tidak berubah perilakunya | **Terpenuhi** 10 September 2026 | `DeretBilling_TetapMemakaiUlangNomorSaatTransaksiDibatalkan` lulus. **Batas yang jujur:** yang dipanggil langsung hanya jalur invoice. Tiga deret lainnya — deposit, kwitansi, dan shift kasir — memanggil method privat yang sama, `AllocateNumberAsync` pada `BillingNumberSeriesService.cs:141` (dipanggil dari baris 80, 95, 115, dan 130), dan berkas itu tidak berubah sejak `058e070` tanggal 28 Agustus 2026, sebelum slice ini dimulai |

**Keempat acceptance criteria terpenuhi.** Task ini karena itu naik ke ✅.

### Definition of Done

| Butir DoD | Status |
| --- | --- |
| `AC-PLT-003` dan `AC-PLT-004` lulus **di PostgreSQL**, bukan InMemory | **Terpenuhi** — `QuilvianNewDevSukma`, 10 September 2026 |
| Nol uji inti dinyatakan lulus berdasarkan provider tanpa transaksi | **Terpenuhi** — berkas uji ini terpisah dari uji SQLite |
| Empat deret Billing terbukti tidak berubah perilakunya | **Terpenuhi**, dengan batas pembuktian pada baris `AC-PLT-012` di atas |
| Angka hasil uji dicatat apa adanya, termasuk yang `NOT RUN` | **Terpenuhi** — bagian 5, termasuk ketiga riwayat `NOT RUN` |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build solution `210 Warning(s)`, sama persis baseline. Project integrasi memuat `CS8603` dan `MSB3277` yang sudah ada sebelumnya, nol dari berkas task ini |
| Masalah yang diketahui | **(1)** Bagian *Susunan folder* pada README project uji masih hanya menyebut `Operational/`, padahal kini ada `Platform/`, `Laboratory/`, `Radiology/`, dan `ClinicalIntegration/`. Di luar scope, tidak diperbaiki. **(2)** Roadmap Bank Darah bagian 6.1 masih mencatat `PLT-BE-004` 🟡 — itu artefak blueprint lain dan tidak disentuh task ini. **(3)** Roadmap Platform bagian 0 dan kartu `PLT-BE-005` masih menyatakan tabel `NumNumberSeries` belum ada di lingkungan mana pun. Uji ini membuktikan tabel itu **ada** di `QuilvianNewDevSukma`; baris traceability-nya sudah diperbarui, sedangkan kartu `PLT-BE-005` bukan milik task ini |
| Risiko tersisa | **(1)** Fixture tidak dapat membuktikan bahwa database yang disebut di opt-in memang milik si pemanggil. Nama persis menutup salah ketik, bukan penyalahgunaan sengaja. **(2)** Durabilitas terbukti pada satu database di satu server; lingkungan lain belum diuji. **(3)** Sekitar lima baris `PLT_UJI_*` tertinggal permanen di `NumNumberSeries` pada `QuilvianNewDevSukma` dan akan tampil di layar pemantauan `PLT-BE-005` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat blok di bawah |
| Langkah berikutnya | **(1)** `Andry`, pemilik gerbang `G4` Bank Darah, menyatakan gerbang itu tertutup; lalu roadmap Bank Darah diperbarui lewat pemeliharaan blueprint. **(2)** `BE-BD-003` dijadwalkan lewat `build-module-backend`. **(3)** Commit perubahan ini bila diminta — wewenang terpisah |

```text
 M Tests/QuilvianSystemBackend.IntegrationTests.Postgres/Infrastructure/BillingTestDatabaseFixture.cs
 M Tests/QuilvianSystemBackend.IntegrationTests.Postgres/README.md
 M docs/module-blueprints/platform/roadmap/backend-roadmap.md
 M docs/module-blueprints/platform/roadmap/requirement-traceability.md
 M docs/module-blueprints/platform/task/report/backend/PLT-BE-004.md
 M docs/module-blueprints/rawat-jalan/00-interview-decisions.md
```

Nol operasi `stage`, `commit`, `push`, `pull`, `merge`, `rebase`, `stash`, maupun `deploy`
dijalankan. `HEAD` tetap `c606baf`. Perintah database yang dijalankan hanyalah keenam uji terhadap
`QuilvianNewDevSukma`, termasuk `Database.Migrate()` dari fixture, atas wewenang eksplisit pemilik
pekerjaan.
