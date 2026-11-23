# Laporan Perubahan Backend — `BE-HRD-452`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-452` |
| Judul | Verifikasi backend Paket B: kontrak `v29`, registri hak, build, QBE, waktu muat, dan catatan rilis |
| Slice | `S-PKB` — Paket B Navigasi SDM / Verifikasi (`MVP-72`, `MVP-73`) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 30, kartu `BE-HRD-452` |
| Trace | `FR-PKB-01` s.d. `FR-PKB-04`, `FR-PKB-08` s.d. `FR-PKB-10`, `FR-PKB-12`, `FR-PKB-14`, `FR-PKB-15`; DoD PRD 42.11; kebutuhan non-fungsional 42.9 |
| Contract version | `v29` seluruh bagian — **approved** 23 November 2026 |
| Dependency | `BE-HRD-443` ✅, `BE-HRD-445` ✅, `BE-HRD-446` ✅, `BE-HRD-451` ✅ |
| Klasifikasi | `LIGHT` — verifikasi tanpa perubahan source |
| Task mode | `BACKEND` — instruksi pengguna 23 November 2026 |
| Target tulis | Laporan, roadmap, traceability (tidak ada perubahan source) |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `9062bd61` (branch `MHamzah`, berisi seluruh source Paket B) |
| Tanggal | 23 November 2026 |
| Status | 🟡 Sebagian — AC 1, 2, 3, 5 terpenuhi; AC 4 (waktu muat `/summary` di DB dev) `NOT RUN` karena menyalakan backend pada DB dev bersama belum diizinkan |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `HumanResource` / `WorkforceCore`, `RecruitmentManagement`, `EmployeeRelationManagement`, `LifecycleManagement` |
| Keberlakuan | Verifikasi; tanpa perubahan source |
| QBE yang berlaku | Seluruh ID yang dicatat pada `BE-HRD-443` s.d. `BE-HRD-451` |
| Sumber governance | `AGENTS.md`; `docs/engineering/*`; `rules/backend/*` suite Skill 1.22.0 |

---

## 1. Masalah yang diperbaiki

Sebelum frontend Paket B dikerjakan dan sebelum rilis, perlu satu bukti bahwa seluruh endpoint backend Paket B sesuai kontrak `v29`, hak barunya terdaftar, build dan QBE lulus, serta langkah rilis yang wajib (pemberian hak dan perubahan perilaku) tertulis jelas.

## 2. Proses bisnis

Verifikasi dilakukan atas source `9062bd61` tanpa menyalakan aplikasi: pemeriksaan endpoint terhadap kontrak, build, QBE Strict, validator registri hak akses dari assembly hasil build, lalu penyusunan catatan rilis.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Seluruh 18 berkas source Paket B (`git show --stat 9062bd61`), kontrak `v29` bagian 35.1–35.6.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| — | Tidak ada perubahan source; hanya laporan dan dokumen blueprint |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Sesuai kontrak `v29` (tabel 5.1) |
| Database | **Tanpa migration**; tidak ada tabel atau kolom baru |
| Keamanan/Auth | Dua hak baru wajib diberikan sebelum rilis (catatan rilis 6) |

## 4. Dokumentasi endpoint

#### Corporate / Human Resource / Workforce Core / Workforce Dashboard

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/human-resource/workforce-dashboard/summary` | Lima panel | `HrdWorkforceDashboard : Read` |
| `GET` | `/api/v1/corporate/human-resource/workforce-dashboard/drilldown` | Daftar nama | `HrdWorkforceDashboard : Read` |

#### Corporate - HRD Employee Disciplinary & Just Culture

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/hrd/disciplinary/sanctions` | Parameter baru `statuses` | `EmployeeDisciplinary : ViewSanctions` |

#### Corporate / Human Resource / Workforce Core / Employee 360

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/employee-360` | Disiplin menurut `ViewSanctions` | `HrdEmployee360 : Read` |

#### Corporate / Human Resource / Recruitment Management / Candidate Evaluation

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/corporate/human-resource/recruitment/evaluations` | Daftar tersaring penilai; `track`, `pendingOnly` | `CandidateEvaluation : Read` (+ `ViewAll`) |
| `GET` | `/api/v1/corporate/human-resource/recruitment/evaluations/{id}` | Detail; `403` milik penilai lain | `CandidateEvaluation : Read` |
| `POST` | `/api/v1/corporate/human-resource/recruitment/evaluations/{id}/submit-clinical` | Hanya penilai yang ditugaskan | `CandidateEvaluation : EvaluateClinical` |

## 5. Verifikasi

### 5.1 Kesesuaian endpoint dengan kontrak

| Kontrak | Yang diperiksa | Hasil |
| --- | --- | --- |
| 35.1 | Route `api/v1/corporate/human-resource/workforce-dashboard`, `ControllerName = "HrdWorkforceDashboard"`, tag persis, `GET summary` dan `GET drilldown`, `[AccessPermission("HrdWorkforceDashboard","Read")]` | Sesuai |
| 35.2 | `GET api/v1/hrd/disciplinary/sanctions`, `EmployeeDisciplinary : ViewSanctions`, `statuses` + validasi | Sesuai |
| 35.3 | Route Employee 360 dan hak `HrdEmployee360 : Read` tidak berubah; ruas `disciplinaryRecords`, `disciplineHidden` | Sesuai (delta mode Atasan Langsung dicatat di `BE-HRD-443`) |
| 35.4 | Atribut akses empat endpoint evaluasi tidak berubah; perilaku daftar/detail/`submit-clinical` | Sesuai |
| 35.6 | Tujuh pesan galat Bahasa Indonesia | Ketujuhnya ada persis di source |

