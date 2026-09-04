# Laporan Perubahan Backend — `BE-BKC-019`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-019` |
| Judul | Endpoint entri charge dari katalog tarif |
| Slice | Entri manual katalog tarif + coverage per item (`BKC-DEC-059`–`062`, amendment 2 September 2026, blueprint revisi `0.5 approved`) |
| Roadmap | `docs/module-blueprints/billing-kasir/roadmap/backend-roadmap.md` § `BE-BKC-019` (baris 306–319) |
| Trace | `BKC-DEC-059`; `FR-BKC-002`, `FR-BKC-003`; `BIL-VAL-025`, `026`; acceptance `BIL-AT-025`, `026` (`testing/acceptance-test-matrix.md`) |
| Contract version | `BIL-API-0.4` (approved). Endpoint `POST /catalog-charges` sudah dispesifikasi sebagai "Rencana (belum tersedia)" pada `contracts/api-contract.md` amendment 2 September 2026 — task ini mengimplementasikan kontrak yang sudah dikunci tersebut, bukan mendefinisikan baru |
| Dependency | `BE-BKC-018` (kolom `TariffId` pada `BilInvoiceItem`, `SourcePolicies["ADHOC_CATALOG"]`) — **sudah diimplementasikan pada sesi yang sama**, lihat `task/report/backend/BE-BKC-018.md` |
| Klasifikasi | `MEDIUM` — skor 5 (repo 0, berkas diperiksa 1, berkas diubah 1, logika bisnis 1, kontrak API 1, database 0, keamanan 1, UI/workflow 0) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/BillingManagement/Billing/{Dtos,Services,Controllers}`, `QuilvianSystemBackend.Tests/BillingManagement/`, `docs/module-blueprints/billing-kasir/` |
| Model | Claude Sonnet 5 |
| Governance Preflight | Area `HealthServices`; Module/pemilik `BillingManagement / Billing`; prefix `Bil` (registry `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, Lifecycle `ACTIVE`); keberlakuan `NEW CODE` (endpoint dan method baru; tidak ada entity operasional baru — reuse penuh `BilInvoiceItem`/`BilInvoice` milik `BE-BKC-018`) |
| QBE ID berlaku | `QBE-SVC-001` (orkestrasi di Module Service, controller tidak akses `DbContext` langsung), `QBE-API-001` (boundary API/response/status yang sudah mapan — meniru `AddOtherCharge`), `QBE-PERM-001` (reuse permission `BillingInvoice:Create`), `QBE-DTO-001` (DTO baru, tidak membocorkan entity EF), `QBE-VAL-001` (validasi tarif aktif+efektif), `QBE-CODE-002` (controller tidak mengalokasikan nomor bisnis — nomor invoice tetap lewat `BillingNumberSeriesService` yang sudah ada) |
| Commit backend saat dikerjakan | `fec3579` |
| Tanggal | 3 September 2026 |
| Status | Source lengkap, build **terkonfirmasi lulus (`exit code 0`)** untuk revisi sebelum tiga penyesuaian terakhir (lihat § 5). Tiga penyesuaian terakhir (determinisme `SourceDetailId`, perbaikan tabrakan `SortOrder`, empat test baru) **belum diverifikasi ulang oleh sesi ini** — pengguna mengambil alih menjalankan build/test secara manual. Belum boleh ditandai selesai sampai hasil build/test final dikonfirmasi |

---

## 1. Masalah yang diperbaiki

Sebelum task ini, satu-satunya cara kasir mencatat biaya di luar order klinis otomatis adalah lewat
"Biaya Lain-Lain" (`POST /other-charges`) — di sana kasir **mengetik sendiri** nama biaya dan
harganya. Ini cocok untuk biaya benar-benar bebas (misal fotokopi, materai), tapi tidak cocok untuk
menagih dari daftar tarif resmi rumah sakit (`MstTariff`): tidak ada jaminan harga yang diketik
kasir sama dengan tarif resmi, dan kasir bisa saja salah ketik atau — dalam skenario terburuk —
sengaja memasukkan harga lebih rendah dari yang seharusnya.

