# Laporan Perubahan Backend — `BE-RWI-235`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-235` |
| Judul | Writer master mengikuti aturan denah |
| Slice | Menambah, mengurangi, melabeli bed, dan mengubah kelas kamar mengikuti aturan denah tanpa menyentuh penghuni |
| Roadmap | [backend-roadmap-integrasi-denah.md](../../../roadmap/backend-roadmap-integrasi-denah.md) — kartu `BE-RWI-235` |
| Trace | FR-DNH-009, 010, 016; RWI-DEC-304, 317, 318, 322; RWI-AC-444, 454–457, 460; VAL-DNH-07, 08, 10; INV-DNH-02; DNH-AT-444, 454–457, 902, 903, 909 |
| Contract version | `0.13.0` approved — Muhammad Hamzah, 16 Oktober 2026 (`RWI-DEC-324`) |
| Dependency | `BE-RWI-232` ✅ |
| Klasifikasi | `HEAVY` — skor 10: satu repo 0, diperiksa > 20 = 2, diubah 5 = 1, logika kompleks 2, perilaku kontrak 1, persistence existing 1, auth terkait 1, workflow luas 2 |
| Task mode | `BACKEND` — instruksi eksplisit pengguna 16 Oktober 2026 |
| Target tulis | `BedMasterWriteService.cs`, `RoomMasterWriteService.cs`, `PatientClassMasterWriteService.cs`, `BedMutationService.cs`, `BedDtos.cs` |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `ca09cae909597442385ce4513be9c803549be10e` (branch `MHamzah`, belum di-commit) |
| Tanggal | 16 Oktober 2026 |
| Status | ✅ Selesai — source dan build terbukti; bukti nama kelas resmi menunggu D-DNH-1 |

---

## 1. Masalah yang diperbaiki

Label bed diketik manual, sehingga label "329-C" bisa ganda atau tidak konsisten dengan nomor kamar. Bed dapat dihapus lunak selama status masternya Available, walaupun bed itu pernah ditempati — riwayat hunian kehilangan rujukan bed yang "hilang". Penolakan ubah kelas kamar berpenghuni hanya berbunyi "Bed masih ditempati atau dipesan" tanpa jumlah, dan ganti nama kelas tidak ikut mengubah nama kamar.

## 2. Proses bisnis

| Unsur | Isi |
| --- | --- |
| Tujuan | Editor master mengubah susunan kamar/bed sesuai aturan denah tanpa pernah menyentuh pasien yang sedang dirawat |
| Pelaku | Admin MasterData (`Bed : Create/Update/Delete`, `Room : Update`, `PatientClass : Update`) |
| Pemicu | Tambah bed/boks, ubah label, hapus bed, ubah kelas atau arah wing kamar, ganti nama kelas |

Langkah dan aturan:

1. **Tambah bed di kamar berkunci.** Server mengunci pembentukan label lantai itu, mencari huruf terbesar yang dipakai awalan nomor kamar yang sama di lantai itu (termasuk bed nonaktif), lalu memberi huruf berikutnya. Label `<nomor kamar>-<huruf>`, kode `BD-RSMMC-#####`, kesiapan `Unverified` (Tidak Tersedia sampai disahkan perawat). Penanda bed mengikuti jenis kelas; boks rawat gabung dikirim `IsForNewborn` = true.
2. **Ubah label bed** lewat `PUT /beds/{id}`: label unik per lantai di antara bed yang belum dihapus; label berpola `329-E` ikut memperbarui huruf bed `E`.
3. **Hapus bed** hanya bila bed tidak pernah dirujuk pemesanan maupun penempatan dalam status apa pun; bed beriwayat dinonaktifkan lewat `PATCH /beds/{id}/status`.
4. **Ubah kelas kamar berkunci** tetap perubahan berbahaya: alasan wajib dan seluruh bed kosong. Penolakan kini menyebut jumlah penghuni dan pesanan. Bila lolos: nama kamar dibentuk ulang, penanda turunan kamar dan seluruh bed ditulis ulang, kesiapan semua bed dibatalkan.
5. **Ubah arah wing** tidak berbahaya; `LocationName` disegarkan.
6. **Ganti nama kelas** memperbarui nama semua kamar berkunci bernomor angka yang memakai kelas itu, dalam transaksi yang sama; snapshot lokasi penempatan lama tidak ditulis ulang.

Contoh berangka:

