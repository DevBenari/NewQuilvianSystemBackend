# API Contract — Modul Laboratorium

| Field | Value |
|---|---|
| Contract version | `LAB-API-v1` |
| Revision | `5` |
| Status | `approved` — `r3` dikunci 2026-09-02; **amandemen `r4` dan `r5` disetujui pemilik modul 2026-09-03** |
| Isi amandemen `r4` | Sepuluh endpoint baca ditambahkan: `GET /filters/metadata` dan `GET /summary` pada kelima grup Laboratorium yang sudah punya controller. Seluruhnya **aditif** — tidak satu pun endpoint, ruas, atau nilai enum `r3` yang berubah, berganti nama, atau hilang. Dikerjakan `BE-LAB-17` |
| Isi amandemen `r5` | `GET /lab-orders` memperoleh penyaring, pengurutan, dan pagination di sisi server lewat `LabOrderPagedQuery`. **Ini satu-satunya perubahan breaking**: bentuk responsnya berubah dari `ApiResponse<List<LabOrderListResponse>>` menjadi `ApiResponse<PagedResult<LabOrderListResponse>>`. Dampak konsumen dinilai — lihat catatan di bawah. Dikerjakan `BE-LAB-18` |

| Batas penguncian | **Terkunci penuh sejak 2026-09-02.** `LAB-OPEN-021` dijawab: penamaan memakai prefix `Lab`, sehingga tidak ada lagi bagian yang dikecualikan |
| Owner | Yoga Aji Pratama (`yogaaji452@gmail.com`) |
| `approved_by` / `approved_at` | Yoga Aji Pratama (`yogaaji452@gmail.com`) / 2026-09-02 |
| Input revision | Decisions rev 20; `LAB-DA-001` rev 4 |
| Input hash | `sha256:6504b18a327b9966526bd1df8f3cb878d7f6d6519dacc1f7df16b1066729ae82` atas `00-interview-decisions.md`, dihitung 2026-09-02 |
| Dampak kompatibilitas | **Breaking** pada endpoint sampel — lihat bagian 3 |
| Backend SHA | `c87d9c0` |

Seluruh endpoint memerlukan login (`[Authorize]`). Pembungkus respons memakai
`ApiResponse<T>.Ok(data, pesan)` dan `ApiResponse<T>.Fail(kode, pesan)`.

Endpoint yang belum ada di kode ditandai **`Rencana (belum tersedia)`**.

> **Penilaian dampak amandemen `r5`.** Satu-satunya konsumen `GET /lab-orders` yang ditemukan
> adalah modul IGD pada
> `src/lib/state/slice/health-services/emergency-installation-management/emergency-assessment-slice.jsx`.
> Pembungkusnya memakai `unwrapItems`, yang sudah menangani **kedua** bentuk — larik langsung
> maupun objek ber-`items` — sehingga perubahan bentuk ini **tidak memutusnya**.
>
> Yang berubah bagi IGD adalah perilaku, bukan bentuk: sebelumnya ia menerima seluruh isi
> tabel lalu menyaringnya sendiri di browser; kini ia menerima halaman pertama saja. Selama
> ia belum mengirim `?encounterId=`, pesanan pasien yang berada di luar halaman pertama tidak
> akan tampil. **Perbaikannya satu baris di sisi IGD** dan justru menutup keterbatasan yang
> sudah mereka catat sendiri sebagai `IGD-DEC-105`. Laboratorium tidak mengubah source
> frontend; temuan ini dilaporkan sebagaimana diwajibkan `AGENTS.md`.

---

## 1. Kontrak As-Is — yang benar-benar ada pada `c87d9c0`

### Health Services / Laboratory Management / Lab Order

