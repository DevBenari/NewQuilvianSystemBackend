# Laporan Perubahan Frontend — `FE-LAB-28`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-28` |
| Judul | Layar laporan Patologi Anatomi |
| Slice | `S4c` — gelombang `MVP-6b2` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian 6e.5 |
| Trace | `FR-13.12`..`FR-13.17`; `LAB-FE-022`..`LAB-FE-029`; `LAB-DEC-085`, `LAB-DEC-086`, `LAB-DEC-088`; `INV-36`, `INV-38`, `INV-40`; `AC-146`..`AC-152`; `VAL-95`, `VAL-97`, `VAL-100` |
| Contract version | `LAB-API-v1` **`r25`** bagian 20 — `approved` 2026-09-18 |
| Wewenang UI | Satu layar baru di bawah daftar pantau Patologi Anatomi, satu aksi baru pada daftar itu. **Satu perbaikan cacat `FE-LAB-31`** dilaporkan terpisah pada bagian 7 |
| Dependency | `BE-LAB-52` ✅ (2026-09-18); **pemetaan jenis pemeriksaan terisi** ✅ (2026-09-23, bagian 2) |
| Klasifikasi | `HEAVY` — 7 acceptance criteria, 8 invariant UI, 1 layar berwilayah enam |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `1629bf025` (branch `YogaV2`) |
| Tanggal | 2026-09-23 |
| Status | ✅ **`SELESAI`** (naik 2026-10-02) — ketujuh AC terbukti **di peramban** dengan akun analis asli (Vina) terhadap backend lokal dan PostgreSQL dev, 13/13. `Buka Kembali`, `Simpan`, dan `Selesaikan` **dijalankan sungguhan** pada pesanan uji `LAB-RSMMC-000009` yang diizinkan pemilik modul — ketiganya `200`. Lihat 9. *(Semula 2026-09-23: ⚠ — 15 uji unit, aturan murni atas jawaban server sungguhan; belum diklik di peramban; tiga jalur tulis sengaja nol dijalankan.)* |

---

## 1. Masalah yang diperbaiki

Backend laporan Patologi Anatomi berdiri sejak `BE-LAB-52` (2026-09-18): formulir yang
dibangkitkan dari data induk, simpan, selesaikan, dan buka kembali. **Nol satu pun layar
memanggilnya**, sehingga patolog nol punya tempat menulis laporan sama sekali.

Task ini sempat berstatus `MENUNGGU PEMETAAN TERISI` — dan penahan itu nyata, bukan formalitas:
pesanan yang jenis pemeriksaannya belum digolongkan **nol menyumbang satu pun ruas**, sehingga
formulirnya kosong. Membangun layar di atas pemetaan yang kosong menghasilkan layar yang benar
tetapi tidak dapat dilihat bekerja.

---

## 2. Pemetaan jenis pemeriksaan — dikerjakan lebih dulu

Atas instruksi pemilik modul, enam jenis pemeriksaan yang tersisa digolongkan lewat
`POST /lab-procedure-pathology-categories` memakai akun superadmin. **Setiap usulan ditinjau,
bukan diterima begitu saja** — itulah inti `LAB-DEC-087`.

| Jenis pemeriksaan | Usulan kata kunci | Disimpan | Penilaian |
| --- | --- | --- | --- |
| Histopatologi Biopsi Kecil | `HISTO` | `HISTO` | Tepat |
| Histopatologi Jaringan Operasi | `HISTO` | `HISTO` | Tepat |
| Imunohistokimia HER2 | `IHK` | `IHK` | Tepat |
| Imunohistokimia PR | `IHK` | `IHK` | Tepat |
| Liquid-Based Cytology (LBC) | `SITO_GIN` | `SITO_GIN` | ⚠ **Perlu ditinjau** — lazimnya serviks, tetapi LBC juga dipakai untuk bahan non-ginekologi |
| Sitologi FNAB | **nol usulan** | `SITO_NONGIN` | ⚠ **Perlu ditinjau** — FNAB aspirasi jarum halus dari massa (tiroid, payudara, KGB), sehingga non-ginekologi; sejalan dengan *Sitologi Cairan Tubuh → SITO_NONGIN* yang sudah ada |

Ringkasan sesudahnya: **`10 / 10 / 0`** — sepuluh terpetakan, nol tersisa. Angka itu sekaligus
membuktikan `AC-194` milik `BE-LAB-66`: `unmappedProcedure` turun **6 → 0 pada enam
penyimpanan**.

