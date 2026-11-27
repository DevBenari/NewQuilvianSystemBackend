# Manajemen Vendor — Kontrak API

| Field | Nilai |
| --- | --- |
| Kontrak | `PBJ-VD-API` · `last_changed_in: 1.1` (`PBJ-DEC-064`, 2026-11-19) · status `approved` (`PBJ-DEC-062`) |
| Pemilik | Pengguna sesi · `approved_by: Pengguna sesi` · `approved_at: 2026-11-17`; versi 1.1 disahkan 2026-11-19 |
| Masukan | `02-backend-architecture.md` (approved); `PBJ-VD-STATE` 1.1; `PBJ-VD-VAL` 1.1; `PBJ-PP-API` 1.4 (konvensi bersama); laporan `BE-PBJ-VD-001`..`008` (delta D-x) |
| Kompatibilitas | Seluruh endpoint Pengadaan baru. Satu perubahan perilaku pada endpoint lama: Master Data Supplier berhenti menulis status blokir (`PBJ-DEC-060`). Perubahan 1.1 bersifat aditif atau memperjelas perilaku; tidak ada field yang dihapus atau diganti namanya |

**Status endpoint (1.1):** seluruh endpoint di dokumen ini **sudah tersedia** di backend (`BE-PBJ-VD-002`..`008`) dan
diuji di `QuilvianNewDevSukma` (`AT-VD-01`..`64`). Baris bertanda **(1.1, …)** adalah perilaku yang dibangun lalu
disahkan `PBJ-DEC-064`, dengan rujukan delta di laporan task (mis. `VD-003 D-9` = laporan `BE-PBJ-VD-003` delta D-9).

Konvensi bersama mengikuti `perencanaan-permintaan/contracts/api-contract.md`: `ApiResponse<T>`, `PagedResult<T>`,
`Idempotency-Key` pada aksi status, `expectedRowVersion` pada perubahan, kode 400/401/403/404/409/422, serta saringan
tanggal daftar `startDate`/`endDate`/`customPeriod` (`PBJ-DEC-055`, basis tanggal data dibuat). Base URL:
`api/v1/corporate/procurement-management`. Kunci vendor di URL adalah `supplierId` (`MstSupplier.Id`).

**Konvensi galat (1.1, `VD-002 D-8`):** isian yang gagal anotasi DTO (`[Required]`, `[MaxLength]`, `[Range]`) dijawab
**400** `ValidationProblemDetails` sebelum aturan bisnis berjalan. Aturan bisnis tanpa kode khusus dijawab 422 tanpa
`errors.code`; daftarnya ada di `validation-matrix.md` bagian 2.

---

### Corporate / Procurement Management / Vendor / Vendor

