# API Contract — Modul Rawat Inap

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| `contract_version` | Mengikuti set kontrak pada [manifest](../blueprint-manifest.md), amandemen Bed Management draft. Riwayat metadata sebelumnya: **`0.11.0`** — bagian 12 Workspace PPRI, `approved` 2026-10-08 (`RWI-DEC-265`). Sebelumnya `0.10.0` — bagian 11 Finishing, `approved` (`RWI-DEC-221`); `0.9.0` — bagian 10 |
| `last_changed_in` | **`0.12.0`** — Bed Management. Riwayat metadata sebelumnya: **`0.11.0`** — grup baru `Inpatient Admission Workspace`, isian baru dua master, peringatan detail episode. Sebelumnya `0.10.0` — Finishing; `0.9.0` — census dokter, penugasan pendukung, tiga isian resume, akibat penutupan |
| Status | **`draft`** — amandemen Bed Management belum disetujui. Riwayat metadata sebelumnya: **`draft`** untuk `0.9.0`. `0.8.0` **`approved`** — disetujui **Muhammad Hamzah** 2026-09-11 lewat `RWI-DEC-105` |
| Owner | Produk/domain/API Muhammad Hamzah; keamanan/privasi OPEN; frontend sesuai DEC-292. Riwayat metadata sebelumnya: Product/Domain Owner sementara sesuai `RWI-DEC-006`; nama belum diisi |
| `approved_by` / `approved_at` | Belum ada untuk amandemen Bed Management. Riwayat metadata sebelumnya: **Muhammad Hamzah — Product/Domain owner (`RWI-DEC-061`), 10 September 2026**, lewat instruksi eksplisit untuk mengerjakan `BE-RWI-069`. Mengikuti pola approval per-task yang sudah dipakai `BE-RWI-036` pada 1 September 2026 |
| `input_revision` | Decision 46; gate 1.12 / BM-RCG-20261010-01; audit BM-AUD-20261010-01 rev1. Riwayat metadata sebelumnya: `02-backend-architecture.md` revision `0.4`; `00-interview-decisions.md` revision `15`; `04-prd-to-mvp.md` revision `0.6.0` |
| Backend SHA | Bed Management: d4e1eca06fb28c05934c68c1e51a4dca01935a10. Riwayat metadata: `44099e4` — hasil merge `QuilvianIntegrationBackend`. Sebelumnya `5afb54b` |
| Dampak kompatibilitas | Bed Management: GET aditif, mutation diperketat (expected version/key/category/reason); cutover seluruh konsumen wajib. Riwayat metadata sebelumnya: **Seluruhnya aditif.** Tidak ada endpoint existing yang berubah bentuknya. Satu endpoint existing berubah **perilakunya**, lihat bagian 7 |


### Perubahan pada `contract_version` `0.8.0`

**Status: `approved` sejak 11 September 2026** lewat `RWI-DEC-105`. Amandemen ini menyerap `RWI-DEC-101`, yang menutup temuan `P0` nomor satu pada `PRD-to-MVP-Rawat-Inap-V2`.

**Masalah yang ditutupnya.** Kelayakan tempat tidur ikut menilai jenis kelamin **penghuni kamar
lain**. Akibatnya kamar berisi satu pasien laki-laki menolak seluruh pasien perempuan, walaupun
tempat tidur yang dituju memang dikonfigurasi Admin Master Data untuk menerima keduanya. Keputusan
privasi berubah menjadi akibat sampingan dari siapa yang kebetulan datang lebih dulu, dan petugas
admisi tidak punya jalan keluar selain memindahkan pasien yang sudah dirawat.

| Yang berubah | Dasar |
| --- | --- |
| Kode penolakan `ROOM_GENDER_MIXED` **dihapus seluruhnya**. Ia tidak lagi muncul pada `failures[]` mana pun, baik pada pencarian, pemesanan, penempatan, maupun perpindahan | `RWI-DEC-101`; `MVP-RWI-D-002` |
| Aturan nomor 6 pada Kelayakan Penempatan **dipensiunkan**. Nomor 6 dibiarkan kosong dan tidak dipakai ulang | `RWI-DEC-101` |
| Aturan nomor 5 `PATIENT_GENDER_UNKNOWN` **dipersempit**: syaratnya kini hanya tempat tidur menerima laki-laki dan perempuan sekaligus. Syarat "kamar belum berpenghuni" dicabut | `RWI-DEC-101`; `FR-MVP-EP-006` |
| `BED_GENDER_MISMATCH` aturan 4 **tidak berubah sama sekali** | `RWI-RULE-012` B.1 tetap berlaku |
| `ISOLATION_REQUIRED` aturan 7 dan `ISOLATION_BED_RESERVED` aturan 8 **tidak berubah sama sekali** | `RWI-RULE-012` bagian A tidak tersentuh |
| Pengecualian boks bayi **tidak berubah** | `RWI-RULE-012` B.5 tetap berlaku |

**Kenapa ini perubahan yang merusak, dan bagi siapa.** Bagi pemanggil yang hanya membaca daftar
bed yang lolos, perubahan ini **menambah** bed yang sebelumnya tertolak, sehingga tidak ada bentuk
response yang berubah. Yang rusak adalah pemanggil yang **memetakan kode penolakan**: frontend
menyimpan `ROOM_GENDER_MIXED` pada `inpatient-placement-utils.jsx`, dan tiga berkas test
menguncinya, yaitu `tests/unit/inpatient-placement.test.mjs` pada tiga tempat serta
`tests/e2e/inpatient-episode-detail.spec.mjs`. Karena itu backend dan frontend **wajib berada pada
satu gelombang rilis**; menurunkan salah satunya lebih dulu meninggalkan test yang menguji kode
yang sudah tidak pernah terbit.

**Contoh perubahan perilaku yang dapat diuji.** Kamar Melati 1 berisi tiga tempat tidur yang
seluruhnya dikonfigurasi `IsForMale` dan `IsForFemale` bernilai benar. Pukul 08:00 Tn. Budi
menempati `MELATI-01-A`.

| Keadaan | Sebelum `0.8.0` | Sejak `0.8.0` |
| --- | --- | --- |
| Ny. Sari ditempatkan ke `MELATI-01-B` | **Ditolak** `422 ROOM_GENDER_MIXED` | **Berhasil** |
| Pasien laki-laki ke tempat tidur bertanda perempuan saja | Ditolak `422 BED_GENDER_MISMATCH` | **Tetap ditolak** `422 BED_GENDER_MISMATCH` |
| Pasien tanpa jenis kelamin tercatat ke `MELATI-01-B` | **Ditolak** `422 PATIENT_GENDER_UNKNOWN` karena kamar sudah berpenghuni | **Berhasil**, karena tempat tidurnya menerima keduanya |
| Pasien tanpa jenis kelamin tercatat ke tempat tidur perempuan saja | Ditolak `422 PATIENT_GENDER_UNKNOWN` | **Tetap ditolak** `422 PATIENT_GENDER_UNKNOWN` |
| Pasien tanpa kebutuhan isolasi ke tempat tidur isolasi | Ditolak `422 ISOLATION_BED_RESERVED` | **Tetap ditolak** `422 ISOLATION_BED_RESERVED` |

### Perubahan pada `contract_version` `0.7.0`

**Status: `approved` sejak 10 September 2026.** Gerbang persetujuan pemilik dicabut hari itu,
dan `BE-RWI-069` langsung dikerjakan pada tanggal yang sama — lihat
[laporan `BE-RWI-069`](../task/report/backend/BE-RWI-069.md). `FE-RWI-057` pada roadmap frontend
**tidak** ikut terbuka oleh approval ini; gerbangnya sendiri diputuskan terpisah.

> **Keadaan sebelumnya, disimpan sebagai jejak.** Sampai 9 September 2026 versi ini berstatus
> `draft`, dan selama itu `BE-RWI-069` serta `FE-RWI-057` berstatus
> `BLOCKED_PENDING_OWNER_APPROVAL` pada roadmap masing-masing.

Masalah yang ditutupnya. Layar pemilihan tempat tidur hari ini hanya menerima daftar bed yang
**lolos** kelayakan. Bed yang ditolak hilang begitu saja tanpa satu pun keterangan, sehingga layar
terpaksa menebak dari kolom seadanya dan berakhir pada kalimat "Tidak lolos kelayakan". Kalimat itu
tidak memberitahu petugas apa pun. Padahal server sudah menghitung alasannya lengkap untuk setiap
bed, lalu membuangnya di `SearchAvailableBedsAsync`.

| Yang berubah | Dasar |
| --- | --- |
| `GET /bed-occupancies/available-beds` menerima query baru `includeIneligible`, bawaannya `false` | `RWI-RULE-012`; bukti runtime pemilik 9 September 2026 |
| `AvailableBedPagedResult` mendapat field baru `ineligible`, berisi daftar `{ bedId, failures[] }` | Bentuk `failures[]` **sudah ada**, dipakai jawaban 422 pada bagian Bed Occupancy |
| Nol Resource baru, nol Action baru, nol endpoint baru | Hak aksesnya tetap `InpatientBedOccupancy : Read` |

**Kenapa aditif dan aman.** `includeIneligible` bawaannya mati, sehingga setiap pemanggil lama
menerima jawaban yang sama persis seperti sebelumnya. Field `ineligible` terkirim sebagai array
kosong bagi pemanggil yang tidak memintanya. Tidak ada bentuk lama yang berubah dan tidak ada
pemanggil lama yang perlu disesuaikan.

**Batas yang mengikat.** Daftar `ineligible` hanya diisi ketika `episodeId` ikut dikirim. Tanpa
episode, empat dari delapan aturan kelayakan tidak dapat dinilai sama sekali, sehingga alasan yang
dikirim akan menyesatkan. Permintaan `includeIneligible=true` tanpa `episodeId` dijawab dengan
`ineligible` kosong, bukan dengan tebakan sebagian.

Contoh jawaban, dipersingkat pada bagian yang tidak berubah.

```json
{
  "success": true,
  "statusCode": 200,
  "data": {
    "pageNumber": 1,
    "pageSize": 100,
    "totalData": 1,
    "totalPage": 1,
    "items": [
      { "bedId": "ea773e3d-5cca-4883-b112-26b83b6746b1", "bedCode": "BD-RSMMC-00003" }
    ],
    "ineligible": [
      {
        "bedId": "4842ce58-a1c9-481f-b691-aa6c5cbc88f9",
        "failures": [
          {
            "ruleNumber": 4,
            "code": "BED_GENDER_MISMATCH",
            "message": "Tempat tidur ini hanya menerima pasien perempuan.",
            "statusCode": 422
          }
        ]
      },
      {
        "bedId": "f24fe210-b852-45c7-9813-8253597c1e71",
        "failures": [
          {
            "ruleNumber": 8,
            "code": "ISOLATION_BED_RESERVED",
            "message": "Tempat tidur isolasi hanya untuk pasien yang membutuhkan isolasi.",
            "statusCode": 422
          }
        ]
      }
    ]
  }
}
```

### Perubahan pada `contract_version` `0.2.0`

| Yang berubah | Dasar |
| --- | --- |
| Endpoint baru `POST /discharges/{episodeId}/record-departure` | `RWI-DEC-055` |
| Kode 409 baru pada penempatan: pasien sudah punya episode yang hadir | `RWI-DEC-054` |
| `POST /bed-occupancies/placements/transfer` menolak pasien yang kepergiannya sudah dicatat | `RWI-DEC-055` |
| `GET /discharges/{episodeId}/summary` dapat menyertakan riwayat versi resume | `RWI-DEC-057` |

Tidak ada endpoint yang dihapus dan tidak ada bentuk request atau response yang berubah.

### Perubahan pada `contract_version` `0.3.0`

| Yang berubah | Dasar |
| --- | --- |
| Endpoint baru `PATCH /episodes/{id}/isolation-requirement` | `RWI-DEC-065` |
| Endpoint baru `GET /monitoring/isolation-mismatch` | `RWI-DEC-065` aturan 7 |
| Penempatan dan perpindahan menolak lima keadaan baru: jenis kelamin tidak cocok, jenis kelamin belum tercatat, kamar sudah dihuni jenis kelamin berbeda, dan dua aturan isolasi | `RWI-DEC-064`, `RWI-DEC-066` |
| `GET /bed-occupancies/available-beds` menyaring hasil memakai kedelapan aturan Kelayakan Penempatan | `RWI-DEC-064` |

### Perubahan pada `contract_version` `0.4.0`

| Yang berubah | Dasar |
| --- | --- |
| `POST /placements` menolak satu keadaan baru: pasien asal IGD yang belum tercatat tiba | `RWI-DEC-072` |
| `POST /placements` tidak lagi selalu menetapkan waktu mulai sendiri; untuk pasien asal IGD waktunya dibaca dari catatan kepergian IGD | `RWI-DEC-072` |
| `GET /available-beds` menyaring memakai **sembilan** aturan Kelayakan Penempatan, bukan delapan | `RWI-DEC-072` |

**Tidak ada bentuk request atau response yang berubah, dan tidak ada endpoint baru.** Keduanya
hanya berlaku pada jalur serah terima IGD, yaitu `INP-S09` yang di luar scope revisi ini. Untuk
seluruh endpoint yang dipakai MVP, perilakunya sama persis seperti `0.3.0`.

