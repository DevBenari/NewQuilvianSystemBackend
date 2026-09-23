# Laporan Perubahan Backend — `BE-IGD-054`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-054` |
| Judul | Daftar Menunggu Triage terpadu (`GET triage-queue`) |
| Slice | `S2` · `EPIC IGD-11` · `MVP-7` |
| Roadmap | [backend-roadmap.md](../../../roadmap/backend-roadmap.md) bagian R3.13 |
| Trace | `FR-IGD-070`; `AT-IGD-167`; `IGD-DEC-139` butir 2 (encounter tanpa kunjungan adalah keadaan normal), `IGD-DEC-144` (daftar = tampilan turunan milik IGD, bukan antrean `TrxQueue`), `IGD-DEC-143` dan `IGD-DEC-128` (aksi per baris), `IGD-DEC-142` (pergi sebelum ditriage), `IGD-DEC-151` (rekam pengganti tampil apa adanya) |
| Contract version | API **`0.11.0`** §8.3.1; permission/audit **`0.5.0`** §7.1 baris `Read`; validation **`0.8.0`** §10.1 aturan 1 dan 2 dipakai sebagai rumus. Seluruh bagian encounter-first **`approved`** (`IGD-DEC-157`), terkunci hash. Task ini **tidak** mengubah berkas kontrak |
| Dependency | `BE-IGD-051` ✅ (22 September 2026) — menyediakan `EmergencyEpisodeRule`; `BE-IGD-055` ✅ dan `BE-IGD-052` ✅ (23 September 2026) tidak menahan task ini |
| Klasifikasi | `MEDIUM` — skor 6: cakupan repository 0, berkas diperiksa 1 (sepuluh berkas source), berkas diubah 0 (tiga berkas), logika bisnis 1 (gabungan dua asal baris + halaman di basis data), kontrak API 1 (memakai kontrak yang sudah disetujui, tidak mengubahnya), database 1 (hanya perilaku kueri; nol schema), keamanan/auth 1 (memakai izin `EmergencyVisit : Read` yang sudah ada), UI/workflow 1 (satu layar, `FE-IGD-035`) |
| Task mode | `BACKEND` — perintah pemilik 23 September 2026. Target tulis: source IGD dan `docs/module-blueprints/igd/`. **Tidak** ada wewenang `dotnet build`, membuat/menjalankan migration, menulis basis data, atau operasi Git; **tidak** menyentuh `Program.cs`, berkas Registrasi, berkas kontrak, frontend |
| Target tulis | `NewQuilvianSystemBackend` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `d86d5aab` pada branch `rizkiG`. Working tree sudah memuat pembaruan dokumen `BE-IGD-052` yang belum di-commit; perubahan task ini ditambahkan di atasnya |
| Tanggal | 23 September 2026 |
| Status | 🟡 **SEBAGIAN — 23 September 2026.** Implementation Complete: satu endpoint baca-saja, satu method service, dua DTO. **Belum:** `dotnet build` (acceptance 8, milik pemilik — `NOT RUN`) dan uji API S1–S11 (acceptance 1–6, `NOT RUN`). Nol schema, nol migration, nol `Program.cs` |

### Backend Governance Preflight

