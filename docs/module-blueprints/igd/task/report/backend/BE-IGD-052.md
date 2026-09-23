# Laporan Perubahan Backend — `BE-IGD-052`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-052` |
| Judul | Rekonsiliasi encounter `Emergency` historis (*Historical Emergency Encounter Reconciliation*) |
| Slice | `S6` · `EPIC IGD-11` · `MVP-7` |
| Roadmap | [backend-roadmap.md](../../../roadmap/backend-roadmap.md) bagian R3.13 |
| Trace | `FR-IGD-084`; `AT-IGD-182`, `AT-IGD-183`; `IGD-DEC-148` (bentuk endpoint admin, kelas K1–K4), `IGD-DEC-162` (tanpa layar), `IGD-DEC-138` (larangan pembersihan tanpa audit), `IGD-DEC-136` (pelaku tidak dikarang), `IGD-DEC-157` (kontrak disetujui) |
| Contract version | API **`0.11.0`** §8.4; validation **`0.8.0`** §10.6; state **`0.5.0`** §8.4; integration **`0.4.0`** §5.2 baris rekonsiliasi; permission/audit **`0.5.0`** §7.1, §7.2. Seluruh bagian encounter-first **`approved`** (`IGD-DEC-157`), terkunci hash. Task ini **tidak** mengubah berkas kontrak |
| Dependency | `BE-IGD-051` ✅ (22 September 2026), `BE-IGD-055` ✅ (23 September 2026) |
| Klasifikasi | `HEAVY` — skor 11: cakupan repository 0, berkas diperiksa 2, berkas diubah 2 (9 berkas baru + `ApplicationDbContext`), logika bisnis 2 (klasifikasi lima kelas, penjaga data basi, pembalikan bersyarat), kontrak API 2 (grup endpoint baru), database 2 (dua tabel baru + migration), keamanan/auth 1 (resource izin baru), UI/workflow 0 (tanpa layar, `IGD-DEC-162`) |
| Task mode | `BACKEND` — go-ahead pemilik 23 September 2026. Target tulis: source IGD, konfigurasi EF, `ApplicationDbContext`, dan `docs/module-blueprints/igd/`. **Tidak** ada wewenang `dotnet build`, membuat/menjalankan migration, menulis basis data, atau operasi Git; **tidak** menyentuh `Program.cs`, berkas Registrasi, berkas kontrak, frontend |
| Target tulis | `NewQuilvianSystemBackend` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `dce1f138` pada branch `rizkiG` (sesudah merge `62c8360a` dari `QuilvianIntegrationBackend`). Working tree sudah memuat penandaan `BE-IGD-055` yang belum di-commit; perubahan task ini ditambahkan di atasnya |
| Tanggal | 23 September 2026 |
| Status | 🟡 **SEBAGIAN — 23 September 2026.** Implementation Complete: sembilan berkas baru + dua `DbSet`. **Belum:** migration `AddEmergencyEncounterReconciliation` (kriteria 7, milik pemilik — belum dibuat), `dotnet build` (kriteria 9, milik pemilik — `NOT RUN`), uji API pembalikan (kriteria 5), dan **angka kueri D** yang dibutuhkan kriteria 1. **Endpoint belum dapat dipanggil sebelum migration diterapkan** |

### Backend Governance Preflight

