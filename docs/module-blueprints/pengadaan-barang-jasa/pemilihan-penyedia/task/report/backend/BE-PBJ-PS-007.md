# Laporan Perubahan Backend — `BE-PBJ-PS-007`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-PBJ-PS-007` |
| Judul | Persiapan paket |
| Slice | `MVP-PS1` — `PS-02` persiapan (`04-prd-to-mvp.md` bagian 20) |
| Roadmap | `pemilihan-penyedia/roadmap/backend-roadmap.md` revisi 1, kartu `BE-PBJ-PS-007`, gelombang 6 |
| Trace | `PBJ-DEC-068`, `073`, `074`, `086`; `PBJ-ASM-007`; FR-PS-012, 013; `PBJ-PS-API` `PUT /{id}`, `method-suggestion`, `criteria`, `evaluators`; `PBJ-PS-VAL` `PBJ-VAL-090`, `125`, `126`, `128`, `149`; `AT-PS-11`, `12`, `17`; `UAT-PS-02`; `BE-PBJ-PS-002` D-7 |
| Contract version | Kontrak `PBJ-PS-*` set `1.0` (`approved`, `PBJ-DEC-095`) |
| Dependency | `BE-PBJ-PS-006` ✅ |
| Klasifikasi | `HEAVY` — skor 8: berkas diperiksa 9–20 (1), berkas diubah 4–8 (1), logika tinggi (2: salinan metode, rentang, checklist, kunci status), kontrak API baru (2), keamanan harga (1), menyentuh master (1) |
| Task mode | `BACKEND` (pemilik menjawab "lanjutkan" atas tawaran `BE-PBJ-PS-007`, 27 November 2026) |
| Wewenang database | Tanpa migration. Uji runtime dan SQL langsung (konfigurasi `Wfl` uji, status paket uji sementara, pembersihan) serta pengubahan sementara master metode contoh lewat API atas konfirmasi terpisah pemilik, 27 November 2026: "Ya, jalankan dan bersihkan" |
| Target tulis | `Sourcing/Services/ProcurementPackagePreparationService.cs` (baru), `ProcurementPackageService.cs`, `ProcurementSourcingExceptions.cs`, `Sourcing/DTOs/PackageDtos.cs`, `Sourcing/Controllers/ProcurementPackagesController.cs`, `MasterData/Services/ProcurementMethodService.cs`, DI |
| Model | Claude Opus 5.5 |
| Branch / commit | Backend `sukmagp` @ `de41c752`; perubahan `BE-PBJ-PS-005` dan `006` masih belum di-commit di working tree. Frontend `sukmagpV2` @ `3e5386679` |
| Tanggal | 27 November 2026 |
| Status | ✅ **SELESAI.** Persiapan paket dapat diubah (judul, spesifikasi, HPS, metode, alasan), metode disarankan dan disalin beserta checklist, kriteria teknis dan evaluator dapat ditetapkan, HPS terkunci di luar persiapan; metode yang dipakai paket tidak dapat dihapus. Build Release 0 error; QBE Strict `PASS` 7 berkas; uji runtime 20/20 `PASS`; fixture dibersihkan |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `ProcurementManagement` / `Sourcing`; `MasterData` (pemeriksaan hapus metode) |
| Registry | `ProcurementManagement / Sourcing · Prc · ACTIVE` (`PBJ-DEC-096`); `MasterData` memakai baris `Mst` yang sudah `ACTIVE` |
| Keberlakuan | `NEW CODE` untuk `ProcurementPackagePreparationService.cs` dan DTO baru; `TOUCHED LEGACY` untuk `ProcurementPackageService.cs`, `ProcurementPackagesController.cs`, `ProcurementSourcingExceptions.cs`, `ProcurementMethodService.cs`, DI |
| Arketipe endpoint | Transaksi — aggregate ber-lifecycle: `PUT /{id}` (suntingan isi selama persiapan), sub-resource `PUT /{id}/items/{itemId}/criteria` dan `PUT /{id}/evaluators`, baca `GET /{id}/method-suggestion`. Tanpa `PATCH /{id}/status` dan tanpa `DELETE /{id}` |
| Akses | `[AccessAction]` + `[AccessPermission]` pada setiap action: `ProcurementPackage : Read` (saran metode), `Update` (persiapan, kriteria, evaluator). Kewenangan data: cakupan unit (`ReadAllUnits`), ubah HPS butuh `ReadPrice`. Tanpa role hardcode |
| QBE yang berlaku | `QBE-NAM-001`, `QBE-NAM-002`, `QBE-MOD-001..003`, `QBE-SVC-001`, `QBE-CODE-004`, `QBE-DEL-001` |
| Hasil checker | QBE Strict `PASS` (7 berkas, 0 temuan) |

