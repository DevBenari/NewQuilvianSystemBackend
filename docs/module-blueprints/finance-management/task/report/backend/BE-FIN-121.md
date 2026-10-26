# Laporan Perubahan Backend — `BE-FIN-121`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-121` |
| Judul | Faktur supplier hanya dapat ditautkan ke PO/GR milik supplier yang sama, GR yang sudah dibatalkan tidak dapat menjadi dasar faktur, dan GR yang sudah dirujuk faktur tidak dapat dibatalkan |
| Slice | `REV-22` Slice A — Finance AP mengikuti BLM |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md`, bagian "AMENDMENT ROADMAP REVISI 22" |
| Trace | `AP-UAT-09` (dokumen proses bisnis AP BLM, 26 Oktober 2026); `FIN-DEC-051`; `state-transition-matrix.md` §B.2 dan §B.3 |
| Contract version | `FIN-VAL-1.13` (approved) tidak diedit. Tiga aturan baru (supplier TF = supplier PO/GR, GR `CANCELLED` tidak dapat dirujuk TF, pemeriksaan rujukan TF di dalam kunci PO) dicatat sebagai delta pada roadmap `REV-22` |
| Dependency | Tidak ada |
| Klasifikasi | `MEDIUM` — skor 4: repository 0; berkas diperiksa 1 (sekitar 9); berkas diubah 0 (2 source + 1 test); logika bisnis 1 (penguncian lintas-dokumen); kontrak API 1 (memakai kontrak yang ada); database 1 (hanya perilaku query dan transaksi yang ada); keamanan/auth 0; UI/workflow 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` branch `Yasmina`: `Areas/Corporate/FinanceManagement/Purchasing/`, `Tests/QuilvianSystemBackend.PharmacyTests/`, `docs/module-blueprints/finance-management/` |
| Model | Claude Sonnet 5.5 |
| Commit backend saat dikerjakan | `48011b09` (branch `Yasmina`). Working tree sudah memuat perubahan Billing milik pengguna yang tidak disentuh task ini |
| Tanggal | 2026-10-26 |
| Status | 🟡 **SEBAGIAN** — seluruh kriteria terpetakan ke source dan test; `dotnet build` dan test otomatis **NOT RUN** |

---

## 1. Masalah yang diperbaiki

**Masalah 1 — faktur supplier A dapat ditempelkan ke penerimaan barang supplier B.** `FinanceInvoiceExchangeService.CreateAsync` hanya
memeriksa bahwa PO ada dan bahwa GR milik PO yang sama. Supplier Tukar Faktur tidak pernah dibandingkan dengan supplier PO/GR. Akibatnya
dokumen yang saling bertentangan lolos masuk antrean verifikasi, dan setelah disetujui utangnya terbentuk atas nama supplier pada Tukar Faktur.

Contoh: PO-001 dan GR-001 milik Supplier A. Petugas mencatat Tukar Faktur atas nama Supplier B dan merujuk PO-001. Sebelum perbaikan, Tukar Faktur
tersimpan berstatus `RECEIVED`.

**Masalah 2 — Tukar Faktur dapat merujuk GR yang sudah dibatalkan.** GR berstatus `CANCELLED` bukan lagi bukti barang diterima, tetapi
pembuatan Tukar Faktur tidak memeriksa status GR.

**Masalah 3 — celah balapan antara pembatalan GR dan pembuatan Tukar Faktur.** `FinanceGoodsReceiptService.CancelAsync` memeriksa "GR sudah
dirujuk Tukar Faktur" **sebelum** transaksi dan kunci PO diambil, sedangkan pembuatan Tukar Faktur tidak mengambil kunci apa pun. Bila keduanya
berjalan hampir bersamaan, pemeriksaan pembatalan dapat lulus tepat sebelum Tukar Faktur tersimpan, sehingga GR terbatalkan padahal sudah
dirujuk faktur. Memindahkan pemeriksaan ke dalam kunci saja tidak cukup; sisi pembuatan Tukar Faktur harus memakai kunci yang sama.

---

## 2. Proses bisnis

**Pelaku:** petugas AP (mencatat Tukar Faktur), gudang atau farmasi (mencatat dan membatalkan GR).

**Alur sesudah perbaikan, pencatatan Tukar Faktur:**

