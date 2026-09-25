# Laporan Perubahan Backend — `BE-IGD-050`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-050` |
| Judul | Pra-cek episode ganda sebelum encounter dibuat (korektif) |
| Slice | `IGD-S02` · `EPIC IGD-01` · `MVP-2` |
| Roadmap | [backend-roadmap.md](../../../roadmap/backend-roadmap.md) bagian R3.12 |
| Trace | **`IGD-DEC-138`** (`approved` Rizki Gunawan, 21 September 2026 — perilaku target; mekanisme lapis A diputuskan pemilik 21 September 2026 malam); `IGD-DEC-084`; `IGD-DEC-135`; `FR-IGD-005`…`012`; `IGD-OQ-093` (`open`); temuan `FE-IGD-014` ([laporan](../frontend/FE-IGD-014.md) bagian 6.1 dan 8) |
| Contract version | API **`0.10.0`** — `draft`, **aditif** (naik dari `0.9.0`); bagian baru `1.3`. Validation matrix **`0.7.0`** (naik dari `0.6.0`); bagian baru `1.2` |
| Dependency | `BE-IGD-023` ✅, `BE-IGD-025` ✅ |
| Klasifikasi | `MEDIUM` — skor 5: cakupan repository 0, berkas diperiksa 1 (± 12), berkas diubah 0 (3 source; sisanya dokumen), logika bisnis 1 (satu parameter opsional pada kueri yang ada), kontrak API 2 (menambah endpoint), database 1 (hanya perilaku query), keamanan/auth 0 (permission yang sudah ada, nol baru), UI/workflow 0 |
| Task mode | `BACKEND` — target tulis: source backend dan `docs/module-blueprints/igd/**`. **Tidak** ada wewenang build (pemilik: *"Build hanya bila saya perintahkan"*), commit, push, pull, merge, rebase, pindah branch, stash, migration, atau tulis basis data |
| Target tulis | `NewQuilvianSystemBackend` |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | `267b56a0` pada branch `rizkiG` — **working tree belum di-commit** |
| Tanggal | 21 September 2026 (malam) |
| Status | ✅ **Selesai atas penilaian pemilik — 21 September 2026 (malam).** Implementation Complete; **build dan uji API S1–S7 dijalankan pemilik dan dilaporkan `PASS` semuanya** (pernyataan pemilik pada percakapan; agent **tidak** mengulang dan tidak mengamati hasilnya). **Dikecualikan atau belum tercatat, disebut apa adanya:** (a) **acceptance 8** — hasil kueri audit A dan B belum dilaporkan; (b) angka tidak dilampirkan: jumlah warning build, hitungan baris S4, baris log SQL S5. Pernyataan pemilik bahwa agent lain memverifikasi hasil yang sama **tidak** dipakai sebagai bukti tersendiri. UAT belum dan tidak diklaim |

### Backend Governance Preflight

| Butir | Hasil |
| --- | --- |
| Area / Module | `HealthServices` / `EmergencyInstallationManagement` |
| Owner / prefix registry | Emergency, prefix `Emg`, lifecycle `ACTIVE / LEGACY` — entri **ada** (registry repo baris 19), nol blocker `QBE-MOD-002` |
| Keberlakuan | `TOUCHED LEGACY` — controller, service, dan DTO yang sudah ada; **nol entity, nol tabel, nol prefix baru** |
| QBE ID yang berlaku | `QBE-SVC-001` (action baru **tidak** menyentuh `DbContext`; ia hanya memanggil `EmergencyVisitService` — kepatuhan penuh untuk kode baru meski controller ini lama), `QBE-API-001` (envelope `ApiResponse<T>`, route `api/v1/...`, kode status mengikuti kebiasaan controller), `QBE-PERM-001` (pasangan `[AccessAction]` + `[AccessPermission]` pada method yang sama, nama aksi sama huruf demi huruf), `QBE-DTO-001` (entity EF tidak terekspos; respons berupa DTO), `QBE-VAL-001` (`patientId` divalidasi) |
| Tidak berlaku | `QBE-ENT-*`, `QBE-CFG-*`, `QBE-NAM-*`, `QBE-CODE-*`, `QBE-DB-*` (nol entity/migration); `QBE-LOG-001` (endpoint baca-saja, tidak mengubah state — tidak ada peristiwa yang dicatat); `QBE-TXN-001` (satu pembacaan) |
| Governance | `AGENTS.md`, `CLAUDE.md`, `rules/backend/*` (suite 1.17.1), `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`, dan `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` terbaca — bukan `BLOCKED`. Branch `rizkiG` — sama dengan yang dipakai modul ini pada seluruh task sebelumnya. **Selisih yang dilaporkan:** salinan registry pada suite skill (103 baris) lebih tua daripada salinan repo (114 baris) — tidak memuat `Num`, `Gz`, `Acc`, dan menandai `Opr`/`Rad` `PLANNED`. Yang berlaku salinan repo (`AGENTS.md`); baris `Emg` **identik** pada keduanya, jadi tidak memengaruhi task ini. Kontrak rekayasa identik (73 baris, `diff` bersih). Tidak ada folder `agents/rules/`, `.codex/`, maupun `rules/` peninggalan di repo |

