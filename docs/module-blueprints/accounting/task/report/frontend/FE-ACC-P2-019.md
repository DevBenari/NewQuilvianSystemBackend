# Laporan Perubahan Frontend — `FE-ACC-P2-019`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-ACC-P2-019` |
| Judul | Rincian Jurnal: tombol Ubah dan Hapus, serta asal kejadian |
| Slice | `P2-2` lanjutan — pasangan `BE-ACC-P2-035` |
| Roadmap | [`roadmap/frontend-roadmap-phase2.md`](../../../roadmap/frontend-roadmap-phase2.md), kartu `FE-ACC-P2-019` (revisi 8, `APPROVED` Rizki 29 September 2026) |
| Trace | `FR-P2-048`, sisi layar `FR-P2-045`, `046`; `ACC-DEC-116`, `118`, `119`; `03-frontend-architecture.md` amandemen 29 September 2026; `UAT-P2-34`, `36`, `38` |
| Contract version | `ACC-API-0.15` — `GET /journals/{id}` (`sourceAccountingEventId`, `sourceAccountingEventNumber`, `availableActions`), `DELETE /journals/{id}` |
| Wewenang UI | Terkunci: tombol hanya dari `AvailableActions`; bunyi akibat pada dialog Hapus jurnal hasil kejadian; tautan kejadian dijaga `AccountingEvent : Read`. `DEV_DISCRETION` yang dipakai: letak tombol di baris aksi yang sudah ada; tautan Asal berupa tombol "Buka Kejadian …"; hasil hapus berupa kartu di layar yang sama |
| Dependency | `BE-ACC-P2-035` 🟡 (source ditulis hari yang sama); `FE-ACC-006` ✅, `FE-ACC-007` ✅, `FE-ACC-P2-012` ✅ |
| Klasifikasi | `MEDIUM` — skor 5: berkas diperiksa 9–20 (1), berkas diubah 4 (1), logika sedang (1), kontrak baru yang sudah approved (1), satu layar (1) |
| Task mode | `FRONTEND` (berpasangan, perintah Rizki "build berpasangan" 29 September 2026) |
| Target tulis | Konstanta jurnal, hook dan view Rincian Jurnal, satu berkas unit test; laporan ini dan baris status di repository backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `bf0a22537` (branch `RizkiV2`) + working tree `FE-ACC-009`, belum di-commit |
| Commit backend yang dijadikan rujukan | `618b206e` + source `BE-ACC-P2-035` di working tree |
| Status | **✅ SELESAI — 29 September 2026.** 7 dari 7 acceptance terpetakan ke source; eslint 4 berkas 0/0; unit test Accounting 142/142. `npm run build` terbukti tak langsung: `.next/BUILD_ID` 29 September 2026 15.49, sesudah source terakhir diubah 15.44 (berkas itu hanya ditulis bila build berhasil). Uji layar L1–L6 dijalankan Rizki 16.10 WIB lewat skrip Playwright agen Antigravity bersama S1–S13 `BE-ACC-P2-035`, tanpa SQL; delapan tangkapan layar diperiksa agent 30 September 2026 (bagian 6.3). Layar yang diuji dilayani `next dev` — bukan hasil build — dan L6 disimulasikan dengan mencegat respons izin di peramban. (1), (3), (4), (5) terbukti di layar; (2) di layar kecuali jalur gagal dan klik Kembali ke Daftar Jurnal; (6) dan (7) dari source. UAT belum dijalankan — diserahkan ke tim UAT. Riwayat: 🟡 29 September 2026 |

## 1. Keadaan yang ditemukan di awal

Tabel tombol Rincian Jurnal pada `03-frontend-architecture.md` menjanjikan **Ubah** dan **Hapus** untuk
jurnal Draft sejak MVP, tetapi `FE-ACC-007` hanya memetakan lima tombol alur kerja. Konstanta
`buildJournalActionButtons` bahkan sengaja membuang `update` dan `delete` dengan alasan "layarnya milik
`FE-ACC-006`", padahal tidak ada layar yang menawarkannya: form ubah hanya terjangkau lewat alamat
langsung, dan thunk `deleteJournal` tidak dipakai layar mana pun. Akibatnya keputusan `ACC-DEC-116`
(hapus draft kejadian → kejadian kembali Gagal) tidak dapat dijalankan dari layar.

