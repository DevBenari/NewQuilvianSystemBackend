# Laporan Perubahan Frontend — `FE-LAB-35`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-35` |
| Judul | Halaman Mikrobiologi mengikuti izin hasil, route netral, dan penjaga Final |
| Slice | Gelombang `MVP-8a` — `EPIC-LAB-14`; pasangan rilis `BE-LAB-67` dan `BE-LAB-68` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) gelombang `MVP-8`, bagian `FE-LAB-35` |
| Trace | `FR-14.6`, `FR-14.12` (juga bagian layar `FR-14.1` dan `FR-14.3`); `LAB-DEC-146`, `LAB-DEC-147`; `03-frontend-architecture.md` bagian *Halaman Mikrobiologi — diperbarui* |
| Contract version | `LAB-API-v1` **`r33`** bagian 28.2 (route netral, izin, kalimat `403`) dan 28.4 (pencabutan route lama); `LAB-VAL-v1` `r11` `VAL-120`, `VAL-121`; `LAB-PERM-v1` **revision 10** — **`approved` 2026-09-24** |
| Wewenang UI | Disetujui: kontrol tulis **tersembunyi** bagi yang tidak memegang `LabExaminationResult : Update`; `403` dan `409` tampil terbaca; isian tidak hilang. `DEV_DISCRETION` yang dipakai: catatan baca-saja berupa `InformationAlert` info di atas formulir; pemilih hasil dikunci (bukan disembunyikan) supaya hasilnya tetap terbaca; catatan *"Belum ada konsultasi yang dicatat."* bagi pembaca |
| Dependency | `BE-LAB-67` ⚠ (route netral dan izin hasil — batas tersisa hanya `AC-221`/`AC-222` dengan akun asli); `BE-LAB-68` ✅ (penjaga Final) |
| Klasifikasi | `MEDIUM` — 7 berkas source diubah, 1 berkas uji unit baru; 3 route berpindah ke route netral, 1 method berubah (`POST` → `PUT`); slice Redux diperluas dua ruas; nol komponen baru, nol dependency baru |
| Task mode | `FRONTEND` — backend baca saja |
| Target tulis | `QuilvianSystemFrontendDev` (source); `NewQuilvianSystemBackend` hanya laporan ini serta tautan buktinya pada roadmap frontend dan `traceability.md` |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `68195b2be` (branch `YogaV2`), di atas perubahan `FE-LAB-44`/`FE-LAB-45` yang belum ter-commit |
| Commit backend yang dijadikan rujukan | `7ff35b8c` (branch `yoga`) beserta perubahan `BE-LAB-67`..`BE-LAB-86` yang belum ter-commit; DLL hasil build 2026-09-30 13:21, lebih baru dari seluruh source |
| Tanggal | 2026-10-01 |
| Status | ✅ **`SELESAI`** — 2026-10-01, sesudah verifikasi susulan dengan **dua akun analis asli** (bagian 9): halaman baca-saja terbukti pada akun tanpa izin hasil, `403` asli backend terbukti berbentuk sama dengan yang diuji, dan cacat lama *"Hasil Mikrobiologi gagal dimuat."* pada pembatalan diperbaiki (uji unit **11/11**). **Belum dirilis** — tetap wajib serempak dengan `BE-LAB-67`/`BE-LAB-68`, beserta pemberian izin hasil kepada jabatan analis. *Semula ⚠ `SELESAI DENGAN BATAS VERIFIKASI`:* Final, Buka Kembali, dan Konsultasi memanggil route netral; nol rujukan tersisa ke route lama; penolakan `409`/`403` tampil dengan kalimatnya sendiri dan **nilai yang diketik tetap di isian**; tanpa izin hasil halaman menjadi baca-saja. Uji unit **8/8**, lint 0 error, build hijau. Layar hasil build **dijalankan terhadap backend lokal `BE-LAB-67`/`BE-LAB-68`**: simpan, Final, dan konsultasi atas hasil yang sudah Final ditolak **`409` sungguhan**, nilai tetap di isian, dan isi hasil di database identik sebelum dan sesudah — **29/29** butir. **Batas:** bagian antarmuka `AC-221` dibuktikan dengan daftar izin yang disuapkan, karena belum ada jabatan di dev yang memegang `LabExaminationResult : Update`; `403` asli tidak dapat dipancing lewat superadmin. **Belum dirilis** — wajib dirilis bersama `BE-LAB-67`/`BE-LAB-68` |

