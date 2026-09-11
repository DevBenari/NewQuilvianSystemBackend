# Laporan Perubahan Backend — `BE-RWI-041`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-041` |
| Judul | Kunjungan dokter punya tempat menyimpan |
| Slice | `DOK-MVP-1` — fondasi konteks, kolom, tabel visite, pelonggaran |
| Roadmap | `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/backend-roadmap.md`, task `BE-RWI-041` |
| Trace | `RWI-DEC-084`, `RWI-DEC-085`; `CON-EXT-015`; `02-backend-architecture.md` §4.6; `data/data-dictionary.md` §6; `INV-DOK-06`, `INV-DOK-07`, `INV-DOK-08` |
| Contract version | `0.3.0`, `APPROVED` Muhammad Hamzah 3 September 2026 |
| Dependency | `BE-RWI-040` — 🟡 sebagian, lihat [laporan](BE-RWI-040.md) |
| Klasifikasi | `HEAVY`, skor 9: repository 0, berkas diperiksa 1, berkas diubah 2, logika bisnis 1, kontrak API 0, database 2, keamanan/auth 1, UI/workflow 0, ditambah satu tingkat karena melahirkan tabel baru beserta alokator nomor bisnisnya |
| Task mode | `BACKEND` |
| Target tulis | Repository `NewQuilvianSystemBackend`; source `ClinicalManagement`, `Repositories/`, `Program.cs`, `Migrations/`, project uji, dokumen tracked sub-modul |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `c8e83854af240186b5091da412fadde3810afcb1` pada branch `MHamzah` |
| Tanggal | 3 September 2026 |
| Status | ✅ **Selesai, 8 September 2026.** Keenam acceptance criteria terbukti, dan butir Definition of Done terakhir ditutup: kedua test `PhysicianVisitUniquenessTests` **hijau** terhadap PostgreSQL 15.15 sungguhan. Database uji tersendiri disediakan sebagai container sekali pakai, sehingga penjagaan fixture dipenuhi apa adanya dan **tidak** dilemahkan. Menutupnya menyingkap satu cacat pada uji itu sendiri, yang ikut diperbaiki. Rinciannya pada bagian 9 |

## Backend Governance Preflight

| Pemeriksaan | Hasil |
| --- | --- |
| Area / Module | `HealthServices` / `ClinicalManagement` |
| Pemilik / prefix registry | `ClinicalManagement / Cli`, `ACTIVE`. Entri registry sudah ada **sebelum** berkas model pertama dibuat, sesuai `QBE-MOD-002` dan `QBE-MOD-003` |
| Applicability | `NEW CODE` — tabel, entity, configuration, dan service ini seluruhnya baru |
| QBE berlaku | `QBE-ENT-001`, `QBE-ENT-002`, `QBE-NAM-001`, `QBE-NAM-002`, `QBE-CFG-001`, `QBE-MOD-001`, `QBE-MOD-002`, `QBE-SVC-001`, `QBE-CODE-002`, `QBE-CODE-003`, `QBE-CODE-004`, `QBE-CODE-005`, `QBE-DEL-001` |
| Penamaan | `CliPhysicianVisit` — `<PrefixPemilikDisetujui><KonsepBisnis>`. Entity, berkas, configuration, `DbSet`, dan nama tabel satu paket: `CliPhysicianVisit` / `CliPhysicianVisit.cs` / `CliPhysicianVisitConfiguration` / `CliPhysicianVisits` / `public."CliPhysicianVisit"` |
| Archetype | Transaksi, aggregate ber-lifecycle dengan dua keadaan. Permukaan API-nya belum dibuat; itu pekerjaan `BE-RWI-048` dan `BE-RWI-049` |
| Database authority | Pembuatan migration `PROVIDED` oleh acceptance criteria task. **Eksekusi migration tidak diberikan dan tidak dilakukan** |
| Frontend | Tidak disentuh |

---

## 1. Masalah yang diperbaiki

Sampai sekarang sistem tidak punya tempat untuk mencatat bahwa seorang dokter **benar-benar
mendatangi** pasiennya. Yang ada hanya catatan yang ia tulis.

