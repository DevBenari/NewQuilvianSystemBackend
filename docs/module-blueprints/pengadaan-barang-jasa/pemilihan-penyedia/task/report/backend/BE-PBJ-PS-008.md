# Laporan Perubahan Backend — `BE-PBJ-PS-008`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-PBJ-PS-008` |
| Judul | Persetujuan persiapan |
| Slice | `MVP-PS1` — `PS-02` persetujuan persiapan (`04-prd-to-mvp.md` bagian 20) |
| Roadmap | `pemilihan-penyedia/roadmap/backend-roadmap.md` revisi 1, kartu `BE-PBJ-PS-008`, gelombang 7 |
| Trace | `PBJ-DEC-007`, `038`, `072`, `073`, `097`; FR-PS-014; `PBJ-PS-API` `preparation/*`, `approval`, `workflow/synchronize`; `PBJ-PS-STATE` 1.1 bagian 1; `PBJ-PS-VAL` `PBJ-VAL-090`, `091`, `124`..`128`, `148`; `AT-PS-10`, `12`..`16`, `18` |
| Contract version | `PBJ-PS-API`, `-VAL`, `-PERM`, `-MVP` 1.0; `PBJ-PS-STATE`, `-INT`, `-TEST` **1.1** (`PBJ-DEC-097`, 27 November 2026: `PBJ-VAL-091` = 409) |
| Dependency | `BE-PBJ-PS-007` ✅ |
| Klasifikasi | `HEAVY` — skor 10: berkas diperiksa > 20 (2), berkas diubah 4–8 (1), logika tinggi (2), integrasi `Wfl` (2), kontrak API baru (1), menyentuh `Common` (1), menyentuh baris PR (1) |
| Task mode | `BACKEND` (pemilik menjawab "lanjutkan" atas tawaran `BE-PBJ-PS-008`, 27 November 2026) |
| Keputusan sebelum task | `PBJ-DEC-097` (pemilik: "409 untuk semua"): `PBJ-VAL-091` selalu 409; dicatat di `00-interview-decisions.md`, kontrak `PBJ-PS-STATE`/`-INT`/`-TEST` naik ke 1.1, manifest sub-modul diperbarui |
| Wewenang database | Tanpa migration. Uji runtime dan SQL langsung (konfigurasi `Wfl` uji dua definisi, alasan tolak uji, pembersihan) atas konfirmasi terpisah pemilik, 27 November 2026: "Ya, jalankan dan bersihkan" |
| Target tulis | `Sourcing/Services/ProcurementPackageApprovalService.cs` (baru), `ProcurementPackageService.cs`, `Sourcing/DTOs/PackageDtos.cs`, `Sourcing/Controllers/ProcurementPackagesController.cs`, `Common/Services/ProcurementApprovalTimelineService.cs`, DI |
| Model | Claude Opus 5.5 |
| Branch / commit | Backend `sukmagp` @ `de41c752`; perubahan `BE-PBJ-PS-005`..`007` masih belum di-commit di working tree. Frontend `sukmagpV2` @ `3e5386679` |
| Tanggal | 27 November 2026 |
| Status | ✅ **SELESAI.** Persiapan paket dapat diajukan ke `Wfl`, ditarik pengajunya, disetujui/dikembalikan/ditolak lewat kotak masuk, dibuka ulang, dan disinkronkan ulang; nomor `PKT` lahir saat pertama diajukan. Build Release 0 error; QBE Strict `PASS` 6 berkas; uji runtime 21/21 `PASS`; fixture dan konfigurasi `Wfl` uji dibersihkan |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `ProcurementManagement` / `Sourcing`; `Common` (linimasa persetujuan) |
| Registry | `ProcurementManagement / Sourcing · Prc · ACTIVE` (`PBJ-DEC-096`); `Common` memakai baris `Prc` yang sudah `ACTIVE` |
| Keberlakuan | `NEW CODE` untuk `ProcurementPackageApprovalService.cs` dan DTO baru; `TOUCHED LEGACY` untuk `ProcurementPackageService.cs`, `ProcurementPackagesController.cs`, `ProcurementApprovalTimelineService.cs`, DI |
| Arketipe endpoint | Transaksi — aggregate ber-lifecycle: perpindahan status lewat `POST /{id}/preparation/<aksi>`; baca linimasa `GET /{id}/approval`; `POST /{id}/workflow/synchronize` |
| Akses | `ProcurementPackage : Submit` (ajukan, tarik, sinkron ulang), `Revise` (buka ulang), `Read` (linimasa). Pengaju ≠ penyetuju dijaga `Wfl` (`AllowSelfApproval = false`) dan pemeriksaan ulang di `ProcurementWorkflowLifecycleHandler`; tarik hanya oleh pemohon instance (kepemilikan data). Tanpa role hardcode |
| QBE yang berlaku | `QBE-NAM-001`, `QBE-NAM-002`, `QBE-MOD-001..003`, `QBE-SVC-001`, `QBE-CODE-004` |
| Hasil checker | QBE Strict `PASS` (6 berkas, 0 temuan) |

