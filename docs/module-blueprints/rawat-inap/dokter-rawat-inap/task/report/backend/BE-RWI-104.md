# Laporan Perubahan Backend — `BE-RWI-104`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-104` |
| Judul | Kolom instruksi pada pesanan Lab dan Radiologi (migration R8) |
| Slice | Gelombang 3 — `DOK-V2-3` |
| Roadmap | [`roadmap/backend-roadmap-v2.md`](../../../roadmap/backend-roadmap-v2.md) — kartu `BE-RWI-104` |
| Trace | `FR-DOK-106`; **`RWI-DEC-153`**; `INT-DOK-19`; `VAL-DOK-46`, `47`, `50`, `50a`; api-contract 0.6.0 bagian 12.12; kamus data 0.5 bagian 13.11 |
| Contract version | `0.6.0` |
| Dependency | `BE-RWI-097` ✅ |
| Klasifikasi | `HEAVY` — migration pada dua tabel milik modul lain, perubahan jalur buat pesanan yang dipakai poliklinik dan IGD |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/LaboratoryManagement`, `Areas/HealthServices/RadiologyManagement`, `Repositories`, `Migrations`, dokumentasi blueprint |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `23a31501` (branch `MHamzah`) |
| Tanggal | 17 September 2026 |
| Status | ✅ Selesai berdasarkan validasi statis source dan skema; `dotnet build`, eksekusi migration, dan **regresi alur Lab/Rad NOT RUN** atas instruksi pemilik |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area/module | `HealthServices / LaboratoryManagement` dan `HealthServices / RadiologyManagement` |
| Registry/prefix | `Lab` dan `Rad` `ACTIVE / LEGACY`; tabel `LabOrder` dan `RadOrder` sudah ada, nol tabel baru |
| Keberlakuan | `TOUCHED LEGACY` — hanya blok instruksi yang ditambahkan; pola lama tiap service dipertahankan (`LabOrderService` melempar exception, `RadOrderService` mengembalikan `RadOperationResult`) |
| QBE relevan | `QBE-CFG-002`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-ENUM-001`, `QBE-PAGE-001` |
| Persetujuan pemilik tabel | **Yoga Aji**, pemilik `LaboratoryManagement` dan `RadiologyManagement`, lewat **`RWI-DEC-153`** (16 September 2026) |
| Hak akses baru | `LabOrder : Verify`, `RadOrder : Verify` (disetujui `RWI-DEC-153`) |
| Database | Migration `20260916007000_AddLabRadInstructionColumns` dibuat, **tidak dijalankan** |

## 1. Masalah yang diperbaiki

Di rawat inap, pesanan laboratorium dan radiologi sering dimasukkan perawat atas instruksi dokter.
Tabel `LabOrder` dan `RadOrder` tidak menyimpan siapa dokter yang memberi instruksi dan apakah dokter
itu sudah mengonfirmasinya, sehingga pesanan perawat tidak dapat ditelusuri kembali ke dokter.

## 2. Proses bisnis

1. Ns. Wati memesan darah lengkap untuk Budi (rawat inap) atas instruksi lisan dr. Rina →
   `POST /lab-orders` dengan `InpEpisodeId` dan `InstructingDoctorId = dr. Rina`.
2. Sistem memeriksa dr. Rina **sedang bertugas** atas episode Budi lewat
   `InpatientClinicalContextService.IsDoctorAssignedAsync` (membaca penugasan, tidak menyalinnya). Pesanan
   tersimpan dengan `InstructionVerificationStatus = Pending`.
3. dr. Rina membuka daftar tunggu `GET /lab-orders/instruction-verification-worklist` → pesanan Budi
   tampil beserta nama perawat penginput. Ia memverifikasi `PUT /lab-orders/{id}/verify-instruction` →
   `Verified`, `InstructionVerifiedAt`, `InstructionVerifiedByUserId` terisi. Penginput, pemeriksaan, dan
   status pesanan **tidak berubah**.
4. Radiologi berjalan sama lewat `/rad-orders`.
5. dr. Rina sendiri yang memesan → akun tertaut dokter, `NotRequired`; isian pemberi instruksi diabaikan.
6. **Alur poliklinik dan IGD:** pesanan tanpa `InpEpisodeId` melewati blok instruksi seluruhnya →
   `NotRequired`, perilaku lama tidak berubah.
7. **Jalur tidak normal:**
   - Perawat tidak memilih dokter → `400` "Pilih dokter yang memberi instruksi."
   - Dokter terpilih tidak bertugas atas pasien → `403` "Dokter yang dipilih tidak sedang bertugas atas pasien ini."
   - dr. Ahmad (bukan pemberi instruksi) memverifikasi → `403` "Hanya dokter pemberi instruksi yang dapat memverifikasi pesanan ini."
   - Pesanan sudah `Verified` atau `NotRequired` → `409` "Pesanan ini sudah diverifikasi atau tidak memerlukan verifikasi."
   - Akun tanpa tautan dokter membuka daftar tunggu → `403`, bukan daftar kosong.
   - Dua verifikasi bersamaan pada Lab → `409` lewat token konkurensi `Version`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kamus data 13.11 dan 13.13; api-contract 12.12; `LabOrder.cs`, `RadOrder.cs`, kedua configuration,
