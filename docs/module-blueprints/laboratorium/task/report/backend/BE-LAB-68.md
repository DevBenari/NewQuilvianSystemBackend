# Laporan Perubahan Backend — `BE-LAB-68`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-68` |
| Judul | Penjaga hasil Final |
| Slice | Gelombang `MVP-8a` — `EPIC-LAB-14`, perbaikan `S4b` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian **6aj.2**, termasuk perluasan 2026-09-25 |
| Trace | `FR-14.3`, `FR-14.4`; `LAB-DEC-147`, `LAB-DEC-141` butir 4; `VAL-120`, `VAL-121`; `AC-225` beserta jalur setengah jalannya, `AC-226`, `AC-227`; temuan `02-backend-architecture.md` 20.1 dan 20.12 |
| Contract version | `LAB-VAL-v1` **`r11`** `VAL-120`, `VAL-121`; `LAB-STATE-v1` **`r4`** bagian 6.3; `LAB-API-v1` **`r33`** 28.2 (kode `409` bila *baris baru saja diubah orang lain*) — ketiganya **`approved` 2026-09-24**. Perluasan `Version` berasal dari `02-backend-architecture.md` 20.1, yang disetujui bersama `r34` pada 2026-09-25 |
| Dependency | `BE-LAB-67` ⚠ selesai dengan batas verifikasi, ter-commit pada `b23556b2` — `FinalizeResultAsync` dan `ReopenResultAsync` sudah berdiri |
| Klasifikasi | `MEDIUM` — skor 5: repository 0, berkas diperiksa 1 (±14), berkas diubah 0 (3), logika bisnis 1, kontrak API 1 (memenuhi kode `409` yang sudah dikontrakkan), database 1 (perilaku persistensi dan token konkurensi yang sudah ada), keamanan/auth 0, UI/workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/LaboratoryManagement/` dan dokumen blueprint Laboratorium |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `017d1819` (branch `yoga`) — merge upstream yang dibuat pemilik modul 2026-09-29 10:34, di atas `b23556b2` |
| Tanggal | 2026-09-29 |
| Status | ✅ **`SELESAI`** — 2026-09-29 sore (bagian 8). **`409` `VAL-120` dan `VAL-121` terbukti lewat HTTP** pada hasil Mikrobiologi yang sudah Final, dengan status Final dan `Version` identik sebelum dan sesudah — nol baris berubah. Jalur Patologi Klinik memakai aturan yang sama persis dan terbukti pada harness 19/19, sebab dev tidak punya hasil PK Final dan membuatnya berarti menulis data klinis. *Semula `SELESAI DENGAN BATAS VERIFIKASI`: HTTP tertahan seeder Hemodialisa* |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `LaboratoryManagement` |
| Pemilik / prefix registry | `Lab` — `ACTIVE` (`MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 27 dan 114) |
| Keberlakuan | `TOUCHED LEGACY` — tiga berkas yang sudah ada; nol entity, nol berkas baru |
| QBE yang berlaku | `QBE-SVC-001` (penjaga tinggal di service), `QBE-API-001` (`409` lewat `ApiResponse` yang mapan), `QBE-VAL-001`, `QBE-TXN-001` (transaksi simpan Mikrobiologi dan letak penjaga di luar transaksi), `QBE-LOG-001` |
| Governance yang dibaca | `AGENTS.md` backend; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `rules/backend/` suite Skill |

---

## 1. Masalah yang diperbaiki

**Hasil yang sudah dinyatakan selesai masih dapat berubah tanpa jejak.** Ada dua celah, dan
keduanya menjawab `200` tanpa satu pun galat.

**Celah pertama: Final tidak menjaga apa pun.** Sesudah analis menekan Final, siapa pun yang
memegang izin hasil masih dapat menyimpan ulang nilainya atau mencatat konsultasi. Final hanya
tercatat sebagai `FinalizedAt`, dan tidak ada penulisan yang membacanya.