| Butir | Hasil |
| --- | --- |
| Governance terbaca | `AGENTS.md` dan `CLAUDE.md` backend; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md` dan `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `rules/backend/` suite skill 1.17.1; `rules/rule-output/` |
| Selisih governance | `AGENTS.md`, `CLAUDE.md`, dan kontrak engineering **tidak berubah** oleh merge integration (`git diff 48a78703..HEAD`). Registry **berubah** — baris modul lain bertambah; baris `Emg` tetap ada dan tidak berubah. Kontrak engineering repo identik dengan salinan suite |
| Area / Module | `HealthServices` / `EmergencyInstallationManagement` |
| Owner / prefix registry | Emergency, prefix `Emg`, `ACTIVE / LEGACY` — dua entity baru memakai prefix terdaftar, nol blocker `QBE-MOD-002`/`QBE-MOD-003` |
| Keberlakuan | `NEW CODE` seluruhnya: dua model, dua configuration, dua enum, DTO, service static, controller. `TOUCHED LEGACY`: `ApplicationDbContext` (+2 `DbSet`) |
| QBE ID yang berlaku | `QBE-ENT-001` (kedua entity mewarisi `IdentityModel`), `QBE-ENT-002`, `QBE-ENT-003` (nol kolom presentasi), `QBE-NAM-001`/`002` (nama `Emg*`, nol `Trx*`), `QBE-CFG-001` (dua `IEntityTypeConfiguration` dengan key, index, relasi), `QBE-MOD-001`, `QBE-SVC-001` (controller nol akses `DbContext` langsung — seluruh kueri dan orkestrasi di service static), `QBE-API-001`, `QBE-PERM-001` (resource baru `EmergencyEncounterReconciliation` dengan pasangan atribut per method), `QBE-VAL-001`, `QBE-TXN-001` (satu transaksi per run, kunci advisory), `QBE-DTO-001`, `QBE-ENUM-001`, `QBE-CODE-001`/`003`/`004` (`RunNumber` dari `EmergencyDocumentNumberService`, nol Count+1, unique index), `QBE-LOG-001` (log eksekusi dan pembalikan memuat jumlah baris; aktor dari klaim), `QBE-AUD-001` (jejak nilai sebelum/sesudah di tabel, terpisah dari log aplikasi) |
| Tidak berlaku | `QBE-NAM-003`, `QBE-DB-001`/`002` (bukan `LEGACY MIGRATION`); `QBE-DEL-001` (nol jalur hapus); `QBE-OPT-001` (nol feed options — tanpa layar) |
| Branch | `rizkiG`, sejajar `origin/rizkiG` pada `dce1f138`. Tidak di-commit, tidak di-push |
| Hardcode role | Tidak ada. Kewenangan sepenuhnya lewat `[AccessPermission]`; nol `IsInRole`, nama peran, atau `UserType` |
| Endpoint standard | Transaksi, arketipe **proses admin ber-run**: `GET /preview` baca-saja, `POST /runs` aksi bernama, `POST /runs/{id}/reverse` aksi pembalikan. Nol `PUT`, nol `DELETE`, nol `PATCH /{id}/status` generik — sesuai `transaction-endpoint-standard` |

---

## 1. Masalah yang diperbaiki

Sampai `BE-IGD-051` dirilis, modul IGD tidak pernah menutup encounter. Setiap kunjungan IGD yang selesai atau
dibatalkan meninggalkan encounter-nya berstatus "Terdaftar" **selamanya**. Data lama itu tidak hilang begitu
`BE-IGD-051` aktif — ia menumpuk sebagai encounter yang terlihat "masih berjalan" padahal pasiennya sudah lama pulang.

Akibatnya langsung terasa pada task berikutnya: penjaga pendaftaran ganda (`BE-IGD-053`) membaca "pasien punya
encounter IGD terbuka" sebagai alasan menolak pendaftaran baru. Bila encounter lama tidak dibereskan lebih dulu,
pasien yang kunjungan lamanya **sudah selesai tahun lalu** akan ikut tertolak saat datang kembali.

Task ini memberi admin data satu pintu untuk membereskannya: melihat berapa banyak dan kelasnya apa, menutup
**hanya** yang buktinya pasti, dan membalik seluruh tindakan itu bila ternyata keliru.

*Contoh.* Encounter `ENC-RSMMC-00042` dibuat 3 Maret, kunjungan IGD-nya diselesaikan 3 Maret pukul 16.20, tetapi
encounter-nya masih `Registered` sampai hari ini. Rekonsiliasi menutupnya menjadi `Completed` dengan `CompletedAt`
16.20 — diambil dari kunjungan, bukan dari jam hari ini.

