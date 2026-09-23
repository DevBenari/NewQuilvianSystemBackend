# Laporan Perubahan Backend — `BE-BD-009`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BD-009` |
| Judul | Kantong `PendingReview` diselesaikan lewat tiga wewenang terpisah |
| Roadmap | [roadmap/backend-roadmap.md](../../../roadmap/backend-roadmap.md) bagian 5, revisi **11** |
| Trace | `DEC-BD-019`, `DEC-BD-028`, `DEC-BD-043`, `DEC-BD-045`; `INV-BD-013`, `INV-BD-020`, `INV-BD-028`, `INV-BD-034`, `INV-BD-035`; api-contract `v4` baris 114–116; state-transition §3; validation-matrix §2 (`VAL-BD-016`), §4b (`VAL-BD-064`), §4d (`VAL-BD-080/081/082`); `02-backend-architecture.md` §F.4 |
| Contract version | `v4` — **`approved`** |
| Dependency | `G1` ✅, `G2b` ✅, `BE-BD-006` ✅, `BE-BD-007` ✅ |
| Klasifikasi | `MEDIUM` — tiga endpoint, dua DTO baru, satu kolom baru pada entity yang sudah ada; nol entity baru, nol modul lain tersentuh |
| Target tulis | `DevBenari/NewQuilvianSystemBackend` cabang `sukmagp` |
| Model | `claude-opus-5` |
| Commit backend saat dikerjakan | Mulai dari `c597535c` (BE-BD-008), working tree bersih. Source dipertahankan pemilik lewat commit WIP `67980758` (`wip(bank-darah): preserve BE-BD-009 transfer state`) saat perpindahan sesi. Sisa perubahan (perbaikan DTO, migration, dokumen) **belum ter-commit** atas instruksi pemilik |
| Tanggal | 16–17 September 2026 |
| Status | ✅ **SELESAI 17 September 2026.** Kesembilan acceptance terbukti runtime lewat API sungguhan + verifikasi database; kelima kode `VAL-BD-016/064/080/081/082` terkirim pada `errors.code` dengan HTTP dan pesan persis `validation-matrix.md` (dibandingkan terprogram); otorisasi dibuktikan dua aktor non-SuperAdmin; migration terterapkan `0 pending`; build `0 Error(s)` / `198 Warning(s)`; QBE Strict `PASS`. Satu cacat source ditemukan runtime (`AC-BD-025`) dan diperbaiki di dalam berkas milik task ini |

---

## 1. Berkas implementasi

| Berkas | Sifat |
| --- | --- |
| `Areas/HealthServices/BloodBankManagement/DTOs/BloodUnitResolutionDtos.cs` | **BARU** — `ReallocateUnitRequest` (`BloodOrderLineId`, `ReasonCode`, `Version`) dan `ResolveWithReasonRequest` (`ReasonCode`, `Version`); nama persis kontrak |
| `Areas/HealthServices/BloodBankManagement/Services/BbkBloodUnitService.cs` | **DISENTUH** — `ReallocateAsync`, `ReturnToProviderAsync`, `MarkNotUsableAsync`, penolong bersama `ResolveOutOfCirculationAsync`, `BeginResolutionAsync`, `ResolveControlledReasonAsync`; tiga nama aksi pada `AvailableActions` untuk `PendingReview`; `SupersededReason` pada pembacaan bukti |
| `Areas/HealthServices/BloodBankManagement/Controllers/BbkBloodUnitController.cs` | **DISENTUH** — tiga endpoint, tiga `[AccessAction]` + `[AccessPermission]` dengan `DeniedCode` `VAL-BD-080/081/082`; `LogAllocationAsync` menerima nama aksi (bawaan tetap `Allocate`, pemanggil lama tidak berubah) |
| `Areas/HealthServices/BloodBankManagement/Models/BbkCompatibilityEvidence.cs` | **DISENTUH** — kolom `SupersededReason` `string(200)?` sesuai kamus data |
| `Repositories/Configurations/.../BbkCompatibilityEvidenceConfiguration.cs` | **DISENTUH** — `HasMaxLength(200)` |
| `Areas/HealthServices/BloodBankManagement/DTOs/CompatibilityEvidenceDtos.cs` | **DISENTUH** — `SupersededReason` pada DTO baca |
| `Migrations/20260917021029_AddBbkCompatibilityEvidenceSupersededReason*.cs` + snapshot | **BARU** |