> Pukul 09.10 analis Sari menekan Final pada Kalium bernilai 4,6. Pukul 09.12 tab lama milik
> rekannya, yang masih memuat nilai sebelum diulang, mengirim Kalium 6,1. Sebelum task ini
> **sistem menerimanya**. Kalium Final kini bernilai 6,1, padahal nilai yang dinyatakan selesai
> adalah 4,6, dan tidak ada Reopen maupun alasan yang tercatat.

**Celah kedua: token `Version` tidak pernah bentrok.** `LabExamination.Version` sudah dikonfigurasi
sebagai token konkurensi (`LabExaminationConfiguration.cs:27`). Namun hanya batal, cito, dan duplo
yang menaikkannya. Simpan hasil, Final, dan konsultasi menyimpan tanpa menaikkannya, sehingga dua
permintaan bersamaan sama-sama lolos. Janji `r33` 28.2, yaitu `409` bila *baris baru saja diubah
orang lain*, **belum benar pada kode**. Temuan ini dicatat `02-backend-architecture.md` 20.1 dan
diteruskan ke task ini lewat 20.12.

---

## 2. Proses bisnis

**Tujuan.** Hasil yang sudah Final hanya dapat berubah lewat satu pintu: **Reopen beralasan**.

**Pelaku.** Analis pemegang `LabExaminationResult : Update` (`BE-LAB-67`).

**Keadaan penulisan hasil** diturunkan dari fakta yang tersimpan (`LAB-STATE-v1` `r4` 6.1). Tidak
ada status baru.

| Keadaan | Diturunkan dari |
| --- | --- |
| Belum diisi | `ResultEnteredAt` kosong |
| Draft | `ResultEnteredAt` terisi, `FinalizedAt` kosong |
| Final | `FinalizedAt` terisi |

**Alur normal, berurutan.**

1. Analis menyimpan hasil → **Draft**. Boleh diulang berkali-kali.
2. Analis menekan Final → **Final**.
3. Bila ada yang perlu diubah, analis menekan Reopen dengan alasan wajib → **Draft**.
   `ReopenCount` naik satu, dan satu baris `LabTransitionHistory` mencatat alasannya.
4. Analis menyimpan perbaikan, lalu menekan Final lagi. `FinalizedAt` berisi waktu baru.

**Jalur tidak normal.**

| Keadaan | Jawaban sistem | Aturan |
| --- | --- | --- |
| Simpan hasil Patologi Klinik atau Mikrobiologi pada hasil Final | `409` — *"Hasil ini sudah dinyatakan selesai. Buka kembali lebih dulu bila perlu diubah."* | `VAL-120` |
| Catat konsultasi pada hasil Final | `409` — *"Hasil ini sudah dinyatakan selesai. Buka kembali lebih dulu sebelum mencatat konsultasi."* | `VAL-121` |
| Final ditekan dua kali berurutan | `409` — *"Hasil pemeriksaan ini sudah dinyatakan selesai."* | Aturan yang sudah berjalan |
| Dua permintaan menulis baris yang sama pada saat bersamaan | Yang pertama `200`; yang kedua `409` — *"Hasil ini baru saja diubah orang lain. Muat ulang lalu ulangi."* | `r33` 28.2 |

**Jalur setengah jalan, dan kenapa letak penjaga menentukan segalanya.** Simpan Mikrobiologi
**mengganti** seluruh isolat: isolat lama dibuang lebih dulu, lalu yang baru ditambahkan
(`AC-114`). Bila penjaga Final diletakkan sesudah isolat lama ditandai hapus, permintaan yang
ditolak dapat meninggalkan hasil tanpa isolat.

> Kultur urin sudah Final dengan satu isolat *Escherichia coli* dan dua belas baris antibiogram.
> Permintaan simpan yang mengganti isolat itu menjadi *Klebsiella pneumoniae* ditolak `409`
> **sebelum** isolat lama disentuh. Yang tersimpan tetap satu isolat *E. coli* dan dua belas
> baris.

