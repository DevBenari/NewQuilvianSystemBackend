# Laporan Perubahan Backend — `BE-LAB-75`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-75` |
| Judul | *Kembalikan ke analis* |
| Slice | Gelombang `MVP-9b` — `EPIC-LAB-15`, tindakan `S4` validasi dan rilis hasil |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian **6ak.6** |
| Trace | `FR-15.6`, `FR-15.15` bagian pengembalian; `LAB-DEC-138`; `INV-47` |
| Contract version | `LAB-API-v1` **`r34`** 29.2-29.3; `LAB-VAL-v1` **`r12`** `VAL-134`, `VAL-135`, `VAL-138`; `LAB-STATE-v1` **`r5`** 7.2 — seluruhnya **`approved` 2026-09-25** |
| Dependency | `BE-LAB-74` ⚠ — jalur `409` bagi hasil yang sudah dirilis membutuhkan rilis berdiri; `BE-LAB-71` ✅ — daftar alasan koreksi sudah terisi |
| Klasifikasi | `MEDIUM` — skor 8: repository 0, berkas diperiksa 1 (±12), berkas diubah 0 (3), logika bisnis **2**, kontrak API **2** (endpoint baru), database 1, keamanan/auth **2**, UI/workflow 0 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/LaboratoryManagement/`, dokumen blueprint Laboratorium |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `017d1819` (branch `yoga`), di atas `BE-LAB-67`..`74` yang belum ter-commit |
| Tanggal | 2026-09-29 |
| Status | ✅ **`SELESAI`** (naik 2026-10-06) — pengembalian yang berhasil kini teramati **lewat HTTP asli** terhadap PostgreSQL devYoga (Leukosit oleh dr. Bima, BTA oleh dr. Nabila); riwayat validasi lama utuh di `LabTransitionHistory`. Lihat 8. *(Semula: ⚠ — kode lengkap; build 0 error tanpa warning baru; startup lolos dengan registri **1575**; **20 dari 20 skenario** lolos pada harness EF InMemory, termasuk **riwayat validasi lama utuh sampai ke setiap ruasnya**; regresi `BE-LAB-73` **38/38** dan `BE-LAB-74` **28/28**; lima panggilan HTTP penolakan lolos tanpa menulis data. **Belum lewat HTTP:** pengembalian yang berhasil — dev nol punya hasil tervalidasi)* |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `LaboratoryManagement` |
| Pemilik / prefix registry | `Lab` — `ACTIVE` |
| Keberlakuan | `NEW CODE` — `ReturnToAnalystAsync`, endpoint `return`, `LabResultReturnRequest`; `TOUCHED LEGACY` — `LabExaminationController`, `LabExaminationResultDtos.cs`; `LabResultValidationService.AddHistory` digeneralisasi |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001` (`[AccessAction("Return")]` + `[AccessPermission("LabExaminationResult", "Return")]`), `QBE-VAL-001` (`VAL-134`, `135`, `138`), `QBE-DTO-001`, `QBE-TXN-001`, `QBE-LOG-001`, `QBE-AUD-001` (riwayat hanya **ditambah**, tidak pernah diubah). **Tidak berlaku:** `QBE-ENT-*`, `QBE-CFG-*`, `QBE-CODE-*` |
| Governance yang dibaca | Sama dengan `BE-LAB-73`/`74` |

---

## 1. Masalah yang diperbaiki

**Perilis yang menemukan kesalahan pada hasil tervalidasi hanya dapat menolak merilis.** Contohnya
sampel yang tertukar antar pasien, atau angka yang salah ketik. Hasilnya tertahan dalam keadaan
*tervalidasi*: analis tidak dapat membukanya kembali (`VAL-136`, `BE-LAB-73`), dan tidak ada jalan
mengembalikannya untuk diperbaiki.

**Jalan pintasnya pun berbahaya.** Menghapus atau menimpa baris validasi supaya riwayat "bersih"
menghilangkan bukti bahwa seorang dokter pernah mengesahkan angka yang ternyata salah. Justru
kejadian itulah yang dibutuhkan laporan mutu.

---

## 2. Proses bisnis

**Pelaku.** Pemegang aksi `Return` pada jabatannya **dan** pemegang kode validasi **atau** kode rilis
Patologi Klinik pada kredensial Human Resource (`LAB-DEC-138` butir 1).