---

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Encounter IGD lama yang tertinggal terbuka ditutup sesuai bukti kunjungannya, tercatat, dan dapat dibalik |
| Pelaku | Admin data yang memegang `EmergencyEncounterReconciliation : Process`. Peran klinis **tidak** diberi hak ini (permission §7.1) |
| Pemicu | Dijalankan manual lewat Swagger di Development atau HTTP client bertoken (`IGD-DEC-162` — **tanpa layar**) |
| Kapan | Sekali per lingkungan, **sesudah** `BE-IGD-051` aktif di lingkungan itu dan **sebelum** `BE-IGD-053` dirilis |

**Lima kelas** (`IGD-DEC-148`). Dasar hitungnya: encounter yang belum berakhir menurut rumus lima tanda
`EmergencyEpisodeRule`, tidak terhapus.

| Kelas | Isi | Ditulis? |
| --- | --- | :-: |
| K1 | Encounter `Emergency`, kunjungannya `Completed`/`Cancelled` | **Ya** |
| K1-Outpatient | Sama, tetapi encounter bertipe `Outpatient` — kunjungan IGD lama masa transisi | **Ya** |
| K2 | Kunjungannya masih berjalan | Tidak — keadaan normal |
| K3 | Encounter `Emergency` tanpa kunjungan sama sekali | Tidak — dilaporkan sebagai daftar kerja petugas pendaftaran |
| K4 | Kunjungan satu-satunya sudah dihapus lunak | Tidak — dilaporkan |

**Langkah berurutan.**

1. **Pratinjau** (`GET /preview`) menghitung kelima kelas dan menampilkan daftar baris K1/K1-Outpatient berhalaman,
   lengkap dengan nilai yang **akan** ditulis (`statusAfter`, `completedAtAfter`). Tidak menulis apa pun.
2. Admin membaca `expectedCount` dari pratinjau — jumlah K1 + K1-Outpatient.
3. **Eksekusi** (`POST /runs`) dengan alasan dan `expectedCount`. Di dalam satu transaksi: kunci proses diambil,
   kelima kelas dihitung ulang, daftar calon dibaca ulang. Bila jumlahnya **tidak sama** dengan `expectedCount` →
   `409` dan tidak ada satu baris pun berubah.
4. Setiap baris dievaluasi sendiri — **tidak ada satu pernyataan update massal**. Status encounter mengikuti
   kunjungannya: kunjungan `Completed` → encounter `Completed`; kunjungan `Cancelled` → encounter `Cancelled`.
   `CompletedAt` **hanya** dari `EmgVisit.VisitCompletedAt`; bila kunjungan tidak menyimpannya, kolom itu dibiarkan
   kosong. Nilai sebelum dan sesudah disimpan per baris.
5. **Pembalikan** (`POST /runs/{id}/reverse`) mengembalikan nilai sebelum, tetapi **hanya** untuk baris yang
   encounter-nya masih persis bernilai hasil run. Baris yang sudah berubah sesudahnya dilewati, dan alasan
   dilewatinya disimpan pada baris itu. Run yang sudah dibalik tidak dapat dibalik lagi.

*Contoh berangka.* Pratinjau: K1 = 42, K1-Outpatient = 3, K2 = 5, K3 = 7, K4 = 1 → `expectedCount` = 45. Sementara
admin mengetik alasan, satu kunjungan lain diselesaikan perawat sehingga K1 menjadi 43. Eksekusi dengan
`expectedCount = 45` ditolak `409` *"Data berubah sejak pratinjau; muat ulang pratinjau."* Admin memuat ulang,
mengirim `expectedCount = 46`, dan run tercatat dengan 46 baris.

