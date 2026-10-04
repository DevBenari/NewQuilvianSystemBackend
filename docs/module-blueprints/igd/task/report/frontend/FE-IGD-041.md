# Laporan Perubahan Frontend — `FE-IGD-041`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-IGD-041` |
| Judul | Saringan dan penanda "menunggu penutupan" pada daftar kunjungan IGD |
| Slice | `S5` · `EPIC IGD-13` · `MVP-8` |
| Roadmap | `docs/module-blueprints/igd/roadmap/frontend-roadmap.md` bagian R3.13.1 |
| Trace | `FR-IGD-091`; `IGD-DEC-164`, `168`; `AT-IGD-191` (sisi layar); `03-frontend-architecture.md` §14.2 |
| Contract version | API `0.12.0` §9.2 — query `awaitingClosure`, ruas `isAwaitingClosure` dan `awaitingClosureReason`; `approved` (`IGD-DEC-170`) |
| Wewenang UI | `DEV_DISCRETION` untuk rupa penanda dan letak saringan; nol elemen `NEW` |
| Dependency | `BE-IGD-063` 🟡 — source selesai 1 Oktober 2026, build dan uji API pemilik belum. Task ini dikerjakan pada sisi source atas perintah pemilik; uji layarnya menunggu build backend |
| Klasifikasi | `MEDIUM` — skor 5: repository 0, berkas diperiksa 1, berkas diubah 2 (4), logika 1, kontrak API 1, database 0, keamanan/auth 0, UI/workflow 0 |
| Task mode | `FRONTEND` — wewenang pemilik 1 Oktober 2026. Backend baca-saja, kecuali laporan ini dan status pada roadmap/traceability |
| Target tulis | `QuilvianSystemFrontendDev` (branch `RizkiV2`); laporan di repository backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `aa0b1168b` (`RizkiV2`) + working tree `FE-IGD-038`, `036`, `039`, `040`, `042` yang belum di-commit |
| Commit backend yang dijadikan rujukan | `b9076c71` + working tree `BE-IGD-053`, `057`, `058`, `059`, `061`, `062`, `063` |
| Tanggal | 1 Oktober 2026 |
| Status | ✅ **SELESAI — 4 Oktober 2026.** Uji layar U2, U6, U7, R pada hasil build 11.46 (standalone, 1440 × 900, akun Perawat IGD) — **4 dari 4 terbukti** pada bukti mentah. Kriteria 2 terpenuhi: penanda di bawah lencana utuh di dalam pembungkus pada 10 dari 10 baris, tanpa kolom PENUTUPAN. Dua pemeriksaan ketat dari panduan gagal karena lebar nama pasien uji, bukan karena task ini (tabel luber 119 piksel; `scrollWidth` berselisih 7 piksel) — bagian *Pemeriksaan bukti uji gabungan `MVP-8`*. Bukti diterima dengan penyimpangan tercatat (`IGD-DEC-188`). Tanpa UAT. *Sebelumnya:* 🟡 **SEBAGIAN — 3 Oktober 2026 (pengerjaan ulang tata letak).** Penanda *Menunggu penutupan* dipindah ke bawah lencana STATUS KUNJUNGAN dan kolom PENUTUPAN dihapus; lebar kolom ditata ulang (jumlah `minWidth` 1.102 piksel, sebelumnya 1.392). Pengukuran harness pada 1440 piksel: penanda utuh di dalam bidang pandang, tabel luber 0 piksel dengan nama biasa. `eslint` 0 error; unit test IGD 91/91; `npm run build` agent **lulus** 17.03 (465/465 halaman, 0 warning, `postbuild` standalone berhasil) dan server standalone port 3000 dinyalakan ulang dari hasil build itu. **Belum:** uji layar U2, U6, U7 revisi pada hasil build — bagian *Pengerjaan ulang 3 Oktober 2026*. *Sebelumnya:* 🟡 **SEBAGIAN — 2 Oktober 2026.** Build pemilik terbukti. Uji layar: **4 terbukti** (U1, U3, U4, U5), **1 sebagian** (U2), **2 belum terbukti** (U6, U7). **Kriteria 2 belum terpenuhi pada bukti:** pada lebar 1440 piksel kolom PENUTUPAN terdorong ke luar bidang pandang, sehingga penanda dan alasannya tidak terbaca tanpa menggulir ke samping. *Sebelumnya:* 🟡 **SEBAGIAN — Implementation Complete.** Empat berkas (tiga source diubah, satu util baru); `eslint` 0 error, 0 warning; unit test tidak dipakai atas perintah pemilik 1 Oktober 2026. **Belum:** `npm run build` dan uji layar U1–U7 (milik pemilik, sesudah build `BE-IGD-063`) |

---

## 1. Keadaan yang ditemukan di awal

| Temuan | Bukti |
| --- | --- |
| Satu-satunya layar yang membaca `GET /emergency-visits` sebagai daftar adalah **Pengkajian Pasien IGD** | Pencarian `emergency-visits` pada `src/`: `fetchAssessmentPatients` di `emergency-assessment-slice.jsx`. Daftar triage sudah pindah ke `triage-queue` (`FE-IGD-035`) |
| Layar itu sudah punya panel saringan (`DataFilter`: status kunjungan, rentang tanggal) dan tabel (`DataTable`) | `emergency-assessment-list-view.jsx` |
| Tidak ada cara melihat kunjungan yang tindak lanjutnya sudah dilaksanakan tetapi belum tertutup | Kolom status hanya menampilkan *Tindak lanjut ditetapkan*, tanpa alasan penahan |

