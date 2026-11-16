# Laporan Perubahan Backend — `BE-AST-015`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-AST-015` |
| Judul | Parameter, kebijakan, buku nilai, penulis kotak keluar |
| Slice | `NLI-S1` Tabel dasar, parameter, buku nilai, kotak keluar (gelombang `MVP-0`) |
| Roadmap | `docs/module-blueprints/manajemen-aset/nilai/roadmap/backend-roadmap.md` `REV-1`, kartu `BE-AST-015` |
| Trace | `CAP-EXT-02`, `CAP-EXT-04`; `AST-DEC-027`, `040`; `AST-DES-032`, `033`; `contracts/api-contract.md` 2.1–2.2; `testing/acceptance-test-matrix.md` `AT-NLI-07`, `08`, `13`, `33`, `34` |
| Contract version | `AST-NLI-1.0.0` — `approved` 2026-11-12 (`AST-DEC-051`). Kepala berkas `contracts/api-contract.md` masih menulis `draft`; selisih ini dicatat, tidak diubah |
| Dependency | `BE-AST-014` ✅ 2026-11-16; `BE-AST-005` ✅ 2026-11-16 (`AssetEntityResolver`, `AssetManagementException`, `AddAssetManagement()`) |
| Klasifikasi | `MEDIUM` — satu repository, 9 berkas `.cs`, 6 endpoint baru, kolom rupiah sensitif, tanpa migration |
| Task mode | `BACKEND` — perintah Rizki (pemilik modul) "lanjutkan BE-AST-015", 2026-11-16 |
| Target tulis | `Areas/Corporate/AssetManagement/Valuation/{DTOs,Services,Controllers}/`; `Areas/Corporate/AssetManagement/AssetManagementServiceCollectionExtensions.cs`; laporan ini; baris status roadmap `nilai` (backend, frontend) dan `register` (backend), `requirement-traceability.md` sub-modul `nilai` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend | Kode: `9f8c73dfee78589a0586f43c4283dcee11b3df72` (branch `rizkiG`, commit Rizki, **belum di-push**; `rizkiG` 3 commit di depan `origin/rizkiG`). Laporan dan tanda status ini belum di-commit |
| Mesin | PC kantor |
| Tanggal | 2026-11-16 |
| Status | 🟡 Sebagian — kelima acceptance criteria terpetakan ke source. Kriteria 1 terbukti di runtime (35/35). Kriteria 2–5 belum teruji runtime, karena pemanggil service buku (`BE-AST-007`, `BE-AST-012`) dan penyusutan (`BE-AST-027`) belum ada. Akibatnya `AT-NLI-07`, `08`, `13`, `33` belum dijalankan. Rincian di bagian 6 |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` |
| Module | `AssetManagement` (Manajemen Aset & Inventaris) |
| Submodule | `Valuation` (Nilai Aset) |
| Pemilik / prefix registry | `AssetManagement / Valuation / Nilai Aset` / `Ast` / `ACTIVE` (baris registry `BE-AST-001`) |
| Keberlakuan | `NEW CODE` (8 berkas baru); `TOUCHED LEGACY` hanya `AssetManagementServiceCollectionExtensions.cs` (+7 baris aditif, milik modul ini sendiri sejak `BE-AST-005`) |
| QBE ID yang berlaku | `QBE-SVC-001..003`, `QBE-API-001..003`, `QBE-PERM-001..003`, `QBE-DTO-001..003`, `QBE-NAM-001`, `QBE-NAM-002`, `QBE-MOD-001..003`. Tidak berlaku: `QBE-ENT-*`/`QBE-CFG-001` (tanpa entity/configuration baru), `QBE-CODE-001..003` (tanpa alokator nomor), `QBE-NAM-003`/`QBE-DB-001/002` (bukan legacy migration) |
| Governance terbaca | `AGENTS.md` backend, `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, `tooling/qbe/README.md`; tidak berubah sejak `BE-AST-014` |

### Pemeriksaan snapshot

