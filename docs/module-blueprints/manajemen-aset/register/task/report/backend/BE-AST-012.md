# Laporan Perubahan Backend — `BE-AST-012`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-AST-012` |
| Judul | Impor data awal |
| Slice | `REG-S2` — Register aset: pencatatan, label, penempatan, kondisi, dokumen, pemanfaatan, impor, dashboard |
| Roadmap | `register/roadmap/backend-roadmap.md` — kartu `BE-AST-012` |
| Trace | `CAP-EXT-01`; `AST-DEC-023`, `AST-DEC-037`, `AST-DEC-050`, `AST-RULE-007`, `AST-OQ-006`; API 2.15; `state-transition-matrix` §6; `validation-matrix` §6 (`AST-VAL-REG-060..068`); `permission-audit-matrix` (`AssetImport`); `AT-REG-10`, `43`, `44`, `45` |
| Contract version | `AST-REG-1.0.0` `approved` 2026-11-12 |
| Dependency | `BE-AST-009` ✅ (penempatan); memakai `BE-AST-005` (penomoran, lampiran, linimasa), `BE-AST-015` (`AssetBookService.CreateOpeningBalanceAsync`, parameter nilai) |
| Klasifikasi | `HEAVY` — 1 aggregate ber-lifecycle (batch, 7 status), pembacaan Excel, penerapan massal satu transaksi, 1 controller (11 endpoint); tanpa entity atau migration baru |
| Task mode | `BACKEND`, diminta Rizki 2026-11-18 ("lanjutkan pengerjaan dan commit langsung") |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/AssetManagement/**`; laporan ini dan baris status roadmap/traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `ae054ee8` (branch `rizkiG`) |
| Tanggal | 18 November 2026 |
| Status | 🟡 SEBAGIAN 18 November 2026 — keempat kriteria terpetakan ke source dan teruji: QBE Strict `PASS`, build Rizki berhasil, runtime **33/33**; satu perbaikan pesan sesudah uji (pemisah ribuan pada `061`) menunggu build ulang |

**Backend Governance Preflight**

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AssetManagement` / `Register` (+ `Valuation`: satu helper statis) |
| Pemilik / prefix registry | `AssetManagement / Register Aset` dan `Nilai Aset`, prefix `Ast`, `ACTIVE` |
| Keberlakuan | `NEW CODE` (+ `TOUCHED`: `AssetBookService` milik `BE-AST-015`, `AssetRegisterSettingReader` dan `AssetAttachmentAccessService` milik `BE-AST-011`) |
| QBE yang berlaku | `QBE-MOD-001`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001` (isi baris dan rupiah tidak ke log), `QBE-CODE-001`/`002`/`003` (nomor `AST-IMP` dan nomor inventaris lewat deret bersama), `QBE-VAL-001`, `QBE-TXN-001` (penerapan satu transaksi), `QBE-DTO-001`, `QBE-DEL-001` (batch dibatalkan/ditolak, tidak dihapus) |
| Tidak berlaku | `QBE-ENT-*`, `QBE-CFG-*`, `QBE-NAM-*`, `QBE-DB-*` — `AstImportBatch`/`AstImportBatchRow` sudah ada sejak `BE-AST-003`/`004`; tidak ada migration |
| Arketipe transaksi | Aggregate ber-lifecycle (`Tervalidasi`/`GagalValidasi` → `DisetujuiBagianAset` → `Diterapkan`; `Ditolak`, `Dibatalkan`) |

---

## 1. Masalah yang diperbaiki

Aset yang sudah ada sebelum aplikasi dipakai (data lama) hanya bisa dicatat satu per satu lewat draft dan
persetujuan, tanpa akumulasi penyusutan. Untuk 2.400 aset RS hal itu tidak mungkin, dan saldo awal nilai buku tidak
punya jalan masuk. `AST-RULE-007` meminta impor Excel per tanggal awal, semua atau tidak sama sekali, dengan dua
persetujuan (`AST-DEC-050`) dan tanpa kiriman ke Akuntansi (`AST-DEC-023`).

---

## 2. Proses bisnis

| Langkah | Pelaku | Yang terjadi |
|---|---|---|
| 0 | Kepala Bagian Aset | Mengisi **Tanggal Awal** di Pengaturan Aset. Tanpa itu unggahan ditolak `068` |
| 1 | Petugas Aset | **Unduh templat**: lembar *Data Aset* (19 kolom), *Petunjuk*, dan daftar kode kelompok, kepemilikan, lokasi |
| 2 | Petugas Aset | **Unggah** `.xlsx` → seluruh baris divalidasi sekaligus. Nol baris salah → `Tervalidasi`; satu baris salah → `GagalValidasi` beserta kesalahan per nomor baris Excel, **nol** aset |
| 3 | Kepala Bagian Aset (bukan pengunggah) | **Setujui data aset** → `DisetujuiBagianAset`, atau **Tolak** beralasan |
| 4 | Kepala Akuntansi (bukan pengunggah) | **Setujui angka** → seluruh baris dicek ulang terhadap master saat itu, lalu diterapkan dalam **satu transaksi**: tiap baris jadi aset `Terdaftar` + `Ditempatkan` (penempatan sumber `IMPOR`), nomor inventaris terbit, buku nilai **saldo awal** (akumulasi sampai tanggal awal), linimasa `Diimpor Saldo Awal`. **Tanpa** kotak keluar Akuntansi. Atau tolak beralasan lewat endpoint yang sama |
| 5 | Pengunggah | **Batalkan** sebelum diterapkan |

Contoh dari uji: berkas 2.400 baris dengan kode kelompok `XXX` di baris Excel 1501 → `AST-IMP-000001` Gagal Validasi,
satu kesalahan, nol aset. Berkas 30 baris (25 lemari, 5 monitor pasien) → Kepala Bagian Aset → Akuntansi → 30 aset
`AST-MBL-…`/`AST-ALKMED-…` di Ruang Uji U9KOL dengan buku saldo awal; total akumulasi Rp 237.500.000 sama dengan
berkas.

**Jalur tidak normal**

| Kondisi | Hasil |
|---|---|
| Tanggal awal kosong | 409 `AST-VAL-REG-068` |
| Bukan `.xlsx`, lembar/kepala kolom tidak cocok, berkas kosong | 400 `AST-VAL-REG-060` |
| Lebih dari `AssetManagement:Import:MaxRows` (bawaan 5.000) baris | 400 `AST-VAL-REG-061` |
| Baris: kelompok/kepemilikan tak dikenal (`062`), unit/lokasi/PJ salah (`063`), akumulasi > harga − residu (`064`), siap pakai sesudah tanggal awal (`065`), merek+seri kembar di berkas atau dengan aset (`066`), atribut medis kosong (`007`), harga ≤ 0 (`006`), siap pakai < perolehan (`005`), isian wajib/tanggal/angka tidak sah (`AST-REG-FIELD`) | Baris galat; batch `GagalValidasi` |
| Pengunggah menyetujui (tahap mana pun) | 403 `AST-VAL-REG-067` |
| Data master berubah di antara dua persetujuan | 409 `AST-IMP-REVALIDATE` "Baris n: …"; batch tetap `DisetujuiBagianAset`, nol aset |
| `/reject` sesudah disetujui Bagian Aset | 409 `AST-IMP-ROUTE` (tolak lewat `/approve-value`) |
| Membatalkan batch orang lain | 403 `AST-IMP-OWNER` |
| Tanpa `AssetRegister : ViewAmount` | Total dan rupiah per baris tidak dikirim (`amountsVisible=false`) |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` backend; `rules/backend/` (engineering contract, registry, role-access, transaction standard); kontrak
2.15, state §6, validasi §6, kamus §3.9; `AssetBookService` (`CreateOpeningBalanceAsync`, `BuildBookAsync`),
`AssetPlacementService.ApplyPlacementAsync`, `AssetRecordService.Approval`, `NumberSeriesAllocator`, pola ClosedXML
di Finance (`FinanceReceiptService`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Register/Services/AssetImportWorkbook.cs` | Baru — definisi 19 kolom, pembuatan templat (Data Aset, Petunjuk, 3 lembar kode), parsing ClosedXML (tanggal sel/teks, angka sel/teks gaya Indonesia) |
| `Register/Services/AssetImportService.cs` | Baru — unggah, setujui data aset, setujui/tolak angka + penerapan satu transaksi, tolak, batalkan, templat; opsi `AssetManagement:Import:MaxRows` |
| `Register/Services/AssetImportService.Validation.cs` | Baru — validasi seluruh baris dengan master dimuat sekali per unggahan |
| `Register/Services/AssetImportService.Query.cs` | Baru — metadata, summary, daftar (`awaitingMyDecision`), rincian, baris berhalaman |
| `Register/Controllers/AssetImportsController.cs` | Baru — 11 endpoint, resource `AssetImport` (Read/Upload/ApproveAsset/ApproveValue) |
| `Register/DTOs/AssetImportDtos.cs` | Baru |
| `Valuation/Services/AssetBookService.cs` | + `CheckOpeningBalance` statis (aturan `064`/`NLI-040` yang sama dengan `CreateOpeningBalanceAsync`, tanpa akses DB) |
| `Register/Services/AssetRegisterSettingReader.cs` | + `CutoverDate` |
| `Register/Services/AssetAttachmentAccessService.cs` | + pemilik `AstImportBatch` (unduh berkas asli dengan `AssetImport : Read`) |
| `Register/Services/AssetManagementException.cs` | + kode `060..068`, `AST-IMP-*` dan pesannya |
| `Register/Services/AssetRecordService.cs` | + label status batch |
| `AssetManagementServiceCollectionExtensions.cs` | + opsi impor, `AssetImportService` |

### 3.3 Dampak kontrak API, database, dan keamanan

**Delta terhadap `AST-REG-1.0.0`:**

| No | Delta | Alasan |
|---|---|---|
| 1 | Kolom templat ditetapkan pengembang (19 kolom, bagian 4) | Blueprint tidak merinci kolom; diturunkan dari `AST-RULE-007` dan validasi `062..066`. Kunci: kelompok = **Kode Inventaris** (unik), kepemilikan = `OwnershipCode`, unit = `UnitCode`, lokasi = `LocationCode`, PJ = `EmployeeNumber` **atau** `EmployeeCode` |
| 2 | `GET /{id}/rows?onlyErrors=` berhalaman | Rincian 2.400 baris dalam satu respons terlalu besar; `GET /{id}` memuat ringkasan |
| 3 | `GET filters/metadata`, `GET summary` (termasuk tanggal awal dan batas baris) | Pola layar modul |
| 4 | Penerapan memvalidasi ulang semua baris | Master dapat berubah di antara dua persetujuan; kegagalan = 409 tanpa aset (`AT-REG-45`) |
| 5 | Atribut medis (`007`), tanggal (`005`), harga (`006`) ikut divalidasi saat impor | Aturan yang sama dengan pencatatan biasa; tidak ada aturan baru |
| 6 | Rupiah batch/baris hanya untuk `AssetRegister : ViewAmount` | Konsisten dengan `AST-DEC-025` |
| 7 | `ApprovedBy` aset impor = penyetuju Akuntansi; `SubmittedBy` = pengunggah | Kolom aset menuntut pelaku |
| 8 | Linimasa: satu kejadian `Diimpor Saldo Awal` (berisi unit · lokasi), tanpa kejadian `Ditempatkan` terpisah | Penempatan dan pendaftaran terjadi sekaligus |

**Database:** tanpa migration. Data uji: bagian 5.4.

**Keamanan:** 11/11 pasangan `[AccessAction]`/`[AccessPermission]` cocok; tanpa hardcode peran; pemisahan
pengunggah ≠ penyetuju dan pemilik pembatalan diperiksa di service.

---

## 4. Dokumentasi endpoint

#### Corporate / Asset Management / Register / Asset Import (`…/register/imports`)

| Method | Path | Hak | Keterangan |
|---|---|---|---|
| `GET` | `/filters/metadata`, `/summary` | `AssetImport : Read` | — |
| `GET` | `/template` | `AssetImport : Read` | `templat-impor-aset.xlsx` |
| `GET` | `/`, `/{id}` | `AssetImport : Read` | `status` (berulang), `awaitingMyDecision`, `search` |
| `GET` | `/{id}/rows` | `AssetImport : Read` | `onlyErrors`, `pageNumber`, `pageSize` (≤ 100); `errors[]` berformat "KODE pesan" |
| `POST` | `/` | `AssetImport : Upload` | multipart `file` |
| `POST` | `/{id}/approve-asset` | `AssetImport : ApproveAsset` | `{ reason, rowVersion }` |
| `POST` | `/{id}/approve-value` | `AssetImport : ApproveValue` | `{ accept, reason, rowVersion }`; tolak wajib alasan |
| `POST` | `/{id}/reject` | `AssetImport : ApproveAsset` | `{ reason, rowVersion }` — hanya `Tervalidasi` |
| `POST` | `/{id}/cancel` | `AssetImport : Upload` | Pengunggah saja |

Kolom templat (lembar *Data Aset*, baris 1 kepala, data mulai baris 2): Kode Kelompok\*, Kode Kepemilikan\*, Nama Aset\*,
Keterangan, Merek, Tipe/Model, Nomor Seri, Produsen, Nomor Izin Edar, Tahun Pembuatan, Distributor, Pemasok, Tanggal
Perolehan\*, Tanggal Siap Pakai\*, Harga Perolehan\*, Akumulasi Penyusutan\*, Kode Unit\*, Kode Lokasi\*, NIP Penanggung
Jawab\*.

---

## 5. Verifikasi

### 5.1 Statis dan build

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| QBE `-Mode Strict` (WorkingTree) | `Files evaluated: 12`, `VIOLATION: 0`, `REVIEW: 0` | `PASS` |
| Atribut akses | 11/11 | `PASS` |
| Baris komentar baru | 0 | `PASS` |
| `dotnet build -p:RunAnalyzers=false` oleh Rizki | DLL 2026-11-18 15:02:51 memuat class impor; backend dijalankan ulang 15:03:30 | `PASS` |
| Registrasi hak | `AssetImport` 4 aksi di `role-access/resources/structured` | `PASS` |

### 5.2 Runtime (HTTPS 7184)

Skrip dan bukti: `QuilvianSystemFrontendDev/test-with-agy/asset-management/be-ast-012-20261118/`
(`ast012-runtime.mjs` → `ast012-runtime.json`; berkas uji dibuat `buat-berkas.fsx` dari templat yang diunduh lewat API;
pemeriksaan DB baca-saja `cek-db.fsx` dengan `default_transaction_read_only=on`). Aktor: superadmin (pengunggah),
akun penyetuju uji (diberi `AssetImport` sementara), LOKET. Tanggal awal diisi 2026-11-30 lewat Pengaturan Aset lalu
dikosongkan kembali; policy penyetuju dipulihkan sama persis.

| Skenario | Hasil |
|---|---|
| `068` tanpa tanggal awal; `060` CSV dan kepala kolom salah; `061` 5.001 baris (0,9 dtk) | `PASS` |
| `AT-REG-43` — 2.400 baris, 1 salah di baris Excel 1501 → `GagalValidasi`, `errorCount` 1, galat `062`; DB: 0 aset, 0 buku; validasi 2,6 dtk | `PASS` |
| Batch `GagalValidasi` tidak dapat disetujui (409) | `PASS` |
| Galat per baris: 10 baris salah tepat dengan kodenya (`062` ×2, `063` ×2, `064`, `065`, `066`, `007`, `FIELD`, `006`), 2 baris benar tanpa galat | `PASS` |
| Unggah 30 baris → `Tervalidasi`, total harga Rp 362.500.000, akumulasi Rp 237.500.000 | `PASS` |
| `AT-REG-44` — pengunggah menyetujui tahap 1 dan tahap 2 | `PASS` (403 `067` keduanya) |
| `/reject` sesudah tahap 1 → 409 `AST-IMP-ROUTE` | `PASS` |
| `AT-REG-10` (skala 30) — diterapkan, 30 aset; DB: 30 aset Terdaftar+Ditempatkan+SaldoAwal, 30 buku, 30 entri `SaldoAwal`, akumulasi Rp 237.500.000, 30 penempatan `IMPOR`, 30 linimasa, **0 kotak keluar** | `PASS` |
| Aset hasil impor: `AST-ALKMED-000005`, buku akumulasi Rp 40.000.000, perolehan Saldo Awal, Ruang Uji U9KOL A, PJ terisi | `PASS` |
| `AT-REG-45` — kelompok `TI` dinonaktifkan sesudah tahap 1 → 409 `AST-IMP-REVALIDATE` "Baris 5: …", batch tetap `DisetujuiBagianAset`, 0 aset; kelompok dipulihkan → diterapkan 4 aset | `PASS` |
| Tolak Bagian Aset dan tolak Akuntansi (tanpa alasan 400, dengan alasan → `Ditolak`, 0 aset) | `PASS` |
| Batal: bukan pengunggah 403 `AST-IMP-OWNER`, pengunggah → `Dibatalkan`, ulang 409 | `PASS` |
| Berkas asli lewat `/attachments/{id}`: penyetuju 200 xlsx, LOKET 403; LOKET `GET /imports` 403 | `PASS` |
| Tanpa `ViewAmount`: `amountsVisible=false`, total dan rupiah baris `null` | `PASS` |

**Rekap: 33/33.**

### 5.3 Temuan

| Temuan | Tindak |
|---|---|
| Pesan `061` menampilkan "5,000" (format Inggris) | Diperbaiki (`CultureInfo id-ID`) sesudah uji; **menunggu build ulang** untuk dibuktikan |
| Penerapan ≈ 189 ms per baris (30 baris 5,7 dtk) → 2.400 baris diperkirakan ±7,5 menit dalam satu request. Penyebab: satu alokasi nomor (koneksi dan transaksi sendiri) dan ±4 kueri buku nilai per baris ke DB jarak jauh | **Risiko go-live**: proxy produksi dengan batas waktu default (mis. 60 dtk) akan memutus request; transaksi tetap berjalan di server tetapi layar menerima galat. Saran: unggah per unit (≤ 300 baris) atau naikkan batas waktu proxy untuk rute ini; optimasi penomoran massal butuh perubahan `NumberSeriesAllocator` milik Platform |
| Buku saldo awal aset lama mendapat umur manfaat 0 bila kebijakan penyusutan kelompok baru berlaku sesudah periode mulai susut (kebijakan dev berlaku Nov 2026; aset siap pakai 2019) | Perilaku `BE-AST-015` (`GetPolicyAtAsync` pada periode mulai susut). Diserahkan ke sub-modul nilai (`BE-AST-027`) untuk diputuskan: kebijakan pada periode mulai susut atau pada tanggal awal |
| `AT-REG-10` diuji pada 30 baris, bukan 2.400 | Skala penuh menambah 2.400 aset permanen ke DB dev; jalur 2.400 baris sudah teruji untuk validasi (`AT-REG-43`) |

### 5.4 Data uji di `QuilvianNewDevRizki`

Batch `AST-IMP-000001`..`000009`: Gagal Validasi 000001, 000002, 000008; Diterapkan 000003 (30 aset), 000004 (4 aset),
000009 (5 aset, uji layar); Ditolak 000005, 000006; Dibatalkan 000007; **39 aset impor** (30 + 4 + 5 dari uji layar) di unit Instalasi Rawat Inap; tanggal
awal kembali kosong; kelompok `TI` aktif kembali; policy penyetuju dipulihkan.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Source | Bukti | Status |
|---|---|---|---|
| 1. Satu baris salah → seluruh berkas `GagalValidasi` | `UploadAsync`, `ValidateAsync` | `AT-REG-43`, galat per baris | ✅ |
| 2. Dua persetujuan, bukan pengunggah | `ApproveAssetAsync`, `ApproveValueAsync`, `EnsureNotUploader` | `AT-REG-44` | ✅ |
| 3. Satu transaksi: aset `Terdaftar` + `Ditempatkan` + buku saldo awal, tanpa kotak keluar | `ApplyAsync` + `CreateOpeningBalanceAsync` | `AT-REG-10` (30), `AT-REG-45`, DB | ✅ |
| 4. Batas 5.000 baris dari konfigurasi | `AssetImportOptions.MaxRows` | `061` | ✅ |

| DoD | Status |
|---|---|
| `V-BE` QBE, build, registrasi hak, runtime | Terpenuhi; perbaikan pesan `061` menunggu build ulang |
| Laporan, roadmap, traceability | Ditulis |
| Commit | Belum |

---

## 7. Catatan penutup

- **Task berikutnya:** `BE-AST-013` (dashboard, laporan register, History & Audit), pasangan `FE-AST-010`.
- `git status --short` backend: 6 berkas diubah, 6 berkas baru, + `Storage/uploads/asset-management/` (tidak di-commit);
  dua berkas `docs/module-blueprints/igd/...` yang belum terlacak bukan milik task ini.
