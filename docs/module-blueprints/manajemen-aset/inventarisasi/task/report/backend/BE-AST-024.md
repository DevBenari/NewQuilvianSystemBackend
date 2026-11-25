# Laporan Perubahan Backend — `BE-AST-024`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-AST-024` |
| Judul | Dashboard 1.5 dan laporan inventaris |
| Gelombang | `MVP-3` — sub-modul `inventarisasi` |
| Roadmap | `inventarisasi/roadmap/backend-roadmap.md` — kartu `BE-AST-024` |
| Trace | `CAP-01.5`, `CAP-11.4`; `AST-DEC-031`; `contracts/api-contract.md` 2.3, 2.4; `AT-INV-11` |
| Contract version | `AST-INV-1.0.0` (bagian dari `AST-BP-001` rev 1, `approved` 2026-11-12) |
| Dependency | `BE-AST-023` ✅; memakai `AssetReportWorkbook`, `AssetReportQueryService.MaxExportRows`, `AssetDashboardCardResponse` [REG] |
| Klasifikasi | `STANDARD` — 1 service baru, 2 controller baru, tambahan query/DTO; hanya baca, tanpa migration |
| Task mode | `BACKEND`, diminta Rizki 2026-11-24 ("lanjutkan kerjakan langkah berikutnya"), berpasangan dengan `FE-AST-018` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/AssetManagement/PhysicalInventory/**`, `AssetManagementServiceCollectionExtensions.cs` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `424d3f07` (branch `rizkiG`) |
| Tanggal | 24 November 2026 |
| Status | ✅ SELESAI 24 November 2026 — kedua kriteria terpetakan ke source dan dibuktikan runtime **23/23**; isi ekspor xlsx diperiksa langsung; build Rizki DLL 2026-11-24 08:56:45 |

**Backend Governance Preflight**

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AssetManagement` / `PhysicalInventory` |
| Pemilik / prefix registry | `AssetManagement / PhysicalInventory / Inventarisasi Fisik`, prefix `Ast`, `ACTIVE` — pada `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` repository (yang dibaca checker QBE). Salinan registry di suite `quilvian-engineering-skills` 1.17.1 tidak memuat baris `Ast`; task ini tidak menambah entity sehingga `QBE-MOD-002` tidak terpicu, tetapi selisih salinan itu perlu diselaraskan pemilik suite |
| Keberlakuan | `NEW CODE` (+ `TOUCHED`: `StockTakeSessionService.Query`, DTO, pendaftaran DI) |
| QBE yang berlaku | `QBE-API-*`, `QBE-PERM-*` (atribut `AssetStockTake : Read` dan `AssetReport : Read` identik dengan controller asalnya), `QBE-SVC-*` |
| Tidak berlaku | `QBE-DB-*` — tanpa migration; `QBE-MOD-002` — tanpa entity baru |

---

## 1. Masalah yang diperbaiki

Dashboard aset belum memberi tahu bahwa ada opname yang lewat jadwal tetapi belum diverifikasi, maupun selisih
opname yang usulannya belum ditindaklanjuti (`AST-DEC-031`). Laporan inventaris (menu 11.4) belum ada, sehingga
rekap hasil opname per sesi hanya bisa dilihat satu per satu lewat layar pencocokan.

---

## 2. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `PhysicalInventory/Services/StockTakeQueryService.cs` | Baru — ringkasan kartu 1.5; laporan per sesi yang mengisi hitungan kategori hasil dan hitungan usulan dalam dua kueri kelompok; ekspor xlsx 27 kolom lewat `AssetReportWorkbook`, dibatasi `MaxExportRows` 10.000 (`AST-RPT-EXPORT-LIMIT`) |
| `PhysicalInventory/Controllers/AssetStockTakeDashboardController.cs` | Baru — `GET physical-inventory/dashboard/summary`, atribut identik `AssetStockTake : Read` |
| `PhysicalInventory/Controllers/AssetStockTakeReportsController.cs` | Baru — `GET physical-inventory/reports/stock-takes`, `/export`, `/filters/metadata`, atribut identik resource `AssetReport` milik register |
| `PhysicalInventory/Services/StockTakeSessionService.Query.cs` | Saringan `overdue` dan `hasOpenFollowUps` di kueri sesi bersama (dipakai daftar jadwal dan laporan); `ScopedFor`, `Overdue`, `WithOpenFollowUps`, `ListForExportAsync` internal; metadata memuat dua parameter baru |
| `PhysicalInventory/DTOs/AssetStockTakeDtos.cs` | `StockTakePagedQuery` + `Overdue`, `HasOpenFollowUps`; `StockTakeSessionResponse` + `FollowUpCounts`; baru `StockTakeFollowUpCountsResponse`, `StockTakeDashboardQuery`, `StockTakeSummaryResponse` |
| `AssetManagementServiceCollectionExtensions.cs` | + `StockTakeQueryService` |

Tanpa baris komentar.

### 2.1 Aturan kartu 1.5

| Angka | Aturan di source |
| --- | --- |
| `overdueSessions` — "Opname Lewat Jadwal" | Sesi dalam cakupan pengguna berstatus `Dijadwalkan`, `Berjalan`, `Pencocokan`, atau `MenungguVerifikasi` dengan tanggal selesai rencana sebelum hari operasional (`AssetRegisterSettingReader.Today`). Sama dengan penanda `isPastPlannedEnd` yang sudah ada di sesi |
| `openFollowUps` — "Selisih Belum Ditindaklanjuti" | Usulan berstatus `Usulan` pada sesi dalam cakupan |
| `pendingTotal` — "Inventaris Tertunda" | Jumlah keduanya |
| `sessionsWithOpenFollowUps` | Sesi yang masih punya usulan `Usulan` |

Cakupan sama dengan daftar sesi: tanpa hak seluruh unit, hanya sesi unit pengguna, yang ia jadwalkan, atau tempat
ia menjadi anggota tim. `organizationUnitId` opsional, ditolak bila unit di luar akses. Setiap kartu membawa
`filter` (`overdue=true`, `hasOpenFollowUps=true`) untuk penelusuran.

### 2.2 Delta terhadap kontrak

| Delta | Alasan |
| --- | --- |
| Dua controller (`AssetStockTakeDashboard`, `AssetStockTakeReports`); arsitektur §5.3 menulis satu `AssetStockTakeDashboardController` untuk 2.3 dan 2.4 | Hak aksesnya berbeda resource (`AssetStockTake : Read` vs `AssetReport : Read`), sedangkan `[AccessController]` berlaku per kelas. Sama dengan `BE-AST-019` (dashboard dan laporan mutasi) |
| + `GET /reports/stock-takes/filters/metadata` | Layar laporan membutuhkan metadata saringan, sama dengan laporan mutasi |
| `StockTakeSessionResponse` + `followUpCounts`; `resultCounts` kini juga terisi di laporan | Laporan 11.4 memuat hitungan per kategori dan status usulan per sesi tanpa membuka sesi satu per satu; keduanya `null` bila sesi belum punya hasil/usulan |
| `StockTakePagedQuery` + `overdue`, `hasOpenFollowUps` | Penelusuran kartu 1.5 ke daftar jadwal (`AssetStockTake : Read`, hak yang sama dengan kartu) |
| `StockTakeSummaryResponse` memuat `cards[]` (`AssetDashboardCardResponse`) dan angka rincian | Bentuk kartu sama dengan dashboard register dan mutasi |
| Laporan hanya menegakkan `AssetReport : Read` + cakupan unit | Sama dengan laporan register dan mutasi; aturan "ditambah `: Read` resource sumbernya" pada peta menu adalah urusan tampilnya menu |

---

## 3. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| QBE `-Mode Strict` (ExplicitFiles, 6 berkas) | `VIOLATION: 0`, `REVIEW: 0`, `Final result: PASS` | `PASS` |
| Build | DLL 2026-11-24 08:56:45 (Rizki); backend dijalankan Rizki 08:57:53 | `PASS` |
| Runtime | **23/23** (putaran 2) — `QuilvianSystemFrontendDev/test-with-agy/asset-management/be-ast-024-20261124/ast024.mjs`, hasil `ast024.json` | `PASS` |
| Ekspor | `laporan-inventaris.xlsx` dibongkar dan dibaca langsung — 3.2 | `PASS` |

Skrip tidak mengubah policy siapa pun: SuperAdmin (bukan anggota tim, pemegang `Create` berakses unit) menjalankan alur
sesi, Dimas memindai sebagai anggota tim, akun penyetuju aset (tanpa `AssetStockTake : Read` dan `AssetReport : Read`,
dibaca dari policy saat uji) menjadi aktor tanpa hak.

**Putaran 1 — 22/23.** `EKSPOR-XLSX` gagal karena skrip memanggil `unzip` dari Node, padahal `unzip` hanya ada di PATH
Git Bash; isi berkas sebenarnya benar (diperiksa manual, 3.2). Skrip beralih ke `C:/Windows/System32/tar.exe`; bukti
putaran 1 disimpan `ast024-putaran1.json`.

### 3.1 Skenario runtime (putaran 2, sesi `AST-OPN-000022`)

| Skenario | Hasil |
| --- | --- |
| Hak `AssetStockTake` tetap 8 aksi; `AssetReport` memuat `Read` | Lulus |
| Bentuk ringkasan: 2 kartu, filter `overdue`/`hasOpenFollowUps`, `pendingTotal` = jumlah keduanya | Awal 0 / 0 / 0 |
| Jadwalkan sesi bertanggal 22–23 Nov 2026 | 200, `isPastPlannedEnd = true` |
| `AT-INV-11` kartu bertambah | Lewat jadwal 0 → 1, tertunda 0 → 1 |
| Saring unit Instalasi Rawat Inap | Nama unit terisi, lewat jadwal 1 |
| Daftar `overdue=true` | Memuat sesi, semua baris `isPastPlannedEnd` |
| `overdue=true&status=Diverifikasi` | 0 baris |
| Laporan sebelum pencocokan | `resultCounts` dan `followUpCounts` kosong |
| Mulai, pindai 2 aset, tutup, 21 catatan, ajukan | Menunggu Verifikasi; masih terhitung lewat jadwal |
| Verifikasi | Lewat jadwal 1 → 0; selisih belum ditindaklanjuti 0 → 21; sesi dengan usulan terbuka 1; tertunda 21 |
| Laporan `hasOpenFollowUps=true` — hitungan kategori | Sama persis dengan rincian sesi: total 22, sesuai 1, tidak ditemukan 20, beda lokasi 1, selisih 21 |
| Hitungan usulan | Total 21, terbuka 21, diproses 0, diabaikan 0 |
| Saringan selisih terbuka | Semua baris punya usulan terbuka |
| Paginasi `pageSize=1` | 22 data, 22 halaman |
| Metadata laporan | Memuat `overdue`, `hasOpenFollowUps` |
| Ekspor `hasOpenFollowUps=true` | 200 xlsx, berisi judul, saringan, kolom Usulan Terbuka, nomor sesi |
| Ekspor tanpa hasil | 200 xlsx |
| Akun tanpa hak: ringkasan, laporan, ekspor | 403 ketiganya |
| Rapikan 21 usulan | Semua 200; kartu kembali 0 / 0 |

### 3.2 Isi ekspor (putaran 1, `AST-OPN-000021`, dibaca langsung)

Baris info: "Laporan Inventaris Aset", "Dicetak: 24 November 2026 08.59 WIB oleh SuperAdmin", "Saringan: Hanya sesi
dengan selisih belum ditindaklanjuti", "Jumlah sesi: 1". 27 kolom dari No sampai Alasan Keputusan. Baris data: Instalasi
Rawat Inap, Ruang Uji U9KOL A, rencana 22 dan 23 Nov 2026 (bertipe tanggal), Diverifikasi, Lewat Jadwal "Tidak", ketua
Dimas Kurniawan, tim 2, diharapkan 22, dipindai 2, sesuai 1, sedang dipinjam 0, tidak ditemukan 20, beda lokasi 1, beda
kondisi 0, tidak terdaftar 0, selisih 21, usulan terbuka 21, diproses 0, diabaikan 0, pengaju dan verifikator SuperAdmin.

### 3.3 Data uji

Sesi `AST-OPN-000021` dan `000022` berstatus `Diverifikasi` dengan seluruh usulannya `Diabaikan`; tidak ada dokumen
tujuan yang dibuat. Kartu 1.5 kembali ke angka awal (0 / 0).

---

## 4. Acceptance criteria dan Definition of Done

| Kriteria | Source | Bukti | Status |
|---|---|---|---|
| 1. Kartu: sesi lewat tanggal belum diverifikasi + usulan `Usulan` | `StockTakeQueryService.GetSummaryAsync`, `StockTakeSessionService.Overdue` | `AT-INV-11`, pergeseran kartu sesudah verifikasi, saring unit, 403 | ✅ |
| 2. Laporan per sesi dan kategori, ekspor | `GetReportAsync`, `ExportReportAsync`, `FillCountsAsync` | Hitungan cocok rincian, paginasi, metadata, isi xlsx, 403 | ✅ |

| DoD | Status |
|---|---|
| `V-BE` QBE, build, runtime | Terpenuhi |
| Laporan, roadmap, traceability | Ditulis |
| Commit | Bersama `FE-AST-018`, menunggu izin Rizki |

---

## 5. Langkah berikutnya

- Sub-modul `inventarisasi` lengkap 9/9 bersama `FE-AST-018`.
- Berikutnya jalur `nilai`: tuntaskan `BE-AST-015` (🟡), lalu `BE-AST-025` (migration `AddAssetValuationProcessing` dibuat Rizki).
