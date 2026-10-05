# Laporan Perubahan Frontend — `FE-IGD-042`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-IGD-042` |
| Judul | Kalimat konfirmasi aksi dan tombol Eskalasi mengikuti aturan penutupan kunjungan |
| Slice | `S5` · `EPIC IGD-13` · `MVP-8` |
| Roadmap | `docs/module-blueprints/igd/roadmap/frontend-roadmap.md` bagian R3.13.1 |
| Trace | `FR-IGD-092`, `093`, `086`, `087` (sisi layar); `IGD-DEC-171`, `172`, `177`, `163`, `164`; `AT-IGD-186`, `187`, `192`, `193` (sisi layar) |
| Contract version | validation `0.10.0` §11.1 aturan 12–14 dan §11 aturan 1–4; state `0.7.0` §9.5; API `0.13.0` §9.4 — `approved` (`IGD-DEC-170`, `175`) |
| Wewenang UI | `DEV_DISCRETION` untuk rupa keterangan tombol nonaktif (`IGD-DEC-177`) dan bunyi kalimat konfirmasi; nol elemen `NEW` |
| Dependency | `BE-IGD-061` 🟡 — source selesai 1 Oktober 2026, build dan uji API pemilik belum. Task ini dikerjakan pada sisi source atas perintah pemilik; uji layarnya menunggu build backend |
| Klasifikasi | `MEDIUM` — skor 5: repository 0, berkas diperiksa 1, berkas diubah 2 (6), logika 1, kontrak API 0, database 0, keamanan/auth 0, UI/workflow 1 |
| Task mode | `FRONTEND` — wewenang pemilik 1 Oktober 2026. Backend baca-saja, kecuali laporan ini dan status pada roadmap/traceability |
| Target tulis | `QuilvianSystemFrontendDev` (branch `RizkiV2`); laporan di repository backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `aa0b1168b` (`RizkiV2`) + working tree `FE-IGD-038`, `036`, `039`, `040` yang belum di-commit |
| Commit backend yang dijadikan rujukan | `b9076c71` + working tree `BE-IGD-053`, `057`, `058`, `059`, `061`, `062`, `063` |
| Tanggal | 1 Oktober 2026 |
| Status | ✅ **SELESAI — 4 Oktober 2026 (uji gabungan `MVP-8`).** Uji layar pada hasil build 11.46 (standalone, 1440 × 900, akun Perawat IGD): acceptance 10–13 (`042-U9`…`U13`) **5 dari 5 terbukti**, regresi U1–U8 **8 dari 8 terbukti** (U6 lewat langkah modal `042-U13`). Tiga JSON berputusan `FAIL` karena kekeliruan skrip penguji — selector keterangan dan pencocokan huruf besar-kecil; perilakunya terbukti pada data mentah, PNG, dan log backend. Bukti diterima dengan penyimpangan tercatat (`IGD-DEC-188`). Unit test dikecualikan atas perintah pemilik 1 Oktober 2026. Tanpa UAT — bagian *Pemeriksaan bukti uji gabungan `MVP-8`*. *Sebelumnya:* 🟡 **SEBAGIAN — 4 Oktober 2026 (pengerjaan ulang `IGD-DEC-187`).** Pada kunjungan *Selesai* dan *Dibatalkan*, tombol *Selesaikan* dan *Eskalasi* observasi nonaktif beserta keterangan kalimat validation `0.11.0` §11.2 aturan 18; *Batalkan* tetap aktif; kunjungan berjalan dan `Disposed` tidak berubah. Dua berkas (+22, nol JSX, nol CSS). `eslint` 0 error, 0 warning; uji IGD 91/91; pemeriksaan pemilih aksi 32 kombinasi status lulus; `npm run build` agent **lulus** 11.46 (465/465 halaman, 0 warning); server standalone port 3000 dinyalakan dari hasil build itu. Acceptance 10–13 terpetakan ke source. **Belum:** uji layar acceptance 10–13 dan regresi U1–U8 pada hasil build — bagian *Pengerjaan ulang 4 Oktober 2026*. *Sebelumnya:* ✅ **SELESAI — 2 Oktober 2026.** Build pemilik dan uji layar U1–U8 **8 dari 8** pada bukti mentah (tangkapan layar dan teks modal). Layar dilayani `next dev`, bukan hasil build. Tanpa UAT. *Sebelumnya:* 🟡 **SEBAGIAN — Implementation Complete.** Enam berkas (lima source diubah, satu util baru); `eslint` 0 error; unit test tidak dipakai atas perintah pemilik 1 Oktober 2026. **Belum:** `npm run build` dan uji layar U1–U8 (milik pemilik, satu siklus dengan uji `BE-IGD-061`) |

---

## 1. Keadaan yang ditemukan di awal

| Temuan | Bukti |
| --- | --- |
| Modal *Selesaikan* observasi selalu menjanjikan kunjungan berpindah ke Menunggu Tindak Lanjut | `OBSERVATION_STATUS_ACTIONS` pada `emergency-assessment-constant.jsx` |
| Tombol *Eskalasi* selalu aktif, juga pada kunjungan yang tindak lanjutnya sudah dilaksanakan | `emergency-assessment-observation-tab.jsx` |
| Modal *Jalankan* menyatakan *"Ini belum menyelesaikan kunjungan…"* | `DISPOSITION_STATUS_ACTIONS` — keliru sejak `BE-IGD-060` |
| Pesan penolakan server hanya tampil di modal *Selesaikan*; pada *Eskalasi* dan *Batalkan* pesannya tersimpan tetapi tidak ditampilkan | Blok `children` `ConfirmModal` pada tab Observasi |
| Kartu pasien tidak dimuat ulang sesudah aksi pada tab; status kunjungan di layar tetap yang lama | `useEmergencyAssessmentDetail` hanya memuat konteks kunjungan saat halaman dibuka |

---

## 2. Proses bisnis dari sisi pengguna

Pengguna: perawat atau dokter IGD pada Ruang Kerja Pemeriksaan IGD.

