# Laporan Perubahan Frontend — `FE-LAB-41`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-41` |
| Judul | Validasi, rilis, dan pengembalian pada Halaman Hasil Mikrobiologi |
| Slice | Gelombang `MVP-10b` — `EPIC-LAB-16`, `S4d-1` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) gelombang `MVP-10`, bagian `FE-LAB-41`; *Keputusan 2026-09-25 malam* |
| Trace | `FR-16.8`, bagian layar `FR-16.1`..`FR-16.4` dan `FR-16.7`, bagian Mikrobiologi `FR-15.19`; `LAB-FE-004`, `LAB-DEC-114`, `LAB-DEC-120`, `LAB-DEC-138`, `LAB-DEC-149`, `LAB-DEC-156` |
| Contract version | `LAB-API-v1` `r35` 30.2, 30.3, 30.5 (`approved` 2026-09-25); `r34` 29.2-29.3 (bentuk permintaan); `LAB-VAL-v1` `r13` `VAL-144`, `VAL-126`; `LAB-PERM-v1` rev 11 bagian 13.4 |
| Wewenang UI | Diputuskan: lima label lewat konstanta bersama (`LAB-DEC-156`); penanda pengecualian sebagai teks (`LAB-FE-004`); tindakan di halaman hasil (`LAB-DEC-149`); tombol per izin; Validasi tidak ditawarkan pada `Sementara` beserta keterangannya. `DEV_DISCRETION` yang dipakai: tiga tombol dan panel alasan pada **bagian *Pengesahan* di batang Kelengkapan**, tepat di bawah tombol Final; wujud panel sama dengan `FE-LAB-39`; *Keadaan Order* sebagai kartu ke-6 ringkasan order |
| Dependency | `FE-LAB-39` ⚠ (pola, aturan, tiga fungsi service, konstanta bersama), `FE-LAB-35` ✅; `BE-LAB-78` ⚠, `BE-LAB-79` ✅ |
| Klasifikasi | `MEDIUM` — 9 berkas diubah, 1 berkas uji baru; nol route baru; nol perubahan backend |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`; `NewQuilvianSystemBackend` hanya laporan ini, roadmap frontend, dan `traceability.md` |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `e613321c5` (branch `YogaV2`, upstream `origin/YogaV2`), di atas `FE-LAB-35`..`FE-LAB-40` yang belum ter-commit |
| Commit backend yang dijadikan rujukan | `55b032b0` (branch `yoga`) + `BE-LAB-67`..`87` yang belum ter-commit; biner lokal tidak lebih tua dari satu pun berkas `.cs`; `https://localhost:7184` di atas `QuilvianNewDevYoga` |
| Tanggal | 2026-10-02 |
| Status | ✅ **`SELESAI`** (naik 2026-10-06) — validasi, pengembalian, dan rilis BTA dijalankan **sungguhan** di Halaman Hasil Mikrobiologi oleh dr. Nabila (akun asli, kredensial `LAB-VAL-MB`/`LAB-REL-MB`); rilis tercatat di rekam medis; analis asli mengisi ulang hasil yang dikembalikan dan nol tombol pengesahan. Lihat 10. *(Semula: ⚠ — ketiga tindakan berjalan dari Halaman Hasil Mikrobiologi; `Sementara` tidak menawarkan Validasi; pengesah dan penanda terbaca sebagai teks; *Pemeriksaan Selesai*. Uji unit 23 baru, lint nol peringatan baru, build hijau, layar **21/21**. **Batas (diperbarui 2026-10-06):** setup uji langkah rilis di dev sudah lengkap — kode `LAB-VAL-MB`/`LAB-REL-MB`, kredensial dr. Nabila, izin jabatan — dan penolakan lapis orang terbukti dengan akun asli (lihat 9); yang tersisa **validasi dan rilis sungguhan oleh dr. Nabila**, menunggu sandi akunnya. *Semula:* kode kewenangan Mikrobiologi (`LAB-COORD-016`) belum ada di katalog Human Resource, sehingga tidak satu akun pun dapat memvalidasi atau merilis hasil Mikrobiologi di dev — jalur berhasil dibuktikan lewat pencegatan; ketiga akun samaran roadmap belum ada (lihat 6))* |

---

## 1. Keadaan yang ditemukan di awal