Base URL: `api/v1/health-services/laboratory-management/lab-orders`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar pesanan lab — **tanpa satu pun parameter**, mengembalikan seluruh isi tabel | `LabOrder : Read` | — | `ApiResponse<List<LabOrderListResponse>>` | Tersedia pada `c87d9c0`; **digantikan `r5`** |
| `GET` | `/{id}` | Detail satu pesanan | `LabOrder : Read` | — | `ApiResponse<LabOrderDetailResponse>` | Tersedia |
| `POST` | `/` | Membuat pesanan lab | `LabOrder : Create` | `CreateLabOrderRequest` | `ApiResponse<LabOrderDetailResponse>` | Tersedia |
| `PUT` | `/{id}/start-process` | Menandai pesanan mulai dikerjakan | `LabOrder : Process` | — | `ApiResponse<LabOrderDetailResponse>` | Tersedia |
| `PUT` | `/{id}/complete` | Menandai pesanan selesai | `LabOrder : Process` | — | `ApiResponse<LabOrderDetailResponse>` | Tersedia |
| `PUT` | `/{id}/hold` | Menahan pesanan | `LabOrder : Hold` | `HoldLabRequest` | `ApiResponse<LabOrderDetailResponse>` | Tersedia |
| `PUT` | `/{id}/resume` | Melanjutkan pesanan | `LabOrder : Hold` | `ResumeLabRequest` | `ApiResponse<LabOrderDetailResponse>` | Tersedia |
| `PUT` | `/{id}/cancel` | Membatalkan pesanan | `LabOrder : Update` | — | `ApiResponse<LabOrderDetailResponse>` | Tersedia |

### Health Services / Laboratory Management / Lab Specimen

