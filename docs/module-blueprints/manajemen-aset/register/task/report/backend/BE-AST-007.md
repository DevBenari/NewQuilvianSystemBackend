# Laporan Perubahan Backend — `BE-AST-007`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-AST-007` |
| Judul | Pencatatan, persetujuan, dan penomoran aset |
| Slice | `REG-S2` Register aset (gelombang `MVP-1`) |
| Roadmap | `docs/module-blueprints/manajemen-aset/register/roadmap/backend-roadmap.md` `REV-1`, kartu `BE-AST-007` |
| Trace | `CAP-03.1..03.3`, `03.6`, `CAP-EXT-04`; `AST-DEC-025`, `040`, `043`; `AST-DES-002`, `004`, `016`; `contracts/api-contract.md` 2.8; `state-transition-matrix.md` §1; `validation-matrix.md` §2; `permission-audit-matrix.md` §2–5; `testing/acceptance-test-matrix.md` `AT-REG-01..03`, `30..33`, `35` |
| Contract version | `AST-REG-1.0.0` — `approved` 2026-11-12 (`AST-DEC-051`). Kepala berkas kontrak masih menulis `draft`; dicatat, tidak diubah |
| Dependency | `BE-AST-006` ✅ 2026-11-16; `BE-AST-015` [NLI] 🟡 2026-11-16 (service buku nilai dan kotak keluar tersedia; yang belum adalah uji runtime-nya, yang justru dibuka task ini) |
| Klasifikasi | `HIGH` — 13 endpoint, penerbitan nomor permanen, transaksi lintas sub-modul (buku nilai, kotak keluar), kunci baris, batas unit |
| Task mode | `BACKEND` — perintah Rizki "lakukan langkah selanjutnya", 2026-11-16, sesudah laporan `BE-AST-015` menyebut `BE-AST-007` sebagai langkah berikutnya |
| Target tulis | `Areas/Corporate/AssetManagement/Register/{DTOs,Services,Controllers}/`, `Register/AssetRegisterExplicitPermissions.cs`, `Register/Services/AssetManagementException.cs`, `AssetManagementServiceCollectionExtensions.cs`; laporan ini; baris status roadmap dan traceability `register` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `67b38ae6` (branch `rizkiG`, 4 commit di depan `origin/rizkiG`, belum push). Kode task ini belum di-commit |
| Mesin | PC kantor |
| Tanggal | 2026-11-16 |
| Status | ✅ Selesai — kedelapan acceptance criteria terpetakan ke source **dan** teruji di runtime. Bukti: QBE Strict `PASS`; build Rizki berhasil; Swagger 13 operasi; penanda `ReadAllUnits` terdaftar di DB; runtime **52/52** (alur utama) + **14/14** (penolakan, batas unit, `ViewAmount`); pemeriksaan DB baca-saja (kotak keluar, buku, linimasa, deret nomor). `AT-REG-32` (alokator gagal) dikecualikan atas persetujuan Rizki 2026-11-16: dijaga source, tidak dapat dipicu lewat API tanpa merusak deret nomor. Belum di-commit. **Riwayat:** 🟡 pada 2026-11-16 sore, ketika baru jalur pengaju yang teruji (36/36) karena akun penyetuju belum ada |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` |
| Module | `AssetManagement` (Manajemen Aset & Inventaris) |
| Submodule | `Register` (Register Aset) |
| Pemilik / prefix registry | `AssetManagement / Register` / `Ast` / `ACTIVE` (baris registry `BE-AST-001`) |
| Keberlakuan | `NEW CODE` (8 berkas baru); `TOUCHED LEGACY` hanya berkas modul ini sendiri: `AssetManagementException.cs` (+33 baris konstanta) dan `AssetManagementServiceCollectionExtensions.cs` (+2 baris) |
| QBE ID yang berlaku | `QBE-SVC-001..003`, `QBE-API-001..003`, `QBE-PERM-001..003`, `QBE-DTO-001..003`, `QBE-PAGE-001`, `QBE-OPT-001`, `QBE-NAM-001`, `QBE-NAM-002`, `QBE-MOD-001..003`. Tidak berlaku: `QBE-ENT-*`/`QBE-CFG-001` (tanpa entity baru), `QBE-CODE-001..003` (nomor lewat `NumberSeriesAllocator`, bukan Count/Max+1), `QBE-NAM-003`/`QBE-DB-001/002` |
| Governance terbaca | `AGENTS.md` backend, `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, `tooling/qbe/README.md`, standar endpoint transaksi suite; tidak berubah sejak `BE-AST-015` |
| Arketipe transaksi | **Aggregate ber-lifecycle** (`AstAsset`: `Draft` → `Diajukan` → `Terdaftar`/`Draft`; `Draft` → `Dibatalkan`) |

### Pemeriksaan snapshot

HEAD `67b38ae6`, working tree bersih sebelum task. Nama class baru (`AssetRecordService`, `AssetsController`,
`AssetRegisterLabels`, DTO register) masing-masing satu deklarasi. Kontrak `AST-REG-1.0.0` dan service
`BE-AST-005`/`BE-AST-015` tidak berubah sejak dirujuk.