Task dilanjutkan dari kondisi terverifikasi: `9f8c73df` memuat seluruh kode, dan working tree bersih sebelum laporan
ini ditulis. Rizki menjalankan build pada 2026-11-16, menghasilkan DLL 14:45:33 WIB (sesudah commit 14:39:30 WIB).
Rizki juga menjalankan aplikasi; prosesnya mulai 14:48:16 WIB dan mendengar di 7184/5107. Jadi runtime yang diuji
memakai kode `9f8c73df`. Tidak ada penyuntingan source pada sesi lanjutan ini.

---

## 1. Masalah yang diperbaiki

Sesudah `BE-AST-014` dan migration `BE-AST-004`, kelima tabel dasar nilai sudah ada di database dev. Tetapi belum
ada jalan untuk mengisinya. Kepala Akuntansi belum bisa menetapkan batas kapitalisasi dan batas persetujuan Direktur.
Umur ekonomis per kelompok juga belum bisa ditetapkan. Service register `BE-AST-007` belum punya tempat untuk
membuat buku nilai saat aset disetujui, dan kejadian hibah belum bisa diantrekan ke Akuntansi.

Contoh: monitor pasien Rp45.000.000 milik RS yang disetujui hari ini harus dicek terhadap batas kapitalisasi
badan hukum yang berlaku bulan ini. Bila lolos, monitor mendapat buku dengan umur dan residu dari kebijakan
kelompok Alat Kedokteran. Penyusutannya mulai bulan sesudah monitor siap dipakai. Bila monitor itu hibah, satu
kejadian `AST-PEROLEHAN-HIBAH` ditulis ke kotak keluar dan tertahan (`Held`) sampai kodenya dinyalakan.

---

## 2. Proses bisnis

| Unsur | Isi |
|---|---|
| Tujuan | Parameter nilai dan kebijakan penyusutan dapat ditetapkan. Buku nilai, saldo awal, dan kejadian akuntansi siap dipanggil register |
| Pelaku | Kepala Akuntansi lewat API 2.1–2.2. Service register (`BE-AST-007`, `BE-AST-012`) dan penyusutan (`BE-AST-027`) memanggil `AssetBookService` dan `AssetAccountingEventService` |
| Pemicu | Penetapan parameter/kebijakan dari layar `FE-AST-011`; persetujuan aset atau penerapan impor |
| Hasil akhir | 6 endpoint, 4 service, 1 class aturan, dan DTO terdaftar lewat `AddAssetManagement()` |

Aturan yang dijaga service:

1. **Parameter dan kebijakan pertama boleh berlaku bulan berjalan. Baris berikutnya hanya bulan depan**
   (`AST-DES-032`). Contoh: badan hukum RS belum punya parameter pada November 2026. Penetapan pertama berlaku
   1 Nov 2026. Penetapan kedua untuk November ditolak `AST-VAL-NLI-002` "Parameter baru berlaku mulai Desember
   2026". Penetapan untuk Desember diterima.
2. **Satu baris per bulan berlaku.** Penetapan Desember kedua ditolak `409 AST-VAL-NLI-003`. Penolakan ini datang
   dari pemeriksaan awal, dan dari pelanggaran index unik bila dua permintaan berbarengan.
3. **Batas rupiah ≥ 0, paling banyak dua desimal** (`AST-VAL-NLI-001`). **Umur 1–600 bulan, residu 0–100%**
   (`AST-VAL-NLI-004`). **Kelompok yang kategorinya tidak disusutkan** (mis. Tanah `TNH`) tidak boleh diberi
   kebijakan (`AST-VAL-NLI-005`).
4. **Kebijakan berlaku ke depan bagi aset tanpa estimasi sendiri** (`AST-DES-033`). `ResolveEstimateAsync`
   memakai kebijakan kelompok yang berlaku pada periode yang dihitung, kecuali buku punya `HasEstimateOverride`.
   Contoh `AT-NLI-07`: laptop 48 bulan, kebijakan menjadi 60 bulan mulai Juli. Juni tetap 48, dan Juli dihitung
   dari sisa.
