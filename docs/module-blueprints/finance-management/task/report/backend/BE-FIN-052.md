# Laporan Perubahan Backend — `BE-FIN-052`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-052` |
| Judul | Sumbu klaim penjamin pada `FinReceivableInvoiceBatch` |
| Slice | `REV-13C` — `EPIC FIN-18` |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian "AMENDMENT ROADMAP REVISI 13" |
| Trace | `FIN-DEC-097`, `098`; `FIN-DES-070`, `071`, `072` |
| Contract version | `FIN-API-1.3` §D.1, D.3; `FIN-STATE-1.4` §D.1; `FIN-VAL-1.5` (`FIN-VAL-147`..`153`); `FIN-PERM-1.5` §E.1 — kelimanya `approved` 1 Oktober 2026 (lihat `blueprint-manifest.md` `status_note_revision_13`) |
| Dependency | Tidak ada — task ini berdiri sendiri pada grafik dependency `REV-13` |
| Klasifikasi | `MEDIUM` — satu repository (skor 0); 5 berkas diubah (skor 1); 1 tabel diperluas 7 kolom + 1 migration yang **belum dibuat** (skor 2); 3 endpoint baru, pola arketipe sudah ada (skor 1); database — 1 tabel `Diperbarui`, 1 migration (skor 2); keamanan/auth — nol resource/action baru (skor 0); UI/workflow — sumbu status baru dengan invariant lintas-sumbu yang ketat (skor 1). Total 7 → `MEDIUM` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/Receivable/**`, `Repositories/Configurations/Corporate/FinanceManagement/Receivable/**`, dan laporan ini |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | Belum di-commit — branch `Yasmina`, HEAD `d6978487` |
| Tanggal | 1 Oktober 2026 |
| Status | ✅ **Selesai 1 Oktober 2026.** Source dan migration lengkap sesuai kontrak. `dotnet build` **PASS** (dikonfirmasi pengguna) dan migration `20261001090000_AddClaimTrackingToFinReceivableInvoiceBatch` **berhasil dieksekusi ke database** (dikonfirmasi pengguna) |

---

## 1. Masalah yang diperbaiki

Batch Tagihan AR (`FinReceivableInvoiceBatch`) sudah bisa diterbitkan ke penjamin dan dilunasi,
tetapi sistem tidak mencatat **apa jawaban penjamin** atas tagihan itu — apakah berkasnya sudah
diterima, berapa nominal yang disetujui, dan kapan klaimnya ditutup. Petugas AR tidak punya cara
melacak keadaan yang paling sering terjadi pada klaim penjamin: *"penjamin sudah menyetujui
nominalnya, tetapi uangnya belum masuk sama sekali"* — karena hanya ada satu kolom status
(`Status`) yang menjawab "apakah dokumen terbit dan apakah lunas", bukan "apa kata penjamin".

Contoh konkret: RS mengirim tagihan Rp 120.000.000 ke BPJS. Dua minggu kemudian BPJS membalas
hanya menyetujui Rp 118.500.000 karena satu item ditolak. Sebelum task ini, informasi itu tidak
punya tempat tersimpan sama sekali — petugas harus mengingatnya di luar sistem, dan selisih
Rp 1.500.000 tidak pernah terlihat sebagai pekerjaan yang menunggu dihapus dari buku.

---

## 2. Proses bisnis

**Pelaku:** Petugas AR Finance.

**Pemicu:** Tagihan gabungan (`FinReceivableInvoiceBatch`) sudah diterbitkan (`Status = ISSUED`).

**Langkah normal:**

1. Saat petugas menerbitkan tagihan (`POST /{id}/issue`, sudah ada), sistem **otomatis** menandai
   klaim sebagai `SUBMITTED` — petugas tidak perlu tindakan tambahan.
2. Penjamin memeriksa berkas. Bila dinyatakan lengkap, petugas AR menandainya **Diverifikasi**
   (`POST /{id}/claim/verify`).
3. Penjamin membalas dengan nominal yang disetujui. Petugas AR mencatatnya (`POST /{id}/claim/approve`),
   boleh langsung dari `SUBMITTED` (melompati langkah verifikasi) maupun dari `PAYER_VERIFIED`.
   - Bila nominal yang disetujui **sama dengan** total tagihan: selesai, nol selisih.
   - Bila **lebih kecil**: petugas **wajib** menuliskan alasannya. Selisihnya tidak hilang — ia
     tetap terlihat pada response sebagai pekerjaan yang menunggu dihapus dari buku lewat jalur
     penghapusan piutang yang sudah ada (di luar cakupan task ini).
4. Setelah tidak ada tindak lanjut lagi, petugas AR menutup klaim (`POST /{id}/claim/close`).

**Aturan yang berlaku:** Sumbu klaim (`ClaimStatus`) **tidak pernah** mengubah sumbu dokumen/
pelunasan (`Status`), dan sebaliknya. Keduanya bergerak sendiri-sendiri pada entity yang sama —
lihat §2 Addendum untuk contoh konkretnya.

**Jalur tidak normal:**

- Mencoba aksi klaim pada tagihan yang belum pernah diterbitkan → ditolak, diarahkan menerbitkan
  tagihan lebih dulu (`FIN-VAL-147`).
- Nominal disetujui melebihi total tagihan → ditolak (`FIN-VAL-150`).
- Nominal disetujui lebih kecil tanpa alasan → ditolak (`FIN-VAL-151`).
- Mengubah klaim yang sudah ditutup, atau melompat ke langkah yang tidak sah dari status saat ini
  → ditolak (`FIN-VAL-152`).
- Dua petugas mengubah klaim yang sama bersamaan → petugas kedua ditolak dan diminta memuat ulang
  (`FIN-VAL-153`, lewat `RowVersion`/concurrency token yang sudah ada).

**Hasil akhir:** Batch Tagihan AR memiliki dua sumbu informasi yang dapat dibaca terpisah: status
dokumen/pelunasan (tidak berubah, `B.7`) dan status klaim penjamin (baru, `D.1`), beserta selisih
klaim yang dihitung otomatis pada setiap response.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/02-backend-architecture.md` — AMENDMENT REVISI 13
  bagian J (rancangan lengkap: class diagram, migration plan, endpoint)
- `docs/module-blueprints/finance-management/contracts/api-contract.md` §D.1, D.3
- `docs/module-blueprints/finance-management/contracts/state-transition-matrix.md` §D.1
- `docs/module-blueprints/finance-management/contracts/validation-matrix.md` (`FIN-VAL-147`..`153`)
- `docs/module-blueprints/finance-management/contracts/permission-audit-matrix.md` §E.1-E.3
- `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivableInvoiceBatch.cs` — model yang
  diperluas
- `Areas/Corporate/FinanceManagement/Receivable/Services/FinanceReceivableInvoiceBatchService.cs`
  — pola `IssueAsync`/`CancelAsync` (concurrency token, exception, audit) dipakai ulang persis
  untuk ketiga aksi klaim baru
- `Areas/Corporate/FinanceManagement/Receivable/Controllers/FinanceReceivableInvoiceBatchesController.cs`
  — pola `[AccessAction]`/`[AccessPermission]` existing; dikonfirmasi resource hak akses yang
  benar-benar dipakai registry adalah argumen ke-1 `[AccessPermission]` (`"FinanceReceivableInvoiceBatch"`),
  **bukan** `ControllerName` pada `[AccessController]` (`"ReceivableInvoiceBatch"`) — dibuktikan
  langsung dari `Services/Security/PermissionRegistryDescriptor.cs` baris 374-375
  (`permission.Arguments[0]` adalah `ResourceName`). Keempat action lama pada controller ini sudah
  memakai pola itu secara konsisten; ketiga aksi klaim baru mengikutinya
- `Areas/Corporate/FinanceManagement/Receivable/Dtos/FinanceReceivableInvoiceBatchDtos.cs` — pola
  `[Range(..., ErrorMessage = "...")]` yang sudah dipakai `FinanceArDtos.cs`/`FinanceApDtos.cs`
  untuk pesan validasi Bahasa Indonesia pada DataAnnotation
- `Repositories/Configurations/Corporate/FinanceManagement/Receivable/FinReceivableInvoiceBatchConfiguration.cs`
  — pola check constraint dan index existing untuk `Status`
- `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` — dikonfirmasi `FinanceManagement /
  Receivable / Piutang` sudah terdaftar prefix `Fin`, `ACTIVE`. Nol pendaftaran baru dibutuhkan
  (model yang disentuh sudah ada, bukan entity baru)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/.../Receivable/Models/FinReceivableInvoiceBatch.cs` | Tujuh properti nullable baru (`ClaimStatus`, `ApprovedAmount`, `PayerClaimReference`, `ClaimNote`, `PayerVerifiedAt`, `ClaimApprovedAt`, `ClaimClosedAt`) beserta static class `FinReceivableInvoiceBatchClaimStatuses` (empat konstanta) |
| `Repositories/.../FinReceivableInvoiceBatchConfiguration.cs` | Konfigurasi tujuh kolom baru (`HasMaxLength`/`HasPrecision`/`HasColumnType`), satu check constraint `CK_FinReceivableInvoiceBatch_ClaimStatus`, satu index `IX_FinReceivableInvoiceBatch_ClaimStatus` |
| `Areas/.../Receivable/Dtos/FinanceReceivableInvoiceBatchDtos.cs` | Delapan field baru pada `ReceivableInvoiceBatchResponse` (termasuk `ClaimVarianceAmount` yang dihitung, bukan disimpan); tiga DTO request baru: `ClaimVerifyRequest`, `ClaimApproveRequest`, `ClaimCloseRequest` |
| `Areas/.../Receivable/Services/FinanceReceivableInvoiceBatchService.cs` | `IssueAsync` menambah satu baris (`ClaimStatus = Submitted` otomatis saat terbit); tiga method baru `VerifyClaimAsync`/`ApproveClaimAsync`/`CloseClaimAsync`; helper `ResolveEffectiveClaimStatus` (lihat §3.3); `Map()` diperluas delapan field, dipanggil dari dua tempat (`GetPagedAsync` lewat `Select`, dan `GetByIdAsync` lewat `ReceivableInvoiceBatchDetailResponse`) |
| `Areas/.../Receivable/Controllers/FinanceReceivableInvoiceBatchesController.cs` | Tiga endpoint baru (`ClaimVerify`/`ClaimApprove`/`ClaimClose`), `Map()` diperluas delapan field, doc comment kelas diperbarui |
| `Migrations/20261001090000_AddClaimTrackingToFinReceivableInvoiceBatch.cs` **(baru)** | `Up()`: tujuh `AddColumn` + satu `migrationBuilder.Sql(...)` check constraint + satu `CreateIndex`. `Down()`: urutan kebalikannya |
| `Migrations/20261001090000_AddClaimTrackingToFinReceivableInvoiceBatch.Designer.cs` **(baru)** | Snapshot model lengkap pada titik migration ini — dibuat dengan menyalin `ApplicationDbContextModelSnapshot.cs` yang sudah diperbarui (lihat §3.3 catatan metode), diverifikasi `diff` byte-identik atas isi `BuildModel`/`BuildTargetModel` |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Tujuh properti, satu check constraint, satu index baru ditambahkan ke blok `FinReceivableInvoiceBatch`, urutan alfabetis dipertahankan sesuai konvensi EF Core |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tiga endpoint baru, aditif murni — lihat Bagian 4. Response `ReceivableInvoiceBatchResponse`/`ReceivableInvoiceBatchDetailResponse` bertambah 8 field, konsumen lama tidak terdampak |
| Database | **Satu tabel `Diperbarui`** (`FinReceivableInvoiceBatch`, 7 kolom nullable + 1 check constraint + 1 index). **Migration `20261001090000_AddClaimTrackingToFinReceivableInvoiceBatch` dibuat** (file `.cs` + `.Designer.cs`), `ApplicationDbContextModelSnapshot.cs` diperbarui selaras. **Belum dieksekusi** ke database — wewenang terpisah, belum diminta |
| Keamanan/Auth | **Nol** resource dan **nol** action baru. Ketiga endpoint memakai `[AccessPermission("FinanceReceivableInvoiceBatch", "Update")]` yang sudah terdaftar (dipakai `Cancel`). Satu temuan desain dicatat eksplisit di source: batch lama yang sudah `ISSUED` sebelum migration ini dijalankan tidak di-backfill — `ResolveEffectiveClaimStatus` memperlakukannya seolah `SUBMITTED` tanpa menulis apa pun ke baris lama, supaya tetap bisa ditindaklanjuti petugas |

**Catatan desain yang perlu ditegaskan (bukan penyimpangan dari rancangan, tapi detail yang baru
konkret saat implementasi):** rancangan `02-backend-architecture.md` §J.3 menyebut tabel transisi
untuk `Approve` hanya dari `PAYER_VERIFIED`, tetapi baris lain pada tabel yang sama
(`state-transition-matrix.md` §D.1) eksplisit mengizinkan `SUBMITTED → APPROVED` (melompati
verifikasi). Implementasi mengikuti kontrak §D.1 (sumber kebenaran yang lebih rinci dan lebih
baru), bukan ringkasan §J.3 — keduanya tidak bertentangan, §J.3 hanya tidak menyebutkan jalur
pintas itu secara eksplisit.

**Metode pembuatan migration (atas permintaan eksplisit pengguna, tanpa `dotnet build`/`dotnet ef`):**
Ketiga berkas migration **ditulis manual**, bukan dihasilkan `dotnet ef migrations add` — pengguna
secara eksplisit meminta berkasnya dibuat tanpa menjalankan `dotnet build`. Urutan kerjanya:
(1) blok `FinReceivableInvoiceBatch` pada `ApplicationDbContextModelSnapshot.cs` disunting langsung,
menambahkan tujuh properti pada posisi alfabetis yang benar (meniru pola `TotalAmount` yang sudah
ada persis pada entity yang sama untuk `.HasPrecision(18, 2)`), satu check constraint, satu index;
(2) berkas `.Designer.cs` migration dibentuk dengan menyalin isi `BuildModel`
(`ApplicationDbContextModelSnapshot.cs` yang sudah diperbarui) ke `BuildTargetModel` migration baru,
lalu **diverifikasi `diff` baris-per-baris** bahwa isi keduanya byte-identik (`exit code 0`) — bukan
disalin dengan asumsi; (3) berkas migration utama (`Up`/`Down`) ditulis mengikuti pola
`migrationBuilder.AddColumn`/`.Sql(...)` untuk check constraint/`.CreateIndex` yang sudah dipakai
`AddPPNAmountToFinSupplierReturn.cs` dan `AddArInvoiceBatchAndReceiptDeduction.cs`. UTF-8 BOM pada
`.Designer.cs` disamakan dengan berkas Designer existing. `Migrations/MigrationMetadata.g.cs`
**tidak disentuh** — berkas itu auto-generated oleh skrip terpisah dan baru mencakup migration
sampai pertengahan September; migration-migration terbaru (termasuk punya task ini) belum diarsipkan
ke sana.

**Risiko metode ini yang MUST dicatat apa adanya:** karena migration dibuat **tanpa** `dotnet build`,
**tidak ada verifikasi compiler** bahwa `Up()`/`Down()` benar-benar valid C# atau bahwa `Designer.cs`
benar-benar cocok dengan model runtime — verifikasinya murni review manual dan `diff` tekstual
terhadap `ApplicationDbContextModelSnapshot.cs`. Risiko ini ditanggung sadar sesuai permintaan
eksplisit pengguna, dan **MUST** ditutup dengan `dotnet build` sebelum migration ini dianggap aman
dieksekusi ke database.

---

## 4. Dokumentasi endpoint

#### Corporate / Finance Management / Receivable Invoice Batch

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/{id}/claim/verify` | Menandai berkas klaim sudah diterima dan dinyatakan lengkap oleh penjamin | `FinanceReceivableInvoiceBatch : Update` |
| `POST` | `/{id}/claim/approve` | Mencatat nominal yang disetujui penjamin, beserta selisih bila lebih kecil dari tagihan | `FinanceReceivableInvoiceBatch : Update` |
| `POST` | `/{id}/claim/close` | Menutup klaim; tidak ada tindak lanjut lagi | `FinanceReceivableInvoiceBatch : Update` |

Ketiganya mengembalikan `ApiResponse<ReceivableInvoiceBatchResponse>` yang sekarang memuat
`ClaimStatus`, `ApprovedAmount`, `ClaimVarianceAmount` (dihitung), `PayerClaimReference`,
`ClaimNote`, `PayerVerifiedAt`, `ClaimApprovedAt`, `ClaimClosedAt`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | Berhasil tanpa error | `PASS` | Dikonfirmasi pengguna, 1 Oktober 2026 — membuktikan source **dan** migration manual (lihat §3.3) valid secara compiler |
| Eksekusi migration ke database | Berhasil | `PASS` | Dikonfirmasi pengguna, 1 Oktober 2026 |
| `diff` isi `BuildModel`/`BuildTargetModel` antara snapshot dan Designer.cs migration baru | Byte-identik | `PASS` | `diff` keluar kode 0 atas 123.729 baris yang dibandingkan |
| Review diff/scope | 8 berkas source + migration, persis sesuai desain `02-backend-architecture.md` §J; `FinBankDeposit.cs` terlihat di `git status` **bukan dari task ini**, tidak disentuh | `PASS` | `git status --short` Bagian 7 |
| Review kontrak API terhadap implementasi | Tiga endpoint, bentuk request/response, kode status 400/403/404/409/422 sesuai `FIN-API-1.3` §D.1 dan `FIN-VAL-1.5` | `PASS` | Perbandingan langsung kode vs kontrak |
| Review proses bisnis — dua sumbu status tidak saling menulis | `VerifyClaimAsync`/`ApproveClaimAsync`/`CloseClaimAsync` tidak menyentuh `Status`/`IssuedAt`; `RefreshStatusAsync` (pelunasan) tidak menyentuh kolom klaim | `PASS` | Baca kode kedua blok, nol baris bersinggungan |
| Review proses bisnis — selisih tidak disimpan | `ClaimVarianceAmount` murni properti response, dihitung di `Map()`, tidak ada kolom tabel untuknya | `PASS` | `FinReceivableInvoiceBatch.cs` tidak memiliki properti itu |
| QBE preflight | Area `Corporate/Finance`, Module `FinanceManagement`, Submodule `Receivable`, prefix `Fin` `ACTIVE` — sudah terdaftar, applicability `TOUCHED LEGACY` | `PASS` | `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 15 |

Uji manual: `NOT FEASIBLE` — tidak ada dev server/klien HTTP yang dijalankan pada sesi ini untuk
memanggil ketiga endpoint aksi klaim secara runtime. Build dan migration sudah PASS, tetapi
pemanggilan endpoint sungguhan belum dicoba.

**AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis
(`rules/backend/TEST_POLICY.md`).**

**Tidak dijalankan:** `dotnet build` (pengguna eksplisit meminta migration dibuat tanpa build);
eksekusi migration ke database (wewenang terpisah, belum diminta); uji manual/runtime (bergantung
pada build dan eksekusi migration).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `ClaimStatus` berpindah sesuai `FIN-STATE-1.5` §D.1 | Terpenuhi | `VerifyClaimAsync`/`ApproveClaimAsync`/`CloseClaimAsync` menegakkan `ResolveEffectiveClaimStatus` persis sesuai tabel transisi |
| `ApprovedAmount` lebih kecil dari `TotalAmount` **tidak** mengubah `OutstandingAmount` piutang anggota mana pun | Terpenuhi | `ApproveClaimAsync` hanya menulis kolom pada `FinReceivableInvoiceBatch`, tidak pernah memuat/menulis `FinReceivable` |
| `FIN-VAL-147`..`153` ditegakkan | Terpenuhi | Ketujuh aturan dipetakan langsung ke pengecualian dan kondisinya masing-masing di ketiga method |
| Nol action hak akses baru | Terpenuhi | Ketiga endpoint memakai `[AccessAction("Update", ...)]` dan `[AccessPermission("FinanceReceivableInvoiceBatch", "Update")]` yang sudah ada |
| Build PASS | Terpenuhi | `dotnet build` PASS, dikonfirmasi pengguna 1 Oktober 2026 |
| Migration dibuat dan dieksekusi atas izin eksplisit | Terpenuhi | `Migrations/20261001090000_AddClaimTrackingToFinReceivableInvoiceBatch.cs`/`.Designer.cs` dibuat; eksekusi ke database dikonfirmasi berhasil 1 Oktober 2026 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Migration dibuat **manual** tanpa `dotnet build`/`dotnet ef` atas permintaan eksplisit pengguna — lihat §3.3 "Metode pembuatan migration". Risiko itu **tertutup**: `dotnet build` PASS dan eksekusi migration berhasil, keduanya dikonfirmasi pengguna 1 Oktober 2026 |
| Masalah yang diketahui | Tidak ada temuan baru di luar yang sudah dicatat rancangan (`FIN-OQ-044`, tidak tersentuh task ini) |
| Risiko tersisa | **Rendah.** Build dan migration sudah dibuktikan PASS. Uji manual/runtime atas ketiga endpoint aksi klaim belum dicoba — risiko tersisa murni pada perilaku runtime yang belum diverifikasi, bukan lagi pada validitas compiler/skema |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `git status --short` mencakup tujuh berkas source dan migration task ini (lihat Bagian 3.2), ditambah perubahan dokumen blueprint dari pass sebelumnya pada sesi yang sama (tidak disentuh task ini). **Satu berkas tidak terkait** (`Areas/Corporate/FinanceManagement/CashManagement/Models/FinBankDeposit.cs`) juga termodifikasi tetapi **bukan dari task ini** — dibiarkan apa adanya, bukan pekerjaan task ini untuk disentuh atau dilaporkan isinya |
| Langkah berikutnya | Uji manual/runtime ketiga endpoint aksi klaim bila dibutuhkan; lanjut `BE-FIN-054`/`055` (`REV-13B`, berdiri sendiri) atau `BE-FIN-056` (`REV-13D`, piutang sewa non-pasien) |
