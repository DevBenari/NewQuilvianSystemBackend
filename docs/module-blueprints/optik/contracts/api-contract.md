# Optik — Kontrak API

| Field | Nilai |
| --- | --- |
| Blueprint ID | `OPT-BP-001` |
| `contract_version` | `OPT-API-v1.4` — status `approved` (sisi Optik), Yoga Aji Pratama 2026-11-06. Dasarnya: `OPT-API-v1.3` `approved` (sisi Optik), Yoga Aji Pratama 2026-11-06. Hanya keterangan status yang berubah; bentuk endpoint sama. Bagian Registrasi disetujui `OPT-APR-013`, bagian Billing disetujui `OPT-APR-014` (`OPT-DEC-084`) |
| Owner | Yoga Aji Pratama (domain); pemilik API backend: belum ditunjuk |
| `approved_by` / `approved_at` | Yoga Aji Pratama / 2026-11-06 untuk `v1.4`, `v1.3`, dan `v1.2`; `v1` 2026-11-03 |
| `input_revision` | `02-backend-architecture.md` r4, `03-domain-architecture.md` r1, `00-interview-decisions.md` r12 |
| Bukti source | Backend `06e40fb7` + `BE-OPT-060`, frontend `811f22159` + `FE-OPT-040` di working tree (branch `yoga`, `YogaV2`) |
| Dampak kompatibilitas | `v1.1` hanya menambah (pemanggil lama tidak terdampak). **`v1.2` mengubah perilaku:** empat `POST` pembuat menolak kiriman tanpa header `Idempotency-Key`. Satu-satunya pemanggilnya layar Optik. Frontend wajib terbit lebih dulu atau bersamaan, karena backend lama mengabaikan header itu, sedangkan backend baru menolak layar lama. `v1.3` hanya menegaskan bunyi konvensi dan memperbarui label; tidak ada perilaku yang berubah dari `v1.2`. Endpoint Billing yang dipakai tidak diubah bentuknya |

Seluruh 45 endpoint Optik di bawah sudah ada di kode. Bagian yang belum ada diberi label
**Rencana (belum tersedia)** pada barisnya. Semua
respons dibungkus `ApiResponse<T>`. Daftar memakai `PagedResult<T>` dengan `page`, `pageSize`,
`search`, `sortBy`, `sortDirection` (`QBE-PAGE-001`).

**Konvensi tindakan yang mengubah data** (`OPT-DEC-077`, `081`). Cara mencegah efek ganda mengikuti jenis tindakannya.
Data yang baru dibuat belum punya versi, sehingga wajib membawa header `Idempotency-Key` (Guid). Data yang sudah ada dilindungi
field `ExpectedVersion` (nilai `Version` terakhir yang dibaca), dan `ExpectedVersion` yang tertinggal ditolak `409`.

**Contoh:** Dedi menekan Kirim ke Pengerjaan dua kali karena jaringan lambat. Kiriman pertama memotong stok dan menaikkan versi.
Kiriman kedua dijawab `409`, layar memuat ulang pesanan berstatus Dalam Pengerjaan, dan stok tidak terpotong lagi.

**Cara penegakannya**

| Jenis tindakan | Endpoint | Penegak | Bila terkirim dua kali | Status |
| --- | --- | --- | --- | --- |
| Pembuat data | `POST visits` | Header wajib; kunci disimpan Registrasi (`OPT-VISIT:{kunci}`) | Kunjungan pertama, `IsReplay = true` | Tersedia |
| Pembuat data | `POST prescriptions`, `POST prescriptions/external`, `POST orders`, `POST warranty-claims` | Header wajib; kunci disimpan di kolom `IdempotencyKey` (`OPT-DEC-077`) | Data pertama, `IsReplay = true` | Tersedia |
| Mulai pemeriksaan | `POST refraction-exams` | Draf terbuka per kunjungan dipakai lagi (`OptRefractionExamService.StartAsync`) | Draf yang sama | Tersedia; tidak termasuk `OPT-DEC-077` |
| Tindakan atas data yang sudah ada | `bill`, `send-to-work`, `mark-ready`, `hand-over`, `cancel`, `close-not-collected`, revisi, keputusan | `ExpectedVersion`, status, dan kunci stok per baris; tanpa `Idempotency-Key` (`OPT-DEC-081`) | `409`; layar memuat ulang data | Tersedia |

