# `BE-ACC-015` — Nama aktor pada respons Journal

- **TASK ID:** `BE-ACC-015` — Nama aktor pada respons Journal
- **Menutup:** `ACC-GAP-011`
- **Blueprint:** `ACC-BP-001` revisi 9
- **Kontrak:** `ACC-API-0.4` → **`ACC-API-0.5`**
- **Branch:** `rizkiG` @ `f31a5d8`, source baseline `822d48a`
- **Task mode:** `BACKEND`
- **Status:** **`DONE`** — 4 September 2026
- **Keputusan owner:** pilihan **A** dari tiga pilihan yang diajukan pada audit `ACC-GAP-011`

## Ringkasan untuk pembaca umum

Layar rincian jurnal perlu menjawab **"siapa menyetujui apa dan kapan"** — itu alasan riwayat
persetujuan ada, dan kalimat itu tertulis di `JournalApprovalAction.cs` sendiri.

Sebelum task ini, respons hanya memuat `Guid` aktornya. Layar akan menampilkan
`0ba84a1a-2559-49ba-a320-10fb1f399d70`, bukan nama orang. Task ini menambahkan namanya.

## 1. Backend Governance Preflight

| Field | Nilai |
|---|---|
| Area | `Corporate` |
| Module | `AccountingManagement` |
| Submodule | `JournalManagement` |
| Keberlakuan | `NEW CODE` (penambahan pada modul yang sudah ada) |
| Entity baru | **Nol** |
| Model persisted baru | **Nol** |
| Registry `Acc` di backend | **Belum ada** — `ACC-DEP-007`, `OPEN` |

**`QBE-MOD-002` dan `QBE-MOD-003` tidak berlaku pada task ini.** Keduanya menggerbangi
*pembuatan entity/model persisted*, dan task ini membuat **nol** di antaranya. `ACC-DEP-007` tetap
`OPEN` dan tetap menahan **merge ke integration**, bukan penulisan kode lokal — posisi yang sudah
tercatat di `MODULE-STATUS.md`.

### QBE ID yang benar-benar berlaku

| ID | Ketentuan | Kepatuhan |
|---|---|---|
| `QBE-ENT-003` | MUST NOT menambah field persisted yang murni kebutuhan presentasi | **PATUH, dan justru menentukan rancangannya.** Nama ditambahkan pada **DTO**, bukan pada tabel. Menyimpan nama di baris riwayat akan melanggar aturan ini sekaligus membuat data basi saat pengguna berganti nama |
| `QBE-DTO-001` | MUST tidak mengekspos entity EF sebagai kontrak API | **PATUH.** `ApplicationUser` tidak bocor; hanya `string?` yang diekspos |
| `QBE-SVC-001` | MUST service memiliki orkestrasi; controller tidak menyentuh context | **PATUH.** Seluruh pencarian nama berada di `AccJournalService`; controller tidak disentuh |
| `QBE-API-001` | MUST memakai boundary API dan response yang mapan | **PATUH.** Nol endpoint berubah, nol perubahan bentuk amplop |
| `QBE-AUD-001` | MUST audit database terpisah dari application logging | **PATUH.** `AccJournalApproval` tidak disentuh |

Tidak berlaku: `QBE-ENT-001/002`, `QBE-NAM-*`, `QBE-CFG-*`, `QBE-MOD-*`, `QBE-CODE-*`,
`QBE-DB-*`, `QBE-DEL-001` — seluruhnya menggerbangi entity, penamaan tabel, alokasi nomor, atau
migration legacy, dan task ini tidak menyentuh satu pun.

## 2. Status migration dan database

| Wewenang | Keadaan |
|---|---|
| Implementasi source | **DIPAKAI** |
| Pembuatan migration | **TIDAK DIPAKAI — dan memang tidak dibutuhkan** |
| Eksekusi database | **TIDAK DIPAKAI** |
| Deployment | **TIDAK DIPAKAI** |

Alasannya diverifikasi lebih dahulu, bukan diasumsikan:

| Yang diperiksa | Hasil |
|---|---|
| `ApplicationDbContext` | `: IdentityDbContext<ApplicationUser, ApplicationRole, Guid>` → `.Users` sudah tersedia |
| `AccJournalService._db` | sudah bertipe `ApplicationDbContext` |
| `ApplicationUser.DisplayName` / `.UserCode` | sudah ada sebagai properti entity |
| `ActionBy`, `SubmittedBy`, `ApprovedBy`, `PostedBy` | sudah tersimpan sebagai kolom `Guid` |

Nol kolom baru, nol tabel baru, nol perubahan `ModelSnapshot`. **Migration tidak dibuat.**

## 3. Cakupan ternyata 4 field, bukan 1

`ACC-GAP-011` semula mencatat satu field, `ActionByName`. Pemeriksaan terhadap
`JournalDetailResponse` menemukan **tiga `Guid` aktor lain** yang bermasalah sama:
`SubmittedBy`, `ApprovedBy`, `PostedBy`. Keempatnya ditangani sekaligus — memperbaiki satu dan
meninggalkan tiga akan menyisakan layar yang setengah terbaca.

## 4. Berkas yang berubah

Tepat dua berkas source, sesuai batas yang ditetapkan owner.

| Berkas | Perubahan |
|---|---|
| `Areas/Corporate/AccountingManagement/JournalManagement/DTOs/JournalDtos.cs` | +4 properti `string?` beserta keterangannya |
| `Areas/Corporate/AccountingManagement/JournalManagement/Services/AccJournalService.cs` | Helper `AmbilNamaAktorAsync`, `PilihNamaPertama`, `NamaAktor`; pemetaan pada `PetakanRincianAsync` |

