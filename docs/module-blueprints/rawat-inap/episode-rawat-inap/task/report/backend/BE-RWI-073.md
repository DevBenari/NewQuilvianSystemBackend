# Laporan Perubahan Backend — `BE-RWI-073`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-RWI-073` |
| Judul | Kelayakan tempat tidur berhenti menilai penghuni kamar lain |
| Slice | `S9. Gelombang 1A — Rawat Inap Safety Corrections` |
| Roadmap | `docs/module-blueprints/rawat-inap/episode-rawat-inap/roadmap/backend-roadmap.md` bagian `S9` |
| Trace | `RWI-DEC-101`; `RWI-DEC-104`; `RWI-DEC-105`; `RWI-DEC-066`; `FR-RI-179` s.d. `FR-RI-184`; `MVP-RWI-D-002`; `04-prd-to-mvp.md` bagian 21.3 dan 21.5 |
| Contract version | `api-contract.md` `0.8.0` — **`approved`** 11 September 2026 oleh Muhammad Hamzah lewat `RWI-DEC-105`. `validation-matrix.md` `0.8.0` — **`approved`** pada tanggal yang sama. `02-backend-architecture.md` revision `0.7` bagian 0.1 |
| Dependency | Approval kontrak `0.8.0` — **sudah lepas** 11 September 2026. Nol prasyarat task lain |
| Klasifikasi | `MEDIUM` — skor 7. Repository 0, berkas diperiksa 1, berkas diubah 1, logika bisnis 1, kontrak API 2, database 1, keamanan/auth 1, UI/workflow 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — satu berkas source pada `Areas/HealthServices/InPatientManagement/`, ditambah laporan ini, baris status pada `roadmap/backend-roadmap.md`, dan baris bukti pada `roadmap/requirement-traceability.md` |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `c11904ea99dc0f9a9ecfb3b41f6edb2bf500d206` pada branch `MHamzah` |
| Tanggal | 11 September 2026 |
| Status | **SELESAI 11 September 2026.** Kedelapan acceptance criteria terbukti pada source. Dua butir verifikasi tidak dijalankan dan ditulis apa adanya: `dotnet build` dikecualikan atas keputusan pemilik, dan kedua integration test tidak dapat dijalankan karena backend tidak memelihara project test. Satu pekerjaan lintas repository tetap terbuka sebagai gerbang rilis |

---

## 1. Masalah yang diperbaiki

Sebelum perubahan ini, sistem menolak pasien masuk sebuah kamar karena **siapa yang sudah ada di
dalamnya**, bukan karena tempat tidur yang dituju memang tidak cocok.

Contoh nyata. Kamar Melati 1 berisi tiga tempat tidur, dan Admin Master Data sudah menandai
ketiganya menerima laki-laki maupun perempuan. Pukul 08:00 Tn. Budi menempati `MELATI-01-A`.
Pukul 09:00 petugas admisi hendak menempatkan Ny. Sari di `MELATI-01-B` — tempat tidur yang
berbeda, di kamar yang sama. Sistem menolak dengan pesan "Kamar Melati 1 sedang dihuni pasien
laki-laki, sehingga tidak dapat menerima pasien perempuan."

Akibatnya di lapangan ada tiga:

1. **Keputusan privasi berubah menjadi soal siapa datang lebih dulu.** Satu pasien laki-laki
   menutup seluruh kamar bagi pasien perempuan, walaupun kamar itu memang dirancang menerima
   keduanya.
2. **Tempat tidur kosong terbaca penuh.** Pada layar pemilihan tempat tidur, dua tempat tidur
   yang benar-benar bebas muncul sebagai tidak dapat dipilih.
3. **Petugas admisi tidak punya jalan keluar yang benar.** Satu-satunya cara memasukkan pasien
   berikutnya adalah memindahkan pasien yang sudah dirawat — tindakan klinis yang dilakukan hanya
   demi mengakali aturan.

Masalah kedua ada pada pasien yang jenis kelaminnya belum tercatat. Pasien seperti itu dulu
menuntut **kamar yang belum ada penghuninya sama sekali**, sehingga pasien tidak dikenal identitas
lengkapnya — justru yang paling sering datang lewat keadaan darurat — adalah pasien yang paling
sulit ditempatkan.

Kedua hal itu tercatat sebagai temuan `P0` nomor satu pada `PRD-to-MVP-Rawat-Inap-V2`, lalu
diputuskan dicabut lewat `RWI-DEC-101` dan disahkan menjadi kontrak `0.8.0` lewat `RWI-DEC-105`.

---

## 2. Proses bisnis

**Tujuan.** Menempatkan pasien rawat inap pada tempat tidur yang layak, dan menolak yang tidak
layak beserta alasan yang dapat dibaca petugas.

**Pelaku.** Petugas admisi, perawat ruangan, dan supervisor — masing-masing menurut hak akses yang
diberikan admin pada layar Pengaturan → Manajemen Role → Akses Role. Hak aksesnya **tidak berubah**
oleh task ini.

