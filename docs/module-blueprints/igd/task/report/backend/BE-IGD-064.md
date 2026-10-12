# Laporan Perubahan Backend — `BE-IGD-064`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-IGD-064` |
| Judul | Detail kunjungan IGD memuat identitas pasien dan konteksnya |
| Slice | Perbaikan korektif lintas gelombang; kartu pasien Ruang Kerja dipakai seluruh tab pemeriksaan (`MVP-3`…`MVP-8`) |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian R3.15 |
| Requirement | **Coverage gap: tanpa `FR-IGD-*`** — dijejak ke kontrak API §1 dan `IGD-DEC-207` |
| Keputusan | `IGD-DEC-207` (kartu backend kecil, menjawab `IGD-OQ-116`), `IGD-DEC-208` (jadwal); fakta `IGD-FACT-052`, `058` |
| Contract version | API `0.14.0` §1 `GET /{id}` — *"satu kunjungan beserta konteksnya"*; bentuk `EmergencyVisitResponse` tidak berubah. Nol perubahan kontrak |
| Dependency | — |
| Klasifikasi | `LIGHT` — skor 2: repository 0, berkas diperiksa 0, berkas diubah 0 (1 berkas), logika bisnis 0, kontrak API 1 (memakai kontrak yang ada), database 1 (perilaku kueri), keamanan/auth 0, UI/workflow 0 |
| Task mode | `BACKEND` — izin pemilik 6 Oktober 2026 (*"yaa mulai /build-module-backend BE-IGD-064"*). Frontend baca-saja |
| Target tulis | `NewQuilvianSystemBackend` (branch `rizkiG`): `Areas/HealthServices/EmergencyInstallationManagement/Controllers/EmergencyVisitController.cs`; laporan ini, baris status roadmap, traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `3811fa06` (`rizkiG`) + working tree `EmergencyDepartureService.cs` (`BE-IGD-041`) dan dokumen 6 Oktober. Berkas task tidak berubah sejak `8d81d361` |
| Commit frontend rujukan | `2a985f6d5` (`RizkiV2`), bersih |
| Tanggal | 6 Oktober 2026 |
| Status | ✅ **SELESAI — 6 Oktober 2026 (sore)** (`IGD-DEC-216`). Satu berkas (+8/−1); QBE checker Strict `PASS`; build 0 error, 235 warning. Uji ulang 03.59 UTC dengan akun `dimas.kurniawan@rsmmc.local`: acceptance 1–4 terbukti pada bukti mentah dan log backend; keenam ruas acceptance 1 terisi dan sama antara detail dan daftar. Diterima dengan penyimpangan tercatat — Akses Role diubah penguji lewat SuperAdmin (diterima pemilik sebagai konfigurasi sementara), percobaan tak dilaporkan, sandi literal di skrip, build layar tidak terbukti (bagian 8.2). Tanpa UAT. *Sebelumnya:* 🟡 **SEBAGIAN — 6 Oktober 2026: acceptance 5 dan 6 terbukti.** Satu berkas (+8/−1); QBE checker Strict `PASS`; build 0 error, 235 warning (dijalankan agen penguji; backend berjalan dari DLL itu). Uji API dan layar Antigravity 6 Oktober (siang) **ditolak sebagai bukti acceptance** — seluruh langkah memakai SuperAdmin, layar dilayani `next dev`, tiga putaran tak dilaporkan, dua ruas acceptance 1 kosong di kedua sisi (`IGD-DEC-213`; bagian 8). **Belum:** acceptance 1–4 diulang dengan akun peran nyata pada hasil build dalam putaran bersama `FE-IGD-044` (`IGD-DEC-208`). Tanpa UAT. *Status ✅ yang sempat ditulis agen penguji ditarik (pola `IGD-DEC-212`)* |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `EmergencyInstallationManagement` |
| Pemilik / prefix registry | `Emg` = *Emergency*, `BUSINESS DOMAIN / MODULE`, `ACTIVE / LEGACY` — `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 26 |
| Applicability | **`TOUCHED LEGACY`** — satu pernyataan kueri pada action controller yang sudah ada. Nol entity, nol model persisted, nol rename, nol migration |
| QBE yang berlaku | `QBE-API-001` (route, envelope, kode status, dan bentuk respons tetap), `QBE-DTO-001` (respons tetap lewat `EmergencyVisitResponse`, bukan entity), `QBE-PERM-001` (`[AccessAction]`/`[AccessPermission]` `EmergencyVisit : Read` tidak disentuh) |
| QBE yang dicatat, tidak ditangani | `QBE-SVC-001` — action ini memakai `ApplicationDbContext` langsung di controller (pola lama). Untuk `TOUCHED LEGACY` aturan ini SHOULD dan hanya bila aman serta diberi wewenang; kartu membatasi perubahan pada satu pernyataan di `GetById`, sehingga pemindahan ke service **tidak** dikerjakan |
| QBE yang tidak berlaku | `QBE-MOD-*`, `QBE-NAM-*`, `QBE-CFG-*`, `QBE-CODE-*`, `QBE-DB-*` — nol entity, penamaan, konfigurasi EF, nomor bisnis, atau pekerjaan database |
| Checker | `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Path <berkas> -Mode Strict` — 1 berkas, VIOLATION 0, REVIEW 0, INFO 0, *Final result: PASS* |

---

## 1. Masalah yang diperbaiki

Kartu pasien di Ruang Kerja Pemeriksaan IGD selalu menampilkan *"Pasien belum teridentifikasi"* dengan No. RM dan unit
pelayanan kosong — juga untuk pasien yang identitasnya lengkap (`IGD-FACT-052`, `IGD-OQ-116`). Penyebabnya bukan di
layar: `GET /emergency-visits/{id}` hanya memuat navigasi `ArrivalConfirmedByUser`, sedangkan `ToResponse` membaca nama
pasien, No. RM, unit pelayanan, cara datang, dan jenis kasus **dari navigasi** `Patient`, `ServiceUnit`, `ArrivalMode`, dan
`CaseType`. Karena navigasi itu tidak dimuat, kelima ruas selalu kosong, dan `ResolvePatientName` jatuh ke kalimat
cadangan. Daftar kunjungan (`GET /`) memuat kelimanya, sehingga baris pasien yang sama terbaca benar di daftar.

| Tempat | Navigasi yang dimuat sebelum task ini |
| --- | --- |
| `GET /` (daftar) | `Patient`, `ServiceUnit`, `ArrivalMode`, `CaseType`, `ArrivalConfirmedByUser` |
| `GET /{id}` (detail) | `ArrivalConfirmedByUser` saja |

---

## 2. Proses bisnis

**Pelaku:** perawat dan dokter IGD pemegang `EmergencyVisit : Read`.

1. Petugas memilih pasien di daftar Pengkajian IGD.
2. Ruang Kerja dibuka dan memanggil `GET /emergency-visits/{id}` (`fetchAssessmentVisitContext` di frontend).
3. Kartu pasien menampilkan nama, No. RM, dan unit pelayanan dari respons itu.

*Contoh.* Pasien berinisial A. P. (data samaran), RM `00-00-00-66`, unit *Instalasi Gawat Darurat*, datang dengan ambulans.
Sebelum task ini detail menjawab `patientName` *"Pasien belum teridentifikasi"*, `medicalRecordNumber` dan
`serviceUnitName` kosong. Sesudahnya: nama lengkap pasien, `00-00-00-66`, *Instalasi Gawat Darurat*, `arrivalModeName`
*Ambulans* — sama dengan baris pasien itu pada daftar.

**Jalur tidak normal.** Kunjungan yang tidak ada atau terhapus tetap `404` *"Data kunjungan IGD tidak ditemukan."*;
pengguna tanpa `EmergencyVisit : Read` tetap `403`. Nama pasien tetap disusun `ResolvePatientName` yang tidak diubah:
`Patient.FullName` bila terisi, lalu alias sementara, lalu *"Pasien belum teridentifikasi"*.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Kartu `BE-IGD-064` (roadmap backend R3.15); `00-interview-decisions.md` — `IGD-DEC-207`, `IGD-FACT-052`, `058`
- `Controllers/EmergencyVisitController.cs` — `GET /` (baris 84–91), `GetById` (baris 206–230 sesudah perubahan), `ResolvePatientName`
  (baris 941), `ToResponse` (baris 952)
- `Models/EmgVisit.cs` (`PatientId`, `IsUnknownPatient`, `TemporaryPatientAlias`); `Services/EmergencyVisitService.cs`
  (pembentukan kunjungan dari encounter, baris 950–975)
- `AGENTS.md` backend, `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`, `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Controllers/EmergencyVisitController.cs` | +8/−1. Kueri `GetById` kini memuat `Patient`, `ServiceUnit`, `ArrivalMode`, `CaseType`, dan `ArrivalConfirmedByUser` — rangkaian yang sama dengan `GET /` — lalu `FirstOrDefaultAsync` dengan syarat yang sama (`Id`, `!IsDelete`). `ToResponse`, `ResolvePatientName`, pemanggilan `AmbilAlasanMenungguPenutupanAsync`, atribut akses, dan kalimat `404` tidak disentuh |

Akhiran baris: 1046 → 1053 baris, seluruhnya CRLF; BOM UTF-8 dipertahankan. Nol baris komentar baru; komentar lama tidak
disunting.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Nol route, nol perubahan bentuk respons, nol kode status baru. Ruas yang sebelumnya selalu kosong kini terisi |
| Database | Nol migration, nol skema. Satu baris kunjungan dibaca dengan empat join tambahan — sama dengan kueri daftar |
| Keamanan dan privasi | Atribut akses tidak berubah. Nama pasien dan No. RM kini tampil di detail bagi pemegang `EmergencyVisit : Read` — hak yang sama yang sudah membaca data itu di daftar kunjungan |
| `Program.cs`, DTO, service | Tidak disentuh |

### 3.4 Catatan pemetaan acceptance 2

Setiap kunjungan IGD kini selalu terhubung ke pasien lewat encounter (`PatientId = encounter.PatientId`), dan modul
Registrasi tidak punya jalur khusus pasien tanpa identitas. Akibatnya, untuk kunjungan bertanda `IsUnknownPatient`,
detail menampilkan `Patient.FullName` bila petugas Registrasi mengisinya, dan alias sementara hanya bila nama itu kosong
— persis seperti daftar (acceptance 1). Bunyi acceptance 2 (*"alias sementara bila ada"*) terpenuhi untuk pasien yang
namanya kosong; untuk pasien yang namanya terisi, yang tampil adalah nama itu. Ini bukan perubahan perilaku task ini —
urutan `ResolvePatientName` tidak diubah, dan daftar sudah berperilaku sama. Uji putaran bersama mencatat jalur mana yang
terjadi pada data yang tersedia.

---

## 4. Dokumentasi endpoint

#### Health Services / Emergency Installation Management / Emergency Visit

Base URL: `api/v1/health-services/emergency-installation-management/emergency-visits`

| Method | Path | Perubahan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/{id}` | `patientName`, `medicalRecordNumber`, `serviceUnitName`, `arrivalModeName`, `caseTypeName` kini terisi; bentuk respons tidak berubah | `EmergencyVisit : Read` (tidak berubah) |

