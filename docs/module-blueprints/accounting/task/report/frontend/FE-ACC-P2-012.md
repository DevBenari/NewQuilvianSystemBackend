# Laporan Perubahan Frontend — `FE-ACC-P2-012`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-ACC-P2-012` |
| Judul | Rincian kejadian: coba ulang dan abaikan |
| Roadmap | [`roadmap/frontend-roadmap-phase2.md`](../../../roadmap/frontend-roadmap-phase2.md), kartu `FE-ACC-P2-012` (revisi 5, `APPROVED`) |
| Trace | `FR-P2-006`, `010`, `011`, `016`, `017`; `ACC-DEC-046`, `049`, `078`, `080`, `092`; `03-frontend-architecture.md` bagian 11.2 |
| Kontrak | `BE-ACC-P2-024` (`GET /{id}`), `BE-ACC-P2-025` (`POST /{id}/retry`, `PATCH /{id}/ignore`) |
| Dependency | `BE-ACC-P2-024` ✅, `BE-ACC-P2-025` ✅, `FE-ACC-P2-011` ✅ |
| Branch / commit | `RizkiV2` — di-commit Rizki `70bb05446` (24 September 2026), di atas `c941012ac` |
| Tanggal | 24 September 2026 |
| Status | **🟡 SEBAGIAN — pengerjaan ulang 24 September 2026** atas permintaan Rizki (bagian 9): rincian diringkas dari 13 kotak + 3 kartu menjadi 7 kotak, dan Isi Pesan Asli tidak lagi tampil sebagai kode. eslint 3 berkas **0/0**. **Belum:** `npm run build` Rizki dan uji layar ulang 6 skenario (bagian 9.6). Versi sebelumnya ✅ — build berhasil dan uji layar 7/7 lulus, commit `70bb05446` |

## 1. Yang dibangun

Halaman **Rincian Kejadian** — layar anak Kotak Masuk, dibuka dengan klik dua kali pada baris.
Alamatnya memakai **slug**, bukan Id: `/corporate/accounting/accounting-events/evt-uji-001-e566bcc10b7c`
= nomor kejadian + akhiran acak dari `crypto.randomUUID()`; Id dipetakan di `sessionStorage` oleh
`registerPrivateRouteToken`, pola yang sama dengan Jurnal dan COA.

| Wilayah | Isi |
| --- | --- |
| Kepala | Nomor, status, tombol Kembali, **Coba Ulang**, **Abaikan** |
| Informasi | 15 baris: status, alasan tertahan (kalimat), jenis, modul, no transaksi asal, versi, tgl akuntansi, tgl dokumen, waktu kejadian, waktu diterima, nilai, mata uang, alasan diabaikan |
| Jurnal yang Dihasilkan | Nomor, status jurnal, periode, tombol **Buka Jurnal** (slug jurnal); "(belum ada)" bila kosong |
| Rincian Komponen | Tabel, hanya tampil bila ada komponen |
| Riwayat Percobaan | Tabel terbaru di atas; "Belum pernah dicoba." bila kosong |
| Isi Pesan Asli | Tersembunyi bawaan; tombol Lihat/Sembunyikan; JSON dirapikan |

## 2. Tombol dan hak akses (`ACC-DEC-080`, `ACC-DEC-092`)

| Tombol | Aktif bila | Dimatikan dengan keterangan (`title`) bila |
| --- | --- | --- |
| Coba Ulang | `AccountingEvent : Retry` **dan** status Gagal/Tertahan | Tidak berhak, atau status lain |
| Abaikan | `AccountingEvent : Ignore` **dan** status Gagal | Tidak berhak, atau status lain — untuk Tertahan keterangannya menyuruh melengkapi aturan posting lalu Coba Ulang |

Abaikan memakai `ConfirmModal` bawaan `BaseDetailView` dengan `requireReason` — kalimat konfirmasi
menyebut tindakan **tidak dapat dibatalkan**. Backend tetap penegak akhirnya (`403`/`409`).

## 3. `UI GATE`

| Elemen | Keputusan |
| --- | --- |
| Kerangka rincian, baris informasi, toast, modal konfirmasi beralasan | `REUSE` `BaseDetailView` + `ConfirmModal` bawaannya (pola rincian Jurnal) |
| Tombol | `REUSE` `BaseButton` |
| Status | `REUSE` `StatusBadge` |
| Kartu dan tabel | `REUSE` kelas `journal-detail-view.module.css` — nol CSS baru |