## 1. Masalah yang diselesaikan

Persiapan paket sudah dapat disusun (`BE-PBJ-PS-007`), tetapi belum dapat disetujui. Task ini menyambungkan paket ke mesin
persetujuan bersama (`Wfl`). Contoh `AT-PS-15`: Andi (Tim Pengadaan) mengajukan persiapan 15 kursi roda, HPS Rp 42.750.000,
metode Penunjukan Langsung (contoh). Paket mendapat nomor `PKT-…` dan menunggu persetujuan. Penyetuju menyetujui lewat kotak
masuk → paket `READY_FOR_RFQ` dan HPS Rp 42.750.000 terkunci sebagai HPS disetujui. Bila Andi memilih metode di luar rentang
HPS (dengan alasan), matriks menambah satu jenjang persetujuan (`AT-PS-12`). Bila penyetuju menolak, baris PR kembali bebas
untuk dipaketkan ulang.

## 2. Aturan yang dibangun

| Langkah | Syarat | Bila dilanggar |
| --- | --- | --- |
| Ajukan (`DRAFT`/`PREPARATION_RETURNED` → `PREPARATION_PENDING_APPROVAL`) | Versi data cocok; HPS > 0 setiap barang; metode terpilih masih aktif; alasan bila di luar rentang salinan metode; seluruh dokumen wajib checklist terlampir; definisi `PBJ-PKT-PERSIAPAN` aktif | 409 `PBJ-VAL-090`; 422 `PBJ-VAL-124`, `125`, `126`, `127`; 409 `PBJ-VAL-091` |
| Tarik (`PREPARATION_PENDING_APPROVAL` → `DRAFT`) | Hanya pengaju (pemohon instance); instance belum diputus | 403; 409 `PBJ-VAL-128` |
| Hasil `Wfl` | Disetujui → `READY_FOR_RFQ` + `ApprovedHpsTotal` + `PreparationApprovedAt`; minta revisi/dikembalikan → `PREPARATION_RETURNED`; ditolak → `REJECTED` + baris PR dilepas; ditarik → `DRAFT` | — |
| Buka ulang (`READY_FOR_RFQ`/`EVALUATION` → `DRAFT`) | Alasan wajib; RFQ `DRAFT`/`SENT` dibatalkan | 422 `PBJ-VAL-148`; 409 `PBJ-VAL-128` |
| Ubah HPS setelah disetujui | — | 409 `PBJ-VAL-149` (dari `BE-PBJ-PS-007`) |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu task; `state-transition-matrix.md` bagian 1; `validation-matrix.md`; `integration-contract.md` (`Wfl`);
`permission-audit-matrix.md` bagian 3–5; `acceptance-test-matrix.md` `AT-PS-10`..`18`; `00-interview-decisions.md`
(`PBJ-DEC-072`, `073`, `097`). Source: `ProcurementApprovalGateway`, `ProcurementWorkflowLifecycleHandler`
(`IProcurementWorkflowStatusApplier`, anti-setujui-sendiri), `ProcurementApprovalTimelineService`,
`ProcurementPackagePlanService` (ajukan, nomor, penerap status), `ProcurementNeedPlanService` (transaksi penerap),
`ProcurementPurchaseRequestVerificationService` dan `ProcurementPurchaseRequestService` (tarik, sinkron ulang),
`ProcurementWorkflowDefinitionSeeder` (`PBJ-PKT-PERSIAPAN-01`/`02`, kondisi `PBJ_METHOD_OUT_OF_RANGE`), `WorkflowService`
(langkah berkondisi), `ApprovalInboxController`, model `TrxWorkflowStepInstance`, `PrcRfq` (CHECK status).

