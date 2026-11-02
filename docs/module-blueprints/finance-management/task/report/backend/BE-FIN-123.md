# Laporan Perubahan Backend — `BE-FIN-123`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-123` |
| Judul | Faktur supplier dengan materai dan ongkir dapat dicatat dengan komponen yang jelas, tanpa menyamarkan biaya sebagai potongan negatif |
| Slice | `REV-22` — Finance AP mengikuti BLM, `Slice B` |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md`, bagian "AMENDMENT ROADMAP REVISI 22" |
| Trace | `AP-UAT-10` (dokumen proses bisnis AP BLM, 26 Oktober 2026); `AP-DEC-04`, diterima pemilik pada instruksi "lanjutkan pengerjaan slice b" 26 Oktober 2026 atas rekomendasi agen |
| Contract version | `FIN-VAL-107` dan `FIN-API-1.1` §B.4 (locked) **diperluas** tanpa diedit: rumus total bertambah dua suku penambah, request dan response bertambah dua field aditif. Dicatat sebagai delta pada roadmap `REV-22` |
| Dependency | Tidak ada |
| Klasifikasi | `MEDIUM` — skor 6: repository 0; berkas diperiksa 1; berkas diubah 1 (5 source + 3 artefak migration); logika bisnis 1; kontrak API 1 (aditif); database 2 (dua kolom pada tabel existing, migration); keamanan/auth 0; UI/workflow 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` branch `Yasmina`: `Areas/Corporate/FinanceManagement/Purchasing/`, `Repositories/Configurations/.../Purchasing/`, `Migrations/`, `docs/module-blueprints/finance-management/` |
| Model | Claude Sonnet 5.5 |
| Commit backend saat dikerjakan | `48011b09` (branch `Yasmina`), dengan perubahan `BE-FIN-120`..`122` dan pekerjaan Billing milik pengguna yang belum di-commit |
| Tanggal | 2026-10-26 |
| Status | 🟡 **SEBAGIAN** — seluruh kriteria terpetakan ke source; `dotnet build` **NOT RUN**, migration belum diterapkan |

---

## 1. Masalah yang diperbaiki

Faktur supplier di BLM memuat bea materai dan ongkos kirim sebagai komponen tersendiri, dikirim terpisah dari PPN. `FinPurchasingInvoice` tidak punya tempat
untuk keduanya: rumus totalnya hanya `Subtotal − Diskon + PPN − DP − Potongan lain`. Petugas yang harus mencatat faktur bermaterai dan berongkir terpaksa
memasukkannya ke tempat yang salah, misalnya sebagai baris barang fiktif, atau mengurangi nilai lain agar angkanya "pas". Dokumen proses bisnis AP melarang
menyamarkan biaya positif sebagai potongan negatif.

**Contoh.** Obat Rp 900.000, PPN 11% Rp 99.000, materai Rp 10.000, ongkir Rp 15.000. Total faktur supplier Rp 1.024.000. Sebelum task ini, total itu ditolak
sebagai "tidak seimbang" karena materai dan ongkir tidak dikenal.

---

## 2. Proses bisnis

**Pelaku:** petugas AP (menyusun), penyetuju Finance (menyetujui).

**Alur:**

1. Petugas AP menyusun invoice dari Tukar Faktur, mengisi barang, diskon, PPN, DP, potongan lain, **materai**, **ongkir**, dan total.
2. Sistem menolak materai atau ongkir negatif (`400`), lalu memeriksa total: `Subtotal − Diskon + PPN + Materai + Ongkir − DP − Potongan lain` harus sama dengan total yang dikirim (`422` bila tidak).
3. Invoice diajukan dan disetujui seperti biasa. Saat disetujui, utang dibentuk sebesar **total penuh** (termasuk materai dan ongkir).
4. Kejadian ke Accounting: `PENGAKUAN-HUTANG-SUPPLIER` bernilai `Total − PPN`, `PPN-MASUKAN-PEMBELIAN` bernilai PPN. Keduanya tetap berjumlah sama dengan utang.

**Contoh berangka:**

