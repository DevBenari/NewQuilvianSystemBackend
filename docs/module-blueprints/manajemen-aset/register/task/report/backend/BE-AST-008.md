# Laporan Perubahan Backend — `BE-AST-008`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-AST-008` |
| Judul | Label QR dan pencarian nomor |
| Slice | `REG-S2` — Register aset: pencatatan, label, penempatan, kondisi, dokumen, pemanfaatan, impor, dashboard |
| Roadmap | `register/roadmap/backend-roadmap.md` — kartu `BE-AST-008` |
| Trace | `CAP-03.4`; `AST-DEC-010`, `AST-RULE-004`, `AST-FE-003`; `INT-REG-11`; API 2.8 (`/labels`, `/by-number`); `permission-audit-matrix` (pengecualian logger `/labels`); `validation-matrix` `AST-VAL-REG-016`; `AT-REG-13` |
| Contract version | `AST-REG-1.0.0` `approved` 2026-11-12 |
| Dependency | `BE-AST-007` ✅ |
| Klasifikasi | `MEDIUM` — 1 service baru, 2 endpoint, tanpa entity, tanpa migration |
| Task mode | `BACKEND`, diminta Rizki 2026-11-17 ("lanjutkan tahap selanjutnya") |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/AssetManagement/**`; laporan ini dan baris status roadmap/traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `4e5a85c8` (branch `rizkiG`) |
| Tanggal | 17 November 2026 |
| Status | ✅ SELESAI 17 November 2026 — kedua kriteria terpetakan ke source; QBE Strict `PASS`; build Rizki berhasil; uji runtime 13/13 |

**Backend Governance Preflight**

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AssetManagement` / `Register` |
| Pemilik / prefix registry | `AssetManagement / Register Aset`, prefix `Ast`, `ACTIVE` |
| Keberlakuan | `NEW CODE` |
| QBE yang berlaku | `QBE-MOD-001`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-DTO-001`, `QBE-VAL-001` |
| QBE yang tidak berlaku | `QBE-ENT-*`, `QBE-CFG-*`, `QBE-NAM-*`, `QBE-CODE-*`, `QBE-DB-*` — tidak ada entity, configuration, nomor bisnis, maupun migration baru |

---

## 1. Masalah yang diperbaiki

Sesudah `BE-AST-007`, aset sudah bernomor inventaris, tetapi:

- **Belum ada label fisik.** `AST-RULE-004` mewajibkan label QR berisi nomor inventaris yang dicetak dari sistem
  dalam bentuk PDF. Contoh dari wawancara: petugas mencetak 40 label kursi sekaligus.
- **Hasil pindai QR belum bisa dibuka.** Kamera hanya membaca nomor (misalnya `AST-MBL-000025`), sedangkan API
  rincian meminta id internal. Layar `FE-AST-004` sementara memakai pencarian daftar, sehingga aset unit lain
  tampil sebagai "tidak ditemukan" alih-alih 403 yang jelas.

---

## 2. Proses bisnis

**Cetak label (`SCR-AST-REG-13`, layar `FE-AST-005`)**

| Langkah | Pelaku | Yang terjadi |
|---|---|---|
| 1 | Petugas Aset | Memilih 1–200 aset `Terdaftar`, misalnya 40 kursi yang baru disetujui |
| 2 | Sistem | Memeriksa setiap aset: ada dan milik badan hukum aktif, berada di unit petugas (atau petugas memegang `ReadAllUnits`), dan berstatus `Terdaftar` |
| 3 | Sistem | Mengurutkan aset menurut nomor, lalu membuat PDF A4: 21 stiker per halaman (3 kolom × 7 baris, 63,5 × 38,1 mm). Contoh: 40 aset menghasilkan 2 halaman (21 + 19) |
| 4 | Petugas Aset | Mencetak PDF ke lembar stiker lalu menempelkannya |

Isi satu stiker:

```text
AST-MBL-000025                    2026     ← nomor (tebal) dan tahun perolehan, satu baris
[QR 24 mm]   Kursi Tunggu UJI-008-YWW1     ← nama aset, paling banyak 3 baris
             PT Metropolitan Medical Centre ← badan hukum pemilik
```

QR hanya berisi teks nomor inventaris (`INT-REG-11`). Nama, harga, lokasi, dan data lain tidak masuk ke QR.

**Jalur tidak normal**

| Kondisi | Hasil | Pesan |
|---|---|---|
| 0 atau lebih dari 200 aset | 400 `AST-REG-FIELD` | "Pilih 1 sampai 200 aset untuk dicetak labelnya." |
| Salah satu aset tidak ada / terhapus / milik badan hukum lain | 404 `AST-REG-NOT-FOUND` | "Aset tidak ditemukan atau sudah dihapus." |
| Salah satu aset di luar unit petugas | 403 `AST-VAL-REG-016` | "Aset ini bukan milik unit Anda." |
| Salah satu aset belum `Terdaftar` (misalnya Draft) | 409 `AST-REG-STATUS` | "Aset berstatus Draft, sehingga tidak dapat dicetak labelnya." |

Satu aset yang bermasalah menggagalkan seluruh permintaan, sehingga petugas tidak mencetak lembar yang
diam-diam kurang satu label.

**Cari aset dari hasil pindai (`/by-number`)**

| Langkah | Pelaku | Yang terjadi |
|---|---|---|
| 1 | Petugas lapangan | Memindai QR → mendapat `AST-MBL-000025` |
| 2 | Sistem | Merapikan nomor (hapus spasi, huruf besar; `" ast-mbl-000025 "` diperlakukan sama) |
| 3 | Sistem | Mencari nomor itu dalam badan hukum aktif, lalu memakai jalur rincian yang sama dengan `GET /{id}` — termasuk batas unit |
| 4 | Petugas lapangan | Melihat rincian aset beserta unit, ruangan, penanggung jawab, dan status penempatan |

Jalur tidak normal: nomor kosong atau lebih dari 30 karakter → 400 `AST-REG-FIELD`; nomor tidak dikenal → 404
(dipakai `inventarisasi` untuk tombol "Catat sebagai temuan"); aset unit lain → 403 `AST-VAL-REG-016`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` backend; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`, `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`;
`rules/backend/` (`TASK_RULES`, `API_RULES`, `REVIEW_RULES`, `REPORT_TEMPLATE`); kontrak `api-contract.md` 2.8,
`permission-audit-matrix.md`, `validation-matrix.md`, `integration-contract.md`, `acceptance-test-matrix.md`;
`00-interview-decisions.md` (`AST-DEC-010`, `AST-RULE-004`, `AST-OQ-016`); `02-backend-architecture.md`;
`AssetsController`, `AssetMasterControllerBase`, `AssetRecordService.Query.cs`, `AssetActorResolver`,
`AssetEntityResolver`, `AstAssetConfiguration` (indeks unik nomor); pola QRCoder di
`EmployeeDocumentIssuanceService.cs` dan QuestPDF di `CanceledInvoiceReportDocuments.cs`; dokumentasi XML paket
QuestPDF 2026.7.1 dan QRCoder 1.8.0 untuk memastikan tanda tangan API.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/AssetManagement/Register/Services/AssetLabelDocumentService.cs` | Baru — validasi 1–200 aset, badan hukum, batas unit, status `Terdaftar`; QR `PngByteQRCode` (ECC M) berisi nomor saja; PDF QuestPDF A4 3 × 7 |
| `Areas/Corporate/AssetManagement/Register/Services/AssetRecordService.Query.cs` | `GetAssetByNumberAsync`: rapikan nomor, cari id dalam badan hukum aktif, lalu `GetAssetAsync` (batas unit, nama, label status) |
| `Areas/Corporate/AssetManagement/Register/Controllers/AssetsController.cs` | `GET by-number/{inventoryNumber}` dan `POST labels` (`AssetRegister : Read`); `labels` mengembalikan `application/pdf`, galat tetap `ApiResponse` |
| `Areas/Corporate/AssetManagement/Register/DTOs/AssetRecordDtos.cs` | `AssetLabelRequest` (`AssetIds`) |
| `Areas/Corporate/AssetManagement/Register/Services/AssetManagementException.cs` | Pesan `LabelBatchSize`, `InventoryNumberRequired` |
| `Areas/Corporate/AssetManagement/AssetManagementServiceCollectionExtensions.cs` | Registrasi `AssetLabelDocumentService` |

Total +59/−1 baris pada berkas lama + 1 berkas baru; nol baris komentar.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Dua endpoint 2.8 yang berstatus "Rencana" kini tersedia. Delta: `AssetLabelRequest` belum dirinci kontrak, diisi `AssetIds` (1–200, sama dengan batas persetujuan massal); kode galat memakai `AST-REG-FIELD`/`AST-REG-STATUS`/`AST-REG-NOT-FOUND` yang sudah ada |
| Database | `NOT APPLICABLE` — hanya membaca `AstAsset`; tidak ada entity, configuration, atau migration |
| Keamanan/Auth | `[AccessPermission("AssetRegister", "Read")]` di kedua endpoint; batas unit `AST-VAL-REG-016` ditegakkan per aset; `/labels` tidak menulis log audit (`LoggerService`) sesuai pengecualian bernama `permission-audit-matrix`. QR tidak memuat data selain nomor |

---

## 4. Dokumentasi endpoint

#### Corporate / Asset Management / Register / Asset

Base URL: `api/v1/corporate/asset-management/register/assets`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/by-number/{inventoryNumber}` | Rincian aset dari hasil pindai QR (`AssetDetailResponse`) | `AssetRegister : Read` |
| `POST` | `/labels` | PDF label QR untuk 1–200 aset terpilih (`application/pdf`, nama berkas `label-aset-<waktu>.pdf`) | `AssetRegister : Read` |

Contoh permintaan `POST /labels`:

```json
{ "assetIds": ["5f90eaba-a4b3-4d6f-b5c9-82ef0b4ec3a3", "…"] }
```

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE Strict `WorkingTree` (`tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict`) | 6 berkas, `VIOLATION: 0`, `REVIEW: 0` — dijalankan dua kali (sebelum dan sesudah perbaikan tata letak) | `PASS` | Keluaran perintah |
| Grep komentar pada diff dan berkas baru | 0 baris | `PASS` | — |
| `dotnet build` | Dijalankan Rizki dan berhasil dua kali; DLL terakhir 2026-11-17 10:34:00, sesudah suntingan terakhir 10:23:24. Swagger memuat kedua route | `PASS` | `bin/Debug/net9.0`, `/swagger/corporate/swagger.json` |
| Uji runtime putaran 1 (DLL 10:12) | 12/14 — `/labels` 40 aset → 500 `DocumentLayoutException` | `NEW ERROR` → diperbaiki | Log `quilvian-backend-20261117.json` |
| Uji runtime putaran 2 (DLL 10:34) | **13/13** | `PASS` | `test-with-agy/asset-management/be-ast-008-20261117/ast008-runtime.json`, `label-aset.pdf` |

Rincian putaran 2 (superadmin uji; penyetuju = akun "Uji Penyetuju Aset" dengan izin dicabut sementara):

| ID | Skenario | Hasil |
|---|---|---|
| BN-01 | `GET /by-number/AST-ALKMED-000001` | 200; id sama dengan daftar; bidang unit, ruangan, penanggung jawab ada (aset belum ditempatkan, jadi kosong dan `Belum Ditempatkan`) |
| BN-02 | Nomor huruf kecil berspasi | 200, aset yang sama |
| BN-03 | `AST-XXX-999999` | 404 `AST-REG-NOT-FOUND` |
| BN-04 | Nomor 31 karakter | 400 `AST-REG-FIELD` |
| BN-05 | Penyetuju tanpa `ReadAllUnits` | 403 `AST-VAL-REG-016` "Aset ini bukan milik unit Anda." |
| LB-01 | Daftar kosong | 400 `AST-REG-FIELD` + pesan label |
| LB-02 | 201 id | 400 `AST-REG-FIELD` |
| LB-03 | Satu id tidak ada | 404 `AST-REG-NOT-FOUND` |
| LB-04 | Satu aset Draft | 409 `AST-REG-STATUS` "Aset berstatus Draft, sehingga tidak dapat dicetak labelnya." |
| LB-05 | **`AT-REG-13`** 40 aset | 200 `application/pdf`, `attachment; filename=label-aset-20261117104024.pdf`, 167.337 byte, **2 halaman** |
| LB-06 | **`AT-REG-13`** isi QR | 40 gambar QR untuk 40 aset; **39 terbaca persis** sama dengan nomornya; 1 (`AST-MBL-000025`) matriksnya **identik modul per modul** dengan QR referensi yang dibuat dari nomor itu saja (lihat catatan) |
| LB-08 | Label aset unit lain | 403 `AST-VAL-REG-016` |
| PM-01 | Tanpa `AssetRegister : Read` | `/by-number` 403, `/labels` 403 |

Izin penyetuju dipulihkan ke Read/Approve/ViewAmount/ReadAllUnits di akhir (200).

**Perbaikan di antara dua putaran.** Putaran 1 gagal karena tiga kolom stiker (3 × 63,5 mm = 540 pt) lebih
lebar 0,28 pt dari area cetak (210 − 2 × 9,75 mm = 539,72 pt). Kesalahan ini direproduksi di luar server dengan
`dotnet fsi` yang memuat QuestPDF dari `bin` (`QuestPDF.Settings.EnableDebugging`), lalu diperbaiki: margin
samping 9,5 mm; nomor dan tahun dipindah ke satu baris selebar stiker karena pada percobaan pertama nomor
`AST-ALKMED-…` patah dan tahun turun ke baris bawah; QR 24 mm; latar halaman putih. Hasilnya dirender ke gambar
dan diperiksa visual (`pratinjau-tata-letak-hal1.png`): 21 stiker per halaman, nomor dan tahun sebaris.

**Cara QR dibaca.** QR diambil langsung dari gambar di dalam PDF (DeviceGray, FlateDecode, 290 × 290 px) dan
dibaca dengan `@zxing/library` 0.19.3 yang sudah ada di `node_modules` frontend (tanpa memasang paket). Untuk
`AST-MBL-000025`, zxing-js gagal (`FormatException`) pada skala, tepi, dan binarizer apa pun. Matriks QR itu
dibandingkan dengan keluaran encoder independen `qrcode` (paket frontend): **0 modul berbeda**, dan zxing-js
juga gagal membaca keluaran encoder independen itu. Jadi kodenya benar dan yang gagal adalah pembaca zxing-js
untuk pola tersebut (`qr-compare.mjs`).

Uji manual: `NOT APPLICABLE` (endpoint diuji langsung; layar menyusul `FE-AST-005`).

**Tidak dijalankan:**
- Pemindaian label tercetak dengan kamera HP. Belum ada layar pindai (`FE-AST-005`/`FE-AST-016`) dan printer
  label. Diuji di UAT.
- Pemeriksaan teks PDF otomatis (tahun di samping nomor). Teks PDF tersimpan sebagai glyph font ter-subset;
  buktinya source (`AcquisitionDate.Year` di baris yang sama dengan nomor) dan render visual dari tata letak
  yang sama.

`AUTOMATED TEST: SKIPPED (opsional) — kebijakan tanpa unit test`.

**Data yang ditulis ke `QuilvianNewDevRizki`** (atas pilihan Rizki untuk `AT-REG-13`): 29 aset "Kursi Tunggu
UJI-008-YWW1" dibuat lewat API (1 draft × 29 unit → diajukan superadmin → disetujui penyetuju), bernomor
`AST-MBL-000008` sampai `AST-MBL-000036`, `Terdaftar`. DB dev kini berisi 40 aset `Terdaftar`. Policy penyetuju
diubah sementara lalu dipulihkan.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. PDF label untuk aset terpilih; QR berisi nomor inventaris saja; tahun perolehan tercetak di samping nomor | Terpenuhi | `AssetLabelDocumentService`; LB-05 (40 label, 2 halaman), LB-06 (40/40 QR berisi nomor saja), render visual baris nomor + tahun; LB-01..04, LB-08 jalur galat |
| 2. `/by-number` mengembalikan aset beserta lokasi tercatat, menghormati batas unit | Terpenuhi | `GetAssetByNumberAsync` → `GetAssetAsync`; BN-01..05. Lokasi teramati kosong karena belum ada penempatan (`BE-AST-009`); bidangnya ada dan diisi dari penempatan berjalan |
| `DoD-BE` | Terpenuhi | QBE Strict `PASS`, build Rizki, runtime 13/13, laporan, roadmap, traceability |

Rekap: **2 dari 2 kriteria terpenuhi** → `✅`.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Keluaran `dotnet build` tidak dilihat agen; Rizki melaporkan build berhasil, dan DLL serta Swagger membuktikan kode baru termuat |
| Masalah yang diketahui | (1) Ukuran stiker `AST-OQ-016` masih default "stiker umum" A4 3 × 7; bila Bagian Aset memakai printer rol, cukup ubah konstanta ukuran di `AssetLabelDocumentService`. (2) `@zxing/library` 0.19.3 gagal membaca satu pola QR yang sah; bila layar pindai `FE-AST-016` memakai pustaka ini, uji pola tersebut lebih dulu atau pakai pembaca lain |
| Risiko tersisa | Petugas dengan `AssetRegister : Read` dapat mencetak label aset unitnya sendiri; ini sesuai kontrak (label hanya membaca) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE`; satu kegagalan runtime (putaran 1) diperbaiki di dalam task |
| Status Git | `M` `AssetManagementServiceCollectionExtensions.cs`, `AssetsController.cs`, `AssetRecordDtos.cs`, `AssetManagementException.cs`, `AssetRecordService.Query.cs`; `??` `AssetLabelDocumentService.cs`; dokumen laporan/roadmap/traceability. Belum di-commit |
| Langkah berikutnya | Pasangan frontend: layar pindai `FE-AST-004` beralih ke `/by-number`, lalu `FE-AST-005` (cetak label) |
