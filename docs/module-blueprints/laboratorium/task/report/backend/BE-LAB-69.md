# Laporan Perubahan Backend — `BE-LAB-69`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-69` |
| Judul | Jalur baca per order dan penanda rujukan |
| Slice | Gelombang `MVP-8b` — `EPIC-LAB-14`, perluasan hasil Patologi Klinik |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian **6aj.3** |
| Trace | `FR-14.5` bagi Patologi Klinik, `FR-14.7`, `FR-14.8`; `LAB-DEC-149`, `LAB-FE-015`, `LAB-DEC-135`, `LAB-DEC-141`, `LAB-DEC-048`; `VAL-107`, `VAL-123`; `AC-234`, `AC-219`, `AC-196`, `AC-197`, `AC-214`, `AC-236` |
| Contract version | `LAB-API-v1` **`r33`** bagian 28.2-28.3; `LAB-VAL-v1` **`r11`** `VAL-123` — keduanya **`approved` 2026-09-24**. Rancangan: `02-backend-architecture.md` 19.3-19.4 |
| Dependency | `BE-LAB-68` ⚠ selesai dengan batas verifikasi, 2026-09-29 (belum ter-commit) |
| Klasifikasi | `MEDIUM` — skor 7: repository 0, berkas diperiksa 1 (±18), berkas diubah 1 (5), logika bisnis 2 (penanda dari batas saat hasil disimpan), kontrak API 1 (jalur dan ruas yang sudah dikontrakkan), database 1 (kueri berkelompok, nol schema), keamanan/auth 0, UI/workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/LaboratoryManagement/` dan dokumen blueprint Laboratorium |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `017d1819` (branch `yoga`), di atas `BE-LAB-68` yang belum ter-commit |
| Tanggal | 2026-09-29 |
| Status | ✅ **`SELESAI`** — 2026-09-29 sore (bagian 8). Kode status jalur per order **terbukti lewat HTTP** tanpa menulis data: order Patologi Klinik `200` (5 baris, yang gugur ikut), `VAL-123` `422` pada order Patologi Anatomi dan Mikrobiologi, order asing `404`; sembilan ruas baru terbaca pada `GET /{id}/result`. Perilaku inti tetap dari harness 31/31 — 18 dari 19, `High`/`Low`/`OutOfReference`, 8 kueri tetap. *Semula `SELESAI DENGAN BATAS VERIFIKASI`: HTTP tertahan seeder Hemodialisa* |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `LaboratoryManagement` |
| Pemilik / prefix registry | `Lab` — `ACTIVE` (`MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 27 dan 114) |
| Keberlakuan | `TOUCHED LEGACY` untuk controller, service, DTO, dan komentar model. `NEW CODE` untuk enum `LabReferenceFlag` (prefix `Lab`, tidak disimpan) dan jalur baca per order |
| QBE yang berlaku | `QBE-SVC-001` (kueri di service, controller tanpa `DbContext`), `QBE-API-001`, `QBE-DTO-001` (ruas tambahan pada DTO yang ada), `QBE-ENUM-001` (enum `LabReferenceFlag` dimiliki modul Laboratorium), `QBE-PERM-001` (`[AccessAction]` + `[AccessPermission]` berpasangan), `QBE-VAL-001` (`VAL-123` → `422`) |
| Governance yang dibaca | `AGENTS.md` backend; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `rules/backend/` suite Skill |

---

## 1. Masalah yang diperbaiki

**Halaman hasil Patologi Klinik per order tidak punya jalur baca yang layak.** Satu order Patologi
Klinik lazim berisi belasan pemeriksaan. Sebelum task ini, satu-satunya cara membaca hasilnya
adalah `GET /{id}/result` **per pemeriksaan**. Order berisi 18 pemeriksaan berarti 18 panggilan,
dan terukur **62 kueri** basis data.

