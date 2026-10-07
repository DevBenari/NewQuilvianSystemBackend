# Laporan Perubahan Backend — `BE-LAB-73`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-73` |
| Judul | Validasi hasil dan penjaga Reopen |
| Slice | Gelombang `MVP-9b` — `EPIC-LAB-15`, tindakan `S4` validasi dan rilis hasil |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian **6ak.4** |
| Trace | `FR-15.1`, `FR-15.3` bagian validasi, `FR-15.5`, `FR-15.7`, `FR-15.15`; `LAB-DEC-003`, `LAB-DEC-135`, `LAB-DEC-142`, `LAB-DEC-150`; `02-backend-architecture.md` 20.1, 20.4, 20.10 butir 5 |
| Contract version | `LAB-API-v1` **`r34`** 29.2-29.3; `LAB-VAL-v1` **`r12`** `VAL-124`..`VAL-130`, `VAL-132`, `VAL-136`; `LAB-PERM-v1` **rev 11** 13.2-13.6; `LAB-STATE-v1` **`r5`** 7.2-7.3 — seluruhnya **`approved` 2026-09-25** |
| Dependency | `BE-LAB-67` ⚠ (resource `LabExaminationResult`), `BE-LAB-71` ✅ (daftar alasan, terisi), `BE-LAB-72` ✅ (resolver kewenangan) |
| Klasifikasi | `HEAVY` — skor 10: repository 0, berkas diperiksa 2 (>20), berkas diubah 1 (6), logika bisnis **2**, kontrak API **2** (satu endpoint baru, ruas respons bertambah), database 1 (menulis kolom yang sudah ada), keamanan/auth **2**, UI/workflow 0. Satu slice terbatas — bukan `EPIC` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/LaboratoryManagement/`, `Program.cs`, dokumen blueprint Laboratorium |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `017d1819` (branch `yoga`), di atas `BE-LAB-67`..`72` yang belum ter-commit |
| Tanggal | 2026-09-29 |
| Status | ✅ **`SELESAI`** (naik 2026-10-06) — jalur berhasil, penolakan lapis orang, `AC-238`, `VAL-136`, dan balapan dua validasi kini teramati **lewat HTTP asli** terhadap PostgreSQL devYoga dengan akun dr. Bima dan Vina. Lihat 8. *(Semula: ⚠ — kode lengkap; build 0 error tanpa warning baru; startup lolos dengan registri **1573** kunci; **38 dari 38 skenario lolos** pada harness EF InMemory; tujuh panggilan HTTP lolos tanpa menulis data. **Belum lewat HTTP:** jalur berhasil, penolakan lapis orang, `VAL-136`, dan balapan sungguhan — keempatnya butuh aksi `Validate` diberikan, penunjukan di Human Resource, dan hasil Patologi Klinik yang Final, yang seluruhnya berarti menulis ke database bersama)* |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `LaboratoryManagement` |
| Pemilik / prefix registry | `Lab` — `ACTIVE` (`MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 28 dan 56) |
| Keberlakuan | `NEW CODE` — `LabResultValidationService.cs`, endpoint `validate`, `LabResultSignOffRequest`; `TOUCHED LEGACY` — `LabExaminationService` (Reopen, penyusun respons), `LabExaminationController`, `LabExaminationResultDtos.cs`, `Program.cs`, dan satu teks pesan pada `LabClinicalPrivilegeResolver.cs` (`BE-LAB-72`) |
| QBE yang berlaku | `QBE-SVC-001` (controller tanpa `DbContext`), `QBE-API-001` (`ApiResponse`, kode status kontrak), `QBE-PERM-001` (`[AccessAction("Validate")]` + `[AccessPermission("LabExaminationResult", "Validate")]`), `QBE-VAL-001` (`VAL-124`..`136`), `QBE-DTO-001` (entity tidak diekspos), `QBE-LOG-001` (log dengan pelaku, tanpa nilai hasil dan catatan), `QBE-TXN-001` (satu `SaveChangesAsync`, `Version` sebagai token), `QBE-AUD-001` (riwayat transisi terpisah dari logger). **Tidak berlaku:** `QBE-ENT-*`, `QBE-CFG-*`, `QBE-CODE-*` — nol entity baru, nol nomor bisnis |
| Governance yang dibaca | `AGENTS.md` backend; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `rules/backend/TASK_RULES.md`, `TASK_CLASSIFICATION.md`, `API_RULES.md`, `DATABASE_RULES.md`, `REVIEW_RULES.md`, `REPORT_TEMPLATE.md`, `transaction-endpoint-standard.md` |