### 1.1 Reuse, bukan penyalinan aturan

- **Gerbang alokasi.** `ReallocateAsync` memanggil `EvaluateAllocationGateAsync` apa adanya dan meneruskan
  `RuleCode`-nya ke `errors.code`. Aturan lokasi dan tonggak penyimpanan **tidak** ditulis ulang
  (§F.4: satu gerbang untuk `allocate` dan `reallocate`).
- **Pasien tujuan.** Divalidasi lewat `CheckOrderLineAsync` yang sama dengan alokasi biasa (baris ada, order
  masih berjalan, kunjungan belum berakhir). Body **tidak** menerima `PatientId`.
- **Alokasi.** Pengalihan menulis baris `BbkBloodUnitAllocation` baru untuk baris kebutuhan tujuan; alokasi
  asal yang masih aktif (bila ada) dilepas dengan alasan yang sama. Index unik terfilter satu-alokasi-aktif
  tetap menjadi penjaga terakhir (`TrySaveAsync` → `409 VAL-BD-018c`).
- **Riwayat.** `AppendTransition` yang sudah ada; append-only.
- **Pelaku** dari klaim akun yang login; **token `Version`** dihormati pada ketiga jalur.

### 1.2 Perilaku ketiga jalur

| Jalur | Dari → Ke | Kategori alasan sah | Gerbang lokasi |
| --- | --- | --- | --- |
| `reallocate` | `PendingReview` → `Reallocated` | `PendingReviewResolution` | **Ya** (`VAL-BD-063/064`) |
| `return-to-provider` | `PendingReview` → `ReturnedToProvider` (terminal) | `Return` | Tidak — arah mengeluarkan darah |
| `mark-not-usable` | `PendingReview` → `NotUsable` (terminal) | `NotUsable` | Tidak — arah mengeluarkan darah |

Status selain `PendingReview` (termasuk terminal) ditolak `422` sebelum apa pun ditulis, sehingga status
akhir tidak dapat dibuka kembali. Pada `reallocate`, **seluruh** bukti kecocokan yang masih berlaku pada
kantong ditandai `IsSuperseded = true` dengan `SupersededReason = "Kantong dialihkan ke pasien lain."`;
barisnya tidak dihapus (`DEC-BD-028`).

---

## 2. Migration

`20260917021029_AddBbkCompatibilityEvidenceSupersededReason`.

| Pemeriksaan | Hasil |
| --- | --- |
| Scope `Up` | Satu `AddColumn` `SupersededReason` `character varying(200)` nullable pada `public.BbkCompatibilityEvidence`. **Nol FK, nol index, nol operasi tabel lain** |
| Scope `Down` | Satu `DropColumn` kolom yang sama |
| Snapshot | Hanya tambahan properti yang sama (4 baris) |
| `has-pending-model-changes` | "No changes have been made to the model since the last migration." |
| `migrations list` sebelum | **Tepat satu** `(Pending)` — migration ini |
| `database update` | `Done.` ke `QuilvianNewDevSukma` |
| `migrations list` sesudah | **0 pending** |
| PostgreSQL | Kolom ada: `character varying`, panjang `200`, nullable |

`HostAbortedException` dari host design-time EF tercatat saat `migrations add` dan diabaikan — perintah
berakhir `Done`.

---

## 3. Temuan runtime dan keputusan

### 3.1 Cacat source `AC-BD-025` — ditemukan runtime, diperbaiki