Menghitung kunjungan dari catatan terdengar praktis, tetapi salah pada dua arah sekaligus:

| Keadaan nyata | Yang terbaca bila dihitung dari catatan |
| --- | --- |
| Dokter datang pukul 07.40, memeriksa pasien, tetapi belum sempat menulis apa pun | **Nol kunjungan.** Padahal ia benar-benar datang |
| Dokter datang sekali, lalu menulis tiga catatan susulan pada hari yang sama | **Tiga kunjungan.** Padahal ia datang sekali |

Kekeliruan ini bukan sekadar angka di layar. Kunjungan dokter adalah dasar penagihan jasa visite,
dan menjadi bukti bahwa pasien memang dipantau dokter setiap hari.

---

## 2. Proses bisnis

**Tujuan.** Sistem memiliki tempat mencatat kunjungan dokter sebagai **kejadian tersendiri**,
terpisah dari catatan apa pun yang ia tulis.

**Pelaku.** Dokter yang mendatangi pasien, atau petugas yang mencatatnya.

**Pemicu.** Dokter selesai mendatangi pasien.

**Langkah yang berurutan.**

1. Permintaan pencatatan membawa kunjungan, perawatan, pasien, dokter, waktu kedatangan, peran
   dokter, dan **kunci permintaan**.
2. Kunci permintaan diperiksa lebih dulu. Bila kunci yang sama sudah pernah dipakai, kejadian yang
   sudah ada dikembalikan apa adanya — bukan kejadian kedua.
3. Bila kuncinya baru, nomor bisnis dialokasikan service, lalu kejadiannya disimpan berstatus
   tercatat.
4. Kejadian yang salah catat dibatalkan **beserta alasannya**. Kejadian itu tetap tersimpan dan
   tetap tampil pada riwayat.
5. Pencatatan ulang setelah pembatalan menunjuk kejadian yang digantikannya.

**Aturan yang berlaku.**

- **Waktu yang disimpan adalah waktu kedatangan, bukan waktu pencatatan.** Visite pukul 07.40 yang
  baru dicatat pukul 07.52 tetap terbaca pada pukul 07.40. Waktu pencatatannya sendiri sudah
  tersimpan pada kolom audit.
- **Dua visite nyata pada tanggal yang sama menghasilkan dua baris.** Dokter yang benar-benar
  datang dua kali memang datang dua kali. Contoh: DPJP memeriksa pukul 07.30, lalu dipanggil lagi
  pukul 16.00 karena demam pasien naik. Hitungan hari itu **dua**, bukan satu.
- **Kunci permintaan wajib terisi dan dijaga unique penuh.** Unique-nya tidak parsial: kunci milik
  kejadian yang **sudah dibatalkan pun tidak boleh dipakai ulang**. Bila boleh, sebuah kiriman
  ulang lama dapat menghidupkan kembali kejadian yang sengaja dibatalkan.
- **Ketiga tautan dokumen bersifat opsional.** Satu kejadian tidak wajib punya catatan, dan satu
  catatan tidak wajib punya kejadian. Perilaku hapusnya mengosongkan tautan, bukan menghapus
  dokumennya.
- **Nomor bisnis tidak dibentuk dari hitungan baris.** Dua dokter yang menekan Simpan pada saat
  hampir bersamaan akan membaca angka yang sama bila memakai cara hitung, lalu menghasilkan nomor
  kembar.

**Status yang dihasilkan.** `Recorded` atau `Cancelled`. Tepat dua, dan itu disengaja: kejadian
visite tidak punya alur persetujuan.

**Jalur tidak normal.**

| Keadaan | Hasilnya |
| --- | --- |
| Kunci permintaan kosong | Ditolak `400`, "Kunci permintaan wajib diisi." |
| Pembatalan tanpa alasan | Ditolak `400`, "Alasan pembatalan wajib diisi." |
| Membatalkan kejadian yang sudah batal | Ditolak `409` |
| Kejadian tidak ditemukan | Ditolak `404` |