---

## 1. Masalah yang diperbaiki

**Belum ada cara menyatakan bahwa angka hasil Patologi Klinik sudah diperiksa dokter dan benar.**
*Final* hanya berarti analis selesai menulis (`LAB-DEC-097`). Tanpa validasi, tidak ada tahap
yang mencatat siapa yang mengesahkan hasil, dengan dasar kewenangan apa, dan apakah ia orang yang
sama dengan yang mengisi.

**Ada juga celah pada Reopen.** Sebelum task ini, analis dapat membuka kembali hasil kapan saja
selama belum dirilis. Reopen pun tidak menaikkan `Version`. Akibatnya, bila Reopen dan Validasi
ditekan pada detik yang sama, keduanya dapat berhasil, dan tersimpan hasil **tervalidasi** yang
`FinalizedAt`-nya **kosong**: dokter mengesahkan angka yang saat itu sedang disunting.

---

## 2. Proses bisnis

**Pelaku.** Dokter berkewenangan laboratorium yang jabatannya diberi aksi `Validate` **dan**
dirinya ditunjuk pada kredensial Human Resource (`LAB-DEC-150`, dua lapis `LAB-DEC-142`).

**Alur Validasi, berurutan** (`02-backend-architecture.md` 20.4; `LAB-VAL-v1` `r12` 14.1):

| Langkah | Pemeriksaan | Bila gagal |
| ---: | --- | --- |
| 0 | Filter hak akses: jabatan memegang `LabExaminationResult : Validate` — lapis jabatan | `403` *"Anda tidak memiliki akses ke menu atau fitur ini."* |
| 1 | Pemeriksaan ada | `404` |
| 2 | Patologi Klinik — `VAL-126` | `422` *"Validasi dan rilis hasil Mikrobiologi serta Patologi Anatomi belum tersedia."* |
| 3 | Tidak gugur, tidak batal, order tidak batal — `VAL-127` | `422` |
| 4 | Sudah Final — `VAL-124`; belum divalidasi — `VAL-125` | `422` / `409` |
| 5 | **Lapis orang** — penunjukan berlaku hari itu (`VAL-128`, `BE-LAB-72`) | `403` dengan **sebabnya**; `503` bila data kewenangan tidak terbaca |
| 6 | Pengisi tercatat — `VAL-130` | `422` |
| 7 | Empat mata — pengisi yang memvalidasi wajib membawa alasan aktif (`VAL-129`, `VAL-132`) | `422` |
| 8 | Tulis: tujuh kolom, satu baris riwayat, `Version` naik — satu `SaveChangesAsync` | `409` bila baris baru saja diubah orang lain |

**Kenapa lapis orang sesudah keadaan hasil.** Dokter yang belum ditunjuk menekan Validasi pada hasil
yang masih Draft akan membaca *"belum dinyatakan selesai oleh analis"*, sebab yang sebenarnya.
Bila urutannya dibalik, ia membaca *"Anda belum ditunjuk…"* untuk hasil yang memang belum boleh
divalidasi siapa pun.

**Yang tercatat bila berhasil — contoh.** dr. A memvalidasi Kalium yang diisi analis:

| Kolom | Isi |
| --- | --- |
| `ValidatedAt`, `ValidatedByUserId` | Saat itu; dr. A |
| `ValidatedByPositionId`, `ValidatedByPositionNameSnapshot` | Penempatan dr. A yang **memegang** `Validate` — misalnya *Dokter Penanggung Jawab Laboratorium*, walau penempatan utamanya *Dokter Umum* (`AC-239`) |
| `ValidatedByPrivilegeId` | Baris penunjukan Human Resource yang berlaku saat itu |
| `ValidationExceptionReasonId` dan snapshot namanya | Kosong — dr. A bukan pengisi |
| Riwayat | Satu baris `LabExamination.ValidateResult`, `Finalized` → `Validated` |

