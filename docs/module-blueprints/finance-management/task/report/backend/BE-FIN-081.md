# Laporan Perubahan Backend — `BE-FIN-081`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-081` |
| Judul | Setiap baris tagihan lama divalidasi, dan petugas tahu baris mana yang salah |
| Slice | `REV-14E` — `EPIC FIN-24` (migrasi tagihan lama lewat spreadsheet) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md`, bagian "Task REV-14E — `EPIC FIN-24`" |
| Trace | `FIN-DEC-136`, `FIN-DEC-140`; `FIN-DES-089`, `FIN-DES-093`; `FIN-API-1.6` F.2; `FIN-VAL-1.8` `FIN-VAL-186`..`191` dan `224`..`227`; `contracts/state-transition-matrix.md` F.2 |
| Contract version | `02-backend-architecture.md` bagian M (revisi 15); `contracts/permission-audit-matrix.md` bagian yang memuat atribut `[AccessController]`/`[AccessAction]` controller ini |
| Dependency | `BE-FIN-080` — 🟡 **sebagian** (source lengkap, `dotnet build` belum dijalankan atas permintaan eksplisit pada giliran itu); `CutoverDate` dari `BE-FIN-064` |
| Klasifikasi | `HEAVY` — mesin validasi enam aturan baris, penyimpanan berkas fisik tanpa kolom path khusus (keputusan desain), dan dua endpoint baru dengan banyak jalur gagal |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/Corporate/FinanceManagement/AccountingIntegration/**`, `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs`, `docs/module-blueprints/finance-management/**` |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | `0e256765563ef2693107e1f35e9c5a67476ca1f0` (belum ada commit baru dibuat pada sesi ini) |
| Tanggal | 2 Oktober 2026 |
| Status | 🟡 **Sebagian** — seluruh source selesai; `dotnet build` **belum dijalankan** atas permintaan eksplisit pemilik task pada giliran ini |

---

## 1. Masalah yang diperbaiki

`BE-FIN-080` membuat jalur **masuk** (templat dan pengurai), tetapi berkas yang terbaca menjadi
baris mentah **belum diperiksa kebenarannya sama sekali**. Tanpa task ini, siapa pun dapat
mengunggah berkas yang jenis debiturnya ngawur, nomor dokumennya kembar, atau tanggalnya di masa
depan — dan sistem tidak akan tahu.

Contoh konkret: bila petugas tidak sengaja menyalin baris yang sama dua kali dalam satu berkas
("INV-2024-001" muncul di baris 5 dan baris 12), tanpa task ini kedua baris akan sama-sama lolos
dan kelak (`BE-FIN-082`) melahirkan **dua** piutang untuk tagihan yang sama — penggandaan utang yang
baru ketahuan saat laporan keuangan tidak balance. Task ini menutup celah itu **sebelum** baris
mana pun sempat menjadi data sungguhan: `FIN-VAL-191` menolak baris ke-12, menyebut baris ke-5
sebagai rujukan kembarannya.

---

## 2. Proses bisnis

1. Petugas memanggil `POST /opening-item-batches` dengan berkas CSV dan `itemKind`
   (`RECEIVABLE`/`SUPPLIER_PAYABLE`). Sistem memeriksa berkas dapat dibaca dan kolom templatnya
   lengkap (`FIN-VAL-186`), lalu mengambil `CutoverDate` dari saldo awal cutover yang sudah dicatat
   (`BE-FIN-064`). Batch tersimpan berstatus `DRAFT`, berkas fisiknya disimpan untuk diperiksa ulang
   saat validasi (lihat bagian 7 — jalur penyimpanannya adalah keputusan desain, bukan kolom skema).