**Jalur tidak normal.** Alasan kosong atau lebih dari 500 karakter → `400`. Run tidak ada → `404`. Run sudah
dibalik → `409`. Bila satu baris calon ternyata sudah tidak layak saat ditulis (misalnya encounter-nya baru saja
ditutup jalur lain), seluruh eksekusi dibatalkan `409` — bukan dilewati diam-diam, supaya jumlah yang ditulis
selalu sama persis dengan `expectedCount`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Kelompok | Berkas |
| --- | --- |
| Kontrak | `api-contract.md` §8.4, `validation-matrix.md` §10.6, `state-transition-matrix.md` §8.4, `integration-contract.md` §5.2, `permission-audit-matrix.md` §7.1–7.2, `02-backend-architecture.md` §13.3, §13.6 |
| Source rujukan | `EmergencyEpisodeRule.cs` (rumus `EncounterNotEnded`), `EmergencyVisitService.cs` (pola `Hasil<T>`, kunci advisory, penomoran), `EmergencyDepartureService.cs`, `EmergencyDepartureController.cs` (pola `[AccessController]`), `EmergencyDocumentNumberService.cs`, `EmgVisitConfiguration.cs`, `RegPatientEncounter.cs`, `IdentityModel.cs`, `ApplicationUser.cs`, `PagedResult.cs`, `ApiResponse.cs`, `AccessMenuSeeder.cs`, `Program.cs` (baca saja — memastikan `EmergencyDocumentNumberService` sudah terdaftar) |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `…/Enums/EmergencyReconciliationClass.cs` | **Baru** — `K1 = 1`, `K1Outpatient = 2`, `K2 = 3`, `K3 = 4`, `K4 = 5` |
| `…/Enums/EmergencyReconciliationRunStatus.cs` | **Baru** — `Executed = 1`, `Reversed = 2` (state §8.4) |
| `…/Models/EmgEncounterReconciliationRun.cs` | **Baru** — kepala run: nomor, status, alasan, lima jumlah kelas, pelaku dan waktu eksekusi serta pembalikan |
| `…/Models/EmgEncounterReconciliationItem.cs` | **Baru** — satu baris encounter yang ditulis, beserta nilai **sebelum** dan **sesudah**, penanda dibalik, dan alasan dilewati |
| `Repositories/Configurations/…/EmgEncounterReconciliationRunConfiguration.cs` | **Baru** — unique `RunNumber`, index `ExecutedAt`, FK pelaku `Restrict`, relasi item `Cascade` |
| `Repositories/Configurations/…/EmgEncounterReconciliationItemConfiguration.cs` | **Baru** — unique `(RunId, EncounterId)`, index `EncounterId`, FK encounter dan kunjungan `Restrict` |
| `…/DTOs/EmergencyEncounterReconciliationDtos.cs` | **Baru** — pratinjau, baris pratinjau, permintaan eksekusi, permintaan pembalikan, response run, response baris |
| `…/Services/EmergencyEncounterReconciliation.cs` | **Baru** (static, tanpa DI) — hitung kelas, pratinjau, eksekusi, pembalikan, riwayat, dan pemetaan response |
| `…/Controllers/EmergencyEncounterReconciliationController.cs` | **Baru** — lima action; `[AccessController]` resource `EmergencyEncounterReconciliation`, `SortOrder = 10` |
| `Repositories/ApplicationDbContext.cs` | + dua `DbSet` pada region transaksi IGD |

Nol perubahan `Program.cs` (service static, dan `EmergencyDocumentNumberService` sudah terdaftar di `:515`), nol
`Migrations/`, nol berkas Registrasi, nol berkas kontrak, nol frontend. Kode ditulis **tanpa baris komentar**.

**Selisih terhadap kartu dan kontrak:**

