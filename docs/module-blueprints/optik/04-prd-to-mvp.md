# Optik — PRD ke MVP

## 1. Identitas dokumen

| Field | Nilai |
| --- | --- |
| Produk | Quilvian SIMRS v2 |
| Modul | Optik (`OPT`) — `OPT-BP-001` |
| Dokumen | `OPT-PRD` revision `1` — `contract_version` `OPT-PRD-v1`, status `approved` (sisi Optik) |
| Owner | Yoga Aji Pratama |
| `approved_by` / `approved_at` | Yoga Aji Pratama / 2026-11-03 |
| Repository target | `NewQuilvianSystemBackend`, `QuilvianSystemFrontendDev` |
| Commit SHA baseline | Backend `2a5094ca`, frontend `b6dd6c304` |
| `input_revision` | `02-backend-architecture.md` r1, `03-frontend-architecture.md` r1, `erd/` r1, `contracts/*` v1, `03-domain-architecture.md` r1 (`DOMAIN_ARCHITECTURE_READY`), `00-interview-decisions.md` r4 |
| Cakupan | Optik milik RS untuk pasien umum, asuransi swasta, dan perusahaan: dari kunjungan dan pemeriksaan refraksi sampai kacamata diserahkan dan keluhan garansi ditangani |

## 2. Ringkasan eksekutif

Setiap kacamata dan lensa kontak yang dibuat Optik dapat dilacak dari resep sampai diterima
pasien. Ukuran lensa terkunci sebelum dikerjakan, uang muka melindungi RS dari lensa custom yang
tidak diambil, dan setiap rupiah tercatat di tagihan kunjungan yang sama. Kalau ada keluhan,
kacamata dapat ditelusuri balik ke resep dan pesanannya, lalu dibuat ulang tanpa biaya bila masih
bergaransi.

## 3. Masalah produk

| Kondisi sekarang | Bukti |
| --- | --- |
| Backend tidak punya apa pun tentang Optik | `OPT-CAP-10`; `rg optik/refraksi/kacamata` di `Areas/` kosong |
| Menu Optik v2 hanya sisa berkas yang tidak dipakai | `OPT-CAP-29` (`config.jsx` tidak diimpor) |
| Layar v1 berisi data contoh tertulis di kode, dan pendaftarannya memanggil endpoint yang tidak ada | `OPT-CAP-30` |
| Yang sudah ada dan dipakai ulang: kunjungan unit penunjang, stok Farmasi, tarif per barang, fakta tagihan resmi, pembayaran sebagian, diskon Billing, STR/SIP HR, pembaca Rekam Medis | `OPT-CAP-02`, `16`, `19`, `20`, `23`, `06`, `12` |

## 4. Visi produk

1. Pasien terdaftar di unit Optik dalam satu kunjungan.
2. RO/Sp.M memeriksa → hasil ukur berversi → resep berversi.
3. Petugas membuat pesanan dari versi resep yang berlaku → tagihan per item muncul di kasir.
4. Kasir menerima uang muka → pesanan dikirim ke bengkel/lab → resep terkunci, stok berkurang.
5. Kacamata siap → pasien melunasi → diserahkan dengan identitas dicocokkan → garansi mulai → kunjungan dan tagihan selesai.
6. Hasil ukur dan resep tampil di Rekam Medis; jasa pemeriksa ditarik Medical Fee.
7. Keluhan dalam garansi → pemeriksaan ulang → remake Rp 0 yang tertaut ke pesanan asal.

## 5. Batas MVP

**Titik mulai**

1. Pasien sudah terdaftar di Patient Management.
2. Petugas mendaftarkan kunjungan pada unit Optik (datang sendiri, rujukan Poli Mata, atau keluhan).

**Titik akhir**

1. Pesanan berstatus `HandedOver`, `Cancelled`, atau `NotCollected`, dan tagihannya lunas atau menjadi piutang di Billing.
2. Kunjungan pemesanan ditutup oleh finalisasi tagihan.
3. Keluhan garansi berstatus `Approved` (remake diserahkan) atau `Rejected`.

## 6. Pelaku sasaran

| Pelaku | Tanggung jawab di MVP |
| --- | --- |
| Admisi / petugas pendaftaran | Mendaftarkan kunjungan Optik |
| Refraksionis Optisien (RO) | Pemeriksaan refraksi, resep, salin resep luar, pemeriksaan ulang keluhan |
| Dokter Sp.M | Sama dengan RO, ditambah melewati rentang isian dengan alasan |
| Petugas penjualan Optik | Pesanan, tagihkan, ajukan diskon, kirim ke pengerjaan, tandai siap, serahkan, ajukan batal, catat keluhan |
| Kepala Optik | Setujui batal sesudah pengerjaan, setujui diskon di atas batas, tutup tidak diambil, putuskan keluhan |
| Admin Optik | Profil barang dan pengaturan |
| Kasir (Billing) | Uang muka, pelunasan, finalisasi tagihan |