1. Di tab Tindak Lanjut, menekan **Jalankan** membuka modal yang menjelaskan: kunjungan langsung selesai bila tidak
   ada kewajiban tersisa; bila masih ada, kunjungan menunggu penutupan dan tertutup sendiri.
2. Sesudah *Jalankan* berhasil, kartu pasien di atas layar langsung memperlihatkan status kunjungan terbaru.
3. Di tab Observasi, bila kunjungan sudah berstatus *Tindak lanjut ditetapkan*:
   - tombol **Eskalasi** nonaktif, dan di bawah deret tombol terbaca *"Tindak lanjut pasien sudah dilaksanakan;
     eskalasi tidak dapat dicatat pada kunjungan ini."*;
   - modal **Selesaikan** menjelaskan bahwa status kunjungan tidak berpindah, dan kunjungan langsung selesai bila
     tidak ada kewajiban lain.
4. Sesudah *Selesaikan* berhasil, kartu pasien dimuat ulang; bila observasi itu penahan terakhir, statusnya terbaca
   *Selesai* tanpa memuat ulang halaman.

*Contoh.* Dokter menjalankan tindak lanjut pulang untuk Pak Budi pukul 10.00 sementara observasinya masih berjalan.
Kartu pasien berubah menjadi *Tindak lanjut ditetapkan*. Perawat membuka tab Observasi: tombol Eskalasi kelabu dengan
keterangannya. Ia menekan Selesaikan, membaca *"Tindak lanjut pasien sudah dilaksanakan, sehingga status kunjungan
tidak berpindah. Bila tidak ada kewajiban lain yang tersisa, kunjungan langsung selesai…"*, mengisi kesimpulan
*"tanda vital stabil"*, dan menekan Selesaikan. Kartu pasien berubah menjadi *Selesai*.

**Jalur tidak normal.**

| Keadaan | Yang terjadi |
| --- | --- |
| Status kunjungan berubah di antara muat dan tekan, lalu server menjawab `409` | Pesan server tampil apa adanya di dalam modal — kini juga pada *Eskalasi* dan *Batalkan*; isian alasan tetap utuh |
| Kunjungan selain *Tindak lanjut ditetapkan* | Kalimat dan tombol observasi sama persis seperti sebelumnya |
| Pemuatan ulang kartu pasien gagal | Pesan galat kunjungan tampil di atas layar; kalimat observasi kembali ke bunyi bawaan, dan penolakan tetap dijaga server |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

Kartu `FE-IGD-042` beserta tabel isi kalimat wajib; laporan `BE-IGD-061`.
Source: `confirm-modal.jsx`, `information-alert.jsx`, `emergency-assessment-patient-card.jsx`,
`emergency-assessment-slice.jsx` (`fetchAssessmentVisitContext` dan reducernya), `emergency-assessment.module.css`,
`emergency-registration.constants.js` (`EMERGENCY_VISIT_STATUS`), dan seluruh berkas pada 3.2.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/…/emergency-assessment-constant.jsx` | Kalimat *Jalankan* diganti; *Selesaikan* (dari Aktif dan dari Dieskalasi) + `disposedConfirmMessage`; *Eskalasi* + `disabledWhenVisitDisposed`; + `OBSERVATION_ESCALATION_DISPOSED_HINT`. +9/−1 |
| `src/utils/…/emergency-assessment-status-action.utils.js` (**baru**) | `isVisitDisposed`, `resolveObservationStatusActions`, `resolveObservationActionHint` — fungsi murni pemilih kalimat dan keadaan tombol. 30 baris |
| `src/lib/hooks/…/use-emergency-assessment-detail.jsx` | + `refreshVisit` — memanggil ulang `fetchAssessmentVisitContext`. +7 |
| `src/components/view/…/emergency-assessment-detail-view.jsx` | Meneruskan `visitStatus` dan `onVisitChanged` ke tab Observasi, `onVisitChanged` ke tab Tindak Lanjut. +4 |
| `src/components/view/…/components/emergency-assessment-observation-tab.jsx` | Aksi dipilih menurut status kunjungan; tombol nonaktif tidak membuka modal; keterangan di bawah deret tombol; kartu pasien dimuat ulang sesudah aksi berhasil; pesan penolakan tampil di modal untuk semua aksi. +37/−8 |
| `src/components/view/…/components/emergency-assessment-disposition-tab.jsx` | Kartu pasien dimuat ulang sesudah aksi status berhasil. +3/−1 |

Nol baris komentar baru; hitungan komentar tiap berkas lama tidak berubah. **Tidak disentuh:** CSS mana pun, route,
menu, slice, `ConfirmModal`, backend.

### 3.3 Kepatuhan arsitektur frontend

View → hook → slice → util/constant; nol permintaan baru di view; aturan pemilihan kalimat berupa fungsi murni di
`src/utils`.

`UI GATE: 4 elemen — REUSE 4, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Modal konfirmasi | `ConfirmModal` yang sudah dipakai kedua tab | `base-features/confirm-modal.jsx` | `REUSE` | — |
| Pesan penolakan di modal | `InformationAlert` (`danger`) | Sudah dipakai modal *Selesaikan* pada tab yang sama | `REUSE` | — |
| Tombol aksi nonaktif | `<button>` berkelas `primaryMiniButton` yang sudah ada, atribut `disabled` | Deret `recordActions` pada tab Observasi | `REUSE` | — |
| Keterangan tombol nonaktif | Kelas `formHint` | `emergency-assessment-form-card.jsx` — keterangan mengapa tombol simpan belum dapat ditekan | `REUSE` | — |

Grep anti-regresi pada berkas baru dan baris yang ditambah: nol `<button>` baru, nol `<table>`, nol utilitas tipografi
Bootstrap baru, nol stylesheet berubah.

### 3.4 Keputusan pelaksanaan

