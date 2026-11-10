# Laporan Perubahan Backend — `BE-RHB-008`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RHB-008` |
| Judul | Worklist dan ruang kerja episode (baca) |
| Slice | S-B — Permintaan dan episode (`MVP-1`) |
| Roadmap | `docs/module-blueprints/rehab-medik/roadmap/backend-roadmap.md` — kartu `BE-RHB-008` |
| Trace | FEAT-007..029 (tampilan); `state-transition-matrix.md` §2–5 (aksi per status) |
| Contract version | Set kontrak `v1`, `approved` 2026-11-05 — grup API *Rehab Episode* |
| Dependency | `BE-RHB-007` ✅ |
| Klasifikasi | `STANDARD` — endpoint baca, tanpa schema |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/RehabilitationManagement/{Controllers,Services,DTOs}`, dokumen blueprint |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `6bb3fba7c0dab893647b87018a73c55359c19d08` (`sukmagp`), belum di-commit |
| Tanggal | 2026-11-05 |
| Status | **✅ Selesai** (2026-11-05). Build `PASS`, QBE Strict `PASS`, uji API manual 28/28. Urutan kronologis timeline baru terbukti lewat kode, karena belum ada dokumen episode di DB dev (§6). |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RehabilitationManagement` (`Rhb`, `ACTIVE`) |
| Applicability | `NEW CODE` |
| Registry | `Rhb` `ACTIVE`. Tidak ada entity baru. |
| Jenis capability | **Transaksi — worklist/read-only.** Hanya `GET`; episode dibuka oleh `accept` permintaan dan ditutup oleh ringkasan akhir. Tidak ada `options`, `PATCH status`, maupun `DELETE`. |
| QBE yang berlaku | QBE-NAM, QBE-SVC-001 (controller tanpa `ApplicationDbContext`), QBE-API-001, standar hak akses (konstanta `RehabilitationPermissions.Episode`) |
| Lintas modul | Baca saja: `MstPatient`, `MstDoctor`, `RegPatientEncounter`. `AccessPermissionService.GetEffectivePermissionsAsync` dipakai untuk menyaring aksi. |

## 1. Masalah yang diperbaiki

Setelah permintaan diterima, tidak ada layar tempat dokter dan terapis melihat episode mana yang
menunggu asesmen, mana yang programnya perlu ditinjau, dan apa yang boleh dikerjakan berikutnya.

**Contoh.** dr. Sp.KFR membuka worklist, menyaring *Menunggu asesmen medis*, lalu membuka
`RHB-EPS-2026-000001`. Rincian menampilkan identitas pasien, permintaan asalnya, layanan yang
diminta, jumlah dokumen per jenis, dan tombol yang boleh ia pakai: *Asesmen medis*, *Edukasi dan
persetujuan*, *Ringkasan akhir*. Petugas yang hanya boleh melihat episode mendapat rincian yang sama,
tetapi tanpa tombol.

## 2. Proses bisnis

1. Worklist menyaring episode menurut periode dibuka, status, layanan (diminta atau diresepkan),
   dokter penanggung jawab, *perlu ditinjau*, *evaluasi jatuh tempo*, dan pencarian nomor
   episode/permintaan/pasien.
2. Rincian episode menyatukan pasien (baca saja), permintaan asal, layanan beserta status program
   aktifnya, ringkasan jumlah dokumen, dan aksi yang tersedia.
3. Aksi = sah menurut status episode (`state-transition-matrix.md` §2–5) **dan** pengguna memegang
   butir hak aksesnya. Kewenangan yang melekat pada data (profesi, penulis) tetap diperiksa endpoint
   aksinya masing-masing.
4. Timeline menampilkan asesmen → program → edukasi → sesi → reasesmen → ringkasan akhir menurut
   waktu, tanpa isi klinis. Jenis yang belum punya data cukup tidak muncul.
