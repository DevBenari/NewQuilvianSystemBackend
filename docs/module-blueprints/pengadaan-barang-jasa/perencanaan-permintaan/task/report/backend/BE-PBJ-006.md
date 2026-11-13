# Laporan Perubahan Backend — `BE-PBJ-006`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-PBJ-006` |
| Judul | PR draft |
| Slice | `MVP-1` — permintaan pengadaan (`EPIC PBJ-04`) |
| Roadmap | `perencanaan-permintaan/roadmap/backend-roadmap.md` revisi 1, kartu `BE-PBJ-006` |
| Trace | `PBJ-DEC-020`, `022`, `036`; FR-PBJ-030, 033; `PBJ-VAL-002`..`005`, `010`, `090`, `093`; `AT-PP-08`, `09` |
| Contract version | `PBJ-PP-API` 1.1 (bagian Purchase Request, dengan delta di §3.3), `PBJ-PP-PERM` 1.2 |
| Dependency | `BE-PBJ-003` 🟡 |
| Klasifikasi | `MEDIUM` (logika 2, kontrak API 1, database 2, keamanan 1) |
| Task mode | `BACKEND` (diberikan pemilik 12 November 2026, bersama `BE-PBJ-005`) |
| Target tulis | `Areas/Corporate/ProcurementManagement/{Request,Common}/**`, service master (cek pemakaian), konfigurasi EF, `ApplicationDbContext.cs`, extension DI, migration `AddProcurementPurchaseRequest`; basis data dev tidak disentuh |
| Model | Claude Opus 5.5 |
| Branch / commit | `sukmagp` @ `100c011d` (belum di-commit) |
| Tanggal | 12 November 2026 |
| Status | 🟡 **SEBAGIAN.** Kode dan migration ada; build, QBE, skrip EF, dan konsistensi snapshot `PASS`. Belum: uji runtime `AT-PP-08`, `09` (migration belum diterapkan; aplikasi tidak dapat start di dev DB karena migration rekan) |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `ProcurementManagement` / `Request` |
| Registry | `Corporate / Procurement | ProcurementManagement / Request | Prc | ACTIVE` (`PBJ-DEC-035`) |
| Keberlakuan | `NEW CODE`; `TOUCHED LEGACY` untuk `ApplicationDbContext.cs` |
| Arketipe endpoint | Transaksi ber-lifecycle — tanpa `GET /options`, `PATCH /{id}/status`, `DELETE /{id}` |
| QBE yang berlaku | `QBE-ENT-001`, `QBE-NAM-002` (`Prc`), `QBE-CFG-001`, `QBE-MOD-001..003`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-DEL-001` (baris lama ditandai `IsDelete`), `QBE-CODE-002..004` (nomor PR belum dibuat saat draf), `QBE-TXN-001`, `QBE-LOG-001`, `QBE-AUD-001` |
| Hasil checker | QBE `PASS` (15 berkas, 0 temuan) |

## 1. Masalah yang diperbaiki

Unit belum punya tempat menyusun permintaan pengadaan. Task ini menyediakan draf PR: dibuat, diubah, dilihat
dalam daftar dan ringkasan, serta riwayatnya — dengan pembatasan unit, tanpa nomor (nomor dibuat saat diajukan,
`BE-PBJ-007`).

## 2. Proses bisnis

1. Pengusul memilih unit penempatannya, sumber dana, tanggal dibutuhkan, urgensi, lalu menambah baris barang
   dari katalog aktif.
2. Server memeriksa:
   - unit harus unit penempatan pengusul hari ini (`PBJ-VAL-010`), **walaupun** pengguna boleh melihat semua unit;
   - jumlah > 0 (`PBJ-VAL-002`), tidak pecahan bila satuannya bulat (`PBJ-VAL-003`), harga ≥ 0 (`PBJ-VAL-004`);
   - barang aktif di katalog (`PBJ-VAL-005`).
3. Server menghitung total (jumlah × perkiraan harga, dibulatkan 2 desimal) — angka dari klien diabaikan.
4. PR tersimpan berstatus `DRAFT` tanpa nomor, dengan satu baris riwayat `CREATE`.
5. Mengubah draf hanya oleh pembuatnya, saat `DRAFT`/`RETURNED`, dengan versi data terakhir yang dibaca.
   Baris lama ditandai terhapus, baris baru ditulis, dalam satu transaksi.
6. Daftar, ringkasan, detail, dan riwayat disaring cakupan unit: tanpa `ProcurementRequest : ReadAllUnits` hanya
   PR unit penempatan; PR unit lain dijawab 404.

**Contoh penolakan:**
- Staf IGD membuat PR untuk Radiologi → 422 "Anda tidak terdaftar di unit Radiologi."
- Baris 2 berisi 1,5 Box → 422 "Satuan Box tidak boleh pecahan."
- Dua orang membuka draf yang sama; yang kedua menyimpan → 409 "Data sudah diubah orang lain…"
- Staf IGD membuka tautan PR Radiologi → 404 "Pengajuan tidak ditemukan atau bukan milik unit Anda."

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Model dan konfigurasi master Pengadaan, `ProcurementDataScopeService`, `ProcurementDocumentHistoryWriter`,
`IProcurementDocumentAccessProvider`, pola controller master Pengadaan, `AccessExplicitPermissionAttribute` dan
contoh `RadiologyExplicitPermissions.cs`, `MstMeasurement.IsDecimalAllowed`, `ApplicationUserOrganization`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Request/Models/PrcPurchaseRequest.cs`, `PrcPurchaseRequestLine.cs` + 2 konfigurasi | **Baru.** Nomor unik bila terisi; `RowVersion` concurrency token; FK `Restrict` ke unit, sumber dana, barang; baris `Cascade`; cek `RequestedQuantity > 0`; unik (PR, nomor baris) untuk baris hidup |
| `Request/DTOs/PurchaseRequestDtos.cs` | **Baru.** Query, create/update, response, list item, ringkasan, metadata filter |
| `Request/Services/ProcurementPurchaseRequestService.cs` | **Baru.** Metadata, ringkasan, daftar, detail (+`CanEdit`), riwayat, buat, ubah; penyedia akses lampiran `PBJ_PR` (`PurchaseRequestAttachmentAccessProvider`) |
| `Request/Services/ProcurementRequestExceptions.cs` | **Baru.** 422 / 409 / 403 |
| `Request/Controllers/PurchaseRequestsController.cs` | **Baru.** 7 endpoint |
| `Request/ProcurementRequestExplicitPermissions.cs` | **Baru.** `ProcurementRequest : ReadAllUnits` (`PBJ-DEC-036`) |
| `Common/Services/ProcurementDataScopeService.cs` | +`GetPlacementDepartmentIdsAsync`, +`GetUserId` |
| `Common/Constants/ProcurementUrgencyLevels.cs`, `ProcurementPermissions.cs` | Konstanta urgensi; `RequestModuleCode`/`Name` |
| `MasterData/Services/ProcurementItemService.cs`, `ProcurementFundSourceService.cs` | Hapus ditolak 400 bila dipakai PR (`PBJ-VAL-071`) |
| `ApplicationDbContext.cs`, extension DI | +2 `DbSet`, +service, +penyedia akses |
| `Migrations/20261112160000_AddProcurementPurchaseRequest.cs` + `.Designer.cs`; snapshot | **Baru** / diperbarui |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Delta terhadap 1.1: (a) riwayat di `GET /{id}/status-history` (kartu menyebut `/history`) agar seragam dengan sub-resource riwayat dokumen lain; (b) tambahan `GET /filters/metadata` untuk konfigurasi saringan daftar; (c) query `mineOnly` untuk "PR saya". Tidak ada endpoint yang dihapus |
| Database | 2 tabel baru, 11 index (2 unik terfilter), 4 FK, 1 check constraint. Migration **dibuat, tidak diterapkan** |
| Keamanan/Auth | Setiap endpoint `[AccessAction]` + `[AccessPermission]` pada `ProcurementRequest`; cakupan unit lewat `ReadAllUnits` yang dapat dicentang di Akses Role; kepemilikan draf (pembuat) dan status diperiksa di service, tanpa hardcode peran. Lampiran PR kini aktif: pembuat saat `DRAFT`/`RETURNED` = ubah, lainnya dalam cakupan = baca, di luar cakupan = 404 |

