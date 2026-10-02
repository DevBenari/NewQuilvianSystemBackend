# Laporan Perubahan Frontend — `FE-LAB-36`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-36` |
| Judul | Halaman Hasil Patologi Klinik per order |
| Slice | Gelombang `MVP-8c` — `EPIC-LAB-14` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) gelombang `MVP-8`, bagian `FE-LAB-36`; tambahan cakupan *Keputusan 2026-09-25 malam* (`LAB-DEC-156`) |
| Trace | `FR-14.5`, `FR-14.8`, `FR-14.10`; `LAB-DEC-135`, `LAB-DEC-141`, `LAB-DEC-149`, `LAB-DEC-156`; `LAB-FE-015`..`LAB-FE-017`; `03-frontend-architecture.md` amandemen 2026-09-24 |
| Contract version | `LAB-API-v1` **`r33`** 28.2-28.3; `LAB-VAL-v1` **`r11`** `VAL-120`..`VAL-123`; `LAB-PERM-v1` **revision 10** — **`approved` 2026-09-24** |
| Wewenang UI | Diputuskan: halaman per order, satu tabel, **nol modal** untuk isian (`LAB-FE-016`, `LAB-FE-017`), penanda berhuruf (`LAB-FE-015`), tombol **Pemeriksaan Selesai** dan label *Menunggu Hasil*/*Draft*/*Menunggu Validasi* (`LAB-DEC-156`). `DEV_DISCRETION` yang dipakai: urutan kolom; tombol di kolom *Aksi*; Buka Kembali dan konsultasi lewat **panel di bawah tabel**; nama berkas |
| Dependency | `BE-LAB-69` ✅ (jalur baca per order, terbukti lewat HTTP 2026-09-29 sore); `BE-LAB-67` ⚠ dan `BE-LAB-68` ✅ (route netral, izin hasil, penjaga Final) |
| Klasifikasi | `HEAVY` — 8 berkas baru (route ×2, view ×2, hook, aturan, konstanta, CSS module) + 1 uji; 3 berkas diubah; 5 endpoint (1 baca, 4 tulis); nol slice Redux baru, nol dependency baru |
| Task mode | `FRONTEND` — backend baca saja |
| Target tulis | `QuilvianSystemFrontendDev`; `NewQuilvianSystemBackend` hanya laporan ini, roadmap frontend, dan `traceability.md` |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `e613321c5` (branch `YogaV2`), di atas perbaikan susulan `FE-LAB-35` yang belum ter-commit |
| Commit backend yang dijadikan rujukan | `55b032b0` (branch `yoga`), dijalankan di `https://localhost:7184` di atas `QuilvianNewDevYoga` |
| Tanggal | 2026-10-01 |
| Status | ✅ **`SELESAI`** — 2026-10-01, sesudah susulan atas instruksi pemilik modul (bagian 10): *Informasi pasien* tampil (`LAB-DEC-166`, `BE-LAB-87`), teks `+2 — Di luar rujukan` (`LAB-DEC-165`), dan baris batal terbukti tidak ikut; uji unit 18/18, layar 9/9 tambahan. *Semula ⚠ `SELESAI DENGAN BATAS VERIFIKASI`:* halaman hidup di route `lab-monitoring/clinical-pathology/[slug]` dan dibuka dari aksi *Buka Hasil* daftar pantau Patologi Klinik. Seluruh pemeriksaan satu order tampil dari **satu** panggilan; tiap baris diisi, disimpan, dinyatakan selesai, dibuka kembali, dan dicatat konsultasinya sendiri-sendiri. Uji unit **16/16**, lint nol peringatan, build hijau, verifikasi layar **35/35** terhadap backend sungguhan dengan akun analis asli (penulisan ke order uji diizinkan pemilik modul). **Batas:** (1) bagian *Informasi pasien* belum dapat ditampilkan — sumber yang ditetapkan arsitektur tidak membawa ruas pasien; (2) nol order Patologi Klinik di dev yang memuat pemeriksaan **batal**; (3) teks `OutOfReference` menunggu `FR-14.9` |

---

## 1. Keadaan yang ditemukan di awal