---

## 1. Masalah yang diperbaiki

Pendaftaran IGD dikerjakan layar sebagai **dua permintaan HTTP terpisah**. Yang pertama, `POST patient-encounters`
(modul Registrasi), menyimpan `RegPatientEncounter` dan langsung commit. Yang kedua, `POST emergency-visits`, baru
memeriksa apakah pasien masih punya kunjungan IGD yang berjalan. Penolakan `409` pada permintaan kedua karena itu
**selalu** meninggalkan encounter dari permintaan pertama — encounter **yatim**, tanpa kunjungan IGD.

Ini terkonfirmasi dari source dan **terbukti lewat layar oleh pemilik 21 September 2026 (malam)**: percobaan ulang
menampilkan peringatan *"Encounter sudah terbentuk"*, artinya encounter dari percobaan gagal sebelumnya tersimpan
tanpa kunjungan ([laporan `FE-IGD-014`](../frontend/FE-IGD-014.md) bagian 6.1).

*Contoh.* Pasien RAYYAN didaftarkan pukul 09.35 (kunjungan `IGD-0001`, *Menunggu triage*). Pukul 09.40 petugas lain
mendaftarkannya lagi tanpa alasan: langkah 1 membuat encounter `REG-…-B`, langkah 2 menjawab `409`. Sekarang ada
`REG-…-B` **tanpa kunjungan IGD** di basis data. Sesudah task ini, layar dapat bertanya **sebelum** langkah 1 dan
berhenti — `REG-…-B` tidak pernah lahir.

Keputusan pemilik `IGD-DEC-138`: pendaftaran ganda tidak boleh meninggalkan encounter tanpa `EmgVisit`, dan **dilarang**
menghapus keras atau membersihkan encounter yatim yang sudah ada tanpa audit seluruh referensinya.

---

## 2. Proses bisnis

**Pelaku:** layar pendaftaran IGD (atas nama petugas pendaftaran). **Pemicu:** petugas memilih pasien yang sudah dikenal,
**sebelum** layar memanggil `POST patient-encounters`.

1. Layar memanggil `GET .../emergency-visits/active-episode?patientId={id}`.
2. Backend memastikan `patientId` bukan kosong. Bila kosong → `400`.
3. Backend mencari kunjungan IGD pasien itu yang **masih berjalan** memakai `CariEpisodeAktifAsync` — method yang
   **sama** dengan penolakan `409` pada `POST /`. Kunjungan dihitung berjalan bila milik pasien itu, belum ditandai
   terhapus, dan statusnya **bukan** `Completed` maupun `Cancelled`. `Disposed` **masih berjalan**.
4. Nama pasien ikut terbaca dalam kueri yang sama (satu `JOIN`), lalu dibentuk oleh `ResolvePatientName` — fungsi yang
   sama dengan daftar dan detail kunjungan.
5. Backend menjawab `200` dengan `hasActiveEpisode` dan `visit`. **Tidak ada satu pun baris yang ditulis.**
6. Layar memutuskan: bila ada episode berjalan dan alasan pendaftaran ganda kosong, layar berhenti dan menampilkan
   kunjungan yang sudah ada — **encounter tidak dibuat**. (Perilaku layar milik `FE-IGD-034`, bukan task ini.)

**Jalur tidak normal.**

