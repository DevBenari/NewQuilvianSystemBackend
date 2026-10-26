# Laporan Perubahan Backend — `BE-FIN-124`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-124` |
| Judul | Jatuh tempo utang dari invoice yang merujuk Tanda Terima Barang dihitung dari tanggal barang diterima, dan dasar perhitungannya tersimpan |
| Slice | `REV-22` — Finance AP mengikuti BLM, `Slice B` |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md`, bagian "AMENDMENT ROADMAP REVISI 22" |
| Trace | `AP-UAT-11` (dokumen proses bisnis AP BLM, bagian 10.2); `AP-DEC-03`, diterima pemilik pada instruksi "lanjutkan pengerjaan slice b" 26 Oktober 2026 atas rekomendasi agen |
| Contract version | Aturan jatuh tempo ledger pada `data-dictionary.md` §4.1 (tanggal faktur + termin) **berubah untuk invoice yang merujuk GR**; `SupplierPayableResponse` bertambah `dueDateBasis`. Dicatat sebagai delta pada roadmap `REV-22`; kontrak yang approved tidak diedit |
| Dependency | `BE-FIN-121` 🟡 — memastikan GR yang sudah dirujuk Tukar Faktur tidak dapat dibatalkan, sehingga tanggal terima yang dipakai tidak bisa hilang |
| Klasifikasi | `MEDIUM` — skor 6: repository 0; berkas diperiksa 1; berkas diubah 1 (6 source + artefak migration); logika bisnis 1; kontrak API 1 (aditif); database 2 (satu kolom, migration); keamanan/auth 0; UI/workflow 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` branch `Yasmina`: `Areas/Corporate/FinanceManagement/Payable/` dan `Purchasing/`, `Repositories/Configurations/`, `Migrations/`, `docs/module-blueprints/finance-management/` |
| Model | Claude Sonnet 5.5 |
| Commit backend saat dikerjakan | `48011b09` (branch `Yasmina`), dengan perubahan `BE-FIN-120`..`123` dan pekerjaan Billing milik pengguna yang belum di-commit |
| Tanggal | 2026-10-26 |
| Status | 🟡 **SEBAGIAN** — seluruh kriteria terpetakan ke source; `dotnet build` **NOT RUN**, migration belum diterapkan |

---

## 1. Masalah yang diperbaiki

Jatuh tempo utang supplier dihitung dari **tanggal faktur** + termin, sedangkan BLM menghitungnya dari **tanggal barang diterima** + termin (default form penerimaan).
Dua tanggal itu sering berbeda, dan estimasi jatuh tempo pada Tukar Faktur memakai tanggal ketiga (tanggal dokumen diterima AP). Tanpa dasar yang tersimpan, selisih
antar tanggal tidak dapat dijelaskan dan jadwal bayar bisa meleset dari kesepakatan dengan supplier.

**Contoh.** Termin 30 hari. Barang diterima 10 Oktober 2026; faktur bertanggal 8 Oktober 2026. Aturan lama: jatuh tempo 7 November 2026. Aturan BLM: 9 November 2026.

---

## 2. Proses bisnis

**Pelaku:** penyetuju Finance (menyetujui invoice, yang membentuk utang).

**Alur saat invoice disetujui:**

1. Sistem melihat Tukar Faktur sumber. Bila Tukar Faktur merujuk GR, tanggal terima GR itu dipakai.
2. Jatuh tempo utang = tanggal terima barang + termin supplier; dasar tersimpan sebagai `GOODS_RECEIVED_DATE`.
3. Bila Tukar Faktur tidak merujuk GR, perilaku lama berlaku: tanggal faktur + termin; dasar `SUPPLIER_INVOICE_DATE`.
4. Utang yang diinput manual selalu tanggal faktur + termin, dasar `SUPPLIER_INVOICE_DATE`.
5. Utang lama dan saldo awal hasil migrasi: jatuh tempo tidak berubah, dasar kosong (tidak tercatat).

**Contoh berangka:**

| Skenario | Tanggal acuan | Termin | Jatuh tempo | Dasar |
| --- | --- | ---: | --- | --- |
| Invoice ber-GR, GR 10 Oktober, faktur 8 Oktober 2026 | 10 Oktober 2026 | 30 hari | 9 November 2026 | `GOODS_RECEIVED_DATE` |
| Invoice tanpa GR, faktur 8 Oktober 2026 | 8 Oktober 2026 | 30 hari | 7 November 2026 | `SUPPLIER_INVOICE_DATE` |
| Input manual, faktur 8 Oktober 2026 | 8 Oktober 2026 | 30 hari | 7 November 2026 | `SUPPLIER_INVOICE_DATE` |
| Utang lama | — | — | tetap | kosong |

