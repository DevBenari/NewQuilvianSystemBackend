# Laporan Perubahan Frontend — `FE-LAB-42`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-42` |
| Judul | Penyaring disiplin pada antrean validasi |
| Slice | Gelombang `MVP-10b` — `EPIC-LAB-16` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) gelombang `MVP-10`, bagian `FE-LAB-42` |
| Trace | `FR-16.6`; `LAB-DEC-135` butir 2; `02-backend-architecture.md` 21.10 butir 3 dan 4; `AC-196` (bagian antarmuka Mikrobiologi); baris *Antrean dua disiplin* matriks uji |
| Contract version | `LAB-API-v1` `r35` 30.4; `LAB-VAL-v1` `r13` `VAL-145` — `approved` 2026-09-25 |
| Wewenang UI | Diputuskan roadmap: *keduanya* dikirim sebagai `discipline` kosong (satu permintaan); disiplin baris tampil bila keduanya dipilih; baris Mikrobiologi membuka Halaman Hasil Mikrobiologi; nol tombol tindakan; `resultQualifier` tidak dipakai menyaring. `DEV_DISCRETION` yang dipakai: wujud penyaring — `FilterSelect` *Disiplin* di `DataFilter`, bawaan *Semua Disiplin*; disiplin ditulis **di bawah nama pemeriksaan** (*Pemeriksaan / Disiplin*), bukan kolom tersendiri (6.2) |
| Dependency | `FE-LAB-40` ⚠, `FE-LAB-41` ⚠; `BE-LAB-80` ✅ |
| Klasifikasi | `LIGHT` — 5 berkas source diubah, 1 berkas uji diubah, 1 berkas uji baru; nol route, nol endpoint, nol perubahan backend |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`; `NewQuilvianSystemBackend` hanya laporan ini, roadmap frontend, dan `traceability.md` |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `a1404291d` (branch `YogaV2`; merge `origin/QuilvianIntegrationFrontend` di atas `ba4a90f3c`). Perubahan task ini belum ter-commit |
| Commit backend yang dijadikan rujukan | `6924b689` (branch `yoga`); biner lokal 2026-10-02 14.25, lebih baru dari commit kode Laboratorium terakhir |
| Tanggal | 2026-10-06 |
| Status | ✅ **`SELESAI`** (naik 2026-10-06) — antrean dua disiplin dengan data asli; validasi sungguhan memindahkan baris ke *Menunggu Rilis*. Lihat 9. *(Semula: ⚠ — penyaring hidup terhadap backend asli (*Semua Disiplin* memuat BTA Mikrobiologi asli; *Mikrobiologi* dan *Patologi Klinik* tersaring benar; `422` tidak pernah dipicu); baris membuka halaman disiplinnya; nol tombol tindakan. Uji unit 14 baru, lint 0 error, build hijau, layar **12/12** + nol tulis. **Batas:** dev nol punya hasil Patologi Klinik Final dan nol akun yang dapat memvalidasi Mikrobiologi (`LAB-COORD-016`, sama dengan `FE-LAB-41`) — antrean campuran dan perpindahan tahap sesudah validasi dibuktikan dengan jawaban antrean yang disuapkan (lihat 6))* |

---

## 1. Keadaan yang ditemukan di awal

| Yang ditemukan | Bukti |
| --- | --- |
| Antrean selalu mengirim `discipline=ClinicalPathology` lewat konstanta tetap — baris Mikrobiologi tidak pernah tampil walau backend `r35` sudah melayaninya | `LAB_VALIDATION_QUEUE_DISCIPLINE` pada `lab-worklist-constants.jsx`; `buildValidationQueueParams` |
| Baris tanpa `discipline` dibuka sebagai Patologi Klinik — aman selama antrean PK saja, tetapi menjadi **jebakan** begitu dua disiplin tercampur | `resolveQueueResultRoute` jatuh ke `LAB_VALIDATION_QUEUE_DISCIPLINE` |
| Backend `BE-LAB-80`: `discipline` kosong = PK **dan** Mikrobiologi; `AnatomicalPathology`, angka, atau nilai lain → `422` `VAL-145`; hasil `Sementara` tidak masuk; item membawa `discipline` dan `resultQualifier`; baris selalu berdisiplin PK atau Mikrobiologi | `LabWorklistService.ResolveQueueDisciplines`; `LabValidationQueueItemResponse` |
| Antrean asli dev 2026-10-06: *keduanya* → 1 baris (BTA `025be4cf…`, `LAB-RSMMC-000014`, Final); PK → 0; Mikrobiologi → 1; PA → `422`; tahap rilis → 0 di semua disiplin. BTA kedua order itu (Draft) tidak masuk | HTTP superadmin, sebelum layar disentuh |

---

## 2. Proses bisnis dari sisi pengguna

1. Pemvalidasi membuka **Antrean Validasi**. Bawaannya **Semua Disiplin** — hasil Patologi Klinik dan
   Mikrobiologi yang menunggu tampil bersama; tiap baris menyebut disiplinnya di bawah nama pemeriksaan.
2. Pemvalidasi Mikrobiologi memilih **Mikrobiologi** pada penyaring *Disiplin*: hanya hasil Mikrobiologi
   yang tampil, tanpa tercampur Patologi Klinik. Pilihan itu bertahan saat berpindah tab tahap, mencari,
   atau kembali dari halaman hasil.
3. *Buka Hasil* membuka **Halaman Hasil disiplin baris itu** — Mikrobiologi ke Halaman Hasil Mikrobiologi,
   Patologi Klinik ke Halaman Hasil per order. Validasi, Rilis, dan Kembalikan tetap hanya di sana.
4. Kembali ke antrean memuat ulang barisnya; hasil yang baru divalidasi sudah pindah ke *Menunggu Rilis*.
5. *Atur ulang filter* mengembalikan *Semua Disiplin* tanpa memindahkan tab tahap.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Keperluan |
| --- | --- |
| `use-lab-validation-queue.jsx`, `lab-validation-queue-view.jsx`, `lab-worklist-table-columns.jsx` | Layar `FE-LAB-40` yang diperluas |
| `lab-worklist-rules.js`, `lab-worklist-constants.jsx`, `lab-worklist-slice.jsx` | Parameter, route, penyaring bawaan, dan reset |
| `lab-catalog-constants.jsx` | `LAB_DISCIPLINE_LABEL` dan pola label *Semua Disiplin* |
| `Services/LabWorklistService.cs`, `DTOs/LabWorklistDtos.cs` (backend) | Bukti perilaku `discipline` dan `VAL-145` |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/.../lab-worklist-constants.jsx` | `LAB_VALIDATION_QUEUE_DISCIPLINE` (tetap PK) **diganti** `LAB_VALIDATION_QUEUE_DISCIPLINES` dan `LAB_VALIDATION_QUEUE_DISCIPLINE_OPTIONS` (*Semua Disiplin*, Patologi Klinik, Mikrobiologi — nol Patologi Anatomi); `discipline: ""` pada penyaring bawaan; salinan teks tidak lagi menyebut PK saja dan menyebut hasil `Sementara` tidak masuk |
| `src/lib/hooks/.../lab-worklist-rules.js` | `normalizeValidationQueueDiscipline` (nilai di luar PK/Mikrobiologi → *keduanya*, tidak pernah terkirim); `isBothQueueDisciplines`; `buildValidationQueueParams` mengirim `discipline` hanya bila satu disiplin; `resolveQueueResultRoute(row, token, filterDiscipline)` — disiplin baris, lalu penyaring satu disiplin, **tidak pernah** jatuh ke PK |
| `src/lib/hooks/.../use-lab-validation-queue.jsx` | `discipline`, `disciplineOptions`, `setDiscipline`, `showDisciplineColumn`; *Buka Hasil* membawa disiplin penyaring |
| `src/components/view/.../lab-validation-queue-view.jsx` | `FilterSelect` *Disiplin* pertama di `DataFilter`; kolom mengikuti penyaring |
| `src/components/view/.../lab-worklist-table-columns.jsx` | Pada *keduanya*, kolom *Pemeriksaan* menjadi *Pemeriksaan / Disiplin*; tombol *Buka Hasil* memakai disiplin penyaring sebagai cadangan |
| `tests/unit/lab-validation-queue-rules.test.mjs` | Tiga uji `FE-LAB-40` yang perilakunya **digantikan** task ini disesuaikan: bawaan tanpa `discipline`; uji "disiplin SELALU PK" dicabut; baris tanpa disiplin tidak lagi jatuh ke PK |
| `tests/unit/lab-validation-queue-fe42-rules.test.mjs` | **Baru** — 14 uji |

