# Laporan Perubahan Backend — `FIX-EPS-040-BE`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FIX-EPS-040-BE` (bagian backend `PLAN-REPAIR-EPS-040`) |
| Judul | Gelang bayi bernama benar, bed Perinatologi hanya di ruang bayi, waktu lahir tervalidasi zona rumah sakit, identitas wali tervalidasi, DPJP bayi beralasan |
| Slice | Repair admisi bayi baru lahir hasil live testing 25-10-2026 |
| Roadmap | **Tidak ada entri roadmap.** Register status dipegang [PLAN-REPAIR-EPS-040](../../../docs/plan-repair/plan-repair-040-temuan-live-testing-admisi-bayi.md) bagian 1, sama seperti `FIX-EPS-038`/`FIX-EPS-039` |
| Trace | [ISSUE-EPS-040](../../../docs/issue/issue-040-temuan-live-testing-admisi-bayi.md); keputusan pemilik K-01, K-02, K-03, K-04, K-05, K-06 = rekomendasi (chat 25-10-2026); skema `05-skema-tampilan.md` 3.12; `issue-031`; MTG-BAYI-04 |
| Contract version | Perluasan additive (lihat bagian 3.3); tidak ada field lama yang dihapus |
| Dependency | Tidak ada. `FIX-EPS-040-FE` memakai field baru dari task ini |
| Klasifikasi | `HEAVY` — skor 9: berkas diperiksa > 20 (2), berkas diubah > 8 (2), logika bisnis sedang (1), kontrak API berubah additive (2), database hanya perilaku query (1), keamanan/auth berkaitan (1) |
| Task mode | `BACKEND` (frontend baca-saja pada task ini) |
| Target tulis | `Areas/HealthServices/InPatientManagement/**`, `Areas/HealthServices/PatientManagement/MasterData/Services/**`, laporan ini, register plan repair |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `985a1fc75c2bb69163251bb7744bead949ae7285` (`MHamzah`) |
| Tanggal | 2026-10-25 |
| Status | Source selesai untuk 15 perbaikan backend. **`dotnet build` dijalankan sendiri oleh pemilik** (permintaan 25-10-2026); verifikasi runtime belum dilakukan |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `InPatientManagement` dan `HealthServices` / `PatientManagement` |
| Pemilik / prefix registry | Inpatient / `Inp` (`ACTIVE`); PatientManagement operational / `Pat` (`ACTIVE`) |
| Keberlakuan | `TOUCHED LEGACY` — mengubah service, DTO, dan controller yang sudah ada; satu helper statis baru `NewbornPatientNaming` tanpa entity |
| QBE yang berlaku | `QBE-VAL-001` (validasi request dan invarian), `QBE-API-001` (envelope dan status existing), `QBE-DTO-001` (DTO, bukan entity), `QBE-SVC-001` (logika di service), `QBE-CODE-003` (alasan menonaktifkan consumer BE-RWI-184) |
| QBE yang tidak berlaku | `QBE-MOD-002/003`, `QBE-NAM-*` (tidak ada model baru), `QBE-CFG-*`, `QBE-DB-*` (tidak ada migration), `QBE-PERM-001` (tidak ada endpoint baru) |

---

## 1. Masalah yang diperbaiki

Live testing 25-10-2026 menemukan beberapa masalah di sisi backend:
- Gelang bayi tercetak "BY. NY. NY. [IBU]" dan mengabaikan nama bayi yang diisi petugas.
- Bayi Perinatologi bisa dipesankan boks di kamar ibu lain atau bed NICU.
- Bayi yang lahir pukul 00.00–06.59 WIB ditolak karena tanggal dibandingkan dengan tanggal UTC.
- Waktu lahir yang belum terjadi dan tanggal absurd diterima.
- NIK penanda tangan bebas isi.
- DPJP bayi tidak dibatasi.
- Bed terisi diberi alasan ganda.
- Status kasus tampil "InProgress".
- Nomor episode memakai jam UTC.

Contoh nyata: bayi bernama "Bagas Pratama" memiliki label "BAGAS PRATAMA / BAYI 1", tetapi gelangnya "BY. NY. NY. RINA MARLINA / BAYI 1".

