# Laporan Perubahan Backend — `BE-LAB-74`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-74` |
| Judul | Rilis, pendaftaran rekam medis, dan penjaga batal |
| Slice | Gelombang `MVP-9b` — `EPIC-LAB-15`, tindakan `S4` validasi dan rilis hasil |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian **6ak.5** |
| Trace | `FR-15.2`, `FR-15.3` bagian rilis, `FR-15.5` bagian perilis, `FR-15.8`, `FR-15.13`; `LAB-INH-007`, `LAB-DEC-017`, `LAB-DEC-120`; `02-backend-architecture.md` 20.10 butir 1, 3, 6, 10 |
| Contract version | `LAB-API-v1` **`r34`** 29.2, 29.3, 29.7; `LAB-VAL-v1` **`r12`** `VAL-131`, `VAL-133`, `VAL-137`, `VAL-143`; `LAB-INT-v1` **`r4`** `INT-08` — seluruhnya **`approved` 2026-09-25** |
| Dependency | `BE-LAB-73` ⚠ — `LabResultValidationService` dan `ValidateAsync` berdiri |
| Klasifikasi | `HEAVY` — skor 10: repository 0, berkas diperiksa 2 (>20), berkas diubah 1 (4), logika bisnis **2**, kontrak API **2** (endpoint baru, ruas bertambah, penjaga baru pada endpoint lama), database 1, keamanan/auth **2**, UI/workflow 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/LaboratoryManagement/`, dokumen blueprint Laboratorium |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `017d1819` (branch `yoga`), di atas `BE-LAB-67`..`73` yang belum ter-commit |
| Tanggal | 2026-09-29 |
| Status | ✅ **`SELESAI`** (naik 2026-10-06) — rilis yang berhasil beserta dokumen rekam medisnya kini teramati **lewat HTTP asli** terhadap PostgreSQL devYoga: Hemoglobin dirilis dr. Bima, tepat satu `MrcClinicalDocumentIntegrity` tertanda tangan dan terkunci. Lihat 8. *(Semula: ⚠ — kode lengkap; build 0 error tanpa warning baru; startup lolos dengan registri **1574**; **28 dari 28 skenario rilis** dan **38 dari 38 regresi validasi** lolos pada harness EF InMemory, termasuk **rilis atomik pada jalur gagal** (`DbContext` bersih, nol tersimpan); lima panggilan HTTP penolakan lolos tanpa menulis data. **Belum lewat HTTP:** rilis yang berhasil beserta dokumen rekam medisnya — butuh hasil tervalidasi, aksi `Release`, dan penunjukan rilis di database bersama)* |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `LaboratoryManagement`; memanggil `MedicalRecordManagement` (`Mrc`) **tanpa mengubahnya** |
| Pemilik / prefix registry | `Lab` — `ACTIVE`. Dokumen yang didaftarkan milik Rekam Medis — ditulis lewat service pemiliknya, `RegisterSignedAsync`, bukan langsung |
| Keberlakuan | `NEW CODE` — `ReleaseAsync`, endpoint `release`, ruas rilis; `TOUCHED LEGACY` — `CancelAsync` (penjaga `VAL-143`), `BuildCompletionResponse`, `LabExaminationController`, `LabExaminationResultDtos.cs`; `LabResultValidationService` direfaktor (satu aturan empat mata bagi validasi dan rilis) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001` (`[AccessAction("Release")]` + `[AccessPermission("LabExaminationResult", "Release")]`), `QBE-VAL-001` (`VAL-131`, `133`, `137`, `143`), `QBE-DTO-001`, `QBE-TXN-001` (**satu `SaveChangesAsync`** untuk rilis, riwayat, dan dokumen rekam medis), `QBE-LOG-001`, `QBE-AUD-001`. **Tidak berlaku:** `QBE-ENT-*`, `QBE-CFG-*`, `QBE-CODE-*` |
| Governance yang dibaca | Sama dengan `BE-LAB-73` — `AGENTS.md`, `docs/engineering/`, `rules/backend/` termasuk `API_RULES.md` dan `transaction-endpoint-standard.md` |

---

## 1. Masalah yang diperbaiki