### 3.3 Kepatuhan arsitektur dan keputusan kecil

| Hal | Keputusan |
| --- | --- |
| Pola | `FilterSelect` di `DataFilter` dan penyaring di Redux — sama dengan Daftar Kerja; nol state, service, atau komponen paralel |
| Bawaan *Semua Disiplin* | Arti `discipline` kosong bagi backend (`r35` 30.4); pemvalidasi satu disiplin memilihnya sekali dan pilihan bertahan di Redux |
| Label *Semua Disiplin* | Sama dengan Daftar Kerja dan Laporan Operasional. *"Patologi Klinik & Mikrobiologi"* sempat dipakai, tetapi **terpotong** pada lebar penyaring (tangkapan 1440 px); deskripsi halaman sudah menyebut kedua disiplin |
| Nilai di luar PK/Mikrobiologi | Jatuh ke *keduanya*, tidak diteruskan — mengirimnya hanya untuk menerima `422` `VAL-145` membuat petugas melihat galat atas sesuatu yang tidak ia pilih (pola `normalizeValidationQueueStage`) |
| `resultQualifier` | **Tidak** disaring di layar — backend sudah mengeluarkan hasil `Sementara`; menyaring ulang akan menutupi bila backend keliru |
| Urutan baris | Tetap milik backend (`sortLatestFirst={false}`, `LAB-FE-006`) |