Karena itu "layar daftar kunjungan IGD" pada kartu dibaca sebagai daftar Pengkajian Pasien IGD.

---

## 2. Proses bisnis dari sisi pengguna

Pengguna: perawat atau dokter IGD pemegang `EmergencyVisit : Read`, pada daftar Pengkajian Pasien IGD.

1. Pada panel **Filter Pasien** ada pilihan baru **Penutupan Kunjungan** berisi *Semua kunjungan* dan
   *Menunggu penutupan*.
2. Memilih *Menunggu penutupan* menampilkan hanya kunjungan yang tindak lanjutnya sudah dilaksanakan tetapi belum
   tertutup. Di atas tabel terbaca jumlahnya, misalnya **"3 kunjungan menunggu penutupan"**.
3. Tabel bertambah kolom **PENUTUPAN**. Tiap baris memuat penanda *Menunggu penutupan* beserta alasan penahannya,
   misalnya *"Menunggu penutupan — Masih ada observasi yang belum diselesaikan."*
4. Memilih *Semua kunjungan* atau menekan Reset mengembalikan daftar seperti semula.
5. Tanpa saringan, kolom PENUTUPAN hanya muncul bila halaman yang sedang dibuka memuat kunjungan yang menunggu
   penutupan; baris lain berisi tanda hubung.

*Contoh.* Kepala jaga membuka daftar pukul 14.00 dan memilih *Menunggu penutupan*. Terbaca "2 kunjungan menunggu
penutupan": Pak Budi dengan *"Masih ada observasi yang belum diselesaikan."* dan Ibu Sari dengan *"Masih ada proses
kepergian pasien yang belum selesai."* Ia membuka pemeriksaan Pak Budi, menyelesaikan observasinya, kembali ke
daftar, dan jumlahnya menjadi satu.

**Jalur tidak normal.**

| Keadaan | Yang terjadi |
| --- | --- |
| Tidak ada kunjungan yang menunggu penutupan | Tabel menampilkan *"Tidak ada kunjungan yang menunggu penutupan."* beserta penjelasannya; baris jumlah tidak tampil |
| Alasan penahan kosong | Penanda *Menunggu penutupan* tetap tampil tanpa keterangan tambahan |
| Permintaan gagal | Pesan galat umum daftar beserta tombol *Coba lagi* (tidak berubah); baris jumlah tidak tampil |
| Backend belum memuat `BE-IGD-063` | Parameter diabaikan server: daftar tidak tersaring dan tidak ada penanda — karena itu uji layar menunggu build backend |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu `FE-IGD-041`; API §9.2; 03 §14.2–14.3; laporan `BE-IGD-063`.
Source: `data-filter.jsx`, `filter-select.jsx`, `data-table.jsx`, `base-data-components.module.css`,
`emergency-assessment.module.css`, `use-emergency-assessment-list.jsx`, dan seluruh berkas pada 3.2.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/…/emergency-assessment-constant.jsx` | `DEFAULT_EMERGENCY_ASSESSMENT_FILTERS` + `awaitingClosure`; + `AWAITING_CLOSURE_FILTER_VALUE`, `AWAITING_CLOSURE_LABEL`, `AWAITING_CLOSURE_FILTER_OPTIONS`, `EMERGENCY_ASSESSMENT_LIST_MESSAGES`. +22 |
| `src/utils/…/emergency-assessment-list.utils.js` (**baru**) | `isAwaitingClosureFilterActive`, `buildAssessmentPatientParams`, `resolveAwaitingClosureMarker`, `shouldShowClosureColumn`, `buildAwaitingClosureCountMessage`, `resolveAssessmentListEmptyState`. 51 baris |
| `src/lib/state/slice/…/emergency-assessment-slice.jsx` | Parameter `fetchAssessmentPatients` kini dibentuk `buildAssessmentPatientParams` — isi lama dipindahkan apa adanya, ditambah `awaitingClosure`. +2/−11 |
| `src/components/view/…/emergency-assessment-list-view.jsx` | + pilihan saringan *Penutupan Kunjungan*; + kolom PENUTUPAN bersyarat; + baris jumlah; kalimat kosong mengikuti saringan. +53/−3 |

Nol baris komentar baru; hitungan komentar berkas lama tidak berubah. **Tidak disentuh:** hook daftar
(`use-emergency-assessment-list.jsx` — saringan baru lewat `updateFilter` yang sudah ada), CSS mana pun, route, menu,
base component, backend.

### 3.3 Kepatuhan arsitektur frontend

View → hook → slice → util/constant; permintaan lewat `InstanceAxios` di thunk; aturan saringan dan penanda berupa
fungsi murni di `src/utils`.

`UI GATE: 4 elemen — REUSE 4, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Pilihan saringan | `FilterSelect` di dalam `DataFilter` | Saringan *Status Kunjungan* pada layar yang sama | `REUSE` | — |
| Kolom penanda | Kolom `DataTable` dengan `render` | Kolom lain pada tabel yang sama | `REUSE` | — |
| Rupa penanda dan alasannya | Kelas `formHint` (kuning, teks membungkus) | `emergency-assessment-form-card.jsx`; dipakai juga `FE-IGD-042` | `REUSE` | — |
| Baris jumlah | Kelas `sectionNotice` (`strong` + `p`) | Kotak pemberitahuan pada layar yang sama | `REUSE` | — |

