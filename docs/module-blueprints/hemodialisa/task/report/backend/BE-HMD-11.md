# Laporan Perubahan Backend — `BE-HMD-11`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HMD-11` |
| Judul | Penugasan Staf, Pengecekan Gerbang Kompetensi, dan Penyusunan Daftar Kerja Unit |
| Slice | `MVP-3` — Penjadwalan Sesi dan Pencegahan Tabrakan Sumber Daya |
| Roadmap | `docs/module-blueprints/hemodialisa/roadmap/backend-roadmap.md` bagian 2.4 |
| Trace | `FR-HMD-034`, `CAP-20`, `CAP-21`, `HMD-DEC-013`, `HMD-DEP-002`, `NFR-006`; `contracts/api-contract.md` grup Schedule & Sessions; `permission-audit-matrix.md` bagian 7; `HMD-VAL-037`, `HMD-VAL-038` |
| Contract version | `HMD-CONTRACT-v1` — `approved` 18 September 2026 |
| Dependency | `BE-HMD-10` ✅ |
| Klasifikasi | `MEDIUM` — skor 6: repository 0, berkas diperiksa 1, berkas diubah 1, logika 1, kontrak API 1, database 1, keamanan 1, UI 0 |
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
| QBE ID yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-DEL-001`, `QBE-PAGE-001`, `QBE-OPT-001` |

---

## 1. Masalah yang diperbaiki

Koordinator butuh satu layar "siapa cuci darah hari ini, di mesin mana, dengan perawat siapa".
Layar itu sering tampil di monitor bersama di ruang unit, sehingga tidak boleh membocorkan
diagnosis atau hasil serologi pasien. Selain itu, kewenangan dialisis perawat belum dapat
diperiksa otomatis karena data Human Resource belum tersedia — sistem harus jujur soal itu,
bukan berpura-pura sudah memeriksa.

---

## 2. Proses bisnis

**Penugasan petugas.**

1. Koordinator menyimpan dokter penanggung jawab dan daftar perawat sesi
   (`PUT /{id}/staff-assignments`). Petugas yang tidak lagi ada di daftar ditandai terhapus
   (jejaknya tetap), petugas baru ditambahkan, petugas yang tetap dipertahankan.
2. Untuk setiap petugas, `HmdCompetencyGateService` mengembalikan salah satu dari tiga status:
   `Verified`, `NotAuthorized`, atau `NotVerifiable`. **Saat ini selalu `NotVerifiable`** dengan
   rujukan `HMD-DEP-002`, karena pemetaan kewenangan HR ke tindakan dialisis belum disetujui —
   menebaknya berarti mengarang kebijakan kredensial. Status ini tidak pernah ditulis `Verified`.
3. Sakelar pengaturan unit menentukan akibatnya:

| Sakelar | Bawaan | Bila `false` | Bila `true` |
| --- | --- | --- | --- |
| `EnforceCompetencyCheck` | `false` | Penugasan diterima dengan peringatan "Kewenangan petugas ini belum dapat diperiksa …" | Petugas `NotAuthorized` ditolak `422 HMD-VAL-037` |
| `EnforceNurseRatio` | `false` | Melebihi `MaxPatientsPerNurse` hanya peringatan | Ditolak `422 HMD-VAL-038` |

4. Sesi `AwaitingFinalization` atau `Cancelled` tidak dapat diubah penugasannya; sesi `Finalized`
   ditolak `423 HMD-VAL-075`.

**Contoh.** Perawat Rina ditugaskan pada sesi pagi. Karena kredensialing HR belum terhubung,
penugasannya tersimpan dengan `CompetencyVerificationStatus = NotVerifiable`. Sesi tetap dapat
dilanjutkan karena `EnforceCompetencyCheck = false`, dan worklist menandai sesi itu
`HasUnverifiedCompetency = true` supaya koordinator sadar.

**Daftar kerja (worklist).**

1. `GET /worklist` berfilter tanggal, shift, status sesi, unit, "butuh dokumentasi", dan
   pencarian pasien; berhalaman (`PagedResult<HmdWorklistItemResponse>`, bawaan 25 per halaman).
2. Setiap baris memuat jam, pasien, mesin, station, dokter, perawat utama, status, dan tiga
   penanda ringkas: `IsIsolationRequired` (boolean), `HasUnverifiedCompetency`, dan
   `NeedsDocumentation`.
3. **Tidak ada** nama penyakit, jenis kebutuhan isolasi, maupun hasil serologi pada respons.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `contracts/api-contract.md` grup Schedule & Sessions, `permission-audit-matrix.md` bagian 7
- `MstWorkforceProfile`, `WfpClinicalPrivilege`, `MstDoctor`, `HmdSessionStaffAssignment`, `HmdSetting`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Services/HmdCompetencyGateService.cs` | Baru. Selalu `NotVerifiable` beserta rujukan dan waktu pemeriksaan |
| `Services/HmdScheduleService.cs` | Worklist, ringkasan, metadata filter, baca/simpan penugasan, `PlanStaffAsync` (kompetensi dan rasio perawat) |
| `Controllers/HmdScheduleController.cs` | Endpoint worklist dan penugasan |
| `DTOs/HmdSessionDtos.cs` | `HmdWorklistQuery`, `HmdWorklistItemResponse`, request/response penugasan |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 3 endpoint kontrak terpenuhi. **Delta**: `worklist/filters/metadata` dan `worklist/summary` untuk layar daftar kerja |
| Database | Tidak ada perubahan schema. Unique `(SessionId, WorkforceProfileId)` hanya untuk baris belum terhapus, sehingga petugas yang pernah dilepas dapat ditugaskan kembali |
| Keamanan/Auth | `HemodialysisSchedule : Read/Update`. Privasi: DTO worklist disaring di backend (`NFR-006`) |

