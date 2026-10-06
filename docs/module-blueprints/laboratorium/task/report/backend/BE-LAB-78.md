# Laporan Perubahan Backend — `BE-LAB-78`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-78` |
| Judul | Validasi, rilis, dan pengembalian menerima Mikrobiologi |
| Slice | Gelombang `MVP-10a` — `EPIC-LAB-16`, `S4d-1` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian **6al.1** |
| Trace | `FR-16.1`, `FR-16.2`, `FR-16.3`, `FR-16.5`; `LAB-DEC-085` (`INV-53`), `LAB-DEC-097`, `LAB-DEC-114` (`INV-52`), `LAB-DEC-143`, `LAB-DEC-148`, `LAB-DEC-152`, `LAB-DEC-153`; 21.10 butir 1, 2, 5 |
| Contract version | `LAB-API-v1` **`r35`** 30.2; `LAB-VAL-v1` **`r13`** `VAL-126` bunyi baru dan `VAL-144`; `LAB-STATE-v1` **`r6`** 8.2-8.3; `LAB-INT-v1` **`r5`** 9.1-9.2 — keempatnya **`approved` 2026-09-25**; `LAB-PERM-v1` rev 11 apa adanya |
| Dependency | `BE-LAB-72` ✅, `BE-LAB-73`..`75` ⚠ (batas verifikasi HTTP terwarisi), `BE-LAB-82` ✅ (`LabReleasableDisciplines`) |
| Klasifikasi | `HIGH` — roadmap menandainya **penjaga keselamatan epic ini**: kewenangan klinis per disiplin. Skor: berkas diubah 2 (5), logika bisnis 2, kontrak API 2 (aturan approved `r12` berubah bunyi), keamanan/auth **3** (kode kewenangan per disiplin), UI/workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/LaboratoryManagement/`, dokumen blueprint Laboratorium |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `7ff35b8c` (branch `yoga`), di atas perubahan `BE-LAB-84`..`86` yang belum ter-commit. Rancangan bagian 21 disusun pada `cfafad8d`; impact scan `BE-LAB-81`/`82`/`84` sudah memastikan nol perubahan source relevan di commit sesudahnya |
| Tanggal | 2026-09-30 |
| Status | ✅ **`SELESAI`** (naik 2026-10-06) — jalur berhasil lewat **HTTP asli**: dr. Nabila memvalidasi, mengembalikan, dan merilis BTA `LAB-RSMMC-000014`; dr. Bima (kode PK) ditolak `403` berkata *Mikrobiologi*; rilis tercatat satu dokumen rekam medis. Baris `S4` empat mata dan konkurensi diuji khusus Mikrobiologi — harness **37/37**. Lihat 8. *(Semula: ⚠ — ketiga tindakan menerima Mikrobiologi **dengan kode Mikrobiologi**. Harness **29/29**, `AC-241` diuji **pertama**: pemegang kode Patologi Klinik → `403` berkata *Mikrobiologi*; pemegang `LAB-VAL-MB` → `200`. `VAL-144`, urutan pemeriksaan, `ARCH-GAP-LAB-10`, `INV-53`, `INT-08` Mikrobiologi, dan `VAL-126` bunyi baru terbukti. **HTTP terhadap PostgreSQL** pada jalur penolakan, **nol penulisan**. Build 0 error; nol migration; nol string hak akses baru; registri tetap 1577. **Batasnya sama dengan `BE-LAB-73`..`75`:** jalur berhasil lewat HTTP menunggu kode `LAB-COORD-016` di katalog Human Resource, penunjukan, dan kebijakan jabatan)* |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `LaboratoryManagement` |
| Pemilik / prefix registry | `Lab` — `ACTIVE` |
| Keberlakuan | `TOUCHED LEGACY` — `LabResultValidationService`, `LabClinicalPrivilegeCodes`, `LabReleasableDisciplines`, `LabExaminationService` (dokumentasi), `LabExaminationController` (deskripsi aksi) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-VAL-001` (`VAL-126`, `VAL-144`, urutan 21.1), `QBE-API-001` (kode status kontrak), `QBE-PERM-001` (metadata aksi yang sudah ada; deskripsi diperbarui), `QBE-LOG-001` (pelaku pada log validasi/rilis/kembalikan — **diperbaiki**, lihat 3.2). **Tidak berlaku:** `QBE-DB-*`/`QBE-ENT-*` (nol migration, nol kolom — ke-14 kolom `BE-LAB-70` sudah per pemeriksaan) |
| Governance yang dibaca | `AGENTS.md` backend; `BACKEND_ENGINEERING_CONTRACT.md`; `MODULE_OWNERSHIP_PREFIX_REGISTRY.md`; `Services/Logging/LoggerService.cs` |