## 1. Masalah yang diselesaikan

Paket draf sudah berisi barang dari PR, tetapi Tim Pengadaan belum bisa menyiapkannya untuk diajukan. Task ini membangun
persiapan itu. Contoh `AT-PS-11`: paket 15 kursi roda, HPS satuan diubah dari harga PR Rp 2.800.000 menjadi Rp 2.850.000,
sehingga HPS paket Rp 42.750.000. Sistem menyarankan "Penunjukan Langsung (contoh)" (≤ Rp 50.000.000). Saat metode itu
dipilih, aturannya disalin ke paket, beserta checklist dokumen wajibnya (Dasar HPS). Bila petugas memilih metode yang
rentangnya tidak cocok, misalnya Pembandingan Penawaran (> Rp 50.000.000), alasan wajib diisi.

## 2. Aturan yang dibangun

| Aturan | Bila dilanggar | Contoh |
| --- | --- | --- |
| Persiapan hanya dapat diubah saat `DRAFT`/`PREPARATION_RETURNED` | 409 `PBJ-VAL-149` bila persiapan sudah disetujui (`READY_FOR_RFQ` dst.), 409 `PBJ-VAL-128` untuk status lain | Ubah HPS paket `READY_FOR_RFQ` → "HPS sudah disetujui. Buka ulang persiapan untuk mengubahnya." |
| Metode yang dipilih harus aktif | 422 `PBJ-VAL-125` | Memilih metode yang sudah dinonaktifkan |
| HPS di luar rentang salinan metode (`Min < HPS ≤ Max`) wajib beralasan | 422 `PBJ-VAL-126` | HPS Rp 42.750.000 dengan Pembandingan Penawaran tanpa alasan |
| Metode dan aturannya disalin saat dipilih | — | Master Penunjukan Langsung diturunkan ke Rp 45.000.000 → paket tetap mencatat Rp 50.000.000 (`AT-PS-17`) |
| Checklist = dokumen wajib metode ∪ dokumen wajib jenis pengadaan, disalin saat metode dipilih | — | Barang Stok + Penunjukan Langsung → Dasar HPS |
| Checklist terpenuhi bila ada lampiran paket berlaku berkategori kode jenis dokumennya | — | Unggah `DASAR_HPS` → terpenuhi |
| Mengubah HPS butuh `ProcurementPackage : ReadPrice` | 403 | Staf yang tidak melihat harga mencoba mengubah HPS |
| Versi data harus cocok | 409 `PBJ-VAL-090` | Dua petugas menyunting bersamaan |
| Metode yang dipakai paket tidak dapat dihapus | 400 `PBJ-VAL-071` (`UAT-PS-02`) | "Metode ini tidak dapat dihapus karena sudah dipakai paket pengadaan." |

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu task; `api-contract.md` §Procurement Package; `validation-matrix.md` bagian 1–2; `state-transition-matrix.md` bagian 1;
kamus data `PrcPackage`, `PrcPackageItem`, `PrcPackageChecklistItem`, `PrcPackageEvaluator`, `PrcTechnicalCriterion`, master
metode dan dokumen wajib; `flowcharts/persiapan-paket.md`; `acceptance-test-matrix.md` (`AT-PS-10`..`18`);
`00-interview-decisions.md` `PBJ-DEC-068`, `073`, `074`, `086`, `PBJ-ASM-007`. Source: model dan konfigurasi Sourcing (index unik
checklist, evaluator, kriteria; CHECK alasan metode), `ProcurementMethodService` (rentang, hapus), `ProcurementSourcingMasterSeeder`
(metode contoh dan dokumen wajib), `ProcurementPackageService`, `ProcurementPackagesController`.

