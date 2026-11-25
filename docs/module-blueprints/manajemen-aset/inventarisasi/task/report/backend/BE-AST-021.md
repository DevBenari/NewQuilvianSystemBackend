# Laporan Perubahan Backend — `BE-AST-021`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-AST-021` |
| Judul | Sesi opname dan gerbang penahanan |
| Gelombang | `MVP-3` — sub-modul `inventarisasi` |
| Roadmap | `inventarisasi/roadmap/backend-roadmap.md` — kartu `BE-AST-021` |
| Trace | `CAP-07.1`, `AST-DEC-018`; `AST-DES-008`, `027`, `031`; `contracts/api-contract.md` 2.1 (jadwal, mulai, batal); `contracts/state-transition-matrix.md` §1; `contracts/validation-matrix.md` `AST-VAL-INV-001..007`, `020`, `023`, `024`; `contracts/integration-contract.md` `INT-INV-01..04` |
| Contract version | `AST-INV-1.0.0` (bagian dari `AST-BP-001` rev 1, `approved` 2026-11-12) |
| Dependency | `BE-AST-020` ✅, `BE-AST-009` [REG] ✅, `BE-AST-017` [MOV] ✅, `BE-AST-018` [MOV] ✅ |
| Klasifikasi | `STANDARD` — 1 controller baru (8 operasi), 1 service baru; menyentuh service `pergerakan` dan `register` untuk gerbang opname |
| Task mode | `BACKEND`, diminta Rizki 2026-11-23 ("lakukan commit, dan lanjutkan pengerjaannya") |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/AssetManagement/**` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `f151eb1e` (branch `rizkiG`) |
| Tanggal | 23 November 2026 |
| Status | ✅ SELESAI 23 November 2026 — kelima kriteria terpetakan ke source dan dibuktikan runtime **45/45** (putaran kedua) serta DB baca-saja; build Rizki DLL 2026-11-23 12:06:17 |

**Backend Governance Preflight**

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AssetManagement` / `PhysicalInventory` (+ `TOUCHED`: `Movement`, `Register`) |
| Pemilik / prefix registry | `AssetManagement / PhysicalInventory / Inventarisasi Fisik`, prefix `Ast`, `ACTIVE` |
| Keberlakuan | `NEW CODE` (PhysicalInventory) + `TOUCHED` (gerbang, kueri pergerakan/penempatan, deret nomor, DI) |
| QBE yang berlaku | `QBE-API-*` (rute, `ApiResponse`), `QBE-PERM-*` (`[AccessController]`/`[AccessAction]`/`[AccessPermission]` selaras), `QBE-SVC-*` (service + transaksi), `QBE-CODE-004` (penomoran lewat `NumberSeriesAllocator`), `QBE-MOD-001`/`002` |
| Tidak berlaku | `QBE-DB-*` — tanpa migration (tabel dari `BE-AST-020`) |

---

## 1. Masalah yang diperbaiki

Tabel opname sudah ada, tetapi belum ada cara menjadwalkan sesi, memulainya, atau membatalkannya. Gerbang penahanan
`AssetStockTakeGate` yang sudah dipanggil pengajuan mutasi, pinjaman, dan penyerahan masih berupa kerangka yang selalu
menjawab "tidak ada unit ditahan". Selain itu mutasi dan pinjaman yang tertahan menjawab kode register
`AST-VAL-REG-026`, padahal matriks validasi pergerakan menetapkan `AST-VAL-MOV-006` beserta nama unit.

---

## 2. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `PhysicalInventory/Services/StockTakeSessionService.cs` | Baru — jadwalkan, ubah jadwal (tim diganti dalam dua tahap supaya index satu ketua tidak terlanggar), mulai, batal; `FindStartBlockersAsync` |
| `PhysicalInventory/Services/StockTakeSessionService.Query.cs` | Baru — `filters/metadata`, ringkasan per status, daftar (status, unit, tahun, `mine`, cari), rincian beserta tim, `availableActions`, dan `startBlockers` |
| `PhysicalInventory/Services/AssetStockTakeErrors.cs` | Baru — kode `AST-VAL-INV-001..007`, `020`, `023`, `024`, `AST-INV-FIELD/NOT-FOUND/SCOPE`, pesan, label status |
| `PhysicalInventory/DTOs/AssetStockTakeDtos.cs` | Baru — permintaan jadwal/versi/keputusan, respons sesi, tim, penahan mulai, ringkasan |
| `PhysicalInventory/Controllers/AssetStockTakesController.cs` | Baru — `physical-inventory/stock-takes`, resource `AssetStockTake` aksi `Read`, `Create`, `Start`, `Cancel`; modul akses `CORPORATE_ASSET_MANAGEMENT_PHYSICAL_INVENTORY` |
| `Register/Services/AssetStockTakeGate.cs` | Diisi — sesi berstatus `Berjalan`, `Pencocokan`, `MenungguVerifikasi` menahan unit; + `IsUnitUnderStockTakeAsync`, `FindUnitNameInStockTakeAsync` |
| `Movement/Services/AssetMovementErrors.cs`, `AssetTransferService.cs`, `AssetLoanService.cs` | Pengajuan mutasi dan pinjaman memakai `AssetMovementStockTake.EnsureNotHeldAsync` → `AST-VAL-MOV-006` dengan nama unit |
| `Movement/Services/MovementQueryService.cs` | Status pinjaman penahan mulai opname tanpa `Dipinjam`; + `FindLoanedOutAssetsAsync` (aset `Dipinjam` milik unit → unit peminjam) |
| `Register/Services/AssetPlacementService.Query.cs` | + `FindPendingHandoverNumbersForUnitAsync` (penyerahan `Diserahkan` ke unit itu atau berisi aset unit itu) |
| `Register/Services/AssetNumberingService.cs` | + deret `AST_STOCKTAKE` / `AST-OPN` |
| `AssetManagementServiceCollectionExtensions.cs` | + `StockTakeSessionService` |

Tidak ada baris komentar. `Program.cs` tidak disentuh.

### 2.1 Aturan mulai sesi

1. Status `Dijadwalkan` dan `rowVersion` cocok.
2. Pelaku adalah ketua tim, **atau** pemegang `AssetStockTake : Create` yang berakses ke unit. Peran tidak di-hardcode.
3. Hari ini (WIB) ≥ tanggal mulai rencana (`AST-VAL-INV-005`).
4. Aset unit dikunci `FOR UPDATE` lebih dulu, lalu diperiksa: tidak ada sesi aktif lain (`007`), tidak ada mutasi
   belum diterima, pinjaman `Diajukan`/`Disetujui`/`MenungguKonfirmasiKembali`, atau penyerahan `Diserahkan` (`006`,
   pesan menyebut jumlah dan nomornya).
5. Snapshot: aset `Terdaftar` + `Ditempatkan` di unit (dan ruangan bila dipilih), berikut ruangan, penanggung jawab,
   kondisi, dan unit peminjam bila sedang `Dipinjam`.

Pengajuan mutasi/pinjaman/penyerahan juga mengunci aset `FOR UPDATE`, sehingga pengajuan yang bersamaan dengan
mulai sesi menunggu kunci, lalu melihat sesi `Berjalan` dan ditolak gerbang.

### 2.2 Delta terhadap kontrak

| Delta | Alasan |
| --- | --- |
| + `GET /filters/metadata`, + `GET /summary` | Layar daftar (pola sub-modul lain); `summary` ini hitungan per status, bukan kartu 1.5 (`BE-AST-024`) |
| `POST /{id}/start` menerima `{ rowVersion }` (kontrak: tanpa body) | Konkurensi, sama dengan `pickup`/`return` pinjaman |
| Rincian memuat `startBlockers` | Layar dapat menampilkan proses penahan sebelum menekan Mulai (`FE-AST-015` kriteria 2) |
| Gerbang tetap `AssetStockTakeGate` di `Register/Services`, bukan `StockTakeGateQueryService` baru di `PhysicalInventory` | Titik panggil sudah dipasang `BE-AST-009`/`017`/`018`; memindahkan kelas hanya mengubah tiga konstruktor tanpa manfaat |
| `PUT /{id}` memakai aksi `Create` | Sesuai kolom hak akses kontrak (`AssetStockTake : Create`) |
| Kode tambahan `AST-INV-FIELD` (tanggal kosong, catatan > 1000, `rowVersion` kosong), `AST-INV-NOT-FOUND`, `AST-INV-SCOPE` | Pola `AST-MOV-*`; belum ada di matriks validasi |

### 2.3 Tafsiran yang perlu diketahui pemilik

Matriks state §1 menulis "unit tanpa penyerahan/mutasi/pinjaman yang belum selesai", sedangkan `AST-DES-027`,
`AT-INV-05`, dan kriteria 3 kartu ini mengharapkan aset yang sedang dipinjam keluar ikut snapshot. Bila pinjaman
`Dipinjam` ikut menahan, aset yang dipinjam keluar tidak akan pernah masuk snapshot. Maka yang menahan hanya pinjaman
`Diajukan`, `Disetujui`, dan `MenungguKonfirmasiKembali` (aset sudah kembali secara fisik tetapi belum dikonfirmasi).
Pinjaman `Dipinjam` tidak menahan dan asetnya masuk snapshot dengan unit peminjam. `HasInFlightForUnitAsync` dan
`FindInFlightForUnitAsync` (dibuat `BE-AST-019` khusus untuk inventarisasi) ikut memakai definisi ini; kartu dashboard
1.4 tidak berubah.

---

## 3. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| QBE `-Mode Strict` (ExplicitFiles, 13 berkas) | `VIOLATION: 0`, `REVIEW: 0`, `INFO: 0`, `Final result: PASS` | `PASS` |
| Build | DLL 2026-11-23 12:06:17 (build ulang Rizki sesudah bersih disk); putaran pertama memakai DLL 11:33:41 | `PASS` |
| Runtime putaran 1 | 39/44 — kelima FAIL adalah skrip membandingkan status sebagai teks, padahal enum JSON berupa angka; nilai di baris FAIL sendiri benar. Sisa data dirapikan `rapikan.mjs` | Skrip diperbaiki |
| Runtime putaran 2 | **45/45** | `PASS` |
| DB baca-saja | Lihat 3.2 | `PASS` |

Skrip: `QuilvianSystemFrontendDev/test-with-agy/asset-management/be-ast-021-20261123/ast021.mjs` (hasil `ast021.json`,
putaran pertama `ast021-putaran1.json`), `cek-db.fsx`. Policy Siti (Kepala Unit Instalasi Rawat Inap) dan akun penyetuju
aset dibaca segar di awal, diubah sementara, dan dipulihkan di akhir (`RESTORE-POLICY-TERBUKTI` lulus).

### 3.1 Skenario runtime

| Skenario | Hasil |
| --- | --- |
| Registrasi hak `AssetStockTake` = `Read`, `Create`, `Start`, `Cancel`; swagger 8 operasi | Lulus |
| `AST-VAL-INV-001` unit kosong; `002` selesai < mulai; `003` tanpa tim, dua ketua, tanpa ketua, pegawai tak ada; `004` ruangan unit lain | 400 dengan kode yang tepat |
| `AT-INV-01` jadwalkan IGD, tim 2 orang | `AST-OPN-000005`, status `Dijadwalkan`, tepat satu ketua, aksi `edit`/`start`/`cancel` |
| Ubah jadwal: ganti ketua dan anggota; `rowVersion` usang | 200, anggota lama dihapus; 409 `AST-VAL-INV-024` |
| `AST-VAL-INV-005` mulai sebelum tanggal | 409 "Sesi baru dapat dimulai pada 28 November 2026." |
| `AT-INV-30` mulai saat ada mutasi `AST-MUT-000014` belum diterima | 409 `AST-VAL-INV-006` menyebut nomornya; rincian `startBlockers.transferNumbers` memuatnya |
| Pinjaman `Disetujui` menahan mulai | 409 menyebut `AST-PJM-000012` |
| Mulai oleh akun yang berhak `Start` tetapi bukan ketua dan tanpa `Create` | 403 `AST-INV-SCOPE` |
| `AT-INV-02` mulai sesi Instalasi Rawat Inap | `Berjalan`, `ExpectedCount` 44 = jumlah aset terdaftar dan ditempatkan di unit itu; aksi tinggal `cancel` |
| `AT-INV-31` jadwal atau pindah jadwal ke unit yang sedang opname | 409 `AST-VAL-INV-007` menyebut `AST-OPN-000007` |
| `AT-INV-03` / `AT-MOV-24` ajukan mutasi dan pinjaman dari unit yang sedang opname | 409 `AST-VAL-MOV-006` "…inventarisasi unit Instalasi Rawat Inap selesai." |
| `AT-REG-38` penyerahan di unit yang sedang opname | 409 `AST-VAL-REG-026` |
| Daftar: saring status+unit, cari nomor, tahun; ringkasan = hitungan daftar; metadata 6 status | Lulus |
| Hak: LOKET tanpa hak → 403 pada daftar, jadwalkan, batal | Lulus |
| Batal: alasan kosong → 400 `AST-VAL-INV-020`; batal sesi `Berjalan` → `Dibatalkan`; batal lagi → 409 `AST-VAL-INV-023` | Lulus |
| Gerbang lepas sesudah batal: mutasi dari unit itu kembali diterima | 200 |

### 3.2 DB baca-saja (sesi `AST-OPN-000007`)

| Pemeriksaan | Hasil |
| --- | --- |
| Baris `AstStockTakeExpectedItem` | 44 = `ExpectedCount` |
| Aset yang dipinjam keluar | 1 baris, `LoanBorrowerOrganizationUnitId` = Instalasi Gawat Darurat (aset pinjaman uji) |
| Snapshot = keadaan aset (ruangan, penanggung jawab, kondisi) | 44/44 cocok |
| Data uji | `AST-OPN-000001..008` semuanya `Dibatalkan`; `AST-PJM-000011`, `000012` `Dikembalikan`; mutasi uji dibatalkan |

---

## 4. Acceptance criteria dan Definition of Done

| Kriteria | Source | Bukti | Status |
|---|---|---|---|
| 1. Jadwal dengan tim tepat satu ketua | `ValidateScheduleAsync`, `IX_AstStockTakeTeamMember_LeaderPerSession` | `AT-INV-01`, `VAL-INV-003` ×4 | ✅ |
| 2. Mulai menolak bila ada penyerahan/mutasi/pinjaman berjalan, dan menyebut nomornya | `StartAsync`, `FindStartBlockersAsync` | `AT-INV-30`, pinjaman `Disetujui` | ✅ (penyerahan diperiksa lewat kueri yang sama; tidak ada penyerahan `Diserahkan` saat uji) |
| 3. Snapshot dibekukan, termasuk aset dipinjam keluar | `StartAsync` + `FindLoanedOutAssetsAsync` | `AT-INV-02`, DB 3.2 | ✅ |
| 4. Gerbang dipanggil pengajuan mutasi, pinjaman, penyerahan (`AST-VAL-MOV-006`, `AST-VAL-REG-026`) | `AssetStockTakeGate`, `AssetMovementStockTake` | `AT-INV-03`/`AT-MOV-24`, `AT-REG-38` | ✅ |
| 5. Batal hanya Kepala Bagian Aset | `[AccessPermission("AssetStockTake", "Cancel")]` — hak diberikan lewat Akses Role, bukan nama peran | LOKET 403; batal oleh pemegang hak | ✅ |

| DoD | Status |
|---|---|
| `V-BE` QBE, build, runtime, DB | Terpenuhi |
| Laporan, roadmap, traceability | Ditulis |
| Commit | Bersama `FE-AST-015` |

Belum diuji: mulai oleh ketua tim yang tidak memegang `Create` (jalur positif) — tidak ada akun uji yang pegawainya
dapat dijadikan ketua tanpa mengubah data HR. Jalur negatif (bukan ketua, tanpa `Create` → 403) lulus.

---

## 5. Langkah berikutnya

- `BE-AST-022` (pemindaian, temuan, pencocokan) bersama `FE-AST-016`.
- Matriks validasi inventarisasi dapat ditambah kode `AST-INV-FIELD/NOT-FOUND/SCOPE` oleh pemilik dokumen.
