# Laporan Perubahan Backend — `BE-RWI-036`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-036` |
| Judul | Metadata reservasi aktif tersedia pada papan tempat tidur |
| Slice | Pencarian/pemesanan tempat tidur dan dukungan konfirmasi pasien masuk |
| Roadmap | `docs/module-blueprints/rawat-inap/roadmap/backend-roadmap.md`, task `BE-RWI-036` |
| Trace | `RWI-CAP-006`, `FR-RI-105` s.d. `FR-RI-112`, `RWI-DEC-076`, `RWI-UI-GAP-003` |
| Contract version | API `0.4.0` + `RWI-BED-BOARD-RESERVATION-001 1.0.0`, `APPROVED` Muhammad Hamzah pada 1 September 2026 |
| Dependency | `BE-RWI-010` selesai; task ini membuka dependency data bagi `FE-RWI-020`, `026`, `030`, `032`, dan `036` |
| Klasifikasi | `MEDIUM`, skor 8: repository 0, berkas diperiksa 2, berkas diubah 2, logika bisnis 0, kontrak API 2, database 1, keamanan/auth 1, UI/workflow 0 |
| Task mode | `BACKEND` setelah pencatatan task/kontrak dalam `MODULE BLUEPRINT MODE` |
| Target tulis | Repository `NewQuilvianSystemBackend`; source InPatientManagement, test, dan dokumen tracked modul Rawat Inap |
| Model | GPT-5 Codex |
| Commit backend saat dikerjakan | `133d70c13697bb969f8e150dbef7d5d3cd57bd16` pada branch `MHamzah` |
| Tanggal | 1 September 2026 |
| Status | **Selesai.** Enam acceptance criteria terpenuhi; tidak ada migration atau database execution |

## Backend Governance Preflight

| Pemeriksaan | Hasil |
| --- | --- |
| Bounded context | `HealthServices / InPatientManagement` |
| Prefix ownership | `Inp` terdaftar dan `ACTIVE`; task tidak menambah entity, modul, atau prefix |
| Applicability | `TOUCHED LEGACY`; perubahan dibatasi pada DTO, projection service, kontrak, dan test existing |
| QBE berlaku | `QBE-MOD-001`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-DTO-001`, `QBE-AUD-001` |
| Archetype transaksi | Read-only surface dari aggregate penghunian/pemesanan tempat tidur; tidak ada command atau state transition baru |
| Database authority | `NONE`; query existing boleh diperluas, schema/migration/database execution tidak diotorisasi dan tidak dilakukan |
| Frontend | Repository frontend diperiksa read-only saat preflight dan tidak diubah |

---

## 1. Masalah yang diperbaiki

Sebelumnya papan tempat tidur sudah dapat menyatakan sebuah bed `Reserved`, menampilkan nomor
episode dan nama pasien, tetapi tidak mengembalikan ID episode, ID reservation, atau batas waktu
reservation. Akibatnya layar tidak mempunyai identifier server-authoritative untuk menjalankan
konfirmasi masuk atau membatalkan reservation setelah halaman dimuat ulang.

Contoh: pasien Ibu Rina memegang bed 3A sampai pukul 11:15. Papan lama hanya menyebut nama dan
nomor episode. Papan baru juga membawa `HoldingEpisodeId`, `ReservationId`, dan
`ReservationExpiresAt`, sehingga frontend dapat mengirim identifier yang benar dan menghitung
sisa waktu dari nilai server.

---

## 2. Proses bisnis

1. Petugas berhak membuka papan tempat tidur.
2. Service menjalankan expiry-on-read existing untuk menggugurkan reservation yang sudah lewat.
3. Service membaca penempatan aktif serta reservation aktif yang belum kedaluwarsa.
4. Bila bed hanya mempunyai reservation aktif, response menandainya `IsReserved = true` dan
   mengisi identitas episode/pasien, `ReservationId`, serta `ReservationExpiresAt`.
5. Bila bed sudah ditempati, penempatan aktif menang: identitas pemegang berasal dari penghuni,
   sedangkan kedua field khusus reservation bernilai `null`.
6. Bila bed tidak mempunyai pemegang atau reservation-nya expired/deleted, seluruh metadata
   pemegang/reservation bernilai `null`; counter existing tetap dihitung dengan aturan lama.

Jalur tidak normal yang dijaga adalah data stale ketika penempatan dan reservation aktif sama-sama
terbaca. Response tetap menyatakan `Occupied` dan tidak membocorkan metadata reservation stale.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `AGENTS.md` serta aturan governance backend yang dirujuknya.
- Roadmap backend/frontend, requirement traceability, API contract, dan laporan `BE-RWI-035`.
- `InpatientBedOccupancyDtos.cs`, `InpBedOccupancyService.cs`,
  `InpatientBedOccupancyController.cs`, model reservation, test reservation, dan test world rawat inap.
- Source frontend hanya diperiksa untuk memastikan kebutuhan konsumennya; tidak ada write frontend.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientBedOccupancyDtos.cs` | Menambah `HoldingEpisodeId`, `ReservationId`, dan `ReservationExpiresAt` nullable |
| `Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs` | Memproyeksikan ID episode/reservation dan expiry; hanya reservation aktif yang belum expired; placement menang atas data reservation stale |
| `QuilvianSystemBackend.Tests/InPatientManagement/InpBedBoardReservationMetadataTests.cs` | Empat test response untuk reserved, expired, occupied, dan tanpa pemegang |
| `docs/module-blueprints/rawat-inap/contracts/bed-board-reservation-metadata-contract.md` | Kontrak addendum approved `1.0.0` |
| `docs/module-blueprints/rawat-inap/contracts/api-contract.md` | Menghubungkan endpoint board ke kontrak addendum |
| `docs/module-blueprints/rawat-inap/roadmap/backend-roadmap.md` | Mencatat approval, scope, AC, hasil, dan tautan laporan task |
| `docs/module-blueprints/rawat-inap/roadmap/frontend-roadmap.md` | Menutup gap backend 003 tanpa mengubah status approval roadmap frontend |
| `docs/module-blueprints/rawat-inap/roadmap/requirement-traceability.md` | Menautkan task ke epic/FR/task frontend dan menutup `RWI-UI-GAP-003` pada level kontrak/source backend |
| `docs/module-blueprints/rawat-inap/task/report/backend/BE-RWI-036.md` | Laporan tracked task ini |