| No | Selisih | Alasan |
| ---: | --- | --- |
| 1 | `[Tags("Health Services / Emergency Installation Management / Emergency Encounter Reconciliation")]`, bukan `[Tags("Emergency Encounter Reconciliation")]` seperti tertulis di kontrak | Seluruh controller IGD memakai bentuk panjang; kontrak menuliskan tag dalam bentuk pendek untuk §8.3 juga, padahal source yang sudah ada memakai bentuk panjang |
| 2 | Eksekusi dan pembalikan mengambil kunci advisory `EMG_ENCOUNTER_RECONCILIATION` | Tidak diminta kontrak. Tanpa ini dua admin yang menekan eksekusi bersamaan sama-sama lolos pemeriksaan `expectedCount` dan menulis dua run untuk baris yang sama |
| 3 | `expectedCount` kosong → `400` dengan pesan sendiri | Kontrak menyebut ruas ini wajib tetapi tidak menetapkan pesannya |
| 4 | Pelaku tidak terbaca dari token → `400` dengan pesan sendiri | `ExecutedByUserId` adalah FK wajib; menyimpan GUID kosong melanggar relasi dan memalsukan pelaku (`IGD-DEC-136`) |
| 5 | Satu baris calon yang ternyata sudah tidak layak saat ditulis → seluruh eksekusi dibatalkan `409`, bukan dilewati | Menjaga janji acceptance 2: jumlah yang ditulis **sama persis** dengan `expectedCount` |
| 6 | K2 dan K4 dihitung untuk encounter `Emergency` maupun `Outpatient` yang punya kunjungan IGD; K3 hanya `Emergency` | Kontrak menyebut kelas tanpa kualifikasi tipe kecuali K1-Outpatient. `Outpatient` tanpa kunjungan IGD adalah rawat jalan biasa dan tidak boleh ikut terhitung |
| 7 | `RunNumber` dibentuk `EmergencyDocumentNumberService` dengan prefix `REKIGD` | Arsitektur menyebut "`DocumentNumberService`"; yang ada di modul IGD adalah service ini, dan ia sudah dipakai nomor kunjungan |
| 8 | `countReversed` dan `countSkipped` bertipe nullable dan hanya terisi bila baris ikut dimuat | Endpoint daftar run sengaja tidak memuat seluruh barisnya; mengisi `0` akan terbaca sebagai fakta, padahal artinya "belum dihitung" |
| 9 | DTO baris pratinjau (`…PreviewRowResponse`) ditambahkan | Arsitektur §13.4 menyebut lima tipe DTO; daftar berhalaman pada `GET /preview` menuntut satu tipe baris |
| 10 | `IsActive` tidak ada pada kedua tabel | `IdentityModel` tidak memuatnya, dan arsitektur §13.6 tidak memintanya |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif.** Satu grup baru dengan lima endpoint. Nol endpoint lama berubah |
| Database | **Dua tabel baru** (`EmgEncounterReconciliationRun`, `EmgEncounterReconciliationItem`). Migration `AddEmergencyEncounterReconciliation` **belum dibuat** — milik pemilik, dibuat **di atas** `20260923021224`. Nol kolom baru pada `RegPatientEncounter`. Eksekusi menulis kolom encounter hanya dari daftar tertutup integration §5.2, dan hanya dua di antaranya: `EncounterStatus` dan `CompletedAt` (+ `UpdateBy`/`UpdateDateTime`) |
| Keamanan/Auth | **Resource izin baru** `EmergencyEncounterReconciliation` dengan tiga aksi: `Read`, `Process`, `Reverse`. Ketiganya muncul di layar Akses Role dan harus diberikan admin; rekomendasi permission §7.1: `Process` dan `Reverse` hanya untuk peran admin data, terpisah dari peran klinis. Alasan run **tidak** ditulis ke log aplikasi (permission §7.2) |

---

## 4. Dokumentasi endpoint

#### Health Services / Emergency Installation Management / Emergency Encounter Reconciliation