**Hasil akhirnya.** Kunjungan dokter tercatat sebagai fakta tersendiri, dan hitungannya diturunkan
dari kejadian — bukan dari catatan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `Areas/HealthServices/ClinicalManagement/Models/CliClinicalMilestoneFact.cs` dan
  configuration-nya — pola entity berprefix pemilik
- `Areas/HealthServices/InPatientManagement/Services/InpEpisodeNumberService.cs` dan
  `Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyDocumentNumberService.cs`
  — pola alokasi nomor bisnis milik modul sendiri
- `Areas/HealthServices/BillingManagement/Billing/Services/BillingNumberSeriesService.cs` —
  penyedia seri nomor milik Billing, ditelusuri lalu **tidak dipakai**; alasannya di bagian 6
- `data/data-dictionary.md` §6 dan §11, `02-backend-architecture.md` §4.6
- `rules/backend/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Models/CliPhysicianVisit.cs` | **Baru.** Entity kejadian visite, 20 kolom bisnis di luar kolom audit |
| `Areas/HealthServices/ClinicalManagement/Enums/PhysicianVisitRole.cs` | **Baru.** `Dpjp`, `Consultant`, `OnCall` |
| `Areas/HealthServices/ClinicalManagement/Enums/PhysicianVisitStatus.cs` | **Baru.** `Recorded`, `Cancelled` |
| `Repositories/Configurations/HealthServices/ClinicalManagement/CliPhysicianVisitConfiguration.cs` | **Baru.** Bentuk kolom, sepuluh foreign key, dua unique, empat index |
| `Repositories/ApplicationDbContext.cs` | `DbSet<CliPhysicianVisit> CliPhysicianVisits` |
| `Areas/HealthServices/ClinicalManagement/Services/PhysicianVisitNumberService.cs` | **Baru.** Alokator nomor bisnis milik modul |
| `Areas/HealthServices/ClinicalManagement/Services/PhysicianVisitService.cs` | **Baru.** Pemilik CRUD dan orkestrasi kejadian visite |
| `Repositories/Configurations/HealthServices/TrxDoctorConsultationConfiguration.cs` | Foreign key opsional dari catatan dokter ke kejadian visite |
| `Repositories/Configurations/HealthServices/TrxPatientProcedureConfiguration.cs` | Foreign key opsional dari tindakan ke kejadian visite |
| `Program.cs` | Pendaftaran kedua service pada dependency injection |
| `Migrations/20260903093510_AddCliPhysicianVisit.cs` | **Baru.** Migration tabel beserta index dan foreign key-nya |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/ClinicalManagement/InpatientClinicalSchemaTests.cs` | Uji bentuk tabel, alokasi nomor, idempotency, dua visite sehari, dan pembatalan |
| `Tests/QuilvianSystemBackend.IntegrationTests.Postgres/ClinicalIntegration/PhysicianVisitUniquenessTests.cs` | **Baru.** Dua uji yang hanya dapat dibuktikan PostgreSQL sungguhan |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE`. Belum ada endpoint. Controller, DTO, dan butir hak aksesnya adalah pekerjaan `BE-RWI-048` dan `BE-RWI-049` |
| Database | **Satu tabel baru, `public."CliPhysicianVisit"`**, beserta dua unique index — kunci permintaan dan nomor bisnis — empat index biasa, dan sepuluh foreign key. Ditambah dua foreign key opsional dari catatan dokter dan tindakan ke tabel ini. Satu migration: `20260903093510_AddCliPhysicianVisit`. **Belum diterapkan ke database mana pun** |
| Keamanan/Auth | `NOT APPLICABLE` pada task ini. Service tidak membaca peran, jabatan, maupun jenis pengguna. Butir hak akses lahir bersama endpoint-nya pada `BE-RWI-048` |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE`. Task ini sengaja tidak membuat endpoint; scope-nya berhenti pada tabel,
configuration, `DbSet`, enum, service, dan alokator nomor.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj` | Berhasil, `0 Error(s)` | `PASS` | Keluaran perintah |
| Nama tabel, schema, dan awalan entity | `CliPhysicianVisit` pada schema `public`; berawalan `Cli`, bukan `Trx` | `PASS` | `InpatientClinicalSchemaTests.TabelVisite_BentuknyaSesuaiKamusData` |
| Kunci permintaan wajib terisi dan unique **penuh** | Kolom tidak nullable; index unique tanpa penyaring | `PASS` | Uji yang sama |
| Index perawatan-waktu dan dokter-waktu | Keduanya ada | `PASS` | Uji yang sama |
| Tidak ada unique atas pasangan perawatan, dokter, dan tanggal | Tidak ditemukan satu pun | `PASS` | Uji yang sama |
| Nomor bisnis tidak dibentuk dari hitungan baris | 200 nomor dibentuk pada detik yang sama persis, seluruhnya berbeda, seluruhnya ≤ 30 karakter | `PASS` | `…NomorVisite_TidakDibentukDariHitunganBaris` |
| Kiriman ulang berkunci sama | Satu kejadian; yang kedua mengembalikan identitas yang sama dengan kode `200` | `PASS` | `…VisiteBerkunciSama_TidakMelahirkanKejadianKedua` |
| Dua visite pada tanggal yang sama | Dua baris, hitungan `2` | `PASS` | `…DuaVisitePadaTanggalSama_MenghasilkanDuaBaris` |
| Pembatalan beralasan, tanpa alasan, dan berulang | Tanpa alasan `400`; berhasil sekali; pembatalan ulang `409`; kejadian tetap tampil pada riwayat beserta alasannya; hitungan menjadi `0` | `PASS` | `…VisiteYangDibatalkan_TetapTersimpanDanTidakDihitung` |
| Pembangkitan SQL migration arah maju dan mundur | Keduanya dihasilkan tanpa galat | `PASS` | `dotnet ef migrations script` dua arah |
| **Dua baris berkunci sama ditolak PostgreSQL** | Baris kedua ditolak database; tersisa satu baris | `PASS` **8 September 2026** | `PhysicianVisitUniquenessTests.KunciPermintaanKembar_DitolakDatabase` — lihat bagian 9 |
| **Dua visite dokter yang sama pada tanggal sama diterima PostgreSQL** | Keduanya diterima; hitungan `2`; dua nomor bisnis berbeda | `PASS` **8 September 2026** | `PhysicianVisitUniquenessTests.DuaVisitePadaTanggalSama_DiterimaKeduanya` — lihat bagian 9 |
| **Uji migration maju dan mundur terhadap PostgreSQL** | Berhasil kedua arah | `PASS` **5 September 2026** | Lihat bagian 8.2 |
| `dotnet test` seluruh berkas uji SQLite | `Failed: 0, Passed: 219` | `PASS` | Keluaran perintah |

