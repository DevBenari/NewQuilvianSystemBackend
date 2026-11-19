# Laporan Perubahan Backend — `BE-AST-013`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-AST-013` |
| Judul | Dashboard, laporan register, History & Audit |
| Slice | `REG-S2` — Register aset: pencatatan, label, penempatan, kondisi, dokumen, pemanfaatan, impor, dashboard |
| Roadmap | `register/roadmap/backend-roadmap.md` — kartu `BE-AST-013` |
| Trace | `CAP-01.1`–`01.3`, `01.6`, `11.1`, `11.3`, `11.7`; `AST-DEC-025`, `AST-DES-004`, `AST-DES-016`; API 2.16–2.17; `permission-audit-matrix` (`AssetDashboard`, `AssetReport`, ekspor tanpa rupiah); `AT-REG-12`, `16`, `46` |
| Contract version | `AST-REG-1.0.0` `approved` 2026-11-12 |
| Dependency | `BE-AST-009` ✅, `BE-AST-010` ✅; memakai `BE-AST-005` (`AstAssetHistory`), `BE-AST-007` (daftar aset dan pembatasan unit) |
| Klasifikasi | `STANDARD` — read-only, 2 query service, 2 controller (7 endpoint), ekspor Excel; tanpa entity atau migration |
| Task mode | `BACKEND`, diminta Rizki 2026-11-18 ("kerjakan BE-AST-013 lalu FE-AST-010", commit langsung) |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/AssetManagement/**`; laporan ini dan baris status roadmap/traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `b73ebef5` (branch `rizkiG`) |
| Tanggal | 18–19 November 2026 |
| Status | 🟡 SEBAGIAN 19 November 2026 — keempat kriteria terpetakan ke source; QBE Strict `PASS`; build Rizki berhasil; runtime **74/74** + uji unit sendiri 18/21. Kriteria 1, 3, 4 terbukti penuh. Kriteria 2 terbukti untuk akun tanpa unit (semua angka 0, unit lain 403, `AT-REG-46` lulus), tetapi **`AT-REG-12` positif** (akun terbatas melihat aset unitnya sendiri) belum dapat diuji: di DB dev tidak ada akun non-SuperAdmin yang punya penugasan unit di Instalasi Rawat Inap, satu-satunya unit beraset |

**Backend Governance Preflight**

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AssetManagement` / `Register` |
| Pemilik / prefix registry | `AssetManagement / Register Aset`, prefix `Ast`, `ACTIVE` |
| Keberlakuan | `NEW CODE` (+ `TOUCHED`: `AssetRecordService.Query` milik `BE-AST-007`, saringan daftar dipisah tanpa mengubah perilaku) |
| QBE yang berlaku | `QBE-MOD-001`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001` (tanpa rupiah di log maupun ekspor), `QBE-DTO-001`, `QBE-VAL-001` |
| Tidak berlaku | `QBE-ENT-*`, `QBE-CFG-*`, `QBE-NAM-*`, `QBE-DB-*`, `QBE-CODE-*`, `QBE-TXN-001`, `QBE-DEL-001` — read-only, tanpa entity, nomor, transaksi tulis, atau migration |
| Arketipe | Read-only (dashboard dan laporan) |

---

## 1. Masalah yang diperbaiki

Bagian Aset, Kepala Unit, Akuntansi, dan auditor belum punya satu layar ringkasan aset maupun laporan cetak. Jumlah
aset per kondisi, aset yang belum ditempatkan, sebaran per unit, dan jejak siapa mengubah apa hanya bisa dilihat
satu per satu dari daftar dan linimasa tiap aset. `CAP-01` (Dashboard Aset) dan `CAP-11.1`/`11.3`/`11.7` (laporan)
meminta angka yang sama dengan daftar, terbatas pada unit pengguna, dan ekspor Excel tanpa rupiah.

---

## 2. Proses bisnis

| Langkah | Pelaku | Yang terjadi |
|---|---|---|
| 1 | Bagian Aset / Kepala Unit / Direktur | Membuka **Dashboard Aset**. Kartu Total Aset, Menunggu Nomor Inventaris, Belum Ditempatkan, Sudah Ditempatkan, Aset Menganggur; lima kartu kondisi; jumlah per unit; sepuluh aset rusak terbaru. Pilih unit → angka unit itu saja |
| 2 | Sama | Setiap kartu membawa saringannya sendiri. Dibuka di Daftar Inventaris, jumlah barisnya **sama** dengan angka kartu |
| 3 | Bagian Aset / Auditor | **Daftar Inventaris Per-unit** (urut unit → ruangan → nomor) dan **Kondisi Aset** (urut kondisi → unit → nomor), disaring unit/ruangan/kelompok/kondisi/penempatan, lalu **Ekspor Excel** |
| 4 | Auditor | **History & Audit**: seluruh kejadian linimasa dari semua sub-modul, saring kejadian, sub-modul, pelaku, periode (tanggal WIB), aset, unit; ekspor Excel |

Contoh dari uji: dashboard menghitung 79 aset terdaftar, 32 belum ditempatkan, 47 di Instalasi Rawat Inap (45 Aktif,
1 Rusak Ringan, 1 Rusak Berat). Klik "Belum Ditempatkan" → Daftar Inventaris 32 baris. History & Audit memuat 101
kejadian, sama dengan isi `AstAssetHistory` di DB; 7 di antaranya "Kondisi Berubah".

**Jalur tidak normal**

| Keadaan | Hasil |
|---|---|
| Tanpa `AssetRegister : ReadAllUnits` | Semua angka, laporan, dan History hanya dari aset yang saat ini ditempatkan di unit penugasan pengguna |
| Dashboard dengan `organizationUnitId` di luar unit pengguna | 403 `AST-VAL-REG-016` "Aset ini bukan milik unit Anda." |
| Periode History terbalik / sub-modul tidak dikenal | 400 `AST-RPT-FIELD` |
| Kode laporan ekspor tidak dikenal | 404 `AST-RPT-NOT-FOUND` |
| Hasil ekspor lebih dari 10.000 baris | 400 `AST-RPT-EXPORT-LIMIT` "Persempit saringan, misalnya per unit atau per periode" |
| Tanpa `AssetDashboard : Read` / `AssetReport : Read` | 403 |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` backend; `rules/backend/` (engineering contract, registry, role-access, transaction standard read-only);
kontrak 2.16–2.17, `02-backend-architecture.md` (`AssetDashboardQueryService`, `AssetReportQueryService`,
`AssetDashboardDtos.cs`), `permission-audit-matrix` §2–3, `03-frontend-architecture.md` §3.5, `02-module-map.md`;
`AssetRecordService.Query` (daftar, `ScopedAssetsAsync`), `AssetHistoryService`, `AssetImportWorkbook` (pola ClosedXML).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Register/Services/AssetRecordService.Query.cs` | Saringan daftar dipisah menjadi `FilteredAssetsAsync` (internal) yang dipakai daftar, dashboard, dan laporan — satu sumber saringan supaya kartu = daftar; `ProjectList`, `Describe`, `UserNamesAsync` menjadi `internal`. Perilaku `GET /assets` tidak berubah |
| `Register/Services/AssetDashboardQueryService.cs` | Baru — ringkasan (5 kartu + 5 kondisi + aset rusak terbaru, tiap kartu membawa `filter`), per unit |
| `Register/Services/AssetReportQueryService.cs` | Baru — 11.1, 11.3, 11.7, metadata, ekspor (batas 10.000 baris), keterangan saringan dan pencetak di lembar Excel |
| `Register/Services/AssetReportWorkbook.cs` | Baru — penulis lembar Excel (judul, info, kepala tebal, autofilter, kolom dibekukan) |
| `Register/Controllers/AssetDashboardController.cs` | Baru — 2 endpoint, resource `AssetDashboard : Read` |
| `Register/Controllers/AssetReportsController.cs` | Baru — 5 endpoint, resource `AssetReport : Read` |
| `Register/DTOs/AssetDashboardDtos.cs` | Baru |
| `Register/Services/AssetManagementException.cs` | + kode `AST-RPT-FIELD`, `AST-RPT-NOT-FOUND`, `AST-RPT-EXPORT-LIMIT` dan pesannya |
| `AssetManagementServiceCollectionExtensions.cs` | + 2 service |

### 3.3 Dampak kontrak API, database, dan keamanan

**Delta terhadap `AST-REG-1.0.0`:**

| No | Delta | Alasan |
|---|---|---|
| 1 | `GET /reports/filters/metadata` | Layar butuh daftar kode laporan, jenis kejadian, sub-modul, dan batas ekspor |
| 2 | `/reports/history` mengembalikan `AssetHistoryReportItemResponse` (= `AssetHistoryItemResponse` + `assetId`, `inventoryNumber`, `assetName`, `currentOrganizationUnitName`, `subModuleName`) | Laporan lintas aset tanpa identitas aset tidak terbaca; DTO linimasa per aset tidak diubah |
| 3 | Saringan History tambahan: `subModule`, `occurredFrom`/`occurredTo` (tanggal WIB), `assetId`, `organizationUnitId`, `groupId`, `search`; `sortDirection` | Kontrak menyebut `AssetReportQuery` + `eventType`, `actorUserId` tanpa rincian |
| 4 | Kartu dashboard berbentuk `{ key, label, count, filter }`; `filter` = parameter `GET /assets` | Kriteria 1 (kartu = daftar) dan tautan kartu ke daftar |
| 5 | Kartu "Menunggu Nomor Inventaris" (status Diajukan) dan "Aset Menganggur" (`isIdle`) | Diagram §3.5 menyebut "Total aset, Terdaftar, Belum ditempatkan"; arsitektur menyebut daftar aset menganggur. Rupa kartu `DEV_DISCRETION` (`AST-FE-005`) |
| 6 | Laporan 11.1 dan 11.3 hanya aset `Terdaftar` | Draft, Diajukan, Dibatalkan belum menjadi inventaris |
| 7 | Dashboard menolak unit di luar hak dengan 403 `016` (daftar mengembalikan kosong) | Memberi tahu pengguna, bukan angka nol yang menyesatkan |
| 8 | Batas ekspor 10.000 baris → 400 `AST-RPT-EXPORT-LIMIT` | Batas teknis memori ClosedXML; 2.400 aset RS muat |

**Database:** tanpa migration dan tanpa penulisan. Uji hanya menulis kebijakan Akses Role sementara dan
memulihkannya (bagian 5.4).

**Keamanan:** 7/7 pasangan `[AccessAction]`/`[AccessPermission]` cocok; tanpa hardcode peran. Batas unit memakai
`AssetActorResolver` yang sama dengan daftar. Ekspor tanpa kolom rupiah, sesuai `permission-audit-matrix` §3
(diperiksa pada kepala kolom dan isi sel).

---

## 4. Dokumentasi endpoint

#### Corporate / Asset Management / Register / Dashboard (`…/register/dashboard`)

| Method | Path | Hak | Keterangan |
|---|---|---|---|
| `GET` | `/summary` | `AssetDashboard : Read` | `organizationUnitId` opsional → `cards[]`, `conditions[]` (`key`, `label`, `count`, `filter`), `damagedTotal`, `recentDamaged[]` (10), `canReadAllUnits` |
| `GET` | `/by-unit` | `AssetDashboard : Read` | Aset `Terdaftar` yang ditempatkan, per unit: `total`, `active`, `lightlyDamaged`, `heavilyDamaged`, `unused`, `lost`, `filter` |

#### Corporate / Asset Management / Register / Report (`…/register/reports`)

| Method | Path | Hak | Keterangan |
|---|---|---|---|
| `GET` | `/filters/metadata` | `AssetReport : Read` | Kode laporan, urutan, jenis kejadian, sub-modul, kondisi, penempatan, `maxExportRows` |
| `GET` | `/inventory-by-unit` | `AssetReport : Read` | `search`, `organizationUnitId`, `locationId`, `categoryId`, `groupId`, `condition`, `placementState`, `sortBy` (`unit` bawaan, `inventoryNumber`, `name`, `condition`), paging |
| `GET` | `/condition` | `AssetReport : Read` | Saringan sama; `sortBy` bawaan `condition` |
| `GET` | `/history` | `AssetReport : Read` | + `eventType`, `subModule`, `actorUserId`, `assetId`, `occurredFrom`, `occurredTo`, `sortDirection` (`desc` bawaan) |
| `GET` | `/{reportCode}/export` | `AssetReport : Read` | `inventory-by-unit` / `condition` / `history`, saringan sama tanpa paging → `laporan-aset-<kode>-<yyyyMMdd-HHmm>.xlsx` |

Enum di query string boleh angka atau nama (`condition=2` atau `condition=RusakRingan`).

---

## 5. Verifikasi

### 5.1 Statis dan build

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| QBE `-Mode Strict` (WorkingTree) | `Files evaluated: 9`, `VIOLATION: 0`, `REVIEW: 0`, `Final result: PASS` | `PASS` |
| Atribut akses | 7/7 | `PASS` |
| Baris komentar baru | 0 | `PASS` |
| `dotnet build -p:RunAnalyzers=false` oleh Rizki | DLL 2026-11-19 08:36:06 memuat `AssetDashboardController`, `AssetReportsController`, `AssetReportWorkbook`, kedua service | `PASS` |
| Registrasi hak | `AssetDashboard` (`Read`) dan `AssetReport` (`Read`) di `role-access/resources/structured` | `PASS` |

Backend sempat tidak dapat dijalankan pukul 08:41 dan 08:44: server DB dev `160.22.250.77:5432` tidak menjawab
(timeout) sementara port 22 server itu terbuka. Penyebabnya di sisi jaringan atau server, bukan kode; setelah DB pulih
backend dijalankan ulang Rizki tanpa build ulang.

### 5.2 Runtime (HTTPS 7184) — `test-with-agy/asset-management/be-ast-013-20261119/ast013-runtime.mjs`

| Kelompok | Langkah | Hasil |
|---|---|---|
| Kriteria 1 (`AT-REG-16`) | 10 kartu dashboard = `totalData` `GET /assets` dengan `filter` kartu (79, 2, 32, 47, 0, 76, 1, 1, 1, 0); total rusak = daftar; baris per unit dan lima kolom kondisinya = daftar; jumlah per unit = kartu Ditempatkan; ringkasan unit Instalasi Rawat Inap = daftar unit itu | 17/17 |
| Laporan 11.1, 11.3 | Total = 79 aset `Terdaftar`; urut unit dan urut kondisi; saring unit dan saring `condition=RusakRingan` = daftar; tanpa kunci rupiah di respons | 7/7 |
| Kriteria 3 (History lintas sub-modul) | Total API = isi `AstAssetHistory` di DB (101 = 101, cek baca-saja `cek-db.fsx`); per sub-modul API = DB (register 101, empat sub-modul lain 0 karena belum dibangun); `eventType=KondisiBerubah` = 7 = DB; `assetId` = linimasa aset; saring pelaku; periode WIB; urut terbaru; periode terbalik dan sub-modul salah → 400 `AST-RPT-FIELD`; metadata | 14/14 |
| Kriteria 4 (ekspor tanpa rupiah) | 4 berkas `.xlsx` dibaca ClosedXML (`baca-xlsx.fsx`): 79/1/101/7 baris = API, kepala kolom tanpa harga/nilai/penyusutan, 0 sel berisi "Rp", keterangan saringan dan pencetak tercetak; kode tak dikenal → 404 | 9/9 |
| Hak akses | LOKET → 403 pada ketujuh endpoint | 7/7 |
| Kriteria 2 (akun penyetuju uji tanpa `ReadAllUnits`, tanpa penugasan unit) | Semua kartu 0 = daftar 0; per unit kosong; Instalasi Rawat Inap → 403 `AST-VAL-REG-016`; laporan, History, ekspor 0 baris | 16/16 |
| Penyiapan dan pemulihan | Registrasi hak, policy sementara, policy pulih identik (6 hak) | 4/4 |

**Rekap: 74/74.**

Uji tambahan `ast013-unit-sendiri.mjs` (akun PENERIMA, non-SuperAdmin, diberi `AssetRegister`/`AssetDashboard`/
`AssetReport : Read` sementara tanpa `ReadAllUnits`): **18/21**. Lulus: kartu = daftar untuk akun terbatas,
**`AT-REG-46`** (detail aset di luar unit → 403 `AST-VAL-REG-016`), History aset di luar unit kosong, policy pulih
identik (22 hak). Tiga langkah positif gagal karena akun itu tidak punya penugasan unit aktif (dicek baca-saja di
`WfpOrganizationAssignment`), sehingga angka unit sendiri 0, bukan karena cacat kode.

### 5.3 Temuan

| Temuan | Tindak |
|---|---|
| `AT-REG-12` positif tidak dapat diuji: di DB dev tidak ada akun non-SuperAdmin dengan penugasan unit di Instalasi Rawat Inap (Dimas = SuperAdmin; penyetuju uji dan PENERIMA tanpa penugasan) | Butuh penugasan unit HR untuk salah satu akun uji, atau penempatan aset ke unit akun yang sudah punya penugasan. Bukan perubahan kode |
| Header `Content-Disposition` tidak diekspos CORS, jadi nama berkas tidak terbaca di browser | FE memakai nama cadangan `laporan-aset-<kode>-<yyyyMMdd>.xlsx`; `Program.cs` (konfigurasi CORS bersama) tidak diubah karena di luar lingkup modul |
| Sisa uji `BE-AST-011`: pekerja harian mengubah `AST-PMF-000008` Disetujui → **Berjalan** pada 19 Nov 02:33 UTC, kunci aset tetap, linimasa `AST-MBL-000005` "Dimanfaatkan" | Diperiksa `be-ast-011-20261118/cek-pekerja-19nov.mjs` 3/3; dicatat di `BE-AST-011.md` |

### 5.4 Data uji di `QuilvianNewDevRizki`

Tanpa data baru. Kebijakan Akses Role jabatan penyetuju uji (`d95ca274…/01c0c1c8…`) dan PENERIMA
(`3aa08cc0…/20f0bbda…`) diubah sementara lalu dipulihkan dan dibandingkan dengan salinan aslinya
(`ra-policy-asli.json`, `ra-policy-penerima-asli.json`): identik.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Source | Bukti | Status |
|---|---|---|---|
| 1. Kartu = hitungan daftar bersaringan | `FilteredAssetsAsync` dipakai daftar dan dashboard; `filter` per kartu | `AT-REG-16` 17/17 + 10/10 akun terbatas | ✅ |
| 2. Batas unit berlaku | `ScopedAssetsAsync` lewat `FilteredAssetsAsync` di semua endpoint; `EnsureCanAccessUnit` pada unit dashboard | Akun tanpa unit: semua 0, unit lain 403; `AT-REG-46` lulus. `AT-REG-12` positif belum | 🟡 |
| 3. History & Audit membaca `AstAssetHistory` lintas sub-modul | `HistoryBaseAsync` tanpa batas sub-modul; saring `subModule` | API = DB total dan per sub-modul | ✅ |
| 4. Ekspor tanpa kolom rupiah | `InventorySheet`, `ConditionSheet`, `HistorySheet` | 4 berkas dibaca: 0 kolom/sel rupiah | ✅ |

| DoD | Status |
|---|---|
| `V-BE` QBE, build, registrasi hak, runtime | Terpenuhi |
| `AT-REG-16` | Lulus |
| `AT-REG-12` | Sebagian — pembatasan dan penolakan terbukti; kasus positif menunggu akun uji berpenugasan unit |
| Laporan, roadmap, traceability | Ditulis |
| Commit | Dilakukan bersama `FE-AST-010` atas perintah Rizki |

---

## 7. Catatan penutup

- **Untuk menutup 🟡:** beri salah satu akun uji non-SuperAdmin penugasan unit Instalasi Rawat Inap (atau tempatkan
  aset ke unit akun yang sudah berpenugasan), lalu jalankan ulang `ast013-unit-sendiri.mjs`.
- **Task berikutnya:** `FE-AST-010` (dikerjakan bersama). Sesudah itu register tinggal `BE-AST-011`/`FE-AST-008`
  (`AT-REG-15` positif mulai 20 Nov) dan `AT-REG-12` di atas.
- `Storage/uploads/asset-management/` dan dua berkas `docs/module-blueprints/igd/...` yang belum terlacak bukan milik
  task ini dan tidak di-commit.