**Penanda `L`/`H` tidak ada.** Backend hanya mengirim `isOutOfNormalRange` (ya/tidak) pada saat
simpan. Layar tidak tahu arahnya — rendah atau tinggi — dan cenderung menurunkannya sendiri dari
`normalLow`/`normalHigh`. `FE-LAB-36` menyebut itu sebagai salah satu jebakannya.

**Keadaan Final dan konsultasi tidak terbaca.** Respons bentuk hasil tidak membawa `FinalizedAt`,
`ReopenCount`, maupun fakta konsultasi. Akibatnya halaman tidak dapat menampilkan tombol yang
benar per baris.

**Satu jebakan ditemukan saat mengerjakan, dan ini temuan terpenting laporan ini.** Kontrak
menetapkan penanda dihitung dari *"batas nilai yang tersimpan bersama hasil
(`ResultValueBoundId`)"*. Namun batas nilai **disunting di baris yang sama**
(`LabValueBoundService.cs:470-471`), sehingga `ResultValueBoundId` saja **tidak membekukan
rentangnya**.

> Kalium 6,4 disimpan hari Senin dengan rujukan 3,5–5,1 → **High**. Hari Rabu kepala instalasi
> mengubah batas atas menjadi 7,0 pada baris yang sama. Bila penanda dibaca dari baris itu apa
> adanya, hasil Senin berubah menjadi **Normal** tanpa satu pun orang menyentuh hasilnya.

Bagian 3.2 menjelaskan cara task ini menutupnya tanpa migration.

---

## 2. Proses bisnis

**Tujuan.** Analis membuka satu halaman untuk satu order Patologi Klinik. Di halaman itu ia
melihat seluruh pemeriksaan beserta hasil, penanda `L`/`H`, keadaan Final, dan konsultasinya.

**Pelaku.** Pemegang `LabExamination : Read` untuk membaca. Menulis tetap memerlukan
`LabExaminationResult : Update` (`BE-LAB-67`).

**Alur, berurutan.**

1. Halaman memanggil `GET /lab-examinations/by-order/{labOrderId}/results` **sekali**.
2. Backend memastikan order ada (`404` bila tidak) dan memang Patologi Klinik (`422` `VAL-123`
   bila bukan).
3. Backend mengembalikan **satu baris per pemeriksaan yang tidak batal**. Setiap baris berbentuk
   persis sama dengan `GET /{id}/result`.
4. Setiap baris membawa dua jenis batas, dan keduanya berbeda dengan sengaja:
   - **bentuk isian** — rentang, satuan, dan pilihan — memakai batas yang berlaku **hari ini**
     bagi pasien itu, sebab itulah yang dipakai bila hasil disimpan sekarang;
   - **`referenceFlag`** memakai batas yang berlaku **saat hasil disimpan**.
5. Analis mengisi dan menyimpan per baris lewat `PUT /{id}/result`. Responsnya kini ikut membawa
   `referenceFlag`.

**Aturan penanda.**

| Hasil | Batas saat disimpan | `referenceFlag` | Tampil (`FE-LAB-36`) |
| --- | --- | --- | --- |
| Kalium 6,4 mmol/L | 3,5 – 5,1 | `High` | `H 6,4` |
| Hemoglobin 9,4 g/dL | 13,0 – 17,0 | `Low` | `L 9,4` |
| Natrium 140 mmol/L | 135 – 145 | `Normal` | `140` |
| Protein urin `+2` | pilihan `+2` ditandai di luar rujukan | `OutOfReference` | teks — `FR-14.9` belum diputuskan |
| Protein urin `Negatif` | pilihan tidak ditandai | `Normal` | `Negatif` |
| Belum diisi | — | kosong | — |
| Angka, tetapi batasnya tidak punya rentang normal | — | kosong | — |

**Nilai kritis tidak dihitung dan tidak dikirim** — itu `S5`. Enum `LabReferenceFlag` sengaja
tidak punya nilai `Critical`, dan respons tidak memuat `CriticalLow`, `CriticalHigh`, maupun
`IsCritical` pilihan.