**Hasil yang sudah divalidasi belum dapat keluar.** Rilis adalah saat hasil menjadi **dokumen klinis
pasien** — boleh dikirim, dibaca dokter pemesan, dan tercatat di rekam medis (`LAB-DEC-017`). Tanpa
rilis, `isReleased` selalu salah dan setiap hasil membawa *"belum boleh dikirim kepada pasien"*.

**Yang berisiko bukan rilisnya, melainkan setengah-rilis.** Bila hasil tercatat dirilis tetapi
dokumen rekam medisnya gagal terdaftar, hasil itu sudah dibaca dokter namun **tidak pernah dapat
dikoreksi**, sebab mesin koreksi rekam medis tidak mengenalnya. Keadaan sebaliknya juga berbahaya:
dokumen terkunci untuk hasil yang tidak pernah dirilis.

**Celah kedua: pembatalan.** Tanpa penjaga, pemeriksaan yang hasilnya sudah dirilis dan dibaca
dokter dapat dibatalkan, sehingga hasilnya hilang dari pandangan tanpa jejak koreksi.

---

## 2. Proses bisnis

**Pelaku.** Orang **kedua** yang ditunjuk merilis — pemegang aksi `Release` pada jabatannya **dan**
kode rilis Patologi Klinik pada kredensial Human Resource (`LAB-DEC-120`).

**Alur Rilis, berurutan:**

| Langkah | Pemeriksaan / tindakan | Bila gagal |
| ---: | --- | --- |
| 0 | Filter hak akses — aksi `Release` | `403` |
| 1 | Pemeriksaan ada; Patologi Klinik (`VAL-126`); tidak gugur atau batal (`VAL-127`) | `404` / `422` |
| 2 | Sudah divalidasi; belum dirilis (`VAL-133`) | `422` / `409` |
| 3 | **Lapis orang** — kode **rilis** yang berlaku hari itu. Pemegang kode validasi saja **ditolak** | `403` bersebab; `503` bila tidak terbaca |
| 4 | Empat mata — pemvalidasi yang merilis wajib membawa alasan aktif (`VAL-131`, `VAL-132`) | `422` |
| 5 | **Daftarkan dokumen rekam medis lebih dulu**, tanpa menyimpan: `LaboratoryResult`, id pemeriksaan, pasien **dari kunjungan order**, kunjungan order, perilis sebagai penulis sekaligus penanda tangan, perangkat, alamat jaringan, waktu rilis | `422` `VAL-137` — **belum ada satu perubahan pun** |
| 6 | Tujuh kolom rilis, satu riwayat `ReleaseResult`, `Version` naik | — |
| 7 | **Satu `SaveChangesAsync`** untuk semuanya | `409` bila bentrok |

**Kenapa pendaftaran di langkah 5, sebelum kolom rilis diubah.** `RegisterSignedAsync` tidak
menyimpan dan melempar galat **sebelum** menambah baris bila datanya tidak sah. Dengan memanggilnya
lebih dulu, kegagalan meninggalkan `DbContext` yang **bersih total**. Artinya tidak sekadar "belum
disimpan", tetapi memang tidak ada perubahan tertunda yang dapat terbawa penyimpanan lain.

**Contoh berhasil.** Kalium 7,2 divalidasi dr. A pukul 02.12. Pukul 02.14 perilis B menekan Rilis.
Pada penyimpanan yang **sama** tersimpan:

| Tempat | Isi |
| --- | --- |
| `LabExamination` | `ReleasedAt` 02.14, perilis B, jabatannya saat itu, baris kode rilisnya |
| `LabTransitionHistory` | Satu baris `LabExamination.ReleaseResult`, `Validated` → `Released` |
| `MrcClinicalDocumentIntegrity` | **Satu** baris `LaboratoryResult`, `Signed`, `LockedAt` 02.14, pemicu `AuthorSigned`, penulis dan penanda tangan = perilis B |

Respons: `resultStatus = Released`, `isReleased = true`, `deliveryBlockedReason` **kosong**, *Validasi
oleh* dr. A, *Otorisasi oleh* perilis B.

**Contoh gagal (`VAL-137`).** Order yang kunjungannya tidak dapat ditemukan menghasilkan `422`:
*"Hasil tidak dapat dirilis karena pendaftaran ke rekam medis gagal: Id pasien tidak valid. Hasil
tetap tervalidasi."* Tidak ada `ReleasedAt`, riwayat, maupun dokumen yang tersimpan, dan `Version`
tetap. Hasil dapat dirilis ulang begitu kunjungannya dibetulkan.