| Yang ditemukan | Bukti |
| --- | --- |
| Hasil Patologi Klinik hanya dapat diisi lewat dialog Daftar Kerja, satu pemeriksaan per dialog | `lab-worklist-view.jsx:253-370` (`ConfirmModal`) |
| Daftar pantau Patologi Klinik **sengaja** belum punya aksi *Buka Hasil* — "layarnya belum dibangun" | Komentar di `lab-monitoring-view.jsx` |
| Backend sudah menyediakan `GET /by-order/{id}/results` dengan ruas `r33` 28.3 (`urgency`, `referenceFlag`, Final, konsultasi) — **ditambah** `resultStatus` dari `r34` | `LabExaminationResultDtos.cs:93-203`; respons sungguhan order `LAB-RSMMC-000001` |
| Fungsi service simpan hasil (`PUT /{id}/result`) sudah ada; aturan isian `VAL-80`/`VAL-82` sudah ada di berkas aturan Daftar Kerja | `lab-examination.service.js`, `lab-worklist-rules.js` |
| **`BaseTextField` dengan `type: "number"` membuang pemisah desimal** — `6,4` menjadi `64` | `normalizeNumberInputValue` di `base-form-control.jsx` (`replace(/\D+/g, "")`, `Math.trunc`) |
| Rincian order (`GET /lab-orders/{id}`) **tidak membawa ruas pasien** apa pun | Daftar ruas respons sungguhan; halaman Mikrobiologi pun tidak menampilkan pasien |

---

## 2. Proses bisnis dari sisi pengguna

**Siapa:** analis laboratorium (pemegang `LabExaminationResult : Update`); pembaca lain — dokter
pemesan, petugas administrasi — melihat halaman yang sama dalam keadaan baca-saja.

**Alur normal:**

1. Analis membuka **Pemeriksaan Patologi Klinik**, memilih aksi **Buka Hasil** pada satu order.
2. Halaman *Hasil Patologi Klinik — LAB-RSMMC-000001* tampil: ringkasan order, lalu satu tabel berisi
   seluruh pemeriksaan yang tidak batal. Pemeriksaan **cito** di urutan teratas dengan lencana *Cito*.
3. Analis mengetik nilai — misalnya Trombosit `50` dan Leukosit `7,5` — lalu menekan **Simpan** pada
   baris Trombosit. Hanya baris itu yang dikirim; isian Leukosit yang belum disimpan **tetap** di tempatnya.
4. Kolom *Hasil Tersimpan* menampilkan **`L 50`** — huruf `L` dari penanda backend, karena rujukannya
   150–400. Hemoglobin `16,2` pada rujukan 12–15 tampil **`H 16,2`**.
5. Analis menekan **Pemeriksaan Selesai** pada Hemoglobin cito. Labelnya menjadi *Menunggu Validasi* —
   bukan *Final* — dan baris lain tetap dapat diisi dan disimpan.
6. Bila perlu koreksi: **Buka Kembali** membuka panel di bawah tabel; alasan wajib diisi. Konsultasi dicatat
   lewat panel yang sama; faktanya tampil di kolom *Keadaan*: *"Dikonsultasikan kepada dr. … · 1 Okt 2026, 10.55"*.

**Jalur tidak normal:**

