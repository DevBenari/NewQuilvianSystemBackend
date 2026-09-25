# Laporan Perubahan Backend — `BE-BD-020`

> **Riwayat berkas ini.** Dibuka dan diselesaikan 24 September 2026 dalam satu sesi: audit, keputusan
> pemilik `D1`–`D4`, implementasi, build, validasi runtime, lalu laporan ini.

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BD-020` |
| Judul | Blood Unit Inactive Location Filter |
| Slice | Penyaring kantong tertahan di lokasi nonaktif pada daftar kantong — prasyarat backend `FE-BD-012` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 5, kartu `BE-BD-020` |
| Trace | `DEC-BD-037`; `INV-BD-028`; `VAL-BD-064`, `VAL-BD-068`; `FE-BD-012`; keputusan pemilik `D1`–`D4` (bagian 2) |
| Contract version | `v5` `approved`, dengan amandemen aditif **`D6`** pada grup Blood Unit — dikerjakan **di dalam** task ini |
| Dependency | `BE-BD-014` ✅, `BE-BD-015` ✅, `BE-BD-016` ✅, keputusan `D1`–`D4` ✅ — **nol penahan** |
| Klasifikasi | `LIGHT` — skor 3: satu repository (0), 9–20 berkas diperiksa (1), 3 berkas source diubah (0), logika sederhana (0), kontrak diubah aditif (2), database: hanya perilaku query (0 — nol schema), keamanan: tidak berubah (0), UI: tidak ada (0). Dibulatkan konservatif karena perubahan kontrak |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/BloodBankManagement/**`, `contracts/api-contract.md`, `testing/acceptance-test-matrix.md`, laporan ini, roadmap backend, dan `requirement-traceability.md` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `b9f141ea` (branch `sukmagp`, upstream `origin/sukmagp`), working tree bersih sebelum task |
| Tanggal | 24 September 2026 |
| Status | ✅ **SELESAI — 24 September 2026. Ketujuh acceptance `AC-BD-118`..`AC-BD-124` terpenuhi dan Definition of Done terpenuhi.** Build `0 Error(s)` / `214 Warning(s)` (sama dengan baseline); `has-pending-model-changes` bersih, nol migration; QBE Strict `PASS`; validasi runtime R1–R14 `PASS` lewat HTTP sungguhan terhadap `QuilvianNewDevSukma`. **Batas bukti:** aktor tunggal `superadmin` — jalur `403` `AC-BD-124` terbukti pada tingkat atribut hak akses, bukan lewat aktor tanpa hak baca (bagian 5.4) |

---

## 1. Masalah yang diperbaiki

Ketika petugas Setup Bank Darah menonaktifkan sebuah kulkas darah, sistem **sengaja tidak** memindahkan
kantong di dalamnya (`DEC-BD-037`). Kantong itu tetap tercatat di kulkas yang nonaktif, dan selama belum
dipindahkan ke kulkas aktif **tidak dapat dialokasikan** (`VAL-BD-064`). Petugas hanya diberi tahu
**jumlahnya** lewat peringatan `VAL-BD-068`, misalnya "Ada 6 kantong yang masih tercatat di sana".

Sebelum task ini, petugas **tidak punya cara menemukan keenam kantong itu** di daftar kantong.
`GET /blood-units` tidak mengenal saringan lokasi nonaktif; parameter `inactiveLocation=true` yang
dijanjikan dokumen arsitektur frontend dibuang diam-diam oleh model binder dan daftarnya tetap memuat
seluruh kantong. Akibatnya kantong tertahan bisa terlupa sampai ada order yang membutuhkannya, dan saat
itulah alokasinya ditolak.

Karena itu layar `FE-BD-012` (penyimpanan dan perpindahan kantong) tertahan sejak 23 September 2026.

---

## 2. Keputusan yang mengikat task ini

Audit menemukan satu blocker dan tiga celah kontrak. Keempatnya diputuskan pemilik **`Sukmagp`,
24 September 2026**, seluruhnya mengikuti rekomendasi audit:

