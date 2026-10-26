# Laporan Perubahan Backend — `BE-FIN-127`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-127` |
| Judul | Petugas Finance dapat menyaring daftar invoice menurut rentang jatuh tempo aktual utang dan melihat estimasi dari Tukar Faktur secara terpisah |
| Slice | `REV-22` — Finance AP mengikuti BLM, `Slice B` lanjutan |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md`, bagian "AMENDMENT ROADMAP REVISI 22" |
| Trace | `AUD-AP-04` (dokumen `Integrasi_AP_Quilvian_V2_BLM_Konsolidasi_dan_Gap.md`, commit audit `cfe22fe8`); `AP-UAT-21` |
| Contract version | `FIN-API-1.1` §B.4 (locked): "disaring supplier/status/jatuh tempo" **dipenuhi**. Response daftar bertambah empat field aditif; dicatat sebagai delta pada roadmap `REV-22` |
| Dependency | `BE-FIN-122` (daftar invoice) dan `BE-FIN-124` (penetapan `DueDate` dan `DueDateBasis` utang) |
| Klasifikasi | `MEDIUM` — skor 4: repository 0; berkas diperiksa 1; berkas diubah 0 (3 source); logika bisnis 1; kontrak API 1 (aditif); database 1 (query pada tabel existing); keamanan/auth 0; UI/workflow 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` branch `Yasmina`: `Areas/Corporate/FinanceManagement/Purchasing/`, `docs/module-blueprints/finance-management/` |
| Model | Claude Sonnet 5.5 |
| Commit backend saat dikerjakan | `cfe22fe8` (branch `Yasmina`), working tree bersih pada jalur Finance sebelum task ini |
| Tanggal | 2026-10-26 |
| Status | 🟡 **SEBAGIAN** — seluruh kriteria terpetakan ke source; `dotnet build` **NOT RUN**, kueri belum dijalankan terhadap PostgreSQL |

---

## 1. Masalah yang diperbaiki

Kontrak daftar Purchasing Invoice menyebut penyaringan menurut supplier, status, **dan jatuh tempo**. `BE-FIN-122` menyediakan dua yang pertama dan menahan yang ketiga karena
Purchasing Invoice tidak menyimpan tanggal jatuh tempo dan basis tanggalnya saat itu belum ditetapkan. Setelah `BE-FIN-124` menetapkan `DueDate` dan `DueDateBasis` pada utang yang
dibentuk invoice, dokumen konsolidasi audit ulang (`AUD-AP-04`) menunjukkan tidak ada tabel tanggal baru yang diperlukan: sumber tanggal aktual sudah ada pada utang.

Akibat sebelum task ini: petugas yang ingin tahu "invoice mana yang jatuh tempo bulan November?" harus membuka utang satu per satu atau memakai laporan lain.

---

## 2. Proses bisnis

**Pelaku:** petugas AP, penyetuju Finance, reviewer.

**Alur:**

1. Pengguna membuka daftar invoice dan, bila perlu, mengisi `dueDateFrom` dan/atau `dueDateTo` (keduanya inklusif dan masing-masing boleh kosong).
2. Sistem menolak `dueDateFrom` yang melebihi `dueDateTo` (`400`).
3. Tanpa filter jatuh tempo: seluruh invoice tampil seperti sebelumnya, kini dengan kolom jatuh tempo tambahan.
4. Dengan filter jatuh tempo: hanya invoice yang **sudah membentuk utang tidak dibatalkan** dengan `DueDate` pada rentang itu.
5. Tiap baris menampilkan: `actualDueDate`, `dueDateBasis`, dan `payableStatus` dari utang yang dibentuk invoice, serta `estimatedDueDate` dari Tukar Faktur sumber.

**Aturan:**

| Hal | Perlakuan |
| --- | --- |
| Invoice belum membentuk utang (`DRAFT`, `PENDING_APPROVAL`, `REJECTED`, `CANCELLED`) | `actualDueDate` kosong; tidak ikut tersaring begitu filter jatuh tempo diisi |
| Utang berstatus `CANCELLED` | Tidak dihitung pada filter; datanya tetap tampil pada baris dengan `payableStatus` `CANCELLED` |
| Estimasi Tukar Faktur | Hanya ditampilkan; tidak pernah dipakai menyaring dan tidak menggantikan tanggal aktual |
| Satu invoice dengan data tidak wajar punya lebih dari satu utang | Utang yang tidak dibatalkan didahulukan; baris invoice tetap satu |
| Penyaringan | Memakai `EXISTS`, bukan `JOIN`, sehingga `totalData` dan paging tidak terganda |

