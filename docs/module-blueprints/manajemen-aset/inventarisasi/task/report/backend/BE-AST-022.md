# Laporan Perubahan Backend — `BE-AST-022`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-AST-022` |
| Judul | Pemindaian dan pencocokan |
| Gelombang | `MVP-3` — sub-modul `inventarisasi` |
| Roadmap | `inventarisasi/roadmap/backend-roadmap.md` — kartu `BE-AST-022` |
| Trace | `CAP-07.2`, `07.3`; `AST-DES-027..029`; `contracts/api-contract.md` 2.1 (pindai, temuan, tutup, hasil); `contracts/state-transition-matrix.md` §1–2; `contracts/validation-matrix.md` `AST-VAL-INV-010..016`, `020` |
| Contract version | `AST-INV-1.0.0` (bagian dari `AST-BP-001` rev 1, `approved` 2026-11-12) |
| Dependency | `BE-AST-021` ✅, `BE-AST-008` [REG] ✅ |
| Klasifikasi | `STANDARD` — 2 service baru, 1 partial service, 8 endpoint baru pada controller yang sama; tanpa migration |
| Task mode | `BACKEND`, diminta Rizki 2026-11-23 ("mulai pengerjaan BE-AST-022 bersama FE-AST-016") |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/AssetManagement/PhysicalInventory/**`, `AssetManagementServiceCollectionExtensions.cs` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `e719258e` (branch `rizkiG`) |
| Tanggal | 23 November 2026 |
| Status | ✅ SELESAI 23 November 2026 — kelima kriteria terpetakan ke source dan dibuktikan runtime **37/37** serta DB baca-saja; build Rizki DLL 2026-11-23 14:40:09 |

**Backend Governance Preflight**

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AssetManagement` / `PhysicalInventory` |
| Pemilik / prefix registry | `AssetManagement / PhysicalInventory / Inventarisasi Fisik`, prefix `Ast`, `ACTIVE` |
| Keberlakuan | `NEW CODE` (+ `TOUCHED`: pendaftaran DI) |
| QBE yang berlaku | `QBE-API-*`, `QBE-PERM-*` (aksi `Scan`, `Close` selaras antara `[AccessAction]` dan `[AccessPermission]`), `QBE-SVC-*`, `QBE-MOD-001`/`002` |
| Tidak berlaku | `QBE-DB-*` — tanpa migration |

---

## 1. Masalah yang diperbaiki

Sesi opname sudah bisa dimulai, tetapi tim belum bisa mencatat pindaian, temuan tidak terdaftar, maupun menutup
pemindaian untuk menghitung hasil. Tanpa itu snapshot yang dibekukan `BE-AST-021` tidak pernah dibandingkan dengan
keadaan fisik.

---

## 2. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `PhysicalInventory/Services/StockTakeScanService.cs` | Baru — cari nomor (`lookup`), pindai (foto opsional), temuan tidak terdaftar (keterangan + foto wajib), daftar aset diharapkan (`scanned` true/false), riwayat pindaian. Hanya anggota tim, hanya saat `Berjalan`; pindai ulang membuat pindaian lama `IsLatest=false`; `ScannedCount` = aset diharapkan yang punya pindaian terakhir |
| `PhysicalInventory/Services/StockTakeMatchingService.cs` | Baru — menghitung enam kategori dari snapshot + pindaian terakhir; hasil lama dihapus lunak, catatan tim dibawa ke hasil baru (kunci aset atau pindaian); daftar hasil bersaring kategori; hitungan per kategori |
| `PhysicalInventory/Services/StockTakeSessionService.Counting.cs` | Baru — tutup pemindaian (`Berjalan` → `Pencocokan`, hasil dihitung dalam transaksi yang sama) dan buka kembali dengan alasan (`Pencocokan` → `Berjalan`) |
| `StockTakeSessionService.cs`, `.Query.cs` | + `StockTakeMatchingService`; aksi `scan`, `closeCounting`, `reopenCounting`; rincian memuat `findingCount` dan `resultCounts`; `GetResultsAsync` memeriksa hak lihat sesi |
| `PhysicalInventory/Services/AssetStockTakeErrors.cs` | + kode `AST-VAL-INV-010..016`, pesan, label kategori |
| `PhysicalInventory/DTOs/AssetStockTakeDtos.cs` | + permintaan pindai/temuan (multipart), query item, respons lookup/pindaian/aset diharapkan/hasil/hitungan |
| `PhysicalInventory/Controllers/AssetStockTakesController.cs` | + 8 endpoint; resource `AssetStockTake` kini 6 aksi (`Read`, `Create`, `Start`, `Scan`, `Close`, `Cancel`) |
| `AssetManagementServiceCollectionExtensions.cs` | + `StockTakeMatchingService`, `StockTakeScanService` |

Tanpa baris komentar.

### 2.1 Aturan pencocokan

| Kategori | Aturan di source |
| --- | --- |
| `Sesuai` | Pindaian terakhir di ruangan yang diharapkan, kondisi sama |
| `SesuaiSedangDipinjam` | Aset diharapkan dengan unit peminjam; ruangan pindaian milik unit peminjam; kondisi sama |
| `TidakDitemukan` | Aset diharapkan tanpa pindaian terakhir |
| `BedaLokasi` | Ruangan berbeda (untuk aset dipinjam: bukan unit peminjam); atau aset di luar snapshot (`IsForeignAsset` bila unit pemiliknya berbeda, `AST-DES-028`) — `IsConditionMismatch` ikut diisi |
| `BedaKondisi` | Lokasi sesuai, kondisi berbeda (termasuk aset dipinjam di unit peminjam) |
| `TidakTerdaftar` | Setiap temuan tidak terdaftar |

### 2.2 Delta terhadap kontrak

| Delta | Alasan |
| --- | --- |
| + `GET /{id}/lookup?code=` (hak `Scan`) | Layar pindai perlu ruangan/kondisi tercatat, status pinjaman, dan pindaian sebelumnya dari sudut sesi; `/register/assets/by-number` tidak memuatnya dan menuntut hak `AssetRegister : Read` yang belum tentu dimiliki tim |
| `GET /{id}/expected-items` memakai `StockTakeExpectedItemResponse` (kontrak menulis `StockTakeResultResponse`) | Sebelum ditutup belum ada hasil; yang ditampilkan adalah snapshot + pindaian terakhir |
| `POST /{id}/close-counting` menerima `{ rowVersion }` | Konkurensi, sama dengan `start` |
| Rincian sesi memuat `findingCount`, `resultCounts` | Penghitung layar pindai dan pencocokan |
| Tutup/buka kembali oleh ketua **atau** pemegang `Create` berakses unit | Matriks state menulis "Ketua tim"; peta peran memberi `Close` juga kepada Petugas Aset. Aturan sama dengan mulai sesi (`BE-AST-021`) |
| `PUT /{id}/results/{resultId}` (catatan tim) belum dibuat | Dipakai saat mengajukan verifikasi (`BE-AST-023` kriteria 1) |
| Unduhan foto pindaian (`OwnerType` `StockTakeScan`) belum diizinkan `AssetAttachmentAccessService` | Baru dibutuhkan layar selisih (`FE-AST-017`); dicatat untuk `BE-AST-023` |

---

## 3. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| QBE `-Mode Strict` (ExplicitFiles, 9 berkas) | `VIOLATION: 0`, `REVIEW: 0`, `Final result: PASS` | `PASS` |
| Build | DLL 2026-11-23 14:40:09 (Rizki) | `PASS` |
| Runtime | **37/37** — `QuilvianSystemFrontendDev/test-with-agy/asset-management/be-ast-022-20261123/ast022.mjs`, hasil `ast022.json` | `PASS` |
| DB baca-saja | `cek-db.fsx` (3.2) | `PASS` |

Tim uji: Siti Nurhaliza (bukan SuperAdmin) ketua dengan hak sementara `Read`/`Start`/`Scan`/`Close` tanpa `Create`,
Dimas Kurniawan anggota; SuperAdmin dan akun penyetuju aset tanpa pegawai sebagai bukan-anggota. Policy dibaca segar,
diubah sementara, dan dipulihkan (`RESTORE-POLICY-TERBUKTI` lulus).

### 3.1 Skenario runtime

| Skenario | Hasil |
| --- | --- |
| Hak `AssetStockTake` 6 aksi; swagger 16 operasi | Lulus |
| Ketua tim tanpa `Create` memulai sesi (jalur positif yang belum teruji di `BE-AST-021`) | 200, 44 aset diharapkan |
| Daftar diharapkan memuat aset dipinjam ke Instalasi Gawat Darurat | Lulus |
| Lookup: aset diharapkan (nomor huruf kecil dinormalkan), aset dipinjam, aset unit lain (`isExpected=false`) | Lulus |
| `AST-VAL-INV-012` nomor tak dikenal | 404 menyebut nomornya |
| `AT-INV-32` bukan anggota tim | 403 `AST-VAL-INV-010` |
| `AST-VAL-INV-013` ruangan kosong; `015` kondisi Hilang | 400 |
| Pindai dengan foto; pindai oleh anggota non-SuperAdmin (manual); pindai beda kondisi | 200; foto tersimpan; `isManualEntry` dan pemindai tercatat |
| `AT-INV-06` pindai ulang aset yang sama | Riwayat 2 pindaian, tepat 1 `IsLatest`, hitungan dipindai tidak bertambah |
| `AT-INV-05` aset dipinjam dipindai di ruangan IGD | 200 |
| `AST-VAL-INV-016` temuan tanpa foto | 400; dengan foto 200, `assetId` null |
| Saring sudah/belum dipindai | 5 / 39 |
| Tutup oleh akun yang berhak `Close` tetapi bukan ketua dan tanpa `Create` | 403 `AST-INV-SCOPE` |
| `AT-INV-04` tutup oleh ketua | `Pencocokan`; Sesuai 2, Sesuai Sedang Dipinjam 1, Tidak Ditemukan 39, Beda Lokasi 2, Beda Kondisi 1, Tidak Terdaftar 1 (total 46 = 44 + aset unit lain + temuan) |
| Hasil per aset | a1 Sesuai, a2 Beda Lokasi + `isLocationMismatch`, a3 Beda Kondisi + `isConditionMismatch`, a4 (pindai ulang) Sesuai, aset dipinjam Sesuai Sedang Dipinjam |
| `AST-DES-028` aset IGD ditemukan di Rawat Inap | Beda Lokasi, `isForeignAsset`, pemilik Instalasi Gawat Darurat |
| Saring kategori ganda (`TidakTerdaftar` + `TidakDitemukan`) | 40 |
| `AT-INV-33` pindai sesudah ditutup | 409 `AST-VAL-INV-011` |
| Buka kembali: tanpa alasan → 400 `AST-VAL-INV-020`; dengan alasan → `Berjalan` | Lulus |
| Pindai satu aset lagi, tutup ulang oleh SuperAdmin (pemegang `Create`) | Sesuai 3, Tidak Ditemukan 38 |

### 3.2 DB baca-saja (sesi `AST-OPN-000010`)

| Pemeriksaan | Hasil |
| --- | --- |
| Pindaian | 9 baris: 8 `IsLatest`, 1 digantikan |
| Hasil | 46 aktif; 46 dari tutup pertama terhapus lunak |
| Foto | 2 lampiran `StockTakeScan` (pindaian + temuan) |
| Aset tidak berubah | Linimasa `Diopname` = 0 (perubahan aset baru lewat verifikasi `BE-AST-023`, `AST-DEC-019`) |
| Data uji | Sesi dibatalkan; pinjaman `AST-PJM-000013` dikembalikan |

---

## 4. Acceptance criteria dan Definition of Done

| Kriteria | Source | Bukti | Status |
|---|---|---|---|
| 1. Hanya anggota tim saat `Berjalan` | `RequireMemberAsync`, `EnsureRunning` | `AT-INV-32`, `AT-INV-33` | ✅ |
| 2. Pindai ulang menggantikan (`IsLatest`) | `ScanAsync` | `AT-INV-06`, DB | ✅ |
| 3. Temuan wajib keterangan + foto | `RecordFindingAsync` | `AST-VAL-INV-016`, temuan 200 | ✅ |
| 4. Tutup menghitung enam kategori; buka kembali dengan alasan | `StockTakeMatchingService.ComputeAsync`, `CloseCountingAsync`, `ReopenCountingAsync` | `AT-INV-04`, buka kembali, tutup ulang | ✅ |
| 5. Aset unit lain yang ditemukan → `BedaLokasi` (`AST-DES-028`) | `ComputeAsync` (pindaian di luar snapshot) | Hasil aset IGD | ✅ |

| DoD | Status |
|---|---|
| `V-BE` QBE, build, runtime, DB | Terpenuhi |
| Laporan, roadmap, traceability | Ditulis |
| Commit | Bersama `FE-AST-016` |

---

## 5. Langkah berikutnya

- `BE-AST-023`: ajukan verifikasi (catatan selisih wajib, `PUT results/{id}`), verifikasi oleh Kepala Bagian Aset
  bukan anggota tim, usulan tindak lanjut, PDF berita acara, izin unduh foto `StockTakeScan`.