---

## 1. Masalah yang diperbaiki

Sesudah master (`BE-AST-006`) dan service nilai (`BE-AST-015`) tersedia, belum ada jalan untuk mencatat aset.
Petugas Aset belum bisa membuat draft dan mengajukannya, Kepala Bagian Aset belum bisa menyetujuinya, dan
nomor inventaris serta buku nilai belum bisa terbit.

Contoh: RS menerima 40 kursi tunggu. Petugas Aset membuat 40 draft sekaligus lalu mengajukannya. Kepala
Bagian Aset menyetujui 38 di antaranya, sehingga terbit 38 nomor `AST-MBL-######` berurutan, masing-masing
dengan buku nilai. Dua draft yang datanya belum benar tetap tanpa nomor.

---

## 2. Proses bisnis

| Unsur | Isi |
|---|---|
| Tujuan | Aset dapat dicatat dari draft sampai `Terdaftar` dengan nomor permanen dan buku nilai |
| Pelaku | Petugas Aset (draft, ubah, ajukan, batal), Kepala Bagian Aset (setujui, tolak), sistem (nomor, buku nilai, kotak keluar hibah, linimasa) |
| Pemicu | Barang diterima (pembelian, dengan atau tanpa tautan penerimaan barang) atau hibah masuk |
| Hasil akhir | Aset `Terdaftar`, kondisi `Aktif`, `Belum Ditempatkan`; siap diserahkan (`BE-AST-009`) |

Aturan yang dijaga:

1. **Draft 1–200 sekaligus** (`AST-VAL-REG-008`). Saat dibuat, yang diperiksa hanya nama, kelompok,
   kepemilikan, jenis perolehan, tanggal, format harga, dan tautan GR. Atribut alat medis dan nomor BA hibah
   **tidak** diperiksa saat draft. Contoh matriks: 10 syringe pump tanpa nomor seri tetap tersimpan sebagai
   draft.
2. **Ajukan** memeriksa kelengkapan penuh: kelompok dan kepemilikan masih aktif, BA hibah (`004`), tanggal
   (`005`), harga > Rp 0 (`006`), atribut medis untuk kelompok yang klasifikasinya medis (`007`), dan GR yang
   belum dibatalkan (`009`). Contoh `AT-REG-30`: monitor tanpa nomor izin edar ditolak 400, dan statusnya
   tetap `Draft`.
3. **Setujui:** baris aset dikunci (`SELECT … FOR UPDATE`). Lalu diperiksa: status masih `Diajukan`,
   penyetuju bukan pengaju (`012`), kelompok masih aktif (`011`), dan parameter nilai ada (`AST-VAL-NLI-008`).
   Pemeriksaan parameter sengaja dilakukan **sebelum** nomor diminta, supaya kegagalan yang bisa diramalkan
   tidak membuang nomor. Sesudah lolos, nomor diterbitkan alokator dengan transaksinya sendiri; bila
   alokator gagal, hasilnya 409 `013`. Aset menjadi `Terdaftar`. Buku nilai dan kejadian hibah ditulis oleh
   `AssetBookService.CreateForRegisteredAssetAsync`, dan linimasa `Didaftarkan` ditulis. Semuanya masuk satu
   transaksi.
4. **Persetujuan bersamaan** atas aset yang sama: permintaan kedua menunggu kunci baris, lalu melihat aset
   sudah `Terdaftar` dan dijawab 409 `015`. Jadi hanya satu nomor terbit (`AT-REG-33`).
5. **Persetujuan massal** (1–200 aset): setiap aset diproses dalam transaksinya sendiri, dan hasilnya per aset
   (`AssetDecisionResultResponse`). Satu kegagalan tidak membatalkan aset lain.
6. **Tolak** mengembalikan aset ke `Draft` tanpa nomor, dengan alasan wajib (`010`). Pengaju tidak boleh
   menolak pengajuannya sendiri (`012`, kolom "Berlaku pada: Setujui, tolak").
7. **Batalkan** hanya untuk `Draft`, dengan alasan wajib. Aset menjadi `Dibatalkan`, dan `IsCancel`,
   `CancelBy`, serta `CancelDateTime` diisi.
8. **Rupiah hanya lewat `/amounts`** (`AST-DES-016`, `AST-FE-004`). Daftar dan detail tidak memuat harga
   perolehan.
9. **Batas unit.** Tanpa penanda `AssetRegister : ReadAllUnits`, daftar, ringkasan, dan detail dibatasi ke unit
   penempatan pengguna. Draft dan aset yang belum ditempatkan tidak punya unit, sehingga hanya terlihat oleh
   pemegang penanda. Ini sesuai matriks: semua peran yang membuat dan menyetujui aset memegang penanda itu.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kontrak 2.8, matriks status/validasi/izin/uji, kamus §3.1, arsitektur register §1–5; `AstAsset`,