Uji manual: `NOT APPLICABLE`.

**Tidak dijalankan:**

- **Dua uji PostgreSQL yang diminta task.** Keduanya sudah ditulis, ikut terkompilasi, dan siap
  dijalankan. Ketika dicoba, fixture berhenti pada penjagaannya sendiri dengan penanda
  `BLOCKED_BY_TEST_DB_CONFIGURATION`: environment variable database uji tidak diisi. Lingkungan
  kerja tidak memiliki PostgreSQL lokal dan Docker Desktop tidak berjalan, sehingga container
  sekali pakai tidak dapat dinyalakan. Mengarahkannya ke database bersama dilarang tegas, dan itu
  tidak dilakukan.
- **Uji migration maju-mundur terhadap PostgreSQL**, dengan alasan yang sama.
- Eksekusi migration ke database mana pun.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Tabel bernama `CliPhysicianVisit` terbentuk — **bukan** berawalan `Trx` | Terpenuhi | Migration `20260903093510_AddCliPhysicianVisit` membuat `public."CliPhysicianVisit"`; `…TabelVisite_BentuknyaSesuaiKamusData` |
| 2. Kunci permintaan **wajib terisi** dan dijaga unique penuh | Terpenuhi pada bentuknya | Kolom `IdempotencyKey` tidak nullable; `IX_CliPhysicianVisit_IdempotencyKey` unique tanpa penyaring. Penegakannya oleh PostgreSQL belum diuji — lihat kriteria 6 |
| 3. Index perawatan-waktu dan dokter-waktu terbentuk | Terpenuhi | `IX_CliPhysicianVisit_InpEpisodeId_VisitDateTime` dan `IX_CliPhysicianVisit_DoctorId_VisitDateTime` |
| 4. **Tidak ada** unique atas pasangan perawatan, dokter, dan tanggal | Terpenuhi | `…TabelVisite_BentuknyaSesuaiKamusData`; ditambah `…DuaVisitePadaTanggalSama_MenghasilkanDuaBaris` yang membuktikan dua baris memang lahir |
| 5. Nomor bisnis dialokasikan service lewat penyedia seri nomor, **bukan** Count+1 atau Max+1 | Terpenuhi, **dengan satu selisih bentuk** | `…NomorVisite_TidakDibentukDariHitunganBaris`. Penyedianya adalah service nomor milik modul sendiri, bukan penyedia seri nomor milik Billing — lihat catatan di bawah |
| 6. Migration maju dan mundur berhasil | **Belum terpenuhi** | SQL kedua arah dihasilkan tanpa galat, tetapi belum dijalankan terhadap PostgreSQL |

