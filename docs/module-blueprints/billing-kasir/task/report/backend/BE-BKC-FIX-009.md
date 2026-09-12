# Laporan Perubahan Backend — `BE-BKC-FIX-009`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-FIX-009` (ad-hoc, di luar roadmap, permintaan langsung pengguna) |
| Judul | Lengkapi `InvoiceEditContextResponse` dengan baris biaya mentah — menutup gap kontrak `BE-BKC-047` yang ditemukan `FE-BKC-028` |
| Slice | Perbaikan kontrak atas task yang sudah `✅` (`BE-BKC-047`), bukan fitur baru |
| Roadmap | `NOT APPLICABLE` — perbaikan gap, bukan task roadmap baru |
| Trace | `BE-BKC-047`, `MPY-DES-015`, `BIL-API-1.0` (kontrak tidak naik versi — aditif murni) |
| Contract version | `BIL-API-1.0` tidak berubah — satu field baru (`Items`) bersifat aditif, tidak ada field yang diganti nama/dihapus, tidak ada konsumen existing yang rusak |
| Backend Governance Preflight | Area `HealthServices`, Module `BillingManagement`, Submodule `Billing` — terdaftar `ACTIVE` (`MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 20). Keberlakuan: `TOUCHED LEGACY` (menambah field pada DTO dan method service yang sudah ada, nol tabel/kolom database baru) |
| Dependency | `BE-BKC-047` — `✅`, tidak berubah perilakunya (lihat § 3) |
| Klasifikasi | `LOW` — aditif murni, nol query database baru (data yang sama sudah dimuat method ini), nol perubahan endpoint/route/kontrak request |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Dtos/BillingPayerEditDtos.cs`, `Services/BillingPayerEditService.cs`, `Services/BillingInvoiceService.cs` |
| Model | Claude Sonnet 5 |
| Tanggal | 12 September 2026 |
| Status | Source selesai. `dotnet build`/`test` **TIDAK dijalankan** (instruksi baku pengguna — build backend dijalankan manual). **Belum diverifikasi hidup** — menunggu rebuild pengguna |

---

## 1. Masalah

Saat mengerjakan `FE-BKC-028` (halaman Edit Tagihan), ditemukan bahwa `InvoiceEditContextResponse`
(`GET /{id}/edit-context`, dibangun `BE-BKC-047`) didokumentasikan di kode sumbernya sendiri
sebagai *"seluruh bahan layar Edit Tagihan dalam satu panggilan"*, tetapi kenyataannya **tidak
membawa** deskripsi, satuan, harga satuan, kuantitas, maupun nama kategori per baris biaya —
field yang dibutuhkan tabel "Rincian tagihan" pada wireframe `FE-MPY-01`
(`03-frontend-architecture.md`).

Field yang ADA pada response ini (`Calculation.Breakdown.Items` bertipe `CalculationItemResponse`,
dan `ItemPayerAssignments` bertipe `ItemPayerAssignmentResponse`) hanya membawa angka kalkulasi
dan status penanggung — bukan atribut tampilan item. Field yang dibutuhkan **hanya ada** pada
`InvoiceItemResponse`, dipakai `GET /{id}` (`BillingInvoiceService.GetDetailAsync`, task lama,
tidak terkait `BE-BKC-047`).

Frontend (`FE-BKC-028`) sudah dikerjakan dengan **memanggil kedua endpoint** sebagai jalan keluar
sementara — bukan dengan menebak bentuk payload yang belum ada. Task ini menutup gap itu di sisi
backend, supaya `edit-context` benar-benar cukup sendiri seperti niat desain aslinya.

**Ini bukan keputusan bisnis baru.** Data yang ditambahkan sudah ada dan sudah disetujui sejak
`BE-BKC-047`/lebih lama — task ini murni mengekspos data yang sudah dihitung/dimuat method yang
sama, lewat rumus pemetaan yang sudah ada dan sudah teruji (`GET /{id}`), tanpa mengubah satu pun
aturan bisnis, nilai, atau kontrak request.

---

## 2. Perubahan yang dikerjakan

### 2.1 `Services/BillingInvoiceService.cs` — ekstraksi `MapItems` (baru, `internal static`)

Pemetaan `BilInvoiceItem` → `InvoiceItemResponse` sebelumnya ditulis inline di dalam
`MapDetail(BilInvoice, bool)` (dipanggil 6 tempat berbeda di file yang sama). Diekstrak apa adanya
(nol perubahan logika/nilai) menjadi `internal static List<InvoiceItemResponse> MapItems(
IEnumerable<BilInvoiceItem> items)`, supaya bisa dipanggil ulang dari `BillingPayerEditService`
tanpa menyalin rumusnya. `MapDetail` diperbarui memanggil `MapItems(invoice.Items)` — perilakunya
identik byte-per-byte dengan sebelumnya (dikonfirmasi dengan membandingkan ekspresi lama vs baru
baris per baris, bukan hanya dipindah).

### 2.2 `Dtos/BillingPayerEditDtos.cs` — `InvoiceEditContextResponse.Items` (baru)

Satu field baru: `public List<InvoiceItemResponse> Items { get; set; } = [];`, memakai ulang tipe
`InvoiceItemResponse` yang sudah ada (namespace yang sama, `BillingInvoiceDtos.cs`) — bukan DTO
baru. Bentuk dan isinya **sama persis** dengan `InvoiceDetailResponse.Items` milik `GET /{id}`,
termasuk item berstatus `VOIDED` tetap disertakan (konsumen menyaring sendiri, sama seperti pola
Menu Pembayaran) — bukan backend yang menyaring lebih dulu.

### 2.3 `Services/BillingPayerEditService.cs` — `GetEditContextAsync`

Dua perubahan:

1. **Include chain diperluas.** Sebelumnya hanya `.Include(x => x.Items).ThenInclude(x =>
   x.Category)`. Ditambah `.Include(x => x.Items).ThenInclude(x => x.Tariff).ThenInclude(x =>
   x!.Drug).ThenInclude(x => x!.DispenseUnitMeasurement)` — disalin persis dari
   `BillingInvoiceService.GetDetailAsync` (baris 691). Tanpa include kedua ini, field `Unit` pada
   item farmasi/alkes akan selalu `null` walau field-nya ada di response — gap tersembunyi yang
   sama sekali tidak terlihat dari bentuk DTO, hanya terlihat dari nilainya saat runtime.
2. **Response diisi**: `Items = BillingInvoiceService.MapItems(invoice.Items)`, ditambahkan pada
   konstruksi `InvoiceEditContextResponse` yang sudah ada. Tidak ada query database tambahan —
   `invoice.Items` sudah dimuat baris pertama method ini sejak `BE-BKC-047`.

**Yang sengaja TIDAK diubah:** kontrak request (`GET /{id}/edit-context` tetap tanpa parameter
selain `id`), field lain pada `InvoiceEditContextResponse`, endpoint/route, hak akses
(`BillingInvoice:Read`, tidak berubah), dan seluruh method lain di kedua service (
`PreviewPayerComparisonAsync`, `SwitchPaymentSourceAsync` tidak tersentuh — masing-masing punya
`Include(Items).ThenInclude(Category)` sendiri yang terpisah, sengaja tidak ikut diperluas karena
tidak membutuhkan `Unit`).

---

## 3. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| `MapItems` menghasilkan nilai identik dengan ekspresi lama | Dibandingkan langsung field-per-field antara ekspresi inline lama dan isi method baru — sama persis, murni dipindah, nol perubahan logika | `PASS` (tinjauan kode) |
| Namespace `BillingInvoiceService`/`BillingPayerEditService`/`InvoiceItemResponse`/`InvoiceEditContextResponse` cocok tanpa `using` tambahan | Dikonfirmasi: `Services/BillingInvoiceService.cs` dan `Services/BillingPayerEditService.cs` sama-sama `namespace ....Billing.Services`; `Dtos/BillingInvoiceDtos.cs` dan `Dtos/BillingPayerEditDtos.cs` sama-sama `namespace ....Billing.Dtos` | `PASS` (tinjauan kode) |
| Include chain `Tariff.Drug.DispenseUnitMeasurement` valid | Disalin persis (bukan ditulis ulang/ditebak) dari `BillingInvoiceService.GetDetailAsync` baris 691 yang sudah berjalan | `PASS` (tinjauan kode) |
| `PreviewPayerComparisonAsync`/`SwitchPaymentSourceAsync` tidak ikut berubah | Diperiksa: kedua method punya query `BilInvoices` sendiri yang terpisah (baris 233, 1012) — tidak disentuh task ini | `PASS` (tinjauan kode) |
| `MapDetail` (6 titik panggilan) tetap mengembalikan `InvoiceDetailResponse` yang sama | Diperiksa seluruh 6 titik panggilan (baris 696, 849, 1226, 1319, 1376, 1450) — tidak ada yang berubah signature maupun perilaku, hanya isi `MapDetail` yang kini mendelegasikan ke `MapItems` | `PASS` (tinjauan kode) |

**`AUTOMATED TEST: BLOCKED`** — `dotnet build`/`test` tidak dijalankan (instruksi baku pengguna).
**`MANUAL TEST: BLOCKED`** — belum di-rebuild pengguna, belum ada panggilan nyata ke
`GET /{id}/edit-context` pasca-perubahan.

---

## 4. Risiko dan catatan penutup

| Hal | Isi |
| --- | --- |
| Belum diverifikasi hidup | Seluruhnya tinjauan kode + perbandingan terhadap ekspresi/Include yang sudah terbukti berjalan di `GET /{id}` — bukan hasil query nyata pasca-rebuild |
| Payload response membesar | `edit-context` kini membawa seluruh baris item (bisa puluhan pada tagihan rawat inap panjang) — sama seperti `GET /{id}` sudah lakukan sejak lama, bukan beban baru yang belum pernah ada di sistem ini |
| Frontend sudah dibersihkan mengikutinya | **Update 12 September 2026, sesi yang sama:** `FE-BKC-028` diperbarui — `use-billing-invoice-edit-tagihan.js` berhenti memanggil `GET /{id}` terpisah, `edit-tagihan-view.jsx` kini membaca `editContext.items` langsung. Lihat `task/report/frontend/fe-bkc-028-halaman-edit-tagihan-dan-panel-edit-asuransi.md` § "Update 12 September 2026". Frontend juga masih **belum** di-build/test (menunggu pengguna) |
| Perubahan sampingan | `NONE` — nol tabel/kolom database baru, nol endpoint baru, nol perubahan request |
| Status Git | Modified: `BillingPayerEditDtos.cs`, `BillingInvoiceService.cs`, `BillingPayerEditService.cs`. Belum staged/commit |
| Langkah berikutnya | Pengguna menjalankan `dotnet build`/`dotnet test` (backend) dan `npm run lint`/`test:unit`/`build` (frontend), lalu verifikasi `GET /{id}/edit-context` nyata membawa `Items` terisi benar (termasuk `Unit` pada item farmasi) dan halaman Edit Tagihan tetap tampil benar dengan satu panggilan API |
