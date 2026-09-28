# Laporan Perubahan Frontend — `FE-ACC-P2-015`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-ACC-P2-015` |
| Judul | Kotak Masuk dan Rincian mengenali pesan saldo subledger |
| Slice | Wave D — pesan saldo subledger |
| Roadmap | [`roadmap/frontend-roadmap-phase2.md`](../../../roadmap/frontend-roadmap-phase2.md), kartu `FE-ACC-P2-015` (revisi 6, `APPROVED` Rizki 28 September 2026) |
| Trace | `ACC-DEC-087`, `093`; `ACC-STATE-0.4` status `Tercatat`; `03-frontend-architecture.md` bagian 11.1, 11.2. **Belum ada FR** — coverage gap FR saldo subledger |
| Contract version | `ACC-API-0.12` grup Accounting Event (`approved`, `GATE-DESAIN-0924`) — `GET /accounting-events`, `GET /accounting-events/summary`, `GET /accounting-events/{id}` |
| Wewenang UI | Isi layar dikunci kartu (tab Tercatat; baris Jurnal menyatakan "tidak dijurnal"). Teks persis dan letak tab `DEV_DISCRETION` — dipilih: tab di urutan terakhir sesudah Terjurnal; teks "Tidak dijurnal — pesan saldo subledger" |
| Dependency | `BE-ACC-P2-028` 🟡 — source ada di `rizkiG` (belum di-commit); frontend ini diuji bersama backend-nya atas keputusan Rizki 28 September 2026 |
| Klasifikasi | `LIGHT` — skor 3: berkas diperiksa 9–20 (1), memakai kontrak API yang ada (1), UI satu halaman terbatas (1); berkas diubah 2, logika sederhana, database/keamanan tidak ada |
| Task mode | `FRONTEND`; laporan dan tanda status ditulis ke repository backend (wewenang lintas repository yang sempit) |
| Target tulis | `QuilvianSystemFrontendDev` — constants dan hook Kotak Masuk kejadian; laporan ini, baris status roadmap frontend, dan traceability |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `95ea41cd9` (branch `RizkiV2`), perubahan belum di-commit |
| Commit backend yang dijadikan rujukan | `f06d487f` (branch `rizkiG`) + source `BE-ACC-P2-028` yang belum di-commit |
| Tanggal | 28 September 2026 |
| Status | **🟡 SEBAGIAN** — 28 September 2026. 5 dari 5 acceptance terpetakan ke source **dan terbukti di layar** — uji layar Rizki B1–B7 lulus bersama `BE-ACC-P2-028` (bagian 9). eslint 2 berkas **0 error, 0 warning**. **Satu-satunya yang belum:** hasil `npm run build` belum dilaporkan; begitu dilaporkan berhasil, task ini ✅ |

## 1. Keadaan yang ditemukan di awal

Layar Kotak Masuk (`FE-ACC-P2-011`) dan Rincian (`FE-ACC-P2-012`) sudah mengenal status `6`
"Tercatat" pada label dan warna badge, dan Isi Pesan Asli sudah menampilkan rincian
`SubledgerBalance` berlabel "Saldo Subledger — …". Dua celah tersisa begitu `BE-ACC-P2-028` mulai
menghasilkan kejadian `Tercatat`:

1. Tidak ada tab untuk status itu. Kejadian `Tercatat` hanya muncul di tab Semua, dan jumlahnya
   dari `/summary` tidak pernah terlihat.
2. Baris Jurnal di Rincian bertuliskan **"Belum ada jurnal"** — seolah jurnalnya akan menyusul,
   padahal pesan saldo **tidak pernah** dijurnal (`ACC-DEC-087`).

## 2. Proses bisnis dari sisi pengguna

1. Finance mengirim saldo akhir periode per akun kontrol (lewat akun layanan; di lingkungan
   pengembang lewat Swagger — hak `Receive` bukan milik petugas).
2. Petugas akuntansi membuka **Kotak Masuk Kejadian** dan melihat tab **Tercatat** beserta
   jumlahnya, misalnya "Tercatat 3".
3. Petugas memilih tab itu; daftar hanya berisi pesan saldo yang sudah dicatat.
4. Petugas membuka satu kejadian. Baris Jurnal bertuliskan **"Tidak dijurnal — pesan saldo
   subledger"**, tanpa tombol Buka Jurnal; tombol Coba Ulang dan Abaikan mati. Rincian periode dan
   akun kontrolnya terbaca di Isi Pesan Asli.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Untuk apa |
