# Laporan Perubahan Backend — `BE-PBJ-PS-001`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-PBJ-PS-001` |
| Judul | Fondasi Sourcing |
| Slice | `MVP-PS0` — fondasi, master pemilihan, kolom baris PR, skema (`04-prd-to-mvp.md` bagian 20) |
| Roadmap | `pemilihan-penyedia/roadmap/backend-roadmap.md` revisi 1, kartu `BE-PBJ-PS-001` |
| Trace | `PBJ-DEC-068`, `069`, `072`, `086`, `096`; `PBJ-ASM-007`, `008`; FR-PS-003; `AT-PS-60`, `61`, `62` (master), `64` |
| Contract version | Kontrak `PBJ-PS-*` set `1.0` (`approved`, `PBJ-DEC-095`); kamus data 1.0 bagian 2; arsitektur backend bagian 5, 8–10 |
| Dependency | — (gelombang 1) |
| Klasifikasi | `MEDIUM` — skor 8: berkas diperiksa > 20 (2), berkas diubah > 8 (2), logika sedang (1), database/migration (2), keamanan berkaitan lewat konstanta izin (1) |
| Task mode | `BACKEND` (pemilik menjawab "lanjutkan" atas tawaran `build-module-backend` untuk `BE-PBJ-PS-001`, 26 November 2026) |
| Wewenang database | Tiga konfirmasi terpisah dari pemilik, 26 November 2026: (1) membuat migration; (2) menerapkannya ke `QuilvianNewDevSukma`; (3) menjalankan seeder dua kali di `https://localhost:7185` serta `SELECT` baca-saja dan uji batasan di dalam transaksi yang di-rollback |
| Target tulis | Tiga model `MasterData`, tiga konfigurasi EF, `ApplicationDbContext.cs`, enam konstanta `Common/Constants`, dua seeder, `Program.cs`, migration `AddProcurementSourcingMasterData` |
| Model | Claude Opus 5.5 |
| Branch / commit | Backend `sukmagp` @ `45b22777` (dokumen approval dan roadmap pemilihan-penyedia belum di-commit), frontend `3e5386679` |
| Tanggal | 26 November 2026 |
| Status | ✅ **SELESAI.** Tiga tabel master terpasang di `QuilvianNewDevSukma` dan cocok dengan kamus data; 16 uji batasan `PASS`; seeder dua kali tanpa baris berlipat; QBE Strict `PASS`; model tanpa perubahan tertunda |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `ProcurementManagement` / `MasterData` (tiga master pemilihan). Folder `Sourcing` belum disentuh task ini |
| Registry | Master `MstProcurement*` di baris `MasterData` (`Mst`) yang sudah `ACTIVE`; submodule `ProcurementManagement / Sourcing · Prc · ACTIVE` sudah terdaftar 2026-11-26 (`PBJ-DEC-096`) untuk task berikutnya |
| Keberlakuan | `NEW CODE` untuk tiga model, tiga konfigurasi, `ProcurementSourcingMasterSeeder`, migration; `TOUCHED LEGACY` untuk `ApplicationDbContext.cs`, `Program.cs`, enam konstanta, dan `ProcurementWorkflowDefinitionSeeder` |
| Arketipe endpoint | Tidak ada endpoint (di luar cakupan task) |
| QBE yang berlaku | `QBE-ENT-001`, `QBE-NAM-002` (`Mst`), `QBE-CFG-001`, `QBE-MOD-001..003`, `QBE-CODE-004`, `QBE-DEL-001` (hapus = `IsDelete`, FK `Restrict`) |
| Hasil checker | QBE Strict `PASS` (19 berkas, 0 temuan) |

## 1. Masalah yang diselesaikan

Pemilihan penyedia belum punya tempat menyimpan aturan dasarnya. Sebelum task ini tidak ada daftar metode pengadaan
beserta batas HPS-nya, jenis dokumen paket (KAK/TOR, Dasar HPS, Surat Penawaran Harga), dan dokumen wajib per metode atau
jenis pengadaan. Akibatnya, paket tidak dapat diberi saran metode, dan checklist persiapan tidak dapat disusun. Task ini
membangun ketiga master itu, nama-nama tetap (konstanta) yang dipakai seluruh task pemilihan-penyedia, data awal contoh,
dan kerangka dua alur persetujuan.

