# Laporan Perubahan Backend — `BE-IGD-060`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-060` |
| Judul | Kunjungan tertutup saat disposisi dilaksanakan |
| Slice | `S5` · `EPIC IGD-13` · `MVP-8` |
| Roadmap | [backend-roadmap.md](../../../roadmap/backend-roadmap.md) bagian R3.14 |
| Trace | `FR-IGD-086`, `FR-IGD-087`, `FR-IGD-089`; `AT-IGD-186`, `187` langkah 1; `IGD-DEC-163`, `164`, `165` (penanda asal), `167`; approval `IGD-DEC-170` |
| Contract version | Validation **`0.9.0`** §11 aturan 1–7; state **`0.6.0`** §9; integration **`0.5.0`** §6; permission/audit **`0.6.0`** §8; API **`0.12.0`** §9.1 nomor 4, §9.3 baris `Executed`. Kelima bagian **`approved`** lewat `IGD-DEC-170` (Rizki Gunawan, 23 September 2026). Isi kontrak tidak berubah sejak commit approval `d86d5aab` (diperiksa `git diff`) — lihat catatan hash di bagian 7 |
| Dependency | `BE-IGD-051` ✅ (22 September 2026), `BE-IGD-055` ✅ (23 September 2026) |
| Klasifikasi | `HEAVY` — skor 9: repository 0, berkas diperiksa 2 (> 20 source + dokumen), berkas diubah 1 (4 source), logika bisnis 1, kontrak API 1 (efek samping baru pada endpoint yang ada), database 2 (satu kolom + FK + migration), keamanan/auth 1 (penutupan menumpang izin aksi pemicu), UI/workflow 1 |
| Task mode | `BACKEND` — go-ahead pemilik 30 September 2026 (*"lanjutkan"*, urutan yang disepakati `FE-IGD-035` → `BE-IGD-060` → `061`/`062`/`063` → `FE-IGD-041`). Target tulis: source IGD (`EmergencyInstallationManagement`, konfigurasi EF `EmgVisit`) dan `docs/module-blueprints/igd/` (laporan, roadmap, traceability). **Tanpa** wewenang `dotnet build` (dijalankan pemilik), `dotnet ef migrations add` (dibuat pemilik), eksekusi database, `Program.cs`, commit, atau push |
| Target tulis | `NewQuilvianSystemBackend`, branch `rizkiG` (upstream `origin/rizkiG`) |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `327ccad3` (`rizkiG`) + perubahan working tree task ini (belum di-commit) |
| Status | ✅ **SELESAI 30 September 2026 — atas penilaian pemilik.** Implementation Complete; migration `20260930064430_AddEmergencyVisitClosureSource` dibuat dan diterapkan ke dev oleh pemilik; penjaga `Down()` disisipkan agent dan lulus uji empat tahap di basis data terpisah (kriteria 6); build pemilik berhasil, DLL 13.59 memuat penjaga (kriteria 7 sebagian — jumlah warning tidak dilaporkan). **Uji S1–S6 dijalankan pemilik** lewat Playwright (Antigravity) 15.11–15.15 WIB pada dev: keenamnya `PASS` menurut bukti mentah yang diperiksa agent (JSON hasil, skrip, log backend, nilai basis data) — S1 dan S2 lewat klik layar, S3–S5 lewat panggilan API. Tanpa UAT. *Sebelumnya: 🟡 30 September 2026 14.10 — tinggal uji S1–S6* |

### Backend Governance Preflight