Grep anti-regresi pada berkas baru dan baris yang ditambah: nol `<button>`, nol `<table>`, nol utilitas tipografi
Bootstrap, nol stylesheet berubah.

### 3.4 Keputusan pelaksanaan

| Keputusan | Alasan |
| --- | --- |
| Saringan berupa pilihan dua nilai (*Semua kunjungan*, *Menunggu penutupan*), bukan tiga | Nilai `false` pada kontrak ("kebalikannya") tidak punya kegunaan bagi petugas di layar ini; 03 §14.2 menyebut **satu** pilihan baru |
| Pilihan *Semua kunjungan* disediakan sebagai butir, bukan hanya lewat Reset | Kriteria 4: saringan dapat dimatikan tanpa membuang saringan lain |
| Penanda ditaruh pada kolom tersendiri yang hanya tampil bila diperlukan | Alasan penahan bisa sepanjang satu kalimat dan wajib terbaca utuh; kelas sel nama dan lencana status memotong teks. Tanpa saringan dan tanpa baris tertahan, tabel sama persis seperti sebelumnya (kriteria 4) |
| Alasan penahan ditampilkan apa adanya dari server | Kalimatnya milik penjaga penutupan backend; layar tidak menerjemahkan ulang |
| Jumlah dibaca dari `totalData` permintaan yang sama | 03 §14.2; nol permintaan tambahan |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | *"Mengambil pasien IGD..."* (tidak berubah); baris jumlah disembunyikan |
| Kosong | Saringan aktif: *"Tidak ada kunjungan yang menunggu penutupan."* Saringan mati: kalimat lama |
| Gagal | Pesan galat umum daftar + *Coba lagi* (tidak berubah) |
| Tanpa hak akses | Kotak *"Anda tidak memiliki hak akses…"* (tidak berubah) |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Emergency Installation Management / Emergency Visit

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/emergency-installation-management/emergency-visits` | Daftar Pengkajian Pasien IGD — kini dapat membawa `awaitingClosure=true`, dan membaca `isAwaitingClosure`, `awaitingClosureReason` tiap baris | `EmergencyVisit : Read` |

Nol endpoint baru.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` empat berkas task | 0 error, 0 warning | `PASS` | Keluaran perintah |
| Unit test | Tidak dipakai | `SKIPPED` | Perintah pemilik 1 Oktober 2026: *"gausah pake unit test"*. Berkas test yang sempat ditulis untuk task ini (sembilan kasus) dihapus dan tidak ikut pada working tree |
| Baris komentar pada tambahan | Nol | `PASS` | Hitungan komentar tiap berkas sebelum dan sesudah |
| Akhiran baris | CRLF dipertahankan | `PASS` | Pemeriksaan byte |
| Nol menu, nol route, nol CSS | Nol berkas di `src/app` dan `src/style`; `menu-items.jsx` tidak disentuh task ini | `PASS` | `git diff --stat` |
| `npm run build` | — | `NOT RUN` | Milik pemilik |
| Uji layar U1–U7 | — | `NOT RUN` | Milik pemilik; menunggu build `BE-IGD-063` |

`AUTOMATED TEST: SKIPPED (opsional) — atas perintah pemilik 1 Oktober 2026`

`MANUAL TEST: NOT FEASIBLE — backend tidak berjalan pada sesi ini dan uji layar dijalankan pemilik`

Uji manual: `REQUIRED` — pekerjaan saringan: daftar pilihan, keadaan terpilih, efek pada permintaan, reset, saringan
gabungan, dan paginasi (U1–U7).

### 6.1 Skenario uji layar untuk pemilik

| # | Langkah | Yang diharapkan | Kriteria |
| ---: | --- | --- | ---: |
| U1 | Buka Pengkajian Pasien IGD, lihat panel Filter Pasien | Ada pilihan **Penutupan Kunjungan** berisi *Semua kunjungan* dan *Menunggu penutupan* | 1 |
| U2 | Pilih *Menunggu penutupan* | Permintaan membawa `awaitingClosure=true`; hanya kunjungan tertahan yang tampil; kolom PENUTUPAN memuat penanda dan alasan tiap baris, terbaca utuh tanpa membuka detail | 2 |
| U3 | Masih pada U2 | Di atas tabel terbaca *"N kunjungan menunggu penutupan"*, sama dengan jumlah pada baris paginasi | 3 |
| U4 | Pilih *Semua kunjungan*, lalu ulangi dengan tombol Reset | Daftar kembali seperti semula; permintaan tanpa `awaitingClosure`; baris jumlah hilang | 4 |
| U5 | Pilih *Menunggu penutupan* saat tidak ada kunjungan tertahan (atau gabungkan dengan tanggal yang kosong) | *"Tidak ada kunjungan yang menunggu penutupan."* beserta penjelasannya | 5 |
| U6 | Gabungkan *Menunggu penutupan* dengan pencarian nama, lalu pindah halaman bila baris lebih dari sepuluh | Kedua saringan berlaku bersama; pindah halaman tetap tersaring; mengubah saringan kembali ke halaman 1 | 2 |
| U7 | Tanpa saringan, buka halaman yang memuat kunjungan tertahan | Kolom PENUTUPAN tampil; baris tertahan berpenanda, baris lain bertanda hubung | 2 |

