# Laporan Perubahan Backend — `BE-RWI-230`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-230` |
| Judul | Endpoint kelayakan admisi untuk langkah 1, dan penolakan pasien yang sedang hadir sejak admisi dibuka |
| Slice | Perbaikan `PLAN-REPAIR-EPS-010` — `FIX-EPS-010-01` |
| Roadmap | [`backend-roadmap-admisi-pendaftaran.md`](../../../roadmap/backend-roadmap-admisi-pendaftaran.md) bagian 3 dan 4.5 |
| Trace | `ISS-EPS-010-01`, `-02`, `-03`, `-T1`; `RWI-DEC-054` (`INV-INP-10`), `RWI-DEC-297`; `VAL-RWF-87`/`88`; RJ-DOC-REV-BE-007 (kunjungan aktif) |
| Contract version | API `0.11.0` `approved` + amandemen `PLAN-REPAIR-EPS-010` (delta di bagian 3.3) |
| Dependency | Tidak ada |
| Klasifikasi | `MEDIUM` — satu endpoint baca baru, satu penjaga pada jalur tulis yang sudah ada, pemindahan aturan lintas modul tanpa perubahan perilaku; tanpa database |
| Task mode | `BACKEND` (cross-repo: frontend dikerjakan terpisah sebagai `FE-RWI-244`) |
| Target tulis | `NewQuilvianSystemBackend`: enam berkas pada bagian 3.2; laporan dan register blueprint |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `8fce8c966803d21d763faa31de4ff3c049d557cf` (`MHamzah`) |
| Tanggal | 13 Oktober 2026 |
| Status | ✅ Selesai (source + build + QBE). Uji API runtime `NOT RUN` |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `InPatientManagement` (utama); `HealthServices` / `RegistrationManagement` (aturan kunjungan aktif dipakai bersama) |
| Prefix registry | `Inp` — `ACTIVE`; `Reg` — `ACTIVE` (`MODULE_OWNERSHIP_PREFIX_REGISTRY.md`) |
| Keberlakuan | `NEW CODE` (`InpEpisodeService.AdmissionEligibility.cs`, `InpatientAdmissionEligibilityDtos.cs`, action controller); `TOUCHED LEGACY` (`InpEpisodeService.cs`, `PatientEncounterController.cs`, `OutpatientEncounterRules.cs`) |
| QBE yang berlaku | `QBE-SVC-001` (logika di service, controller tanpa context), `QBE-API-001` (`ApiResponse<T>`, 400/404), `QBE-PERM-001` (`[AccessAction]`+`[AccessPermission]` `InpatientEpisode : Read`), `QBE-DTO-001` (response DTO, bukan entity), `QBE-VAL-001` (`patientId` kosong, pasien nonaktif, `INV-INP-10`), `QBE-TXN-001` (penolakan sebelum transaksi admisi dibuka) |
| Entity baru | Tidak ada — `QBE-ENT/CFG/MOD/NAM` tidak berlaku |
| Wewenang | Source: ya. Migration, eksekusi database, deployment: tidak |

---

## 1. Masalah yang diperbaiki

Langkah 1 admisi Pasien Lama hanya memeriksa "sudah ada pasien terpilih". Tiga aturan yang pasti
menolak pasien baru ditegakkan server lima langkah kemudian:

| Aturan | Ditegakkan di mana sebelum task ini |
| --- | --- |
| `INV-INP-10` — pasien yang sedang hadir | `POST /bed-occupancies/reservations` dan penempatan (`InpBedOccupancyService.cs:363`, `:612`) |
| Permintaan Kamar Pulih menunggu (`VAL-RWF-87`) | `POST /episodes` (`OpenAdmissionAsync`) |
| Kunjungan rawat jalan aktif | `POST /patient-encounters/admin` (`PatientEncounterController`) |

Akibat terburuknya: untuk pasien yang sedang dirawat, `POST /patient-encounters/admin` dan
`POST /episodes` berhasil lebih dulu, lalu pemesanan bed ditolak — kunjungan rawat inap kedua dan
episode `Draft` kedua tertinggal sebagai admisi ganda "Sedang Disiapkan".

---

## 2. Proses bisnis

