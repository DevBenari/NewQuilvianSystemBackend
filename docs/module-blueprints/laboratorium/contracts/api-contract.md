# API Contract — Modul Laboratorium

| Field | Value |
|---|---|
| Contract version | `LAB-API-v1` |
| Revision | `25` |
| `r25` approved_by / approved_at | Yoga Aji Pratama (`yogaaji452@gmail.com`) / **2026-09-18** |
| Isi amandemen `r25` | **`approved` — 2026-09-18.** Laporan Patologi Anatomi **per pesanan** (`S4c` sesudah dirancang ulang), menurunkan `LAB-DEC-085`..`LAB-DEC-088` dan `LAB-DEC-091`..`LAB-DEC-094` beserta `LAB-DA-001` rev 7. Enam endpoint laporan/konteks klinis dan empat data induk. **Menggantikan bagian 19.3** yang ditandai `superseded` — jalur `/lab-examinations/{id}/result/pathology` salah alamat karena laporan PA melekat pada **pesanan**, bukan pemeriksaan. **Aditif terhadap yang berjalan**: bagian 19.3 nol pernah dibangun, sehingga penggantiannya nol memutus pemakai. Bagian 19.2 Mikrobiologi dan 19.4 data induk Mikrobiologi **tidak tersentuh**. Disetujui bersama `LAB-VAL-v1` `r8` dan `LAB-PERM-v1` rev 7 pada hari yang sama. Lihat bagian 20 |
| `r24` approved_by / approved_at | Yoga Aji Pratama (`yogaaji452@gmail.com`) / **2026-09-18** |
| Isi amandemen `r24` | **`approved` — 2026-09-18.** Pengisian hasil Mikrobiologi dan Patologi Anatomi (`S4b`, `S4c`), menurunkan `LAB-DEC-027`, `LAB-DEC-080`, `LAB-DEC-081`, dan `LAB-DEC-084`. Empat jalur hasil dan delapan jalur data induk. **Aditif** — nol endpoint yang sudah ada berubah, nol ruas bergeser. Disetujui bersama `LAB-VAL-v1` `r7` dan `LAB-PERM-v1` rev 6 pada hari yang sama |
| `r19` approved_by / approved_at | Yoga Aji Pratama (`yogaaji452@gmail.com`) / 2026-09-17 |
| Isi amandemen `r19` | **`approved` — 2026-09-17.** Satu ruas `gender` pada `LabMonitoringItemResponse`, menutup `REC3-NEW-010`. **Mengoreksi catatan lama yang menyebutnya "kewenangan UI, tidak menyentuh kontrak backend"** — DTO itu ternyata **nol** membawa gender, sehingga ikonnya mustahil dibangun dari sisi layar saja. Aditif; nol endpoint, nol permission, nol migration. **Artifact hanya menyebut dua warna dan diam soal dua nilai enum lainnya beserta `null`;** kediaman itu tidak ditebak melainkan diputuskan pemilik modul — ikon netral abu-abu dengan **label yang tetap membedakan** "tidak diketahui" dari "tidak diinformasikan". Lihat bagian 14 |
| `r18` approved_by / approved_at | Yoga Aji Pratama (`yogaaji452@gmail.com`) / 2026-09-17 |
| Isi amandemen `r18` | **`approved` — 2026-09-17.** Dua ruas opsional pada `LabMonitoringQuery`, menurunkan `BR-50` butir 1: `identityNumber` (penyaring **NIK**, menutup `REC3-NEW-003`) dan `dateCategory` (**Kategori Periode**, menutup `REC3-NEW-002` **sebagian**). Aditif seluruhnya — nol endpoint, ruas respons, permission, dan migration. **Nilai `ExaminationDate` sengaja TIDAK dicantumkan** pada enumnya: `LabExamination` nol punya kolom waktu pemeriksaan, dan mencantumkannya berarti mendirikan pilihan yang tidak menuju ke mana-mana — pola yang sudah enam kali menimpa modul ini. **Dua keputusan pemilik diambil 2026-09-17:** bentuk penyaring memakai **dua ruas terpisah beserta pilihan di layar** (13.2.1 pilihan A), dan Kategori Periode berjalan **dua pilihan sekarang** tanpa menampilkan yang ketiga. Lihat bagian 13 |
| `r16` / `r17` approved_by / approved_at | Yoga Aji Pratama (`yogaaji452@gmail.com`) / 2026-09-17 |
| Isi amandemen `r17` | **`approved` — 2026-09-17.** Satu endpoint baca baru `GET /lab-specimens`: daftar penerimaan **lintas pesanan**, disaring rentang **waktu kedatangan sebenarnya**. Menutup celah yang menahan `FE-LAB-12` — nol endpoint mengembalikan daftar wadah lintas pesanan; yang ada hanya rekap tanpa baris dan daftar per satu pesanan. Aditif, nol migration, nol permission baru. **Satu peringatan dibawa serta:** rentangnya wajib disaring pada `PhysicallyReceivedAt ?? CreateDateTime`, karena bila keliru seluruh guna kolom itu hilang tanpa satu pun kesalahan yang terlihat. Lihat bagian 12 |
| Isi amandemen `r16` | **`approved` — 2026-09-17. Memuat DUA hal dari dua tempat berbeda.** Pertama, ruas `orderNumber` pada `LabOrderListResponse` dan `LabMonitoringItemResponse` — kolomnya berdiri lewat `BE-LAB-36` tetapi nol DTO mengembalikannya. Kedua, ruas `collectedAt` opsional pada `PlanLabSpecimenRequest`, menurunkan **`LAB-CONFLICT-006` pilihan A yang sudah diputuskan 2026-09-16** tetapi amandemennya tidak pernah ditulis; laporan `BE-LAB-22` §6 menyebut sasaran "`r10`" yang ditulis ketika kontrak masih `r7`. Membukanya membuat `VAL-59` dan `AC-66` dapat ditegakkan penuh. Aditif seluruhnya, nol migration, nol permission baru. Lihat bagian 11 |
| `r15` approved_by / approved_at | Yoga Aji Pratama (`yogaaji452@gmail.com`) / 2026-09-16 |
| Isi amandemen `r15` | **`approved` — 2026-09-16.** Satu ruas `orderedProcedures` pada `LabOrderDetailResponse`, berisi pemeriksaan yang benar-benar dipesan. Menutup celah yang menahan `FE-LAB-17`: `LabOrderedProcedure` berdiri sejak `BE-LAB-26` dan terisi sejak `BE-LAB-27`, tetapi **nol DTO dan nol endpoint mengembalikannya**, sedangkan `LabOrder.ProcedureId` hanyalah penunjuk **wakil**. Aditif, nol migration, nol permission baru. Lihat bagian 10 |
| `r14` approved_by / approved_at | Yoga Aji Pratama (`yogaaji452@gmail.com`) / 2026-09-16 |
| Isi amandemen `r14` | **`approved` — 2026-09-16. Koreksi atas `r13`, bukan kebutuhan baru.** Tiga ruas tampil ditambahkan pada `LabMonitoringItemResponse`: `confirmedAt`, `confirmedByName`, dan `examinerDoctorName`. `r13` menyebut `LabOrderListResponse`, padahal ketiga menu pemeriksaan membaca grup `Lab Monitoring` — dan `GET /lab-orders/by-discipline/{discipline}` yang menerima ruas `r13` nol dipakai frontend. Nol penunjuk dikirim: daftar pantau adalah layar baca. Aditif, nol migration, nol permission baru. Lihat bagian 9 |
| Status | `approved` — `r3`..`r6` dikunci sebelumnya; **amandemen `r7`, `r8`, dan `r9` disetujui pemilik modul 2026-09-14**; **amandemen `r10` disetujui pemilik modul 2026-09-15**; **amandemen `r11` (pencabutan satu ruas) disetujui pemilik modul 2026-09-15**; **amandemen `r12` disetujui pemilik modul 2026-09-15**; **amandemen `r13` disetujui pemilik modul 2026-09-16** |
| `r13` approved_by / approved_at | Yoga Aji Pratama (`yogaaji452@gmail.com`) / 2026-09-16 |
| Isi amandemen `r13` | **`approved` — 2026-09-16.** Lima ruas respons konfirmasi ditambahkan: `confirmedAt`, `confirmedByName`, dan `examinerDoctorName` pada `LabOrderListResponse`; `confirmedByUserId` dan `examinerDoctorId` pada `LabOrderDetailResponse`. **Seluruhnya aditif** — nol endpoint, ruas, nilai enum, permission, dan migration yang berubah. Menutup celah yang ditemukan `BE-LAB-31`: nilai konfirmasi sudah tersimpan sejak `BE-LAB-30` tetapi tidak punya jalan keluar, sehingga `FE-LAB-15` dan `FE-LAB-17` tertahan dan `AC-94`/`AC-95` hanya terpenuhi sebagian. Lihat bagian 8 |
| `r11` approved_by / approved_at | Yoga Aji Pratama (`yogaaji452@gmail.com`) / 2026-09-15 |
| Isi amandemen `r12` | **`approved` — 2026-09-15.** Satu endpoint baru `POST /lab-orders/{id}/confirm` menurunkan `LAB-DEC-061`, dan `PUT /lab-orders/{id}/cancel` diperketat menurunkan `LAB-DEC-063`: `cancelReason` menjadi **wajib** dan pembatalan hanya sah pada `Requested`/`Confirmed`. **Ini perubahan breaking pada endpoint yang sudah dipakai** — lihat bagian 7. Nol permission baru. Status pembayaran `LAB-DEC-062` sengaja tidak dikontrakkan di sini: endpointnya milik Billing dan tertahan `LAB-COORD-010` |
| Isi amandemen `r11` | **`approved` — 2026-09-15. Ruas `clinicalNote` pada `POST /lab-orders/by-examinations` dicabut sebelum sempat dibangun.** `BE-LAB-27` menemukan `LabOrder` **tidak memiliki kolom catatan**, sedangkan cakupan task itu nol migration — sehingga ruas yang diterima kontrak tidak punya tempat disimpan. Menerima lalu membuangnya diam-diam ditolak: pemanggil akan mengira catatannya tersimpan dan baru tahu tidak ketika seseorang mencarinya. **Ini selisih pada kontrak yang ditulis sesi perancangan `r10` sendiri**, bukan temuan pada pekerjaan orang lain. Pemilik modul memilih mencabut, bukan menambah kolom, atas dasar **tidak ada peminta**: `FE-LAB-14` — satu-satunya layar yang memanggil endpoint ini — tidak menyebut catatan klinis sama sekali dan tidak memuat kotak isian untuknya. **Nol dampak kode**: penelusuran `ClinicalNote` di seluruh modul Laboratorium menemukan nol kemunculan, sehingga pencabutan ini menghapus janji, bukan perilaku. Bila catatan klinis kelak benar dibutuhkan, ia masuk sebagai task tersendiri berisi satu kolom beserta migration-nya, dengan peminta yang jelas |
| `r10` approved_by / approved_at | Yoga Aji Pratama (`yogaaji452@gmail.com`) / 2026-09-15 |
| `r7` / `r8` / `r9` approved_by / approved_at | Yoga Aji Pratama (`yogaaji452@gmail.com`) / 2026-09-14 |
| Isi amandemen `r10` | **`approved` — 2026-09-15.** Satu endpoint baru pada grup `Lab Order`: `POST /lab-orders/by-examinations`, yang menerima daftar pemeriksaan lalu membentuk **satu pesanan per disiplin**. **Aditif** — `POST /lab-orders` yang sudah ada tidak berubah bentuk, ruas, maupun perilakunya. Menurunkan `LAB-DEC-055`, `LAB-DEC-056`, dan `LAB-DEC-057`. Sambungan kiosk **dibuka penuh** oleh `LAB-REQ-006` pada 2026-09-15; endpoint pembacaan sesi kiosk milik `registration-management` dan dikontrakkan di sisi modul itu, bukan di sini |
| Isi amandemen `r9` | **Ruas `Quantity` pada `POST /lab-examinations` dicabut sebelum sempat dibangun.** `BE-LAB-23` menemukan index unik `(SpecimenId, ProcedureId)` — dipasang atas dasar `BR-20` dan `AC-35` pada 2026-09-01 — membuat `Quantity` > 1 untuk jenis pemeriksaan yang sama pada wadah yang sama mustahil. Akibatnya `POST /lab-examinations` **tidak berubah sama sekali** dari `r6` ke `r9`. Ditutup `LAB-DEC-050` |
| Isi amandemen `r8` | Dua endpoint baca ditambahkan pada grup `Lab Specimen Type`: `GET /filters/metadata` dan `GET /summary`. **Aditif** — tidak satu pun endpoint, ruas, atau nilai enum yang berubah, berganti nama, atau hilang. Keduanya adalah permukaan baseline yang diwajibkan `rules/backend/master-data-endpoint-standard.md` dan sudah menjadi pola nyata pada grup master data Laboratorium lain; ketiadaannya pada `r7` adalah kelalaian penulisan kontrak, bukan keputusan. Ditemukan dan ditutup saat `BE-LAB-20` dikerjakan. Jumlah endpoint grup naik dari **7 menjadi 9** |
| Isi amandemen `r7` | Satu grup baru `Lab Specimen Type` berisi tujuh endpoint, ditambah perluasan aditif pada `POST /lab-specimens/by-order/{labOrderId}` (lima ruas jenis, volume, dan waktu penerimaan fisik) dan `POST /lab-examinations` (ruas `Quantity` yang memperbanyak baris). **Tidak satu pun endpoint, ruas, atau nilai enum `r3`..`r6` berubah, berganti nama, atau hilang.** Endpoint pengusulan instansi perujuk dan pembacaan metode pembayaran **sengaja tidak dicantumkan** karena milik modul lain dan masih menunggu `LAB-REQ-005`. Menurunkan `LAB-DEC-038`, `LAB-DEC-040`, `LAB-DEC-041`, `LAB-DEC-042` |
| Isi amandemen `r4` | Sepuluh endpoint baca ditambahkan: `GET /filters/metadata` dan `GET /summary` pada kelima grup Laboratorium yang sudah punya controller. Seluruhnya **aditif** — tidak satu pun endpoint, ruas, atau nilai enum `r3` yang berubah, berganti nama, atau hilang. Dikerjakan `BE-LAB-17` |
| Isi amandemen `r5` | `GET /lab-orders` memperoleh penyaring, pengurutan, dan pagination di sisi server lewat `LabOrderPagedQuery`. **Ini satu-satunya perubahan breaking**: bentuk responsnya berubah dari `ApiResponse<List<LabOrderListResponse>>` menjadi `ApiResponse<PagedResult<LabOrderListResponse>>`. Dampak konsumen dinilai — lihat catatan di bawah. Dikerjakan `BE-LAB-18` |
| Isi amandemen `r6` | Satu endpoint baca ditambahkan: `GET /lab-rejection-reasons/{id}`. **Aditif** — tidak satu pun endpoint, ruas, atau nilai enum yang berubah, berganti nama, atau hilang. Grup ini semula satu-satunya grup Laboratorium tanpa jalur detail, sehingga formulir ubah `FE-LAB-03` memuat barisnya dari halaman daftar yang sedang terbuka dan diam-diam gagal pada tautan langsung maupun muat ulang. Jumlah endpoint grup naik dari **7 menjadi 8**, dan penjaga `ControllerPengelolaan_MemakaiBaseRouteYangDikunciKontrak` disesuaikan bersamaan |

| Batas penguncian | **Terkunci penuh sejak 2026-09-02.** `LAB-OPEN-021` dijawab: penamaan memakai prefix `Lab`, sehingga tidak ada lagi bagian yang dikecualikan |
| Owner | Yoga Aji Pratama (`yogaaji452@gmail.com`) |
| `approved_by` / `approved_at` | Yoga Aji Pratama (`yogaaji452@gmail.com`) / 2026-09-02 |
| Input revision | Decisions rev 20; `LAB-DA-001` rev 4 |
| Input hash | `sha256:6504b18a327b9966526bd1df8f3cb878d7f6d6519dacc1f7df16b1066729ae82` atas `00-interview-decisions.md`, dihitung 2026-09-02 |
| Dampak kompatibilitas | **Breaking** pada endpoint sampel — lihat bagian 3 |
| Backend SHA | `c87d9c0` |

Seluruh endpoint memerlukan login (`[Authorize]`). Pembungkus respons memakai
`ApiResponse<T>.Ok(data, pesan)` dan `ApiResponse<T>.Fail(kode, pesan)`.

Endpoint yang belum ada di kode ditandai **`Rencana (belum tersedia)`**. Per 2026-09-08 tidak
ada satu pun endpoint yang menyandangnya — lihat koreksi di bawah.

> **Koreksi status, 2026-09-08 — bukan amandemen.** Enam belas baris masih tertulis
> `Rencana (belum tersedia)` padahal endpointnya sudah ada sejak `BE-LAB-04`, `BE-LAB-05`, dan
> `BE-LAB-06` selesai pada 2026-09-02 dan 2026-09-03. Selisih ini dicatat `FE-LAB-02` pada
> 2026-09-04 dan tidak pernah ditindaklanjuti, sehingga dokumen kontrak menyatakan sebagian
> modulnya belum dibangun padahal sudah.
>
> **Yang berubah hanya kolom status.** Tidak ada endpoint yang ditambah, dihapus, diubah
> route, verb, hak akses, maupun bentuk permintaan dan jawabannya. `LAB-API-v1` tetap
> revision `5` dan tetap terkunci; koreksi ini membuat dokumen menyebutkan keadaan yang
> sebenarnya, bukan mengubah kesepakatannya.
>
> **Bukti**, dibaca langsung dari controller pada tanggal koreksi:
>
> | Grup | Baris dikoreksi | Endpoint yang terbukti ada |
> |---|---:|---|
> | Lab Value Bound | 6 | `GET /`, `GET /{id}`, `POST /`, `PUT /{id}`, `PUT /{id}/deactivate`, `GET /{id}/history` |
> | Lab Critical Bound Approval | 5 | `GET /`, `POST /`, `POST /{requestId}/approve`, `POST /{requestId}/reject`, `POST /{requestId}/withdraw` |
> | Lab Rejection Reason | 5 | `GET /`, `POST /`, `PUT /{id}`, `PUT /{id}/activation`, `PUT /{id}/system-flags` |
>
> Ketiga grup itu **tidak** memperoleh endpoint baru lewat koreksi ini. Khususnya Lab
> Rejection Reason tetap tanpa `GET /{id}`: penambahannya adalah amandemen kontrak yang
> belum disetujui, dan uji `ControllerPengelolaan_MemakaiBaseRouteYangDikunciKontrak`
> menegakkannya dengan mengunci jumlah endpoint grup itu pada tujuh.

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
| `POST` | `/by-examinations` | Membuat pesanan dari **daftar pemeriksaan sekaligus**, terpecah otomatis menjadi satu pesanan per disiplin | `LabOrder : Create` | `CreateLabOrderByExaminationsRequest` | `ApiResponse<List<LabOrderDetailResponse>>` | `Rencana (belum tersedia)` — **`r10` `approved`** |

#### Perluasan `r10` — `POST /by-examinations`

**Permintaan — `CreateLabOrderByExaminationsRequest`:**

| Ruas | Tipe | Wajib | Catatan |
|---|---|:---:|---|
| `encounterId` | `guid` | ya | Kunjungan yang sudah ada, dari mana pun asalnya. **Laboratorium tidak membentuk kunjungan** (`AC-45`) |
| `inpEpisodeId` | `guid?` | tidak | Diteruskan apa adanya ke setiap pesanan, mengikuti `CreateLabOrderRequest` |
| `examinations` | `guid[]` | ya | Daftar `MstProcedure`. Sekurang-kurangnya satu (`VAL-64`), tidak boleh kembar (`VAL-65`) |
| `citoExaminations` | `guid[]` | tidak | Bagian dari `examinations` yang ditandai cito. Penanda cito melekat pada pemeriksaan sejak `LAB-DEC-026` |

**Respons.** Satu `LabOrderDetailResponse` **per disiplin**, terurut: Patologi Klinik, Patologi
Anatomi, Mikrobiologi, lalu kelompok tanpa disiplin. DTO-nya **dipakai ulang apa adanya**, tidak
dibuat varian baru.

**Perilaku yang dikunci:**

| Hal | Ketentuan |
|---|---|
| Pemecahan | Satu pesanan per nilai `MstProcedure.LabDiscipline`. Pemeriksaan yang belum digolongkan berkumpul menjadi **satu** pesanan ber-`discipline` `null` (`AC-85`, `AC-87`) |
| Transaksi | **Seluruhnya satu transaksi.** Satu pemeriksaan ditolak berarti nol pesanan terbentuk |
| `procedureId` pada tiap pesanan | Diisi pemeriksaan **pertama** kelompoknya sebagai penunjuk wakil, supaya pembaca lama tetap memperoleh nilai yang masuk akal |
| Endpoint lama | `POST /lab-orders` **tidak berubah sama sekali** — bentuk, ruas, maupun perilakunya (`AC-88`) |

> **Endpoint pembacaan sesi kiosk tidak ada di sini karena bukan milik Laboratorium.** Bentuknya
> ditetapkan pemilik `registration-management` di bawah wewenang `LAB-REQ-006`, bukan oleh blueprint ini. Kewenangan Registrasi
> membentuk serta menutup kunjungan tetap utuh (`AC-45`).
> Endpoint di atas menerima `encounterId` yang sudah jadi, sehingga pembukaan kedua penahan itu
> **tidak mengubah satu baris pun** pada kontraknya.

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
| `GET` | `/` | Daftar batas nilai, dapat disaring per jenis pemeriksaan | `LabValueBound : Read` | `LabValueBoundPagedQuery` | `ApiResponse<PagedResult<LabValueBoundListResponse>>` | Tersedia |
| `GET` | `/{id}` | Detail satu batas nilai beserta pilihannya | `LabValueBound : Read` | — | `ApiResponse<LabValueBoundDetailResponse>` | Tersedia |
| `POST` | `/` | Membuat batas nilai baru | `LabValueBound : Create` | `CreateLabValueBoundRequest` | `ApiResponse<LabValueBoundDetailResponse>` | Tersedia |
| `PUT` | `/{id}` | Mengubah satuan, batas normal, batas waktu cito, dan daftar pilihan | `LabValueBound : Update` | `UpdateLabValueBoundRequest` | `ApiResponse<LabValueBoundDetailResponse>` | Tersedia |
| `PUT` | `/{id}/deactivate` | Menonaktifkan batas nilai | `LabValueBound : Update` | — | `ApiResponse<LabValueBoundDetailResponse>` | Tersedia |
| `GET` | `/{id}/history` | Riwayat perubahan batas nilai | `LabValueBound : Read` | — | `ApiResponse<List<LabValueBoundHistoryResponse>>` | Tersedia |

### Health Services / Laboratory Management / Lab Critical Bound Approval