| Butir | Pertanyaan | Keputusan | Alasan |
| --- | --- | --- | --- |
| `D1` | Kartu `BE-BD-020` belum ada di roadmap backend — tanpa acceptance, DoD, maupun dependency | **Kartu dibuka** mengikuti preseden `BE-BD-019` | `FE-BD-012` sudah tercatat menunggu task ini |
| `D2` | Kantong berstatus akhir (`Issued`, `ReturnedToProvider`, `NotUsable`) masih menunjuk lokasi terakhirnya. Ikut muncul? | **Tidak — hanya kantong yang masih di stok** | Sama dengan hitungan `VAL-BD-068`; kantong yang sudah keluar tidak perlu dipindahkan |
| `D3` | Lokasi yang di-soft-delete (`IsDelete = true`) dianggap nonaktif? | **Ya** — nonaktif = `!IsActive \|\| IsDelete` | Sama dengan gerbang alokasi `VAL-BD-064` dan penanda `isCurrentStorageLocationActive` |
| `D4` | Arti `inactiveLocation=false`? | **Kebalikan persis dari `true`** | Pola yang sama dengan `emergencyPendingEvidence` |

**Kenapa `D2` perlu diputuskan, bukan diserahkan ke implementasi.** Source menunjukkan `CurrentPlacementId`
**tidak pernah dikosongkan** saat kantong diberikan, dikembalikan, atau dinyatakan tidak layak. Tanpa
batasan status, setiap kantong yang sudah diberikan dari kulkas yang kemudian dinonaktifkan akan
muncul di daftar "tertahan" — padahal kantongnya sudah masuk ke tubuh pasien. Data pengembangan saat ini
memang memuat dua kantong seperti itu (`TEST-BD008-…-04` dan `-05`, `Issued` di `TBD008-LOCB`).

---

## 3. Proses bisnis

| Langkah | Pelaku | Yang terjadi |
| ---: | --- | --- |
| 1 | Pengelola Setup Bank Darah | Menonaktifkan kulkas, misalnya karena rusak. Balasan `200` membawa peringatan `VAL-BD-068`: "Ada 6 kantong yang masih tercatat di sana…" |
| 2 | Sistem | **Tidak** memindahkan kantong dan **tidak** mengubah statusnya (`DEC-BD-037`). Gerbang alokasi dan pemberian normal tertutup bagi keenam kantong itu |
| 3 | Petugas BDRS | Membuka daftar kantong dengan saringan **tertahan di lokasi nonaktif** (`inactiveLocation=true`) dan melihat keenam kantong itu — tidak lebih, tidak kurang |
| 4 | Petugas BDRS | Memindahkan kantong satu per satu ke kulkas aktif (`PUT /blood-units/{id}/storage-location`, milik `BE-BD-015`) |
| 5 | Sistem | Kantong yang sudah dipindahkan otomatis keluar dari saringan, dan gerbang alokasinya terbuka kembali |

**Jalur tidak normal.**

| Keadaan | Yang terjadi |
| --- | --- |
| Kantong `Received`, belum pernah disimpan | **Tidak** dianggap tertahan — ia belum punya lokasi. Muncul pada `inactiveLocation=false` |
| Kantong sudah diberikan dari kulkas yang kemudian dinonaktifkan | **Tidak** muncul pada `true` (`D2`) |
| Kulkas ditandai terhapus walaupun `IsActive` masih `true` | Kantong di dalamnya **muncul** pada `true` (`D3`) — gerbang `VAL-BD-064` juga menolak alokasinya |
| Kulkas diaktifkan kembali | Kantong di dalamnya langsung keluar dari saringan, tanpa satu pun baris kantong disunting |
| Nilai `inactiveLocation` bukan boolean | Ditolak `400` oleh model binder |

**Contoh berangka dari validasi runtime.** Sebelum pengujian ada 42 kantong: 1 tertahan dan 41 tidak.
Kulkas `TBD006-LOC2` berisi 7 kantong — 1 `Tersedia`, 3 `Dialokasikan`, 2 `Menunggu keputusan`, dan
1 `Diberikan`. Setelah kulkas itu dinonaktifkan: peringatan menyebut **6**, saringan `true` memulangkan
**7** (6 dari `TBD006-LOC2` + 1 yang sudah tertahan sebelumnya), kantong `Diberikan` tidak ikut, dan
`7 + 35 = 42`.

---

## 4. Perubahan yang dikerjakan

### 4.1 Berkas yang diperiksa