Putaran pertama: `POST /return-to-provider` dengan `reasonCode: ""` mengembalikan `400`, tetapi berupa
`ProblemDetails` bawaan ASP.NET (`"The ReasonCode field is required."`) — **tanpa** `errors.code` dan
tanpa pesan kanonis. Sebabnya `[Required]`/`[MaxLength(30)]` pada `ReasonCode` di DTO baru: validasi model
otomatis menolak body sebelum service berjalan. Perilaku bisnis benar (tolak, nol mutasi), payload kontrak
salah, sehingga **tidak diklaim PASS**.

Perbaikan minimal pada berkas milik task ini saja: kedua atribut dilepas dari `ReasonCode` pada
`ReallocateUnitRequest` dan `ResolveWithReasonRequest`. Service sudah menolak kode kosong/spasi
(`BeginResolutionAsync`) dan kode yang tidak ada di master (`ResolveControlledReasonAsync`) dengan
`400 VAL-BD-016` secara fail-closed. `CancelWithReasonRequest` milik `BE-BD-006` **tidak disentuh**.
Sesudah build ulang, kasus kosong, field tidak dikirim, dan spasi seluruhnya `400 VAL-BD-016` persis
(bagian 5).

### 3.2 Sinkronisasi `api-contract` — `VAL-BD-016` = `400`

Baris `reallocate` pada api-contract semula menulis `422 VAL-BD-016/064`, sedangkan validation-matrix —
pemilik HTTP per kode — menetapkan `VAL-BD-016` `400` dan `VAL-BD-064` `422`. Implementasi mengikuti
validation-matrix (konsisten dengan perilaku `BE-BD-006` yang sudah tertutup). Runtime membuktikan
keduanya, sehingga api-contract diselaraskan: `400 VAL-BD-016` pada ketiga endpoint, `422 VAL-BD-064`
pada `reallocate`. Penyelarasan penulisan, bukan perubahan aturan.

### 3.3 Login aktor uji memerlukan lokasi

Akun karyawan tunduk geofence login. Klien uji mengirim koordinat rumah sakit yang terkonfigurasi pada
`LoginGeofence` — data yang sama dengan yang dikirim klien sungguhan. Penanda bypass geolokasi **tidak**
disentuh.

---

## 4. Runtime acceptance — 9 dari 9

Seluruhnya lewat API sungguhan terhadap `QuilvianNewDevSukma` + verifikasi database. Fixture
`TEST-BD009-20260917100009`. Kantong disingkat `-01`..`-08`.