---

## 7. Acceptance criteria dan Definition of Done

| # | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Saringan "Menunggu penutupan" tersedia pada panel saringan yang sudah ada | **Terpenuhi pada source** | `FilterSelect` kedua di `DataFilter` dengan `AWAITING_CLOSURE_FILTER_OPTIONS`. Uji layar U1 belum |
| 2 | Saat aktif, hanya kunjungan menunggu penutupan yang tampil, masing-masing dengan alasannya | **Terpenuhi pada source** | `buildAssessmentPatientParams` mengirim `awaitingClosure=true`; `resolveAwaitingClosureMarker` mengisi kolom PENUTUPAN. Uji layar U2, U6, U7 belum — bergantung pada `BE-IGD-063` |
| 3 | Jumlahnya terbaca tanpa menghitung manual | **Terpenuhi pada source** | Baris jumlah dari `totalData`. Uji layar U3 belum |
| 4 | Saat saringan mati, daftar kembali seperti semula | **Terpenuhi pada source** | Tanpa saringan, `awaitingClosure` tidak dikirim dan parameter lain sama dengan sebelumnya; kolom PENUTUPAN tidak tampil. Uji layar U4 belum |
| 5 | Keadaan kosong memakai kalimat yang menjelaskan | **Terpenuhi pada source** | `resolveAssessmentListEmptyState`. Uji layar U5 belum |
| 6 | Nol butir menu baru, nol route baru | **Terpenuhi** | Bagian 6, baris keenam |
| 7 | `npm run lint` dan `npm run build` bersih | **Sebagian** | `eslint` berkas task 0 error, 0 warning; build belum |

DoD: laporan tracked ✅ (berkas ini).

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Kolom PENUTUPAN muncul dan hilang menurut isi halaman ketika saringan mati, sehingga lebar tabel dapat berubah saat berpindah halaman |
| Masalah yang diketahui | Kunjungan lama yang tindak lanjutnya `Executed` tetapi statusnya belum `Disposed` ikut tampil dengan alasan *"Kunjungan hanya dapat diselesaikan setelah keputusan tindak lanjut ditetapkan."* — kalimat dari backend, dicatat pada laporan `BE-IGD-063` |
| Dependency backend | `BE-IGD-063` 🟡 — tanpa build itu, saringan tidak berpengaruh dan penanda tidak pernah tampil |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Empat berkas oleh task ini (tiga diubah, satu baru); berkas konstanta juga memuat perubahan `FE-IGD-042`. Tanpa stage, commit, atau push |
| Langkah berikutnya | Pemilik: build backend dan frontend, lalu U1–U7 bersama S1–S8 `BE-IGD-063` |

---

## Pemeriksaan bukti uji gabungan — 2 Oktober 2026

Bukti mentah di `QuilvianSystemFrontendDev/test-with-agy/igd/` (`results-tahap-1.json`…`results-tahap-4.json`, skrip `test-tahap-*.mjs`, tangkapan layar `<ID>.png`) dan log backend `Logs/quilvian-backend-20261001.json`, `quilvian-backend-20261002.json`. Ringkasan agen penguji ([laporan uji gabungan](../../../testing/2026-10-01-laporan-uji-gabungan-r313-r314.md)) **tidak** dipakai sebagai bukti: uraian skenarionya pada beberapa task tidak sama dengan panduan, dan daftar `FAIL`-nya tidak cocok dengan JSON mentah.

`npm run build` pemilik terbukti dari artefak: `.next/BUILD_ID` bertanggal 1 Oktober 2026 15.22, sesudah edit source terakhir (14.59). **Uji layar dilayani `next dev`** (lencana "N" pada setiap tangkapan layar), bukan hasil build. Source di-commit pemilik sebagai `de70687a9`.

| Skenario | Putusan | Yang teramati pada bukti mentah |
| --- | --- | --- |
| `041-U1` | **Terbukti** | Pilihan *Semua kunjungan* dan *Menunggu penutupan* |
| `041-U2` | **Sebagian** | Permintaan membawa `awaitingClosure=true`; hanya kunjungan *Tindak lanjut ditetapkan* yang tampil. Kolom PENUTUPAN **terpotong di tepi kanan** — isinya tidak terbaca pada tangkapan layar |
| `041-U3` | **Terbukti** | *"20 kunjungan menunggu penutupan"*, sama dengan *"1 sampai 10 dari 20 data"* |
| `041-U4` | **Terbukti** | Kembali ke *Semua kunjungan*; baris jumlah hilang; 155 data |
| `041-U5` | **Terbukti** | *"Tidak ada kunjungan yang menunggu penutupan."* beserta penjelasannya |
| `041-U6` | **Belum terbukti** | Skrip menulis lulus tanpa memeriksa (`pass = true`). Tangkapan layar: pencarian "Pasien" + saringan menghasilkan **0 baris** walau ada 20 kunjungan tertahan. Pindah halaman tidak dijalankan |
| `041-U7` | **Belum terbukti** | Tanpa saringan, tangkapan layar berhenti di kolom STATUS KUNJUNGAN; kolom PENUTUPAN tidak terlihat. Skrip hanya memeriksa keberadaan elemen |

