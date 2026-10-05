# Laporan Perubahan Backend — `BE-IGD-053`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-053` |
| Judul | Penjaga episode terbuka pada pintu encounter `Emergency` (realisasi `IGD-OQ-093`) |
| Slice | `S1` · `EPIC IGD-11` · `MVP-7` |
| Roadmap | `docs/module-blueprints/igd/roadmap/backend-roadmap.md` bagian R3.13 |
| Trace | `FR-IGD-069` (sisi backend), `071`, `072`, `073`, `074`; `IGD-DEC-139`, `144`, `145`, `146`, `084`, `138`, `135`; `AT-IGD-166`, `168`…`171`, `185` |
| Contract version | API `0.11.0` §8.1 nomor 1, 2, 7, 8, 9; §8.2 baris `POST`; §8.3.6. Validation `0.8.0` §10.1 aturan 2–7. Integration `0.4.0` §5.2 baris pertama, §5.3 dua baris pertama, §5.4. Permission/audit `0.5.0` §7.1, §7.2. Seluruhnya `approved` (`IGD-DEC-157`). Berkas kontrak kini berversi API `0.13.0` / validation `0.10.0`; bagian encounter-first di dalamnya **tidak berubah**, dan hash kelima berkas cocok dengan manifest bagian 0j/0j.1 saat task dimulai |
| Dependency | `BE-IGD-050` ✅, `BE-IGD-052` ✅, `BE-IGD-055` ✅ |
| Klasifikasi | `HEAVY` — skor 11: repository 0, berkas diperiksa 2 (> 20 source dan dokumen), berkas diubah 2 (9 source), logika bisnis 2 (kunci serentak + dua jalur pembuka episode), kontrak API 2 (penolakan `409` baru, ruas baru), database 2 (tabel baru + migration), keamanan/auth 1 (jejak audit override; nol izin baru), UI/workflow 0 |
| Task mode | `BACKEND` — wewenang tulis dari pemilik 1 Oktober 2026 (*"dahulukan dan selesaikan terlebih dahulu jalur pendaftaran–triage"*). Target tulis: source IGD, dua berkas Registrasi di bawah `IGD-DEC-135`, `ApplicationDbContext` (+1 `DbSet`), dan `docs/module-blueprints/igd/` (laporan, status roadmap, traceability). **Tanpa** wewenang `dotnet build` (dijalankan pemilik), `dotnet ef migrations add` (dibuat pemilik), eksekusi database, `Program.cs`, commit, atau push |
| Target tulis | `NewQuilvianSystemBackend` saja; frontend baca-saja |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `b9076c71` (branch `rizkiG`, sesudah merge integration 1 Oktober 2026); working tree bersih saat task dimulai |
| Tanggal | 1 Oktober 2026 |
| Status | ✅ **SELESAI 1 Oktober 2026 — atas penilaian pemilik.** Migration `20261001023520_AddEmergencyDuplicateEpisodeOverride` dibuat dan diterapkan ke dev oleh pemilik (isi diperiksa agent: satu `CreateTable`, nol operasi pada tabel lain); build pemilik berhasil (jumlah warning tidak dilaporkan); uji API S1–S12 `PASS` (dijalankan pemilik lewat Playwright; bukti mentah JSON diperiksa agent); penjaga `Down()` lulus uji empat tahap di basis data terpisah (agent). **Catatan:** alur loket dua langkah lewat layar — yang semula belum dijalankan — **terbukti pada uji layar `FE-IGD-038` U1** (tangkapan layar diperiksa agent: kunjungan `IGD-261001034358-B98C47` lahir dari loket dan tampil Menunggu Triage), sehingga kriteria 9 terpenuhi; kriteria 14 sebagian (jumlah warning). Tanpa UAT. *Tulisan agen penguji "LULUS PENUH (VERIFIED & CLOSED)" diluruskan di bagian 5.3. Sebelumnya: 🟡 pagi hari yang sama* |

### Backend Governance Preflight