| Acceptance | Kantong | Keadaan awal | Request | HTTP | `errors.code` | Keadaan database sesudah | Hasil |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `AC-BD-007` | `-02`, `-03` | `PendingReview` — order asal `ORD-00000086` dibatalkan lalu alokasi dibatalkan | `POST /allocate` ke baris order pasien lain (aktor pemegang `Allocate`) | `422` | — (lihat 7) | Tetap `PendingReview`, `Version` tidak bergeser, nol alokasi aktif. Pesan persis `VAL-BD-033` | ✅ |
| `AC-BD-008` | tujuh kantong | `PendingReview` | `GET /blood-units?unitStatus=PendingReview` (aktor operasional) | `200` | — | Ketujuh kantong fixture ada di daftar kerja #2 | ✅ |
| `AC-BD-024` | `-01` | `PendingReview`, pernah dialokasikan ke `ORD-00000086` | `POST /reallocate` ke baris `ORD-00000087`, alasan `TBD009-ALIH` (aktor klinis) | `200` | — | `Reallocated` (6), `Version` 5. Alokasi: baris lama `Cancelled` + `TBD009-BATALALOK` (pasien asal) → baris baru `Active` menunjuk baris order pasien tujuan. Transisi `PendingReview → Reallocated`, `TBD009-ALIH`, pelaku, waktu | ✅ |
| `AC-BD-025` | `-07`, `-02` | `PendingReview` | alasan `""` / field tidak dikirim / `"  "` pada ketiga endpoint | `400` | `VAL-BD-016` | Status dan `Version` tidak bergeser | ✅ (sesudah 3.1) |
| `AC-BD-029` | `-07`, `-02` | `PendingReview` | alasan diketik bebas (27 dan >30 karakter), kode tidak dikenal | `400` | `VAL-BD-016` | Status dan `Version` tidak bergeser | ✅ |
| `AC-BD-071` | `-08` | `PendingReview` di `TBD009-LOCB` yang **dinonaktifkan** | `POST /reallocate`, alasan sah, aktor klinis | `422` | `VAL-BD-064` | Tetap `PendingReview`, `Version` 1, **nol** baris alokasi | ✅ |
| `AC-BD-092` | `-05` | `PendingReview` | `POST /return-to-provider`, `TBD009-KEMBALI`, aktor operasional | `200` | — | `ReturnedToProvider` (7); transisi `PendingReview → ReturnedToProvider` dengan kode alasan, salinan teks, pelaku `1ee3b984…`, waktu | ✅ |
| `AC-BD-093` | `-06` | `PendingReview` | `POST /reallocate` oleh aktor **operasional** | `403` | `VAL-BD-080` | Status dan `Version` tidak bergeser | ✅ |
| `AC-BD-094` | `-01` | `PendingReview`, satu bukti `Compatible` terhadap pasien asal, belum gugur | `POST /reallocate` oleh aktor **klinis** | `200` | — | Bukti tetap 1 baris (tidak dihapus): `IsSuperseded = true`, `SupersededReason = "Kantong dialihkan ke pasien lain."`, `PatientId` = pasien asal. Terbaca juga lewat `GET /blood-units/{id}` | ✅ |

**Ketiga jalur sukses:** A `reallocate` (`-01` → `Reallocated`), B `return-to-provider` (`-05` →
`ReturnedToProvider`), C `mark-not-usable` (`-06` → `NotUsable`, `TBD009-TIDAKLAYAK`, aktor operasional,
transisi `PendingReview → NotUsable`).

---

## 5. Contract acceptance — 5 dari 5

Pesan diambil terprogram dari tabel `contracts/validation-matrix.md` dan dibandingkan dengan
`message` respons (spasi dinormalkan). Kode dibaca dari `errors.code`. Diverifikasi ulang pada assembly
build final.

| Kode | Endpoint yang dibuktikan | HTTP | `errors.code` | Pesan persis |
| --- | --- | --- | --- | --- |
| `VAL-BD-016` | ketiganya (kosong, tidak dikirim, spasi, teks bebas, kode tidak dikenal) | `400` ✅ | ✅ | ✅ |
| `VAL-BD-064` | `reallocate` | `422` ✅ | ✅ | ✅ |
| `VAL-BD-080` | `reallocate` | `403` ✅ | ✅ | ✅ |
| `VAL-BD-081` | `return-to-provider` | `403` ✅ | ✅ | ✅ |
| `VAL-BD-082` | `mark-not-usable` | `403` ✅ | ✅ | ✅ |

**Nol partial state.** Di sekitar empat penolakan terakhir, jumlah baris transisi / alokasi / bukti gugur
pada seluruh kantong fixture tetap `34 / 5 / 1` sebelum dan sesudah.

---

## 6. Otorisasi, konkurensi, dan penjaga keadaan

Dua aktor `Employee`, **tanpa role SuperAdmin**, masing-masing pada Department/Position `TEST-BD009`
tersendiri. Butir hak akses dipilih dari registry yang di-seed `AccessMenuSeeder` dari `[AccessAction]`
saat start — **nol action dummy**.

| Aktor | Policy | `reallocate` | `return-to-provider` | `mark-not-usable` |
| --- | --- | --- | --- | --- |
| Klinis `test.bd009.clin` | `Read`, `ResolveReallocate` | **`200`** | `403 VAL-BD-081` | `403 VAL-BD-082` |
| Operasional `test.bd009.ops` | `Read`, `ResolveReturn`, `ResolveNotUsable` | `403 VAL-BD-080` | **`200`** | **`200`** |

