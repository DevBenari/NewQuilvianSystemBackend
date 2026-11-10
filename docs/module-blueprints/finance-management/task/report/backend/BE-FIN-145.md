# Laporan Perubahan Backend — `BE-FIN-145`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-145` |
| Judul | Status dan kemajuan PO BLM dihitung dari jumlah sah ternormalisasi per item, tidak pernah dari tebakan konversi |
| Slice | `MVP-19` — Penampung Data PO/GR dari BLM (`EPIC FIN-26`) |
| Roadmap | `roadmap/01-backend-roadmap.md`, bagian "AMENDMENT ROADMAP REV-24" |
| Trace | `FR-FIN-278` (sisi penghitung), `FR-FIN-282`; `FIN-DEC-268`, `276`, `277`; `FIN-DES-142`; `FIN-STATE-1.12` W.4; `FIN-VAL-311`, `316` |
| Contract version | `02-backend-architecture.md` W.11; `contracts/state-transition-matrix.md` W.4; `contracts/validation-matrix.md` W (Revisi 23, approved 2026-11-09) |
| Dependency | `BE-FIN-142` 🟡 (model sudah ada di working tree) |
| Klasifikasi | `LIGHT` — skor 3: repository 0, berkas diperiksa 1 (±12), berkas diubah 0 (1 berkas baru), logika bisnis 2 (aturan status per item), kontrak API 0, database 0, keamanan 0, UI 0 |
| Task mode | `BACKEND MODE` |
| Target tulis | `NewQuilvianSystemBackend`, branch `Yasmina`, satu berkas di `Areas/Corporate/FinanceManagement/Purchasing/Services/` |
| Model | Claude Sonnet 5.5 |
| Commit backend saat dikerjakan | `98a9140c` (working tree memuat pekerjaan `BE-FIN-142`..`144` yang belum di-commit; tidak disentuh) |
| Tanggal | 10 November 2026 |
| Status | 🟡 **Source selesai; `dotnet build` berhasil menurut laporan pemilik (10 November 2026, keluaran build Release kemudian dilampirkan pemilik (10 November 2026): `0 Error(s)`, `12 Warning(s)` (seluruhnya peringatan paket NU1902/NU1903 `SixLabors.ImageSharp` 3.1.11; tidak ada di folder Purchasing), `Build succeeded`).** Tetap 🟡 karena roadmap membatasi task ini paling jauh 🟡 sampai migration diterapkan dan W.T5–W.T12 dijalankan pada data nyata; logika ditelusuri lewat port Python (24 skenario sesuai), bukan eksekusi kode C# |

## Backend Governance Preflight

| Butir | Hasil |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `FinanceManagement` / `Purchasing` (`Services/`) |
| Pemilik dan prefix (registry) | `Fin`, "Corporate / Finance — FinanceManagement / Purchasing / Pembelian", `ACTIVE` |
| Keberlakuan | `NEW CODE`. Tidak ada entity, tabel, migration, atau endpoint baru |
| QBE yang berlaku | `QBE-MOD-001` (capability di pemiliknya), `QBE-NAM-002` (nama mengikuti pola `Purchasing…` pada folder yang sama). Tidak berlaku: `QBE-CODE-*` (tidak membuat nomor bisnis), `QBE-DB-*` |
| Pola yang ditiru | `PurchasingInvoiceAllocationPlanner`: kelas statis berisi fungsi murni, record sebagai masukan/keluaran, tanpa akses `DbContext`. Bukan arsitektur generic repository dan bukan service ber-scope, sehingga **tidak ada perubahan `Program.cs`** |
| Branch | `Yasmina` |
| Selisih governance | Sama dengan task sebelumnya (salinan registry di suite lebih ringkas; salinan repository dipakai) |
| Working tree sebelum task | 25 entri berubah milik `BE-FIN-142`..`144` dan dokumen; tidak disentuh |

---

## 1. Masalah yang Diperbaiki

Rumus status PO yang ada (`FinanceGoodsReceiptService.RecomputePurchaseOrderStatus`) menjumlah `Quantity` seluruh item lalu membandingkannya dengan jumlah `ReceivedQuantity` seluruh item. Itu cukup untuk GR buatan Quilvian, tetapi salah untuk PO BLM:

1. **Satuan berbeda.** PO 1 BOX berisi 12 PCS, GR pertama 6 PCS. Dibandingkan mentah, "6 dari 1" terbaca sudah lebih dari penuh.
2. **Total lintas item.** PO dengan produk A (12 PCS) dan produk B (24 BOX) menjumlah satuan yang tidak sepadan. Item A yang penuh dapat menutupi item B yang belum diterima sama sekali.
3. **Kondisi barang.** Barang rusak atau menunggu pemeriksaan terhitung sebagai diterima padahal tidak sah (`FIN-DEC-268`).
4. **Konversi tidak pasti.** Bila isi per satuan hilang, tebakan "1" akan menghasilkan status yang tampak benar tetapi salah (`FIN-DEC-276` melarangnya).

