# Laporan Perubahan Backend — `BE-LAB-82`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-82` |
| Judul | Batas waktu cito dan disiplin yang dapat dirilis di satu tempat |
| Slice | Gelombang `MVP-11a` — **task pertama, tersendiri** |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian **6an.1** |
| Trace | `FR-17.4`, `FR-17.5`; `INV-57`; `ARCH-GAP-LAB-11`; `02-backend-architecture.md` 23.10 butir 5 |
| Contract version | `LAB-API-v1` **`r37`** 32.4 baris ketiga (*"nol perubahan perilaku"*); rancangan bagian 23.4 — **`approved` 2026-09-28** |
| Dependency | `BE-LAB-73` ⚠ (penjaga disiplin di `LabResultValidationService`), `BE-LAB-77` ✅ (`LabWorklistService.cs`) |
| Klasifikasi | `MEDIUM` — skor 7: repository 0, berkas diperiksa 1 (±12), berkas diubah 2 (5), logika bisnis 1 (pemindahan tanpa perubahan), kontrak API 0, database 1 (kueri baca dipindah), keamanan/auth 1 (penjaga klinis `VAL-126` disentuh), UI/workflow 0. Risiko roadmap **Sedang** — seluruh gelombang `MVP-11` bersandar pada task ini |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/LaboratoryManagement/`, `Program.cs`, dokumen blueprint Laboratorium |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `017d1819` (branch `yoga`), di atas `BE-LAB-67`..`81` yang belum ter-commit. Rancangan gelombang disusun pada `4a94628a`; **impact scan:** 30 commit, yang menyentuh Laboratorium hanya `LabExaminationController`/`LabExaminationService` dan yang menyentuh `Program.cs` hanya modul Gizi dan Akuntansi — **nol** pada berkas task ini. `BatasWaktuCitoAsync` kini di baris 399, bukan 240 (bergeser oleh `BE-LAB-77`) |
| Tanggal | 2026-09-30 |
| Status | ✅ **`SELESAI`** — **nol perubahan perilaku, terbukti dua cara.** Uji karakterisasi ditulis dan lulus **sebelum** pemindahan (12/12, 48 baris pengamatan), lalu dijalankan ulang **tanpa disunting** (SHA-256 berkas uji sama) — keluarannya **identik baris per baris**. Panggilan HTTP terhadap PostgreSQL sebelum dan sesudah — daftar pantau cito dan enam penolakan `VAL-126` — juga **identik**. Uji unit `GetLimitsAsync` 13/13; regresi `BE-LAB-69`, `72`..`77`, `81` utuh; build 0 error tanpa warning baru; registri tetap 1575 |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `LaboratoryManagement` |
| Pemilik / prefix registry | `Lab` — `ACTIVE` |
| Keberlakuan | `NEW CODE` — `Services/LabCitoTurnaroundPolicy.cs`, `Constants/LabReleasableDisciplines.cs`. `TOUCHED LEGACY` — `LabWorklistService`, `LabResultValidationService`, `Program.cs` |
| QBE yang berlaku | `QBE-SVC-001` (aturan bersama sebagai service terdaftar DI, bukan pembantu statis yang membuka `DbContext` sendiri). **Tidak berlaku:** `QBE-API-*` dan `QBE-DTO-*` (nol endpoint, nol ruas), `QBE-PERM-*` (nol aksi), `QBE-LOG-001` (nol perubahan state), `QBE-ENT-*`/`QBE-DB-*` (nol entity, nol migration) |
| Governance yang dibaca | `AGENTS.md` backend; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` (baris `Lab` `ACTIVE`) |

---

## 1. Masalah yang diperbaiki

**Dua jawaban yang harus tunggal akan segera punya dua pemakai.** Laporan waktu penyelesaian
(`BE-LAB-85`) menilai *terlambat* dengan batas cito, dan laporan jumlah pemeriksaan (`BE-LAB-83`)
harus tahu disiplin mana yang sudah dapat dirilis. Kedua jawaban itu hari ini terkunci di dalam
kelas lain:

