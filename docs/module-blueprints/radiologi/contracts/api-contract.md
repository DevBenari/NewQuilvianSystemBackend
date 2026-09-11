# API Contract — Modul Radiologi

| Field | Value |
|---|---|
| Contract version | `RAD-API-001` |
| Revision | `10` |
| Status | `approved` |
| Backend SHA | `50ccf615` |
| Input | `RAD-ARCH-BE-001`, `RAD-DA-001-r1` |
| Owner | Yoga Aji Pratama |
| `approved_by` / `approved_at` | Yoga Aji Pratama, 2026-09-10 |

> **Amandemen revision 3 — 2026-09-10.** Disetujui pemilik modul setelah `BE-RAD-03` selesai.
> Tiga perubahan, seluruhnya pada grup *Master Data / Rad Safety Rule*:
>
> 1. Delapan endpoint yang semula berlabel `Rencana (belum tersedia)` kini **`Tersedia`**.
> 2. Tiga endpoint ditambahkan: `GET /filters/metadata` dan `GET /summary` karena diwajibkan
>    baseline standar endpoint, serta **`GET /{id}`** karena form ubah memerlukan pembacaan satu
>    baris — memakai `GET /` berfilter untuk keperluan itu boros dan janggal.
> 3. Tiga endpoint baseline master data dinyatakan **sengaja tidak dibuat** beserta alasannya.

> **Amandemen revision 4 — 2026-09-11.** Menyusul `BE-RAD-04`. Grup *Master Data / Rad Modality*
> berpindah dari `Rencana (belum tersedia)` menjadi **berjalan**, dengan **sembilan** endpoint:
> lima yang semula direncanakan, ditambah empat baseline wajib `master-data-endpoint-standard.md`
> — `filters/metadata`, `summary`, `options`, dan `PATCH /{id}/status`.
>
> `DELETE /{id}` diperjelas artinya: **menandai terhapus**, bukan sekadar menonaktifkan.
> Menyalakan dan mematikan alat kini punya endpointnya sendiri.

> **Amandemen revision 5 — 2026-09-11.** Menyusul `BE-RAD-05`. Grup *Master Data / Rad Safety
> Requirement* berpindah menjadi **berjalan** dengan sembilan endpoint, bentuknya sama seperti
> Rad Modality. `DELETE /{id}` di grup itu juga diperjelas sebagai **menandai terhapus**, dan
> penonaktifan diberi endpointnya sendiri.
>
> Dengan ini seluruh gelombang `MVP-0` tersedia: aturan keselamatan dapat disusun dan disahkan,
> alat pencitraan dan butir keselamatannya dapat dikelola, dan gerbang menilai `RuleStatus`.

> **Amandemen revision 6 — 2026-09-11. Ditulis pelaksana `BE-RAD-09`, DISETUJUI pemilik modul
> Yoga Aji Pratama, 2026-09-11.** Berbeda dari revision 3 sampai 5 yang disetujui lebih dulu,
> catatan ini ditulis menyusul pekerjaan lalu dibaca dan disetujui pemilik modul pada hari yang
> sama, bersama revision 7 sampai 10.
>
> Tiga hal pada grup *Rad Report*:
>
> 1. **Delapan endpoint berpindah menjadi berjalan**: `GET /`, `GET /{id}`,
>    `GET /by-study/{radStudyId}`, `GET /by-encounter/{encounterId}`,
>    `POST /by-study/{radStudyId}/draft`, `PUT /{id}/draft`, `POST /{id}/validate`, dan
>    `POST /{id}/release`. Dua endpoint sisanya — `GET /{id}/versions` dan
>    `POST /{id}/amendments` — **tetap rencana**, karena keduanya milik `BE-RAD-10`.
> 2. **Dua endpoint baca ditambahkan**: `GET /filters/metadata` dan `GET /summary`. Keduanya
>    baseline wajib `transaction-endpoint-standard.md` bagian 4, dengan alasan yang sama seperti
>    penambahan pada revision 4 dan 5. **Tidak ada string hak akses baru** — keduanya memakai
>    `RadReport : Read` yang sudah ada.
> 3. **`RadReportValidateRequest` tidak dibuat.** Kontrak menyebut namanya, tetapi
>    `RAD-STATE-001` bagian 3 dan `RAD-VAL-001` bagian 1 tidak menetapkan satu pun isian untuk
>    pengesahan, dan `RAD-ERD-DICT-001` tidak punya kolom yang dapat diisinya.
>    `POST /{id}/validate` karena itu tidak menerima badan permintaan. Bila kelak pengesahan
>    perlu membawa catatan, kolomnya harus ditambahkan lebih dulu.
>
> **Selisih yang dicatat apa adanya.** Bagian 4 memberi contoh `201` untuk "Draf bacaan
> tersimpan". Yang dikerjakan adalah `200`, mengikuti seluruh endpoint create modul Radiologi
> yang sudah berjalan — `RadModality`, `RadSafetyRequirement`, dan `RadSafetyRule` — dan
> mengikuti `ApiResponse<T>.Ok` yang menuliskan `200` pada badan jawabannya. Menjadikan satu
> endpoint ini `201` akan membuat modul menjawab dua bentuk berbeda untuk perbuatan yang sama.