| Komponen | Nilai |
| --- | ---: |
| Subtotal barang | Rp 900.000 |
| Diskon | Rp 0 |
| PPN | Rp 99.000 |
| Materai | Rp 10.000 |
| Ongkir | Rp 15.000 |
| DP dan potongan lain | Rp 0 |
| **Total (harus dikirim klien)** | **Rp 1.024.000** |
| Kejadian `PENGAKUAN-HUTANG-SUPPLIER` | Rp 925.000 |
| Kejadian `PPN-MASUKAN-PEMBELIAN` | Rp 99.000 |

**Jalur tidak normal:** total Rp 999.000 untuk komponen yang sama ditolak `422`; materai atau ongkir negatif ditolak `400`; klien lama yang tidak mengirim kedua field
tetap diterima karena nilainya 0.

**Keputusan yang dipakai:** materai dan ongkir **tidak masuk dasar PPN** (mengikuti builder BLM). PPN tetap angka kiriman klien yang divalidasi lewat keseimbangan total; sistem tidak menghitung PPN sendiri.
**Koreksi atas rekomendasi sebelumnya:** nilai kejadian pengakuan **tidak dipisah** dari materai dan ongkir. Memisahkannya membuat buku besar Accounting lebih kecil daripada buku pembantu utang.
Penempatan akun untuk porsi itu adalah keputusan Accounting dan butuh kode kejadian tersendiri bila ingin dipisah.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `Areas/Corporate/FinanceManagement/Purchasing/Models/FinPurchasingInvoice.cs` dan `Repositories/Configurations/.../Purchasing/FinPurchasingInvoiceConfiguration.cs`
- `Areas/Corporate/FinanceManagement/Purchasing/Services/FinancePurchasingInvoiceService.cs`, `Controllers/FinancePurchasingInvoicesController.cs`, `Dtos/FinancePurchasingInvoiceDtos.cs`
- `Migrations/20260929130000_AddPPNAmountToFinSupplierReturn.cs` (pola kolom aditif), `Migrations/ApplicationDbContextModelSnapshot.cs`
- `docs/module-blueprints/finance-management/contracts/validation-matrix.md` (`FIN-VAL-107`)
- Pencarian `OtherDeductionAmount`/`DownPaymentAmount` pada seluruh `Areas/` untuk memastikan tidak ada penghitung total lain

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Models/FinPurchasingInvoice.cs` | Properti `StampDutyAmount` dan `ShippingCostAmount`; ringkasan invarian diperbarui |
| `Repositories/Configurations/.../FinPurchasingInvoiceConfiguration.cs` | Presisi `(18,2)` dan default 0 untuk kedua kolom |
| `Dtos/FinancePurchasingInvoiceDtos.cs` | Dua field pada `PurchasingInvoiceResponse`, `CreatePurchasingInvoiceRequest`, `UpdatePurchasingInvoiceRequest` |
| `Services/FinancePurchasingInvoiceService.cs` | `CreateAsync`/`UpdateAsync` menerima dua parameter baru; `ValidateCharges` baru; `ValidateBalance` memuat dua suku penambah; komentar nilai kejadian Accounting |
| `Controllers/FinancePurchasingInvoicesController.cs` | Meneruskan dan memetakan kedua field pada Create, Update, `Map`, dan `MapDetail` |
| `Migrations/20261026120000_AddPurchasingApBlmComponents.cs`, `.Designer.cs`, `ApplicationDbContextModelSnapshot.cs` | Migration bersama `BE-FIN-123`..`125` (lihat bagian 3.3) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Aditif: `stampDutyAmount` dan `shippingCostAmount` pada request dan response invoice, default 0. Konsumen lama tidak rusak. `FIN-VAL-107` berubah maknanya (rumus lebih panjang) |
| Database | Dua kolom `numeric(18,2) NOT NULL DEFAULT 0` pada `public."FinPurchasingInvoice"`. Migration `20261026120000_AddPurchasingApBlmComponents` (bersama `BE-FIN-124` dan `125`), Designer, dan snapshot disertakan. **Belum diterapkan ke database**: pemilik menjalankan `dotnet ef database update`. Invoice lama langsung memenuhi rumus baru karena nilainya 0, tanpa backfill |
| Keamanan/Auth | `NOT APPLICABLE` |

---

## 4. Dokumentasi endpoint

#### Corporate / Finance Management / Purchasing / Purchasing Invoice

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/corporate/finance-management/purchasing/purchasing-invoices` | Menyusun invoice; request memuat `stampDutyAmount` dan `shippingCostAmount` (opsional) | `FinancePurchasingInvoice : Create` |
| `PUT` | `/api/v1/corporate/finance-management/purchasing/purchasing-invoices/{id}` | Memperbaiki invoice `DRAFT`; field yang sama | `FinancePurchasingInvoice : Update` |
| `GET` | `/api/v1/corporate/finance-management/purchasing/purchasing-invoices` dan `/{id}` | Response memuat kedua komponen | `FinancePurchasingInvoice : Read` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Instruksi pemilik: build dijalankan mandiri oleh pemilik. Build penuh membawa 3 berkas Designer 5,7 MB dan berlangsung lama |
| QBE preflight | `TOUCHED LEGACY`; `QBE-VAL-001`, `QBE-CFG-002`, `QBE-API-001`, `QBE-DTO-001`, `QBE-LOG-001` terpenuhi | `PASS` | Penelusuran source |
| Review diff dan scope | Lima berkas source, tiga artefak migration, roadmap | `PASS` | `git status --short` |
| Tidak ada penghitung total lain | `OtherDeductionAmount`/`DownPaymentAmount` hanya muncul di empat berkas yang semuanya diperbarui | `PASS` | Pencarian pada `Areas/` |
| Penelusuran contoh berangka | 900.000 + 99.000 + 10.000 + 15.000 = 1.024.000; kejadian 925.000 + 99.000 = 1.024.000; total 999.000 ditolak | `PASS` | Perhitungan atas `ValidateBalance` dan `ApproveAsync` |
| Snapshot dan Designer | Snapshot +23 baris (tiga blok sisipan pada tiga entity yang benar); Designer = snapshot + 3 baris header | `PASS` | `git diff` snapshot; jumlah baris 138.194 dan 138.197 |
| Migration diterapkan ke database | Tidak dijalankan | `NOT RUN` | Wewenang pemilik |