### 3.3 Bukti hash source dan kontrak

| Artefak | SHA-256 |
| --- | --- |
| DTO | `545fc0e04ed92c87d04b6d1c9b86eecf2439ce93108df4f487a3b1ffa224caf6` |
| Service | `5c3abc1ea347fb6526c10d27af37ccb4c6cf74b324101c51e9a95586cde0c5a3` |
| Controller yang tetap | `8583d95d84b74470e088b9cded7f79293f4529ed30d72c143331f40dc5b85c05` |
| Test fokus | `9b1f4157f20120bdf1a4f81b63358ee044aac951b8886032b739f177077f53d5` |
| Kontrak addendum | `ea5f3fc69488100841b44d6d838d74c681981088b1a08de61721e523ca7593d8` |
| API contract | `30d14bf1b963cd969d8e31b5bd86f1087bd13077323ee1f6e6d1b3253df455dd` |

### 3.4 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Perubahan aditif pada `BedBoardBedResponse`: tiga field nullable baru. Route, request, status HTTP, field existing, dan counter tidak berubah |
| Database | Tidak ada schema, entity, atau migration. Query existing hanya menambah projection dan penjagaan `ExpiresAt`; tidak ada database execution |
| Keamanan/Auth | Permission tetap `InpatientBedOccupancy : Read`. `PatientName` dan nomor episode sudah tersedia sebelumnya; identifier/timestamp baru hanya berada pada surface berizin yang sama |
| Audit/log | `NOT APPLICABLE`; task read-only dan tidak menambah state transition atau command |

---

## 4. Dokumentasi endpoint