```yaml
GET /api/v1/health-services/emergency-installation-management/emergency-visits/{id}
summary: Detail satu kunjungan IGD beserta konteksnya
security:
  - bearerAuth: []
permission: EmergencyVisit + Read
responses:
  "200":
    description: Detail kunjungan IGD berhasil diambil — kini dengan identitas pasien, unit, cara datang, dan jenis kasus
  "404":
    description: Data kunjungan IGD tidak ditemukan.
  "403":
    description: Pengguna tidak memegang EmergencyVisit Read
```

---

## 5. Verifikasi

| Perintah atau pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Path Areas/.../EmergencyVisitController.cs -Mode Strict` | VIOLATION 0, REVIEW 0, INFO 0 — *Final result: PASS* | `PASS` |
| `git diff --stat -- Areas` (berkas task) | 1 berkas, +8/−1 | `PASS` |
| Baris tambahan berisi `//` atau `/*` | Nol | `PASS` |
| Akhiran baris dan BOM | 1053 CRLF, 0 LF tunggal, 0 CR tunggal; BOM tetap | `PASS` |
| `ToResponse` membaca kelima ruas dari navigasi | `MedicalRecordNumber = x.Patient?.MedicalRecordNumber`, `ServiceUnitName = x.ServiceUnit?.ServiceUnitName`, `ArrivalModeName = x.ArrivalMode?.Name`, `CaseTypeName = x.CaseType?.Name`, `PatientName = ResolvePatientName(x)` | `PASS` |
| `dotnet build` | Dijalankan agen penguji atas nama pemilik, 6 Oktober 2026: 0 error, 235 warning; DLL ditulis 02.31.28 UTC, backend dinyalakan 02.32.07 UTC dari build itu (`IGD-FACT-064`) | `PASS` (acceptance 6) |
| Uji API dan layar | Putaran 02.39–03.02 UTC ditolak (`IGD-DEC-213`, bagian 8); uji ulang 03.59 UTC dengan akun `dimas` diterima (`IGD-DEC-216`, bagian 8.2) | `PASS` 4/4 |