### 3.2 Berkas yang berubah

| Berkas | Status | Isi |
| --- | --- | --- |
| `Areas/Corporate/ProcurementManagement/Sourcing/Services/ProcurementPackageApprovalService.cs` | Baru | Ajukan, tarik, buka ulang, sinkron ulang, penerap hasil `Wfl`; kelas `ProcurementSourcingStatusApplier` (`PBJ_PACKAGE`) |
| `Areas/Corporate/ProcurementManagement/Sourcing/Controllers/ProcurementPackagesController.cs` | Diperbarui | `POST /{id}/preparation/submit`, `/withdraw`, `/reopen`; `GET /{id}/approval`; `POST /{id}/workflow/synchronize` |
| `Areas/Corporate/ProcurementManagement/Sourcing/DTOs/PackageDtos.cs` | Diperbarui | `SubmitPackagePreparationRequest`, `WithdrawPackagePreparationRequest`, `ReopenPackagePreparationRequest`; `PackageResponse.PreparationWorkflowInstanceId` |
| `Areas/Corporate/ProcurementManagement/Sourcing/Services/ProcurementPackageService.cs` | Diperbarui | Detail memuat `PreparationWorkflowInstanceId` |
| `Areas/Corporate/ProcurementManagement/Common/Services/ProcurementApprovalTimelineService.cs` | Diperbarui | Cabang `PBJ_PACKAGE` → `PreparationWorkflowInstanceId` |
| `Areas/Corporate/ProcurementManagement/ProcurementManagementServiceCollectionExtensions.cs` | Diperbarui | `ProcurementPackageApprovalService`, `IProcurementWorkflowStatusApplier` → `ProcurementSourcingStatusApplier` |
| `docs/module-blueprints/pengadaan-barang-jasa/00-interview-decisions.md` | Diperbarui | `PBJ-DEC-097` |
| `pemilihan-penyedia/contracts/state-transition-matrix.md`, `integration-contract.md`; `testing/acceptance-test-matrix.md`; `blueprint-manifest.md` | Diperbarui | `PBJ-VAL-091` 409 (versi 1.1) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Lima endpoint baru sesuai kontrak; `PBJ-VAL-091` 409 sesuai `PBJ-DEC-097` (kartu roadmap masih menulis 422, digantikan kontrak 1.1) |
| Database | Tanpa migration. Menulis `PrcPackage` (status, nomor, instance, jumlah pengajuan, HPS disetujui), `PrcPackageItemSource` (dilepas saat ditolak), `PrcPurchaseRequestLine`/`PrcPurchaseRequest` lewat `ReleaseLinesAsync`, `PrcRfq` (dibatalkan saat buka ulang), `PrcDocumentHistory`; deret nomor `PBJ_PACKAGE` |
| Integrasi `Wfl` | Instance dibuat atas nama pengaju lewat `ProcurementApprovalGateway` (kunci `PBJ_PACKAGE:{id}:{jumlah pengajuan}`); nominal = HPS paket termasuk PPN; kondisi `PBJ_METHOD_OUT_OF_RANGE` bila di luar rentang. Penerap status idempoten: hanya paket `PREPARATION_PENDING_APPROVAL` yang tertaut instance itu yang dipindahkan |
| Keamanan/Auth | Pengaju tidak mendapat tugas atas persiapannya sendiri (`AT-PS-16`); tarik hanya oleh pengaju; harga tidak masuk logger |

**Selisih terhadap desain (delta), dicatat apa adanya.**