Base URL: `api/v1/health-services/laboratory-management/lab-value-bounds/{id}/critical-change-requests`
Contract version: `LAB-API-v1` — status `approved`, dikunci 2026-09-02

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/filters/metadata` | Pilihan status pengajuan, urutan, ukuran halaman, dan dua penanda keselamatan: larangan menyetujui pengajuan sendiri serta batas satu pengajuan belum diputuskan | `LabCriticalBound : Read` | — | `ApiResponse<LabCriticalBoundApprovalFilterMetadataResponse>` | **Tersedia** — `r4`, `BE-LAB-17` |
| `GET` | `/summary` | Rekap pengajuan untuk **satu** batas nilai, per status | `LabCriticalBound : Read` | — | `ApiResponse<LabCriticalBoundApprovalSummaryResponse>` | **Tersedia** — `r4`, `BE-LAB-17` |
| `GET` | `/` | Daftar pengajuan perubahan batas kritis | `LabCriticalBound : Read` | — | `ApiResponse<List<LabBoundChangeRequestResponse>>` | Tersedia |
| `POST` | `/` | Mengajukan perubahan batas kritis | `LabValueBound : Update` | `SubmitCriticalBoundChangeRequest` | `ApiResponse<LabBoundChangeRequestResponse>` | Tersedia |
| `POST` | `/{requestId}/approve` | Menyetujui pengajuan; batas baru mulai berlaku | `LabCriticalBound : Approve` | `DecideCriticalBoundChangeRequest` | `ApiResponse<LabBoundChangeRequestResponse>` | Tersedia |
| `POST` | `/{requestId}/reject` | Menolak pengajuan | `LabCriticalBound : Approve` | `DecideCriticalBoundChangeRequest` | `ApiResponse<LabBoundChangeRequestResponse>` | Tersedia |
| `POST` | `/{requestId}/withdraw` | Menarik pengajuan sendiri | `LabValueBound : Update` | — | `ApiResponse<LabBoundChangeRequestResponse>` | Tersedia |

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
| `GET` | `/` | Daftar alasan penolakan untuk pengelolaan | `LabRejectionReason : Read` | `LabRejectionReasonPagedQuery` | `ApiResponse<PagedResult<LabRejectionReasonResponse>>` | Tersedia |
| `GET` | `/{id}` | Detail satu alasan penolakan beserta kedua penanda sistemnya | `LabRejectionReason : Read` | — | `ApiResponse<LabRejectionReasonResponse>` | **Tersedia** — `r6`, 2026-09-08 |
| `POST` | `/` | Menambah alasan penolakan | `LabRejectionReason : Create` | `CreateLabRejectionReasonRequest` | `ApiResponse<LabRejectionReasonResponse>` | Tersedia |
| `PUT` | `/{id}` | Mengubah nama, keterangan, dan urutan | `LabRejectionReason : Update` | `UpdateLabRejectionReasonRequest` | `ApiResponse<LabRejectionReasonResponse>` | Tersedia |
| `PUT` | `/{id}/activation` | Mengaktifkan atau menonaktifkan | `LabRejectionReason : Update` | `SetLabRejectionReasonActivationRequest` | `ApiResponse<LabRejectionReasonResponse>` | Tersedia |
| `PUT` | `/{id}/system-flags` | Menyetel penanda kesalahan internal dan penanda wajib catatan | `LabRejectionReason : SystemFlag` | `SetLabRejectionReasonSystemFlagsRequest` | `ApiResponse<LabRejectionReasonResponse>` | Tersedia |

`GET /lab-specimens/rejection-reasons` yang sudah ada **tetap dipertahankan** sebagai jalur baca
bagi petugas yang sedang menolak sampel. Endpoint pengelolaan di atas adalah jalur terpisah.

### Health Services / Laboratory Management / Lab Patient Registration

Base URL: `api/v1/health-services/laboratory-management/lab-patient-registrations`
Contract version: `LAB-API-v1` — status `approved`, dikunci 2026-09-02

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/patient-search` | Mencari pasien terdaftar sebelum mendaftarkan yang baru | `LabPatientRegistration : Read` | `LabPatientSearchQuery` | `ApiResponse<PagedResult<LabPatientSearchResponse>>` | **Tersedia** — `BE-LAB-08` |
| `POST` | `/walk-in` | Mendaftarkan pasien datang langsung; memanggil Registrasi lalu mengembalikan penunjuk kunjungan | `LabPatientRegistration : Create` | `RegisterLabWalkInRequest` | `ApiResponse<LabRegistrationResultResponse>` | **Tersedia** — `BE-LAB-08` |
| `POST` | `/external-referral` | Mendaftarkan pasien rujukan luar beserta instansi dan dokter perujuknya | `LabPatientRegistration : Create` | `RegisterLabExternalReferralRequest` | `ApiResponse<LabRegistrationResultResponse>` | **Tersedia** — `BE-LAB-08` |

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
| Grup Lab Specimen Type | `LAB-DEC-040` | AC-58, AC-59, AC-60 |
| `POST /lab-specimens/by-order/{labOrderId}` yang berubah — `r7` | `LAB-DEC-040`, `LAB-DEC-041`, `LAB-DEC-042` | AC-58, AC-59, AC-62, AC-63, AC-64, AC-66 |
| ~~`POST /lab-examinations` yang berubah — `r7`~~ | ~~`LAB-DEC-038`~~ | **Dicabut `r9` 2026-09-14** bersama `AC-52`..`AC-54`, oleh `LAB-DEC-050` |

---

## 6. Amandemen `r7` — 2026-09-14, Penerimaan Sampling/Specimen

**Status `approved`**, disetujui Yoga Aji Pratama selaku pemilik modul pada 2026-09-14. Label
`Rencana (belum tersedia)` pada tabel di bawah berarti **endpointnya belum ada di kode**, bukan
kontraknya belum disetujui.

**Aditif, dengan satu perluasan yang kompatibel ke belakang.** Tidak satu pun endpoint, ruas,
atau nilai enum `r3`..`r6` yang berubah, berganti nama, atau hilang.

> **Batas amandemen ini.** Endpoint untuk pengusulan instansi perujuk dan pembacaan metode
> pembayaran **tidak dicantumkan**, karena keduanya milik modul lain dan masih menunggu
> `LAB-REQ-005`. Menuliskan bentuknya sekarang akan dibaca implementer sebagai kesepakatan yang
> belum pernah terjadi.

### Health Services / Laboratory Management / Lab Specimen Type

Base URL: `api/v1/health-services/laboratory-management/lab-specimen-types`
Contract version: `LAB-API-v1` — amandemen `r7` dan `r8`, status `approved` 2026-09-14

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/filters/metadata` | Pilihan urutan, ukuran halaman, dan penanda tidak-dapat-dihapus serta penanda `Lainnya` yang tidak dapat disetel | `LabSpecimenType : Read` | — | `ApiResponse<LabSpecimenTypeFilterMetadataResponse>` | **Tersedia** — `r8`, `BE-LAB-20` |
| `GET` | `/summary` | Rekap total, aktif, nonaktif, dan jumlah jalan keluar `Lainnya` yang aktif | `LabSpecimenType : Read` | — | `ApiResponse<LabSpecimenTypeSummaryResponse>` | **Tersedia** — `r8`, `BE-LAB-20` |
| `GET` | `/options` | Daftar pilihan jenis specimen untuk layar penerimaan, hanya yang aktif | `LabSpecimenType : Read` | `LabSpecimenTypeOptionQuery` | `ApiResponse<PagedResult<LabSpecimenTypeOptionResponse>>` | **Tersedia** — `BE-LAB-20` |
| `GET` | `/` | Daftar jenis specimen untuk pengelolaan kepala instalasi | `LabSpecimenType : Read` | `LabSpecimenTypePagedQuery` | `ApiResponse<PagedResult<LabSpecimenTypeResponse>>` | **Tersedia** — `BE-LAB-20` |
| `GET` | `/{id:guid}` | Detail satu jenis specimen | `LabSpecimenType : Read` | — | `ApiResponse<LabSpecimenTypeResponse>` | **Tersedia** — `BE-LAB-20` |
| `GET` | `/other-usage` | **Daftar pantau** pemakaian `Lainnya`: keterangan yang pernah dipakai beserta jumlah dan pemakaian terakhirnya | `LabSpecimenType : Read` | `startDate`, `endDate` | `ApiResponse<PagedResult<LabSpecimenOtherUsageResponse>>` | **Tersedia** — `BE-LAB-25`, 2026-09-15 |
| `POST` | `/` | Menambah jenis specimen | `LabSpecimenType : Create` | `CreateLabSpecimenTypeRequest` | `ApiResponse<LabSpecimenTypeResponse>` | **Tersedia** — `BE-LAB-20` |
| `PUT` | `/{id:guid}` | Mengubah nama, keterangan, dan urutan | `LabSpecimenType : Update` | `UpdateLabSpecimenTypeRequest` | `ApiResponse<LabSpecimenTypeResponse>` | **Tersedia** — `BE-LAB-20` |
| `PUT` | `/{id:guid}/activation` | Mengaktifkan atau menonaktifkan | `LabSpecimenType : Update` | `SetLabSpecimenTypeActivationRequest` | `ApiResponse<LabSpecimenTypeResponse>` | **Tersedia** — `BE-LAB-20` |

**`GET /other-usage` adalah jalur yang menutup `AC-60`.** Ia tidak membaca tabel ringkasan;
rekapnya diturunkan dengan mengelompokkan wadah yang jenisnya ber-`IsOtherBucket` menurut
keterangannya. Dari layar ini kepala instalasi melihat bahwa "cairan kista", "Cairan Kista",
dan "c. kista" sebenarnya satu hal, lalu menaikkannya menjadi nilai tetap lewat `POST /`.

**Tidak ada `DELETE`.** Jenis yang sudah pernah dipakai wadah dinonaktifkan, bukan dihapus —
sama seperti pola `MstLabRejectionReason` dan `VAL-38`.

### Health Services / Laboratory Management / Lab Specimen — perluasan `r7`

Base URL: `api/v1/health-services/laboratory-management/lab-specimens`

| Method | Path | Yang berubah pada `r7` | Sifat |
|---|---|---|---|
| `POST` | `/by-order/{labOrderId}` | `PlanLabSpecimenRequest` bertambah `SpecimenTypeId`, `SpecimenTypeOtherNote`, `VolumeAmount`, `VolumeUnitId`, dan `PhysicallyReceivedAt` | **Aditif.** Wajib untuk wadah baru; permintaan tanpa ruas ini ditolak `422` |
| `GET` | `/{id}` dan seluruh jalur baca | `LabSpecimenResponse` bertambah kelima ruas di atas beserta nama jenis dan simbol satuannya | **Aditif.** Konsumen lama mengabaikan ruas yang tidak dikenalnya |

**`ReceivedAt` tidak menjadi ruas permintaan pada endpoint mana pun.** Bila dikirim, diabaikan
tanpa pesan kesalahan (`AC-65`). Yang diisi petugas adalah `PhysicallyReceivedAt`.

### Health Services / Laboratory Management / Lab Examination — perluasan `r7`

Base URL: `api/v1/health-services/laboratory-management/lab-examinations`

| Method | Path | Yang berubah pada `r7` | Keadaan pada `r9` |
|---|---|---|---|
| `POST` | `/` | ~~`CreateLabExaminationRequest` bertambah `Quantity` (`int`, bawaan `1`)~~ | **Dicabut `r9`, 2026-09-14.** Ruas `Quantity` **tidak jadi dibuat** |

> **Kenapa dicabut sebelum sempat dibangun.** `BE-LAB-23` menemukan `LabExamination` memiliki
> index unik di tingkat database atas `(SpecimenId, ProcedureId)`, dipasang atas dasar `BR-20`
> dan `AC-35` pada 2026-09-01. `Quantity` bernilai lebih dari satu untuk jenis pemeriksaan yang
> sama pada wadah yang sama karena itu **mustahil** — baris kedua ditolak service, dan bila
> lolos, ditolak database.
>
> Petugas yang memerlukan dua pemeriksaan memilih **dua butir katalog yang berbeda**; pada
> katalog yang tergolong benar, Glukosa Puasa dan Glukosa 2 Jam PP memang dua `MstProcedure`
> terpisah. Ditutup `LAB-DEC-050`.
>
> **`POST /lab-examinations` karena itu tidak berubah sama sekali** dari `r6` ke `r9`.

### Hak akses baru

| Permission | Dipakai |
|---|---|
| `[AccessPermission("LabSpecimenType", "Read")]` | Keempat endpoint baca |
| `[AccessPermission("LabSpecimenType", "Create")]` | `POST /` |
| `[AccessPermission("LabSpecimenType", "Update")]` | `PUT /{id}` dan `PUT /{id}/activation` |

Mengikuti `RJ-BIL-GATE-DEC-003`: hak membaca daftar jenis specimen tidak memberi hak
mengelolanya, dan jabatan organisasi tidak memberi kewenangan apa pun dengan sendirinya.

### Dampak kompatibilitas `r7`

| Konsumen | Dampak |
|---|---|
| `FE-LAB-06` formulir wadah | **Perlu penyesuaian** — tiga ruas baru wajib diisi |
| `FE-LAB-06` daftar dan detail wadah | Tidak ada. Ruas baru bersifat tambahan |
| Konsumen `POST /lab-examinations` yang sudah ada | **Tidak ada.** `Quantity` bernilai bawaan `1` |
| Penjaga base route yang dikunci kontrak | Perlu satu baris tambahan untuk grup `lab-specimen-types` |

---

## 7. Amandemen `r12` — Konfirmasi pesanan dan pembatalan beralasan, 2026-09-15

Menurunkan `LAB-DEC-061` dan `LAB-DEC-063`. Disetujui pemilik modul 2026-09-15.

### 7.1 Satu endpoint baru — `POST /lab-orders/{id}/confirm`

| Butir | Isi |
|---|---|
| Verb dan path | `POST /api/v1/health-services/laboratory-management/lab-orders/{id}/confirm` |
| Hak akses | `LabOrder : Update` — **tidak** membuat resource permission baru |
| Permintaan | `ConfirmLabOrderRequest` |
| Respons | `ApiResponse<LabOrderDetailResponse>`, status `200` |

**Permintaan.**

| Ruas | Tipe | Wajib | Catatan |
|---|---|:---:|---|
| `examinerDoctorId` | `guid` | ya | Dokter pemeriksa. Wajib ada dan aktif (`VAL-72`, `VAL-73`) |

**Yang sengaja tidak ada pada permintaan ini.** Nol ruas konfirmator dan nol ruas waktu
konfirmasi. Keduanya diturunkan server dari pengguna yang login dan jam server — ruas yang dapat
dikirim pemanggil adalah ruas yang dapat dipalsukan pemanggil, dan nama konfirmator adalah
pertanyaan audit, bukan pertanyaan tampilan.

**Akibat pada status.** `Requested` → `Confirmed` (`LAB-STATE-v1` `r3`). Konfirmasi hanya sah
sekali (`VAL-70`, `VAL-71`).

### 7.2 Satu endpoint yang sudah ada dan ruasnya diperketat — `PUT /lab-orders/{id}/cancel`

| Butir | `r11` | `r12` |
|---|---|---|
| Badan permintaan | Tercatat `—` pada kontrak, padahal source menerima `CancelLabSpecimenRequest?` yang **opsional** | `CancelLabOrderRequest` dengan `cancelReason` **wajib** |
| Status yang sah | Selain `Cancelled` dan `Completed` | Hanya `Requested` dan `Confirmed` (`VAL-75`) |

> **Koreksi kontrak, bukan amandemen.** Baris `r11` menulis badan permintaan endpoint ini sebagai
> `—`. Source menerima badan opsional bertipe `CancelLabSpecimenRequest` sejak jalur pembatalan
> dibangun, dan alasannya sudah tersimpan sebagai `ReasonNote` pada `LabTransitionHistory`.
> Kontraknya yang tertinggal, bukan sourcenya.

**Ini perubahan breaking, dan ditulis terang.** Pemanggil yang hari ini membatalkan pesanan tanpa
mengirim alasan akan mulai ditolak `422` (`VAL-74`), dan pembatalan dari `Accepted`, `InProcess`,
atau `OnHold` akan mulai ditolak `409` (`VAL-75`).

| Yang wajib diperiksa sebelum ditegakkan | Kenapa |
|---|---|
| Berapa pesanan berstatus `Accepted`, `InProcess`, `OnHold` hari ini | Mereka kehilangan jalur pembatalannya |
| Siapa saja pemanggil `PUT /{id}/cancel` hari ini | Frontend yang belum punya kotak alasan akan gagal menyimpan |

**Alasan pembatalan tetap disimpan di tempatnya yang sekarang** — `ReasonNote` pada jejak audit.
Nol kolom baru pada `LabOrder`, dan nol migration untuk bagian ini.

### 7.3 Yang **tidak** berubah

`POST /lab-orders`, `POST /lab-orders/by-examinations`, seluruh endpoint wadah, pemeriksaan, dan
data induk tidak berubah bentuk, ruas, maupun perilakunya. Nol permission baru pada seluruh
amandemen `r12`.

### 7.4 Status pembayaran — **tidak dikontrakkan di sini**

`LAB-DEC-062` menetapkan layar Laboratorium mengunci tombol Proses Pemeriksaan bagi pasien
Mandiri/tunai sampai `Lunas`, dengan nilainya **dibaca dari Billing**. Endpoint bacanya milik
Billing dan belum ada; ia tertahan `LAB-COORD-010` dan **sengaja tidak** dicantumkan di sini,
mengikuti cara `r10` memperlakukan endpoint sesi kiosk milik `registration-management`.

---

## 8. Amandemen `r13` — Ruas respons konfirmasi, 2026-09-16

Menutup celah yang ditemukan `BE-LAB-31` saat endpoint konfirmasi selesai dibangun: `r12` §7.1
mendefinisikan **badan permintaan** tetapi tidak menambah satu pun ruas respons, sehingga nilai
yang sudah tersimpan tidak punya jalan keluar. Disetujui pemilik modul 2026-09-16.

### 8.1 Masalahnya, ditulis terang

`BE-LAB-30` mendirikan tiga kolom dan `BE-LAB-31` mengisinya. Ketiganya **tersimpan dengan benar
dan terbukti** — tetapi tidak dapat dibaca siapa pun lewat API.

Akibatnya berantai pada dua layar sekaligus:

| Layar | Yang diwajibkan | Yang menghalanginya |
|---|---|---|
| `FE-LAB-15` | Kolom Konfirmasi memuat **nama konfirmator beserta tanggal dan waktu** | Ketiga nilai tidak dikembalikan endpoint mana pun |
| `FE-LAB-17` | Tanda tangan **konfirmator** dan **dokter pemeriksa** pada ringkasan cetak | Sama |

`AC-94` dan `AC-95` karena itu hanya terpenuhi sebagian, dan penyebabnya bukan pekerjaan yang
kurang melainkan bentuk kontraknya.

### 8.2 Lima ruas ditambahkan

| Ruas | Tipe | Pada | Isi |
|---|---|---|---|
| `confirmedAt` | `datetime?` | `LabOrderListResponse` | Waktu konfirmasi. Kosong selama pesanan belum dikonfirmasi |
| `confirmedByName` | `string?` | `LabOrderListResponse` | Nama konfirmator, **siap tampil** |
| `examinerDoctorName` | `string?` | `LabOrderListResponse` | Nama dokter pemeriksa, **siap tampil** |
| `confirmedByUserId` | `guid?` | `LabOrderDetailResponse` | Penunjuk konfirmator |
| `examinerDoctorId` | `guid?` | `LabOrderDetailResponse` | Penunjuk dokter pemeriksa |

`LabOrderDetailResponse` mewarisi `LabOrderListResponse`, sehingga detail memperoleh kelima-limanya.

### 8.3 Kenapa nama di daftar dan penunjuk di detail

**Nama ada di daftar karena layar tidak boleh menampilkan penunjuk.** Aturan `no-uuid-display`
sudah dipakai `requestedByName`, dan kolom Konfirmasi menampilkan nama orang — bukan UUID. Daftar
yang hanya membawa penunjuk memaksa layar memanggil endpoint kedua per baris hanya untuk
menerjemahkannya.

**Penunjuk ada di detail karena aksi membutuhkannya.** Layar yang kelak mengubah dokter pemeriksa
perlu nilai yang dapat dikirim balik, dan nama bukan nilai yang dapat dikirim balik.

Pembagian ini **sama persis** dengan `RequestedByUserId` dan `RequestedByName` yang sudah berlaku
sejak `LAB-API-v1` `r3`; nol pola baru diperkenalkan.

### 8.4 Aman bagi pembaca lama

Kelimanya **penambahan**. Tidak satu pun endpoint, ruas, nilai enum, atau bentuk pembungkus yang
berubah, berganti nama, atau hilang.

| Yang dinilai | Hasil |
|---|---|
| Pemanggil yang mengabaikan ruas baru | Tidak terpengaruh — penilaian yang sama sudah dipakai `r3` untuk tiga ruas kesegeraan |
| Pesanan yang belum pernah dikonfirmasi | Kelima ruas `null`, dan itu keadaan sah |
| Permission | **Nol** resource maupun action baru |
| Migration | **Nol.** Ketiga kolomnya sudah berdiri sejak `BE-LAB-30` |

### 8.5 Sumber nilainya

| Ruas | Dari mana |
|---|---|
| `confirmedAt`, `confirmedByUserId`, `examinerDoctorId` | Kolom `LabOrder` yang didirikan `BE-LAB-30`, dibaca apa adanya |
| `confirmedByName` | `ResolveUserNameAsync` — **jalur yang sama** dengan `RequestedByName`, supaya satu orang tidak terbaca dengan dua nama berbeda antar layar |
| `examinerDoctorName` | `MstDoctor.FullName` lewat penunjuk `ExaminerDoctorId` |

### 8.6 Yang **tidak** berubah

`POST /lab-orders/{id}/confirm` dan `PUT /lab-orders/{id}/cancel` tidak berubah sama sekali —
badan permintaan, kode status, maupun aturan validasinya. Amandemen ini hanya menambah ruas pada
respons yang sudah ada.

**`LabOrderSummaryResponse` sengaja tidak disentuh.** Rekap pesanan belum mengenal `Confirmed` —
tercatat pada [`BE-LAB-31.md`](../task/report/backend/BE-LAB-31.md) bagian 7.1 — tetapi
memperbaikinya berarti menambah ember status, dan itu keputusan tersendiri yang tidak dibutuhkan
`FE-LAB-15` maupun `FE-LAB-17`.

---

## 9. Amandemen `r14` — Ruas konfirmasi pada daftar pantau, 2026-09-16

**Koreksi atas `r13`, bukan kebutuhan baru.** Disetujui pemilik modul 2026-09-16.

### 9.1 Apa yang salah pada `r13`, ditulis terang

`r13` bagian 8.1 menyatakan tujuannya sendiri: membuat nilai konfirmasi dapat dibaca layar yang
membutuhkannya — `FE-LAB-15`, kolom Konfirmasi pada **ketiga menu pemeriksaan**. Tujuannya benar.
**DTO yang disebutnya tidak.**

| Yang disebut `r13` | Yang sebenarnya dibaca ketiga menu itu |
|---|---|
| `LabOrderListResponse` — dipakai `GET /lab-orders` dan `GET /lab-orders/by-discipline/{discipline}` | `LabMonitoringItemResponse` — dipakai grup `Lab Monitoring` |

Ketiga menu pemeriksaan adalah `lab-monitoring/clinical-pathology`,
`lab-monitoring/anatomic-pathology`, dan `lab-monitoring/microbiology`, dan seluruhnya membaca
grup `Lab Monitoring`. Endpoint `GET /lab-orders/by-discipline/{discipline}` yang memang menerima
kelima ruas `r13` **nol dipakai frontend**.

**Yang tidak terbuang dari `r13`.** Detail pesanan dan setiap jawaban aksi yang melewati
`GetDetailAsync` — termasuk `POST /lab-orders/{id}/confirm` — kini membawa kelima ruasnya. Pop-up
konfirmasi `FE-LAB-15` memakai itu untuk menampilkan hasilnya seketika tanpa memuat ulang daftar.
Yang belum tertutup hanyalah **kolomnya**.

### 9.2 Tiga ruas ditambahkan

| Ruas | Tipe | Pada | Isi |
|---|---|---|---|
| `confirmedAt` | `datetime?` | `LabMonitoringItemResponse` | Waktu konfirmasi. Kosong selama pesanan belum dikonfirmasi |
| `confirmedByName` | `string?` | `LabMonitoringItemResponse` | Nama konfirmator, **siap tampil** |
| `examinerDoctorName` | `string?` | `LabMonitoringItemResponse` | Nama dokter pemeriksa, **siap tampil** |

### 9.3 Kenapa hanya tiga, bukan lima

**Nol penunjuk dikirim, dan itu disengaja.** Daftar pantau adalah layar **baca**: ia menampilkan
antrean, tidak melakukan aksi apa pun terhadap dokter pemeriksa maupun konfirmator. Penunjuk hanya
dibutuhkan aksi, dan aksi pada modul ini berjalan lewat detail pesanan — yang sudah membawa
`confirmedByUserId` dan `examinerDoctorId` sejak `r13`.

Mengirim penunjuk yang tidak dipakai berarti mengirim nilai yang tidak boleh ditampilkan
(`no-uuid-display`) ke layar yang tidak membutuhkannya.

### 9.4 Aman bagi pembaca lama

| Yang dinilai | Hasil |
|---|---|
| Bentuk respons | **Aditif.** Nol endpoint, ruas, nilai enum, atau pembungkus yang berubah, berganti nama, atau hilang |
| Pesanan yang belum dikonfirmasi | Ketiganya `null`, dan itu keadaan sah |
| Permission | **Nol** resource maupun action baru. Grup `Lab Monitoring` tetap `LabMonitoring : Read` |
| Migration | **Nol.** Ketiga kolomnya sudah berdiri sejak `BE-LAB-30` |

### 9.5 Sumber nilainya

Sama persis dengan `r13` bagian 8.5: `ConfirmedAt` dibaca apa adanya dari `LabOrder`;
`confirmedByName` lewat jalur yang sama dengan `RequestedByName`; `examinerDoctorName` dari
`MstDoctor.FullName`. Nol jalur terjemahan baru diperkenalkan.

### 9.6 Pelajaran yang dicatat, supaya tidak terulang

`r13` disusun dengan membaca **apa yang dibutuhkan layar**, tetapi tanpa memeriksa **endpoint mana
yang layar itu benar-benar panggil**. Keduanya pertanyaan yang berbeda, dan hanya yang kedua dapat
dijawab dari source. Amandemen berikutnya yang menambah ruas respons wajib menyebut **endpoint dan
DTO yang diverifikasi dari source konsumennya**, bukan DTO yang paling masuk akal namanya.

---

## 10. Amandemen `r15` — Daftar pemeriksaan terpesan pada detail pesanan, 2026-09-16

> **Status: `approved` — disetujui pemilik modul 2026-09-16, pada hari yang sama ia diusulkan.**
> Ditulis ketika `FE-LAB-17` diputuskan **ditunda sampai amandemen ini jalan**, bukan diturunkan
> menjadi versi sebagian. `BE-LAB-35` boleh dimulai.

### 10.1 Celah yang ditemukan, dan bagaimana ia lolos selama ini

`BR-47` menetapkan satu tindakan petugas menghasilkan **satu pesanan per disiplin**, dan
pemeriksaan yang sedisiplin **berkumpul pada pesanan yang sama**. `BE-LAB-26` mendirikan
`LabOrderedProcedure` untuk menyimpan daftarnya, dan `BE-LAB-27` mengisinya.

Yang tidak pernah dikerjakan: **mengembalikannya**.

| Yang dinilai | Keadaan hari ini |
|---|---|
| DTO respons yang memuat daftar pemeriksaan terpesan | **Nol.** Pencarian `ProcedureNameSnapshot` pada seluruh area `LaboratoryManagement` menghasilkan nol kemunculan |
| Endpoint yang mengembalikannya | **Nol** |
| `LabOrder.ProcedureId` | **Penunjuk wakil, bukan satu-satunya.** Dinyatakan oleh komentar kodenya sendiri pada `LabOrderService.cs` — *"Penunjuk wakil, bukan satu-satunya pemeriksaan pesanan ini"* |
| `LabMonitoringItemResponse.ExaminationCount` | Menghitung `LabExamination`, yaitu yang **sedang dikerjakan dari wadah** — bukan yang dipesan. Pesanan yang belum berwadah bernilai `0` |

Akibatnya tunggal dan berkonsekuensi: **tidak ada satu pun konsumen yang dapat mengetahui isi
sebuah pesanan.** Untuk pesanan Hemoglobin + Kalium yang sengaja digabung `BR-47`, layar hanya
mengetahui satu nama.

**Kenapa celah ini tidak ketahuan lebih awal.** Ia tidak memutus apa pun. Ketiga menu pemeriksaan
adalah layar **antrean** — mereka memang tidak menampilkan isi pesanan, dan `VAL-68` serta `VAL-69`
membaca `LabOrderedProcedure` **di dalam backend**, sehingga aturannya tetap tegak tanpa ruas
respons. Yang pertama membutuhkannya adalah konsumen yang mencetak.

### 10.2 Siapa yang membutuhkannya, dan endpointnya diverifikasi dari source

`FE-LAB-17` — ringkasan cetak pesanan. Pemilik modul menetapkan 2026-09-16: **tombol berdiri per
baris pada ketiga menu pemeriksaan, dan satu klik mencetak satu pesanan.**

Mengikuti pelajaran `r14` bagian 9.6, jalur konsumennya ditelusuri dari source lebih dulu, bukan
ditebak dari nama DTO:

| Yang dibutuhkan dokumen cetak | Sudah tersedia? | Dari mana |
|---|---|---|
| Nama pasien dan No. Rekam Medis | **Ya** | `LabMonitoringItemResponse` — baris yang tombolnya ditekan |
| No. Kunjungan, disiplin, status, kesegeraan, waktu diminta | **Ya** | Baris yang sama |
| Tanda tangan konfirmator dan dokter pemeriksa | **Ya**, sejak `r13` dan `r14` | `confirmedByName`, `examinerDoctorName` |
| Tanda tangan pembuat order | **Ya**, sejak `r3` | `LabOrderDetailResponse.requestedByName` |
| **Daftar pemeriksaan yang dipesan** | **Tidak** | — inilah satu-satunya yang kurang |

Jalur detailnya `GET /v1/health-services/laboratory-management/lab-orders/{id}`, mengembalikan
`LabOrderDetailResponse` dengan hak akses `LabOrder : Read`. Jalur itu **sudah dipanggil
frontend** lewat `getLabOrderDetail` pada `lab-order.service.js`, sehingga amandemen ini tidak
melahirkan jalur baru — ia mengisi jalur yang sudah dipakai.

### 10.3 Satu ruas ditambahkan

| Ruas | Tipe | Pada | Isi |
|---|---|---|---|
| `orderedProcedures` | `LabOrderedProcedureResponse[]` | `LabOrderDetailResponse` | Pemeriksaan yang benar-benar dipesan, urut sesuai penyimpanannya |

Bentuk `LabOrderedProcedureResponse`:

| Ruas | Tipe | Isi |
|---|---|---|
| `procedureCode` | `string` | Kode pemeriksaan **pada saat dipesan** |
| `procedureName` | `string` | Nama pemeriksaan **pada saat dipesan**, siap tampil |
| `urgency` | `string` | `Routine` atau `Cito`, mengikuti nama enum `LabExaminationUrgency` |
| `orderedStatus` | `string` | `Ordered`, `Fulfilled`, atau `Cancelled`, mengikuti `LabOrderedProcedureStatus` |

### 10.4 Kenapa nol penunjuk dikirim

Sama dengan alasan `r14` bagian 9.3, dan diterapkan lagi di sini dengan sengaja: dokumen cetak
adalah keluaran **baca**. Ia tidak melakukan aksi apa pun terhadap baris pemeriksaan, sehingga
`procedureId` tidak dibutuhkan — dan mengirimnya berarti mengirim nilai yang tidak boleh
ditampilkan (`no-uuid-display`) ke konsumen yang tidak membutuhkannya. Bila kelak ada layar yang
benar-benar **mengubah** baris terpesan, penunjuknya ditambahkan oleh amandemen milik layar itu.

### 10.5 Kenapa snapshot, bukan katalog hari ini

Keempat ruas dibaca dari kolom snapshot `LabOrderedProcedure`, bukan dari `MstProcedure` yang
berlaku saat dokumen dicetak. Dokumen resmi harus menyebut apa yang **dipesan waktu itu**; nama
katalog yang kemudian diganti tidak boleh mengubah isi dokumen yang sudah pernah dicetak.

### 10.6 Kenapa hanya pada detail, bukan pada daftar pantau

Ketiga menu pemeriksaan menampilkan antrean, dan menambahkan daftar bersarang pada setiap baris
akan memperbesar respons layar yang paling sering dimuat demi data yang hanya dibaca ketika
tombol cetak ditekan. Cetak per baris memanggil detail **satu kali**, saat dibutuhkan.

### 10.7 Pesanan lama tidak punya baris, dan itu keadaan sah

Pesanan yang dibuat lewat `POST /lab-orders` — jalur lama berpemeriksaan tunggal — **nol
memiliki** baris `LabOrderedProcedure`. Keadaan itu sudah diakui `LabSpecimenService` sejak
`BE-LAB-28`, yang menegakkan `VAL-69` hanya bila pesanannya memiliki baris terpesan.

`orderedProcedures` karena itu mengembalikan **array kosong**, bukan galat. Aturan bagi
konsumennya ditulis di sini supaya tidak ditafsirkan sendiri-sendiri:

> Bila `orderedProcedures` **kosong**, pesanan itu berpemeriksaan tunggal dan `procedureName`
> pada pesanan **adalah** isi lengkapnya — cetak itu. Bila **terisi**, `procedureName` hanyalah
> wakil dan **tidak boleh** dicetak sebagai isi pesanan; yang dicetak adalah daftarnya.

### 10.8 Aman bagi pembaca lama

| Yang dinilai | Hasil |
|---|---|
| Bentuk respons | **Aditif.** Nol endpoint, ruas, nilai enum, atau pembungkus yang berubah, berganti nama, atau hilang |
| Pembaca lama `GET /lab-orders/{id}` | Menerima satu ruas tambahan yang boleh diabaikan |
| Permission | **Nol** resource maupun action baru. Tetap `LabOrder : Read` |
| Migration | **Nol.** Tabel beserta keempat kolom snapshotnya sudah berdiri sejak `BE-LAB-26` |
| Pesanan lama | Array kosong — lihat 10.7 |

### 10.9 Pelajaran yang dicatat

Celah ini lahir dari pola yang sudah tiga kali muncul pada modul ini — `LAB-CONFLICT-007`,
`r13`, dan kedua cacat `FE-LAB-14`: **tabel yang ditulis tanpa pembacanya tidak akan menghasilkan
galat apa pun sampai seseorang membutuhkannya.** `LabOrderedProcedure` berdiri, terisi, dan
menegakkan dua aturan validasi dengan benar selama dua hari, sementara isinya tidak pernah dapat
dilihat siapa pun di luar backend.

Yang menemukannya bukan lint, build, uji, maupun tinjauan kontrak — melainkan pertanyaan
"apa yang akan tercetak pada dokumen ini". **Task yang menghasilkan dokumen resmi wajib
menelusuri setiap ruas dokumennya sampai ke sumbernya sebelum dimulai**, karena dokumen yang
salah tidak menimbulkan galat; ia hanya dipercaya orang.

---

## 11. Amandemen `r16` — Nomor order terbaca dan waktu pengambilan yang dinyatakan, 2026-09-17

**Status `approved`**, disetujui Yoga Aji Pratama selaku pemilik modul pada 2026-09-17.

**Aditif seluruhnya.** Tidak satu pun endpoint, ruas, nilai enum, permission, atau migration
`r3`..`r15` yang berubah, berganti nama, atau hilang.

> **Amandemen ini memuat DUA hal yang datang dari dua tempat berbeda**, dan digabung justru
> supaya nomornya tidak bertabrakan. Yang kedua **sudah menjadi keputusan sejak 2026-09-16** dan
> hanya kurang dituliskan; laporan `BE-LAB-22` §6 menyebut sasarannya "`r10`" — angka itu ditulis
> ketika kontrak masih `r7`, dan sasaran sebenarnya adalah revisi ini.

### 11.1 Isi pertama — `orderNumber` dapat dibaca

`BE-LAB-36` mendirikan kolom `LabOrder.OrderNumber` menurunkan `LAB-DEC-072`: nomor pesanan yang
dapat dibaca, dicetak, dan **disebut lewat telepon**. Kolomnya `NOT NULL`, unik, dan kedelapan
pesanan lama sudah terisi — tetapi **nol DTO mengembalikannya**, sehingga ia tidak dapat dibaca
siapa pun di luar backend.

| Ruas | DTO | Tipe | Alasan penempatan |
|---|---|---|---|
| `orderNumber` | `LabOrderListResponse` | `string` | Kolom `No. Order` pada daftar pesanan. Siap tampil, bukan penunjuk |
| `orderNumber` | `LabMonitoringItemResponse` | `string` | Ketiga menu pemeriksaan membaca grup **Lab Monitoring**, bukan `LabOrderListResponse` |

`LabOrderDetailResponse` **tidak perlu disebut terpisah**: ia mewarisi `LabOrderListResponse`.

> **Kenapa dua DTO, bukan satu.** Ini pelajaran `r13`/`r14` yang diterapkan sejak awal, bukan
> diulang. `r13` menambahkan ruas pada DTO yang **paling masuk akal namanya** dan ternyata nol
> dipakai layar; `r14` harus mengoreksinya pada hari yang sama. Penempatan di sini diperiksa dari
> **source konsumennya** lebih dulu.

### 11.2 Isi kedua — `collectedAt` yang dinyatakan petugas

Menurunkan **`LAB-CONFLICT-006` pilihan A**, yang diputuskan pemilik modul pada 2026-09-16.

| Ruas | DTO | Tipe | Ketentuan |
|---|---|---|---|
| `collectedAt` | `PlanLabSpecimenRequest` | `DateTime?` | Waktu pengambilan sampel **yang dinyatakan petugas**. Boleh kosong |

**Alasan yang menentukan bukan aturannya, melainkan datanya.** Waktu pengambilan di klinik
perujuk memang diketahui petugas, dan hari ini **hilang — tidak tersimpan di mana pun**.
`CollectedAt` yang ada diisi server pada tindakan pengambilan, dan pada jalur rujukan luar cap
waktu itu justru **lebih akhir** daripada waktu kedatangan sampel.

**Yang dibuka ruas ini:** `VAL-59` dan `AC-66` menjadi dapat ditegakkan penuh. Keduanya selama ini
terpenuhi separuh — `VAL-58` tegak, `VAL-59` tidak, karena pembandingnya tidak ada pada satu pun
DTO permintaan.

> **Pilihan B ditolak, dan alasannya dicatat supaya tidak diusulkan ulang.** Mempersempit
> `VAL-59` akan membuat aturannya tidak pernah menyala — sama dengan mencabutnya — tetapi
> meninggalkan satu baris matriks validasi yang **terlihat aktif padahal tidak**. Aturan yang
> tampak berlaku dan diam-diam tidak pernah berjalan lebih berbahaya daripada aturan yang
> dicabut terang-terangan, seperti `VAL-60`.

### 11.3 Dampak kompatibilitas

| Yang dinilai | Hasil |
|---|---|
| Bentuk respons | **Aditif.** Pembaca lama menerima satu ruas tambahan yang boleh diabaikan |
| Bentuk permintaan | **Aditif dan opsional.** `collectedAt` boleh tidak dikirim; pemanggil lama tidak berubah |
| Permission | **Nol** resource maupun action baru |
| Migration | **Nol.** `LabOrder.OrderNumber` sudah berdiri lewat `BE-LAB-36`; `LabSpecimen.CollectedAt` sudah ada sejak semula |

---

## 12. Amandemen `r17` — Daftar penerimaan lintas pesanan, 2026-09-17

**Status `approved`**, disetujui Yoga Aji Pratama selaku pemilik modul pada 2026-09-17.

**Aditif.** Satu endpoint baca baru; nol endpoint yang sudah ada berubah.

### 12.1 Kenapa amandemen ini ada

`FE-LAB-12` menuntut *"layar daftar penerimaan beserta penyaring rentang tanggal"* — daftar wadah
**lintas pesanan**. Penelusuran source membuktikan **nol endpoint mengembalikannya**:

| Jalur yang ada | Yang dikembalikan |
|---|---|
| `GET /lab-specimens/summary` | **Angka rekap saja** — nol baris |
| `GET /lab-specimens/by-order/{labOrderId}` | Baris untuk **satu** pesanan |

Ditelusuri pula ke luar controller wadah: `LabMonitoringService` menyentuh `LabSpecimens` hanya
untuk **mencacah** dan sebagai sub-query penyaring; `LabWorklistService` nol menyentuhnya.

**Setengahnya justru sudah siap.** `GetSummaryAsync` menyaring tepat pada
`PhysicallyReceivedAt ?? CreateDateTime`, dan `AC-67` sudah terpenuhi di sana. **Yang hilang
barisnya, bukan aturannya.**

### 12.2 Endpoint

Base URL: `api/v1/health-services/laboratory-management/lab-specimens`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/` | Daftar penerimaan lintas pesanan | `LabSpecimen : Read` | `LabSpecimenPagedQuery` | `ApiResponse<PagedResult<LabSpecimenListResponse>>` |