Base URL: `/vendors` — tersedia (`BE-PBJ-VD-003`, `005`, `006`).

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan saringan: periode, urutan (`code`/`name`/`status`), kategori, status vendor, keadaan dokumen. **(1.1)** `vendorStatusOptions` menambah `NON_ACTIVE` "Semua selain Aktif" (`VD-003 D-10`); `bankOptions` = nama bank unik dari Master Data (`VD-003 D-9`) | `ProcurementVendor : Read` | — | `VendorFilterMetadataResponse` |
| `GET` | `/summary` | Total, Aktif, Dibatasi, Diblokir, Nonaktif. **(1.1)** `expiredDocuments`/`expiringSoonDocuments` menghitung **dokumen** terverifikasi terakhir per jenis, ditambah `expiryWarningDays` (`VD-003 D-3`) | `ProcurementVendor : Read` | — | `VendorSummaryResponse` |
| `GET` | `/` | Daftar vendor. **(1.1)** Semua supplier yang belum dihapus tampil, termasuk yang nonaktif di Master Data dengan status `INACTIVE` (`VD-003 D-1`). Saringan: `itemCategoryId`, `vendorStatus` (`ACTIVE`/`RESTRICTED`/`BLOCKED`/`INACTIVE`/`NON_ACTIVE` = semua selain Aktif, `VD-003 D-10`), `documentState` (`EXPIRED`/`EXPIRING_SOON`/`PENDING`/`VALID`/`NONE`, `VD-003 D-2`), `bankName` (tanpa beda huruf, `VD-003 D-9`), tanggal, `search` kode/nama/nama resmi. Setiap baris membawa `bankName`, `maskedBankAccount`, `bankAccountName` (`VD-003 D-9`) | `ProcurementVendor : Read` | `VendorQuery` | `PagedResult<VendorListItem>` |
| `GET` | `/{supplierId}` | Detail: identitas baca saja dari Master Data (nama resmi, jenis, NPWP, NIB, kontak, alamat; `VD-003 D-7`), status, keterangan Dibatasi, kategori, ringkasan dokumen, rekening tersamarkan, penilaian final terakhir per kategori, `rowVersion` profil | `ProcurementVendor : Read` | — | `VendorDetailResponse` |
| `PUT` | `/{supplierId}/categories` | Atur kategori vendor; profil dibuat bila belum ada. **(1.1)** `expectedRowVersion` wajib bila profil sudah ada, diabaikan saat profil pertama dibuat (`VD-003 D-4`); kategori yang **ditambahkan** harus aktif, kategori lama yang kini nonaktif boleh dipertahankan (`VD-003 D-5`); simpan tanpa perubahan → 200 tanpa riwayat (`VD-003 D-6`) | `ProcurementVendor : Update` | `UpdateVendorCategoriesRequest` | `VendorDetailResponse` |
| `POST` | `/{supplierId}/restriction` | Tetapkan Dibatasi (`PBJ-DEC-047`). **(1.1)** Vendor yang sedang Dibatasi tidak bisa dibatasi ulang: cabut dulu, lalu batasi lagi → 409 `PBJ-VAL-093` (`VD-006 D-1`, aturan bisnis `PBJ-DEC-064`); `expectedRowVersion` wajib bila profil sudah ada (`VD-006 D-2`); vendor nonaktif di Master Data tetap boleh dibatasi dan tetap tampil Nonaktif (`VD-006 D-4`) | `ProcurementVendor : Restrict` | `RestrictVendorRequest` `{ reason, restrictedUntil?, expectedRowVersion? }` + `Idempotency-Key` | `VendorDetailResponse` |
| `POST` | `/{supplierId}/restriction/lift` | Cabut Dibatasi. **(1.1)** Mengosongkan alasan, tanggal berakhir, waktu, dan pelaku pembatasan di profil; jejaknya tetap di riwayat (`VD-006 D-3`) | `ProcurementVendor : Restrict` | `LiftRestrictionRequest` `{ reason, expectedRowVersion }` | `VendorDetailResponse` |
| `GET` | `/{supplierId}/history` | Riwayat vendor (`PBJ-ASM-004`) | `ProcurementVendor : Read` | — | `List<DocumentHistoryResponse>` |
| `GET` | `/eligibility` | Kelayakan vendor untuk satu kategori; dipakai pemilihan vendor. **(1.1)** `itemCategoryId` wajib (kosong → 400; tak dikenal → 404), `supplierId` opsional, `onlyEligible` (bawaan `false`); tanpa `supplierId` mengembalikan semua vendor yang melayani kategori itu (`VD-005 D-2`, `D-4`). Kategori nonaktif → semua vendor tidak layak (`VD-005 D-3`, aturan bisnis `PBJ-DEC-064`) | `ProcurementVendor : Read` | `VendorEligibilityQuery` | `List<VendorEligibilityResponse>` |

**Contoh respons kelayakan (1.1, `VD-005 D-1`):**

```json
{ "supplierId": "…", "supplierCode": "SUP-RSMMC-00023", "supplierName": "PT Sehat Farma",
  "itemCategoryId": "…", "isEligible": false, "isRestricted": false,
  "reasons": ["Dokumen wajib Izin PBF kedaluwarsa sejak 01-04-2027."],
  "warnings": ["Dokumen wajib Izin Distribusi Alkes berakhir dalam 20 hari (21-04-2027)."] }
```

`warnings` berisi catatan yang **tidak** menggugurkan kelayakan: alasan Dibatasi dan dokumen wajib yang segera kedaluwarsa.

**Rekening tersamarkan:** `maskedBankAccount = "****1234"`, `bankName`, `bankAccountName`; `bankAccountVerification =
"Tidak tersedia di Master Data"`. Nomor utuh tidak pernah dikirim, baik di detail maupun daftar (`PBJ-DEC-015`).

**Contoh saringan (1.1):** `GET /vendors?vendorStatus=NON_ACTIVE&bankName=Bank%20Mandiri` → vendor Dibatasi, Diblokir,
atau Nonaktif yang rekeningnya di Bank Mandiri; nilai `vendorStatus` lain yang tidak dikenal → 422 "Status vendor 'X'
tidak dikenal."