---

## 4. State yang ditangani di layar

| State | Perilaku |
| --- | --- |
| Memuat | Penyaring *Disiplin* nonaktif selama memuat, seperti penyaring lain |
| Kosong | Kalimat kosong tidak lagi menyebut PK saja; menyebut Draft dan `Sementara` belum masuk |
| Galat / `403` / `500` | Tidak berubah dari `FE-LAB-40` |
| Baris tanpa disiplin | *Semua Disiplin*: tanpa tombol *Buka Hasil* (bukan tautan ke halaman yang mungkin salah); satu disiplin: memakai disiplin penyaring |
| Layar sempit | 390 px tanpa gulir halaman |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Laboratory Management / Lab Worklist

| Method | Path | Parameter yang dikirim | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/lab-worklists/validation-queue` | `stage` (selalu), `discipline` (`ClinicalPathology`/`Microbiology`, **tidak dikirim** pada *Semua Disiplin*), `search`, `onlyCito`, `pageNumber`, `pageSize` | `LabWorklist : Read` — tidak berubah |

Nol endpoint baru. Delta kontrak: nol.

---

## 6. Verifikasi

### 6.1 Validasi

| Perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| Uji unit berkas antrean (`fe42` + `FE-LAB-40`) | **35/35** | `PASS` |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 2469 uji: **2461 lulus, 8 gagal** — 6 kegagalan baseline lama (Hemodialisa ×4, Bank Darah M0, petty cash) dan **2 baru dari merge `a1404291d`** (*Procedure Parity* Rawat Inap/Jalan, *Outpatient Prescription Parity*) — keduanya uji struktural modul Rawat Jalan yang membaca berkas yang **tidak** disentuh task ini. Nol uji Laboratorium gagal | `EXISTING / ENVIRONMENT ISSUE` |
| `npx eslint` berkas yang disentuh; `npm run lint:errors` | 0 masalah; 0 error | `PASS` |
| `npm run build` (server BE/FE dimatikan lebih dulu) | `✓ Compiled successfully`; route `validation-queue` terdaftar | `PASS` |

Sesudah build hanya dua komentar yang diselaraskan (view dan hook) — nol perubahan kode.

### 6.2 Layar — Playwright terhadap backend lokal dan PostgreSQL dev

Login lewat formulir sebagai **superadmin** (berkas sandi akun uji sudah dihapus sesudah verifikasi
2026-10-06; penyaring ini tidak bergantung pada izin baru). Seluruh tulis digagalkan. *Asli* = jawaban
backend apa adanya; *suap* = jawaban antrean disuapkan di peramban.

| ID | Mode | Skenario | Hasil | Bukti |
| --- | --- | --- | --- | --- |
| Q1 | Asli | Bawaan *Semua Disiplin*: **satu** permintaan tanpa `discipline`; baris BTA Mikrobiologi asli tampil dengan disiplinnya | `PASS` | `{pageNumber:1, pageSize:20, stage:AwaitingValidation}` |
| Q2 | Asli | `AC-196` Mikrobiologi: order `LAB-RSMMC-000014` punya dua BTA — hanya yang Final tampil, yang **Draft** tidak | `PASS` | 1 baris BTA |
| Q3 | Asli | Pilihan tepat tiga, nol Patologi Anatomi (`VAL-145` tidak dapat dipicu dari layar) | `PASS` | `["Semua Disiplin","Patologi Klinik","Mikrobiologi"]` |
| Q4 | Asli | *Mikrobiologi*: `discipline=Microbiology`, halaman 1; baris BTA tampil; disiplin baris tidak ditulis | `PASS` | — |
| Q5 | Asli | *Patologi Klinik*: `discipline=ClinicalPathology`; baris Mikrobiologi tidak tampil; keadaan kosong | `PASS` | — |
| Q6 | Asli | Gabungan: disiplin + pencarian terkirim bersama; pindah tab membawa disiplin; *Atur ulang* → *Semua Disiplin* tanpa `discipline`, tab tetap | `PASS` | — |
| Q7 | Asli | *Buka Hasil* baris Mikrobiologi → **Halaman Hasil Mikrobiologi** order itu, alamat bertoken; kembali → antrean dimuat ulang dengan *Mikrobiologi* tetap | `PASS` | `/lab-monitoring/microbiology/pewarnaan-bta-sputum-…` |
| Q8 | Suap | Campuran PK (`LAB-RSMMC-000001`, Hemoglobin) + Mikrobiologi asli: disiplin baris *Patologi Klinik* dan *Mikrobiologi*; baris PK → Halaman Hasil PK, baris Mikrobiologi → Halaman Hasil Mikrobiologi | `PASS` | `/clinical-pathology/hemoglobin-…`, `/microbiology/pewarnaan-bta-sputum-…` |
| Q9 | Suap | Kembali sesudah validasi (**disimulasikan** pada jawaban antrean): baris Mikrobiologi hilang dari *Menunggu Validasi* dan muncul di *Menunggu Rilis* beserta pemvalidasi; nol tombol Validasi/Rilis/Kembalikan | `PASS` | — |
| Q10 | Suap | 1440 px, tahap rilis dengan disiplin (kolom terbanyak): *Buka Hasil* utuh, tabel tanpa gulir menyamping | **`FAIL` → `PASS`** | Sebagai kolom tersendiri, *Disiplin* mendorong *Buka Hasil* ke `x=1461` pada layar 1440 (gulir tabel 77 px). Dipindah ke bawah nama pemeriksaan: `x=1384`, gulir 0 |
| Q11 | Suap | Baris tanpa disiplin: *Semua Disiplin* → **tanpa** tombol; *Mikrobiologi* → membuka Halaman Hasil Mikrobiologi | `PASS` | Tidak pernah jatuh ke PK |
| Q12 | Suap | 390 px tanpa gulir horizontal halaman | `PASS` | `375 ≤ 390` |
| Q13 | — | Nol tulis; nol galat runtime | `PASS` | — |

Layar **12/12** + Q13, sesudah satu perbaikan (Q10) dan penggantian label *keduanya* (3.3).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-196` bagian antarmuka bagi Mikrobiologi — hasil Draft tidak muncul | ✅ Terbukti dengan data asli | Q2 |
| Matriks *Antrean dua disiplin* — *keduanya* memuat dua disiplin | ✅ Satu permintaan tanpa `discipline` dengan data asli (Q1); campuran PK + Mikrobiologi dengan suapan (Q8) — dev nol punya hasil PK Final | Q1, Q8 |
| Matriks — *Mikrobiologi* hanya Mikrobiologi | ✅ Data asli | Q4, Q5 |
| Matriks — memvalidasi dari halaman lalu kembali → baris pindah ke *Menunggu Rilis* | ⚠ **Disimulasikan** — tidak satu akun pun dapat memvalidasi Mikrobiologi di dev (`LAB-COORD-016`) | Q9 |
| Unit test pemetaan penyaring ke `discipline` dan tujuan navigasi per disiplin | ✅ | 14 uji baru |
| DoD — penyaring berjalan; baris membuka halaman yang benar; nol tombol tindakan | ✅ | Q1–Q11 |
| DoD — `resultQualifier` tidak disaring di layar | ✅ Struktural — nol penyaringan di hook, view, maupun kolom | 3.3 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nol peringatan lint pada berkas yang disentuh |
| Masalah yang diketahui | Dua uji struktural Rawat Jalan gagal sejak merge `a1404291d` — di luar Laboratorium, dilaporkan, nol disentuh |
| Risiko tersisa | **Rendah.** Bawaan berubah dari *PK saja* menjadi *Semua Disiplin*: pemvalidasi PK kini melihat baris Mikrobiologi sampai memilih *Patologi Klinik*. Itu arti `r35`, dan tindakan validasi tetap dijaga backend per disiplin |
| Dependency backend | Nol perubahan. Validasi Mikrobiologi sungguhan menunggu kode kewenangan `LAB-COORD-016` di katalog Human Resource — sama dengan `FE-LAB-41` |
| Perubahan sampingan | `NONE` di repository. Lingkungan: `TaskStop` meninggalkan dua proses `next dev`; dihentikan dan port 3000/7184/5107 dipastikan bebas sebelum build |
| Interupsi | `NONE` |
| Status Git | Frontend: ` M` `lab-validation-queue-view.jsx`, `lab-worklist-table-columns.jsx`, `lab-worklist-constants.jsx`, `lab-worklist-rules.js`, `use-lab-validation-queue.jsx`, `tests/unit/lab-validation-queue-rules.test.mjs`; `??` `tests/unit/lab-validation-queue-fe42-rules.test.mjs`. Backend: laporan ini, `frontend-roadmap.md`, `traceability.md`. **Nol operasi Git dijalankan** |
| Langkah berikutnya | Naik ✅ sesudah satu hasil Mikrobiologi divalidasi sungguhan dari halaman lalu antrean diperiksa kembali — bersama `FE-LAB-40` dan `FE-LAB-41` |