| Keadaan | Hasil |
| --- | --- |
| `patientId` tidak dikirim atau `00000000-0000-0000-0000-000000000000` | `400` — "patientId wajib diisi. Pilih pasien lebih dulu sebelum memeriksa kunjungan IGD yang masih berjalan." |
| `patientId` bukan berbentuk GUID (`abc`) | `400` bentuk galat bawaan ASP.NET Core (bukan `ApiResponse`) — sama dengan semua filter `Guid` pada `GET /`; **tidak diubah** |
| Pasien tidak punya kunjungan berjalan | `200`, `hasActiveEpisode: false`, `visit: null` — bukan `404`, agar pra-cek yang normal tidak menghasilkan galat |
| Pasien punya dua kunjungan berjalan (data yang seharusnya tidak ada) | Yang `arrivalDateTime`-nya terbaru dikembalikan — aturan yang sama dengan `POST /` |
| Kunjungan berstatus `Disposed` | Masih berjalan → `hasActiveEpisode: true` |
| Kunjungan berstatus `Completed` atau `Cancelled` | Tidak menahan → `hasActiveEpisode: false` |
| Kunjungan ditandai terhapus (`IsDelete`) | Diabaikan → `hasActiveEpisode: false` |
| Pasien tanpa identitas (tanpa `PatientId`) | Tidak dapat diperiksa; layar tidak punya `patientId` untuk dikirim, dan pasien tanpa identitas **memang tidak pernah tertahan** (`AT-IGD-085`) |
| Nama pasien kosong pada master, kunjungan punya alias sementara | Alias dipakai; bila alias juga kosong, "Pasien belum teridentifikasi" — tidak pernah string kosong |
| Pengguna tidak memegang `EmergencyVisit : Create` | `403` |
| Pra-cek gagal (jaringan, `5xx`) | Tidak menahan pendaftaran — sikap layar (`fail-open`) adalah keputusan pemilik untuk `FE-IGD-034` |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`, `CLAUDE.md`; `TASK_RULES`, `TASK_CLASSIFICATION`, `API_RULES`, `REVIEW_RULES`, `REPORT_TEMPLATE`,
`role-access-rules.md`; `BACKEND_ENGINEERING_CONTRACT.md` dan registry (kedua salinan); `MODULE-STATUS.md`; kartu
`BE-IGD-050` dan `BE-IGD-049` pada roadmap; `IGD-DEC-138` dan `IGD-OQ-093`; laporan `BE-IGD-049` (bentuk) dan
`FE-IGD-014` bagian 6.1 dan 8;
`EmergencyVisitController.cs`; `EmergencyVisitService.cs`; `EmergencyVisitDtos.cs`; `EmgVisit.cs`; `MstPatient.cs`;
`EmergencyVisitStatus.cs`; `Responses/ApiResponse.cs`; `Seeders/AccessMenuSeeder.cs` (perilaku baris aksi bersama);
`api-contract.md`; `validation-matrix.md`. Frontend **tidak** diperiksa ulang (hanya-baca, dan laporan `FE-IGD-014`
sudah memuat perilaku konsumennya).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/EmergencyInstallationManagement/Controllers/EmergencyVisitController.cs` | Satu action `GetActiveEpisode` (`GET active-episode`) beserta pasangan `[AccessAction]`/`[AccessPermission]`. **+57 baris, 0 baris dihapus.** Action `Create` dan seluruh action lain **tidak diubah** |
| `Areas/HealthServices/EmergencyInstallationManagement/DTOs/EmergencyVisitDtos.cs` | Dua kelas respons baru: `EmergencyActiveEpisodeResponse` dan `EmergencyActiveEpisodeVisitSummary`. **+36 baris, 0 dihapus** |
| `Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyVisitService.cs` | `CariEpisodeAktifAsync` mendapat satu parameter opsional `bool sertakanPasien = false` di ujung daftar; bila `true`, kueri memuat `Include(x => x.Patient)`. **+19 baris, 3 diganti** (pembentukan kueri dipecah menjadi variabel). Predikat aturan "berjalan" **tidak disentuh** |
| `docs/module-blueprints/igd/contracts/api-contract.md` | `0.10.0`; baris tabel `GET /active-episode`; bagian `1.3` baru |
| `docs/module-blueprints/igd/contracts/validation-matrix.md` | `0.7.0`; bagian `1.2` baru (lima aturan pra-cek) |
| Roadmap backend dan frontend, `requirement-traceability.md`, `MODULE-STATUS.md`, laporan ini | Penandaan status dan tautan bukti |