**Alur, berurutan** (`LAB-VAL-v1` `r12` 14.1):

| Langkah | Pemeriksaan | Bila gagal |
| ---: | --- | --- |
| 0 | Filter hak akses — aksi `Return` | `403` |
| 1 | Pemeriksaan ada; Patologi Klinik (`VAL-126`); tidak gugur atau batal (`VAL-127`) | `404` / `422` |
| 2 | Sudah divalidasi; **belum** dirilis (`VAL-134`) | `422` *"Hasil ini belum divalidasi; analis dapat membukanya kembali sendiri."* / `409` *"Hasil yang sudah dirilis hanya dapat diubah lewat koreksi."* |
| 3 | Lapis orang — kode validasi **atau** rilis yang berlaku (`VAL-138`) | `403` *"Anda bukan pemegang kewenangan validasi atau rilis Patologi Klinik."*; `503` bila data kewenangan tidak terbaca |
| 4 | Alasan dari daftar koreksi, **aktif**; catatan bila alasannya mewajibkan, maks. 500 (`VAL-135`) | `422` |
| 5 | Tulis — satu `SaveChangesAsync`, `Version` naik | `409` bila bentrok |

**Kenapa lapis orang diperiksa di service, bukan lewat atribut.** Satu atribut hak akses tidak dapat
menyatakan *"kode validasi **atau** kode rilis"*. Service memeriksa kode validasi lebih dulu, lalu
kode rilis bila yang pertama tidak ada.

**Yang terjadi bila berhasil — contoh (`AC-205`, `AC-206`).** Kalium 7,2 divalidasi dr. A pukul
02.12. Pukul 02.13 perilis B menyadari sampelnya tertukar dan menekan *Kembalikan* dengan alasan
*Sampel tertukar*:

| Yang berubah | Isi sesudahnya |
| --- | --- |
| `FinalizedAt`, `FinalizedByUserId` | **Kosong** — hasil kembali Draft, di tangan analis |
| Tujuh kolom validasi | **Kosong** — hasil tidak lagi tervalidasi |
| `ReopenCount` | **Tetap** — yang dihitung Reopen adalah analis yang membuka sendiri; pengembalian dihitung dari riwayatnya |
| Nilai hasil | Tetap — analis yang memperbaikinya |
| `Version` | Naik satu |

| Riwayat | Keadaan |
| --- | --- |
| Baris `ValidateResult` dr. A pukul 02.12 | **Tetap ada dan tidak berubah sedikit pun** — pelaku, waktu, status, kode, catatan |
| Baris baru `ReturnResultToAnalyst` | `Validated` → `Draft`, pelaku perilis B, kode `SAMPEL-TERTUKAR`, catatan bila ada |

