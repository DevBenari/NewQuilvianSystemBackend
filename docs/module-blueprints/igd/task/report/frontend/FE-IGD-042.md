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
| Status | ✅ **SELESAI — 2 Oktober 2026.** Build pemilik dan uji layar U1–U8 **8 dari 8** pada bukti mentah (tangkapan layar dan teks modal). Layar dilayani `next dev`, bukan hasil build. Tanpa UAT. *Sebelumnya:* 🟡 **SEBAGIAN — Implementation Complete.** Enam berkas (lima source diubah, satu util baru); `eslint` 0 error; unit test tidak dipakai atas perintah pemilik 1 Oktober 2026. **Belum:** `npm run build` dan uji layar U1–U8 (milik pemilik, satu siklus dengan uji `BE-IGD-061`) |

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
