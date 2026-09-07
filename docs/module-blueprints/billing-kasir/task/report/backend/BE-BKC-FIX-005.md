# Laporan Perubahan Backend — `BE-BKC-FIX-005`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-FIX-005` (ad-hoc, di luar roadmap, ditemukan lewat pengujian langsung invoice Allianz nyata oleh pengguna) |
| Judul | `Matches()` di waterfall coverage tidak lagi menggerbangi SEMUA dimensi rule di belakang satu tag `CoverageItemType` tunggal — diselaraskan dengan pola `InsuranceCoverageService.FindCoverageRuleAsync` (OR-chain per dimensi) |
| Slice | Lanjutan investigasi laporan bug pengguna atas invoice Allianz nyata, setelah `BE-BKC-FIX-003`/`BE-BKC-FIX-004` |
| Roadmap | `NOT APPLICABLE` |
| Trace | `NOT APPLICABLE` |
| Contract version | `NOT APPLICABLE` — tidak ada perubahan endpoint/skema; murni perbaikan logika matching internal |
| Backend Governance Preflight | Area `HealthServices`, Module `BillingManagement`, Submodule `Billing` — sudah terdaftar. Keberlakuan: `TOUCHED LEGACY` |
| Dependency | `BE-BKC-FIX-003`/`BE-BKC-FIX-004` (task ini melanjutkan source yang sama) |
| Klasifikasi | `HIGH` — mengubah logika inti pencocokan rule waterfall coverage, berdampak finansial langsung pada seluruh rule bertipe `ServiceCategory`/`Drug`/`Procedure` yang menyasar kategori Pharmacy/Drug/Consumable-Alkes atau Procedure |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `BillingCoverageAdapter.cs` |
| Model | Claude Sonnet 5 |
| Tanggal | 4 September 2026 |
| Status | Source selesai. Build/test **TIDAK dijalankan** (instruksi eksplisit pengguna). **Belum diverifikasi hidup** — menunggu rebuild |

---

## 1. Masalah

Pengguna menambahkan dua rule Allianz baru lewat master data Insurance Coverage Rule -
"Coverage Obat Rajal" (`ItemType="ServiceCategory"`, `TariffCategoryId` → kategori "Drug") dan
"Coverage Pharmacy" (`ItemType="ServiceCategory"`, `TariffCategoryId` → kategori "Pharmacy"),
keduanya Covered 100%. Item invoice nyata "ABBOTIC GRANUL 125 MG/5ML 30 ML SYRUP*" (kategori Drug,
rawat jalan) tetap berstatus "Tunai" di Menu Pembayaran walau rule itu ada dan `TariffCategoryId`-nya
sudah benar diisi. Sebagai pembanding, dropdown preview tarif ("Buat Invoice Manual") justru
menunjukkan item ini "Tercover" dengan benar — dua engine coverage yang ada di sistem ini
(`InsuranceCoverageService.FindCoverageRuleAsync` untuk preview, `RegistrationBillingCoverageAdapter.
Matches()` untuk kalkulasi Menu Pembayaran sesungguhnya) memberi hasil BERBEDA untuk rule yang SAMA.