1. Petugas AP memilih supplier, nomor dan tanggal faktur, tanggal dokumen diterima, dan (opsional) PO serta GR yang dicakup.
2. Sistem memeriksa supplier aktif dan nomor faktur terisi (tidak berubah).
3. Bila PO diberikan: PO harus ada. Bila GR diberikan: GR harus ada dan, bila PO juga diberikan, milik PO itu (`400`).
4. Supplier PO yang dirujuk (langsung, atau lewat GR bila PO tidak diberikan) harus sama dengan supplier Tukar Faktur (`422`).
5. Bila GR diberikan, sistem mengambil kunci PO milik GR itu, membaca ulang status GR di dalam kunci, dan menolak bila bukan `RECEIVED` (`422`).
6. Tukar Faktur tersimpan `RECEIVED` dengan `EstimatedDueDate` = tanggal dokumen diterima + termin supplier (tidak berubah).
7. Tukar Faktur tanpa PO dan tanpa GR tetap sah (`FIN-DEC-051`) dan tidak punya pembanding supplier.

**Alur sesudah perbaikan, pembatalan GR:** sistem mengambil kunci PO lebih dulu, baru memeriksa apakah ada Tukar Faktur yang merujuk GR itu. Karena
pembuatan Tukar Faktur memakai kunci yang sama, salah satu dari keduanya selalu menunggu yang lain selesai.

**Contoh berangka:**