Tidak ada pemberitahuan yang dikirim dan tidak ada versi bernomor yang dibuat. Sesudahnya analis
memperbaiki hasil lalu menyatakan Final lagi. Sebelum itu, validasi ulang ditolak *"belum
dinyatakan selesai"* (`VAL-124`).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / dokumen | Kenapa dibaca |
| --- | --- |
| `roadmap/backend-roadmap.md` 6ak.6 | Cakupan, AC, jebakan menghapus riwayat |
| `contracts/api-contract.md` 29.2-29.3 | Endpoint dan `LabResultReturnRequest` |
| `contracts/validation-matrix.md` 14.1; `contracts/state-transition-matrix.md` 7.2 | Teks dan urutan `VAL-134`/`135`/`138`; isi transisi |
| `testing/acceptance-test-matrix.md` | `AC-205`..`AC-207` |
| `Models/LabResultCorrectionReason.cs` | Alasan koreksi `BE-LAB-70`/`71` |
| `Services/LabResultValidationService.cs` | Pembantu `BE-LAB-73`/`74` yang dipakai ulang |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Services/LabResultValidationService.cs` | **`ReturnToAnalystAsync`** baru. `AddHistory` digeneralisasi agar menerima kode dan catatan alasan apa pun, bukan hanya alasan empat mata. Kedua pemanggil lama disesuaikan tanpa mengubah perilaku, dibuktikan regresi 38/38 dan 28/28 |
| `Controllers/LabExaminationController.cs` | Endpoint `POST /{id}/result/return` |
| `DTOs/LabExaminationResultDtos.cs` | `LabResultReturnRequest` — `correctionReasonId`, `note` |

**Nol entity, nol migration, nol seeder.** Registrasi DI tidak berubah — service yang sama.

**Keputusan kecil.**

| Hal | Keputusan dan alasan |
| --- | --- |
| Alasan yang tidak ada, nonaktif, atau tidak dikirim | Satu pesan: *"Alasan pengembalian wajib dipilih dari daftar."* — kontrak `VAL-135` hanya menyediakan pesan itu |
| Catatan lebih dari 500 karakter | *"Catatan paling panjang 500 karakter."* — teks yang sama dengan `VAL-132` |
| Catatan diberikan padahal alasan tidak mewajibkannya | Disimpan — kontrak menyebutnya *catatan bebas pada riwayat* |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif, sesuai `r34` 29.2-29.3.** Satu endpoint baru; respons memakai `LabExaminationCompletionResponse` yang sama |
| Database | **Nol migration.** Mengosongkan sembilan kolom pada `LabExamination` dan **menambah** satu baris `LabTransitionHistory`. **Nol baris riwayat diubah atau dihapus** — service hanya punya satu titik tulis riwayat, `Add`. Nol data ditulis ke database bersama oleh task ini |
| Keamanan/Auth | **Aksi baru `LabExaminationResult : Return`** — registri 1574 → **1575**; terdaftar, **nol pemegang**. Lapis orang `VAL-138` fail-closed. Log tanpa catatan, nilai hasil, maupun nama pasien |

---

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Examination

Base URL: `api/v1/health-services/laboratory-management/lab-examinations`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/{id}/result/return` | **Baru.** Mengembalikan hasil tervalidasi yang **belum** dirilis kepada analis; hasil kembali Draft. Badan: `correctionReasonId` (wajib), `note` | `LabExaminationResult : Return` |