| Yang ditemukan | Bukti |
| --- | --- |
| Batang Kelengkapan menampilkan peringatan *"Penulisan selesai — belum dirilis"*, tombol **Simpan Final**, dan Buka Kembali pada setiap hasil Final; **nol** tombol validasi/rilis | `lab-microbiology-completion-bar.jsx` (komentar `FE-LAB-33`: *"nol tombol validasi atau rilis"*) |
| Backend sudah mengirim `resultStatus`, `resultEnteredByUserId`, `validatedByName`, `authorizingOfficerName`, jabatan, waktu, dan kedua penanda pada `GET /{id}/result/microbiology` | `LabMicrobiologyResultResponse` (`BE-LAB-79`); di dev BTA `025be4cf…` → `resultStatus: "Final"`, 39 ruas |
| `resultQualifier` dikirim sebagai **angka** enum (`1` Definitif, `2` Sementara) — tidak ada `JsonStringEnumConverter` | `LabMicrobiologyResultDtos.cs`; formulir sudah memakai angka |
| Respons Mikrobiologi **tidak** punya `releasedByName`/`isValidated`; perilis dibaca dari `authorizingOfficerName` | `r35` 30.3 |
| Detail order Mikrobiologi sudah membawa `resultProgress` | `GET /lab-orders/{id}` LAB-RSMMC-000014 → `InProgress` |
| Ketiga fungsi service dan pola panel alasan sudah berdiri di `FE-LAB-39` | `lab-examination.service.js`, `lab-clinical-pathology-result-rules.js` |
| Data dev: satu-satunya hasil Final Mikrobiologi BTA `025be4cf…` (kualifikasi kosong, **diisi akun superadmin**); BTA kedua *Menunggu Hasil*; nol hasil Tervalidasi/Dirilis | Pembacaan HTTP |

---

## 2. Proses bisnis dari sisi pengguna

**Analis** mengisi hasil seperti sebelumnya, lalu menekan **Pemeriksaan Selesai** (dulu *Simpan Final*). Di bawah
*Kelengkapan*, baris **Keadaan** kini menunjukkan *Menunggu Hasil* → *Draft* → **Menunggu Validasi**. Selama
*Menunggu Validasi*, ia masih dapat **Buka Kembali**.

**Dokter pemvalidasi Mikrobiologi** (dr. Nabila kelak) membuka hasil yang sama. Di bagian **Pengesahan**:

1. Hasil *Menunggu Validasi* menawarkan **Validasi**. Bila ia sendiri pengisi hasilnya, panel alasan pengecualian
   terbuka **sebelum** apa pun terkirim; selain itu langsung terkirim.
2. Sesudah divalidasi: keadaan **Tervalidasi**, baris *"Validasi oleh: dr. … — jabatan · waktu"*, dan bila ada,
   penanda pengecualian sebagai **kalimat utuh dari backend** di kotak bertanda. Isolat, antibiogram, status temuan,
   dan kualifikasi baca-saja; **Buka Kembali tidak lagi ditawarkan**.
3. Hasil **Sementara** tidak menawarkan Validasi; keterangannya terbaca: *"Hasil sementara belum dapat divalidasi —
   Buka kembali dan ubah kualifikasinya menjadi Definitif bila hasil sudah definitif."* Kualifikasi **kosong** tetap
   menawarkan Validasi.

**Perilis** pada hasil *Tervalidasi* menekan **Rilis** → panel konfirmasi *"…menjadi dokumen klinis pasien dan
tercatat pada rekam medis…"*; bila ia juga pemvalidasinya, alasan pengecualian diminta di panel yang sama. Sesudah
rilis: **Dirilis**, baris *"Petugas Otorisasi: …"*, dan kartu **Keadaan Order** di ringkasan order beralih
*Dalam Pemeriksaan* → *Selesai* bila seluruh pemeriksaan sudah dirilis.

**Keduanya** dapat ***Kembalikan ke analis*** pada hasil *Tervalidasi*: alasan wajib dari daftar; hasil kembali Draft.

