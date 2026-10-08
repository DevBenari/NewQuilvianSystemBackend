# Laporan Perubahan Frontend — `FE-LAB-49`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-LAB-49` |
| Judul | Susunan menu Laboratorium dan judul halaman mengikuti FE v1 |
| Slice | Susulan `S15` — amendment pass putaran 22 |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md) bagian *Susulan putaran 22* |
| Trace | BR-137; `LAB-DEC-192` (mengamandemen label `LAB-DEC-048`); `LAB-FE-031`; rekomendasi Nota diterima pemilik modul 2026-10-07; capability map rev 7 `CAP-P22-16`..`-20` |
| Contract version | Nol endpoint. `LAB-PERM-v1` revision 12 — `LabOperationalReport : Read` tetap menjaga *Laporan Operasional* |
| Wewenang UI | `LAB-FE-031` `decided` — label, urutan, pengelompokan, dan judul **wajib persis** BR-137; route, key lama, dan izin tidak boleh berubah. Yang `DEV_DISCRETION`: ikon dan nama key grup baru (`healthServicesLabReports`) |
| Dependency | Nol |
| Klasifikasi | `LIGHT` — teks menu dan judul, satu grup menu baru, satu uji baru; nol logika bisnis |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev`; laporan ini beserta status roadmap dan traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `5427ddbe4` (branch `YogaV2`), di atas `FE-LAB-46`..`48` yang belum di-commit |
| Commit backend yang dijadikan rujukan | `171dc314` (branch `yoga`) |
| Tanggal | 2026-10-07 |
| Status | ✅ **`SELESAI`** — `AC-282` dan dua AC tambahan terbukti di peramban dan lewat uji unit |

---

## 1. Keadaan yang ditemukan di awal

| Bukti | Isi |
| --- | --- |
| `menu-items.jsx` | Urutan: Ringkasan Laboratorium · Pendaftaran · Penerimaan · Laporan Penerimaan · Laporan Operasional · Daftar Kerja · Pantau Cito · Antrean Validasi · Pemeriksaan PK/PA/Mikro · Tarif · Master Data |
| Blok komentar Laboratorium | Berdiri di atas blok **Radiologi**; menyebut *"penyaringan per izin belum ditegakkan"* (sudah ditegakkan sejak `FE-LAB-44`) dan *"menu Pesanan Laboratorium"* (dicabut `LAB-DEC-048`) |
| Judul | Hero daftar `Pemeriksaan <disiplin>`; Hero Beranda *Ringkasan Laboratorium*; tab peramban *Monitoring <disiplin>* (sisa sebelum `LAB-DEC-048`) |
| Nota cetak | Subjudul = judul halaman (`disciplineTitle={title}`) — ikut berubah bila judul diganti |
| Uji | Nol uji atas menu Laboratorium maupun judulnya |

---

## 2. Proses bisnis dari sisi pengguna

**Pengguna:** seluruh petugas Laboratorium, kepala instalasi, dan manajemen.

1. Petugas membuka menu **Laboratorium** di sidebar.
2. Urutan yang terlihat sama dengan FE v1: **Beranda** · Penerimaan Sampling/Specimen · Pendaftaran Pasien
   Laboratorium · **Daftar Pasien Lab Patologi Klinik** · **Daftar Pasien Lab Patologi Anatomi** · **Daftar
   Pasien Lab Mikrobiologi** · Daftar Kerja Laboratorium · Pantau Keterlambatan Cito · Antrean Validasi ·
   **Laporan** (▸ Laporan Penerimaan, Laporan Operasional) · Tarif Laboratorium · Master Data.
3. Membuka *Daftar Pasien Lab Patologi Klinik* membawa ke halaman yang sama seperti dahulu — alamatnya tidak
   berubah — dengan judul halaman dan tab peramban berbunyi sama dengan menunya.
4. Mencetak Nota dari daftar tetap menghasilkan kertas bersubjudul *Pemeriksaan Patologi Klinik*.

**Jalur tidak normal.**

| Keadaan | Yang terjadi |
| --- | --- |
| Pengguna tanpa `LabOperationalReport : Read` (misalnya analis) | Grup *Laporan* tetap tampil, berisi *Laporan Penerimaan* saja |
| Seluruh anak grup *Laporan* tersaring | Grup ikut hilang — grup tidak menunjuk halaman sendiri |
| Tautan atau penanda lama ke route Laboratorium | Tetap berjalan: nol `pathname` berubah |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`menu-items.jsx`; `filter-menu-items-by-permission.jsx`; `filter-menu-items-by-role.jsx`;
`left-sidebar-items-virtualized.jsx`; `lab-monitoring-constants.jsx`; `use-lab-monitoring.jsx`;
`lab-monitoring-view.jsx`; `lab-order-print-document.jsx`; `laboratory-overview-view.jsx`; empat
`page.jsx`; `lab-monitoring.service.js`; `tests/unit/menu-permission-filter.test.mjs`;
`tests/unit/petty-cash-finance-separation.test.mjs` (pola uji menu).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/menu-sidebar/menu-items.jsx` | Urutan dan label BR-137; grup **Laporan** (`subItems`, key baru `healthServicesLabReports`) berisi Laporan Penerimaan dan Laporan Operasional — `requiredPermission` Laporan Operasional ikut pindah utuh; blok komentar Laboratorium dipindah ke atas grupnya dan dibetulkan (rujukan `FR-10.1`/`LAB-DEC-192`, penyaringan izin sudah berlaku, pesanan dokter lewat *Terima Sampling*) |
| `src/lib/constants/.../lab-monitoring-constants.jsx` | `buildTitle` → `Daftar Pasien Lab <disiplin>`; **baru** `buildPrintSubtitle` → `Pemeriksaan <disiplin>`; rujukan komentar |
| `src/components/view/.../lab-monitoring-view.jsx` | Nota memakai `buildPrintSubtitle(label)`, bukan judul halaman; komentar *"Dua pilihan"* basi dan rujukan `LAB-DEC-025` dibetulkan |
| `src/components/view/.../laboratory-overview-view.jsx` | Hero *Beranda* |
| `src/app/.../lab-monitoring/{clinical-pathology,anatomic-pathology,microbiology}/page.jsx`, `src/app/.../overview/page.jsx` | `metadata.title` sama dengan label menu |
| `src/lib/services/.../lab-monitoring.service.js` | Rujukan komentar `FR-10.1` |
| `tests/unit/lab-menu-br137.test.mjs` | **Baru.** 7 uji: urutan dan label, key dan pathname lama (dengan satu key baru), izin di dalam grup, ejaan, judul, Nota |