---

## 1. Keadaan yang ditemukan di awal

| Yang ditemukan | Bukti |
| --- | --- |
| Halaman Mikrobiologi masih memanggil tiga route lama `/result/microbiology/finalize`, `/reopen`, `/consultation` — dan konsultasi dikirim dengan `POST` | `lab-microbiology-result-constants.jsx:16,19,22` dan `recordMicrobiologyConsultation` pada HEAD |
| Backend sudah mencabut ketiga route lama **tanpa alias** | Dipanggil langsung pada backend lokal: ketiganya `404` (bagian 6) |
| Seluruh tombol tulis tampil bagi siapa saja yang membuka halaman, termasuk dokter pemesan | Nol pemanggilan `usePermission` pada hook editor dan komponen hasil Mikrobiologi |
| Penolakan dibaca hanya sebagai teks; kode statusnya hilang di slice | `submitMicrobiologyResult.rejected` hanya menyimpan `message`; `readServerFailure` sebenarnya sudah membawa `statusCode` |
| Kalimat `403` dari backend bersifat umum: *"Anda tidak memiliki akses ke menu atau fitur ini."* | `Filters/AccessPermissionFilter.cs:19`; kontrak `r33` 28.2 meminta kalimat yang menjelaskan hak yang kurang |
| Formulir **sudah** hanya diisi ulang bila hasil tersimpan berubah (cap hasil) — dasar yang dibutuhkan `AC-228` | `use-lab-microbiology-result-editor.jsx` efek `resultStamp` |

---

## 2. Proses bisnis dari sisi pengguna

**Siapa:** analis Mikrobiologi (memegang `LabExaminationResult : Update`), dan pembaca hasil seperti dokter
pemesan (memegang `LabExamination : Update` untuk cito/batal, tetapi **tidak** memegang izin hasil).

**Alur normal analis** — tidak berubah dari sisi tampilan:

1. Analis membuka order Mikrobiologi dari daftar pantau, memilih pemeriksaan, mengisi status temuan,
   kualifikasi, jenis biakan, dan isolat.
2. Menekan **Simpan Draft**, lalu **Simpan Final**. Simpan Final selalu menyimpan lebih dulu, baru
   menyatakan selesai — kini lewat route `POST …/result/finalize`.
3. Bila perlu koreksi sesudah Final: mengisi *Alasan membuka kembali* lalu **Buka Kembali**
   (`POST …/result/reopen`). Konsultasi dicatat lewat **Catat Konsultasi** (`PUT …/result/consultation`).

**Alur pembaca (dokter pemesan):**

1. Membuka halaman yang sama dan melihat catatan: *"Halaman ini baca-saja bagi Anda. Mengisi,
   menyelesaikan, membuka kembali, dan mencatat konsultasi hasil memerlukan hak hasil laboratorium."*
2. Hasil tetap terbaca: keempat pemilih hasil menampilkan nilainya tetapi terkunci; Kelengkapan dan
   catatan konsultasi tampil apa adanya.
3. Tidak ada tombol **Simpan Draft**, **Simpan Final**, **Buka Kembali**, **Catat Konsultasi**, maupun
   **Tambah Isolat**.

**Jalur tidak normal — contoh nyata dari verifikasi:**