**Catatan kriteria 5 — penyedia nomor mana yang dipakai.** Repository ini memiliki satu penyedia
seri nomor berbasis tabel, `BillingNumberSeriesService` beserta tabel `BilNumberSeries`. Keduanya
dimiliki `BillingManagement` dan sampai hari ini **hanya dipakai di dalam modul itu**; pendaftaran
dan seluruh pemakaiannya berada di sana. Memanggilnya dari `ClinicalManagement` berarti menulis ke
tabel milik modul lain tanpa wewenang lintas modul, dan task ini tidak memilikinya.

Yang dipakai adalah pola alokasi nomor milik modul sendiri, sama persis dengan
`InpEpisodeNumberService` dan `EmergencyDocumentNumberService` yang sudah berjalan pada repository
ini: awalan, waktu sampai detik, lalu enam huruf/angka acak — contohnya
`VST-260903074012-A1B2C3`, 23 karakter, muat pada kolom 30 karakter. Larangan inti kriteria 5 —
`Count + 1` dan `Max + 1` — dipatuhi penuh, dan `QBE-CODE-004` dipenuhi lewat unique index pada
nomor bisnisnya. Selisih bentuk ini diteruskan kepada pemilik arsitektur backend bila kelak
penyedia seri nomor bersama hendak dijadikan wajib lintas modul.

**Definition of Done.**