| Butir | Hasil |
| --- | --- |
| Governance terbaca | `AGENTS.md` dan `CLAUDE.md` backend; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md` dan `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `rules/backend/` suite skill 1.17.1; `rules/rule-output/` |
| Selisih governance | Tidak ada selisih baru sejak `BE-IGD-052`. Baris registry `Emg` tetap `ACTIVE / LEGACY` dan tidak berubah |
| Area / Module | `HealthServices` / `EmergencyInstallationManagement` |
| Owner / prefix registry | Emergency, prefix `Emg`, `ACTIVE / LEGACY`. Task ini **tidak** membuat entity operasional, jadi `QBE-MOD-002`/`QBE-MOD-003` tidak berlaku |
| Keberlakuan | `NEW CODE`: satu action controller, satu method service beserta pembantunya, dua DTO. `TOUCHED LEGACY`: tiga berkas yang sudah ada, seluruhnya **hanya bertambah** — nol baris dihapus atau diubah |
| QBE ID yang berlaku | `QBE-MOD-001` (capability tinggal di modul pemiliknya), `QBE-SVC-001` (seluruh kueri dan aturan ada di service; controller nol akses `DbContext` untuk action ini), `QBE-API-001` (boundary `ApiResponse`/`PagedResult` dan kode status yang sudah mapan), `QBE-PERM-001` (pasangan `[AccessAction]`+`[AccessPermission]` memakai resource yang sudah ada), `QBE-VAL-001` (halaman dan status antrean divalidasi sebelum kueri jalan), `QBE-DTO-001` (nol entity EF keluar sebagai kontrak — `EmgVisit` dan `RegPatientEncounter` diproyeksikan ke DTO), `QBE-PAGE-001` (paging dan pencarian memakai bentuk yang sudah mapan di modul) |
| Tidak berlaku | `QBE-ENT-001`…`003`, `QBE-CFG-001`/`002`, `QBE-ENUM-001`, `QBE-NAM-003`, `QBE-DB-001`/`002` (nol entity, nol configuration, nol enum, nol schema); `QBE-TXN-001` (baca-saja, nol tulisan); `QBE-LOG-001` dan `QBE-AUD-001` (nol perubahan state — tidak ada peristiwa yang perlu dicatat); `QBE-DEL-001` (nol jalur hapus); `QBE-OPT-001` (nol feed options); `QBE-CODE-001`…`006` (nol penomoran dokumen) |
| Branch | `rizkiG`, sejajar `origin/rizkiG` pada `d86d5aab`. Tidak di-commit, tidak di-push |
| Hardcode role | Tidak ada. Kewenangan sepenuhnya lewat `[AccessPermission]`; nol `IsInRole`, nama peran, nama departemen, atau `UserType`. `availableActions` **bukan** penentu kewenangan — ia hanya menyatakan aksi yang masuk akal bagi keadaan baris; setiap endpoint aksi tetap memeriksa izinnya sendiri |
| Endpoint standard | Transaksi, arketipe **worklist** baca-saja: satu `GET /triage-queue` berhalaman. Nol `GET /options`, nol `PATCH /{id}/status` generik, nol `DELETE` — sesuai `transaction-endpoint-standard` |

---

## 1. Masalah yang diperbaiki

Sejak perjalanan pasien IGD dibalik menjadi *encounter-first* (`IGD-DEC-139`), pasien yang baru
didaftarkan di loket **belum punya kunjungan IGD**. Yang ada hanya encounter Registrasi bertipe
`Emergency`. Padahal layar Triage Pasien hari ini hanya membaca `GET /emergency-visits`, yaitu
daftar kunjungan — sehingga pasien yang paling perlu ditriage justru **tidak kelihatan** di layar
triage sampai seseorang menekan Mulai Triage, dan tidak ada yang tahu harus menekannya untuk siapa.

Sebelum task ini, satu-satunya jalan keluar adalah layar menggabungkan dua sumber sendiri: memanggil
daftar kunjungan, lalu memanggil daftar encounter, lalu membuang encounter yang kunjungannya sudah
lahir. Cara itu mustahil dihalamani dengan benar — halaman 2 akan mengulang atau melompati pasien —
dan menggandakan rumus "encounter sudah berakhir" ke dalam kode layar, tempat ia pasti menyimpang.

Task ini menambahkan **satu** daftar terpadu yang dihitung di basis data: `GET /triage-queue`.

## 2. Proses bisnis

**Alur normal.** Petugas loket mendaftarkan pasien IGD. Encounter `Emergency` lahir tanpa kunjungan.
Perawat triage membuka layar Triage Pasien, yang memanggil `GET /triage-queue`. Pasien tadi muncul
sebagai baris berstatus *Menunggu Triage* dengan keterangan **Terdaftar 09.35** — bukan "Tiba 09.35",
karena waktu tibanya memang belum pernah dikonfirmasi siapa pun. Perawat menekan **Mulai Triage**
atau **Tangani Segera**; kunjungan lahir (`BE-IGD-055`); pada pemanggilan daftar berikutnya pasien
yang sama muncul **sekali saja**, kini sebagai baris kunjungan bernomor `IGD-…` dengan keterangan
**Tiba 09.35**.

