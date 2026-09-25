# Laporan Perubahan Backend — `BE-HMD-01`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HMD-01` |
| Judul | Fondasi Entity, Konfigurasi EF Core, DbContext, dan Migration 22 Tabel `Hmd*` |
| Slice | `MVP-0` — Fondasi, Model Data, dan Tata Kelola |
| Roadmap | `docs/module-blueprints/hemodialisa/roadmap/backend-roadmap.md` bagian 2.1 |
| Trace | `HMD-DEC-004`, `HMD-DEC-007`, `HMD-DEC-008`, `FR-HMD-001`, `FR-HMD-030`, `NFR-001`, `NFR-002`; `02-backend-architecture.md` bagian 3, 5, 6, 8; `data/data-dictionary.md` seluruh entitas |
| Contract version | `HMD-CONTRACT-v1` — `approved` 18 September 2026 menurut kepala roadmap (catatan: kepala `data-dictionary.md` masih tertulis `draft`, lihat bagian 7) |
| Dependency | Gerbang registry `HMD-DEC-007` (tindakan 1–4 selesai 22 September 2026, tindakan 5 dijalankan pemilik) dan persetujuan kontrak 18 September 2026 |
| Klasifikasi | `MEDIUM` — skor 7: repository 0, berkas diperiksa 2, berkas diubah 2, logika 1, kontrak API 0, database 2, keamanan 0, UI 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/HemodialysisManagement/Models/`, `.../Enums/`, `Repositories/Configurations/HealthServices/HemodialysisManagement/`, `Repositories/ApplicationDbContext.cs`, `Migrations/` |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit backend saat dikerjakan | `25b02786` pada branch `MHamzah` — seluruh perubahan belum di-commit |
| Tanggal | 22 September 2026 |
| Status | ✅ Selesai — tiga acceptance criteria terpetakan ke source, migration diterapkan ke DB pribadi `QuilvianNewDevHamzah` |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module / Submodule | `HemodialysisManagement` / `Hemodialysis` |
| Pemilik / prefix registry | `HemodialysisManagement / Hemodialysis` — `Hmd` |
| Status registry | `ACTIVE` — baris ditambahkan 22 September 2026 pada `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` dan dua salinan suite skill (identik, md5 `9d1d3039…`); belum di-commit |
| Keberlakuan | `NEW CODE` untuk seluruh `Hmd*`; `TOUCHED LEGACY` untuk `ApplicationDbContext.cs` dan `ApplicationDbContextModelSnapshot.cs` |
| QBE ID yang berlaku | `QBE-MOD-002`, `QBE-MOD-003`, `QBE-NAM-001`, `QBE-NAM-002`, `QBE-NAM-004`, `QBE-ENT-001`, `QBE-ENT-002`, `QBE-CFG-001`, `QBE-CODE-004`, `QBE-DEL-001`, `QBE-ENUM-001` |

---

## 1. Masalah yang diperbaiki

Sebelum task ini, sistem tidak punya tempat menyimpan apa pun tentang cuci darah. Permintaan HD
dari bangsal, program HD pasien, resep dialisis, jadwal mesin, dan catatan sesi tidak dapat
dicatat sama sekali, sehingga seluruh task berikutnya tidak mungkin dikerjakan.

Contoh akibatnya: koordinator unit HD yang ingin memastikan mesin `M-01` tidak dipakai dua pasien
pada jam yang sama tidak punya data jadwal untuk diperiksa. Pemeriksaan itu hanya bisa dilakukan
di buku catatan manual.

---

## 2. Proses bisnis

Task ini tidak menambah proses yang dijalankan pengguna. Ia menyiapkan **tempat penyimpanan** untuk
seluruh proses modul Hemodialisa:

1. **Permintaan** masuk (`HmdOrder`) dari rawat inap, IGD, atau rawat jalan.
2. **Program HD pasien** (`HmdEpisode`) beserta kelayakan, akses vaskular, tinjauan serologi, dan
   keputusan isolasi.
3. **Resep HD** (`HmdPrescription`) yang tidak dapat diubah setelah aktif dan diganti dengan resep baru.
4. **Sesi** (`HmdSession`) beserta checklist Pra-HD, penilaian pra/pasca, observasi, obat,
   komplikasi, dan penugasan petugas.
5. **Sumber daya unit**: mesin dan riwayat statusnya, station, butir checklist, butir kesiapan,
   lembar kesiapan shift, dan pengaturan unit.

Aturan yang dijaga langsung oleh basis data — lapis terakhir bila pemeriksaan di service terlewat:

| Aturan | Penjaga di database | Contoh |
| --- | --- | --- |
| Satu pasien hanya punya satu program HD aktif | Unique index bersyarat `IX_HmdEpisode_PatientId_Active` (`EpisodeStatus = 2` dan `IsDelete = false`) | Ibu Sinta sudah punya episode aktif `HD-EP-…`; baris aktif kedua ditolak database |
| Satu episode hanya punya satu resep aktif | `IX_HmdPrescription_EpisodeId_Active` | Resep lama wajib `Superseded` dulu sebelum resep baru aktif |
| Tombol Mulai yang ditekan dua kali tidak membuat dua sesi berjalan | Unique `IX_HmdSession_IdempotencyKey` dan `IX_HmdSession_PatientProcedureId` (bila terisi) | Kunci `abc-123` dikirim dua kali, baris kedua ditolak |
| Satu lembar kesiapan per unit, tanggal, dan shift | Unique `(ServiceUnitId, ReadinessDate, Shift)` bagi baris `IsDelete = false` | Shift Pagi 10 September hanya punya satu lembar |
| Pemeriksaan tabrakan jadwal cepat | Index biasa `(MachineId, ScheduledStartAt, ScheduledEndAt)`, `(StationId, …)`, `(EpisodeId, ScheduledStartAt)` | Dipakai service ketika memeriksa `M-01` pukul 07.00–11.00 |

Penghapusan seluruh tabel bersifat penandaan `IsDelete`; semua relasi memakai `Restrict` sehingga
tidak ada penghapusan berantai.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/hemodialisa/data/data-dictionary.md` seluruh bagian, `02-backend-architecture.md` bagian 3, 5, 6, 8
- `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, `AGENTS.md` backend, `rules/backend/**` suite skill
- Pola domain terdekat: `Areas/HealthServices/BloodBankManagement/**`, `Areas/HealthServices/RadiologyManagement/**`, `Repositories/Configurations/HealthServices/**`
- Entity yang dirujuk: `MstPatient`, `RegPatientEncounter`, `InpEpisode`, `MstDoctor`, `MstWorkforceProfile`, `MstServiceUnit`, `MstRoom`, `MstProcedure`, `TrxPatientProcedure`, `MstDrug`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/HemodialysisManagement/Enums/HemodialysisEnums.cs` | Baru. Seluruh enum modul: status permintaan, episode, resep, sesi (12 nilai), shift, kebutuhan isolasi, hasil checklist, status serah terima Farmasi/Billing, status kompetensi, status mesin/station/kesiapan, dan lainnya |
| `Areas/HealthServices/HemodialysisManagement/Models/Hmd*.cs` (22 berkas) | Baru. Seluruhnya mewarisi `IdentityModel`, tabel tunggal PascalCase pada schema `public` |
| `Repositories/Configurations/HealthServices/HemodialysisManagement/Hmd*Configuration.cs` (22 berkas) | Baru. Panjang kolom, presisi desimal, enum sebagai integer, FK `Restrict`, dan index sesuai kamus data |
| `Repositories/ApplicationDbContext.cs` | Region `HEMODIALYSIS MANAGEMENT` berisi 22 `DbSet<Hmd*>`; akhir baris CRLF dipertahankan |
| `Migrations/20260922044002_AddHemodialysisManagement.cs` dan `.Designer.cs` | Baru, dihasilkan `dotnet ef migrations add` — 22 `CreateTable` pada `Up()`, 22 `DropTable` pada `Down()` |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Dihasilkan ulang oleh EF. Lihat catatan diff besar pada bagian 7 |

**Delta kolom terhadap `data-dictionary.md`** — kolom yang dibutuhkan kriteria task lain tetapi
belum tertulis di kamus data:

| Tabel | Kolom tambahan | Alasan |
| --- | --- | --- |
| `HmdOrder`, `HmdEpisode`, `HmdPrescription`, `HmdSession`, `HmdMachine`, `HmdStation`, `HmdUnitReadiness`, `HmdSetting` | `Version` (concurrency token) | Dua orang menekan tombol yang sama bersamaan → yang kedua mendapat `409`, bukan menimpa diam-diam |
| `HmdEpisode` | `ActivatedAt/By`, `ClosedAt/By` | Jejak siapa yang mengaktifkan dan menutup program |
| `HmdPrescription` | `DialysateComposition`, `SodiumBicarbonateProfile`, `DialysateTemperatureC`, `ActivatedAt/By` | Parameter resep yang disebut `BE-HMD-09` |
| `HmdSession` | `CheckedInByUserId`, `ReadyAt/By`, `HoldReason`, `EndedByUserId`, `CancelReason`, `ReturnReason`, `RecordHash` | Jejak pelaku setiap perpindahan status dan sidik jari catatan final |
| `HmdSessionMedication` | `PharmacyStorageLocationId`, `PharmacyMeasurementId`, `PharmacyQuantity`, `HandoffAttemptedAt`, `HandoffError` | Masukan wajib `DrugUsageService` dan alasan kegagalan penerusan |
| `HmdSessionComplication` | `Severity` | Derajat keparahan yang diminta `BE-HMD-14` |
| `HmdStation` | `StatusReason`, `LastStatusChangedAt` | Alasan perubahan status station |
| `HmdChecklistItem` | `OverridableDecisionNote/DecidedByUserId/DecidedAt`; `CheckSequence` menggantikan `SortOrder` | Jejak keputusan tata kelola klinis; `SortOrder` generik dilarang untuk kode baru |
| `HmdReadinessItem` | `CheckSequence` | Sama dengan di atas |
| `HmdSetting` | `ProcedureId` (FK `MstProcedure`) | Tindakan yang diterbitkan saat sesi dimulai |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — task ini tidak membuat endpoint |
| Database | 22 tabel baru, 138 index, 46 foreign key. Migration `20260922044002_AddHemodialysisManagement` **diterapkan** ke DB pribadi `QuilvianNewDevHamzah` pada 22 September 2026 atas wewenang pemilik. `Down()` diuji lalu diterapkan kembali. Tidak ada tabel modul lain yang diubah. DB tim dan deployment tidak disentuh |
| Keamanan/Auth | `NOT APPLICABLE` — tidak ada perubahan authorization |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task fondasi data tanpa controller.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build .\QuilvianSystemBackend.csproj --configuration Debug` (keluaran diarahkan ke folder scratch karena `bin/` dikunci aplikasi pengguna yang sedang berjalan) | `0 Error(s)`, `224 Warning(s)`, waktu 58 detik; **0 warning dari berkas Hemodialisa** | `PASS` | Log build 22 September 2026 11.52 WIB, diulang 12.19 WIB sesudah perbaikan gerbang checklist `BE-HMD-12` dengan angka yang sama. Garis dasar 21 September 2026: 222 warning. Warning pada berkas yang disentuh hanya berada di method lama yang tidak diubah |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` atas 94 berkas `.cs` baru/berubah | `VIOLATION: 0`, `REVIEW: 0`, `Final result: PASS` | `PASS` | Dijalankan ulang 22 September 2026 setelah seluruh source final |
| Isi migration | 22 `CreateTable`, 22 `DropTable`, 0 `AlterColumn`/`AddColumn`/`DropColumn` | `PASS` | Pemeriksaan teks berkas migration |
| Katalog DB `QuilvianNewDevHamzah` sesudah `Up()` | 22 tabel `Hmd*`, 138 index, 46 FK, baris `20260922044002_AddHemodialysisManagement` di `__EFMigrationsHistory` | `PASS` | Kueri `information_schema`/`pg_indexes`/`pg_constraint` baca-saja lewat `dotnet fsi` + Npgsql; nama DB dikonfirmasi `current_database()` |
| Index bersyarat terpasang | `IX_HmdEpisode_PatientId_Active … WHERE (("EpisodeStatus" = 2) AND ("IsDelete" = false))`, begitu pula resep aktif, `IdempotencyKey`, `PatientProcedureId`, penugasan petugas, lembar kesiapan | `PASS` | Definisi dari `pg_indexes` |
| `Down()` lalu `Up()` kembali | Seluruh 22 tabel hilang lalu terbentuk ulang tanpa sisa | `PASS` | Dijalankan pada sesi 22 September 2026 terhadap `QuilvianNewDevHamzah` |
| Diff snapshot terhadap `HEAD` | 7.702 dari 7.703 baris yang tampak "terhapus" muncul kembali utuh di tempat lain; satu sisanya baris pertama karena BOM | `PASS` | Perbandingan multiset baris diff |
| AUTOMATED TEST | `NOT APPLICABLE` — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`) | `NOT APPLICABLE` | Keputusan pengguna 22 September 2026: "Tanpa test" |

Uji manual: `NOT APPLICABLE` — task tanpa endpoint.

**Tidak dijalankan:** tidak ada.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. `dotnet ef migrations add AddHemodialysisManagement` menghasilkan kode yang menciptakan tepat 22 tabel berprefix `Hmd` tanpa menyentuh tabel modul lain | Terpenuhi | Migration berisi tepat 22 `CreateTable` `Hmd*` dan tidak satu pun operasi pada tabel lain |
| 2. Unique index `HmdEpisode (PatientId, EpisodeStatus)` untuk status `Active`; unique composite index jadwal sesi untuk mesin dan waktu | Terpenuhi, **dengan delta** | Bagian episode: `IX_HmdEpisode_PatientId_Active` unique bersyarat. Bagian jadwal: dipasang **index biasa** `IX_HmdSession_Machine_Window` mengikuti `data-dictionary.md` bagian 3.8 yang menetapkan "index biasa, bukan unique, karena tumpang tindih waktu tidak dapat dinyatakan sebagai keunikan sederhana". Tabrakan dicegah `HmdScheduleService` di dalam transaksi dengan `pg_advisory_xact_lock` (`BE-HMD-10`) |
| 3. Migration dapat dijalankan `Up()` dan dibatalkan `Down()` tanpa artefak yatim | Terpenuhi | `Up()` → `Down()` → `Up()` pada `QuilvianNewDevHamzah` |
| DoD: migration terbentuk otomatis dari model EF Core | Terpenuhi | Dihasilkan `dotnet ef migrations add`, bukan ditulis tangan |
| DoD: kompilasi 100% bebas error | Terpenuhi | `0 Error(s)`. Bunyi "0 warning" pada bukti roadmap tidak mungkin berlaku untuk solusi (garis dasar 222 warning); yang dipenuhi adalah 0 warning dari berkas task ini |
| DoD: QBE preflight lolos untuk seluruh entity `Hmd*` | Terpenuhi | QBE Strict `PASS` |
| Bukti roadmap: "uji coba rollback `Down()`" | Terpenuhi | Lihat kriteria 3 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Diff `ApplicationDbContextModelSnapshot.cs` terlihat sangat besar (±10.254 tambah, ±7.703 hapus) karena EF menyusun ulang urutan entity yang sebelumnya disunting manual dan menambah BOM. Isinya bagi entity lain tidak berubah — dibuktikan perbandingan multiset baris. Risiko nyatanya adalah konflik merge dengan anggota tim yang juga menambah migration |
| Masalah yang diketahui | Kepala `data-dictionary.md` masih tertulis `status draft` dan `approved_by —`, sementara roadmap dan manifest menyatakan kontrak `approved`. Di luar wewenang build skill untuk mengubahnya |
| Risiko tersisa | Index jadwal non-unique berarti tabrakan hanya dicegah service; penulisan langsung ke tabel di luar service tidak terjaga |
| Perubahan sampingan | `NONE` |
| Interupsi | Satu kali pemadatan konteks sesi; dilanjutkan dari keadaan source dan database yang diperiksa ulang |
| Status Git | Lihat blok di bawah |
| Langkah berikutnya | Pemilik meninjau diff snapshot sebelum commit; `BE-HMD-02` dan `BE-HMD-03` |

```text
 M Areas/HealthServices/MasterData/Enums/ServiceUnitType.cs
 M Areas/HealthServices/MedicalRecordManagement/Enums/ClinicalDocumentKind.cs
 M Areas/HealthServices/MedicalRecordManagement/Services/ClinicalDocumentIntegrityService.cs
 M Migrations/ApplicationDbContextModelSnapshot.cs
 M Program.cs
 M Repositories/ApplicationDbContext.cs
 M docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md
 M docs/module-blueprints/hemodialisa/roadmap/backend-roadmap.md
 M docs/module-blueprints/hemodialisa/roadmap/requirement-traceability.md
?? Areas/HealthServices/HemodialysisManagement/
?? Migrations/20260922044002_AddHemodialysisManagement.Designer.cs
?? Migrations/20260922044002_AddHemodialysisManagement.cs
?? Repositories/Configurations/HealthServices/HemodialysisManagement/
?? docs/module-blueprints/hemodialisa/task/
?? docs/module-blueprints/rawat-inap/dokter-rawat-inap/testing/test-by-agy/
```

Folder `docs/module-blueprints/rawat-inap/dokter-rawat-inap/testing/test-by-agy/` bukan milik task
ini dan tidak disentuh.
