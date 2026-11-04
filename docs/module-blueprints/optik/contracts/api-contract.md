# Optik — Kontrak API

| Field | Nilai |
| --- | --- |
| Blueprint ID | `OPT-BP-001` |
| `contract_version` | `OPT-API-v1` — status `approved` (sisi Optik) |
| Owner | Yoga Aji Pratama (domain); pemilik API backend: belum ditunjuk |
| `approved_by` / `approved_at` | Yoga Aji Pratama / 2026-11-03 |
| `input_revision` | `02-backend-architecture.md` r1, `03-domain-architecture.md` r1, `00-interview-decisions.md` r4 |
| Dampak kompatibilitas | Seluruh endpoint Optik baru; tidak ada kontrak lama yang berubah. Endpoint Billing yang dipakai tidak diubah bentuknya |

Semua endpoint di bawah **Rencana (belum tersedia)** kecuali yang ditandai *Sudah ada*. Semua
respons dibungkus `ApiResponse<T>`. Daftar memakai `PagedResult<T>` dengan `page`, `pageSize`,
`search`, `sortBy`, `sortDirection` (`QBE-PAGE-001`).

**Konvensi tindakan yang mengubah data.** Setiap `POST`/`PUT` yang mengubah status membawa header
`Idempotency-Key` (Guid) dan field `ExpectedVersion` (nilai `Version` terakhir yang dibaca). Permintaan
ulang dengan kunci yang sama mengembalikan hasil pertama tanpa efek ganda. `ExpectedVersion` yang
tertinggal ditolak `409`.

**Kode status umum**

| Kode | Arti bagi pengguna |
| --- | --- |
| `200` | Berhasil |
| `201` | Data baru berhasil dibuat |
| `400` | Isian tidak lengkap atau tidak sah; pesan menyebut isian mana (lihat `validation-matrix.md`) |
| `403` | Anda tidak punya hak akses untuk tindakan ini, atau izin praktik (STR/SIP) Anda tidak berlaku |
| `404` | Data tidak ditemukan |
| `409` | Data sudah diubah orang lain, atau status saat ini tidak mengizinkan tindakan ini |
| `503` | Modul pendukung (Billing/HR/Farmasi) sedang tidak dapat dihubungi; tindakan ditolak tanpa mengubah data |

---

### Health Services / Optical Management / Opt Visit

Base URL: `api/v1/health-services/optical-management/visits`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar kunjungan Optik (terbuka/hari ini/semua) beserta ringkasan pemeriksaan dan pesanan | `OptVisit : Read` | `OptVisitListQuery` (tanggal, status kunjungan, ada pesanan terbuka, cari nama/RM) | `PagedResult<OptVisitListItemResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{encounterId}` | Ringkasan satu kunjungan: pemeriksaan, resep, pesanan, keluhan | `OptVisit : Read` | — | `OptVisitDetailResponse` | **Rencana (belum tersedia)** |
| `POST` | `/` | Daftarkan kunjungan Optik (datang sendiri, rujukan Poli Mata, atau keluhan) | `OptVisit : Create` | `OptVisitRegisterRequest` (`PatientId`, cara bayar/penjamin, `ReferralSourceEncounterId?`, `ReferralDoctorId?`) | `OptVisitListItemResponse` | **Rencana (belum tersedia)** |

### Health Services / Optical Management / Opt Refraction Exam

Base URL: `api/v1/health-services/optical-management/refraction-exams`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar pemeriksaan per kunjungan/pasien | `OptRefractionExam : Read` | query `encounterId`, `patientId` | `PagedResult<OptRefractionExamResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Detail beserta seluruh revisi | `OptRefractionExam : Read` | — | `OptRefractionExamResponse` | **Rencana (belum tersedia)** |
| `POST` | `/` | Mulai pemeriksaan | `OptRefractionExam : Create` | `OptRefractionExamStartRequest` (`EncounterId`) | `OptRefractionExamResponse` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}/measurements` | Isi/ubah hasil ukur selama `Draft` | `OptRefractionExam : Update` | `OptRefractionExamMeasurementRequest` | `OptRefractionExamResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/finalize` | Simpan final; menerbitkan tarif pemeriksaan sekali | `OptRefractionExam : Finalize` | `OptRefractionExamFinalizeRequest` (`ExpectedVersion`) | `OptRefractionExamResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/revisions` | Revisi hasil ukur sesudah final, tanpa tagih ulang | `OptRefractionExam : Revise` | `OptRefractionExamReviseRequest` (nilai + `RevisionReason`) | `OptRefractionExamResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/cancel` | Batalkan pemeriksaan `Draft` | `OptRefractionExam : Cancel` | `OptRefractionExamCancelRequest` (`Reason`) | `OptRefractionExamResponse` | **Rencana (belum tersedia)** |

### Health Services / Optical Management / Opt Prescription