| Butir | Hasil |
| --- | --- |
| Governance terbaca | `AGENTS.md` backend; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md` dan `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` (repo — isi kontrak sama dengan salinan suite); `rules/backend/` suite skill 1.17.1 (`TASK_RULES`, `TASK_CLASSIFICATION`, `API_RULES`, `DATABASE_RULES`, `REVIEW_RULES`, `REPORT_TEMPLATE`) |
| Area / Module | `HealthServices` / `EmergencyInstallationManagement`; titik sentuh `HealthServices` / `RegistrationManagement` di bawah izin remediasi teknis `IGD-DEC-135` |
| Owner / prefix registry | Emergency, prefix `Emg`, `ACTIVE / LEGACY` — entity baru `EmgDuplicateEpisodeOverride` memakai prefix terdaftar; nol blocker `QBE-MOD-002`/`003` |
| Keberlakuan | `NEW CODE`: `EmgDuplicateEpisodeOverride`, konfigurasinya, `EmergencyEpisodeRule.GuardEncounterRegistrationAsync`, dua pesan baru pada `EmergencyVisitService`. `TOUCHED LEGACY`: `PatientEncounterController.CreateEncounterCoreAsync`, `PatientEncounterDtos`, `EmergencyVisitController.Create` dan `GetActiveEpisode`, `EmergencyVisitService.CariEpisodeAktifAsync`, `EmergencyVisitDtos`, `ApplicationDbContext` |
| Branch | `rizkiG`, upstream `origin/rizkiG` |
| QBE yang berlaku | `QBE-ENT-001` (mewarisi `IdentityModel`), `QBE-NAM-001`/`002` (prefix `Emg`), `QBE-CFG-001` (configuration tersendiri: FK, unique index, check constraint), `QBE-MOD-001` (model di modul IGD), `QBE-SVC-001` (aturan penjaga tinggal di `EmergencyEpisodeRule`; controller Registrasi hanya memanggil), `QBE-TXN-001` (encounter + catatan override satu transaksi), `QBE-VAL-001`, `QBE-DTO-001`, `QBE-PERM-001` (metadata akses tidak berubah), `QBE-LOG-001` (jejak pelaku di tabel audit, bukan logger), `QBE-AUD-001` |
| Tidak berlaku | `QBE-CODE-*` (nol nomor bisnis baru), `QBE-PAGE-001`, `QBE-OPT-001`, `QBE-DEL-001` (catatan tambah-saja), `QBE-NAM-003`/`QBE-DB-001`/`002` (bukan legacy migration) |
| Checker QBE | `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict`, cakupan working tree: **9 berkas dinilai, 0 `VIOLATION`, 0 `REVIEW`, `PASS`** |

---

## 1. Masalah yang diperbaiki

Sebelum task ini, satu-satunya penjaga "satu pasien satu episode IGD" berada di `POST /emergency-visits`, yaitu
**sesudah** encounter pasien dibuat di modul Registrasi. Akibatnya ada dua celah yang dicatat `IGD-OQ-093`:

1. **Pendaftaran serentak.** Dua petugas loket yang menekan simpan pada detik yang sama sama-sama lolos, karena
   masing-masing memeriksa sebelum yang lain menyimpan.
2. **Klien tanpa pra-cek.** Layar atau pemanggil yang tidak menjalankan pra-cek `active-episode` tetap berhasil
   membuat encounter kedua.

Celah ketiga muncul dari desain encounter-first: pasien yang sudah didaftarkan tetapi belum ditriage hanya punya
encounter, belum punya kunjungan — dan penjaga lama hanya membaca kunjungan, sehingga pasien itu tidak terlihat.

*Contoh.* RAYYAN didaftarkan pukul 09.35 dan sedang menunggu triage. Pukul 09.40 petugas lain mendaftarkannya lagi.
Tanpa penjaga di pintu encounter, encounter kedua lahir dan RAYYAN muncul dua kali pada daftar Menunggu Triage.

Selain itu, encounter IGD ikut membuat antrean dokter bila unit IGD diset wajib antre, padahal urutan IGD ditentukan
kegawatan, bukan urutan datang (`IGD-DEC-144`).

---

## 2. Proses bisnis

**Tujuan.** Pasien beridentitas hanya boleh punya satu episode IGD terbuka; pendaftaran kedua yang memang sah tetap
bisa dilakukan, dengan alasan yang tercatat.

**Pelaku.** Petugas pendaftaran (loket, admisi, kiosk) dan sistem.

**Langkah pada `POST /patient-encounters` bertipe `Emergency`.**

1. Validasi yang sudah ada berjalan seperti biasa (pasien, unit, penjamin).
2. Transaksi Registrasi dibuka — transaksi yang sama seperti sebelumnya.
3. **Kunci per pasien** diambil. Permintaan lain untuk pasien yang sama menunggu di sini sampai transaksi pertama
   selesai.
4. **Rumus episode terbuka** dijalankan: kunjungan pasien yang belum selesai/batal, atau encounter gawat darurat lain
   yang belum berakhir.
5. Bila tidak ada episode terbuka → lanjut ke langkah 8. Alasan pendaftaran ganda yang ikut dikirim **diabaikan**.
6. Bila ada dan alasan kosong → **ditolak `409`**; tidak ada yang tersimpan.
7. Bila ada dan alasan terisi → bila lebih dari 500 karakter ditolak `400`; selain itu **catatan override** disiapkan
   (encounter baru, episode yang dilangkahi, alasan, pelaku dari token, waktu server).
8. Baru kunci penomoran diambil dan encounter disisipkan — **tanpa baris antrean**, apa pun pengaturan
   `IsQueueRequired` unit/klinik.
9. Encounter dan catatan override tersimpan pada satu penyimpanan; gagal salah satu, keduanya batal.

**Jalur lama `POST /emergency-visits`** (dipakai loket sampai `FE-IGD-036`): kini berjalan di dalam transaksi
eksplisit, mengambil kunci per pasien yang sama, dan memakai rumus yang sama dengan mengecualikan encounter miliknya
sendiri. Alasan tingkat kunjungan tetap berlaku di jalur ini.

**Pra-cek `GET /emergency-visits/active-episode`**: kini juga mengenali pasien yang sedang menunggu triage dan
mengembalikannya pada ruas `encounter`.

**Jalur tidak normal.**

| Keadaan | Yang terjadi |
| --- | --- |
| Dua pendaftaran serentak untuk pasien yang sama | Yang kedua menunggu kunci, lalu membaca encounter yang baru dibuat dan ditolak `409` |
| Penulisan catatan override gagal | Encounter tidak dibuat (satu transaksi) |
| Pasien punya encounter IGD lama tanpa kunjungan (kelas K3) | Terhitung episode terbuka (klausa A) → ditolak kecuali beralasan, atau encounter lamanya dibatalkan lebih dulu |
| Encounter bertipe rawat jalan atau rawat inap | Tidak dikunci, tidak diperiksa, antreannya tetap dibuat seperti semula |
| Pasien tanpa identitas di jalur lama (`PatientId` kosong) | Tidak dikunci dan tidak diperiksa — tidak ada pasien yang dapat dibandingkan |

*Contoh berangka.* Petugas mendaftarkan pasien yang kunjungan paginya `IGD-0012` (tiba 07.10) belum ditutup dokter.
Tanpa alasan: `409` — *"Pasien ini masih memiliki kunjungan IGD IGD-0012, tiba pukul 07.10 WIB tanggal 01-10-2026.
Buka kunjungan tersebut, jangan mendaftar ulang. Bila pendaftaran kedua memang sah, isi alasan pendaftaran ganda."*
Dengan alasan *"Pasien kembali 2 jam kemudian, kunjungan pagi belum ditutup dokter"*: encounter kedua lahir, dan
satu baris `EmgDuplicateEpisodeOverride` menunjuk kunjungan `IGD-0012`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Governance: `AGENTS.md`, `docs/engineering/*`, `rules/backend/*`.
- Blueprint: kartu `BE-IGD-053` (roadmap R3.13); `contracts/api-contract.md` §8.1–8.3.6; `validation-matrix.md` §10.1;
  `integration-contract.md` §5.2–5.4; `permission-audit-matrix.md` §7.2; `02-backend-architecture.md` §13.3–13.6;
  `IGD-DEC-139`, `144`, `145`, `146`.
- Source: `PatientEncounterController.cs` (`CreateEncounterCoreAsync` dan blok galatnya), `PatientEncounterDtos.cs`,
  `PatientEncounterNumberService.cs`, `EmergencyEpisodeRule.cs`, `EmergencyVisitService.cs`,
  `EmergencyVisitController.cs`, `EmergencyVisitDtos.cs`, `EmgEncounterReconciliationRun.cs` beserta konfigurasinya
  (pembanding terdekat), `EmgVisit.cs`, `EmgVisitConfiguration.cs`, `ApplicationDbContext.cs`, `IdentityModel.cs`,
  `AccJournalLineConfiguration.cs` (pola check constraint), `tooling/qbe/Invoke-QbeConformanceCheck.ps1`.
- Pergeseran source sejak audit R3.13 (`0d13f3a8` → `b9076c71`): `PatientEncounterController.cs` dan DTO-nya berubah
  oleh pekerjaan kiosk (penjamin perusahaan pada rute kiosk, konversi tanggal UTC) — **tidak** bersinggungan dengan
  cabang Emergency.
- Frontend (baca saja): `emergency-registration.utils.js` — penormal pra-cek sudah menerima `visit: null` saat
  `hasActiveEpisode` benar.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/EmergencyInstallationManagement/Models/EmgDuplicateEpisodeOverride.cs` (**baru**) | Entity tambah-saja mewarisi `IdentityModel`: `EncounterId`, `PatientId`, `OverriddenEncounterId?`, `OverriddenVisitId?`, `Reason` (500), `OverriddenByUserId`, `OverriddenAt`, lima navigasi |
| `Repositories/Configurations/HealthServices/EmergencyInstallationManagement/EmgDuplicateEpisodeOverrideConfiguration.cs` (**baru**) | Tabel `public."EmgDuplicateEpisodeOverride"`; unique index `EncounterId`; index `PatientId`; lima FK `Restrict`; check constraint `CK_EmgDuplicateEpisodeOverride_EpisodeYangDilangkahi` (salah satu penunjuk episode lama wajib terisi) |
| `Repositories/ApplicationDbContext.cs` | + `DbSet<EmgDuplicateEpisodeOverride> EmgDuplicateEpisodeOverrides`. Configuration ditemukan otomatis |
| `Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyEpisodeRule.cs` | + `GuardEncounterRegistrationAsync` (kunci → rumus → tolak atau siapkan catatan override; **tidak** menyimpan dan **tidak** membuka transaksi), record `HasilPenjagaPendaftaran`, `NormalizeOverrideReason`, konstanta batas 500 dan pesannya. `FindOpenEpisodeAsync` + parameter opsional `includePatient` (memuat nama pasien dalam kueri yang sama); **rumusnya tidak berubah** |
| `Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyVisitService.cs` | `CariEpisodeAktifAsync` **mendelegasikan** ke `FindOpenEpisodeAsync` dan kini mengembalikan `OpenEpisode?`; parameter kedua berganti dari pengecualian kunjungan (tidak pernah dipakai pemanggil mana pun) menjadi pengecualian encounter. + `PesanEpisodeTerbukaSaatPendaftaran` dan `PesanEncounterMenungguTriage` — kalimat validation §10.1 aturan 3 |
| `Areas/HealthServices/EmergencyInstallationManagement/Controllers/EmergencyVisitController.cs` | `Create`: transaksi eksplisit + kunci per pasien + rumus A+B dengan pengecualian encounter sendiri; pesan `409` memakai kalimat "Menunggu Triage" bila episodenya berupa encounter. `GetActiveEpisode`: mengisi ruas `encounter` |
| `Areas/HealthServices/EmergencyInstallationManagement/DTOs/EmergencyVisitDtos.cs` | + `EmergencyActiveEncounterSummary` (`Id`, `EncounterNumber`, `RegisteredAt`); `EmergencyActiveEpisodeResponse` + `Encounter` |
| `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs` — **berkas Registrasi** | `CreateEncounterCoreAsync`: `isQueueRequired` dipaksa salah untuk Emergency; pemanggilan penjaga di dalam transaksi **sebelum** alokasi nomor encounter; penolakan dikembalikan dengan kode dari penjaga. + `ProducesResponseType` `409` pada dua action pembuat. + satu `using` |
| `Areas/HealthServices/RegistrationManagement/DTOS/PatientEncounterDtos.cs` — **berkas Registrasi** | `PatientEncounterCreateRequest` + `DuplicateEpisodeOverrideReason` (`string?`, tanpa atribut — supaya tipe lain tidak tertolak oleh validasi model) |

**Nol baris komentar baru dalam bentuk apa pun** (`//`, `///`, `/* */`) — arahan pemilik, ditegaskan lagi 1 Oktober
2026. Suntingan atas dua komentar dokumentasi lama yang sempat dibuat **dikembalikan ke teks aslinya**, sehingga
nol baris komentar ditambahkan. Satu baris komentar lama di dalam blok `GetActiveEpisode` yang ditulis ulang ikut
hilang bersama blok lamanya. Perubahan itu hanya menyentuh komentar, jadi DLL hasil build pemilik tetap berlaku.
Dua komentar dokumentasi lama kini tidak tepat lagi (ringkasan `CariEpisodeAktifAsync` dan catatan celah
`IGD-OQ-093` pada `GetActiveEpisode`) — dicatat di bagian 7, tidak disunting.

**Tidak disentuh:** `Program.cs`, `EncounterIntakeService`, `InpEpisodeService`, berkas kontrak, frontend, migration.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Sesuai kontrak terkunci: `409` baru dan ruas `duplicateEpisodeOverrideReason` pada `POST /patient-encounters`; encounter Emergency tanpa antrean; `POST /emergency-visits` kini juga menolak untuk klausa A; `active-episode` + ruas `encounter`. Nol route baru |
| Database | **Satu tabel baru** `EmgDuplicateEpisodeOverride`. Migration `20261001023520_AddEmergencyDuplicateEpisodeOverride` **dibuat dan diterapkan ke dev oleh pemilik** 1 Oktober 2026; penjaga `Down()` disisipkan pemilik sesuai potongan §5.1. Nol kolom baru pada `RegPatientEncounter` |
| Keamanan/Auth | Nol izin baru: override menumpang `PatientEncounter : Create` (`IGD-DEC-145`). Alasan override **tidak** masuk custom logger — pesan log pembuatan encounter dan pesan galat basis data tidak memuat ruas itu; jejaknya hanya di tabel audit |

### 3.4 Keputusan pelaksanaan yang tidak tertulis pada kartu

| Keputusan | Alasan |
| --- | --- |
| Urutan langkah penjaga berada di `EmergencyEpisodeRule`, bukan di controller Registrasi | Integration §5.2 menyebut pintu encounter memanggil `EmergencyEpisodeRule`; berkas Registrasi hanya bertambah satu pemanggilan (`IGD-DEC-135` — remediasi sesempit mungkin) dan aturan IGD tetap di satu tempat (`QBE-SVC-001`) |
| Batas 500 karakter diperiksa **hanya** bila episode terbuka | API §8.2: ruas itu *hanya dibaca* bila tipe Emergency dan episode terbuka, serta diabaikan untuk selainnya. Atribut `[MaxLength]` pada DTO akan menolak tipe lain |
| Catatan override mengisi **satu** penunjuk: `OverriddenVisitId` bila episodenya kunjungan, `OverriddenEncounterId` bila encounter | Mengikuti kamus data §6 kolom demi kolom |
| `IsQueueRequired` encounter Emergency tersimpan `false` dan statusnya `Registered` | Variabel yang sama menentukan status awal, kolom encounter, pembuatan antrean, dan ruas respons (`:544`, `:608`, `:749` pada kartu) — memaksanya di satu titik menjamin keempatnya sepakat |
| Jalur lama: alasan tingkat kunjungan dengan episode berupa encounter menyimpan `DuplicateEpisodeOverrideOfVisitId` kosong | Kolom itu menunjuk kunjungan; untuk klausa A tidak ada kunjungan yang dapat ditunjuk |
| Format waktu pada pesan memakai `FormatWaktuLokal` yang sudah ada (`HH.mm WIB tanggal dd-MM-yyyy`) | Sama dengan pesan `BE-IGD-055`, supaya petugas membaca satu gaya |

---

## 4. Dokumentasi endpoint

#### Health Services / Registration Management / Patient Encounter

Base URL: `api/v1/health-services/registration-management/patient-encounters` — milik Registration Management

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/`, `/kiosk`, `/admin` | Membuat encounter. **Khusus Emergency:** kunci per pasien, rumus episode, override beralasan, tanpa antrean | `PatientEncounter : Create` (tidak berubah) |

