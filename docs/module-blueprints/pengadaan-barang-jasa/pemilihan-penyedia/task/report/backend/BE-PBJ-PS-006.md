# Laporan Perubahan Backend — `BE-PBJ-PS-006`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-PBJ-PS-006` |
| Judul | Lampiran paket dan lampiran harga tertutup (menyentuh `Common`) |
| Slice | `MVP-PS1` — saringan lampiran harga (`04-prd-to-mvp.md` bagian 20; roadmap "Penyimpangan urutan yang disengaja") |
| Roadmap | `pemilihan-penyedia/roadmap/backend-roadmap.md` revisi 1, kartu `BE-PBJ-PS-006`, gelombang 5 |
| Trace | `PBJ-DEC-074`, `079`, `080`, `095`; `PBJ-CAP-004`, `026`; `PBJ-PS-API` §Perubahan sub-modul lain (`/attachments`); `PBJ-PS-PERM` bagian 3–4; `PBJ-PP-VAL` `PBJ-VAL-080`..`083`; `BE-PBJ-PS-002` D-5 |
| Contract version | Kontrak `PBJ-PS-*` set `1.0` (`approved`, `PBJ-DEC-095`) |
| Dependency | `BE-PBJ-PS-005` ✅ |
| Klasifikasi | `MEDIUM` — skor 7: berkas diperiksa 9–20 (1), berkas diubah 4–8 (1), logika sedang (1), menyentuh fondasi `Common` sub-modul lain (2), keamanan harga dan cakupan (2) |
| Task mode | `BACKEND` (pemilik menjawab "lanjutkan" atas tawaran `BE-PBJ-PS-006`, 27 November 2026) |
| Wewenang database | Tanpa migration. Uji runtime dan SQL langsung (konfigurasi `Wfl` uji, status paket uji sementara, pembersihan) atas konfirmasi terpisah pemilik, 27 November 2026: "Ya, jalankan dan bersihkan" |
| Target tulis | `Common/Services/ProcurementAttachmentService.cs`, `Common/Services/IProcurementAttachmentCategoryPolicy.cs` (baru), `Common/Constants/ProcurementStatuses.cs`, `Sourcing/Services/ProcurementSourcingAccessProvider.cs` (baru), `Sourcing/Services/ProcurementPackageService.cs`, DI |
| Model | Claude Opus 5.5 |
| Branch / commit | Backend `sukmagp` @ `de41c752`; perubahan `BE-PBJ-PS-005` masih belum di-commit di working tree. Frontend `sukmagpV2` @ `3e5386679` |
| Tanggal | 27 November 2026 |
| Status | ✅ **SELESAI.** Lampiran paket dapat diunggah dan dilihat Tim Pengadaan; lampiran jenis harga tersembunyi dan unduhannya 404 tanpa `ReadPrice`; kategori bebas ditolak; lampiran PR dan rencana paket tidak berubah. Build Release 0 error; QBE Strict `PASS` 6 berkas; uji runtime 17/17 `PASS`; fixture dibersihkan |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `ProcurementManagement` / `Common` (lampiran bersama, milik `perencanaan-permintaan`) dan `Sourcing` (penyedia akses dan aturan kategori) |
| Registry | `ProcurementManagement / Sourcing · Prc · ACTIVE` (`PBJ-DEC-096`); `Common` memakai baris `Prc` yang sudah `ACTIVE` |
| Keberlakuan | `NEW CODE` untuk `IProcurementAttachmentCategoryPolicy.cs` dan `ProcurementSourcingAccessProvider.cs`; `TOUCHED LEGACY` untuk `ProcurementAttachmentService.cs`, `ProcurementStatuses.cs`, `ProcurementPackageService.cs`, `ProcurementManagementServiceCollectionExtensions.cs` |
| Arketipe endpoint | Tidak ada endpoint baru. `ProcurementAttachmentsController` (sub-resource yang dicakup dokumen induk) tidak diubah; perilakunya berubah hanya untuk `PBJ_PACKAGE` dan `PBJ_RFQ` |
| Akses | Tanpa role hardcode. Hak atas dokumen induk dibaca dari izin `ProcurementPackage : Read`/`Update`, `ProcurementRfq : Read`/`Update`, `ProcurementQuotation : Read`/`Create`/`Update` lewat `AccessPermissionService`, ditambah cakupan unit (`ReadAllUnits`) dan penugasan evaluator (data). Penutup harga membaca penanda `ProcurementPackage : ReadPrice` |
| QBE yang berlaku | `QBE-NAM-001`, `QBE-MOD-001..003`, `QBE-SVC-001`, `QBE-CODE-004` |
| Hasil checker | QBE Strict `PASS` (6 berkas, 0 temuan) |