**Jalur pengecualian — contoh (`AC-01`).** Pukul 02.12, dr. B satu-satunya dokter yang bertugas dan
ia sendiri yang mengisi hasil. Validasi tanpa alasan → `422` *"Anda yang mengisi hasil ini. Validasi
oleh orang yang sama memerlukan alasan pengecualian."* Dengan alasan aktif *Shift tunggal, tidak ada
dokter lain bertugas* → diterima. Respons dan riwayat membawa penanda, **berbunyi persis** seperti
yang disetujui pada 20.10 butir 5:

> *Divalidasi oleh pengisi sendiri — dr. B — Shift tunggal, tidak ada dokter lain bertugas*

Alasan yang dikirim padahal pelaku **tidak** merangkap → `422`, supaya penanda pengecualian tidak
pernah tercetak pada hasil yang tidak merangkap.

**Reopen kini terjaga.**

| Keadaan | Reopen oleh analis |
| --- | --- |
| Final, belum divalidasi | `200` — seperti sebelumnya, **kini menaikkan `Version`** |
| Sudah divalidasi | `409` *"Hasil ini sudah divalidasi. Minta pemvalidasi atau perilis mengembalikannya bila perlu diubah."* (`VAL-136`) |

**Dua tindakan pada detik yang sama** — Reopen lawan Validasi, atau dua dokter memvalidasi — **tidak
dapat sama-sama berhasil**. Yang menyimpan kedua menerima `409` *"Hasil ini baru saja diubah orang
lain. Muat ulang lalu ulangi."*

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / dokumen | Kenapa dibaca |
| --- | --- |
| `roadmap/backend-roadmap.md` 6ak.4-6ak.7 | Cakupan, AC, jebakan; batas dengan `BE-LAB-74` dan `BE-LAB-76` |
| `contracts/api-contract.md` 29.2-29.3 | Endpoint, `LabResultSignOffRequest`, ruas respons, kode status |
| `contracts/validation-matrix.md` 14.1 | Teks dan urutan `VAL-124`..`VAL-136` |
| `contracts/state-transition-matrix.md` 7.2-7.3 | Transisi sah dan tidak sah; *setiap tindakan menaikkan `Version`* |
| `contracts/permission-audit-matrix.md` 13.2-13.6 | Atribut aksi `Validate`; isi riwayat dan payload log |
| `02-backend-architecture.md` 20.1, 20.4, 20.10 | Urutan pemeriksaan, satu `SaveChangesAsync`, bunyi penanda |
| `Services/LabExaminationService.cs` | Pola Final/Reopen, `SaveResultWriteAsync`, `BuildCompletionResponse`, riwayat Reopen |
| `Controllers/LabExaminationController.cs`, `DTOs/LabExaminationResultDtos.cs` | Pola endpoint dan pemetaan pengecualian |
| `Models/LabExamination.cs`, `Models/LabTransitionHistory.cs`, `LabExaminationConfiguration.cs` | Kolom validasi `BE-LAB-70`; `Version` sebagai token konkurensi |
| `rules/backend/transaction-endpoint-standard.md` | Verb aksi, kewenangan transisi, idempotency |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Services/LabResultValidationService.cs` | **Baru.** `ValidateAsync` — urutan 20.4, tujuh kolom, snapshot jabatan dari `ResolveActingPositionAsync`, `ValidatedByPrivilegeId`, satu riwayat `ValidateResult`, `Version` naik, `DbUpdateConcurrencyException` → `409`, log tanpa nilai hasil dan catatan |
| `Services/LabExaminationService.cs` | **Reopen:** `VAL-136` (`409`), `Version` naik, simpan lewat `SaveResultWriteAsync` sehingga bentrok menjadi `409`, bukan `500`. **`BuildCompletionResponse`** menjadi `internal static` — satu penyusun bagi Final, Reopen, konsultasi, dan Validasi — bertambah ruas validasi dan penanda. **Dua pembantu `internal`:** `ResolveDiscipline` (dipakai juga penjaga Patologi Anatomi, sehingga keduanya tidak pernah berbeda pendapat) dan `DeriveResultStatus` |
| `Controllers/LabExaminationController.cs` | Endpoint `POST /{id}/result/validate`; Reopen kini memetakan `409` |
| `DTOs/LabExaminationResultDtos.cs` | `LabResultSignOffRequest` baru; tujuh ruas pada `LabExaminationCompletionResponse` |
| `Program.cs` | `AddScoped<LabResultValidationService>()` |
| `Services/LabClinicalPrivilegeResolver.cs` | Teks `LabPrivilegeReadException` disamakan **persis** dengan kontrak 29.2: *"Data kewenangan klinis tidak dapat dibaca saat ini. Tindakan tidak dilakukan; coba lagi."* |

**Nol entity, nol migration, nol seeder.** Tujuh kolom validasi sudah lahir pada `BE-LAB-70`.

**Selisih dan keputusan — dicatat, bukan disembunyikan.**

| Hal | Keputusan dan alasan |
| --- | --- |
| `resultStatus` pada `LabExaminationCompletionResponse` | **Ditambahkan di sini.** Kontrak 29.3 mencantumkannya pada respons ini, sedangkan `BE-LAB-76` hanya mencakup `LabExaminationResultFormResponse` dan jalur per order. Penurunnya (`DeriveResultStatus`) sudah tersedia untuk dipakai ulang `BE-LAB-76` |
| `exceptionNote` dikirim **tanpa** `exceptionReasonId` oleh pelaku yang tidak merangkap | Ditolak `422`, sama dengan alasan yang dikirim tanpa perlu — catatan pengecualian tanpa pengecualian tidak berarti apa-apa |
| `IdempotencyKey` (`transaction-endpoint-standard` 5.3) | **Tidak ditambahkan** — tidak ada di kontrak `r34` yang disetujui. Klik ganda tetap tidak menghasilkan dua kejadian: permintaan kedua ditolak `VAL-125`, dan permintaan yang benar-benar bersamaan ditolak token `Version` |
| `422`, bukan `400`, untuk transisi tidak sah | Mengikuti kontrak `r34` yang disetujui dan konvensi modul Laboratorium |
| `AvailableActions` | Tidak ada di kontrak; layar membaca keadaan dari `resultStatus` |
| Nama pemvalidasi pada respons | Dibaca dari akun (`DisplayName ?? UserName ?? Email ?? UserCode`, pola modul). Kolomnya **bukan** snapshot — hanya jabatan dan alasan yang di-snapshot, sesuai kamus data 17 |
| Superadmin | Melewati filter hak akses, tetapi **tidak** melewati lapis orang: tanpa data tenaga kerja ia ditolak `403`. Tidak ada jalur pintas |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif, sesuai `r34` 29.2-29.3.** Satu endpoint baru; tujuh ruas baru pada `LabExaminationCompletionResponse` — ruas lama tidak berubah. Reopen memperoleh kode `409` baru (`VAL-136`, bentrok) |
| Database | **Nol migration.** Menulis tujuh kolom validasi `BE-LAB-70` dan satu baris `LabTransitionHistory`. Reopen kini menaikkan `Version`. **Tidak ada data yang ditulis ke database bersama oleh task ini** — selain sinkronisasi registri hak akses yang biasa terjadi setiap startup |
| Keamanan/Auth | **Aksi baru `LabExaminationResult : Validate`** — registri 1572 → **1573**; terdaftar di database dengan **nol pemegang**. Sampai admin memberikannya kepada jabatan dokter berkewenangan (`UNK-P14-03`, 20.7 langkah 6), **tidak seorang pun** dapat memvalidasi. Analis pemegang `Update` tidak memperoleh `Validate` — nama aksinya berbeda (`LAB-CONFLICT-012`). Lapis orang fail-closed |

---

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Examination

Base URL: `api/v1/health-services/laboratory-management/lab-examinations`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/{id}/result/validate` | **Baru.** Menyatakan angka hasil Patologi Klinik yang sudah Final benar. Badan: `exceptionReasonId`, `exceptionNote` — hanya bila pemvalidasi juga pengisi | `LabExaminationResult : Validate` |
| `POST` | `/{id}/result/reopen` | Tetap — kini **ditolak `409`** bila hasil sudah divalidasi, dan menaikkan `Version` | `LabExaminationResult : Update` |