Task ini menyediakan **penghitung bersama** yang menjawab keempatnya. Ia belum dipanggil siapa pun: pemanggilan oleh `FinanceGoodsReceiptService` dan jejak dokumen dikerjakan `BE-FIN-146` dan `BE-FIN-147`.

---

## 2. Proses Bisnis

Tujuan: menentukan, dari data yang sudah tersimpan, berapa banyak yang **sah diterima** per item PO dan apa status PO-nya.

| Langkah | Aturan | Contoh (PO 1 BOX = 12 PCS) |
| ---: | --- | --- |
| 1 | Hanya PO bertanda `BLM` yang dihitung; PO lain mengembalikan `null` sehingga pemanggil memakai rumus lama | PO lama "PO-2026-0001" → tidak dihitung di sini |
| 2 | GR berstatus `CANCELLED` serta baris/GR yang dihapus dibuang | GR-2 dibatalkan → keluar dari semua jumlah |
| 3 | Baris GR dicocokkan ke item PO **hanya lewat ID item**. Baris yang tidak menunjuk item PO mana pun membuat PO "tidak pasti" | Tidak ada pencocokan lewat nama produk |
| 4 | Sebuah baris **pasti** bila isi per satuan > 0, satuan dasarnya sama dengan satuan dasar item PO, jumlah fisik dan sah terisi (≥ 0, sah ≤ fisik), kondisi dikenal, dan hanya kondisi `GOOD` yang boleh punya jumlah sah | 6 PCS `GOOD`, isi per satuan 1, satuan dasar `PCS` → pasti |
| 5 | Baris tidak pasti **tidak dijumlahkan** dan menandai item itu `IsConversionResolved = false` | Satuan dasar baris `ML` ≠ `PCS` → tidak dijumlah, item ditandai |
| 6 | Per item: fisik, sah, menunggu periksa, rusak, sisa = maks(0, dipesan − sah), semuanya satuan dasar tanpa pembulatan | GR 6 `GOOD` → sah 6, sisa 6 |
| 7 | Item lengkap bila dipesan sah, semua barisnya pasti, dan sah ≥ dipesan | 12 dari 12 → lengkap |
| 8 | Status PO: `FULLY_RECEIVED` bila **setiap** item lengkap dan seluruhnya pasti; `PARTIALLY_RECEIVED` bila ada jumlah sah tetapi belum lengkap; selain itu `APPROVED` | Lihat tabel di bawah |

**Penelusuran skenario matriks uji (W.T5–W.T12).** Dikerjakan lewat port Python atas logika yang sama, bukan eksekusi C#.

| Skenario | Masukan | Hasil yang dihitung | Sesuai tulisan |
| --- | --- | --- | --- |
| PO baru | tanpa GR | sisa 12, `APPROVED` | Ya |
| W.T5 | GR-1: 6 PCS `GOOD` | sah 6, sisa 6, `PARTIALLY_RECEIVED`; tampilan `ReceivedQuantity` 6 ÷ 12 = 0,50 | Ya |
| W.T6 | + GR-2: 6 PCS `GOOD` | sah 12, sisa 0, `FULLY_RECEIVED` | Ya |
| W.T7 | GR-2 = 4 `GOOD` + 2 `PENDING_INSPECTION` | sah 10, menunggu 2, `PARTIALLY_RECEIVED` | Ya |
| W.T8 | GR-2 = 4 `GOOD` + 2 `DAMAGED` | sah 10, rusak 2, `PARTIALLY_RECEIVED` | Ya |
| W.T9 | GR dalam BOX: 1 BOX `GOOD`, isi per satuan 12 | normalisasi 1 × 12 = 12, sah 12, `FULLY_RECEIVED` | Ya |
| W.T10 | baris dengan satuan dasar `ML` | tidak dijumlah, `IsConversionResolved = false`, bukan `FULLY_RECEIVED` | Ya |
| W.T11 | GR-2 dibatalkan | kembali sah 6, `PARTIALLY_RECEIVED` | Ya |
| W.T12 | dua item: A penuh, B nol | `PARTIALLY_RECEIVED` (bukan penuh) | Ya |