`AstAssetHistory`, master `Mst*`, `FinGoodsReceipt`, `ApplicationUser`; service `BE-AST-005` dan
`BE-AST-015`; `AssetMasterControllerBase`, `AssetMasterDataService` (gaya); `NumberSeriesAllocator`
(transaksi sendiri lewat `IDbContextFactory`); `AccessExplicitPermissionAttribute`,
`PermissionRegistryDescriptor` (penanda wajib menunjuk resource yang sudah terdaftar);
`RadiologyExplicitPermissions.cs` (preseden); pola `FOR UPDATE` di `DailyMonitoringService`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/AssetManagement/Register/DTOs/AssetRecordDtos.cs` | Baru — query, draft (basis bersama + create/update), keputusan, hasil per aset, daftar, detail (+`AvailableActions`, `RowVersion`), `/amounts` + ringkasan buku, linimasa, ringkasan |
| `.../Register/Services/AssetRecordService.cs` | Baru — konstruktor, transaksi (`DbUpdateConcurrencyException` → 409 `015`), kunci baris, label enum `AssetRegisterLabels` |
| `.../Register/Services/AssetRecordService.Draft.cs` | Baru — buat draft, ubah (cek `RowVersion`), ajukan, batalkan, validasi draft dan pengajuan |
| `.../Register/Services/AssetRecordService.Approval.cs` | Baru — setujui, setujui massal, tolak |
| `.../Register/Services/AssetRecordService.Query.cs` | Baru — metadata, ringkasan, daftar, detail, `/amounts`, linimasa, pembatasan unit |
| `.../Register/Controllers/AssetsController.cs` | Baru — 13 endpoint, modul `CORPORATE_ASSET_MANAGEMENT_REGISTER`, resource `AssetRegister` |
| `.../Register/AssetRegisterExplicitPermissions.cs` | Baru — `[assembly: AccessExplicitPermission]` `AssetRegister : ReadAllUnits` (tunggakan `BE-AST-005`) |
| `.../Register/Services/AssetManagementException.cs` | +16 kode galat, +17 pesan (`001..012`, `014`, `AST-REG-FIELD/STATUS/NOT-FOUND`) |
| `.../AssetManagementServiceCollectionExtensions.cs` | +`AddScoped<AssetRecordService>()` |
| Laporan ini, roadmap backend dan frontend serta traceability `register` | Status `BE-AST-007` |
| `register/task/report/backend/BE-AST-006.md`, baris `BE-AST-006` di roadmap/traceability `register` | `AT-REG-35` kini lulus |
| `nilai/task/report/backend/BE-AST-015.md`, roadmap dan traceability `nilai` | `AT-NLI-13` lulus; kriteria 3 sebagian teruji |

Nol baris komentar pada seluruh berkas `.cs` yang dibuat atau diubah.

Keputusan teknis di dalam cakupan:

| Keputusan | Alasan |
|---|---|
| Kunci baris `FOR UPDATE` untuk ubah, ajukan, setujui, tolak, batal | Alokator punya transaksi sendiri. Tanpa kunci, dua penyetuju bisa sama-sama meminta nomor, lalu satu gagal menyimpan dan nomornya terbuang. Dengan kunci, penyetuju kedua menunggu lalu ditolak sebelum meminta nomor |
| Approve/reject atas aset yang sudah diputuskan → 409 `015`; transisi tak sah lain → 409 `AST-REG-STATUS` | Kontrak 2.8: "409 pada `/approve` berarti aset sudah disetujui atau ditolak orang lain"; `AT-REG-33` merujuk `015` |
| `PUT` membawa `RowVersion`; aksi bernama tidak | Kontrak memberi `-` sebagai request `submit`/`approve`. Untuk aksi itu, status yang dicek di bawah kunci baris sudah mencegah keputusan ganda |
| `SaldoAwal` ditolak saat draft (`AST-REG-FIELD`) | Saldo awal hanya lewat impor (`AST-DEC-037`, `050`, state §1 "(impor) → Terdaftar"). Bila draft `SaldoAwal` disetujui, buku perolehan dan kotak keluar akan ditulis untuk aset lama |
| Format harga (< 0, > 2 desimal) diperiksa juga saat draft | Kolom `numeric(18,2)` akan membulatkan diam-diam. Syarat "> 0" tetap hanya saat diajukan, sesuai matriks |
| `PurchaseOrderId` dan `GoodsReceiptNumber` diambil dari GR | Kamus: snapshot nomor GR; PO dari GR yang sama, sehingga tidak bisa tidak cocok |
| `ConditionChangedAt` diisi saat disetujui | Dasar hitung aset menganggur (`AST-DES-017`) dimulai saat aset terdaftar |
| Batas unit juga berlaku untuk tulis | Kontrak `016`: "Baca dan ubah". Draft tanpa unit hanya untuk pemegang `ReadAllUnits` |
| Logger: jejak di `message`, tanpa rupiah | `permission-audit-matrix` §4: argumen `data` tidak tertulis; §5: harga tidak masuk logger |
| Tidak ada `IdempotencyKey` | Klik ganda pada aksi dijawab 409 di bawah kunci baris, jadi tidak ada kejadian ganda. Pembuatan draft ganda tidak berdampak keuangan dan dapat dibatalkan |

### 3.3 Delta terhadap kontrak `AST-REG-1.0.0`

| No | Kontrak | Source | Alasan |
|---|---|---|---|
| 1 | — | `GET /filters/metadata`, `GET /summary` | Baseline aggregate ber-lifecycle pada standar endpoint transaksi; `AssetMasterFilterMetadataResponse` dipakai ulang |
| 2 | `AssetDetailResponse` | + `AvailableActions` (`Update`, `Submit`, `Cancel` / `Approve`, `Reject`), `RowVersion`, nama pengaju/penyetuju/penolak | Standar transaksi §5.1; `RowVersion` dibutuhkan `PUT` (`015`) |
| 3 | Paging `page`, bawaan 20; `sortDir` | `pageNumber`, bawaan 25; `sortDirection` | Sama dengan endpoint master `BE-AST-006` dan `PagedResult` proyek |
| 4 | `AssetPagedQuery` (+`isIdle`) | + `acquisitionType`; **tanpa** `isIdle` | Aset menganggur adalah keadaan turunan milik `BE-AST-011` (kartu AC2) |
| 5 | `POST /approve-batch` | Batas 1–200 aset per permintaan | Sama dengan batas draft; mencegah permintaan tanpa batas |
| 6 | Kode galat | + `AST-REG-FIELD` (400), `AST-REG-STATUS` (409), `AST-REG-NOT-FOUND` (404) | Pola `AST-MST-*`; belum ada di matriks galat |
| 7 | `POST /drafts` → `201` (kontrak §1 "Data baru tersimpan") | `200` | `AssetMasterControllerBase.ExecuteAsync` yang dipakai seluruh modul mengembalikan `200` |
| 8 | Kartu: `StageDonationAcquisition` | Tidak ada; hibah diantrekan di dalam `CreateForRegisteredAssetAsync` | Delta `BE-AST-015` no. 8 |
| 9 | Kontrak 2.8: `/labels`, `/by-number`, `/idle` | Tidak dibuat | `BE-AST-008` (label, cari nomor) dan `BE-AST-011` (menganggur) |
| 10 | `03-frontend-architecture.md` §3.2: `GET /?recordStatus=Draft&createdByMe=true`; §3.3: baris milik pengaju tidak dapat dicentang | **Tambalan 2026-11-17** (atas pilihan Rizki saat `FE-AST-003`): `AssetPagedQuery.CreatedByMe` (saring `CreateBy` = pengguna), dan `AssetListItemResponse` + `CreateBy`, `SubmittedBy`, `RejectedAt`, `DecisionReason`. Metadata daftar ikut memuat `createdByMe`. +15 baris di `AssetRecordDtos.cs` dan `AssetRecordService.Query.cs`; QBE Strict `PASS`; nol komentar; build Rizki 2026-11-17 berhasil (DLL 08:54:22); kolom teramati di respons daftar dan dipakai uji layar `FE-AST-003` | Tanpa kolom ini, layar tidak dapat menyaring "draft saya", menandai draft yang dikembalikan, atau menonaktifkan centang pengaju tanpa membuka rincian per baris. Kolom bukan rupiah, jadi `AST-DES-016` tetap terjaga |
| 11 | `03-frontend-architecture.md` `SCR-AST-REG-11` Data Perolehan: kolom perolehan di daftar | **Tambalan 2026-11-17** (atas izin Rizki saat `FE-AST-004`): `AssetListItemResponse` + `ReadyForUseDate`, `SupplierName`, `GoodsReceiptNumber`, `DonationReference`, dipetakan di `AssetRecordService.Query.cs`. +8 baris; QBE `PASS`; nol komentar; build Rizki 2026-11-17 berhasil (DLL 09:29:11); kolom teramati di layar Data Perolehan (saring Hibah Masuk → nomor `BA-HIBAH…`) | Tanpa kolom ini, Data Perolehan harus membuka rincian per baris. Kolom bukan rupiah, jadi `AST-DES-016` tetap terjaga |

### 3.4 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 13 endpoint baru (bagian 4); delta 3.3 |
| Database | Tanpa migration, tanpa perubahan skema. Saat startup, registry akses menambah `SysControllerAccess` `AssetRegister` dan 7 `SysActionAccess` (perilaku standar seeder). Uji runtime menulis data (bagian 5.2) |
| Keamanan/Auth | Setiap action ber-`[AccessAction]` + `[AccessPermission]`. Arg ke-1 `AssetRegister` = `ControllerName`, arg ke-2 = nama `AccessAction`; `AccessType` hanya `Read`/`Create`/`Update`. Tidak ada `IsInRole`, nama peran, atau `UserType`. Kewenangan data dijaga service: pengaju ≠ penyetuju dan batas unit. Penanda `ReadAllUnits` hanya **dapat diberikan**; tidak ada policy yang dibuat otomatis |

---

## 4. Dokumentasi endpoint

Base: `api/v1/corporate/asset-management/register/assets`. Semua respons `ApiResponse<T>`; galat membawa
`errors.code`.

| Method | Path | Izin | Request | Response | Galat utama |
|---|---|---|---|---|---|
| `GET` | `/filters/metadata` | `AssetRegister : Read` | — | `AssetMasterFilterMetadataResponse` | — |
| `GET` | `/summary` | `AssetRegister : Read` | — | `AssetRegisterSummaryResponse` | 503 badan hukum belum diatur |
| `GET` | `/` | `AssetRegister : Read` | `AssetPagedQuery` | `PagedResult<AssetListItemResponse>` | — |
| `GET` | `/{id}` | `AssetRegister : Read` | — | `AssetDetailResponse` | 403 `016`, 404 |
| `GET` | `/{id}/amounts` | `AssetRegister : ViewAmount` | — | `AssetAmountResponse` | 403 `016`, 404 |
| `GET` | `/{id}/history` | `AssetRegister : Read` | `pageNumber`, `pageSize` | `PagedResult<AssetHistoryItemResponse>` | 403 `016`, 404 |
| `POST` | `/drafts` | `AssetRegister : Create` | `AssetDraftCreateRequest` (`quantity` + isian) | `List<AssetListItemResponse>` | 400 `001..003`, `005`, `006`, `008`, `009`, `FIELD` |
| `PUT` | `/{id}` | `AssetRegister : Update` | `AssetDraftUpdateRequest` (+`rowVersion`) | `AssetDetailResponse` | 400 seperti draft; 409 `014`, `015` |
| `POST` | `/{id}/submit` | `AssetRegister : Submit` | — | `AssetDetailResponse` | 400 `002..007`, `009`; 409 `STATUS` |
| `POST` | `/{id}/approve` | `AssetRegister : Approve` | — | `AssetDetailResponse` | 403 `012`; 409 `011`, `013`, `015`, `AST-VAL-NLI-008` |
| `POST` | `/approve-batch` | `AssetRegister : Approve` | `{ assetIds: Guid[] }` | `List<AssetDecisionResultResponse>` | 400 `FIELD`; galat per aset di isi |
| `POST` | `/{id}/reject` | `AssetRegister : Approve` | `{ reason }` | `AssetDetailResponse` | 400 `010`; 403 `012`; 409 `015` |
| `POST` | `/{id}/cancel` | `AssetRegister : Update` | `{ reason }` | `AssetDetailResponse` | 400 `010`; 409 `STATUS` |

Kode `0xx` = `AST-VAL-REG-0xx`.

---

## 5. Verifikasi

### 5.1 Statis dan build

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` (WorkingTree) | `Files evaluated: 9`; `VIOLATION: 0`, `REVIEW: 0`, `INFO: 0`; `Final result: PASS` | `PASS` | 2026-11-16 sore |
| Baris komentar | 0 pada 7 berkas baru + 0 baris komentar ditambahkan pada 2 berkas yang diubah | `PASS` | `Select-String` dan `git diff -U0` |
| `dotnet build` | Dijalankan Rizki dan dilaporkan berhasil. DLL 2026-11-16 15:45:37 WIB, sesudah suntingan terakhir 15:17:22 WIB, memuat `AssetRecordService`, `AssetsController`, `AssetDraftCreateRequest`, `AssetRegisterLabels` | `PASS` | Jumlah warning tidak dilaporkan |
| Swagger `/swagger/corporate/swagger.json` (aplikasi Rizki, mulai 15:50:19) | 13 operasi di bawah `/register/assets`, tag `Corporate / Asset Management / Register / Asset`, persis bagian 4 | `PASS` | Skrip Node |
| Registry akses (DB baca saja, `default_transaction_read_only=on`) | `SysActionAccess` untuk `AssetRegister`: `Read`, `ViewAmount`, `Create`, `Update`, `Submit`, `Approve`, `ReadAllUnits` (tanpa route, `Read`); semuanya aktif, dibuat 15:50:40 WIB. Aplikasi start normal, jadi deklarasi penanda diterima | `PASS` | `dotnet fsi` + Npgsql dari `bin` |