| Keadaan | Hasil |
| --- | --- |
| Kamar 329 punya 329-A s.d. 329-C, dua penghuni; admin tambah bed | Bed baru `329-D`, kode `BD-RSMMC-00187` (contoh), `Unverified`; penghuni tidak tersentuh; badge `3/2` menjadi `4/2` sesudah disahkan |
| Lantai 3 punya kamar 321 (321-A s.d. 321-C) dan kamar 321 kedua; tambah bed di kamar kedua | `321-D` |
| Kamar kosong 505 (6 bed) Kelas 2 → ICU dengan alasan | Nama "ICU 505" (nomor angka), `RoomType` IntensiveCareRoom, keenam bed `IsIntensiveCareBed` = true dan Tidak Tersedia sampai disahkan |
| Kamar "Kelas 2 329" dihuni 2 pasien, ubah kelas | 409 `BED_HOLD_CONFLICT` "Kelas 2 329 masih dihuni 2 pasien. Perubahan ini baru dapat disimpan setelah kamar kosong." — tanpa perubahan |
| Kelas "Kelas I" diganti "Kelas 1" | "Kelas I 210" → "Kelas 1 210"; kamar tanpa nomor angka ("R. ICU CK") tetap |
| Label "329-C" sudah ada di Lantai 3; bed lain di lantai itu diberi label "329-C" | 409 `BED_LABEL_DUPLICATE` |
| Hapus 329-A yang pernah ditempati | 422 `BED_HAS_USAGE_HISTORY`; nonaktifkan tetap bisa |

Hasil akhir: susunan master mengikuti denah; seluruh bed yang tersentuh perubahan berbahaya kembali harus disahkan perawat (RWI-DEC-282, 285).

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Backend 15.4/15.6, kamus data 22.2.1/22.2.2, API 14.2/14.4, validasi 17; `BedMasterWriteService.cs`, `RoomMasterWriteService.cs`, `PatientClassMasterWriteService.cs`, `BedMutationService.cs`, `InpBedMutationCoordinator.cs`, `BedController.cs`, `BedDtos.cs`, `MstBedConfiguration.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/MasterData/Services/BedMasterWriteService.cs` | Label server + penanda turunan saat create (baris 45, 112); label unik per lantai saat update (baris 154); hapus hanya tanpa riwayat (baris 341); `LoadKeyedRoomAsync`, `EnsureBedLabelUniqueOnFloorAsync` (baris 451); response create membawa `IsForNewborn` |
| `Areas/HealthServices/MasterData/Services/BedMutationService.cs` | `RequireRoomEmptyAsync` dengan jumlah penghuni/pesanan (baris 70, 154); `LockFloorBedLabelsAsync`, `UsedBedLettersAsync`, `FloorBedLabelsAsync` (baris 204–240) |
| `Areas/HealthServices/MasterData/Services/RoomMasterWriteService.cs` | Kamar berkunci: nama dan penanda milik server; ubah kelas menulis ulang penanda kamar dan bed (baris 123, `RewriteBedClassFlagsAsync` baris 397) |
| `Areas/HealthServices/MasterData/Services/PatientClassMasterWriteService.cs` | Ganti nama kelas → nama kamar berkunci (baris 82) |
| `Areas/HealthServices/MasterData/Services/FloorPlanRoomNaming.cs` | Dipakai (dibuat di `BE-RWI-232`) |
| `Areas/HealthServices/MasterData/DTOs/BedDtos.cs` | `CreateBedRequest.BedName` tidak lagi `[Required]` (opsional untuk kamar berkunci, tetap wajib untuk kamar biasa lewat validasi service); `BedCreateResponse.IsForNewborn` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Perilaku: label bed kamar berkunci dibentuk server; kode baru `BED_LABEL_DUPLICATE`, `BED_HAS_USAGE_HISTORY`; pesan `BED_HOLD_CONFLICT` berangka. Method/path tidak berubah |
| Database | Tidak ada schema baru |
| Keamanan/Auth | `NOT APPLICABLE` — hak dan scope unit existing (`GUARD-DNH-06`) |

## 4. Dokumentasi endpoint

#### Health Services / Master Data / Bed

Base URL: `api/v1/health-services/master-data/beds`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Tambah bed atau boks; kamar berkunci berlabel otomatis | `Bed : Create` | `CreateBedRequest` (header `Idempotency-Key`) | `BedCreateResponse` + `OperationMeta` |
| `PUT` | `/{id}` | Ubah label bed; unik per lantai | `Bed : Update` | `UpdateBedRequest` | `BedUpdateResponse` + `OperationMeta` |
| `DELETE` | `/{id}` | Hapus bed yang belum pernah dipakai | `Bed : Delete` | `DeleteBedRequest` (alasan wajib) | `BedDeleteResponse` |

#### Health Services / Master Data / Room