| Butir | Isi |
| --- | --- |
| Tujuan | Petugas admisi tahu di langkah 1 apakah pasien boleh dibukakan admisi baru, dengan alasan dan jalan keluarnya |
| Pelaku | Petugas admisi (hak `InpatientEpisode : Read` untuk memeriksa, `Create` untuk membuka admisi) |
| Pemicu | Pasien dipilih di langkah 1, tombol Lanjut ditekan, dan sekali lagi sebelum kunjungan dibuat di langkah Pilih Bed |
| Langkah | (1) Pasien harus ada dan aktif. (2) Episode hadir → penghalang `PATIENT_ALREADY_ADMITTED`. (3) Permintaan Kamar Pulih → `INP-ADM-REF-001`/`002` (kecuali permintaan yang sedang dipakai jalur Kamar Pulih). (4) Saklar `BlockActiveEncounter` hidup dan ada kunjungan rawat jalan berklinik status Draft–Sedang Konsultasi → `ACTIVE_ENCOUNTER_EXISTS`. (5) Draft lain: Draft lewat batas digugurkan dulu; yang masih berlaku dilaporkan sebagai peringatan |
| Jalur tidak normal | `patientId` kosong → 400; pasien tidak ada/nonaktif → 404; tidak berhak → 403 dari middleware hak akses |
| Hasil akhir | `canContinue`, penghalang pertama (`blockingCode`, `message`) beserta seluruh penghalang, rincian episode hadir/kunjungan aktif/Draft, dan peringatan |

**Contoh.** Tn. Umar (pseudonim) dirawat di Kamar 02 BED 001 sejak 6 Oktober dan juga masih punya
kunjungan poli 30 Juli yang belum selesai. Jawaban: `canContinue=false`,
`blockingCode=PATIENT_ALREADY_ADMITTED`, `message` "Tn. Umar sudah dirawat pada episode RI-… di
Kamar 02 BED 001. Bila memang pindah kamar, pakai perpindahan, bukan admisi baru.", `blockers` berisi
dua butir (episode hadir, lalu kunjungan aktif). Bila petugas tetap memanggil `POST /episodes`,
server menolak 409 `PATIENT_ALREADY_ADMITTED` tanpa menulis apa pun.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`InpEpisodeService.cs` (`OpenAdmissionAsync`, `ExpireDraftIfDueAsync`, `LoadEpisodeForWriteAsync`,
`InpEpisodeOperationResult`), `InpEpisodeService.Assignments.cs` (`FindPresentEpisodeAsync`,
`InpPresentEpisodeInfo`), `InpEpisodeService.Reads.cs` (`ExpireDueDraftEpisodesAsync`),
`InpAdmissionReferralService.cs` (`CheckForAdmissionAsync`, `AdmissionReferralCheck`),
`InpBedOccupancyService.cs` (penjaga `INV-INP-10` lama), `InpAdmissionTransferService*.cs` (jalur
transfer memanggil `OpenAdmissionAsync` dan meneruskan `result.Code`), `InpatientEpisodeController.cs`
(`active-mothers`, `FromStatus`), `PatientEncounterController.cs` (saklar dan pesan kunjungan aktif),
`OutpatientEncounterRules.cs`, `Program.cs` (registrasi DI), `appsettings.json` (nama key saklar saja).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.AdmissionEligibility.cs` (baru) | `GetAdmissionEligibilityAsync`, konstanta `PatientAlreadyAdmittedCode`/`ActiveEncounterExistsCode`, `BuildOtherDraftWarning` (kalimat Draft ganda dipakai bersama), `FindOpenDraftAsync` (menggugurkan Draft lewat batas lewat `ExpireDraftIfDueAsync`, membaca pemesanan aktif) |
| `Areas/HealthServices/InPatientManagement/DTOs/InpatientAdmissionEligibilityDtos.cs` (baru) | `InpatientAdmissionEligibilityQuery`, `InpatientAdmissionEligibilityResponse`, `…BlockerResponse`, `…EpisodeResponse`, `…EncounterResponse` |
| `Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.cs` | Dependency `IConfiguration`; `OpenAdmissionAsync` menolak 409 `PATIENT_ALREADY_ADMITTED` sesudah pasien terbaca dan sebelum pemeriksaan lain/transaksi; kalimat Draft ganda memakai `BuildOtherDraftWarning` (teks tidak berubah) |
| `Areas/HealthServices/InPatientManagement/Controllers/InpatientEpisodeController.cs` | Action `GET admission-eligibility` |
| `Areas/HealthServices/RegistrationManagement/Services/OutpatientEncounterRules.cs` | `BlockActiveEncounterConfigKey`, `IsActiveEncounterBlockEnabled`, `FindBlockingEncounterAsync`, `BuildActiveEncounterMessage`, record `BlockingEncounterInfo` — dipindah dari controller pendaftaran tanpa perubahan isi |
| `Areas/HealthServices/RegistrationManagement/Controllers/PatientEncounterController.cs` | Konstanta, record, dan dua method private kini mendelegasikan ke `OutpatientEncounterRules`; pemanggil (`:808-826`, `:1795-1803`) tidak berubah |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Baru:** `GET /api/v1/health-services/inpatient-management/episodes/admission-eligibility`. **Berubah:** `POST /episodes` (dan admisi transfer yang memanggilnya) menjawab 409 `errors.code = "PATIENT_ALREADY_ADMITTED"` untuk pasien yang sedang hadir — sebelumnya 200 lalu ditolak 409 kode yang sama pada pemesanan bed. Endpoint lain dan seluruh pesan pendaftaran tidak berubah |
| Database | Tidak ada perubahan schema atau migration. Pembacaan kelayakan dapat menggugurkan Draft lewat batas milik pasien itu (menulis riwayat `ExpireDraft`), pola yang sama dengan `GET /episodes` (`RWI-DEC-030`) |
| Keamanan/Auth | `InpatientEpisode : Read` (`[AccessAction("Read", …)]` + `[AccessPermission("InpatientEpisode", "Read")]`, sama dengan `active-mothers`). Tidak ada role hardcode. Response tidak memuat NIK, alamat, atau catatan klinis; `EpisodeId` hanya untuk tautan |

Jalur transfer IGD/Poli: `InpAdmissionTransferService.BedManagement.cs:27-30` mengubah hasil `Conflict`
menjadi `InpBedOperationException(409, result.Code …)`, sehingga kodenya tetap
`PATIENT_ALREADY_ADMITTED` dan seluruh transaksi koordinator digulung balik seperti sebelumnya.
Aturan kunjungan aktif **sengaja tidak** ditambahkan ke `OpenAdmissionAsync`: jalur transfer membuat
kunjungan rawat inap untuk pasien yang kunjungan poli asalnya memang masih aktif.

---

## 4. Dokumentasi endpoint

#### Health Services / Inpatient Management / Inpatient Episode

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/inpatient-management/episodes/admission-eligibility` | Kelayakan pasien dibukakan admisi baru | `InpatientEpisode : Read` |
| `POST` | `/api/v1/health-services/inpatient-management/episodes` | Membuka admisi (perilaku baru: 409 `PATIENT_ALREADY_ADMITTED`) | `InpatientEpisode : Create` |