**Aturan kunci pada empat `POST` pembuat** (`OPT-DEC-077`)

| Aturan | Isi | Contoh |
| --- | --- | --- |
| Kunci wajib | Header kosong atau berisi Guid nol ditolak `400` `OPT-INVALID-REQUEST`, sama seperti `POST visits` | Layar lama mengirim pesanan tanpa header: pesanan tidak tersimpan, pesan menyebut header wajib |
| Kunci diperiksa paling awal | Pemeriksaan kunci mendahului pemeriksaan status dan isi, agar kiriman ulang tidak ditolak oleh hasil kiriman pertamanya sendiri | Resep dari pemeriksaan EXM-0001 sudah tersimpan. Kiriman ulang berkunci sama menerima resep itu, bukan `409` "Resep dari pemeriksaan ini sudah ada" |
| Jawaban kiriman ulang | `200` dengan isi data pertama dan `IsReplay = true`. Logger mencatat "diulang dengan kunci yang sama", bukan pembuatan baru | Keluhan KLM-0001 dikirim dua kali: dua jawaban sama-sama KLM-0001 |
| Isi tidak dibandingkan | Server hanya mengenali kunci. Kunci sama dengan isi berbeda tetap mengembalikan data pertama. Layar mencegahnya dengan membuat kunci baru saat isi diubah (`05-screen-brief.md` bagian 6) | Pola yang sama dengan `FinPatientRefundRequest` |
| Kiriman serentak | Indeks unik menolak kiriman kedua. Service membaca data pemenang lalu mengembalikannya, sehingga pengguna tidak melihat galat | Dua tab menekan Simpan pada saat yang sama: satu pesanan tersimpan, keduanya menerima pesanan itu |
| Kunci berlaku bersama, tanpa kedaluwarsa | Kunci unik per tabel dan tidak terikat pengguna. Resep internal dan resep luar memakai satu kolom yang sama | — |

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
| `GET` | `/` | Daftar kunjungan Optik (terbuka/hari ini/semua) beserta ringkasan pemeriksaan dan pesanan | `OptVisit : Read` | `OptVisitListQuery` (tanggal, status kunjungan, ada pesanan terbuka, cari nama/RM) | `PagedResult<OptVisitListItemResponse>` | Tersedia |
| `GET` | `/{encounterId}` | Ringkasan satu kunjungan: pemeriksaan, resep, pesanan, keluhan | `OptVisit : Read` | — | `OptVisitDetailResponse` | Tersedia |
| `POST` | `/` | Daftarkan kunjungan Optik (datang sendiri, rujukan Poli Mata, atau keluhan) | `OptVisit : Create` | Header `Idempotency-Key` + `OptVisitRegisterRequest` (`PatientId`, cara bayar/penjamin, `ReferralSourceEncounterId?`, `ReferralDoctorId?`) | `OptVisitListItemResponse` | Tersedia, termasuk Penjamin Perusahaan (`BE-OPT-061`) |

Penjamin Perusahaan pada kunjungan Optik diterima sesudah perubahan Registrasi (`OPT-DEC-078`, `OPT-APR-013`, `BE-OPT-061`). Kartu yang tidak lolos
pemeriksaan ditolak `400` `OPT-REG-REJECTED`. Bentuk permintaan tidak berubah. **Contoh:** karyawan PT Contoh
(samaran) memilih Kartu Penjamin Perusahaan yang berlaku, dan kunjungan tersimpan dengan penjamin itu.

### Health Services / Optical Management / Opt Refraction Exam

