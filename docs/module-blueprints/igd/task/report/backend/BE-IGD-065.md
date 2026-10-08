# Laporan Perubahan Backend — `BE-IGD-065`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-065` |
| Judul | Daftar dan detail kunjungan IGD membawa DPJP aktif, dapat disaring per dokter dan kunjungan berjalan |
| Slice | `EPIC IGD-14` / `MVP-9` — daftar pasien dokter |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian R3.16 |
| Requirement | `FR-IGD-096`; `AT-IGD-200`; DoD PRD §10.4 butir 1 |
| Keputusan | `IGD-DEC-222` (semua pasien berjalan + saringan *Pasien saya*); `IGD-DEC-230` pilihan desain 8 (`doctorId` dikirim layar); arti DPJP aktif `IGD-DEC-117`, `130` |
| Contract version | API **`0.15.0`** §10.1 nomor 1–2, §10.2; validation **`0.14.0`** §12.1 aturan 1–3; permission/audit **`0.8.0`** §9.1 baris 1 — seluruhnya `approved` (`IGD-DEC-230`, sementara pola `IGD-DEC-174`) |
| Dependency | — (baseline: `BE-IGD-044`, `045` ✅ — tabel dan definisi penugasan dokter) |
| Klasifikasi | `MEDIUM` — skor 4: repository 0, berkas diperiksa 0 (8 berkas), berkas diubah 0 (3 berkas), logika bisnis 1, kontrak API 2 (aditif: dua saringan, dua ruas respons), database 1 (perilaku kueri), keamanan/auth 0, UI/workflow 0 |
| Task mode | `BACKEND` — izin pemilik 7 Oktober 2026 (*"iyaa lanjut kerjakan"*, atas tawaran membangun `BE-IGD-065` lalu `FE-IGD-045`). Frontend baca-saja |
| Target tulis | `NewQuilvianSystemBackend` (branch `rizkiG`): tiga berkas `Areas/HealthServices/EmergencyInstallationManagement/**` pada bagian 3.2; laporan ini, baris status roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `30ea0a3a` (`rizkiG`, sejajar origin, bersih). Source identik dengan `43dab6da` — dua commit sesudahnya hanya dokumen |
| Commit frontend rujukan | `6c66327aa` (`RizkiV2`) + perubahan pemilik pada `emergency-assessment-observation-tab.jsx` (bukan task ini) |
| Tanggal | 7 Oktober 2026 |
| Status | 🟡 **SEBAGIAN — 7–8 Oktober 2026: implementasi selesai dan dibuild pemilik; 3 dari 8 acceptance terbukti penuh (6 dengan delta, 7, 8).** Tiga berkas (+75/−12); QBE checker Strict `PASS` pada ketiganya. Build pemilik: `dotnet build` 0 error, 235 warning (identik baseline; dilaporkan 8 Oktober 2026). Delta 1 (`ongoing=false` simetris) disahkan sebagai `IGD-DEC-233`. **Belum:** uji API acceptance 1–5 pada putaran uji 1 bersama `FE-IGD-045` (roadmap backend R3.16.4). Tanpa UAT |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `EmergencyInstallationManagement` |
| Pemilik / prefix registry | `Emg` = *Emergency*, `BUSINESS DOMAIN / MODULE`, `ACTIVE / LEGACY` — `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 26 |
| Applicability | **`TOUCHED LEGACY`** untuk `EmergencyVisitController` (`GetAll`, `GetById`, konstruktor, `ToResponse` tiga-argumen), `EmergencyDoctorAssignmentService.AmbilAktifAsync`, dan `EmergencyVisitResponse`; **`NEW CODE`** untuk `KueriBerjalan`, `AmbilBerjalanPerKunjunganAsync`, dua parameter kueri, dan dua ruas respons. Nol entity, nol model persisted, nol rename, nol migration |
| QBE yang berlaku | `QBE-API-001` (route, envelope `ApiResponse<PagedResult<…>>`, kode status, dan pola saringan boolean tetap), `QBE-DTO-001` (DPJP keluar lewat `EmergencyVisitResponse`, bukan entity), `QBE-PERM-001` (`[AccessAction]`/`[AccessPermission]` `EmergencyVisit : Read` tidak disentuh), `QBE-PAGE-001` (paging dan urutan daftar tidak berubah), `QBE-SVC-001` (definisi penugasan berjalan dan pengambilan DPJP per halaman tinggal di service terdaftar) |
| QBE yang dicatat, tidak ditangani | `QBE-SVC-001` untuk kode lama — `GetAll`/`GetById` tetap membangun kueri kunjungan dengan `ApplicationDbContext` langsung di controller (pola lama, `02-backend-architecture.md` §15.5 *utang teknis yang tidak ditiru dan tidak dirapikan*) |
| QBE yang tidak berlaku | `QBE-MOD-*`, `QBE-NAM-*`, `QBE-CFG-*`, `QBE-CODE-*`, `QBE-DB-*` — nol entity, penamaan, konfigurasi EF, nomor bisnis, atau pekerjaan database |
| Checker | `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Path <berkas> -Mode Strict` — per berkas, ketiganya VIOLATION 0, REVIEW 0, INFO 0, *Final result: PASS* |

---

## 1. Masalah yang diperbaiki

Layar dokter IGD yang direncanakan (`FE-IGD-045`) harus menampilkan daftar pasien IGD yang kunjungannya masih berjalan,
bawaannya hanya pasien dengan dokter itu sebagai DPJP, dan nama DPJP di setiap baris. Sebelum task ini,
`GET /emergency-visits` tidak dapat disaring per dokter maupun per *kunjungan berjalan*, dan barisnya tidak membawa DPJP
sama sekali. Layar hanya dapat menebusnya dengan memanggil `GET emergency-doctor-assignments/active` **sekali per baris**
— 25 pasien berarti 25 permintaan tambahan setiap kali daftar dimuat (`IGD-CAP-71`).

*Contoh.* Pukul 10.00 ada tiga kunjungan berjalan: Tn. Budi (DPJP dr. Ani sejak 09.20), Ny. Sari (belum ada DPJP), dan
An. Dewi (DPJP dr. Ani 08.00–09.00, lalu dialihkan ke dr. Rudi). Tanpa task ini, layar dr. Ani tidak dapat meminta
*"hanya pasien saya"* kepada server, dan tidak tahu siapa DPJP Ny. Sari atau An. Dewi tanpa tiga permintaan tambahan.

---

## 2. Proses bisnis

| Butir | Isi |
| --- | --- |
| Tujuan | Dokter IGD melihat pasien yang menjadi tanggung jawabnya lebih dulu, tanpa kehilangan akses ke pasien IGD lain (`IGD-DEC-222`) |
| Pelaku | Dokter IGD (pemakai layar); pemegang `EmergencyVisit : Read` lain tetap memakai daftar yang sama |
| Pemicu | Layar dokter dibuka, atau saringan *Pasien saya* / *Semua pasien IGD* diganti |
| Prasyarat | Penugasan DPJP dicatat di layar triage (`BE-IGD-045`); layar mengenal `doctorId` pengguna dari `auth/me` |

**Langkah.**

1. Layar meminta `GET /emergency-visits?ongoing=true&doctorId=<doctorId pengguna>` (saringan *Pasien saya*).
2. Server menyaring kunjungan yang **belum** `Completed` dan **belum** `Cancelled`, lalu yang punya penugasan berjalan
   (`EffectiveTo` kosong, tidak dihapus) dengan dokter itu.
3. Server mengambil DPJP berjalan untuk seluruh baris halaman itu dalam **satu** kueri, lalu mengisi `activeDoctorId` dan
   `activeDoctorName` setiap baris.
4. Dokter mengganti saringan ke *Semua pasien IGD*: layar mengirim permintaan tanpa `doctorId`; seluruh kunjungan
   berjalan tampil, masing-masing dengan DPJP-nya atau `null`.
5. Dokter memilih satu pasien: `GET /emergency-visits/{id}` membawa dua ruas yang sama.

**Aturan.** `doctorId` adalah saringan tampilan, **bukan** hak akses — dokter lain tetap dapat membuka pasien itu
(validation §12.1 aturan 1). Arti *DPJP aktif* hanya satu: penugasan yang `EffectiveTo`-nya kosong (`IGD-DEC-117`,
`130`); definisi itu kini tinggal di satu tempat (`KueriBerjalan`) dan dipakai saringan, proyeksi daftar, detail, dan
`GET emergency-doctor-assignments/active`.

**Status.** Task ini tidak mengubah status apa pun; ia hanya membaca.

**Jalur tidak normal.**

| Keadaan | Perilaku |
| --- | --- |
| `doctorId` kosong atau `00000000-…` | Diabaikan, sama dengan saringan GUID lain pada daftar ini |
| Dokter tanpa penugasan berjalan | Daftar kosong (`totalData` 0); layar menampilkan kalimat kosong `FE-IGD-045` |
| Kunjungan tanpa DPJP | Tidak ikut saringan `doctorId`; pada daftar tanpa saringan tampil dengan `activeDoctorId`/`activeDoctorName` `null` |
| DPJP dialihkan | Hanya dokter penugasan berjalan yang dihitung; dokter sebelumnya tidak lagi melihat pasien itu di *Pasien saya* |
| `ongoing=false` | Hanya kunjungan `Completed` atau `Cancelled` — lihat delta kontrak bagian 3.3 |

**Hasil akhir.** Satu permintaan daftar menghasilkan baris lengkap dengan DPJP; nol perubahan data.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `Areas/HealthServices/EmergencyInstallationManagement/Controllers/EmergencyVisitController.cs` — `GetAll` `:61`, `GetById` `:211`, `ToResponse` `:952`, `:999`
- `…/Services/EmergencyDoctorAssignmentService.cs` — `AmbilAktifAsync` `:133`, `ProyeksiResponse` `:89`
- `…/DTOs/EmergencyVisitDtos.cs`, `…/DTOs/EmergencyDoctorAssignmentDtos.cs` (`EmergencyDoctorAssignmentResponse`)
- `…/Models/EmgVisit.cs` (`DoctorAssignments`), `…/Models/EmgDoctorAssignment.cs` (`DoctorId`, `Doctor`)
- `…/Enums/EmergencyVisitStatus.cs` (`Completed` = 9, `Cancelled` = 8)
- `Program.cs` `:594` — `EmergencyDoctorAssignmentService` sudah terdaftar `AddScoped`
- `…/Controllers/EmergencyDoctorAssignmentController.cs` `:99` — satu-satunya pemanggil `AmbilAktifAsync`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyDoctorAssignmentService.cs` (+33/−8; LF, tanpa BOM — dipertahankan) | + `KueriBerjalan()`: penugasan tidak dihapus dengan `EffectiveTo` kosong, `AsNoTracking`. + `AmbilBerjalanPerKunjunganAsync(ids)`: satu kueri untuk banyak kunjungan lewat `KueriBerjalan` dan `ProyeksiResponse` yang sudah ada (nama dokter ikut terbawa), dikembalikan sebagai kamus per kunjungan; bila ada lebih dari satu baris berjalan, yang `EffectiveFrom`-nya terbaru dipakai — sama dengan `AmbilAktifAsync`. `AmbilAktifAsync` tanpa `at` kini memakai `KueriBerjalan`; cabang `at` tidak berubah artinya |
| `…/Controllers/EmergencyVisitController.cs` (+39/−4; CRLF, BOM — dipertahankan) | Konstruktor menerima `EmergencyDoctorAssignmentService` (sudah terdaftar; nol `Program.cs`). `GetAll`: parameter `doctorId` (`Guid?`) — `EXISTS` penugasan berjalan dokter itu, dan `ongoing` (`bool?`) — `true` tanpa `Completed`/`Cancelled`, `false` hanya keduanya. Sesudah halaman dimuat, DPJP diambil sekali untuk seluruh baris. `GetById`: DPJP diambil untuk satu kunjungan. `ToResponse` tiga-argumen mengisi `ActiveDoctorId`/`ActiveDoctorName`. Atribut akses tidak disentuh |
| `…/DTOs/EmergencyVisitDtos.cs` (+3; CRLF — dipertahankan) | `EmergencyVisitResponse` + `ActiveDoctorId` (`Guid?`), `ActiveDoctorName` (`string?`) |

