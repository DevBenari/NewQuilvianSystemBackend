# Laporan Perubahan Backend — `BE-HRD-443`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-443` |
| Judul | Employee 360: bagian disiplin dari sanksi resmi dan catatan lama, hanya untuk pemegang `ViewSanctions` |
| Slice | `S-PKB` — Paket B Navigasi SDM / `MVP-72` · `EPIC HRD-128` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 30, kartu `BE-HRD-443` |
| Trace | `FR-PKB-14`, `FR-PKB-15`; `HRD-DEC-230`, `HRD-DEC-238` butir 2 dan 4, `HRD-ASM-91`; `VR-PKB-09`; `INT-HR-114` |
| Contract version | `v29` — API 35.3, Validation 34, Integration 26, Permission 33, Data 30 — **approved** 23 November 2026 |
| Dependency | — |
| Klasifikasi | `MEDIUM` — satu service dan satu DTO berubah, menyentuh data sensitif (disiplin) dan perilaku hak akses |
| Task mode | `BACKEND` — instruksi pengguna 23 November 2026: "kerjakan semua task yang ada di backend roadmap tersebut di mulai dari BE-HRD-443" |
| Target tulis | `Areas/Corporate/HumanResource/WorkforceCore/**`; laporan, roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `7e4c3774` (branch `MHamzah`); source task ini sudah di-commit pengguna di `9062bd61` |
| Tanggal | 23 November 2026 |
| Status | ✅ Selesai — 4/4 acceptance criteria terpetakan ke source; build 0 error, QBE Strict `PASS`, registri hak `IsValid=True`; uji API runtime 23 November 2026 `PASS` (bentuk respons; DB dev belum punya data sanksi) |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `WorkforceCore` |
| Pemilik / prefix registry | Human Resource, `Hrd` (`ACTIVE / LEGACY`); entity yang dibaca `HrdDisciplinarySanction` dan `WfpDisciplinaryAction` milik `EmployeeRelationManagement` |
| Keberlakuan | `TOUCHED LEGACY` — tanpa entity baru, tanpa migration |
| QBE yang berlaku | `QBE-DTO-001`, `QBE-SVC-001`, `QBE-PERM-001`, `QBE-API-001`, `QBE-LOG-001` (tidak ada data sensitif di log) |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`, `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Bagian "Data Pribadi & Disiplin" Employee 360 hanya membaca catatan profil lama (`WfpDisciplinaryAction`). Sanksi yang diterbitkan lewat alur disiplin resmi (`HrdDisciplinarySanction`, SP 1–SP 3 dan teguran lisan) tidak pernah tampil. Selain itu, siapa pun yang lolos mode HR (memegang `HrdPersonnelActionOrder : Read`) ikut melihat riwayat disiplin, walaupun tidak memegang hak lihat sanksi.

Contoh: Budi menerima SP 1 lewat alur resmi pada 1 Oktober 2026. Staf administrasi SK yang membuka Employee 360 Budi tidak melihat SP 1 itu (karena hanya membaca catatan lama), tetapi bisa melihat catatan lama yang mungkin tidak boleh ia lihat.

## 2. Proses bisnis

1. Petugas membuka Employee 360 seorang pegawai (`HrdEmployee360 : Read`).
2. Sistem menentukan mode: **HR** (memegang `HrdEmployee360 : ReadFull` atau `HrdPersonnelActionOrder : Read`) atau **Atasan Langsung** (atasan utama aktif). Aturan ini tidak berubah.
3. Pada mode HR, sistem memeriksa hak **`EmployeeDisciplinary : ViewSanctions`**:
   - **Punya hak:** `disciplinaryRecords` diisi sanksi alur resmi (`OFFICIAL`; kolom `employee_id` dicari dengan `MstEmployee.Id` **dan** `MstWorkforceProfile.Id`) ditambah catatan profil lama (`LEGACY`, selalu `isActive = false`), diurutkan tanggal terbit terbaru; `disciplineHidden = false`.
   - **Tanpa hak:** `disciplineHidden = true`; `disciplinaryRecords` dan `disciplinarySanctions` dikirim kosong. Data pribadi lain (NIK, keluarga) tetap mengikuti aturan mode HR yang lama.
4. Pada mode Atasan Langsung, seluruh bagian rahasia (`confidentialPersonalAndDiscipline`) tetap `null` seperti sebelumnya — tidak ada data disiplin yang dikirim.