2. Petugas memanggil `POST /opening-item-batches/{id}/validate`. Sistem membaca ulang berkas yang
   tersimpan, lalu memeriksa **setiap baris** berurutan:
   - Nomor dokumen harus ada dan **tidak kembar** di dalam berkas yang sama (`FIN-VAL-191`) —
     diperiksa **lebih dulu** dari aturan lain, supaya baris kembar tetap ketahuan walau baris
     pertamanya sendiri punya masalah lain.
   - (Piutang) Jenis debitur harus `PAYER`, `PATIENT_GUARANTOR`, atau `EMPLOYEE_BENEFIT`
     (`FIN-VAL-187`). (Utang) Kode supplier harus cocok dengan master supplier yang sudah ada
     (`FIN-VAL-188`).
   - Sisa tagihan harus berupa angka yang sah dan lebih besar dari nol (`FIN-VAL-189`; angka yang
     tidak dapat diurai sama sekali dijawab `FIN-VAL-226`, **bukan** ditebak).
   - Tanggal dokumen dan tanggal jatuh tempo harus berupa tanggal yang sah (format `yyyy-MM-dd`;
     gagal urai → `FIN-VAL-226`), dan tanggal dokumen **tidak boleh** melewati tanggal cutover
     (`FIN-VAL-190`).
3. Bila **nol** baris bergalat, batch berpindah ke `VALIDATED` dan `TotalItemCount`/
   `TotalOutstandingAmount` tercatat dari baris yang lolos. Bila **masih ada** baris bergalat,
   batch **tetap** `DRAFT` beserta daftar galat lengkap (nomor baris, pesan, dan identitas baris —
   nama debitur atau kode supplier — supaya petugas dapat menemukan dan memperbaikinya sendiri).
   Validasi boleh dipanggil ulang sebanyak yang dibutuhkan selama batch masih `DRAFT`.
4. Jalur tidak normal: memvalidasi batch yang sudah `VALIDATED`/`APPROVED`/`LOCKED`/`REJECTED`
   ditolak `409` (`state-transition-matrix.md` F.2 hanya mengizinkan validasi dari `DRAFT`).
   Mengunggah berkas XLSX ditolak `503` (pembacanya belum ada, `BE-FIN-083`), bukan `400` — nilainya
   sah menurut kontrak, sistem yang belum siap. Mengunggah sebelum saldo awal cutover pernah dicatat
   ditolak `422`.

Yang **belum** dibangun task ini, dan sengaja: pembuatan piutang/utang sungguhan dari baris yang
lolos (`BE-FIN-082`, perpindahan ke `APPROVED`); deklarasi saldo awal Accounting
(`declare-accounting-opening`); dan `GET /`/`GET /{id}` (keduanya masih "Rencana" di kontrak).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `02-backend-architecture.md` M.1–M.8; `contracts/api-contract.md` F.2;
  `contracts/validation-matrix.md` F.4 (`FIN-VAL-186`..`191`) dan G.2 (`FIN-VAL-224`..`227`);
  `contracts/state-transition-matrix.md` F.2 (siklus `DRAFT → VALIDATED`)
- `contracts/permission-audit-matrix.md` — konfirmasi ulang blok `[AccessController]` (sama
  dengan `BE-FIN-080`, nol perubahan) dan baris aksi `Create`/`Update` untuk endpoint baru
- `Areas/Corporate/FinanceManagement/AccountingIntegration/Models/FinOpeningItemBatch.cs`,
  `Readers/{IOpeningItemFileReader,OpeningItemRawRow,CsvOpeningItemFileReader}.cs`,
  `Storage/templates/finance/*.csv` (`BE-FIN-079`/`080`) — kolom templat dan kontrak antarmuka
  pembaca yang menjadi dasar mesin validasi task ini
- `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/{FinanceOpeningBalanceService,FinanceSubledgerSetupExceptions}.cs` — pola `CutoverDate`, gaya exception, dan `ILogger<T>.LogInformation` (bukan `LoggerService.AuditAsync`) yang dipakai rumpun `AccountingIntegration` ini
- `Areas/Corporate/FinanceManagement/Collection/Services/FinanceTransactionProofService.cs` —
  pola lengkap tulis-berkas-fisik-lalu-DB (termasuk pembersihan berkas yatim bila `SaveChangesAsync` gagal), ditiru untuk penyimpanan berkas migrasi
