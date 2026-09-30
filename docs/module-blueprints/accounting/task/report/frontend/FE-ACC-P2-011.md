# Laporan Perubahan Frontend — `FE-ACC-P2-011`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-ACC-P2-011` |
| Judul | Layar Kotak Masuk Kejadian beserta penanda angka menu |
| Slice | `P2-6` — Wave B |
| Roadmap | [`roadmap/frontend-roadmap-phase2.md`](../../../roadmap/frontend-roadmap-phase2.md), kartu `FE-ACC-P2-011` (revisi 5, `APPROVED`) |
| Trace | `FR-P2-006`, `FR-P2-015`; `ACC-DEC-057`; `03-frontend-architecture.md` bagian 9 butir 1, bagian 10 butir 13, bagian 11.1 |
| Kontrak | `BE-ACC-P2-024` — `GET /accounting-events`, `GET /accounting-events/summary` (isi DTO pada [laporan `BE-ACC-P2-024`](../backend/BE-ACC-P2-024.md)) |
| Dependency | `BE-ACC-P2-024` ✅ |
| Branch / commit | `RizkiV2` — di-commit Rizki `70bb05446` (24 September 2026), di atas `c941012ac` |
| Model | Claude Opus 5.5 |
| Tanggal | 24 September 2026 |
| Status | **✅ SELESAI** — 24 September 2026. Uji layar Rizki lulus **8 dari 8** skenario (bagian 8); `npm run build` Rizki berhasil; eslint 0/0; di-commit `70bb05446`. Angka di butir menu **ditunda atas keputusan Rizki** (bagian 3) — dikecualikan dari acceptance (3), `ACC-DEC-057` tetap terbuka. UAT belum dijalankan — diserahkan ke tim UAT. Riwayat: 🟡 pada hari yang sama |

## 1. Yang dibangun

Layar **Kotak Masuk Kejadian** di `/corporate/accounting/accounting-events`, butir menu tingkat 2
di bawah Akuntansi (sesudah Jurnal).

| Wilayah | Isi | Sumber data |
| --- | --- | --- |
| Pemilih badan hukum | Dipakai ulang dari layar Akuntansi lain | `useAccountingLegalEntity` |
| Tab cepat | Semua · Tertahan *n* · Gagal *n* · Terjurnal *n* — angka 0 tidak ditampilkan | `GET /accounting-events/summary` |
| Penyaring | Jenis kejadian, periode, jumlah baris, cari nomor kejadian | `GET /event-types/options`, `GET /periods` |
| Tabel | No, No Kejadian, Jenis, Tgl Akuntansi, Nilai, Status (+ alasan tahan dalam kalimat), Jurnal | `GET /accounting-events` |
| Gagal dimuat | Pesan galat + tombol **Coba Lagi** | — |
| Kosong | Tiga keadaan: badan hukum belum dipilih / penyaring tanpa hasil / "Belum ada kejadian keuangan yang diterima." | — |

## 2. Keputusan base component (`UI GATE`)

| Elemen | Keputusan | Bukti |
| --- | --- | --- |
| Hero | `REUSE` `Hero` | Layar Jurnal |
| Pemilih badan hukum | `REUSE` `AccountingLegalEntitySelect` | Layar Jurnal |
| Tab cepat | `REUSE` prop `tabs`/`activeTab`/`onTabChange` pada `DataFilter` | `inpatient-monitoring-view`, `administrator-region-view` |
| Penyaring | `REUSE` `FilterSelect` di dalam `DataFilter` | Layar Jurnal |
| Tabel + paginasi | `REUSE` `DataTable` + `RegionPagination` | Layar Jurnal |
| Status | `REUSE` `StatusBadge` — lima nada yang ada, nol nada baru | `JOURNAL_STATUS_TONE` |
| Tombol | `REUSE` `BaseButton` `secondary` | — |
| Style | `REUSE` `journal-view.module.css` (`contextCard`, `amount`, `typeCell`) — nol CSS baru, `globals.css` tidak disentuh | — |
| **Angka penanda di butir menu** | **Ditunda — keputusan Rizki 24 September 2026** (bagian 3) | Sidebar hanya mengenal penanda teks `isNew` |

`UI GATE: PASS` untuk seluruh elemen di layar. Angka penanda menu: **ditunda atas keputusan Rizki 24 September 2026** (bagian 3) — sidebar tidak diubah.