> **Status data ini: DATA UJI, bukan penggolongan resmi rumah sakit.** Blueprint menetapkan isi
> pemetaan sebagai keputusan kepala instalasi bersama `DR-LAB-003` (dr. Citra Maharani, Sp.PA),
> bukan programmer. Keenamnya disimpan pada database dev atas instruksi pemilik modul supaya
> layar ini dapat dibuktikan bekerja, dan **menunggu tinjauan klinis** — terutama LBC dan FNAB.
> Grup ini **nol punya `DELETE`**; penggolongan yang keliru dipindahkan lewat `PUT /{id}`.
>
> `LAB-OPEN-039` mencatat FNAB sebagai **varian cetak tersendiri**. Bila bentuk laporannya
> memang berbeda dari sitologi non-ginekologi, yang dibutuhkan adalah **golongan kelima**, bukan
> memindahkan FNAB ke salah satu dari empat yang ada.

---

## 3. Proses bisnis

**Pelaku.** Patolog (`LabExamination : Update`).

**Langkah berurutan:**

1. Patolog membuka daftar pantau Patologi Anatomi dan menekan **Buka Hasil** pada satu baris.
2. Layar laporan terbuka per **pesanan** (`LAB-DEC-085`) — nol pemilih pemeriksaan di tengahnya.
3. Patolog membaca identitas (A), konteks klinis dari dokter pemesan (B), dan pemeriksaan
   beserta golongannya (C) — ketiganya **baca-saja**.
4. Patolog mengisi ruas laporan (D) yang **dibangkitkan dari golongan pesanan**, memilih tingkat
   temuan (E), lalu **Simpan**.
5. Sesudah seluruh ruas wajib terisi dan tersimpan, patolog menekan **Selesaikan**. Laporan
   terkunci. **Ini bukan rilis** — hasil belum sampai ke dokter pemesan maupun pasien.
6. Bila perlu diperbaiki, **Buka Kembali** beserta alasannya.

**Contoh berangka dari data sungguhan.** Satu pesanan bergolongan `SITO_NONGIN` membangkitkan
**3 ruas**, ketiganya wajib dan masih kosong — `Selesaikan` tertahan sampai ketiganya terisi dan
tersimpan. Pesanan lain yang memuat **empat golongan** membangkitkan
**15 ruas**, dan `MAKROSKOPIK` yang dipakai Histologi *dan* Sitologi Non-Ginekologi **muncul
sekali**, bukan dua kali.

---

## 4. Perubahan yang dikerjakan

### 4.1 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `lib/constants/.../lab-pathology-report-constants.jsx` | **BARU** — jalur API, route, pilihan tingkat temuan, copy. **Nol daftar ruas** |
| `lib/services/.../lab-pathology-report.service.js` | **BARU** — 4 fungsi. Nol validasi, nol rilis, nol kirim |
| `lib/hooks/.../lab-pathology-report-rules.js` | **BARU** — 7 fungsi murni |
| `lib/state/slice/.../lab-pathology-report-slice.jsx` | **BARU** — 4 thunk, satu `actionLoading` bersama |
| `lib/hooks/.../use-lab-pathology-report.jsx` | **BARU** — memuat pesanan, pemeriksaan, konteks, laporan |
| `components/view/.../anatomic-pathology/lab-pathology-report-view.jsx` | **BARU** — wilayah A–F |
| `app/.../anatomic-pathology/[slug]/pathology-report/page.jsx` + `route-token.js` | **BARU** |
| `lib/hooks/.../use-lab-monitoring.jsx` | **+1 aksi** `openPathologyReport` |
| `components/view/.../lab-monitoring-view.jsx` | Gerbang disiplin kini memasang aksi pada **Patologi Anatomi** pula; variabel dinamai ulang dari `openMicrobiologyResult` yang menyesatkan menjadi `openResultAction` |
| `lib/state/store.jsx` | **+1 registrasi** |
| `components/view/.../lab-microbiology-result-form.jsx` | **Perbaikan cacat `FE-LAB-31`** — bagian 7 |
| `tests/unit/lab-pathology-report-fe28-rules.test.mjs` | **BARU** — 15 uji |

**Konteks klinis memakai slice `FE-LAB-29` apa adanya** — nol sumber kedua bagi data yang sama.

### 4.2 Lima keputusan pelaksanaan

**1. `Selesaikan` dimatikan selama ada perubahan yang belum tersimpan.** Backend menguji
kelengkapan terhadap isi yang **tersimpan**. Tanpa penjaga ini patolog dapat melihat isi A di
layar sementara yang dinyatakan selesai adalah isi B di server — dan laporan yang terkunci berisi
sesuatu yang tidak pernah ia baca ulang. Menyimpan diam-diam sebelum menyelesaikan **juga
ditolak**, sebab `LAB-FE-024` justru menuntut keduanya dua tindakan yang disadari.

