# BM-IMP-20261011-01 — Pemeriksaan ulang bukti Bed Management

Tanggal: 11 Oktober 2026. Revision: `1`, status **draft**. Blueprint `RWI-BP-001`, sub-modul `episode-rawat-inap`, kontrak target mengikuti manifest anak. Keputusan produk tetap revision `46`, `RWI-DEC-274`–`294` dan `RWI-AC-396`–`426`; gate tetap `1.12`, `BM-RCG-20261010-01`. Approval desain: `approved_by: null`, `approved_at: null`.

## 1. Batas pemeriksaan dan asal bukti

Pemeriksaan ini memakai `trace-existing-capabilities` untuk melanjutkan finalisasi dokumen, tanpa mengubah source aplikasi. Source backend menjadi bukti perilaku yang tersedia; source frontend menunjukkan konsumen. Tidak menjalankan aplikasi, API, PostgreSQL, migration, atau UAT.

| Repository | Snapshot desain sebelumnya | HEAD yang diperiksa ulang |
| --- | --- | --- |
| Backend | `d4e1eca06fb28c05934c68c1e51a4dca01935a10` | `c5d3b5bfdc15c014a2399d904d7c4c2207505905` |
| Frontend | `969acfcc04cdf31074a1911e9827c31d25ddadd0` | `5aa2c7c70754e6131e4243b5ea3ee5f768b7a5d2` |

Kedua working tree bersih saat sesi lanjutan dimulai. Perbandingan Git menemukan 55 path backend berubah, termasuk 12 path source, dan 29 path frontend berubah. Source inti bed, model/configuration hunian, master bed, serta hook papan tidak berubah **antara kedua commit Git**. Ini tidak membuktikan kesamaan dengan working tree kotor sesi lama.

Folder `artifacts/bed-management/` beserta audit `BM-AUD-20261010-01` dan daftar 62 fingerprint lama tidak ditemukan di workspace maupun pencarian dalam `Documents/Quilvian`. Tidak dibuat salinan yang mengaku sebagai bukti asli. Referensi dan hash lama pada gate/interview dipertahankan sebagai provenance historis. Pernyataan sesi lama bahwa 62 fingerprint cocok tidak dapat dijalankan ulang dari daftar aslinya.

[Snapshot bukti terbaru](./bed-management-source-snapshot-20261011.json) mencatat hash source yang dipilih, hash masukan hulu, kedua HEAD, dan daftar perubahan antarcommit. Hash source adalah hasil pembacaan file terbaru; bukan rekonstruksi 62 fingerprint lama. Keputusan dan gate tidak disunting. Bukti ini menjadi pendamping audit lama untuk draft current, tanpa menaikkan gate menjadi approval implementasi.

## 2. Indeks source yang diperiksa

Path backend relatif terhadap `NewQuilvianSystemBackend`, path frontend relatif terhadap `QuilvianSystemFrontendDev`. Hash lengkap setiap file ada pada snapshot JSON. Nomor baris mengacu HEAD terbaru di atas.