## 9. Verifikasi nyata 2026-10-06 — sesudah setup langkah rilis di devYoga

**Status: `FE-LAB-42` ✅ `SELESAI`.** Perpindahan tahap sesudah validasi kini dibuktikan dengan data asli (Hemoglobin PK). Perpindahan baris Mikrobiologi memakai jalur layar yang sama — tidak bergantung disiplin — dan *(diperbarui 2026-10-06)* kini juga dibuktikan dengan data asli: BTA berpindah *Menunggu Validasi* → *Menunggu Rilis* → keluar, serta kembali ke antrean sesudah dikembalikan dan diselesaikan ulang ([`FE-LAB-41.md`](FE-LAB-41.md) bagian 10, N1/N4/N6/N8).

**Konteks.** Atas persetujuan pemilik modul, langkah rilis `MVP-9d`/`MVP-10c` dijalankan sebagai **setup uji di devYoga**
([`backend-roadmap.md`](../../../roadmap/backend-roadmap.md) 6ak.10, 6al.5): kode kewenangan `LAB-*` di katalog Human Resource,
kredensial dr. Bima (`LAB-VAL-PK`/`LAB-REL-PK`) dan dr. Nabila (`LAB-VAL-MB`/`LAB-REL-MB`), serta `Validate`/`Release`/`Return`,
`LabWorklist : Read`, dan hak baca kedua daftar alasan bagi jabatan *Kepala Instalasi Laboratorium* dan *Dokter Penanggung Jawab
Laboratorium*. Hemoglobin dan Leukosit `LAB-RSMMC-000001` dinyatakan selesai sebagai hasil uji.

