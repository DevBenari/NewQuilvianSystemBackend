# Laporan Perubahan Backend — `BE-RHB-009`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RHB-009` |
| Judul | Asesmen medis dan profesi (draft, ubah, tanda tangan, batal; pengukuran; resep layanan) |
| Slice | S-C — Asesmen, edukasi, program |
| Roadmap | `docs/module-blueprints/rehab-medik/roadmap/backend-roadmap.md` — kartu `BE-RHB-009` |
| Trace | FR-RHB-011..016; FEAT-007..010; RHB-DEC-005, 006, 017; `state-transition-matrix.md` §2–3; `validation-matrix.md` V-ASM-01..05, V-MEA-01 |
| Contract version | Set kontrak `v1`, `approved` 2026-11-05 — grup API *Rehab Medical Assessment* dan *Rehab Discipline Assessment* |
| Dependency | `BE-RHB-004` ✅ (2026-11-06), `BE-RHB-008` ✅ |
| Klasifikasi | `STANDARD` — endpoint transaksi baru, tanpa schema baru, memakai mesin keutuhan RM dan mesin instrumen |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/HealthServices/RehabilitationManagement/{Controllers,Services,DTOs,Constants}`, `Program.cs`, dokumen blueprint |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `6bb3fba7` (`sukmagp`), belum di-commit |
| Tanggal | 2026-11-06 |
| Status | **✅ Selesai** (2026-11-06). Build owner `PASS` (0 error, 0 warning di folder Rehab), QBE Strict `PASS`. Uji API manual: bagian asesmen medis 27/27, bagian asesmen profesi 12/12, jalur instrumen 6/6. Bukti DB cocok (§6). |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `RehabilitationManagement` (`Rhb`, `ACTIVE`) |
| Applicability | `NEW CODE` |
| Registry | `Rhb` `ACTIVE`. Tidak ada entity atau tabel baru; `RhbAssessment`, `RhbMeasurement`, `RhbEpisodeDiscipline` dari `BE-RHB-002`. |
| Jenis capability | **Transaksi — sub-proses ter-scope induk (episode).** Buat lewat `POST /by-episode/{episodeId}`, ubah `PUT /{id}`, perpindahan status `POST /{id}/sign` dan `POST /{id}/cancel`. Tidak ada `options`, `PATCH status`, maupun `DELETE`. |
| QBE yang berlaku | QBE-NAM, QBE-SVC-001 (controller tanpa `ApplicationDbContext`), QBE-API-001, standar hak akses (konstanta `RehabilitationPermissions`), aturan transaksi gateway RM |
| Lintas modul | Tulis lewat gateway: `MrcClinicalDocumentIntegrity` (jenis 47) melalui `RhbDocumentIntegrityGateway`, dengan persetujuan RM `RHB-OQ-012` ✅. Baca saja: `CliClinicalInstrument(Version)`, `RegPatientEncounter`, `InpEpisode`, data profesi akun. |

## 1. Masalah yang diperbaiki

Episode yang dibuka dari permintaan berhenti di *Menunggu asesmen medis*: belum ada cara bagi
dr. Sp.KFR untuk mengasesmen pasien dan meresepkan layanan terapi. Terapis juga belum bisa
membuat asesmen profesinya.

**Contoh.** dr. Sp.KFR membuka episode `RHB-EPS-2026-000001`, mengisi riwayat, temuan, masalah,
kesimpulan, dan pengukuran ROM bahu 90°, lalu meresepkan dua layanan. Draft itu ia simpan. Esoknya
nilai ROM ia perbaiki menjadi 95°, lalu ia menandatangani. Hasilnya:
- episode pindah ke *Perencanaan*;
- kedua layanan tercatat diresepkan;
- dokumen dikunci Rekam Medis.

Saat ia mencoba mengubah isi, permintaan ditolak dan ia diarahkan ke addendum. Terapis yang
profesinya sesuai kemudian membuat asesmen profesinya. Terapis dengan profesi lain ditolak.

## 2. Proses bisnis

1. **Buat draft** (`Create`).
   - Episode harus belum ditutup.
   - Kunjungan wajib milik pasien episode, masih terbuka, dan (untuk rawat inap) pasien belum pulang.
   - Draft didaftarkan ke Rekam Medis berstatus *Draf* dalam transaksi yang sama.
   - Asesmen medis: maksimal satu yang belum dibatalkan per episode. Draft kedua dijawab 409, dan asesmen sesudah final dijawab 409 `RHB-VAL-033`.
   - Asesmen profesi: layanan harus sudah diresepkan asesmen medis final (`RHB-VAL-032`), dan profesi akun harus sama dengan profesi layanan (`RHB-VAL-031`).
2. **Ubah draft** (`Update`).
   - Hanya penulis (`RHB-VAL-022`), dengan `expectedVersion` (`RHB-VAL-010`), dan selama Rekam Medis masih mengizinkan (`EnsureMutableAsync`).
   - Daftar pengukuran dan resep diganti utuh. Kunjungan dan layanan tidak dapat diganti.
3. **Tanda tangan** (`Sign`).
   - Riwayat, temuan, masalah/diagnosis, dan kesimpulan wajib terisi (`RHB-VAL-034`).
   - Asesmen medis wajib punya minimal satu resep layanan aktif (`RHB-VAL-030`).
   - Pengukuran berinstrumen harus lengkap, dan versinya masih boleh dipakai (`RHB-VAL-035`).
   - Dalam satu transaksi: status *Final*, episode *Menunggu asesmen medis* → *Perencanaan* beserta riwayatnya, dan tanda tangan Rekam Medis (perangkat dan IP dari permintaan HTTP).
4. **Batal draft** (`Update`). Alasan wajib (`RHB-VAL-008`). Rekam Medis menandai dokumen *Dibatalkan*, dan resep draft dihapus lunak.
5. **Sesudah final**, ubah dan batal dijawab 409 `RHB-VAL-021`. Koreksi lewat addendum Rekam Medis jenis 47, dan catatan asli tetap utuh.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang berubah

| Berkas | Status | Isi |
| --- | --- | --- |
| `Controllers/RehabMedicalAssessmentController.cs` | Baru | 6 endpoint, Resource `RehabMedicalAssessment`, aksi `Read`/`Create`/`Update`/`Sign` |
| `Controllers/RehabDisciplineAssessmentController.cs` | Baru | 6 endpoint, Resource `RehabDisciplineAssessment`, bentuk sama |
| `Services/RhbAssessmentService.cs` | Baru | Seluruh aturan §2. Validasi pengukuran memakai `ClinicalInstrumentDefinitionEngine` (`Score`, `MissingRequiredItems`) dan izin konsep `ClinicalInstrumentService.IsDraftAllowedInThisEnvironment`. |
| `DTOs/RehabAssessmentDtos.cs` | Baru | `SaveRehabAssessmentRequest`, `RehabMeasurementRequest`, `RehabPrescribedDisciplineRequest`, `RehabSignRequest`, `RehabAssessmentListItem`, `RehabAssessmentDetailResponse`, `RehabMeasurementItem`, `RehabPrescribedDisciplineItem` |
| `Constants/RhbErrorCodes.cs` | Diperbarui | Delta: `RHB-VAL-081` kunjungan asesmen tidak sah, `RHB-VAL-082` pengukuran terstruktur tidak lengkap |
| `Services/RhbEpisodeService.cs` | Diperbarui | Resep dihitung hanya dari asesmen medis **final**: rincian layanan, saringan layanan, dan nama layanan daftar. `RecordStatusName` dibuka `public`. |
| `Services/RhbRequestService.cs` | Diperbarui | Cek duplikasi permintaan memakai resep final saja |
| `Program.cs` | Diperbarui | `AddScoped<RhbAssessmentService>()` |

### 3.2 Dampak kontrak, database, dan keamanan

| Hal | Isi |
| --- | --- |
| Schema / migration | Tidak ada |
| Hak akses | Dua controller baru muncul di Akses Role dengan aksi Read, Create, Update, dan Sign. Sign bertipe `Update`, terpisah agar bisa diberikan hanya kepada Sp.KFR (RHB-DEC-005). Tidak ada nama peran di kode. Profesi dan kepenulisan adalah kewenangan yang melekat pada data. |
| Logger | Hanya id, episode, status, dan versi. Isi klinis tidak dicatat. |
| Rekam Medis | Hanya lewat gateway. Tanda tangan dibungkus transaksi karena `SignAsync` RM menyimpan sendiri. |

### 3.3 Delta terhadap kontrak dan desain

| Delta | Rincian |
| --- | --- |
| Penyimpanan resep draft | Desain hanya menyebut `RhbEpisodeDiscipline` "dibentuk saat tanda tangan", padahal kontrak `POST`/`PUT` draft memuat `prescribedDisciplines`. Resep draft disimpan sebagai baris `RhbEpisodeDiscipline` yang tertaut ke asesmen draft, lalu baru dihitung "diresepkan" setelah asesmennya final. Ini aman terhadap indeks unik `(EpisodeId, DisciplineId)` karena episode hanya boleh punya satu asesmen medis yang belum batal. Tiga pembaca lama sudah disaring. |
| 409 `RHB-VAL-020` untuk draft kedua | Asesmen medis kedua saat draft masih ada dijawab 409 `RHB-VAL-020`. Tujuannya mencegah dua draft bersaing; tidak ada baris matriks khusus untuk ini. |
| Kunjungan dan layanan tidak dapat diganti | Baris keutuhan RM terikat ke kunjungan saat draft dibuat. Mengganti kunjungan dijawab 422 `RHB-VAL-081` dengan saran membatalkan lalu membuat draft baru. |
| Kode `RHB-VAL-081`/`082` | Kunjungan tidak sah (422) dan pengukuran terstruktur tanpa nama atau nilai (400) belum punya baris di matriks v1 |
| Instrumen wajib terpasang | Pengukuran berinstrumen hanya untuk jenis yang dipasang konfigurasi layanan, atau konfigurasi asesmen medis (`RhbDisciplineInstrument`). Selain itu 422 `RHB-VAL-035`. |
| Jawaban instrumen draft | Boleh belum lengkap saat draft. Kelengkapan (isian berskor dan isian wajib) dituntut saat tanda tangan. |
| `availableActions` | Rincian memuat `update`/`sign`/`cancel` untuk penulis draft, dan `addendum` setelah final atau terkunci |
| Jumlah asesmen profesi | Tidak dibatasi satu per layanan karena aturan itu tidak ada di matriks. Dicatat sebagai pertanyaan untuk owner bila perlu. |

## 4. Validasi statis

| Pemeriksaan | Hasil |
| --- | --- |
| `dotnet build .\QuilvianSystemBackend.csproj --configuration Debug -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RunAnalyzers=false` (perintah owner) | `PASS`, exit 0, 0 error. Tidak ada warning di folder `RehabilitationManagement`; warning yang ada milik berkas lain (misalnya `AccessPermissionFilter.cs` CS1573). Build pertama terputus karena sesi berakhir, lalu diulang dan selesai. |
| QBE Strict | `PASS`. Controller tanpa `ApplicationDbContext`; pasangan `[AccessAction]`/`[AccessPermission]` identik untuk keempat aksi di kedua controller; grep `IsInRole`/`UserType`/nama peran/departemen/jabatan = 0. |
| Review diff | 4 berkas baru, 4 berkas lama diubah (§3.1); tidak ada berkas modul lain yang ditulis |

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

## 5. Uji API manual

**Lingkungan.**
- Backend Debug di `https://localhost:7184`, DB dev `QuilvianNewDevSukma`, 2026-11-06.
- Skrip Playwright `request` dari scratchpad; kredensial dibaca di dalam skrip.
- Data uji:
  - episode `RHB-EPS-2026-000001` (EP1) dan `…000002` (EP2);
  - disiplin uji A `RHB_UJI_10722EA2` (profesi Dokter Umum), diaktifkan sementara;
  - disiplin uji B `RHB_UJI_B9C5AB4` (Perawat), dibuat untuk uji ini.