---

## 1. Masalah yang diperbaiki

Validasi, rilis, dan *Kembalikan ke analis* hanya menerima Patologi Klinik. Hasil Mikrobiologi — kultur
urin, BTA sputum — berhenti di Final tanpa jalan untuk disahkan, walau pemegang validasinya sudah
ditetapkan (`LAB-DEC-152`). Membukanya dengan sembrono berbahaya: bila penjaga disiplin dibuka **tanpa**
memilih kode kewenangan per disiplin, dokter Patologi Klinik dapat memvalidasi hasil Mikrobiologi tanpa
satu galat pun.

---

## 2. Proses bisnis

| Siapa / apa | Sekarang |
| --- | --- |
| Dokter yang ditunjuk **validasi Mikrobiologi** (dr. Nabila) | Dapat memvalidasi hasil Mikrobiologi Final |
| Dokter pemegang kode **Patologi Klinik** saja | **Ditolak** memvalidasi, merilis, atau mengembalikan hasil Mikrobiologi — *"Anda belum ditunjuk sebagai pemegang kewenangan validasi Mikrobiologi."* |
| Perilis yang ditunjuk **rilis Mikrobiologi** | Dapat merilis — dan **tepat satu** dokumen tercatat di rekam medis pasien untuk pemeriksaan itu, **nol** untuk isolatnya |
| Hasil berkualifikasi **Sementara** | **Ditolak** validasi dan rilis — *"Hasil Mikrobiologi sementara belum dapat divalidasi maupun dirilis. Buka kembali dan ubah kualifikasinya bila hasil sudah definitif."* Analis membuka kembali, mengubah kualifikasi menjadi Definitif, lalu Final ulang |
| Hasil tanpa kualifikasi | Diterima — kosong bukan Sementara (21.10 butir 2) |
| Isolat dan antibiogram sesudah validasi | Terkunci (`VAL-120`) sampai hasil dikembalikan ke analis |
| Patologi Anatomi | Tetap ditolak — *"Validasi dan rilis hasil Patologi Anatomi belum tersedia."* |

