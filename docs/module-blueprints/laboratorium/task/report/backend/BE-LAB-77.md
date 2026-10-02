# Laporan Perubahan Backend — `BE-LAB-77`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-77` |
| Judul | Antrean validasi dan Daftar Kerja |
| Slice | Gelombang `MVP-9b` — `EPIC-LAB-15`, jalur baca `S4` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian **6ak.8** |
| Trace | `FR-15.9`, `FR-15.11`; `LAB-DEC-135` butir 2; `LAB-FE-006` (urutan antrean) |
| Contract version | `LAB-API-v1` **`r34`** 29.4 dan 29.8; `LAB-VAL-v1` `r12` `VAL-139` — **`approved` 2026-09-25** |
| Dependency | `BE-LAB-76` ✅ (turunan `resultStatus`), `BE-LAB-70` ✅ (`ReleasedAt`, enum `LabValidationQueueStage`) |
| Klasifikasi | `MEDIUM` — skor 8: repository 0, berkas diperiksa 1 (±12), berkas diubah 2 (4), logika bisnis 1, kontrak API **2** (endpoint baru; perilaku dua endpoint lama berubah), database 1 (kueri baca), keamanan/auth 0 (hak yang sudah ada), UI/workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/LaboratoryManagement/`, dokumen blueprint Laboratorium |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `017d1819` (branch `yoga`), di atas `BE-LAB-67`..`76` yang belum ter-commit |
| Tanggal | 2026-09-29 (implementasi dan verifikasi); laporan ditutup 2026-09-30 |
| Status | ✅ **`SELESAI`** — endpoint berjalan **lewat HTTP terhadap PostgreSQL** (`VAL-139` `422`, kedua tahap `200`, `401` tanpa token); build 0 error tanpa warning baru; harness **21/21**, termasuk `AC-17` dan jumlah kueri tetap per halaman; regresi `BE-LAB-69` 31/31, `BE-LAB-72` 43/43, `BE-LAB-73` 38/38, `BE-LAB-74` 28/28, `BE-LAB-75` 20/20, `BE-LAB-76` 16/16. Registri tetap **1575**. Isi antrean yang tidak kosong dan `AC-17` belum teramati lewat HTTP — dev nol punya hasil Patologi Klinik Final maupun hasil yang dirilis |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `LaboratoryManagement` |
| Pemilik / prefix registry | `Lab` — `ACTIVE` |
| Keberlakuan | `TOUCHED LEGACY` — `LabWorklistService`, `LabWorklistController`, `LabWorklistDtos`, `LabExaminationService` (dua pembantu internal). Nol berkas baru |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-DTO-001` (query dan respons tersendiri; entity tidak diekspos), `QBE-PERM-001` (aksi `Read` pada controller yang sudah ada, `[AccessPermission]` terpasang), `QBE-ENT-003` (tahap antrean **turunan**, nol kolom). **Tidak berlaku:** `QBE-LOG-001` (nol perubahan state), `QBE-ENT-001`/`CFG-001` (nol entity), `QBE-DB-*` (nol migration) |
| Governance yang dibaca | Sama dengan `BE-LAB-73`..`76` |

---

## 1. Masalah yang diperbaiki

**Pemvalidasi dan perilis tidak punya tempat untuk menemukan pekerjaannya.** Tindakan validasi dan
rilis sudah berdiri (`BE-LAB-73`..`75`), tetapi satu-satunya jalan menemukan hasil yang menunggu
adalah membuka order satu per satu.

**Daftar Kerja dan daftar pantau cito tidak tahu tentang rilis.** Keduanya mengeluarkan pemeriksaan
hanya bila ordernya ditandai selesai. Kalium cito yang dirilis tepat waktu tetap tercatat
*terlambat* sampai seseorang menutup ordernya (`AC-17`).

---

## 2. Proses bisnis

**Antrean dua tahap** — `GET /lab-worklists/validation-queue?stage=…`:

| Tahap | Isi | Sejak kapan menunggu |
| --- | --- | --- |
| `AwaitingValidation` — *Menunggu Validasi* | Hasil **Final** yang belum divalidasi | `finalizedAt` |
| `AwaitingRelease` — *Menunggu Rilis* | Hasil **tervalidasi** yang belum dirilis; nama pemvalidasi ditampilkan | `validatedAt` |

**Urutan:** cito lebih dulu, lalu yang **paling lama menunggu** (`LAB-FE-006`). Contoh pada harness:
*Kalium cito (30 menit) → Hemoglobin (120 menit) → Natrium (90 menit) → …* — Kalium cito tetap di atas
walau menunggu paling singkat.