> **Amandemen revision 7 — 2026-09-11. Ditulis pelaksana `BE-RAD-10`, DISETUJUI pemilik modul
> Yoga Aji Pratama, 2026-09-11.** Dua endpoint terakhir grup *Rad Report* berpindah menjadi berjalan:
> `GET /{id}/versions` dan `POST /{id}/amendments`. Dengan ini grup *Rad Report* **lengkap**.
>
> **`RadReportDetailResponse` bertambah satu field: `WorkingVersionNumber`.** Ia menyebut nomor
> versi yang **sedang dikerjakan**, sementara `CurrentVersionNumber` tetap menyebut versi yang
> **berlaku bagi pembaca**. Selama koreksi disusun keduanya berbeda satu angka, dan perbedaan
> itulah yang memberi tahu layar bahwa ada koreksi yang belum sah — tanpa pernah menawarkan draf
> koreksi sebagai hasil yang berlaku. Penambahan field pada response tidak merusak pemanggil
> lama.
>
> **Selisih kontrak yang ditemukan dan perlu Anda putuskan.** `RAD-STATE-001` bagian 3 menulis
> pada baris "Tulis draf koreksi": *"Versi lama menjadi `Superseded`"* — seolah perpindahan itu
> terjadi saat draf koreksi ditulis. Bagian 4 dokumen yang sama, dan contoh `FR-RAD-020` pada
> `04-prd-to-mvp.md`, menyatakan sebaliknya: versi lama menjadi `Superseded` **ketika koreksinya
> dirilis**.
>
> Yang dikerjakan mengikuti bagian 4 dan `FR-RAD-020`, karena bacaan penerapannya lebih aman —
> lihat `task/report/backend/BE-RAD-10.md` bagian 2.3. Kalimat pada bagian 3 perlu diperbaiki
> agar tidak terbaca sebaliknya oleh implementer berikutnya.

> **Amandemen revision 8 — 2026-09-11. Ditulis pelaksana `BE-RAD-11`, DISETUJUI pemilik modul
> Yoga Aji Pratama, 2026-09-11.** Satu **perubahan perilaku** pada endpoint yang sudah berjalan sejak
> revision 6, karena itu dicatat tegas:
>
> **`GET /by-encounter/{encounterId}` kini hanya mengembalikan bacaan yang sudah pernah
> dirilis.** Bacaan berstatus `Pending`, `Drafted`, dan `Validated` **tidak muncul sama sekali**
> — bukan muncul dengan penanda, dan bukan muncul dalam keadaan terkunci. Ini penerapan
> `FR-RAD-032` dan `RAD-INT-001` bagian 2.
>
> **Pemanggil yang perlu melihat draf** — layar kerja Radiologi, bukan pembaca hasil — memakai
> `GET /?encounterId=…` yang tidak disaring. Pemisahan itu disengaja: satu endpoint untuk
> pekerjaan radiologi, satu endpoint untuk pembaca hasil.
>
> **Apa yang berubah bagi pemanggil lama.** Frontend yang dibangun terhadap revision 6 atau 7
> akan melihat **lebih sedikit baris** daripada sebelumnya. Tidak ada bentuk data yang berubah
> dan tidak ada field yang hilang; yang berubah adalah baris mana yang layak ditampilkan kepada
> dokter pengirim.

> **Amandemen revision 9 — 2026-09-11. Ditulis pelaksana `BE-RAD-12`, DISETUJUI pemilik modul
> Yoga Aji Pratama, 2026-09-11.** Penanda cito pada grup *Rad Order* — `RAD-DEC-013`.
>
> Dua perubahan yang semula tercatat sebagai rencana pada bagian *Rad Order — tambahan* kini
> **berjalan**:
>
> 1. **`POST /` menerima `IsUrgent`** yang boleh kosong, bawaannya `false`. Pemanggil lama yang
>    tidak mengirim field itu tetap berhasil — dibuktikan uji, dan dibuktikan lagi oleh 481 uji
>    modul Rawat Inap yang memanggil endpoint ini dan seluruhnya tetap lulus.
> 2. **`GET /` dan `GET /{id}` memuat `IsUrgent`.**
>
> **Satu penambahan di luar teks kontrak, dicatat apa adanya.** `RadOrderDetailResponse` juga
> memuat **`UrgentMarkedByUserId`** dan **`UrgentMarkedAt`**. Keduanya tidak disebut kontrak,
> tetapi `RAD-ERD-DICT-001` bagian 4 menetapkan kolomnya dan AC-42 menuntut penanda cito tercatat
> siapa yang menetapkannya dan kapan — tanpa membawanya ke response, kriteria itu tidak dapat
> dibaca siapa pun di luar database. Keduanya hanya pada **rincian**, bukan pada daftar:
> pertanyaan "siapa yang menandai cito" ditanyakan atas satu pesanan tertentu, bukan atas
> seluruh layar. Penambahan field pada response tidak merusak pemanggil lama.
>
> **Yang tetap rencana:** `GET /worklist` dan `PUT /{id}/urgency`, keduanya milik `BE-RAD-13`.