### 3.2 Berkas yang berubah

| Berkas | Status | Isi |
| --- | --- | --- |
| `Areas/Corporate/ProcurementManagement/Sourcing/Services/ProcurementPackagePreparationService.cs` | Baru | Ubah persiapan, saran metode, salin metode dan checklist, kriteria teknis, evaluator, kunci status |
| `Areas/Corporate/ProcurementManagement/Sourcing/Services/ProcurementPackageService.cs` | Diperbarui | Detail paket memuat checklist (dengan status terpenuhi), evaluator aktif, dan kriteria per barang; status checklist jenis harga ditutup tanpa `ReadPrice` |
| `Areas/Corporate/ProcurementManagement/Sourcing/Services/ProcurementSourcingExceptions.cs` | Diperbarui | `ProcurementSourcingForbiddenException` (403) |
| `Areas/Corporate/ProcurementManagement/Sourcing/DTOs/PackageDtos.cs` | Diperbarui | `UpdatePackagePreparationRequest`, `UpdatePackageItemPreparationRequest`, `MethodSuggestionResponse`, `MethodSuggestionItem`, `UpdateTechnicalCriteriaRequest`, `TechnicalCriterionRequest`, `UpdateEvaluatorsRequest`, `PackageChecklistItemResponse`, `PackageEvaluatorResponse`, `PackageCriterionResponse`; `PackageResponse.Checklist`/`Evaluators`, `PackageItemResponse.Criteria` |
| `Areas/Corporate/ProcurementManagement/Sourcing/Controllers/ProcurementPackagesController.cs` | Diperbarui | Empat endpoint baru; pemetaan 403 |
| `Areas/Corporate/ProcurementManagement/MasterData/Services/ProcurementMethodService.cs` | Diperbarui | Hapus metode yang dipakai paket → 400 `PBJ-VAL-071` |
| `Areas/Corporate/ProcurementManagement/ProcurementManagementServiceCollectionExtensions.cs` | Diperbarui | `AddScoped<ProcurementPackagePreparationService>()` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Empat endpoint baru sesuai kontrak; detail paket bertambah `checklist`, `evaluators`, `items[].criteria` (aditif) |
| Database | Tanpa migration dan tanpa perubahan skema. Menulis `PrcPackage` (salinan metode, HPS, alasan), `PrcPackageItem`, `PrcPackageChecklistItem`, `PrcTechnicalCriterion`, `PrcPackageEvaluator`, `PrcDocumentHistory` |
| Keamanan/Auth | HPS hanya dapat diubah pemegang `ReadPrice`; saran metode dan detail menutup HPS bagi yang lain; status checklist jenis harga `null` bagi yang lain. Cuplikan riwayat memuat HPS lama → baru (`PBJ-PS-PERM` bagian 5), tetapi riwayat API tidak mengembalikan cuplikan dan logger tidak memuat harga |

**Selisih terhadap desain (delta), dicatat apa adanya.**

