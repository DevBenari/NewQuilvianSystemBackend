# Laporan Perubahan Backend — `BE-AST-023`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-AST-023` |
| Judul | Verifikasi, berita acara, tindak lanjut |
| Gelombang | `MVP-3` — sub-modul `inventarisasi` |
| Roadmap | `inventarisasi/roadmap/backend-roadmap.md` — kartu `BE-AST-023` |
| Trace | `CAP-07.4..07.6`, tindak lanjut selisih; `AST-DEC-019`, `046`; `AST-DES-007`, `030`; `contracts/api-contract.md` 2.1 (catatan, ajukan, verifikasi, dokumen), 2.2; `contracts/state-transition-matrix.md` §3; `contracts/validation-matrix.md` `AST-VAL-INV-020..023`, `030`, `031` |
| Contract version | `AST-INV-1.0.0` (bagian dari `AST-BP-001` rev 1, `approved` 2026-11-12) |
| Dependency | `BE-AST-022` ✅; memakai `AssetTransferService` [MOV], `AssetPlacementService`, `AssetConditionService`, `AssetRecordService` [REG] |
| Klasifikasi | `STANDARD` — 3 service baru, 1 partial service, 1 controller baru, 4 endpoint baru pada controller sesi, 1 metode baru di service register; tanpa migration |
| Task mode | `BACKEND`, diminta Rizki 2026-11-23 (berpasangan dengan `FE-AST-017`) |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/AssetManagement/PhysicalInventory/**`, `Register/Services/{AssetPlacementService,AssetAttachmentAccessService}.cs`, `Register/DTOs/AssetPlacementDtos.cs`, `AssetManagementServiceCollectionExtensions.cs` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `fda1fe1f` (branch `rizkiG`) |
| Tanggal | 23 November 2026 |
| Status | ✅ SELESAI 23 November 2026 — kelima kriteria terpetakan ke source dan dibuktikan runtime **40/40**; PDF berita acara dibaca langsung; build Rizki DLL 2026-11-23 15:26:45 |

**Backend Governance Preflight**

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AssetManagement` / `PhysicalInventory` |
| Pemilik / prefix registry | `AssetManagement / PhysicalInventory / Inventarisasi Fisik`, prefix `Ast`, `ACTIVE` |
| Keberlakuan | `NEW CODE` (+ `TOUCHED`: `AssetPlacementService`, `AssetAttachmentAccessService`, pendaftaran DI) |
| QBE yang berlaku | `QBE-API-*`, `QBE-PERM-*` (aksi `Submit`, `Verify`, `Process`, `Dismiss` selaras antara `[AccessAction]` dan `[AccessPermission]`), `QBE-SVC-*`, `QBE-MOD-001`/`002` |
| Tidak berlaku | `QBE-DB-*` — tanpa migration |

---

## 1. Masalah yang diperbaiki

Sesudah pemindaian ditutup, hasil pencocokan berhenti di status `Pencocokan`. Ketua tim belum bisa memberi catatan
selisih maupun mengajukan verifikasi, Kepala Bagian Aset belum bisa memverifikasi, berita acara belum ada, dan selisih
tidak punya jalur tindak lanjut ke dokumen yang benar-benar mengubah aset (mutasi, penyerahan, laporan kondisi, draft
aset).

---

## 2. Perubahan yang dikerjakan

| Berkas | Perubahan |
| --- | --- |
| `PhysicalInventory/Services/StockTakeSessionService.Verification.cs` | Baru — catatan tim per hasil (`PUT results/{id}`, ≤ 1.000 karakter, hanya `Pencocokan`); ajukan (setiap selisih wajib bercatatan, `AST-VAL-INV-021` menyebut jumlah yang kurang); verifikasi setuju/kembalikan (verifikator bukan anggota tim dan berakses unit, `AST-VAL-INV-022`; kembalikan wajib alasan); verifikasi membuat usulan dan linimasa `Diopname` untuk setiap aset diharapkan dalam **satu transaksi**; unduh PDF hanya saat `Diverifikasi` |
| `PhysicalInventory/Services/StockTakeFollowUpService.cs` | Baru — usulan per selisih saat verifikasi (`Tidak Ditemukan` → Usulan Hilang; `Beda Lokasi` → Koreksi Lokasi; `Beda Kondisi` atau beda lokasi + kondisi → Perubahan Kondisi; `Tidak Terdaftar` → Pendaftaran Temuan); daftar/rincian dengan rencana tindakan; proses dengan pola klaim → buat dokumen tujuan → isi target, klaim dilepas bila pembuatan gagal; abaikan dengan alasan |
| `PhysicalInventory/Services/StockTakeDocumentService.cs` | Baru — PDF berita acara (QuestPDF): kop entitas, identitas sesi, tim, ringkasan enam kategori, tabel selisih beserta catatan tim, blok pengaju dan verifikator, catatan bahwa opname tidak mengubah data aset |
| `PhysicalInventory/Controllers/AssetStockTakeFollowUpsController.cs` | Baru — resource `AssetStockTakeFollowUp` (`Read`, `Process`, `Dismiss`), 4 operasi |
| `PhysicalInventory/Controllers/AssetStockTakesController.cs` | + `PUT /{id}/results/{resultId}` (`Close`), `POST /{id}/submit` (`Submit`), `POST /{id}/verify` (`Verify`), `GET /{id}/document` (`Read`); resource `AssetStockTake` kini 8 aksi |
| `StockTakeSessionService.cs`, `.Query.cs`, `StockTakeMatchingService.cs` | Aksi `editNote`, `submit`, `verify`, `document`; rincian memuat pengaju, verifikator, alasan keputusan; hasil memuat ringkasan usulan per baris |
| `PhysicalInventory/Services/AssetStockTakeErrors.cs`, `DTOs/AssetStockTakeDtos.cs` | + kode `AST-VAL-INV-021`, `022`, `030`, `031`, `AST-INV-FOLLOWUP-NOT-FOUND`, `AST-INV-RESULT-NOT-FOUND`; permintaan catatan, verifikasi, proses (+ `draft`), respons usulan |
| `Register/Services/AssetPlacementService.cs`, `Register/DTOs/AssetPlacementDtos.cs` | + `CreateStockTakeCorrectionAsync` — penyerahan bertujuan `KoreksiOpname` untuk koreksi ruangan di unit yang sama |
| `Register/Services/AssetAttachmentAccessService.cs` | Lampiran `OwnerType` `StockTakeScan` boleh diunduh oleh pemegang hak baca sesi (foto temuan di layar selisih) |
| `AssetManagementServiceCollectionExtensions.cs` | + `StockTakeFollowUpService`, `StockTakeDocumentService` |

Tanpa baris komentar. `AssetTransferService.CreateFromStockTakeAsync` (alasan `KoreksiOpname`) dan sumber laporan
kondisi `TindakLanjutOpname` sudah tersedia dari sub-modul pergerakan dan register; task ini hanya memanggilnya.

### 2.1 Pemetaan usulan → dokumen tujuan (`AST-DES-007`)

| Usulan | Dokumen tujuan | Nomor |
|---|---|---|
| Koreksi Lokasi, ruangan ditemukan di unit **lain** | Mutasi `Diajukan`, alasan `Koreksi Opname`, unit tujuan = unit tempat aset ditemukan | `AST-MUT-*` |
| Koreksi Lokasi, ruangan ditemukan di unit **yang sama** | Penyerahan bertujuan `Koreksi Opname` ke ruangan ditemukan | `AST-PNY-*` |
| Perubahan Kondisi | Laporan kondisi sumber `Tindak Lanjut Opname` dengan kondisi teramati | `AST-KND-*` |
| Usulan Hilang | Laporan kondisi `Hilang` (berita acara kehilangan tetap jalur laporan kondisi) | `AST-KND-*` |
| Pendaftaran Temuan | Draft aset; data draft diisi pemroses lewat `draft` pada permintaan proses; keterangan draft memuat nomor sesi | Draft aset (status `Draft`) |

Dokumen tujuan mengikuti alur persetujuannya sendiri; verifikasi dan proses usulan tidak pernah mengubah data aset
langsung (`AST-DEC-019`).

### 2.2 Delta terhadap kontrak

| Delta | Alasan |
| --- | --- |
| + `GET /follow-ups/{id}` (hak `Read`) | Modal proses perlu rencana tindakan dan data usulan terbaru sebelum pemroses menekan Proses |
| `StockTakeFollowUpProcessRequest` + `draft` (nama, kelompok, kepemilikan, cara dan tanggal perolehan, nilai) | Temuan tidak membawa data register; pemroses yang melengkapi. Tanpa `draft` → 400 `AST-INV-FIELD` |
| Foto temuan tetap sebagai lampiran pindaian, tidak disalin ke draft aset | `AstAsset` tidak punya kolom foto; foto tetap dapat diunduh lewat `photoAttachmentId` usulan |
| `submit` dan `verify` menerima `rowVersion` | Konkurensi, sama dengan `start`/`close-counting` |
| `GET /{id}/document` hanya saat `Diverifikasi` (409 sebelumnya) | Berita acara adalah dokumen hasil verifikasi (`AST-DEC-046`) |
| + `AST-INV-FOLLOWUP-NOT-FOUND`, `AST-INV-RESULT-NOT-FOUND` (404) | Kode spesifik untuk id usulan/hasil yang tidak ada |
| Hasil pencocokan memuat `followUps[]` per baris | Layar selisih menampilkan status usulan dan nomor dokumen tujuan tanpa permintaan tambahan |
| Penyerahan koreksi ruangan lewat metode baru `AssetPlacementService.CreateStockTakeCorrectionAsync` | Kontrak menulis "penyerahan"; jalur penyerahan biasa menuntut unit belum ditahan dan alur penerimaan pengguna, tidak cocok untuk koreksi data |
| Catatan dan ajukan: ketua tim **atau** pemegang `Create` berakses unit | Aturan sama dengan tutup/buka pemindaian (`BE-AST-022`) |

---

## 3. Verifikasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| QBE `-Mode Strict` (ExplicitFiles, 14 berkas, diulang 2026-11-23 sesudah build) | `VIOLATION: 0`, `REVIEW: 0`, `Final result: PASS` | `PASS` |
| Build | DLL 2026-11-23 15:26:45 (Rizki); backend dijalankan `dotnet run --no-build` | `PASS` |
| Runtime | **40/40** (putaran 2) — `QuilvianSystemFrontendDev/test-with-agy/asset-management/be-ast-023-20261123/ast023.mjs`, hasil `ast023.json` | `PASS` |
| Berita acara | `berita-acara.pdf` (`AST-OPN-000018`) dibaca langsung — 3.2 | `PASS` |

Tim uji: Siti Nurhaliza (bukan SuperAdmin) ketua dengan hak sementara `Read`/`Start`/`Scan`/`Close`/`Submit`, Dimas
Kurniawan anggota; akun penyetuju aset (tanpa pegawai, bukan anggota tim) sebagai Kepala Bagian Aset dengan hak sementara
`Verify` + usulan `Read`/`Dismiss` tanpa `Process`; SuperAdmin sebagai Petugas Aset pemroses. Policy dibaca segar,
diubah sementara, dan dipulihkan (`RESTORE-POLICY-TERBUKTI` lulus).

**Putaran 1 — 39/40.** `AT-INV-36` gagal karena pemilihan data skrip: aset ke-4 (`AST-MBL-000008`) sudah memegang kunci
**pemanfaatan** dari data uji lama, jadi proses usulan pertamanya pun benar ditolak 409 `AST-VAL-INV-031`. Kunci
pemanfaatan memang tidak menahan mulai opname (hanya penyerahan/mutasi/pinjaman berjalan), sehingga aset itu sah masuk
sesi. Skrip diperbaiki untuk hanya memilih aset tanpa kunci proses; bukti putaran 1 disimpan `ast023-putaran1.json`.

### 3.1 Skenario runtime (putaran 2, sesi `AST-OPN-000018`)

| Skenario | Hasil |
| --- | --- |
| Hak `AssetStockTake` 8 aksi; `AssetStockTakeFollowUp` 3 aksi | Lulus |
| Siapkan: 22 aset diharapkan; pindai 4 aset + 1 aset IGD + 1 temuan berfoto; tutup | Sesuai 1, Tidak Ditemukan 18, Beda Lokasi 3, Beda Kondisi 1, Tidak Terdaftar 1 |
| `AT-INV-34` ajukan tanpa catatan | 400 `AST-VAL-INV-021` "Beri catatan pada 23 selisih …" |
| Catatan oleh akun tanpa hak `Close` | 403 (lapisan hak akses; aturan ketua di service tidak tersentuh skenario ini) |
| Catatan > 1.000 karakter | 400 `AST-INV-FIELD` |
| Catatan pada 23 selisih oleh ketua | 23/23 tersimpan |
| Aksi ketua di `Pencocokan` | `reopenCounting`, `editNote`, `submit` |
| Ajukan | 200, `MenungguVerifikasi`, pengaju tercatat |
| Anggota tim melihat sesi | Aksi `verify` tidak ditawarkan |
| `AT-INV-35` anggota tim memverifikasi | 403 `AST-VAL-INV-022` |
| Kembalikan tanpa alasan → 400 `AST-VAL-INV-020`; dengan alasan → `Pencocokan`, `decisionReason` tersimpan | Lulus |
| Ajukan ulang; Kepala Bagian Aset ditawari `verify` | Lulus |
| `AT-INV-07` verifikasi | 200, `Diverifikasi`, verifikator tercatat, aksi `document` |
| Usulan per selisih | 24 = 18 Usulan Hilang + 3 Koreksi Lokasi + 2 Perubahan Kondisi (satu dari beda lokasi + kondisi) + 1 Pendaftaran Temuan, semua `Usulan` |
| `AST-DEC-019` aset tidak berubah sesudah verifikasi | Ruangan dan kondisi dua aset selisih sama dengan sebelum verifikasi |
| Linimasa `Diopname` bernomor sesi | Lulus |
| `AT-INV-37` ubah catatan sesudah diverifikasi | 409 `AST-VAL-INV-023` |
| Hasil `BedaLokasi` memuat `followUps` | Lulus |
| Rencana tindakan | "Penyerahan koreksi opname ke ruangan Ruang Uji U9KOL B"; "Mutasi koreksi opname ke Instalasi Rawat Inap / Ruang Uji U9KOL A" |
| `AT-INV-09` beda ruangan, unit sama | 200, penyerahan `AST-PNY-000014` |
| `AT-INV-08` aset IGD ditemukan di Rawat Inap | 200, mutasi `AST-MUT-000018` `Diajukan`, alasan Koreksi Opname, unit tujuan Rawat Inap |
| Perubahan kondisi | 200, laporan `AST-KND-000017` |
| Usulan hilang | 200, laporan `AST-KND-000018` kondisi Hilang |
| Temuan tanpa `draft` → 400; `AT-INV-10` dengan `draft` | 200, draft "Lemari besi kecil" berstatus Draft, keterangan memuat nomor sesi |
| `AST-VAL-INV-030` proses dua kali | 409 |
| `AT-INV-36` proses kedua untuk aset yang sama (koreksi lokasi lalu kondisi) | Pertama 200 `AST-PNY-000015`; kedua 409 `AST-VAL-INV-031` "aset AST-MBL-000009 sedang dalam proses penyerahan", usulan tetap `Usulan` (klaim dilepas) |
| Abaikan tanpa alasan → 400; dengan alasan → `Diabaikan`; abaikan yang sudah diproses → 409 `030` | Lulus |
| Proses oleh akun tanpa hak `Process` | 403 |
| Foto temuan diunduh lewat `register/attachments/{id}` | 200 `image/png` |
| `AT-INV-12` PDF berita acara oleh Kepala Bagian Aset | 200 `application/pdf`, 61.132 byte, diawali `%PDF` |
| Rapikan: 23 dokumen tujuan/usulan dibatalkan, ditolak, atau diabaikan | Semua 200 |

### 3.2 Isi berita acara `AST-OPN-000018` (dibaca langsung)

Dua halaman A4: kop PT Metropolitan Medical Centre; unit Instalasi Rawat Inap, ruangan Ruang Uji U9KOL A, rencana
23–24 November 2026, dimulai dan ditutup oleh Siti Nurhaliza, 22 aset diharapkan, tim Siti (ketua) dan Dimas; ringkasan
enam kategori (jumlah 24); tabel **Selisih (23)** dengan kolom tercatat, ditemukan, dan catatan tim (aset IGD tercatat
di Ruang Uji U94W3 C, ditemukan di U9KOL A; temuan "Lemari besi kecil tanpa label"); blok "Diajukan ketua tim" (Siti
Nurhaliza, Perawat Rawat Inap) dan "Diverifikasi Kepala Bagian Aset" (Uji Penyetuju Aset, Staff Logistik) dengan
waktu; penutup "Hasil opname tidak mengubah data aset; setiap selisih ditindaklanjuti lewat usulan"; nomor halaman.

### 3.3 Data uji

Sesi `AST-OPN-000017` (putaran 1) dan `000018` (putaran 2) berstatus `Diverifikasi` — sesi terverifikasi tidak dapat
dibatalkan. Semua usulannya `Diproses` atau `Diabaikan`; dokumen tujuannya dibatalkan (penyerahan, mutasi, draft) atau
ditolak (laporan kondisi), sehingga tidak ada aset yang masih terkunci oleh uji ini (dicek lewat daftar aset Rawat Inap
dan IGD). `AST-MBL-000008` tetap memegang kunci pemanfaatan lama yang bukan berasal dari uji ini.

---

## 4. Acceptance criteria dan Definition of Done

| Kriteria | Source | Bukti | Status |
|---|---|---|---|
| 1. Ajukan mewajibkan catatan selisih | `SubmitAsync`, `UpdateResultNoteAsync` | `AT-INV-34`, catatan 23/23, ajukan 200 | ✅ |
| 2. Verifikator Kepala Bagian Aset bukan anggota tim | `VerifyAsync` (`isMember`, `CanAccessUnit`) | `AT-INV-35`, aksi `verify` tersembunyi bagi anggota | ✅ |
| 3. Verifikasi membuat usulan tanpa mengubah aset; linimasa `Diopname` | `VerifyAsync` + `AddForSessionAsync` (satu transaksi) | `AT-INV-07`, 24 usulan, `AST-DEC-019`, linimasa | ✅ |
| 4. Proses usulan membuat dokumen tujuan sesuai jenis (`AST-DES-007`); abaikan dengan alasan | `StockTakeFollowUpService.ProcessAsync`, `DismissAsync` | `AT-INV-08`, `09`, `10`, kondisi, hilang, `AT-INV-36`, `030` | ✅ |
| 5. PDF berita acara (`AST-DEC-046`) | `StockTakeDocumentService`, `GetDocumentAsync` | `AT-INV-12`, isi 3.2 | ✅ |

| DoD | Status |
|---|---|
| `V-BE` QBE, build, runtime | Terpenuhi |
| Laporan, roadmap, traceability | Ditulis |
| `UAT-INV-04`, `05` | Milik tim UAT |
| Commit | Bersama `FE-AST-017`, menunggu izin Rizki |

---

## 5. Catatan

- Kunci **pemanfaatan** tidak menahan mulai opname, jadi aset yang sedang dimanfaatkan pihak lain bisa masuk sesi dan
  usulannya baru dapat diproses setelah pemanfaatan selesai (409 `AST-VAL-INV-031` menyebut prosesnya). Ini sesuai
  daftar penahan `AST-DES-031`; dicatat agar tidak terbaca sebagai cacat saat UAT.
- Penolakan catatan oleh bukan ketua hanya terbukti di lapisan hak akses. Aturan ketua di service (`EnsureSubmitActorAsync`)
  sama dengan aturan tutup pemindaian yang sudah terbukti di `BE-AST-022`.

---

## 6. Langkah berikutnya

- `BE-AST-024`: kartu 1.5 (sesi lewat tanggal belum diverifikasi + usulan `Usulan`) dan laporan inventaris per sesi dan
  kategori dengan ekspor; lalu `FE-AST-018`.