Nol perubahan pada entity, configuration, `Migrations/`, `ModelSnapshot`, `Program.cs`, controller,
dan modul lain.

## 5. Keputusan teknis

### Satu kueri, bukan N+1

Seluruh `Guid` aktor pada satu jurnal — baris riwayat ditambah tiga aktor pada kepala —
dikumpulkan, di-`Distinct()`, lalu diambil dalam **satu** `ToListAsync`. Jurnal dengan 20 baris
riwayat tetap menghasilkan satu kueri.

### Pemilihan nama dikerjakan di memori, bukan di SQL

Preseden `NurseStationClusterController` memakai `DisplayName ?? UserName ?? Email ?? UserCode`
di dalam `Select`. Rantai `??` itu **tidak menangani string kosong** — dan `DisplayName` bertipe
`string` non-nullable, sehingga nilai `""` akan lolos sebagai nama yang sah.

Karena itu keempat kandidat diambil apa adanya, lalu dipilih di memori dengan
`!string.IsNullOrWhiteSpace`. Alasan kedua: `string.IsNullOrWhiteSpace` tidak dijamin dapat
diterjemahkan provider. Jumlah aktor per jurnal paling banyak empat, jadi biayanya nol.

**Delta terhadap preseden dicatat, bukan didiamkan**: polanya sama, penanganan string kosongnya
lebih ketat.

### `null`, bukan string kosong

Aktor yang belum ada — `ApprovedByName` pada jurnal yang belum disetujui — mengembalikan `null`.
Frontend dengan begitu dapat membedakan *"belum disetujui"* dari *"disetujui oleh orang tanpa
nama"*.

## 6. Validasi yang benar-benar dijalankan

| Perintah | Hasil |
|---|---|
| `dotnet build ./QuilvianSystemBackend.sln` | **GAGAL** — `MSB3027`, `bin/Debug/net9.0/QuilvianSystemBackend.exe` terkunci proses backend yang sedang berjalan (PID 16980). **Bukan kegagalan kompilasi** |
| `BaseOutputPath=obj/beacc015/ dotnet build ./QuilvianSystemBackend.sln` | **Build succeeded — 0 Error(s)**, 23 warning (seluruhnya XML comment lama, bukan dari task ini) |
| Pembersihan | `obj/beacc015` dihapus |

Kegagalan pertama dilaporkan apa adanya. Ia terjadi karena backend sedang dipakai owner, dan
jalan keluarnya memakai direktori keluaran terpisah — cara yang menghindari menghentikan proses
yang sedang berjalan.

### Pembuktian terhadap data sungguhan

Dijalankan pada `QuilvianNewDevRizki` sebagai **`SELECT` baca-saja**, meniru join yang dilakukan
kode:

| Bukti | Hasil |
|---|---|
| `JB/2026/09/00001` → `SubmittedByName` | `'SuperAdmin'` |
| Riwayat → `ActionByName` | `'SuperAdmin'` untuk tindakan *Diajukan* |
| `ApprovedByName`, `PostedByName` | `null` — benar, jurnalnya memang belum disetujui |
| `AspNetUsers.DisplayName` | terisi pada **50 dari 50** pengguna; nol `NULL`, nol string kosong |

Acceptance (1), (2), (4), dan (5) **terbukti**. Acceptance (3) — satu kueri, bukan N+1 —
terbukti di kode, belum diukur pada jalur HTTP sungguhan.

## 7. `MANUAL TEST: NOT FEASIBLE` sebagian

Memanggil `GET /api/v1/corporate/accounting/journals/{id}` menuntut sesi login. Sesi ini tidak
memiliki kredensial, dan mengambilnya dari `.env` dilarang aturan keselamatan lingkungan.

Yang **belum** dibuktikan lewat HTTP: bentuk JSON respons sesungguhnya. Yang **sudah** dibuktikan:
kompilasi bersih, dan join namanya menghasilkan nilai yang benar pada data yang sama.

Cara owner memverifikasinya dalam satu langkah: buka `FE-ACC-007` setelah layar itu berdiri, dan
lihat riwayat persetujuan `JB/2026/09/00001` berbunyi *"Diajukan oleh SuperAdmin"*.

## 8. Delta terhadap kontrak

`ACC-API` naik `0.4` → `0.5`. Bagian *Daftar DTO* pada `contracts/api-contract.md` diperbarui, dan
catatan lama yang berbunyi *"`ActionByName` tidak ada di source mana pun"* diganti keterangan
bahwa keempatnya kini tersedia dan tidak dipersistensi. Hash manifest digeser
`b4a20208` → `60bce4cd`.

## 9. Risiko yang tersisa

| Risiko | Berat | Keterangan |
|---|---|---|
| Bentuk JSON belum dilihat lewat HTTP | Rendah | Tertutup sendiri saat `FE-ACC-007` diuji |
| Nol test otomatis | Sedang | Seluruh test Accounting dihapus atas keputusan owner (`ACC-TD-016`). Task ini tidak memulihkannya — itu keputusan tersendiri |
| `ACC-DEP-007` masih `OPEN` | Sedang | Registry backend masih nol baris `Acc`. Tidak menahan task ini, tetap menahan merge ke integration |

## 10. Task berikutnya

**`FE-ACC-007` — rincian jurnal dan tombol aksi.** Gerbangnya kini terbuka: `ACC-GAP-011`
`CLOSED`, `AvailableActions` sudah tersedia, dan kelima thunk aksi sudah berdiri di
`accounting-journal-slice.jsx`. Layar itu yang akan menutup `UAT-01` sepenuhnya.