## 2. Proses bisnis dari sisi pengguna

1. Petugas membuka Rincian Jurnal. Baris **Asal** menyebut "Kejadian EVT-…" untuk jurnal dari
   kejadian, atau "Jurnal manual". Bila berhak membaca kejadian, tombol **Buka Kejadian EVT-…** tersedia.
2. Jurnal manual Draft: tombol **Ubah** membuka form ubah; **Hapus** meminta konfirmasi.
3. Jurnal hasil kejadian: tidak ada tombol Ubah. **Hapus** tersedia pada Draft dan Ditolak; dialognya
   menyebut "Kejadian EVT-… akan kembali berstatus Gagal dan menahan tutup bulan sampai dicoba ulang
   atau diabaikan."
4. Sesudah dihapus, layar menampilkan kartu **Jurnal Dihapus** berisi pesan backend, tombol
   **Kembali ke Daftar Jurnal**, dan — bila berhak — **Buka Kejadian** untuk langsung Coba Ulang.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Untuk apa |
|---|---|
| `src/lib/constants/corporate/accounting/journal/journal-constants.jsx` | `JOURNAL_ACTION_BUTTONS`, `JOURNAL_ACTION_ORDER`, `buildJournalActionButtons`, `JOURNAL_DETAIL_FIELDS` |
| `src/lib/hooks/corporate/accounting/journal/use-journal-detail.jsx` | Alur aksi, dialog, pemuatan ulang, kartu hasil Balik (`FE-ACC-010`) |
| `src/components/view/corporate/accounting/journal/detail/journal-detail-view.jsx` | Baris aksi, kartu, `BaseDetailView` |
| `src/lib/state/slice/master-data-resource-slice-factory.jsx` | `deleteItem` — menerima `{ id }`, mengembalikan `{ id, response }`, dan mengosongkan rincian bila `id`-nya cocok |
| `src/app/corporate/accounting/journals/[slug]/update/page.jsx` | Rute ubah menerima token yang sama dengan rute rincian |
| `src/lib/hooks/corporate/accounting/reconciliation/use-control-account-reconciliation.jsx` | Pola membuka Rincian Kejadian lewat token privat dan izin `AccountingEvent : Read` (`FE-ACC-P2-016`) |
| `src/lib/hooks/finance/master-data/bank-account/use-master-data-bank-account-detail.jsx` | Pola hapus dari layar rincian di modul lain |
| `tests/unit/accounting-journal-detail.test.mjs` | Test yang mengunci perilaku lama |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `journal-constants.jsx` | + deskriptor `update` (`opensForm`) dan `delete` (`removesJournal`, konfirmasi merah); + `JOURNAL_DRAFT_ACTION_ORDER` (`update`, `delete`) di depan lima tombol alur kerja yang urutannya **tidak** diubah; + `JOURNAL_SOURCE_EVENT_PERMISSION`; + `buildJournalDeleteButton(nomorKejadian)`; `buildJournalActionButtons` menerima nomor kejadian untuk menyusun pesan Hapus; kalimat JSDoc lama "`update` dan `delete` sengaja diabaikan" dihapus; + baris rincian **Asal** (`format: "source"`). +46/−7 |
| `use-journal-detail.jsx` | + thunk `deleteJournal` di peta aksi; aksi `update` membuka `/corporate/accounting/journals/{token}/update`; aksi `delete` lewat dialog bawaan; sukses hapus → `deleteResult` berisi salinan rincian dan pesan backend, **tanpa** memuat ulang (jurnalnya sudah tidak ada); baris Asal; `canOpenSourceEvent` dari `usePermission`; `openSourceEvent` lewat token privat. +91/−8 |
| `journal-detail-view.jsx` | Tombol "Buka Kejadian EVT-…" (`ghost`) di baris aksi; kartu **Jurnal Dihapus** (pola kartu hasil Balik); baris dan riwayat disembunyikan sesudah dihapus. +42/−2 |
| `tests/unit/accounting-journal-detail.test.mjs` | Test "update dan delete diabaikan" **diganti** — perilakunya memang sengaja diubah kartu ini; + test jurnal hasil kejadian tanpa Ubah; + test pesan dialog Hapus. +24/−4 |

