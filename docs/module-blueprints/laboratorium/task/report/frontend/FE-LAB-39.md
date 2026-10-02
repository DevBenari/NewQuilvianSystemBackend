# Laporan Perubahan Frontend — `FE-LAB-39`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-39` |
| Judul | Validasi, rilis, dan pengembalian pada Halaman Hasil Patologi Klinik |
| Slice | Gelombang `MVP-9c` — `EPIC-LAB-15` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) gelombang `MVP-9`, bagian `FE-LAB-39` |
| Trace | `FR-15.1`..`FR-15.7`, `FR-15.10`, `FR-15.14`; `LAB-FE-004`, `LAB-DEC-003`, `LAB-DEC-120`, `LAB-DEC-138`, `LAB-DEC-156` |
| Contract version | `LAB-API-v1` `r34` 29.2, 29.3, 29.5, 29.6 `GET /options` (`approved` 2026-09-25); `LAB-VAL-v1` `r12` 14.1-14.2; `LAB-PERM-v1` rev 11 bagian 13.4 |
| Wewenang UI | Diputuskan: label lima keadaan (`LAB-DEC-156`) lewat **satu** konstanta bersama; pengesah dan penanda pengecualian sebagai teks (`LAB-FE-004`); tindakan hanya di halaman ini (`LAB-DEC-149`); tombol per izin. `DEV_DISCRETION` yang dipakai: pertanyaan alasan dan konfirmasi rilis sebagai **panel di bawah tabel** (pola Buka Kembali/konsultasi `FE-LAB-36`); Validasi tanpa merangkap terkirim **tanpa** konfirmasi; Rilis **selalu** berkonfirmasi; nama berkas konstanta `lab-result-status-constants.jsx`; warna lencana |
| Dependency | `FE-LAB-36` ✅; `BE-LAB-73`..`BE-LAB-75` ⚠, `BE-LAB-76` ✅ (berdiri pada kode, berjalan di backend lokal) |
| Klasifikasi | `STANDARD` — 6 berkas diubah, 2 berkas baru, 1 berkas uji diperluas; nol route baru, nol perubahan backend |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`; `NewQuilvianSystemBackend` hanya laporan ini, roadmap frontend, dan `traceability.md` |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `e613321c5` (branch `YogaV2`), di atas `FE-LAB-35`..`FE-LAB-38` yang belum ter-commit |
| Commit backend yang dijadikan rujukan | `55b032b0` + `BE-LAB-87` (belum ter-commit), `https://localhost:7184` di atas `QuilvianNewDevYoga` |
| Tanggal | 2026-10-01 |
| Status | ⚠ **`SELESAI DENGAN BATAS VERIFIKASI`** — ketiga tindakan berjalan dari halaman; penanda terbaca sebagai teks; nol tombol pengesahan di luar halaman ini. Uji unit 39/39 (berkas terdampak), lint nol peringatan, build hijau, layar 34/34 + 4/4 akun analis asli, nol tulis. **Batas:** dev belum punya dokter pemvalidasi/perilis yang ditunjuk dan tidak ada baris Final — lihat 6 |

---

## 1. Keadaan yang ditemukan di awal