### Corporate / Procurement Management / Vendor / Vendor Document

Base URL: `/vendor-documents` — tersedia (`BE-PBJ-VD-004`). Berkas diunggah lewat endpoint lampiran yang sudah ada
(`POST /attachments`, `documentType = PBJ_VENDOR_DOCUMENT`, `documentId = id dokumen`).

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Jenis dokumen (beserta `requiresExpiryDate`, `isActive`), status verifikasi, keadaan berlaku, periode, `expiryWarningDays` | `ProcurementVendorDocument : Read` | — | `VendorDocumentFilterMetadataResponse` |
| `GET` | `/summary` | Menunggu verifikasi, terverifikasi, ditolak, kedaluwarsa, segera kedaluwarsa. **(1.1)** Bisa disaring `supplierId` (`VD-004 D-7`) | `ProcurementVendorDocument : Read` | `supplierId?` | `VendorDocumentSummaryResponse` |
| `GET` | `/` | Daftar dokumen; saringan vendor, jenis, status verifikasi, keadaan berlaku, tanggal. **(1.1)** Keadaan berlaku bertambah `REPLACED` (Digantikan) untuk dokumen terverifikasi yang sudah punya pengganti (`VD-004 D-1`); saringan keadaan berlaku hanya mengenai dokumen terverifikasi; setiap baris membawa `daysUntilExpiry` dan `fileCount` (`VD-004 D-7`) | `ProcurementVendorDocument : Read` | `VendorDocumentQuery` | `PagedResult<VendorDocumentListItem>` |
| `GET` | `/{id}` | Detail dokumen beserta berkas terkini, pengunggah, pemeriksa | `ProcurementVendorDocument : Read` | — | `VendorDocumentResponse` |
| `POST` | `/` | Catat dokumen baru berstatus `PENDING`. **(1.1)** Vendor tanpa profil → profil dibuat sekaligus beserta riwayat status blokir awal (`VD-004 D-4`); jenis nonaktif atau vendor tak dikenal → 422 tanpa kode (`VD-004 D-5`) | `ProcurementVendorDocument : Create` | `CreateVendorDocumentRequest` | `VendorDocumentResponse` (201) |
| `POST` | `/{id}/verify` | Tandai Terverifikasi; ditolak bila pemeriksa = pengunggah. **(1.1)** `expectedRowVersion` wajib (`VD-004 D-2`) | `ProcurementVendorDocument : Verify` | `{ expectedRowVersion }` + `Idempotency-Key` | `VendorDocumentResponse` |
| `POST` | `/{id}/reject` | Tandai Ditolak dengan alasan. **(1.1)** `expectedRowVersion` wajib; pemeriksa dan waktunya dicatat (`VD-004 D-2`, `D-6`) | `ProcurementVendorDocument : Verify` | `{ expectedRowVersion, reason }` | `VendorDocumentResponse` |
| `POST` | `/{id}/cancel` | Batalkan dokumen `PENDING` yang salah catat. **(1.1)** Hanya pengunggahnya; orang lain → 403 tanpa kode (`VD-004 D-3`); `expectedRowVersion` opsional (`VD-004 D-2`) | `ProcurementVendorDocument : Create` | `{ reason, expectedRowVersion? }` | `VendorDocumentResponse` |
| `GET` | `/{id}/history` | Riwayat dokumen | `ProcurementVendorDocument : Read` | — | `List<DocumentHistoryResponse>` |

### Corporate / Procurement Management / Vendor / Vendor Status Request