**Kode status `validate`:** `200`; `403` tanpa aksi `Validate` atau tidak ditunjuk — pesan menyebut
sebabnya; `404`; `409` sudah divalidasi atau bentrok; `422` `VAL-124`, `VAL-126`, `VAL-127`,
`VAL-129`, `VAL-130`, `VAL-132`; `503` data kewenangan tidak terbaca.

**Ruas baru pada respons** — seluruh respons `finalize`, `reopen`, `consultation`, dan `validate`:
`resultStatus`, `isValidated`, `validatedAt`, `validatedByUserId`, `validatedByName`,
`validatedByPositionName`, `validationExceptionMarker`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` | **0 error, 230 warning** — sama dengan baseline; **nol** warning dari berkas yang disentuh; 90 detik | `PASS` | Keluaran build disaring pada nama berkas |
| Startup Development | Seluruh seeder lolos; *"Permission registry valid"* **1573** kunci — naik tepat satu | `PASS` | Log startup |
| Registri di database | `LabExaminationResult : Validate`, `AccessType = Update`, terlihat di Akses Role, **nol pemegang** | `PASS` | Kueri baca-saja |
| Swagger | `POST /lab-examinations/{id}/result/validate` di bawah tag `Lab Examination`; respons `200,403,404,409,422,503` | `PASS` | Dokumen Swagger `health-services` |
| Harness perilaku EF InMemory | **38 `PASS`, 0 `FAIL`** | `PASS` | Rincian di bawah. Nol baris ke database bersama |
| Panggilan HTTP — tujuh skenario penolakan | Ketujuhnya sesuai | `PASS` | Rincian di bawah |
| Isi `LabExamination` sebelum dan sesudah panggilan HTTP | 10 baris — status Final dan `Version` **identik** | `PASS` | Kueri baca-saja |
| Jalur berhasil, lapis orang, `VAL-136`, dan balapan **lewat HTTP** | Tidak dijalankan | `NOT RUN` | Lihat *Tidak dijalankan* |

**Rincian HTTP** — aplikasi sungguhan pada `http://localhost:5107`, database dev bersama.