**Jalur tidak normal:** `403` lapis orang menuliskan **sebabnya apa adanya** (belum ditunjuk sebagai pemegang
kewenangan validasi **Mikrobiologi**, akun belum terhubung data tenaga kerja, dan seterusnya); `403` lapis jabatan →
kalimat kontrak per tindakan; `409` memuat ulang hasil dan menutup panel; `422` — termasuk `VAL-144` bila tetap datang —
tampil pada pemeriksaan itu dan alasan yang dipilih **tetap**; `503` → kalimat kontrak dan **Coba lagi**.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / dokumen | Kenapa dibaca |
| --- | --- |
| `frontend-roadmap.md` `FE-LAB-41` dan *Keputusan 2026-09-25 malam*; `03-frontend-architecture.md` amandemen 2026-09-25 (kedua dan ketiga); `contracts/api-contract.md` 30.1-30.7; `acceptance-test-matrix.md` `AC-183`, `AC-218`, `AC-241`, `AC-247`, baris *30.3* dan *Layar — `Sementara`* | Cakupan, wujud, kontrak, kriteria |
| `task/report/backend/BE-LAB-78.md`, `BE-LAB-79.md`, `task/report/frontend/FE-LAB-39.md` | Perilaku backend; pola yang dipakai ulang |
| Backend `LabMicrobiologyResultDtos.cs`, `LabResultValidationService.cs` (`ValidateAsync`, `EnsureAppointedAsync`) | Ruas, tipe enum, urutan pemeriksaan (penunjukan sebelum empat mata dan sebelum menulis) |
| Seluruh berkas Halaman Hasil Mikrobiologi (`FE-LAB-30`..`35`), `lab-microbiology-result-slice.jsx`, berkas Halaman Hasil PK `FE-LAB-39`, `use-permission.jsx`, `permission-slice.jsx`, `rules/frontend/design-tokens.md` | Pola yang diperluas; token |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/.../microbiology/lab-microbiology-completion-bar.jsx` | Baris **Keadaan** (label bersama), pengesah dan penanda `role="note"`; peringatan *"Penulisan selesai — belum dirilis"* **dicabut**; **Pemeriksaan Selesai**; Buka Kembali hanya per `canReopen`; bagian **Pengesahan**: keterangan `Sementara`, tombol Validasi/Rilis/*Kembalikan ke analis*, pesan dan **Coba lagi**, panel alasan/konfirmasi |
| `src/lib/hooks/.../use-lab-microbiology-result-editor.jsx` | Izin pengesahan **ketat**; id pengguna sesi; `startValidate`/`startRelease`/`startReturn`, panel, pilihan alasan dari `GET /options`, `403` dua lapis, `409` memuat ulang, `422` mempertahankan pilihan, `503` dapat dicoba lagi; `readOnly` hanya bila tanpa izin hasil **dan** tanpa izin pengesahan; `onSignedOff` memuat ulang label order |
| `src/lib/hooks/.../lab-microbiology-result-rules.js` | `readMicrobiologyStatusKey`/`readMicrobiologyStatus`, `isPreliminaryResult`, `readMicrobiologySignOffActions`, `readMicrobiologySignOffLines`; `isResultLocked` juga mengunci *Final/Tervalidasi/Dirilis* dari `resultStatus` |
| `src/lib/constants/.../lab-microbiology-result-constants.jsx` | `LAB_RESULT_QUALIFIER`; kalimat *Pemeriksaan Selesai*, *Keadaan*, *Validasi oleh*, *Petugas Otorisasi*, keterangan `Sementara`; `finalizedNotReleased` **dicabut** |
| `src/components/view/.../microbiology/lab-microbiology-result-panel.jsx` | Banner baca-saja dari `editor.readOnly`; sambungan ke batang kelengkapan; `onSignedOff` |
| `src/components/view/.../microbiology/lab-microbiology-workspace-view.jsx` | Kartu **Keadaan Order** dari `resultProgress`; `SummaryGrid minWidth={260}` (enam kartu — lihat 3.3) |
| `src/lib/hooks/.../use-lab-microbiology-workspace.jsx` | `refreshOrderProgress` — hanya rincian order yang dimuat ulang sesudah pengesahan |
| `src/style/.../lab-microbiology-workspace.module.css` | Kelas keadaan, pengesah, penanda, pesan — **token saja**, nol hex baru |
| `src/lib/services/.../lab-microbiology-result.service.js` | Komentar usang *"nol endpoint validasi, nol endpoint rilis"* diperbarui; nol fungsi baru |
| `tests/unit/lab-microbiology-result-fe41-rules.test.mjs` (baru) | 23 uji — lihat 6 |

### 3.3 Kepatuhan arsitektur dan keputusan kecil

| Hal | Keputusan dan alasan |
| --- | --- |
| Dipakai ulang dari `FE-LAB-39` | Tiga fungsi service, `needsExceptionReason`, `isFourEyesRejection`, `readReasonOptions`, `describeReasonOptionsFailure`, `validateReasonDraft`, `buildSignOffPayload`, `buildReturnPayload`, `describeSignOffFailure`, `readResultProgressLabel`, kalimat pengesahan `LAB_CLINICAL_PATHOLOGY_COPY`, `LAB_RESULT_STATUS`. **Nol** service atau aturan kembar |
| Arah impor | Hook Mikrobiologi mengimpor aturan PK; berkas aturan Mikrobiologi **tidak** — aturan PK sudah mengimpor aturan Mikrobiologi, sehingga impor balik akan melingkar |
| Service langsung, bukan thunk | Tindakan lain halaman ini memakai thunk, tetapi pemetaan `403` dua lapis membutuhkan galat Axios utuh. Keadaan tindakan tinggal di hook, sama dengan `FE-LAB-39`; hasil tetap dimuat ulang lewat thunk yang ada |
| Pengesah | Dari `validatedByName` dan `authorizingOfficerName` saja; nama analis atau pengguna sesi tidak pernah dipakai (`LAB-DEC-120`) |
| `Sementara` | Dikenali dari angka `2` dan nama `Preliminary`; kosong **bukan** Sementara (21.10 butir 2) |
| Respons tanpa `resultStatus` | Tanpa label (bukan tebakan); Buka Kembali jatuh ke perilaku lama (`isFinalized`) |
| Kartu ringkasan | Kartu ke-6 membuat label tertulis tegak huruf per huruf pada lebar bawaan — ditemukan saat uji layar; diperbaiki dengan `minWidth={260}`, pola Halaman Hasil PK |

`UI GATE: 8 elemen — REUSE 8` — `BaseButton`, `StatusBadge`, `BaseSelectField`, `BaseTextAreaField`,
`InformationAlert`, `SummaryGrid`, bagian `subSection`/`actionRow` yang sudah ada, panel `FE-LAB-39`. Nol komponen baru.
Grep anti-regresi: nol `<button` mentah, nol `<table`, nol `fw-`/`fs-`; CSS baru nol hex/rgb/`!important`.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat pilihan alasan | *"Memuat daftar alasan..."*; tombol kirim nonaktif |
| Daftar alasan kosong / `403` | Sebab tertulis; tombol kirim nonaktif |
| Galat isian panel | Alasan wajib, catatan wajib, catatan > 500 — di bawah isian |
| `403` lapis jabatan / lapis orang | Kalimat kontrak per tindakan / sebab backend apa adanya — pada pemeriksaan itu; halaman **tidak** beralih baca-saja |
| `409` | Pesan backend; hasil dimuat ulang; panel ditutup |
| `422` (`VAL-144`, `VAL-129`/`131`, alasan tak sah) | Pesan backend; pilihan alasan tetap; `VAL-129`/`131` membuka pertanyaan alasan |
| `503` | Kalimat kontrak + **Coba lagi** (mengirim badan yang sama) |
| Sibuk | Tombol pengesahan nonaktif; penjaga klik ganda |
| Sukses | Pesan backend; hasil dan *Keadaan Order* dimuat ulang |
| Tanpa izin hasil maupun pengesahan | Banner baca-saja `FE-LAB-35` |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Laboratory Management / Lab Examination

Base URL: `api/v1/health-services/laboratory-management/lab-examinations`

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/{id}/result/validate` | Validasi — `{}` atau `{ exceptionReasonId, exceptionNote }` | `LabExaminationResult : Validate` |
| `POST` | `/{id}/result/release` | Rilis — sama | `LabExaminationResult : Release` |
| `POST` | `/{id}/result/return` | *Kembalikan ke analis* — `{ correctionReasonId, note }` | `LabExaminationResult : Return` |
| `GET` | `/{id}/result/microbiology` | Tetap — kini membaca ruas `r35` 30.3 | `LabExamination : Read` |