| Butir | Status |
| --- | --- |
| Keenam acceptance criteria terbukti | Terpenuhi — kriteria 6 ditutup 5 September 2026, lihat bagian 8.2 |
| Satu migration | Terpenuhi — `20260903093510_AddCliPhysicianVisit` |
| Dua test PostgreSQL hijau | Terpenuhi **8 September 2026** — keduanya dijalankan dan hijau; lihat bagian 9 |
| Laporan menyebut nama tabel apa adanya | Terpenuhi — `CliPhysicianVisit`, pada schema `public` |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning baru dari berkas task ini |
| Masalah yang diketahui | Tabel dan service sudah ada, tetapi belum punya permukaan API. Pencatatan dan pembatalan lewat endpoint adalah `BE-RWI-048` dan `BE-RWI-049` |
| Risiko tersisa | Penegakan unique oleh PostgreSQL belum diuji. Sampai kedua uji PostgreSQL hijau, jaminan "tombol tertekan dua kali tidak melahirkan dua kejadian" baru terbukti pada lapisan aplikasi dan pada SQLite, belum pada database yang sesungguhnya dipakai |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` pada bagian task ini |
| Status Git | Tidak ada stage, commit, maupun push |
| Langkah berikutnya | Menyalakan PostgreSQL sekali pakai, mengisi environment variable database uji, lalu menjalankan `PhysicianVisitUniquenessTests` beserta uji migration maju-mundur. Setelah keduanya hijau, status task dapat dinaikkan menjadi selesai |

---

## 8. Pembaruan 5 September 2026 — kriteria 6 ditutup, dua test masih `NOT RUN`

### 8.1 Lingkungan uji

| Hal | Nilai |
| --- | --- |
| Server | PostgreSQL **15.15** (Debian), `160.22.250.77:5432` |
| Database | `QuilvianNewDevHamzah` — database pengembang **perorangan** |
| Wewenang | Diberikan eksplisit oleh pemilik Product/Domain 5 September 2026 |
| Keadaan awal | 115 dari 135 migration terpasang, 607 tabel, 175 encounter data nyata |

### 8.2 Kriteria 6 — migration maju dan mundur

| Langkah | Hasil |
| --- | --- |
| Maju | `Done.` — `20260903093510_AddCliPhysicianVisit` terpasang |
| Bukti maju | Tabel **`CliPhysicianVisit`** berdiri: **29 kolom**, **14 index** |
| Mundur | `Done.` — tabel hilang seluruhnya, `0` sisa pada katalog |
| Maju lagi | `Done.` — tabel kembali utuh |

Empat hal yang diminta acceptance criteria, dibaca langsung dari `pg_indexes`:

| Yang diminta | Yang ada di PostgreSQL |
| --- | --- |
| Nama tabel `Cli*`, bukan `Trx*` | `CliPhysicianVisit` — terpenuhi |
| Kunci permintaan unique **penuh** | `IX_CliPhysicianVisit_IdempotencyKey` — `CREATE UNIQUE INDEX … USING btree ("IdempotencyKey")`, **tanpa** klausa `WHERE` |
| Kedua index waktu ada | `IX_CliPhysicianVisit_DoctorId_VisitDateTime` dan `IX_CliPhysicianVisit_InpEpisodeId_VisitDateTime` |
| **Nol** unique atas pasangan perawatan-dokter-tanggal | Terbukti — hanya dua unique index yang ada, yaitu `IdempotencyKey` dan `PhysicianVisitNumber` |

Butir terakhir itu yang paling menentukan: unique atas pasangan perawatan-dokter-tanggal akan
menolak visite kedua yang sah pada hari yang sama, dan katalog membuktikan index seperti itu
memang tidak dibuat.

### 8.3 Yang **masih** belum terpenuhi

| Butir DoD | Status | Sebab |
| --- | --- | --- |
| Dua test PostgreSQL hijau | ⛔ **`NOT RUN`** pada 5 September 2026; **ditutup 8 September 2026**, lihat bagian 9 | `BLOCKED_BY_TEST_DB_CONFIGURATION` |

`PhysicianVisitUniquenessTests` memakai `BillingTestDatabaseFixture`, yang **menjalankan
`Database.Migrate()` lalu menulis dan menghapus baris**. Karena itu fixture menolak setiap
database yang namanya mengandung `dev`, `shared`, `staging`, `uat`, `prod`, atau `live`, dan
menuntut nama yang mengandung `test` sebagai bukti afirmatif — penjagaan yang dipasang setelah
insiden `RJ-BIL-BE-002`.

`QuilvianNewDevHamzah` ditolak oleh penjagaan itu, dan **penolakannya benar**: database itu
adalah lingkungan kerja pemiliknya, bukan database sekali pakai yang boleh ditulisi dan dibuang.
Role `Quilvian_2026@` tidak memiliki hak `CREATEDB`, sehingga database uji tersendiri tidak dapat
dibuat dari sesi ini.

**Keputusan pemilik 5 September 2026: dilewati.** Penjagaan fixture sengaja **tidak** dilemahkan.

Yang dibutuhkan untuk menutupnya: satu database bernama misalnya `QuilvianHamzahTest` pada server
yang sama, dibuat oleh yang berwenang, lalu `QUILVIAN_BILLING_TEST_DB` diisi menunjuk ke sana.
Uji-nya sudah ditulis dan terkompilasi; tidak ada pekerjaan implementasi yang tersisa.

### 8.4 Catatan penutup pembaruan

| Hal | Isi |
| --- | --- |
| Migration | `20260903093510_AddCliPhysicianVisit` — terpasang pada `QuilvianNewDevHamzah` |
| Validasi | `dotnet test` project uji SQLite `Failed: 0, Passed: 324`; project `Tests` `Failed: 0, Passed: 288` |
| Status Git | Tidak ada stage, commit, maupun push |

---

## 9. Pembaruan 8 September 2026 — dua test PostgreSQL hijau, task ditutup

### 9.1 Database uji tersendiri akhirnya tersedia

Gerbang yang menahan task ini sejak 4 September 2026 bukan ketiadaan PostgreSQL, melainkan
ketiadaan database yang **boleh dibuang**. `BillingTestDatabaseFixture` menjalankan
`Database.Migrate()` lalu menulis dan menghapus baris, sehingga ia menolak setiap nama database
yang mengandung `dev`, `shared`, `staging`, `uat`, `prod`, atau `live`, dan menuntut penanda
`test` sebagai bukti afirmatif.

Penjagaan itu **tidak dilemahkan, tidak diberi jalan pintas, dan tidak diberi override**. Yang
disediakan justru database yang memenuhi tuntutannya apa adanya: satu container PostgreSQL
sekali pakai.

| Hal | Nilai |
| --- | --- |
| Server | PostgreSQL **15.15** (Debian 15.15-1.pgdg13+1) — versi yang sama persis dengan server pengembang |
| Bentuk | Container sekali pakai `quilvian-rwi-pgtest`, dari image `postgres:15.15` |
| Database | `quilvian_rwi_test` — memuat penanda `test`, nol penanda terlarang |
| Alamat | `localhost:55432`, terpisah penuh dari server mana pun yang dipakai bersama |
| Isi awal | Kosong. Migration dijalankan dari nol: **148 migration**, **555 tabel** |
| Data nyata yang tersentuh | **Nol.** Tidak ada satu pun perintah dikirim ke `160.22.250.77` maupun database bersama lainnya |

Cara ini menutup gerbangnya tanpa menunggu DBA membuat database dan tanpa hak `CREATEDB` pada
server bersama: databasenya lahir dan mati di mesin yang sama dengan yang menjalankan uji.

Cara mengulanginya, apa adanya:

```bash
docker run -d --name quilvian-rwi-pgtest     -e POSTGRES_USER=rwitest -e POSTGRES_PASSWORD=rwitest     -e POSTGRES_DB=quilvian_rwi_test     -p 55432:5432 postgres:15.15