| Skenario | Akun | Hasil |
| --- | --- | --- |
| `VAL-124` — Hemoglobin Patologi Klinik, masih Draft | superadmin | `422` *"Hasil ini belum dinyatakan selesai oleh analis…"* |
| `VAL-127` — Urinalisis Protein Patologi Klinik yang gugur | superadmin | `422` *"Pemeriksaan ini sudah dibatalkan atau gugur…"* |
| `VAL-126` — hasil Mikrobiologi yang sudah Final; pemeriksaan Patologi Anatomi | superadmin | Keduanya `422` |
| Pemeriksaan tidak dikenal | superadmin | `404` |
| **Lapis jabatan** — Kepala Instalasi, jabatannya tidak memegang `Validate` | dr. Bima | **`403`** dari filter, sebelum service |
| Regresi Reopen — Patologi Klinik Draft (`VAL-107`) | superadmin | `422` — tidak berubah |

**Rincian harness.**

| Skenario | Hasil sebenarnya |
| --- | --- |
| `VAL-126` Mikrobiologi dan Patologi Anatomi; `VAL-127` gugur, batal, order batal; `VAL-124` Draft; `404` | Sesuai, dengan pesan kontrak |
| **Jebakan urutan** — pengguna tanpa kewenangan menekan Validasi pada hasil Draft | `422` *"belum dinyatakan selesai"* — **bukan** `403` |
| `AC-215` nol penunjukan; akun tanpa `WorkforceProfileId`; `AC-230` penunjukan ditangguhkan | `403` masing-masing dengan pesan sebabnya (`AC-233`) |
| `AC-230` — sesudah tiga penolakan | `Version` dan kolom validasi tidak berubah; nol riwayat |
| `VAL-128` — pembaca kewenangan gagal | `503` dengan teks kontrak; **nol** tersimpan |
| `VAL-130` pengisi tidak tercatat; `VAL-129` pengisi tanpa alasan | `422` dengan teks kontrak |
| `VAL-132` — alasan nonaktif, alasan asing, `requiresNote` tanpa catatan, catatan 501 karakter, alasan atau catatan dikirim padahal tidak merangkap | Semuanya `422`; nol riwayat |
| **`AC-229`** — dr. A memvalidasi hasil analis | `200`; pelaku, waktu, `ValidatedByPrivilegeId` = baris penunjukannya; `Version` +1; satu riwayat `ValidateResult` `Finalized` → `Validated` dengan `EncounterId` terisi |
| **`AC-239`** — penempatan utama dr. A *Dokter Umum* (tanpa `Validate`) | Snapshot = *Dokter Penanggung Jawab Laboratorium* — penempatan yang **memberi izin** |
| Respons | `resultStatus = Validated`, nama dan jabatan terisi, penanda kosong, `isReleased = false` |
| `VAL-125` — validasi kedua | `409` *"Hasil ini sudah divalidasi."* |
| **`AC-01`** — pengisi memvalidasi dengan alasan aktif | `200`; penanda **persis** *"Divalidasi oleh pengisi sendiri — dr. Contoh B, Sp.PK — Shift tunggal, tidak ada dokter lain bertugas"*; alasan tersimpan sebagai penunjuk dan snapshot; riwayat membawa kode `SHIFT-TUNGGAL` dan catatannya |
| **`VAL-136`** — Reopen hasil tervalidasi | `409` dengan teks kontrak; hasil tetap Final dan tervalidasi |
| Reopen hasil Final belum tervalidasi | `200`; `Version` +1; `resultStatus = Draft` |
| **Balapan** Reopen lebih dulu / Validasi lebih dulu / dua validasi | Selalu satu `200` dan satu `409` dengan pesan bentrok; **nol** baris tervalidasi yang `FinalizedAt`-nya kosong; baris pemeriksaan milik pihak yang menang |
| `AC-232` | Baris penunjukan Human Resource tidak tersentuh sepanjang skenario |
| Regresi Final | Respons membawa `resultStatus = Final`, ruas validasi kosong |

