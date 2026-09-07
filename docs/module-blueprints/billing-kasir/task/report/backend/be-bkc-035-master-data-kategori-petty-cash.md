# BE-BKC-035 — Master data kategori pengeluaran kas kecil (Petty Cash Category)

- TASK ID: `BE-BKC-035`
- TASK TYPE: Implementasi backend — CRUD master data baru di atas tabel yang sudah ada
- COMPLEXITY: `MEDIUM` (skor 7 — repository 0, berkas diperiksa >20 → 2, berkas diubah 5 → 1, logika bisnis sedang → 1, kontrak API memakai kontrak yang sudah dikunci → 1, database hanya perilaku persistence pada tabel existing → 1, keamanan mengaitkan permission baru mengikuti pola baku → 1, UI/workflow tidak ada → 0)
- CLASSIFICATION SCORE: 7
- MODEL: Claude Sonnet 5
- TASK MODE: `BACKEND` — repository `NewQuilvianSystemBackend`, branch `Yasmina`
- WRITE TARGET: Source backend (`Areas/HealthServices/BillingManagement/MasterData/**`), test backend (`Tests/QuilvianSystemBackend.UnitTests.InMemory/**`), dan laporan task ini

## 1. Apa yang dikerjakan dan kenapa

**Tujuan bisnis.** Finance perlu mengelola sendiri daftar kategori pengeluaran kas kecil
(misalnya Transport, Konsumsi, Alat Tulis Kantor) tanpa harus meminta developer mengubah kode
setiap kali ada kategori baru atau kategori lama sudah tidak dipakai lagi.

**Yang sudah ada sebelumnya (dari `BE-BKC-033`).** Tabel `MstPettyCashCategory` beserta lima
baris data awal (`TRANSPORT`, `OPERASIONAL`, `KONSUMSI`, `MAINTENANCE`, `ATK`) sudah dirancang
dan migration-nya sudah dibuat. Tabel ini **belum** punya satu pun endpoint API untuk
dibaca, ditambah, diubah, atau dihapus lewat aplikasi — itulah yang dikerjakan task ini.

**Yang ditambahkan task ini.** Sembilan endpoint API baru mengikuti pola `TaxRulesController`
apa adanya (modul Tax Rule yang sudah lebih dulu ada di Billing Master Data), sehingga Finance
bisa:

1. Melihat daftar kategori dengan pencarian dan halaman.
2. Melihat detail satu kategori.
3. Menambah kategori baru.
4. Mengubah seluruh isian satu kategori.
5. Mengaktifkan atau menonaktifkan kategori tanpa mengetik ulang seluruh isian.
6. Menghapus kategori — tetapi **ditolak** bila kategori itu masih pernah dipakai voucher.
7. Mengambil daftar pendek untuk dropdown "Pilih Kategori" saat kasir/pemohon membuat voucher.
8. Melihat ringkasan jumlah kategori total, aktif, dan nonaktif.
9. Membaca metadata filter untuk membangun form pencarian di layar.

## 2. Proses bisnis

**Pelaku.** Finance (pengelola anggaran kas kecil) — satu-satunya peran yang berwenang membuat,
mengubah, dan menghapus kategori (lihat `contracts/permission-audit-matrix.md` § Resource
`PettyCashCategory`).

**Pemicu.** Finance membuka menu Master Data Kategori Petty Cash saat ada kebutuhan menambah
jenis pengeluaran baru, atau saat sebuah kategori lama sudah tidak relevan lagi.

**Prasyarat.** Tidak ada — kategori adalah data paling dasar dalam rumpun Petty Cash, tidak
bergantung data lain.

**Langkah utama — menambah kategori baru:**