## 7. Pemilihan kemampuan MVP

| Kemampuan | ID kemampuan asal | Keputusan MVP |
| --- | --- | --- |
| Kunjungan unit Optik | `OPT-CAP-02`, `03` | Wajib; jangkar tagihan dan rekam medis |
| Rujukan Poli Mata bertaut kunjungan asal | `OPT-CAP-05` | Wajib; Sp.M menulis resep di form Optik (`OPT-DEC-039`) |
| Gerbang izin praktik | `OPT-CAP-06` | Wajib; syarat sah resep |
| Pemeriksaan refraksi berversi | `OPT-CAP-10`, `11` | Wajib; dasar resep dan tagihan pemeriksaan |
| Resep kacamata/lensa kontak berversi, terkunci, berlaku 12 bulan | `OPT-CAP-10`, `11` | Wajib; tanpa resep tidak ada pesanan berukuran |
| Pesanan berpengerjaan dengan uang muka | `OPT-CAP-20`, `21` | Wajib; inti modul |
| Potong/kembali stok depo Optik | `OPT-CAP-15`..`17` | Wajib; stok harus sesuai rak |
| Tagihan sumber resmi Optik | `OPT-CAP-20` | Wajib; tanpa ini tidak ada pembayaran |
| Kunjungan tidak tertutup selama pesanan berjalan | `OPT-CAP-24` | Wajib; tanpa ini kunjungan tertutup saat pasien lunas di awal |
| Penyerahan, batal, tidak diambil | — (baru) | Wajib; titik akhir MVP |
| Penjualan langsung | `OPT-CAP-16` | Wajib; omzet barang tanpa resep (`OPT-DEC-020`) |
| Diskon sampai batas | `OPT-CAP-23` | Wajib; memakai Billing |
| Keluhan garansi dan remake | — (baru) | Wajib; `OPT-DEC-015` |
| Profil barang dan pengaturan | `OPT-CAP-14` | Wajib; tanpa ini katalog dan angka bawaan tidak ada |
| Hasil ke Rekam Medis | `OPT-CAP-12` | Wajib; `OPT-DEC-024` |
| Jasa medis pemeriksa | — (Medical Fee) | Wajib; `OPT-DEC-042` |
| Empat laporan inti | — (baru) | Wajib; `OPT-DEC-025` |

## 8. Kemampuan yang ditunda

| Kemampuan | ID kemampuan asal | Alasan ditunda | Pengganti selama MVP |
| --- | --- | --- | --- |
| Kacamata BPJS | `OPT-CAP-25`..`27` | Fondasi penjamin BPJS tidak ada di sistem dan modul Insurance `PARTIAL` (`OPT-DEC-030`) | Pasien BPJS dilayani sebagai pasien umum, atau klaim diproses manual di luar sistem |
| Alokasi pembayaran per baris tagihan | `OPT-CAP-24` | Billing hanya mengalokasikan per tagihan; butuh fitur Billing (`OPT-APR-003`) | Aturan sementara: satu pesanan berpengerjaan belum lunas per kunjungan (`OPT-DEC-035`) |
| Persetujuan diskon di atas batas dari layar Optik | `OPT-CAP-23` | Endpoint persetujuan Billing saat ini khusus diskon dokter (`OPT-APR-004`) | Diskon ≤ 10% langsung berlaku; di atas batas diproses Billing/kasir sampai persetujuan per `ApproverRole` tersedia |
| Uji coba dan kontrol lensa kontak | — | Ditunda pemilik (`OPT-DEC-022`) | Resep dan penjualan lensa kontak saja |
| Pesan WhatsApp pengingat | — | Kanal pesan milik platform notifikasi | Daftar "perlu dihubungi" di layar pesanan |
| Retur penjualan langsung | — | Bukan keputusan MVP (RCG `n3`) | Pengembalian dana lewat Billing-Kasir; stok dikoreksi Farmasi |
| Integrasi alat autorefraktometer | — | Tidak diminta | Hasil alat diketik manual |

## 9. Alur bisnis target

**`FLOW-OPT-MVP-001` — Kacamata berresep, pasien umum**