- `roadmap/backend-roadmap.md`, `roadmap/frontend-roadmap.md` (kartu `FE-BD-012`), `roadmap/requirement-traceability.md`
- `contracts/api-contract.md` (grup Blood Unit dan Blood Storage Location), `contracts/state-transition-matrix.md`, `testing/acceptance-test-matrix.md`
- `03-frontend-architecture.md` baris 157, `00-interview-decisions.md` (`DEC-BD-037`, `INV-BD-027/028`)
- `Controllers/BbkBloodUnitController.cs`, `Services/BbkBloodUnitService.cs` (`GetPagedAsync`, `StillInStockStatuses`, `EvaluateAllocationGateAsync`, `BuildFilterMetadata`, `BaseQuery`), `DTOs/BloodUnitDtos.cs`, `Models/BbkBloodUnitPlacement.cs`, `Enums/BbkBloodUnitStatus.cs`
- `Areas/HealthServices/MasterData/Services/BloodStorageLocationService.cs` (`CountHeldUnitsAsync`), `Controllers/BloodStorageLocationController.cs`
- `AGENTS.md`, `rules/backend/*`, `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` (`Bbk` `ACTIVE`), laporan `BE-BD-019`

### 4.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BloodBankManagement/Controllers/BbkBloodUnitController.cs` | `GetAll` menerima `[FromQuery] bool? inactiveLocation = null` dan meneruskannya ke service; komentar ringkasan menyebut saringan baru |
| `Areas/HealthServices/BloodBankManagement/Services/BbkBloodUnitService.cs` | `GetPagedAsync` memperoleh parameter opsional terakhir `inactiveLocation`. Penyaringnya diletakkan **sebelum** `CountAsync` sehingga `totalData`/`totalPage` menghitung hasil akhir. `true` = `StillInStockStatuses` **dan** penempatan berlaku ada **dan** lokasinya `!IsActive \|\| IsDelete`; `false` = negasi persisnya |
| `Areas/HealthServices/BloodBankManagement/DTOs/BloodUnitDtos.cs` | `BloodUnitDefaultFilterResponse.InactiveLocation` (`bool?`, bawaan `null`) untuk `GET /filters/metadata` |
| `docs/module-blueprints/bank-darah/contracts/api-contract.md` | Amandemen `v5` **`D6`** di bawah tabel grup Blood Unit; baris `GET /` dan metadata `last_changed_in` diperbarui |
| `docs/module-blueprints/bank-darah/testing/acceptance-test-matrix.md` | Bagian 13 baru: `AC-BD-118`..`AC-BD-124` |
| `docs/module-blueprints/bank-darah/roadmap/backend-roadmap.md` | Kartu `BE-BD-020` baru (`D1`) beserta statusnya |
| `docs/module-blueprints/bank-darah/roadmap/requirement-traceability.md` | Bukti baru pada baris penyimpanan kantong, tabel acceptance, dan baris "dapat dijadwalkan hari ini" |
| `docs/module-blueprints/bank-darah/task/report/backend/BE-BD-020.md` | Berkas ini |

### 4.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif penuh** (`D6`): satu query opsional pada `GET /`, satu isian bawaan pada `GET /filters/metadata`. Nol endpoint baru, nol perubahan bentuk respons, nol perubahan perilaku bila parameter tidak dikirim. Satu-satunya pemanggil `GetPagedAsync` adalah controller ini |
| Database | **Nol** schema, entity, konfigurasi EF, maupun migration. Hanya query baca. `has-pending-model-changes` bersih |
| Keamanan/Auth | **Tidak berubah.** `[AccessPermission("BloodUnit", "Read")]` dan `[AccessAction("Read", …)]` pada `GetAll` dan `GetFilterMetadata` tidak disentuh diff; nol butir hak akses baru. Nol hardcode role |

### 4.4 Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `BloodBankManagement` |
| Owner / prefix | `Bbk` — `ACTIVE` pada registry (baris 2026-09-03) |
| Keberlakuan | `NEW CODE` — seluruh berkas yang disentuh adalah kode `Bbk*` modul ini |
| Arketipe endpoint | Transaksi, **worklist baca-saja** — saringan pada `GET /` yang sudah ada; bukan pola master data |
| QBE yang berlaku | `QBE-API-001` (kontrak/route dipertahankan), `QBE-PERM-001` (hak akses tidak berubah), `QBE-DTO-001` (DTO di folder domain), `QBE-SVC-001` (logika di service, bukan controller) |
| Generic repository / hardcode role | Tidak ada |

---

## 5. Verifikasi

