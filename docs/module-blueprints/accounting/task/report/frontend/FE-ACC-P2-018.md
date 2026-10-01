# Laporan Perubahan Frontend — `FE-ACC-P2-018`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-ACC-P2-018` |
| Judul | Form Aturan Posting tidak menawarkan jenis Saldo Subledger |
| Slice | Wave D — pasangan `BE-ACC-P2-030` |
| Roadmap | [`roadmap/frontend-roadmap-phase2.md`](../../../roadmap/frontend-roadmap-phase2.md), kartu `FE-ACC-P2-018` (revisi 7, `APPROVED` Rizki 28 September 2026) |
| Trace | `ACC-DEC-096` butir 2 dan 3; perluasan `FR-P2-008` |
| Contract version | `ACC-API-0.14` — `EventTypeOptionResponse.EventKind` (angka); `422` dari `POST`/`PUT /posting-rules`; `PATCH /posting-rules/{id}/deactivate` tetap diterima |
| Wewenang UI | Cakupan dikunci kartu: saringan hanya pada form Aturan Posting. `DEV_DISCRETION` (kartu, Risiko): cara menampilkan jenis aturan lama — dipakai tampilan baca-saja yang sudah ada |
| Dependency | `BE-ACC-P2-030` 🟡 — berpasangan |
| Klasifikasi | `LIGHT` — skor 2: berkas diperiksa 9–20 (1), berkas diubah 3 (0), logika sederhana (1); kontrak dan UI baru 0 |
| Task mode | `FRONTEND` (Langkah D, berpasangan atas perintah Rizki 28 September 2026) |
| Target tulis | Hook editor Aturan Posting, utils Aturan Posting, satu keterangan isian di form; laporan ini dan baris status di repository backend |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `5adc9f316` (branch `RizkiV2`), belum di-commit |
| Commit backend yang dijadikan rujukan | `618b206e` + source `BE-ACC-P2-030` di working tree |
| Status | **✅ SELESAI — 29 September 2026.** 5 dari 5 acceptance terpetakan ke source; eslint 3 berkas 0/0. Build Rizki terbukti tak langsung: `.next/BUILD_ID` dan `.next/standalone/server.js` bertanggal 29 September 2026 10.17, sesudah berkas terakhir diubah (10.04). Uji layar L1, L3, L4, L5 dijalankan Rizki 29 September 2026 12.01–12.02 WIB lewat skrip Playwright, tangkapan layar diperiksa agent (bagian 6.3); L2 lewat API; L6 tidak berlaku (acceptance (4) dari source). UAT belum dijalankan — diserahkan ke tim UAT. Riwayat: 🟡 29 September 2026 |

## 1. Keadaan yang ditemukan di awal

`use-posting-rule-editor.jsx` memetakan seluruh butir `GET /event-types/options` menjadi pilihan
Jenis Kejadian, sehingga jenis Saldo Subledger seperti `UJI-SALDO-028` ikut ditawarkan. Pada mode Ubah,
jenis kejadian sudah tampil sebagai isian baca-saja dari rincian aturan, bukan dari daftar opsi. Pesan
`400`/`409`/`422` dari simpan juga sudah tampil apa adanya lewat `errorMessage` (`FE-ACC-P2-010`).

Opsi yang sama juga dibaca dua layar lain: penyaring daftar Aturan Posting (`use-posting-rule.jsx`) dan
penyaring Kotak Masuk Kejadian (`use-accounting-event-inbox.jsx`). Keduanya **tidak** disentuh.

## 2. Proses bisnis dari sisi pengguna

1. Petugas membuka Tambah Aturan Posting. Daftar Jenis Kejadian hanya berisi jenis aktif berperlakuan
   Transaksi, dan keterangan isiannya menjelaskan alasannya.
2. Aturan lama yang berjenis saldo tetap tampil di daftar dan rinciannya. Bila dibuka di form Ubah,
   jenisnya tampil apa adanya, dan simpan ditolak backend dengan pesan yang ditampilkan utuh.