**Dokter tunggal (`AC-02`).** Bila dr. C memvalidasi lalu merilis sendiri, rilis tanpa alasan →
`422` *"Anda yang memvalidasi hasil ini. Rilis oleh orang yang sama memerlukan alasan
pengecualian."* Dengan alasan aktif → diterima, dengan penanda berbunyi persis:

> *Dirilis oleh pemvalidasi sendiri — dr. C — Shift tunggal, tidak ada dokter lain bertugas*

**Kunjungan yang sudah ditutup.** Rilis **tetap berjalan**, dan dokumennya langsung tertanda tangan
dan terkunci. Hasil pasien rawat jalan yang keluar sesudah pasien pulang adalah hal yang lazim.

**Penjaga batal (`VAL-143`).** Membatalkan pemeriksaan yang hasilnya **sudah dirilis** → `422`
*"Pemeriksaan yang hasilnya sudah dirilis tidak dapat dibatalkan. Hasilnya hanya dapat diperbaiki
lewat koreksi."* Pemeriksaan yang tervalidasi tetapi **belum** dirilis tetap boleh dibatalkan. Ini
arah sementara `ARCH-GAP-LAB-09` sampai `DEC-LAB-019` dijawab.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / dokumen | Kenapa dibaca |
| --- | --- |
| `roadmap/backend-roadmap.md` 6ak.5 | Cakupan, AC, tiga jebakan |
| `contracts/api-contract.md` 29.2, 29.3, 29.7; `contracts/integration-contract.md` 8.3 | Endpoint, ruas rilis, `INT-08` |
| `contracts/validation-matrix.md` 14.1 | Teks `VAL-131`, `133`, `137`, `143` |
| `MedicalRecordManagement/Services/ClinicalDocumentIntegrityService.cs` 53-260 | `RegisterSignedAsync` — tidak menyimpan, kapan melempar, idempotensinya; `JenisYangDitegakkan` |
| `PharmacyManagement/Services/ConsultationFinalizationService.cs` 150-215 | Pola gagal-pendaftaran → penolakan terbaca |
| `ClinicalManagement/Controllers/*` | Cara pemanggil lain mengambil perangkat dan alamat jaringan |
| `RegistrationManagement/Models/RegPatientEncounter.cs`, `LabOrder.cs` | Jalan ke `PatientId` lewat kunjungan order |
| `Services/Logging/LoggerService.cs` | Memastikan logger **tidak** menulis lewat `DbContext` yang sama |
| `Services/LabExaminationService.cs` — `CancelAsync`, `BuildCompletionResponse` | Tempat penjaga batal dan ruas rilis |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Services/LabResultValidationService.cs` | **`ReleaseAsync`** baru. Aturan bersama diekstrak supaya tidak bercabang antara validasi dan rilis: `EnsureClinicalPathologyAndRunning` (`VAL-126`/`127`), `EnsureAppointedAsync` (lapis orang), `ResolveFourEyesExceptionAsync` (`VAL-129`/`131`/`132`), `AddHistory`, `SaveAsync` (bentrok `Version` **dan** pelanggaran unik `23505` → `409`), `ReadNamesAsync` (satu kueri untuk nama pemvalidasi dan perilis). Konstruktor menerima `ClinicalDocumentIntegrityService` |
| `Services/LabExaminationService.cs` | `CancelAsync`: `VAL-143`. `BuildCompletionResponse`: `isReleased` dari `ReleasedAt`, `deliveryBlockedReason` kosong bila dirilis, lima ruas rilis, penanda rilis |
| `Controllers/LabExaminationController.cs` | Endpoint `POST /{id}/result/release` — mengirim `User-Agent` dan alamat jaringan pemanggil. `cancel` mendokumentasikan `422` di Swagger |
| `DTOs/LabExaminationResultDtos.cs` | Lima ruas rilis pada `LabExaminationCompletionResponse` |

**Berkas milik Rekam Medis: nol baris berubah.** `JenisYangDitegakkan` tidak disentuh (20.9).
**Nol entity, nol migration, nol seeder.**

**Selisih dan keputusan.**

| Hal | Keputusan dan alasan |
| --- | --- |
| Urutan pendaftaran | Roadmap menulis *"kolom rilis, riwayat, lalu `RegisterSignedAsync`"*. Yang dibangun: **pendaftaran dulu, baru kolom** — tetap satu `SaveChangesAsync`. Atomiknya sama; bedanya, kegagalan tidak meninggalkan perubahan tertunda di `DbContext` |
| Pasien tidak ditemukan dari kunjungan | Dikirim sebagai `Guid.Empty`, sehingga `RegisterSignedAsync` sendiri yang menolak (*"Id pasien tidak valid."*) — aturan milik Rekam Medis tidak ditulis ulang di Laboratorium |
| Rilis ganda bersamaan | Pihak kedua ditolak oleh token `Version` **atau** index unik `(DocumentKind, DocumentId)` milik Rekam Medis — keduanya dijawab `409` dengan pesan bentrok yang sama |
| `IdempotencyKey`, `AvailableActions` | Tidak ada di kontrak `r34`; klik ganda dijaga `VAL-133` (`409` *"sudah dirilis"*). Sama dengan `BE-LAB-73` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif, sesuai `r34`.** Satu endpoint baru. `isReleased` kini bernilai sebenarnya bagi Patologi Klinik, dan `deliveryBlockedReason` menjadi kosong sesudah rilis. Lima ruas baru. `POST /{id}/cancel` memperoleh jawaban `422` baru (`VAL-143`) |
| Database | **Nol migration.** Menulis tujuh kolom rilis, satu `LabTransitionHistory`, dan satu `MrcClinicalDocumentIntegrity` lewat service Rekam Medis — **satu transaksi**. **Nol data ditulis ke database bersama** oleh task ini, selain sinkronisasi registri hak akses saat startup |
| Keamanan/Auth | **Aksi baru `LabExaminationResult : Release`** — registri 1573 → **1574**; terdaftar, **nol pemegang**. Pemegang `Validate` tidak memperoleh `Release`, dan pemegang kode validasi saja ditolak lapis orang. Tanda tangan mencatat perangkat dan alamat jaringan. Log tanpa nilai hasil maupun catatan pengecualian |

---

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Examination

Base URL: `api/v1/health-services/laboratory-management/lab-examinations`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/{id}/result/release` | **Baru.** Merilis hasil Patologi Klinik tervalidasi sehingga menjadi dokumen klinis pasien, sekaligus tertanda tangan dan terkunci di rekam medis. Badan: `exceptionReasonId`, `exceptionNote` — hanya bila perilis juga pemvalidasi | `LabExaminationResult : Release` |
| `POST` | `/{id}/cancel` | Tetap — kini **ditolak `422`** bila hasil pemeriksaan sudah dirilis | `LabExamination : Update` |

