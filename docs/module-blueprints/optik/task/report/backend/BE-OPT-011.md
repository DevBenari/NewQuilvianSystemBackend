# Laporan Perubahan Backend — `BE-OPT-011`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-OPT-011` |
| Judul | Pemeriksaan refraksi |
| Slice | `MVP-1` — `EPIC OPT-02` |
| Roadmap | `roadmap/backend-roadmap.md` (`OPT-RM-BE-001` r2) — Rincian `BE-OPT-011` |
| Trace | `FR-OPT-020`; `OPT-DEC-005`, `012`, `047`, `056`..`059`; `OPT-VAL-001`..`003`, `030`..`034`; `OPT-AC-001`, `002`; `UAT-03` (bagian versi), `UAT-04` |
| Contract version | `OPT-API-v1` grup Opt Refraction Exam, `OPT-STATE-v1` bagian 1, `OPT-VAL-v1` bagian 1 dan 3, `OPT-PERM-v1` — `approved` sisi Optik |
| Keputusan baru (2026-11-03, Yoga Aji Pratama) | `OPT-DEC-056` SIP **dan** STR wajib (menutup `OPT-OQ-017`); `057` isian visus/autoref disahkan; `058` profesi RO/Sp.M dari daftar kode konfigurasi; `059` notasi visus `n/n`, `1/~`, `0` |
| Dependency | `BE-OPT-010` ✅; `BE-OPT-004` ✅ (nomor, pembaca izin) |
| Klasifikasi | `MEDIUM` — satu controller (7 endpoint), dua service baru, DTO |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/OpticalManagement/{Controllers,Services,DTOs}/`, `Program.cs`, `Tests/QuilvianSystemBackend.OpticalTests/` |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `2a5094ca` (branch `yoga`) |
| Tanggal | 2026-11-03 |
| Status | Selesai dan tervalidasi. Belum di-commit |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `OpticalManagement` |
| Registry | `Opt` / `ACTIVE` |
| Keberlakuan | `NEW CODE`; `TOUCHED LEGACY` hanya `Program.cs` (DI) |
| QBE yang berlaku | `QBE-SVC-001` (controller tanpa `DbContext`), `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-PAGE-001`, `QBE-CODE-001` (nomor lewat `NumberSeriesAllocator`), `QBE-AUD-001` |
| Wewenang terpisah | Source ✅ · migration tidak diperlukan (kolom sudah ada di `20261103050001_AddOpticalManagement`) · eksekusi database ✘ · deployment ✘ |

---

## 1. Masalah yang diperbaiki

RO dan Sp.M belum bisa mencatat pemeriksaan refraksi. Resep (`BE-OPT-012`) dan penagihan tarif pemeriksaan
(`BE-OPT-031`) sama-sama bergantung pada pemeriksaan yang tersimpan final.

---

## 2. Proses bisnis

1. **Rina (RO) memulai pemeriksaan** pada kunjungan Optik Ibu Sari. Sistem memeriksa bahwa Rina berprofesi RO, lalu SIP-nya, lalu STR-nya, pada tanggal WIB hari itu. Pemeriksaan `OPT-EXM-RSMMC-000001` terbentuk berstatus Draf dengan nomor SIP Rina tercatat. Bila Mulai tertekan dua kali, yang kembali tetap draf yang sama.
2. **Rina mengisi hasil ukur.** **Contoh:** visus jauh tanpa koreksi OD `6/12`, OS `6/15`; autoref OD −1,50 / −0,50 × 180; PD 62,5. Selama masih draf, isian boleh diganti berkali-kali.
3. **Rina menyimpan final.** Visus jauh mata kanan dan kiri wajib terisi. Tarif pemeriksaan **belum** ditagih di task ini; penagihan dikerjakan `BE-OPT-031`.
4. **Koreksi sesudah final (`UAT-03`).** Rina merevisi OS 6/15 → 6/12 dengan alasan "salah ketik visus OS". Versi 2 bertambah dan versi 1 tetap utuh. Revisi kedua menambah versi 3.
5. **Draf dibatalkan.** **Contoh:** pasien menolak diperiksa. Draf dibatalkan dengan alasan, dan pemeriksaan baru tetap bisa dimulai pada kunjungan itu.

**Jalur tidak normal:**

| Keadaan | Hasil |
| --- | --- |
| Petugas penjualan, atau akun tanpa data pegawai (`OPT-AC-001`) | `403` `OPT-VAL-001` "Anda tidak berwenang membuat resep atau pemeriksaan refraksi."; tidak ada baris pemeriksaan |
| SIP berakhir kemarin (`UAT-04`) | `403` `OPT-VAL-002` "SIP Anda tidak berlaku sejak 02-11-2026." |
| SIP berlaku tetapi STR berakhir 31-10-2026 (`OPT-DEC-056`) | `403` "STR Anda tidak berlaku sejak 31-10-2026." |
| SIP berakhir hari ini, simpan pukul 23.30 WIB (`OPT-AC-002`) | Diterima |
| Data HR tidak terbaca | `503` `OPT-VAL-003` |
| Kunjungan sudah ditutup | `409` `OPT-VAL-030` "Kunjungan sudah ditutup. Daftarkan kunjungan baru." |
| Final tanpa visus jauh kedua mata | `400` `OPT-VAL-031` |
| Visus `6-12` | `400` `OPT-VAL-034` "Visus ditulis seperti 6/12." |
| Final ulang, atau isi draf sesudah final | `409` `OPT-VAL-032` |
| Batalkan pemeriksaan final | `409` `OPT-VAL-033` "Pemeriksaan final tidak dapat dibatalkan; gunakan revisi." |
| Revisi tanpa alasan / batal tanpa alasan | `400` "Tuliskan alasan revisi." / "Tuliskan alasan pembatalan." |
| Revisi atas draf; tindakan apa pun atas pemeriksaan batal | `409` `OPT-INVALID-STATE` |
| Dua petugas menyimpan bersamaan | Yang kedua `409` "Pemeriksaan sudah diubah petugas lain. Muat ulang halaman." |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`OptRefractionExam`/`OptRefractionExamRevision` dan konfigurasinya, `OptPractitionerLicenseReader`, `OptNumberService`, `OptVisitService`; HR: `MstWorkforceProfile`, `MstEmployee` (`ProfessionId`), `MstDoctor` (`SpecializationId`), `MstProfession`, `MstSpecialization`, `WfpCredentialLicense`. Tidak ada kode profesi RO/Sp.M yang di-seed di repository, sehingga penentuannya diputuskan lewat `OPT-DEC-058`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/OpticalManagement/Controllers/OptRefractionExamController.cs` | **Baru.** Tujuh endpoint (bagian 4). Tanpa `<summary>` XML pada action |
| `Areas/HealthServices/OpticalManagement/Services/OptRefractionExamService.cs` | **Baru.** Mulai, isi, final, revisi, batal, daftar, detail; validasi `OPT-VAL-030`..`034`; concurrency `Version` |
| `Areas/HealthServices/OpticalManagement/Services/OptPractitionerGuard.cs` | **Baru.** Profesi RO/Sp.M dari daftar kode konfigurasi → SIP → STR (`OPT-VAL-001`..`003`). Dipakai juga `BE-OPT-012` |
| `Areas/HealthServices/OpticalManagement/DTOs/OptRefractionExamDtos.cs` | **Baru.** Penyaring, permintaan mulai/isi/final/revisi/batal, respons beserta versi |
| `Areas/HealthServices/OpticalManagement/Services/OptVisitService.cs` | **Diperbarui.** `FindOpticalEncounterAsync` dan `IsClosed` untuk membedakan `404` dari `OPT-VAL-030` |
| `Areas/HealthServices/OpticalManagement/Services/OptOperationResult.cs` | **Diperbarui.** Kode `OPT-VAL-001`..`003`, `030`..`034`, `OPT-INVALID-STATE` |
| `Areas/HealthServices/OpticalManagement/Services/OptPractitionerLicenseReader.cs` | **Diperbarui.** Komentar saja: STR kini wajib (`OPT-DEC-056`) |
| `Program.cs` | **Diperbarui.** `AddScoped<OptPractitionerGuard>()`, `AddScoped<OptRefractionExamService>()` |
| `Tests/QuilvianSystemBackend.OpticalTests/Be011RefractionExamTests.cs` | **Baru** (diabaikan Git). 34 uji, termasuk teori |
| `Tests/QuilvianSystemBackend.OpticalTests/OpticalPermissionRegistryTests.cs` | **Diperbarui.** Hitungan endpoint Optik 8 → 15 |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Tujuh endpoint grup Opt Refraction Exam tersedia (rincian delta di bawah) |
| Database | Tanpa migration. Menulis `OptRefractionExam` dan `OptRefractionExamRevision`. `BillingFactEmittedAt` tidak disentuh |
| Keamanan/Auth | `[AccessPermission("OptRefractionExam", Read/Create/Update/Finalize/Revise/Cancel)]`, lolos `PermissionRegistryValidator`. Service menambah syarat profesi serta SIP dan STR. Kalimat log hanya memuat nomor pemeriksaan, nomor versi, dan nama pelaku, tanpa nilai ukur |
| Konfigurasi | **Baru dan wajib diisi:** `OpticalManagement:Practitioner:RefractionistProfessionCodes` dan `OpticalManagement:Practitioner:OphthalmologistSpecializationCodes`. Bentuknya larik JSON atau teks dipisah koma. Selama kosong, semua pemeriksa ditolak `OPT-VAL-001` |