**2. `analystUserId` yang tersimpan dikirim ulang walau layar nol punya pemilihnya.** `PUT`
mengganti **seluruh** isi laporan. Formulir yang nol mengirim ruas itu akan mengosongkan
penanggung jawab analis hanya karena patolog menyunting satu kalimat.

**3. Pemilih "Penanggung Jawab Analis" nol dibangun, dan itu disengaja.** Ruasnya menunjuk
`AspNetUsers`, sedangkan resource pemilih yang tersedia — `employees` — mengembalikan id
**pegawai**, tipe yang berbeda. Membangunnya berarti menebak sumber data; skill ini melarangnya.
Lihat bagian 7.

**4. Payload hanya memuat ruas yang diumumkan server.** Kunci lain di keadaan formulir diabaikan.
Backend menolak parameter yang tidak berlaku bagi golongan pesanannya (`VAL-96`), dan layar nol
boleh menjadi tempat kedua yang tahu ruas mana milik golongan mana.

**5. `Buka Kembali` memakai `ConfirmModal` bawaan dengan `requireReason`.** Komponen itu sudah
menahan tombol konfirmasinya sampai alasan terisi. Pemeriksaan di hook tetap dijalankan — penjaga
yang hanya hidup di komponen tampilan akan hilang begitu komponennya diganti.

---

## 5. Dokumentasi endpoint

