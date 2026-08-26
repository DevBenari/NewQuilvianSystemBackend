# API Contract — Modul Rawat Inap

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| `contract_version` | `0.3.0` |
| Status | `draft` |
| Owner | Product/Domain Owner sementara sesuai `RWI-DEC-006`; nama belum diisi |
| `approved_by` / `approved_at` | Belum ada |
| `input_revision` | `02-backend-architecture.md` revision `0.3`; `00-interview-decisions.md` revision `5` |
| Backend SHA | `5afb54b` |
| Dampak kompatibilitas | **Seluruhnya aditif.** Tidak ada endpoint existing yang berubah bentuknya. Satu endpoint existing berubah **perilakunya**, lihat bagian 7 |

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

> **Seluruh endpoint pada dokumen ini berstatus `Rencana (belum tersedia)`,** kecuali yang
> disebutkan lain. Tidak satu pun sudah ada di dalam kode pada SHA `5afb54b`.

Base URL modul: `api/v1/health-services/inpatient-management/`

---

## Health Services / Inpatient Management / Inpatient Episode

Base URL: `api/v1/health-services/inpatient-management/episodes`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Mengambil pilihan penyaring beserta nilai bawaannya untuk layar daftar episode | `InpatientEpisode : Read` | – | `ApiResponse<InpatientEpisodeFilterMetadataResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/summary` | Ringkasan jumlah episode per status | `InpatientEpisode : Read` | Query | `ApiResponse<InpatientEpisodeSummaryResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/` | Daftar episode bertingkat, dapat disaring unit layanan, status, tanggal, dan nama pasien | `InpatientEpisode : Read` | Query | `ApiResponse<InpatientEpisodePagedResult>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Detail satu episode beserta DPJP aktif, perawat aktif, dan lokasi terkini | `InpatientEpisode : Read` | – | `ApiResponse<InpatientEpisodeDetailResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/status-history` | Riwayat perpindahan status episode | `InpatientEpisode : Read` | – | `ApiResponse<List<InpatientStatusHistoryResponse>>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Membuka admisi. Membuat episode `Draft` dan menetapkan DPJP pertama | `InpatientEpisode : Create` | `OpenAdmissionRequest` | `ApiResponse<InpatientEpisodeDetailResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Mengubah isian admisi selama episode masih `Draft` | `InpatientEpisode : Update` | `UpdateAdmissionRequest` | `ApiResponse<InpatientEpisodeDetailResponse>` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/cancel` | Membatalkan admisi. Melepas pemesanan dan penempatan dalam satu tindakan | `InpatientEpisode : Update` | `CancelAdmissionRequest` | `ApiResponse<InpatientEpisodeDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/doctor-assignments` | Mengalihkan DPJP. Menutup penugasan lama dan membuka penugasan baru | `InpatientEpisode : Update` | `HandoverDoctorRequest` | `ApiResponse<InpatientDoctorAssignmentResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/doctor-assignments` | Riwayat DPJP episode | `InpatientEpisode : Read` | – | `ApiResponse<List<InpatientDoctorAssignmentResponse>>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/nurse-assignments` | Menugaskan atau mengganti perawat penanggung jawab | `InpatientEpisode : Update` | `AssignNurseRequest` | `ApiResponse<InpatientNurseAssignmentResponse>` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/isolation-requirement` | Menetapkan atau mengubah kebutuhan isolasi episode | `InpatientEpisode : SetIsolation` | `SetIsolationRequirementRequest` | `ApiResponse<InpatientEpisodeDetailResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/nurse-assignments` | Riwayat perawat penanggung jawab | `InpatientEpisode : Read` | – | `ApiResponse<List<InpatientNurseAssignmentResponse>>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/correction-sessions` | Membuka sesi koreksi pada episode yang sudah ditutup | `InpatientEpisode : Reopen` | `OpenCorrectionSessionRequest` | `ApiResponse<InpatientCorrectionSessionResponse>` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/correction-sessions/{sessionId}/close` | Menutup sesi koreksi beserta daftar perubahannya | `InpatientEpisode : Reopen` | `CloseCorrectionSessionRequest` | `ApiResponse<InpatientCorrectionSessionResponse>` | **Rencana (belum tersedia)** |

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
| `GET` | `/available-beds` | Mencari tempat tidur yang benar-benar dapat ditempati, sudah memperhitungkan pemesanan yang masih berlaku | `InpatientBedOccupancy : Read` | Query | `ApiResponse<AvailableBedPagedResult>` | **Rencana (belum tersedia)** |
| `GET` | `/bed-board` | Papan ketersediaan tempat tidur per unit layanan dan kamar | `InpatientBedOccupancy : Read` | Query | `ApiResponse<BedBoardResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/reservations` | Memesan tempat tidur untuk satu episode `Draft` | `InpatientBedOccupancy : Create` | `ReserveBedRequest` | `ApiResponse<BedReservationResponse>` | **Rencana (belum tersedia)** |
| `PATCH` | `/reservations/{id}/cancel` | Membatalkan pemesanan sebelum dipakai | `InpatientBedOccupancy : Update` | `CancelReservationRequest` | `ApiResponse<BedReservationResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/placements` | Menempatkan pasien ke tempat tidur dan mengaktifkan episode | `InpatientBedOccupancy : Create` | `PlacePatientRequest` | `ApiResponse<BedPlacementResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/placements/transfer` | Memindahkan pasien ke tempat tidur lain dalam satu tindakan utuh | `InpatientBedOccupancy : Transfer` | `TransferPatientRequest` | `ApiResponse<BedPlacementResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/placements/by-episode/{episodeId}` | Riwayat penempatan satu episode, dari tempat tidur pertama sampai terakhir | `InpatientBedOccupancy : Read` | – | `ApiResponse<List<BedPlacementResponse>>` | **Rencana (belum tersedia)** |