1. Admisi mendaftarkan kunjungan Ibu Sari di unit Optik.
2. Rina (RO, SIP berlaku) memulai pemeriksaan, mengisi visus dan koreksi, lalu menyimpan final. Tarif pemeriksaan Rp 75.000 (samaran) masuk tagihan.
3. Rina membuat resep OD −1,50, OS −1,75, PD 62.
4. Petugas penjualan memilih resep, frame Rp 450.000, dan lensa stok Rp 600.000, lalu menyimpan dan menagihkan. Tagihan kunjungan kini Rp 1.125.000.
5. Kasir menerima uang muka Rp 525.000 (50% dari bagian pesanan Rp 1.050.000).
6. Petugas mengirim pesanan ke bengkel sendiri dengan perkiraan jadi besok. Resep terkunci; stok frame dan lensa masing-masing −1.
7. Besok kacamata jadi, lalu ditandai siap diambil.
8. Ibu Sari datang dan melunasi Rp 600.000 (sisa pesanan + pemeriksaan). Petugas mencocokkan identitas, mencatat *fitting*, lalu menyerahkan. Garansi mulai.
9. Billing memfinalisasi tagihan dan kunjungan ditutup.
10. Dua minggu kemudian Ibu Sari mengeluh pusing. Kunjungan keluhan baru dibuat, RO memeriksa ulang, dan Kepala Optik menyetujui remake Rp 0 yang tertaut ke pesanan asal.

## 10. Epic dan functional requirement

| Epic | Tujuan | Disposisi backend |
| --- | --- | --- |
| `EPIC OPT-00` Fondasi | Registry prefix, enum, permission, migration, master awal | `MISSING / NEW` (+ `EXTEND` enum) |
| `EPIC OPT-01` Kunjungan Optik | Pendaftaran dan ringkasan kunjungan | `EXTEND` (Registrasi) + `MISSING / NEW` |
| `EPIC OPT-02` Pemeriksaan refraksi | Pemeriksaan berversi dan tagihannya | `MISSING / NEW` |
| `EPIC OPT-03` Resep | Resep berversi, transposisi, kunci, usia | `MISSING / NEW` |
| `EPIC OPT-04` Pesanan berpengerjaan | Pesanan, uang muka, pengerjaan, stok | `MISSING / NEW` + `EXISTING / REUSE` (stok) |
| `EPIC OPT-05` Penjualan langsung | Barang tanpa resep | `MISSING / NEW` |
| `EPIC OPT-06` Penyerahan dan penutupan | Serah, batal, tidak diambil | `MISSING / NEW` |
| `EPIC OPT-07` Garansi dan remake | Keluhan dan remake | `MISSING / NEW` |
| `EPIC OPT-08` Tagihan Optik | Sumber `Optical`, kebijakan finalisasi, diskon | `EXTEND` (Billing) |
| `EPIC OPT-09` Rekam Medis dan jasa medis | Pembaca RM dan adapter Medical Fee | `EXTEND` |
| `EPIC OPT-10` Laporan | Empat laporan | `MISSING / NEW` |
| `EPIC OPT-11` Profil barang dan pengaturan | Katalog dan angka bawaan | `MISSING / NEW` |

**Functional requirement**