Base URL: `api/v1/health-services/laboratory-management/lab-specimens`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/rejection-reasons` | Daftar alasan penolakan | `LabSpecimen : Read` | — | `ApiResponse<List<LabRejectionReasonResponse>>` | Tersedia |
| `GET` | `/by-order/{labOrderId}` | Daftar sampel satu pesanan | `LabSpecimen : Read` | — | `ApiResponse<List<LabSpecimenResponse>>` | Tersedia |
| `GET` | `/by-order/{labOrderId}/history` | Riwayat perpindahan status | `LabSpecimen : Read` | — | `ApiResponse<List<LabTransitionHistoryResponse>>` | Tersedia |
| `POST` | `/by-order/{labOrderId}` | Menambah sampel | `LabSpecimen : Plan` | `PlanLabSpecimenRequest` | `ApiResponse<LabSpecimenResponse>` | Tersedia |
| `POST` | `/{id}/collect` | Mencatat pengambilan | `LabSpecimen : Collect` | `CollectLabSpecimenRequest` | `ApiResponse<LabSpecimenResponse>` | Tersedia |
| `POST` | `/{id}/receive` | Mencatat tiba di lab | `LabSpecimen : Receive` | `ReceiveLabSpecimenRequest` | `ApiResponse<LabSpecimenResponse>` | Tersedia |
| `POST` | `/{id}/accept` | Menyatakan layak | `LabSpecimen : Accept` | `AcceptLabSpecimenRequest` | `ApiResponse<LabBillingHandoffResponse>` | Tersedia |
| `POST` | `/{id}/reject` | Menolak sampel | `LabSpecimen : Accept` | `RejectLabSpecimenRequest` | `ApiResponse<LabSpecimenResponse>` | Tersedia |
| `POST` | `/{id}/request-recollection` | Meminta ambil ulang | `LabSpecimen : Accept` | `RequestLabRecollectionRequest` | `ApiResponse<LabSpecimenResponse>` | Tersedia |
| `POST` | `/{id}/hold` | Menahan sampel | `LabSpecimen : Hold` | `HoldLabRequest` | `ApiResponse<LabSpecimenResponse>` | Tersedia |
| `POST` | `/{id}/resume` | Melanjutkan sampel | `LabSpecimen : Hold` | `ResumeLabRequest` | `ApiResponse<LabSpecimenResponse>` | Tersedia |
| `POST` | `/{id}/cancel` | Membatalkan sampel | `LabSpecimen : Update` | `CancelLabSpecimenRequest` | `ApiResponse<LabSpecimenResponse>` | Tersedia |

**Batas kontrak as-is.** Pada `c87d9c0`, `PlanLabSpecimenRequest` membawa satu `ProcedureId`,
sehingga satu sampel sama dengan satu pemeriksaan. Ini yang diubah oleh `LAB-DEC-024`.

---

## 2. Kontrak To-Be — target yang disetujui pemilik modul

### Health Services / Laboratory Management / Lab Order

Base URL: `api/v1/health-services/laboratory-management/lab-orders`
Contract version: `LAB-API-v1` — status `approved`, dikunci 2026-09-02

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar pesanan dengan penyaring, pengurutan, dan pagination di sisi server | `LabOrder : Read` | `LabOrderPagedQuery` | `ApiResponse<PagedResult<LabOrderListResponse>>` | **Tersedia** — `r5`, `BE-LAB-18`. **Breaking**: bentuk respons berubah dari `List<T>` menjadi `PagedResult<T>` |
| `GET` | `/filters/metadata` | Pilihan penyaring, urutan, dan ukuran halaman untuk layar daftar pesanan | `LabOrder : Read` | — | `ApiResponse<LabOrderFilterMetadataResponse>` | **Tersedia** — `r4`, `BE-LAB-17` |
| `GET` | `/summary` | Rekap pesanan pada satu rentang waktu, per status dan per disiplin | `LabOrder : Read` | `startDate`, `endDate` | `ApiResponse<LabOrderSummaryResponse>` | **Tersedia** — `r4`, `BE-LAB-17` |
| `GET` | `/by-discipline/{discipline}` | Daftar pesanan per disiplin: Patologi Klinik, Patologi Anatomi, atau Mikrobiologi | `LabOrder : Read` | `LabOrderPagedQuery` | `ApiResponse<PagedResult<LabOrderListResponse>>` | **Tersedia** — `BE-LAB-15` |

Delapan endpoint pesanan yang sudah ada tetap berlaku apa adanya. `LabOrderDetailResponse`
bertambah satu ruas: `discipline` (`LAB-DEC-025`).

> **Penanda cito pindah.** Pada revision 1 kontrak ini, penandaan cito berada di
> `PUT /lab-orders/{id}/urgency`. `LAB-DEC-026` memindahkannya ke tingkat pemeriksaan, sehingga
> endpoint itu **dibatalkan** dan digantikan `PUT /lab-examinations/{id}/urgency` di bawah.

### Health Services / Laboratory Management / Lab Examination

Base URL: `api/v1/health-services/laboratory-management/lab-examinations`
Contract version: `LAB-API-v1` — status `approved`, dikunci 2026-09-02

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/by-order/{labOrderId}` | Daftar pemeriksaan terpesan pada satu pesanan | `LabExamination : Read` | — | `ApiResponse<List<LabExaminationResponse>>` | **Tersedia** — `BE-LAB-16` |
| `GET` | `/by-specimen/{specimenId}` | Daftar pemeriksaan yang ditopang satu wadah | `LabExamination : Read` | — | `ApiResponse<List<LabExaminationResponse>>` | **Tersedia** — `BE-LAB-16` |
| `POST` | `/by-order/{labOrderId}` | Menambah pemeriksaan terpesan dan menautkannya ke wadah | `LabExamination : Create` | `AddLabExaminationRequest` | `ApiResponse<LabExaminationResponse>` | **Tersedia** — `BE-LAB-16` |
| `POST` | `/{id}/cancel` | Membatalkan satu pemeriksaan terpesan | `LabExamination : Update` | `CancelLabExaminationRequest` | `ApiResponse<LabExaminationResponse>` | **Tersedia** — `BE-LAB-16` |
| `PUT` | `/{id}/urgency` | Menandai **satu pemeriksaan** sebagai cito atau mengembalikannya menjadi biasa | `LabExamination : Update` | `SetLabExaminationUrgencyRequest` | `ApiResponse<LabExaminationResponse>` | **Tersedia** — `BE-LAB-10` |
| `PUT` | `/{id}/duplo` | Menandai satu pemeriksaan dikerjakan ganda | `LabExamination : Update` | `SetLabExaminationDuploRequest` | `ApiResponse<LabExaminationResponse>` | **Tersedia** — `BE-LAB-10` |

`LabExaminationResponse` memuat `urgency`, `urgencyMarkedAt`, `urgencyMarkedByUserName`, dan
`isDuplo` (`LAB-DEC-026`).

### Health Services / Laboratory Management / Lab Specimen