**Pemicu.** Salah satu dari empat jalur: pencarian tempat tidur, pemesanan tempat tidur,
penempatan pasien pertama kali, dan perpindahan pasien. Keempatnya memanggil **satu** pemeriksaan
kelayakan yang sama, yaitu `EvaluatePlacementEligibilityAsync`.

**Langkah berurutan pada jalur normal.**

1. Petugas membuka admisi, sehingga episode berstatus `Draft` terbentuk.
2. Petugas mencari tempat tidur. Setiap tempat tidur kandidat diperiksa kelayakannya satu per satu.
3. Tempat tidur yang lolos masuk daftar pilihan; yang tidak lolos disebutkan beserta aturan yang
   menolaknya — kemampuan yang dibuka `BE-RWI-069`.
4. Petugas memilih satu tempat tidur, lalu memesan atau langsung menempatkan pasien.
5. Pemeriksaan kelayakan dijalankan ulang di dalam transaksi penyimpanan, sehingga hasil pencarian
   yang sudah basi tidak dapat dipakai menerobos aturan.
6. Penempatan tersimpan, dan salinan status pada `MstBed` diperbarui.

**Aturan yang berlaku sesudah perubahan ini.** Daftar aturannya bernomor, dan nomornya ikut
terkirim pada `failures[]` supaya layar dapat mengenali aturan mana yang menolak.

| Nomor | Kode | Kapan menolak | Kode status |
| ---: | --- | --- | ---: |
| 1 | `BED_INACTIVE`, `BED_CLOSED`, `BED_NOT_RESERVABLE` | Tempat tidur tidak aktif, sedang ditutup, atau tidak dapat dipesan | 422 |
| 2 | `BED_RESERVED_BY_OTHER`, `BED_OCCUPIED_BY_OTHER` | Tempat tidur sedang dipegang episode lain | 409 |
| 3 | — | **Bukan penolakan.** Pemesanan milik episode ini yang masih berlaku dipakai ulang | — |
| 4 | `BED_GENDER_MISMATCH` | Penanda tempat tidur tidak menerima jenis kelamin pasien | 422 |
| 5 | `PATIENT_GENDER_UNKNOWN` | Jenis kelamin pasien belum tercatat **dan** tempat tidur tidak menerima keduanya sekaligus | 422 |
| 6 | — | **Dipensiunkan `RWI-DEC-101`.** Nomornya dibiarkan kosong dan tidak dipakai ulang | — |
| 7 | `ISOLATION_REQUIRED` | Pasien butuh isolasi, tempat tidurnya bukan tempat tidur isolasi | 422 |
| 8 | `ISOLATION_BED_RESERVED` | Pasien tidak butuh isolasi, tempat tidurnya tempat tidur isolasi | 422 |

**Jalur tidak normal.**

- **Pasien laki-laki ke tempat tidur bertanda perempuan saja.** Aturan 4 menolak dengan 422 dan
  pesan "Tempat tidur ini hanya untuk pasien laki-laki." atau padanannya. **Tidak berubah.**
- **Pasien tanpa jenis kelamin tercatat ke tempat tidur satu jenis kelamin.** Aturan 5 menolak
  dengan 422. **Tidak berubah.** Yang dicabut hanya syarat tambahan tentang penghuni kamar.
- **Pasien butuh isolasi ke tempat tidur biasa, dan sebaliknya.** Aturan 7 dan 8 menolak dengan
  422. **Tidak berubah.**
- **Tempat tidur sudah dipegang pasien lain.** Aturan 2 menolak dengan 409, dan isian admisi
  petugas tetap tersimpan.
- **Boks bayi.** Menempatkan pasien **ke** tempat tidur bertanda `IsForNewborn` melewati aturan 4
  dan 5 seluruhnya, sehingga bayi laki-laki tetap boleh menempati boks di kamar ibunya. Aturan
  isolasi tetap diperiksa.

**Hasil akhir.** Kamar berisi pasien laki-laki kini menerima pasien perempuan pada tempat tidur
yang memang dikonfigurasi menerima keduanya. Privasi jenis kelamin sepenuhnya bersandar pada
penanda `IsForMale` dan `IsForFemale` milik **tempat tidur**, yang ditetapkan Admin Master Data
secara sengaja — bukan pada akibat sampingan dari urutan kedatangan pasien.

### 2.1 Contoh berangka jalur normal dan tidak normal

Kamar Melati 1, tiga tempat tidur, seluruhnya `IsForMale = true` dan `IsForFemale = true`, bukan
tempat tidur isolasi. Tn. Budi menempati `MELATI-01-A` sejak pukul 08:00.