| Yang ditemukan | Bukti |
| --- | --- |
| Halaman `FE-LAB-36` menampilkan keadaan **turunan `MVP-8`** dari `isFinalized` dan isi hasil, dan menawarkan Buka Kembali pada setiap baris Final | `lab-clinical-pathology-result-rules.js` `readRowStateKey`, `readRowActions` |
| Backend lokal sudah mengirim `resultStatus`, `resultEnteredByUserId`, ruas pengesah, kedua penanda, dan `resultProgress` | `LabExaminationResultFormResponse`, `LabOrderDetailResponse.ResultProgress` |
| `403` lapis jabatan dari filter izin selalu berbunyi generik *"Anda tidak memiliki akses ke menu atau fitur ini."*; `403` lapis orang membawa sebabnya | `AccessPermissionFilter.GenericDeniedMessage`; `LabResultValidationService` |
| Pesan `VAL-129`/`VAL-131` diawali *"Anda yang mengisi hasil ini."* / *"Anda yang memvalidasi hasil ini."* | `LabResultValidationService.ValidateAsync`/`ReleaseAsync` |
| Data dev: order PK hanya berbaris *Draft*/*Menunggu Hasil* — **nol** baris Final, Tervalidasi, atau Dirilis | `GET /by-order/{id}/results` keenam order PK |
| `id` pengguna sesi tersedia di `auth.userInfo.userId` dan sama dengan `GetCurrentUserId` backend | `selectUserInfo`; respons login superadmin |

---

## 2. Proses bisnis dari sisi pengguna

**Dokter pemvalidasi** membuka Halaman Hasil Patologi Klinik order itu (dari daftar pantau atau Daftar Kerja):

1. Baris yang dinyatakan selesai analis berlabel **Menunggu Validasi** dan bertombol **Validasi**.
2. Menekan **Validasi** pada hasil yang diisi orang lain → terkirim langsung; baris menjadi **Tervalidasi**
   dengan *"Validasi oleh: dr. … — Dokter Penanggung Jawab Laboratorium · waktu"*.
3. Bila ia sendiri yang mengisi hasil itu, panel di bawah tabel terbuka **sebelum** apa pun terkirim:
   *"Anda yang mengisi hasil ini…"*, pilihan **Alasan pengecualian** dari daftar aktif, dan catatan bila
   alasannya mewajibkan.

**Perilis** pada baris **Tervalidasi** menekan **Rilis** → panel konfirmasi *"Hasil yang dirilis menjadi
dokumen klinis pasien dan tercatat pada rekam medis…"*; bila ia juga pemvalidasinya, alasan pengecualian
diminta di panel yang sama. Sesudah rilis baris berlabel **Dirilis**, *"Otorisasi oleh: …"* tampil, dan
penanda pengecualian — bila ada — tampil sebagai kalimat utuh dari backend.

**Keduanya** dapat menekan ***Kembalikan ke analis*** pada baris Tervalidasi: alasan wajib dari daftar
alasan pengembalian; baris kembali **Draft**. *Keadaan Order* di kartu Informasi Order beralih *Dalam
Pemeriksaan* → *Selesai* hanya bila seluruh pemeriksaan sudah dirilis.

**Analis** tidak melihat ketiga tombol; Buka Kembali hanya ditawarkan pada baris Menunggu Validasi.

**Jalur tidak normal:** `403` lapis orang menuliskan sebabnya pada baris itu; `409` memuat ulang lembar;
`422` mempertahankan alasan yang sudah dipilih; `503` memberi tombol **Coba lagi**; daftar alasan kosong atau
tak terbaca disebut sebabnya.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / dokumen | Kenapa dibaca |
| --- | --- |
| `frontend-roadmap.md` `FE-LAB-39` dan *Keputusan 2026-09-25 malam*; `03-frontend-architecture.md` amandemen 2026-09-25 (pertama dan ketiga); `contracts/api-contract.md` 29.1-29.6; `acceptance-test-matrix.md` `AC-02`, `AC-205`, `AC-233`, `AC-247` | Cakupan, wujud, kontrak, kriteria |
| Backend `LabExaminationController.cs` (validate/release/return), `LabResultValidationService.cs`, `LabExaminationResultDtos.cs`, `AccessPermissionFilter.cs`, `LabOrderDtos.cs` | Urutan aturan, pesan, bentuk permintaan, `403` dua lapis |
| Seluruh berkas Halaman Hasil PK `FE-LAB-36`; `lab-microbiology-result-rules.js` `describeResultWriteFailure`; `login-slice.jsx`; `use-permission.js` | Pola yang diperluas |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/laboratory-management/lab-result-status-constants.jsx` (baru) | **Konstanta bersama** `LAB_RESULT_STATUS` (lima label `LAB-DEC-156`) dan `LAB_RESULT_PROGRESS` (label order) — untuk `FE-LAB-41` dan `FE-LAB-43` juga |
| `src/lib/constants/.../lab-clinical-pathology-result-constants.jsx` | `LAB_CLINICAL_PATHOLOGY_SIGN_OFF_PERMISSION` (Validate/Release/Return); kalimat tombol, pesan sukses cadangan, `403` per tindakan, `503`, panel alasan; batas catatan 500. Konstanta keadaan turunan `MVP-8` **dicabut** |
| `src/lib/hooks/.../lab-clinical-pathology-result-rules.js` | `readRowStatusKey`/`readRowState` dari `resultStatus`; `readResultProgressLabel`; `readRowActions` + `canValidate`/`canRelease`/`canReturn`, Buka Kembali hanya pada *Final*; `needsExceptionReason`; `isFourEyesRejection`; `readReasonOptions`; `describeReasonOptionsFailure`; `validateReasonDraft`; `buildSignOffPayload`; `buildReturnPayload`; `describeSignOffFailure`; `readSignOffLines` |
| `src/lib/hooks/.../use-lab-clinical-pathology-result-sheet.jsx` | Izin pengesahan **ketat**; `startValidate`/`startRelease`/`startReturn`; panel alasan dengan pilihan dari `GET /options`; `403` pengesahan **tidak** mengunci pengisian; `409` memuat ulang dan menutup panel; `422` mempertahankan pilihan; `503` menyimpan data coba lagi; rincian order dimuat ulang sesudah pengesahan |
| `src/components/view/.../clinical-pathology/lab-clinical-pathology-result-columns.jsx` | Kolom *Keadaan*: label bersama, baris pengesah, penanda pengecualian `role="note"`; kolom *Aksi*: Validasi, Rilis, *Kembalikan ke analis*, Coba lagi |
| `src/components/view/.../clinical-pathology/lab-clinical-pathology-result-view.jsx` | Kartu *Keadaan Order*; isi panel pengesahan; panel digulir ke pandangan saat dibuka |
| `src/style/.../lab-clinical-pathology-result.module.css` | Kelas pengesah, penanda, dan teks panel |
| `src/lib/services/.../lab-examination.service.js` | `validateLabExaminationResult`, `releaseLabExaminationResult`, `returnLabExaminationResultToAnalyst` — tiga fungsi service |
| `src/lib/services/.../master-data/lab-result-reason.service.js` (baru) | `getLabResultCorrectionReasonOptions`, `getLabFourEyesExceptionReasonOptions` — dua fungsi service data induk |
| `tests/unit/lab-clinical-pathology-result-rules.test.mjs` | Uji label turunan `MVP-8` diganti; 13 uji baru — lihat 6 |

### 3.3 Kepatuhan arsitektur dan keputusan kecil

| Hal | Keputusan dan alasan |
| --- | --- |
| Label keadaan | **Dibaca** dari `resultStatus` lewat konstanta bersama; nilai tak dikenal → tanpa label, bukan tebakan |
| Izin ketiga tombol | **Ketat** (tampil sesudah daftar izin termuat dan memuatnya) — amandemen: *`403` lapis jabatan — tombol tidak tampil sejak awal*. Lapis orang **tidak ditebak**: dokter yang belum ditunjuk tetap melihat tombol dan menerima sebabnya |
| Kapan alasan pengecualian ditanyakan | Sebelum mengirim, bila `resultEnteredByUserId` (validasi) atau `validatedByUserId` (rilis) sama dengan pengguna. Bila id pengguna tak terbaca, `422` `VAL-129`/`VAL-131` backend membuka pertanyaannya — tidak pernah mengirim alasan tanpa perlu (`VAL-132`) |
| Penanda pengecualian | Ditampilkan **apa adanya**; layar tidak menyusun kalimatnya |
| `403` dua lapis | Lapis jabatan dikenali dari balasan generik filter izin → kalimat kontrak per tindakan; selainnya sebab backend apa adanya |
| Halaman baca-saja | Banner *baca-saja* hanya bila pengguna tak memegang izin hasil **maupun** izin pengesahan — dokter pemvalidasi tidak disuguhi kalimat yang menyesatkan |
| Nol tombol pengesahan di luar halaman | Tidak ada perubahan pada antrean, Daftar Kerja, maupun daftar pantau |

`UI GATE: 6 elemen — REUSE 6` — `BaseButton`, `StatusBadge`, `BaseSelectField`, `BaseTextAreaField`,
`InformationAlert`, panel `FE-LAB-36`. Nol komponen baru.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat pilihan alasan | *"Memuat daftar alasan..."*; tombol kirim nonaktif |
| Daftar alasan kosong | *"Daftar alasan belum diisi kepala instalasi. Tindakan ini belum dapat dilakukan."*; tombol kirim nonaktif |
| Daftar alasan `403` | *"Daftar alasan tidak dapat dibaca: akun Anda belum diberi izin membaca daftar alasan ini…"* |
| Galat isian panel | Di bawah isian: alasan wajib, catatan wajib, catatan > 500 |
| `403` lapis jabatan / lapis orang | Pada baris itu: kalimat kontrak per tindakan / sebab dari backend |
| `409` | Pesan backend pada baris; lembar dimuat ulang; panel ditutup |
| `422` | Pesan backend pada baris dan panel; pilihan alasan **tetap** |
| `503` | Kalimat kontrak dan tombol **Coba lagi** pada baris |
| Sibuk | Tombol baris nonaktif; penjaga klik ganda per baris |
| Sukses | Pesan backend pada baris; lembar dan *Keadaan Order* dimuat ulang |

---

## 5. Endpoint yang dikonsumsi

| Method | Path | Badan |
| --- | --- | --- |
| `POST` | `/lab-examinations/{id}/result/validate` | `{}` atau `{ exceptionReasonId, exceptionNote }` |
| `POST` | `/lab-examinations/{id}/result/release` | Sama |
| `POST` | `/lab-examinations/{id}/result/return` | `{ correctionReasonId, note }` |
| `GET` | `/lab-four-eyes-exception-reasons/options`, `/lab-result-correction-reasons/options` | — |
| `GET` | `/lab-examinations/by-order/{id}/results`, `/lab-orders/{id}` | Tetap — kini membaca ruas `r34` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/lab-clinical-pathology-result-rules.test.mjs tests/unit/lab-worklist-result-entry.test.mjs` | **39/39** | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 2240 uji: 2234 lolos, 6 gagal — kegagalan lama di luar Laboratorium (Hemodialisa ×4, Bank Darah M0, petty cash) | `EXISTING / ENVIRONMENT ISSUE` | Keluaran perintah |
| `npx eslint --max-warnings=0` berkas yang disentuh | Nol masalah | `PASS` | Keluaran perintah |
| `npm run build` | Exit 0; route `clinical-pathology/[slug]` ada | `PASS` | Keluaran build |

Uji unit baru: kelima label tepat per `resultStatus` dan nol *Final*; tanpa `resultStatus` tanpa label; label order dan
`orderStatus` tak dipakai; tombol per keadaan dan izin (termasuk Buka Kembali tidak pada Tervalidasi/Dirilis);
tombol per izin masing-masing; kapan alasan pengecualian diminta (GUID tanpa peka huruf, id kosong); pengenalan
`VAL-129`/`VAL-131`; pilihan dan validasi alasan; badan permintaan; `403` dua lapis, `503`, `409`; pengesah dan
penanda apa adanya; pelestarian alasan saat `422` dan `403` pengesahan tidak mengunci pengisian; jalur service.

**Verifikasi manual** — `next dev` `localhost:3000` terhadap backend lokal, order uji `LAB-RSMMC-000001`.
**Nol tulis:** lembar hasil **asli** diubah keadaannya di browser (Hemoglobin *Final*, Leukosit *Tervalidasi*,
Urinalisis Protein *Dirilis* berpenanda); POST pengesahan dicegat dan dijawab tiruan, **kecuali** satu Validasi
yang diteruskan ke backend asli dan ditolak `422` `VAL-124` sebelum menulis (barisnya di database masih Draft).
Pilihan alasan dari `GET /options` **asli**.

| Skenario | Hasil sebenarnya |
| --- | --- |
| S1 Lembar asli | Keadaan *Draft* dari `resultStatus`; nol tombol pengesahan; *Keadaan Order: Dalam Pemeriksaan* |
| S2 Keadaan uji | *Menunggu Validasi* / *Tervalidasi* / *Dirilis*, nol *Final*; tombol Validasi + Buka Kembali / Rilis + *Kembalikan ke analis* (nol Buka Kembali) / nol tombol; *Validasi oleh: nama — jabatan*; penanda tampil sebagai teks utuh |
| S3 Validasi bukan pengisi | Terkirim langsung, badan `{}`; **backend asli** `422` *"Hasil ini belum dinyatakan selesai oleh analis…"* pada baris |
| S4 Validasi oleh pengisi | Panel alasan terbuka **sebelum** mengirim; opsi `SHIFT-TUNGGAL`; badan `{exceptionReasonId, exceptionNote:null}`; `422` tiruan → panel tetap, alasan terpilih tetap |
| S5 Rilis | Panel konfirmasi rekam medis tanpa alasan; badan `{}`; sukses → panel tertutup, lembar dimuat ulang, pesan backend pada baris |
| S6 *Kembalikan ke analis* | Opsi `SAMPEL-TERTUKAR`/`SALAH-KETIK`; tanpa alasan ditolak di layar; badan `{correctionReasonId, note:null}`; `409` → panel tertutup, lembar dimuat ulang, pesan pada baris |
| S7 Rilis dijawab `VAL-131` | Panel beralih ke pertanyaan alasan |
| S8 `403` | Lapis orang: sebab apa adanya pada baris (`AC-233`), nol banner baca-saja; lapis jabatan: *"Anda tidak punya hak memvalidasi hasil laboratorium."* |
| S9 `503` | Kalimat kontrak + **Coba lagi**; mengirim ulang badan yang sama; sukses |
| S10 Pilihan alasan | Kosong → sebab tertulis, tombol nonaktif; `403` → sebab izin tertulis |
| S11 Izin tiruan | Analis (Update): Buka Kembali saja, Tervalidasi nol tombol; dokter (Validate/Release/Return): Validasi; Rilis + Kembalikan; nol Buka Kembali; nol banner baca-saja |
| **Akun analis asli** (salah satu dari dua akun analis `FE-LAB-35`; izin nyata: Update ya, Validate/Release/Return tidak) | Nol tombol pengesahan; Menunggu Validasi → Buka Kembali; Tervalidasi → nol tombol; nol tulis — **4/4** |

Layar: **34/34** + **4/4** `PASS`.

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/lab-clinical-pathology-result-rules.test.mjs — PASS`

`MANUAL TEST: PASS` — dengan batas di bawah.

**Batas verifikasi:**

- Roadmap meminta tiga akun — analis, dokter A, perilis B. **Analis terbukti dengan akun asli.** Dokter
  pemvalidasi dan perilis yang **ditunjuk** pada kredensial Human Resource belum ada di dev (`BE-LAB-73`..`75`
  ⚠ karena hal yang sama), sehingga validasi, rilis, dan pengembalian yang **berhasil** belum pernah terjadi
  terhadap database; bentuk permintaannya dibuktikan lewat pencegatan.
- Dev belum punya satu pun baris Final; keadaan Tervalidasi/Dirilis dan penanda diuji pada lembar asli yang
  diubah di browser.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-02` bagian antarmuka — penanda tampil di halaman hasil | Terpenuhi pada lembar uji | S2; uji unit |
| `AC-205` bagian antarmuka — *Kembalikan* tanpa alasan ditolak, dengan alasan terkirim | Terpenuhi pada layar; `200` sungguhan menunggu dokter yang ditunjuk | S6 |
| `AC-233` bagian antarmuka — sebab lapis orang tampil | Terpenuhi (sebab tiruan berbentuk sama dengan backend) | S8 |
| `AC-247` bagi Patologi Klinik | Terpenuhi | S2; uji unit |
| `LAB-FE-004` — penanda terbaca sebagai teks | Terpenuhi | S2; uji unit |
| Peringatan pengisi sebelum Validasi | Terpenuhi | S4 |
| `403` lapis orang tampil pada barisnya | Terpenuhi | S8 |
| DoD — tiga tindakan berjalan dari halaman | Terpenuhi pada layar; keberhasilan terhadap database menunggu akun | 6 |
| DoD — nol tombol Validasi/Rilis di luar halaman ini | Terpenuhi | Nol perubahan antrean/Daftar Kerja |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nol peringatan lint; build tanpa error |
| Risiko | (1) Pengenalan `403` lapis jabatan dan `VAL-129`/`VAL-131` bergantung pada **kalimat** backend; uji unit menguncinya. (2) Pemegang `Validate`/`Release`/`Return` **wajib** juga diberi `Read` pada kedua data induk alasan — tanpa itu pertanyaan alasan tidak dapat dijawab (layar menuliskan sebabnya). `BE-LAB-71` 8.2 mencatat izin itu sengaja belum diberikan, menunggu `UNK-P14-03`. (3) Bila daftar izin gagal dimuat, ketiga tombol tersembunyi — sengaja ketat |
| Dependency backend | Nol perubahan. Untuk menaikkan ke ✅: dokter pemvalidasi dan perilis yang ditunjuk pada data kewenangan klinis, berizin `Validate`/`Release`/`Return` dan `Read` kedua data induk alasan, serta satu baris Final |
| Perubahan sampingan | `NONE` di repository. **Lingkungan:** sebelum build, `TaskStop` kembali meninggalkan proses `next dev`; dihentikan dan port 3000/7184/5107 dipastikan bebas sebelum build |
| Interupsi | `NONE` |
| Status Git | Frontend (`e613321c5`): ` M` `lab-examination.service.js` (juga milik `FE-LAB-36`); `??` berkas Halaman Hasil PK (milik `FE-LAB-36`, diperluas di sini), `lab-result-status-constants.jsx`, `master-data/lab-result-reason.service.js`. Backend: laporan ini, `frontend-roadmap.md`, `traceability.md`. **Nol operasi Git dijalankan** |
| Langkah berikutnya | `FE-LAB-40` (antrean validasi) kini dapat dikerjakan; `FE-LAB-41` (pola yang sama untuk Mikrobiologi) dan `FE-LAB-43` (memakai `LAB_RESULT_PROGRESS`) juga terbuka |
