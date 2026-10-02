# Laporan Perubahan Backend — `BE-BKC-FIX-010`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-FIX-010` (ad-hoc, di luar roadmap, permintaan langsung pengguna lewat `/grill-me` → `/build-module-backend`) |
| Judul | Hapus kolom `TaxableCategory` dari `MstTaxRule` — model/DTO/service/controller, kolom fisik database dipertahankan sebagai orphan |
| Slice | Perbaikan/pembersihan master data atas entity yang sudah ada (`MstTaxRule`), bukan fitur baru |
| Roadmap | `NOT APPLICABLE` — keputusan ad-hoc, dikunci lewat amendment decision log, bukan task roadmap terjadwal |
| Trace | `BKC-DEC-098`, `BKC-DEC-099` (`00-interview-decisions.md`, amendment 16 September 2026) |
| Contract version | `BIL-API-1.0` — **breaking change disengaja**: field `taxableCategory` dihapus dari `TaxRuleQuery` (query filter), `CreateTaxRuleRequest`/`UpdateTaxRuleRequest` (request body), `TaxRuleResponse`, `TaxRuleOptionResponse`, `TaxRuleDefaultFilterResponse`, `TaxRuleFilterMetadataResponse.TaxableCategories`, dan query param `taxableCategory` pada `GET /options` dihapus dari `TaxRulesController`. Konsumen (frontend) yang masih mengirim/mengharapkan field ini akan diabaikan (request)/tidak menerima field itu (response) — bukan error, field cuma tidak ada lagi. |
| Backend Governance Preflight | Area `HealthServices`, Module `BillingManagement` (`Bil`), Submodule `MasterData` (`Mst`) — terdaftar `ACTIVE` (`MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 14 & 20). Keberlakuan: `TOUCHED LEGACY` (menghapus properti pada entity/DTO/service yang sudah ada; bukan `NEW CODE`, bukan `LEGACY MIGRATION` — tidak ada rename/pemindahan struktur fisik). Tidak ada QBE ID `MUST` yang berlaku wajib untuk penghapusan properti biasa pada entity existing. |
| Dependency | Tidak ada — `TaxableCategory` sudah dikonfirmasi (audit read-only sebelum keputusan) tidak dipakai gerbang PPN mana pun (`BKC-DEC-078`/`079` berbasis `item.IsPharmacy` + `BilInvoice.ServiceType`, bukan `MstTaxRule.TaxableCategory`) |
| Klasifikasi | `LOW` — penghapusan field yang sudah dikonfirmasi tidak punya konsekuensi kalkulasi, nol migration, nol tabel/kolom database baru |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Models/MstTaxRule.cs`, `Configurations/MstTaxRuleConfiguration.cs`, `Dtos/TaxRuleDtos.cs`, `Services/TaxRuleService.cs`, `Controllers/TaxRulesController.cs`, komentar `Billing/Services/BillingCalculationService.cs`. Frontend **TIDAK disentuh** — di luar wewenang tulis skill ini, menunggu invocation `build-module-frontend` terpisah. |
| Model | Claude Sonnet 5 |
| Tanggal | 16 September 2026 |
| Status | Source selesai. `dotnet build`/`test` **TIDAK dijalankan** (instruksi baku pengguna — build backend dijalankan manual). **Belum diverifikasi hidup** — menunggu rebuild pengguna |

---

## 1. Masalah

Pengguna (module owner) menanyakan makna kolom `TaxableCategory` pada `MstTaxRule`. Audit
read-only membuktikan kolom ini sudah tidak lagi punya konsekuensi kalkulasi pajak apa pun sejak
`BKC-DEC-078`/`079` mengubah basis gerbang PPN menjadi `item.IsPharmacy` + `BilInvoice.ServiceType`
(rawat jalan vs rawat inap) — bukan lagi kategori pada tax rule (komentar eksplisit lama
`BillingCalculationService.cs:790-793`: "Isi TaxableCategory kini murni label bagi pengguna dan
tidak memengaruhi perhitungan sama sekali"). Satu-satunya pemakaian aktif yang tersisa sebelum
task ini: (1) label/filter UI, (2) overlap-check periode — dua tax rule aktif dengan
`TaxableCategory` sama tidak boleh periodenya tumpang tindih.

Keputusan penghapusan dikunci lewat `/grill-me` (amendment pass terhadap blueprint `billing-kasir`
yang sudah `APPROVED`) menjadi `BKC-DEC-098` (hapus total dari model/DTO/service/frontend, kolom
fisik database dipertahankan sebagai orphan, tanpa migration) dan `BKC-DEC-099` (overlap-check
periode jadi global, tanpa kategori — karena sistem sudah membatasi hanya SATU tax rule aktif
secara global sepanjang waktu di `LoadInvoiceTaxRuleAsync`, sehingga overlap-check per-kategori
sudah redundant).

**Ini bukan keputusan bisnis baru yang dibuat sepihak oleh agent** — kedua keputusan di atas
disetujui eksplisit oleh Product/Domain Owner lewat wawancara terstruktur sebelum implementasi
dimulai.

---

## 2. Perubahan yang dikerjakan

### 2.1 `Models/MstTaxRule.cs`

Properti `TaxableCategory` (`[Required, MaxLength(30)]`) dihapus dari entity. Kolom fisik
`TaxableCategory` di tabel `MstTaxRule` pada database **tetap ada** — tidak ada migration yang
dibuat/dijalankan (`BKC-DEC-098`), tapi sejak commit ini EF Core tidak lagi memetakan kolom
tersebut sama sekali (orphan).

### 2.2 `Configurations/MstTaxRuleConfiguration.cs`

- Baris `entity.Property(x => x.TaxableCategory).HasMaxLength(30).IsRequired();` dihapus.
- Index gabungan `(TaxableCategory, EffectiveFrom, EffectiveTo, IsActive, IsDelete)` diganti jadi
  `(EffectiveFrom, EffectiveTo, IsActive, IsDelete)` — tanpa kolom kategori, menopang overlap-check
  global (`BKC-DEC-099`). **Catatan penting**: ini adalah perubahan definisi index di kode; karena
  migration tidak dibuat/dijalankan sesuai `BKC-DEC-098`, index fisik lama di database **belum
  berubah** — model EF Core dan schema database akan berbeda (drift) sampai migration terpisah
  dibuat dan dijalankan dengan otorisasi eksplisit. Ini konsekuensi yang disadari, bukan bug.

### 2.3 `Dtos/TaxRuleDtos.cs`

Field `TaxableCategory`/`TaxableCategories` dihapus dari: `TaxRuleQuery` (filter query),
`CreateTaxRuleRequest` (diwarisi `UpdateTaxRuleRequest`), `TaxRuleResponse`, `TaxRuleOptionResponse`,
`TaxRuleDefaultFilterResponse`, dan `TaxRuleFilterMetadataResponse.TaxableCategories`.

### 2.4 `Services/TaxRuleService.cs`

- `GetPagedAsync`: filter `request.TaxableCategory` dihapus; urutan hasil yang sebelumnya
  `OrderBy(TaxableCategory).ThenByDescending(EffectiveFrom)` diganti
  `OrderByDescending(EffectiveFrom).ThenBy(Code)` (tidak ada keputusan bisnis soal urutan tampilan
  daftar sebelumnya — dipilih urutan yang paling relevan secara operasional: rule paling baru
  efektif tampil dulu).
- `GetOptionsAsync`: parameter `taxableCategory` dihapus dari signature; filter dan pemetaan
  kategori ikut dihapus; urutan diganti `OrderBy(Name)`.
- `GetFilterMetadataAsync`: query `Distinct()` kategori dihapus total (sebelumnya baca langsung
  dari data, bukan konstanta — komentar penjelasnya juga dihapus karena sudah tidak relevan);
  method diringkas jadi sinkron (`Task.FromResult`, tidak ada lagi query database).
- `CreateAsync`/`UpdateAsync`: assignment `TaxableCategory = values.Category` dihapus.
- `ActivateAsync`: kondisi overlap `x.TaxableCategory == entity.TaxableCategory` dihapus dari query
  — overlap-check jadi global (`BKC-DEC-099`); pesan error diperbarui ("tumpang tindih periodenya"
  tanpa menyebut kategori).
- `ValidateAsync`: parsing/validasi `TaxableCategory` dihapus dari return tuple; kondisi overlap di
  `AnyAsync` create/update juga dihapus filter kategorinya (overlap-check global, konsisten dengan
  `ActivateAsync`); pesan error diperbarui.
- `AuditAsync`/`Map`: `entity.TaxableCategory` dihapus dari payload audit dan dari mapping response.

### 2.5 `Controllers/TaxRulesController.cs`

`GET /options`: parameter query `[FromQuery] string? taxableCategory` dihapus dari action
`GetOptions`, signature panggilan ke `_service.GetOptionsAsync` disesuaikan.

### 2.6 `Billing/Services/BillingCalculationService.cs`

Komentar penjelas di atas `LoadInvoiceTaxRuleAsync` diperbarui — sebelumnya menjelaskan
`TaxableCategory` "kini murni label", sekarang mencatat bahwa kolomnya sudah dihapus dari model
(`BKC-DEC-098`/`099`) karena sudah tidak punya konsekuensi kalkulasi apa pun. **Tidak ada
perubahan logika** pada file ini — method ini sejak awal tidak pernah membaca `TaxableCategory`.

**Yang sengaja TIDAK diubah**: gerbang PPN rawat jalan/rawat inap (`BKC-DEC-078`/`079`), alokasi
PPN `PROPORTIONAL` (`BKC-DEC-077`), field lain pada `MstTaxRule` (`Code`, `Name`, `Rate`,
`RoundingMode`, `AllocationRule`, `EffectiveFrom/To`, `IsActive`), dan endpoint/route lain pada
`TaxRulesController` (`GET`, `POST`, `PUT`, `DELETE`, `deactivate`, `activate`, `summary`,
`filters/metadata`).

---

## 3. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| Seluruh referensi `TaxableCategory` pada source non-migration | `Grep` ulang setelah edit: sisa hanya komentar eksplisit di `MstTaxRuleConfiguration.cs` dan `BillingCalculationService.cs` yang menjelaskan keputusan ini — nol properti/identifier `.TaxableCategory` yang masih dipanggil di kode | `PASS` (tinjauan kode) |
| Konsistensi tuple return `ValidateAsync` dengan pemanggilnya (`CreateAsync`/`UpdateAsync`) | Tuple `(Code, Name, RoundingMode, AllocationRule)` — kedua pemanggil hanya membaca `values.Code/Name/RoundingMode/AllocationRule`, tidak ada sisa referensi ke `values.Category` | `PASS` (tinjauan kode) |
| Signature `GetOptionsAsync` (service) selaras dengan pemanggil (`TaxRulesController.GetOptions`) | Kedua sisi disunting bersamaan: `(bool onlyActive, string? search, CancellationToken)` | `PASS` (tinjauan kode) |
| Overlap-check global konsisten antara `ValidateAsync` dan `ActivateAsync` | Kedua query `AnyAsync` sama-sama hanya memeriksa `IsActive`, `Id != excluded/entity.Id`, dan rentang periode — tidak ada satu pun yang masih memfilter kategori | `PASS` (tinjauan kode) |
| `MstTaxRuleConfiguration.cs` tidak lagi mereferensikan properti yang sudah dihapus dari entity | Baris `entity.Property(x => x.TaxableCategory)` dan index lama dihapus bersamaan dengan penghapusan properti di `MstTaxRule.cs` — tidak ada referensi silang yang tertinggal | `PASS` (tinjauan kode) |

**`AUTOMATED TEST: BLOCKED`** — `dotnet build`/`test` tidak dijalankan (instruksi baku pengguna).
**`MANUAL TEST: BLOCKED`** — belum di-rebuild pengguna, belum ada panggilan nyata ke endpoint
`TaxRulesController` pasca-perubahan.

---

## 4. Risiko dan catatan penutup

| Hal | Isi |
| --- | --- |
| Belum diverifikasi hidup | Seluruhnya tinjauan kode — `dotnet build` belum dijalankan sama sekali sejak perubahan ini, jadi kemungkinan kesalahan sintaks/referensi yang lolos tinjauan manual belum tertutup |
| Model EF vs schema database drift | Disengaja dan disetujui (`BKC-DEC-098`): kolom fisik `TaxableCategory` dan index lama yang menyertakannya masih ada di database, tapi model EF Core sudah tidak lagi mengenalinya. Migration untuk menyinkronkan (atau memutuskan tidak perlu disinkronkan) adalah wewenang terpisah yang belum diminta |
| Breaking API contract disengaja | Field `taxableCategory` hilang dari request/response `TaxRulesController` seluruhnya dan query param `taxableCategory` pada `GET /options` dihapus. Frontend (`tax-rule-editor-config.jsx`, `use-tax-rule-editor.js`, `tax-rule-view.jsx`) **belum disesuaikan** — masih mengirim/menampilkan field ini sampai task frontend terpisah dikerjakan |
| Frontend belum disentuh | Sengaja di luar scope task ini (skill `build-module-backend` hanya berwewenang backend). Sampai frontend disesuaikan, form create/update tax rule di UI kemungkinan masih menampilkan field yang sudah tidak diterima backend — request `POST`/`PUT` yang menyertakan `taxableCategory` akan diabaikan begitu saja (bukan error, karena DTO tidak lagi punya field itu) |
| Perubahan sampingan | `NONE` — nol tabel/kolom database baru, nol endpoint baru dihapus (hanya satu query param dihapus dari endpoint existing) |
| Status Git | Modified (backend): `Areas/HealthServices/BillingManagement/Billing/Services/BillingCalculationService.cs`, `Areas/HealthServices/BillingManagement/MasterData/Controllers/TaxRulesController.cs`, `Areas/HealthServices/BillingManagement/MasterData/DTOs/TaxRuleDtos.cs`, `Areas/HealthServices/BillingManagement/MasterData/Models/MstTaxRule.cs`, `Areas/HealthServices/BillingManagement/MasterData/Services/TaxRuleService.cs`, `Repositories/Configurations/HealthServices/BillingManagement/MasterData/MstTaxRuleConfiguration.cs`, `docs/module-blueprints/billing-kasir/00-interview-decisions.md`. Belum staged/commit |
| Langkah berikutnya | (1) Pengguna menjalankan `dotnet build`/`dotnet test` backend secara manual; (2) jalankan `build-module-frontend` untuk menyesuaikan `tax-rule-editor-config.jsx`/`use-tax-rule-editor.js`/`tax-rule-view.jsx` (hapus field dari form/tampilan, sesuai `BKC-DEC-098`); (3) keputusan terpisah: apakah migration untuk index perlu dibuat sekarang atau ditunda; (4) update roadmap/`requirement-traceability.md` modul `billing-kasir` mencatat `BE-BKC-FIX-010` |