> **Amandemen revision 10 — 2026-09-11. Ditulis pelaksana `BE-RAD-13`, DISETUJUI pemilik modul
> Yoga Aji Pratama, 2026-09-11.** Dua endpoint terakhir grup *Rad Order — tambahan* berpindah menjadi
> **berjalan**: `GET /worklist` dan `PUT /{id}/urgency`. Dengan ini **seluruh endpoint backend
> modul Radiologi pada roadmap sudah ada.**
>
> **Perilaku `GET /worklist` yang perlu diketahui pembuat layar:**
>
> | Parameter | Ketentuan |
> | --- | --- |
> | `modalityId` | **Wajib.** Tanpanya dijawab `400`, bukan daftar seluruh rumah sakit |
> | `date` | Opsional. **Kosong berarti hari ini**, dibaca sebagai tanggal kalender **Waktu Indonesia Barat** — bukan UTC |
> | `status` | Opsional. Kosong berarti seluruh keadaan, termasuk yang sudah selesai |
>
> **Mengapa harinya WIB dan bukan UTC.** Selisihnya tujuh jam: shift pagi yang mulai pukul 06.00
> WIB berjalan pada pukul 23.00 UTC **hari sebelumnya**. Memakai tanggal UTC akan menyajikan
> daftar kerja kemarin kepada petugas yang baru masuk, tepat pada jam ketika seluruh pekerjaan
> hari itu belum satu pun terlihat.
>
> **Bawaan "hari ini" berada di luar teks kontrak** — kontrak hanya menyebut `date` opsional
> tanpa menetapkan artinya bila kosong. Dipilih begitu karena daftar kerja tanpa batas hari akan
> memuat seluruh riwayat alat tersebut, dan `GET /worklist` tidak berhalaman.
>
> **Selisih terhadap `transaction-endpoint-standard.md`, dicatat apa adanya.** Standar itu
> menyatakan `PUT /{id}/<aksi>` **dilarang**, dan menempatkan perubahan satu atribut tanpa
> perpindahan status sebagai `PATCH /{id}/<field>`. Kontrak ini menuliskannya `PUT /{id}/urgency`,
> dan itu yang dikerjakan — dengan dua alasan: kontrak modul adalah target yang terkunci bagi
> task, dan **seluruh dua belas endpoint aksi pada `RadOrderController` yang sudah berjalan
> memakai `PUT`**. Menjadikan penanda cito satu-satunya `PATCH` di controller itu akan membuat
> permukaannya tidak dapat ditebak.
>
> **Keputusan pemilik modul, 2026-09-11: `PUT` dipertahankan.** Menyeragamkan `urgency` saja
> dengan standar akan membuat satu endpoint berbeda sendiri dari sebelas endpoint aksi lain pada
> controller yang sama — dan permukaan yang tidak dapat ditebak lebih mahal bagi pembuat layar
> daripada selisih terhadap standar yang sudah dicatat terang-terangan di sini. Penyeragaman
> seluruh `RadOrderController`, bila kelak diinginkan, menjadi task tersendiri yang menyentuh dua
> belas endpoint beserta konsumennya sekaligus. Rinciannya di
> `task/report/backend/BE-RAD-13.md` bagian 3.4.

Endpoint yang **belum ada di kode** diberi label `Rencana (belum tersedia)`. Yang tidak berlabel
sudah dapat dipakai sekarang.

---

## 1. As-Is — Endpoint yang Sudah Tersedia

### Health Services / Radiology Management / Rad Order