3. Tombol Nonaktifkan tetap tersedia untuk aturan itu.
4. Di Kotak Masuk, penyaring jenis tetap memuat semua jenis aktif, sehingga pesan saldo tetap bisa dicari.

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Untuk apa |
|---|---|
| `src/lib/hooks/corporate/accounting/posting-rule/use-posting-rule-editor.jsx` | Pemetaan opsi jenis, tampilan baca-saja mode Ubah, penanganan galat simpan |
| `src/components/view/corporate/accounting/posting-rule/form/posting-rule-form-view.jsx` | Isian Jenis Kejadian mode Tambah dan Ubah |
| `src/lib/hooks/corporate/accounting/posting-rule/use-posting-rule.jsx` | Penyaring daftar Aturan Posting — tidak diubah |
| `src/lib/hooks/corporate/accounting/accounting-event/use-accounting-event-inbox.jsx` | Penyaring Kotak Masuk — tidak diubah (acceptance 2) |
| `src/lib/state/slice/corporate/accounting/accounting-event-type-slice.jsx` | Thunk `getEventTypeOptions` — bentuk data tidak berubah |
| `src/lib/constants/corporate/accounting/event-type/event-type-constants.jsx` | `EVENT_TYPE_KIND` (`FE-ACC-P2-013`); tanpa impor, aman dari impor melingkar |
| `src/utils/corporate/accounting/posting-rule/posting-rule-utils.jsx` | Tempat fungsi murni layar ini |

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/corporate/accounting/posting-rule/posting-rule-utils.jsx` | + `isPostingRuleEventTypeOption(item)` — `false` hanya bila `eventKind`/`EventKind` bernilai `2`. Butir tanpa bidang itu tetap ditawarkan, supaya backend yang belum di-build ulang tidak mengosongkan daftar; backend tetap pengamannya. +5/−0 |
| `src/lib/hooks/corporate/accounting/posting-rule/use-posting-rule-editor.jsx` | `eventTypeOptions` disaring `isPostingRuleEventTypeOption` sebelum dipetakan menjadi `{ value, label }`. +7/−4 |
| `src/components/view/corporate/accounting/posting-rule/form/posting-rule-form-view.jsx` | Keterangan isian Jenis Kejadian: "Hanya jenis aktif berperlakuan Transaksi; jenis Saldo Subledger tidak memerlukan aturan posting. Satu jenis hanya boleh punya satu aturan aktif per badan hukum." +1/−1 |

### 3.3 Kepatuhan arsitektur frontend

| Elemen | Keputusan | Dasar |
|---|---|---|
| Isian Jenis Kejadian mode Tambah | `REUSE` `renderSelect` yang ada | Hanya isi opsi yang berubah |
| Jenis aturan lama pada mode Ubah | `REUSE` `BaseTextField` baca-saja yang ada | `DEV_DISCRETION` kartu — tampilan yang ada sudah memenuhi acceptance (3) |
| Pesan `422` | `REUSE` `errorMessage` editor | Perilaku `FE-ACC-P2-010` |

`UI GATE`: REUSE 3, NEW 0. Nol CSS, nol state baru, nol panggilan HTTP baru. API tetap lewat thunk Redux.

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Opsi jenis memuat / gagal | Tidak berubah dari `FE-ACC-P2-010` |
| Semua jenis aktif berperlakuan saldo | Daftar Jenis Kejadian kosong; keterangan isian menjelaskan alasannya |
| Simpan ditolak `422` | Pesan backend tampil apa adanya |

## 5. Endpoint yang dikonsumsi

| Method | Path | Perubahan |
| --- | --- | --- |
| `GET` | `/v1/corporate/accounting/event-types/options` | Membaca `eventKind` |
| `POST` / `PUT` | `/v1/corporate/accounting/posting-rules[/{id}]` | Tidak berubah; `422` baru ditampilkan lewat jalur galat yang ada |

## 6. Verifikasi

### 6.1 Yang sudah dijalankan agent

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` tiga berkas di bagian 3.2 (bersama `menu-items.jsx` milik `FE-ACC-P2-017`) | Keluar `0`, nol pesan | `PASS` | Keluaran perintah |
| Pemeriksaan `isPostingRuleEventTypeOption` lewat `node --import ./tests/helpers/register.mjs` | Masukan `{eventKind:1}`, `{eventKind:2}`, `{EventKind:2}`, `{EventKind:"2"}`, `{}`, `{eventKind:null}` → `[true,false,false,false,true,true]` | `PASS` | Keluaran perintah |
| Grep `//` baru, `style={{`, warna literal | Nol | `PASS` | `git diff` |
| `npm run build` (Rizki) | `.next/BUILD_ID` 10.17 dan standalone 10.17, sesudah berkas task terakhir diubah 10.04 | `PASS` (tak langsung) | Tanggal berkas build |
| Uji layar L1, L3, L4, L5 (Rizki, skrip Playwright) | Sesuai harapan; L2 lewat API; L6 tidak berlaku | `PASS` | Bagian 6.3 |
| `AUTOMATED TEST` | `SKIPPED (opsional)` — belum ada berkas test Aturan Posting di `tests/unit`; skrip Playwright Rizki berada di luar suite repository | — | — |

### 6.2 Skenario uji layar untuk Rizki

Jalankan sesudah build backend `BE-ACC-P2-030` dan `npm run build` (atau `npm run dev`). Urutannya
berselang-seling dengan Swagger S1–S9 di laporan [`BE-ACC-P2-030`](../backend/BE-ACC-P2-030.md)
bagian 5.2: **S1–S5 → L1 → S6 (L2) → S7–S9 → L3–L6**.