**Tahap wajib dipilih.** Tanpa tahap, atau dengan tahap yang tidak dikenal, antrean menjawab
`422` *"Tahap antrean wajib dipilih: menunggu validasi atau menunggu rilis."* (`VAL-139`). Layar
tidak dapat menampilkan kedua tahap bercampur.

**Yang tidak masuk antrean** — semuanya karena tidak ada yang dapat ditindak:

| Hasil | Kenapa |
| --- | --- |
| Draft (`AC-196`) | Belum Final — validasi akan menolak `VAL-124` |
| Mikrobiologi dan Patologi Anatomi | Validasi disiplin itu belum dibangun (`VAL-126`); `r34` menetapkan antrean selalu Patologi Klinik |
| Pemeriksaan gugur, batal, atau berorder batal | Tindakan akan menolak `VAL-127` |
| Sudah dirilis | Pekerjaannya selesai |

**Setiap baris** memuat pasien, nomor rekam medis, nomor order, nama pemeriksaan, urgensi,
`resultStatus`, dan `referenceFlag`. Keduanya dihitung dengan **rumus yang sama** dengan halaman
hasil, sehingga baris antrean dan halaman yang dibukanya tidak dapat berbeda pendapat.

**Daftar Kerja dan daftar pantau cito** (`GET /pending`, `GET /cito-overdue`) kini mengeluarkan
pemeriksaan yang sudah **dirilis** (`r34` 29.8). Kalium cito yang terlambat lalu dirilis hilang dari
keduanya pada saat itu juga, tanpa menunggu ordernya ditutup.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / dokumen | Kenapa dibaca |
| --- | --- |
| `roadmap/backend-roadmap.md` 6ak.8 | Cakupan, jebakan, DoD |
| `contracts/api-contract.md` 29.4, 29.8 | Query, respons, hak akses, dan perubahan perilaku kedua daftar |
| `contracts/validation-matrix.md` `VAL-139` | Teks dan kode galat |
| `testing/acceptance-test-matrix.md` | `AC-17`, `AC-196` bagian antrean, baris *Antrean* |
| `04-prd-to-mvp.md` `FR-15.9`, `FR-15.11` | Requirement |
| `Services/LabWorklistService.cs`, `Controllers/LabWorklistController.cs` | Pola `/pending` dan `/cito-overdue`, penyaring bersama |
| `Services/LabExaminationService.cs` — `DeriveResultStatus`, `ProjectResultFormRows`, `ResolveReferenceFlagsAsync` | Rumus yang wajib dipakai ulang |
| `Services/LabResultValidationService.cs` — `EnsureClinicalPathologyAndRunning` | Cara tindakan membaca disiplin dan menolak pemeriksaan batal |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Services/LabExaminationService.cs` | **`HasResultStatus(status)`** — ekspresi kueri untuk satu `resultStatus`, cermin persis `DeriveResultStatus` (dirilis → tervalidasi → Final → Draft → belum diisi). **`ResolveReferenceFlagsByIdAsync(ids)`** — penanda rujukan untuk sekumpulan pemeriksaan lewat proyeksi dan penghitung yang sama dengan halaman hasil. Keduanya `internal` |
| `Services/LabWorklistService.cs` | `GetValidationQueueAsync`. `BelumSelesai()` menambah `ReleasedAt == null`. Pencarian dipisah menjadi `TerapkanPencarian` supaya antrean memakai aturan cari yang sama **tanpa** penyaring disiplin. Konstruktor kini menerima `LabExaminationService` (sudah terdaftar di DI) |
| `Controllers/LabWorklistController.cs` | `GET validation-queue` — `LabWorklist : Read`; `LabExaminationValidationException` → `422` |
| `DTOs/LabWorklistDtos.cs` | `LabValidationQueueQuery` (mewarisi `LabWorklistPagedQuery`, `Stage`) dan `LabValidationQueueItemResponse` — ruas persis `r34` 29.4 |

**Nol entity, nol migration, nol kolom tersimpan, nol aksi hak akses baru.** Registri tetap 1575.
Enum `LabValidationQueueStage` sudah dibuat `BE-LAB-70`.

**Keputusan kecil.**

| Hal | Keputusan dan alasan |
| --- | --- |
| Order yang ditandai **selesai secara manual** | **Tetap masuk antrean.** Hasil Final di dalamnya masih menunggu disahkan; mengeluarkannya membuat hasil itu tidak dapat ditemukan siapa pun. Daftar Kerja tetap mengeluarkannya — artinya lain: pekerjaan analis sudah selesai. Keadaan ini akan dicegah `BE-LAB-81` |
| Order lama tanpa disiplin | Masuk bila katalog pemeriksaannya Patologi Klinik — **cara yang sama dengan tindakan validasi** (`VAL-126`). Antrean yang lebih sempit dari tindakannya menyembunyikan hasil yang sah. Di dev tidak ada order seperti itu |
| Ruas `discipline` pada query | **Diabaikan**, sesuai `r34` 29.4. Swagger tetap menampilkannya karena diwarisi dari penyaring daftar kerja |
| `stage` berupa angka (`1`, `2`) | **Ditolak** `VAL-139`. Kontrak hanya menyebut nama tahap; pengurai enum bawaan menerima angka apa pun, termasuk `99` |
| `stage` huruf kecil (`awaitingrelease`) | Diterima — pengurai tidak peka huruf besar-kecil, sama dengan penyaring disiplin |
| Hak akses | `LabWorklist : Read`, sesuai kontrak. Tidak ada aksi baru — pemvalidasi dan perilis wajib diberi hak ini (langkah rilis 6ak.10 butir 4) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Sesuai `r34` 29.4 dan 29.8.** Satu endpoint baru. `GET /pending` dan `GET /cito-overdue`: **perilaku berubah** — pemeriksaan dirilis keluar; bentuk respons tetap |
| Database | **Baca saja.** Antrean: hitung, halaman, lalu pasien, nama pemvalidasi, dan penanda — **6 kueri per halaman berapa pun barisnya**, 2 kueri bila halaman kosong. Kolom yang disaring (`FinalizedAt`, `ValidatedAt`, `ReleasedAt`) sudah ada; nol index baru |
| Keamanan/Auth | Hak yang sudah ada. Antrean memuat nama pasien dan nomor rekam medis — setara daftar Pemeriksaan, yang dilindungi hak baca modul yang sama |

---

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Worklist

Base URL: `api/v1/health-services/laboratory-management/lab-worklists`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/validation-queue` | **Baru.** Antrean hasil Patologi Klinik menunggu validasi atau rilis | `LabWorklist : Read` | `LabValidationQueueQuery` | `ApiResponse<PagedResult<LabValidationQueueItemResponse>>` |
| `GET` | `/pending` | Tetap — pemeriksaan dirilis kini keluar | `LabWorklist : Read` | `LabWorklistPagedQuery` | Tetap |
| `GET` | `/cito-overdue` | Tetap — keterlambatan berhenti saat dirilis (`AC-17`) | `LabWorklist : Read` | `LabWorklistPagedQuery` | Tetap |