**Delta terhadap kontrak:**

1. **`ExpectedVersion` dipakai di semua tindakan yang mengubah data**, termasuk isi hasil ukur dan batal. Kontrak hanya menuliskannya di final dan revisi; ini mengikuti konvensi umum `api-contract.md`.
2. **`Idempotency-Key` tidak dipakai.** Tabel pemeriksaan tidak punya kolom kunci idempoten, dan migration baru tidak diizinkan. Simpan ganda dicegah lewat keadaan data: Mulai mengembalikan draf yang sudah ada, final ulang ditolak `OPT-VAL-032`, dan tindakan lain memakai `ExpectedVersion`.
3. **"Simpan final menerbitkan tarif pemeriksaan"** pada kontrak belum dikerjakan; itu bagian `BE-OPT-031`.
4. **Aturan isian yang tidak ada di `OPT-VAL-v1` bagian 3** diambil dari kamus data:
   - CYL wajib notasi minus;
   - AXIS 0–180 dan hanya diisi bila ada silinder;
   - ADD positif;
   - batas presisi kolom.

   Kodenya `OPT-INVALID-REQUEST`, karena `OPT-VAL-011`..`013` milik resep.
5. **`OPT-VAL-034` diperluas** oleh `OPT-DEC-059` (`1/~`, `0`, angka berdesimal). Penyegarannya dicatat di bagian 11 decision log.
6. **Daftar** memuat versi berlaku (`Current`) tanpa riwayat; riwayat lengkap ada di detail.