**Kode status `release`:** `200`; `403` tanpa aksi `Release` atau tidak ditunjuk (sebabnya
disebut); `404`; `409` sudah dirilis atau bentrok; `422` `VAL-126`, `VAL-127`, `VAL-131`, `VAL-132`,
`VAL-133`, `VAL-137`; `503` data kewenangan tidak terbaca.

**Ruas baru pada respons kelengkapan:** `releasedAt`, `releasedByUserId`, `releasedByName`,
`releasedByPositionName`, `releaseExceptionMarker`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` | **0 error, 230 warning**; **nol** dari berkas yang disentuh; 91 detik | `PASS` | Keluaran build |
| Startup Development | Seluruh seeder lolos; registri **1574** — naik tepat satu | `PASS` | Log startup |
| Registri di database | `Update`, `Validate`, `Release` pada `LabExaminationResult`; **ketiganya nol pemegang** | `PASS` | Kueri baca-saja |
| Swagger | `release` dengan `200,403,404,409,422,503`; `cancel` kini mencantumkan `422` | `PASS` | Dokumen Swagger |
| Harness rilis EF InMemory | **28 `PASS`, 0 `FAIL`** | `PASS` | Rincian di bawah |
| Regresi harness `BE-LAB-73` sesudah refaktor | **38 `PASS`, 0 `FAIL`** | `PASS` | Aturan empat mata yang diekstrak tidak mengubah perilaku validasi |
| Panggilan HTTP — lima penolakan | Kelimanya sesuai | `PASS` | Rincian di bawah |
| Rekonsiliasi `INT-08` pada database dev | 0 pemeriksaan dirilis = 0 dokumen `LaboratoryResult` | `PASS` | Kueri baca-saja |
| `LabExamination` sebelum dan sesudah HTTP | 10 baris identik | `PASS` | Kueri baca-saja |
| Berkas milik Rekam Medis | Nol baris berubah | `PASS` | `git diff --stat` |
| Rilis berhasil **lewat HTTP** | Tidak dijalankan | `NOT RUN` | Lihat *Tidak dijalankan* |

**Rincian HTTP.**

| Skenario | Akun | Hasil |
| --- | --- | --- |
| `VAL-133` — Hemoglobin Patologi Klinik belum divalidasi | superadmin | `422` *"Hasil ini belum divalidasi, jadi belum dapat dirilis."* |
| `VAL-127` — pemeriksaan Patologi Klinik yang gugur | superadmin | `422` |
| `VAL-126` — hasil Mikrobiologi yang sudah Final | superadmin | `422` |
| Pemeriksaan tidak dikenal | superadmin | `404` |
| Lapis jabatan — Kepala Instalasi tanpa aksi `Release` | dr. Bima | **`403`** dari filter |

**Rincian harness rilis.**

| Skenario | Hasil sebenarnya |
| --- | --- |
| `VAL-133` belum divalidasi; `VAL-126` Mikrobiologi; `VAL-127` gugur; `404` | Sesuai, dengan teks kontrak |
| **`AC-217`/`AC-231`** — pemegang kode validasi saja merilis | `403` *"Anda belum ditunjuk sebagai pemegang kewenangan rilis Patologi Klinik."* |
| Pembaca kewenangan gagal | `503`; nol rilis, nol dokumen |
| **Perilis B merilis hasil dr. A** | `200`; tujuh kolom; `ReleasedByPrivilegeId` = baris kode rilisnya; snapshot jabatan perilis; `Version` +1; satu riwayat `ReleaseResult` |
| **`INT-08` — dokumen rekam medis** | **Tepat satu** baris `LaboratoryResult`; `PatientId` = pasien **kunjungan order**; `EncounterId` = kunjungan order; `Signed`; `LockedAt` = `SignedAt` = `ReleasedAt`; pemicu `AuthorSigned`; penulis = penanda tangan = perilis; perangkat dan alamat jaringan tercatat |
| Respons | `Released`, `isReleased`, `deliveryBlockedReason` kosong, nama pemvalidasi dan perilis |
| Rilis kedua | `409` *"Hasil ini sudah dirilis."*; tetap satu dokumen |
| **`VAL-131`** — pemvalidasi merilis tanpa alasan; `VAL-132` alasan tanpa merangkap | `422` masing-masing |
| **`AC-02`** — pemvalidasi merilis dengan alasan aktif | `200`; penanda **persis** 20.10 butir 5; alasan dan kodenya tercatat |
| **`VAL-137` — kunjungan tidak sah** | `422` dengan sebabnya; **`DbContext` bersih — nol entity berubah**; nol `ReleasedAt`, nol riwayat, nol dokumen; `Version` tetap; hasil tetap tervalidasi |
| **Kunjungan tertutup** (`Completed`) | `200`; dokumen tertanda tangan dan terkunci |
| **Balapan dua rilis** | Satu `200`, satu `409`; baris milik yang menang; **tetap satu dokumen** |
| **`VAL-143`** — batal sesudah rilis; batal tervalidasi belum dirilis | `422` dan tetap dirilis / `200` |
| **Rekonsiliasi `INT-08`** | 4 pemeriksaan dirilis = 4 dokumen `LaboratoryResult` |
| `AC-232` | Penunjukan Human Resource tidak tersentuh |

**Batas harness, disebut apa adanya.**

- **Balapan:** baris riwayat pihak yang kalah ikut tersimpan di InMemory, karena InMemory tidak punya
  transaksi (terbukti pada `BE-LAB-73`). Dokumen rekam medis **tetap satu**: pihak yang kalah
  menemukan dokumen yang sudah tertanda tangan dan tidak menambah baris.
- Navigasi wajib (`Procedure`, `Specimen`) berperilaku *inner join* di InMemory, sehingga data uji
  memuat baris induknya.

Uji manual: **`NOT FEASIBLE`** — layar `FE-LAB-39` belum dibangun.

**Tidak dijalankan:**

- **Rilis yang berhasil lewat HTTP**, dan karena itu juga pemeriksaan baris `MrcClinicalDocumentIntegrity`
  di PostgreSQL serta jalur gagal `VAL-137` sungguhan. Syaratnya — hasil tervalidasi, aksi `Release`,
  penunjukan kode rilis, dan kunjungan uji yang rusak — seluruhnya berarti menulis ke database
  bersama tanpa instruksi. Dev hari ini punya **nol** hasil tervalidasi.
- Analyzer — build memakai `-p:RunAnalyzers=False`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-217` bagian rilis; `AC-231` | ✅ **Terpenuhi** pada harness | Pemegang kode validasi saja `403` |
| `AC-198` bagian backend — hasil dirilis terbaca `Released` | ✅ **Terpenuhi** pada harness | `resultStatus`, `isReleased`, `deliveryBlockedReason` |
| `VAL-131` | ✅ **Terpenuhi** pada harness | `422` |
| `VAL-133` | ✅ **Terpenuhi** — `422` lewat HTTP, `409` pada harness | — |
| `VAL-143` | ✅ **Terpenuhi** pada harness | `422`; pembatalan sebelum rilis tetap boleh |
| `INT-08` berhasil — tepat satu dokumen `Signed`, `LockedAt` = waktu rilis, penulis = perilis | ✅ **Terpenuhi** pada harness dan **lewat HTTP 2026-10-06** | Bagian 8 — satu baris di PostgreSQL |
| `INT-08` gagal — basis data **tidak berubah sama sekali** | ✅ **Terpenuhi** pada harness | Bahkan `DbContext` bersih |
| `INT-08` kunjungan tertutup | ✅ **Terpenuhi** pada harness | `200`, terkunci |
| Konkurensi dua rilis | ✅ **Terpenuhi** pada harness | Satu `200`, satu `409`, satu dokumen |
| Rekonsiliasi `INT-08` | ✅ **Terpenuhi** pada harness (4 = 4) dan dev (0 = 0) | — |
| DoD — rilis atomik terbukti pada jalur gagal | ✅ **Terpenuhi** pada harness | — |
| DoD — penjaga batal berjalan | ✅ **Terpenuhi** | — |
| DoD — laporan `BE-LAB-74.md` | ✅ **Terpenuhi** | Berkas ini |
| Verifikasi — rilis sungguhan lewat HTTP dan baris rekam medis di PostgreSQL | ✅ **Terpenuhi** 2026-10-06 | Bagian 8. *Semula belum terpenuhi* |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 230 warning compiler, **nol** dari berkas yang disentuh |
| Masalah yang diketahui | **1.** Bila dokumen `LaboratoryResult` untuk pemeriksaan yang sama **sudah ada dan terkunci**, `RegisterSignedAsync` mengembalikannya tanpa tanda tangan kedua (perilaku Rekam Medis). Keadaan itu tidak dapat terjadi lewat jalur resmi, karena rilis kedua ditolak `VAL-133` dan *Kembalikan* sesudah rilis ditolak `VAL-134`. **2.** `LaboratoryResult` sengaja **tidak** masuk `JenisYangDitegakkan`, sehingga `EnsureMutableAsync` belum mengunci hasil lab terhadap penyuntingan lewat jalur Rekam Medis — penjaga di sisi Laboratorium adalah `VAL-120`/`136`/`143`. Penegakannya milik pemilik Rekam Medis bersama `S6` |
| Risiko tersisa | **Sedang.** (a) Rilis yang berhasil belum pernah berjalan terhadap PostgreSQL. (b) Sampai `Release` diberikan, tidak seorang pun dapat merilis — disengaja. (c) Rilis tidak menerbitkan fakta tagihan maupun pemberitahuan — sesuai 29.7 |
| Perubahan sampingan | `NONE` di repository. Startup menjalankan sinkronisasi registri, sehingga baris `SysActionAccess` `Release` kini ada di database dev. Harness di scratchpad sesi |
| Interupsi | `NONE` |
| Status Git | Berkas `BE-LAB-74`: laporan ini (`??`); ` M` `Services/LabExaminationService.cs`, `Controllers/LabExaminationController.cs`, `DTOs/LabExaminationResultDtos.cs`; `Services/LabResultValidationService.cs` (`??`, lahir `BE-LAB-73`); `roadmap/backend-roadmap.md`, `roadmap/traceability.md`. Perubahan `BE-LAB-67`..`73` yang belum ter-commit ikut ada. **Nol operasi Git dijalankan** |
| Langkah berikutnya | **1.** `BE-LAB-75` (*Kembalikan ke analis*) kini `SIAP DIKERJAKAN`: `ReturnToAnalystAsync` di service yang sama, memakai `LabResultCorrectionReason` yang sudah terisi. **2.** Untuk menutup batas verifikasi `BE-LAB-73`/`74` sekaligus: satu hasil PK uji, jabatan dokter dan perilis dengan `Validate`/`Release`, dan penunjukan uji di Human Resource — lalu validasi, rilis, dan periksa baris rekam medisnya |