Task ini menutup celah itu dengan endpoint baru yang **hanya menerima pilihan tarif dan
kuantitasnya** — harga, kategori, dan deskripsi seluruhnya ditentukan server dari `MstTariff`, sama
sekali tidak bisa dikirim atau dimanipulasi dari sisi client. Contoh konkret: kasir memilih
"Konsultasi Spesialis" dari dropdown yang menampilkan harga Rp150.000, submit tanpa field harga
sama sekali — baris tagihan yang tercatat dijamin bernilai Rp150.000 persis, sama dengan
`MstTariff.NormalPrice` pada saat itu, bukan angka yang bisa disetel client.

---

## 2. Proses bisnis

**Tujuan**: kasir/penguji menambah item invoice dari katalog tarif resmi dengan harga yang
sepenuhnya ditentukan server.

**Pelaku**: kasir (lewat frontend `FE-BKC-014`, belum dikerjakan) atau penguji API langsung.

**Pemicu**: kasir memilih satu tarif dari picker (yang nantinya di `FE-BKC-014` sudah disaring
memakai `ServiceUnitId`/`ClinicId`/`PatientClassId` milik kunjungan — lihat `BE-BKC-018`) dan
mengisi kuantitas.

**Langkah**:

1. Client mengirim `POST /catalog-charges` berisi `EncounterId`, `TariffId`, `Quantity`, dan
   sepasang `CorrelationId`/`CausationId`, disertai header `Idempotency-Key`. **Tidak ada field
   harga, nama, atau kategori** pada request ini — beda mendasar dari `AddOtherChargeRequest`.
2. Server mencari `MstTariff` dengan `Id` yang cocok, **dan** harus **aktif** (`IsActive=true`,
   tidak dihapus/dibatalkan) **dan** sedang **efektif** — `EffectiveStartDate` sudah lewat (atau
   kosong) dan `EffectiveEndDate` belum lewat (atau kosong). Bila tidak ditemukan yang memenuhi
   ketiganya (tarif salah Id, dinonaktifkan, atau di luar masa berlaku), permintaan ditolak `422`
   dengan pesan "Tarif tidak ditemukan, tidak aktif, atau sudah kedaluwarsa." — tidak ada baris
   tagihan yang tersimpan.
3. Bila tarif valid, server membangun charge dengan `SourceDomain="ADHOC_CATALOG"`,
   `UnitPrice=MstTariff.NormalPrice`, `CategoryId=MstTariff.TariffCategoryId`,
   `DescriptionSnapshot=MstTariff.TariffName`, dan `TariffId=MstTariff.Id` — lalu meneruskannya ke
   `UpsertChargeAsync` yang sudah ada (mesin idempotensi, locking, dan pembuatan/pemakaian invoice
   yang sama dipakai penuh, tidak ditulis ulang).
4. `UpsertChargeAsync` membuat invoice baru (bila kunjungan ini belum punya invoice `OPEN`) atau
   menambah baris pada invoice yang sudah ada, lalu menyimpan `BilChargeReceipt` untuk keperluan
   replay idempotency.
5. Bila `Idempotency-Key` yang sama dikirim ulang (misal karena retry jaringan) dengan payload
   identik, permintaan kedua **tidak membuat baris tagihan baru** — mengembalikan hasil yang sama
   persis (`IsReplay=true`). Bila `Idempotency-Key` baru dipakai untuk permintaan yang sama sekali
   berbeda (tarif/kuantitas berbeda), itu dianggap entri baru dan menambah baris kedua.

**Aturan yang berlaku**: `BIL-VAL-025`/`026` — harga dan deskripsi tidak boleh berasal dari client;
tarif harus aktif dan berada dalam masa berlakunya.

**Status yang dihasilkan**: baris tagihan baru berstatus `ADDED` pada invoice yang tetap `OPEN`
(tidak ada perubahan status invoice pada langkah ini).

**Jalur tidak normal**: `EncounterId` tidak ditemukan/sudah dibatalkan → `404`. Invoice sudah tidak
`OPEN` (sudah final) → `422` ("Invoice final tidak dapat diedit; ajukan adjustment."). Kategori
tarif nonaktif pada saat charge dibuat → `422` (validasi kategori yang sudah ada pada
`UpsertChargeAsync`, dipakai apa adanya). `Idempotency-Key` dipakai ulang untuk payload berbeda →
`409`.