## 1. Masalah yang diselesaikan

Paket sudah bisa dibuat (`BE-PBJ-PS-005`), tetapi lampirannya belum bisa diunggah: mesin lampiran `Common` menolak jenis
dokumen tanpa penyedia akses (fail-closed). Task ini membuka lampiran paket dan RFQ dengan tiga penjaga:

1. **Siapa boleh melihat dan mengubah.** Contoh: Tim Pengadaan mengunggah KAK/TOR selama paket masih draf; Keuangan hanya
   membaca; staf unit peminta yang tidak memegang `ProcurementPackage : Read` tidak melihat lampiran paket sama sekali.
2. **Lampiran harga tertutup** (`PBJ-DEC-079`). Contoh: berkas "Dasar HPS" tidak muncul di daftar lampiran Keuangan, dan
   tautan unduhnya dijawab 404, karena Keuangan tidak memegang `ProcurementPackage : ReadPrice`.
3. **Kategori wajib dari daftar jenis dokumen paket** (`PBJ-DEC-095`). Contoh: kategori `KAK_TOR` diterima, kategori
   "Lain-lain" ditolak. Tanpa penjaga ini, berkas harga yang diberi kategori bebas akan lolos dari saringan harga.

## 2. Aturan yang dibangun

| Dokumen induk | `Read` (lihat, unduh) | `Edit` (unggah, ganti, tarik) |
| --- | --- | --- |
| Paket `PBJ_PACKAGE` | Paket dalam cakupan unit **dan** pengguna memegang `ProcurementPackage : Read`; atau pengguna evaluator aktif paket itu (`PrcPackageEvaluator`) | Syarat `Read` lewat izin, ditambah `ProcurementPackage : Update`, dan paket `DRAFT`/`PREPARATION_RETURNED` |
| RFQ `PBJ_RFQ` | Paket induk dalam cakupan **dan** `ProcurementRfq : Read` atau `ProcurementQuotation : Read`; atau evaluator paket itu | `ProcurementRfq : Update`, `ProcurementQuotation : Create`, atau `ProcurementQuotation : Update`, selama RFQ `DRAFT`/`SENT`/`CLOSED` dan paket `READY_FOR_RFQ`/`RFQ_OPEN`/`EVALUATION` |

Di luar hak → 404 (dokumen tidak dibocorkan). Hak `Read` saat butuh `Edit` → 409 `PBJ-VAL-083` (perilaku lama `Common`).

| Aturan kategori (`PBJ_PACKAGE`, `PBJ_RFQ`) | Bila dilanggar |
| --- | --- |
| Kategori wajib diisi dengan kode jenis dokumen paket yang aktif; huruf kecil dibakukan | 422 `PBJ-VAL-080` |
| Pengguna tanpa `ReadPrice` hanya melihat lampiran berkategori jenis dokumen yang bukan `IsPriceSensitive`; kategori tak dikenal ikut tersembunyi | Tidak didaftar; unduh, ganti, dan tarik → 404 |
| Mengganti lampiran tanpa kategori mewarisi kategori versi sebelumnya | — |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu task dan tabel roadmap; arsitektur §6 (tabel service, class diagram); `api-contract.md` (§Technical Evaluation,
§Perubahan sub-modul lain); `permission-audit-matrix.md` bagian 3–4; kamus data (`MstProcurementPackageDocumentType`,
`PrcAttachment`); `perencanaan-permintaan/contracts/validation-matrix.md` (`PBJ-VAL-080`..`083`); laporan `BE-PBJ-PS-002`
(D-5). Source: `ProcurementAttachmentService`, `ProcurementAttachmentsController`, `ProcurementAttachmentDtos`,
`IProcurementDocumentAccessProvider` beserta enam penyedia yang ada (PR, rencana kebutuhan, rencana paket, pagu, dokumen
vendor, penilaian vendor), `ProcurementPackageDocumentTypeService` (kunci kode), `ProcurementPackageService`,
`PrcPackageEvaluator`, `PrcRfq`, `AccessPermissionService.HasAccessAsync`, `ProcurementPermissions`.

### 3.2 Berkas yang berubah