| Keadaan | Yang terjadi |
| --- | --- |
| **Halaman basi.** Analis A membuka hasil yang belum Final, lalu analis B menyatakannya Final. A memilih *Kualifikasi hasil = Sementara* dan *Jenis biakan = Jamur*, lalu menekan Simpan Draft | Backend menolak `409`. Layar menampilkan *"Hasil ini sudah dinyatakan selesai. Buka kembali lebih dulu bila perlu diubah."* — dan **Sementara** serta **Jamur** tetap terpilih, tidak tertimpa nilai server |
| Halaman basi, A menekan Simpan Final | Final ditolak `409`: *"Hasil pemeriksaan ini sudah dinyatakan selesai."*; pilihan A tetap di isian |
| Hasil sudah Final, analis mencatat konsultasi ke *"dr. Uji"* | Ditolak `409`: *"Hasil ini sudah dinyatakan selesai. Buka kembali lebih dulu sebelum mencatat konsultasi."*; nama konsultan tetap di ruasnya |
| Daftar izin di layar masih menyatakan berhak, tetapi backend menolak `403` (misalnya izin baru dicabut) | Layar menampilkan kalimat kontrak *"Anda tidak punya hak mengisi atau menyelesaikan hasil laboratorium."* — bukan kalimat umum filter izin; alasan yang sudah diketik tetap di ruasnya |
| Daftar izin sesi belum termuat | Tombol tetap tampil (konvensi `usePermission`: `allowed` bernilai benar selama belum diketahui) — tidak ada kilasan "baca-saja" bagi analis; backend tetap menegakkan `403` |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas / dokumen | Kenapa dibaca |
| --- | --- |
| `frontend-roadmap.md` `FE-LAB-35`; `03-frontend-architecture.md` *Halaman Mikrobiologi — diperbarui*; `04-prd-to-mvp.md` `FR-14.6`, `FR-14.12` | Cakupan, wewenang UI, DoD |
| `contracts/api-contract.md` `r33` 28.2 dan 28.4; `contracts/validation-matrix.md` `VAL-120`, `VAL-121`; `testing/acceptance-test-matrix.md` `AC-221`, `AC-228` | Route, method, izin, kalimat `403`, bunyi `409` |
| Backend `LabExaminationController.cs` (route dan `[AccessPermission]`), `Filters/AccessPermissionFilter.cs` (bentuk badan `403`) | Bukti as-is route netral, izin, dan bentuk galat |
| Laporan `BE-LAB-67.md` dan `BE-LAB-68.md` bagian 8 | Pemeriksaan uji Final di dev dan bukti `409` sisi backend |
| `use-permission.jsx`, `permission-slice` | Konvensi `allowed` selama izin belum diketahui |
| `use-lab-microbiology-result-editor.jsx`, `lab-microbiology-result-slice.jsx` (`readServerFailure`), komponen hasil dan bilah kelengkapan | Tempat penanganan galat dan kontrol tulis |
| `base-form-control.jsx` (`BaseSelectField` → `FilterSelect`) | Perilaku pemilih saat `disabled` |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/.../lab-microbiology-result-constants.jsx` | `finalize`, `reopen`, `consultation` beralih ke route netral `/{id}/result/finalize\|reopen\|consultation` (baris 22-29); tiga kalimat baru: `resultReadOnly`, `resultWriteForbidden` (kalimat kontrak `403`), `resultAlreadyFinal` (cadangan `409`, sama dengan `VAL-120`) |
| `src/lib/services/.../lab-microbiology-result.service.js` | Konsultasi dikirim dengan **`PUT`** sesuai `r33` 28.2; komentar hak akses diperbarui ke `LabExaminationResult : Update` |
| `src/lib/hooks/.../lab-microbiology-result-rules.js` | `resultFormStamp` (baris 293) — cap hasil tersimpan dipindah ke berkas aturan murni supaya teruji; `describeResultWriteFailure` (baris 309) — `403` → kalimat kontrak, `409` → pesan backend apa adanya dengan cadangan `VAL-120`, lainnya → pesan backend lalu cadangan pemanggil |
| `src/lib/hooks/.../use-lab-microbiology-result-editor.jsx` | `usePermission("LabExaminationResult", "Update")` (baris 112) diekspos sebagai `canWriteResult`; `errorMessage` membaca galat simpan dan aksi lewat `describeResultWriteFailure` (baris 422-423); cap formulir memakai `resultFormStamp` |
| `src/lib/state/slice/.../lab-microbiology-result-slice.jsx` | Dua ruas baru `saveErrorStatus` dan `actionErrorStatus`, diisi dari `statusCode` penolakan (baris 271, 391) dan dikosongkan pada permintaan berikutnya. `state.result` **sengaja tidak disentuh** saat penolakan |
| `src/components/view/.../microbiology/lab-microbiology-result-panel.jsx` | Catatan baca-saja bila `!canWriteResult` (baris 116); formulir dikunci `locked \|\| !canWriteResult` (baris 122); `canWrite` diteruskan ke bilah kelengkapan |
| `src/components/view/.../microbiology/lab-microbiology-completion-bar.jsx` | Prop `canWrite`: Simpan Draft/Simpan Final, Buka Kembali, dan isian konsultasi **tidak dirender** bila salah (baris 116, 139, 182); catatan *"Belum ada konsultasi yang dicatat."* bagi pembaca (baris 178). Selisih diff besar karena indentasi blok yang dibungkus |
| `tests/unit/lab-microbiology-result-fe35-rules.test.mjs` | **Baru** — 8 uji: route netral, `PUT` konsultasi, pemetaan `409`/`403`/lainnya, pelestarian isian lewat reducer sungguhan, cap berubah sesudah Final/Buka Kembali |

**Dua jebakan yang dihindari.**

| Jebakan | Bagaimana dihindari | Bukti |
| --- | --- | --- |
| Memuat ulang hasil sesudah penolakan — formulir tertimpa nilai server, ketikan analis hilang | Handler hanya memuat ulang hasil bila aksi **berhasil**; penolakan tidak mengubah `state.result`, jadi cap tidak berubah dan formulir tidak diisi ulang | Uji unit reducer; layar S1-S3 |
| Menampilkan kalimat umum filter izin pada `403` | `describeResultWriteFailure` mengganti `403` dengan kalimat kontrak; `409` tetap memakai pesan backend karena bunyinya sudah spesifik (`VAL-120` ≠ `VAL-121` ≠ Final ganda) | Uji unit; layar S4 |

### 3.3 Kepatuhan arsitektur frontend

- Alur tetap: view → hook editor → slice/thunk → service → `InstanceAxios`. View nol Axios.
- Aturan murni (pemetaan pesan, cap hasil) berada di berkas aturan yang sudah ada dan teruji tanpa React.
- Izin memakai `usePermission` yang sudah baku — nol mekanisme izin baru.

`UI GATE: 3 elemen — REUSE 3, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat base | Status | Catatan |
| --- | --- | --- | --- |
| Catatan baca-saja | `InformationAlert` varian `info` | REUSE | Pola yang sama dengan catatan Informasi Specimen baca-saja |
| Pesan `403`/`409` | `InformationAlert` varian `danger` yang sudah dipakai `editor.errorMessage` | REUSE | Nol komponen baru |
| Pemilih hasil terkunci | `BaseSelectField` `disabled` | REUSE | Dikunci, tidak disembunyikan — nilai hasil tetap terbaca |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tidak berubah dari `FE-LAB-31`..`34`; tombol berlabel memuat selama simpan atau aksi berjalan |
| Kosong | Pembaca tanpa konsultasi tercatat: *"Belum ada konsultasi yang dicatat."* |
| Gagal | `409`: pesan backend (`VAL-120`, `VAL-121`, atau Final ganda); `403`: kalimat kontrak. Isian tetap; pemulihan: Buka Kembali (bagi yang berhak) atau muat ulang halaman |
| Tanpa hak akses | Tanpa `LabExaminationResult : Update`: catatan baca-saja, pemilih terkunci, nol tombol tulis hasil |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Laboratory Management / Lab Examination

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/laboratory-management/lab-examinations/{id}/result/microbiology` | Memuat hasil — tidak berubah | `LabExamination : Read` |
| `PUT` | `/v1/health-services/laboratory-management/lab-examinations/{id}/result/microbiology` | Simpan Draft — route tidak berubah; ditolak `409` `VAL-120` bila hasil sudah Final | `LabExaminationResult : Update` |
| `POST` | `/v1/health-services/laboratory-management/lab-examinations/{id}/result/finalize` | Simpan Final — **route netral baru** | `LabExaminationResult : Update` |
| `POST` | `/v1/health-services/laboratory-management/lab-examinations/{id}/result/reopen` | Buka Kembali beralasan — **route netral baru** | `LabExaminationResult : Update` |
| `PUT` | `/v1/health-services/laboratory-management/lab-examinations/{id}/result/consultation` | Catat Konsultasi — **route netral baru, method `PUT`**; ditolak `409` `VAL-121` bila hasil sudah Final | `LabExaminationResult : Update` |

Route lama `/result/microbiology/finalize`, `/reopen`, `/consultation` **tidak lagi dipanggil** dan di backend
menjawab `404`.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/lab-microbiology-result-fe35-rules.test.mjs` | **8/8** | `PASS` | Keluaran perintah |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 2087 uji: 2080 lolos, **7 gagal — tujuh kegagalan lama yang sama dengan baseline `FE-LAB-44`** (akuntansi reconciliation, petty cash, Bank Darah M0, dua privasi Hemodialisa, dua `FE-HMD-01` AC-3); nol menyangkut Laboratorium | `EXISTING / ENVIRONMENT ISSUE` | Keluaran perintah; [`FE-LAB-44.md`](FE-LAB-44.md) bagian 6 |
| `npx eslint` pada 7 berkas source dan berkas uji yang disentuh | 0 error, 1 warning: *"Calling setState synchronously within an effect"* pada efek pengisi ulang formulir — **sudah ada di HEAD** (baris 156), tidak disentuh task ini | `EXISTING / ENVIRONMENT ISSUE` | Keluaran perintah; lint versi HEAD berkas yang sama |
| `npm run build` | Exit 0, *Compiled successfully*; route `lab-monitoring/microbiology/[slug]` ada di keluaran | `PASS` | Keluaran build |
| `grep -rnE "result/microbiology/(finalize\|reopen\|consultation)" src tests` | Nol hasil | `PASS` | Keluaran perintah |
| Backend lokal `BE-LAB-67`/`BE-LAB-68` lewat HTTP, pemeriksaan Final *Pewarnaan BTA Sputum* (sama dengan uji `BE-LAB-68`) | Simpan `409` `VAL-120`; Final `409` *"Hasil pemeriksaan ini sudah dinyatakan selesai."*; konsultasi `PUT` `409` `VAL-121`; ketiga route lama `404`; isi hasil **identik** sebelum dan sesudah | `PASS` | Skrip HTTP; log backend |
| Layar hasil build terhadap backend lokal — lima skenario, 29 butir (rincian di bawah) | **29/29** pada empat putaran penuh berturut-turut terakhir | `PASS` | Skrip Playwright; tangkapan layar |