| # | Desain | Yang dibangun | Alasan |
| ---: | --- | --- | --- |
| D-1 | Arsitektur: persiapan di `ProcurementPackageService` | Service terpisah `ProcurementPackagePreparationService`, controller yang sama | `ProcurementPackageService` sudah ~800 baris; tanggung jawab persiapan berdiri sendiri. Cakupan dan bentuk respons tetap dari `ProcurementPackageService` |
| D-2 | Kontrak: `UpdatePackagePreparationRequest` belum rinci | `items` hanya memuat barang yang diubah; `hpsUnitPrice` kosong = tidak diubah; `methodId` kosong = metode dikosongkan | Pengguna tanpa `ReadPrice` dapat menyunting judul/spesifikasi tanpa mengirim HPS yang tidak ia lihat |
| D-3 | — | Mengubah HPS tanpa `ReadPrice` → 403 | Kewenangan melekat pada data (`PBJ-DEC-079`): tidak boleh menimpa angka yang tidak boleh dilihat |
| D-4 | `PBJ-VAL-125` tercatat pada "Ajukan persiapan" | Juga saat `PUT` memilih metode baru yang tidak aktif/tidak ada. Metode yang tidak berganti tidak diperiksa ulang | Salinan sudah tersimpan (`PBJ-DEC-068`); metode yang dinonaktifkan sesudah dipilih tidak membatalkan paket. Pengajuan (`BE-PBJ-PS-008`) tetap memeriksa `PBJ-VAL-125` |
| D-5 | `PBJ-VAL-126` pada "Ajukan persiapan, ubah metode" | Diperiksa pada setiap `PUT`, termasuk bila hanya HPS yang berubah; alasan dikosongkan bila HPS kembali dalam rentang | CHECK `CK_PrcPackage_MethodDeviationReason` mewajibkan alasan selama di luar rentang |
| D-6 | Checklist disalin saat metode dipilih | Dibangun ulang saat metode berganti (juga saat dikosongkan), atau saat paket belum punya checklist; butir yang tetap wajib dipertahankan beserta salinan namanya, yang tidak lagi wajib dihapus lunak | Checklist ikut salinan metode; jenis pengadaan tetap masuk walau metode dikosongkan |
| D-7 | — | `checklist[].isPriceSensitive`; `isFulfilled` `null` untuk jenis harga tanpa `ReadPrice` | Keberadaan lampiran harga tidak dibocorkan (konsisten dengan `BE-PBJ-PS-006`) |
| D-8 | — | Kriteria ditimpa per nomor butir (ubah teks), kelebihan dihapus lunak; maksimal 50 butir, teks ≤ 500 | Menghindari bentrok index unik `(PackageItemId, CriterionNo)`; nomor tetap 1, 2, 3, … |
| D-9 | — | Evaluator: daftar pengganti; yang dilepas dinonaktifkan (bukan dihapus); pengguna wajib aktif (400); maksimal 20 | Riwayat penugasan tetap ada; izin `ProcurementTechnicalEvaluation` tetap diatur admin di Akses Role |
| D-10 | — | Kriteria dan evaluator di luar persiapan → 409 `PBJ-VAL-128` | Bukan HPS/metode, sehingga bukan `PBJ-VAL-149`; perubahan sesudah disetujui lewat buka ulang (`BE-PBJ-PS-008`) |
| D-11 | Riwayat "ubah HPS/metode" | Aksi `UPDATE` untuk persiapan, kriteria, dan evaluator | Tidak ada kode aksi khusus di `PBJ-PS-PERM` bagian 5 |
| D-12 | — | Saran metode memuat semua metode aktif yang rentangnya mencakup HPS; `isCurrentMethodInRange` memakai salinan paket | Lebih dari satu metode dapat cocok bila rentang bersinggungan di batas; master melarang tumpang tindih |
| D-13 | — | Pemeriksaan "metode dipakai paket" menghitung semua paket yang belum dihapus, termasuk yang batal/ditolak | Rujukan `MethodId` tetap utuh untuk riwayat |
| D-14 | Kartu: "salinan checklist dari dokumen wajib metode dan jenis" | Sesuai; data awal Barang Stok tidak punya dokumen wajib jenis, sehingga checklist uji hanya Dasar HPS | Data awal `BE-PBJ-PS-001` (KAK/TOR untuk Jasa, Spesifikasi Teknis untuk Aset) |

## 4. Dokumentasi endpoint

### Corporate / Procurement Management / Sourcing / Procurement Package

Base URL: `api/v1/corporate/procurement-management/procurement-packages` — **tersedia**.