#### Health Services / Laboratory Management / Lab Pathology Report

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/lab-orders/{id}/pathology-report` | Laporan beserta bentuk formulirnya | `LabExamination : Read` |
| `PUT` | `/lab-orders/{id}/pathology-report` | Menyimpan — **penggantian utuh** | `LabExamination : Update` |
| `POST` | `/lab-orders/{id}/pathology-report/finalize` | Menyatakan selesai ditulis. **Bukan rilis** | `LabExamination : Update` |
| `POST` | `/lab-orders/{id}/pathology-report/reopen` | Membuka kembali, **beralasan** | `LabExamination : Update` |
| `GET` | `/lab-orders/{id}/pathology-context` | Konteks klinis, baca-saja di layar ini | `LabOrder : Read` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | **0 error** | `PASS` | Keluaran perintah |
| Uji unit berkas ini | **15/15 lulus** | `PASS` | Keluaran perintah |
| Seluruh suite unit | **1674 lulus, 7 gagal** | `PASS` | Naik dari 1659; gagal **tetap 7**, seluruhnya di luar Laboratorium |
| `npm run build` | **Hijau**, route laporan terkompilasi | `PASS` | `✓ Compiled successfully` |
| `GET /pathology-report` keempat pesanan PA | **Keempatnya `200`**, 15 / 3 / 3 / 3 ruas | `PASS` | Respons HTTP |
| **Aturan murni dijalankan terhadap jawaban server sungguhan** | Lihat bawah | `PASS` | Skrip `node` atas JSON respons |
| `Simpan`, `Selesaikan`, `Buka Kembali` terhadap server | **Nol dijalankan** | `NOT RUN` | Disengaja — lihat bawah |

**Aturan murni atas data sungguhan** — bukan fixture yang ditulis sendiri:

| Pesanan | Ruas | Golongan | Ruas bersama | Duplikat | Terkunci | Wajib kosong | Kunci payload |
| --- | ---: | --- | --- | ---: | --- | ---: | --- |
| 4 golongan, final | **15** | HISTO, SITO_NONGIN, SITO_GIN, IHK | `MAKROSKOPIK`, `MIKROSKOPIK`, `KESIMPULAN` [HISTO+SITO_NONGIN]; `ANJURAN` [IHK+SITO_GIN] | **0** | **ya** | 0 | `findingStatus`, `analystUserId`, `values` |
| 1 golongan, baru | **3** | SITO_NONGIN | — | 0 | tidak | **3** | sama |

**Kenapa `Simpan`/`Selesaikan`/`Buka Kembali` nol dijalankan.** Keempat pesanan PA di database
dev adalah pesanan pasien sungguhan. `PUT` pertama **membuat** baris laporan yang nol punya jalan
dihapus, dan `Buka Kembali` menambah `ReopenCount` yang **nol dapat dikembalikan**. Instruksi
pemilik modul memberi wewenang menulis **pemetaan**, bukan isi laporan klinis. Ketiga jalur itu
sudah terbukti di sisi backend oleh `BE-LAB-52` (`AC-136`..`AC-142` terhadap aplikasi berjalan).

Uji manual: **`NOT FEASIBLE`** — sesi ini nol punya alat kendali peramban.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-146` formulir **dibangkitkan dari daftar parameter server**, nol daftar ruas ditulis di kode | ✅ **Terbukti atas data sungguhan** | 15 ruas dan 3 ruas terbangkitkan dari dua pesanan nyata; berkas konstanta nol memuat nama ruas; 4 uji |
| `AC-147` **nol tombol Validasi/Rilis/Kirim** | ✅ **Terpenuhi** | Service hanya memuat 4 fungsi; tampilan hanya merender Simpan, Selesaikan, Buka Kembali |
| `AC-148` `Selesaikan` **terpisah** dari `Simpan` dan disertai penegasan ia bukan rilis | ✅ **Terbukti di peramban** (R7, R10) | Dua tombol; penegasan selalu tampak di sebelahnya, bukan hanya di dalam dialog |
| `AC-149` konteks klinis **baca-saja** | ✅ **Terbukti di peramban** (R4) | Wilayah B dirender sebagai teks, nol isian; kosong bertanda *"Belum diisi dokter pemesan"* |
| `AC-150` Waktu Efektif dan Issued **baca-saja bertanda turunan**, nol kotak isian | ✅ **Terbukti terbalik** | Payload atas data sungguhan nol memuat keduanya; 2 uji |
| `AC-151` `Buka Kembali` **meminta alasan** | ✅ **Terbukti di peramban** (R6) | `ConfirmModal requireReason` + penjaga di hook; 1 uji; tombol mati tanpa alasan, dengan alasan → `200` |
| `AC-152` pesanan tanpa pemetaan menampilkan **sebab dan siapa yang mengatur** | ✅ **Terbukti di peramban** (R11) | 3 uji; keadaannya kini ada pada data dev (pesanan yang jenis pemeriksaannya belum digolongkan) dan kalimatnya tampil |
| DoD — isi laporan **tidak muncul** pada layar non-klinis | ✅ **Terpenuhi** | Satu-satunya pemanggil `pathology-report` adalah layar ini |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| **Cacat `FE-LAB-31` ditemukan dan diperbaiki** | Dialog hapus isolat (`AC-126`) mengirim `onClose` kepada `ConfirmModal`, **padahal komponen itu nol mengenal `onClose`** — yang dikenalnya `onHide` dan `onCancel`. Akibatnya **tombol Batal dan Esc nol berbuat apa-apa**, dan satu-satunya jalan keluar dari dialog itu adalah menekan **"Ya, Hapus"**. Dialog yang dibuat untuk mencegah penghapusan tak sengaja justru memaksanya. Ditemukan saat memasang `ConfirmModal` yang sama pada layar ini; diperbaiki satu kata. Dilaporkan terbuka, bukan ditambal diam-diam |
| **Cacat yang sama di modul lain — dilaporkan, NOL diperbaiki** | `inpatient-management/doctor-inpatient/my-notes/my-drafts-tab.jsx:306`, `inpatient-management/doctor-inpatient/needs-review/physician-needs-review-view.jsx:337`, `radiology-management/rad-orders/form/rad-order-form-view.jsx:291`. Di luar cakupan dan wewenang tulis Laboratorium |
| **Pemilih analis nol dibangun** | Butuh resource pemilih yang mengembalikan id `AspNetUsers` bagi staf laboratorium. Nilai yang sudah tersimpan tetap terjaga |
| **Satu ketiadaan data dicatat** | Ringkasan pesanan membaca `patientName` dan `medicalRecordNumber` dari detail pesanan. Ruas itu ada pada jalur **daftar pantau**; keberadaannya pada jalur **detail** belum dilihat di peramban. Bila kosong, wilayah A menampilkan `-`, bukan galat |
| Peringatan | Nol peringatan lint baru |
| Risiko tersisa | **Sedang, dan terpusat pada kata-kata.** Label `Selesaikan` dan penegasan "bukan rilis" adalah satu-satunya yang mencegah patolog mengira hasilnya sudah sampai ke dokter. Keduanya bukan `DEV_DISCRETION` |
| Perubahan sampingan | `NONE` di luar perbaikan cacat yang dilaporkan di atas |
| Status Git | Seluruh berkas dalam cakupan. **Nol operasi Git dijalankan** |
| **Catatan lingkungan** | Backend kini membutuhkan `version.json` yang **tidak lagi di-track** sejak `b17d1dfc` (dibangkitkan CI). Untuk pengujian ini ia dipulihkan sementara dari `b17d1dfc^` lalu **dihapus lagi** — tree backend bersih. Build penuh backend kini ~30 menit dan ~27 GB RAM akibat empat migration `Designer.cs` baru berukuran ~116.000 baris |
| Langkah berikutnya | **1.** Tinjauan klinis atas pemetaan LBC dan FNAB oleh `DR-LAB-003`. ~~**2.** Klik layar terhadap pesanan uji — bukan pesanan pasien sungguhan.~~ (selesai 2026-10-02 pada `LAB-RSMMC-000009`, lihat 9) **3.** Putuskan sumber pemilih analis |