Ruas request baru: `duplicateEpisodeOverrideReason` (`string?`, opsional, di-*trim*, maksimal 500, hanya dibaca
bila Emergency dan episode terbuka). Kode status baru: `409`.

#### Health Services / Emergency Installation Management / Emergency Visit

Base URL: `api/v1/health-services/emergency-installation-management/emergency-visits`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/active-episode?patientId=` | Pra-cek episode terbuka — kini klausa A atau B; ruas `encounter` terisi bila pasien sedang menunggu triage | `EmergencyVisit : Create` (tidak berubah) |
| `POST` | `/` | Jalur lama pembuatan kunjungan — kini transaksi + kunci + rumus A+B | `EmergencyVisit : Create` (tidak berubah) |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` (working tree) | 9 berkas, 0 `VIOLATION`, 0 `REVIEW`, `Final result: PASS` | `PASS` | Keluaran perintah, 1 Oktober 2026 |
| Review diff 7 berkas tracked + 2 berkas baru | Hanya perubahan task; akhiran baris tiap berkas dipertahankan; nol `//` baru | `PASS` | `git diff --stat`: 7 berkas, +211/−37 |
| `dotnet build` | Berhasil menurut pemilik. Artefak: `bin/…/QuilvianSystemBackend.dll` 09.36 (memuat tabel dan id migration baru), `obj/…/QuilvianSystemBackend.dll` 09.45 (juga memuat penjaga `Down()`). Jumlah warning tidak dilaporkan | `PASS` (pernyataan pemilik + artefak) | Cap waktu dan isi DLL diperiksa agent |
| `dotnet ef migrations add` + `database update` (dev) | Migration `20261001023520` dibuat dan diterapkan pemilik. `Up()`: satu `CreateTable`, lima FK `Restrict`, satu check constraint, lima index (satu unique). Snapshot +211/−92: 92 baris yang terhapus seluruhnya muncul kembali (dua blok `FinReceiptAllocation`/`FinReceiptDeduction` bertukar urutan); tambahan bersih 119 baris seluruhnya blok tabel baru | `PASS` | Berkas migration; `git diff` snapshot dihitung agent |
| Uji API S1–S12 | 12 dari 12 `PASS` menurut `s1_s12_test_results.json` (02.56.39–02.57.13 UTC) | `PASS` — S9 dengan catatan (bagian 5.3) | `QuilvianSystemFrontendDev/test-with-agy/igd/` — JSON, skrip `.mjs`, pembantu DB baca-saja |
| Uji `Down()` berpenjaga, basis data terpisah | Empat tahap lulus (bagian 5.4) | `PASS` | Dijalankan agent 1 Oktober 2026, PostgreSQL 16.15 lokal sekali pakai |