---

## 2. Proses bisnis

**Tujuan:** bayi baru lahir didaftarkan, ditempatkan di bed yang sesuai kebutuhannya, dan diberi identitas yang sama di gelang, label, dan kartu.

**Pelaku:** petugas admisi rawat inap; DPJP bayi; orang tua/wali sebagai penanda tangan.

| Langkah | Aturan baru di server | Contoh |
| --- | --- | --- |
| 2 Identitas | Waktu lahir (tanggal + jam) tidak boleh melewati waktu rumah sakit sekarang + toleransi 5 menit; tanggal lahir tidak boleh lebih dari 1 hari sebelum kasus persalinan diminta; nama hanya huruf, spasi, `.`, `,`, `'`, `-` | Pukul 01.15 WIB 26-10, lahir 01.00 → diterima. Kasus diminta 23-10, lahir 21-10 → ditolak `400` |
| 3 DPJP | Dokter yang spesialisasinya bukan "anak"/"pediatri" wajib disertai `nonPediatricDoctorReason`; alasan ditulis ke catatan admisi | dr. Gigi tanpa alasan → `400` "Alasan memilih dokter non-Anak sebagai DPJP bayi wajib diisi." |
| 4 Penempatan | Mode Neonatal hanya bed di kamar `IsForNewborn`; bayi non-intensif tidak memakai bed/kamar intensif | 305-BOKS untuk bayi Perinatologi → tidak layak, `NEWBORN_NEONATAL_ROOM_REQUIRED` |
| 5 Persetujuan | NIK 16 angka atau paspor 6–9 huruf/angka; HP 10–13 angka; untuk selain pasien sendiri ketiganya (identitas, HP, hubungan) wajib — termasuk revisi di Workspace PPRI | NIK `ABCDE<script>999` → `400` |
| 7 Dokumen | Gelang bayi = nama resmi pasien + "/ BAYI n", ditambah baris `MotherLine` "IBU: [nama ibu tanpa sapaan]" | "BAGAS PRATAMA / BAYI 1" + "IBU: RINA MARLINA" |