**Cara layar diuji.** Build standalone (`:3710`) mengarah ke API dev bersama, jadi panggilan `/v1/**` dicegat
Playwright lalu **diteruskan ke backend lokal** (`:5107`, database dev) dengan sesi superadmin. Dua hal disuapkan:
daftar izin `/v1/auth/permissions` (superadmin melewati pemeriksaan izin backend), dan — pada skenario halaman
basi — GET hasil **pertama** dibalik menjadi belum Final. Demi data klinis bersama, **hanya** tiga tulis yang
pasti ditolak (simpan, Final, konsultasi atas pemeriksaan yang sudah Final) yang diteruskan; tulis lain
disuapkan, dan skrip berhenti sebelum menulis apa pun bila pemeriksaan uji tidak Final.

| Skenario | Hasil sebenarnya |
| --- | --- |
| S1 — halaman basi, Simpan Draft dengan *Sementara* + *Jamur* | `PUT /result/microbiology` diteruskan → **`409` sungguhan**; pesan `VAL-120` tampil; pemilih tetap *Normal \| Sementara \| Jamur \| Pilih data* |
| S2 — halaman basi, Simpan Final dengan *Definitif* (simpan disuapkan `200`) | `POST /result/finalize` — route netral — diteruskan → **`409` sungguhan**; pesan backend tampil; *Definitif* tetap |
| S3 — hasil Final, konsultasi ke *"dr. Uji FE-LAB-35"* | Simpan Draft dan Simpan Final nonaktif; **`PUT`** `/result/consultation` diteruskan → **`409` sungguhan**; pesan `VAL-121` tampil; nama tetap di ruas |
| S4 — hasil Final, Buka Kembali beralasan (dijawab `403` suapan berbentuk badan `AccessPermissionFilter`) | `POST /result/reopen` — route netral; kalimat kontrak tampil, kalimat umum **tidak**; alasan tetap di ruas. Buka Kembali sengaja tidak diteruskan karena akan berhasil dan menulis data |
| S5 — dokter pemesan (`LabExamination : Read`/`Update`, tanpa izin hasil), kedua pemeriksaan order (Final dan belum Final) | Catatan baca-saja tampil; nol tombol tulis hasil dan nol *Tambah Isolat*; nol ruas konsultasi dan alasan; **4 dari 4** pemilih hasil terkunci; *"Belum ada konsultasi yang dicatat."*; **nol** permintaan tulis |
| Data sesudah seluruh putaran | Hasil kedua pemeriksaan **identik** sebelum dan sesudah. Log backend: 35 tulis selama verifikasi — simpan `409` ×11, Final `409` ×15, konsultasi `409` ×9 — ditambah route lama `404` ×3; **nol tulis diterima** |