#### Health Services / Laboratory Management / Lab Four Eyes Exception Reason · Lab Result Correction Reason

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/lab-four-eyes-exception-reasons/options`, `/lab-result-correction-reasons/options` | Pilihan alasan | `… : Read` |

#### Health Services / Laboratory Management / Lab Order

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/lab-orders/{id}` | Tetap — kini membaca `resultProgress`; dimuat ulang sesudah pengesahan | `LabOrder : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test` kelima berkas uji Mikrobiologi dan PK | **99/99** (23 baru) | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 2285: 2279 lolos, 6 gagal — **sama dengan baseline** `FE-LAB-39`/`40` (Hemodialisa ×4, Bank Darah M0, petty cash) | `EXISTING / ENVIRONMENT ISSUE` | Keluaran perintah |
| `npx eslint --max-warnings=0` berkas yang disentuh | Nol masalah, kecuali satu peringatan `react-hooks/set-state-in-effect` di `use-lab-microbiology-workspace.jsx:63` — **ada di `HEAD`** (kode `FE-LAB-30`), tidak disentuh | `EXISTING WARNING` | Keluaran perintah; lint salinan `HEAD` |
| `npm run lint:errors` | Exit 0 | `PASS` | Keluaran perintah |
| `npm run build` (server BE/FE dimatikan, port dipastikan bebas) | Exit 0; route `microbiology/[slug]` ada | `PASS` | Keluaran build |