Base URL: `api/v1/health-services/radiology-management/rad-orders`
Contract version: `v1` — status **berjalan**

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Melihat daftar pesanan dengan penyaringan dan halaman | `RadOrder : Read` | Query | `ApiResponse<PagedResult<RadOrderListResponse>>` | Tersedia |
| `GET` | `/{id}` | Melihat rincian satu pesanan | `RadOrder : Read` | — | `ApiResponse<RadOrderDetailResponse>` | Tersedia |
| `GET` | `/episodes/{episodeId}` | Melihat pesanan milik satu kunjungan | `RadOrder : Read` | — | `ApiResponse<List<RadOrderListResponse>>` | Tersedia |
| `POST` | `/` | Dokter membuat pesanan baru | `RadOrder : Create` | `CreateRadOrderRequest` | `ApiResponse<RadOrderDetailResponse>` | Tersedia |
| `PUT` | `/{id}/accept` | Radiologi menerima pesanan | `RadOrder : Process` | `RadOrderTransitionRequest` | `ApiResponse<RadOrderDetailResponse>` | Tersedia |
| `PUT` | `/{id}/schedule` | Menjadwalkan pemeriksaan | `RadOrder : Schedule` | `RadOrderTransitionRequest` | `ApiResponse<RadOrderDetailResponse>` | Tersedia |
| `PUT` | `/{id}/start` | Menandai mulai dikerjakan | `RadOrder : Process` | `RadOrderTransitionRequest` | `ApiResponse<RadOrderDetailResponse>` | Tersedia |
| `PUT` | `/{id}/complete` | Menandai selesai dikerjakan | `RadOrder : Process` | `RadOrderTransitionRequest` | `ApiResponse<RadOrderDetailResponse>` | Tersedia |
| `PUT` | `/{id}/hold` | Menahan pesanan sementara | `RadOrder : Hold` | `RadOrderTransitionRequest` | `ApiResponse<RadOrderDetailResponse>` | Tersedia |
| `PUT` | `/{id}/resume` | Melanjutkan pesanan yang tertahan | `RadOrder : Hold` | `RadOrderTransitionRequest` | `ApiResponse<RadOrderDetailResponse>` | Tersedia |
| `PUT` | `/{id}/reject` | Radiologi menolak pesanan | `RadOrder : Update` | `RadOrderTransitionRequest` | `ApiResponse<RadOrderDetailResponse>` | Tersedia |
| `PUT` | `/{id}/cancel` | Membatalkan pesanan | `RadOrder : Cancel` | `RadOrderTransitionRequest` | `ApiResponse<RadOrderDetailResponse>` | Tersedia |

### Health Services / Radiology Management / Rad Study

Base URL: `api/v1/health-services/radiology-management/rad-studies`
Contract version: `v1` — status **berjalan**

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/modalities` | Melihat daftar alat pencitraan | `RadStudy : Read` | — | `ApiResponse<List<RadModalityResponse>>` | Tersedia — **akan digantikan**, lihat bagian 3 |
| `GET` | `/safety-requirements` | Melihat daftar butir keselamatan | `RadStudy : Read` | — | `ApiResponse<List<RadSafetyRequirementResponse>>` | Tersedia — **akan digantikan** |
| `GET` | `/by-order/{radOrderId}` | Melihat study milik satu pesanan | `RadStudy : Read` | — | `ApiResponse<List<RadStudyResponse>>` | Tersedia |
| `GET` | `/by-order/{radOrderId}/history` | Melihat riwayat perpindahan status | `RadStudy : Read` | — | `ApiResponse<List<RadTransitionHistoryResponse>>` | Tersedia |
| `POST` | `/by-order/{radOrderId}` | Membuat rencana pengambilan citra | `RadStudy : Create` | `CreateRadStudyRequest` | `ApiResponse<RadStudyResponse>` | Tersedia |
| `POST` | `/{id}/verify-patient` | Memastikan identitas pasien benar | `RadStudy : Verify` | — | `ApiResponse<RadStudyResponse>` | Tersedia |
| `POST` | `/{id}/safety-checks` | Mengisi jawaban butir keselamatan | `RadStudy : Safety` | `RadSafetyCheckDecisionRequest` | `ApiResponse<RadStudyResponse>` | Tersedia |
| `POST` | `/{id}/clear-safety` | Menyatakan gerbang keselamatan lolos | `RadStudy : Safety` | — | `ApiResponse<RadStudyResponse>` | Tersedia |
| `POST` | `/{id}/start-acquisition` | Mulai mengambil citra | `RadStudy : Acquire` | — | `ApiResponse<RadStudyResponse>` | Tersedia |
| `POST` | `/{id}/complete-acquisition` | Selesai mengambil citra | `RadStudy : Acquire` | — | `ApiResponse<RadStudyResponse>` | Tersedia |
| `POST` | `/{id}/abort-acquisition` | Menghentikan di tengah jalan | `RadStudy : Acquire` | `RadAbortAcquisitionRequest` | `ApiResponse<RadStudyResponse>` | Tersedia |
| `POST` | `/{id}/decide-quality` | Menyatakan citra layak atau tidak | `RadStudy : Quality` | `RadAcquisitionQualityRequest` | `ApiResponse<RadStudyActionResult>` | Tersedia |
| `POST` | `/{id}/repeat` | Membuat study pengulangan | `RadStudy : Repeat` | `RadRepeatStudyRequest` | `ApiResponse<RadStudyResponse>` | Tersedia |
| `POST` | `/{id}/consumptions` | Mencatat bahan terpakai | `RadStudy : Consumption` | `RadConsumptionRequest` | `ApiResponse<RadConsumptionResponse>` | Tersedia |

---

## 2. To-Be — Endpoint Baru

### Health Services / Radiology Management / Rad Order — tambahan

Base URL: `api/v1/health-services/radiology-management/rad-orders`
Contract version: `v2` — status `draft`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/worklist` | **Daftar kerja petugas pada satu alat**, pesanan cito di urutan atas | `RadOrder : Read` | Query: `modalityId` wajib, `date`, `status` | `ApiResponse<List<RadWorklistItemResponse>>` | Tersedia sejak `BE-RAD-13` |
| `PUT` | `/{id}/urgency` | Mengubah penanda cito setelah pesanan dibuat | `RadOrder : Update` | `RadOrderUrgencyRequest` | `ApiResponse<RadOrderDetailResponse>` | Tersedia sejak `BE-RAD-13` |