### 5.2 Akun penyetuju uji

Atas arahan Rizki 2026-11-16, akun ini dibuat agent dengan login superadmin lewat API aplikasi, tanpa SQL tulis:

| Langkah | Hasil |
|---|---|
| `POST /corporate/human-resource/master-data/external-users` | Pengguna eksternal "Uji Penyetuju Aset" (`uji.penyetuju.aset@example.com`, userId `9e42f707-…`) + akun login; penempatan Logistik dan Gudang × Staff Logistik |
| Pemeriksaan sebelum memberi izin (DB baca-saja dan `GET role-access/policies`) | Pasangan Logistik dan Gudang × Staff Logistik: 0 pengguna, 0 policy. Jadi pemberian izin tidak mengubah hak orang lain |
| `POST role-access/policies` (policy lama digabung dengan yang baru, bukan ditimpa) | 6 izin: `AssetRegister` Read, Approve, ViewAmount, ReadAllUnits; `Profile` Read, Update. **Tanpa** Create/Update/Submit, sehingga penyetuju terpisah dari pengaju |
| Login pertama + `PUT /self-services/profile/password` | Sandi awal diganti dengan sandi acak; `MustChangePassword` selesai |
| Kredensial | Disimpan ke env **User** Windows `QUILVIAN_ASET_APPROVER_EMAIL`/`_PASSWORD`; sandi tidak dicetak dan tidak ditulis ke berkas mana pun |