Base URL: `api/v1/health-services/optical-management/refraction-exams`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar pemeriksaan per kunjungan/pasien | `OptRefractionExam : Read` | query `encounterId`, `patientId` | `PagedResult<OptRefractionExamResponse>` | Tersedia |
| `GET` | `/{id}` | Detail beserta seluruh revisi | `OptRefractionExam : Read` | — | `OptRefractionExamResponse` | Tersedia |
| `POST` | `/` | Mulai pemeriksaan | `OptRefractionExam : Create` | `OptRefractionExamStartRequest` (`EncounterId`) | `OptRefractionExamResponse` | Tersedia |
| `PUT` | `/{id}/measurements` | Isi/ubah hasil ukur selama `Draft` | `OptRefractionExam : Update` | `OptRefractionExamMeasurementRequest` | `OptRefractionExamResponse` | Tersedia |
| `POST` | `/{id}/finalize` | Simpan final; menerbitkan tarif pemeriksaan sekali | `OptRefractionExam : Finalize` | `OptRefractionExamFinalizeRequest` (`ExpectedVersion`) | `OptRefractionExamResponse` | Tersedia |
| `POST` | `/{id}/revisions` | Revisi hasil ukur sesudah final, tanpa tagih ulang | `OptRefractionExam : Revise` | `OptRefractionExamReviseRequest` (nilai + `RevisionReason`) | `OptRefractionExamResponse` | Tersedia |
| `POST` | `/{id}/cancel` | Batalkan pemeriksaan `Draft` | `OptRefractionExam : Cancel` | `OptRefractionExamCancelRequest` (`Reason`) | `OptRefractionExamResponse` | Tersedia |

### Health Services / Optical Management / Opt Prescription

Base URL: `api/v1/health-services/optical-management/prescriptions`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar resep pasien (filter jenis, hanya yang berlaku) | `OptPrescription : Read` | `OptPrescriptionListQuery` | `PagedResult<OptPrescriptionResponse>` | Tersedia |
| `GET` | `/{id}` | Detail beserta seluruh versi | `OptPrescription : Read` | — | `OptPrescriptionResponse` | Tersedia |
| `GET` | `/options` | Resep berlaku untuk dipilih di pesanan | `OptPrescription : Read` | query `patientId`, `prescriptionType` | `List<OptPrescriptionOptionResponse>` | Tersedia |
| `GET` | `/range-override` | Memeriksa apakah pengguna boleh mengonfirmasi ukuran di luar rentang | `OptPrescription : OverrideRange` | — | `OptRangeOverrideEligibilityResponse` | Tersedia |
| `POST` | `/` | Buat resep internal (dari pemeriksaan) | `OptPrescription : Create` | Header `Idempotency-Key` + `OptPrescriptionCreateRequest` | `OptPrescriptionResponse` (+ `IsReplay`) | Tersedia. Header wajib |
| `POST` | `/external` | Salin resep luar (transposisi otomatis bila plus) | `OptPrescription : Create` | Header `Idempotency-Key` + `OptPrescriptionCopyExternalRequest` | `OptPrescriptionResponse` (+ `IsReplay`) | Tersedia. Header wajib |
| `POST` | `/{id}/revisions` | Revisi resep yang belum terkunci | `OptPrescription : Revise` | `OptPrescriptionReviseRequest` (`ExpectedVersion`, nilai, `RevisionReason`) | `OptPrescriptionResponse` | Tersedia |

Melewati rentang isian memerlukan `OptPrescription : OverrideRange` **dan** profesi Sp.M; field
`RangeOverrideReason` wajib diisi.

### Health Services / Optical Management / Opt Order