**Ketidakstabilan yang ditemukan dan sebabnya.** S2 sempat gagal pada dua putaran awal (pesan tidak muncul
dalam 15 detik). Log backend mencatat tiga `GLOBAL_UNHANDLEDEXCEPTION` pada GET (08:58:01, 08:58:37, 09:02:11)
dengan sebab *"An existing connection was forcibly closed by the remote host"* — koneksi ke database dev remote
diputus — dan waktunya berimpit dengan kedua putaran gagal itu. Sesudahnya S2 lolos **10 kali berturut-turut**
(6 sendirian, 4 dalam putaran penuh). Disimpulkan sebab lingkungan, bukan layar; kesimpulan ini berdasar
kecocokan waktu, bukan diagnosis yang tertangkap langsung.

Uji manual: `PASS` — dengan batas di bawah.

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/lab-microbiology-result-fe35-rules.test.mjs — PASS`

`MANUAL TEST: PASS pada build produksi terhadap backend lokal BE-LAB-67/68 (409 sungguhan); 403 asli dan akun tanpa izin hasil NOT FEASIBLE — lihat di bawah`

**Tidak dijalankan:**

- **`AC-221` bagian antarmuka dengan akun asli.** Belum ada jabatan di dev yang memegang
  `LabExaminationResult : Update` (`BE-LAB-67.md` bagian 8), dan superadmin melewati pemeriksaan izin. Tampilan
  baca-saja dibuktikan dengan daftar izin yang disuapkan dalam bentuk respons `/v1/auth/permissions`.
- **`403` asli dari backend.** Alasan yang sama; bentuk badannya diambil dari `AccessPermissionFilter.cs`.
- **Buka Kembali yang berhasil** dan **Final yang berhasil** lewat route netral. Keduanya akan menulis data
  klinis bersama; route-nya terbukti dari permintaan yang dikirim layar, dan jalur berhasilnya dibuktikan
  `BE-LAB-68` bagian 8 (`ReopenCount` naik, Final ulang).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-228` — halaman Mikrobiologi menerima `409`: pesan terbaca; nilai yang diketik tetap di isian | Terpenuhi | Uji unit 8/8; layar S1-S3 terhadap `409` sungguhan |