export QUILVIAN_BILLING_TEST_DB="Host=localhost;Port=55432;Database=quilvian_rwi_test;Username=rwitest;Password=rwitest"

dotnet test Tests/QuilvianSystemBackend.IntegrationTests.Postgres

docker rm -f quilvian-rwi-pgtest
```

Container-nya **dihapus** setelah uji selesai; ia tidak ditinggalkan berjalan.

### 9.2 Hasil kedua test

| Test | Yang dibuktikan | Hasil |
| --- | --- | --- |
| `KunciPermintaanKembar_DitolakDatabase` | Baris kedua berkunci sama ditolak **database**, bukan pemeriksaan aplikasi yang kebetulan lebih dulu berjalan. Sesudahnya tersisa tepat satu baris | `PASS` |
| `DuaVisitePadaTanggalSama_DiterimaKeduanya` | Dua visite dokter yang sama pada tanggal yang sama **diterima keduanya**; hitungan `2`; dua nomor bisnis berbeda | `PASS` |

Perintah dan hasilnya:

```text
dotnet test Tests/QuilvianSystemBackend.IntegrationTests.Postgres --filter PhysicianVisitUniquenessTests
Passed!  -  Failed: 0, Passed: 3, Skipped: 0, Total: 3
```

Angka tiga, bukan dua, karena satu test milik `BE-RWI-048` tinggal di kelas yang sama —
lihat [laporan BE-RWI-048](BE-RWI-048.md).

### 9.3 Cacat yang tersingkap saat gerbangnya dibuka

Menjalankan uji yang selama ini hanya dikompilasi menyingkap satu hal yang tidak terlihat selama
ia tidak pernah dijalankan: **`DuaVisitePadaTanggalSama_DiterimaKeduanya` gagal pada percobaan
pertama.**

| Hal | Isi |
| --- | --- |
| Gejala | `Assert.True(pagi.IsSuccess)` gagal |
| Sebab | Uji memakai jam tetap: `DateTime.UtcNow.Date.AddHours(7)` untuk visite pagi dan `+9 jam` untuk visite sore, yaitu pukul 07.00 dan 16.00 UTC |
| Kenapa baru sekarang | Uji ini ditulis 3 September 2026 pada `BE-RWI-041`. Sehari kemudian `BE-RWI-048` menambahkan penjagaan `VAL-DOK-16`: waktu kedatangan **tidak boleh melewati sekarang**, dengan toleransi 2 menit. Sejak itu uji ini hanya dapat hijau bila dijalankan lewat pukul 16.00 UTC |
| Saat dijalankan | Pukul 05.16 UTC — kedua jam tetap itu masih di masa depan, sehingga **keduanya** ditolak `400` |
| Perbaikan | Kedua waktu diturunkan dari jam sekarang dan dijamin sudah lewat, sambil tetap berada pada tanggal yang sama |

Yang dibuktikan uji itu **tidak berubah**: dua kunjungan nyata pada tanggal yang sama, keduanya
diterima, hitungannya dua. Yang berubah hanya cara waktunya ditentukan.

**Penjagaan `VAL-DOK-16` sengaja tidak disentuh.** Menolak waktu kedatangan yang belum terjadi
adalah perilaku yang benar: kunjungan besok bukan fakta, dan mencatatnya membuat hitungan visite
hari ini memuat kunjungan yang belum terjadi. Yang keliru adalah asumsi jam pada uji, dan itulah
yang diperbaiki.

Assertion-nya sekaligus diberi pesan kegagalan — `Assert.True(pagi.IsSuccess, pagi.ErrorMessage)`
— supaya kegagalan berikutnya menyebutkan alasan penolakannya, bukan sekadar `Expected: True`.

### 9.4 Bukti katalog PostgreSQL

Dibaca langsung dari `pg_indexes` pada database uji sesudah 148 migration terpasang:

| Yang diminta acceptance criteria | Yang ada di PostgreSQL |
| --- | --- |
| Kunci permintaan unique **penuh**, tanpa penyaring | `CREATE UNIQUE INDEX "IX_CliPhysicianVisit_IdempotencyKey" ON public."CliPhysicianVisit" USING btree ("IdempotencyKey")` — **tanpa** klausa `WHERE` |
| Nomor bisnis unique | `IX_CliPhysicianVisit_PhysicianVisitNumber` |
| **Nol** unique atas pasangan perawatan, dokter, dan tanggal | Terbukti — hanya tiga index unique yang ada pada tabel ini, yaitu primary key, kunci permintaan, dan nomor bisnis |

### 9.5 Berkas yang berubah pada pembaruan ini

| Berkas | Perubahan |
| --- | --- |
| `Tests/QuilvianSystemBackend.IntegrationTests.Postgres/ClinicalIntegration/PhysicianVisitUniquenessTests.cs` | Waktu kunjungan pada `DuaVisitePadaTanggalSama_DiterimaKeduanya` diturunkan dari jam sekarang, bukan jam tetap; assertion diberi pesan kegagalan |

Nol perubahan pada source aplikasi milik task ini. Nol migration. Nol perintah ke database mana
pun selain container sekali pakai.

### 9.6 Catatan penutup pembaruan

| Hal | Isi |
| --- | --- |
| Status akhir | ✅ **Selesai.** Keenam acceptance criteria dan keempat butir Definition of Done terpenuhi |
| Peringatan | Nol peringatan build baru dari berkas task ini |
| Risiko tersisa | Container uji bersifat sekali pakai dan **tidak** otomatis dinyalakan CI. Selama CI belum menyediakan PostgreSQL, kedua test ini akan kembali `NOT RUN` di sana — bukan gagal, melainkan terhalang konfigurasi. Menyediakannya di CI dicatat sebagai pekerjaan lingkungan, di luar lingkup task ini |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Tidak ada stage, commit, maupun push |