| # | Desain | Yang dibangun | Alasan |
| ---: | --- | --- | --- |
| D-1 | Kontrak 1.0 dan kartu roadmap: `AT-PS-14` → 422 `PBJ-VAL-091` | 409 `PBJ-VAL-091` | Keputusan pemilik `PBJ-DEC-097` (kontrak 1.1); sama dengan PR, rencana, pagu, vendor |
| D-2 | Arsitektur: `ProcurementSourcingStatusApplier` untuk `PBJ_PACKAGE` dan `PBJ_PACKAGE_AWARD` | Kelas itu melayani `PBJ_PACKAGE`; logikanya di `ProcurementPackageApprovalService` | Interface penerap melayani satu jenis dokumen; penetapan menyusul di `BE-PBJ-PS-013`. Service diambil lewat `IServiceProvider` (pola penerap rencana paket) untuk menghindari lingkaran dependensi dengan `WorkflowService` |
| D-3 | Kontrak: request ajukan `expectedRowVersion` | Body `expectedRowVersion` + header `Idempotency-Key` (opsional, pola PR/rencana) | Tekan-ganda dengan kunci sama mengembalikan hasil pertama tanpa instance kedua |
| D-4 | — | Nomor `PKT` dialokasikan sebelum `Wfl` (pola rencana paket `RPK`) | Nomor dibutuhkan sebagai nomor rujukan di kotak masuk. Akibatnya pengajuan yang ditolak `Wfl` tetap memakan nomor: pada uji, `PKT-000001` terpakai oleh `AT-PS-14` dan paket pertama mendapat `PKT-000002` |
| D-5 | "Tarik: Pengaju" | Pengaju = pemohon instance `Wfl`; pengguna lain → 403; alasan opsional (bawaan "Pengajuan persiapan ditarik pengaju.") | `Wfl` hanya mengizinkan pemohon menarik |
| D-6 | "Buka ulang: RFQ terbuka dibatalkan; riwayat RFQ tetap" | RFQ `DRAFT`/`SENT` → `CANCELLED` dengan riwayat `PBJ_RFQ` `CANCEL`; `ApprovedHpsTotal` dan `PreparationApprovedAt` dipertahankan sampai persetujuan berikutnya menimpanya | Jejak HPS yang pernah disetujui tetap terbaca; RFQ belum dapat dibuat lewat API (`BE-PBJ-PS-009`), sehingga cabang RFQ belum teruji runtime |
| D-7 | — | `Wfl` `Withdrawn`/`Cancelled` → `DRAFT`; `Returned`/`RevisionRequested` → `PREPARATION_RETURNED` | Pemetaan status khusus paket (`ProcurementWorkflowStatusMap` hanya mengenal PR dan rencana) |
| D-8 | "Ditolak → baris PR dilepas" | Semua sumber aktif ditandai `IsReleased`, baris PR dilepas lewat `ReleaseLinesAsync`; barang paket tetap tersimpan; paket `REJECTED` final | Paket yang ditolak tetap dapat dibaca sebagai riwayat |
| D-9 | `PBJ-VAL-125` "metode kosong atau tidak aktif" | Saat diajukan, metode master yang dirujuk harus masih aktif walaupun salinannya tersimpan | Metode yang dinonaktifkan admin tidak dipakai untuk pengajuan baru |
| D-10 | — | `ProcurementApprovalTimelineService` (Common) mengenal `PBJ_PACKAGE` | Linimasa memakai mesin yang sama dengan PR dan rencana |
| D-11 | — | Sinkron ulang paket tanpa instance → 409 `PBJ-VAL-128` | Pola PR memakai kode status untuk keadaan ini |

## 4. Dokumentasi endpoint

### Corporate / Procurement Management / Sourcing / Procurement Package

Base URL: `api/v1/corporate/procurement-management/procurement-packages` — **tersedia**.