**Yang tidak berubah.** Reopen tidak menaikkan `Version`. Perbaikannya milik `BE-LAB-73` bersama
penjaga `VAL-136` (roadmap 6aj.2). Tidak ada celah baru dari keputusan itu: Reopen hanya berlaku
pada hasil Final, dan ketiga penulisan hasil kini **menolak** hasil Final. Jadi tidak ada penulisan
hasil sah yang dapat menyela Reopen.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / dokumen | Kenapa dibaca |
| --- | --- |
| `roadmap/backend-roadmap.md` 6aj.2 | Cakupan, perluasan 2026-09-25, verifikasi, dan DoD |
| `contracts/validation-matrix.md` baris 534-543 | Teks pesan `VAL-120`/`VAL-121` persis, dan contoh jalur setengah jalan |
| `contracts/state-transition-matrix.md` bagian 6 | Tindakan sah dan tidak sah, beserta kodenya |
| `02-backend-architecture.md` 20.1 dan 20.12 | Temuan `Version` dan pesan `409` konkurensi |
| `testing/acceptance-test-matrix.md` baris 592-595 | Isi `AC-225`..`AC-227` |
| `Repositories/Configurations/.../LabExaminationConfiguration.cs` | `Version` sudah `IsConcurrencyToken()` — nol migration diperlukan |
| `Services/LabExaminationService.cs` | Tiga penulisan, `SaveAsync`, pola kenaikan `Version` pada batal/cito/duplo |
| `Services/LabMicrobiologyResultService.cs` | Transaksi simpan dan `ReplaceIsolatesAsync` |
| `Services/LabSpecimenService.cs:2033-2044` | Pola `SaveWithConcurrencyGuardAsync` yang sudah ada di modul ini |
| `Controllers/LabExaminationController.cs` | Pemetaan exception ke kode status pada keempat endpoint |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Services/LabExaminationService.cs` | **Pembantu baru:** `EnsureResultNotFinalized` (`VAL-120`, `internal static` agar Mikrobiologi memakai salinan yang sama), konstanta `ResultConcurrencyMessage`, dan `SaveResultWriteAsync` yang menerjemahkan `DbUpdateConcurrencyException` menjadi `409`. **`SetResultAsync`:** penjaga `VAL-120` sesudah pemeriksaan batal/gugur dan **sebelum** validasi isian; menaikkan `Version`. **`FinalizeResultAsync`:** menaikkan `Version`. **`RecordConsultationAsync`:** penjaga `VAL-121` sebelum validasi isian; menaikkan `Version` |
| `Services/LabMicrobiologyResultService.cs` | `SetResultAsync`: penjaga `VAL-120` **sebelum** transaksi dibuka dan sebelum `ReplaceIsolatesAsync`; menaikkan `Version` di dalam transaksi; `DbUpdateConcurrencyException` di-rollback lalu dijawab `409` |
| `Controllers/LabExaminationController.cs` | `RecordConsultation` dan `SetMicrobiologyResult` kini menangkap `LabExaminationConflictException` menjadi `409`, dan `[ProducesResponseType(409)]` ditambahkan. Tanpa ini penjaga baru pada kedua endpoint akan menjadi `500`. `SetResult` dan `FinalizeResult` sudah menangkapnya |

**Nol migration.** `Version` sudah menjadi token konkurensi sejak awal, jadi yang kurang hanya
kenaikannya. **Nol perubahan izin**, dan **Reopen tidak disentuh**.

**Kenapa penjaga diletakkan sebelum validasi isian.** Hasil Final harus dijawab stabil dengan
`409`, apa pun isi permintaannya. Bila validasi isian lebih dulu, tab lama yang mengirim isian tidak
lengkap ke hasil Final mendapat `422` tentang isian. Petugas lalu memperbaiki isiannya, hanya untuk
ditolak lagi dengan alasan yang berbeda.

**Kenapa satu salinan aturan.** `VAL-120` berlaku bagi dua service. Aturannya ditulis sekali di
`LabExaminationService.EnsureResultNotFinalized` dan dipanggil dari keduanya, supaya teks pesan dan
syaratnya tidak bercabang.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Memenuhi kontrak yang sudah disetujui, tidak mengubahnya.** Route, request, dan response tetap. Kode `409` baru muncul pada keempat penulisan sesuai `r33` 28.2 dan `LAB-STATE-v1` `r4` 6.3. Konsumen yang terdampak: halaman Mikrobiologi dan halaman hasil Patologi Klinik kini dapat menerima `409`. Penanganannya milik `FE-LAB-35` (`FR-14.12`, `AC-228`) |
| Database | **Nol migration, nol perubahan schema.** Keempat penulisan kini menaikkan `LabExamination.Version` satu kali per penulisan yang berhasil. Simpan Mikrobiologi tetap satu transaksi dan satu `SaveChangesAsync` |
| Keamanan/Auth | **`NOT APPLICABLE`.** Izin tidak berubah; kelima tindakan tetap `LabExaminationResult : Update` dari `BE-LAB-67`. Payload log tidak berubah |

---

## 4. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Examination

Base URL: `api/v1/health-services/laboratory-management/lab-examinations`

| Method | Path | Kegunaan | Hak akses | Yang berubah |
| --- | --- | --- | --- | --- |
| `PUT` | `/{id}/result` | Mengisi hasil Patologi Klinik | `LabExaminationResult : Update` | `409` `VAL-120` bila Final; `409` bila bentrok; `Version` naik |
| `PUT` | `/{id}/result/microbiology` | Mengisi hasil Mikrobiologi beserta isolat dan antibiogram | `LabExaminationResult : Update` | `409` `VAL-120` bila Final, **sebelum** isolat lama disentuh; `409` bila bentrok; `Version` naik |
| `POST` | `/{id}/result/finalize` | Menyatakan penulisan hasil selesai — bukan rilis | `LabExaminationResult : Update` | `409` bila bentrok; `Version` naik |
| `PUT` | `/{id}/result/consultation` | Mencatat kepada siapa hasil dikonsultasikan dan kapan | `LabExaminationResult : Update` | `409` `VAL-121` bila Final; `409` bila bentrok; `Version` naik |

`POST /{id}/result/reopen` **tidak berubah** pada task ini.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` | **0 error**, 230 warning, 92 detik. **Nol warning** menyebut ketiga berkas yang disentuh | `PASS` | Keluaran build disaring pada ketiga nama berkas |
| `PermissionRegistryValidator` terhadap DLL baru | **Lolos** — 1564 kunci. Naik dari 1541 karena controller Finance dan Accounting dari merge upstream `017d1819`, bukan dari task ini. Nol route `result/microbiology/(finalize\|reopen\|consultation)` | `PASS` | Harness validator yang sama dengan `BE-LAB-67` |
| Startup Development | **Masih berhenti** di `HemodialysisMasterDataSeeder`: `42P01 relation "public.HmdChecklistItem" does not exist` | `EXISTING / ENVIRONMENT ISSUE` | Dicoba ulang 2026-09-29 sesudah merge upstream. Merge hanya menambah registrasi Gizi, Accounting, dan satu seeder Finance di `Program.cs`; penahannya tidak berubah |
| **Harness perilaku EF InMemory** — 19 skenario di bawah | **19 `PASS`, 0 `FAIL`** | `PASS` | Harness sekali pakai di luar repository. Ia memakai `LabExaminationService` dan `LabMicrobiologyResultService` dari DLL hasil build dengan database InMemory tersendiri. **Nol baris ditulis ke database dev bersama** |