## 4. Dokumentasi endpoint

#### Corporate / Procurement Management / Request / Purchase Request

Base URL: `api/v1/corporate/procurement-management/purchase-requests`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan status, urgensi, urutan, ukuran halaman | `ProcurementRequest : Read` | — | `PurchaseRequestFilterMetadataResponse` |
| `GET` | `/summary` | Total, mendesak, luar rencana, melebihi pagu, per status | `ProcurementRequest : Read` | — | `PurchaseRequestSummaryResponse` |
| `GET` | `/` | Daftar tersaring cakupan | `ProcurementRequest : Read` | `PurchaseRequestQuery` | `PagedResult<PurchaseRequestListItem>` |
| `GET` | `/{id}` | Detail + baris + `canEdit` | `ProcurementRequest : Read` | — | `PurchaseRequestResponse` |
| `GET` | `/{id}/status-history` | Riwayat dokumen | `ProcurementRequest : Read` | — | `List<DocumentHistoryResponse>` |
| `POST` | `/` | Simpan draf | `ProcurementRequest : Create` | `CreatePurchaseRequestRequest` | `PurchaseRequestResponse` |
| `PUT` | `/{id}` | Ubah draf | `ProcurementRequest : Update` | `UpdatePurchaseRequestRequest` (+`expectedRowVersion`) | `PurchaseRequestResponse` |

