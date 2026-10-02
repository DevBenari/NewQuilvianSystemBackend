# Laporan Perubahan Backend — `BE-LAB-76`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-76` |
| Judul | Ruas baca: pengesah, keadaan hasil, label order |
| Slice | Gelombang `MVP-9b` — `EPIC-LAB-15`, jalur baca `S4` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian **6ak.7** |
| Trace | `FR-15.10`, `FR-15.14`; `LAB-DEC-008`, `LAB-DEC-080`, `LAB-DEC-120`, `LAB-DEC-135`; rancangan 22 langkah 3 (definisi pemeriksaan yang dihitung) |
| Contract version | `LAB-API-v1` **`r34`** 29.3 dan 29.5 — **`approved` 2026-09-25** |
| Dependency | `BE-LAB-75` ⚠ (ketiga tindakan berdiri), `BE-LAB-69` ✅ (jalur baca per order) |
| Klasifikasi | `MEDIUM` — skor 8: repository 0, berkas diperiksa 1 (±15), berkas diubah 2 (7), logika bisnis 1, kontrak API **2** (ruas baru pada tiga respons), database 1 (kueri baca), keamanan/auth 0, UI/workflow 1 |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/HealthServices/LaboratoryManagement/`, dokumen blueprint Laboratorium |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `017d1819` (branch `yoga`), di atas `BE-LAB-67`..`75` yang belum ter-commit |
| Tanggal | 2026-09-29 |
| Status | ✅ **`SELESAI`** — seluruh jalur baca terbukti **lewat HTTP terhadap PostgreSQL** tanpa menulis data; build 0 error tanpa warning baru; harness **16/16**, termasuk jumlah kueri tetap untuk 18 maupun 4 baris; regresi `BE-LAB-69` **31/31** (jumlah kueri lembar tetap 8), `BE-LAB-73` 38/38, `BE-LAB-74` 28/28, `BE-LAB-75` 20/20. `orderStatus` terbukti tak tersentuh |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `LaboratoryManagement` |
| Pemilik / prefix registry | `Lab` — `ACTIVE` |
| Keberlakuan | `NEW CODE` — `LabOrderResultProgressRules.cs`; `TOUCHED LEGACY` — `LabExaminationService` (penyusun lembar hasil, penyusun respons), `LabMonitoringService`, `LabOrderService.GetDetailAsync`, tiga berkas DTO |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-DTO-001` (ruas baru pada DTO, entity tidak diekspos), `QBE-ENT-003` (nol kolom presentasi disimpan — `resultStatus` dan `resultProgress` **turunan**). **Tidak berlaku:** `QBE-PERM-*` (nol endpoint dan aksi baru; hak baca yang sudah ada), `QBE-LOG-001` (nol perubahan state), `QBE-ENT-001`/`CFG-001` (nol entity) |
| Governance yang dibaca | Sama dengan `BE-LAB-73`..`75` |

---

## 1. Masalah yang diperbaiki

**Halaman hasil belum dapat menunjukkan siapa yang mengesahkan.** Ketiga tindakan `S4` (`BE-LAB-73`..`75`)
menulis pemvalidasi, perilis, jabatan mereka, dan alasan pengecualian. Namun lembar hasil per
order dan `GET /{id}/result` belum membacanya. Layar tidak tahu apakah sebuah Kalium sudah boleh
dikirim, dan tidak dapat menampilkan baris *Validasi oleh* maupun *Otorisasi oleh*.

**Daftar Pemeriksaan tidak dapat membedakan order yang hasilnya sudah keluar semua.** Satu-satunya
status order, `orderStatus`, punya arti lain: `Completed` berarti *ditandai selesai secara manual*.
Menyetelnya otomatis saat seluruh hasil dirilis adalah keputusan yang belum diambil
(`LAB-CONFLICT-014`). Yang dibutuhkan adalah label **turunan** yang tidak menyentuhnya.

---

## 2. Proses bisnis

**Lembar hasil (halaman hasil per order dan `GET /{id}/result`).** Setiap baris kini membawa:

| Ruas | Contoh — Kalium dirilis | Kalium Final |
| --- | --- | --- |
| `resultStatus` | `Released` | `Final` |
| `validatedByName` / `validatedByPositionName` | *dr. Pemvalidasi A* / *Dokter Penanggung Jawab Laboratorium* | kosong |
| `releasedByName` / `releasedByPositionName` | *dr. Perilis B* / *Perilis Laboratorium* | kosong |
| `deliveryBlockedReason` | **kosong** — boleh dikirim | *"Hasil ini belum dirilis, sehingga belum boleh dikirim kepada pasien."* |
| `resultEnteredByUserId` | Pengisi | Pengisi — layar memperingatkan pengisi **sebelum** ia menekan Validasi atas hasilnya sendiri |