**Kode status:** `200`; `403` tanpa aksi `Return`, atau bukan pemegang kode validasi maupun rilis;
`404`; `409` sudah dirilis atau bentrok; `422` `VAL-126`, `VAL-127`, `VAL-134`, `VAL-135`; `503`
data kewenangan tidak terbaca.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` | **0 error, 230 warning**; **nol** dari berkas yang disentuh; 54 detik | `PASS` | Keluaran build |
| Startup Development | Registri **1575** — naik tepat satu | `PASS` | Log startup |
| Registri di database | `Update`, `Validate`, `Release`, `Return` pada `LabExaminationResult` — **keempatnya nol pemegang** | `PASS` | Kueri baca-saja |
| Swagger | `POST /{id}/result/return` dengan `200,403,404,409,422,503` | `PASS` | Dokumen Swagger |
| Harness EF InMemory | **20 `PASS`, 0 `FAIL`** | `PASS` | Rincian di bawah |
| Regresi `BE-LAB-73` / `BE-LAB-74` sesudah `AddHistory` digeneralisasi | **38/38** dan **28/28** | `PASS` | Harness yang sama dijalankan ulang |
| Panggilan HTTP — lima penolakan | Kelimanya sesuai | `PASS` | Rincian di bawah |
| `LabExamination` sebelum dan sesudah HTTP | 10 baris identik | `PASS` | Kueri baca-saja |
| Tinjauan kode — riwayat hanya ditambah | Nol `Update`/`Remove`/`ExecuteUpdate`/`ExecuteDelete` atas riwayat; satu titik `Add` | `PASS` | Pencarian pola |
| Pengembalian yang berhasil **lewat HTTP** | Tidak dijalankan | `NOT RUN` | Lihat *Tidak dijalankan* |

**Rincian HTTP.**

| Skenario | Akun | Hasil |
| --- | --- | --- |
| `VAL-134` — Hemoglobin Patologi Klinik belum divalidasi | superadmin | `422` *"Hasil ini belum divalidasi; analis dapat membukanya kembali sendiri."* |
| `VAL-127` — pemeriksaan gugur; `VAL-126` — Mikrobiologi | superadmin | `422` keduanya |
| Pemeriksaan tidak dikenal | superadmin | `404` |
| Lapis jabatan — Kepala Instalasi tanpa aksi `Return` | dr. Bima | **`403`** dari filter |

**Rincian harness.**

| Skenario | Hasil sebenarnya |
| --- | --- |
| `404`; `VAL-126` Mikrobiologi; `VAL-127` gugur | Sesuai |
| `VAL-134` belum divalidasi | `422` dengan teks kontrak |
| **`AC-207`** — hasil yang sudah dirilis | `409` *"Hasil yang sudah dirilis hanya dapat diubah lewat koreksi."*; tetap dirilis |
| **`VAL-138`** — analis tanpa kode; akun tanpa data tenaga kerja | `403` dengan teks kontrak |
| Pembaca kewenangan gagal | `503`; nol berubah |
| **`AC-205`** — tanpa alasan | `422` *"Alasan pengembalian wajib dipilih dari daftar."* |
| `VAL-135` — alasan nonaktif, asing, `requiresNote` tanpa catatan, catatan 501 karakter | `422` semuanya; nol tertulis |
| **`AC-205`** — dengan alasan *Sampel tertukar* | `200`; `FinalizedAt` dan **ketujuh** kolom validasi kosong; `ReopenCount` **tetap**; `Version` +1; nilai hasil tetap; `resultStatus = Draft` |
| **`AC-206`** — baris `ValidateResult` lama | **Masih ada; aksi, pelaku, waktu, status, kode, dan catatannya identik** dengan sebelum pengembalian |
| **`AC-206`** — baris baru | Tepat satu `ReturnResultToAnalyst`, `Validated` → `Draft`, kode `SAMPEL-TERTUKAR`, pelaku, `EncounterId` terisi |
| Validasi ulang sebelum analis Final lagi | `422` `VAL-124` |
| **`VAL-138` "atau"** — pemegang kode **rilis** saja | `200` |
| Catatan diberikan | Tersimpan pada riwayat |
| **Balapan** *Kembalikan* lebih dulu lawan Rilis | *Kembalikan* `200`, Rilis `409`; hasil Draft dan **tidak** dirilis |

**Batas harness, disebut apa adanya.** Pada balapan di atas, InMemory menyisakan **satu** baris
dokumen rekam medis milik rilis yang **kalah**. InMemory tidak bertransaksi, sehingga `INSERT`
dokumen tertulis sebelum `UPDATE` pemeriksaannya ditolak token `Version`. Di PostgreSQL keduanya
berada dalam satu transaksi `SaveChangesAsync`, dan seluruh penyimpanan rilis yang kalah dibatalkan.
Keadaan baris pemeriksaan tetap benar pada harness. Kelas batas yang sama dicatat `BE-LAB-73` dan
`BE-LAB-74`.

Uji manual: **`NOT FEASIBLE`** — layar `FE-LAB-39` belum dibangun.

**Tidak dijalankan:**

- **Pengembalian yang berhasil lewat HTTP.** Syaratnya — hasil tervalidasi, aksi `Return`, dan
  penunjukan kode validasi atau rilis — seluruhnya menulis ke database bersama. Dev nol punya hasil
  tervalidasi.
- Analyzer — build memakai `-p:RunAnalyzers=False`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-205` — tanpa alasan `422`; dengan alasan `200`, Draft, `FinalizedAt` dan kolom validasi kosong, `ReopenCount` tidak naik | ✅ **Terpenuhi** pada harness | — |
| `AC-206` — `ValidateResult` masih ada dan tidak berubah; `ReturnResultToAnalyst` berkode alasan; nol pemberitahuan dan versi bernomor | ✅ **Terpenuhi** pada harness dan tinjauan kode | Perbandingan per ruas; service tidak punya jalur pemberitahuan maupun versi |
| `AC-207` — hasil dirilis `409` | ✅ **Terpenuhi** pada harness | — |
| `VAL-138` | ✅ **Terpenuhi** pada harness | Tolak tanpa keduanya; terima dengan kode rilis saja |
| DoD — pengembalian berjalan; riwayat utuh | ✅ **Terpenuhi** pada harness | — |
| DoD — laporan `BE-LAB-75.md` | ✅ **Terpenuhi** | Berkas ini |
| Verifikasi lewat aplikasi yang berjalan — pengembalian berhasil | ✅ **Terpenuhi** 2026-10-06 | Bagian 8. *Semula belum terpenuhi* |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 230 warning compiler, **nol** dari berkas yang disentuh |
| Masalah yang diketahui | `NONE` baru |
| Risiko tersisa | **Rendah pada kodenya, sedang pada verifikasinya.** Pengembalian yang berhasil belum berjalan terhadap PostgreSQL. Sampai `Return` diberikan kepada jabatan pemvalidasi dan perilis, tidak seorang pun dapat mengembalikan — disengaja |
| Perubahan sampingan | `NONE` di repository. Startup menyinkronkan registri, sehingga baris `SysActionAccess` `Return` kini ada di database dev |
| Interupsi | `NONE` |
| Status Git | Berkas `BE-LAB-75`: laporan ini (`??`); ` M` `Controllers/LabExaminationController.cs`, `DTOs/LabExaminationResultDtos.cs`; `Services/LabResultValidationService.cs` (`??`, lahir `BE-LAB-73`); `roadmap/backend-roadmap.md`, `roadmap/traceability.md`. Perubahan `BE-LAB-67`..`74` yang belum ter-commit ikut ada. **Nol operasi Git dijalankan** |
| Langkah berikutnya | **1.** `BE-LAB-76` (ruas baca: pengesah, keadaan hasil, label order) kini `SIAP DIKERJAKAN` — `DeriveResultStatus` dan penyusun penanda sudah tersedia. **2.** Ketiga tindakan `S4` kini berdiri pada kode. Pembuktian HTTP ketiganya sekaligus butuh satu instruksi data: jabatan dengan `Validate`/`Release`/`Return`, penunjukan uji di Human Resource, dan satu hasil Patologi Klinik uji yang Final |