| FR | Epic | Requirement yang dapat diuji |
| --- | --- | --- |
| `FR-OPT-001` | 00 | Prefix `Opt` dan folder `OpticalManagement` tercatat di registry sebelum model pertama dibuat. **Bukti:** baris registry + QBE GitRange lulus |
| `FR-OPT-002` | 00 | `ServiceUnitType.Optical = 11` tersedia, dan nilai lama tidak bergeser |
| `FR-OPT-010` | 01 | Kunjungan Optik terbentuk lewat `EncounterIntakeService`; Optik tidak menulis `RegPatientEncounter` langsung |
| `FR-OPT-011` | 01 | Kunjungan rujukan menyimpan kunjungan Poli Mata asal; asal milik pasien lain ditolak |
| `FR-OPT-020` | 02 | Hanya RO/Sp.M dengan SIP berlaku (tanggal WIB) yang bisa memulai/memfinalkan. **Contoh:** SIP s.d. 31-10-2026 ditolak pada 03-11-2026 |
| `FR-OPT-021` | 02 | Final menerbitkan tepat satu baris tarif pemeriksaan; dua revisi sesudahnya tidak menambah baris |
| `FR-OPT-030` | 03 | Isian di luar rentang `OPT-DEC-045` ditolak untuk RO dan diterima untuk Sp.M dengan alasan |
| `FR-OPT-031` | 03 | Resep plus +1,00 / +0,50 × 90 tersimpan +1,50 / −0,50 × 180 beserta teks asli |
| `FR-OPT-032` | 03 | Revisi resep terkunci ditolak `409`; resep > 12 bulan tidak bisa dipakai pesanan baru |
| `FR-OPT-040` | 04 | Kirim ke pengerjaan ditolak bila terbayar < minimum. **Contoh:** bagian pasien Rp 1.050.000, terbayar Rp 500.000 → ditolak; Rp 525.000 → diterima |
| `FR-OPT-041` | 04 | Pasien berpenjamin: minimum dari bagian pasien; bagian Rp 0 langsung diterima |
| `FR-OPT-042` | 04 | Kirim memotong stok frame/lensa stok tepat sekali walau tombol ditekan dua kali |
| `FR-OPT-043` | 04 | Pesanan kedua di kunjungan yang sama ditolak selama pesanan pertama berpengerjaan belum lunas |
| `FR-OPT-050` | 05 | Penjualan langsung: lunas lalu diserahkan; stok dipotong saat diserahkan; lensa kontak tanpa resep berlaku ditolak |
| `FR-OPT-060` | 06 | Serah ditolak bila belum lunas atau identitas belum dicocokkan |
| `FR-OPT-061` | 06 | Batal sesudah pengerjaan hanya lewat persetujuan Kepala Optik; frame dan lensa stok belum dipotong kembali ke stok dan barisnya batal; lensa custom tetap ditagih |
| `FR-OPT-062` | 06 | `NotCollected` hanya sesudah 90 hari sejak siap; tetap ditagih; penyerahan susulan sesudah lunas |
| `FR-OPT-070` | 07 | Keluhan hanya pada kunjungan baru atas pesanan `HandedOver`; status garansi dihitung dari snapshot |
| `FR-OPT-071` | 07 | Remake disetujui → pesanan Rp 0 tanpa fakta tagihan, tertaut pesanan asal; paling banyak satu per keluhan |
| `FR-OPT-080` | 08 | Dua pesanan berframe sama → dua baris tagihan terpisah |
| `FR-OPT-081` | 08 | Pasien lunas di awal → tagihan tidak difinalisasi selama pesanan `InFabrication`/`ReadyForPickup`; difinalisasi setelah pesanan terakhir terminal dan lunas |
| `FR-OPT-082` | 08 | Diskon ≤ 10% langsung tercatat di baris tagihan; di atas batas menunggu persetujuan |
| `FR-OPT-090` | 09 | Rekam Medis menampilkan pemeriksaan dan resep versi terakhir tanpa harga |
| `FR-OPT-091` | 09 | Medical Fee dapat menarik pemeriksaan final beserta pemeriksanya |
| `FR-OPT-100` | 10 | Laporan pesanan terlambat menampilkan pesanan lewat perkiraan jadi dan `ReadyForPickup` > 30 hari |
| `FR-OPT-110` | 11 | Barang Optik tidak muncul di peresepan dokter (`IsPrescribable = false`) |
| `FR-OPT-111` | 11 | Ubah pengaturan tidak mengubah pesanan/resep yang sudah dibuat; perubahan tercatat nilai lama-baru |

## 11. Model status yang diusulkan

Rinci di `contracts/state-transition-matrix.md`.

| Objek | Status | Invariant utama |
| --- | --- | --- |
| Pemeriksaan | `Draft` → `Final` (+ revisi) / `Cancelled` | Final sekali, tagih sekali |
| Resep | versi 1..n; terkunci; berlaku | Tidak berubah setelah terkunci |
| Pesanan | `Draft` → `AwaitingDownPayment` → `InFabrication` → `ReadyForPickup` → `HandedOver`; `Cancelled`; `NotCollected` | Uang muka, stok sekali, lunas sebelum serah |
| Keluhan | `Submitted` → `Examined` → `Approved` / `Rejected` | Dalam garansi untuk remake |

## 12. Sasaran arsitektur

| Kategori | Isi |
| --- | --- |
| Dipakai ulang | `EncounterIntakeService`, `DrugStockService`, `ClinicalMilestoneFactProducer`, `NumberSeriesAllocator`, `MstDrug`/`MstTariff`/`MstSupplier`, `WfpCredentialLicense`, `IMedicalRecordDocumentSource`, endpoint diskon Billing |
| Diperluas | `RegPatientEncounter` (+`ReferralSourceEncounterId`), `ServiceUnitType`, `ClinicalDocumentKind`, `DrugStockSourceDocumentTypes`, `BillingSourceContract`, `BillingChargeSourceAdapter`, `BillingSourceTariffResolver`, `MedicalFeeServiceSourceAdapter` |
| Baru | 12 model `Opt*`, 13 service, 7 controller (`02-backend-architecture.md`) |