|---|---|
| `src/lib/constants/corporate/accounting/accounting-event/accounting-event-constants.jsx` | Daftar tab, label dan warna status, status yang boleh Coba Ulang/Abaikan, label pesan asli |
| `src/lib/hooks/corporate/accounting/accounting-event/use-accounting-event-inbox.jsx` | Cara tab dihitung dari `/summary` (`summaryKey` dan versi berhuruf besar) |
| `src/lib/hooks/corporate/accounting/accounting-event/use-accounting-event-detail.jsx` | `statusValue`, `summaryRows`, baris Jurnal, `payloadRows` |
| `src/components/view/corporate/accounting/accounting-event/detail/accounting-event-detail-view.jsx` | Tombol Buka Jurnal hanya dirender bila `journal` ada |
| Backend `AccountingEventDtos.cs` (read-only) | `AccountingEventSummaryDto.Tercatat`; `AccountingEventPagedQuery.EventStatus` bertipe enum nullable |
| Backend `AccAccountingEventService.GetPagedAsync` (read-only) | Penyaring `EventStatus` diterapkan apa adanya — nilai `6` diterima |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/corporate/accounting/accounting-event/accounting-event-constants.jsx` | `ACCOUNTING_EVENT_TABS` + `{ key: "6", label: "Tercatat", summaryKey: "tercatat" }`; konstanta baru `ACCOUNTING_EVENT_RECORDED_STATUS = "6"` dan `ACCOUNTING_EVENT_RECORDED_JOURNAL_LABEL` |
| `src/lib/hooks/corporate/accounting/accounting-event/use-accounting-event-detail.jsx` | Baris Jurnal: bila tidak ada jurnal **dan** status `6`, nilainya `ACCOUNTING_EVENT_RECORDED_JOURNAL_LABEL`; selain itu tetap "Belum ada jurnal". `statusValue` ditambahkan ke dependency `useMemo` |

Nol perubahan pada JSX, CSS, route, menu, sidebar bersama, Redux slice, dan service.

### 3.3 Kepatuhan arsitektur frontend

**UI GATE: 2 elemen — REUSE 2, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0**

| Kebutuhan UI | Kandidat base | Bukti | Status | Rekomendasi |
|---|---|---|---|---|
| Tab Tercatat di Kotak Masuk | Tab `DataFilter` yang sudah dirender dari `ACCOUNTING_EVENT_TABS` | `use-accounting-event-inbox.jsx` baris 148–166 | REUSE | Tambah satu entri konfigurasi; penghitung `/summary` dan penyaring status bekerja tanpa kode baru |
| Teks baris Jurnal untuk pesan saldo | Baris `summaryRows` di `BaseDetailCard` layar Rincian | `use-accounting-event-detail.jsx` `summaryRows`; `accounting-event-detail-view.jsx` baris 43 | REUSE | Ubah nilai baris saja; tombol Buka Jurnal otomatis tidak dirender karena `journal` kosong |

Karena seluruh elemen `REUSE`, tidak ada pilihan yang perlu diputuskan Rizki. Teks ditaruh di
constants domain, mengikuti pola label lain di berkas yang sama. Tanpa baris komentar `//`.

**Grep anti-regresi** (checklist G) — tidak ada berkas style atau JSX yang diubah, sehingga grep 1–6
tidak berlaku (`N/A`).

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat, kosong, gagal, tanpa hak akses | Tidak berubah — tetap perilaku `FE-ACC-P2-011`/`012`. Tab Tercatat yang kosong menampilkan kalimat penyaring kosong yang sudah ada: "Tidak ada kejadian yang cocok dengan penyaring." |
| Tab Tercatat berangka 0 | Tab tampil tanpa angka — sama dengan tab lain |

## 5. Endpoint yang dikonsumsi