**Rincian harness.** Setiap baris dibaca ulang lewat `DbContext` baru, bukan dari objek yang
masih dipegang service.

| Skenario | Hasil sebenarnya |
| --- | --- |
| `AC-225` Mikrobiologi — hasil Final disimpan ulang dengan isolat pengganti | `409` *"Hasil ini sudah dinyatakan selesai. Buka kembali lebih dulu bila perlu diubah."* |
| `AC-225` **jalur setengah jalan** — isi basis data sesudah penolakan | **Tetap 1 isolat (*Escherichia coli*) dan 12 baris antibiogram** |
| `AC-225` Mikrobiologi — `Version` dan `ResultEnteredAt` | Tidak bergerak — `Version` 3 → 3 |
| `AC-225` Patologi Klinik — tab lama mengirim Kalium 6,1 ke hasil Final 4,6 | `409`; nilai **tetap 4,6**; `Version` 5 → 5 |
| `AC-226` konsultasi pada hasil Final | `409` *"… sebelum mencatat konsultasi."*; `ConsultedToName` tetap kosong; `Version` 2 → 2 |
| `AC-227` Reopen beralasan | `200`; `FinalizedAt` kosong; `ReopenCount` = 1; `Version` 3 → 3 — Reopen tidak menaikkannya, sesuai cakupan |
| `AC-227` riwayat Reopen | Tepat 1 baris `LabTransitionHistory` dengan alasan *"Salah ketik jenis kultur"* |
| `AC-227` simpan Mikrobiologi sesudah Reopen | `200`; tersimpan; `Version` 3 → **4** |
| `AC-227` Final lagi | `200`; `FinalizedAt` **berisi waktu baru**; `Version` 4 → **5** |
| Final ditekan dua kali berurutan | `409` *"Hasil pemeriksaan ini sudah dinyatakan selesai."* |
| **Dua Final bersamaan** — permintaan kedua sudah membaca baris sebelum yang pertama menyimpan | Pertama `200`; kedua **`409`** *"Hasil ini baru saja diubah orang lain. Muat ulang lalu ulangi."* |
| Dua Final bersamaan — `Version` | 7 → **8**: naik tepat satu |
| Simpan Patologi Klinik sesudah Reopen | `200`; nilai 4,8; `Version` 5 → **6** |
| Konsultasi pada Draft | `200`; `Version` 6 → **7** |

