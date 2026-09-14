# Laporan Perubahan Backend — `BE-ACC-P2-017`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-ACC-P2-017` |
| Judul | API Jenis Kejadian |
| Slice | `P2-0b` — Wave A, batch 14 September 2026 |
| Roadmap | [`roadmap/backend-roadmap-phase2.md`](../../../roadmap/backend-roadmap-phase2.md), kartu `BE-ACC-P2-017` (revisi 3) |
| Trace | `FR-P2-007` (prasyarat); `ACC-DEC-045`, `ACC-DEC-075` |
| Contract version | `ACC-API-0.10` grup Event Type (masih `Rencana (belum tersedia)` di kontrak); `ACC-PERMISSION-0.5` `EventType : Read/Create/Update` |
| Dependency | `BE-ACC-P2-015` ✅ |
| Klasifikasi | `MEDIUM` — skor 6: repository 0, berkas diperiksa 1, berkas diubah 1 (4 berkas), logika bisnis 1, kontrak API 2 (grup endpoint baru), database 1 (query dan persistence, tanpa skema baru dari task ini), keamanan 1 (hak akses baru), UI 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — controller, service, DTO jenis kejadian, satu registrasi `Program.cs`; laporan ini, baris status roadmap, traceability |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `b3ab542e` (branch `rizkiG`), perubahan belum di-commit. Source ter-commit Rizki di `da1a4b6d`; diperbarui pada `917e97fd` |
| Tanggal | 14 September 2026 |
| Status | **🟡 SEBAGIAN** — 6 dari 6 acceptance terpetakan ke source. Dari dua butir verifikasi yang semula belum, **build ulang sesudah perbaikan log sudah terpenuhi** (0 error, 192 warning) dan migration `BE-ACC-P2-016` sudah diterapkan. **Sisa satu-satunya:** uji panggil `GET /event-types` dan `GET /posting-rules` saat backend berjalan — belum dapat dijalankan karena port 5107 tertutup saat diperiksa 14 September 2026 13.17 WIB. **Pembaruan 15.20 WIB:** backend berjalan dan kedua rute terbukti terdaftar (`401` tanpa login, rute kontrol `404`); respons `200` dengan login belum ada, jadi tetap 🟡 |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AccountingManagement` / `MasterData/EventType` |
| Registry | `Acc` — `ACTIVE`, `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 31 (salinan suite 1.17.1 tidak memuatnya — `ACC-TD-015`) |
| Keberlakuan | `NEW CODE` |
| Jenis capability | **Master data** — standar `master-data-endpoint-standard` berlaku; selisihnya dicatat bagian 4 |
| QBE yang berlaku | `QBE-SVC-001` (controller tanpa `DbContext`), `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-PAGE-001`, `QBE-OPT-001` (`/options` dikonsumsi form aturan posting), `QBE-DEL-001` (tanpa hapus — nonaktif) |

---

## 1. Masalah yang diperbaiki

Tabel `AccEventType` (`BE-ACC-P2-015`) belum punya jalan masuk. Tanpa API, administrator akuntansi
tidak dapat mendaftarkan jenis kejadian, dan form aturan posting (`BE-ACC-P2-018`,
`FE-ACC-P2-010`) tidak punya pilihan jenis kejadian untuk dipetakan.

Task ini **tidak** mengisi datanya. Daftar jenis kejadian yang sungguh diterbitkan Finance menunggu
`DEC-ACC-P2-002`; yang disediakan hanya tempat mengisinya.

---

## 2. Proses bisnis

### 2.1 Alur normal

| Langkah | Pelaku | Yang terjadi |
| ---: | --- | --- |
| 1 | Administrator akuntansi | Menambah jenis `PENGAKUAN-PIUTANG`, nama "Pengakuan piutang pasien", modul asal `Finance` |
| 2 | Sistem | Memeriksa badan hukum utama, panjang isian, kode kembar tanpa membedakan huruf besar kecil; menyimpan **aktif** → `201` |
| 3 | Administrator | Mengubah nama atau modul asal — **kode tidak dapat diubah** → `200` |
| 4 | Akuntansi | Memilih jenis ini pada form aturan posting lewat `GET /options` (hanya yang aktif) |
| 5 | Administrator | Menonaktifkan jenis yang tidak lagi diterima |

### 2.2 Aturan dan jalur tidak normal