Bila dokter tunggal memvalidasi dan merilis sendiri, kedua penanda tampil dengan bunyi yang **sama
persis** dengan respons tindakannya, sebab keduanya disusun satu fungsi:

> *Divalidasi oleh pengisi sendiri — dr. C — Shift tunggal, tidak ada dokter lain bertugas*
> *Dirilis oleh pemvalidasi sendiri — dr. C — Shift tunggal, tidak ada dokter lain bertugas*

**Label order `resultProgress`** — daftar Pemeriksaan Patologi Klinik dan detail order:

| Keadaan order | Label | Contoh |
| --- | --- | --- |
| Masih ada pemeriksaan yang dihitung dan belum dirilis | `InProgress` — *Dalam Pemeriksaan* | Kalium dirilis, Hemoglobin masih Draft (`AC-08`, `AC-198`) |
| Seluruh pemeriksaan yang dihitung sudah dirilis | `AllReleased` — *Selesai* | Kalium dan Hemoglobin dirilis, satu pemeriksaan lain **batal** (`AC-199`) |
| Tidak ada pemeriksaan yang dihitung, atau bukan Patologi Klinik | kosong | Order yang baru diminta; order Mikrobiologi |

**Pemeriksaan yang dihitung:** tidak terhapus, tidak **gugur**, dan tidak **batal** — definisi yang
sama persis dengan penjaga penyelesaian order pada rancangan 22 langkah 3. Pemeriksaan gugur tidak
pernah dapat dirilis; bila ikut dihitung, order yang wadahnya pernah ditolak tidak akan pernah
*Selesai*.

**Yang tidak berubah:** `orderStatus`. Order yang seluruh hasilnya dirilis tetap `InProcess` sampai
seseorang menandainya selesai. Menyelaraskan keduanya adalah pekerjaan `BE-LAB-81`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / dokumen | Kenapa dibaca |
| --- | --- |
| `roadmap/backend-roadmap.md` 6ak.7 | Cakupan, dua jebakan, DoD |
| `contracts/api-contract.md` 29.3, 29.5 | Ruas lembar hasil dan label order |
| `02-backend-architecture.md` 22 langkah 3 | Definisi pemeriksaan yang dihitung — mengecualikan `Voided` dan `Cancelled` |
| `testing/acceptance-test-matrix.md` | `AC-02`, `AC-08`, `AC-198`, `AC-199` |
| `Services/LabExaminationService.cs` — `GetResultFormAsync`, `GetResultSheetByOrderAsync`, `BuildResultFormsAsync` | Penyusun bersama `BE-LAB-69`, beserta janji jumlah kuerinya |
| `Services/LabMonitoringService.cs`, `Services/LabOrderService.cs` | Proyeksi daftar Pemeriksaan dan detail order |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Services/LabOrderResultProgressRules.cs` | **Baru.** Satu definisi *pemeriksaan yang dihitung* (`Counted`) dan `ReadAsync` — label untuk banyak order dalam **satu kueri berkelompok** |
| `Services/LabExaminationService.cs` | Proyeksi baris lembar hasil bertambah sebelas kolom pengesah. `BuildResultFormsAsync` membaca nama pemvalidasi dan perilis **sekali untuk seluruh baris**, dilewati bila tidak ada yang tervalidasi. Pembantu bersama: `ValidationExceptionMarker`, `ReleaseExceptionMarker`, `NotReleasedMessage`, dan `DeriveResultStatus` versi baris proyeksi — dipakai **juga** oleh `BuildCompletionResponse`, sehingga respons tindakan dan jalur baca tidak dapat berbeda bunyi |
| `Services/LabMonitoringService.cs` | `resultProgress` pada jalur Patologi Klinik — satu kueri tambahan per halaman |
| `Services/LabOrderService.cs` | `resultProgress` pada detail order Patologi Klinik |
| `DTOs/LabExaminationResultDtos.cs` | Lima belas ruas pada `LabExaminationResultFormResponse` |
| `DTOs/LabMonitoringDtos.cs`, `DTOs/LabOrderDtos.cs` | `ResultProgress` |

**Nol endpoint, nol entity, nol migration, nol kolom tersimpan.** Registri hak akses tetap 1575.

**Keputusan kecil.**

| Hal | Keputusan dan alasan |
| --- | --- |
| `resultProgress` pada jalur Mikrobiologi dan Patologi Anatomi | Kosong — ruasnya ada karena DTO dipakai ketiga jalur, tetapi rilis kedua disiplin itu belum dibangun |
| Order Patologi Klinik lama yang disiplinnya kosong | Label kosong — sejalan dengan daftar Pemeriksaan, yang hanya memuat order berdisiplin. Di dev tidak ada order seperti itu |
| Kueri nama pada lembar hasil | Dilewati bila tidak ada baris tervalidasi, sehingga lembar yang belum disahkan tetap 8 kueri seperti janji `BE-LAB-69` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | **Aditif, sesuai `r34` 29.3 dan 29.5.** Ruas baru pada `GET /lab-examinations/{id}/result`, `GET /lab-examinations/by-order/{id}/results`, `GET /lab-monitoring/clinical-pathology` (dan kosong pada dua jalur saudaranya), serta `GET /lab-orders/{id}`. Nol ruas lama berubah |
| Database | **Baca saja.** Satu kueri berkelompok untuk label order; satu kueri nama pelaku per lembar bila ada yang tervalidasi. **Nol kolom tersimpan; nol penulisan** |
| Keamanan/Auth | `NOT APPLICABLE` — hak baca yang sudah ada (`LabExamination : Read`, hak daftar Pemeriksaan dan order). Nama dan jabatan pengesah memang untuk ditampilkan; tidak ada data pasien baru |

---

## 4. Dokumentasi endpoint

Nol endpoint baru; empat endpoint lama bertambah ruas.

#### Health Services / Laboratory Management / Lab Examination

| Method | Path | Perubahan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/lab-examinations/{id}/result` | Bertambah `resultStatus`, `resultEnteredByUserId`, ruas validasi dan rilis, kedua penanda, `deliveryBlockedReason` | `LabExamination : Read` |
| `GET` | `/lab-examinations/by-order/{labOrderId}/results` | Sama, pada setiap baris | `LabExamination : Read` |