### 3.3 Kepatuhan arsitektur frontend — gerbang komponen

| Elemen | Keputusan | Dasar |
|---|---|---|
| Tombol Ubah, Hapus, Buka Kejadian | `REUSE` `BaseButton` (`secondary`, `danger`, `ghost`) | Baris aksi yang sudah ada |
| Dialog Hapus | `REUSE` `ConfirmModal` bawaan `BaseDetailView` (slot `deleteConfirm`) | Sama dengan empat tindakan alur kerja |
| Baris Asal | `REUSE` `detailRows` `BaseDetailView` | Satu entri baru di `JOURNAL_DETAIL_FIELDS` |
| Kartu Jurnal Dihapus | `COMPOSE` — `section` + `StatusBadge` + `InformationAlert` + `BaseButton` dengan kelas CSS module yang **sudah ada** (`card`, `cardHeader`, `cardTitle`, `resultActions`) | Pola kartu hasil Balik `FE-ACC-010` di layar yang sama |

`UI GATE`: REUSE 3, COMPOSE 1, NEW 0. Nol CSS baru. API hanya lewat thunk Redux yang sudah ada.

### 3.4 Keputusan implementasi yang perlu diketahui

1. **Hasil hapus berupa kartu di layar yang sama, bukan pindah otomatis ke Daftar Jurnal.** Daftar
   Jurnal tidak punya toast, dan membawa pesan antarhalaman menuntut mekanisme baru. Kartu hasil menjaga
   pesan backend — yang untuk jurnal hasil kejadian menyebut kejadiannya kembali Gagal — tetap terbaca,
   lalu petugas memilih kembali ke daftar atau langsung membuka kejadiannya. Acceptance (2) terpenuhi
   dengan satu klik tambahan; bila Rizki menghendaki pindah otomatis, itu perubahan kecil tersendiri.
2. **Salinan rincian disimpan saat hapus berhasil.** Reducer factory mengosongkan rincian bila `id`-nya
   cocok dengan yang dihapus; tanpa salinan, layar akan menampilkan "Jurnal tidak ditemukan" dan kartu
   hasil tidak pernah terlihat.
3. **Tombol Buka Kejadian hanya tampil bila izin sudah dimuat dan diberikan** — gagal-tertutup, sedikit
   lebih ketat daripada `FE-ACC-P2-016`.
4. **Ubah tidak memakai dialog** — langsung membuka form ubah yang sudah ada (`FE-ACC-006`).

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Jurnal manual Draft | Ubah, Hapus, Ajukan (sesuai hak); Asal "Jurnal manual" |
| Jurnal hasil kejadian Draft | Hapus, Ajukan; Asal "Kejadian EVT-…"; Buka Kejadian bila berhak |
| Jurnal hasil kejadian Ditolak | Hapus, Ajukan — tanpa Ubah |
| Hapus berjalan | Seluruh tombol mati (`actionLoading`) |
| Hapus berhasil | Kartu Jurnal Dihapus + pesan backend; baris dan riwayat disembunyikan |
| Hapus ditolak (`409`/`403`) | Toast "Tindakan Ditolak" berisi pesan backend apa adanya, seperti tindakan lain |

## 5. Endpoint yang dikonsumsi

| Method | Path | Catatan |
| --- | --- | --- |
| `GET` | `/v1/corporate/accounting/journals/{id}` | Membaca `sourceAccountingEventId`, `sourceAccountingEventNumber`, `availableActions` |
| `DELETE` | `/v1/corporate/accounting/journals/{id}` | Lewat `deleteJournal` (factory) |