**`LabValidationQueueQuery`:** `stage` (**wajib**: `AwaitingValidation` atau `AwaitingRelease`),
`pageNumber`, `pageSize` (1-100), `onlyCito`, `search` (kode dan nama pemeriksaan, barcode wadah).
`discipline` diabaikan.

**`LabValidationQueueItemResponse`:** `examinationId`, `labOrderId`, `orderNumber`, `encounterId`,
`patientName`, `medicalRecordNumber`, `procedureName`, `urgency`, `resultStatus`, `referenceFlag`,
`finalizedAt`, `validatedAt`, `validatedByName`, `waitingSince`.

| Kode | Kapan |
| --- | --- |
| `200` | Antrean terbaca, termasuk bila kosong |
| `401` | Tanpa token |
| `403` | Jabatan tidak memegang `LabWorklist : Read` — diperiksa **sebelum** `stage` |
| `422` | `VAL-139` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` | **0 error, 230 warning**; **nol** dari berkas yang disentuh; 53 detik | `PASS` | Keluaran build |
| Harness EF InMemory `BE-LAB-77` | **21 `PASS`, 0 `FAIL`** | `PASS` | Rincian di bawah |
| Regresi | `BE-LAB-69` **31/31** (lembar tetap 8 kueri); `BE-LAB-72` 43/43; `BE-LAB-73` 38/38; `BE-LAB-74` 28/28; `BE-LAB-75` 20/20; `BE-LAB-76` 16/16 | `PASS` | Harness yang sama dijalankan ulang |
| Startup Development | Registri tetap **1575**; hanya galat yang sudah dikenal (`42P01 AccAccountingEvent`, FK `HrdAttendanceProcessingRun`) | `PASS` | Log startup |
| Swagger | `validation-queue` terdaftar; parameter `Stage`, `PageNumber`, `PageSize`, `Discipline`, `OnlyCito`, `Search`; respons `200`, `422`; **tanpa** teks deskripsi | `PASS` | `/swagger/health-services/swagger.json` |
| **HTTP terhadap PostgreSQL** | Rincian di bawah | `PASS` | Panggilan sungguhan |
| `/pending` di dev: definisi lama lawan baru | **9 = 9**, sama dengan jawaban HTTP — dev nol punya hasil dirilis | `PASS` | Kueri baca-saja |

**Rincian HTTP** — aplikasi sungguhan, database dev bersama, nol penulisan.

| Panggilan | Akun | Hasil |
| --- | --- | --- |
| `GET /validation-queue` tanpa `stage` | Superadmin | `422` — teks `VAL-139` persis |
| `?stage=1`, `?stage=Selesai` | Superadmin | `422` — teks yang sama |
| `?stage=AwaitingValidation` | Superadmin | `200`, `totalData = 0` |
| `?stage=awaitingrelease&onlyCito=true` | Superadmin | `200`, `totalData = 0` |
| `GET /pending`, `GET /cito-overdue` | Superadmin | `200` — 9 dan 2 baris |
| Ketujuh panggilan di atas | Kepala Instalasi | **`403` pada ketiganya**, termasuk `/pending` dan `/cito-overdue` yang sudah ada sebelumnya — jabatannya tidak memegang `LabWorklist : Read` (hak ini dipegang dua jabatan lain). Membuktikan antrean dijaga hak yang **sama** dengan kedua daftar saudaranya, dan hak diperiksa sebelum `stage` |
| `?stage=AwaitingValidation` tanpa token | — | `401` |

**Antrean kosong di dev itu benar.** Satu-satunya hasil Final yang belum divalidasi di dev adalah
*Pewarnaan BTA Sputum* — **Mikrobiologi**. Ketidakhadirannya adalah bukti di data sungguhan bahwa
hasil Mikrobiologi Final tidak masuk antrean.

**Rincian harness.**

| Skenario | Hasil sebenarnya |
| --- | --- |
| `VAL-139` — `stage` kosong, string kosong, `Selesai`, `1` | Keempatnya `422` dengan teks persis |
| Isi *Menunggu Validasi* | Tepat lima hasil Final Patologi Klinik |
| Urutan | Kalium cito → Hemoglobin (120 menit) → Natrium (90) → Asam urat (20) → SGPT (10) |
| Tidak masuk | Tervalidasi, dirilis, **Draft** (`AC-196`), **Mikrobiologi Final**, gugur, order batal — nol |
| Order yang ditandai selesai manual | Masuk |
| Order tanpa disiplin, pemeriksaan Patologi Klinik | Masuk |
| Ruas baris | `waitingSince = finalizedAt`; pasien, rekam medis, nomor order, urgensi terisi |
| Isi *Menunggu Rilis* | Satu hasil tervalidasi; `validatedByName` terisi; `waitingSince = validatedAt` |
| **Jebakan roadmap** — `resultStatus` dan `referenceFlag` tiap baris lawan `GET /{id}/result` | **6/6 identik** (contoh `Final`/`High` untuk Kalium 6,4) |
| `onlyCito`; `search`; `discipline = Microbiology` | 1 baris; 1 baris; diabaikan — tetap 5 baris Patologi Klinik |
| **Jumlah kueri** — halaman 5 baris lawan 1 baris | **6 = 6**; halaman kosong 2 |
| **`AC-17`** — Kalium cito terlambat, tervalidasi | Ada di `/pending` dan `/cito-overdue` |
| … lalu dirilis | **Hilang dari keduanya** dan dari antrean rilis |
| Regresi — pemeriksaan belum dirilis | Tetap di `/pending` |

Uji manual: **`NOT FEASIBLE`** — layar `FE-LAB-40` belum dibangun.

**Tidak dijalankan:**

- Antrean **berisi** lewat HTTP, dan `AC-17` lewat HTTP — dev nol punya hasil Patologi Klinik Final
  maupun hasil dirilis. Menyiapkannya berarti menulis ke database bersama. Tiga kueri pelengkap
  (pasien, nama pemvalidasi, penanda) karena itu belum diterjemahkan Npgsql; penanda memakai
  proyeksi `ProjectResultFormRows` yang sudah berjalan di PostgreSQL lewat `GET /{id}/result`
  (`BE-LAB-76`), dua lainnya kueri `Contains` biasa.
- Akun pemegang `LabWorklist : Read` selain superadmin — tidak ada kredensialnya di sesi ini.
- Analyzer — build memakai `-p:RunAnalyzers=False`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-196` bagian antrean — Kalium Draft tidak ada di `AwaitingValidation` | ✅ **Terpenuhi** pada harness | Validasi Draft `422` `VAL-124` sudah dibuktikan `BE-LAB-73` |
| `AC-17` — cito yang dirilis tidak muncul pada daftar pantau keterlambatan | ✅ **Terpenuhi** pada harness | Belum teramati lewat HTTP |
| `VAL-139` — `stage` kosong atau tidak sah → `422` | ✅ **Terpenuhi** — harness dan HTTP | Teks persis |
| Baris *Antrean* matriks uji — isi tiap tahap sesuai 29.4; cito di atas; tanpa `stage` → `422` | ✅ **Terpenuhi** pada harness; `422` lewat HTTP | — |
| Verifikasi roadmap — Kalium cito terlambat lalu dirilis → hilang dari `/cito-overdue` dan `/pending` | ✅ **Terpenuhi** pada harness | — |
| Verifikasi roadmap — hasil Mikrobiologi Final **tidak** muncul di antrean | ✅ **Terpenuhi** — harness **dan** data dev (Pewarnaan BTA Sputum) | — |
| Risiko roadmap — keadaan antrean dengan rumus berbeda dari `resultStatus` | ✅ **Tertutup** — `HasResultStatus` cermin `DeriveResultStatus`; 6/6 baris identik dengan halaman hasil | — |
| DoD — antrean berjalan; Daftar Kerja menyesuaikan; laporan | ✅ **Terpenuhi** | — |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 230 warning compiler, **nol** dari berkas yang disentuh |
| Masalah yang diketahui | **1.** Akun Kepala Instalasi tidak memegang `LabWorklist : Read`, sehingga ia tidak dapat membuka Daftar Kerja, daftar pantau cito, maupun antrean. Ini keadaan kebijakan dev sejak sebelum task ini, bukan regresi. Pemberiannya bagian langkah rilis 6ak.10 butir 4 (`UNK-P14-03`). **2.** Order Patologi Klinik lama tanpa disiplin masuk antrean, tetapi `resultProgress`-nya (`BE-LAB-76`) kosong — tidak ada di dev. **3.** Satu baris `BE-LAB-80` akan membalik bukti *"Mikrobiologi Final tidak muncul"*; roadmap sudah mencatatnya sebagai perubahan yang disetujui |
| Risiko tersisa | **Rendah.** Seluruhnya jalur baca. Perubahan perilaku `/pending` dan `/cito-overdue` hanya mengeluarkan pemeriksaan yang sudah selesai |
| Perubahan sampingan | `NONE` |
| Interupsi | Satu pemadatan konteks di tengah verifikasi. Dilanjutkan dari keadaan terverifikasi: build yang sudah lolos, harness yang sudah ditulis; nol suntingan ganda |
| Status Git | Berkas `BE-LAB-77`: ` M` `Services/LabExaminationService.cs`, `Services/LabWorklistService.cs`, `Controllers/LabWorklistController.cs`, `DTOs/LabWorklistDtos.cs`, `roadmap/backend-roadmap.md`, `roadmap/traceability.md`; `??` laporan ini. Perubahan `BE-LAB-67`..`76` yang belum ter-commit ikut ada. **Nol operasi Git dijalankan** |
| Langkah berikutnya | **1.** `BE-LAB-81` (penjaga penyelesaian order) — memakai `LabOrderResultProgressRules.Counted`; sesudahnya keputusan kecil *order selesai manual tetap masuk antrean* tidak lagi punya kasus nyata. **2.** Gelombang `MVP-9b` (`BE-LAB-73`..`77`) kini seluruhnya berdiri pada kode — `73`..`75` tetap ⚠ karena jalur berhasilnya belum teramati lewat HTTP. Dengan aturan yang sama dengan naiknya `BE-LAB-74` sesudah `BE-LAB-73` ⚠, **`BE-LAB-78`** (prasyarat `MVP-10`) dan **`BE-LAB-82`** (prasyarat `MVP-11` dan dependency `BE-LAB-73`/`77`) naik menjadi `SIAP DIKERJAKAN`; `BE-LAB-80` kini hanya menunggu `BE-LAB-78`. **3.** Layar `FE-LAB-40` dapat mulai memakai `GET /validation-queue` |