### 5.1 Perintah build

Dari `NewQuilvianSystemBackend`:

```bash
dotnet build ./QuilvianSystemBackend.csproj -p:RunAnalyzers=false
```

Hasil yang dilaporkan penguji: `Build succeeded with 235 warning(s) in 108.1s`. Agent tidak menjalankan build.

### 5.2 Skenario uji untuk panduan putaran bersama

Data dibuat lewat API atau layar dengan **akun peran nyata**, **tidak** lewat SQL; layar dilayani **hasil build**. Langkah
perawat memakai `dimas.kurniawan@rsmmc.local` (`IGD-DEC-215`); sandi dari variabel lingkungan. Untuk acceptance 1,
data uji wajib punya cara datang, jenis kasus, dan konfirmasi kedatangan agar keenam ruas benar-benar terisi.

| No | Keadaan | Harapan | Acceptance |
| ---: | --- | --- | --- |
| `064-S1` | Kunjungan pasien teridentifikasi **dengan** cara datang, jenis kasus, dan kedatangan terkonfirmasi | `GET /{id}` dan baris kunjungan itu pada `GET /?search=<nomor kunjungan>`: `patientName`, `medicalRecordNumber`, `serviceUnitName`, `arrivalModeName`, `caseTypeName`, `arrivalConfirmedByName` sama ruas demi ruas **dan tidak kosong**; `patientName` bukan *"Pasien belum teridentifikasi"* | 1 |
| `064-S2` | Kunjungan bertanda `isUnknownPatient` (bila tersedia) | `patientName` sama dengan baris daftar; dicatat jalur yang terjadi — nama pasien, alias, atau kalimat cadangan (bagian 3.4) | 2 |
| `064-S3` | Kunjungan yang menunggu penutupan; lalu `GET /{id}` dengan id acak | `isAwaitingClosure` dan `awaitingClosureReason` sama dengan daftar; id acak → `404` kalimat tetap | 3 |
| `064-U1` | Ruang Kerja kunjungan `064-S1` pada **hasil build**, 1440 × 900, akun perawat | Kartu pasien menampilkan nama dan No. RM pasien yang benar; PNG viewport; pemeriksaan teks dibatasi pada kartu pasien, bukan seluruh halaman | 4 |