**Contoh berangka:** utang dari invoice `PI-001` jatuh tempo 9 November 2026 (hasil `BE-FIN-124`: GR diterima 10 Oktober 2026 + termin 30 hari), estimasi Tukar Faktur 4 November 2026.

| Filter | `PI-001` |
| --- | --- |
| `dueDateFrom=2026-11-01&dueDateTo=2026-11-30` | Muncul; `actualDueDate` 9 November, `estimatedDueDate` 4 November |
| `dueDateFrom=2026-10-01&dueDateTo=2026-10-30` | Tidak muncul |
| `dueDateFrom=2026-11-10` | Tidak muncul |
| `dueDateTo=2026-11-09` | Muncul (batas inklusif) |
| `dueDateFrom=2026-11-30&dueDateTo=2026-11-01` | Ditolak `400` |
| Tanpa filter jatuh tempo | Muncul bersama seluruh invoice lain, termasuk draft dengan `actualDueDate` kosong |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `Purchasing/Services/FinancePurchasingInvoiceService.cs` (`GetPagedAsync` hasil `BE-FIN-122`, `ApproveAsync`)
- `Purchasing/Controllers/FinancePurchasingInvoicesController.cs`, `Purchasing/Dtos/FinancePurchasingInvoiceDtos.cs`
- `Payable/Models/FinSupplierPayable.cs` (`SourcePurchasingInvoiceId`, `DueDate`, `DueDateBasis`, `Status`, `FinSupplierPayableStatuses`)
- `Purchasing/Models/FinInvoiceExchange.cs` (`EstimatedDueDate`)
- `Responses/PagedResult.cs`
- Pemanggil `FinancePurchasingInvoiceService.GetPagedAsync` pada seluruh `Areas/` (hanya controller invoice)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Purchasing/Dtos/FinancePurchasingInvoiceDtos.cs` | `PurchasingInvoiceQuery` menerima `DueDateFrom`/`DueDateTo`; kelas baru `PurchasingInvoiceListItemResponse` (turunan `PurchasingInvoiceResponse`) dengan `EstimatedDueDate`, `ActualDueDate`, `DueDateBasis`, `PayableStatus` |
| `Purchasing/Services/FinancePurchasingInvoiceService.cs` | `GetPagedAsync` mengembalikan `PagedResult<PurchasingInvoiceListRow>`; validasi rentang; penyaringan `EXISTS`; dua query kecil untuk estimasi dan utang pada halaman itu; urutan stabil (`CreateDateTime` menurun lalu `Id`); `using` untuk `Payable.Models`; record `PurchasingInvoiceListRow` |
| `Purchasing/Controllers/FinancePurchasingInvoicesController.cs` | `GetList` memetakan baris ke `PurchasingInvoiceListItemResponse` lewat `MapListItem` dan menangani `400`; komentar header diperbarui |
| `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` | `BE-FIN-127` pada amendment `REV-22`; teks "filter jatuh tempo ditunda" diperbarui |
| `.../task/report/backend/BE-FIN-122.md` | Dua kalimat yang menyatakan filter jatuh tempo masih ditunda diberi keterangan bahwa itu dikerjakan sebagai `BE-FIN-127` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `GET /purchasing-invoices` menerima dua query opsional dan mengembalikan empat field tambahan per baris (turunan dari respons lama, jadi konsumen lama tidak rusak). Kini dapat menjawab `400` untuk rentang tidak valid |
| Database | `NOT APPLICABLE` — nol migration. Hanya `SELECT` atas `FinSupplierPayable`, `FinInvoiceExchange`, dan `FinPurchasingInvoice`. Indeks `IX_FinSupplierPayable_SourcePurchasingInvoiceId` sudah ada dan melayani `EXISTS` |
| Keamanan/Auth | `NOT APPLICABLE` — hak akses `FinancePurchasingInvoice : Read` tidak berubah |

---

## 4. Dokumentasi endpoint

#### Corporate / Finance Management / Purchasing / Purchasing Invoice

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/finance-management/purchasing/purchasing-invoices` | Daftar invoice berpaging; query `supplierId`, `status`, `dueDateFrom`, `dueDateTo`, `pageNumber`, `pageSize`; respons memuat `estimatedDueDate`, `actualDueDate`, `dueDateBasis`, `payableStatus` | `FinancePurchasingInvoice : Read` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Instruksi pemilik: build dijalankan mandiri oleh pemilik |
| QBE preflight | `TOUCHED LEGACY`; `QBE-VAL-001`, `QBE-API-001`, `QBE-DTO-001`, `QBE-PAGE-001`, `QBE-SVC-001` terpenuhi | `PASS` | Penelusuran source |
| Pembacaan ulang setelah edit | Ditemukan `using ...Payable.Models` belum ada untuk `FinSupplierPayableStatuses`; sudah ditambahkan sebelum laporan ini ditulis | `PASS` | `FinancePurchasingInvoiceService.cs` baris 5 |
| Tidak ada pemanggil lain atas `GetPagedAsync` invoice | Hanya controller invoice | `PASS` | Pencarian pada `Areas/` |
| Penelusuran contoh berangka | Tabel bagian 2, termasuk batas inklusif dan rentang terbalik | `PASS` | Penelusuran `GetPagedAsync` |
| Tipe `PagedResult<T>.Items` | `List<T>`; `Select(MapListItem).ToList()` cocok | `PASS` | `Responses/PagedResult.cs` |
| Terjemahan EF atas `EXISTS` dengan `IQueryable` tertangkap, dan atas `ToDictionaryAsync` | Tidak dibuktikan | `NOT RUN` | Hanya terbukti saat build dan eksekusi terhadap PostgreSQL |
| Menjalankan endpoint terhadap database | Tidak dijalankan | `NOT RUN` | Server backend tidak dijalankan |