| `AC-221` bagian antarmuka — akun tanpa izin hasil melihat halaman baca-saja | Terpenuhi — dengan izin disuapkan (S5) dan, sejak 2026-10-01, dengan **dua akun analis asli** | Layar S5; bagian 9 |
| Verifikasi roadmap — unit test pemetaan `409` dan pelestarian isian | Terpenuhi | 8/8 |
| Verifikasi roadmap — dijalankan terhadap backend `BE-LAB-68`: simpan hasil Final ditolak dan nilai tetap di isian | Terpenuhi | S1 |
| DoD — nol rujukan tersisa ke `/result/microbiology/finalize\|reopen\|consultation` | Terpenuhi | `grep` nol hasil |
| DoD — lint dan build hijau | Terpenuhi — 0 error; 1 warning lama dari HEAD | Bagian 6 |
| DoD — laporan `FE-LAB-35.md` | Terpenuhi | Berkas ini |
| Rilis bersama `BE-LAB-67` dan `BE-LAB-68` | **Belum** — belum ada yang di-deploy | Bagian 8 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Warning lint lama pada efek pengisi ulang formulir (`use-lab-microbiology-result-editor.jsx`), ada sejak HEAD; tidak diperbaiki karena di luar lingkup |
| Masalah yang diketahui | **1. Risiko rilis — wajib serempak.** Dirilis tanpa backend, Final/Buka Kembali/konsultasi memanggil route yang belum ada; backend dirilis tanpa ini, halaman memanggil route yang sudah dicabut (`404`). **2. Izin harus diberikan di jendela rilis yang sama** (`FR-14.13`, `backend-roadmap.md` 6aj.5): hari ini nol jabatan memegang `LabExaminationResult : Update`, sehingga tanpa pemberian itu **seluruh analis akan melihat halaman baca-saja**. **3. Pengamatan di luar lingkup:** bagian *Informasi Specimen* (`FE-LAB-32`) tidak disaring izin di layar — tombol *Simpan Koreksi Specimen* tampil bagi pembaca yang hanya memegang `LabSpecimen : Read`; penegakannya tetap `403` backend. Tidak diubah karena bukan cakupan task ini |
| Dependency backend | `BE-LAB-67` ⚠ — `AC-221` sisi penolakan kini terbukti dengan akun asli (bagian 9 [`BE-LAB-67.md`](../backend/BE-LAB-67.md)); tersisa `AC-222`, menunggu langkah rilis 6aj.5. `BE-LAB-68` ✅ |
| Perubahan sampingan | Verifikasi layar memakai skrip Playwright di luar repository (bukan `playwright test`), sehingga `test-results/` tidak tersentuh. **2026-10-01:** satu baris di luar lingkup ditambahkan pada `src/utils/menu-sidebar/menu-items.jsx` — impor `RiSafeLine`. Ikon itu dipakai tanpa diimpor oleh commit `5adc9f3160` di `origin/QuilvianIntegrationFrontend` dan ikut masuk lewat merge `e613321c5`; akibatnya **seluruh halaman `500`** (`ReferenceError: RiSafeLine is not defined`) dan verifikasi tidak dapat berjalan. Perbaikan yang sama perlu dikirim ke branch integrasi |
| Interupsi | Sesi sebelumnya terputus sesudah implementasi dan lint. Dilanjutkan dari `git diff` yang diperiksa ulang; nol pekerjaan diulang. Backend lokal dan server standalone dinyalakan untuk verifikasi lalu dimatikan |
| Status Git | Pekerjaan awal `FE-LAB-35` ter-commit pemilik modul pada `82ee64597` (branch `YogaV2`). Sesudah verifikasi susulan 2026-10-01, frontend (`e613321c5`): ` M` `lab-microbiology-result-slice.jsx` dan `tests/unit/lab-microbiology-result-fe35-rules.test.mjs` (perbaikan pembatalan), ` M` `src/utils/menu-sidebar/menu-items.jsx` (impor `RiSafeLine`). Backend (`55b032b0`): laporan ini, `BE-LAB-67.md`, `backend-roadmap.md`, `frontend-roadmap.md`, `traceability.md`. **Nol operasi Git dijalankan** |
| Langkah berikutnya | Rilis `MVP-8a` serempak: deploy `BE-LAB-67` + `BE-LAB-68` + `FE-LAB-35`, dan pada jendela yang sama beri `LabExaminationResult : Update` kepada jabatan analis (6aj.5); lalu jalankan `AC-222` dan bagian `200` `AC-221` dengan akun asli |