Base URL: `api/v1/health-services/emergency-installation-management/emergency-encounter-reconciliations`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/preview` | Jumlah kelima kelas + daftar K1/K1-Outpatient berhalaman; tidak menulis | `EmergencyEncounterReconciliation : Read` |
| `POST` | `/runs` | Menutup baris K1 dan K1-Outpatient | `EmergencyEncounterReconciliation : Process` |
| `GET` | `/runs` | Riwayat run berhalaman | `EmergencyEncounterReconciliation : Read` |
| `GET` | `/runs/{id}` | Satu run beserta seluruh barisnya | `EmergencyEncounterReconciliation : Read` |
| `POST` | `/runs/{id}/reverse` | Membalik satu run | `EmergencyEncounterReconciliation : Reverse` |

**Request `POST /runs`:** `{ "reason": "Penutupan encounter IGD sebelum BE-IGD-051", "expectedCount": 45 }`

**Request `POST /runs/{id}/reverse`:** `{ "reason": "Salah rentang; dikembalikan" }`

| Kode | Keadaan | Pesan (validation §10.6) |
| --- | --- | --- |
| `200` | Pratinjau, riwayat, detail, pembalikan berhasil | — |
| `201` | Run tercatat | *"Rekonsiliasi {nomor} tercatat; {n} encounter ditutup."* |
| `400` | Alasan kosong atau > 500 karakter | *"Alasan rekonsiliasi wajib diisi (maksimal 500 karakter)."* / *"Alasan pembalikan wajib diisi (maksimal 500 karakter)."* |
| `409` | Jumlah berubah sejak pratinjau | *"Data berubah sejak pratinjau; muat ulang pratinjau."* |
| `409` | Run sudah dibalik | *"Run ini sudah dibalik."* |
| `404` | Run tidak ada | *"Run rekonsiliasi tidak ditemukan."* |
| `401` / `403` | Tanpa token / tanpa hak admin rekonsiliasi | Penanganan standar |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Pembacaan diff dan scope | 9 berkas baru + `ApplicationDbContext` (+2 baris); nol `Program.cs`, `Migrations/`, Registrasi, kontrak, frontend | `PASS` | `git status --short` bagian 7 |
| Nol baris komentar baru | `grep -c "^\s*//"` = **0** pada kesembilan berkas baru | `PASS` | Perintah `grep` |
| Pemetaan kontrak | Kelima endpoint, lima kelas, empat pesan penolakan §10.6, dan enum status run dipetakan satu per satu ke source | `PASS` | Pembacaan silang kontrak dan source |
| Pemeriksaan izin | `[AccessController(ControllerName = "EmergencyEncounterReconciliation")]`; argumen pertama `[AccessPermission]` sama persis; argumen kedua sama persis dengan `[AccessAction]` pada method yang sama (`Read`, `Process`, `Reverse`) | `PASS` | Pembacaan source |
| Pemeriksaan rahasia | Nol credential, token, atau connection string pada source dan laporan | `PASS` | Pembacaan diff |
| `dotnet build` | Belum — milik pemilik | `NOT RUN` | Larangan task |
| Migration `AddEmergencyEncounterReconciliation` | Belum dibuat — milik pemilik | `NOT RUN` | Larangan task |
| Uji API pratinjau, eksekusi, pembalikan | Belum — butuh migration dan build | `NOT RUN` | — |
| Angka kueri D | Belum ada dari pemilik | `NOT RUN` | Kriteria 1 |

Uji manual: `REQUIRED` — skenario 5.3, sesudah migration diterapkan dan build berhasil.

**Tidak dijalankan:** `dotnet build`, `dotnet ef`, kueri basis data, dan aplikasi — seluruhnya di luar wewenang
task ini. Nol proyek test backend sejak 11 September 2026.

### 5.1 Perintah build untuk pemilik

```bash
dotnet build ./QuilvianSystemBackend.sln -p:RunAnalyzers=false
```

### 5.2 Migration untuk pemilik (`AddEmergencyEncounterReconciliation`)

```bash
dotnet ef migrations add AddEmergencyEncounterReconciliation --no-build
```

**Isi `Up()` yang diharapkan — tidak lebih:** dua `CreateTable` (`EmgEncounterReconciliationRun`,
`EmgEncounterReconciliationItem`) beserta FK-nya, dan index bawaan: unique `RunNumber`, `ExecutedAt`,
unique `(RunId, EncounterId)`, `EncounterId`, ditambah index FK konvensi EF untuk `ExecutedByUserId`,
`ReversedByUserId`, dan `EmergencyVisitId`. Bila muncul operasi milik modul lain, **berhenti** dan kabari agent.

**Penjaga `Down()`** — sisipkan di baris pertama `Down()`, pola `BE-IGD-048` dan `BE-IGD-055`:

```csharp
migrationBuilder.Sql("""
    DO $$
    BEGIN
        IF EXISTS (SELECT 1 FROM public."EmgEncounterReconciliationRun") THEN
            RAISE EXCEPTION 'Down BE-IGD-052 dihentikan: ada run rekonsiliasi yang sudah tercatat. Menghapus tabel ini menghilangkan jejak encounter mana yang ditutup beserta nilai sebelumnya, sehingga run tidak dapat dibalik lagi.';
        END IF;
    END $$;
    """);