| Method | Path | Kegunaan | Hak akses | Request | Response | Kode |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/{id}/preparation/submit` | Ajukan persiapan ke `Wfl`; nomor `PKT` saat pertama | `ProcurementPackage : Submit` | `SubmitPackagePreparationRequest` + header `Idempotency-Key` | `ApiResponse<PackageResponse>` | 200, 404, 409 (`090`, `091`, `128`), 422 (`124`..`127`) |
| `POST` | `/{id}/preparation/withdraw` | Tarik pengajuan yang belum diputus | `ProcurementPackage : Submit` (pengaju) | `WithdrawPackagePreparationRequest` | `ApiResponse<PackageResponse>` | 200, 403, 404, 409 (`090`, `128`) |
| `POST` | `/{id}/preparation/reopen` | Buka ulang persiapan yang sudah disetujui | `ProcurementPackage : Revise` | `ReopenPackagePreparationRequest` | `ApiResponse<PackageResponse>` | 200, 404, 409 (`090`, `128`), 422 (`148`) |
| `GET` | `/{id}/approval` | Linimasa persetujuan persiapan | `ProcurementPackage : Read` | — | `ApiResponse<ProcurementApprovalTimelineResponse>` | 200, 404 |
| `POST` | `/{id}/workflow/synchronize` | Sinkron ulang hasil `Wfl` | `ProcurementPackage : Submit` | — | `ApiResponse<PackageResponse>` | 200, 404, 409 (`128`) |

Contoh ajukan: `POST /{id}/preparation/submit`, header `Idempotency-Key: 8f1c…`, body `{ "expectedRowVersion": "<rowVersion>" }`
→ `status: "PREPARATION_PENDING_APPROVAL"`, `packageNumber: "PKT-000002"`, `preparationWorkflowInstanceId: "<instance>"`.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -c Release --no-incremental` | 0 error, 620 warning (= baseline); 0 warning dari berkas yang diubah | `PASS` | Log build scratchpad sesi |
| QBE Strict, 6 berkas | 0 `VIOLATION`, 0 `REVIEW`, 0 `INFO` | `PASS` | Keluaran checker |
| Uji runtime (`https://localhost:7185` → `QuilvianNewDevSukma`) | 21/21 `PASS` | `PASS` | `ps008.results.json` scratchpad sesi |
| Pembersihan | Hapus lunak satu transaksi; `PBJ-PR` dan `PBJ-PKT-PERSIAPAN` kembali tidak aktif/`Draft`, 0 matriks uji hidup | `PASS` | Bagian 5.2 |

**Fixture (`TEST-PBJ PS008`, lewat API SuperAdmin).** Unit IGD dan Pengadaan; lima akun `Employee` tanpa role: pemohon IGD,
verifikator, penyetuju (`ApprovalInbox : Read, Approve, Reject, RequestRevision` + baca paket), Tim Pengadaan pengaju
(`ProcurementPackage` lengkap + `ApprovalInbox : Read, Approve` untuk uji setujui-sendiri) dan rekan Tim Pengadaan. Tiga PR
IGD (`APPROVED` lewat `Wfl` uji `PBJ-PR`): kursi roda 15, kursi roda 2, jasa kalibrasi 1 (jenis Jasa). Konfigurasi uji
`PBJ-PKT-PERSIAPAN` (dua langkah `SpecificUser` = penyetuju, satu alasan tolak `PBJ_PACKAGE`, bertanda `DATA UJI PBJ PS008`)
dipasang **sesudah** `AT-PS-14`.

### 5.1 Skenario runtime