### 3.3 Kepatuhan arsitektur frontend

Grup *Laporan* mengikuti pola `subItems` milik *Master Data* Laboratorium yang sudah dirender sidebar, dan
penyaring izin yang sudah membaca setiap kedalaman (uji `M7`). `menu-items.jsx` hanya dibaca sidebar dan
penyaring per peran tidak menyentuh grup Laboratorium, sehingga key baru tidak berdampak di tempat lain.
Uji menu membaca teks berkas, karena berkas itu memuat JSX — preseden
`petty-cash-finance-separation.test.mjs`. Nol komponen, route, atau gaya baru.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tidak berubah |
| Kosong | Tidak berubah |
| Gagal | Tidak berubah |
| Tanpa hak akses | *Laporan Operasional* tersembunyi bagi yang tidak memegang `LabOperationalReport : Read`; grup *Laporan* tetap berisi *Laporan Penerimaan* |

---

## 5. Endpoint yang dikonsumsi

`NOT APPLICABLE` — nol panggilan API baru atau berubah.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| ESLint berkas yang disentuh | 0 error, 0 warning | `PASS` | Keluaran kosong |
| `npm run lint:errors` | Nol error | `PASS` | Keluaran kosong |
| `node --import ./tests/helpers/register.mjs --test tests/unit/` | 2499 uji: 2491 lulus, 8 gagal | `PASS` (nol kegagalan baru) | 7 uji baru lulus; kedelapan nama kegagalan identik dengan baseline 2026-10-06 |
| `npm run build` | `Compiled successfully in 47s` | `PASS` | Dijalankan saat nol server lokal hidup |
| M1 — urutan dan label di sidebar | Persis BR-137 butir 1 (12 butir) | `PASS` | Dibaca dari DOM sidebar |
| M2 — isi grup Laporan (pemegang izin) | Laporan Penerimaan · Laporan Operasional, `href` route lama | `PASS` | DOM sidebar |
| M3 — route butir daftar | *Daftar Pasien Lab Patologi Klinik* → `/lab-monitoring/clinical-pathology` | `PASS` | `href` |
| M4 — tanpa `LabOperationalReport : Read` | Grup Laporan tetap, hanya *Laporan Penerimaan* | `PASS` | Daftar izin disuapkan tanpa resource itu |
| J — judul Hero dan tab peramban | *Beranda*, *Daftar Pasien Lab Patologi Klinik/Patologi Anatomi/Mikrobiologi* — Hero dan tab sama | `PASS` | `heading` dan `document.title` empat halaman |
| N1 — Nota | Subjudul *Pemeriksaan Patologi Klinik*; nol kata *Daftar Pasien Lab* di pratinjau | `PASS` | Pratinjau Nota |
| Penjaga tulis, galat halaman | Nol tulis; nol galat JavaScript | `PASS` | Log skrip |