| # | Langkah | Hasil yang diharapkan | Membuktikan |
|---:|---|---|---|
| L1 | Aturan Posting → **Tambah** → buka daftar Jenis Kejadian | `UJI-SALDO-028` **tidak** ada; jenis Transaksi aktif ada; keterangan isian baru tampil | (1) |
| L2 | Masih di Tambah: pilih `UJI-030-TRX`, isi satu baris debit dan satu kredit, lalu Simpan | Tersimpan — dipakai S7 | Persiapan |
| L3 | Sesudah S8: buka Tambah lagi | `UJI-030-TRX` kini **tidak** ditawarkan | (1) |
| L4 | Kotak Masuk Kejadian → penyaring Jenis | `UJI-SALDO-028` dan `UJI-030-TRX` **ada** | (2) |
| L5 | Daftar Aturan Posting → buka aturan `UJI-030-TRX` → Ubah → Simpan | Jenis tampil "UJI-030-TRX — Uji BE-ACC-P2-030" baca-saja; simpan menampilkan pesan `422` backend apa adanya | (3), (5) |
| L6 | *(bila S2 menemukan aturan lama berjenis saldo yang masih aktif, dan S5 dilewati)* Tekan **Nonaktifkan** pada aturan itu | Berhasil; status menjadi Nonaktif | (4) |

### 6.3 Hasil uji Rizki — 29 September 2026

Dijalankan lewat skrip `test-with-agy/test-p2-030-and-018.mjs` bersama uji API `BE-ACC-P2-030`
(laporan [`BE-ACC-P2-030`](../backend/BE-ACC-P2-030.md) bagian 5.3). Tangkapan layar diperiksa
langsung oleh agent, dicocokkan dengan `QuilvianSystemFrontendDev/test-with-agy/be_p2_030_fe_p2_018_test_report.json`.

| # | Yang terlihat | Klasifikasi |
| --- | --- | --- |
| L1 | `p2_l1_posting_rule_create_options.png` (12.01): dropdown Jenis Kejadian di Tambah Aturan Posting hanya berisi "PATIENT_PAYMENT — Pembayaran Pasien"; `UJI-SALDO-028` tidak ada. Keterangan isian tertutup dropdown pada gambar; JSON mencatat `hasDescriptionNotice: true` | `PASS` |
| L2 | Aturan `UJI-030-TRX` dibuat lewat API (`201`), bukan lewat form | Persiapan |
| L3 | `p2_l3_posting_rule_create_after_saldo.png` (12.02): sesudah S8, dropdown tetap hanya PATIENT_PAYMENT — `UJI-030-TRX` tidak ditawarkan | `PASS` |
| L4 | `p2_l4_accounting_events_filter.png` (12.02): penyaring jenis Kotak Masuk berisi Semua jenis, PATIENT_PAYMENT, UJI-030-TRX, UJI-SALDO-028 | `PASS` |
| L5 | `p2_l5_posting_rule_update_422.png` (12.02): Ubah Aturan Posting — Badan Hukum dan "UJI-030-TRX — Uji BE-ACC-P2-030" baca-saja; toast "Aturan Posting Ditolak" dengan pesan `422` backend utuh | `PASS` |
| L6 | Tidak dijalankan — tidak ada aturan lama berjenis saldo yang masih aktif (S2: 0) | `NOT APPLICABLE` — (4) dari source |

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Form Tambah tidak menawarkan jenis Saldo Subledger | Terpenuhi di source | `eventTypeOptions` disaring `isPostingRuleEventTypeOption` |
| (2) Penyaring Kotak Masuk tetap memuat seluruh jenis aktif | Terpenuhi — tidak diubah | `use-accounting-event-inbox.jsx` tanpa diff |
| (3) Aturan lama berjenis saldo terbaca; di Ubah jenis tampil apa adanya; `422` tampil apa adanya | Terpenuhi — perilaku yang ada | Mode Ubah memakai `readOnlyIdentity.eventType` dari rincian, bukan dari opsi; `errorMessage` editor |
| (4) Tombol Nonaktifkan tetap bekerja | Terpenuhi — tidak diubah | Aksi nonaktif `FE-ACC-P2-010` tanpa diff; backend `DeactivateAsync` tidak memeriksa jenis |
| (5) Saringan layar hanya kenyamanan; backend pengamannya | Terpenuhi | Butir tanpa `eventKind` tetap ditawarkan; penolakan tetap dari `BE-ACC-P2-030` |

| Butir DoD | Keadaan |
| --- | --- |
| Lint hijau | ✅ |
| Build owner berhasil | ✅ tak langsung — `.next` 29 September 2026 10.17 |
| Uji layar tercatat | ✅ 29 September 2026 — bagian 6.3 |
| Laporan task tertulis | ✅ |

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Status Git frontend | ` M` tiga berkas di bagian 3.2, ditambah `menu-items.jsx` milik perluasan `FE-ACC-P2-017` |
| Risiko | Penyaring daftar Aturan Posting (`use-posting-rule.jsx`) sengaja tidak disaring, supaya aturan lama berjenis saldo tetap dapat dicari lalu dinonaktifkan |
| Temuan di luar cakupan | — |
| Task berikutnya | `BE-ACC-P2-029` (backend saja) |