**Batas harness, disebut apa adanya.**

- **InMemory mengabaikan transaksi.** Jalur rollback Mikrobiologi sesudah bentrok tidak diuji
  terhadap PostgreSQL. Jalur setengah jalan tetap terbukti karena penolakan terjadi **sebelum**
  transaksi dibuka dan sebelum isolat mana pun disentuh. Hal itu tidak bergantung pada transaksi.
- **Bentrok `Version` memakai pemeriksaan token milik EF Core**, yang sama pada InMemory dan
  PostgreSQL. EF menulis `WHERE "Version" = <nilai asli>` dan menganggap nol baris terdampak sebagai
  bentrok. Bentroknya disimulasikan dengan dua `DbContext`, bukan dua permintaan HTTP sungguhan.
- **Bukan uji kontrol negatif.** Harness tidak dijalankan terhadap kode sebelum task ini. Bahwa kode
  lama gagal pada skenario yang sama diketahui dari pembacaan kode dan dari temuan 20.1: tidak ada
  pembacaan `FinalizedAt`, dan tidak ada kenaikan `Version`.

Uji manual: **`NOT FEASIBLE`** saat ini — aplikasi tidak dapat start pada database dev bersama.

**Tidak dijalankan:**

- Analyzer — build memakai `-p:RunAnalyzers=False`, sesuai praktik tetap repository ini.
- Semua panggilan HTTP. Satu-satunya penyebab adalah penahan Hemodialisa. Begitu aplikasi hidup,
  `409` `VAL-120`/`VAL-121` dapat dibuktikan tanpa menulis apa pun ke database: cukup kirim simpan
  atau konsultasi ke pemeriksaan yang sudah Final.