**Jalur tidak normal.**

| Keadaan | Jawaban |
| --- | --- |
| Order Mikrobiologi atau Patologi Anatomi | `422` — *"Order ini bukan Patologi Klinik. Buka halaman hasil sesuai disiplinnya."* (`VAL-123`) |
| Order lama yang disiplinnya kosong | Disiplin dibaca dari katalog tiap pemeriksaannya (`LAB-DEC-048`). Bila ada yang bukan Patologi Klinik → `422` |
| Order tidak dikenal | `404` — *"Pesanan laboratorium tidak ditemukan."* |
| Pemeriksaan **dibatalkan** | Tidak ikut — `AC-234` |
| Pemeriksaan **gugur** bersama wadahnya | **Ikut**, dengan isian terkunci beserta alasannya. Petugas perlu tahu wadahnya ditolak, bukan melihat baris itu lenyap |
| Pemeriksaan tanpa batas nilai yang berlaku | Ikut, isian terkunci: *"… belum memiliki batas nilai yang berlaku …"* |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / dokumen | Kenapa dibaca |
| --- | --- |
| `roadmap/backend-roadmap.md` 6aj.3 dan `roadmap/frontend-roadmap.md` `FE-LAB-36` | Cakupan, verifikasi, dan jebakan yang harus dihindari kedua sisi |
| `contracts/api-contract.md` 28.2-28.3 | Jalur baru, sembilan ruas, contoh penanda |
| `contracts/validation-matrix.md` baris 537 | Teks `VAL-123` |
| `02-backend-architecture.md` 19.3-19.4, 19.10 | Rancangan class, `ReferenceFlag` sebagai teks tanpa `Critical`, `FR-14.9` masih terbuka |
| `testing/acceptance-test-matrix.md` baris 597-604 | `AC-196`, `AC-197`, `AC-214`, `AC-219`, `AC-234`, `AC-236` |
| `Services/LabValueBoundService.cs` | **Menemukan penyuntingan batas di baris yang sama**, riwayat per kolom (`RecordChange`), dan format nilai lama (`InvariantCulture`) |
| `Services/LabCriticalBoundApprovalService.cs`, seluruh source yang menyebut `NormalLow` | Memastikan **tidak ada** jalur lain yang mengubah rentang normal tanpa riwayat |
| `Repositories/Configurations/.../LabExaminationConfiguration.cs:82-93` | `ResultOptionId` dan `ResultValueBoundId` ber-`Restrict` — pilihan yang dipakai hasil tidak dapat dihapus |
| `Services/LabOrderService.cs`, `Services/LabWorklistService.cs` | Penanganan order yang disiplinnya kosong |
| `Program.cs` | Tidak ada `JsonStringEnumConverter` global, sehingga enum respons Lab dikirim sebagai `string` |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Enums/LaboratoryEnums.cs` | **Enum baru `LabReferenceFlag`** — `Normal = 1`, `Low = 2`, `High = 3`, `OutOfReference = 4`. Tidak disimpan, dan tanpa nilai `Critical` |
| `DTOs/LabExaminationResultDtos.cs` | `LabExaminationResultFormResponse` **+9 ruas**: `Urgency`, `ReferenceFlag`, `IsFinalized`, `FinalizedAt`, `FinalizedByUserId`, `ReopenCount`, `IsConsulted`, `ConsultedToName`, `ConsultedAt`. `LabExaminationResultResponse` **+`ReferenceFlag`**; `IsOutOfNormalRange` **tetap** |
| `Services/LabExaminationService.cs` | **(1)** `GetResultSheetByOrderAsync` baru beserta `VAL-123`. **(2)** `GetResultFormAsync` kini melewati penyusun yang sama, `BuildResultFormsAsync`, sehingga jalur per order bukan salinan kedua. **(3)** `ResolveValueBoundAsync` dipecah menjadi `LoadBoundPatientContextAsync`, `ApplicableBounds`, dan `PickBound`, tanpa perubahan aturan, supaya jalur tunggal dan jalur per order memilih batas dengan cara yang sama. **(4)** `ResolveReferenceFlag` baru, berdampingan dengan `ResolveOutOfNormalRange`. **(5)** `ResolveReferenceFlagsAsync` dan `LoadNormalRangesAtResultAsync` menghitung penanda dari rentang **saat hasil disimpan**. **(6)** `SetResultAsync` mengisi `ReferenceFlag` |
| `Controllers/LabExaminationController.cs` | Endpoint baru `GET by-order/{labOrderId:guid}/results` — `[AccessAction("Read", …)]` berpasangan `[AccessPermission("LabExamination", "Read")]`; `404` dan `422` |
| `Models/LabExamination.cs` | **Komentar saja**, diminta rancangan 19.4: kolom Final dan konsultasi berlaku juga bagi Patologi Klinik sejak `MVP-8`. Nol kolom berubah |

**Cara penanda hasil lama dibekukan tanpa migration.** Setiap penyuntingan batas mencatat satu
baris `LabValueBoundHistory` per kolom yang berubah, lengkap dengan nilai lama dan barunya (`AC-34`).
Rentang yang berlaku bagi sebuah hasil dibaca sebagai berikut:

- bila rentang **tidak berubah** sejak `ResultEnteredAt`, nilai sekarang yang berlaku;
- bila **berubah**, yang berlaku adalah **nilai lama pada perubahan pertama sesudah waktu itu**.

Contoh berangka — Kalium dengan batas atas 5,1:

| Waktu | Kejadian | Batas atas | Hasil A (6,4, disimpan Senin) | Hasil B (6,4, disimpan Rabu) |
| --- | --- | --- | --- | --- |
| Senin | Hasil A disimpan | 5,1 | `High` | — |
| Selasa | Batas diubah 5,1 → 7,0 | 7,0 | tetap `High` — perubahan pertama sesudah Senin punya nilai lama 5,1 | — |
| Rabu | Hasil B disimpan | 7,0 | `High` | `Normal` |
| Kamis | Batas diubah 7,0 → 6,0 | 6,0 | tetap `High` | tetap `Normal` — perubahan pertama sesudah Rabu punya nilai lama 7,0 |

Hanya `LabValueBoundService.UpdateAsync` yang mengubah rentang normal, dan ia selalu mencatat
riwayat. Penyuntingan langsung lewat SQL tidak tercakup. Bila nilai lama di riwayat tidak terbaca
sebagai angka, penanda **dikosongkan**, tidak ditebak.

**Hasil pilihan tidak memerlukan rekonstruksi.** `ResultOptionId` ber-`Restrict`, sehingga
pilihan yang sudah dipakai hasil tidak dapat dihapus. `IsOutOfReference`-nya adalah nilai saat
hasil disimpan.

**Kenapa jumlah kueri tetap.** Jalur per order menjalankan paling banyak delapan kueri, apa pun
jumlah barisnya: order, baris pemeriksaan (**satu kueri**, `AsNoTracking`), konteks pasien, batas
yang berlaku, pilihan bentuk isian, pilihan hasil, batas saat hasil disimpan, dan riwayat batas.
Kueri yang tidak diperlukan dilewati.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif, sesuai `r33` 28.2-28.3.** Satu jalur baru; sembilan ruas pada `GET /{id}/result`; satu ruas pada `PUT /{id}/result`. Nol ruas lama berubah atau hilang. **Satu selisih dari kontrak dicatat:** kontrak menyebut penanda berasal dari `ResultValueBoundId`, sedangkan kode memakai baris itu **ditambah riwayat perubahannya**. Hasil yang dijanjikan kontrak tetap sama — perubahan batas kemudian hari tidak mengubah penanda hasil lama. Kontrak tidak perlu diubah, tetapi kalimatnya pantas diperjelas pada revisi berikutnya |
| Database | **Nol migration, nol perubahan schema.** Tabel `LabValueBoundHistory` yang sudah ada dibaca. `GET /{id}/result` tunggal naik dari 3 menjadi 5 kueri untuk hasil angka: batas saat disimpan dan riwayatnya ikut dibaca |
| Keamanan/Auth | Jalur baru memakai `LabExamination : Read` yang sudah ada — **nol resource atau aksi baru** (registri tetap 1564 kunci). Respons tidak memuat nilai atau batas kritis |

---

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Examination

Base URL: `api/v1/health-services/laboratory-management/lab-examinations`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/by-order/{labOrderId}/results` | **Baru.** Membaca seluruh pemeriksaan Patologi Klinik yang tidak batal pada satu order — bentuk hasil, nilai, penanda, Final, dan konsultasi — dalam satu panggilan | `LabExamination : Read` |
| `GET` | `/{id}/result` | Membaca bentuk dan isi hasil satu pemeriksaan. **Ruas bertambah** — sembilan ruas di bawah | `LabExamination : Read` |
| `PUT` | `/{id}/result` | Mengisi hasil Patologi Klinik. **Respons bertambah `referenceFlag`** | `LabExaminationResult : Update` |

