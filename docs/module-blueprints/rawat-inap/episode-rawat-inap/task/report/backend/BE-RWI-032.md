# Laporan Perubahan Backend — `BE-RWI-032`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-032` |
| Judul | Empat modul tetangga terbukti tidak rusak |
| Slice | Regresi jalur `MstBed` yang disentuh perubahan perilaku |
| Roadmap | `docs/module-blueprints/rawat-inap/roadmap/backend-roadmap.md`, task `BE-RWI-032` |
| Trace | `RWI-DEC-051`, `RWI-DEC-062`; `RWI-RISK-002`; `NFR-008`; `RWI-AC-114`; `testing/acceptance-test-matrix.md` bagian 12 |
| Contract version | API `0.4.0`; task ini tidak menyentuh kontrak |
| Dependency | `BE-RWI-006` — **dikerjakan bersamanya**, bukan sesudahnya |
| Klasifikasi | `LIGHT`, skor 4: repository 0, berkas diperiksa 2, berkas diubah 1, logika bisnis 0, kontrak API 0, database 0, keamanan/auth 0, UI/workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | Repository `NewQuilvianSystemBackend`; **hanya** berkas test dan dokumen tracked |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `514b1d8232720eb450bc40f6deea6c6661160c8d` pada branch `MHamzah` |
| Tanggal | 1 September 2026 |
| Status | **Selesai.** Keempat acceptance criteria terbukti. Tidak satu baris source modul tetangga disentuh task ini |

## Backend Governance Preflight

| Pemeriksaan | Hasil |
| --- | --- |
| Bounded context | `HealthServices / InPatientManagement` untuk lokasi test; sasaran ujinya `MasterData` |
| Prefix ownership | Tidak ada entity, modul, maupun prefix baru |
| Applicability | `NEW CODE` terbatas pada berkas test |
| QBE berlaku | `QBE-MOD-001` |
| Database authority | `NONE` |
| Source modul tetangga | **Tidak disentuh.** Task ini hanya menambah test |

---

## 1. Masalah yang diperbaiki

`RWI-RISK-002` mencatat kenyataan yang tidak nyaman: sampai hari ini **tidak ada satu pun test**
yang menjaga jalur poliklinik, IGD, dan farmasi. Sementara itu `BE-RWI-006` mengubah perilaku
`BedController`, yang dimiliki modul `MasterData` dan dibaca beberapa modul lain.

Tanpa test regresi, keyakinan bahwa "jalur lama tidak rusak" hanya berupa keyakinan. Yang lebih
berbahaya: kesalahan yang paling mungkin terjadi bukan perubahan yang gagal, melainkan
perubahan yang **terlalu lebar** — penjaga baru yang tanpa sengaja ikut menolak nilai yang
seharusnya masih diizinkan. Kesalahan seperti itu tidak menghasilkan galat apa pun di sisi
backend; yang terjadi adalah admin tiba-tiba tidak dapat menutup tempat tidur rusak, dan tidak
ada yang tahu penyebabnya sampai ada yang mengeluh.

**Contoh konkret.** Seandainya penjaga `BE-RWI-006` ditulis sebagai "tolak semua perubahan
status bila tempat tidur punya baris penempatan", maka tempat tidur yang pasiennya **sudah
pulang tiga minggu lalu** ikut terkunci selamanya — karena baris penempatannya tetap ada,
hanya sudah diberi waktu berakhir. Test `PenempatanYangSudahBerakhir_TidakMenahanPerubahanStatus`
ada khusus untuk menangkap kesalahan itu.

---

## 2. Cakupan yang sengaja dibatasi

Roadmap menulis peringatan yang diikuti apa adanya: *"Task ini menutup lubang itu **hanya**
untuk jalur `MstBed` yang benar-benar disentuh — bukan untuk seluruh modul tetangga. Jangan
melebarkan scope-nya diam-diam."*

| Yang diuji | Yang **tidak** diuji |
| --- | --- |
| Aksi `PATCH /beds/{id}/availability` untuk seluruh nilai `BedStatus` | Endpoint `BedController` lain di luar `/availability` |
| Jalur **baca** `MstBed` — mencari tempat tidur satu kamar beserta penandanya | Alur bisnis poliklinik, IGD, dan farmasi secara utuh |
| Jalur **tulis** `MstBed` di luar `/availability` | Modul `ClinicalManagement` dan `PharmacyManagement` |
| Bentuk balasan `BedUpdateResponse` | Layar frontend mana pun |