**Uji unit baru (23):** lima label tepat per `resultStatus`, nol *Final*; tanpa `resultStatus` tanpa label; tombol
*Pemeriksaan Selesai*; `Sementara` dari angka dan nama; kosong/Definitif bukan Sementara; `Sementara` Final → nol
Validasi + keterangan, juga bagi pengguna tanpa izin; Validasi **tampil** pada kualifikasi kosong; tombol per keadaan
(Menunggu Validasi, Tervalidasi, Dirilis, Draft) dan per izin; Buka Kembali tidak pada Tervalidasi; perilaku lama tanpa
`resultStatus`; kunci isian pada Tervalidasi/Dirilis; pengesah kosong sebelum validasi (`AC-183`), *Validasi oleh* dari
`validatedByName`, *Petugas Otorisasi* dari `authorizingOfficerName`, penanda apa adanya, nama analis tak pernah
dipakai; alasan pengecualian sebelum validasi/rilis; `403` lapis orang berkata *Mikrobiologi* apa adanya; `403` lapis
jabatan; `422` `VAL-144`; `503`.

**Verifikasi manual** — `next dev` `localhost:3000` terhadap backend lokal, login superadmin lewat formulir,
Playwright, order `LAB-RSMMC-000014`. Lembar hasil **asli** dibaca dari backend lalu keadaannya diubah di peramban
(Sementara, Tervalidasi, Dirilis, pengisi/pemvalidasi = pengguna); POST pengesahan dicegat. Pilihan alasan dan
rincian order dari backend **asli**.

| Skenario | Hasil sebenarnya |
| --- | --- |
| M1 BTA Final asli | **Menunggu Validasi**; Validasi + Buka Kembali; **Pemeriksaan Selesai** dan Simpan Draft nonaktif; nol pengesah; nol peringatan lama; *Keadaan Order: Dalam Pemeriksaan* |
| M2 Data asli — superadmin **pengisi** BTA itu | Panel alasan terbuka **sebelum** apa pun terkirim (*"Anda yang mengisi hasil ini…"*), pilihan `Shift tunggal…` dari backend; Validasi beralasan **diteruskan ke backend asli** → `403` lapis orang *"Akun Anda belum terhubung dengan data tenaga kerja, sehingga kewenangan validasi tidak dapat diperiksa. Hubungi bagian SDM."* tampil apa adanya pada pemeriksaan; panel tetap; nol banner baca-saja |
| M3 BTA kedua | **Menunggu Hasil**; nol tombol pengesahan dan Buka Kembali |
| M4 Final `Sementara` | Nol Validasi; *"Hasil sementara belum dapat divalidasi"* + petunjuknya; Buka Kembali ada |
| M4b `422` `VAL-144` yang tetap datang | Kalimat backend tampil pada pemeriksaan |
| M5 Tervalidasi | Label; *"Validasi oleh: dr. … — Dokter Penanggung Jawab Mikrobiologi · waktu"*; penanda sebagai teks `role="note"`; Rilis + *Kembalikan*; nol Buka Kembali/Validasi |
| M6 Rilis | Panel konfirmasi rekam medis tanpa alasan; badan `{}`; sukses → panel tertutup, pesan backend, hasil **dan** rincian order dimuat ulang; *Keadaan Order: Selesai* |
| M7 Rilis oleh pemvalidasi | Alasan diminta; tanpa alasan ditolak layar; badan `{exceptionReasonId, exceptionNote}`; `422` → panel dan pilihan tetap |
| M8 *Kembalikan* | Alasan wajib (pilihan `Sampel tertukar`/`Salah ketik hasil` dari backend); badan `{correctionReasonId, note}`; `409` → panel tertutup, pesan, hasil dimuat ulang |
| M9 Validasi oleh pengisi | Pertanyaan alasan sebelum mengirim |
| M10 `503` | Kalimat kontrak + **Coba lagi**; badan sama; sukses |
| M11 `403` lapis jabatan | *"Anda tidak punya hak memvalidasi hasil laboratorium."* |
| M12 Dirilis | Label; *Validasi oleh* + *Petugas Otorisasi: dr. … — jabatan*; kedua penanda; nol tombol |
| M13 Izin tiruan | Analis (Update saja): Final → Buka Kembali, nol Validasi; Tervalidasi → nol tombol. Dokter (Validate/Release/Return tanpa Update): Validasi ada; nol Simpan/Pemeriksaan Selesai/Buka Kembali; nol banner baca-saja |
| M14 390 px | Tanpa gulir horizontal halaman |
| M15–M16 | Satu Validasi diteruskan (ditolak `403`); nol tulis lain lewat layar; nol galat runtime |
| Ringkasan order | Sesudah `minWidth={260}`: enam kartu membungkus dua baris, label satu-dua baris (sebelumnya tegak huruf per huruf) |