5. **Buku nilai** (`AST-RULE-001`). Aset dikapitalisasi bila harga ≥ batas kapitalisasi yang berlaku saat disetujui
   **dan** jenis kepemilikannya dapat dikapitalisasi. Aset disusutkan bila dikapitalisasi dan kategorinya
   disusutkan. Penyusutan mulai bulan sesudah tanggal siap pakai. Satu entri `Perolehan` ditulis ke riwayat nilai.
6. **Saldo awal impor.** Bulan yang sudah disusutkan dihitung dari awal penyusutan sampai bulan cutover. Akumulasi
   ditolak bila negatif atau melebihi biaya dikurangi residu (`AST-VAL-REG-064`). Umur yang sudah habis tetapi nilai
   buku masih di atas residu juga ditolak (`AST-VAL-NLI-040`). Satu entri `SaldoAwal` ditulis, **tanpa** kotak keluar.
7. **Kejadian akuntansi** ditulis `Pending` bila kodenya ada di `AssetManagement:AccountingDispatch:EnabledEventCodes`,
   selain itu `Held`. Kode di luar enam kode modul ditolak `AST-NLI-EVENT-CODE`. Pasangan sumber dan versi yang sama
   tidak ditulis dua kali (idempoten).
8. **Persetujuan aset tanpa parameter yang berlaku** ditolak `409 AST-VAL-NLI-008` "Parameter nilai belum ditetapkan
   Akuntansi; aset belum dapat disetujui."

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu dan tabel task roadmap `nilai` dan `register` (`BE-AST-007`, `BE-AST-012`); `contracts/api-contract.md`
2.1–2.2; `testing/acceptance-test-matrix.md`; laporan `BE-AST-005`, `BE-AST-006`, `BE-AST-014`;
`AssetMasterControllerBase`, `AssetEntityResolver`, `AssetManagementException`; `FinanceBusinessDate`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/AssetManagement/Valuation/DTOs/AssetValuationDtos.cs` | Baru — `ValuationPagedQuery`, `ValuationParameterSetRequest`/`Response`/`CurrentResponse`, `DepreciationPolicySetRequest`/`Response`/`GroupResponse` |
| `.../Valuation/Services/AssetValuationRules.cs` | Baru — kode galat, pesan, `AssetValuationPeriod` (bulan bisnis WIB lewat `FinanceBusinessDate`), `AssetValuationMoney` |
| `.../Valuation/Services/AssetValuationParameterQueryService.cs` | Baru — parameter/kebijakan yang berlaku pada tanggal tertentu; `GetRequiredParameterAtAsync` → `AST-VAL-NLI-008` |
| `.../Valuation/Services/AssetValuationParameterService.cs` | Baru — riwayat, `current`, penetapan parameter; daftar kebijakan per kelompok, riwayat, penetapan kebijakan; audit `LoggerService.AuditAsync` tanpa nilai rupiah parameter |
| `.../Valuation/Services/AssetBookService.cs` | Baru — `CreateForRegisteredAssetAsync`, `CreateOpeningBalanceAsync`, `ResolveEstimateAsync`, `GetNetBookValue`, `GetDepreciationStartPeriod` |
| `.../Valuation/Services/AssetAccountingEventService.cs` | Baru — enam kode kejadian, opsi `AssetAccountingDispatchOptions`, `ResolveInitialStatus`, `QueueAsync` |
| `.../Valuation/Controllers/AssetValuationParametersController.cs` | Baru — 2.1, resource izin `AssetValuationParameter` |
| `.../Valuation/Controllers/AssetDepreciationPoliciesController.cs` | Baru — 2.2, resource izin `AssetDepreciationPolicy` |
| `Areas/Corporate/AssetManagement/AssetManagementServiceCollectionExtensions.cs` | +1 `using`, +1 `Configure<AssetAccountingDispatchOptions>`, +4 `AddScoped`; `Program.cs` tidak disentuh |
| `docs/.../nilai/task/report/backend/BE-AST-015.md` | Laporan ini |
| `docs/.../nilai/roadmap/backend-roadmap.md`, `frontend-roadmap.md`, `requirement-traceability.md`; `docs/.../register/roadmap/backend-roadmap.md` | Tanda status `BE-AST-015` |

Seluruh berkas `.cs` **tanpa satu pun baris komentar**. Hasil pemeriksaan: 0 baris `//`, `/*`, `*` yang ditambahkan
pada `git show 9f8c73df`.