## 8. Verifikasi lanjutan 2026-10-06 — HTTP asli

**Status: `BE-LAB-74` ✅ `SELESAI`.** Rilis yang berhasil beserta dokumen rekam medisnya kini teramati.

Atas persetujuan pemilik modul, langkah rilis `MVP-9d` dijalankan sebagai setup uji di devYoga
([`backend-roadmap.md`](../../../roadmap/backend-roadmap.md) 6ak.10): kode `LAB-*` di katalog Human Resource, kredensial
dr. Bima (`LAB-VAL-PK`/`LAB-REL-PK`), `Validate`/`Release`/`Return` beserta izin baca bagi jabatan dokter. Panggilan berjalan
terhadap backend lokal dan PostgreSQL devYoga dengan akun asli: dr. Bima (Kepala Instalasi) dan Vina (analis). Tulis hanya pada
Hemoglobin dan Leukosit `LAB-RSMMC-000001` (pesanan uji). Rincian layar ada di
[`FE-LAB-39.md`](../frontend/FE-LAB-39.md) bagian 9.

| Butir | Bukti HTTP asli | Hasil |
| --- | --- | --- |
| Rilis berhasil, empat mata (`VAL-131`, `AC-02`) | dr. Bima, pemvalidasi Hemoglobin, merilis dengan alasan *Shift tunggal, tidak ada dokter lain bertugas* → `200` *"Hasil dirilis dan tercatat pada rekam medis pasien."*; `ReleasedByPrivilegeId` menunjuk penunjukan `LAB-REL-PK` (kueri baca-saja); Halaman Hasil menulis *Dirilis*, *Otorisasi oleh*, dan penanda pengecualian | `PASS` |
| Dokumen rekam medis (`INT-08`) | **Tepat satu** `MrcClinicalDocumentIntegrity` (`DocumentKind` 15) bagi Hemoglobin, ditandatangani dan dikunci saat rilis; Leukosit yang tervalidasi tetapi belum dirilis **nol** dokumen | `PASS` |
| Sesudah rilis | Hemoglobin keluar dari antrean *Menunggu Rilis*; *Buka Kembali* oleh analis `409` (`VAL-136`); analis `403` pada validasi | `PASS` |
| Mikrobiologi | Rilis BTA `LAB-RSMMC-000014` oleh dr. Nabila juga `200` dengan tepat satu dokumen ([`BE-LAB-78.md`](BE-LAB-78.md) bagian 8) | `PASS` |

**Tetap tidak dijalankan:** jalur gagal `VAL-137` sungguhan. Uji itu butuh merusak kunjungan uji di database bersama. Rilis
atomik pada jalur gagal terbukti pada harness (`DbContext` bersih, nol tersimpan). Balapan dua rilis terbukti pada harness
(juga khusus Mikrobiologi, `BE-LAB-78`), dan token `Version` yang sama teramati menolak balapan validasi di PostgreSQL
([`BE-LAB-73.md`](BE-LAB-73.md) bagian 8). Analyzer tidak dijalankan.

**Risiko tersisa: rendah.**

**Jejak di devYoga:** Hemoglobin **Dirilis**; Leukosit **Tervalidasi** oleh dr. Bima (menunggu rilis) sesudah satu
pengembalian `SAMPEL-TERTUKAR`. Data uji. **Nol perubahan kode. Nol operasi Git dijalankan.**