**Ruas tambahan pada `LabExaminationResultFormResponse`:**

| Ruas | Tipe | Boleh kosong | Arti |
| --- | --- | :---: | --- |
| `urgency` | string | Tidak | `Routine` atau `Cito` |
| `referenceFlag` | string | Ya | `Normal`, `Low`, `High`, atau `OutOfReference`, dari batas **saat hasil disimpan** |
| `isFinalized` | bool | Tidak | `finalizedAt` terisi |
| `finalizedAt` | DateTime | Ya | Kapan penulis menyatakan selesai |
| `finalizedByUserId` | Guid | Ya | Siapa yang menyatakan selesai |
| `reopenCount` | int | Tidak | Berapa kali penulisan dibuka kembali |
| `isConsulted` | bool | Tidak | `consultedAt` terisi |
| `consultedToName` | string | Ya | Kepada siapa dikonsultasikan |
| `consultedAt` | DateTime | Ya | Kapan dikonsultasikan |

**Kode status jalur baru:** `200`; `404` order tidak dikenal; `422` `VAL-123`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` | **0 error**, 230 warning, 82 detik. **Nol warning** menyebut berkas yang disentuh | `PASS` | Keluaran build disaring pada nama berkas |
| `PermissionRegistryValidator` | **Lolos** — 1564 kunci, tidak bertambah. Route `by-order/{labOrderId:guid}/results` terdaftar dengan `LabExamination,Read` | `PASS` | Harness validator |
| Harness perilaku `BE-LAB-69` (EF InMemory) | **31 `PASS`, 0 `FAIL`** | `PASS` | Rincian di bawah. Nol baris ditulis ke database dev bersama |
| Regresi — harness `BE-LAB-68` dijalankan ulang di atas kode ini | **19 `PASS`, 0 `FAIL`** | `PASS` | Pemecahan `ResolveValueBoundAsync` tidak mengubah jalur simpan |
| Startup Development | Masih berhenti di `HemodialysisMasterDataSeeder` (`42P01`, `HmdChecklistItem`) | `EXISTING / ENVIRONMENT ISSUE` | Sama dengan `BE-LAB-67`/`68` |

**Rincian harness `BE-LAB-69`.**

| Skenario | Hasil sebenarnya |
| --- | --- |
| `PUT /result` Kalium 6,4 / Hemoglobin 9,4 / Natrium 140 / Protein urin `+2` | `High` / `Low` / `Normal` / `OutOfReference`; `isOutOfNormalRange` tetap terisi `true`/`true`/`false`/`true` |
| `AC-234` order berisi 19 pemeriksaan, satu dibatalkan | `200`, **18 baris**; baris batal tidak ada |
| `AC-219` pada lembar per order | Kalium `High`, Hemoglobin `Low`, Protein urin `OutOfReference`, Natrium `Normal`; 14 baris tanpa hasil → kosong |
| Sembilan ruas — Kalium cito Final tanpa konsultasi | `urgency=Cito`, `isFinalized=true`, `finalizedByUserId` = analis, `reopenCount=0`, `isConsulted=false` |
| `AC-214` — Natrium dikonsultasikan lalu Final | `isConsulted=true`, `consultedToName`, dan `consultedAt` terbaca |
| `AC-236` — Kalium cito Final, Hemoglobin masih diisi | Final `200`; simpan Hemoglobin `200` |
| Nol nilai kritis | Jawaban 9.306 karakter tanpa kata *critical*, padahal pilihan `+2` ditandai kritis di data induk. Enum hanya `Normal`, `Low`, `High`, `OutOfReference` |
| Setiap baris lembar identik dengan `GET /{id}/result` | 18 dari 18 identik sebagai JSON |
| **Batas diubah sesudah hasil disimpan** — 5,1 → 7,0 lewat `LabValueBoundService.UpdateAsync` | Hasil lama **tetap `High`**; rentang tampil (hari ini) 3,5–7,0; hasil baru 6,4 → `Normal` |
| **Perubahan kedua** — 7,0 → 6,0 | Hasil pertama tetap `High`; hasil kedua tetap `Normal` — tiap hasil memakai batas saat ia disimpan |
| Order kedua: satu Kalium, satu gugur, satu batal | 2 baris; yang gugur ikut dengan `canEnterResult=false` dan alasannya |
| `VAL-123` — order Mikrobiologi, order Patologi Anatomi, order lama tanpa disiplin berisi Kultur urin | `422` ketiganya, dengan pesan `VAL-123` |
| Order lama tanpa disiplin berisi Hemoglobin | `200`, 1 baris |
| Order tidak dikenal | `404` |
| `AC-196` — Reopen hasil Patologi Klinik Draft | `422` `VAL-107` |
| `AC-197` — Reopen Kalium Final | `200`; `isFinalized=false`; `reopenCount=1`; riwayat `LabExamination.ReopenResult` mencatat pelaku dan waktu |
| **Jumlah kueri** — diukur dengan menghitung setiap eksekusi di `QueryCompiler` EF | 18 baris = **8 kueri**; 4 baris berkomposisi sama = **8 kueri**; 2 baris tanpa bentuk pilihan = 6 kueri; satu `GET /{id}/result` = 5 kueri; 18 × `GET /{id}/result` = **62 kueri** |

**Batas harness, disebut apa adanya.**

- Jumlah kueri dihitung pada tingkat EF, bukan dari log SQL PostgreSQL. Setiap eksekusi LINQ
  dihitung satu. Di PostgreSQL, proyeksi baris pemeriksaan tetap satu perintah SQL dengan join ke
  order dan katalog; itu belum dilihat langsung.
- InMemory tidak menegakkan relasi `Restrict` maupun indeks unik. Keduanya tidak menjadi bagian
  dari perilaku yang diuji di sini.
- Waktu perubahan batas diurutkan dengan jeda puluhan milidetik antar-langkah, bukan hari yang
  berbeda. Logika perbandingan waktunya sama.

Uji manual: **`NOT FEASIBLE`** saat ini — aplikasi tidak dapat start pada database dev bersama.

**Tidak dijalankan:**

- Analyzer — build memakai `-p:RunAnalyzers=False`.
- Semua panggilan HTTP dan log SQL sungguhan. Begitu aplikasi hidup, jalur baru dapat dibuktikan
  **tanpa menulis data apa pun**: panggil `GET /by-order/{id}/results` pada order Patologi Klinik
  dan order Mikrobiologi yang sudah ada.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-234` bagian backend — 19 pemeriksaan, satu dibatalkan, jawaban **18** baris | ✅ **Terpenuhi** pada harness; HTTP `NOT RUN` | Harness |