`UI GATE: PASS`.

## 4. Pemetaan acceptance

| # | Acceptance | Bukti | Status |
| ---: | --- | --- | :---: |
| 1 | Kepala: nomor, status, alasan tahan dalam kalimat, nomor jurnal dapat diklik | `detailRows` + kartu Jurnal + `openJournal`. Diuji skenario 1–3 | ✅ |
| 2 | Riwayat percobaan tampil | `renderAttempts`. Diuji skenario 4 | ✅ |
| 3 | Abaikan hanya Gagal, alasan wajib | `ACCOUNTING_EVENT_IGNORABLE_STATUSES = ["3"]`, `requireReason: true`. Diuji: Abaikan mati untuk Tertahan (skenario 3); jalur Gagal lewat source (bagian 8.2) | ✅ |
| 4 | Coba Ulang aktif untuk Gagal dan Tertahan | `ACCOUNTING_EVENT_RETRYABLE_STATUSES = ["2", "3"]`. Diuji untuk Tertahan (skenario 3–4) dan mati untuk Terjurnal (skenario 5); Gagal lewat source (bagian 8.2) | ✅ |
| 5 | Galat server tampil apa adanya, status tidak berubah | Toast "Tindakan Ditolak" dengan pesan backend; rincian dimuat ulang hanya sesudah berhasil | ✅ source |

`FE-ACC-P2-011` acceptance (5) — klik baris membuka rincian — kini punya halaman tujuan.

## 5. Berkas

| Berkas | Status |
| --- | --- |
| `src/app/corporate/accounting/accounting-events/[slug]/page.jsx` | Baru |
| `src/components/view/corporate/accounting/accounting-event/detail/accounting-event-detail-view.jsx` | Baru |
| `src/lib/hooks/corporate/accounting/accounting-event/use-accounting-event-detail.jsx` | Baru |
| `src/lib/state/slice/corporate/accounting/accounting-event-slice.jsx` | Diperbarui — thunk `retryAccountingEvent`, `ignoreAccountingEvent` |
| `src/lib/constants/corporate/accounting/accounting-event/accounting-event-constants.jsx` | Diperbarui — bidang rincian, deskriptor tindakan |

## 6. Validasi

| Pemeriksaan | Hasil |
| --- | --- |
| `npx eslint` berkas di atas | **0 error, 0 warning** |
| `npm run build` | **Berhasil** — Rizki, 24 September 2026: tabel rute tercetak lalu `postbuild` (`prepare-standalone`) selesai "Standalone runtime siap dijalankan" — npm hanya menjalankan `postbuild` bila `next build` keluar dengan kode 0. Jumlah halaman tidak terlihat di tangkapan layar |
| `MANUAL TEST` | **PASS 7 dari 7** — Rizki, 24 September 2026, bagian 8 |
| `AUTOMATED TEST` | `SKIPPED (opsional)` |

## 7. Skenario uji layar

Prasyarat: backend dengan `024` dan `025` sudah di-build dan berjalan; kejadian uji dari `021`.

| # | Langkah | Diharapkan |
| ---: | --- | --- |
| 1 | Kotak Masuk → klik dua kali `EVT-UJI-002` | Rincian tampil, URL berupa slug `evt-uji-002-…`, kartu Jurnal berisi `JU/2026/09/00005` |
| 2 | Tekan **Buka Jurnal** | Rincian jurnal tersebut terbuka |
| 3 | Kembali, buka kejadian **Tertahan** `EVT-UJI-001`; catat kode jenisnya | Alasan "Jenis kejadian belum terdaftar"; Coba Ulang aktif; Abaikan mati dengan keterangan saat diarahkan kursor |
| 4 | Daftarkan jenis berkode yang dicatat di langkah 3 + aturan posting-nya, lalu tekan **Coba Ulang** | Toast berhasil, status Terjurnal, riwayat percobaan bertambah satu baris |
| 5 | Tekan Coba Ulang pada kejadian Terjurnal | Tombol mati — status bukan Gagal/Tertahan |
| 6 | Tekan **Lihat** pada Isi Pesan Asli | JSON pesan tampil rapi |
| 7 | Salin URL rincian ke jendela privat | "Tautan kejadian tidak valid…" — slug hanya berlaku di sesi peramban yang membuatnya |