Base URL: `api/v1/health-services/optical-management/orders`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar pesanan; filter status, "perlu dihubungi", "terlambat", "menunggu persetujuan batal", "lewat batas ambil" (`OPT-DEC-067`) | `OptOrder : Read` | `OptOrderListQuery` (`NeedsContact?`, `Overdue?`, `HasPendingCancellation?`, `PastNotCollectedLimit?`) | `PagedResult<OptOrderListItemResponse>` (+ `IsOverdue`, `NeedsContact`, `HasPendingCancellation`, `IsPastNotCollectedLimit`) | Tersedia |
| `GET` | `/{id}` | Detail pesanan, item, riwayat status, pengajuan batal | `OptOrder : Read` | — | `OptOrderResponse` | Tersedia |
| `GET` | `/{id}/payment-status` | Bagian pasien, terbayar, minimum uang muka, lunas atau belum | `OptOrder : Read` | — | `OptOrderPaymentStatusResponse` | Tersedia |
| `GET` | `/{id}/price-preview` | Perkiraan harga pesanan `Draft` yang sudah disimpan (`OPT-DEC-075`, `076`) | `OptOrder : Read` | — | `OptOrderPricePreviewResponse` (`Lines[]`, `EstimatedTotal?`, `AllTariffsAvailable`) | Tersedia |
| `POST` | `/` | Buat pesanan `Draft` (berpengerjaan atau penjualan langsung) | `OptOrder : Create` | Header `Idempotency-Key` + `OptOrderCreateRequest` (`EncounterId`, `OrderKind`, `PrescriptionVersionId?`, `Items[]`) | `OptOrderResponse` (+ `IsReplay`) | Tersedia. Header wajib |
| `PUT` | `/{id}` | Ubah item selama `Draft` | `OptOrder : Update` | `OptOrderCreateRequest` + `ExpectedVersion` | `OptOrderResponse` | Tersedia |
| `POST` | `/{id}/bill` | Simpan & tagihkan: menerbitkan fakta tagihan per item | `OptOrder : Bill` | `ExpectedVersion` | `OptOrderResponse` | Tersedia |
| `POST` | `/{id}/send-to-work` | Kirim ke pengerjaan; kunci resep, potong stok | `OptOrder : SendToWork` | `OptOrderSendToWorkRequest` (`WorkshopType`, `ExternalLabSupplierId?`, `EstimatedReadyDate`, `ExpectedVersion`) | `OptOrderResponse` | Tersedia |
| `POST` | `/{id}/mark-ready` | Tandai siap diambil | `OptOrder : MarkReady` | `ExpectedVersion` | `OptOrderResponse` | Tersedia |
| `POST` | `/{id}/hand-over` | Serahkan (lunas, identitas dicocokkan, *fitting*); penjualan langsung juga memotong stok di sini | `OptOrder : HandOver` | `OptOrderHandOverRequest` (`HandoverIdentityVerified`, `ReceiverName?`, `ReceiverRelationship?`, `FittingNote?`, `ExpectedVersion`) | `OptOrderResponse` | Tersedia |
| `POST` | `/{id}/cancel` | Batal sebelum pengerjaan | `OptOrder : Cancel` | `OptOrderCancelRequest` (`Reason`, `ExpectedVersion`) | `OptOrderResponse` | Tersedia |
| `POST` | `/{id}/cancellation-requests` | Ajukan batal sesudah pengerjaan (tandai lensa sudah/belum dipotong) | `OptOrder : RequestCancel` | `OptOrderCancelRequest` (`Reason`, `Items[]{ItemId, IsLensCut}`) | `OptOrderResponse` | Tersedia |
| `POST` | `/{id}/cancellation-requests/{requestId}/decision` | Kepala Optik menyetujui/menolak | `OptOrder : DecideCancel` | `OptOrderCancelDecisionRequest` (`Approve`, `DecisionNote?`, `ExpectedVersion`) | `OptOrderResponse` | Tersedia |
| `POST` | `/{id}/close-not-collected` | Tutup sebagai tidak diambil | `OptOrder : CloseNotCollected` | `OptOrderNotCollectedRequest` (`Reason`, `ExpectedVersion`) | `OptOrderResponse` | Tersedia |

**Aturan pesanan dari pelaksanaan `BE-OPT-032`:**

| Aturan | Contoh |
| --- | --- |
| `POST /` hanya membuat pesanan berjenis `Fabrication` dan `DirectSale`. Pesanan `Remake` dibuat sistem saat keluhan disetujui | Permintaan `OrderKind = Remake` ditolak `400` |
| Lensa kontak dan lensa kacamata dalam satu pesanan ditolak, karena satu pesanan hanya memakai satu versi resep | Lensa kacamata + lensa kontak dalam satu pesanan ditolak dengan pesan "Lensa kontak dan lensa kacamata memakai resep berbeda. Buat pesanan terpisah." |
| Versi resep yang dipilih wajib versi terkini | Resep sudah v2, pesanan yang memilih v1 ditolak |
| Syarat uang muka dihitung per tagihan kunjungan. `OPT-VAL-047` aktif bila ada pesanan berpengerjaan lain dan sisa tagihan di luar baris pesanan ini belum nol | Pesanan A belum lunas: pesanan B ditolak dikirim sampai sisa A nol |