| Skenario | Hasil |
| --- | --- |
| PO milik Supplier A, Tukar Faktur atas nama Supplier B | Ditolak `422` "Supplier Tukar Faktur harus sama dengan supplier Purchase Order yang dirujuk." |
| Hanya GR yang dirujuk, PO milik GR itu milik Supplier A, Tukar Faktur atas nama Supplier B | Ditolak `422` |
| Supplier A (termin 30 hari), PO dan GR Supplier A, diterima 10 Oktober 2026 | Diterima; `EstimatedDueDate` 9 November 2026 |
| Tukar Faktur tanpa PO dan tanpa GR | Diterima |
| GR sudah dibatalkan, lalu dirujuk Tukar Faktur | Ditolak `422` "... sudah dibatalkan dan tidak dapat dijadikan dasar Tukar Faktur." |
| GR sudah dirujuk Tukar Faktur, lalu dibatalkan | Ditolak `422` "... sudah menjadi Tukar Faktur dan tidak dapat dibatalkan." (tidak berubah) |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `Areas/Corporate/FinanceManagement/Purchasing/Services/FinanceInvoiceExchangeService.cs`
- `Areas/Corporate/FinanceManagement/Purchasing/Services/FinanceGoodsReceiptService.cs`
- `Areas/Corporate/FinanceManagement/Purchasing/Services/FinancePurchasingInvoiceService.cs` (pola transaksi dan kunci)
- `Areas/Corporate/FinanceManagement/Purchasing/Controllers/FinanceInvoiceExchangesController.cs` (pemetaan exception, sudah menangani `422`)
- `Areas/Corporate/FinanceManagement/Purchasing/Models/FinPurchaseOrder.cs`, `FinGoodsReceipt.cs`, `FinInvoiceExchange.cs`
- `docs/module-blueprints/finance-management/contracts/validation-matrix.md` dan `state-transition-matrix.md` §B.2, §B.3
- `Tests/QuilvianSystemBackend.PharmacyTests/Infrastructure/TestDatabase.cs` (fungsi kunci tiruan untuk SQLite)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/Purchasing/Services/FinanceInvoiceExchangeService.cs` | (1) `CreateAsync` mengumpulkan supplier PO yang dirujuk (langsung atau lewat GR) dan menolak bila berbeda dari supplier Tukar Faktur. (2) Bila GR diberikan: transaksi `Serializable` dan kunci `FIN_PURCHASE_ORDER_{id}` (kunci yang sama dengan `FinanceGoodsReceiptService`), status GR dibaca ulang di dalam kunci. (3) Helper transaksi (`BeginTransactionAsync`, `AcquireLockAsync`, `CommitAsync`, `RollbackAsync`) ditambahkan mengikuti pola service lain; `using` untuk `IDbContextTransaction` dan `IsolationLevel` |
| `Areas/Corporate/FinanceManagement/Purchasing/Services/FinanceGoodsReceiptService.cs` | `CancelAsync`: pemeriksaan "GR sudah dirujuk Tukar Faktur" dipindah ke setelah transaksi dan kunci PO diambil, dengan komentar alasan |
| `Tests/QuilvianSystemBackend.PharmacyTests/FinanceInvoiceExchangeSupplierConsistencyTests.cs` | Baru: 6 test |
| `Tests/QuilvianSystemBackend.PharmacyTests/Infrastructure/FinanceApSeed.cs` | Baru, dibuat pada `BE-FIN-120` dan dipakai bersama |
| `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` | Bagian "AMENDMENT ROADMAP REVISI 22" (dipakai bersama `BE-FIN-120`) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tidak ada endpoint, DTO, atau route baru. `POST /invoice-exchanges` kini dapat menjawab `422` karena supplier berbeda atau GR sudah dibatalkan; controller sudah memetakan `PurchasingValidationException` ke `422` sehingga tidak diubah |
| Database | `NOT APPLICABLE` — nol entity, tabel, kolom, dan migration. Pembuatan Tukar Faktur yang merujuk GR kini berjalan dalam transaksi `Serializable` |
| Keamanan/Auth | `NOT APPLICABLE` — metadata akses tidak berubah |

---

## 4. Dokumentasi endpoint

#### Corporate / Finance Management / Purchasing

Grup sesuai `[Tags(...)]` controller Tukar Faktur dan Tanda Terima Barang.

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/api/v1/corporate/finance-management/purchasing/invoice-exchanges` | Mencatat Tukar Faktur; kini menolak supplier yang berbeda dari PO/GR dan GR yang sudah dibatalkan (`422`) | Tidak diubah |
| `POST` | `/api/v1/corporate/finance-management/purchasing/goods-receipts/{id}/cancel` | Membatalkan GR; pemeriksaan rujukan Tukar Faktur kini berjalan di dalam kunci PO | Tidak diubah |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Instruksi pemilik: build dijalankan mandiri oleh pemilik, bukan otomatis oleh agent |
| QBE preflight | `TOUCHED LEGACY`, nol entity/migration/endpoint baru; `QBE-VAL-001`, `QBE-TXN-001`, `QBE-SVC-001`, `QBE-LOG-001` terpenuhi | `PASS` | Penelusuran source; `AuditAsync("Create")` dan `AuditAsync("Cancel")` tidak berubah |
| Review diff dan scope | `FinanceInvoiceExchangeService.cs` +84/-7, `FinanceGoodsReceiptService.cs` +6/-3, satu berkas test baru, helper bersama, dan roadmap. Tidak ada perubahan dependency, konfigurasi, atau migration | `PASS` | `git diff --numstat` dan `git status --short` (bagian 7) |
| Pembacaan ulang `CreateAsync` setelah edit | Urutan: supplier aktif, nomor faktur, PO, GR, kecocokan GR-PO, supplier PO, kunci, status GR, simpan, commit; `catch` melakukan rollback dan `finally` membuang transaksi | `PASS` | `FinanceInvoiceExchangeService.cs` baris 72-198 |
| Kesamaan kunci antara pembuatan TF dan pembatalan GR | Keduanya memakai string `FIN_PURCHASE_ORDER_{PurchaseOrderId}` dengan `Guid` diformat bawaan | `PASS` | `FinanceInvoiceExchangeService.cs` baris 172 dan `FinanceGoodsReceiptService.cs` baris 91 dan 197 |
| Test otomatis (6 test) | Ditulis, tidak dijalankan | `NOT RUN` | Lihat baris status di bawah |
| Dua permintaan bersamaan (pembatalan GR vs pembuatan TF) | Tidak dibuktikan | `NOT RUN` | Basis data uji hanya meniru bentuk fungsi kunci PostgreSQL, bukan perilakunya (`TestDatabase.cs`); hanya dapat dibuktikan pada PostgreSQL |

`AUTOMATED TEST: NOT RUN — 6 test ditulis atas persetujuan pemilik pada rencana kerja 26 Oktober 2026 (lihat roadmap REV-22), tidak dijalankan karena build dan test dijalankan mandiri oleh pemilik.`

Uji manual: `NOT FEASIBLE` — server backend tidak dijalankan pada sesi ini.

