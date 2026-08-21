# Permission dan Audit Matrix — Modul Rawat Inap

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| `contract_version` | `0.1.0` |
| Status | `draft` |
| Owner | Product/Domain Owner sementara sesuai `RWI-DEC-006`; pemilik keamanan/privasi **belum ditunjuk** |
| `input_revision` | `00-interview-decisions.md` revision `2`; `contracts/api-contract.md` revision `0.1.0` |
| Backend SHA | `5afb54b` |
| Dampak kompatibilitas | Butir hak akses baru bersifat aditif. Terdaftar otomatis oleh `AccessMenuSeeder` saat aplikasi dinyalakan |

String pada kolom "String yang dipakai" ditulis **apa adanya** supaya implementer menyalin, bukan
menerjemahkan.

Konvensi project: **GET tidak dicatat logger.** Payload log hanya memuat `EntityId`, controller,
action, dan status — tidak pernah memuat kolom bertanda sensitif pada kamus data.

---

## 1. Cara kerja hak akses di repository ini

Sudah terbukti dari source pada `RWI-TRC-007`:

1. Controller diberi `[AccessController(...)]` yang menyebut kode modul dan nama controller.
2. Setiap endpoint diberi `[AccessAction(...)]` dan `[AccessPermission("Resource", "Action")]`.
3. Saat aplikasi dinyalakan, `Seeders/AccessMenuSeeder.cs` menyisir seluruh endpoint dan membuat
   baris modul, controller, dan action di database bila belum ada.
4. Saat permintaan masuk, `Filters/AccessPermissionFilter.cs` memeriksa apakah peran pengguna punya
   akses ke pasangan controller dan action tersebut.

**Konsekuensinya bagi modul ini:** tidak perlu membangun mesin hak akses baru. Cukup memberi
atribut yang sama, dan butir haknya muncul sendiri.

### 1.1 Atribut controller yang dipakai

| Controller | `moduleCode` | `ControllerName` |
| --- | --- | --- |
| `InpatientEpisodeController` | `HEALTH_SERVICE_INPATIENT` | `InpatientEpisode` |
| `InpatientBedOccupancyController` | `HEALTH_SERVICE_INPATIENT` | `InpatientBedOccupancy` |
| `InpatientDischargeController` | `HEALTH_SERVICE_INPATIENT` | `InpatientDischarge` |
| `InpatientCensusController` | `HEALTH_SERVICE_INPATIENT` | `InpatientCensus` |
| `InpatientMonitoringController` | `HEALTH_SERVICE_INPATIENT` | `InpatientMonitoring` |
| `InpatientSettingController` | `HEALTH_SERVICE_MASTER_DATA` | `InpatientSetting` |
| `InpatientClearanceItemController` | `HEALTH_SERVICE_MASTER_DATA` | `InpatientClearanceItem` |

---

## 2. Matriks endpoint, hak akses, dan pencatatan