- `Areas/Administrator/MasterData/Models/MstSupplier.cs` — `SupplierCode` sebagai kunci pencarian `FIN-VAL-188`
- `Repositories/ApplicationDbContext.cs` — konfirmasi nama `DbSet` (`FinOpeningItemBatches`, `FinOpeningBalances`, `MstSuppliers`)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceOpeningItemBatchService.cs` | **Baru.** `UploadAsync` (FIN-VAL-186, 225, penyimpanan berkas, pengambilan `CutoverDate`) dan `ValidateAsync` (enam aturan baris, `FIN-VAL-187`..`191`/`226`, perpindahan status) |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceOpeningItemBatchExceptions.cs` | **Baru.** `OpeningItemBatchBadRequestException` (400), `ValidationException` (422), `ConflictException` (409), `NotReadyException` (503) |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/DTOs/OpeningItemBatchDtos.cs` | Ditambah `UploadOpeningItemBatchRequest`, `OpeningItemBatchResponse`, `OpeningItemBatchRowValidationResult`, `OpeningItemBatchDetailResponse` |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Controllers/FinanceOpeningItemBatchesController.cs` | Ditambah `POST /` (`Create`) dan `POST /{id}/validate` (`Update`); peta exception->status kode |
| `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` | `services.AddScoped<FinanceOpeningItemBatchService>()` ditambah |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Dua endpoint baru**: `POST /opening-item-batches` dan `POST /opening-item-batches/{id}/validate`, bentuknya mengikuti `api-contract.md` F.2. `POST /` menerima CSV **dan** XLSX menurut kontrak; XLSX dijawab `503` (lihat bagian 1 laporan `BE-FIN-080` untuk pola yang sama) karena pembacanya belum dibangun — delta ini dicatat, bukan diam-diam menyimpang |
| Database | `NOT APPLICABLE` — **nol** migration, **nol** perubahan schema. Baris `FinOpeningItemBatch` ditulis dan diperbarui lewat kode aplikasi biasa ke tabel yang sudah dirancang `BE-FIN-079` (belum dieksekusi ke database manapun pada sesi ini) |
| Keamanan/Auth | `[AccessPermission("FinanceOpeningItemBatch", "Create")]` dan `"Update"` dipasang mengikuti `[AccessController]` yang sama persis dengan `BE-FIN-080` (`ControllerName = "FinanceOpeningItemBatch"`), konsisten dengan `permission-audit-matrix.md`. `ValidationSummaryJson` (dapat memuat nama debitur/kode supplier) **tidak pernah** dicatat ke `_logger` — hanya Id batch dan angka ringkasan yang dicatat |

---

## 4. Dokumentasi endpoint

#### Corporate / Finance Management / Opening Item Batch

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Mengunggah spreadsheet migrasi (`multipart/form-data`: `File`, `ItemKind`), membuat batch `DRAFT`. XLSX dijawab `503`; selain CSV/XLSX dijawab `400`; saldo awal cutover belum ada dijawab `422` | `FinanceOpeningItemBatch : Create` |
| `POST` | `/{id:guid}/validate` | Menjalankan validasi per baris. Batch bukan `DRAFT` dijawab `409` | `FinanceOpeningItemBatch : Update` |