### 5.1 Perintah

| Perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:UseSharedCompilation=false -o <scratchpad>/out` | **`Build succeeded`** — **`0 Error(s)`, `214 Warning(s)`**, `01:10:28` | `PASS` | Jumlah peringatan sama dengan baseline `BE-BD-017`..`019`. Tiga peringatan `CS1573` pada `BbkBloodUnitService.cs` berada di record `BloodUnitResult` lama (nomor barisnya bergeser 22 karena sisipan task ini), **bukan** di baris task |
| `dotnet ef migrations has-pending-model-changes --no-build` | "No changes have been made to the model since the last migration." | `PASS` | Dijalankan atas assembly `bin/Debug` build `HEAD`; sah karena diff task nol berkas model/konfigurasi/migration (diverifikasi `git diff --name-only`) |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` (WorkingTree) | 3 berkas dievaluasi, `VIOLATION 0` / `REVIEW 0` / `INFO 0` | `PASS` | Keluaran terminal: `Final result: PASS` |
| `dotnet test` | — | `NOT RUN` | Tidak ada project test terpisah pada repository (sesuai `AGENTS.md`) |

**Cara build.** Build diarahkan ke folder output di scratchpad sesi karena `QuilvianSystemBackend` milik
pemilik sedang berjalan dan mengunci `bin/Debug` (port `5107`). `UseSharedCompilation=false` dipakai sejak
awal atas pelajaran `STATUS_STACK_OVERFLOW` pada `BE-BD-019` bagian 6.1; build lulus pada percobaan
pertama.

### 5.2 Validasi runtime — R1–R14 `PASS`

Dijalankan langsung oleh agent terhadap **`QuilvianNewDevSukma`**. Aplikasi hasil build dijalankan dari
folder output scratchpad pada `http://localhost:5217` (port `5107` dipakai aplikasi pemilik),
autentikasi memakai cookie sesi `superadmin` hasil `POST /api/v1/Auth/login`. Ekspektasi dihitung
**lebih dulu** dan independen lewat SQL langsung ke database, lalu dibandingkan dengan hasil API.

| No | Skenario | Hasil sebenarnya | AC | Klasifikasi |
| ---: | --- | --- | --- | :---: |
| R1 | Tanpa saringan | `200`, `totalData 42` — sama dengan SQL | — | `PASS` |
| R2 | `inactiveLocation=true`, data apa adanya | `totalData 1`: `TEST-BD009-20260917100009-08`, Menunggu keputusan, `TBD009-LOCB`, `isCurrentStorageLocationActive=false` — sama dengan SQL | `AC-BD-118` | `PASS` |
| R3 | `inactiveLocation=false` | `totalData 41` — sama dengan SQL | `AC-BD-123` | `PASS` |
| R4 | Partisi `true`/`false` | Irisan **0**; gabungan **sama persis** dengan 42 id tanpa saringan | `AC-BD-123` | `PASS` |
| R5 | Kantong di lokasi aktif (32 kantong) | Nol di `true`; 32 di `false` | `AC-BD-119` | `PASS` |
| R6 | Kantong tanpa lokasi (7 kantong, seluruhnya `Received`) | Nol di `true`; 7 di `false` | `AC-BD-120` | `PASS` |
| R7 | Kantong `Diberikan` di lokasi nonaktif `TBD008-LOCB` (`-04`, `-05`) | **Tidak** di `true`; ada di `false` | `AC-BD-121` | `PASS` |
| R8 | `PATCH /blood-storage-locations/{TBD006-LOC2}/status` `isActive=false` | `200`, pesan "Ada **6** kantong yang masih tercatat di sana…" | `AC-BD-118`, `AC-BD-121` | `PASS` |
| R9 | `inactiveLocation=true` sesudah R8 | `totalData 7`: 6 kantong `TBD006-LOC2` (1 Tersedia, 3 Dialokasikan, 2 Menunggu keputusan) + `TBD009-…-08`. Kantong **Diberikan** `TBD006-…-06` **tidak** ikut. Jumlah dari `TBD006-LOC2` = **6 = angka `VAL-BD-068`**; status ketujuh kantong di lokasi itu **tidak berubah** | `AC-BD-118`, `AC-BD-121` | `PASS` |
| R10 | Paging dan kombinasi sesudah R8 | `true&pageSize=2&pageNumber=2` → `totalData 7`, `totalPage 4`, 2 butir. `true&unitStatus=3&pageSize=2&pageNumber=2` → `totalData 3`, `totalPage 2`, 1 butir. `true&search=TEST-BD006&pageSize=4&pageNumber=2` → `totalData 6`, `totalPage 2`, 2 butir. Partisi `7 + 35 = 42` | `AC-BD-123` | `PASS` |
| R11 | Kulkas diaktifkan kembali (`isActive=true`) | `200` "Lokasi penyimpanan darah berhasil diaktifkan."; `true` kembali ke `totalData 1` | — | `PASS` |
| R12 | `TBD010-LOCA` ditandai `IsDelete=true` lewat SQL (`IsActive` tetap `true`), lalu `inactiveLocation=true` | `totalData 2`: `TEST-BD010-20260917135556-04` Dialokasikan di `TBD010-LOCA` (`active=false`) + `TBD009-…-08`. Kantong `Diberikan` di lokasi yang sama tidak ikut. Dipulihkan sesudahnya | `AC-BD-122` | `PASS` |
| R13 | `GET /filters/metadata` | `200`, `defaultFilter.inactiveLocation = null` | — | `PASS` |
| R14 | Tanpa login; nilai tidak sah | Tanpa cookie → `401`. `inactiveLocation=bukan-bool` → `400` "The value 'bukan-bool' is not valid." | `AC-BD-124` | `PASS` |