Base URL: `api/v1/health-services/master-data/rooms`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `PUT` | `/{id}` | Ubah kelas (kamar kosong) atau arah wing | `Room : Update` | `UpdateRoomRequest` + `AffectedBedVersions`, `OperationReason` | `RoomUpdateResponse` + `OperationMeta` |

#### Health Services / Master Data / Patient Class

Base URL: `api/v1/health-services/master-data/patient-classes`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `PUT` | `/{id}` | Ganti nama/`ClassLevel`; nama kamar berkunci ikut | `PatientClass : Update` | `UpdatePatientClassRequest` | Response existing + `OperationMeta` |

Kode status: 409 `BED_HOLD_CONFLICT` (kamar berpenghuni/berpesanan) atau `BED_LABEL_DUPLICATE`; 422 `REASON_REQUIRED`, `BED_HAS_USAGE_HISTORY`.

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE preflight | `Mst` ACTIVE; `TOUCHED LEGACY` writer; QBE-VAL-001, TXN-001, DEL-001 | `PASS` | Bagian 3.2 |
| `dotnet build --no-incremental` akhir | 0 error, 286 warning; 0 warning dari kode baru | `PASS` | Keluaran perintah |
| DNH-AT-444 / 455 | Ubah kelas tetap berbahaya (`HasAvailabilityChange` memuat `PatientClassId`), penolakan berangka, kesiapan dibatalkan | `PASS` | `BedMutationService.cs:63-103, 154-163` |
| DNH-AT-454 / 456 / 457 | Label `329-D`, kode alokator bersama, boks `IsForNewborn`; create bukan perubahan berbahaya sehingga kamar berpenghuni tetap bisa ditambah bed | `PASS` | `BedMasterWriteService.cs:45-56, 112` |
| DNH-AT-902 | Huruf lanjut per awalan nomor kamar di lantai; label unik per lantai di bawah kunci advisory lantai | `PASS` | `BedMutationService.cs:204-240`; `BedMasterWriteService.cs:451` |
| DNH-AT-903 | Hapus ditolak bila ada `InpBedPlacement`/`InpBedReservation` apa pun | `PASS` | `BedMasterWriteService.cs:337-343` |
| DNH-AT-909 | Ganti nama kelas memperbarui `RoomName` kamar berkunci di transaksi koordinator kelas; snapshot `InpBedPlacement` tidak disentuh | `PASS` | `PatientClassMasterWriteService.cs:72-90` |

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

Uji manual: `NOT FEASIBLE` — belum ada kamar berkunci di DB Hamzah (impor D-DNH-3 belum dijalankan) dan backend lokal belum dijalankan ulang.

**Tidak dijalankan:** uji runtime editor; bukti nama kelas resmi menunggu D-DNH-1.

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Tambah bed di 329 berpenghuni 2 → `329-D`, kode `BD-RSMMC-#####`, `Unverified`; tambah boks → `IsForNewborn` true | Terpenuhi | `CreateBedAsync` + alokator; root kesiapan baru `Unverified` |
| Ubah kelas kamar kosong 505 → nama dan penanda turunan berubah; keenam bed Tidak Tersedia sampai disahkan | Terpenuhi | `RoomMasterWriteService.cs:121-130`; invalidasi `BedMutationService` |
| Ganti nama "Kelas I" → "Kelas 1" → kamar berkunci "Kelas 1 …"; snapshot lama tetap | Terpenuhi | `PatientClassMasterWriteService.cs:82` |
| Ubah kelas kamar berpenghuni → 409 dengan jumlah penghuni dan pesanan, tanpa perubahan | Terpenuhi | `RequireRoomEmptyAsync` |
| Label ganda di lantai sama → 409 `BED_LABEL_DUPLICATE`; kamar 321 kedua melanjutkan `321-D` | Terpenuhi | `EnsureBedLabelUniqueOnFloorAsync`; `UsedBedLettersAsync` |
| Hapus bed beriwayat → 422 `BED_HAS_USAGE_HISTORY`; nonaktif tetap bisa | Terpenuhi | `DeleteBedAsync`; `PATCH /{id}/status` tidak berubah |
| D-BE | Terpenuhi | Bagian 5 |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kamar Perinatologi yang diubah ke kelas lain: bed-nya tidak lagi otomatis bertanda bayi; admin menandai boks rawat gabung ulang bila perlu |
| Masalah yang diketahui | Perubahan `PatientClassType` sebuah kelas (mis. biasa → ICU) tidak menulis ulang penanda kamar yang memakainya; di luar scope (langkah data D-DNH-1) |
| Risiko tersisa | Penanda turunan hanya ditulis saat kamar kosong (dijaga guard) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat laporan `BE-RWI-240` bagian 7 |
| Langkah berikutnya | `FE-RWI-250` (editor master denah) |
