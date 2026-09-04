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
| Status | 🟡 **Sebagian.** Lima dari enam acceptance criteria terbukti; kriteria 6 — migration maju dan mundur berhasil — **belum terbukti**, dan dua test PostgreSQL yang diminta **sudah ditulis tetapi belum dijalankan** karena tidak ada PostgreSQL yang tersedia |

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
| **Dua baris berkunci sama ditolak PostgreSQL** | **Tidak dijalankan** | `NOT RUN` | Uji sudah ditulis: `PhysicianVisitUniquenessTests.KunciPermintaanKembar_DitolakDatabase` |
| **Dua visite dokter yang sama pada tanggal sama diterima PostgreSQL** | **Tidak dijalankan** | `NOT RUN` | Uji sudah ditulis: `PhysicianVisitUniquenessTests.DuaVisitePadaTanggalSama_DiterimaKeduanya` |
| **Uji migration maju dan mundur terhadap PostgreSQL** | **Tidak dijalankan** | `NOT RUN` | Lihat "Tidak dijalankan" |
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
| Keenam acceptance criteria terbukti | **Belum** — kriteria 6 belum terbukti |
| Satu migration | Terpenuhi — `20260903093510_AddCliPhysicianVisit` |
| Dua test PostgreSQL hijau | **Belum terpenuhi** — keduanya sudah ditulis dan terkompilasi, tetapi belum dijalankan |
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
