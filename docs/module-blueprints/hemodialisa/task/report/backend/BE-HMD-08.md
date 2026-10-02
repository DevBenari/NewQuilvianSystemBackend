# Laporan Perubahan Backend — `BE-HMD-08`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HMD-08` |
| Judul | Pengelolaan Program Episode Pasien, Penilaian Kelayakan, Akses Vaskular, dan Isolasi PPI |
| Slice | `MVP-2` — Permintaan HD Masuk, Program Episode, dan Resep Hemodialisa |
| Roadmap | `docs/module-blueprints/hemodialisa/roadmap/backend-roadmap.md` bagian 2.3 |
| Trace | `FR-HMD-010` s.d. `FR-HMD-014`, `CAP-16`, `CAP-25`, `CAP-26`, `CAP-27`, `HMD-DEC-001`; `contracts/api-contract.md` grup Episode; `state-transition-matrix.md` bagian 2; `HMD-VAL-010` s.d. `HMD-VAL-013` |
| Contract version | `HMD-CONTRACT-v1` — `approved` 18 September 2026 |
| Dependency | `BE-HMD-01` ✅, `BE-HMD-03` ✅ |
| Klasifikasi | `HEAVY` — skor 9: repository 0, berkas diperiksa 2, berkas diubah 2, logika 2, kontrak API 1, database 1, keamanan 1, UI 0 |
| Task mode | `BACKEND` |
| Target tulis | `Areas/HealthServices/HemodialysisManagement/{Controllers,DTOs,Services}/` |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Commit backend saat dikerjakan | `25b02786` pada branch `MHamzah` — belum di-commit |
| Tanggal | 22 September 2026 |
| Status | ✅ Selesai — tiga acceptance criteria terpetakan ke source |

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module / Submodule | `HealthServices` / `HemodialysisManagement` / `Hemodialysis` |
| Pemilik / prefix registry | `Hmd` — `ACTIVE` |
| Keberlakuan | `NEW CODE` |
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001`, `QBE-PAGE-001`, `QBE-CODE-001` s.d. `QBE-CODE-006` |

---

## 1. Masalah yang diperbaiki

Pasien HD rutin menjalani cuci darah dua sampai tiga kali seminggu selama bertahun-tahun.
Tanpa "program" yang menampung seluruh sesinya, kelayakan klinis, akses vaskular, hasil
serologi, dan keputusan isolasi tersebar di catatan terpisah. Risiko nyatanya: pasien dengan
Hepatitis B dijadwalkan ke mesin umum karena keputusan isolasinya tidak terbaca saat penjadwalan.

---

## 2. Proses bisnis

**Program HD (episode).**

1. Petugas membuat episode `Draft` (`POST /`) dengan pasien, unit HD, dan dokter penanggung
   jawab (DPJP) aktif; data tidak lengkap → `400 HMD-VAL-010`. Nomor `HD-EP-xxxxxxxx`.
2. Episode diaktifkan (`PATCH /{id}/status` → `Active`). Bila pasien sudah punya episode aktif
   lain → `409 HMD-VAL-011`. Unique index bersyarat di database menjadi lapis terakhir bila dua
   aktivasi terjadi bersamaan.
3. Episode dapat ditangguhkan (`Suspended`) dengan alasan wajib (`400 HMD-VAL-012`) dan
   diaktifkan kembali.
4. Episode ditutup (`Closed`) dengan sebab penutupan wajib. Episode berjalan hanya boleh ditutup
   bila **seluruh** sesinya sudah `Finalized` atau `Cancelled`; bila tidak → `422 HMD-VAL-013`
   beserta daftar sesi yang menahan (nomor, tanggal, status).

**Sub-proses di bawah episode** — masing-masing hanya pada episode yang belum ditutup:

| Sub-proses | Pelaku | Aturan |
| --- | --- | --- |
| Penilaian kelayakan | Dokter — pelaku yang tidak tertaut ke `MstDoctor` ditolak `403` | Sistem tidak pernah menentukan kelayakan sendiri; hasilnya `Eligible`, `Deferred`, `Modified`, atau `Referred` beserta alasan |
| Akses vaskular | Perawat/dokter | Tipe AV fistula, AV graft, kateter (lokasi femoral/jugular/subklavia dicatat pada `AccessSite`), atau lainnya; status `Usable`/`NeedsAttention`/`NotUsable` dapat diubah |
| Tinjauan serologi | Petugas berhak `HemodialysisSerology` | HBsAg, Anti-HBs, Anti-HCV, Anti-HIV: **merujuk** pemeriksaan lab (`LabExaminationId`), tanggal, penanda hasil, dan catatan tinjauan. Setiap pembacaan daftar serologi dicatat logger |
| Keputusan isolasi | Pemegang `HemodialysisIsolation : Decide` (tim PPI/dokter dialisis) | Keputusan baru menutup keputusan aktif sebelumnya (tanggal akhir diisi sehari sebelum yang baru berlaku). Kebutuhan: `None`, `HepatitisB`, atau `Other` |

**Contoh.** Hasil Anti-HCV Ibu Sinta reaktif. Dokter dialisis mencatat keputusan isolasi
`Other` berlaku mulai 23 September dengan rujukan tinjauan serologi tersebut. Mulai tanggal itu,
penjadwalan hanya menerima mesin yang dikhususkan untuk kebutuhan `Other` dan station isolasi;
mesin umum ditolak `422 HMD-VAL-035` (`BE-HMD-10`).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `contracts/api-contract.md` grup Episode, `state-transition-matrix.md` bagian 2, `validation-matrix.md`, `permission-audit-matrix.md` bagian 5 dan 7
- `HmdEpisode`, `HmdEligibilityAssessment`, `HmdVascularAccess`, `HmdSerologyReview`, `HmdIsolationDecision`
- `InpatientClinicalContextService`, `MstPatient`, `MstDoctor`, `MstServiceUnit`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Services/HmdEpisodeService.cs` | Baru. Episode (ringkasan, daftar, rincian, buat, ubah, ubah status) dan empat sub-proses |
| `Controllers/HmdEpisodeController.cs` | Baru, 7 endpoint |
| `Controllers/HmdEligibilityController.cs`, `HmdVascularAccessController.cs`, `HmdSerologyController.cs`, `HmdIsolationController.cs` | Baru — satu controller per Resource hak akses, berbagi base URL episode, karena layar Akses Role mendaftarkan aksi per `ControllerName` |
| `DTOs/HmdEpisodeDtos.cs` | Baru |
| `Services/HmdServiceSupport.cs` | `GetActiveIsolationAsync` — kebutuhan isolasi yang berlaku pada tanggal tertentu |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 14 endpoint kontrak terpenuhi. **Delta**: `filters/metadata` dan `summary` episode untuk layar daftar |
| Database | Tidak ada perubahan schema |
| Keamanan/Auth | Lima Resource: `HemodialysisEpisode`, `HemodialysisEligibility`, `HemodialysisVascularAccess`, `HemodialysisSerology`, `HemodialysisIsolation`. Serologi hanya terbaca lewat hak `HemodialysisSerology : Read` dan tidak pernah muncul di worklist |