Uji manual: `PASS` untuk S1–S12 lewat API; alur loket lewat layar `PASS` pada `FE-IGD-038` U1 (laporan `FE-IGD-038` bagian 6.2).

### 5.1 Langkah pemilik

1. Build tanpa analyzer (lebih cepat): `dotnet build ./QuilvianSystemBackend.sln -p:RunAnalyzers=false`
2. Buat migration: `dotnet ef migrations add AddEmergencyDuplicateEpisodeOverride`. Isi `Up()` yang diharapkan:
   satu `CreateTable` `EmgDuplicateEpisodeOverride` beserta lima FK, tiga index FK biasa, satu unique index
   `EncounterId`, satu index `PatientId`, dan satu check constraint. **Nol operasi pada tabel lain** — bila ada,
   berhenti dan laporkan (snapshot tidak sinkron).
3. Sisipkan penjaga di baris pertama `Down()` (pola `BE-IGD-060`):

   ```csharp
   migrationBuilder.Sql("""
       DO $$
       BEGIN
           IF EXISTS (SELECT 1 FROM public."EmgDuplicateEpisodeOverride") THEN
               RAISE EXCEPTION 'Down BE-IGD-053 dihentikan: tabel EmgDuplicateEpisodeOverride berisi catatan pendaftaran ganda. Menghapus tabel ini menghilangkan jejak audit tanpa bekas.';
           END IF;
       END $$;
       """);
   ```