## 13. Sasaran kemampuan API

Bagian dari `contracts/api-contract.md` (`OPT-API-v1`); semua **Rencana (belum tersedia)**.

### Health Services / Optical Management / Opt Visit

Base URL: `api/v1/health-services/optical-management/visits`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar kunjungan Optik | `OptVisit : Read` | `OptVisitListQuery` | `PagedResult<OptVisitListItemResponse>` | `EPIC OPT-01` | **Rencana (belum tersedia)** |
| `POST` | `/` | Daftarkan kunjungan | `OptVisit : Create` | `OptVisitRegisterRequest` | `OptVisitListItemResponse` | `EPIC OPT-01` | **Rencana (belum tersedia)** |

### Health Services / Optical Management / Opt Refraction Exam

Base URL: `api/v1/health-services/optical-management/refraction-exams`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Mulai pemeriksaan | `OptRefractionExam : Create` | `OptRefractionExamStartRequest` | `OptRefractionExamResponse` | `EPIC OPT-02` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/finalize` | Simpan final | `OptRefractionExam : Finalize` | `OptRefractionExamFinalizeRequest` | `OptRefractionExamResponse` | `EPIC OPT-02` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/revisions` | Revisi sesudah final | `OptRefractionExam : Revise` | `OptRefractionExamReviseRequest` | `OptRefractionExamResponse` | `EPIC OPT-02` | **Rencana (belum tersedia)** |

### Health Services / Optical Management / Opt Prescription

Base URL: `api/v1/health-services/optical-management/prescriptions`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Buat resep internal | `OptPrescription : Create` | `OptPrescriptionCreateRequest` | `OptPrescriptionResponse` | `EPIC OPT-03` | **Rencana (belum tersedia)** |
| `POST` | `/external` | Salin resep luar | `OptPrescription : Create` | `OptPrescriptionCopyExternalRequest` | `OptPrescriptionResponse` | `EPIC OPT-03` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/revisions` | Revisi resep | `OptPrescription : Revise` | `OptPrescriptionReviseRequest` | `OptPrescriptionResponse` | `EPIC OPT-03` | **Rencana (belum tersedia)** |

### Health Services / Optical Management / Opt Order

Base URL: `api/v1/health-services/optical-management/orders`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Buat pesanan | `OptOrder : Create` | `OptOrderCreateRequest` | `OptOrderResponse` | `EPIC OPT-04` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/bill` | Simpan & tagihkan | `OptOrder : Bill` | `ExpectedVersion` | `OptOrderResponse` | `EPIC OPT-04`, `08` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/send-to-work` | Kirim ke pengerjaan | `OptOrder : SendToWork` | `OptOrderSendToWorkRequest` | `OptOrderResponse` | `EPIC OPT-04` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/hand-over` | Serahkan | `OptOrder : HandOver` | `OptOrderHandOverRequest` | `OptOrderResponse` | `EPIC OPT-05`, `06` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/cancellation-requests/{requestId}/decision` | Putuskan batal | `OptOrder : DecideCancel` | `OptOrderCancelDecisionRequest` | `OptOrderResponse` | `EPIC OPT-06` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/close-not-collected` | Tutup tidak diambil | `OptOrder : CloseNotCollected` | `OptOrderNotCollectedRequest` | `OptOrderResponse` | `EPIC OPT-06` | **Rencana (belum tersedia)** |

### Health Services / Optical Management / Opt Warranty Claim

Base URL: `api/v1/health-services/optical-management/warranty-claims`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Catat keluhan | `OptWarrantyClaim : Create` | `OptWarrantyClaimCreateRequest` | `OptWarrantyClaimResponse` | `EPIC OPT-07` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/decision` | Putuskan keluhan | `OptWarrantyClaim : Decide` | `OptWarrantyClaimDecisionRequest` | `OptWarrantyClaimResponse` | `EPIC OPT-07` | **Rencana (belum tersedia)** |

### Health Services / Optical Management / Opt Reference Data

Base URL: `api/v1/health-services/optical-management/reference-data`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `PUT` | `/settings` | Ubah pengaturan | `OptReferenceData : UpdateSetting` | `OptSettingUpdateRequest` | `OptSettingResponse` | `EPIC OPT-11` | **Rencana (belum tersedia)** |

### Health Services / Optical Management / Opt Report

Base URL: `api/v1/health-services/optical-management/reports`

| Method | Path | Kegunaan | Hak akses | Request | Response | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/open-orders` | Pesanan terbuka dan terlambat | `OptReport : Read` | per tanggal | `OptOpenOrderReportResponse` | `EPIC OPT-10` | **Rencana (belum tersedia)** |

