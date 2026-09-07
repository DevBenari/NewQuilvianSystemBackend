# Laporan Perubahan Backend — `BE-BKC-FIX-003`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-BKC-FIX-003` (ad-hoc, di luar roadmap, otorisasi eksplisit pengguna lewat `AskUserQuestion`) |
| Judul | Waterfall coverage melacak hasil PER KOMPONEN (item/admin-fee/room-charge), bukan cuma total gabungan — mengganti pendekatan proporsional/heuristik pada badge status per item dan split Subtotal/Pajak Mandiri-Asuransi dengan angka EKSAK |
| Slice | Ditemukan lewat pengujian langsung pengguna atas `FE-BKC-FIX-006`/`BE-BKC-FIX-002` — bukan bagian scope aslinya |
| Roadmap | `NOT APPLICABLE` |
| Trace | `NOT APPLICABLE` |
| Contract version | `NOT APPLICABLE` — tidak ada perubahan endpoint; field baru murni tambahan pada payload `GET .../calculation-preview` yang sudah ada (`breakdown.items[].itemPrimaryAmount/itemUnresolvedAmount/taxPrimaryAmount/taxUnresolvedAmount`, `breakdown.administrationFee/roomCharge.primaryAmount/unresolvedAmount`) |
| Backend Governance Preflight | Area `HealthServices`, Module `BillingManagement`, Submodule `Billing` — sudah terdaftar (dipakai berulang sepanjang sesi ini). Keberlakuan: `TOUCHED LEGACY` |
| Dependency | Tidak ada |
| Klasifikasi | `HIGH` — mengubah algoritma inti waterfall coverage (`RegistrationBillingCoverageAdapter`), menyentuh 3 DTO, dan konsumen frontend (badge + ringkasan pembayaran) |
| Task mode | `BACKEND` + `FRONTEND` gabungan (satu task ad-hoc yang sama, disetujui sekaligus lewat `AskUserQuestion`) |
| Target tulis | `NewQuilvianSystemBackend` — `BillingCoverageAdapter.cs`, `BillingCalculationService.cs`, `BillingInvoiceDtos.cs`. `QuilvianSystemFrontendDev` — `menu-pembayaran-view.jsx`, `billing-invoice-constants.js` |
| Model | Claude Sonnet 5 |
| Tanggal | 4 September 2026 |
| Status | Source selesai (backend + frontend), lint frontend PASS. Build/test backend **TIDAK dijalankan** (instruksi eksplisit pengguna). **Belum diverifikasi hidup** — menunggu pengguna rebuild backend dan restart dev server frontend |

---

## 1. Masalah

Investigasi lanjutan atas laporan bug pengguna (invoice Allianz milik IKBAL YULIYANTO, setelah
`BE-BKC-FIX-002` diterapkan) menemukan DUA masalah baru yang sama-sama berakar dari satu
keterbatasan arsitektur:

