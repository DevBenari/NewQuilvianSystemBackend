# Laporan Perubahan Backend — `BE-RWI-061`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-061` |
| Judul | Tindakan keperawatan tercatat sekali walaupun tombolnya tertekan dua kali |
| Slice | Gelombang `KEP-MVP-3` |
| Roadmap | [`../../../roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 4 |
| Trace | `FR-KEP-018`, `FR-KEP-019`, `FR-KEP-020`; PRD `CAP-014` aturan 1, 2, 3; `AC-CAP014-01`; `VAL-KEP-13`, `VAL-KEP-14`, `VAL-KEP-15` |
| Contract version | API `0.3.0` grup Nursing Intervention; state transition `0.3.0` bagian 3; integration `INT-KEP-05` |
| Dependency | `BE-RWI-056` — ✅ selesai 8 September 2026 |
| Klasifikasi | `HEAVY` — repository 1 (0), berkas diperiksa > 8 (1), berkas diubah > 3 (1), logika bisnis sedang (1), kontrak baru (1), entity dan migration baru (1), keamanan hak akses baru (1), workflow status baru (1). Total **7** |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source aplikasi, uji, dan `docs/module-blueprints/rawat-inap/keperawatan/**` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `6f7d81e0` pada branch `MHamzah` |
| Tanggal | 7 September 2026; **divalidasi ulang 8 September 2026** |
| Status | ✅ **SELESAI 8 September 2026.** Keenam acceptance criteria terpetakan ke source dan **seluruhnya terbukti penuh**. Butir DoD yang menahan status — *uji PostgreSQL hijau* — **sudah terpenuhi**: `dotnet test` project Postgres, tapis `NursingInterventionIdempotencyTests`, menjawab `Failed: 0, Passed: 3, Total: 3` terhadap PostgreSQL 16 sungguhan. Rinciannya pada bagian 5.1 |

---

## 1. Masalah yang diperbaiki

Tabel tindakan keperawatan **belum ada sama sekali**. Perawat tidak punya tempat mencatat apa yang
benar-benar dikerjakan untuk pasien, kapan, oleh siapa, dan apa hasilnya.

`TrxPatientProcedure` yang sudah ada **tidak dipakai ulang**, dan itu keputusan sadar: tabel itu
mewajibkan `ConsultationId` dan `DoctorId`. Tindakan keperawatan justru sering lahir tanpa
konsultasi dokter sama sekali — perawat mengganti balutan, memasang infus, memantau tanda vital.
Melonggarkan kedua kolom itu akan melemahkan penjagaan bagi tindakan dokter yang membutuhkan
keduanya untuk penagihan.

**Masalah kedua, yang lebih halus.** Ruang perawatan memakai jaringan yang tidak selalu baik.
Perawat menekan Simpan, balasannya tidak sampai, lalu ia menekan lagi. Tanpa penjaga, rekam medis
memuat **dua** pemasangan infus untuk satu pemasangan yang benar-benar terjadi — dan dua-duanya
ikut tertagih.

---

## 2. Proses bisnis

**Tujuan.** Perawat mencatat tindakan yang benar-benar dilakukan beserta waktu dan hasilnya, dan
jaringan yang buruk tidak lagi menghasilkan tindakan ganda pada rekam medis.

**Pelaku.** Perawat yang bertugas pada perawatan itu.

**Pemicu.** Sebuah tindakan sudah selesai dikerjakan.

**Langkah yang berurutan.**

1. Perawat mengirim catatan tindakan beserta kunci permintaan yang dibuat layar.
2. Bila kunci itu **sudah pernah tersimpan**, sistem langsung mengembalikan catatan yang sudah
   ada beserta kode `200`. Tidak ada baris kedua, dan tidak ada galat yang membingungkan.
3. Bila belum, sistem menurunkan konteks perawatan dari kunjungan yang disebut — pasien, episode,
   dan saat pasien masuk kamar.
4. Sistem memeriksa dua batas waktu: tindakan tidak boleh **di masa depan**, dan tidak boleh
   **sebelum pasien masuk kamar**.
5. Bila tindakan itu mendasari sebuah butir rencana asuhan, rujukannya diperiksa: butir milik
   perawatan lain ditolak.
6. Catatan tersimpan berkeadaan **tercatat**. Bila tindakannya dapat ditagih, penanda pengiriman
   tagihannya lahir berkeadaan **menunggu dikirim**.

**Aturan yang berlaku.**

| Aturan | Isinya | Kode |
| --- | --- | --- |
| `VAL-KEP-13` | Waktu tindakan di masa depan | `400` — *"Waktu tindakan tidak boleh melewati waktu sekarang."* |
| `VAL-KEP-14` | Waktu tindakan sebelum pasien masuk kamar | `400` — *"Waktu tindakan sebelum pasien masuk kamar. Periksa kembali waktunya."* |
| `VAL-KEP-15` | Kunci permintaan sama dengan yang sudah tersimpan | **Bukan galat.** `200` beserta catatan yang sudah ada |
| `CAP-014` aturan 3 | Tindakan mendadak boleh tanpa rencana asuhan | — |

**Contoh berangka penjaga idempotency.** Ns. Sari mencatat pemasangan infus dengan kunci
`kunci-001`. Jaringan putus sebelum balasan sampai, dan ia menekan Simpan lagi dengan kunci yang
sama. Yang terjadi: permintaan kedua dijawab `200` beserta **catatan yang sama persis** — bukan
`201` yang melahirkan baris kedua, dan bukan `409` yang menampilkan galat padahal tindakannya sudah
tercatat dengan benar. Di database tetap ada **satu** baris.

**Kenapa penjaganya harus di database, bukan hanya di aplikasi.** Ketika dua instance aplikasi
berjalan bersamaan, keduanya dapat membaca "kunci ini belum ada" pada saat yang sama, lalu keduanya
menyimpan. Pemeriksaan di dalam aplikasi tidak pernah dapat mencegah perlombaan itu; yang dapat
adalah unique index parsial pada database. Karena itu keduanya dipasang, dan ketika penjaga
database yang menolak, aplikasi membaca ulang baris pemenangnya lalu menjawab seperti kiriman ulang
biasa.

**Hasil akhir.** Satu perawatan memiliki daftar tindakan yang terurut menurut **waktu tindakan**,
bukan waktu pencatatan, sehingga perkembangan pasien terbaca seperti yang benar-benar terjadi.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas atau dokumen | Untuk menetapkan |
| --- | --- |
| `roadmap/backend-roadmap.md` kartu `BE-RWI-061` | Scope, acceptance criteria, dan syarat uji PostgreSQL |
| `contracts/api-contract.md` `0.3.0` bagian 3 beserta catatan idempotency | Bentuk endpoint dan kode balasannya |
| `contracts/state-transition-matrix.md` `0.3.0` bagian 3 dan 3.1 | Dua mesin status yang terpisah |
| `contracts/validation-matrix.md` bagian 4 | `VAL-KEP-13`, `VAL-KEP-14`, `VAL-KEP-15` |
| `contracts/integration-contract.md` `0.3.0` `INT-KEP-05` | Keadaan `BillingManagement` hari ini |
| `data/data-dictionary.md` bagian 6 | Bentuk kolom tabel tindakan |
| `Areas/HealthServices/ClinicalManagement/Models/TrxPatientProcedure.cs` | Memastikan alasan tidak memakainya ulang masih berlaku |
| `Areas/HealthServices/ClinicalManagement/Services/PhysicianVisitService.cs` | Pola idempotency yang sudah terbukti pada `BE-RWI-041` |
| `Repositories/Configurations/.../CliPhysicianVisitConfiguration.cs` | Pembanding unique penuh versus unique parsial |
| `Tests/QuilvianSystemBackend.IntegrationTests.Postgres/**` | Fixture, pengaman target database, dan pola uji PostgreSQL |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/ClinicalManagement/Enums/NursingInterventionStatus.cs` | **Baru.** `Recorded`, `Finalized`. Nol nilai `Amended` — dicabut `0.3.0` |
| `Areas/HealthServices/ClinicalManagement/Enums/NursingBillingDispatchStatus.cs` | **Baru.** `NotApplicable`, `Pending`, `Dispatched`, `Failed` |
| `Areas/HealthServices/ClinicalManagement/Models/CliNursingIntervention.cs` | **Baru.** Catatan tindakan keperawatan |
| `Repositories/Configurations/HealthServices/ClinicalManagement/CliNursingInterventionConfiguration.cs` | **Baru.** Unique parsial kunci permintaan; `CarePlanItemId` memakai `SetNull` |
| `Areas/HealthServices/ClinicalManagement/DTOs/NursingInterventionDtos.cs` | **Baru.** Kontrak transport pencatatan dan daftar |
| `Areas/HealthServices/ClinicalManagement/Services/NursingInterventionService.cs` | **Baru.** Pemilik perintah dan pembacaan tindakan |
| `Areas/HealthServices/ClinicalManagement/Controllers/NursingInterventionController.cs` | **Baru.** Dua endpoint slice ini |
| `Repositories/ApplicationDbContext.cs` | Satu `DbSet` baru |
| `Program.cs` | Pendaftaran `NursingInterventionService` |
| `Migrations/20260906151002_AddNursingIntervention.cs` | **Baru.** Satu tabel, tujuh index termasuk unique parsial |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/ClinicalManagement/NursingInterventionTests.cs` | **Baru.** 11 uji acceptance |
| `Tests/QuilvianSystemBackend.IntegrationTests.Postgres/ClinicalIntegration/NursingInterventionIdempotencyTests.cs` | **Baru.** Tiga uji yang hanya dapat dibuktikan PostgreSQL sungguhan |
| `Tests/QuilvianSystemBackend.UnitTests.Sqlite/ClinicalManagement/NursingAssessmentAccessContractTests.cs` | Controller baru ikut dijaga kontrak penamaan hak akses |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Bertambah.** Dua endpoint sesuai `api-contract.md` `0.3.0` |
| Database | **Satu tabel baru** `public."CliNursingIntervention"` beserta unique parsial `IX_CliNursingIntervention_IdempotencyKey` berpenyaring `"IdempotencyKey" IS NOT NULL AND "IsDelete" = false`. Migration `20260906151002_AddNursingIntervention` dibuat dan **belum diterapkan ke database mana pun** |
| Keamanan/Auth | **Resource baru `NursingIntervention`** dengan action `Read` dan `Create` pada slice ini. Nol hardcode nama peran, departemen, jabatan, maupun `UserType` |

---

## 4. Dokumentasi endpoint

#### Health Services / Clinical Management / Nursing Intervention

Base URL: `api/v1/health-services/clinical-management/nursing-interventions`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Mencatat tindakan yang **sudah dilakukan**. Menerima `Idempotency-Key` pada badan permintaan atau header | `NursingIntervention : Create` |
| `GET` | `/episodes/{episodeId}` | Daftar tindakan satu perawatan, terurut waktu tindakan. Penyaring `from`, `to`, `performedBy` | `NursingIntervention : Read` |

### Kode status dan artinya bagi pengguna

| Kode | Artinya |
| --- | --- |
| `201` | Tindakan tersimpan |
| `200` | Tindakan **sudah** tersimpan sebelumnya — kiriman ulang berkunci sama |
| `400` | Waktu tindakan tidak masuk akal, rujukan rencana asuhan keliru, atau akun belum tertaut ke data pegawai |
| `404` | Kunjungan tidak ditemukan |
| `422` | Pasien tidak sedang dirawat inap, atau perawatannya belum atau sudah tidak berjalan |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.sln --no-incremental` | `Build succeeded`, `213 Warning(s)`, `0 Error(s)` | `PASS` | Jumlah warning sama persis dengan garis dasar `6f7d81e` |
| `dotnet test` project SQLite, tapis `NursingInterventionTests` | `Failed: 0, Passed: 11, Total: 11` | `PASS` | Keluaran perintah |
| `dotnet test` project SQLite, seluruhnya | `Failed: 0, Passed: 448, Total: 448` | `PASS` | Garis dasar sebelum slice ini 394 |
| `dotnet test` project InMemory, seluruhnya | `Failed: 9, Passed: 917, Total: 926` | `EXISTING / ENVIRONMENT ISSUE` | Kesembilan kegagalan berada di `BillingManagement`; worktree pada commit `6f7d81e` tanpa perubahan slice ini menghasilkan angka yang sama persis |
| Skenario kirim ulang dengan kunci sama | `201` lalu `200`; satu baris di database; `Id` keduanya sama | `PASS` | `KiriminUlangKunciSama_MenghasilkanSatuBarisDanDijawab200` |
| Skenario kirim dengan kunci berbeda | Dua baris terbentuk | `PASS` | `KunciBerbeda_MenghasilkanDuaTindakan` |
| Skenario tindakan tanpa rencana asuhan | `201`, rujukan rencana kosong | `PASS` | `TindakanMendadak_DapatDicatatTanpaRencanaAsuhan` |
| Skenario dua batas waktu | Masa depan `400`; sebelum masuk kamar `400`; keduanya memakai kalimat validation matrix apa adanya | `PASS` | `WaktuTindakanMasaDepan_Ditolak400`, `WaktuTindakanSebelumMasukKamar_Ditolak400` |
| Bentuk unique parsial pada model EF Core | `IsUnique` benar; penyaring memuat `"IdempotencyKey" IS NOT NULL` **dan** `"IsDelete" = false` | `PASS` | `IndexKunciPermintaan_UniqueDanParsial`; nilai yang sama muncul apa adanya pada `Migrations/20260906151002_AddNursingIntervention.cs` |
| Banyak tindakan tanpa kunci berdampingan | Dua baris berkunci kosong tersimpan tanpa saling menolak | `PASS` | `TindakanTanpaKunci_TidakSalingMenghalangi` |
| `dotnet test` project SQLite, seluruhnya, **8 September 2026** | `Failed: 0, Passed: 453, Skipped: 0, Total: 453` | `PASS` | Dijalankan ulang sesudah `BE-RWI-056` ditutup; nol kegagalan pada uji tindakan keperawatan |
| **Dua permintaan bersamaan dengan kunci sama, terhadap PostgreSQL sungguhan** | `Failed: 0, Passed: 3, Total: 3` | `PASS` | `DuaPermintaanBersamaan_KunciSama_HanyaSatuBaris`; rincian pada bagian 5.1 |
| **Penolakan kunci kembar oleh database sungguhan** | Baris kedua ditolak `DbUpdateException`; tetap satu baris | `PASS` | `KunciKembar_DitolakDatabase` |
| **Penyaring index memang parsial pada database sungguhan** | Tiga tindakan tanpa kunci tersimpan berdampingan | `PASS` | `TigaTindakanTanpaKunci_DiterimaSeluruhnya` |
| **Bentuk index dibaca langsung dari katalog PostgreSQL** | `CREATE UNIQUE INDEX "IX_CliNursingIntervention_IdempotencyKey" … WHERE (("IdempotencyKey" IS NOT NULL) AND ("IsDelete" = false))` | `PASS` | `SELECT indexname, indexdef FROM pg_indexes WHERE tablename = 'CliNursingIntervention'` |

### 5.1 Uji PostgreSQL — bagaimana lingkungannya akhirnya tersedia

Ketiga uji itu sudah ditulis sejak 7 September 2026 pada
`Tests/QuilvianSystemBackend.IntegrationTests.Postgres/ClinicalIntegration/NursingInterventionIdempotencyTests.cs`.
Yang belum ada waktu itu adalah databasenya. Jalan buntu yang tercatat pada revisi laporan
sebelumnya — peran database tanpa `rolcreatedb`, dan satu-satunya database yang tersedia bernama
mengandung `dev` sehingga ditolak fixture — **kedua-duanya benar dan kedua-duanya bukan syarat
yang sebenarnya**. Fixture tidak menuntut database milik server tim; ia menuntut database test
tersendiri yang boleh dibuang.

Databasenya karena itu dibuat sebagai **container PostgreSQL 16 sekali pakai**, bukan dengan
meminta hak baru pada server bersama:

| Langkah | Isi |
| --- | --- |
| Container | `postgres:16` bernama `quilvian-kep-pg`, dipetakan ke port `55433`, dibuang sesudah uji selesai |
| Database | `quilvian_kep_test` — mengandung penanda `test` yang dituntut fixture, dan **tidak** mengandung satu pun penanda terlarang (`prod`, `live`, `staging`, `uat`, `dev`, `shared`) |
| Variable | `QUILVIAN_BILLING_TEST_DB` diisi hanya untuk proses `dotnet test` itu, tidak ditulis ke berkas konfigurasi mana pun |
| Skema | `Database.Migrate()` menerapkan **seluruh** migration dari nol dan berhasil, termasuk `20260906151002_AddNursingIntervention` |

**Tidak satu pun database bersama, dev, staging, atau production tersentuh.** Migration tetap
**belum diterapkan** ke database mana pun yang dipakai orang lain; yang menerimanya adalah container
sekali pakai yang sudah dihapus.

#### Yang akhirnya terbukti

| Bukti | Isi |
| --- | --- |
| Kiriman **bersamaan** | Dua permintaan berkunci sama dimulai serentak pada **dua koneksi berbeda**; keduanya dijawab berhasil, keduanya menunjuk `Id` yang sama, tepat satu ditandai kiriman ulang, dan database berisi **satu** baris |
| Penegakan oleh **database**, bukan aplikasi | Baris kedua berkunci sama yang ditulis **langsung** tanpa melewati pemeriksaan service ditolak `DbUpdateException` |
| Penyaring memang **parsial** | Tiga tindakan tanpa kunci tersimpan berdampingan tanpa saling menolak |
| Bentuk index pada database sungguhan | `pg_indexes` menjawab `CREATE UNIQUE INDEX … WHERE (("IdempotencyKey" IS NOT NULL) AND ("IsDelete" = false))` — sama persis dengan yang diminta kriteria 6 |

Uji manual: `NOT FEASIBLE` — alasannya sama dengan `BE-RWI-059`.

**Tidak dijalankan:**

- **Eksekusi migration ke database bersama, dev, staging, atau production mana pun.** Wewenangnya
  terpisah dan tidak diberikan. Yang dijalankan hanyalah `Database.Migrate()` ke container sekali
  pakai yang sudah dihapus, dan itu bagian dari uji, bukan penerapan.
- **Uji migration mundur (`Down`) terhadap PostgreSQL.** Tidak diminta DoD task ini.
- **Uji hak akses non-SuperAdmin lewat HTTP.** Sama seperti `BE-RWI-055` dan `BE-RWI-059`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Tindakan menyimpan apa, kapan, oleh siapa, dan hasilnya, beserta konteks episode | Terpenuhi | Kolom `InterventionName`, `PerformedAt`, `PerformedByEmployeeId`, `ResultNote`, `InpEpisodeId`, `EncounterId`, `PatientId`; uji `Tindakan_MenyimpanApaKapanSiapaDanHasilnya` dan `DaftarTindakan_TerurutWaktuTindakan` |
| 2. Tindakan mendadak dapat dicatat **tanpa** rujukan ke rencana asuhan (`CAP-014` aturan 3) | Terpenuhi | `CarePlanItemId` nullable; uji `TindakanMendadak_DapatDicatatTanpaRencanaAsuhan` |
| 3. Permintaan berulang dengan `Idempotency-Key` yang sama menghasilkan **satu** baris dan dijawab `200` beserta baris yang sudah ada (`VAL-KEP-15`) | **Terpenuhi** | Kiriman ulang **berurutan**: `KiriminUlangKunciSama_MenghasilkanSatuBarisDanDijawab200` (SQLite). Kiriman **bersamaan**: `DuaPermintaanBersamaan_KunciSama_HanyaSatuBaris` terhadap PostgreSQL 16 sungguhan — dua koneksi berbeda dimulai serentak, keduanya berhasil, `Id`-nya sama, tepat satu ditandai kiriman ulang, dan database berisi satu baris |
| 4. Waktu tindakan di masa depan ditolak `400` (`VAL-KEP-13`) | Terpenuhi | `WaktuTindakanMasaDepan_Ditolak400` |
| 5. Waktu tindakan sebelum pasien masuk kamar ditolak `400` (`VAL-KEP-14`) | Terpenuhi | `WaktuTindakanSebelumMasukKamar_Ditolak400` |
| 6. Unique parsial pada kunci idempotency terbentuk dengan penyaring kunci tidak kosong dan belum terhapus | **Terpenuhi** | Bentuknya pada model EF Core dan berkas migration (`IndexKunciPermintaan_UniqueDanParsial`), **penegakannya** oleh database (`KunciKembar_DitolakDatabase` — baris kedua yang ditulis langsung tanpa melewati service ditolak `DbUpdateException`), **keparsialannya** (`TigaTindakanTanpaKunci_DiterimaSeluruhnya`), dan **bentuk akhirnya dibaca langsung dari katalog PostgreSQL**: `CREATE UNIQUE INDEX … WHERE (("IdempotencyKey" IS NOT NULL) AND ("IsDelete" = false))` |

### Definition of Done

| Butir DoD | Status |
| --- | --- |
| Satu entity | Terpenuhi — `CliNursingIntervention` |
| Configuration | Terpenuhi |
| `DbSet` | Terpenuhi |
| Satu enum | **Terlampaui** — dua enum; penjelasannya pada bagian 7 |
| Migration | Terpenuhi — `20260906151002_AddNursingIntervention`. **Belum diterapkan** ke database bersama, dev, staging, atau production mana pun; yang menerimanya adalah container PostgreSQL 16 sekali pakai yang sudah dibuang |
| Dua endpoint | Terpenuhi |
| Penjaga idempotency di database | **Terpenuhi** — bentuk dan penegakannya terbukti terhadap PostgreSQL 16 sungguhan |
| Keenam acceptance criteria terbukti | **Terpenuhi** — keenamnya terbukti penuh |
| **Uji PostgreSQL hijau** | **Terpenuhi** — `Failed: 0, Passed: 3, Total: 3` |
| `dotnet build` lulus | Terpenuhi |

---

## 7. Catatan penutup

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `ClinicalManagement` |
| Pemilik / prefix registry | `ClinicalManagement / Clinical` — prefix **`Cli`**, lifecycle `ACTIVE / LEGACY` |
| Keberlakuan | `NEW CODE`; `TOUCHED LEGACY` aditif pada `ApplicationDbContext.cs` dan `Program.cs` |
| QBE ID yang berlaku | `QBE-ENT-001`, `QBE-ENT-002`, `QBE-ENT-003`, `QBE-NAM-001`, `QBE-NAM-002`, `QBE-CFG-001`, `QBE-MOD-001`, `QBE-MOD-002`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-CODE-004`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001`, `QBE-ENUM-001`, `QBE-PAGE-001`, `QBE-DEL-001`, `QBE-AUD-001` |
| QBE ID yang **tidak** berlaku | `QBE-CODE-001`, `QBE-CODE-002`, `QBE-CODE-003`, `QBE-CODE-005`, `QBE-CODE-006` — kamus data bagian 6 tidak menetapkan nomor bisnis bagi tabel ini, sehingga tidak ada kode yang dialokasikan sama sekali. `QBE-NAM-003`, `QBE-DB-001`, `QBE-DB-002` — bukan `LEGACY MIGRATION` |
| `QBE-CODE-004` | **Ditegakkan.** Kunci permintaan bersifat unik dan punya unique index parsial pada database sesuai scope-nya |

### Delta kontrak yang dilaporkan ke pemilik kontrak

| Delta | Isinya | Kenapa |
| --- | --- | --- |
| **Dua enum, bukan satu** | Kartu task menyebut "enum keadaan pengiriman tagihan". Yang dibuat: `NursingBillingDispatchStatus` **dan** `NursingInterventionStatus` | Kamus data bagian 6 mencantumkan `RecordStatus` sebagai enum berbawaan `Recorded`, dan mesin status bagian 3 memakai `Recorded` serta `Finalized`. Tidak ada enum yang sudah ada yang cocok dipakai ulang |
| **Nama tabel `CliNursingIntervention`** | Kamus data bagian 6 dan `INT-KEP-05` menulis `TrxNursingIntervention` | `QBE-NAM-001` melarang awalan `Trx*` untuk kode baru, dan roadmap bagian 0.5 sudah menyatakannya sejak kartu task pertama. `INT-KEP-05` yang menyebut `TrxNursingIntervention.IdempotencyKey` karena itu terbaca sebagai `CliNursingIntervention.IdempotencyKey` |
| **Kolom `PatientId` ditambahkan** | Kamus data bagian 6 tidak mencantumkannya | Dibutuhkan pendaftaran ke mesin keutuhan pada `BE-RWI-062`, dan berfungsi sebagai penjaga salah pasien seperti pada `CliPhysicianVisit` |
| **Kolom `FinalizedByUserId`, `BillingDispatchedAt`, `BillingDispatchAttemptCount`, `BillingDispatchFailureReason` ditambahkan** | Kamus data bagian 6 hanya mencantumkan `FinalizedAt` dan `BillingDispatchStatus` | `AC-CAP014-02` menuntut kegagalan pengiriman **terlihat sebagai keadaan yang dapat dicoba ulang**. Keadaan tanpa waktu percobaan, hitungan, dan sebab kegagalan tidak dapat direkonsiliasi siapa pun. Keempatnya dibuat di sini supaya `BE-RWI-062` tidak perlu migration sama sekali |
| **Kolom `AmendReason` tidak dibuat** | Kamus data bagian 6 mencantumkannya | Kontrak `0.3.0` mencabut status `Amended` dan memindahkan koreksi ke mesin addendum milik `MedicalRecordManagement`, yang sudah menyimpan `CorrectionReason` sendiri. Menyediakan kolom kedua untuk jawaban yang sama akan melahirkan dua sumber kebenaran — persis yang dilarang `RWI-DEC-087`. Kamus data pada butir ini tertinggal dari kontrak `0.3.0` |
| **Kunci permintaan bersifat opsional** | Berbeda dari `CliPhysicianVisit` yang mewajibkannya | Mengikuti kamus data bagian 6. Menahan pencatatan tindakan yang sudah dilakukan hanya karena layar tidak mengirim kunci lebih berbahaya daripada risiko duplikat yang masih dapat dikoreksi |
| **Tiga endpoint baseline master data tidak dibuat** | `GET /filters/metadata`, `GET /summary`, `GET /` | Kontrak `0.3.0` tidak memintanya. Dicatat sebagai kekurangan terhadap baseline, bukan didiamkan |

### Ringkasan lain

| Hal | Isi |
| --- | --- |
| Peringatan | Jumlah warning solusi tetap `213`, sama persis dengan garis dasar |
| Masalah yang diketahui | **Nol.** Butir DoD "uji PostgreSQL hijau" sudah terpenuhi pada 8 September 2026. Ramalan revisi sebelumnya terbukti tepat: ketiga uji berjalan **tanpa satu pun perubahan kode**, begitu `QUILVIAN_BILLING_TEST_DB` diarahkan ke database bernama `quilvian_kep_test` |
| Risiko yang **sudah ditutup** | Perlindungan terhadap dua instance aplikasi yang berjalan bersamaan kini terbukti: yang menolak baris kedua adalah **database**, bukan pemeriksaan di dalam aplikasi yang kebetulan lebih dulu berjalan |
| Risiko tersisa | Unique parsial itu baru **benar-benar** menjaga produksi setelah `20260906151002_AddNursingIntervention` diterapkan ke database yang dipakai aplikasi. Sampai itu terjadi, bukti hari ini berlaku bagi skema, bukan bagi baris yang sudah hidup. Penerapannya adalah wewenang terpisah dan **tidak** diminta task ini |
| Risiko tersisa | `BillingManagement` belum memiliki kemampuan transaksi yang menerima pemicu `INT-KEP-05`. Tindakan yang dapat ditagih karena itu menunggu di keadaan `Pending`, dan tidak ada satu pun yang hilang sementara itu |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Perubahan masih di working tree; **agent tidak menjalankan satu pun** `stage`, `commit`, `push`, `pull`, `merge`, `rebase`, maupun deployment. Validasi ulang 8 September 2026 **nol menyentuh berkas source**: yang berubah hanya laporan ini beserta roadmap dan `requirement-traceability.md` |
| Langkah berikutnya | 1. Terapkan `20260906151002_AddNursingIntervention` ke database aplikasi lewat wewenang eksekusi database yang terpisah. 2. Pertimbangkan menuliskan resep container sekali pakai ini pada `Tests/QuilvianSystemBackend.IntegrationTests.Postgres/README.md`, supaya uji PostgreSQL tidak lagi terhenti menunggu hak pada server bersama. 3. `BE-RWI-062` sudah selesai lebih dulu dan tetap hijau pada uji regresi hari ini |