Base URL: `api/v1/health-services/optical-management/prescriptions`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar resep pasien (filter jenis, hanya yang berlaku) | `OptPrescription : Read` | `OptPrescriptionListQuery` | `PagedResult<OptPrescriptionResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Detail beserta seluruh versi | `OptPrescription : Read` | — | `OptPrescriptionResponse` | **Rencana (belum tersedia)** |
| `GET` | `/options` | Resep berlaku untuk dipilih di pesanan | `OptPrescription : Read` | query `patientId`, `prescriptionType` | `List<OptPrescriptionOptionResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Buat resep internal (dari pemeriksaan) | `OptPrescription : Create` | `OptPrescriptionCreateRequest` | `OptPrescriptionResponse` | **Rencana (belum tersedia)** |
| `POST` | `/external` | Salin resep luar (transposisi otomatis bila plus) | `OptPrescription : Create` | `OptPrescriptionCopyExternalRequest` | `OptPrescriptionResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/revisions` | Revisi resep yang belum terkunci | `OptPrescription : Revise` | `OptPrescriptionReviseRequest` (`ExpectedVersion`, nilai, `RevisionReason`) | `OptPrescriptionResponse` | **Rencana (belum tersedia)** |

Melewati rentang isian memerlukan `OptPrescription : OverrideRange` **dan** profesi Sp.M; field
`RangeOverrideReason` wajib diisi.

### Health Services / Optical Management / Opt Order

Base URL: `api/v1/health-services/optical-management/orders`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar pesanan; filter status, "perlu dihubungi", "terlambat" | `OptOrder : Read` | `OptOrderListQuery` | `PagedResult<OptOrderListItemResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Detail pesanan, item, riwayat status, pengajuan batal | `OptOrder : Read` | — | `OptOrderResponse` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/payment-status` | Bagian pasien, terbayar, minimum uang muka, lunas atau belum | `OptOrder : Read` | — | `OptOrderPaymentStatusResponse` | **Rencana (belum tersedia)** |
| `POST` | `/` | Buat pesanan `Draft` (berpengerjaan atau penjualan langsung) | `OptOrder : Create` | `OptOrderCreateRequest` (`EncounterId`, `OrderKind`, `PrescriptionVersionId?`, `Items[]`) | `OptOrderResponse` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Ubah item selama `Draft` | `OptOrder : Update` | `OptOrderCreateRequest` + `ExpectedVersion` | `OptOrderResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/bill` | Simpan & tagihkan: menerbitkan fakta tagihan per item | `OptOrder : Bill` | `ExpectedVersion` | `OptOrderResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/send-to-work` | Kirim ke pengerjaan; kunci resep, potong stok | `OptOrder : SendToWork` | `OptOrderSendToWorkRequest` (`WorkshopType`, `ExternalLabSupplierId?`, `EstimatedReadyDate`, `ExpectedVersion`) | `OptOrderResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/mark-ready` | Tandai siap diambil | `OptOrder : MarkReady` | `ExpectedVersion` | `OptOrderResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/hand-over` | Serahkan (lunas, identitas dicocokkan, *fitting*); penjualan langsung juga memotong stok di sini | `OptOrder : HandOver` | `OptOrderHandOverRequest` (`HandoverIdentityVerified`, `ReceiverName?`, `ReceiverRelationship?`, `FittingNote?`, `ExpectedVersion`) | `OptOrderResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/cancel` | Batal sebelum pengerjaan | `OptOrder : Cancel` | `OptOrderCancelRequest` (`Reason`, `ExpectedVersion`) | `OptOrderResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/cancellation-requests` | Ajukan batal sesudah pengerjaan (tandai lensa sudah/belum dipotong) | `OptOrder : RequestCancel` | `OptOrderCancelRequest` (`Reason`, `Items[]{ItemId, IsLensCut}`) | `OptOrderResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/cancellation-requests/{requestId}/decision` | Kepala Optik menyetujui/menolak | `OptOrder : DecideCancel` | `OptOrderCancelDecisionRequest` (`Approve`, `DecisionNote?`, `ExpectedVersion`) | `OptOrderResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/close-not-collected` | Tutup sebagai tidak diambil | `OptOrder : CloseNotCollected` | `OptOrderNotCollectedRequest` (`Reason`, `ExpectedVersion`) | `OptOrderResponse` | **Rencana (belum tersedia)** |

Diskon **tidak** punya endpoint Optik. Layar Optik memanggil endpoint Billing yang sudah ada
(bagian akhir dokumen ini).

### Health Services / Optical Management / Opt Warranty Claim