Base URL: `api/v1/health-services/laboratory-management/lab-specimens`
Contract version: `LAB-API-v1` — status `approved`, dikunci 2026-09-02

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/filters/metadata` | Pilihan status wadah, sebab ambil ulang, urutan, dan ukuran halaman | `LabSpecimen : Read` | — | `ApiResponse<LabSpecimenFilterMetadataResponse>` | **Tersedia** — `r4`, `BE-LAB-17` |
| `GET` | `/summary` | Rekap wadah pada satu rentang waktu, per status dan per sebab ambil ulang | `LabSpecimen : Read` | `startDate`, `endDate` | `ApiResponse<LabSpecimenSummaryResponse>` | **Tersedia** — `r4`, `BE-LAB-17` |
| `POST` | `/by-order/{labOrderId}` | Merencanakan **satu wadah** beserta pemeriksaan yang ditopangnya | `LabSpecimen : Plan` | `PlanLabSpecimenRequest` **(berubah)** | `ApiResponse<LabSpecimenResponse>` | **Tersedia** — `BE-LAB-12` |
| `POST` | `/{id}/accept` | Menyatakan wadah layak; seluruh pemeriksaan yang ditopangnya menjadi layak tagih | `LabSpecimen : Accept` | `AcceptLabSpecimenRequest` | `ApiResponse<LabBillingHandoffResponse>` **(berubah)** | **Tersedia** — `BE-LAB-12`; penerbitan fakta per pemeriksaan menunggu `BE-LAB-13` |
| `POST` | `/{id}/reject` | Menolak wadah; menggugurkan seluruh pemeriksaan yang ditopangnya | `LabSpecimen : Accept` | `RejectLabSpecimenRequest` | `ApiResponse<LabSpecimenResponse>` **(berubah)** | **Tersedia** — `BE-LAB-12` |

Sembilan endpoint sampel lainnya tetap berlaku apa adanya.

### Health Services / Laboratory Management / Lab Value Bound

Base URL: `api/v1/health-services/laboratory-management/lab-value-bounds`
Contract version: `LAB-API-v1` — status `approved`, dikunci 2026-09-02

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/filters/metadata` | Pilihan bentuk hasil, jenis kelamin, urutan, ukuran halaman, dan penanda bahwa batas kritis hanya berubah lewat pengajuan | `LabValueBound : Read` | — | `ApiResponse<LabValueBoundFilterMetadataResponse>` | **Tersedia** — `r4`, `BE-LAB-17` |
| `GET` | `/summary` | Rekap batas nilai: aktif, nonaktif, per bentuk hasil, dan yang menunggu persetujuan batas kritis | `LabValueBound : Read` | — | `ApiResponse<LabValueBoundSummaryResponse>` | **Tersedia** — `r4`, `BE-LAB-17` |
| `GET` | `/` | Daftar batas nilai, dapat disaring per jenis pemeriksaan | `LabValueBound : Read` | `LabValueBoundPagedQuery` | `ApiResponse<PagedResult<LabValueBoundListResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Detail satu batas nilai beserta pilihannya | `LabValueBound : Read` | — | `ApiResponse<LabValueBoundDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Membuat batas nilai baru | `LabValueBound : Create` | `CreateLabValueBoundRequest` | `ApiResponse<LabValueBoundDetailResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Mengubah satuan, batas normal, batas waktu cito, dan daftar pilihan | `LabValueBound : Update` | `UpdateLabValueBoundRequest` | `ApiResponse<LabValueBoundDetailResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}/deactivate` | Menonaktifkan batas nilai | `LabValueBound : Update` | — | `ApiResponse<LabValueBoundDetailResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/history` | Riwayat perubahan batas nilai | `LabValueBound : Read` | — | `ApiResponse<List<LabValueBoundHistoryResponse>>` | **Rencana (belum tersedia)** |

### Health Services / Laboratory Management / Lab Critical Bound Approval

Base URL: `api/v1/health-services/laboratory-management/lab-value-bounds/{id}/critical-change-requests`
Contract version: `LAB-API-v1` — status `approved`, dikunci 2026-09-02

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/filters/metadata` | Pilihan status pengajuan, urutan, ukuran halaman, dan dua penanda keselamatan: larangan menyetujui pengajuan sendiri serta batas satu pengajuan belum diputuskan | `LabCriticalBound : Read` | — | `ApiResponse<LabCriticalBoundApprovalFilterMetadataResponse>` | **Tersedia** — `r4`, `BE-LAB-17` |
| `GET` | `/summary` | Rekap pengajuan untuk **satu** batas nilai, per status | `LabCriticalBound : Read` | — | `ApiResponse<LabCriticalBoundApprovalSummaryResponse>` | **Tersedia** — `r4`, `BE-LAB-17` |
| `GET` | `/` | Daftar pengajuan perubahan batas kritis | `LabCriticalBound : Read` | — | `ApiResponse<List<LabBoundChangeRequestResponse>>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Mengajukan perubahan batas kritis | `LabValueBound : Update` | `SubmitCriticalBoundChangeRequest` | `ApiResponse<LabBoundChangeRequestResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{requestId}/approve` | Menyetujui pengajuan; batas baru mulai berlaku | `LabCriticalBound : Approve` | `DecideCriticalBoundChangeRequest` | `ApiResponse<LabBoundChangeRequestResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{requestId}/reject` | Menolak pengajuan | `LabCriticalBound : Approve` | `DecideCriticalBoundChangeRequest` | `ApiResponse<LabBoundChangeRequestResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{requestId}/withdraw` | Menarik pengajuan sendiri | `LabValueBound : Update` | — | `ApiResponse<LabBoundChangeRequestResponse>` | **Rencana (belum tersedia)** |

### Health Services / Laboratory Management / Lab Worklist

Base URL: `api/v1/health-services/laboratory-management/lab-worklists`
Contract version: `LAB-API-v1` — status `approved`, dikunci 2026-09-02

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/pending` | Daftar kerja pekerjaan yang belum selesai, cito di urutan atas | `LabWorklist : Read` | `LabWorklistPagedQuery` | `ApiResponse<PagedResult<LabWorklistItemResponse>>` | **Tersedia** — `BE-LAB-14` |
| `GET` | `/cito-overdue` | Daftar pantau pesanan cito yang melewati batas waktu | `LabWorklist : Read` | `LabWorklistPagedQuery` | `ApiResponse<PagedResult<LabCitoOverdueResponse>>` | **Tersedia** — `BE-LAB-14` |