Layar: **21/21** `PASS`.

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/lab-microbiology-result-fe41-rules.test.mjs — PASS`

`MANUAL TEST: PASS` — dengan batas di bawah.

**Insiden saat verifikasi (dicatat apa adanya).** Pada putaran pertama skrip, jawaban "teruskan ke backend" yang
disiapkan untuk Validasi tidak terpakai (superadmin ternyata pengisi hasil, sehingga Validasi membuka panel tanpa
mengirim), lalu terpakai oleh **Rilis** pada skenario berikutnya. Satu `POST /025be4cf…/result/release` sampai ke
backend asli dan **ditolak `422`** (hasil belum divalidasi) sebelum menulis. Pembacaan ulang sesudahnya: BTA tetap
*Final*, `isReleased` salah, `validatedByName` kosong. Skrip lalu diperbaiki: antrean jawaban dikosongkan per
skenario dan "teruskan" hanya berlaku bagi Validasi. Total permintaan pengesahan yang sampai ke backend dari sesi
ini menurut log backend: **tiga** — satu rilis `422` (tak sengaja, 03.38 UTC) dan dua validasi `403` (disengaja, dua
putaran skrip, 03.41 dan 03.42 UTC); **nol tulis**.

**Batas verifikasi:**

- Roadmap meminta tiga akun samaran — analis, dr. Nabila, perilis — ditambah dokter pemegang kode Patologi Klinik
  saja. Kode kewenangan Mikrobiologi (`LAB-VAL-MB`/`LAB-REL-MB`, `LAB-COORD-016`) belum ada di katalog Human Resource,
  sehingga **tidak ada** akun yang dapat memvalidasi atau merilis hasil Mikrobiologi di dev (`BE-LAB-78` ⚠ karena hal
  yang sama). Jalur berhasil dibuktikan lewat pencegatan; peran analis dan dokter lewat daftar izin tiruan.
- `403` lapis orang **asli** teramati dengan sebab *akun belum terhubung data tenaga kerja*; kalimat `AC-241`
  berkata *Mikrobiologi* belum teramati dengan akun asli — layar menampilkan sebab backend apa adanya, dikunci uji unit.
- Dev nol punya hasil Tervalidasi/Dirilis maupun hasil `Sementara`.

**Tidak dijalankan:** `npm run test:e2e` — tidak diminta dan tidak ada spec Mikrobiologi untuk tindakan ini.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-247` bagi Mikrobiologi — lima label, *Pemeriksaan Selesai*, *Dalam Pemeriksaan* hanya label order | **Terpenuhi** | M1, M3, M5, M12; uji unit |
| Baris *Layar — `Sementara`* — Validasi tidak ditawarkan, keterangan terbaca | **Terpenuhi** | M4; uji unit |
| Bagian antarmuka `AC-241`/`AC-218` — `403` lapis orang pada pemeriksaannya | Terpenuhi pada layar dengan sebab backend asli (data tenaga kerja); kalimat *Mikrobiologi* menunggu akun pemegang kode PK | M2; uji unit |
| `LAB-FE-004` — penanda sebagai teks | **Terpenuhi** | M5, M12 |
| `AC-183` di layar — kedua pengesah kosong sebelum pengesahan | **Terpenuhi** — data asli | M1; uji unit |
| Isian baca-saja sesudah validasi; Reopen tidak ditawarkan | **Terpenuhi** | M5; uji unit |
| Alasan pengecualian sebelum mengirim; alasan pengembalian wajib | **Terpenuhi** — termasuk data asli | M2, M7, M8, M9 |
| Label order dari `resultProgress` | **Terpenuhi** | M1, M6 |
| `403` dua lapis, `409`, `422`, `503` | **Terpenuhi** | M2, M4b, M7, M8, M10, M11 |
| DoD — tiga tindakan berjalan dari halaman | Terpenuhi pada layar; keberhasilan terhadap database menunggu `LAB-COORD-016` | 6 |
| DoD — `Sementara` tak menawarkan Validasi; pengesah dan penanda terbaca sebagai teks | **Terpenuhi** | M4, M5, M12 |
| DoD — nol cetakan baru; lint dan build hijau; laporan | **Terpenuhi** | 6 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Satu `EXISTING WARNING` lint (`use-lab-microbiology-workspace.jsx:63`, kode `FE-LAB-30`) — tidak diperbaiki, di luar cakupan |
| Masalah yang diketahui | **1.** Kalimat pengesahan diambil dari `LAB_CLINICAL_PATHOLOGY_COPY` — kalimatnya umum bagi kedua disiplin, tetapi nama konstantanya PK; memindahkannya ke berkas bersama adalah perapian tersendiri. **2.** Pengenalan `403` lapis jabatan dan `VAL-129`/`131` bergantung pada **kalimat** backend (warisan `FE-LAB-39`) |
| Dependency backend | Nol perubahan. Untuk menaikkan ke ✅: kode `LAB-COORD-016` di katalog Human Resource, dr. Nabila dan perilis ditunjuk, jabatan berizin `Validate`/`Release`/`Return` dan `Read` kedua data induk alasan, satu hasil Mikrobiologi Final — langkah rilis `MVP-10c` |
| Perubahan sampingan | `NONE` di repository. Lingkungan: proses `next dev`/backend tertinggal sesudah `TaskStop` dihentikan sebelum build; port dipastikan bebas |
| Interupsi | `NONE` |
| Status Git | Frontend (`e613321c5`): ` M` `lab-microbiology-completion-bar.jsx`, `lab-microbiology-result-panel.jsx`, `lab-microbiology-workspace-view.jsx`, `lab-microbiology-result-constants.jsx`, `lab-microbiology-result-rules.js`, `use-lab-microbiology-result-editor.jsx`, `use-lab-microbiology-workspace.jsx`, `lab-microbiology-result.service.js`, `lab-microbiology-workspace.module.css`; `??` `tests/unit/lab-microbiology-result-fe41-rules.test.mjs`. Perubahan `FE-LAB-35`..`40` yang belum ter-commit ikut ada. Backend: laporan ini, `frontend-roadmap.md`, `traceability.md`. **Nol operasi Git dijalankan** |
| Langkah berikutnya | `FE-LAB-42` (penyaring disiplin antrean) kini dapat dikerjakan — baris Mikrobiologi antrean membuka halaman yang sudah dapat bertindak; `FE-LAB-43` tetap terbuka |