| Berkas | Status | Isi |
| --- | --- | --- |
| `Areas/Corporate/ProcurementManagement/Common/Services/IProcurementAttachmentCategoryPolicy.cs` | Baru | Titik perluasan opsional: `Handles`, `NormalizeCategoryAsync`, `GetVisibleCategoriesAsync` |
| `Areas/Corporate/ProcurementManagement/Common/Services/ProcurementAttachmentService.cs` | Diperbarui | Memanggil aturan kategori bila ada: daftar disaring, unggah memeriksa kategori, ganti/tarik/unduh lampiran tersembunyi → 404. Jenis dokumen tanpa aturan tidak berubah |
| `Areas/Corporate/ProcurementManagement/Sourcing/Services/ProcurementSourcingAccessProvider.cs` | Baru | `ProcurementSourcingAccessProvider` (logika hak paket dan RFQ), adaptor `PackageAttachmentAccessProvider` dan `RfqAttachmentAccessProvider`, aturan `ProcurementSourcingAttachmentCategoryPolicy` |
| `Areas/Corporate/ProcurementManagement/Sourcing/Services/ProcurementPackageService.cs` | Diperbarui | `IsInScopeAsync` (cakupan unit paket, dipakai penyedia akses); pemeriksaan status draf memakai helper baru |
| `Areas/Corporate/ProcurementManagement/Common/Constants/ProcurementStatuses.cs` | Diperbarui | `Package.IsPreparationEditable(status)` |
| `Areas/Corporate/ProcurementManagement/ProcurementManagementServiceCollectionExtensions.cs` | Diperbarui | Pendaftaran penyedia akses (×2), logika penyedia, dan aturan kategori |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Sesuai `PBJ-PS-API` §Perubahan sub-modul lain: untuk `PBJ_PACKAGE`/`PBJ_RFQ`, `GET /attachments` tidak mengembalikan lampiran harga dan unduhannya 404 tanpa `ReadPrice`. Unggah kedua jenis itu kini mewajibkan `attachmentCategory`. Jenis dokumen lain tidak berubah |
| Database | Tanpa migration dan tanpa perubahan skema |
| Keamanan/Auth | Lampiran harga disembunyikan di satu titik (aturan kategori), termasuk saat ganti/tarik lewat ID. Kategori tak dikenal tersembunyi (fail-closed). Lampiran paket butuh `ProcurementPackage : Read`, tidak cukup cakupan unit saja |

**Selisih terhadap desain (delta), dicatat apa adanya.**

| # | Desain | Yang dibangun | Alasan |
| ---: | --- | --- | --- |
| D-1 | Arsitektur: "`ProcurementAttachmentService` diperbarui menyaring lampiran `IsPriceSensitive`" | Titik perluasan `IProcurementAttachmentCategoryPolicy` di `Common`; aturan paket berada di `Sourcing` | Pola `IProcurementDocumentAccessProvider`: lampiran `Common` tidak mengenal aturan tiap dokumen. Satu-satunya perubahan perilaku `Common` adalah memanggil aturan bila terdaftar |
| D-2 | Satu kelas `ProcurementSourcingAccessProvider` | Kelas logika dengan nama itu + dua adaptor `IProcurementDocumentAccessProvider` | Interface melayani satu `DocumentType` per penyedia; pola adaptor sama dengan penyedia PR/rencana/vendor |
| D-3 | "Tim Pengadaan dan penyetuju `Read`/`Edit`" | Diterjemahkan ke izin: `Read` = `ProcurementPackage : Read` + cakupan; `Edit` = juga `Update` + status persiapan | Kewenangan dari layar Akses Role, bukan nama peran (bundel `PBJ-DEC-080`: Tim Pengadaan punya `Update`, penyetuju tidak) |
| D-4 | — | Lampiran paket butuh `ProcurementPackage : Read`; cakupan unit saja tidak cukup | Unit Peminta hanya `ReadSummary` (`PBJ-DEC-079`, `080`). Penyedia PR lama hanya memakai cakupan; dicatat, tidak diubah |
| D-5 | Kartu: `PBJ_RFQ` diatur sama | Aturan RFQ seperti tabel bagian 2 | Izin RFQ dan penawaran dari bundel `PBJ-DEC-080`. Resource `ProcurementRfq`/`ProcurementQuotation` belum punya endpoint, sehingga cabang `Read`/`Edit` RFQ tertutup sampai `BE-PBJ-PS-009`/`010`; bukti runtime di sana. Di task ini hanya fail-closed yang diuji (`T-13`) |
| D-6 | — | Kategori dibakukan huruf besar; jenis tidak aktif ditolak; `Stage` jenis dokumen tidak dicocokkan dengan `PBJ_PACKAGE`/`PBJ_RFQ` | Kode master selalu huruf besar. Kecocokan tahap ↔ dokumen induk belum diputuskan; tidak dikarang |
| D-7 | — | Galat kategori memakai `PBJ-VAL-080` | `PBJ-PS-VAL` tidak punya kode kategori; `PBJ-VAL-080` sudah dipakai untuk isian lampiran yang tidak sah (mis. kategori > 50 karakter) |
| D-8 | — | Pengguna tanpa `ReadPrice` tetap boleh **mengunggah** berkas berkategori harga | Tidak membocorkan harga (ia tidak dapat melihatnya kembali). Menolaknya adalah aturan baru yang belum diputuskan |
| D-9 | — | `ProcurementPackageService.IsInScopeAsync` dan `ProcurementStatuses.Package.IsPreparationEditable` | Satu sumber aturan cakupan dan status persiapan untuk service paket dan penyedia akses; tanpa perubahan perilaku `BE-PBJ-PS-005` |