5. Riwayat status episode dibaca dari `RhbStatusHistory`.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Status | Isi |
| --- | --- | --- |
| `Services/RhbEpisodeService.cs` | Diperbarui (645 baris) | Pembukaan dari `BE-RHB-007` dipertahankan apa adanya. Ditambah metadata, ringkasan, worklist, rincian, timeline, riwayat, dan aturan aksi. Konstruktor kini menerima `AccessPermissionService`. |
| `DTOs/RehabEpisodeDtos.cs` | Baru (183 baris) | `RehabEpisodeQuery`, `RehabEpisodeListItem`, `RehabEpisodeSummaryResponse`, `RehabEpisodeFilterMetadataResponse`, `RehabEpisodeDetailResponse` (+ pasien, permintaan, layanan, ringkasan dokumen, aksi), `RehabTimelineItem` |
| `Controllers/RehabEpisodeController.cs` | Baru (99 baris) | 6 endpoint `GET` |
| `Services/RhbServiceSupport.cs` | Diperbarui | `TodayLocal()` (tanggal rumah sakit, untuk *evaluasi jatuh tempo*) |

`Program.cs` tidak berubah, karena `RhbEpisodeService` sudah terdaftar di `BE-RHB-007`.

### 3.2 Aturan aksi episode

| Aksi | Butir hak akses | Status episode yang mengizinkan | Syarat tambahan |
| --- | --- | --- | --- |
| `create-medical-assessment` | `RehabMedicalAssessment : Create` | Menunggu asesmen medis, Perencanaan, Terapi berjalan | Belum ada asesmen medis draf/final |
| `create-discipline-assessment` | `RehabDisciplineAssessment : Create` | Perencanaan, Terapi berjalan | — |
| `create-goal` | `RehabGoal : Create` | Perencanaan, Terapi berjalan | — |
| `create-program` | `RehabProgram : Create` | Perencanaan, Terapi berjalan | — |
| `create-education` | `RehabEducation : Create` | Menunggu asesmen medis, Perencanaan, Terapi berjalan | — |
| `start-session` | `RehabSession : Create` | Terapi berjalan | — |
| `create-progress-review` | `RehabProgressReview : Create` | Perencanaan, Terapi berjalan | — |
| `edit-discharge-summary` | `RehabDischargeSummary : Update` | Menunggu asesmen medis, Perencanaan, Terapi berjalan | Ringkasan akhir belum final |

Episode `Ditutup` tidak punya aksi. Endpoint aksi-aksi ini dibangun di `BE-RHB-009`..`015`; daftar
ini memberi tahu layar tombol mana yang ditampilkan.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Grup baru *Rehab Episode*, 6 endpoint sesuai kontrak v1 |
| Database | Tidak ada perubahan schema maupun migration. Hanya baca. |
| Keamanan/Auth | Resource `RehabEpisode`, aksi `Read`. Aksi pada rincian disaring dengan sumber izin yang sama dengan penjaga endpoint. |
| Privasi | Daftar dan timeline tanpa kolom sensitif. Rincian memuat indikasi permintaan dan kewaspadaan medis untuk ruang kerja klinis. |

## 4. Dokumentasi endpoint

**[Tags("Health Services / Rehabilitation Management / Rehab Episode")]** — base URL
`/api/v1/health-services/rehabilitation-management/rehab-episodes`. Hak akses semua endpoint:
`RehabEpisode : Read`.

| Method | Path | Request | Response | Kode status |
| --- | --- | --- | --- | --- |
| GET | `/filters/metadata` | — | Periode, status (4), layanan, dokter penanggung jawab yang punya episode, urutan, ukuran halaman | 200 |
| GET | `/summary` | `RehabEpisodeQuery` | Jumlah per status + perlu ditinjau + evaluasi jatuh tempo | 200, 400 (`076`) |
| GET | `/` | `startDate`, `endDate`, `customPeriod`, `status`, `disciplineId`, `responsibleDoctorId`, `needsReview`, `evaluationDue`, `search`, `sortBy` (`openedAt`/`episodeNumber`/`episodeStatus`), `sortDirection`, `pageNumber`, `pageSize` (maks. 100) | `PagedResult<RehabEpisodeListItem>` | 200, 400 (`076`) |
| GET | `/{id}` | — | `RehabEpisodeDetailResponse` | 200, 404 |
| GET | `/{id}/timeline` | — | `List<RehabTimelineItem>` | 200, 404 |
| GET | `/{id}/status-history` | — | `List<RehabStatusHistoryItem>` | 200, 404 |