**Jalur tidak normal:** profil rumah sakit tidak terbaca → zona waktu jatuh ke `Asia/Jakarta`. Catatan admisi bersama alasan DPJP melebihi 1000 karakter → `400`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`InpWristbandRules.cs`, `InpAdmissionWorkspaceQueryService.cs`, `InpNewbornAdmissionRules.cs`, `InpBedOccupancyService.cs`, `InpEpisodeService.cs` (+ `.NewbornCandidates.cs`), `InpEpisodeNumberService.cs`, `InpatientAdmissionConsentService.cs`, `InpatientAdmissionConsentController.cs`, `InpatientNewbornBirthEventConsumerController.cs` dan service-nya, `InpatientEpisodeController.cs`, `NewbornPatientRegistrationService.cs`, `HospitalSiteProfileQueryService.cs`, `InpAdmissionText.cs`, `MstRoom.cs`, `MstBed.cs`, `MstDoctor.cs`, `MstSpecialization.cs`, `OprCase.cs`, `OperatingRoomCaseService.cs`, `Program.cs`, DTO terkait, serta `QuilvianSystemBackend.csproj`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/PatientManagement/MasterData/Services/NewbornPatientNaming.cs` (baru) | Satu-satunya pembersih sapaan nama ibu (FIX-01) |
| `.../PatientManagement/MasterData/Services/NewbornPatientRegistrationService.cs` | Memakai `NewbornPatientNaming`; zona waktu rumah sakit; validasi waktu lahir belum terjadi (FIX-05/06), batas bawah K-03 (FIX-07), karakter nama (FIX-21) |
| `.../InPatientManagement/Services/InpWristbandRules.cs` | `BuildDisplayName` bayi = nama resmi + "/ BAYI n"; `BuildMotherLine` baru (FIX-01/02) |
| `.../InPatientManagement/Services/InpAdmissionWorkspaceQueryService.cs` | Mengisi `MotherLine` pada respons gelang |
| `.../InPatientManagement/DTOs/InpatientAdmissionWorkspaceDtos.cs` | Field `IdentityWristbandResponse.MotherLine` |
| `.../InPatientManagement/Services/InpNewbornAdmissionRules.cs` | Aturan `NEWBORN_NEONATAL_ROOM_REQUIRED` (FIX-03) dan `NEWBORN_INTENSIVE_BED_RESERVED` (FIX-04, K-02); `IsPediatricSpecialty`, `ValidateNewbornDoctorAsync`, `MergeNonPediatricReason` (FIX-09) |
| `.../InPatientManagement/DTOs/InpatientNewbornDtos.cs` | `NonPediatricDoctorReason` pada konteks bayi; `CurrentBed*`/`CurrentPatientClassName` pada kandidat ibu |
| `.../InPatientManagement/Services/InpEpisodeService.cs` | Validasi DPJP bayi dan penggabungan alasan ke catatan pada pembukaan dan pembaruan admisi; nomor kunjungan memakai jam rumah sakit (FIX-22) |
| `.../InPatientManagement/Services/InpEpisodeService.NewbornCandidates.cs` | Bed dan kelas ibu (FIX-12); label status Indonesia (FIX-13) |
| `.../InPatientManagement/Services/InpBedOccupancyService.cs` | `BED_NOT_READY` hanya bila belum ada alasan spesifik (FIX-16) |
| `.../InPatientManagement/Services/InpEpisodeNumberService.cs` | `HospitalNowAsync`; nomor episode dari jam rumah sakit (FIX-22) |
| `.../InPatientManagement/Services/InpatientAdmissionConsentService.cs` | `ValidateSignerIdentity` pada simpan dan revisi persetujuan (FIX-08) |
| `.../InPatientManagement/Controllers/InpatientAdmissionConsentController.cs` | Tag Swagger mengikuti pola modul (FIX-23) |
| `.../InPatientManagement/Controllers/InpatientNewbornBirthEventConsumerController.cs` | **Di luar FIX, atas persetujuan pemilik:** `BaseController` yang tidak ada diganti `ControllerBase`; aktor dibaca dari claim |
| `.../InPatientManagement/Services/InpatientNewbornBirthEventConsumerService.cs` | **Di luar FIX, atas persetujuan pemilik:** isi consumer BE-RWI-184 dinonaktifkan (mengembalikan "belum aktif") |
| `.../InPatientManagement/Controllers/InpatientEpisodeController.cs` | **Di luar FIX, atas persetujuan pemilik:** `PATCH {id}/bed-hold` (BE-RWI-183) mengembalikan `501` "belum tersedia" |

**Kenapa ada perubahan di luar FIX.** Commit HEAD `985a1fc7` tidak dapat di-build: `dotnet build -c Release` melaporkan 9 error di dua berkas, yaitu consumer BE-RWI-184 dan endpoint bed-hold BE-RWI-183, setelah error `BaseController` diperbaiki. Pemilik memilih "netralkan agar bisa build" (chat 25-10-2026). Consumer **dinonaktifkan, tidak sekadar dibetulkan nama field-nya**, karena draf aslinya membuat No. RM acak `BY-yyMMdd-####` di luar deret nomor resmi (`QBE-CODE-003`), episode berunit layanan `Guid.Empty`, dan encounter fiktif. Endpoint itu tidak dipanggil otomatis oleh mana pun, dan frontend tidak memanggil `PATCH bed-hold`. Draf asli keduanya tetap ada di riwayat Git.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Additive: `wristband.motherLine`; kandidat ibu `currentBedId/Code/Name`, `currentPatientClassName`; `newbornAdmission.nonPediatricDoctorReason`. Berubah nilai: `deliveryCaseStatusName` kini Indonesia; nomor episode/kunjungan berbasis jam rumah sakit. Penolakan baru `400` (pendaftaran bayi, persetujuan, episode) dan kode kelayakan baru `422`. `PATCH bed-hold` → `501`; `POST integration/birth-event-consumer` → `400` "belum aktif" |
| Database | **Tidak ada** perubahan schema, entity, atau migration. Alasan DPJP disimpan di kolom `Notes` yang sudah ada (maks 1000) |
| Keamanan/Auth | Tidak ada endpoint baru dan metadata akses tidak berubah. Validasi identitas wali mengurangi isian berbahaya pada dokumen hukum |