**`LabSpecimenPagedQuery`** mengikuti bentuk `LabOrderPagedQuery` yang sudah berjalan sejak `r5`:

| Ruas | Tipe | Ketentuan |
|---|---|---|
| `startDate`, `endDate` | `DateTime?` | Rentang **waktu kedatangan sebenarnya** — lihat 12.3 |
| `specimenStatus` | `LabSpecimenStatus?` | Menyaring menurut status wadah |
| `search` | `string?` | Barcode wadah, nomor order, nama pasien, atau No. RM |
| `pageNumber`, `pageSize` | `int` | Bawaan 1 dan 20; paling banyak 100 |

**`LabSpecimenListResponse`** memuat seluruh ruas `LabSpecimenResponse` yang sudah ada, ditambah:

| Ruas | Tipe | Alasan |
|---|---|---|
| `labOrderId` | `Guid` | Menautkan baris ke pesanannya |
| `orderNumber` | `string` | Nomor yang dapat disebut petugas. **Bergantung `r16`** |
| `patientName` | `string` | Daftar ini dibaca per pasien, bukan per penunjuk |
| `medicalRecordNumber` | `string` | Pembeda ketika nama pasien sama |
| `createDateTime` | `DateTime` | **Supaya selisihnya dapat ditampilkan** — lihat 12.3 |

### 12.3 Satu hal yang menentukan, dan paling mudah keliru

**Rentangnya wajib disaring pada `PhysicallyReceivedAt ?? CreateDateTime`** — persis seperti
`GetSummaryAsync`, bukan pada `CreateDateTime` saja.

Inilah inti `LAB-DEC-042`. Wadah yang tiba **Senin 21.10** dan baru diregistrasi **Selasa 08.05**
wajib muncul pada hari **Senin**. Bila penyaringnya keliru, seluruh guna kolom
`PhysicallyReceivedAt` hilang **tanpa satu pun kesalahan yang terlihat**: layarnya tetap tampil
benar, hanya tanggalnya yang salah.

`createDateTime` ikut dikembalikan justru supaya **selisih** kedua waktu itu dapat dilihat kepala
instalasi, sebagaimana dituntut DoD `FE-LAB-12`.

### 12.4 Dampak kompatibilitas

| Yang dinilai | Hasil |
|---|---|
| Endpoint yang sudah ada | **Nol berubah** |
| Permission | **Nol** resource baru. Memakai ulang `LabSpecimen : Read` |
| Migration | **Nol.** Seluruh kolomnya sudah berdiri sejak `BE-LAB-21` dan `BE-LAB-22` |

---

## 13. Amandemen `r18` — Penyaring NIK dan Kategori Periode pada daftar pantau, 2026-09-17

**Status `approved`**, disetujui Yoga Aji Pratama selaku pemilik modul pada 2026-09-17.

**Aditif seluruhnya.** Dua ruas opsional pada satu DTO permintaan. Nol endpoint baru, nol ruas
respons, nol nilai enum yang bergeser, nol permission, nol migration.

### 13.1 Kenapa amandemen ini ada

`BR-50` butir 1 menetapkan penyaring menu Hasil: **NIK/No. RM, Kategori Periode, Tgl Awal, Tgl
Akhir, dan Jenis Kunjungan.** Empat dari enam sudah berjalan. Dua belum, dan keduanya
diturunkan di sini.

| Penyaring `BR-50` | Keadaan hari ini |
|---|---|
| Tgl Awal / Tgl Akhir | ✅ `StartDate` / `EndDate`, beserta penjagaan tanggal masa depan (`FE-LAB-18`, `AC-98`) |
| Jenis Kunjungan | ✅ `EncounterType` |
| No. RM | ✅ `MedicalRecordNumber` |
| Keyword Search | ✅ `Search` — nama pasien, No. RM, nomor kunjungan |
| **NIK** | ❌ `MstPatient.IdentityNumber` **ada** (`MstPatient.cs:54`), tetapi nol dijangkau penyaring Laboratorium |
| **Kategori Periode** | ❌ `LabMonitoringService.cs:172-182` menyaring **hanya** pada `RequestedAt ?? CreateDateTime` |

> **Kenapa diusulkan sekarang padahal menu Hasil sendiri tertahan `LAB-SIGN-001`.** Keduanya
> **tidak** bergantung pada hasil pemeriksaan. Ia menyaring pesanan, dan pesanan sudah ada.
> Ketiga menu Pemeriksaan yang berjalan hari ini memakai DTO yang sama dan memperoleh keduanya
> seketika. Sisa `BR-50` tetap tertahan; dua butir ini memang tidak.

### 13.2 Ruas pertama — `identityNumber`

Ditambahkan pada **`LabMonitoringQuery`**.

| Ruas | Tipe | Ketentuan |
|---|---|---|
| `identityNumber` | `string?` | NIK pasien, **cocok sebagian**, mengikuti persis pola `medicalRecordNumber` yang sudah berjalan |

Dibaca dari `MstPatient.IdentityNumber` lewat sub-query ke `Encounter.PatientId` — **cara yang
sama** dengan `PatientName` dan `MedicalRecordNumber` pada service itu, bukan lewat navigation
property baru. `BR-50` butir 1 melarang mengarang kolom; ruas ini memakai kolom yang sudah ada.

**`Search` sengaja TIDAK ikut diperlebar, dan itu keputusan, bukan kelalaian.** Memasukkan NIK
ke pencarian bebas membuat ketiga menu Pemeriksaan yang sudah berjalan **mengembalikan baris
yang sebelumnya tidak muncul**, tanpa satu pun layar meminta perubahan itu. Pelebaran diam-diam
sama merusaknya dengan pengetatan diam-diam — pelajaran `BE-LAB-21`, yang di sini berlaku ke
arah sebaliknya.

#### 13.2.1 Satu hal yang perlu diputuskan pemilik modul

Artifact menuliskannya sebagai **satu** kotak, `NIK/No. RM`. Kontrak ini menyediakan **dua**
ruas terpisah. Keduanya dapat benar, tetapi bentuk layarnya berbeda:

| # | Bentuk | Konsekuensinya |
|---:|---|---|
| **A** | Dua ruas tetap terpisah; layar menampilkan **pilihan** `Cari menurut: NIK / No. RM` di samping satu kotak | Tegas dan dapat diaudit. Petugas tahu persis apa yang dicarinya. **Nol tebakan format** |
| B | Satu kotak; layar **menebak** dari bentuk isian — 16 digit dianggap NIK, selainnya No. RM | Satu kotak, tetapi tebakannya rapuh: No. RM 16 digit akan dibaca sebagai NIK dan hasilnya kosong tanpa sebab yang terlihat |
| C | Satu ruas baru yang mencocokkan **NIK atau No. RM** sekaligus di backend | Paling dekat dengan artifact, tetapi menduplikasi `medicalRecordNumber` yang sudah ada, dan dua ruas yang saling tumpang tindih akan membingungkan pemanggil berikutnya |

**Usul: A.** Ia menghindari tebakan format yang gagalnya tidak terlihat, dan nol menduplikasi
ruas yang sudah berjalan. Pilihan ada pada pemilik modul.

### 13.3 Ruas kedua — `dateCategory`

Ditambahkan pada **`LabMonitoringQuery`**.

| Ruas | Tipe | Bawaan |
|---|---|---|
| `dateCategory` | `LabDateCategory?` | Kosong berarti `OrderDate` |

**`LabDateCategory`** — enum baru, **dua** nilai:

| Nilai | Angka | Disaring pada | Sumber |
|---|---:|---|---|
| `OrderDate` | `1` | `RequestedAt ?? CreateDateTime` | Perilaku yang berjalan hari ini |
| `SamplingDate` | `2` | `LabSpecimen.CollectedAt` | `BE-LAB-21`; ruas tulisnya dibuka `r16` |

#### 13.3.1 Kompatibilitas: pemanggil lama tidak boleh berubah perilakunya

**Ruasnya opsional, dan kosong berarti `OrderDate`.** Ketiga menu Pemeriksaan yang berjalan hari
ini nol mengirimnya, sehingga keduanya menempuh cabang yang **persis sama** dengan hari ini.
Inilah butir DoD yang paling mudah dilanggar diam-diam, dan ia wajib dibuktikan terbalik saat
dibangun: muatan lama **tetap** menghasilkan baris yang sama.

#### 13.3.2 Aturan `SamplingDate` — satu pesanan, beberapa wadah

Satu pesanan dapat memiliki beberapa wadah dengan waktu pengambilan berbeda. Aturannya:

> Sebuah pesanan ikut tersaring bila **salah satu** wadahnya ber-`CollectedAt` di dalam rentang.

Pola ini **bukan baru** — ia persis aturan `SpecimenStatus` yang sudah berlaku pada DTO yang
sama, dan alasannya ditulis di sana. Memakai ulang aturan yang sudah dipahami petugas lebih baik
daripada memperkenalkan aturan kedua untuk perkara yang sama.

#### 13.3.3 `CollectedAt` kosong tidak pernah cocok — dan kenapa ini BERBEDA dari `r17`

Wadah yang belum dinyatakan waktu pengambilannya ber-`CollectedAt` **`null`**. Aturannya:
**baris itu tidak pernah cocok ke rentang mana pun** pada kategori `SamplingDate`. **Tidak ada
jatuh-tempo ke `CreateDateTime`.**

Ini **sengaja berbeda** dari `r17`, yang justru menyaring pada `PhysicallyReceivedAt ??
CreateDateTime`, dan perbedaannya ditulis terang supaya tidak terbaca sebagai ketidakkonsistenan:

| | `r17` — laporan penerimaan | `r18` — kategori periode |
|---|---|---|
| Pertanyaannya | *"Wadah apa saja yang masuk hari itu?"* | *"Pesanan mana yang **diambil sampelnya** dalam rentang ini?"* |
| Substitusi terlihat pembaca? | **Ya** — `FE-LAB-12` menandai terang-terangan baris yang waktu tibanya tidak pernah dicatat, beserta selisihnya | **Tidak** — hasilnya hanya daftar; baris bersubstitusi tidak dapat dibedakan |
| Akibat substitusi | Baris tetap terhitung, dan pembacanya tahu | Pesanan yang **tidak pernah diambil sampelnya** akan muncul sebagai *"diambil tanggal sekian"* |

Jatuh-tempo di sini akan menjawab pertanyaan yang tidak ditanyakan, dan salahnya **tidak terlihat
dari layar**. Itu kelas kesalahan yang sudah enam kali menimpa modul ini.

#### 13.3.4 `ExaminationDate` sengaja TIDAK ada pada enum

Artifact menawarkan **tiga** pilihan; yang ketiga adalah **Tanggal Pemeriksaan**. Ia **tidak
dicantumkan**, dan itu keputusan yang paling penting pada amandemen ini.