| Keadaan yang diuji | Sebelum `0.8.0` | Sesudah perubahan ini | Aturan yang menentukan |
| --- | --- | --- | --- |
| Ny. Sari ke `MELATI-01-B` | Ditolak 422 `ROOM_GENDER_MIXED` | **Berhasil** | Aturan 4 lolos karena `IsForFemale = true`; aturan 6 sudah tidak ada |
| Pasien tanpa jenis kelamin tercatat ke `MELATI-01-B` | Ditolak 422 `PATIENT_GENDER_UNKNOWN` karena kamar berpenghuni | **Berhasil** | Aturan 5 lolos karena kedua penanda tempat tidur benar |
| Pasien tanpa jenis kelamin tercatat ke tempat tidur `IsForMale = false` | Ditolak 422 `PATIENT_GENDER_UNKNOWN` | **Tetap ditolak** 422 `PATIENT_GENDER_UNKNOWN` | Aturan 5, klausa `!bed.IsForMale` |
| Tn. Budi ke tempat tidur `IsForMale = false`, `IsForFemale = true` | Ditolak 422 `BED_GENDER_MISMATCH` | **Tetap ditolak** 422 `BED_GENDER_MISMATCH` | Aturan 4 |
| Episode `RequiresIsolation = true` ke tempat tidur `IsIsolationBed = false` | Ditolak 422 `ISOLATION_REQUIRED` | **Tetap ditolak** 422 `ISOLATION_REQUIRED` | Aturan 7 |
| Episode `RequiresIsolation = false` ke tempat tidur `IsIsolationBed = true` | Ditolak 422 `ISOLATION_BED_RESERVED` | **Tetap ditolak** 422 `ISOLATION_BED_RESERVED` | Aturan 8 |
| Bayi laki-laki ke boks `IsForNewborn = true` di kamar ibunya | Berhasil | **Tetap berhasil** | Penjaga `if (!bed.IsForNewborn)` melewati aturan 4 dan 5 |

Nomor aturan yang dapat terbit pada `failures[]` sesudah perubahan ini adalah **1, 2, 4, 5, 7, dan
8**. Nomor 6 tidak pernah terbit lagi, dan nomor 7 serta 8 **tidak bergeser** — keduanya tetap 7
dan 8 pada source, sehingga pemanggil yang sudah terbit tidak perlu memetakan ulang apa pun.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

**Tata kelola.**

- `AGENTS.md` backend
- `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`
- `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`
- `rules/backend/TASK_RULES.md`, `TASK_CLASSIFICATION.md`, `REVIEW_RULES.md`, `REPORT_TEMPLATE.md`, `TEST_POLICY.md`
- `rules/rule-output/status-task-roadmap.md`

**Kontrak dan keputusan.**

- `contracts/api-contract.md` `0.8.0` bagian perubahan `0.8.0`
- `contracts/validation-matrix.md` `0.8.0` bagian 3
- `02-backend-architecture.md` revision `0.7` bagian 0.1
- `04-prd-to-mvp.md` bagian 21.3, 21.4, dan 21.5
- `testing/acceptance-test-matrix.md` `0.8.0` bagian 2A.1, 2A.2, dan 2A.3
- `roadmap/backend-roadmap.md` bagian `S9`
- `roadmap/requirement-traceability.md` bagian Gelombang 1A

**Source.**

- `Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs` — seluruh
  `EvaluatePlacementEligibilityAsync`, keempat pemanggilnya, dan seluruh method penolong yang
  dipakai blok jenis kelamin
- `Areas/HealthServices/InPatientManagement/Controllers/InpatientBedOccupancyController.cs` —
  metadata hak akses keenam endpoint yang memanggil pemeriksaan kelayakan