Keputusan teknis di dalam cakupan:

| Keputusan | Alasan |
|---|---|
| Service buku dan kotak keluar **tidak** memanggil `SaveChanges` | Pemanggil (persetujuan aset, penerapan impor, proses penyusutan) wajib menyimpan aset, buku, entri, dan kotak keluar dalam satu transaksi. `FindExistingAsync` dan `QueueAsync` juga memeriksa `Local`, jadi panggilan ulang dalam unit kerja yang sama tidak menulis dua kali |
| Kejadian hibah diantrekan **di dalam** `CreateForRegisteredAssetAsync` | Persetujuan hibah selalu menghasilkan buku dan kejadian bersamaan. Kartu `BE-AST-007` menyebut `AssetAccountingEventService.StageDonationAcquisition`, yang tidak dibuat. `BE-AST-007` cukup memanggil `CreateForRegisteredAssetAsync` (lihat delta) |
| Sumber kejadian hibah `GRANT:<AssetId>` versi `1` | Satu aset hanya punya satu perolehan hibah. Pasangan ini mengikat index unik `(SourceTransactionId, SourceVersion)` |
| Batas kapitalisasi diambil dari parameter yang berlaku pada **tanggal persetujuan** (aset baru) atau **tanggal cutover** (impor) | `AST-DEC-027`: parameter berlaku per bulan; aset dinilai terhadap aturan saat ia dicatat |
| Periode memakai `FinanceBusinessDate` (WIB) | Bulan bisnis sama dengan modul Akuntansi; pada 31 Okt pukul 23.30 WIB periodenya Oktober, bukan November menurut UTC |
| Audit penetapan parameter **tidak** memuat nilai batas | Kolom rupiah sensitif menurut kamus; jejak audit memuat Id, badan hukum, periode, pelaku |

### 3.3 Delta terhadap kontrak `AST-NLI-1.0.0`

Penambahan permukaan teknis saja, tanpa aturan bisnis baru:

| No | Kontrak | Source | Alasan |
|---|---|---|---|
| 1 | Kartu: 1 controller `AssetValuationParametersController` untuk 2.1–2.2 | 2 controller: `AssetValuationParametersController` (2.1) dan `AssetDepreciationPoliciesController` (2.2) | Kedua bagian memakai resource izin berbeda (`AssetValuationParameter`, `AssetDepreciationPolicy`). Argumen ke-1 `[AccessPermission]` wajib sama dengan `ControllerName` satu controller |
| 2 | `GET /parameters/current` → `ValuationParameterResponse` | `ValuationParameterCurrentResponse` { `LegalEntityId`, `LegalEntityName`, `CurrentPeriod`, `NextAllowedPeriod`, `Current?`, `Upcoming?` } | Kegunaan di kontrak meminta "yang berlaku hari ini **dan** yang akan berlaku"; satu objek tidak bisa memuat keduanya. `NextAllowedPeriod` memberi tahu layar bulan yang boleh dipilih |
| 3 | `GET /depreciation-policies` → `PagedResult<DepreciationPolicyResponse>`, filter `assetGroupId` | `PagedResult<DepreciationPolicyGroupResponse>` — satu baris per kelompok yang kategorinya disusutkan, berisi `Current?` dan `Upcoming?`; filter `assetGroupId` dan `search` | Kegunaan "kebijakan berlaku per kelompok". Kelompok yang belum punya kebijakan tetap tampil, sehingga Kepala Akuntansi melihat yang belum ditetapkan |
| 4 | Paging `page`, `pageSize` | `pageNumber`, `pageSize` (bawaan 25, maks 100) | Sama dengan endpoint master `BE-AST-006` dan `PagedResult` proyek |
| 5 | `POST /parameters` "berlaku bulan berikutnya" | `EffectiveFromPeriod` opsional; bila kosong: baris pertama = bulan berjalan, selain itu bulan depan | Acceptance criteria 1 dan `AST-DES-032` |
| 6 | Kode galat 2.1–2.2 | Tambahan `AST-NLI-FIELD` (400, kelompok kosong/catatan > 500), `AST-NLI-NOT-FOUND` (404, kelompok tidak ada), `AST-NLI-EVENT-CODE` (400, kode kejadian tak dikenal, internal) | Pola `AST-MST-FIELD`/`AST-MST-NOT-FOUND` dari `BE-AST-006`. Belum tercatat di matriks galat |
| 7 | — | Opsi konfigurasi `AssetManagement:AccountingDispatch:EnabledEventCodes` (bawaan kosong → semua `Held`) | Acceptance criteria 4 menyebut "sesuai konfigurasi kode". Belum ditulis di `appsettings`, jadi bawaannya kosong |
| 8 | Kartu `BE-AST-007`: `AssetAccountingEventService.StageDonationAcquisition` | Tidak ada; hibah diantrekan oleh `AssetBookService.CreateForRegisteredAssetAsync` | Lihat keputusan teknis. `BE-AST-007` perlu tahu agar tidak mengantrekan dua kali |