- **Batas cito** di dalam fungsi privat `LabWorklistService.BatasWaktuCitoAsync`.
- **Disiplin yang dapat dirilis** tertulis langsung di penjaga `VAL-126` pada `LabResultValidationService`.

Menyalinnya ke laporan berarti dua rumus yang akan bercabang tanpa satu galat pun. Daftar pantau
lalu menyebut Kalium terlambat sementara laporan menyebutnya tepat waktu (`INV-57`), atau laporan
menulis 0 pemeriksaan bagi disiplin yang memang belum dapat dirilis (`ARCH-GAP-LAB-11`).

---

## 2. Proses bisnis

**Tidak ada yang berubah bagi pengguna mana pun**, dan itu inti task ini.

- **Daftar pantau cito** menyebut pemeriksaan yang sama, dengan batas, tenggat, dan keterangan yang
  sama. Aturan pemilihan batas tetap:
  - baris umum — *semua jenis kelamin, tanpa kelompok umur* — bila baris itu mengisi batas cito;
  - bila tidak, batas **terkecil** di antara baris aktif lain (janji yang paling ketat).
- **Validasi, rilis, dan *Kembalikan*** tetap hanya untuk Patologi Klinik. Mikrobiologi dan Patologi
  Anatomi tetap ditolak dengan bunyi `VAL-126` yang sama kata per kata.

