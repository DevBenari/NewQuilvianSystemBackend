# Laporan Perubahan Backend — `BE-RWI-166`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-166` |
| Judul | Diet Medis atas instruksi dan verifikasi dokter (`K10`) |
| Slice | `MVP-1` / `RWF-W2` — Catatan Keperawatan V1 |
| Roadmap | [`../../../roadmap/backend-roadmap-finishing.md`](../../../roadmap/backend-roadmap-finishing.md) — kartu `BE-RWI-166` |
| Trace | `FR-RWF-055`; `RWI-DEC-178`, `RWI-DEC-188`, `RWI-DEC-191`; `INV-RWF-19`; `AC-RWF-055`; `UAT-RWF-26`; `RWI-AC-303` |
| Contract version | `keperawatan` `0.6.0` **`approved`** (`RWI-DEC-221`): API 8.8, 8.9; integrasi 9.5 (`INT-RWF-13`); backend 12.7 |
| Dependency | — (enum `GziInstructionVerificationStatus` dan `VerifyNutritionInstructionRequest` dipakai ulang dari `BE-RWI-161`) |
| Klasifikasi | `HEAVY` — skor 11: repository 0, berkas diperiksa 1, berkas diubah 2, logika 2, kontrak API 2, database 2, keamanan 1, workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/NutritionManagement/**`, `Areas/HealthServices/InPatientManagement/**`, `Repositories/**`, `Program.cs`, `Migrations/**` |
| Model | Codex (implementasi awal); Claude Opus 5.5 (review, build akhir, migration, database update, laporan) |
| Commit backend saat dikerjakan | `f32b2308` (branch `MHamzah`), working tree |
| Tanggal | 5 Oktober 2026 |
| Status | ✅ Selesai. Kedelapan acceptance criteria terpetakan ke source; build akhir `PASS`; kolom `K10` diterapkan ke database development |

## Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `NutritionManagement` (pemilik diet, Ikbal Yulianto, persetujuan `RWI-DEC-191`); adapter dan controller di `InPatientManagement` (Rawat Inap tidak menyimpan diet, `RWI-DEC-178` butir 5) |
| Prefix registry | `Gz` — `ACTIVE` (entity `GziPatientDiet` legacy dipertahankan namanya); `Inp` — `ACTIVE` |
| Keberlakuan | `NEW CODE` (adapter, dua controller, partial service verifikasi); `TOUCHED LEGACY` (`GziPatientDiet`, `NutritionDietService`, `NutritionDietDtos`, `GziNutritionConfigurations`) |
| QBE yang berlaku | `QBE-ENT-002`, `QBE-CFG-002`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001` |
| Wewenang | Source: ya. Migration: ya (`AGENTS.md` 5 Oktober 2026). Database development: ya, instruksi pengguna 5 Oktober 2026. Deployment: tidak |
| Branch | `MHamzah`; tidak ada operasi Git tulis |

## 1. Masalah yang diperbaiki

Diet pasien rawat inap sebelumnya hanya bisa ditetapkan lewat modul Gizi. Perawat yang menerima instruksi lisan dokter, misalnya lewat telepon pukul 23.00, tidak punya jalan resmi untuk mencatatnya, dan modul Gizi tidak tahu bahwa diet itu masih menunggu tanda verifikasi dokter. Akibatnya instruksi lisan tercatat di luar sistem atau tercatat atas nama perawat, dan tidak ada daftar "perlu diverifikasi" bagi dokter.

## 2. Proses bisnis

**Tujuan.** Perawat dapat menetapkan, mengganti, atau menghentikan diet atas instruksi dokter berpenugasan; diet tetap disimpan modul Gizi dengan status verifikasi, lalu dokter penetap memverifikasinya.

**Pelaku.**

| Pelaku | Peran |
| --- | --- |
| Perawat bangsal | Menginput diet atas instruksi; wajib memilih dokter pemberi instruksi |
| Dokter berpenugasan aktif pada episode | Pemberi instruksi; memverifikasi diet yang diinput perawat |
| Dokter atau ahli gizi yang menulis sendiri | Diet langsung berstatus "tidak perlu verifikasi" |

**Pemicu.** Instruksi diet dari dokter.

**Langkah utama.**

1. Perawat membuka Diet Medis dari layar Rawat Inap dan memilih dokter pemberi instruksi, jenis diet, bentuk makanan, dan kunci idempotensi.
2. Adapter Rawat Inap memeriksa episode masih menerima diet (bukan tertutup, pasien belum keluar ruangan).
3. Adapter memeriksa dokter itu berpenugasan aktif pada episode (`IsDoctorAssignedAsync`).
4. Adapter memetakan dokter ke profil tenaga kerjanya (`MstDoctor.WorkforceProfileId`) sebagai `PrescribedByWorkforceId`.
5. Modul Gizi menyimpan diet dengan status verifikasi `Pending` bila penginput bukan penetap, atau `NotRequired` bila dokter itu sendiri yang menulis. Aturan Gizi yang lain tetap berlaku, termasuk alasan wajib saat mengganti diet aktif (`GIZ010`).
6. Dokter penetap membuka daftar "perlu diverifikasi" dan memverifikasi; waktu dan pengguna verifikasi tersimpan.

**Contoh.** Pukul 23.00 dr. Yoga menginstruksikan diet lunak rendah garam untuk pasien Budi. Ns. Siti menyimpan diet dengan dr. Yoga sebagai dokter pemberi instruksi. Diet tersimpan di Gizi berstatus `Pending` dengan penetap profil dr. Yoga. Esok pagi dr. Yoga memverifikasinya dan status menjadi `Verified`. Bila dr. Andi (bukan penetap) mencoba memverifikasi, sistem menolak `403` `GIZ-VER-001`.

**Aturan bisnis.**

| Aturan | Akibat |
| --- | --- |
| Perawat tanpa dokter pemberi instruksi | `400` "Dokter pemberi instruksi wajib dipilih" |
| Dokter tidak berpenugasan aktif | `403`, tidak ada yang tersimpan |
| Hanya dokter penetap yang boleh memverifikasi | `403` `GIZ-VER-001` |
| Diet sudah diverifikasi atau tidak memerlukan verifikasi | `409` `GIZ-VER-002` |
| Versi diet berubah saat verifikasi | `409` `GIZ012` |
| Kunci idempotensi sama dengan isi berbeda | `409` `GIZ012` |
| Status `Verified` tidak boleh dikirim lewat penetapan | `422` `GIZ-VER-001` |

**Perubahan status verifikasi.**

| Dari status | Tindakan | Ke status | Siapa yang boleh | Syarat |
| --- | --- | --- | --- | --- |
| — | Penetapan oleh penetap sendiri | `NotRequired` | Dokter/ahli gizi | Profil akun = penetap |
| — | Penetapan oleh perawat atas instruksi | `Pending` | Perawat ber-`NutritionPatientDiet : Update` | Dokter berpenugasan |
| `Pending` | Verifikasi | `Verified` | Dokter penetap ber-`NutritionPatientDiet : VerifyInstruction` | Versi cocok |

**Jalur tidak normal.** Pengiriman ulang dengan kunci idempotensi yang sama mengembalikan diet yang sama tanpa membuat diet kedua. Penetapan bersamaan pada kunjungan yang sama diantre kunci baris kunjungan. Penghentian diet atas instruksi juga berstatus `Pending` dengan penetap = dokter pemberi instruksi penghentian (lihat catatan risiko).

**Hasil akhir.** Diet tersimpan di modul Gizi beserta status verifikasinya; Rawat Inap tidak menyimpan salinan.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

API 8.8–8.9, integrasi 9.5, `02-backend-architecture.md` 12.7, `RWI-DEC-188`/`191` pada decision log, `NutritionDietService` (penetapan, penghentian, idempotensi), `NutritionDietController`, `InpatientClinicalContextService`, `NursingEpisodeWriteGuard`, pola verifikasi `BE-RWI-161`, dan registry hak akses bersama `NutritionPatientDiet`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/InPatientManagement/Services/InpDietOrderAdapter.cs` | Baru. Pemeriksaan episode, penugasan dokter, pemetaan profil, penentuan status verifikasi |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientDietController.cs` | Baru. Dua endpoint API 8.8 |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientDietDtos.cs` | Baru. Request penetapan dan penghentian |
| `Areas/HealthServices/NutritionManagement/Services/NutritionDietService.InstructionVerification.cs` | Baru. Resolusi status, daftar kerja, verifikasi |
| `Areas/HealthServices/NutritionManagement/Controllers/NutritionDietVerificationController.cs` | Baru. Daftar kerja dan verifikasi API 8.9 |
| `Areas/HealthServices/NutritionManagement/Models/GziPatientDiet.cs` | Tiga kolom verifikasi |
| `Areas/HealthServices/NutritionManagement/Services/NutritionDietService.cs` | Penetapan dan penghentian dalam transaksi dengan kunci; status verifikasi; pemeriksaan isi ulang idempotensi |
| `Areas/HealthServices/NutritionManagement/DTOs/NutritionDietDtos.cs` | Field opsional status verifikasi pada request; tiga field verifikasi pada response |
| `Repositories/Configurations/HealthServices/NutritionManagement/GziNutritionConfigurations.cs` | Bagian `GziPatientDiet`: konversi enum, bawaan `NotRequired`, index status, FK `Restrict` ke `AspNetUsers` |
| `Program.cs` | Registrasi `InpDietOrderAdapter` |
| Migration gabungan `20261005071042_AddRawatInapKeperawatanFinishing` | Tiga kolom, satu index, satu FK pada `GziPatientDiet` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Dua endpoint baru di Rawat Inap dan dua di Gizi; `POST nutrition-management/diets` menerima field opsional baru. Pemanggil lama tetap `NotRequired`. Delta: daftar kerja mengembalikan `PagedResult<GziPatientDietResponse>` (sudah memuat status verifikasi), bukan DTO terpisah `DietVerificationItem` |
| Database | `GziPatientDiet`: `InstructionVerificationStatus` (`integer`, bawaan 0 = `NotRequired`), `InstructionVerifiedAt`, `InstructionVerifiedByUserId` (FK `Restrict`). Baris lama otomatis `NotRequired`. **Diterapkan** ke database development 5 Oktober 2026 |
| Keamanan/Auth | Resource `NutritionPatientDiet` modul `HEALTH_SERVICE_NUTRITION_MANAGEMENT`; kemampuan baru `VerifyInstruction` lahir dari atribut. Penugasan dokter dan kepemilikan penetap diperiksa dari data, bukan nama peran |

## 4. Dokumentasi endpoint

#### Health Services / Inpatient Management / Inpatient Diet

Base URL: `api/v1/health-services/inpatient-management/episodes/{episodeId}/diets`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/` | Menetapkan atau mengganti diet atas instruksi dokter berpenugasan | `NutritionPatientDiet : Update` | `PrescribeInpatientDietRequest` | `GziPatientDietResponse` |
| `POST` | `/{dietId}/stop` | Menghentikan diet dengan alasan | `NutritionPatientDiet : Update` | `StopInpatientDietRequest` | `GziPatientDietResponse` |

Kode status: `400` dokter belum dipilih atau isian tidak sah; `403` dokter tidak berpenugasan atau perawat tidak berwenang; `404` episode atau diet tidak ditemukan; `409` versi berubah atau kunci idempotensi dipakai permintaan lain; `422` episode tidak lagi menerima diet atau aturan Gizi seperti `GIZ010`.

#### Health Services / Nutrition Management / Patient Diet

Base URL: `api/v1/health-services/nutrition-management/diets`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/instruction-verification-worklist` | Diet "perlu diverifikasi" milik dokter yang login | `NutritionPatientDiet : VerifyInstruction` | Query `GziNutritionPatientQuery` | `PagedResult<GziPatientDietResponse>` |
| `POST` | `/{dietId}/verify-instruction` | Dokter penetap memverifikasi | `NutritionPatientDiet : VerifyInstruction` | `VerifyNutritionInstructionRequest` | `GziPatientDietResponse` |

Kode status: `403` akun bukan dokter aktif atau bukan penetap (`GIZ-VER-001`); `404` diet tidak ditemukan; `409` sudah diverifikasi (`GIZ-VER-002`) atau versi berubah (`GIZ012`).

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -v minimal` — build akhir | `Build succeeded`, `0 Error(s)`, `233 Warning(s)` | `PASS` | Log build akhir; nol warning di berkas baru |
| Build tahap penyelesaian | Error kompilasi Codex di berkas lain sudah diperbaiki sebelum build akhir | `NEW ERROR` (sudah diperbaiki) | Lihat laporan `BE-RWI-165` bagian 5 |
| `dotnet ef migrations add` dan `dotnet ef database update --no-build` | Migration `20261005071042` memuat tiga kolom `GziPatientDiet`; diterapkan `Done.`; nol `Pending` | `PASS` | Keluaran perintah |
| Pemeriksaan statis atribut hak akses | Empat endpoint cocok `NutritionPatientDiet` ↔ `Update`/`VerifyInstruction`; satu `moduleCode` dengan controller Gizi lama | `PASS` | Skrip pemeriksa atribut |
| Verifikasi proses bisnis — perawat tanpa dokter, dokter tak berpenugasan, status `Pending`/`NotRequired` | Sesuai aturan pada bagian 2 | `PASS` | `InpDietOrderAdapter.ContextAsync`, `PrescribeAsync`; `ResolveDietInstructionStatusAsync` |
| Verifikasi proses bisnis — verifikasi oleh bukan penetap, verifikasi kedua | `403` `GIZ-VER-001`; `409` `GIZ-VER-002` | `PASS` | `VerifyDietInstructionAsync` |
| Regresi alur Gizi (`RWI-AC-303`) | Pemanggil `POST diets` lama tanpa field baru tetap `NotRequired`; `GIZ010` tetap di `PrescribeAsync` | `PASS` (penelusuran source) | `NutritionDietService.cs:200`; `ResolveDietInstructionStatusAsync` baris pertama |
| Uji API HTTP dan UAT `UAT-RWF-26` | Tidak dijalankan | `NOT RUN` | Dikecualikan atas instruksi pengguna 5 Oktober 2026 |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

Uji manual: `NOT FEASIBLE` pada sesi ini (aplikasi tidak dijalankan).

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Perawat tanpa dokter → 400 "Dokter pemberi instruksi wajib dipilih" | Terpenuhi | `InpDietOrderAdapter.ContextAsync` |
| 2. Dengan dokter berpenugasan → diet tersimpan di Gizi `Pending`, `PrescribedByWorkforceId` = profil dokter itu | Terpenuhi | `ContextAsync` (pemetaan `WorkforceProfileId`), `PrescribeAsync` |
| 3. Dokter tidak berpenugasan → 403 dan tidak ada yang tersimpan | Terpenuhi | Penolakan terjadi sebelum `NutritionDietService.PrescribeAsync` dipanggil |
| 4. Dokter lain → 403 `GIZ-VER-001`; penetap → `Verified`; verifikasi kedua → 409 `GIZ-VER-002` | Terpenuhi | `VerifyDietInstructionAsync` |
| 5. Dokter atau ahli gizi menulis sendiri → `NotRequired` | Terpenuhi | Adapter `IsSelf`; `ResolveDietInstructionStatusAsync` (tanpa field → `NotRequired`; profil akun = penetap → `NotRequired`) |
| 6. Ganti diet aktif tanpa alasan → `GIZ010` tetap berlaku | Terpenuhi | `NutritionDietService.cs:200` |
| 7. Alur diet Gizi yang sudah ada tidak berubah (`RWI-AC-303`) | Terpenuhi | Field baru opsional; pemanggil lama `NotRequired` |
| 8. `IdempotencyKey` sama tidak membuat diet ganda | Terpenuhi | `DeterministicId` + replay pada `PrescribeAsync` di bawah kunci kunjungan |
| DoD: build tanpa error; laporan; roadmap dan traceability | Terpenuhi | Bagian 5; roadmap diperbarui |
| DoD: verifikasi API/UAT runtime | **Dikecualikan atas instruksi pengguna 5 Oktober 2026** | `NOT RUN` |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `PrescribeAsync` dan `StopAsync` kini membuka transaksi sendiri. Pemanggil saat ini (`NutritionDietController`, `InpDietOrderAdapter`) tidak berada di dalam transaksi, tetapi pemanggil baru tidak boleh memanggilnya dari dalam transaksi lain |
| Masalah yang diketahui | **Perlu keputusan pemilik:** penghentian diet atas instruksi menimpa `PrescribedByWorkforceId` dengan dokter pemberi instruksi penghentian dan mengembalikan status ke `Pending`, karena kamus data `K10` hanya menyediakan tiga kolom verifikasi. Akibatnya riwayat menampilkan dokter penghenti sebagai "penetap" diet yang dihentikan; penetap asal hanya tertelusur dari log. Alternatifnya kolom penghenti terpisah (perubahan kamus data) |
| Risiko tersisa | Belum diuji lewat HTTP dan bersama dokter/perawat; perilaku `409` `GIZ012` baru untuk kunci idempotensi yang dipakai ulang dengan isi berbeda |
| Perubahan sampingan | `NONE` di luar scope task |
| Interupsi | Sesi Codex terhenti karena batas penggunaan penyedia; dilanjutkan Claude |
| Status Git | 151 entri `git status --short` (63 `M`, 3 `D`, 85 `??`), termasuk pekerjaan sesi sebelumnya; berkas task ini pada 3.2; tidak ada stage/commit/push |
| Langkah berikutnya | `FE-RWI-184` (layar Diet Medis) dan `FE-RWI-175` (worklist verifikasi dokter); keputusan pemilik atas semantik penghentian |