### 3.4 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 6 endpoint baru (bagian 4), delta di 3.3 |
| Database | Tanpa migration dan tanpa perubahan skema. Uji runtime **menulis data** ke `QuilvianNewDevRizki` atas izin Rizki 2026-11-16 (bagian 5.2) |
| Keamanan/Auth | Seluruh action ber-`[Authorize]`, `[AccessAction]`, dan `[AccessPermission]`. Arg ke-1 sama dengan `ControllerName`, arg ke-2 sama dengan nama `AccessAction`; `AccessType` hanya `Read`/`Update`. Tidak ada `IsInRole`, nama peran, atau `UserType`. Respons memuat rupiah, sehingga hanya resource ber-hak nilai yang bisa membacanya (kontrak bagian 1). Log audit tidak memuat nilai batas |

---

## 4. Dokumentasi endpoint

Base: `api/v1/corporate/asset-management/valuation`. Semua respons dibungkus `ApiResponse<T>`. Galat
`AssetManagementException` dipetakan oleh `AssetMasterControllerBase` ke status dan `errors.code`.

| Method | Path | Izin | Request | Response | Galat |
|---|---|---|---|---|---|
| `GET` | `/parameters` | `AssetValuationParameter : Read` | `pageNumber`, `pageSize` | `PagedResult<ValuationParameterResponse>`, urut periode menurun | — |
| `GET` | `/parameters/current` | `AssetValuationParameter : Read` | — | `ValuationParameterCurrentResponse` | 503 bila badan hukum belum dikonfigurasi |
| `POST` | `/parameters` | `AssetValuationParameter : Update` | `{ effectiveFromPeriod?, capitalizationThreshold, disposalDirectorThreshold, utilizationDirectorThreshold, note? }` | `ValuationParameterResponse` | 400 `001`, `002`, `AST-NLI-FIELD`; 409 `003` |
| `GET` | `/depreciation-policies` | `AssetDepreciationPolicy : Read` | `assetGroupId?`, `search?`, `pageNumber`, `pageSize` | `PagedResult<DepreciationPolicyGroupResponse>` | — |
| `GET` | `/depreciation-policies/{assetGroupId}/history` | `AssetDepreciationPolicy : Read` | — | `List<DepreciationPolicyResponse>`, urut menurun | 404 `AST-NLI-NOT-FOUND` |
| `POST` | `/depreciation-policies` | `AssetDepreciationPolicy : Update` | `{ assetGroupId, effectiveFromPeriod?, usefulLifeMonths, residualValuePercent }` | `DepreciationPolicyResponse` | 400 `004`, `005`, `002`, `AST-NLI-FIELD`; 404 `AST-NLI-NOT-FOUND`; 409 `003` |

Kode `00x` = `AST-VAL-NLI-00x`.

---

## 5. Verifikasi

