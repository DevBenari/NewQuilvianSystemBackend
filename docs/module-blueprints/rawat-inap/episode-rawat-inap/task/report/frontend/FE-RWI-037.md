# Laporan Perubahan Frontend — `FE-RWI-037`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-037` |
| Judul | Census mempunyai jalan kerja saat berisi maupun kosong |
| Slice | `F12 — Repair layar existing` |
| Roadmap | [`roadmap/frontend-roadmap.md`](../../../roadmap/frontend-roadmap.md), task `FE-RWI-037` |
| Trace | Bukti runtime pemilik 28 Agustus 2026; `FE-INP-01`; skema §8 dan §24.1; `EPIC RI-24`; `FR-RI-113` s.d. `FR-RI-115` |
| Contract version | API `0.4.0`; source backend pada commit `e9c4c78d8`. Tidak ada endpoint atau bentuk respons baru pada task ini |
| Wewenang UI | Susunan wilayah, kolom, tujuan aksi, dan state mengikuti skema §8 yang disetujui pemilik pada 1 September 2026; detail visual `DEV_DISCRETION` |
| Dependency | `FE-RWI-033` ✅ selesai. `RWI-UI-GAP-007` masih terbuka dan membatasi pembuktian dengan data runtime |
| Klasifikasi | `MEDIUM` — empat berkas existing diubah, dua berkas source baru ditambahkan, tanpa perubahan komponen bersama dan tanpa perubahan backend |
| Task mode | `FRONTEND` — source backend read-only; wewenang lintas repository hanya laporan, roadmap, dan traceability modul ini |
| Target tulis | `QuilvianSystemFrontendDev` untuk source; folder laporan, roadmap, dan traceability Rawat Inap pada `NewQuilvianSystemBackend` |
| Model | OpenAI Codex (GPT-5) |
| Commit frontend saat dikerjakan | `fe90a8bfe413659c112274069d53171c1e09c35a` pada branch `HamzahV2` |
| Commit backend yang dijadikan rujukan | `e9c4c78d816c8079be899c9e7bddbbe45a28ecee` pada branch `MHamzah` |
| Tanggal | 1 September 2026 |
| Status | ✅ **SELESAI 1 September 2026.** Empat kriteria terpenuhi penuh dan dua kriteria permission-aware terpenuhi secara struktural dengan batas platform pada bagian 7. `npm run lint` lulus dengan `0 errors` dan 571 warning existing; `npm run build` berhasil. Test `.mjs` tidak dijalankan sesuai arahan pengguna |

---

## 1. Keadaan yang ditemukan di awal

Layar sudah membaca `GET /census` dan metadata penyaring, tetapi belum menjadi jalan kerja yang
lengkap. Tombol Detail Episode memakai kelas Bootstrap mentah, keadaan kosong selalu memberi
pesan umum, dan CTA ke admisi maupun daftar kerja episode belum ada. Ketika pembacaan gagal,
tabel kosong tetap dirender sehingga kegagalan dapat terbaca seperti census bernilai nol.

Definisi kolom juga berada di komponen halaman yang sama sehingga hierarki layar sukar dibaca.
Kolom klinis sensitif memang sudah dibatasi oleh normalizer existing; pembatasan itu dipertahankan.

Approval tampilan yang sebelumnya menjadi blocker diberikan langsung oleh pemilik pada
1 September 2026. Dependency `FE-RWI-033` juga telah selesai. Data runtime untuk membuktikan
baris `Admitted`/`DischargePending` masih menunggu `RWI-UI-GAP-007`.

---

## 2. Proses bisnis dari sisi pengguna

Petugas yang berhak membaca census membuka **Rawat Inap → Census**. Layar mengambil pilihan
unit layanan dan kelas perawatan dari metadata khusus census, lalu menampilkan pasien yang
sedang dirawat dalam urutan server.

1. Petugas dapat mencari atau mempersempit daftar dengan unit layanan dan kelas perawatan.
2. Setiap baris memperlihatkan Episode, Pasien, Lokasi, DPJP, Perawat, Hari Rawat, Status, dan
   Aksi tanpa menampilkan diagnosis, catatan episode, nomor penjamin, atau alasan isolasi.
3. Petugas memilih **Detail Episode** untuk melanjutkan ke data episode yang bersangkutan.
4. Bila belum ada pasien, layar menjelaskan bahwa admisi dapat dibuka atau episode yang masih
   tertunda dapat diperiksa melalui **Daftar Kerja Episode**.