**Kasus tepi yang ikut ditelusuri (semua sesuai):** isi per satuan kosong atau 0 → tidak pasti; satuan `" pcs "` disamakan dengan `PCS`; kelebihan terima (15 dari 12) → `FULLY_RECEIVED` dengan sisa 0 dan tidak ditolak; baris tanpa item PO → tidak pasti; baris terhapus diabaikan; item PO tanpa jumlah dipesan → dipesan dan sisa `null`; `DAMAGED` dengan jumlah sah > 0 → tidak pasti; kondisi tak dikenal → tidak pasti; sah > fisik → tidak pasti; PO tanpa item → `APPROVED`; 1 PCS dari BOX isi 12 → sisa 11 dan tampilan 0,08 BOX; normalisasi tanpa isi per satuan → `null` (bukan 1).

**Jalur tidak normal.** Konversi tidak pasti tidak pernah menghasilkan `FULLY_RECEIVED`, dan tidak ada angka yang ditebak. Penulisan konflik `CONVERSION_UNRESOLVED` bukan tugas penghitung; itu tugas pengolah kiriman.

---

## 3. Perubahan yang Dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`; registry dan `BACKEND_ENGINEERING_CONTRACT.md`; `02-backend-architecture.md` W.11; `contracts/state-transition-matrix.md` W.4; `contracts/validation-matrix.md` `FIN-VAL-311`, `316`; `testing/acceptance-test-matrix.md` W.T5–W.T12; `PurchasingInvoiceAllocationPlanner.cs` (pola); model `FinPurchaseOrder`, `FinPurchaseOrderItem`, `FinGoodsReceipt`, `FinGoodsReceiptItem` (hasil `BE-FIN-142`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/Purchasing/Services/PurchasingReceiptQuantityCalculator.cs` | **Baru.** Empat record (`ReceiptOrderItemInput`, `ReceiptLineInput`, `ReceiptItemProgress`, `ReceiptProgress`) dan kelas statis `PurchasingReceiptQuantityCalculator` dengan `CalculateForPurchaseOrder` (dari entity; `null` bila bukan BLM), `Calculate` (fungsi murni inti), `NormalizeToBase` (kuantitas asli × isi per satuan; `null` bila isi per satuan tidak sah), dan `ToPurchaseOrderUnit` (pembagian + pembulatan 2 desimal khusus tampilan dan kolom lama `ReceivedQuantity`) |