Diskon **tidak** punya endpoint Optik. Layar Optik memanggil endpoint Billing yang sudah ada
(bagian akhir dokumen ini).

### Health Services / Optical Management / Opt Warranty Claim

Base URL: `api/v1/health-services/optical-management/warranty-claims`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar keluhan | `OptWarrantyClaim : Read` | `OptWarrantyClaimListQuery` | `PagedResult<OptWarrantyClaimResponse>` | Tersedia |
| `GET` | `/{id}` | Detail keluhan | `OptWarrantyClaim : Read` | — | `OptWarrantyClaimResponse` | Tersedia |
| `POST` | `/` | Catat keluhan pada kunjungan baru; status garansi dihitung otomatis | `OptWarrantyClaim : Create` | Header `Idempotency-Key` + `OptWarrantyClaimCreateRequest` (`OrderId`, `EncounterId`, `ComplaintType`, `ComplaintNote`) | `OptWarrantyClaimResponse` (+ `IsReplay`) | Tersedia. Header wajib |
| `POST` | `/{id}/examination` | Catat pemeriksaan ulang | `OptWarrantyClaim : Examine` | `OptWarrantyClaimExamineRequest` (`ExaminationFinding`, `ReexamRefractionExamId?`, `ExpectedVersion`) | `OptWarrantyClaimResponse` | Tersedia |
| `POST` | `/{id}/decision` | Setujui remake atau tolak | `OptWarrantyClaim : Decide` | `OptWarrantyClaimDecisionRequest` (`Approve`, `DecisionReason?`, `RemakeItems[]?` = id baris pesanan asal, kosong berarti semua baris; `PrescriptionVersionId?` = resep baru hasil pemeriksaan ulang untuk remake; `ExpectedVersion`) | `OptWarrantyClaimResponse` | Tersedia |

### Health Services / Optical Management / Opt Reference Data

Base URL: `api/v1/health-services/optical-management/reference-data`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/metadata` | Daftar nilai enum berlabel Indonesia untuk layar | `OptReferenceData : Read` | — | `OptMetadataResponse` | Tersedia |
| `GET` | `/item-catalog` | Katalog barang Optik: profil, kategori, stok tersedia di depo Optik | `OptReferenceData : Read` | `OptItemCatalogQuery` (kategori, warna, ukuran, indeks, cari) | `PagedResult<OptItemProfileResponse>` | Tersedia |
| `PUT` | `/item-profiles/{drugId}` | Buat/ubah profil Optik untuk barang | `OptReferenceData : Update` | `OptItemProfileUpsertRequest` | `OptItemProfileResponse` | Tersedia |
| `GET` | `/settings` | Pengaturan unit Optik | `OptReferenceData : Read` | — | `OptSettingResponse` | Tersedia |
| `PUT` | `/settings` | Ubah pengaturan (berlaku ke depan) | `OptReferenceData : UpdateSetting` | `OptSettingUpdateRequest` (+ `ExpectedVersion`) | `OptSettingResponse` | Tersedia |

### Health Services / Optical Management / Opt Report

Base URL: `api/v1/health-services/optical-management/reports`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/visits-by-examiner` | Rekap kunjungan dan pemeriksaan per pemeriksa | `OptReport : Read` | periode | `OptVisitReportResponse` | Tersedia |
| `GET` | `/sales` | Rekap penjualan per jenis barang dan cara bayar | `OptReport : Read` | periode | `OptSalesReportResponse` | Tersedia |
| `GET` | `/open-orders` | Pesanan terbuka, terlambat, dan perlu dihubungi | `OptReport : Read` | per tanggal | `OptOpenOrderReportResponse` | Tersedia |
| `GET` | `/warranty` | Keluhan, garansi, dan remake | `OptReport : Read` | periode | `OptWarrantyReportResponse` | Tersedia |

Rekap penjualan membaca jumlah dari Billing (baris tagihan sumber `Optical`), bukan dari harga
yang disimpan Optik.

