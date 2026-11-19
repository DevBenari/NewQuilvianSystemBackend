# Laporan Perubahan Backend — `BE-AST-018`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-AST-018` |
| Judul | Peminjaman dan pengembalian |
| Gelombang | `MVP-2` — sub-modul `pergerakan` |
| Roadmap | `pergerakan/roadmap/backend-roadmap.md` — kartu `BE-AST-018` |
| Trace | `CAP-05.4`, `05.5`; `AST-DEC-012`; `AST-DES-021`, `022`; `AST-RCG-G07`; API 2.2; state §2–3; validasi `AST-VAL-MOV-001..007`, `010`, `011`, `014..016`, `020..023`; `INT-MOV-03`; `AT-MOV-04..06`, `32..35` |
| Contract version | `AST-MOV-1.0.0` (bagian dari `AST-BP-001` rev 1, `approved` 2026-11-12) |
| Dependency | `BE-AST-016` ✅, `BE-AST-010` [REG] ✅; memakai `AssetConditionService.OpenReportAsync`, `AssetProcessLockService`, `AssetActorResolver`, `AssetHistoryService`, `AssetNumberingService`, `AssetStockTakeGate` |
| Klasifikasi | `HEAVY` — aggregate ber-lifecycle 7 status + perpanjangan 3 status, 2 pihak Kepala Unit, kunci proses, keadaan turunan Terlambat, laporan kondisi otomatis, PDF |
| Task mode | `BACKEND`, diminta Rizki 2026-11-19 ("lanjut BE-AST-018") |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/AssetManagement/Movement/**` + 1 baris deret nomor di `Register` + DI; laporan ini dan baris status roadmap/traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `defec7ce` (branch `rizkiG`) |
| Tanggal | 19 November 2026 |
| Status | ✅ SELESAI 19 November 2026 — kelima kriteria terpetakan ke source dan teruji: QBE Strict `PASS`, build Rizki berhasil (DLL 14:28:05), runtime **73/73** |

**Backend Governance Preflight**

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AssetManagement` / `Movement` (+ `Register`: satu baris deret nomor) |
| Pemilik / prefix registry | `AssetManagement / Movement / Pergerakan Aset`, prefix `Ast`, `ACTIVE` |
| Keberlakuan | `NEW CODE` (+ `TOUCHED`: `AssetNumberingService`, `AssetMovementErrors`, `AssetMovementDocumentService`, DI) |
| QBE yang berlaku | `QBE-MOD-001`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-CODE-001`/`002`/`003` (nomor `AST-PJM` lewat deret bersama), `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001`, `QBE-DEL-001` (pinjaman ditolak/dibatalkan, tidak dihapus) |
| Tidak berlaku | `QBE-ENT-*`, `QBE-CFG-*`, `QBE-NAM-*`, `QBE-DB-*` — tabel dibuat `BE-AST-016`; tanpa migration |
| Arketipe transaksi | Aggregate ber-lifecycle (`Diajukan` → `Disetujui` → `Dipinjam` → `MenungguKonfirmasiKembali` → `Dikembalikan`; `Ditolak`, `Dibatalkan`) dengan sub-proses perpanjangan ter-scope induk |

---

## 1. Masalah yang diperbaiki

Unit yang butuh alat sementara hanya punya jalan mutasi, yang memindahkan kepemilikan unit. `AST-DEC-012` meminta
peminjaman antar unit dengan rencana tanggal kembali: aset tetap tercatat milik unit pemilik, keterlambatan terlihat,
tanggal kembali bisa diperpanjang dengan persetujuan pemilik, dan kondisi saat kembali diperiksa pemilik.

---

## 2. Proses bisnis

| Langkah | Pelaku | Yang terjadi |
|---|---|---|
| 1 | Petugas Aset atau Kepala Unit peminjam | Ajukan: unit peminjam, tujuan, tanggal mulai (paling awal kemarin) dan rencana tanggal kembali, 1–200 aset dari satu unit pemilik. Aset dikunci proses Peminjaman; nomor `AST-PJM-######` |
| 1a | Kepala Unit pemilik sebagai pengaju | Langsung `Disetujui` atas namanya (`AST-DES-021`) |
| 2 | Kepala Unit pemilik (bukan pengaju) | Setujui → `Disetujui`, atau tolak beralasan → `Ditolak`, kunci lepas |
| 3 | Kepala Unit peminjam | Ambil → `Dipinjam`; linimasa `Dipinjam` (pemilik → peminjam). Penempatan aset **tidak** berubah |
| 3a | Kepala Unit peminjam | Minta perpanjangan (tanggal baru > tanggal kembali berlaku, alasan); hanya satu yang terbuka |
| 3b | Kepala Unit pemilik | Setujui → tanggal kembali diganti, tanggal semula tetap tersimpan; atau tolak beralasan |
| 4 | Kepala Unit peminjam | Kembalikan → `MenungguKonfirmasiKembali` |
| 5 | Kepala Unit pemilik | Konfirmasi kembali dengan kondisi tiap aset → `Dikembalikan`, kunci lepas, linimasa `Dikembalikan`. Aset yang kondisinya berbeda dari kondisi tercatat membuka laporan kondisi bersumber **Pengembalian Pinjaman**. Perpanjangan yang masih terbuka ditolak otomatis |
| — | Pengaju atau Kepala Unit pemilik | Batalkan selama `Diajukan`/`Disetujui` |
| — | Sistem saat dibaca | `isOverdue` = `Dipinjam`/`MenungguKonfirmasiKembali` dan rencana tanggal kembali < hari ini WIB; `overdueDays` |
| — | Siapa pun yang berhak lihat | Berita Acara Peminjaman (dan Pengembalian, bila sudah dikonfirmasi) sejak diambil |

Contoh dari uji: `AST-PJM-000001`, dua monitor milik Instalasi Rawat Inap dipinjam IGD 18–18 Nov. Setelah diambil Dimas
pada 19 Nov pinjaman tampil Terlambat 1 hari. Dimas meminta perpanjangan sampai 22 Nov, Siti menyetujui, tanda Terlambat
hilang. Monitor kembali; Siti mengisi satu Aktif dan satu Rusak Ringan, lalu sistem membuka `AST-KND-000013`.

---

## 3. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `Movement/Services/AssetLoanService.cs` | Baru — ajukan, putuskan, ambil, kembalikan, konfirmasi kembali, minta/putuskan perpanjangan, batalkan |
| `Movement/Services/AssetLoanService.Query.cs` | Baru — metadata, ringkasan, daftar berbatas unit (`isOverdue`, `awaitingMe`, status ganda), rincian + perpanjangan, aset yang dapat dipinjam, `availableActions`, `awaitingParty`, PDF |
| `Movement/DTOs/AssetLoanDtos.cs` | Baru |
| `Movement/Controllers/AssetLoansController.cs` | Baru — 14 endpoint, resource `AssetLoan` (Read, Create, Approve, Receive, Return, ConfirmReturn, Extend, Cancel) |
| `Movement/Services/AssetMovementDocumentService.cs` | + `RenderLoanReport` (lima kotak persetujuan) |
| `Movement/Services/AssetMovementErrors.cs` | + kode `020..023`, pesan pinjaman, label `LoanStatus`, `LoanExtensionStatus` |
| `Register/Services/AssetNumberingService.cs` | + deret `AST_LOAN` / `AST-PJM` |
| `AssetManagementServiceCollectionExtensions.cs` | + `AssetLoanService` |

### 3.1 Delta terhadap `AST-MOV-1.0.0`

| No | Delta | Alasan |
|---|---|---|
| 1 | `GET /filters/metadata`, `GET /summary` (termasuk `overdue`, `awaitingMe`) | Pola layar modul, sama dengan mutasi |
| 2 | `GET /loanable-assets` (`AssetLoan : Create`): aset Terdaftar, Ditempatkan, tidak terkunci, bukan Hilang/Rusak Berat, lintas unit, saringan `ownerOrganizationUnitId` | Daftar aset register dibatasi unit pengguna, sehingga Kepala Unit peminjam tidak bisa memilih aset milik unit lain |
| 3 | Body `pickup`/`return` = `{ rowVersion }` wajib; `cancel` = `{ reason, rowVersion }` (`AssetLoanReasonRequest`, bukan `AssetLoanDecisionRequest`); `decide` dan keputusan perpanjangan = `{ approve, reason, rowVersion }` dengan `rowVersion` pinjaman | `AST-VAL-MOV-016` berlaku di semua langkah; bentuk sama dengan mutasi |
| 4 | `status` boleh diulang (`status=3&status=4`), `sortBy=plannedReturnDate` | Layar Pengembalian Aset menyaring dua status |
| 5 | Respons: `overdueDays`, `openExtension`, `extensions[]`, `originalPlannedReturnDate` | Tanda Terlambat dan keputusan perpanjangan di layar |
| 6 | Kode `004` dipakai juga untuk "satu pinjaman hanya untuk aset dari satu unit pemilik" | Pinjaman punya satu unit pemilik; matriks hanya menyebut mutasi |
| 7 | Tujuan peminjaman wajib (≤ 1000) → `AST-MOV-FIELD` | Kolom `Purpose` wajib di data dict; tidak ada kode validasi khusus |
| 8 | Konfirmasi kembali menolak otomatis perpanjangan yang masih terbuka | Perpanjangan tidak bermakna setelah aset kembali |
| 9 | Laporan kondisi dibuka bila kondisi kembali ≠ kondisi aset yang tercatat (bukan `ConditionAtPickup`) | Laporan kondisi mencatat kondisi tercatat sebagai kondisi sebelumnya |
| 10 | Kondisi kembali hanya Aktif, Rusak Ringan, Rusak Berat, Tidak Digunakan | Status Hilang hanya lewat persetujuan kehilangan, sama dengan penerimaan mutasi |
| 11 | Terlambat berlaku juga pada `MenungguKonfirmasiKembali` | Matriks state §2; `AST-DES-022` hanya menyebut `Dipinjam` |

**Database:** tanpa migration. **Keamanan:** 14/14 pasangan `[AccessAction]`/`[AccessPermission]` cocok; Kepala Unit
pemilik/peminjam dari `AssetActorResolver` (penugasan manajerial aktif), tanpa hardcode peran.

---

## 4. Dokumentasi endpoint — `…/movement/loans`

| Method | Path | Hak | Keterangan |
|---|---|---|---|
| `GET` | `/filters/metadata`, `/summary` | `AssetLoan : Read` | — |
| `GET` | `/`, `/{id}` | `AssetLoan : Read` | `search`, `status[]`, `ownerOrganizationUnitId`, `borrowerOrganizationUnitId`, `isOverdue`, `awaitingMe`, `from`, `to`, `sortBy`; tanpa hak seluruh unit hanya pinjaman unit pemilik/peminjamnya atau yang ia ajukan |
| `GET` | `/loanable-assets` | `AssetLoan : Create` | Delta no. 2 |
| `GET` | `/{id}/document` | `AssetLoan : Read` | PDF; sejak `Dipinjam` |
| `POST` | `/` | `AssetLoan : Create` | — |
| `POST` | `/{id}/decide` | `AssetLoan : Approve` | Kepala Unit pemilik, bukan pengaju |
| `POST` | `/{id}/pickup` | `AssetLoan : Receive` | Kepala Unit peminjam |
| `POST` | `/{id}/return` | `AssetLoan : Return` | Kepala Unit peminjam |
| `POST` | `/{id}/confirm-return` | `AssetLoan : ConfirmReturn` | Kepala Unit pemilik; `items[{ assetId, returnCondition }]` lengkap |
| `POST` | `/{id}/extensions` | `AssetLoan : Extend` | Kepala Unit peminjam; `{ requestedReturnDate, reason, rowVersion }` |
| `POST` | `/{id}/extensions/{extensionId}/decide` | `AssetLoan : Approve` | Kepala Unit pemilik, bukan peminta |
| `POST` | `/{id}/cancel` | `AssetLoan : Cancel` | Pengaju atau Kepala Unit pemilik |

`availableActions`: `approve`, `reject`, `pickup`, `return`, `extend`, `decideExtension`, `confirmReturn`, `cancel`, `document`.
`isAwaitingMe` = ada `approve`, `pickup`, `confirmReturn`, atau `decideExtension`.

---

## 5. Verifikasi

### 5.1 Statis dan build

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| QBE `-Mode Strict` (WorkingTree) | `Files evaluated: 8`, `VIOLATION: 0`, `REVIEW: 0`, `Final result: PASS` | `PASS` |
| Atribut akses | 14/14 | `PASS` |
| Baris komentar baru | 0 | `PASS` |
| Build | Rizki, DLL 2026-11-19 14:28:05 memuat `AssetLoanService` dan rute `loanable-assets` | `PASS` |
| Registrasi hak | `AssetLoan` 8 aksi di `role-access/resources/structured`; Swagger 14 operasi | `PASS` |

### 5.2 Runtime (HTTPS 7184) — `test-with-agy/asset-management/be-ast-018-20261119/ast018.mjs`

Pelaku: SuperAdmin uji (Petugas Aset, pengaju), Siti (Kepala Unit Instalasi Rawat Inap, pemilik), Dimas (Kepala Unit
IGD, peminjam), akun penyetuju aset (tanpa hak `Create`). Kebijakan Akses Role jabatan Siti dan akun penyetuju diberi
hak `AssetLoan` sementara lalu dipulihkan identik (dicek ulang: 22 dan 6 butir).

| Kelompok | Langkah | Hasil |
|---|---|---|
| Registrasi | 8 aksi, Swagger 14 operasi, metadata | 3/3 |
| Aset yang dapat dipinjam | Dimas melihat 42 aset Rawat Inap (lintas unit, tanpa Hilang/Rusak Berat); akun tanpa `Create` → 403 | 3/3 |
| Validasi ajukan | **`AT-MOV-32`** (`020` kembali sebelum mulai), `020` mulai > 1 hari lampau dan tanggal kosong, `005`, `007`, tujuan wajib, unit peminjam tidak ada, `004` dua unit pemilik, `003` aset sedang dipinjam | 9/9 |
| Ajukan → setujui | `AST-PJM-000001` `Diajukan`, kunci Peminjaman (3); Siti melihat `approve`/`reject` dan antrean `awaitingMe`; **`AT-MOV-35`** (SuperAdmin tanpa penugasan Kepala Unit pemilik → 403 `010`); Dimas (Kepala Unit peminjam) → 403 `010`; `rowVersion` wajib (400) dan basi (409 `016`); Siti setuju | 9/9 |
| Ambil | Siti → 403 `021`; Dimas melihat `pickup`, mengambil → `Dipinjam`, tombol `return`/`extend`/`document`; **`AT-MOV-04`** `isOverdue=true`, `overdueDays=1` (tanggal kembali 18 Nov); saringan `isOverdue` true/false; ringkasan `overdue` +1; **AC5** aset tetap di Instalasi Rawat Inap / Ruang Uji U9KOL A; linimasa `Dipinjam` (Rawat Inap → IGD) | 8/8 |
| Batal saat dipinjam | **`AT-MOV-33`** → 409 `015` | 1/1 |
| Perpanjangan | `021`, `020` (tanggal tidak maju), `014`; minta → `openExtension`, tombol `extend` hilang; **`AT-MOV-34`** kedua → 409 `023`; Siti melihat `decideExtension`; Dimas memutuskan → 403 `010`; **`AT-MOV-05`** disetujui → kembali 22 Nov, semula 18 Nov, `isOverdue=false`; putuskan ulang → 409 `015`; tolak tanpa alasan 400 `014`; tolak beralasan | 11/11 |
| Kembali | Siti → 403 `021`; Dimas kembalikan → `MenungguKonfirmasiKembali` | 2/2 |
| Konfirmasi kembali | Dimas → 403 `010`; kondisi kurang satu → 400 `022`; Hilang → 400; aset di luar pinjaman → 400; **AC3** konfirmasi (Aktif + Rusak Ringan), perpanjangan terbuka tertutup otomatis; kunci lepas, pemilik tetap; linimasa `Dikembalikan`; **`AT-MOV-06`** `AST-KND-000013` sumber Pengembalian Pinjaman hanya untuk aset yang berubah; PDF 46.862 byte (diperiksa visual, `ba-pinjaman.png`); langkah sesudah `Dikembalikan` → 409 | 9/9 |
| Jalur lain | **`AST-DES-021`** Siti mengajukan → langsung `Disetujui`, `isApprovalByRequester`; PDF sebelum diambil 409; batal tanpa alasan 400; batal oleh pengaju; tolak tanpa alasan 400; tolak pemilik (kunci lepas); batal oleh Dimas (bukan pengaju, bukan pemilik) 403; batal oleh Kepala Unit pemilik | 8/8 |
| Daftar | Status ganda `status=5&status=7`, cari nomor inventaris, cakupan daftar = rincian, rincian tidak ada 404, ringkasan akhir | 5/5 |
| Pemulihan | Policy Siti dan penyetuju dipulihkan identik | 3/3 |

**Rekap: 73/73** (`ast018.json`).

### 5.3 Temuan

| Temuan | Tindak |
|---|---|
| Akun penyetuju aset ternyata melihat semua pinjaman (punya hak lihat seluruh unit), sehingga cakupan negatif "pinjaman di luar unit → 403" tidak teruji dengan akun nyata | Logika sama dengan mutasi (`Scoped`/`CanSee`); cek yang dijalankan: daftar dan rincian konsisten. Uji negatif menunggu akun berunit tanpa hak seluruh unit |
| Pesan `015` memakai templat bersama: "Pengajuan ini sudah dipinjam; tindakan tidak dapat dilakukan." | Sesuai matriks validasi; bila kurang enak dibaca, ubah templat di matriks dahulu |
| `AST-KND-000013` tetap terbuka (Dilaporkan) | Data uji; menunggu penetapan Bagian Aset |

### 5.4 Data uji di `QuilvianNewDevRizki`

`AST-PJM-000001` Dikembalikan (`AST-ALKMED-000007` Aktif, `000008` → laporan `AST-KND-000013`), `000002` dan `000004`
Dibatalkan, `000003` Ditolak. Semua kunci aset sudah lepas.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Source | Bukti | Status |
|---|---|---|---|
| 1. Matriks state §2–3 | `AssetLoanService` | Seluruh transisi §2 (termasuk `AST-DES-021`) dan §3; transisi tidak sah 409 (`AT-MOV-33`, sesudah `Dikembalikan`, perpanjangan sudah diputuskan) | ✅ |
| 2. `isOverdue` turunan pada respons | `DescribeAsync`, `Overdue` | `AT-MOV-04` (true, 1 hari), `AT-MOV-05` (false sesudah perpanjangan), saringan dan ringkasan | ✅ |
| 3. Konfirmasi kembali dengan kondisi per butir; kondisi berubah → laporan `PengembalianPinjaman` | `ConfirmReturnAsync` | `022`, `AT-MOV-06` (`AST-KND-000013`) | ✅ |
| 4. Satu perpanjangan terbuka | `RequestExtensionAsync` + index unik parsial | `AT-MOV-34` (409 `023`) | ✅ |
| 5. Aset tetap milik unit pemilik | Tanpa `ApplyPlacementAsync` | Unit dan ruangan tetap selama dipinjam dan sesudah kembali | ✅ |

| DoD | Status |
|---|---|
| `V-BE` QBE, build, registrasi hak, runtime | Terpenuhi |
| Laporan, roadmap, traceability | Ditulis |
| Commit | Bersama `FE-AST-013` sesuai pola modul, atau terpisah bila Rizki minta |

Task berikutnya: `FE-AST-013` (layar Peminjaman Sementara, Pengembalian Aset, Detail Pinjaman), lalu `BE-AST-019`.
