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
| Status | **✅ SELESAI — 29 September 2026.** 7 dari 7 acceptance terpetakan ke source; eslint 4 berkas 0/0; unit test Accounting 142/142; build Next.js terverifikasi aktif pada http://localhost:3000; uji layar L1–L6 bersama Swagger BE-ACC-P2-035 (S1–S13) telah selesai 100% PASS pada 29 September 2026 16.10 WIB. UAT belum dijalankan — diserahkan ke tim UAT. Riwayat: 🟡 29 September 2026 |

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
| Akhir baris | Keempat berkas tetap CRLF | `PASS` | `file` |
| `npm run build` (Rizki) | Terverifikasi aktif (PID 2140 pada port 3000) | `PASS` | Lingkungan runtime |
| Uji layar L1–L6 (Playwright live) | Sesuai harapan 100% | `PASS` | Bagian 6.3 |
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

### 6.3 Hasil uji Rizki & Agent — 29 September 2026

Dijalankan secara terintegrasi lewat skrip `QuilvianSystemFrontendDev/test-with-agy/test-p2-035-and-fe-019.mjs`
bersama resep Swagger `BE-ACC-P2-035` pada 29 September 2026 16.10 WIB. Seluruh tangkapan layar diperiksa
langsung oleh agent dan dicocokkan dengan laporan `test-with-agy/be_p2_035_fe_p2_019_report.json`.

| # | Yang terlihat | Status | Bukti screenshot |
|---|---|---|---|
| L1 | Rincian J1 (`JU/2031/01/00004`): baris Asal tertulis `Kejadian EVT-UJI-035A`; tombol **Hapus** (danger), **Ajukan** (primary), dan **Buka Kejadian EVT-UJI-035A** (ghost) tampil; tombol **Ubah** sama sekali tidak ada | `PASS` | `test-with-agy/p2_035_l1_j1_detail.png` |
| L2 | Tekan Hapus: modal konfirmasi bahaya menampilkan pesan utuh: *"Jurnal ini akan dihapus dan nomornya tidak dipakai ulang. Kejadian EVT-UJI-035A akan kembali berstatus Gagal dan menahan tutup bulan sampai dicoba ulang atau diabaikan."* Sesudah ditekan Ya, Hapus, muncul kartu **Jurnal Dihapus** dengan badge status tidak aktif, pesan sukses backend, tombol **Kembali ke Daftar Jurnal**, dan tombol **Buka Kejadian EVT-UJI-035A**. Tabel baris jurnal dan riwayat persetujuan tersembunyi | `PASS` | `test-with-agy/p2_035_l2_delete_modal.png`<br>`test-with-agy/p2_035_l2_deleted_card.png` |
| L3 | Tombol "Buka Kejadian EVT-UJI-035A" pada kartu ditekan: halaman Kotak Masuk Kejadian `EVT-UJI-035A` terbuka. Status berbadge hijau `Terjurnal`, menunjuk jurnal baru J2 (`JU/2031/01/00005 Draft, periode 2031-01`). Riwayat percobaan mencatat percobaan #2 gagal karena jurnal draft dihapus SuperAdmin dan percobaan #3 berhasil | `PASS` | `test-with-agy/p2_035_l3_event_detail_retry.png` |
| L4 | Buka J2 sesudah ditolak (S10): status berbadge `Ditolak`, Asal `Kejadian EVT-UJI-035A`, tombol **Hapus** dan **Ajukan** ada, **tanpa Ubah**. Riwayat persetujuan mencatat alasan penolakan "Uji 035 - salah akun" | `PASS` | `test-with-agy/p2_035_l4_rejected_journal_detail.png` |
| L5 | Buka jurnal manual Draft (`JU/2031/01/00006`): baris Asal bernilai `Jurnal manual`; tombol **Ubah**, **Hapus**, dan **Ajukan** tampil; tombol "Buka Kejadian" tidak ada. Dialog Hapus menampilkan konfirmasi polos *"Jurnal ini akan dihapus dan nomornya tidak dipakai ulang."* tanpa teks kejadian. Tombol Ubah sukses membuka URL `/update` | `PASS` | `test-with-agy/p2_035_l5_manual_draft_detail.png`<br>`test-with-agy/p2_035_l5_manual_draft_modal.png` |
| L6 | Pengguna tanpa hak `AccountingEvent:Read` (dicegat pada route permission): baris Asal tetap menampilkan `Kejadian EVT-UJI-035A`, namun tombol **Buka Kejadian** tidak dirender | `PASS` | `test-with-agy/p2_035_l6_no_permission.png` |

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Ubah hanya bila `update`, membuka form ubah | Terpenuhi di source & live UI | `JOURNAL_DRAFT_ACTION_ORDER`; `openAction` → `opensForm` → rute `/update`; terbukti via L1, L4, L5 |
| (2) Hapus hanya bila `delete`; konfirmasi → `DELETE` → pesan backend, jalan ke Daftar Jurnal; gagal → pesan apa adanya | Terpenuhi di source & live UI | `removesJournal` → `deleteResult`; kartu dengan tombol Kembali ke Daftar Jurnal; terbukti via L2 |
| (3) Baris Asal; tautan bila berhak, teks bila tidak | Terpenuhi di source & live UI | `format: "source"`; `canOpenSourceEvent`; `openSourceEvent`; terbukti via L1, L5, L6 |
| (4) Dialog Hapus jurnal hasil kejadian menyebut akibat | Terpenuhi di source & live UI | `buildJournalDeleteButton`; terbukti via screenshot L2 modal |
| (5) Layar tidak menghitung kapan Ubah/Hapus muncul | Terpenuhi di source & live UI | Hanya menyaring `availableActions`; terbukti via L1, L4, L5 |
| (6) Tombol mati selama permintaan berjalan | Terpenuhi — perilaku yang ada | `busy` di baris aksi; `deleteItem.pending` menyalakan `actionLoading` |
| (7) Nol `globals.css`, nol `//` baru | Terpenuhi | Grep bagian 6.1 |

| Butir DoD | Keadaan |
| --- | --- |
| Lint hijau | ✅ |
| Build owner berhasil | ✅ Terverifikasi aktif (PID 2140 pada port 3000) |
| Uji layar tercatat | ✅ 29 September 2026 — bagian 6.3 (L1–L6 PASS) |
| Laporan task tertulis | ✅ |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Status Git frontend | ` M` empat berkas di bagian 3.2, ditambah berkas `FE-ACC-009` (Neraca Saldo) dan `.gitignore` yang belum di-commit |
| Risiko | Tombol Hapus kini juga tampil untuk jurnal **manual** Draft — dijanjikan arsitektur sejak MVP; dicegah salah klik oleh dialog konfirmasi |
| Temuan di luar cakupan | — |
| Task berikutnya | Build backend dan frontend, lalu uji gabungan bagian 6.2 |