## 14. Matriks kewenangan

String persis dari `contracts/permission-audit-matrix.md`.

| Tindakan | String | Pelaku |
| --- | --- | --- |
| Buat/final/revisi pemeriksaan | `[AccessPermission("OptRefractionExam", "Create")]`, `"Finalize"`, `"Revise"` | RO, Sp.M |
| Buat/revisi resep | `[AccessPermission("OptPrescription", "Create")]`, `"Revise"` | RO, Sp.M |
| Lewat rentang | `[AccessPermission("OptPrescription", "OverrideRange")]` | Sp.M |
| Pesanan | `[AccessPermission("OptOrder", "Create")]`, `"Bill"`, `"SendToWork"`, `"MarkReady"`, `"HandOver"`, `"Cancel"`, `"RequestCancel"` | Petugas penjualan, Kepala Optik |
| Putuskan batal, tutup tidak diambil | `[AccessPermission("OptOrder", "DecideCancel")]`, `"CloseNotCollected"` | Kepala Optik |
| Keluhan | `[AccessPermission("OptWarrantyClaim", "Create")]`, `"Examine"`, `"Decide"` | Petugas / RO-Sp.M / Kepala Optik |
| Pengaturan | `[AccessPermission("OptReferenceData", "UpdateSetting")]` | Admin Optik |

## 15. Batas integrasi dan billing

| Optik **tidak boleh** membuat sendiri | Pemiliknya |
| --- | --- |
| Data pasien, kunjungan, penutupan kunjungan | Patient Management, Registrasi, Billing (finalisasi) |
| Harga, diskon, pembayaran, uang muka, piutang, refund | Master tarif, Billing |
| Saldo dan mutasi stok | Farmasi (`DrugStockService`) |
| Jasa medis | Medical Fee |
| Jurnal | Accounting |
| Klaim penjamin | Insurance Management |

## 16. Guardrail regulasi

| Kewajiban | Wujud di MVP |
| --- | --- |
| Resep hanya oleh tenaga berwenang dengan izin praktik berlaku | Gerbang profesi + STR/SIP (`OPT-DEC-005`, `012`); dasar hukum kewenangan RO menunggu konfirmasi kebijakan RS |
| Rekam medis tidak boleh diubah diam-diam | Pemeriksaan dan resep berversi; versi lama tidak pernah diubah |
| Privasi data klinis | Kolom sensitif tidak masuk log; contoh dokumentasi memakai data samaran |
| Kacamata BPJS | Tidak dilayani lewat jalur BPJS di MVP (`OPT-DEC-030`) |

## 17. Kebutuhan non-fungsional

| ID | Kebutuhan |
| --- | --- |
| `NFR-001` | Atomicity: perpindahan status pesanan, riwayat, kunci resep, dan mutasi stok berada dalam satu transaksi; gagal satu = gagal semua |
| `NFR-002` | Concurrency: setiap aksi membawa `ExpectedVersion`; benturan → `409` |
| `NFR-003` | Idempotensi: `Idempotency-Key` per aksi; kunci stok tetap per pesanan per item |
| `NFR-004` | Audit: pelaku, waktu, alasan untuk setiap versi, transisi, pengajuan batal, dan perubahan pengaturan |
| `NFR-005` | Otorisasi *fail-closed*: HR/Billing tidak menjawab → tindakan ditolak tanpa mengubah data |
| `NFR-006` | Koreksi: tidak ada hapus; koreksi lewat versi atau status |
| `NFR-007` | Waktu: simpan UTC; aturan tanggal (izin, berlaku resep, garansi, 90 hari) memakai tanggal WIB |

## 18. Skenario UAT

