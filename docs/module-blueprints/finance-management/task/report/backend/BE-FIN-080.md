# Laporan Perubahan Backend — `BE-FIN-080`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-080` |
| Judul | Berkas migrasi dapat dibaca menjadi baris bernomor, dan jalur CSV berjalan tanpa paket apa pun |
| Slice | `REV-14E` — `EPIC FIN-24` (migrasi tagihan lama lewat spreadsheet) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md`, bagian "Task REV-14E — `EPIC FIN-24`" |
| Trace | `FIN-DEC-136`, `FIN-DEC-140`; `FIN-DES-093`; `FIN-API-1.6` F.2; `FIN-VAL-1.8` `FIN-VAL-224`/`226`; `erd/data-dictionary.md`/`02-backend-architecture.md` bagian M |
| Contract version | `02-backend-architecture.md` bagian M (revisi 15), disetujui Yasmin 2 Oktober 2026 (`blueprint-manifest.md` `approval_revision_15`); `contracts/permission-audit-matrix.md` bagian yang memuat atribut `[AccessController]` controller ini persis |
| Dependency | `BE-FIN-079` — 🟡 **sebagian** (source lengkap, `dotnet build`/migration belum dijalankan atas permintaan eksplisit pada giliran itu). Dilanjutkan karena bagian yang dibutuhkan `BE-FIN-080` (model `FinOpeningItemBatch` dan konstanta `ItemKind`) sudah ada di source, bukan menunggu build berhasil |
| Klasifikasi | `MEDIUM` — nol migration/model baru, tetapi menambah folder baru (`Readers/`), satu pengurai CSV tulisan tangan, satu controller baru, dan dua berkas statis yang isinya adalah keputusan desain (lihat bagian 7) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/Corporate/FinanceManagement/AccountingIntegration/**`, `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs`, `Storage/templates/finance/**`, `docs/module-blueprints/finance-management/**` |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | `0e256765563ef2693107e1f35e9c5a67476ca1f0` (belum ada commit baru dibuat pada sesi ini) |
| Tanggal | 2 Oktober 2026 |
| Status | 🟡 **Sebagian** — seluruh source dan kedua berkas templat CSV selesai; `dotnet build` **belum dijalankan** atas permintaan eksplisit pemilik task pada giliran ini |

---

## 1. Masalah yang diperbaiki

`BE-FIN-079` membuat **tempat penyimpanan** batch migrasi (`FinOpeningItemBatch`), tetapi belum ada
cara mengubah berkas yang diunggah petugas menjadi data yang dapat diperiksa sistem. Tanpa task ini,
petugas tidak punya:

1. **Bentuk berkas yang sah** untuk diisi — tanpa templat, petugas harus menebak kolom apa yang
   diharapkan, dan tebakan yang salah berarti seluruh baris ditolak tanpa penjelasan yang berguna.
2. **Cara membaca isi berkas** menjadi sesuatu yang dapat divalidasi — tanpa pengurai yang
   konsisten, dua pengembang dapat menulis dua logika pembacaan berbeda yang menghasilkan baris
   berbeda dari berkas yang sama persis.

Contoh konkret: tanpa templat kolom `SisaTagihan` yang jelas, petugas yang mengisi "Rp 2.500.000"
(dengan simbol mata uang) di satu baris dan "2500000" di baris lain menghasilkan dua bentuk data
yang sama-sama "benar" menurut berkasnya sendiri, padahal keduanya **harus** diperlakukan sama oleh
sistem. Templat yang menetapkan satu bentuk angka, dan pengurai yang memulangkan teks mentah apa
adanya (bukan menebak), adalah dua potongan yang saling melengkapi untuk mencegah masalah ini **sebelum**
sampai ke lapisan validasi (`BE-FIN-081`, belum dibangun).

Task ini **tidak** membangun validasi barisnya sendiri (jenis debitur dikenal, supplier ditemukan,
dst.) — itu `BE-FIN-081`. Yang dibangun di sini murni **jalur masuknya**: templat yang benar, dan
pengurai yang memulangkan isinya apa adanya beserta nomor barisnya.

---

## 2. Proses bisnis

1. Petugas memanggil `GET /opening-item-batches/template?itemKind=RECEIVABLE&format=CSV` (atau
   `SUPPLIER_PAYABLE`) dan mengunduh templat CSV yang sesuai.