#### Health Services / Inpatient Management / Bed Occupancy

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/inpatient-management/bed-occupancies/bed-board` | Membaca papan bed beserta identitas pemegang dan metadata reservation aktif | `InpatientBedOccupancy : Read` |

Field JSON aditif: `holdingEpisodeId`, `reservationId`, dan `reservationExpiresAt`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Test fokus `InpBedBoardReservationMetadataTests` | 4/4 lulus | `PASS` | Reserved, expired, occupied dengan reservation stale, dan tanpa pemegang |
| Seluruh namespace `InPatientManagement` | 257/257 lulus | `PASS` | `dotnet test ... --filter FullyQualifiedName~QuilvianSystemBackend.Tests.InPatientManagement` |
| Build solution | 0 error, 2 warning existing pada project Billing | `PASS` | `dotnet build QuilvianSystemBackend.sln --no-restore` |
| Project test backend utama | 790/790 lulus | `PASS` | Hasil project `QuilvianSystemBackend.Tests` pada `dotnet test QuilvianSystemBackend.sln --no-build --no-restore` |
| Project Billing pada run solution | 21 lulus, 37 gagal sebelum test berjalan karena `QUILVIAN_BILLING_TEST_DB` tidak tersedia | `EXISTING / ENVIRONMENT ISSUE` | Fixture secara sengaja menolak berjalan tanpa database test khusus yang namanya mengandung `test`; tidak berkaitan dengan file task ini |
| Route dan permission | Tetap `GET bed-board` dan `InpatientBedOccupancy : Read` | `PASS` | Controller tidak berubah; controller contract ikut dalam 257/257 test rawat inap |
| `git diff --check` | Tidak ada whitespace error | `PASS` | Perintah selesai tanpa error; hanya warning line-ending LF/CRLF workspace |

Uji manual: `NOT APPLICABLE` untuk acceptance response karena seluruh cabang dipastikan melalui
integration-style service test InMemory. Uji runtime dengan database tidak dijalankan karena task
tidak diberi wewenang database dan tidak membutuhkannya.

**Tidak dijalankan:** test Billing berbasis PostgreSQL tidak dapat dijalankan tanpa
`QUILVIAN_BILLING_TEST_DB`. Tidak dibuat fallback ke database development dan tidak ada migration
yang diterapkan.

---

## 6. Acceptance criteria dan Definition of Done

### 6.1 Acceptance criteria

| Kriteria persis seperti roadmap | Status | Bukti |
| --- | --- | --- |
| 1. Bed `Reserved` mengembalikan lima metadata yang dikunci kontrak | Terpenuhi | Test `Reserved_MengembalikanIdentitasPemegangDanBatasWaktuReservasiAktif` |
| 2. Reservation expired tidak mengekspos metadata | Terpenuhi | Test `Expired_TidakMengeksposIdentitasAtauMetadataReservasi`; status persisted menjadi `Expired` |
| 3. Bed `Occupied` mempertahankan penghuni dan mengosongkan field reservation | Terpenuhi | Test `Occupied_MemprioritaskanPenghuniDanTidakMengeksposReservasiStale` |
| 4. Bed tanpa pemegang tidak mengekspos metadata | Terpenuhi | Test `TanpaPemegang_TidakMengeksposMetadataDanMempertahankanCounterExisting` |
| 5. Counter dan field existing tidak berubah makna | Terpenuhi | Assert counter pada keempat test baru serta regresi 257/257 rawat inap |
| 6. Route/permission tetap dan tidak ada migration/database execution | Terpenuhi | Controller tidak berubah; build/test hijau; status Git tidak memuat migration |

### 6.2 Definition of Done

| Butir DoD | Status | Catatan |
| --- | --- | --- |
| Enam acceptance criteria lulus | Terpenuhi | 4 test fokus dan 257 test rawat inap lulus |
| Kontrak, source, dan test cocok | Terpenuhi | Field dan invariant sama pada contract/DTO/service/test |
| Build dan test backend hijau | Terpenuhi | Build 0 error; project utama 790/790. Kendala Billing dicatat terpisah sebagai environment issue |
| Laporan tracked menyertakan file/symbol/SHA | Terpenuhi | Laporan ini bagian 3 |
| `RWI-UI-GAP-003` ditutup untuk kontrak/source backend | Terpenuhi | Frontend roadmap dan requirement traceability diperbarui tanpa menyatakan task frontend selesai |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Repository mempunyai warning compile/analyzer existing; tidak ada warning baru dari berkas task |
| Masalah yang diketahui | Full solution test membutuhkan database test Billing khusus; 37 test Billing menolak berjalan karena environment variable tidak tersedia |
| Risiko tersisa | Frontend belum mengonsumsi field baru dan bukti runtime masih menunggu `RWI-UI-GAP-007`; `FE-RWI-036` juga masih menunggu approval roadmap UI |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Delapan artefak source/docs task berubah atau baru sebelum laporan; laporan ini menjadi artefak kesembilan. Tidak ada commit dibuat |
| Langkah berikutnya | `FE-RWI-030` dapat mulai memakai metadata board untuk dialog Konfirmasi Masuk; `FE-RWI-032/036` tidak lagi diblokir gap kontrak 003 |

Tidak ada credential, token, connection string, atau nilai konfigurasi sensitif yang ditulis dalam
laporan ini.