## 9. Verifikasi nyata 2026-10-06 — sesudah setup langkah rilis di devYoga

**Status saat bagian ini ditulis: `FE-LAB-41` tetap ⚠** — naik ✅ di bagian 10. Bukti baru: penolakan lapis orang dengan akun asli (M1). Jalur berhasil — validasi dan rilis BTA oleh dr. Nabila (kredensial `LAB-VAL-MB`/`LAB-REL-MB` sudah aktif) — menunggu sandi akunnya.

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
| P3 | 39 | Leukosit: Validasi `200`, lalu *Kembalikan ke analis* — tanpa alasan nol permintaan; dengan *"Sampel tertukar"* **`200`**; kembali *Draft* | `PASS` |
| Q3 | 40, 42 | Kembali ke antrean: Hemoglobin pindah ke *Menunggu Rilis* dengan *Divalidasi oleh dr. Bima*; Leukosit (dikembalikan) di kedua tahap tidak ada | `PASS` |
| P4 | 39 | Rilis oleh pemvalidasi sendiri: panel menyebut rekam medis dan meminta alasan pengecualian (*"Shift tunggal…"*) → **`200`**; *Dirilis*, *Otorisasi oleh* dr. Bima, penanda pengecualian sebagai teks; nol tombol | `PASS` |
| RM | 39 | Baris rekam medis: satu `MrcClinicalDocumentIntegrity` bagi Hemoglobin — ditandatangani dan dikunci atas nama dr. Bima saat rilis | `PASS` |
| Q4 | 40 | Sesudah dirilis Hemoglobin hilang dari *Menunggu Rilis* | `PASS` |
| M1 | 41 | dr. Bima (berizin `Validate`, **tanpa** kewenangan Mikrobiologi) memvalidasi BTA → **`403` lapis orang asli**: *"Anda belum ditunjuk sebagai pemegang kewenangan validasi Mikrobiologi."* tampil apa adanya | `PASS` |
| V1 | 39 | Vina (analis): Hemoglobin *Dirilis* nol tombol; Leukosit yang dikembalikan dapat diisi lagi, nol tombol pengesahan | `PASS` |
| Z1 | — | Nol tulis di luar tindakan yang diizinkan; nol galat runtime | `PASS` |

**Jejak di devYoga:** Hemoglobin `LAB-RSMMC-000001` **Dirilis** (validasi dan rilis oleh dr. Bima, berpenanda pengecualian empat
mata); Leukosit dikembalikan ke *Draft* dengan alasan *Sampel tertukar* (`SAMPEL-TERTUKAR`; *dikoreksi 2026-10-06 dari riwayat transisi — semula tertulis Salah ketik hasil*). Data uji.

**Nol perubahan kode** pada task ini. **Nol operasi Git dijalankan.**

## 10. Verifikasi nyata jalur berhasil — dr. Nabila, 2026-10-06

**Status: `FE-LAB-41` ✅ `SELESAI`.** Batas terakhir — validasi dan rilis Mikrobiologi sungguhan — tertutup.