**Tidak dijalankan:** `dotnet build`, `dotnet test`, uji manual Swagger, dan uji konkurensi pada PostgreSQL. Semuanya menunggu pemilik.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. TF atas nama Supplier B yang merujuk PO Supplier A ditolak `422` | Terpetakan ke source; belum dibuktikan build/test | Test `Create_RejectsSupplierDifferentFromReferencedPurchaseOrder` |
| 2. TF atas nama Supplier B yang hanya merujuk GR milik PO Supplier A ditolak `422` | Terpetakan ke source; belum dibuktikan build/test | Test `Create_RejectsSupplierDifferentFromPurchaseOrderOfReferencedGoodsReceipt` |
| 3. Supplier sama lewat PO dan GR diterima, `EstimatedDueDate` = tanggal terima + termin | Terpetakan ke source; belum dibuktikan build/test | Test `Create_AcceptsMatchingSupplier_ThroughPurchaseOrderAndGoodsReceipt` (10 Oktober 2026 + 30 hari = 9 November 2026) |
| 4. TF tanpa PO dan tanpa GR tetap diterima | Terpetakan ke source; belum dibuktikan build/test | Test `Create_StillAllowsInvoiceExchangeWithoutPurchaseOrderOrGoodsReceipt` |
| 5. TF yang merujuk GR `CANCELLED` ditolak `422` | Terpetakan ke source; belum dibuktikan build/test | Test `Create_RejectsCancelledGoodsReceipt`; pembacaan ulang status di dalam kunci |
| 6. Pembatalan GR yang sudah dirujuk TF tetap ditolak `422`, diperiksa setelah kunci PO | Terpetakan ke source; belum dibuktikan build/test | Test `CancelGoodsReceipt_IsRejectedOnceAnInvoiceExchangeReferencesIt`; `FinanceGoodsReceiptService.cs` setelah `AcquireLockAsync` |
| DoD: preflight selesai | Terpenuhi | Bagian Metadata dan bagian 5 |
| DoD: `dotnet build` berhasil | **Belum terpenuhi** | `NOT RUN` |
| DoD: laporan tracked ada | Terpenuhi | Berkas ini |

Task ini **belum** dapat ditandai ✅ sampai pemilik mengonfirmasi `dotnet build` dan test, sesuai `status-task-roadmap.md`.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | (1) Perilaku kunci dua permintaan bersamaan belum dibuktikan; pada PostgreSQL, isolasi `Serializable` menjaga konsistensi walau kunci tidak menolong, tetapi kegagalan serialisasi (`40001`) belum dipetakan menjadi `409` (sama dengan service Finance lain). (2) Pembatalan GR membaca dan mengubah GR sebelum kunci diambil; konflik ditangkap oleh `RowVersion` seperti sebelumnya |
| Masalah yang diketahui | Tiga aturan baru belum bernomor `FIN-VAL` di kontrak; dicatat pada roadmap `REV-22` bagian "Delta Kontrak". Tukar Faktur belum memeriksa status PO (misalnya PO `CANCELLED`); di luar cakupan dan dilaporkan saja |
| Risiko tersisa | Rendah untuk kebenaran logika. Tukar Faktur yang merujuk GR kini menunggu kunci PO sehingga dapat sedikit lebih lambat bila ada penerimaan barang bersamaan pada PO yang sama. Kebenaran runtime belum dibuktikan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat blok di bawah |
| Langkah berikutnya | Pemilik menjalankan `dotnet build` dan test `FinanceInvoiceExchangeSupplierConsistencyTests`, lalu `BE-FIN-121` dinaikkan ke ✅ pada roadmap. Slice B menunggu keputusan pemilik soal jatuh tempo, materai/ongkir, dan banyak GR per faktur |

```text
M Areas/Corporate/FinanceManagement/Purchasing/Services/FinanceGoodsReceiptService.cs
M Areas/Corporate/FinanceManagement/Purchasing/Services/FinanceInvoiceExchangeService.cs
?? Tests/QuilvianSystemBackend.PharmacyTests/FinanceInvoiceExchangeSupplierConsistencyTests.cs
?? Tests/QuilvianSystemBackend.PharmacyTests/Infrastructure/FinanceApSeed.cs
M docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md
?? docs/module-blueprints/finance-management/task/report/backend/BE-FIN-121.md
```

Berkas lain pada `git status --short` adalah milik task lain: Billing dan test Pharmacy (pekerjaan pengguna yang sudah ada sebelum task ini, tidak disentuh), serta berkas task `REV-22` lainnya (`BE-FIN-120` dan `BE-FIN-122`..`126`).