Base URL: `/vendor-status-requests` — tersedia (`BE-PBJ-VD-007`). Lampiran pendukung memakai `documentType =
PBJ_VENDOR_STATUS`; pengusul boleh menambah lampiran selama `DRAFT`/`RETURNED` (1.1, `VD-007 D-6`).

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Jenis usulan, status, periode, urutan | `ProcurementVendorStatusRequest : Read` | — | `VendorStatusRequestFilterMetadataResponse` |
| `GET` | `/summary` | Jumlah per status | `ProcurementVendorStatusRequest : Read` | — | `VendorStatusRequestSummaryResponse` |
| `GET` | `/` | Daftar usulan; saringan `supplierId`, `requestType`, `status`, tanggal, `search`; urut `createDateTime`/`requestNumber`/`supplier`/`status` | `ProcurementVendorStatusRequest : Read` | `VendorStatusRequestQuery` | `PagedResult<VendorStatusRequestListItem>` |
| `GET` | `/{id}` | Detail usulan | `ProcurementVendorStatusRequest : Read` | — | `VendorStatusRequestResponse` |
| `POST` | `/` | Buat usulan blokir/buka blokir (`DRAFT`) | `ProcurementVendorStatusRequest : Create` | `{ supplierId, requestType, reason, evidenceEvaluationId? }` | `VendorStatusRequestResponse` (201) |
| `PUT` | `/{id}` | Ubah alasan/bukti saat `DRAFT`/`RETURNED`. **(1.1)** Hanya pengusul; orang lain → 403 tanpa kode (`VD-007 D-2`) | `ProcurementVendorStatusRequest : Create` | `{ expectedRowVersion, reason, evidenceEvaluationId? }` | `VendorStatusRequestResponse` |
| `POST` | `/{id}/submit` | Ajukan ke approval; nomor `UBV` dialokasikan pada pengajuan pertama. **(1.1)** Hanya pengusul (`VD-007 D-2`). Nomor dialokasikan sebelum gerbang approval, jadi bila ditolak `PBJ-VAL-091` nomornya hangus (`VD-007 D-3`, `DEC-PLT-008`) | `ProcurementVendorStatusRequest : Create` | `{ expectedRowVersion? }` + `Idempotency-Key` | `VendorStatusRequestResponse` |
| `POST` | `/{id}/cancel` | Batalkan draf atau tarik dari approval. **(1.1)** Batal draf: pemegang `Cancel` mana pun; tarik usulan `PENDING_APPROVAL`: hanya pengusul (`VD-007 D-2`) | `ProcurementVendorStatusRequest : Cancel` | `{ reason, expectedRowVersion? }` | `VendorStatusRequestResponse` |
| `POST` | `/{id}/workflow/synchronize` | **(1.1, baru, `VD-007 D-1`)** Sinkron ulang status usulan dengan keputusan `Wfl` bila penerapannya sempat gagal | `ProcurementVendorStatusRequest : Create` | — | `VendorStatusRequestResponse` |
| `GET` | `/{id}/history` | Riwayat usulan | `ProcurementVendorStatusRequest : Read` | — | `List<DocumentHistoryResponse>` |
| `GET` | `/{id}/approval` | Langkah dan keputusan `Wfl` (pola `PBJ-DEC-040`) | `ProcurementVendorStatusRequest : Read` | — | `ProcurementApprovalTimelineResponse` |

Keputusan setuju/tolak/kembalikan dikerjakan penyetuju di kotak masuk `Wfl` yang sudah ada, termasuk Antrean Persetujuan
Pengadaan (jenis "Usulan Status Vendor", `FE-PBJ-VD-004`).

**(1.1, `VD-007 D-4`, aturan bisnis `PBJ-DEC-064`)** Usulan `BLOCK` yang disetujui juga membuat supplier bukan lagi supplier
pilihan (`IsPreferredSupplier = false`). Usulan `UNBLOCK` yang disetujui **tidak** mengembalikannya; menjadikannya pilihan
lagi tetap keputusan Master Data. **Contoh:** PT Alkes Maju (supplier pilihan) diblokir lewat `UBV-000012`; sesudah blokir
dibuka, ia tetap bukan supplier pilihan sampai admin Master Data mencentangnya lagi.

### Corporate / Procurement Management / Vendor / Vendor Evaluation