```

Terapkan: `dotnet ef database update --no-build`.

### 5.3 Skenario uji untuk pemilik

Token yang memegang ketiga hak akses (`Read`, `Process`, `Reverse`) — hak ini **baru**, jadi harus diberikan lebih
dulu lewat layar Pengaturan → Manajemen Role → Akses Role sesudah aplikasi dijalankan sekali.

| # | Langkah | Hasil yang diharapkan | Kriteria |
| --- | --- | --- | --- |
| R1 | `GET /preview` | `200`; lima jumlah kelas; `expectedCount` = K1 + K1-Outpatient; daftar baris memuat `statusAfter` dan `completedAtAfter`. Bandingkan kelima angka dengan kueri D | 1 |
| R2 | Hitung baris `EmgVisit` dan `RegPatientEncounter` sebelum dan sesudah `GET /preview` | Tidak ada yang berubah — pratinjau tidak menulis | 1 |
| R3 | `POST /runs` dengan `expectedCount` sengaja salah (misalnya `expectedCount + 1`) | `409` *"Data berubah sejak pratinjau; muat ulang pratinjau."*; nol baris berubah | 2 |
| R4 | `POST /runs` dengan `expectedCount` benar | `201`; jumlah encounter yang berubah = `expectedCount`; hanya K1/K1-Outpatient | 2 |
| R5 | Periksa beberapa baris hasil R4 | `EncounterStatus` mengikuti status kunjungan; `CompletedAt` sama dengan `VisitCompletedAt` kunjungan, dan tetap kosong bila kunjungan tidak menyimpannya | 4 |
| R6 | `GET /runs/{id}` | Setiap baris memuat `statusBefore`, `statusAfter`, `completedAtBefore`, `completedAtAfter` | 6 |
| R7 | `POST /runs/{id}/reverse` tanpa `reason` | `400` *"Alasan pembalikan wajib diisi (maksimal 500 karakter)."* | 5 |
| R8 | Ubah satu encounter hasil run secara sah (misalnya lewat aksi IGD), lalu `POST /runs/{id}/reverse` | `200`; baris yang tidak berubah dikembalikan; baris yang sudah berubah **dilewati** dengan `reverseSkipReason` terisi | 5 |
| R9 | `POST /runs/{id}/reverse` sekali lagi | `409` *"Run ini sudah dibalik."* | 5 |
| R10 | Token tanpa `Process` | `403` | — |

---

## 6. Acceptance criteria dan Definition of Done

| No | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Pratinjau menghasilkan jumlah per kelas yang sama dengan kueri D; nol baris berubah | Belum terpenuhi — source ada | `EmergencyEncounterReconciliation.HitungKelasAsync` (lima `CountAsync` terpisah, nol penulisan); **menunggu angka kueri D** dan uji R1/R2 |
| 2 | Eksekusi dengan `expectedCount` basi → `409`; eksekusi benar → hanya K1/K1-Outpatient berubah sejumlah `expectedCount` | Belum terpenuhi — source ada | Pemeriksaan di dalam transaksi pada `ExecuteAsync`; uji R3/R4 `NOT RUN` |
| 3 | Nol pernyataan update massal; setiap baris dievaluasi ulang di dalam transaksi run | **Terpenuhi — terbukti agent** | `ExecuteAsync` memuat tiap encounter satu per satu dan memeriksa ulang `IsEncounterEnded` sebelum menulis; nol `ExecuteUpdate`/`ExecuteDelete`/SQL massal pada berkas baru |
| 4 | `CompletedAt` hanya dari `EmgVisit.VisitCompletedAt`; bila kosong tetap kosong; nol pelaku dikarang | **Terpenuhi — terbukti agent** (bagian source) | Nilai diambil dari `baris.VisitCompletedAt`; untuk kunjungan `Cancelled` kolom tidak disentuh; pelaku selalu dari token dan permintaan ditolak bila tidak terbaca. Contoh baris menunggu uji R5 |
| 5 | Pembalikan mengembalikan nilai sebelum; baris yang sudah berubah dilewati dan dilaporkan; run yang sudah dibalik → `409` | Belum terpenuhi — source ada | `ReverseAsync`; uji R7–R9 `NOT RUN` |
| 6 | Nilai sebelum/sesudah tercatat per baris; alasan tidak masuk custom logger | **Terpenuhi — terbukti agent** | `EmgEncounterReconciliationItem` menyimpan empat nilai; payload `LoggerService` hanya memuat nomor run dan jumlah — nol `Reason` |
| 7 | `Down()` berpenjaga diuji di basis data terpisah; snapshot hanya bertambah blok dua tabel ini | Belum terpenuhi — **milik pemilik** | Bagian 5.2 |
| 8 | Nol baris `Program.cs`; nol kolom baru `RegPatientEncounter` | **Terpenuhi — terbukti agent** | `git status --short`: `Program.cs` tidak tersentuh; nol perubahan model Registrasi |
| 9 | Build 0 error, warning sama dengan baseline | Belum terpenuhi — **milik pemilik** | Bagian 5.1 |

**DoD.** Kriteria 3, 4 (sisi source), 6, dan 8 terbukti; kriteria 1, 2, 5, 7, 9 menunggu migration, build, uji, dan
angka kueri D. Laporan tracked ada (berkas ini). Daftar kerja K3/K4 diserahkan lewat `GET /preview` — angkanya baru
dapat dibacakan sesudah endpoint hidup.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `ApplicationDbContext` sudah memuat dua `DbSet` baru. Sampai migration diterapkan, **seluruh** endpoint rekonsiliasi gagal "relation does not exist"; endpoint lain tidak terpengaruh karena tabelnya berbeda. Hak akses ketiga aksi baru juga harus diberikan lewat layar Akses Role sebelum dapat dipakai |
| Masalah yang diketahui | (1) Kelas K3 dan K4 hanya dilaporkan, tidak ditulis — tindak lanjutnya manual oleh petugas pendaftaran (R3.13.5). (2) Pembalikan hanya mengembalikan `EncounterStatus` dan `CompletedAt`; bila ada jalur lain yang menulis kolom penutupan encounter di antara run dan pembalikan, baris itu dilewati dengan alasan tercatat — bukan dipaksa kembali |
| Risiko tersisa | Rekonsiliasi mengubah data historis milik tabel Registrasi. Penjaganya: `expectedCount`, evaluasi ulang per baris di dalam transaksi, kunci advisory, dan run yang dapat dibalik. **Urutan rilis wajib** (roadmap R3.13.5): jalankan rekonsiliasi di sebuah lingkungan **sebelum** `BE-IGD-053` dirilis di sana, kalau tidak pasien lama ikut tertolak saat mendaftar |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | 9 berkas baru belum terlacak, `ApplicationDbContext.cs` berubah 2 baris; ditambah berkas `BE-IGD-055` yang masih terbuka (penjaga `Down()`, laporan, roadmap, traceability). Belum di-commit |
| Langkah berikutnya | (1) Pemilik: build → migration 5.2 (di atas `20260923021224`) → beri hak akses → uji 5.3, dan sampaikan angka kueri D untuk kriteria 1. (2) `BE-IGD-054` (daftar Menunggu Triage terpadu) tidak bergantung migration ini dan dapat dikerjakan paralel. (3) `BE-IGD-053` baru boleh **dirilis** sesudah rekonsiliasi K1 benar-benar dijalankan di lingkungan itu |
