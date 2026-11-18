# Laporan Perubahan Backend — `BE-AST-011`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-AST-011` |
| Judul | Dokumen kepemilikan, pengamanan, pemanfaatan |
| Slice | `REG-S2` — Register aset: pencatatan, label, penempatan, kondisi, dokumen, pemanfaatan, impor, dashboard |
| Roadmap | `register/roadmap/backend-roadmap.md` — kartu `BE-AST-011` |
| Trace | `CAP-03.5`, `CAP-08.1`–`08.5`; `AST-DEC-021`, `AST-DEC-029`, `AST-RULE-013`, `AST-DES-017`, `AST-DES-018`; API 2.12–2.14 (+ `GET /assets/idle` di 2.8); `state-transition-matrix` §5 dan §7; `validation-matrix` (`AST-VAL-REG-040..045`, `070..072`, `080`); `permission-audit-matrix` (`AssetDocument`, `AssetUtilization`); `AT-REG-08`, `09`, `14`, `15`, `41`, `42` |
| Contract version | `AST-REG-1.0.0` `approved` 2026-11-12 |
| Dependency | `BE-AST-007` ✅; memakai `BE-AST-005` (lampiran, penomoran, kunci proses, linimasa, aktor), `BE-AST-015` (batas rupiah `UtilizationDirectorThreshold`), `BE-AST-010` (kondisi) |
| Klasifikasi | `HEAVY` — 1 aggregate ber-lifecycle (8 status), 1 sub-resource dokumen ber-CRUD, `BackgroundService`, 3 controller (23 endpoint) + 1 endpoint di controller aset; tanpa entity atau migration baru |
| Task mode | `BACKEND`, diminta Rizki 2026-11-18 ("lanjutkan BE-AST-011 lalu FE-AST-008 baru proses build be dan fe lalu pengujian") |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/AssetManagement/**`; laporan ini dan baris status roadmap/traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `8a326c55` (branch `rizkiG`) |
| Tanggal | 18 November 2026 |
| Status | 🟡 SEBAGIAN 18 November 2026 — kelima kriteria terpetakan ke source; QBE Strict `PASS`; build Rizki berhasil; runtime 60/60 + 13/15 (2 kegagalan = harapan skrip keliru, bagian 5); pekerja harian `Berjalan`→`Berakhir` terbukti sesudah restart (4/4); **belum terbukti**: pekerja harian `Disetujui`→`Berjalan` (`AST-PMF-000008` mulai 19 Nov) dan `AT-REG-15` kasus positif (butuh aset Tidak Digunakan lebih dari N hari) |

**Backend Governance Preflight**

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AssetManagement` / `Register` |
| Pemilik / prefix registry | `AssetManagement / Register Aset`, prefix `Ast`, `ACTIVE` |
| Keberlakuan | `NEW CODE` (+ `TOUCHED` satu baris di `AssetConditionService` milik `BE-AST-010`, bagian 3.3) |
| QBE yang berlaku | `QBE-MOD-001`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001` (nomor dokumen dan nama mitra tidak ditulis ke log), `QBE-CODE-001`/`002`/`003` (nomor `AST-PMF` lewat deret bersama), `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001`, `QBE-DEL-001` (dokumen dihapus lunak; pemanfaatan dibatalkan, bukan dihapus) |
| Tidak berlaku | `QBE-ENT-*`, `QBE-CFG-*`, `QBE-NAM-*`, `QBE-DB-*` — model `AstAssetDocument`, `AstAttachment`, `AstUtilization`, `AstUtilizationItem` dan pengaturannya sudah ada sejak `BE-AST-002`/`003`/`004`; tidak ada migration |
| Arketipe transaksi | Pemanfaatan: aggregate ber-lifecycle. Dokumen kepemilikan: sub-proses ter-scope aset (CRUD, `DELETE` lunak sesuai kontrak 2.12) |

---

## 1. Masalah yang diperbaiki

Menu 3.5 dan seluruh menu 8 (Pemanfaatan & Pengamanan Aset) belum punya backend:

- Sertifikat, BPKB, STNK, dan polis tidak dapat dicatat pada aset, sehingga tidak ada yang tahu aset mana yang
  dokumen wajibnya belum lengkap atau dokumennya hampir habis masa berlakunya.
- Aset yang lama tidak digunakan tidak terpantau.
- Permohonan pemanfaatan oleh pihak ketiga (sewa, KSO, pinjam pakai) tidak punya alur persetujuan berjenjang
  `AST-RULE-013`, sehingga batas kewenangan Kepala Bagian Aset vs Direktur tidak ditegakkan, dan aset yang sedang
  dimanfaatkan tidak terkunci dari proses lain.

---

## 2. Proses bisnis

**Pemanfaatan** — pelaku: Petugas Aset (mengajukan), Kepala Bagian Aset, Direktur.

| Langkah | Pelaku | Yang terjadi |
|---|---|---|
| 1 | Petugas Aset | Membuat **draft**: jenis, mitra, nomor dan nilai perjanjian, tanggal mulai–berakhir, 1–200 aset. Nomor `AST-PMF-######` terbit saat draft dibuat. Berkas perjanjian boleh diunggah kapan saja sebelum berakhir |
| 2 | Petugas Aset | **Ajukan** → sistem memeriksa setiap aset (terdaftar, bukan Hilang/Rusak Berat, tidak terkunci proses lain, **milik RS**), lalu menghitung penyetuju: **Direktur** bila nilai ≥ batas rupiah sub-modul nilai **atau** masa > batas bulan di Pengaturan Aset; selain itu **Kepala Bagian Aset**. Penyetuju dan batas rupiah yang dipakai **dikunci**; aset dikunci proses `Pemanfaatan` |
| 3a | Kepala Bagian Aset | Penyetuju Kepala Bagian Aset: **Setujui** → `Disetujui`; bila tanggal mulai ≤ hari ini langsung `Berjalan` dan linimasa aset `Dimanfaatkan` |
| 3b | Kepala Bagian Aset | Penyetuju Direktur: hanya dapat **Teruskan** → `Menunggu Direktur` (menyetujui langsung ditolak 403) |
| 4 | Direktur | **Setujui** atau **Tolak** beralasan |
| 5 | Pekerja harian | `Disetujui` → `Berjalan` saat tanggal mulai tiba; `Berjalan` → `Berakhir` sesudah tanggal berakhir lewat; kunci aset dilepas, linimasa `Pemanfaatan Berakhir` |
| 6 | Kepala Bagian Aset | **Akhiri lebih awal** beralasan dari `Disetujui`/`Berjalan` |
| 7 | Pengaju | **Batalkan** beralasan dari `Draft`/`Diajukan` |

Contoh dari uji: sewa lahan ATM Rp 12.000.000 selama 12 bulan (`AST-PMF-000002`) → penyetuju Kepala Bagian Aset,
batas Rp 50.000.000 → disetujui → langsung **Berjalan** karena mulai hari ini. KSO alat hemodialisa Rp 30.000.000
selama 60 bulan → penyetuju **Direktur** karena masanya walau nilainya di bawah batas; Kepala Bagian Aset mencoba
menyetujui → 403, meneruskan → Direktur menyetujui.

**Dokumen dan pengamanan** — Petugas Aset menambah dokumen beserta pindaiannya pada aset terdaftar. Daftar
**Kelengkapan** menampilkan aset yang belum memiliki semua jenis dokumen wajib kategorinya; daftar **Pengamanan
Administrasi** menampilkan dokumen yang habis atau akan habis dalam `DocumentExpiryWarningDays` hari. **Aset
menganggur** = aset terdaftar berkondisi Tidak Digunakan lebih dari `IdleThresholdDays` hari, dihitung saat dibaca.

**Jalur tidak normal**

| Kondisi | Hasil |
|---|---|
| Tanggal berakhir ≤ tanggal mulai | 400 `AST-VAL-REG-041` |
| Mitra/nomor perjanjian kosong saat diajukan; nilai negatif | 400 `AST-VAL-REG-042` |
| Jenis kosong | 400 `AST-VAL-REG-043` |
| Aset terkunci, Hilang, Rusak Berat, bukan Terdaftar | 409 `AST-VAL-REG-040` |
| Aset bukan milik RS (jenis kepemilikan tidak dapat dikapitalisasi) | 409 `AST-VAL-REG-044` |
| Menyetujui pengajuan sendiri; Kepala Bagian Aset menyetujui yang wajib ke Direktur | 403 `AST-VAL-REG-045` |
| Meneruskan yang cukup Kepala Bagian Aset; menolak lewat `/reject` saat menunggu Direktur | 409 `AST-UTL-ROUTE` |
| Mengubah/membatalkan pengajuan orang lain | 403 `AST-UTL-OWNER` |
| Dokumen tanpa nomor, atau tanpa tanggal berakhir untuk jenis ber-masa-berlaku | 400 `AST-VAL-REG-080` |
| Dokumen pada aset bukan Terdaftar | 409 `AST-REG-STATUS` |
| Unduh lampiran tanpa hak atas pemiliknya | 403 |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md` backend; `rules/backend/` (engineering contract, registry, role-access, transaction endpoint
standard); kontrak 2.8, 2.12–2.14, state §5/§7, validasi §5/§7, permission §2–3, kamus data §2.5, §2.6, §2.8,
§3.6–3.8; source `BE-AST-005`, `BE-AST-007`, `BE-AST-009`, `BE-AST-010` (pola `InTransactionAsync`, `FOR UPDATE`,
`availableActions`, lampiran); `AssetValuationParameterQueryService`; contoh `BackgroundService` repository
(`OutpatientPriorityReservationClosureHostedService`) dan penanganan `Runtime:Role` di `Program.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Register/Services/AssetDocumentService.cs` | Baru — tambah/ubah (multipart, berkas opsional, berkas baru menggantikan), hapus lunak; batas unit; jenis dokumen aktif; `080` |
| `Register/Services/AssetDocumentService.Query.cs` | Baru — metadata, summary, daftar, `/expiring`, `/completeness`, rincian; masa berlaku turunan (`Berlaku`/`HampirHabis`/`Habis`/`TanpaMasaBerlaku`, `daysToExpiry`) |
| `Register/Services/AssetUtilizationService.cs` | Baru — draft, ubah, unggah perjanjian, ajukan (hitung + kunci penyetuju dan batas), setujui/teruskan, keputusan Direktur, tolak, akhiri, batalkan, `RunDailyStatusAsync` |
| `Register/Services/AssetUtilizationService.Query.cs` | Baru — metadata, summary (termasuk "menunggu keputusan Anda"), daftar (status berulang, `awaitingMyDecision`, `activeOn`), rincian dengan butir aset dan `availableActions` |
| `Register/Services/AssetAttachmentAccessService.cs` | Baru — `GET /attachments/{id}`: memeriksa hak `Read` atas resource pemilik (dokumen, pemanfaatan, laporan kondisi) dan cakupan unit |
| `Register/Services/AssetDailyStatusWorker.cs` | Baru — `BackgroundService` + opsi `AssetManagement:DailyStatusWorker` (`Enabled` bawaan `false`, `RunAtLocalTime` bawaan `00:10` WIB, `SystemActorUserId`); jalan saat start lalu harian; hanya pada `Runtime:Role` `All`/`Worker` |
| `Register/Services/AssetRegisterSettingReader.cs` | Baru — membaca `MstAssetSetting` dengan nilai bawaan; tanggal operasional WIB |
| `Register/Controllers/AssetDocumentsController.cs` | Baru — 9 endpoint, resource `AssetDocument` (Read/Create/Update/Delete) |
| `Register/Controllers/AssetUtilizationsController.cs` | Baru — 13 endpoint, resource `AssetUtilization` (Read/Create/Submit/Approve/ApproveDirector/End) |
| `Register/Controllers/AssetAttachmentsController.cs` | Baru — 1 endpoint, `AssetRegister : Read` (sesuai kontrak 2.13) |
| `Register/DTOs/AssetDocumentDtos.cs`, `AssetUtilizationDtos.cs` | Baru |
| `Register/Controllers/AssetsController.cs` | + `GET /idle` |
| `Register/Services/AssetRecordService.Query.cs` | + `GetIdleAssetsAsync`, filter `isIdle` di daftar aset, parameter metadata |
| `Register/DTOs/AssetRecordDtos.cs` | + `IsIdle`, `AssetIdleItemResponse` |
| `Register/Enums/AssetRegisterEnums.cs` | + `AssetDocumentExpiryState` (tidak dipersistensi) |
| `Register/Services/AssetManagementException.cs` | + kode `040..045`, `080`, `AST-UTL-*`, `AST-DOC-*` dan pesannya |
| `Register/Services/AssetRecordService.cs` | + label jenis/status/penyetuju pemanfaatan dan masa berlaku dokumen |
| `Register/Services/AssetConditionService.cs` | + 1 baris: `ConditionChangedAt = now` saat kondisi ditetapkan |
| `AssetManagementServiceCollectionExtensions.cs` | + 3 service, opsi worker, `AddHostedService<AssetDailyStatusWorker>` |

Tidak ada perubahan pada `Program.cs`, model, configuration, atau migration.

### 3.3 Dampak kontrak API, database, dan keamanan

**Delta terhadap kontrak `AST-REG-1.0.0`** (permukaan teknis, tanpa aturan bisnis baru):

| No | Delta | Alasan |
|---|---|---|
| 1 | `POST /utilizations/{id}/agreement` (multipart, `AssetUtilization : Create`) | Kolom `AgreementAttachmentId` ada di kamus data, tetapi kontrak 2.14 tidak menyediakan cara mengunggahnya; dipisah dari `POST`/`PUT` JSON agar draft tetap JSON |
| 2 | `GET filters/metadata` dan `GET summary` pada dokumen dan pemanfaatan; `GET /documents/{id}` | Pola layar daftar modul ini (`BE-AST-009`/`010`) |
| 3 | `/approve` menerima `{ forward: true }` untuk "Teruskan ke Direktur" | Kontrak menyebut `/approve` = setujui **atau** teruskan; perlu penanda niat. Tanpa `forward` pada pemanfaatan tingkat Direktur → 403 `045` (`AT-REG-41`) |
| 4 | `/approve-director` memakai `{ accept: false, reason }` untuk menolak | Sesuai catatan kontrak 2.14 |
| 5 | `GET /utilizations?status=2&status=3` (status berulang), `awaitingMyDecision`, `activeOn` | Layar antrean `SCR-AST-REG-32` (`?status=Diajukan,MenungguDirektur`) |
| 6 | `availableActions` pada respons dokumen dan pemanfaatan | Pola modul; frontend memotongnya dengan hak pengguna |
| 7 | `ConditionChangedAt` kini diisi saat kondisi ditetapkan (`BE-AST-010`) | Tanpa itu "aset menganggur" (`AST-DES-017`) tidak dapat dihitung; sebelumnya hanya diisi saat aset disetujui |
| 8 | `idleSince = ConditionChangedAt ?? ApprovedAt ?? CreateDateTime` | Aset lama yang kondisinya ditetapkan sebelum perbaikan no. 7 tetap punya titik hitung |
| 9 | Durasi pemanfaatan = jumlah bulan dibulatkan ke atas; "lebih dari 12 bulan" = tanggal berakhir sesudah tanggal mulai + 12 bulan | `AST-RULE-013` tidak merinci cara hitung; sewa 1 Jan–31 Des tetap 12 bulan |
| 10 | Pelaku pekerja harian = `SystemActorUserId` bila diatur, selain itu penyetuju terakhir | Linimasa mewajibkan pelaku; pola `SystemActorUserId` sudah dipakai scheduler Accounting/Finance |
| 11 | Hak atas lampiran kondisi lewat `/attachments/{id}` mengikuti aturan lihat laporan kondisi (unit, penanggung jawab, pelapor) | Endpoint generik 2.13 harus konsisten dengan `GET /condition-reports/{id}/attachments/{attId}` |

**Database:** tanpa migration. Data yang ditulis uji ke `QuilvianNewDevRizki`: bagian 5.4.

**Keamanan:** setiap endpoint punya `[AccessAction]` + `[AccessPermission]` yang cocok (39/39 di 4 controller
yang disentuh). Tidak ada hardcode peran; pemisahan pengaju ≠ penyetuju, penyetuju sesuai tahap, dan pemilik draft
diperiksa di service. Nomor dokumen dan nama mitra tidak masuk log audit.

---

## 4. Dokumentasi endpoint

#### Corporate / Asset Management / Register / Asset Document (`…/register/documents`)

| Method | Path | Hak | Keterangan |
|---|---|---|---|
| `GET` | `/filters/metadata` | `AssetDocument : Read` | Saringan `expiryState` |
| `GET` | `/summary` | `AssetDocument : Read` | `total`, `expired`, `expiringSoon`, `incompleteAssets`, `warningDays` |
| `GET` | `/` | `AssetDocument : Read` | `assetId`, `documentTypeId`, `categoryId`, `organizationUnitId`, `expiryState`, `search` |
| `GET` | `/completeness` | `AssetDocument : Read` | Aset terdaftar dengan dokumen wajib belum lengkap; `missingDocumentTypes`, `ownedCount`/`requiredCount` |
| `GET` | `/expiring` | `AssetDocument : Read` | Dokumen dengan `expiryDate ≤ hari ini + warningDays`, terdekat di atas |
| `GET` | `/{id}` | `AssetDocument : Read` | — |
| `POST` | `/` | `AssetDocument : Create` | multipart: `assetId`, `documentTypeId`, `documentNumber`, `issuedDate`, `expiryDate`, `holderName`, `notes`, `file` |
| `PUT` | `/{id}` | `AssetDocument : Update` | multipart; aset tidak dapat diganti |
| `DELETE` | `/{id}` | `AssetDocument : Delete` | Hapus lunak |

#### Corporate / Asset Management / Register / Asset Attachment (`…/register/attachments`)

| `GET` | `/{id}` | `AssetRegister : Read` + hak `Read` resource pemilik | Berkas |
|---|---|---|---|

#### Corporate / Asset Management / Register / Asset Utilization (`…/register/utilizations`)

| Method | Path | Hak | Body |
|---|---|---|---|
| `GET` | `/filters/metadata`, `/summary`, `/`, `/{id}` | `AssetUtilization : Read` | — |
| `POST` | `/` | `AssetUtilization : Create` | `{ utilizationType: 1..3, partnerName, agreementNumber, agreementValue, startDate, endDate, purpose, assetIds[] }` |
| `PUT` | `/{id}` | `AssetUtilization : Create` | sama + `rowVersion` |
| `POST` | `/{id}/agreement` | `AssetUtilization : Create` | multipart `file`, `rowVersion` |
| `POST` | `/{id}/submit` | `AssetUtilization : Submit` | — |
| `POST` | `/{id}/approve` | `AssetUtilization : Approve` | `{ forward, reason, rowVersion }` |
| `POST` | `/{id}/approve-director` | `AssetUtilization : ApproveDirector` | `{ accept, reason, rowVersion }` (tolak wajib alasan) |
| `POST` | `/{id}/reject` | `AssetUtilization : Approve` | `{ reason, rowVersion }` |
| `POST` | `/{id}/end` | `AssetUtilization : End` | `{ reason, rowVersion }` |
| `POST` | `/{id}/cancel` | `AssetUtilization : Create` | `{ reason, rowVersion }` |

#### Corporate / Asset Management / Register / Asset (`…/register/assets`)

| `GET` | `/idle` | `AssetRegister : Read` | `PagedResult<AssetIdleItemResponse>` (`idleSince`, `idleDays`, `idleThresholdDays`); `GET /?isIdle=true` memakai saringan yang sama |
|---|---|---|---|

Enum pada body JSON wajib angka (pola modul).

---

## 5. Verifikasi

### 5.1 Statis

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict` (WorkingTree) | `Files evaluated: 20`, `VIOLATION: 0`, `REVIEW: 0` | `PASS` |
| Pasangan `[AccessAction]`/`[AccessPermission]` vs `ControllerName` | 39/39 cocok (dokumen 9, lampiran 1, pemanfaatan 13, aset 16) | `PASS` |
| Baris komentar baru | 0 | `PASS` |

### 5.2 Build dan registrasi

| Pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `dotnet build -p:RunAnalyzers=false` oleh Rizki | Berhasil; DLL 2026-11-18 10:39:38, tidak ada source yang lebih baru; DLL memuat keempat class baru | `PASS` |
| Registrasi hak akses | `AssetDocument` 4 aksi, `AssetUtilization` 6 aksi muncul di `role-access/resources/structured` | `PASS` |
| Worker terdaftar | Log 10:41:39 WIB: "Pekerja harian status pemanfaatan aset dinonaktifkan melalui konfigurasi." (bawaan mati) | `PASS` |

### 5.3 Runtime (HTTPS 7184; aktor: superadmin = pengaju, akun penyetuju uji, LOKET)

Skrip dan bukti mentah: `QuilvianSystemFrontendDev/test-with-agy/asset-management/be-ast-011-20261118/`
(`ast011-runtime.mjs` → `ast011-runtime.json`, `ast011-tambahan.mjs` → `ast011-tambahan.json`). Akun penyetuju diberi
`AssetUtilization` Read/Approve/ApproveDirector/End (dan Create pada skrip tambahan) + `AssetDocument` Read
sementara; policy dipulihkan dan dibandingkan sama persis dengan aslinya di akhir kedua skrip (6 butir).

| Skenario | Hasil |
|---|---|
| `AT-REG-14` — polis berakhir +27 hari muncul di `/expiring` (Hampir Habis, 27 hari), polis +60 hari tidak, polis −1 hari muncul paling atas (Habis) | `PASS` |
| `AT-REG-08` — sewa ATM Rp 12 jt 12 bulan → penyetuju Kepala Bagian Aset, batas Rp 50 jt dikunci → disetujui → `Berjalan` (mulai hari ini), linimasa `Dimanfaatkan`, aset terkunci `Pemanfaatan` | `PASS` (bagian "oleh pekerja harian": 5.5 dan 5.6) |
| `AT-REG-09` — KSO 60 bulan Rp 30 jt → Direktur (karena masa) → diteruskan → disetujui Direktur | `PASS` |
| `AT-REG-41` — Kepala Bagian Aset menyetujui KSO 60 bulan | `PASS` (403 `045`) |
| `AT-REG-42` — `AST-ALKMED-000002` (KSO milik vendor) diajukan | `PASS` (409 `044`) |
| Nilai Rp 60 jt 6 bulan → Direktur (karena nilai); Direktur menolak tanpa alasan 400 `010`, dengan alasan → `Ditolak`, kunci lepas | `PASS` |
| `040` aset terkunci, `041`, `042` (submit mitra kosong; nilai negatif), `043`, tanpa aset, swasetuju 403 `045`, teruskan tidak perlu 409, tolak lewat `/reject` saat menunggu Direktur 409, akhiri lebih awal (kunci lepas), batal pengaju (kunci lepas), ubah draft | `PASS` |
| Antrean penyetuju `awaitingMyDecision` dan `availableActions` (`forward`+`reject` untuk tingkat Direktur, tanpa `approve`) | `PASS` |
| Perjanjian diunggah; `GET /attachments/{id}` penyetuju 200 PDF, LOKET 403, id tak dikenal 404 | `PASS` |
| Dokumen: `080` (nomor kosong, tanpa tanggal berakhir, terbit > berakhir), ubah, aset tidak dapat diganti, hapus lalu 404, aset Draft 409, LOKET 403 | `PASS` |
| AC1 kelengkapan — Alat Medis diberi syarat Polis + Faktur sementara: `AST-ALKMED-000003` 1/2 kurang Faktur; sesudah Faktur ditambah hilang dari daftar (4 → 3); `summary.incompleteAssets` = 3; syarat dipulihkan (0) | `PASS` |
| Pemeriksaan pemilik: penyetuju (punya `Create`) mengubah/membatalkan draft orang lain | `PASS` (403 `AST-UTL-OWNER`) |
| `IDLE-BOUNDARY-TODAY` — `AST-MBL-000006` ditetapkan Tidak Digunakan hari ini (`AST-KND-000011`): tidak menganggur (batas 90), muncul di `isIdle=false`; `/idle` = `isIdle=true` | `PASS` |

**Rekap:** `ast011-runtime` **60/60**; `ast011-tambahan` **13/15**. Dua kegagalan (`AC1-COMPLETENESS-NONE`,
`COMPLETENESS-FILTER-TYPE`) adalah harapan skrip yang keliru: skrip mengira hanya `AST-ALKMED-000003` yang punya
polis, padahal `AST-ALKMED-000001` sudah diberi `POL-UJI-011-HABIS` pada skrip pertama. Hasil backend (1/2 untuk
`000001`; saringan jenis Polis = 2 aset tanpa polis) sesuai data. Skrip tidak dijalankan ulang karena akan
menggandakan data uji.

### 5.4 Data uji yang tertinggal di `QuilvianNewDevRizki`

Pemanfaatan `AST-PMF-000001`..`000008`: 000001 pinjam pakai Dibatalkan; 000002 sewa ATM **Berjalan** atas
`AST-MBL-000001` (tetap terkunci); 000003 Dibatalkan; 000004 Draft (aset KSO, ditolak `044` saat diajukan);
000005 KSO hemodialisa Berakhir (diakhiri lebih awal); 000006 Ditolak Direktur; 000007 **Berjalan** dengan tanggal
berakhir kemarin (`AST-MBL-000004`, bahan uji pekerja harian); 000008 **Disetujui** mulai 19 Nov (`AST-MBL-000005`).
Uji layar `FE-AST-008` menambah 000009 (Berjalan, `AST-MBL-000007`) dan 000010 (Disetujui Direktur, `AST-MBL-000008`);
dokumen `POL-UJI-011-HAMPIR` (`ALKMED-000003`), `POL-UJI-011-HABIS` (`ALKMED-000001`), `FKT-UJI-011-ALKMED3`,
`POL-UJI-011-JAUH-2` (dihapus lunak); laporan kondisi `AST-KND-000011` (`AST-MBL-000006` → Tidak Digunakan). Syarat
dokumen kategori dan policy penyetuju dipulihkan.

### 5.5 Pekerja harian sesudah restart backend (18 Nov 11:01 WIB)

User-secret `AssetManagement:DailyStatusWorker:Enabled=true` di-set agent di PC kantor; Rizki me-restart backend
(proses mulai 11:01:06). Log 11:01:32: "Pekerja harian status pemanfaatan aset selesai untuk 2026-11-18: 1 jatuh
tempo, 0 mulai berjalan, 1 berakhir." Pemeriksaan API (`cek-pekerja-harian.mjs` → `cek-pekerja-harian.json`) **4/4**:

| Skenario | Hasil |
|---|---|
| `AST-PMF-000007` (Berjalan, berakhir 17 Nov) → `Berakhir`, `ended` 04:01:32Z | `PASS` |
| Kunci `AST-MBL-000004` dilepas | `PASS` |
| Linimasa `AST-MBL-000004`: `Pemanfaatan Berakhir` bernomor `AST-PMF-000007`, alasan "Masa pemanfaatan berakhir sesuai tanggal perjanjian." | `PASS` |
| `AST-PMF-000008` (mulai 19 Nov) tetap `Disetujui`, aset tetap terkunci | `PASS` |

Pelaku tercatat = penyetuju terakhir (Uji Penyetuju Aset) karena `SystemActorUserId` belum diatur (delta no. 10).

### 5.6 Belum terbukti

| Butir | Sebab | Cara menutup |
|---|---|---|
| AC5 / `AT-REG-08` bagian `Disetujui`→`Berjalan` oleh pekerja harian | Tanggal mulai `AST-PMF-000008` = 19 Nov | Pekerja harian jalan 00:10 WIB (atau saat backend dinyalakan) mulai 19 Nov → `AST-PMF-000008` harus `Berjalan` dan linimasa `AST-MBL-000005` `Dimanfaatkan` |
| `AT-REG-15` kasus positif | Belum ada aset Tidak Digunakan lebih dari N hari; data tidak diubah langsung di database | Mulai 20 Nov: atur batas menganggur = 1 lewat Pengaturan Aset → `AST-MBL-000006` muncul di `/idle` (2 hari > 1), lalu pulihkan 90 |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Source | Bukti | Status |
|---|---|---|---|
| 1. Kelengkapan dokumen per kategori dan dokumen hampir habis menurut pengaturan | `AssetDocumentService.Query` (`IncompleteAssets`, `/expiring`, `DocumentExpiryWarningDays`) | `AT-REG-14`, AC1 kelengkapan | ✅ |
| 2. Aset menganggur sebagai keadaan turunan | `AssetRecordService.Query.Idle/GetIdleAssetsAsync`, `IdleThresholdDays`, `ConditionChangedAt` | Batas negatif lulus; positif menunggu waktu (5.6) | 🟡 |
| 3. Penyetuju dihitung dan dikunci saat diajukan (batas dari `nilai`) | `AssetUtilizationService.SubmitAsync` (`GetRequiredParameterAtAsync`, `RequiredApprovalLevel`, `ThresholdUsed`) | `AT-REG-08/09/41`, nilai Rp 60 jt | ✅ |
| 4. Aset bukan milik RS ditolak | `EnsureEligible` (`IsCapitalizable`) | `AT-REG-42` | ✅ |
| 5. Pekerja harian `Disetujui`→`Berjalan`→`Berakhir` | `AssetDailyStatusWorker`, `RunDailyStatusAsync` | `Berjalan`→`Berakhir` 4/4 (5.5); `Disetujui`→`Berjalan` menunggu 19 Nov (5.6) | 🟡 |

| DoD | Status |
|---|---|
| `V-BE` QBE, build, Swagger/registrasi hak, runtime | Terpenuhi kecuali dua butir 5.6 |
| Laporan, roadmap, traceability | Ditulis |
| Commit | Belum |

---

## 7. Catatan penutup

- **Risiko:** pelaku linimasa dari pekerja harian adalah penyetuju terakhir selama `SystemActorUserId` belum
  diatur; atur akun sistem sebelum produksi bila linimasa harus menyebut "sistem".
- **Task berikutnya:** `FE-AST-008` (pasangan; sudah dikode dan diuji, laporannya terpisah), lalu `BE-AST-012`
  (impor data awal) atau `BE-AST-013`.
- `git status --short` backend: 8 berkas diubah, 12 berkas baru, + `Storage/uploads/asset-management/` (tidak di-commit).