#### Corporate - Accounting - Accounting Event

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/v1/corporate/accounting/accounting-events?eventStatus=6` | Daftar pada tab Tercatat | `AccountingEvent : Read` |
| `GET` | `/v1/corporate/accounting/accounting-events/summary` | Angka pada tab Tercatat (bidang `tercatat`) | `AccountingEvent : Read` |
| `GET` | `/v1/corporate/accounting/accounting-events/{id}` | Baris Jurnal pada Rincian | `AccountingEvent : Read` |

Nol endpoint baru; nol delta kontrak.

## 6. Verifikasi

### 6.1 Yang sudah dijalankan agent

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` pada 2 berkas yang berubah | 0 error, 0 warning (exit 0) | `PASS` | Keluaran perintah, 28 September 2026 |
| `npm run test:unit` | 1792 test: **1782 lulus, 10 gagal**. Kesepuluhnya berada di `accounting-reconciliation`, `accounting-trial-balance`, `inpatient-physician-entry`, `inpatient-physician-workspace`, `menu-permission-filter`, `petty-cash-finance-separation` — **tidak satu pun** berkas test itu mengimpor berkas yang diubah task ini (diperiksa dengan `grep`), dan belum ada test yang merujuk Kotak Masuk kejadian | `UNRELATED EXISTING ISSUE` | Keluaran perintah; bagian 8 |
| `npm run build` | Belum dijalankan — build oleh Rizki | `NOT RUN` | — |
| Uji layar (Rizki) | B1–B7 lulus, 28 September 2026 | `PASS` | Bagian 9 |
| `AUTOMATED TEST` | `SKIPPED (opsional)` — perubahan berupa konfigurasi tab dan satu nilai teks; tidak ada logika murni baru yang layak diuji unit | — | `test-policy.md` |

### 6.2 Skenario uji layar — dijalankan bersama `BE-ACC-P2-028`

Jalankan **sesudah** skenario 1–4 dan 11 pada laporan
[`BE-ACC-P2-028`](../backend/BE-ACC-P2-028.md) bagian 5.2.

| # | Langkah | Hasil yang diharapkan | Acceptance |
|---:|---|---|---|
| 1 | Buka Kotak Masuk Kejadian | Ada tab **Tercatat** berangka sesuai jumlah pesan saldo yang tercatat (contoh: 5 sesudah skenario 1–4 dan 11) | (1) |
| 2 | Pilih tab Tercatat | Hanya `EVT-UJI-128A`..`D` dan `128J` yang tampil, seluruhnya berbadge Tercatat | (2) |
| 3 | Buka `EVT-UJI-128A` | Baris Jurnal: "Tidak dijurnal — pesan saldo subledger", **tanpa** tombol Buka Jurnal; Isi Pesan Asli memuat "Saldo Subledger — Accounting Period Code: 2026-09" dan kode akun kontrolnya | (3) |
| 4 | Di Rincian yang sama | Tombol Coba Ulang dan Abaikan **mati** | (5) |
| 5 | Buka satu kejadian Terjurnal dan satu Tertahan lama | Baris Jurnal tetap nomor jurnal / "Belum ada jurnal" seperti sebelumnya | (4) |
| 6 | Tab Semua, Tertahan, Gagal, Terjurnal | Angka dan isinya tidak berubah dibanding sebelum task ini | (4) |

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Tab Tercatat tampil; angkanya dari `/summary`; angka 0 tidak ditampilkan | Terpenuhi di source | Entri `ACCOUNTING_EVENT_TABS` dengan `summaryKey: "tercatat"`; penghitung yang sudah ada membaca `tercatat`/`Tercatat` dan hanya menempel angka bila `> 0` |
| (2) Tab menyaring status Tercatat | Terpenuhi di source | `key: "6"` dipakai sebagai nilai penyaring `eventStatus`, sama dengan tab lain; backend menerima `6` |
| (3) Rincian Tercatat menyatakan tidak dijurnal | Terpenuhi di source | `summaryRows` baris `journal`; tombol Buka Jurnal hanya dirender bila `journal` ada |
| (4) Status lain tidak berubah | Terpenuhi di source | Cabang baru hanya aktif bila `journal` kosong **dan** `statusValue === "6"` |
| (5) Coba Ulang dan Abaikan tetap mati untuk Tercatat | Terpenuhi — tanpa perubahan | `ACCOUNTING_EVENT_RETRYABLE_STATUSES = ["2","3"]`, `ACCOUNTING_EVENT_IGNORABLE_STATUSES = ["3"]` |
| DoD: lint hijau | Terpenuhi | Bagian 6.1 |
| DoD: build owner berhasil | **Belum dilaporkan** | Hasil `npm run build` ditunggu dari Rizki |
| DoD: uji layar tercatat | Terpenuhi | Bagian 9 |
| DoD: laporan task tertulis | Terpenuhi | Berkas ini |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada |
| Masalah yang diketahui | **Temuan, di luar task ini:** 10 unit test gagal pada `RizkiV2` `95ea41cd9`. Dua berkas berada di wilayah Accounting — `accounting-reconciliation.test.mjs` ("route, menu, dan store terdaftar": pola `pathname: "/corporate/accounting/reconciliation"` tidak ditemukan) dan `accounting-trial-balance.test.mjs` (dua test layar Neraca Saldo; commit `95ea41cd9` "memperbaiki tampilan neraca saldo" mengubah layar itu). Delapan lainnya milik modul rawat inap, bank darah, dan petty cash. Tidak diperbaiki — di luar wewenang task ini |
| Dependency backend | `BE-ACC-P2-028` 🟡 — tanpa backend yang sudah di-build, tab Tercatat selalu kosong dan tidak ada kejadian untuk membuktikan acceptance (3) |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Frontend: `M src/lib/constants/corporate/accounting/accounting-event/accounting-event-constants.jsx`, `M src/lib/hooks/corporate/accounting/accounting-event/use-accounting-event-detail.jsx` |
| Langkah berikutnya | Rizki: build backend (`dotnet build -p:RunAnalyzers=false`) dan frontend (`npm run build`), jalankan skenario `BE-ACC-P2-028` bagian 5.2 lalu bagian 6.2 laporan ini; tempel hasil mentah — sesudah itu kedua task dinaikkan ke ✅ |