| Keadaan | Yang terjadi |
| --- | --- |
| Halaman basi: hasil sudah dinyatakan selesai orang lain, lalu analis menyimpan | `409` — pesan *"Hasil ini sudah dinyatakan selesai. Buka kembali lebih dulu bila perlu diubah."* tampil **pada baris itu**; nilai yang diketik (`15,9`) tetap; baris dimuat ulang dan kini *Menunggu Validasi*, isiannya terkunci |
| Pemeriksaan gugur | Baris tampil dengan alasannya (*"Pemeriksaan yang sudah gugur atau dibatalkan tidak dapat diisi hasilnya."*), tanpa isian, keadaan "—" |
| Pemeriksaan batal | Tidak tampil sama sekali — dikeluarkan backend |
| Order yang bukan Patologi Klinik dibuka lewat route ini | `422` `VAL-123` — pesan backend dan tombol **Buka Hasil Mikrobiologi** (atau *Buka Laporan Patologi Anatomi*) ke order yang sama |
| Order tanpa pemeriksaan Patologi Klinik | *"Order ini tidak memuat pemeriksaan Patologi Klinik yang dapat ditampilkan. Pemeriksaan yang dibatalkan tidak ikut ditampilkan."* |
| Pengguna tanpa izin hasil | Catatan baca-saja; nol isian dan nol tombol tulis; nilai tersimpan beserta penandanya tetap terbaca |
| Backend menjawab `403` meski layar mengira berhak | Kalimat kontrak *"Anda tidak punya hak mengisi atau menyelesaikan hasil laboratorium."* pada baris; tabel beralih baca-saja |
| Klik ganda | Tepat satu permintaan — penjaga per baris |
| Menekan Pemeriksaan Selesai sebelum ada hasil | *"Isi dan simpan hasilnya lebih dulu sebelum menyatakan pemeriksaan selesai."* |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / dokumen | Kenapa dibaca |
| --- | --- |
| `frontend-roadmap.md` `FE-LAB-36` dan *Keputusan 2026-09-25 malam*; `03-frontend-architecture.md` amandemen 2026-09-24; `04-prd-to-mvp.md` `FR-14.9` | Cakupan, wewenang tampilan, jebakan |
| `api-contract.md` `r33` 28.2-28.3; `validation-matrix.md` `VAL-120`..`VAL-123`; `acceptance-test-matrix.md` `AC-214`, `AC-219`, `AC-234`, `AC-236`, `AC-237`, `AC-247` | Kontrak dan kriteria |
| Backend `LabExaminationController.cs`, `LabExaminationResultDtos.cs` | Bentuk as-is respons dan permintaan |
| `use-lab-microbiology-workspace.jsx`, `lab-microbiology-workspace-view.jsx` | Pola acuan halaman per order |
| `lab-worklist-rules.js`, `use-lab-worklist.jsx`, `lab-worklist-view.jsx` | Aturan isian yang dipakai ulang |
| `use-lab-monitoring.jsx`, `lab-monitoring-view.jsx`, `lab-monitoring-table-columns.jsx` | Titik masuk *Buka Hasil* |
| `base-form-control.jsx`, `data-table.jsx`, `summary-grid.jsx`, `status-badge.jsx` | Komponen dasar |
| `private-route-token-utils.js` | Resolusi token route membaca `sessionStorage` |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/.../lab-monitoring/clinical-pathology/[slug]/page.jsx`, `route-token.js` | **Baru** — padanan persis route Mikrobiologi (`LAB-FE-001`) |
| `src/components/view/.../lab-monitoring/clinical-pathology/lab-clinical-pathology-result-view.jsx` | **Baru** — Hero, ringkasan order, peringatan, tabel, panel Buka Kembali/konsultasi |
| `src/components/view/.../lab-monitoring/clinical-pathology/lab-clinical-pathology-result-columns.jsx` | **Baru** — tujuh kolom; isian angka bertipe teks ber-`inputMode: decimal` dengan normalizer desimal; label isian tersembunyi secara visual tetapi terbaca pembaca layar |
| `src/lib/hooks/.../use-lab-clinical-pathology-result-sheet.jsx` | **Baru** — satu panggilan per order; suntingan per baris disimpan di samping data server; penjaga klik ganda per baris; `409` memuat ulang tanpa melepas suntingan; `403` → baca-saja; token route lewat `useSyncExternalStore` (nol `setState` di dalam efek) |
| `src/lib/hooks/.../lab-clinical-pathology-result-rules.js` | **Baru** — urutan cito, penanda berhuruf dari `referenceFlag`, nilai rujukan, keadaan turunan `MVP-8`, isian per baris, normalizer desimal, pemetaan penolakan, tautan disiplin. Memakai ulang `validateResultEntry`/`buildResultPayload` dan `describeResultWriteFailure` |
| `src/lib/constants/.../lab-clinical-pathology-result-constants.jsx` | **Baru** — penanda `H`/`L` (`OutOfReference: null` menunggu `FR-14.9`), label keadaan, kalimat layar, batas panjang |
| `src/style/.../lab-monitoring/lab-clinical-pathology-result.module.css` | **Baru** — tata letak; satu warna penanda dari token `--color-warning` |
| `src/lib/services/.../lab-examination.service.js` | `getLabExaminationResultSheet`, `finalizeLabExaminationResult`, `reopenLabExaminationResult`, `recordLabExaminationConsultation`; komentar hak akses `setLabExaminationResult` diperbarui ke `LabExaminationResult : Update` |
| `src/lib/hooks/.../use-lab-monitoring.jsx` | `openClinicalPathologyResult` — token privat yang sama dengan detail pesanan |
| `src/components/view/.../lab-monitoring/lab-monitoring-view.jsx` | Aksi *Buka Hasil* dipasang pada Patologi Klinik |
| `tests/unit/lab-clinical-pathology-result-rules.test.mjs` | **Baru** — 16 uji |

**Tiga jebakan roadmap.**

| Jebakan | Bagaimana dihindari | Bukti |
| --- | --- | --- |
| `GET /{id}/result` per baris | Hook hanya memanggil `getLabExaminationResultSheet` | Layar: 1 panggilan lembar, 0 panggilan per baris; uji unit memastikan hook nol memakai `getLabExaminationResultForm` |
| Menghapus isian baris lain saat satu baris disimpan | Suntingan per `labExaminationId` disimpan terpisah dari data server; memuat ulang hanya mengganti data server | `AC-237` uji unit dan layar |
| Menurunkan `L`/`H` sendiri dari `normalLow`/`normalHigh` | Penanda dibaca dari `referenceFlag` | Uji: nilai 6,4 berpenanda `Normal` tampil tanpa huruf; nilai 4,2 berpenanda `High` tampil `H 4,2` |

**Dua jebakan tambahan yang ditemukan.**

| Jebakan | Penanganan |
| --- | --- |
| `BaseTextField type: "number"` memotong desimal (`6,4` → `64`) | Isian angka bertipe teks dengan `normalizeDecimalInput`: digit, satu pemisah koma/titik, minus di depan. Uji: `6,4` dikirim sebagai `6.4` |
| Membaca ulang `examinedAt` yang dipotong sebagai waktu lokal menggeser tujuh jam | `examinedAt` tersimpan dibawa utuh (ISO) dan dikirim kembali apa adanya; bila kosong tidak dikirim |

**Keputusan kecil.**

| Hal | Keputusan dan alasan |
| --- | --- |
| Keadaan baris | Turunan `MVP-8` dari isi hasil dan `isFinalized`, sesuai `LAB-DEC-156`, walau backend as-is sudah mengirim `resultStatus` (`r34`). Peralihan ke `resultStatus` milik `FE-LAB-39` |
| Pemeriksaan Selesai atas isian belum tersimpan | Disimpan dulu, baru dinyatakan selesai — pola *Simpan Final* Mikrobiologi |
| Baris tidak dapat diisi tanpa hasil | Keadaan "—", bukan *Menunggu Hasil* — label itu menyesatkan untuk pemeriksaan gugur |
| Ringkasan order | Sumber sama dengan halaman Mikrobiologi (rincian order): No. order, No. lab, disiplin, waktu diminta, peminta, dokter pemeriksa. Lebar kartu minimum 260 px supaya label tidak terpotong |

### 3.3 Kepatuhan arsitektur frontend

- Alur: view → hook → service → `InstanceAxios`; aturan murni terpisah dan teruji tanpa React. Rincian order
  memakai thunk `lab-order-slice` yang sudah ada; lembar hasil memakai state hook — pola `use-lab-worklist`.
  Nol slice baru.
- `UI GATE: 6 elemen — REUSE 6, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0` — `Hero`, `SummaryGrid`, `DataTable`
  (`pagination={false}`, `sortLatestFirst={false}` supaya urutan cito tidak diacak), `BaseTextField`/
  `BaseSelectField`/`BaseTextAreaField`, `BaseButton`, `StatusBadge`, `InformationAlert`, `AccessDeniedGate`.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tabel *"Memuat lembar hasil..."*; tombol *Muat Ulang* nonaktif; tombol baris nonaktif selama tindakan baris itu berjalan |
| Kosong | *"Order ini tidak memuat pemeriksaan Patologi Klinik yang dapat ditampilkan. Pemeriksaan yang dibatalkan tidak ikut ditampilkan."* |
| Gagal | *"Lembar hasil Patologi Klinik gagal dimuat."* beserta pesan backend dan ajakan *Muat Ulang*; `422` `VAL-123` beserta tautan ke halaman disiplin yang benar; galat tindakan tampil pada barisnya |
| Tanpa hak akses | Catatan baca-saja; nol isian dan nol tombol tulis; `403` dari backend menampilkan kalimat kontrak dan mengalihkan tabel ke baca-saja |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Laboratory Management / Lab Examination

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/laboratory-management/lab-examinations/by-order/{labOrderId}/results` | Seluruh baris tabel dalam satu panggilan | `LabExamination : Read` |
| `PUT` | `/v1/health-services/laboratory-management/lab-examinations/{id}/result` | Simpan satu baris | `LabExaminationResult : Update` |
| `POST` | `/v1/health-services/laboratory-management/lab-examinations/{id}/result/finalize` | Pemeriksaan Selesai | `LabExaminationResult : Update` |
| `POST` | `/v1/health-services/laboratory-management/lab-examinations/{id}/result/reopen` | Buka Kembali beralasan | `LabExaminationResult : Update` |
| `PUT` | `/v1/health-services/laboratory-management/lab-examinations/{id}/result/consultation` | Catat konsultasi | `LabExaminationResult : Update` |