| ID | Skenario | Hasil | Bukti |
| --- | --- | --- | --- |
| `F-00` | Aksi `ProcurementPackage : Submit` dan `Revise` terdaftar di registry Akses Role | `PASS` | — |
| `F-01` | Tiga PR `APPROVED`, tiga paket `DRAFT` | `PASS` | PR-000032..034 |
| `T-01` | Ajukan tanpa metode → 422 `PBJ-VAL-125` | `PASS` | 422 |
| `T-02` | `AT-PS-10`: HPS satu barang 0 → 422 `PBJ-VAL-124` | `PASS` | 422 |
| `T-03` | Dasar HPS belum dilampirkan → 422 `PBJ-VAL-127` "Dokumen wajib belum lengkap: Dasar Harga Perkiraan Sendiri." | `PASS` | 422 |
| `T-04` | `AT-PS-13`: paket Jasa tanpa KAK/TOR → 422 `PBJ-VAL-127` "… Kerangka Acuan Kerja / TOR." | `PASS` | 422 |
| `T-05` | `AT-PS-14`: persiapan lengkap, definisi `PBJ-PKT-PERSIAPAN` tidak aktif → 409 `PBJ-VAL-091`; paket tetap `DRAFT` tanpa nomor | `PASS` | 409 |
| `T-06` | Ajukan (HPS 42.750.000, PL) → `PREPARATION_PENDING_APPROVAL`, nomor `PKT-000002`, instance satu langkah (`-01`); kunci idempoten sama → 200 instance sama; kunci lain → 409 `PBJ-VAL-128` | `PASS` | 200 / 200 / 409 |
| `T-07` | `GET /approval` terisi untuk instance paket | `PASS` | 200 |
| `T-08` | Ubah HPS saat menunggu persetujuan → 409 `PBJ-VAL-128` | `PASS` | 409 |
| `T-09` | `AT-PS-16`: pengaju menyetujui persiapannya sendiri → 404 "Approval assignment tidak ditemukan pada inbox user login."; tidak ada tugas bagi pengaju; paket tetap menunggu | `PASS` | 404 |
| `T-10` | Tarik oleh rekan → 403; oleh pengaju → 200, `DRAFT`, instance `Withdrawn` | `PASS` | 403 / 200 |
| `T-11` | `AT-PS-15` (setuju): ajukan ulang → instance baru; disetujui → `READY_FOR_RFQ`, `ApprovedHpsTotal` 42.750.000, `PreparationApprovedAt` terisi, pengajuan ke-2, nomor tetap | `PASS` | `PKT-000002` |
| `T-12` | `AT-PS-18`: ubah HPS `READY_FOR_RFQ` → 409 `PBJ-VAL-149`; buka ulang tanpa alasan → 422 `PBJ-VAL-148`; dengan alasan → `DRAFT`; HPS/metode dapat diubah | `PASS` | 409 / 422 / 200 / 200 |
| `T-13` | `AT-PS-12` (`Wfl`): Pembandingan Penawaran di luar rentang (beralasan) → instance dua langkah (`-01`, `-02`); disetujui dua jenjang → `READY_FOR_RFQ` | `PASS` | 2 langkah, 2 persetujuan |
| `T-14` | `AT-PS-15` (kembalikan): penyetuju meminta revisi → `PREPARATION_RETURNED`; persiapan dapat diubah lagi | `PASS` | 200 |
| `T-15` | `AT-PS-15` (tolak): ajukan ulang menutup instance lama (`Cancelled`); ditolak → `REJECTED`; sumber dilepas, baris PR kosong dan siap dipaketkan lagi; PR tetap `APPROVED` | `PASS` | 200 |
| `T-16` | Paket `REJECTED` tidak dapat dibuka ulang → 409 `PBJ-VAL-128` | `PASS` | 409 |
| `T-17` | Sinkron ulang paket bersetuju → 200 tanpa perubahan; paket tanpa alur → 409 `PBJ-VAL-128` | `PASS` | 200 / 409 |
| `T-18` | Riwayat: `SUBMIT`, `WITHDRAW`, `WF_APPROVED` (2×), `REOPEN` tercatat | `PASS` | 12 baris |
| `T-19` | Tanpa `ProcurementPackage : Revise` → 403 | `PASS` | 403 |

### 5.2 Pembersihan

| Tabel | Baris |
| --- | --- |
| `PrcAttachment`, `PrcPackageChecklistItem` | 3, 4 dihapus lunak |
| `PrcPackageItemSource` / `PrcPackageItem` / `PrcPackage` | 3 / 3 / 3 dihapus lunak |
| `PrcPurchaseRequestLine` / `PrcPurchaseRequest` | 3 dilepas lalu dihapus lunak / 3 dihapus lunak |
| `MstProcurementItem`, `SysAccessPolicy`, `MstEmployee`, `MstPosition`, `MstDepartment` | 2, 37, 5, 5, 2 dihapus lunak; kode unit diberi akhiran `-DEL-<run>` |
| `AspNetUsers` | 5 nonaktif + lockout |
| `MstApprovalMatrix`, `MstRejectionReason` uji | Dihapus lunak; `PBJ-PR` dan `PBJ-PKT-PERSIAPAN` kembali `IsActive = false`, `Draft` |