## 8. Hasil uji layar — Rizki, 24 September 2026

### 8.1 Yang diuji

| # | Skenario | Hasil |
| ---: | --- | :---: |
| 1 | Klik dua kali `EVT-UJI-002` → rincian tampil, alamat berupa slug, kartu Jurnal `JU/2026/09/00005` | ✅ |
| 2 | **Buka Jurnal** → rincian jurnal terbuka | ✅ |
| 3 | `EVT-UJI-001` Tertahan → alasan "Jenis kejadian belum terdaftar"; Coba Ulang aktif; Abaikan mati dengan keterangan saat diarahkan kursor | ✅ |
| 4 | Jenis berkode sama + aturan posting didaftarkan, lalu **Coba Ulang** → toast berhasil, Tertahan → Terjurnal, riwayat percobaan bertambah | ✅ |
| 5 | Coba Ulang pada kejadian Terjurnal → tombol mati | ✅ |
| 6 | **Lihat** Isi Pesan Asli → JSON tampil rapi | ✅ |
| 7 | Alamat rincian dibuka di jendela privat → "Tautan kejadian tidak valid…" | ✅ |

`MANUAL TEST: PASS` 7 dari 7. Penolakan backend untuk Abaikan (`409` pada Tertahan dan Terjurnal)
diuji lewat Swagger — lihat [laporan `BE-ACC-P2-025`](../backend/BE-ACC-P2-025.md) bagian 6.1.
UAT belum dijalankan — diserahkan ke tim UAT.

### 8.2 Yang dibuktikan lewat source — keputusan Rizki, 24 September 2026

Belum ada kode yang menghasilkan kejadian `Gagal` sebelum `BE-ACC-P2-023`, sehingga tampilan
tombol untuk status itu tidak dapat dilihat di layar. Rizki menerima pembacaan source:

| Perilaku | Bukti source |
| --- | --- |
| Abaikan aktif untuk Gagal, dengan alasan wajib | `ACCOUNTING_EVENT_IGNORABLE_STATUSES = ["3"]` (`Gagal = 3`); `ConfirmModal` dengan `requireReason: true` — deskriptor yang sama yang terbukti mematikan Abaikan untuk Tertahan pada skenario 3 |
| Coba Ulang aktif untuk Gagal | `ACCOUNTING_EVENT_RETRYABLE_STATUSES = ["2", "3"]` — daftar yang sama yang terbukti mengaktifkan Tertahan (`2`) pada skenario 3 |

Diuji ulang lewat layar begitu `BE-ACC-P2-023` menghasilkan kejadian Gagal sungguhan.

**Perubahan data uji.** `EVT-UJI-001` kini Terjurnal; kotak masuk tidak lagi punya kejadian Tertahan.

### 8.3 Pembaruan — tombol untuk kejadian Gagal diuji, 24 September 2026

Sesudah `BE-ACC-P2-023` menghasilkan kejadian Gagal sungguhan, Rizki menguji di layar Rincian:
**Abaikan** pada `EVT-UJI-023A` (tanpa alasan ditolak, dengan alasan → Diabaikan) dan **Coba Ulang** pada
`EVT-UJI-023B` (tombol aktif; Terjurnal sesudah periode 2030 dibangkitkan). Bagian 8.2 kini diperkuat bukti
layar. Rincian di [laporan `BE-ACC-P2-023`](../backend/BE-ACC-P2-023.md) bagian 8.

---

## 9. Pengerjaan ulang — 24 September 2026, permintaan Rizki

### 9.1 Masalah

| Keluhan | Keadaan sebelumnya |
| --- | --- |
| Isi Pesan Asli tampil sebagai kode | Pesan ditampilkan sebagai JSON mentah (`{ "EventNumber": ... }`), tidak terbaca oleh petugas akuntansi yang tidak mengerti kode |
| Terlalu banyak kartu | Informasi Utama berisi 13 kotak, ditambah bagian Informasi Tambahan (Status) serta kartu Jurnal yang Dihasilkan dan Rincian Komponen yang terpisah |
| **Temuan saat memeriksa** | Baris "No Transaksi Asal" **tidak pernah tampil**: `BaseDetailCard` menyembunyikan otomatis setiap baris ber-key berakhiran `id` (`sourceTransactionId`) lewat `isIdLikeText` |