#### Health Services / Laboratory Management / Lab Order

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/laboratory-management/lab-orders/{id}` | Ringkasan order (thunk yang sudah ada) | `LabOrder : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/lab-clinical-pathology-result-rules.test.mjs` | **16/16** | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 2209 uji: 2203 lolos, **6 gagal — kegagalan lama** di luar Laboratorium (dua privasi Hemodialisa, dua `FE-HMD-01` AC-3, Bank Darah M0, petty cash); kegagalan akuntansi lama kini lolos sesudah merge | `EXISTING / ENVIRONMENT ISSUE` | Keluaran perintah |
| `npx eslint --max-warnings=0` pada seluruh berkas baru dan berubah | Nol error, nol peringatan | `PASS` | Keluaran perintah |
| `npm run build` | Exit 0 (99 detik); route `clinical-pathology/[slug]` ada | `PASS` | Keluaran build |
| Layar `next dev` `localhost:3000` terhadap backend `55b032b0` (`https://localhost:7184`, `QuilvianNewDevYoga`), akun analis asli | **35/35** butir (rincian di bawah) | `PASS` | Skrip Playwright; tangkapan layar |

**Verifikasi manual** — Chromium (Playwright), akun analis *Penunjang Medis / Analis Laboratorium*, order
uji `LAB-RSMMC-000001`. **Penulisan ke order uji ini diizinkan eksplisit pemilik modul** (2026-10-01).