**Root cause**: `Matches()` (`BillingCoverageAdapter.cs`, sebelum perbaikan ini) memakai gerbang
tunggal `rule.ItemType == component.CoverageItemType` di awal fungsi, SEBELUM memeriksa dimensi
referensi spesifik (`TariffId`/`DrugId`/`DrugCategoryId`/`ProcedureId`/`TariffCategoryId`) apa pun.
`component.CoverageItemType` sendiri berasal dari `CoverageItemType(BilInvoiceItem item)` di
`BillingCalculationService.cs`, yang MEMAKSA setiap item berkategori `IsPharmacy=true` (Drug/
Pharmacy/Consumable-Alkes) untuk selalu bertag `"Drug"`, tidak peduli rule-nya sendiri ditujukan
sebagai apa. Rule bertipe `"ServiceCategory"` yang menyasar kategori Drug/Pharmacy lewat
`TariffCategoryId` TIDAK PERNAH bisa cocok — gerbang `ItemType` menolaknya lebih dulu, walau
`TariffCategoryId`-nya benar. `InsuranceCoverageService.FindCoverageRuleAsync` (engine preview) TIDAK
punya gerbang tunggal semacam ini — polanya adalah OR-chain lima kondisi independen, masing-masing
menggerbangi dimensinya sendiri dengan `ItemType` yang sepadan (`Tariff`+`TariffId`,
`Drug`+`DrugId`, `DrugCategory`+`DrugCategoryId`, `Procedure`+`ProcedureId`,
`ServiceCategory`+`TariffCategoryId`) — sehingga rule `ServiceCategory` menyasar kategori Drug tetap
bisa cocok di sana.

---

## 2. Perubahan yang dikerjakan

### 2.1 `BillingCoverageAdapter.cs` — `Matches()`

Ditulis ulang total, gerbang tunggal `rule.ItemType == component.CoverageItemType` dihapus, diganti
OR-chain lima kondisi independen — persis pola `InsuranceCoverageService.FindCoverageRuleAsync`
(literal string `ItemType` dikonfirmasi lewat pembacaan langsung source itu, baris 514-518, BUKAN
tebakan):

```csharp
return
    (string.Equals(rule.ItemType, "Tariff", StringComparison.OrdinalIgnoreCase)
        && rule.TariffId.HasValue && rule.TariffId == component.TariffId)
    || (string.Equals(rule.ItemType, "Drug", StringComparison.OrdinalIgnoreCase)
        && rule.DrugId.HasValue && rule.DrugId == component.DrugId)
    || (string.Equals(rule.ItemType, "DrugCategory", StringComparison.OrdinalIgnoreCase)
        && rule.DrugCategoryId.HasValue && rule.DrugCategoryId == component.DrugCategoryId)
    || (string.Equals(rule.ItemType, "Procedure", StringComparison.OrdinalIgnoreCase)
        && rule.ProcedureId.HasValue && rule.ProcedureId == component.ProcedureId)
    || (string.Equals(rule.ItemType, "ServiceCategory", StringComparison.OrdinalIgnoreCase)
        && rule.TariffCategoryId.HasValue && rule.TariffCategoryId == component.TariffCategoryId);
```

`BillingCoverageComponent.CoverageItemType` dan helper `CoverageItemType()` di
`BillingCalculationService.cs` **dibiarkan ada** (masih dipopulasikan di `BuildCoverageComponents()`
seperti sebelumnya) — sudah tidak dipakai untuk gating di `Matches()` lagi, jadi kini vestigial;
tidak dibersihkan dalam task ini (di luar scope yang diminta), dicatat sebagai potensi cleanup masa
depan bila benar-benar tidak terpakai di tempat lain.

Urutan pemilihan rule saat lebih dari satu cocok untuk komponen yang sama (`rules.FirstOrDefault
(x => Matches(x, component))`, `ResolveAsync` baris 109) **tidak diubah** — tetap memakai urutan
`Priority` menurun lalu `RuleCode` (query di baris 97-98), sama seperti sebelum perbaikan ini.
`InsuranceCoverageService.FindCoverageRuleAsync` memakai `GetRuleSpecificity()` sebagai tie-breaker
tambahan (Tariff > Drug/Procedure > DrugCategory > ServiceCategory) — pola ini TIDAK disalin ke
`RegistrationBillingCoverageAdapter` karena di luar scope permintaan pengguna (hanya
`Matches()`/matching per-dimensi yang diminta diselaraskan, bukan urutan tie-breaking) dan
`Priority` sudah menjadi mekanisme tie-breaking eksplisit yang dikonfigurasi admin di sini.

---