**Query.** `patientId` (Guid, wajib), `admissionReferralId` (Guid, opsional — jalur Kamar Pulih).

**Response 200** (`ApiResponse<InpatientAdmissionEligibilityResponse>`), contoh pasien yang sedang dirawat:

```json
{
  "patientId": "…",
  "canContinue": false,
  "blockingCode": "PATIENT_ALREADY_ADMITTED",
  "message": "Tn. Umar sudah dirawat pada episode RI-261006-46CC di Kamar 02 BED 001. Bila memang pindah kamar, pakai perpindahan, bukan admisi baru.",
  "blockers": [
    { "code": "PATIENT_ALREADY_ADMITTED", "message": "Tn. Umar sudah dirawat pada episode …" }
  ],
  "presentEpisode": { "episodeId": "…", "episodeNumber": "RI-261006-46CC", "locationText": "Kamar 02 BED 001", "createdAt": null, "hasActiveReservation": false },
  "activeEncounter": null,
  "draftEpisode": null,
  "warnings": []
}
```

| Kode penghalang | Sumber aturan | Jalan keluar bagi petugas |
| --- | --- | --- |
| `PATIENT_ALREADY_ADMITTED` | `INV-INP-10` / `RWI-DEC-054` | Perpindahan tempat tidur dari Detail Episode |
| `INP-ADM-REF-001` | `VAL-RWF-87` | Admisi lewat jalur Kamar Pulih |
| `INP-ADM-REF-002` | `VAL-RWF-88` | Permintaan yang dirujuk sudah selesai/dibatalkan atau bukan milik pasien |
| `ACTIVE_ENCOUNTER_EXISTS` | RJ-DOC-REV-BE-007, saklar `HealthServices:Registration:BlockActiveEncounter` | Selesaikan/batalkan kunjungan, atau jalur Transfer IGD/Poli |