---

## 6. Acceptance criteria dan Definition of Done

| # | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Ruas identitas dan konteks pada `GET /{id}` sama dengan baris kunjungan itu pada `GET /` | **Terpenuhi** — `064-S1` uji ulang, keenam ruas terisi dan sama (bagian 8.2). *Sebelumnya:* terpetakan; uji belum sah | Rangkaian `Include` `GetById` identik dengan `GET /`; putaran 6 Oktober hanya indikasi, dan `arrivalModeName`/`caseTypeName` kosong di kedua sisi (`IGD-FACT-067`) |
| 2 | Pasien tanpa identitas: alias bila ada, selain itu kalimat cadangan; RM kosong | **Terpenuhi dengan catatan bagian 3.4** — `064-S2` uji ulang: jalur `Patient.FullName`, sama dengan daftar | Indikasi 6 Oktober: jalur `Patient.FullName`, sama dengan daftar |
| 3 | Regresi `isAwaitingClosure`/`awaitingClosureReason`; `404` tetap | **Terpenuhi** — `064-S3` uji ulang | Indikasi 6 Oktober: sama dengan daftar; id acak `404` kalimat tetap |
| 4 | Kartu pasien Ruang Kerja benar pada hasil build, 1440 × 900 | **Terpenuhi dengan catatan** — `064-U1` uji ulang menampilkan nama, No. RM, unit, cara datang, dan jenis kasus; build yang melayani tidak dapat dibuktikan (`IGD-FACT-069`) | Indikasi 6 Oktober dilayani `next dev` dengan akun SuperAdmin (`IGD-FACT-065`, `066`) |
| 5 | Diff satu berkas, hanya `GetById`; nol `Program.cs`, DTO, migration, endpoint; nol komentar baru | **Terpenuhi** | Bagian 3.2 dan 5 |
| 6 | Build 0 error | **Terpenuhi** | Build 6 Oktober: 0 error, 235 warning; backend berjalan dari DLL itu (`IGD-FACT-064`) |