**Catatan.**

- **Temuan pada task ini.** Kolom PENUTUPAN (lebar minimum 280) membuat tabel melebihi lebar kartu pada 1440 piksel bila nama pasien panjang. Perlu perbaikan tata letak sebelum kriteria 2 dapat dinyatakan terpenuhi.
- **Temuan lama, bukan dari task ini.** Hasil kosong pada `041-U6` berasal dari backend: `search` tidak mencari nama pasien maupun nomor rekam medis (lihat laporan `BE-IGD-063`). Gabungan saringan dengan pencarian nomor kunjungan terbukti lewat `063-S7`.

Putusan: **🟡 sebagian** — Build pemilik terbukti. Uji layar: **4 terbukti** (U1, U3, U4, U5), **1 sebagian** (U2), **2 belum terbukti** (U6, U7). **Kriteria 2 belum terpenuhi pada bukti:** pada lebar 1440 piksel kolom PENUTUPAN terdorong ke luar bidang pandang, sehingga penanda dan alasannya tidak terbaca tanpa menggulir ke samping.

---

## Pengerjaan ulang — 3 Oktober 2026: letak penanda menunggu penutupan

| Field | Nilai |
| --- | --- |
| Pemicu | Temuan uji gabungan 2 Oktober 2026 pada kriteria 2: kolom PENUTUPAN di luar bidang pandang pada 1440 piksel |
| Wewenang | Rizki Gunawan, 3 Oktober 2026: *"tuntaskan MVP-8 — mulai dari perbaikan tata letak FE-IGD-041 (kolom PENUTUPAN pada 1440 piksel)"* |
| Commit frontend dasar | `521b18a9a` (`RizkiV2`, `ahead 2`). Working tree juga memuat perbaikan loket `IGD-DEC-182` pada `emergency-registration.service.js` — **bukan** task ini, tidak disentuh |
| Kontrak | Tidak berubah — API `0.12.0` §9.2 |
| Wewenang UI | `DEV_DISCRETION` (rupa dan letak penanda). Batas yang dijaga: alasan penahan terbaca tanpa membuka detail |

### Penyebab

Bidang pandang tabel pada 1440 piksel dengan sidebar terbuka: 1440 − 260 (sidebar) − 30 (padding konten) − 24
(padding halaman) − bilah gulir ≈ **1.099 piksel** (terukur). Jumlah `minWidth` tujuh kolom lama sudah 1.112 piksel;
kolom PENUTUPAN (`minWidth` 280) menambah 280 lagi. Kolom itu berada di urutan ketujuh dan mulai sekitar piksel 960,
sehingga hanya ±140 piksel pertamanya yang tampak. Nama pasien yang panjang melebarkan kolom NAMA PASIEN
(`white-space: nowrap`) dan mendorong kolom itu makin jauh. Kedua gejala pada bukti 2 Oktober — terpotong di tepi
kanan (U2) dan tangkapan layar berhenti di STATUS KUNJUNGAN (U7) — cocok dengan hitungan ini.