### 5.1 Statis dan build

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -BaseRef 9f8c73df~1 -HeadRef 9f8c73df -Mode Strict` | `Scope: GitRange`; `Files evaluated: 9`; `VIOLATION: 0`, `REVIEW: 0`, `INFO: 0`; `Findings: none`; `Final result: PASS` | `PASS` | Dijalankan ulang 2026-11-16 sore atas commit; sama dengan hasil sebelum commit |
| Baris komentar | 0 baris komentar ditambahkan pada 9 berkas `.cs` | `PASS` | `git show 9f8c73df -- '*.cs'` disaring pola komentar |
| `dotnet build` | Dijalankan Rizki. DLL `bin/Debug/net9.0/QuilvianSystemBackend.dll` 2026-11-16 14:45:33 WIB, sesudah commit 14:39:30 WIB, memuat kelima class service baru | `PASS` | Build gagal tidak menerbitkan DLL baru. **Jumlah warning tidak dilaporkan** |
| Swagger `/swagger/corporate/swagger.json` (aplikasi dijalankan Rizki) | 6 operasi pada 4 path `/valuation/`, tag `Corporate / Asset Management / Valuation / Valuation Parameter` dan `... / Depreciation Policy`, persis bagian 4 | `PASS` | Skrip Node atas dokumen Swagger |
| Review diff dan cakupan | 8 berkas baru + 7 baris aditif di extension modul; `Program.cs`, `Migrations/`, `*.csproj`, `ApplicationDbContext.cs` tidak berubah | `PASS` | `git show --stat 9f8c73df` |

### 5.2 Runtime 2.1–2.2

Lewat HTTPS `https://localhost:7184`, akun superadmin uji (sandi dari `QUILVIAN_SUPERADMIN_*`), badan hukum PT
Metropolitan Medical Centre, periode berjalan November 2026. Skrip ada di scratchpad sesi; label di keluarannya
diselaraskan dengan matriks sesudah dijalankan. **Hasil: 35 dari 35 `PASS`.**

| Kelompok | Skenario | Hasil |
|---|---|---|
| Baca awal | Login 200; `GET /parameters/current` 200, belum ada parameter, `NextAllowedPeriod` = 2026-11-01; `GET /parameters` 200 | 4/4 |
| Validasi parameter | Batas −1 → 400 `001`; 1,234 → 400 `001`; periode 15 Des → 400 `002`; periode Jan 2027 → 400 `002` | 4/4 |
| Validasi kebijakan | Daftar per kelompok 200, hanya 6 kelompok yang disusutkan (`ALKMED`, `ALNM`, `GDG`, `KEND`, `MBL`, `TI`); `TNH` → 400 `005`; umur 0 dan 601 → 400 `004`; residu 100,5 → 400 `004`; tanpa kelompok → 400 `AST-NLI-FIELD`; kelompok tidak ada → 404 `AST-NLI-NOT-FOUND` (`POST` dan `history`) | 9/9 |
| Parameter (menulis) | Baris pertama tanpa periode → 200, berlaku 2026-11-01; **`AT-NLI-34`** baris kedua bulan ini → 400 `002` "Parameter baru berlaku mulai Desember 2026"; Desember → 200; Desember diulang → 409 `003`; tanpa periode (bawaan Desember, sudah ada) → 409 `003`; `current` = Nov, `upcoming` = Des, `NextAllowedPeriod` = Des; riwayat 2 baris, Des di atas | 8/8 |
| Kebijakan (menulis) | `ALKMED` pertama → 200, berlaku 2026-11-01; kedua bulan ini → 400 `002`; Desember → 200; diulang → 409 `003`; riwayat 2 baris, Des di atas; daftar: `current` Nov, `upcoming` Des | 9/9 |
| Auth | Tanpa login → 401 | 1/1 |

**Data yang ditulis ke `QuilvianNewDevRizki`** (izin Rizki 2026-11-16, angka contoh, bukan `AST-OQ-001..004`):