`LabExamination` hari ini punya 20 properti dan **nol** di antaranya waktu pemeriksaan —
diverifikasi ulang 2026-09-17. Mencantumkan nilai enum yang tidak punya kolom pendukung berarti
mendirikan **pilihan yang tidak menuju ke mana-mana**: layar menawarkannya, petugas memilihnya,
dan daftarnya kosong atau salah tanpa satu pun galat.

**Itu persis pola yang sudah enam kali menimpa modul ini** — sesuatu yang berdiri tanpa
pasangannya dan nol menimbulkan galat sampai seseorang membutuhkannya. Nilai `ExaminationDate`
masuk **bersama** kolomnya, pada amandemen tersendiri, ketika `S4` terbuka.

**Akibatnya pada layar, ditulis supaya tidak ditemukan belakangan:** pilihan Kategori Periode
memuat **dua** butir, bukan tiga. Bila yang ketiga wajib terlihat sebagai penanda arah, ia
ditampilkan **nonaktif beserta alasannya** — bukan aktif dan kosong.

### 13.4 Validasi

Satu aturan baru pada `LAB-VAL-v1`:

| ID | Aturan | Kode | Ditegakkan oleh |
|---|---|---|---|
| `VAL-76` | `dateCategory` wajib salah satu nilai yang dikenal | `400` | **Pengikatan query ASP.NET Core**, bukan penjaga di controller |

**Penegaknya diperiksa, bukan diasumsikan — dan hasilnya membalik rancangan awal.** Penjaga
`Enum.IsDefined` sempat ditulis di controller, lalu dibuktikan **tidak pernah tercapai**:
pengikatan query menjalankan pemeriksaan yang sama lebih dulu, sehingga `?dateCategory=99`
maupun `=0` sudah ditolak sebelum satu baris controller pun berjalan. Penjaganya **dicabut** —
penjaga yang tidak pernah tercapai terbaca seolah menjadi penegaknya, dan pesannya akan
dipelihara orang tanpa pernah sampai ke siapa pun.