## 2. Proses bisnis yang dilayani fondasi ini

| Langkah bisnis | Yang disiapkan task ini | Contoh |
| --- | --- | --- |
| Admin menetapkan metode pengadaan dari SOP | `MstProcurementMethod` + dua metode **contoh** bertanda `IsSample` | Penunjukan Langsung (contoh): HPS sampai Rp 50.000.000, 1 penawaran, tanpa evaluasi teknis |
| Tim Pengadaan menyusun HPS, sistem menyarankan metode | Rentang HPS per metode: `MinHpsAmount < HPS ≤ MaxHpsAmount` | HPS Rp 42.750.000 → Penunjukan Langsung (contoh); tepat Rp 50.000.000 juga Penunjukan Langsung |
| Tim Pengadaan melengkapi checklist dokumen persiapan | `MstProcurementPackageDocumentType`, `MstProcurementPackageDocumentRequirement` | Paket Jasa wajib KAK/TOR; semua metode contoh wajib Dasar HPS |
| Evaluator tidak boleh melihat berkas harga | Penanda `IsPriceSensitive` pada jenis dokumen | Dasar HPS, Surat Penawaran Harga, Berita Acara Negosiasi tertutup harga |
| Persiapan dan penetapan disetujui berjenjang | Definisi `Wfl` `PBJ-PKT-PERSIAPAN` dan `PBJ-PKT-PENETAPAN`, **tidak aktif** | Langkah tambahan dibuat bila metode di luar rentang (`PBJ_METHOD_OUT_OF_RANGE`) atau nilai penetapan di atas HPS (`PBJ_OVER_HPS`) |

Jalur tidak normal yang sudah dijaga di tingkat basis data: batas HPS negatif atau terbalik, minimum penawaran di luar
1–10, tahap dokumen di luar tiga nilai, kode ganda, dokumen wajib yang mengisi metode **dan** jenis sekaligus (atau tidak
satu pun), serta penghapusan fisik metode yang masih dipakai dokumen wajib.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`MstProcurementVendorDocumentType`, `MstProcurementCategoryDocumentRequirement`, `MstProcurementEvaluationCriterion`,
`MstProcurementType` beserta konfigurasinya; `ProcurementVendorMasterSeeder`, `ProcurementMasterDataSeeder` (kode jenis
`SERVICE`/`ASSET`), `ProcurementWorkflowDefinitionSeeder`; `MstWorkflowDefinition`, `MstWorkflowStep` (panjang kode);
enam konstanta `Common/Constants`; `ProcurementAttachmentService` (perilaku jenis dokumen tanpa penyedia akses),
`ProcurementApprovalGateway`, `ProcurementDocumentHistoryWriter` (pemakai `ProcurementDocumentTypes.IsKnown`);
`ApplicationDbContext.cs`, `ApplicationDbContextFactory.cs`, `Program.cs` (urutan seeder); kamus data, arsitektur, dan
matriks hak akses pemilihan-penyedia; laporan `BE-PBJ-VD-001` (pola).

### 3.2 Berkas yang berubah