| Tabel | Baris |
|---|---|
| `AstValuationParameter` | PT Metropolitan Medical Centre, berlaku 2026-11-01 dan 2026-12-01: kapitalisasi Rp1.000.000, penghapusan Rp50.000.000, pemanfaatan Rp50.000.000; catatan "Uji BE-AST-015" / "Uji BE-AST-015 bulan depan" |
| `AstDepreciationPolicy` | Kelompok `ALKMED`, berlaku 2026-11-01 dan 2026-12-01: 60 bulan, residu 0,00% |

Baris ini tidak bisa dihapus lewat API. Parameter November inilah yang berlaku saat `BE-AST-007` diuji.

### 5.3 Yang belum dijalankan

| Skenario | Status | Alasan |
|---|---|---|
| `AT-NLI-07` kebijakan 48 → 60 bulan berlaku ke depan | `NOT RUN` | Butuh buku aset dan perhitungan penyusutan (`BE-AST-027`) |
| `AT-NLI-08` saldo awal Rp60 jt, akumulasi Rp24 jt, 24/60 bulan → Rp1.000.000/bulan | `NOT RUN` | Pemanggil `CreateOpeningBalanceAsync` adalah penerapan impor `BE-AST-012`. Matriks menandainya `UNIT`, sedangkan backend tidak memelihara project test (`rules/backend/TEST_POLICY.md`) |
| `AT-NLI-13` hibah disetujui → satu kejadian `AST-PEROLEHAN-HIBAH` `Held` | `NOT RUN` | Butuh persetujuan aset `BE-AST-007` |
| `AT-NLI-33` setujui aset sebelum parameter ada → 409 `008` | `NOT RUN` | Butuh `BE-AST-007`. **Catatan:** sesudah uji 5.2, badan hukum dev sudah punya parameter mulai November 2026. Skenario ini tidak bisa lagi dijalankan dengan aset yang disetujui hari ini pada badan hukum itu. Pilihannya: badan hukum lain tanpa parameter, atau impor dengan tanggal cutover sebelum November 2026 (jalur `BuildBookAsync` yang sama) |
| 403 untuk pengguna tanpa izin `AssetValuationParameter`/`AssetDepreciationPolicy` | `NOT RUN` | Akun uji superadmin melewati pemeriksaan izin. Atribut sudah dicocokkan statis (bagian 3.4) |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.

**Tidak dijalankan agent:** `dotnet build`, `dotnet run`, `dotnet ef`, migration, dan SQL. Build dan aplikasi
dijalankan Rizki.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Baris pertama parameter boleh bulan berjalan, berikutnya bulan depan | Terpenuhi, teruji runtime | `AssetValuationParameterService.ResolveEffectivePeriod` (dipakai parameter dan kebijakan); runtime 5.2 termasuk `AT-NLI-34` |
| 2. Kebijakan kelompok berlaku ke depan bagi aset tanpa estimasi sendiri | Terpetakan ke source; runtime menunggu `BE-AST-027` | `ResolveEstimateAsync` (lewati bila `HasEstimateOverride`) + `GetPolicyAtAsync` (kebijakan terakhir dengan `EffectiveFromPeriod ≤ periode`); periode kebijakan baru hanya bulan depan (kriteria 1). `AT-NLI-07` `NOT RUN` |
| 3. Buku: dikapitalisasi bila harga ≥ batas **dan** kepemilikan dapat dikapitalisasi; mulai bulan sesudah siap pakai; saldo awal menghitung bulan yang sudah disusutkan | Terpetakan ke source; runtime menunggu `BE-AST-007`/`012` | `BuildBookAsync` (`isCapitalized = IsCapitalizable && cost >= CapitalizationThreshold`), `GetDepreciationStartPeriod` (`Next(ReadyForUseDate)`), `CreateOpeningBalanceAsync` (`DepreciatedMonths = MonthsBetween(start, cutover)`, dibatasi umur). `AT-NLI-08` `NOT RUN` |
| 4. Kejadian hibah ditulis `Held`/`Pending` sesuai konfigurasi kode | Terpetakan ke source; runtime menunggu `BE-AST-007` | `CreateForRegisteredAssetAsync` → `QueueAsync(AST-PEROLEHAN-HIBAH)`; `ResolveInitialStatus` dari `EnabledEventCodes`. `AT-NLI-13` `NOT RUN` |
| 5. Persetujuan aset tanpa parameter → `AST-VAL-NLI-008` | Terpetakan ke source; runtime menunggu `BE-AST-007` | `GetRequiredParameterAtAsync` → `AssetManagementException.Conflict(AST-VAL-NLI-008)` dipanggil `BuildBookAsync`. `AT-NLI-33` `NOT RUN` |
| Cakupan kartu: 3 service + 1 controller | Terpenuhi dengan delta | 4 service (+ query service terpisah, seperti kartu `QueryService`) dan 2 controller (delta 3.3 no. 1) |
| `DoD-BE`: `V-BE` dijalankan dan dicatat apa adanya | **Sebagian** | QBE, build, Swagger, runtime 2.1–2.2 35/35. `AT-NLI-07`, `08`, `13`, `33` belum (5.3) |
| `DoD-BE`: laporan, roadmap, traceability, `git status --short` | Terpenuhi | Laporan ini; bagian 7 |