| Penjaga | Uji | Hasil |
| --- | --- | --- |
| Status akhir tertutup | `reallocate` atas kantong `ReturnedToProvider` | `422`, status 7 dan `Version` tetap |
| Kategori alasan | `return-to-provider` dengan kode kategori `NotUsable` | `422`, nol mutasi |
| Konkurensi optimistis | `mark-not-usable` dengan `version` usang | `409`, nol mutasi |
| Pasien tujuan tidak dipercaya | `reallocate` ke baris order yang sudah `Cancelled` | `422`, nol alokasi baru |
| Pelaku | `ActorUserId` riwayat = id akun yang login, bukan dari body | ✅ |

---

## 7. Regression minimal

| Uji | Hasil |
| --- | --- |
| `allocate` atas `PendingReview` | `422`, pesan `VAL-BD-033` — gerbang `BE-BD-006` utuh |
| `allocate` atas kantong di lokasi nonaktif | `422`, pesan `VAL-BD-064` — gerbang `BE-BD-015` utuh |
| `allocate` kantong `Available` ke order aktif | `200`, `Allocated` |
| `issue` kantong `Allocated` tanpa bukti | `422 VAL-BD-018` — gerbang `BE-BD-007` utuh |
| `issue` kantong `Reallocated` | `422 VAL-BD-017`; `IssuedAt` tetap kosong — bukti lama tidak membuka pemberian |
| Fixture `TEST-BD006/007/008` | `UpdateDateTime` terakhir 14/16 September 2026 — tidak tersentuh sesi ini |

---

## 8. Build dan QBE

| Gate | Hasil |
| --- | --- |
| `dotnet build` final (assembly yang diuji ulang) | **`Build succeeded`**, **`0 Error(s)`**, `198 Warning(s)` — sama dengan baseline `BE-BD-008`, nol warning baru |
| QBE Strict `GitRange` `c597535c..HEAD` | **`PASS`** — 6 berkas, `VIOLATION 0` / `REVIEW 0` / `INFO 0` |
| QBE Strict `WorkingTree` | **`PASS`** — `VIOLATION 0` / `REVIEW 0` / `INFO 0` |
| `git diff --check` | bersih |

> **Catatan build workstation.** Assembly proyek ini ±151 MB; Roslyn memerlukan puluhan GB. Server
> compiler bersama sempat membengkak ±20 GB dan paging, dan satu percobaan dengan batas heap GC gagal
> `Out of memory`. Build yang dipakai: `-m:1 -nodeReuse:false -p:UseSharedCompilation=false`, tanpa batas
> heap (±26–46 menit per build).

---

## 9. Fixture ledger `TEST-BD009` — belum dibersihkan

Seluruhnya dibuat lewat API. Prefix `TEST-BD009-20260917100009`.

**Master.** Komponen `TBD009-P` `125721b1-9d11-4cd7-9430-cd01b65bcfc2` (validity 24) · lokasi `TBD009-LOCA`
`78d0fa1b-87af-4e72-894c-ac339eabcd66` (aktif) dan `TBD009-LOCB` `10460a96-255b-4b21-b77a-a2c17f7f6414`
(**sengaja nonaktif**) · alasan `TBD009-ALIH` (`PendingReviewResolution`), `TBD009-KEMBALI` (`Return`),
`TBD009-TIDAKLAYAK` (`NotUsable`), `TBD009-BATALORD` (`OrderCancellationOperational`), `TBD009-BATALALOK`
(`AllocationCancellation`).

**Identitas.** Klinis: Department `ca7969d2-61c9-418b-b4af-26302f5098f4`, Position
`82ec345a-2f3a-4469-9071-3fd2e252e97e`, user `be87bf7d-aed5-4af8-8f56-4681487fd5f6`
(`test.bd009.clin@rsmmc.local`). Operasional: Department `b5c1a7ac-fa83-41f5-bc25-78413cc9b5b9`, Position
`9aa13928-939d-4c41-a8fe-a61c31b48976`, user `1ee3b984-e535-41f1-982d-56b29e663b13`
(`test.bd009.ops@rsmmc.local`). Masing-masing beserta employee/workforce profile/organisasi yang dibuat
endpoint employee · **lima baris `SysAccessPolicy`**.