**Bentuk jawaban yang benar-benar diterima pemanggil**, dibaca apa adanya dari aplikasi yang
berjalan pada 2026-09-17:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": { "DateCategory": ["The value '99' is invalid."] }
}
```

> **Satu selisih dilaporkan, tidak diperbaiki di sini.** Bentuk itu adalah `ProblemDetails`
> bawaan framework, **bukan** amplop `ApiResponse` yang dipakai seluruh jawaban modul ini, dan
> pesannya berbahasa Inggris. Selisih ini **berlaku bagi setiap ruas enum pada query string di
> seluruh aplikasi**, bukan hanya ruas ini — menyeragamkannya adalah keputusan tingkat aplikasi
> di luar cakupan amandemen ini. Yang perlu diketahui sisi layar: nilai di luar daftar **ditolak**
> dengan benar, tetapi pesannya tidak siap ditampilkan apa adanya kepada petugas. Layar memang
> tidak pernah mengirimkannya — pilihannya datang dari daftar tertutup.

**Nol aturan baru untuk `identityNumber`** — ia dirapikan dan diabaikan bila kosong, persis
`medicalRecordNumber`.

> **Satu catatan teknis yang pernah menelan biaya.** `Program.cs` **nol** mendaftarkan
> `JsonStringEnumConverter`, sehingga enum pada **body** JSON hanya terbaca sebagai angka —
> temuan `FE-LAB-13`. `dateCategory` adalah ruas **query string**, dan pengikatan query mengenali
> **nama**; `?dateCategory=SamplingDate` sah. Ditulis di sini supaya tidak diperdebatkan ulang
> saat dibangun.

### 13.4b Cacat yang sudah ada, ditemukan saat amandemen ini diverifikasi

**Penyaring tanggal pada ketiga layar Pemeriksaan menjawab `500`, bukan daftar** — dan sudah
begitu sebelum amandemen ini. Ditemukan 2026-09-17 ketika `dateCategory` diuji terhadap aplikasi
yang benar-benar berjalan; **cabang `OrderDate` yang tidak disentuh `r18` gagal identik**,
sehingga ia bukan regresi amandemen ini.

| Hal | Temuan |
|---|---|
| Yang dikirim layar | `YYYY-MM-DD` apa adanya — `FilterDatePicker` menghasilkan bentuk itu, dan `cleanParams` hanya membuang nilai kosong |
| Yang dikontrakkan | `Type = "date"`, `Example = "2026-09-01"` — **bentuk itu memang kontraknya** |
| Yang terjadi | Nilai tanpa zona terikat `DateTimeKind.Unspecified`; Npgsql **menolak** menulisnya ke `timestamp with time zone` |
| Jawabannya | `500 Terjadi kesalahan pada server.` |

**Dua hal diperbaiki sekaligus, dan yang kedua tidak akan terlihat tanpa yang pertama.**

1. **Penormalan zona.** Nilai tanpa zona dibaca sebagai **jam dinding WIB** — bukan UTC — karena
   itulah yang dimaksud petugas ketika mengetik satu tanggal. Ditempatkan pada
   `AppDateTimeHelper.ToUtc`, bukan disalin ke controller.
2. **Akhir rentang dinaikkan ke penghabisan hari.** `LAB-DEC-071` menetapkan pembandingnya
   **inklusif** justru supaya pencarian **satu hari** mungkin. Tanpa ini,
   `startDate=2026-09-16&endDate=2026-09-16` berarti 00:00 sampai 00:00 dan mengembalikan **nol
   baris** — persis kegagalan yang keputusan itu tulis untuk dicegah. Hanya nilai yang jamnya
   tepat tengah malam yang dinaikkan; nilai yang membawa jam sendiri tidak disentuh.

Terbukti sesudah perbaikan, dengan format polos yang **persis** dikirim layar:

| Pencarian satu hari | Hasil |
|---|---:|
| `OrderDate` 2026-09-09 | 3 baris |
| `OrderDate` 2026-09-16 | 1 baris |
| `SamplingDate` 2026-09-09 | 3 baris |
| `SamplingDate` 2026-09-16 | **0 baris** — benar; pesanan hari itu nol wadah terambil |

> **Cakupan perbaikan pertama hanya grup `Lab Monitoring`;** grup lain dicatat sebagai pekerjaan
> tersendiri, bukan diperbaiki diam-diam di luar cakupan. **Pemeriksaan itu dijalankan pada hari
> yang sama — lihat 13.4c.**

### 13.4c Pemeriksaan susulan — cacat yang sama ternyata mengenai lima endpoint lain

Dijalankan 2026-09-17 sebagai penutup utang 13.4b. **Setiap** ruas tanggal pada grup Laboratorium
diprobe dengan bentuk polos `YYYY-MM-DD` terhadap aplikasi yang berjalan.

| Endpoint | Sebelum | Sesudah |
|---|---|---|
| `GET /lab-orders` | **`500`** | `200` |
| `GET /lab-orders/summary` | **`500`** | `200` |
| `GET /lab-specimens` | **`500`** | `200` |
| `GET /lab-specimens/summary` | **`500`** | `200` |
| `GET /lab-specimen-types/other-usage` | **`500`** | `200` |
| `GET /lab-monitoring/*` | sudah diperbaiki 13.4b | `200` |

> **Satu negatif palsu dicatat supaya caranya tidak ditiru.** Putaran pertama menyimpulkan
> `lab-specimen-types` **selamat**, padahal yang diprobe adalah `/summary` — endpoint yang
> **nol menerima** ruas tanggal, sehingga parameternya diabaikan dan jawabannya `200` tanpa
> membuktikan apa pun. Endpoint yang sebenarnya, `/other-usage`, ternyata ikut terkena.
> **Probe yang menjawab `200` karena tidak membaca masukannya terlihat persis sama dengan probe
> yang benar-benar lulus.**

**Aturannya dipindah ke satu tempat, bukan disalin enam kali.** `LabQueryDateRange` memegang
penormalan zona dan kenaikan akhir hari; keenam pemanggilnya menyebut nama yang sama. Enam
salinan aturan yang sama pasti bercabang, dan cabangnya **tidak menimbulkan galat** — hanya
tanggal yang salah pada satu layar dan benar pada layar lain.

**Dibuktikan isinya, bukan hanya kode statusnya:** pencarian satu hari pada `lab-orders`
mengembalikan 3, 2, dan 3 baris untuk 09-09, 09-14, dan 09-16 — berjumlah **8**, yaitu seluruh
pesanan aktif. Bentuk polos dan bentuk `...Z` menghasilkan angka yang sama pada data ini.

> **Keduanya bukan rentang yang identik, dan itu disengaja.** Bentuk polos berarti hari kalender
> **WIB**; bentuk `...Z` dihormati apa adanya sebagai UTC. Keduanya kebetulan sama di sini karena
> barisnya jauh dari batas hari. Pemanggil yang sudah mengirim `...Z` karena itu **nol berubah
> perilakunya**.

### 13.5 Dampak kompatibilitas

| Yang dinilai | Hasil |
|---|---|
| Endpoint | **Nol berubah.** Keduanya ruas pada DTO permintaan yang sudah ada |
| Ruas respons | **Nol.** Tidak ada satu pun ruas baru dikembalikan |
| Nilai enum yang sudah ada | **Nol bergeser.** `LabDateCategory` enum **baru**, bukan sisipan ke enum lama |
| Pemanggil lama | **Nol diketatkan.** Kedua ruas opsional; kosong berarti perilaku hari ini |
| Permission | **Nol** resource baru. Memakai ulang `LabOrder : Read` |
| Migration | **Nol.** `MstPatient.IdentityNumber` dan `LabSpecimen.CollectedAt` keduanya sudah berdiri |

### 13.6 Traceability

| Butir | Menurunkan | Menutup |
|---|---|---|
| `identityNumber` | `BR-50` butir 1; `LAB-DEC-065` | `REC3-NEW-003` |
| `dateCategory` | `BR-50` butir 1 | `REC3-NEW-002` **sebagian** — dua dari tiga pilihan |

`REC3-NEW-002` **tidak** tertutup penuh, dan itu ditulis terang: pilihan ketiga menunggu `S4`.

### 13.7 Keputusan pemilik modul — 2026-09-17

| # | Pertanyaan | Keputusan |
|---:|---|---|
| 1 | Bentuk penyaring NIK/No. RM | **A** — dua ruas tetap terpisah; layar menampilkan pilihan `Cari menurut: NIK / No. RM` di samping satu kotak. Nol tebakan format |
| 2 | Kategori Periode dua pilihan sekarang, atau tunggu `S4` | **Dua sekarang.** Pilihan ketiga **tidak** ditampilkan sama sekali — bukan ditampilkan nonaktif — sampai kolomnya ada |

---

## 14. Amandemen `r19` — Gender pasien pada daftar pantau, 2026-09-17

**Status `approved`**, disetujui Yoga Aji Pratama selaku pemilik modul pada 2026-09-17.

**Aditif.** Satu ruas respons. Nol endpoint, nol ruas permintaan, nol permission, nol migration.

### 14.1 Kenapa amandemen ini ada — dan koreksi atas catatan sebelumnya

`REC3-NEW-010` sempat dicatat sebagai **"kewenangan UI, tidak menyentuh kontrak backend"**.
Pemeriksaan source 2026-09-17 membantahnya: **`LabMonitoringItemResponse` nol membawa gender**,
dan penelusuran seluruh DTO Laboratorium hanya menemukannya pada
`LabPatientRegistrationDtos.cs` — jalur pendaftaran, bukan daftar pantau.

Ikon gender karena itu **tidak dapat dibangun dari sisi layar saja**. Catatan lamanya keliru
karena mengandaikan datanya sudah sampai; ia tidak.

### 14.2 Ruas

Ditambahkan pada **`LabMonitoringItemResponse`**.

| Ruas | Tipe | Nilai |
|---|---|---|
| `gender` | `string?` | `Male`, `Female`, `Unknown`, `NotDisclosed`, atau `null` |

**Bentuknya mengikuti ruas enum lain pada DTO yang sama** — `orderStatus`, `encounterType`, dan
`paymentType` seluruhnya `string` berisi **nama enum**, dihasilkan `.ToString()`. Nol pola baru
diperkenalkan.

**`null` tetap mungkin dan bukan kelalaian:** `MstPatient.Gender` nullable, dan pesanan tanpa
kunjungan tidak punya pasien yang dapat dibaca.

### 14.3 Empat keadaan, tiga tampilan — keputusan pemilik modul

Artifact hanya menyebut **dua** warna: biru/navy untuk laki-laki, pink untuk perempuan. Ia
**diam** soal dua nilai enum lainnya dan soal `null`. Kediaman itu tidak ditebak.

| Nilai | Tampilan | Label aksesibel |
|---|---|---|
| `Male` | Ikon **biru/navy** | Laki-laki |
| `Female` | Ikon **pink** | Perempuan |
| `Unknown` / `null` | Ikon **netral abu-abu** | Gender tidak diketahui |
| `NotDisclosed` | Ikon **netral abu-abu** | Gender tidak diinformasikan |

**Kenapa bukan "tanpa ikon".** Baris tanpa ikon tidak dapat dibedakan dari baris yang ikonnya
gagal dimuat — petugas tidak tahu apakah datanya memang kosong atau layarnya bermasalah.

**Kenapa bukan tanda tanya.** `NotDisclosed` adalah pilihan **sadar** pasien, bukan pengisian
yang kurang. Tanda tanya membacanya sebagai kekurangan data; keduanya hal yang berbeda.
Warnanya disamakan karena keduanya sama-sama "bukan laki-laki/perempuan yang dapat ditampilkan",
tetapi **labelnya dibedakan** sehingga perbedaannya tetap terbaca pembaca layar dan tooltip.

### 14.4 Dampak kompatibilitas

| Yang dinilai | Hasil |
|---|---|
| Endpoint | **Nol berubah** |
| Ruas yang sudah ada | **Nol berubah**; `gender` murni tambahan |
| Pemanggil lama | **Nol diketatkan** — ruas baru pada respons diabaikan pemanggil yang tidak membacanya |
| Permission | **Nol** resource baru |
| Migration | **Nol.** `MstPatient.Gender` sudah berdiri |

### 14.5 Traceability

| Butir | Menurunkan | Menutup |
|---|---|---|
| `gender` | `BR-50` bagian 5.3 baris kolom Pasien | `REC3-NEW-010` |

> **Cakupannya ketiga layar Pemeriksaan, bukan Menu Hasil.** Kolom Pasien pada artifact adalah
> kolom **Datatable Hasil** (`S17`), yang belum ada. Karena `LAB-DEC-070` menetapkan Menu Hasil
> mengikuti pola **tiga daftar sejajar** yang sama, ruas ini berdiri pada DTO yang kelak
> dipakainya — dan ketiga layar Pemeriksaan memperolehnya sekarang.

---

## 15. Amandemen `r20` — Golongan darah pada daftar pantau, 2026-09-17

**Status `approved`**, disetujui Yoga Aji Pratama selaku pemilik modul pada 2026-09-17.

**Aditif.** Satu ruas respons. Nol endpoint, nol ruas permintaan, nol permission, nol migration.

### 15.1 Kenapa amandemen ini ada

`REC3-NEW-009` dipindahkan ke **Rilis 1** atas keputusan pemilik modul 2026-09-17, mengamandemen
penempatan `LAB-DEC-030`. Dari ketiga dokumennya, **Label Goldar** ditempel pada tube berisi
sampling pasien dan — atas keputusan pemilik modul pada tanggal yang sama — **menampilkan
golongan darahnya**.

Artifact sendiri hanya menulis *"informasi utama pasien"* untuk label ini dan **tidak menyebut
golongan darah**, walaupun namanya menyebutnya. Kediaman itu tidak ditebak: pemilik modul
memutuskan golongan darahnya ikut, karena label tube tanpa golongan darah sulit dibedakan
gunanya dari Label Lab.

`MstPatient.BloodType` **sudah berdiri**; yang tidak ada adalah jalannya ke layar.

### 15.2 Ruas

Ditambahkan pada **`LabMonitoringItemResponse`**.

| Ruas | Tipe | Nilai |
|---|---|---|
| `bloodType` | `string?` | `APositive`, `ANegative`, `BPositive`, `BNegative`, `ABPositive`, `ABNegative`, `OPositive`, `ONegative`, `Unknown`, `NotDisclosed`, atau `null` |

**Ditempatkan bersama `gender` dan `patientName`, bukan pada detail pesanan.** Dokumen cetak
mengambil identitas pasien dari **baris daftar**; detail hanya dipanggil untuk dua hal yang tidak
dimiliki baris — nama pembuat order dan daftar pemeriksaan terpesan (`r15`). Menempatkan
golongan darah di detail berarti label tube sebesar 50×25 mm memaksa satu panggilan tambahan
untuk satu kata.

**Bentuknya nama enum**, mengikuti `orderStatus`, `encounterType`, `paymentType`, dan `gender`
pada DTO yang sama. Nol pola baru.

> **`MstPatient.BloodType` non-nullable dan berbawaan `Unknown`**, tetapi ruas ini tetap `string?`
> karena pesanan tanpa kunjungan nol punya pasien yang dapat dibaca. Layar memperlakukan `null`
> sama dengan `Unknown`.

### 15.3 Singkatannya milik layar, bukan kontrak

Kontrak mengirim **nama enum**; `A+` dan `O−` dibentuk layar. Alasannya: singkatan adalah
keputusan tampilan yang berbeda per media — label tube 50×25 mm menuntut `A+`, sedangkan layar
dan Nota Lab masih muat menuliskan `A Positif`. Mengirim singkatannya dari backend mengunci
keduanya pada satu bentuk.

**`Unknown`, `NotDisclosed`, dan `null` ditampilkan sebagai tanda pisah beserta katanya, bukan
dikosongkan.** Label tube yang kosong pada bagian golongan darah tidak dapat dibedakan dari label
yang tercetak tidak sempurna — dan ini label yang menempel pada spesimen pasien.

### 15.4 Dampak kompatibilitas

| Yang dinilai | Hasil |
|---|---|
| Endpoint | **Nol berubah** |
| Ruas yang sudah ada | **Nol berubah** |
| Pemanggil lama | **Nol diketatkan** — ruas baru pada respons diabaikan pemanggil yang tidak membacanya |
| Permission | **Nol** resource baru |
| Migration | **Nol.** `MstPatient.BloodType` sudah berdiri |

### 15.5 Traceability

| Butir | Menurunkan | Menutup |
|---|---|---|
| `bloodType` | `BR-50` bagian 5.5 Label Goldar; `LAB-DEC-030` sebagaimana diamandemen 2026-09-17 | Bagian `REC3-NEW-009` yang menuntut golongan darah |

---

## 16. Amandemen `r21` — Pengisian hasil pemeriksaan, 2026-09-17

**Status `approved`**, disetujui Yoga Aji Pratama selaku pemilik modul pada 2026-09-17,
menurunkan `LAB-DEC-005` di bawah pemecahan slice `LAB-DEC-076`.

**Satu endpoint tulis baru.** Nol endpoint yang sudah ada berubah.

### 16.1 Batas yang paling penting pada amandemen ini

**Endpoint ini mengisi hasil. Ia tidak memvalidasi, tidak merilis, tidak menandai nilai kritis,
dan tidak mengoreksi.** Keempatnya tertahan `LAB-SIGN-001` lewat `LAB-DEC-003`, `LAB-DEC-004`,
dan `LAB-DEC-007`.

**Nol status hasil diperkenalkan.** `LabExaminationStatus` tetap berisi empat nilainya yang lama,
dan komentar pada enum itu — yang sudah menyatakan `Pending`/`InProcess`/`Completed`/
`Validated`/`Released` sengaja ditahan — **tetap berlaku apa adanya**.

> *"Hasil sudah diisi"* dibaca dari `resultEnteredAt != null`. Itu **fakta yang tercatat**, bukan
> **janji tentang apa yang terjadi berikutnya** — dan perbedaan itu persis yang memisahkan `S4a`
> dari `S4`.

### 16.2 Endpoint

Base URL: `api/v1/health-services/laboratory-management/lab-examinations`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `PUT` | `/{id}/result` | Mengisi atau memperbaiki hasil yang **belum** divalidasi | `LabExamination : Update` | `LabExaminationResultRequest` | `ApiResponse<LabExaminationResultResponse>` |

**Memakai ulang `LabExamination : Update`, bukan resource baru.** Selama validasi dan rilis belum
ada, memecah izin pengisian dari izin pemeriksaan lain berarti menetapkan pembagian wewenang yang
justru menunggu jawaban `LAB-SIGN-001`.

**`LabExaminationResultRequest`**

| Ruas | Tipe | Ketentuan |
|---|---|---|
| `resultNumeric` | `decimal?` | Diisi **hanya** bila bentuk hasilnya `Numeric` |
| `resultOptionId` | `Guid?` | Diisi **hanya** bila bentuk hasilnya `Choice`; wajib menunjuk `LabValueOption` milik batas nilai yang berlaku |
| `examinedAt` | `DateTime?` | **Kapan pemeriksaannya dikerjakan.** Boleh kosong; bila kosong diisi waktu sekarang |

**Yang sengaja TIDAK diterima:** satuan, batas nilai, waktu pengetikan, dan pelakunya.
Keempatnya **diturunkan server** — satuan dan batas dari `LabValueBound` yang berlaku, waktu dan
pelaku dari sesi. Menerimanya dari pemanggil berarti mengizinkan hasil mengaku diperiksa dengan
batas yang tidak pernah berlaku baginya.

### 16.3 Validasi

| ID | Aturan | Kode |
|---|---|---|
| `VAL-77` | Pemeriksaan wajib ada dan belum terhapus | `404` |
| `VAL-78` | Pemeriksaan yang `Voided` atau `Cancelled` tidak dapat diisi hasilnya | `409` |
| `VAL-79` | Bentuk hasilnya wajib punya batas nilai yang berlaku bagi pemeriksaan itu | `422` |
| `VAL-80` | Bentuk `Numeric` menuntut `resultNumeric` dan **menolak** `resultOptionId`; bentuk `Choice` sebaliknya | `422` |
| `VAL-81` | `resultOptionId` wajib milik batas nilai yang berlaku, bukan pilihan dari pemeriksaan lain | `422` |
| `VAL-82` | `examinedAt` **tidak boleh di masa depan** | `422` |

**`VAL-82` mengikuti `LAB-DEC-064`** yang sudah melarang tanggal masa depan pada penyaring —
aturan yang sama, diterapkan pada waktu kejadian. Pemeriksaan yang mengaku dikerjakan besok
adalah data yang salah, dan salahnya baru ketahuan ketika laporannya dibaca.

### 16.4 Batas nilai dipilih server, dan snapshot-nya disimpan

`LabValueBound` dibedakan menurut **jenis kelamin** dan **kelompok umur** (`LAB-DEC-018`), jadi
satu jenis pemeriksaan dapat punya beberapa baris batas. Server memilih baris yang berlaku bagi
pasien pemeriksaan itu, lalu menyimpan **penunjuknya** (`resultValueBoundId`) beserta **satuannya**
(`resultUnitSnapshot`).

**Tanpa ini, hasil lama berubah artinya ketika batasnya diperbarui.** Kalium 3,4 yang hari ini di
bawah normal dapat menjadi normal besok bila batasnya digeser — dan perubahan itu berlaku surut
pada hasil yang sudah tercetak.

### 16.5 Dampak kompatibilitas

| Yang dinilai | Hasil |
|---|---|
| Endpoint yang sudah ada | **Nol berubah** |
| Nilai enum | **Nol bertambah** — `LabExaminationStatus` tidak disentuh |
| Permission | **Nol** resource baru |
| Migration | **Satu**, aditif — tujuh kolom nullable pada `LabExamination`; ketujuh baris lama utuh |

---

## 17. Amandemen `r22` — Jalur baca pengisian hasil, 2026-09-17

**Status `approved`**, disetujui Yoga Aji Pratama selaku pemilik modul pada 2026-09-17.

**Aditif.** Satu endpoint baca baru dan dua ruas pada daftar kerja. Nol endpoint yang sudah ada
berubah bentuk maupun perilakunya.

### 17.1 Kenapa amandemen ini ada

`r21` membuka jalur **tulis** hasil, tetapi layar tidak dapat memakainya tanpa mengetahui **bentuk
hasil** pemeriksaan itu lebih dulu: kotak angka untuk `Numeric`, daftar pilihan untuk `Choice`.
Bentuk itu ditentukan `LabValueBound` yang berlaku bagi **pasien tertentu** — dibedakan jenis
kelamin dan kelompok umur — sehingga layar tidak dapat menurunkannya sendiri dari jenis
pemeriksaan saja.

**Menebaknya berarti menampilkan kotak angka untuk pemeriksaan yang hanya menerima pilihan**, dan
petugas baru mengetahuinya sesudah `422` datang.

### 17.2 Endpoint

| Method | Path | Kegunaan | Hak akses | Response |
|---|---|---|---|---|
| `GET` | `/lab-examinations/{id}/result` | Bentuk hasil yang berlaku, batas rujukannya, pilihan yang sah, dan hasil yang sudah terisi | `LabExamination : Read` | `ApiResponse<LabExaminationResultFormResponse>` |

**Batas yang berlaku dipilih server dengan aturan yang sama persis** seperti pada `r21` — yang
paling khusus menang. Satu jalur pemilihan, dipakai baca maupun tulis; dua salinan aturan yang
sama pasti bercabang, dan cabangnya membuat layar menampilkan rujukan yang berbeda dari yang
dipakai menilai hasilnya.

**`LabExaminationResultFormResponse`**

| Ruas | Tipe | Keterangan |
|---|---|---|
| `resultForm` | `string` | `Numeric` atau `Choice` |
| `unit`, `normalLow`, `normalHigh` | — | **Rujukan yang ditampilkan kepada analis** saat mengetik |
| `options[]` | daftar | Hanya terisi pada `Choice`: `id`, `optionName`, `isOutOfReference` |
| `resultNumeric`, `resultOptionId`, `examinedAt`, `resultEnteredAt` | — | Hasil yang **sudah** terisi, supaya layar dapat menampilkan dan memperbaikinya |
| `canEnterResult` | `bool` | `false` bila pemeriksaannya `Voided`/`Cancelled` atau nol punya batas nilai |

> **`criticalLow` dan `criticalHigh` sengaja TIDAK dikembalikan.** Batas kritis adalah
> `LAB-DEC-004`, yang tertahan `LAB-SIGN-001`. Mengirimkannya akan mengundang layar menandai
> nilai kritis — dan penandaan itu menjanjikan alur pelaporan yang belum diputuskan pihak klinis.

### 17.3 Dua ruas pada daftar kerja

Ditambahkan pada **`LabWorklistItemResponse`**: `examinedAt` dan `resultEnteredAt`.

**Tanpa keduanya, daftar kerja tidak dapat membedakan pemeriksaan yang sudah diisi dari yang
belum** — dan analis akan mengetik ulang hasil yang sudah ada tanpa satu pun tanda.

Keduanya **waktu, bukan status**. Disiplin `r21` tetap berlaku: yang dikirim adalah **apa yang
tercatat**, bukan **apa yang terjadi berikutnya**.

### 17.4 Dampak kompatibilitas

| Yang dinilai | Hasil |
|---|---|
| Endpoint yang sudah ada | **Nol berubah** |
| Pemanggil lama | **Nol diketatkan** — dua ruas baru pada respons diabaikan yang tidak membacanya |
| Permission | **Nol** resource baru; memakai ulang `LabExamination : Read` |
| Migration | **Nol** |

---

## 18. Amandemen `r23` — Pilihan ketiga Kategori Periode, 2026-09-17

**Status `approved`**, disetujui Yoga Aji Pratama selaku pemilik modul pada 2026-09-17.

**Satu nilai enum baru.** Nol endpoint, nol ruas, nol permission, nol migration.

### 18.1 Kenapa sekarang, padahal `r18` menolaknya

`r18` menolak mencantumkan `ExaminationDate` dengan alasan yang ditulis terang: `LabExamination`
nol punya kolom waktu pemeriksaan, sehingga mencantumkannya berarti mendirikan **pilihan yang
tidak menuju ke mana-mana** — layar menawarkannya, petugas memilihnya, dan daftarnya kosong tanpa
satu pun galat.

**Kedua syarat itu kini terpenuhi:**

| Syarat | Dipenuhi oleh |
|---|---|
| Kolomnya ada | `LabExamination.ExaminedAt` — `r21`, migration `AddLabExaminationResultEntry` |
| **Ada yang mengisinya** | `PUT /lab-examinations/{id}/result` (`r21`) beserta layar pengisiannya (`r22`) |

Syarat kedua yang menentukan. Kolom tanpa penulis adalah persis kesalahan `BE-EXT-04`, dan
mencantumkan pilihannya saat itu akan mengulangnya dari sisi yang berbeda.

> **Penjagaan `r18` bekerja sebagaimana dirancang.** Uji unit dan pemeriksaan layar yang mengunci
> **ketiadaan** pilihan ketiga adalah gerbangnya — dan gerbang itu baru dibuka sekarang, bukan
> dicabut diam-diam. Keduanya diperbarui bersama amandemen ini, dan perubahannya sendiri yang
> menjadi bukti bahwa prasyaratnya berubah.

### 18.2 Nilai

`LabDateCategory` memperoleh nilai ketiga:

| Nilai | Angka | Disaring pada |
|---|---:|---|
| `OrderDate` | `1` | `RequestedAt ?? CreateDateTime` |
| `SamplingDate` | `2` | `LabSpecimen.CollectedAt` |
| **`ExaminationDate`** | **`3`** | **`LabExamination.ExaminedAt`** |

**Nilai lama nol bergeser**, sehingga pemanggil yang mengirim `1` atau `2` — maupun namanya —
memperoleh perilaku yang persis sama.

### 18.3 Aturannya sama dengan `SamplingDate`, dan itu disengaja

> Sebuah pesanan ikut tersaring bila **salah satu** pemeriksaannya ber-`ExaminedAt` di dalam
> rentang.

Satu pesanan dapat memuat beberapa pemeriksaan yang dikerjakan pada waktu berbeda — elektrolit
pagi, kultur sore. Pola ini sudah dipakai `SamplingDate` dan penyaring status wadah pada DTO yang
sama; memperkenalkan aturan kedua untuk perkara yang sama hanya akan membuat dua penyaring
bertetangga berperilaku berbeda tanpa sebab yang terbaca.

**`ExaminedAt` kosong tidak pernah cocok, dan tidak jatuh-tempo ke waktu mana pun.** Sebab yang
sama dengan `CollectedAt` pada `r18` bagian 13.3.3: pertanyaannya *"pesanan mana yang
**dikerjakan** dalam rentang ini"*, dan pemeriksaan yang belum dikerjakan bukan bagian dari
jawabannya.

### 18.4 Dampak kompatibilitas

| Yang dinilai | Hasil |
|---|---|
| Endpoint | **Nol berubah** |
| Nilai enum yang sudah ada | **Nol bergeser** — `ExaminationDate` disisipkan sebagai `3`, bukan menggeser yang lain |
| Pemanggil lama | **Nol diketatkan** — ruasnya tetap opsional dan bawaannya tetap `OrderDate` |
| Permission, migration | **Nol** |

### 18.5 Traceability

Menutup **`REC3-NEW-002` sepenuhnya**. Ketiga pilihan yang artifact tawarkan kini berdiri, dan
ketiganya punya sumber data yang benar-benar terisi.

---

## 19. Amandemen `r24` — Pengisian hasil Mikrobiologi dan Patologi Anatomi, 2026-09-18

> ### ✅ STATUS: `approved` — 2026-09-18
>
> Disetujui **Yoga Aji Pratama** selaku pemilik modul pada 2026-09-18, bersama `LAB-VAL-v1` `r7`
> dan `LAB-PERM-v1` revision 6.
>
> | Field | Nilai |
> |---|---|
> | `contract_version` | `LAB-API-v1` |
> | Revision | `r24` |
> | Status | **`approved`** |
> | `approved_by` / `approved_at` | Yoga Aji Pratama (`yogaaji452@gmail.com`) / 2026-09-18 |
> | `input_revision` | decisions rev 44; `LAB-RCG-001-r7`; `LAB-DA-001` rev 6 |
> | Kesiapan arsitektur domain | `DOMAIN_ARCHITECTURE_READY` |
> | Dampak kompatibilitas | **Aditif.** Nol endpoint yang sudah ada berubah; nol ruas yang sudah ada bergeser |
>
> **Label `Rencana (belum tersedia)` pada tabel di bawah tetap berlaku apa adanya.** Ia menyatakan
> endpointnya **belum ada di kode**, bukan bahwa kontraknya belum disetujui. Keduanya hal yang
> berbeda, dan label itu baru dicabut ketika task pembangunannya selesai.

Menurunkan `LAB-DEC-027` (BR-23), `LAB-DEC-084`, `LAB-DEC-080`, dan `LAB-DEC-081`.

### 19.1 Batas yang paling penting pada amandemen ini

Sama seperti `r21`, dan perlu diulang karena scope-nya persis sekelas:

**Seluruh endpoint di bawah mengisi hasil. Nol di antaranya memvalidasi, merilis, menandai nilai
kritis, atau mengoreksi hasil yang sudah dirilis.** Keempatnya milik `S4`, `S4d`, `S4e`, dan
`S6` — dan ketiga slice validasi masih tertahan `DEC-LAB-011`.

**Nol status hasil diperkenalkan** (`LAB-DEC-080`, ditegakkan `INV-29`). *"Hasil sudah diisi"*
tetap dibaca dari `resultEnteredAt != null`.

**Nol penanda `Definitif`** (`LAB-DEC-081`). **Nol penilaian kritis otomatis** (`INV-28`) — BR-23
menyatakan bakteri resisten dan kesimpulan patologi adalah penilaian klinis, bukan perbandingan
angka.

### 19.2 Endpoint — hasil Mikrobiologi (`S4b`)

Base URL: `api/v1/health-services/laboratory-management/lab-examinations`

`[Tags("Health Services / Laboratory Management / Lab Examination")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `PUT` | `/{id}/result/microbiology` | Mengisi status temuan beserta seluruh isolat dan kepekaannya sekaligus | `LabExamination : Update` | `LabMicrobiologyResultRequest` | `ApiResponse<LabMicrobiologyResultResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/result/microbiology` | Membaca hasil mikrobiologi yang sudah diisi | `LabExamination : Read` | — | `ApiResponse<LabMicrobiologyResultResponse>` | **Rencana (belum tersedia)** |

> **Kenapa satu `PUT` utuh, bukan endpoint terpisah per isolat.** Isolat dan kepekaannya adalah
> **isi sebuah hasil**, bukan benda yang berdiri sendiri — `LAB-DA-001` menempatkan keduanya di
> dalam `AGG-LAB-01` lewat pemeriksaan. Endpoint `POST /isolates` tersendiri akan membuat separuh
> hasil dapat tersimpan tanpa separuh lainnya, dan itu persis batas konsistensi yang dilindungi
> aggregate. Mengganti seluruh isi dalam satu panggilan juga membuatnya **idempoten** — pola yang
> sama dengan `PUT /{id}/result` pada `r21`.

**`LabMicrobiologyResultRequest`**

| Ruas | Tipe | Ketentuan |
|---|---|---|
| `microbiologyFinding` | `LabMicrobiologyFinding` | **Wajib.** `Normal` / `Positive` / `Negative`. **Status TEMUAN, bukan status lifecycle** |
| `examinedAt` | `DateTime?` | Kapan pemeriksaannya dikerjakan; bila kosong diisi waktu sekarang |
| `isolates` | `LabMicrobiologyIsolateRequest[]` | Boleh **kosong** — nol pertumbuhan adalah hasil yang sah |

**`LabMicrobiologyIsolateRequest`**

| Ruas | Tipe | Ketentuan |
|---|---|---|
| `labOrganismId` | `Guid` | **Wajib**, menunjuk `LabOrganism` yang aktif. Pengetikan bebas **ditolak** (`INV-30`) |
| `note` | `string?` | Maks 500 |
| `susceptibilities` | `LabIsolateSusceptibilityRequest[]` | Boleh kosong |

**`LabIsolateSusceptibilityRequest`**

| Ruas | Tipe | Ketentuan |
|---|---|---|
| `labAntibioticId` | `Guid` | **Wajib**, menunjuk `LabAntibiotic` yang aktif |
| `concentration` | `decimal?` | Kadar; opsional |
| `zoneDiameterMm` | `int?` | Lebar zona hambat dalam milimeter; opsional |
| `result` | `LabSusceptibilityResult` | **Wajib.** `Resistant` / `Intermediate` / `Sensitive` |

**Yang sengaja TIDAK diterima:** nama organisme, nama antibiotik, waktu pengetikan, dan
pelakunya. Nama **diturunkan server** dari data induk lalu disimpan sebagai snapshot; waktu dan
pelaku dari sesi. Menerimanya dari pemanggil berarti mengizinkan hasil menyebut kuman yang tidak
ada di daftar mana pun.

### 19.3 Endpoint — hasil Patologi Anatomi (`S4c`)

> ### ⚠ BAGIAN INI `superseded` — 2026-09-18 sore, pada hari ia disetujui
>
> Bukti lapangan `LAB-EVD-003` datang beberapa jam sesudah `r24` disetujui, dan amendment pass
> putaran 8 mengubah **tiga hal yang menjadi dasar bagian ini**:
>
> | Keputusan | Yang berubah |
> |---|---|
> | `LAB-DEC-085` | Hasil Patologi Anatomi melekat pada **order**, bukan pada pemeriksaan. Path `/lab-examinations/{id}/result/pathology` **salah alamat** |
> | `LAB-DEC-086` | Ruas hasilnya **bukan tiga kolom tetap**, melainkan nilai per parameter dari data induk — dan ada **15** parameter, bukan 3 |
> | `LAB-DEC-088` | Bertambah `Simpan Final` dan `Reopen`, dicatat sebagai fakta `FinalizedAt`/`FinalizedByUserId` |
>
> **Bagian 19.2 hasil Mikrobiologi dan bagian 19.4 data induk TIDAK terdampak** dan tetap
> `approved` — artifact itu nol membahas Mikrobiologi.
>
> **Amandemen `r25` dibutuhkan sebelum bagian ini dibangun.** Ia belum ditulis. `BE-LAB-46` dan
> `FE-LAB-25` dibekukan sampai itu selesai dan disetujui.
>
> Isi di bawah dipertahankan apa adanya sebagai jejak — **bukan** sebagai kontrak yang berlaku.

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `PUT` | `/{id}/result/pathology` | Mengisi laporan makroskopik, mikroskopik, dan kesimpulan | `LabExamination : Update` | `LabPathologyResultRequest` | `ApiResponse<LabPathologyResultResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/result/pathology` | Membaca laporan yang sudah diisi | `LabExamination : Read` | — | `ApiResponse<LabPathologyResultResponse>` | **Rencana (belum tersedia)** |

**`LabPathologyResultRequest`**

| Ruas | Tipe | Ketentuan |
|---|---|---|
| `macroscopic` | `string` | **Wajib**, maks 4000 |
| `microscopic` | `string` | **Wajib**, maks 4000 |
| `conclusion` | `string` | **Wajib**, maks 4000 |
| `examinedAt` | `DateTime?` | Bila kosong diisi waktu sekarang |

> **Ketiganya wajib, dan itu bukan pilihan desain melainkan `INV-25`** yang diturunkan langsung
> dari BR-23 aturan turunan butir 3. Laporan dengan mikroskopik terisi tetapi kesimpulan kosong
> **ditolak**, bukan disimpan sebagai draft — sebab laporan patologi tanpa kesimpulan tidak dapat
> dipakai dokter untuk memutuskan apa pun.
>
> **Gambar contoh TIDAK ada pada amandemen ini.** Ia menunggu `DEC-LAB-016`, keputusan privasi
> penyimpanan berkas klinis.

### 19.4 Endpoint — dua data induk baru (`LAB-DEC-084`)

`[Tags("Health Services / Laboratory Management / Lab Organism")]`
Base URL: `api/v1/health-services/laboratory-management/lab-organisms`

| Method | Path | Kegunaan | Hak akses | Status |
|---|---|---|---|---|
| `GET` | `/` | Daftar organisme, ber-pagination dan penyaring aktif | `LabOrganism : Read` | **Tersedia** — `BE-LAB-44` |
| `GET` | `/options` | Daftar ringkas untuk pilihan di layar | `LabOrganism : Read` | **Tersedia** — `BE-LAB-44` |
| `POST` | `/` | Menambah organisme | `LabOrganism : Create` | **Tersedia** — `BE-LAB-44` |
| `PUT` | `/{id}` | Mengubah nama atau penanda aktif | `LabOrganism : Update` | **Tersedia** — `BE-LAB-44` |

`[Tags("Health Services / Laboratory Management / Lab Antibiotic")]`
Base URL: `api/v1/health-services/laboratory-management/lab-antibiotics`

| Method | Path | Kegunaan | Hak akses | Status |
|---|---|---|---|---|
| `GET` | `/` | Daftar antibiotik | `LabAntibiotic : Read` | **Tersedia** — `BE-LAB-44` |
| `GET` | `/options` | Daftar ringkas untuk pilihan di layar | `LabAntibiotic : Read` | **Tersedia** — `BE-LAB-44` |
| `POST` | `/` | Menambah antibiotik | `LabAntibiotic : Create` | **Tersedia** — `BE-LAB-44` |
| `PUT` | `/{id}` | Mengubah nama atau penanda aktif | `LabAntibiotic : Update` | **Tersedia** — `BE-LAB-44` |

> ### Kedua kelompok endpoint ini bukan pelengkap
>
> **Nol `DELETE` disediakan**, dan itu disengaja: menghapus organisme yang sudah dipakai berarti
> menghapus temuan pasien. Penonaktifan lewat `IsActive` sudah cukup, dan `INV-31` menjamin baris
> lama tidak ikut berubah.
>
> **Keduanya bagian dari definisi selesai slice ini, bukan tambahan yang boleh menyusul.**
> `LAB-COORD-006` dan `MST-POS-WRITE` membuktikan tabel data induk tanpa endpoint tulis adalah
> kegagalan yang **sudah berulang dua kali** di modul ini — satu masih terbuka sampai hari ini,
> satu lagi harus ditambal lewat SQL langsung ke basis data.

### 19.5 Validasi

| ID | Aturan | Kode |
|---|---|---|
| `VAL-83` | Pemeriksaan wajib ada, belum terhapus, dan tidak `Voided`/`Cancelled` | `404` / `409` |
| `VAL-84` | Jalur `/microbiology` **hanya** untuk pemeriksaan berbentuk `MicrobiologyStructured`; jalur `/pathology` **hanya** untuk `AnatomicPathologyNarrative` | `422` |
| `VAL-85` | `labOrganismId` wajib menunjuk organisme yang **ada dan aktif** | `422` |
| `VAL-86` | `labAntibioticId` wajib menunjuk antibiotik yang **ada dan aktif** | `422` |
| `VAL-87` | Satu antibiotik **tidak boleh** muncul dua kali pada isolat yang sama | `422` |
| `VAL-88` | Ketiga ruas laporan Patologi Anatomi wajib terisi dan tidak boleh hanya berisi spasi | `422` |
| `VAL-89` | `examinedAt` **tidak boleh di masa depan** — aturan yang sama dengan `VAL-82` | `422` |
| `VAL-90` | `zoneDiameterMm` bila diisi wajib **lebih besar dari nol** | `422` |
| `VAL-91` | Kode organisme dan kode antibiotik **unik** pada data induknya | `409` |

> **`VAL-85` dan `VAL-86` menyebut "aktif", dan itu berlaku hanya bagi baris BARU.** Hasil lama
> yang menunjuk organisme yang kemudian dinonaktifkan **tetap sah dan tetap terbaca** — `INV-31`.
> Menegakkan "aktif" pada pembacaan akan membuat hasil pasien menghilang karena data induknya
> dirapikan, dan itu kelas kesalahan yang sama dengan mengubah batas nilai secara surut.

### 19.6 Permission yang diusulkan

| Resource | Action | Keterangan |
|---|---|---|
| `LabExamination` | `Update`, `Read` | **Dipakai ulang**, nol resource baru untuk pengisian hasil — mengikuti alasan `r21` |
| `LabOrganism` | `Read`, `Create`, `Update` | **Baru.** Nol `Delete` |
| `LabAntibiotic` | `Read`, `Create`, `Update` | **Baru.** Nol `Delete` |

### 19.7 Traceability

| Yang diusulkan | Keputusan | Invariant |
|---|---|---|
| Jalur hasil Mikrobiologi | `LAB-DEC-027` (BR-23) | `INV-24`, `INV-26`, `INV-27` |
| Jalur hasil Patologi Anatomi | `LAB-DEC-027` (BR-23) | `INV-24`, `INV-25` |
| Dua data induk beserta endpoint tulisnya | `LAB-DEC-084` | `INV-30`, `INV-31` |
| Nol status hasil | `LAB-DEC-080` | `INV-29` |
| Nol penilaian kritis otomatis | BR-23 | `INV-28` |
| Nol penanda `Definitif` | `LAB-DEC-081` | — |

---

## 20. Amandemen `r25` — Laporan Patologi Anatomi per pesanan, 2026-09-18

> ### ✅ STATUS: `approved` — **DISETUJUI 2026-09-18**
>
> **Menggantikan bagian 19.3** yang sudah ditandai `superseded`. Bagian 19.2 Mikrobiologi dan
> 19.4 data induk Mikrobiologi **tidak tersentuh** dan tetap `approved`.
>
> | Field | Nilai |
> |---|---|
> | Revision | `r25` |
> | Status | **`approved`** |
> | `approved_by` / `approved_at` | Yoga Aji Pratama (`yogaaji452@gmail.com`) / **2026-09-18** |
> | `input_revision` | decisions rev 48; `LAB-DA-001` rev 7 |
> | Dampak kompatibilitas | **Aditif terhadap yang berjalan.** `r24` bagian 19.3 belum pernah dibangun, sehingga penggantiannya nol memutus pemakai |
>
> **Label "Rencana (belum tersedia)" pada tabel endpoint di bawah TETAP berlaku** dan tidak
> berubah oleh persetujuan ini. Ia menyatakan endpointnya belum ada **di kode**, bukan bahwa
> kontraknya belum disetujui. Yang membangunnya: `BE-LAB-50` (bagian 20.3), `BE-LAB-51`, dan
> `BE-LAB-52` (bagian 20.2).

### 20.1 Batas amandemen ini

**Mengisi laporan. Bukan memvalidasi, bukan merilis, bukan mengirim.** `S4e` tertahan
`DEC-LAB-011`. **Nol status hasil** (`INV-36`). **Nol ruas `issuedAt` dan `effectiveAt` yang
dapat dikirim pemanggil** — keduanya diturunkan server (`INV-38`).

### 20.2 Endpoint — laporan Patologi Anatomi

`[Tags("Health Services / Laboratory Management / Lab Pathology Report")]`
Base URL: `api/v1/health-services/laboratory-management/lab-orders`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/{labOrderId}/pathology-report` | Membaca laporan beserta **parameter yang berlaku** bagi pesanan itu | `LabExamination : Read` | — | `ApiResponse<LabPathologyReportResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{labOrderId}/pathology-report` | Menyimpan seluruh isi laporan sekaligus | `LabExamination : Update` | `LabPathologyReportRequest` | `ApiResponse<LabPathologyReportResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{labOrderId}/pathology-report/finalize` | Menyatakan laporan selesai ditulis | `LabExamination : Update` | — | `ApiResponse<LabPathologyReportResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{labOrderId}/pathology-report/reopen` | Membuka kembali laporan yang sudah final | `LabExamination : Update` | `LabPathologyReopenRequest` | `ApiResponse<LabPathologyReportResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{labOrderId}/pathology-context` | Membaca konteks klinis pesanan | `LabOrder : Read` | — | `ApiResponse<LabPathologyOrderContextResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{labOrderId}/pathology-context` | Menulis konteks klinis | `LabOrder : Update` | `LabPathologyOrderContextRequest` | `ApiResponse<LabPathologyOrderContextResponse>` | **Rencana (belum tersedia)** |

> **Base URL-nya `lab-orders`, bukan `lab-examinations`, dan itu bukan kerapian.** `LAB-DEC-085`
> menetapkan laporan PA melekat pada **pesanan**. Jalur `/lab-examinations/{id}/result/pathology`
> pada `r24` **salah alamat**, dan itu sebab utama ia digantikan.

> **`finalize` dan `reopen` dipisahkan dari `PUT`, dan itu disengaja.** Keduanya **pernyataan
> profesional**, bukan penyimpanan. Menggabungkannya ke dalam `PUT` akan membuat seseorang dapat
> memfinalkan laporan **tanpa sadar** hanya karena mengirim satu ruas tambahan.

> **Konteks klinis memakai `LabOrder : Update`, bukan `LabExamination : Update`.** `LAB-DEC-091`
> menetapkan penulisnya **dokter pemesan**, bukan patolog — dan keduanya tidak boleh berbagi satu
> hak akses.

**`LabPathologyReportRequest`**

| Ruas | Tipe | Ketentuan |
|---|---|---|
| `findingStatus` | `LabPathologyFindingStatus?` | `Normal` / `NeedsAttention` / `Critical`. **Nilai temuan, bukan status lifecycle** |
| `analystUserId` | `Guid?` | Penanggung jawab analis |
| `values` | `LabPathologyReportValueRequest[]` | Nilai per parameter. Boleh sebagian saat menyimpan; kelengkapannya diuji saat `finalize` |

**`LabPathologyReportValueRequest`**

| Ruas | Tipe | Ketentuan |
|---|---|---|
| `labPathologyParameterId` | `Guid` | **Wajib**, dan wajib berlaku bagi kategori pesanan ini (`VAL-93`) |
| `value` | `string` | Teks tanpa batas panjang (`RULE-011`) |

**`LabPathologyReportResponse`** — selain isinya, **wajib membawa daftar parameter yang berlaku**
beserta penanda wajibnya, sehingga layar nol perlu menebak bentuk formulirnya. Ditambah dua ruas
**turunan**: `issuedAt` dari `FinalizedAt`, dan `effectiveAt` dari `LabSpecimen.CollectedAt`.

**`LabPathologyReopenRequest`** — satu ruas `reason` (`string`, **wajib**).

**Yang sengaja TIDAK diterima:** `issuedAt`, `effectiveAt`, waktu finalisasi, pelaku, dan nama
parameter. Seluruhnya **diturunkan server**. Menerimanya berarti mengizinkan laporan mengaku
terbit pada waktu yang tidak pernah terjadi.

### 20.3 Endpoint — empat data induk Patologi Anatomi

`[Tags("Health Services / Laboratory Management / Lab Pathology Master Data")]`

| Resource | Base URL | Method yang disediakan | Hak akses |
|---|---|---|---|
| Parameter | `.../lab-pathology-parameters` | `GET`, `GET /options`, `POST`, `PUT /{id}` | `LabPathologyParameter : Read/Create/Update` |
| Kategori | `.../lab-pathology-categories` | `GET`, `GET /options`, `POST`, `PUT /{id}` | `LabPathologyCategory : Read/Create/Update` |
| Keberlakuan | `.../lab-pathology-categories/{id}/parameters` | `GET`, `PUT` (mengganti seluruh daftar) | `LabPathologyCategory : Update` |
| Pemetaan pemeriksaan | `.../lab-procedure-pathology-categories` | `GET`, `POST`, `PUT /{id}`, `GET /suggestions` | `LabPathologyCategory : Read/Update` |

**Nol `DELETE` pada keempatnya.** Penonaktifan lewat `IsActive`; `INV-37` menjamin laporan lama
tidak berubah.

> **`GET /suggestions` adalah satu-satunya tempat keenam keyword artifact hidup.** Ia
> **mengusulkan** pemetaan awal dengan mencocokkan `HISTO`, `PAPSMEAR`, `LBC`, `HPV`,
> `NON GINEKOLOGI`, dan `IHK` pada nama pemeriksaan — lalu **manusia memeriksanya sebelum
> disimpan**. Sesudah pengisian awal selesai, endpoint ini boleh tidak dipakai lagi
> (`LAB-DEC-087`).

### 20.4 Validasi

| ID | Aturan | Kode |
|---|---|---|
| `VAL-92` | Pesanan wajib ada, belum dibatalkan, dan berdisiplin **Patologi Anatomi** | `404` / `422` |
| `VAL-93` | Setiap `labPathologyParameterId` wajib **berlaku bagi sekurang-kurangnya satu kategori** yang melekat pada pesanan itu | `422` |
| `VAL-94` | Satu parameter **tidak boleh** dikirim dua kali dalam satu permintaan | `422` |
| `VAL-95` | `finalize` **ditolak** selama masih ada parameter **wajib** yang kosong; jawabannya menyebut parameter mana saja | `422` |
| `VAL-96` | `PUT` **ditolak** ketika laporan sudah final; yang tersedia hanya `reopen` | `409` |
| `VAL-97` | `reopen` menuntut **alasan**; kosong atau hanya spasi ditolak | `422` |
| `VAL-98` | `reopen` **ditolak** bila laporan belum pernah difinalkan | `409` |
| `VAL-99` | Parameter atau kategori yang **dinonaktifkan** ditolak pada nilai **baru**; laporan lama tetap terbaca utuh | `422` |
| `VAL-100` | Pesanan yang **nol punya pemetaan kategori** menjawab daftar parameter **kosong**, dan `finalize` ditolak dengan sebab yang menyebutkan pemetaannya belum diatur | `422` |
| `VAL-101` | Kode parameter dan kode kategori **unik** | `409` |

> **`VAL-100` adalah keadaan yang PASTI terjadi pada hari pertama**, sebelum pemetaan diisi.
> Menjawabnya dengan daftar kosong tanpa sebab akan membuat patolog mengira sistemnya rusak.
> Pesannya wajib menyebut **apa yang belum diatur dan siapa yang mengaturnya**.

### 20.5 Permission yang diusulkan

| Resource | Action | Keterangan |
|---|---|---|
| `LabExamination` | `Read`, `Update` | **Dipakai ulang** untuk laporan PA — mengikuti alasan `r21` dan `r24` |
| `LabOrder` | `Read`, `Update` | **Dipakai ulang** untuk konteks klinis; penulisnya dokter pemesan |
| `LabPathologyParameter` | `Read`, `Create`, `Update` | **Baru.** Nol `Delete` |
| `LabPathologyCategory` | `Read`, `Create`, `Update` | **Baru.** Nol `Delete`. Mencakup keberlakuan dan pemetaan |

### 20.6 Traceability

| Yang diusulkan | Keputusan | Invariant |
|---|---|---|
| Laporan per pesanan | `LAB-DEC-085` | `INV-32` |
| Parameter berindentitas + keberlakuan | `LAB-DEC-086` | `INV-33`, `INV-34`, `INV-37` |
| Pemetaan kategori + `GET /suggestions` | `LAB-DEC-087` | `INV-39` |
| `finalize` / `reopen` sebagai fakta | `LAB-DEC-088` | `INV-35`, `INV-36` |
| Konteks klinis ber-hak akses `LabOrder` | `LAB-DEC-091` | `INV-40` |
| `issuedAt` / `effectiveAt` turunan | `LAB-DEC-092` | `INV-38` |
| `analystUserId` | `LAB-DEC-093` | — |
| `findingStatus` sebagai nilai | `LAB-DEC-094` | — |

---

## 21. Amandemen `r26` — `S4b` sesudah amendment pass putaran 9 dan 10, 2026-09-21

> ### ✅ STATUS: `approved` — 2026-09-21
>
> Disetujui **Yoga Aji Pratama** selaku pemilik modul pada 2026-09-21, bersama `LAB-VAL-v1`
> `r9` dan `LAB-PERM-v1` revision 8.
>
> **Label `Rencana (belum tersedia)` pada tabel di bawah tetap berlaku apa adanya.** Ia
> menyatakan endpointnya belum ada di kode, bukan bahwa kontraknya belum disetujui.
>
> | Field | Nilai |
> |---|---|
> | `contract_version` | `LAB-API-v1` |
> | Revision | `r26` |
> | Status | **`approved`** |
> | `approved_by` / `approved_at` | Yoga Aji Pratama (`yogaaji452@gmail.com`) / 2026-09-21 |
> | `input_revision` | decisions **rev 50** (`LAB-DEC-095`..`LAB-DEC-113`); capability map **rev 4**; `LAB-DA-001` rev 6 bagian A3 |
> | Kesiapan arsitektur domain | `DOMAIN_ARCHITECTURE_READY` untuk `S4b` |
> | Dampak kompatibilitas | **Aditif.** Nol endpoint yang sudah ada dihapus. **Satu response diperluas** — `LabMicrobiologyResultResponse` bertambah ruas turunan, dan penambahan ruas response bersifat aditif bagi pemanggil |

Menurunkan `LAB-DEC-096`, `097`, `098`, `099`, `100`, `103`, `104`, `106`, `107`, `111`, `112`,
dan `113`.

### 21.1 Batas amandemen ini

**Seluruh endpoint di bawah tetap mengisi hasil.** Nol di antaranya memvalidasi, merilis, atau
mengoreksi hasil yang sudah dirilis — ketiganya tetap `S4d` dan `S6`, dan `S4d` masih tertahan
`DEC-LAB-011`.

**Tiga batas yang berubah dari `r24`, dan ketiganya karena keputusan pemilik modul:**

| Batas `r24` | Keadaan `r26` | Dasar |
|---|---|---|
| *"Nol status hasil diperkenalkan"* | **Tetap berlaku.** `FinalizedAt` adalah **fakta**, bukan status — pola `LAB-DEC-088` | `LAB-DEC-097` |
| *"Nol penanda `Definitif`"* | **DICABUT.** `LAB-DEC-081` digantikan; maknanya sudah ditetapkan `LAB-DEC-106` | `LAB-DEC-106` |
| *"Nol penilaian kritis otomatis (`INV-28`)"* | **DICABUT untuk Mikrobiologi.** Penandanya kini dihitung dari data induk aturan, bukan dari perbandingan angka — yang justru ditolak BR-23 | `LAB-DEC-103` |

> **`INV-28` tidak salah ketika ditulis, dan itu perlu dicatat jujur.** Ia menolak penilaian
> kritis lewat **mekanisme batas nilai**, dan penolakan itu tetap benar — bakteri resisten
> bukan angka yang dapat dibandingkan dengan ambang. `LAB-DEC-103` menyediakan mekanisme
> **ketiga** yang belum ada ketika `INV-28` ditulis: pencocokan terhadap daftar kombinasi yang
> disahkan wewenang klinis. Invariant itu karena itu **dipersempit**, bukan dibatalkan.

### 21.2 Kelengkapan dan konsultasi hasil Mikrobiologi

Base URL: `api/v1/health-services/laboratory-management/lab-examinations`

`[Tags("Health Services / Laboratory Management / Lab Examination")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `POST` | `/{id}/result/microbiology/finalize` | Menyatakan penulisan hasil selesai | `LabExamination : Update` | — | `ApiResponse<LabMicrobiologyResultResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/result/microbiology/reopen` | Membuka kembali penulisan sebelum rilis | `LabExamination : Update` | `LabReopenRequest` | `ApiResponse<LabMicrobiologyResultResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}/result/microbiology/consultation` | Mencatat fakta konsultasi (penanda `Definitif`) | `LabExamination : Update` | `LabConsultationRequest` | `ApiResponse<LabMicrobiologyResultResponse>` | **Rencana (belum tersedia)** |

**`LabReopenRequest`**

| Ruas | Tipe | Ketentuan |
|---|---|---|
| `reason` | `string` | **Wajib**, maks 500. Alasan membuka kembali |

**`LabConsultationRequest`**

| Ruas | Tipe | Ketentuan |
|---|---|---|
| `consultedToName` | `string` | **Wajib**, maks 200. Kepada siapa dikonsultasikan |
| `consultedAt` | `DateTime` | **Wajib.** Tidak boleh masa depan (`VAL-108`) |

**Yang sengaja TIDAK diterima:** `consultedByUserId` dan `finalizedByUserId`. Keduanya diturunkan
dari sesi. Menerimanya dari pemanggil berarti mengizinkan seseorang mencatat konsultasi atas nama
orang lain — alasan yang sama dipakai `LAB-DEC-105` menolak ruas `Analis` yang dapat dipilih.

> **Kenapa konsultasi memakai `PUT` tersendiri, bukan ikut `PUT /result/microbiology`.**
> Mencatat konsultasi dan mengisi hasil adalah dua tindakan pada waktu berbeda: hasil diisi
> analis hari ini, konsultasi dicatat sesudah berbicara dengan konsultan besok. Menggabungkannya
> memaksa pemanggil mengirim ulang seluruh isolat hanya untuk menambah satu tanggal — dan setiap
> pengiriman ulang adalah kesempatan isolat tertimpa.

> **Kenapa `finalize` tidak menerima apa pun.** Ia menyatakan bahwa penulisnya selesai, dan
> seluruh isinya sudah tersimpan lewat `PUT /result/microbiology`. Menerima badan permintaan
> akan membuat dua jalur menulis hasil yang sama.

### 21.3 Ruas turunan pada pembacaan hasil Mikrobiologi

`LabMicrobiologyResultResponse` **bertambah** ruas berikut. Seluruhnya **baca-saja** dan
**tidak** diterima pada request mana pun.

| Ruas | Tipe | Diturunkan dari | Dasar |
|---|---|---|---|
| `effectiveAt` | `DateTime?` | `LabSpecimen.CollectedAt` pada specimen pemeriksaan itu | `LAB-DEC-096` |
| `issuedAt` | `DateTime?` | `LabExamination.FinalizedAt` | `LAB-DEC-096` |
| `analystName` | `string?` | `ResultEnteredByUserId` | `LAB-DEC-105` |
| `isFinalized` | `bool` | `FinalizedAt != null` | `LAB-DEC-097` |
| `reopenCount` | `int` | Kolomnya | `LAB-DEC-097` |
| `isConsulted` | `bool` | `ConsultedAt != null` | `LAB-DEC-106` |
| `criticalRuleAvailable` | `bool` | Ada tidaknya baris `LabMicrobiologyCriticalRule` yang aktif | `LAB-DEC-103` butir 5 |
| `susceptibilities[].isCritical` | `bool` | Pencocokan terhadap aturan kritis **saat dibaca** | `LAB-DEC-103` |

> **`criticalRuleAvailable` bukan ruas hiasan.** `LAB-DEC-103` butir 5 mewajibkan layar
> menyatakan secara terbaca ketika aturan kritis masih kosong — bukan diam. Tanpa ruas ini,
> layar tidak punya cara membedakan *"tidak ada yang kritis"* dari *"belum ada aturannya"*, dan
> keduanya terlihat persis sama: nol penanda menyala.

> **`isCritical` dihitung saat dibaca, bukan disimpan.** Aturan kritis dapat berubah; nilai
> tersimpan akan membekukan penilaian lama sebagai kalau-kalau fakta. Konsekuensinya diterima:
> hasil yang dibaca ulang tahun depan dinilai dengan aturan tahun depan.

### 21.4 Koreksi Informasi Specimen dari halaman hasil

Base URL: `api/v1/health-services/laboratory-management/lab-specimens`

`[Tags("Health Services / Laboratory Management / Lab Specimen")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `PATCH` | `/{id}/correction` | Mengoreksi dan melengkapi ruas specimen dari halaman hasil | `LabSpecimen : Update` | `LabSpecimenCorrectionRequest` | `ApiResponse<LabSpecimenResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/field-changes` | Membaca jejak perubahan ruas specimen | `LabSpecimen : Read` | `ApiResponse<LabFieldChangeResponse[]>` | **Rencana (belum tersedia)** |

**`LabSpecimenCorrectionRequest`** — seluruh ruas **opsional**; yang tidak dikirim tidak diubah.

| Ruas | Tipe | Ketentuan |
|---|---|---|
| `specimenTypeId` | `Guid?` | Menunjuk `LabSpecimenType` yang aktif |
| `specimenTypeOtherNote` | `string?` | **Wajib** bila jenis yang dipilih bertanda `IsOtherBucket` (`VAL-104`) |
| `detailTypeIds` | `Guid[]?` | Spesifik Specimen; **menggantikan seluruh** pilihan yang ada |
| `volumeAmount` | `decimal?` | Angka volume |
| `volumeUnitId` | `Guid?` | Menunjuk `MstMeasurement` bertanda `IsForLaboratory` |
| `specimenDescription` | `string?` | Keterangan operasional bebas, maks 500 |
| `physicallyReceivedAt` | `DateTime?` | Waktu nyata penerimaan; aturan BR-37 tetap berlaku |

> **Kenapa `PATCH`, bukan `PUT`.** Halaman hasil mengoreksi **sebagian** — biasanya satu ruas
> yang keliru. `PUT` menuntut pemanggil mengirim seluruh isi specimen, dan ruas yang lupa
> disertakan akan terhapus diam-diam. Itu risiko yang tidak sebanding untuk memperbaiki satu
> salah pilih.

> **Kenapa endpoint tersendiri, bukan menambah `PUT` pada `LabSpecimenController`.** Controller
> itu hari ini murni berisi **aksi siklus hidup** — `collect`, `receive`, `accept`, `reject`,
> `hold`, `resume`, `cancel`. Koreksi ruas bersumbu berbeda, dan `/correction` membuat
> perbedaannya terbaca dari path-nya sendiri. Alasan yang sama dipakai `LAB-DEC-112` memisahkan
> jejaknya dari `LabTransitionHistory`.

**`LabFieldChangeResponse`**

| Ruas | Tipe | Keterangan |
|---|---|---|
| `fieldName` | `string` | Nama ruas yang berubah |
| `fieldLabel` | `string` | Nama ruas dalam Bahasa Indonesia untuk layar |
| `oldValue` | `string?` | Nilai lama, sudah dibaca-manusiakan |
| `newValue` | `string?` | Nilai baru |
| `changedByName` | `string` | Nama pelaku |
| `changedAt` | `DateTime` | Waktu |

### 21.5 Data induk Spesifik Specimen

Base URL: `api/v1/health-services/laboratory-management/lab-specimen-detail-types`

`[Tags("Health Services / Laboratory Management / Lab Specimen Detail Type")]`

Mengikuti **master-data-endpoint-standard** yang sama dengan `LabSpecimenType` dan `LabOrganism`.

| Method | Path | Kegunaan | Hak akses | Status |
|---|---|---|---|---|
| `GET` | `/filters/metadata` | Metadata penyaring | `LabSpecimenDetailType : Read` | **Rencana (belum tersedia)** |
| `GET` | `/summary` | Ringkasan | `LabSpecimenDetailType : Read` | **Rencana (belum tersedia)** |
| `GET` | `/` | Daftar berhalaman | `LabSpecimenDetailType : Read` | **Rencana (belum tersedia)** |
| `GET` | `/options` | Pilihan untuk layar, **disaring `specimenTypeId`** | `LabSpecimenDetailType : Read` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Satu baris | `LabSpecimenDetailType : Read` | **Rencana (belum tersedia)** |
| `POST` | `/` | Menambah | `LabSpecimenDetailType : Create` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Mengubah | `LabSpecimenDetailType : Update` | **Rencana (belum tersedia)** |
| `DELETE` | `/{id}` | Menonaktifkan | `LabSpecimenDetailType : Delete` | **Rencana (belum tersedia)** |

> **Nol endpoint yang membuat baris baru dari halaman hasil.** `LAB-DEC-098` butir 5 menegakkan
> `LAB-DEC-040`: petugas memakai jalan keluar `Lainnya` beserta keterangannya, dan hanya kepala
> instalasi yang menaikkannya menjadi nilai tetap lewat `POST` di atas. Menyediakan jalan pintas
> dari halaman hasil akan mengubah tata kelolanya diam-diam.

### 21.6 Data induk aturan kritis Mikrobiologi

Base URL: `api/v1/health-services/laboratory-management/lab-microbiology-critical-rules`

`[Tags("Health Services / Laboratory Management / Lab Microbiology Critical Rule")]`

| Method | Path | Kegunaan | Hak akses | Status |
|---|---|---|---|---|
| `GET` | `/` | Daftar berhalaman | `LabMicrobiologyCriticalRule : Read` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Satu baris | `LabMicrobiologyCriticalRule : Read` | **Rencana (belum tersedia)** |
| `POST` | `/` | Menambah aturan | `LabMicrobiologyCriticalRule : Create` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Mengubah aturan | `LabMicrobiologyCriticalRule : Update` | **Rencana (belum tersedia)** |
| `DELETE` | `/{id}` | Menonaktifkan aturan | `LabMicrobiologyCriticalRule : Delete` | **Rencana (belum tersedia)** |

**`LabMicrobiologyCriticalRuleRequest`**

| Ruas | Tipe | Ketentuan |
|---|---|---|
| `labOrganismId` | `Guid?` | Kosong berarti **kuman apa saja** |
| `labAntibioticId` | `Guid?` | Kosong berarti **antibiotik apa saja** |
| `susceptibilityResult` | `LabSusceptibilityResult?` | Kosong berarti **hasil apa saja** |
| `ruleNote` | `string?` | Maks 500. Alasan klinis aturan ini |

> **Hak akses ketiga tindakan tulis dipegang wewenang klinis Mikrobiologi (`DR-LAB-002`), bukan
> kepala instalasi.** Ini berbeda dari `LabOrganism` dan `LabAntibiotic` yang memang urusan
> panel uji. Menetapkan kombinasi mana yang membahayakan pasien adalah **penilaian klinis**, dan
> `LAB-DEC-103` butir 4 menyerahkannya secara tegas.

### 21.7 Pembacaan dokter konfirmator

Base URL: `api/v1/health-services/laboratory-management/lab-examinations`

| Method | Path | Kegunaan | Hak akses | Response | Status |
|---|---|---|---|---|---|
| `GET` | `/{id}/confirming-doctor-options` | Pilihan dokter konfirmator beserta sumbernya | `LabExamination : Read` | `ApiResponse<LabConfirmingDoctorOptionsResponse>` | **Rencana (belum tersedia)** |

**`LabConfirmingDoctorOptionsResponse`**

| Ruas | Tipe | Keterangan |
|---|---|---|
| `attendingDoctor` | `LabDoctorOption?` | DPJP dari pesanan |
| `onDutyDoctors` | `LabDoctorOption[]` | Dari `TrxOnCallAssignment` yang aktif pada waktu permintaan |
| `onDutyScheduleAvailable` | `bool` | Bernilai salah ketika `onDutyDoctors` kosong karena jadwal jaga belum terisi |
| `fallbackDoctors` | `LabDoctorOption[]` | Daftar dokter aktif yang dapat dicari; **hanya terisi** ketika `onDutyScheduleAvailable` bernilai salah |

`LabDoctorOption` memuat `doctorId`, `fullName`, dan `whatsAppNumber` — ketiganya dibaca dari
`MstDoctor`, nol disalin ke tabel Laboratorium.

> **`onDutyScheduleAvailable` adalah ruas yang membuat `LAB-DEC-111` dapat diuji.** Tanpa ia,
> layar tidak dapat membedakan *"malam ini memang tidak ada dokter jaga"* dari *"jadwal jaganya
> belum pernah diisi siapa pun"* — dan `TrxOnCallAssignment` hari ini nol punya endpoint
> pengisi (`LAB-COORD-014`), sehingga keadaan kedua itulah yang pasti terjadi lebih dulu.
> `AC-174` menguji ruas ini secara langsung.

### 21.8 Yang TIDAK ditambahkan amandemen ini

| Yang ditolak | Alasan |
|---|---|
| Ruas dan endpoint `HL7` | `LAB-DEC-109` |
| Endpoint validasi dan rilis Mikrobiologi | `S4d`, tertahan `DEC-LAB-011` |
| Endpoint pengiriman hasil dan cetak dwibahasa | `LAB-COORD-011`, `LAB-COORD-013` |
| Endpoint tulis `TrxOnCallAssignment` | Milik `human-resource` (`LAB-COORD-014`) |
| Endpoint tulis `MstMeasurement` | `MeasurementController` sudah menyediakan `POST`/`PUT`/`DELETE`; satuan baru `LAB-DEC-100` cukup lewat data induk awal |

### 21.9 Traceability `r26`

| Yang berubah | Keputusan | Invariant |
|---|---|---|
| `finalize` / `reopen` Mikrobiologi sebagai fakta | `LAB-DEC-097` | Menutup `ARCH-GAP-LAB-04` |
| `issuedAt` / `effectiveAt` turunan | `LAB-DEC-096` | — |
| `PUT /consultation` dan penanda `Definitif` | `LAB-DEC-106` | Menggantikan `LAB-DEC-081` |
| `isCritical` dan `criticalRuleAvailable` | `LAB-DEC-103` | Mempersempit `INV-28` |
| `PATCH /correction` dan `GET /field-changes` | `LAB-DEC-107`, `LAB-DEC-112` | — |
| Data induk `LabSpecimenDetailType` | `LAB-DEC-098`, `LAB-DEC-099` | Menegakkan `LAB-DEC-040` |
| `analystName` turunan | `LAB-DEC-105` | — |
| `confirming-doctor-options` beserta jalur jatuhnya | `LAB-DEC-111` | — |

---

## 22. Amandemen `r27` — `S4b` sesudah bukti cetak, 2026-09-21

> ### ✅ STATUS: `approved` — 2026-09-21
>
> Disetujui **Yoga Aji Pratama** selaku pemilik modul pada 2026-09-21, bersama `LAB-VAL-v1`
> `r10` dan `LAB-PERM-v1` revision 9. **Persetujuan ini mencakup perubahan yang TIDAK
> aditif** pada butir 10 bagian 22.1 — `result` turun dari wajib menjadi opsional.
>
> | Field | Nilai |
> |---|---|
> | `contract_version` | `LAB-API-v1` |
> | Revision | `r27` |
> | Status | **`approved`** |
> | `approved_by` / `approved_at` | Yoga Aji Pratama (`yogaaji452@gmail.com`) / 2026-09-21 |
> | `input_revision` | decisions **rev 52** (`LAB-DEC-114`..`LAB-DEC-128`); `LAB-EVD-005`, `LAB-EVD-006` |
> | Dampak kompatibilitas | **Sebagian TIDAK aditif.** Satu ruas request berubah sifat: `result` pada baris kepekaan turun dari **wajib** menjadi **opsional** karena kini dihitung server |
>
> ⚠ **`r26` berumur beberapa jam ketika amandemen ini disusun.** Nol endpoint `r26` dihapus,
> tetapi **dua belas** perubahan menumpuk di atasnya. Siapa pun yang membaca `r26` sendirian
> akan salah.

### 22.1 Yang berubah dari `r26`, diringkas

| # | Perubahan | Keputusan | Sifat |
|---|---|---|---|
| 1 | `resultQualifier` — `Definitif`/`Sementara` | `LAB-DEC-114` | Aditif |
| 2 | `concentrationUnitId` pada baris kepekaan | `LAB-DEC-115` | Aditif |
| 3 | `cultureType` — Bakteri/Jamur | `LAB-DEC-116`, `124` | Aditif |
| 4 | `susceptibilityMethod` — Difusi/Dilusi | `LAB-DEC-124` | Aditif |
| 5 | `labReportNumber` pada pembacaan order | `LAB-DEC-117` | Aditif |
| 6 | Pemetaan tanggal cetak | `LAB-DEC-118` | Nol perubahan kontrak |
| 7 | Data induk pengaturan disiplin | `LAB-DEC-119`, `127` | Aditif — endpoint baru |
| 8 | `Petugas Otorisasi` dari perilis | `LAB-DEC-120` | Aditif, **tampil kosong sampai `S4d`** |
| 9 | `discContentUg` dan rentang breakpoint | `LAB-DEC-122` | Aditif — endpoint baru |
| 10 | **`result` turun dari wajib menjadi opsional** | `LAB-DEC-123` | **TIDAK aditif** |
| 11 | Profil Mikrobiologi pada katalog | `LAB-DEC-125` | Aditif — endpoint baru |
| 12 | `isSusceptibilityTested` pada isolat | `LAB-DEC-126` | Aditif |

### 22.2 `LabMicrobiologyResultRequest` — ruas yang ditambahkan

| Ruas | Tipe | Ketentuan |
|---|---|---|
| `resultQualifier` | `LabResultQualifier?` | `Definitif` / `Sementara`. Dicetak pada baris `HASIL YANG DIPEROLEH` |
| `cultureType` | `LabCultureType?` | `Bacterial` / `Fungal`. Menentukan **kata pada label cetak** |
| `susceptibilityMethod` | `LabSusceptibilityMethod?` | `DiscDiffusion` / `Dilution`. Menentukan **kolom mana yang berlaku** |

Ketiganya **opsional** — lihat 22.8.

**`LabMicrobiologyIsolateRequest` bertambah:**

| Ruas | Tipe | Ketentuan |
|---|---|---|
| `isSusceptibilityTested` | `bool` | Berdefault benar. Bernilai salah untuk kuman yang ditemukan tetapi tidak diuji (`LAB-DEC-126`) |

**`LabIsolateSusceptibilityRequest` — bentuk barunya:**

| Ruas | Tipe | Ketentuan |
|---|---|---|
| `labAntibioticId` | `Guid` | **Wajib** |
| `concentration` | `decimal?` | Nilai MIC. Hanya bermakna pada metode **dilusi** |
| `concentrationUnitId` | `Guid?` | **Wajib bila `concentration` terisi** (`VAL-112`). Menunjuk `MstMeasurement` |
| `zoneDiameterMm` | `int?` | Hanya bermakna pada metode **difusi**. **`0` adalah nilai sah**; kosong berarti belum diukur (`LAB-DEC-128`) |
| `result` | `LabSusceptibilityResult?` | **Turun dari wajib menjadi opsional.** Diisi server bila breakpoint tersedia |
| `resultOverrideReason` | `string?` | **Wajib bila `result` dikirim DAN berbeda dari hitungan server** (`VAL-113`) |

**Yang TIDAK diterima dari pemanggil:** `discContentUg`, rentang breakpoint, dan
`computedResult`. Ketiganya **diturunkan server** dari data induk lalu disimpan sebagai
snapshot. Menerimanya berarti mengizinkan pemanggil menyebut breakpoint yang tidak pernah
disahkan siapa pun.

> **Kenapa `result` tidak dihapus sama sekali dari request.** Ketika breakpoint untuk kombinasi
> organisme dan antibiotik itu **belum terisi**, server nol punya dasar menghitung. Pada
> keadaan itu ruasnya kembali menjadi satu-satunya sumber, dan `VAL-114` mewajibkannya.
> Ini keadaan yang **pasti terjadi lebih dulu**, sama seperti jadwal jaga pada `LAB-DEC-111`.

### 22.3 Ruas turunan yang ditambahkan pada pembacaan

`LabMicrobiologyResultResponse` bertambah, seluruhnya **baca-saja**:

| Ruas | Diturunkan dari | Dasar |
|---|---|---|
| `labReportNumber` | Nomor cetak per disiplin per tahun pada order | `LAB-DEC-117` |
| `printReceivedAt` | Waktu penerimaan fisik specimen | `LAB-DEC-118` |
| `printCompletedAt` | `FinalizedAt` | `LAB-DEC-118` |
| `consultantLabel`, `consultantName` | `LabDisciplineSetting` | `LAB-DEC-119` |
| `standingNote` | `LabDisciplineSetting` | `LAB-DEC-127` |
| `authorizingOfficerName` | **Perilis hasil.** Kosong selama belum dirilis | `LAB-DEC-120` |
| `validatedByName` | Pemvalidasi. Kosong selama belum divalidasi | `LAB-DEC-120` |
| `usesSusceptibilitySet` | `LabProcedureMicrobiologyProfile` | `LAB-DEC-125` |
| `breakpointAvailable` | Ada tidaknya breakpoint untuk kombinasi pada hasil ini | `LAB-DEC-123` |
| `susceptibilities[].discContentUg` | Snapshot | `LAB-DEC-122` |
| `susceptibilities[].breakpointLowerMm`, `...UpperMm` | Snapshot | `LAB-DEC-122` |
| `susceptibilities[].computedResult` | Hitungan saat disimpan | `LAB-DEC-123` |
| `susceptibilities[].isResultOverridden` | `computedResult != result` | `LAB-DEC-123` |

> **`breakpointAvailable` sekerabat dengan `criticalRuleAvailable` pada `r26`, dan alasannya
> sama.** Tanpa ia, layar tidak dapat membedakan *"interpretasinya memang perlu diketik"* dari
> *"sistem gagal menghitung"*. Keduanya terlihat persis sama: ruas kosong.

### 22.4 Aturan perhitungan interpretasi

Server menghitung `computedResult` dari `zoneDiameterMm` terhadap rentang breakpoint:

| Keadaan | Hasil |
|---|---|
| `zone < lowerMm` | `Resistant` |
| `lowerMm <= zone <= upperMm` | `Intermediate` |
| `zone > upperMm` | `Sensitive` |
| `zone` kosong **atau** breakpoint tidak tersedia | **Nol dihitung** — `result` wajib dikirim |

Contoh dari `LAB-EVD-006`: zona `13` pada rentang `12 - 15` menghasilkan `Intermediate`; zona
`11` pada `12 - 16` menghasilkan `Resistant`; zona `32` pada `13 - 16` menghasilkan
`Sensitive`; zona `0` pada `12 - 15` menghasilkan `Resistant`.

### 22.5 Data induk breakpoint

Base URL: `api/v1/health-services/laboratory-management/lab-susceptibility-breakpoints`

`[Tags("Health Services / Laboratory Management / Lab Susceptibility Breakpoint")]`

| Method | Path | Kegunaan | Hak akses | Status |
|---|---|---|---|---|
| `GET` | `/` | Daftar berhalaman, dapat disaring organisme dan antibiotik | `LabSusceptibilityBreakpoint : Read` | **Tersedia** — `BE-LAB-60` |
| `GET` | `/{id}` | Satu baris | `LabSusceptibilityBreakpoint : Read` | **Tersedia** — `BE-LAB-60` |
| `POST` | `/` | Menambah | `LabSusceptibilityBreakpoint : Create` | **Tersedia** — `BE-LAB-60` |
| `PUT` | `/{id}` | Mengubah | `LabSusceptibilityBreakpoint : Update` | **Tersedia** — `BE-LAB-60` |
| `DELETE` | `/{id}` | Menonaktifkan | `LabSusceptibilityBreakpoint : Delete` | **Tersedia** — `BE-LAB-60` |

Ruas: `labOrganismId` dan `labAntibioticId` **wajib**, `lowerMm` dan `upperMm` **wajib** dengan
`lowerMm <= upperMm` (`VAL-115`), `guidelineVersion` opsional.

**Hak tulis dipegang wewenang klinis Mikrobiologi (`DR-LAB-002`)**, sama seperti aturan kritis
`r26` bagian 21.6 dan atas alasan yang sama: angka breakpoint menentukan pasien mendapat
antibiotik yang benar.

### 22.6 Data induk profil Mikrobiologi katalog

Base URL: `api/v1/health-services/laboratory-management/lab-procedure-microbiology-profiles`

`[Tags("Health Services / Laboratory Management / Lab Procedure Microbiology Profile")]`

Lima endpoint bergaya sama. Ruas: `procedureId` **wajib dan unik**, `usesSusceptibilitySet`
**wajib**, `defaultCultureType` dan `defaultSusceptibilityMethod` opsional. Hak tulis pada
**kepala instalasi**.

> Mengikuti pola `LabProcedurePathologyCategory` yang sudah berdiri. **Nol kolom ditambahkan ke
> `MstProcedure`** — lihat alasannya pada ERD amandemen kedua.

### 22.7 Data induk pengaturan disiplin

Base URL: `api/v1/health-services/laboratory-management/lab-discipline-settings`

Empat endpoint: `GET /`, `GET /{discipline}`, `PUT /{discipline}`, dan `GET /options`. **Nol
`POST` dan nol `DELETE`** — barisnya tetap tiga, satu per disiplin, dan hanya isinya yang
berubah. Hak tulis pada **kepala instalasi**.

Ruas: `consultantLabel` **wajib**, `consultantName` opsional, `standingNote` opsional,
`reportNumberPrefix` opsional.

### 22.8 Kenapa tiga ruas baru dibuat OPSIONAL, bukan wajib

`resultQualifier`, `cultureType`, dan `susceptibilityMethod` seluruhnya **boleh kosong**, dan
itu keputusan sadar.

> `LAB-OPEN-039` masih menyisakan **enam varian cetak yang belum pernah dilihat**, dan
> `LAB-DEC-116` sudah sekali terkoreksi kurang dari satu jam sesudah dicatat justru karena
> disimpulkan dari satu contoh. Mewajibkan ketiganya sekarang berarti menutup kemungkinan
> bentuk kelima yang belum terlihat — dan bila ia muncul, kolom wajib jauh lebih mahal
> dibongkar daripada nilai enum ditambah.
>
> Kewajibannya dinaikkan **sesudah** keenam varian diterima, lewat amandemen tersendiri.

### 22.9 Yang TIDAK ditambahkan amandemen ini

| Yang ditolak | Alasan |
|---|---|
| Endpoint validasi dan rilis | `S4d`, tertahan `DEC-LAB-011` |
| Endpoint cetak itu sendiri | `LAB-COORD-011` — pembangkit PDF nol pada platform |
| Ruas `HL7` | `LAB-DEC-109` |
| Susunan cetak dua isolat berantibiogram | Belum pernah terlihat — `LAB-OPEN-039` |
| Kalimat untuk hasil nol pertumbuhan | Belum pernah terlihat — `LAB-OPEN-039` |

### 22.10 Traceability `r27`

| Yang berubah | Keputusan | AC |
|---|---|---|
| `resultQualifier` | `LAB-DEC-114` | `AC-177` |
| `concentrationUnitId` | `LAB-DEC-115` | `AC-178` |
| `cultureType` + `susceptibilityMethod` | `LAB-DEC-116`, `124` | `AC-179`, `AC-188` |
| `labReportNumber` | `LAB-DEC-117` | `AC-180` |
| Pemetaan tanggal cetak | `LAB-DEC-118` | `AC-181` |
| `LabDisciplineSetting` | `LAB-DEC-119`, `127` | `AC-182` |
| `authorizingOfficerName` | `LAB-DEC-120` | `AC-183` |
| `discContentUg` + breakpoint snapshot | `LAB-DEC-122` | `AC-185` |
| Interpretasi terhitung + penimpaan | `LAB-DEC-123` | `AC-186`, `AC-187` |
| Profil Mikrobiologi katalog | `LAB-DEC-125` | `AC-189` |
| `isSusceptibilityTested` | `LAB-DEC-126` | `AC-190` |
| Zona `0` sah | `LAB-DEC-128` | `AC-191` |

---

## 23. Amandemen `r28` — Data induk specimen dari `LAB-EVD-007`, 2026-09-21

> ### ✅ STATUS: `approved` — 2026-09-21
>
> Disetujui **Yoga Aji Pratama** selaku pemilik modul pada 2026-09-21. **Aditif penuh** —
> nol endpoint berubah, dan `LabSpecimenType` hanya bertambah isinya.
>
> | Field | Nilai |
> |---|---|
> | `contract_version` | `LAB-API-v1` |
> | Revision | `r28` |
> | Status | **`approved`** |
> | `approved_by` / `approved_at` | Yoga Aji Pratama (`yogaaji452@gmail.com`) / 2026-09-21 |
> | `input_revision` | decisions **rev 53** (`LAB-DEC-129`..`LAB-DEC-132`); `LAB-EVD-007` |
> | Dampak kompatibilitas | **Aditif.** Nol endpoint berubah; tiga ruas response bertambah, dan daftar nilai `LabSpecimenType` bertambah dari 7 menjadi 31 |

### 23.1 Yang berubah dari `r27`

| # | Perubahan | Keputusan |
|---|---|---|
| 1 | `LabSpecimenType` berisi **31** nilai, bukan 7 | `LAB-DEC-129` |
| 2 | `LabSpecimenDetailType` bertambah `subTypeName` | `LAB-DEC-129` |
| 3 | `LabSpecimenDetailType` bertambah `snomedCode` | `LAB-DEC-132` |
| 4 | Nama Indonesia **boleh kosong**; Inggris tampil sementara | `LAB-DEC-131` |
| 5 | Penyaring **belum diterjemahkan** pada daftar data induk | `LAB-DEC-131` |
| 6 | 166 baris ter-seed **nonaktif** | `LAB-DEC-130` |

### 23.2 `LabSpecimenDetailType` — bentuk akhirnya

| Ruas | Tipe | Ketentuan |
|---|---|---|
| `labSpecimenTypeId` | `Guid` | **Wajib.** Induknya, salah satu dari 31 kelompok |
| `detailTypeCode` | `string` | **Wajib**, dinormalkan huruf kapital, unik parsial (`VAL-91`) |
| `detailTypeNameId` | `string?` | Nama Indonesia. **Boleh kosong** (`LAB-DEC-131`) |
| `detailTypeNameEn` | `string` | **Wajib.** Nama SNOMED CT berbahasa Inggris |
| `subTypeName` | `string?` | `subjenis_specimen` — **atribut pengelompokan**, bukan tingkat pilihan |
| `snomedCode` | `string?` | Kode SNOMED CT. **Kosong** untuk baris yang ditambahkan lewat `Lainnya` |
| `sortOrder` | `int` | Urutan tampil |
| `isActive` | `bool` | Ke-166 baris berkonfidensi `Rendah` ter-seed bernilai **salah** |

> **`detailTypeNameEn` wajib, `detailTypeNameId` boleh kosong — dan urutannya sengaja
> begitu.** Seluruh 1.767 baris punya nama Inggris hari ini; nol punya nama Indonesia.
> Mewajibkan yang belum ada berarti seeder gagal pada baris pertama.

**Aturan tampil:** layar menampilkan `detailTypeNameId` bila terisi, `detailTypeNameEn` bila
belum. Pencarian dan pemeriksaan duplikasi membandingkan **keduanya** (`RULE-007`).

### 23.3 Penyaring yang ditambahkan

`GET /lab-specimen-detail-types` bertambah:

| Penyaring | Kegunaan |
|---|---|
| `labSpecimenTypeId` | Menyaring per kelompok |
| `subTypeName` | Menyaring per subjenis, untuk pelaporan |
| `untranslatedOnly` | **Hanya baris yang `detailTypeNameId`-nya kosong** |
| `includeInactive` | Menyertakan ke-166 baris nonaktif |

> **`untranslatedOnly` bukan penyaring hiasan.** `LAB-DEC-131` mewajibkan ada **cara melihat
> mana yang belum diterjemahkan**. Tanpa ruas ini, pekerjaan menerjemahkan 1.767 baris tidak
> punya cara diketahui kemajuannya, dan ia akan berhenti di tengah tanpa ada yang menyadari.

`GET /lab-specimen-detail-types/options` **hanya mengembalikan baris aktif**, disaring
`labSpecimenTypeId`.

### 23.4 Ruas yang ditambahkan pada pembacaan specimen

`LabSpecimenResponse.details[]` bertambah `subTypeName` dan `snomedCode`, keduanya baca-saja
dan diturunkan dari data induk lewat snapshot nama yang sudah ada.

### 23.5 Yang TIDAK berubah

| Hal | Alasan |
|---|---|
| Endpoint tulis `LabSpecimenDetailType` | `r27` bagian 21.5 tetap berlaku apa adanya |
| Tata kelola `Lainnya` | `LAB-DEC-098` butir 5 utuh — hanya kepala instalasi yang menaikkan nilai tetap |
| `LabSpecimenType` sebagai tabel | Hanya **isinya** yang bertambah dari 7 ke 31 |

### 23.6 Traceability `r28`

| Yang berubah | Keputusan | AC |
|---|---|---|
| 31 nilai `LabSpecimenType` | `LAB-DEC-129` | `AC-192` |
| `subTypeName` | `LAB-DEC-129` | `AC-192` |
| Seed 1.601 aktif + 166 nonaktif | `LAB-DEC-130` | `AC-193` |
| Nama dua bahasa + `untranslatedOnly` | `LAB-DEC-131` | `AC-194` |
| `snomedCode` | `LAB-DEC-132` | `AC-195` |

---

## 24. Amandemen `r29` — Bentuk nomor cetak per disiplin, 2026-09-22

> ### ✅ STATUS: `approved` — 2026-09-22
>
> | Butir | Isi |
> |---|---|
> | Status | **`approved`** |
> | `approved_by` / `approved_at` | Yoga Aji Pratama (`yogaaji452@gmail.com`) / 2026-09-22 |
> | Menutup | `LAB-OPEN-043` |
> | Sifat | **ADITIF.** Dua kolom ditambahkan; nol ruas dicabut, nol ruas berubah arti |
> | Dilaksanakan | Migration `20260922064824_AddLabReportNumberShape`, diterapkan hari yang sama |

### 24.1 Kenapa amandemen ini ada

`r27` bagian 22.7 menyetujui **satu** ruas `reportNumberPrefix`. Bukti cetak `LAB-EVD-005`
memperlihatkan bahwa satu ruas awalan **tidak cukup**:

| Disiplin | Bukti | Awalan | Pemisah | Lebar urut |
|---|---|---|---|---|
| Mikrobiologi | `26-1129` | — | `-` | 4 |
| Patologi Anatomi | `26.0919` | — | `.` | 4 |
| Patologi Klinik | `25039254` | — | **kosong** | **6** |

`BE-LAB-63` membangunnya dengan `-` dan empat digit bagi ketiganya — **cocok Mikrobiologi
saja** — lalu mengangkat selisihnya sebagai `LAB-OPEN-043` alih-alih menutupnya diam-diam.

### 24.2 `LabDisciplineSetting` bertambah dua kolom

| Ruas | Tipe | Keterangan |
|---|---|---|
| `reportNumberSeparator` | `string?`, maks 5 | Pemisah tahun dan nomor urut. **Teks kosong = sengaja tanpa pemisah**; `null` = belum pernah disetel, jatuh ke `-` |
| `reportNumberLength` | `int`, 1..12, default 4 | Lebar minimum nomor urut, diisi nol di depan |

**`null` dan teks kosong sengaja DIBEDAKAN**, dan itu satu-satunya kehalusan pada amandemen
ini. Patologi Klinik memang menempelkan tahun langsung pada nomornya, sehingga "tanpa
pemisah" adalah jawaban yang sah — bukan pertanyaan yang belum dijawab. Karena itu
`LabDisciplineSettingService` **tidak** menormalkan ruas ini menjadi `null` seperti ruas teks
lainnya.

### 24.3 Satu ruas baca-saja ditambahkan pada respons

| Ruas | Isi |
|---|---|
| `reportNumberExample` | Contoh nomor yang dihasilkan bentuk ini pada tahun berjalan — `26-0001`, `26.0001`, `26000001` |

Ia ada supaya kepala instalasi melihat akibat setelannya **sebelum** lembar pertama tercetak,
bukan sesudah.

### 24.4 Kenapa dua kolom, bukan satu template

Jalan `ReportNumberFormat` berisi `{yy}.{0000}` sempat dinilai dan **ditolak**: yang dibutuhkan
hanya dua hal dan keduanya berhingga — pemisah dan lebar. Kolom bertipe tegas dapat
divalidasi saat disimpan (`Range(1,12)`, `MaxLength(5)`); template teks bebas hanya gagal
ketika lembarnya sudah tercetak dan berada di tangan pasien.

### 24.5 AKIBAT YANG WAJIB DIBACA — mengubah bentuk MENGULANG penghitung

Nomor urut dihitung dengan mencocokkan **awalan tetap** nomor (`prefix` + dua digit tahun +
pemisah). Mengubah pemisah atau awalan berarti awalan tetapnya berubah, sehingga pencarian
nomor tertinggi **nol menemukan nomor lama** dan urutannya mulai dari satu lagi.

Terbukti saat pengujian: Patologi Anatomi sudah memiliki `26-0001`; sesudah pemisahnya diubah
menjadi `.`, pesanan berikutnya memperoleh **`26.0001`**, bukan `26.0002`.

> **Ini bukan cacat, dan bukan pula hal yang boleh dilupakan.** Dua seri penomoran dalam satu
> tahun pada satu disiplin adalah akibat langsung dari mengubah bentuknya di tengah tahun.
> Index unik atas `(Discipline, LabReportNumber)` tetap menjaga nol ada dua lembar bernomor
> sama — `26-0001` dan `26.0001` memang berbeda — tetapi **urutannya patah**, dan itu terbaca
> oleh siapa pun yang mengarsipkan lembar per nomor.
>
> **Anjuran: ubah bentuk nomor hanya pada pergantian tahun.**

### 24.6 Yang TIDAK diubah amandemen ini

| Yang tetap | Alasan |
|---|---|
| `OrderNumber` | `LAB-DEC-072` utuh — ia identitas internal dan sumber barcode |
| Index unik `(Discipline, LabReportNumber)` parsial | Nol alasan berubah |
| Alokasi per disiplin per tahun | `LAB-DEC-117` nol disentuh |
| `reportNumberPrefix` | Tetap ada dan tetap kosong pada ketiga baris awal |

### 24.7 Traceability `r29`

| Keputusan | Ruas | Dilaksanakan | Terbukti |
|---|---|---|---|
| `LAB-OPEN-043` ditutup | `reportNumberSeparator`, `reportNumberLength` | `BE-LAB-63` diperluas | Tiga pesanan nyata: `26.0001`, `26000001`, `26-0004` |

---

## 25. Amandemen `r30` — Permukaan baseline data induk Mikrobiologi, 2026-09-22

> ### ✅ STATUS: `approved` — 2026-09-22
>
> | Butir | Isi |
> |---|---|
> | Status | **`approved`** |
> | `approved_by` / `approved_at` | Yoga Aji Pratama (`yogaaji452@gmail.com`) / 2026-09-22 |
> | Menutup | Selisih antara `r26`/`r27` dan `rules/backend/master-data-endpoint-standard.md` |
> | Sifat | **ADITIF.** Delapan endpoint ditambahkan; nol endpoint berubah route, verb, bentuk, atau hak akses |
> | Migration | **Nol.** Nol kolom, nol tabel, nol index |
> | Permission baru | **Nol.** Kedelapannya menumpang `Read` dan `Update` yang sudah ada |

### 25.1 Kenapa amandemen ini ada

`rules/backend/master-data-endpoint-standard.md` mewajibkan **sembilan** endpoint bagi setiap
grup data induk berkoleksi. Dua grup Mikrobiologi berdiri hanya dengan **lima**:

| Grup | Dikontrakkan | Dibangun | Kurang |
|---|---|---|---|
| `lab-susceptibility-breakpoints` | `r27` 22.5 — lima | `BE-LAB-60` — lima | `filters/metadata`, `summary`, `options`, `PATCH {id}/status` |
| `lab-procedure-microbiology-profiles` | `r27` 22.6 — "lima endpoint bergaya sama" | `BE-LAB-62` — lima | keempat yang sama |

**Ini kelalaian penulisan kontrak, bukan keputusan** — bentuk kelalaian yang sama persis
dengan `r8`, yang dahulu menambahkan dua endpoint baseline pada `Lab Specimen Type` atas alasan
identik. `r27` menyebut kelimanya dan berhenti di situ; nol baris menyatakan keempat sisanya
sengaja ditiadakan, dan nol alasan dapat dikemukakan mengapa dua grup Mikrobiologi berbeda dari
tujuh grup data induk Laboratorium lain yang sudah memenuhi baseline.

Selisihnya tidak terlihat sampai `FE-LAB-34` hendak dibangun: standar frontend
`master-data-feature-standard.md` menuntut layar data induk memuat kartu ringkasan, penyaring
yang bentuknya dibaca dari server, dan sakelar aktif per baris. Ketiganya mustahil dibangun di
atas lima endpoint.

### 25.2 Delapan endpoint yang ditambahkan

Keduanya memakai bentuk baku yang sama dengan grup data induk Laboratorium lain.

**`api/v1/health-services/laboratory-management/lab-susceptibility-breakpoints`**

| Method | Path | Kegunaan | Hak akses | Status |
|---|---|---|---|---|
| `GET` | `/filters/metadata` | Bentuk penyaring, pengurutan, dan ukuran halaman | `LabSusceptibilityBreakpoint : Read` | **Tersedia** |
| `GET` | `/summary` | Enam angka ringkasan | `LabSusceptibilityBreakpoint : Read` | **Tersedia** |
| `GET` | `/options` | Pilihan berhalaman untuk dropdown | `LabSusceptibilityBreakpoint : Read` | **Tersedia** |
| `PATCH` | `/{id}/status` | Mengaktifkan atau menonaktifkan satu baris | `LabSusceptibilityBreakpoint : Update` | **Tersedia** |

**`api/v1/health-services/laboratory-management/lab-procedure-microbiology-profiles`**

| Method | Path | Kegunaan | Hak akses | Status |
|---|---|---|---|---|
| `GET` | `/filters/metadata` | Bentuk penyaring, ditambah pilihan enum jenis biakan dan metode uji | `LabProcedureMicrobiologyProfile : Read` | **Tersedia** |
| `GET` | `/summary` | Lima angka ringkasan | `LabProcedureMicrobiologyProfile : Read` | **Tersedia** |
| `GET` | `/options` | Pilihan berhalaman untuk dropdown | `LabProcedureMicrobiologyProfile : Read` | **Tersedia** |
| `PATCH` | `/{id}/status` | Mengaktifkan atau menonaktifkan satu pemetaan | `LabProcedureMicrobiologyProfile : Update` | **Tersedia** |

### 25.3 Bentuk `summary`

| Grup | Ruas |
|---|---|
| Breakpoint | `totalBreakpoint`, `activeBreakpoint`, `inactiveBreakpoint`, `coveredOrganism`, `coveredAntibiotic`, `missingDiscContent` |
| Profil | `totalProfile`, `activeProfile`, `inactiveProfile`, `usesSusceptibilitySet`, `withoutSusceptibilitySet` |

**Keempat angka cakupan dihitung atas baris AKTIF saja**, dan itu disengaja.
`coveredOrganism` menjawab "berapa kuman yang interpretasinya dapat dihitung hari ini" —
baris nonaktif nol dipakai penghitung `r27` 22.4, sehingga memasukkannya akan melaporkan
kesiapan yang tidak dimiliki. Ia dibuktikan pada bagian 25.6.

`missingDiscContent` menghitung breakpoint aktif yang antibiotiknya **nol** punya
`discContentUg`. Angka itu bukan kesalahan melainkan pekerjaan yang tersisa: tanpa kandungan
cakram, lembar antibiogram kehilangan kolom UG-nya.

### 25.4 Bentuk `options`

| Grup | Ruas | Penyaring |
|---|---|---|
| Breakpoint | `id`, `label`, `labOrganismId`, `labAntibioticId` | `search`, `onlyActive` (default `true`), `pageNumber`, `pageSize` |
| Profil | `id`, `procedureId`, `label`, `usesSusceptibilitySet` | sama |

`label` breakpoint berbentuk `"<nama kuman> — <nama antibiotik>"`. Kedua penunjuk **tetap
dikirim di samping label** karena pemanggil yang hendak menyaring antibiogram per kuman
membutuhkan penunjuknya, dan mengurainya kembali dari teks label adalah cara yang pasti pecah
pada nama kuman yang memuat tanda pisah.

### 25.5 `PATCH /{id}/status`

Permintaan: `{ "isActive": true | false }`. Jawaban: **baris utuh** dalam bentuk respons
detail yang sudah dikontrakkan, bukan sekadar `204`, supaya layar dapat memperbarui satu baris
tanpa memuat ulang daftarnya.

`404` ketika penunjuknya tidak ditemukan atau barisnya sudah terhapus.

> **`PATCH /{id}/status` dan `DELETE /{id}` bukan dua nama untuk satu hal.** `DELETE`
> menonaktifkan **dan** menandai `IsDelete`, sehingga barisnya lenyap dari daftar. `PATCH`
> hanya membalik `IsActive`, dan barisnya tetap terlihat beserta sakelarnya. Menggabungkan
> keduanya akan membuat menonaktifkan satu breakpoint sementara — hal yang lazim ketika
> pedoman CLSI sedang ditinjau — mustahil dibatalkan dari layar.

### 25.6 Bukti terhadap database sungguhan

Dijalankan 2026-09-22 atas `QuilvianNewDevYoga`, seluruhnya lewat HTTP.

| Yang diuji | Hasil |
|---|---|
| Kedelapan endpoint | `200` |
| `PATCH` breakpoint → `false` | `activeBreakpoint` 1→0, `inactiveBreakpoint` 0→1 |
| Akibatnya pada cakupan | `coveredOrganism` 1→0, `coveredAntibiotic` 1→0, `missingDiscContent` 1→0 — membuktikan 25.3 |
| `options?onlyActive=true` sesudahnya | `totalData` 0 |
| `options?onlyActive=false` sesudahnya | `totalData` 1 |
| `PATCH` profil → `false` | `activeProfile` 1→0, `usesSusceptibilitySet` 1→0 |
| `options?search=Leuko` / `search=zzz` | `totalData` 1 / 0 |
| `PATCH` dengan penunjuk asing | `404` beserta pesan Indonesia |
| Keduanya dikembalikan ke `true` | `activeBreakpoint` 1, `activeProfile` 1 — **data dev pulih seperti semula** |

### 25.7 Yang TIDAK diubah amandemen ini

| Butir | Alasan |
|---|---|
| Kelima endpoint lama pada kedua grup | Nol berubah route, verb, bentuk, maupun hak akses |
| `lab-discipline-settings` | **Sengaja tetap empat endpoint.** Standar backend menyebut varian sah **"master data pengaturan tunggal"**: baris yang jumlahnya tetap tiga dan hanya isinya berubah nol memerlukan `summary`, `metadata`, maupun `DELETE`. Bagian `r29` 24 nol tersentuh |
| Hak akses | Nol permission baru. `Read` dan `Update` yang sudah ada menaungi kedelapannya |
| Skema database | Nol migration |

### 25.8 Satu koreksi status yang dibawa serta

Bagian 22.5 menandai kelima endpoint breakpoint **`Rencana (belum tersedia)`** padahal
`BE-LAB-60` membangunnya pada 2026-09-21. Kolom statusnya dikoreksi menjadi **`Tersedia`**.
**Yang berubah hanya kolom status** — nol endpoint ditambah, dihapus, atau diubah oleh koreksi
itu.

### 25.9 Traceability `r30`

| Yang ditutup | Endpoint | Dilaksanakan | Terbukti |
|---|---|---|---|
| Selisih baseline data induk Mikrobiologi | Delapan | `BE-LAB-64` | Bagian 25.6 — seluruhnya terhadap `QuilvianNewDevYoga` |
| Status `22.5` yang tertinggal | — | Koreksi dokumen | Kelimanya dipanggil dan menjawab `200` |

---

## 26. Amandemen `r31` — Permukaan baseline dua data induk Mikrobiologi, 2026-09-22

> ### ✅ STATUS: `approved` — 2026-09-22
>
> | Butir | Isi |
> |---|---|
> | Status | **`approved`** |
> | `approved_by` / `approved_at` | Yoga Aji Pratama (`yogaaji452@gmail.com`) / 2026-09-22 |
> | Menutup | Selisih `19.4` terhadap `rules/backend/master-data-endpoint-standard.md`, dan satu kolom yang nol punya jalan diisi |
> | Sifat | **ADITIF.** Delapan endpoint dan satu ruas ditambahkan; nol endpoint berubah route, verb, bentuk, atau hak akses |
> | Migration | **Nol.** `DiscContentUg` sudah ada pada tabel sejak `BE-LAB-60`; yang kurang hanya jalan keluarnya |
> | Permission baru | **Nol.** Kedelapannya menumpang `Read` dan `Update` yang sudah ada |

### 26.1 Kenapa amandemen ini ada

Ini **pengulangan `r30` pada dua grup berikutnya**, dan sebabnya sama persis: `r24` bagian 19.4
menyebut empat endpoint, empat endpoint itulah yang dibangun `BE-LAB-44`, dan
`master-data-endpoint-standard.md` nol dibaca saat itu.

| Grup | Dikontrakkan `r24` | Dibangun | Kurang |
|---|---|---|---|
| `lab-organisms` | empat | empat | `filters/metadata`, `summary`, `GET /{id}`, `PATCH {id}/status` |
| `lab-antibiotics` | empat | empat | keempat yang sama |

**`GET /{id}` adalah yang paling mahal di antara keempatnya.** Tanpa jalur detail, formulir ubah
yang dibuka lewat tautan langsung atau sesudah halaman disegarkan nol punya cara memuat
barisnya — dan **gagalnya diam**: layarnya sekadar tampak kosong. Modul ini sudah membayar kelas
kesalahan itu sekali lewat `r6`, sesudah `FE-LAB-03` diam-diam gagal di luar halaman daftar.
Roadmap frontend revision 33 sudah mencatatnya sebagai peringatan bagi `FE-LAB-27`; di sini ia
ditutup untuk kedua data induk Mikrobiologi.

### 26.2 Delapan endpoint yang ditambahkan

**`api/v1/health-services/laboratory-management/lab-organisms`**

| Method | Path | Kegunaan | Hak akses | Status |
|---|---|---|---|---|
| `GET` | `/filters/metadata` | Bentuk penyaring, pengurutan, dan ukuran halaman | `LabOrganism : Read` | **Tersedia** |
| `GET` | `/summary` | Empat angka ringkasan | `LabOrganism : Read` | **Tersedia** |
| `GET` | `/{id}` | Satu baris beserta seluruh ruasnya | `LabOrganism : Read` | **Tersedia** |
| `PATCH` | `/{id}/status` | Mengaktifkan atau menonaktifkan satu baris | `LabOrganism : Update` | **Tersedia** |

**`api/v1/health-services/laboratory-management/lab-antibiotics`**

| Method | Path | Kegunaan | Hak akses | Status |
|---|---|---|---|---|
| `GET` | `/filters/metadata` | Bentuk penyaring, pengurutan, dan ukuran halaman | `LabAntibiotic : Read` | **Tersedia** |
| `GET` | `/summary` | Lima angka ringkasan | `LabAntibiotic : Read` | **Tersedia** |
| `GET` | `/{id}` | Satu baris beserta seluruh ruasnya | `LabAntibiotic : Read` | **Tersedia** |
| `PATCH` | `/{id}/status` | Mengaktifkan atau menonaktifkan satu baris | `LabAntibiotic : Update` | **Tersedia** |

> ### `DELETE` tetap NOL disediakan, dan itu bukan kelalaian
>
> `r24` bagian 19.4 menolaknya atas alasan klinis: isolat yang sudah tercatat menunjuk ke baris
> ini, dan menghapusnya berarti menghapus temuan pasien. `AC-117` menuntut layarnya nol
> menampilkan tombol Hapus. Kedua grup karena itu berhenti di **delapan** endpoint, bukan
> sembilan — dan `filters/metadata` menyatakannya lewat `isDeletable: false`, supaya layar
> membacanya alih-alih menyimpulkan dari ada-tidaknya endpoint.

### 26.3 Satu ruas ditambahkan — `discContentUg`

| Ruas | Tipe | Ada pada |
|---|---|---|
| `discContentUg` | `int?` | `LabAntibioticResponse`, `CreateLabAntibioticRequest`, `UpdateLabAntibioticRequest` |

**Kolomnya sudah ada di tabel sejak `BE-LAB-60`, tetapi nol satu pun DTO membawanya.** Akibatnya
`missingDiscContent` pada ringkasan breakpoint (`r30` bagian 25.3) adalah angka yang **nol dapat
diturunkan dari layar mana pun** — ia melaporkan pekerjaan yang tersisa tanpa menyediakan jalan
mengerjakannya.

`null` berarti **belum diisi**, bukan nol mikrogram. Lembar antibiogram mencetaknya sebagai
kolom `UG`, dan baris tanpa angka ini tercetak dengan kolom kosong.

### 26.4 Bentuk `summary`

| Grup | Ruas |
|---|---|
| Organisme | `totalOrganism`, `activeOrganism`, `inactiveOrganism`, `withBreakpoint` |
| Antibiotik | `totalAntibiotic`, `activeAntibiotic`, `inactiveAntibiotic`, `withBreakpoint`, `missingDiscContent` |

**`withBreakpoint` menghitung baris yang AKTIF dan punya breakpoint AKTIF — kedua syarat.**
Menghitung hanya breakpoint aktif tanpa memeriksa barisnya sendiri menghasilkan ringkasan yang
membantah dirinya: nol organisme aktif, tetapi satu tercakup. Perilaku ini **dibuktikan** pada
bagian 26.6, bukan diasumsikan.

### 26.5 `PATCH /{id}/status` bukan jalan pintas `PUT /{id}`

`PUT` menuntut seluruh ruas dikirim. Menonaktifkan satu baris dari halaman daftar lewat `PUT`
karena itu memaksa layar memuat detailnya lebih dulu hanya untuk mengirim balik ruas yang nol
berubah — dan **setiap ruas yang ikut terkirim adalah ruas yang dapat tertimpa nilai basi**.

Permintaan: `{ "isActive": true | false }`. Jawaban: baris utuh. `404` bila penunjuknya tidak
ditemukan.

### 26.6 Bukti terhadap database sungguhan

Dijalankan 2026-09-22 atas `QuilvianNewDevYoga`, seluruhnya lewat HTTPS.

| Yang diuji | Hasil |
|---|---|
| Kedelapan endpoint baru | `200` |
| `GET /{id}` penunjuk asing | `404` |
| `PATCH` penunjuk asing | `404` |
| `PUT` antibiotik dengan `discContentUg: 10` | Tersimpan dan terbaca |
| `missingDiscContent` ringkasan **antibiotik** sesudahnya | 1 → **0** |
| `missingDiscContent` ringkasan **breakpoint** sesudahnya | 1 → **0** |
| Kolom `discContentUg` pada daftar breakpoint | `null` → **10** |
| `PATCH` organisme → `false` | `activeOrganism` 1→0, `withBreakpoint` 1→**0** |
| `GET /options` sesudahnya | `totalData` **0** — `VAL-85` tegak, kuman nonaktif nol dapat dipilih |
| `GET /` sesudahnya | `totalData` **1** — `AC-118` tegak, barisnya tetap terlihat |
| Seluruhnya dikembalikan | `missingDiscContent` kembali 1, keduanya aktif — **data dev pulih seperti semula** |
| Baris `SysActionAccess` sesudah seeder | **3 per controller** — `Read`, `Create`, `Update`; **nol baris `Delete`**, sejalan dengan nol endpoint `DELETE` |

### 26.7 Yang TIDAK diubah amandemen ini

| Butir | Alasan |
|---|---|
| Keempat endpoint lama pada kedua grup | Nol berubah route, verb, bentuk, maupun hak akses |
| `DELETE` | Bagian 26.2 — ditolak atas alasan klinis, bukan kelalaian |
| Hak akses | Nol permission baru |
| Skema database | Nol migration — `DiscContentUg` sudah ada sejak `BE-LAB-60` |

### 26.8 Satu koreksi status yang dibawa serta

Bagian 19.4 menandai kedelapan endpoint lama **`Rencana (belum tersedia)`** padahal `BE-LAB-44`
membangunnya 2026-09-18. Kolom statusnya dikoreksi menjadi **`Tersedia`**. Yang berubah hanya
kolom status.

### 26.9 Traceability `r31`

| Yang ditutup | Endpoint/ruas | Dilaksanakan | Terbukti |
|---|---|---|---|
| Selisih baseline `19.4` | Delapan endpoint | `BE-LAB-65` | Bagian 26.6 |
| `missingDiscContent` nol punya jalan diisi | `discContentUg` | `BE-LAB-65` | 1 → 0 pada **kedua** ringkasan |
| Status `19.4` yang tertinggal | — | Koreksi dokumen | Kedelapannya dipanggil dan menjawab `200` |

---

## 27. Amandemen `r32` — Permukaan baseline tiga data induk Patologi Anatomi, 2026-09-23

> ### ✅ STATUS: `approved` — 2026-09-23
>
> | Butir | Isi |
> |---|---|
> | Status | **`approved`** |
> | `approved_by` / `approved_at` | Yoga Aji Pratama (`yogaaji452@gmail.com`) / 2026-09-23 |
> | Dasar persetujuan | Instruksi pemilik modul pada sesi 2026-09-23, memilih **"Setujui penuh — 11 endpoint"** atas usul `BE-LAB-66` bagian 6ae.6 |
> | Menutup | Selisih bagian `20` terhadap `rules/backend/master-data-endpoint-standard.md` pada ketiga grup Patologi Anatomi |
> | Sifat | **ADITIF.** Sebelas endpoint ditambahkan; nol endpoint berubah route, verb, bentuk, atau hak akses |
> | Migration | **Nol.** Keempat tabel berdiri sejak `BE-LAB-50`; yang kurang hanya jalan keluarnya |
> | Permission baru | **Nol.** Kesebelasnya menumpang `Read` dan `Update` yang sudah ada |

### 27.1 Kenapa amandemen ini ada

Ini **pengulangan `r30` dan `r31` pada tiga grup terakhir**, dan sebabnya sama persis: `r25`
menyebut sepuluh endpoint data induk, sepuluh itulah yang dibangun `BE-LAB-50`, dan
`master-data-endpoint-standard.md` nol dibaca saat itu.

Bedanya dengan dua pendahulunya: **selisih ini sudah diketahui sejak 2026-09-22 dan sengaja
dicatat.** `BE-LAB-65` bagian 6ad.1 menyapu kedua puluh dua controller Laboratorium, menemukan
ketiga grup ini kurang, lalu meninggalkannya — mengerjakannya di sana berarti dua task dalam satu
pemanggilan. Amandemen ini menutup catatan itu.

| Grup | Dikontrakkan `r25` | Dibangun | Kurang |
|---|---|---|---|
| `lab-pathology-parameters` | empat | empat | `filters/metadata`, `summary`, `GET /{id}`, `PATCH {id}/status` |
| `lab-pathology-categories` | enam | enam | keempat yang sama |
| `lab-procedure-pathology-categories` | empat | empat | `filters/metadata`, `summary`, `GET /{id}` — **tiga**, lihat 27.4 |

### 27.2 Sebelas endpoint yang ditambahkan

**`api/v1/health-services/laboratory-management/lab-pathology-parameters`**

| Method | Path | Kegunaan | Hak akses |
|---|---|---|---|
| `GET` | `/filters/metadata` | Bentuk penyaring, pengurutan, dan ukuran halaman | `LabPathologyParameter : Read` |
| `GET` | `/summary` | Empat angka ringkasan | `LabPathologyParameter : Read` |
| `GET` | `/{id}` | Satu baris beserta seluruh ruasnya | `LabPathologyParameter : Read` |
| `PATCH` | `/{id}/status` | Mengaktifkan atau menonaktifkan satu baris | `LabPathologyParameter : Update` |

**`api/v1/health-services/laboratory-management/lab-pathology-categories`**

| Method | Path | Kegunaan | Hak akses |
|---|---|---|---|
| `GET` | `/filters/metadata` | Bentuk penyaring, pengurutan, dan ukuran halaman | `LabPathologyCategory : Read` |
| `GET` | `/summary` | Empat angka ringkasan | `LabPathologyCategory : Read` |
| `GET` | `/{id}` | Satu baris beserta jumlah keberlakuannya | `LabPathologyCategory : Read` |
| `PATCH` | `/{id}/status` | Mengaktifkan atau menonaktifkan satu baris | `LabPathologyCategory : Update` |

**`api/v1/health-services/laboratory-management/lab-procedure-pathology-categories`**

| Method | Path | Kegunaan | Hak akses |
|---|---|---|---|
| `GET` | `/filters/metadata` | Bentuk penyaring, pengurutan, dan ukuran halaman | `LabPathologyCategory : Read` |
| `GET` | `/summary` | Tiga angka ringkasan, termasuk `unmappedProcedure` | `LabPathologyCategory : Read` |
| `GET` | `/{id}` | Satu pemetaan beserta pemeriksaan dan golongannya | `LabPathologyCategory : Read` |

### 27.3 Bentuk `summary`

| Grup | Ruas |
|---|---|
| Parameter | `totalParameter`, `activeParameter`, `inactiveParameter`, `usedInCategory` |
| Golongan | `totalCategory`, `activeCategory`, `inactiveCategory`, `withParameter` |
| Pemetaan | `totalProcedure`, `mappedProcedure`, `unmappedProcedure` |

**`usedInCategory` dan `withParameter` dihitung dari keberlakuan yang hidup** — baris
`LabPathologyParameterCategory` yang belum ditandai terhapus. Keduanya menjawab pertanyaan yang
sama dari dua arah: parameter yang nol pernah dipakai golongan mana pun, dan golongan yang nol
punya satu pun ruas. **Golongan tanpa ruas menghasilkan formulir kosong**, dan tanpa angka ini
keadaannya baru ketahuan ketika patolog sudah membuka layar hasil.

**`unmappedProcedure` bukan hiasan.** Ia angka yang menjawab penahan `FE-LAB-28`: berapa jenis
pemeriksaan Patologi Anatomi yang belum digolongkan. Ketiganya memakai definisi yang **sama
persis** dengan `GET /suggestions` yang sudah berjalan — `IsLaboratory` benar dan `LabDiscipline`
bernilai `AnatomicalPathology`. Memakai definisi lain akan membuat ringkasan dan daftar usulan
saling membantah.

### 27.4 `PATCH /{id}/status` dan `GET /options` NOL berlaku bagi grup pemetaan

**`LabProcedurePathologyCategory` nol punya `IsActive`.** Ia memuat `Id`, `ProcedureId`,
`LabPathologyCategoryId`, dan dua navigasi di atas `IdentityModel` — baris pemetaan, bukan data
induk berstatus. Menambahkan kolom status di atasnya berarti **migration**, dan itu keluar dari
sifat aditif amandemen ini. Memindahkan penggolongan satu pemeriksaan dilakukan lewat `PUT /{id}`
yang sudah ada sejak `r25`.

**`GET /options` nol dibangun bagi grup itu.** Nol satu pun layar memilih sebuah *pemetaan* dari
kotak pilihan — yang dipilih adalah jenis pemeriksaan dan golongannya, dan keduanya sudah punya
`/options` sendiri. `QBE-OPT-001` menetapkan options disediakan **hanya bila dikonsumsi**;
membangunnya di sini berarti mengulang pola `BE-LAB-26`, yaitu sesuatu yang berdiri tanpa pembaca
lalu nol menghasilkan galat apa pun sampai seseorang membutuhkannya.

> Bagian 6ad.1 pada roadmap backend sempat mencatat grup ini *"kurang kelimanya"*. Angka itu
> diturunkan dari baseline sembilan tanpa memeriksa entity-nya. **Tiga** yang benar-benar
> berlaku, dan ini koreksinya — bukan pengurangan cakupan.

### 27.5 `DELETE` tetap NOL disediakan pada ketiganya

Alasannya sama dengan `r24` 19.4 dan `r31` 26.2, dan di sini lebih tajam: **parameter yang
dihapus menarik ruas dari laporan pasien yang sudah tersimpan**, dan **golongan yang dihapus
membuat pesanan lama nol punya bentuk formulir**. `AC-143` menuntut layarnya nol menampilkan
tombol Hapus.

Ketiga `filters/metadata` menyatakannya lewat `isDeletable: false`, supaya layar membacanya
alih-alih menyimpulkan dari ada-tidaknya endpoint.

### 27.6 Yang NOL berubah

| Butir | Alasan |
|---|---|
| Kesepuluh endpoint lama pada ketiga grup | Nol berubah route, verb, bentuk, maupun hak akses |
| `{id}/parameters` dan `suggestions` | Endpoint tambahan di luar baseline, sah menurut standar bagian 3, dan sudah dipakai rancangan `FE-LAB-27` |
| `DELETE` | Bagian 27.5 |
| Hak akses | Nol permission baru; grup pemetaan tetap menumpang `LabPathologyCategory` |
| Skema database | **Nol migration** |

### 27.7 Traceability `r32`

| Yang ditutup | Endpoint | Dilaksanakan | Terbukti |
|---|---|---|---|
| Selisih baseline ketiga grup PA | Sebelas endpoint | `BE-LAB-66` | `AC-192`..`AC-194`, roadmap bagian 6af |
| Penahan `FE-LAB-27` | Keseluruhan | `BE-LAB-66` | Ketiga layar dapat dibangun sesuai `master-data-feature-standard` |
| Catatan `BE-LAB-65` 6ad.1 | — | `BE-LAB-66` | Ketiga grup nol lagi tercatat kurang |