| Method | Path | Kegunaan | Hak akses | Request | Response | Kode |
| --- | --- | --- | --- | --- | --- | --- |
| `PUT` | `/{id}` | Ubah judul, spesifikasi dan HPS satuan barang, metode, alasan di luar rentang | `ProcurementPackage : Update` (+ `ReadPrice` untuk HPS) | `UpdatePackagePreparationRequest` | `ApiResponse<PackageResponse>` | 200, 400, 403, 404, 409 (`090`, `128`, `149`), 422 (`125`, `126`) |
| `GET` | `/{id}/method-suggestion` | Metode aktif yang rentangnya mencakup HPS paket | `ProcurementPackage : Read` | — | `ApiResponse<MethodSuggestionResponse>` | 200, 404 |
| `PUT` | `/{id}/items/{itemId}/criteria` | Ganti kriteria teknis satu barang | `ProcurementPackage : Update` | `UpdateTechnicalCriteriaRequest` | `ApiResponse<PackageResponse>` | 200, 400, 404, 409 (`090`, `128`) |
| `PUT` | `/{id}/evaluators` | Ganti evaluator aktif paket | `ProcurementPackage : Update` | `UpdateEvaluatorsRequest` | `ApiResponse<PackageResponse>` | 200, 400, 404, 409 (`090`, `128`) |

Contoh `PUT /{id}`:

```json
{ "expectedRowVersion": "<rowVersion>", "title": "Kursi Roda 2027",
  "methodId": "<Penunjukan Langsung (contoh)>", "methodDeviationReason": null,
  "items": [{ "packageItemId": "<kursi roda>", "specification": "Kursi roda standar, rangka baja", "hpsUnitPrice": 2850000 }] }
```

Respons ringkas: `hpsTotal: 42750000.00`, `methodCode: "PENUNJUKAN_LANGSUNG_CONTOH"`, `methodMaxHpsAmount: 50000000.00`,
`isMethodOutOfRange: false`, `checklist: [{ documentTypeCode: "DASAR_HPS", isFulfilled: false }]`.

### Corporate / Procurement Management / Master Data / Procurement Method

`DELETE /master-data/procurement-methods/{id}` — perilaku berubah: metode yang dipakai paket → 400 `PBJ-VAL-071`.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -c Release --no-incremental` | 0 error, 620 warning (= baseline); 0 warning dari berkas yang diubah | `PASS` | Log build scratchpad sesi |
| QBE Strict, 7 berkas | 0 `VIOLATION`, 0 `REVIEW`, 0 `INFO` | `PASS` | Keluaran checker |
| Uji runtime (`https://localhost:7185` → `QuilvianNewDevSukma`) | 20/20 `PASS` | `PASS` | `ps007.results.json` scratchpad sesi |
| Pembersihan | Hapus lunak satu transaksi; `PBJ-PR` kembali tidak aktif/`Draft`; batas PL `50.000.000,00` | `PASS` | Bagian 5.2 |

**Fixture (`TEST-PBJ PS007`, lewat API SuperAdmin).** Unit IGD dan Pengadaan; tujuh akun `Employee` tanpa role: pemohon
IGD, staf IGD (`ProcurementPackage : Read, Create, Update` + lampiran, tanpa `ReadPrice`), dua evaluator (hanya
`ProcurementAttachment : Read`), verifikator, penyetuju, Tim Pengadaan (`ProcurementPackage` + `ReadAllUnits` + `ReadPrice`,
lampiran, master metode). Dua PR IGD (15 dan 2 kursi roda @ Rp 2.800.000) dibawa ke `APPROVED` lewat `Wfl` uji, lalu
dipaketkan.

### 5.1 Skenario runtime