**Perubahan pada endpoint yang sudah ada:**

| Endpoint | Perubahan | Kompatibilitas |
|---|---|---|
| `POST /` | `CreateRadOrderRequest` menerima field `IsUrgent` yang **boleh kosong**, bawaannya `false` | **Aman.** Pemanggil lama yang tidak mengirim field itu tetap berjalan. **Berjalan sejak `BE-RAD-12`** |
| `GET /`, `GET /{id}` | Response memuat `IsUrgent`; rincian juga memuat `UrgentMarkedByUserId` dan `UrgentMarkedAt` | **Aman.** Penambahan field pada response tidak merusak pemanggil lama. **Berjalan sejak `BE-RAD-12`** |
| `GET /`, `GET /{id}`, `GET /episodes/{episodeId}` | Response memuat `OrderNumber` dan objek `Patient` berisi konteks pasien dan kunjungan — No. RM, nama, jenis kelamin, tanggal lahir, umur saat kunjungan, No. registrasi, jenis kunjungan, unit layanan, ruangan, kelas, jenis penjamin, dan nama penjamin | **Aman.** Penambahan field. **Berjalan sejak `RAD-CONF-001` bagian 8** |
| `GET /{id}` | Rincian juga memuat `ConfirmedByUserId` dan `ConfirmedAt`, diturunkan dari baris `Order.Accept` pada `RadTransitionHistory` — bukan kolom baru | **Aman.** Penambahan field. **Berjalan sejak `RAD-CONF-001` bagian 8** |
| `GET /` | Menerima `limit`, bawaannya `200` dan paling banyak `1000`. Daftar ini belum memakai `PagedResult` — perpindahannya tetap task tersendiri karena merusak bentuk response — sehingga batas baris inilah yang menjaga pencarian se-rumah-sakit tetap terbatas | **Aman.** Bawaannya jauh di atas jumlah pesanan satu kunjungan mana pun, sehingga pemanggil lama tidak menyentuhnya. `GET /episodes/{episodeId}` sengaja tetap tanpa batas |
| `GET /` | Menerima 19 parameter penyaring dan pengurutan: `encounterId`, `patientId`, `medicalRecordNumber`, `encounterNumber`, `orderNumber`, `studyNumber`, `startDate`, `endDate`, `encounterType`, `serviceUnitId`, `roomId`, `modalityId`, `procedureId`, `orderStatus`, `onlyUrgent`, `onlyScheduled`, `scheduledFrom`, `scheduledTo`, `search`. Daftarnya juga terbit pada `GET /filters/metadata` | **Aman.** Seluruh parameter opsional; `encounterId`, `sortBy`, dan `sortDirection` tetap bernama sama sehingga pemanggil lama tidak berubah. **Berjalan sejak `RAD-CONF-001` bagian 8** |
| `POST /` | Pesanan baru mendapat `OrderNumber` berbentuk `RAD-ORD-yyMMddHHmmss-XXXXXX`, dijaga index unik | **Aman.** Dibentuk server; pemanggil tidak mengirim apa pun. **Berjalan sejak `RAD-CONF-001` bagian 8** |

> **Mengapa `GET /worklist` diletakkan pada controller pesanan, bukan controller study.**
> Daftar kerja dimulai dari pesanan yang sudah diterima tetapi belum tentu punya study. Kalau
> diletakkan di controller study, pekerjaan yang belum direncanakan sama sekali tidak akan
> muncul — padahal justru itu yang paling perlu dikerjakan.

### Health Services / Radiology Management / Rad Report