1. **Badge status per item salah lagi** (semua baris tertulis "Penjamin", termasuk "Konsultasi
   Dokter Umum Rajal" yang seharusnya "Tunai"). Root cause: badge (`FE-BKC-FIX-006`) memakai flag
   `coverable` (kelayakan tingkat KATEGORI) sebagai pendekatan — begitu `BE-BKC-FIX-002` membuat
   `IsCoveredByInsuranceDefault` benar untuk SEMUA kategori, flag itu jadi `true` di mana-mana dan
   berhenti membedakan apa pun.
2. **Subtotal Mandiri/Asuransi salah hitung** ("karena pengelompokkan itu", kutip pengguna).
   Root cause: formula `subtotalTagihan - subtotalAsuransi` (FE-BKC-016) diam-diam
   menggelembungkan Subtotal Mandiri sebesar jumlah yang sebenarnya masih "Penjamin Belum
   Terverifikasi" (unresolved) — unresolved BUKAN tanggungan pasien maupun penjamin.

Kedua masalah TIDAK bisa diperbaiki tuntas tanpa mengubah backend: waterfall coverage
(`RegistrationBillingCoverageAdapter.ResolveAsync`) HANYA mengembalikan TOTAL gabungan
(`PrimaryAmount`/`UnresolvedAmount` untuk SELURUH invoice), tidak pernah per komponen/item.
Pendekatan proporsional yang sempat dicoba di frontend (alokasi Pajak Mandiri/Asuransi berdasarkan
rasio invoice) juga cuma perkiraan, bukan angka sesungguhnya. Disajikan ke pengguna lewat
`AskUserQuestion` — pilihan yang diambil: kerjakan pelacakan per komponen sekarang.

---

## 2. Perubahan yang dikerjakan

### 2.1 Backend — `BillingCoverageAdapter.cs`

| Perubahan | Detail |
| --- | --- |
| `BillingCoverageComponentOutcome` (record baru) | `(Guid ComponentId, string ComponentType, decimal PrimaryAmount, decimal UnresolvedAmount)` — hasil waterfall PER KOMPONEN. Porsi Patient komponen itu diturunkan pemanggil sebagai `component.Amount - PrimaryAmount - UnresolvedAmount` (identitas selalu benar by construction) |
| `BillingCoverageDecision` | Ditambah field `IReadOnlyList<BillingCoverageComponentOutcome> ComponentOutcomes` |
| `ResolveAsync` | Setiap cabang (`rule is null` / `NeedApproval`+limit bulanan nyata / `NotCovered` / matched-normal) sekarang JUGA menambahkan satu `BillingCoverageComponentOutcome` ke daftar `outcomes`, sejalan persis dengan akumulasi pooled `primary`/`unresolved` yang sudah ada — tidak ada logika bisnis yang berubah, murni instrumentasi tambahan yang merekam apa yang SUDAH terjadi per komponen |
| `SelfPay()` | Mengembalikan `ComponentOutcomes` kosong — SEMUA komponen dianggap Patient sepenuhnya oleh pemanggil (default kosong = Patient, sesuai semantik SELF_PAY) |
| `Unresolved(components, primaryStatus)` | Signature berubah dari `(decimal amount, string)` jadi `(IReadOnlyList<BillingCoverageComponent> components, string)` — sekarang membangun outcome eksplisit PER KOMPONEN coverable (menandai seluruh `Amount`-nya unresolved), bukan cuma total pooled, supaya jalur REJECTED/UNRESOLVED (provider tidak eligible, encounter tidak ditemukan) juga terlacak per item dengan benar |

### 2.2 Backend — `BillingCalculationService.cs`

| Perubahan | Detail |
| --- | --- |
| `BuildCoverageComponents` — komponen TAX per item | **Bug tersembunyi ditemukan dan diperbaiki**: `ComponentId` komponen TAX sebelumnya memakai `tax.TaxRuleId` (SEMUA item berbagi satu tax rule aktif yang sama — lihat `LoadInvoiceTaxRuleAsync` — sehingga ComponentId-nya identik lintas item, tidak bisa dipakai melacak hasil per item). Diganti `item.Id` (sama seperti komponen ITEM-nya, beda `ComponentType`) — aman karena `ComponentId` untuk TAX tidak pernah dipakai logika matching/pembatasan manapun sebelumnya, murni identifier pasif |
| `CalculateAsync` | Setelah `coverage` didapat: bangun `outcomeByComponent` (Dictionary keyed `(ComponentId, ComponentType)`), salin `ItemPrimaryAmount`/`ItemUnresolvedAmount` (key `(item.Id,"ITEM")`) dan `TaxPrimaryAmount`/`TaxUnresolvedAmount` (key `(item.Id,"TAX")`) ke tiap `CalculationItemResponse`; salin `PrimaryAmount`/`UnresolvedAmount` ke `administrationFee`/`roomCharge` (key `(PolicyId ?? Guid.Empty, "ADMINISTRATION_FEE"/"ROOM_CHARGE")`) |

### 2.3 Backend — `BillingInvoiceDtos.cs`

| DTO | Field baru |
| --- | --- |
| `CalculationItemResponse` | `ItemPrimaryAmount`, `ItemUnresolvedAmount`, `TaxPrimaryAmount`, `TaxUnresolvedAmount` (semua `decimal`) |
| `AdministrationFeeCalculationResponse` | `PrimaryAmount`, `UnresolvedAmount` |
| `RoomChargeCalculationResponse` | `PrimaryAmount`, `UnresolvedAmount` |

### 2.4 Frontend — `menu-pembayaran-view.jsx`

| Perubahan | Detail |
| --- | --- |
| `getItemCoverageStatus(item)` | Diganti total — tidak lagi memakai `coverable` (kategori) + `invoiceHasActualCoverage` (heuristik tingkat invoice). Sekarang murni dari `breakdown.items[].{item,tax}{Primary,Unresolved}Amount` milik item itu sendiri: unresolved>0 → `"belum_terverifikasi"`; else primary>0 → `"penjamin"`; else → `"tunai"` |
| `subtotalMandiri`/`subtotalAsuransi` | Diganti dari `subtotalTagihan - subtotalAsuransi(pooled)` (FE-BKC-016, sudah terbukti salah - lihat § 1) menjadi jumlah EKSAK `itemPrimaryAmount`/`itemUnresolvedAmount` semua item + `administrationFee`/`roomCharge` PrimaryAmount/UnresolvedAmount-nya sendiri. `subtotalMandiri = subtotalTagihan - subtotalAsuransi(eksak) - unresolvedPreTax(eksak)` |
| `pajakMandiri`/`pajakAsuransi` | Diganti dari perkiraan proporsional (rasio invoice) menjadi jumlah EKSAK `taxPrimaryAmount`/`taxUnresolvedAmount` semua item (admin fee/room charge tidak lagi pernah kena pajak sejak fix PPN sebelumnya, jadi kontribusinya selalu 0) |

### 2.5 Frontend — `billing-invoice-constants.js`

| Perubahan | Detail |
| --- | --- |
| `BILLING_ITEM_COVERAGE_BADGE_CONFIG` | Tambah status `belum_terverifikasi` → label "Menunggu Verifikasi", `className: "region-status-pending"` (token yang sudah ada, dipakai ulang dari `CATALOG_CHARGE_COVERAGE_BADGE_CONFIG.needapproval`) |

---

## 3. Kepatuhan arsitektur

Tidak ada endpoint baru, tidak ada perubahan kontrak publik (field baru murni tambahan pada
payload response yang sudah ada). Tidak ada base component baru/extend di sisi frontend — badge
tetap memakai `StatusBadge`/`BILLING_ITEM_COVERAGE_BADGE_CONFIG` yang sudah ada, hanya sumber
datanya yang berubah.

---

## 4. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Grep pemanggil `BillingCoverageDecision`/`BillingCoverageComponentOutcome` di seluruh repo | Hanya `BillingCoverageAdapter.cs` (deklarasi) dan `BillingCalculationService.cs` (konsumsi) — tidak ada pemanggil lain yang perlu ikut berubah | `PASS` | Grep terarah |
| Hitung argumen `new BillingCoverageDecision(...)` (3 titik: `ResolveAsync`, `SelfPay`, `Unresolved`) | Ketiganya membawa tepat 8 argumen sesuai record baru (7 lama + `ComponentOutcomes`) | `PASS` | Ditelusuri manual per baris |
| `npx eslint` pada `menu-pembayaran-view.jsx` dan `billing-invoice-constants.js` | Berhasil tanpa error (1 warning unused-directive ditemukan dan dibersihkan) | `PASS` | Keluaran perintah kosong pada run terakhir |
| Penelusuran manual identitas aritmetika: `subtotalMandiri + subtotalAsuransi + unresolvedPreTax == subtotalTagihan`, `pajakMandiri + pajakAsuransi + taxUnresolvedSum == pajak` | Terbukti benar by construction (setiap komponen ITEM/TAX/ADMIN_FEE/ROOM_CHARGE persis satu kali disumbangkan ke salah satu dari Primary/Unresolved/Patient-implisit, tidak ada komponen yang terlewat atau terhitung dua kali) | `PASS` (analisis, bukan uji hidup) | Lihat § 2.1 desain outcome per cabang |

**`AUTOMATED TEST: BLOCKED`** — `dotnet build`/`dotnet test` TIDAK dijalankan (instruksi eksplisit
pengguna). **`MANUAL TEST: BLOCKED`** — backend belum di-rebuild pengguna sejak perubahan ini;
frontend dev server juga belum mengonfirmasi memuat ulang perubahan sebelumnya (`BE-BKC-FIX-002`
sempat menunjukkan staleness serupa). Verifikasi hidup ATAS PERUBAHAN INI BELUM dilakukan.

---

## 5. Risiko dan catatan penutup

| Hal | Isi |
| --- | --- |
| Risiko regresi tersembunyi | Perbaikan `ComponentId` pada komponen TAX (§ 2.2) mengubah nilai yang SEBELUMNYA dipakai sebagai identifier pasif - dikonfirmasi lewat grep bahwa `ComponentId` TAX tidak pernah dipakai logika matching/pembatasan (hanya `CoverageItemType`/`TariffId`/dst yang dipakai `Matches()`; `appliedPerVisit` di-key oleh `rule.Id`, bukan `ComponentId`), jadi perubahan ini AMAN, tapi tetap dicatat sebagai perubahan pada representasi data internal, bukan cuma penambahan murni |
| Belum diverifikasi hidup | Task ini BELUM dibuktikan bekerja dengan data nyata - nomor pada laporan ini adalah hasil penelusuran logika/kode, bukan hasil kalkulasi backend yang sudah di-build ulang. Langkah berikutnya WAJIB verifikasi hidup begitu pengguna rebuild backend + restart frontend dev server |
| Perubahan sampingan | `NONE`. Tidak ada file test dibuat (frontend, instruksi eksplisit pengguna) |
| Interupsi | `NONE` |
| Status Git | Modified: `BillingCoverageAdapter.cs`, `BillingCalculationService.cs`, `BillingInvoiceDtos.cs` (backend); `menu-pembayaran-view.jsx`, `billing-invoice-constants.js` (frontend). Belum staged/commit |
| Langkah berikutnya | (1) Pengguna rebuild + restart backend. (2) Pengguna restart frontend dev server. (3) Verifikasi hidup ulang invoice IKBAL YULIYANTO: konfirmasi "Konsultasi Dokter Umum Rajal" → Tunai, "Biaya Administrasi"/"ATLAS=CERVICAL 1" → Penjamin (rule sudah cocok), "ABBOTIC GRANUL..." (Drug) → Menunggu Verifikasi (belum ada rule Allianz untuk kategori Drug/Pharmacy - lihat catatan `BE-BKC-FIX-002`), dan Subtotal Mandiri + Subtotal Asuransi + Pajak Mandiri + Pajak Asuransi + Penjamin Belum Terverifikasi menjumlah persis ke Total Tagihan |