- Frontend `QuilvianSystemFrontendDev` — **read-only**, hanya untuk menghitung pemakaian kode
  penolakan yang dicabut

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs` | Blok aturan 6 beserta kode `ROOM_GENDER_MIXED` dihapus. Klausa `countedOccupants.Count > 0` pada aturan 5 dicabut, dan kalimat penolakannya disesuaikan dengan `validation-matrix.md` `0.8.0`. Pemuatan penghuni kamar `LoadRoomOccupantsAsync`, kelas penolongnya `RoomOccupant`, dan pembentuk label `GenderLabel` dihapus karena terbukti menjadi kode mati. Komentar dokumentasi method diperbarui supaya mencatat pemensiunan aturan 6 beserta dasarnya |

Ringkasan diff: **1 berkas, 21 baris ditambahkan, 76 baris dihapus.** Nol berkas lain disentuh.

#### Temuan kode mati — jawaban atas butir cakupan task

Kartu task memerintahkan "periksa apakah `LoadRoomOccupantsAsync` menjadi kode mati dan bereskan
sesuai temuan", dan `02-backend-architecture.md` bagian 0.1 menegaskan pemeriksaan itu wajib
dilakukan, bukan disimpulkan. Hasil pemeriksaannya:

| Simbol | Pemanggil sebelum perubahan | Keadaan sesudah aturan 6 dicabut | Tindakan |
| --- | --- | --- | --- |
| `LoadRoomOccupantsAsync` | **Satu**, yaitu blok aturan 4/5/6 | Nol pemanggil — kode mati | Dihapus |
| `RoomOccupant` | **Dua**, yaitu tipe balikan dan proyeksi `Select` di dalam `LoadRoomOccupantsAsync` | Nol pemanggil — kode mati | Dihapus |
| `GenderLabel` | **Satu**, yaitu kalimat penolakan aturan 6 | Nol pemanggil — kode mati | Dihapus |
| `NormalizeGender` | **Dua**, yaitu aturan 4/5 dan aturan 6 | **Satu** pemanggil tersisa pada aturan 4/5 | Dipertahankan; alasan pada komentarnya diperbarui supaya tidak lagi menyebut pencampuran kamar |
| `BedGenderMessage` | **Satu**, yaitu aturan 4 | **Satu** — tidak terdampak | Dipertahankan apa adanya |

`GenderLabel` tidak disebut kartu task, tetapi ia mati karena sebab yang sama persis dan hanya
dipakai oleh kalimat penolakan yang dihapus. Pemeriksaannya dijalankan dengan pencarian seluruh
source, bukan dengan perkiraan.

#### Yang sengaja **tidak** diubah

| Yang dibiarkan | Alasan |
| --- | --- |
| Parameter `MstRoom room` pada `EvaluatePlacementEligibilityAsync` | Sesudah aturan 6 dicabut, parameter ini tidak lagi dibaca di dalam method. Menghapusnya menyentuh empat pemanggil sekaligus dan mengubah tanda tangan method yang dipakai jalur pencarian, pemesanan, penempatan, dan perpindahan. Itu di luar cakupan task yang berbunyi "nol service baru". Dicatat sebagai temuan, bukan diperbaiki tanpa wewenang |
| Nomor aturan 7 dan 8 | Kriteria 8 melarangnya bergeser. Keduanya tetap `result.Add(7, ...)` dan `result.Add(8, ...)` |
| Kolom "boleh campur" pada `MstRoom` | Ditolak tegas `RWI-DEC-066`. Pencabutan ini justru membuatnya tidak dibutuhkan |
| Metadata hak akses pada controller | Nol `[AccessAction]` dan nol `[AccessPermission]` baru. Task ini tidak menyentuh satu pun endpoint |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Perilaku satu endpoint berubah, bentuknya tidak.** Nol route baru, nol route berubah, nol field request/response berubah. Yang berubah adalah **isi** `failures[]` dan `ineligible[].failures[]`: kode `ROOM_GENDER_MIXED` tidak pernah terbit lagi dari `GET /available-beds`, `POST /reservations`, `POST /placements`, maupun `POST /placements/transfer`. Bagi pemanggil yang hanya membaca daftar tempat tidur yang lolos, perubahan ini **menambah** tempat tidur yang tersedia. Bagi pemanggil yang memetakan kode penolakan, ini **perubahan yang merusak** — lihat bagian 7 |
| Database | `NOT APPLICABLE` untuk schema. Nol tabel, nol kolom, nol index, nol migration, nol eksekusi database. Yang berubah hanya **perilaku query**: satu query pembacaan penghuni kamar dihapus dari setiap pemeriksaan kelayakan yang punya episode dan tempat tidurnya bukan boks bayi. Pada pencarian tempat tidur, pemeriksaan itu dijalankan sekali untuk **setiap** tempat tidur kandidat, sehingga satu pencarian yang menghasilkan 30 tempat tidur kandidat kini menjalankan **30 query lebih sedikit** daripada sebelumnya |
| Keamanan/Auth | Hak akses **tidak berubah sama sekali**. Nol `[AccessAction]` dan nol `[AccessPermission]` baru; keempat jalur tetap memakai `InpatientBedOccupancy : Read`, `: Create`, dan `: Transfer` seperti sebelumnya. Nol pemeriksaan role, nama departemen, atau `UserType` yang ditanam di kode. **Dampak privasi ada dan disengaja:** aturan privasi jenis kelamin tingkat kamar dicabut atas keputusan pemilik `RWI-DEC-101`, dan privasi kini ditegakkan lewat penanda tempat tidur yang ditetapkan Admin Master Data |

---

## 4. Dokumentasi endpoint

Task ini **tidak menyentuh satu pun endpoint**: nol route, nol verb, nol DTO, nol hak akses yang
berubah. Yang berubah hanya isi daftar penolakan yang dikembalikan endpoint yang sudah ada.

Keenam endpoint di bawah dicantumkan sebagai **daftar terdampak perilaku**, bukan sebagai
perubahan kontrak.

#### Health Services / Inpatient Management / Bed Occupancy

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/inpatient-management/bed-occupancies/available-beds` | Mencari tempat tidur yang tersedia; sejak `BE-RWI-069` dapat menyertakan tempat tidur yang ditolak beserta alasannya | `InpatientBedOccupancy : Read` |
| `GET` | `/api/v1/health-services/inpatient-management/bed-occupancies/bed-board` | Melihat papan ketersediaan tempat tidur | `InpatientBedOccupancy : Read` |
| `POST` | `/api/v1/health-services/inpatient-management/bed-occupancies/reservations` | Memesan tempat tidur untuk satu episode | `InpatientBedOccupancy : Create` |
| `POST` | `/api/v1/health-services/inpatient-management/bed-occupancies/placements` | Menempatkan pasien pertama kali | `InpatientBedOccupancy : Create` |
| `POST` | `/api/v1/health-services/inpatient-management/bed-occupancies/placements/transfer` | Memindahkan pasien ke tempat tidur lain | `InpatientBedOccupancy : Transfer` |
| `PATCH` | `/api/v1/health-services/inpatient-management/bed-occupancies/reservations/{id}/cancel` | Membatalkan pemesanan tempat tidur | `InpatientBedOccupancy : Update` |