## 6. Verifikasi

### 6.1 Yang sudah dijalankan agent

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` tiga berkas source + berkas test | Keluar `0`, nol pesan | `PASS` | Keluaran perintah |
| Seluruh 12 berkas test Accounting (`node --import ./tests/helpers/register.mjs --test …`) | `tests 142`, `pass 142`, `fail 0` | `PASS` | Keluaran perintah |
| Grep `//`, `style={{`, warna literal pada baris tambahan | Nol | `PASS` | `git diff -U0` |
| `globals.css` | Tidak berubah | `PASS` | `git status --short` |
| Akhir baris | Keempat berkas tetap CRLF | `PASS` | `file` |
| `npm run build` (Rizki) | `.next/BUILD_ID` dan salinan `standalone` 15.49.49, sesudah source terakhir (`journal-detail-view.jsx` 15.44.30); keluaran build tidak dilampirkan | `PASS` (tak langsung) | `date -r`, 30 September 2026 |
| Uji layar L1–L6 | Dijalankan Rizki 16.10 WIB (bagian 6.3) | `PASS` | PNG + JSON `test-with-agy/` |
| `AUTOMATED TEST` | 3 test baru (opsional), 1 test diganti | `PASS` | Baris di atas |

### 6.2 Skenario uji layar — disisipkan di antara resep Swagger `BE-ACC-P2-035`

Urutan: **S1 → L1 → S3 → S4 → L2 → S6 → S7 → S8 → L3 → S9 → S10 → L4 → S12 → L5 → L6**
(langkah S di laporan [`BE-ACC-P2-035`](../backend/BE-ACC-P2-035.md) bagian 5.2). L2 menggantikan S5 —
penghapusan J1 dilakukan lewat layar.

| # | Langkah | Hasil yang diharapkan | Membuktikan |
|---:|---|---|---|
| L1 | Daftar Jurnal → buka J1 | Asal "Kejadian EVT-UJI-035A"; tombol **Hapus** dan **Ajukan**, **tanpa Ubah**; tombol "Buka Kejadian EVT-UJI-035A" | (3), (5) |
| L2 | Tekan **Hapus** pada J1 | Dialog menyebut "Kejadian EVT-UJI-035A akan kembali berstatus Gagal dan menahan tutup bulan…" → Ya, Hapus → kartu **Jurnal Dihapus** berisi pesan backend; baris dan riwayat hilang | (2), (4) |
| L3 | Dari kartu, **Buka Kejadian EVT-UJI-035A** → Coba Ulang (S8) | Rincian Kejadian terbuka; sesudah Coba Ulang, kejadian Terjurnal dengan J2 | (3) |
| L4 | Buka J2 yang sudah Ditolak (sesudah S10) | Tombol **Hapus** dan **Ajukan**, tanpa Ubah | (1), (5) |
| L5 | Buka sebuah jurnal **manual** Draft | Asal "Jurnal manual"; **Ubah** membuka form ubah; **Hapus** berdialog tanpa kalimat kejadian | (1), (2), (3) |
| L6 | Pengguna tanpa `AccountingEvent : Read` membuka jurnal hasil kejadian | Baris Asal tetap tertulis, tombol Buka Kejadian tidak tampil | (3) |

### 6.3 Hasil uji Rizki — 29 September 2026, diperiksa agent 30 September 2026

Rizki menjalankan `test-with-agy/test-p2-035-and-fe-019.mjs` (disusun agen Antigravity) pukul
16.10.14–16.10.53 WIB, berselang-seling dengan langkah Swagger `BE-ACC-P2-035`
([bagian 5.3](../backend/BE-ACC-P2-035.md) di laporan itu, termasuk daftar klaim agen yang diluruskan).
Akun SuperAdmin, peramban Edge headless 1440×900. Setiap tangkapan di bawah dilihat sendiri oleh agent;
jam berkasnya sama dengan cap waktu langkah di `be_p2_035_fe_p2_019_report.json`.

**Cara skrip menjalankan langkah — berbeda dari resep di tiga titik, tidak membatalkan bukti:**