**Galat.** 400 "Pasien belum dipilih." (`patientId` kosong); 404 "Pasien tidak ditemukan atau tidak aktif."

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build ./QuilvianSystemBackend.csproj --configuration Debug -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false -p:RunAnalyzers=false -p:BaseOutputPath=<scratchpad>/be-bin/ -p:UseAppHost=false` | Build succeeded, 0 error, 284 warning, 2 m 5 d | `PASS` | Keluaran perintah. Warning pada berkas yang disentuh hanya yang lama: CS1573 `CancelAdmissionAsync` (`InpEpisodeService.cs:540-544`) dan CS8619 `PatientEncounterController.cs:2778`; tidak ada warning pada baris baru. Jumlah sama dengan garis dasar 13-10 |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` pada 6 berkas yang disentuh | 6 × PASS — 0 violation, 0 review, 0 info | `PASS` | Keluaran perintah |
| Review diff/scope | 4 berkas diubah (+124/−32), 2 berkas baru; tidak ada dependency, konfigurasi, atau migration. `Storage/uploads/patient-qrcodes/00-00-00-25/` sudah ada sebelum task dan tidak disentuh | `PASS` | `git status --short`, `git diff --stat` |
| Kalimat kunjungan aktif pendaftaran tidak berubah | Isi `BuildActiveEncounterMessage` dipindah apa adanya; pemanggil memakai record dengan field yang sama | `PASS` (review) | Diff `PatientEncounterController.cs` |
| Uji API terarah (AC 1–8) terhadap backend berjalan | Tidak dijalankan | `NOT RUN` | Backend lokal pemilik berjalan `--no-build` dari DLL lama; build dan uji runtime dijalankan pemilik (keputusan 10-09-2026) |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria (`FIX-EPS-010-01`) | Status | Bukti |
| --- | --- | --- |
| AC 1 — pasien `Admitted` → `PATIENT_ALREADY_ADMITTED` + episode/lokasi; `DischargePending` sudah pergi tidak menghalangi | Terpenuhi pada source | `FindPresentEpisodeAsync` (definisi hadir sama dengan index `IX_InpEpisode_PatientId_Present`) → `PresentEpisode` + `RejectionMessage` |
| AC 2 — kunjungan aktif menghalangi bila saklar hidup, kalimat sama persis; IGD/rawat inap/penunjang tidak | Terpenuhi pada source | `OutpatientEncounterRules.IsActiveEncounterBlockEnabled` + `FindBlockingEncounterAsync` (`WhereBlocksRegistration`) + `BuildActiveEncounterMessage`, dipakai juga pendaftaran |
| AC 3 — permintaan Kamar Pulih `INP-ADM-REF-001`/`002`; permintaan sendiri tidak menghalangi | Terpenuhi pada source | `CheckForAdmissionAsync(patientId, admissionReferralId)` |
| AC 4 — Draft berlaku → peringatan + `draftEpisode`; Draft lewat batas digugurkan dulu | Terpenuhi pada source | `FindOpenDraftAsync` → `ExpireDraftIfDueAsync` (berkunci `LockDraftStillDueAsync`), `BuildOtherDraftWarning` |
| AC 5 — pasien bersih → `canContinue=true`, tanpa peringatan | Terpenuhi pada source | `CanContinue = Blockers.Count == 0` |
| AC 6 — 400 `patientId` kosong; 404 pasien tidak ada/nonaktif | Terpenuhi pada source | Action `GetAdmissionEligibility`; `GetAdmissionEligibilityAsync` mengembalikan `null` |
| AC 7 — `POST /episodes` pasien hadir → 409 `PATIENT_ALREADY_ADMITTED` tanpa tulisan | Terpenuhi pada source | Penjaga di `OpenAdmissionAsync` sebelum transaksi dan sebelum kunjungan/episode dibuat |
| AC 8 — pendaftaran tidak berubah | Terpenuhi (review) | Delegasi tanpa perubahan isi |
| DoD: build | Terpenuhi | Bagian 5 |
| DoD: QBE | Terpenuhi | Bagian 5 |
| DoD: uji API runtime | Belum dijalankan | Dikecualikan atas keputusan pemilik |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada warning baru |
| Masalah yang diketahui | Bila pasien ditempatkan petugas lain **di antara** pemeriksaan ulang frontend dan `POST /episodes` (hitungan detik), kunjungan rawat inap yang baru dibuat tetap tertinggal — episodenya tidak, karena kini ditolak. Ditutup tuntas hanya oleh endpoint atomik (opsi C `PLAN-REPAIR-EPS-010`), di luar permintaan |
| Risiko tersisa | `GET admission-eligibility` dapat menulis (menggugurkan Draft lewat batas) — konsisten dengan `RWI-RULE-022`. Literal `"PATIENT_ALREADY_ADMITTED"` lama di `InpBedOccupancyService` dibiarkan (legacy, tidak disentuh) |
| Perubahan sampingan | `NONE` (keluaran build di scratchpad; `obj/` ikut diperbarui seperti build biasa) |
| Interupsi | `NONE` |
| Status Git | ` M` 4 berkas source di atas, `??` 2 berkas source baru, `??` issue/plan repair 010, laporan ini, roadmap/traceability admisi-pendaftaran diperbarui. Di luar task ini: `BillingDepositService.cs` dan dokumen `ISSUE-EPS-011`/`PLAN-REPAIR-EPS-011` dari agen lain yang berjalan bersamaan (tidak disentuh), serta `Storage/uploads/patient-qrcodes/00-00-00-25/` yang sudah ada sebelum task |
| Langkah berikutnya | `FE-RWI-244` (`FIX-EPS-010-02`). Pemilik: hentikan backend, build ulang, jalankan, lalu uji `PLAN-REPAIR-EPS-010` bagian 8 |