| Ref | Repository / path | Simbol atau baris penting | Fakta |
| --- | --- | --- | --- |
| S01 | BE `Areas/HealthServices/MasterData/Models/MstBed.cs`, `MstRoom.cs`, `MstPatientClass.cs`, `Enums/BedStatus.cs` | `MstPatientClass.ClassLevel`; `BedStatus` | Master tetap pemilik lokasi/kelas; enum raw 0–7; default level 0 tidak membuktikan urutan resmi |
| S02 | BE `Areas/HealthServices/InPatientManagement/Models/InpBedPlacement.cs`, `InpBedReservation.cs` | Hunian, reservasi dan versi koreksi | Riwayat hunian tersedia; kategori manual/snapshot lokasi dan alasan pembatalan target belum ada |
| S03 | BE `Repositories/Configurations/HealthServices/InPatientManagement/InpBedPlacementConfiguration.cs`, `InpBedReservationConfiguration.cs` | Partial unique index baris aktif | Index tiap tabel tidak menjadi satu constraint lintas reservasi dan hunian |
| S04 | BE `Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs` | `SearchAvailableBedsAsync` 92; `GetBedBoardAsync` 267; `ReserveBedAsync` 451; `CancelReservationAsync` 583; `TransferAsync` 949; `ReleaseActivePlacementAsync` 1879; `ReleaseBedStatusCopyAsync` 1985 | Reserve memeriksa eligibility sebelum lock; transfer memakai transaksi; pelepasan menulis Available; reason cancel tidak disimpan |
| S05 | BE `Areas/HealthServices/MasterData/Controllers/BedController.cs` | Summary 105; POST 319; PUT 404; status 479; availability 536; DELETE 608 | Summary memakai raw status; availability memeriksa placement aktif, belum seluruh holder/writer |
| S06 | BE `Areas/HealthServices/InPatientManagement/Controllers/InpatientBedOccupancyController.cs`, `DTOs/InpatientBedOccupancyDtos.cs` | Route/tag/permission; `TransferPatientRequest`, `CancelReservationRequest` | Kontrak existing delapan endpoint; reason cancel nullable; kategori manual dan versi bed target belum ada |
| S07 | BE `Areas/HealthServices/InPatientManagement/Services/InpDischargeService.Departure.cs`, `InpDischargeService.Closure.cs` | `ReleaseActivePlacementAsync` pada baris 47 / 815 | Kepergian dan penutupan memakai jalur release existing; target harus mempertahankan perlindungan episode lama |
| S08 | BE `Areas/HealthServices/InPatientManagement/Services/InpRoomTransferReportService.cs`, `InpIntegrationReplayService.cs` | Target transfer/koreksi; `SupersededByCorrectionId == null` | Laporan transfer berbeda dari seluruh segmen penggunaan; replay tidak memakai `!IsSuperseded` sebagai filter efektif |
| S09 | BE `Program.cs`, `Repositories/ApplicationDbContext.cs` | DI 647/671/679/681; DbSet 859–860 | Service bed dan storage existing terpasang; target readiness/HK/receipt belum tersedia di source |
| S10 | BE `Filters/AccessPermissionFilter.cs`, `Services/Security/AccessPermissionService.cs` | `HasAccessAsync` | Mesin hak akses tersedia; grant akun/scope dan penunjukan verifikator nyata tetap belum dibuktikan |
| S11 | FE `src/utils/menu-sidebar/menu-items.jsx`, `src/components/view/health-services/inpatient-management/inpatient-bed-board-view.jsx` | Menu 1674–1677; aksi papan | Menu masih Papan Tempat Tidur; belum tiga tab target. Visibilitas dan scope harus diperiksa pada implementasi |
| S12 | FE `src/lib/hooks/health-services/inpatient-management/use-inpatient-bed-board.jsx`, `use-inpatient-bed-board-actions.jsx` | `refresh`, `requestConfirmAdmission`, `requestCancelReservation` | Refresh menaikkan token dan tidak mengembalikan Promise hasil baca; dialog memakai bed dari closure lama |
| S13 | FE `src/lib/services/health-services/inpatient-management/bed-occupancy.service.js`, `src/utils/health-services/inpatient-management/inpatient-bed-utils.jsx` | Helper API, payload cancel | Konsumen existing dapat diperluas; reason/versi/key target memerlukan pembaruan bersama backend |
| S14 | BE `Areas/HealthServices/InPatientManagement/Services/InpAdmissionTransferService.cs`, `Controllers/InpatientAdmissionTransferController.cs`, `DTOs/InpatientAdmissionTransferDtos.cs` | `ExecuteTransferAdmissionAsync` 203; `ReserveBedAsync` 273; POST admit 75 | Jalur admisi transfer IGD memanggil reserve secara internal, setelah membuka episode; kegagalan reserve memakai kompensasi cancel episode |
| S15 | FE `src/lib/services/health-services/inpatient-management/inpatient-admission-transfer.service.js` | `admitFromTransfer` 67 | Helper POST admit tersedia; pencarian source tidak menemukan pemanggilan helper tersebut dari UI. Komentar “atomik” bukan bukti transaksi backend |

## 3. Peta kemampuan current

ID `BM-CAP-01`–`17` dipertahankan dari pemetaan yang sudah tersimpan pada module map bagian 9 dan gate bagian 21. Kolom status berikut adalah hasil pemeriksaan source terbaru, satu status per kemampuan; tidak mengaku menyalin isi audit yang hilang.