---

## 4. Dokumentasi endpoint

#### Health Services / Hemodialysis Management / Hemodialysis Episode

Base: `/api/v1/health-services/hemodialysis-management/hemodialysis-episodes`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan filter layar daftar episode | `HemodialysisEpisode : Read` |
| `GET` | `/summary` | Jumlah episode per status | `HemodialysisEpisode : Read` |
| `GET` | `/` | Daftar episode berhalaman | `HemodialysisEpisode : Read` |
| `GET` | `/{id}` | Rincian episode | `HemodialysisEpisode : Read` |
| `POST` | `/` | Membuat episode `Draft` (`201`) | `HemodialysisEpisode : Create` |
| `PUT` | `/{id}` | Mengubah data episode | `HemodialysisEpisode : Update` |
| `PATCH` | `/{id}/status` | Mengaktifkan, menangguhkan, atau menutup episode | `HemodialysisEpisode : ChangeStatus` |
| `GET` | `/{id}/eligibility-assessments` | Riwayat penilaian kelayakan | `HemodialysisEligibility : Read` |
| `POST` | `/{id}/eligibility-assessments` | Dokter mencatat keputusan kelayakan | `HemodialysisEligibility : Decide` |
| `GET` | `/{id}/vascular-accesses` | Daftar akses vaskular | `HemodialysisVascularAccess : Read` |
| `POST` | `/{id}/vascular-accesses` | Mencatat akses vaskular | `HemodialysisVascularAccess : Create` |
| `PATCH` | `/vascular-accesses/{accessId}/status` | Mengubah kelaikan fungsi akses | `HemodialysisVascularAccess : Update` |
| `GET` | `/{id}/serology-reviews` | Riwayat tinjauan serologi; pembacaannya dicatat logger | `HemodialysisSerology : Read` |
| `POST` | `/{id}/serology-reviews` | Mencatat tinjauan serologi berujuk lab | `HemodialysisSerology : Create` |
| `GET` | `/{id}/isolation-decisions` | Riwayat keputusan isolasi | `HemodialysisIsolation : Read` |
| `POST` | `/{id}/isolation-decisions` | Menetapkan keputusan isolasi | `HemodialysisIsolation : Decide` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` (lihat `BE-HMD-01`) | `0 Error(s)`, `224 Warning(s)`, 0 dari berkas Hemodialisa | `PASS` | Log build 22 September 2026 |
| QBE Strict atas 94 berkas | `PASS`, 0 violation | `PASS` | 22 September 2026 |
| Audit akses reflektif | 14 endpoint kontrak ada dengan hak akses sama persis | `PASS` | Skrip audit 22 September 2026 |
| Pemeriksaan source satu episode aktif | `409 HMD-VAL-011` di service; unique violation dipetakan ke kode yang sama | `PASS` | `HmdEpisodeService.cs` baris 326–337 dan 411 |
| Pemeriksaan source penutupan | Sesi yang belum `Finalized`/`Cancelled` → `422 HMD-VAL-013` dengan rincian | `PASS` | `HmdEpisodeService.cs` baris 360–384 |
| Index di DB | `IX_HmdEpisode_PatientId_Active` unique bersyarat terpasang | `PASS` | `pg_indexes` `QuilvianNewDevHamzah` |
| Uji runtime HTTP | Tidak dijalankan | `NOT RUN` | Aplikasi pengguna berjalan dari build lama |
| AUTOMATED TEST | `NOT APPLICABLE` — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`) | `NOT APPLICABLE` | `EpisodeLifecycleAndIsolationTests` **dikecualikan atas keputusan pengguna 22 September 2026** |