---

## 9. Verifikasi susulan — 2026-10-01, akun analis asli

Pemilik modul menyediakan **dua akun analis Laboratorium** di `QuilvianNewDevYoga` (kredensial tidak
dicatat di sini). Pengecekan dijalankan pada `localhost`, persis seperti pemakaian sehari-hari:
frontend `next dev` di `http://localhost:3000` (lewat `.env.local`) dan backend hasil merge `55b032b0`
di `https://localhost:7184`. Agar backend hasil merge dapat start, 22 migration modul lain yang
tertunda diterapkan ke `QuilvianNewDevYoga` atas instruksi pemilik modul (rincian di bagian 9
[`BE-LAB-67.md`](../backend/BE-LAB-67.md)).

### 9.1 Yang dialami analis di layar

| Langkah | Hasil sebenarnya (kedua akun sama) |
| --- | --- |
| Login lewat halaman login | Berhasil, masuk ke dashboard |
| Izin dari `/v1/auth/permissions` | 28 izin Laboratorium; **ada** `LabExamination : Update`, **tidak ada** `LabExaminationResult : Update` |
| Pantau Mikrobiologi | Terbuka, 5 order |
| Daftar Kerja | Terbuka, 9 baris |
| Halaman hasil Mikrobiologi (order uji BTA) | Catatan baca-saja tampil; **nol** tombol Simpan Draft, Simpan Final, Buka Kembali, Catat Konsultasi; **nol** permintaan tulis |
| `403` asli backend bila tindakan hasil dipaksa lewat API | Badan `ApiResponse` dengan kalimat *"Anda tidak memiliki akses ke menu atau fitur ini."* — **bentuk yang sama** dengan respons suapan pada S4, sehingga pemetaan ke kalimat kontrak berlaku untuk `403` sungguhan |