| Skenario | Hasil sebenarnya |
| --- | --- |
| Titik masuk | Aksi *Buka Hasil* di daftar pantau membuka route `clinical-pathology/{token}`; **1** `GET /by-order/{id}/results`, **0** `GET /{id}/result` |
| Susunan | 5 baris, Hemoglobin cito pertama; baris gugur beralasan tanpa isian; baris pilihan memakai pemilih; nol tombol Validasi/Rilis, nol kata *kritis*, nol dialog |
| `AC-237` | Trombosit `50` dan Leukosit `7,5` diketik; Simpan Trombosit → **hanya satu** `PUT`; Leukosit tetap `7,5` |
| `AC-219` (`L`/`H` dari backend) | Trombosit 50 → **`L 50`**; Hemoglobin 16,2 → **`H 16,2`**; Leukosit 7,5 → `7,5` |
| `AC-236` | Hemoglobin cito dinyatakan selesai → *Menunggu Validasi*, tombol *Buka Kembali*; Trombosit lalu disimpan `55` → `L 55` |
| `409` dari halaman basi | Konteks kedua dibuka sebelum Final; simpan Hemoglobin `15,9` → `409` sungguhan, pesan `VAL-120` **pada baris Hemoglobin saja**; `15,9` tetap di isian; baris dimuat ulang → *Menunggu Validasi*, isian terkunci |
| Buka Kembali | Panel di bawah tabel (bukan dialog); alasan kosong ditolak di layar; dengan alasan → Draft, dapat disimpan |
| Konsultasi (`AC-214` bagian layar) | `PUT /result/consultation`; *"Dikonsultasikan kepada dr. Uji FE-LAB-36, Sp.PK · 1 Okt 2026, 10.55"*; nol pilihan *Definitif* |
| Klik ganda *Pemeriksaan Selesai* | Tepat **1** `POST /result/finalize`; baris lalu dibuka kembali ke Draft |
| `VAL-123` | Order Mikrobiologi lewat route ini → pesan backend `422`; tombol *Buka Hasil Mikrobiologi* membuka halaman Mikrobiologi order yang sama |
| Kosong | Order `LAB-RSMMC-000016` (0 baris) → kalimat kosong bersebab |
| Baca-saja | Daftar izin tanpa izin hasil (disuapkan) → catatan baca-saja, nol isian, nol tombol tulis, `L 55` tetap terbaca, nol permintaan tulis |
| Sesudah build produksi | `next dev` tetap menjawab `200` pada route `[slug]` PK dan Mikrobiologi |