| Keadaan | Hasil | Alasan |
| --- | --- | --- |
| Kode `pengakuan-piutang` padahal `PENGAKUAN-PIUTANG` sudah ada | `409` "Kode jenis kejadian pengakuan-piutang sudah dipakai." | Pencocokan dengan pesan kejadian lewat kode; dua kode yang hanya beda huruf membingungkan |
| Kode kosong atau lebih dari 50 karakter | `400` | Kamus data bagian 11 |
| Nama kosong / > 200, modul asal kosong / > 50 | `400` | Idem |
| Menonaktifkan `PENGAKUAN-PIUTANG` yang masih dipakai **1** aturan posting aktif milik PT Metropolitan Medical Centre | `409` "Jenis kejadian PENGAKUAN-PIUTANG masih dipakai 1 aturan posting aktif. Nonaktifkan aturan posting itu lebih dahulu." | Tidak boleh ada aturan aktif yang menunjuk jenis yang tidak diterima. Dihitung lintas **seluruh** badan hukum |
| Menonaktifkan jenis yang sudah nonaktif, atau mengaktifkan yang sudah aktif | `409` | — |
| Badan hukum utama tidak tunggal | `409` dari `AccountingLegalEntityGuard` | Sama dengan seluruh endpoint Accounting |
| Id tidak ada atau sudah dihapus | `404` | — |
| Tabel belum ada di database | `500` (`42P01`) | **Riwayat sampai 14 September 2026 siang.** Migration `BE-ACC-P2-016` kini sudah diterapkan di `QuilvianNewDevRizki`; keadaan ini hanya muncul pada database yang belum menerapkan `20260914044507` — misalnya database yang dibangun dari baseline integration. Lihat bagian 7 |

**Kenapa kode tidak dapat diubah.** Kejadian yang tertahan menyimpan `EventTypeCode` aslinya
(`ACC-DEC-075`). Mengganti `PENGAKUAN-PIUTANG` menjadi `PIUTANG-DIAKUI` akan membuat kejadian
tertahan itu tidak pernah menemukan jenisnya lagi.