## 3. Keputusan yang dibutuhkan — angka penanda di menu (`ACC-DEC-057`)

Komponen sidebar (`left-sidebar-items-virtualized.jsx`) dipakai **seluruh aplikasi** dan hanya
mengenal penanda teks "New". Menampilkan angka Gagal di butir menu menuntut:

1. **Menunda angka di menu; angka Gagal/Tertahan tampil di tab layar Kotak Masuk** *(direkomendasikan)* —
   nol perubahan komponen bersama, nol permintaan tambahan di setiap halaman. Konsekuensinya petugas
   baru melihat angkanya setelah membuka layar. `ACC-DEC-057` belum terpenuhi sepenuhnya.
2. **`EXTEND` sidebar** — prop angka opsional pada butir menu, diisi dari `/summary` saat sidebar
   dimuat. Memenuhi `ACC-DEC-057` penuh, tetapi menyentuh komponen yang dipakai semua modul, dan
   sidebar akan memanggil endpoint Accounting untuk **setiap** pengguna — yang tidak berhak menerima
   `403` di latar belakang. Butuh penjaga hak akses di sidebar.

**Keputusan Rizki, 24 September 2026: pilihan 1 — tunda.** Angka Gagal dan Tertahan hanya tampil
di tab layar Kotak Masuk. Komponen sidebar bersama tidak disentuh. Bagian "di menu" pada acceptance
(3) dikecualikan dari task ini sebagai pengecualian yang disetujui owner; `ACC-DEC-057` belum terpenuhi
sepenuhnya dan tetap terbuka untuk dikerjakan kelak sebagai task tersendiri. Keputusan ini belum
dicatat di `00-interview-decisions.md` — di luar wewenang skill build.

## 4. Pemetaan acceptance

| # | Acceptance kartu | Bukti | Status |
| ---: | --- | --- | :---: |
| 1 | Daftar berhalaman dengan penyaring status, jenis, periode, badan hukum | `use-accounting-event-inbox.jsx` — `requestParams`; tab = status | ✅ source |
| 2 | Status `Tercatat` dilabeli dan dapat disaring | `ACCOUNTING_EVENT_STATUS_OPTIONS` nilai `6` | ✅ source |
| 3 | Angka tab dan menu dari `/summary`; 0 tidak ditampilkan | Tab ✅ diuji (skenario 2); menu **ditunda atas keputusan Rizki** (bagian 3) — dikecualikan | ✅ tab / dikecualikan menu |
| 4 | Keadaan kosong | `ACCOUNTING_EVENT_EMPTY_REASONS.empty` | ✅ source |
| 5 | Klik baris membuka rincian | `openDetail` — token rute privat ke `/accounting-events/{token}`; halaman dibangun `FE-ACC-P2-012` dan terbukti terbuka (`FE-ACC-P2-012` skenario 1) | ✅ diuji |
| 6 | `SourceTransactionId` dan nominal tidak masuk log peramban | Nol `console.*`; daftar memang tidak memuat `SourceTransactionId` | ✅ source |

## 5. Berkas

| Berkas | Status |
| --- | --- |
| `src/app/corporate/accounting/accounting-events/page.jsx` | Baru |
| `src/app/corporate/accounting/accounting-events/accounting-event-client.jsx` | Baru |
| `src/components/view/corporate/accounting/accounting-event/accounting-event-inbox-view.jsx` | Baru |
| `src/lib/hooks/corporate/accounting/accounting-event/use-accounting-event-inbox.jsx` | Baru |
| `src/lib/state/slice/corporate/accounting/accounting-event-slice.jsx` | Baru — factory + thunk `getAccountingEventSummary` |
| `src/lib/constants/corporate/accounting/accounting-event/accounting-event-constants.jsx` | Baru |
| `src/lib/state/store.jsx` | Diperbarui — `accountingEvent` |
| `src/utils/menu-sidebar/menu-items.jsx` | Diperbarui — butir "Kotak Masuk Kejadian", ikon `RiInboxLine` |

Pilihan periode diambil lewat `getAccountingPeriodList(...).unwrap()` ke state lokal hook, supaya
tidak menimpa daftar di layar Periode Akuntansi.

## 6. Validasi