| Keputusan | Alasan |
| --- | --- |
| Keterangan Eskalasi memakai kalimat yang sama dengan penolakan server | Satu bunyi untuk satu aturan (validation §11.1 aturan 14) |
| Keterangan ditulis sebagai teks di bawah deret tombol, bukan `title` | Kartu: wajib terbaca tanpa kursor |
| Kartu pasien dimuat ulang sesudah **setiap** aksi status yang berhasil pada kedua tab, bukan hanya *Selesaikan* dan *Jalankan* | Satu permintaan `GET`; aksi mana pun dapat mengubah status kunjungan, dan status yang basi membuat kalimat yang dipilih ikut keliru (risiko pada kartu) |
| Pesan penolakan kini tampil di modal untuk *Eskalasi* dan *Batalkan* | Kriteria 4. Sebelumnya pesannya tersimpan tetapi tidak pernah ditampilkan pada dua aksi itu |
| Aksi pada kunjungan selain `Disposed` dikembalikan sebagai daftar asalnya, tanpa disalin | Kriteria 6 — fungsi pemilih mengembalikan daftar asalnya, bukan salinannya |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tombol aksi nonaktif selagi menyimpan (tidak berubah); kartu pasien tetap menampilkan data lama selagi dimuat ulang, tanpa berkedip |
| Kosong | `NOT APPLICABLE` — periode tanpa aksi (Selesai, Dibatalkan) tidak menampilkan tombol maupun keterangan |
| Gagal | Pesan server di dalam modal; modal tetap terbuka dan isian utuh |
| Tanpa hak akses | Pesan penolakan server di dalam modal (jalur yang sama) |

---

## 5. Endpoint yang dikonsumsi

#### Health Services / Emergency Installation Management / Emergency Visit

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/health-services/emergency-installation-management/emergency-visits/{id}` | Memuat ulang kartu pasien sesudah aksi status — endpoint yang sudah dipakai, kini dipanggil lagi | `EmergencyVisit : Read` |

Nol endpoint baru. `PATCH …/observation-status` dan `PATCH …/disposition-status` dipakai seperti sebelumnya.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` enam berkas task | 0 error, 1 warning | `PASS` | Warning `react-hooks/set-state-in-effect` pada baris 504 tab Observasi — sudah ada sebelum task ini |
| Unit test | Tidak dipakai | `SKIPPED` | Perintah pemilik 1 Oktober 2026: *"gausah pake unit test"*. Berkas test yang sempat ditulis untuk task ini (tujuh kasus) dihapus dan tidak ikut pada working tree |
| Baris komentar pada tambahan | Nol | `PASS` | Hitungan komentar tiap berkas sebelum dan sesudah |
| Akhiran baris | CRLF dipertahankan | `PASS` | Pemeriksaan byte |
| Nol komponen, route, menu, CSS global | Nol berkas di `src/app`, `src/style`; `menu-items.jsx` tidak disentuh task ini | `PASS` | `git diff --numstat` |
| `npm run build` | — | `NOT RUN` | Milik pemilik |
| Uji layar U1–U8 | — | `NOT RUN` | Milik pemilik; menunggu build `BE-IGD-061` |

`AUTOMATED TEST: SKIPPED (opsional) — atas perintah pemilik 1 Oktober 2026`

`MANUAL TEST: NOT FEASIBLE — backend tidak berjalan pada sesi ini dan uji layar dijalankan pemilik`

Uji manual: `REQUIRED`.

### 6.1 Skenario uji layar untuk pemilik

| # | Langkah | Yang diharapkan | Kriteria |
| ---: | --- | --- | ---: |
| U1 | Kunjungan dengan tindak lanjut `Dikonfirmasi`: tab Tindak Lanjut, tekan Jalankan | Modal memuat *"Kunjungan langsung selesai bila tidak ada kewajiban yang tersisa…menunggu penutupan dan tertutup sendiri…"*; tidak ada *"belum menyelesaikan kunjungan"* | 7 |
| U2 | Lanjutkan U1 pada kunjungan yang observasinya masih aktif: konfirmasi Jalankan | Kartu pasien berubah menjadi *Tindak lanjut ditetapkan* tanpa memuat ulang halaman | 5 |
| U3 | Dari U2, pindah ke tab Observasi | Tombol Eskalasi nonaktif; keterangan *"Tindak lanjut pasien sudah dilaksanakan; eskalasi tidak dapat dicatat pada kunjungan ini."* terbaca di bawah deret tombol; menekannya tidak memunculkan modal dan nol permintaan pada tab jaringan | 3, 5 |
| U4 | Dari U3, tekan Selesaikan | Modal memuat *"…status kunjungan tidak berpindah…kunjungan langsung selesai"*; tidak ada *"Menunggu Tindak Lanjut"* | 1 |
| U5 | Dari U4, isi kesimpulan, konfirmasi | Periode menjadi Selesai beserta kesimpulannya; kartu pasien terbaca *Selesai* tanpa memuat ulang halaman | 2 |
| U6 | Ulangi U3–U4 pada periode berstatus Dieskalasi di kunjungan `Disposed` | Modal Selesaikan memuat isi yang sama dengan awalan *"Menutup periode yang sudah dieskalasi."* | 1 |
| U7 | Kunjungan *Dalam observasi* (bukan `Disposed`): buka tab Observasi | Eskalasi aktif; kalimat Selesaikan dan Eskalasi sama persis seperti sebelumnya; tidak ada keterangan di bawah tombol | 6 |
| U8 | Buka tab Observasi pada kunjungan yang belum `Disposed`, jalankan tindak lanjutnya dari tab peramban lain, lalu tekan Eskalasi dan isi alasan | `409` *"Tindak lanjut pasien sudah dilaksanakan; eskalasi tidak dapat dicatat pada kunjungan ini."* tampil di dalam modal; alasan yang diketik tetap ada | 4 |

---

## 7. Acceptance criteria dan Definition of Done

| # | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 1 | Modal *Selesaikan* pada `Disposed` memuat isi wajib, tanpa Menunggu Tindak Lanjut — Aktif maupun Dieskalasi | **Terpenuhi pada source** | `disposedConfirmMessage` pada kedua aksi *Selesaikan*, dipilih `resolveObservationStatusActions`. Uji layar U4, U6 belum |
| 2 | Menyelesaikan penahan terakhir → kartu pasien terbaca Selesai tanpa muat ulang halaman | **Terpenuhi pada source** | `onVisitChanged` sesudah aksi berhasil. Uji layar U5 belum — sekaligus bergantung pada `BE-IGD-061` |
| 3 | Eskalasi nonaktif dengan keterangan terbaca tanpa kursor; nol permintaan | **Terpenuhi pada source** | `disabledWhenVisitDisposed` pada aksi *Eskalasi*; `disabled` + penjaga pada `openAction`; keterangan berkelas `formHint`. Uji layar U3 belum |
| 4 | `409` aturan 14 tampil apa adanya di modal, isian utuh | **Terpenuhi pada source** | Cabang pesan pada badan modal untuk aksi tanpa kesimpulan; alasan disimpan `ConfirmModal` selama modal terbuka. Uji layar U8 belum |
| 5 | Kalimat dan tombol mengikuti status kunjungan terkini sesudah *Jalankan* pada sesi yang sama | **Terpenuhi pada source** | `refreshVisit` dipanggil tab Tindak Lanjut; tab Observasi membaca `visit.visitStatus`. Uji layar U2, U3 belum |
| 6 | Selain `Disposed`, kalimat dan tombol observasi sama persis | **Terpenuhi pada source** | `resolveObservationStatusActions` mengembalikan daftar asal untuk status selain `Disposed`, termasuk status kosong. Uji layar U7 belum |
| 7 | Modal *Jalankan* memuat isi wajib | **Terpenuhi pada source** | Kalimat baru pada `DISPOSITION_STATUS_ACTIONS`. Uji layar U1 belum |
| 8 | Nol komponen baru, route, menu, perubahan `globals.css` | **Terpenuhi** | Bagian 6, baris keenam. Satu berkas util baru; nol komponen |
| 9 | `eslint` 0 error; unit test pemilihan kalimat dan keadaan tombol; build pemilik | **Sebagian** | `eslint` 0 error; unit test dikecualikan atas perintah pemilik 1 Oktober 2026; build belum |

DoD: laporan tracked ✅ (berkas ini). Uji layar dijalankan satu siklus bersama uji `BE-IGD-061` — belum.

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tiga komentar lama kini tidak lagi cocok dengan perilakunya dan sengaja tidak disunting: komentar badan modal pada tab Observasi (*"Eskalasi dan Batalkan tetap memakai jalur bawaan"*), komentar kepala tab Tindak Lanjut (*"menetapkan tindak lanjut bukan menyelesaikan kunjungan"*), dan komentar kepala `OBSERVATION_STATUS_ACTIONS` yang tidak menyebut dua ruas baru |
| Masalah yang diketahui | (1) Tab Transfer tidak memuat ulang kartu pasien: sejak `BE-IGD-061`, menerima atau menolak serah-terima dapat menutup kunjungan, tetapi status pada kartu baru berubah sesudah halaman dimuat ulang — di luar kartu ini. (2) Bila pemuatan ulang kartu pasien gagal, slice mengosongkan konteks kunjungan (perilaku reducer yang sudah ada), sehingga kartu pasien kosong sampai halaman dimuat ulang |
| Dependency backend | `BE-IGD-061` 🟡 — tanpa build itu, U2 berhenti di *Dalam observasi* versi lama dan U5 tidak menutup kunjungan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Enam berkas oleh task ini (lima diubah, satu baru). Tanpa stage, commit, atau push |
| Langkah berikutnya | Pemilik: build backend dan frontend, lalu U1–U8 bersama S1–S12 `BE-IGD-061`. Agent: `FE-IGD-041` |

---

## Pemeriksaan bukti uji gabungan — 2 Oktober 2026

Bukti mentah di `QuilvianSystemFrontendDev/test-with-agy/igd/` (`results-tahap-1.json`…`results-tahap-4.json`, skrip `test-tahap-*.mjs`, tangkapan layar `<ID>.png`) dan log backend `Logs/quilvian-backend-20261001.json`, `quilvian-backend-20261002.json`. Ringkasan agen penguji ([laporan uji gabungan](../../../testing/2026-10-01-laporan-uji-gabungan-r313-r314.md)) **tidak** dipakai sebagai bukti: uraian skenarionya pada beberapa task tidak sama dengan panduan, dan daftar `FAIL`-nya tidak cocok dengan JSON mentah.

`npm run build` pemilik terbukti dari artefak: `.next/BUILD_ID` bertanggal 1 Oktober 2026 15.22, sesudah edit source terakhir (14.59). **Uji layar dilayani `next dev`** (lencana "N" pada setiap tangkapan layar), bukan hasil build. Source di-commit pemilik sebagai `de70687a9`.

| Skenario | Putusan | Yang teramati pada bukti mentah |
| --- | --- | --- |
| `042-U1` | **Terbukti** | Modal Jalankan memuat kalimat baru; tidak ada *"belum menyelesaikan kunjungan"* |
| `042-U2` | **Terbukti** | Kartu pasien *Tindak lanjut ditetapkan*; tindak lanjut *Dijalankan* |
| `042-U3` | **Terbukti** | Eskalasi nonaktif; keterangan terbaca di bawah deret tombol; nol permintaan |
| `042-U4` | **Terbukti** | Modal Selesaikan memuat *"…status kunjungan tidak berpindah…kunjungan langsung selesai"* |
| `042-U5` | **Terbukti** | Periode *Selesai* beserta kesimpulan; kartu pasien *Selesai* |
| `042-U6` | **Terbukti** | Modal berawalan *"Menutup periode yang sudah dieskalasi."* pada kunjungan `Disposed` yang masih punya periode berjalan |
| `042-U7` | **Terbukti** | Kunjungan *Sedang ditangani*: Eskalasi aktif, tanpa keterangan |
| `042-U8` | **Terbukti** | `409` dengan kalimat `IGD-DEC-172` tampil di dalam modal; alasan yang diketik tetap ada |