---

## 4. Dokumentasi endpoint

#### Health Services / Optical Management / Opt Refraction Exam

Base URL: `api/v1/health-services/optical-management/refraction-exams`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Daftar pemeriksaan: `EncounterId`, `PatientId`, `Status`, halaman | `OptRefractionExam : Read` |
| `GET` | `/{id}` | Detail beserta seluruh versi | `OptRefractionExam : Read` |
| `POST` | `/` | Mulai pemeriksaan (`EncounterId`) | `OptRefractionExam : Create` |
| `PUT` | `/{id}/measurements` | Isi/ganti hasil ukur selama draf (`ExpectedVersion` + set lengkap) | `OptRefractionExam : Update` |
| `POST` | `/{id}/finalize` | Simpan final (`ExpectedVersion`) | `OptRefractionExam : Finalize` |
| `POST` | `/{id}/revisions` | Revisi sesudah final (`ExpectedVersion`, set lengkap, `RevisionReason`) | `OptRefractionExam : Revise` |
| `POST` | `/{id}/cancel` | Batalkan draf (`ExpectedVersion`, `Reason`) | `OptRefractionExam : Cancel` |

Semua tindakan selain `GET` juga mensyaratkan profesi RO/Sp.M serta SIP dan STR berlaku. Kode status: `200` berhasil; `400` isian tidak sah; `403` bukan RO/Sp.M atau izin tidak berlaku; `404` tidak ditemukan; `409` kunjungan tertutup, status tidak mengizinkan, atau versi tertinggal; `503` data HR tidak terbaca.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet test Tests/QuilvianSystemBackend.OpticalTests -p:RunAnalyzers=False` | 76 lulus, 0 gagal (34 baru) | `PASS` | Keluaran `dotnet test` |
| `UAT-03` bagian versi: mulai → isi → final → revisi dua kali | Tiga versi; versi 1 utuh; alasan tersimpan; `BillingFactEmittedAt` kosong | `PASS` | Uji |
| Mulai dua kali | Satu pemeriksaan | `PASS` | Uji |
| Sp.M dikenali dari kode spesialisasi dokter | Diterima | `PASS` | Uji |
| `OPT-AC-001`, akun tanpa pegawai, daftar kode kosong | `403 OPT-VAL-001`; nol baris | `PASS` | Uji |
| `UAT-04`, `OPT-AC-002` (23.30 WIB), STR kedaluwarsa | Kalimat dan kode kontrak | `PASS` | Uji dengan jam tetap 2026-11-03 16.30 UTC |
| Izin dicek ulang saat final oleh petugas lain | `403`; status tetap Draf | `PASS` | Uji |
| `OPT-VAL-030` dan kunjungan non-Optik | `409` / `404` | `PASS` | Uji |
| `OPT-VAL-034` (10 notasi), aturan CYL/AXIS (4 kasus), `OPT-VAL-031` | Lulus | `PASS` | Uji teori |
| `OPT-VAL-032`, `033`, revisi tanpa alasan, revisi atas draf, alur batal | Lulus | `PASS` | Uji |
| `ExpectedVersion` tertinggal | `409`; data tidak berubah | `PASS` | Uji |
| Daftar per kunjungan memuat versi berlaku | Lulus | `PASS` | Uji |
| Controller memetakan `200/400/403/404/409` | Lulus | `PASS` | Uji controller |
| `PermissionRegistryValidator` atas 15 endpoint Optik | Nol temuan | `PASS` | Uji registri |
| QBE checker atas 7 berkas + kontrol multi-baris | Kontrol terkena 5 temuan; 7 berkas bersih | `PASS` | Keluaran checker; probe dihapus |

**Tidak dijalankan:** panggilan HTTP ke backend yang menyala. Tabel Optik belum ada di devYoga karena migration belum diterapkan, dan wewenang database tidak diberikan.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) `OPT-AC-001`, `002` | Terpenuhi | Uji |
| (2) Revisi sesudah final menambah revisi dengan alasan wajib | Terpenuhi | Uji `UAT-03` dan uji alasan kosong |
| (3) Batal final ditolak `409` | Terpenuhi | Uji `OPT-VAL-033` |
| (4) `UAT-04` | Terpenuhi di harness | Uji |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Isi dua kunci konfigurasi sebelum dipakai.** Contoh: `"OpticalManagement": { "Practitioner": { "RefractionistProfessionCodes": ["RO"], "OphthalmologistSpecializationCodes": ["SP-M"] } }`. Kodenya harus sama dengan `ProfessionCode`/`SpecializationCode` di data HR. Selama kosong, semua ditolak |
| Masalah yang diketahui | Profesi pada pegawai (`MstEmployee.ProfessionId`) bersifat opsional di HR. RO tanpa profesi tercatat akan ditolak `OPT-VAL-001` |
| Risiko tersisa | Kode profesi RO/Sp.M di data HR server belum diperiksa (tidak ada eksekusi database). Uji memakai SQLite. Fakta tagihan dan jasa medis menunggu `BE-OPT-031`; dokumen Rekam Medis per versi menunggu `BE-OPT-013` |
| Perubahan sampingan | `NONE`. Probe QBE sementara dibuat dan dihapus |
| Interupsi | `NONE` |
| Status Git | `M Program.cs`; `?? Areas/HealthServices/OpticalManagement/` bertambah `OptRefractionExamController.cs`, `OptRefractionExamService.cs`, `OptPractitionerGuard.cs`, `OptRefractionExamDtos.cs` |
| Langkah berikutnya | `BE-OPT-012` (resep: internal, salin resep luar + transposisi, revisi berversi, lewat rentang hanya Sp.M). Sejajar dengan itu `BE-OPT-003` dan `BE-OPT-030` |