### 5.3 Runtime alur utama

HTTPS `https://localhost:7184`; pengaju = superadmin uji, penyetuju = akun 5.2; tag data `UJI-007-UK0W`. **Hasil:
52 dari 52 `PASS`.** Putaran pertama sebelum akun penyetuju ada (tag `UJI-007-XH9P`) menghasilkan 36/36 untuk
jalur pengaju; seluruh skenarionya diulang di putaran ini.

| Kelompok | Skenario | Hasil |
|---|---|---|
| Baca | Login pengaju dan penyetuju; `filters/metadata`, `summary`, daftar 200; master `ALKMED`, `MBL`, `MILIK`, `KSO` tersedia | 6/6 |
| Validasi draft | Jumlah 0 dan 201 → 400 `008`; nama kosong → `001`; kelompok tak dikenal → `002`; kepemilikan tak dikenal → `003`; `SaldoAwal` → `AST-REG-FIELD`; siap pakai < perolehan dan perolehan masa depan → `005`; harga −1 → `006` | 9/9 |
| Draft | 3 draft monitor `ALKMED` Rp45.000.000 tanpa izin edar → 200, tanpa nomor, `Draft`; daftar dan detail tanpa `registeredAcquisitionCost` | 4/4 |
| **`AT-REG-30`** | Ajukan monitor tanpa izin edar → 400 `007`; status tetap `Draft`, aksi `Update,Submit,Cancel` | 2/2 |
| Ubah dan ajukan | `RowVersion` basi → 409 `015`; benar → 200; ajukan → 200; ubah aset diajukan → 409 `014`; ajukan ulang → 409 `AST-REG-STATUS` | 5/5 |
| **`AT-REG-31`** | Pengaju menyetujui → 403 `012`; menolak → 403 `012`; persetujuan massal oleh pengaju → 200 dengan hasil per aset `012`; status tetap `Diajukan` | 4/4 |
| Batal dan lain-lain | Batal tanpa alasan → 400 `010`; dengan alasan → 200; batal ulang → 409 `STATUS`; batch kosong → 400 `FIELD`; aset tak ada → 404 `AST-REG-NOT-FOUND`; `/amounts` draft → 200 tanpa buku; tanpa login → 401 | 7/7 |
| Persetujuan | Penyetuju lain menyetujui → 200, nomor `AST-ALKMED-000001`, `Terdaftar`, `Belum Ditempatkan`; setujui ulang → 409 `015`; buku: dikapitalisasi, disusutkan, Rp45.000.000, umur 60, mulai 2026-12-01 (bulan sesudah siap pakai 2026-11-10); linimasa 1 kejadian `Didaftarkan` bernomor sama | 5/5 |
| **`AT-REG-35`** (tunggakan `BE-AST-006`) | Ubah kode kelompok `ALKMED` yang sudah bernomor → 409 `AST-VAL-REG-053` | 1/1 |
| **`AT-REG-01`** | 4 kursi `MBL` Rp450.000 diajukan; persetujuan massal atas 3 kursi + 1 draft + 1 id asing → 200; hasil `AST-MBL-000001`, `000002`, `000003` berurutan, draft → 409 `AST-REG-STATUS`, id asing → 404 | 2/2 |
| **`AT-REG-02`** | Kursi Rp450.000 dengan batas Rp1.000.000 → buku `IsCapitalized=false`, `IsDepreciable=false` | 1/1 |
| **`AT-REG-33`** | Dua permintaan setujui bersamaan atas kursi ke-4 → satu 200 (`AST-MBL-000004`), satu 409 `015` | 1/1 |
| Kepemilikan | Monitor ber-kepemilikan `KSO` (tidak dapat dikapitalisasi) disetujui → buku `IsCapitalized=false` | 2/2 |
| **`AT-REG-03`** | Kursi roda hibah Rp3.000.000 tanpa nomor BA → 400 `004`; dilengkapi → diajukan → disetujui 200 | 3/3 |