### Health Services / Laboratory Management / Lab Rejection Reason

Base URL: `api/v1/health-services/laboratory-management/lab-rejection-reasons`
Contract version: `LAB-API-v1` — status `approved`, dikunci 2026-09-02

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/filters/metadata` | Pilihan urutan, ukuran halaman, dan daftar ruas yang terkunci bagi kepala instalasi | `LabRejectionReason : Read` | — | `ApiResponse<LabRejectionReasonFilterMetadataResponse>` | **Tersedia** — `r4`, `BE-LAB-17` |
| `GET` | `/summary` | Rekap alasan penolakan: aktif, nonaktif, berpenanda kesalahan internal, dan wajib catatan | `LabRejectionReason : Read` | — | `ApiResponse<LabRejectionReasonSummaryResponse>` | **Tersedia** — `r4`, `BE-LAB-17` |
| `GET` | `/` | Daftar alasan penolakan untuk pengelolaan | `LabRejectionReason : Read` | `LabRejectionReasonPagedQuery` | `ApiResponse<PagedResult<LabRejectionReasonResponse>>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Menambah alasan penolakan | `LabRejectionReason : Create` | `CreateLabRejectionReasonRequest` | `ApiResponse<LabRejectionReasonResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Mengubah nama, keterangan, dan urutan | `LabRejectionReason : Update` | `UpdateLabRejectionReasonRequest` | `ApiResponse<LabRejectionReasonResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}/activation` | Mengaktifkan atau menonaktifkan | `LabRejectionReason : Update` | `SetLabRejectionReasonActivationRequest` | `ApiResponse<LabRejectionReasonResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}/system-flags` | Menyetel penanda kesalahan internal dan penanda wajib catatan | `LabRejectionReason : SystemFlag` | `SetLabRejectionReasonSystemFlagsRequest` | `ApiResponse<LabRejectionReasonResponse>` | **Rencana (belum tersedia)** |

`GET /lab-specimens/rejection-reasons` yang sudah ada **tetap dipertahankan** sebagai jalur baca
bagi petugas yang sedang menolak sampel. Endpoint pengelolaan di atas adalah jalur terpisah.

### Health Services / Laboratory Management / Lab Patient Registration

Base URL: `api/v1/health-services/laboratory-management/lab-patient-registrations`
Contract version: `LAB-API-v1` — status `approved`, dikunci 2026-09-02

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/patient-search` | Mencari pasien terdaftar sebelum mendaftarkan yang baru | `LabPatientRegistration : Read` | `LabPatientSearchQuery` | `ApiResponse<List<LabPatientSearchResponse>>` | **Rencana (belum tersedia)** |
| `POST` | `/walk-in` | Mendaftarkan pasien datang langsung; memanggil Registrasi lalu mengembalikan penunjuk kunjungan | `LabPatientRegistration : Create` | `RegisterLabWalkInRequest` | `ApiResponse<LabRegistrationResultResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/external-referral` | Mendaftarkan pasien rujukan luar beserta instansi dan dokter perujuknya | `LabPatientRegistration : Create` | `RegisterLabExternalReferralRequest` | `ApiResponse<LabRegistrationResultResponse>` | **Rencana (belum tersedia)** |