| ID | Skenario | Hasil | Bukti |
| --- | --- | --- | --- |
| `F-01` | Fixture: dua PR `APPROVED`; dua paket `DRAFT`; HPS awal paket 1 Rp 42.000.000 | `PASS` | PR-000030, PR-000031 |
| `T-01` | Detail paket baru: tanpa metode, checklist, evaluator, kriteria | `PASS` | 200 |
| `T-02` | `PUT`: HPS 2.850.000 × 15 = 42.750.000; PL tersalin (≤ 50 juta, min 1 penawaran, tanpa evaluasi teknis); checklist `DASAR_HPS` belum terpenuhi | `PASS` | 200 |
| `T-03` | `AT-PS-11`: saran untuk HPS 42.750.000 = "Penunjukan Langsung (contoh)"; metode sekarang dalam rentang | `PASS` | 200 |
| `T-04` | Saran bagi staf tanpa `ReadPrice`: `hpsTotal` null, saran tetap ada | `PASS` | 200 |
| `T-05` | `AT-PS-12`: Pembandingan tanpa alasan → 422 `PBJ-VAL-126`; dengan alasan → 200, di luar rentang, salinan min 3 penawaran + evaluasi teknis | `PASS` | 422 / 200 |
| `T-06` | `rowVersion` basi → 409 `PBJ-VAL-090` | `PASS` | 409 |
| `T-07` | Metode tak dikenal 422 `PBJ-VAL-125`; barang tak dikenal 400; HPS negatif 400; judul kosong 400 | `PASS` | 422 / 400 / 400 / 400 |
| `T-08` | Staf tanpa `ReadPrice`: ubah HPS 403; ubah judul saja 200, HPS tetap tertutup | `PASS` | 403 / 200 |
| `T-09` | HPS tidak berubah oleh suntingan staf | `PASS` | 42.750.000 |
| `T-10` | Unggah `DASAR_HPS` → checklist terpenuhi bagi Tim Pengadaan, `null` bagi staf | `PASS` | 200 |
| `T-11` | Kembali ke PL (dalam rentang): tidak di luar rentang, alasan dikosongkan | `PASS` | 200 |
| `T-12` | `AT-PS-17`: master PL diubah 50 → 45 juta; salinan paket tetap 50 juta di detail, sesudah `PUT` ulang, dan di DB; tetap dalam rentang | `PASS` | DB `50000000.00` |
| `T-13` | `UAT-PS-02`: metode uji (45–50 juta) dipakai paket 2 (checklist kosong untuk Barang Stok) → hapus 400 `PBJ-VAL-071`; nonaktif 200; paket lain memilih metode nonaktif → 422 `PBJ-VAL-125`; paket 2 tetap dapat disimpan dengan salinannya; master PL dikembalikan 50 juta | `PASS` | 400 / 200 / 422 / 200 / 200 |
| `T-14` | Kriteria: 3 butir bernomor 1–3; ganti 2 butir → 1–2, butir ke-3 dihapus lunak (DB: 2 hidup, 1 terhapus); butir kosong 400; barang tak dikenal 404; basi 409 | `PASS` | 200 / 200 / 400 / 404 / 409 |
| `T-15` | Evaluator: [A, B, A] → dua aktif; ganti [B] → A nonaktif; pengguna tak dikenal 400 | `PASS` | 200 / 200 / 400 |
| `T-16` | Cabang evaluator `BE-PBJ-PS-006`: evaluator aktif (hanya `ProcurementAttachment : Read`) membaca lampiran paket — KAK/TOR saja, Dasar HPS tersembunyi; detail paket 403; sesudah dilepas → 404 | `PASS` | 200 (1 lampiran) / 403 / 404 |
| `T-17` | Status terkunci (diset lewat SQL uji): `READY_FOR_RFQ` → `PUT` 409 `PBJ-VAL-149`, kriteria/evaluator 409 `PBJ-VAL-128`; `PREPARATION_PENDING_APPROVAL` → `PUT` 409 `PBJ-VAL-128` | `PASS` | 409 |
| `T-18` | Riwayat mencatat `UPDATE` untuk persiapan, kriteria, evaluator | `PASS` | 9× `UPDATE` |
| `T-19` | Regresi daftar paket memuat metode terpilih | `PASS` | "Penunjukan Langsung (contoh)" |

### 5.2 Pembersihan

| Tabel | Baris |
| --- | --- |
| `PrcAttachment`, `PrcTechnicalCriterion`, `PrcPackageChecklistItem`, `PrcPackageEvaluator` | 2, 2, 1, 2 dihapus lunak |
| `PrcPackageItemSource` / `PrcPackageItem` / `PrcPackage` | 2 / 2 / 2 dihapus lunak |
| `MstProcurementMethod` (metode uji) | 1 dihapus lunak; metode contoh PL tetap `MaxHpsAmount = 50.000.000,00` |
| `PrcPurchaseRequestLine` / `PrcPurchaseRequest` | 2 dilepas lalu dihapus lunak / 2 dihapus lunak |
| `MstProcurementItem`, `SysAccessPolicy`, `MstEmployee`, `MstPosition`, `MstDepartment` | 1, 29, 7, 7, 2 dihapus lunak; kode unit diberi akhiran `-DEL-<run>` |
| `AspNetUsers` | 7 nonaktif + lockout |
| `MstApprovalMatrix` uji | Dihapus lunak; `PBJ-PR` kembali `IsActive = false`, `Draft` |