**Dua jenis baris, satu bentuk.** Baris tanpa kunjungan membawa nomor encounter dan waktu terdaftar;
baris kunjungan membawa nomor kunjungan, status kunjungannya, dan waktu tiba. Layar tidak perlu tahu
asal barisnya: yang dibacanya adalah `availableActions`, dan tombol yang muncul mengikuti daftar itu.

**Jalur tidak normal yang ditangani.**

| Keadaan | Yang terjadi pada daftar |
| --- | --- |
| Encounter sudah berakhir — `Completed`, `Cancelled`, `NoShow`, `IsCancel`, atau salah satu dari `CancelledAt`/`CompletedAt`/`NoShowAt` terisi | Hilang dari daftar. Rumusnya satu-satunya milik `EmergencyEpisodeRule`, tidak disalin ulang |
| Encounter bertipe rawat jalan atau rawat inap | Tidak pernah muncul — daftar ini hanya membaca encounter `Emergency` |
| Pasien pergi sebelum ditriage lalu ditandai `NoShow` (`IGD-DEC-142`) | Hilang dari daftar pada pemanggilan berikutnya |
| Kunjungan sudah `Completed` atau `Cancelled` | Tidak muncul. Episodenya sudah tuntas, dan `BE-IGD-051` sudah menutup encounter-nya |
| Pasien tanpa identitas (`IGD-DEC-151`) | Muncul dengan nama rekam penggantinya apa adanya; bila rekam itu belum bernama, dipakai alias sementara kunjungan, lalu keterangan bawaan. Kolom nama tidak pernah kosong |
| Encounter lama yang satu-satunya kunjungannya sudah dihapus lunak (kelas K4 `BE-IGD-052`) | **Tetap muncul** sebagai baris tanpa kunjungan, dan Mulai Triage akan menjawab `409`. Lihat bagian 7 |
| Encounter lama tanpa kunjungan sama sekali (kelas K3) | **Tetap muncul** sampai petugas pendaftaran membatalkannya — memang begitu keputusannya (`IGD-DEC-148`, risiko kartu R3.13) |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`EmergencyVisitController.cs`, `EmergencyVisitService.cs`, `EmergencyEpisodeRule.cs`,
`EmergencyVisitDtos.cs`, `EmgVisit.cs`, `RegPatientEncounter.cs`, `MstPatient.cs`,
`EmergencyVisitStatus.cs`, `Responses/PagedResult.cs`, `QuilvianSystemBackend.csproj`.

### 3.2 Berkas yang berubah

Ketiganya **hanya bertambah** — nol baris dihapus, nol baris lama diubah.

| Berkas | Perubahan |
| --- | --- |
| `…/DTOs/EmergencyVisitDtos.cs` | +94 baris: `EmergencyTriageQueueQuery` (`page`, `pageSize`, `search`, `queueStatus`) dan `EmergencyTriageQueueRowResponse` (15 ruas sesuai kontrak §8.3.1) |
| `…/Services/EmergencyVisitService.cs` | +270 baris: `GetTriageQueueAsync`, pembantu `ToTriageQueueRow`, `AksiBarisAntrean`, `ResolveNamaPasienAntrean`, bentuk baris bersama `BarisAntreanTriage`, delapan konstanta, dan satu `using` (`QuilvianSystemBackend.Responses`) |
| `…/Controllers/EmergencyVisitController.cs` | +29 baris: action `TriageQueue`, diletakkan di antara `GetAll` dan `GetById` |

**Sebelas selisih terhadap kartu dan kontrak** — dicatat apa adanya, bukan didiamkan.