`GET /template` (`BE-FIN-080`) tidak berubah. `GET /`, `GET /{id}`, `/declare-accounting-opening`,
`/approve`, `/reject` tetap "Rencana (belum tersedia)" — `BE-FIN-082`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Permintaan eksplisit pemilik task pada giliran ini |
| Verifikasi kontrak API | Path, `multipart/form-data`, kode status (`200/400/409/422/503`), dan `[AccessController]`/`[AccessPermission]` dibandingkan terhadap `api-contract.md` F.2 dan `permission-audit-matrix.md` | `PASS` (pembacaan manual) | Bagian 3–4 di atas |
| Verifikasi proses bisnis keenam aturan baris | Penelusuran manual (bukan automated test) sepuluh skenario buatan tangan untuk `ValidateReceivableRows`/`ValidateSupplierPayableRowsAsync`: (1) baris sah, (2) nomor dokumen kosong, (3) nomor dokumen kembar (baris 5 vs 12 — pesan menyebut baris 5), (4) jenis debitur tidak dikenal, (5) kode supplier tidak ditemukan, (6) sisa tagihan nol, (7) sisa tagihan negatif, (8) sisa tagihan bukan angka (`"Rp 100"`, memakai simbol mata uang — sengaja ditolak, bukan ditebak), (9) tanggal format salah (`"05-10-2026"`, bukan `yyyy-MM-dd`), (10) tanggal dokumen melewati cutover | `PASS` untuk kesepuluh skenario — kode galat dan pesan dilacak tangan baris-per-baris terhadap kode | Penelusuran dituliskan selama pengerjaan; **bukan** pengganti `dotnet build`/`dotnet run` sungguhan |
| Keseimbangan kurung kurawal dan kurung biasa pada seluruh berkas `.cs` baru/berubah | Sama | `PASS` | `grep -o` per berkas, seluruhnya seimbang |
| QBE preflight | Area/Module/Submodule tidak berubah dari `BE-FIN-080` (`AccountingIntegration`, prefix `Fin` terdaftar). **Nol** entity database baru | `PASS` | `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` (status tidak berubah sejak task sebelumnya) |
| Review diff/scope | Seluruh berkas yang berubah ditelusuri; dipastikan **nol** sentuhan pada migration atau skema `BE-FIN-079` | `PASS` | Tabel 3.2 |

Uji manual: `NOT FEASIBLE` pada sesi ini — menjalankan server sungguhan menuntut `dotnet build`/`dotnet run` beserta database yang migration-nya sudah diterapkan (belum terjadi).

**Tidak dijalankan:** `dotnet restore`/`dotnet build`/`dotnet run` (permintaan eksplisit); pengujian
otomatis (`rules/backend/TEST_POLICY.md` — tidak diminta eksplisit).

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Validasi bekerja di atas baris terurai, bukan di atas berkas — aturan tunggal untuk kedua format | **Terpenuhi untuk CSV; XLSX menyusul `BE-FIN-083`** | `ValidateReceivableRows`/`ValidateSupplierPayableRowsAsync` menerima `List<OpeningItemRawRow>` tanpa mengetahui asal formatnya — kode yang sama akan berlaku untuk baris hasil `XlsxOpeningItemFileReader` kelak, **tetapi** lihat KNOWN LIMITATION `SourceFormat` di bagian 7 |
| Format ditentukan dari tipe media dan ekstensi, bukan ruas yang diisi pengguna | **Terpenuhi** | `UploadOpeningItemBatchRequest` sengaja **tidak** punya ruas `format`; `IOpeningItemFileReader.CanRead(mediaType, extension)` yang memutuskan |
| Format di luar CSV/XLSX ditolak `400` | **Terpenuhi** | `FIN-VAL-225` di `UploadAsync`, lewat `_readers.FirstOrDefault(...) ?? throw OpeningItemBatchBadRequestException` |
| Galat baris memuat nomor baris | **Terpenuhi** | Setiap pesan galat pada `ValidateReceivableRows`/`ValidateSupplierPayableRowsAsync` menyertakan `row.RowNumber` |
| Batch tetap DRAFT selama masih ada galat | **Terpenuhi** | `entity.Status = errorCount == 0 ? Validated : Draft` di `ValidateAsync` |
| `SourceFormat` tercatat | **Terpenuhi untuk CSV** | `entity.SourceFormat = sourceFormat` saat `UploadAsync`; nilainya saat ini selalu `"CSV"` karena **hanya** `CsvOpeningItemFileReader` terdaftar — lihat KNOWN LIMITATION bagian 7 untuk kenapa ini **MUST** ditinjau ulang saat `BE-FIN-083` menambah pembaca kedua |
| `dotnet build` | **Belum terpenuhi** | Sengaja `NOT RUN` atas permintaan eksplisit |
| Nol migration | **Terpenuhi** | Tabel 3.2 — nol berkas `Migrations/` disentuh |
| Laporan task tracked ada | **Terpenuhi** | Berkas ini |