Tidak ada perubahan pada `FinanceGoodsReceiptService`, `FinancePurchasingDocumentTrailService`, DTO, controller, `Program.cs`, model, konfigurasi, atau migration.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — belum dipanggil dari endpoint mana pun |
| Database | `NOT APPLICABLE` — tidak membaca atau menulis; tidak ada transaksi |
| Keamanan/Auth | `NOT APPLICABLE` |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak menyentuh endpoint.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet restore` / `dotnet build` | Berhasil | `PASS` (laporan pemilik) | Pemilik, 10 November 2026; keluaran perintah dilampirkan kemudian oleh pemilik (build Release: `0 Error(s)`, `12 Warning(s)` (seluruhnya peringatan paket NU1902/NU1903 `SixLabors.ImageSharp` 3.1.11; tidak ada di folder Purchasing), `Build succeeded`) |
| QBE preflight / conformance | Lulus | `PASS` | Pola kelas statis di folder `Services/` yang sama; tanpa entity, `DbContext`, atau nomor bisnis |
| Review scope | Satu berkas baru; tidak menyentuh `FinanceGoodsReceiptService` (milik `BE-FIN-146`) | `PASS` | `git status --short` |
| Keseimbangan kurung | Seimbang | `PASS` | Skrip statis (bukan kompilasi) |
| Nama tipe unik di assembly (5 tipe baru) | Masing-masing satu definisi | `PASS` | `grep` |
| Tidak ada pemanggil lain / tidak ada registrasi DI | Tidak ada | `PASS` | `grep` pada `Areas`, `Repositories`, `Program.cs` |
| Penelusuran W.T5–W.T12 + kasus tepi | 24 dari 24 sesuai | `PASS` (penelusuran) | Port Python `port_calc.py` di scratchpad sesi (di luar repository). **Bukan uji atas kode C#** |
| Kompilasi C# | Berhasil | `PASS` (laporan pemilik) | Pemilik, 10 November 2026; keluaran perintah dilampirkan kemudian oleh pemilik (build Release: `0 Error(s)`, `12 Warning(s)` (seluruhnya peringatan paket NU1902/NU1903 `SixLabors.ImageSharp` 3.1.11; tidak ada di folder Purchasing), `Build succeeded`) |
| Perilaku runtime (eksekusi kode C# dan data nyata) | Belum. Migration sudah diterapkan (laporan pemilik), tetapi W.T5–W.T12 belum dijalankan dengan baris BLM | `NOT RUN` | Menunggu pengujian pemilik |

Uji manual: `NOT APPLICABLE` (belum ada alur yang memanggil penghitung).

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

**Tidak dijalankan oleh agen (build dijalankan pemilik, lihat laporan pemilik di tabel):** semua perintah `dotnet`; eksekusi C# apa pun.

---

## 6. Acceptance Criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Per item: dipesan, fisik, sah, menunggu periksa, rusak, sisa (satuan dasar) dan `IsConversionResolved` | Terpenuhi | Record `ReceiptItemProgress`; `CalculateItem` |
| 2. Mengabaikan GR `CANCELLED` dan baris/GR terhapus | Terpenuhi | Penyaringan `activeLines` di `Calculate` |
| 3. `FULLY_RECEIVED` hanya bila setiap item sah ≥ dipesan dan semua konversi pasti | Terpenuhi | `Calculate`: `fullyReceived`; W.T10, W.T12 dan kasus baris tanpa item PO |
| 4. Tidak ada nilai bawaan 1 | Terpenuhi | `NormalizeToBase` mengembalikan `null`; `IsLineResolved` menolak isi per satuan kosong atau ≤ 0 |
| 5. Tidak mencocokkan lewat nama produk | Terpenuhi | Satu-satunya kunci pencocokan: `PurchaseOrderItemId`; tipe masukan tidak membawa nama produk |
| 6. Contoh W.T5–W.T12 menghasilkan angka dan status yang tertulis | Terpenuhi menurut penelusuran; **belum dieksekusi pada kode C#** | Bagian 2 |
| DoD: laporan tracked | Terpenuhi | Berkas ini |
| DoD: `dotnet build` | Terpenuhi (laporan pemilik) | Pemilik, 10 November 2026; keluaran perintah dilampirkan kemudian oleh pemilik (build Release: `0 Error(s)`, `12 Warning(s)` (seluruhnya peringatan paket NU1902/NU1903 `SixLabors.ImageSharp` 3.1.11; tidak ada di folder Purchasing), `Build succeeded`) |

Status akhir **🟡**: kriteria 1–6 terpenuhi menurut review dan penelusuran, dan kompilasi lulus (laporan pemilik). Yang menahan ✅ adalah batas yang tertulis di roadmap ("paling jauh 🟡 sampai migration diterapkan"; migration kini sudah diterapkan menurut pemilik, tetapi buktinya belum): W.T5–W.T12 belum dijalankan pada data nyata di database uji, dan penelusuran memakai port Python, bukan kode C#.

---

## 7. Catatan Penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kelas ini belum dipanggil siapa pun; PO BLM belum ada. Dampak perilaku baru baru muncul setelah `BE-FIN-146` memanggilnya dan integrasi mengisi data BLM |
| Keputusan implementasi | (1) Kelas statis berisi fungsi murni (bukan service ber-scope): tidak perlu `Program.cs`, mudah ditinjau. (2) Baris yang tidak pasti **tidak dijumlahkan** sama sekali, bukan dijumlahkan sebagian, supaya tidak ada angka setengah benar. (3) Baris GR yang tidak menunjuk item PO dihitung sebagai ketidakpastian dan dilaporkan lewat `UnmatchedLineCount`. (4) Konsistensi `BaseQuantity` = `SourceQuantity` × isi per satuan **tidak** diperiksa ulang: kontrak W.11 hanya menuntut isi per satuan > 0 dan satuan dasar sama; pemeriksaan itu tugas pengolah kiriman saat menerapkan baris |
| Masalah yang diketahui | (1) Penelusuran memakai port Python, bukan eksekusi C#; kesalahan sintaks atau perilaku pola `is not > 0m` pada `decimal?` baru terlihat saat build. (2) Satuan dibandingkan sebagai kode setelah dipangkas dan diseragamkan huruf besar; bila BLM mengirim nama satuan alih-alih kode, dua penulisan berbeda dianggap tidak sama dan menjadi `CONVERSION_UNRESOLVED` (sudah dicatat sebagai syarat kontrak di `FIN-OQ-141`) |
| Risiko tersisa | Kompilasi lulus (laporan pemilik), tetapi perilaku logika C# belum dieksekusi; kemungkinan selisih antara port Python dan kode C# hanya hilang bila W.T5–W.T12 dijalankan. Rumus lama untuk PO lama tidak diubah (`FIN-DEC-277`), sehingga bug lamanya tetap ada untuk PO lama; ini disengaja |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `??` satu berkas source baru + laporan ini; ditambah pekerjaan `BE-FIN-142`..`144` yang belum di-commit. Tidak ada berkas staged |
| Langkah berikutnya | Setelah migration diterapkan ke database uji: jalankan W.T5–W.T12 (baris BLM disisipkan lewat SQL), lalu task ini dapat ditandai ✅ |