**Data yang berubah di `QuilvianNewDevYoga`** (order uji `LAB-RSMMC-000001`, atas izin pemilik modul):

| Pemeriksaan | Sebelum | Sesudah |
| --- | --- | --- |
| Hemoglobin (cito) | 13,5 `Normal`, Draft, dibuka kembali 1× | 16,2 `High`, Draft, dibuka kembali 2× |
| Leukosit | kosong, Draft | 7,5 `Normal`, Draft, dikonsultasikan |
| Trombosit | kosong, Menunggu Hasil | 55 `Low`, Draft, dibuka kembali 1× |
| Urinalisis Protein ×2 | kosong | **tidak berubah** |

Uji manual: `PASS` — dengan batas di bawah.

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/lab-clinical-pathology-result-rules.test.mjs — PASS`

`MANUAL TEST: PASS terhadap backend sungguhan dengan akun analis asli; pemeriksaan batal dan teks OutOfReference NOT FEASIBLE — lihat di bawah`

**Tidak dijalankan:**

- **Order berisi pemeriksaan batal.** Nol order Patologi Klinik di dev yang memuat pemeriksaan batal; membuatnya
  berarti membatalkan pemeriksaan sungguhan. Pengecualian baris batal adalah perilaku backend, terbukti
  `BE-LAB-69` (18 baris dari 19). Layar menampilkan apa yang dikirim; baris **gugur** terbukti tampil beralasan.
- **Teks `OutOfReference`** — `FR-14.9` `OPEN DECISION`; layar menampilkan nilai saja (uji unit).
- **Baca-saja dengan akun asli** — kedua akun analis kini memegang izin hasil; akun jabatan lain belum tersedia.
  Dibuktikan dengan daftar izin yang disuapkan, seperti `FE-LAB-35`.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-234` — satu panggilan per order; baris batal tidak ikut | Terpenuhi — satu panggilan; sejak 2026-10-01 baris batal terbukti tidak ikut (5 → 4 baris) | Bagian 6 dan 10 |
