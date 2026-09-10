# Laporan Perubahan Frontend — `FE-BKC-FIX-006`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BKC-FIX-006` (ad-hoc bug fix — bukan task roadmap bernomor `FE-BKC-0xx`, dibuat sendiri untuk menjaga jejak laporan tetap tracked) |
| Judul | Badge status coverage per item di Menu Pembayaran salah — selalu "Penjamin" untuk pasien asuransi walau item tidak tercover / invoice tidak dapat coverage sama sekali |
| Slice | Laporan bug pengguna langsung (dua screenshot: tabel item + ringkasan pembayaran) atas hasil `FE-BKC-016`/alur `FE-BKC-014` — bukan bagian scope aslinya |
| Roadmap | `NOT APPLICABLE` — tidak ada baris roadmap untuk perbaikan ini |
| Trace | `NOT APPLICABLE` |
| Contract version | `NOT APPLICABLE` — tidak ada perubahan kontrak API; field `breakdown.items[].coverable` SUDAH ADA di response `GET .../calculation-preview` (`BE-BKC-*` sebelumnya), hanya belum pernah dikonsumsi frontend untuk badge per item |
| Wewenang UI | `REUSE` — memakai `StatusBadge`/`BILLING_ITEM_COVERAGE_BADGE_CONFIG` yang sudah ada, hanya mengganti SUMBER data status (dari invoice-level ke item-level), bukan base component baru/extend |
| Dependency | Tidak ada |
| Klasifikasi | `MEDIUM` — 1 berkas diubah, tapi menyangkut interpretasi data finansial (status coverage per item) yang butuh keputusan bisnis eksplisit dari pengguna sebelum diimplementasikan |
| Task mode | `FRONTEND` — bug fix ad-hoc, otorisasi eksplisit pengguna + keputusan lewat `AskUserQuestion` atas logika badge |
| Target tulis | `QuilvianSystemFrontendDev` — `src/components/view/health-services/billing-management/billing-invoices/menu-pembayaran/menu-pembayaran-view.jsx` |
| Model | Claude Sonnet 5 |
| Commit frontend saat dikerjakan | (working tree belum di-commit) |
| Commit backend yang dijadikan rujukan | `fec3579` |
| Tanggal | 3 September 2026 |
| Status | Source selesai, lint bersih, **terverifikasi hidup** untuk kasus yang dilaporkan (login sungguhan, invoice nyata pengguna). Jalur "item benar-benar tertanggung penjamin" TIDAK bisa diverifikasi hidup — lihat § 6 dan § 8 |

---

## 1. Keadaan yang ditemukan di awal

Pengguna melaporkan (dua screenshot): menambahkan item ke invoice yang menurut badge pratinjau
saat memilih tarif ("Tidak Tercover") TIDAK dicover asuransi, tapi di tabel item Menu Pembayaran
malah tertulis badge "Penjamin". Sekaligus, pada Ringkasan Pembayaran, "Subtotal Asuransi"
menampilkan Rp 0 sementara seluruh nilai (termasuk item yang tertulis "Penjamin" itu) masuk ke
"Subtotal Mandiri" — kontradiksi antara badge per baris dan ringkasan di bawahnya.

Ditelusuri sampai ke root cause: badge status per baris item (`menu-pembayaran-view.jsx`, variabel
`coverageStatus`) dihitung dari `isSelfPay` — SATU nilai untuk SELURUH invoice, berdasarkan
`paymentType`/`guarantorName` KUNJUNGAN (apakah pasien punya penjamin TERDAFTAR), BUKAN dari hasil
kalkulasi coverage yang sesungguhnya. Akibatnya SETIAP baris item pada invoice pasien "Asuransi"
selalu tertulis "Penjamin", bahkan ketika hasil kalkulasi (`coverage.primaryStatus`) adalah
`NO_COVERAGE` dan `Subtotal Asuransi` benar-benar Rp 0.

Diverifikasi lewat data nyata (invoice pengguna, `GET .../calculation-preview`):

```json
"coverage": { "primaryStatus": "NO_COVERAGE", "primaryAmount": 0, ... },
"items": [
  { "invoiceItemId": "...", "categoryCode": "PROCEDURE", "coverable": false, ... },
  ...
]
```