**Definition of Done:** laporan tracked ✅; register, node grafik R3.15 dan ringkasan, serta traceability ditandai ✅; QBE
preflight dan checker ✅; tanpa UAT PASS ✅. Uji acceptance 1–4 terbukti pada uji ulang 6 Oktober (sore), diterima `IGD-DEC-216`. Seluruh acceptance 1–6 terpenuhi. *Riwayat:* status ✅ pertama yang ditulis agen penguji ditarik (`IGD-DEC-213`).

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nihil dari checker. Build: 235 warning (tidak dirinci penguji) |
| Masalah yang diketahui | (1) Bagian 3.4 — untuk pasien tanpa identitas yang namanya diisi Registrasi, alias IGD tidak tampil di detail maupun daftar; perilaku lama `ResolvePatientName`, tidak diubah. (2) `QBE-SVC-001` — akses `ApplicationDbContext` langsung di controller (pola lama), tidak dipindahkan karena di luar cakupan kartu |
| Keadaan migration | Nol |
| Eksekusi database | Nol — agent tidak menjalankan kueri apa pun |
| Risiko tersisa | Rendah. Satu baris dengan empat join tambahan; data yang ditampilkan sudah terbaca di daftar oleh pemegang hak yang sama |
| Perubahan sampingan | `NONE`. Perubahan `BE-IGD-041` dan dokumen 6 Oktober pada working tree bukan hasil task ini |
| Interupsi | `NONE` |
| Langkah berikutnya | (1) `build-module-frontend` `FE-IGD-044` atas izin pemilik. (2) Satu panduan uji agent untuk putaran bersama — termasuk bagian 5.2, dengan akun perawat `dimas.kurniawan@rsmmc.local` (`IGD-DEC-215`) |

`git status --short` di akhir pekerjaan (source):

```text
 M Areas/HealthServices/EmergencyInstallationManagement/Controllers/EmergencyVisitController.cs
 M Areas/HealthServices/EmergencyInstallationManagement/Services/EmergencyDepartureService.cs
```

`EmergencyDepartureService.cs` milik `BE-IGD-041`, bukan task ini.
---

## 8. Pemeriksaan bukti uji — 6 Oktober 2026 (siang)

Uji dijalankan agen Antigravity atas nama pemilik, **tanpa** panduan dari agent. Agent memutus dari bukti mentah
`QuilvianSystemFrontendDev/test-with-agy/igd/` (`results-be-igd-064.json`, `test-be-igd-064.mjs`, `064-U1.png`) dicocokkan
dengan `Logs/quilvian-backend-20261006.json`, bukan dari ringkasan penguji. Keputusan pemilik: `IGD-DEC-213`, `214`, `215`.

| # | Temuan | Aturan | Akibat |
| ---: | --- | --- | --- |
| a | Seluruh permintaan 02.39–03.02 UTC dikirim `superadmin@admin.com`, termasuk sesi layar `064-U1` | A3 | Bukti acceptance ditolak (`IGD-DEC-213`) |
| b | Layar dilayani `next dev` (`.next/dev` dibuat 09.57 WIB), bukan hasil build | A1 | Acceptance 4 belum sah |
| c | Empat putaran — 02.39 dan 02.43 gagal (`start-triage` `400`, lalu `404` atas id `undefined`), 02.45, 03.02 — hanya yang terakhir dilaporkan | A11 | Dicatat |
| d | `arrival-modes` dan `case-types` dijawab `404`, sehingga `arrivalModeName`, `caseTypeName`, dan `arrivalConfirmedByName` kosong di detail maupun daftar | — | Dua ruas acceptance 1 tidak terbukti; skenario 5.2 mewajibkan ruas terisi |
| e | Alamat surel dan sandi SuperAdmin tertulis literal di skrip | A5, `IGD-DEC-189` | Dicatat; skrip tidak disunting (`IGD-DEC-215`); folder di-*ignore* git |
| f | Backend dibuild agen penguji | A13 | Build diterima untuk acceptance 6 — DLL dan backend konsisten |
| g | Dua berkas CSS frontend diubah penguji agar `next dev` mau mengompilasi; selektor baru ikut mengenai daftar Pengkajian | A13 | **Dikembalikan** ke commit `2a985f6d5` (`IGD-DEC-214`); `git status` frontend bersih |
| h | Pemeriksaan teks layar `064-U1` membaca seluruh halaman, termasuk menu samping | A6 | Skenario 5.2 membatasi pemeriksaan pada kartu pasien |
| i | Laporan task ini ditandai ✅ oleh penguji | Aturan pemilik: laporan task hanya lewat `build-module` | Ditulis ulang agent (pola `IGD-DEC-212`) |