| `AC-236` — Final Kalium cito, Hemoglobin masih diisi | Terpenuhi (Hemoglobin cito Final, Trombosit tetap disimpan) | Layar |
| `AC-237` — isi dua baris, simpan satu | Terpenuhi | Uji unit + layar |
| `AC-219` bagian antarmuka — `H`/`L` berhuruf | Terpenuhi — `High`/`Low` berhuruf; `OutOfReference` → `+2 — Di luar rujukan` sejak `LAB-DEC-165` | Uji unit + layar (bagian 10) |
| `AC-214` bagian antarmuka — fakta konsultasi terbaca, nol *Definitif* | Terpenuhi | Layar |
| `AC-247` bagian tombol — *Pemeriksaan Selesai*; label tidak pernah *Final* | Terpenuhi | Uji unit + layar |
| DoD — halaman hidup | Terpenuhi | Layar, build |
| DoD — nol modal untuk isian hasil | Terpenuhi | Layar: nol dialog |
| DoD — nol penanda `KRITIS` | Terpenuhi | Layar, uji unit |
| DoD — nol tombol Validasi/Rilis | Terpenuhi | Layar |
| DoD — `OutOfReference` belum diberi teks final | Digantikan `LAB-DEC-165` (2026-10-01): teks final kini dipasang | `LAB_REFERENCE_FLAG_SUFFIX` |
| DoD — laporan `FE-LAB-36.md` | Terpenuhi | Berkas ini |
| Arsitektur — bagian *Informasi pasien* | Terpenuhi sejak 2026-10-01 (`LAB-DEC-166`, `BE-LAB-87`) | Bagian 10 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nol peringatan lint pada berkas yang disentuh; build tanpa error |
| Masalah yang diketahui | **1. ~~Informasi pasien tidak tampil~~ — diselesaikan 2026-10-01** (`LAB-DEC-166`, `BE-LAB-87`, bagian 10); laporan Patologi Anatomi ikut terisi. **2. Label keadaan sebelum `FE-LAB-39`:** baris yang sudah *Tervalidasi*/*Dirilis* (fitur `r34`) tetap berlabel *Menunggu Validasi* karena turunan `MVP-8`; tombol *Buka Kembali* yang tampil akan ditolak backend dengan pesan pada barisnya. **3. Tabrakan nomor:** tabel lama di `03-frontend-architecture.md` memakai `LAB-FE-015` untuk "nol status hasil ditampilkan", nomor yang di decisions berarti penanda berhuruf; `LAB-DEC-156` yang berlaku |
| Temuan di luar lingkup — **prioritas tinggi** | `BaseTextField type: "number"` membuang desimal: **kadar antibiogram** (`0,5` → `5`) dan **volume specimen** (`1,5` → `15`) di halaman Mikrobiologi. **Diperbaiki 2026-10-01** atas instruksi pemilik modul — bagian 10.4, dan bagian susulan [`FE-LAB-31.md`](FE-LAB-31.md) / [`FE-LAB-32.md`](FE-LAB-32.md) |
| Temuan di luar lingkup | Dialog hasil Daftar Kerja memotong `examinedAt` UTC lalu membacanya sebagai waktu lokal; setiap simpan ulang menggeser waktu pemeriksaan **mundur tujuh jam** (`lab-worklist-rules.js` `buildResultFormState` + `buildResultPayload`). **Tuntas 2026-10-01** — dialog beserta `buildResultFormState` dicabut `FE-LAB-37` |
| Dependency backend | Nol task backend yang belum selesai bagi halaman ini. Pemakaian sungguhan menunggu rilis `MVP-8` (route netral `BE-LAB-67`) beserta izin hasil bagi jabatan analis — di dev sudah diberikan 2026-10-01 |
| Perubahan sampingan | `NONE` — verifikasi memakai skrip Playwright di luar repository; `test-results/` tidak tersentuh |
| Interupsi | `NONE` |
| Status Git | Frontend (`e613321c5`): `??` route `clinical-pathology/[slug]/`, view `clinical-pathology/`, `lab-clinical-pathology-result-constants.jsx`, `lab-clinical-pathology-result-rules.js`, `use-lab-clinical-pathology-result-sheet.jsx`, CSS module, uji unit; ` M` `lab-examination.service.js`, `use-lab-monitoring.jsx`, `lab-monitoring-view.jsx` — milik `FE-LAB-36`. ` M` `lab-microbiology-result-slice.jsx`, `lab-microbiology-result-fe35-rules.test.mjs`, `menu-items.jsx` milik verifikasi susulan `FE-LAB-35`. Backend: laporan ini, `frontend-roadmap.md`, `traceability.md`. **Nol operasi Git dijalankan** |
| Langkah berikutnya | `FE-LAB-37` siap: cabut dialog isi hasil Daftar Kerja dan arahkan barisnya ke halaman ini |

---

## 10. Susulan 2026-10-01 — tiga batas ditutup dan perbaikan desimal

Atas instruksi pemilik modul sesudah laporan pertama: *Informasi pasien* ditambahkan di backend, pemeriksaan
batal diuji, rekomendasi teks `FR-14.9` disetujui langsung, dan cacat desimal Mikrobiologi diperbaiki.

### 10.1 Informasi pasien — `LAB-DEC-166`, `BE-LAB-87`

Rincian order kini membawa delapan ruas identitas pasien (`LAB-API-v1` `r38` bagian 33). Halaman menampilkan
bagian **Informasi Pasien** di atas **Informasi Order**: Pasien, No. RM, Umur, Jenis Kelamin, Tipe Kunjungan,
Unit Layanan.

| Hal | Keputusan |
| --- | --- |
| Umur | Dihitung layar (`formatAge`) dari tanggal **kalender** lahir pada tanggal WIB hari ini: `N tahun`, di bawah setahun `N bulan`, di bawah sebulan `N hari` — tidak pernah usang karena disimpan |
| Jenis kelamin | Label `LAB_PATIENT_GENDER_LABEL` yang sudah ada (*Laki-laki*/*Perempuan*) |
| Tipe kunjungan | Label opsi daftar pantau yang sudah ada; nama enum backend dipetakan ke nilainya (urutan enum `EncounterType` sama) |
| Tata letak | Dua `SummaryGrid` berjudul; kartu pasien `minWidth` 320 px supaya nama panjang tidak memotong label |

### 10.2 Teks hasil pilihan di luar rujukan — `LAB-DEC-165`