**Batas harness, disebut apa adanya.**

- **Atomik riwayat pada balapan belum teramati di PostgreSQL.** Pada ketiga balapan, InMemory
  menyimpan **dua** baris riwayat — milik pihak yang menang **dan** yang kalah — walau pembaruan
  baris pemeriksaan pihak yang kalah ditolak. Sebabnya dibuktikan: InMemory tidak punya transaksi,
  sehingga `INSERT` riwayat tetap tertulis sebelum `UPDATE` yang bentrok ditolak. Di PostgreSQL
  keduanya berada dalam satu transaksi `SaveChangesAsync`, dan `UPDATE` yang ditolak token
  `Version` membatalkan `INSERT`-nya. Batas yang sama dicatat `BE-LAB-68`.
- Balapan disimulasikan dengan dua `DbContext`, yang kedua sudah membaca baris sebelum yang pertama
  menyimpan — bukan dua permintaan HTTP sungguhan.
- `Include(Procedure)` atas navigasi wajib berperilaku *inner join* di InMemory; data uji karenanya
  memuat baris prosedur. Di PostgreSQL, FK menjaminnya.

Uji manual: **`NOT FEASIBLE`** — layar `FE-LAB-39` belum dibangun.

**Tidak dijalankan:**

- **Jalur berhasil, penolakan lapis orang, `VAL-136`, dan balapan lewat HTTP.** Ketiga syaratnya
  semuanya menulis ke database bersama: (1) memberi aksi `Validate` kepada jabatan — langkah rilis
  20.7 butir 6, menunggu `UNK-P14-03`; (2) baris penunjukan di Human Resource — menunggu kode
  `LAB-COORD-016` dan penunjukan `DEC-LAB-017`; (3) hasil Patologi Klinik yang Final — dev
  **nol** memilikinya. Dengan superadmin, lapis orang tidak terjangkau lewat HTTP: nol hasil PK
  yang Final, dan hasil Draft berhenti di `VAL-124` lebih dulu.
- `AC-238` dengan akun analis sungguhan — akun analis tidak tersedia. Buktinya struktural:
  `Validate` adalah aksi terpisah dengan nol pemegang.