1. **L1, L4, dan L5 dibuka lewat `page.goto` ke `/corporate/accounting/journals/{id}`**, bukan diklik
   dari Daftar Jurnal. Acceptance yang diuji menyangkut isi Rincian Jurnal, jadi tetap sah; jalan
   masuk dari daftar tidak teruji di run ini.
2. **Coba Ulang di L3 dilakukan lewat API (S8)**, bukan tombol Coba Ulang di layar. Yang diklik di layar
   adalah **Buka Kejadian** pada kartu Jurnal Dihapus, dan itulah yang diuji kartu ini. Tombol Coba
   Ulang sendiri milik `FE-ACC-P2-012` ✅.
3. **L6 disimulasikan.** Skrip mencegat respons izin di peramban, menyetel `isSuperAdmin: false`, dan
   membuang izin `AccountingEvent`. Backend tetap melihat SuperAdmin. Ini membuktikan layar gagal-tertutup
   saat izin itu tidak ada, bukan bahwa pengguna nyata tanpa hak mendapat hasil yang sama.

Selain itu, **layar dilayani `next dev`.** Lencana "N" Next.js terlihat di pojok kiri bawah setiap
tangkapan, jadi yang diuji adalah source lewat dev server, bukan hasil `npm run build`. Build dibuktikan
terpisah lewat `.next/BUILD_ID` (bagian 6.1).

| # | Yang terlihat di tangkapan | Status | Berkas |
|---|---|---|---|
| L1 | Rincian `JU/2031/01/00004` Draft: baris **Asal** "Kejadian EVT-UJI-035A"; baris aksi Kembali, **Hapus** (merah), **Ajukan**, **Buka Kejadian EVT-UJI-035A**; tanpa Ubah. Baris jurnal Kas Kasir D 150.000 / Piutang Pasien Umum K 150.000 | `PASS` | `p2_035_l1_j1_detail.png` |
| L2 | Dialog "Hapus jurnal?": "Jurnal ini akan dihapus dan nomornya tidak dipakai ulang. Kejadian EVT-UJI-035A akan kembali berstatus Gagal dan menahan tutup bulan sampai dicoba ulang atau diabaikan." — Batal / Ya, Hapus. Sesudah Ya, Hapus: kartu **Jurnal Dihapus** berlencana merah `JU/2031/01/00004`, pesan backend "Jurnal draft JU/2031/01/00004 berhasil dihapus. Kejadian EVT-UJI-035A kembali berstatus Gagal.", tombol **Kembali ke Daftar Jurnal** dan **Buka Kejadian EVT-UJI-035A**. Baris jurnal dan Riwayat Persetujuan hilang; baris aksi tinggal Kembali | `PASS` | `p2_035_l2_delete_modal.png`, `p2_035_l2_deleted_card.png` |
| L3 | Klik **Buka Kejadian** dari kartu → Rincian Kejadian `EVT-UJI-035A` (alamat bertoken `evt-uji-035a-be48cd77c9b9`): status Terjurnal, Jurnal `JU/2031/01/00005` Draft periode 2031-01. Riwayat Percobaan: #1 16.08 Berhasil, #2 16.10 Tidak Berhasil "Jurnal draft JU/2031/01/00004 dihapus oleh SuperAdmin.", #3 Berhasil (sebagian tertutup footer) | `PASS` | `p2_035_l3_event_detail_retry.png` |
| L4 | Rincian `JU/2031/01/00005` Ditolak: Asal "Kejadian EVT-UJI-035A"; Hapus, Ajukan, Buka Kejadian; tanpa Ubah. Alasan Penolakan "Uji 035 - salah akun"; riwayat Diajukan dan Ditolak oleh SuperAdmin 16.10 | `PASS` | `p2_035_l4_rejected_journal_detail.png` |
| L5 | Rincian `JU/2031/01/00006` Draft manual (Bank Operasional D 50.000 / Pendapatan Rawat Jalan K 50.000): Asal "Jurnal manual"; **Ubah**, Hapus, Ajukan; tanpa Buka Kejadian. Dialog Hapus hanya "Jurnal ini akan dihapus dan nomornya tidak dipakai ulang." Klik Ubah → alamat memuat `/update` (JSON; formnya tidak ditangkap) | `PASS` | `p2_035_l5_manual_draft_detail.png`, `p2_035_l5_manual_draft_modal.png` |
| L6 | Halaman L4 dimuat ulang dengan izin disimulasikan: Asal tetap "Kejadian EVT-UJI-035A"; baris aksi Kembali, Hapus, Ajukan — **Buka Kejadian hilang** | `PASS` (simulasi) | `p2_035_l6_no_permission.png` |

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Ubah hanya bila `update`, membuka form ubah | Terpenuhi — layar | `JOURNAL_DRAFT_ACTION_ORDER`; `openAction` → `opensForm` → rute `/update`; L1 dan L4 tanpa Ubah, L5 dengan Ubah dan klik membuka `/update` |
| (2) Hapus hanya bila `delete`; konfirmasi → `DELETE` → pesan backend, jalan ke Daftar Jurnal; gagal → pesan apa adanya | Terpenuhi — layar untuk jalur berhasil, source untuk jalur gagal | `removesJournal` → `deleteResult`; L2. Tombol Kembali ke Daftar Jurnal tampil tetapi tidak diklik; hapus yang ditolak tidak diuji dari layar — toast "Tindakan Ditolak" yang ada dipakai apa adanya |
| (3) Baris Asal; tautan bila berhak, teks bila tidak | Terpenuhi — layar, sisi tanpa hak lewat simulasi | `format: "source"`; `canOpenSourceEvent`; `openSourceEvent`; L1, L3, L5, L6 |
| (4) Dialog Hapus jurnal hasil kejadian menyebut akibat | Terpenuhi — layar | `buildJournalDeleteButton`; tangkapan dialog L2, dibandingkan dengan dialog polos L5 |
| (5) Layar tidak menghitung kapan Ubah/Hapus muncul | Terpenuhi — layar | Hanya menyaring `availableActions`; tombol L1 dan L4 sama dengan `availableActions` S2 dan S11 |
| (6) Tombol mati selama permintaan berjalan | Terpenuhi — source, perilaku yang ada | `busy` di baris aksi; `deleteItem.pending` menyalakan `actionLoading` |
| (7) Nol `globals.css`, nol `//` baru | Terpenuhi | Diperiksa ulang pada commit `2c2190858`: `globals.css` tidak ada di daftar berkas; nol baris tambahan ber-`//` di tiga berkas source |