Rekomendasi yang disetujui: **`+2 — Di luar rujukan`** — teks di belakang nilai, sesuai usulan `r33` 28.3.
Huruf `H`/`L` keliru bagi hasil pilihan (tidak punya arah), simbol `*` tidak menjelaskan dirinya, dan teks utuh
tetap terbaca di cetak hitam-putih. Konstanta `LAB_REFERENCE_FLAG_SUFFIX`; nol perubahan backend.

### 10.3 Pemeriksaan batal

Satu pemeriksaan order uji dibatalkan lewat `POST /lab-examinations/{id}/cancel` — **Trombosit**, baris yang
nilainya ditulis pengujian ini sendiri, supaya data uji yang sudah ada tidak hilang. Pembatalan tidak dapat
dibalik; ia hanya mengubah status dan mencatat log (koreksi tagihan milik `BE-LAB-13`).

### 10.4 Perbaikan desimal Mikrobiologi (di luar lingkup task, atas instruksi)

Normalizer desimal dipindah ke berkas bersama `lab-decimal-input-rules.js` (`normalizeDecimalInput`,
`parseDecimal`, `DECIMAL_FIELD_PROPS`) dan dipakai halaman ini serta halaman Mikrobiologi:

| Berkas | Perubahan |
| --- | --- |
| `lab-microbiology-result-form.jsx` | Kadar antibiogram: teks + `inputMode: decimal` + normalizer, bukan `type: "number"` |
| `lab-microbiology-specimen-section.jsx` | Volume specimen: sama |
| `lab-microbiology-result-rules.js` | `hasNumber`/`optionalNumber`/batas volume negatif memakai `parseDecimal` — koma diterima; tanpa ini `"0,5"` dianggap bukan angka dan kadarnya **terkirim kosong** |
| `lab-clinical-pathology-result-rules.js` | Memakai normalizer bersama (salinan lokal dihapus) |

### 10.5 Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/lab-clinical-pathology-result-rules.test.mjs tests/unit/lab-decimal-input-rules.test.mjs` | **24/24** (18 + 6) | `PASS` |
| Suite `tests/unit/` | 2217 uji: 2211 lolos, 6 gagal — kegagalan lama di luar Laboratorium; uji Mikrobiologi `FE-LAB-31`/`32` lolos | `EXISTING / ENVIRONMENT ISSUE` |
| `npx eslint --max-warnings=0` seluruh berkas yang disentuh | Nol masalah | `PASS` |
| `npm run build` | Exit 0 (99 detik) | `PASS` |
| Pembatalan Trombosit (`200`) → lembar per order 5 → **4** baris; daftar pemeriksaan order tetap memuat Trombosit berstatus `Cancelled` | Sesuai | `PASS` |
| Layar: 4 baris tanpa Trombosit | Sesuai | `PASS` |
| Layar: *Informasi Pasien* — *AGNES YULIANI RAJA GUK GUK*, *00-00-00-13*, *26 tahun*, *Perempuan*, *Rawat Jalan*, *Laboratorium Klinik* | Sesuai | `PASS` |
| Layar: Urinalisis Protein diisi `+2` → **`+2 — Di luar rujukan`**; backend menyimpan `referenceFlag = OutOfReference`; hanya satu `PUT` | Sesuai | `PASS` |
| Mikrobiologi (permintaan simpan **dicegat**, nol data berubah): volume `1,5` tampil `1,5`, badan `PATCH` `volumeAmount: 1.5`; kadar `0,5` tampil `0,5`, badan `PUT` `concentration: 0.5` | Sesuai | `PASS` |

**Data uji yang berubah** (atas izin pemilik modul): Trombosit `LAB-RSMMC-000001` **dibatalkan**; Urinalisis
Protein (pilihan) diisi `+2`.

### 10.6 Temuan baru di luar lingkup

| Temuan | Dampak | Saran |
| --- | --- | --- |
| Form login tidak menetapkan `method="post"`. Bila tombol *Masuk* ditekan **sebelum** React selesai dimuat (jaringan lambat, kompilasi dev), form terkirim sebagai GET biasa — **email dan sandi tercantum di URL** (`/login?email=…&password=…`), ikut tersimpan di riwayat browser dan log server | Keamanan kredensial | Tambahkan `method="post"` (dan `action` yang aman) pada form login — milik pemilik autentikasi. Satu kejadian tertangkap di log dev server lokal saat verifikasi dan sudah disensor |
