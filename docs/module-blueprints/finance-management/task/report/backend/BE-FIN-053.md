# Laporan Perubahan Backend — `BE-FIN-053`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-053` |
| Judul | Daftar penghapusan piutang lintas piutang — `GET /receivables/write-offs` |
| Slice | `REV-13B` — `EPIC FIN-18` |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian "AMENDMENT ROADMAP REVISI 13" |
| Trace | `FIN-DEC-094`; `FIN-DES-073` |
| Contract version | `FIN-API-1.4` §D.2 — `approved` 1 Oktober 2026 (lihat `blueprint-manifest.md` `status_note_revision_13`) |
| Dependency | Tidak ada — berdiri sendiri pada grafik dependency `REV-13` |
| Klasifikasi | `LIGHT` — satu repository (skor 0); 3 berkas diubah (skor 0); nol model baru, murni query baca (skor 0); 1 endpoint baru, pola arketipe read-only sudah ada (skor 0); database — nol migration (skor 0); keamanan/auth — nol resource/action baru (skor 0); UI/workflow — permukaan baca murni, nol alur baru (skor 0). Total 0 → `LIGHT` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/Receivable/**` dan laporan ini |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | Belum di-commit — branch `Yasmina`, HEAD `d6978487` |
| Tanggal | 1 Oktober 2026 |
| Status | ✅ **Selesai 1 Oktober 2026.** Source lengkap sesuai kontrak. `dotnet build` **PASS** (dikonfirmasi pengguna) |

---

## 1. Masalah yang diperbaiki

Penghapusan piutang (write-off) sudah bisa diajukan dan disetujui/ditolak, tetapi hanya dapat
dilihat **satu per satu** di dalam detail piutangnya masing-masing (`GET /receivables/{id}`).
Tidak ada cara bagi petugas AR melihat **seluruh** penghapusan piutang sekaligus dalam satu
daftar — misalnya untuk meninjau semua penghapusan bulan ini, atau semua penghapusan atas satu
penjamin tertentu. Layar "Pemutihan Piutang" pada menu V1 (`FIN-DEC-094`) butuh permukaan baca
seperti ini dan belum punya endpoint yang bisa dipakai.

---

## 2. Proses bisnis

**Pelaku:** Petugas AR Finance.

**Pemicu:** Petugas membuka layar "Pemutihan Piutang" untuk meninjau penghapusan piutang yang
sudah terjadi.

**Langkah normal:**

1. Petugas membuka daftar, opsional menyaring berdasarkan periode (`StartDate`/`EndDate` atas
   tanggal pengajuan), status (`Requested`/`Approved`/`Rejected`), atau penjamin tertentu.
2. Sistem menampilkan setiap baris penghapusan beserta piutang asalnya (nomor dan penjamin),
   nominal, alasan, status, tanggal pengajuan, dan tanggal keputusan (bila sudah diputuskan).
3. Petugas dapat menelusuri ke detail piutang asal bila perlu tindak lanjut lebih jauh —
   **di luar cakupan task ini**, tautannya memakai `GET /receivables/{id}` yang sudah ada.

**Aturan yang berlaku:** Endpoint ini **baca saja**. Pengajuan, persetujuan, dan penolakan
write-off **tetap** lewat tiga endpoint per-piutang yang sudah ada (`POST .../write-offs`,
`.../write-offs/{id}/approve`, `.../write-offs/{id}/reject`) beserta maker-checker-nya — task ini
**tidak menambah atau mengubah satu pun** aturan di jalur itu.

**Jalur tidak normal:** Tidak ada — endpoint `GET` murni, nol kondisi gagal bisnis selain
parameter saringan yang tidak valid (ditangani validasi standar ASP.NET Core).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/02-backend-architecture.md` AMENDMENT REVISI 13
  §J.5 (rancangan endpoint)
- `docs/module-blueprints/finance-management/contracts/api-contract.md` §D.2
- `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivableWriteOff.cs` — model yang
  dibaca (tidak diubah), termasuk konfirmasi bahwa `ApprovedBy`/`ApprovedAt` dipakai untuk
  **kedua** keputusan (setuju maupun tolak), bukan hanya persetujuan — dibuktikan langsung dari
  `FinanceReceivableService.DecideWriteOffAsync` baris 456-457
- `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableService.cs` — pola
  `GetPagedAsync` (saringan, sorting, paginasi) dipakai ulang persis untuk method baru
- `Areas/Corporate/FinanceManagement/Receivable/Controllers/FinanceReceivablesController.cs` —
  dikonfirmasi resource hak akses `[AccessPermission]` controller ini memakai nama yang **sama**
  dengan `ControllerName` pada `[AccessController]` (`"FinanceReceivable"`) — berbeda dari
  `FinanceReceivableInvoiceBatchesController` yang dikonfirmasi `BE-FIN-052`; routing literal
  segment (`write-offs`) beserta `{id:guid}` yang sudah berdampingan aman untuk tiga route literal
  lain (`filters/metadata`, `summary`, `aging`) — pola yang sama dipakai ulang
- `Areas/Corporate/FinanceManagement/Receivable/DTOs/FinanceReceivableDtos.cs` — pola
  `ReceivableQuery`/`ReceivableResponse` dipakai sebagai acuan bentuk `PagedQuery`/`Response`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/.../Receivable/Dtos/FinanceReceivableDtos.cs` | Dua DTO baru: `ReceivableWriteOffQuery` (saringan `StartDate`/`EndDate`/`Status`/`DebtorReferenceId`, paginasi) dan `ReceivableWriteOffRowResponse` (baris lintas piutang, memuat `ReceivableId`/`ReceivableNumber`/`DebtorReferenceId`) |
| `Areas/.../Receivable/Services/FinanceReceivableService.cs` | Satu method baru `GetWriteOffsAsync` — query `FinReceivableWriteOffs` di-`Include(Receivable)`, disaring, diurutkan, dipaginasi; **tidak menyentuh** `RequestWriteOffAsync`/`ApproveWriteOffAsync`/`RejectWriteOffAsync`/`DecideWriteOffAsync` sama sekali |
| `Areas/.../Receivable/Controllers/FinanceReceivablesController.cs` | Satu endpoint baru `GET write-offs`, memakai `[AccessPermission("FinanceReceivable", "Read")]` yang sudah terdaftar; doc comment kelas diperbarui |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Satu endpoint baru, aditif murni. Nol endpoint lama berubah bentuk |
| Database | **Nol.** Murni query baca atas tabel yang sudah ada (`FinReceivableWriteOff`, `FinReceivable`). Nol migration |
| Keamanan/Auth | **Nol** resource dan **nol** action baru. Memakai `FinanceReceivable : Read` yang sudah terdaftar dan dipakai tiga endpoint lain pada controller yang sama |

---

## 4. Dokumentasi endpoint

#### Corporate / Finance Management / Receivable

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/write-offs` | Daftar penghapusan piutang lintas piutang — layar "Pemutihan Piutang" | `FinanceReceivable : Read` |

Mengembalikan `ApiResponse<PagedResult<ReceivableWriteOffRowResponse>>`: `Id`, `WriteOffNumber`,
`ReceivableId`, `ReceivableNumber`, `DebtorReferenceId`, `Amount`, `Reason`, `Status`,
`RequestedAt`, `DecidedAt`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Berhasil tanpa error | `PASS` | Dikonfirmasi pengguna, 1 Oktober 2026 |
| Review diff/scope | 3 berkas, persis sesuai desain `02-backend-architecture.md` §J.5; empat berkas tidak terkait (`FinBankDeposit.cs`, `FinDailyCashSnapshot.cs`, `20260921000004_AddFinanceCashManagement.Designer.cs`, dan sisa `BE-FIN-052` yang belum di-build) terlihat di `git status` **bukan dari task ini**, tidak disentuh | `PASS` | `git status --short` Bagian 7 |
| Review kontrak API terhadap implementasi | Satu endpoint, bentuk query/response sesuai `FIN-API-1.4` §D.2 | `PASS` | Perbandingan langsung kode vs kontrak |
| Review proses bisnis — nol perubahan pada jalur tulis write-off | `RequestWriteOffAsync`/`ApproveWriteOffAsync`/`RejectWriteOffAsync`/`DecideWriteOffAsync` nol baris berubah | `PASS` | `git diff` method-method itu kosong |
| QBE preflight | Area `Corporate/Finance`, Module `FinanceManagement`, Submodule `Receivable`, prefix `Fin` `ACTIVE` — sudah terdaftar, applicability `TOUCHED LEGACY` (menambah method pada service yang sudah ada) | `PASS` | `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 15 |

Uji manual: `NOT FEASIBLE` — tidak ada dev server/klien HTTP yang dijalankan pada sesi ini untuk
memanggil endpoint secara runtime. Build sudah PASS, tetapi pemanggilan endpoint sungguhan belum
dicoba.

**AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).**

**Tidak dijalankan:** uji manual/runtime (endpoint belum dipanggil langsung).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Daftar tersaring periode, status, dan penjamin | Terpenuhi | `ReceivableWriteOffQuery` beserta penerapannya di `GetWriteOffsAsync` |
| Paginasi benar | Terpenuhi | Pola `Skip`/`Take`/`TotalPage` identik dengan `GetPagedAsync` yang sudah berjalan |
| Nol perubahan pada jalur pembuatan write-off | Terpenuhi | Lihat Bagian 5 |
| Build PASS | Terpenuhi | `dotnet build` PASS, dikonfirmasi pengguna 1 Oktober 2026 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `dotnet build` dan migration `BE-FIN-052` yang menumpuk pada working tree yang sama dikonfirmasi PASS/berhasil oleh pengguna, 1 Oktober 2026 — mencakup perubahan task ini juga |
| Masalah yang diketahui | Tidak ada |
| Risiko tersisa | **Rendah.** Build sudah dibuktikan PASS. Uji manual/runtime endpoint belum dicoba |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `git status --short` mencakup tiga berkas task ini (Bagian 3.2). **Empat berkas tidak terkait** terlihat termodifikasi (`FinBankDeposit.cs`, `FinDailyCashSnapshot.cs`, `Migrations/20260921000004_AddFinanceCashManagement.Designer.cs` — kemungkinan dari sesi IDE pengguna yang berjalan bersamaan) — **bukan dari task ini**, tidak disentuh. Sisa perubahan `BE-FIN-052` (migration, model, service, controller, DTO Batch Tagihan AR) juga masih ada, belum di-build/dieksekusi — bukan bagian task ini |
| Langkah berikutnya | Lanjut `BE-FIN-054`/`055` (`REV-13B`, berdiri sendiri) atau `BE-FIN-056` (`REV-13D`) |
