# Laporan Perubahan Backend — `BE-FIN-043`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-043` |
| Judul | Kolom `PPNAmount` pada `FinSupplierReturn`, constraint `PPNAmount >= 0`, dan kalkulasi deposit retur saat `CONFIRMED` memperhitungkan PPN |
| Slice | `POST-MVP` (`REV-6/8`) — Penyelarasan katalog kejadian Accounting |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian 3 (tabel task) |
| Trace | `FIN-DEC-047`, `FIN-DEC-061`, `FIN-DEC-067`, **`FIN-DEC-068`**; `FIN-DES-055`; `02-backend-architecture.md` §E.9-E.10 |
| Contract version | `FIN-INTEGRATION-1.6` §5.10.2 kode 36; `FIN-VAL-1.5` `FIN-VAL-133`, `135`, `136`, `143`; `erd/data-dictionary.md` |
| Dependency | `BE-FIN-030` 🟡, `BE-FIN-031` 🟡, `BE-FIN-035` 🟡 — seluruhnya source selesai; task ini menulis di atasnya |
| Klasifikasi | `MEDIUM` — satu repository (skor 0); ≤8 berkas diperiksa domain terdekat (skor 0), >8 total dibaca lintas kontrak/desain (skor 1); 7 berkas diubah + 2 berkas migration baru (skor 1); logika bisnis sedang — dua aturan validasi baru + pemisahan nilai kejadian (skor 1); kontrak API — nol endpoint baru, dua DTO diperluas aditif (skor 1); database — satu kolom + satu check constraint pada tabel yang sudah berjalan (skor 1); keamanan/auth — nol perubahan (skor 0); UI/workflow — nol (skor 0). Total 5 → `MEDIUM` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/Purchasing/**`, `Areas/Corporate/FinanceManagement/AccountingIntegration/Models/FinAccountingEventOutbox.cs`, `Repositories/Configurations/Corporate/FinanceManagement/Purchasing/**`, `Migrations/**`, `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` (tanda status) |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | Belum di-commit — working tree `Yasmina`, HEAD `7811c048` |
| Tanggal | 29 September 2026 |
| Status | 🟡 **SEBAGIAN.** Source dan berkas migration selesai; otorisasi pembuatan berkas migration diberikan eksplisit pengguna 29 September 2026. `dotnet build` **NOT RUN** (aturan berdiri "jangan build otomatis"). Eksekusi migration ke database **belum diminta** — wewenang terpisah |

---

## 1. Masalah yang diperbaiki

`FinSupplierReturn` hanya menyimpan satu angka nilai (`TotalAmount`), padahal
`FinPurchasingInvoice` di rumpun yang sama sudah memisahkan `PPNAmount`. Akibatnya porsi PPN retur
tidak punya tempat, dan dua hal rusak sekaligus:

1. **Kredit retur tercatat lebih kecil daripada yang diakui supplier.** Supplier mengakui pokok
   beserta PPN-nya, tetapi Deposit Retur yang terbit hanya sebesar pokok.
2. **Syarat ratifikasi Accounting tidak dapat dipenuhi.** Accounting meminta nilai
   `RETUR-PEMBELIAN` adalah pokok tanpa PPN, dan porsi PPN dikirim lewat kode terpisah
   (`evidence/14` bagian 3.6). Tanpa kolom terpisah, keduanya tidak dapat dibedakan.

**Contoh konkret.** Obat pokok Rp 1.000.000 + PPN Rp 110.000 diretur ke PBF. Supplier mengakui
kredit Rp 1.110.000. Sebelum task ini, Deposit Retur terbit hanya Rp 1.000.000 — selisih
Rp 110.000 tidak pernah dapat dipakai mengurangi utang, dan pada rekonsiliasi supplier ia terbaca
sebagai utang yang masih harus dibayar padahal sudah diselesaikan lewat retur. Sesudah task ini,
Deposit Retur terbit Rp 1.110.000, kejadian `RETUR-PEMBELIAN` tetap Rp 1.000.000, dan kejadian
`PPN-MASUKAN-RETUR-PEMBELIAN` Rp 110.000 terbit terpisah.

---

## 2. Proses bisnis

**Pelaku:** Petugas AP/Pembelian (mencatat dan mengonfirmasi retur).

**Langkah normal:**

1. Petugas mencatat retur atas Purchasing Invoice yang sudah `APPROVED`, mengisi rincian barang
   **dan** porsi PPN-nya (`POST /purchasing/supplier-returns`, field `ppnAmount` baru).
2. Backend menghitung pokok dari rincian barang (`TotalAmount` = jumlah `LineTotal`) — nilai ini
   **tidak pernah** diambil dari permintaan. Porsi PPN disimpan terpisah pada `PPNAmount`.
3. Retur tersimpan `DRAFT`.
4. Petugas mengonfirmasi retur (`POST /{id}/confirm`). Dalam **satu transaksi**: Deposit Retur
   terbit berstatus `AVAILABLE` sebesar **pokok + PPN**, kejadian `RETUR-PEMBELIAN` terbit sebesar
   **pokok saja**, dan — hanya bila PPN > 0 — kejadian `PPN-MASUKAN-RETUR-PEMBELIAN` terbit sebesar
   porsi PPN-nya.
5. Deposit itu kemudian dipakai mengurangi pembayaran supplier lewat jalur yang sudah ada
   (`BE-FIN-036`), tanpa perubahan apa pun pada jalur itu.

**Jalur tidak normal:**

- PPN negatif → ditolak `400` (`FIN-VAL-133`).
- Pokok saja sudah melebihi nilai faktur → ditolak `422` (`FIN-VAL-111`, perilaku lama, **tidak
  berubah**).
- Pokok masih muat tetapi pokok + PPN melebihi nilai faktur → ditolak `400` (`FIN-VAL-143`).
- Gagal di titik mana pun saat konfirmasi → status tetap `DRAFT`, nol deposit, nol kejadian
  (rollback penuh — perilaku lama, tidak berubah).

**Hasil akhir:** retur lama (`PPNAmount = 0`) berperilaku **identik** dengan sebelum task ini —
deposit tetap sebesar pokok, dan nol kejadian PPN terbit.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` (kartu `BE-FIN-043`)
- `02-backend-architecture.md` §E.4 (`FIN-DES-055`), §E.9 (status model dan tiga perubahan
  perilaku), §E.10 (rencana migration)
- `contracts/validation-matrix.md` §D.2 (`FIN-VAL-133`..`136`, `143`),
  `contracts/integration-contract.md` §5.10.2 (kode 36)
- `Areas/Corporate/FinanceManagement/Purchasing/Models/FinSupplierReturn.cs`,
  `FinSupplierReturnDeposit.cs`, `FinSupplierReturnItem.cs`
- `Areas/Corporate/FinanceManagement/Purchasing/Services/FinanceSupplierReturnService.cs`
  (`CreateAsync`, `ConfirmAsync`, dan empat method deposit milik `BE-FIN-036`)
- `Areas/Corporate/FinanceManagement/Payable/Services/FinancePaymentService.cs` — **diperiksa,
  tidak diubah**: ia hanya memakai `ReserveAsync`/`ReleaseUsageAsync`/`ReleaseReservedByPaymentAsync`/
  `MarkAppliedByPaymentAsync`, nol pemakaian `CreateAsync`/`ConfirmAsync`
- `Repositories/Configurations/Corporate/FinanceManagement/Purchasing/FinPurchasingInvoiceConfiguration.cs`
  — pola `PPNAmount` yang sudah ada (`HasPrecision(18,2)` + `HasDefaultValue(0m)`), ditiru persis
- `Migrations/20260928120000_AddDepositAppliedAmountToFinPayment.cs` — pola migration kolom aditif
  pada tabel berjalan, ditiru persis
- `Migrations/ApplicationDbContextModelSnapshot.cs` — urutan properti dan bentuk check constraint

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/Purchasing/Models/FinSupplierReturn.cs` | Kolom `PPNAmount`; XML doc `TotalAmount` diperjelas (pokok tanpa PPN) |
| `Repositories/Configurations/.../FinSupplierReturnConfiguration.cs` | `PPNAmount` `numeric(18,2)` default `0`; check constraint `CK_FinSupplierReturn_PPNAmount` (`>= 0`) |
| `Areas/Corporate/FinanceManagement/AccountingIntegration/Models/FinAccountingEventOutbox.cs` | Konstanta kode ke-36 `PpnMasukanReturPembelian = "PPN-MASUKAN-RETUR-PEMBELIAN"` |
| `Areas/Corporate/FinanceManagement/Purchasing/Dtos/FinanceSupplierReturnDtos.cs` | `CreateSupplierReturnRequest.PPNAmount` (opsional, default 0); `SupplierReturnResponse.PPNAmount` |
| `Areas/Corporate/FinanceManagement/Purchasing/Services/FinanceSupplierReturnService.cs` | `CreateAsync` bertambah parameter `ppnAmount` + `FIN-VAL-133` + `FIN-VAL-143`; `ConfirmAsync` menghitung kredit `= TotalAmount + PPNAmount` dan menerbitkan kejadian PPN terpisah bila `> 0` |
| `Areas/Corporate/FinanceManagement/Purchasing/Controllers/FinanceSupplierReturnsController.cs` | Meneruskan `request.PPNAmount`; dua pemetaan response membawa `PPNAmount` |
| `Migrations/20260929130000_AddPPNAmountToFinSupplierReturn.cs` + `.Designer.cs` | **Baru** — kolom + check constraint, `Down()` membalik keduanya |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Properti `PPNAmount` pada blok `FinSupplierReturn` + check constraint pada `ToTable` |
| `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` | Tanda status `BE-FIN-043` → 🟡 |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif murni.** `POST /purchasing/supplier-returns` menerima `ppnAmount` opsional — permintaan tanpa field itu berperilaku persis seperti sebelumnya. Response retur bertambah `ppnAmount`. Nol endpoint baru, nol endpoint berubah bentuk |
| Database | Satu kolom + satu check constraint pada `FinSupplierReturn` (**tabel yang sudah berjalan**). Migration `AddPPNAmountToFinSupplierReturn` aditif, **tanpa downtime**, nol backfill — kolom ber-default 0 membuat seluruh baris lama langsung memenuhi constraint |
| Keamanan/Auth | **Nol perubahan.** Nol resource/action baru; atribut `[AccessController]`/`[AccessAction]`/`[AccessPermission]` controller ini tidak disentuh sama sekali |
| **Perubahan perilaku pada kode berjalan** | Satu, dan sudah diantisipasi desain (`§E.9` butir 2): nilai Deposit Retur saat `CONFIRMED` berubah dari `TotalAmount` menjadi `TotalAmount + PPNAmount`. Untuk seluruh retur lama `PPNAmount = 0`, sehingga nilainya **identik** — nol dampak bagi data yang sudah ada |

---

## 4. Dokumentasi endpoint

#### Corporate / Finance Management / Purchasing / Supplier Return

| Method | Path | Kegunaan | Hak akses | Perubahan |
| --- | --- | --- | --- | --- |
| `POST` | `/` | Mencatat Retur Pembelian `DRAFT` | `SupplierReturn : Create` | **Diperluas aditif** — body bertambah `ppnAmount` (opsional, `>= 0`) |
| `POST` | `/{id:guid}/confirm` | Mengonfirmasi retur; menerbitkan Deposit Retur + kejadian | `SupplierReturn : Confirm` | **Perilaku diperluas** — deposit `= pokok + PPN`; kejadian PPN terpisah bila PPN > 0 |

**Contoh request `POST /purchasing/supplier-returns`:**

```json
{
  "purchasingInvoiceId": "3f1c9a2e-0000-4000-8000-000000000001",
  "reason": "Obat rusak saat pengiriman",
  "ppnAmount": 110000.00,
  "items": [
    { "description": "Paracetamol 500mg", "quantity": 100, "unitPrice": 10000.00 }
  ]
}
```

Hasil konfirmasi: Deposit Retur Rp 1.110.000; kejadian `RETUR-PEMBELIAN` Rp 1.000.000; kejadian
`PPN-MASUKAN-RETUR-PEMBELIAN` Rp 110.000.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | — | `NOT RUN` | Aturan berdiri pengguna: jangan build otomatis |
| Retur dengan PPN negatif | Ditolak `400` | `NOT RUN` (review kode) | `CreateAsync` — `FIN-VAL-133` |
| Pokok Rp 1.000.000 + PPN Rp 110.000 atas faktur Rp 1.110.000 | Diterima; deposit Rp 1.110.000 | `NOT RUN` (review kode) | `ConfirmAsync` — `creditAmount` |
| Pokok Rp 1.110.000 + PPN Rp 110.000 atas faktur Rp 1.110.000 | Ditolak `400` (`FIN-VAL-143`) — pokok lolos `FIN-VAL-111`, total ditolak | `NOT RUN` (review kode) | `CreateAsync` — batas kedua |
| Pokok Rp 2.000.000 atas faktur Rp 1.110.000 | Ditolak `422` (`FIN-VAL-111`) — **kode status tidak berubah** | `NOT RUN` (review kode) | Urutan pemeriksaan sengaja: `111` lebih dulu |
| Kejadian `RETUR-PEMBELIAN` bernilai pokok saja | Terpenuhi — `Amount = supplierReturn.TotalAmount`, tidak disentuh task ini | `NOT RUN` (review kode) | `ConfirmAsync` — `FIN-VAL-136` |
| Retur ber-PPN 0 dikonfirmasi | Nol kejadian PPN terbit; deposit = pokok — identik sebelum task ini | `NOT RUN` (review kode) | Penjaga `if (PPNAmount > 0)` |
| Dua kejadian berbagi `SourceTransactionId` yang sama | Tidak bertabrakan — unique index outbox memuat `EventTypeCode`, dan `SourceVersion` dihitung per pasangan (`SourceTransactionId`, `EventTypeCode`) | `NOT RUN` (review kode) | `FinanceAccountingOutboxService.ResolveNextSourceVersionAsync` |
| Regresi jalur `BE-FIN-036` | Nol — `FinancePaymentService` hanya memakai method deposit yang tidak disentuh | `NOT RUN` (review diff) | Grep seluruh pemakaian service ini |

Uji manual/runtime: `NOT FEASIBLE` — memerlukan aplikasi berjalan dan migration task ini
dieksekusi, keduanya belum diotorisasi/dijalankan.

**AUTOMATED TEST:** `NOT APPLICABLE` — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).

**Tidak dijalankan:** `dotnet build`, eksekusi migration, seluruh uji runtime/manual, dan
`Invoke-QbeConformanceCheck.ps1`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria (kartu roadmap) | Status | Bukti |
| --- | --- | --- |
| 1. Migration aditif dengan default 0, seluruh baris lama memenuhi constraint tanpa backfill | Terpenuhi (berkas) | `AddPPNAmountToFinSupplierReturn.Up()` |
| 2. Retur dengan PPN negatif ditolak `400` (`FIN-VAL-143`) | Terpenuhi (source) | Ditegakkan `FIN-VAL-133`; `FIN-VAL-143` menegakkan batas pokok + PPN — lihat catatan 7 |
| 3. Deposit Retur yang terbit bernilai `TotalAmount + PPNAmount` | Terpenuhi (source) | `ConfirmAsync` — `FIN-VAL-135` |
| Migration file aditif, Designer, model snapshot terbarui | Terpenuhi | Tiga berkas |
| Service menghitung saldo deposit termasuk PPN | Terpenuhi | `ConfirmAsync` |
| `dotnet build` berhasil | **Belum** | Sengaja `NOT RUN` |
| Migration dieksekusi di lingkungan pengembangan | **Belum** | Wewenang terpisah, belum diminta |

Task ini **belum** dapat ditandai `✅` — dua butir validasi terakhir belum terpenuhi.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Keputusan interpretasi yang perlu diketahui peninjau | Kartu menyebut kriteria "PPN negatif ditolak `400` (`FIN-VAL-143`)", tetapi pada `validation-matrix.md` **`FIN-VAL-133`** yang mengatur PPN negatif; `FIN-VAL-143` mengatur batas `pokok + PPN ≤ nilai faktur`. Keduanya diimplementasikan, masing-masing sesuai kontrak. Kartu roadmap tampaknya menyingkat dua aturan menjadi satu nomor — dicatat sebagai selisih penulisan kartu, bukan selisih implementasi |
| Keputusan interpretasi kedua | `FIN-VAL-111` (lama, `422`) **sengaja dipertahankan apa adanya** dan `FIN-VAL-143` (baru, `400`) ditambahkan sebagai batas kedua, diperiksa **sesudahnya**. Alternatifnya — mengganti perbandingan `FIN-VAL-111` menjadi pokok + PPN — akan mengubah kode status penolakan yang sudah berjalan dari `422` menjadi `400` untuk kasus yang selama ini sudah ditolak. Bentuk yang dipilih memenuhi `FIN-DES-055` ("batasnya MUST menjadi `TotalAmount + PPNAmount ≤ invoice.TotalAmount`") karena batas yang lebih ketat selalu ikut menjaga, **tanpa** regresi kode status |
| Aturan kontrak yang **tidak** diimplementasikan, beserta alasannya | `FIN-VAL-134` (`TotalAmount` ≠ jumlah `LineTotal` → `422`, diperiksa saat konfirmasi). Tidak dibangun karena **tidak dapat dicapai lewat API**: `TotalAmount` selalu dihitung backend dari rincian barang dan tidak pernah diterima dari permintaan, sehingga divergensi hanya mungkin bila data diubah langsung di database. Menambahkannya menuntut `Include(Items)` tambahan pada `ConfirmAsync` dan berada di luar cakupan kartu. Dicatat di sini, bukan didiamkan |
| Masalah yang diketahui (pra-ada, bukan dari task ini) | `FIN-VAL-111`/`FIN-VAL-143` membandingkan terhadap satu faktur **tanpa akumulasi lintas retur lain** atas faktur yang sama — dua retur berurutan masing-masing setengah nilai faktur tetap lolos keduanya. Ini keterbatasan yang sudah dicatat `BE-FIN-035` dan tidak diperluas task ini |
| Risiko tersisa | `Down()` migration aman **hanya** selama belum ada baris ber-`PPNAmount > 0`. Bila sudah ada, `DROP COLUMN` membuang nilai PPN dan Deposit Retur yang terlanjur terbit sebesar pokok + PPN **MUST** dihitung ulang lebih dulu — tertulis pada komentar migration dan `§E.10` |
| Gerbang yang masih berlaku | Worker pengiriman kode `PPN-MASUKAN-RETUR-PEMBELIAN` **MUST NOT** diaktifkan sebelum Accounting meratifikasinya (`FIN-OQ-029`). Gerbang itu ada di level worker, bukan penulisan — baris outbox tetap ditulis `PENDING`, pola yang sama dengan `FIN-VAL-122`. Nol worker pengiriman dibangun task ini |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M` tujuh berkas (model, configuration, DTO, service, controller, konstanta outbox, snapshot) + `??` dua berkas migration + `??` laporan ini |
| Langkah berikutnya | (1) `dotnet build`; (2) otorisasi dan eksekusi migration `AddPPNAmountToFinSupplierReturn` — perhatikan ia mensyaratkan `AddPurchasingApRumpun` (`BE-FIN-031`) sudah dieksekusi lebih dulu karena tabelnya lahir di sana; (3) uji runtime `FIN-TEST` rumpun retur; (4) `BE-FIN-044` sudah 🟡 dan `BE-FIN-045`/`046`/`047` bebas dikerjakan — tidak bergantung pada task ini |