Pemeriksaan hak akses yang dijalankan: argumen pertama `[AccessPermission]` pada keenam action
sama persis dengan `ControllerName = "InpatientBedOccupancy"` pada `[AccessController]`, dan
argumen keduanya sama persis dengan argumen pertama `[AccessAction]` pada method yang sama. Tidak
ada penyimpangan, dan tidak ada baris yang disentuh task ini.

---

## 5. Verifikasi

### 5.1 Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `InPatientManagement` / Inpatient |
| Submodule | `Services` — tidak ada submodule terdaftar tersendiri |
| Pemilik/prefix registry | `Inp`, lifecycle `ACTIVE` sejak 2026-08-24 lewat `RWI-DEC-068`, disetujui Muhammad Hamzah |
| Keberlakuan | `TOUCHED LEGACY` terhadap berkas yang sudah ada, dengan isi perubahan berupa **pengurangan** aturan bisnis. Nol entity baru, nol model persisted baru, sehingga `QBE-MOD-002` dan `QBE-MOD-003` tidak berlaku pada task ini |
| Status registry | `ACTIVE` — memberi wewenang penamaan. Wewenang tulis source diberikan pemilik lewat instruksi eksplisit mengerjakan `BE-RWI-073` pada 11 September 2026, mengikuti pola approval per-task `BE-RWI-036` dan `BE-RWI-069` |
| QBE ID yang berlaku | `QBE-SVC-001` — aturan bisnis tetap tinggal di module service, controller tidak menyentuh `DbContext`. `QBE-API-001` — boundary, envelope `ApiResponse<T>`, dan kode status yang sudah mapan dipertahankan. `QBE-VAL-001` — invarian bisnis tetap divalidasi di backend; yang dicabut adalah invariannya sendiri atas keputusan pemilik, bukan tempat pemeriksaannya. `QBE-PERM-001` — metadata Access tidak berubah. `QBE-DTO-001` — nol entity EF terekspos |
| QBE ID yang **tidak** berlaku | `QBE-ENT-*`, `QBE-CFG-*`, `QBE-NAM-*`, `QBE-CODE-*`, `QBE-DB-*` — nol entity, nol configuration, nol penamaan baru, nol nomor bisnis, nol pekerjaan database |
| Pengecualian QBE | `NONE` |