**Satu perilaku berubah seketika sesudah deploy, dan itu disengaja** (21.7): sebelum kode Mikrobiologi
ada di katalog Human Resource, petugas yang mencoba memvalidasi hasil Mikrobiologi membaca penolakan
kewenangan, bukan lagi *"belum tersedia"*.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / dokumen | Kenapa dibaca |
| --- | --- |
| `roadmap/backend-roadmap.md` 6al, 6al.1 | Cakupan, empat jebakan, verifikasi, DoD |
| `contracts/api-contract.md` bagian 30 | Tiga tindakan bagi Mikrobiologi, kode status tambahan |
| `contracts/validation-matrix.md` `VAL-126` bunyi `r13`, `VAL-144` | Bunyi dan kode |
| `contracts/state-transition-matrix.md` bagian 8 | Syarat tambahan per tindakan; *Kembalikan* tanpa `VAL-144` |
| `02-backend-architecture.md` 21.1, 21.4, 21.10 | Urutan pemeriksaan, kelas yang berubah, keputusan |
| `testing/acceptance-test-matrix.md` amandemen `S4d-1` | `AC-241`, `VAL-144`, jalur Reopen, `ARCH-GAP-LAB-10`, `INV-53`, `INT-08` |
| `Services/LabResultValidationService.cs`, `LabClinicalPrivilegeResolver.cs`, `Constants/*` | Titik kode kewenangan dipilih |
| `Services/LabMicrobiologyResultService.cs` — `SetResultAsync` | Penjaga `VAL-120` yang menegakkan `INV-53` |
| `Services/Logging/LoggerService.cs` | Cara ruas payload dibaca — lihat 3.2 |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Constants/LabClinicalPrivilegeCodes.cs` | `ValidationMicrobiology = "LAB-VAL-MB"`, `ReleaseMicrobiology = "LAB-REL-MB"` — **usulan**, final lewat `LAB-COORD-016`. `For(Mikrobiologi, …)` mengembalikan keduanya; `For(Patologi Anatomi, …)` tetap `null` (fail-closed berlapis) |
| `Constants/LabReleasableDisciplines.cs` | Tambah **Mikrobiologi** — di sini, bukan di penjaga service (catatan urutan 6al.1). Dokumentasi: menambah disiplin wajib disertai kodenya pada perubahan yang sama |
| `Services/LabResultValidationService.cs` | `EnsureClinicalPathologyAndRunning` → **`EnsureReleasableAndRunning`**, mengembalikan disiplin pemeriksaan; bunyi `VAL-126` `r13`. **`EnsureNotPreliminary`** — `VAL-144` pada validasi dan rilis, sesudah `VAL-127`, sebelum `VAL-124`/`VAL-133`; *Kembalikan* tidak memakainya. **`EnsureAppointedAsync(…, discipline, …)`** — kode dari disiplin pemeriksaan. *Kembalikan*: kedua resolver memakai disiplin pemeriksaan; pesan `403` menyebut disiplin itu. Tiga pencatatan log diperbaiki (di bawah) |
| `Services/LabExaminationService.cs` | Dokumentasi `BuildCompletionResponse` saja — `IsReleased` sudah dibaca dari `ReleasedAt` bagi setiap disiplin; catatan *"Mikrobiologi tetap salah sampai S4d"* diakhiri |
| `Controllers/LabExaminationController.cs` | Deskripsi aksi `Validate` dan `Release` kini *"… Patologi Klinik dan Mikrobiologi …"* — teks yang dibaca admin di layar Akses Role. **Nama aksi dan string izin tidak berubah** |

**Nol migration, nol kolom, nol string hak akses baru.** Registri tetap 1577.

**Cacat log yang ditemukan dan diperbaiki pada berkas yang sama.** Payload `AuditAsync` validasi, rilis,
dan pengembalian (`BE-LAB-73`..`75`) memuat properti `Id` (id pemeriksaan). `LoggerService` membaca
ruas bernama `Id` — tidak peka huruf besar-kecil — untuk **menimpa `UserId` pelaku**, dan tidak menuliskan
objek payload ke log. Akibatnya log aplikasi ketiga tindakan itu mencatat **id pemeriksaan sebagai
pelaku**. Jejak audit utama tidak terdampak: `LabTransitionHistory` menyimpan pelakunya sendiri. Perbaikannya:

- ruas diganti `ExaminationId`;
- disiplin, id pemeriksaan, dan kode pengecualian atau alasan ditulis di teks pesan — tanpa catatan dan
  tanpa nilai hasil (13.6).

Harness membuktikan log kini mencatat dokter dan perilis sebagai pelaku.

**Keempat jebakan roadmap.**

| Jebakan | Bagaimana dihindari | Bukti |
| --- | --- | --- |
| (a) Membuka penjaga disiplin tanpa kode per disiplin | Kedua perubahan dalam **satu** task; `AC-241` diuji **pertama** | Pemegang `LAB-VAL-PK` → `403` *Mikrobiologi*; sebaliknya pemegang `LAB-VAL-MB` → `403` *Patologi Klinik* |
| (b) Kode dari jabatan pelaku | Kode dari `EnsureReleasableAndRunning` — disiplin order, lalu katalog pemeriksaan | Ketiga tindakan; `ValidatedByPrivilegeId`/`ReleasedByPrivilegeId` = penunjukan Mikrobiologi |
| (c) `VAL-144` sesudah `VAL-128` | `EnsureNotPreliminary` sebelum keadaan hasil dan kewenangan | Pelaku tanpa kode Mikrobiologi membaca `VAL-144` (`422`), bukan `403`; Sementara belum Final membaca `VAL-144`; Sementara gugur membaca `VAL-127` |
| (d) Kualifikasi kosong = Sementara | Hanya `Preliminary` yang ditolak | Kosong → `200` (harness); kedua BTA dev berkualifikasi kosong lolos `VAL-144` (HTTP) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Sesuai `r35` 30.2. **`VAL-126` berubah bunyi** — aturan approved `r12` yang diubah `r13` atas persetujuan pemilik modul (21.10 butir 5). Nol konsumen hari ini |
| Database | **Nol perubahan skema.** Nol baris ditulis selama verifikasi |
| Keamanan/Auth | Kewenangan orang kini per disiplin pemeriksaan. Kode Mikrobiologi **usulan** — sampai `LAB-COORD-016`, resolver menolak setiap tindakan Mikrobiologi (`NotAppointed`); deploy **tidak membuka** pemakaian |
| Dampak ke `MVP-11` | `LabReleasableDisciplines` kini memuat Mikrobiologi, sehingga laporan jumlah pemeriksaan dan waktu penyelesaian menghitung Mikrobiologi (`isCountable = true`) — sesuai rancangan 23.4 *"Sesudah BE-LAB-78: Patologi Klinik dan Mikrobiologi"* |

---

## 4. Dokumentasi endpoint

Nol endpoint baru. Tiga endpoint lama **kini menerima Mikrobiologi**:

| Method | Path | Hak akses | Perubahan |
| --- | --- | --- | --- |
| `POST` | `/lab-examinations/{id}/result/validate` | `LabExaminationResult : Validate` | Mikrobiologi dengan `LAB-VAL-MB`; Sementara `422` `VAL-144` |
| `POST` | `/lab-examinations/{id}/result/release` | `LabExaminationResult : Release` | Mikrobiologi dengan `LAB-REL-MB`; satu dokumen rekam medis per pemeriksaan; Sementara `422` |
| `POST` | `/lab-examinations/{id}/result/return` | `LabExaminationResult : Return` | Mikrobiologi dengan kode validasi **atau** rilis Mikrobiologi |

Kode status tambahan `r35` 30.2: `422` `VAL-144`; `422` `VAL-126` bunyi baru; `403` berkata *Mikrobiologi*.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build … -p:RunAnalyzers=False` | **0 error, 230 warning**; nol dari berkas yang disentuh | `PASS` | Keluaran build |
| Tinjauan kode — literal `LAB-VAL-`/`LAB-REL-` | **Hanya** di `LabClinicalPrivilegeCodes.cs` (empat konstanta) | `PASS` | `grep` |
| Harness EF InMemory `BE-LAB-78` | **29 `PASS`, 0 `FAIL`** | `PASS` | Rincian di bawah |
| Regresi | `BE-LAB-69` 31/31, `72` 43/43, `76` 16/16, `77` 21/21, `81` 25/25, `84` 15/15, `86` 19/19 utuh | `PASS` | Harness yang sama |
| **Regresi yang berubah — disengaja** | Tepat **enam** baris lama gagal, semuanya perilaku yang diganti kontrak: `BE-LAB-73`/`74`/`75` — *"Mikrobiologi → 422 VAL-126"* kini `403` lapis orang (21.7); `BE-LAB-82` unit — *"Mikrobiologi belum dapat dirilis"*; `BE-LAB-83`/`85` — *"Mikrobiologi belum dapat dihitung"* (23.4). Harness karakterisasi `BE-LAB-82` (berkas tak disunting): **hanya** 18 baris `VAL-126` berubah — bunyi PA baru, Mikrobiologi lolos ke aturan berikutnya — dan ke-17 baris daftar pantau cito **identik** | `PASS` — perubahan yang disetujui | `diff` |
| Startup | Registri tetap **1577**; deskripsi `Validate`/`Release` di `SysActionAccess` ikut diperbarui sinkronisasi registri; nol galat tak dikenal | `PASS` | Log; kueri baca-saja |
| **HTTP terhadap PostgreSQL** | Rincian di bawah | `PASS` | Panggilan sungguhan |