**Contoh berangka.** Profil Budi punya SP 1 resmi (`employee_id` = `MstEmployee.Id`, terbit 1 Oktober 2026, berlaku sampai 1 April 2027, `Active`), SP 2 resmi yang tercatat dengan `employee_id` = `WorkforceProfileId` (terbit 10 Januari 2026, `Expired`), dan satu catatan lama "Teguran tertulis keterlambatan" (5 Maret 2025). Sari (HR Relations, punya `ViewSanctions`) menerima tiga baris: SP 1 `OFFICIAL` `isActive = true`, SP 2 `OFFICIAL` `isActive = false`, dan catatan lama `LEGACY` `isActive = false`. Staf SK tanpa `ViewSanctions` menerima `disciplineHidden = true` dan daftar kosong.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`WorkforceCore/Controllers/HrdEmployee360Controller.cs`, `WorkforceCore/Services/HrdEmployee360Service.cs`, `WorkforceCore/Services/IHrdEmployee360Service.cs`, `WorkforceCore/DTOs/Employee360Dtos.cs`, `EmployeeRelationManagement/Models/HrdDisciplinarySanction.cs`, `EmployeeRelationManagement/Models/WfpDisciplinaryAction.cs`, `EmployeeRelationManagement/Services/DisciplinaryListQueryService.cs`, `Services/Security/AccessPermissionService.cs`; consumer frontend `employee-360-view.jsx` (read-only).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/WorkforceCore/Services/HrdEmployee360Service.cs` | Pemeriksaan `EmployeeDisciplinary : ViewSanctions` pada mode HR; metode baru `GetDisciplinaryHistoryAsync` (sanksi resmi dua jenis ID + catatan lama, urut terbaru, uraian dipotong 120 karakter); `DisciplineHidden`; ruas usang `DisciplinarySanctions` diisi dari sumber yang sama |
| `Areas/Corporate/HumanResource/WorkforceCore/DTOs/Employee360Dtos.cs` | `DisciplinaryRecords`, `DisciplineHidden`, kelas `Employee360DisciplinaryItemDto`; `DisciplinarySanctions` ditandai `[Obsolete]` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Route dan hak endpoint tetap. Ruas baru `confidentialPersonalAndDiscipline.disciplinaryRecords[]` dan `disciplineHidden`; `disciplinarySanctions` tetap ada (usang) dan kini juga memuat sanksi resmi dengan format `SP1: <nomor> (yyyy-MM-dd)` |
| Database | Tidak ada. Hanya membaca `hrd_disciplinary_sanction` dan `wfp_disciplinary_action`; tanpa migration |
| Keamanan/Auth | Data disiplin kini hanya untuk pemegang `EmployeeDisciplinary : ViewSanctions` (`VR-PKB-09`). Tidak ada pengecualian nama peran |

**Delta terhadap desain 37.3 (dicatat, tidak mengubah kontrak):**

1. Desain menaruh pemeriksaan `ViewSanctions` di controller dan meneruskan `includeDiscipline` ke service. Source sudah memeriksa hak mode HR di dalam service lewat `AccessPermissionService`, sehingga pemeriksaan baru diletakkan di tempat yang sama. Perilakunya identik; signature interface tidak berubah.
2. **Fakta as-is yang mengoreksi desain:** atasan langsung ternyata **tidak pernah** menerima bagian `confidentialPersonalAndDiscipline` (selalu `null` di mode Atasan Langsung). Yang benar-benar kehilangan akses disiplin sesudah task ini adalah **pengguna mode HR yang tidak memegang `ViewSanctions`** (mis. petugas SK). Catatan rilis di `BE-HRD-452` memakai rumusan yang benar ini. Untuk mode Atasan Langsung objeknya tetap `null` (lebih ketat dari kontrak dan konsisten dengan layar yang sudah menampilkan pesan terbatas bila objek kosong).
3. Uraian pelanggaran sanksi resmi dimuat ke `title` dalam bentuk dipotong 120 karakter; isi lengkap tetap lewat detail sanksi per pegawai yang juga dijaga `ViewSanctions`.

## 4. Dokumentasi endpoint

#### Corporate / Human Resource / Workforce Core / Employee 360

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/employee-360` | Profil 360 pegawai; bagian disiplin terisi hanya bagi pemegang `ViewSanctions` | `HrdEmployee360 : Read`; isi disiplin bergantung `EmployeeDisciplinary : ViewSanctions` |

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratchpad>` sesudah `MVP-72` | 0 error, 614 warning; tidak ada warning baru di berkas task ini (CS8602 baris 99 dan 236 adalah kode lama yang tidak disentuh) | `PASS` | Log build 23 November 2026 |
| `dotnet build` penuh seluruh Paket B | 0 error, 611 warning; tidak ada warning di berkas yang diubah | `PASS` | Log build 23 November 2026 |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` (19 berkas Paket B) | 0 VIOLATION, 0 REVIEW, 0 INFO, `PASS` | `PASS` | Keluaran checker |
| Validator registri hak akses (skrip `dotnet fsi` atas assembly hasil build) | `IsValid=True`, 2.662 identitas, 0 endpoint tak terdaftar | `PASS` | Keluaran skrip |
| Penelusuran: pegawai dengan sanksi resmi berkunci `MstEmployee.Id` dan `WorkforceProfileId` + catatan lama | Kedua kunci masuk `subjectIds`; catatan lama dibaca per `WorkforceProfileId` | `PASS` | `GetDisciplinaryHistoryAsync` |
| Penelusuran: mode HR tanpa `ViewSanctions` | `DisciplineHidden = true`, kedua daftar kosong | `PASS` | Blok pengisian `ConfidentialPersonalAndDisciplineDto` |
| Uji API runtime di DB dev (`QuilvianNewDevHamzah`), 23 November 2026 — backend `9062bd61` di port lokal 5199, akun superadmin + akun non-admin: Employee 360 profil aktif oleh superadmin (mode HR, memegang `ViewSanctions`) | `200`, `viewMode = HR`, `disciplineHidden = false`, `disciplinaryRecords = []` (bentuk sesuai kontrak; DB dev: 34 pegawai aktif; 0 kontrak, 0 lisensi, 0 SPK/RKK, 0 sanksi, 0 evaluasi kandidat) | `PASS` | Skrip uji API (27 skenario: 26 `PASS`, 1 gagal karena login akun uji atasan ditolak `401` — masalah akun, bukan kode) |
| Uji API akun mode HR **tanpa** `ViewSanctions` | Tidak ada akun uji dengan kombinasi hak itu | `NOT RUN` | Dibuktikan lewat source (`DisciplineHidden = !canViewSanctions`) |
| Kueri baca-saja jumlah `WfpDisciplinaryAction` (`HRD-DEC-238` butir 1; izin pengguna 23 November 2026) | **0 baris** (tidak ada yang aktif maupun terhapus) — HR tidak perlu memasukkan ulang catatan lama ke alur resmi | `PASS` | Kueri `SELECT` baca-saja (`readonly` session) |

