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
| Status | 🟡 **SEBAGIAN — 2 Oktober 2026.** Build pemilik terbukti. Uji layar: **4 terbukti** (U1, U3, U4, U5), **1 sebagian** (U2), **2 belum terbukti** (U6, U7). **Kriteria 2 belum terpenuhi pada bukti:** pada lebar 1440 piksel kolom PENUTUPAN terdorong ke luar bidang pandang, sehingga penanda dan alasannya tidak terbaca tanpa menggulir ke samping. *Sebelumnya:* 🟡 **SEBAGIAN — Implementation Complete.** Empat berkas (tiga source diubah, satu util baru); `eslint` 0 error, 0 warning; unit test tidak dipakai atas perintah pemilik 1 Oktober 2026. **Belum:** `npm run build` dan uji layar U1–U7 (milik pemilik, sesudah build `BE-IGD-063`) |

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