| UAT | Epic | Jalur | Kondisi awal | Langkah | Hasil yang diharapkan |
| --- | --- | --- | --- | --- | --- |
| `UAT-01` | 01 | Berhasil | Pasien terdaftar | Admisi mendaftarkan kunjungan Optik | Kunjungan tampil di daftar kunjungan Optik |
| `UAT-02` | 01 | Gagal | Kunjungan Poli Mata milik pasien A | Daftarkan kunjungan Optik pasien B dengan rujukan itu | Ditolak; tidak ada kunjungan terbentuk |
| `UAT-03` | 02 | Berhasil | RO ber-SIP berlaku | Isi dan simpan final, lalu revisi dua kali | Satu tarif pemeriksaan di kasir; tiga versi di riwayat |
| `UAT-04` | 02 | Gagal | RO dengan SIP berakhir kemarin | Mulai pemeriksaan | Ditolak dengan tanggal berakhirnya SIP |
| `UAT-05` | 03 | Berhasil | Resep luar +1,00 / +0,50 × 90 | RO menyalin | Tersimpan −0,50 × 180 dengan SPH +1,50; teks asli terlihat |
| `UAT-06` | 03 | Gagal | Resep dipakai pesanan yang sudah dikirim | RO merevisi | Ditolak "Resep terkunci…" |
| `UAT-07` | 04 | Berhasil | Pesanan Rp 1.050.000, uang muka Rp 525.000 | Kirim ke bengkel | `InFabrication`; stok −1 frame, −1 lensa |
| `UAT-08` | 04 | Gagal | Uang muka Rp 500.000 | Kirim | Ditolak "minimum Rp 525.000" |
| `UAT-09` | 04 | Gagal | Pesanan pertama dikirim, belum lunas | Kirim pesanan kedua di kunjungan sama | Ditolak dengan nomor pesanan pertama |
| `UAT-10` | 05 | Berhasil | Cairan bertarif Rp 85.000 | Tagihkan, kasir terima, serahkan | Stok −1; kunjungan selesai saat tagihan final |
| `UAT-11` | 05 | Gagal | Lensa kontak kosmetik, pasien tanpa resep | Buat penjualan langsung | Ditolak "wajib resep lensa kontak" |
| `UAT-12` | 06 | Berhasil | Pesanan siap, lunas | Centang identitas, serahkan | `HandedOver`; tagihan final; kunjungan ditutup |
| `UAT-13` | 06 | Gagal | Pesanan siap, sisa Rp 525.000 | Serahkan | Ditolak "Tagihan belum lunas…" |
| `UAT-14` | 06 | Berhasil | Pesanan `InFabrication`, lensa custom | Ajukan batal → Kepala Optik setuju | Frame kembali stok; lensa tetap ditagih; sisa jadi piutang |
| `UAT-15` | 06 | Gagal | Pesanan `InFabrication` | Petugas membatalkan langsung | Ditolak "perlu persetujuan Kepala Optik" |
| `UAT-16` | 07 | Berhasil | Diserahkan 10 hari lalu | Keluhan ukuran → periksa ulang → setujui | Remake Rp 0 tertaut pesanan asal |
| `UAT-17` | 07 | Gagal | Diserahkan 31 hari lalu | Keluhan ukuran → setujui | Ditolak "di luar masa garansi" |
| `UAT-18` | 08 | Berhasil | Pasien membayar lunas penuh saat pesan | Pesanan masih dikerjakan | Tagihan tetap terbuka; kunjungan belum ditutup |
| `UAT-19` | 08 | Gagal | Diskon 15% (batas 10%) | Petugas mengajukan | Diskon menunggu persetujuan; tagihan belum berkurang |
| `UAT-20` | 09 | Berhasil | Pemeriksaan dan resep final | Buka Rekam Medis pasien | Visus dan resep versi terakhir tampil tanpa harga |
| `UAT-21` | 09 | Gagal | Pengguna tanpa izin Rekam Medis | Buka riwayat Optik di Rekam Medis | Ditolak oleh izin Rekam Medis |
| `UAT-22` | 10 | Berhasil | Pesanan siap 35 hari | Buka laporan pesanan terbuka | Pesanan tampil di "perlu dihubungi" |
| `UAT-23` | 10 | Gagal | Pengguna petugas penjualan | Buka laporan | Ditolak `403` |
| `UAT-24` | 11 | Berhasil | Pengingat 30 hari | Admin ubah ke 21 | Pesanan baru memakai 21; riwayat perubahan tercatat |
| `UAT-25` | 11 | Gagal | Pengingat 30 | Admin isi batas tutup 20 | Ditolak "Batas tutup harus lebih lama…" |
| `UAT-26` | 00 | Berhasil | Barang Optik terdaftar | Dokter mencari "frame" di peresepan | Tidak tampil |
| `UAT-27` | 00 | Gagal | Migration diterapkan pada DB dev | Jalankan `Down` | Tabel Optik terhapus tanpa menyentuh tabel lain |

## 19. Definition of Done