Base URL: `api/v1/health-services/radiology-management/rad-reports`
Contract version: `v1` — status `draft`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/filters/metadata` | Melihat pilihan penyaring, pengurutan, dan aksi layar bacaan | `RadReport : Read` | — | `ApiResponse<RadReportFilterMetadataResponse>` | Tersedia sejak `BE-RAD-09` |
| `GET` | `/summary` | Melihat rekap jumlah bacaan per keadaan | `RadReport : Read` | — | `ApiResponse<RadReportSummaryResponse>` | Tersedia sejak `BE-RAD-09` |
| `GET` | `/` | Melihat daftar bacaan dengan penyaringan dan halaman | `RadReport : Read` | Query | `ApiResponse<PagedResult<RadReportListResponse>>` | Tersedia sejak `BE-RAD-09` |
| `GET` | `/{id}` | Melihat bacaan beserta versi yang sedang berlaku | `RadReport : Read` | — | `ApiResponse<RadReportDetailResponse>` | Tersedia sejak `BE-RAD-09` |
| `GET` | `/{id}/versions` | Melihat seluruh versi bacaan, terbaru lebih dulu | `RadReport : Read` | — | `ApiResponse<List<RadReportVersionResponse>>` | Tersedia sejak `BE-RAD-10` |
| `GET` | `/by-study/{radStudyId}` | Melihat bacaan atas satu study | `RadReport : Read` | — | `ApiResponse<RadReportDetailResponse>` | Tersedia sejak `BE-RAD-09` |
| `GET` | `/by-encounter/{encounterId}` | Melihat bacaan satu kunjungan yang **sudah dirilis** — dipakai rekam medis | `RadReport : Read` | — | `ApiResponse<List<RadReportListResponse>>` | Tersedia sejak `BE-RAD-09`; **disaring sejak `BE-RAD-11`**, lihat revision 8 |
| `POST` | `/by-study/{radStudyId}/draft` | Menulis draf bacaan | `RadReport : Create` | `CreateRadReportDraftRequest` | `ApiResponse<RadReportDetailResponse>` | Tersedia sejak `BE-RAD-09` |
| `PUT` | `/{id}/draft` | Mengubah draf yang belum disahkan | `RadReport : Update` | `UpdateRadReportDraftRequest` | `ApiResponse<RadReportDetailResponse>` | Tersedia sejak `BE-RAD-09` |
| `POST` | `/{id}/validate` | Mengesahkan bacaan | `RadReport : Validate` | — *(lihat revision 6)* | `ApiResponse<RadReportDetailResponse>` | Tersedia sejak `BE-RAD-09` |
| `POST` | `/{id}/release` | Merilis bacaan ke dokter pengirim | `RadReport : Release` | — | `ApiResponse<RadReportDetailResponse>` | Tersedia sejak `BE-RAD-09` |
| `POST` | `/{id}/amendments` | Menulis draf koreksi atas bacaan yang sudah dirilis | `RadReport : Amend` | `CreateRadReportAmendmentRequest` | `ApiResponse<RadReportDetailResponse>` | Tersedia sejak `BE-RAD-10` |

> **`RadReportDetailResponse` membawa `AvailableActions`.** Daftar aksi yang masuk akal atas
> bacaan itu menurut keadaannya, sesuai `transaction-endpoint-standard.md` bagian 5.1. Ia
> **bantuan tampilan, bukan pengaman**: daftarnya diturunkan dari keadaan bacaan saja dan tidak
> tahu siapa yang sedang melihat, sehingga setiap endpoint aksi tetap memeriksa ulang kelayakan
> status dan kewenangan pelakunya di backend.

### Health Services / Radiology Management / Master Data / Rad Modality

Base URL: `api/v1/health-services/radiology-management/master-data/rad-modalities`
Contract version: `v1` — status **berjalan** sejak `BE-RAD-04`, 2026-09-11

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/filters/metadata` | Pilihan penyaring dan pengurutan layar daftar | `RadModality : Read` | — | `ApiResponse<RadModalityFilterMetadataResponse>` | Tersedia |
| `GET` | `/summary` | Rekap jumlah alat, termasuk yang belum punya aturan keselamatan | `RadModality : Read` | — | `ApiResponse<RadModalitySummaryResponse>` | Tersedia |
| `GET` | `/` | Melihat daftar alat pencitraan | `RadModality : Read` | Query | `ApiResponse<PagedResult<RadModalityResponse>>` | Tersedia |
| `GET` | `/options` | Pilihan ringan untuk dropdown pada form lain | `RadModality : Read` | Query | `ApiResponse<List<RadModalityOptionResponse>>` | Tersedia |
| `GET` | `/{id}` | Melihat rincian satu alat beserta jejak auditnya | `RadModality : Read` | — | `ApiResponse<RadModalityDetailResponse>` | Tersedia |
| `POST` | `/` | Mendaftarkan alat baru | `RadModality : Create` | `CreateRadModalityRequest` | `ApiResponse<RadModalityDetailResponse>` | Tersedia |
| `PUT` | `/{id}` | Mengubah data alat | `RadModality : Update` | `UpdateRadModalityRequest` | `ApiResponse<RadModalityDetailResponse>` | Tersedia |
| `PATCH` | `/{id}/status` | Menyalakan atau mematikan alat | `RadModality : Update` | `UpdateRadModalityStatusRequest` | `ApiResponse<RadModalityDetailResponse>` | Tersedia |
| `DELETE` | `/{id}` | Menandai alat terhapus tanpa menghapus fisik | `RadModality : Delete` | — | `ApiResponse<bool>` | Tersedia |

> **Alat yang masih dipakai tidak dapat dipensiunkan.** `PATCH /{id}/status` dan `DELETE /{id}`
> sama-sama ditolak `409` selama masih ada aturan keselamatan **berlaku** pada alat itu.
> Urutan yang benar: hentikan aturannya lebih dulu lewat
> `POST /rad-safety-rules/{id}/deactivate` — wewenang penanggung jawab klinis — baru alatnya
> dipensiunkan. Mencabut kebijakan klinis sebagai efek samping mematikan sebuah alat berarti
> mengubahnya tanpa sepengetahuan penanggung jawabnya.