**Yang tidak berubah:** estimasi jatuh tempo pada Tukar Faktur (tanggal dokumen diterima + termin) tetap terpisah dan tidak disentuh. Termin disalin dari master saat utang dibuat dan tidak berubah kemudian.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `Payable/Models/FinSupplierPayable.cs` dan `Repositories/Configurations/.../Payable/FinSupplierPayableConfiguration.cs`
- `Payable/Services/FinanceSupplierPayableService.cs` (`CreateAsync`), `Purchasing/Services/FinancePurchasingInvoiceService.cs` (`ApproveAsync`)
- `Payable/Dtos/FinanceSupplierPayableDtos.cs`, `Payable/Controllers/FinanceSupplierPayablesController.cs`
- `Purchasing/Models/FinInvoiceExchange.cs`, `FinGoodsReceipt.cs`
- Pemanggil `FinanceSupplierPayableService.CreateAsync`: invoice service dan controller utang manual (parameter baru bersifat opsional, di akhir)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Payable/Models/FinSupplierPayable.cs` | Properti `DueDateBasis` (nullable) dan kelas konstanta `FinSupplierPayableDueDateBases` |
| `Repositories/Configurations/.../FinSupplierPayableConfiguration.cs` | `DueDateBasis` panjang maksimal 30 |
| `Payable/Services/FinanceSupplierPayableService.cs` | `CreateAsync` menerima `goodsReceivedDate` opsional; `DueDate` dan `DueDateBasis` mengikutinya |
| `Purchasing/Services/FinancePurchasingInvoiceService.cs` | `ApproveAsync` membaca tanggal terima GR dari Tukar Faktur dan meneruskannya |
| `Payable/Dtos/FinanceSupplierPayableDtos.cs`, `Controllers/FinanceSupplierPayablesController.cs` | `dueDateBasis` pada response dan pemetaannya |
| `Migrations/20261026120000_AddPurchasingApBlmComponents.*`, `ApplicationDbContextModelSnapshot.cs` | Migration bersama (lihat laporan `BE-FIN-123`) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Aditif: `dueDateBasis` pada response utang. Tidak ada endpoint baru. Nilai `dueDate` utang baru dari invoice ber-GR dapat lebih besar daripada sebelumnya |
| Database | Satu kolom nullable `varchar(30)` pada `public."FinSupplierPayable"` lewat migration bersama; tanpa backfill. **Belum diterapkan ke database** |
| Keamanan/Auth | `NOT APPLICABLE` |

---

## 4. Dokumentasi endpoint

Tidak ada endpoint baru. Respons utang kini memuat `dueDateBasis` karena semua jalur memakai pemeta yang sama (`FinanceSupplierPayablesController.MapPayable`/`Map`).

#### Corporate / Finance Management / Supplier Payable

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/finance-management/supplier-payables/{id}` | Rincian utang memuat `dueDateBasis` | Tidak diubah |
| `POST` | `/api/v1/corporate/finance-management/supplier-payables` dan `/{id}/cancel` | Respons utang memuat `dueDateBasis` | Tidak diubah |

#### Corporate / Finance Management / AP V2

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/finance/payable` dan `/{id}` | Daftar dan rincian utang memuat `dueDateBasis` | `Finance.AP : View` (tidak diubah) |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Instruksi pemilik: build dijalankan mandiri |
| QBE preflight | `TOUCHED LEGACY`; `QBE-VAL-001`, `QBE-CFG-002`, `QBE-API-001`, `QBE-TXN-001` (pembacaan GR dalam transaksi persetujuan yang sama), `QBE-LOG-001` terpenuhi | `PASS` | Penelusuran source |
| Kompatibilitas pemanggil `CreateAsync` | Parameter baru opsional di akhir; pemanggil lain tidak berubah | `PASS` | Pencarian pemanggil |
| Penelusuran contoh berangka | 10 Oktober 2026 + 30 hari = 9 November 2026; 8 Oktober 2026 + 30 hari = 7 November 2026 | `PASS` | Perhitungan atas `CreateAsync` |
| Snapshot, Designer, migration | Lihat laporan `BE-FIN-123` | `PASS` | `git diff` snapshot |
| Migration diterapkan ke database | Tidak dijalankan | `NOT RUN` | Wewenang pemilik |

`AUTOMATED TEST: NOT APPLICABLE — task ini tidak disertai test otomatis; persetujuan pemilik atas test hanya mencakup BE-FIN-120 dan BE-FIN-121 (rules/backend/TEST_POLICY.md).`

Uji manual: `NOT FEASIBLE` — server backend tidak dijalankan.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Invoice ber-GR: jatuh tempo 9 November 2026, dasar `GOODS_RECEIVED_DATE` | Terpetakan ke source; belum dibuktikan build | `ApproveAsync` meneruskan `goodsReceivedDate`; `CreateAsync` |
| 2. Invoice tanpa GR: tanggal faktur + termin, dasar `SUPPLIER_INVOICE_DATE` | Terpetakan ke source; belum dibuktikan build | `goodsReceivedDate` null |
| 3. Input manual: tanggal faktur + termin | Terpetakan ke source; belum dibuktikan build | Pemanggil manual tidak meneruskan parameter |
| 4. Utang lama: jatuh tempo tetap, dasar kosong | Terpetakan ke source; belum dibuktikan migration | Kolom nullable tanpa backfill |
| 5. Rincian dan daftar utang menampilkan `dueDateBasis` | Terpetakan ke source; belum dibuktikan build | `MapPayable` dan `Map` dipakai juga oleh `FinanceApController` (daftar dan rincian) |
| DoD: preflight, migration, Designer, snapshot | Terpenuhi | Bagian 3 dan 5 |
| DoD: `dotnet build` berhasil dan migration diterapkan | **Belum terpenuhi** | `NOT RUN` |
| DoD: laporan tracked ada | Terpenuhi | Berkas ini |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Aging utang (`GetAgingSummaryAsync`) memakai `DueDate`, sehingga utang baru ber-GR dapat masuk bucket yang berbeda dari perkiraan lama. Bukan cacat, tetapi perubahan yang terlihat pada laporan |
| Masalah yang diketahui | Bila GR sumber tidak ditemukan atau tidak berstatus `RECEIVED` saat persetujuan, jatuh tempo kembali ke tanggal faktur dan dasarnya tercatat demikian (tidak diam-diam memakai tanggal lain). Setelah `BE-FIN-121` kondisi itu seharusnya tidak terjadi |
| Risiko tersisa | Rendah untuk kebenaran logika. Migration belum diterapkan, build belum dijalankan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat `git status --short`; berkas task ini tercantum pada bagian 3.2 |
| Langkah berikutnya | Pemilik menjalankan `dotnet build` lalu `dotnet ef database update`; lalu `BE-FIN-124` dinaikkan ke ✅ |