**Kolom bantu `ActivePostingRuleCount`.** Daftar dan rincian menyebut jumlah aturan posting aktif
yang memakai jenis itu, supaya layar dapat menjelaskan **sebelum** tombol ditekan kenapa jenis
tertentu tidak dapat dinonaktifkan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu roadmap `015`, `017`, `018`; `ACC-API-0.10` grup Event Type (api-contract baris 472–487);
`permission-audit-matrix.md` baris 199–201; kamus data bagian 11; `ChartOfAccountController` dan
`AccChartOfAccountService` sebagai pola; `JournalController`, `RecurringJournalController` (pola
muatan `EntityId`); `Services/Logging/LoggerService.cs`; `AccountingServiceResult.cs`,
`AccountingLegalEntityGuard.cs`, `PagedResult.cs`, `IdentityModel.cs`; `Program.cs` blok
registrasi Accounting.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/AccountingManagement/MasterData/EventType/Controllers/EventTypeController.cs` | **Baru.** 7 action. **Diperbaiki sesi ini:** muatan log Update, Deactivate, Activate memakai `EntityId = id`, bukan `id` — lihat 3.4 |
| `Areas/Corporate/AccountingManagement/MasterData/EventType/Services/AccEventTypeService.cs` | **Baru.** Baca berhalaman, rincian, options, tambah, ubah, nonaktifkan, aktifkan |
| `Areas/Corporate/AccountingManagement/MasterData/EventType/DTOs/EventTypeDtos.cs` | **Baru.** `EventTypePagedQuery`, `EventTypeListResponse`, `EventTypeDetailResponse`, `EventTypeOptionResponse`, `CreateEventTypeRequest`, `UpdateEventTypeRequest` |
| `Program.cs` | `builder.Services.AddScoped<AccEventTypeService>();` dan satu `using`, di blok registrasi Accounting yang sudah ada |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Grup baru `api/v1/corporate/accounting/event-types` — 6 endpoint kontrak + 1 di luar kontrak (`activate`). Delta di bagian 4 |
| Database | Membaca dan menulis `AccEventType`, membaca `AccPostingRule`. Nol skema dari task ini. Tabel berdiri sejak migration `20260914044507_AddAccountingPostingRuleMaster` diterapkan Rizki, 14 September 2026 (`BE-ACC-P2-016` ✅) |
| Keamanan/Auth | `[AccessController]` `ControllerName = "EventType"`, modul `ACCOUNTING_MASTER_DATA`, urutan 4. Hak baru `EventType : Read/Create/Update` — **belum diberikan ke peran mana pun**; admin mencentangnya lewat layar Akses Role. Nol hardcode peran |

### 3.4 Perbaikan dalam sesi ini — pelaku log tertimpa

`LoggerService.WriteAsync` baris 76 menjalankan
`userId = GetValueFromData(data, "UserId", "Id") ?? userId;` — properti bernama `Id` pada muatan
**menggantikan** pengguna yang login. Muatan semula `new { id, request }` dan `new { id }`, sehingga
log Update, Deactivate, dan Activate mencatat **id jenis kejadian** sebagai `UserId`.

Contoh sebelum perbaikan: Budi (`UserId` `7c1e…`) menonaktifkan `PENGAKUAN-PIUTANG` (`Id` `a93f…`).
Log: `UserId="a93f…"`. Sesudah perbaikan: `UserId="7c1e…"`.

Diperbaiki dengan mengganti nama properti menjadi `EntityId` — pola `JournalController` dan
`RecurringJournalController`. Tiga baris kode dan satu komentar. Pola cacat yang sama pada
`ChartOfAccountController` dan `JournalTypeController` **tidak** disentuh — di luar cakupan,
dilaporkan pada `BE-ACC-P2-033` bagian 7.

---

## 4. Dokumentasi endpoint

#### Corporate / Accounting / Master Data / Event Type

Base URL `api/v1/corporate/accounting/event-types`.

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Daftar berhalaman; saring `isActive`, `sourceModule`, `search` (kode atau nama); urut `eventTypeCode` (bawaan), `eventTypeName`, `sourceModule`, `createDateTime` | `EventType : Read` |
| `GET` | `/options` | Jenis **aktif** untuk isian pilihan form aturan posting; `search` opsional | `EventType : Read` |
| `GET` | `/{id}` | Rincian beserta jejak perubahan dan `ActivePostingRuleCount` | `EventType : Read` |
| `POST` | `/` | Menambah jenis kejadian; lahir aktif → `201` | `EventType : Create` |
| `PUT` | `/{id}` | Mengubah nama dan modul asal; kode tetap | `EventType : Update` |
| `PATCH` | `/{id}/deactivate` | Menonaktifkan; `409` bila masih dipakai aturan posting aktif | `EventType : Update` |
| `PATCH` | `/{id}/activate` | **Di luar kontrak.** Mengaktifkan kembali | `EventType : Update` |

### Delta kontrak dan standar master data

| # | Delta | Alasan |
| ---: | --- | --- |
| 1 | `PATCH /{id}/activate` ditambahkan | Tanpa pasangan ini jenis yang pernah dinonaktifkan tidak akan pernah dapat dipakai lagi. Pola `ChartOfAccountController.Activate`; hak `Update` sama dengan `deactivate` |
| 2 | Standar master data: `GET /filters/metadata`, `GET /summary`, `PATCH /{id}/status`, `DELETE /{id}` **tidak** dibuat | Kontrak `ACC-API-0.10` hanya mencantumkan 6 endpoint; keaktifan diubah lewat `activate`/`deactivate` (preseden daftar akun dan jenis jurnal); penghapusan tidak diputuskan untuk master yang dirujuk kejadian tertahan. **Kekurangan terhadap standar, dilaporkan** |
| 3 | `[Tags]` di source `Corporate / Accounting / Master Data / Event Type`; kontrak menulis `Corporate - Accounting - Master Data - Event Type` | Source mengikuti seluruh controller Accounting yang sudah ada (garis miring) |
| 4 | Nama DTO berakhiran `Response`; kontrak menulis `EventTypeListDto`, `EventTypeDetailDto`, `EventTypeOptionDto` | Konvensi source Accounting — selisih lama `ACC-GAP-004` |
| 5 | Bidang `ActivePostingRuleCount` pada daftar dan rincian | Lihat bagian 2.2 |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=false` oleh Rizki, 14 Sep 2026 — **sebelum** perbaikan 3.4 | `0 error`, 189 warning | `PASS` | Tangkapan layar owner |
| Build ulang sesudah perbaikan 3.4 — **pagi 14 Sep 2026, riwayat** | Belum dijalankan | `NOT RUN` | Perubahan hanya nama properti tipe anonim; perlu build owner sebagai bukti |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=false` oleh Rizki — **sesudah** perbaikan 3.4, migration `016`, dan merge `ba124bbb` | `0 error`, 192 warning. Satu-satunya warning Accounting yang terlihat, `AccJournalService.cs(381)`, berasal dari `0d4ad3adf` (3 Sep 2026); tidak ada warning dari berkas `MasterData/EventType/` pada keluaran yang dilaporkan owner | `PASS` | Terminal Rizki. Perbaikan termuat: `EventTypeController.cs` baris 90, 104, 127 memakai `EntityId = id` (diperiksa ulang sesi penutupan) |
| Pemeriksaan source — kecocokan hak akses | 7 action: argumen ke-1 `[AccessPermission]` = `EventType` = `ControllerName`; argumen ke-2 sama dengan argumen ke-1 `[AccessAction]` (`Read`, `Create`, `Update`); `AccessType` dari `AccessTypes` | `PASS` | `EventTypeController.cs` |
| Pemeriksaan source — hardcode peran | Nol `IsInRole`, nol nama peran | `PASS` | Grep |
| Pemeriksaan source — lapisan | Controller hanya memanggil `AccEventTypeService`; nol `ApplicationDbContext` di controller | `PASS` | Idem |
| Pemeriksaan kontrak | 6 endpoint `ACC-API-0.10` hadir dengan method, path, dan hak yang sama | `PASS` | Bagian 4 |
| **Pemanggilan endpoint sesudah `016` diterapkan** — pagi 14 Sep 2026, riwayat | Belum dapat dijalankan — tabel belum ada | `NOT RUN` | Menunggu `BE-ACC-P2-016` (Rizki) |
| Pemeriksaan backend berjalan — sesi penutupan, 14 Sep 2026 13.17 WIB | **Port 5107 tertutup.** `Test-NetConnection localhost -Port 5107` → `TcpTestSucceeded=False`; `curl http://127.0.0.1:5107/` gagal terhubung (exit 7); nol proses mendengarkan di 5107 maupun 7184 | — | Keluaran perintah pada sesi penutupan |
| Pemeriksaan ulang — 14 Sep 2026 15.20 WIB | **Backend berjalan** (PID 2368, mendengarkan 5107 dan 7184; Swagger `200`). Permintaan `GET` tanpa login ke `https://127.0.0.1:7184/api/v1/corporate/accounting/`: `event-types` **`401`**, `posting-rules` **`401`**, `event-types/options` **`401`**; rute kontrol `rute-tidak-ada` **`404`**. Port 5107 menjawab `307` ke HTTPS | `PASS` sebagian — membuktikan **rute terdaftar** dan penjaga login aktif; **belum** membuktikan tabel terbaca maupun bentuk respons, karena `401` terjadi sebelum query | `curl` agent |
| Pernyataan owner — 14 Sep 2026 | Backend dijalankan dan pemberian hak lewat Akses Role **sudah berjalan di database utama**. Di `QuilvianNewDevRizki` belum, karena **departemen di sana kosong** — kebijakan hak disimpan per Departemen + Jabatan (`SysAccessPolicy`), sehingga tidak ada yang dapat dicentang. Respons `200` belum diserahkan | Dicatat apa adanya | Chat owner |
| **Uji kontrak read-only `GET /event-types` dan `GET /posting-rules`** | Belum dijalankan — backend tidak berjalan. `016` ✅ sudah diterapkan, jadi penghalangnya kini hanya proses backend, bukan skema. Rencana ujinya: login, lalu `GET` saja — tanpa `POST`, `PUT`, atau `PATCH` | `NOT RUN` | Menunggu backend dijalankan |