Yang berubah hanya **tempat** kedua jawaban itu, supaya laporan `BE-LAB-83`..`85` memakai jawaban yang
sama persis.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / dokumen | Kenapa dibaca |
| --- | --- |
| `roadmap/backend-roadmap.md` 6an, 6an.1 | Cakupan, tiga jebakan, verifikasi, DoD |
| `02-backend-architecture.md` 23.4, 23.10 butir 5 | Letak, nama, dan pemakai kedua kelas; batas yang berlaku saat dibuka |
| `testing/acceptance-test-matrix.md` — *Matriks — pemindahan tanpa perubahan perilaku*; baris `AC-17`, `VAL-126` | Skenario karakterisasi |
| `contracts/validation-matrix.md` `VAL-126` | Bunyi pesan yang wajib tetap |
| `Services/LabWorklistService.cs` | Fungsi yang dipindah dan pemanggilnya |
| `Services/LabResultValidationService.cs` | Penjaga disiplin |
| Seluruh `Services/*.cs` dan `Controllers/*.cs` Laboratorium — `LabDiscipline.ClinicalPathology`, `CitoTurnaroundMinutes` | Memastikan tidak ada penjaga *"dapat dirilis"* atau pembaca batas cito lain yang terlewat |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Services/LabCitoTurnaroundPolicy.cs` | **Baru.** `GetLimitsAsync(procedureIds, ct)` — isi `BatasWaktuCitoAsync` **apa adanya**, termasuk aturan pemilihannya. Terdaftar di DI |
| `Constants/LabReleasableDisciplines.cs` | **Baru.** `Contains(LabDiscipline)`. Isi hari ini: Patologi Klinik |
| `Services/LabWorklistService.cs` | Menerima `LabCitoTurnaroundPolicy` lewat konstruktor dan memanggilnya. Fungsi privat lama **dihapus** — nol salinan |
| `Services/LabResultValidationService.cs` | Penjaga `VAL-126` membaca `LabReleasableDisciplines`. Disiplin kosong tetap ditolak. Pesan **tidak berubah** |
| `Program.cs` | `AddScoped<LabCitoTurnaroundPolicy>()` |

**Nol endpoint, nol ruas, nol entity, nol migration, nol aksi hak akses.**

**Ketiga jebakan roadmap.**

| Jebakan | Bagaimana dihindari | Bukti |
| --- | --- | --- |
| (a) *Merapikan* aturan pemilihan batas saat memindah | Isi fungsi disalin baris per baris, termasuk `FirstOrDefault` atas baris umum. Aturannya kini tertulis di dokumentasi kelas dengan peringatan *jangan dirapikan tanpa keputusan* | Karakterisasi identik; uji unit tujuh bentuk batas |
| (b) Meninggalkan salinan fungsi lama | Fungsi privat dihapus; `LabWorklistService` hanya punya satu konstruktor, yang menuntut kebijakan baru | Refleksi: nol metode batas pada `LabWorklistService` |
| (c) Urutan terhadap `BE-LAB-78` | Task ini datang **lebih dulu**. Himpunan berisi Patologi Klinik saja; `BE-LAB-78` menambahkan Mikrobiologi **pada `LabReleasableDisciplines`**, bukan pada penjaga di service — dicatat di dokumentasi kelas dan roadmap 6al.1 | — |

**Di luar cakupan, sengaja tidak disentuh.**

| Tempat | Kenapa |
| --- | --- |
| Penyaring Patologi Klinik antrean validasi (`LabWorklistService.GetValidationQueueAsync`) | `r34` 29.4 menetapkannya tetap; perluasannya milik `BE-LAB-80` |
| `LabDiscipline.ClinicalPathology` pada pembaca kewenangan orang (`LabResultValidationService`) | Kode kewenangan per disiplin milik `BE-LAB-78` |
| `resultProgress` khusus Patologi Klinik (`LabMonitoringService`, `LabOrderService`) | Milik `BE-LAB-79` |
| Lembar hasil per order khusus Patologi Klinik (`VAL-123`) | Aturan lain — bentuk lembar, bukan soal dapat dirilis |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Nol** — sesuai `r37` 32.4 baris ketiga |
| Database | **Nol perubahan.** Kueri batas cito yang sama, dari kelas lain; tetap satu kueri per halaman daftar pantau |
| Keamanan/Auth | Penjaga klinis `VAL-126` disentuh **tanpa** mengubah himpunan yang diterima. Kode kewenangan per disiplin (`LabClinicalPrivilegeCodes`) tetap lapis kedua yang menolak Mikrobiologi dan PA, seandainya penjaga ini keliru |

---

## 4. Dokumentasi endpoint

**Nol endpoint baru atau berubah.** Dua endpoint yang **perilakunya dibuktikan tetap**:

| Method | Path | Hak akses | Bukti tetap |
| --- | --- | --- | --- |
| `GET` | `/lab-worklists/cito-overdue` | `LabWorklist : Read` | HTTP sebelum = sesudah |
| `POST` | `/lab-examinations/{id}/result/validate`, `/release`, `/return` | `LabExaminationResult : Validate` / `Release` / `Return` | Enam penolakan `VAL-126` sebelum = sesudah |

---

## 5. Verifikasi

**Urutan sesuai DoD:** uji karakterisasi ditulis → dijalankan pada kode **lama** dan keluarannya
disimpan → acuan HTTP diambil dari aplikasi berkode lama → kode dipindah → build → uji karakterisasi
dijalankan ulang **tanpa disunting** → HTTP diulang → keduanya dibandingkan.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| **Uji karakterisasi, putaran 1 — kode lama** | **12 `PASS`, 0 `FAIL`**; 48 baris pengamatan disimpan | `PASS` | `hasil-sebelum.txt` |
| **Uji karakterisasi, putaran 2 — kode baru, berkas uji tidak disunting** | **12 `PASS`, 0 `FAIL`**; `sha256sum -c` atas berkas uji `OK`; `diff` 48 baris → **identik** | `PASS` | `hasil-sesudah.txt` |
| **HTTP terhadap PostgreSQL, sebelum dan sesudah** | `diff` → **identik** (9 baris) | `PASS` | Rincian di bawah |
| Uji unit `LabCitoTurnaroundPolicy` dan `LabReleasableDisciplines` | **13 `PASS`, 0 `FAIL`** | `PASS` | Rincian di bawah |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` | **0 error, 230 warning**; **nol** dari berkas yang disentuh; 70 detik | `PASS` | Keluaran build |
| Regresi | `BE-LAB-69` 31/31, `72` 43/43, `73` 38/38, `74` 28/28, `75` 20/20, `76` 16/16, `77` 21/21, `81` 25/25 | `PASS` | Harness `BE-LAB-77` disesuaikan satu baris — konstruktor `LabWorklistService` kini tiga parameter |
| Startup Development | Registri tetap **1575**; nol galat di luar yang sudah dikenal | `PASS` | Log startup |
| Tinjauan kode — `grep CitoTurnaroundMinutes` di luar `LabCitoTurnaroundPolicy.cs`, `LabValueBound*`, dan DTO | **Satu baris**: `CitoTurnaroundMinutes = menit` di `LabWorklistService.Baris` — **pengisian ruas respons** daftar pantau, bukan pembacaan batas. Pembaca batas di luar kebijakan dan layanan data induknya: **nol** | `PASS` dengan catatan | `grep` |