4. Terapkan ke dev: `dotnet ef database update`.

### 5.2 Skenario uji untuk pemilik

| # | Skenario | Hasil yang diharapkan | Kriteria |
| ---: | --- | --- | ---: |
| S1 | Daftarkan pasien tanpa episode terbuka (Emergency), unit IGD `IsQueueRequired = true` bila ada | `200`; satu encounter berstatus `Registered`; nol baris `TrxQueue` untuk encounter itu; `isQueueCreated = false` | 1 |
| S2 | Daftarkan lagi pasien S1 (masih menunggu triage) tanpa alasan | `409`; pesan memuat nomor encounter dan "Menunggu Triage"; jumlah encounter tetap | 2 |
| S3 | Pasien dengan kunjungan IGD berjalan didaftarkan tanpa alasan | `409`; pesan memuat nomor kunjungan dan jam tiba | 3 |
| S4 | Dua `POST /patient-encounters` paralel untuk satu pasien tanpa episode | Tepat satu `200` dan satu `409`; satu encounter terbuka | 4 |
| S5 | Ulangi S2 dengan `duplicateEpisodeOverrideReason` terisi | `200`; satu baris `EmgDuplicateEpisodeOverride`: alasan, pelaku token, waktu server, `OverriddenEncounterId` = encounter S1; tabel encounter tanpa kolom baru | 5 |
| S6 | Panggil `POST /patient-encounters` langsung tanpa pra-cek untuk pasien S1 | `409` | 6 |
| S7 | Pasien yang kunjungannya sudah `Completed` dan encounter-nya tertutup | `200` | 7 |
| S8 | Pendaftaran rawat jalan untuk pasien yang punya episode IGD terbuka | `200`; antrean tetap dibuat seperti semula | 8 |
| S9 | Loket (layar sekarang) mendaftarkan pasien baru sampai kunjungan lahir | Berhasil; kunjungan berstatus Menunggu Triage | 9 |
| S10 | `GET /active-episode` untuk pasien S1 | `hasActiveEpisode = true`, `visit = null`, `encounter` berisi nomor dan waktu daftar | 10 |
| S11 | S2 dengan alasan 501 karakter | `400` *"Alasan pendaftaran ganda maksimal 500 karakter."*; nol encounter baru | — |
| S12 | Rawat jalan dengan `duplicateEpisodeOverrideReason` 600 karakter | `200` — ruas diabaikan untuk tipe lain | 8 |