Nol baris komentar ditambahkan; komentar lama tidak disunting.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Aditif sesuai API `0.15.0` §10.2: dua parameter kueri pada `GET /`, dua ruas respons pada `GET /` dan `GET /{id}`. Pemanggil lama tidak terdampak. **Delta 1 — `ongoing=false`:** kontrak hanya mendefinisikan `true`; nilai `false` dibuat simetris (hanya `Completed`/`Cancelled`) mengikuti pola saringan boolean lain pada action yang sama (`awaitingClosure`, `hasDuplicateEpisodeOverride`, `isActive`). **Delta 2 — acceptance 6:** DPJP diambil dengan **satu kueri tambahan per halaman**, bukan di dalam kueri daftar yang sama — pola yang sama dengan `AmbilAlasanMenungguPenutupanAsync` pada action ini — supaya definisi penugasan berjalan tetap satu (`KueriBerjalan`, `02` §15.4). Maksud acceptance 6 (tanpa pemanggilan per baris) terpenuhi; bunyinya tidak persis |
| Database | Nol schema, entity, migration. Satu kueri baca tambahan per permintaan daftar atau detail, memakai indeks `IX_EmgDoctorAssignment_EmergencyVisitId_Active` yang sudah ada; saringan `doctorId` menjadi subkueri `EXISTS` |
| Keamanan/Auth | `NOT APPLICABLE` — butir hak akses tidak berubah; `doctorId` bukan hak akses. Data yang ditambahkan (nama dokter) sudah terbaca pemegang `EmergencyVisit : Read` lewat layar triage |