`AUTOMATED TEST: NOT APPLICABLE — task ini tidak disertai test otomatis; persetujuan pemilik atas test hanya mencakup BE-FIN-120 dan BE-FIN-121 (rules/backend/TEST_POLICY.md).`

Uji manual: `NOT FEASIBLE` — server backend tidak dijalankan.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Invoice `APPROVED` berutang jatuh tempo 9 November 2026 muncul pada rentang November, tidak pada rentang Oktober | Terpetakan ke source; belum dibuktikan build | Filter `DueDate >= from`/`<= to` pada `EXISTS` |
| 2. Invoice `DRAFT` tidak muncul pada filter jatuh tempo, tetap muncul tanpa filter dengan `actualDueDate` kosong | Terpetakan ke source; belum dibuktikan build | `EXISTS` hanya aktif bila filter diisi; baris tanpa utang menghasilkan `null` |
| 3. Utang `CANCELLED` tidak dihitung pada filter | Terpetakan ke source; belum dibuktikan build | `p.Status != FinSupplierPayableStatuses.Cancelled` |
| 4. `estimatedDueDate` terpisah dari `actualDueDate` | Terpetakan ke source; belum dibuktikan build | Dua sumber berbeda pada `PurchasingInvoiceListRow` |
| 5. Satu invoice tampil satu kali; `totalData`/paging tidak berubah akibat filter | Terpetakan ke source; belum dibuktikan build | `EXISTS` sebelum `CountAsync`/`Skip`/`Take`; pemilihan satu utang per invoice |
| 6. `dueDateFrom` > `dueDateTo` ditolak `400` | Terpetakan ke source; belum dibuktikan build | Pemeriksaan awal `GetPagedAsync`, `Failure` pada controller |
| 7. Hanya salah satu batas diisi diterima | Terpetakan ke source; belum dibuktikan build | Dua cabang `if` yang independen |
| DoD: preflight selesai | Terpenuhi | Metadata dan bagian 5 |
| DoD: `dotnet build` berhasil | **Belum terpenuhi** | `NOT RUN` |
| DoD: laporan tracked ada | Terpenuhi | Berkas ini |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Filter menyaring berdasarkan jatuh tempo **utang**. Utang lama dan saldo awal hasil migrasi ikut tersaring memakai `DueDate`-nya meski `dueDateBasis` kosong. Utang manual (bukan dari invoice) tidak punya invoice dan tidak muncul di daftar ini |
| Masalah yang diketahui | Tidak ada filter berdasarkan estimasi Tukar Faktur; bila draft perlu disaring menurut estimasi, itu parameter terpisah yang belum diminta. Pencarian teks dan pengurutan pilihan pengguna belum ada (sama dengan daftar Finance lain) |
| Risiko tersisa | Kebenaran terjemahan SQL belum dibuktikan. Bila build atau eksekusi menolak pola `IQueryable` tertangkap, penggantinya adalah subquery inline pada lambda yang sama tanpa mengubah perilaku |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat `git status --short`; berkas task ini tercantum pada bagian 3.2 |
| Langkah berikutnya | Pemilik menjalankan `dotnet build`; coba satu panggilan Swagger dengan rentang yang berisi utang ber-GR; lalu `BE-FIN-127` dinaikkan ke ✅ |