### 2.1 Inpatient Episode

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger |
| --- | --- | --- | --- | :---: |
| `GET /episodes/filters/metadata` | `InpatientEpisode` | `Read` | `[AccessPermission("InpatientEpisode", "Read")]` | Tidak |
| `GET /episodes/summary` | `InpatientEpisode` | `Read` | `[AccessPermission("InpatientEpisode", "Read")]` | Tidak |
| `GET /episodes` | `InpatientEpisode` | `Read` | `[AccessPermission("InpatientEpisode", "Read")]` | Tidak |
| `GET /episodes/{id}` | `InpatientEpisode` | `Read` | `[AccessPermission("InpatientEpisode", "Read")]` | Tidak |
| `GET /episodes/{id}/status-history` | `InpatientEpisode` | `Read` | `[AccessPermission("InpatientEpisode", "Read")]` | Tidak |
| `POST /episodes` | `InpatientEpisode` | `Create` | `[AccessPermission("InpatientEpisode", "Create")]` | Ya |
| `PUT /episodes/{id}` | `InpatientEpisode` | `Update` | `[AccessPermission("InpatientEpisode", "Update")]` | Ya |
| `PATCH /episodes/{id}/cancel` | `InpatientEpisode` | `Update` | `[AccessPermission("InpatientEpisode", "Update")]` | Ya |
| `POST /episodes/{id}/doctor-assignments` | `InpatientEpisode` | `Update` | `[AccessPermission("InpatientEpisode", "Update")]` | Ya |
| `GET /episodes/{id}/doctor-assignments` | `InpatientEpisode` | `Read` | `[AccessPermission("InpatientEpisode", "Read")]` | Tidak |
| `POST /episodes/{id}/nurse-assignments` | `InpatientEpisode` | `Update` | `[AccessPermission("InpatientEpisode", "Update")]` | Ya |
| `GET /episodes/{id}/nurse-assignments` | `InpatientEpisode` | `Read` | `[AccessPermission("InpatientEpisode", "Read")]` | Tidak |
| `POST /episodes/{id}/correction-sessions` | `InpatientEpisode` | `Reopen` | `[AccessPermission("InpatientEpisode", "Reopen")]` | Ya |
| `PATCH /episodes/{id}/correction-sessions/{sessionId}/close` | `InpatientEpisode` | `Reopen` | `[AccessPermission("InpatientEpisode", "Reopen")]` | Ya |

### 2.2 Bed Occupancy

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger |
| --- | --- | --- | --- | :---: |
| `GET /bed-occupancies/available-beds` | `InpatientBedOccupancy` | `Read` | `[AccessPermission("InpatientBedOccupancy", "Read")]` | Tidak |
| `GET /bed-occupancies/bed-board` | `InpatientBedOccupancy` | `Read` | `[AccessPermission("InpatientBedOccupancy", "Read")]` | Tidak |
| `POST /bed-occupancies/reservations` | `InpatientBedOccupancy` | `Create` | `[AccessPermission("InpatientBedOccupancy", "Create")]` | Ya |
| `PATCH /bed-occupancies/reservations/{id}/cancel` | `InpatientBedOccupancy` | `Update` | `[AccessPermission("InpatientBedOccupancy", "Update")]` | Ya |
| `POST /bed-occupancies/placements` | `InpatientBedOccupancy` | `Create` | `[AccessPermission("InpatientBedOccupancy", "Create")]` | Ya |
| `POST /bed-occupancies/placements/transfer` | `InpatientBedOccupancy` | `Transfer` | `[AccessPermission("InpatientBedOccupancy", "Transfer")]` | Ya |
| `GET /bed-occupancies/placements/by-episode/{episodeId}` | `InpatientBedOccupancy` | `Read` | `[AccessPermission("InpatientBedOccupancy", "Read")]` | Tidak |

### 2.3 Inpatient Discharge

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger |
| --- | --- | --- | --- | :---: |
| `POST /discharges/{episodeId}/decide` | `InpatientDischarge` | `Update` | `[AccessPermission("InpatientDischarge", "Update")]` | Ya |
| `GET /discharges/{episodeId}/summary` | `InpatientDischarge` | `Read` | `[AccessPermission("InpatientDischarge", "Read")]` | Tidak |
| `PUT /discharges/{episodeId}/summary` | `InpatientDischarge` | `Update` | `[AccessPermission("InpatientDischarge", "Update")]` | Ya |
| `PATCH /discharges/{episodeId}/summary/sign` | `InpatientDischarge` | `Sign` | `[AccessPermission("InpatientDischarge", "Sign")]` | Ya |
| `GET /discharges/{episodeId}/clearance` | `InpatientDischarge` | `Read` | `[AccessPermission("InpatientDischarge", "Read")]` | Tidak |
| `POST /discharges/{episodeId}/clearance/{itemId}/mark` | `InpatientDischarge` | `Update` | `[AccessPermission("InpatientDischarge", "Update")]` | Ya |
| `POST /discharges/{episodeId}/financial-clearance` | `InpatientFinancialClearance` | `Update` | `[AccessPermission("InpatientFinancialClearance", "Update")]` | Ya |
| `GET /discharges/{episodeId}/closure-readiness` | `InpatientDischarge` | `Read` | `[AccessPermission("InpatientDischarge", "Read")]` | Tidak |
| `POST /discharges/{episodeId}/close` | `InpatientEpisode` | `Close` | `[AccessPermission("InpatientEpisode", "Close")]` | Ya |
| `POST /discharges/{episodeId}/close-with-override` | `InpatientEpisode` | `CloseOverride` | `[AccessPermission("InpatientEpisode", "CloseOverride")]` | Ya |