### 5.2 Perintah

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj --no-restore -p:OutDir=<scratchpad>` (seluruh Paket B) | Berhasil, 0 error, 611 warning; tidak ada warning di 18 berkas yang diubah | `PASS` | Log build 23 November 2026 (5 menit 6 detik) |
| QBE `Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path` (19 berkas: 18 source + `Program.cs`) | 0 VIOLATION, 0 REVIEW, 0 INFO, `PASS` | `PASS` | Keluaran checker |
| Validator registri hak akses (assembly hasil build, skrip `dotnet fsi`) | `IsValid=True`, 2.662 identitas (sebelumnya 2.660 + `HrdWorkforceDashboard.Read` + `CandidateEvaluation.ViewAll`); 0 endpoint tak terdaftar, 0 identitas ganda, 0 `AccessType` tidak sah | `PASS` | Keluaran skrip |
| Migration | Tidak ada migration yang dibuat atau diperlukan | `PASS` | `git show --stat 9062bd61` (tanpa `Migrations/`) |
| Waktu muat `/summary` di DB dev (target ≤ 2 detik untuk ±500 pegawai) | Belum diukur | `NOT RUN` | Menyalakan backend menjalankan ±50 worker latar pada DB dev bersama; butuh izin pengguna |
| Uji API ringkas per endpoint di DB dev | Belum dijalankan | `NOT RUN` | Alasan sama |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Seluruh endpoint 35.1–35.4 diperiksa terhadap source: route, method, atribut akses, kode status, pesan 35.6 | Terpenuhi | Tabel 5.1 |
| 2. Validator registri bersih; dua hak baru tampil di Akses Role | Terpenuhi | `IsValid=True`; kedua identitas ada di snapshot (muncul di layar Akses Role saat aplikasi dijalankan) |
| 3. `dotnet build` 0 galat; QBE tanpa pelanggaran baru; laporan menyatakan tanpa migration | Terpenuhi | Tabel 5.2 |
| 4. Waktu muat `/summary` dicatat terhadap target ≤ 2 detik | **Belum terpenuhi** | `NOT RUN` — menunggu izin menyalakan backend pada DB dev |
| 5. Catatan rilis memuat pemberian dua hak dan perubahan perilaku | Terpenuhi | Bagian "Catatan rilis" di bawah |
| DoD: backend `MVP-72`/`MVP-73` siap dipakai frontend | Terpenuhi untuk pengembangan frontend; rilis menunggu AC 4 dan pemberian hak | — |

### Catatan rilis Paket B (backend)

1. **Berikan `HrdWorkforceDashboard : Read`** (Manajemen Role → Akses Role, modul *Human Resource Workforce Core*, layar *Dashboard SDM*) ke Manajer SDM dan Admin HRD.
2. **Berikan `CandidateEvaluation : ViewAll`** (modul *Human Resource Recruitment*, *Evaluasi Kandidat* → *View All Evaluations*) ke Recruiter HR dan Manajer SDM **sebelum** rilis. Tanpa langkah ini, recruiter tiba-tiba hanya melihat evaluasi yang ditugaskan kepadanya sendiri.
3. **Perubahan perilaku yang disengaja (`HRD-DEC-238` butir 4):** bagian riwayat disiplin Employee 360 kini hanya terisi bagi pemegang `EmployeeDisciplinary : ViewSanctions`. Yang terdampak adalah **pengguna mode HR tanpa `ViewSanctions`** (misalnya petugas SK yang memegang `HrdPersonnelActionOrder : Read`). Koreksi atas desain 37.9: atasan langsung **tidak** terdampak, karena sejak awal tidak pernah menerima bagian rahasia Employee 360.
4. **Perubahan perilaku evaluasi:** asesor yang tidak ditugaskan tidak lagi dapat membuka atau mengirim nilai uji klinis evaluasi milik penilai lain.
5. Tidak ada migration; tidak perlu `dotnet ef database update` untuk Paket B.

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning baru |
| Masalah yang diketahui | `BE-HRD-447` AC 1 (perbandingan respons layar Kontrak) juga menunggu uji DB dev; temuan di luar scope: aksi `HrdEmployee360 : ReadFull` dipakai service tetapi tidak terdaftar; worklist siklus hidup memakai aturan 60 hari tersendiri; radar sertifikat menghitung hari-H sebagai ≤ 30 hari |
| Risiko tersisa | Rilis tanpa langkah catatan rilis 1–2 membuat dashboard tidak dapat dibuka dan recruiter kehilangan visibilitas evaluasi |
| Perubahan sampingan | `NONE` |
| Interupsi | Pengguna meng-commit source dan dokumen blueprint di tengah sesi (`1be0a12a`, `9062bd61`); tidak ada konflik |
| Status Git | Source bersih (sudah di-commit); dokumen blueprint (laporan, roadmap, traceability) berubah dan belum di-commit |
| Langkah berikutnya | Izin menyalakan backend di DB dev untuk AC 4 dan uji API Paket B; lalu `FE-HRD-393` s.d. `FE-HRD-401` |