Jalur tidak normal ditangani berbeda: hasil kosong karena penyaring meminta pengguna mengatur
ulang penyaring; gagal baca menyembunyikan tabel dan menyediakan **Coba Lagi** tanpa menghapus
penyaring; `401`/`403` tetap ditangani `AccessDeniedGate`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- Blueprint Rawat Inap: `05-skema-tampilan.md`, `roadmap/frontend-roadmap.md`,
  `roadmap/requirement-traceability.md`, dan laporan `FE-RWI-033`/`FE-RWI-036`.
- Source frontend existing: view, hook, service, constants, normalizer census, `DataTable`,
  `DataFilter`, `BaseButton`, `StatusBadge`, `InformationAlert`, serta pola tabel Administrator.
- Source backend read-only: `InpatientCensusController`, `InpatientEpisodeController`, query,
  response DTO, dan permission attribute terkait.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/health-services/inpatient-management/inpatient-census-view.jsx` | Menyusun ulang shell halaman, membedakan state kosong/gagal, menambahkan retry dan CTA kosong, serta memakai definisi kolom terpisah |
| `src/components/view/health-services/inpatient-management/inpatient-census-table-columns.jsx` | Menetapkan delapan kolom yang disetujui, badge status, dan tombol Detail Episode berbasis `BaseButton` |
| `src/lib/constants/health-services/inpatient-management/inpatient-census-constants.jsx` | Menambahkan copy state kosong normal dan hasil filter kosong |
| `src/lib/hooks/health-services/inpatient-management/use-inpatient-census.jsx` | Membatalkan request census lama ketika effect dibersihkan agar jawaban usang tidak menimpa state baru |
| `src/utils/health-services/inpatient-management/inpatient-census-utils.jsx` | Menambahkan pemeriksaan penyaring aktif untuk memilih empty state yang benar |
| `src/style/health-services/inpatient-management/inpatient-census.module.css` | Menambahkan susunan badge status dengan design token spacing existing |

### 3.3 Kepatuhan arsitektur frontend

Alur dependensi tetap `view → hook → service → axios client`. View tidak memanggil HTTP secara
langsung. Normalizer existing tetap menjadi allow-list agar informasi klinis di luar kontrak
tidak masuk ke tampilan.

Keputusan UI:

| Elemen | Keputusan | Implementasi |
| --- | --- | --- |
| Hero | `REUSE` | `Hero` existing dengan action retry |
| Penyaring | `REUSE` | `DataFilter` dan `FilterSelect` existing |
| Tabel dan pagination | `REUSE` | `DataTable` dan `RegionPagination` existing |
| Badge status | `REUSE` | `StatusBadge` existing |
| Aksi detail/CTA | `REUSE` | `BaseButton` sebagai `next/link` |
| State gagal | `COMPOSE` | `InformationAlert` + action `BaseButton` |
| State kosong | `COMPOSE` | copy `DataTable` + actions `DataFilter` |

`UI GATE: 7 elements — REUSE 5, COMPOSE 2, EXTEND 0, WRAP 0, NEW 0.`

Tidak ada warna, ukuran huruf, tabel mentah, tombol Bootstrap mentah, atau `!important` baru.

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tabel menampilkan “Mengambil daftar pasien yang sedang dirawat...” dan kontrol loading existing |
| Berisi | Delapan kolom yang disetujui beserta Detail Episode pada setiap baris yang memiliki `episodeId` |
| Kosong tanpa penyaring | “Belum ada pasien yang sedang dirawat” disertai Buka Admisi dan Buka Daftar Kerja Episode |
| Kosong karena penyaring | “Tidak ada pasien yang cocok dengan penyaring ini” dengan saran mengatur ulang penyaring |
| Gagal | Alert bahaya dan tombol Coba Lagi; tabel tidak dirender sehingga error tidak tampak sebagai nol data |
| Tanpa hak akses | `AccessDeniedGate` mengganti isi halaman ketika server menjawab `401`/`403` |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Inpatient Management / Inpatient Census

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/inpatient-management/census/filters/metadata` | Mengambil pilihan unit layanan dan kelas perawatan yang berlaku untuk census | `InpatientCensus : Read` |
| `GET` | `/api/v1/health-services/inpatient-management/census` | Mengambil daftar pasien dirawat beserta pagination | `InpatientCensus : Read` |

