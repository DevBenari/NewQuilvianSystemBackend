# Laporan Perubahan Backend — `BE-FIN-143`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-143` |
| Judul | Kiriman BLM, pemetaan supplier, konflik, dan pembatalan eksternal punya tabel sendiri |
| Slice | `MVP-19` — Penampung Data PO/GR dari BLM (`EPIC FIN-26`) |
| Roadmap | `roadmap/01-backend-roadmap.md`, bagian "AMENDMENT ROADMAP REV-24" |
| Trace | `FR-FIN-276`, `277`, `279`, `280` (dan sisi jenis konflik dari `FR-FIN-278`); `FIN-DEC-261`..`264`; `FIN-DES-132`, `133`, `136`, `137`; `FIN-VAL-308`..`310`, `312`, `313` |
| Contract version | `data/data-dictionary.md` bagian 13.6, 13.7, 13.9, 13.10 (Revisi 23, approved 2026-11-09); `FIN-VAL-1.15` bagian W; `FIN-STATE-1.12` bagian W.1–W.3; tidak ada endpoint |
| Dependency | Tidak ada (gelombang 1) |
| Klasifikasi | `MEDIUM` — skor 6: repository 0, berkas diperiksa 1, berkas diubah 2 (9), logika bisnis 1 (deklaratif), kontrak API 0, database 2 (empat tabel baru), keamanan 0, UI 0 |
| Task mode | `BACKEND MODE` |
| Target tulis | `NewQuilvianSystemBackend`, branch `Yasmina`. Frontend tidak disentuh |
| Model | Claude Sonnet 5.5 |
| Commit backend saat dikerjakan | `98a9140c` (working tree memuat perubahan `BE-FIN-142` yang belum di-commit; tidak ditimpa) |
| Tanggal | 10 November 2026 |
| Status | ✅ **Selesai (10 November 2026).** Source selesai; `dotnet build` berhasil menurut laporan pemilik (keluaran build Release kemudian dilampirkan pemilik (10 November 2026): `0 Error(s)`, `12 Warning(s)` (seluruhnya peringatan paket NU1902/NU1903 `SixLabors.ImageSharp` 3.1.11; tidak ada di folder Purchasing), `Build succeeded`). Migration sudah diterapkan (laporan pemilik) dan uji tulis W.T17 lulus 12 dari 12 skenario (rincian di laporan `BE-FIN-144`, bagian 5.1) |

## Backend Governance Preflight