2. Petugas mengisi templat itu di aplikasi spreadsheet apa pun yang dapat menyimpan CSV, lalu
   menyiapkannya untuk diunggah (unggahnya sendiri — `POST /opening-item-batches` — **belum**
   dibangun task ini; menyusul `BE-FIN-081`).
3. Ketika unggahnya kelak dibangun, `CsvOpeningItemFileReader` akan membaca berkas itu baris demi
   baris, memulangkan setiap baris sebagai `OpeningItemRawRow` — nomor baris persis seperti terlihat
   di aplikasi spreadsheet (baris header = baris 1, baris data pertama = baris 2), dan nilai tiap
   kolom sebagai **teks mentah**, apa adanya, tanpa ditebak atau diubah bentuknya.
4. Jalur tidak normal yang **sudah** ditangani pada task ini:
   - `format` kosong atau di luar `CSV`/`XLSX` → `GET /template` menjawab `400`
     (*"Pilih format templat: CSV atau XLSX."*, persis `FIN-VAL-224`).
   - `itemKind` kosong atau di luar `RECEIVABLE`/`SUPPLIER_PAYABLE` → `400` (pemeriksaan teknis
     tambahan, lihat bagian 7).
   - `format=XLSX` — nilai itu **sah** menurut kontrak, tetapi templat XLSX-nya belum ada (menunggu
     `BE-FIN-083`, terblokir `FIN-OQ-081`) → dijawab `503`, bukan `400`: artinya *nilai Anda benar,
     tetapi sistem belum siap memenuhinya* — pola yang sama persis dengan `FIN-VAL-220` pada bukti
     pembayaran.
   - Berkas CSV dengan sel yang dibungkus tanda kutip berisi koma atau baris baru di dalamnya
     dibaca **benar** (RFC 4180), bukan terpotong di koma pertama.
5. Jalur yang **belum** ada pada task ini, dan sengaja: membaca `format=CSV` yang baris-barisnya
   salah secara bisnis (jenis debitur tidak dikenal, dst.) — itu `BE-FIN-081`. Mengunggah berkas
   sungguhan sama sekali belum ada endpoint-nya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `02-backend-architecture.md` bagian M.1–M.8 (revisi 15, epik CSV/XLSX) — sumber utama bentuk
  `IOpeningItemFileReader`/`OpeningItemRawRow`/`CsvOpeningItemFileReader`, letak folder, dan batas
  tanggung jawab pengurai vs validasi
- `contracts/api-contract.md` F.2 — bentuk `GET /opening-item-batches/template`
- `contracts/validation-matrix.md` G.2 (`FIN-VAL-224`..`227`) dan F.4 (`FIN-VAL-186`..`191`,
  rujukan field baris piutang/utang)
- `contracts/permission-audit-matrix.md` — blok `[AccessController]` controller ini **sudah**
  dituliskan persis di sana (termasuk `SortOrder = 71`) sejak revisi 14/15; disalin apa adanya,
  bukan ditebak
- `Areas/Corporate/FinanceManagement/AccountingIntegration/Models/FinOpeningItemBatch.cs`
  (`BE-FIN-079`) — konstanta `FinOpeningItemBatchItemKinds`
- `Areas/Corporate/FinanceManagement/Collection/Controllers/FinanceTransactionProofsController.cs`
  dan `Services/FinanceTransactionProofService.cs` (`BE-FIN-074`/`075`) — pola `[AccessController]`,
  resolusi `IWebHostEnvironment.ContentRootPath`, dan gaya penanganan 503 fail-closed yang ditiru
- `Areas/Corporate/FinanceManagement/Payable/Models/FinSupplierPayableItem.cs`,
  `Receivable/Models/FinReceivableItem.cs`, `Administrator/MasterData/Models/MstSupplier.cs` —
  diperiksa untuk menetapkan kolom templat CSV yang realistis terhadap skema tujuan akhirnya
  (lihat bagian 7)