| # | Selisih | Dasar |
| ---: | --- | --- |
| 1 | Baris kunjungan dibatasi `VisitStatus` **bukan** `Completed` dan **bukan** `Cancelled`. Kartu hanya menyebut "kunjungan" tanpa batas status | Klausa B validation §10.1 aturan 2 — definisi *episode terbuka* yang sudah disetujui. Tanpa batas ini, layar bernama "Menunggu Triage" akan memuat seluruh riwayat kunjungan IGD |
| 2 | Parameter halaman bernama `page`, bukan `pageNumber` seperti endpoint lain modul ini | Kontrak §8.3.1 mengunci `page`. Perbedaan ini **disengaja** dan perlu diketahui `FE-IGD-035` |
| 3 | Halaman tidak sah **ditolak `400`**, bukan dinormalkan diam-diam seperti `NormalizePaging` pada endpoint lain | Kartu dan kontrak mencantumkan `400` "parameter halaman tidak sah" |
| 4 | `queueStatus` yang tidak dikenal juga ditolak `400`, dengan pesan yang menyebutkan nilai yang sah | Perluasan `QBE-VAL-001`; kontrak hanya menyebut `400` untuk parameter halaman. Alternatifnya — mengembalikan daftar kosong — menyembunyikan salah ketik |
| 5 | Baris kunjungan berstatus selain `Arrived`/`WaitingForTriage` mendapat `availableActions` **kosong** | Kartu aturan 4 hanya mendefinisikan dua kasus. Purwarupa layar menampilkan tombol "Buka" untuk baris `Sedang ditangani`, tetapi aksi itu tidak ada di kontrak dan tidak dikarang di sini |
| 6 | `registeredAt` baris kunjungan diambil dari encounter-nya; untuk kunjungan lama **tanpa** encounter dipakai `arrivalDateTime` | Kontrak menuntut `registeredAt` selalu terisi, sementara `encounterId` boleh kosong untuk kunjungan lama |
| 7 | `visitStatus` diketik `EmergencyVisitStatus?`, bukan `int?` | Terserialisasi sebagai angka karena proyek tidak memasang `JsonStringEnumConverter`, jadi bentuk JSON-nya sama dengan kontrak sekaligus konsisten dengan DTO lain modul ini |
| 8 | Urutan daftar: waktu **menurun**, ditutup kunci baris sebagai pemecah seri | Kartu dan kontrak tidak menetapkan urutan. Menurun mengikuti purwarupa layar dan bawaan `GET /emergency-visits`; pemecah seri wajib ada, kalau tidak baris berwaktu sama dapat berpindah halaman di antara dua permintaan dan melanggar acceptance 5 |
| 9 | Baris tanpa kunjungan selalu `isUnknownPatient = false` dan `temporaryPatientAlias` kosong | Kontrak menyatakan kedua ruas itu "dari kunjungan"; sebelum kunjungan lahir keduanya memang belum ada |
| 10 | Pencarian pada baris kunjungan juga mencakup `temporaryPatientAlias` | Empat ruas kontrak (nama, RM, nomor encounter, nomor kunjungan) tidak dapat menemukan rekam pengganti yang baru beralias. Tambahan kecil, tidak mengurangi apa pun |
| 11 | Baris tanpa kunjungan disaring dengan "tidak punya kunjungan yang **belum dihapus**", sehingga kelas K4 tetap muncul | Mengikuti bunyi kartu aturan 1–2 dan klausa B yang keduanya memakai `IsDelete = false`. Akibatnya dicatat sebagai masalah yang diketahui, bagian 7 |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Bertambah satu endpoint** yang sudah ada di kontrak `0.11.0` §8.3.1 sebagai *Rencana*; berkas kontrak **tidak** diubah oleh task ini. `GET /emergency-visits` dan seluruh endpoint lain tidak berubah sama sekali |
| Database | `NOT APPLICABLE` untuk schema: nol entity, nol configuration, nol `DbSet`, nol migration. Dampaknya hanya perilaku kueri — dua kueri baca-saja per permintaan (`COUNT` dan satu halaman) atas `EmgVisit`, `RegPatientEncounter`, dan `MstPatient` |
| Keamanan/Auth | Memakai resource izin yang **sudah ada**, `EmergencyVisit : Read` — nol aksi izin baru, jadi tidak ada yang perlu dicentang ulang di layar Akses Role. Daftar ini menampilkan nama pasien dan nomor rekam medis, yaitu ruas yang sudah ditampilkan `GET /emergency-visits` kepada pemegang izin yang sama |

---

## 4. Dokumentasi endpoint

#### Health Services / Emergency Installation Management / Emergency Visit

