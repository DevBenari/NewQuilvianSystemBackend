# Laporan Perubahan Frontend — `FE-HMD-19`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HMD-19` |
| Judul | Uji Keterjangkauan Navigasi, Validasi 4 State Layar, Perlindungan Privasi, dan UAT Layar End-to-End |
| Slice | Lintas Potong — berlaku untuk seluruh layar modul |
| Roadmap | `docs/module-blueprints/hemodialisa/roadmap/frontend-roadmap.md` bagian 4.7 |
| Trace | `NFR-006`, `NFR-009`, `NFR-010`, `03-frontend-architecture.md` Bagian 1, 2, 3, 6, dan 8; Skenario `UAT-01` s/d `UAT-22` |
| Contract version | `HMD-CONTRACT-v1` — status `approved` (18 September 2026) |
| Wewenang UI | Tidak ada perubahan tampilan pada task ini; cakupannya audit dan pengujian |
| Keputusan UI Gate | `UI GATE: N/A` — task ini tidak membuat maupun mengubah route, view, komponen, atau style yang terlihat pengguna |
| Dependency | `FE-HMD-01` s/d `FE-HMD-18` (seluruhnya selesai 23 September 2026 atau lebih awal) |
| Klasifikasi | `MEDIUM` — skor 8: repository 0, berkas diperiksa 20, berkas dibuat 1, berkas diubah 0, logika 3, kontrak API 0, database 0, UI 0 |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` (berkas audit otomatis), ditambah wewenang sempit lintas repository untuk laporan ini |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `a03676d1d` — branch `HamzahV2` |
| Commit backend yang dijadikan rujukan | `9deb23de` — branch `MHamzah` |
| Tanggal | 23 September 2026 |
| Status | 🟡 **Sebagian** — `AC-1` dan audit privasi terpenuhi dengan bukti otomatis; `AC-2` (eksekusi 22 skenario UAT di antarmuka) dan `AC-3` (pembuktian runtime tombol coba lagi memanggil ulang API) **belum terpenuhi** karena menuntut lingkungan aplikasi yang berjalan. Rinciannya pada bagian 7 |

---

## 1. Keadaan yang Ditemukan di Awal

1. Seluruh delapan belas task layar modul Hemodialisa sudah selesai, sehingga audit lintas potong baru dapat dijalankan sekarang.
2. Repository **tidak memakai Jest maupun `@testing-library`**, dan **tidak memiliki `playwright.config.*`** di root meskipun `@playwright/test` terpasang sebagai dependency. Artinya suite E2E yang diminta roadmap tidak dapat dijalankan tanpa menambah konfigurasi baru — sebuah perubahan dependency dan konfigurasi yang memerlukan permintaan eksplisit pemilik.
3. Sesi kerja ini tidak memiliki server pengembangan frontend maupun backend yang berjalan beserta sesi login berotorisasi.

---

## 2. Proses Bisnis dari Sisi Pengguna

Task ini tidak menambah layar. Yang dijaga adalah tiga janji kepada pengguna:

1. **Tidak ada layar yatim.** Setiap layar modul dapat dicapai, entah lewat butir menu di sidebar atau lewat tombol pada layar induknya. Petugas tidak pernah perlu mengetik URL sendiri.
2. **Tidak ada layar hampa.** Ketika data sedang dimuat, kosong, atau gagal dibaca, layar menjelaskan keadaannya dan menawarkan jalan keluar, bukan menampilkan area putih tanpa keterangan.
3. **Tidak ada kebocoran privasi.** Layar daftar kerja dan beranda dilihat banyak orang sekaligus di ruang terbuka unit. Tidak boleh ada satu pun istilah serologi atau diagnosis infeksi yang muncul di sana.

---

## 3. Perubahan yang Dikerjakan

### 3.1 Berkas yang Diperiksa

Seluruh sebelas berkas rute modul, seluruh berkas view modul, seluruh hook modul, `src/utils/menu-sidebar/menu-items.jsx`, ketiga stylesheet layar baru, dan `hemodialysisConstants.js`.

### 3.2 Berkas yang Berubah

| Berkas | Perubahan |
| --- | --- |
| `tests/unit/hemodialysis-navigation-and-privacy-audit.test.mjs` | **Baru.** 12 pemeriksaan otomatis atas keterjangkauan navigasi, empat keadaan layar, perlindungan privasi, dan konsistensi UI lintas layar baru |

Tidak ada source aplikasi yang diubah. Audit ini murni pemeriksaan.

### 3.3 Isi Audit Otomatis

**Keterjangkauan navigasi.**

- Modul memiliki tepat sebelas rute layar.
- Kesembilan rute statis seluruhnya terdaftar sebagai butir menu sidebar.
- Kedua rute dinamis — ruang kerja episode dan ruang kerja sesi — punya jalan masuk dari layar induknya, dan keduanya dibangun lewat pembantu rute bernama pada constants, bukan string yang tersebar.
- Setiap berkas rute tetap tipis (maksimal 40 baris) dan tidak memanggil Axios langsung.

**Empat keadaan layar.**

- Keenam layar daftar dan ruang kerja membungkus isinya dengan `ClinicalStateBoundary`.
- Ketiga layar yang dibangun `FE-HMD-12` s/d `FE-HMD-18` memasang tombol coba lagi pada keadaan gagal lewat prop `retryAction`.
- Layar daftar menyediakan kalimat keadaan kosong.

**Perlindungan privasi.**

- Kelima berkas layar bersama — view dan kolom tabel daftar kerja, view beranda, serta kedua berkas utilitas tampilannya — diperiksa terhadap enam istilah terlarang: `hiv`, `hepatitis`, `hbsag`, `anti-hcv`, `serologi`, dan `reaktif`. Tidak satu pun ditemukan.
- Kolom tabel daftar kerja tidak memuat kolom diagnosis dalam bentuk apa pun.
- Penanda isolasi terbukti hanya berbunyi "Perlu Isolasi" atau tanda hubung.

**Konsistensi UI lintas layar baru.**

- Tidak ada `<button>` mentah maupun dialog bawaan peramban pada ketiga folder layar baru.
- Ketiga stylesheet layar baru tidak menulis warna literal, tidak menimpa typography komponen shared, tidak memakai `!important`, dan tidak menambahkan blok dark mode.

**Dua positif palsu yang ditemukan dan diperbaiki pada auditnya sendiri.** Pemeriksaan privasi versi pertama menandai kebocoran pada berkas kolom tabel daftar kerja, padahal istilah itu muncul di **komentar yang justru melarangnya** dan tidak pernah sampai ke layar. Auditnya diperbaiki agar membuang komentar lebih dulu. Pemeriksaan keterjangkauan versi pertama juga keliru menyatakan layar sesi yatim, karena hanya memindai berkas view sementara jalan masuknya berada di hook pengendali layar worklist; cakupan pemindaiannya diperluas.

---

## 4. State yang Ditangani di Layar

`NOT APPLICABLE` — task ini tidak menambah atau mengubah layar. Hasil audit atas keempat keadaan layar yang sudah ada tercantum pada bagian 3.3.

---

## 5. Endpoint yang Dikonsumsi

`NOT APPLICABLE` — audit statis atas source; tidak ada endpoint yang dipanggil.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `node --import ./tests/helpers/register.mjs --test tests/unit/hemodialysis-navigation-and-privacy-audit.test.mjs` | 12 test, 12 lolos, 0 gagal | `PASS` | Berkas audit baru |
| `node --import ./tests/helpers/register.mjs --test "tests/unit/*.test.mjs"` | 1746 test, 1739 lolos, 7 gagal | `PASS` untuk cakupan modul ini | Ketujuh kegagalan berada di `accounting-reconciliation.test.mjs`, `inpatient-physician-entry.test.mjs`, `inpatient-physician-workspace.test.mjs`, dan `inpatient-supporting-service-v2.test.mjs` — tidak satu pun berkasnya disentuh pekerjaan modul Hemodialisa. Garis dasar sebelum pekerjaan ini: 1617 test, 1610 lolos, 7 gagal |
| `npx eslint src --quiet` | exit code 0, tanpa keluaran | `PASS` | 0 error pada seluruh `src/` |
| `npm run test:unit` | Gagal `ERR_UNSUPPORTED_DIR_IMPORT` | `EXISTING / ENVIRONMENT ISSUE` | Node 24.13.0 menolak direktori sebagai argumen `--test`; suite dijalankan memakai pola glob |
| `npm run lint:errors` | Gagal sebelum memeriksa berkas mana pun | `EXISTING / ENVIRONMENT ISSUE` | Folder kerja lokal `test-with-agy/` di-`.gitignore` tetapi tidak dikecualikan `eslint.config.mjs` |
| Eksekusi 22 skenario UAT di antarmuka | Tidak dijalankan | `NOT FEASIBLE` | Menuntut aplikasi frontend dan backend yang berjalan beserta beberapa akun berotorisasi berbeda peran, data pasien, dan data sesi yang sudah melewati seluruh daur hidupnya |
| Suite E2E `HemodialysisFrontendUatE2ETests` | Tidak dibuat | `NOT RUN` | Repository tidak memiliki `playwright.config.*`; membuatnya adalah perubahan konfigurasi yang memerlukan permintaan eksplisit pemilik |
| `npm run build` | Tidak dijalankan | `NOT RUN` | Dikecualikan atas keputusan tetap pemilik 10 September 2026 |

**Uji manual:** `NOT FEASIBLE`.

**Tidak dijalankan:** `npm run build`, `npm run test:e2e`, `npm run test:uat`.

---

## 7. Acceptance Criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| **AC-1** Pengujian otomatis membuktikan tidak ada rute yang tidak dapat dicapai dari antarmuka pengguna | **Terpenuhi** | Empat pemeriksaan otomatis pada berkas audit: jumlah rute, kesesuaian seluruh rute statis dengan butir menu sidebar, keberadaan jalan masuk kedua rute dinamis dari layar induknya, dan ketipisan berkas rute. Seluruhnya lolos |
| **AC-2** Seluruh 22 skenario UAT pada PRD MVP teruji pada antarmuka frontend dengan hasil sukses sesuai ekspektasi pengguna | **Belum terpenuhi** | Tidak ada skenario UAT yang dieksekusi. Pengujiannya menuntut aplikasi yang berjalan beserta akun berperan berbeda dan data sesi lengkap, yang tidak tersedia pada sesi kerja ini |
| **AC-3** Tombol coba lagi pada keadaan error terbukti memicu pemanggilan ulang API dengan sukses | **Terpenuhi sebagian** | Terbukti secara statis bahwa ketiga layar baru memasang `retryAction` yang tertaut ke fungsi pemuatan ulang. **Belum terbukti** bahwa penekanannya benar-benar menghasilkan permintaan HTTP baru yang sukses, karena itu menuntut pengujian runtime |

**Definition of Done** — "Seluruh layar terverifikasi keterjangkauannya, 4 keadaan UI lolos uji, pengujian privasi dan UAT E2E lulus 100%":

| Butir | Status |
| --- | --- |
| Audit keterjangkauan seluruh layar | Terpenuhi, dengan bukti otomatis |
| Audit empat keadaan layar | Terpenuhi pada tingkat source; keempat keadaan terbukti tersedia dan terhubung |
| Audit privasi layar bersama | Terpenuhi, dengan bukti otomatis atas enam istilah terlarang |
| UAT E2E lulus 100% | **Belum terpenuhi** |

**Mengapa status task ini 🟡, bukan ✅.** Keputusan tetap pemilik 1 September 2026 melepas butir Definition of Done yang berbentuk test `.mjs`, E2E, atau uji manual peramban **pada task fitur**, karena di sana kekurangannya adalah bukti, sementara source-nya sudah ada. Pada `FE-HMD-19`, pengujian itu **adalah** deliverable task-nya: `AC-2` menuntut eksekusi 22 skenario UAT, dan tanpa eksekusi itu tidak ada source maupun bukti yang dapat dirujuk. Keputusan yang sama menyatakan pengecualian tidak berlaku ketika yang kurang adalah source-nya. Karena itu task ini dilaporkan sebagian, dan bagian yang benar-benar dapat dibuktikan hari ini sudah dikerjakan serta dibuktikan.

---

## 8. Catatan Penutup

| Hal | Isi |
| --- | --- |
| Peringatan | `NONE` |
| Masalah yang diketahui | **Empat cacat modul di luar cakupan** yang ditemukan sepanjang pengerjaan `FE-HMD-12` s/d `FE-HMD-18` dan belum diperbaiki: (1) `hemodialysis-orders-table-columns.jsx` memakai kunci kolom `label` dan tanda tangan `render(val, row, index)` sedangkan `DataTable` membaca `header` dan `render(item, meta)`, sehingga judul kolom kosong dan isi selnya salah; (2) `hemodialysis-patients-table-columns.jsx` memakai `header`+`accessor`+`cell` tanpa `key` dan tanpa `render`, sehingga seluruh sel jatuh ke tanda hubung; (3) `resolveReadinessBadge` membandingkan status dengan teks sedangkan backend mengirim angka; (4) Ruang Kerja Episode memanggil `ClinicalStateBoundary` dengan prop `onRetry` dan `loadingText` yang tidak dikenali komponennya, sehingga tombol coba lagi tidak pernah muncul di sana. Keempatnya berada pada task yang sudah ditandai selesai dan memerlukan keputusan pemilik untuk dijadwalkan ulang. **Satu usulan backend**: endpoint daftar serah terima tagihan, misalnya `GET /hemodialysis-sessions/billing-handoff?date=...&status=Failed`, agar beranda tidak perlu membaca status per sesi |
| Dependency backend | `BE-HMD-19` berstatus 🟡 sebagian pada roadmap backend; tidak menahan audit frontend ini |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Tidak ada `git add`, commit, push, pull, merge, rebase, maupun perpindahan branch |
| Langkah berikutnya | Dua langkah untuk menutup `FE-HMD-19` menjadi ✅: **(a)** putuskan apakah suite Playwright beserta `playwright.config.*` boleh ditambahkan — ini perubahan konfigurasi yang menunggu persetujuan; **(b)** sediakan lingkungan UAT berisi aplikasi berjalan beserta akun perawat, akun dokter penanggung jawab, dan akun koordinator, lalu eksekusi 22 skenario UAT. Sebagai alternatif, pemilik dapat memperluas pengecualian 1 September 2026 sehingga mencakup task lintas potong ini, dan status dinaikkan menjadi ✅ beserta catatan pengecualiannya |