- `QuilvianSystemBackend.csproj` — dipastikan tidak ada pengecualian glob yang akan membuang
  `Storage/templates/finance/**`, dan dipastikan nol paket pembaca spreadsheet baru ditambahkan

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Readers/IOpeningItemFileReader.cs` | **Baru.** Antarmuka `CanRead(mediaType, extension)` dan `Read(stream) -> List<OpeningItemRawRow>` |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Readers/OpeningItemRawRow.cs` | **Baru.** DTO internal: `RowNumber` (nomor baris persis seperti di berkas asal), `Cells` (`IReadOnlyDictionary<string,string>`, teks mentah) |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Readers/CsvOpeningItemFileReader.cs` | **Baru.** Pengurai CSV RFC 4180 tulisan tangan (kutip ganda, escape `""`, CRLF/LF) — nol paket |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/DTOs/OpeningItemBatchDtos.cs` | **Baru.** `OpeningItemBatchTemplateQuery` (`ItemKind`, `Format`) |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Controllers/FinanceOpeningItemBatchesController.cs` | **Baru.** Satu endpoint: `GET /template` |
| `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` | `services.AddScoped<IOpeningItemFileReader, CsvOpeningItemFileReader>()` ditambah — pola `IEnumerable<IOpeningItemFileReader>` disiapkan untuk `BE-FIN-083` menambah `XlsxOpeningItemFileReader` tanpa mengubah baris ini |
| `Storage/templates/finance/opening-item-receivable.csv` | **Baru.** Templat CSV piutang — baris header saja (lihat bagian 7 untuk kolomnya) |
| `Storage/templates/finance/opening-item-supplier-payable.csv` | **Baru.** Templat CSV utang supplier — baris header saja |
| `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` | Ditandai 🟡 pada baris tabel dan grafik dependency `BE-FIN-080` (lihat luar laporan ini) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Satu endpoint baru**: `GET /opening-item-batches/template`. Bentuknya mengikuti `api-contract.md` F.2 persis untuk `format=CSV`; untuk `format=XLSX` kontrak menyebut nilai itu sah tetapi task ini menjawabnya `503` karena templat/pembacanya belum ada — delta ini dicatat eksplisit (bagian 7), bukan diam-diam menyimpang dari kontrak |
| Database | `NOT APPLICABLE` — **nol** migration, **nol** perubahan schema. Task ini murni kode dan dua berkas statis |
| Keamanan/Auth | `[AccessController]` dan `[AccessPermission("FinanceOpeningItemBatch", "Read")]` dipasang **persis** seperti sudah dituliskan `permission-audit-matrix.md` (ID, nama, `ControllerName`, `SortOrder = 71`) — disalin, bukan ditebak. Endpoint hanya membaca berkas statis yang sudah ditentukan sistem (dua nama berkas tetap, dipilih lewat `switch` atas nilai `itemKind` yang **sudah** divalidasi terhadap enum sebelum dipakai) — **nol** penggabungan string dari masukan pengguna ke dalam jalur berkas, sehingga **nol** permukaan path traversal |

---

## 4. Dokumentasi endpoint

#### Corporate / Finance Management / Opening Item Batch

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/template?itemKind=&format=` | Mengunduh templat CSV sesuai jenis item. `format=XLSX` menjawab `503` (templat belum ada); nilai lain pada kedua ruas menjawab `400` | `FinanceOpeningItemBatch : Read` |

Endpoint lain pada grup ini (`GET /`, `GET /{id}`, `POST /`, `/validate`, `/declare-accounting-opening`,
`/approve`, `/reject`) **belum dibangun** — menyusul `BE-FIN-081`/`082`, persis seperti dicatat
`api-contract.md` F.2 ("Rencana, belum tersedia").

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Permintaan eksplisit pemilik task pada giliran ini: "jangan lakukan build backend secara automatis" |
| Verifikasi kontrak API (`GET /template`) | Path, query param, kode status (`200`/`400`/`503`), dan `[AccessController]`/`[AccessPermission]` dibandingkan baris-per-baris terhadap `api-contract.md` F.2 dan `permission-audit-matrix.md` | `PASS` (pembacaan manual, bukan hasil `dotnet build`/runtime) | Bagian 3.2–4 di atas; `[AccessController]` disalin karakter-demi-karakter dari `permission-audit-matrix.md` baris 898–902 |
| Verifikasi proses bisnis penguraian CSV | Penelusuran manual (bukan automated test, mengikuti `TEST_POLICY.md`) atas lima skenario: (1) baris biasa, (2) berkas kosong, (3) templat tanpa baris data, (4) sel berkutip berisi koma (`"Jl. Sudirman, No 5"`), (5) kutip berenkode ganda (`"She said ""hi"""`) dan baris baru di dalam sel berkutip, termasuk kombinasi CRLF/LF | `PASS` untuk kelima skenario — hasil penguraian dan `RowNumber` dilacak tangan per karakter terhadap `ParseRecords`/`Read` | Penelusuran dituliskan selama pengerjaan (lihat bagian 7 untuk ringkasannya); **bukan** pengganti `dotnet build`/`dotnet run` sungguhan |
| Keseimbangan kurung kurawal pada seluruh berkas `.cs` baru/berubah | Sama | `PASS` | `grep -o "{"`/`"}"` per berkas: seluruhnya seimbang (lihat lampiran internal sesi) |
| Pemeriksaan `QuilvianSystemBackend.csproj` | **Nol** paket baru ditambahkan; **nol** aturan `Compile Remove`/`Content Remove` yang akan membuang `Storage/templates/finance/**` atau `Areas/.../Readers/**` | `PASS` | Pembacaan penuh `QuilvianSystemBackend.csproj` (dicatat bagian 3.1) |
| QBE preflight | Area `Corporate`, Module `FinanceManagement`, Submodule `AccountingIntegration` — prefix `Fin` **sudah terdaftar**; folder `Readers/` baru di dalam submodule yang sudah terdaftar, sehingga **nol** gerbang `QBE-MOD-003`. **Nol** entity database baru pada task ini, sehingga **nol** gerbang `QBE-MOD-002` | `PASS` | `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` (dibaca ulang dari sesi sebelumnya pada task ini, status tidak berubah) |
| Review diff/scope | Seluruh berkas yang berubah ditelusuri satu per satu; dipastikan **nol** sentuhan pada model/konfigurasi/migration `BE-FIN-079` | `PASS` | Tabel 3.2 |