**Rincian HTTP** — superadmin (lolos lapis jabatan, **tidak** punya data tenaga kerja sehingga lapis orang
menolaknya). Status, `Version`, riwayat tindakan, dan dokumen rekam medis **identik** sebelum dan sesudah.

| Panggilan | Hasil |
| --- | --- |
| Validasi, rilis, *Kembalikan* atas Histopatologi Biopsi Besar (PA, LAB-RSMMC-000009) | `422` ×3 — *"Validasi dan rilis hasil Patologi Anatomi belum tersedia."* |
| Validasi Pewarnaan BTA Sputum (Mikrobiologi, Final, **kualifikasi kosong**) | `403` — *"Akun Anda belum terhubung dengan data tenaga kerja, sehingga kewenangan validasi tidak dapat diperiksa…"* — lolos `VAL-126` **dan** `VAL-144` sampai lapis orang (`ARCH-GAP-LAB-10` pada data sungguhan) |
| Rilis dan *Kembalikan* atas BTA yang sama | `422` — *"Hasil ini belum divalidasi…"* (`VAL-133`, `VAL-134`) |
| Validasi BTA kedua (belum Final) | `422` `VAL-124` |
| Regresi Kalium PK (belum Final) | `422` `VAL-124` |

**Rincian harness.**

| Skenario | Hasil sebenarnya |
| --- | --- |
| **`AC-241`** — pemegang `LAB-VAL-PK` saja memvalidasi kultur urin | `403` *"Anda belum ditunjuk sebagai pemegang kewenangan validasi Mikrobiologi."*; nol kolom berubah |
| **`AC-241`** — dr. Nabila (`LAB-VAL-MB`) | `200`; `ValidatedByPrivilegeId` = penunjukan Mikrobiologinya |
| Sebaliknya — pemegang `LAB-VAL-MB` memvalidasi Kalium | `403` berkata *Patologi Klinik* |
| Regresi PK — pemegang `LAB-VAL-PK` memvalidasi Kalium | `200` |
| Rilis oleh pemegang `LAB-REL-PK` saja | `403` berkata *rilis Mikrobiologi* |
| Rilis oleh pemegang `LAB-REL-MB` | `200`; `isReleased`; `deliveryBlockedReason` kosong; `ReleasedByPrivilegeId` = penunjukan rilis Mikrobiologi |
| **`INT-08` Mikrobiologi** | **Tepat satu** `MrcClinicalDocumentIntegrity` `LaboratoryResult` untuk pemeriksaan; **nol** untuk isolatnya |
| **`VAL-144`** — validasi Sementara | `422` kata per kata; `Version` tetap, nol riwayat |
| Urutan — Sementara oleh pelaku tanpa kode; Sementara belum Final; Sementara gugur | `VAL-144`; `VAL-144`; `VAL-127` |
| `VAL-144` pada rilis | `422` |
| *Kembalikan* hasil Sementara tervalidasi | `200` — tanpa `VAL-144` |
| **`VAL-144` jalur Reopen** — Reopen → kualifikasi Definitif → Final ulang → validasi | `200` ×3, lalu `200` |
| **`ARCH-GAP-LAB-10`** — kualifikasi kosong | `200` |
| *Kembalikan* oleh pemegang kode PK; oleh pemegang kode rilis Mikrobiologi | `403` *"…validasi atau rilis Mikrobiologi."*; `200` (`VAL-138`) |
| **`INV-53`** — simpan hasil Mikrobiologi sesudah rilis; sesudah validasi | `409` `VAL-120` keduanya; isolat tidak berubah |
| **`INV-53` pengembalian** — sesudah *Kembalikan* | `FinalizedAt` kosong; simpan **lolos** `VAL-120` (harness tidak menyuntikkan service antibiogram, sehingga berhenti sesudah penjaga); satu baris riwayat pengembalian |
| **`VAL-126` bunyi baru** — tiga tindakan atas PA | `422` ×3 kata per kata |
| `For(Patologi Anatomi, *)`; kode usulan; `LabReleasableDisciplines` | `null`/`null`; `LAB-VAL-MB`, `LAB-REL-MB`; PK dan Mikrobiologi |
| **Log** — validasi dan rilis Mikrobiologi | `UserId` = dr. Nabila / perilis — bukan id pemeriksaan; pesan memuat disiplin dan id pemeriksaan |

