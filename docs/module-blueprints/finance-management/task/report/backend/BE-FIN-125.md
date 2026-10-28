# Laporan Perubahan Backend — `BE-FIN-125`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-125` |
| Judul | Barang yang diterima dalam beberapa batch tercatat lengkap dengan nomor batch dan tanggal kedaluwarsanya |
| Slice | `REV-22` — Finance AP mengikuti BLM, `Slice B` |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md`, bagian "AMENDMENT ROADMAP REVISI 22" |
| Trace | `AP-UAT-06` (dokumen proses bisnis AP BLM) |
| Contract version | `FIN-API-1.1` §B.2 (locked) diperluas aditif tanpa diedit: request dan response baris GR bertambah dua field opsional. Dicatat sebagai delta pada roadmap `REV-22` |
| Dependency | Tidak ada |
| Klasifikasi | `MEDIUM` — skor 4: repository 0; berkas diperiksa 0; berkas diubah 1 (5 source + artefak migration); logika bisnis 0; kontrak API 1 (aditif); database 2 (dua kolom, migration); keamanan/auth 0; UI/workflow 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` branch `Yasmina`: `Areas/Corporate/FinanceManagement/Purchasing/`, `Repositories/Configurations/.../Purchasing/`, `Migrations/`, `docs/module-blueprints/finance-management/` |
| Model | Claude Sonnet 5.5 |
| Commit backend saat dikerjakan | `48011b09` (branch `Yasmina`), dengan perubahan `BE-FIN-120`..`124` dan pekerjaan Billing milik pengguna yang belum di-commit |
| Tanggal | 2026-10-26 |
| Status | 🟡 **SEBAGIAN** — seluruh kriteria terpetakan ke source; `dotnet build` **NOT RUN**, migration belum diterapkan |

---

## 1. Masalah yang diperbaiki

Penerimaan barang obat dan alkes di BLM mencatat nomor batch dan tanggal kedaluwarsa per baris. `FinGoodsReceiptItem` hanya menyimpan PO item, kuantitas, dan catatan bebas, sehingga
batch dan kedaluwarsa hilang atau terpaksa ditulis di catatan tanpa struktur. Bila satu baris PO datang dalam dua batch, tidak ada tempat yang jelas untuk membedakannya.

---

## 2. Proses bisnis

**Pelaku:** gudang atau farmasi (mencatat penerimaan).

**Alur:**

1. Petugas memilih PO dan mengisi baris penerimaan. Satu baris PO yang datang dalam beberapa batch dikirim sebagai **beberapa baris** dengan batch berbeda.
2. Tiap baris boleh memuat nomor batch dan tanggal kedaluwarsa. Keduanya opsional.
3. Sistem tetap menegakkan kuantitas: akumulasi seluruh baris pada satu baris PO tidak boleh melebihi pesanan (`422`), apa pun batch-nya.
4. GR tersimpan; batch dan kedaluwarsa tampil pada rincian GR.

**Contoh berangka:** PO 100 unit. Batch A 60 unit (kedaluwarsa 31 Desember 2027) dan batch B 40 unit (30 Juni 2028) dalam satu GR: akumulasi 100, PO menjadi `FULLY_RECEIVED`.
Bila batch B dikirim 41 unit: ditolak `422` karena 60 + 41 = 101 melebihi 100.

**Jalur tidak normal:** nomor batch lebih dari 100 karakter ditolak `400`. GR tanpa batch tetap diterima. GR lama tampil dengan batch kosong.