**Kesimpulan status:** 🟡 **Sebagian**, bukan ✅. Satu butir DoD eksplisit (`dotnet build PASS`)
**belum terpenuhi** semata karena belum dijalankan atas instruksi eksplisit.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Dua keputusan desain pada task ini **tidak tertulis verbatim** di kontrak mana pun — lihat baris berikutnya. Pemilik modul **MUST** meninjau keduanya |
| Masalah yang diketahui | (1) **Penyimpanan berkas fisik tanpa kolom path.** `POST /{id}/validate` pada kontrak **tidak** menerima request body (`api-contract.md` F.2), sehingga berkas yang diunggah `POST /` **harus** tersimpan agar bisa dibaca ulang saat validasi. `FinOpeningItemBatch` (R14.6) **tidak** punya kolom `RelativePath`/semacamnya (berbeda dari `FinTransactionProof`). Solusi yang dipakai: jalur fisik dihitung **deterministik** dari `Id`+`SourceFormat` (`Storage/uploads/finance/opening-item-batches/{Id:N}.csv`), tanpa kolom baru — menghindari membuka kembali migration `BE-FIN-079` yang sudah dilaporkan selesai. Ini keputusan implementasi, bukan kontrak yang disetujui eksplisit. (2) **`SourceFormat` selalu "CSV" untuk saat ini** (KNOWN LIMITATION, ditulis sebagai komentar kode di `UploadAsync`): menebak format dari ekstensi berkas akan **salah** untuk berkas yang diterima pembaca lewat sinyal tipe media saja (mis. `.txt` bertipe media `text/csv`, yang diterima `CsvOpeningItemFileReader.CanRead` sengaja longgar). Benar untuk sekarang karena hanya satu pembaca terdaftar, tetapi `BE-FIN-083` **MUST** mengganti logikanya (mis. menentukan dari pembaca mana yang sungguh menerima, bukan dari ekstensi secara terpisah) saat `XlsxOpeningItemFileReader` ditambahkan. (3) **Kolom templat CSV** (disusun `BE-FIN-080`, didokumentasikan sebagai keputusan implementasi di laporannya) dipakai apa adanya sebagai nama kunci `Cells` pada task ini — bila pemilik modul mengubah kolom itu, `ReceivableRequiredColumns`/`SupplierPayableRequiredColumns` dan seluruh `GetCell(row, "...")` pada `FinanceOpeningItemBatchService` **MUST** disesuaikan bersamaan |
| Risiko tersisa | (1) Pengurai/validator tidak pernah dikompilasi maupun dijalankan pada sesi ini (`dotnet build` `NOT RUN`) — kebenarannya diverifikasi lewat penelusuran manual sepuluh skenario (bagian 5), **bukan** pengganti kompilasi dan uji sungguhan. (2) Format angka (`NumberStyles.AllowDecimalPoint | AllowLeadingSign`, `InvariantCulture`, **tanpa** pemisah ribuan) dan tanggal (`yyyy-MM-dd` ketat) adalah keputusan implementasi yang **belum** dikomunikasikan ke petugas lewat templat itu sendiri (templat `BE-FIN-080` hanya berisi baris header, sengaja tanpa baris contoh — lihat laporan `BE-FIN-080` bagian 7). Risikonya: petugas mengisi "2.500.000" (format Indonesia umum) dan seluruh barisnya ditolak `FIN-VAL-226` tanpa penjelasan lebih lanjut selain "format tidak sesuai templat" — `FE-FIN-032` (frontend) **MUST** menjelaskan format yang tepat di layar unggah, bukan mengandalkan petugas menebak dari pesan galat generik. (3) `GenerateBatchNumber` memakai pola `{PREFIX}-{tanggal}-{GUID}` seperti seluruh generator nomor serumpun — **KNOWN ISSUE** bersama yang lain, belum memakai provider number-series atomik (dicatat, bukan diperbaiki di luar wewenang task ini) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat `git status --short` terkini; akumulasi task-task sebelumnya pada sesi ini tetap belum di-commit, ditambah berkas task ini pada tabel 3.2. Nol `git add`/`git commit`/`git push` dilakukan pada task ini |
| Langkah berikutnya | (1) Pemilik modul meninjau keputusan penyimpanan berkas fisik dan keterbatasan `SourceFormat` (bagian 7) sebelum `BE-FIN-083` dimulai. (2) Pemilik menjalankan `dotnet build` dan memperbarui status laporan ini ke ✅ bila PASS. (3) Lanjut `BE-FIN-082` (deklarasi saldo awal, persetujuan, penolakan, pembuatan item sungguhan) hanya setelah diminta eksplisit |