### 5.3 Hasil uji S1–S12 — bukti mentah yang diperiksa

Uji dijalankan pemilik lewat agen penguji (Playwright) terhadap dev. Ringkasan agen itu
(`testing/2026-10-01-laporan-uji-s1-s12-be-igd-053.md`) **bukan** bukti; yang dipakai adalah JSON hasil, skrip,
dan pembantu basis datanya.

| # | Hasil pada JSON | Penilaian |
| ---: | --- | --- |
| S1 | `200`; `ENC-RSMMC-00182`; status `Registered`; `IsQueueRequired = false`; nol `TrxQueue` | `PASS` |
| S2 | `409`; pesan memuat `ENC-RSMMC-00182` dan *"Menunggu Triage sejak 09.56 WIB tanggal 01-10-2026"*; jumlah encounter tetap 1 | `PASS` |
| S3 | `409`; pesan memuat `IGD-260917045149-BBFF77` dan jam tiba | `PASS` |
| S4 | Dua `fetch` lewat `Promise.all` → `[200, 409]`; satu encounter Emergency di basis data | `PASS` |
| S5 | `200`; satu baris override: `OverriddenEncounterId` = encounter S1, `OverriddenVisitId` kosong, alasan, pelaku token, `OverriddenAt` waktu server | `PASS` |
| S6 | `409` tanpa pra-cek | `PASS` |
| S7 | `200`; `ENC-RSMMC-00185` | `PASS` |
| S8 | `200`; antrean rawat jalan dibuat (1 baris) | `PASS` |
| S9 | `200`; kunjungan `IGD-261001025709-70F99F` berstatus `WaitingForTriage` | `PASS` **sebagai uji API jalur lama** — lihat catatan |
| S10 | `200`; `hasActiveEpisode = true`, `visit` kosong, `encounter` berisi nomor dan `registeredAt` | `PASS` |
| S11 | `400` *"Alasan pendaftaran ganda maksimal 500 karakter."*; jumlah encounter tetap | `PASS` |
| S12 | `200`; ruas diabaikan untuk rawat jalan | `PASS` |

**Yang diluruskan dari ringkasan agen penguji.**