Uji manual: `NOT RUN` — backend tidak berjalan saat diperiksa. Semula `NOT FEASIBLE` karena tabel
belum ada; alasan itu gugur sejak migration `016` diterapkan.

**Tidak dijalankan:** automated test (`ACC-DEC-081`); pemanggilan endpoint (backend tidak berjalan);
query database oleh agent (di luar wewenang).

---

## 6. Acceptance criteria dan Definition of Done

| # | Kriteria persis seperti roadmap | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Enam endpoint kontrak tersedia dengan `[AccessAction]` dan `[AccessPermission]` `EventType` | **Terpenuhi** | Bagian 4 dan 5 |
| 2 | Kode jenis kembar ditolak `409` | **Terpenuhi** | `CreateAsync` — `EventTypeCode.ToLower() == pembanding`, tanpa membedakan huruf |
| 3 | Penonaktifan jenis yang masih dipakai aturan posting aktif ditolak `409` | **Terpenuhi** | `DeactivateAsync` menghitung `AccPostingRule` aktif lintas badan hukum |
| 4 | Penjaga badan hukum utama dipanggil | **Terpenuhi** | `AccountingLegalEntityGuard.PeriksaAsync` di awal ketujuh method publik |
| 5 | Tambah, ubah, dan nonaktifkan tercatat `LoggerService` | **Terpenuhi** | `CatatAsync` pada Create, Update, Deactivate, Activate; pelaku kini benar (3.4) |
| 6 | Selisih terhadap standar endpoint master data dicatat sebagai delta kontrak pada laporan task | **Terpenuhi** | Bagian 4, delta 1–5 |