**Tidak disentuh:** `PatientEncounterController.cs` dan seluruh modul Registrasi, `EncounterIntakeService`, `Program.cs`
(nol service baru), model, konfigurasi EF, `Migrations/`, snapshot, seluruh frontend. Pencarian teks memastikan **hanya dua**
pemanggil `CariEpisodeAktifAsync`: `POST /` (tidak berubah) dan action baru.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif.** Satu endpoint baru `GET /active-episode`; nol ruas lama berubah, nol request berubah, nol penolakan baru pada endpoint yang ada. Versi `0.9.0` → `0.10.0`. Konsumen lama tidak terpengaruh |
| Database | **NOT APPLICABLE untuk schema.** Nol entity, kolom, index, migration. Perilaku query: **satu** `SELECT` baca-saja pada `EmgVisit` dengan `LEFT JOIN` ke `MstPatient`. Nol tulis basis data |
| Keamanan/Auth | **Nol permission baru.** Memakai aksi `Create` yang sudah ada pada `EmergencyVisit` (keputusan pemilik). Tidak ada `IsInRole`, daftar peran, nama departemen, atau `UserType`. Nama pasien kini terkirim ke pemegang `EmergencyVisit : Create` — peran yang sama yang sudah menerima nama itu pada respons `POST /` |

**Keputusan teknis 1 — bagaimana nama pasien didapat (diminta pemilik untuk diputuskan dan dicatat).**
`CariEpisodeAktifAsync` mengembalikan `EmgVisit` **tanpa** `Include` pasien, sementara `ResolvePatientName` privat pada
controller. Acceptance 6 menuntut satu kueri tanpa `N+1`, dan acceptance 4 menuntut aturan "berjalan" tidak diduplikasi.
Tiga bentuk ditimbang:

| Bentuk | Kueri | Aturan "berjalan" | Nama pasien | Dipilih |
| --- | --- | --- | --- | :-: |
| **A.** Parameter opsional `sertakanPasien` pada `CariEpisodeAktifAsync` → `Include(Patient)`; nama lewat `ResolvePatientName` yang sudah ada | **1** (`JOIN`) | Satu tempat, dipanggil **persis** oleh kedua pemanggil | Satu sumber (`ResolvePatientName`) | **Ya** |
| **B.** `CariEpisodeAktifAsync` apa adanya, lalu kueri kedua `MstPatient` berdasarkan kunci | 2 saat ada episode | Satu tempat | Satu sumber | Tidak — melanggar "satu kueri" |
| **C.** Pembantu baru di service yang memproyeksikan kolom, dengan predikat "berjalan" dipisah menjadi pembangun bersama | 1 (proyeksi kolom) | Satu pembangun, tetapi **`CariEpisodeAktifAsync` sendiri dibongkar** di jalur `POST /`; acceptance 4 ("keduanya memanggil `CariEpisodeAktifAsync`") tidak lagi harfiah | Aturan nama (nama → alias → bawaan) **tersalin** ke service karena `ResolvePatientName` privat | Tidak |

Bentuk A dipilih karena **terkecil** (satu parameter dengan nilai bawaan, satu kondisi) dan satu-satunya yang membuat
**kedua** aturan — "berjalan" dan "nama pasien" — tetap punya tepat satu tempat. Jalur `POST /` menjalankan kueri
yang **persis sama** seperti sebelumnya (`sertakanPasien` bernilai `false`).

**Kekurangan bentuk A, dinyatakan apa adanya.** Kueri bersifat **satu kueri ber-`JOIN`, bukan proyeksi kolom**: seluruh
kolom `EmgVisit` dan `MstPatient` terbaca, lalu diproyeksikan ke tujuh ruas respons di memori. Untuk **satu baris** ini
tidak berarti (`GET /` yang sudah ada memuat `Include(Patient)` untuk 25 baris per halaman, dan `MstPatient` tidak punya
kolom biner), tetapi bila pemilik menuntut proyeksi kolom secara harfiah pada acceptance 6, bentuk C-lah jalannya dan
menuntut keputusan atas pembongkaran `CariEpisodeAktifAsync`.

**Keputusan teknis 2 — hak akses dan baris bersama pada layar Akses Role.** Method baru memakai
`[AccessAction("Create", "Create Emergency Visit", …, AccessType = AccessTypes.Create, SortOrder = 2)]` dan
`[AccessPermission("EmergencyVisit", "Create")]`. Ketiga nilai dicocokkan huruf demi huruf: `ControllerName =
"EmergencyVisit"` (baris 31) = argumen ke-1 `AccessPermission`; argumen ke-1 `AccessAction` `"Create"` = argumen ke-2
`AccessPermission` `"Create"`. `AccessType` salah satu dari empat nilai baku; `VisibleInRoleAccess` dan `IsSystemOnly`
tidak disentuh.