`AUTOMATED TEST: NOT APPLICABLE — task ini tidak disertai test otomatis; persetujuan pemilik atas test hanya mencakup BE-FIN-120 dan BE-FIN-121 (rules/backend/TEST_POLICY.md).`

Uji manual: `NOT FEASIBLE` — server backend tidak dijalankan.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Total Rp 1.024.000 untuk komponen contoh diterima | Terpetakan ke source; belum dibuktikan build | `ValidateBalance` |
| 2. Total Rp 999.000 untuk komponen yang sama ditolak `422` | Terpetakan ke source; belum dibuktikan build | `ValidateBalance` |
| 3. Materai atau ongkir negatif ditolak `400` | Terpetakan ke source; belum dibuktikan build | `ValidateCharges` |
| 4. Klien lama tanpa kedua field tetap diterima | Terpetakan ke source; belum dibuktikan build | Default `0m` pada DTO dan model |
| 5. Setelah disetujui: utang Rp 1.024.000, kejadian Rp 925.000 dan Rp 99.000 | Terpetakan ke source; belum dibuktikan build | `ApproveAsync`: item sintetis `TotalAmount`, override `Total − PPN` |
| DoD: preflight, migration, Designer, snapshot | Terpenuhi | Bagian 3 dan 5 |
| DoD: `dotnet build` berhasil dan migration diterapkan | **Belum terpenuhi** | `NOT RUN` |
| DoD: laporan tracked ada | Terpenuhi | Berkas ini |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Materai dan ongkir masuk ke nilai `PENGAKUAN-HUTANG-SUPPLIER`. Accounting perlu mengetahui bahwa porsi itu kini ada di dalamnya |
| Masalah yang diketahui | Barang pada invoice masih berupa nama bebas tanpa tautan ke baris PO/GR, sehingga pencocokan per barang belum mungkin (pekerjaan lanjutan) |
| Risiko tersisa | Rendah untuk kebenaran logika. Migration belum diterapkan, build belum dijalankan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat `git status --short`; berkas task ini tercantum pada bagian 3.2 |
| Langkah berikutnya | Pemilik menjalankan `dotnet build` lalu `dotnet ef database update`; konfirmasi perlakuan akun materai/ongkir ke Accounting; lalu `BE-FIN-123` dinaikkan ke ✅ |