`RWI-RISK-002` karena itu **tetap terbuka** untuk cakupan di luar tabel kiri. Task ini
menurunkannya, tidak menutupnya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Alasan diperiksa |
| --- | --- |
| `Areas/HealthServices/MasterData/Controllers/BedController.cs` | Menetapkan jalur mana yang benar-benar disentuh `BE-RWI-006` |
| `Areas/HealthServices/MasterData/DTOs/BedDtos.cs` | Bentuk `UpdateBedAvailabilityRequest` dan `BedUpdateResponse` yang harus tetap sama |
| `Areas/HealthServices/MasterData/Models/MstBed.cs` | Kolom yang dibaca modul lain |
| `QuilvianSystemBackend.Tests/HealthServices/RegistrationManagement/PatientEncounterTestWorld.cs` | Pola pembangunan controller pada test |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `QuilvianSystemBackend.Tests/InPatientManagement/BedAvailabilityRegressionTests.cs` | **Berkas baru.** Sepuluh test: empat membuktikan aturan `BE-RWI-006`, enam menjaga jalur lama |

**Tidak ada satu baris source pun** yang diubah task ini.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` |
| Database | `NOT APPLICABLE` |
| Keamanan/Auth | `NOT APPLICABLE` |

---

## 4. Verifikasi

### 4.1 Keluaran sebelum dan sesudah `BE-RWI-006`

Roadmap mewajibkan keluaran sebelum dan sesudah dilampirkan apa adanya.

| Tahap | Suite `InPatientManagement` | Project test utama | Build solution |
| --- | --- | --- | --- |
| **Sebelum** — sebelum `BE-RWI-034` dan `BE-RWI-006` | `Failed: 0, Passed: 257, Skipped: 0, Total: 257` | `Failed: 0, Passed: 844, Skipped: 0, Total: 844` | `Build succeeded. 200 Warning(s), 0 Error(s)` |
| **Setelah `BE-RWI-034`** | `Failed: 0, Passed: 280, Skipped: 0, Total: 280` | `Failed: 0, Passed: 867, Skipped: 0, Total: 867` | — |
| **Sesudah** — setelah `BE-RWI-006` dan `BE-RWI-032` | `Failed: 0, Passed: 292, Skipped: 0, Total: 292` | `Failed: 0, Passed: 879, Skipped: 0, Total: 879` | `Build succeeded. 206 Warning(s), 0 Error(s)` |

Tidak ada satu pun test yang berubah dari lulus menjadi gagal. Pertambahan 257 → 292 seluruhnya
berasal dari test baru kedua task, bukan dari test lama yang dipecah.

### 4.2 Acceptance criteria

| Kriteria | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| 1. Layar master tempat tidur tetap berfungsi untuk `Cleaning`, `Maintenance`, `Blocked`, `Inactive` | Lulus, 5 kasus termasuk `Available` | `PASS` | `LayarMasterTempatTidur_TetapBerfungsiUntukNilaiYangMasihDiizinkan` |
| 2. Jalur pemakaian `MstBed` oleh modul lain yang tidak menyetel status tetap berjalan | Lulus | `PASS` | `JalurBacaMstBedOlehModulLain_TetapBerjalan`, `JalurTulisLainPadaMstBed_TidakIkutTerkunci` |
| 3. Test gagal bila perubahan `BedController` melebihi kesepakatan | Lulus | `PASS` | Kriteria 1 dan `PenempatanYangSudahBerakhir_TidakMenahanPerubahanStatus` — keduanya gagal bila penjaga ditulis terlalu lebar |
| 4. Test dijalankan pada rangkaian yang sama dengan test modul Rawat Inap | Lulus | `PASS` | Berkas berada di `QuilvianSystemBackend.Tests/InPatientManagement/` dan ikut pada filter `FullyQualifiedName~InPatientManagement` |

Uji manual: `NOT APPLICABLE` — task ini seluruhnya berupa test otomatis.

**Tidak dijalankan:** project `QuilvianSystemBackend.BillingTests` — menuntut
`QUILVIAN_BILLING_TEST_DB` dan di luar scope.

---

## 5. Risiko yang tersisa

| Risiko | Keadaan |
| --- | --- |
| `RWI-RISK-002` | **Turun, belum tertutup.** Jalur poliklinik, IGD, dan farmasi masih tanpa test di luar jalur `MstBed` yang diuji di sini |
| Pemesanan aktif tidak diuji sebagai penahan | Sejalan dengan `BE-RWI-006`: penjaganya memang hanya memeriksa penempatan. Bila butir keputusan pada laporan `BE-RWI-006` bagian 7 diputuskan diperluas, test ini wajib ikut bertambah |

---

## 6. Task berikutnya

`BE-RWI-033` — penutup bukti penerimaan dan traceability modul.