| Butir | Hasil |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `FinanceManagement` / `Purchasing` |
| Pemilik dan prefix (registry) | `Fin`, baris "Corporate / Finance — FinanceManagement / Purchasing / Pembelian", lifecycle `ACTIVE` pada `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Keberlakuan | `NEW CODE` untuk empat entity baru; `TOUCHED LEGACY` untuk `Repositories/ApplicationDbContext.cs` |
| QBE yang berlaku | `QBE-MOD-001`, `QBE-MOD-002`, `QBE-MOD-003` (folder `Purchasing` sudah terdaftar `ACTIVE`, jadi entity pertama boleh dibuat), `QBE-NAM-001` (tanpa `Trx*`), `QBE-NAM-002` (prefix `Fin` dari registry), `QBE-NAM-004` (tidak ada prefix baru). `QBE-CODE-*` tidak berlaku: task ini tidak membuat nomor bisnis. `QBE-DB-001`/`002` tidak berlaku (bukan `LEGACY MIGRATION`) |
| Nama sepaket | Entity = file = `…Configuration` = `DbSet` jamak = tabel `public."Fin…"` (tabel tunggal PascalCase) |
| Branch | `Yasmina` |
| Selisih governance | Sama dengan `BE-FIN-142`: salinan registry di suite skill lebih ringkas daripada salinan repository; salinan repository yang dipakai. Folder `agents/rules/` peninggalan tidak ditemukan |
| Working tree sebelum task | 11 berkas berubah milik `BE-FIN-142` (8 source + 2 roadmap + 1 laporan); tidak disentuh |

---

## 1. Masalah yang Diperbaiki

BLM akan mengirim PO dan GR ke Quilvian (`FIN-DEC-261`), tetapi Quilvian belum punya tempat yang aman untuk menampungnya:

1. **Kiriman yang belum bisa diterapkan akan hilang.** Contoh: BLM mengirim GR untuk supplier yang belum dipetakan. Tanpa kotak masuk, kiriman itu ditolak dan tidak ada yang tahu perlu dicoba lagi.
2. **Kiriman ulang membuat dokumen ganda.** Tanpa nomor kejadian yang unik, kiriman yang sama dua kali menjadi dua GR.
3. **Supplier BLM tidak dapat dikaitkan ke `MstSupplier`.** GUID supplier kedua sistem tidak sama (`FIN-DEC-263`), dan `MstSupplier` milik Administrator tidak boleh diberi kolom BLM.
4. **Benturan dengan AP tidak tercatat.** Bila BLM membatalkan GR yang sudah menjadi dasar utang, utang tidak boleh dibalik otomatis (`FIN-DEC-264`); Finance perlu daftar yang bisa ditinjau.
5. **Pembatalan lintas sistem tidak berjejak.** Tidak ada tempat mencatat siapa meminta batal, apa alasannya, dan apakah BLM sudah mengonfirmasi.

Task ini menyiapkan **tabelnya saja**. Pengolah kiriman, endpoint, dan layar menyusul di slice integrasi.

---

## 2. Proses Bisnis

Tidak ada proses bisnis yang berubah pada task ini (tabel belum dipakai kode mana pun). Aturan yang ditanam di database:

| Tabel | Aturan | Cara ditegakkan | Contoh |
| --- | --- | --- | --- |
| Kotak masuk | Satu nomor kejadian BLM hanya diterima sekali | Unik (`SourceSystem`, `EventId`), **tanpa** filter hapus | Kiriman `EVT-001` dikirim dua kali → baris kedua ditolak database |
| Kotak masuk | `APPLIED` hanya sah bila tepat satu sasaran terisi sesuai jenis dokumen | `CK_…_Applied` | Kiriman GR `APPLIED` tanpa `AppliedGoodsReceiptId` ditolak |
| Kotak masuk | `EventType` bebas | Tanpa check (kontrak BLM belum pasti, `FIN-OQ-141`) | Kode kejadian baru dari BLM tidak memecah penyimpanan |
| Pemetaan supplier | Satu pemetaan **aktif** per supplier BLM | Unik terfilter `IsActive = true AND IsDelete = false` | Pemetaan lama dinonaktifkan lalu yang baru dibuat; jejak lama tetap |
| Konflik | Paling banyak satu konflik **terbuka** per (kiriman, jenis) | Unik terfilter `Status = 'OPEN'` | Kiriman GR yang sama tidak menghasilkan dua konflik `CONVERSION_UNRESOLVED` terbuka |
| Konflik | `RESOLVED` wajib berisi kode, pelaku, dan waktu | `CK_…_Resolved` | Penutupan tanpa kode penyelesaian ditolak |
| Pembatalan | Permintaan dari Quilvian wajib punya pemohon; `CONFIRMED` wajib punya waktu batal; nomor kejadian unik per (sumber, arah) | `CK_…_Requester`, `CK_…_Confirmed`, unik | Permintaan batal tanpa `RequestedByUserId` ditolak |

**Jalur tidak normal.** Tabel pemetaan supplier sengaja kosong saat rilis (`FIN-OQ-144`); tidak ada fitur yang berhenti karena itu karena belum ada pengolah.

---

## 3. Perubahan yang Dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`; `docs/engineering/` (kontrak dan registry); `rules/backend/TEST_POLICY.md`, `TASK_CLASSIFICATION.md`, `REPORT_TEMPLATE.md`; kamus data 13.6, 13.7, 13.9, 13.10; `02-backend-architecture.md` W.3; `FinAccountingEventOutbox` dan `FinAccountingEventOutboxConfiguration` (pola kotak keluar); `FinPurchasingIdempotencyRecordConfiguration` (pola check status); `Repositories/ApplicationDbContext.cs` (letak `DbSet`, pemakaian `ApplyConfigurationsFromAssembly`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/Purchasing/Models/FinPurchasingInboundMessage.cs` | **Baru.** Entity kotak masuk (20 kolom) + `FinPurchasingDocumentTypes`, `FinPurchasingInboundProcessingStatuses` |
| `Areas/Corporate/FinanceManagement/Purchasing/Models/FinPurchasingSupplierMap.cs` | **Baru.** Pemetaan supplier BLM → `MstSupplier` (7 kolom) |
| `Areas/Corporate/FinanceManagement/Purchasing/Models/FinPurchasingSyncConflict.cs` | **Baru.** Konflik sinkronisasi (13 kolom) + `…ConflictTypes`, `…ConflictStatuses`, `…ConflictResolutionCodes`, `…ConflictDependentTypes` |
| `Areas/Corporate/FinanceManagement/Purchasing/Models/FinPurchasingExternalCancellation.cs` | **Baru.** Audit dan antrean pembatalan eksternal (21 kolom) + `FinPurchasingCancellationDirections`, `FinPurchasingCancellationStatuses` |
| `Repositories/Configurations/Corporate/FinanceManagement/Purchasing/FinPurchasingInboundMessageConfiguration.cs` | **Baru.** 6 check, 2 FK `Restrict`, 5 index (1 unik) |
| `Repositories/Configurations/Corporate/FinanceManagement/Purchasing/FinPurchasingSupplierMapConfiguration.cs` | **Baru.** 1 check, FK `Restrict` ke `MstSupplier`, 1 unik terfilter, 1 index |
| `Repositories/Configurations/Corporate/FinanceManagement/Purchasing/FinPurchasingSyncConflictConfiguration.cs` | **Baru.** 7 check, 3 FK `Restrict`, 1 unik terfilter, 3 index |
| `Repositories/Configurations/Corporate/FinanceManagement/Purchasing/FinPurchasingExternalCancellationConfiguration.cs` | **Baru.** 8 check, 3 FK `Restrict`, 1 unik, 4 index |
| `Repositories/ApplicationDbContext.cs` | **Diperbarui.** + 4 `DbSet` jamak. Konfigurasi terdaftar otomatis lewat `ApplyConfigurationsFromAssembly` |

Total: 8 berkas baru, 1 berkas diperbarui (+6 baris). Tidak ada service, controller, DTO, migration, seed, atau konfigurasi aplikasi yang berubah.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — tidak ada endpoint. Tidak ada tabel pemetaan produk maupun satuan (`FIN-DEC-274`, `276`) |
| Database | Empat tabel baru. Semua FK `Restrict`, tidak ada cascade. Migration dibuat pada `BE-FIN-144` dan, menurut laporan pemilik (10 November 2026), sudah diterapkan. `DbSet` baru tidak mengubah query yang sudah ada selama tabelnya belum dipakai; tetapi migration berikutnya yang dibangkitkan (`BE-FIN-144`) akan memuat tabel ini. Kode tidak boleh dirilis sebelum migration diterapkan (`FIN-DES-138`) |
| Keamanan/Auth | Tidak ada endpoint, jadi tidak ada izin. Dua kolom ditandai **sensitif** pada komentar dan kamus data: `FinPurchasingInboundMessage.PayloadJson` dan `FinPurchasingExternalCancellation.CancelledBy` (identitas pegawai BLM) — **MUST NOT** masuk custom logger |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — task ini tidak menyentuh endpoint.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet restore` / `dotnet build` | Berhasil | `PASS` (laporan pemilik) | Pemilik, 10 November 2026; keluaran perintah dilampirkan kemudian oleh pemilik (build Release: `0 Error(s)`, `12 Warning(s)` (seluruhnya peringatan paket NU1902/NU1903 `SixLabors.ImageSharp` 3.1.11; tidak ada di folder Purchasing), `Build succeeded`) |
| QBE preflight / conformance | Prefix `Fin` terdaftar `ACTIVE` untuk `FinanceManagement / Purchasing`; tidak ada `Trx*`; nama sepaket; tidak ada generator nomor; tidak ada `DbContext` di controller | `PASS` | Bacaan registry dan diff |
| Review diff / scope | 9 berkas dalam cakupan; tidak menyentuh milik `BE-FIN-142` | `PASS` | `git status --short` |
| Kesesuaian kolom dengan kamus data (nama) | 20 + 7 + 13 + 21 = 61 kolom kamus seluruhnya ada di model; properti tambahan hanya navigasi | `PASS` | Skrip pencocokan statis |
| Kesesuaian panjang kolom `varchar` dengan kamus data | Sama | `PASS` | Skrip pencocokan statis |
| Keseimbangan kurung (9 berkas) | Seimbang | `PASS` | Skrip statis (bukan kompilasi) |
| Nama tipe unik di assembly (12 tipe baru) | Masing-masing tepat satu definisi | `PASS` | `grep` |
| Kompilasi C# | Berhasil | `PASS` (laporan pemilik) | Pemilik, 10 November 2026; keluaran perintah dilampirkan kemudian oleh pemilik (build Release: `0 Error(s)`, `12 Warning(s)` (seluruhnya peringatan paket NU1902/NU1903 `SixLabors.ImageSharp` 3.1.11; tidak ada di folder Purchasing), `Build succeeded`) |
| Penerapan constraint/index pada PostgreSQL (W.T17) | Lulus 12 dari 12 skenario (kotak masuk, konflik, pembatalan, pemetaan supplier) | `PASS` (laporan pemilik) | Pemilik, 10 November 2026 |

Uji manual: `NOT APPLICABLE` (tidak ada perilaku runtime).

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

**Tidak dijalankan oleh agen (build dijalankan pemilik, lihat laporan pemilik di tabel):** `dotnet restore`/`dotnet build` (instruksi pemilik), `dotnet ef migrations add`, `dotnet ef database update` (wewenang terpisah, `FIN-DEC-273`), SQL pada database uji.

---

## 6. Acceptance Criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Nama paket konsisten (model, `…Configuration`, `DbSet` jamak, tabel `public."Fin…"`) | Terpenuhi | Daftar berkas 3.2; `DbSet` di `ApplicationDbContext.cs` |
| 2. FK `Restrict` persis kamus data | Terpenuhi | Kotak masuk → PO/GR; pemetaan → `MstSupplier`; konflik → kotak masuk/PO/GR; pembatalan → PO/GR/kotak masuk; semuanya `DeleteBehavior.Restrict` |
| 3. Unik (`SourceSystem`, `EventId`), unik terfilter pemetaan aktif, unik konflik terbuka | Terpenuhi | `IX_FinPurchasingInboundMessage_SourceSystem_EventId`, `IX_FinPurchasingSupplierMap_SourceSystem_ExternalSupplierId`, `IX_FinPurchasingSyncConflict_OpenPerMessageType` (plus unik pembatalan) |
| 4. Semua check status dan jenis termasuk `CONVERSION_UNRESOLVED` | Terpenuhi | `CK_FinPurchasingSyncConflict_ConflictType` memuat lima jenis; check status inbox, konflik, dan pembatalan |
| 5. `EventType` tanpa check | Terpenuhi | Tidak ada check pada `EventType` di `FinPurchasingInboundMessageConfiguration.cs` |
| 6. Tidak ada tabel pemetaan produk maupun satuan | Terpenuhi | Tidak ada entity itu di diff |
| DoD: laporan tracked | Terpenuhi | Berkas ini |
| DoD: `dotnet build` | Terpenuhi (laporan pemilik) | Pemilik, 10 November 2026; keluaran perintah dilampirkan kemudian oleh pemilik (build Release: `0 Error(s)`, `12 Warning(s)` (seluruhnya peringatan paket NU1902/NU1903 `SixLabors.ImageSharp` 3.1.11; tidak ada di folder Purchasing), `Build succeeded`) |
| DoD: bukti penerapan constraint di database | Bukan DoD task ini | Roadmap menempatkan W.T14–W.T18 pada `BE-FIN-144`; sebelumnya tertulis di sini sebagai kehati-hatian tambahan |

Status akhir **✅**: kriteria 1–6 terbukti dari diff, kompilasi lulus (laporan pemilik), dan laporan tracked ada, sesuai DoD roadmap. Bukti constraint di PostgreSQL ada pada `BE-FIN-144`.

---

## 7. Catatan Penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Sama dengan `BE-FIN-142`: jangan rilis atau gabung ke branch rilis sebelum `BE-FIN-144` diterapkan (`FIN-DES-138`) |
| Keputusan implementasi | (1) Unik idempotensi kotak masuk dan pembatalan **tanpa** filter `IsDelete`, lebih ketat daripada pola `FinAccountingEventOutbox`: nomor kejadian yang pernah diterima tidak boleh masuk lagi walau barisnya kelak ditandai hapus. Kamus data menulis "Unik" tanpa filter, jadi ini sesuai. (2) `Status`, `Direction`, dan `DocumentType` pada pembatalan tidak punya nilai bawaan (kamus data: bawaan "—"); pembuat baris wajib mengisinya |
| Masalah yang diketahui | (1) `CK_FinPurchasingExternalCancellation_Target` mewajibkan FK lokal PO/GR sesuai jenis dokumen (kamus data 13.10). Akibatnya pemberitahuan batal dari BLM atas dokumen yang belum pernah diterima Quilvian tidak dapat dicatat di tabel ini; pengolah nanti perlu menandainya `REJECTED` di kotak masuk. Perlu dikonfirmasi saat slice integrasi. (2) Panjang `CategoryCode` (50), `CategoryName` (100), `OtherCategoryText` (300), `Reason` (1000), dan `CancelledBy` (100) adalah asumsi sampai kontrak BLM diterima (`FIN-OQ-141`) |
| Risiko tersisa | Kompilasi dan constraint/index di PostgreSQL lulus (laporan pemilik) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M` `ApplicationDbContext.cs`; `??` delapan berkas baru; ditambah berkas `BE-FIN-142` yang belum di-commit. Tidak ada berkas staged |
| Langkah berikutnya | `BE-FIN-144`: penerapan migration (mencakup `BE-FIN-142` dan `143`) ke database uji, lalu W.T17 |