| Berkas | Status | Isi |
| --- | --- | --- |
| `Areas/Corporate/ProcurementManagement/MasterData/Models/MstProcurementMethod.cs` | Baru | Metode, rentang HPS, minimum penawaran, wajib evaluasi teknis, `IsSample` |
| `.../MasterData/Models/MstProcurementPackageDocumentType.cs` | Baru | Jenis dokumen paket, tahap, `IsPriceSensitive` |
| `.../MasterData/Models/MstProcurementPackageDocumentRequirement.cs` | Baru | Dokumen wajib per metode atau per jenis pengadaan |
| `Repositories/Configurations/Corporate/ProcurementManagement/MasterData/` (3 berkas) | Baru | Konfigurasi EF, 6 CHECK, 3 FK `Restrict`, index unik parsial |
| `.../MasterData/Seeders/ProcurementSourcingMasterSeeder.cs` | Baru | 2 metode contoh, 6 jenis dokumen, 4 dokumen wajib |
| `.../Common/Seeders/ProcurementWorkflowDefinitionSeeder.cs` | Diperbarui | Definisi `PBJ-PKT-PERSIAPAN` dan `PBJ-PKT-PENETAPAN`, masing-masing satu langkah biasa + satu langkah bersyarat |
| `.../Common/Constants/ProcurementDocumentTypes.cs` | Diperbarui | `PBJ_PACKAGE`, `PBJ_RFQ`, `PBJ_PACKAGE_AWARD` (+ himpunan `Known`) |
| `.../Common/Constants/ProcurementNumberSeries.cs` | Diperbarui | Deret `PBJ_PACKAGE`/`PKT`, `PBJ_RFQ`/`RFQ` |
| `.../Common/Constants/ProcurementWorkflowCodes.cs` | Diperbarui | Dua kode definisi, kondisi `PBJ_METHOD_OUT_OF_RANGE` dan `PBJ_OVER_HPS` |
| `.../Common/Constants/ProcurementPermissions.cs` | Diperbarui | Kode modul `CORPORATE_PROCUREMENT_MANAGEMENT_SOURCING`, 8 aksi (`RecordOrder`, `ReadSummary`, `Send`, `Close`, `Negotiate`, `Evaluate`, `Revoke`, `ReadPrice`), 8 resource (`PBJ-PS-PERM` §2) |
| `.../Common/Constants/ProcurementStatuses.cs` | Diperbarui | Status `Package`, `Rfq`, `Quotation`, `Award`, hasil evaluasi `PASS`/`FAIL`, tahap dokumen paket |
| `.../Common/Constants/ProcurementDocumentHistoryActions.cs` | Diperbarui | 15 kode aksi riwayat paket, RFQ, dan penetapan (`PBJ-PS-PERM` §5) |
| `Repositories/ApplicationDbContext.cs` | Diperbarui | Tiga `DbSet` |
| `Program.cs` | Diperbarui | Pemanggilan `ProcurementSourcingMasterSeeder` sesudah seeder master vendor; komentar seeder workflow menjadi sembilan definisi |
| `Migrations/20261126043027_AddProcurementSourcingMasterData.cs` + `.Designer.cs` | Baru | Tiga `CreateTable`, enam index, enam CHECK; `Down` menghapus tiga tabel |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Diperbarui | Hasil generator EF, termasuk pemulihan entri yang hilang (lihat D-9) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — tidak ada endpoint baru atau berubah |
| Database | Tiga tabel baru di schema `public`; tidak ada tabel lama yang berubah. Migration dapat dijalankan tanpa mematikan layanan karena hanya menambah tabel. Langkah mundur: `Down` menghapus ketiga tabel dalam urutan aman terhadap FK (belum ada data transaksi). **Diterapkan** ke `QuilvianNewDevSukma` 26 November 2026; lingkungan lain belum |
| Keamanan/Auth | Konstanta resource dan aksi disiapkan supaya argumen `[AccessController]`, `[AccessAction]`, dan `[AccessPermission]` task berikutnya tidak berselisih satu huruf. Belum ada butir di layar Akses Role karena belum ada controller. Tiga jenis dokumen baru dikenal `IsKnown`, tetapi belum punya penyedia akses lampiran, sehingga permintaan lampiran untuk jenisnya dijawab 404 (fail-closed) sampai `BE-PBJ-PS-006`. Kedua definisi persetujuan tidak aktif dan tanpa matriks, sehingga pengajuan nanti ditolak `PBJ-VAL-091` sampai admin mengisi penyetuju dari SOP |

**Selisih terhadap desain (delta), dicatat apa adanya.**