| Pemeriksaan | Hasil |
| --- | --- |
| `npx eslint` delapan berkas | **0 error, 0 warning** |
| Warna / nilai visual literal baru | Nol — tidak ada CSS baru |
| `npm run build` | **Berhasil** — Rizki, 24 September 2026: tabel rute tercetak lalu `postbuild` (`prepare-standalone`) selesai "Standalone runtime siap dijalankan" — npm hanya menjalankan `postbuild` bila `next build` keluar dengan kode 0. Jumlah halaman tidak terlihat di tangkapan layar |
| `MANUAL TEST` | **PASS 8 dari 8** — Rizki, 24 September 2026, bagian 8 |
| `AUTOMATED TEST` | `SKIPPED (opsional)` — kebijakan test repository |

## 7. Skenario uji layar

Prasyarat: backend dengan `BE-ACC-P2-021` dan `024` berjalan; beberapa kejadian dari uji Swagger
`021` sudah tersimpan.

| # | Langkah | Diharapkan |
| ---: | --- | --- |
| 1 | Buka menu Akuntansi › Kotak Masuk Kejadian | Daftar tampil, kejadian uji `EVT-UJI-*` terlihat |
| 2 | Lihat tab | Angka pada Tertahan/Terjurnal sesuai isi; tab bernilai 0 tanpa angka |
| 3 | Klik tab Tertahan | Hanya kejadian Tertahan, kolom Status menyebut alasannya (mis. "Jenis kejadian belum terdaftar") |
| 4 | Pilih jenis "Pembayaran Pasien" | Hanya `EVT-UJI-002` |
| 5 | Pilih periode September 2026 | Hanya kejadian bertanggal akuntansi September |
| 6 | Cari `EVT-UJI-002` | Satu baris, kolom Jurnal `JU/2026/09/00005` |
| 7 | Tekan Reset | Kembali ke Semua, penyaring kosong |
| 8 | Matikan backend, tekan Segarkan | Pesan galat + tombol Coba Lagi |

## 8. Hasil uji layar — Rizki, 24 September 2026

Catatan: sebelum uji, dev server menjawab `404` untuk seluruh sub-rute `/corporate/accounting/*` (juga
rute lama seperti Jurnal) dan `/finance/master-data/*`. Penyebabnya cache dev server Turbopack, bukan
source; hilang setelah `.next\dev` dihapus dan `npm run dev` dijalankan ulang.

| # | Skenario | Hasil |
| ---: | --- | :---: |
| 1 | Menu Kotak Masuk Kejadian membuka daftar; kejadian `EVT-UJI-*` tampil | ✅ |
| 2 | Angka tab: Tertahan 1, Terjurnal 1, Gagal tanpa angka | ✅ |
| 3 | Tab Tertahan → hanya kejadian Tertahan, alasan "Jenis kejadian belum terdaftar" | ✅ |
| 4 | Penyaring jenis "Pembayaran Pasien" → hanya `EVT-UJI-002` | ✅ (laporan kedua) |
| 5 | Penyaring periode September 2026 | ✅ |
| 6 | Cari `EVT-UJI-002` → satu baris, Jurnal `JU/2026/09/00005` | ✅ (laporan kedua) |
| 7 | Tab Semua tanpa penyaring → dua kejadian, kolom Jurnal `JU/2026/09/00005` | ✅ |
| 8 | Backend dimatikan, Segarkan → pesan galat + Coba Lagi; normal kembali sesudah backend menyala | ✅ (laporan kedua) |

Skenario 4, 6, dan 8 dilaporkan Rizki pada uji kedua, 24 September 2026. `MANUAL TEST: PASS` 8 dari 8.
UAT belum dijalankan — diserahkan ke tim UAT.

Skenario 4 dan 6 di bagian 7 diselaraskan 24 September 2026 dengan data yang benar-benar ada: uji Swagger
`021` menomori kejadian berbeda dari rencana di laporannya — `EVT-UJI-002` yang Terjurnal (jenis
"Pembayaran Pasien"), `EVT-UJI-001` yang Tertahan.

**Temuan**: jenis "Pembayaran Pasien" kemungkinan `PATIENT_PAYMENT`, yang menurut `ACC-DEC-083` harus
dinonaktifkan lewat UI. Kodenya perlu dicek di rincian; penonaktifan dilakukan Rizki lewat layar Jenis
Kejadian, bukan SQL.
