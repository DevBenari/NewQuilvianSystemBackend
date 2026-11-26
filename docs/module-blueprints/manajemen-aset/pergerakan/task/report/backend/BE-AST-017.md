# Laporan Perubahan Backend — `BE-AST-017`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-AST-017` |
| Judul | Mutasi antar unit |
| Gelombang | `MVP-2` — sub-modul `pergerakan` |
| Roadmap | `pergerakan/roadmap/backend-roadmap.md` — kartu `BE-AST-017` |
| Trace | `CAP-05.1`–`05.3`; `AST-DEC-011`, `048`; `AST-DES-023..025`; API 2.1; state §1; validasi `AST-VAL-MOV-001..016`; `INT-MOV-01..05`, `07`; `AT-MOV-01..03`, `20..23`, `25..31` |
| Contract version | `AST-MOV-1.0.0` (bagian dari `AST-BP-001` rev 1, `approved` 2026-11-12) |
| Dependency | `BE-AST-016` ✅, `BE-AST-009` [REG] ✅; memakai `AssetPlacementService.ApplyPlacementAsync`, `AssetProcessLockService`, `AssetActorResolver`, `AssetConditionService.OpenReportAsync`, `AssetNumberingService` |
| Klasifikasi | `HEAVY` — aggregate ber-lifecycle 6 status, 4 tahap pelaku, kunci proses, perpindahan penempatan, laporan kondisi otomatis, PDF |
| Task mode | `BACKEND`, diminta Rizki 2026-11-19 |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/AssetManagement/Movement/**` + 4 sentuhan kecil di `Register` + DI; laporan ini dan baris status roadmap/traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `c4d9f589` (branch `rizkiG`) |
| Tanggal | 19 November 2026 |
| Status | ✅ SELESAI 19 November 2026 — ketujuh kriteria terpetakan ke source dan teruji: QBE Strict `PASS`, build berhasil (DLL 11:27:30), runtime **65/65** (30 bagian A + 35 bagian B) dengan Kepala Unit dari penugasan HR |

**Backend Governance Preflight**

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AssetManagement` / `Movement` (+ `Register`: sentuhan kecil) |
| Pemilik / prefix registry | `AssetManagement / Movement / Pergerakan Aset`, prefix `Ast`, `ACTIVE` |
| Keberlakuan | `NEW CODE` (+ `TOUCHED`: `AssetRegisterEnums`, `AssetRecordService` label, `AssetNumberingService`, `AssetConditionService.OpenReportAsync`, DI) |
| QBE yang berlaku | `QBE-MOD-001`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-CODE-001`/`002`/`003` (nomor `AST-MUT` lewat deret bersama), `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001`, `QBE-DEL-001` (mutasi dibatalkan/ditolak, tidak dihapus) |
| Tidak berlaku | `QBE-ENT-*`, `QBE-CFG-*`, `QBE-NAM-*`, `QBE-DB-*` — tabel dibuat `BE-AST-016`; ConditionReportSource baru bernilai int tanpa migration |
| Arketipe transaksi | Aggregate ber-lifecycle (`Diajukan` → `DisetujuiUnitAsal` → `DisetujuiBagianAset` → `Diserahterimakan`; `Ditolak`, `Dibatalkan`) |

---

## 1. Masalah yang diperbaiki

Aset tidak bisa berpindah antar unit. Penyerahan (`BE-AST-009`) hanya menempatkan aset baru atau memindahkan aset di
dalam satu unit. `AST-DEC-011` meminta mutasi antar unit dengan persetujuan Kepala Unit asal, lalu Kepala Bagian Aset,
lalu penerimaan oleh Kepala Unit tujuan. Penempatan berpindah saat diterima, dan bukti serah terimanya dicetak.

---

## 2. Proses bisnis

| Langkah | Pelaku | Yang terjadi |
|---|---|---|
| 1 | Petugas Aset / Kepala Unit | Ajukan: unit tujuan, alasan (Kebutuhan Layanan, Koreksi Mutasi, Lainnya), catatan, 1–200 aset dari satu unit; ruangan/penanggung jawab tujuan opsional. Aset dikunci proses Mutasi; nomor `AST-MUT-######` |
| 1a | Kepala Unit asal sebagai pengaju | Tahap unit asal langsung tercatat atas namanya, status `DisetujuiUnitAsal` (`AST-DEC-048`) |
| 2 | Kepala Unit asal (bukan pengaju) | Setujui → `DisetujuiUnitAsal`, atau tolak beralasan → `Ditolak`, kunci lepas |
| 3 | Kepala Bagian Aset (bukan pengaju) | Setujui → `DisetujuiBagianAset`, atau tolak beralasan |
| 4 | Kepala Unit tujuan | Terima: ruangan milik unit tujuan + penanggung jawab aktif wajib, kondisi per aset. Penempatan berpindah (`ApplyPlacementAsync`, sumber `MUTASI`), linimasa `Dimutasi` (sub-modul pergerakan), kunci lepas. Aset yang kondisinya berbeda dari saat diajukan membuka laporan kondisi bersumber **Penerimaan Mutasi** untuk ditetapkan Bagian Aset. Atau tolak terima beralasan |
| — | Pengaju atau pemegang hak Bagian Aset | Batalkan sebelum diterima → `Dibatalkan`, kunci lepas |
| — | Siapa pun yang berhak lihat | Cetak Berita Acara Serah Terima Mutasi (PDF) setelah diterima |

Contoh dari uji: `AST-MUT-000002`, 2 monitor dari Instalasi Rawat Inap ke IGD. Siti (Kepala Unit Rawat Inap)
menyetujui, akun penyetuju Bagian Aset menyetujui, Dimas (Kepala Unit IGD) menerima di Ruang Uji U94W3 C dengan satu
monitor Rusak Ringan. Kedua monitor pindah ke IGD; laporan `AST-KND-000012` (Penerimaan Mutasi) terbuka.

---

## 3. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `Movement/Services/AssetTransferService.cs` | Baru — ajukan, `CreateFromStockTakeAsync` (untuk `inventarisasi`, tanpa transaksi sendiri), setujui/tolak unit asal, setujui/tolak Bagian Aset, terima, tolak terima, batalkan |
| `Movement/Services/AssetTransferService.Query.cs` | Baru — metadata, ringkasan, daftar berbatas unit (`awaitingMe`), rincian, `availableActions`, `awaitingParty`, PDF |
| `Movement/Services/AssetMovementDocumentService.cs` | Baru — Berita Acara Serah Terima Mutasi (QuestPDF, empat kotak persetujuan) |
| `Movement/Services/AssetMovementErrors.cs` | Baru — kode `AST-VAL-MOV-*`, `AST-MOV-FIELD/NOT-FOUND/SCOPE`, pesan, label status/alasan |
| `Movement/DTOs/AssetTransferDtos.cs` | Baru |
| `Movement/Controllers/AssetTransfersController.cs` | Baru — 10 endpoint, resource `AssetTransfer` (Read, Create, ApproveUnit, ApproveAsset, Receive, Cancel), modul `CORPORATE_ASSET_MANAGEMENT_MOVEMENT` |
| `Register/Enums/AssetRegisterEnums.cs` | + `ConditionReportSource.PenerimaanMutasi = 5` |
| `Register/Services/AssetConditionService.cs` | `OpenReportAsync` menerima sumber `PenerimaanMutasi` |
| `Register/Services/AssetNumberingService.cs` | + deret `AST_TRANSFER` / `AST-MUT` |
| `Register/Services/AssetRecordService.cs` | + label "Penerimaan Mutasi" |
| `AssetManagementServiceCollectionExtensions.cs` | + 2 service |

### 3.1 Delta terhadap `AST-MOV-1.0.0`

| No | Delta | Alasan |
|---|---|---|
| 1 | `GET /filters/metadata`, `GET /summary` | Pola layar modul (kartu dan tab) |
| 2 | Body: `approve-unit`/`approve-asset` = `{ approve, reason, rowVersion }`; `reject-receipt`/`cancel` = `{ reason, rowVersion }` (`AssetTransferReasonRequest`); `receive` = `{ targetLocationId, targetCustodianEmployeeId, items[{ assetId, conditionAtReceipt }], rowVersion }` | Kontrak hanya menamai DTO |
| 3 | `rowVersion` wajib pada setiap langkah (400 bila kosong) | `AST-VAL-MOV-016` |
| 4 | `ConditionReportSource.PenerimaanMutasi = 5` | Enum register belum punya sumber untuk penerimaan mutasi; `INT-MOV-03` hanya menyebut pengembalian pinjaman |
| 5 | Linimasa ditulis hanya saat diterima (`Dimutasi`) | `INT-MOV-05` menyebut juga "diajukan", tetapi `AssetHistoryEventType` tidak punya jenis kejadian pengajuan |
| 6 | Pembatalan: pengaju **atau** pemegang hak `AssetTransfer : ApproveAsset` | "Kepala Bagian Aset" dikenali dari hak, bukan nama peran |
| 7 | Kondisi saat diterima hanya Aktif, Rusak Ringan, Rusak Berat, Tidak Digunakan | Status Hilang hanya lewat persetujuan kehilangan |
| 8 | `CreateFromStockTakeAsync` memakai transaksi pemanggil | Dipanggil dari dalam transaksi `inventarisasi` |

**Database:** tanpa migration baru. **Keamanan:** 10/10 pasangan `[AccessAction]`/`[AccessPermission]` cocok; Kepala
Unit dari `AssetActorResolver` (penugasan manajerial aktif), tanpa hardcode peran.

---

## 4. Dokumentasi endpoint — `…/movement/transfers`

| Method | Path | Hak | Keterangan |
|---|---|---|---|
| `GET` | `/filters/metadata`, `/summary` | `AssetTransfer : Read` | Ringkasan per status + `awaitingMe` |
| `GET` | `/`, `/{id}` | `AssetTransfer : Read` | `status`, `reason`, `sourceOrganizationUnitId`, `targetOrganizationUnitId`, `awaitingMe`, `from`, `to`, `search`; tanpa hak seluruh unit hanya mutasi dari/ke unitnya atau yang ia ajukan |
| `GET` | `/{id}/document` | `AssetTransfer : Read` | PDF; hanya `Diserahterimakan` |
| `POST` | `/` | `AssetTransfer : Create` | — |
| `POST` | `/{id}/approve-unit` | `AssetTransfer : ApproveUnit` | Kepala Unit asal |
| `POST` | `/{id}/approve-asset` | `AssetTransfer : ApproveAsset` | Bukan pengaju |
| `POST` | `/{id}/receive`, `/{id}/reject-receipt` | `AssetTransfer : Receive` | Kepala Unit tujuan |
| `POST` | `/{id}/cancel` | `AssetTransfer : Cancel` | Pengaju atau pemegang `ApproveAsset` |

`availableActions`: `approveUnit`, `rejectUnit`, `approveAsset`, `rejectAsset`, `receive`, `rejectReceipt`, `cancel`, `document`.

---

## 5. Verifikasi

### 5.1 Statis dan build

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| QBE `-Mode Strict` (WorkingTree) | `Files evaluated: 11`, `VIOLATION: 0`, `REVIEW: 0`, `Final result: PASS` | `PASS` |
| Atribut akses | 10/10 | `PASS` |
| Baris komentar baru | 0 | `PASS` |
| Build | DLL 2026-11-19 11:27:30 (dibangun saat `migrations add`) memuat `AssetTransfersController`, `AssetMovementDocumentService` | `PASS` |
| Registrasi hak | `AssetTransfer` 6 aksi di `role-access/resources/structured` | `PASS` |

### 5.2 Data uji yang disiapkan

Di DB dev belum ada satu pun penugasan manajerial. Atas izin Rizki ("biarkan permanen"), lewat API HR
(`POST …/workforce-profiles/{id}/organization-assignments`, `Secondary`, bukan utama) dibuat:
Siti Nurhaliza (PENERIMA) = Kepala Unit **Instalasi Rawat Inap** (`96af6385…`), Dimas Kurniawan = Kepala Unit
**IGD** (`4ab22afc…`). Kebijakan Akses Role jabatan Siti dan akun penyetuju diberi hak `AssetTransfer` sementara lalu
dipulihkan identik.

### 5.3 Runtime (HTTPS 7184) — `test-with-agy/asset-management/be-ast-017-20261119/`

| Kelompok | Langkah | Hasil |
|---|---|---|
| Bagian A (`ast017-a.mjs`) | Registrasi 6 aksi; metadata/ringkasan; validasi `008` (×3), `007`, field unit tujuan, **`AT-MOV-23`** (`005`), **`AT-MOV-20`** (`002`, Rusak Berat), `001` (belum ditempatkan), **`AT-MOV-21`** (`003`; terkunci pemanfaatan dan terkunci mutasi, karena peminjaman belum ada sampai `BE-AST-018`), `013`; ajukan → `AST-MUT-000001` `Diajukan`, tombol pengaju hanya `cancel`, kunci Mutasi; **`AT-MOV-25`** (`010`, bukan Kepala Unit asal → 403), `015` (lompat tahap, terima dini, batal ulang), **`AT-MOV-31`** (`016`, versi basi → 409; varian dua penyetuju bersamaan tidak dijalankan), versi wajib, `014`, PDF dini 409; LOKET 403; `awaitingMe`, cari nomor inventaris; batal → kunci lepas, penempatan tetap | **30/30** |
| Bagian B (`ast017-b.mjs`) | Siti melihat tombol unit asal dan antrean `awaitingMe`; setuju unit asal; **`AT-MOV-26`** (`011`, pengaju menyetujui tahap Bagian Aset → 403); tombol Bagian Aset; setuju Bagian Aset; **`AT-MOV-27`** (`012`, Siti bukan Kepala Unit tujuan → 403); **`AT-MOV-28`** (`013`, tanpa ruangan dan ruangan unit lain → 400); kondisi Hilang dan aset di luar mutasi ditolak; **`AT-MOV-01`** terima; penempatan pindah + kunci lepas; linimasa `Dimutasi` (pergerakan, "Rawat Inap / U9KOL A → IGD / U94W3 C"); **kondisi berbeda → `AST-KND-000012` Penerimaan Mutasi**, aset sama kondisi tanpa laporan; PDF 44.884 byte (diperiksa visual); **`AT-MOV-30`** batal sesudah diterima 409; **`AT-MOV-22`** (`004`) dua unit asal; **`AT-MOV-02`** pengajuan Kepala Unit asal otomatis; tolak tahap Bagian Aset, tolak tahap unit asal, **`AT-MOV-29`** tolak terima; **`AT-MOV-03`** koreksi mutasi (rujukan `AST-MUT-000002` tersimpan; mutasi koreksi lalu dibatalkan, sehingga baris linimasa kedua tidak terbentuk); batal oleh Bagian Aset; batal bukan pengaju 403; ringkasan; pemulihan policy | **35/35** |

**Rekap: 65/65.**

### 5.4 Temuan

| Temuan | Tindak |
|---|---|
| Endpoint HR `organization-assignments` (milik HR) untuk Siti menjawab 500 karena pelanggaran unik `IX_AspNetUserOrganization_UserId_DepartmentId_PositionId_Effec…`, padahal penugasan sudah tersimpan (tidak dibatalkan bersih) | Bukan modul Aset; dicatat untuk pemilik HR. Penugasan Siti tetap valid untuk `AssetActorResolver` |
| Dimas (SuperAdmin) memegang `ApproveAsset`, sehingga melihat tombol Batalkan pada mutasi orang lain | Konfigurasi peran, bukan cacat |
| `AST-KND-000012` tetap terbuka (Dilaporkan) | Data uji; menunggu penetapan Bagian Aset |

### 5.5 Data uji di `QuilvianNewDevRizki`

`AST-MUT-000001..000008`: `000002` dan `000008` (uji layar) Diserahterimakan (3 aset kini di IGD), `000003..000005`
Ditolak, `000001`, `000006`, `000007` Dibatalkan. Penugasan Kepala Unit Siti dan Dimas permanen.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Source | Bukti | Status |
|---|---|---|---|
| 1. Matriks state §1 lengkap, termasuk tahap 1 otomatis bila pengaju Kepala Unit asal | `AssetTransferService` | `AT-MOV-02`, `25`, `26`, `01`, `29`, tolak tiap tahap, batal, transisi tidak sah 409 | ✅ |
| 2. Penempatan berpindah hanya saat diterima Kepala Unit tujuan | `ReceiveAsync` → `ApplyPlacementAsync` | Penempatan tetap saat ditolak/dibatalkan; pindah saat diterima | ✅ |
| 3. Kondisi diterima berbeda membuka laporan kondisi | `OpenReportAsync` sumber `PenerimaanMutasi` | `AST-KND-000012` | ✅ |
| 4. Kunci proses dipasang/dilepas | `AddItems`, `ReleaseLocksAsync`, `ReceiveAsync` | Kunci Mutasi 2, lepas pada batal/tolak/terima | ✅ |
| 5. `AST-VAL-MOV-001..016` | `AssetTransferService` | 001–005, 007, 008, 010–016 teruji; `006` (opname) memakai `AssetStockTakeGate` yang masih kosong sampai `BE-AST-021` | ✅ (`006` menyusul `inventarisasi`) |
| 6. `CreateFromStockTake` tersedia untuk `inventarisasi` | `CreateFromStockTakeAsync` | Ada di source; pemanggilnya `BE-AST-023` | ✅ (diuji pemanggilnya) |
| 7. `AvailableActions` dihitung backend | `DescribeAsync` | Tombol per aktor sesuai tahap (Siti, penyetuju, Dimas, pengaju) | ✅ |

| DoD | Status |
|---|---|
| `V-BE` QBE, build, registrasi hak, runtime | Terpenuhi |
| Laporan, roadmap, traceability | Ditulis |
| Commit | Bersama `FE-AST-012` |