**Transaksional.** Order `ORD-00000086` `16d3868e-ff53-4e77-a5a8-5e5d5392dffc` (**Cancelled**, asal) dan
`ORD-00000087` `c2eae7e2-5a9a-4dfb-8ff2-32cd3392f17c` (aktif, tujuan) — pasien dan kunjungan yang sudah ada
dibaca, tidak diubah · permintaan PMI `PMI-00000042` `582f4b57-73be-440a-a07e-14dc06c109dc` · delapan kantong
(4 biasa, 4 berlebih) beserta penempatan · 5 baris alokasi (2 aktif, 3 dibatalkan) · 1 bukti kecocokan
(gugur).

| Kantong | Id | Status akhir |
| --- | --- | --- |
| `-01` | `716879bc-ae1a-4257-92f7-3498724d4cf4` | `Reallocated` — alokasi aktif ke `ORD-00000087` |
| `-02` | `a587691e-1bae-4910-bea9-f2cb464c30ce` | `PendingReview` |
| `-03` | `45ae0cc6-9e87-4e77-b387-bcfd21af2127` | `PendingReview` |
| `-04` | `54ef3bf2-9534-4b02-a1f7-92580cdb5c18` | `Allocated` ke `ORD-00000087` (regresi) |
| `-05` | `e3c32fbc-e0d8-4ac6-b673-7917490e9d6e` | `ReturnedToProvider` (terminal) |
| `-06` | `4d221ec2-7efd-46d0-b4d0-9b594a9a1bf0` | `NotUsable` (terminal) |
| `-07` | `11334ee2-cac2-4769-be74-1ccd1b2de9ec` | `PendingReview` |
| `-08` | `6a4dc812-7ca9-4194-96a1-d80cdf642f28` | `PendingReview` di lokasi nonaktif |

**Cleanup belum dijalankan dan belum disetujui.** Dua kantong terminal; lima baris `SysAccessPolicy` dan dua
akun uji perlu dicabut terpisah.

---

## 10. Gap

### 10.1 Pre-existing — bukan regresi dan bukan milik task ini

- **`POST /allocate` tidak mengisi `errors.code`.** `AllocateAsync` (`BE-BD-006`) memulangkan pesan
  kanonis `VAL-BD-033`/`VAL-BD-064` tanpa kode. Perilaku bisnis `AC-BD-007` terbukti; celah amplop ini
  sama dengan temuan amplop `VAL-BD-016` yang sudah tercatat pada laporan `BE-BD-006` §9.6. `BE-BD-006`
  **tidak dibuka ulang**.
- `CancelWithReasonRequest` (`BE-BD-006`) masih memakai `[Required]`, sehingga pembatalan alokasi tanpa
  alasan memulangkan `ProblemDetails`, bukan amplop `VAL-BD-016`. Dicatat, tidak diubah.

### 10.2 Tersisa — keputusan kontrak di luar scope

- **Tidak ada jalur keluar dari `Reallocated`.** Matriks §3 menetapkan `PendingReview → Reallocated`,
  tetapi tidak mendefinisikan perpindahan sesudahnya. Pencatatan bukti dan pemberian menuntut `Allocated`,
  sehingga kantong `Reallocated` belum dapat diberikan kepada pasien tujuan (terbukti: `issue` →
  `422 VAL-BD-017`). Task ini mengimplementasikan matriks apa adanya dan **tidak** mengarang transisi baru.
  Perlu keputusan pemilik kontrak.
- **Cleanup fixture** `TEST-BD009` menunggu persetujuan pemilik.
- **Commit** belum dilakukan atas instruksi pemilik.
- `BE-BD-010` **tidak** dikerjakan dan **tidak** ditandai selesai.