### Health Services / Radiology Management / Master Data / Rad Safety Requirement

Base URL: `api/v1/health-services/radiology-management/master-data/rad-safety-requirements`
Contract version: `v1` — status **berjalan** sejak `BE-RAD-05`, 2026-09-11

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/filters/metadata` | Pilihan penyaring, termasuk kelompok butir yang benar-benar dipakai | `RadSafetyRequirement : Read` | — | `ApiResponse<RadSafetyRequirementFilterMetadataResponse>` | Tersedia |
| `GET` | `/summary` | Rekap butir, termasuk yang dipakai aturan berlaku dan yang belum dipakai | `RadSafetyRequirement : Read` | — | `ApiResponse<RadSafetyRequirementSummaryResponse>` | Tersedia |
| `GET` | `/` | Melihat daftar butir keselamatan | `RadSafetyRequirement : Read` | Query | `ApiResponse<PagedResult<RadSafetyRequirementResponse>>` | Tersedia |
| `GET` | `/options` | Pilihan ringan untuk form penyusunan aturan keselamatan | `RadSafetyRequirement : Read` | Query | `ApiResponse<List<RadSafetyRequirementOptionResponse>>` | Tersedia |
| `GET` | `/{id}` | Melihat rincian satu butir beserta jumlah aturan berlaku yang memakainya | `RadSafetyRequirement : Read` | — | `ApiResponse<RadSafetyRequirementDetailResponse>` | Tersedia |
| `POST` | `/` | Menambah butir keselamatan | `RadSafetyRequirement : Create` | `CreateRadSafetyRequirementRequest` | `ApiResponse<RadSafetyRequirementDetailResponse>` | Tersedia |
| `PUT` | `/{id}` | Mengubah butir keselamatan | `RadSafetyRequirement : Update` | `UpdateRadSafetyRequirementRequest` | `ApiResponse<RadSafetyRequirementDetailResponse>` | Tersedia |
| `PATCH` | `/{id}/status` | Menyalakan atau mematikan butir | `RadSafetyRequirement : Update` | `UpdateRadSafetyRequirementStatusRequest` | `ApiResponse<RadSafetyRequirementDetailResponse>` | Tersedia |
| `DELETE` | `/{id}` | Menandai butir terhapus tanpa menghapus fisik | `RadSafetyRequirement : Delete` | — | `ApiResponse<bool>` | Tersedia |

> **Menambah butir di sini tidak membuatnya berlaku bagi pasien mana pun.** Butir keselamatan
> adalah **kosakata**; yang mengikat adalah aturan keselamatan yang menyusunnya untuk sebuah
> alat dan disahkan penanggung jawab klinis.
>
> **Butir yang masih dipakai aturan berlaku tidak dapat hilang.** `PATCH /{id}/status` dan
> `DELETE /{id}` sama-sama ditolak `409`. Pertanyaan keselamatan yang kehilangan rumusannya
> lebih buruk daripada pertanyaan yang tidak pernah ada, karena ia tetap terlihat dijawab.

### Health Services / Radiology Management / Master Data / Rad Safety Rule

Base URL: `api/v1/health-services/radiology-management/master-data/rad-safety-rules`
Contract version: `v1` — status **berjalan** sejak `BE-RAD-03`, 2026-09-10

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/filters/metadata` | Pilihan penyaring, pengurutan, dan daftar aksi yang sah | `RadSafetyRule : Read` | — | `ApiResponse<RadSafetyRuleFilterMetadataResponse>` | Tersedia |
| `GET` | `/summary` | Rekap jumlah aturan per keadaan beserta alat yang belum tercakup | `RadSafetyRule : Read` | — | `ApiResponse<RadSafetyRuleSummaryResponse>` | Tersedia |
| `GET` | `/` | Melihat daftar aturan keselamatan | `RadSafetyRule : Read` | Query | `ApiResponse<PagedResult<RadSafetyRuleResponse>>` | Tersedia |
| `GET` | `/coverage` | **Memeriksa alat mana yang belum punya aturan aktif** | `RadSafetyRule : Read` | — | `ApiResponse<List<RadModalityCoverageResponse>>` | Tersedia |
| `GET` | `/{id}` | Melihat rincian satu aturan; dipakai form ubah | `RadSafetyRule : Read` | — | `ApiResponse<RadSafetyRuleResponse>` | Tersedia |
| `POST` | `/` | Menyusun draf aturan | `RadSafetyRule : Create` | `CreateRadSafetyRuleRequest` | `ApiResponse<RadSafetyRuleResponse>` | Tersedia |
| `PUT` | `/{id}` | Mengubah draf aturan | `RadSafetyRule : Update` | `UpdateRadSafetyRuleRequest` | `ApiResponse<RadSafetyRuleResponse>` | Tersedia |
| `POST` | `/{id}/submit` | Mengajukan aturan untuk disahkan | `RadSafetyRule : Submit` | — | `ApiResponse<RadSafetyRuleResponse>` | Tersedia |
| `POST` | `/{id}/approve` | Mengesahkan aturan; versi naik satu | `RadSafetyRule : Approve` | — | `ApiResponse<RadSafetyRuleResponse>` | Tersedia |
| `POST` | `/{id}/reject` | Menolak pengajuan aturan | `RadSafetyRule : Reject` | `RadSafetyRuleRejectRequest` | `ApiResponse<RadSafetyRuleResponse>` | Tersedia |
| `POST` | `/{id}/deactivate` | Menonaktifkan aturan yang berlaku | `RadSafetyRule : Deactivate` | — | `ApiResponse<RadSafetyRuleResponse>` | Tersedia |