Contoh potongan rincian (SuperAdmin, episode *Menunggu asesmen medis*):

```json
{
  "episodeNumber": "RHB-EPS-2026-000001",
  "episodeStatus": 1,
  "episodeStatusName": "Menunggu asesmen medis",
  "request": { "requestNumber": "RHB-REQ-2026-000001", "sourceTypeName": "Rawat jalan" },
  "disciplines": [ { "disciplineName": "Uji BE-RHB-007", "isRequested": true, "isPrescribed": false } ],
  "documents": { "medicalAssessmentDraft": 0, "medicalAssessmentFinal": 0, "dischargeSummaryStatus": null },
  "availableActions": [
    { "action": "create-medical-assessment", "label": "Asesmen medis", "resource": "RehabMedicalAssessment", "permission": "Create" },
    { "action": "create-education", "label": "Edukasi dan persetujuan", "resource": "RehabEducation", "permission": "Create" },
    { "action": "edit-discharge-summary", "label": "Ringkasan akhir", "resource": "RehabDischargeSummary", "permission": "Update" }
  ]
}
```

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE conformance `-Mode Strict` (4 berkas) | 0 violation untuk setiap berkas | `PASS` | Dijalankan agent 2026-11-05 |
| `dotnet build` Debug, perintah owner | `0 Error(s)`, 426 warning, 0 warning Rehab, `EXIT=0`, 7 menit 51 detik (16:12:29–16:20:22) | `PASS` | Log `build-BE-RHB-008.log` |
| Pendaftaran hak akses | `SysActionAccess` `RehabEpisode : Read` terdaftar saat startup | `PASS` | Query DB |
| Uji API manual (backend dev, data dari `BE-RHB-007`: 3 episode) | 28/28 | `PASS` | Tabel di bawah |

Hasil uji API (2026-11-05):

