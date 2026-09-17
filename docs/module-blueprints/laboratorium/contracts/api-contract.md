# API Contract — Modul Laboratorium

| Field | Value |
|---|---|
| Contract version | `LAB-API-v1` |
| Revision | `17` |
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