**Tidak dijalankan:**

- **Jalur berhasil lewat HTTP** — butuh (1) `LAB-VAL-MB`/`LAB-REL-MB` di katalog Human Resource
  (`LAB-COORD-016`), (2) penunjukan dr. Nabila dan perilis, (3) kebijakan `Validate`/`Release`/`Return`
  bagi jabatannya (`UNK-P14-03`), dan (4) hasil Mikrobiologi Final di dev. Seluruhnya menulis ke data
  bersama dan milik langkah rilis `MVP-10c`.
- `AC-241` dengan tiga akun samaran lewat HTTP — alasan yang sama.
- Analyzer — build memakai `-p:RunAnalyzers=False`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-241` | ✅ **Terpenuhi** pada harness, diuji pertama; **lewat HTTP asli 2026-10-06** — dr. Bima (kode PK) `403`, dr. Nabila (kode Mikrobiologi) `200` | Bagian 8.1 |
| `AC-218` — pesan menyebut disiplin | ✅ **Terpenuhi** — *Mikrobiologi* / *Patologi Klinik* pada penolakan | — |
| `VAL-126` bunyi baru | ✅ **Terpenuhi** — harness dan HTTP | — |
| `VAL-144` beserta jalur Reopen | ✅ **Terpenuhi** pada harness | — |
| `ARCH-GAP-LAB-10` | ✅ **Terpenuhi** — harness `200`; HTTP: kosong lolos sampai lapis orang | — |
| Kedua baris `INV-53` | ✅ **Terpenuhi** pada harness | Tanpa kode baru, sesuai rancangan |
| `INT-08` Mikrobiologi | ✅ **Terpenuhi** pada harness | — |
| Baris amandemen `S4` bagi Mikrobiologi — empat mata, dua lapis, konkurensi | ✅ **Terpenuhi** (2026-10-06) — empat mata (`VAL-129`, `VAL-131`, `VAL-132`, `AC-02`) dan konkurensi diuji khusus Mikrobiologi pada harness; jalur pengecualian rilis juga teramati lewat HTTP asli. *Semula ⚠ Sebagian — empat mata dan konkurensi tidak diuji ulang khusus Mikrobiologi* | Bagian 8.2 |
| Verifikasi roadmap — `grep` literal kode | ✅ **Terpenuhi** | — |
| DoD — tiga tindakan dengan kode Mikrobiologi; `VAL-126` baru dan `VAL-144`; nol migration; nol string hak akses baru; laporan | ✅ **Terpenuhi** | — |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 230 warning compiler, **nol** dari berkas yang disentuh |
| Masalah yang diketahui | **1.** Nilai `LAB-VAL-MB`/`LAB-REL-MB` masih usulan — `LAB-COORD-016`; bila berubah, hanya konstanta yang disunting. **2.** Cacat log `BE-LAB-73`..`75` (pelaku tertimpa id pemeriksaan) diperbaiki di sini; log yang sudah terlanjur tertulis sebelumnya tidak dapat dikoreksi — jejak `LabTransitionHistory` tetap benar. **3.** Enam baris harness lama kini sengaja gagal (5); disimpan apa adanya sebagai bukti perubahan perilaku |
| Risiko tersisa | **Sedang.** Kewenangan per disiplin terbukti pada harness; di dev tertutup secara bawaan (nol kode, nol penunjukan) |
| Perubahan sampingan | Deskripsi aksi `Validate`/`Release` di registri — diperbarui otomatis saat startup; teks saja |
| Interupsi | `NONE` |
| Status Git | ` M` `Constants/LabClinicalPrivilegeCodes.cs`, `Constants/LabReleasableDisciplines.cs`, `Services/LabResultValidationService.cs`, `Services/LabExaminationService.cs`, `Controllers/LabExaminationController.cs`, `roadmap/backend-roadmap.md`, `roadmap/traceability.md`; `??` laporan ini. Perubahan `BE-LAB-84`..`86` yang belum ter-commit ikut ada. **Nol operasi Git dijalankan** |
| Langkah berikutnya | **1.** `BE-LAB-79` (pengesah pada respons hasil Mikrobiologi dan label order Mikrobiologi) dan `BE-LAB-80` (antrean dua disiplin) naik menjadi `SIAP DIKERJAKAN` — boleh sejajar. **2.** Langkah rilis `MVP-10c` tetap `BLOCKED` (6al.5) |

## 8. Verifikasi lanjutan 2026-10-06 — HTTP asli dan baris `S4` Mikrobiologi

**Status: `BE-LAB-78` ✅ `SELESAI`.** Kedua batas lama tertutup. Jalur berhasil lewat HTTP kini teramati dengan akun asli
sesudah setup langkah rilis di devYoga. Baris amandemen `S4` (empat mata dan konkurensi) diuji khusus Mikrobiologi. Nol
perubahan kode.

### 8.1 HTTP asli — backend lokal terhadap devYoga

Setup uji langkah rilis `MVP-10c` dijalankan atas persetujuan pemilik modul ([`backend-roadmap.md`](../../../roadmap/backend-roadmap.md)
6al.5): kode `LAB-VAL-MB`/`LAB-REL-MB` di katalog Human Resource, kredensial dr. Nabila, dan izin jabatan. Panggilan dikirim
Halaman Hasil Mikrobiologi (`FE-LAB-41`) ke backend lokal. Tulis hanya pada BTA `LAB-RSMMC-000014` (`025be4cf…`).

| Panggilan | Pelaku | Hasil |
| --- | --- | --- |
| `POST …/result/validate` | dr. Bima — `LAB-VAL-PK` saja | `403` *"Anda belum ditunjuk sebagai pemegang kewenangan validasi Mikrobiologi."* — `AC-241` sisi tolak, `AC-218` |
| `POST …/result/validate` | dr. Nabila — `LAB-VAL-MB`, bukan pengisi | `200` *"Hasil divalidasi. Hasil ini belum dirilis."*; pengesah dan jabatannya terbaca di Halaman Hasil — `AC-241` sisi terima |
| `POST …/result/return` (alasan *Sampel tertukar*) | dr. Nabila | `200` *"Hasil dikembalikan kepada analis dan kembali menjadi Draft."* |
| `PUT …/result/microbiology` lalu `POST …/result/finalize` | Vina (analis) | `200`/`200` — sesudah pengembalian, penjaga `VAL-120` mengizinkan simpan (`INV-53` pengembalian) |
| `POST …/result/validate` | dr. Nabila | `200` |
| `POST …/result/release` (alasan pengecualian *Shift tunggal…*) | dr. Nabila — pemvalidasi sendiri, `LAB-REL-MB` | `200` *"Hasil dirilis dan tercatat pada rekam medis pasien."*; penanda pengecualian rilis pada respons |
| Rekam medis | — | **Tepat satu** `MrcClinicalDocumentIntegrity` (`DocumentKind` 15) bagi BTA, ditandatangani dan dikunci saat rilis — `INT-08` |
| Sesudah rilis | — | BTA keluar dari kedua tahap antrean validasi (`BE-LAB-80`) |

Rincian layar ada di [`FE-LAB-41.md`](../frontend/FE-LAB-41.md) bagian 10 (12/12) dan [`FE-LAB-39.md`](../frontend/FE-LAB-39.md)
bagian 9 (M1). Jalur Patologi Klinik pada setup yang sama juga lulus: validasi, pengembalian, dan rilis Hemoglobin
`LAB-RSMMC-000001` oleh dr. Bima.

### 8.2 Harness — baris `S4` khusus Mikrobiologi

Salinan harness 2026-09-30 dijalankan terhadap kode sekarang (sudah memuat `BE-LAB-79`..`88`) dengan delapan skenario baru.
Hasilnya **37 `PASS`, 0 `FAIL`**; ke-29 skenario lama tetap utuh.

| Skenario baru | Hasil sebenarnya |
| --- | --- |
| `VAL-129` — pengisi (pemegang `LAB-VAL-MB`) memvalidasi tanpa alasan | `422` *"Anda yang mengisi hasil ini. Validasi oleh orang yang sama memerlukan alasan pengecualian."*; `Version` tetap, nol riwayat |
| `VAL-129` dengan alasan aktif | `200`; alasan tersimpan; penanda *"Divalidasi oleh pengisi sendiri — dr. Tunggal Mikro — Shift tunggal, tidak ada dokter lain bertugas"* |
| `VAL-131` — pemvalidasi merilis tanpa alasan | `422` *"Anda yang memvalidasi hasil ini. Rilis oleh orang yang sama memerlukan alasan pengecualian."*; nol dokumen rekam medis |
| `AC-02` — pemvalidasi merilis dengan alasan aktif | `200`; penanda *"Dirilis oleh pemvalidasi sendiri — …"* (20.10 butir 5); `ReleasedByPrivilegeId` = penunjukan rilis Mikrobiologi; satu dokumen |
| `VAL-132` — alasan dikirim padahal tidak merangkap | `422` |
| Konkurensi — Reopen lebih dulu, Validasi kalah | `200`/`409` *"Hasil ini baru saja diubah orang lain. Muat ulang lalu ulangi."*; nol `ValidatedAt` |
| Konkurensi — dua validasi bersamaan | `200`/`409`; baris milik yang menang |
| Konkurensi — dua rilis bersamaan | `200`/`409`; baris milik yang menang; tetap **satu** dokumen |

### 8.3 Yang tetap di luar task ini

- Analyzer tidak dijalankan (`-p:RunAnalyzers=False`).
- `VAL-144` dan `INV-53` terbukti pada harness. Dev tidak punya hasil Mikrobiologi `Sementara` untuk mengulanginya lewat
  HTTP. Keduanya tidak pernah menjadi batas status.
- `LAB-COORD-016` tetap terbuka sebagai koordinasi rilis. Katalog Human Resource tidak menerima kode manual lewat API (kode
  dibangkitkan `CPC-RSMMC-#####`), sehingga di dev keempat kode disisipkan lewat SQL. Keputusannya milik langkah rilis
  `MVP-10c` (6al.5), bukan kode task ini.
- `BE-LAB-73`..`75` tetap ⚠ pada laporannya sendiri. Bukti jalur Patologi Klinik di atas dapat dipakai untuk menaikkannya.

**Jejak di devYoga:** BTA `025be4cf…` **Dirilis** (data uji). **Nol perubahan kode. Nol operasi Git dijalankan.**