Uji manual: `NOT FEASIBLE` pada sesi ini — menjalankan server dan memanggil endpoint sungguhan
menuntut `dotnet build`/`dotnet run`, keduanya sengaja tidak dijalankan.

**Tidak dijalankan:** `dotnet restore`/`dotnet build`/`dotnet run` (permintaan eksplisit); pengujian
otomatis (`rules/backend/TEST_POLICY.md` — tidak diminta eksplisit pada task ini).

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Antarmuka memulangkan baris terurai — nol `DataTable`, `Stream`, maupun tipe milik paket melewati batasnya | **Terpenuhi** | `IOpeningItemFileReader.Read` mengembalikan `List<OpeningItemRawRow>`; `CsvOpeningItemFileReader` hanya memakai `StreamReader`/`StringBuilder` bawaan .NET, keduanya tidak melewati batas antarmuka |
| Nilai sel dipulangkan sebagai teks mentah | **Terpenuhi** | `OpeningItemRawRow.Cells` bertipe `IReadOnlyDictionary<string,string>`; **nol** `Parse`/`TryParse` angka atau tanggal di dalam `CsvOpeningItemFileReader` (disengaja — lihat bagian 2 dan 7) |
| `format` di luar `CSV`/`XLSX` ditolak `400` | **Terpenuhi** | `FinanceOpeningItemBatchesController.GetTemplate`, pesan persis `FIN-VAL-224` |
| Baris CSV yang format angka/tanggalnya tidak sesuai templat ditolak beserta nomor barisnya, **MUST NOT** ditebak | **Terpenuhi sebagian — lihat catatan** | Pengurai **mempertahankan** `RowNumber` dan teks mentah apa adanya (prasyarat yang **MUST** ada), tetapi **penolakannya sendiri** (`FIN-VAL-226`) ada di lapisan validasi `BE-FIN-081` yang belum dibangun — bukan di pengurai ini (M.8 eksplisit melarang penguraian angka/tanggal di pembaca). Task ini menyediakan fondasi yang benar; perilaku tolak-nya sendiri menyusul |
| Build PASS | **Belum terpenuhi** | Sengaja `NOT RUN` atas permintaan eksplisit pemilik task pada giliran ini |
| Nol migration | **Terpenuhi** | Tabel 3.2 — nol berkas di `Migrations/` disentuh |
| Nol paket ditambahkan | **Terpenuhi** | `QuilvianSystemBackend.csproj` tidak diubah; `CsvOpeningItemFileReader` hanya memakai `System.Text`/`System.IO` bawaan |
| Laporan task tracked ada | **Terpenuhi** | Berkas ini |