### 5.3 Keadaan database sesudah pengujian

| Butir | Keadaan |
| --- | --- |
| `TBD010-LOCA` | `IsDelete` dikembalikan ke `false` lewat SQL. `UpdateDateTime`/`UpdateBy` **tidak** tersentuh (tetap `null` / GUID kosong, identik dengan snapshot awal) |
| `TBD006-LOC2` | `IsActive` kembali `true` lewat API yang sah. **`UpdateDateTime` dan `UpdateBy` kini terisi** (`2026-09-24T05:27:36Z`, akun `superadmin`) — jejak audit wajar dari dua panggilan `PATCH /status`; sebelumnya `null` |
| Kantong | Nol baris kantong disunting. Hitungan akhir `true = 1`, total `42` — sama dengan sebelum pengujian |
| Data baru | Nol kantong, lokasi, order, atau penempatan baru |

### 5.4 Batas bukti

- **Aktor tunggal `superadmin`.** Tidak ada akun non-SuperAdmin yang kredensialnya tersedia bagi agent.
  `AC-BD-124` karena itu terbukti pada **tingkat atribut hak akses** — pola yang sama dengan `AC-BD-037`
  dan `AC-BD-077/078`: diff tidak menyentuh satu pun baris `[AccessAction]`/`[AccessPermission]`, jalur
  tanpa autentikasi terbukti `401`, dan penegakan `403` oleh `AccessPermission` untuk `BloodUnit : Read`
  sudah terbukti runtime dengan dua aktor non-SuperAdmin pada `BE-BD-018`. Jalur `403` untuk saringan
  baru ini sendiri **belum** ditembakkan dengan aktor tanpa hak baca.
- **`AC-BD-122` memakai `IsDelete` yang disetel lewat SQL**, bukan `DELETE /blood-storage-locations/{id}`,
  karena penghapusan lewat API tidak dapat dibatalkan lewat API. Yang diuji adalah perilaku penyaring
  terhadap baris yang terhapus, dan itu identik apa pun cara barisnya terhapus.
- Uji manual lewat layar: `NOT APPLICABLE` — layar saringan ini milik `FE-BD-012`.

---

## 6. Dokumentasi endpoint