| `VAL-123` pada order Mikrobiologi | ✅ **Terpenuhi** pada harness | `422` dengan pesan kontrak |
| `AC-219` bagian backend — `High`, `Low`, `OutOfReference` pada ketiga contoh | ✅ **Terpenuhi** | Harness, pada `PUT /result` maupun lembar per order |
| `AC-196` pada Patologi Klinik lewat route netral | ✅ **Terpenuhi** — bagian yang berlaku sekarang | `422` `VAL-107`. Bagian *"belum masuk antrean validasi"* diuji bersama `S4` |
| `AC-197` pada Patologi Klinik | ✅ **Terpenuhi** — bagian `MVP-8` | `FinalizedAt` kosong, riwayat mencatat pelaku dan waktu. Penolakan sesudah validasi adalah `BE-LAB-73` |
| `AC-214` pada Patologi Klinik | ✅ **Terpenuhi** bagian backend | Final tanpa konsultasi dan dengan konsultasi, keduanya sah; ketiga fakta konsultasi terbaca. *"Nol pilihan Definitif"* adalah bagian antarmuka |
| `AC-236` | ✅ **Terpenuhi** bagian backend | Kalium cito Final; Hemoglobin tetap dapat disimpan |
| Verifikasi — **satu** kueri untuk seluruh baris, bukan satu per baris | ✅ **Terpenuhi** pada tingkat EF | Baris pemeriksaan satu kueri; total 8 untuk 18 maupun 4 baris. Log SQL PostgreSQL `NOT RUN` |
| Verifikasi — mengubah batas normal sesudah hasil disimpan **tidak** mengubah `referenceFlag` hasil lama | ✅ **Terpenuhi** | Dua perubahan berturut-turut lewat service yang sebenarnya |
| Verifikasi — nol nilai `Critical` di jawaban mana pun | ✅ **Terpenuhi** | Harness |
| DoD — jalur baru berjalan; ruas tambahan terisi | ✅ **Terpenuhi** pada harness | — |
| DoD — `IsOutOfNormalRange` **tetap** dikirim | ✅ **Terpenuhi** | `PUT /result` |
| DoD — nol `Critical`; nol migration | ✅ **Terpenuhi** | — |
| DoD — laporan `BE-LAB-69.md` | ✅ **Terpenuhi** | Berkas ini |
| Kode status dibuktikan terhadap aplikasi yang berjalan | **Belum terpenuhi** | Penahan Hemodialisa |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 230 warning compiler, **nol** di berkas yang disentuh |
| Masalah yang diketahui | **1. Rentang yang tampil dan penanda dapat bersumber dari batas berbeda.** Bentuk isian — `normalLow`/`normalHigh` — tetap batas **hari ini**, sesuai perilaku `GET /{id}/result` sejak `r22`. `referenceFlag` memakai batas **saat disimpan**. Sesudah batas diubah, halaman dapat menampilkan `H 6,4` di samping rujukan 3,5–7,0. Keduanya benar menurut kontraknya masing-masing, tetapi bagi pembaca terlihat bertentangan. **Perlu keputusan pemilik modul** sebelum cetakan (`S17`): kirim juga rentang saat disimpan (ruas baru, tanpa migration), atau tampilkan rentang hari ini hanya untuk baris yang belum Final. **2.** `FR-14.9` — teks tampil `OutOfReference` — masih `OPEN DECISION`; backend sudah mengirim nilainya. **3.** Temuan `BE-LAB-68` tentang `500` saat batal, cito, atau duplo bentrok masih terbuka |
| Risiko tersisa | **Rendah.** Jalur baru hanya membaca. Rekonstruksi rentang bergantung pada kelengkapan `LabValueBoundHistory`; penyuntingan batas langsung lewat SQL tidak akan tercermin. Kode status belum dibuktikan lewat HTTP |
| Perubahan sampingan | `NONE` di repository. Harness dibuat di scratchpad sesi. Startup yang gagal kembali menjalankan seeder biasa ke database dev bersama |
| Interupsi | `NONE` selama task ini |
| Status Git | Berkas `BE-LAB-69`: ` M` `Controllers/LabExaminationController.cs`, `DTOs/LabExaminationResultDtos.cs`, `Enums/LaboratoryEnums.cs`, `Models/LabExamination.cs`, `Services/LabExaminationService.cs`. Perubahan `BE-LAB-68` yang belum ter-commit ada di `Controllers/LabExaminationController.cs`, `Services/LabExaminationService.cs`, dan `Services/LabMicrobiologyResultService.cs`. Ditambah laporan ini, `BE-LAB-68.md`, `roadmap/backend-roadmap.md`, dan `roadmap/traceability.md`. **Nol operasi Git dijalankan** |
| Langkah berikutnya | **1.** Backend `MVP-8` seluruhnya berdiri pada kode; `BE-LAB-70` (`MVP-9a`) kini `SIAP DIKERJAKAN`. **2.** `FE-LAB-35` dan `FE-LAB-36` dapat diverifikasi terhadap backend ini begitu aplikasi dapat start. **3.** Putuskan butir *Masalah yang diketahui* nomor 1 sebelum cetakan. **4.** Perjelas kalimat snapshot pada `LAB-API-v1` revisi berikutnya |

