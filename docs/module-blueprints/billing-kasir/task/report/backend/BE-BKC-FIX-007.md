# Laporan Perubahan Backend — `BE-BKC-FIX-007`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-FIX-007` (ad-hoc, di luar roadmap, permintaan langsung pengguna) |
| Judul | Kolom "Satuan" item Drug/Pharmacy/Consumable-Alkes di Menu Pembayaran kini menampilkan `MeasurementName` dari `DispenseUnitMeasurementId` |
| Slice | Independen dari investigasi coverage/PPN sebelumnya — permintaan tampilan baru |
| Roadmap | `NOT APPLICABLE` |
| Trace | `NOT APPLICABLE` |
| Contract version | `NOT APPLICABLE` — field baru nullable ditambahkan ke `InvoiceItemResponse` (`GET .../invoices/{id}`), tidak breaking existing consumer |
| Backend Governance Preflight | Area `HealthServices`, Module `BillingManagement`, Submodule `Billing` — sudah terdaftar. Keberlakuan: `TOUCHED LEGACY` |
| Dependency | `NONE` |
| Klasifikasi | `LOW` — murni penambahan field response read-only + Include query, tidak mengubah kalkulasi/bisnis apa pun |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `BillingInvoiceDtos.cs`, `BillingInvoiceService.cs` |
| Model | Claude Sonnet 5 |
| Tanggal | 4 September 2026 |
| Status | Source selesai. Build/test **TIDAK dijalankan** (instruksi eksplisit pengguna). **Belum diverifikasi hidup** — menunggu rebuild |

---

## 1. Masalah

Kolom "Satuan" pada tabel item invoice di Menu Pembayaran (`menu-pembayaran-view.jsx`) selalu
menampilkan "-" untuk SEMUA item, karena `InvoiceItemResponse` (DTO respons `GET .../invoices/{id}`,
sumber data tabel item Menu Pembayaran — BUKAN `breakdown.items[]` dari calculation-preview yang
dipakai fitur-fitur sebelumnya) tidak punya field Unit sama sekali. `BilInvoiceItem` (model) juga
tidak punya kolom Unit.

Pengguna meminta: untuk item berkategori Drug, Pharmacy, dan Alkes (Consumable), kolom itu
menampilkan `MeasurementName` berdasarkan `DispenseUnitMeasurementId`.

---

## 2. Analisis rantai data

`DispenseUnitMeasurementId` ada di `MstDrug` ("Satuan default saat diberikan ke pasien - Contoh:
tablet, kapsul, botol, vial"). Kategori Drug/Pharmacy/Consumable-Alkes SEMUA dibedakan lewat flag
`MstDrug.IsConsumable`, tapi tetap satu model `MstDrug` yang sama — rantai join dari item invoice:

```
BilInvoiceItem.TariffId → MstTariff.DrugId → MstDrug.DispenseUnitMeasurementId → MstMeasurement.MeasurementName
```

Kedua navigation property yang dibutuhkan SUDAH ADA di model (dikonfirmasi lewat pembacaan
langsung, tidak perlu migration): `MstTariff.Drug` dan `MstDrug.DispenseUnitMeasurement`. Gerbang
kategori memakai `MstTariffCategory.IsPharmacy` (flag yang sama dipakai `BE-BKC-FIX-004` untuk
scoping PPN — sudah dikonfirmasi sebelumnya di sesi ini hanya `true` untuk persis tiga kategori:
Pharmacy/Drug/Consumable), bukan mengecek nama kategori satu per satu.

---

## 3. Perubahan yang dikerjakan

### 3.1 `Dtos/BillingInvoiceDtos.cs`

`InvoiceItemResponse` — tambah `public string? Unit { get; set; }` (nullable, setelah
`DescriptionSnapshot`).

### 3.2 `Services/BillingInvoiceService.cs`

- `MapDetail(BilInvoice invoice, bool isReplay)`: tambah pemetaan
  `Unit = x.Category != null && x.Category.IsPharmacy ? x.Tariff?.Drug?.DispenseUnitMeasurement?.MeasurementName : null,`
  (pola ternary sama persis dengan `CategoryCode`/`CategoryName` yang sudah ada di object initializer yang sama).
- EMPAT titik query yang memuat `invoice` sebelum memanggil `MapDetail` — `GetDetailAsync` (baris
  ~143-147), pembuatan/pemuatan invoice saat entri manual katalog (baris ~597-600), pembatalan/void
  item (baris ~715-720), lookup by idempotency key (baris ~832-837) — masing-masing ditambah SATU
  `.Include()` baru: `.Include(x => x.Items).ThenInclude(x => x.Tariff).ThenInclude(x => x!.Drug).ThenInclude(x => x!.DispenseUnitMeasurement)`
  (pola sama dengan yang sudah dipakai `BillingCalculationService.cs` untuk kebutuhan coverage
  DrugId — Category dan Tariff adalah SIBLING property di bawah `Items`, jadi masing-masing butuh
  chain `.Include(x => x.Items).ThenInclude(...)` sendiri, tidak bisa disambung dari `.ThenInclude(x => x.Category)`).

---

## 4. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| Grep `DispenseUnitMeasurement)` di `BillingInvoiceService.cs` setelah edit | 4 kemunculan (baris 145, 599, 719, 837) — cocok dengan 4 titik query yang diidentifikasi | `PASS` |
| Baca ulang `MapDetail` setelah edit | Object initializer `InvoiceItemResponse` valid, `Unit` ternary konsisten pola `CategoryCode`/`CategoryName` | `PASS` |
| Analisis: item tanpa `TariffId` (SourceDomain "ADHOC"/domain lama) atau kategori non-Pharmacy | `Unit = null` — frontend (`item?.unit || item?.Unit || "-"`) sudah fallback ke "-", TIDAK disentuh, perilaku lama tetap untuk kasus ini | `PASS` (analisis) |

**`AUTOMATED TEST: BLOCKED`** — build/test tidak dijalankan (instruksi eksplisit pengguna).
**`MANUAL TEST: BLOCKED`** — belum di-rebuild pengguna.

---

## 5. Risiko dan catatan penutup

| Hal | Isi |
| --- | --- |
| Belum diverifikasi hidup | Berdasar penelusuran kode + verifikasi navigation property lewat pembacaan model, bukan hasil kalkulasi nyata pasca-rebuild |
| Perubahan sampingan | `NONE` |
| Frontend | TIDAK perlu perubahan — `menu-pembayaran-view.jsx` sudah membaca `item?.unit \|\| item?.Unit \|\| "-"` di kolom Satuan, field baru langsung terpakai begitu backend di-rebuild |
| Status Git | Modified: `BillingInvoiceDtos.cs`, `BillingInvoiceService.cs`. Belum staged/commit |
| Langkah berikutnya | Rebuild backend, verifikasi item Drug/Pharmacy/Alkes di Menu Pembayaran (mis. "ABBOTIC GRANUL...") menampilkan satuan dispensing yang benar (mis. "Botol"/"Tablet") di kolom Satuan, kategori lain (Administration/Procedure/Radiology/dst) tetap "-" |