### 2.4 Census dan Monitoring

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger |
| --- | --- | --- | --- | :---: |
| `GET /census/filters/metadata` | `InpatientCensus` | `Read` | `[AccessPermission("InpatientCensus", "Read")]` | Tidak |
| `GET /census/summary` | `InpatientCensus` | `Read` | `[AccessPermission("InpatientCensus", "Read")]` | Tidak |
| `GET /census` | `InpatientCensus` | `Read` | `[AccessPermission("InpatientCensus", "Read")]` | Tidak |
| `GET /monitoring/pending-closures` | `InpatientMonitoring` | `Read` | `[AccessPermission("InpatientMonitoring", "Read")]` | Tidak |
| `GET /monitoring/closures-without-financial-clearance` | `InpatientMonitoring` | `Read` | `[AccessPermission("InpatientMonitoring", "Read")]` | Tidak |
| `GET /monitoring/unassigned-nurse-episodes` | `InpatientMonitoring` | `Read` | `[AccessPermission("InpatientMonitoring", "Read")]` | Tidak |
| `GET /monitoring/bed-drift` | `InpatientMonitoring` | `Read` | `[AccessPermission("InpatientMonitoring", "Read")]` | Tidak |

### 2.5 Master Data

| Endpoint | Resource | Action | String yang dipakai | Dicatat logger |
| --- | --- | --- | --- | :---: |
| `GET /master-data/inpatient-settings` | `InpatientSetting` | `Read` | `[AccessPermission("InpatientSetting", "Read")]` | Tidak |
| `PUT /master-data/inpatient-settings/{id}` | `InpatientSetting` | `Update` | `[AccessPermission("InpatientSetting", "Update")]` | Ya |
| `GET /master-data/inpatient-clearance-items` | `InpatientClearanceItem` | `Read` | `[AccessPermission("InpatientClearanceItem", "Read")]` | Tidak |
| `GET /master-data/inpatient-clearance-items/{id}` | `InpatientClearanceItem` | `Read` | `[AccessPermission("InpatientClearanceItem", "Read")]` | Tidak |
| `POST /master-data/inpatient-clearance-items` | `InpatientClearanceItem` | `Create` | `[AccessPermission("InpatientClearanceItem", "Create")]` | Ya |
| `PUT /master-data/inpatient-clearance-items/{id}` | `InpatientClearanceItem` | `Update` | `[AccessPermission("InpatientClearanceItem", "Update")]` | Ya |
| `PATCH /master-data/inpatient-clearance-items/{id}/status` | `InpatientClearanceItem` | `Update` | `[AccessPermission("InpatientClearanceItem", "Update")]` | Ya |
| `DELETE /master-data/inpatient-clearance-items/{id}` | `InpatientClearanceItem` | `Delete` | `[AccessPermission("InpatientClearanceItem", "Delete")]` | Ya |

---

## 3. Peta peran ke butir hak akses

Ini **usulan pemetaan**, bukan kebijakan yang sudah disahkan. Penetapan peran ke butir hak akses
dilakukan admin lewat layar Role Access yang sudah ada, dan pemilik keamanan belum ditunjuk.