## 8. Verifikasi lanjutan 2026-10-06 — HTTP asli

**Status: `BE-LAB-75` ✅ `SELESAI`.** Pengembalian yang berhasil kini teramati.

Atas persetujuan pemilik modul, langkah rilis `MVP-9d` dijalankan sebagai setup uji di devYoga
([`backend-roadmap.md`](../../../roadmap/backend-roadmap.md) 6ak.10): kode `LAB-*` di katalog Human Resource, kredensial
dr. Bima (`LAB-VAL-PK`/`LAB-REL-PK`), `Validate`/`Release`/`Return` beserta izin baca bagi jabatan dokter. Panggilan berjalan
terhadap backend lokal dan PostgreSQL devYoga dengan akun asli: dr. Bima (Kepala Instalasi) dan Vina (analis). Tulis hanya pada
Hemoglobin dan Leukosit `LAB-RSMMC-000001` (pesanan uji). Rincian layar ada di
[`FE-LAB-39.md`](../frontend/FE-LAB-39.md) bagian 9.

| Butir | Bukti HTTP asli | Hasil |
| --- | --- | --- |
| Pengembalian berhasil (`AC-205`) | dr. Bima mengembalikan Leukosit yang baru ia validasi, dengan alasan *Sampel tertukar* → `200` *"Hasil dikembalikan kepada analis dan kembali menjadi Draft."*; hasil *Draft* dan dapat diisi lagi oleh Vina. Tanpa alasan, layar menahan dan tidak mengirim permintaan | `PASS` |
| Riwayat utuh (`AC-206`) | `LabTransitionHistory` (kueri baca-saja): `ValidateResult` Leukosit (15.58 WIB) **tetap ada**, disusul `ReturnResultToAnalyst` berkode `SAMPEL-TERTUKAR`; pola yang sama pada BTA | `PASS` |
| `VAL-138` — pengembali pemegang kode | dr. Bima (`LAB-VAL-PK`) dan dr. Nabila (`LAB-VAL-MB`, [`BE-LAB-78.md`](BE-LAB-78.md) bagian 8) | `PASS` |
| Siklus sesudahnya | Analis menyatakan selesai lagi `200`, validasi berikutnya `200` — Leukosit (dr. Bima) dan BTA (dr. Nabila, lalu dirilis) | `PASS` |

**Tetap pada harness:** `AC-207` (hasil dirilis `409`) dan pengembali pemegang kode rilis saja (`VAL-138`). Dev tidak punya
akun pemegang kode rilis saja. Analyzer tidak dijalankan.

**Risiko tersisa: rendah.**

**Jejak di devYoga:** Hemoglobin **Dirilis**; Leukosit **Tervalidasi** oleh dr. Bima (menunggu rilis) sesudah satu
pengembalian `SAMPEL-TERTUKAR`. Data uji. **Nol perubahan kode. Nol operasi Git dijalankan.**