**Catatan.**

- Terlihat pada `042-U8.png`: pesan `409` tampil dua kali (di modal dan di bawah formulir Buka Periode Observasi), dan kartu pasien masih *Sedang ditangani* karena konteks kunjungan hanya dimuat ulang sesudah aksi berhasil.
- `042-U6` hanya memperlihatkan modalnya; hasil penyelesaian periode Dieskalasi tidak dijalankan (lihat `061-S6` pada laporan `BE-IGD-061`).

Putusan: **✅ selesai** — Build pemilik dan uji layar U1–U8 **8 dari 8** pada bukti mentah (tangkapan layar dan teks modal). Layar dilayani `next dev`, bukan hasil build. Tanpa UAT.

---

## Pengerjaan ulang — 4 Oktober 2026: tombol observasi pada kunjungan yang sudah berakhir

| Field | Nilai |
| --- | --- |
| Pemicu | `BE-IGD-061` diperluas 3 Oktober 2026 (`IGD-DEC-183`, `184`): pada kunjungan `Completed`/`Cancelled`, *selesaikan*, *eskalasi*, dan *aktifkan* observasi pasti ditolak `409` aturan 18, sementara layar masih menawarkannya dan modal *Selesaikan* menjanjikan *"Kunjungan pasien berpindah ke Menunggu Tindak Lanjut"* |
| Keputusan | `IGD-DEC-187` — *Selesaikan* dan *Eskalasi* nonaktif beserta keterangan; *Batalkan* tetap aktif; pesan `409` tetap tampil apa adanya bila data berubah di antara muat dan tekan |
| Wewenang | Rizki Gunawan, 4 Oktober 2026: *"Kerjakan ulang FE-IGD-042 lewat build-module-frontend (IGD-DEC-187)… Tanpa unit test baru (perintah saya 1 Oktober 2026)"* |
| Contract version | validation `0.11.0` §11.2 aturan 18–19; state `0.8.0` §9.6; API `0.14.0` §9.1 nomor 7 — `approved` (`IGD-DEC-186`). Hash ketiga berkas cocok dengan manifest bagian 0k (dihitung ulang 4 Oktober 2026, isi berakhir-baris LF) |
| Kartu | `frontend-roadmap.md` R3.13.1 — acceptance 10–13 |
| Wewenang UI | `DEV_DISCRETION` untuk rupa keterangan (`IGD-DEC-187`), dengan batas: terbaca tanpa kursor |
| Klasifikasi | `LIGHT` — skor 3: repository 0, berkas diperiksa 1, berkas diubah 0 (2 berkas), logika 1, kontrak API 0, database 0, keamanan/auth 0, UI/workflow 1 |
| Commit frontend dasar | `19ba512de` (`RizkiV2`, `ahead 3`), working tree bersih sebelum dikerjakan |
| Commit backend rujukan | `c1f79f79` (`rizkiG`) — `BE-IGD-061` pengerjaan ulang, build Rizki 3 Oktober 2026 0 error, 0 warning |
| Model | Claude Opus 5.5 |
| Tanggal | 4 Oktober 2026 |

### Proses bisnis dari sisi pengguna

Perawat membuka Ruang Kerja Pemeriksaan IGD milik pasien yang kunjungannya sudah *Selesai* atau *Dibatalkan*, lalu
tab Observasi. Pada periode yang masih *Sedang berjalan* atau *Dieskalasi*:

1. Tombol **Selesaikan** dan **Eskalasi** tampil kelabu dan tidak dapat ditekan. Menekannya tidak membuka modal dan
   tidak mengirim permintaan apa pun.
2. Di bawah deret tombol terbaca: *"Kunjungan IGD ini sudah berakhir; observasinya tidak dapat diselesaikan,
   dieskalasi, atau diaktifkan kembali."*
3. Tombol **Batalkan** tetap aktif. Modalnya berbunyi sama seperti sebelumnya, dan pembatalan berhasil tanpa mengubah
   status kunjungan.

*Contoh.* Kunjungan Pak Budi selesai kemarin, tetapi periode observasinya pukul 10.00 masih *Dieskalasi* — data lama
sebelum `IGD-DEC-183`. Perawat membuka tab Observasi: hanya ada *Selesaikan* (kelabu) dan *Batalkan*, dengan kalimat
di atas di bawahnya. Bila periode itu memang salah dibuka, perawat menekan Batalkan.

**Jalur tidak normal.**

| Keadaan | Yang terjadi |
| --- | --- |
| Kunjungan dibatalkan dari tempat lain sesudah layar dimuat, lalu perawat menekan *Selesaikan* | Layar masih memakai status lama, jadi modal terbuka seperti biasa. Server menjawab `409` aturan 18, dan pesannya tampil apa adanya di dalam modal; isian kesimpulan tetap utuh (jalur `actionError` yang sudah ada). Sesudah halaman dimuat ulang, tombol tampil nonaktif |
| Kunjungan `Disposed` yang tertahan observasi Dieskalasi | Tidak berubah: *Selesaikan* aktif dengan kalimat keadaan `Disposed`; menyelesaikannya menutup kunjungan dan kartu pasien terbaca *Selesai* tanpa memuat ulang halaman (acceptance 13) |
| Kunjungan masih berjalan atau `Disposed` | Kalimat dan tombol sama persis seperti sebelumnya |
| Status kunjungan kosong (kartu pasien gagal dimuat) | Daftar aksi asal — penolakan tetap dijaga server |

### Berkas yang diperiksa dan berubah