**Lingkungan:** backend lokal Development, `next dev` port 3000 ke backend lokal, DB dev bersama (baca saja).
**Akun:** superadmin; peran tanpa izin laporan lewat daftar izin yang disuapkan. Server dimatikan sesudah uji.

Uji manual: `PASS`.

AUTOMATED TEST: `node --import ./tests/helpers/register.mjs --test tests/unit/` — PASS (7 uji baru lulus; nol kegagalan baru).

**Tidak dijalankan:** akun analis asli (sandi tidak tersedia).

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AC-282` — urutan dan label BR-137 butir 1 | Terpenuhi | M1, uji unit |
| `AC-282` — seluruh `pathname` dan `key` sama dengan sebelum perubahan | Terpenuhi | Uji unit (14 key lama + 1 key grup baru; 12 pathname lama); M3 |
| `AC-282` — Laporan Operasional tetap tersembunyi tanpa `LabOperationalReport : Read` | Terpenuhi | M4, uji unit |
| `AC-282` — judul halaman Beranda dan ketiga daftar sama dengan label menunya | Terpenuhi | J, uji unit (Hero dan `metadata.title`) |
| Tambahan (a) — kertas Nota tetap bersubjudul *Pemeriksaan <disiplin>* | Terpenuhi | N1, uji unit |
| Tambahan (b) — daftar `key` dan `pathname` lama sebelum = sesudah | Terpenuhi | Uji unit |
| BR-137 butir 5 — ejaan *Mikrobiologi* | Terpenuhi | Uji unit |
| DoD — uji, lint, build, laporan | Terpenuhi | Bagian 6 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Nol |
| Masalah yang diketahui | `03-frontend-architecture.md` 10.2 masih memuat label lama — coverage gap milik `design-business-module`, tercatat pada traceability putaran 22 |
| Dependency backend | `NONE` |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Frontend: 14 berkas `M`, 4 berkas baru `??` — gabungan `FE-LAB-46`..`49`, belum di-commit |
| Langkah berikutnya | Seluruh task frontend putaran 22 selesai pada kode. Sisa: `BE-LAB-89` (tidak memblokir), pencatatan `Q-P22-01`..`03` lewat `/grill-me`, dan penyelesaian sungguhan `FE-LAB-48` bila data tersedia |