Base URL: `/vendor-evaluations` — tersedia (`BE-PBJ-VD-008`). Lampiran pendukung memakai `documentType =
PBJ_VENDOR_EVALUATION` (1.1, `VD-008 D-6`).

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Kategori, jenis periode, status, rekomendasi, urutan. **(1.1)** Jenis periode `SEMESTER` (1–2), `QUARTER` (1–4), `YEAR` (1) beserta `maxPeriodNo` (`VD-008 D-8`) | `ProcurementVendorEvaluation : Read` | — | `VendorEvaluationFilterMetadataResponse` |
| `GET` | `/summary` | **(1.1, `VD-008 D-7`)** Hitungan per status (`draft`, `final`, `superseded`, `cancelled`), sebaran rekomendasi penilaian final, dan rata-rata nilai akhir; saringan `itemCategoryId?`, `periodYear?`, `periodType?`, `periodNo?` | `ProcurementVendorEvaluation : Read` | Saringan kategori dan periode | `VendorEvaluationSummaryResponse` |
| `GET` | `/` | Daftar penilaian; saringan `supplierId`, `itemCategoryId`, `periodYear`, `periodType`, `periodNo`, `status`, `recommendation`, tanggal, `search`; urut `createDateTime`/`period`/`supplier`/`finalScore` | `ProcurementVendorEvaluation : Read` | `VendorEvaluationQuery` | `PagedResult<VendorEvaluationListItem>` |
| `GET` | `/{id}` | Detail penilaian beserta nilai per kriteria (salinan nama dan bobot) | `ProcurementVendorEvaluation : Read` | — | `VendorEvaluationResponse` |
| `POST` | `/` | Buat draf; server menyalin kriteria aktif kategori. **(1.1)** Pemeriksaan "satu penilaian per vendor × kategori × periode" mencakup revisi `DRAFT`/`FINAL` (`VD-008 D-1`); nomor periode di luar rentang jenisnya → 422 tanpa kode (`VD-008 D-8`) | `ProcurementVendorEvaluation : Create` | `{ supplierId, itemCategoryId, periodYear, periodType?, periodNo, notes? }` | `VendorEvaluationResponse` (201) |
| `PUT` | `/{id}` | Isi nilai, rekomendasi, catatan (`DRAFT`). **(1.1)** Boleh oleh pemegang `Update` mana pun, bukan hanya pembuat (`VD-008 D-4`, aturan bisnis `PBJ-DEC-064`); nilai akhir dihitung ulang setiap simpan bila semua nilai terisi, dua desimal, setengah ke atas (`VD-008 D-5`) | `ProcurementVendorEvaluation : Update` | `{ expectedRowVersion, scores: [{ criterionId, score?, note? }], recommendation?, notes? }` | `VendorEvaluationResponse` |
| `POST` | `/{id}/finalize` | Finalkan; ditolak bila pemfinal = penilai. **(1.1)** Body `{ expectedRowVersion? }` opsional; body kosong diterima (`VD-008 D-2`) | `ProcurementVendorEvaluation : Finalize` | `{ expectedRowVersion? }` + `Idempotency-Key` | `VendorEvaluationResponse` |
| `POST` | `/{id}/revisions` | Buat revisi dari penilaian `FINAL`. **(1.1)** Revisi menyalin nilai, rekomendasi, catatan, dan **snapshot kriteria** penilaian asal (bukan kriteria master terkini); pembuat revisi menjadi penilai revisi, sehingga pemfinalnya harus orang lain lagi (`VD-008 D-3`) | `ProcurementVendorEvaluation : Revise` | `{ reason }` | `VendorEvaluationResponse` (201) |
| `POST` | `/{id}/cancel` | Batalkan draf | `ProcurementVendorEvaluation : Update` | `{ reason, expectedRowVersion? }` | `VendorEvaluationResponse` |
| `GET` | `/{id}/history` | Riwayat penilaian | `ProcurementVendorEvaluation : Read` | — | `List<DocumentHistoryResponse>` |

**Contoh nilai akhir:** bobot 40/40/20, nilai 4/5/3 → `finalScore = 4.20`. Bobot master diubah menjadi 40/50/10 sesudahnya →
penilaian itu tetap 4,20 karena bobotnya sudah disalin.

### Corporate / Procurement Management / Master Data / Vendor Document Type

Base URL: `/master-data/vendor-document-types` — tersedia (`BE-PBJ-VD-002`). Sembilan endpoint baku master data:
`GET /filters/metadata`, `GET /summary`, `GET /`, `GET /options`, `GET /{id}`, `POST /`, `PUT /{id}`,
`PATCH /{id}/status`, `DELETE /{id}`. Resource `ProcurementVendorDocumentType : Read/Create/Update/Delete`.
Hapus jenis yang sudah dipakai dokumen → 400 `errors.code` `PBJ-VAL-071` (`PBJ-DEC-041`). **(1.1, `VD-002 D-4`)** Jenis
yang masih diwajibkan kewajiban aktif tidak dapat dinonaktifkan → 409.

### Corporate / Procurement Management / Master Data / Category Document Requirement

Base URL: `/master-data/category-document-requirements` — tersedia (`BE-PBJ-VD-002`). Sembilan endpoint baku; resource
`ProcurementCategoryDocumentRequirement`. `PATCH /{id}/status` = aktifkan/nonaktifkan kewajiban (`PBJ-DEC-061`); aktivasi
pertama mengisi `ActivatedAt`. **(1.1)**