- Disiplin seed belum punya profesi dan nonaktif, jadi tidak dipakai.

| # | Pemeriksaan | Hasil |
| --- | --- | --- |
| 1 | `GET by-episode` sebelum ada asesmen → 200 `[]` | PASS |
| 2 | Buat tanpa kunjungan → 422 `RHB-VAL-081` | PASS |
| 3 | **AC5**: instrumen *Monitoring Nyeri* versi konsep, lingkungan tanpa izin konsep → 422 `RHB-VAL-035` | PASS |
| 4 | Pengukuran terstruktur tanpa nilai → 400 `RHB-VAL-082` | PASS |
| 5 | Buat draft (ROM 90°, resep A + B) → 201, versi 0, keutuhan *Draf*, aksi `update/sign/cancel`; 1 pengukuran + 2 resep | PASS ×2 |
| 6 | Asesmen medis kedua saat draft ada → 409 `RHB-VAL-020` | PASS |
| 7 | Resep draft belum dihitung diresepkan di rincian episode (0) | PASS |
| 8 | Versi basi → 409 `RHB-VAL-010` | PASS |
| 9 | Tanda tangan isian belum lengkap → 422 `RHB-VAL-034`, `fields` menunjuk 3 isian kosong | PASS |
| 10 | Resep dikosongkan → 200 v1; tanda tangan → 422 `RHB-VAL-030` (V-ASM-01) | PASS ×2 |
| 11 | Ubah lengkap + resep dua layanan (ROM 95°) → 200 v2. Resep diganti tanpa bentrok indeks unik. | PASS |
| 12 | **UAT-04**: tanda tangan → 200 *Final*, keutuhan *Ditandatangani*, aksi `addendum`; episode *Perencanaan*; dua layanan diresepkan; riwayat `sign-medical-assessment` | PASS ×3 |
| 13 | **UAT-05 / AC2**: ubah dan batal sesudah final → 409 `RHB-VAL-021`; addendum RM jenis 47 → 201 dan terbaca; teks asli tetap | PASS ×4 |
| 14 | **AC4**: asesmen medis sesudah final → 409 `RHB-VAL-033` | PASS |
| 15 | **AC3**: SuperAdmin (tanpa profesi) membuat asesmen profesi → 403 `RHB-VAL-031` | PASS |
| 16 | Layanan belum diresepkan di EP2 → 422 `RHB-VAL-032` (V-ASM-03) | PASS |
| 17 | Controller akses `RehabDisciplineAssessment` terdaftar dengan Read/Create/Update/Sign | PASS |
| 18 | Akun uji (profesi Dokter Umum, butir asesmen profesi saja) login | FAIL di run pertama, PASS di run lanjutan (lihat bawah) |
| 19 | **AC3**: profesi Dokter Umum untuk layanan berprofesi Perawat → 403 `RHB-VAL-031` | PASS |
| 20 | Profesi cocok → 201; bukan penulis mengubah → 403 `RHB-VAL-022`; dibaca lewat rute asesmen medis → 404; tanda tangan → 200 *Final* | PASS ×4 |
| 21 | Batal tanpa alasan → 422 `RHB-VAL-008`; dengan alasan → 200 *Dibatalkan* + keutuhan *Dibatalkan*; daftar → 2 baris | PASS ×3 |
| 22 | Akun tanpa butir `RehabMedicalAssessment : Read` → 403 | PASS |
| 23 | Izin konsep menyala (`ClinicalConfiguration__AllowDraftVersionsForTesting=true`, hanya run ini): lookup instrumen tersedia; kode isian asing → 422 `RHB-VAL-035` beserta rincian; draft berinstrumen → 201 jenis 3; tanda tangan → 422; draft dibatalkan; EP2 tetap *Menunggu asesmen medis* | PASS ×6 |

