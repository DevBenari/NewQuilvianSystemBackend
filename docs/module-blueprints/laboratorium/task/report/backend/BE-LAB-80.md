# Laporan Perubahan Backend — `BE-LAB-80`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-80` |
| Judul | Antrean validasi dua disiplin |
| Slice | Gelombang `MVP-10a` — `EPIC-LAB-16`, penutup gelombang |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian **6al.3** |
| Trace | `FR-16.6`; `LAB-DEC-135` butir 2; `02-backend-architecture.md` 21.10 butir 3 dan 4 |
| Contract version | `LAB-API-v1` **`r35`** 30.4; `LAB-VAL-v1` **`r13`** `VAL-145` — **`approved` 2026-09-25** |
| Dependency | `BE-LAB-78` ⚠ (ketiga tindakan menerima Mikrobiologi pada kode), `BE-LAB-77` ✅ (antrean itu sendiri) |
| Klasifikasi | `MEDIUM` — skor 7: berkas diperiksa 1, berkas diubah 1 (3), logika bisnis 1, kontrak API 2 (parameter yang semula diabaikan kini dibaca dan dapat `422`; dua ruas baru), database 1 (kueri baca), keamanan/auth 0 (hak yang sudah ada), UI/workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/LaboratoryManagement/`, dokumen blueprint Laboratorium |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `7ff35b8c` (branch `yoga`), di atas perubahan `BE-LAB-78`, `79`, dan `84`..`86` yang belum ter-commit |
| Tanggal | 2026-09-30 |
| Status | ✅ **`SELESAI`** — `GET /lab-worklists/validation-queue` kini membaca `discipline`: kosong = Patologi Klinik **dan** Mikrobiologi, satu nama = disiplin itu saja, Patologi Anatomi, angka, atau nilai tak dikenal → `422` `VAL-145` kata per kata. Hasil Mikrobiologi `Sementara` tidak masuk tahap mana pun; kualifikasi kosong tetap masuk. Urutan cito lalu paling lama menunggu berlaku lintas disiplin. Harness **17/17**; **HTTP terhadap PostgreSQL**: BTA Final dev — satu-satunya hasil Final di database — kini muncul dengan `discipline = Microbiology`; keempat penolakan `422` terbukti. **Satu bukti `BE-LAB-77` berbalik arah secara disengaja** (5). Build 0 error; nol migration; nol string hak akses baru; registri tetap 1577. **`MVP-10a` selesai pada kode** |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `LaboratoryManagement` |
| Pemilik / prefix registry | `Lab` — `ACTIVE` |
| Keberlakuan | `TOUCHED LEGACY` — `LabWorklistService`, `LabWorklistDtos`, `LabWorklistController` (komentar dan deskripsi aksi). Nol berkas baru |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-VAL-001` (`VAL-145` sesudah `VAL-139`), `QBE-API-001` (`422` sesuai kontrak), `QBE-DTO-001` (dua ruas baru pada DTO respons; entity tidak diekspos), `QBE-PERM-001` (aksi `Read` yang sudah ada; deskripsi diperbarui), `QBE-ENT-003` (disiplin dan tahap **turunan**, nol kolom). **Tidak berlaku:** `QBE-LOG-001` (baca saja), `QBE-ENT-001`/`CFG-001` (nol entity), `QBE-DB-*` (nol migration) |
| Governance yang dibaca | Sama dengan `BE-LAB-78` dan `BE-LAB-79` pada sesi yang sama |

---

## 1. Masalah yang diperbaiki

Sesudah `BE-LAB-78`, hasil Mikrobiologi dapat divalidasi dan dirilis — tetapi antrean validasi masih
**Patologi Klinik saja** dan parameter `discipline` diabaikan. Pemvalidasi dan perilis Mikrobiologi
tidak punya tempat untuk menemukan hasil yang menunggu mereka. Sebaliknya, bila antrean dibuka begitu
saja, hasil Mikrobiologi `Sementara` akan ikut tampil — padahal setiap hasil itu pasti ditolak `VAL-144`.

---

## 2. Proses bisnis