Base URL: `api/v1/health-services/optical-management/warranty-claims`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar keluhan | `OptWarrantyClaim : Read` | `OptWarrantyClaimListQuery` | `PagedResult<OptWarrantyClaimResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Detail keluhan | `OptWarrantyClaim : Read` | — | `OptWarrantyClaimResponse` | **Rencana (belum tersedia)** |
| `POST` | `/` | Catat keluhan pada kunjungan baru; status garansi dihitung otomatis | `OptWarrantyClaim : Create` | `OptWarrantyClaimCreateRequest` (`OrderId`, `EncounterId`, `ComplaintType`, `ComplaintNote`) | `OptWarrantyClaimResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/examination` | Catat pemeriksaan ulang | `OptWarrantyClaim : Examine` | `OptWarrantyClaimExamineRequest` (`ExaminationFinding`, `ReexamRefractionExamId?`, `ExpectedVersion`) | `OptWarrantyClaimResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/decision` | Setujui remake atau tolak | `OptWarrantyClaim : Decide` | `OptWarrantyClaimDecisionRequest` (`Approve`, `DecisionReason?`, `RemakeItems[]?`, `ExpectedVersion`) | `OptWarrantyClaimResponse` | **Rencana (belum tersedia)** |

### Health Services / Optical Management / Opt Reference Data

Base URL: `api/v1/health-services/optical-management/reference-data`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/metadata` | Daftar nilai enum berlabel Indonesia untuk layar | `OptReferenceData : Read` | — | `OptMetadataResponse` | **Rencana (belum tersedia)** |
| `GET` | `/item-catalog` | Katalog barang Optik: profil, kategori, stok tersedia di depo Optik | `OptReferenceData : Read` | `OptItemCatalogQuery` (kategori, warna, ukuran, indeks, cari) | `PagedResult<OptItemProfileResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/item-profiles/{drugId}` | Buat/ubah profil Optik untuk barang | `OptReferenceData : Update` | `OptItemProfileUpsertRequest` | `OptItemProfileResponse` | **Rencana (belum tersedia)** |
| `GET` | `/settings` | Pengaturan unit Optik | `OptReferenceData : Read` | — | `OptSettingResponse` | **Rencana (belum tersedia)** |
| `PUT` | `/settings` | Ubah pengaturan (berlaku ke depan) | `OptReferenceData : UpdateSetting` | `OptSettingUpdateRequest` (+ `ExpectedVersion`) | `OptSettingResponse` | **Rencana (belum tersedia)** |

### Health Services / Optical Management / Opt Report

Base URL: `api/v1/health-services/optical-management/reports`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/visits-by-examiner` | Rekap kunjungan dan pemeriksaan per pemeriksa | `OptReport : Read` | periode | `OptVisitReportResponse` | **Rencana (belum tersedia)** |
| `GET` | `/sales` | Rekap penjualan per jenis barang dan cara bayar | `OptReport : Read` | periode | `OptSalesReportResponse` | **Rencana (belum tersedia)** |
| `GET` | `/open-orders` | Pesanan terbuka, terlambat, dan perlu dihubungi | `OptReport : Read` | per tanggal | `OptOpenOrderReportResponse` | **Rencana (belum tersedia)** |
| `GET` | `/warranty` | Keluhan, garansi, dan remake | `OptReport : Read` | periode | `OptWarrantyReportResponse` | **Rencana (belum tersedia)** |

Rekap penjualan membaca jumlah dari Billing (baris tagihan sumber `Optical`), bukan dari harga
yang disimpan Optik.

---

## Endpoint modul lain yang dipakai (as-is)

### Health Services / Billing Management / Billing / Invoices — *Sudah ada*

Base URL: `api/v1/health-services/billing-management/billing/invoices`

| Method | Path | Kegunaan bagi Optik | Hak akses | Status |
| --- | --- | --- | --- | --- |
| `POST` | `/{id}/discounts` | Petugas Optik mengajukan diskon pada baris tagihan pesanan | `BillingDiscount : Create` | Sudah ada |
| `POST` | `/{id}/discounts/{discountId}/cancel` | Membatalkan pengajuan diskon | `BillingDiscount : Update` | Sudah ada |

Persetujuan diskon oleh Kepala Optik memakai alur persetujuan kebijakan diskon Billing. Endpoint
persetujuan yang ada saat ini khusus diskon dokter (`BillingDoctorDiscount : Approve`). Billing
perlu menyediakan persetujuan berdasarkan `ApproverRole` kebijakan (`OPT-APR-004`, dicatat di
`integration-contract.md`).

## Jejak requirement

| Grup | Keputusan | Kriteria uji |
| --- | --- | --- |
| Opt Visit | `OPT-DEC-006`, `032`, `037`, `043` | `OPT-AC-023`, `025`, `026` |
| Opt Refraction Exam | `OPT-DEC-005`, `012`, `042`, `047` | `OPT-AC-001`, `002`, `024` |
| Opt Prescription | `OPT-DEC-007`, `011`, `017`, `022`, `023`, `039`, `045`, `046` | `OPT-AC-003`, `004`, `012`, `028` |
| Opt Order | `OPT-DEC-008`..`010`, `013`, `016`, `018`..`021`, `027`..`029`, `034`, `035`, `038`, `044`, `048`, `049` | `OPT-AC-005`..`011`, `016`..`021`, `027`, `029` |
| Opt Warranty Claim | `OPT-DEC-015`, `041`, `043` | `OPT-AC-013`, `022`, `026` |
| Opt Reference Data | `OPT-DEC-009`, `031`, `040`, `041` | `OPT-AC-006`, `020` |
| Opt Report | `OPT-DEC-025` | `testing/acceptance-test-matrix.md` |