---

## 4. Dokumentasi endpoint

#### Health Services / Patient Management / Master Data / Patient

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/newborns` | Mendaftarkan pasien bayi; kini menolak waktu lahir yang belum terjadi, tanggal sebelum kasus persalinan −1 hari, dan nama bertag | `Patient : Create` |

#### Health Services / Inpatient Management / Inpatient Episode

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/newborn-mother-candidates` | Kandidat ibu, kini dengan bed dan kelas ibu serta label status Indonesia | `InpatientEpisode : Read` |
| `POST` | `/` | Membuka episode Draft; DPJP bayi non-anak wajib beralasan | `InpatientEpisode : Create` |
| `PATCH` | `/{id}/bed-hold` | **Dinetralkan** — `501` sampai method service BE-RWI-183 dibuat | `InpatientEpisode : Update` |

#### Health Services / Inpatient Management / Bed Occupancy

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/available-beds` | Kelayakan bed bayi dengan aturan Neonatal dan intensif yang baru; alasan tidak lagi ganda | `InpatientBedOccupancy : Read` |

#### Health Services / Inpatient Management / Inpatient Admission Workspace

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/identity-labels` | Gelang bayi bernama resmi + baris ibu | `InpatientAdmissionDocument : Print` |

#### Health Services / Inpatient Management / Inpatient Admission Consent

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/` | Menyimpan persetujuan dengan validasi identitas wali | `InpatientEpisode : Create` |

#### Health Services / Inpatient Management / Integration

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/birth-event-consumer` | **Dinetralkan** — mengembalikan `400` "Consumer kelahiran SC (BE-RWI-184) belum aktif" | `InpatientAdmission : Create` (lihat temuan 7.1) |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -c Release` pada HEAD + perubahan, sebelum penetralan | 1 error `BaseController` (CS0246), lalu 9 error di consumer BE-RWI-184 dan bed-hold BE-RWI-183; **0 error di berkas FIX** | `EXISTING / ENVIRONMENT ISSUE` | Daftar error: `InpatientNewbornBirthEventConsumerService.cs` (7), `InpatientEpisodeController.cs` (2); berkas berasal dari commit `985a1fc7`, DLL yang berjalan dibangun 24-10 00.54 sebelum commit itu |
| `dotnet build` setelah penetralan | Tidak dijalankan agent | `NOT RUN` | Pemilik meminta menjalankan `dotnet build` sendiri (25-10-2026); pemeriksa perintah shell juga sedang gagal sementara |
| Tinjauan baca-ulang kode | Namespace, cakupan variabel (`admissionNotes` di `OpenAdmissionAsync`, `updatedNotes` di `UpdateAdmissionAsync`, `hospitalTimeZone` di `RegisterAsync`), pemanggil `BuildDisplayName` (1) | `PASS` | Bagian 3.1 |
| QBE Strict (`tooling/qbe/Invoke-QbeConformanceCheck.ps1`) | Tidak dijalankan | `NOT RUN` | Menunggu build pemilik |
| Verifikasi runtime | Tidak dijalankan | `NOT RUN` | Backend tidak di-restart atas permintaan pemilik |

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`. Plan repair menyebut "uji unit"; sesuai TEST_POLICY, tidak ada test backend yang dibuat.

Uji manual: `REQUIRED` — skenario V-01 s.d. V-10 plan repair bagian 8 setelah build dan restart.

---

## 6. Acceptance criteria dan Definition of Done

Setiap acceptance criteria di bawah sudah terpetakan ke source. Tidak satu pun sudah **terbukti** lewat build atau uji, sehingga status register `🟡`.