> **Mengapa `GET /coverage` ada.** Gerbang keselamatan bersifat fail-closed. Tanpa layar yang
> memberi tahu alat mana yang belum punya aturan aktif, admin baru tahu ada yang kurang ketika
> pasien sudah berdiri di depan alat dan pemeriksaannya ditolak.
>
> Daftar ini hanya memuat alat yang **belum** tercakup, bukan seluruh alat beserta penandanya.
> **Daftar kosong berarti seluruh alat siap dipakai** — bentuk jawaban yang dituntut
> acceptance test `BE-RAD-15`.

### Tiga endpoint baseline master data yang sengaja tidak dibuat

Standar endpoint master data menyebut sembilan endpoint. Grup ini hanya memakai enam di
antaranya, dan selisihnya disengaja.

| Tidak dibuat | Alasan |
|---|---|
| `GET /options` | Aturan keselamatan bukan isi dropdown. Yang dipilih pada form lain adalah alat dan butir keselamatannya, bukan aturannya |
| `PATCH /{id}/status` | Keadaan aturan berpindah karena kejadian bernama — diajukan, disahkan, ditolak, dihentikan — bukan karena seseorang menyetel nilai. Menyediakan penyetelan status generik akan melewati seluruh pengesahan berjenjang `RAD-DEC-005` |
| `DELETE /{id}` | Aturan yang pernah berlaku tidak dihapus. Study lama yang lolos memakainya tetap harus dapat ditelusuri; yang tersedia adalah `POST /{id}/deactivate` |

---

## 3. Endpoint yang Akan Digantikan

| Endpoint lama | Penggantinya | Rencana |
|---|---|---|
| `GET /rad-studies/modalities` | `GET /master-data/rad-modalities` | **Tetap dipertahankan** sampai seluruh konsumen berpindah |
| `GET /rad-studies/safety-requirements` | `GET /master-data/rad-safety-requirements` | **Tetap dipertahankan** sampai seluruh konsumen berpindah |

Penghapusan keduanya menjadi **task tersendiri**, bukan bagian dari pekerjaan ini. Menghapusnya
sekarang akan merusak konsumen yang belum tentu diketahui semuanya.

---

## 4. Arti Kode Status bagi Pengguna

| Kode | Arti bagi pengguna | Contoh kejadian di modul ini |
|---|---|---|
| `200` | Permintaan berhasil dan datanya dikembalikan | Membuka daftar pesanan |
| `201` | Data baru berhasil dibuat | Draf bacaan tersimpan |
| `400` | Isian yang dikirim tidak lengkap atau formatnya salah | Menolak aturan tanpa mengisi alasan |
| `403` | Pengguna tidak punya hak akses untuk tindakan ini | Residen mengesahkan drafnya sendiri |
| `404` | Data yang dicari tidak ditemukan | Membuka bacaan yang tidak ada |
| `409` | Tindakan bertabrakan dengan keadaan data saat ini | Mulai foto padahal gerbang keselamatan belum lolos; dua orang mengesahkan draf yang sama bersamaan |
| `422` | Aturan bisnis menolak, walau bentuk isiannya benar | Menulis bacaan atas study yang citranya dinyatakan tidak layak |

---

## 5. Traceability

| Endpoint | Decision asal | Slice |
|---|---|---|
| `POST /rad-reports/by-study/{id}/draft` | `RJ-BIL-GATE-DEC-004`, `RAD-DEC-003` | `S9` |
| `POST /rad-reports/{id}/validate` | `RAD-DEC-003` | `S9` |
| `POST /rad-reports/{id}/amendments` | `RJ-BIL-GATE-DEC-004` | `S10` |
| `GET /rad-reports/by-encounter/{id}` | `RAD-DEC-006` | `S14` |
| `POST /master-data/rad-safety-rules/{id}/approve` | `RAD-DEC-005` | `S4` |
| `GET /master-data/rad-safety-rules/coverage` | `RJ-BIL-DEC-014` | `S4` |
| CRUD `master-data/rad-modalities` | `RAD-DEC-001` butir 14 | `S13` |
| `GET /rad-orders/worklist` | `RAD-DEC-012` | `S12` |
| `PUT /rad-orders/{id}/urgency` | `RAD-DEC-013` | `S12` |