**Kesimpulan status:** 🟡 **Sebagian**, bukan ✅. Satu butir DoD eksplisit (`dotnet build PASS`)
**belum terpenuhi** semata karena belum dijalankan atas instruksi eksplisit, bukan karena ditemukan
kegagalan. Status ini **wajib** ditinjau ulang ke ✅ setelah `dotnet build` dijalankan dengan hasil PASS.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Kolom templat CSV bukan keputusan yang tertulis verbatim di kontrak mana pun** — lihat baris berikutnya |
| Masalah yang diketahui | (1) **Kolom templat.** `validation-matrix.md` F.4 (`FIN-VAL-187`..`191`) menyebut *field*-nya ("jenis debitur", "supplier tidak ditemukan", "sisa tagihan", "tanggal dokumen", "nomor dokumen kembar") tetapi **tidak ada satu dokumen pun** yang mencantumkan nama kolom/urutan templat secara eksplisit. Kolom yang dipakai pada kedua templat — disusun langsung dari kelima aturan itu ditambah kolom `TanggalJatuhTempo`/`Keterangan` yang dibutuhkan skema `FinReceivable`/`FinSupplierPayable` tujuan akhirnya — adalah: piutang = `NomorDokumen, JenisDebitur, NamaDebitur, TanggalDokumen, TanggalJatuhTempo, SisaTagihan, Keterangan`; utang = `KodeSupplier, NomorInvoiceSupplier, TanggalInvoiceSupplier, TanggalJatuhTempo, SisaTagihan, Keterangan`. **Ini keputusan implementasi, bukan kontrak yang disetujui** — pemilik modul **MUST** meninjau kedua daftar kolom ini sebelum templat disebarkan ke petugas sungguhan, dan `BE-FIN-081` (lapisan validasi) **MUST** memverifikasi ulang kesesuaiannya saat membangun pemetaan baris->field. (2) **Status `format=XLSX` pada `GET /template`.** `FIN-VAL-224` hanya mengatur nilai `format` yang sah, bukan ketersediaan templatnya. Karena templat XLSX belum ada (`BE-FIN-083` `⛔ BLOCKED`), task ini menjawabnya `503` mengikuti pola `FIN-VAL-220` (nilai benar, sistem belum siap) — ini juga keputusan teknis tanpa rujukan tertulis eksplisit, didokumentasikan di sini supaya tidak disangka kontrak baku |
| Risiko tersisa | (1) **Penyebaran berkas statis.** `Storage/templates/finance/*.csv` adalah berkas sumber biasa (bukan `EmbeddedResource`, mengikuti desain `02-backend-architecture.md` M.5 yang eksplisit menyebutnya "berkas statis, bukan kode"). Task ini **tidak** memverifikasi bahwa proses publish/deployment (Docker image, dsb.) benar-benar menyalin folder `Storage/templates/` ke lingkungan produksi — itu wewenang/pemeriksaan operasional di luar cakupan task kode ini, dan **MUST** diperiksa sebelum endpoint ini dipakai di lingkungan nyata. (2) **Pengurai CSV tulisan tangan tidak pernah dikompilasi maupun dijalankan pada sesi ini** (`dotnet build` `NOT RUN`). Kebenarannya diverifikasi lewat penelusuran manual lima skenario (bagian 5) sebagai pengganti sementara, **bukan** pengganti kompilasi dan uji sungguhan — `dotnet build` **MUST** dijalankan sebelum task ini dianggap selesai sepenuhnya. (3) Kutip ganda yang muncul **di tengah** field tanpa membungkusnya sejak awal (CSV tidak baku) dapat diurai secara tak terduga — keterbatasan yang melekat pada pengurai ringan tanpa paket, diterima karena templat terkontrol dan bukan masukan dari pihak luar yang adversarial |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` — task ini dikerjakan tanpa jeda sejak diminta |
| Status Git | Lihat `git status --short` terkini; akumulasi task-task sebelumnya pada sesi ini (`BE-FIN-069` dst., termasuk `BE-FIN-079`) tetap belum di-commit, ditambah berkas task ini pada tabel 3.2. Nol `git add`/`git commit`/`git push` dilakukan pada task ini |
| Langkah berikutnya | (1) Pemilik modul meninjau kolom kedua templat CSV (bagian 7) sebelum disebarkan ke petugas. (2) Pemilik menjalankan `dotnet build` dan memperbarui status laporan ini ke ✅ bila PASS. (3) Lanjut `BE-FIN-081` (unggah, validasi baris, `FinanceOpeningItemBatchService`) hanya setelah diminta eksplisit — dan **MUST** menetapkan ulang (atau mengonfirmasi) kolom templat ini saat memetakan `Cells` ke field `FinReceivable`/`FinSupplierPayable` |