> **DIPERBARUI 26 Agustus 2026 — ke-49 endpoint baru berstatus `Tersedia`.** Lihat pemutakhiran
> 1 September 2026 di bawah.
>
> Catatan sebelumnya berbunyi *"Seluruh endpoint pada dokumen ini berstatus `Rencana (belum
> tersedia)`. Tidak satu pun sudah ada di dalam kode pada SHA `5afb54b`."* Pernyataan itu benar
> pada SHA tersebut dan **sudah tidak berlaku**.
>
> Buktinya bukan pembacaan source, melainkan aplikasi yang benar-benar menyala:
>
> | Bukti | Hasil |
> | --- | --- |
> | Migration diterapkan ke PostgreSQL | 13 tabel `Inp*`/`MstInpatient*` terbentuk; 6 unique index parsial hidup |
> | Aplikasi menyala dan melayani | `GET /health` → **200** |
> | Dokumen Swagger `health-services` | **HTTP 200**, 4.230.239 byte |
> | Operasi HTTP pada path `inpatient` | **49** — cocok persis dengan jumlah baris pada dokumen ini |
> | Lima endpoint dipanggil tanpa token | **401** semuanya — `[Authorize]` tegak saat runtime |
>
> Baris `PATCH /{id}/availability` pada bagian Bed saat itu **tetap** `Rencana perubahan
> perilaku`, karena `BE-RWI-006` masih terblokir `FE-RWI-001`.
>
> **DIPERBARUI 1 September 2026 — kontrak modul tertutup penuh.**
>
> | Perubahan | Task |
> | --- | --- |
> | Endpoint baru ke-50, `GET /discharges/{episodeId}/financial-clearance`, berstatus `Tersedia` | `BE-RWI-034` |
> | Sembilan baris kolom hak akses dibetulkan menjadi pasangan yang benar-benar didaftarkan `AccessMenuSeeder` | `BE-RWI-034` |
> | Baris `PATCH /{id}/availability` naik dari `Rencana perubahan perilaku` menjadi **`Diterapkan`** | `BE-RWI-006` |
>
> Tidak ada lagi baris berstatus `Rencana` pada dokumen ini.

### Koreksi pada `contract_version` `0.6.1`

Trace terhadap source `44099e4` menemukan tiga baris `0.6.0` yang salah, bukan sekadar kurang.

| Yang dikoreksi | Buktinya |
| --- | --- |
| `episodeId` **tidak jadi** ditambahkan pada `top-ups`, dan kolom `EpisodeId` dibatalkan | `BilDepositAccountConfiguration.cs:27` dan `InpEpisodeConfiguration.cs:26` sama-sama mengunci `EncounterId` unique; episodenya terbaca lewat join |
| Rute `POST /deposits/episodes/{episodeId}/refunds` **dicabut** | `BillingFinancialExceptionsController.cs:112` sudah menyediakan `POST /financial-exceptions/refunds` beserta `approve`, di bawah `BIL-API-0.4` yang sudah disetujui |
| Idempotensi tidak perlu dibangun | `BillingPatientFundsController.cs:99` memakai header `Idempotency-Key`; `BilDepositMovementConfiguration.cs:31` menguncinya unique |

`POST /deposits/episodes/{episodeId}/settle` **dipertahankan sebagai rencana**: alokasi yang ada
hari ini bekerja per kunjungan dan belum menghasilkan posisi settlement per episode.

### Perubahan pada `contract_version` `0.5.0` dan `0.6.0`

| Yang berubah | Dasar |
| --- | --- |
| Bagian baru **Deposit Rawat Inap** pada `BillingManagement`, berisi tujuh baris: tiga rute `patient-funds` yang sudah ada dan empat rute baru | `EPIC RI-35`, `FR-RI-163` s.d. `FR-RI-178` |
| ~~`POST /patient-funds/deposits/{encounterId}/top-ups` wajib menerima `episodeId`~~ — **dicabut `0.6.1`** | `FR-RI-163`, `RWI-DEC-093` sebagaimana dikoreksi |
| Rute baru `GET /patient-funds/deposit-policies` sebagai sumber minimum deposit pada langkah admisi | `FR-RI-175`, `RWI-DEC-094` |
| Rute baru `GET /patient-funds/deposits/episodes/{episodeId}` dan `POST …/settle`. Bagian `…/refunds` **dicabut `0.6.1`** | `FR-RI-167`, `FR-RI-170`, `FR-RI-171` |
| Usulan base URL `billing-management/inpatient-deposits` **dicabut** sebelum sempat dipakai | `04-prd-to-mvp.md` `0.6.0` |
| `GET /monitoring/deposit-shortfall` sebagai daftar pantau kekurangan deposit | `FR-RI-177`, `RWI-DEC-096` |

Tidak ada endpoint yang dihapus. Seluruh perubahan aditif, kecuali penambahan `episodeId` pada
`top-ups` yang bersifat **aditif pada request body** dan tidak mengubah pemanggilan lama.

Base URL modul: `api/v1/health-services/inpatient-management/`

---

## Health Services / Inpatient Management / Inpatient Episode

Base URL: `api/v1/health-services/inpatient-management/episodes`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Mengambil pilihan penyaring beserta nilai bawaannya untuk layar daftar episode | `InpatientEpisode : Read` | – | `ApiResponse<InpatientEpisodeFilterMetadataResponse>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |
| `GET` | `/summary` | Ringkasan jumlah episode per status | `InpatientEpisode : Read` | Query | `ApiResponse<InpatientEpisodeSummaryResponse>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |
| `GET` | `/` | Daftar episode bertingkat, dapat disaring unit layanan, status, tanggal, dan nama pasien | `InpatientEpisode : Read` | Query | `ApiResponse<InpatientEpisodePagedResult>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |
| `GET` | `/{id}` | Detail satu episode beserta DPJP aktif, perawat aktif, dan lokasi terkini | `InpatientEpisode : Read` | – | `ApiResponse<InpatientEpisodeDetailResponse>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |
| `GET` | `/{id}/status-history` | Riwayat perpindahan status episode | `InpatientEpisode : Read` | – | `ApiResponse<List<InpatientStatusHistoryResponse>>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |
| `POST` | `/` | Membuka admisi. Membuat episode `Draft` dan menetapkan DPJP pertama | `InpatientEpisode : Create` | `OpenAdmissionRequest` | `ApiResponse<InpatientEpisodeDetailResponse>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |
| `PUT` | `/{id}` | Mengubah isian admisi selama episode masih `Draft` | `InpatientEpisode : Update` | `UpdateAdmissionRequest` | `ApiResponse<InpatientEpisodeDetailResponse>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |
| `PATCH` | `/{id}/cancel` | Membatalkan admisi. Melepas pemesanan dan penempatan dalam satu tindakan | `InpatientEpisode : Update` | `CancelAdmissionRequest` | `ApiResponse<InpatientEpisodeDetailResponse>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |
| `POST` | `/{id}/doctor-assignments` | Mengalihkan DPJP. Menutup penugasan lama dan membuka penugasan baru | `InpatientEpisode : Update` | `HandoverDoctorRequest` | `ApiResponse<InpatientDoctorAssignmentResponse>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |
| `GET` | `/{id}/doctor-assignments` | Riwayat DPJP episode | `InpatientEpisode : Read` | – | `ApiResponse<List<InpatientDoctorAssignmentResponse>>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |
| `POST` | `/{id}/nurse-assignments` | Menugaskan atau mengganti perawat penanggung jawab | `InpatientEpisode : Update` | `AssignNurseRequest` | `ApiResponse<InpatientNurseAssignmentResponse>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |
| `PATCH` | `/{id}/isolation-requirement` | Menetapkan atau mengubah kebutuhan isolasi episode | `InpatientEpisode : SetIsolation` | `SetIsolationRequirementRequest` | `ApiResponse<InpatientEpisodeDetailResponse>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |
| `GET` | `/{id}/nurse-assignments` | Riwayat perawat penanggung jawab | `InpatientEpisode : Read` | – | `ApiResponse<List<InpatientNurseAssignmentResponse>>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |
| `POST` | `/{id}/correction-sessions` | Membuka sesi koreksi pada episode yang sudah ditutup | `InpatientEpisode : Reopen` | `OpenCorrectionSessionRequest` | `ApiResponse<InpatientCorrectionSessionResponse>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |
| `PATCH` | `/{id}/correction-sessions/{sessionId}/close` | Menutup sesi koreksi beserta daftar perubahannya | `InpatientEpisode : Reopen` | `CloseCorrectionSessionRequest` | `ApiResponse<InpatientCorrectionSessionResponse>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |

Kode status yang mungkin muncul dan artinya bagi pengguna:

| Kode | Arti bagi pengguna |
| --- | --- |
| 200 | Permintaan berhasil |
| 400 | Isian tidak lengkap atau tidak masuk akal, misalnya alasan pembatalan kosong |
| 401 | Pengguna belum login atau sesi sudah berakhir |
| 403 | Pengguna tidak punya hak akses untuk tindakan ini |
| 404 | Episode yang dimaksud tidak ditemukan |
| 409 | Tindakan bertabrakan dengan keadaan sekarang, misalnya episode sudah ditutup |
| 422 | Aturan bisnis menolak, misalnya membatalkan episode yang sudah punya catatan klinis |

---

## Health Services / Inpatient Management / Bed Occupancy

Base URL: `api/v1/health-services/inpatient-management/bed-occupancies`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/available-beds` | Mencari tempat tidur yang benar-benar dapat ditempati, sudah memperhitungkan pemesanan yang masih berlaku. Sejak `0.7.0` dapat sekaligus menyebutkan tempat tidur yang **ditolak** beserta aturan yang menolaknya | `InpatientBedOccupancy : Read` | Query, termasuk `includeIneligible` sejak `0.7.0` | `ApiResponse<AvailableBedPagedResult>`, dengan field `ineligible` sejak `0.7.0` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026. Bagian `ineligible` ikut ✅ **Tersedia** sejak 10 Sep 2026 lewat `BE-RWI-069` |
| `GET` | `/bed-board` | Papan ketersediaan tempat tidur per unit layanan dan kamar | `InpatientBedOccupancy : Read` | Query | `ApiResponse<BedBoardResponse>` + metadata aditif [`RWI-BED-BOARD-RESERVATION-001 1.0.0`](bed-board-reservation-metadata-contract.md) | ✅ **Tersedia** — metadata reservasi aktif dilengkapi `BE-RWI-036` pada 1 Sep 2026 |
| `POST` | `/reservations` | Memesan tempat tidur untuk satu episode `Draft` | `InpatientBedOccupancy : Create` | `ReserveBedRequest` | `ApiResponse<BedReservationResponse>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |
| `PATCH` | `/reservations/{id}/cancel` | Membatalkan pemesanan sebelum dipakai | `InpatientBedOccupancy : Update` | `CancelReservationRequest` | `ApiResponse<BedReservationResponse>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |
| `POST` | `/placements` | Menempatkan pasien ke tempat tidur dan mengaktifkan episode | `InpatientBedOccupancy : Create` | `PlacePatientRequest` | `ApiResponse<BedPlacementResponse>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |
| `POST` | `/placements/transfer` | Memindahkan pasien ke tempat tidur lain dalam satu tindakan utuh | `InpatientBedOccupancy : Transfer` | `TransferPatientRequest` | `ApiResponse<BedPlacementResponse>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |
| `GET` | `/placements/by-episode/{episodeId}` | Riwayat penempatan satu episode, dari tempat tidur pertama sampai terakhir | `InpatientBedOccupancy : Read` | – | `ApiResponse<List<BedPlacementResponse>>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |

Kode status tambahan yang khas bagian ini:

| Kode | Arti bagi pengguna |
| --- | --- |
| 409 | Tempat tidur sudah ditempati atau sudah dipesan pasien lain. Ini yang muncul ketika dua petugas merebut tempat tidur yang sama |
| 422 | Tempat tidur tidak lolos pemeriksaan kelayakan. Sejak `0.3.0` ini mencakup lima alasan baru: penanda tempat tidur tidak menerima jenis kelamin pasien, jenis kelamin pasien belum tercatat, kamar sudah dihuni pasien berjenis kelamin berbeda, pasien butuh isolasi tetapi tempat tidurnya bukan isolasi, dan pasien tidak butuh isolasi tetapi tempat tidurnya isolasi |
| 422 | Sejak `0.4.0` bertambah satu alasan lagi: pasien berasal dari serah terima IGD tetapi belum tercatat tiba di bangsal. Hanya berlaku pada jalur `INP-S09`, yang di luar scope revisi ini |

**Waktu mulai penempatan.** Untuk pasien asal IGD, `StartDateTime` pada jawaban **bukan** waktu
endpoint dipanggil, melainkan waktu tiba yang dibaca dari catatan kepergian IGD. Untuk jalur
datang langsung dan poliklinik nilainya tetap waktu penempatan dibuat. Dasarnya `RWI-DEC-072`.

**Bentuk jawaban penolakan kelayakan.** Jawaban 422 menyertakan **daftar aturan yang gagal**, bukan
satu kalimat umum. Petugas perlu tahu apakah yang menghalangi jenis kelaminnya, isolasinya, atau
keadaan tempat tidurnya, karena tindakan lanjutannya berbeda.

---

## Health Services / Inpatient Management / Inpatient Discharge

Base URL: `api/v1/health-services/inpatient-management/discharges`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/{episodeId}/decide` | DPJP memutuskan pasien boleh pulang beserta cara pulangnya | `InpatientDischarge : Update` | `DecideDischargeRequest` | `ApiResponse<InpatientEpisodeDetailResponse>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |
| `POST` | `/{episodeId}/record-departure` | Mencatat pasien sudah meninggalkan ruangan. Melepas tempat tidur seketika **tanpa** menutup episode | `InpatientDischarge : RecordDeparture` | `RecordDepartureRequest` | `ApiResponse<InpatientEpisodeDetailResponse>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |
| `GET` | `/{episodeId}/summary` | Mengambil resume pulang episode beserta daftar versi sebelumnya bila ada | `InpatientDischarge : Read` | Query `includeRevisions` | `ApiResponse<DischargeSummaryResponse>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |
| `PUT` | `/{episodeId}/summary` | Menyusun atau memperbarui resume pulang | `InpatientDischarge : Update` | `UpsertDischargeSummaryRequest` | `ApiResponse<DischargeSummaryResponse>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |
| `PATCH` | `/{episodeId}/summary/sign` | DPJP menandatangani resume pulang | `InpatientDischarge : Sign` | `SignDischargeSummaryRequest` | `ApiResponse<DischargeSummaryResponse>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |
| `GET` | `/{episodeId}/clearance` | Daftar butir administrasi beserta status penandaannya | `InpatientDischarge : Read` | – | `ApiResponse<ClearanceChecklistResponse>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |
| `POST` | `/{episodeId}/clearance/{itemId}/mark` | Menandai satu butir daftar periksa administrasi | `InpatientDischarge : Update` | `MarkClearanceItemRequest` | `ApiResponse<ClearanceChecklistResponse>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |
| `POST` | `/{episodeId}/financial-clearance` | Petugas kasir menandai kelayakan keuangan | `InpatientDischarge : MarkFinancialClearance` | `MarkFinancialClearanceRequest` | `ApiResponse<FinancialClearanceResponse>` | ✅ **Tersedia** — hak akses diperbaiki `BE-RWI-034` pada 1 Sep 2026 |
| `GET` | `/{episodeId}/financial-clearance` | Membaca penandaan kelayakan keuangan beserta seluruh riwayatnya. Hak aksesnya butir tersendiri supaya kasir dapat diberi kemampuan ini tanpa ikut membaca isi resume pulang | `InpatientDischarge : ReadFinancialClearance` | – | `ApiResponse<FinancialClearanceResponse>` | ✅ **Tersedia** — dibuka `BE-RWI-034` pada 1 Sep 2026 |
| `GET` | `/{episodeId}/closure-readiness` | Memeriksa kelima syarat penutupan dan menampilkan mana yang belum terpenuhi | `InpatientDischarge : Read` | – | `ApiResponse<ClosureReadinessResponse>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |
| `POST` | `/{episodeId}/close` | Menutup episode dan melepas tempat tidur | `InpatientDischarge : Close` | `CloseEpisodeRequest` | `ApiResponse<InpatientEpisodeDetailResponse>` | ✅ **Tersedia** — hak akses diperbaiki `BE-RWI-034` pada 1 Sep 2026 |
| `POST` | `/{episodeId}/close-with-override` | Supervisor menutup episode menembus gerbang keuangan | `InpatientDischarge : CloseOverride` | `CloseEpisodeOverrideRequest` | `ApiResponse<InpatientEpisodeDetailResponse>` | ✅ **Tersedia** — hak akses diperbaiki `BE-RWI-034` pada 1 Sep 2026 |

Kode status tambahan yang khas bagian ini:

| Kode | Arti bagi pengguna |
| --- | --- |
| 422 | Ada syarat penutupan yang belum terpenuhi. Jawabannya menyebut syarat mana saja, bukan sekadar menolak |

**Catatan bentuk jawaban `/record-departure`.** Endpoint ini **tidak** mengubah status episode.
Episode tetap `DischargePending` dan tetap wajib ditutup. Yang berubah hanya tiga hal: kolom waktu
kepergian pada episode terisi, baris penempatan ditutup dengan alasan kepergian pasien, dan salinan
status tempat tidur kembali `Available`. Jawabannya tetap berupa detail episode supaya layar dapat
langsung memperbarui tampilannya.

Endpoint ini juga tidak dapat dibatalkan. Bila ternyata pasien belum jadi pulang, jalannya adalah
menutup episode lalu menjalankan admisi baru, sesuai `RWI-RULE-036`.

**Catatan bentuk jawaban `closure-readiness`.** Endpoint ini sengaja mengembalikan **daftar syarat
yang belum terpenuhi**, bukan sekadar boleh atau tidak. Petugas admisi perlu tahu apa yang harus
dikejar, bukan hanya bahwa tombol tutup masih mati.

---

## Health Services / Billing Management / Deposit Rawat Inap

Base URL: `api/v1/health-services/billing-management/billing/patient-funds`

Bagian ini **milik `BillingManagement`**, bukan `InPatientManagement`. Ia dicantumkan di sini karena
`EPIC RI-35` bergantung padanya dan karena `EpisodeId` adalah kontrak lintas modulnya. Tidak boleh
ada controller deposit di area Rawat Inap.

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/deposits/{encounterId}` | Membaca akun deposit satu kunjungan | `BillingDeposit : Read` | – | `ApiResponse<BillingDepositResponse>` | ✅ **Tersedia** — `BillingPatientFundsController.cs:67` |
| `POST` | `/deposits/{encounterId}/top-ups` | Menerima deposit awal dan top-up. Header `Idempotency-Key` **wajib** | `BillingDeposit : Create` | `DepositTopUpRequest` | `ApiResponse<SettlementResponse>` | ✅ **Tersedia apa adanya** — `BillingPatientFundsController.cs:93`. Koreksi `0.6.1`: tidak perlu `episodeId` |
| `POST` | `/deposits/{encounterId}/allocations` | Mengalokasikan deposit ke tagihan | `BillingDeposit : Allocate` | `AllocateDepositRequest` | `ApiResponse<BillingAllocationResponse>` | ✅ **Tersedia apa adanya** — `BillingPatientFundsController.cs:31` |
| `GET` | `/deposit-policies` | Kebijakan deposit untuk kombinasi penjamin dan kelas perawatan | `BillingDeposit : Read` | `guarantorId`, `patientClassId` | `ApiResponse<DepositPolicyResponse>` | **Rencana `0.6.0`** |
| `GET` | `/deposits/episodes/{episodeId}` | Ringkasan deposit satu episode | `BillingDeposit : Read` | – | `ApiResponse<EpisodeDepositSummaryResponse>` | **Rencana `0.6.0`** |
| `POST` | `/deposits/episodes/{episodeId}/settle` | Alokasi deposit terhadap tagihan final beserta selisihnya | `BillingDeposit : Settle` | `SettleEpisodeDepositRequest` | `ApiResponse<EpisodeDepositSettlementResponse>` | **Rencana `0.6.0`** |
| ~~`POST`~~ | ~~`/deposits/episodes/{episodeId}/refunds`~~ | **Dicabut `0.6.1`** — bertabrakan dengan kontrak Billing yang sudah disetujui | – | – | – | ❌ **Dicabut** |
| `POST` | `/financial-exceptions/refunds` dan `/refunds/{id}/approve` | Refund kelebihan deposit beserta persetujuannya | `BillingRefund : Create` / `Approve` | Kontrak `BIL-API-0.4` | – | ✅ **Tersedia** — `BillingFinancialExceptionsController.cs:112,157` |
| `GET` | `/invoices/encounters/{encounterId}/charge-summary` | Rekap tagihan satu kunjungan; sumber angka tagihan final pada settlement | `BillingInvoice : Read` | – | `ApiResponse<EncounterChargeSummaryResponse>` | ✅ **Tersedia** — `BillingInvoicesController.cs:122` |