---

## 4. Dokumentasi endpoint

#### Health Services / Hemodialysis Management / Hemodialysis Schedule

Base: `/api/v1/health-services/hemodialysis-management/hemodialysis-sessions`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/worklist/filters/metadata` | Pilihan filter tanggal, shift, dan status untuk layar daftar kerja | `HemodialysisSchedule : Read` |
| `GET` | `/worklist/summary` | Jumlah sesi per kelompok status pada tanggal terpilih | `HemodialysisSchedule : Read` |
| `GET` | `/worklist` | Daftar kerja harian berhalaman, tanpa data serologi | `HemodialysisSchedule : Read` |
| `GET` | `/{id}/staff-assignments` | Penugasan petugas sesi beserta status kompetensinya | `HemodialysisSchedule : Read` |
| `PUT` | `/{id}/staff-assignments` | Menyimpan dokter penanggung jawab dan perawat sesi | `HemodialysisSchedule : Update` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` (lihat `BE-HMD-01`) | `0 Error(s)`, `224 Warning(s)`, 0 dari berkas Hemodialisa | `PASS` | Log build 22 September 2026 |
| QBE Strict atas 94 berkas | `PASS`, 0 violation | `PASS` | 22 September 2026 |
| Audit akses reflektif | Endpoint kontrak ada dengan hak akses sama persis | `PASS` | Skrip audit 22 September 2026 |
| Pemeriksaan source kompetensi | Selalu `NotVerifiable`; tidak pernah `Verified` | `PASS` | `HmdCompetencyGateService.cs` |
| Pemeriksaan source proyeksi worklist | Hanya `IsIsolationRequired` boolean; tidak ada kolom serologi atau jenis isolasi | `PASS` | `HmdScheduleService.cs` baris 140–165 |
| Pemeriksaan source paging | `NormalizePaging` dan `PagedResult` | `PASS` | `HmdScheduleService.cs` baris 95–97 |
| Uji runtime HTTP | Tidak dijalankan | `NOT RUN` | Aplikasi pengguna berjalan dari build lama |
| AUTOMATED TEST | `NOT APPLICABLE` — backend tidak memelihara project test otomatis (`rules/backend/TEST_POLICY.md`) | `NOT APPLICABLE` | `WorklistAndStaffAssignmentTests` **dikecualikan atas keputusan pengguna 22 September 2026** |

Uji manual: `NOT FEASIBLE` pada sesi ini.

**Tidak dijalankan:** uji runtime HTTP — dikecualikan menurut keputusan tetap pemilik 10 September 2026.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Penugasan perawat Rina tersimpan `NotVerifiable`; sesi tetap berjalan karena `EnforceCompetencyCheck = false` | Terpenuhi | `HmdCompetencyGateService` + `PlanStaffAsync` |
| 2. Worklist berpenanda isolasi boolean tanpa string penyakit | Terpenuhi | Proyeksi `HmdWorklistItemResponse` |
| 3. Query worklist berhalaman standar Quilvian | Terpenuhi | `PagedResult<HmdWorklistItemResponse>` |
| DoD: service kompetensi, penugasan, dan worklist | Terpenuhi pada source; pengujian otomatis **dikecualikan atas keputusan pengguna 22 September 2026** | — |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Gerbang kompetensi belum pernah bisa menghasilkan `NotAuthorized` sampai HR menyediakan pembacaan kewenangan (`HMD-DEP-002`); menyalakan `EnforceCompetencyCheck` saat ini tidak menolak siapa pun |
| Masalah yang diketahui | `NONE` |
| Risiko tersisa | Penanda `IsIsolationRequired` tetap terbaca di layar bersama; itu disengaja kontrak, karena petugas perlu tahu tanpa tahu penyakitnya |
| Perubahan sampingan | `NONE` |
| Interupsi | Satu kali pemadatan konteks sesi |
| Status Git | Keluaran lengkap tercantum pada laporan `BE-HMD-01` bagian 7 |
| Langkah berikutnya | Kontrak HR untuk kewenangan dialisis (`HMD-DEP-002`); uji runtime oleh pemilik |
