# Laporan Perubahan Backend — `BE-FIN-054`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-054` |
| Judul | Daftar alokasi penerimaan yang dibalik — `GET /receipts/reversed-allocations` |
| Slice | `REV-13B` — `EPIC FIN-18` |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian "AMENDMENT ROADMAP REVISI 13" |
| Trace | `FIN-DEC-094`; `FIN-DES-073` |
| Contract version | `FIN-API-1.4` §D.2 — `approved` 1 Oktober 2026 (lihat `blueprint-manifest.md` `status_note_revision_13`) |
| Dependency | Tidak ada — berdiri sendiri pada grafik dependency `REV-13` |
| Klasifikasi | `LIGHT` — satu repository (skor 0); 3 berkas diubah (skor 0); nol model baru, murni query baca (skor 0); 1 endpoint baru, pola arketipe read-only sudah ada (skor 0); database — nol migration (skor 0); keamanan/auth — nol resource/action baru (skor 0); UI/workflow — permukaan baca murni (skor 0). Total 0 → `LIGHT` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/Collection/**` dan laporan ini |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | Belum di-commit — branch `Yasmina` |
| Tanggal | 1 Oktober 2026 |
| Status | ✅ **Selesai** 1 Oktober 2026 — source lengkap dengan satu delta kontrak yang dicatat (lihat §3.3, bukan blocker — `ReversalReason` dicabut karena memang tidak ada pada source). `dotnet build` **PASS**, dikonfirmasi pengguna |

---

## 1. Masalah yang diperbaiki

Pembalikan alokasi penerimaan (misalnya karena tender Billing dibatalkan, atau petugas salah
alokasi) sudah bisa dilakukan satu per satu lewat
`POST /receipts/{id}/allocations/{allocationId}/reverse`, tetapi hasilnya hanya terlihat di dalam
detail penerimaan asalnya. Tidak ada daftar yang menampilkan **seluruh** alokasi yang pernah
dibalik — layar "Receiveable AR Canceled" pada menu V1 (`FIN-DEC-094`) butuh permukaan ini.

---

## 2. Proses bisnis

**Pelaku:** Petugas AR Finance.

**Pemicu:** Petugas membuka layar "Receiveable AR Canceled" untuk meninjau alokasi yang pernah
dibatalkan.

**Langkah normal:**

1. Petugas membuka daftar, opsional menyaring periode (`StartDate`/`EndDate` atas tanggal
   pembalikan) atau penerimaan tertentu (`ReceiptId`).
2. Sistem menampilkan setiap baris pembalikan beserta penerimaan asalnya (nomor), piutang yang
   terdampak (bila ada), nominal, dan tanggal pembalikan.

**Aturan yang berlaku:** Endpoint ini **baca saja**. Pembalikan sendiri tetap satu-satunya lewat
`POST .../allocations/{allocationId}/reverse` yang sudah ada — task ini **tidak menambah atau
mengubah** satu pun aturan di jalur itu. Grain barisnya adalah **alokasi** (baris `IsReversal =
true`), bukan penerimaan — satu penerimaan dapat punya banyak baris alokasi yang dibalik.

**Jalur tidak normal:** Tidak ada — endpoint `GET` murni.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/02-backend-architecture.md` AMENDMENT REVISI 13 §J.5
- `docs/module-blueprints/finance-management/contracts/api-contract.md` §D.2
- `Areas/Corporate/FinanceManagement/Collection/Models/FinReceiptAllocation.cs` — model yang
  dibaca (tidak diubah). **Temuan:** model ini **tidak memiliki kolom alasan apa pun** untuk
  pembalikan — lihat §3.3
- `Areas/Corporate/FinanceManagement/Collection/Services/FinanceReceiptService.cs` —
  `ReverseAllocationAsync` dibaca penuh untuk memastikan field apa saja yang sungguh diisi saat
  pembalikan (`ReversalOfAllocationId`, `AllocatedAt`, **tanpa** parameter alasan); pola
  `GetRegisterAsync` dipakai ulang persis untuk method baru
- `Areas/Corporate/FinanceManagement/Collection/Controllers/FinanceReceiptsController.cs` —
  dikonfirmasi resource `FinanceReceipt` dan routing literal segment yang sudah aman berdampingan
  dengan `{id:guid}` (pola yang sama dengan `register`/`shift-reconciliation`, `BE-FIN-050`)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/.../Collection/Dtos/FinanceReceiptDtos.cs` | Dua DTO baru: `ReversedAllocationQuery` (saringan `StartDate`/`EndDate`/`ReceiptId`, paginasi) dan `ReversedAllocationRowResponse` |
| `Areas/.../Collection/Services/FinanceReceiptService.cs` | Satu method baru `GetReversedAllocationsAsync` — query `FinReceiptAllocations` disaring `IsReversal = true`, di-`Include(Receipt, Receivable)`, diurutkan, dipaginasi; **tidak menyentuh** `ReverseAllocationAsync`/`AllocateAsync` sama sekali |
| `Areas/.../Collection/Controllers/FinanceReceiptsController.cs` | Satu endpoint baru `GET reversed-allocations`, memakai `[AccessPermission("FinanceReceipt", "Read")]` yang sudah terdaftar; doc comment kelas diperbarui |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Satu endpoint baru, aditif murni. **Satu delta dicatat terhadap `FIN-API-1.3` §D.2**: kontrak menyebut field `ReversalReason` pada `ReversedAllocationRowResponse`, tetapi `FinReceiptAllocation` **tidak punya kolom alasan apa pun** — dikonfirmasi langsung dari model dan dari `ReverseAllocationAsync` yang tidak menerima parameter alasan sama sekali. Field itu **dicabut** dari response, bukan dikarang nilainya. Ini bukan keputusan bisnis yang perlu ditanyakan — murni ketidaksesuaian kontrak-vs-source yang MUST dilaporkan, bukan dilewatkan |
| Database | **Nol.** Murni query baca. Nol migration |
| Keamanan/Auth | **Nol** resource dan **nol** action baru. Memakai `FinanceReceipt : Read` yang sudah terdaftar |

---

## 4. Dokumentasi endpoint

#### Corporate / Finance Management / Receipt

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/reversed-allocations` | Daftar baris alokasi yang dibalik — layar "Receiveable AR Canceled" | `FinanceReceipt : Read` |

Mengembalikan `ApiResponse<PagedResult<ReversedAllocationRowResponse>>`: `AllocationId`,
`ReceiptId`, `ReceiptNumber`, `ReceivableId`, `ReceivableNumber`, `Amount`,
`ReversalOfAllocationId`, `ReversedAt`. **`ReversalReason` tidak ada** — lihat §3.3.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Berhasil tanpa error | `PASS` | Dikonfirmasi pengguna, 1 Oktober 2026 |
| Review diff/scope | 3 berkas, persis sesuai desain, nol berkas tidak terkait | `PASS` | `git status --short` Bagian 7 |
| Review kontrak API terhadap implementasi | Satu endpoint; satu delta kontrak dicatat (`ReversalReason` dicabut) | `PASS` | Perbandingan langsung kode vs kontrak vs model |
| Review proses bisnis — nol perubahan pada jalur pembalikan | `ReverseAllocationAsync`/`AllocateAsync` nol baris berubah | `PASS` | `git diff` method-method itu kosong |
| QBE preflight | Area `Corporate/Finance`, Module `FinanceManagement`, Submodule `Collection`, prefix `Fin` `ACTIVE` | `PASS` | `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |

Uji manual: `NOT FEASIBLE` — server tidak dijalankan pada task ini.

**AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).**

**Tidak dijalankan:** uji manual/runtime (server tidak dijalankan).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Grain baris adalah alokasi, bukan penerimaan | Terpenuhi | Query menyaring `FinReceiptAllocations.Where(IsReversal)`, bukan `FinReceipts` |
| Tersaring periode dan penerimaan | Terpenuhi | `ReversedAllocationQuery` |
| Paginasi benar | Terpenuhi | Pola `Skip`/`Take`/`TotalPage` identik dengan `GetRegisterAsync` |
| Build PASS | Terpenuhi | `dotnet build` PASS, dikonfirmasi pengguna 1 Oktober 2026 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Delta kontrak:** field `ReversalReason` pada `FIN-API-1.3` §D.2 **dicabut** dari implementasi — `FinReceiptAllocation` tidak punya kolom alasan, dan menambahkannya di luar cakupan `LIGHT` task ini (butuh migration + keputusan apakah alasan wajib diisi). Bila alasan pembalikan memang dibutuhkan produk, ini **MUST** diangkat sebagai task tersendiri, bukan ditambahkan diam-diam di sini |
| Masalah yang diketahui | Tidak ada temuan lain |
| Risiko tersisa | **Rendah.** Build sudah dibuktikan PASS. Perubahan aditif murni mengikuti pola existing persis |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `git status --short` mencakup tepat tiga berkas task ini |
| Langkah berikutnya | Tidak ada — task ini selesai |