**Yang perlu dipahami tentang tiga endpoint ini.** Ketiganya **tidak membuat kunjungan sendiri**.
Endpoint pendaftaran meneruskan isian ke Registrasi, menunggu jawabannya, lalu mengembalikan
penunjuk kunjungan yang dibuat Registrasi. Bila Registrasi menolak, penolakan itu diteruskan apa
adanya dan **tidak ada data yang disimpan Laboratorium**.

`LabRegistrationResultResponse` memuat penunjuk kunjungan, nomor kunjungan, dan identitas pasien
seadanya — cukup untuk langsung membuat pesanan lab pada layar berikutnya.

### Health Services / Laboratory Management / Lab Catalog

Base URL: `api/v1/health-services/laboratory-management/lab-catalog`
Contract version: `LAB-API-v1` — status `approved`, dikunci 2026-09-02

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/examinations` | Daftar pemeriksaan laboratorium yang dapat dipesan, disaring per disiplin | `LabCatalog : Read` | `LabCatalogQuery` | `ApiResponse<PagedResult<LabCatalogItemResponse>>` | **Tersedia** — `BE-LAB-07` |
| `GET` | `/examinations/{procedureId}/price` | Harga berlaku dan status cakupan penjamin untuk satu pemeriksaan | `LabCatalog : Read` | `LabPriceQuery` | `ApiResponse<LabPriceResponse>` | **Tersedia** — `BE-LAB-07` |
| `GET` | `/tariffs` | Tampilan tersaring daftar tarif pemeriksaan laboratorium — **baca saja** | `LabCatalog : Read` | `LabTariffQuery` | `ApiResponse<PagedResult<LabTariffViewResponse>>` | **Tersedia** — `BE-LAB-07` |

`LabCatalogItemResponse` memuat nama pemeriksaan, disiplin, harga satuan berlaku, dan penanda
tercakup penjamin. `LabPriceResponse` memuat harga rumah sakit, harga kontrak penjamin bila ada,
dan penanda tidak tercakup.

**Batas yang tegas.** Seluruh grup ini **baca saja**. Tidak ada `POST`, `PUT`, maupun `DELETE`.
Pengubahan tarif dilakukan lewat modul Master Data (`LAB-DEC-033`).

### Health Services / Laboratory Management / Lab Monitoring

Base URL: `api/v1/health-services/laboratory-management/lab-monitoring`
Contract version: `LAB-API-v1` — status `approved`, dikunci 2026-09-02

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/clinical-pathology` | Daftar pantau pesanan Patologi Klinik | `LabMonitoring : Read` | `LabMonitoringQuery` | `ApiResponse<PagedResult<LabMonitoringItemResponse>>` | **Tersedia** — `BE-LAB-15` |
| `GET` | `/anatomic-pathology` | Daftar pantau pesanan Patologi Anatomi | `LabMonitoring : Read` | `LabMonitoringQuery` | `ApiResponse<PagedResult<LabMonitoringItemResponse>>` | **Tersedia** — `BE-LAB-15` |
| `GET` | `/microbiology` | Daftar pantau pesanan Mikrobiologi | `LabMonitoring : Read` | `LabMonitoringQuery` | `ApiResponse<PagedResult<LabMonitoringItemResponse>>` | **Tersedia** — `BE-LAB-15` |