Field `breakdown.items[].coverable` (per item, SUDAH ADA di kontrak API, endpoint yang sama
dipakai sejak awal) TERNYATA belum pernah dikonsumsi frontend untuk badge per baris — hanya
dipakai backend secara internal.

**Batasan arsitektur yang ditemukan sekaligus**: kalkulasi coverage (`primaryAmount`/`excessAmount`)
dihitung sebagai SATU waterfall gabungan di level invoice (`BillingCalculationService.ApplyCoverageWaterfall`),
bukan dialokasikan per item — jadi tidak ada data "berapa rupiah dari item SPESIFIK ini yang
ditanggung penjamin". Field `coverable` per item hanya mewakili KELAYAKAN (apakah item ini secara
aturan boleh masuk kolam coverage), bukan porsi rupiah pastinya.

---

## 2. Proses bisnis dari sisi pengguna

**Pengguna**: kasir yang membuka Menu Pembayaran untuk invoice pasien asuransi.

**Langkah (sesudah perbaikan)**:

1. Kasir membuka Menu Pembayaran. Setiap baris item pada tabel kini menampilkan badge "Penjamin"
   HANYA bila DUA syarat terpenuhi sekaligus: (a) item itu sendiri layak diklaim ke penjamin
   (`coverable = true` dari hasil kalkulasi terbaru) DAN (b) invoice ini nyatanya mendapat
   coverage (`Subtotal Asuransi > 0`). Bila salah satu tidak terpenuhi, baris ditandai "Tunai"
   (mandiri).
2. Pada kasus yang dilaporkan (invoice `NO_COVERAGE`, semua item `coverable=false`): ketiga item
   kini konsisten menampilkan "Tunai", sejalan dengan Subtotal Asuransi Rp 0 dan Subtotal Mandiri
   yang menampung seluruh nilai.

**Aturan yang berlaku**: `getItemCoverageStatus(item) = (coverableByItemId.get(item.id) && subtotalAsuransi > 0) ? "penjamin" : "tunai"`.
Diputuskan eksplisit lewat `AskUserQuestion` (dua opsi: kombinasi coverable+coverage aktual
[dipilih] vs. coverable saja tanpa melihat hasil aktual).

**Jalur tidak normal**: bila `breakdown.items` tidak tersedia (mis. kalkulasi belum pernah
dihitung untuk invoice ini), `coverableByItemId` kosong → seluruh item default ke "Tunai" (lebih
aman secara finansial daripada salah menandai "Penjamin" tanpa dasar).