---

## 8. Verifikasi runtime susulan — 2026-09-29 sore

Penahan Hemodialisa ditutup atas instruksi eksplisit pemilik modul (bagian 8
[`BE-LAB-70.md`](BE-LAB-70.md)). Sesuai bagian 5, jalur baru dibuktikan **tanpa menulis data apa pun**,
pada order yang sudah ada.

| Skenario | Hasil | Klasifikasi |
| --- | --- | --- |
| Swagger | `GET /lab-examinations/by-order/{labOrderId}/results` terdaftar | `PASS` |
| Order Patologi Klinik `LAB-RSMMC-000001` | `200`, **5 baris**: Hemoglobin `Normal`; Leukosit, Trombosit, Urinalisis Protein tanpa penanda; satu Urinalisis Protein **gugur** ikut dengan `canEnterResult = false`. Nol pemeriksaan batal pada order ini | `PASS` |
| Kata *critical* pada jawaban | **Tidak ada** | `PASS` |
| `VAL-123` — order Patologi Anatomi dan order Mikrobiologi | Keduanya `422` *"Order ini bukan Patologi Klinik. Buka halaman hasil sesuai disiplinnya."* | `PASS` |
| Order tidak dikenal | `404` *"Pesanan laboratorium tidak ditemukan."* | `PASS` |
| `GET /{id}/result` — sembilan ruas baru | Terbaca semua: `urgency=Cito`, `referenceFlag=Normal`, `isFinalized=false`, `reopenCount=1`, `isConsulted=true`, `consultedToName`, `consultedAt` | `PASS` |
| `AC-234` bentuk 19 → 18 pada data sungguhan | Tidak ada order berisi 19 pemeriksaan; bentuknya terbukti pada harness | `NOT RUN` |
| Log SQL PostgreSQL untuk jumlah kueri | Tidak dijalankan | `NOT RUN` |

**Satu data janggal ditemukan, bukan cacat task ini.** Leukosit pada order uji punya
`ResultEnteredAt` terisi tetapi **nol** `ResultNumeric`, `ResultOptionId`, maupun `ResultValueBoundId`.
Tanpa nilai, penanda kosong adalah jawaban yang benar. Asal data itu pantas diperiksa pemiliknya
— tampaknya sisa uji lama.

**Status baru: ✅ `SELESAI`.** Seluruh kode status jalur baru — `200`, `404`, `422` — terbukti
terhadap aplikasi sungguhan. Bentuk dan penanda terbukti pada harness, dan dikonfirmasi pada data dev.