*Observasi, tidak diperbaiki.* `AccessMenuSeeder.EnsureAction` menyimpan **satu** baris `SysActionAccess` per pasangan
(controller, `ActionName`) dan pada setiap method bernama sama **menimpa** `DisplayName`, `HttpMethod`, `RoutePath`, dan
`Description` baris itu — yang terakhir diproses menang. Akibatnya baris `EmergencyVisit : Create` pada layar Akses Role
kini dapat menampilkan rute `GET …/active-episode` alih-alih `POST …/emergency-visits`. **Kosmetik saja:** penegakan
`AccessPermission` memakai (controller, `ActionName`), bukan rute, dan tidak ada baris baru yang dapat dicentang. Pola yang
sama sudah ada pada controller ini (empat method `Update`, dua `Read`). `DisplayName` sengaja dibuat **sama** dengan `POST /`
supaya label baris tidak berubah apa pun urutannya, dan `Description` ditulis sebagai gabungan kedua kegunaan.

**Keputusan teknis 3 — bentuk route.** `[HttpGet("active-episode")]` tidak bertabrakan dengan `[HttpGet("{id:guid}")]`:
segmen `active-episode` bukan GUID sehingga batasan `:guid` menolaknya, dan segmen literal berprioritas di atas
parameter. Tidak ada controller lain yang berbagi awalan `emergency-visits`. Terbukti dari source; **belum terbukti
runtime** — uji API S1 sekaligus memastikannya.

---

## 4. Dokumentasi endpoint