#### Health Services / Laboratory Management / Lab Monitoring

| Method | Path | Perubahan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/lab-monitoring/clinical-pathology` | Setiap item bertambah `resultProgress` | Tetap |

#### Health Services / Laboratory Management / Lab Order

| Method | Path | Perubahan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/lab-orders/{id}` | Detail bertambah `resultProgress` | Tetap |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build QuilvianSystemBackend.csproj -p:RunAnalyzers=False` | **0 error, 230 warning**; **nol** dari berkas yang disentuh; 81 detik | `PASS` | Keluaran build |
| Harness EF InMemory `BE-LAB-76` | **16 `PASS`, 0 `FAIL`** | `PASS` | Rincian di bawah |
| Regresi | `BE-LAB-69` **31/31** — lembar 18 baris tetap **8 kueri**; `BE-LAB-73` 38/38; `BE-LAB-74` 28/28; `BE-LAB-75` 20/20 | `PASS` | Harness yang sama dijalankan ulang |
| Startup Development | Registri tetap **1575** | `PASS` | Log startup |
| **HTTP terhadap PostgreSQL** — lima jalur baca | Seluruhnya `200`, ruas sesuai (rincian di bawah) | `PASS` | Panggilan sungguhan |
| `orderStatus` seluruh order dev sebelum dan sesudah | **Identik** | `PASS` | Kueri baca-saja |
| Tinjauan kode — penulisan `OrderStatus` atau `SaveChanges` pada perubahan task ini | **Nol** | `PASS` | `git diff` dan pencarian pola |

**Rincian HTTP** — aplikasi sungguhan, database dev bersama, akun superadmin, nol penulisan.

| Panggilan | Hasil |
| --- | --- |
| `GET /lab-examinations/by-order/{LAB-RSMMC-000001}/results` | `200`; lima baris — Hemoglobin dan Leukosit `Draft` dengan pengisi, tiga `NotEntered`; **kelima belas ruas baru hadir di setiap baris**; `deliveryBlockedReason` terisi karena belum dirilis |
| `GET /lab-examinations/{Hemoglobin}/result` | `200`; `resultStatus = Draft`, `isValidated = false`, alasan belum dikirim terisi |
| `GET /lab-monitoring/clinical-pathology` | `200`; tiga order berpemeriksaan → `InProgress`; tiga order tanpa pemeriksaan → kosong |
| `GET /lab-monitoring/microbiology` | `200`; `resultProgress` kosong pada kelima order |
| `GET /lab-orders/{PK}` dan `/{Mikrobiologi}` | `200`; `InProgress` dan kosong |

**Rincian harness.**

| Skenario | Hasil sebenarnya |
| --- | --- |
| Lembar 18 baris dengan campuran Dirilis, dokter tunggal, Final, Draft, Tervalidasi, belum diisi | 18 baris |
| **Jumlah kueri** — 18 baris lawan 4 baris berkomposisi sama | **5 = 5** — nama pelaku satu kueri, tidak per baris |
| Baris dirilis | `Released`, `isReleased`, `deliveryBlockedReason` kosong, nama dan jabatan pemvalidasi serta perilis |
| **`AC-02` bagian baca** — dokter tunggal | Kedua penanda berbunyi persis 20.10 butir 5 |
| Final / Draft / Tervalidasi / belum diisi | `resultStatus` sesuai; `resultEnteredByUserId` terisi pada Draft; ruas pengesah kosong pada yang belum disahkan |
| `GET /{id}/result` lawan barisnya di lembar | JSON **identik** |
| **`AC-08`/`AC-198`** — dirilis + Draft | `InProgress` pada daftar **dan** detail |
| **`AC-199`** — semua dirilis + satu batal | `AllReleased` pada daftar dan detail |
| Dirilis + satu **gugur** | `AllReleased` — gugur tidak menahan |
| Pemeriksaan batal semua; order tanpa pemeriksaan | Kosong |
| Jalur Mikrobiologi; detail order Mikrobiologi | Kosong |
| **`orderStatus`** sesudah seluruh pembacaan | Tetap `InProcess` pada order yang seluruh hasilnya dirilis |

Uji manual: **`NOT FEASIBLE`** — layar `FE-LAB-39` belum dibangun.

**Tidak dijalankan:**

- Label `AllReleased` dan ruas pengesah terisi **lewat HTTP** — dev nol punya hasil yang dirilis
  atau divalidasi (menunggu pembuktian `BE-LAB-73`..`75`). Keduanya terbukti pada harness.
- Log SQL PostgreSQL untuk jumlah kueri — dihitung pada tingkat EF, seperti `BE-LAB-69`.
- Analyzer — build memakai `-p:RunAnalyzers=False`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-08`, `AC-198` — Kalium dirilis, Hemoglobin Draft → Kalium `Released`, order `InProgress` | ✅ **Terpenuhi** pada harness; `InProgress` lewat HTTP | — |
| `AC-199` — semua dirilis, satu batal → `AllReleased`; nol kolom baru pada `LabOrder` | ✅ **Terpenuhi** | Harness; nol migration |
| `AC-02` bagian baca — penanda berbunyi persis | ✅ **Terpenuhi** pada harness | Satu penyusun bersama |
| `AC-239` bagian baca — jabatan pengesah saat itu | ✅ **Terpenuhi** | `validatedByPositionName`/`releasedByPositionName` dari snapshot |
| Verifikasi — nama pelaku satu kueri untuk 18 baris | ✅ **Terpenuhi** | 5 = 5 kueri |
| Verifikasi — satu batal dan sisanya dirilis → `AllReleased` | ✅ **Terpenuhi** | — |
| Verifikasi — `orderStatus` tidak berubah oleh rilis mana pun | ✅ **Terpenuhi** | Harness, dev, dan tinjauan kode |
| DoD — ruas terisi; `resultProgress` benar pada tiga keadaan; `orderStatus` tak tersentuh; laporan | ✅ **Terpenuhi** | — |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 230 warning compiler, **nol** dari berkas yang disentuh |
| Masalah yang diketahui | **1.** `resultProgress` kosong bagi order Patologi Klinik lama yang disiplinnya kosong — tidak ada di dev. **2.** `LAB-CONFLICT-014` tetap terbuka pada kode sampai `BE-LAB-81`: order dapat `AllReleased` sambil `orderStatus` masih `InProcess`, dan sebaliknya `Completed` manual masih dapat mendahului rilis |
| Risiko tersisa | **Rendah.** Seluruhnya jalur baca, terbukti terhadap PostgreSQL |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Berkas `BE-LAB-76`: `??` `Services/LabOrderResultProgressRules.cs`, laporan ini; ` M` `Services/LabExaminationService.cs`, `Services/LabMonitoringService.cs`, `Services/LabOrderService.cs`, `DTOs/LabExaminationResultDtos.cs`, `DTOs/LabMonitoringDtos.cs`, `DTOs/LabOrderDtos.cs`, `roadmap/backend-roadmap.md`, `roadmap/traceability.md`. Perubahan `BE-LAB-67`..`75` yang belum ter-commit ikut ada. **Nol operasi Git dijalankan** |
| Langkah berikutnya | **1.** `BE-LAB-77` (antrean validasi dan Daftar Kerja) kini `SIAP DIKERJAKAN` — `DeriveResultStatus` dan definisi pemeriksaan yang dihitung sudah tersedia. **2.** `BE-LAB-81` (penjaga penyelesaian order) sebaiknya memakai `LabOrderResultProgressRules.Counted` supaya label dan penjaganya satu definisi |