Yang sengaja tidak dihapus: `PrcDocumentHistory`, instance `Wfl` PR dan paket uji, nomor `PR-000032`..`000034` dan
`PKT-000001`..`000002`, tiga berkas fisik lampiran uji di `App_Data`.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-PS-10` HPS 0 → 422 `PBJ-VAL-124` | Terpenuhi | `T-02` |
| `AT-PS-12` langkah tambahan aktif | Terpenuhi | `T-13` (bagian persiapan di `BE-PBJ-PS-007`) |
| `AT-PS-13` Jasa tanpa KAK/TOR → 422 `PBJ-VAL-127` | Terpenuhi | `T-04` |
| `AT-PS-14` definisi tidak aktif → `PBJ-VAL-091`, paket tetap `DRAFT` | Terpenuhi dengan **409** sesuai `PBJ-DEC-097` (kontrak 1.1) | `T-05` |
| `AT-PS-15` disetujui, dikembalikan, ditolak | Terpenuhi untuk API | `T-11`, `T-14`, `T-15`. Bagian "Layar" milik `FE-PBJ-PS-003` |
| `AT-PS-16` pengaju menyetujui sendiri → ditolak | Terpenuhi | `T-09` |
| `AT-PS-18` HPS `READY_FOR_RFQ` → 409 `PBJ-VAL-149`; lewat buka ulang → `DRAFT` | Terpenuhi | `T-12` |
| Cakupan kartu: ajukan/tarik/buka ulang, linimasa, sinkron ulang, penerap `PBJ_PACKAGE`, nomor `PKT` | Terpenuhi | `T-06`..`T-17` |
| Matriks `Wfl` uji dipasang lalu dicabut | Terpenuhi | Bagian 5.2 |
| Laporan | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `PBJ-PKT-PERSIAPAN` di lingkungan mana pun tetap tidak aktif sampai admin mengisi matriks penyetuju dari SOP (`PBJ-OQ-019`); sebelum itu setiap pengajuan dijawab 409 `PBJ-VAL-091` (fail-closed) |
| Masalah yang diketahui | Pengajuan yang ditolak `Wfl` (mis. definisi belum aktif) tetap memakan satu nomor `PKT` (D-4), sama dengan pola `RPK`. Bila nomor tanpa celah dibutuhkan, alokasi perlu dipindah setelah `Wfl` berhasil — keputusan pemilik. Cabang pembatalan RFQ saat buka ulang belum teruji runtime (D-6; `BE-PBJ-PS-009`) |
| Risiko tersisa | Penerap status berjalan di dalam transaksi `Wfl`; bila penerapan gagal, keputusan `Wfl` tetap sah dan paket tertinggal menunggu sampai `workflow/synchronize` dipanggil (pola `BE-PBJ-008` `AT-PP-34`) |
| Perubahan sampingan | `NONE` di source. Di DB uji: fixture dihapus lunak; dua definisi `Wfl` dikembalikan |
| Status Git | Belum di-commit. Baru: `ProcurementPackageApprovalService.cs`, laporan ini. Diubah: `ProcurementPackagesController.cs`, `PackageDtos.cs`, `ProcurementPackageService.cs`, `ProcurementApprovalTimelineService.cs`, DI, `00-interview-decisions.md`, tiga kontrak dan manifest sub-modul, roadmap backend, traceability, `MODULE-STATUS.md`. Ditambah perubahan `BE-PBJ-PS-005`..`007` yang juga belum di-commit |
| Langkah berikutnya | `BE-PBJ-PS-009` (RFQ ke vendor layak: buat, kirim, batas waktu, dokumen RFQ PDF, nomor `RFQ`) — gelombang 8. `FE-PBJ-PS-001`..`003` boleh mulai paralel |