---

## 9. Verifikasi susulan 2026-10-02 — di peramban dengan akun asli, tulis pada pesanan uji

**Lingkungan.** Backend lokal (`dotnet run`, `Development`) terhadap PostgreSQL dev bersama; `next dev`
port 3000 dari working tree `YogaV2`; Chromium lewat Playwright; login **lewat formulir** dengan akun
**Vina (analis Laboratorium)**.

**Wewenang tulis.** Pemilik modul memilih pesanan **`LAB-RSMMC-000009`** sebagai pesanan uji Patologi
Anatomi dan mengizinkan tulis padanya. Penjaga tulis **meneruskan hanya** `reopen`, `PUT`, dan
`finalize` milik pesanan itu; selebihnya digagalkan. Nilai yang ditulis bertanda uji
(`… [uji layar FE-LAB-28 2026-10-02]`), sehingga dapat dikenali kemudian.

| ID | Skenario | Hasil | Bukti |
| --- | --- | --- | --- |
| R0 | Login Vina lewat formulir | `PASS` | — |
| R1 | `AC-146` formulir dibangkitkan dari ruas server — jumlah dan nama sama dengan jawaban backend | `PASS` | 15 ruas: Makroskopik, Mikroskopik, Kesimpulan, … , Ki-67, status reseptor |
| R2 | Laporan `Final`: isian terkunci; hanya `Buka Kembali`; nol `Simpan`/`Selesaikan` | `PASS` | — |
| R3 | `AC-147` nol tombol `Validasi`/`Rilis`/`Kirim` | `PASS` | 0 |
| R4 | `AC-149` konteks klinis dokter pemesan tampil baca-saja | `PASS` | 0 isian |
| R5 | `AC-150` Waktu Efektif dan Waktu Issued sebagai teks bertanda turunan | `PASS` | "Diturunkan sistem — tidak dapat diubah." |
| R6 | `AC-151` `Buka Kembali`: tombol mati tanpa alasan; dengan alasan → `POST …/reopen` **`200`**; isian dapat disunting | `PASS` | Badan `{reason}`; pesan backend "Laporan Patologi Anatomi berhasil dibuka kembali." |
| R7 | `AC-148` sesudah dibuka: `Simpan` dan `Selesaikan` terpisah; penegasan "BUKAN rilis" tampak di sebelahnya | `PASS` | — |
| R8 | `Simpan`: `Selesaikan` mati selama ada perubahan; `PUT` → **`200`** | `PASS` | Kunci badan `findingStatus`, `analystUserId`, `values` |
| R9 | Muat ulang: nilai tersimpan terbaca dari backend | `PASS` | Akhiran `[uji layar FE-LAB-28 2026-10-02]` |
| R10 | `Selesaikan`: `POST …/finalize` → **`200`**; terkunci lagi; nol tombol rilis | `PASS` | "Laporan Patologi Anatomi berhasil diselesaikan." |
| R11 | `AC-152` pesanan tanpa ruas: sebab tertulis — **apa** yang belum diatur dan **siapa** yang mengatur | `PASS` | "…belum digolongkan… diatur kepala instalasi laboratorium lewat menu Data Master → Penggolongan Jenis Pemeriksaan PA" |
| R12 | 390 px: tanpa gulir horizontal halaman | `PASS` | `375 ≤ 390` |
| R13 | Nol tulis di luar pesanan uji | `PASS` | Tiga tulis diteruskan, seluruhnya milik `LAB-RSMMC-000009` |

**Jejak di database dev.** `LAB-RSMMC-000009`: satu kali dibuka kembali (`ReopenCount` naik satu), satu
nilai laporan berakhiran penanda uji di atas, lalu diselesaikan lagi — keadaan akhirnya `Final`, sama
dengan keadaan awal.

**Nol perubahan kode** diperlukan untuk task ini. Validasi akhir sesi: uji unit 2303/2309 (6 gagal =
baseline, nol Laboratorium), `lint:errors` 0 error, `npm run build` hijau. **Nol operasi Git dijalankan.**