| Peran | Butir hak akses yang diusulkan |
| --- | --- |
| Petugas admisi | `InpatientEpisode : Read/Create/Update/Close`, `InpatientBedOccupancy : Read/Create/Update`, `InpatientDischarge : Read/Update`, `InpatientCensus : Read`, `InpatientMonitoring : Read` |
| Perawat pelaksana | `InpatientEpisode : Read`, `InpatientBedOccupancy : Read/Transfer`, `InpatientCensus : Read` |
| Kepala ruangan | Seperti perawat pelaksana, ditambah `InpatientEpisode : Update` untuk penugasan perawat dan pengalihan DPJP, serta `InpatientMonitoring : Read` |
| Dokter dan DPJP | `InpatientEpisode : Read`, `InpatientBedOccupancy : Read/Transfer`, `InpatientDischarge : Read/Update/Sign`, `InpatientCensus : Read` |
| Petugas kasir atau billing | `InpatientEpisode : Read`, `InpatientFinancialClearance : Update`, `InpatientCensus : Read` |
| Supervisor | Seluruh butir di atas, ditambah `InpatientEpisode : Reopen` dan `InpatientEpisode : CloseOverride` |
| Admin master data | `InpatientSetting : Read/Update`, `InpatientClearanceItem : Read/Create/Update/Delete` |

---

## 4. Kewenangan yang **tidak dapat** dijaga mesin hak akses

Ini bagian terpenting dokumen ini, dan yang paling mudah terlewat saat implementasi.

Mesin hak akses repository ini hanya mengenal **"peran ini boleh memanggil endpoint ini"**. Ia
sama sekali tidak mengenal **"orang ini boleh melakukan tindakan ini terhadap pasien ini"**.
Buktinya `RWI-TF-014`.

Padahal `RWI-RULE-030` menuntut tiga penjaga yang bersifat per-pasien:

| Penjaga | Isinya | Ditulis di mana |
| --- | --- | --- |
| `GUARD-INP-01` | Permintaan perpindahan oleh **dokter** hanya diterima bila dokter itu DPJP aktif episode tersebut | `InpBedOccupancyService.TransferAsync` |
| `GUARD-INP-02` | Keputusan pulang hanya diterima dari DPJP aktif | `InpDischargeService.DecideDischargeAsync` |
| `GUARD-INP-03` | Penandatanganan resume hanya diterima dari DPJP aktif | `InpDischargeService.SignSummaryAsync` |

### 4.1 Kenapa ini berisiko

Karena penjaga ditulis di dalam service dan bukan dipasang sebagai atribut, ia **hanya bekerja bila
benar-benar dipanggil**. Endpoint baru yang lupa memanggilnya akan lolos tanpa peringatan apa pun —
tidak ada kesalahan kompilasi, tidak ada peringatan runtime.

Risiko ini tercatat sebagai `RWI-RISK-004` dan diturunkan oleh `RWI-DEC-051` yang mewajibkan test.
Acceptance criteria yang membuktikannya adalah `RWI-AC-115`.

### 4.2 Yang **tidak** dijaga penjaga ini

`GUARD-INP-01` berlaku hanya untuk pemohon berperan dokter. Kepala ruangan, perawat pelaksana, dan
supervisor tetap boleh memindahkan pasien tanpa menjadi DPJP — itu keputusan `RWI-DEC-012` yang
tidak dicabut, dan risikonya sudah diterima secara sadar sebagai `RWI-RISK-001`.

---

## 5. Audit dan histori

### 5.1 Tiga lapis yang dipakai

| Lapis | Isinya | Dipakai untuk |
| --- | --- | --- |
| Kolom `IdentityModel` | Siapa dan kapan, hanya perubahan terakhir | Menjawab "siapa terakhir menyentuh baris ini" |
| `InpStatusHistory` | Riwayat lengkap perpindahan status, tidak dapat diubah | Menjawab "apa saja yang terjadi pada episode ini, urut" |
| `LoggerService` | Catatan aktivitas teknis | Menelusuri kejadian teknis, **bukan** bukti tindakan bisnis |

### 5.2 Kejadian yang wajib meninggalkan jejak tahan lama