---

## 8. Addendum — dua cacat ditemukan dan diperbaiki saat `BE-FIN-082`

Task ini (berkas laporan yang sama, sesuai aturan "task dikerjakan ulang karena temuan baru")
disentuh ulang sewaktu mengerjakan `BE-FIN-082`, karena membangun persetujuan batch menuntut
membaca ulang logika validasi yang ditulis di sini dan menyingkap dua cacat:

| # | Cacat | Lokasi | Perbaikan |
|---:|---|---|---|
| 1 | `ValidateReceivableRows` menerima `EMPLOYEE_BENEFIT` sebagai jenis debitur sah untuk migrasi, padahal `CK_FinReceivable_BenefitOwner` mewajibkan `BenefitOwnerId` terisi untuk jenis itu — dan templat CSV piutang (`BE-FIN-080`) tidak punya kolom untuk itu. Baris semacam ini akan lolos validasi di sini lalu **gagal di database** saat `BE-FIN-082` mencoba membuat `FinReceivable`-nya (melanggar check constraint) | `FinanceOpeningItemBatchService.ValidateReceivableRows`, pemeriksaan `FIN-VAL-187` | Jenis debitur migrasi dipersempit menjadi hanya `PAYER`/`PATIENT_GUARANTOR` — konsisten dengan intake Billing yang juga tidak pernah menghasilkan `EMPLOYEE_BENEFIT` (FIN-DES-024, OPEN DECISION). Pesan `FIN-VAL-187` diperjelas menyebut kedua jenis yang diterima |
| 2 | `ValidateAsync` tidak memutar `entity.RowVersion` sebelum `SaveChangesAsync`, sehingga `ExpectedRowVersion` sisi klien tetap "berlaku" setelah validasi sukses — melemahkan jaminan optimistic concurrency yang menjadi tujuan `[ConcurrencyCheck]` pada kolom ini (pola seharusnya sama dengan `FinanceReceivableService`/`FinanceSupplierPayableService`, yang selalu memutar `RowVersion` pada setiap mutasi) | `FinanceOpeningItemBatchService.ValidateAsync` | Ditambah `entity.RowVersion = Guid.NewGuid();` sebelum `SaveChangesAsync` |

Keduanya **tidak** mengubah bentuk kontrak API `POST /{id}/validate` (nol perubahan request/response),
sehingga status 🟡 **Sebagian** dan seluruh isi laporan di atas (bagian 1–7) tetap berlaku apa
adanya — addendum ini murni mencatat perbaikan kode, bukan mengubah status acceptance criteria.