| ID | Kebutuhan / pemilik | Bukti | Status | Gap atau adapter | Risiko |
| --- | --- | --- | --- | --- | --- |
| BM-CAP-01 | Master bed/room/class, MasterData | S01/S05 | Reuse with adapter | Gunakan master existing dan guard writer bersama | Duplikasi master bila ownership diabaikan |
| BM-CAP-02 | Satu menu dan tiga tab, frontend Rawat Inap | S11 | Extend | Rename leaf; tambah tab target | Layar tidak terjangkau atau kemampuan ganda |
| BM-CAP-03 | Monitoring/counts/ketersediaan, InPatientManagement | S04/S05 | Repair | Predicate dan proyeksi belum tunggal | Bed tidak layak dianggap tersedia |
| BM-CAP-04 | Reservasi dengan satu holder sah, InPatientManagement | S03/S04 | Repair | Recheck dua sumber holder sesudah lock | Reservasi versus hunian bersamaan |
| BM-CAP-05 | Expiry server, InPatientManagement | S04; `InpSettingService` default 120 menit | Reuse with adapter | Pertahankan expiry, tambah guard/readiness | Browser timer dianggap keputusan server |
| BM-CAP-06 | Reminder/perpanjangan | Pencarian scope bed; DEC-283 | Missing | Tidak dibuat dalam MVP sesuai keputusan | Dimasukkan tanpa keputusan baru |
| BM-CAP-07 | Enam status dengan tahap cleaning, InPatientManagement | S01/S04/S09 | Missing | Readiness target belum tersedia | Raw Cleaning dianggap siap |
| BM-CAP-08 | Bed bekas pasien tertahan, InPatientManagement | S04/S07 | Repair | Release existing masih Available | Pasien baru ditempatkan sebelum verifikasi |
| BM-CAP-09 | HK mulai/selesai dan perawat verifikasi | S09; pencarian class target | Missing | Entity/service/endpoint target belum ada | SOP/penugasan fiktif |
| BM-CAP-10 | Tutup/buka beralasan, MasterData | S05 | Repair | Guard belum meliputi semua holder/writer | Master menimpa fakta pasien |
| BM-CAP-11 | Transfer satu transaksi, InPatientManagement | S04 | Reuse with adapter | Pertahankan transaksi/outbox/handover; tambah guard/kategori | Callback dianggap alasan reverse transfer |
| BM-CAP-12 | Form/detail/report transfer | S04/S06/S08 | Extend | Kategori manual dan snapshot target | Kategori dipilih otomatis/tidak cocok |
| BM-CAP-13 | Semua segmen penggunaan per bed/periode | S02/S08 | Extend | Histori per episode/laporan transfer belum query target | Pasien tanpa transfer hilang |
| BM-CAP-14 | Alasan pembatalan/koreksi | S04/S06/S08 | Repair | Persist reason; pertahankan versi koreksi | Alasan hilang atau histori ditimpa |
| BM-CAP-15 | Permission/scope/masking dan aksi UI | S10/S11/S12 | Repair | Grant/scope target belum terbukti; server wajib membatasi | Identitas pasien bocor pada profil operasional |
| BM-CAP-16 | Kesiapan lingkungan | S09; tanpa runtime/DB/SOP/grant aktual | Unknown | BM-G01–04 tetap terbuka | Source dianggap bukti kesiapan produksi |
| BM-CAP-17 | Billing dari timeline existing | S04/S08; kontrak integrasi-billing 1.1.0 bagian 3.4 | Reuse with adapter | Pertahankan event canonical dan koreksi | Salah filter menghilangkan hunian transfer |

Fakta dan inferensi dipisahkan: bukti source menunjukkan jalur yang dapat salah; kejadian race atau kegagalan operasional nyata belum direproduksi. Temuan lama F01–F07 tetap berstatus belum dibuktikan selesai. Tidak menetapkan ulang arti detail setiap ID F yang audit aslinya hilang.

## 4. Kontrak as-is yang relevan

Semua endpoint berikut **tersedia di source**, belum dibuktikan runtime. Envelope memakai `ApiResponse<T>` existing. Detail DTO ada di source S06/S14, bukan DTO target draft. UUID adalah identifier existing; actor berasal dari pengguna terautentikasi.