#### Health Services / Inpatient Management / Inpatient Episode

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/api/v1/health-services/inpatient-management/episodes/{id}` | Dibaca halaman tujuan setelah pengguna memilih Detail Episode | `InpatientEpisode : Read` |

CTA kosong hanya melakukan navigasi ke route existing `/admissions` dan `/episodes`; task ini
tidak menambah request tulis dari halaman census.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm.cmd run lint` | Exit `0`; 571 warning existing dan `0 errors` | `PASS` | Keluaran ESLint 1 September 2026 |
| ESLint terarah pada berkas JSX/JS task | Exit `0`; tidak ada warning pada source yang diperiksa | `PASS` | `npm.cmd exec -- eslint ...` |
| `npm.cmd run build` | Next.js `16.2.12` berhasil compile dan menghasilkan 245 halaman statis | `PASS` | `✓ Compiled successfully in 33.7s`; postbuild standalone selesai |
| `git diff --check` | Tidak ada whitespace error; hanya peringatan konversi LF ke CRLF dari Git Windows | `PASS` | Exit `0` |
| Pemeriksaan tombol/style mentah | Tidak ditemukan `<button>`, kelas `btn-*`, warna literal, typography literal, tabel mentah, atau `!important` pada perubahan task | `PASS` | Pemeriksaan terarah source |
| State berisi/kosong/gagal/peran pada runtime | Tidak dijalankan karena data environment menunggu `RWI-UI-GAP-007` dan pengguna menetapkan lint/build sebagai validasi penyelesaian | `NOT FEASIBLE` | Gap dan arahan pengguna 1 September 2026 |

Uji manual: `NOT FEASIBLE` — data runtime yang dibutuhkan belum tersedia.

`AUTOMATED TEST: SKIPPED (opsional)` — pengguna secara eksplisit meminta tidak menjalankan
`testing.mjs`; perubahan ini divalidasi dengan lint dan production build.

`MANUAL TEST: NOT FEASIBLE` — state dengan episode aktif dan variasi peran belum dapat dibuktikan
tanpa penyelesaian `RWI-UI-GAP-007`.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Episode, Pasien, Lokasi, DPJP, Perawat, Hari Rawat, Status, dan Aksi terbaca pada desktop maupun sempit | Terpenuhi | Delapan definisi kolom memakai `DataTable` existing yang menyediakan pembungkus overflow pada viewport sempit |
| Detail Episode tampil pada setiap baris hanya bila berhak | Terpenuhi secara struktural | Tombol hanya dirender bila `episodeId` ada dan halaman tujuan dijaga `InpatientEpisode : Read`. Frontend belum memiliki katalog permission pengguna untuk menyembunyikannya sebelum navigasi |
| Empty state menyediakan Buka Admisi dan/atau Buka Daftar Kerja Episode sesuai permission | Terpenuhi secara struktural | Kedua CTA menuju route existing yang dijaga server. Frontend belum memiliki katalog permission pengguna untuk memilih salah satunya sebelum navigasi |
| Filter berasal dari `filters/metadata` | Terpenuhi | Hook metadata existing dipertahankan; tidak ada pengambilan master umum baru |
| Gagal baca menyediakan Coba Lagi dan tidak ditampilkan sebagai nol data | Terpenuhi | Hero menampilkan Coba Lagi dan tabel tidak dirender selama `loadError` |
| Informasi klinis sensitif di luar skema tidak ditampilkan | Terpenuhi | Allow-list normalizer existing dipertahankan; kolom hanya menggunakan field census yang disetujui |

Definition of Done source, lint, build, laporan, roadmap, dan traceability terpenuhi. Bukti visual
runtime penuh tetap menjadi pekerjaan environment pada `RWI-UI-GAP-007`, bukan diganti data palsu.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `npm run lint` pertama kali tertahan execution policy PowerShell pada `npm.ps1`; perintah ekuivalen `npm.cmd run lint` berhasil. ESLint melaporkan 571 warning existing dan nol error |
| Masalah yang diketahui | UI belum menerima daftar permission pengguna, sehingga Detail Episode dan dua CTA kosong tidak dapat disembunyikan secara client-side. Otorisasi server/halaman tujuan tetap menjadi pengaman |
| Dependency backend | Endpoint census dan episode tersedia. `RWI-UI-GAP-007` masih menahan data master/episode runtime untuk uji state nyata |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Frontend: empat berkas modified dan dua berkas untracked milik task ini. Backend: laporan, roadmap, dan traceability milik task ini akan modified/untracked |
| Langkah berikutnya | Jalankan state runtime setelah gap 007 selesai. Kerjakan `FE-RWI-038` pada invocation skill berikutnya karena skill frontend membatasi satu task per invocation |