---

## Endpoint modul lain yang dipakai (as-is)

### Health Services / Billing Management / Billing / Invoices — *Sudah ada*

Base URL: `api/v1/health-services/billing-management/billing/invoices`

| Method | Path | Kegunaan bagi Optik | Hak akses | Status |
| --- | --- | --- | --- | --- |
| `POST` | `/{id}/discounts` | Petugas Optik menerapkan kebijakan diskon Optik pada baris tagihan pesanan | `BillingDiscount : Create` | Sudah ada |
| `POST` | `/{id}/discounts/{discountId}/cancel` | Membatalkan pengajuan diskon | `BillingDiscount : Update` | Sudah ada |

### Health Services / Billing Management / Master Data / Discount Policy — *Sudah ada*

Base URL: `api/v1/health-services/billing-management/master-data/discount-policies`

| Method | Path | Kegunaan bagi Optik | Hak akses | Status |
| --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar kebijakan aktif untuk modal Ajukan Diskon. Layar menyaring kebijakan Optik | `DiscountPolicy : Read` | Sudah ada |

**Diskon MVP Optik** (`OPT-DEC-079`). Kebijakan Optik bernilai tetap paling tinggi 10%, berjenis `PROMO_ITEM`, dan langsung
mengurangi tagihan, karena Billing hanya mewajibkan persetujuan untuk diskon dokter
(`DiscountPolicyService.ResolveApproval`). **Contoh:** Diskon Optik 10% pada frame Rp 450.000 mengurangi tagihan Rp 45.000. Permintaan
15% tidak punya pilihan di modal.

Penyaringan hanya di layar, sebab `MstDiscountPolicy` tidak punya ruas sumber atau unit. Backend Billing tetap menerima kebijakan
lain bila dikirim langsung ke endpoint; risiko ini diterima untuk MVP. Penanda kebijakan Optik: kode berawalan `OPT-`, misalnya
`OPT-DISC-10` (`OPT-DEC-084`).

Persetujuan diskon di atas 10% oleh Kepala Optik ditunda ke slice `OPT-APR-004`. Endpoint persetujuan yang ada saat ini khusus
diskon dokter (`BillingDoctorDiscount : Approve`).

### Health Services / Master Data / Drug — *Sudah ada*

Base URL: `api/v1/health-services/master-data/drugs`

| Method | Path | Kegunaan bagi Optik | Hak akses | Status |
| --- | --- | --- | --- | --- |
| `GET` | `/options` | Pilihan barang pada aksi Daftarkan Barang, dengan `isPrescribable=false` (`OPT-DEC-080`) | `Drug : Read` | Sudah ada |

## Jejak requirement

| Grup | Keputusan | Kriteria uji |
| --- | --- | --- |
| Opt Visit | `OPT-DEC-006`, `032`, `037`, `043`, `078` | `OPT-AC-023`, `025`, `026`, `036` |
| Opt Refraction Exam | `OPT-DEC-005`, `012`, `042`, `047`, `082`, `083` | `OPT-AC-001`, `002`, `024`, `039`..`041`. Pekerja latar kirim ulang tarif tidak punya endpoint |
| Opt Prescription | `OPT-DEC-007`, `011`, `017`, `022`, `023`, `039`, `045`, `046`, `071`, `077` | `OPT-AC-003`, `004`, `012`, `028`, `033`, `035` |
| Opt Order | `OPT-DEC-008`..`010`, `013`, `016`, `018`..`021`, `027`..`029`, `034`, `035`, `038`, `044`, `048`, `049`, `067`, `075`..`077`, `079` | `OPT-AC-005`..`011`, `016`..`021`, `027`, `029`, `031`, `034`, `035`, `037` |
| Opt Warranty Claim | `OPT-DEC-015`, `041`, `043`, `068`, `077` | `OPT-AC-013`, `022`, `026`, `032`, `035` |
| Opt Reference Data | `OPT-DEC-009`, `031`, `040`, `041`, `080` | `OPT-AC-006`, `020`, `038` |
| Opt Report | `OPT-DEC-025`, `061`, `070` | `OPT-AC-030`; `testing/acceptance-test-matrix.md` |