**Kode status:** 403 bukan pembuat draf; 404 tidak ada atau di luar cakupan; 409 data basi (`PBJ-VAL-090`) atau
status bukan draf (`PBJ-VAL-093`); 422 isian tidak sah.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` (perintah baku, bersama `BE-PBJ-005`) | 0 error, 608 warning (= baseline); DLL lebih baru dari source terakhir | `PASS` | Log build |
| EF `migrations script AddProcurementAttachment → AddProcurementPurchaseRequest` | 2 `CREATE TABLE`, 4 FK (3 `RESTRICT`, 1 `CASCADE`), 1 `CHECK`, 11 index | `PASS` | Keluaran skrip |
| Konsistensi snapshot (migration pemeriksa sementara, lalu dihapus) | 0 penyebutan entity Pengadaan; sisa = drift HR lama identik dengan pemeriksaan `BE-PBJ-003`/`004` | `PASS` | Log pemeriksaan |
| QBE checker | 0 temuan | `PASS` | Laporan checker |
| Kriteria 1 — draf tanpa nomor | `RequestNumber` tidak diisi saat buat/ubah | `PASS` (statis) | Review source |
| Kriteria 4 — hanya pembuat | 403 bila `RequesterUserId` ≠ pengguna | `PASS` (statis) | Review source |
| Kriteria 5 — `PBJ-VAL-002`..`005` | Divalidasi per baris dengan nomor baris di pesan | `PASS` (statis) | Review source |
| `AT-PP-08` (unit lain → 404), `AT-PP-09` (data basi → 409) | Tidak dijalankan | `NOT RUN` | Migration belum diterapkan; aplikasi tidak dapat start di dev DB (migration rekan tertunda) |

- **Uji manual:** `NOT FEASIBLE` saat ini.
- **AUTOMATED TEST:** `NOT APPLICABLE` — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`).

## 6. Status migration dan basis data

`20261112160000_AddProcurementPurchaseRequest` dibuat, **tidak diterapkan**. Urutan: setelah
`AddProcurementAttachment`. Aman tanpa downtime (tabel baru saja). Mundur: `Down` menghapus kedua tabel.

## 7. Risiko yang tersisa

| Risiko | Penanganan |
| --- | --- |
| Alasan kebutuhan dan alasan mendesak boleh kosong saat draf | Disengaja: diwajibkan saat diajukan (`BE-PBJ-007`) |
| `IsOutOfPlan`, pembagian dalam/luar rencana, dan kebutuhan beli belum dihitung | Milik `BE-PBJ-007` (saat diajukan) dan `BE-PBJ-008` (cek stok) |
| Pemakaian barang oleh baris rencana kebutuhan dan sumber dana oleh pagu belum diperiksa saat hapus | Ditandai di kode; `BE-PBJ-009` dan `BE-PBJ-010` |

## 8. Task berikutnya

`BE-PBJ-007` (ajukan bernomor, batal, tarik) setelah `BE-PBJ-009`; uji `AT-PP-53`..`56` lampiran kini mungkin
begitu migration diterapkan.