- Saringan `itemCategoryId` tersedia di daftar, ringkasan, dan pilihan, tetapi **tidak wajib**; tanpa saringan semua kategori tampil (`VD-002 D-1`).
- Kewajiban dianggap terpakai bila **pernah berlaku** (`ActivatedAt` terisi); yang belum pernah berlaku boleh dihapus (`VD-002 D-2`).
- Pasangan kategori × jenis pada kewajiban yang pernah berlaku tidak bisa diganti lewat `PUT` → 409; gantinya nonaktifkan lalu buat kewajiban baru (`VD-002 D-3`).
- Kewajiban hanya bisa diaktifkan bila kategori dan jenis dokumennya aktif → 422 (`VD-002 D-4`).
- `filters/metadata` memuat `itemCategoryOptions` dan `documentTypeOptions` (`VD-002 D-7`).

### Corporate / Procurement Management / Master Data / Evaluation Criterion

Base URL: `/master-data/evaluation-criteria` — tersedia (`BE-PBJ-VD-002`). Sembilan endpoint baku; resource
`ProcurementEvaluationCriterion`. **(1.1)**

- `GET /summary` memuat `categories[]` (total bobot aktif dan `isComplete` per kategori); dengan `?itemCategoryId=` juga `activeWeightTotal`, dipakai peringatan "bobot belum 100%" sebelum draf penilaian dibuat. Kategori tak dikenal → 404 (`VD-002 D-6`).
- Kategori kriteria yang sudah dipakai penilaian tidak dapat dipindah → 409; bobot lebih dari dua desimal → 422 tanpa kode (`VD-002 D-5`).
- `filters/metadata` memuat `itemCategoryOptions` (`VD-002 D-7`).

### Administrator / Master Data / Supplier (sudah ada — perilaku berubah)

| Method | Path | Perubahan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `api/v1/administrator/master-data/suppliers` | `isBlacklisted`/`blacklistReason` pada request **diabaikan**; supplier baru selalu tidak diblokir | `Supplier : Create` (tetap) |
| `PUT` | `api/v1/administrator/master-data/suppliers/{id}` | `isBlacklisted`/`blacklistReason` pada request **diabaikan**; nilai tersimpan tidak berubah. **(1.1, `VD-007 D-4`, `D-5`)** Supplier yang sedang diblokir tidak bisa dijadikan supplier pilihan; validasi lama "alasan blacklist wajib" dan "blacklist tidak boleh preferred" dihapus karena kedua field kini diabaikan | `Supplier : Update` (tetap) |

Respons tetap memuat `isBlacklisted` dan `blacklistReason` (baca saja). Perubahan hanya lewat usulan Pengadaan yang
disetujui (`PBJ-DEC-060`). Layar Master Data Supplier menampilkan centang "Diblokir (Pengadaan)" baca saja (`FE-PBJ-VD-004`).

---

## Perubahan 1.0 → 1.1 (`PBJ-DEC-064`, 19 November 2026)

| Laporan task | Delta yang masuk kontrak ini | Delta di kontrak lain atau di luar kontrak |
| --- | --- | --- |
| `BE-PBJ-VD-001` | — | D-1..D-5 menyangkut seed deret nomor, index, nilai bawaan, dan CHECK di basis data (kamus data dan arsitektur backend, bukan kontrak API). Tetap berlaku sebagaimana dibangun |
| `BE-PBJ-VD-002` | D-1..D-7 (master), D-8 (konvensi 400) | — |
| `BE-PBJ-VD-003` | D-1..D-7, D-9, D-10 | D-8 (perhitungan di memori; catatan kinerja) |
| `BE-PBJ-VD-004` | D-1..D-7 | D-1 juga di `PBJ-VD-STATE` 1.1; D-8 (perhitungan di memori) |
| `BE-PBJ-VD-005` | D-2..D-4 | D-1 di `PBJ-VD-INT` 1.1 |
| `BE-PBJ-VD-006` | D-1..D-4 | D-5 (bukti uji) |
| `BE-PBJ-VD-007` | D-1..D-6 | D-7 (isi riwayat) di `PBJ-VD-PERM` 1.1 bagian 5 |
| `BE-PBJ-VD-008` | D-1..D-8 | — |