**Hasil akhir**: baris tagihan tercatat pada invoice kunjungan, dengan `TariffId` terisi sehingga
bisa ditelusuri balik ke tarif resmi asalnya — memenuhi `BIL-AT-025`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`docs/module-blueprints/billing-kasir/{roadmap/backend-roadmap.md, contracts/api-contract.md,
testing/acceptance-test-matrix.md}`; `Areas/HealthServices/BillingManagement/Billing/
{Services/BillingInvoiceService.cs (AddOtherChargeAsync, UpsertChargeAsync, ApplySource,
ValidateRequest), Dtos/BillingInvoiceDtos.cs (AddOtherChargeRequest, UpsertChargeRequest),
Controllers/BillingInvoicesController.cs (AddOtherCharge, seluruh `SortOrder` yang terpakai)}`;
`Areas/HealthServices/MasterData/Models/MstTariff.cs`; `Repositories/ApplicationDbContext.cs`
(`DbSet<MstTariff> MstTariffs`); `QuilvianSystemBackend.Tests/BillingManagement/
BillingInvoiceServiceTests.cs` (pola `SeedAsync`, `Encounter`, `Request`, `CreateService`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/.../Billing/Dtos/BillingInvoiceDtos.cs` | DTO baru `AddCatalogChargeRequest` (`EncounterId`, `TariffId`, `Quantity`, `CorrelationId`, `CausationId` — sengaja tanpa field harga/nama). `UpsertChargeRequest` diperluas `Guid? TariffId` (opsional, default `null`, pemanggil lama tidak berubah) |
| `Areas/.../Billing/Services/BillingInvoiceService.cs` | Method baru `AddCatalogChargeAsync` (lookup `MstTariff` aktif+efektif, bangun `UpsertChargeRequest` dengan `SourceDomain="ADHOC_CATALOG"`); `ApplySource` menyalin `TariffId` dari request ke `BilInvoiceItem` (berlaku untuk create maupun update path) |
| `Areas/.../Billing/Controllers/BillingInvoicesController.cs` | Action baru `POST /catalog-charges` (`AddCatalogCharge`) — pola identik `AddOtherCharge`: `[AccessAction("Create", ..., SortOrder=13)]`, `[AccessPermission("BillingInvoice","Create")]`, catch `KeyNotFoundException`/`BillingInvoiceConflictException`/`BillingInvoiceValidationException` |
| `QuilvianSystemBackend.Tests/BillingManagement/BillingInvoiceServiceTests.cs` | 4 test baru (§ 5) + helper `Tariff(...)`/`CatalogChargeRequest(...)` |
| `docs/module-blueprints/billing-kasir/contracts/api-contract.md` | Status baris `POST /catalog-charges` diperbarui dari "Rencana (belum tersedia)" menjadi "Diimplementasikan (backend, belum diverifikasi manual)"; catatan tanggal ditambahkan pada amendment 2 September 2026 |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Endpoint baru `POST /catalog-charges` — sudah dispesifikasi sebelumnya di `contracts/api-contract.md` (status berubah dari "Rencana" menjadi "Diimplementasikan"). Tidak mengubah endpoint yang sudah ada |
| Database | `NOT APPLICABLE` — tidak ada migration baru; reuse penuh kolom `TariffId` yang sudah ditambahkan `BE-BKC-018`. Sesuai DoD "DB tidak disentuh" |
| Keamanan/Auth | Reuse permission `BillingInvoice : Create` yang sudah ada (permission yang sama dipakai `POST /from-source` dan `POST /other-charges`) — tidak ada permission baru. `[AccessAction]`/`[AccessPermission]` dipasang sesuai `role-access-rules.md` supaya kemampuannya muncul di layar Akses Role |

---

## 4. Dokumentasi endpoint

#### Health Services / Billing Management / Billing / Invoices

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/catalog-charges` | Tambah baris tagihan dari katalog tarif resmi (`MstTariff`) — harga, kategori, dan deskripsi ditentukan server, klien hanya memilih tarif dan kuantitas (`BKC-DEC-059`) | `BillingInvoice : Create` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -t:Compile` (setelah DTO + `AddCatalogChargeAsync` + action controller ditambahkan) | Berhasil | `PASS` | Tool selesai dengan `exit code 0` |
| `dotnet build QuilvianSystemBackend.csproj -t:Compile` (setelah propagasi `TariffId` ke `ApplySource` ditambahkan) | Berhasil | `PASS` | Tool selesai dengan `exit code 0` |
| `dotnet build`/`dotnet test` **setelah** perbaikan determinisme `SourceDetailId`, perbaikan tabrakan `SortOrder` (`12`→`13`), dan 4 test baru ditambahkan | **Belum diverifikasi ulang oleh sesi ini** | `NOT RUN` | Dua percobaan build latar belakang yang dijalankan bersamaan pada project yang sama saling bertabrakan (macet, tidak selesai); pengguna kemudian mengambil alih menjalankan build/test secara manual (lihat § 7 *Interupsi*) |
| 4 test baru: `CatalogChargeUsesServerSidePriceCategoryAndTariffIdFromActiveTariff`, `CatalogChargeRejectsInactiveTariff`, `CatalogChargeRejectsExpiredTariff`, `CatalogChargeIdempotencyReplayIsNoOpForSameKeyButNewKeyAddsAnotherItem` | Ditulis, **belum dijalankan** pada revisi final (lihat baris di atas) | `NOT RUN` | Kode test ada di `BillingInvoiceServiceTests.cs`; diverifikasi lewat pembacaan source (bukan eksekusi) bahwa nama field/tipe yang dipakai cocok dengan DTO sebenarnya |

Uji manual: `NOT FEASIBLE` pada sesi ini — endpoint baru, belum ada frontend (`FE-BKC-014` belum
dikerjakan) untuk klik-coba; verifikasi lewat Swagger/Postman terautentikasi belum dilakukan.

**Tidak dijalankan:** eksekusi database (`NOT APPLICABLE` — task ini tidak menyentuh database);
verifikasi manual lewat Swagger; sanitasi contoh Swagger untuk endpoint baru ini (di luar scope
task, belum ada task tersendiri untuk itu).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `BIL-AT-025` — kasir pilih tarif Rp150.000, submit tanpa field harga; `BilInvoiceItem.UnitPrice = MstTariff.NormalPrice` persis; `TariffId` terisi; `SourceDomain="ADHOC_CATALOG"` | Terpenuhi (source + test, **belum dijalankan pada revisi final** — lihat § 5) | `CatalogChargeUsesServerSidePriceCategoryAndTariffIdFromActiveTariff`; `AddCatalogChargeRequest` tidak punya field harga sama sekali |
| `BIL-AT-026` — tarif `TariffId` valid tapi `IsActive=false` atau di luar `EffectiveEndDate` ditolak `422` dengan `BIL-VAL-025`; tidak ada `BilInvoiceItem` tersimpan | Terpenuhi (source + test, **belum dijalankan pada revisi final**) | `CatalogChargeRejectsInactiveTariff`, `CatalogChargeRejectsExpiredTariff` |
| Reuse `BillingInvoiceService.UpsertChargeAsync` dipakai penuh (idempotensi/locking/invoice-upsert) | Terpenuhi | `AddCatalogChargeAsync` memanggil `UpsertChargeAsync` yang sudah ada, tidak menulis ulang logikanya |
| DoD: Endpoint + tests + Swagger sesuai `contracts/api-contract.md`; build lulus; DB tidak disentuh | **Belum sepenuhnya terpenuhi** — endpoint dan test sudah ada dan cocok kontrak; build/test **belum dikonfirmasi lulus pada revisi final** (§ 5); Swagger belum diverifikasi manual; DB memang tidak disentuh | Lihat § 5 dan § 7 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Ditemukan bug desain pada draf pertama: `SourceDetailId` awalnya di-generate acak (`Guid.NewGuid()`) di setiap panggilan `AddCatalogChargeAsync`. Karena `UpsertChargeAsync` mencocokkan replay lewat `(SourceDomain, SourceDetailId)`, ini akan membuat **setiap retry dengan `Idempotency-Key` yang sama tetap dianggap konflik**, bukan di-replay — idempotensi rusak secara diam-diam. Diperbaiki dengan menurunkan `SourceDetailId` dari `idempotencyKey` itu sendiri (`idempotencyKey.ToString("N")`), sehingga retry dengan key yang sama menghasilkan `SourceDetailId` yang sama pula. Pola lama yang sama (`Guid.NewGuid()` acak) masih ada di `AddOtherChargeAsync` yang sudah lama berjalan — **tidak diperbaiki pada task ini** karena di luar wewenang tulis `BE-BKC-019` (lihat baris *Masalah yang diketahui*) |
| Masalah yang diketahui | 1) Pola `SourceDetailId` acak yang sama pada `AddOtherChargeAsync` (lihat *Peringatan*) berpotensi membuat retry `POST /other-charges` yang genuine salah dianggap konflik `409` alih-alih di-replay — belum diperbaiki, di luar scope task ini. 2) Task ini **tidak** memvalidasi bahwa `TariffId` yang dikirim client konsisten dengan `ServiceUnitId`/`ClinicId`/`PatientClassId` milik `EncounterId`-nya (scoping ganda tarif, `BKC-DEC-061`) — sesuai scope roadmap yang eksplisit hanya menyebut "lookup aktif+efektif", disambiguasi tarif diserahkan ke picker frontend (`FE-BKC-014`, belum dikerjakan). Ini adalah risiko yang sudah dicatat roadmap sendiri (baris *Risiko/pemilik*), bukan temuan baru |
| Risiko tersisa | Tidak ada validasi cross-check tarif-vs-konteks-kunjungan di backend (lihat poin 2 di atas) — bila `FE-BKC-014` gagal menyaring dengan benar, backend akan tetap menerima kombinasi tarif/kunjungan yang secara bisnis tidak seharusnya cocok, selama tarifnya sendiri aktif dan efektif |
| Perubahan sampingan | `NONE` |
| Interupsi | Dua percobaan `dotnet build` yang dijalankan berurutan-tapi-tumpang-tindih di latar belakang (pada project yang sama) saling bertabrakan dan macet tanpa menghasilkan output — kesalahan proses pada sesi ini, bukan kegagalan build yang sesungguhnya. Pengguna kemudian secara eksplisit mengambil alih ("background build saya lakukan secara manual saja") untuk menjalankan build/test secara manual. Task **belum ditandai selesai** sampai hasil build/test final dikonfirmasi — lihat § 5 dan Langkah berikutnya |
| Status Git | `On branch Yasmina, up to date with 'origin/Yasmina'`. Modified: `Areas/HealthServices/BillingManagement/Billing/{Controllers/BillingInvoicesController.cs, Dtos/BillingInvoiceDtos.cs, Models/BilInvoiceItem.cs, Services/BillingChargeSourceAdapter.cs, Services/BillingInvoiceService.cs}`, `Migrations/ApplicationDbContextModelSnapshot.cs`, `QuilvianSystemBackend.Tests/BillingManagement/BillingInvoiceServiceTests.cs`, `Repositories/Configurations/.../BilInvoiceItemConfiguration.cs`, `docs/module-blueprints/billing-kasir/contracts/api-contract.md`. Untracked: `Migrations/20260903015730_AddTariffIdToBilInvoiceItem.{cs,Designer.cs}`, `QuilvianSystemBackend.Tests/BillingManagement/BillingChargeSourceAdapterTests.cs`, `docs/module-blueprints/billing-kasir/task/report/backend/{BE-BKC-018.md,BE-BKC-019.md}`. Belum staged/commit — perubahan `BE-BKC-018` dan `BE-BKC-019` berbagi working tree yang sama pada sesi ini |
| Langkah berikutnya | 1) **Konfirmasi hasil `dotnet build`/`dotnet test` final** dari pengguna sebelum task ini ditandai selesai — bila ada kegagalan baru (bukan 2 kegagalan pra-eksisting yang sudah didokumentasikan di `BE-BKC-018.md` § 5), laporkan agar diperbaiki. 2) Verifikasi manual lewat Swagger/Postman terautentikasi. 3) Lanjut `BE-BKC-020` (endpoint preview coverage, independen dari task ini — boleh paralel) atau `FE-BKC-014` (picker tarif frontend, dependen pada `BE-BKC-018`+`019`) |