Kode status tambahan yang khas bagian ini:

| Kode | Arti bagi pengguna |
| --- | --- |
| 409 | Tempat tidur sudah ditempati atau sudah dipesan pasien lain. Ini yang muncul ketika dua petugas merebut tempat tidur yang sama |
| 422 | Tempat tidur tidak lolos pemeriksaan kelayakan. Sejak `0.3.0` ini mencakup lima alasan baru: penanda tempat tidur tidak menerima jenis kelamin pasien, jenis kelamin pasien belum tercatat, kamar sudah dihuni pasien berjenis kelamin berbeda, pasien butuh isolasi tetapi tempat tidurnya bukan isolasi, dan pasien tidak butuh isolasi tetapi tempat tidurnya isolasi |

**Bentuk jawaban penolakan kelayakan.** Jawaban 422 menyertakan **daftar aturan yang gagal**, bukan
satu kalimat umum. Petugas perlu tahu apakah yang menghalangi jenis kelaminnya, isolasinya, atau
keadaan tempat tidurnya, karena tindakan lanjutannya berbeda.

---

## Health Services / Inpatient Management / Inpatient Discharge

Base URL: `api/v1/health-services/inpatient-management/discharges`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `POST` | `/{episodeId}/decide` | DPJP memutuskan pasien boleh pulang beserta cara pulangnya | `InpatientDischarge : Update` | `DecideDischargeRequest` | `ApiResponse<InpatientEpisodeDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{episodeId}/record-departure` | Mencatat pasien sudah meninggalkan ruangan. Melepas tempat tidur seketika **tanpa** menutup episode | `InpatientDischarge : RecordDeparture` | `RecordDepartureRequest` | `ApiResponse<InpatientEpisodeDetailResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{episodeId}/summary` | Mengambil resume pulang episode beserta daftar versi sebelumnya bila ada | `InpatientDischarge : Read` | Query `includeRevisions` | `ApiResponse<DischargeSummaryResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{episodeId}/summary` | Menyusun atau memperbarui resume pulang | `InpatientDischarge : Update` | `UpsertDischargeSummaryRequest` | `ApiResponse<DischargeSummaryResponse>` | **Rencana (belum tersedia)** |
| `PATCH` | `/{episodeId}/summary/sign` | DPJP menandatangani resume pulang | `InpatientDischarge : Sign` | `SignDischargeSummaryRequest` | `ApiResponse<DischargeSummaryResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{episodeId}/clearance` | Daftar butir administrasi beserta status penandaannya | `InpatientDischarge : Read` | – | `ApiResponse<ClearanceChecklistResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{episodeId}/clearance/{itemId}/mark` | Menandai satu butir daftar periksa administrasi | `InpatientDischarge : Update` | `MarkClearanceItemRequest` | `ApiResponse<ClearanceChecklistResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{episodeId}/financial-clearance` | Petugas kasir menandai kelayakan keuangan | `InpatientFinancialClearance : Update` | `MarkFinancialClearanceRequest` | `ApiResponse<FinancialClearanceResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{episodeId}/closure-readiness` | Memeriksa kelima syarat penutupan dan menampilkan mana yang belum terpenuhi | `InpatientDischarge : Read` | – | `ApiResponse<ClosureReadinessResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{episodeId}/close` | Menutup episode dan melepas tempat tidur | `InpatientEpisode : Close` | `CloseEpisodeRequest` | `ApiResponse<InpatientEpisodeDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{episodeId}/close-with-override` | Supervisor menutup episode menembus gerbang keuangan | `InpatientEpisode : CloseOverride` | `CloseEpisodeOverrideRequest` | `ApiResponse<InpatientEpisodeDetailResponse>` | **Rencana (belum tersedia)** |

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

## Health Services / Inpatient Management / Inpatient Census

Base URL: `api/v1/health-services/inpatient-management/census`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan penyaring census | `InpatientCensus : Read` | – | `ApiResponse<CensusFilterMetadataResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/summary` | Ringkasan jumlah pasien dirawat per unit layanan dan per kelas | `InpatientCensus : Read` | Query | `ApiResponse<CensusSummaryResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/` | Daftar pasien yang sedang dirawat beserta lokasi, DPJP, perawat, dan lama dirawat | `InpatientCensus : Read` | Query | `ApiResponse<CensusPagedResult>` | **Rencana (belum tersedia)** |

