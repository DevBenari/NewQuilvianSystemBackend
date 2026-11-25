# Laporan Perubahan Backend — `BE-AST-019`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-AST-019` |
| Judul | Kartu dashboard 1.4 dan laporan mutasi |
| Gelombang | `MVP-2` — sub-modul `pergerakan` |
| Roadmap | `pergerakan/roadmap/backend-roadmap.md` — kartu `BE-AST-019` |
| Trace | `CAP-01.4`, `CAP-11.2`; API 2.3–2.4; `INT-MOV-08` / `INT-INV-02` (`AST-DES-008`); `AT-MOV-04` (kartu), `AT-MOV-07` |
| Contract version | `AST-MOV-1.0.0` (bagian dari `AST-BP-001` rev 1, `approved` 2026-11-12) |
| Dependency | `BE-AST-017` ✅, `BE-AST-018` ✅; memakai `AssetReportWorkbook` dan batas ekspor `AssetReportQueryService.MaxExportRows` [REG] |
| Klasifikasi | `STANDARD` — baca saja, tanpa perubahan status maupun schema |
| Task mode | `BACKEND`, diminta Rizki 2026-11-23 ("lanjutkan langkah selanjutnya") |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/AssetManagement/Movement/**` + DI; laporan ini dan baris status roadmap/traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `0825e310` (branch `rizkiG`) |
| Tanggal | 23 November 2026 |
| Status | ✅ SELESAI 23 November 2026 — ketiga kriteria terpetakan ke source: QBE Strict `PASS`, build Rizki berhasil (DLL 09:07:56), runtime **22/23** (satu FAIL di langkah perapian data uji, diselesaikan ulang 1/1); kriteria 3 diuji pemanggilnya (`BE-AST-021`) |

**Backend Governance Preflight**

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AssetManagement` / `Movement` |
| Pemilik / prefix registry | `AssetManagement / Movement / Pergerakan Aset`, prefix `Ast`, `ACTIVE` |
| Keberlakuan | `NEW CODE` (+ `TOUCHED`: `AssetTransferService.Query`, `AssetLoanService.Query`, DI) |
| QBE yang berlaku | `QBE-MOD-001`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-DTO-001` |
| Tidak berlaku | `QBE-ENT-*`, `QBE-CFG-*`, `QBE-NAM-*`, `QBE-DB-*`, `QBE-CODE-*`, `QBE-TXN-001` — tanpa entity, nomor, maupun tulis |
| Arketipe transaksi | Read-only (ringkasan dan laporan) |

---

## 1. Masalah yang diperbaiki

Dashboard Aset belum menampilkan mutasi dan pinjaman yang sedang berjalan (kartu 1.4), dan belum ada laporan mutasi
per periode dengan ekspor (laporan 11.2). Sub-modul `inventarisasi` juga butuh cara bertanya apakah sebuah unit masih punya
mutasi atau pinjaman yang belum selesai sebelum sesi opname dimulai (`AST-DES-008`).

---

## 2. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `Movement/Services/MovementQueryService.cs` | Baru — `GetSummaryAsync` (kartu 1.4), `GetTransferReportAsync`, `ExportTransferReportAsync`, `HasInFlightForUnitAsync`, `FindInFlightForUnitAsync` |
| `Movement/DTOs/AssetMovementQueryDtos.cs` | Baru — `AssetMovementSummaryQuery`, `AssetMovementSummaryResponse` (memakai ulang `AssetDashboardCardResponse` register), `AssetMovementInFlightResult` |
| `Movement/Controllers/AssetMovementDashboardController.cs` | Baru — `GET movement/dashboard/summary`, resource `AssetTransfer : Read` (atribut identik `AssetTransfersController`) |
| `Movement/Controllers/AssetMovementReportsController.cs` | Baru — `GET movement/reports/transfers`, `/export`, `/filters/metadata`, resource `AssetReport : Read` milik register (atribut identik `AssetReportsController`, sehingga satu centang hak berlaku untuk semua laporan) |
| `Movement/Services/AssetTransferService.Query.cs` | Saringan daftar dipisah ke `Filtered`/`Ordered`; + `ListForExportAsync`, `ScopedFor` (internal). Perilaku `GET /movement/transfers` tidak berubah |
| `Movement/Services/AssetLoanService.Query.cs` | + `ScopedFor`; `Overdue` jadi internal |
| `AssetManagementServiceCollectionExtensions.cs` | + `MovementQueryService` |

Kartu 1.4: tiga kartu mutasi berjalan (`Diajukan`, `DisetujuiUnitAsal`, `DisetujuiBagianAset`) dan, bila pengguna juga
memegang `AssetLoan : Read`, empat kartu pinjaman (diajukan, menunggu diambil, aktif = `Dipinjam` + `MenungguKonfirmasiKembali`,
terlambat). Setiap kartu membawa `filter` (`status` atau `isOverdue`). Cakupan sama dengan daftar: tanpa hak seluruh unit hanya
mutasi/pinjaman unit pengguna; `organizationUnitId` di luar cakupan → 403 `AST-VAL-REG-016`.

Laporan memakai saringan dan cakupan yang persis sama dengan daftar mutasi (`AssetTransferPagedQuery`: periode diajukan
`from`/`to`, unit asal/tujuan, alasan, status, kata kunci). Ekspor Excel 19 kolom tanpa rupiah, baris info memuat pencetak,
saringan, dan jumlah; lebih dari 10.000 baris → 400 `AST-RPT-EXPORT-LIMIT` (kode bersama laporan register).

`HasInFlightForUnitAsync(unitId)` bernilai benar bila unit menjadi asal/tujuan mutasi yang belum diterima/ditolak/dibatalkan,
atau pemilik/peminjam pinjaman yang belum dikembalikan/ditolak/dibatalkan; tanpa cakupan pengguna (dipanggil sistem).
`FindInFlightForUnitAsync` mengembalikan nomor dokumennya (paling banyak 50 per jenis) untuk pesan galat `inventarisasi`.

### 2.1 Delta terhadap `AST-MOV-1.0.0`

| No | Delta | Alasan |
|---|---|---|
| 1 | Dua controller (`AssetMovementDashboardController`, `AssetMovementReportsController`), bukan satu | API 2.3 berhak `AssetTransfer`, API 2.4 berhak `AssetReport` milik register; satu kelas hanya punya satu `[AccessController]` (preseden `BE-AST-015`) |
| 2 | `GET movement/reports/transfers/filters/metadata` | Pemegang `AssetReport` belum tentu memegang `AssetTransfer : Read` untuk metadata daftar mutasi |
| 3 | Kartu pinjaman hanya bila `AssetLoan : Read` (`canReadLoans`) | Kontrak 2.3 hanya mensyaratkan `AssetTransfer : Read`; angka pinjaman tidak dibuka bagi yang tak berhak membaca pinjaman |
| 4 | `filter` kartu pinjaman aktif = `status=Dipinjam,MenungguKonfirmasiKembali` (dipisah koma) | `filter` berbentuk kamus satu nilai; layar menautkan kartu itu ke Pengembalian Aset |
| 5 | Periode laporan = tanggal diajukan | Saringan yang sudah ada di daftar mutasi; satu definisi untuk layar dan Excel |

**Database:** tanpa migration. **Keamanan:** 4/4 pasangan `[AccessAction]`/`[AccessPermission]` cocok; resource `AssetReport`
tetap satu dengan satu aksi `Read` (terbukti di daftar hak).

---

## 3. Verifikasi

### 3.1 Statis dan build

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| QBE `-Mode Strict` (WorkingTree) | `Files evaluated: 7`, `VIOLATION: 0`, `REVIEW: 0`, `Final result: PASS` | `PASS` |
| Baris komentar baru | 0 | `PASS` |
| Build | Rizki, DLL 2026-11-23 09:07:56 memuat `MovementQueryService` | `PASS` |
| Registrasi hak | `AssetReport` satu resource, aksi `["Read"]`; `AssetTransfer` tetap 6 aksi; Swagger 4 operasi baru | `PASS` |

### 3.2 Runtime (HTTPS 7184) — `test-with-agy/asset-management/be-ast-019-20261123/ast019.mjs`

Data disiapkan lewat API lalu dirapikan: mutasi `AST-MUT-000010` (Diajukan → Dibatalkan), pinjaman `AST-PJM-000008`
(Diajukan → Dibatalkan), pinjaman terlambat `AST-PJM-000009` (diambil kemarin-kemarin → Dikembalikan). Policy Siti dan akun
penyetuju diberi hak sementara lalu dipulihkan identik (22 dan 6 butir).

| Langkah | Hasil |
|---|---|
| Registrasi hak dan Swagger (4 operasi) | PASS |
| **AC1** ketujuh kartu = `totalData` daftar dengan saringan yang sama (mutasi 1/0/0, pinjaman 1/0/1/1), `transfersInFlight` = jumlah kartu mutasi | PASS |
| Pinjaman aktif dan terlambat terhitung; `filter` kartu benar | PASS |
| Saringan unit Rawat Inap: kartu = mutasi asal + tujuan unit itu, pinjaman = pemilik + peminjam | PASS |
| Siti dengan `AssetTransfer : Read` tanpa `AssetLoan : Read` → `canReadLoans=false`, kartu pinjaman kosong | PASS |
| Siti meminta unit IGD → 403 `AST-VAL-REG-016`; LOKET → 403 | PASS |
| Metadata laporan | PASS |
| **AC2 / `AT-MOV-07`** laporan periode hari ini = daftar periode sama (1 = 1); periode 1–18 Nov tanpa mutasi hari ini; saringan status | PASS |
| **AC2** ekspor seluruh laporan: 10 baris = 10, nomor mutasi per baris sama urutannya, 0 sel rupiah, nama berkas `laporan-mutasi-aset-yyyyMMdd-HHmm.xlsx` | PASS |
| Ekspor bersaringan (hari ini, unit asal Rawat Inap): 1 baris; baris info "Unit asal: Instalasi Rawat Inap; Periode diajukan: 23 November 2026 s.d. 23 November 2026" | PASS |
| Hak `AssetReport` menentukan akses laporan dan ekspor (penyetuju tanpa hak → 403/403); LOKET → 403 | PASS |
| Perapian data uji | **FAIL** — `AST-PJM-000009` tertahan di Menunggu Konfirmasi Kembali karena policy sementara Siti tidak memuat `ConfirmReturn` (kesalahan skrip). Diselesaikan dengan `rapikan.mjs` → Dikembalikan, policy pulih (`ast019-rapikan.json`); skrip diperbaiki |
| Pemulihan policy | PASS |

**Rekap: 22/23 + perapian ulang 1/1.**

### 3.3 Temuan

| Temuan | Tindak |
|---|---|
| Swagger `corporate` butuh beberapa menit pada permintaan pertama sesudah restart (endpoint lain menjawab < 2 dtk) | Bukan cacat task; uji menunggu Swagger |
| `HasInFlightForUnitAsync` belum dipanggil dari HTTP mana pun | Dipakai `BE-AST-021` saat memulai sesi opname; diuji di sana (preseden `CreateFromStockTakeAsync` di `BE-AST-017`) |
| Uji batas ekspor 10.000 baris tidak dijalankan | Data dev 11 mutasi; pemeriksaan memakai helper yang sama dengan laporan register yang sudah teruji |

---

## 4. Acceptance criteria dan Definition of Done

| Kriteria | Source | Bukti | Status |
|---|---|---|---|
| 1. Ringkasan mutasi berjalan per status, pinjaman aktif, pinjaman terlambat | `MovementQueryService.GetSummaryAsync` | Kartu = daftar; cakupan unit; kartu pinjaman mengikuti hak | ✅ |
| 2. Laporan mutasi per periode dan ekspor | `GetTransferReportAsync`, `ExportTransferReportAsync` | `AT-MOV-07`; Excel sama dengan laporan | ✅ |
| 3. `HasInFlightForUnit` untuk `inventarisasi` | `HasInFlightForUnitAsync`, `FindInFlightForUnitAsync` | Ada di source; pemanggilnya `BE-AST-021` | ✅ (diuji pemanggilnya) |

| DoD | Status |
|---|---|
| `V-BE` QBE, build, registrasi hak, runtime | Terpenuhi |
| Laporan, roadmap, traceability | Ditulis |

Task berikutnya: `FE-AST-014` (pasangan layar), lalu sub-modul `inventarisasi` (`BE-AST-020`).