## 4. Dokumentasi endpoint

Tidak ada endpoint baru. Perilaku endpoint yang sudah ada berubah untuk dua jenis dokumen:

### Corporate / Procurement Management / Common / Attachment

Base URL: `api/v1/corporate/procurement-management/attachments` — **tersedia** (sejak `BE-PBJ-004`).

| Method | Path | Perubahan untuk `PBJ_PACKAGE`/`PBJ_RFQ` | Hak akses | Kode |
| --- | --- | --- | --- | --- |
| `GET` | `/?documentType=&documentId=&includeHistory=` | Lampiran berkategori jenis harga tidak didaftar tanpa `ProcurementPackage : ReadPrice` | `ProcurementAttachment : Read` + hak dokumen induk | 200, 404 |
| `POST` | `/` (multipart) | `attachmentCategory` wajib = kode jenis dokumen paket aktif (atau diwarisi saat mengganti) | `ProcurementAttachment : Upload` + `Edit` dokumen induk | 200, 404, 409 `PBJ-VAL-083`, 422 `PBJ-VAL-080`..`082` |
| `GET` | `/{id}/download` | Lampiran jenis harga → 404 tanpa `ReadPrice` | `ProcurementAttachment : Read` + hak dokumen induk | 200, 404 |
| `POST` | `/{id}/deactivate` | Lampiran jenis harga → 404 tanpa `ReadPrice` | `ProcurementAttachment : Upload` + `Edit` dokumen induk | 200, 404, 409 |

Contoh unggah: `documentType=PBJ_PACKAGE`, `documentId=<paket>`, `attachmentCategory=DASAR_HPS`, `file=dasar-hps.pdf` →
200. Keuangan membuka `GET /attachments?documentType=PBJ_PACKAGE&documentId=<paket>` → hanya KAK/TOR dan spesifikasi.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -c Release --no-incremental` | 0 error, 620 warning (= baseline `BE-PBJ-PS-003`..`005`); 0 warning dari berkas yang diubah | `PASS` | Log build scratchpad sesi |
| QBE Strict, 6 berkas | 0 `VIOLATION`, 0 `REVIEW`, 0 `INFO` | `PASS` | Keluaran checker |
| Uji runtime (`https://localhost:7185` → `QuilvianNewDevSukma`) | 17/17 `PASS` | `PASS` | `ps006.results.json` scratchpad sesi |
| Pembersihan | Hapus lunak dalam satu transaksi; `PBJ-PR` kembali `IsActive = false`, `Draft`, 0 matriks uji hidup | `PASS` | Bagian 5.2 |

**Fixture (`TEST-PBJ PS006`, lewat API SuperAdmin).** Unit IGD dan Pengadaan; enam akun `Employee` tanpa role: pemohon
IGD (`ProcurementRequest` + `ProcurementAttachment : Read, Upload`), staf IGD (`ProcurementPackage : Read, Create, Update`
+ lampiran, **tanpa** `ReadPrice`), Keuangan (`ProcurementPackage : Read, ReadAllUnits` + `ProcurementAttachment : Read`),
verifikator, penyetuju, Tim Pengadaan (`ProcurementPackage : Read, Create, Update, ReadAllUnits, ReadPrice` + lampiran +
master jenis dokumen paket). PR IGD dibawa ke `APPROVED` lewat alur sebenarnya memakai `Wfl` uji (pola `BE-PBJ-PS-005`),
lalu dipaketkan. Jenis dokumen yang dipakai: `KAK_TOR` (biasa) dan `DASAR_HPS` (`IsPriceSensitive`) dari data awal.