#### Health Services / Emergency Installation Management / Emergency Visit

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/active-episode?patientId={uuid}` | Memeriksa, tanpa menulis apa pun, apakah pasien masih punya kunjungan IGD berjalan — dipanggil **sebelum** encounter dibuat. Respons `200` `hasActiveEpisode` + `visit`; `400` bila `patientId` kosong | `EmergencyVisit : Create` |

Route `POST /` dan seluruh route lain pada grup ini **tidak berubah**. Bentuk respons lengkap: `api-contract.md` bagian `1.3`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Dijalankan **pemilik** (21 September 2026 malam, langsung ke `bin/Debug`); agent tidak menjalankannya. DLL berjam 15:09, lebih baru dari source terakhir (14:47), dan memuat `active-episode`, `GetActiveEpisode`, `sertakanPasien`. Backend lalu dijalankan pemilik dengan DLL itu | `PASS` (pernyataan pemilik; DLL diperiksa agent) | **Keluaran build tidak dilampirkan** — jumlah warning (baseline 207) belum tercatat |
| Tinjauan diff | 3 berkas source, `+112/−3`. **Nol baris dihapus** pada controller dan DTO; service: 3 baris diganti (pembentukan kueri). Nol baris atribut `Http*`/`Route`/`Authorize`/`Access*` **yang sudah ada** berubah | `PASS` | `git diff -U0` |
| Aturan "berjalan" tidak diduplikasi | Predikat `Completed`/`Cancelled`/`IsDelete`/`PatientId` ada di **satu** tempat; dua pemanggil (`POST /`, action baru) | `PASS` | Pencarian teks `CariEpisodeAktifAsync(`; diff service tidak menyentuh predikat |
| Action baru tidak menulis | Nol `Add`, `Update`, `Remove`, `SaveChanges`, dan nol `_dbContext` di dalam `GetActiveEpisode` | `PASS` | Baca source |
| Pasangan `Access*` | `ControllerName` = argumen ke-1 `AccessPermission`; argumen ke-1 `AccessAction` = argumen ke-2 `AccessPermission` (`"Create"`); `AccessType.Create` | `PASS` | Baca source, baris 31, 199–200 |
| Akhir baris dan BOM | Controller dan service: seluruh baris CRLF + BOM, seperti `HEAD`; DTO: CRLF tanpa BOM, seperti `HEAD`. Dokumen: LF, seperti sebelumnya | `PASS` | Hitungan CRLF per berkas |
| Nol schema | `git status --short`: tidak ada berkas di `Models/`, `Repositories/`, `Migrations/`, maupun `Program.cs` | `PASS` | `git status --short` |
| Uji API S1–S7 (bagian 5.1) | Dijalankan pemilik; **ketujuh skenario dilaporkan `PASS`, tidak ada yang gagal** | `PASS` (pernyataan pemilik) | Pernyataan pemilik pada percakapan; tanpa lampiran. Agent tidak mengamati |
| Hitungan baris sebelum/sesudah (acceptance 5, S4) | Dilaporkan `PASS` (sama sebelum dan sesudah) | `PASS` (pernyataan pemilik) | Angka tidak dilampirkan; agent dilarang menjalankan perintah basis data |
| Log SQL satu kueri (acceptance 6, S5) | Dilaporkan `PASS` (satu kueri) | `PASS` (pernyataan pemilik) | Baris log tidak dilampirkan |
| Kueri audit encounter yatim A dan B (acceptance 8) | Hasil belum dilaporkan | `NOT RUN` | **Hanya dijalankan pemilik (baca-saja)**; kueri ada pada kartu roadmap. **Dikecualikan dari ✅** (bagian 6) |

**Tidak ada automated test** (proyek test dihapus 11 September 2026, `IGD-DEC-110`).

Uji manual: `REQUIRED` — dijalankan pemilik (bagian 5.1).

### 5.1 Uji API untuk pemilik (S1–S7)

Jalankan **sesudah** build ulang dan restart backend. Gunakan akun yang memegang `EmergencyVisit : Create`; jalankan juga
sekali dengan akun **tanpa** hak itu (SuperAdmin melewati seluruh pemeriksaan, jadi S6 tidak dapat diuji dengannya).

| # | Skenario | Yang diharapkan |
| ---: | --- | --- |
| S1 | Pasien yang punya kunjungan berjalan (mis. pasien `IGD-260917023643-4A7A93`, *Triaged*) → `GET …/active-episode?patientId={id}` | `200`, `hasActiveEpisode: true`, `visit` berisi `id`, `encounterId`, `patientId`, `patientName`, `emergencyVisitNumber`, `visitStatus` (`3`), `arrivalDateTime` — dan **bukan** `404` (rute tidak bertabrakan dengan `{id:guid}`) |
| S2 | Pasien tanpa kunjungan berjalan (atau yang semua kunjungannya `Completed`/`Cancelled`) | `200`, `hasActiveEpisode: false`, `visit: null` |
| S3 | `patientId` dihilangkan, dan `patientId=00000000-0000-0000-0000-000000000000` | `400` dengan pesan pada bagian 2 |
| S4 | **Baca-saja:** hitung baris `RegPatientEncounter` dan `EmgVisit` **sebelum** dan **sesudah** S1–S3 (`SELECT count(*)` pada masing-masing tabel) | Kedua hitungan sama persis |
| S5 | Aktifkan log SQL EF, panggil S1 | **Satu** `SELECT` pada `EmgVisit` dengan `LEFT JOIN "MstPatient"`; tidak ada kueri kedua |
| S6 | Akun tanpa `EmergencyVisit : Create` memanggil S1 | `403` |
| S7 | `POST /emergency-visits` untuk pasien S1 tanpa alasan pendaftaran ganda | Tetap `409` dengan pesan yang menyebut nomor kunjungan — **tidak berubah** |

Sebagai tambahan, pastikan baris `EmergencyVisit : Create` masih muncul (satu baris) pada layar Pengaturan → Manajemen
Role → Akses Role, dan bahwa peran yang boleh mendaftarkan kunjungan tetap dapat memanggil `POST /`.

---

## 6. Acceptance criteria dan Definition of Done

| # | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Pasien dengan episode aktif → `200`, `hasActiveEpisode` benar, `visit` berisi `id`, `encounterId`, `patientId`, `patientName`, `emergencyVisitNumber`, `visitStatus`, `arrivalDateTime` | **Terpenuhi — runtime dilaporkan pemilik `PASS` (S1)** | `GetActiveEpisode` mengisi ketujuh ruas; DTO `EmergencyActiveEpisodeVisitSummary`. Isi respons tidak dilampirkan |
| 2 | Pasien tanpa episode aktif → `200`, `hasActiveEpisode` salah, `visit` `null` | **Terpenuhi — runtime dilaporkan pemilik `PASS` (S2)** | Cabang `episodeAktif == null` |
| 3 | `patientId` kosong atau `Guid.Empty` → `400` dengan pesan jelas | **Terpenuhi — runtime dilaporkan pemilik `PASS` (S3)** | `if (patientId == Guid.Empty)` — mencakup parameter yang dihilangkan (nilai bawaan `Guid.Empty`) |
| 4 | Aturan "aktif" **sama persis** dengan penolakan `POST /`: keduanya memanggil `CariEpisodeAktifAsync`, nol salinan aturan | **Terpenuhi** | Dua pemanggil, satu predikat; diff service tidak menyentuh predikat (bagian 5) |
| 5 | **Baca-saja:** jumlah baris `RegPatientEncounter` dan `EmgVisit` sebelum dan sesudah sama | **Terpenuhi — dilaporkan pemilik `PASS` (S4)**; source: nol tulis pada action | Hitungan baris sebelum/sesudah **tidak dilampirkan** |
| 6 | Satu kueri berproyeksi per pemanggilan; nol `N+1` | **Terpenuhi dengan catatan — dilaporkan pemilik `PASS` (S5).** **Satu** kueri ber-`JOIN` (nol `N+1`); **bukan proyeksi kolom** — lihat keputusan teknis 1 | Satu `FirstOrDefaultAsync`; baris log SQL tidak dilampirkan |
| 7 | `POST /emergency-visits` **tidak berubah** — `409` tetap sebagai jaring pengaman | **Terpenuhi — dilaporkan pemilik `PASS` (S7)** | Nol baris `Create` berubah; pemanggilnya memakai `sertakanPasien = false` (bawaan) sehingga kueri identik |
| 8 | **Audit encounter yatim yang sudah ada — baca-saja, dijalankan pemilik, hasilnya dicatat.** Tidak ada `hard-delete` dan tidak ada pembersihan | **DIKECUALIKAN — hasil belum dilaporkan.** Agent tidak menjalankan kueri; **tidak ada** penghapusan atau pembersihan dilakukan (bagian pelarangan terpenuhi) | Kueri A dan B pada kartu roadmap; angka **belum** dicatat. Diisi pemilik bila kueri dijalankan |
| 9 | Celah yang **tidak** ditutup dinyatakan apa adanya: dua pendaftaran serentak dan klien tanpa pra-cek (`IGD-OQ-093`) | **Terpenuhi** | Bagian 7.1; `api-contract.md` §1.3; `validation-matrix.md` §1.2 |
| 10 | Nol schema; nol migration; nol tulis basis data; `dotnet build -p:RunAnalyzers=false` → 0 error, warning sama dengan baseline | **Terpenuhi menurut pemilik.** Nol schema/migration/tulis DB terbukti dari `git status --short`. Build dijalankan pemilik (DLL `bin/Debug` berjam 15:09 memuat `active-episode`, `GetActiveEpisode`, `sertakanPasien` — diperiksa agent) dan backend berjalan dengan DLL itu | **Jumlah warning tidak dilampirkan**, jadi "warning sama dengan baseline (207)" belum tercatat sebagai angka |

**Definition of Done.** Terpenuhi **dengan pengecualian yang disebut apa adanya**: **acceptance 8** (hasil kueri audit A dan B
belum dilaporkan; tidak ada pembersihan dilakukan), serta tiga angka yang tidak dilampirkan (warning build, hitungan baris S4,
log SQL S5). Kesahihan hasil runtime bertumpu pada **pernyataan pemilik**; agent tidak mengulang uji dan tidak dapat
mengamatinya. Status **✅** ditetapkan atas penilaian pemilik pada 21 September 2026 (malam). Laporan tracked ada; roadmap,
traceability, dan `MODULE-STATUS.md` diperbarui; `IGD-OQ-093` tercatat pada bagian 7.1.
Syarat pemilik untuk memulai `FE-IGD-034` — *"kontrak selesai dan build terverifikasi"* — **terpenuhi**. Pemilik memerintahkan
`FE-IGD-034` dimulai pada percakapan yang sama.

---

## 7. Catatan penutup

### 7.1 Backend gap eksplisit — `IGD-OQ-093` (`open`)

Task ini menerapkan **lapis A** saja. Jaminan ditegakkan oleh **pemanggil**, bukan server. Dua celah **tidak ditutup**:

| # | Celah | Akibat |
| ---: | --- | --- |
| (a) | **Dua pendaftaran serentak** untuk pasien yang sama: keduanya bertanya, keduanya mendapat `hasActiveEpisode: false`, keduanya membuat encounter, lalu salah satunya ditolak `409` pada `POST /` | Satu encounter yatim |
| (b) | **Klien yang tidak memanggil pra-cek** — layar lama yang belum memakai `FE-IGD-034`, klien lain, atau pemanggilan langsung — memanggil `POST patient-encounters` lalu `POST /` | Satu encounter yatim bila `POST /` ditolak |

Menutupnya butuh jaminan sisi server, dan itu menyentuh modul Registrasi atau membuat pola baru: **B1** guard episode
ganda pada pembuatan encounter `Emergency` di Registrasi; **B2** satu endpoint orkestrasi IGD yang membuat encounter dan
kunjungan dalam satu transaksi; **B3** kompensasi otomatis (ditolak — menghapus atau membatalkan encounter tanpa audit
referensi bertentangan dengan `IGD-DEC-138`). Agent tidak memilih; rekomendasi pada `IGD-OQ-093` adalah B1 bila pemilik
Registrasi setuju. Sampai diputuskan, `FE-IGD-014` **tidak boleh** ditandai ✅ final end-to-end atas nama celah ini.

**Encounter yatim yang sudah ada tidak disentuh.** Nol penghapusan, nol pembersihan, nol perintah basis data oleh agent.
Uji layar pemilik 21 September 2026 malam sendiri meninggalkan minimal satu encounter yatim di dev (`FE-IGD-014` bagian
6.1) — kandidat pertama kueri audit A.

### 7.2 Catatan lain

| Hal | Isi |
| --- | --- |
| Peringatan | *Diperbarui:* pemilik sudah membangun ulang (DLL 15:09) dan menjalankan uji API. Tidak ada `obj/efbuild` — pemilik membangun langsung ke `bin/Debug` karena backend sedang tidak berjalan |
| Masalah yang diketahui | (1) Kueri satu `JOIN`, bukan proyeksi kolom (bagian 3.3, keputusan 1). (2) Baris `EmergencyVisit : Create` pada layar Akses Role dapat menampilkan rute `GET` (bagian 3.3, keputusan 2) — kosmetik. (3) Pesan `409` `PesanEpisodeGanda` menulis nama enum berbahasa Inggris (*Triaged*) — sudah dicatat `FE-IGD-014`, di luar cakupan. (4) `blueprint-manifest.md` `artifact_hashes` untuk `api-contract.md` dan `validation-matrix.md` **belum dihitung ulang** (juga belum sejak `BE-IGD-049`); berkas itu di luar daftar berkas yang boleh disentuh task ini. (5) `00-interview-decisions.md` `IGD-DEC-138` masih menulis mekanisme *"menunggu konfirmasi pemilik atas laporan rencana"*, padahal pemilik sudah memutuskan lapis A (21 September 2026 malam); teks itu **tidak** diubah karena berkasnya di luar daftar yang boleh disentuh — keputusan tercatat pada roadmap, `MODULE-STATUS.md`, dan laporan ini |
| Risiko tersisa | Menengah, sesuai kartu: ini pintu masuk pasien. Endpoint baru **tidak menahan** pendaftaran karena baca-saja, tetapi bila layar (`FE-IGD-034`) memperlakukan kegagalan pra-cek sebagai penolak, pasien dapat tertahan — pemilik sudah memutuskan `fail-open`. Celah `IGD-OQ-093` tetap terbuka |
| Perubahan sampingan | `NONE` — tidak ada direktori `obj/efbuild/` (agent tidak membangun; pemilik membangun langsung ke `bin/Debug`). Nol berkas di luar daftar yang diizinkan |
| Interupsi | `NONE` |
| Status Git | Lihat keluaran `git status --short` pada respons akhir. Enam berkas source `M` (tiga milik `BE-IGD-049`, tiga milik task ini) dan dokumen `igd/**` `M`/`??`; sebagian sudah `M`/`??` sebelum task ini. Tidak ada stage atau commit |
| Langkah berikutnya | (1) ~~Build dan uji API S1–S7~~ — dilaporkan pemilik `PASS`. (2) **Kueri audit A/B (acceptance 8)** — jalankan pemilik, catat angkanya di sini; opsional: lampirkan jumlah warning, hitungan baris S4, dan baris log S5. (3) `FE-IGD-034` — dimulai 21 September 2026 (malam). (4) Putuskan `IGD-OQ-093` |