### [Tags("Health Services / Inpatient Management / Bed Occupancy")]

Base URL: `api/v1/health-services/inpatient-management/bed-occupancies`.

| Method | Path | Kegunaan | Hak akses | Request | Response | HTTP utama |
| --- | --- | --- | --- | --- | --- | --- |
| GET | /available-beds | Cari kandidat | InpatientBedOccupancy : Read | AvailableBedQuery | AvailableBedPagedResult | 200 |
| GET | /bed-board | Papan existing | InpatientBedOccupancy : Read | serviceUnitId? | BedBoardResponse | 200 |
| POST | /reservations | Pesan bed | InpatientBedOccupancy : Create | ReserveBedRequest | BedReservationResponse | 200/400/404/409/422 |
| PATCH | /reservations/{id}/cancel | Batal pesanan | InpatientBedOccupancy : Update | CancelReservationRequest | BedReservationResponse | 200/404/409 |
| POST | /placements | Tempatkan pasien | InpatientBedOccupancy : Create | PlacePatientRequest | BedPlacementResponse | 200/400/404/409/422 |
| POST | /placements/transfer | Transfer bed | InpatientBedOccupancy : Transfer | TransferPatientRequest | BedPlacementResponse | 200/400/404/409/422 |
| POST | /placements/{placementId}/corrections | Koreksi hunian | InpatientBedOccupancy : Correct | CorrectPlacementRequest | BedPlacementResponse | 200/400/404/409/422 |
| GET | /placements/by-episode/{episodeId} | Histori episode | InpatientBedOccupancy : Read | episodeId | List<BedPlacementResponse> | 200/404 |

### [Tags("Health Services / Inpatient Management / Inpatient Admission Transfer")]

Base URL: `api/v1/health-services/inpatient-management/admission-transfers`.

| Method | Path | Kegunaan | Hak akses | Request | Response | HTTP utama |
| --- | --- | --- | --- | --- | --- | --- |
| POST | /admit | Buka episode dari transfer IGD dan reserve bed | InpatientEpisode : Create | OpenAdmissionFromTransferRequest | ApiResponse<object>, Data dari hasil episode service | 201/400/404/409/422 |

401/403 berasal dari autentikasi/permission; 409 menandai konflik, 422 aturan bisnis. Tidak menambah permission baru untuk jalur admisi ini. Karena tersedia dari controller, jalur ini tetap harus dijaga saat cutover walaupun helper frontend belum ditemukan dipakai.

## 5. Dampak, rekomendasi, dan batas penutupan

1. Tambahkan wrapper admisi transfer IGD pada inventaris writer target. `ExpectedBedVersion` dan kunci intent harus diteruskan ke reserve coordinator. Retry wrapper harus memulihkan episode/reservasi yang sudah dibuat, bukan membuka episode kedua. Tetapkan pemilik transaksi dan jalur gagal dalam desain; jangan mengklaim atomisitas lintas layanan yang source belum buktikan. Pilihan teknis final berada pada backend14.14, bukan pernyataan bahwa source as-is sudah memenuhi target.
2. Selaraskan nama event pada rancangan dengan kontrak Billing canonical: `BED_OCCUPIED` untuk penempatan/transfer biasa dan `OCCUPANCY_CORRECTED` untuk koreksi. Tidak mengubah schema event.
3. Perbaiki hash current, termasuk perbedaan akhir baris LF/CRLF. Simpan SHA256 byte aktual untuk draft current; snapshot JSON juga menyimpan hash teks LF agar perubahan format dapat dibedakan dari perubahan isi.
4. Hasil pemeriksaan ini tidak membuka pertanyaan pilihan produk. Bukti kelas resmi, SOP/penugasan, akses/privasi, dan implementasi/UAT tetap diselesaikan melalui BM-G01–04. Jika bukti baru mengubah state/data/authority, nilai ulang slice terkait.

Pemicu audit ulang: perubahan HEAD/source yang memengaruhi writer, predicate, scope, billing, atau konsumen; perubahan keputusan/gate; bukti lingkungan yang bertentangan. Bukti asli audit lama tetap `UNAVAILABLE`; pemeriksaan baru tidak menghapus keterbatasan tersebut. Langkah sesudah approval draft adalah `plan-module-delivery`; bukan implementasi otomatis.