Base URL: `api/v1/health-services/emergency-installation-management/emergency-visits`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/triage-queue` | Daftar *Menunggu Triage* terpadu: encounter IGD yang belum berakhir dan belum punya kunjungan, digabung dengan kunjungan yang episodenya masih terbuka | `EmergencyVisit : Read` |

**Query.** `page` (bawaan `1`, minimal `1`), `pageSize` (bawaan `20`, `1`–`100`), `search`
(nama pasien, nomor rekam medis, nomor encounter, nomor kunjungan, alias sementara),
`queueStatus` (`WaitingForTriage` menyaring kedua jenis baris; nilai `EmergencyVisitStatus` lain
menyaring baris kunjungan saja).

**Respons `200`.** `PagedResult<EmergencyTriageQueueRowResponse>` di dalam `ApiResponse`. Tiap baris
memuat `rowKey`, `encounterId`, `emergencyVisitId`, `patientId`, `patientName`,
`medicalRecordNumber`, `isUnknownPatient`, `temporaryPatientAlias`, `encounterNumber`,
`emergencyVisitNumber`, `queueStatus`, `visitStatus`, `registeredAt`, `arrivalDateTime`, dan
`availableActions`.

**Kode status.** `200`; `400` bila `page`, `pageSize`, atau `queueStatus` tidak sah; `401` tanpa
token; `403` tanpa `EmergencyVisit : Read`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Pembacaan diff dan scope | Tiga berkas, **465 baris bertambah, 0 baris dihapus**; nol `Program.cs`, nol `Migrations/`, nol berkas Registrasi, nol berkas kontrak, nol frontend | `PASS` | `git diff --stat` |
| Nol baris komentar baru | Nol baris `//` pada seluruh baris yang bertambah; penjelasan ditulis sebagai dokumentasi `///` | `PASS` | `git diff -U0 \| grep "^+" \| grep "//"` |
| Pemetaan kontrak | Kelima belas ruas §8.3.1, keempat parameter query, dan keempat kode status dipetakan satu per satu ke source; sebelas selisih dicatat pada bagian 3.2 | `PASS` | Pembacaan silang kontrak dan source |
| Pemeriksaan izin | `[AccessController(ControllerName = "EmergencyVisit")]`; `[AccessPermission("EmergencyVisit", "Read")]` — argumen pertama sama persis dengan `ControllerName`, argumen kedua sama persis dengan argumen pertama `[AccessAction]` pada method yang sama | `PASS` | Pembacaan source |
| Rumus episode tidak disalin | `GetTriageQueueAsync` memanggil `EmergencyEpisodeRule.EncounterNotEnded`; nol pengulangan lima tanda berakhir di berkas mana pun yang disentuh | `PASS` | `grep` pada berkas yang berubah |
| Pemeriksaan rahasia | Nol credential, token, atau connection string pada source dan laporan | `PASS` | Pembacaan diff |
| `dotnet build` | Belum — milik pemilik | `NOT RUN` | Larangan task; bagian 5.1 |
| Uji API S1–S11 | Belum — butuh aplikasi berjalan | `NOT RUN` | Bagian 5.2 |
| Jumlah kueri per permintaan (acceptance 6) | Belum diukur dari log SQL; secara source tepat dua `await` ke basis data dan nol pemuatan navigasi per baris | `NOT RUN` | Bagian 5.2 skenario S11 |

Uji manual: `REQUIRED` — skenario 5.2, sesudah build berhasil.

**Tidak dijalankan:** `dotnet build`, `dotnet ef`, kueri basis data, dan aplikasi — seluruhnya di
luar wewenang task ini. Nol proyek test backend sejak 11 September 2026.

### 5.1 Perintah build untuk pemilik

```bash
dotnet build ./QuilvianSystemBackend.sln -p:RunAnalyzers=false
```

Yang perlu diperhatikan pada keluarannya: **0 error**, dan **jumlah warning** supaya dapat
dibandingkan dengan baseline — angka inilah yang membuat acceptance 10 `BE-IGD-055` dan acceptance 9
`BE-IGD-052` hanya terpenuhi sebagian. Titik paling mungkin gagal adalah penggabungan dua proyeksi
(`Concat`) di `GetTriageQueueAsync`; bila EF menolak menerjemahkannya, galatnya muncul **saat
permintaan pertama**, bukan saat build.

### 5.2 Skenario uji untuk pemilik

Token yang memegang `EmergencyVisit : Read` — hak yang **sudah ada**, tidak perlu dicentang ulang.