**Uji karakterisasi** — harness EF InMemory. Service dibangun lewat DI yang **memindai** kelas service
Laboratorium, sehingga konstruktor yang bertambah parameter tidak memaksa berkas uji disunting. Jam
acuan tetap: 10.20 WIB.

| Skenario | Keluaran — identik pada kedua putaran |
| --- | --- |
| `AC-17` baris 1 — Kalium 60 menit, layak 09.00, belum dirilis 10.20 | Terlambat **20** menit |
| `AC-17` baris 2 — Kalium dirilis 09.45 | Tidak muncul |
| Cito tanpa batas | Tampil, tidak terlambat, *"Batas waktu cito untuk pemeriksaan ini belum diatur."* |
| Batas bertingkat — umum 45, kelompok umur 30, pria 25 | **45** |
| Baris umum tanpa batas cito — kelompok umur 50, wanita 35 | **35** |
| Baris umum nonaktif 20 — kelompok umur 40 | **40** |
| Baris umum terhapus 15 — kelompok umur 70 | **70** |
| Seluruh baris nonaktif | Tanpa batas |
| Belum lewat batas; rutin; belum layak | Tidak muncul |
| Urutan daftar | Terlambat terlama lebih dulu, tanpa batas di bawah — 7 baris |
| `VAL-126` — validasi, rilis, *Kembalikan* atas Mikrobiologi Final, Patologi Anatomi, order lama berkatalog PA, order lama berkatalog Mikrobiologi, order dan katalog tanpa disiplin, Mikrobiologi gugur | **18 × `422`** dengan bunyi `VAL-126` kata per kata — termasuk keenam baris 638 matriks uji |
| Patologi Klinik Draft; PK gugur; order lama berkatalog PK gugur | Lolos `VAL-126`; ditolak aturan berikutnya dengan bunyi yang sama pada kedua putaran |

**Rincian HTTP** — aplikasi sungguhan, database dev bersama, superadmin, **nol penulisan** (penjaga
menolak sebelum apa pun ditulis).

| Panggilan | Sebelum = sesudah |
| --- | --- |
| `GET /lab-worklists/cito-overdue` | 2 baris — Kalium (batas 30, tenggat 06.00 UTC) dan Hemoglobin (batas 60, tenggat 06.20 UTC); seluruh ruas sama, kecuali `overdueMinutes` yang bergantung jam dinding dan sengaja dikeluarkan dari perbandingan |
| `POST …/result/validate`, `/release`, `/return` atas Histopatologi Biopsi Besar (PA, LAB-RSMMC-000009) | 3 × `422` `VAL-126` |
| Sama, atas Pewarnaan BTA Sputum (Mikrobiologi, LAB-RSMMC-000014) | 3 × `422` `VAL-126` |

**Uji unit.**