**Run pertama** menghasilkan 28/37. Ke-27 pemeriksaan SuperAdmin lulus, tetapi login akun uji
dijawab 403 "Akurasi lokasi tidak terbaca". Skrip belum mengirim `accuracyMeters` bersama
koordinat geofence, sehingga 9 pemeriksaan sesudahnya gagal sebagai akibat.

Bagian profesi lalu diulang dengan skrip lanjutan (EP1 sudah berisi asesmen medis final, jadi run
penuh tidak dapat diulang apa adanya). Hasilnya **12/12**. Tidak ada perubahan source di antara
kedua run.

## 6. Bukti database (kueri READ ONLY)

| Data | Hasil |
| --- | --- |
| `RhbAssessment` + `MrcClinicalDocumentIntegrity` jenis 47 | Medis: *Final* v3, keutuhan *Signed*, pemicu kunci tanda tangan penulis, IP tercatat. Profesi: *Final* (keutuhan *Signed*) dan *Dibatalkan* (keutuhan *Cancelled*). Draft instrumen di EP2: *Dibatalkan*. |
| `RhbEpisodeDiscipline` EP1 | 2 aktif (resep final) + 2 terhapus lunak (resep draft yang diganti) |
| `RhbMeasurement` | ROM 90° terhapus lunak, ROM 95° aktif, jarak tempuh 320 m aktif |
| `RhbEpisode` EP1 | Status 2 (*Planning*), versi 1 |
| `RhbStatusHistory` EP1 | `open` → `sign-medical-assessment` (*AwaitingMedicalAssessment* → *Planning*) |
| `MrcClinicalNoteAddendum` jenis 47 | 1 |