| # | Desain | Yang dibangun | Alasan |
| ---: | --- | --- | --- |
| D-1 | Kamus data: kolom `SortOrder int` pada `MstProcurementMethod` | **Tidak dibuat.** Urutan tampil = batas bawah HPS, lalu nama | Kontrak engineering melarang `SortOrder` presentasi tersimpan untuk kode baru; urutan bisnis memakai field semantik (preseden `MstProcurementEvaluationCriterion`). Kamus data perlu diselaraskan pemilik desain |
| D-2 | Arsitektur §10: "Penunjukan Langsung ≤ Rp 50.000.000", "Pembandingan > Rp 50.000.000"; kamus data tidak menyebut sisi inklusif | Rentang ditetapkan `MinHpsAmount < HPS ≤ MaxHpsAmount` (bawah eksklusif, atas inklusif) dan didokumentasikan di model | Satu-satunya tafsiran yang membuat kedua contoh tidak tumpang tindih di Rp 50.000.000. `BE-PBJ-PS-002` (cek tumpang tindih) dan `BE-PBJ-PS-007` (saran metode) wajib memakai tafsiran yang sama |
| D-3 | Kamus data: unik (`DocumentTypeId`, `MethodId`, `ProcurementTypeId`) | Dua index unik parsial: per metode (`MethodId IS NOT NULL`) dan per jenis (`ProcurementTypeId IS NOT NULL`) | PostgreSQL menganggap dua NULL berbeda, sehingga satu index tiga kolom tidak mencegah duplikat. Nama index ditulis eksplisit ≤ 63 karakter (D-10) |
| D-4 | Kartu roadmap: "tiga dokumen wajib" | Empat baris: KAK/TOR × Jasa, Spesifikasi Teknis × Aset, Dasar HPS × kedua metode contoh | Arsitektur §10 mewajibkan Dasar HPS untuk **semua** metode contoh, yaitu dua baris. Kartu roadmap menghitung aturannya, bukan barisnya |
| D-5 | Arsitektur §10: `PBJ-PKT-PERSIAPAN` "1 langkah" | Satu langkah biasa + satu langkah bersyarat `PBJ_METHOD_OUT_OF_RANGE` | Arsitektur §8 dan `AT-PS-12` menuntut langkah tambahan saat metode di luar rentang; tanpa langkah bersyarat kondisi itu tidak berefek |
| D-6 | Arsitektur §10: deret nomor `PBJ_PACKAGE`, `PBJ_RFQ` | Tidak di-seed, hanya konstanta kunci dan awalan | `NumberSeriesAllocator` membuat deret pada alokasi pertama (preseden `BE-PBJ-VD-001` D-1) |
| D-7 | Nilai bawaan kolom `MinimumQuotationCount` = 1, `Stage` = `PREPARATION` | Diisi model, bukan default basis data | Pola konfigurasi Pengadaan (`BE-PBJ-VD-001` D-3); default basis data pada `int` memicu masalah sentinel EF |
| D-8 | Arsitektur §10: nama metode "Penunjukan Langsung (contoh)", "Pembandingan Penawaran (contoh)" | Kode `PENUNJUKAN_LANGSUNG_CONTOH` dan `PEMBANDINGAN_PENAWARAN_CONTOH` | Akhiran `_CONTOH` mencegah bentrok kode unik saat admin memasukkan metode SOP berkode `PENUNJUKAN_LANGSUNG` |
| D-9 | — | Snapshot model diperbaiki; operasi drift dibuang dari migration | Snapshot di commit merge `c97642bb` kehilangan entri empat migration yang sudah terpasang: `20261117074530_AddProcurementVendorManagement` (tiga FK master vendor), `20261120090000_AddHrdRetirementPolicyAge`, `20261120092000_AddHrdContractDecisionNoticeEvidence`, `20261120093000_AddHrdAssignmentLetter`. Generator memasukkan objek itu ke migration baru. Seluruh objeknya terbukti sudah ada di `QuilvianNewDevSukma` (bagian 5), jadi operasinya dibuang dari `Up`/`Down`; snapshot dan Designer tetap memuat model lengkap. Catatan ini juga ditulis di XML remarks migration |
| D-10 | — | Nama dua index unik dokumen wajib = `IX_MstProcurementPackageDocumentRequirement_DocumentTypeId_Meth` dan `..._Proc` | Nama panjang pertama (67 dan 76 karakter) dipotong diam-diam oleh PostgreSQL menjadi 63 karakter saat migration diterapkan. Model, migration, Designer, dan snapshot diselaraskan ke nama yang tersimpan di basis data, sehingga migration berikutnya yang menyentuh index itu tidak gagal |
| D-11 | Arsitektur folder: DI dan `ProcurementSourcingExplicitPermissions.cs` | Tidak dibuat pada task ini | Belum ada service dan endpoint; keduanya milik `BE-PBJ-PS-004` sesuai roadmap |

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — tidak ada endpoint pada task ini (kartu task, "Di luar cakupan").

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -c Release` | 0 error | `PASS` | Keluaran build |
| `dotnet build -c Release --no-incremental` (setelah seluruh perubahan) | 0 error, 620 warning; **0 warning** dari berkas baru/berubah | `PASS` | Log build disaring per berkas |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` cakupan working tree (percobaan pertama) | Proses berakhir tanpa menulis keluaran | `NOT RUN` | Diganti percobaan kedua di bawah |
| `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path <19 berkas .cs berubah>` | VIOLATION 0, REVIEW 0, INFO 0; `Final result: PASS` | `PASS` | Laporan QBE + JSON di scratchpad sesi |
| Target basis data sebelum perintah EF | `appsettings.Development.json` → `QuilvianNewDevSukma`; environment kosong (→ Development); user secrets dan variabel lingkungan tidak memuat connection string | `PASS` | Pemeriksaan nama key tanpa mencetak nilai |
| `dotnet ef migrations list` | Tidak ada migration `(Pending)` | `PASS` | Keluaran perintah |
| `dotnet ef migrations add AddProcurementSourcingMasterData` | Migration terbentuk, tetapi memuat drift dari snapshot yang tertinggal | `EXISTING / ENVIRONMENT ISSUE` | D-9; diperbaiki sebelum diterapkan |
| `SELECT` keberadaan objek drift | Kolom `NormalRetirementAge` ada; 4 kolom `NoticeEvidence*` ada; 2 tabel `HrdAssignmentLetter*` ada; 3 FK master vendor ada | `PASS` | Keluaran `information_schema`/`pg_constraint` |
| Isi migration setelah dirapikan | 3 `CreateTable`, 6 `CreateIndex`, 3 `DropTable` di `Down`; tidak ada tabel modul lain | `PASS` | Pemeriksaan berkas migration |
| `dotnet ef database update AddProcurementSourcingMasterData` | "Applying migration '20261126043027_AddProcurementSourcingMasterData'. Done." | `PASS` | Keluaran perintah; baris `__EFMigrationsHistory` |
| `dotnet ef migrations has-pending-model-changes` (sebelum dan sesudah D-10) | "No changes have been made to the model since the last migration." | `PASS` | Keluaran perintah |
| Uji batasan (bagian 5.2) | 16 / 16 | `PASS` | Bagian 5.2 |
| Seeder dua kali (bagian 5.3) | Run 1 menambah, run 2 tidak menambah apa pun | `PASS` | Bagian 5.3 |
| Galat `InpatientIntegrationOutboxWorker` pada log run 1 | `Npgsql ... The operation has timed out` (transient) di worker Rawat Inap | `EXISTING / ENVIRONMENT ISSUE` | Tidak terkait task; tidak memengaruhi seeder yang sudah selesai sebelumnya |
| `dotnet test` | Tidak ada project test (kebijakan repository) | `NOT RUN` | — |
| Frontend | Tidak disentuh task ini | `NOT APPLICABLE` | — |