### 5.4 Runtime penolakan, batas unit, dan hak rupiah

Tag `UJI-007X-VJ68`. **Hasil: 14 dari 14 `PASS`.**

| Skenario | Hasil |
|---|---|
| Tolak tanpa alasan → 400 `010`; tolak oleh penyetuju → 200; aset kembali `Draft`, **tanpa nomor**, alasan dan nama penolak tercatat | 3/3 |
| Tolak ulang aset yang sudah ditolak → 409 `015`; ajukan ulang → 200 dan alasan penolakan dihapus | 3/3 |
| Izin `ReadAllUnits` dan `ViewAmount` dicabut sementara dari peran penyetuju uji → detail aset tanpa unit → 403 `016`; daftar → 200 dengan 0 baris; setujui → 403 `016`; **`AT-REG-47`** `/amounts` tanpa `ViewAmount` → 403 | 5/5 |
| Izin dipulihkan → detail kembali 200 | 3/3 |

### 5.5 Pemeriksaan DB baca-saja

`dotnet fsi` + Npgsql dari `bin`, `default_transaction_read_only=on`.

| Yang diperiksa | Hasil |
|---|---|
| **`AT-NLI-13`** kotak keluar hibah | Tepat **1 baris** di seluruh `AstAccountingOutbox`: `AST-PEROLEHAN-HIBAH`, `GRANT:50038639-…`, versi `1`, status `2` (`Held`), Rp3.000.000, komponen `[{"ComponentCode":"ALKMED","Amount":3000000.00}]`, tanggal akuntansi 2026-11-16, tanggal dokumen 2026-11-05. Status `Held`, bukan `Pending` seperti `AT-REG-03`, karena `EnabledEventCodes` kosong sampai `GATE-AST-ACC` (`BE-AST-015` kriteria 4) |
| Buku + entri hibah | Dikapitalisasi, Rp3.000.000, 60 bulan, mulai 2026-12-01; satu entri `Perolehan` periode 2026-11, nomor `AST-ALKMED-000003` |
| Seluruh aset bernomor | 7: `AST-ALKMED-000001` (dikapitalisasi), `000002` (KSO, tidak), `000003` (hibah, ya), `AST-MBL-000001..000004` (tidak) |
| Deret nomor | `AST_INV_ALKMED` = 3, `AST_INV_MBL` = 4, sama persis dengan jumlah aset bernomor. Jadi **tidak ada nomor terbuang**, termasuk pada uji bersamaan `AT-REG-33` |
| Linimasa | 7 kejadian `Didaftarkan`, sub-modul `register`; satu per aset terdaftar |