## 9. Hasil uji layar — Rizki, 28 September 2026

Bukti: tangkapan layar Kotak Masuk Kejadian, Rincian, dan Jenis Kejadian yang dikirim Rizki. Uji
dijalankan bersama skenario Swagger `BE-ACC-P2-028` (laporan backend bagian 5.3). Karena
`EVT-UJI-128A` tertahan (jenisnya didaftarkan sesudah pesan terkirim), tab Tercatat berisi 3
kejadian, bukan 4 seperti rencana.

| # | Yang diperiksa | Hasil di layar | Acceptance |
|---|---|---|---|
| B1 | Tab Kotak Masuk | Semua · **Tertahan 5** · Gagal · **Terjurnal 5** · **Tercatat 3** — tab Gagal tanpa angka karena nol | (1) ✅ |
| B2 | Tab Tercatat | Hanya `EVT-UJI-128D` (Rp 0), `128C` (−Rp 1.500.000), `128B` (Rp 425.000.000), seluruhnya berbadge Tercatat; kolom Jurnal "-" | (2) ✅ |
| B3 | Rincian `EVT-UJI-128B` | Status Tercatat; baris Jurnal **"Tidak dijurnal — pesan saldo subledger"**, tanpa tombol Buka Jurnal | (3) ✅ |
| B4 | Rincian `EVT-UJI-128C` | Nilai −Rp 1.500.000; Sumber "… versi 3"; Riwayat Percobaan 1 baris Berhasil | (3) ✅ |
| B5 | Rincian `EVT-UJI-128J` saat masih Tertahan | Baris Jurnal tetap "Belum ada jurnal"; alasan "Jenis kejadian belum terdaftar" | (4) ✅ |
| B6 | `EVT-UJI-128J` sesudah `UJI-SALDO-028B` didaftarkan → Coba Ulang | Tercatat; baris Jurnal "Tidak dijurnal — pesan saldo subledger"; Riwayat 3 baris | (3) ✅ |
| B7 | `EVT-UJI-128K` sesudah `UJI-SALDO-028C` didaftarkan → Coba Ulang, lalu Abaikan | Toast "Coba ulang kejadian EVT-UJI-128K belum berhasil. Lihat riwayat percobaan."; status Gagal membuka tombol Abaikan; Diabaikan beralasan "Membersihkan data uji 028" | (5): tombol aksi mengikuti status ✅ |

Acceptance (5) untuk status Tercatat — Coba Ulang dan Abaikan mati — **dinyatakan lulus Rizki**;
tangkapan layar tidak membedakan tampilan tombol mati dan hidup dengan jelas, dan source menjamin
perilakunya (`ACCOUNTING_EVENT_RETRYABLE_STATUSES = ["2","3"]`, `IGNORABLE = ["3"]`).