Uji manual: `NOT APPLICABLE` (tidak ada endpoint).

### 5.1 Skema terhadap kamus data (`AT-PS-62`, bagian master)

Diperiksa dari `information_schema.columns`, `pg_indexes`, dan `pg_constraint`.

| Tabel | Kolom bisnis | Index | CHECK | FK (perilaku hapus) | Cocok |
| --- | --- | --- | --- | --- | :---: |
| `MstProcurementMethod` | `MethodCode` varchar(30), `MethodName` varchar(150), `MinHpsAmount`/`MaxHpsAmount` numeric(18,2) NULL, `MinimumQuotationCount` int, `RequiresTechnicalEvaluation`, `IsSample`, `Description` varchar(500) NULL, `IsActive` | Unik `MethodCode` di antara `IsDelete = false` | `MinHpsAmount ≥ 0`, `MaxHpsAmount > 0`, `Min < Max`, minimum penawaran 1–10 | — | Ya (tanpa `SortOrder`, D-1) |
| `MstProcurementPackageDocumentType` | `DocumentTypeCode` varchar(30), `DocumentTypeName` varchar(150), `Stage` varchar(20), `IsPriceSensitive`, `IsActive` | Unik `DocumentTypeCode` di antara `IsDelete = false` | `Stage` ∈ `PREPARATION`/`QUOTATION`/`NEGOTIATION` | — | Ya |
| `MstProcurementPackageDocumentRequirement` | `DocumentTypeId`, `MethodId` NULL, `ProcurementTypeId` NULL, `IsActive` | Unik parsial per metode dan per jenis (D-3); index `MethodId`, `ProcurementTypeId` | Tepat satu dari `MethodId`/`ProcurementTypeId` | Ketiganya `RESTRICT` | Ya |