Uji manual: `PASS` sebatas data dev (lihat tabel).

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Pemegang `ViewSanctions`: SP 1 resmi (`MstEmployee.Id`), SP 2 resmi (`WorkforceProfileId`), dan satu catatan lama → 2 `OFFICIAL` + 1 `LEGACY`, `disciplineHidden = false` (`AT-HRD-PKB-13`) | Terpenuhi | `subjectIds` memuat kedua ID; baris resmi dan lama digabung lalu diurutkan |
| 2. Akun `HrdEmployee360 : Read` tanpa `ViewSanctions` → `disciplineHidden = true`, kedua daftar kosong, bagian 360 lain tetap terisi (`AT-HRD-PKB-14` bagian backend) | Terpenuhi | Mode HR tanpa hak: `DisciplineHidden = !canViewSanctions`; mode Atasan Langsung: objek rahasia tetap `null` (delta 2) |
| 3. `LEGACY` selalu `isActive = false`; `OFFICIAL` aktif hanya bila `Status = Active` dan `ValidUntil` belum lewat | Terpenuhi | Pemetaan `IsActive` di `GetDisciplinaryHistoryAsync` |
| 4. Hak endpoint tetap `HrdEmployee360 : Read`; tanpa pengecualian nama peran | Terpenuhi | Controller tidak diubah; pemeriksaan memakai `HasAccessAsync` |
| DoD: laporan tracked, tanda status roadmap | Terpenuhi | Berkas ini; roadmap bagian 30 |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning baru |
| Masalah yang diketahui | (1) Kolom `employee_id` modul disiplin berisi dua jenis ID — utang teknis, sengaja tidak diperbaiki. (2) Temuan di luar scope: service memeriksa `HrdEmployee360 : ReadFull`, tetapi aksi itu **tidak terdaftar** di registri (snapshot hanya memuat `HrdEmployee360.Read`), sehingga mode HR praktis hanya didapat lewat `HrdPersonnelActionOrder : Read` |
| Risiko tersisa | Petugas mode HR tanpa `ViewSanctions` kehilangan riwayat disiplin yang selama ini terlihat — disengaja (`HRD-DEC-238` butir 4), masuk catatan rilis `BE-HRD-452` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE`. Pengguna meng-commit source di tengah sesi (`9062bd61`); isinya sama dengan hasil task |
| Status Git | `git status --short` sesudah source: bersih untuk `Areas/` dan `Program.cs` (sudah di-commit pengguna); hanya dokumen blueprint yang berubah |
| Langkah berikutnya | `FE-HRD-393` (tampilan `disciplinaryRecords`) |