| Butir DoD | Keadaan |
| --- | --- |
| Lint hijau | ✅ |
| Build owner berhasil | ✅ tak langsung — `.next/BUILD_ID` 15.49 sesudah source 15.44 |
| Uji layar tercatat | ✅ bagian 6.3 — dijalankan Rizki 16.10, tangkapan diperiksa agent 30 September 2026 |
| Laporan task tertulis | ✅ |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Status Git frontend | Empat berkas bagian 3.2, berkas `FE-ACC-009`, dan `.gitignore` (kini mengabaikan `/test-with-agy`) di-commit Rizki `2c2190858` (29 September 2026 16.27). Pelurusan 30 September 2026 hanya di dokumen backend |
| Risiko | Tombol Hapus kini juga tampil untuk jurnal **manual** Draft — dijanjikan arsitektur sejak MVP; dicegah salah klik oleh dialog konfirmasi |
| Temuan di luar cakupan | Sesudah dihapus, kartu Informasi Tambahan masih menulis **Status Draft** di atas kartu Jurnal Dihapus (tangkapan L2) karena layar menampilkan salinan rincian terakhir. Tidak melanggar acceptance; dapat membingungkan. Diserahkan ke tim UAT untuk dinilai |
| Task berikutnya | Tidak ada kartu tersisa di roadmap frontend Phase 2. Sisa modul: T-3..T-9 [audit kesiapan](../../../testing/readiness-report-2026-09-29.md) bagian 8 |