#### Health Services / Blood Bank Management / Blood Unit

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/blood-bank-management/blood-units?inactiveLocation=true` | Daftar kantong yang masih di stok tetapi tertahan di lokasi nonaktif — pekerjaan pemindahan yang menunggu | `BloodUnit : Read` |
| `GET` | `/api/v1/health-services/blood-bank-management/blood-units?inactiveLocation=false` | Seluruh kantong lainnya — kebalikan persis | `BloodUnit : Read` |
| `GET` | `/api/v1/health-services/blood-bank-management/blood-units/filters/metadata` | Bawaan penyaring kini memuat `inactiveLocation: null` | `BloodUnit : Read` |

Rincian kontraknya ada pada [api-contract.md](../../../contracts/api-contract.md) Amendment `v5` `D6`.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-BD-118` — lokasi nonaktif muncul | Terpenuhi | R2, R8, R9 — termasuk kantong `Tersedia` yang tertahan, status tidak berubah |
| `AC-BD-119` — lokasi aktif tidak muncul | Terpenuhi | R5 |
| `AC-BD-120` — tanpa lokasi tidak dianggap nonaktif | Terpenuhi | R6 |
| `AC-BD-121` — status akhir tidak muncul (`D2`) | Terpenuhi | R7, R9 — himpunan dari `TBD006-LOC2` = 6 = angka `VAL-BD-068` |
| `AC-BD-122` — lokasi terhapus dianggap nonaktif (`D3`) | Terpenuhi | R12 |
| `AC-BD-123` — kebalikan dan paging (`D4`) | Terpenuhi | R3, R4, R10 |
| `AC-BD-124` — hak akses tetap | Terpenuhi pada tingkat atribut hak akses | R14 dan diff; batasnya bagian 5.4 |
| DoD: `api-contract.md` diamandemen `D6` | Terpenuhi | Bagian 4.2 |
| DoD: `AC-BD-118`..`124` di matriks acceptance | Terpenuhi | Bagian 13 matriks |
| DoD: nol migration | Terpenuhi | Bagian 5.1 |
| DoD: laporan tracked | Terpenuhi | Berkas ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Build: 214 peringatan, seluruhnya sudah ada sebelumnya; nol dari baris task. `git` memperingatkan konversi LF→CRLF pada berkas yang disentuh — perilaku `core.autocrlf` biasa, diff tetap sempit (lihat `git diff --stat`) |
| Technical debt | **`CurrentPlacementId` tetap terisi pada status akhir** (`Issued`, `ReturnedToProvider`, `NotUsable`): kantong yang sudah keluar dari stok masih menunjuk lokasi terakhirnya. Dicatat sebagai technical debt atas keputusan pemilik `Sukmagp` 24 September 2026 dan **sengaja tidak masuk task ini** — perilaku `CurrentPlacement` tidak diubah. Dampaknya pada penyaring ini sudah dinetralkan lewat `D2`; setiap penyaring atau hitungan baru yang membaca `CurrentPlacementId` wajib ikut membatasi ke `StillInStockStatuses`. Perbaikannya, bila diinginkan, menjadi task tersendiri dengan keputusan bisnis atas riwayat penempatan |
| Masalah yang diketahui | (1) Baris `GET /` api-contract dan `03-frontend-architecture.md` menulis `status=`, sedangkan parameter sebenarnya `unitStatus` — dicatat di `D6`, tidak diubah. (2) Tabel ringkasan status roadmap backend bagian 3 masih menghitung **18** task dan tidak memuat `BE-BD-019` maupun `BE-BD-020`, begitu pula grafik dependency bagian 4 — selisih ini sudah ada sejak `BE-BD-019` dan di luar wewenang tulis roadmap task ini (hanya kartu dan status) |
| Risiko tersisa | Rendah. Frontend `FE-BD-012` wajib memakai `unitStatus` (bukan `status`) untuk saringan `Received`. `GET /summary` tidak menghitung kantong tertahan — bila layar butuh angka lencana, pakai `totalData` dari `inactiveLocation=true` |
| Perubahan sampingan | `NONE` di repository. Di database: `UpdateDateTime`/`UpdateBy` `TBD006-LOC2` terisi (bagian 5.3). Alat bantu sekali pakai (kueri SQL, klien HTTP, output build) berada di scratchpad sesi, **di luar** repository; kredensial dibaca dari konfigurasi tanpa disalin atau dicetak |
| Interupsi | `NONE` |
| Status Git | `M` 3 berkas source `Areas/HealthServices/BloodBankManagement/**` dan `M` 4 dokumen blueprint + `??` laporan ini — seluruhnya milik task ini. Nol stage, nol commit |
| Langkah berikutnya | Commit bila disetujui pemilik. **`FE-BD-012` kini tidak lagi tertahan backend** — saringan `Received` (`unitStatus=0`) dan `inactiveLocation=true` sudah tersedia untuk ditempel pada layar daftar kantong `FE-BD-004`. Opsional: sinkronkan tabel ringkasan dan grafik roadmap backend untuk `BE-BD-019`/`BE-BD-020` lewat pass perencanaan |