Sepuluh kolom warisan `IdentityModel` ada di ketiga tabel.

### 5.2 Uji batasan di dalam transaksi yang di-rollback

Satu blok `DO` dijalankan di antara `BEGIN` dan `ROLLBACK`; setiap kasus memakai sub-transaksi. Sesudahnya tidak ada
baris uji (`MethodCode LIKE 'UJI_%'` = 0).

| No | Skenario | Diharapkan | Hasil |
| ---: | --- | --- | --- |
| 1 | Batas bawah HPS −1 | Ditolak CHECK | `PASS` |
| 2 | Batas atas HPS 0 | Ditolak CHECK | `PASS` |
| 3 | Batas bawah 100 juta, batas atas 50 juta | Ditolak CHECK | `PASS` |
| 4 | Minimum penawaran 0 | Ditolak CHECK | `PASS` |
| 5 | Minimum penawaran 11 | Ditolak CHECK | `PASS` |
| 6 | Kode `PENUNJUKAN_LANGSUNG_CONTOH` ganda | Ditolak unik | `PASS` |
| 7 | Kode sama dengan baris yang sudah `IsDelete` | Diterima | `PASS` |
| 8 | Tahap dokumen `OTHER` | Ditolak CHECK | `PASS` |
| 9 | Kode jenis dokumen `KAK_TOR` ganda | Ditolak unik | `PASS` |
| 10 | Dokumen wajib mengisi metode **dan** jenis | Ditolak CHECK | `PASS` |
| 11 | Dokumen wajib tanpa metode maupun jenis | Ditolak CHECK | `PASS` |
| 12 | Dasar HPS × Penunjukan Langsung ganda | Ditolak unik | `PASS` |
| 13 | KAK/TOR × Jasa ganda | Ditolak unik | `PASS` |
| 14 | Dasar HPS per metode dan per jenis berdampingan | Diterima | `PASS` |
| 15 | Jenis dokumen fiktif | Ditolak FK | `PASS` |
| 16 | Hapus fisik metode yang dipakai dokumen wajib | Ditolak FK `RESTRICT` | `PASS` |

16 skenario, 16 `PASS`.

### 5.3 Seeder dijalankan dua kali (`AT-PS-64`)

Build Release dijalankan dua kali di `https://localhost:7185` terhadap `QuilvianNewDevSukma`; server `7184` milik pemilik
tidak disentuh.

| Run | Log seeder master pemilihan penyedia | Log seeder definisi workflow |
| --- | --- | --- |
| 1 | "2 metode contoh, 6 jenis dokumen paket, 4 dokumen wajib ditambahkan." | "2 definisi, 4 langkah ditambahkan" |
| 2 | "0 metode contoh, 0 jenis dokumen paket, 0 dokumen wajib ditambahkan." | "0 definisi, 0 langkah ditambahkan" |

Isi basis data sesudah run 2:

| Data | Isi | Cocok |
| --- | --- | :---: |
| Metode | `PENUNJUKAN_LANGSUNG_CONTOH` (batas atas 50.000.000, 1 penawaran, tanpa evaluasi teknis); `PEMBANDINGAN_PENAWARAN_CONTOH` (batas bawah 50.000.000, 3 penawaran, evaluasi teknis); keduanya `IsSample = true`, aktif | Ya |
| Jenis dokumen | `KAK_TOR`, `SPESIFIKASI_TEKNIS`, `DASAR_HPS` (tahap persiapan); `SURAT_PENAWARAN_HARGA`, `LAMPIRAN_TEKNIS_PENAWARAN` (penawaran); `BERITA_ACARA_NEGOSIASI` (negosiasi); tertutup harga: `DASAR_HPS`, `SURAT_PENAWARAN_HARGA`, `BERITA_ACARA_NEGOSIASI` | Ya |
| Dokumen wajib | `KAK_TOR` × `SERVICE`; `SPESIFIKASI_TEKNIS` × `ASSET`; `DASAR_HPS` × kedua metode contoh | Ya (D-4) |
| Definisi `PBJ-PKT-PERSIAPAN` | `RequestType = PBJ_PACKAGE`, `IsActive = false`, `Draft`, versi 1; langkah 1 `Position`; langkah 2 `Position` bersyarat `PBJ_METHOD_OUT_OF_RANGE`; `AllowSelfApproval = false` | Ya (D-5) |
| Definisi `PBJ-PKT-PENETAPAN` | `RequestType = PBJ_PACKAGE_AWARD`, `IsActive = false`, `Draft`, versi 1; langkah 1 `Position`; langkah 2 `Position` bersyarat `PBJ_OVER_HPS`; `AllowSelfApproval = false` | Ya |
| Matriks approval `PBJ-PKT-*` | 0 baris (tanpa matriks, fail-closed `PBJ-DEC-038`) | Ya |