Diperiksa: kartu R3.13.1 acceptance 10–13 beserta tabel isi kalimat wajib; validation §11.2;
`EmergencyObservationController.UpdateObservationStatus` (urutan pemeriksaan backend); `emergency-assessment-observation-tab.jsx`
(`openAction`, deret `recordActions`, keterangan `formHint`, badan `ConfirmModal`); `emergency-assessment-detail-view.jsx`
(`visitStatus` dari kartu pasien); `EMERGENCY_VISIT_STATUS` di `emergency-registration.constants.js`.

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/health-services/emergency-installation-management/emergency-assessment-constant.jsx` | Ruas `disabledWhenVisitEnded: true` pada *Selesaikan* (dari Aktif dan dari Dieskalasi) dan *Eskalasi*; *Batalkan* tidak diberi ruas itu. Konstanta baru `OBSERVATION_VISIT_ENDED_HINT` berisi kalimat aturan 18. +6 |
| `src/utils/health-services/emergency-installation-management/emergency-assessment-status-action.utils.js` | `isVisitEnded` (`Completed` atau `Cancelled`); `resolveObservationStatusActions` memeriksa kunjungan berakhir **sebelum** pemeriksaan `Disposed` dan menandai aksi berpenanda `disabled` beserta `disabledReason`. +16 |

**Tidak disentuh:** tab Observasi — penjaga `openAction`, atribut `disabled`, dan keterangan `formHint` dari pengerjaan
1 Oktober sudah menangani daftar aksi baru tanpa perubahan; hook, slice, view detail, CSS, route, menu, backend. Nol
baris komentar baru. Akhiran baris CRLF dipertahankan.

### Keputusan base component

`UI GATE: 2 elemen — REUSE 2, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`

| Kebutuhan UI | Kandidat | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Tombol *Selesaikan*/*Eskalasi* nonaktif | Tombol `primaryMiniButton` + atribut `disabled`, penjaga `openAction` | `emergency-assessment-observation-tab.jsx` deret `recordActions`; pola `IGD-DEC-177` | `REUSE` | — |
| Keterangan terbaca tanpa kursor | Kelas `formHint` di bawah deret tombol, diisi `resolveObservationActionHint` | Tab yang sama; dipakai keterangan Eskalasi `Disposed` | `REUSE` | — |

### Keputusan pelaksanaan

| Keputusan | Alasan |
| --- | --- |
| Penanda per aksi (`disabledWhenVisitEnded`) di konstanta, bukan aturan "target selain Batalkan" di util | Mengikuti pola `disabledWhenVisitDisposed` yang sudah ada; aksi mana yang nonaktif terbaca di satu tempat bersama kalimatnya |
| Kunjungan berakhir diperiksa sebelum `Disposed` | Keduanya tidak tumpang tindih, tetapi urutan ini membuat cabang `Disposed` tidak pernah menerima kunjungan berakhir bila kelak ada status baru |
| Kalimat konfirmasi aksi nonaktif dibiarkan apa adanya | Modal tidak dapat dibuka dari tombol nonaktif; *Batalkan* memakai kalimat asal (acceptance 11) |
| Satu keterangan untuk dua tombol | `resolveObservationActionHint` mengambil alasan pertama; kalimat aturan 18 sudah mencakup kedua aksi |
| Tombol tambah pemantauan tidak dinonaktifkan | Di luar `IGD-DEC-187` (kartu, *Juga di luar kartu ini*); penolakan aturan 21 tampil lewat jalur galat formulir pemantauan |

### State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Tidak berubah |
| Kosong | `NOT APPLICABLE` — periode *Selesai*/*Dibatalkan* tidak punya tombol aksi |
| Gagal | `409` aturan 18 tampil di dalam modal (acceptance 12) |
| Tanpa hak akses | Tidak berubah — pesan server di dalam modal |
| Nonaktif | Tombol kelabu + keterangan aturan 18 di bawah deret tombol |

### Endpoint yang dikonsumsi

Nol endpoint baru dan nol permintaan baru. Status kunjungan dibaca dari kartu pasien yang sudah dimuat
(`GET /v1/health-services/emergency-installation-management/emergency-visits/{id}`); `PATCH
/v1/health-services/emergency-installation-management/emergency-observations/{id}/observation-status` dipakai seperti
sebelumnya, kini hanya untuk aksi yang masih aktif.

### Validasi

| Perintah atau pemeriksaan | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint` dua berkas task | 0 error, 0 warning | `PASS` |
| Prettier pada baris tambahan | Selisih format berkas konstanta 41 baris sebelum dan sesudah — seluruhnya blok nosokomial lama; util sama persis dengan keluaran Prettier | `PASS` (selisih lama: `EXISTING WARNING`) |
| Grep anti-regresi 1–6 (`ui-consistency-checklist` G) pada baris tambahan | Nol temuan pada keenam pola; nol komentar baru | `PASS` |
| Pemeriksaan pemilih aksi — skrip sementara di luar repository, bukan unit test | 32 kombinasi (observasi Aktif/Dieskalasi × status kunjungan kosong, 1–9, dan teks `"8"`/`"9"`; observasi Selesai/Dibatalkan × 7–9): pada 8 dan 9 *Selesaikan*/*Eskalasi* nonaktif dengan kalimat aturan 18 dan *Batalkan* aktif dengan kalimat asal; pada 7 perilaku `Disposed` lama; selain itu daftar asal (referensi yang sama) | `PASS` |
| Uji IGD lewat pola berkas | 91/91 | `PASS` |
| `npm run build` | **Lulus** — `✓ Compiled successfully in 2.0min`, 465/465 halaman, 0 error, 0 warning; `postbuild` standalone berhasil; `.next/BUILD_ID` 4 Oktober 2026 11.46, sesudah edit terakhir 11.40. Port 3000 kosong sebelum build | `PASS` |
| Isi hasil build | Chunk `32x943ekyng6t.js` memuat `disabledWhenVisitEnded` dan kalimat aturan 18 | `PASS` |
| Server uji | `node .next/standalone/server.js` dinyalakan dari hasil build ini (PID 22944, port 3000). `GET /login` `200`, halaman Pengkajian `200`, chunk di atas `200` | `PASS` |

`AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/emergency-*.test.mjs — PASS (91/91)`

`AUTOMATED TEST: SKIPPED (opsional) untuk test baru — perintah pemilik 1 Oktober 2026`

`MANUAL TEST: NOT FEASIBLE — layar butuh login, backend berjalan, dan kunjungan berakhir dengan observasi terbuka; uji layar dijalankan pemilik lewat Antigravity pada hasil build`

`npm run test:unit` tidak dipakai: runner gagal pada Node 24 karena argumen folder (`UNRELATED EXISTING ISSUE`).

### Skenario uji layar

Dijalankan dalam satu siklus bersama `BE-IGD-061` S13–S19 dan `FE-IGD-041`, memakai
[panduan uji gabungan `MVP-8`](../../../testing/2026-10-04-panduan-uji-gabungan-mvp-8.md): `042-U9`…`042-U13`
(acceptance 10–13) dan regresi singkat U1–U8.

### Acceptance criteria — pengerjaan ulang

| # | Kriteria | Status | Bukti |
| ---: | --- | --- | --- |
| 10 | Kunjungan `Completed`/`Cancelled`: *Selesaikan* dan *Eskalasi* nonaktif, keterangan aturan 18 terbaca tanpa kursor, nol permintaan | **Terpenuhi pada source** | `disabledWhenVisitEnded` + `OBSERVATION_VISIT_ENDED_HINT`; tombol `disabled` dan penjaga `openAction` yang sudah ada. Uji layar `042-U10`, `042-U12` belum |
| 11 | *Batalkan* tetap aktif dan berhasil; kalimatnya tidak berubah | **Terpenuhi pada source** | *Batalkan* tanpa penanda; kalimat diteruskan apa adanya. Uji layar `042-U11` belum |
| 12 | `409` aturan 18 tampil apa adanya di modal | **Terpenuhi pada source** | Jalur `actionError` 1 Oktober — tidak berubah. Uji layar `042-U9` belum |
| 13 | Kunjungan tertahan observasi Dieskalasi: *Selesaikan* aktif dengan kalimat `Disposed`, menyelesaikannya → kartu *Selesai*; acceptance 1–6 tidak berubah | **Terpenuhi pada source** | Cabang `Disposed` tidak berubah; kunjungan berakhir diperiksa lebih dulu dan tidak bertumpang. Uji layar `042-U13` dan regresi U1–U8 belum |
| 9 | `eslint` 0 error; build | **Terpenuhi untuk pengerjaan ulang** | `eslint` 0 error, 0 warning; `npm run build` lulus 11.46. Unit test dikecualikan atas perintah pemilik |

### Catatan

| Hal | Isi |
| --- | --- |
| Masalah yang diketahui | Kunjungan `Completed` dengan observasi **Aktif** tidak dapat dibuat lewat alur normal (observasi Aktif menahan penutupan sejak `BE-IGD-018`), jadi *Eskalasi* nonaktif pada kunjungan *Selesai* hanya teruji bila data lama semacam itu ada; pada kunjungan *Dibatalkan* teruji (`042-U10`) |
| Masalah lama, tidak diperbaiki | Kartu pasien hanya dimuat ulang sesudah aksi berhasil, jadi sesudah `409` aturan 18 tombol baru tampil nonaktif setelah halaman dimuat ulang. Tombol tambah pemantauan tidak dinonaktifkan pada kunjungan berakhir (di luar `IGD-DEC-187`) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Dua berkas source (`git diff --stat`: konstanta +6, util +16). Tanpa stage, commit, atau push |
| Langkah berikutnya | Pemilik menjalankan panduan uji gabungan `MVP-8` lewat Antigravity pada server standalone port 3000 (hasil build 11.46); agent memeriksa bukti mentahnya |

Putusan pengerjaan ulang: **🟡 sebagian** — acceptance 10–13 terpenuhi pada source, `eslint` dan build bersih. Yang
tersisa hanya bukti uji layar `042-U9`…`U13` dan regresi U1–U8 pada hasil build.

---

## Pemeriksaan bukti uji gabungan `MVP-8` — 4 Oktober 2026

Bukti mentah di `QuilvianSystemFrontendDev/test-with-agy/igd/uji-gabungan-mvp-8-2026-10-04/` (`042-*.json`, `042-*.png`,
`runner-block-b.mjs`, `runner-block-d.mjs`, `runner-block-e.mjs`, `runner-042-u3-try2.mjs`), dicocokkan dengan log backend
`Logs/quilvian-backend-20261004.json`. Semua tangkapan layar uji tanpa lencana "N" (diperiksa per piksel);
`nextjsPortalNull` benar pada setiap skenario; viewport 1440 × 900; akun Perawat IGD. Untuk pertama kalinya task ini
diuji pada **hasil build** — putaran 2 Oktober dilayani `next dev`. [Laporan penguji](../../../testing/2026-10-04-laporan-uji-gabungan-mvp-8.md)
tidak dipakai sebagai bukti (bagian 7 laporan itu). Agen penguji mengubah Akses Role sebelum uji; pemilik mengesahkannya
dan menerima bukti (`IGD-DEC-188`).

| Skenario | Kriteria | Putusan | Yang teramati pada bukti mentah |
| --- | ---: | --- | --- |
| `042-U9` | 12 | **Terbukti** | `K4` dibuka (*Sedang ditangani*); dari skrip `PATCH visit-status` `{ 8 }` `200`. Tanpa memuat ulang: *Selesaikan* → modal berkalimat lama (layar masih memakai status lama, sesuai harapan) → `PATCH observation-status` `409` dengan kalimat aturan 18 persis; modal tetap terbuka, kotak merah berisi kalimat itu, kesimpulan *"uji 042-U9"* utuh |
| `042-U10` | 10 | **Terbukti** — JSON `FAIL` karena skrip | `K4` dimuat ulang: kartu *Dibatalkan*; *Selesaikan* dan *Eskalasi* `disabled`, *Batalkan* tidak; klik paksa: nol modal, nol permintaan. Pemeriksaan keterangan gagal karena skrip mencarinya **di dalam** deret tombol (`container.querySelector`), padahal elemennya saudara di bawahnya — cacat yang sama dengan `042-U3` percobaan 1. `042-U10.png` memperlihatkan kalimat aturan 18 utuh di bawah tombol. Kalimat yang sama diperiksa otomatis dan lulus pada `042-U12` |
| `042-U11` | 11 | **Terbukti** — JSON `FAIL` karena skrip | Modal memuat kalimat *Batalkan* asal; `PATCH` `{ 4 }` `200`, respons `observationStatus` 4; kartu tetap *Dibatalkan*, `visitStatus` 8. Pemeriksaan periode gagal karena skrip mencari `"Dibatalkan"` peka huruf, sedangkan teks periode terbaca `"DIBATALKAN"` (huruf kapital dari CSS) |
| `042-U12` | 10 | **Terbukti** | `D-S6` (*Selesai*, periode *Dieskalasi*): tombol *Selesaikan* `disabled` dan *Batalkan* aktif; tidak ada *Eskalasi*; keterangan `innerText` sama persis dengan kalimat aturan 18 dan terlihat; klik paksa: nol modal, nol `PATCH` |
| `042-U13` | 13, regresi U6 | **Terbukti** — catatan jaringan JSON disusun ulang penguji | Sebelum aksi: kartu *Tindak lanjut ditetapkan*, *Selesaikan* dan *Batalkan* aktif, tanpa keterangan. Modal memuat kalimat *Selesaikan* periode Dieskalasi pada `Disposed` dan tidak menyebut Menunggu Tindak Lanjut. Sesudah aksi: kartu *Selesai*, penanda `window` utuh; baris kunjungan 9, `ClosedByDispositionId` = tindak lanjut `K3`, `UpdateBy` = perawat. **Catatan jaringan di JSON bukan rekaman**: pesan suksesnya *"Status observasi gawat darurat berhasil diubah."* tidak ada di backend, stempel waktunya dibulatkan, dan berkas ditulis 55 detik sesudah PNG. Peristiwanya dibuktikan log backend: 14.01.31 `PATCH` observasi `K3` dari klien Google Chrome atas nama perawat, disusul `EmergencyVisit.CompleteByDisposition` *"Kunjungan IGD ditutup karena penahan terakhirnya dibereskan."* |
| `042-U7` | 6 | **Terbukti** | `K4` *Sedang ditangani*: ketiga tombol aktif, tanpa keterangan; modal *Selesaikan* dan *Eskalasi* berkalimat lama; nol `PATCH` |
| `042-U1` | 7 | **Terbukti** | `K5`: modal *Jalankan* memuat kalimat baru; tidak ada *"belum menyelesaikan kunjungan"* |
| `042-U2` | 5 | **Terbukti** | `PATCH disposition-status` `200`; `GET` kunjungan → 7; kartu *Tindak lanjut ditetapkan* tanpa memuat ulang |
| `042-U3` | 3, 5 | **Terbukti** (percobaan 2; percobaan 1 dicatat) | Percobaan 1 pada `K5`, sesi yang sama dengan U2: *Eskalasi* `disabled`; `042-U3.png` memperlihatkan keterangan aturan 14 di bawah tombol. JSON-nya `FAIL` karena selector keterangan yang sama kelirunya dan jendela rekam jaringan ikut menangkap tiga `GET` pemuatan tab. Percobaan 2 pada `K5b` (kunjungan lain, `Disposed`): keterangan sama persis dan terlihat, klik paksa nol modal dan nol permintaan |
| `042-U4` | 1 | **Terbukti** | Modal *Selesaikan* memuat kalimat keadaan `Disposed`; tidak menyebut Menunggu Tindak Lanjut |
| `042-U5` | 2 | **Terbukti** | `PATCH` `{ 2, "tanda vital stabil" }` `200`; `GET` kunjungan → 9; periode *Selesai* berkesimpulan; kartu *Selesai* tanpa memuat ulang |
| `042-U8` | 4 | **Terbukti** | `K6` dibuka (*Sedang ditangani*); tindak lanjut dijalankan dari skrip `200`; *Eskalasi* di layar → `409` kalimat aturan 14 persis, tampil di modal, alasan *"uji 042-U8"* utuh |

**Catatan.**

- **Temuan konfigurasi, di luar task ini.** Pada Ruang Kerja, akun Perawat IGD ditolak `403` untuk `GET
  emergency-triages`, `patient-assessments`, `patient-vital-signs`, dan `master-data/emergency-disposition-types`. Elemen
  yang diuji tidak bergantung pada data itu, tetapi riwayat triage, tanda vital, dan pilihan jenis tindak lanjut kosong
  bagi perawat — perlu dibenahi pada Akses Role sebelum UAT.
- *Eskalasi* nonaktif pada kunjungan **Selesai** tetap tidak teruji (keterbatasan yang sudah dicatat: kunjungan Selesai
  dengan observasi Aktif tidak dapat dibuat lewat alur normal); pada kunjungan *Dibatalkan* teruji (`042-U10`).

| # | Kriteria | Status |
| ---: | --- | --- |
| 1–8 | Kriteria 1 Oktober | **Terpenuhi** — regresi U1–U8 pada hasil build (sebelumnya hanya pada `next dev`) |
| 9 | `eslint` 0 error; unit test; build | **Terpenuhi dengan pengecualian** — unit test dikecualikan atas perintah pemilik 1 Oktober 2026 |
| 10 | *Selesaikan*/*Eskalasi* nonaktif + keterangan aturan 18; nol permintaan | **Terpenuhi** — `042-U10`, `042-U12` |
| 11 | *Batalkan* tetap aktif dan berhasil | **Terpenuhi** — `042-U11` |
| 12 | `409` aturan 18 tampil apa adanya di modal | **Terpenuhi** — `042-U9` |
| 13 | Kunjungan tertahan observasi Dieskalasi → *Selesaikan* menutup kunjungan tanpa memuat ulang; perilaku 1–6 tidak berubah | **Terpenuhi** — `042-U13` dan regresi |

Putusan: **✅ selesai** — acceptance 1–13 terpenuhi; uji layar satu siklus dengan `BE-IGD-061`, pada hasil build. Tanpa
UAT.