**Akun asli:** dr. Nabila Rahmawati, Sp.MK (*Dokter Penanggung Jawab Laboratorium*, kredensial `LAB-VAL-MB`/`LAB-REL-MB` dari
setup bagian 9) dan Vina (analis). Sandi diberikan pemilik modul; hanya dipakai skrip uji lokal, tidak dicetak dan tidak disimpan.
Tulis yang diteruskan hanya pada BTA `LAB-RSMMC-000014` (`025be4cf…`), dibatasi per fase: dr. Nabila `validate`/`return`, Vina
`PUT …/result/microbiology` + `finalize`, lalu dr. Nabila `validate`/`release`. Selainnya digagalkan.

| ID | Task | Skenario | Hasil |
| --- | --- | --- | --- |
| N1 | 42 | dr. Nabila: penyaring *Mikrobiologi* memuat BTA `LAB-RSMMC-000014` dari backend asli; `discipline=Microbiology` | `PASS` |
| N2 | 41 | Dibuka dari antrean: *Menunggu Validasi*; hanya Validasi yang ditawarkan | `PASS` |
| N3 | 41 | Validasi → **`200`** langsung (bukan pengisi hasil, tanpa alasan pengecualian); *Tervalidasi*, *"Validasi oleh: dr. Nabila Rahmawati, Sp.MK — Dokter Penanggung Jawab Laboratorium"*; Rilis dan Kembalikan ditawarkan | `PASS` |
| N4 | 40, 42 | Antrean Mikrobiologi: BTA keluar dari *Menunggu Validasi*, masuk *Menunggu Rilis* dengan dr. Nabila sebagai pemvalidasi | `PASS` |
| N5 | 41 | *Kembalikan ke analis*: tanpa alasan nol permintaan (*Alasan wajib dipilih…*); dengan *Sampel tertukar* **`200`** → *Draft*, validasi dicabut, nol tombol pengesahan | `PASS` |
| V2 | 41 | Vina: hasil yang dikembalikan dapat diisi lagi, nol tombol pengesahan; *Pemeriksaan Selesai* → simpan `200` lalu `finalize` `200`; kembali *Menunggu Validasi* | `PASS` |
| N6 | 41, 42 | BTA kembali di antrean *Menunggu Validasi*; validasi ulang **`200`** | `PASS` |
| N7 | 41 | Rilis oleh pemvalidasi sendiri: panel menyebut rekam medis dan meminta alasan pengecualian (*"Shift tunggal, tidak ada dokter lain bertugas"*) → **`200`** *"Hasil dirilis dan tercatat pada rekam medis pasien."*; *Dirilis*, *Petugas Otorisasi* dr. Nabila, penanda pengecualian sebagai teks; nol tombol | `PASS` |
| RM | 41 | Satu baris `MrcClinicalDocumentIntegrity` (`DocumentKind` 15) bagi BTA — ditandatangani dan dikunci 16.17 WIB, saat rilis | `PASS` |
| N8 | 40, 42 | Sesudah dirilis: BTA keluar dari kedua tahap antrean Mikrobiologi | `PASS` |
| V3 | 41 | Vina: *Dirilis* dengan *Petugas Otorisasi*; *Simpan Draft* dan *Pemeriksaan Selesai* nonaktif, nol tombol pengesahan | `PASS` |
| Z1 | — | Nol tulis di luar tindakan yang diizinkan; nol galat runtime | `PASS` |

**Jalannya uji.** Putaran pertama V2 `FAIL` karena penjaga tulis skrip, bukan layar: di Mikrobiologi *Pemeriksaan Selesai*
menyimpan isian dulu (`PUT …/result/microbiology`) baru `finalize`. `PUT` itu digagalkan skrip, layar menampilkan *Network Error*
dan `finalize` tidak dikirim — perilaku yang benar (gagal simpan menghentikan penyelesaian). Isian tersimpan dibandingkan dengan
layar (temuan *Normal*, tanpa kualifikasi, biakan, metode, maupun isolat — sama), `PUT` diizinkan pada fase Vina, lalu uji
dilanjutkan dari fase itu. Kedua putaran: **12/12**.

**Jejak di devYoga:** BTA `025be4cf…` `LAB-RSMMC-000014` **Dirilis** (pengisi terakhir Vina; validasi dan rilis dr. Nabila,
berpenanda pengecualian empat mata); satu pengembalian tercatat (*Sampel tertukar*). BTA kedua (`1f3670d7…`) tidak disentuh. Data uji.

**Tidak diperiksa:** pencocokan akun penandatangan baris rekam medis ke surel lewat tabel pengguna ditolak pengaman izin (data
pribadi) dan tidak dikejar. Buktinya: waktu tanda tangan sama dengan rilis, dan layar menulis *Petugas Otorisasi* dr. Nabila.

**Nol perubahan kode** pada task ini. **Nol operasi Git dijalankan.**