Seeder mencari menurut Id atau kode, termasuk baris yang sudah dihapus. Akibatnya, metode atau jenis dokumen yang
dihapus admin tidak dihidupkan lagi, dan dokumen wajib yang menunjuk baris terhapus itu dilewati.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-PS-60` registry `Sourcing` `ACTIVE` sebelum model pertama; QBE lulus | Terpenuhi | Baris registry `ProcurementManagement / Sourcing · Prc · ACTIVE` (2026-11-26, `PBJ-DEC-096`); model task ini di `MasterData` (`Mst`); QBE Strict `PASS` |
| `AT-PS-61` build | Terpenuhi untuk backend; frontend tidak disentuh | Bagian 5 |
| `AT-PS-62` tiga tabel master cocok kamus data | Terpenuhi, dengan delta D-1, D-3 | Bagian 5.1, 5.2; `has-pending-model-changes` bersih |
| `AT-PS-64` seeder dua kali tanpa baris berlipat, metode contoh bertanda `IsSample` | Terpenuhi | Bagian 5.3 |
| Laporan `task/report/backend/BE-PBJ-PS-001.md` | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Metode, jenis dokumen, dan dokumen wajib adalah **nilai contoh**; nilai final menunggu SOP (`PBJ-OQ-005`, `PBJ-OQ-019`). Kedua definisi `PBJ-PKT-*` tidak aktif sampai admin mengisi jabatan penyetuju |
| Masalah yang diketahui | (1) Kamus data masih memuat `SortOrder` (D-1) dan belum menyebut sisi inklusif rentang HPS (D-2); perlu diselaraskan pemilik desain. (2) `ProcurementStatuses.PurchaseRequest.Final` memuat `CONVERTED_TO_PACKAGE`, padahal `PBJ-DEC-077` membolehkan PR kembali ke `APPROVED` saat paket batal. Tidak berdampak pada task ini; wajib ditangani di `BE-PBJ-PS-005` |
| Risiko tersisa | Snapshot yang tertinggal (D-9) juga ada di branch lain yang membawa merge `c97642bb`. Developer lain yang membuat migration dari snapshot lama akan mendapat drift yang sama, kecuali snapshot hasil task ini sudah ikut di-merge |
| Perubahan sampingan | Snapshot model kini memuat entri empat migration modul lain yang sebelumnya hilang (D-9). Ini perlu agar `has-pending-model-changes` bersih; tidak ada berkas modul lain yang diubah |
| Interupsi | Percobaan QBE pertama berakhir tanpa keluaran; diulang dengan cakupan berkas eksplisit. Skrip seeder run 1 sempat disunting saat berjalan, sehingga muncul pesan shell; hasil run tidak terpengaruh |
| Status Git | Belum di-commit. Baru: 3 model, 3 konfigurasi EF, `ProcurementSourcingMasterSeeder.cs`, migration + Designer, laporan ini. Diubah: `ApplicationDbContext.cs`, `Program.cs`, `ApplicationDbContextModelSnapshot.cs`, enam konstanta, `ProcurementWorkflowDefinitionSeeder.cs`, roadmap, traceability, `MODULE-STATUS.md`. Masih ada pula perubahan dokumen approval pemilihan-penyedia, registry, dan roadmap dari sesi blueprint sebelumnya |
| Langkah berikutnya | `BE-PBJ-PS-002` (tiga master × sembilan endpoint) dan `BE-PBJ-PS-003` (kolom paket pada baris PR), keduanya gelombang 2 dan boleh paralel |