| Kriteria (plan repair bagian 4) | Status | Bukti source |
| --- | --- | --- |
| FIX-01 AC1–4 sapaan ganda hilang, dewasa tidak berubah | Ada di source, belum terbukti | `InpWristbandRules.BuildDisplayName`/`BuildMotherLine`, `NewbornPatientNaming` |
| FIX-02 AC1–4 nama resmi + baris ibu | Ada di source (BE), belum terbukti | `MotherLine`; tampilan di `FIX-EPS-040-FE` |
| FIX-03 AC1–5 boks kamar dewasa tidak layak untuk Neonatal | Ada di source, belum terbukti; AC5 (denah Lantai 2) bergantung data P-01 | `InpNewbornAdmissionRules.ValidateBed` |
| FIX-04 AC1–2 NICU hanya untuk bayi intensif; AC3 tidak berlaku (K-02 = tidak) | Ada di source, belum terbukti | idem |
| FIX-05 AC1–3 zona rumah sakit | Ada di source, belum terbukti | `NewbornPatientRegistrationService.RegisterAsync` |
| FIX-06 AC3 (server) waktu belum terjadi `400` | Ada di source, belum terbukti | idem |
| FIX-07 AC1–3 (server) batas bawah K-03 | Ada di source, belum terbukti | idem, setelah kasus dibaca |
| FIX-08 AC4–5 (server) identitas wali | Ada di source, belum terbukti | `ValidateSignerIdentity` |
| FIX-09 AC2–3 (server) DPJP non-anak beralasan | Ada di source, belum terbukti | `ValidateNewbornDoctorAsync`, `MergeNonPediatricReason` |
| FIX-12 AC1, AC3 | Ada di source, belum terbukti | `NewbornCandidates.cs` |
| FIX-13 AC1–2 | Ada di source, belum terbukti | `DeliveryCaseStatusLabel` |
| FIX-16 AC1–2 | Ada di source, belum terbukti | `SpecificUnavailableCodes` |
| FIX-21 AC4 (server) nama bertag `400` | Ada di source, belum terbukti | `NewbornNamePattern` |
| FIX-22 AC1–2 | Ada di source, belum terbukti | `InpEpisodeNumberService.HospitalNowAsync` |
| FIX-23 AC1 | Ada di source, belum terbukti | `[Tags(...)]` |
| DoD: `dotnet build` lulus | **Belum terpenuhi** | Dijalankan pemilik |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | (1) Toleransi 5 menit pada "waktu lahir belum terjadi" ditambahkan di server untuk selisih jam perangkat; tidak tertulis di plan. (2) Pengenal dokter anak berbasis nama spesialisasi ("anak"/"pediatri"), karena master spesialisasi belum punya penanda khusus |
| Masalah yang diketahui | 7.1 `InpatientNewbornBirthEventConsumerController` memakai `[AccessPermission("InpatientAdmission", "Create")]` yang tidak sama dengan `ControllerName = "InpatientNewbornBirthEventConsumer"`, sehingga hasilnya 403 permanen (role-access-rules). Dicatat, tidak diperbaiki. 7.2 BE-RWI-183 (bed-hold) dan BE-RWI-184 (consumer) perlu dikerjakan ulang. 7.3 `rules/backend/TEST_POLICY.md` menyatakan folder `Tests/` sudah dihapus, tetapi repository masih memuatnya (dikecualikan dari build lewat csproj); tidak dipakai dan tidak ditambah |
| Risiko tersisa | Bila kamar R. Bayi/Ruang NICU belum bertanda `IsForNewborn`, FIX-03 menolak bed yang benar (cek P-01). Persetujuan lama dengan NIK tidak valid harus dikoreksi saat diedit. Nomor episode lama (UTC) dan baru (WIB) tercampur |
| Perubahan sampingan | `NONE` |
| Interupsi | Pemeriksa keamanan perintah shell gagal sementara berulang kali; build lanjutan tidak dijalankan. Pemilik kemudian meminta menjalankan `dotnet build` sendiri |
| Status Git | Tidak dapat dijalankan (pemeriksa shell gagal). Berkas source: 15 diubah + 1 baru (`NewbornPatientNaming.cs`), seluruhnya di bagian 3.2; ditambah dokumen issue/plan/laporan |
| Langkah berikutnya | 1) Pemilik menjalankan `dotnet build`. 2) QBE Strict pada berkas bagian 3.2. 3) Restart backend dan uji ulang V-01–V-10 dengan ibu cadangan. 4) Jalankan P-01. 5) Rencanakan ulang BE-RWI-183/184 |