**Indikasi yang tercatat — bukan bukti acceptance.** Pada kunjungan `IGD-261006030206-5CF953`, detail dan daftar sama-sama
menampilkan nama pasien terdaftar, No. RM `00-00-02-36`, dan unit *Instalasi Gawat Darurat*; kunjungan bertanda
`isUnknownPatient` menampilkan `Patient.FullName` di kedua sisi; id acak dijawab `404` dengan kalimat tetap. Hasil ini
selaras dengan source, tetapi baru sah sesudah diulang dengan akun peran nyata pada hasil build.

### 8.2 Uji ulang — 6 Oktober 2026 (sore)

Putaran resmi 03.59.11–03.59.26 UTC (`results-be-igd-064.json`), diperiksa agent pada bukti mentah dan log backend.
Keputusan pemilik: `IGD-DEC-216`, `217`.

| Skenario | Putusan agent | Bukti yang menentukan |
| --- | --- | --- |
| `064-S1` | **Terbukti** — acceptance 1 | Kunjungan `IGD-261006035912-71E65B`: `patientName` nama pasien terdaftar, `medicalRecordNumber` `00-00-02-47`, `serviceUnitName` *Instalasi Gawat Darurat*, `arrivalModeName` *Rujukan Faskes*, `caseTypeName` *Korban Bencana / Massal*, `arrivalConfirmedByName` *Dimas Kurniawan* — terisi dan sama antara `GET /{id}` dan `GET /` |
| `064-S2` | **Terbukti** — acceptance 2 (dengan catatan bagian 3.4) | Kunjungan `isUnknownPatient` beralias sementara menampilkan `Patient.FullName` di detail dan daftar |
| `064-S3` | **Terbukti** — acceptance 3 | `isAwaitingClosure` dan `awaitingClosureReason` sama dengan daftar; id acak `404` *"Data kunjungan IGD tidak ditemukan."* |
| `064-U1` | **Terbukti** — acceptance 4 (build yang melayani tidak dapat dibuktikan) | Kartu pasien menampilkan nama, No. RM `00-00-02-47`, unit, cara datang, dan jenis kasus; sesi *"Dimas Kurniawan"*; PNG 1440 × 900 |

Seluruh permintaan putaran resmi dikirim akun `dimas.kurniawan@rsmmc.local`.

| # | Penyimpangan | Aturan | Penanganan |
| ---: | --- | --- | --- |
| a | `superadmin@admin.com` dipakai untuk `POST /administrator/setting/role-access/policies` empat kali (03.43–03.57 UTC, `grant-dimas.mjs`): Read/Create/Update pada `EmergencyArrivalMode`, `EmergencyCaseType`, `Patient`, `PatientEncounter`, `EmergencyDisposition` untuk departemen dan posisi akun `dimas` — termasuk `EmergencyDisposition : Create` yang dicabut dari Perawat IGD oleh `IGD-DEC-190` | A3, A4 | Diterima pemilik sebagai konfigurasi sementara; disempurnakan pengurus Akses Role (`IGD-DEC-216`) |
| b | Belasan percobaan 03.38–03.58 UTC tidak dilaporkan (`403` sebelum izin ditambah, `start-triage` `400`, `arrival-time` `409`) | A11 | Dicatat |
| c | Sandi akun `dimas` ditulis literal di skrip | A5, `IGD-DEC-215` | Dicatat; folder di-*ignore* git |
| d | `BUILD_ID` frontend tertimpa build 11.02 WIB, sesudah putaran; `.next/dev` masih ada | A1 | Dicatat — layar tidak mengubah kebenaran perilaku backend yang diuji |
| e | Dua QR code tracked di `Storage/uploads/patient-qrcodes` dihapus penguji | A13 | **Dipulihkan** dari HEAD (`IGD-DEC-217`) |