| Skenario | Hasil |
| --- | --- |
| `GetLimitsAsync` — tujuh bentuk batas, ditambah pemeriksaan tanpa baris batas | Nilai tepat; setiap id muncul pada hasil (8/8) |
| Hanya baris pria 25 dan wanita 35, tanpa baris umum | 25 — terkecil |
| `LabWorklistService` — metode batas cito | **Nol**; konstruktor menuntut `LabCitoTurnaroundPolicy` |
| `LabReleasableDisciplines` | Patologi Klinik `true`; Mikrobiologi dan Patologi Anatomi `false` |

**Tidak dijalankan:**

- Batas bertingkat terhadap PostgreSQL — dev nol punya jenis pemeriksaan berbatas cito bertingkat, dan
  menyiapkannya berarti menulis data induk bersama. Terbukti pada harness.
- Analyzer — build memakai `-p:RunAnalyzers=False`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Matriks pemindahan — `AC-17` sesudah pemindahan, ditambah cito tanpa batas | ✅ **Identik** | Karakterisasi dan HTTP |
| Matriks pemindahan — batas cito bertingkat | ✅ **Sama dengan fungsi lama** | Karakterisasi dan uji unit |
| Matriks pemindahan — `VAL-126` sesudah pemindahan | ✅ **Identik** — baris `S4`. Baris `S4d-1` berlaku sesudah `BE-LAB-78` | Karakterisasi (18 penolakan) dan HTTP (6) |
| Verifikasi roadmap — daftar pantau identik sebelum dan sesudah pada `asOf` yang sama | ✅ | Karakterisasi |
| Verifikasi roadmap — `VAL-126` kata per kata | ✅ | Karakterisasi dan HTTP |
| Verifikasi roadmap — `grep CitoTurnaroundMinutes` → nol | ✅ **dengan catatan** — satu pengisian ruas DTO, nol pembaca | 5 |
| DoD — dua kelas bersama berdiri; nol salinan; karakterisasi lulus sebelum dan sesudah tanpa disunting; nol migration; nol endpoint; laporan memuat kedua putaran | ✅ **Terpenuhi** | — |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 230 warning compiler, **nol** dari berkas yang disentuh |
| Masalah yang diketahui | **1.** Aturan pemilihan memakai `FirstOrDefault` tanpa urutan bila satu jenis pemeriksaan punya **lebih dari satu** baris umum aktif berbatas cito. Hasilnya bergantung urutan baris dari basis data. Perilaku ini sudah ada dan **sengaja dipindah apa adanya**; bila perlu ditutup, itu keputusan pemilik modul, dan dampaknya kena daftar pantau dan laporan sekaligus. **2.** Nama `EnsureClinicalPathologyAndRunning` masih akurat hari ini; `BE-LAB-78` sebaiknya menamainya ulang saat Mikrobiologi masuk |
| Risiko tersisa | **Rendah.** Perilaku terbukti tetap dengan dua cara independen |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Berkas `BE-LAB-82`: `??` `Services/LabCitoTurnaroundPolicy.cs`, `Constants/LabReleasableDisciplines.cs`, laporan ini; ` M` `Services/LabWorklistService.cs`, `Program.cs`, `roadmap/backend-roadmap.md`, `roadmap/traceability.md`. `Services/LabResultValidationService.cs` masih `??` sejak `BE-LAB-73`. Perubahan `BE-LAB-67`..`81` yang belum ter-commit ikut ada. **Nol operasi Git dijalankan** |
| Langkah berikutnya | **1.** `BE-LAB-83` (laporan jumlah pemeriksaan, penyaring periode, izin laporan) naik menjadi `SIAP DIKERJAKAN`. **2.** `BE-LAB-78` tetap `SIAP DIKERJAKAN`; Mikrobiologi kini wajib masuk lewat `LabReleasableDisciplines`, dan baris `VAL-126` `S4d-1` pada matriks pemindahan dibuktikan di sana. **3.** `BE-LAB-85` tetap menunggu `BE-LAB-84`; pendahulu `BE-LAB-82`-nya sudah terpenuhi |