**Pembersihan.**
- 0 dari 19 akun `rhb-uji-*` yang masih aktif;
- kebijakan aktif pasangan uji: 0 baris;
- semua disiplin `RHB_UJI*` nonaktif;
- backend dihentikan.

Data asesmen uji di EP1 dibiarkan sebagai bukti. Data itu tercatat di dokumen klinis dan tidak
dapat dihapus (addendum dan tanda tangan bersifat permanen).

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) UAT-04 | Terpenuhi | §5 #12, §6 |
| (2) UAT-05 (409 + addendum RM jenis 47) | Terpenuhi | §5 #13, §6 |
| (3) Profesi tidak cocok → 403 `RHB-VAL-031` | Terpenuhi | §5 #15, #19 |
| (4) Asesmen medis final kedua → 409 `RHB-VAL-033` | Terpenuhi | §5 #14 |
| (5) Instrumen konsep di non-dev → 422 `RHB-VAL-035` | Terpenuhi | §5 #3; jalur izin konsep §5 #23 |
| DoD: build owner | Terpenuhi | §4 |
| DoD: laporan | Terpenuhi | Berkas ini |

## 8. Risiko dan tindak lanjut

| Butir | Rincian |
| --- | --- |
| Disiplin seed belum siap | Keempat disiplin seed nonaktif dan belum punya profesi; data master dev belum punya profesi fisioterapis/okupasi/wicara. Asesmen profesi baru bisa dipakai setelah admin melengkapi konfigurasi layanan (`FE-RHB-002`). |
| Instrumen rehab jenis 8–13 | Belum ada definisi di mesin instrumen. Sampai ada, pengukuran rehab memakai jalur terstruktur (RHB-DEC-017). |
| Draft terkunci kunjungan | Bila kunjungan ditutup sebelum tanda tangan, Rekam Medis mengunci draft (*LockedUnsigned*). Ubah dijawab 409 `RHB-VAL-021`, dan penyelesaiannya lewat addendum atestasi RM. Perilaku ini mengikuti mesin RM dan belum diuji di sini. |
| Task berikutnya | `BE-RHB-010` (edukasi) 🟡 sekarang; `BE-RHB-011` menunggu `BE-RHB-009` ✅ + `BE-RHB-010`; `FE-RHB-005` kini hanya menunggu jadwal |