---

## Health Services / Inpatient Management / Inpatient Monitoring

Base URL: `api/v1/health-services/inpatient-management/monitoring`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/pending-closures` | Daftar pantau episode yang sudah boleh pulang tetapi belum ditutup melewati ambang waktu | `InpatientMonitoring : Read` | Query | `ApiResponse<PendingClosurePagedResult>` | **Rencana (belum tersedia)** |
| `GET` | `/closures-without-financial-clearance` | Daftar pantau episode yang ditutup menembus gerbang keuangan | `InpatientMonitoring : Read` | Query | `ApiResponse<OverrideClosurePagedResult>` | **Rencana (belum tersedia)** |
| `GET` | `/unassigned-nurse-episodes` | Daftar episode aktif yang belum punya perawat penanggung jawab | `InpatientMonitoring : Read` | Query | `ApiResponse<UnassignedNursePagedResult>` | **Rencana (belum tersedia)** |
| `GET` | `/bed-drift` | Laporan selisih antara salinan status tempat tidur dan catatan penempatan | `InpatientMonitoring : Read` | Query | `ApiResponse<BedDriftPagedResult>` | **Rencana (belum tersedia)** |
| `GET` | `/isolation-mismatch` | Daftar pantau episode yang kebutuhan isolasinya tidak cocok dengan sifat tempat tidur yang sedang ditempati | `InpatientMonitoring : Read` | Query | `ApiResponse<IsolationMismatchPagedResult>` | **Rencana (belum tersedia)** |

**Daftar pantau ketiga yang tidak ada di sini.** `RWI-RULE-023` menyebut tiga daftar pantau, dan
salah satunya adalah kepatuhan pengkajian awal dan verifikasi CPPT. Daftar itu **tidak** dirancang
pada revisi ini karena bergantung pada slice dokumentasi klinis yang masih menunggu `DEC-INP-001`.

---

## Health Services / Master Data / Inpatient Setting

Base URL: `api/v1/health-services/master-data/inpatient-settings`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Membaca pengaturan Rawat Inap yang berlaku | `InpatientSetting : Read` | – | `ApiResponse<InpatientSettingResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Mengubah nilai pengaturan | `InpatientSetting : Update` | `UpdateInpatientSettingRequest` | `ApiResponse<InpatientSettingResponse>` | **Rencana (belum tersedia)** |

---

## Health Services / Master Data / Inpatient Clearance Item

Base URL: `api/v1/health-services/master-data/inpatient-clearance-items`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar butir administrasi | `InpatientClearanceItem : Read` | Query | `ApiResponse<InpatientClearanceItemPagedResult>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Detail satu butir | `InpatientClearanceItem : Read` | – | `ApiResponse<InpatientClearanceItemResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Menambah butir baru | `InpatientClearanceItem : Create` | `CreateInpatientClearanceItemRequest` | `ApiResponse<InpatientClearanceItemResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Mengubah butir | `InpatientClearanceItem : Update` | `UpdateInpatientClearanceItemRequest` | `ApiResponse<InpatientClearanceItemResponse>` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/status` | Mengaktifkan atau menonaktifkan butir | `InpatientClearanceItem : Update` | `UpdateStatusRequest` | `ApiResponse<InpatientClearanceItemResponse>` | **Rencana (belum tersedia)** |
| `DELETE` | `/{id}` | Menandai butir terhapus | `InpatientClearanceItem : Delete` | `DeleteRequest` | `ApiResponse<InpatientClearanceItemResponse>` | **Rencana (belum tersedia)** |

---

## 7. Perubahan pada endpoint yang sudah ada

### Health Services / Master Data / Bed

Base URL: `api/v1/health-services/master-data/beds`
Sumber as-is: `Areas/HealthServices/MasterData/Controllers/BedController.cs` pada SHA `5afb54b`

| Method | Path | Perubahan | Alasan | Status |
| --- | --- | --- | --- | --- |
| `PATCH` | `/{id}/availability` | Menolak nilai `Reserved` dan `Occupied` dengan kode 422. Nilai `Cleaning`, `Maintenance`, `Blocked`, dan `Inactive` tetap diterima | `RWI-RULE-027` aturan 4 dan 5: status penghunian hanya boleh lahir dari tindakan Rawat Inap | **Rencana perubahan perilaku** |

Pesan penolakannya: *"Status Terisi dan Dipesan hanya dapat diubah lewat modul Rawat Inap. Untuk
menutup tempat tidur sementara, pakai status Pembersihan, Perbaikan, atau Diblokir."*

**Yang tidak berubah:** bentuk request, bentuk response, kode status yang sudah ada, dan seluruh
endpoint lain pada grup ini. Ini perubahan perilaku, bukan perubahan kontrak.

**Persetujuan yang dibutuhkan:** pemilik modul `MasterData`, tercatat sebagai `RWI-OQ-033`, belum
ada.

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