---

## 4. Dokumentasi endpoint

#### Health Services / Emergency Installation Management / Emergency Visit

Base URL: `api/v1/health-services/emergency-installation-management/emergency-visits`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar kunjungan IGD; **kini** dapat disaring per DPJP aktif (`doctorId`) dan kunjungan berjalan (`ongoing`), dan setiap baris membawa DPJP aktif | `EmergencyVisit : Read` | Query lama + `doctorId` (`uuid`), `ongoing` (`boolean`) | `PagedResult<EmergencyVisitResponse>` + `activeDoctorId`, `activeDoctorName` |
| `GET` | `/{id}` | Detail kunjungan; **kini** membawa DPJP aktif | `EmergencyVisit : Read` | — | `EmergencyVisitResponse` + dua ruas yang sama |

Kode status tidak berubah: `200` berhasil; `403` tanpa `EmergencyVisit : Read`; `404` *"Data kunjungan IGD tidak
ditemukan."* pada detail.

Contoh satu baris `GET /?ongoing=true&doctorId=<dr. Ani>` (data samaran):

```json
{
  "id": "6b1e…",
  "emergencyVisitNumber": "IGD-261006091502-1A2B3C",
  "patientName": "Budi Santoso",
  "visitStatus": 4,
  "isAwaitingClosure": false,
  "awaitingClosureReason": null,
  "activeDoctorId": "3f2a…",
  "activeDoctorName": "dr. Ani Rahma"
}
```