| # | Langkah | Hasil yang diharapkan | Acceptance |
| --- | --- | --- | --- |
| S1 | `GET /triage-queue` pada lingkungan yang punya encounter IGD tanpa kunjungan | `200`; baris itu muncul dengan `queueStatus = "WaitingForTriage"`, `rowKey` berawalan `enc:`, `registeredAt` terisi, `arrivalDateTime` **kosong**, `emergencyVisitId` **kosong** | 1 |
| S2 | Jalankan `POST /start-triage` pada baris S1, lalu panggil ulang `GET /triage-queue` | Pasien yang sama muncul **tepat sekali**, kini `rowKey` berawalan `visit:`, `arrivalDateTime` terisi, dan baris `enc:` lamanya hilang | 2 |
| S3 | Tandai satu pasien `NoShow` (`POST /no-show`), lalu panggil ulang. Periksa juga satu encounter rawat jalan | Keduanya **tidak** muncul | 3 |
| S4 | Periksa `availableActions` pada tiga baris: tanpa kunjungan, kunjungan `WaitingForTriage`/`Arrived`, kunjungan `InTreatment` | Berturut-turut `["StartTriage","ImmediateCare","NoShow"]`; `["FillTriage","ImmediateCare"]`; **kosong** | 4 |
| S5 | `GET /triage-queue?pageSize=2` lalu susuri `page=1` sampai halaman terakhir | Gabungan seluruh `rowKey` = `totalData`, **tanpa pengulangan dan tanpa yang terlewat**; `totalPage` sesuai | 5 |
| S6 | `page=0`; `pageSize=0`; `pageSize=101`; `queueStatus=Ngawur` | Keempatnya `400` dengan pesan yang menyebutkan batasnya; untuk `queueStatus`, pesannya menyebutkan daftar nilai yang sah | — |
| S7 | `search` dengan nama pasien, lalu nomor rekam medis, lalu nomor encounter, lalu nomor kunjungan | Masing-masing menemukan barisnya; pencarian tidak membedakan huruf besar-kecil | — |
| S8 | `queueStatus=WaitingForTriage`, lalu `queueStatus=InTreatment` | Yang pertama memuat **kedua** jenis baris; yang kedua **hanya** baris kunjungan | — |
| S9 | `GET /emergency-visits` dengan parameter yang biasa dipakai layar lain | Balasan **sama persis** seperti sebelum task ini | 7 |
| S10 | Token tanpa `EmergencyVisit : Read` | `403` | — |
| S11 | Nyalakan log SQL EF, panggil `GET /triage-queue` pada data yang punya puluhan baris, lalu pada data yang punya ratusan | **Dua** perintah SQL pada kedua kasus — satu `COUNT`, satu halaman. Jumlahnya **tidak** bertambah mengikuti jumlah baris | 6 |

---

## 6. Acceptance criteria dan Definition of Done

| No | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Encounter Emergency belum berakhir tanpa kunjungan tampil `WaitingForTriage` dengan `registeredAt`, tanpa `arrivalDateTime` | Belum terpenuhi — source ada | Proyeksi baris encounter mengisi `RegisteredAt` dari `RegPatientEncounter.RegisteredAt` dan menetapkan `ArrivalDateTime` kosong; `QueueStatus` dibentuk dari `nameof(EmergencyVisitStatus.WaitingForTriage)`. Uji S1 `NOT RUN` |
| 2 | Satu episode tidak pernah tampil dua kali | Belum terpenuhi — source ada | Baris encounter disaring `!Any(kunjungan belum dihapus untuk encounter itu)`; baris kunjungan tidak pernah memuat encounter-nya sebagai baris kedua. Uji S2 `NOT RUN` |
| 3 | Encounter berakhir dan tipe lain tidak tampil | Belum terpenuhi — source ada | `.Where(EncounterType == Emergency)` dan `.Where(EmergencyEpisodeRule.EncounterNotEnded)` — rumus milik `BE-IGD-051`, bukan salinan. Uji S3 `NOT RUN` |
| 4 | `availableActions` sesuai aturan 4 | Belum terpenuhi — source ada | `AksiBarisAntrean` mengembalikan tiga aksi untuk baris encounter dan dua aksi untuk kunjungan `Arrived`/`WaitingForTriage`; status lain kosong (selisih 5 bagian 3.2). Uji S4 `NOT RUN` |
| 5 | Gabungan halaman 1..n = seluruh baris, tanpa ulang, tanpa loncat | Belum terpenuhi — source ada | Urutan `OrderByDescending(Urutan).ThenByDescending(KunciUrutan)` ditutup kunci baris yang unik, lalu `Skip`/`Take` di basis data. Uji S5 `NOT RUN` |
| 6 | Jumlah kueri tetap berapa pun jumlah baris; nol `N+1` | Belum terpenuhi — source ada | Tepat dua pemanggilan basis data (`CountAsync`, `ToListAsync`); seluruh ruas diproyeksikan di dalam kueri, nol `Include`, nol pemuatan lambat per baris. Uji S11 `NOT RUN` |
| 7 | `GET /emergency-visits` tidak berubah; nol schema, nol migration | **Terpenuhi — terbukti agent** | `git diff` memperlihatkan **hanya penambahan**: action `GetAll` beserta seluruh kode lamanya tidak tersentuh; nol berkas di `Migrations/`, `Models/`, dan `Repositories/Configurations/`. Sisi uji API-nya tetap dianjurkan (S9) |
| 8 | Build 0 error, warning sama dengan baseline | Belum terpenuhi — **milik pemilik** | Bagian 5.1 |