| Klaim | Kenyataan |
| --- | --- |
| S9 "loket mendaftarkan pasien baru sampai kunjungan lahir" | Skrip memanggil `POST /emergency-visits` **langsung, tanpa encounter**. Itu membuktikan jalur lama berjalan di dalam transaksi dan kunci, tetapi **bukan** alur loket yang sebenarnya — `POST /patient-encounters` lalu `POST /emergency-visits` dengan encounter itu, lewat layar. Bagian itu diuji kemudian pada `FE-IGD-038` U1 dan lulus |
| "Uji `Down()` berpenjaga: PASS" | Yang diperiksa agen itu hanya keberadaan potongan SQL. Uji sebenarnya dijalankan terpisah — bagian 5.4 |
| Tiga tangkapan layar S1, S4, S5 | Ketiganya berkas yang **sama persis** (md5 identik) — halaman daftar triage, bukan bukti skenario. Buktinya adalah JSON |
| "IGD-OQ-093 resmi ditutup" | Celahnya terbukti hilang (S4, S6). Pencatatan pada decision log dikerjakan pass penyelarasan, sesuai DoD kartu |

Pembantu basis data (`db_helper.py`) hanya berisi `SELECT`; nol penulisan langsung ke basis data.

**Data uji yang tertinggal di dev** (dibersihkan pemilik lewat aksi pembatalan, bukan SQL): encounter IGD terbuka
`ENC-RSMMC-00182` dan encounter override-nya (Indra Gunawan), `ENC-RSMMC-00183` (Ikbal Yuliyanto),
`ENC-RSMMC-00185` (Dede Kurniawan); kunjungan `IGD-261001025709-70F99F` tanpa encounter (Andry Zainudin); dua
encounter rawat jalan `ENC-RSMMC-00186`, `00187`. Selama belum dibersihkan, pasien-pasien itu tertahan penjaga dan
tampil pada daftar Menunggu Triage.

### 5.4 Uji `Down()` berpenjaga di basis data terpisah

Dijalankan agent 1 Oktober 2026 pada kontainer `postgres:16` sekali pakai (PostgreSQL 16.15; dev memakai 15),
nol sentuhan ke dev. Karena backend sedang berjalan, `bin` masih memuat DLL 09.36 tanpa penjaga; uji memakai
**salinan** keluaran build di folder sementara yang ditimpa DLL `obj` 09.45 — `bin` tidak disentuh, `dotnet build`
tidak dijalankan. Seluruh rantai naik bersih ke basis data kosong: **270 migration, 759 tabel**.

| Tahap | Tindakan | Hasil |
| ---: | --- | --- |
| 0 | Sidik jari di head | 758 tabel lain; tabel target 18 kolom, 6 index, 7 constraint; riwayat 270 |
| 1 | `Down()` saat tabel **kosong** | Berhasil; tabel target hilang; sidik jari kolom, index, dan constraint 758 tabel lain **identik**; riwayat 269 |
| 2 | `Up()` lagi | Sidik jari tabel target **identik** dengan tahap 0; riwayat 270 |
| 3 | Sisip satu baris, lalu `Down()` | **Ditolak** — `P0001: Down BE-IGD-053 dihentikan: tabel EmgDuplicateEpisodeOverride berisi catatan pendaftaran ganda…`; tabel dan barisnya utuh; riwayat tetap 270 |
| 4 | Hapus baris, `Down()`, lalu `Up()` | Keduanya berhasil; sidik jari akhir identik dengan tahap 0 |

Sidik jari: `md5` atas daftar kolom (nama, tipe, nullability, panjang), definisi index, dan definisi constraint.

---

## 6. Acceptance criteria dan Definition of Done

| # | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Satu encounter Emergency, nol baris antrean | **Terpenuhi** | S1: `200`, `IsQueueRequired = false`, nol baris `TrxQueue` |
| 2 | Pasien Menunggu Triage didaftarkan lagi tanpa alasan → `409` | **Terpenuhi** | S2: `409` memuat nomor encounter dan "Menunggu Triage" |
| 3 | Klausa B → `409` dengan nomor kunjungan | **Terpenuhi** | S3: `409` memuat nomor kunjungan dan jam tiba |
| 4 | Dua pendaftaran paralel → satu `200`, satu `409` | **Terpenuhi** | S4: `[200, 409]`, satu encounter di basis data |
| 5 | Pendaftaran kedua beralasan → catatan override lengkap; tabel encounter tanpa kolom baru | **Terpenuhi** | S5: satu baris `EmgDuplicateEpisodeOverride` (episode yang dilangkahi, alasan, pelaku, waktu server); migration nol kolom pada `RegPatientEncounter` |
| 6 | Klien tanpa pra-cek → tetap `409` | **Terpenuhi** | S6 |
| 7 | Episode sudah berakhir → encounter dibuat | **Terpenuhi** | S7: `200`, `ENC-RSMMC-00185`. K1 hasil rekonsiliasi tidak diuji terpisah |
| 8 | Rawat jalan/rawat inap, `EncounterIntakeService`, `InpEpisodeService` tidak berubah | **Terpenuhi** | S8 (antrean rawat jalan tetap dibuat), S12 (ruas diabaikan); `git diff`: kedua service tidak tersentuh. Rawat inap tidak diuji lewat API |
| 9 | Jalur lama dalam transaksi eksplisit dengan kunci yang sama; loket tetap dapat mendaftar | **Terpenuhi** | Source: `EmergencyVisitController.Create`. S9 (API) `PASS`. Alur loket dua langkah lewat layar: `FE-IGD-038` U1 — tangkapan layar langkah Selesai memuat `IGD-261001034358-B98C47` beserta id encounter dan id kunjungan, dan daftar Triage Pasien menampilkan pasien itu berstatus Menunggu Triage |
| 10 | `active-episode` mengenali klausa A; ruas lama tidak berubah | **Terpenuhi** | S10 |
| 11 | Satu rumus episode untuk keempat pemakai; nol heuristik waktu | **Terpenuhi** | Keempatnya berakhir di `EmergencyEpisodeRule.FindOpenEpisodeAsync` |
| 12 | `Down()` berpenjaga diuji; snapshot hanya bertambah blok tabel override | **Terpenuhi** | Bagian 5.4 (empat tahap). Snapshot: tambahan bersih hanya blok `EmgDuplicateEpisodeOverride`; 92 baris lain hanya bertukar urutan, nol hilang |
| 13 | Laporan menyatakan celah `IGD-OQ-093` hilang, dengan bukti kriteria 4 dan 6 | **Terpenuhi** | Kedua celah `IGD-OQ-093` — pendaftaran serentak dan klien tanpa pra-cek — **hilang**: S4 `[200, 409]` dengan satu encounter di basis data, dan S6 `409` tanpa pra-cek (bagian 5.3) |
| 14 | Build 0 error, warning sama dengan baseline | **Sebagian** | Build berhasil menurut pemilik dan artefaknya ada; jumlah warning tidak dilaporkan |

DoD: laporan tracked ✅ (berkas ini). Realisasi `IGD-OQ-093` terbukti (kriteria 13); pencatatannya pada decision log menunggu pass penyelarasan. `FE-IGD-038` dirilis bersamaan — ✅ 1 Oktober 2026.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Jangan dirilis tanpa `FE-IGD-038`.** Loket hari ini mengirim alasan pendaftaran ganda hanya ke `POST /emergency-visits`. Sesudah task ini, pintu encounter menolak lebih dulu, sehingga pendaftaran ganda yang sah tidak dapat dilakukan dari loket sampai `FE-IGD-038` mengirim alasan ke `POST /patient-encounters` |
| Masalah yang diketahui | (1) Pasien dengan encounter IGD yatim (kelas K3, enam di dev) akan tertolak mendaftar ulang kecuali beralasan atau encounter lamanya dibatalkan — sesuai aturan 3 kartu. (2) Komentar dokumentasi lama pada `CariEpisodeAktifAsync` dan `GetActiveEpisode` sudah tidak tepat (masih menyebut hanya kunjungan, dan celah `IGD-OQ-093` sebagai belum ditutup); dibiarkan karena kode tidak boleh ditambahi baris komentar. (3) `IGD-UNK-06` — jumlah `TrxQueue` lama yang tertaut encounter Emergency belum diketahui; task ini tidak membersihkannya. (4) Data uji tertinggal di dev (bagian 5.3). (5) **Di luar lingkup, modul lain — bukan masalah skema:** berkas `Migrations/20260930144500_AddInpatientDoctorMedicalAssessmentOperationalColumns.cs` (rawat inap, commit `ec2f70af`) tidak punya atribut `[Migration]` maupun Designer, sehingga EF tidak mengenalinya. **Isinya sudah diterapkan lewat jalur lain:** ke-47 kolom `TrxPatientAssessment` di berkas itu seluruhnya juga ada di `20260930082632_UpdatePendingModelChanges` (commit yang sama, beratribut), dan migration itu berstatus diterapkan di dev (`dotnet ef migrations list --no-build`, 1 Oktober 2026: nol `Pending`). Berkas `144500` hanyalah salinan mati; bila kelak diberi atribut, ia akan gagal karena kolomnya sudah ada. *Catatan versi awal laporan ini — "tidak ikut diterapkan" — benar untuk berkasnya, tetapi menyesatkan untuk skemanya* |
| Risiko tersisa | Pintu masuk pasien: penjaga yang salah menolak pasien gawat. Urutan rilis R3.13.5 (rekonsiliasi K1 → `GET /preview` `expectedCount` = 0 → rilis bersama `FE-IGD-038`) tidak boleh dilewati |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | 7 berkas source berubah, 2 berkas source baru, migration + Designer + snapshot (pemilik), laporan ini, laporan uji, dan status pada roadmap/traceability. Tanpa stage, commit, atau push oleh agent |
| Langkah berikutnya | `BE-IGD-059` → `FE-IGD-036` → `FE-IGD-039`, `040` |