### 5.6 Data yang ditulis ke `QuilvianNewDevRizki`

Izin Rizki 2026-11-16. Aset terdaftar tidak dapat dihapus lewat API.

| Data | Rincian |
|---|---|
| Aset | Putaran `XH9P`: 3 monitor (1 `Diajukan`, 1 `Draft`, 1 `Dibatalkan`). Putaran `UK0W`: 7 aset `Terdaftar` di atas, ditambah 1 monitor `Draft` dan 1 monitor `Dibatalkan`. Putaran `VJ68`: 1 lemari `MBL` `Diajukan` (sesudah ditolak sekali) |
| Nilai | 7 buku nilai, 7 entri `Perolehan`, 1 baris kotak keluar `Held` |
| Akun | 1 pengguna eksternal + akun login + penempatan; 6 `SysAccessPolicy` untuk Logistik dan Gudang × Staff Logistik |
| Master | Tidak berubah. Uji `AT-REG-35` ditolak 409, jadi kode `ALKMED` tetap |

### 5.7 Pengecualian

| Skenario | Status | Alasan |
|---|---|---|
| `AT-REG-32` alokator gagal di tengah persetujuan | `NOT RUN`, **dikecualikan atas persetujuan Rizki 2026-11-16** | Alokator hanya gagal pada input tak sah atau luapan deret. Prefix terpanjang `AST-` + 11 huruf = 15 karakter, pas di batas `MaxPrefixLength` 15, jadi kegagalan tidak dapat dipicu lewat API tanpa mengubah `NumNumberSeries`. Dijaga source: nomor diminta paling akhir sesudah semua pemeriksaan; `NumberSeriesAllocationException` → 409 `013`; `InTransactionAsync` membatalkan transaksi bisnis; nomor yang sempat terbit tidak dipakai ulang karena alokator memakai transaksinya sendiri |

Skrip `ast007-runtime.mjs`, `ast007-extra.mjs`, `ast007-setup-approver.mjs`, dan `ast007-ro.fsx` ada di scratchpad sesi.

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.