---

## 5. Verifikasi

| Perintah atau pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Path …/EmergencyVisitController.cs -Mode Strict` | VIOLATION 0, REVIEW 0, INFO 0 — *Final result: PASS* | `PASS` |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Path …/EmergencyDoctorAssignmentService.cs -Mode Strict` | VIOLATION 0, REVIEW 0, INFO 0 — *Final result: PASS* | `PASS` |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Path …/EmergencyVisitDtos.cs -Mode Strict` | VIOLATION 0, REVIEW 0, INFO 0 — *Final result: PASS* | `PASS` |
| `git diff --stat` (berkas task) | 3 berkas, +75/−12 | `PASS` |
| Baris komentar ditambah atau dihapus | Nol | `PASS` |
| Akhiran baris dan BOM (hitungan byte lewat Node) | Controller 1088 CRLF, 0 LF, BOM tetap; service 419 LF, 0 CRLF, tanpa BOM; DTO 361 CRLF, 0 LF | `PASS` |
| `dotnet build` | Dijalankan Rizki 7 Oktober 2026 (*"sudah saya build"*); DLL 09.54.00 WIB memuat suntingan 08.48–08.49 WIB. Error: nihil (DLL terbentuk). Warning: belum dilaporkan | `PASS` (error) |
| Uji API acceptance 1–5 | Belum — putaran uji 1 bersama `FE-IGD-045` | `NOT RUN` |

Uji manual: `REQUIRED` — lewat agen Antigravity dengan panduan dari agent.

AUTOMATED TEST: SKIPPED (opsional) — atas perintah pemilik.

### 5.1 Perintah build

Dari `NewQuilvianSystemBackend`:

```bash
dotnet build ./QuilvianSystemBackend.csproj -p:RunAnalyzers=false
```

Mohon laporkan jumlah **error dan warning** sesudahnya (build terakhir yang tercatat: 0 error, 235 warning).

### 5.2 Skenario uji untuk panduan putaran 1

Data dibuat lewat layar atau API dengan **akun peran nyata**, **tidak** lewat SQL; backend berjalan dari hasil build di
atas. Penetapan dan pengalihan DPJP lewat layar triage (`BE-IGD-045`). Dokter `ranger.biru@admin.com`; langkah perawat
`dimas.kurniawan@rsmmc.local` (`IGD-DEC-215`); sandi dari variabel lingkungan.

| No | Keadaan | Harapan | Acceptance |
| ---: | --- | --- | --- |
| `065-S1` | Tiga kunjungan berjalan: A ber-DPJP dokter X, B tanpa DPJP, C dialihkan dari X ke Y | `GET /?ongoing=true&doctorId=X` → hanya A | 1 |
| `065-S2` | Tanpa `doctorId` dan tanpa `ongoing`, saringan lain sama | `totalData` dan urutan sama dengan permintaan yang sama tanpa kedua parameter sebelum build baru (atau dibandingkan dengan `GET /?visitStatus=…` yang setara) | 2 |
| `065-S3` | Satu kunjungan `Completed` atau `Cancelled` di antara data | `ongoing=true` tidak memuatnya; `ongoing=true&doctorId=X&search=<nomor A>` tetap memuat A; `awaitingClosure=true&ongoing=true` dapat digabung | 3 |
| `065-S4` | Kunjungan A, B, C | `activeDoctorId`/`activeDoctorName` pada daftar dan `GET /{id}` sama dengan `GET emergency-doctor-assignments/active?emergencyVisitId=`; B `null` | 4 |
| `065-S5` | Kunjungan yang menunggu penutupan, dan kunjungan pasien teridentifikasi | `isAwaitingClosure`, `awaitingClosureReason`, dan ruas identitas pasien pada detail sama dengan sebelum task ini | 5 |

---

## 6. Acceptance criteria dan Definition of Done

| # | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | `?doctorId=X` hanya memuat kunjungan yang penugasan berjalannya dokter X; tanpa DPJP dan yang sudah dialihkan dari X tidak ikut | **Terpetakan, belum diuji** | `GetAll`: subkueri `KueriBerjalan().Where(DoctorId == X)` → `EXISTS` per kunjungan; `KueriBerjalan` = `!IsDelete && EffectiveTo == null` |
| 2 | Tanpa `doctorId` dan `ongoing`, hasil sama dengan sebelum task untuk saringan lain yang sama | **Terpetakan, belum diuji** | Kedua saringan hanya berlaku bila parameternya ada; urutan dan paging tidak disentuh |
| 3 | `ongoing=true` tanpa `Completed`/`Cancelled`; dapat digabung dengan `doctorId`, `search`, `awaitingClosure` | **Terpetakan, belum diuji** | Saringan `VisitStatus` disusun pada `IQueryable` yang sama dengan saringan lain |
| 4 | `activeDoctorId`/`activeDoctorName` daftar dan detail sama dengan `GET …/active`; `null` tanpa penugasan berjalan | **Terpetakan, belum diuji** | Ketiganya membaca `KueriBerjalan` dan `ProyeksiResponse` yang sama; `AmbilAktifAsync` tanpa `at` kini memakai `KueriBerjalan` |
| 5 | Regresi ruas `BE-IGD-063` dan `BE-IGD-064` pada detail tidak berubah | **Terpetakan, belum diuji** | `Include` dan pengisian `alasanMenungguPenutupan` tidak disentuh; DPJP ditambahkan sesudahnya |
| 6 | Ruas DPJP dibentuk di kueri daftar yang sama, bukan pemanggilan per baris | **Terpenuhi menurut maksudnya, dengan delta** | Satu kueri `AmbilBerjalanPerKunjunganAsync` per halaman (bukan per baris); bukan kueri yang sama — bagian 3.3 delta 2 |
| 7 | Diff hanya tiga berkas Cakupan; nol `Program.cs`, migration; nol komentar baru; komentar lama tidak disunting | **Terpenuhi** | Bagian 3.2 dan 5 |
| 8 | Build 0 error; jumlah warning dilaporkan | **Terpenuhi** | Build pemilik 8 Oktober 2026: 0 error, 235 warning (identik baseline 235 warning) |

**Definition of Done:** laporan tracked ✅; register, node grafik R3.16, dan traceability ditandai 🟡; QBE preflight dan
checker ✅; tanpa UAT PASS ✅. **Belum:** jumlah warning build (8) dan uji API 1–5 pada putaran 1.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nihil dari checker. Git memperingatkan `LF will be replaced by CRLF` untuk service — berkas itu memang LF di working tree sebelum task ini; akhiran barisnya dipertahankan |
| Masalah yang diketahui | (1) Delta 1 dan 2 pada bagian 3.3 — mohon dikonfirmasi saat review. (2) `QBE-SVC-001` untuk kode lama — kueri kunjungan tetap di controller. (3) Pemeriksaan "penugasan berjalan" di `TetapkanAsync`/`AlihkanAsync` (`EffectiveTo == null` ditulis langsung, `:183` sebelum task) belum memakai `KueriBerjalan`; di luar cakupan kartu, tidak diubah |
| Keadaan migration | Nol |
| Eksekusi database | Nol — agent tidak menjalankan kueri apa pun |
| Risiko tersisa | Rendah–sedang. Satu kueri baca tambahan per permintaan daftar/detail. `BE-IGD-069` (ClinicalManagement) akan membaca penugasan berjalan langsung dari tabel karena modul lain tidak memanggil service IGD (§15.3); definisinya wajib sama dengan `KueriBerjalan` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Langkah berikutnya | (1) `build-module-frontend` `FE-IGD-045` — sedang dikerjakan sebagai pasangan (izin 7 Oktober 2026). (2) Build backend oleh Rizki (bagian 5.1). (3) Panduan uji putaran 1 sesudah langkah 1–4 R3.16.4, atau lebih awal bila pemilik ingin menguji pasangan ini saja |

`git status --short` di akhir pekerjaan (source backend):

```text
 M Areas/HealthServices/EmergencyInstallationManagement/Controllers/EmergencyVisitController.cs
 M Areas/HealthServices/EmergencyInstallationManagement/DTOs/EmergencyVisitDtos.cs
 M Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyDoctorAssignmentService.cs
```