### Perubahan

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/…/emergency-assessment-list-view.jsx` | Kolom PENUTUPAN bersyarat dihapus. Sel STATUS KUNJUNGAN kini memuat lencana status dan, bila `isAwaitingClosure`, kotak *"Menunggu penutupan — <alasan>"* di bawahnya. `minWidth`: TANGGAL MASUK 180→150, STATUS KUNJUNGAN 170→210, AKSI 150→130. +22/−32 |
| `src/utils/…/emergency-assessment-list.utils.js` | `shouldShowClosureColumn` dihapus — tidak dipakai lagi. Fungsi itu ditambahkan task ini sendiri pada 1 Oktober. −6 |
| `src/lib/constants/…/emergency-assessment-constant.jsx` | `awaitingClosureCountHint`: *"…begitu penahan pada kolom Penutupan dibereskan."* → *"…begitu penahan yang tertulis di bawah status kunjungannya dibereskan."* +1/−1 |
| `src/style/…/emergency-assessment.module.css` | Kelas baru `.statusCell` (flex kolom, `gap: var(--space-2)`) untuk menumpuk lencana dan penanda. +11 |

Nol komentar baru di JSX; satu baris komentar CSS mengikuti pola komentar per aturan pada berkas itu. Akhiran baris LF
dipertahankan. Tidak disentuh: hook, slice, route, menu, base component, backend.

### Keputusan base component

`UI GATE: 1 elemen berubah — REUSE 0, EXTEND 0, COMPOSE 1, WRAP 0, NEW 0` (tiga elemen lain dari 1 Oktober tetap `REUSE`)

| Kebutuhan UI | Kandidat | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Penanda dan alasan pada daftar | Kolom `DataTable` `render` + kelas `statusBadge` dan `formHint` | `data-table.jsx` (`render`, `minWidth`); kedua kelas di `emergency-assessment.module.css` | `COMPOSE` | Opsi A |

- **A. Penanda di bawah lencana pada sel STATUS KUNJUNGAN — dipilih (rekomendasi).** Tabel tidak melebar dan tidak
  berubah lebar saat berpindah halaman. Biayanya satu kelas CSS berbasis token.
- **B. Kolom PENUTUPAN dipertahankan, dipindah sesudah NAMA PASIEN, dipersempit ke 200.** Penanda tampak, tetapi tabel
  tetap ±1.312 piksel sehingga UNIT, STATUS, dan tombol *Pemeriksaan* yang terdorong keluar.
- **C. Extend `DataTable` dengan baris rincian di bawah baris.** Alasan selebar tabel, tetapi mengubah base component
  yang dipakai banyak modul; butuh persetujuan; biaya terbesar.

### Pengukuran tata letak (harness)

Halaman uji statis memuat CSS asli repository dengan urutan yang sama seperti `src/app/layout.js` (Bootstrap,
`style.css`, `responsive.css`, `globals.css`, `v1-visual-parity.css`, Poppins, `base-data-components.module.css`,
`emergency-assessment.module.css`). DOM tabelnya disusun sama dengan `DataTable` beserta tiga baris contoh rekaan.
Pengukuran memakai Chromium headless shell 1234 (Playwright), viewport tinggi 900. **Ini bukan uji layar**: tidak ada
login, backend, maupun data sungguhan. Gunanya membuktikan hitungan lebar sebelum build dan uji pemilik.

| Susunan | 1440 · nama biasa | 1440 · nama panjang | 1366 · nama biasa | 1920 |
| --- | --- | --- | --- | --- |
| Sebelum task (`8cc155e02`, tanpa kolom PENUTUPAN) | luber 8 | luber 58 | luber 82 | luber 0 |
| `FE-IGD-041` 1 Oktober (`de70687a9`) | luber 288 — **penanda terpotong** | luber 338 — **terpotong** | luber 362 — **terpotong** | luber 0 |
| **Sesudah perbaikan** | **luber 0 — penanda utuh (3 baris), AKSI utuh** | luber 35 — penanda utuh, AKSI terpotong | luber 58 — penanda utuh | luber 0 |

*Luber* = `scrollWidth` tabel − lebar tampak pembungkus, dalam piksel. Nama panjang yang dipakai: *"Pasien Uji Encounter
First Petugas Loket Tiga"*. Susunan 1 Oktober mereproduksi temuan uji gabungan, sehingga harness ini sah sebagai
pembanding.

### Validasi

| Perintah atau pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint` tiga berkas JS/JSX yang berubah | 0 error, 0 warning | `PASS` |
| Grep anti-regresi 1–6 (`ui-consistency-checklist` G) pada baris tambahan | Nol temuan pada keenam pola | `PASS` |
| `npm run test:unit` | Runner gagal sebelum menjalankan test: `ERR_UNSUPPORTED_DIR_IMPORT` — Node `24.18.1` menolak argumen folder `tests/unit` | `UNRELATED EXISTING ISSUE` |
| Suite lengkap lewat pola berkas `tests/unit/**/*.test.mjs` | 2.358 test: 2.350 lulus, 8 gagal — sidebar Keuangan/petty cash, menu Hemodialisa, Bank Darah, paritas Rawat Jalan/Rawat Inap. Tak satu pun berkas test yang gagal mengimpor berkas task ini | `UNRELATED EXISTING ISSUE` |
| `npm run build` | **Lulus** — `✓ Compiled successfully in 2.3min`, 465/465 halaman, 0 error, 0 warning; `postbuild` standalone berhasil; `.next/BUILD_ID` 17.03. Atas izin pemilik, server uji di port 3000 dihentikan lebih dulu: PID 22684 (standalone) dan `next dev` yang menggantikannya pukul 16.39 sudah berhenti sendiri saat akan dihentikan | `PASS` |
| Isi hasil build | Chunk layar memuat `header:"STATUS KUNJUNGAN",minWidth:210`; nol chunk memuat `header:"PENUTUPAN"`; kalimat petunjuk baru ada dan kalimat lama tidak ada; kelas `statusCell` ada di CSS hasil build | `PASS` |
| Server uji | `node .next/standalone/server.js` dinyalakan ulang dari hasil build ini (PID 22352, port 3000). `GET /login` `200`, chunk statis layar `200`, halaman Pengkajian `200` | `PASS` |

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/emergency-*.test.mjs — PASS (91/91)`

`MANUAL TEST: NOT FEASIBLE — layar butuh login dan backend berjalan; uji layar dijalankan pemilik lewat Antigravity pada hasil build`

### Skenario uji layar revisi — menggantikan U2, U6, U7 pada §6.1

**Prasyarat:** hasil build (`npm run start` atau standalone), **bukan** `next dev` — tangkapan layar tanpa lencana
"N". Viewport **1440 × 900** dengan sidebar terbuka. Akun pemegang `EmergencyVisit : Read`. Minimal 11 kunjungan
menunggu penutupan di dev (2 Oktober: 20). Skrip wajib **memeriksa** setiap harapan; `pass = true` tanpa pemeriksaan
tidak diterima.

| # | Langkah | Yang diharapkan | Bukti yang diserahkan | Kriteria |
| ---: | --- | --- | --- | ---: |
| U2 | Pilih *Menunggu penutupan* | Permintaan membawa `awaitingClosure=true`. Tabel **tanpa** kolom PENUTUPAN. Tiap baris memuat lencana *Tindak lanjut ditetapkan* dan, di bawahnya, kotak kuning *"Menunggu penutupan — <alasan>"*. Seluruh kotak berada di dalam bidang pandang tanpa menggulir ke samping | Tangkapan layar 1440; catatan jaringan; JSON ukuran dari halaman: `clientWidth` dan `scrollWidth` pembungkus tabel, `scrollLeft` = 0, serta tepi kanan tiap kotak penanda ≤ tepi kanan pembungkus | 2 |
| U6 | (a) Saringan aktif, cari nomor kunjungan **lengkap** milik satu baris tertahan. (b) Ganti pencarian menjadi `IGD`, lalu pindah ke halaman 2. (c) Di halaman 2, ubah *Status Kunjungan* | (a) Satu baris, tetap berpenanda; permintaan membawa `search` dan `awaitingClosure=true`. (b) Lebih dari sepuluh baris; permintaan halaman 2 membawa `pageNumber=2`, `awaitingClosure=true`, dan `search=IGD`; semua baris halaman 2 berpenanda. (c) Permintaan kembali ke `pageNumber=1` | Catatan jaringan ketiga langkah; tangkapan layar (a) dan (b) | 2 |
| U7 | Pilih *Semua kunjungan*. Cari halaman yang memuat baris tertahan — atau cari nomor kunjungan tertahan tanpa saringan. Lalu pindah ke halaman tanpa baris tertahan | Baris tertahan menampilkan kotak penanda di bawah lencana; baris lain hanya lencana. Tidak ada kolom PENUTUPAN. `scrollWidth` tabel sama pada kedua halaman | Tangkapan layar kedua halaman; JSON `scrollWidth` kedua halaman | 2 |
| R | Ulang singkat U1, U3, U4, U5 | Seperti §6.1. U3: kalimat petunjuk kini *"…begitu penahan yang tertulis di bawah status kunjungannya dibereskan."* | Tangkapan layar | 1, 3–5 |

**Jangan memakai pencarian nama pasien atau nomor rekam medis.** Backend tidak mencari kedua ruas itu (temuan lama,
laporan `BE-IGD-063`). Itulah penyebab hasil kosong `041-U6` pada 2 Oktober.

### Catatan

| Hal | Isi |
| --- | --- |
| Peringatan §8 lama | *"Lebar tabel dapat berubah saat berpindah halaman"* **tidak berlaku lagi** — tidak ada kolom yang muncul dan hilang |
| Masalah lama, tidak diperbaiki | Nama pasien panjang melebarkan kolom NAMA PASIEN karena `.patientCell strong` memakai `white-space: nowrap` tanpa batas lebar sel. Pada 1440 piksel tombol AKSI dapat terpotong (harness: 35 piksel; susunan sebelum task: 58). Penanda tetap utuh. Perbaikannya berarti memotong nama pasien — menyangkut identifikasi pasien, jadi perlu keputusan tersendiri |
| Masalah lama, tidak diperbaiki | Placeholder pencarian *"Cari No. RM, nama pasien, atau nomor kunjungan..."* menjanjikan dua ruas yang tidak dicari backend |
| Masalah perkakas | `npm run test:unit` gagal pada Node 24 karena argumen folder; suite berjalan lewat pola berkas |
| Status Git | Empat berkas oleh pengerjaan ulang ini (`git diff --stat`: view +22/−32, util −6, konstanta +1/−1, CSS +11), ditambah `emergency-registration.service.js` milik `IGD-DEC-182`. Tanpa stage, commit, atau push |
| Langkah berikutnya | Pemilik menjalankan U2, U6, U7, dan R lewat Antigravity pada server standalone port 3000 (hasil build 17.03), lalu agent memeriksa bukti mentahnya |

Putusan pengerjaan ulang: **🟡 sebagian** — kriteria 2 terpenuhi pada source dan pada pengukuran harness; kriteria 7
(build) **terpenuhi** — `npm run build` lulus, dan `eslint` berkas task bersih. Yang tersisa hanya bukti uji layar U2,
U6, U7 pada hasil build.

**4 Oktober 2026.** Source pengerjaan ulang ini di-commit pemilik sebagai `19ba512de`. Uji U2, U6, U7, dan R dijalankan
lewat [panduan uji gabungan `MVP-8`](../../../testing/2026-10-04-panduan-uji-gabungan-mvp-8.md) pada hasil build 4
Oktober 2026 11.46 — build itu memuat perubahan task ini apa adanya ditambah pengerjaan ulang `FE-IGD-042`.

---

## Pemeriksaan bukti uji gabungan `MVP-8` — 4 Oktober 2026

Bukti mentah di `QuilvianSystemFrontendDev/test-with-agy/igd/uji-gabungan-mvp-8-2026-10-04/` (`041-*.json`, `041-*.png`,
`runner-block-c.mjs`). Semua tangkapan layar uji tanpa lencana "N" (diperiksa per piksel), `nextjsPortalNull` bernilai
benar pada keempat skenario, viewport 1440 × 900, akun Perawat IGD. [Laporan penguji](../../../testing/2026-10-04-laporan-uji-gabungan-mvp-8.md)
tidak dipakai sebagai bukti (bagian 7 laporan itu). Agen penguji mengubah Akses Role sebelum uji; pemilik mengesahkannya
dan menerima bukti (`IGD-DEC-188`).

| Skenario | Putusan | Yang teramati pada bukti mentah |
| --- | --- | --- |
| `041-U2` | **Terbukti** untuk kriteria 2 | Permintaan membawa `awaitingClosure=true` (`totalData` 23). Nol kolom PENUTUPAN. 10 dari 10 baris memuat lencana *Tindak lanjut ditetapkan* dan kotak *"Menunggu penutupan — …"*; tepi kanan setiap kotak 1.359 piksel ≤ tepi kanan pembungkus 1.397; `scrollLeft` 0. Pemeriksaan *"tanpa luber"* **gagal**: `scrollWidth` 1.218 > `clientWidth` 1.099 — lihat catatan |
| `041-U6` | **Terbukti** | (a) `search=<nomor K3>&awaitingClosure=true` → 1 baris berpenanda. (b) `search=IGD&awaitingClosure=true&pageNumber=2` → 10 baris, semuanya berpenanda; paginasi *"Menampilkan 11 sampai 20 dari 23 data"*. (c) Mengubah *Status Kunjungan* di halaman 2 → permintaan `pageNumber=1` |
| `041-U7` | **Terbukti** untuk kriteria 2 | Tanpa saringan: baris `K3` berkotak penanda, baris lain hanya lencana; nol kolom PENUTUPAN pada kedua tampilan. Pemeriksaan *"`scrollWidth` sama"* **gagal** tipis: 1.217 vs 1.224 — lihat catatan |
| `041-R` | **Terbukti** | U1: *Semua kunjungan* dan *Menunggu penutupan*. U3: *"23 kunjungan menunggu penutupan"* = `totalData` respons yang sama; petunjuk sama persis dengan kalimat baru. U4: permintaan tanpa `awaitingClosure`, baris jumlah hilang. U5: *"Tidak ada kunjungan yang menunggu penutupan."* |

**Catatan.**

- **Dua pemeriksaan yang gagal berasal dari harapan panduan yang terlalu ketat**, bukan dari task ini. Luber 119 piksel
  pada `041-U2` disebabkan nama pasien uji di dev yang panjang (misalnya *"Pasien Bersih UI_1790904204089_1679
  39U1_ThreeButtons"*, 52 karakter) dan `white-space: nowrap` pada kolom NAMA PASIEN — masalah lama yang sudah tercatat
  pada bagian *Pengerjaan ulang 3 Oktober 2026*. Selisih 7 piksel pada `041-U7` datang dari sumber yang sama. Kolom yang
  muncul dan hilang akan menggeser lebar ±280 piksel. Yang dijaga kriteria 2 — alasan penahan terbaca tanpa membuka
  detail — terbukti pada kedua skenario.
- **Akibat nyata masalah lama itu pada 1440 piksel**: dengan nama sepanjang data uji, kolom AKSI (tombol *Pemeriksaan*)
  terdorong ke luar bidang pandang (`041-U2.png`). Memperbaikinya berarti membatasi lebar nama pasien — menyangkut
  identifikasi pasien, jadi butuh keputusan pemilik dan kartu tersendiri.
- Laporan penguji menulis *"11 kunjungan menunggu penutupan"* pada `041-R`; DOM dan respons sama-sama 23.

| # | Kriteria | Status |
| ---: | --- | --- |
| 1 | Saringan tersedia pada panel yang sudah ada | **Terpenuhi** — `041-R` U1 (dan `041-U1` 2 Oktober) |
| 2 | Hanya kunjungan tertahan yang tampil, masing-masing dengan alasannya | **Terpenuhi** — `041-U2`, `U6`, `U7` |
| 3 | Jumlah terbaca tanpa menghitung manual | **Terpenuhi** — `041-R` U3 |
| 4 | Saringan mati → daftar seperti semula | **Terpenuhi** — `041-R` U4 |
| 5 | Keadaan kosong memakai kalimat yang menjelaskan | **Terpenuhi** — `041-R` U5 |
| 6 | Nol menu, nol route baru | **Terpenuhi** (1 Oktober) |
| 7 | `lint` dan `build` bersih | **Terpenuhi** — build 3 Oktober 17.03 dan 4 Oktober 11.46 |

Putusan: **✅ selesai** — acceptance 1–7 terpenuhi; uji layar pada hasil build. Tanpa UAT.