**Tidak dijalankan agent:** `dotnet build`, `dotnet run`, `dotnet ef`, migration, SQL tulis. Seluruh penulisan data
lewat API aplikasi.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. 1–200 draft per permintaan | Terpenuhi, teruji runtime | `CreateDraftsAsync`; 0/201 → 400 `008`, 3 draft → 200 |
| 2. Atribut medis wajib saat diajukan | Terpenuhi, teruji runtime | `EnsureReadyForSubmissionAsync`; `AT-REG-30` |
| 3. Persetujuan: nomor terbit, `Terdaftar`, buku nilai, hibah → satu baris kotak keluar | Terpenuhi, teruji runtime | `ApproveCoreAsync` → `AllocateInventoryNumberAsync` → `CreateForRegisteredAssetAsync` → `Append(Didaftarkan)` dalam satu transaksi. Bukti 5.3 (nomor, buku) dan 5.5 (1 baris kotak keluar, linimasa) |
| 4. Persetujuan massal per aset transaksi sendiri | Terpenuhi, teruji runtime | `ApproveBatchAsync`; `AT-REG-01`: 3 berhasil berurutan, 2 gagal tanpa membatalkan yang lain |
| 5. Pengaju ≠ penyetuju | Terpenuhi, teruji runtime | `EnsureNotSubmitter`; `AT-REG-31` (approve, reject, batch) |
| 6. Tolak → `Draft` tanpa nomor | Terpenuhi, teruji runtime | `RejectAsync`; 5.4 |
| 7. Rupiah hanya `/amounts` | Terpenuhi, teruji runtime | Daftar/detail tanpa kolom rupiah; `AT-REG-47` 403 tanpa `ViewAmount` |
| 8. Batas unit sesuai `AST-DES-016` | Terpenuhi, teruji runtime | `ScopedAssetsAsync`, `EnsureCanAccessUnit`; 5.4: tanpa `ReadAllUnits` → 403 `016` pada detail dan persetujuan, daftar kosong |
| `DoD-BE`: `V-BE` | Terpenuhi dengan pengecualian | QBE, build, Swagger, registry, runtime 52/52 + 14/14, DB baca-saja. `AT-REG-32` dikecualikan (5.7) |
| `DoD-BE`: laporan, roadmap, traceability, `git status` | Terpenuhi | Laporan ini; bagian 7 |

Rekap: **8 dari 8 kriteria terpetakan ke source dan teruji di runtime**, seluruh `DoD-BE` terpenuhi dengan
pengecualian `AT-REG-32` yang disetujui Rizki → `✅`. Riwayat: `🟡` ketika baru jalur pengaju yang teruji.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada peringatan tool. Jumlah warning build tidak dilaporkan |
| Masalah yang diketahui | (1) `AT-REG-03` menulis `Pending`; dengan konfigurasi saat ini hasilnya `Held` sampai `GATE-AST-ACC`, sesuai `BE-AST-015` kriteria 4. (2) Kode `AST-REG-*` belum ada di matriks galat. (3) Kontrak masih `draft` di kepala berkas |
| Risiko tersisa | (1) `FOR UPDATE` menahan baris aset selama alokator bekerja; pada persetujuan massal 200 aset prosesnya berurutan, jadi waktunya sebanding jumlah aset. (2) Pengguna ber-`Create` tanpa `ReadAllUnits` dapat membuat draft, tetapi tidak dapat membuka atau mengubahnya. (3) Peran Logistik dan Gudang × Staff Logistik di DB dev kini memegang 6 izin uji (5.2); pegawai yang kelak ditempatkan di sana ikut mendapatkannya, kecuali izin itu dicabut |
| Perubahan sampingan | Akun penyetuju uji dan izin perannya (5.2), dibuat atas arahan Rizki. Tidak ada perubahan source di luar cakupan |
| Interupsi | Uji persetujuan sempat tertahan karena belum ada akun kedua; sudah diselesaikan lewat 5.2 |
| Langkah berikutnya | (1) Commit `BE-AST-007`. (2) Berikutnya `BE-AST-008`/`009`/`010`/`011` (boleh paralel) dan `FE-AST-003` sebagai pasangan layar pencatatan |

`git status --short` di akhir task:

```text
 M Areas/Corporate/AssetManagement/AssetManagementServiceCollectionExtensions.cs
 M Areas/Corporate/AssetManagement/Register/Services/AssetManagementException.cs
 M docs/module-blueprints/manajemen-aset/nilai/roadmap/backend-roadmap.md
 M docs/module-blueprints/manajemen-aset/nilai/roadmap/requirement-traceability.md
 M docs/module-blueprints/manajemen-aset/nilai/task/report/backend/BE-AST-015.md
 M docs/module-blueprints/manajemen-aset/register/roadmap/backend-roadmap.md
 M docs/module-blueprints/manajemen-aset/register/roadmap/frontend-roadmap.md
 M docs/module-blueprints/manajemen-aset/register/roadmap/requirement-traceability.md
 M docs/module-blueprints/manajemen-aset/register/task/report/backend/BE-AST-006.md
?? Areas/Corporate/AssetManagement/Register/AssetRegisterExplicitPermissions.cs
?? Areas/Corporate/AssetManagement/Register/Controllers/
?? Areas/Corporate/AssetManagement/Register/DTOs/
?? Areas/Corporate/AssetManagement/Register/Services/AssetRecordService.Approval.cs
?? Areas/Corporate/AssetManagement/Register/Services/AssetRecordService.Draft.cs
?? Areas/Corporate/AssetManagement/Register/Services/AssetRecordService.Query.cs
?? Areas/Corporate/AssetManagement/Register/Services/AssetRecordService.cs
?? docs/module-blueprints/manajemen-aset/register/task/report/backend/BE-AST-007.md
```

Tidak ada yang di-stage atau di-commit.