**Antrean Validasi dan Rilis** kini melayani dua disiplin.

| Pilihan layar | Isi antrean |
| --- | --- |
| Tanpa pilihan disiplin | Patologi Klinik **dan** Mikrobiologi, dalam satu urutan: cito lebih dulu, lalu yang paling lama menunggu |
| *Patologi Klinik* | Patologi Klinik saja |
| *Mikrobiologi* | Mikrobiologi saja |
| Patologi Anatomi atau nilai lain | Ditolak: *"Antrean validasi hanya tersedia untuk Patologi Klinik dan Mikrobiologi."* |

Setiap baris membawa **disiplinnya** dan **kualifikasi hasil** Mikrobiologi, sehingga layar dapat
menandainya tanpa menebak.

**Hasil Mikrobiologi `Sementara` tidak pernah masuk antrean**, pada tahap validasi maupun rilis
(21.10 butir 4): antrean berarti *menunggu tindakan Anda*, dan hasil itu belum boleh ditindak. Analis
membuka kembali hasilnya dan mengubah kualifikasinya bila sudah definitif — sesudah itu hasil masuk
antrean dengan sendirinya. Hasil **tanpa kualifikasi** tetap masuk, sama dengan aturan validasinya
(`ARCH-GAP-LAB-10`).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / dokumen | Kenapa dibaca |
| --- | --- |
| `roadmap/backend-roadmap.md` 6al.3 | Cakupan, tiga jebakan, verifikasi, DoD |
| `contracts/api-contract.md` 30.4; `contracts/validation-matrix.md` `VAL-145` | Bunyi pesan, nilai yang diterima, ruas baru |
| `Services/LabWorklistService.cs` — `GetValidationQueueAsync`, `TerapkanPenyaringBersama`, `TerapkanPencarian` | Antrean `BE-LAB-77`; penyaring disiplin daftar kerja |
| `Services/LabResultValidationService.cs` — `EnsureReleasableAndRunning`, `EnsureNotPreliminary` | Pembacaan disiplin `VAL-126` dan aturan kualifikasi `VAL-144` yang wajib dicerminkan |
| `Constants/LabReleasableDisciplines.cs` | Himpunan disiplin yang dapat dirilis |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Services/LabWorklistService.cs` | `ResolveQueueDisciplines` (baru, privat) menerjemahkan `discipline` menjadi himpunan disiplin atau melempar `VAL-145`. Penyaring `ClinicalPathology` yang tertulis mati diganti `disiplinDipilih.Contains(disiplin order ?? disiplin katalog)`; penyaring `ResultQualifier != Preliminary` ditambahkan; proyeksi dan item membawa `Discipline` dan `ResultQualifier`. Dokumentasi method diperbarui |
| `DTOs/LabWorklistDtos.cs` | `LabValidationQueueItemResponse` + `Discipline`, `ResultQualifier`; dokumentasi `LabValidationQueueQuery` menyebut `discipline` kini dibaca |
| `Controllers/LabWorklistController.cs` | Komentar endpoint; deskripsi `[AccessAction]` menjadi *"Melihat antrean validasi dan rilis hasil Patologi Klinik dan Mikrobiologi"* |

**Nol endpoint, nol entity, nol migration, nol string hak akses baru.** Registri tetap 1577.

**Ketiga jebakan roadmap.**

| Jebakan | Bagaimana dihindari | Bukti |
| --- | --- | --- |
| Menampilkan hasil `Sementara` bertanda | Dikeluarkan pada kueri, bukan ditandai; tahap validasi **dan** rilis | Kultur sputum cito paling lama menunggu dan kultur tervalidasi `Sementara` **absen** |
| Nilai `discipline` tak dikenal dianggap kosong | Hanya kosong/spasi yang berarti "keduanya"; selebihnya harus nama disiplin yang dapat dirilis | `Hematology`, `3`, `1` → `422`, bukan kedua disiplin |
| Tahap Mikrobiologi dengan rumus berbeda | Tahap tetap satu penyaring `LabExaminationService.HasResultStatus` untuk kedua disiplin | `resultStatus` 7/7 item identik dengan halaman hasil **disiplinnya sendiri** |

**Keputusan kecil.**

| Hal | Keputusan dan alasan |
| --- | --- |
| Angka ditolak | `discipline=3` adalah Mikrobiologi dalam enum, tetapi ditolak — sama dengan `stage` (`VAL-139`, `BE-LAB-77`): kontrak menyebut **nama**, dan angka membuat parameter bergantung pada urutan enum |
| Huruf besar-kecil | Diterima (`clinicalpathology` = `ClinicalPathology`), sama dengan `stage` |
| Himpunan "keduanya" | Diturunkan dari `LabReleasableDisciplines`, bukan ditulis ulang — Patologi Anatomi otomatis ikut saat `S4e` menambahkannya, bersama penjaga validasi, laporan, dan `resultProgress` |
| Pembacaan disiplin | Disiplin order, lalu katalog pemeriksaan bagi order lama — **sama dengan `VAL-126`**, sehingga setiap baris antrean adalah hasil yang memang diterima tindakan validasi |
| `TerapkanPenyaringBersama` **tidak** dipakai | Roadmap menyebutnya sebagai *reuse*, tetapi penyaring itu membaca disiplin **order** saja; memakainya akan menghilangkan order lama tanpa disiplin yang diterima `VAL-126`. Komentar di kode menyebut alasannya |
| Urutan pemeriksaan | `VAL-139` (tahap) lebih dulu, lalu `VAL-145` — tahap kosong dan disiplin salah sekaligus membaca pesan tahap |
| Hasil Patologi Klinik | `resultQualifier` selalu kosong |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Sesuai `r35` 30.4. **Perilaku berubah pada parameter lama:** `discipline` yang semula diabaikan kini menyaring, dan nilai yang tidak didukung kini `422`. Pemanggil yang mengirim `discipline=ClinicalPathology` mendapat hasil yang sama dengan sebelumnya; pemanggil tanpa `discipline` kini juga menerima baris Mikrobiologi. Dua ruas baru pada item |
| Database | **Baca saja**; nol kueri tambahan per halaman — 6 kueri berapa pun barisnya, halaman kosong 2 (harness `BE-LAB-77`) |
| Keamanan/Auth | Hak `LabWorklist : Read` yang sudah ada. Antrean **tidak** menyaring per kewenangan klinis orang — pemegang `LAB-VAL-PK` saja tetap melihat baris Mikrobiologi dan ditolak `403` saat menindaknya (lapis orang `BE-LAB-78`). Sesuai 21.9: pembagian per disiplin ditegakkan pada tindakan. Deskripsi `[AccessAction]` baru **tidak tampil** di registri — baris `LabWorklist : Read` memakai deskripsi yang terbaca pertama (`GET pending`) |

---

## 4. Dokumentasi endpoint

Nol endpoint baru.

| Method | Path | Perubahan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/lab-worklists/validation-queue` | `discipline` dibaca (`ClinicalPathology`, `Microbiology`, kosong = keduanya; lainnya `422` `VAL-145`); hasil `Sementara` dikeluarkan; item + `discipline`, `resultQualifier` | `LabWorklist : Read` — tetap |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build … -p:RunAnalyzers=False` | **0 error, 230 warning**; nol dari berkas yang disentuh; ketiga berkas tetap LF | `PASS` | Keluaran build |
| Harness `BE-LAB-80` | **17 `PASS`, 0 `FAIL`** | `PASS` | Rincian di bawah |
| Regresi | `BE-LAB-72` 43/43, `78` 29/29, `79` 13/13, `81` 25/25, `84` 15/15, `86` 19/19 | `PASS` | Harness yang sama |
| Regresi yang sudah tercatat berubah | `BE-LAB-73`/`74`/`75` (1 baris masing-masing), `76` (2), `82` unit (1), `83` (1), `85` (1) — **persis** baris yang dicatat `BE-LAB-78` dan `BE-LAB-79`; nol baris baru | `PASS` | Harness yang sama |
| **Regresi yang berubah — disengaja** | `BE-LAB-77`: **lima** baris, satu sebab — *Kultur urin Final* kini masuk antrean (rincian di bawah) | `PASS` — perubahan yang disetujui roadmap 6al.3 | Harness `BE-LAB-77` |
| Startup | Registri **1577** kunci dari 1577 kemampuan; galat hanya yang bawaan (`AccAccountingEvent` `42P01`); seeder radiologi menambah 0 — sama dengan startup `BE-LAB-79` | `PASS` | Log |
| **HTTP terhadap PostgreSQL** | Rincian di bawah | `PASS` | Panggilan sungguhan |

**Bukti `BE-LAB-77` yang berbalik arah — disengaja.** Roadmap 6al.3 mewajibkan laporan ini menyebutnya
sebagai perubahan yang disetujui, bukan regresi. Kelima baris gagal karena **satu** baris data yang sama,
*Kultur urin Final* (Mikrobiologi, belum divalidasi), kini masuk antrean:

| Baris harness `BE-LAB-77` | Semula | Kini |
| --- | --- | --- |
| *"Isi — hanya Final yang dapat ditindak"* | 5 baris Patologi Klinik | 6 baris — ditambah Kultur urin Final |
| *"Urutan — cito lebih dulu, lalu yang paling lama menunggu"* | Tanpa Kultur urin | Kultur urin di tempatnya menurut waktu tunggu |
| *"Keluar: … Mikrobiologi Final …"* | Kultur urin absen | Kultur urin **muncul** — inilah bukti yang dimaksud roadmap |
| *"Ruas discipline diabaikan — tetap Patologi Klinik"* | `discipline=Microbiology` → 5 baris PK | → 1 baris Mikrobiologi (`r35` 30.4) |
| *"Tidak N+1 — …"* | 5 baris = 6 kueri; 1 baris = 6; kosong = 2 | **6 baris = 6 kueri**; 1 baris = 6; kosong = 2 — sifatnya tetap, hanya jumlah baris yang diharapkan berubah |

Seluruh 16 baris `BE-LAB-77` lainnya utuh, termasuk *"resultStatus dan referenceFlag SETIAP item
identik dengan `GET /{id}/result`"* yang kini ikut mencakup Kultur urin.

**Rincian HTTP** — superadmin, baca saja. Kueri baca-saja ke database membuktikan **hanya satu**
pemeriksaan Final di seluruh dev: BTA `025be4cf` (LAB-RSMMC-000014, Mikrobiologi, kualifikasi kosong,
belum divalidasi).

| Panggilan | Hasil |
| --- | --- |
| `?stage=AwaitingValidation` | `200` — satu baris: Pewarnaan BTA Sputum, `discipline = Microbiology`, `resultQualifier = null`, `resultStatus = Final`. **Sebelum `BE-LAB-80` antrean ini kosong** (`BE-LAB-77`: *"satu-satunya hasil Final di dev (Mikrobiologi) tidak masuk antrean"*) |
| `?stage=AwaitingValidation&discipline=Microbiology` | `200` — baris yang sama |
| `?stage=AwaitingValidation&discipline=ClinicalPathology` | `200` — kosong |
| `?stage=AwaitingRelease` | `200` — kosong (nol hasil tervalidasi di dev) |
| `…&discipline=AnatomicalPathology` | `422` — *"Antrean validasi hanya tersedia untuk Patologi Klinik dan Mikrobiologi."* |
| `…&discipline=Hematology` | `422` — bunyi yang sama |
| `…&discipline=3` | `422` — bunyi yang sama, walau `3` adalah Mikrobiologi di database |
| `?discipline=Hematology` (tanpa `stage`) | `422` — *"Tahap antrean wajib dipilih…"* (`VAL-139` lebih dulu) |

**Rincian harness** — EF InMemory, `LabWorklistService` apa adanya; halaman hasil Mikrobiologi dibangun
lewat DI yang memindai service Laboratorium.

| Skenario | Hasil sebenarnya |
| --- | --- |
| Tanpa `discipline` — tahap validasi | 5 baris: dua PK dan tiga Mikrobiologi |
| Urutan lintas disiplin | Kultur darah cito (30 mnt) → Kalium cito (10 mnt) → Kultur urin (120 mnt) → Kultur order lama (90 mnt) → Kalium rutin (50 mnt) |
| Hasil `Sementara` | **Absen** — walau cito dan paling lama menunggu (200 mnt) |
| Kualifikasi kosong | **Masuk** (`ARCH-GAP-LAB-10`) |
| Patologi Anatomi Final | Absen |
| Order lama tanpa disiplin, katalog Mikrobiologi | Masuk, `discipline = Microbiology` — sama dengan `VAL-126` |
| Ruas item | `Microbiology`/`Definitive`/`Final`; PK `ClinicalPathology`, kualifikasi kosong |
| `discipline = Microbiology`; `clinicalpathology`; spasi | Mikrobiologi saja; PK saja; keduanya |
| Tahap rilis | Kultur luka tervalidasi dan Kreatinin tervalidasi, pemvalidasi terisi; kultur tervalidasi `Sementara` **absen** |
| `resultStatus` vs halaman hasil disiplinnya | **7/7** identik — Mikrobiologi dibandingkan dengan `GET /{id}/result/microbiology` (`BE-LAB-79`), PK dengan `GET /{id}/result` |
| `VAL-145` — `AnatomicalPathology`, `Hematology`, `3`, `1` | `422` kata per kata |
| Tahap kosong + disiplin salah | `422` `VAL-139` |

**Tidak dijalankan:**

- `401`/`403` lewat HTTP — atribut hak akses endpoint tidak berubah selain deskripsi; dibuktikan `BE-LAB-77`.
- Baris Mikrobiologi `Sementara` dan tahap rilis Mikrobiologi lewat HTTP — dev tidak punya datanya, dan
  menulis baris uji ke database bersama butuh wewenang tersendiri. Terbukti pada harness.
- Harness karakterisasi `BE-LAB-82` — berkas yang dikarakterisasinya (`VAL-126`, daftar pantau cito) tidak
  disentuh.
- Analyzer — build memakai `-p:RunAnalyzers=False`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-196` bagian antrean bagi Mikrobiologi | ✅ **Terpenuhi** — harness dan HTTP | BTA dev muncul |
| `VAL-145` | ✅ **Terpenuhi** — harness dan HTTP, kata per kata | — |
| Baris *Antrean dua disiplin* matriks uji | ✅ **Terpenuhi** pada harness; tanpa `discipline` dan `Microbiology` juga lewat HTTP | — |
| Verifikasi roadmap — bukti `BE-LAB-77` berbalik arah disebut sebagai perubahan yang disetujui | ✅ **Terpenuhi** — bagian 5 | — |
| DoD — antrean dua disiplin berjalan; `VAL-145` ditegakkan; hasil `Sementara` absen; laporan | ✅ **Terpenuhi** | — |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 230 warning compiler, **nol** dari berkas yang disentuh |
| Masalah yang diketahui | Lima baris harness `BE-LAB-77` kini sengaja gagal (5) — disimpan apa adanya sebagai bukti perubahan perilaku |
| Risiko tersisa | **Rendah.** Jalur baca; pemakaian Mikrobiologi sungguhan tetap tertutup oleh lapis orang sampai `MVP-10c` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | ` M` `Controllers/LabWorklistController.cs`, `DTOs/LabWorklistDtos.cs`, `Services/LabWorklistService.cs`, `roadmap/backend-roadmap.md`, `roadmap/traceability.md`; `??` laporan ini. Perubahan `BE-LAB-78`, `79`, `84`..`86` yang belum ter-commit ikut ada. **Nol operasi Git dijalankan** |
| Langkah berikutnya | **1.** `MVP-10a` selesai pada kode (`BE-LAB-78` ⚠, `79` ✅, `80` ✅) — frontend `MVP-10b` (`FE-LAB-41`..`43`) dapat memakai ruas `r35`. **2.** Langkah rilis `MVP-10c` tetap `BLOCKED` (6al.5): `LAB-COORD-016`, penunjukan, kebijakan jabatan |