Uji manual: `NOT FEASIBLE` pada sesi ini.

**Tidak dijalankan:** uji runtime HTTP — dikecualikan menurut keputusan tetap pemilik 10 September 2026.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Episode aktif kedua untuk pasien yang sama → `409` | Terpenuhi | Service + unique index bersyarat |
| 2. Penutupan saat masih ada sesi `AwaitingFinalization` → `422` dengan rincian sesi | Terpenuhi | `HMD-VAL-013` beserta `HmdBlockingSessionItem` |
| 3. Anti-HCV reaktif → keputusan isolasi tersimpan dan menjadi prasyarat penjadwalan mesin | Terpenuhi | `CreateIsolationDecisionAsync`; dibaca `GetActiveIsolationAsync` pada penjadwalan |
| DoD: seluruh endpoint episode dan sub-proses sesuai kontrak; invariant 1 episode aktif terjaga | Terpenuhi | Audit akses; index DB |
| DoD: pengujian otomatis | **Dikecualikan atas keputusan pengguna 22 September 2026** | — |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` |
| Masalah yang diketahui | Sakelar `HmdSetting.AllowMultipleActiveEpisodePerPatient = true` dibaca service, tetapi episode aktif kedua tetap ditolak unique index bersyarat yang ditetapkan `data-dictionary.md`. Hasilnya *fail-closed*. Bila rumah sakit kelak membutuhkan lebih dari satu episode aktif, index itu harus diubah lewat keputusan pemilik dan migration baru |
| Risiko tersisa | Tinjauan serologi menyimpan penanda hasil dan ringkasan; nilai lengkap tetap di modul Laboratorium |
| Perubahan sampingan | `NONE` |
| Interupsi | Satu kali pemadatan konteks sesi |
| Status Git | Keluaran lengkap tercantum pada laporan `BE-HMD-01` bagian 7 |
| Langkah berikutnya | Uji runtime oleh pemilik; `BE-HMD-09` |