### 9.2 Yang berubah

| Tempat | Sesudah |
| --- | --- |
| Informasi Utama | **7 kotak** (4 baris pada layar lebar): **Status** (lencana + alasan tertahan atau alasan diabaikan dalam satu kotak), **Jurnal** (nomor, status, periode, tombol Buka Jurnal; menggantikan kartu terpisah), **Jenis Kejadian** (nama + kode), **Nilai** (+ rincian komponen bila lebih dari satu komponen atau bukan `TOTAL`; menggantikan kartu Rincian Komponen), **Tanggal Akuntansi** (+ tanggal dokumen hanya bila berbeda), **Sumber** (modul · nomor transaksi · versi bila bukan 1), **Waktu** (terjadi · diterima) |
| Dihapus | Kotak Nomor Kejadian (sudah menjadi judul halaman), Kode Jenis, Mata Uang (selalu Rupiah, `ACC-DEC-020`), bagian Informasi Tambahan, kartu Jurnal yang Dihasilkan, kartu Rincian Komponen |
| Isi Pesan Asli | Tetap tersembunyi bawaan dengan tombol Lihat/Sembunyikan. Isinya kini **tabel dua kolom** "Keterangan — Isi yang dikirim Finance" berlabel Indonesia: tanggal dan waktu diformat, nilai sebagai Rupiah, mata uang "Rupiah (IDR)", badan hukum sebagai **nama** (bukan kode sistem), tiap komponen satu baris ("Komponen Total — Rp 1.000.000"). Bidang kosong (mis. Saldo Subledger yang belum dipakai) tidak ditampilkan. Dua nomor penelusuran teknis diberi label "(untuk tim IT)" di baris terakhir |
| Pengaman | Bila isi pesan tersimpan bukan JSON yang sah, teksnya tampil apa adanya sebagai satu baris; pesan asli tidak pernah hilang dari layar |

Total bagian pada halaman: **3** (Informasi Utama, Riwayat Percobaan, Isi Pesan Asli), sebelumnya 5–6.

**FR-P2-006** ("isi pesan asli disimpan utuh dan dapat dilihat kembali") tetap terpenuhi: seluruh bidang
pesan yang tidak kosong tampil, dibaca langsung dari `RawPayload`, bukan dari kolom hasil olahan.

**Delta dokumen desain, untuk Rizki.** `03-frontend-architecture.md` bagian 11.2 menulis Isi pesan asli
sebagai "teks mentah, tersembunyi secara bawaan". Bentuknya kini tabel terbaca atas permintaan owner;
dokumen itu belum diubah karena di luar wewenang skill build.

### 9.3 `UI GATE`

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
| --- | --- | --- | --- | --- |
| Informasi Utama ringkas | `BaseDetailView` → `BaseDetailCard` | Prop `detailRows` yang sama; baris kosong disaring bawaan; `group: "main"` menahan Status di Informasi Utama | REUSE | — |
| Status dengan lencana | `StatusBadge` di dalam `row.render` | `BaseDetailCard` mendukung `row.render` (baris 504) | COMPOSE | Pilihan A di bawah |
| Jurnal dengan tombol | `BaseButton size="sm"` di dalam `row.render` | Sama | COMPOSE | Pilihan A di bawah |
| Tabel pesan asli | `styles.table` + `tableWrapper` dari `journal-detail-view.module.css` | Kelas yang sama dengan tabel Riwayat Percobaan | REUSE | — |
| Nama badan hukum | `useAccountingLegalEntity` (resource select `legalEntities`) | Dipakai Kotak Masuk, Periode, COA | REUSE | — |

```
UI GATE: 5 elemen — REUSE 3, EXTEND 0, COMPOSE 2, WRAP 0, NEW 0
```

**Keputusan: Status dan Jurnal sebagai kotak berisi lencana atau tombol.**

- **A. Rangkai `StatusBadge`/`BaseButton` di dalam `row.render` `BaseDetailCard` — dijalankan (rekomendasi).**
  Tanpa perubahan base component; kotaknya ikut tampilan dan responsif `BaseDetailCard`. Satu kotak memuat
  lencana plus teks, atau nomor plus tombol kecil.
- **B. Pertahankan kartu Jurnal terpisah di bawah Informasi Utama.** Paling mirip versi lama, tetapi
  menambah satu kartu, bertentangan dengan permintaan "tidak banyak baris card".