**Hasil akhir**: badge per baris tidak lagi bisa bertentangan dengan Ringkasan Pembayaran di
bawahnya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`menu-pembayaran-view.jsx` (perhitungan `coverageStatus` lama, render tabel item); backend
`BillingCalculationService.cs` (`ApplyCoverageWaterfall`, `BuildCoverageComponents`,
`MapResponse`/`DeserializeBreakdown` — dibaca saja, TIDAK diubah, dipakai untuk memastikan
`breakdown.items[].coverable` benar-benar ada dan konsisten baik dari jalur pratinjau langsung
maupun dari versi kalkulasi tersimpan); `BillingInvoiceDtos.cs`
(`CalculationItemResponse.Coverable`, `CalculationBreakdownResponse.Items` — dibaca saja, kontrak
sudah ada sebelumnya); `BillingInvoicesController.cs` (`PreviewCalculation` — dibaca saja);
`use-menu-pembayaran.js` (`displayedCalculation` — dikonfirmasi meneruskan `.breakdown` apa
adanya dari response API, baik dari pratinjau langsung maupun fallback versi tersimpan yang
sama-sama lewat `BillingCalculationService.MapResponse`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `menu-pembayaran-view.jsx` | Hapus variabel invoice-level `coverageStatus` (`isSelfPay ? "tunai" : "penjamin"`) yang dipakai untuk SEMUA baris; tambah `breakdownItems`, `coverableByItemId` (Map dari `invoiceItemId` → `coverable`), `invoiceHasActualCoverage` (`subtotalAsuransi > 0`), dan fungsi `getItemCoverageStatus(item)`; badge `StatusBadge` pada tabel item kini memakai `getItemCoverageStatus(item)` alih-alih `coverageStatus` yang sama untuk semua baris |

### 3.3 Kepatuhan arsitektur frontend

**Tabel keputusan base component:**

| Elemen | Keputusan | Alasan |
| --- | --- | --- |
| Badge status per item | `REUSE` | `StatusBadge`/`BILLING_ITEM_COVERAGE_BADGE_CONFIG` sudah ada dan tetap dipakai apa adanya — perubahan murni pada SUMBER data status (per item, bukan invoice-level), tidak menyentuh base component |

**`UI GATE`**: tidak ada — bukan perubahan base component. TAPI karena ini menyangkut interpretasi
data finansial (bukan sekadar tampilan), logika badge tetap disajikan sebagai pilihan bernomor ke
pengguna lewat `AskUserQuestion` sebelum diimplementasikan (lihat § 8 untuk alasan kenapa gerbang
keputusan tetap dijalankan meski bukan base-component-decision-gate).

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Item coverable + invoice dapat coverage aktual | Badge "Penjamin" |
| Item TIDAK coverable, ATAU invoice tidak dapat coverage aktual sama sekali | Badge "Tunai" |
| `breakdown.items` tidak tersedia (kalkulasi belum pernah jalan) | Default aman: seluruh item "Tunai" |

---

## 5. Endpoint yang dikonsumsi

`GET .../invoices/{id}/calculation-preview` — endpoint yang SAMA, tidak ada perubahan kontrak.
Field `data.breakdown.items[].coverable` yang SUDAH ADA di response kini benar-benar dipakai
frontend.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint menu-pembayaran-view.jsx` | Berhasil tanpa error (termasuk tidak ada referensi tersisa ke variabel `coverageStatus` lama yang dihapus) | `PASS` | Keluaran perintah kosong |
| Buka Menu Pembayaran invoice yang dilaporkan pengguna (`primaryStatus: NO_COVERAGE`, 3 item `coverable: false`) | Ketiga baris item ("Biaya Administrasi Rawat Jalan", "Konsultasi Dokter Umum Rajal", "ATLAS=CERVICAL 1") kini menampilkan badge "Tunai" (bukan lagi "Penjamin") — konsisten dengan Subtotal Asuransi Rp 0 / Subtotal Mandiri Rp 683.000 di ringkasan | `PASS` | Teks badge dan isi baris tabel diperiksa langsung dari DOM |
| Console error selama pengujian | Hanya noise pra-eksisting tidak terkait (CSP foto profil, dsb.) — tidak ada error baru dari perubahan ini | `PASS` | Console dipantau selama sesi Playwright |
| Jalur "item coverable + invoice dapat coverage aktual (`primaryAmount > 0`) → badge Penjamin" | **TIDAK bisa diverifikasi hidup** — seluruh 3 invoice yang ada di database dev saat ini berstatus `SELF_PAY` atau `NO_COVERAGE` (`primaryAmount: 0` semua), tidak ada satu pun invoice dengan coverage aktual untuk dijadikan kasus uji positif | `MANUAL TEST: NOT FEASIBLE` | Ketiga invoice di database dev di-scan satu per satu lewat `calculation-preview`, dicatat `primaryAmount`/`primaryStatus` masing-masing — semua nol |

Uji manual: `PASS` untuk kasus yang dilaporkan; jalur positif (`Penjamin` sungguhan) diverifikasi
lewat pembacaan logika kode saja (lihat § 8), bukan data hidup.

**Tidak dijalankan:** `npm run build`/`next build` penuh; component test (instruksi eksplisit
pengguna sepanjang sesi ini — tanpa file test).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Badge per item tidak lagi bertentangan dengan Ringkasan Pembayaran (Subtotal Asuransi Rp 0 tidak boleh ada baris "Penjamin") | Terpenuhi, terverifikasi hidup pada kasus yang dilaporkan | Lihat § 6 |
| Badge memakai data kelayakan per item (`coverable`) yang sudah ada di kontrak API, bukan cara bayar kunjungan | Terpenuhi | Lihat § 3.2 |
| Keputusan logika badge diambil eksplisit oleh pengguna | Terpenuhi | `AskUserQuestion`, opsi "Coverable + ada coverage aktual" dipilih |
| lint lulus | Terpenuhi | Lihat § 6 |
| Jalur positif (badge "Penjamin" saat coverage benar-benar diterapkan) terverifikasi hidup | **TIDAK terpenuhi** — tidak ada data uji yang tersedia, lihat § 6 dan § 8 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Kenapa tetap lewat gerbang keputusan pengguna meski bukan base component | Perubahan ini menafsirkan ULANG makna "status coverage per item" pada data finansial yang dilihat kasir langsung (potensi salah tagih/salah info ke pasien bila keliru) — level risikonya setara dengan keputusan UI gate base component, jadi tetap disajikan sebagai pilihan bernomor + rekomendasi sebelum kode diubah, konsisten dengan kehati-hatian yang sama dipakai untuk perubahan base component pada task-task lain |
| Batasan arsitektur yang ditemukan (bukan diperbaiki task ini) | Coverage dihitung sebagai waterfall gabungan per invoice, bukan dialokasikan per item — jadi pada invoice dengan coverage SEBAGIAN (primaryAmount > 0 tapi tidak menutup semua item coverable), badge "Penjamin" akan tampil untuk SEMUA item coverable tanpa membedakan mana yang benar-benar "kebagian" kuota coverage dan mana yang tidak. Ini bukan regresi dari perbaikan ini (state SEBELUMNYA lebih buruk - badge Penjamin muncul untuk SEMUA item pasien asuransi tanpa syarat apa pun) - hanya batas akurasi tertinggi yang bisa dicapai tanpa itemisasi waterfall coverage di backend (perubahan besar, di luar scope) |
| Jalur yang tidak terverifikasi hidup | Kombinasi "item coverable + invoice dapat coverage aktual" (badge seharusnya "Penjamin") tidak punya data uji di database dev saat ini (3 invoice yang ada semuanya `SELF_PAY`/`NO_COVERAGE`). Logika sudah diperiksa lewat pembacaan kode dan konsisten dengan keputusan pengguna, tapi BELUM dibuktikan lewat data nyata — disarankan diverifikasi ulang begitu ada invoice dengan coverage aktual di lingkungan manapun |
| Dependency backend | `NONE` — tidak ada perubahan backend, field yang dipakai sudah ada di kontrak sejak sebelumnya |
| Perubahan sampingan | `NONE`. Tidak ada file test dibuat |
| Interupsi | `NONE` |
| Status Git | Modified (task ini): `menu-pembayaran-view.jsx`. Berkas lain pada working tree yang sama milik task-task sebelumnya (lihat laporan masing-masing). Belum staged/commit |
| Langkah berikutnya | Verifikasi ulang jalur "Penjamin" begitu tersedia invoice dengan coverage aktual (`primaryAmount`/`excessAmount` > 0) di lingkungan mana pun |
| **Update 3 September 2026 (lanjutan)** | Investigasi kenapa "Penjamin" tidak pernah muncul (dilaporkan pengguna atas invoice Allianz-nya) menemukan DUA root cause backend terpisah yang membuat coverage asuransi TIDAK PERNAH benar-benar diterapkan ke item invoice mana pun di sistem ini (bukan spesifik invoice ini) - diperbaiki lewat task ad-hoc `BE-BKC-FIX-002`: (1) `MstTariffCategory.IsCoveredByInsuranceDefault` ter-backfill `false` untuk semua kategori akibat migration 2 September yang keliru, (2) pencocokan rule asuransi (`RegistrationBillingCoverageAdapter.Matches`) tidak pernah bisa match item apa pun karena rujukannya diambil dari idempotency key, bukan referensi domain. Lihat `task/report/backend/BE-BKC-FIX-002.md`. Migration BELUM dijalankan ke database (wewenang pengguna) - jalur "Penjamin" masih belum bisa diverifikasi hidup |