**DoD.** Acceptance 7 terbukti agent; acceptance 1–6 dan 8 menunggu build dan uji API pemilik.
Laporan tracked ada (berkas ini). `FE-IGD-035` boleh mulai **sesudah build terverifikasi**, sesuai
DoD kartu.

**Status terpisah.** `IMPLEMENTATION STATUS` = **COMPLETE**. `DEVELOPER VERIFICATION STATUS` =
**BELUM** — build dan uji API milik pemilik. `UAT STATUS` = **BELUM DIJALANKAN**, diserahkan ke tim
UAT dan bukan penghalang task berikutnya.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Penggabungan dua proyeksi (`Concat`) adalah satu-satunya bagian yang bisa gagal **saat dijalankan** walau build berhasil: bila EF Core menolak menerjemahkan salah satu sisi, permintaan pertama menjawab `500`. Karena itu S1 wajib dijalankan lebih dulu sebelum skenario lain |
| Masalah yang diketahui | (1) Encounter kelas **K4** — satu-satunya kunjungannya dihapus lunak — tetap muncul sebagai baris tanpa kunjungan, dan Mulai Triage menjawabnya `409` karena unique index `EmgVisit.EncounterId` tidak menyaring baris terhapus (`IGD-EV-143` butir 7). Perawat melihat baris yang tidak dapat ditindaklanjuti. Menyembunyikannya menuntut keputusan pemilik, jadi **tidak** dilakukan sendiri di sini. (2) Baris kunjungan berstatus lanjut, misalnya `InTreatment`, muncul tanpa satu pun aksi; purwarupa layar menyiratkan tombol "Buka", tetapi aksi itu belum ada di kontrak |
| Risiko tersisa | Daftar ini menjadi **satu-satunya** sumber layar Triage Pasien (`FE-IGD-035`). Bila saringannya salah, pasien hilang dari layar triage — bukan sekadar tampil ganda. Karena itu acceptance 1, 2, 3, dan 5 sebaiknya diuji pada data yang benar-benar memuat kelima keadaan bagian 2, bukan hanya satu pasien uji |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M` pada tiga berkas source IGD, ditambah tiga berkas dokumen `BE-IGD-052` yang sudah terbuka sebelumnya, ditambah laporan ini dan penandaan roadmap/traceability. Belum di-commit, belum di-push |
| Langkah berikutnya | (1) Pemilik: `dotnet build`, lalu uji S1–S11 bagian 5.2 — S1 dan S11 paling menentukan. (2) `FE-IGD-035` boleh mulai sesudah build terverifikasi. (3) Antrean berikutnya R3.14: `BE-IGD-060` (membawa migration `AddEmergencyVisitClosureSource`), lalu `BE-IGD-061`/`062`/`063` paralel, lalu `FE-IGD-041`. (4) Bila pemilik ingin baris K4 disembunyikan dari daftar, itu keputusan baru — kartu dan kontrak sekarang menuntutnya tetap tampil |