Yang sengaja tidak dihapus: `PrcDocumentHistory`, instance `Wfl` PR uji, nomor `PR-000030`..`000031`, dua berkas fisik lampiran
uji di `App_Data`. Master PL sempat bernilai 45 juta di antara `T-12` dan `T-13`, lalu dikembalikan lewat API dan diperiksa ulang saat pembersihan.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-PS-11` HPS Rp 42.750.000 → saran Penunjukan Langsung (contoh) | Terpenuhi | `T-02`, `T-03` |
| `AT-PS-12` 422 `PBJ-VAL-126` tanpa alasan | Terpenuhi untuk bagian persiapan | `T-05`. Bagian "kondisi `PBJ_METHOD_OUT_OF_RANGE` aktif, langkah tambahan dibuat" milik pengajuan `Wfl` (`BE-PBJ-PS-008`, kartu `AT-PS-12` kondisi) |
| `AT-PS-17` ubah master tidak mengubah salinan | Terpenuhi | `T-12` |
| `UAT-PS-02` hapus metode yang dipakai paket → 400 `PBJ-VAL-071` (`BE-PBJ-PS-002` D-7) | Terpenuhi | `T-13` |
| Cakupan kartu: `PUT /{id}`, saran metode, salinan metode, salinan dan status checklist, kriteria, evaluator, HPS terkunci 409 `PBJ-VAL-149` | Terpenuhi | `T-02`..`T-17` |
| Laporan | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `PUT /{id}` memakai semantik penggantian untuk `methodId`: mengirim `null` mengosongkan metode. Layar persiapan wajib selalu mengirim metode yang sedang terpilih |
| Masalah yang diketahui | Kamus data `PrcTechnicalCriterion` masih menyebut `SortOrder` (kode memakai `CriterionNo`, `BE-PBJ-PS-004` D-1). Status `PBJ-VAL-091` (409 di `PBJ-PP-VAL`, 422 di `PBJ-PS-VAL`) wajib diputuskan sebelum `BE-PBJ-PS-008` |
| Bukti tambahan | Cabang evaluator `BE-PBJ-PS-006` (lampiran) kini terbukti runtime (`T-16`); kartu `BE-PBJ-PS-006` semula menundanya ke `BE-PBJ-PS-011` |
| Risiko tersisa | Kunci `PBJ-VAL-149` dibuktikan dengan status yang diset lewat SQL; jalur status sebenarnya lahir di `BE-PBJ-PS-008` |
| Perubahan sampingan | `NONE` di source. Di DB uji: fixture dihapus lunak; `Wfl` uji dicabut; master PL dikembalikan |
| Status Git | Belum di-commit. Baru: `ProcurementPackagePreparationService.cs`, laporan ini. Diubah: `ProcurementPackageService.cs`, `ProcurementSourcingExceptions.cs`, `PackageDtos.cs`, `ProcurementPackagesController.cs`, `ProcurementMethodService.cs`, DI, roadmap backend, traceability, `MODULE-STATUS.md`. Ditambah perubahan `BE-PBJ-PS-005`, `006` yang juga belum di-commit |
| Langkah berikutnya | `BE-PBJ-PS-008` (persetujuan persiapan lewat `Wfl`: ajukan/tarik/buka ulang, penerap `PBJ_PACKAGE`, kondisi `PBJ_METHOD_OUT_OF_RANGE`, nomor `PKT`) — gelombang 7. **Butuh keputusan `PBJ-VAL-091` (409 atau 422) lebih dulu.** `FE-PBJ-PS-001` boleh paralel |