### 5.1 Skenario runtime

| ID | Skenario | Hasil | Bukti |
| --- | --- | --- | --- |
| `F-01` | Fixture: PR IGD `APPROVED` lewat `Wfl` uji lalu dipaketkan (`DRAFT`) | `PASS` | PR-000029 |
| `T-01` | AC1: Tim Pengadaan mengunggah `KAK_TOR` dan `DASAR_HPS` → 200; `kak_tor` dibakukan menjadi `KAK_TOR` | `PASS` | 200 / 200 / 200 |
| `T-02` | AC1: Tim Pengadaan (`ReadPrice`) melihat tiga lampiran, termasuk Dasar HPS | `PASS` | 3 baris |
| `T-03` | AC1: Tim Pengadaan mengunduh Dasar HPS → 200 berkas PDF | `PASS` | 45 byte `application/pdf` |
| `T-04` | AC3: kategori kosong, "Lain-lain", atau kode tak dikenal → 422 | `PASS` | "Pilih jenis dokumen paket untuk lampiran ini." |
| `T-05` | AC2: Keuangan melihat 2 lampiran biasa; Dasar HPS tidak didaftar, juga dengan `includeHistory=true` | `PASS` | 2 / 2 |
| `T-06` | AC2: Keuangan mengunduh Dasar HPS → 404; KAK/TOR → 200 | `PASS` | 404 / 200 |
| `T-07` | Keuangan tanpa `ProcurementAttachment : Upload` → 403 | `PASS` | 403 |
| `T-08` | Staf IGD (`Update`, tanpa `ReadPrice`): daftar tanpa Dasar HPS; unggah jenis biasa 200; ganti, tarik, unduh Dasar HPS lewat ID → 404 | `PASS` | 200 / 404 / 404 / 404 |
| `T-09` | Pemohon IGD tanpa `ProcurementPackage : Read` (punya `ProcurementAttachment : Read`) → 404 pada lampiran paket | `PASS` | 404 (D-4) |
| `T-10` | Ganti KAK/TOR tanpa kategori → versi 2 mewarisi `KAK_TOR`; tarik versi 2 → 200 | `PASS` | `versionNo` 2 |
| `T-11` | Paket `READY_FOR_RFQ` (diset sementara lewat SQL uji) → unggah 409 `PBJ-VAL-083`; daftar tetap 200; status dikembalikan `DRAFT` | `PASS` | 409 / 200 |
| `T-12` | `BE-PBJ-PS-002` D-5: kode jenis dokumen yang dipakai lampiran paket tidak dapat diganti (409) dan jenisnya tidak dapat dihapus (400 `PBJ-VAL-071`); sesudah dinonaktifkan, kodenya ditolak untuk unggahan baru (422) | `PASS` | 409 / 400 / 200 / 422 |
| `T-13` | Fail-closed: RFQ tak dikenal 404; `PBJ_PACKAGE_AWARD` (belum punya penyedia) 404; paket tak dikenal 404 | `PASS` | 404 / 404 / 404 |
| `T-14` | AC4 regresi PR: kategori bebas 200, daftar 200, unduh 200, isi palsu 422, tanpa kategori 200, tarik 200; PR `APPROVED` → 409 | `PASS` | Semua sesuai perilaku `BE-PBJ-004` |
| `T-15` | AC4 regresi rencana paket: kategori bebas tetap diterima dan terdaftar | `PASS` | 200 / 1 baris |
| `T-16` | Riwayat paket mencatat `ATTACH`/`DETACH` | `PASS` | 7 baris |

Regresi pagu dan vendor tidak dijalankan sebagai skenario tersendiri. Jalur kodenya sama dengan PR dan rencana paket:
tidak ada aturan kategori yang terdaftar untuk `PBJ_BUDGET`, `PBJ_VENDOR_DOCUMENT`, dan `PBJ_VENDOR_EVALUATION`, sehingga
`GetVisibleCategoriesAsync` mengembalikan `null` dan perilakunya identik dengan sebelum task ini.

### 5.2 Pembersihan