| Butir | Hasil |
| --- | --- |
| Governance terbaca | `AGENTS.md` backend; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md` dan `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` (repo); `rules/backend/` suite skill 1.17.1 (`TASK_RULES`, `TASK_CLASSIFICATION`, `REVIEW_RULES`, `REPORT_TEMPLATE`, `DATABASE_RULES`). Kontrak engineering repo dan suite **identik** (`diff -q`). Nol folder `agents/rules/` atau `.codex/` peninggalan |
| Area / Module | `HealthServices` / `EmergencyInstallationManagement` |
| Owner / prefix registry | Emergency, prefix `Emg`, `ACTIVE / LEGACY`. Nol entity baru, jadi nol blocker `QBE-MOD-002` |
| Keberlakuan | `NEW CODE`: `EmergencyVisitService.TryCloseAfterDispositionAsync` dan record `HasilPenutupanSusulan`. `TOUCHED LEGACY`: `EmgVisit` (+1 kolom, +1 navigasi), `EmgVisitConfiguration` (+1 FK), `EmergencyDispositionController.UpdateDispositionStatus` |
| Branch | `rizkiG`, upstream `origin/rizkiG`. Working tree hanya memuat dokumen `FE-IGD-035` yang belum di-commit (bukan source) saat task dimulai |
| QBE yang berlaku | `QBE-ENT-002` (FK `Guid?` bermakna domain), `QBE-CFG-001`/`002` (FK eksplisit di configuration), `QBE-SVC-001` (aturan penutupan di service; controller hanya memanggil), `QBE-TXN-001` (satu `SaveChanges`), `QBE-VAL-001` (penjaga penutupan dipakai ulang), `QBE-LOG-001` (log perubahan status beserta aktor), `QBE-DTO-001` (nol entity keluar), `QBE-PERM-001` (nol aksi baru; metadata akses tidak diubah), `QBE-NAM-001` (nol `Trx*`), `QBE-AUD-001` (log aplikasi terpisah dari kolom audit) |
| Tidak berlaku | `QBE-CODE-*` (nol nomor bisnis), `QBE-PAGE-001`, `QBE-OPT-001`, `QBE-DEL-001`, `QBE-NAM-003`/`QBE-DB-001`/`002` (bukan legacy migration) |

---

## 1. Masalah yang diperbaiki

Sebelum task ini, disposisi yang ditandai **dilaksanakan** (`Executed`) hanya memindahkan kunjungan ke
`Disposed`. Kunjungan baru benar-benar selesai bila petugas **ingat** menekan aksi *selesaikan kunjungan*
(`PATCH /emergency-visits/{id}/complete`). Bila lupa, encounter pasien tertinggal terbuka dan catatan klinisnya
tetap dapat diubah. Inilah yang ditemukan pemilik saat meninjau rekonsiliasi `BE-IGD-052`.

*Contoh.* Pasien pulang pukul 14.00. Perawat menandai disposisi *Pulang* dilaksanakan. Tanpa task ini,
kunjungannya tetap *Tindak lanjut ditetapkan* sampai ada yang ingat menutupnya. Sesudah task ini, kunjungan dan
encounter tertutup pada penyimpanan yang sama, dan kunjungan mencatat disposisi mana yang menutupnya.

---

## 2. Proses bisnis

**Pelaku:** perawat atau petugas IGD yang memegang izin `EmergencyDisposition : Update`.

**Pemicu:** disposisi berpindah ke `Executed` lewat `PATCH /emergency-dispositions/{id}/disposition-status`.
Layar Pengkajian IGD (tab Tindak Lanjut) memakai jalur ini: disposisi dibuat `Draft`, dikonfirmasi, lalu
dilaksanakan.

**Langkah, berurutan, dalam satu penyimpanan:**

1. Kunjungan dipindahkan ke `Disposed` lewat penjaga transisi `BE-IGD-018` (perilaku lama, tidak berubah).
2. Disposisi ditandai `Executed`, `ExecutedAt` diisi bila masih kosong (perilaku lama).
3. **Baru:** sistem mencoba menutup kunjungan memakai penjaga penutupan yang sudah ada **tanpa diubah**:
   - status kunjungan `Disposed`;
   - nol observasi aktif;
   - nol kepergian yang belum tuntas;
   - nol pesanan serah terima yang belum ditentukan sikapnya.
4. **Bila penjaga lolos:**
   - kunjungan menjadi `Completed` dan `VisitCompletedAt` = waktu server;
   - `ClosedByDispositionId` diisi disposisi pemicunya;
   - `UpdateBy` = petugas yang melaksanakan disposisi;
   - encounter ikut `Completed` dan catatan klinis terbuka dikunci lewat jalur `BE-IGD-051`.
5. **Bila penjaga menolak:** disposisi **tetap** `Executed`, kunjungan tetap `Disposed` (*menunggu penutupan*),
   dan alasan penahannya tercatat di log aplikasi. Ini **bukan** galat bagi petugas (`IGD-DEC-164`).
6. Semuanya tersimpan pada satu `SaveChanges`. Bila penguncian catatan klinis gagal, seluruh aksi batal dan
   petugas mengulang (integration §6.3).

**Jalur tidak normal:**

| Keadaan | Hasil |
| --- | --- |
| Observasi masih aktif saat disposisi dilaksanakan | Disposisi `Executed`, kunjungan tetap `Disposed`; log mencatat *"Masih ada observasi yang belum diselesaikan."* |
| PATCH `Executed` diulang pada disposisi yang sudah `Executed` dan kunjungannya sudah selesai (klik ganda, percobaan ulang) | `200`, nol perubahan status kunjungan, nol galat (kriteria 4). **Sebelum perbaikan di task ini, jalur ini akan menjawab `409`** — lihat bagian 3.4 |
| Disposisi **lain** dilaksanakan pada kunjungan yang sudah `Completed` | Tetap ditolak `409` seperti sebelumnya (*"Status kunjungan tidak dapat berubah dari Completed ke Disposed."*). Kunjungan selesai tidak pernah dibuka kembali (`IGD-DEC-166`) |
| Kunjungan tanpa encounter (data lama) | Kunjungan tetap tertutup; encounter tidak ada yang ditulis (`ApplyEncounterClosureAsync` mengembalikan `false`) |

**Tidak berlaku surut** (`IGD-DEC-167`): kunjungan lama yang disposisinya sudah `Executed` sebelum rilis ini tidak
ditutup massal. Penutupan menyusul dari observasi, kepergian, dan sikap pesanan adalah lingkup `BE-IGD-061`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Governance: `AGENTS.md`; `docs/engineering/*`; `rules/backend/TASK_RULES.md`, `TASK_CLASSIFICATION.md`,
  `REVIEW_RULES.md`, `REPORT_TEMPLATE.md`, `DATABASE_RULES.md`, `engineering/BACKEND_ENGINEERING_CONTRACT.md`.
- Blueprint: kartu `BE-IGD-060`, `061` dan R3.14.1–R3.14.4 pada backend roadmap; `blueprint-manifest.md` bagian
  0i dan 2; validation §11; state §9; integration §6; permission/audit §8; API §9;
  `02-backend-architecture.md` §14; `00-interview-decisions.md` baris `IGD-DEC-170`.
- Source: `EmergencyDispositionController.cs` (seluruh aksi tulis), `EmergencyDispositionService.cs`,
  `EmergencyVisitService.cs` (constructor, `EpisodeMasihBerjalan`, `TryApplyVisitStatus`, `CanTransition`,
  `ApplyEncounterClosureAsync`), `EmergencyVisitController.Complete`, `EmergencyDepartureService` dan
  `EmergencyUnitAuthorityService` (constructor, untuk memastikan nol siklus DI), `EmgVisit.cs`, `EmgDisposition.cs`,
  `EmgVisitConfiguration.cs`, `EmgDispositionConfiguration.cs`, `Program.cs` (registrasi service),
  `Migrations/20260923021224_AddEmergencyArrivalTimeSource.cs` (pola `Down()` berpenjaga).
- Frontend (baca saja): `emergency-assessment-slice.jsx`, `emergency-assessment-disposition-tab.jsx` — untuk
  memastikan layar membuat disposisi `Draft` lalu memindahkannya lewat PATCH.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/EmergencyInstallationManagement/Models/EmgVisit.cs` | + `Guid? ClosedByDispositionId`, + navigasi `EmgDisposition? ClosedByDisposition` |
| `Repositories/Configurations/HealthServices/EmergencyInstallationManagement/EmgVisitConfiguration.cs` | + `HasOne(ClosedByDisposition).WithMany().HasForeignKey(ClosedByDispositionId).OnDelete(Restrict)`. `WithMany()` tanpa navigasi balik, sehingga EF tidak memasangkannya dengan relasi `Dispositions` ↔ `EmergencyVisit` yang sudah ada. Index FK dibuat konvensi EF |
| `Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyVisitService.cs` | Constructor + `EmergencyDispositionService`. + record `HasilPenutupanSusulan(Ditutup, DispositionId, AlasanPenahan, EncounterDitutup)` beserta nilai `Dilewati`. + `TryCloseAfterDispositionAsync(emergencyVisitId, actorUserId, now, dispositionPemicuId?, ct)` — tidak menyimpan dan tidak membuka transaksi sendiri |
| `Areas/HealthServices/EmergencyInstallationManagement/Controllers/EmergencyDispositionController.cs` | `UpdateDispositionStatus`: (a) pengulangan `Executed` pada kunjungan yang sudah selesai dilewati tanpa `409`; (b) pemicu penutupan sebelum `SaveChanges`; (c) payload log ditambah `KunjunganDitutup` dan `AlasanPenahanPenutupan`; (d) log baru `EmergencyVisit.CompleteByDisposition` beserta `ActorUserId`. Komentar lama lima baris *"VisitCompletedAt sengaja TIDAK diisi di sini … hanya diisi oleh PATCH /complete"* **dihapus** karena kini tidak benar lagi |

| `Migrations/20260930064430_AddEmergencyVisitClosureSource.cs` (+ `.Designer.cs`) | **Dibuat pemilik** 30 September 2026 13.44 lewat `dotnet ef migrations add`. **Penjaga `Down()` disisipkan agent** (13.52) atas izin pemilik — satu `migrationBuilder.Sql` `DO $$ … RAISE EXCEPTION` di baris pertama `Down()`, pola `BE-IGD-055`. `Up()` tidak diubah |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | **Digenerate pemilik** bersama migration: +12 baris, nol dihapus — properti `ClosedByDispositionId`, `HasIndex`, relasi `ClosedByDisposition` (`Restrict`), dan `Navigation` pada blok `EmgVisit` saja |

**Nol baris komentar baru** (arahan pemilik). Alasan setiap keputusan ada di laporan ini.
**Tidak disentuh:** `Program.cs`, berkas Registrasi, berkas kontrak, frontend, Bank Darah, Laboratorium.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Bentuk request/response `PATCH /emergency-dispositions/{id}/disposition-status` **tidak berubah**. Efek samping baru sesuai API §9.1 nomor 4: kunjungan bisa menjadi `Completed` pada respons berikutnya. Pengulangan `Executed` pada kunjungan selesai kini `200`, sebelumnya akan `409` |
| Database | Satu kolom `EmgVisit.ClosedByDispositionId` (`uuid`, nullable, tanpa bawaan) + FK ke `EmgDisposition` (`Restrict`) + index. Migration `20260930064430_AddEmergencyVisitClosureSource` **dibuat dan diterapkan ke dev oleh pemilik** 30 September 2026. `Down()` berpenjaga, diuji agent di basis data terpisah (bagian 5.3). Lingkungan lain wajib menerapkannya sebelum rilis: tanpa kolom ini setiap kueri `EmgVisit` gagal *"column does not exist"* |
| Keamanan/Auth | Nol aksi dan nol resource baru. Penutupan menumpang `EmergencyDisposition : Update` (permission §8.1). Keterbatasan yang diterima: pemegang izin disposisi dapat menyebabkan kunjungan tertutup tanpa memegang `EmergencyVisit : Update`; penjaga penutupan tetap memastikan kewajiban klinis tuntas lebih dulu, dan pelakunya terekam |

### 3.4 Keputusan pelaksanaan yang tidak tertulis pada kartu

| # | Keputusan | Alasan |
| ---: | --- | --- |
| 1 | Pengulangan `Executed` pada disposisi yang **sudah** `Executed` dan kunjungannya sudah selesai melewati penjaga `Disposed` tanpa galat | Disposisi `Executed → Executed` sah (`CanTransition` idempoten), tetapi kunjungan `Completed` menolak semua transisi. Karena task ini membuat kunjungan bisa selesai pada PATCH pertama, klik ganda akan langsung berbuah `409` — regresi yang melanggar kriteria 4. Pengecualiannya sempit: disposisi yang baru dilaksanakan (`Confirmed → Executed`) pada kunjungan selesai tetap ditolak |
| 2 | Signature memakai `dispositionPemicuId` opsional | Pada pemicu PATCH, perubahan disposisi ke `Executed` belum tersimpan saat penutupan dicoba, jadi kueri basis data belum melihatnya; id-nya dikirim langsung. `BE-IGD-061` memanggil tanpa parameter itu, dan method mencari disposisi `Executed` terbaru yang sudah tersimpan. Delta kecil dari §14.2 (`TryCloseAfterDispositionAsync(visitId, actor, now, ct)`) |
| 3 | Kunjungan dimuat ulang di dalam method dengan pelacakan | EF memakai `TrackAll` bawaan (nol `QueryTrackingBehavior` global), sehingga instans yang sama — yang sudah `Disposed` di memori — yang dibaca. Tanpa itu penjaga `ValidateVisitClosureAsync` akan melihat status lama |
| 4 | `ValidateVisitClosureAsync` dipanggil apa adanya | Kontrak mewajibkan penjaga yang sama tanpa perubahan (validation §11 aturan 2) |
| 5 | Nol siklus DI | `EmergencyDispositionService` → `EmergencyDepartureService` → `EmergencyDocumentNumberService`, `EmergencyUnitAuthorityService`; tak satu pun bergantung pada `EmergencyVisitService`. Nol `new EmergencyVisitService(` di source (grep) |

---

## 4. Dokumentasi endpoint

#### Health Services / Emergency Installation Management / Emergency Disposition

Base URL `api/v1/health-services/emergency-installation-management/emergency-dispositions`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `PATCH` | `/{id}/disposition-status` | Memindahkan status tindak lanjut. Ke `Executed`: kunjungan pindah ke `Disposed` lalu **dicoba ditutup**; bila penjaga lolos, kunjungan dan encounter selesai pada penyimpanan yang sama | `EmergencyDisposition : Update` (tidak berubah) |

**Contoh permintaan** (tidak berubah):

```json
PATCH /api/v1/health-services/emergency-installation-management/emergency-dispositions/{id}/disposition-status
{ "dispositionStatus": 3, "notes": null }
```

Nilai `dispositionStatus`: `1` Draft, `2` Confirmed, `3` Executed, `4` Cancelled.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Review diff | Empat berkas source, +112/−9 baris; seluruhnya dalam lingkup kartu | `PASS` | `git diff --stat` |
| Grep baris komentar baru | 0 | `PASS` | Baris `+` pada diff tanpa `//` maupun `///` |
| Grep instansiasi manual `EmergencyVisitService` | 0 | `PASS` | Hanya lewat DI |
| `Program.cs` | Tidak berubah | `PASS` | `git status --short` |
| `dotnet build` (pemilik) | Berhasil dua kali — DLL 13.47 (sesudah migration dibuat) dan **13.59** (sesudah penjaga `Down()` disisipkan 13.52). DLL 13.59 memuat literal pesan penjaga (dicari dalam bentuk UTF-16 pada `bin/Debug/net9.0/QuilvianSystemBackend.dll`: 1 kemunculan). **Jumlah warning tidak dilaporkan** | `PASS` (artefak) | Waktu berkas dan pencarian byte oleh agent |
| Migration `20260930064430` — isi `Up()` | Tepat tiga operasi pada `EmgVisit`: `AddColumn` `uuid` nullable tanpa bawaan, `CreateIndex` `IX_EmgVisit_ClosedByDispositionId`, `AddForeignKey` → `EmgDisposition.Id` `Restrict` | `PASS` | Dibaca agent |
| Snapshot EF | +12 / −0 baris, hanya blok `EmgVisit` | `PASS` | `git diff --stat` |
| Migration diterapkan ke dev | Dinyatakan pemilik sudah diterapkan | `PASS` (pernyataan owner) | Agent tidak membaca `__EFMigrationsHistory` dev |
| Uji `Down()` berpenjaga di basis data terpisah | Tahap A–D lulus (bagian 5.3) | `PASS` | Dijalankan agent atas izin pemilik; keluaran `dotnet ef` dan `psql` |
| Uji S1–S6 (pemilik) | 6/6 `PASS` — rincian 5.1 | `PASS` | Dijalankan pemilik lewat Playwright (Antigravity) pada dev. Bukti mentah diperiksa agent: `test-with-agy/igd/s1_s6_test_results.json`, skrip `test-s1-s6-be-igd-060.mjs`, `db_helper.py` (**SELECT saja**, nol SQL tulis), log `Logs/quilvian-backend-20260930.json`. Ringkasan agen `testing/2026-09-30-laporan-uji-s1-s6-be-igd-060.md` **bukan** bukti: ia menyebut akun `sysadmin_test` padahal log mencatat `superadmin@admin.com`, dan menyatakan seluruh skenario lewat layar padahal S3–S5 lewat API |

Uji manual: `PASS` — atas penilaian pemilik (S1–S6); `PASS` teramati agent untuk uji `Down()` berpenjaga.

### 5.1 Skenario uji untuk pemilik — Hasil Verifikasi

Dijalankan pemilik lewat Playwright pada dev, 30 September 2026. **Cara menjalankannya berbeda per skenario**
(diperiksa agent dari skrip): S1 dan S2 lewat klik layar Pengkajian IGD tab Tindak Lanjut (tombol *Jalankan* +
dialog konfirmasi), sesudah status kunjungan dipindahkan ke `AwaitingDisposition` lewat endpoint resmi
`PATCH /emergency-visits/{id}/visit-status`; S3, S4, dan S5 lewat `fetch` ke API dari sesi yang sama. Eksekusi S1 dan S2
terjadi pada putaran 15.11–15.12; putaran terakhir 15.14–15.15 membaca ulang keadaannya lalu menjalankan S3–S5.
Tangkapan layar `s4_…` dan `s5_…` byte-nya identik dengan `s1_03_tab_tindak_lanjut.png` — layar tidak berubah
karena aksinya lewat API — jadi buktinya kode HTTP di JSON, bukan gambar. Baris basis data dibaca dengan kueri
**baca-saja**:

```sql
SELECT "VisitStatus", "VisitCompletedAt", "ClosedByDispositionId", "UpdateBy"
FROM public."EmgVisit" WHERE "Id" = '<id kunjungan>';

SELECT "EncounterStatus", "CompletedAt"
FROM public."RegPatientEncounter" WHERE "Id" = '<id encounter>';
```

| # | Langkah | Yang diharapkan | Hasil Verifikasi Aktual | Kriteria | Status |
| ---: | --- | --- | --- | ---: | :---: |
| S1 | Kunjungan tanpa observasi aktif, tanpa kepergian menggantung, tanpa pesanan penahan. Laksanakan disposisinya | `200`. `VisitStatus` = `9` (Completed), `VisitCompletedAt` terisi, `ClosedByDispositionId` = id disposisi itu, `UpdateBy` = petugas. Encounter `EncounterStatus` Completed, `CompletedAt` terisi. Log `EmergencyVisit.CompleteByDisposition` muncul | **HTTP 200.** `VisitStatus` = `9`, `VisitCompletedAt` = `2026-09-30 08:11:34.454Z`, `ClosedByDispositionId` = `f169d5d1-e0d3-4ee0-8d31-0ec7690b3c35`, Encounter `9` (Completed), `CompletedAt` terisi sinkron. Diperiksa agent: `VisitCompletedAt`, encounter `CompletedAt`, dan disposisi `ExecutedAt` **sama persis sampai mikrodetik** (satu nilai `now`, satu `SaveChanges`); log backend memuat `EmergencyVisit.CompleteByDisposition` untuk disposisi itu dengan pelaku `superadmin` | 1 | **PASS** |
| S2 | Kunjungan dengan **observasi aktif**. Laksanakan disposisinya | `200`; disposisi `Executed`; `VisitStatus` = `7` (Disposed); `ClosedByDispositionId` kosong; log `UpdateDispositionStatus` memuat `AlasanPenahanPenutupan` = *"Masih ada observasi yang belum diselesaikan."* | **HTTP 200.** Disposisi `3` (Executed), `VisitStatus` = `7` (Disposed), `ClosedByDispositionId` = `null` (tertahan observasi aktif — observasi itu memang masih `Active` saat S3 dimulai). **Tidak teramati:** isi `AlasanPenahanPenutupan` — berkas log backend tidak menuliskan payload data, hanya kategori, pesan, path, dan pelaku | 2 | **PASS** |
| S3 | Lanjutan S2: tutup observasinya, lalu tekan **selesaikan kunjungan** (`PATCH /emergency-visits/{id}/complete`) | Kunjungan `Completed` dengan `ClosedByDispositionId` **kosong** (ditutup manual). Catatan: sebelum `BE-IGD-061`, menutup observasi **belum** menutup kunjungan sendiri | **HTTP 200.** Kunjungan `Completed` (`9`), `VisitCompletedAt` = `2026-09-30 08:15:00.034Z`, Encounter `9` (Completed), `ClosedByDispositionId` = `null` (**KOSONG**). **Catatan penting:** observasi tidak dapat *diselesaikan* (`Completed`) — backend menjawab `409` karena menyelesaikan observasi mencoba memindahkan kunjungan `Disposed` mundur ke `AwaitingDisposition`. Skrip lalu **membatalkannya** (`ObservationStatus` = `4`). Lihat bagian 7 | 3 | **PASS** |
| S4 | Ulangi PATCH `dispositionStatus: 3` pada disposisi S1 | `200`, nol galat; `VisitStatus` tetap `9`, `VisitCompletedAt` **tidak berubah** | **HTTP 200.** Nol galat, `VisitStatus` tetap `9`, `VisitCompletedAt` identik `2026-09-30 08:11:34.454Z` (idempoten aman). | 4 | **PASS** |
| S5 | Buat disposisi **baru** pada kunjungan S1, konfirmasi, lalu laksanakan | `409` *"Status kunjungan tidak dapat berubah dari Completed ke Disposed."* — kunjungan tidak dibuka kembali | **HTTP 409 Conflict.** Pesan: *"Status kunjungan tidak dapat berubah dari Completed ke Disposed."* Kunjungan tetap `9`. | 4 | **PASS** |
| S6 | Jalankan aplikasi dan lakukan S1 | Aplikasi hidup tanpa galat DI (nol siklus); `git diff --stat Program.cs` kosong | **HTTP 200.** Health check backend OK, seluruh alur berjalan tanpa galat DI/siklus. | 5 | **PASS** |

### 5.2 Migration — dibuat pemilik

Perintah, dari folder `NewQuilvianSystemBackend`:

```bash
dotnet build ./QuilvianSystemBackend.sln -p:RunAnalyzers=false
dotnet ef migrations add AddEmergencyVisitClosureSource --no-build
```

**Isi `Up()` yang diharapkan — hanya tiga operasi**, semuanya pada `public."EmgVisit"`:

| Operasi | Isi |
| --- | --- |
| `AddColumn` | `ClosedByDispositionId`, `uuid`, `nullable: true` |
| `CreateIndex` | `IX_EmgVisit_ClosedByDispositionId` |
| `AddForeignKey` | `FK_EmgVisit_EmgDisposition_ClosedByDispositionId` → `public."EmgDisposition"."Id"`, `onDelete: Restrict` |

Bila `Up()` memuat operasi lain — misalnya tabel modul lain — **jangan diterapkan**. Itu tanda snapshot EF
bergeser akibat merge integration (20 migration modul lain masuk sejak 23 September), dan migration-nya perlu
diperiksa lebih dulu.

**Tempelkan penjaga ini sebagai baris pertama `Down()`** (desain §14.6: `Down()` menolak bila jejak asal
penutupan sudah ada), mengikuti pola `20260923021224_AddEmergencyArrivalTimeSource`:

```csharp
migrationBuilder.Sql("""
    DO $$
    BEGIN
        IF EXISTS (SELECT 1 FROM public."EmgVisit" WHERE "ClosedByDispositionId" IS NOT NULL) THEN
            RAISE EXCEPTION 'Down BE-IGD-060 dihentikan: ada kunjungan IGD yang ditutup lewat disposisi (ClosedByDispositionId terisi). Menghapus kolom ini menghilangkan jejak asal penutupan tanpa bekas.';
        END IF;
    END $$;
    """);
```

Snapshot `ApplicationDbContextModelSnapshot.cs` semestinya hanya bertambah satu properti, satu index, dan satu
relasi pada blok `EmgVisit`.

**Hasilnya (30 September 2026):** migration dibuat pemilik 13.44 sebagai `20260930064430`; `Up()` dan snapshot
persis seperti di atas. Penjaga `Down()` belum tertempel, lalu disisipkan agent 13.52 atas izin pemilik. Pemilik
menerapkan migration ke dev dan build ulang 13.59.

### 5.3 Uji `Down()` berpenjaga di basis data terpisah

Dijalankan agent atas izin pemilik, pada kontainer `postgres:16` sementara (`--pull never`, port 55432,
`max_locks_per_transaction=4096`) — **bukan** dev. `dotnet ef database update --no-build` memakai DLL pemilik 13.59.
Sidik jari skema: md5 atas seluruh kolom, index, dan constraint **di luar** `EmgVisit`, ditambah jumlah kolom
`EmgVisit` dan keberadaan kolom/index/FK baru.

| Tahap | Tindakan | Hasil |
| --- | --- | --- |
| Dasar | Terapkan 260 migration sampai `20260928041937_AddAccSubledgerBalance` (1 menit 23 detik) | 748 tabel; `EmgVisit` 41 kolom; kolom/index/FK baru 0 |
| Up | Terapkan `20260930064430` | `EmgVisit` 42 kolom; kolom, index, FK = 1/1/1. Sidik jari tabel lain **identik** dengan dasar |
| A | `Down` tanpa data | **Berhasil**. Sidik jari **identik** dengan dasar |
| B | `Up` lagi | Sidik jari **identik** dengan hasil Up pertama |
| C | Sisipkan satu kunjungan uji `UJI-060-001` (`VisitStatus` 9) dengan disposisi `Executed` dan `ClosedByDispositionId` terisi, lalu `Down` | **Ditolak** `Npgsql.PostgresException P0001: Down BE-IGD-060 dihentikan: ada kunjungan IGD yang ditutup lewat disposisi …`. Kolom, index, FK, baris uji, dan catatan migration **utuh** |
| D | Kosongkan `ClosedByDispositionId` baris uji, lalu `Down` | **Berhasil**. Sidik jari identik dengan dasar; baris kunjungan uji tetap ada (hanya kolomnya yang hilang) |

Baris uji disisipkan dengan `SET LOCAL session_replication_role = replica` supaya FK ke master tidak menahan —
hanya pada basis data buangan ini. Sesudah uji: kontainer dihapus (`docker rm -f -v`) dan Docker Desktop
dimatikan kembali seperti semula. Catatan: mesin uji PostgreSQL 16, dev PostgreSQL 15.

---

## 6. Acceptance criteria dan Definition of Done

| # | Kriteria (persis roadmap) | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Pasien tanpa penahan, disposisi ditandai dilaksanakan → kunjungan `Completed`, encounter tertutup, `ClosedByDispositionId` menunjuk disposisi itu | **Terpenuhi** — atas penilaian pemilik | Source + S1 lewat layar: nilai basis data dan log backend (5.1) |
| 2 | Ada observasi aktif → disposisi tetap `Executed`, kunjungan **tidak** tertutup | **Terpenuhi** — atas penilaian pemilik | S2 lewat layar: disposisi `3`, kunjungan `7`, `ClosedByDispositionId` kosong |
| 3 | Kunjungan yang ditutup manual tetap punya `ClosedByDispositionId` kosong | **Terpenuhi** — atas penilaian pemilik | S3 lewat API `PATCH /complete` |
| 4 | Kunjungan yang sudah selesai dilewati tanpa galat saat disposisi disentuh lagi | **Terpenuhi** — atas penilaian pemilik | S4 `200`, `VisitCompletedAt` tidak bergeser; S5 disposisi baru ditolak `409` dengan pesan yang diharapkan |
| 5 | Nol perubahan `Program.cs`; nol siklus dependency saat aplikasi dijalankan | **Terpenuhi** | `Program.cs` tidak berubah (`git status`); aplikasi melayani S1–S5 dan `GET /health` `200` (S6) |
| 6 | Migration: baris lama `null`; `Down()` berpenjaga diuji di basis data terpisah; snapshot hanya bertambah satu kolom, satu FK, satu index | **Terpenuhi — terbukti agent** | `AddColumn` nullable tanpa bawaan, jadi setiap baris lama `null`; uji `Down()` empat tahap bagian 5.3; snapshot +12/−0 hanya blok `EmgVisit` |
| 7 | Build 0 error, warning sama dengan baseline | **Sebagian** | Build pemilik berhasil (DLL 13.59 memuat penjaga). **Jumlah warning tidak dilaporkan**, jadi kesamaannya dengan baseline tidak dapat dinyatakan — pola yang sama dengan `BE-IGD-052`, `054`, `055` |

**DoD.** Acceptance 1–6 terpenuhi; acceptance 7 sebagian (jumlah warning build tidak dilaporkan — pola yang sama
dengan `BE-IGD-052`, `054`, `055`, yang tetap ✅ atas penilaian pemilik). Laporan tracked ✅. **✅ SELESAI 30 September
2026 atas penilaian pemilik**; UAT belum dijalankan. `BE-IGD-061`, `062`, `063` boleh mulai — lihat peringatan
observasi di bagian 7 sebelum `BE-IGD-061`.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Hash kontrak di manifest 0i tidak cocok dengan berkas saat ini.** Kelima kontrak berbeda hash, sedangkan `02-backend-architecture.md`, `04-prd-to-mvp.md`, `erd/data-dictionary.md`, dan `flowcharts/penutupan-lewat-disposisi.md` cocok. Sebabnya: hash dihitung sebelum baris approval `IGD-DEC-170` ditambahkan ke kepala tiap kontrak. Desain, approval, dan kartu masuk satu commit `d86d5aab`, dan `git diff d86d5aab HEAD` atas kelima kontrak **kosong**. Jadi isi yang disetujui sama dengan yang diimplementasikan; yang basi adalah catatan hash-nya. Juga: baris *"Status bagian ini: `draft` — menunggu approval pemilik"* di dalam validation §11, state §9, dan integration §6 belum diperbarui, padahal kepala berkasnya sudah menulis `approved (IGD-DEC-170)` |
| Masalah yang diketahui | (1) **Celah lama:** `POST /emergency-dispositions` menerima `dispositionStatus: 3` (Executed) langsung tanpa memindahkan kunjungan ke `Disposed`, sehingga penutupan tidak terpicu. Layar tidak memakai jalur ini (membuat `Draft`); tidak ditambal karena di luar kartu. (2) Validasi pembuatan disposisi menolak kunjungan `Disposed`/`Cancelled` tetapi **tidak** `Completed`, sehingga disposisi baru bisa dibuat pada kunjungan selesai (lalu ditolak saat dilaksanakan, S5). Di luar kartu |
| Risiko tersisa | Sesudah kunjungan tertutup, Bank Darah menolak order darah baru dan alokasi kantong, dan Laboratorium menolak pemesanan baru (`IGD-DEC-169`, diterima). Kunjungan yang masih menunggu penutupan baru tertutup sendiri sesudah `BE-IGD-061` |
| **Temuan uji S3 — menahan `BE-IGD-061`** | `EmergencyObservationController.UpdateObservationStatus` memetakan observasi `Completed` → kunjungan `AwaitingDisposition` dan `Escalated` → `InTreatment` (perilaku `BE-IGD-021`). Dari `Disposed`, kedua perpindahan itu dilarang penjaga transisi, jadi **sesudah disposisi dilaksanakan, observasi yang masih aktif hanya bisa dibatalkan, tidak bisa diselesaikan dengan kesimpulan** — terbukti `409` pada S3. Padahal validation §11 aturan 4 dan kartu `BE-IGD-061` kriteria 1 mengandaikan perawat *menutup* observasi lalu kunjungan ikut tertutup. Ini keputusan desain yang harus diambil pemilik **sebelum** `BE-IGD-061`: misalnya, observasi `Completed` pada kunjungan `Disposed` tidak memindahkan status kunjungan (seperti `Cancelled` sekarang) |
| Data uji tertinggal di dev | Kunjungan DEDE KURNIAWAN (`0703289d…`) dan AGNES YULIANI (`291ed4fa…`) kini `Completed` beserta encounter-nya; observasi Agnes `8e26b5bb…` `Cancelled`; satu disposisi baru dari S5 (berstatus `Confirmed`) menempel pada kunjungan Dede yang sudah selesai. Seluruhnya dibuat lewat API resmi di basis data milik pemilik, nol SQL tulis |
| Kebersihan dokumen uji | Ringkasan agen `testing/2026-09-30-laporan-uji-s1-s6-be-igd-060.md` memuat baris akun **beserta kata sandinya** — sebaiknya dihapus sebelum commit, sama seperti ringkasan `FE-IGD-035`. `test-with-agy/igd/db_helper.py` menyimpan kredensial basis data dev dalam teks biasa; folder itu di-ignore git, jadi tidak ikut ter-commit |
| Perubahan sampingan | `NONE`. Satu komentar lama yang menjadi tidak benar dihapus (bagian 3.2) |
| Interupsi | `NONE` |
| Status Git | Source: `M` `EmergencyDispositionController.cs`, `EmgVisit.cs`, `EmergencyVisitService.cs`, `EmgVisitConfiguration.cs`, `ApplicationDbContextModelSnapshot.cs`; `??` `Migrations/20260930064430_AddEmergencyVisitClosureSource.cs` dan `.Designer.cs`. Dokumen: laporan ini dan pembaruan roadmap/traceability; ditambah dokumen `FE-IGD-035` dan dua ringkasan agen uji di `testing/` dari task sebelumnya. Belum di-commit |
| Langkah berikutnya | Putuskan temuan observasi di atas, lalu `BE-IGD-061`. `BE-IGD-062` dan `BE-IGD-063` tidak terdampak temuan itu dan boleh jalan lebih dulu |