### 9.2 Cacat lama yang ditemukan dan diperbaiki

**Gejala.** Alert merah *"Hasil Mikrobiologi gagal dimuat."* tampil padahal hasilnya termuat (`GET`
menjawab `200`, Kelengkapan dan Analis terisi). Muncul untuk analis **dan** superadmin pada `next dev`,
jadi bukan soal izin.

**Sebab.** Hook editor membatalkan permintaan hasil saat cleanup (`request.abort()`). Redux Toolkit
lalu mengirim `rejected` **tanpa payload**, dan reducer (sejak `FE-LAB-31`) memasang kalimat cadangan
"gagal dimuat". Penolakan itu tiba **sesudah** permintaan penggantinya dimulai, sedangkan `fulfilled`
penggantinya tidak membersihkan galat. Di `next dev` hal ini selalu terjadi karena StrictMode
menjalankan efek dua kali. Di produksi terjadi bila analis berpindah pemeriksaan sebelum hasil pertama
termuat.

**Perbaikan.** Pola baku repository (299 pemakaian): `if (action.meta?.aborted) return;` di awal
reducer penolakan keenam thunk baca yang dapat dibatalkan di
`lab-microbiology-result-slice.jsx` — hasil, dokter konfirmator, jejak perubahan specimen, dan tiga
katalog. Pada ketiga katalog, penolakan karena pembatalan juga tidak lagi mematikan penanda memuat
milik permintaan penggantinya.

| Bukti | Hasil | Klasifikasi |
| --- | --- | --- |
| 3 uji baru di `tests/unit/lab-microbiology-result-fe35-rules.test.mjs`: urutan pembatalan, penolakan sungguhan tetap tampil, penanda memuat katalog | **11/11** | `PASS` |
| `npx eslint` pada kedua berkas | Nol masalah | `PASS` |
| `npm run lint:errors` (seluruh repository, sesudah impor `RiSafeLine`) | Exit 0 | `PASS` |
| Layar `next dev`, akun analis — buka halaman hasil lalu Muat Ulang | Alert **tidak** muncul lagi; sebelum perbaikan selalu muncul | `PASS` |

### 9.3 Temuan di luar lingkup task

| Temuan | Dampak | Pemilik |
| --- | --- | --- |
| Jabatan analis tidak memegang `LabExaminationResult : Update` | Sesudah `MVP-8a` dirilis, analis tidak dapat mengisi hasil sampai izin diberikan | **Diselesaikan di dev 2026-10-01** (6aj.5 langkah 1 dan 3): izin diberikan kepada *Analis Laboratorium* saja; layar hasil kini menampilkan kontrol tulis, konsultasi atas hasil Final menampilkan `409` `VAL-121` sungguhan dan nama tetap di isian |
| `GET /health-services/master-data/measurements/options` menjawab `403` bagi analis | Pilihan *Satuan* pada Informasi Specimen (dan satuan kadar antibiogram) kosong bagi analis | **Diselesaikan di dev 2026-10-01** atas instruksi pemilik modul: `Measurement : Read` diberikan kepada *Analis Laboratorium*; endpoint `200` |
| Lima data dashboard menjawab `403` bagi analis (pasien, dokter, poliklinik, jadwal dokter, kunjungan) | Dashboard menampilkan *"Data dashboard belum dapat dimuat sepenuhnya."* | **Sebagian diselesaikan 2026-10-01:** `PatientEncounter : Read` diberikan, data kunjungan termuat. Empat lainnya dijaga kebijakan Identity `KioskRead` (role `SuperAdmin`/`Administrator`/`Kiosk`), **bukan** izin jabatan — tidak dapat dibuka lewat Akses Role; dashboard kini menulis *"Sebagian sumber dashboard belum tersedia."*. Pemilik: dashboard dan modul data induk |
| Akun bertanda `mustChangePassword`, tetapi frontend langsung meneruskan ke dashboard | Penggantian sandi pertama tidak dipaksakan | Pemilik autentikasi |
| `SESSION_SIGNING_SECRET` tidak ada di `.env` maupun environment mesin | `next dev` tidak dapat membuat sesi tanpa nilai yang diberikan saat menjalankan | Pengembang yang menjalankan lokal |