| Pemeriksaan | Hasil |
| --- | --- |
| Tanpa login | 401 |
| `pageSize=500` | 200, `pageSize` = **100** |
| Kunci baris daftar | 17 kunci identitas/status/tanggal; **tidak ada** `indicationText`, nama/nomor rujukan eksternal, alasan, maupun `medicalPrecautionText` |
| `pageSize=1&pageNumber=2`; `pageSize=0` | 1 baris, `totalPage` = `totalData` = 3; bawaan 25 |
| Filter status 1 / status 4 | 3 episode, semuanya status 1 / kosong |
| Filter layanan yang diminta / layanan acak | 3 / 0 |
| Cari nomor episode / nomor permintaan | Masing-masing 1 baris, episode yang tepat |
| `needsReview=true`, `evaluationDue=true` | 0 (belum ada program) |
| Rentang tanggal terbalik | 400 `RHB-VAL-076` |
| Urut nomor episode naik | `000001`, `000002`, `000004` |
| Ringkasan | `total 3, awaitingMedicalAssessment 3, planning 0, active 0, closed 0, needsReview 0, evaluationDue 0` |
| Metadata | 4 status, 1 dokter penanggung jawab |
| Rincian `RHB-EPS-2026-000001` | Pasien, permintaan `RHB-REQ-2026-000001` (Rawat jalan), 1 layanan (diminta, belum diresepkan), ringkasan dokumen semuanya nol |
| **(2)** Aksi untuk SuperAdmin | `create-medical-assessment`, `create-education`, `edit-discharge-summary` |
| **(2)** Aksi untuk akun uji dengan `RehabEpisode : Read` saja | Rincian 200, **0 aksi** |
| Akun uji: daftar episode / `rehab-requests` | 200 / 403 |
| Rincian, timeline: id acak | 404, 404 |
| **(3)** Timeline episode tanpa dokumen | 200, `[]` |
| Riwayat status | 1 baris `open` → `AwaitingMedicalAssessment` |
| Akun uji sesudah dinonaktifkan | 403 |

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Daftar tanpa kolom sensitif dan `pageSize` maksimum 100 | Terpenuhi (runtime) | §5 |
| (2) Rincian menyertakan aksi yang tersedia menurut status dan hak pengguna | Terpenuhi. Penyaringan hak terbukti di runtime: SuperAdmin 3 aksi, pengguna `Read` saja 0 aksi. Penyaringan status terbukti di runtime untuk *Menunggu asesmen medis*; status lain terbukti lewat kode (tabel §3.2), karena episode `Planning`/`Active`/`Closed` baru ada setelah `BE-RHB-009`/`011`/`015`. | §3.2, §5 |
| (3) Timeline kronologis; jenis dokumen yang belum ada slice-nya tampil kosong, bukan galat | Terpenuhi. Daftar kosong 200 terbukti di runtime. Urutan kronologis terbukti lewat kode (`OrderBy(OccurredAt)`, lalu urutan jenis asesmen → program → edukasi → sesi → reasesmen → ringkasan) karena belum ada dokumen di DB dev; pembuktian runtime-nya terjadi di `BE-RHB-009`. | `RhbEpisodeService.GetTimelineAsync` |
| Verifikasi kartu: API manual | Terpenuhi | §5 |
| DoD: kriteria, laporan | Terpenuhi | Berkas ini |

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Delta kontrak | (a) Bentuk DTO tidak dirinci kontrak (`DEV_DISCRETION`); tercatat di §3.1 dan §4. (b) Nama aksi dan aturannya (§3.2) diturunkan dari `state-transition-matrix.md` §2–5; endpoint-nya menyusul di task berikutnya. (c) Waktu timeline: asesmen = `AssessedAt`, program = `ActivatedAt` (atau waktu dibuat), edukasi = `EducatedAt`, sesi = `StartedAt` (atau waktu dibuat), reasesmen = `ReviewedAt`, ringkasan = `SignedAt` (atau waktu dibuat). Dokumen yang dibatalkan tetap tampil dengan status *Dibatalkan*. (d) *Evaluasi jatuh tempo* = program aktif dengan `EvaluationDueDate` ≤ hari ini menurut zona waktu rumah sakit. |
| Peringatan | Aksi hanya memeriksa butir hak akses, belum profesi. Contohnya, terapis wicara yang memegang `RehabProgram : Create` tetap melihat tombol *Program terapi*, lalu ditolak 403 `RHB-VAL-031` oleh endpoint program bila profesinya tidak cocok dengan disiplin. Ini sengaja: tombol bersifat episode, sedangkan profesi melekat pada disiplin. |
| Data uji di `QuilvianNewDevSukma` | Tidak ada data rehab baru. Satu akun uji tambahan `rhb-uji-adccb3b6@example.test` **sudah dinonaktifkan**; password awalnya tidak pernah dicetak atau disimpan. Kebijakan *Pendaftaran / Admission Officer* **dikembalikan ke kosong** (0 kebijakan). |
| Perubahan sampingan | `NONE`. Backend sudah dimatikan; port 7184 kosong. |
| Interupsi | `NONE` |
| Status Git | `M Services/RhbEpisodeService.cs` dan `M Services/RhbServiceSupport.cs` (keduanya masih untracked sejak task sebelumnya), `??` 2 berkas baru (DTO, controller) |
| Langkah berikutnya | `BE-RHB-009` dan `BE-RHB-010` masih ⛔ karena juga menunggu `BE-RHB-004` (bukti persetujuan RM, `RHB-OQ-012`). `FE-RHB-004` menunggu `FE-RHB-003`. |