| Butir | Bukti |
| --- | --- |
| Prefix `Opt` tercatat di registry sebelum model pertama | Baris registry; QBE GitRange lulus |
| Satu kasus `FLOW-OPT-MVP-001` berjalan dari kunjungan sampai serah dan kunjungan ditutup | `UAT-01`, `03`, `07`, `12` |
| Resep tidak bisa berubah setelah lensa dikerjakan | `UAT-06` |
| Izin praktik kedaluwarsa menolak pemeriksaan dan resep | `UAT-04` |
| Uang muka di bawah minimum menolak pengerjaan | `UAT-08` |
| Stok dipotong tepat sekali dan kembali saat batal yang sah | `OPT-AC-008`, `UAT-14` |
| Kunjungan tidak tertutup selama pesanan berjalan | `UAT-18` |
| Pembatalan sesudah pengerjaan hanya dengan persetujuan | `UAT-15` |
| Remake hanya dalam garansi dan tanpa tagihan | `UAT-16`, `UAT-17` |
| Seluruh master MVP terisi | Rencana data master awal `02-backend-architecture.md` bagian 8 |
| Persetujuan pemilik modul mitra tercatat | `OPT-APR-001`..`012` berstatus disetujui di decision log |
| Seluruh baris `testing/acceptance-test-matrix.md` punya bukti | Laporan task per baris |

## 20. Urutan pengiriman dan pertanyaan terbuka

| Gelombang | Isi | Syarat mulai |
| --- | --- | --- |
| `MVP-0` | `EPIC OPT-00`, `EPIC OPT-11` — registry, enum, permission, 12 model + migration, seeder, profil barang, pengaturan | Blueprint disetujui; `OPT-APR-005`, `006` |
| `MVP-1` | `EPIC OPT-01`, `02`, `03`, `09` (pembaca RM) — kunjungan, pemeriksaan, resep | `MVP-0`; `OPT-APR-007`, `008`, `010` |
| `MVP-2` | `EPIC OPT-08`, `04` — sumber tagihan + kebijakan finalisasi, pesanan, uang muka, pengerjaan, stok | `MVP-1`; `OPT-APR-001`, `002`, `011`, `012` |
| `MVP-3` | `EPIC OPT-05`, `06` — penjualan langsung, serah, batal, tidak diambil; diskon sampai batas | `MVP-2`; `OPT-APR-004` (sebagian) |
| `MVP-4` | `EPIC OPT-07`, `09` (Medical Fee), `10` — garansi, remake, jasa medis, laporan | `MVP-3`; `OPT-APR-009` |
| `POST-MVP` | Bagian 8: BPJS, alokasi per baris, persetujuan diskon per `ApproverRole`, lensa kontak lanjutan, WhatsApp, retur | Di luar rilis pertama |

Tidak ada epic `OPEN DECISION` di gelombang mana pun.

**Pertanyaan sebelum development lock**

| Pertanyaan | Siapa yang menjawab | Dampak bila belum dijawab | Memblokir |
| --- | --- | --- | :---: |
| Persetujuan Billing: sumber `Optical`, kebijakan finalisasi, pembayaran sebagian sebagai uang muka, tarif pemeriksaan (`OPT-APR-001`, `002`, `011`, `012`) | Pemilik Billing | `MVP-2` tidak dapat dimulai | Ya |
| Persetujuan Registrasi: kolom rujukan dan kunjungan terbuka lintas hari (`OPT-APR-008`, `010`) | Pemilik Registrasi/Rawat Jalan | `MVP-1` tidak dapat dimulai | Ya |
| Persetujuan Master Data/Farmasi: kategori, depo, jenis dokumen stok, jenis unit (`OPT-APR-005`, `006`) | Pemilik Master Data/Farmasi | `MVP-0` tidak dapat dimulai | Ya |
| Persetujuan Rekam Medis: jenis dokumen dan pembaca (`OPT-APR-007`) | Pemilik Rekam Medis | Bagian RM `MVP-1` tertahan | Ya |
| Persetujuan Medical Fee: kelayakan RO atas jasa (`OPT-APR-009`) | Pemilik Medical Fee | Bagian Medical Fee `MVP-4` tertahan | Ya (hanya `MVP-4`) |
| Konfirmasi rincian isian visus/autoref (`PROPOSED`, RCG `n1`) | Yoga + RO/Sp.M | Form pemeriksaan bisa berubah kolom | Ya (`MVP-1`) |
| Konfirmasi rentang BC/DIA dan daftar atribut barang (`PROPOSED`) | Yoga + RO/Sp.M | Validasi lensa kontak dan katalog | Tidak |
| Format nomor dokumen `OPT-EXM/RX/ORD/CLM-RSMMC-nnnnnn` | Yoga | Hanya label nomor | Tidak |
| Profesi RO dan jenis izinnya di HR (`OPT-CAP-07`) | HR | Gerbang izin tak bisa diuji | Ya (`MVP-1`) |
| Screen brief Optik (`OPT-FE-001`, `002`) | Yoga | Layar frontend tidak dapat dibangun | Ya (frontend) |