**Ringkasan episode wajib memuat dua angka kekurangan yang berbeda.** Kekurangan terhadap **minimum
kebijakan** dipakai langkah admisi dan daftar pantau; kekurangan terhadap **tagihan final** dipakai
settlement dan gerbang `FinancialClearance`. Menyatukan keduanya membuat episode yang depositnya
kurang tampak seperti episode yang tagihannya kurang.

---

## Health Services / Inpatient Management / Inpatient Census

Base URL: `api/v1/health-services/inpatient-management/census`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan penyaring census | `InpatientCensus : Read` | – | `ApiResponse<CensusFilterMetadataResponse>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |
| `GET` | `/summary` | Ringkasan jumlah pasien dirawat per unit layanan dan per kelas | `InpatientCensus : Read` | Query | `ApiResponse<CensusSummaryResponse>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |
| `GET` | `/` | Daftar pasien yang sedang dirawat beserta lokasi, DPJP, perawat, dan lama dirawat | `InpatientCensus : Read` | Query | `ApiResponse<CensusPagedResult>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |

---

## Health Services / Inpatient Management / Inpatient Monitoring

Base URL: `api/v1/health-services/inpatient-management/monitoring`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/pending-closures` | Daftar pantau episode yang sudah boleh pulang tetapi belum ditutup melewati ambang waktu | `InpatientMonitoring : Read` | Query | `ApiResponse<PendingClosurePagedResult>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |
| `GET` | `/closures-without-financial-clearance` | Daftar pantau episode yang ditutup menembus gerbang keuangan | `InpatientMonitoring : Read` | Query | `ApiResponse<OverrideClosurePagedResult>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |
| `GET` | `/deposit-shortfall` | Daftar pantau episode aktif yang depositnya masih di bawah minimum kebijakan, muncul kembali tiap kelipatan ambang tindak lanjut | `InpatientMonitoring : Read` | Query | `ApiResponse<DepositShortfallPagedResult>` | ✅ **Tersedia** — terpasang di `InpatientMonitoringController` 17 Sep 2026 |
| `GET` | `/unassigned-nurse-episodes` | Daftar episode aktif yang belum punya perawat penanggung jawab | `InpatientMonitoring : Read` | Query | `ApiResponse<UnassignedNursePagedResult>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |
| `GET` | `/bed-drift` | Laporan selisih antara salinan status tempat tidur dan catatan penempatan | `InpatientMonitoring : Read` | Query | `ApiResponse<BedDriftPagedResult>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |
| `GET` | `/isolation-mismatch` | Daftar pantau episode yang kebutuhan isolasinya tidak cocok dengan sifat tempat tidur yang sedang ditempati | `InpatientMonitoring : Read` | Query | `ApiResponse<IsolationMismatchPagedResult>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |

**Daftar pantau ketiga yang tidak ada di sini.** `RWI-RULE-023` menyebut tiga daftar pantau, dan
salah satunya adalah kepatuhan pengkajian awal dan verifikasi CPPT. Daftar itu **tidak** dirancang
pada revisi ini karena bergantung pada slice dokumentasi klinis yang masih menunggu `DEC-INP-001`.

---

## Health Services / Master Data / Inpatient Setting

Base URL: `api/v1/health-services/master-data/inpatient-settings`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Membaca pengaturan Rawat Inap yang berlaku | `InpatientSetting : Read` | – | `ApiResponse<InpatientSettingResponse>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |
| `PUT` | `/{id}` | Mengubah nilai pengaturan | `InpatientSetting : Update` | `UpdateInpatientSettingRequest` | `ApiResponse<InpatientSettingResponse>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |

---

## Health Services / Master Data / Inpatient Clearance Item

Base URL: `api/v1/health-services/master-data/inpatient-clearance-items`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar butir administrasi | `InpatientClearanceItem : Read` | Query | `ApiResponse<InpatientClearanceItemPagedResult>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |
| `GET` | `/{id}` | Detail satu butir | `InpatientClearanceItem : Read` | – | `ApiResponse<InpatientClearanceItemResponse>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |
| `POST` | `/` | Menambah butir baru | `InpatientClearanceItem : Create` | `CreateInpatientClearanceItemRequest` | `ApiResponse<InpatientClearanceItemResponse>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |
| `PUT` | `/{id}` | Mengubah butir | `InpatientClearanceItem : Update` | `UpdateInpatientClearanceItemRequest` | `ApiResponse<InpatientClearanceItemResponse>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |
| `PATCH` | `/{id}/status` | Mengaktifkan atau menonaktifkan butir | `InpatientClearanceItem : Update` | `UpdateStatusRequest` | `ApiResponse<InpatientClearanceItemResponse>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |
| `DELETE` | `/{id}` | Menandai butir terhapus | `InpatientClearanceItem : Delete` | `DeleteRequest` | `ApiResponse<InpatientClearanceItemResponse>` | ✅ **Tersedia** — terbukti berjalan 26 Agu 2026 |

---

## 7. Perubahan pada endpoint yang sudah ada

### Health Services / Master Data / Bed

Base URL: `api/v1/health-services/master-data/beds`
Sumber as-is: `Areas/HealthServices/MasterData/Controllers/BedController.cs` pada SHA `5afb54b`

| Method | Path | Perubahan | Alasan | Status |
| --- | --- | --- | --- | --- |
| `PATCH` | `/{id}/availability` | Menolak nilai `Reserved` dan `Occupied` dengan kode 422, juga menolak saat tempat tidur masih ditempati. Nilai `Available`, `Cleaning`, `Maintenance`, `Blocked`, dan `Inactive` tetap diterima | `RWI-RULE-027` aturan 4 dan 5: status penghunian hanya boleh lahir dari tindakan Rawat Inap | ✅ **Diterapkan** — `BE-RWI-006` pada 1 Sep 2026, dengan test regresi `BE-RWI-032` |

Pesan penolakannya: *"Status Terisi dan Dipesan hanya dapat diubah lewat modul Rawat Inap. Untuk
menutup tempat tidur sementara, pakai status Pembersihan, Perbaikan, atau Diblokir."*

**Yang tidak berubah:** bentuk request, bentuk response, kode status yang sudah ada, dan seluruh
endpoint lain pada grup ini. Ini perubahan perilaku, bukan perubahan kontrak.

**Persetujuan:** Pemilik `MasterData` HealthServices, tercatat sebagai `RWI-OQ-033` — **sudah diberikan** 21 Agustus 2026 lewat `RWI-DEC-062`. Diterapkan `BE-RWI-006` pada 1 September 2026, bersama test regresi
`BE-RWI-032`.

---

## 8. Yang sengaja tidak dibuat

| Endpoint yang tidak dibuat | Alasan |
| --- | --- |
| Endpoint apa pun untuk mengubah atau menghapus `InpStatusHistory` | Riwayat status tidak dapat diubah dan tidak dapat dihapus, sesuai `RWI-RULE-031` aturan 5 |
| Endpoint apa pun untuk mengubah atau menghapus `InpDischargeSummaryRevision` | Salinan versi resume juga tidak dapat diubah dan tidak dapat dihapus, sesuai `RWI-DEC-057` |
| Endpoint untuk membatalkan pencatatan kepergian fisik | `RWI-RULE-036` menetapkan tidak ada pembatalan. Pasien yang ternyata belum jadi pulang menjalani admisi baru |
| `PATCH /episodes/{id}/status` yang menerima status bebas | Melanggar `RWI-RULE-031` aturan 4 tentang satu pintu. Setiap perpindahan status punya endpoint bermakna sendiri |
| Endpoint pengkajian, catatan dokter, tindakan, dan resep | Memakai modul Clinical dan Pharmacy yang sudah ada. Menunggu `DEC-INP-001` |
| Endpoint serah terima dari IGD | Menunggu `DEC-INP-002` |
| Endpoint pengiriman SATUSEHAT | Menunggu `DEC-INP-005` |

Baris kedua adalah yang paling perlu diperhatikan. Pola `PATCH /{id}/status` memang dipakai hampir
seluruh master di repository ini, tetapi untuk episode rawat inap pola itu **tidak dipakai**, karena
akan membuat status dapat disetel ke nilai apa pun tanpa memeriksa aturan perpindahan — persis
cacat yang sudah ditemukan pada `PatientEncounterController` dan tercatat sebagai `RWI-TF-007`.

---

## 9. Traceability

| Grup endpoint | Requirement dan decision asal |
| --- | --- |
| Inpatient Episode | `RWI-RULE-003`, `RWI-RULE-004`, `RWI-RULE-005`, `RWI-RULE-020`, `RWI-RULE-030`, `RWI-RULE-033` |
| Bed Occupancy | `RWI-RULE-001`, `RWI-RULE-002`, `RWI-RULE-006`, `RWI-RULE-007`, `RWI-RULE-008`, `RWI-RULE-012`, `RWI-RULE-015`, `RWI-RULE-027` |
| Inpatient Discharge | `RWI-RULE-009`, `RWI-RULE-010`, `RWI-RULE-011`, `RWI-RULE-018`, `RWI-RULE-028`, `RWI-RULE-032`, `RWI-RULE-036` |
| Inpatient Census | `RWI-RULE-019`, CAP-008 |
| Inpatient Monitoring | `RWI-RULE-023`, `RWI-RULE-027` aturan 6 |
| Master Data Inpatient Setting dan Clearance Item | `RWI-RULE-018`, `RWI-RULE-034` |
| Perubahan Bed | `RWI-RULE-027`, `RWI-DEC-039` |

---

## 10. Perubahan pada `contract_version` `0.9.0` — amandemen terbatas penyelarasan `PRD-RWI-V2-001` ★ 15 September 2026

| Field | Nilai |
| --- | --- |
| Status | **`draft`** — belum disetujui manusia |
| `input_revision` | `02-backend-architecture.md` `0.8` bagian 11; `data/data-dictionary.md` `0.5` bagian 18; decision log `21` |
| Dampak kompatibilitas | **Aditif** untuk query, isian, dan endpoint baru. **Perubahan perilaku** pada `close` dan `close-with-override`: penutupan kini ikut mengunci konsep, membatalkan pesanan tindakan tertunda, dan membatalkan dosis obat masa depan. Bentuk request tidak berubah; response bertambah `SideEffects` |
| Keputusan | `RWI-DEC-111`, `112`, `130`, `138`, `143` |

### 10.1 Health Services / Inpatient Management / Inpatient Census — query baru