1. Finance mengisi kode kategori (contoh `PARKIR`), nama kategori (contoh "Parkir Kendaraan
   Dinas"), dan deskripsi opsional.
2. Sistem mengubah kode menjadi huruf besar semua dan memangkas spasi di awal/akhir, lalu
   memeriksa apakah kode itu sudah dipakai kategori lain yang belum dihapus.
3. Bila kode sudah dipakai, sistem menolak dengan pesan "Kode kategori sudah dipakai kategori
   lain. Gunakan kode yang berbeda." dan kode status `409`.
4. Bila kode belum dipakai, kategori baru langsung tersimpan dan langsung bisa dipakai (tidak
   ada tahap approval — beda dengan diskon dokter yang butuh persetujuan berjenjang).

**Langkah utama — menghapus kategori:**

1. Finance memilih satu kategori dan menekan hapus.
2. Sistem memeriksa apakah kategori itu pernah dipakai pada voucher kas kecil mana pun, baik
   voucher yang masih berjalan maupun yang sudah selesai.
3. Bila pernah dipakai, penghapusan **ditolak** dengan pesan "Kategori ini tidak dapat dihapus
   karena sudah dipakai voucher. Nonaktifkan saja bila tidak dipakai lagi." dan kode status
   `400`. Ini sengaja dibuat begitu supaya riwayat pengeluaran lama tetap bisa ditelusuri —
   kategori yang datanya ikut dihapus akan membuat laporan lama kehilangan informasi.
4. Bila belum pernah dipakai, kategori ditandai terhapus (data tidak benar-benar hilang dari
   database, hanya disembunyikan dari daftar — pola yang sama dipakai `TaxRuleService`).

**Contoh berangka.** Kategori `MAINTENANCE` sudah pernah dipakai pada voucher
`PTC-20260907-0001` senilai Rp 50.000 untuk servis genset. Ketika Finance mencoba menghapus
`MAINTENANCE`, sistem menemukan baris voucher itu dan menolak penghapusan dengan kode `400`.
Finance kemudian memilih "Nonaktifkan" saja — kategori tetap ada di database dan riwayat
voucher tetap utuh, hanya saja kategori ini tidak lagi muncul di dropdown "Pilih Kategori" saat
pemohon membuat voucher baru.

**Perubahan status:**

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
| --- | --- | --- | --- | --- |
| Aktif | Nonaktifkan (`PATCH .../status`) | Nonaktif | Finance | Tidak ada — bisa kapan saja |
| Nonaktif | Aktifkan kembali (`PATCH .../status`) | Aktif | Finance | Tidak ada — bisa kapan saja |
| Aktif/Nonaktif | Hapus (`DELETE`) | Terhapus (tersembunyi) | Finance | Belum pernah dipakai voucher mana pun |

**Jalur tidak normal.** Kode kategori duplikat ditolak `409` saat membuat maupun mengubah.
Kategori yang tidak ditemukan (sudah terhapus atau ID salah) mengembalikan `404`. Kategori yang
masih dipakai voucher ditolak `400` saat dihapus — jalur keluarnya adalah nonaktivasi, bukan
penghapusan.

**Hasil akhir.** Finance memegang kendali penuh atas daftar kategori tanpa menyentuh kode
aplikasi, dan riwayat pengeluaran lama tidak pernah kehilangan kategorinya karena penghapusan
selalu ditolak selama masih dipakai.

## 3. Health Services / Billing Management / Master Data / Petty Cash Category

Base URL: `api/v1/health-services/billing-management/master-data/petty-cash-categories`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Mengambil pilihan ukuran halaman dan nilai filter bawaan untuk membangun form pencarian di layar | `PettyCashCategory : Read` | – | `ApiResponse<PettyCashCategoryFilterMetadataResponse>` |
| `GET` | `/summary` | Menampilkan jumlah kategori total, aktif, dan nonaktif | `PettyCashCategory : Read` | – | `ApiResponse<PettyCashCategorySummaryResponse>` |
| `GET` | `/` | Daftar kategori dengan pencarian, filter aktif/nonaktif, urutan, dan halaman | `PettyCashCategory : Read` | Query `search`, `isActive`, `sortBy`, `sortDirection`, `pageNumber`, `pageSize` | `ApiResponse<PagedResult<PettyCashCategoryResponse>>` |
| `GET` | `/options` | Isi dropdown "Pilih Kategori" pada modal buat voucher; bawaan hanya kategori aktif | `PettyCashCategory : Read` | Query `onlyActive` (bawaan `true`), `search` | `ApiResponse<List<PettyCashCategoryOptionResponse>>` |
| `GET` | `/{id}` | Detail satu kategori | `PettyCashCategory : Read` | Path `id` | `ApiResponse<PettyCashCategoryResponse>` |
| `POST` | `/` | Finance menambah kategori baru | `PettyCashCategory : Create` | `CreatePettyCashCategoryRequest` | `ApiResponse<PettyCashCategoryResponse>` |
| `PUT` | `/{id}` | Finance mengubah seluruh isian kategori | `PettyCashCategory : Update` | `UpdatePettyCashCategoryRequest` | `ApiResponse<PettyCashCategoryResponse>` |
| `PATCH` | `/{id}/status` | Mengaktifkan atau menonaktifkan kategori tanpa mengirim seluruh isian | `PettyCashCategory : Update` | `UpdatePettyCashCategoryStatusRequest` | `ApiResponse<PettyCashCategoryResponse>` |
| `DELETE` | `/{id}` | Menandai kategori terhapus; ditolak bila masih dipakai voucher mana pun | `PettyCashCategory : Delete` | Path `id` | `ApiResponse<PettyCashCategoryDeleteResponse>` |

**Kode status yang mungkin muncul:**

| Kode | Arti bagi pengguna |
| --- | --- |
| `200` | Permintaan berhasil |
| `400` | Kategori tidak dapat dihapus karena masih dipakai voucher (`BIL-VAL-053`) |
| `404` | Kategori dengan `id` tersebut tidak ditemukan |
| `409` | Kode kategori yang diisi sudah dipakai kategori lain (`BIL-VAL-056`) |
| `422` | Isian wajib (kode atau nama kategori) kosong |

Bentuk `CreatePettyCashCategoryRequest`: `categoryCode` (teks, wajib, maksimal 30 karakter,
otomatis diseragamkan huruf besar, unik), `categoryName` (teks, wajib, maksimal 100 karakter),
`description` (teks, opsional, maksimal 300 karakter), `isActive` (boolean, bawaan `true`).
`UpdatePettyCashCategoryRequest` berbentuk sama. `UpdatePettyCashCategoryStatusRequest` hanya
berisi `isActive` (boolean).

## 4. Berkas yang diperiksa

Governance dan kontrak: `AGENTS.md`, `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`,
`docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`, `rules/backend/TASK_RULES.md`,
`rules/backend/TASK_CLASSIFICATION.md`, `rules/backend/REVIEW_RULES.md`,
`rules/backend/REPORT_TEMPLATE.md`, `rules/rule-output/lokasi-laporan-task.md`,
`rules/rule-output/aturan-output-dokumentasi.md`.

Blueprint modul: `roadmap/backend-roadmap.md` (kartu `BE-BKC-035`), `02-backend-architecture.md`
(bagian `PettyCashCategoryService`/`PettyCashCategoriesController`),
`contracts/api-contract.md` (§ Petty Cash Category), `contracts/validation-matrix.md`
(`BIL-VAL-053`, `BIL-VAL-056`), `contracts/permission-audit-matrix.md` (Resource
`PettyCashCategory`), `roadmap/requirement-traceability.md`.

Pola implementasi terdekat yang dipakai apa adanya: `TaxRulesController.cs`,
`TaxRuleService.cs`, `TaxRuleDtos.cs` (referensi utama sesuai instruksi roadmap), serta pola
`PATCH {id}/status` dari `BankController.cs`/`BankDtos.cs` (`UpdateBankStatusRequest`) karena
Tax Rule sendiri masih memakai dua endpoint terpisah (`activate`/`deactivate`), sedangkan
kontrak Petty Cash Category secara eksplisit meminta satu endpoint `PATCH .../status`.

Model dan konteks data: `MstPettyCashCategory.cs`, `BilPettyCashVoucher.cs` (untuk kolom
`CategoryId` yang dipakai penjaga hapus), `Repositories/ApplicationDbContext.cs` (nama
`DbSet`), `Models/IdentityModel.cs` (kolom audit/soft-delete warisan).

## 5. Berkas yang diubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/BillingManagement/MasterData/DTOs/PettyCashCategoryDtos.cs` | **Baru.** Query, request, dan response DTO |
| `Areas/HealthServices/BillingManagement/MasterData/Services/PettyCashCategoryService.cs` | **Baru.** CRUD, metadata, ringkasan, opsi dropdown, aktivasi/nonaktivasi, penolakan hapus |
| `Areas/HealthServices/BillingManagement/MasterData/Controllers/PettyCashCategoriesController.cs` | **Baru.** Sembilan endpoint |
| `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` | **Diubah.** Satu baris `services.AddScoped<PettyCashCategoryService>();` ditambahkan |
| `Tests/QuilvianSystemBackend.UnitTests.InMemory/BillingManagement/PettyCashCategoryServiceTests.cs` | **Baru.** 13 unit test domain |

Tidak ada model, migration, atau tabel baru — task ini murni menambahkan lapisan API di atas
tabel `MstPettyCashCategory` yang sudah dibuat `BE-BKC-033`.

## 6. Implementasi

- `PettyCashCategoryService` membuka **tidak ada** transaction eksplisit (CRUD baris tunggal,
  sama seperti `TaxRuleService`) dan tidak pernah diakses langsung dari controller — controller
  hanya memanggil service, sesuai `QBE-SVC-001`.
- Kode kategori diseragamkan huruf besar (`ToUpperInvariant`) dan dipangkas spasi sebelum
  disimpan maupun dibandingkan keunikannya, mengikuti pola `TaxRuleService.Code`.
- Penolakan hapus memakai query `_dbContext.BilPettyCashVouchers.AnyAsync(x => x.CategoryId ==
  id, ...)` — sengaja **tanpa** filter status voucher, karena voucher yang sudah selesai maupun
  yang masih berjalan sama-sama membuat kategorinya tidak boleh dihapus (riwayat harus tetap
  bisa ditelusuri).
- Tiga exception baru: `PettyCashCategoryValidationException` (422 — isian wajib kosong),
  `PettyCashCategoryConflictException` (409 — kode duplikat), `PettyCashCategoryInUseException`
  (400 — masih dipakai voucher, khusus untuk aksi hapus).
- Permission `PettyCashCategory : Read/Create/Update/Delete` diwujudkan lewat atribut
  `[AccessPermission]` per action, memakai mekanisme discovery attribute yang sama seperti
  seluruh controller lain di repository ini — tidak ada baris seed manual yang perlu ditambah.

## 7. Backend Governance Preflight

| Aspek | Nilai |
| --- | --- |
| Area | HealthServices |
| Module | BillingManagement / Billing |
| Submodule | MasterData (kategori Petty Cash mengikuti prefix `Mst` sesuai baris registry `Master / Reference / MasterData`) |
| Prefix entity | `Mst` — tidak ada entity baru dibuat task ini; `MstPettyCashCategory` sudah ada dan `ACTIVE` sejak `BE-BKC-033` |
| Keberlakuan | `NEW CODE` — service, controller, DTO baru di atas model yang sudah disetujui |
| Status registry | `ACTIVE` (baris `Administrator / HealthServices` / `Master / Reference / MasterData`); tidak ada `QBE-MOD-002` yang menahan task ini karena tidak ada model persisted baru |
| QBE ID yang berlaku | `QBE-SVC-001` (controller dilarang akses `ApplicationDbContext` langsung) — dipatuhi; `QBE-CODE-003` (dilarang mekanisme penomoran/ID karangan) — tidak relevan, task ini tidak membuat nomor apa pun |

## 8. Dampak kontrak API

Mengimplementasikan sembilan endpoint yang sudah dikunci di `contracts/api-contract.md` §
`Petty Cash Category` (revisi kontrak `BIL-API-0.9`, status **DESIGN_APPROVED** menurut
`blueprint-manifest.md`). Tidak ada perubahan pada endpoint modul Billing lain.

## 9. Dampak database

Tidak ada. Tidak ada migration baru, tidak ada perubahan schema. Task ini membaca dan menulis
ke tabel `MstPettyCashCategory` yang sudah dibuat migration `20260907062238_AddTablePettyCashModule`
milik `BE-BKC-033`, dan hanya membaca (tanpa menulis) tabel `BilPettyCashVoucher` untuk
penjaga hapus.

## 10. Dampak keamanan

Resource baru `PettyCashCategory` dengan empat permission (`Read`, `Create`, `Update`,
`Delete`) — sesuai `contracts/permission-audit-matrix.md`. Tidak ada data sensitif pada
kategori (kode dan nama kategori bukan data pribadi/klinis), sehingga tidak ada kebutuhan
scrub log khusus untuk task ini.

- VISUAL REFERENCE: NOT REQUIRED (tidak ada perubahan UI pada task backend ini)

## 11. Validasi

| Perintah/pemeriksaan | Hasil | Klasifikasi | Bukti/catatan |
| --- | --- | --- | --- |
| `dotnet build` | **LULUS** (dilaporkan pengguna) | `PASS` | Pengguna menjalankan build secara manual di luar sesi (8 September 2026) dan melaporkan hasilnya berhasil. Sesi ini tidak menjalankan build sendiri sesuai instruksi baku pengguna; jumlah warning/error rinci tidak dikutip karena tidak dibagikan |
| `dotnet test --filter PettyCashCategoryServiceTests` | Tidak dijalankan/dilaporkan | `NOT RUN` | Konfirmasi pengguna menyebut "build", bukan `dotnet test` secara eksplisit — 13 unit test domain di `PettyCashCategoryServiceTests.cs` masih menunggu bukti eksekusi test yang sebenarnya |
| Review diff/scope | Dilakukan | `PASS` | `git status --short` menunjukkan hanya lima berkas yang tersentuh, seluruhnya berada dalam lingkup Petty Cash Category; tidak ada berkas modul lain yang berubah |
| Review kesesuaian QBE | Dilakukan | `PASS` | `QBE-SVC-001` dipatuhi (controller tidak menyentuh `ApplicationDbContext`); tidak ada model persisted baru sehingga `QBE-MOD-002`/`003` tidak berlaku |
| Pemeriksaan rahasia | Dilakukan | `PASS` | Tidak ada credential/token/connection string pada berkas yang berubah |

13 unit test domain ditulis pada `PettyCashCategoryServiceTests.cs`, mencakup: create dengan
normalisasi kode, kode duplikat saat create dan update (`409`), update baris yang
mempertahankan kodenya sendiri, kategori tidak ditemukan (`404`), aktivasi/nonaktivasi lewat
`UpdateStatusAsync`, hapus kategori yang belum dipakai (berhasil), hapus kategori yang sudah
dipakai voucher (ditolak, `PettyCashCategoryInUseException`), filter dan pencarian daftar
berhalaman, opsi dropdown bawaan hanya aktif, ringkasan jumlah, dan resolusi dependency injection
`PettyCashCategoryService` lewat `AddBillingManagement()`. **Belum ada satu pun yang benar-benar
dieksekusi** — status di atas menunggu pengguna menjalankan `dotnet test` secara manual.

- MANUAL TEST: NOT APPLICABLE (task backend murni, tidak ada UI untuk diuji manual)

## 12. Peringatan dan risiko yang tersisa

- `dotnet build` sudah dikonfirmasi lulus oleh pengguna (8 September 2026). **`dotnet test`
  belum dikonfirmasi** — Task ini **belum boleh ditandai selesai** sampai 13 unit test domain
  pada `PettyCashCategoryServiceTests.cs` benar-benar dieksekusi dan hasilnya dilaporkan.
- `BE-BKC-033` (tabel `MstPettyCashCategory`) masih berstatus 🟡 pada roadmap — bukan karena
  tabelnya tidak ada (migration `20260907062238_AddTablePettyCashModule` sudah memuatnya),
  melainkan karena bukti baris seed di database dan review Finance atas lima kategori seed
  belum ada. Ini tidak menghalangi source `BE-BKC-035`, tetapi wajib tertutup sebelum
  gelombang `MVP-13` dinyatakan naik.
- Task ini **tidak** menyentuh `BE-BKC-034` (penomoran voucher) maupun `BE-BKC-036`/`037`
  (anggaran dan siklus hidup voucher) — sepenuhnya independen sesuai desain roadmap.

## 13. Perubahan sampingan

- INCIDENTAL CHANGES: NONE — hanya lima berkas dalam lingkup task yang berubah, sesuai
  `git status --short` di bawah.

## 14. Interupsi

- INTERRUPTIONS: NONE — task dikerjakan dalam satu sesi berkelanjutan tanpa interupsi.

## 15. Status Git

```
 M Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs
?? Areas/HealthServices/BillingManagement/MasterData/Controllers/PettyCashCategoriesController.cs
?? Areas/HealthServices/BillingManagement/MasterData/DTOs/PettyCashCategoryDtos.cs
?? Areas/HealthServices/BillingManagement/MasterData/Services/PettyCashCategoryService.cs
?? Tests/QuilvianSystemBackend.UnitTests.InMemory/BillingManagement/PettyCashCategoryServiceTests.cs
```

Belum di-stage maupun di-commit. Branch `Yasmina`, HEAD `07be199ac958728388a14446fa0ef9ac8e72219b`
sebelum perubahan task ini.

## 16. Langkah berikutnya yang disarankan

1. Pengguna menjalankan `dotnet build` dan `dotnet test` secara manual, lalu melaporkan hasil
   sebenarnya (jumlah lulus/gagal) agar bisa dicatat di laporan ini dan roadmap.
2. Setelah build/test terbukti lulus, perbarui tabel status pada `roadmap/backend-roadmap.md`
   (kartu `BE-BKC-035` dan baris ringkasan gelombang `MVP-13`) serta baris terkait
   `PC-DES-002`/`FR-BKC-061`–`063` pada `roadmap/requirement-traceability.md`, menautkannya ke
   laporan ini.
3. Lanjutkan ke `BE-BKC-036` (kolam anggaran dan saldo berjalan) atau selesaikan verifikasi
   `BE-BKC-033`/`034` yang masih tertunda, sesuai prioritas pengguna.

- KNOWN ISSUES: Tidak ada yang ditemukan pada scope task ini di luar butir Peringatan/risiko
  di atas.
- STALE EVIDENCE / BLOCKED PHASES: NOT APPLICABLE (bukan `MODULE BLUEPRINT MODE`)
- BLUEPRINT STATUS/EVIDENCE: NOT APPLICABLE (bukan `MODULE BLUEPRINT MODE`)