### 5.2 Perintah dan skenario

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:BaseOutputPath=<scratch>/ -p:UseAppHost=false` | Dinyalakan 11 September 2026 ke folder keluaran terpisah supaya `bin/` aplikasi dev tidak terkunci, tetapi **belum selesai dalam sesi ini**. Hasilnya **tidak diklaim** — nol error dan nol warning yang dilaporkan, karena keluarannya memang belum ada | `NOT RUN` | Berkas keluaran perintah masih kosong saat laporan ini ditulis |
| `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Path <berkas yang berubah> -Mode ReportOnly` | `Files evaluated: 1`, `VIOLATION: 0`, `REVIEW: 0`, `INFO: 0`, `Findings: none`, `Final result: PASS` | `PASS` | Keluaran perintah |
| Kode `ROOM_GENDER_MIXED` nol hasil pada source aktif backend | `grep -rn "\"ROOM_GENDER_MIXED\"" Areas/` → **0 baris**. `grep -rn "ROOM_GENDER_MIXED" Areas/` → **1 baris**, yaitu komentar dokumentasi sejarah pada `InpBedOccupancyService.cs:1249`, yang secara eksplisit diizinkan `04-prd-to-mvp.md` bagian 21.5 dengan kalimat "di luar komentar sejarah" | `PASS` | Keluaran `grep` |
| `LoadRoomOccupantsAsync`, `RoomOccupant`, `GenderLabel`, dan `countedOccupants` nol hasil | `grep -rn` pada `Areas/HealthServices/InPatientManagement/` → **0 baris** untuk keempatnya | `PASS` | Keluaran `grep` |
| Aturan 4 `BED_GENDER_MISMATCH` masih menolak | Ada pada `InpBedOccupancyService.cs:1379`, kondisinya tidak berubah satu karakter pun | `PASS` | Penelusuran source |
| Aturan 5 `PATIENT_GENDER_UNKNOWN` masih menolak tempat tidur satu jenis kelamin | Ada pada `InpBedOccupancyService.cs:1386-1394`; kondisinya kini `!bed.IsForMale \|\| !bed.IsForFemale` | `PASS` | Penelusuran source |
| Aturan 7 `ISOLATION_REQUIRED` masih menolak — **regresi terpenting task ini** | Ada pada `InpBedOccupancyService.cs:1399-1406`, nomornya tetap `7`, kondisi dan kalimatnya tidak berubah | `PASS` | Penelusuran source dan review diff |
| Aturan 8 `ISOLATION_BED_RESERVED` masih menolak — **regresi terpenting task ini** | Ada pada `InpBedOccupancyService.cs:1409-1415`, nomornya tetap `8`, kondisi dan kalimatnya tidak berubah | `PASS` | Penelusuran source dan review diff |
| Pengecualian boks bayi masih berlaku | Penjaga `if (!bed.IsForNewborn)` pada `InpBedOccupancyService.cs:1366` tetap membungkus aturan 4 dan 5 | `PASS` | Penelusuran source |
| Nomor aturan 7 dan 8 tidak bergeser, nomor 6 kosong | Nomor yang dapat terbit: 1, 2, 4, 5, 7, 8. Nol pemanggil `result.Add(6, ...)` | `PASS` | Penelusuran source |
| Verifikasi hak akses | Keenam action pada `InpatientBedOccupancyController` tetap cocok antara `[AccessController]`, `[AccessAction]`, dan `[AccessPermission]`. Nol baris disentuh | `PASS` | Penelusuran source |
| Review diff dan scope | 1 berkas, 21 baris ditambah, 76 baris dihapus. Nol perubahan dependency, konfigurasi, migration, atau berkas hasil generate. `git status --short` memuat satu berkas source | `PASS` | `git diff --stat`, `git status --short` |
| Pemeriksaan rahasia | Nol credential, token, connection string, atau key pada diff maupun laporan ini | `PASS` | Review diff |
| Integration test `RWI-AC-133a` dan `RWI-AC-133b` | `NOT RUN` | `NOT RUN` | Backend tidak memelihara project automated test — lihat bagian 5.3 |
| Pemetaan kode penolakan pada frontend | `NOT APPLICABLE` untuk task ini. Frontend masih memuat **5 baris aktif** pada **3 berkas** — lihat bagian 7 | `NOT RUN` | `grep` read-only pada repository frontend |

Uji manual: `NOT FEASIBLE`. Membuktikan penempatan sungguhan menuntut aplikasi berjalan dengan data
master kamar dan tempat tidur yang sudah terisi benar, dan gerbang "Kesiapan data master" pada
roadmap masih terbuka. Perilakunya dibuktikan lewat penelusuran source beserta contoh berangka
pada bagian 2.1.

**Tidak dijalankan, beserta alasannya:**

- **`dotnet build` project aplikasi** — `NOT RUN`, **dikecualikan atas keputusan pemilik 10
  September 2026** yang berbunyi "skip build, jika sudah diimplementasikan code, skip build biar
  saya lakukan secara mandiri". Build penuh pada mesin pemilik memakan sekitar 25 menit dan
  bentrok dengan aplikasi dev yang sedang berjalan. Perintahnya tetap dinyalakan pada 11 September
  2026 ke folder keluaran terpisah, tetapi belum selesai dalam sesi ini, sehingga hasilnya tidak
  diklaim ke arah mana pun. Sebagai ganti bukti otomatis, pemeriksa kesesuaian QBE dijalankan
  penuh pada berkas yang berubah dan menjawab `PASS` dengan nol temuan.
- **Integration test `RWI-AC-133a` dan `RWI-AC-133b`** — `NOT RUN`. Folder `Tests/` beserta kelima
  project test sudah dihapus dari repository dan tidak lagi ada di disk, sedangkan
  `rules/backend/TEST_POLICY.md` melarang membuatnya kembali tanpa permintaan eksplisit pemilik
  pada task aktif. Task ini tidak memuat permintaan itu. Kolom `Verification` pada kartu task
  berstatus **HISTORICAL EVIDENCE** menurut `TEST_POLICY.md` bagian 6. Kedua skenario itu tetap
  dibuktikan, dengan bentuk bukti pada `TEST_POLICY.md` bagian 5, yaitu penelusuran aturan pada
  source beserta contoh berangka — lihat empat baris isolasi dan boks bayi pada tabel di atas.
- **`dotnet test`** — `NOT APPLICABLE`. Tidak ada project test di repository.
- **Eksekusi database dan deployment** — `NOT APPLICABLE`. Task ini nol migration dan nol
  perubahan schema.
- **Penyesuaian unit test yang mengunci aturan kamar lama** — `NOT APPLICABLE`. Kartu task dan
  butir DoD memerintahkan "unit test lama yang mengunci aturan kamar disesuaikan, bukan dihapus
  diam-diam". Perintah itu ditulis ketika `Tests/` masih ada. Pemeriksaan pada 11 September 2026
  menemukan folder itu **sudah tidak ada di disk**, sehingga tidak ada test yang dapat
  disesuaikan. Tidak satu pun test dihapus oleh task ini.

### 5.3 Status automated test

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

---

## 6. Acceptance criteria dan Definition of Done

### 6.1 Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Kamar berpenghuni laki-laki menerima pasien perempuan pada tempat tidur netral | **Terpenuhi** | Blok yang menolaknya sudah tidak ada. Aturan 4 pada `InpBedOccupancyService.cs:1372-1380` meloloskan pasien perempuan pada tempat tidur `IsForFemale = true` tanpa membaca penghuni kamar mana pun. Contoh berangka baris pertama tabel 2.1 |
| 2. Kode `ROOM_GENDER_MIXED` nol hasil pada pencarian source | **Terpenuhi** | Nol baris untuk literal `"ROOM_GENDER_MIXED"` pada seluruh `Areas/`. Satu-satunya sisa adalah komentar dokumentasi sejarah pada baris 1249, yang diizinkan `04-prd-to-mvp.md` bagian 21.5 |
| 3. Pasien tanpa jenis kelamin tercatat berhasil ditempatkan di kamar berpenghuni, pada tempat tidur yang menerima keduanya | **Terpenuhi** | Klausa `countedOccupants.Count > 0` dicabut; kondisi aturan 5 pada baris 1386 kini hanya `!bed.IsForMale \|\| !bed.IsForFemale`. Contoh berangka baris kedua tabel 2.1 |
| 4. Pasien tanpa jenis kelamin tercatat **tetap ditolak** pada tempat tidur satu jenis kelamin | **Terpenuhi** | Kedua klausa penanda tempat tidur pada baris 1386 dipertahankan utuh. Contoh berangka baris ketiga tabel 2.1 |
| 5. `BED_GENDER_MISMATCH` **tetap** menolak | **Terpenuhi** | `InpBedOccupancyService.cs:1379` tidak berubah satu karakter pun, termasuk kalimat penolakan dari `BedGenderMessage` |
| 6. `ISOLATION_REQUIRED` dan `ISOLATION_BED_RESERVED` **tetap** menolak | **Terpenuhi** | Kedua blok pada baris 1398-1415 tidak berubah. Ini regresi terpenting task ini, karena keduanya bertetangga di dalam method yang sama dengan aturan yang dihapus; review diff membuktikan batas penghapusan berhenti sebelum keduanya |
| 7. Pengecualian boks bayi tetap berlaku | **Terpenuhi** | Penjaga `if (!bed.IsForNewborn)` pada baris 1366 utuh dan tetap membungkus kedua aturan jenis kelamin yang tersisa |
| 8. Nomor aturan 7 dan 8 **tidak bergeser**; nomor 6 dibiarkan kosong | **Terpenuhi** | `result.Add(7, ...)` dan `result.Add(8, ...)` tetap bernomor 7 dan 8. Nol pemanggil `result.Add(6, ...)`. Pemensiunan nomor 6 ditulis sebagai komentar supaya tidak dipakai ulang pembaca berikutnya |

Kedelapan kriteria **terpenuhi**.

### 6.2 Definition of Done

| Butir DoD | Status | Keterangan |
| --- | --- | --- |
| Butir 21.5 pertama — `ROOM_GENDER_MIXED` nol hasil pada pencarian source backend | **Terpenuhi** | Nol baris aktif; satu komentar sejarah yang diizinkan |
| Butir 21.5 kedua — pemetaan kode hilang dari frontend dan ketiga berkas test disesuaikan | **Belum terpenuhi, dan bukan milik task ini** | Pekerjaan frontend `FE-RWI-062`. Pemeriksaan read-only 11 September 2026 menemukan 5 baris aktif pada 3 berkas — lihat bagian 7. Task backend tidak punya wewenang tulis frontend |
| Butir 21.5 ketiga — aturan isolasi terbukti masih menolak | **Terpenuhi dengan bukti berbeda** | Dibuktikan lewat penelusuran source dan review diff, bukan lewat `RWI-AC-133a`, karena backend tidak memelihara project test |
| Butir 21.5 keempat — pengecualian boks bayi terbukti masih berlaku | **Terpenuhi dengan bukti berbeda** | Sama seperti di atas, menggantikan `RWI-AC-133b` |
| Unit test lama yang mengunci aturan kamar disesuaikan, bukan dihapus diam-diam | `NOT APPLICABLE` | Folder `Tests/` sudah tidak ada di repository. Nol test dihapus oleh task ini |
| `dotnet build` tanpa error baru | **Dikecualikan atas keputusan pemilik 10 September 2026** | `NOT RUN`. Pemilik menyatakan build dijalankan sendiri. Butir ini bukan salah satu dari kedelapan acceptance criteria, dan kesesuaian QBE sudah dijalankan penuh sebagai bukti pengganti |
| Kesesuaian QBE dan preflight engineering diselesaikan saat eksekusi | **Terpenuhi** | Bagian 5.1 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nol warning baru yang berasal dari perubahan ini. Parameter `MstRoom room` pada `EvaluatePlacementEligibilityAsync` kini tidak dibaca di dalam method; C# tidak memperingatkan parameter yang tidak dipakai, sehingga ini tidak memunculkan warning compiler |
| Masalah yang diketahui | **Frontend masih memetakan kode yang sudah tidak pernah terbit.** Pemeriksaan read-only pada `QuilvianSystemFrontendDev` menemukan 5 baris aktif di 3 berkas: `src/utils/health-services/inpatient-management/inpatient-placement-utils.jsx` baris 12, `tests/unit/inpatient-placement.test.mjs` baris 57, 82, dan 129, serta `tests/e2e/inpatient-episode-detail.spec.mjs` baris 605. Angka itu cocok persis dengan perkiraan `api-contract.md` `0.8.0`. Seluruhnya milik `FE-RWI-062`, dan **tidak** disentuh task ini karena task backend tidak diberi wewenang tulis frontend |
| Masalah yang diketahui | **Parameter `room` menjadi tidak terpakai.** Membersihkannya menuntut perubahan tanda tangan method publik yang dipakai empat pemanggil, sehingga dibiarkan dan dilaporkan. Bila pemilik menghendaki, itu task pembersihan tersendiri |
| Masalah yang diketahui | **Solution masih menyebut lima project test yang tidak ada di disk.** `QuilvianSystemBackend.sln` masih memuat baris `Project(...)` untuk `Tests\QuilvianSystemBackend.Tests`, `...UnitTests.InMemory`, `...UnitTests.Sqlite`, `...IntegrationTests.Postgres`, dan folder `Tests`, sementara folder `Tests/` sudah dihapus. Akibatnya build tingkat solution tidak dapat dipakai, dan verifikasi harus menyasar `QuilvianSystemBackend.csproj` secara langsung. **Keadaan ini sudah ada sebelum task ini** dan tidak diperbaiki tanpa wewenang |
| Masalah yang diketahui | **Satu kalimat pada roadmap sudah basi sejak sebelum task ini.** Bagian `S9` menulis "Dua task, keduanya `⛔` menunggu approval kontrak", padahal `RWI-DEC-105` mencabut gerbang itu pada 11 September 2026 dan kartu kedua task sudah berbunyi "BELUM DIKERJAKAN, siap dimulai". Kalimat itu narasi lingkup, bukan baris status, sehingga di luar wewenang tulis task ini. Dilaporkan untuk skill perencanaan |
| Risiko tersisa | **Backend dan frontend wajib rilis pada satu gelombang.** `api-contract.md` `0.8.0` menyatakannya tegas. Menurunkan backend lebih dulu membuat `inpatient-placement-utils.jsx` memetakan kode yang tidak pernah terbit lagi, dan dua berkas test frontend menguji perilaku yang sudah tidak ada. Tidak ada layar yang rusak karenanya — cabang pemetaan itu hanya tidak pernah tercapai — tetapi test frontend akan gagal bila dijalankan |
| Risiko tersisa | **Privasi jenis kelamin kini sepenuhnya bergantung pada kebenaran data master.** Sebelumnya ada dua lapis: penanda tempat tidur dan penghuni kamar. Kini hanya satu. Bila Admin Master Data menandai sebuah tempat tidur menerima laki-laki dan perempuan padahal kamarnya tidak pantas dicampur, tidak ada lagi lapis kedua yang menahannya. Ini **konsekuensi yang disengaja** dari `RWI-DEC-101` dan `RWI-DEC-066`, bukan cacat implementasi. Pemilik risiko: Admin Master Data bersama pemilik modul |
| Risiko tersisa | Nol risiko database. Nol migration dibuat, nol perintah database dijalankan |
| Perubahan sampingan | Satu, sudah dipulihkan. Skrip penyuntingan sempat menambahkan penanda urutan byte UTF-8 pada awal `InpBedOccupancyService.cs`, yang aslinya tidak ada. Penanda itu dihapus kembali, dan diff akhir sudah tidak memuat baris `using` pertama sebagai perubahan. Nol perubahan sampingan lain, dan nol pekerjaan pengguna yang dibatalkan |
| Interupsi | `NONE` |
| Status Git | `M Areas/HealthServices/InPatientManagement/Services/InpBedOccupancyService.cs` — satu-satunya perubahan source, dan seluruhnya berasal dari task ini. Working tree bersih sebelum task dimulai. Nol stage, nol commit, nol push. Branch `MHamzah` |
| Langkah berikutnya | 1. Pemilik menjalankan `dotnet build QuilvianSystemBackend.csproj` sendiri bila hasil build pada bagian 5.2 belum terisi. 2. `FE-RWI-062` mencabut pemetaan `ROOM_GENDER_MIXED` dari ketiga berkas frontend, lalu backend dan frontend dirilis bersama. 3. `BE-RWI-074` boleh dikerjakan paralel dan tidak menunggu task ini. 4. Bila pembersihan parameter `room` dikehendaki, buka task tersendiri |