Base URL: `api/v1/health-services/inpatient-management/census`
Judul grup: `[Tags("Health Services / Inpatient Management / Inpatient Census")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Census. Dengan `assignedToMe=true`: hanya pasien yang dokter login punya penugasan aktif sebagai DPJP, konsulen, atau dokter jaga; `DoctorId` query diabaikan | `InpatientCensus : Read` | `CensusQuery` + **`AssignedToMe`** (`bool`, bawaan `false`) | `ApiResponse<PagedResult<CensusItemResponse>>` + **`MyAssignmentRole`**, **`MyAssignmentPurpose`** | ✅ **Tersedia**, query dan isian **Rencana (belum tersedia)** |
| `GET` | `/summary` | Angka dari daftar yang sama; bertambah `NeedsReviewCount` bila `assignedToMe=true` | `InpatientCensus : Read` | Sama | `ApiResponse<CensusSummaryResponse>` + **`NeedsReviewCount`** | ✅ **Tersedia**, query dan isian **Rencana** |

Contoh: dr. Ahmad DPJP Budi dan konsulen Sari; 120 pasien lain dirawat → `TotalCount = 2`; baris Sari
`MyAssignmentRole = Consultant`, kolom DPJP tetap "dr. Rina". Pengguna tanpa data dokter → daftar kosong,
`Message` "Akun Anda tidak terhubung dengan data dokter" — **bukan** `403`, karena hak baca census tetap sah.

### 10.2 Health Services / Inpatient Management / Inpatient Episode — penugasan pendukung

Base URL: `api/v1/health-services/inpatient-management/episodes`
Judul grup: `[Tags("Health Services / Inpatient Management / Inpatient Episode")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/{id}/doctor-assignments/supporting` | Kepala ruangan atau supervisor melibatkan konsulen, memanggil dokter jaga, atau membuat **penugasan singkat penulisan catatan terlambat** | `InpatientEpisode : Update` + penjaga kepala ruangan/supervisor | `AssignSupportingDoctorRequest` (`DoctorId`, `AssignmentRole` `Consultant`/`OnCallDoctor`, `AssignmentPurpose` `Regular`/`LateDocumentation`, `StartDateTime`, `EndDateTime`, `Reason` wajib) + `Idempotency-Key` | `ApiResponse<InpatientDoctorAssignmentResponse>` + `AssignmentPurpose` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/doctor-assignments/{assignmentId}/end` | Mengakhiri konsulen atau dokter jaga | Sama | `EndSupportingAssignmentRequest` (`EndDateTime`, `Reason` opsional) | Sama | **Rencana (belum tersedia)** |
| `GET` | `/{id}/doctor-assignments` | Riwayat penugasan. **Perubahan:** bertambah `AssignmentPurpose` | `InpatientEpisode : Read` | — | Sama | ✅ **Tersedia**, isian **Rencana** |

Contoh `LateDocumentation`: dr. Rina, `OnCallDoctor`, Kamis 10.00–11.00, alasan "penulisan kajian medis Selasa 15.00" →
`201`. Tanpa `EndDateTime` → `400` `VAL-INP-01`.

| Kode | Artinya bagi pengguna |
| --- | --- |
| `400` | Waktu selesai wajib untuk penugasan singkat; alasan kosong; waktu selesai sebelum waktu mulai |
| `403` | Hanya kepala ruangan atau supervisor yang dapat menugaskan dokter pendukung |
| `404` | Episode atau penugasan tidak ditemukan |
| `409` | Dokter ini sudah punya penugasan aktif dengan peran yang sama pada periode itu; penugasan sudah berakhir; penugasan DPJP diakhiri lewat pengalihan DPJP |
| `422` | Episode tidak berstatus `Admitted` atau `DischargePending`; penugasan singkat berperan selain dokter jaga; dokter tidak aktif |

### 10.3 Health Services / Inpatient Management / Inpatient Discharge — resume, penutupan

Base URL: `api/v1/health-services/inpatient-management/discharges`
Judul grup: `[Tags("Health Services / Inpatient Management / Inpatient Discharge")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/{episodeId}/summary` | Resume. **Perubahan:** tiga isian | `InpatientDischarge : Read` | Query `includeRevisions` | `DischargeSummaryResponse` + `ImportantFindingsSummary`, `DischargeConditionNote`, `EducationSummary` | ✅ **Tersedia**, isian **Rencana** |
| `PUT` | `/{episodeId}/summary` | Simpan draf. **Perubahan:** tiga isian | `InpatientDischarge : Update` | `UpsertDischargeSummaryRequest` + tiga isian | Sama | ✅ **Tersedia**, isian **Rencana** |
| `GET` | `/{episodeId}/summary-prefill` | Usulan isian dari sumber klinis, **tidak menyimpan** | `InpatientDischarge : Read` | — | `ApiResponse<DischargeSummaryPrefillResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{episodeId}/closure-readiness` | **Perubahan:** bertambah `Warnings[]` yang tidak mempengaruhi `CanClose` | `InpatientDischarge : Read` | — | `ClosureReadinessResponse` + `Warnings[]` (`Code`, `Count`, `Message`, `Details[]`) | ✅ **Tersedia**, isian **Rencana** |
| `POST` | `/{episodeId}/close` | **Perilaku baru:** langkah 4–6 `02-backend-architecture.md` 11.5.4 dalam transaksi yang sama | `InpatientDischarge : Close` | Tidak berubah | `InpatientEpisodeDetailResponse` + **`SideEffects`** (`LockedDraftCount`, `CancelledProcedureOrderCount`, `BilledPendingProcedureOrderCount`, `CancelledFutureDoseCount`) | ✅ **Tersedia**, perilaku **Rencana** |
| `POST` | `/{episodeId}/close-with-override` | Sama | `InpatientDischarge : CloseOverride` | Tidak berubah | Sama | ✅ **Tersedia**, perilaku **Rencana** |

**`DischargeSummaryPrefillResponse`** — satu objek per isian:

| Isian | `Value` | `Sources[]` | `SourceStatus` |
| --- | --- | --- | --- |
| `PrimaryDiagnosisText`, `SecondaryDiagnosisText` | Diagnosis kerja/akhir encounter | Kode, dokter, waktu | `Available`/`Empty`/`Unavailable` |
| `ProcedureSummary` | Tindakan `Completed` episode | Nama, pelaksana, waktu | Sama |
| `DischargeMedicationNote` | Butir resep pulang | Nomor resep, dokter | Sama |
| `ImportantFindingsSummary` | Hasil laboratorium/radiologi final yang kritis atau abnormal | Nama pemeriksaan, nilai, waktu | Sama |
| `EducationSummary` | Materi dari Assesment Edukasi selesai | Perawat, waktu | Sama |
| `ClinicalSummary`, `DischargeConditionNote`, `FollowUpInstruction` | Tidak diusulkan | — | `Empty` |

Contoh `closure-readiness` Joko 12.55: `CanClose = true`; `Warnings`: 1 konsep SOAP dr. Yoga akan terkunci; 1 pesanan cek
GDS akan batal; 1 dosis 08.00 belum dicatat.

| Kode | Artinya bagi pengguna |
| --- | --- |
| `500` pada penutupan | "Penutupan gagal disimpan, coba lagi" — tidak ada satu pun langkah yang tersimpan; aman diulang |

### 10.4 Health Services / Inpatient Management / Inpatient Monitoring — daftar pantau baru

Base URL: `api/v1/health-services/inpatient-management/monitoring`
Judul grup: `[Tags("Health Services / Inpatient Management / Inpatient Monitoring")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/billed-pending-procedure-orders` | Pesanan tindakan tertunda **yang sudah ditagih** pada episode `Closed` — tidak dibatalkan saat penutupan dan perlu ditindaklanjuti bersama Billing | `InpatientMonitoring : Read` | Query `serviceUnitId`, `closedFrom`, `closedTo`, `pageNumber`, `pageSize` | `ApiResponse<PagedResult<BilledPendingProcedureOrderItem>>` (pasien, episode, tindakan, penginput, waktu pesan, waktu tutup, nomor tagihan) | **Rencana (belum tersedia)** |

### 10.5 Yang tidak ada di kontrak `0.9.0`

| Tidak ada | Alasan |
| --- | --- |
| Endpoint membuat penugasan oleh dokter sendiri | `RWI-DEC-130` (4) |
| Endpoint menyimpan usulan isian resume | `RWI-DEC-112` |
| Endpoint Resume ODC | `RWI-DEC-123` |
| Endpoint "Catatan Saya" | `RWI-DEC-142` |
| Membatalkan pesanan tertagih dari Rawat Inap | `RWI-DEC-143` (c) |

---

## 11. Perubahan pada `contract_version` `0.10.0` — Finishing Rawat Inap ★ 1 Oktober 2026

| Field | Nilai |
|---|---|
| `last_changed_in` | `0.10.0` |
| Status | **`approved`** — Muhammad Hamzah, 2026-10-02 (`RWI-DEC-221`) |
| Owner | Muhammad Hamzah (Rawat Inap dan Clinical); Ikbal Yulianto (Kamar Operasi — **disetujui Ikbal Yulianto, `RWI-DEC-208`**); `MasterData` milik seluruh tim (`RWI-DEC-193`); Billing untuk `SourceContext = OPERATING_ROOM` (`RWI-DEC-192`, `196`) |
| `input_revision` | `02-backend-architecture.md` `0.9` bagian 12; `data/data-dictionary.md` bagian 19; decision log revision `30`; PRD Finishing v`0.4`; gate `1.9` |
| Dampak kompatibilitas | **Aditif** untuk endpoint dan isian baru. **Perubahan permission** pada dua endpoint serah terima OK (`Update` → `Send`/`Receive`). **Perubahan perilaku**: penundaan kasus menandai pra-operasi "perlu diperbarui"; gerbang "Siap" bertambah syarat; penerimaan serah terima memeriksa penerima dan bed; `POST episodes` menolak admisi pasien yang punya permintaan admisi `Pending` tanpa merujuknya |
| Traceability | `FR-RWF-040` s.d. `049`, `071`, `080` s.d. `082`, `086` s.d. `090`; `RWI-DEC-173` s.d. `177`, `182`, `189`, `196`, `199`, `201`, `204`, `205` |

Respons sukses selalu `ApiResponse<T>`. Seluruh endpoint dan isian baru berlabel **Rencana (belum tersedia)**. Respons yang dibaca perawat **tidak pernah** memuat rupiah.

### 11.1 Endpoint yang sudah ada dan dipakai layar baru

| Tag | Method dan path | Kegunaan | Hak akses | Status |
|---|---|---|---|---|
| `Health Services / Operating Room Management / Cases` | `GET api/v1/health-services/operating-room-management/cases?encounterId=` | Daftar Pesanan Ruang Bedah di bangsal (`FE-INP-26`) | `OperatingRoomCase : Read` | ✅ Tersedia; respons **ditambah** (11.5.1) |
| `Health Services / Operating Room Management / Cases` | `GET …/cases/{id}/schedule/history` | Riwayat jadwal dan penundaan | `OperatingRoomCase : Read` | ✅ Tersedia, tetap |
| `Health Services / Clinical Management / Patient Procedure` | `GET …/patient-procedures?encounterId=&procedureStatus=` | Pilihan order tindakan operasi aktif saat memesan | `PatientProcedure : Read` | ✅ Tersedia, tetap |
| `Health Services / Inpatient Management / Bed Occupancy` | `POST …/bed-occupancies/placements/transfer` | Pindah pasien sebelum menerima serah terima ke unit lain; transfer antarunit kini juga membuat dokumen serah terima transfer (11.8) | `InpatientBedOccupancy : Transfer` | ✅ Tersedia; efek sesudah commit **Rencana** |

### 11.2 Health Services / Inpatient Management / Inpatient Surgery Booking — grup baru

Base URL: `api/v1/health-services/inpatient-management/episodes`
Judul grup: `[Tags("Health Services / Inpatient Management / Inpatient Surgery Booking")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `POST` | `/{episodeId}/surgery-bookings` | Pesan ruang bedah dari bangsal, tab Bedah Operasi atau Bedah Obgyn. Membuat kasus OK `Requested` | `OperatingRoomCase : Create` | `SurgeryBookingRequest` + header `Idempotency-Key` | `ApiResponse<OprCaseResponse>` | **Rencana (belum tersedia)** |

**`SurgeryBookingRequest`**

| Field | Tipe | Wajib | Aturan |
|---|---|:---:|---|
| `BookingTab` | `string` | Ya | `Surgery` atau `Obstetric`. `Obstetric` memaksa `SurgicalServiceType = Obstetric` |
| `PatientProcedureId` | `Guid` | Ya | Tepat satu order tindakan operasi berstatus aktif milik kunjungan episode |
| `PreferredAt` | `DateTime` | Ya | Tanggal dan jam yang diinginkan; tidak di masa lalu lebih dari 15 menit |
| `PlannedAnesthesiaType` | `string` | Ya | `General`, `Regional`, `Local`, `Sedation` |
| `Priority` | `string` | Ya | `Routine`, `Urgent`, `Emergency` |
| `CaseType` | `string` | Ya | `Elective`, `Emergency` |
| `Indication` | `string` | Ya | Maks. 4000 |
| `Laterality` | `string` | Tidak | `Left`, `Right`, `Bilateral`, `NotApplicable` |
| `EstimatedMinutes` | `int` | Ya | 1–1440 |
| `Note` | `string` | Tidak | Maks. 1000 |

Dokter operator diambil dari order tindakan; `RequesterDoctorId` dari dokter pemesan order; penginput dari akun login.

Contoh: Budi S., Melati 302/2, order "Appendektomi" aktif → `BookingTab = Surgery`, `PreferredAt = 2026-10-03T08:00`, `PlannedAnesthesiaType = General`, `Laterality = NotApplicable` → `201`, kasus `OK-2026-0142` `Requested`.

| Kode | Artinya bagi pengguna |
|---|---|
| `400` | Isian wajib kosong; nilai pilihan tidak dikenal |
| `403` | Tidak berhak memesan ruang bedah |
| `404` | Episode atau order tindakan tidak ditemukan |
| `409` | Kunci idempotensi dipakai dengan isi berbeda |
| `422` `INP-SRG-001` | "Tindakan operasi belum dipesan dokter" — order tidak ada, tidak aktif, atau bukan milik kunjungan episode ini |
| `422` `INP-SRG-002` | Episode bukan `Admitted` |

### 11.3 Health Services / Operating Room Management / Preparation — Catatan Pra-Operasi bangsal

Base URL: `api/v1/health-services/operating-room-management/cases/{caseId}/preparation`
Judul grup: `[Tags("Health Services / Operating Room Management / Preparation")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/ward-pre-op` | Versi pra-operasi terbaru beserta butir dan penandaan | `OperatingRoomWardPreOp : Read` | — | `ApiResponse<WardPreOpResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/ward-pre-op/versions` | Seluruh versi, termasuk yang "perlu diperbarui" dan yang digantikan | `OperatingRoomWardPreOp : Read` | — | `ApiResponse<List<WardPreOpVersionSummary>>` | **Rencana (belum tersedia)** |
| `PUT` | `/ward-pre-op/draft` | Simpan draf pengirim. Membuat versi baru bila versi terbaru `NeedsUpdate`, menyalin butir lama sebagai usulan | `OperatingRoomWardPreOp : Send` | `SaveWardPreOpDraftRequest` | `ApiResponse<WardPreOpResponse>` | **Rencana (belum tersedia)** |
| `PATCH` | `/ward-pre-op/send` | Kirim. Server membekukan tanda vital dan nyeri terakhir sebagai potret | `OperatingRoomWardPreOp : Send` | `{ ExpectedVersion }` + `Idempotency-Key` | Sama | **Rencana (belum tersedia)** |
| `PATCH` | `/ward-pre-op/confirm` | Konfirmasi penerima per butir dan penandaan | `OperatingRoomWardPreOp : Confirm` | `ConfirmWardPreOpRequest` | Sama | **Rencana (belum tersedia)** |
| `GET` | `/` | Kesiapan kasus. **Perubahan:** `Blockers[]` bertambah kode `WARD_PRE_OP_INCOMPLETE`, `WARD_PRE_OP_NEEDS_UPDATE` | `OperatingRoomPreparation : Read` | — | Tetap + kode baru | ✅ Tersedia, isian **Rencana** |

**`SaveWardPreOpDraftRequest`**: `Items[]` (`PreparationItemId`, `SenderConfirmed` `bool`, `Note` maks. 500), `SiteMarks[]` (`BodyView` `Front`/`Back`/`Left`/`Right`, `X` dan `Y` 0–100, `Label` maks. 100), `MarkingLaterality` (`Left`/`Right`/`Bilateral`/`NotApplicable`), `MarkingLocationNote` (maks. 500), `ExpectedVersion`.

**`ConfirmWardPreOpRequest`**: `Items[]` (`ItemId`, `ReceiverConfirmed`, `Note`), `SiteMarkingConfirmed` (`bool`), `ExpectedVersion`, `Idempotency-Key`.

**`WardPreOpResponse`**: `Id`, `VersionNumber`, `Status`, `VitalSnapshot` (`SystolicBp`, `DiastolicBp`, `PulseRate`, `RespiratoryRate`, `Temperature`, `SpO2`, `RecordedAt`), `PainSnapshot` (`Score`, `ScaleName`, `RecordedAt`), `Items[]` (`ItemId`, `GroupName`, `ItemName`, `IsMandatory`, `SenderConfirmed`, `ReceiverConfirmed`, `Note`), `SiteMarks[]`, `MarkingLaterality`, `CaseLaterality`, `SentByName`, `SentAt`, `ConfirmedByName`, `ConfirmedAt`, `PreviousVersionId`, `Version`.

Contoh: versi 1 `Confirmed`; kasus ditunda → versi 1 `NeedsUpdate`; perawat bangsal `PUT /ward-pre-op/draft` → versi 2 `Draft` dengan butir versi 1 sebagai usulan; `PATCH /send` → potret TD 130/85 dari pencatatan terbaru.

| Kode | Artinya bagi pengguna |
|---|---|
| `403` | Tidak berhak mengirim atau mengonfirmasi |
| `409` | Versi berubah |
| `422` `OPR-WPO-001` | "Sisi penandaan berbeda dengan sisi pada pesanan operasi" |
| `422` `OPR-WPO-002` | Pengirim dan penerima harus akun berbeda |
| `422` `OPR-WPO-003` | Butir wajib pengirim belum dikonfirmasi saat mengirim |
| `422` `OPR-WPO-004` | Kasus `Rejected`, `Cancelled`, `InProgress`, atau `Completed` — pra-operasi tidak dapat diubah |
| `422` `OPR-WPO-005` | Belum ada tanda vital tercatat untuk episode ini |

### 11.4 Health Services / Master Data / Surgical Preparation Item — grup baru

Base URL: `api/v1/health-services/master-data/surgical-preparation-items`
Judul grup: `[Tags("Health Services / Master Data / Surgical Preparation Item")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar butir; saringan kelompok dan aktif | `SurgicalPreparationItem : Read` | `PagedQuery { Search?, GroupName?, IsActive? }` | `ApiResponse<PagedResult<SurgicalPreparationItemResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Detail | `SurgicalPreparationItem : Read` | — | `ApiResponse<SurgicalPreparationItemResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Tambah butir | `SurgicalPreparationItem : Create` | `{ Code, GroupName, ItemName, IsMandatory, SortOrder, Description? }` | Sama | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Ubah | `SurgicalPreparationItem : Update` | Sama + `RowVersion` | Sama | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/status` | Aktif/nonaktif. Butir nonaktif tidak muncul di versi baru; versi lama tetap utuh | `SurgicalPreparationItem : Update` | `{ IsActive }` | Sama | **Rencana (belum tersedia)** |

Kode khusus: `409` `MST-SPI-001` kode sudah dipakai.

### 11.5 Kamar Operasi — kasus, serah terima, dan daftar serah terima

#### 11.5.1 Health Services / Operating Room Management / Cases — perubahan

Base URL: `api/v1/health-services/operating-room-management/cases`
Judul grup: `[Tags("Health Services / Operating Room Management / Cases")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `POST` | `/` | **Perubahan:** menerima `SurgicalServiceType` (bawaan `General`) dan `PlannedAnesthesiaType` (opsional) | `OperatingRoomCase : Create` | `CreateOprCaseRequest` + dua isian | `OprCaseResponse` | ✅ Tersedia, isian **Rencana** |
| `GET` | `/`, `/{id}` | **Perubahan:** respons bertambah `SurgicalServiceType`, `PlannedAnesthesiaType`, `RejectedAt`, `RejectedByName`, `RejectionReason`, `LastStatusReason` (alasan tunda atau batal terakhir), `WardPreOpStatus`, `HandoverStatus` | `OperatingRoomCase : Read` | Query tetap; `Status` menerima `Rejected` | Tetap + isian | ✅ Tersedia, isian **Rencana** |
| `PATCH` | `/{id}/reject` | Tolak order dari `Requested` dengan alasan; status akhir `Rejected` | `OperatingRoomCase : Reject` | `{ Reason (wajib, 10–500), ExpectedVersion }` + `Idempotency-Key` | `ApiResponse<OprCaseResponse>` | **Rencana (belum tersedia)** — disetujui `RWI-DEC-208` |
| `GET` | `/{id}/post-operative-summary` | Ringkasan operasi baca-saja untuk bangsal | `OperatingRoomCase : Read` | — | `ApiResponse<PostOperativeSummaryResponse>` | **Rencana (belum tersedia)** |

**`PostOperativeSummaryResponse`**: `CaseId`, `CaseNumber`, `ProcedureNames[]`, `PrimarySurgeonName`, `ReportFinal` (`bool`), dan bila `ReportFinal = true`: `PostDiagnosis`, `Findings`, `Complications`, `BloodLossMl`, `ImplantDrainNote`, `PostPlan`, `FinishedAt`; `AnesthesiaTechnique`, `PlannedAnesthesiaType`; `RecoveryScoreSystem`, `RecoveryScoreValue`, `RecoveryDecision`; `HandoverInstructionSummary`, `HandoverStatus`. Bila `ReportFinal = false`, isian klinis `null` dan `Message` "Laporan operasi belum final".

| Kode `PATCH /reject` | Artinya bagi pengguna |
|---|---|
| `400` | Alasan kosong atau kurang dari 10 karakter |
| `403` | Tidak berhak menolak order operasi |
| `409` | Versi berubah |
| `422` `OPR-CASE-REJ-001` | Hanya kasus berstatus Diminta yang dapat ditolak |

Tindakan pada kasus `Rejected` (`PUT /{id}`, `PATCH /{id}/schedule`, `/postpone`, `/cancel`, `/start`) → `422` `OPR-CASE-REJ-002` "Kasus yang ditolak tidak dapat diubah; pesan ulang sebagai kasus baru".

#### 11.5.2 Health Services / Operating Room Management / Execution — serah terima

Base URL: `api/v1/health-services/operating-room-management/cases/{caseId}/execution`
Judul grup: `[Tags("Health Services / Operating Room Management / Execution")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `POST` | `/handovers` | Kirim serah terima. **Perubahan permission** | ~~`OperatingRoomHandover : Update`~~ → `OperatingRoomHandover : Send` | Tetap | Tetap | ✅ Tersedia, permission **Rencana** |
| `PATCH` | `/handovers/{handoverId}/accept` | Terima atau tolak. **Perubahan permission dan aturan:** penerima ≠ pengirim; pasien menempati bed aktif di unit tujuan | ~~`OperatingRoomHandover : Update`~~ → `OperatingRoomHandover : Receive` | Tetap (`Accept`, `RejectionReason`, `IdempotencyKey`) | Tetap | ✅ Tersedia, aturan **Rencana** |
| `PUT` | `/recovery` | Simpan kamar pulih. **Perubahan perilaku:** keputusan `Inpatient`/`Icu` untuk pasien tanpa episode hadir membuat permintaan admisi; keputusan berubah dari itu membatalkannya | `OperatingRoomAnesthesia : Update` | Tetap | Tetap + `AdmissionReferralState` (`NotNeeded`, `Created`, `Cancelled`) | ✅ Tersedia, perilaku **Rencana** — disetujui `RWI-DEC-208` |

| Kode tambahan `accept` | Artinya bagi pengguna |
|---|---|
| `403` | Tidak berhak menerima serah terima |
| `422` `OPR-HO-001` | "Pindahkan pasien ke tempat tidur di unit ini lewat Transfer Pasien sebelum menerima serah terima" |
| `422` `OPR-HO-002` | Pengirim tidak dapat menerima serah terima sendiri |

#### 11.5.3 Health Services / Operating Room Management / Handovers — grup baru, baca saja

Base URL: `api/v1/health-services/operating-room-management/handovers`
Judul grup: `[Tags("Health Services / Operating Room Management / Handovers")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Serah terima per unit tujuan dan status; `overdueOnly` memakai `MstInpatientSetting.PendingSurgicalHandoverAlertMinutes` | `OperatingRoomHandover : Read` | `{ DestinationUnitId?, Status?, OverdueOnly?, PageNumber, PageSize }` | `ApiResponse<PagedResult<HandoverQueueItemResponse>>` | **Rencana (belum tersedia)** |

`HandoverQueueItemResponse`: `HandoverId`, `CaseId`, `CaseNumber`, `PatientName`, `MedicalRecordNumber`, `DestinationUnitName`, `CurrentUnitName`, `PatientInDestinationUnit` (`bool`), `Status`, `SentByName`, `SentAt`, `WaitingMinutes`, `IsOverdue`.

### 11.6 Health Services / Inpatient Management / Inpatient Admission Referral — grup baru

Base URL: `api/v1/health-services/inpatient-management/admission-referrals`
Judul grup: `[Tags("Health Services / Inpatient Management / Inpatient Admission Referral")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar permintaan admisi dari kamar pulih | `InpatientAdmissionReferral : Read` | `{ Status? (bawaan Pending), OverdueOnly?, Search?, PageNumber, PageSize }` | `ApiResponse<PagedResult<AdmissionReferralResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Detail untuk mengisi awal admisi berlangkah | `InpatientAdmissionReferral : Read` | — | `ApiResponse<AdmissionReferralResponse>` | **Rencana (belum tersedia)** |

Membuat dan membatalkan permintaan **tidak** punya endpoint; keduanya dipanggil service Kamar Operasi dalam proses yang sama saat `PUT …/execution/recovery` (11.5.2).

`AdmissionReferralResponse`: `Id`, `PatientId`, `PatientName`, `MedicalRecordNumber`, `SourceEncounterId`, `SourceEncounterType` (`Outpatient`, `Emergency`, `OneDayCare`), `OprCaseId`, `CaseNumber`, `ProcedureNames[]`, `PrimarySurgeonId`, `PrimarySurgeonName`, `RequestedCareLevel` (`Inpatient`, `Icu`), `RecoveryDecisionNote`, `Status`, `RequestedAt`, `WaitingMinutes`, `IsOverdue`, `CancelledReason`, `CompletedEpisodeId`.

#### Perubahan pada `Inpatient Episode`

| Method | Path | Perubahan | Hak akses | Status |
|---|---|---|---|---|
| `POST` | `api/v1/health-services/inpatient-management/episodes` | `OpenAdmissionRequest` bertambah `AdmissionReferralId` (opsional). Bila diisi: permintaan wajib `Pending` dan milik pasien yang sama; kunjungan asal dirujuk seperti alih IGD; permintaan menjadi `Completed` dalam transaksi yang sama. Bila kosong padahal pasien punya permintaan `Pending` → ditolak | `InpatientEpisode : Create` | ✅ Tersedia, isian **Rencana** |

| Kode | Artinya bagi pengguna |
|---|---|
| `409` `INP-ADM-REF-001` | "Pasien punya permintaan admisi dari kamar pulih; buka admisi dari permintaan itu" |
| `422` `INP-ADM-REF-002` | Permintaan sudah selesai atau dibatalkan |

### 11.7 Health Services / Inpatient Management / Inpatient Report — grup baru (`P2`)

Base URL: `api/v1/health-services/inpatient-management/reports`
Judul grup: `[Tags("Health Services / Inpatient Management / Inpatient Report")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/room-transfers` | Laporan transfer ruangan per periode | `InpatientReport : ReadRoomTransfer` | `RoomTransferReportQuery { PeriodFrom (wajib), PeriodTo (wajib, ≤ 31 hari), FromServiceUnitId?, ToServiceUnitId?, ClassId?, IncludeCorrections (bawaan true), PageNumber, PageSize }` | `ApiResponse<PagedResult<RoomTransferReportRow>>` | **Rencana (belum tersedia)** |
| `GET` | `/room-transfers/export` | Ekspor Excel dengan saringan yang sama; dicatat audit | `InpatientReport : ExportRoomTransfer` | Sama tanpa paging | Berkas `.xlsx` | **Rencana (belum tersedia)** |

`RoomTransferReportRow`: `TransferredAt`, `MedicalRecordNumber`, `PatientName`, `FromClassName`, `FromRoomName`, `FromBedNumber`, `ToClassName`, `ToRoomName`, `ToBedNumber`, `Reason`, `RecordedByName`, `EntryKind` (`Transfer`, `Correction`).

Kode: `400` periode kosong atau lebih dari 31 hari; `403` tanpa permission laporan, apa pun nama perannya.

### 11.8 Health Services / Clinical Management / Transfer Handover — grup baru (`P2`)

Base URL: `api/v1/health-services/clinical-management/transfer-handovers`
Judul grup: `[Tags("Health Services / Clinical Management / Transfer Handover")]`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Dokumen per episode atau per unit, saringan status | `TransferHandover : Read` | `{ EpisodeId?, ServiceUnitId?, Status? }` | `ApiResponse<List<TransferHandoverResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Detail sembilan bagian | `TransferHandover : Read` | — | `ApiResponse<TransferHandoverResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}/draft` | Lengkapi bagian yang diisi pengirim | `TransferHandover : Send` | `SaveTransferHandoverDraftRequest` | Sama | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/send` | Kirim; server membekukan potret klinis | `TransferHandover : Send` | `{ ExpectedVersion }` | Sama | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/accept` | Terima atau tolak beralasan | `TransferHandover : Receive` | `{ Accept, RejectionReason?, ExpectedVersion }` | Sama | **Rencana (belum tersedia)** |

Pembuatan dokumen tidak punya endpoint: dibuat oleh transfer antarunit (11.1).

`SaveTransferHandoverDraftRequest`: `SoapSummary` (maks. 4000), `HandedItems` (maks. 2000), `SpecialInstructions` (maks. 2000), `ExpectedVersion`. GCS, tanda vital, nyeri, risiko jatuh, dan balance cairan dirujuk dari pencatatan terakhir lalu dibekukan saat dikirim; tidak diketik.

Kode: `422` `CLI-TRH-001` penerima sama dengan pengirim; `422` `CLI-TRH-002` penerima harus bertugas di unit tujuan (bed pasien aktif di unit itu); `400` alasan tolak kosong; `409` versi berubah.

### 11.9 Perubahan kecil lain

| Tag | Perubahan | Hak akses | Status |
|---|---|---|---|
| `Health Services / Master Data / Inpatient Setting` | Respons dan `PUT` bertambah `PendingSurgicalHandoverAlertMinutes` (1–1440, bawaan 60) dan `PendingAdmissionReferralAlertMinutes` (1–1440, bawaan 30) | `InpatientSetting : Read`, `: Update` | ✅ Tersedia, isian **Rencana** |
| `Health Services / Inpatient Management / Inpatient Monitoring` | `GET /monitoring/pending-surgical-handovers` dan `GET /monitoring/pending-admission-referrals` — kartu Daftar Pantau; membaca 11.5.3 dan 11.6 dengan `OverdueOnly = true` | `InpatientMonitoring : Read` | **Rencana (belum tersedia)** |
| `Health Services / Master Data / Tariff` | Tiga isian komponen operasi; kontraknya di `keperawatan/contracts/api-contract.md` bagian 8 dan kamus data 12.14 | `Tariff : Update` | Dirancang `keperawatan` `0.6.0` |
| `Health Services / Operating Room Management / Reports` | `GET reports/operations` memisahkan `RejectedCount` dari `CancelledCount` | `OperatingRoomCase : Read` | ✅ Tersedia, isian **Rencana** |

### 11.10 Yang sengaja tidak ada di kontrak `0.10.0`

| Yang tidak ada | Alasan |
|---|---|
| Endpoint menyetujui order | Menyetujui = menjadwalkan (`PATCH cases/{id}/schedule`) |
| Endpoint membuat atau membatalkan permintaan admisi | Dipanggil OK dalam proses (`RWI-DEC-201`) |
| Endpoint penggabungan biaya operasi kunjungan asal | Tidak perlu: Billing menautkan saat memproses `ADMISSION_CONFIRMED` (`RWI-DEC-207`, `integrasi-billing` `INT-RWF-29`) |
| Endpoint menyalin ringkasan operasi ke Rawat Inap | `PR-RWF-05` |
| Field rupiah pada respons OK yang dibaca bangsal | `RWI-DEC-160` |

### 11.11 Penyelarasan decision log revision `31` ★ 2 Oktober 2026

| Keputusan | Akibat pada kontrak |
|---|---|
| `RWI-DEC-207` | `POST episodes` dengan `AdmissionReferralId` tidak berubah bentuk. Kunjungan asal tidak dibawa ke Billing lewat pesan; Billing membacanya dari `InpAdmissionReferral` (`integrasi-billing` `INT-RWF-29`) |
| `RWI-DEC-208` | Endpoint `PATCH cases/{id}/reject` dan perilaku baru `PUT …/execution/recovery` tidak lagi bergerbang |
| `RWI-DEC-218`, `RWI-DEC-219` | `FE-INP-25` memakai `UnitPrice` dan `CoverageStatus` dari `GET clinical-management/patient-procedures` (11.1, ✅ tersedia). Tidak ada endpoint harga baru |
| `RWI-DEC-214`, `RWI-DEC-215` | Butir menu Laporan Rawat Inap memakai endpoint 11.7; tidak ada endpoint baru |
| `RWI-DEC-220` | Pesan `VAL-RWF-71`, `VAL-RWF-87`, dan `VAL-RWF-90` kini keputusan pemilik, bukan tafsiran agent |

---

## 12. Perubahan pada `contract_version` `0.11.0` — Workspace PPRI ★ 7 Oktober 2026

| Field | Nilai |
|---|---|
| `last_changed_in` | `0.11.0` |
| Status | **`approved`** — Muhammad Hamzah, 2026-10-08 (`RWI-DEC-265`). Bagian 11 (`0.10.0`) tetap `approved` (`RWI-DEC-221`) |
| Owner | Muhammad Hamzah (Rawat Inap, Clinical); `MasterData` seluruh tim (`RWI-DEC-193`). Perubahan service di modul lain: `PatientManagement` dan HR Master Data **disetujui** (`RWI-DEC-266`); Registration (`RWI-OQ-128`) dan Billing (`RWI-OQ-129`) masih menunggu pemiliknya — **tidak satu pun endpoint modul lain berubah** |
| `input_revision` | `02-backend-architecture.md` `0.10` bagian 13; `data/data-dictionary.md` bagian 20; decision log revision `38`; gate `1.11` bagian 20; capability map `1.7` bagian 20; `PRD-RWI-ADMISI-001` v`0.2` (SHA-256 `f1fd336f…dc1192`) |
| Dampak kompatibilitas | **Aditif**: satu grup endpoint baru dan isian baru pada dua master. **Perubahan perilaku**: (1) `GET episodes/{id}` menambah teks peringatan; (2) daftar dan penandaan butir penutupan episode hanya membaca butir jenis penutupan (perilaku sama selama belum ada butir jenis lain) |
| Traceability | `FR-RWA-001` s.d. `008`, `020` s.d. `022` (bagian cetak), `030` s.d. `035`, `050` s.d. `053`, `060` s.d. `062`, `070` s.d. `072`, `080` s.d. `085`, `090` s.d. `093` (di luar gelombang), `100` s.d. `103`, `110` s.d. `113`, `120` s.d. `128`; `RWI-DEC-225` s.d. `264` |

Respons sukses selalu `ApiResponse<T>`. Seluruh endpoint dan isian baru berlabel **Rencana (belum tersedia)**. Respons yang dijaga `InpatientAdmissionDocument : Read` **tidak pernah** memuat rupiah; rupiah hanya ada pada endpoint `ViewAmount` (`RWI-DEC-258`).

### 12.1 Endpoint yang sudah ada dan disentuh

| Tag | Method dan path | Perubahan | Hak akses | Status |
|---|---|---|---|---|
| `Health Services / Inpatient Management / Inpatient Episode` | `GET api/v1/health-services/inpatient-management/episodes/{id}` | `Warnings` bertambah, hanya untuk episode `Admitted`/`DischargePending`: "Dokumen admisi belum lengkap: *n* (*nama dokumen*)", "Pelunasan deposit jatuh tempo *tanggal jam* terlewati — lihat kasir", atau "Kelengkapan dokumen admisi tidak dapat dihitung". **Tanpa rupiah** | `InpatientEpisode : Read` | ✅ Tersedia; isi **Rencana** |
| `Health Services / Inpatient Management / Inpatient Discharge` | `GET api/v1/health-services/inpatient-management/discharges/{episodeId}/clearance` dan `POST …/discharges/{episodeId}/clearance/{itemId}/mark` (daftar periksa penutupan yang sudah ada, `InpatientDischargeController.cs:321`, `:347`) | Hanya butir `ChecklistType = EpisodeClosure`. Menandai butir jenis lain → `404` "Butir administrasi tidak ditemukan" | Tetap (`InpatientDischarge : Read`, `InpatientDischarge : Update`) | ✅ Tersedia; perilaku **Rencana** |
| `Health Services / Billing Management / Billing / Patient Funds` | `GET …/patient-funds/deposits/episodes/{episodeId}` | **Tidak berubah** dan **tidak dipanggil layar Workspace PPRI**; server membaca service yang sama | `BillingDeposit : Read` | ✅ Tetap |
| `Health Services / Clinical Management / Patient Allergy` | `GET …/patient-allergies/active-alerts` | Tidak berubah perilakunya; controller memanggil `PatientAllergyQueryService` | `PatientAllergy : Read` | ✅ Tetap |

### 12.2 Health Services / Inpatient Management / Inpatient Admission Workspace — grup baru

Base URL: `api/v1/health-services/inpatient-management/episodes/{episodeId}/admission-workspace`
Judul grup: `[Tags("Health Services / Inpatient Management / Inpatient Admission Workspace")]`
Controller: `InpatientAdmissionDocumentController`, `ControllerName = "InpatientAdmissionDocument"`

**Bacaan**

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/summary` | Header pasien, sembilan menu beserta lencananya, kelengkapan *x* dari *y*, peringatan tanpa rupiah | `InpatientAdmissionDocument : Read` | — | `ApiResponse<AdmissionWorkspaceSummaryResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/summary/amounts` | Status deposit berupiah dan peringatan jatuh tempo berupiah | `InpatientAdmissionDocument : ViewAmount` | — | `ApiResponse<AdmissionWorkspaceAmountsResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/letterhead` | Kop surat dari profil rumah sakit; dipakai juga langkah 8 alur admisi untuk Surat Persetujuan 12 butir. Boleh untuk episode status apa pun | `InpatientAdmissionDocument : Read` | — | `ApiResponse<LetterheadResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/general-consent/print-data` | Data cetak Surat Persetujuan 12 butir dan Formulir General Consent V1: pasien, kamar, tipe kamar, calon penanda tangan. **Tanpa tulis apa pun** (`RWI-DEC-233`) | `InpatientAdmissionDocument : Read` | — | `ApiResponse<GeneralConsentPrintDataResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/prefill/{documentType}` | Isian bawaan sebelum dokumen dibuat | `InpatientAdmissionDocument : Read` | `documentType` | `ApiResponse<AdmissionDocumentPrefillResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/documents` | Daftar dokumen episode, termasuk versi lama bila diminta | `InpatientAdmissionDocument : Read` | query `type?`, `includeHistory` (bawaan `false`) | `ApiResponse<List<AdmissionDocumentSummaryResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/documents/{documentId}` | Satu dokumen lengkap, tanpa rupiah | `InpatientAdmissionDocument : Read` | — | `ApiResponse<AdmissionDocumentResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/documents/{documentId}/amounts` | Angka Pelunasan Deposit (hidup selama `Draft`, beku sesudah dikunci) atau harga Estimasi Biaya | `InpatientAdmissionDocument : ViewAmount` | — | `ApiResponse<AdmissionDocumentAmountsResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/documents/{documentId}/print` | Data cetak dokumen **tanpa rupiah** (Serah Terima, Privasi, Nilai Kepercayaan, Selisih Biaya) | `InpatientAdmissionDocument : Print` | — | `ApiResponse<AdmissionDocumentPrintResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/documents/{documentId}/amount-print` | Data cetak dokumen **berupiah** (Pelunasan Deposit, Estimasi Biaya). Service juga memeriksa `Print` | `InpatientAdmissionDocument : ViewAmount` | — | `ApiResponse<AdmissionDocumentPrintResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/identity-labels` | Data Gelang Dewasa atau Bayi dan Label Pasien | `InpatientAdmissionDocument : Print` | — | `ApiResponse<IdentityLabelResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/base-data` | Data Dasar Rawat Inap (IPD) terangkai, tanpa rupiah | `InpatientAdmissionDocument : Read` | — | `ApiResponse<InpatientBaseDataResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/base-data/amounts` | "Rencana @ Kamar (Rp)" | `InpatientAdmissionDocument : ViewAmount` | — | `ApiResponse<InpatientBaseDataAmountsResponse>` | **Rencana (belum tersedia)**; angka menunggu `RWI-OQ-129` |
| `GET` | `/print-logs` | Riwayat cetak episode | `InpatientAdmissionDocument : Read` | query `kind?`, `documentId?` | `ApiResponse<List<PrintLogResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/patient-rights` | Ringkasan Nilai Kepercayaan dan Permintaan Privasi `Completed` untuk modul lain (`FR-RWA-112`, G-41) | `InpatientEpisode : Read` | — | `ApiResponse<PatientRightsSummaryResponse>` | **Rencana (belum tersedia)** |

**Penulisan**

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `POST` | `/documents` | Simpan konsep baru | `InpatientAdmissionDocument : Create` | Header `Idempotency-Key`; `CreateAdmissionDocumentRequest` | `ApiResponse<AdmissionDocumentResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/documents/{documentId}` | Ubah konsep | `InpatientAdmissionDocument : Update` | `UpdateAdmissionDocumentRequest` | `ApiResponse<AdmissionDocumentResponse>` | **Rencana (belum tersedia)** |
| `PATCH` | `/documents/{documentId}/lock` | Kunci dan minta tanda tangan; membentuk salinan beku | `InpatientAdmissionDocument : Update` | `RowVersionRequest` | `ApiResponse<AdmissionDocumentResponse>` | **Rencana (belum tersedia)** |
| `PATCH` | `/documents/{documentId}/unlock` | Buka kunci bila belum ada tanda tangan | `InpatientAdmissionDocument : Update` | `RowVersionRequest` | `ApiResponse<AdmissionDocumentResponse>` | **Rencana (belum tersedia)** |
| `PATCH` | `/documents/{documentId}/discard` | Buang konsep sendiri | `InpatientAdmissionDocument : Update` | `ReasonRequest` (alasan 1–500) | `ApiResponse<AdmissionDocumentResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/documents/{documentId}/revisions` | Buat versi koreksi dari dokumen `Completed` | `InpatientAdmissionDocument : Update` | Header `Idempotency-Key`; `ReviseAdmissionDocumentRequest` | `ApiResponse<AdmissionDocumentResponse>` (versi baru) | **Rencana (belum tersedia)** |
| `PATCH` | `/documents/{documentId}/cancel` | Batalkan dokumen beralasan | `InpatientAdmissionDocument : Cancel` | `ReasonRequest` (alasan 10–500) | `ApiResponse<AdmissionDocumentResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/documents/{documentId}/signatures/patient-or-family` | Catat lembar kertas yang sudah ditandatangani pasien/keluarga | `InpatientAdmissionDocument : Sign` | Header `Idempotency-Key`; `RecordPaperSignatureRequest` | `ApiResponse<AdmissionDocumentResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/documents/{documentId}/signatures/admission-officer` | Atestasi slot Admission / Petugas PPRI | `InpatientAdmissionDocument : Sign` | Header `Idempotency-Key`; `RowVersionRequest` | `ApiResponse<AdmissionDocumentResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/documents/{documentId}/signatures/cro` | Atestasi slot CRO | `InpatientAdmissionDocument : SignAsCro` | Sama | Sama | **Rencana (belum tersedia)** |
| `POST` | `/documents/{documentId}/signatures/receiving-nurse` | Atestasi slot Perawat penerima | `InpatientAdmissionDocument : SignAsNurse` | Sama | Sama | **Rencana (belum tersedia)** |
| `POST` | `/documents/{documentId}/signatures/head-nurse` | Atestasi slot Kepala Ruangan | `InpatientAdmissionDocument : SignAsHeadNurse` | Sama | Sama | **Rencana (belum tersedia)** |
| `POST` | `/print-logs` | Catat cetak atau cetak ulang | `InpatientAdmissionDocument : Print` | Header `Idempotency-Key`; `RecordPrintRequest` | `ApiResponse<PrintLogResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/procedure-plan-mark` | Pasang atau cabut penanda "ada rencana tindakan/operasi" | `InpatientAdmissionDocument : Update` | `SetProcedurePlanMarkRequest` | `ApiResponse<ProcedurePlanMarkResponse>` | **Rencana (belum tersedia)** — **di luar gelombang** (`EPIC-RWA-09`) |

Tidak ada `DELETE` pada grup ini (`INV-RWA-03`). Jenis dokumen `CostEstimate` pada `POST /documents`, `/prefill/CostEstimate`, dan `/amounts` milik Estimasi Biaya ikut **di luar gelombang** sampai `DEC-INP-020` turun.

### 12.3 Bentuk request dan response

Nama enum mengikuti `02-backend-architecture.md` 13.9. Field identitas pasien tidak pernah ada di request (`INV-RWA-05`); angka deposit dan harga bertarif tidak pernah ada di request (`INV-RWA-06`).

**Request**

| DTO | Jenis | Field |
|---|---|---|
| `CreateAdmissionDocumentRequest` | Create | `DocumentType`; `SigningCity?`; `StatementDate?`; `Note?`; `Party?` (`AdmissionPartyInput`); `HandoverItems?` [`ClearanceItemId`, `Choice?`, `Note?`]; `Privacy?` {`IsTransportPrivacyRequested`, `AllowedVisitors[]` maks. 3, `SpecialRequests[]` maks. 3}; `BeliefItems?` [teks] maks. 5; `CostDifference?` {`Subject`, `SubjectOtherText?`}; `Deposit?` {`DueDate?`}; `CostEstimate?` {`OprCaseId?`, `PlannedProcedureText?`, `PlannedScheduleAt?`, `DoctorId?`, `PatientClassId?`, `EstimatedLengthOfStayDays`, `Lines[]` {`LineType`, `Description`, `ProcedureId?`, `TariffId?`, `DoctorId?`, `Quantity`, `ManualUnitPrice?`, `ManualReason?`}} |
| `UpdateAdmissionDocumentRequest` | Update | Isian yang sama tanpa `DocumentType`, ditambah `RowVersion` |
| `AdmissionPartyInput` | Bagian request | `SourceType`; `SourceRecordId?`; `FullName`; `Relationship?`; `RelationshipText?`; `Address?`; `BirthDate?`; `Gender?`; `Occupation?`; `IdentityType?`; `IdentityNumber?`; `MobilePhone?`; `OfficePhone?`. Bila `SourceType` bukan `Manual`, server **membaca ulang** nama, alamat, dan telepon dari service pemilik dan mengabaikan isian klien; untuk mengubahnya petugas memilih `Manual` |
| `RowVersionRequest` | Status | `RowVersion` |
| `ReasonRequest` | Status | `RowVersion`; `Reason` |
| `ReviseAdmissionDocumentRequest` | Status | `RowVersion`; `CorrectionReason` (10–500) |
| `RecordPaperSignatureRequest` | Status | `RowVersion`; `SignerName`; `SignerRelationship`; `SignerRelationshipText?`; `SignedAt` |
| `RecordPrintRequest` | Create | `PrintKind`; `DocumentId?`; `Copies` (1–10, bawaan 1); `ReprintReason?`; `ReprintNote?` |
| `SetProcedurePlanMarkRequest` | Update | `IsPlanned`; `Note?` |

**Response**

| DTO | Field utama |
|---|---|
| `AdmissionWorkspaceSummaryResponse` | `EpisodeId`; `Availability` (`Available`, `NotYetAdmitted`, `ReadOnly`); `ReadOnlyReason` (`EpisodeClosed`, `EpisodeCancelled`); `Header` {`PatientName`, `Salutation`, `MedicalRecordNumber`, `GenderName`, `BirthDate`, `AgeText`, `EpisodeNumber`, `AdmittedAt`, `PatientClassName`, `ServiceUnitName`, `RoomName`, `BedName`, `IsOccupyingBed`, `AttendingDoctorName`, `PaymentTypeName`, `GuarantorName`, `CardNumber`, `PrimaryEmergencyContact` {`Name`, `RelationshipText`}, `ActiveAllergyNames[]`, `RequiresIsolation`, `PatientRightsSummaryText`}; `Sources` {`Patient`, `Guarantor`, `Allergy`, `Deposit`, `OperatingRoom`, `HospitalProfile`: `Available`/`Failed`/`NotYetAvailable`}; `Menus[]` {`Key`, `Label`, `Badge` (`Completed`, `AwaitingSignature`, `Draft`, `NotCreated`, `NotRequired`, `PrintOnly`, `Printed`, `NotPrinted`, `Uncountable`), `ActiveDocumentId?`, `IsRequired`}; `Completeness` {`CompletedCount`, `RequiredCount`, `UncountableCount`, `MissingNames[]`}; `Warnings[]` |
| `AdmissionWorkspaceAmountsResponse` | `DepositStatus` (`NotRequired`, `Sufficient`, `Shortfall`, `Unavailable`); `MinimumPolicyAmount`; `ReceivedAmount`; `ShortfallAmount`; `ReadAt`; `OverdueStatement?` {`DocumentId`, `DueAt`, `CurrentShortfallAmount`} |
| `LetterheadResponse` | `IsAvailable`; `SiteName`; `AddressLines[]`; `PhoneNumber`; `Email`; `SiteCode` |
| `GeneralConsentPrintDataResponse` | `Letterhead`; `FormCode`; `SigningCity`; `PrintDate`; `Patient` {`FullName`, `Salutation`, `MedicalRecordNumber`, `BirthDate`, `PhoneNumber`, `Address`}; `Episode` {`EpisodeNumber`, `AdmittedAt`, `PatientClassName`, `RoomName`, `BedName`, `AttendingDoctorName`}; `RoomType` (`General`, `Special`) dan `RoomTypeReason`; `Guarantor` {`PaymentTypeName`, `GuarantorName`}; `SignerCandidates[]` {`Source`, `SourceRecordId`, `Name`, `RelationshipType?`, `RelationshipText`, `Address`}; isian Surat Persetujuan 12 butir yang hari ini dibaca `use-inpatient-consent-print.js:58-82` |
| `AdmissionDocumentPrefillResponse` | `DocumentType`; `CanCreate`; `CannotCreateReasonCode?` (`NotRequiredForPayer`, `NoDepositShortfall`, `DepositUnavailable`, `ActiveDocumentExists`); `SigningCity`; `StatementDate`; `PartyCandidates[]`; `HandoverItems[]` {`ClearanceItemId`, `LineNo`, `ItemNumber?`, `ParentItemNumber?`, `Code`, `Name`, `Suggestion?` {`Text`}}; `PreviousBeliefItems[]` dan `PreviousBeliefDocumentId?`; `PatientAsDeclarer?` (Selisih Biaya "diri saya sendiri"); `DefaultDueDate?` dan `MaxDueDate?` (Pelunasan Deposit, tanpa rupiah); `CostEstimateHeader?` |
| `AdmissionDocumentSummaryResponse` | `Id`; `DocumentType`; `Status`; `VersionNo`; `PreviousVersionId`; `CreatedAt`; `CreatedByName`; `LockedAt`; `CompletedAt`; `CancelledAt` |
| `AdmissionDocumentResponse` | Field ringkasan di atas; `RowVersion`; `IsReadOnly`; `AvailableActions[]` (`Update`, `Lock`, `Unlock`, `Revise`, `Discard`, `Cancel`, `SignPatientOrFamily`, `SignAdmissionOfficer`, `SignCro`, `SignReceivingNurse`, `SignHeadNurse`, `Print`, `AmountPrint`) — dihitung dari status dan hak pengguna; `SigningCity`; `StatementDate`; `Note`; `CorrectionReason`; `CancelledReason`; `Slots[]` {`Slot`, `Label`, `Signature?` {`Method`, `SignerName`, `SignerPositionName`, `SignerRelationshipText`, `SignedAt`, `VerifiedByName`}}; `Party?`; `HandoverItems[]` (dengan `Suggestion` hanya bila `Draft`); `Privacy?`; `BeliefItems[]`; `CostDifference?`; `Deposit?` {`DueAt`, `AmountsHidden = true`}; `CostEstimate?` (baris tanpa harga); `SourceChangedSinceLock` — "Data pasien telah diperbarui sejak dokumen ini dikunci" (`FR-RWA-124`) |
| `AdmissionDocumentAmountsResponse` | `Deposit?` {`MinimumPolicyAmount`, `ReceivedAmount`, `ShortfallAmount`, `CalculationText`, `ReadAt`, `IsFrozen`}; `CostEstimate?` {`Lines[]` {`LineNo`, `UnitPrice`, `LineAmount`, `PriceSource`}, `TotalAmount`, `PricesReadAt`, `IsFrozen`, `NotesText`} |
| `AdmissionDocumentPrintResponse` | `DocumentId`; `DocumentType`; `Status`; `VersionNo`; `PrintMarker` (`Draft` "KONSEP — BELUM DITANDATANGANI", `SignatureSheet` "Lembar untuk ditandatangani — versi *n*", `Final`, `Superseded` "DIGANTIKAN VERSI *n+1*", `Cancelled` "DIBATALKAN"); `EpisodeCancelledMarker` ("ADMISI DIBATALKAN"); `NextPrintSequence`; `Letterhead`; `FormCode`; `SigningCity`; `StatementDate`; `Patient`, `Episode`, `Guarantor` (dari salinan beku; data hidup hanya untuk `Draft`); isi khas jenis; `SignatureLines[]` — atestasi "Ditandatangani secara elektronik oleh *nama*, *jabatan*, *tanggal jam*", kertas "Ditandatangani di kertas oleh *nama* (*hubungan*), *waktu*, diverifikasi *petugas*" |
| `IdentityLabelResponse` | `Wristband` {`Kind` (`Adult`, `Infant`), `DisplayName` (contoh "BUDI SANTOSO, Tn."), `BirthDateText`, `AgeText`, `MedicalRecordNumber`, `QrPayload`, `SmallLabelCount` (2 untuk bayi)}; `PatientLabel` {`HospitalCode`, `NameLine`, `BirthDateShort`, `GenderAgeText`, `MedicalRecordNumber`, `CardNumber?`, `QrPayload`}; `PrintCounts` {`AdultWristband`, `InfantWristband`, `PatientLabel`} |
| `InpatientBaseDataResponse` | Bagian kiri dan kanan IPD sesuai PRD Lampiran A.6 dengan sumber `RWI-DEC-244`, `253`, `254`; `BlankFields[]` (kunci isian yang dicetak garis kosong); `RoomRateDisplay = "lihat kasir"` sampai rupiah dibaca dari `/base-data/amounts`; `CanPrint` dan `CannotPrintReason` (`FR-RWA-072`) |
| `InpatientBaseDataAmountsResponse` | `DailyRoomRate?`; `RoomRateState` (`Available`, `NotYetAvailable` — `RWI-OQ-129`, `TariffMissing`) |
| `PatientRightsSummaryResponse` | `BeliefValues?` {`DocumentId`, `CompletedAt`, `Items[]`}; `Privacy?` {`DocumentId`, `AllowedVisitorNames[]`, `SpecialRequests[]`, `IsTransportPrivacyRequested`}; `SummaryText` (contoh "Privasi khusus: hanya 2 kerabat; privasi transportasi: Ya") |
| `PrintLogResponse` | `Id`; `PrintKind`; `DocumentId?`; `DocumentStatusAtPrint?`; `Copies`; `IsReprint`; `PrintSequence`; `ReprintReason?`; `ReprintNote?`; `PrintedByName`; `PrintedAt` |
| `ProcedurePlanMarkResponse` | `IsPlanned`; `MarkedAt?`; `MarkedByName?`; `Note?` |

### 12.4 Perubahan pada grup Master Data

#### Health Services / Master Data / Inpatient Clearance Item

Base URL: `api/v1/health-services/master-data/inpatient-clearance-items`

| Method | Path | Perubahan | Hak akses | Status |
|---|---|---|---|---|
| `GET` | `/`, `/options`, `/summary` | Query `checklistType?` (`EpisodeClosure`, `NewPatientHandover`; kosong = semua). Respons + `ChecklistType`, `ChecklistTypeName`, `ParentItemId`, `ParentItemName`, `HandoverSuggestionSource` | `InpatientClearanceItem : Read` | ✅ Tersedia; isian **Rencana** |
| `GET` | `/{id}` | Respons + tiga isian | `InpatientClearanceItem : Read` | ✅ Tersedia; isian **Rencana** |
| `POST` | `/` | Request + `ChecklistType` (wajib, bawaan `EpisodeClosure`), `ParentItemId?`, `HandoverSuggestionSource` (bawaan `None`) | `InpatientClearanceItem : Create` | ✅ Tersedia; isian **Rencana** |
| `PUT` | `/{id}` | Sama; `ChecklistType` tidak dapat diubah setelah butir dipakai dokumen atau penandaan | `InpatientClearanceItem : Update` | ✅ Tersedia; isian **Rencana** |
| `PATCH`, `DELETE` | `/{id}/status`, `/{id}` | Tidak berubah; menonaktifkan induk tidak menonaktifkan sub-butir otomatis | Tetap | ✅ Tetap |

#### Health Services / Master Data / Inpatient Setting

Base URL: `api/v1/health-services/master-data/inpatient-settings`

| Method | Path | Perubahan | Hak akses | Status |
|---|---|---|---|---|
| `GET` | `/` | Respons + `GeneralConsentFormCode`, `NewPatientHandoverFormCode`, `PrivacyRequestFormCode`, `BeliefValuesFormCode`, `CostDifferenceFormCode`, `DepositSettlementFormCode`, `CostEstimateFormCode`, `InpatientBaseDataFormCode`, `DocumentSigningCity`, `InfantWristbandMaxAgeYears`, `PatientLabelHospitalCode` | `InpatientSetting : Read` | ✅ Tersedia; isian **Rencana** |
| `PUT` | `/{id}` | Request + sebelas isian di atas | `InpatientSetting : Update` | ✅ Tersedia; isian **Rencana** |

### 12.5 Kode status

| Kode | Arti bagi pengguna | Contoh |
|---|---|---|
| `200` | Berhasil, termasuk pengulangan dengan `Idempotency-Key` yang sama | Sari menekan Simpan dua kali; satu dokumen, kedua klik mendapat dokumen yang sama |
| `400` | Isian tidak lengkap atau formatnya salah | Telepon 14 digit; alasan batal 5 karakter |
| `403` | Tidak punya hak untuk tindakan ini | Perawat memanggil `/cancel`; pengguna tanpa `ViewAmount` membuka `/summary/amounts` |
| `404` | Episode atau dokumen tidak ditemukan, atau dokumen bukan milik episode itu | — |
| `409` | Bertabrakan dengan keadaan data | Episode sudah ditutup; admisi belum dikonfirmasi; sudah ada dokumen aktif sejenis; dokumen sudah diubah petugas lain; dokumen `Completed` tidak dapat diubah; slot sudah ditandatangani |
| `422` | Melanggar aturan bisnis | Satu petugas dua slot; butir Belum tanpa keterangan; jatuh tempo melewati batas; tidak ada kekurangan deposit; pasien tunai pada Selisih Biaya; pasien belum menempati bed |

Kode alasan (`INP-ADM-DOC-*`, `INP-ADM-PRT-*`, `MST-ICI-*`, `MST-IST-*`) beserta kalimat pesannya hanya ada di `contracts/validation-matrix.md` bagian 15.

### 12.6 Yang sengaja tidak ada di kontrak `0.11.0`

| Yang tidak ada | Alasan |
|---|---|
| Endpoint menyimpan atau menandatangani General Consent | *Fail-closed* (`RWI-DEC-230`, `233`); `patient-consents` tidak dipanggil |
| Endpoint unggah gambar tanda tangan pasien/keluarga | `EPIC-RWA-13` `OPEN DECISION` |
| `DELETE` dokumen admisi | `INV-RWA-03` |
| Satu endpoint tanda tangan dengan parameter slot | Satu method hanya satu `[AccessPermission]`; slot berbeda dijaga aksi berbeda |
| Isian rupiah pada respons yang dijaga `Read` | `RWI-DEC-258` |
| Endpoint MP Benefit dan Estimasi Rinci | Ditunda (`RWI-OQ-119`, `RWI-OQ-120`) |
| Endpoint tarif visit dokter | `DEC-INP-020` |
| Endpoint baru di `PatientManagement`, Registration, Billing, HR | `RWI-DEC-264` butir 1: service baca baru tidak membuka endpoint modul pemilik |

## 13. Amandemen Bed Management — 10 Oktober 2026

**Status: draft — Amandemen Bed Management, 10 Oktober 2026.** Set kontrak mengikuti `blueprint-manifest.md`; `last_changed_in: 0.12.0`. Owner produk/domain/API: Muhammad Hamzah (RWI-DEC-061); frontend: pengembang dalam batas RWI-DEC-292; keamanan/privasi: OPEN. `approved_by: null`, `approved_at: null` untuk amandemen ini.

Masukan: decision log revision **46**, RWI-DEC-274–294 dan RWI-AC-396–426; gate revision **1.12**, **BM-RCG-20261010-01**, enam BM-CG siap untuk desain produk terbatas. `DOMAIN_ARCHITECTURE_NOT_RUN` untuk slice ini: ownership existing sudah diketahui dan gate mengizinkan desain langsung. Arsitektur domain lama bagi scope lain tetap berlaku. As-is bersumber audit **BM-AUD-20261010-01** revision 1 (section 7 untuk Swagger), bukan bukti runtime.

Snapshot BE `d4e1eca06fb28c05934c68c1e51a4dca01935a10`, FE `969acfcc04cdf31074a1911e9827c31d25ddadd0`. Semua nama class/field/API baru di bawah adalah **target Rencana (belum tersedia)**. Bila bagian lama bertentangan mengenai bed kembali Available, amandemen ini mengikuti RWI-DEC-281/282. Persetujuan produk bukan persetujuan desain atau SOP. Hash masukan terpusat pada manifest.

### 13.1 Envelope, waktu, versi dan compatibility

ApiResponse<T> existing tetap Success, StatusCode, Message, Data, Errors, Timestamp; gunakan Errors={code,fields?} dalam slot existing, bukan envelope kedua. GET/aksi sukses 200 sesuai convention existing; daftar paged memakai PagedResult<T> existing (PageNumber,PageSize,TotalData,TotalPage,Items). Semua timestamp bisnis UTC, Timestamp envelope existing tidak diubah pada slice ini. Semua response target memakai projection DTO, tidak serialize entity.

Path dan raw enum existing dipertahankan; GET tambahan bersifat aditif. Kewajiban expected versions, key dan kategori/reason pada mutation adalah **perubahan kontrak perilaku/request yang memerlukan cutover konsumen**, bukan seluruhnya backward-compatible. Old client yang tidak mengirim field ditolak 400 saat target diaktifkan. Adapter endpoint existing wajib menerapkan guard yang sama; tidak menyediakan jalur legacy yang meloloskan dirty bed. Endpoint new Tag di bawah belum ada; endpoint existing berlabel diperbarui belum mempunyai perilaku target.

### 13.2 Inventaris Swagger target dan permission canonical

#### Health Services / Inpatient Management / Bed Management

Base URL: `api/v1/health-services/inpatient-management/bed-management`

| Method | Path | Kegunaan | Hak akses | Request | Response Data | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| GET | /metadata | Metadata tab/status/kategori dan hak aksi | InpatientBedManagement : Read | serviceUnitId? | BedManagementMetadataResponse | EPIC BM-01 | Rencana (belum tersedia) |
| GET | /monitoring | Monitoring sanitized dan counts | InpatientBedManagement : Read | BedMonitoringQuery | BedMonitoringResponse | EPIC BM-01 | Rencana (belum tersedia) |
| GET | /beds/{bedId}/cleaning-attempts | Jejak operasional bed tanpa pasien | InpatientBedManagement : Read | pageNumber/pageSize | PagedResult<BedCleaningAttemptResponse> | EPIC BM-03 | Rencana (belum tersedia) |
| GET | /usage-history | Semua segmen penggunaan per bed/periode | InpatientBedManagement : ReadUsageHistory | BedUsageHistoryQuery | PagedResult<BedUsageHistoryResponse> | EPIC BM-06 | Rencana (belum tersedia) |
| GET | /operations/{key} | Baca own committed outcome | InpatientBedManagement : Read | key; actor dari token | OperationOutcomeResponse | EPIC BM-02 | Rencana (belum tersedia) |
| POST | /beds/{bedId}/cleaning-attempts | Mulai pekerjaan | InpatientBedManagement : StartCleaning | StartBedCleaningRequest | BedOperationResponse | EPIC BM-03 | Rencana (belum tersedia) |
| PATCH | /cleaning-attempts/{attemptId}/complete | Selesai fisik, menunggu verifikasi | InpatientBedManagement : CompleteCleaning | CompleteBedCleaningRequest | BedOperationResponse | EPIC BM-03 | Rencana (belum tersedia) |
| POST | /beds/{bedId}/readiness-verifications | Sahkan/tolak kesiapan | InpatientBedManagement : VerifyReadiness | VerifyBedReadinessRequest | BedOperationResponse | EPIC BM-03 | Rencana (belum tersedia) |

#### Health Services / Inpatient Management / Bed Occupancy

Base URL: `api/v1/health-services/inpatient-management/bed-occupancies`

| Method | Path | Kegunaan | Hak akses | Request | Response Data | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| GET | /available-beds | Shared predicate + eligibility existing | InpatientBedOccupancy : Read | AvailableBedQuery existing | AvailableBedPagedResult + operational version | EPIC BM-02 | Existing; diperbarui |
| GET | /bed-board | Adapter konsisten untuk konsumen existing | InpatientBedOccupancy : Read | serviceUnitId? | BedBoardResponse + operational fields | EPIC BM-01 | Existing; diperbarui |
| GET | /transfer-context | Asal otomatis dari placement current | InpatientBedOccupancy : Read | episodeId required | BedTransferContextResponse | EPIC BM-05 | Rencana (belum tersedia) |
| POST | /reservations | Reserve TTL existing | InpatientBedOccupancy : Create | ReserveBedRequest + ExpectedBedVersion | BedReservationResponse + OperationMeta | EPIC BM-02 | Existing; diperbarui |
| PATCH | /reservations/{id}/cancel | Cancel beralasan persisten | InpatientBedOccupancy : Update | CancelReservationRequest + Reason wajib + ExpectedBedVersion | BedReservationResponse + OperationMeta | EPIC BM-02 | Existing; diperbarui |
| POST | /placements | Tempatkan dan snapshot awal | InpatientBedOccupancy : Create | PlacePatientRequest + ExpectedBedVersion | BedPlacementResponse + OperationMeta | EPIC BM-02 | Existing; diperbarui |
| POST | /placements/transfer | Transfer atomik satu langkah | InpatientBedOccupancy : Transfer | TransferPatientRequest diperluas | BedPlacementResponse + OperationMeta | EPIC BM-05 | Existing; diperbarui |
| POST | /placements/{placementId}/corrections | Koreksi versioned existing | InpatientBedOccupancy : Correct | CorrectPlacementRequest existing + AffectedBedVersions | BedPlacementResponse + OperationMeta | EPIC BM-06 | Existing; diperbarui |
| GET | /placements/by-episode/{episodeId} | Histori episode existing | InpatientBedOccupancy : Read | episodeId | List<BedPlacementResponse> + snapshot/category | EPIC BM-06 | Existing; diperbarui |

#### Health Services / Master Data / Bed

Base URL: `api/v1/health-services/master-data/beds`

| Method | Path | Kegunaan | Hak akses | Request | Response Data | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| POST | / | Master bed baru; kesiapan Unverified | Bed : Create | CreateBedRequest existing; OperationReason | BedCreateResponse existing + OperationalVersion/OperationMeta | EPIC BM-04 | Existing; guard diperbarui |
| PUT | /{id} | Ubah master melalui guard penuh | Bed : Update | UpdateBedRequest + ExpectedBedVersion/OperationReason | BedUpdateResponse + OperationMeta | EPIC BM-04 | Existing; diperbarui |
| PATCH | /{id}/status | Status administratif; bukan override kesiapan | Bed : Update | UpdateBedStatusRequest + ExpectedBedVersion/OperationReason | BedUpdateResponse + OperationMeta | EPIC BM-04 | Existing; diperbarui |
| PATCH | /{id}/availability | Close/reopen beralasan | Bed : Update | UpdateBedAvailabilityRequest + ExpectedBedVersion/OperationReason | BedUpdateResponse + OperationMeta | EPIC BM-04 | Existing; diperbarui |
| DELETE | /{id} | Soft-delete master tidak melepas holder | Bed : Delete | ExpectedBedVersion/OperationReason (body target) | Envelope existing + OperationMeta | EPIC BM-04 | Existing; guard diperbarui |

#### Health Services / Inpatient Management / Inpatient Discharge

Base URL: `api/v1/health-services/inpatient-management/discharges`

| Method | Path | Kegunaan | Hak akses | Request | Response Data | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| POST | /{episodeId}/record-departure | Kepergian fisik; used bed menunggu bersih | InpatientDischarge : RecordDeparture | RecordDepartureRequest existing + ExpectedPlacementId/ExpectedBedVersion | InpatientDepartureResponse + OperationMeta | EPIC BM-02 | Existing; diperbarui |

#### Health Services / Inpatient Management / Inpatient Report

Base URL: `api/v1/health-services/inpatient-management/reports`

| Method | Path | Kegunaan | Hak akses | Request | Response Data | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| GET | /room-transfers | Laporan transfer existing | InpatientReport : ReadRoomTransfer | RoomTransferReportQuery existing | PagedResult<RoomTransferReportRow> | EPIC BM-06 | Existing / Reuse |
| GET | /room-transfers/export | Export transfer existing, batas existing 31 hari | InpatientReport : ExportRoomTransfer | Query existing | File xlsx existing | EPIC BM-06 | Existing / Reuse |

Tag MasterData/Discharge/Report dan CRUD existing mengacu source controller; tidak mengganti Tag existing. Mapping endpoint→permission **hanya canonical di bagian ini**. Semua route juga tunduk business guards, unit/bed/episode scope dan masking pada permission contract. Tidak menambahkan permissions ke role secara otomatis.

### 13.3 Bentuk DTO target dan validasi

| DTO/kontrak | Properti target | Aturan |
| --- | --- | --- |
| Mutation headers | Idempotency-Key:string required max100 pada seluruh mutation bed terdampak; Content-Type JSON; auth existing | Receipt atomik, header key tidak boleh di-log mentah |
| OperationMeta | ReceiptId:Guid, ResultEntityId:Guid, ResultKind:string, CommittedVersion:long?, CommittedAtUtc:UTC, IsReplay:bool | Tambahan pada response mutation; state DTO hasil reload saat ini, bukan janji state commit lama |
| OperationOutcomeResponse | OperationMeta + OperationName:string; hanya actor pemilik dan scope hasil | 200 bila committed; 404 unknown; tidak ada PHI/body permintaan |
| BedManagementMetadataResponse | Tabs:[{key,label,allowed}], BedStatuses:[{code,label}], TransferCategories:[{code,label}], AllowedActions:string[] | Konstan produk + hak server; tidak grant role di FE |
| BedMonitoringQuery | ServiceUnitId?:Guid, RoomId?:Guid, Status?:enam code, Search?:string max100 | Semua hasil dibatasi scope actor; search bed/kamar saja |
| BedMonitoringResponse | AsOfUtc:UTC, Counts:{Total,Available,Occupied,Reserved,WaitingCleaning,Cleaning,Unavailable}, ServiceUnits:[{id,name,rooms:[{id,name,beds:BedOperationalResponse[]}]}] | Counts dihitung setelah scope/filter; tidak leak unit luar |
| BedOperationalResponse | BedId, BedCode, BedName, RoomId, RoomName, ServiceUnitId, PatientClassId, PatientClassName; StatusCode, StatusLabel; CleaningPhase?:InProgress/AwaitingVerification; IsBookable:bool; OperationalVersion:long; CycleId:Guid; ConflictCode?:string; BlockReasonCodes:string[]; AvailableActions:string[]; HoldingContext?:sanitized | OperationalOnly/HK tidak diberi HoldingContext, patient/episode/reservation ID, diagnosis atau transfer reason |
| HoldingContext | EpisodeId?, EpisodeNumber?, PatientName?, ReservationId?, ReservationExpiresAt? | Hanya jika hak existing patient/episode Read+scope terpenuhi dan actor bukan OperationalOnly; selain itu NULL/omitted, tanpa pseudonym yang dapat ditelusuri |
| StartBedCleaningRequest | CycleId:Guid, ExpectedBedVersion:long | Wajib WaitingCleaning dan SOP assignment; SopReference dari trusted server config |
| CompleteBedCleaningRequest | CycleId:Guid, ExpectedBedVersion:long | attemptId route; wajib Started current cycle; akun HK berwenang scope, boleh petugas lanjutan hanya jika assignment sah |
| VerifyBedReadinessRequest | CycleId:Guid, AttemptId?:Guid, ExpectedBedVersion:long, IsReady:bool, EvidenceReference:string required max200, Reason?:string max500 | AwaitingVerification wajib attemptId current; Unverified tanpa bukti used-dirty boleh verifikasi awal tanpa attempt sesuai SOP; penolakan wajib Reason dan kembali WaitingCleaning |
| BedOperationResponse | Bed:BedOperationalResponse, AttemptId?:Guid, OperationMeta | PHI-free |
| BedCleaningAttemptResponse | Id, CycleId, State, StartedAtUtc, StartedByUserId, CompletedAtUtc?, CompletedByUserId?, VerifiedAtUtc?, VerifiedByUserId?, RejectionReason?, SopReference | Hanya scope operasional bed; alasan bukan diagnosis, viewer actor hanya yang berwenang |
| BedTransferContextResponse | EpisodeId, SourcePlacementId, SourcePlacementVersion:int, SourceBed:BedOperationalResponse, CurrentRoom/Class snapshot, AsOfUtc | Episode read + scope; asal wajib current; daftar tujuan tetap available-beds episode query |
| AvailableBedResponse extensions | OperationalVersion:long, CycleId:Guid, StatusCode:string, IsBookable:bool, ComparisonResult?:DownGrade/UpGrade/SameGrade/Unknown, AllowedManualCategories:string[] | Comparison jika episode sumber ada; Unknown tidak otomatis SameGrade |
| TransferPatientRequest extensions | ManualTransferCategory:enum int required (1 DownGrade/2 UpGrade/3 SameGrade), ExpectedSourcePlacementId:Guid, ExpectedSourcePlacementVersion:int, ExpectedSourceBedVersion:long, ExpectedTargetBedVersion:long | Existing EpisodeId/TargetBedId/TransferReason required; min reason existing 10 dan max500 dipertahankan |
| Reserve/Place/Cancel DTO extensions | ExpectedBedVersion:long required; Cancel Reason trim required max500 | Existing fields tetap; expected version bukan readiness assurance jika state gagal |
| CorrectPlacementRequest extension | AffectedBedVersions:[{BedId:Guid,ExpectedVersion:long}] untuk semua bed berdampak | Pertahankan field versi koreksi existing, reason dan Billing guard; server menentukan set bed dan menolak missing/extra |
| RecordDepartureRequest extension | ExpectedPlacementId:Guid, ExpectedBedVersion:long | Server episode+placement guard; jika sudah lepas outcome checked, jangan release ulang |
| Master mutation extensions | ExpectedBedVersion:long required kecuali create, OperationReason:string max500 wajib close/reopen/delete/perubahan availability; create raw status default tidak menghasilkan Ready | Manual Occupied/Reserved/Cleaning/Unknown ditolak; raw Available pada reopen hanya maksud buka admin, root tetap belum Ready |
| BedUsageHistoryQuery | BedId:Guid required, FromUtc:UTC required, ToUtc:UTC required (From<To), RoomId?:Guid, PageNumber:int=1, PageSize:int=25 | Batas teknis maxPageSize100; tidak mengimpor 31 hari report sebagai kebijakan history |
| BedUsageHistoryResponse | PlacementId, EpisodeId? authorized, PatientIdentity? authorized, BedId, RoomId, ServiceUnitId, PatientClassId, LocationSnapshot?, ContextSource:string, SequenceNumber, StartDateTime, EndDateTime?, EndReason?, TransferCategory?, TransferFromPlacementId?, Version, CorrectsPlacementId?, SupersededByCorrectionId?, IsCurrent, IsCorrection, IsEffectiveForBilling | IsEffectiveForBilling = SupersededByCorrectionId NULL sesuai contract Billing, bukan !IsSuperseded; Reason sensitif hanya jika existing Right Correct/episode access |

Monitoring adapter existing bed-board dan available-beds mengambil proyeksi/predicate yang sama. BedBoard raw BedStatus tetap kompatibel, tambah StatusCode/StatusLabel/CleaningPhase/OperationalVersion. Hanya enam status semantik: Available, Occupied, Reserved, WaitingCleaning, Cleaning, Unavailable, label BA pada state matrix. Raw status tidak dipakai UI sebagai bukti bookable. Field flat existing HoldingEpisodeId, HoldingEpisodeNumber, PatientName dan ReservationId pada BedBoardBedResponse juga wajib null/omitted untuk OperationalOnly/HK atau caller tanpa patient/episode authorization; tidak hanya HoldingContext baru yang dimask. TransferReason/history diagnosis tidak dikirim ke HK.

History memilih overlap setengah-terbuka [FromUtc,ToUtc): StartDateTime<ToUtc dan (EndDateTime=NULL atau EndDateTime>FromUtc). Durasi dihitung dari fakta hunian, bukan reservasi; ongoing end=NULL. Stable sort StartDateTime DESC, PlacementId DESC; response menyertakan semua versi dan flag efektif agar correction tidak tersembunyi. Filter RoomId memeriksa RoomId segmen tersimpan, bukan kamar bed current. Scope identity diperiksa per episode row; row di luar bed/unit scope tidak dikembalikan.

Contoh payload transfer samaran: EpisodeId=episode-uji-A, TargetBedId=bed-uji-B, ManualTransferCategory=3, ExpectedSourcePlacementId=placement-uji-A, ExpectedSourcePlacementVersion=1, ExpectedSourceBedVersion=7, ExpectedTargetBedVersion=4, TransferReason='Pindah kamar sesuai kebutuhan'. ID samaran ini adalah penjelasan, bukan literal UUID API; test memakai UUID fixture. Petugas harus memilih kategori, bukan server/FE mengisi default.

### 13.4 Error, retry dan hasil tidak pasti

| HTTP | Errors.code | Pemicu | Hasil/aksi klien |
| --- | --- | --- | --- |
| 400 | INVALID_INPUT | Field/format wajib, key/expected version hilang, rentang waktu tidak valid | Tidak menulis perubahan |
| 401 / 403 | UNAUTHENTICATED / ACCESS_DENIED | Token/permission/scope/operational profile tidak sah | Tidak leak keberadaan episode di luar scope |
| 404 | NOT_FOUND / OPERATION_UNKNOWN | Entity dalam scope tidak ada atau own receipt belum ditemukan | Outcome unknown tetap perlu cek sebelum retry |
| 409 | STALE_BED_VERSION / STALE_PLACEMENT / BED_HOLD_CONFLICT / IDEMPOTENCY_KEY_REUSED / CLEANING_CYCLE_STALE | State berubah, pemegang lain, key input berbeda atau attempt siklus lama | Reload; tidak otomatis mengirim key baru |
| 422 | BED_NOT_READY / BED_UNAVAILABLE / TRANSFER_CATEGORY_MISMATCH / CLASS_ORDER_UNVERIFIED / WORKFLOW_NOT_ACTIVATED / REASON_REQUIRED | Aturan bisnis/gate proof tidak terpenuhi | Tidak mengubah data, tampilkan pesan Indonesia yang dapat ditindaklanjuti |
| 503 | DEPENDENCY_UNAVAILABLE | Guard/SOP reference/config proof tidak dapat diverifikasi | Fail closed; tidak mengklaim simpan sukses |

Mutation pertama mengembalikan DTO route + OperationMeta(IsReplay=false). Retry actor/key/hash yang sama memuat ulang result entity yang masih terotorisasi, OperationMeta menunjuk commit semula dengan IsReplay=true. Semua field current pada DTO dibaca saat replay; CommittedVersion bukan current Version. GET own outcome tidak mengembalikan detail pasien. Timeout setelah commit tidak menyebabkan reversal transfer. Tidak retry otomatis mutation non-idempotent memakai key berbeda.

### 13.5 Coverage

DEC-274–294; AC-396–426. API/PG/contract tests target belum dijalankan (BM-G04). Contracttest harus membuktikan seluruh mutation existing termasuk master generic write tidak bypass serta identity HK tidak berada dalam JSON, bukan hanya kolom tersembunyi.

### 13.6 Writer master hierarchy yang ikut invariant

Route/Tag berikut ditelusuri langsung dari RoomController.cs, ServiceUnitController.cs, PatientClassController.cs pada snapshot desain. Ini pengamanan invariant bed, bukan master baru. GET/POST master hierarchy tidak berubah; PUT/status/delete yang memengaruhi existing beds masuk coordinator, sehingga old writer tidak menjadi jalur bypass.

#### Health Services / Master Data / Room

Base URL: `api/v1/health-services/master-data/rooms`

| Method | Path | Kegunaan | Hak akses | Request | Response Data | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| PUT | /{id} | Guard hierarchy yang mengubah availability/class bed | Room : Update | UpdateRoomRequest existing + hierarchy mutation extension | Response existing + OperationMeta | EPIC BM-04 | Existing; guard diperbarui |
| PATCH | /{id}/status | Aktivasi/nonaktif hierarchy terkoordinasi | Room : Update | UpdateRoomStatusRequest existing + hierarchy mutation extension | Response existing + OperationMeta | EPIC BM-04 | Existing; guard diperbarui |
| DELETE | /{id} | Soft-delete hierarchy tanpa melepas holder | Room : Delete | DeleteRoomRequest existing + hierarchy mutation extension | Envelope existing + OperationMeta | EPIC BM-04 | Existing; guard diperbarui |

#### Health Services / Master Data / Service Unit

Base URL: `api/v1/health-services/master-data/service-units`

| Method | Path | Kegunaan | Hak akses | Request | Response Data | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| PUT | /{id} | Guard hierarchy yang mengubah availability/class bed | ServiceUnit : Update | UpdateServiceUnitRequest existing + hierarchy mutation extension | Response existing + OperationMeta | EPIC BM-04 | Existing; guard diperbarui |
| PATCH | /{id}/status | Aktivasi/nonaktif hierarchy terkoordinasi | ServiceUnit : Update | UpdateServiceUnitStatusRequest existing + hierarchy mutation extension | Response existing + OperationMeta | EPIC BM-04 | Existing; guard diperbarui |
| DELETE | /{id} | Soft-delete hierarchy tanpa melepas holder | ServiceUnit : Delete | DeleteServiceUnitRequest existing + hierarchy mutation extension | Envelope existing + OperationMeta | EPIC BM-04 | Existing; guard diperbarui |

#### Health Services / Master Data / Patient Class

Base URL: `api/v1/health-services/master-data/patient-classes`

| Method | Path | Kegunaan | Hak akses | Request | Response Data | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| PUT | /{id} | Guard hierarchy yang mengubah availability/class bed | PatientClass : Update | UpdatePatientClassRequest existing + hierarchy mutation extension | Response existing + OperationMeta | EPIC BM-04 | Existing; guard diperbarui |
| PATCH | /{id}/status | Aktivasi/nonaktif hierarchy terkoordinasi | PatientClass : Update | UpdatePatientClassStatusRequest existing + hierarchy mutation extension | Response existing + OperationMeta | EPIC BM-04 | Existing; guard diperbarui |
| DELETE | /{id} | Soft-delete hierarchy tanpa melepas holder | PatientClass : Delete | DeletePatientClassRequest existing + hierarchy mutation extension | Envelope existing + OperationMeta | EPIC BM-04 | Existing; guard diperbarui |

Hierarchy mutation extension: OperationReason:string max500 required bila mengubah availability/location class atau delete; AffectedBedVersions:[{BedId:Guid,ExpectedVersion:long}] sesuai set yang ditentukan server, plus Idempotency-Key. Receipt ResultKind dapat Room/ServiceUnit/PatientClass; outcome scoped pada semua bed hasil. Perubahan label/rank non-availability tetap mengunci parent dan menginvalidasi digest comparison jika berubah, tanpa mengganti snapshot sejarah; tidak menutup bed atau menghapus holder otomatis.

### 13.7 Read versions untuk master consumer

| Tag | Base URL | Method | Path | Hak akses | Response tambahan |
| --- | --- | --- | --- | --- | --- |
| Health Services / Master Data / Bed | `api/v1/health-services/master-data/beds` | GET | /{id} | Bed : Read | Bed detail existing + OperationalVersion/CycleId |
| Health Services / Master Data / Room | `api/v1/health-services/master-data/rooms` | GET | /{id} | Room : Read | Detail existing + AffectedBedVersions:[{BedId,ExpectedVersion}] |
| Health Services / Master Data / Service Unit | `api/v1/health-services/master-data/service-units` | GET | /{id} | ServiceUnit : Read | Detail existing + AffectedBedVersions:[{BedId,ExpectedVersion}] |
| Health Services / Master Data / Patient Class | `api/v1/health-services/master-data/patient-classes` | GET | /{id} | PatientClass : Read | Detail existing + AffectedBedVersions:[{BedId,ExpectedVersion}] |

GET detail master tambahan tidak memuat holder/pasien. AffectedBedVersions adalah read-only snapshot IDs+versions; server tetap menghitung ulang seluruh set dan recheck setelah lock. Perubahan set/versi menolak commit stale; client reload, tidak menebak expected version.

### 13.8 Writer admisi transfer IGD — pemeriksaan 11 Oktober 2026

#### [Tags("Health Services / Inpatient Management / Inpatient Admission Transfer")]

Base URL: `api/v1/health-services/inpatient-management/admission-transfers`

| Method | Path | Kegunaan | Hak akses | Request | Response Data | Epic | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| POST | /admit | Admisi transfer IGD dengan reserve melalui coordinator | InpatientEpisode : Create | OpenAdmissionFromTransferRequest existing + ExpectedBedVersion; Idempotency-Key header | Data episode existing + OperationMeta | EPIC BM-02 | Existing; guard diperbarui |

`ExpectedBedVersion:long` wajib, nilainya dari hasil available-beds terbaru, tidak ditebak. Field/header hilang →400; akses/scope tidak sah →403; versi/holder/key konflik →409; tidak siap atau dependency aktivasi →422/503 sesuai13.4. Sukses pertama/replay tetap HTTP201 dengan envelope existing; replay ditandai OperationMeta.IsReplay. Endpoint ini pengecualian eksplisit dari sukses200 pada13.1, sesuai controller existing.

Writer menjalankan komposisi transaksi backend14.14; lookup receipt actor/key/hash mendahului pembukaan episode. ResultKind=Reservation menunjuk reservasi hasil wrapper dan episode terkait. GET own outcome tetap tidak mengembalikan PHI. Kontrak ini tidak menambah endpoint/permission IGD atau keputusan admisi baru. Bukti source dan keterbatasan helper frontend ada pada impact scan terbaru; perilaku target belum tersedia.