| Tabel | Baris |
| --- | --- |
| `PrcAttachment` | 9 dihapus lunak (paket, PR, rencana paket uji) |
| `PrcPackageItemSource` / `PrcPackageItem` / `PrcPackage` | 1 / 1 / 1 dihapus lunak |
| `PrcPackagePlan`, `MstProcurementPackageDocumentType` | 1, 1 dihapus lunak |
| `PrcPurchaseRequestLine` / `PrcPurchaseRequest` | 2 dilepas dari paket lalu dihapus lunak / 2 dihapus lunak |
| `MstProcurementItem`, `SysAccessPolicy`, `MstEmployee`, `MstPosition`, `MstDepartment` | 1, 32, 6, 6, 2 dihapus lunak; kode unit diberi akhiran `-DEL-<run>` |
| `AspNetUsers` | 6 nonaktif + lockout |
| `MstApprovalMatrix` uji | Dihapus lunak; `PBJ-PR` kembali `IsActive = false`, `Draft` |

Yang sengaja tidak dihapus: `PrcDocumentHistory` dan instance `Wfl` milik PR uji (jejak audit), nomor `PR-000029` (PR kedua tetap draf tanpa nomor),
serta 9 berkas fisik lampiran uji di `App_Data/procurement-attachments` (tidak tercatat lagi, jadi tidak dapat diunduh).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Tim Pengadaan mengunggah dan melihat lampiran paket | Terpenuhi | `T-01`, `T-02`, `T-03`, `T-10` |
| (2) Pemegang `ProcurementPackage : Read` tanpa `ReadPrice` (bundel Keuangan) tidak melihat Dasar HPS, unduhannya 404 | Terpenuhi | `T-05`, `T-06`, `T-08` |
| (3) Kategori di luar daftar jenis dokumen ditolak | Terpenuhi | `T-04`, `T-12` (jenis nonaktif) |
| (4) Regresi lampiran PR, rencana, pagu, vendor | Terpenuhi: PR dan rencana paket diuji runtime; pagu dan vendor lewat jalur kode yang sama (bagian 5.1) | `T-14`, `T-15` |
| Cabang evaluator (`AT-PS-31` bagian lampiran) | **Dikecualikan oleh kartu** — dibuktikan di `BE-PBJ-PS-011` | Kode ada (`IsAssignedEvaluatorAsync`); penugasan baru ada di `BE-PBJ-PS-007` |
| `BE-PBJ-PS-002` D-5 (kode terkunci lampiran paket) | Terpenuhi | `T-12` |
| Laporan | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Layar unggah lampiran paket wajib mengirim `attachmentCategory` dari daftar jenis dokumen paket (`GET /master-data/package-document-types/options`); tanpa itu 422 |
| Masalah yang diketahui | `ProcurementAttachmentsController` tidak mengirim `errors.code` pada 409/422 (perilaku lama `BE-PBJ-004`), sehingga FE hanya mendapat pesan. Penyedia akses PR hanya memakai cakupan unit tanpa `ProcurementRequest : Read` (D-4). Keduanya temuan, tidak diubah |
| Keputusan terbuka | Apakah `Stage` jenis dokumen harus cocok dengan dokumen induk (D-6); apakah pengguna tanpa `ReadPrice` boleh mengunggah berkas berkategori harga (D-8) |
| Risiko tersisa | Cabang RFQ dan evaluator belum terbukti runtime (D-5; `BE-PBJ-PS-009`..`011`) |
| Perubahan sampingan | `NONE` di source. Di DB uji: fixture dihapus lunak; konfigurasi `Wfl` uji dicabut |
| Status Git | Belum di-commit. Baru: `IProcurementAttachmentCategoryPolicy.cs`, `ProcurementSourcingAccessProvider.cs`, laporan ini. Diubah: `ProcurementAttachmentService.cs`, `ProcurementStatuses.cs`, `ProcurementPackageService.cs`, `ProcurementManagementServiceCollectionExtensions.cs`, roadmap backend, traceability, `MODULE-STATUS.md`. Ditambah perubahan `BE-PBJ-PS-005` yang juga belum di-commit |
| Langkah berikutnya | `BE-PBJ-PS-007` (persiapan paket: ubah persiapan, saran metode, checklist, kriteria, evaluator; termasuk cabang "metode dipakai paket" `BE-PBJ-PS-002` D-7) — gelombang 6. `FE-PBJ-PS-001` boleh paralel |