## 3. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| Baca literal string `ItemType` di `InsuranceCoverageService.FindCoverageRuleAsync` (baris 514-518) | `"Tariff"`/`"Drug"`/`"DrugCategory"`/`"Procedure"`/`"ServiceCategory"` — persis dipakai di `Matches()` yang baru | `PASS` |
| Analisis backward-compat: rule "Coverage Administration"/"Coverage Laboratory Allianz"/"Coverage Radiology" (`ItemType="ServiceCategory"` + `TariffCategoryId` ke kategori non-Pharmacy) | Kondisi ke-5 OR-chain (`ServiceCategory`+`TariffCategoryId`) sama persis dengan sebelumnya untuk kategori non-Pharmacy (`CoverageItemType` item itu SUDAH `"ServiceCategory"` sebelum perbaikan ini juga) — perilaku tidak berubah | `PASS` (analisis) |
| Analisis backward-compat: rule "Coverage Obat Rajal"/"Coverage Pharmacy" (`ItemType="ServiceCategory"` + `TariffCategoryId` ke kategori Drug/Pharmacy) | Sebelumnya SELALU ditolak gerbang pertama; sekarang kondisi ke-5 OR-chain match langsung berdasar `TariffCategoryId`, tidak lagi bergantung pada `CoverageItemType` item — **bug diperbaiki** | `PASS` (analisis) |
| Grep `CoverageItemType` di seluruh folder `Services/` billing | Hanya dipakai di record definition + populasi `BuildCoverageComponents()`; tidak ada lagi pemakaian untuk gating | `PASS` |

**`AUTOMATED TEST: BLOCKED`** — build/test tidak dijalankan (instruksi eksplisit pengguna).
**`MANUAL TEST: BLOCKED`** — belum di-rebuild pengguna.

---

## 4. Risiko dan catatan penutup

| Hal | Isi |
| --- | --- |
| Risiko perubahan perilaku (dicatat, bukan disembunyikan) | Kode SEBELUMNYA mendukung pola "rule blanket tanpa referensi spesifik apa pun" (`hasSpecificReference == false` → cocok untuk SEMUA komponen dengan `CoverageItemType` yang sama, mis. rule `ItemType="ServiceCategory"` tanpa `TariffCategoryId` diisi akan cocok ke SEMUA item ServiceCategory apa pun kategorinya). Kode BARU tidak lagi mendukung pola ini — setiap kondisi OR-chain mensyaratkan field referensi yang sepadan benar-benar diisi (`HasValue`). Ini SESUAI PERSIS dengan `InsuranceCoverageService.FindCoverageRuleAsync` yang sudah lama berjalan (engine itu JUGA tidak pernah mendukung pola blanket-tanpa-referensi). Tidak ditemukan bukti rule blanket semacam ini dipakai nyata di data yang sudah diverifikasi sepanjang sesi ini (Administration/Laboratory/Radiology/Obat Rajal/Pharmacy semuanya mengisi `TariffCategoryId`) — tapi bila ada rule LAIN di data produksi yang sengaja dibuat tanpa referensi spesifik (mengandalkan perilaku lama), rule itu akan berhenti cocok setelah perbaikan ini. Perlu dicek pengguna bila ada rule semacam itu |
| Belum diverifikasi hidup | Sama seperti `BE-BKC-FIX-003`/`004` — laporan ini berdasar penelusuran kode, bukan hasil kalkulasi nyata pasca-rebuild |
| Perubahan sampingan | `NONE` |
| Status Git | Modified: `BillingCoverageAdapter.cs`. Belum staged/commit |
| Langkah berikutnya | Rebuild backend, lalu verifikasi ulang invoice IKBAL YULIYANTO: item "ABBOTIC GRANUL..." → badge "Penjamin" (bukan lagi "Tunai"), Subtotal Asuransi bertambah sesuai porsi Drug yang ter-cover; pastikan Administration/Laboratory/Radiology tetap "Penjamin" seperti sebelumnya (regresi negatif) |