`LabOrderService.cs`, `RadOrderService.cs`, `RadOperationResult.cs`, kedua controller, DTO kedua modul,
`InpatientClinicalContextService.cs`, `PatientProcedureOrderService.cs` (pola `BE-RWI-097/098`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/LaboratoryManagement/Enums/LabOrderInstructionVerificationStatus.cs` | Baru — `NotRequired = 0`, `Pending = 1`, `Verified = 2` |
| `Areas/HealthServices/RadiologyManagement/Enums/RadOrderInstructionVerificationStatus.cs` | Baru — nilai sama, enum milik modul sendiri |
| `Areas/HealthServices/LaboratoryManagement/Models/LabOrder.cs`, `Areas/HealthServices/RadiologyManagement/Models/RadOrder.cs` | Empat kolom `InstructingDoctorId`, `InstructionVerificationStatus`, `InstructionVerifiedAt`, `InstructionVerifiedByUserId` + navigasi |
| `Repositories/Configurations/HealthServices/LabOrderConfiguration.cs`, `Repositories/Configurations/HealthServices/RadiologyManagement/RadOrderConfiguration.cs` | Bawaan `NotRequired`, konversi enum, FK `Restrict` ke `MstDoctor` dan `AspNetUsers`, index `(InstructingDoctorId, InstructionVerificationStatus)` dan `InstructionVerifiedByUserId` |
| `Migrations/20260916007000_AddLabRadInstructionColumns.cs` | Baru — R8; `Down` ditolak bila ada pesanan yang kolom instruksinya tidak bernilai bawaan |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Delapan properti, empat index, empat relasi |
| `Areas/HealthServices/LaboratoryManagement/DTOs/LabOrderDtos.cs`, `Areas/HealthServices/RadiologyManagement/DTOs/RadiologyDtos.cs` | `InstructingDoctorId` opsional pada request buat; empat field instruksi + nama dokter pada detail; DTO butir daftar tunggu |
| `Areas/HealthServices/LaboratoryManagement/Services/LabOrderService.cs` | `ResolveInstructionAsync` pada buat; `VerifyInstructionAsync`; `GetInstructionVerificationWorklistAsync` |
| `Areas/HealthServices/LaboratoryManagement/Controllers/LabOrderController.cs` | Endpoint verifikasi dan daftar tunggu; `Create` menangkap `UnauthorizedAccessException` → `403` |
| `Areas/HealthServices/RadiologyManagement/Services/RadOperationResult.cs` | Lima kode galat `RAD_INSTRUCTING_DOCTOR_REQUIRED`, `RAD_INSTRUCTING_DOCTOR_NOT_ASSIGNED`, `RAD_NOT_INSTRUCTING_DOCTOR`, `RAD_INSTRUCTION_NOT_PENDING`, `RAD_DOCTOR_NOT_IDENTIFIED` |
| `Areas/HealthServices/RadiologyManagement/Services/RadOrderService.cs` | Blok instruksi pada buat; verifikasi; daftar tunggu |
| `Areas/HealthServices/RadiologyManagement/Controllers/RadOrderController.cs` | Endpoint verifikasi dan daftar tunggu; pemeta hasil menjawab `Forbidden` → `403` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Isian opsional `InstructingDoctorId` pada `POST /lab-orders` dan `POST /rad-orders`; field tambahan pada detail; masing-masing dua endpoint baru. Pemanggil lama tanpa `InpEpisodeId` tidak merasakan perubahan |
| Database | R8: empat kolom nullable/berbawaan pada dua tabel; migration dibuat, **tidak dijalankan** |
| Keamanan/Auth | Resource action baru `LabOrder : Verify` dan `RadOrder : Verify` (`AccessType = Update`, `SortOrder = 6`); pasangan `AccessAction`/`AccessPermission` cocok huruf demi huruf. Kewenangan verifikasi dari data (dokter pemberi instruksi = dokter tertaut akun), bukan dari nama peran. Pemberi instruksi yang penugasannya sudah berakhir tetap boleh memverifikasi |

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Order

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | (diperluas) pesanan rawat inap oleh perawat wajib membawa `InstructingDoctorId` | `LabOrder : Create` |
| `PUT` | `/{id}/verify-instruction` | Dokter pemberi instruksi memverifikasi pesanan | `LabOrder : Verify` |
| `GET` | `/instruction-verification-worklist` | Pesanan `Pending` milik dokter login, `pageNumber`/`pageSize` | `LabOrder : Read` |

#### Health Services / Radiology Management / Rad Order

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | (diperluas) sama seperti laboratorium | `RadOrder : Create` |
| `PUT` | `/{id}/verify-instruction` | Dokter pemberi instruksi memverifikasi pesanan | `RadOrder : Verify` |
| `GET` | `/instruction-verification-worklist` | Pesanan `Pending` milik dokter login | `RadOrder : Read` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| Kesesuaian delapan kolom dengan kamus data 13.11 dan DDL 13.13 | Nama, tipe, nullability, bawaan, index sama | `PASS` | Model, configuration, migration, snapshot |
| Pesanan tanpa `InpEpisodeId` tidak menyentuh blok instruksi (review jalur buat Lab dan Rad) | Blok dilewati; status `NotRequired`; tidak ada pembacaan klaim tambahan | `PASS` | `ResolveInstructionAsync` baris pertama; blok `if (request.InpEpisodeId.HasValue …)` di `RadOrderService` |
| Cabang `Forbidden` baru pada pemeta hasil Radiologi tidak mengubah endpoint lama | Sebelum task ini tidak ada jalur `RadOrderService` yang menghasilkan `Forbidden` | `PASS` | Pencarian source |
| Snapshot hanya bertambah | Nol baris terhapus | `PASS` | `git diff --diff-algorithm=histogram` |
| Pemeriksaan statis ambiguitas tipe dan `using` hilang | Nol temuan pada kode task; alias `using` dipakai untuk `MstDoctor` dan `InpatientClinicalContextService` | `PASS` | Skrip analisis statis sesi ini |
| `dotnet build` | Tidak dijalankan | `NOT RUN` | Instruksi eksplisit pemilik |
| Verifikasi skema pada Postgres sekali pakai | Tidak dijalankan | `NOT RUN` | Migration belum dieksekusi |
| **Regresi alur pemesanan Lab/Rad yang sudah ada** (poliklinik, IGD, rawat inap oleh dokter) | Tidak dijalankan | `NOT RUN` | Menunggu build dan migration — **wajib dijalankan sungguhan sebelum rilis** (kartu roadmap, `RWI-DEC-153`) |

**AUTOMATED TEST: NOT APPLICABLE** — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Empat kolom instruksi ada pada `LabOrder` dan `RadOrder` | Terpenuhi (statis) | Model, configuration, migration R8, snapshot |
| 2. Pesanan Lab/Rad dari rawat inap membawa pemberi instruksi dan status verifikasi — `FR-DOK-106` | Terpenuhi (statis) | Blok instruksi pada kedua jalur buat; verifikasi dan daftar tunggu |
| 3. Alur pemesanan Lab/Rad dari modul lain tidak berubah perilakunya | Terpenuhi (statis) — **regresi runtime `NOT RUN`** | Blok hanya menyala bila `InpEpisodeId` terisi; seluruh isian baru opsional |
| 4. Migration mundur menghapus kolom selama bernilai bawaan | Terpenuhi (statis) | `Down` dengan `RAISE EXCEPTION` bila ada `InstructingDoctorId` terisi atau status ≠ `NotRequired` |
| Laporan tracked merujuk `RWI-DEC-153` | Terpenuhi | Metadata, preflight, bagian 3.3 |
| Regresi Lab/Rad hijau | **Belum** | `NOT RUN` atas instruksi pemilik; ditandai selesai atas instruksi pemilik |
| Roadmap dan traceability diperbarui | Terpenuhi | Kartu `BE-RWI-104`, baris `FR-DOK-106` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Task ditandai ✅ atas instruksi pemilik walaupun Definition of Done kartu mensyaratkan regresi Lab/Rad hijau. Persetujuan `RWI-DEC-153` membuka izin menambah kolom, **bukan** izin mengubah alur Lab/Rad — regresi tetap wajib sebelum rilis |
| Masalah yang diketahui | Pesanan rawat inap oleh akun yang tertaut dokter tidak menyimpan dokter itu sebagai pemberi instruksi (`InstructingDoctorId` kosong, `NotRequired`), sama dengan kamus data 13.11. Verb verifikasi memakai `PUT` mengikuti konvensi transisi controller Lab/Rad yang sudah ada |
| Risiko tersisa | `LabOrderService` dan `RadOrderService` kini bergantung `InpatientClinicalContextService`; bila ada tempat yang membuat service ini secara manual, konstruktornya berubah — pencarian `new LabOrderService(` dan `new RadOrderService(` nol hasil |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Berkas baru `??` (dua enum, migration); berkas lain `M` |
| Langkah berikutnya | `dotnet build`; jalankan migration R8 di Postgres sekali pakai; regresi pemesanan Lab/Rad dari poliklinik, IGD, dan rawat inap; uji verifikasi oleh dokter pemberi instruksi dan dokter lain |