Rekap: **5 dari 5 kriteria terpetakan ke source; 1 dari 5 teruji runtime** → `🟡`. Task menjadi `✅` sesudah
`AT-NLI-13` dan `AT-NLI-33` lolos pada uji runtime `BE-AST-007`, `AT-NLI-08` lolos pada `BE-AST-012`, dan
`AT-NLI-07` lolos pada `BE-AST-027`. Kode `BE-AST-015` tidak perlu diubah untuk itu, kecuali uji tersebut menemukan
cacat.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada peringatan tool |
| Masalah yang diketahui | (1) Kepala berkas `contracts/api-contract.md` masih `draft` dan kolom status 2.1–2.2 masih "Rencana", padahal roadmap menyatakan `approved`. Kolom status kontrak tidak diubah, sesuai preseden `BE-AST-006`. (2) Kode galat `AST-NLI-FIELD`, `AST-NLI-NOT-FOUND`, `AST-NLI-EVENT-CODE` belum ada di matriks galat (bersama delta `BE-AST-005`) |
| Risiko tersisa | (1) Jumlah warning build tidak tercatat. (2) Parameter dan kebijakan uji kini aktif di DB dev. Batas Rp1.000.000 dan residu 0% `ALKMED` akan memengaruhi hasil uji `BE-AST-007` dan `BE-AST-027`; angka sebenarnya menunggu `AST-OQ-001..004`. (3) `AT-NLI-33` butuh jalur uji lain (5.3). (4) `EnabledEventCodes` belum ada di `appsettings`, sehingga semua kejadian `Held`. Itu memang bawaan yang benar sampai `GATE-AST-ACC` |
| Perubahan sampingan | `NONE`. Skrip uji berada di scratchpad sesi |
| Interupsi | Sesi sebelumnya berhenti sesudah commit `9f8c73df`. Dilanjutkan dari commit itu tanpa penyuntingan source |
| Langkah berikutnya | (1) Commit laporan dan tanda status ini. (2) `BE-AST-007`: wajib mendeklarasikan `[assembly: AccessExplicitPermission]` `AssetRegister : ReadAllUnits`, memanggil `AssetBookService.CreateForRegisteredAssetAsync` dalam transaksi persetujuan (tanpa `StageDonationAcquisition` terpisah), lalu menguji `AT-NLI-13` dan `AT-NLI-33` untuk menutup task ini |

`git status --short` di akhir task:

```text
 M docs/module-blueprints/manajemen-aset/nilai/roadmap/backend-roadmap.md
 M docs/module-blueprints/manajemen-aset/nilai/roadmap/frontend-roadmap.md
 M docs/module-blueprints/manajemen-aset/nilai/roadmap/requirement-traceability.md
 M docs/module-blueprints/manajemen-aset/register/roadmap/backend-roadmap.md
?? docs/module-blueprints/manajemen-aset/nilai/task/report/backend/BE-AST-015.md
```

Tidak ada yang di-stage atau di-commit.