Ketiganya memakai penyaring yang sama: pasien, nomor rekam medis, nomor pesanan, periode, jenis
kunjungan, unit atau ruangan, penjamin, status pesanan, status wadah, dan penanda cito.

**Kenapa tiga jalur terpisah, bukan satu dengan penyaring disiplin.** Bukti lapangan menunjukkan
laboratorium memakai **tiga daftar sejajar** sebagai tiga menu berbeda, karena petugasnya pun
berbeda. Menyatukannya menjadi satu jalur berpenyaring akan memaksa petugas memilih disiplin
setiap kali membuka layar.

---

## 3. Dampak Kompatibilitas

| Perubahan | Sifat | Yang terdampak |
|---|---|---|
| `PlanLabSpecimenRequest` menerima daftar `ProcedureId`, bukan satu | **Breaking** | Pemanggil endpoint rencana sampel |
| `LabSpecimenResponse` tidak lagi memuat jenis pemeriksaan dan tarif | **Breaking** | Pemanggil yang membaca tarif dari sampel |
| `LabBillingHandoffResponse` memuat daftar pemeriksaan, bukan satu | **Breaking** | Pemanggil endpoint menyatakan layak |
| `LabOrderDetailResponse` bertambah tiga ruas kesegeraan | Aman | Penambahan ruas tidak memecah pembaca lama |
| Enam grup endpoint baru | Aman | Tidak menyentuh yang sudah ada |

**Kenapa breaking change ini berbiaya rendah.** Capability map `CAP-21` membuktikan **tidak ada
satu pun pemanggil di frontend**. Dua puluh endpoint Laboratorium selama ini tidak punya layar.
Sepanjang tidak ada pemanggil di luar repository, perubahan ini tidak memutus siapa pun. Bila
ternyata ada pemanggil luar, `LAB-OPEN-012` wajib dijawab lebih dulu.

---

## 4. Kode Status dan Artinya bagi Pengguna

| Kode | Arti bagi pengguna |
|---|---|
| `200` | Permintaan berhasil |
| `201` | Data berhasil dibuat |
| `400` | Isian yang dikirim tidak lengkap atau formatnya salah |
| `401` | Pengguna belum masuk atau sesinya sudah berakhir |
| `403` | Pengguna tidak punya hak akses untuk tindakan ini |
| `404` | Data yang dicari tidak ditemukan |
| `409` | Data sedang diubah petugas lain, atau tindakan ini tidak sah pada status sekarang |
| `422` | Aturan bisnis dilanggar, misalnya menolak wadah tanpa alasan terkendali |
| `500` | Terjadi kesalahan pada sistem |

**Kode `409` paling sering muncul pada dua keadaan.** Pertama, dua petugas menyatakan layak
wadah yang sama pada waktu hampir bersamaan — hanya satu yang berhasil, yang lain diminta
memuat ulang. Kedua, tindakan tidak sah pada status sekarang, misalnya menyatakan layak wadah
yang belum pernah diterima di laboratorium.

---

## 5. Traceability

| Endpoint baru | Decision ID | Acceptance criteria |
|---|---|---|
| `PUT /lab-orders/{id}/urgency` | `LAB-DEC-013` | AC-18 |
| Grup Lab Examination | `LAB-DEC-024` | AC-35, AC-37 |
| `POST /lab-specimens/{id}/reject` yang berubah | `LAB-DEC-024` | AC-36 |
| Grup Lab Value Bound | `LAB-DEC-006`, `LAB-DEC-018`, `LAB-DEC-021` | AC-24, AC-25, AC-28 |
| Grup Lab Critical Bound Approval | `LAB-DEC-023` | AC-33, AC-34 |
| Grup Lab Worklist | `LAB-DEC-013` | AC-10, AC-17 |
| Grup Lab Rejection Reason | `LAB-DEC-019` | AC-26 |