- Analyzer — build memakai `-p:RunAnalyzers=False`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-196` — Reopen hasil PK Draft `422` | ✅ **Terpenuhi lewat HTTP** | Regresi `VAL-107` |
| `AC-197` — Reopen hasil Final | ✅ **Terpenuhi** pada harness | `200`, `Version` +1, `Draft` |
| `AC-01` — pengisi memvalidasi dengan alasan | ✅ **Terpenuhi** pada harness | Penanda persis 20.10 butir 5 |
| `AC-215` — jabatan calon, nol penunjukan → `403` | ✅ **Terpenuhi** pada harness dan **lewat HTTP 2026-10-06** | Bagian 8 — dr. Bima tanpa penunjukan Mikrobiologi `403` |
| `AC-216` — jabatan tanpa `Validate` → `403` dari filter | ✅ **Terpenuhi lewat HTTP** | dr. Bima `403`. Ia tidak punya baris penunjukan, tetapi filter berjalan **sebelum** service, jadi hasilnya sama |
| `AC-229` — penunjukan aktif → `200`, `ValidatedByPrivilegeId` menunjuk barisnya | ✅ **Terpenuhi** pada harness dan **lewat HTTP 2026-10-06** | Bagian 8 — menunjuk penunjukan `LAB-VAL-PK` |
| `AC-230` — ditangguhkan → `403`, kolom tidak berubah | ✅ **Terpenuhi** pada harness | — |
| `AC-233` — pesan menyebut sebabnya | ✅ **Terpenuhi** pada harness | Tiga sebab lewat service; delapan pada resolver (`BE-LAB-72`) |
| `AC-238` — analis `403` | ✅ **Terpenuhi lewat HTTP** 2026-10-06 | Bagian 8 — Vina `403` dari filter, nol perubahan |
| `AC-239` — snapshot penempatan yang memberi izin | ✅ **Terpenuhi** pada harness | — |
| `VAL-125` | ✅ **Terpenuhi** pada harness | `409` |
| `VAL-126`, `VAL-127` | ✅ **Terpenuhi lewat HTTP** | `422` |
| `VAL-130`, `VAL-132` | ✅ **Terpenuhi** pada harness | `422` |
| Konkurensi Reopen lawan Validasi | ✅ **Terpenuhi** pada harness untuk baris pemeriksaan; balapan dua validasi **teramati di PostgreSQL** 2026-10-06 | Bagian 8 — `200`/`409`, tepat satu baris riwayat |
| Verifikasi — akun analis, dokter A, akun tanpa `WorkforceProfileId` lewat HTTP; balapan sungguhan | ✅ **Terpenuhi** 2026-10-06 | Bagian 8. *Semula belum terpenuhi — butuh izin, penunjukan, dan data* |
| DoD — validasi berjalan; aturannya ditegakkan; Reopen terjaga | ✅ **Terpenuhi pada kode dan harness** | — |
| DoD — laporan `BE-LAB-73.md` | ✅ **Terpenuhi** | Berkas ini |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 230 warning compiler, **nol** dari berkas yang disentuh |
| Masalah yang diketahui | **1.** Nama pemvalidasi pada respons dibaca dari `DisplayName` akun. Kolom itu tidak dapat kosong (`null`) tetapi dapat berisi teks kosong, sehingga pola `??` modul ini tidak jatuh ke `UserName` — kelemahan yang sudah ada di seluruh modul, tidak diubah di sini. **2.** `LabPrivilegeReadException` (`BE-LAB-72`) teksnya diubah agar sama dengan kontrak; nol pemanggil lain |
| Risiko tersisa | **Sedang.** (a) Jalur berhasil belum pernah berjalan terhadap PostgreSQL. (b) Atomik riwayat pada balapan bergantung pada transaksi `SaveChangesAsync`. (c) Sampai `Validate` diberikan, tidak seorang pun dapat memvalidasi — itu disengaja, tetapi layar yang dirilis lebih dulu akan menampilkan tombol yang selalu `403` |
| Perubahan sampingan | `NONE` di repository. Startup menjalankan sinkronisasi registri hak akses yang biasa, sehingga baris `SysActionAccess` `Validate` kini ada di database dev. Harness dan log di scratchpad sesi |
| Interupsi | `NONE` |
| Status Git | Berkas `BE-LAB-73`: `??` `Services/LabResultValidationService.cs`, laporan ini; ` M` `Services/LabExaminationService.cs`, `Controllers/LabExaminationController.cs`, `DTOs/LabExaminationResultDtos.cs`, `Program.cs`, `Services/LabClinicalPrivilegeResolver.cs` (`??`, milik `BE-LAB-72`), `roadmap/backend-roadmap.md`, `roadmap/traceability.md`. Perubahan `BE-LAB-67`..`72` yang belum ter-commit ikut ada. **Nol operasi Git dijalankan** |
| Langkah berikutnya | **1.** `BE-LAB-74` (rilis dan pendaftaran rekam medis) kini `SIAP DIKERJAKAN`: `ReleaseAsync` tinggal ditambahkan ke `LabResultValidationService`, memakai pola yang sama. **2.** Untuk menutup batas verifikasi: tetapkan jabatan dokter berkewenangan (`UNK-P14-03`), beri `Validate`, catat satu penunjukan uji di Human Resource, dan izinkan satu hasil PK uji dibuat Final — lalu jalankan panggilan HTTP dan balapan sungguhan |

## 8. Verifikasi lanjutan 2026-10-06 — HTTP asli

**Status: `BE-LAB-73` ✅ `SELESAI`.** Keempat butir yang belum lewat HTTP kini teramati. `AC-238` ikut teramati.

Atas persetujuan pemilik modul, langkah rilis `MVP-9d` dijalankan sebagai setup uji di devYoga
([`backend-roadmap.md`](../../../roadmap/backend-roadmap.md) 6ak.10): kode `LAB-*` di katalog Human Resource, kredensial
dr. Bima (`LAB-VAL-PK`/`LAB-REL-PK`), `Validate`/`Release`/`Return` beserta izin baca bagi jabatan dokter. Panggilan berjalan
terhadap backend lokal dan PostgreSQL devYoga dengan akun asli: dr. Bima (Kepala Instalasi) dan Vina (analis). Tulis hanya pada
Hemoglobin dan Leukosit `LAB-RSMMC-000001` (pesanan uji). Rincian layar ada di
[`FE-LAB-39.md`](../frontend/FE-LAB-39.md) bagian 9.

| Butir batas lama | Bukti HTTP asli | Hasil |
| --- | --- | --- |
| Jalur berhasil (`AC-229`, `AC-239`) | dr. Bima memvalidasi Hemoglobin dan Leukosit → `200` *"Hasil divalidasi. Hasil ini belum dirilis."*; `ValidatedByPrivilegeId` menunjuk penunjukan `LAB-VAL-PK` (kueri baca-saja); Halaman Hasil menulis *"Validasi oleh: dr. Bima Prasetya, Sp.PK — Kepala Instalasi Laboratorium"* | `PASS` |
| Penolakan lapis orang (`AC-215`, `AC-233`) | dr. Bima (jabatan ber-`Validate`, penunjukan Patologi Klinik saja) memvalidasi BTA `LAB-RSMMC-000014` → `403` *"Anda belum ditunjuk sebagai pemegang kewenangan validasi Mikrobiologi."*. Akun tanpa `WorkforceProfileId` (superadmin) → `403` *"Akun Anda belum terhubung dengan data tenaga kerja…"* (2026-09-30, [`BE-LAB-78.md`](BE-LAB-78.md) bagian 5). Keduanya lewat `ValidateAsync` yang sama; dev tidak punya hasil PK Final tanpa pemegang untuk mengulanginya khusus PK | `PASS` |
| `AC-238` — analis | Vina (analis, tanpa `Validate`) memvalidasi Hemoglobin → `403` *"Anda tidak memiliki akses ke menu atau fitur ini."* dari filter; keadaan hasil sama sebelum dan sesudah | `PASS` |
| `VAL-136` | Vina membuka kembali Leukosit (tervalidasi, belum dirilis), Hemoglobin, dan BTA (dirilis) → `409` *"Hasil ini sudah divalidasi. Minta pemvalidasi atau perilis mengembalikannya bila perlu diubah."*; `reopenCount` tetap | `PASS` |
| Balapan sungguhan | Vina menyatakan Leukosit selesai lagi (`200`), lalu dua `POST …/result/validate` dr. Bima dikirim bersamaan → `200` dan `409` *"Hasil ini baru saja diubah orang lain. Muat ulang lalu ulangi."* — jalur token `Version`, bukan `VAL-125`. `LabTransitionHistory` memuat **tepat satu** `ValidateResult` untuk siklus itu: atomik riwayat teramati di PostgreSQL | `PASS` |

**Risiko tersisa: rendah.** Analyzer tidak dijalankan (`-p:RunAnalyzers=False`).

**Jejak di devYoga:** Hemoglobin **Dirilis**; Leukosit **Tervalidasi** oleh dr. Bima (menunggu rilis) sesudah satu
pengembalian `SAMPEL-TERTUKAR`. Data uji. **Nol perubahan kode. Nol operasi Git dijalankan.**