| Kejadian | Disimpan di | Yang wajib ada |
| --- | --- | --- |
| Perpindahan status episode | `InpStatusHistory` | Dari, ke, pelaku, waktu, alasan, nomor urut, penanda orang atau sistem |
| Pemesanan dibuat, dipakai, gugur, dibatalkan | `InpBedReservation` + `InpStatusHistory` | Pelaku, waktu, tempat tidur |
| Penempatan dibuka dan ditutup | `InpBedPlacement` | Pelaku, waktu mulai, waktu berakhir, alasan |
| Pengalihan DPJP | `InpDoctorAssignment` | Dokter, masa berlaku, pengalih, alasan |
| Penggantian perawat | `InpNurseAssignment` | Perawat, masa berlaku, penugas |
| Penandaan kelayakan keuangan | `InpFinancialClearance` | Nilai, pelaku, waktu, catatan, penanda manual |
| Penandaan butir administrasi | `InpClearanceMark` | Butir, pelaku, waktu |
| Penandatanganan resume | `InpDischargeSummary` | Penandatangan, waktu |
| Penutupan menembus gerbang keuangan | `InpEpisode` + `InpStatusHistory` | Supervisor, waktu, alasan, penanda |
| Sesi koreksi | `InpCorrectionSession` | Supervisor, waktu buka dan tutup, alasan, daftar perubahan |

### 5.3 Tiga sifat yang mengikat

| Sifat | Isinya |
| --- | --- |
| Ditulis bersamaan | Baris jejak ditulis dalam transaksi yang sama dengan perubahan yang dijejakinya |
| Satu pintu | Seluruh perubahan status lewat `InpEpisodeService.ApplyStatusChangeAsync`. Tidak ada jalur lain |
| Tidak dapat diubah | Tidak disediakan endpoint update maupun delete untuk `InpStatusHistory` |

### 5.4 Kolom sensitif dan aturan logging

Kolom bertanda **Sensitif = Ya** pada kamus data **tidak boleh** masuk ke payload logger. Untuk
modul ini, kolom itu adalah:

| Tabel | Kolom sensitif |
| --- | --- |
| `InpEpisode` | `Notes` |
| `InpDischargeSummary` | `PrimaryDiagnosisText`, `SecondaryDiagnosisText`, `ProcedureSummary`, `DischargeMedicationNote`, `FollowUpInstruction`, `ClinicalSummary` |

Payload log untuk endpoint yang menyentuh tabel di atas hanya boleh memuat `EntityId`, nama
controller, nama action, dan kode status. **Tidak** boleh memuat isi diagnosis, isi instruksi
kontrol, maupun ringkasan klinis.

---

## 6. Privasi dan masa simpan

| Hal | Ketetapannya | Status |
| --- | --- | --- |
| Siapa boleh membaca resume pulang | Peran klinis dan admisi yang punya `InpatientDischarge : Read` | Usulan; menunggu pemilik privasi |
| Penyamaran isi resume pada daftar | Daftar hanya menampilkan nomor episode, nama pasien, dan cara pulang. Isi klinis hanya pada layar detail | Usulan |
| Masa simpan riwayat status | **Belum diputuskan** | `RWI-OQ-035`, keputusan hukum |
| Masa simpan riwayat penempatan | **Belum diputuskan** | Mengikuti `RWI-OQ-035` |

Pemilik keamanan dan privasi **belum ditunjuk**. Seluruh baris pada bagian ini berstatus usulan dan
wajib ditinjau sebelum modul dipakai melayani pasien sungguhan.

---

## 7. Traceability

| Bagian | Requirement dan decision asal |
| --- | --- |
| 1 | `RWI-TRC-007`, `RWI-TF-015` |
| 2 | `contracts/api-contract.md` revision `0.1.0` |
| 3 | `RWI-RULE-004`, `RWI-RULE-006`, `RWI-RULE-010`, `RWI-RULE-018`, `RWI-RULE-020`, `RWI-RULE-028`, `RWI-RULE-033` |
| 4 | `RWI-RULE-030`, `RWI-DEC-042`, `RWI-TF-014`, `RWI-RISK-004` |
| 5 | `RWI-RULE-031`, `RWI-DEC-043` |
| 6 | `RWI-OQ-035`, gerbang privasi pada dokumen keputusan |