- Skenario yang menulis ke database dev bersama, seperti Reopen, simpan, Final, dan Final
  bersamaan. Perlu instruksi eksplisit karena mengubah data milik bersama.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-225` — hasil Patologi Klinik dan Mikrobiologi yang sudah Final disimpan ulang → `409` `VAL-120`; isi basis data tidak berubah | ✅ **Terpenuhi** pada harness; HTTP `NOT RUN` | Harness baris 1-4 |
| `AC-225` jalur setengah jalan — tetap satu isolat dan dua belas baris | ✅ **Terpenuhi** pada harness | 1 isolat *E. coli*, 12 baris antibiogram sesudah penolakan |
| `AC-226` — konsultasi dicatat pada hasil Final → `409` `VAL-121` | ✅ **Terpenuhi** pada harness; HTTP `NOT RUN` | Harness |
| `AC-227` — Reopen beralasan, simpan, Final lagi: `ReopenCount` naik satu; satu baris riwayat beralasan; `FinalizedAt` baru | ✅ **Terpenuhi** pada harness | Harness |
| Verifikasi roadmap — dua Final bersamaan → satu `200`, satu `409`; `Version` naik tepat satu per penulisan berhasil | ✅ **Terpenuhi** pada harness | `Version` 7 → 8 pada balapan; +1 pada simpan PK, simpan Mikrobiologi, Final, dan konsultasi |
| DoD — `VAL-120` dan `VAL-121` ditegakkan pada ketiga jalur | ✅ **Terpenuhi** | Simpan PK, simpan Mikrobiologi, konsultasi |
| DoD — jalur setengah jalan terbukti **nol** mengubah data | ✅ **Terpenuhi** pada harness | Lihat di atas |
| DoD — keempat penulisan menaikkan `Version` dan bentrokannya `409` | ✅ **Terpenuhi** | Kode dan harness |
| DoD — laporan `BE-LAB-68.md` | ✅ **Terpenuhi** | Berkas ini |
| Kode status dibuktikan terhadap aplikasi yang berjalan | **Belum terpenuhi** | Penahan Hemodialisa, bagian 5 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 230 warning compiler, **nol** di ketiga berkas yang disentuh — sama dengan `BE-LAB-67` |
| Masalah yang diketahui | **Batal, cito, dan duplo masih dapat menjawab `500` saat bentrok.** Ketiganya sudah menaikkan `Version` sejak dulu, tetapi `SaveAsync` hanya menangkap pelanggaran unik, sehingga `DbUpdateConcurrencyException` lolos menjadi `500`. Kelas galat ini **sudah ada sebelum task ini**, misalnya batal dan cito yang bersamaan. Sesudah task ini jendelanya sedikit melebar: batal yang bertabrakan dengan simpan hasil kini ikut bentrok, sebab simpan hasil menaikkan `Version`. Hasil akhirnya tetap aman — tidak ada data yang tertimpa, dan yang kalah ditolak. Perbaikannya satu `catch` di `SaveAsync`, tetapi itu menyentuh tiga tindakan di luar cakupan task ini. **Disarankan dilipat ke task Lab berikutnya yang menyentuh `SaveAsync`** |
| Risiko tersisa | **Sedang pada rilisnya.** Halaman Mikrobiologi dan Patologi Klinik yang belum diperbarui `FE-LAB-35` akan menerima `409` saat menyimpan ulang hasil Final — tindakan yang sebelumnya diterima diam-diam. Tanpa `FE-LAB-35`, pesannya mungkin tidak tampil dengan baik. `BE-LAB-67`, `BE-LAB-68`, dan `FE-LAB-35` **wajib dirilis bersama** (6aj.5). Kode status belum dibuktikan lewat HTTP |
| Perubahan sampingan | `NONE` di repository. Dua harness dibuat di scratchpad sesi, di luar repository. Startup yang gagal kembali menjalankan seeder biasa ke database dev bersama, sama seperti `BE-LAB-67` |
| Interupsi | **Ada, bukan dari pekerjaan ini.** Di tengah pengerjaan, pemilik modul meng-commit `BE-LAB-67` (`b23556b2`) dan me-merge upstream (`017d1819`). Commit itu diperiksa: berisi tepat kelima berkas `BE-LAB-67`, dan merge-nya **nol** menyentuh `LaboratoryManagement`, dokumen Laboratorium, `Services/Security`, maupun modul Hemodialisa. Pekerjaan dilanjutkan di atas `017d1819` tanpa suntingan ganda |
| Status Git | ` M Areas/HealthServices/LaboratoryManagement/Controllers/LabExaminationController.cs`; ` M Areas/HealthServices/LaboratoryManagement/Services/LabExaminationService.cs`; ` M Areas/HealthServices/LaboratoryManagement/Services/LabMicrobiologyResultService.cs`; ditambah laporan ini, `roadmap/backend-roadmap.md`, dan `roadmap/traceability.md`. **Nol operasi Git dijalankan** |
| Langkah berikutnya | **1.** Penahan Hemodialisa diselesaikan pemiliknya, lalu `409` `VAL-120`/`VAL-121` dibuktikan lewat HTTP pada pemeriksaan yang sudah Final — tanpa menulis data. **2.** `BE-LAB-69` kini `SIAP DIKERJAKAN`. **3.** `FE-LAB-35` menangani `409` dan `403` sebelum rilis `MVP-8a`. **4.** Pertimbangkan satu `catch` konkurensi pada `SaveAsync` untuk batal, cito, dan duplo |

---

## 8. Verifikasi runtime susulan — 2026-09-29 sore

Penahan Hemodialisa ditutup atas instruksi eksplisit pemilik modul (bagian 8
[`BE-LAB-70.md`](BE-LAB-70.md)). Sesuai bagian 5, `409` dibuktikan **tanpa menulis apa pun**, dengan
mengirim simpan dan konsultasi ke pemeriksaan yang sudah Final. Pemeriksaan ujinya adalah
*Pewarnaan BTA Sputum*, Mikrobiologi, `FinalizedAt` terisi.

| Skenario | Hasil | Klasifikasi |
| --- | --- | --- |
| `VAL-120` — `PUT /{id}/result/microbiology` atas hasil Final | `409` *"Hasil ini sudah dinyatakan selesai. Buka kembali lebih dulu bila perlu diubah."* | `PASS` |
| `VAL-121` — `PUT /{id}/result/consultation` atas hasil Final | `409` *"Hasil ini sudah dinyatakan selesai. Buka kembali lebih dulu sebelum mencatat konsultasi."* | `PASS` |
| Final ditekan lagi | `409` *"Hasil pemeriksaan ini sudah dinyatakan selesai."* | `PASS` |
| Isi database sebelum dan sesudah | Status Final dan `Version` **identik** — nol baris berubah | `PASS` |
| `VAL-120` pada jalur **Patologi Klinik** lewat HTTP | Tidak dijalankan | `NOT RUN` |
| Dua Final bersamaan lewat HTTP | Tidak dijalankan | `NOT RUN` |

**Kenapa jalur Patologi Klinik tidak dijalankan.** Di database dev **nol** hasil Patologi Klinik
berstatus Final, dan membuatnya berarti menulis data klinis bersama. Jalur itu memanggil aturan
yang **sama persis** (`EnsureResultNotFinalized`, satu salinan) yang baru saja terbukti lewat HTTP
pada Mikrobiologi, dan perilakunya terbukti pada harness (bagian 5). Dua Final bersamaan pun
menulis data, dan tetap terbukti pada harness.

**Status baru: ✅ `SELESAI`.** Kode status `409` untuk `VAL-120` dan `VAL-121` kini terbukti terhadap
aplikasi sungguhan, dan jalur setengah jalan terbukti **nol** mengubah data.
