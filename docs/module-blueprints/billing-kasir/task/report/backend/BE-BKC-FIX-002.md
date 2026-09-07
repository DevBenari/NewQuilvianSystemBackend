# Laporan Perubahan Backend — `BE-BKC-FIX-002`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-FIX-002` (ad-hoc bug fix — bukan task roadmap bernomor `BE-BKC-0xx`, dibuat sendiri untuk menjaga jejak laporan tetap tracked) |
| Judul | Coverage asuransi tidak pernah benar-benar diterapkan ke item invoice mana pun — dua root cause terpisah (data + pencocokan rule) |
| Slice | Ditemukan lewat investigasi laporan bug pengguna atas badge coverage per item di Menu Pembayaran (`FE-BKC-FIX-006`) — bukan bagian scope aslinya |
| Roadmap | `NOT APPLICABLE` — tidak ada baris roadmap untuk perbaikan ini |
| Trace | `NOT APPLICABLE` |
| Contract version | `NOT APPLICABLE` — tidak ada perubahan kontrak API publik. `BillingCoverageComponent` adalah record internal (`BillingCoverageAdapter.cs`), bukan DTO yang diekspos |
| Backend Governance Preflight | Area `HealthServices`, Module `BillingManagement`, Submodule `Billing` — sudah terdaftar di registry (dipakai berulang kali sepanjang sesi ini: `BE-BKC-018`–`021`, `BE-BKC-FIX-001`). Keberlakuan: `TOUCHED LEGACY` (mengubah `BillingCalculationService.cs`, `BillingCoverageAdapter.cs` yang sudah ada, plus migration data baru) |
| Dependency | Tidak ada |
| Klasifikasi | `MEDIUM`-`HIGH` — 2 root cause berbeda, salah satunya (Fix 2) mengubah bentuk record internal yang dipakai luas di alur kalkulasi coverage; risiko finansial nyata bila salah (coverage asuransi salah hitung) |
| Task mode | `BACKEND` — bug fix ad-hoc, otorisasi eksplisit pengguna lewat `AskUserQuestion` (dua kali: sekali memutuskan mengerjakan migration+source sekaligus, sekali lagi menegaskan cakupan setelah temuan kedua muncul) |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/BillingManagement/Billing/Services/{BillingCalculationService.cs,BillingCoverageAdapter.cs}`, `Migrations/20260903163000_FixTariffCategoryInsuranceCoverageDefault.{cs,Designer.cs}` |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | (working tree belum di-commit) |
| Tanggal | 3 September 2026 |
| Status | Source + migration selesai. Build/test **TIDAK dijalankan** (instruksi eksplisit pengguna sepanjang sesi ini). **Eksekusi migration ke database (`dotnet ef database update`) BELUM dilakukan** — di luar wewenang task ini, tetap milik pengguna |

---

## 1. Masalah dan proses investigasi

Pengguna melaporkan (via `FE-BKC-FIX-006`): setelah badge coverage per item di Menu Pembayaran
diperbaiki supaya jujur mengikuti hasil kalkulasi (bukan lagi menebak dari cara bayar kunjungan),
badge itu SELALU menampilkan "Tunai" untuk invoice pasiennya — padahal menurut pengguna, pasien
itu (penjamin Allianz) seharusnya punya beberapa item tercover, hanya SATU item ("Konsultasi
Dokter Umum Rawat Jalan") yang seharusnya tetap Tunai.

Investigasi menemukan DUA root cause backend independen, keduanya menyebabkan
`RegistrationBillingCoverageAdapter` SELALU menghasilkan `primaryStatus = "NO_COVERAGE"` untuk
SEMUA invoice di sistem ini, tidak peduli rule asuransi apa yang dikonfigurasi:

### 1.1 Root cause 1 — data: `IsCoveredByInsuranceDefault` ter-backfill `false` untuk semua kategori

Migration `20260902072756_DropTableMstBillingCategory.cs` (2 September, di luar sesi ini)
menambahkan kolom `MstTariffCategory.IsCoveredByInsuranceDefault` dengan
`migrationBuilder.AddColumn<bool>(..., defaultValue: false)`. `AddColumn` dengan default pada
tabel yang SUDAH punya baris (11 kategori tarif) melakukan backfill nilai itu ke SEMUA baris
existing. Model C#-nya sendiri (`MstTariffCategory.cs` baris 35) mendeklarasikan default `true` —
tapi properti inisialisasi C# ini TIDAK pernah dijadikan `HasDefaultValue` di Fluent API/migration
manapun, jadi tidak pernah "menang" atas nilai `false` yang sudah di-backfill ke database.

Akibatnya: `BillingCalculationService.BuildCoverageComponents` (`Coverable = item.Category.IsCoveredByInsuranceDefault`)
SELALU `false` untuk item apa pun, kategori apa pun → `RegistrationBillingCoverageAdapter.ResolveAsync`
(`foreach (var component in context.Components.Where(x => x.Coverable && x.Amount > 0))`) tidak
pernah memproses satu komponen pun → `primaryStatus` selalu `"NO_COVERAGE"`.

Dikonfirmasi lewat data nyata (`GET .../calculation-preview` invoice pengguna): 3 item, 3 kategori
berbeda (ADMINISTRATION/PROCEDURE/RADIOLOGY), ketiganya `coverable: false`.

### 1.2 Root cause 2 — source: rule asuransi tidak pernah bisa match item apa pun

Bahkan SEANDAINYA root cause 1 diperbaiki, ditemukan rule asuransi yang menyasar referensi
spesifik (mis. rule "Coverage Allianz Kategori Radiologi", `TariffCategoryId`-scoped, 75%,
dikonfirmasi ada dan valid di database) TETAP tidak akan pernah match. `BillingCoverageComponent`
lama hanya membawa SATU field `SourceReferenceId` (`Guid?`), diisi dari parsing `item.SourceDetailId`
sebagai Guid. Tapi `SourceDetailId`, di SEMUA jalur pembuatan item invoice yang ada di seluruh
codebase (dikonfirmasi lewat grep: hanya ada dua, `SourceDomain` `"ADHOC_CATALOG"` dan `"ADHOC"`,
`BillingInvoiceService.cs` baris 304-310 dan 369-370), SELALU diisi idempotency key/GUID acak
untuk keperluan idempotency — BUKAN referensi domain (`ProcedureId`/`DrugId`/`TariffId`/
`TariffCategoryId`) sama sekali. `RegistrationBillingCoverageAdapter.Matches()` yang membandingkan
SATU `reference` itu terhadap `rule.TariffId`/`DrugId`/`DrugCategoryId`/`ProcedureId`/
`TariffCategoryId` karena itu tidak akan pernah cocok.

Ini BUKAN regresi dari sesi ini (pola `SourceDetailId` = idempotency key sudah ada sebelum
`BE-BKC-018`/`019` sesi ini dikerjakan — dikonfirmasi lewat `SourceDomain "ADHOC"` yang lebih
lama). Murni gap yang belum pernah berfungsi sejak awal. Dikonfirmasi lewat grep: tidak ada kode
lain yang bergantung pada `SourceReferenceId` versi lama (satu-satunya pemakai adalah file ini
sendiri), sehingga aman diganti total tanpa migrasi kompatibilitas.

Kedua temuan disajikan ke pengguna lewat `AskUserQuestion` (root cause 1 dulu, lalu root cause 2
setelah ditemukan saat investigasi lanjutan) sebelum satu baris kode pun diubah. Pengguna memilih
mengerjakan keduanya sekaligus.

---

## 2. Perubahan yang dikerjakan

### 2.1 Fix 1 — migration data (`20260903163000_FixTariffCategoryInsuranceCoverageDefault`)

| Berkas | Perubahan |
| --- | --- |
| `Migrations/20260903163000_FixTariffCategoryInsuranceCoverageDefault.cs` | Migration baru — `Up()`: `UPDATE "MstTariffCategory" SET "IsCoveredByInsuranceDefault" = true;` (seluruh baris, tidak ada bukti satu pun kategori sengaja diset `false` manual). `Down()`: mengembalikan ke `false` (reversibel murni teknis, BUKAN berarti state itu benar) |
| `Migrations/20260903163000_FixTariffCategoryInsuranceCoverageDefault.Designer.cs` | Disalin dari `20260903015730_AddTariffIdToBilInvoiceItem.Designer.cs` (migration terakhir) dan diganti nama class/atribut `[Migration(...)]` saja — SAH karena migration ini murni perbaikan DATA lewat `Sql()`, TIDAK mengubah bentuk model/kolom apa pun, sehingga `BuildTargetModel` (snapshot bentuk model pada titik itu) identik dengan migration sebelumnya |
| `ApplicationDbContextModelSnapshot.cs` | **TIDAK diubah** — konsisten dengan Designer.cs di atas: migration ini tidak mengubah bentuk model, jadi snapshot model saat ini (yang sudah benar, mencerminkan `MstTariffCategory.IsCoveredByInsuranceDefault` tanpa `HasDefaultValue` eksplisit) tetap valid |

Catatan desain: `defaultValue` kolom di level SQL SENGAJA tidak diubah (migration `AddColumn` lama
tidak disentuh). Properti C# `= true` pada `MstTariffCategory` sudah cukup membuat EF menyimpan
`true` untuk kategori BARU yang dibuat lewat jalur aplikasi normal (`SaveChanges`); mengubah
`DEFAULT` constraint SQL berarti mengedit migration lama yang sudah pernah berjalan, di luar
scope perbaikan data ini.

### 2.2 Fix 2 — source, pencocokan rule per dimensi rujukan

| Berkas | Perubahan |
| --- | --- |
| `BillingCoverageAdapter.cs` | `BillingCoverageComponent` record: `SourceReferenceId` (satu `Guid?`) diganti LIMA field eksplisit — `TariffId`, `ProcedureId`, `DrugId`, `DrugCategoryId`, `TariffCategoryId` (semua `Guid?`). `Matches()`: dari satu `reference` dibandingkan ke lima field rule (`||` chain semua terhadap satu nilai), diganti membandingkan MASING-MASING field rule terhadap field component yang sepadan (`rule.TariffId == component.TariffId`, dst per dimensi) — satu item bisa cocok dengan rule di granularitas manapun (bukan cuma satu dimensi tergabung) |
| `BillingCalculationService.cs` | `BuildCoverageComponents`: komponen ITEM/TAX-per-item kini mengisi kelima field baru dari `item.TariffId`, `item.CategoryId` (→`TariffCategoryId`), `item.Tariff?.ProcedureId`, `item.Tariff?.DrugId`, `item.Tariff?.Drug?.DrugCategoryId`. Komponen `ADMINISTRATION_FEE`/`ROOM_CHARGE`/`TAX` non-item TETAP null di kelima field itu (perilaku tidak berubah — komponen itu memang tidak punya rujukan tarif/prosedur/obat). `CalculateAsync`: query invoice ditambah `.Include(x => x.Items).ThenInclude(x => x.Tariff).ThenInclude(x => x!.Drug)` (sebelumnya hanya `.Include(x => x.Items).ThenInclude(x => x.Category)`) — dibutuhkan supaya `item.Tariff`/`item.Tariff.Drug` ter-hydrate saat `BuildCoverageComponents` dipanggil |

---

## 3. Kepatuhan arsitektur backend

Bukan endpoint baru, bukan perubahan kontrak API publik — `BillingCoverageComponent` adalah
record internal `BillingCoverageAdapter.cs`, tidak pernah diserialisasi ke response manapun.
Tidak ada perubahan role/access (`[AccessAction]`/`[AccessPermission]`) karena tidak ada endpoint
yang disentuh. Tidak ada arsitektur generic repository baru diperkenalkan.

---

## 4. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Grep `new BillingCoverageComponent(` di seluruh repo (produksi + test) | Hanya 2 pemanggil, keduanya di `BillingCalculationService.cs` (item/tax component, dan 4 komponen non-item admin/room-charge) — SEMUA sudah diperbarui ke 11 argumen sesuai record baru; tidak ada file lain (termasuk test) yang mengonstruksi record ini secara langsung | `PASS` | Grep lintas repo, dikonfirmasi 0 pemanggil lain |
| Grep `SourceReferenceId` di area `BillingManagement` | Hanya sisa pada komentar penjelasan di `BillingCoverageAdapter.cs` (bukan kode aktif) - tidak ada pemakaian aktif lagi field lama | `PASS` | Grep terarah ke folder `Areas/HealthServices/BillingManagement` |
| Hitung argumen positional pada tiap `new BillingCoverageComponent(...)` (item, tax-per-item, admin fee, admin fee tax, room charge, room charge tax) | Keenam pemanggil membawa tepat 11 argumen sesuai urutan record (`ComponentId, ComponentType, CoverageItemType, TariffId, ProcedureId, DrugId, DrugCategoryId, TariffCategoryId, Quantity, Amount, Coverable`) | `PASS` | Ditelusuri manual per baris hasil edit |
| Migration Designer.cs baru vs migration terakhir | Class name dan atribut `[Migration(...)]` terkonfirmasi berganti sesuai file baru (`FixTariffCategoryInsuranceCoverageDefault`), sisanya (`BuildTargetModel`) identik by design (tidak ada perubahan bentuk model pada migration data-only ini) | `PASS` | `head` pada file baru, dibandingkan dengan file sumber |

**`AUTOMATED TEST: BLOCKED`** — `dotnet build`/`dotnet test` TIDAK dijalankan sesuai instruksi
eksplisit pengguna sepanjang sesi ini ("jangan lakukan build dibackground biar saya cek sendiri").
Verifikasi di atas murni tekstual/struktural (grep, penghitungan argumen manual), BUKAN kompilasi
sungguhan — risiko kesalahan sintaksis/tipe yang hanya terdeteksi compiler TETAP ADA sampai
pengguna build sendiri.

**Tidak dijalankan / di luar wewenang task ini**: `dotnet build`; `dotnet test`;
`dotnet ef database update` (eksekusi migration ke database — tetap wewenang terpisah milik
pengguna); verifikasi hidup ujung-ke-ujung (badge "Penjamin" muncul benar untuk item Radiologi
Allianz) — BUTUH migration di atas benar-benar dijalankan ke database dan backend di-restart
lebih dulu, keduanya wewenang pengguna.

---

## 5. Risiko dan catatan penutup

| Hal | Isi |
| --- | --- |
| Risiko finansial | Perubahan ini MENGAKTIFKAN jalur coverage asuransi yang SEBELUMNYA selalu mati (`NO_COVERAGE` untuk semua invoice). Setelah migration dijalankan, invoice BARU yang dihitung ulang bisa menghasilkan `primaryAmount`/`excessAmount` > 0 untuk pertama kalinya sejak migration 2 September - PERIKSA rule asuransi yang ada (baru 4 rule di database dev) sebelum dipakai di lingkungan dengan data produksi, supaya tidak ada rule yang ternyata terlalu longgar/sempit dan baru ketahuan sekarang |
| Invoice lama yang sudah final/closed | Migration ini HANYA mengubah `MstTariffCategory` (master data) - tidak menyentuh `BilCalculationVersion` yang sudah tersimpan. Invoice yang SUDAH final dengan versi kalkulasi lama (`NO_COVERAGE`) TIDAK otomatis terhitung ulang - hanya kalkulasi BARU (pratinjau atau recalculate) yang akan mencerminkan perbaikan ini |
| Migration Designer.cs disalin, bukan digenerate `dotnet ef migrations add` | Karena build/EF tooling tidak boleh dijalankan sesi ini, Designer.cs dibuat dengan menyalin migration TERAKHIR (`AddTariffIdToBilInvoiceItem`) lalu mengganti nama class/atribut saja - sah karena migration ini murni `Sql()` data fix tanpa perubahan bentuk model, tapi BELUM pernah diverifikasi lewat `dotnet ef database update` sungguhan. **Disarankan kuat**: pengguna menjalankan `dotnet ef migrations list` dulu sebelum `database update` untuk memastikan migration ini terbaca benar oleh tooling, sebelum benar-benar diterapkan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Modified/baru: `BillingCalculationService.cs`, `BillingCoverageAdapter.cs`, `Migrations/20260903163000_FixTariffCategoryInsuranceCoverageDefault.{cs,Designer.cs}`. Belum staged/commit |
| Langkah berikutnya | (1) Pengguna build backend sendiri untuk memverifikasi kompilasi. (2) Pengguna menjalankan `dotnet ef migrations list` lalu `dotnet ef database update` bila daftar migration terbaca benar. (3) Setelah migration jalan + backend restart, verifikasi hidup ulang: buka invoice Allianz yang dilaporkan, konfirmasi item Radiologi/Administrasi menampilkan badge "Penjamin" sesuai rule yang ada, item Konsultasi (kategori tanpa rule Allianz) tetap "Tunai" |