**Temuan uji — langkah rilis 4 kurang dua izin baca.** Dokter berizin `Validate` tetap *Akses Ditolak* di Halaman Hasil: kedua
jabatan tidak memegang `LabOrder : Read` (`GET /lab-orders/{id}`) dan `LabExamination : Read` (`GET /lab-examinations/by-order/{id}/results`,
`/result/microbiology`). Keduanya ditambahkan atas persetujuan pemilik modul; langkah rilis 4 `MVP-9d`/`MVP-10c` wajib memuatnya.

**Akun asli:** dr. Bima Prasetya, Sp.PK (Kepala Instalasi) dan Vina (analis). Tulis yang diteruskan hanya tindakan pengesahan pada
Hemoglobin/Leukosit `LAB-RSMMC-000001` dan Validasi BTA `LAB-RSMMC-000014` (ditolak `403`).

| ID | Task | Skenario | Hasil |
| --- | --- | --- | --- |
| Q1 | 40, 42 | dr. Bima: antrean *Semua Disiplin* memuat Hemoglobin dan Leukosit (PK) serta BTA (Mikrobiologi) dari backend asli, satu permintaan tanpa `discipline` | `PASS` |
| Q2 | 42 | *Patologi Klinik* → hanya Hemoglobin dan Leukosit; `discipline=ClinicalPathology` | `PASS` |
| P1 | 39 | Halaman Hasil PK, dr. Bima (`Validate` tanpa `Update`): Hemoglobin *Menunggu Validasi*; hanya tombol Validasi | `PASS` |
| P2 | 39 | Validasi Hemoglobin → **`200`**; *Tervalidasi*, *"Validasi oleh: dr. Bima Prasetya, Sp.PK — Kepala Instalasi Laboratorium"*; Rilis dan Kembalikan ditawarkan | `PASS` |
| P3 | 39 | Leukosit: Validasi `200`, lalu *Kembalikan ke analis* — tanpa alasan nol permintaan; dengan *"Salah ketik hasil"* **`200`**; kembali *Draft* | `PASS` |
| Q3 | 40, 42 | Kembali ke antrean: Hemoglobin pindah ke *Menunggu Rilis* dengan *Divalidasi oleh dr. Bima*; Leukosit (dikembalikan) di kedua tahap tidak ada | `PASS` |
| P4 | 39 | Rilis oleh pemvalidasi sendiri: panel menyebut rekam medis dan meminta alasan pengecualian (*"Shift tunggal…"*) → **`200`**; *Dirilis*, *Otorisasi oleh* dr. Bima, penanda pengecualian sebagai teks; nol tombol | `PASS` |
| RM | 39 | Baris rekam medis: satu `MrcClinicalDocumentIntegrity` bagi Hemoglobin — ditandatangani dan dikunci atas nama dr. Bima saat rilis | `PASS` |
| Q4 | 40 | Sesudah dirilis Hemoglobin hilang dari *Menunggu Rilis* | `PASS` |
| M1 | 41 | dr. Bima (berizin `Validate`, **tanpa** kewenangan Mikrobiologi) memvalidasi BTA → **`403` lapis orang asli**: *"Anda belum ditunjuk sebagai pemegang kewenangan validasi Mikrobiologi."* tampil apa adanya | `PASS` |
| V1 | 39 | Vina (analis): Hemoglobin *Dirilis* nol tombol; Leukosit yang dikembalikan dapat diisi lagi, nol tombol pengesahan | `PASS` |
| Z1 | — | Nol tulis di luar tindakan yang diizinkan; nol galat runtime | `PASS` |

**Jejak di devYoga:** Hemoglobin `LAB-RSMMC-000001` **Dirilis** (validasi dan rilis oleh dr. Bima, berpenanda pengecualian empat
mata); Leukosit dikembalikan ke *Draft* dengan alasan *Salah ketik hasil*. Data uji.

**Nol perubahan kode** pada task ini. **Nol operasi Git dijalankan.**