Nol CSS baru; nol perubahan `globals.css` dan komponen global. Kedua tabel kini membawa
`data-flat-table="true"` (kontrak tipografi tabel data di `globals.css`, diwajibkan checklist). Layar
rincian Jurnal yang menjadi rujukan belum memakainya; selisih lintas layar itu tidak disentuh.

### 9.4 Berkas

| Berkas | Perubahan |
| --- | --- |
| `src/components/view/corporate/accounting/accounting-event/detail/accounting-event-detail-view.jsx` | Baris ringkas dengan `render` untuk Status dan Jurnal; kartu Jurnal dan Komponen dihapus; Isi Pesan Asli menjadi tabel; `data-flat-table` |
| `src/lib/hooks/corporate/accounting/accounting-event/use-accounting-event-detail.jsx` | `summaryRows` (7 baris) menggantikan `detailRows` (15 bidang); `payloadRows` menggantikan teks JSON; ringkasan komponen; nama badan hukum lewat `useAccountingLegalEntity` |
| `src/lib/constants/corporate/accounting/accounting-event/accounting-event-constants.jsx` | `ACCOUNTING_EVENT_DETAIL_FIELDS` diganti `ACCOUNTING_EVENT_PAYLOAD_FIELDS` (label Indonesia bidang pesan) + `ACCOUNTING_EVENT_CURRENCY_LABELS` |

Nol baris komentar ditambahkan. Konstanta yang dihapus hanya dipakai hook ini (diperiksa dengan grep).

### 9.5 Validasi

| Pemeriksaan | Hasil |
| --- | --- |
| `npx eslint` ketiga berkas | **0 error, 0 warning** (exit 0), dijalankan ulang sesudah `data-flat-table` |
| Grep anti-regresi (`<button`, `fw-`/`fs-`, warna literal, `style={{`, `<pre`) | Kosong. `<table>`: 2, keduanya ber-`data-flat-table="true"` |
| `npm run build` | **Belum** — Rizki |
| `MANUAL TEST` | `REQUIRED` — bagian 9.6 |
| `AUTOMATED TEST` | `SKIPPED (opsional)` |

### 9.6 Skenario uji layar ulang

| # | Langkah | Diharapkan |
| ---: | --- | --- |
| 1 | Kotak Masuk → klik dua kali `EVT-UJI-002` | Informasi Utama **7 kotak**: Status lencana Terjurnal; Jurnal `JU/2026/09/00005` + status + periode + tombol Buka Jurnal; Jenis "Pembayaran Pasien (PATIENT_PAYMENT)"; Nilai "Rp 1.000.000" tanpa rincian (komponennya hanya TOTAL); Tanggal Akuntansi "24 September 2026"; Sumber "Finance · transaksi AR-UJI-0002"; Waktu "Terjadi … · diterima …". Tidak ada bagian Informasi Tambahan, kartu Jurnal, atau kartu Komponen |
| 2 | Tekan **Buka Jurnal** di kotak Jurnal | Rincian jurnal terbuka |
| 3 | Isi Pesan Asli → **Lihat** | Tabel Keterangan–Isi: Nomor Kejadian, Kode Jenis Kejadian, Modul Pengirim, Nomor Transaksi Asal, Versi Transaksi, Waktu Kejadian, Tanggal Akuntansi, Nilai, Mata Uang "Rupiah (IDR)", Badan Hukum **nama PT**, Komponen Total, dua nomor "(untuk tim IT)". **Tanpa** kurung kurawal, tanda kutip, atau baris Saldo Subledger |
| 4 | Buka `EVT-UJI-023A` (tab Semua) | Status "Diabaikan — Alasan diabaikan: Testing ignore event"; Jurnal "Belum ada jurnal"; **Riwayat Percobaan berisi 4 baris**. Tangkapan layar bagian ini sekaligus menutup bukti `BE-ACC-P2-023` acceptance (1)–(2) |
| 5 | Tombol Coba Ulang dan Abaikan pada `EVT-UJI-002` | Tetap mati, dengan keterangan saat diarahkan kursor (status Terjurnal) |
| 6 | Perkecil jendela peramban | Kotak Informasi Utama tersusun satu kolom; tabel pesan dapat digulir mendatar |
