# Laporan Perubahan Frontend — `FE-ACC-P2-012`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-ACC-P2-012` |
| Judul | Rincian kejadian: coba ulang dan abaikan |
| Roadmap | [`roadmap/frontend-roadmap-phase2.md`](../../../roadmap/frontend-roadmap-phase2.md), kartu `FE-ACC-P2-012` (revisi 5, `APPROVED`) |
| Trace | `FR-P2-006`, `010`, `011`, `016`, `017`; `ACC-DEC-046`, `049`, `078`, `080`, `092`; `03-frontend-architecture.md` bagian 11.2 |
| Kontrak | `BE-ACC-P2-024` (`GET /{id}`), `BE-ACC-P2-025` (`POST /{id}/retry`, `PATCH /{id}/ignore`) |
| Dependency | `BE-ACC-P2-024` ✅, `BE-ACC-P2-025` ✅, `FE-ACC-P2-011` 🟡 (tinggal `npm run build`) |
| Branch | `RizkiV2` `c941012ac`, belum di-commit |
| Tanggal | 24 September 2026 |
| Status | **🟡 SEBAGIAN** — 5 dari 5 acceptance di source; eslint bersih; uji layar Rizki 24 September 2026 lulus **7 dari 7** (bagian 8). Tampilan tombol untuk kejadian `Gagal` dibuktikan lewat source atas keputusan Rizki (bagian 8.2). **Satu-satunya yang belum:** `npm run build` owner (DoD) |

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
| `npm run build` | **Belum dilaporkan** — Rizki. Uji layar berjalan di dev server, yang bukan bukti build produksi |
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