**Yang sengaja tidak dilakukan:** tanggal kedaluwarsa tidak divalidasi terhadap tanggal terima (aturan menolak barang yang sudah kedaluwarsa belum diputuskan), dan GR Finance **tidak** menambah stok obat/alkes; efek stok tetap milik domain persediaan (`AP-DEC-09`).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `Purchasing/Models/FinGoodsReceiptItem.cs`, `Repositories/Configurations/.../FinGoodsReceiptItemConfiguration.cs`
- `Purchasing/Services/FinanceGoodsReceiptService.cs`, `Controllers/FinanceGoodsReceiptsController.cs`, `Dtos/FinanceGoodsReceiptDtos.cs`
- Pencarian pembuat `GoodsReceiptItemRequest` pada seluruh repository (hanya controller GR)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Purchasing/Models/FinGoodsReceiptItem.cs` | Properti `BatchNumber` (maks 100) dan `ExpiredDate` (nullable) |
| `Repositories/Configurations/.../FinGoodsReceiptItemConfiguration.cs` | Panjang batch 100; `ExpiredDate` bertipe `date` |
| `Purchasing/Dtos/FinanceGoodsReceiptDtos.cs` | Dua field pada request dan response baris GR |
| `Purchasing/Services/FinanceGoodsReceiptService.cs` | Record `GoodsReceiptItemRequest` memuat dua parameter opsional; `CreateAsync` memangkas dan memvalidasi panjang batch lalu menyimpannya |
| `Purchasing/Controllers/FinanceGoodsReceiptsController.cs` | Meneruskan dan memetakan kedua field |
| `Migrations/20261026120000_AddPurchasingApBlmComponents.*`, `ApplicationDbContextModelSnapshot.cs` | Migration bersama (lihat laporan `BE-FIN-123`) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Aditif dan opsional; klien lama tidak rusak |
| Database | Dua kolom nullable pada `public."FinGoodsReceiptItem"` (`varchar(100)` dan `date`) lewat migration bersama; tanpa backfill. **Belum diterapkan ke database** |
| Keamanan/Auth | `NOT APPLICABLE` |

---

## 4. Dokumentasi endpoint

#### Corporate / Finance Management / Purchasing / Goods Receipt

Grup sesuai `[Tags(...)]` `FinanceGoodsReceiptsController`.

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/corporate/finance-management/purchasing/goods-receipts` | Mencatat GR; baris memuat `batchNumber` dan `expiredDate` (opsional) | Tidak diubah |
| `GET` | `/api/v1/corporate/finance-management/purchasing/goods-receipts/{id}` | Rincian GR menampilkan batch dan kedaluwarsa per baris | Tidak diubah |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Instruksi pemilik: build dijalankan mandiri |
| QBE preflight | `TOUCHED LEGACY`; `QBE-VAL-001`, `QBE-CFG-002`, `QBE-API-001`, `QBE-DTO-001` terpenuhi | `PASS` | Penelusuran source |
| Akumulasi kuantitas tetap berlaku untuk banyak baris pada PO item yang sama | `receivedSoFar` terakumulasi per PO item dalam satu permintaan; 60 + 40 diterima, 60 + 41 ditolak | `PASS` | Penelusuran `CreateAsync` |
| Kompatibilitas pembuat record | Parameter baru opsional; hanya controller yang membuat `GoodsReceiptItemRequest` | `PASS` | Pencarian pada repository |
| Snapshot, Designer, migration | Lihat laporan `BE-FIN-123` | `PASS` | `git diff` snapshot |
| Migration diterapkan ke database | Tidak dijalankan | `NOT RUN` | Wewenang pemilik |

`AUTOMATED TEST: NOT APPLICABLE — task ini tidak disertai test otomatis; persetujuan pemilik atas test hanya mencakup BE-FIN-120 dan BE-FIN-121 (rules/backend/TEST_POLICY.md).`

Uji manual: `NOT FEASIBLE` — server backend tidak dijalankan.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. PO 100 unit, batch A 60 dan B 40 dalam satu GR: akumulasi 100, `FULLY_RECEIVED` | Terpetakan ke source; belum dibuktikan build | `CreateAsync` (akumulasi `receivedSoFar`, `RecomputePurchaseOrderStatus`) |
| 2. Batch B 41 ditolak `422` | Terpetakan ke source; belum dibuktikan build | Pemeriksaan melebihi pesanan (`FIN-VAL-104`) |
| 3. Batch dan kedaluwarsa tampil pada rincian GR | Terpetakan ke source; belum dibuktikan build | Pemetaan `GoodsReceiptItemResponse` |
| 4. Nomor batch > 100 karakter ditolak `400` | Terpetakan ke source; belum dibuktikan build | Pemeriksaan panjang pada `CreateAsync` |
| 5. GR tanpa batch tetap diterima; GR lama batch kosong | Terpetakan ke source; belum dibuktikan migration | Kolom nullable, parameter opsional |
| DoD: preflight, migration, Designer, snapshot | Terpenuhi | Bagian 3 dan 5 |
| DoD: `dotnet build` berhasil dan migration diterapkan | **Belum terpenuhi** | `NOT RUN` |
| DoD: laporan tracked ada | Terpenuhi | Berkas ini |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kondisi barang dan satuan terima (UOM) belum dicatat; keduanya membutuhkan keputusan pemetaan produk dan satuan |
| Masalah yang diketahui | Nomor batch tidak dijaga unik dan tidak dihubungkan ke master obat; hanya data bawaan dokumen |
| Risiko tersisa | Rendah untuk kebenaran logika. Migration belum diterapkan, build belum dijalankan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat `git status --short`; berkas task ini tercantum pada bagian 3.2 |
| Langkah berikutnya | Pemilik menjalankan `dotnet build` lalu `dotnet ef database update`; lalu `BE-FIN-125` dinaikkan ke ✅ |