**Enam dari enam terpenuhi di source.** Yang menahan ✅ adalah kolom Verifikasi, bukan acceptance:

| Butir verifikasi / DoD | Hasil |
| --- | --- |
| Pemeriksaan source dan kontrak | **Ya** |
| `dotnet build … -p:RunAnalyzers=false` oleh Rizki | **Ya** — 0 error, 192 warning sesudah perbaikan 3.4. Riwayat: **sebagian** pagi hari, hijau hanya sebelum perbaikan |
| Pemanggilan endpoint sesudah `016` diterapkan | **Belum** — `016` ✅ sudah diterapkan, tetapi backend tidak berjalan (port 5107 tertutup, 14 Sep 2026 13.17 WIB). Riwayat: pagi hari `016` belum dibuat |
| Source berubah | **Ya** |
| Laporan task tertulis | **Ya** — berkas ini |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 192 warning build terakhir (sebelumnya 189) — tidak ada yang berasal dari berkas task ini pada keluaran yang dilaporkan owner |
| Masalah yang diketahui | **Tertutup di `QuilvianNewDevRizki` 14 September 2026:** endpoint tidak lagi menjawab `500` (`42P01`) karena `BE-ACC-P2-016` sudah diterapkan. **Tetap berlaku** pada database yang belum menerapkan `20260914044507`, termasuk yang dibangun dari baseline integration selama migration itu belum di-merge ke sana. Endpoint lain dan login tidak terdampak: startup tidak memanggil `Migrate()` |
| Risiko tersisa | (1) Hak `EventType : *` belum diberikan ke peran mana pun — layar `FE-ACC-P2-009` akan `403` sampai admin mencentangnya; hal yang sama berlaku untuk uji panggil, yang menuntut pengguna uji ber-hak `EventType : Read` dan `PostingRule : Read`. (2) Kode kembar dijaga pemeriksaan kode **dan** unique index; dua penyimpanan bersamaan dengan kode sama akan menghasilkan `500` dari unique index, bukan `409` — service ini tidak menerjemahkan `23505` seperti `AccPostingRuleService`. Kemungkinannya sangat rendah untuk master yang diisi manual. (3) Migration `016` belum ada di `origin/QuilvianIntegrationBackend` |
| Perubahan sampingan | `NONE` |
| Interupsi | Source ditulis sesi 14 September 2026 pagi; sesi berikutnya memverifikasi dan memperbaiki muatan log. Penutupan sore hari sempat terputus batas percakapan dan dilanjutkan dari keadaan terverifikasi (`917e97fd`, working tree bersih); sesi penutupan hanya menyunting dokumen |
| Status Git | Source task ini ter-commit Rizki di `da1a4b6d`. Sesi penutupan mengubah dokumen saja: laporan ini dan baris status `roadmap/backend-roadmap-phase2.md`. **Nol commit, push, stage, merge, rebase, atau migration oleh agent** |

### Langkah berikutnya

1. ~~**Rizki:** build ulang untuk menutup butir build sesudah perbaikan 3.4.~~ **Selesai 14 September 2026** — 0 error, 192 warning.
2. ~~**Rizki:** buat dan terapkan migration `BE-ACC-P2-016`.~~ **Selesai 14 September 2026** — `20260914044507`, lihat [`BE-ACC-P2-016`](BE-ACC-P2-016.md).
3. **Rizki:** jalankan backend (`dotnet run`, port 5107) dengan pengguna yang sudah diberi hak `EventType : Read` dan `PostingRule : Read` di Akses Role.
4. Agent lalu menjalankan uji kontrak **read-only**: login, `GET /api/v1/corporate/accounting/event-types` dan `GET /api/v1/corporate/accounting/posting-rules` — tanpa `POST`, `PUT`, atau `PATCH`. Bila keduanya menjawab `200` dengan bentuk `PagedResult` yang cocok dengan bagian 4 laporan ini dan bagian 4 [`BE-ACC-P2-018`](BE-ACC-P2-018.md), laporan ini diperbarui dan task naik ke ✅. Riwayat: rencana semula menyebut `POST /event-types`; dipersempit menjadi `GET` saja atas instruksi owner 14 September 2026.
